using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player.Forms;
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
        /// <summary>The player's numbers, for what is asked blow by blow (conditions on a blow, or on what the target carries). Set by <see cref="PlayerUpgrades"/>.</summary>
        [System.NonSerialized] public PlayerStats Stats;
        /// <summary>Seconds each blow holds an enemy up in the air, and how many blows can.</summary>
        public float AirBlowSeconds = 0.45f;
        public int AirBlowsMost = 3;
        /// <summary>True when the last blow of the weapon that landed was critical.</summary>
        public bool LastBlowCritical { get; private set; }
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
        /// <summary>Seconds left in which the next heavy blow that lands is critical: what a step through an enemy earns.</summary>
        public float HeavyCriticalLeft { get; private set; }

        /// <summary>For this many seconds the next heavy blow that lands is a critical one.</summary>
        public void PromiseHeavyCritical(float seconds) => HeavyCriticalLeft = Mathf.Max(HeavyCriticalLeft, seconds);

        /// <summary>Blows that fell in a weapon's tip band, blows taken on a counter's stance, and blows shrugged off inside a swing.</summary>
        public int SweetBlows { get; private set; }
        public int Counters { get; private set; }
        /// <summary>The stagger of the last blow of the weapon that landed, as it was dealt, and how far off its target stood.</summary>
        public float LastBlowStagger { get; private set; }
        public float LastBlowDistance { get; private set; }
        /// <summary>The stagger a counter adds to what it throws back, on top of the blow's own: what a perfect guard would have rebuffed.</summary>
        public float CounterStagger = 55f;

        /// <summary>
        /// A move landed on something that pays back (once per move, however many it struck), and a finisher landed:
        /// the spells' clocks are cut by each (STEEL_AND_ELEMENT.md 3.1). A finisher is told as a finisher only.
        /// </summary>
        public event System.Action<AttackStep> BlowLanded;
        public event System.Action Finished;
        private bool cutGiven;

        private PlayerController player;
        private PlayerDefence defence;
        private PlayerUpgrades upgrades;
        private PlayerElement element;
        private WeaponShots shots;
        private WeaponSpikes spikes;
        private PlantedArrows arrows;
        /// <summary>The bow's arrows that have landed, or null on a player built without them.</summary>
        public PlantedArrows Arrows => arrows;
        /// <summary>Whom the last strike of an area, a lane or a cone landed on. Read at once: the next such strike empties it.</summary>
        public HashSet<IDamageable> LastStruck => spellStruck;
        /// <summary>True while the bow's string is drawn back: the charge, or the wind-up of a step that looses something.</summary>
        public bool StringDrawn => Combo.IsCharging || (Combo.Current != null && Combo.Phase == AttackPhase.Windup
            && ((Combo.Current.Shot != null && Combo.Current.Shot.Plants && Combo.Current.Damage <= 0f) || Combo.Current.Effect == StepEffect.ArrowRain));

        /// <summary>The enemy the chain holds, or null; and counts for probes: binds made, times the chain pulled the player to something too heavy, perfect guards that did more.</summary>
        public Component BoundTarget => boundTo;
        /// <summary>Whom the finisher in progress is being made on, or null.</summary>
        public Transform FinisherTarget => Combo.IsFinishing ? finisherTarget : null;
        public float BoundLeft => boundTo != null ? boundLeft : 0f;
        public int Bindings { get; private set; }
        public int SelfPulls { get; private set; }
        public int GuardRiders { get; private set; }
        /// <summary>At this weight and over a thing is not dragged by a chain; the player is.</summary>
        public float HeavyWeight = 2f;
        /// <summary>Metres the chain lets its bearer stand from what it binds; a step further and it lets go.</summary>
        public float BindLeash = 7f;
        /// <summary>What a bound enemy's stagger from blows is multiplied by, and seconds something that cannot be held is bound.</summary>
        public float BoundStagger = 1.25f;
        public float GodBindSeconds = 1f;
        private Component boundTo, pullSelfTo;
        private float boundLeft, pullSelfIn;
        private int beatsDone;
        /// <summary>True on a frame in which the step carries the body through enemies (a passing cut, a finisher's crossing): the controller moves it past them.</summary>
        public bool PassingThrough { get; private set; }
        /// <summary>Counts for probes: blows that landed on a back, cuts made out of a dodge.</summary>
        public int BackBlows { get; private set; }
        public int DashAttacks { get; private set; }
        private AttackStep passTurn;
        private Vector3 stageForward, stageOrigin, dashFrom, dashTo, dashCentre;
        private float dashFor, dashAt;
        private bool holdDone;
        private float counterDamage;
        private float counterStagger;
        private readonly Guard usualGuard = new Guard();
        private readonly DodgeRoll usualDodge = new DodgeRoll();
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
            element = GetComponent<PlayerElement>();
            shots = GetComponent<WeaponShots>();
            spikes = GetComponent<WeaponSpikes>();
            arrows = GetComponent<PlantedArrows>();
            // A weapon carried over from the last scene of the run takes the place of the scene's own.
            Equip(Weapons.ByName(string.IsNullOrEmpty(RunState.Weapon) ? StartingWeapon : RunState.Weapon));
            // Every scene starts at half: mana now pays only for the big moves, and the player never starts a fight empty.
            Mana.Empty();
            Mana.AtLeast(Mana.SettleLevel);
        }

        private void Start()
        {
            lightAction = player.FindAction("LightAttack");
            heavyAction = player.FindAction("HeavyAttack");
            moveAction = player.FindAction("Move");
            // The Armament wears whatever element is held, which is known only once the element has woken too.
            if (Weapon != null && Weapon.ElementColoured && element != null && element.Info != null) Equip(Weapons.Armament(element.Info.Name));
        }

        /// <summary>Takes a weapon in hand, dropping whatever attack was in progress.</summary>
        public void Equip(Weapon weapon)
        {
            Weapon = weapon;
            RunState.Weapon = weapon.Name;
            Weapon.Combo.Reset();
            // The skill is on its own button (Triangle, Y, Q); a held heavy is the charged heavy.
            Weapon.Combo.SkillOnButton = true;
            // Whatever was thrown belongs to the weapon put down.
            if (WeaponAway && shots != null) shots.Release(awayShot);
            WeaponAway = false;
            IsDashing = false;
            pendingShot = null;
            followFor = null;
            followStep = null;
            boundTo = null;
            pullSelfTo = null;
            Step = Vector3.zero;
            PassingThrough = false;
            passTurn = null;
            dashFor = 0f;
            counterDamage = 0f;
            counterStagger = 0f;
            // The weapon brings its own guard and its own dodge: their numbers are copied, the state they keep is not.
            var guard = weapon.GuardProfile ?? usualGuard;
            if (defence == null) defence = GetComponent<PlayerDefence>();
            if (defence != null)
            {
                defence.Guard.PerfectWindow = guard.PerfectWindow;
                defence.Guard.PerfectFrom = guard.PerfectFrom;
                defence.Guard.StaminaPerDamage = guard.StaminaPerDamage;
                defence.Guard.ChipFraction = guard.ChipFraction;
                defence.Guard.ArcDegrees = guard.ArcDegrees;
                defence.GuardWidth(guard.ArcDegrees);
            }
            var dodge = weapon.DodgeProfile ?? usualDodge;
            if (player == null) player = GetComponent<PlayerController>();
            if (player != null)
            {
                player.Dodge.Distance = dodge.Distance;
                player.Dodge.Duration = dodge.Duration;
                player.Dodge.StaminaCost = dodge.StaminaCost;
                player.Dodge.InvulnerableFrom = dodge.InvulnerableFrom;
                player.Dodge.InvulnerableUntil = dodge.InvulnerableUntil;
                player.Dodge.Recovery = dodge.Recovery;
            }
            // The marks multiply the weapon's own numbers, not those of whatever was held when the play began.
            if (upgrades == null) upgrades = GetComponent<PlayerUpgrades>();
            if (upgrades != null) upgrades.WeaponChanged();
        }

        /// <summary>While true, a blow that lands on an enemy winding up or striking is critical. The player's upgrades keep it.</summary>
        public bool CounterCut { get; set; }
        /// <summary>Blows made critical that way, for probes.</summary>
        public int CounterCuts { get; private set; }
        /// <summary>While true, nothing that lands on the player interrupts a heavy blow being wound up or swung. The player's upgrades keep it.</summary>
        public bool HeavyUnbroken { get; set; }
        /// <summary>True while a heavy blow is being wound up or swung.</summary>
        public bool InHeavySwing
        {
            get
            {
                var step = Combo.Current;
                return step != null && step.Heavy && (Combo.Phase == AttackPhase.Windup || Combo.Phase == AttackPhase.Active);
            }
        }

        /// <summary>True when the swing in progress shrugs off a blow of this much damage: it still hurts, it does not interrupt.</summary>
        public bool Poised(float damage)
        {
            if (HeavyUnbroken && InHeavySwing) return true;
            var step = Combo.Current;
            return step != null && step.Poise > 0f && damage <= step.Poise
                && (Combo.Phase == AttackPhase.Windup || Combo.Phase == AttackPhase.Active);
        }

        /// <summary>
        /// A blow has reached the player. If a counter's stance is held, ready and paid for, the blow is taken on it and
        /// thrown back at once toward whoever struck: true, and the blow does nothing else.
        /// </summary>
        public bool TryCounter(in Hit hit)
        {
            if (!Combo.SkillIsCounter || !Combo.IsCharged) return false;
            if (!Combo.Counter(player.Stamina, Mana)) return false;
            Counters++;
            SkillsUsed++;
            counterDamage = hit.Damage;
            counterStagger = CounterStagger;
            lunge = 0f;
            beatsDone = 0;
            holdDone = false;
            Vector3 toward = hit.HasOrigin ? hit.Origin - transform.position : -hit.Direction;
            toward.y = 0f;
            if (toward.sqrMagnitude > 0.0025f) transform.rotation = Quaternion.LookRotation(toward.normalized, Vector3.up);
            return true;
        }

        /// <summary>The colour of what the weapon throws: the held element's, or plain stone with none.</summary>
        public Color EffectColour => element != null && element.Info != null ? element.Info.Colour : new Color(0.62f, 0.6f, 0.66f);

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
            PassingThrough = false;
            if (HeavyCriticalLeft > 0f) HeavyCriticalLeft -= deltaTime;
            if (lightAction == null || heavyAction == null) return;
            Tether(deltaTime);
            Combo.EnderAlways = boundTo != null;
            // With the weapon out of the hand there is nothing to attack with; behind a raised guard the light press is the guard's own thrust.
            var buttons = player.Buttons;
            if ((buttons != null ? buttons.Pressed(PlayerButton.LightAttack) : lightAction.WasPressedThisFrame()) && !WeaponAway)
                Combo.Press(defence != null && defence.IsGuarding, player != null && player.Dodge.IsRolling && player.Dodge.Progress >= 0.5f);
            if ((buttons != null ? buttons.Pressed(PlayerButton.HeavyAttack) : heavyAction.WasPressedThisFrame()) && !WeaponAway)
            {
                // Looked for on the press only, not every frame: a staggered enemy in reach turns the heavy into the finisher.
                finisherTarget = FindEnemy(transform.forward, 360f, FinisherRange, true);
                Combo.PressHeavy(finisherTarget != null);
            }
            if (!heavyAction.IsPressed()) Combo.ReleaseHeavy();
            if (buttons != null && buttons.Pressed(PlayerButton.WeaponSkill) && !WeaponAway && Combo.Skill != null)
            {
                // A counter's stance is raised whatever the mana (it says so when braced); a released skill asks for its price first.
                if (!Combo.SkillIsCounter && !Combo.CanPaySkill(player.Stamina, Mana))
                {
                    if (defence != null) defence.Callout(Mana.Current < Combo.SkillPrice ? "No mana" : "No stamina");
                }
                else Combo.PressSkill();
            }
            if (buttons == null || !buttons.SkillHeld) Combo.ReleaseSkill();
            Away(deltaTime);
            if (unavailable)
            {
                Combo.Idle(deltaTime);
                return;
            }

            var events = Combo.Tick(deltaTime * SwingSpeed * StatSwing, player.Stamina, Mana);
            if ((events & ComboEvents.Charged) != 0)
            {
                // The first tier flashes white; a counter says what it is.
                Vfx.Play(VfxKind.TintRing, transform.position + Vector3.up * 0.2f, Vector3.up, 1f, Color.white);
                if (Combo.SkillIsCounter && defence != null) defence.Callout(Mana.Current >= Combo.SkillPrice ? "Braced" : "No mana");
            }
            if ((events & ComboEvents.SkillReady) != 0)
            {
                if (Combo.ChargedHeavy != null) Vfx.Play(VfxKind.TintRing, transform.position + Vector3.up * 0.2f, Vector3.up, 1.4f, EffectColour);
                if (defence != null) defence.Callout("Ready");
            }
            if ((events & ComboEvents.Started) != 0)
            {
                Aim();
                beatsDone = 0;
                holdDone = false;
                passTurn = null;
                dashFor = 0f;
                stageForward = transform.forward;
                stageOrigin = transform.position;
                recoilLeft = Combo.Current.Recoil;
                cutGiven = false;
                if (Combo.Current == Combo.DashAttack) DashAttacks++;
                if (Combo.Current == Combo.Skill) SkillsUsed++;
                else
                {
                    counterDamage = 0f;
                    counterStagger = 0f;
                }
                if (Combo.Current.Effect == StepEffect.GroundSparks)
                    for (int i = 1; i <= 3; i++)
                        Vfx.Play(VfxKind.GuardSpark, transform.position + transform.forward * (0.6f * i) + Vector3.up * 0.1f, transform.forward);
            }
            if ((events & ComboEvents.Hit) != 0) Strike(Combo.Current);
            if ((events & ComboEvents.Repeat) != 0) Strike(Combo.Current, true, Combo.Current.RepeatScale);
            if (pendingShot != null)
            {
                // A shot that leaves late leaves only if the step that throws it is still being made.
                if (Combo.Current != pendingShot) pendingShot = null;
                else if (Combo.Elapsed >= pendingShot.Windup + pendingShot.Shot.ReleaseAfter)
                {
                    Loose(pendingShot);
                    pendingShot = null;
                }
            }

            var step = Combo.Current;
            if (passTurn != null && (step != passTurn || Combo.Phase == AttackPhase.Recovery))
            {
                // The passing cut is over: the player comes round to face the back of what was passed.
                if (step == passTurn) TurnToPassed();
                passTurn = null;
            }
            if (step == null) return;
            float pace = deltaTime * SwingSpeed * StatSwing;
            if (Combo.Phase == AttackPhase.Windup && step.Windup > 0f)
            {
                // The step in (or back) is spread over the wind-up, or over its first part when the step says so.
                float over = step.LungeSeconds > 0f ? Mathf.Min(step.LungeSeconds, step.Windup) : step.Windup;
                if (Combo.Elapsed <= over + 0.0001f) Step = transform.forward * (lunge * pace / over);
                Stage(step);
                if (dashFor > 0f)
                {
                    // A crossing: carried through the target to the far side, whatever stands between.
                    dashAt += pace;
                    float through = Mathf.Clamp01(dashAt / dashFor);
                    Vector3 want = Vector3.Lerp(dashFrom, dashTo, through * through * (3f - 2f * through));
                    Step = new Vector3(want.x - transform.position.x, 0f, want.z - transform.position.z);
                    PassingThrough = true;
                    if (through >= 1f)
                    {
                        dashFor = 0f;
                        // Arrived with the back to it.
                        Vector3 away = dashTo - dashCentre;
                        away.y = 0f;
                        if (away.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(away, Vector3.up);
                    }
                }
            }
            else if (Combo.Phase == AttackPhase.Active && step.Travel > 0f && step.Active > 0f)
            {
                // A blow that is carried: the player covers ground with the swing out, and it strikes all the way.
                Step = transform.forward * (step.Travel * pace / step.Active);
                Strike(step, false);
                if (step.PassThrough)
                {
                    PassingThrough = true;
                    passTurn = step;
                }
            }
            else if (Combo.Phase == AttackPhase.Recovery && step.Recoil > 0f && recoilLeft > 0f)
            {
                // Counted in metres, not in time: on a slow frame a clock would give more or less than the step says.
                float back = Mathf.Min(recoilLeft, step.Recoil * pace / RecoilSeconds);
                recoilLeft -= back;
                Step = -transform.forward * back;
            }
        }

        private const float RecoilSeconds = 0.12f;
        private float recoilLeft;

        // The chain's two ends: the bind runs out, breaks on a roll or at the end of its leash; and a pull on something
        // too heavy to come is paid a moment later by the player going to it.
        private void Tether(float deltaTime)
        {
            if (pullSelfTo != null)
            {
                pullSelfIn -= deltaTime;
                if (pullSelfIn <= 0f)
                {
                    var to = pullSelfTo;
                    pullSelfTo = null;
                    if (to.gameObject.activeInHierarchy && !player.IsLeaping && !(defence != null && defence.IsHelpless))
                    {
                        Vector3 gap = to.transform.position - transform.position;
                        gap.y = 0f;
                        float distance = gap.magnitude;
                        float run = distance - FollowStandOff - 0.4f;
                        if (run > 0.3f)
                        {
                            Vector3 along = gap / distance;
                            run = Mathf.Min(run, player.ClearRun(along, run));
                            SelfPulls++;
                            Vfx.Play(VfxKind.Dust, transform.position, -along, 1.4f);
                            player.Leap(transform.position + along * run, 0.2f, 0.45f);
                        }
                    }
                }
            }
            if (boundTo == null) return;
            boundLeft -= deltaTime;
            Vector3 away = boundTo.transform.position - transform.position;
            away.y = 0f;
            bool gone = !boundTo.gameObject.activeInHierarchy || (boundTo is Enemy enemy && enemy.IsDead);
            if (boundLeft <= 0f || gone || away.magnitude > BindLeash + 1f || (player != null && player.Dodge.IsRolling)) boundTo = null;
        }

        // The chain takes hold of something: it is held still if it can be, and the heavy button now hauls on it.
        private void Bind(Component target, float seconds)
        {
            bool held = target is IAfflictable afflictable && afflictable.Afflictions.Hold(seconds);
            boundTo = target;
            boundLeft = held ? seconds : GodBindSeconds;
            Bindings++;
            Vfx.Play(VfxKind.TintRing, target.transform.position + Vector3.up * 0.2f, Vector3.up, 1.2f, Weapon.SlashColour);
            if (defence != null) defence.Callout("Bound");
        }

        /// <summary>
        /// A blow from here was turned by a perfect guard. Some weapons do more with that: the bow leaves the arrow
        /// from its bearer's fist in the attacker; the chain whips round the attacking limb and yanks it in, off balance.
        /// </summary>
        public void PerfectGuarded(Vector3 from)
        {
            if (Weapon == null || (Weapon.PerfectGuardPlants <= 0 && Weapon.PerfectGuardPull <= 0f && Weapon.PerfectGuardStagger <= 0f)) return;
            var attacker = EnemyAt(from, 1.5f);
            if (attacker == null) return;
            GuardRiders++;
            if (arrows != null)
                for (int i = 0; i < Weapon.PerfectGuardPlants; i++) arrows.Plant(attacker, transform.forward);
            Vector3 toMe = transform.position - attacker.transform.position;
            toMe.y = 0f;
            if (Weapon.PerfectGuardPull > 0f && attacker is IShovable yanked && toMe.sqrMagnitude > 0.01f)
                yanked.Push(toMe.normalized * Mathf.Min(Weapon.PerfectGuardPull, Mathf.Max(0f, toMe.magnitude - 0.9f)));
            if (Weapon.PerfectGuardStagger > 0f)
                StrikeOne(attacker, new SpellBlow { Damage = 0f, Stagger = Weapon.PerfectGuardStagger, Effect = VfxKind.GuardSpark }, transform.position);
        }

        // After a passing cut: face the nearest thing it struck; having struck nothing, face back the way it came.
        private void TurnToPassed()
        {
            Vector3 toward = -transform.forward;
            float nearest = float.MaxValue;
            foreach (var target in struck)
            {
                if (!(target is Component body) || body == null) continue;
                Vector3 gap = body.transform.position - transform.position;
                gap.y = 0f;
                if (gap.sqrMagnitude < 0.01f || gap.sqrMagnitude >= nearest) continue;
                nearest = gap.sqrMagnitude;
                toward = gap;
            }
            transform.rotation = Quaternion.LookRotation(toward.normalized, Vector3.up);
        }

        /// <summary>True while the weapon itself is out of the hand: thrown, standing where it stuck, or on its way back. No attack can start.</summary>
        public bool WeaponAway { get; private set; }
        /// <summary>True during the run to a thrown weapon; blows pass through the player while it lasts.</summary>
        public bool IsDashing { get; private set; }
        /// <summary>For the animator and probes: times a thrown weapon came back to the hand by itself, runs made to one, and runs refused.</summary>
        public int Catches { get; private set; }
        public int FollowDashes { get; private set; }
        public int FollowsRefused { get; private set; }
        public bool IsGuardAttacking => Combo.IsGuardAttack;
        /// <summary>Metres short of an enemy the run to a weapon stuck in it stops.</summary>
        public float FollowStandOff = 1.0f;
        /// <summary>Seconds a weapon called back (not fetched) takes to reach the hand.</summary>
        public float RecallSeconds = 0.5f;

        private AttackStep pendingShot;
        private int awayShot = -1;
        private AttackStep followFor, followStep;
        private float followIn;
        private Vector3 thrownAlong;

        // The shot leaves the weapon. One that is the weapon itself takes it out of the hand until it is home again.
        private void Loose(AttackStep step)
        {
            if (step.Shot == null || shots == null) return;
            Vector3 forward = transform.forward;
            int handle = shots.Fire(this, step.Shot, (step.Shot.FromStepStart ? stageOrigin : transform.position) + forward * 0.4f, forward, EffectColour, element != null ? element.Info : null);
            if (handle < 0 || !(step.Shot.Returns || step.Shot.RaysBack || step.FollowStep != null)) return;
            WeaponAway = true;
            awayShot = handle;
            followFor = step.FollowStep != null ? step : null;
            followIn = step.FollowDelay;
            thrownAlong = forward;
        }

        // The weapon is somewhere else: wait for it to come home, or, once it has stuck, go to it.
        private void Away(float deltaTime)
        {
            if (!WeaponAway) return;
            if (IsDashing)
            {
                if (player.IsLeaping) return;
                // Arrived: the weapon is in the hands again and the body's speed goes into the cut that tears it out.
                IsDashing = false;
                WeaponAway = false;
                if (shots != null) shots.Release(awayShot);
                Vfx.Play(VfxKind.Dust, transform.position, -transform.forward, 1.6f);
                if (followStep != null && !(defence != null && defence.IsHelpless) && Combo.Follow(followStep, player.Stamina))
                {
                    lunge = 0f;
                    beatsDone = 0;
                    holdDone = false;
                    counterDamage = 0f;
                    counterStagger = 0f;
                }
                followStep = null;
                return;
            }
            if (shots == null || !shots.IsOut(awayShot))
            {
                // It came back by itself: caught.
                WeaponAway = false;
                followFor = null;
                Catches++;
                if (hitStop != null) hitStop.Freeze(0.03f);
                if (shaker != null) shaker.Shake(0.08f, 0.1f);
                return;
            }
            if (followFor == null || !shots.IsStuck(awayShot)) return;
            followIn -= deltaTime;
            if (followIn > 0f || Combo.Phase == AttackPhase.Windup || Combo.Phase == AttackPhase.Active) return;
            var step = followFor;
            followFor = null;
            // The stick held away from the throw refuses the run, and so does a body that cannot make it: the weapon comes back instead.
            Vector3 wish = moveAction != null ? player.CameraRelative(moveAction.ReadValue<Vector2>()) : Vector3.zero;
            bool refused = wish.sqrMagnitude > 0.25f && Vector3.Dot(wish.normalized, thrownAlong) < -0.5f;
            if (refused || (defence != null && defence.IsHelpless) || player.FormHold != 0)
            {
                FollowsRefused++;
                shots.Recall(awayShot, RecallSeconds);
                return;
            }
            Vector3 gap = shots.Where(awayShot) - transform.position;
            gap.y = 0f;
            float distance = gap.magnitude;
            float run = Mathf.Max(0f, distance - (shots.StuckInEnemy(awayShot) ? FollowStandOff : 0f));
            Vector3 along = distance > 0.05f ? gap / distance : transform.forward;
            run = Mathf.Min(run, player.ClearRun(along, run));
            followStep = step.FollowStep;
            IsDashing = true;
            FollowDashes++;
            Vfx.Play(VfxKind.Dust, transform.position, -along, 1.4f);
            player.Leap(transform.position + along * run, step.FollowSeconds, 0f);
        }

        // A finisher's staging before its blow: the beats that stop time and spark, and the lean of the camera for the hold.
        private void Stage(AttackStep step)
        {
            float at = Combo.Elapsed;
            Transform target = Combo.IsFinishing ? finisherTarget : null;
            Vector3 where = target != null ? target.position + Vector3.up * 1.1f : transform.position + transform.forward * 1.2f + Vector3.up * 1.1f;
            while (step.Beats != null && beatsDone < step.Beats.Length && at >= step.Beats[beatsDone].Time)
            {
                var beat = step.Beats[beatsDone++];
                Vfx.Play(beat.Effect, where, transform.forward);
                if (Weapon.ElementColoured) Vfx.Play(VfxKind.TintSpark, where, transform.forward, 0.6f, EffectColour);
                if (hitStop != null && beat.HitStopSeconds > 0f) hitStop.Freeze(beat.HitStopSeconds);
                if (shaker != null && beat.Shake > 0f) shaker.Shake(beat.Shake, 0.1f);
                if (beat.DashBeyond > 0f)
                {
                    dashCentre = target != null ? target.position : transform.position + stageForward * 1.5f;
                    dashCentre.y = transform.position.y;
                    dashFrom = transform.position;
                    Vector3 bearing = Quaternion.Euler(0f, beat.DashBearing, 0f) * stageForward;
                    Vector3 line = dashCentre + bearing * beat.DashBeyond - dashFrom;
                    float run = player != null ? player.ClearRun(line, line.magnitude) : line.magnitude;
                    dashTo = dashFrom + line.normalized * run;
                    dashFor = Mathf.Max(0.02f, beat.DashSeconds);
                    dashAt = 0f;
                    if (line.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(line.normalized, Vector3.up);
                    // The line it leaves, through the target, hanging until the blow sets it off.
                    Color white = new Color(Weapon.SlashColour.r * 1.6f, Weapon.SlashColour.g * 1.6f, Weapon.SlashColour.b * 1.6f, 0.95f);
                    FormFx.Cut(dashCentre + Vector3.up * 1.1f, line, run + 0.6f, 0.34f, white, Mathf.Max(0.1f, step.Windup - beat.Time) / Mathf.Max(0.2f, SwingSpeed * StatSwing) + step.HoldSeconds / HoldSlowScale + 0.12f, 0.05f);
                    Vfx.Play(VfxKind.Dust, dashFrom, -line.normalized, 1.2f);
                }
                if (target == null) continue;
                // What can be moved is moved; a god stays where it is and the body plays the same against it.
                if (beat.Shove.sqrMagnitude > 0f && target.GetComponentInParent<IShovable>() is IShovable shovable)
                    shovable.Push(transform.right * beat.Shove.x + transform.forward * beat.Shove.z);
                if (beat.Lift > 0f && target.GetComponentInParent<IAfflictable>() is IAfflictable liftable)
                    liftable.Afflictions.Lift(beat.Lift);
            }
            if (step.HoldAt > 0f && !holdDone && at >= step.HoldAt)
            {
                // The held pose: time runs slow and the camera comes in before the blow, not only after it.
                holdDone = true;
                if (hitStop != null) hitStop.Slow(step.HoldSeconds / HoldSlowScale, HoldSlowScale);
                if (shaker != null && FinisherCameraDistance < 1f && target != null)
                {
                    Vector3 between = (target.position - transform.position) * 0.5f;
                    between.y = 0f;
                    shaker.CloseIn(FinisherCameraDistance, Vector3.ClampMagnitude(between, 2f), step.HoldSeconds / HoldSlowScale + 0.2f);
                }
            }
        }

        /// <summary>How slowly time runs through a finisher's held pose.</summary>
        public float HoldSlowScale = 0.4f;

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
        /// <summary>
        /// False for what pays nothing back for a blow (the Sun-Bearer's loose sun): no mana, no flask earned, no grey health
        /// won back, nothing for the upgrades that feed on blows.
        /// </summary>
        public static bool PaysBack(IDamageable target) => !(target is Clover.Gameplay.IPaysNothing nothing && nothing.PaysNothing);

        private Hit Make(in SpellBlow blow, IDamageable target, Vector3 direction, Vector3 origin)
        {
            float damage = blow.Damage;
            var carrier = target as IAfflictable;
            if (StatAfflicted != 1f && carrier != null && carrier.Afflictions.Any) damage *= StatAfflicted;
            if (Stats != null && carrier != null) damage *= Mathf.Max(0.1f, 1f + Stats.Against(PlayerStat.SpellDamage, carrier.Afflictions));
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
            if (defence != null && PaysBack(target)) defence.DealtDamage(blow.Damage);
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

        public int StrikeArea(Vector3 centre, float radius, in SpellBlow blow, HashSet<IDamageable> once = null) =>
            StrikeArea(centre, radius, blow, once, 0f);

        /// <summary>The same for a band: whoever stands nearer the middle than <paramref name="innerRadius"/> is left alone.</summary>
        public int StrikeArea(Vector3 centre, float radius, in SpellBlow blow, HashSet<IDamageable> once, float innerRadius)
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
                if (innerRadius > 0f && away.magnitude < innerRadius) continue;
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
        /// Strikes everything standing in a lane on the ground: from a point, along a direction, this long and this far
        /// to either side. What a beam, a dragon's lunge or a line of spikes does. Returns how many it hit.
        /// </summary>
        public int StrikeLane(Vector3 from, Vector3 direction, float length, float halfWidth, in SpellBlow blow, HashSet<IDamageable> once = null)
        {
            spellStruck.Clear();
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f || length <= 0f) return 0;
            direction.Normalize();
            Vector3 middle = from + direction * (length * 0.5f);
            int count = Overlap(middle + Vector3.up, length * 0.5f + halfWidth + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || (once != null && once.Contains(target))) continue;
                // The nearest point of it to the lane's own middle line decides.
                Vector3 offset = other.bounds.center - from;
                offset.y = 0f;
                float along = Mathf.Clamp(Vector3.Dot(offset, direction), 0f, length);
                Vector3 onLine = from + direction * along;
                Vector3 gap = other.ClosestPoint(onLine + Vector3.up) - onLine;
                gap.y = 0f;
                if (gap.magnitude > halfWidth) continue;
                bool wasAfflicted = target is IAfflictable afflictable && afflictable.Afflictions.Any;
                var outcome = target.TakeHit(Make(blow, target, direction, from));
                if (outcome == HitOutcome.Missed) continue;
                spellStruck.Add(target);
                if (once != null) once.Add(target);
                Landed(blow, target, other, direction, wasAfflicted);
            }
            return spellStruck.Count;
        }

        /// <summary>Strikes one thing and nothing else, from a place: a bullet, a helper's jab, a cut that was owed. True when it landed.</summary>
        public bool StrikeOne(Component victim, in SpellBlow blow, Vector3 from)
        {
            if (victim == null || !victim.gameObject.activeInHierarchy || !(victim is IDamageable target)) return false;
            Vector3 direction = victim.transform.position - from;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward;
            bool wasAfflicted = target is IAfflictable afflictable && afflictable.Afflictions.Any;
            var outcome = target.TakeHit(Make(blow, target, direction, from));
            if (outcome == HitOutcome.Missed) return false;
            Vector3 at = victim.transform.position + Vector3.up * 1.1f;
            if (blow.Tinted) Vfx.Play(blow.Effect, at, direction, 1f, blow.Tint);
            else Vfx.Play(blow.Effect, at, direction);
            if (defence != null && PaysBack(target)) defence.DealtDamage(blow.Damage);
            if (blow.Spell && upgrades != null) upgrades.OnSpellStruck(target, wasAfflicted);
            return true;
        }

        /// <summary>The enemy standing nearest a point within a radius of it that is not on the list given, or null.</summary>
        public Component EnemyNear(Vector3 point, float radius, HashSet<IDamageable> except)
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
                if (target == null || (except != null && except.Contains((IDamageable)target))) continue;
                float distance = (target.transform.position - point).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                found = target;
            }
            return found;
        }

        /// <summary>Shoves everything in a circle that can be shoved, all the same way: what carries enemies along with something. Returns how many it moved.</summary>
        public int ShoveArea(Vector3 centre, float radius, Vector3 impulse)
        {
            impulse *= StatPush;
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
                if (offset.magnitude > radius) continue;
                spellStruck.Add(target);
                shovable.Push(impulse);
            }
            return spellStruck.Count;
        }

        /// <summary>
        /// Lays an affliction on everything standing in a circle, without a blow: what a Soaked enemy does with the
        /// Static laid on it, and what an upgrade that afflicts all around does. Returns how many it reached.
        /// </summary>
        /// <summary>How far the player's pushes throw, as a share of the usual. The upgrades keep it.</summary>
        public float StatPush = 1f;

        /// <summary>Makes the poison in everything within a circle bite on its next step. Returns how many carried any.</summary>
        public int BiteArea(Vector3 centre, float radius)
        {
            spellStruck.Clear();
            int bitten = 0;
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || !(target is IAfflictable afflictable)) continue;
                Vector3 offset = ((Component)target).transform.position - centre;
                offset.y = 0f;
                if (offset.magnitude > radius) continue;
                spellStruck.Add(target);
                if (afflictable.Afflictions.BiteSoon()) bitten++;
            }
            return bitten;
        }

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

        /// <summary>Throws everything in a circle into the air for a time (longer with the player's own numbers). Returns how many left the ground.</summary>
        public int LiftArea(Vector3 centre, float radius, float seconds)
        {
            spellStruck.Clear();
            int lifted = 0;
            if (Stats != null) seconds += Stats.Sum(PlayerStat.AirSeconds);
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || !(target is IAfflictable afflictable)) continue;
                Vector3 offset = ((Component)target).transform.position - centre;
                offset.y = 0f;
                if (offset.magnitude > radius) continue;
                spellStruck.Add(target);
                if (afflictable.Afflictions.Lift(seconds)) lifted++;
            }
            return lifted;
        }

        /// <summary>Holds everything in a circle still for a time. Returns how many it took hold of.</summary>
        public int HoldArea(Vector3 centre, float radius, float seconds)
        {
            spellStruck.Clear();
            int held = 0;
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || !(target is IAfflictable afflictable)) continue;
                Vector3 offset = ((Component)target).transform.position - centre;
                offset.y = 0f;
                if (offset.magnitude > radius) continue;
                spellStruck.Add(target);
                if (afflictable.Afflictions.Hold(seconds)) held++;
            }
            return held;
        }

        /// <summary>Drags everything in a circle toward its middle, or throws it away from there, by about this many metres. Returns how many it moved.</summary>
        public int Displace(Vector3 centre, float radius, float metres, bool pull)
        {
            if (!pull) metres *= StatPush;
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
                    if (defence != null && PaysBack(target)) defence.DealtDamage(blow.Damage);
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
        public Transform AimTarget() => moveAction == null ? null : FindEnemy(AimDirection(), Weapon != null && Weapon.AimConeDegrees > 0f ? Weapon.AimConeDegrees : AimConeDegrees,
            Weapon != null && Weapon.AimRange > 0f ? Weapon.AimRange : AimRange, false);

        private Vector3 AimDirection()
        {
            // With the mouse in use the cursor says where; the nearest enemy near it is still the one turned to.
            if (player.PointerAim(out Vector3 pointed)) return pointed;
            Vector3 wish = player.CameraRelative(moveAction.ReadValue<Vector2>());
            return wish.sqrMagnitude > 0.01f ? wish.normalized : transform.forward;
        }

        // Face the enemy nearest to where the stick (or the cursor) points; with no enemy in the cone, face that way; with neither, stay.
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
                // What the chain holds is what the haul is aimed at; a bow looks further off than a blade does.
                enemy = boundTo != null && Combo.Current == Combo.ChainHeavy ? boundTo.transform
                    : FindEnemy(toward, Weapon.AimConeDegrees > 0f ? Weapon.AimConeDegrees : AimConeDegrees, Weapon.AimRange > 0f ? Weapon.AimRange : AimRange, false);
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

        // What a blow sets going besides itself: a shot off the blade, spikes out of the ground, the element thrown as a cone or a ring.
        private void Thrown(AttackStep step, bool finisher, Vector3 finished)
        {
            var held = element != null ? element.Info : null;
            Color colour = EffectColour;
            Vector3 forward = transform.forward;
            if (step.Shot != null && shots != null)
            {
                if (step.Shot.ReleaseAfter > 0f) pendingShot = step;
                else Loose(step);
            }
            switch (step.Effect)
            {
                case StepEffect.GroundSlam:
                    // Where the head came down, or where a finished body did.
                    Vector3 spot = (finisher ? new Vector3(finished.x, transform.position.y, finished.z) : transform.position + forward * step.Reach) + Vector3.up * 0.15f;
                    Vfx.Play(VfxKind.Dust, spot, Vector3.up, 2f + step.Radius * 0.6f);
                    Vfx.Play(VfxKind.TintRing, spot, Vector3.up, Mathf.Max(1.2f, step.Radius), colour);
                    break;
                case StepEffect.SpikeLine:
                    // Eight spikes run out along a lane, each enemy struck once by whichever reaches it first.
                    if (spikes != null)
                        spikes.Run(this, transform.position + forward * 1.2f, forward, 8, 1.25f, 0.09f, 0f, colour, true, new SpellBlow
                        {
                            Damage = 34f, Stagger = 45f, Heavy = true, Effect = held != null ? ElementLook.HitEffect(held) : VfxKind.Impact,
                            Affliction = held != null ? held.Affliction : AfflictionKind.None, Stacks = held != null ? 1 : 0,
                            Tinted = held != null && ElementLook.Tints(held), Tint = colour,
                        });
                    break;
                case StepEffect.GroundSplit:
                    // Seen only: the ground opens along the line of the cut, a moment after it, running on past the fallen.
                    if (finisher && spikes != null)
                    {
                        Vector3 along = Quaternion.Euler(0f, step.EffectTurn, 0f) * forward;
                        spikes.Run(this, new Vector3(finished.x, transform.position.y, finished.z) + along * 0.9f, along, 6, 1.0f, 0.04f, 0.35f, colour, false, default);
                    }
                    break;
                case StepEffect.ElementCone:
                    for (int i = 1; i <= 4; i++)
                    {
                        Vector3 at = transform.position + Vector3.up * 1.1f + forward * (step.Reach * i / 4f);
                        Vfx.Play(VfxKind.TintSpark, at, forward, 0.6f + 0.25f * i, colour);
                        Vfx.Play(VfxKind.TintPuff, at, forward, 0.5f + 0.3f * i, colour);
                    }
                    if (shaker != null) ScreenFlash.Flash(new Color(colour.r, colour.g, colour.b, 0.18f), 0.06f);
                    // A finisher's cone goes through and out of the back: it lays the element on what stands behind.
                    if (finisher && held != null && held.Affliction != AfflictionKind.None)
                        AfflictArea(finished + forward * 2.2f, 2f, held.Affliction, 1);
                    break;
                case StepEffect.ArrowRain:
                    // Five arrows come down in a ring round the enemy faced, or round a spot five metres ahead.
                    if (arrows != null)
                    {
                        Transform faced = EnemyAhead(9f);
                        Vector3 middle = faced != null ? faced.position : transform.position + forward * 5f;
                        middle.y = transform.position.y;
                        arrows.Rain(middle, forward, 5, 1.25f, 0.5f, 8f, 8f, 0.8f, 10f);
                    }
                    break;
                case StepEffect.ElementRing:
                    Vector3 centre = transform.position + forward * step.Reach + Vector3.up * 0.15f;
                    Vfx.Play(VfxKind.TintRing, centre, Vector3.up, Mathf.Max(1f, step.Radius), colour);
                    Vfx.Play(VfxKind.Dust, centre, Vector3.up, 2f);
                    Vfx.Play(VfxKind.TintSpark, centre, Vector3.up, 1.5f, colour);
                    break;
            }
        }

        // The way the blade travels through what it strikes: across the body for a sweeping cut, on the side it is
        // swung from to the other, leaning into the target; straight in for a thrust or a blow with no sweep.
        private Vector3 CutDirection(AttackStep step)
        {
            if (Mathf.Abs(step.Sweep) < 0.01f) return transform.forward;
            return (-transform.right * Mathf.Sign(step.Sweep) + transform.forward * 0.45f).normalized;
        }

        // One blow of the weapon. A fresh blow starts a new list of who has been struck and shows its arc; one that is
        // not (a carried blow, struck again each frame) only looks for anyone new. Scale is for the repeats of a step.
        /// <summary>How far round the target the player must be for a blow to count as on its back: the dot of its facing with the way to the player, under this.</summary>
        public float BackDot = -0.3f;

        private void Strike(AttackStep step, bool fresh = true, float scale = 1f)
        {
            if (fresh) struck.Clear();
            int already = struck.Count;
            bool finisher = false;
            bool clean = false;
            bool paidBlow = false;
            Vector3 finished = transform.position;
            Color arcColour = empowered ? empoweredColour : Weapon.ElementColoured ? new Color(EffectColour.r * 2f, EffectColour.g * 2f, EffectColour.b * 2f, 0.9f) : Weapon.SlashColour;
            if (fresh && arcs != null)
            {
                if (step.Radius > 0f) Vfx.Play(VfxKind.TintRing, transform.position + transform.forward * step.Reach + Vector3.up * 0.15f, Vector3.up, step.Radius * 0.6f, arcColour);
                else if (step.LineWidth > 0f) arcs.Show(transform.position, transform.forward, step.Reach, 20f, 0f, arcColour);
                else
                {
                    // A weapon with a tip shows where the tip is: the haft's part of the fan dim, the last metre bright.
                    if (step.SweetFrom > 0f) arcs.Show(transform.position, transform.forward, step.SweetFrom, step.ArcDegrees, step.Sweep, new Color(arcColour.r * 0.45f, arcColour.g * 0.45f, arcColour.b * 0.45f, arcColour.a * 0.5f));
                    arcs.Show(transform.position, transform.forward, step.Reach, step.ArcDegrees, step.Sweep, step.SweetFrom > 0f ? new Color(arcColour.r * 1.4f, arcColour.g * 1.4f, arcColour.b * 1.4f, arcColour.a) : arcColour);
                }
            }
            Vector3 origin = transform.position + Vector3.up;
            Vector3 discCentre = origin + transform.forward * step.Reach;
            var held = element != null ? element.Info : null;
            // The haul on a bound enemy reaches it wherever on its leash it stands.
            float reach = boundTo != null && step == Combo.ChainHeavy ? Mathf.Max(step.Reach, BindLeash + 1f) : step.Reach;
            Component nearest = null;
            float nearestAt = float.MaxValue;
            int count = Overlap(step.Radius > 0f ? discCentre : origin, step.Radius > 0f ? step.Radius + 0.5f : reach);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || struck.Contains(target)) continue;
                // A blade does not reach an enemy on the floor far below or the ledge far above (the gods, whose
                // parts stand high off the ground, are struck as they always were).
                if (target is Enemy below && !below.Kind.IsBoss && !HeightReach.Within(transform.position.y, below.transform.position.y)) continue;
                Vector3 offset = other.ClosestPoint(origin) - origin;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (step.Radius > 0f)
                {
                    // A disc on the ground ahead: whoever stands in it, and no one nearer.
                    Vector3 fromCentre = other.ClosestPoint(discCentre) - discCentre;
                    fromCentre.y = 0f;
                    if (fromCentre.magnitude > step.Radius) continue;
                }
                else if (step.LineWidth > 0f)
                {
                    // A lane straight ahead.
                    float along = Vector3.Dot(offset, transform.forward);
                    if (along < -0.3f || along > reach) continue;
                    if ((offset - transform.forward * along).magnitude > step.LineWidth * 0.5f) continue;
                }
                // Something touching the player is hit whatever the angle; otherwise it must be inside the swing's arc.
                else if (distance > 0.6f && Vector3.Angle(transform.forward, offset) > step.ArcDegrees * 0.5f) continue;
                var afflictable = target as IAfflictable;
                // A blow is critical by chance, or always on an enemy that is Marked.
                bool promised = step.Heavy && HeavyCriticalLeft > 0f;
                float chance = CritChance;
                if (Stats != null) chance += Stats.Blow(PlayerStat.CritChance, step.Heavy, false) + (afflictable != null ? Stats.Against(PlayerStat.CritChance, afflictable.Afflictions) : 0f);
                // Counter Cut: a blow that lands on an enemy showing its own attack (winding up or striking) is critical.
                bool countered = CounterCut && target is Enemy showing && showing.IsStriking;
                if (countered) CounterCuts++;
                bool critical = promised || countered || (afflictable != null && afflictable.Afflictions.MakesBlowsCritical) || (chance > 0f && Random.value < chance);
                bool counter = step == Combo.Skill && Combo.SkillIsCounter;
                float damage = (step.Damage + (counter ? counterDamage : 0f)) * scale * step.BandDamage(distance) * DamageMultiplier * UpgradeDamage * NextBlowDamage * StatDamage;
                float staggerShare = 1f;
                if (Stats != null)
                {
                    // What holds for this kind of blow, or against what this target carries, on top of the standing sums.
                    var carried = afflictable != null ? afflictable.Afflictions : null;
                    var light = Combo.Steps;
                    bool endsChain = step == Combo.ChainHeavy || (light.Count > 0 && step == light[light.Count - 1]);
                    bool inAir = carried != null && carried.IsAirborne;
                    damage *= Mathf.Max(0.1f, 1f + Stats.Blow(PlayerStat.WeaponDamage, step.Heavy, critical, endsChain) + Stats.Against(PlayerStat.WeaponDamage, carried) + (inAir ? Stats.InAir(PlayerStat.WeaponDamage) : 0f));
                    staggerShare = Mathf.Max(0.1f, 1f + Stats.Blow(PlayerStat.StaggerDealt, step.Heavy, critical, endsChain) + Stats.Against(PlayerStat.StaggerDealt, carried) + (inAir ? Stats.InAir(PlayerStat.StaggerDealt) : 0f));
                }
                if (critical) damage *= CritMultiplier;
                // A back is a back: the knives' blows count for more on it. The finisher has its own number.
                bool fromBehind = false;
                if (Weapon.BackMultiplier != 1f && !Combo.IsFinishing && target is Component back)
                {
                    Vector3 toMe = transform.position - back.transform.position;
                    toMe.y = 0f;
                    fromBehind = toMe.sqrMagnitude > 0.01f && Vector3.Dot(back.transform.forward, toMe.normalized) < BackDot;
                    if (fromBehind) damage *= Weapon.BackMultiplier;
                }
                bool wasAfflicted = afflictable != null && afflictable.Afflictions.Any;
                if (wasAfflicted) damage *= StatAfflicted;
                float burn = (BurnStacksOnHit > 0 ? Mathf.Max(BurnOnHit, StackBurnSeconds) : BurnOnHit) * AfflictionSeconds;
                // What the root of the tree and the surge lay, together, in the element's own affliction.
                int stacks = WeaponAfflictionStacks + EmpowerStacks;
                var kind = stacks <= 0 ? AfflictionKind.None : WeaponAfflictionStacks > 0 ? WeaponAffliction : EmpowerAffliction;
                if (kind != AfflictionKind.None) stacks += ExtraStacks;
                int burnStacks = BurnStacksOnHit > 0 ? BurnStacksOnHit + ExtraStacks : 0;
                if (step.ElementStacks > 0 && held != null && held.Affliction != AfflictionKind.None)
                {
                    // A blow that carries the held element by itself (the Armament's coat), whatever the tree has taught.
                    if (held.Affliction == AfflictionKind.Burn)
                    {
                        burnStacks += step.ElementStacks;
                        burn = Mathf.Max(burn, StackBurnSeconds * AfflictionSeconds);
                    }
                    else if (kind == AfflictionKind.None)
                    {
                        kind = held.Affliction;
                        stacks = step.ElementStacks + ExtraStacks;
                    }
                    else if (kind == held.Affliction) stacks += step.ElementStacks;
                }
                float lasts = kind != AfflictionKind.None && AfflictionSeconds != 1f ? Clover.Gameplay.Afflictions.Spec(kind).Seconds * AfflictionSeconds : 0f;
                float dealtStagger = (step.Stagger + (counter ? counterStagger : 0f) + StaggerOnHit) * NextBlowStagger * StatStagger * staggerShare * scale * step.BandStagger(distance);
                if (fromBehind) dealtStagger *= Weapon.BackMultiplier;
                if (boundTo != null && ReferenceEquals(target, boundTo)) dealtStagger *= BoundStagger;
                var outcome = target.TakeHit(new Hit(damage, dealtStagger, transform.forward, step.Heavy, false, transform.position,
                    burn, burnStacks, false, kind, kind != AfflictionKind.None ? stacks : 0, lasts, AfflictionPower, false, critical));
                if (outcome == HitOutcome.Missed) continue;
                struck.Add(target);
                LastBlowDamage = damage;
                if (fromBehind && outcome != HitOutcome.Blocked)
                {
                    BackBlows++;
                    Vfx.Play(VfxKind.TintSpark, other.bounds.center, transform.forward, 0.9f, Weapon.SlashColour);
                }
                LastBlowStagger = dealtStagger;
                LastBlowDistance = distance;
                if (step.IsSweet(distance) && outcome != HitOutcome.Blocked)
                {
                    // The tip: a white star where it bit, so the right distance can be learned by eye.
                    SweetBlows++;
                    Vfx.Play(VfxKind.TintSpark, other.bounds.center, transform.forward, 1.2f, Color.white);
                }
                if (outcome != HitOutcome.Blocked && distance < nearestAt && target is Component nearBody)
                {
                    nearestAt = distance;
                    nearest = nearBody;
                }
                if (step.Pull != 0f && outcome != HitOutcome.Blocked && target is IShovable dragged)
                {
                    // What is too heavy to come is not moved: the one who pulled goes to it instead.
                    if (step.PullSelf && target is IWeighted weighed && weighed.Weight >= HeavyWeight) pullSelfTo = target as Component;
                    // A drag never brings its target past the feet of whoever drags.
                    else dragged.Push(transform.forward * -(step.Pull > 0f ? Mathf.Min(step.Pull, Mathf.Max(0f, distance - 0.9f)) : step.Pull));
                }
                if (outcome != HitOutcome.Blocked && arrows != null && target is Component host)
                {
                    // The stave snaps what stands in the body; the arrow in the fist is left there after.
                    if (step.SnapsArrows) arrows.Snap(host, outcome == HitOutcome.Finisher ? 0.08f : 0.05f);
                    for (int planted = 0; planted < step.PlantsArrows; planted++) arrows.Plant(host, transform.forward);
                }
                if (step.Fling != 0f && outcome == HitOutcome.Finisher && target is IShovable flung)
                    flung.Push((-transform.right * (step.Sweep < 0f ? -1f : 1f) + transform.forward * 0.25f).normalized * step.Fling);
                LastBlowCritical = critical;
                bool pays = PaysBack(target);
                float earned = pays ? step.ManaGain * StatMana : 0f;
                if (Stats != null && afflictable != null) earned *= Mathf.Max(0.1f, 1f + Stats.Against(PlayerStat.ManaFromBlows, afflictable.Afflictions) + (afflictable.Afflictions.IsAirborne ? Stats.InAir(PlayerStat.ManaFromBlows) : 0f));
                if (outcome != HitOutcome.Blocked && pays)
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
                        // A mark of light carries a few critical blows and is spent by them.
                        afflictable.Afflictions.BlowLanded();
                        // A blow that reaches an enemy in the air holds it up a little longer: three times, or more for one who has learned to keep it there.
                        int more = Stats != null ? Mathf.Max(0, Stats.Whole(PlayerStat.AirBlows)) : 0;
                        afflictable.Afflictions.AirBlow(AirBlowSeconds, AirBlowsMost + more, more > 0 ? 0.85f : 1f);
                    }
                }
                if (upgrades != null && pays) upgrades.OnBlow(step, target, outcome, wasAfflicted);
                if (pays && outcome != HitOutcome.Blocked) paidBlow = true;
                // Spells that wait on the weapon (a brand, a copy of the player, blades at the shoulder) are told; they answer on their own next step.
                if (outcome != HitOutcome.Blocked && pays)
                {
                    if (promised) HeavyCriticalLeft = 0f;
                    Clover.Player.Forms.SpellWorks.Blow(step, target as Component, damage);
                }
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
                if (defence != null && pays) defence.DealtDamage(damage);
            }
            if (fresh)
            {
                if (upgrades != null) upgrades.OnSwing(step, struck.Count);
                Clover.Player.Forms.SpellWorks.Swing(step, transform.position, transform.forward);
                if (step.Binds > 0f && nearest != null) Bind(nearest, step.Binds);
                if (pullSelfTo != null) pullSelfIn = 0.1f;
                Thrown(step, finisher, finished);
                counterDamage = 0f;
                counterStagger = 0f;
            }
            if (struck.Count == already) return;
            if (paidBlow)
            {
                // The spells' clocks: a finisher takes its own, larger cut; any other move cuts once however many it struck.
                if (finisher) Finished?.Invoke();
                else if (!cutGiven)
                {
                    cutGiven = true;
                    BlowLanded?.Invoke(step);
                }
            }
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
