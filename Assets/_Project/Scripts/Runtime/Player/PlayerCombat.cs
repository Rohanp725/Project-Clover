using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// The player's weapon: reads the attack buttons, runs the attacks, turns the player toward the nearest enemy when an
    /// attack starts, and deals the blow when it lands. The rules are in <see cref="WeaponCombo"/>. It has no Update of
    /// its own: <see cref="PlayerController"/> calls <see cref="Advance"/> each frame after it has decided about the
    /// dodge, so a dodge and an attack pressed together are always settled in the same order.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private HitStop hitStop;
        [SerializeField] private TopDownCamera shaker;

        public WeaponCombo Combo = Katana();
        public ManaPool Mana = new ManaPool();
        /// <summary>Metres within which an attack turns to face an enemy.</summary>
        public float AimRange = 4.5f;
        /// <summary>Full width in degrees, around the stick's direction, in which an enemy is picked to face.</summary>
        public float AimConeDegrees = 140f;
        /// <summary>Seconds time nearly stops for a finisher, whatever the attack's own value.</summary>
        public float FinisherHitStop = 0.16f;
        public float FinisherShake = 0.4f;
        public LayerMask HitLayers = ~0;

        public bool IsAttacking => Combo.IsAttacking;
        public bool IsCharging => Combo.IsCharging;
        public bool CanBeCancelled => Combo.CanBeCancelled;
        /// <summary>How far the attack moves the player this frame; zero outside a wind-up.</summary>
        public Vector3 Step { get; private set; }
        /// <summary>Counts every blow that struck something, so tests and probes can see hits without watching the screen.</summary>
        public int BlowsLanded { get; private set; }
        public int Finishers { get; private set; }
        public int SkillsUsed { get; private set; }

        private PlayerController player;
        private PlayerDefence defence;
        private InputAction lightAction;
        private InputAction heavyAction;
        private InputAction moveAction;
        private readonly Collider[] overlaps = new Collider[64];
        private bool warnedOverlapsFull;
        private readonly HashSet<IDamageable> struck = new HashSet<IDamageable>();

        public void Configure(HitStop hitStopToUse, TopDownCamera cameraToShake)
        {
            hitStop = hitStopToUse;
            shaker = cameraToShake;
        }

        /// <summary>
        /// The Katana, first version. Light chain: a drawing cut, a return cut, a stepping finisher. Heavy: an overhead
        /// cut, quicker and harder when it ends a chain. Skill: the Iaido Slash, a long drawing cut released from the sheath.
        /// </summary>
        public static WeaponCombo Katana() => new WeaponCombo
        {
            Steps = new List<AttackStep>
            {
                new AttackStep { Name = "attack_light_1", Windup = 0.10f, Active = 0.08f, Recovery = 0.24f, Damage = 10f, Stagger = 10f, StaminaCost = 10f, ManaGain = 5f, Lunge = 0.7f, Reach = 2.4f, ArcDegrees = 140f, HitStopSeconds = 0.045f },
                new AttackStep { Name = "attack_light_2", Windup = 0.11f, Active = 0.08f, Recovery = 0.25f, Damage = 11f, Stagger = 10f, StaminaCost = 10f, ManaGain = 5f, Lunge = 0.6f, Reach = 2.4f, ArcDegrees = 140f, HitStopSeconds = 0.045f },
                new AttackStep { Name = "attack_light_3", Windup = 0.16f, Active = 0.10f, Recovery = 0.38f, Damage = 18f, Stagger = 22f, StaminaCost = 14f, ManaGain = 9f, Lunge = 1.3f, Reach = 2.7f, ArcDegrees = 100f, HitStopSeconds = 0.08f },
            },
            Heavy = new AttackStep { Name = "attack_heavy", Heavy = true, Windup = 0.30f, Active = 0.10f, Recovery = 0.46f, Damage = 26f, Stagger = 34f, StaminaCost = 20f, ManaGain = 10f, Lunge = 1.0f, Reach = 2.8f, ArcDegrees = 90f, HitStopSeconds = 0.10f, Shake = 0.16f },
            ChainHeavy = new AttackStep { Name = "attack_heavy", Heavy = true, Windup = 0.20f, Active = 0.10f, Recovery = 0.46f, Damage = 30f, Stagger = 40f, StaminaCost = 20f, ManaGain = 12f, Lunge = 1.2f, Reach = 2.8f, ArcDegrees = 90f, HitStopSeconds = 0.11f, Shake = 0.2f },
            Skill = new AttackStep { Name = "attack_iaido", Heavy = true, Windup = 0.08f, Active = 0.12f, Recovery = 0.50f, Damage = 48f, Stagger = 50f, StaminaCost = 15f, ManaGain = 0f, Lunge = 2.6f, Reach = 3.3f, ArcDegrees = 170f, HitStopSeconds = 0.14f, Shake = 0.3f },
            SkillManaCost = 30f,
            ChargeTime = 0.4f,
        };

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            defence = GetComponent<PlayerDefence>();
            Combo.Reset();
            Mana.Empty();
        }

        private void Start()
        {
            lightAction = player.FindAction("LightAttack");
            heavyAction = player.FindAction("HeavyAttack");
            moveAction = player.FindAction("Move");
        }

        /// <summary>Stops the attack in progress: a dodge or a guard cutting a recovery short, or a blow landing on the player.</summary>
        public void Interrupt()
        {
            Combo.Cancel();
            Step = Vector3.zero;
        }

        /// <summary>
        /// Runs the weapon for one frame. While the player cannot use it (rolling, flinching, drinking) only the clocks run.
        /// </summary>
        public void Advance(float deltaTime, bool unavailable)
        {
            Step = Vector3.zero;
            if (lightAction == null || heavyAction == null) return;
            if (lightAction.WasPressedThisFrame()) Combo.Press();
            if (heavyAction.WasPressedThisFrame()) Combo.PressHeavy();
            if (!heavyAction.IsPressed()) Combo.ReleaseHeavy();
            if (unavailable)
            {
                Combo.Idle(deltaTime);
                return;
            }

            var events = Combo.Tick(deltaTime, player.Stamina, Mana);
            if ((events & ComboEvents.Charged) != 0 && defence != null) defence.Callout("Ready");
            if ((events & ComboEvents.Started) != 0)
            {
                Aim();
                if (Combo.Current == Combo.Skill) SkillsUsed++;
            }
            if ((events & ComboEvents.Hit) != 0) Strike(Combo.Current);

            var step = Combo.Current;
            if (Combo.Phase == AttackPhase.Windup && step.Windup > 0f)
                Step = transform.forward * (step.Lunge * deltaTime / step.Windup);
        }

        /// <summary>Turns the player to the nearest enemy all around, if one is in range. The guard uses it.</summary>
        public void FaceNearestEnemy()
        {
            if (TryFindEnemy(transform.forward, 360f, out Vector3 direction))
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        // Face the enemy nearest to where the stick points; with no enemy in the cone, face the stick; with no stick, stay.
        private void Aim()
        {
            Vector3 wish = player.CameraRelative(moveAction.ReadValue<Vector2>());
            Vector3 toward = wish.sqrMagnitude > 0.01f ? wish.normalized : transform.forward;
            if (!TryFindEnemy(toward, AimConeDegrees, out Vector3 best)) best = toward;
            transform.rotation = Quaternion.LookRotation(best, Vector3.up);
        }

        private bool TryFindEnemy(Vector3 toward, float coneDegrees, out Vector3 direction)
        {
            direction = toward;
            float bestScore = float.MaxValue;
            int count = Overlap(transform.position, AimRange);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root || other.GetComponentInParent<IDamageable>() == null) continue;
                Vector3 offset = other.bounds.center - transform.position;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (distance < 0.05f) continue;
                float angle = Vector3.Angle(toward, offset);
                if (angle > coneDegrees * 0.5f) continue;
                // Nearness matters most; the angle breaks ties between enemies at similar range.
                float score = distance + angle * 0.02f;
                if (score < bestScore)
                {
                    bestScore = score;
                    direction = offset / distance;
                }
            }
            return bestScore < float.MaxValue;
        }

        private int Overlap(Vector3 centre, float radius)
        {
            int count = Physics.OverlapSphereNonAlloc(centre, radius, overlaps, HitLayers, QueryTriggerInteraction.Ignore);
            // A full buffer means colliders were dropped, and one of them may have been an enemy.
            if (count == overlaps.Length && !warnedOverlapsFull)
            {
                warnedOverlapsFull = true;
                Debug.LogWarning("[Clover.Player] An attack touched more colliders than it can hold; some targets may be missed. Narrow HitLayers to the layers enemies are on.");
            }
            return count;
        }

        private void Strike(AttackStep step)
        {
            struck.Clear();
            bool finisher = false;
            Vector3 origin = transform.position + Vector3.up;
            int count = Overlap(origin, step.Reach);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || struck.Contains(target)) continue;
                Vector3 offset = other.ClosestPoint(origin) - origin;
                offset.y = 0f;
                // Something touching the player is hit whatever the angle; otherwise it must be inside the swing's arc.
                if (offset.magnitude > 0.6f && Vector3.Angle(transform.forward, offset) > step.ArcDegrees * 0.5f) continue;
                var outcome = target.TakeHit(new Hit(step.Damage, step.Stagger, transform.forward, step.Heavy));
                if (outcome == HitOutcome.Missed) continue;
                struck.Add(target);
                finisher |= outcome == HitOutcome.Finisher;
                Mana.Gain(step.ManaGain);
                BlowsLanded++;
                if (defence != null) defence.DealtDamage(step.Damage);
            }
            if (struck.Count == 0) return;

            if (finisher)
            {
                Finishers++;
                if (defence != null) defence.Callout("Finisher");
            }
            if (hitStop != null) hitStop.Freeze(finisher ? Mathf.Max(FinisherHitStop, step.HitStopSeconds) : step.HitStopSeconds);
            float shake = finisher ? Mathf.Max(FinisherShake, step.Shake) : step.Shake;
            if (shaker != null && shake > 0f) shaker.Shake(shake, 0.25f);
        }
    }
}
