using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player.Forms;
using Clover.Player.Magic;
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
        // Until when the next light blow lands as a heavy one (UpgradeAction.NextHeavy); 0 for none.
        private float heavyNextUntil;
        /// <summary>The next light weapon blow within <paramref name="seconds"/> lands as a heavy blow (Gone to Ash's first blow behind the attacker).</summary>
        public void MakeNextBlowHeavy(float seconds) => heavyNextUntil = Time.time + Mathf.Max(0.05f, seconds);
        /// <summary>True while the next light blow would land as a heavy one.</summary>
        public bool NextBlowHeavy => MadeHeavy(false, heavyNextUntil, Time.time);

        /// <summary>True when a light blow is made heavy: it is not heavy already, and a mark's window for it runs at <paramref name="now"/>.</summary>
        public static bool MadeHeavy(bool stepHeavy, float until, float now) => !stepHeavy && now < until;

        /// <summary>The stagger a light blow made heavy breaks: the weapon's heavy blow's, never less than its own.</summary>
        public static float HeavyStagger(float light, float heavy) => Mathf.Max(light, heavy);

        // How much longer the marks in force make the weapon's reach (PlayerStat.WeaponReach).
        private float WeaponReachScale => Stats != null ? Stats.Multiplier(PlayerStat.WeaponReach) : 1f;
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
        /// <summary>Poise the Body adds to every swing of every weapon (Steady, task 5.4): flat, on top of the swing's own.</summary>
        public float StatPoise;
        /// <summary>What damage to an enemy that carries any affliction is multiplied by.</summary>
        public float StatAfflicted = 1f;
        /// <summary>The Blade Drinks Fire: damage added to a weapon blow for each Burn it takes off the enemy struck; 0 for none.</summary>
        [System.NonSerialized] public float DrinksBurn;
        /// <summary>Burns the blade has drunk, counted for probes.</summary>
        public int BurnsDrunk { get; private set; }
        /// <summary>Shared Roots: blows on a Seeded enemy no longer mend the player.</summary>
        [System.NonSerialized] public bool SeedMendingOff;
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

        /// <summary>Stagger the next heavy blow that lands carries on top of its own, and the seconds left for it (Mountain Stance's load).</summary>
        public float HeavyStaggerLoaded { get; private set; }
        private float heavyLoadLeft;

        /// <summary>The next heavy blow to land within <paramref name="seconds"/> carries <paramref name="stagger"/> more.</summary>
        public void LoadNextHeavy(float stagger, float seconds)
        {
            HeavyStaggerLoaded = Mathf.Max(0f, stagger);
            heavyLoadLeft = HeavyStaggerLoaded > 0f ? seconds : 0f;
        }

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
        /// <summary>A move was swung (struck or not): the charged blade's look draws its trail on it.</summary>
        public event System.Action<AttackStep> Swung;
        /// <summary>A blow of the weapon met a body (not a guard): where on it, and the way the blow was going. The swing looks throw their element there.</summary>
        public event System.Action<Vector3, Vector3> BladeMet;

        /// <summary>The colour of the star a blow's sweet spot throws.</summary>
        public static readonly Color SweetStar = new Color(1f, 0.78f, 0.38f);
        private bool cutGiven;

        /// <summary>
        /// The charged blade (IMPLEMENTATION.md 3.3): a slot spell that lands charges it (<see cref="PlayerSpells"/> does),
        /// and the next three moves that land carry its element, 20% harder. Spent here, in the blow path.
        /// </summary>
        public ChargedBlade Blade = new ChargedBlade();
        /// <summary>The technique chord, R1 and R2 together (IMPLEMENTATION.md 3.5). It cancels a light still in its first moment into <see cref="TechniqueStep"/>.</summary>
        public ChordRules Chord = new ChordRules();
        /// <summary>
        /// What the chord plays. Null, as it is until the gated techniques exist (task 5.3): the chord is then not taken
        /// and the heavy press goes on as a heavy, so nothing a player does today changes.
        /// </summary>
        [System.NonSerialized] public AttackStep TechniqueStep;
        /// <summary>Seconds an infused strike's press is remembered while the player is busy.</summary>
        public float StrikeBufferTime = 0.25f;
        /// <summary>Counts for probes: moves that spent a charge, infused and deep strikes started, chords taken, strikes refused.</summary>
        public int ChargedBlows { get; private set; }
        public int InfusedStrikes { get; private set; }
        public int DeepStrikes { get; private set; }
        public int ChordsTaken { get; private set; }
        public int StrikesRefused { get; private set; }
        /// <summary>The ground the last infused strike's carrier covered.</summary>
        public Footprint LastFootprint { get; private set; }
        /// <summary>True while an infused strike's carrier is being made.</summary>
        public bool InfusedNow => infusedStep != null && Combo.Current == infusedStep;

        /// <summary>
        /// The element the move in progress carries on the weapon: the infused strike's, or the charged blade's when the
        /// move began with it charged (for the whole move, though its charge is spent on the way); null for a plain move.
        /// </summary>
        public ElementInfo SwingElement => Combo.Current == null ? null : InfusedNow && infusedElement != null ? infusedElement : moveCharged ? moveElement : null;

        /// <summary>
        /// The colour a landed blow of the move in progress marks what it struck with: its element's trail colour when the
        /// weapon carries one (<see cref="SwingElement"/>), so the flash and the cut streak are tinted and softened and the
        /// trail shows through (the owner's choice, 2026-11-19); clear for a plain move, which keeps its white.
        /// </summary>
        /// <summary>What a heavy blow's star is lit to under an element: its material glows six times past white.</summary>
        public const float ImpactGlow = 0.22f;

        /// <summary>
        /// A critical blow's gold, and what its star and its spray of sparks are lit to: both are drawn added on top of
        /// each other, so lower than a heavy blow's single star, or where they cross they still burn pure white (Light's
        /// films of 2026-11-21: at 0.22 and 0.35 a Marked group still showed 11,000 white pixels).
        /// </summary>
        public static readonly Color CriticalGold = new Color(1f, 0.72f, 0.2f);
        public const float CriticalGlow = 0.12f, CriticalSparkGlow = 0.15f;

        public Color StruckTint
        {
            get
            {
                var look = Clover.Player.Swings.SwingLooks.For(SwingElement);
                return look != null ? look.Tint : default;
            }
        }

        /// <summary>
        /// The tint of the cut across a struck body when the blow carries no element look of its own: the carried element's
        /// colour, or with none the weapon's slash colour; its hue at full strength, never a bright HDR glow.
        /// </summary>
        public Color WeaponCutTint
        {
            get
            {
                if (Weapon == null) return default;
                return CutTintOf(Weapon, element != null && element.HasElement ? element.Info : null);
            }
        }

        /// <summary>
        /// The tint every blow of the weapon in hand lands with: the element look's when the move carries one, else the
        /// weapon's own cut tint, so no blow of the player's (a swing, a shot, a shadow struck) flashes a body white.
        /// </summary>
        public Color BlowTint => BlowTintOf(StruckTint, Weapon, element != null && element.HasElement ? element.Info : null);

        /// <summary>The blow tint for a struck tint (clear when the move carries no element look), a weapon and what it carries.</summary>
        public static Color BlowTintOf(Color struckTint, Weapon weapon, ElementInfo carried) =>
            StruckLook.Tinted(struckTint) ? struckTint : CutTintOf(weapon, carried);

        /// <summary>The cut and flash tint for a weapon with <paramref name="carried"/> (or none): its hue at full strength.</summary>
        public static Color CutTintOf(Weapon weapon, ElementInfo carried)
        {
            if (weapon == null) return default;
            Color slash = carried != null ? carried.Colour : weapon.SlashColour;
            float most = Mathf.Max(slash.r, Mathf.Max(slash.g, slash.b));
            return most > 0.001f ? new Color(slash.r / most, slash.g / most, slash.b / most, 1f) : default;
        }

        /// <summary>True when the move in progress is an infused strike's carrier.</summary>
        public bool SwingInfused => InfusedNow && infusedElement != null;
        /// <summary>The strike clock a move belongs to (4 or 5) when it is an infused strike, else -1: its own blow does not cut its own clock.</summary>
        public int StrikeClockOf(AttackStep step) => step != null && step == infusedStep ? StrikeRules.Clock(infusedSecond) : -1;
        /// <summary>
        /// A spell's blow has landed on something (once for each thing), with the number of the slot cast it came from
        /// (<see cref="SpellBlow.Cast"/>, 0 for none): a slot cast's own first one charges the blade.
        /// </summary>
        public event System.Action<Component, int> SpellBlowLanded;
        /// <summary>A move that carried the charge landed: which of the three, and its element.</summary>
        public event System.Action<ChargedBlow> ChargedBlowLanded;

        private PlayerSpells spells;
        private AttackStep infusedStep;
        private bool infusedDeep, infusedSecond, infusedPending;
        private ElementInfo infusedElement;
        private Component infusedFirst;
        private float infusedDamage;
        private int strikeBuffered = -1;
        private float strikeBufferLeft;
        private bool moveCharged, moveChargeSpent;
        private ElementInfo moveElement;
        private ChargedBlow chargedPending;
        private Component chargedTarget;
        private float chargedDamage;
        private readonly Component[] gathered = new Component[32];

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
            Arm(string.IsNullOrEmpty(RunState.Weapon) ? StartingWeapon : RunState.Weapon);
            // Every scene starts at half: mana now pays only for the big moves, and the player never starts a fight empty.
            Mana.Empty();
            Mana.AtLeast(Mana.SettleLevel);
        }

        private void Start()
        {
            lightAction = player.FindAction("LightAttack");
            heavyAction = player.FindAction("HeavyAttack");
            moveAction = player.FindAction("Move");
            spells = GetComponent<PlayerSpells>();
            // The Armament wears whatever element is held, which is known only once the element has woken too.
            if (Weapon != null && Weapon.ElementColoured && element != null && element.Info != null) Arm(Weapons.ArmamentName, element.Info.Name);
        }

        /// <summary>
        /// Takes a weapon in hand by name, built by the armoury (way_build_plan.md 2.0): in the aspect the Way's choices
        /// give its kind, with the learned marks of its Way that are in force once it is carried. Every place that hands
        /// the player a weapon comes here; with no mark of the Way learned it is the plain weapon, as before.
        /// </summary>
        public void Arm(string weaponName) => Arm(weaponName, null);

        /// <summary>As <see cref="Arm(string)"/>, with the element an Armament is made in (null: the run's).</summary>
        public void Arm(string weaponName, string armamentElement) => Equip(Resolved(weaponName, armamentElement));

        /// <summary>The weapon <see cref="Arm(string, string)"/> would put in hand, without taking it.</summary>
        public Weapon Resolved(string weaponName, string armamentElement)
        {
            if (spells == null) spells = GetComponent<PlayerSpells>();
            if (element == null) element = GetComponent<PlayerElement>();
            PlayerBuild.Fill(armedWith, element, spells, this);
            return Way.Armoury.Arm(weaponName, armedWith, RunState.Unlocked, armamentElement);
        }

        /// <summary>
        /// Builds the weapon in hand again from the armoury, after a mark of the Way is bought or its aspect or keystone is
        /// chosen. Never mid-swing: while an attack is on it waits for the attack to end.
        /// </summary>
        public void Rearm()
        {
            if (Weapon == null) return;
            if (BusyWithWeapon)
            {
                rearmPending = true;
                return;
            }
            rearmPending = false;
            Rearms++;
            Arm(Weapon.Name, Weapon.ElementColoured && element != null && element.Info != null ? element.Info.Name : null);
        }

        /// <summary>What the Way's family mark did to the last infused strike, for probes and the engine pieces still to come.</summary>
        public Way.InfusionBend LastInfusion { get; private set; }
        // Equip drops all of these, so a re-arm waits for them: a swing or a charge, a weapon thrown or a dash out, the
        // infused strike's element still to come, a follow to what was thrown.
        private bool BusyWithWeapon => IsAttacking || IsCharging || WeaponAway || IsDashing || infusedPending || followFor != null || dragFollow != null;

        /// <summary>
        /// True in a swing's wind-up and blow, when its poise holds (<see cref="Poised"/>); not in its recovery, where the
        /// Body's Steady decides as it does outside a swing.
        /// </summary>
        public bool InSwingMoment => Combo.Current != null && (Combo.Phase == AttackPhase.Windup || Combo.Phase == AttackPhase.Active);

        /// <summary>True while a re-arm waits for the swing in progress to end.</summary>
        public bool RearmPending => rearmPending;
        /// <summary>Re-arms done, for probes.</summary>
        public int Rearms { get; private set; }
        private bool rearmPending;
        // What the armoury is asked with: the player's attunements and the Way's choices, the weapon swapped in.
        private readonly Build.Loadout armedWith = new Build.Loadout();

        /// <summary>Takes a weapon in hand, dropping whatever attack was in progress.</summary>
        public void Equip(Weapon weapon)
        {
            Weapon = weapon;
            // A change of aspect starts its counts and windows afresh; the same aspect built again keeps them.
            if (weapon.Aspect != aspectKept) AspectState.Clear();
            aspectKept = weapon.Aspect;
            // The chord plays the technique the armoury found in force on this weapon; none, and the chord stays a heavy.
            TechniqueStep = weapon.Technique;
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
            infusedPending = false;
            infusedStep = null;
            dragFollow = null;
            holdLeft = 0f;
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
                // The Answer's Read the Red: red attacks are easier to guard perfectly.
                if (upgrades == null) upgrades = GetComponent<PlayerUpgrades>();
                defence.Guard.RedWindowBonus = upgrades != null && upgrades.Has(Trees.Ways.KatanaWay.ReadTheRed) ? Way.AnswerWindow.ReadTheRedSeconds : 0f;
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
            // A blow charged with Earth cannot be stopped once it is wound up.
            if (step != null && moveCharged && moveElement != null && ElementSteel.Unstoppable(moveElement.Name)
                && (Combo.Phase == AttackPhase.Windup || Combo.Phase == AttackPhase.Active)) return true;
            return step != null && Way.BodyRules.Poised(step.Poise + StatPoise, damage)
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
            if (rearmPending && !BusyWithWeapon) Rearm();
            TickAspect(deltaTime);
            TurnZanshin(deltaTime);
            if (HeavyCriticalLeft > 0f) HeavyCriticalLeft -= deltaTime;
            if (heavyLoadLeft > 0f && (heavyLoadLeft -= deltaTime) <= 0f) HeavyStaggerLoaded = 0f;
            if (lightAction == null || heavyAction == null) return;
            // The element's strike follows the carrier's blow once the swing is out (or was cut short).
            if (infusedPending && (Combo.Current != infusedStep || Combo.Phase != AttackPhase.Active)) FireInfused();
            Tether(deltaTime);
            Combo.EnderAlways = boundTo != null;
            Chord.Tick(deltaTime);
            // With the weapon out of the hand there is nothing to attack with; behind a raised guard the light press is the guard's own thrust.
            var buttons = player.Buttons;
            bool lightNow = (buttons != null ? buttons.Pressed(PlayerButton.LightAttack) : lightAction.WasPressedThisFrame()) && !WeaponAway;
            bool heavyNow = (buttons != null ? buttons.Pressed(PlayerButton.HeavyAttack) : heavyAction.WasPressedThisFrame()) && !WeaponAway;
            // The Answer's moment (way_build_plan.md 3.4): light is the Riposte, heavy Through the Gap; Kiri-otoshi answers
            // by itself. Never into a wind-up or a swing already out.
            if (Weapon.Aspect is Way.Aspects.AnswerAspect && Way.AnswerWindow.Open(AspectState)
                && (lightNow || heavyNow || Way.AnswerWindow.Answers(upgrades)) && TryAnswer(heavyNow && !lightNow))
            {
                lightNow = false;
                heavyNow = false;
            }
            if (lightNow)
            {
                Chord.Light();
                Combo.Press(defence != null && defence.IsGuarding, player != null && player.Dodge.IsRolling && player.Dodge.Progress >= Combo.DashCutFrom);
            }
            bool chordAfterTick = false, chordNow = false;
            if (heavyNow)
            {
                // R1 and R2 together: the light starts at once as always, and a heavy close behind it cancels it into
                // the technique, so no light is ever held back waiting to see whether a chord is coming.
                if (TechniqueStep != null && Combo.LightWindingUp && Chord.Heavy(Combo.Elapsed)) chordNow = true;
                else if (TechniqueStep != null && lightNow) chordAfterTick = true;
                else PressHeavyNow();
            }
            if (buttons != null && !WeaponAway)
            {
                if (buttons.StrikeFirst)
                {
                    strikeBuffered = 0;
                    strikeBufferLeft = StrikeBufferTime;
                }
                else if (buttons.StrikeSecond)
                {
                    strikeBuffered = 1;
                    strikeBufferLeft = StrikeBufferTime;
                }
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
            if (strikeBufferLeft > 0f)
            {
                strikeBufferLeft -= deltaTime;
                if (strikeBufferLeft <= 0f) strikeBuffered = -1;
            }
            if (unavailable)
            {
                if (chordAfterTick || chordNow) PressHeavyNow();
                // A roll or a flinch takes the place of a return cut still to come.
                dragFollow = null;
                Combo.Idle(deltaTime, player.Dodge.IsRolling);
                return;
            }
            if (dragFollow != null)
            {
                dragFollowIn -= deltaTime;
                if (dragFollowIn <= 0f && Combo.Follow(dragFollow, player.Stamina))
                {
                    dragFollow = null;
                    DragFollows++;
                }
                // Something else began meanwhile: the return cut is let go.
                else if (dragFollowIn < -0.5f) dragFollow = null;
            }

            bool begun = chordNow && TakeChord();
            // A chord that could not be paid for is the heavy it was pressed as.
            if (chordNow && !begun) PressHeavyNow();
            if (strikeBuffered >= 0 && !begun && !WeaponAway && Combo.Phase != AttackPhase.Windup && Combo.Phase != AttackPhase.Active)
            {
                bool second = strikeBuffered == 1;
                strikeBuffered = -1;
                begun = TryInfused(second);
            }
            float swingTime = deltaTime * SwingSpeed * StatSwing;
            // The finisher's held pose (way_build_plan.md question 5): once the blow is in, the step's clock stands still
            // for its hold, so the clip's pose holds there; 0.2 s on the Katana, 0.4 s at Temper V.
            if (Way.Temper.HoldBegins(Combo.Current, Combo.IsFinishing, Combo.Elapsed, finisherHeld))
            {
                finisherHeld = true;
                holdLeft = Combo.Current.PoseHold;
                FinisherHolds++;
            }
            if (holdLeft > 0f)
            {
                holdLeft -= deltaTime;
                swingTime = 0f;
            }
            var events = Combo.Tick(swingTime, player.Stamina, Mana);
            if (chordAfterTick)
            {
                // Both pressed in one frame: the light has just started, and is taken at once.
                if (Combo.LightWindingUp && Chord.Heavy(Combo.Elapsed) && TakeChord()) begun = true;
                else PressHeavyNow();
            }
            if (begun) events |= ComboEvents.Started;
            if ((events & ComboEvents.Started) != 0)
            {
                finisherHeld = false;
                holdLeft = 0f;
            }
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
                // A charged blade rides on every move but an infused strike, which carries its own element.
                moveCharged = Blade.Charged && Combo.Current != infusedStep;
                moveElement = moveCharged ? Elements.ByName(Blade.Element) : null;
                moveCharged &= moveElement != null;
                moveChargeSpent = false;
                beatsDone = 0;
                holdDone = false;
                slamDrawn = false;
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
            if (Combo.Current != null && Combo.Current == infusedStep && Combo.Phase == AttackPhase.Windup) SlamEarly(Combo.Current);
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
            // A finisher's held pose keeps its back to what it passed until the hold is over (the Katana's run-through).
            bool holdingPose = step == passTurn && Combo.IsFinishing && step.PoseHold > 0f && (!finisherHeld || holdLeft > 0f);
            if (passTurn != null && (step != passTurn || (Combo.Phase == AttackPhase.Recovery && !holdingPose)))
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
            var set = target is IAfflictable afflictable ? afflictable.Afflictions : null;
            set?.Hold(seconds);
            boundTo = target;
            // How long the chain binds is the chain's: what can be held at all is bound the whole time, even when a
            // god's guard or the shortness of its hold lets it go sooner; a godly form, held only for a hitch, the short time.
            boundLeft = set != null && set.CanFreeze ? seconds : GodBindSeconds;
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
            int handle = shots.Fire(this, step.Shot, (step.Shot.FromStepStart ? stageOrigin : transform.position) + forward * 0.4f, forward, EffectColour, element != null ? element.Info : null, BlowTint);
            // The infused shot's arrow: told where it stops, so its impact is drawn there.
            if (shotWatchPending && handle >= 0) shots.Watch(handle);
            shotWatchPending = false;
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
                // The beat's sparks in the blow's own hue held low: plain, their cores lit a white star at the bow as its
                // shot was staged (the bow's films of 2026-11-22). Dust stays dust.
                if (beat.Effect == VfxKind.Dust) Vfx.Play(beat.Effect, where, transform.forward);
                else Vfx.Play(beat.Effect, where, transform.forward, 1f, StruckLook.Spark(BlowTint, ImpactGlow));
                if (Weapon.ElementColoured) Vfx.Play(VfxKind.TintSpark, where, transform.forward, 0.6f, StruckLook.Spark(EffectColour, ImpactGlow));
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
            // Critical by its own word, or because its cast was made critical (Daybreak's Master: loosed as a blow would land).
            bool critical = blow.Critical || (upgrades != null && upgrades.CastCritical(blow.Cast));
            // What the body flashes in: the blow's own struck tint; else the spell's colour; else the weapon's (an arrow, a
            // thrown blade, an infused strike's spread). Untinted, these lit whole bodies pure white (the bow's films of
            // 2026-11-22). A reaction's untinted blow is left to the body's lilac.
            Color struck = StruckLook.Tinted(blow.Struck) || blow.FromReaction ? blow.Struck : blow.Tinted && StruckLook.Tinted(blow.Tint) ? blow.Tint : BlowTint;
            var carrier = target as IAfflictable;
            if (StatAfflicted != 1f && carrier != null && carrier.Afflictions.Any) damage *= StatAfflicted;
            if (Stats != null && carrier != null) damage *= Mathf.Max(0.1f, 1f + Stats.Against(PlayerStat.SpellDamage, carrier.Afflictions));
            // A reaction's own blow, harder on what carries what a mark asks (Every Crack Rings: the notched), and critical when a mark makes it so.
            if (blow.ReactionOf > 0 && Stats != null && carrier != null) damage *= Mathf.Max(0.1f, 1f + Stats.Against(PlayerStat.ReactionDamageOn, carrier.Afflictions));
            if (critical) damage *= CritMultiplier;
            if (blow.Affliction == AfflictionKind.Burn)
            {
                // A burn is given in seconds, with or without depth.
                float seconds = (blow.AfflictionSeconds > 0f ? blow.AfflictionSeconds : Clover.Gameplay.Afflictions.Spec(AfflictionKind.Burn).Seconds) * AfflictionSeconds;
                return new Hit(damage, blow.Stagger, direction, blow.Heavy, true, origin, seconds, blow.Stacks > 0 ? blow.Stacks + ExtraStacks : 0, blow.Detonate,
                    AfflictionKind.None, 0, 0f, AfflictionPower, blow.Spell, critical, fromReaction: blow.FromReaction, tint: struck);
            }
            bool lays = blow.Affliction != AfflictionKind.None && blow.Stacks > 0;
            float lasts = !lays ? 0f : blow.AfflictionSeconds > 0f ? blow.AfflictionSeconds * AfflictionSeconds
                : AfflictionSeconds != 1f ? Clover.Gameplay.Afflictions.Spec(blow.Affliction).Seconds * AfflictionSeconds : 0f;
            return new Hit(damage, blow.Stagger, direction, blow.Heavy, true, origin, 0f, 0, blow.Detonate,
                lays ? blow.Affliction : AfflictionKind.None, lays ? blow.Stacks + ExtraStacks : 0, lasts, AfflictionPower, blow.Spell, critical, fromReaction: blow.FromReaction, tint: struck);
        }

        // What every spell's blow does once it has landed: its effect, what it earns, and word to the upgrades.
        private void Landed(in SpellBlow blow, IDamageable target, Collider other, Vector3 direction, bool wasAfflicted)
        {
            // The element art's hit in place of the effect, unless the owner's switch has the old effects showing; an
            // unseen blow (ground its reaction draws) throws neither.
            if (blow.Unseen) { }
            else if (Clover.Player.Forms.HitArt.Play(blow.Look, other.bounds.center, direction)) { }
            else if (blow.Tinted) Vfx.Play(blow.Effect, other.bounds.center, direction, 1f, blow.Tint);
            else Vfx.Play(blow.Effect, other.bounds.center, direction);
            if (defence != null && PaysBack(target)) defence.DealtDamage(blow.Damage);
            if (blow.Spell && upgrades != null) upgrades.OnSpellStruck(target, wasAfflicted, blow.Look != null ? blow.Look.Name : null, blow.Cast); ReactionStruck(blow, target);
            if (blow.Spell && target is Component spellBody)
            {
                SpellBlowLanded?.Invoke(spellBody, blow.Cast);
                ShareAlongChains(spellBody, blow.Damage, true);
            }
        }

        // Chained enemies (Black Chain, Shared Wound): each chain's share of a blow on one is dealt to the other, once, with
        // nothing more: it is not a blow of its own, so it passes no further and answers nothing.
        private const int MostChainMates = 8;
        private readonly Component[] chainMates = new Component[MostChainMates];
        private readonly float[] chainShares = new float[MostChainMates];
        private bool sharingChains;
        // The bodies whose loose shadow a swing reached, and how many such blows have landed (for probes).
        private readonly Component[] shadowBodies = new Component[LooseShadows.Most];
        public int ShadowBlows { get; private set; }
        /// <summary>Shares of blows dealt along chains, counted for probes.</summary>
        public int ChainShares { get; private set; }

        private void ShareAlongChains(Component struck, float damage, bool spell)
        {
            if (sharingChains || damage <= 0f || struck == null || !(struck is IAfflictable)) return;
            int mates = EnemyChains.Mates(struck, spell, chainMates, chainShares);
            if (mates == 0) return;
            sharingChains = true;
            try
            {
                for (int i = 0; i < mates; i++)
                {
                    var mate = chainMates[i];
                    chainMates[i] = null;
                    if (!PlayerReactions.Alive(mate) || !(mate is IDamageable joined)) continue;
                    Vector3 along = mate.transform.position - struck.transform.position;
                    along.y = 0f;
                    var shared = new Hit(damage * chainShares[i], 0f, along.sqrMagnitude > 0.01f ? along.normalized : transform.forward, false, true, struck.transform.position,
                        fromReaction: true, tint: ChainTint);
                    if (joined.TakeHit(shared) != HitOutcome.Missed) ChainShares++;
                }
            }
            finally { sharingChains = false; }
        }

        // The colour a shared blow flashes on the chained one: the chain's dark iron, not a plain blow's white.
        private static readonly Color ChainTint = new Color(0.42f, 0.36f, 0.5f, 1f);

        // A reaction's own blow tells the upgrades whom that reaction struck (the ledger's reaction-struck).
        private void ReactionStruck(in SpellBlow blow, IDamageable target)
        {
            if (blow.ReactionOf > 0 && upgrades != null && target is Component body) upgrades.OnReactionStruck(body, blow.ReactionOf - 1, blow.ReactionCast, blow.Damage);
        }

        /// <summary>
        /// Strikes everything in a cone in front of the player that is not the player: what a spell does. Returns how
        /// many things it hit. The blow is described by the caller, so spells and weapons share one path to a target.
        /// </summary>
        public int StrikeCone(float reach, float arcDegrees, float damage, float stagger, float burnSeconds, VfxKind effect, bool detonate = false) =>
            StrikeCone(reach, arcDegrees, Plain(damage, stagger, burnSeconds, effect, detonate));

        public int StrikeCone(float reach, float arcDegrees, in SpellBlow blow)
        {
            if (upgrades != null) upgrades.SpellFrom = transform.position;
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
        public int StrikeArea(Vector3 centre, float radius, in SpellBlow blow, HashSet<IDamageable> once, float innerRadius,
            AfflictionKind onlyCarrying = AfflictionKind.None)
        {
            if (upgrades != null) upgrades.SpellFrom = centre;
            spellStruck.Clear();
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || (once != null && once.Contains(target))) continue;
                // Only those carrying what is asked (The Sea Finds Land's discharge runs through the water to the Soaked).
                if (!Carries(target, onlyCarrying)) continue;
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
            if (upgrades != null) upgrades.SpellFrom = from;
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
            if (upgrades != null) upgrades.SpellFrom = from;
            if (victim == null || !victim.gameObject.activeInHierarchy || !(victim is IDamageable target)) return false;
            Vector3 direction = victim.transform.position - from;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward;
            bool wasAfflicted = target is IAfflictable afflictable && afflictable.Afflictions.Any;
            var outcome = target.TakeHit(Make(blow, target, direction, from));
            if (outcome == HitOutcome.Missed) return false;
            Vector3 at = victim.transform.position + Vector3.up * 1.1f;
            if (Clover.Player.Forms.HitArt.Play(blow.Look, at, direction)) { }
            else if (blow.Tinted) Vfx.Play(blow.Effect, at, direction, 1f, blow.Tint);
            else Vfx.Play(blow.Effect, at, direction);
            if (defence != null && PaysBack(target)) defence.DealtDamage(blow.Damage);
            if (blow.Spell && upgrades != null) upgrades.OnSpellStruck(target, wasAfflicted, blow.Look != null ? blow.Look.Name : null, blow.Cast); ReactionStruck(blow, target);
            if (blow.Spell && target is Component spellBody)
            {
                SpellBlowLanded?.Invoke(spellBody, blow.Cast);
                ShareAlongChains(spellBody, blow.Damage, true);
            }
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

        public int AfflictArea(Vector3 centre, float radius, AfflictionKind kind, int stacks, Component except = null, LaySource source = LaySource.Effect,
            AfflictionKind onlyCarrying = AfflictionKind.None)
        {
            spellStruck.Clear();
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || spellStruck.Contains(target) || ReferenceEquals(target, except) || !(target is IAfflictable afflictable)) continue;
                // Only those carrying the affliction asked for (Sea Ice's web: the Soaked).
                if (!Carries(target, onlyCarrying)) continue;
                Vector3 offset = ((Component)target).transform.position - centre;
                offset.y = 0f;
                if (offset.magnitude > radius) continue;
                spellStruck.Add(target);
                // Carried, not struck: it is not carried on again from here, so two Soaked enemies cannot pass it back and forth.
                afflictable.Afflictions.Lay(kind, stacks + ExtraStacks, 0f, AfflictionPower, source, (Component)target);
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
        public int HoldArea(Vector3 centre, float radius, float seconds, AfflictionKind onlyCarrying = AfflictionKind.None)
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
                if (!Carries(target, onlyCarrying)) continue;
                Vector3 offset = ((Component)target).transform.position - centre;
                offset.y = 0f;
                if (offset.magnitude > radius) continue;
                spellStruck.Add(target);
                if (afflictable.Afflictions.Hold(seconds)) held++;
            }
            return held;
        }

        /// <summary>Drags everything in a circle toward its middle, or throws it away from there, by about this many metres. Returns how many it moved.</summary>
        public int Displace(Vector3 centre, float radius, float metres, bool pull, AfflictionKind onlyCarrying = AfflictionKind.None)
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
                // Only those carrying the affliction asked for (The Pull of Night: the Soaked).
                if (!Carries(target, onlyCarrying)) continue;
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

        /// <summary>
        /// Draws what the last spell strike reached (<see cref="LastStruck"/>) toward a point, this many metres at most and
        /// never past it; the one standing on the point stays. Returns how many were moved.
        /// </summary>
        public int DrawStruckToward(Vector3 point, float metres)
        {
            int moved = 0;
            foreach (var target in spellStruck)
            {
                if (!(target is IShovable shovable) || !(target is Component body) || body == null) continue;
                Vector3 offset = point - body.transform.position;
                offset.y = 0f;
                float distance = offset.magnitude;
                float step = PullStep(distance, metres);
                if (step <= 0f) continue;
                shovable.Push(offset / distance * step);
                moved++;
            }
            return moved;
        }

        /// <summary>How far a pull of <paramref name="metres"/> moves one this far from its point: never past arm's length of it.</summary>
        public static float PullStep(float distance, float metres) => Mathf.Clamp(distance - 1f, 0f, Mathf.Max(0f, metres));

        /// <summary>True when no affliction is asked for, or the target carries it.</summary>
        public static bool Carries(object target, AfflictionKind kind) =>
            kind == AfflictionKind.None || (target is IAfflictable afflicted && afflicted.Afflictions.Has(kind));

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
        /// <summary>
        /// The nearest (or the farthest) enemy within <paramref name="radius"/> of a point that carries either affliction
        /// given, other than <paramref name="except"/>; null when none does (the ledger's aim-at-afflicted).
        /// </summary>
        public Component EnemyCarrying(Vector3 point, float radius, AfflictionKind kind, AfflictionKind orKind = AfflictionKind.None, bool farthest = false, Component except = null)
        {
            Component found = null;
            float best = 0f;
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
                if (target == null || target == except || !CarriesEither(target, kind, orKind)) continue;
                Vector3 offset = target.transform.position - point;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                if (distance > radius * radius) continue;
                if (found != null && !Better(distance, best, farthest)) continue;
                best = distance;
                found = target;
            }
            return found;
        }

        /// <summary>True when what is struck carries either affliction asked (None asks for nothing more).</summary>
        public static bool CarriesEither(object target, AfflictionKind kind, AfflictionKind orKind) =>
            (kind != AfflictionKind.None && Carries(target, kind)) || (orKind != AfflictionKind.None && Carries(target, orKind));

        /// <summary>Whether a distance beats the best so far: nearer, or farther when the farthest is wanted.</summary>
        public static bool Better(float distance, float best, bool farthest) => farthest ? distance > best : distance < best;

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
            if (upgrades != null) upgrades.SpellFrom = transform.position;
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
                    if (blow.Spell && upgrades != null) upgrades.OnSpellStruck(target, wasAfflicted, blow.Look != null ? blow.Look.Name : null, blow.Cast); ReactionStruck(blow, target);
                    if (blow.Spell && target is Component spellBody) SpellBlowLanded?.Invoke(spellBody, blow.Cast);
                }
                from = at.position;
                at = NextUnstruck(from, leapRange, struck);
            }
            return hits;
        }

        // The nearest enemy to a point that the current strike has not yet touched.
        /// <summary>
        /// An arc from <paramref name="from"/> that never strikes it: it leaps to the nearest other enemy within reach
        /// that carries <paramref name="seeks"/>, then on from there, up to <paramref name="leaps"/> times (Live Iron's
        /// iron to iron). Returns how many it struck.
        /// </summary>
        public int StrikeChainFrom(Transform from, in SpellBlow blow, int leaps, float leapRange, ElementInfo look, AfflictionKind seeks,
            AfflictionKind onwardFrom = AfflictionKind.None)
        {
            var struck = spellStruck;
            struck.Clear();
            if (from == null) return 0;
            var origin = from.GetComponentInParent<IDamageable>();
            if (origin != null) struck.Add(origin);
            Vector3 at = from.position;
            int hits = 0;
            for (int leap = 0; leap < leaps; leap++)
            {
                var next = NextUnstruck(at, leapRange, struck, seeks);
                if (next == null) break;
                var target = next.GetComponentInParent<IDamageable>();
                if (target == null) break;
                struck.Add(target);
                Vector3 direction = next.position - at;
                direction.y = 0f;
                bool wasAfflicted = target is IAfflictable afflictable && afflictable.Afflictions.Any;
                var outcome = target.TakeHit(Make(blow, target, direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward, at));
                ElementLook.Strike(look, next.position, 0.2f);
                if (outcome != HitOutcome.Missed)
                {
                    hits++;
                    if (defence != null && PaysBack(target)) defence.DealtDamage(blow.Damage);
                    if (blow.Spell && upgrades != null) upgrades.OnSpellStruck(target, wasAfflicted, blow.Look != null ? blow.Look.Name : null, blow.Cast); ReactionStruck(blow, target);
                }
                at = next.position;
                // It leaps on only from those carrying what is asked (Mirror to Mirror: Sundered).
                if (!Carries(target, onwardFrom)) break;
            }
            return hits;
        }

        private Transform NextUnstruck(Vector3 from, float range, HashSet<IDamageable> struck, AfflictionKind seeks = AfflictionKind.None)
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
                if (seeks != AfflictionKind.None && !(target is IAfflictable carrier && carrier.Afflictions.Has(seeks))) continue;
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

        /// <summary>
        /// True when any enemy within <paramref name="radius"/> of a point has its poison bite within <paramref name="within"/>
        /// seconds, before or after (the ledger's cast-on-beat: a cast timed to the poison's beat).
        /// </summary>
        public bool AnyNearBeat(Vector3 point, float radius, float within)
        {
            int count = Overlap(point + Vector3.up, radius);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                if (other.GetComponentInParent<IAfflictable>() is IAfflictable enemy && enemy.Afflictions.NearBeat(within)) return true;
            }
            return false;
        }

        /// <summary>Metres within which an enemy's blow about to land is looked for (the shared movement rule).</summary>
        public float AboutToLandRange = 6f;

        /// <summary>True when a blow that lands in <paramref name="strikeIn"/> seconds is about to land, within <paramref name="window"/>.</summary>
        public static bool AboutToLand(float strikeIn, float window) => window > 0f && strikeIn <= window;

        /// <summary>
        /// The nearest enemy within <see cref="AboutToLandRange"/> whose shown blow lands within <paramref name="window"/>
        /// seconds, and when; null when none (the ledger's blow-about-to-land: a movement spell started as a blow would land
        /// counts as a perfect dodge).
        /// </summary>
        public Component BlowAboutToLand(float window, out float strikeIn)
        {
            strikeIn = float.MaxValue;
            Component found = null;
            float best = float.MaxValue;
            int count = Overlap(transform.position + Vector3.up, AboutToLandRange);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var enemy = other.GetComponentInParent<IAfflictable>();
                if (enemy == null || !AboutToLand(enemy.Afflictions.StrikeIn, window)) continue;
                var body = enemy as Component;
                if (body == null) continue;
                float distance = (body.transform.position - transform.position).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                found = body;
                strikeIn = enemy.Afflictions.StrikeIn;
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
                {
                    // Where the head came down, or where a finished body did.
                    Vector3 spot = (finisher ? new Vector3(finished.x, transform.position.y, finished.z) : transform.position + forward * step.Reach) + Vector3.up * 0.15f;
                    // An infused slam lands in its element's own look, which carries its own dust and debris.
                    if (!(slamDrawn && step == infusedStep)) Vfx.Play(VfxKind.Dust, spot, Vector3.up, 2f + step.Radius * 0.6f);
                    // The Great Hammer's slams have no glow ring: it lit white behind the target on film (swing comments,
                    // 2026-11-21), and no strike look draws rings.
                    if (Weapon == null || Weapon.Name != Weapons.GreatHammerName) Vfx.Play(VfxKind.TintRing, spot, Vector3.up, Mathf.Max(1.2f, step.Radius), colour);
                    break;
                }
                case StepEffect.SpikeLine:
                    // Eight spikes run out along a lane, each enemy struck once by whichever reaches it first.
                    if (spikes != null)
                        spikes.Run(this, transform.position + forward * 1.2f, forward, 8, 1.25f, 0.09f, 0f, colour, true, new SpellBlow
                        {
                            Damage = 34f, Stagger = 45f, Heavy = true, Effect = held != null ? ElementLook.HitEffect(held) : VfxKind.Impact,
                            Affliction = held != null ? held.Affliction : AfflictionKind.None, Stacks = held != null ? 1 : 0,
                            Tinted = held != null && ElementLook.Tints(held), Tint = colour, Look = held,
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

        private void PressHeavyNow()
        {
            // Looked for on the press only, not every frame: a staggered enemy in reach turns the heavy into the finisher.
            finisherTarget = FindEnemy(transform.forward, 360f, FinisherRange, true);
            Combo.PressHeavy(finisherTarget != null);
        }

        // The chord taken: the light in its first moment is dropped and the technique starts in its place.
        private bool TakeChord()
        {
            if (TechniqueStep == null || !Combo.Replace(TechniqueStep, player.Stamina)) return false;
            ChordsTaken++;
            return true;
        }

        /// <summary>
        /// L2 with R1 (or R2): the weapon's carrier, played with an element on it (IMPLEMENTATION.md 3.4): R1 the first
        /// attunement, R2 the second; all in or with one element, R2 is the first's deep strike. Its clock and price
        /// are <see cref="PlayerSpells"/>'s.
        /// </summary>
        private bool TryInfused(bool second)
        {
            if (spells == null) spells = GetComponent<PlayerSpells>();
            if (spells == null || element == null || Weapon == null || Weapon.CarrierStep == null) return false;
            var refusal = spells.CheckStrike(second, out bool deep, out var info);
            if (refusal != StrikeRefusal.None)
            {
                StrikesRefused++;
                if (defence != null && refusal != StrikeRefusal.NoElement) defence.Callout(refusal == StrikeRefusal.NoMana ? "No mana" : "Not ready");
                return false;
            }
            var step = Weapon.CarrierStep.Clone();
            // The shape weapons strike where they stand (ElementLookBuilder.Strikes), so Lightning's is the weapon as
            // conductor, not Flash Cut's 5 m lunge; the arc weapons keep the lunge (the owner's answer, 2026-11-22).
            bool inPlace = StrikesInPlace(Weapon, step, info.Index);
            ElementSteel.Bend(step, info.Name, deep, inPlace);
            // The Way's family mark for the element bends the copy too (task 5.3, Way/Infusion.cs).
            if (upgrades == null) upgrades = GetComponent<PlayerUpgrades>();
            LastInfusion = Way.Infusion.Bend(step, Weapon, info.Name, deep, element.AllIn, upgrades);
            player.FacePointer();
            if (!Combo.Follow(step, player.Stamina))
            {
                StrikesRefused++;
                if (defence != null) defence.Callout("No stamina");
                return false;
            }
            spells.BeginStrike(second, deep);
            float untouchable = inPlace ? 0f : ElementSteel.UntouchableSeconds(step, info.Name);
            if (untouchable > 0f && defence != null) defence.UntouchableUntil = Mathf.Max(defence.UntouchableUntil, Time.time + untouchable);
            infusedStep = step;
            infusedSecond = second;
            infusedDeep = deep;
            infusedElement = info;
            infusedPending = false;
            InfusedStrikes++;
            if (deep) DeepStrikes++;
            // The element gathers on the weapon as the carrier winds up: along its edge (the swing looks), or as a puff in front (the old look).
            if (!Clover.Player.Swings.SwingTrails.On) ElementLook.Puff(info, transform.position + transform.forward * 0.6f + Vector3.up * 1.1f, transform.forward, 0.8f);
            return true;
        }

        // The element's strike on the ground the carrier covered.
        private void FireInfused()
        {
            infusedPending = false;
            if (shapeBodiesPending) ShapeStruckNow();
            ElementSteel.Strike(this, infusedElement, LastFootprint, infusedDeep, infusedDamage, infusedFirst);
            infusedFirst = null;
            // The Way's family mark: the cut lying on the floor, the charged path, and Dragging Draw's return cut to come.
            Way.InfusionGround.Lay(LastInfusion, infusedElement, LastFootprint);
            if (LastInfusion.Follow != null)
            {
                dragFollow = LastInfusion.Follow;
                dragFollowIn = LastInfusion.FollowDelay;
            }
        }

        /// <summary>
        /// What the carried aspect keeps (way_build_plan.md 2.0): its counts and windows, ticked once a frame with what
        /// happened, cleared when the aspect changes. Read by probes.
        /// </summary>
        [System.NonSerialized] public Way.AspectRun AspectState;
        private Way.WeaponAspect aspectKept;
        private int landedThisFrame, guardsSeen, blowsSeen, redSeen, blockedSeen;
        /// <summary>Answers given in the Answer's moment (the Riposte, Through the Gap), for probes.</summary>
        public int Answers { get; private set; }
        private bool finishedThisFrame;
        // Stagger the aspect adds to the blow going out (the seventh drum's draw).
        private float aspectStagger;

        // A draw, for the Way (question 22): the first cut of a combo, the skill, the charged heavy.
        private bool IsDraw(AttackStep step) =>
            step != null && (step == Combo.Skill || step == Combo.ChargedHeavy || (Combo.Steps.Count > 0 && step == Combo.Steps[0]));

        // The carried aspect's step: what happened since the last frame, then the counters cleared.
        private void TickAspect(float deltaTime)
        {
            var aspect = Weapon != null ? Weapon.Aspect : null;
            int guards = defence != null ? defence.PerfectGuards : 0, blows = defence != null ? defence.BlowsTaken : 0;
            int red = defence != null ? defence.RedTurns : 0, blocked = defence != null ? defence.Blocks : 0;
            if (aspect != null)
            {
                if (spells == null) spells = GetComponent<PlayerSpells>();
                var context = new Way.AspectContext
                {
                    DeltaTime = deltaTime,
                    Landed = landedThisFrame,
                    PerfectGuard = guards != guardsSeen,
                    Struck = blows != blowsSeen,
                    RedTurned = red != redSeen,
                    GuardHit = blocked != blockedSeen,
                    Guarding = defence != null && defence.IsGuarding,
                    Finished = finishedThisFrame,
                    InFight = spells != null && spells.Fight.InFight,
                    Marks = upgrades,
                };
                aspect.Tick(ref AspectState, context);
            }
            guardsSeen = guards;
            blowsSeen = blows;
            redSeen = red;
            blockedSeen = blocked;
            landedThisFrame = 0;
            finishedThisFrame = false;
        }

        // The answer given in the moment: Through the Gap is untouchable while it passes the attacker.
        private bool TryAnswer(bool heavy)
        {
            if (Combo.Phase == AttackPhase.Windup || Combo.Phase == AttackPhase.Active) return false;
            var kept = AspectState;
            var step = Way.AnswerWindow.Answer(ref AspectState, Weapon, heavy, upgrades);
            if (step == null || !Combo.Follow(step, player.Stamina))
            {
                AspectState = kept;
                return false;
            }
            player.FacePointer();
            if (heavy && defence != null) defence.UntouchableUntil = Mathf.Max(defence.UntouchableUntil, Time.time + step.Windup + step.Active);
            Answers++;
            return true;
        }

        // Zanshin: after a kill, the turn to the next enemy, waiting for a swing to end.
        private Transform zanshinTarget;
        private float zanshinFor, zanshinTurn;
        /// <summary>Zanshin's turns made, for probes.</summary>
        public int ZanshinTurns { get; private set; }

        /// <summary>
        /// Zanshin (way_build_plan.md 3.1 and question 19): for a second after a kill the player turns to the nearest other
        /// enemy within 8 m over 0.15 s. No effect is drawn; the turn is the sign.
        /// </summary>
        public void Zanshin(Component killed)
        {
            var next = FindEnemy(transform.forward, 360f, Way.Aspects.KatanaTrunk.ZanshinRange, false);
            if (next == null || (killed != null && next == killed.transform)) return;
            zanshinTarget = next;
            zanshinFor = Way.Aspects.KatanaTrunk.ZanshinSeconds;
            zanshinTurn = Way.Aspects.KatanaTrunk.ZanshinTurnSeconds;
        }

        private void TurnZanshin(float deltaTime)
        {
            if (zanshinTarget == null) return;
            zanshinFor -= deltaTime;
            if (zanshinFor <= 0f || !zanshinTarget.gameObject.activeInHierarchy)
            {
                zanshinTarget = null;
                return;
            }
            // Never mid-swing or mid-roll: it waits for them.
            if (Combo.Phase == AttackPhase.Windup || Combo.Phase == AttackPhase.Active || (player != null && player.Dodge.IsRolling)) return;
            Vector3 to = zanshinTarget.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.01f)
            {
                zanshinTarget = null;
                return;
            }
            float share = Way.Aspects.KatanaTrunk.TurnShare(deltaTime, zanshinTurn);
            zanshinTurn -= deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to.normalized, Vector3.up), share);
            if (share < 1f) return;
            zanshinTarget = null;
            ZanshinTurns++;
        }

        // The infused strike drawn in its weapon's strike shape (ElementMoves.PlayShape): the Great Hammer's slam about
        // where the head lands, the Scythe's spin, the Great Spear's thrust, the Twin Knives' dash-through and the Chain
        // Sickle's lash about where the player stands as the blow goes out; begun its lead before (Lightning's bolt on the
        // hammer's raised head, the lash's weight on its way out), and its look on each body it reaches.
        private bool slamDrawn, shapeBodiesPending;
        private int drawnShape = -1;
        private readonly Component[] slamStruck = new Component[16];

        /// <summary>True once the current infused strike's shape look is drawn, so the element's arc look is not drawn as well.</summary>
        public bool SlamDrawn => slamDrawn;

        /// <summary>The strike shape a weapon's infused strike is drawn in (<see cref="Clover.Player.Forms.ElementMoves.Shapes"/>), or -1 for an arc.</summary>
        public static int ShapeOf(Weapon weapon)
        {
            if (weapon == null) return -1;
            switch (weapon.Name)
            {
                case Weapons.GreatHammerName: return Clover.Player.Forms.ElementMoves.Slam;
                case Weapons.ScytheName: return Clover.Player.Forms.ElementMoves.Spin;
                case Weapons.GreatSpearName: return Clover.Player.Forms.ElementMoves.Thrust;
                case Weapons.TwinKnivesName: return Clover.Player.Forms.ElementMoves.Dash;
                case Weapons.ChainSickleName: return Clover.Player.Forms.ElementMoves.Lash;
                case Weapons.GreatBowName: return Clover.Player.Forms.ElementMoves.Shot;
                default: return -1;
            }
        }

        /// <summary>
        /// True when the weapon's infused strike keeps its carrier's own shape rather than Lightning's Flash Cut lunge: the
        /// Great Hammer's slam where the head lands, and the Scythe, Great Spear, Twin Knives and Chain Sickle where the
        /// player stands. The arc weapons lunge.
        /// </summary>
        public static bool StrikesInPlace(Weapon weapon, AttackStep step, int elementIndex)
        {
            int shape = ShapeOf(weapon);
            if (shape < 0 || step == null) return false;
            if (shape == Clover.Player.Forms.ElementMoves.Slam) return step.Effect == StepEffect.GroundSlam && Clover.Player.Forms.ElementMoves.HasSlam(elementIndex);
            return true;
        }

        private bool SlamsHere(AttackStep step)
        {
            if (step == null || step != infusedStep || infusedElement == null) return false;
            int shape = ShapeOf(Weapon);
            if (shape < 0 || (shape == Clover.Player.Forms.ElementMoves.Slam && step.Effect != StepEffect.GroundSlam)) return false;
            if (shape == Clover.Player.Forms.ElementMoves.Shot && step.Shot == null) return false;
            return Clover.Player.Forms.ElementMoves.HasShape(shape, infusedElement.Index);
        }

        // Where the shape's look is drawn about: the slam where the head lands, the others where the player stands, each
        // carried on by the lunge still to come in the wind-up.
        private Vector3 ShapeSpot(AttackStep step, int shape, bool inWindup)
        {
            float still = 0f;
            if (inWindup)
            {
                float over = step.LungeSeconds > 0f ? Mathf.Min(step.LungeSeconds, step.Windup) : step.Windup;
                still = over > 0f ? lunge * Mathf.Clamp01((over - Combo.Elapsed) / over) : 0f;
            }
            float ahead = (shape == Clover.Player.Forms.ElementMoves.Slam ? step.Reach : 0f) + still;
            return transform.position + transform.forward * ahead;
        }

        // In the wind-up: a shape that begins before the blow is started its lead early.
        private void SlamEarly(AttackStep step)
        {
            if (slamDrawn || !SlamsHere(step)) return;
            int shape = ShapeOf(Weapon);
            float lead = Clover.Player.Forms.ElementMoves.ShapeLead(shape, infusedElement.Index);
            float left = step.Windup - Combo.Elapsed;
            if (lead <= 0f || left > lead) return;
            Clover.Player.Forms.ElementMoves.PlayShape(shape, infusedElement.Index, ShapeSpot(step, shape, true), transform.forward);
            slamDrawn = true;
            drawnShape = shape;
        }

        // The blow goes out: the shape about its spot (unless begun early), and the slam's on each body its head reached;
        // a carried blow (the dash, the thrust) reaches its bodies through its swing, so theirs wait for it to end.
        private void SlamLands(AttackStep step)
        {
            int shape = ShapeOf(Weapon);
            if (shape == Clover.Player.Forms.ElementMoves.Shot)
            {
                ShotLoosed(step);
                return;
            }
            if (!slamDrawn) Clover.Player.Forms.ElementMoves.PlayShape(shape, infusedElement.Index, ShapeSpot(step, shape, false), transform.forward);
            slamDrawn = true;
            drawnShape = shape;
            Clover.Player.Forms.StrikeArt.Drawn();
            if (shape == Clover.Player.Forms.ElementMoves.Slam) ShapeStruckNow();
            else shapeBodiesPending = true;
        }

        // The Great Bow's shot: its trace about the player as the arrow leaves; its impact waits for the arrow to stop
        // in someone (ShotStopped), so nothing is drawn on a miss.
        private int shotTrace = -1;
        private ElementInfo shotElement;
        private bool shotWatchPending;
        /// <summary>Counts for probes: shot impacts drawn where the arrow stopped in someone, and shots that stopped in no one.</summary>
        public int ShotImpacts { get; private set; }
        public int ShotMisses { get; private set; }
        /// <summary>Where the last infused shot's impact was drawn, for probes.</summary>
        public Vector3 LastShotImpact { get; private set; }

        private void ShotLoosed(AttackStep step)
        {
            int shape = Clover.Player.Forms.ElementMoves.Shot;
            slamDrawn = true;
            drawnShape = shape;
            Clover.Player.Forms.StrikeArt.Drawn();
            shapeBodiesPending = false;
            shotTrace = Clover.Player.Forms.ElementMoves.PlayShotTrace(infusedElement.Index, ShapeSpot(step, shape, false), transform.forward);
            shotElement = infusedElement;
            shotWatchPending = true;
        }

        /// <summary>
        /// A watched shot has stopped (<see cref="WeaponShots.Watch"/>) at <paramref name="at"/> after <paramref name="flown"/>
        /// metres, in someone when <paramref name="inSomeone"/>: the infused shot's impact is drawn there and on whom it struck.
        /// </summary>
        public void ShotStopped(Vector3 at, Vector3 direction, bool inSomeone, float flown)
        {
            var carried = shotElement;
            shotElement = null;
            if (carried == null) return;
            // Fire's Burning Edge burns where the arrow stopped, not down the lane from the bow (the owner's rule: the
            // effect follows the weapon's path).
            if (carried.Name == Elements.Fire) ElementSteel.BurnWhereShotStopped(at);
            if (!inSomeone)
            {
                ShotMisses++;
                return;
            }
            int count = CopyLastStruck(out var bodies);
            if (!Clover.Player.Forms.ElementMoves.ShotLands(carried.Index, at, direction, bodies, count)) return;
            ShotImpacts++;
            LastShotImpact = at;
            // Stopped well short of where the trace is drawn to: the trace goes with it rather than running on past.
            if (flown < ShotTraceMetres - 1f) Clover.Player.Forms.ElementLooks.EndHeld(shotTrace);
            shotTrace = -1;
        }

        /// <summary>True while the move is the Great Bow's infused shot drawn in its own look: its element's ground waits for the arrow.</summary>
        public bool InfusedShot => slamDrawn && drawnShape == Clover.Player.Forms.ElementMoves.Shot;

        /// <summary>How far out the shot's trace is drawn (the recipe's arrow stopped 6.5 m out).</summary>
        public const float ShotTraceMetres = 6.5f;

        private void ShapeStruckNow()
        {
            shapeBodiesPending = false;
            if (infusedElement == null || drawnShape < 0) return;
            int count = 0;
            foreach (var one in struck)
            {
                if (count >= slamStruck.Length) break;
                if (one is Component body && body != null) slamStruck[count++] = body;
            }
            Clover.Player.Forms.ElementMoves.ShapeStruck(drawnShape, infusedElement.Index, slamStruck, count, transform.forward);
        }

        // Dragging Draw's return cut, waiting its moment after the draw; and the finisher's held pose.
        private AttackStep dragFollow;
        private float dragFollowIn;
        private bool finisherHeld;
        private float holdLeft;
        /// <summary>Counts for probes: Dragging Draw's return cuts played, and finisher poses held.</summary>
        public int DragFollows { get; private set; }
        public int FinisherHolds { get; private set; }

        // Puts the overlaps found nearest first, for a blow that stops in the first enemy it meets.
        private void NearestFirst(int count, Vector3 from)
        {
            for (int i = 1; i < count; i++)
            {
                var one = overlaps[i];
                float at = (one.transform.position - from).sqrMagnitude;
                int j = i - 1;
                while (j >= 0 && (overlaps[j].transform.position - from).sqrMagnitude > at)
                {
                    overlaps[j + 1] = overlaps[j];
                    j--;
                }
                overlaps[j + 1] = one;
            }
        }

        /// <summary>Draws a swing's arc from the player, for effects that follow a blow.</summary>
        public void ShowArc(float reach, float arcDegrees, Color colour)
        {
            if (arcs != null) arcs.Show(transform.position, transform.forward, reach, arcDegrees, 0f, colour);
        }

        /// <summary>Whom the weapon's move in progress has struck.</summary>
        public HashSet<IDamageable> Struck => struck;

        /// <summary>Copies whom the last area, lane or cone strike landed on into a buffer of the player's own, so more strikes can follow without disturbing the list. Read at once.</summary>
        public int CopyLastStruck(out Component[] into)
        {
            into = gathered;
            int count = 0;
            foreach (var one in spellStruck)
            {
                if (count >= gathered.Length) break;
                if (one is Component body && body != null) gathered[count++] = body;
            }
            return count;
        }

        /// <summary>Every enemy within a circle, into the same buffer. Read at once.</summary>
        public int Gather(Vector3 centre, float radius, out Component[] into)
        {
            into = gathered;
            int found = 0;
            int count = Overlap(centre + Vector3.up, radius + 0.5f);
            for (int i = 0; i < count && found < gathered.Length; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                if (!(other.GetComponentInParent<IDamageable>() is Component body)) continue;
                bool seen = false;
                for (int j = 0; j < found && !seen; j++) seen = gathered[j] == body;
                if (seen) continue;
                Vector3 offset = body.transform.position - centre;
                offset.y = 0f;
                if (offset.magnitude <= radius) gathered[found++] = body;
            }
            return found;
        }

        // One blow of the weapon. A fresh blow starts a new list of who has been struck and shows its arc; one that is
        // not (a carried blow, struck again each frame) only looks for anyone new. Scale is for the repeats of a step.
        /// <summary>How far round the target the player must be for a blow to count as on its back: the dot of its facing with the way to the player, under this.</summary>
        public float BackDot = -0.3f;

        /// <summary>True when a blow from <paramref name="striker"/> lands on the back of a body at <paramref name="body"/> facing <paramref name="facing"/>: the striker stands behind it.</summary>
        public static bool OnItsBack(Vector3 body, Vector3 facing, Vector3 striker, float backDot)
        {
            Vector3 toStriker = striker - body;
            toStriker.y = 0f;
            return toStriker.sqrMagnitude > 0.01f && Vector3.Dot(facing, toStriker.normalized) < backDot;
        }

        private void Strike(AttackStep step, bool fresh = true, float scale = 1f)
        {
            if (fresh) struck.Clear();
            int already = struck.Count;
            // A light blow a mark made heavy (blow-made-heavy): the heavy's stagger, and every mark that answers a heavy blow.
            bool madeHeavy = MadeHeavy(step.Heavy, heavyNextUntil, Time.time);
            bool heavy = step.Heavy || madeHeavy;
            float lightStagger = madeHeavy ? HeavyStagger(step.Stagger, Combo.Heavy != null ? Combo.Heavy.Stagger : 0f) : step.Stagger;
            if (fresh)
            {
                // The carried aspect's part in the blow going out: the seventh drum's draw, the hanging cut.
                var added = Weapon.Aspect != null ? Weapon.Aspect.Blow(ref AspectState, step, IsDraw(step), upgrades) : default;
                aspectStagger = added.Stagger;
                if (added.HangingCut) Way.Aspects.SheathAspect.LayHangingCut(step, transform.position, transform.forward, step.Damage * StatDamage, Weapon.SlashColour);
            }
            bool finisher = false;
            bool clean = false;
            bool paidBlow = false;
            Vector3 finished = transform.position;
            bool infused = step == infusedStep && infusedElement != null;
            if (infused && fresh)
            {
                // The carrier's blow is out: where it went is the footprint the element's strike acts on.
                LastFootprint = Footprint.Of(step, transform.position, transform.forward);
                infusedPending = true;
                infusedFirst = null;
                infusedDamage = step.Damage * scale * DamageMultiplier * UpgradeDamage * StatDamage;
            }
            // The element's own colour, lit, not its glow: Fire's glow passes into gold and white, and its strike must read as flame.
            // A charged move cuts in its element's colour too, so the three blows can be told from plain ones.
            var arcElement = infused ? infusedElement : moveCharged ? moveElement : null;
            Color arcColour = arcElement != null ? new Color(arcElement.Colour.r * 1.9f, arcElement.Colour.g * 1.9f, arcElement.Colour.b * 1.9f, 0.95f) : empowered ? empoweredColour : Weapon.ElementColoured ? new Color(EffectColour.r * 2f, EffectColour.g * 2f, EffectColour.b * 2f, 0.9f) : Weapon.SlashColour;
            // The element art cuts in the element's own band, never pushed past white: Fire's stays flame, Poison's never highlighter.
            if (arcElement != null && Clover.Player.Forms.ElementArt.Ready) arcColour = Clover.Player.Forms.BladeArt.ArcColour(arcElement.Index);
            // With the swing looks on the element is drawn by the weapon's own trail; the reach arc keeps the weapon's
            // plain colour, so no second, flat "element" stroke lies beside it (filmed 2026-11-20: a pink comet by the flame).
            if (arcElement != null && Clover.Player.Swings.SwingTrails.On)
            {
                Color plain = Weapon.SlashColour;
                arcColour = new Color(plain.r * 0.35f, plain.g * 0.35f, plain.b * 0.35f, plain.a * 0.2f);
            }
            // A strike drawn in its weapon's shape draws no reach arc: the shape's look replaces it (a pale half-disc over
            // the spear's and the scythe's strikes in the stills of 2026-11-22).
            if (fresh && arcs != null && !(infused && SlamsHere(step)))
            {
                if (step.Radius > 0f) Vfx.Play(VfxKind.TintRing, transform.position + transform.forward * step.Reach + Vector3.up * 0.15f, Vector3.up, step.Radius * 0.6f, arcColour);
                else if (step.LineWidth > 0f) arcs.Show(transform.position, transform.forward, step.Reach, 20f, 0f, arcColour);
                else
                {
                    // A weapon with a tip shows where the tip is: the haft's part of the fan dim, the last metre bright.
                    if (step.SweetFrom > 0f) arcs.Show(transform.position, transform.forward, step.SweetFrom, step.ArcDegrees, step.Sweep, new Color(arcColour.r * 0.45f, arcColour.g * 0.45f, arcColour.b * 0.45f, arcColour.a * 0.5f));
                    arcs.Show(transform.position, transform.forward, step.Reach * WeaponReachScale, step.ArcDegrees, step.Sweep, step.SweetFrom > 0f ? new Color(arcColour.r * 1.4f, arcColour.g * 1.4f, arcColour.b * 1.4f, arcColour.a) : arcColour);
                }
            }
            Vector3 origin = transform.position + Vector3.up;
            Vector3 discCentre = origin + transform.forward * step.Reach;
            var held = element != null ? element.Info : null;
            // The haul on a bound enemy reaches it wherever on its leash it stands.
            float reach = boundTo != null && step == Combo.ChainHeavy ? Mathf.Max(step.Reach, BindLeash + 1f) : step.Reach * WeaponReachScale;
            Component nearest = null;
            float nearestAt = float.MaxValue;
            // A plain blow lands in the weapon's own slash colour, held below white, never a white flash (the owner,
            // 2026-11-22: a plain Katana blow lit the soldier's shield as a white disc). Under an element, its colour.
            Color blowTint = BlowTint;
            // Wind's blows go round the body: no cut streak through it.
            var struckLook = Clover.Player.Swings.SwingLooks.For(SwingElement);
            bool cutsRound = struckLook != null && struckLook.CutsRound;
            int count = Overlap(step.Radius > 0f ? discCentre : origin, step.Radius > 0f ? step.Radius + 0.5f : reach);
            if (step.FirstOnly) NearestFirst(count, origin);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                if (other.transform.root == transform.root) continue;
                var target = other.GetComponentInParent<IDamageable>();
                if (target == null || struck.Contains(target)) continue;
                // Settling Draw: the cut stops in the first enemy it meets.
                if (step.FirstOnly && struck.Count > already) break;
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
                // Overdrive: while it lasts, the last blow of every chain is critical.
                bool chainEnds = step == Combo.ChainHeavy || (Combo.Steps.Count > 0 && step == Combo.Steps[Combo.Steps.Count - 1]);
                bool surgeCrit = chainEnds && element != null && element.SurgeActive && element.SurgeKit != null && element.SurgeKit.SurgeCritsChainEnd;
                bool promised = (heavy && HeavyCriticalLeft > 0f) || step.Critical || surgeCrit;
                float chance = CritChance;
                if (Stats != null) chance += Stats.Blow(PlayerStat.CritChance, heavy, false) + (afflictable != null ? Stats.Against(PlayerStat.CritChance, afflictable.Afflictions) : 0f);
                // Counter Cut: a blow that lands on an enemy showing its own attack (winding up or striking) is critical.
                bool countered = CounterCut && target is Enemy showing && showing.IsStriking;
                if (countered) CounterCuts++;
                // Turned away (Turned to the Sun, Turned About): a blow on its back is critical.
                bool onBack = afflictable != null && afflictable.Afflictions.Conditions.Has(ConditionFlags.BackCritical) && target is Component turned && OnItsBack(turned.transform.position, turned.transform.forward, transform.position, BackDot);
                bool critical = promised || countered || onBack || (afflictable != null && afflictable.Afflictions.MakesBlowsCritical) || (chance > 0f && Random.value < chance);
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
                    damage *= Mathf.Max(0.1f, 1f + Stats.Blow(PlayerStat.WeaponDamage, heavy, critical, endsChain) + Stats.Against(PlayerStat.WeaponDamage, carried)
                        + (inAir ? Stats.InAir(PlayerStat.WeaponDamage) + (heavy ? Stats.InAirHeavy(PlayerStat.WeaponDamage) : 0f) : 0f));
                    staggerShare = Mathf.Max(0.1f, 1f + Stats.Blow(PlayerStat.StaggerDealt, heavy, critical, endsChain) + Stats.Against(PlayerStat.StaggerDealt, carried)
                        + (inAir ? Stats.InAir(PlayerStat.StaggerDealt) + (heavy ? Stats.InAirHeavy(PlayerStat.StaggerDealt) : 0f) : 0f));
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
                // Read before the blade drinks: a burn that was the enemy's only affliction still counts it afflicted.
                bool wasAfflicted = afflictable != null && afflictable.Afflictions.Any;
                // The Blade Drinks Fire: the blow takes Burn off a burning enemy (one, three for a heavy blow, all for a
                // finisher) and strikes harder for each Burn it drinks.
                if (DrinksBurn > 0f && afflictable != null && afflictable.Afflictions.Burn.IsBurning)
                {
                    int drunk = afflictable.Afflictions.Spend(AfflictionKind.Burn, Combo.IsFinishing ? int.MaxValue : heavy ? 3 : 1);
                    if (drunk > 0)
                    {
                        damage += DrinksBurn * drunk;
                        BurnsDrunk += drunk;
                    }
                }
                if (wasAfflicted) damage *= StatAfflicted;
                float burn = (BurnStacksOnHit > 0 ? Mathf.Max(BurnOnHit, StackBurnSeconds) : BurnOnHit) * AfflictionSeconds;
                // What the root of the tree and the surge lay, together, in the element's own affliction. The owner's rule
                // (2026-11-18): weapon blows lay only the first element's affliction; the second comes from spells, the
                // charged blade and infused strikes. So a worn surge of the second element still gives its damage and
                // speed but adds no affliction to blows.
                var first = held != null ? held.Affliction : WeaponAffliction;
                bool surgeOwn = EmpowerStacks > 0 && EmpowerAffliction != AfflictionKind.None && EmpowerAffliction == first;
                int stacks = WeaponAfflictionStacks + (surgeOwn ? EmpowerStacks : 0);
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
                // A charged blade: the element's affliction rides on the blow (one stack more of what it lays already,
                // or a second affliction beside it), 20% harder, and Ice breaks what is Frozen with any blow.
                var second = AfflictionKind.None;
                int secondStacks = 0;
                // The move's flag is set when it starts; the blade must still hold the charge (or this move already
                // spent it), so a move that began without that event never carries a stale charge for free.
                bool carries = moveCharged && moveElement != null && (moveChargeSpent || Blade.Charged);
                if (carries)
                {
                    damage *= Blade.DamageShare;
                    damage += ElementSteel.BeforeChargedBlow(moveElement.Name, afflictable, heavy);
                    var charged = moveElement.Affliction;
                    int add = ElementSteel.Stacks(moveElement.Name);
                    if (charged == kind && kind != AfflictionKind.None) stacks += add;
                    else if (kind == AfflictionKind.None)
                    {
                        kind = charged;
                        stacks = add + ExtraStacks;
                    }
                    else
                    {
                        second = charged;
                        secondStacks = add + ExtraStacks;
                    }
                }
                float lasts = kind != AfflictionKind.None && AfflictionSeconds != 1f ? Clover.Gameplay.Afflictions.Spec(kind).Seconds * AfflictionSeconds : 0f;
                float secondLasts = second != AfflictionKind.None && AfflictionSeconds != 1f ? Clover.Gameplay.Afflictions.Spec(second).Seconds * AfflictionSeconds : 0f;
                // A heavy blow takes what a stance loaded on it (Mountain Stance).
                float loaded = heavy ? HeavyStaggerLoaded : 0f;
                float dealtStagger = (lightStagger + (counter ? counterStagger : 0f) + StaggerOnHit + aspectStagger + loaded) * NextBlowStagger * StatStagger * staggerShare * scale * step.BandStagger(distance);
                if (fromBehind) dealtStagger *= Weapon.BackMultiplier;
                if (boundTo != null && ReferenceEquals(target, boundTo)) dealtStagger *= BoundStagger;
                var outcome = target.TakeHit(new Hit(damage, dealtStagger, transform.forward, heavy, carries && ElementSteel.PassesGuard(moveElement.Name), transform.position,
                    burn, burnStacks, false, kind, kind != AfflictionKind.None ? stacks : 0, lasts, AfflictionPower, false, critical, second, secondStacks, secondLasts, tint: blowTint,
                    finisher: step.Heavy && Combo.IsFinishing));
                if (outcome == HitOutcome.Missed) continue;
                struck.Add(target);
                if (infused && infusedFirst == null && outcome != HitOutcome.Blocked && target is Component firstBody) infusedFirst = firstBody;
                if (carries && !moveChargeSpent && outcome != HitOutcome.Blocked && target is Component chargedBody)
                {
                    // One charge a move, however many it lands on; what the element adds is done once the move's own list is finished.
                    moveChargeSpent = true;
                    chargedPending = Blade.OnBlowLanded();
                    if (chargedPending.Carried)
                    {
                        chargedTarget = chargedBody;
                        chargedDamage = damage;
                        ChargedBlows++;
                    }
                }
                LastBlowDamage = damage;
                // Chained to another (Black Chain): a share of the blow is dealt along the chain.
                if (outcome != HitOutcome.Blocked && target is Component chainedBody) ShareAlongChains(chainedBody, damage, false);
                if (fromBehind && outcome != HitOutcome.Blocked)
                {
                    BackBlows++;
                    Vfx.Play(VfxKind.TintSpark, other.bounds.center, transform.forward, 0.9f, Weapon.SlashColour);
                }
                LastBlowStagger = dealtStagger;
                LastBlowDistance = distance;
                if (step.IsSweet(distance) && outcome != HitOutcome.Blocked)
                {
                    // The tip: a star where it bit, so the right distance can be learned by eye; a warm gold, never white
                    // (the swing films of 2026-11-20: white stars on every heavy weapon's tip).
                    SweetBlows++;
                    Vfx.Play(VfxKind.TintSpark, other.bounds.center, transform.forward, 1.2f, SweetStar);
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
                    // A charged blow of an element drawn in its elements 3 look lands in that look, not under a white star.
                    var charge = SwingElement;
                    bool chargeDrawn = !critical && charge != null && Clover.Player.Forms.ElementArt.Ready && Clover.Player.Forms.ElementMoves.HasBlade(charge.Index);
                    if (critical || (NextBlowDamage > 1f && !chargeDrawn))
                    {
                        // The sign of a critical blow: a gold star with long points where it lands. A stored charge shows the same.
                        if (critical) Criticals++;
                        // Held below white, as the heavy blow's star is: thrown untinted it lit a pure-white starburst on
                        // every Marked enemy an infused strike reached (Light's swing films of 2026-11-21, a mark of light
                        // making each blow critical). The star keeps its gold.
                        Vfx.Play(VfxKind.Critical, other.bounds.center, Vector3.up, 1f, StruckLook.Glow(CriticalGold, CriticalGlow));
                        Vfx.Play(VfxKind.TintSpark, other.bounds.center, transform.forward, 1.6f, StruckLook.Glow(CriticalGold, CriticalSparkGlow));
                    }
                    if (afflictable != null)
                    {
                        // Seeded: the blow feeds the one who struck it. Marked: it gives more mana.
                        float fed = SeedMendingOff ? 0f : afflictable.Afflictions.HealOnBlow;
                        if (fed > 0f && defence != null) defence.Heal(fed, false);
                        earned += afflictable.Afflictions.ManaOnBlow;
                        // A mark of light carries a few critical blows and is spent by them.
                        afflictable.Afflictions.BlowLanded();
                        // A blow that reaches an enemy in the air holds it up a little longer: three times, or more for one who has learned to keep it there.
                        int more = Stats != null ? Mathf.Max(0, Stats.Whole(PlayerStat.AirBlows)) : 0;
                        afflictable.Afflictions.AirBlow(AirBlowSeconds, AirBlowsMost + more, more > 0 ? 0.85f : 1f);
                    }
                }
                if (upgrades != null && pays) upgrades.OnBlow(step, target, outcome, wasAfflicted, heavy);
                if (pays && outcome != HitOutcome.Blocked) paidBlow = true;
                // Spells that wait on the weapon (a brand, a copy of the player, blades at the shoulder) are told; they answer on their own next step.
                if (outcome != HitOutcome.Blocked && pays)
                {
                    if (promised) HeavyCriticalLeft = 0f;
                    if (loaded > 0f) LoadNextHeavy(0f, 0f);
                    Clover.Player.Forms.SpellWorks.Blow(step, target as Component, damage);
                }
                finisher |= outcome == HitOutcome.Finisher;
                if (outcome == HitOutcome.Finisher) finished = other.bounds.center;
                clean |= outcome != HitOutcome.Blocked;
                if (outcome != HitOutcome.Blocked)
                {
                    Vector3 contact = other.ClosestPoint(origin);
                    // A blow whose element lands in its own elements 3 look throws no white spark streak through the body
                    // beside it (the owner, 2026-11-20: "Wind is a line through the enemy").
                    var landing = SwingElement;
                    bool drawnByLook = landing != null && Clover.Player.Forms.ElementArt.Ready
                        && (SwingInfused ? Clover.Player.Forms.ElementMoves.HasStrike(landing.Index) : Clover.Player.Forms.ElementMoves.HasBlade(landing.Index));
                    // The sparks in the blow's own colour below white (round 4: no white where a blow lands); a weapon
                    // with no colour at all keeps the warm steel sparks.
                    if (!drawnByLook && StruckLook.Tinted(blowTint)) Vfx.Play(VfxKind.TintSpark, contact, transform.forward, heavy ? 1.2f : 0.8f, StruckLook.Glow(blowTint, ImpactGlow));
                    else if (!drawnByLook) Vfx.Play(heavy ? VfxKind.HeavySpark : VfxKind.HitSpark, contact, transform.forward);
                    BladeMet?.Invoke(contact, transform.forward);
                }
                else
                {
                    // Turned on the enemy's guard: steel on steel, and a jolt back up the arm.
                    BlowsTurned++;
                    // A heavy blow turned aside is no reason to recommend a heavy blow.
                    if (!heavy) Tips.BlowTurned();
                    Vfx.Play(VfxKind.Clang, other.ClosestPoint(origin));
                    if (shaker != null) shaker.Shake(0.06f, 0.1f);
                }
                if (outcome != HitOutcome.Blocked && !cutsRound)
                {
                    // The cut line: which way the blade went through, drawn over the sparks.
                    Vector3 through = CutDirection(step);
                    // The cut across the body in the element's colour, or with none carried the weapon's own slash colour
                    // (round 4: the plain cut was a white line).
                    if (outcome == HitOutcome.Finisher) CutMarks.Finisher(other.bounds.center, through, blowTint);
                    else if (heavy) CutMarks.Heavy(other.bounds.center, through, blowTint);
                    else CutMarks.Light(other.bounds.center, through, blowTint);
                }
                if (reticle != null && target is Component struckThing) reticle.Struck(struckThing.transform, blowTint);
                // A finisher's flash: the element's hue, or the weapon's for a plain one, as the heavy blow's star.
                if (outcome == HitOutcome.Finisher)
                {
                    if (StruckLook.Tinted(blowTint)) Vfx.Play(VfxKind.Burst, other.bounds.center, Vector3.up, 1f, StruckLook.Glow(blowTint, ImpactGlow));
                    else Vfx.Play(VfxKind.Burst, other.bounds.center);
                }
                // A heavy blow's star: the element's hue at a moderate brightness, or the weapon's for a plain one (the
                // swing films of 2026-11-20: a white starburst on every element's infused strike; round 4: none white).
                if (heavy && outcome != HitOutcome.Blocked)
                {
                    if (StruckLook.Tinted(blowTint)) Vfx.Play(VfxKind.Impact, other.bounds.center, Vector3.up, 1f, StruckLook.Glow(blowTint, ImpactGlow));
                    else Vfx.Play(VfxKind.Impact, other.bounds.center);
                }
                Mana.Gain(earned);
                BlowsLanded++;
                if (defence != null && pays) defence.DealtDamage(damage);
            }
            // A shadow cut loose (Dark's charged blade): the swing that reaches it strikes its body too, with the swing's own
            // damage and stagger, if the swing has not struck the body itself.
            if (fresh)
            {
                int shadows = LooseShadows.Reached(transform.position, transform.forward, reach, step.ArcDegrees, shadowBodies);
                for (int s = 0; s < shadows; s++)
                {
                    var body = shadowBodies[s];
                    shadowBodies[s] = null;
                    if (!(body is IDamageable shadowed) || struck.Contains(shadowed) || !PlayerReactions.Alive(body)) continue;
                    float shadowDamage = step.Damage * scale * DamageMultiplier * UpgradeDamage * NextBlowDamage * StatDamage;
                    var outcome = shadowed.TakeHit(new Hit(shadowDamage, lightStagger * scale * StatStagger, transform.forward, heavy, true, transform.position, tint: blowTint));
                    if (outcome == HitOutcome.Missed) continue;
                    struck.Add(shadowed);
                    ShadowBlows++;
                }
            }
            if (chargedTarget != null)
            {
                var chargedOne = chargedTarget;
                chargedTarget = null;
                ElementSteel.AfterChargedBlow(this, moveElement, chargedOne, chargedPending, chargedDamage);
                if (upgrades != null) upgrades.OnChargedBlow(chargedOne, chargedPending.Third);
                ChargedBlowLanded?.Invoke(chargedPending);
            }
            if (fresh)
            {
                // A blow made heavy is spent once it lands on someone.
                if (MadeHeavy(step.Heavy, heavyNextUntil, Time.time) && struck.Count > already) heavyNextUntil = 0f;
                if (upgrades != null) upgrades.OnSwing(step, struck.Count);
                Clover.Player.Forms.SpellWorks.Swing(step, transform.position, transform.forward);
                // Before the swing is told of (SwingTrails): the slam's look is the infused landing, never beside another.
                if (infused && SlamsHere(step)) SlamLands(step);
                Swung?.Invoke(step);
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
                if (finisher)
                {
                    finishedThisFrame = true;
                    Finished?.Invoke();
                }
                else if (!cutGiven)
                {
                    cutGiven = true;
                    landedThisFrame++;
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
