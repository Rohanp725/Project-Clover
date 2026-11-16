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
        [SerializeField] private TargetReticle reticle;

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
        /// <summary>How close the camera leans in on a finisher (times its usual distance); 1 leaves it alone.</summary>
        public float FinisherCameraDistance = 0.8f;
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
        /// <summary>What the element's upgrades add: a standing share of damage (Heat), and a one-off multiplier the next blow that lands spends (Stored Charge).</summary>
        public float UpgradeDamage = 1f;
        public float NextBlowDamage = 1f;
        public float NextBlowStagger = 1f;
        /// <summary>How much deeper each blow of the weapon makes its target's burn, and for how long it then burns.</summary>
        public int BurnStacksOnHit;
        public float StackBurnSeconds = 4f;
        /// <summary>What the learned upgrades and timed boons make of the player's numbers; <see cref="PlayerUpgrades"/> keeps these up to date.</summary>
        public float StatDamage = 1f;
        public float StatStagger = 1f;
        public float StatMana = 1f;
        public float StatSwing = 1f;
        /// <summary>What damage to an enemy that carries any affliction is multiplied by.</summary>
        public float StatAfflicted = 1f;
        /// <summary>The chance that a blow of the weapon is critical, and what a critical blow multiplies its damage by.</summary>
        public float CritChance;
        public float CritMultiplier = 1.5f;
        /// <summary>The affliction every blow of the weapon lays once the element's root is learned, and how deep.</summary>
        public AfflictionKind WeaponAffliction;
        public int WeaponAfflictionStacks;
        /// <summary>Added to every laying of stacks, and what the seconds and the strength of everything laid are multiplied by.</summary>
        public int ExtraStacks;
        public float AfflictionSeconds = 1f;
        public float AfflictionPower = 1f;
        /// <summary>The affliction the surge adds to every blow while it lasts, and how deep.</summary>
        public AfflictionKind EmpowerAffliction { get; private set; }
        public int EmpowerStacks { get; private set; }
        public int Criticals { get; private set; }
        /// <summary>The damage of the last blow of the weapon that landed, as it was dealt.</summary>
        public float LastBlowDamage { get; private set; }

        private PlayerController player;
        private PlayerDefence defence;
        private PlayerUpgrades upgrades;
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
        // Spells and lingering effects keep their own lists, so one landing in the middle of a swing cannot disturb it.
        private readonly HashSet<IDamageable> spellStruck = new HashSet<IDamageable>();
        // What each collider belongs to, found once: looking for whom an attack would face is done several times a second.
        private readonly Dictionary<Collider, Component> targetOf = new Dictionary<Collider, Component>();

        /// <summary>Adds the brackets that show whom the next attack would turn toward, so they can flare when a blow lands.</summary>
        public void ConfigureFeedback(TargetReticle reticleToFlare) => reticle = reticleToFlare;

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
            upgrades = GetComponent<PlayerUpgrades>();
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
        public void Empower(float damageMultiplier, float burnSeconds, Color slashColour, float speed = 1f, float stagger = 0f,
            AfflictionKind affliction = AfflictionKind.None, int stacks = 0)
        {
            EmpowerAffliction = affliction;
            EmpowerStacks = stacks;
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
            EmpowerAffliction = AfflictionKind.None;
            EmpowerStacks = 0;
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

            var events = Combo.Tick(deltaTime * SwingSpeed * StatSwing, player.Stamina, Mana);
            if ((events & ComboEvents.Charged) != 0 && defence != null) defence.Callout("Ready");
            if ((events & ComboEvents.Started) != 0)
            {
                Aim();
                if (Combo.Current == Combo.Skill) SkillsUsed++;
            }
            if ((events & ComboEvents.Hit) != 0) Strike(Combo.Current);

            var step = Combo.Current;
            if (Combo.Phase == AttackPhase.Windup && step.Windup > 0f)
                Step = transform.forward * (lunge * deltaTime * SwingSpeed * StatSwing / step.Windup);
        }

        // The old way of describing a spell's blow, kept for the upgrades written by hand: fire that burns for some seconds.
        private static SpellBlow Plain(float damage, float stagger, float burnSeconds, VfxKind effect, bool detonate, bool heavy = false) => new SpellBlow
        {
            Damage = damage,
            Stagger = stagger,
            Affliction = burnSeconds > 0f ? AfflictionKind.Burn : AfflictionKind.None,
            AfflictionSeconds = burnSeconds,
            Detonate = detonate,
            Heavy = heavy,
            Effect = effect,
        };

        // One blow of a spell or an effect as the target receives it, with everything the player's numbers add.
        private Hit Make(in SpellBlow blow, IDamageable target, Vector3 direction, Vector3 origin)
        {
            float damage = blow.Damage;
            if (StatAfflicted != 1f && target is IAfflictable afflictable && afflictable.Afflictions.Any) damage *= StatAfflicted;
            if (blow.Affliction == AfflictionKind.Burn)
            {
                // A burn is given in seconds, with or without depth.
                float seconds = (blow.AfflictionSeconds > 0f ? blow.AfflictionSeconds : Clover.Gameplay.Afflictions.Spec(AfflictionKind.Burn).Seconds) * AfflictionSeconds;
                return new Hit(damage, blow.Stagger, direction, blow.Heavy, true, origin, seconds, blow.Stacks > 0 ? blow.Stacks + ExtraStacks : 0, blow.Detonate,
                    AfflictionKind.None, 0, 0f, AfflictionPower, blow.Spell);
            }
            bool lays = blow.Affliction != AfflictionKind.None && blow.Stacks > 0;
            float lasts = !lays ? 0f : blow.AfflictionSeconds > 0f ? blow.AfflictionSeconds * AfflictionSeconds
                : AfflictionSeconds != 1f ? Clover.Gameplay.Afflictions.Spec(blow.Affliction).Seconds * AfflictionSeconds : 0f;
            return new Hit(damage, blow.Stagger, direction, blow.Heavy, true, origin, 0f, 0, blow.Detonate,
                lays ? blow.Affliction : AfflictionKind.None, lays ? blow.Stacks + ExtraStacks : 0, lasts, AfflictionPower, blow.Spell);
        }

        // What every spell's blow does once it has landed: its effect, what it earns, and word to the upgrades.
        private void Landed(in SpellBlow blow, IDamageable target, Collider other, Vector3 direction, bool wasAfflicted)
        {
            if (blow.Tinted) Vfx.Play(blow.Effect, other.bounds.center, direction, 1f, blow.Tint);
            else Vfx.Play(blow.Effect, other.bounds.center, direction);
            if (defence != null) defence.DealtDamage(blow.Damage);
            if (blow.Spell && upgrades != null) upgrades.OnSpellStruck(target, wasAfflicted);
        }

        /// <summary>
        /// Strikes everything in a cone in front of the player that is not the player: what a spell does. Returns how
        /// many things it hit. The blow is described by the caller, so spells and weapons share one path to a target.
        /// </summary>
        public int StrikeCone(float reach, float arcDegrees, float damage, float stagger, float burnSeconds, VfxKind effect, bool detonate = false) =>
            StrikeCone(reach, arcDegrees, Plain(damage, stagger, burnSeconds, effect, detonate));

        public int StrikeCone(float reach, float arcDegrees, in SpellBlow blow)
        {
            var struck = spellStruck;
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
                bool wasAfflicted = target is IAfflictable afflictable && afflictable.Afflictions.Any;
                var outcome = target.TakeHit(Make(blow, target, transform.forward, transform.position));
                if (outcome == HitOutcome.Missed) continue;
                struck.Add(target);
                Landed(blow, target, other, transform.forward, wasAfflicted);
            }
            return struck.Count;
        }

        /// <summary>
        /// Strikes everything standing in a circle on the ground: what a burst, a crater or a patch of fire does. Given a
        /// list, it leaves alone whoever is already on it and adds whoever it strikes, so a lasting effect can bite each
        /// enemy once only. Returns how many it hit.
        /// </summary>
        public int StrikeArea(Vector3 centre, float radius, float damage, float stagger, float burnSeconds, VfxKind effect,
            HashSet<IDamageable> once = null, bool detonate = false, bool heavy = false) =>
            StrikeArea(centre, radius, Plain(damage, stagger, burnSeconds, effect, detonate, heavy), once);

        public int StrikeArea(Vector3 centre, float radius, in SpellBlow blow, HashSet<IDamageable> once = null)
        {
            spellStruck.Clear();
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || (once != null && once.Contains(target))) continue;
                Vector3 offset = other.ClosestPoint(centre) - centre;
                offset.y = 0f;
                if (offset.magnitude > radius) continue;
                Vector3 away = ((Component)target).transform.position - centre;
                away.y = 0f;
                bool wasAfflicted = target is IAfflictable afflictable && afflictable.Afflictions.Any;
                var outcome = target.TakeHit(Make(blow, target, away.sqrMagnitude > 0.01f ? away.normalized : transform.forward, centre));
                if (outcome == HitOutcome.Missed) continue;
                spellStruck.Add(target);
                if (once != null) once.Add(target);
                Landed(blow, target, other, Vector3.up, wasAfflicted);
            }
            return spellStruck.Count;
        }

        /// <summary>
        /// Lays an affliction on everything standing in a circle, without a blow: what a Soaked enemy does with the
        /// Static laid on it, and what an upgrade that afflicts all around does. Returns how many it reached.
        /// </summary>
        public int AfflictArea(Vector3 centre, float radius, AfflictionKind kind, int stacks, Component except = null)
        {
            spellStruck.Clear();
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || ReferenceEquals(target, except) || !(target is IAfflictable afflictable)) continue;
                Vector3 offset = ((Component)target).transform.position - centre;
                offset.y = 0f;
                if (offset.magnitude > radius) continue;
                spellStruck.Add(target);
                // Carried, not struck: it is not carried on again from here, so two Soaked enemies cannot pass it back and forth.
                afflictable.Afflictions.Apply(kind, stacks + ExtraStacks, 0f, AfflictionPower);
            }
            return spellStruck.Count;
        }

        /// <summary>Drags everything in a circle toward its middle, or throws it away from there, by about this many metres. Returns how many it moved.</summary>
        public int Displace(Vector3 centre, float radius, float metres, bool pull)
        {
            spellStruck.Clear();
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || !(target is IShovable shovable)) continue;
                Vector3 offset = ((Component)target).transform.position - centre;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (distance > radius) continue;
                spellStruck.Add(target);
                Vector3 away = distance > 0.05f ? offset / distance : transform.forward;
                // A pull never drags it past the middle.
                shovable.Push(pull ? -away * Mathf.Min(metres, distance) : away * metres);
            }
            return spellStruck.Count;
        }

        /// <summary>The nearest enemy all around within aiming range, or null. The guard uses it to know whom to face.</summary>
        public Transform NearestEnemy(float range = 0f) => FindEnemy(transform.forward, 360f, range > 0f ? range : AimRange, false);

        /// <summary>The enemy standing nearest a point, within a radius of it, or null: who it was that struck from there.</summary>
        public Component EnemyAt(Vector3 point, float radius)
        {
            Component found = null;
            float best = float.MaxValue;
            int count = Overlap(point + Vector3.up, radius);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                if (!targetOf.TryGetValue(other, out Component target))
                {
                    target = other.GetComponentInParent<IDamageable>() as Component;
                    targetOf[other] = target;
                }
                if (target == null) continue;
                float distance = (target.transform.position - point).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                found = target;
            }
            return found;
        }

        /// <summary>The enemy the player is facing within a range; failing one, the nearest all around; failing that, null.</summary>
        public Transform EnemyAhead(float range, float coneDegrees = 110f)
        {
            Transform ahead = FindEnemy(transform.forward, coneDegrees, range, false);
            return ahead != null ? ahead : FindEnemy(transform.forward, 360f, range, false);
        }

        /// <summary>
        /// Strikes the enemy the player faces, within reach, and then leaps on to the nearest others not yet struck: what
        /// a bolt does. A bolt from the sky marks each one. Returns how many it hit.
        /// </summary>
        public int StrikeChain(float reach, float damage, float stagger, int leaps, float leapRange) =>
            StrikeChain(reach, new SpellBlow { Damage = damage, Stagger = stagger, Effect = VfxKind.Shock }, leaps, leapRange, Elements.ByName(Elements.Lightning));

        /// <summary>
        /// The same in any element's colour, and from a first enemy of the caller's choosing when one is given (an arc
        /// that starts at whoever was just struck).
        /// </summary>
        public int StrikeChain(float reach, in SpellBlow blow, int leaps, float leapRange, ElementInfo look, Transform first = null)
        {
            var struck = spellStruck;
            struck.Clear();
            Transform at = first != null ? first : FindEnemy(transform.forward, 150f, reach, false);
            Vector3 from = transform.position;
            int hits = 0;
            for (int leap = 0; leap <= leaps && at != null; leap++)
            {
                var target = at.GetComponentInParent<IDamageable>();
                if (target == null) break;
                struck.Add(target);
                Vector3 direction = at.position - from;
                direction.y = 0f;
                bool wasAfflicted = target is IAfflictable afflictable && afflictable.Afflictions.Any;
                var outcome = target.TakeHit(Make(blow, target, direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward, from));
                ElementLook.Strike(look, at.position, leap == 0 ? 0.5f : 0.2f);
                if (outcome != HitOutcome.Missed)
                {
                    hits++;
                    if (defence != null) defence.DealtDamage(blow.Damage);
                    if (blow.Spell && upgrades != null) upgrades.OnSpellStruck(target, wasAfflicted);
                }
                from = at.position;
                at = NextUnstruck(from, leapRange, struck);
            }
            return hits;
        }

        // The nearest enemy to a point that the current strike has not yet touched.
        private Transform NextUnstruck(Vector3 from, float range, HashSet<IDamageable> struck)
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

        /// <summary>
        /// The enemy an attack started now would turn toward, or null: the one nearest to where the stick points (or, with
        /// no stick, to where the player faces) within the aiming cone and range. The attack itself asks this same question.
        /// </summary>
        public Transform AimTarget() => moveAction == null ? null : FindEnemy(AimDirection(), AimConeDegrees, AimRange, false);

        private Vector3 AimDirection()
        {
            Vector3 wish = player.CameraRelative(moveAction.ReadValue<Vector2>());
            return wish.sqrMagnitude > 0.01f ? wish.normalized : transform.forward;
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
                toward = AimDirection();
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
                if (!targetOf.TryGetValue(other, out Component target))
                {
                    target = other.GetComponentInParent<IDamageable>() as Component;
                    targetOf[other] = target;
                }
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

        // The way the blade travels through what it strikes: across the body for a sweeping cut, on the side it is
        // swung from to the other, leaning into the target; straight in for a thrust or a blow with no sweep.
        private Vector3 CutDirection(AttackStep step)
        {
            if (Mathf.Abs(step.Sweep) < 0.01f) return transform.forward;
            return (-transform.right * Mathf.Sign(step.Sweep) + transform.forward * 0.45f).normalized;
        }

        private void Strike(AttackStep step)
        {
            struck.Clear();
            bool finisher = false;
            bool clean = false;
            Vector3 finished = transform.position;
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
                var afflictable = target as IAfflictable;
                // A blow is critical by chance, or always on an enemy that is Marked.
                bool critical = (afflictable != null && afflictable.Afflictions.MakesBlowsCritical) || (CritChance > 0f && Random.value < CritChance);
                float damage = step.Damage * DamageMultiplier * UpgradeDamage * NextBlowDamage * StatDamage;
                if (critical) damage *= CritMultiplier;
                bool wasAfflicted = afflictable != null && afflictable.Afflictions.Any;
                if (wasAfflicted) damage *= StatAfflicted;
                float burn = (BurnStacksOnHit > 0 ? Mathf.Max(BurnOnHit, StackBurnSeconds) : BurnOnHit) * AfflictionSeconds;
                // What the root of the tree and the surge lay, together, in the element's own affliction.
                int stacks = WeaponAfflictionStacks + EmpowerStacks;
                var kind = stacks <= 0 ? AfflictionKind.None : WeaponAfflictionStacks > 0 ? WeaponAffliction : EmpowerAffliction;
                if (kind != AfflictionKind.None) stacks += ExtraStacks;
                float lasts = kind != AfflictionKind.None && AfflictionSeconds != 1f ? Clover.Gameplay.Afflictions.Spec(kind).Seconds * AfflictionSeconds : 0f;
                var outcome = target.TakeHit(new Hit(damage, (step.Stagger + StaggerOnHit) * NextBlowStagger * StatStagger, transform.forward, step.Heavy, false, transform.position,
                    burn, BurnStacksOnHit > 0 ? BurnStacksOnHit + ExtraStacks : 0, false, kind, kind != AfflictionKind.None ? stacks : 0, lasts, AfflictionPower, false, critical));
                if (outcome == HitOutcome.Missed) continue;
                struck.Add(target);
                LastBlowDamage = damage;
                float earned = step.ManaGain * StatMana;
                if (outcome != HitOutcome.Blocked)
                {
                    if (critical || NextBlowDamage > 1f)
                    {
                        // The sign of a critical blow: a gold star with long points where it lands. A stored charge shows the same.
                        if (critical) Criticals++;
                        Vfx.Play(VfxKind.Critical, other.bounds.center);
                        Vfx.Play(VfxKind.TintSpark, other.bounds.center, transform.forward, 1.6f, new Color(1f, 0.85f, 0.35f));
                    }
                    if (afflictable != null)
                    {
                        // Seeded: the blow feeds the one who struck it. Marked: it gives more mana.
                        float fed = afflictable.Afflictions.HealOnBlow;
                        if (fed > 0f && defence != null) defence.Heal(fed, false);
                        earned += afflictable.Afflictions.ManaOnBlow;
                    }
                }
                if (upgrades != null) upgrades.OnBlow(step, target, outcome, wasAfflicted);
                finisher |= outcome == HitOutcome.Finisher;
                if (outcome == HitOutcome.Finisher) finished = other.bounds.center;
                clean |= outcome != HitOutcome.Blocked;
                if (outcome != HitOutcome.Blocked) Vfx.Play(step.Heavy ? VfxKind.HeavySpark : VfxKind.HitSpark, other.ClosestPoint(origin), transform.forward);
                else
                {
                    // Turned on the enemy's guard: steel on steel, and a jolt back up the arm.
                    BlowsTurned++;
                    // A heavy blow turned aside is no reason to recommend a heavy blow.
                    if (!step.Heavy) Tips.BlowTurned();
                    Vfx.Play(VfxKind.Clang, other.ClosestPoint(origin));
                    if (shaker != null) shaker.Shake(0.06f, 0.1f);
                }
                if (outcome != HitOutcome.Blocked)
                {
                    // The cut line: which way the blade went through, drawn over the sparks.
                    Vector3 through = CutDirection(step);
                    if (outcome == HitOutcome.Finisher) CutMarks.Finisher(other.bounds.center, through);
                    else if (step.Heavy) CutMarks.Heavy(other.bounds.center, through);
                    else CutMarks.Light(other.bounds.center, through);
                }
                if (reticle != null && target is Component struckThing) reticle.Struck(struckThing.transform);
                if (outcome == HitOutcome.Finisher) Vfx.Play(VfxKind.Burst, other.bounds.center);
                if (step.Heavy && outcome != HitOutcome.Blocked) Vfx.Play(VfxKind.Impact, other.bounds.center);
                Mana.Gain(earned);
                BlowsLanded++;
                if (defence != null) defence.DealtDamage(damage);
            }
            if (upgrades != null) upgrades.OnSwing(step, struck.Count);
            if (struck.Count == 0) return;
            if (clean && (NextBlowDamage != 1f || NextBlowStagger != 1f))
            {
                // The stored charge is spent on the blow that lands, not on one turned aside.
                NextBlowDamage = 1f;
                NextBlowStagger = 1f;
                Clover.World.LightningBolts.Strike(transform.position + transform.forward * step.Reach * 0.6f, 0.4f);
            }

            if (finisher)
            {
                Finishers++;
                Tips.FinisherLanded();
                if (defence != null) defence.Callout("Finisher");
                ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.45f), 0.07f);
            }
            if (hitStop != null) hitStop.Freeze(finisher ? Mathf.Max(FinisherHitStop, step.HitStopSeconds) : step.HitStopSeconds);
            // After a finisher's stop, time comes back slowly: the blow gets a moment to be looked at.
            // The slow is timed from now and the stop runs first, so it is asked to outlast the stop.
            if (finisher && hitStop != null)
                hitStop.Slow(FinisherSlowSeconds + Mathf.Max(FinisherHitStop, step.HitStopSeconds), FinisherSlowScale);
            if (finisher && shaker != null && FinisherCameraDistance < 1f)
            {
                // The camera leans in between the two of them for as long as time is stopped and slowed. It does
                // nothing while a big fight holds its own framing.
                Vector3 between = (finished - transform.position) * 0.5f;
                between.y = 0f;
                shaker.CloseIn(FinisherCameraDistance, Vector3.ClampMagnitude(between, 2f), FinisherSlowSeconds + Mathf.Max(FinisherHitStop, step.HitStopSeconds));
            }
            float shake = finisher ? Mathf.Max(FinisherShake, step.Shake) : step.Shake;
            if (shaker != null && shake > 0f) shaker.Shake(shake, 0.25f);
        }
    }
}
