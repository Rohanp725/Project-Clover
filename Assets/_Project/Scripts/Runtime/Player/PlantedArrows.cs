using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The Great Bow's arrows once they have landed. One that struck an enemy stands in its body (three at most, the
    /// oldest making way) until a blow of the stave snaps it: each arrow snapped is a blow of its own, a moment after
    /// the last. One that struck nothing stands in the ground and goes off under the first enemy to come near.
    /// Landmines drops a ring of them from the sky. Every arrow is one of a fixed pool of models: nothing is made in play.
    /// </summary>
    public class PlantedArrows : MonoBehaviour
    {
        [SerializeField] private GameObject arrowPrefab;

        public int Capacity = 28;
        /// <summary>Arrows one body carries, and seconds each stays.</summary>
        public int MostInOne = 3;
        public float InBodySeconds = 12f;
        /// <summary>Arrows standing in the ground at once, and seconds each stays.</summary>
        public int MostInGround = 6;
        public float GroundSeconds = 8f;
        /// <summary>What each arrow deals when it is snapped or set off.</summary>
        public float SnapDamage = 10f;
        public float SnapStagger = 14f;
        /// <summary>How near an enemy sets off an arrow in the ground, and how wide it then bursts.</summary>
        public float TripRadius = 1.1f;
        public float BurstRadius = 1.6f;

        private struct Arrow
        {
            public bool Live, Ground, Snapping;
            public Component Host;
            public Vector3 Local;
            public Quaternion LocalTurn;
            public float Left, SnapIn, ArmIn;
        }

        private struct Falling
        {
            public bool Live;
            public Vector3 At;
            public float In, Damage, Stagger, Radius, Seconds;
        }

        private static readonly int BaseColour = Shader.PropertyToID("_BaseColor");

        private Arrow[] arrows;
        private Transform[] shown;
        // Each arrow's fletching: the parts that take the held element's colour as it lands.
        private Renderer[][] vanes;
        private MaterialPropertyBlock block;
        private readonly Falling[] falling = new Falling[8];
        private PlayerCombat combat;
        private PlayerElement element;
        private PlayerDefence defence;

        /// <summary>For probes: arrows planted in bodies, planted in the ground, snapped, set off by an enemy's step, and dropped by Landmines.</summary>
        public int Planted { get; private set; }
        public int Grounded { get; private set; }
        public int Snapped { get; private set; }
        public int Tripped { get; private set; }
        public int Rained { get; private set; }
        /// <summary>How many stand in the ground now.</summary>
        public int Standing { get; private set; }

        public void Configure(GameObject arrow) => arrowPrefab = arrow;

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            element = GetComponent<PlayerElement>();
            defence = GetComponent<PlayerDefence>();
            arrows = new Arrow[Capacity];
            shown = new Transform[Capacity];
            vanes = new Renderer[Capacity][];
            block = new MaterialPropertyBlock();
            var fletching = new System.Collections.Generic.List<Renderer>();
            if (arrowPrefab == null) return;
            for (int i = 0; i < Capacity; i++)
            {
                var arrow = Instantiate(arrowPrefab);
                arrow.name = "Planted Arrow";
                foreach (var collider in arrow.GetComponentsInChildren<Collider>()) Destroy(collider);
                arrow.SetActive(false);
                shown[i] = arrow.transform;
                fletching.Clear();
                foreach (var part in arrow.GetComponentsInChildren<Renderer>(true))
                    if (part.sharedMaterial != null && part.sharedMaterial.name.EndsWith("_accent")) fletching.Add(part);
                vanes[i] = fletching.ToArray();
            }
        }

        private void OnDestroy()
        {
            if (shown == null) return;
            foreach (var arrow in shown)
                if (arrow != null) Destroy(arrow.gameObject);
        }

        /// <summary>How many arrows stand in this body, not counting ones already snapping.</summary>
        public int CountIn(Component host)
        {
            int count = 0;
            if (arrows == null || host == null) return 0;
            for (int i = 0; i < arrows.Length; i++)
                if (arrows[i].Live && !arrows[i].Ground && !arrows[i].Snapping && arrows[i].Host == host) count++;
            return count;
        }

        /// <summary>Leaves an arrow standing in a body, driven in along a direction. A fourth takes the place of the oldest.</summary>
        public void Plant(Component host, Vector3 direction)
        {
            if (arrows == null || host == null) return;
            int oldest = -1, count = 0;
            for (int i = 0; i < arrows.Length; i++)
            {
                if (!arrows[i].Live || arrows[i].Ground || arrows[i].Snapping || arrows[i].Host != host) continue;
                count++;
                if (oldest < 0 || arrows[i].Left < arrows[oldest].Left) oldest = i;
            }
            if (count >= MostInOne && oldest >= 0) End(oldest);
            int slot = Free();
            if (slot < 0) return;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : host.transform.forward;
            // No two stand quite alike: each is turned a little off the line it came in on, and sits at its own height.
            float yaw = (Planted * 47) % 50 - 25f;
            Vector3 along = (Quaternion.AngleAxis(yaw, Vector3.up) * direction - Vector3.up * (0.18f + 0.06f * (Planted % 3))).normalized;
            Vector3 point = host.transform.position + Vector3.up * (1.05f + 0.12f * (Planted % 3)) + along * 0.1f;
            Quaternion turn = Quaternion.LookRotation(along, Vector3.up);
            arrows[slot] = new Arrow
            {
                Live = true, Host = host, Left = InBodySeconds,
                Local = host.transform.InverseTransformPoint(point - along * 0.85f),
                LocalTurn = Quaternion.Inverse(host.transform.rotation) * turn,
            };
            Show(slot, point - along * 0.85f, turn);
            Planted++;
        }

        /// <summary>Leaves an arrow standing in the ground at a point, slanted the way it flew.</summary>
        public void PlantGround(Vector3 at, Vector3 direction, float seconds = 0f)
        {
            if (arrows == null) return;
            int oldest = -1, count = 0;
            for (int i = 0; i < arrows.Length; i++)
            {
                if (!arrows[i].Live || !arrows[i].Ground) continue;
                count++;
                if (oldest < 0 || arrows[i].Left < arrows[oldest].Left) oldest = i;
            }
            if (count >= MostInGround && oldest >= 0) End(oldest);
            int slot = Free();
            if (slot < 0) return;
            direction.y = 0f;
            Vector3 along = (Vector3.down + (direction.sqrMagnitude > 0.0001f ? direction.normalized * 0.45f : Vector3.zero)).normalized;
            Quaternion turn = Quaternion.LookRotation(along, Vector3.forward);
            Vector3 nock = at - Vector3.up * 0.12f - along * 1.1f;
            arrows[slot] = new Arrow { Live = true, Ground = true, Left = seconds > 0f ? seconds : GroundSeconds, Local = at, LocalTurn = turn, ArmIn = 0.3f };
            Show(slot, nock, turn);
            Grounded++;
        }

        /// <summary>
        /// Snaps every arrow standing in a body, one after another this far apart: each is a blow of its own on top of
        /// the one that snapped them, and lays the held element. Returns how many there were.
        /// </summary>
        public int Snap(Component host, float every = 0.05f)
        {
            if (arrows == null || host == null) return 0;
            int count = 0;
            for (int i = 0; i < arrows.Length; i++)
            {
                if (!arrows[i].Live || arrows[i].Ground || arrows[i].Snapping || arrows[i].Host != host) continue;
                arrows[i].Snapping = true;
                arrows[i].SnapIn = every * (count + 1);
                count++;
            }
            if (count >= 2 && defence != null) defence.Callout(count == 2 ? "Snap x2" : "Snap x3");
            return count;
        }

        /// <summary>
        /// Landmines: this many arrows come down in a ring round a point after a delay, each striking where it lands
        /// and then standing there as an arrow in the ground.
        /// </summary>
        public void Rain(Vector3 centre, Vector3 forward, int count, float ringRadius, float delay, float damage, float stagger, float radius, float seconds)
        {
            Color colour = combat != null ? combat.EffectColour : Color.white;
            for (int n = 0; n < count; n++)
            {
                int slot = -1;
                for (int i = 0; i < falling.Length; i++)
                {
                    if (falling[i].Live) continue;
                    slot = i;
                    break;
                }
                if (slot < 0) return;
                Vector3 at = centre + Quaternion.AngleAxis(n * 360f / count + 36f, Vector3.up) * forward * ringRadius;
                falling[slot] = new Falling { Live = true, At = at, In = delay + 0.03f * n, Damage = damage, Stagger = stagger, Radius = radius, Seconds = seconds };
                // Where each will land is shown on the ground while they are in the air.
                Vfx.Play(VfxKind.TintRing, at + Vector3.up * 0.12f, Vector3.up, radius * 0.7f, colour);
                Rained++;
            }
        }

        private SpellBlow Blow(float damage, float stagger, bool laysElement)
        {
            var held = laysElement && element != null ? element.Info : null;
            return new SpellBlow
            {
                Damage = damage, Stagger = stagger, Effect = VfxKind.TintSpark, Tinted = true, Tint = combat != null ? combat.EffectColour : Color.white,
                Affliction = held != null ? held.Affliction : AfflictionKind.None, Stacks = held != null && held.Affliction != AfflictionKind.None ? 1 : 0,
            };
        }

        private void Update()
        {
            if (arrows == null || combat == null) return;
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < falling.Length; i++)
            {
                if (!falling[i].Live) continue;
                falling[i].In -= deltaTime;
                if (falling[i].In > 0f) continue;
                falling[i].Live = false;
                var fell = falling[i];
                combat.StrikeArea(fell.At, fell.Radius, Blow(fell.Damage, fell.Stagger, false));
                Vfx.Play(VfxKind.Dust, fell.At + Vector3.up * 0.1f, Vector3.up, 1.2f);
                PlantGround(fell.At, Vector3.zero, fell.Seconds);
            }
            int standing = 0;
            for (int i = 0; i < arrows.Length; i++)
            {
                if (!arrows[i].Live) continue;
                ref Arrow arrow = ref arrows[i];
                if (!arrow.Ground)
                {
                    if (arrow.Host == null || !arrow.Host.gameObject.activeInHierarchy)
                    {
                        End(i);
                        continue;
                    }
                    var body = arrow.Host.transform;
                    if (shown[i] != null) shown[i].SetPositionAndRotation(body.TransformPoint(arrow.Local), body.rotation * arrow.LocalTurn);
                    if (arrow.Snapping)
                    {
                        arrow.SnapIn -= deltaTime;
                        if (arrow.SnapIn > 0f) continue;
                        var host = arrow.Host;
                        Vector3 at = body.position + Vector3.up * 1.2f;
                        End(i);
                        Snapped++;
                        combat.StrikeOne(host, Blow(SnapDamage, SnapStagger, true), transform.position);
                        Vfx.Play(VfxKind.TintSpark, at, Vector3.up, 1.3f, Color.white);
                        continue;
                    }
                }
                arrow.Left -= deltaTime;
                if (arrow.Left <= 0f)
                {
                    End(i);
                    continue;
                }
                if (!arrow.Ground) continue;
                standing++;
                arrow.ArmIn -= deltaTime;
                if (arrow.ArmIn > 0f) continue;
                var near = combat.EnemyNear(arrow.Local, TripRadius + 0.7f, null);
                if (near == null) continue;
                Vector3 gap = near.transform.position - arrow.Local;
                gap.y = 0f;
                if (gap.magnitude > TripRadius) continue;
                Vector3 spot = arrow.Local;
                End(i);
                standing--;
                Tripped++;
                combat.StrikeArea(spot, BurstRadius, Blow(SnapDamage, SnapStagger, true));
                Vfx.Play(VfxKind.Dust, spot + Vector3.up * 0.1f, Vector3.up, 1.6f);
                Vfx.Play(VfxKind.TintRing, spot + Vector3.up * 0.12f, Vector3.up, BurstRadius, combat.EffectColour);
            }
            Standing = standing;
        }

        private int Free()
        {
            for (int i = 0; i < arrows.Length; i++)
                if (!arrows[i].Live) return i;
            return -1;
        }

        private void Show(int i, Vector3 at, Quaternion turn)
        {
            if (shown[i] == null) return;
            if (element != null && element.Info != null && combat != null)
            {
                // The fletching is the colour of what the arrow will lay when it snaps.
                block.SetColor(BaseColour, combat.EffectColour);
                foreach (var vane in vanes[i]) vane.SetPropertyBlock(block);
            }
            shown[i].SetPositionAndRotation(at, turn);
            shown[i].gameObject.SetActive(true);
        }

        private void End(int i)
        {
            arrows[i].Live = false;
            arrows[i].Host = null;
            if (shown[i] != null) shown[i].gameObject.SetActive(false);
        }
    }
}
