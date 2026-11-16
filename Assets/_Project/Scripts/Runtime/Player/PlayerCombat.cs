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
        [SerializeField] private SlashArcs arcs;

        /// <summary>The weapon the player starts the scene with: a name from <see cref="Weapons"/>.</summary>
        public string StartingWeapon = Weapons.FistsName;
        /// <summary>The weapon in hand. Its numbers live in <see cref="Weapons"/> and are built fresh each play.</summary>
        public Weapon Weapon { get; private set; }
        public WeaponCombo Combo => Weapon.Combo;
        public ManaPool Mana = new ManaPool();
        /// <summary>Metres within which an attack turns to face an enemy.</summary>
        public float AimRange = 4.5f;
        /// <summary>Full width in degrees, around the stick's direction, in which an enemy is picked to face.</summary>
        public float AimConeDegrees = 140f;
        /// <summary>Seconds time nearly stops for a finisher, whatever the attack's own value.</summary>
        public float FinisherHitStop = 0.16f;
        public float FinisherShake = 0.4f;
        /// <summary>Metres within which the heavy button finishes a staggered enemy instead of swinging.</summary>
        public float FinisherRange = 5f;
        /// <summary>Metres from the enemy's centre the finisher steps in to.</summary>
        public float FinisherStandOff = 1.2f;
        /// <summary>Real seconds of slowed time after a finisher's stop, and how slow.</summary>
        public float FinisherSlowSeconds = 0.45f;
        public float FinisherSlowScale = 0.35f;
        public LayerMask HitLayers = ~0;

        public bool IsAttacking => Combo.IsAttacking;
        public bool IsCharging => Combo.IsCharging;
        /// <summary>True during the finisher move; blows pass through the player while it lasts.</summary>
        public bool IsFinishing => Combo.IsFinishing;
        public bool CanBeCancelled => Combo.CanBeCancelled;
        /// <summary>How far the attack moves the player this frame; zero outside a wind-up.</summary>
        public Vector3 Step { get; private set; }
        /// <summary>Counts every blow that struck something, so tests and probes can see hits without watching the screen.</summary>
        public int BlowsLanded { get; private set; }
        public int Finishers { get; private set; }
        public int SkillsUsed { get; private set; }
        /// <summary>Blows an enemy turned on its guard.</summary>
        public int BlowsTurned { get; private set; }
        /// <summary>What an element's surge adds to every blow: more damage, and seconds of burning.</summary>
        public float DamageMultiplier { get; private set; } = 1f;
        public float BurnOnHit { get; private set; }
        /// <summary>How much faster than usual the weapon is swung, and stagger added to each blow.</summary>
        public float SwingSpeed { get; private set; } = 1f;
        public float StaggerOnHit { get; private set; }

        private PlayerController player;
        private PlayerDefence defence;
        private InputAction lightAction;
        private InputAction heavyAction;
        private InputAction moveAction;
        private readonly Collider[] overlaps = new Collider[64];
        private bool warnedOverlapsFull;
        private Transform finisherTarget;
        private float lunge;
        private bool empowered;
        private Color empoweredColour;
        private readonly HashSet<IDamageable> struck = new HashSet<IDamageable>();

        public void Configure(HitStop hitStopToUse, TopDownCamera cameraToShake, SlashArcs arcsToShow)
        {
            hitStop = hitStopToUse;
            shaker = cameraToShake;
            arcs = arcsToShow;
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            defence = GetComponent<PlayerDefence>();
            // A weapon carried over from the last scene of the run takes the place of the scene's own.
            Equip(Weapons.ByName(string.IsNullOrEmpty(RunState.Weapon) ? StartingWeapon : RunState.Weapon));
            Mana.Empty();
        }

        private void Start()
        {
            lightAction = player.FindAction("LightAttack");
            heavyAction = player.FindAction("HeavyAttack");
            moveAction = player.FindAction("Move");
        }

        /// <summary>Takes a weapon in hand, dropping whatever attack was in progress.</summary>
        public void Equip(Weapon weapon)
        {
            Weapon = weapon;
            RunState.Weapon = weapon.Name;
            Weapon.Combo.Reset();
            Step = Vector3.zero;
        }

        /// <summary>An element's surge wraps the weapon: every blow hits harder, sets its target burning, and cuts in the element's colour.</summary>
        public void Empower(float damageMultiplier, float burnSeconds, Color slashColour, float speed = 1f, float stagger = 0f)
        {
            DamageMultiplier = damageMultiplier;
            BurnOnHit = burnSeconds;
            SwingSpeed = Mathf.Max(0.1f, speed);
            StaggerOnHit = stagger;
            empowered = true;
            empoweredColour = slashColour;
        }

        public void ClearEmpower()
        {
            DamageMultiplier = 1f;
            BurnOnHit = 0f;
            SwingSpeed = 1f;
            StaggerOnHit = 0f;
            empowered = false;
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
            if (heavyAction.WasPressedThisFrame())
            {
                // Looked for on the press only, not every frame: a staggered enemy in reach turns the heavy into the finisher.
                finisherTarget = FindEnemy(transform.forward, 360f, FinisherRange, true);
                Combo.PressHeavy(finisherTarget != null);
            }
            if (!heavyAction.IsPressed()) Combo.ReleaseHeavy();
            if (unavailable)
            {
                Combo.Idle(deltaTime);
                return;
            }

            var events = Combo.Tick(deltaTime * SwingSpeed, player.Stamina, Mana);
            if ((events & ComboEvents.Charged) != 0 && defence != null) defence.Callout("Ready");
            if ((events & ComboEvents.Started) != 0)
            {
                Aim();
                if (Combo.Current == Combo.Skill) SkillsUsed++;
            }
            if ((events & ComboEvents.Hit) != 0) Strike(Combo.Current);

            var step = Combo.Current;
            if (Combo.Phase == AttackPhase.Windup && step.Windup > 0f)
                Step = transform.forward * (lunge * deltaTime * SwingSpeed / step.Windup);
        }

        /// <summary>
        /// Strikes everything in a cone in front of the player that is not the player: what a spell does. Returns how
        /// many things it hit. The blow is described by the caller, so spells and weapons share one path to a target.
        /// </summary>
        public int StrikeCone(float reach, float arcDegrees, float damage, float stagger, float burnSeconds, VfxKind effect)
        {
            struck.Clear();
            Vector3 origin = transform.position + Vector3.up;
            int count = Overlap(origin, reach);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || struck.Contains(target)) continue;
                Vector3 offset = other.ClosestPoint(origin) - origin;
                offset.y = 0f;
                if (offset.magnitude > 0.6f && Vector3.Angle(transform.forward, offset) > arcDegrees * 0.5f) continue;
                var outcome = target.TakeHit(new Hit(damage, stagger, transform.forward, false, true, transform.position, burnSeconds));
                if (outcome == HitOutcome.Missed) continue;
                struck.Add(target);
                Vfx.Play(effect, other.bounds.center, transform.forward);
                if (defence != null) defence.DealtDamage(damage);
            }
            return struck.Count;
        }

        /// <summary>The nearest enemy all around within aiming range, or null. The guard uses it to know whom to face.</summary>
        public Transform NearestEnemy(float range = 0f) => FindEnemy(transform.forward, 360f, range > 0f ? range : AimRange, false);

        /// <summary>
        /// Strikes the enemy the player faces, within reach, and then leaps on to the nearest others not yet struck: what
        /// a bolt does. A bolt from the sky marks each one. Returns how many it hit.
        /// </summary>
        public int StrikeChain(float reach, float damage, float stagger, int leaps, float leapRange)
        {
            struck.Clear();
            Transform at = FindEnemy(transform.forward, 150f, reach, false);
            Vector3 from = transform.position;
            int hits = 0;
            for (int leap = 0; leap <= leaps && at != null; leap++)
            {
                var target = at.GetComponentInParent<IDamageable>();
                if (target == null) break;
                struck.Add(target);
                Vector3 direction = at.position - from;
                direction.y = 0f;
                var outcome = target.TakeHit(new Hit(damage, stagger, direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward, false, true, from));
                Clover.World.LightningBolts.Strike(at.position, leap == 0 ? 0.5f : 0.2f);
                if (outcome != HitOutcome.Missed)
                {
                    hits++;
                    if (defence != null) defence.DealtDamage(damage);
                }
                from = at.position;
                at = NextUnstruck(from, leapRange);
            }
            return hits;
        }

        // The nearest enemy to a point that the current strike has not yet touched.
        private Transform NextUnstruck(Vector3 from, float range)
        {
            Transform found = null;
            float best = float.MaxValue;
            int count = Overlap(from, range);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || struck.Contains(target)) continue;
                float distance = (((Component)target).transform.position - from).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                found = ((Component)target).transform;
            }
            return found;
        }

        // Face the enemy nearest to where the stick points; with no enemy in the cone, face the stick; with no stick, stay.
        private void Aim()
        {
            lunge = Combo.Current.Lunge;
            Transform enemy;
            Vector3 toward;
            if (Combo.IsFinishing && finisherTarget != null)
            {
                // The finisher goes to the enemy it was pressed for, and steps in exactly as far as it needs to.
                enemy = finisherTarget;
                toward = transform.forward;
                Vector3 gap = enemy.position - transform.position;
                gap.y = 0f;
                lunge = Mathf.Clamp(gap.magnitude - FinisherStandOff, 0f, FinisherRange);
            }
            else
            {
                Vector3 wish = player.CameraRelative(moveAction.ReadValue<Vector2>());
                toward = wish.sqrMagnitude > 0.01f ? wish.normalized : transform.forward;
                enemy = FindEnemy(toward, AimConeDegrees, AimRange, false);
            }
            if (enemy != null)
            {
                Vector3 offset = enemy.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.0025f) toward = offset.normalized;
            }
            transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
        }

        // The enemy nearest to the given direction within a cone and a range; optionally only one that is staggered.
        private Transform FindEnemy(Vector3 toward, float coneDegrees, float range, bool staggeredOnly)
        {
            Transform found = null;
            float bestScore = float.MaxValue;
            int count = Overlap(transform.position, range);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>() as Component;
                if (target == null) continue;
                if (staggeredOnly && !(target is IStaggerable staggerable && staggerable.IsStaggered)) continue;
                Vector3 offset = target.transform.position - transform.position;
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
                    found = target.transform;
                }
            }
            return found;
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
            if (arcs != null) arcs.Show(transform.position, transform.forward, step.Reach, step.ArcDegrees, step.Sweep, empowered ? empoweredColour : Weapon.SlashColour);
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
                float damage = step.Damage * DamageMultiplier;
                var outcome = target.TakeHit(new Hit(damage, step.Stagger + StaggerOnHit, transform.forward, step.Heavy, false, transform.position, BurnOnHit));
                if (outcome == HitOutcome.Missed) continue;
                struck.Add(target);
                finisher |= outcome == HitOutcome.Finisher;
                if (outcome != HitOutcome.Blocked) Vfx.Play(step.Heavy ? VfxKind.HeavySpark : VfxKind.HitSpark, other.ClosestPoint(origin), transform.forward);
                else
                {
                    // Turned on the enemy's guard: steel on steel, and a jolt back up the arm.
                    BlowsTurned++;
                    Vfx.Play(VfxKind.Clang, other.ClosestPoint(origin));
                    if (shaker != null) shaker.Shake(0.06f, 0.1f);
                }
                if (outcome == HitOutcome.Finisher) Vfx.Play(VfxKind.Burst, other.bounds.center);
                if (step.Heavy && outcome != HitOutcome.Blocked) Vfx.Play(VfxKind.Impact, other.bounds.center);
                Mana.Gain(step.ManaGain);
                BlowsLanded++;
                if (defence != null) defence.DealtDamage(damage);
            }
            if (struck.Count == 0) return;

            if (finisher)
            {
                Finishers++;
                if (defence != null) defence.Callout("Finisher");
                ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.6f), 0.08f);
            }
            if (hitStop != null) hitStop.Freeze(finisher ? Mathf.Max(FinisherHitStop, step.HitStopSeconds) : step.HitStopSeconds);
            // After a finisher's stop, time comes back slowly: the blow gets a moment to be looked at.
            // The slow is timed from now and the stop runs first, so it is asked to outlast the stop.
            if (finisher && hitStop != null)
                hitStop.Slow(FinisherSlowSeconds + Mathf.Max(FinisherHitStop, step.HitStopSeconds), FinisherSlowScale);
            float shake = finisher ? Mathf.Max(FinisherShake, step.Shake) : step.Shake;
            if (shaker != null && shake > 0f) shaker.Shake(shake, 0.25f);
        }
    }
}
