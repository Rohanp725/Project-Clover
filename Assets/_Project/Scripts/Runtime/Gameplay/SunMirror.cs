using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The sun-blind priest's mirror (fire_rework.md, section 1). His beam attack throws a streak of sunlight down a
    /// lane from the disc on his breast; it burns as it passes (the attack's own blow) and the lane counts as sunlit
    /// for a while after, so it can drive the player out of shade. And the calling: a priest that has found the player
    /// in the sun raises the disc and lights them up, and the gilded near him turn to the light and come.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class SunMirror : MonoBehaviour
    {
        [SerializeField] private Transform beamQuad;
        [SerializeField] private Transform laneQuad;
        [SerializeField] private Enemy[] neighbours = new Enemy[0];

        public float DiscHeight = 1.35f;
        public float CallRange = 14f;
        public float CallEvery = 7f;
        public float FlashSeconds = 0.35f;

        public int Beams { get; private set; }
        public int Calls { get; private set; }
        public int Called { get; private set; }

        private Enemy enemy;
        private SunBeamShape shape;
        private Vector3 beamFrom, beamTo;
        private float beamWidth, beamShown, laneLeft, laneSeconds;
        private float untilCall, untilGlint;
        private bool hadFound;

        public void Configure(Transform beam, Transform lane, Enemy[] gilded)
        {
            beamQuad = beam;
            laneQuad = lane;
            neighbours = gilded ?? new Enemy[0];
        }

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            shape = new SunBeamShape(beamQuad, laneQuad);
        }

        private void OnEnable() => enemy.AttackStruck += OnAttackStruck;
        private void OnDisable() => enemy.AttackStruck -= OnAttackStruck;

        private void OnAttackStruck(Enemy who, EnemyAttack attack)
        {
            if (!attack.Beams) return;
            Beams++;
            beamFrom = transform.position + Vector3.up * DiscHeight + transform.forward * 0.4f;
            beamTo = beamFrom + transform.forward * attack.Reach;
            // Stopped by a wall in the way, as light is.
            if (Physics.Raycast(beamFrom, transform.forward, out RaycastHit wall, attack.Reach, ~0, QueryTriggerInteraction.Ignore) && wall.collider.GetComponentInParent<Clover.Player.PlayerDefence>() == null)
                beamTo = wall.point;
            beamWidth = attack.LineWidth;
            beamShown = FlashSeconds;
            laneSeconds = Mathf.Max(0.1f, attack.LightsSeconds);
            laneLeft = laneSeconds;
            Clover.World.Sunlight.Beam(beamFrom, beamTo, attack.LineWidth, attack.LightsSeconds);
            Vfx.Play(VfxKind.Burst, beamFrom, transform.forward, 0.8f);
            for (int i = 1; i <= 6; i++)
                Vfx.Play(VfxKind.TintSpark, Vector3.Lerp(beamFrom, beamTo, i / 6f), transform.forward, 0.3f, new Color(1f, 0.95f, 0.7f, 1f));
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (laneLeft > 0f || beamShown > 0f)
            {
                beamShown -= deltaTime;
                laneLeft -= deltaTime;
                float flash = Mathf.Clamp01(beamShown / FlashSeconds);
                float lit = Mathf.Clamp01(laneLeft / laneSeconds);
                shape.Show(beamFrom, beamTo, beamWidth, flash, Mathf.Min(1f, lit * 1.5f) * 0.8f + flash * 0.2f);
                if (laneLeft <= 0f && beamShown <= 0f) shape.Hide();
            }
            if (enemy.IsDead) return;
            // The disc catches the sun as he turns it toward someone: a glint at his breast through the beam's telegraph.
            var attack = enemy.Brain.Current;
            if (attack != null && attack.Beams && enemy.Brain.State == EnemyState.Telegraph)
            {
                untilGlint -= deltaTime;
                if (untilGlint <= 0f)
                {
                    untilGlint = 0.12f;
                    Vfx.Play(VfxKind.Glint, transform.position + Vector3.up * DiscHeight + transform.forward * 0.4f, transform.forward, 0.6f + enemy.Brain.TelegraphProgress);
                }
            }
            // The calling: the moment he finds the player in the sun, and again while he keeps them, every few seconds.
            untilCall -= deltaTime;
            bool found = enemy.Sense.FoundLit && enemy.InFight;
            if (found && (!hadFound || untilCall <= 0f)) Call();
            hadFound = found;
        }

        /// <summary>Raises the disc: the player is lit up where they stand, and the gilded near him turn to the light and come.</summary>
        public void Call()
        {
            untilCall = CallEvery;
            Calls++;
            var target = enemy.Target;
            Vector3 disc = transform.position + Vector3.up * (DiscHeight + 0.6f);
            Vfx.Play(VfxKind.TintRing, disc, Vector3.up, 1.2f, new Color(1f, 0.9f, 0.55f, 1f));
            Vfx.Play(VfxKind.Glint, disc, Vector3.up, 2f);
            if (target != null)
            {
                Clover.World.Sunlight.Flare(target.position, 1.6f, 2.5f);
                Vfx.Play(VfxKind.TintRing, target.position + Vector3.up * 0.2f, Vector3.up, 0.7f, new Color(1f, 0.9f, 0.55f, 1f));
            }
            foreach (var other in neighbours)
            {
                if (other == null || other.IsDead || other == enemy) continue;
                if ((other.transform.position - transform.position).sqrMagnitude > CallRange * CallRange) continue;
                if (!other.IsAware) Called++;
                other.WakeNow();
            }
        }
    }
}
