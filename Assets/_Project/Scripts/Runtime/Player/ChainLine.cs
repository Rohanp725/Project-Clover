using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The Chain Sickle's chain and what hangs on the end of it. The chain changes length, so it is no part of the
    /// model: a line of points drawn each frame from the left hand to the weight, sagging when slack and straight
    /// when taut. Where the weight is comes from what the bearer is doing: turning in a small circle beside the leg
    /// at rest, in a ring overhead while the heavy is held, down the lane and back for a throw, twice round the
    /// bearer for the whirl, on an enemy for as long as it is bound, and taut between the two hands behind the guard.
    /// For the thrown sickle it is the sickle itself that goes out, and the weight stays in the fist.
    /// </summary>
    public class ChainLine : MonoBehaviour
    {
        [SerializeField] private GameObject weightPrefab, sicklePrefab;
        [SerializeField] private Material material;

        /// <summary>The carry name of the weapon this belongs to.</summary>
        public string Carry = "sickle";
        public int Points = 24;
        public float Width = 0.075f;

        private PlayerCombat combat;
        private PlayerDefence defence;
        private Transform handLeft, handRight, weight, sickle;
        private GameObject inHand;
        private LineRenderer line;
        private bool shownNow, sickleOut;
        private float turn;

        /// <summary>For probes: how far the end of the chain is from the hand now, and the furthest it has been.</summary>
        public float Length { get; private set; }
        public float Furthest { get; set; }

        /// <summary>The weight on the end of the chain and the sickle when it is thrown, as they are in the world (null until made).</summary>
        public Transform WeightProp => weight;
        public Transform SickleProp => sickle;

        public void Configure(GameObject weightModel, GameObject sickleModel, Material chainMaterial)
        {
            weightPrefab = weightModel;
            sicklePrefab = sickleModel;
            material = chainMaterial;
        }

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            handLeft = WeaponRack.FindSocket(transform, WeaponRack.HandLeft);
            handRight = WeaponRack.FindSocket(transform, WeaponRack.HandRight);
            var held = handRight != null ? handRight.Find(Carry + "_hand") : null;
            inHand = held != null ? held.gameObject : null;
            weight = Prop(weightPrefab, "Chain Weight");
            sickle = Prop(sicklePrefab, "Thrown Sickle");
            var drawn = new GameObject("Chain Line");
            line = drawn.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.positionCount = Points;
            line.useWorldSpace = true;
            line.startWidth = line.endWidth = Width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
        }

        private static Transform Prop(GameObject prefab, string name)
        {
            if (prefab == null) return null;
            var prop = Instantiate(prefab);
            prop.name = name;
            foreach (var collider in prop.GetComponentsInChildren<Collider>()) Destroy(collider);
            prop.SetActive(false);
            return prop.transform;
        }

        private void OnDestroy()
        {
            if (weight != null) Destroy(weight.gameObject);
            if (sickle != null) Destroy(sickle.gameObject);
            if (line != null) Destroy(line.gameObject);
        }

        private void Hide()
        {
            shownNow = false;
            sickleOut = false;
            line.enabled = false;
            if (weight != null) weight.gameObject.SetActive(false);
            if (sickle != null) sickle.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            bool mine = combat != null && combat.Weapon != null && combat.Weapon.Carry == Carry && handLeft != null && !(defence != null && defence.IsDead);
            if (!mine)
            {
                if (shownNow) Hide();
                return;
            }
            if (!shownNow)
            {
                shownNow = true;
                line.enabled = true;
                if (weight != null) weight.gameObject.SetActive(true);
            }
            float deltaTime = Time.deltaTime;
            Vector3 hand = handLeft.position, origin = transform.position, forward = transform.forward;
            Vector3 far;
            float sag;
            bool thrownSickle = false;
            var step = combat.Combo.Current;
            float at = combat.Combo.Elapsed;
            // What the chain is wound on: the enemy it binds, or the one a finisher has wrapped (from its first beat to its blow).
            Transform bound = combat.BoundTarget != null ? combat.BoundTarget.transform : null;
            if (bound == null && step != null && step.Beats != null && step.Beats.Length > 0 && at >= step.Beats[0].Time && combat.Combo.Phase == AttackPhase.Windup) bound = combat.FinisherTarget;
            if (step != null && step.Chain == ChainThrow.Sweep)
            {
                // The sickle on the end of the chain: out to the right, flat across the front, and hauled home.
                float hit = step.Windup, done = step.Windup + step.Active;
                float outward = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(hit * 0.45f, hit * 0.8f, at));
                float back = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(done, done + step.Recovery * 0.6f, at));
                float across = Mathf.InverseLerp(hit * 0.8f, done, at);
                float half = step.ArcDegrees * 0.5f;
                Vector3 end = origin + Quaternion.AngleAxis(Mathf.Lerp(half, -half, across), Vector3.up) * forward * step.Reach + Vector3.up;
                far = Vector3.Lerp(hand, end, outward * (1f - back));
                thrownSickle = outward > 0.04f && back < 0.96f;
                sag = 0.02f;
            }
            else if (bound != null)
            {
                // Round the bound enemy's chest, for as long as it is held.
                turn += deltaTime * 540f;
                far = bound.position + Vector3.up * 1.1f + Quaternion.AngleAxis(turn, Vector3.up) * Vector3.forward * 0.35f;
                sag = 0f;
            }
            else if (step != null && (step.Chain == ChainThrow.Lane || (step.Beats != null && step.Beats.Length > 0 && combat.Combo.Phase == AttackPhase.Windup)))
            {
                float hit = step.Windup, done = step.Windup + step.Active;
                float outward = Mathf.InverseLerp(hit * 0.5f, hit, at);
                float back = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(done, done + step.Recovery * 0.7f, at));
                far = Vector3.Lerp(hand, origin + forward * step.Reach + Vector3.up * 1.05f, outward * outward * (1f - back));
                // Whipped back, the chain cracks into a wave.
                sag = back > 0f && back < 1f ? 0.25f * Mathf.Sin(back * Mathf.PI) : 0f;
            }
            else if (step != null && step.Chain == ChainThrow.Whirl)
            {
                // Twice round at full length, the way the cut is said to travel (right to left across the front).
                float hit = step.Windup, done = step.Windup + step.Active;
                float grow = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(hit * 0.3f, hit * 0.9f, at)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(done + step.Recovery * 0.2f, done + step.Recovery * 0.75f, at)));
                float round = Mathf.InverseLerp(hit * 0.5f, done + step.Recovery * 0.3f, at) * 720f + Mathf.InverseLerp(done + step.Recovery * 0.3f, step.Total, at) * 360f;
                Vector3 end = origin + Quaternion.AngleAxis(90f - round * (step.Sweep < 0f ? -1f : 1f), Vector3.up) * forward * step.Reach + Vector3.up * 1.5f;
                far = Vector3.Lerp(hand, end, grow);
                sag = 0f;
            }
            else if (defence != null && defence.IsGuarding && handRight != null)
            {
                // Snapped taut between the two hands.
                far = handRight.position;
                sag = 0f;
            }
            else if (combat.IsCharging)
            {
                // Let out and swung overhead, faster as the hold fills.
                turn += deltaTime * (360f + 360f * combat.Combo.ChargeProgress);
                far = origin + Vector3.up * 2.1f + Quaternion.AngleAxis(-turn, Vector3.up) * forward * 1.5f;
                sag = 0.02f;
            }
            else
            {
                // At rest and in the sickle's own cuts: an upright circle beside the left leg, one turn a loop, never stopping.
                turn += deltaTime * 225f;
                far = hand - Vector3.up * 0.34f + Quaternion.AngleAxis(turn, transform.right) * Vector3.down * 0.26f;
                sag = 0.03f;
            }

            if (thrownSickle != sickleOut)
            {
                sickleOut = thrownSickle;
                if (inHand != null) inHand.SetActive(!thrownSickle);
                if (sickle != null) sickle.gameObject.SetActive(thrownSickle);
            }
            Vector3 span = far - hand;
            Length = span.magnitude;
            if (Length > Furthest) Furthest = Length;
            Quaternion facing = Length > 0.01f ? Quaternion.LookRotation(span / Length, Vector3.up) : transform.rotation;
            if (thrownSickle && sickle != null) sickle.SetPositionAndRotation(far, facing * Quaternion.Euler(0f, 0f, 90f));
            if (weight != null) weight.SetPositionAndRotation(thrownSickle ? hand : far, facing);
            for (int i = 0; i < Points; i++)
            {
                float along = i / (float)(Points - 1);
                Vector3 point = Vector3.Lerp(hand, far, along);
                point.y -= sag * 4f * along * (1f - along) * (sag > 0.2f ? Mathf.Sin(along * Mathf.PI * 3f) : 1f);
                line.SetPosition(i, point);
            }
        }
    }
}
