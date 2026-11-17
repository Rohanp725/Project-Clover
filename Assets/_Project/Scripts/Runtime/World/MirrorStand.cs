using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A polished disc on a stand in the hall of columns (fire_rework.md, section 1). The sun falls on it and it throws
    /// a beam along the way it faces, until a wall or a pillar stops it. The player turns it a step at a time. The beam
    /// burns any enemy standing in it, lights any wick, counts as sunlit where it falls, and lights a
    /// <see cref="MirrorTarget"/> it reaches (a door that opens to the sun, a hidden way).
    /// </summary>
    public class MirrorStand : Interactable
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform beamQuad;
        [SerializeField] private Transform laneQuad;

        /// <summary>Degrees each turn moves the disc, and the seconds the turn takes.</summary>
        public float TurnStep = 45f;
        public float TurnSeconds = 0.4f;
        public float Reach = 22f;
        public float Width = 1f;
        public float Height = 1.4f;
        public float BurnDamage = 8f;
        public float BurnEvery = 0.5f;
        /// <summary>False while no sun falls on it: no beam.</summary>
        public bool InSun = true;

        public int Turns { get; private set; }
        public int EnemiesBurnt { get; private set; }
        public MirrorTarget Lighting { get; private set; }
        public Vector3 BeamEnd { get; private set; }

        /// <summary>Raised when it is turned.</summary>
        public event System.Action<MirrorStand> Turned;

        private static readonly RaycastHit[] hits = new RaycastHit[16];
        private SunBeamShape shape;
        private float yaw, fromYaw, turnLeft;
        private float untilBurn;

        public void Configure(Transform disc, Transform beam, Transform lane)
        {
            head = disc;
            beamQuad = beam;
            laneQuad = lane;
        }

        private void Awake()
        {
            shape = new SunBeamShape(beamQuad, laneQuad) { BeamColour = new Color(3.2f, 2.7f, 1.5f, 1f) };
            yaw = head != null ? head.eulerAngles.y : transform.eulerAngles.y;
            if (string.IsNullOrEmpty(Prompt) || Prompt == "Use") Prompt = "Turn the mirror";
        }

        public override void Use(PlayerInteraction who) => Turn(1);

        /// <summary>Turns the disc one step; a negative count turns it the other way.</summary>
        public void Turn(int steps)
        {
            fromYaw = head != null ? head.eulerAngles.y : yaw;
            yaw += TurnStep * steps;
            turnLeft = TurnSeconds;
            Turns++;
            Vfx.Play(VfxKind.Glint, (head != null ? head.position : transform.position) + Vector3.up * 0.3f, Vector3.up, 1.2f);
            Turned?.Invoke(this);
        }

        /// <summary>Faces the disc straight at a yaw in degrees, at once (a builder's starting turn, a probe).</summary>
        public void Face(float degrees)
        {
            yaw = degrees;
            turnLeft = 0f;
            if (head != null) head.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (head != null)
            {
                if (turnLeft > 0f)
                {
                    turnLeft -= deltaTime;
                    float t = 1f - Mathf.Clamp01(turnLeft / TurnSeconds);
                    head.rotation = Quaternion.Euler(0f, Mathf.LerpAngle(fromYaw, yaw, t * t * (3f - 2f * t)), 0f);
                }
                else head.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
            var lighting = (MirrorTarget)null;
            if (!InSun || turnLeft > 0f)
            {
                shape.Hide();
                Lighting = null;
                return;
            }
            Vector3 facing = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 from = (head != null ? head.position : transform.position) + Vector3.up * 0f;
            from.y = transform.position.y + Height;
            from += facing * 0.4f;
            Vector3 to = from + facing * Reach;
            // The nearest thing in the way that is not a body stops the beam; a target in the way is lit.
            int count = Physics.RaycastNonAlloc(from, facing, hits, Reach, ~0, QueryTriggerInteraction.Collide);
            float nearest = Reach;
            for (int i = 0; i < count; i++)
            {
                var collider = hits[i].collider;
                if (collider.transform.IsChildOf(transform)) continue;
                var target = collider.GetComponentInParent<MirrorTarget>();
                if (target == null && (collider.isTrigger || collider is CharacterController)) continue;
                if (hits[i].distance >= nearest) continue;
                nearest = hits[i].distance;
                lighting = target;
            }
            to = from + facing * nearest;
            BeamEnd = to;
            Lighting = lighting;
            if (lighting != null) lighting.Light(deltaTime);
            float flicker = 0.85f + 0.15f * Mathf.PerlinNoise(Time.time * 5f, 0.7f);
            shape.Show(from, to, Width, flicker, 0.8f);

            untilBurn -= deltaTime;
            if (untilBurn > 0f) return;
            untilBurn = BurnEvery;
            Sunlight.Beam(from, to, Width, BurnEvery + 0.1f);
            // Everything with a body standing in the beam is burnt; wicks catch.
            count = Physics.SphereCastNonAlloc(from - facing * 0.2f, Width * 0.5f, facing, hits, nearest, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var enemy = hits[i].collider.GetComponentInParent<Enemy>();
                if (enemy == null || enemy.IsDead) continue;
                EnemiesBurnt++;
                enemy.TakeHit(new Hit(BurnDamage, 6f, facing, false, true, from, 2f, 1));
                var flame = enemy.GetComponent<WickFlame>();
                if (flame != null) flame.Light();
                Vfx.Play(VfxKind.Fire, enemy.transform.position + Vector3.up * 1.2f, Vector3.up, 0.5f);
            }
        }
    }
}
