using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// What the player has learned from the chart, and the embers to learn more. Embers are taken from the fallen and
    /// spent at a rest fire; the run remembers both from scene to scene. The weapon, the guard, the dodge and the
    /// element each tell this when something happens (a blow lands, a guard is perfect, a dodge begins), and this does
    /// whatever the learned upgrades add to it.
    ///
    /// Most upgrades are data (<see cref="Upgrade.Stats"/>, <see cref="Upgrade.Effects"/>, <see cref="Upgrade.Form"/>)
    /// and are run here by one general runner: the stats are summed into <see cref="Stats"/> and pushed into the
    /// systems that use them, the effects are sorted by trigger and done when their trigger comes, and a learned spell
    /// is worn in the hold-spell slot. The first sixteen upgrades of Fire and Lightning are done by hand, by id, and
    /// their numbers are fields here.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    // It answers the Way's question of a mark (learned and in force: Has), so the armoury can build the weapon from it.
    public class PlayerUpgrades : MonoBehaviour, Way.IMarkQuery
    {
        /// <summary>Upgrades the player has from the start of the scene, for trying things out.</summary>
        public List<string> StartWith = new List<string>();
        /// <summary>Embers the player has from the start of the scene, for trying things out.</summary>
        public int StartingPoints;

        public Heat Heat = new Heat();
        /// <summary>True while the surge's clock is stopped: Scorched Earth is learned and the player stands in fire they left on the ground.</summary>
        public bool SurgeHeld { get; private set; }
        private bool scorched;
        private bool stormFrontOwed;
        /// <summary>The most charges Deep Reserve banks.</summary>
        public const int ReserveMost = 3;
        private int reserveIndex = -1;
        private float stormFrontAt;

        [Header("Fire")]
        public float BackdraftReach = 4.5f;
        public float BackdraftArc = 100f;
        public float BackdraftDamage = 16f;
        public float BackdraftStagger = 18f;
        public float TrailDamage = 4f;
        public float TrailSeconds = 2.6f;
        public float CrescentDamage = 18f;
        public float CrescentSpeed = 8f;
        public float CrescentSeconds = 1.1f;
        public float FirebombRange = 9f;
        public float FirebombDamage = 36f;
        public float FirebombStagger = 30f;
        public float FirebombRadius = 2.6f;

        [Header("Lightning")]
        public float StaticDamagePerStack = 7f;
        public float StaticStaggerPerStack = 5f;
        public float StaticRadius = 4f;
        public float StoredChargeMultiplier = 2f;
        public float BlinkDamage = 10f;
        public float BlinkStagger = 14f;
        public float AfterimageDamage = 26f;
        public float AfterimageStagger = 28f;
        public float AfterimageDelay = 0.3f;
        public float DelayedCutsShare = 0.6f;
        public float DelayedCutsDelay = 0.55f;
        public float OverclockSeconds = 2.5f;
        public float OverclockScale = 0.35f;
        public float PlasmaDamage = 18f;
        public float PlasmaStagger = 16f;
        public float PlasmaRadius = 3.2f;
        /// <summary>Seconds the shape left behind by a lightning dodge stays.</summary>
        public float AfterimageSeconds = 0.45f;

        [Header("The runner")]
        /// <summary>Metres within which an enemy counts as "in a fight" for effects that come every few seconds.</summary>
        public float FightRange = 12f;
        /// <summary>The share of health under which the low-health trigger comes.</summary>
        public float LowHealthShare = 1f / 3f;
        /// <summary>Seconds without a wound before the player counts as unhurt, and without a dodge before they count as planted.</summary>
        public float UnhurtSeconds = 8f;
        public float PlantedSeconds = 3f;
        /// <summary>The most weapon damage grey health held can add, where a mark makes it add any.</summary>
        public float GreyStrengthCap = 0.3f;
        /// <summary>White Heat: swing speed for each level of Heat above five, and what burns the player each second for four seconds when struck up there.</summary>
        public float WhiteHeatSwing = 0.05f;
        public float WhiteHeatBurn = 4f;
        public float WhiteHeatBurnSeconds = 4f;

        /// <summary>The player's numbers as the learned upgrades, the surge and timed boons make them.</summary>
        public PlayerStats Stats { get; } = new PlayerStats();
        /// <summary>The standard rules as the marks in force switch them (IMPLEMENTATION.md 3.10), rebuilt with the stats.</summary>
        public Growth.RuleBook Rules { get; } = new Growth.RuleBook();
        public int Points => RunState.Points;
        public int Bought { get; private set; }
        /// <summary>
        /// The spell in the North slot (Triangle, 2), or null. The old "worn spell" is retired: spells sit in the four
        /// slots of <see cref="PlayerSpells"/>. Kept for older probes, which wore a spell and cast it with a held button.
        /// </summary>
        public Upgrade Worn => spells != null ? spells.MarkIn(1) : null;
        public bool HasSpecial => Worn != null;
        /// <summary>A spell lent from outside the trees, to be tried: while one is lent it sits in the North slot.</summary>
        public Upgrade Lent { get; private set; }
        /// <summary>The learned spells in force now, in the chart's order: what the slots may hold and fill from.</summary>
        public IReadOnlyList<string> SpellsInForce => spellsInForce;
        /// <summary>Goes up whenever what is in force (or lent) may have changed; the slots look again then.</summary>
        public int SpellsVersion { get; private set; }
        private readonly List<string> spellsInForce = new List<string>();
        // What a spell branch makes of its spell (the shape worn, the Empower): asked as each spell is cast (Phase 4).
        private readonly SpellBranches branches = new SpellBranches();
        // Looked up when first wanted: the controller adds the slots to a player built before they existed, perhaps after this Awake.
        private PlayerSpells spellsFound;
        private PlayerSpells spells => spellsFound != null ? spellsFound : (spellsFound = GetComponent<PlayerSpells>());
        /// <summary>True while a perfect guard's charge waits on the next blow.</summary>
        public bool ChargeStored => combat != null && combat.NextBlowDamage > 1f;
        public bool ShowsHeat => hasHeat;
        // Counters so tests and probes can see what happened without watching the screen.
        public int Detonations { get; private set; }
        public int Discharges { get; private set; }
        public int Leaps { get; private set; }
        public int FlasksThrown { get; private set; }
        public int PlasmaBursts { get; private set; }
        public int FreeDodges { get; private set; }
        /// <summary>How many times a worn spell has been cast, and how many data-driven effects have been done.</summary>
        public int WornCasts { get; private set; }
        public int EffectsDone { get; private set; }
        /// <summary>How many enemies the Hold action has taken hold of.</summary>
        public int Holds { get; private set; }
        /// <summary>How many enemies the Lift action has thrown into the air.</summary>
        public int Lifts { get; private set; }
        /// <summary>Standing things fed a shard (Bellows, Wind Sickle): for probes.</summary>
        public int Fed { get; private set; }
        /// <summary>Goes up whenever a count kept by a mark changes; the HUD writes <see cref="TallyText"/> again then.</summary>
        public int TallyVersion { get; private set; }
        /// <summary>The counts kept by the marks in force, as the HUD shows them: "Plates 2/3".</summary>
        public string TallyText { get; private set; } = "";

        private enum Special { None, DeathBurst, Carry }

        private struct Pending
        {
            public UpgradeEffect Effect;
            public Special Special;
            public Component Target;
            public Vector3 At;
            public bool HasAt;
            public float Power;
            public float Radius;
            public AfflictionKind Kind;
            public int Stacks;
            // What the trigger carried: stacks spent or carried by the dead, or points of a count spent.
            public int Spent;
            // Stacks the root already lays with the same blow, not to be laid twice.
            public int Less;
            // What lays what it lays: a reaction's effects start no reaction (IMPLEMENTATION.md 3.7).
            public LaySource Source;
            // What its damage is multiplied by for a ripe or unripe seed it bursts (In Its Season); 0 is read as 1.
            public float Ripe;
            // The reaction whose own effect this is (its pair plus one; 0 none), and which time it went off.
            public int ReactionOf;
            public int ReactionCast;
            // Already handed to one enemy of several (UpgradeEffect.EachWithin): done as it stands.
            public bool Fanned;
            // For a condition's answer: the enemy the condition is joined to (Rot Roots' host), or null.
            public Component Link;
            // For an answer to a reaction (Reacted, ReactionStruck): which reaction, by its pair; -1 for none.
            public int Pair;
            // A reaction's blow struck again (StrikingAgain): what the trigger carried stands for what its Spend would count.
            public bool Again;
            // The number of the cast whose moment this answers (SpellCast.Cast; 0 none), for boons held by one cast.
            public int CastTag;
            // Where the spell's blow that set it going came from (its middle: a field's herb, a burst's centre), for a
            // condition that turns its carrier to face it.
            public Vector3 From;
        }

        // ---- Depth moved between enemies (the ledger's depth-moved).
        private const int MostStruck = 16;
        private static readonly int AfflictionKinds = System.Enum.GetValues(typeof(AfflictionKind)).Length;
        // The enemies the cast now running has raised to the sickest one's depth (Share Alike), emptied at each cast.
        private readonly List<Component> castStruck = new List<Component>(MostStruck);
        private readonly List<AfflictionSet> depthScratch = new List<AfflictionSet>(MostStruck);
        // The doses a line of bursts carries on to its next (One by One), or All Into One's gathered doses; emptied at each cast.
        private int lineCarried;
        // Boons held by one cast (UpgradeEffect.ThisCast), and the cast whose moment is being answered now.
        private readonly CastBoons castBoons = new CastBoons();
        private int answeringCast;
        // A spell begun and gathered ring by ring by the blows landed after it (Daybreak), and the numbers it is loosed with.
        private readonly RingGather rings = new RingGather();
        private readonly SpellSpec loosedSpec = new SpellSpec();

        /// <summary>True while a gathered spell waits to be loosed, and the rings it holds, for the HUD and probes.</summary>
        public bool Gathering => rings.Active;
        public int RingsHeld => rings.Rings;
        /// <summary>The spell gathering now (its mark), the most rings it takes, and what adds them: for the HUD's count by its slot.</summary>
        public Upgrade GatheringMark => rings.Mark;
        public int RingsMost => rings.Most;
        public RingSource GatheringBy => rings.By;
        /// <summary>Counts for probes: grey health given back by Return to the Shadow, and wounds undone by Second Shadow's Master.</summary>
        public float WoundsReturned { get; private set; }
        public int WoundsUndone { get; private set; }
        /// <summary>Counts for probes: gathered spells ended with nothing loosed (Bag of Winds burst as a whirlwind).</summary>
        public int RingsSpilled { get; private set; }
        /// <summary>Counts for probes: gathered spells loosed, and the rings the last one held.</summary>
        public int RingsLoosed { get; private set; }
        public int LastRingsLoosed { get; private set; }

        /// <summary>True when the cast numbered <paramref name="cast"/> was made critical by a mark (UpgradeEffect.ThisCast on CritChance).</summary>
        public bool CastCritical(int cast) => cast != 0 && castBoons.Multiplier(cast, PlayerStat.CritChance, Time.time) > 1f;
        // Whom the cast numbered castHitsOf struck, in order (The Yellow Gale shares its store among them).
        private readonly List<Component> castHits = new List<Component>(MostStruck);
        private int castHitsOf;
        /// <summary>Doses The Yellow Gale has drunk into its store and laid again, counted for probes.</summary>
        public int DosesStored { get; private set; }
        private readonly HashSet<IDamageable> gatherTaken = new HashSet<IDamageable>();
        private readonly HashSet<IDamageable> stateScratch = new HashSet<IDamageable>();
        /// <summary>Bolts thrown by grown things (Lightning Flower), counted for probes.</summary>
        public int GrownBoltsThrown { get; private set; }
        /// <summary>Grown things tended by a mark (Kindled Guard, The Host), counted for probes.</summary>
        public int GrownTended { get; private set; }
        // The death a helper was last raised for, in which frame and by which mark: one death raises one.
        private Transform helperFor;
        private int helperFrame = -1;
        private Upgrade helperOwner;
        // Where each grown thing a call burst stood, for the moments they are.
        private readonly Vector3[] grownBursts = new Vector3[16];
        /// <summary>What the edge carries for its next few weapon blows (Carried on the Blade, Dust on the Blade).</summary>
        public BladeCoat Coat { get; } = new BladeCoat();
        private readonly AfflictionKind[] coatKinds = new AfflictionKind[BladeCoat.Most];
        private readonly int[] coatStacks = new int[BladeCoat.Most];
        /// <summary>Counts for probes: enemies raised to the sickest one's depth, doses gathered into one burst, and doses pushed past a body's depth that burst it.</summary>
        public int DepthsRaised { get; private set; }
        public int DosesGathered { get; private set; }
        public int Overflows { get; private set; }
        /// <summary>Reactions struck again by a mark (Strikes Twice, Spreading Water), counted for probes.</summary>
        public int Echoes { get; private set; }
        // The enemy whose blow was last turned or dodged, and when: the one an answer meant only for it is done to.
        private Component turnedBy;
        private float turnedAt = -100f;
        /// <summary>Seconds after a blow is turned or dodged in which its attacker is still "the one whose blow it was".</summary>
        public const float AttackerWindow = 2f;

        /// <summary>A blow was turned or dodged; <paramref name="attacker"/> dealt it (null when it is not known: a bolt from afar).</summary>
        public void NoteTurned(Component attacker)
        {
            turnedBy = attacker;
            turnedAt = Time.time;
        }

        // The enemy whose blow last landed on you, and when.
        private Component struckBy;
        private float struckByAt = -100f;

        /// <summary>True when an attacker's blow came within <paramref name="window"/> seconds before <paramref name="now"/>.</summary>
        public static bool RecentAttacker(object attacker, float at, float window, float now) => attacker != null && now - at >= 0f && now - at <= window;

        /// <summary>True when <paramref name="target"/> is the one whose blow was turned or dodged at <paramref name="at"/>, still within the window.</summary>
        public static bool IsAttacker(object target, object turned, float at, float now) =>
            target != null && turned != null && ReferenceEquals(target, turned) && now - at <= AttackerWindow;

        /// <summary>Movement spells started as a blow would land, counted as perfect dodges, for probes.</summary>
        public int MovementDodges { get; private set; }
        /// <summary>Conditions ended on demand (Cold Snap), counted for probes.</summary>
        public int ConditionsEnded { get; private set; }
        // The reactions' adapter, which strikes a reaction again; looked up once.
        private PlayerReactions reactionsFound;
        // The reaction whose moment the trigger now being answered is (-1: none), put on each answer's line.
        private int triggerPair = -1;

        private readonly HashSet<IDamageable> fanTaken = new HashSet<IDamageable>();
        // Cherry Red's fan: the enemies already given a blade, or passed over, this throw; and where each blade called home stood.
        private readonly HashSet<IDamageable> plantExcept = new HashSet<IDamageable>();
        // Thrown pieces: the enemies already aimed at, and the ways a fan or ring leaves.
        private readonly HashSet<IDamageable> pieceExcept = new HashSet<IDamageable>();
        private readonly Vector3[] pieceWays = new Vector3[12];

        /// <summary>Pieces thrown out of enemies (feathers, flakes, blades, shards) and far-side cones: for probes.</summary>
        public int PiecesThrown { get; private set; }

        // The enemy a walking helper rose from: it goes to another.
        private readonly HashSet<IDamageable> helperExcept = new HashSet<IDamageable>();
        private readonly Vector3[] bladePlaces = new Vector3[PlantedBlades.Most];

        // One copy of the effect for each enemy near the target carrying what it asks, nearest first.
        private void FanOut(in Pending pending)
        {
            var effect = pending.Effect;
            Component from = pending.Target;
            Vector3 centre = from != null ? from.transform.position : pending.HasAt ? pending.At : transform.position;
            fanTaken.Clear();
            if (effect.EachOther && from is IDamageable self) fanTaken.Add(self);
            int most = effect.Count > 0 && effect.Do != UpgradeAction.Arc ? effect.Count : 24;
            int done = 0;
            for (int guard = 0; guard < 32 && done < most; guard++)
            {
                var near = combat.EnemyNear(centre, effect.EachWithin, fanTaken);
                if (!(near is IDamageable one)) break;
                fanTaken.Add(one);
                // The one that just died where it is done (a swollen enemy's burst) is no one to hand it to.
                if (near is Enemy fallen && fallen.IsDead) continue;
                if (!PlayerCombat.Carries(near, effect.OnlyCarrying)) continue;
                var each = pending;
                each.Target = near;
                each.At = near.transform.position;
                each.HasAt = true;
                each.Fanned = true;
                Enqueue(each);
                done++;
            }
            fanTaken.Clear();
        }

        // ---- The keystones of 2026-11-20 (keystones_fire_water_life.md).
        private bool hasIntoFire, drinksFire, hasTideClock, sharedRoots;
        private DodgeWay lastDodgeWay;
        private bool tideHigh = true;
        private float tideClock;
        // While a reaction's effects are put in the line, what a ripe (or unripe) seed on its enemy makes their damage.
        private float ripeScale = 1f;
        // Shared Roots: the wound shares owed to the Seeded enemies near, struck on this component's next step, not inside
        // the blow that wounded the player.
        private const int MostSharers = 8;
        private readonly Component[] sharers = new Component[MostSharers];
        private int sharerCount;
        private float sharedEach;
        private System.Func<float, float> shareWound;
        /// <summary>The Blade Drinks Fire: what each Burn drunk adds to a blow.</summary>
        public float DrinkDamage = 6f;
        /// <summary>Into the Fire: Burn laid on each enemy a dodge toward the enemy passes, and how wide the dodge's path is.</summary>
        public int IntoFireStacks = 2;
        public float IntoFireWidth = 1.1f;
        /// <summary>High Water, Low Water: seconds between turns of the tide.</summary>
        public float TideTurnSeconds = 6f;
        /// <summary>Shared Roots: the share of each wound the Seeded carry, and how near they must stand.</summary>
        public float SharedShare = 1f / 3f;
        public float SharedReach = 8f;
        /// <summary>
        /// Into the Fire, Drawn Back to Break: metres the player may get from the enemy a run of dodges began against before
        /// the next dodge is measured against a new one. The enemy is also let go when it dies.
        /// </summary>
        public float DodgeHoldRange = DodgeRules.HoldRange;
        /// <summary>The same: seconds without a dodge, from the last one, after which that enemy is let go.</summary>
        public float DodgeHoldSeconds = DodgeRules.HoldSeconds;
        // The enemy the first dodge of a run was measured against; later dodges keep to it (the owner's answer of 2026-11-20).
        private readonly DodgeHold<Transform> dodgeHold = new DodgeHold<Transform>();
        /// <summary>True while the tide is high; read by the HUD and probes while High Water, Low Water is in force.</summary>
        public bool TideHigh => tideHigh;
        public bool KeepsTide => hasTideClock;
        /// <summary>Counts for probes: tides turned, wound shares carried, ripe bursts, dodges that went through enemies.</summary>
        public int TidesTurned { get; private set; }
        public float WoundsCarried { get; private set; }
        public int RipeBursts { get; private set; }
        public int DodgesThrough { get; private set; }
        /// <summary>Which way the last dodge went against the enemy faced.</summary>
        public DodgeWay LastDodgeWay => lastDodgeWay;
        /// <summary>The enemy the dodges are measured against now, held from the first of the run; null when none is held. For probes.</summary>
        public Transform DodgeHeld => dodgeHold.Held;

        // The source of whatever the effect being done lays, and what the last Spend took (read by DamagePerSpent).
        private LaySource laying = LaySource.Effect;
        private int lastSpent;
        // Whose Spend it was and on whom: only a later effect of the same mark on the same enemy counts it.
        private Upgrade spentBy;
        private Component spentOn;
        private readonly HashSet<IDamageable> conditionScratch = new HashSet<IDamageable>();

        private const int QueueCapacity = 64;
        private static readonly int TriggerCount = System.Enum.GetValues(typeof(UpgradeTrigger)).Length;
        // Each affliction belongs to one element; an effect that lays another element's affliction is seen in that element's way.
        private static readonly ElementInfo[] lookOf = LooksByAffliction();

        private PlayerController player;
        private PlayerElement element;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private TopDownCamera shaker;
        private HitStop hitStop;
        private Afterimage afterimage;
        private IReadOnlyList<Upgrade> catalogue;
        private bool[] has;
        // Learned spells of the carried elements, which the slots may hold (a duo spell is in force only once slotted).
        private bool[] offered;
        // The first attunement as last refreshed: only its affliction lays with the weapon.
        private string refreshedFirst;
        // Static is built by the weapon only while Lightning is the first element; a heavy blow still sets it off.
        private bool staticLays;
        // The loadout is read from the player's build (added at play by the controller); without one, from the parts.
        private PlayerBuild buildFound;
        private PlayerBuild build => buildFound != null ? buildFound : (buildFound = GetComponent<PlayerBuild>());
        private readonly Build.Loadout ownLoadout = new Build.Loadout();
        private float[] readyAt;
        private float[] sinceLast;
        private List<UpgradeEffect>[] byTrigger;
        private readonly Pending[] queue = new Pending[QueueCapacity];
        private int queued;
        private bool flushing;
        private readonly List<IDamageable> toDischarge = new List<IDamageable>();
        private readonly List<IDamageable> toBurst = new List<IDamageable>();
        private bool hasHeat, hasDetonate, hasBackdraft, hasTrail, hasCrescent, hasFirebomb;
        // Keystones and marks written by hand on the night of 2026-11-16.
        private bool hasFurnace, hasStroke, hasCleanBreak;
        private readonly List<Component> furnaceList = new List<Component>(16);
        private readonly HashSet<IDamageable> furnaceSeen = new HashSet<IDamageable>();
        private readonly List<Component> toFinish = new List<Component>(4);
        private float firmUntil;
        private System.Func<float, bool> payInHealth;
        // Leaf on the Wind: breaths of wind pay for the dodge. They are the count the mark keeps.
        private bool hasLeaf;
        private int leafIndex = -1;
        private float leafClock;
        private int leafBlows;
        private System.Func<bool> spendBreath;
        // Master's Answer and Spring Tide.
        private bool hasAnswer, hasTide;
        // Deep Well: flasks fill a well that mends only while blows are landing. The mark's count shows it.
        private bool hasWell;
        private int wellIndex = -1;
        private float well;
        // Health owed by Venom's share of what the poison dealt, given back over time.
        private readonly Mending mending = new Mending();
        private float sinceBlowLanded = 999f;
        public float WellPerFlask = 70f;
        public float WellMost = 140f;
        public float WellPerSecond = 6f;
        public float WellBlowSeconds = 3f;
        /// <summary>Health waiting in the well.</summary>
        public float Well => well;
        private float answerUntil;
        /// <summary>Master's Answer: seconds after a perfect guard in which the weapon skill is free and comes at once, and what it costs at any other time.</summary>
        public float AnswerSeconds = 2f;
        public float AnswerDearer = 1.5f;
        /// <summary>Spring Tide: the most a spell grows (damage; reach by the square root), and how long the growth is held for the cast.</summary>
        public float TideMost = 2.5f;
        public float TideSeconds = 1.2f;
        /// <summary>How much greater the last Spring Tide spell was, and how many there have been, for tests and probes.</summary>
        public float LastTide { get; private set; }
        public int Tides { get; private set; }
        /// <summary>Leaf on the Wind: seconds for a breath to come back, and blows landed that win one.</summary>
        public float BreathSeconds = 2f;
        public int BreathBlows = 3;
        /// <summary>Breaths spent on dodges, for tests and probes.</summary>
        public int BreathsSpent { get; private set; }
        /// <summary>Heartwood: health asked for each point of mana a thing costs, and mana that mends one point of health.</summary>
        public float HeartwoodPrice = 0.5f;
        public float HeartwoodManaPerHealth = 3f;
        /// <summary>Health Heartwood has given in place of mana, for tests and probes.</summary>
        public float HeartwoodMended { get; private set; }
        /// <summary>Open Furnace: how far a perfect guard reaches for burning enemies, and the burn each keeps.</summary>
        public float FurnaceRadius = 8f;
        public int FurnaceKeeps = 2;
        /// <summary>Clean Break: a shatter kills what has less than this share of its health left (never a god).</summary>
        public float CleanBreakShare = 0.2f;
        /// <summary>Counts for probes and tests: enemies Open Furnace set off, discharges Single Stroke kept to one, lives Clean Break ended.</summary>
        public int FurnaceBursts { get; private set; }
        public int StrokeDischarges { get; private set; }
        public int CleanBreaks { get; private set; }
        private bool hasStatic, hasCharge, hasBlink, hasAfterimage, hasCuts, hasOverclock, hasStormStep, hasPlasma;
        private float untilRefresh;
        private Vector3 dodgeFrom;
        private ISpellLanding landingForm;
        private SpellCast landingCast;
        private int appliedVersion = -1;
        private int appliedAfflictions;
        private bool appliedSurge;
        private bool inFight;
        private float untilFightLook;
        private bool wasLow, wasFull;
        private float baseGuardChip = 0.35f;
        private readonly UpgradeEffect[] untilBlow = new UpgradeEffect[8];
        private readonly UpgradeEffect[] untilDodge = new UpgradeEffect[4];
        private int untilDodgeCount;
        private int untilBlowCount;
        private readonly HashSet<IDamageable> passExcept = new HashSet<IDamageable>();
        /// <summary>Doses passed on by the Pass action, counted for probes.</summary>
        public int Passed { get; private set; }
        private float riseStilledUntil;
        private bool deadAlready, hollowStrength;
        private float deadDrainOwed, untilDeadLook;
        private bool deadFight;
        private bool risen;
        /// <summary>Health a second the one who is Dead Already loses while an enemy is near, and seconds a kill stills a Risen drain with Hollow Strength.</summary>
        public float DeadAlreadyDrain = 2f;
        public float StilledSeconds = 3f;
        /// <summary>Points the Dead Already drain has taken, counted for probes.</summary>
        public int DeadDrained { get; private set; }
        private float emberCarry;
        // The numbers the stats multiply, as the scene set them.
        private bool basesRead;
        private float baseDodgeDistance, baseDodgeSafe, baseDodgeCost, baseDodgeWindow, baseGuardCost, baseGuardWindow;
        private float baseHealth, baseFlaskHeal, baseGuardMana, baseDodgeMana, baseGreyFade, baseMoveSpeed;
        private int baseFlasks;
        // The Body's numbers (task 5.4) are added to these, read once as the scene made them.
        private float baseStaminaMost, baseStaminaDelay, baseDodgeRecovery, baseHurtSeconds, baseKnockback, baseGreyDelay;
        private float sinceStruck = 99f, sinceDodge = 99f, sinceMoved;
        // A dodge begun on wet sand counts as on it until it has run (The Land Was Sea's slide and the sand it leaves).
        private bool dodgeFromSand;
        private const float DodgeThroughSeconds = 0.6f;
        private Vector3 lastStill;
        private float attackStamina = 1f;
        // The counts marks keep, by the mark's place in the catalogue, and which of the marks in force keep one.
        private int[] tally = new int[0];
        private readonly List<int> tallied = new List<int>(8);
        // When a count banked for a time empties by itself (Ring of Drums: the drums go with the ring); 0 for never.
        private float[] tallyEnds = new float[0];
        private bool tallyDirty;
        private bool hasRoot;
        private Upgrade castSource;
        private bool hasWhiteHeat;
        private int baseHeatLevels, baseHeatBlows;
        private float baseHeatCool;
        private float selfBurnLeft, selfBurnOwed;
        private readonly System.Text.StringBuilder tallyWords = new System.Text.StringBuilder(64);

        private static ElementInfo[] LooksByAffliction()
        {
            var looks = new ElementInfo[Afflictions.Count];
            foreach (var info in Elements.All) looks[(int)info.Affliction] = info;
            return looks;
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            element = GetComponent<PlayerElement>();
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            reactionsFound = GetComponent<PlayerReactions>();
            if (combat != null) combat.Stats = Stats;
            hitStop = GetComponent<HitStop>();
            afterimage = GetComponent<Afterimage>();
            byTrigger = new List<UpgradeEffect>[TriggerCount];
            for (int i = 0; i < byTrigger.Length; i++) byTrigger[i] = new List<UpgradeEffect>(8);
            CatalogueChanged();
            Heat.Reset();
            foreach (string id in StartWith) RunState.Unlocked.Add(id);
            RunState.Points += StartingPoints;
            Hazards.OwnedBy(combat);
        }

        private void OnEnable()
        {
            CombatEvents.Died += OnEnemyDied;
            CombatEvents.AfflictionFull += OnAfflictionFull;
            CombatEvents.Carried += OnCarried;
            CombatEvents.Laid += OnLaid;
            CombatEvents.Shattered += OnShattered;
            CombatEvents.Landed += OnLanded;
            CombatEvents.PoisonBit += OnPoisonBit;
            if (defence != null) defence.Revived += OnRevived;
        }

        private void OnDisable()
        {
            CombatEvents.Died -= OnEnemyDied;
            CombatEvents.AfflictionFull -= OnAfflictionFull;
            CombatEvents.Carried -= OnCarried;
            CombatEvents.Laid -= OnLaid;
            CombatEvents.Shattered -= OnShattered;
            CombatEvents.Landed -= OnLanded;
            CombatEvents.PoisonBit -= OnPoisonBit;
            if (defence != null) defence.Revived -= OnRevived;
        }

        private void Start()
        {
            var main = Camera.main;
            shaker = main != null ? main.GetComponent<TopDownCamera>() : null;
            ReadBases();
            Refresh(true);
            ApplyStats();
        }

        /// <summary>Looks at the catalogue again, as after a probe has added marks to it.</summary>
        public void CatalogueChanged()
        {
            catalogue = Upgrade.All;
            has = new bool[catalogue.Count];
            readyAt = new float[Upgrade.EffectCount];
            firedInFight = new int[Upgrade.EffectCount];
            for (int i = 0; i < firedInFight.Length; i++) firedInFight[i] = -1;
            castAt = new float[catalogue.Count];
            for (int i = 0; i < castAt.Length; i++) castAt[i] = -100f;
            sinceLast = new float[Upgrade.EffectCount];
            tally = new int[catalogue.Count];
            tallyEnds = new float[catalogue.Count];
            if (player != null && basesRead) Refresh(true);
        }

        /// <summary>
        /// A weapon has been taken up and has brought its own guard and its own dodge: those are now what the marks
        /// multiply. Without this the next change of a sum would put the first weapon's numbers back.
        /// </summary>
        public void WeaponChanged()
        {
            if (!basesRead || player == null) return;
            baseDodgeDistance = player.Dodge.Distance;
            baseDodgeSafe = player.Dodge.InvulnerableUntil;
            baseDodgeCost = player.Dodge.StaminaCost;
            baseDodgeRecovery = player.Dodge.Recovery;
            if (defence != null)
            {
                baseGuardCost = defence.Guard.StaminaPerDamage;
                baseGuardWindow = defence.Guard.PerfectWindow;
                baseGuardChip = defence.Guard.ChipFraction;
            }
            appliedVersion = Stats.Version - 1;
            ApplyStats();
        }

        private void ReadBases()
        {
            basesRead = true;
            baseHeatLevels = Heat.MaximumLevel;
            baseHeatBlows = Heat.BlowsPerLevel;
            baseHeatCool = Heat.CoolAfter;
            baseDodgeDistance = player.Dodge.Distance;
            baseDodgeSafe = player.Dodge.InvulnerableUntil;
            baseDodgeCost = player.Dodge.StaminaCost;
            baseDodgeRecovery = player.Dodge.Recovery;
            baseMoveSpeed = player.MoveSpeed;
            baseStaminaMost = player.Stamina.Maximum;
            baseStaminaDelay = player.Stamina.RegenerationDelay;
            if (defence == null) return;
            baseHurtSeconds = defence.HurtSeconds;
            baseKnockback = defence.Knockback;
            baseGreyDelay = defence.Health.GreyDelay;
            baseDodgeWindow = defence.PerfectDodgeWindow;
            baseGuardCost = defence.Guard.StaminaPerDamage;
            baseGuardWindow = defence.Guard.PerfectWindow;
            baseGuardChip = defence.Guard.ChipFraction;
            baseHealth = defence.Health.Maximum;
            baseFlaskHeal = defence.Flasks.HealAmount;
            baseFlasks = defence.Flasks.Maximum;
            baseGuardMana = defence.PerfectGuardMana;
            baseDodgeMana = defence.PerfectDodgeMana;
            baseGreyFade = defence.Health.GreyFadePerSecond;
        }

        /// <summary>True when the upgrade is learned and belongs to the element and weapon the player holds now.</summary>
        public bool Has(string id)
        {
            if (!RunState.Unlocked.Contains(id)) return false;
            return InForce(Upgrade.ById(id));
        }

        // The loadout decides what is in force (IMPLEMENTATION.md 3.6): the attunements, the slots, the weapon kind.
        private bool InForce(Upgrade upgrade) => MarkRules.InForce(upgrade, ReadLoadout());

        private Build.Loadout ReadLoadout() => build != null ? build.Read() : PlayerBuild.Fill(ownLoadout, element, spells, combat);

        /// <summary>Works out what is in force at once, as after the Hand or the attunements change at a rest fire.</summary>
        public void RefreshNow() => Refresh(true);

        /// <summary>True when the mark is learned; an element's base spell (<c>fire.base</c>) is owned while its element is carried.</summary>
        public bool Owns(string id) =>
            RunState.Unlocked.Contains(id) || (Magic.SpellSlots.IsBase(id) && element != null && element.Carries(Build.HandRules.ElementOfBase(id)));

        /// <summary>The number of seconds a slot waits after this spell now, its branch's shape and Empower counted (PlayerSpells starts the clock).</summary>
        public float CooldownOf(Upgrade mark) => branches.CooldownOf(mark, RunState.Unlocked, ReadLoadout());

        /// <summary>The wait of an element's base spell cast by its kit, from <paramref name="usual"/>, with its branch's Empower.</summary>
        public float BaseCooldownOf(string baseId, float usual) => SpellBranches.BaseCooldown(baseId, usual, RunState.Unlocked);

        /// <summary>True when a base spell is cast through its Learn mark (its worn shape casts its own form), not by its element's kit.</summary>
        public bool BaseCastsThroughMark(string baseId) =>
            SpellBranches.CastsThroughMark(Upgrade.ById(baseId), RunState.Unlocked, ReadLoadout());

        /// <summary>True when a learned spell may sit in a slot now: its element (both, for a duo spell) carried.</summary>
        public bool CanSlot(string id)
        {
            var upgrade = Upgrade.ById(id);
            return upgrade != null && Owns(id) && MarkRules.Slottable(upgrade, ReadLoadout());
        }

        /// <summary>The name of the element whose tree the player can learn from now: the first attunement's.</summary>
        public string ElementName => element != null && element.Kit != null ? element.Kit.Name : ElementKit.FireName;

        /// <summary>
        /// True when the mark is on a tree or branch open to the player now: a carried element's, or a mix of one with an
        /// element held or consumed in this run.
        /// </summary>
        public bool Open(Upgrade upgrade)
        {
            // A duo branch in the chart's heart, of any pair: open while both its elements are carried (task 4.5).
            if (upgrade != null && upgrade.Tree == MarkTree.Duo) return MarkRules.Open(upgrade, ReadLoadout(), PlaytestMode.On);
            // The playtest's free chart: every tree and branch is open, whatever is held.
            if (MarkRules.Open(upgrade, ElementName, RunState.ElementsHeld, RunState.ElementsConsumed, PlaytestMode.On)) return true;
            return element != null && element.Mixed && MarkRules.Open(upgrade, element.SecondInfo.Name, RunState.ElementsHeld, RunState.ElementsConsumed, false);
        }

        /// <summary>True when the mark can be bought now. A god's gift is never sold, except in the playtest, where it is a playtest unlock.</summary>
        public bool CanBuy(Upgrade upgrade)
        {
            // A mark priced in Memories is asked of the save's wallet and the Way's own opening; the save is read only then.
            if (upgrade != null && upgrade.Currency == Currency.Memories)
            {
                var data = Progress.Data;
                bool open = upgrade.IsWay ? Way.WayRules.Open(upgrade, data.OwnedWeapons, PlaytestMode.On) : Open(upgrade);
                return MarkRules.CanBuyInItsCurrency(upgrade, RunState.Unlocked, RunState.Points, data.Wallet, open, PlaytestMode.On, Progress.Has, data.GodsFelled);
            }
            return MarkRules.CanBuyInItsCurrency(upgrade, RunState.Unlocked, RunState.Points, null, Open(upgrade), PlaytestMode.On, null, 0);
        }

        // What a reaction's own damage (or reach) is multiplied by: 1 for an effect that is no reaction's (its plain mark or a duo shape).
        private float ReactionScale(Upgrade owner, PlayerStat stat = PlayerStat.ReactionDamage)
        {
            if (owner == null) return 1f;
            int pair = owner.Kind == UpgradeKind.Reaction ? ReactionTable.PairOf(owner.Id) : ReactionTable.IsReactionShape(owner) ? HeartLayout.PairOf(owner) : -1;
            return pair < 0 ? 1f : Stats.ReactionMultiplier(stat, AfflictionPairs.First(pair), AfflictionPairs.Second(pair));
        }

        /// <summary>True when What the Yew Keeps is in force and the dead carried both its seed and its curse: no thorns burst.</summary>
        private bool YewWithholds(AfflictionSet dead) => YewWithholds(Has(Trees.Duo.GirdledDuo.WhatTheYewKeeps), dead);

        /// <summary>The Yew's rule alone: in force, and the dead Seeded and Cursed.</summary>
        public static bool YewWithholds(bool inForce, AfflictionSet dead) =>
            inForce && dead != null && dead.Has(AfflictionKind.Seeded) && dead.Has(AfflictionKind.Cursed);

        /// <summary>How deep the kind lies on the one named, for an effect done by that depth (Night Sun); 0 for none or no one.</summary>
        public static int DepthOn(AfflictionKind kind, IAfflictable on) => on != null && kind != AfflictionKind.None ? on.Afflictions.Stacks(kind) : 0;

        /// <summary>A rest fire: the counts that are a fight's growth (Folded Steel's folds) are emptied.</summary>
        public void Rested()
        {
            if (tally == null) return;
            if (TallySpec.EmptyAtRest(catalogue, tally, RunState.Tallies) > 0) tallyDirty = true;
        }

        /// <summary>Buys the mark, paid in its own currency: Memories from the wallet, Shards from the run's purse.</summary>
        public bool TryBuy(Upgrade upgrade)
        {
            if (!CanBuy(upgrade)) return false;
            if (upgrade.Currency == Currency.Memories)
            {
                if (!Progress.Data.Wallet.Pay(upgrade)) return false;
            }
            else RunState.Points -= upgrade.Cost;
            // Learned; a first aspect or keystone switches on in the live loadout at once, and is kept for the run.
            var loadout = ReadLoadout();
            if (MarkRules.Learn(upgrade, RunState.Unlocked, loadout)) RunState.WayChoices.CopyWayFrom(loadout);
            Bought++;
            // A spell learned goes into an empty slot, if there is one, when the slots next look (PlayerSpells).
            Refresh(true);
            // A mark of the Way changes the weapon itself: it is built again from the armoury once the swing is over.
            if (upgrade.Tree == MarkTree.Way && combat != null) combat.Rearm();
            return true;
        }

        /// <summary>
        /// A god's gift, or anything else given and not bought: it is learned at no cost, and a gift that is a spell is
        /// worn at once. False when there was nothing to give or it was already held.
        /// </summary>
        public bool Give(Upgrade gift)
        {
            if (gift == null || RunState.Unlocked.Contains(gift.Id)) return false;
            RunState.Unlocked.Add(gift.Id);
            Refresh(true);
            if (gift.Tree == MarkTree.Way && combat != null) combat.Rearm();
            // A gift that is a spell is put in a slot at once: the first empty one, else the North slot.
            if (gift.IsSpell && spells != null) spells.Place(gift.Id);
            return true;
        }

        /// <summary>
        /// Puts a learned spell in the North slot (Triangle, 2). False when it is not a spell the player has. For older
        /// probes that wore a spell; the chart puts spells in the next slot round (<see cref="PutInNextSlot"/>).
        /// </summary>
        public bool Wear(string id)
        {
            var upgrade = Upgrade.ById(id);
            if (upgrade == null || !upgrade.IsSpell || !CanSlot(id) || spells == null) return false;
            spells.SetSlot(1, id);
            return true;
        }

        /// <summary>The chart's stopgap until the Hand exists: a learned spell goes into the next slot round. The slot, or -1.</summary>
        public int PutInNextSlot(string id)
        {
            var upgrade = Upgrade.ById(id);
            if (upgrade == null || !upgrade.IsSpell || !CanSlot(id) || spells == null) return -1;
            return spells.PutInNext(id);
        }

        /// <summary>
        /// Lends a spell that is on no tree, in the hold-spell slot, until another is lent or null is given: the yard's
        /// rack of forms uses it. Nothing about the run is written down.
        /// </summary>
        public void Lend(Upgrade spell)
        {
            Lent = spell != null && spell.Spell != null ? spell : null;
            Refresh(true);
            if (Lent != null && spells != null) spells.SetSlot(1, Lent.Id);
        }

        /// <summary>Embers from the fallen; the upgrades may make them count for more.</summary>
        public void Earn(int points)
        {
            if (points <= 0) return;
            float worth = points * Stats.Multiplier(PlayerStat.EmbersEarned) + emberCarry;
            int whole = Mathf.FloorToInt(worth + 0.0001f);
            emberCarry = worth - whole;
            RunState.Points += whole;
        }

        /// <summary>
        /// What the marks in force take off the wait of an element's base spell (stat BaseSpellCooldown): that element's
        /// own marks only. Asked once a cast, so the catalogue is read here rather than kept summed.
        /// </summary>
        public float BaseCooldownShare(string elementName)
        {
            if (catalogue == null || has == null || string.IsNullOrEmpty(elementName)) return 0f;
            float share = 0f;
            for (int i = 0; i < has.Length && i < catalogue.Count; i++)
                if (has[i]) share += Magic.SpellTimings.BaseCooldownShare(catalogue[i], elementName);
            return share;
        }

        /// <summary>Embers given back as they were spent, ember for ember.</summary>
        public void Refund(int points)
        {
            if (points > 0) RunState.Points += points;
        }

        /// <summary>Taking a new element leaves the old one's learning behind. The Way's and the Body's marks are kept: Memories are never lost.</summary>
        public void Forget()
        {
            RunState.Unlocked.RemoveWhere(id => !(Upgrade.ById(id)?.IsWay ?? false));
            Heat.Reset();
            if (combat != null)
            {
                combat.NextBlowDamage = 1f;
                combat.NextBlowStagger = 1f;
            }
            Refresh(true);
        }

        // What is in force is worked out a few times a second, not asked for by name on every blow; the lists the
        // runner reads are only made again when something has changed.
        private void Refresh(bool force = false)
        {
            untilRefresh = 0.25f;
            bool changed = force;
            // What is in force is the loadout's to say (MarkRules.InForce): read once for the whole catalogue.
            var loadout = ReadLoadout();
            if (offered == null || offered.Length != has.Length) offered = new bool[has.Length];
            for (int i = 0; i < has.Length; i++)
            {
                var mark = catalogue[i];
                bool learned = RunState.Unlocked.Contains(mark.Id);
                bool now = MarkRules.InForce(mark, loadout, RunState.Unlocked);
                // A learned spell of the carried elements may sit in a slot even while it is not in force (a duo spell not slotted yet).
                bool canSlot = learned && MarkRules.Slottable(mark, loadout);
                if (now == has[i] && canSlot == offered[i]) continue;
                has[i] = now;
                offered[i] = canSlot;
                changed = true;
            }
            // Only the first element lays its affliction with the weapon: a change of which element is first changes that.
            if (loadout.First != refreshedFirst) changed = true;
            if (!changed) return;
            refreshedFirst = loadout.First;
            string first = refreshedFirst;

            for (int i = 0; i < byTrigger.Length; i++) byTrigger[i].Clear();
            Stats.ClearLearned();
            Rules.Reset();
            int weaponStacks = 0;
            spellsInForce.Clear();
            for (int i = 0; i < offered.Length; i++)
                if (offered[i]) spellsInForce.Add(catalogue[i].Id);
            tallied.Clear();
            hasRoot = false;
            // The Blade Drinks Fire: Fire's root lays nothing with the weapon (root-silenced); the blade drinks instead.
            var drinkMark = Upgrade.ById(Trees.FireTree.BladeDrinksFire);
            drinksFire = drinkMark != null && drinkMark.Index < has.Length && has[drinkMark.Index];
            for (int i = 0; i < has.Length; i++)
            {
                // A count is lost with the mark that kept it.
                if (!has[i])
                {
                    tally[i] = 0;
                    continue;
                }
                var upgrade = catalogue[i];
                // The root that lays with the weapon is the first element's alone (the owner's rule of 2026-11-18), and a
                // silenced root lays nothing at all.
                bool silenced = drinksFire && upgrade.Kind == UpgradeKind.Root && upgrade.Element == Elements.Fire;
                if (upgrade.Kind == UpgradeKind.Root && upgrade.Element == first && !silenced) hasRoot = true;
                if (upgrade.Tally != null)
                {
                    tallied.Add(i);
                    // A count kept for the run comes back with the scene.
                    if (upgrade.Tally.Kept && tally[i] == 0 && RunState.Tallies.TryGetValue(upgrade.Id, out int kept)) tally[i] = kept;
                }
                foreach (var mod in upgrade.Stats) Stats.AddLearned(mod);
                // A Learn's effects, but for those its worn shape sets aside (Golden Sickle keeps the garden standing).
                var asideBy = upgrade.Part == BranchPart.Learn ? WornOver(upgrade, loadout) : null;
                var own = EffectsOf(upgrade);
                for (int e = 0; e < own.Length; e++)
                    if (!ShapeDef.SetAside(asideBy, e)) byTrigger[(int)own[e].On].Add(own[e]);
                Rules.Apply(upgrade);
                // A Shape mark adds the rule of the shape its spell wears, and only that one.
                var worn = SpellBranches.Worn(upgrade, loadout);
                if (worn != null)
                {
                    foreach (var mod in worn.Stats) Stats.AddLearned(mod);
                    foreach (var effect in worn.Effects) byTrigger[(int)effect.On].Add(effect);
                }
                // Weapon blows lay only the first element's affliction: a second element's root lays nothing with the weapon.
                if (!upgrade.IsMix && upgrade.Element == first && !silenced) weaponStacks += upgrade.WeaponStacks;
            }
            // The Body's found pieces add on top, always in force and kept for good (task 5.4).
            foreach (var mod in Way.BodyPieces.StatsOf(Progress.Data.Found)) Stats.AddLearned(mod);
            Rules.Publish();
            // The slots look again: a spell no longer in force leaves its slot, a new one fills an empty slot.
            SpellsVersion++;

            hasHeat = On(Upgrade.Heat);
            // Poison's beat as the marks in force make it, and the two states of the dead.
            Afflictions.PoisonBeatOff = Has(Trees.PoisonTree.Volatile);
            Afflictions.PoisonQuickWhenSoaked = Has(Trees.Duo.FishPoisonDuo.FastWater);
            Afflictions.PoisonKeeps = Has(Trees.PoisonTree.SlowDeath);
            Afflictions.PoisonRipens = Has(Trees.PoisonTree.LeftToRipen) ? 0.25f : 0f;
            Afflictions.PoisonOnCursed = Has(Trees.BlightTree.Wither) ? 0.25f : 0f;
            Afflictions.SharedBeat = Has(Trees.PoisonTree.OneSickness);
            Afflictions.SharedBeatShare = 0.15f;
            deadAlready = Has(Trees.DarkTree.DeadAlready);
            hollowStrength = Has(Trees.DarkTree.HollowStrength);
            // Burn and Detonate is folded into Breath of the Lioness (once Backdraft, the same id): the guard's cone sets a burn off too.
            hasDetonate = On(Upgrade.BurnAndDetonate) || On(Upgrade.Backdraft);
            hasBackdraft = On(Upgrade.Backdraft);
            hasTrail = On(Upgrade.FlameTrail);
            hasCrescent = On(Upgrade.BurningCrescent);
            hasFirebomb = On(Upgrade.FirebombFlask);
            hasStatic = On(Upgrade.Static);
            staticLays = hasStatic && first == Elements.Lightning;
            hasCharge = On(Upgrade.StoredCharge);
            // Deep Reserve is folded into Stored Charge, which keeps the count of three charges itself.
            var reserve = Upgrade.ById(Upgrade.StoredCharge);
            reserveIndex = hasCharge && reserve != null && reserve.Tally != null ? reserve.Index : -1;
            hasBlink = On(Upgrade.Blink);
            hasAfterimage = On(Upgrade.Afterimage);
            hasCuts = On(Upgrade.DelayedCuts);
            hasOverclock = On(Upgrade.Overclock);
            hasStormStep = On(Upgrade.StormStep);
            hasPlasma = On(Upgrade.Plasma);
            hasWhiteHeat = On(Trees.FireTree.WhiteHeat);
            hasIntoFire = On(Trees.FireTree.IntoTheFire);
            // Clear Sky: a Mark (anything spent by blows) lasts one blow longer on a burning enemy.
            Afflictions.MoreBlowsBurning = On(Trees.Duo.KindlingPointDuo.ClearSky) ? 1 : 0;
            Afflictions.BurnBiteOff = drinksFire;
            if (combat != null) combat.DrinksBurn = drinksFire ? DrinkDamage : 0f;
            bool tideNow = On(Trees.WaterTree.HighWaterLowWater);
            if (tideNow != hasTideClock)
            {
                hasTideClock = tideNow;
                tideHigh = true;
                tideClock = 0f;
            }
            sharedRoots = On(Trees.LifeTree.SharedRoots);
            if (combat != null) combat.SeedMendingOff = sharedRoots;
            if (defence != null)
            {
                if (shareWound == null) shareWound = ShareWound;
                defence.WoundShare = sharedRoots ? SharedShare : 0f;
                defence.ShareWound = sharedRoots ? shareWound : null;
            }
            hasFurnace = On(Trees.FireTree.OpenFurnace);
            hasStroke = On(Trees.LightningTree.SingleStroke);
            hasCleanBreak = On(Trees.IceTree.CleanBreak);
            Afflictions.ChillSlowOff = On(Trees.IceTree.AllAtOnce);
            Afflictions.BrittleSeconds = On(Trees.IceTree.Brittle) ? 5f : 0f;
            Afflictions.BuckledShare = On(Trees.ForgeTree.Buckled) ? 0.4f : 0f;
            // Flash Freeze is retired (task 4.15): Supercooled freezes a soaked enemy now, so no mark lowers the freeze.
            Afflictions.FlashFreezeAt = 0;
            hasAnswer = On(Trees.MetalTree.MastersAnswer);
            hasTwoSuns = On(Trees.LightTree.TwoSuns);
            if (!hasTwoSuns) echoForm = null;
            bool wellNow = On(Trees.WaterTree.DeepWell);
            if (wellNow != hasWell)
            {
                hasWell = wellNow;
                var wellMark = Upgrade.ById(Trees.WaterTree.DeepWell);
                wellIndex = wellMark != null ? wellMark.Index : -1;
                well = 0f;
                // The flask's own healing is set where the stats are applied: have them applied again.
                appliedVersion = -1;
            }
            hasTide = On(Trees.WaterTree.SpringTide);
            bool leaf = On(Trees.WindTree.LeafOnTheWind);
            if (leaf != hasLeaf && player != null)
            {
                hasLeaf = leaf;
                var mark = Upgrade.ById(Trees.WindTree.LeafOnTheWind);
                leafIndex = mark != null ? mark.Index : -1;
                if (spendBreath == null) spendBreath = SpendBreath;
                player.Dodge.PayInstead = leaf && leafIndex >= 0 ? spendBreath : null;
                if (leaf && leafIndex >= 0 && leafIndex < tally.Length)
                {
                    tally[leafIndex] = mark.Tally != null ? mark.Tally.Cap : 3;
                    tallyDirty = true;
                }
                leafClock = 0f;
                leafBlows = 0;
            }
            if (combat != null)
            {
                combat.CounterCut = On(Trees.MetalTree.CounterCut);
                // Tasted Every Herb: no poison is laid on you, the enemy's clouds' included.
                if (defence != null) defence.Afflictions.ImmuneTo = On(Trees.PoisonTree.TastedEveryHerb) ? AfflictionKind.Poison : AfflictionKind.None;
                heavyUnbrokenByMarks = On(Trees.IceTree.Unmoved) || On(Trees.EarthTree.HeavyHand);
                combat.HeavyUnbroken = heavyUnbrokenByMarks || Time.time < heavyUnbrokenUntil;
                // Heartwood: no mana. Every price is asked in health and every gain mends.
                bool heart = On(Trees.LifeTree.Heartwood);
                if (heart != combat.Mana.Sealed)
                {
                    if (payInHealth == null) payInHealth = PayInHealth;
                    combat.Mana.Empty();
                    combat.Mana.Instead = heart ? payInHealth : null;
                    combat.Mana.TakeDiverted();
                }
            }

            if (combat != null)
            {
                // The burn of a blow is Cinder's now (the root lays it as every root does), so nothing is laid by hand here.
                combat.BurnStacksOnHit = 0;
                combat.WeaponAffliction = element != null && element.Info != null ? element.Info.Affliction : AfflictionKind.None;
                combat.WeaponAfflictionStacks = weaponStacks;
            }
            // The flask can be thrown once anything answers its being thrown.
            if (defence != null) defence.HoldToThrow = hasFirebomb || byTrigger[(int)UpgradeTrigger.FlaskThrown].Count > 0;
            if (!hasHeat) Heat.Reset();
            ApplyTallies();
        }

        // The counts are few and change seldom: their words and what they add to the stats are made again when one moves.
        // A count banked for a time empties once its time is up (the ring stands its time and takes its drums with it).
        private void EmptyTimedTallies(float now)
        {
            bool emptied = false;
            for (int i = 0; i < tallied.Count; i++)
            {
                int index = tallied[i];
                if (index >= tallyEnds.Length || !TallyRunsOut(tallyEnds[index], now)) continue;
                tallyEnds[index] = 0f;
                if (tally[index] == 0) continue;
                tally[index] = 0;
                emptied = true;
            }
            if (emptied) ApplyTallies();
        }

        /// <summary>True when a count banked to empty at <paramref name="endsAt"/> (0: never) has run out at <paramref name="now"/>.</summary>
        public static bool TallyRunsOut(float endsAt, float now) => endsAt > 0f && now >= endsAt;

        private void ApplyTallies()
        {
            tallyDirty = false;
            Stats.ClearTally();
            tallyWords.Length = 0;
            for (int i = 0; i < tallied.Count; i++)
            {
                var spec = catalogue[tallied[i]].Tally;
                int held = tally[tallied[i]];
                if (spec.PerPoint != 0f && held > 0) Stats.AddTally(spec.Stat, spec.PerPoint * held);
                if (spec.PerPoint2 != 0f && held > 0) Stats.AddTally(spec.Stat2, spec.PerPoint2 * held);
                // While the count is empty (Drawn Back to Break with no wave drawn).
                if (spec.EmptyAmount != 0f && held == 0) Stats.AddTally(spec.EmptyStat, spec.EmptyAmount);
                if (spec.Kept) RunState.Tallies[catalogue[tallied[i]].Id] = held;
                tallyWords.Append("   ").Append(spec.Name).Append(' ').Append(held).Append('/').Append(spec.Cap);
            }
            TallyText = tallyWords.ToString();
            TallyVersion++;
        }

        /// <summary>
        /// A mark's effects as they stand now: a Learn's twins with its Empower's numbers while that Empower is in force
        /// (<see cref="Upgrade.EmpoweredEffects"/>), else its own. What a condition it set does is read through this too.
        /// </summary>
        // The shape worn over a Learn, when its Shape mark is learned and in force; null otherwise.
        private ShapeDef WornOver(Upgrade learn, Build.Loadout loadout)
        {
            var shapeMark = Upgrade.ById(SpellBranch.ShapeId(learn.Branch.Length > 0 ? learn.Branch : learn.Id));
            if (shapeMark == null || has == null || shapeMark.Index < 0 || shapeMark.Index >= has.Length || !has[shapeMark.Index]) return null;
            return SpellBranches.Worn(shapeMark, loadout);
        }

        public UpgradeEffect[] EffectsOf(Upgrade mark)
        {
            if (mark == null) return System.Array.Empty<UpgradeEffect>();
            var empower = mark.EmpowerMark;
            if (mark.EmpoweredEffects != null && empower != null && has != null && empower.Index < has.Length && has[empower.Index]) return mark.EmpoweredEffects;
            return mark.Effects;
        }

        /// <summary>
        /// A reaction's effects as they run now: the plain reaction's (<paramref name="shape"/> -1) or a shape's, with the
        /// numbers of every mark in force that changes that reaction (<see cref="Upgrade.ReactionChanges"/>), else as written.
        /// </summary>
        public UpgradeEffect[] ReactionEffects(int pair, int shape)
        {
            var written = ReactionNumbers.Written(pair, shape);
            var changers = ReactionNumbers.Changers(pair);
            if (changers == null || has == null) return written;
            int mask = 0;
            for (int i = 0; i < changers.Length; i++)
            {
                int index = changers[i].Index;
                if (index < has.Length && has[index]) mask |= 1 << i;
            }
            return mask == 0 ? written : ReactionNumbers.Changed(pair, mask, shape) ?? written;
        }

        /// <summary>The count a mark keeps now; zero when it keeps none or is not in force.</summary>
        public int Tally(string id)
        {
            var upgrade = Upgrade.ById(id);
            return upgrade != null && upgrade.Index < tally.Length ? tally[upgrade.Index] : 0;
        }

        private bool On(string id)
        {
            for (int i = 0; i < has.Length; i++)
                if (has[i] && catalogue[i].Id == id) return true;
            return false;
        }

        // The stats are pushed into the systems that use them, and only when a sum has changed.
        private void ApplyStats()
        {
            bool surging = element != null && element.SurgeActive;
            // What is laid on the player changes its numbers too (PlayerAfflictions).
            int afflicted = defence != null ? defence.Afflictions.Version : 0;
            if (!basesRead || (Stats.Version == appliedVersion && surging == appliedSurge && afflicted == appliedAfflictions)) return;
            appliedAfflictions = afflicted;
            appliedVersion = Stats.Version;
            appliedSurge = surging;
            float surgeSpeed = surging ? Stats.Multiplier(PlayerStat.SurgeSpeed) : 1f;
            player.MoveSpeed = baseMoveSpeed * Stats.Multiplier(PlayerStat.MoveSpeed) * surgeSpeed * (defence != null ? defence.Afflictions.MoveScale : 1f);
            // Afflictions laid on the player last as long as the stat says (Tremor: half as long).
            if (defence != null) defence.Afflictions.SecondsScale = Mathf.Max(0f, Stats.Multiplier(PlayerStat.AfflictionsOnYou));
            player.Dodge.Distance = baseDodgeDistance * Stats.Multiplier(PlayerStat.DodgeDistance) + Stats.Sum(PlayerStat.DodgeMetres);
            // The Body (task 5.4): stamina, how soon it returns, the dodge's wait and its flow.
            player.Stamina.Maximum = Mathf.Max(1f, baseStaminaMost + Stats.Sum(PlayerStat.StaminaMost));
            player.Stamina.RegenerationDelay = Mathf.Max(0f, baseStaminaDelay + Stats.Sum(PlayerStat.StaminaDelay));
            player.Dodge.Recovery = Mathf.Max(0f, baseDodgeRecovery + Stats.Sum(PlayerStat.DodgeRecovery) + (defence != null ? defence.Afflictions.DodgeRecoveryAdd : 0f));
            player.Dodge.Flows = Mathf.Max(0, Stats.Whole(PlayerStat.DodgeFlow));
            player.Dodge.InvulnerableUntil = baseDodgeSafe * Stats.Multiplier(PlayerStat.DodgeSafeTime);
            player.Dodge.StaminaCost = baseDodgeCost * Stats.Multiplier(PlayerStat.StaminaCost) * Stats.Multiplier(PlayerStat.DodgeStamina);
            attackStamina = Stats.Multiplier(PlayerStat.AttackStamina);
            Afflictions.MoreBlows = Mathf.Max(0, Stats.Whole(PlayerStat.MarkBlows));
            Afflictions.FreezeSeconds = Stats.Multiplier(PlayerStat.FreezeSeconds);
            Afflictions.ShatterDamage = Stats.Multiplier(PlayerStat.ShatterDamage);
            Hazards.GrownSecondsScale = Stats.Multiplier(PlayerStat.GrownSeconds);
            scorched = Has(Trees.FireTree.ScorchedEarth);
            Hazards.FireSecondsScale = scorched ? 2f : 1f;
            Hazards.GrownDamageScale = Stats.Multiplier(PlayerStat.GrownDamage);
            // Wood Grows Back: what you grow grows back six seconds after it is spent.
            Hazards.GrowsBackAfter = Has(Trees.LifeTree.WoodGrowsBack) ? Trees.LifeTree.GrowsBackSeconds : 0f;
            // What every grown thing lays with its bites: a notch (Ore in the Root), a Chill (Hidden in the Tree).
            Hazards.GrownBitesMask = (Has(Trees.Duo.IronThornsDuo.OreInTheRoot) ? 1 << (int)AfflictionKind.Sundered : 0)
                | (Has(Trees.Duo.WinterSeedDuo.HiddenInTheTree) ? 1 << (int)AfflictionKind.Chill : 0);
            // Heat as the marks make it: sooner, slower to cool, higher.
            Heat.BlowsPerLevel = Mathf.Max(1, baseHeatBlows - Stats.Whole(PlayerStat.HeatBlows));
            Heat.CoolAfter = baseHeatCool * Stats.Multiplier(PlayerStat.HeatCooling);
            Heat.MaximumLevel = baseHeatLevels + Mathf.Max(0, Stats.Whole(PlayerStat.HeatCap));
            // The held element's affliction may lie deeper than usual.
            Afflictions.SetDeeper(element != null && element.Info != null ? element.Info.Affliction : AfflictionKind.None, Stats.Whole(PlayerStat.AfflictionDepth));
            if (combat != null)
            {
                combat.StatDamage = Stats.Multiplier(PlayerStat.WeaponDamage);
                combat.StatPush = Stats.Multiplier(PlayerStat.PushDistance);
                combat.StatStagger = Stats.Multiplier(PlayerStat.StaggerDealt);
                combat.StatMana = Stats.Multiplier(PlayerStat.ManaFromBlows);
                combat.StatSwing = Stats.Multiplier(PlayerStat.SwingSpeed) * surgeSpeed;
                combat.StatPoise = Mathf.Max(0f, Stats.Sum(PlayerStat.Poise));
                combat.StatAfflicted = Stats.Multiplier(PlayerStat.DamageToAfflicted);
                combat.CritChance = Mathf.Clamp01(Stats.Sum(PlayerStat.CritChance));
                combat.CritMultiplier = Mathf.Max(1f, 1.5f + Stats.Sum(PlayerStat.CritDamage));
                combat.ExtraStacks = Mathf.Max(0, Stats.Whole(PlayerStat.AfflictionStacks));
                combat.AfflictionSeconds = Stats.Multiplier(PlayerStat.AfflictionSeconds);
                combat.AfflictionPower = Stats.Multiplier(PlayerStat.AfflictionPower);
            }
            if (defence == null) return;
            defence.PerfectDodgeWindow = baseDodgeWindow * Stats.Multiplier(PlayerStat.DodgeSafeTime);
            defence.Guard.StaminaPerDamage = baseGuardCost * Stats.Multiplier(PlayerStat.StaminaCost) * Stats.Multiplier(PlayerStat.GuardStamina);
            defence.Flasks.EarnScale = Stats.Multiplier(PlayerStat.FlaskEarn);
            defence.Guard.ChipFraction = baseGuardChip * Stats.Multiplier(PlayerStat.GuardGrey);
            defence.Guard.PerfectWindow = baseGuardWindow * Stats.Multiplier(PlayerStat.PerfectGuardWindow);
            defence.Health.SetMaximum(Mathf.Max(1f, baseHealth + Stats.Sum(PlayerStat.Health)));
            // Deep Well: a flask mends nothing at once; what it holds goes into the well.
            defence.Flasks.HealAmount = hasWell ? 0f : baseFlaskHeal * Stats.Multiplier(PlayerStat.FlaskHeal);
            // A mark may take every flask away.
            defence.Flasks.SetMaximum(Mathf.Max(0, baseFlasks + Stats.Whole(PlayerStat.FlaskCount)));
            defence.DamageTaken = Stats.Multiplier(PlayerStat.DamageTaken) * defence.Afflictions.TakenScale;
            defence.PerfectGuardMana = baseGuardMana * Stats.Multiplier(PlayerStat.ManaFromGuards);
            defence.PerfectDodgeMana = baseDodgeMana * Stats.Multiplier(PlayerStat.ManaFromDodges);
            defence.Health.GreyFadePerSecond = baseGreyFade / Stats.Multiplier(PlayerStat.GreyKept);
            // The Body: Steady's poise outside a swing, the Enduring Body's shorter flinch, smaller push and longer grey.
            defence.Steadiness = Mathf.Max(0f, Stats.Sum(PlayerStat.Poise));
            defence.HurtSeconds = Mathf.Max(0.05f, baseHurtSeconds + Stats.Sum(PlayerStat.FlinchSeconds));
            defence.Knockback = baseKnockback * Stats.Multiplier(PlayerStat.KnockbackTaken) * defence.Afflictions.KnockbackScale;
            defence.Health.GreyDelay = Mathf.Max(0f, baseGreyDelay + Stats.Sum(PlayerStat.GreyDelay));
        }

        private void Update()
        {
            if (toFinish.Count > 0) FinishBroken();
            if (echoForm != null && Time.time >= echoAt) CastEcho();
            ReadStandingNews();
            ReadPickups();
            // A gathered spell nobody loosed goes by itself when its press window is over, or is lost when it lapses (Kept Wind).
            if (rings.Due(Time.time))
            {
                if (rings.Lapses) rings.End();
                else LooseRings();
            }
            if (mending.Owed > 0f && defence != null && !defence.IsDead) defence.Heal(mending.Take(Time.deltaTime), false);
            sinceBlowLanded += Time.deltaTime;
            if (hasWell && well > 0f && sinceBlowLanded < WellBlowSeconds && defence != null && !defence.IsDead && defence.Health.Fraction < 0.999f)
            {
                float paid = Mathf.Min(well, WellPerSecond * Time.deltaTime);
                defence.Heal(paid);
                well -= paid;
                ShowWell();
            }
            if (combat != null && combat.Combo != null)
            {
                bool answering = hasAnswer && Time.time < answerUntil;
                combat.Combo.SkillCostScale = !hasAnswer ? 1f : answering ? 0f : AnswerDearer;
                combat.Combo.SkillAtOnce = answering;
            }
            EmptyTimedTallies(Time.time);
            if (hasLeaf && leafIndex >= 0 && leafIndex < tally.Length && tally[leafIndex] < 3)
            {
                leafClock += Time.deltaTime;
                if (leafClock >= BreathSeconds) GainBreath();
            }
            if (combat != null && combat.Mana.Diverted > 0f)
            {
                float mended = combat.Mana.TakeDiverted() / Mathf.Max(0.1f, HeartwoodManaPerHealth);
                if (defence != null && !defence.IsDead) defence.Heal(mended);
                HeartwoodMended += mended;
            }
            untilRefresh -= Time.unscaledDeltaTime;
            if (untilRefresh <= 0f) Refresh();
            float deltaTime = Time.deltaTime;
            if (hasHeat) Heat.Tick(deltaTime);
            if (hasHeat && Heat.Level > baseHeatLevels)
            {
                // White heat: above the usual five, every level quickens the arm. Given again each frame, it lasts as long as the level does.
                Stats.AddBoon(PlayerStat.SwingSpeed, WhiteHeatSwing * (Heat.Level - baseHeatLevels), 0.25f);
            }
            if (selfBurnLeft > 0f && defence != null && !defence.IsDead)
            {
                // Struck in white heat, the fire is the player's own for a while. It never takes the last point.
                selfBurnLeft -= deltaTime;
                selfBurnOwed += WhiteHeatBurn * deltaTime;
                if (selfBurnOwed >= 1f)
                {
                    selfBurnOwed -= 1f;
                    if (defence.Health.Current > 1.5f) defence.Health.Damage(1f);
                    Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 1.1f, Vector3.up, 0.4f);
                }
            }
            if (combat != null)
            {
                float owed = Stats.Sum(PlayerStat.GreyStrength);
                // Health that is owed makes the arm stronger, for one who has learned to borrow.
                float borrowed = owed > 0f && defence != null ? Mathf.Min(GreyStrengthCap, defence.Health.Grey * owed) : 0f;
                combat.UpgradeDamage = (hasHeat ? Heat.DamageMultiplier : 1f) * (1f + borrowed) * (defence != null ? defence.Afflictions.DealtScale : 1f);
                // The weapon in hand may have changed, and its chain with it.
                if (combat.Combo != null) combat.Combo.StaminaScale = attackStamina;
            }
            Stats.Tick(deltaTime);
            States(deltaTime);
            ApplyStats();
            Watch(deltaTime);
            Flush();
        }

        // The states a learned number may wait on. Each is a plain question asked every frame; nothing moves unless the answer changed.
        private void States(float deltaTime)
        {
            sinceStruck += deltaTime;
            sinceDodge += deltaTime;
            Melt(deltaTime);
            if (reserveIndex >= 0 && reserveIndex < tally.Length && tally[reserveIndex] > 0 && combat != null && combat.NextBlowDamage <= 1f)
            {
                // A blow has spent the charge that was ready: one fewer is banked, and the next is made ready.
                tally[reserveIndex]--;
                tallyDirty = true;
                if (tally[reserveIndex] > 0)
                {
                    combat.NextBlowDamage = StoredChargeMultiplier;
                    combat.NextBlowStagger = StoredChargeMultiplier * Stats.Multiplier(PlayerStat.ChargeStagger);
                    Vfx.Play(VfxKind.Shock, transform.position + Vector3.up * 1.2f, Vector3.up, 0.8f);
                }
            }
            if (stormFrontOwed)
            {
                stormFrontOwed = false;
                if (element != null && Has(Trees.WindTree.StormFront))
                {
                    stormFrontAt = Time.time + 0.4f;
                    element.CastBaseFree(Elements.Wind);
                }
            }
            Stats.SetWhen(StatWhen.Surge, element != null && element.SurgeActive);
            Stats.SetWhen(StatWhen.Unhurt, sinceStruck >= UnhurtSeconds);
            Stats.SetWhen(StatWhen.Risen, Risen);
            Afflictions.TickBeat(deltaTime);
            if (defence != null) defence.Unflinching = (hollowStrength && Risen) || Time.time < firmUntil || IsUnshaken(Stats);
            if (deadAlready && defence != null && combat != null && !defence.IsDead)
            {
                // Dead Already: the drain of the Risen in every fight, a point at a time, never the last.
                untilDeadLook -= deltaTime;
                if (untilDeadLook <= 0f)
                {
                    untilDeadLook = 0.5f;
                    deadFight = combat.NearestEnemy(FightRange) != null;
                }
                if (deadFight && Time.time >= riseStilledUntil)
                {
                    deadDrainOwed += DeadAlreadyDrain * RiseDrainShare * deltaTime;
                    while (deadDrainOwed >= 1f)
                    {
                        deadDrainOwed -= 1f;
                        if (defence.Health.Current > 1.5f)
                        {
                            defence.Health.Damage(1f);
                            DeadDrained++;
                        }
                    }
                }
            }
            // Inside your own ring of stones you are Rooted however you move (The Closed Ring).
            Stats.SetWhen(StatWhen.Planted, sinceDodge >= Mathf.Max(0.5f, PlantedSeconds - Stats.Sum(PlayerStat.PlantedSooner))
                || OwnGrounds.Under(OwnGrounds.StoneRing, transform.position, Time.time));
            // Ground of your own stood in. A dodge begun on wet sand is a dodge through it to its end (The Land Was Sea).
            foreach (var (when, ground) in OwnGroundStates)
            {
                bool inside = InOwnGround(ground);
                if (when == StatWhen.OnWetSand)
                {
                    if (sinceDodge > DodgeThroughSeconds) dodgeFromSand = false;
                    inside |= dodgeFromSand;
                }
                // Any poison ground of yours is a cloud too (Low Mist's sheet, Chokedamp's pools).
                if (when == StatWhen.InCloud) inside |= Hazards.OwnGroundUnder(transform.position, AfflictionKind.Poison);
                Stats.SetWhen(when, inside);
            }
            // Stood still: not moved more than a hair since the last step, for a second (Cairn's and Stone Rise's Masters).
            Vector3 here = transform.position;
            sinceMoved = StillRules.Next(sinceMoved, (here - lastStill).sqrMagnitude, deltaTime);
            lastStill = here;
            Stats.SetWhen(StatWhen.StoodStill, sinceMoved >= StillRules.Seconds);
            Stats.SetWhen(StatWhen.ManaFull, combat != null && !combat.Mana.Sealed && combat.Mana.Current >= combat.Mana.Maximum - 0.001f);
            Stats.SetWhen(StatWhen.HeavySwing, combat != null && combat.InHeavySwing);
            Stats.SetWhen(StatWhen.NearGrown, Hazards.GrownNear(transform.position, 4f));
            Stats.SetWhen(StatWhen.FarFromGrown, !Hazards.GrownNear(transform.position, 7f));
            bool inFire = scorched && Hazards.FireUnder(transform.position);
            Stats.SetWhen(StatWhen.InOwnFire, inFire);
            Stats.SetWhen(StatWhen.InOwnWater, Hazards.OwnGroundUnder(transform.position, AfflictionKind.Soaked));
            if (combat != null) combat.HeavyUnbroken = heavyUnbrokenByMarks || Time.time < heavyUnbrokenUntil;
            // Scattered (Gone to Ash): stand again where you are now, out of the ash.
            if (scatteredUntil > 0f && Time.time >= scatteredUntil)
            {
                scatteredUntil = 0f;
                if (player != null) player.Hidden = false;
                var standing = OwnElement(null);
                for (int i = 0; i < 4; i++) ElementLook.Puff(standing, transform.position + Vector3.up * (0.4f + 0.4f * i), Vector3.down, 0.8f);
            }
            SurgeHeld = inFire;
            if (hasTideClock)
            {
                // The tide turns by itself every few seconds; a perfect guard or dodge turns it at once (TurnTide).
                tideClock += deltaTime;
                if (tideClock >= TideTurnSeconds) TurnTide();
            }
            Stats.SetWhen(StatWhen.HighWater, hasTideClock && tideHigh);
            Stats.SetWhen(StatWhen.LowWater, hasTideClock && !tideHigh);
            if (sharerCount > 0) PayShares();
            if (defence == null) return;
            // Bedrock, Earth's surge: nothing laid on the player stays while it lasts.
            defence.Afflictions.Shedding = element != null && element.SurgeActive && element.DuoSurgeWorn == null && element.SurgeInfo != null && element.SurgeInfo.Name == Elements.Earth;
            Stats.SetWhen(StatWhen.Guarding, defence.IsGuarding);
            Stats.SetWhen(StatWhen.LowHealth, !defence.IsDead && defence.Health.Fraction < LowHealthShare);
            Stats.SetWhen(StatWhen.HalfHealth, !defence.IsDead && defence.Health.Fraction < 0.5f);
            Stats.SetWhen(StatWhen.GreyHeld, defence.Health.Grey > 0.5f);
            Stats.SetWhen(StatWhen.HighHealth, !defence.IsDead && defence.Health.Fraction >= 0.5f);
        }

        // The triggers that are not told to this by anyone: time passing in a fight, health falling low, mana filling.
        private void Watch(float deltaTime)
        {
            var timed = byTrigger[(int)UpgradeTrigger.EverySeconds];
            if (timed.Count > 0 && combat != null)
            {
                untilFightLook -= deltaTime;
                if (untilFightLook <= 0f)
                {
                    untilFightLook = 0.5f;
                    inFight = combat.NearestEnemy(FightRange) != null;
                }
                if (inFight)
                {
                    for (int i = 0; i < timed.Count; i++)
                    {
                        var effect = timed[i];
                        sinceLast[effect.Slot] += deltaTime;
                        if (sinceLast[effect.Slot] < Mathf.Max(0.25f, effect.Every)) continue;
                        sinceLast[effect.Slot] = 0f;
                        Consider(effect, null, transform.position, false);
                    }
                }
            }
            if (defence != null)
            {
                bool low = !defence.IsDead && defence.Health.Fraction < LowHealthShare;
                if (low && !wasLow) Trigger(UpgradeTrigger.LowHealth, null, transform.position, false);
                wasLow = low;
            }
            if (combat != null)
            {
                bool full = combat.Mana.Current >= combat.Mana.Maximum - 0.001f;
                if (full && !wasFull) Trigger(UpgradeTrigger.ManaFull, null, transform.position, false);
                wasFull = full;
            }
        }

        // ---- The runner: effects wait in a line and are done on this component's own step, never in the middle of the blow that caused them.

        private void Trigger(UpgradeTrigger trigger, Component target, Vector3 at, bool hasAt = true, int spent = 0, AfflictionSet dead = null, AfflictionKind what = AfflictionKind.None,
            string reaction = "", LaySource source = LaySource.Effect, float power = 0f)
        {
            // Raised while a reaction's effect is being done (a kill, a full affliction, a shatter it caused): what answers
            // is the reaction's too, and starts nothing.
            if (source == LaySource.Effect) source = laying;
            var list = byTrigger[(int)trigger];
            for (int i = 0; i < list.Count; i++)
            {
                var effect = list[i];
                if (effect.Spent != AfflictionKind.None && effect.Spent != what) continue;
                if (effect.OwnSpell && effect.Owner != castSource) continue;
                // A base spell cast through its Learn mark (its shape) is still the base spell.
                if (effect.BaseSpell && castSource != null && !castSource.IsBaseSpell) continue;
                if (effect.OwnBranch && !SpellBranches.OwnBranch(effect.Owner, castSource, baseElement)) continue;
                // Only once this cast has struck so many (Step Into Light's Master).
                if (effect.StruckAtLeast > 0 && !CastStruckAtLeast(answeringCast, castHitsOf, castHits.Count, effect.StruckAtLeast)) continue;
                // Only another branch's casts (The Tide Brings It answers Riptide's).
                if (!string.IsNullOrEmpty(effect.CastOf) && !SpellBranches.CastOf(effect.CastOf, castSource, baseElement)) continue;
                if (effect.SpentAtLeast > 0 && spent < effect.SpentAtLeast) continue;
                // A standing thing's moment is its own mark's alone.
                if ((trigger == UpgradeTrigger.StandingStruck || trigger == UpgradeTrigger.StandingBurst) && !AnswersStanding(effect, standingOwner, standingHeavy)) continue;
                // A pickup's own mark answers through the effects written beside the one that dropped it (AnswerPickup); here only
                // the marks that answer any pickup of its kind.
                if ((trigger == UpgradeTrigger.PickedUp || trigger == UpgradeTrigger.PickupLeft) && !(effect.AnyDrops && effect.Drops == pickupKind)) continue;
                // Ground told of one standing in it is answered only by the effects that name that ground.
                if (trigger == UpgradeTrigger.InGround && !AnswersGround(effect, groundTold)) continue;
                // A death dealt by one reaction's own blow (The Feast), told with that reaction's id.
                if (trigger == UpgradeTrigger.Kill && !KillAnswers(effect.KilledBy, reaction)) continue;
                // A mark that deepens one reaction answers that one only.
                bool reactionMoment = trigger == UpgradeTrigger.Reacted || trigger == UpgradeTrigger.ReactionStruck;
                if (reactionMoment && !string.IsNullOrEmpty(effect.Reaction) && effect.Reaction != reaction) continue;
                // Every reaction with a given affliction in it (Cold Settles: with Chill).
                if (reactionMoment && effect.ReactionWith != AfflictionKind.None && !ReactionHas(ReactionTable.PairOf(reaction), effect.ReactionWith)) continue;
                triggerPair = reactionMoment ? ReactionTable.PairOf(reaction) : -1;
                Consider(effect, target, at, hasAt, spent, dead, source != LaySource.Reaction || !effect.Cascades ? source : LaySource.Effect, power);
                triggerPair = -1;
            }
        }

        // ---- Hooks for the reactions' adapter (PlayerReactions): small and public, the behaviour is theirs.

        /// <summary>
        /// Fires a trigger for the marks in force (a lift, a duo mark that deepens a reaction), as the player's own systems
        /// do, with what their effects lay counted as <paramref name="source"/>. For <see cref="UpgradeTrigger.Reacted"/>,
        /// <paramref name="reaction"/> is the reaction's id. Done on this component's next step, or at <see cref="FlushNow"/>.
        /// </summary>
        public void Fire(UpgradeTrigger trigger, Component target, Vector3 at, string reaction = "", LaySource source = LaySource.Effect) =>
            Trigger(trigger, target, at, true, 0, null, AfflictionKind.None, reaction ?? "", source);

        /// <summary>
        /// Puts one effect in the runner's line, whether or not its mark is learned: how a reaction's plain mark (always in
        /// force) and a condition's answers are run. <paramref name="power"/> is what the trigger carried (a blow's damage),
        /// <paramref name="spent"/> its count (for PerStack), and <paramref name="dead"/> what a dead enemy carried, read
        /// now, before it is cleared, as the Kill trigger reads it.
        /// </summary>
        public void Fire(UpgradeEffect effect, Component target, Vector3 at, LaySource source, float power = 0f, int spent = 0, AfflictionSet dead = null, Component link = null)
        {
            if (effect == null) return;
            Consider(effect, target, at, true, spent, dead, source == LaySource.Reaction && effect.Cascades ? LaySource.Effect : source, power, link);
        }

        /// <summary>
        /// A condition's moment (its blow, its end, its carrier striking at you), told to the marks in force that answer
        /// that condition whoever set it (<see cref="UpgradeEffect.AnyMark"/>): The Exile Returns and Given Bones answer the
        /// Grudge's bolt in whichever shape calls it. Queued like any trigger; true when any was. Nothing they carry is
        /// multiplied by the condition's blows: their own Spend counts what they need.
        /// </summary>
        public bool FireCondition(UpgradeTrigger trigger, in ConditionNews news, Component who, Vector3 at)
        {
            var list = byTrigger[(int)trigger];
            bool any = false;
            for (int i = 0; i < list.Count; i++)
            {
                var effect = list[i];
                // By its own name, or by what it is: a shape's rocking is a Logan Stone's rocking (New Face).
                if (!effect.AnyMark || (effect.Condition != news.Id && effect.Condition != news.Is)) continue;
                if (trigger == UpgradeTrigger.ConditionStruck && (!PlayerReactions.Answers(effect.By, news) || !PlayerReactions.FromSide(effect, news))) continue;
                if (trigger == UpgradeTrigger.ConditionAttacked && effect.By == StruckBy.Turned && !news.Turned) continue;
                if (trigger == UpgradeTrigger.ConditionEnded && effect.By == StruckBy.Frozen && !news.Frozen) continue;
                if (effect.AtCount > 0 && effect.AtCount != (trigger == UpgradeTrigger.ConditionBeat ? news.Count : news.Blows)) continue;
                Consider(effect, who, at, true);
                any = true;
            }
            return any;
        }

        /// <summary>Does what waits in the runner's line now, rather than on its next step. Never inside a blow.</summary>
        public void FlushNow() => Flush();

        private static bool IsBlow(UpgradeTrigger trigger) => MarkRules.IsBlow(trigger);

        private void Consider(UpgradeEffect effect, Component target, Vector3 at, bool hasAt, int spent = 0, AfflictionSet dead = null,
            LaySource source = LaySource.Effect, float power = 0f, Component link = null)
        {
            if (effect.Slot < 0 || effect.Slot >= readyAt.Length) return;
            if (Time.time < readyAt[effect.Slot]) return;
            // A dodge's way against the enemy faced (Into the Fire, Drawn Back to Break).
            if (effect.Way != DodgeWay.Any && effect.Way != lastDodgeWay) return;
            if (effect.WhileSurge && !(element != null && element.SurgeActive)) return;
            // Blows that take turns (The Yawning Gap): only on this effect's turn.
            if (!MarkRules.OnTurn(blowTurn, effect.TakesTurns, effect.Turn)) return;
            if (effect.While != StatWhen.Always && !Stats.Holds(effect.While)) return;
            if (effect.WhileOwnStands && !Forms.SpellWorks.StandsOf(effect.Owner, transform.position, effect.WithinOwn)) return;
            if (effect.While2 != StatWhen.Always && !Stats.Holds(effect.While2)) return;
            if (effect.NeedsAirborne && !(target is IAfflictable flying && flying.Afflictions.IsAirborne)) return;
            if (effect.NeedsFrozen && !(target is IAfflictable icy && icy.Afflictions.IsFrozen)) return;
            if (effect.NeedsWindup > 0f && !InWindup(target as IAfflictable, effect.NeedsWindup)) return;
            // Only for a cast timed to the poison's beat (Miasma's, One by One's and Share Alike's Masters).
            if (effect.OnCastBeat && !castOnBeat) return;
            // Only within its branch's own spell's window (Poured on the Edge's 12 s).
            if (effect.WithinOwnCast > 0f && !InCastWindow(LastCastOf(effect.Owner), Time.time, effect.WithinOwnCast)) return;
            // Only for a cast begun in the window after its moment (the ledger's spell-windows).
            if (effect.CastAfter != CastMoment.None && !Moments.Within(effect.CastAfter, effect.CastWindow, castBeganAt, effect.CastAfterCarrying)) return;
            // Only beside something grown (of its own kin, when it names one).
            if (effect.NearGrown > 0f && !Hazards.GrownNear(transform.position, effect.NearGrown, effect.Kin > 0 ? FamilyOf(effect) : 0)) return;
            if (effect.AtGrown && Hazards.NearestGrown(transform.position, Or(effect.NearGrown, 4f), out Vector3 grownAt))
            {
                at = grownAt;
                hasAt = true;
                target = null;
            }
            // Only while its own branch's spell gathers (Bag of Winds open), and with its rings come quickly enough.
            if (effect.WhileGathering && !(rings.Active && effect.Owner != null && rings.Mark.Branch == effect.Owner.Branch)) return;
            if (effect.RingsWithin > 0f && !rings.LastWithin(Mathf.Max(1, effect.SpentAtLeast), effect.RingsWithin, Time.time)) return;
            // Only within a moment's window as the trigger comes (Waiting Thunder's Master).
            if (effect.After != CastMoment.None && !Moments.Within(effect.After, effect.AfterWindow, Time.time)) return;
            if (effect.NeedsAttacker && !IsAttacker(target, turnedBy != null ? turnedBy : null, turnedAt, Time.time)) return;
            // Only as an enemy's blow is about to land (the shared movement rule's timing).
            if (effect.BlowLanding > 0f && (combat == null || combat.BlowAboutToLand(effect.BlowLanding, out _) == null)) return;
            // Only in the moment after a red attack was turned (Mountain Stance's, Circle of Steel's and Gone to Ash's Masters).
            if (effect.RedOnly && !RedJustTurned(Time.time, redTurnedAt)) return;
            if (effect.OnBeat > 0f && !(target is IAfflictable beaten && beaten.Afflictions.NearBeat(effect.OnBeat))) return;
            if (effect.HealthBelow > 0f && !(defence != null && !defence.IsDead && defence.Health.Fraction < effect.HealthBelow)) return;
            if (effect.StoneAtBack > 0f && (target == null || !StoneBehind(transform.position, target.transform.position, effect.StoneAtBack))) return;
            if (effect.NearStanding > 0f && !StandingThings.NearKind(transform.position, effect.NearStanding, effect.Raises)) return;
            // Only on an enemy standing in your own ground of that name (The Hag's Staff: heavy blows on its frozen disc).
            if (!string.IsNullOrEmpty(effect.TargetOnGround) && !(target != null && OwnGrounds.Under(effect.TargetOnGround, target.transform.position, Time.time))) return;
            if (effect.Needs != AfflictionKind.None || effect.Needs2 != AfflictionKind.None)
            {
                // Asked of what the dead one carried, of the trigger's own enemy, or, when the trigger names no one, of the enemy faced.
                var carried = dead;
                if (carried == null)
                {
                    if (target == null && !hasAt && combat != null)
                    {
                        var ahead = combat.EnemyAhead(FightRange);
                        if (ahead != null) target = ahead.GetComponentInParent<IAfflictable>() as Component;
                    }
                    carried = target is IAfflictable afflicted ? afflicted.Afflictions : null;
                }
                if (carried == null) return;
                if (effect.Needs != AfflictionKind.None)
                {
                    if (!carried.Has(effect.Needs)) return;
                    if (effect.NeedsFull && carried.Stacks(effect.Needs) < carried.DepthOf(effect.Needs)) return;
                    if (effect.NeedsAtLeast > 0 && carried.Stacks(effect.Needs) < effect.NeedsAtLeast) return;
                    if (dead != null) spent = carried.Stacks(effect.Needs);
                    // Counted now, before any effect of this moment is done: a shape counts what its Learn will spend.
                    else if (effect.CountsNeeds) spent = carried.Stacks(effect.Needs);
                }
                if (effect.Needs2 != AfflictionKind.None && !carried.Has(effect.Needs2)) return;
            }
            // In a condition another mark set (the ledger's condition-needs): asked of the dead one, or of the trigger's enemy.
            if (!string.IsNullOrEmpty(effect.InState) && !InState(effect.InState, dead, target)) return;
            // A reaction's own numbers for an elite or a god (the ranks are read from what decides holding).
            if (effect.Ranks != EnemyRank.Any && !(target is IAfflictable ranked && (ranked.Afflictions.Rank & effect.Ranks) != 0)) return;
            // Weapon blows lay only the first element's affliction, except from a mark on a mixed branch (MarkRules.BlowMayLay
            // keeps the rule): an effect that lays another element's on a blow is not done.
            var laid = effect.Do == UpgradeAction.Afflict ? (effect.Affliction != AfflictionKind.None ? effect.Affliction : OwnElement(effect.Owner).Affliction) : AfflictionKind.None;
            if (combat != null && !MarkRules.BlowMayLay(effect.Owner, effect.On, laid, combat.WeaponAffliction)) return;
            // A mark on a mixed branch lays both its elements' afflictions with a blow or a perfect guard. The root of
            // the first element lays that element's own already, so of it the branch lays only what it lays beyond the root.
            int less = 0;
            if (effect.Do == UpgradeAction.Afflict && effect.Radius <= 0f && effect.Owner != null && effect.Owner.IsMix && (IsBlow(effect.On) || effect.On == UpgradeTrigger.PerfectGuard)
                && hasRoot && combat != null && element != null && element.Info != null
                && laid == element.Info.Affliction)
            {
                less = Mathf.Max(1, combat.WeaponAfflictionStacks);
                if (Or(effect.Stacks, 1) - less <= 0) return;
            }
            // Clocks and counts kept on the enemy itself, asked once the enemy is known and every other test has passed,
            // so only the times that would really happen are counted.
            var kept = (effect.CooldownPerEnemy || effect.EveryOnEnemy > 1) && target is IAfflictable keeper ? keeper.Afflictions.Tally : null;
            if (effect.CooldownPerEnemy && kept != null && !kept.Ready(effect.Slot, Time.time)) return;
            if (effect.EveryOnEnemy > 1 && (kept == null || !kept.AddAndCome(effect.Slot, effect.EveryOnEnemy))) return;
            if (effect.Chance < 1f && Random.value > effect.Chance) return;
            if ((effect.Spends > 0 || effect.SpendsAll) && effect.Owner != null)
            {
                // Paid from the count its mark keeps (or its branch's Learn keeps), or it does not happen.
                int index = PayerOf(effect).Index;
                int holds = index < tally.Length ? tally[index] : 0;
                int price = effect.SpendsAll ? holds : effect.Spends;
                if (price <= 0 || holds < price) return;
                tally[index] -= price;
                tallyDirty = true;
                spent = price;
            }
            // Asked again here for what came with no trigger to filter it (a condition's weals, a dead enemy's depth).
            if (effect.SpentAtLeast > 0 && spent < effect.SpentAtLeast) return;
            // Done to the one whose blow landed on you, or was turned or dodged, a moment ago.
            if (effect.ToStruckBy || effect.ToTurnedBy)
            {
                var attacker = effect.ToStruckBy ? struckBy : turnedBy;
                float when = effect.ToStruckBy ? struckByAt : turnedAt;
                if (!RecentAttacker(attacker, when, effect.CastWindow > 0f ? effect.CastWindow : 1f, Time.time) || !PlayerReactions.Alive(attacker)) return;
                target = attacker;
                at = attacker.transform.position;
                hasAt = true;
            }
            // Done to the chain-mate, found now (a dead one's chain is still on its mate, and its own is cleared after).
            if (effect.ToChainMate)
            {
                var mate = EnemyChains.MateOf(target);
                if (mate == null || !PlayerReactions.Alive(mate)) return;
                at = target.transform.position;
                hasAt = true;
                target = mate;
            }
            // Once a fight (The Burning Tree): not again until the next fight begins.
            if (effect.OncePerFight)
            {
                int fight = spells != null ? spells.Fight.Fights : 0;
                if (effect.Slot >= firedInFight.Length || !MarkRules.FirstInFight(firedInFight[effect.Slot], fight)) return;
                firedInFight[effect.Slot] = fight;
            }
            // Things that can set themselves off again are held to a few times a second at most.
            float least = effect.On == UpgradeTrigger.AfflictionFull || effect.On == UpgradeTrigger.Kill ? 0.2f : 0f;
            float wait = Mathf.Max(effect.Cooldown, least);
            if (effect.CooldownPerEnemy && kept != null) kept.Start(effect.Slot, Time.time, wait);
            else readyAt[effect.Slot] = Time.time + wait;
            // A boon held by the cast is given now, before anything its own blows set going is done (Reap What Is Sown's Master:
            // the cast's blows land as it is cast, and their bursts are done after).
            if (effect.Do == UpgradeAction.Boon && effect.ThisCast)
            {
                castBoons.Add(answeringCast, effect.Stat, effect.Amount, Time.time + Or(effect.Seconds, 5f) * Stats.Multiplier(PlayerStat.BoonSeconds));
                return;
            }
            Enqueue(new Pending { Effect = effect, Target = target, At = at, HasAt = hasAt, Spent = spent, Less = less, Source = source, Power = power, Ripe = ripeScale,
                ReactionOf = reactionPair + 1, ReactionCast = reactionPair >= 0 ? reactionCast : 0, Link = link, Pair = triggerPair, Again = strikingAgain, CastTag = answeringCast, From = SpellFrom });
        }

        /// <summary>
        /// True when what died carried the condition (by its name or what it is), or else the one struck does, or a blow
        /// that just landed on it spent it (<see cref="EnemyConditions.Held"/>).
        /// </summary>
        /// <summary>Breaks the ice of a frozen enemy as a heavy blow would; returns what the breaking deals (0 when it was not frozen).</summary>
        public static float ShatterNow(IAfflictable target) => target != null && target.Afflictions.IsFrozen ? target.Afflictions.Shatter() : 0f;

        /// <summary>Counts for probes: frozen enemies shattered by an effect, not a blow (Brittle to the Core).</summary>
        public int ShattersMade { get; private set; }

        public static bool InState(string state, AfflictionSet dead, Component target)
        {
            var set = dead ?? (target is IAfflictable afflicted ? afflicted.Afflictions : null);
            return set != null && set.Conditions.Held(state);
        }

        private void Enqueue(in Pending pending)
        {
            if (queued >= QueueCapacity) return;
            queue[queued++] = pending;
        }

        private void Flush()
        {
            if (flushing || queued == 0) return;
            flushing = true;
            try
            {
                // What is done here may put more in the line; those are done too, as far as the line has room.
                for (int i = 0; i < queued; i++)
                {
                    var pending = queue[i];
                    queue[i] = default;
                    Execute(pending);
                }
            }
            finally
            {
                // One effect that throws must not leave the runner stopped for the rest of the play.
                queued = 0;
                flushing = false;
            }
            if (tallyDirty) ApplyTallies();
        }

        private SpellBlow Blow(ElementInfo look, AfflictionKind kind, float damage, float stagger, int stacks) => new SpellBlow
        {
            Damage = damage,
            Stagger = stagger,
            Affliction = stacks > 0 ? kind : AfflictionKind.None,
            Stacks = stacks,
            Effect = ElementLook.HitEffect(look),
            Tinted = ElementLook.Tints(look),
            Tint = look.Colour,
            Look = look,
            // What a reaction's effect strikes with starts no reaction of its own.
            FromReaction = laying == LaySource.Reaction,
            // And whom it strikes is told as struck by that reaction (ReactionStruck).
            ReactionOf = layingReaction,
            ReactionCast = layingCast,
            // Critical when a mark makes this reaction's blows so (Seen Plainly: every reaction with Marked in it).
            Critical = layingReaction > 0 && ReactionCritical(layingReaction - 1),
        };

        /// <summary>True when the marks in force make the reaction of this pair strike critically (PlayerStat.ReactionCritical).</summary>
        public bool ReactionCritical(int pair) =>
            pair >= 0 && pair < AfflictionPairs.Count && Stats.ReactionMultiplier(PlayerStat.ReactionCritical, AfflictionPairs.First(pair), AfflictionPairs.Second(pair)) > 1.0001f;

        // A blow drawn by its reaction's own look throws no hit art of its element (Sickle Wind's whirl threw a wind strike's
        // white crescents at every enemy every bite, the film of 2026-11-20), and the body it lands on flashes in the
        // element's own hue at the softer strength, not the plain blow's white (its seven bites flashed four soldiers white).
        private static readonly Color SteelFlash = new Color(0.3f, 0.45f, 0.8f);

        private static SpellBlow Unseen(SpellBlow blow, bool unseen)
        {
            blow.Unseen |= unseen;
            if (unseen && blow.Struck.a <= 0f)
            {
                // A grey element (Sundered's steel, 0.78, 0.8, 0.88) lit to the flash's brightness is white: Sickle Wind's
                // seven bites flashed five soldiers white again and again. A grey tint flashes a steel blue instead.
                Color tint = blow.Tint;
                float most = Mathf.Max(tint.r, Mathf.Max(tint.g, tint.b)), least = Mathf.Min(tint.r, Mathf.Min(tint.g, tint.b));
                if (most - least < 0.2f) tint = SteelFlash;
                blow.Struck = new Color(tint.r, tint.g, tint.b, 1f);
            }
            return blow;
        }

        // Where the patches of a ground or burst effect go: one, a line ahead, a ring, or a row carried downwind past the target.
        /// <summary>How many bolts of a reaction's look are drawn down a lane (<see cref="UpgradeAction.Lane"/>), besides the one on the target.</summary>
        public const int LaneBolts = 4;

        /// <summary>
        /// How many patches a lane of this length and half-width is laid as: one every two thirds of its width, so where
        /// two meet they still cover most of it and a body on the lane is never missed between them.
        /// </summary>
        public static int LanePatches(float length, float half) => Mathf.Max(2, Mathf.CeilToInt(length / Mathf.Max(0.1f, half * 1.34f)) + 1);

        /// <summary>Where a lane's patch lies: the first on the target, the last <paramref name="length"/> beyond it, away from the player.</summary>
        public static Vector3 LanePatch(Vector3 from, Vector3 away, float length, int index, int patches) =>
            from + away * (length * index / Mathf.Max(1, patches - 1));

        private static Vector3 PatchAt(UpgradeEffect effect, int index, int patches, Vector3 centre, Vector3 self, Vector3 aim, float reach)
        {
            switch (effect.Shape)
            {
                case HazardShape.Line:
                    if (effect.AcrossFront)
                    {
                        // Across your front, Amount metres ahead, from your left to your right (The Drawn Line's spears).
                        Vector3 right = new Vector3(aim.z, 0f, -aim.x);
                        return self + aim * Or(effect.Amount, 1.5f) + right * (reach * ((index + 0.5f) / patches - 0.5f));
                    }
                    return self + aim * (reach * (index + 0.5f) / patches);
                case HazardShape.Band:
                {
                    // Across your line to the target, centred on it.
                    Vector3 right = new Vector3(aim.z, 0f, -aim.x);
                    return centre + right * (reach * ((index + 0.5f) / patches - 0.5f));
                }
                case HazardShape.Ring: return centre + Quaternion.Euler(0f, index * 360f / patches, 0f) * Vector3.forward * reach;
                case HazardShape.Downwind: return centre + aim * (reach + (index - (patches - 1) * 0.5f) * Or(effect.Amount, 1f));
                default: return centre;
            }
        }

        // A mark's own element (a duo mark's first); the first attunement's for an effect with no mark.
        private ElementInfo OwnElement(Upgrade owner) => element != null ? element.ElementOf(owner) : Elements.None;

        private static float Or(float value, float usual) => value > 0f ? value : usual;
        private static int Or(int value, int usual) => value > 0 ? value : usual;

        // The source is held for the length of one effect, so every blow, patch and laying it makes is counted as it.
        private void Execute(in Pending pending)
        {
            laying = pending.Source;
            layingReaction = pending.ReactionOf;
            layingCast = pending.ReactionCast;
            answeringLaid = pending.Effect != null && pending.Effect.On == UpgradeTrigger.AfflictionLaid;
            // A mark that runs arcs on as they leave (Nothing Resists) is not answered by its own arcs, whatever set them going.
            answeringArc = pending.Effect != null && (pending.Effect.On == UpgradeTrigger.ArcLeft || (pending.Effect.Do == UpgradeAction.Arc && AnswersArcs(pending.Effect.Owner)));
            try { Do(pending); }
            finally
            {
                laying = LaySource.Effect;
                layingReaction = layingCast = 0;
                answeringLaid = false;
                answeringArc = false;
            }
        }

        private void Do(in Pending pending)
        {
            if (combat == null || element == null) return;
            if (pending.Special == Special.DeathBurst)
            {
                var thorns = lookOf[(int)AfflictionKind.Seeded];
                combat.StrikeArea(pending.At, pending.Radius, Blow(thorns, AfflictionKind.None, pending.Power, 10f * Stats.Multiplier(PlayerStat.BurstStagger), 0));
                ElementLook.Strike(thorns, pending.At, 0.2f);
                return;
            }
            if (pending.Special == Special.Carry)
            {
                combat.AfflictArea(pending.At, pending.Radius, pending.Kind, pending.Stacks, pending.Target, laying);
                // What a reaction lays and the water carries on is seen in the reaction's own look: no ring each time
                // (Seized's film laid Static every half second and rang a ring and a tinted ring each time, the
                // owner's "remove the circles ... less flickery", 2026-11-20).
                if (pending.Source != LaySource.Reaction) ElementLook.Ring(lookOf[(int)pending.Kind], pending.At + Vector3.up * 0.2f);
                return;
            }

            var effect = pending.Effect;
            if (effect == null) return;
            // Done on each enemy near that carries what it asks, as each one's own: put back in the line once for each.
            if (effect.EachWithin > 0f && !pending.Fanned)
            {
                FanOut(pending);
                return;
            }
            EffectsDone++;
            // An effect that names no affliction lays, and is seen in, its own mark's element (IMPLEMENTATION.md 3.6).
            var held = OwnElement(effect.Owner);
            var kind = effect.Affliction != AfflictionKind.None ? effect.Affliction : held.Affliction;
            var look = lookOf[(int)kind] ?? held;
            Vector3 self = transform.position;
            Transform target = pending.Target != null ? pending.Target.transform : null;
            // With no one given, an effect aimed at a target takes the enemy the player faces.
            if (target == null && !pending.HasAt && effect.At == EffectAt.Target) target = combat.EnemyAhead(FightRange);
            Vector3 centre = effect.At == EffectAt.Self ? self : target != null ? target.position : pending.HasAt ? pending.At : self + transform.forward * 2.5f;
            centre.y = self.y;
            Vector3 toward = centre - self;
            toward.y = 0f;
            Vector3 aim = toward.sqrMagnitude > 0.04f ? toward.normalized : transform.forward;

            // A creature drawn by a recipe look plays as the effect is done (on its enemy, or where it is done).
            if (!string.IsNullOrEmpty(effect.Look) && Forms.CreatureLooks.Play(effect.Look, target, centre, aim)) CreatureLooksPlayed++;

            // What the trigger carried multiplies what is done: how many are thrown or fall, or how hard it strikes. With
            // nothing carried, what this mark's Spend just took or counted on the same enemy stands in (a reaction's depth).
            int counted = spentBy == effect.Owner && spentOn == pending.Target ? lastSpent : 0;
            int per = effect.PerStack ? Mathf.Max(1, pending.Spent > 0 ? pending.Spent : counted) : 1;
            if (effect.PerStack && effect.Share > 0f && effect.Do != UpgradeAction.Spend) per = Mathf.Max(1, Mathf.CeilToInt(per * effect.Share - 0.0001f));
            if (effect.PerStack && effect.PerMore > 0) per += effect.PerMore;
            if (effect.PerStack && effect.PerMost > 0) per = Mathf.Min(per, effect.PerMost);
            // By the depth of an affliction on the one the trigger names (Night Sun: 1 a Burn stack).
            if (effect.PerDepthOf != AfflictionKind.None)
            {
                per = DepthOn(effect.PerDepthOf, pending.Target as IAfflictable);
                if (per <= 0) return;
            }
            bool byCount = effect.Do == UpgradeAction.Projectile || effect.Do == UpgradeAction.Falling || effect.Do == UpgradeAction.Ward
                || (effect.Do == UpgradeAction.Hazard && effect.PerStack && effect.Shape != HazardShape.Patch);
            // With no Spend of its own before it, what the trigger carried (a reaction struck again where its enemy fell: Kettle).
            bool bySpent = effect.DamagePerSpent > 0f || effect.StaggerPerSpent > 0f;
            int spentHere = bySpent ? (counted > 0 || !pending.Again ? counted : pending.Spent) : 0;
            // The depth counted as the moment came (CountsNeeds) is what each point of it adds to (Spoken Sentence: 12 a mark).
            if (effect.CountsNeeds && bySpent) spentHere = pending.Spent;
            float eDamage = (byCount ? effect.Damage : effect.Damage * per) + effect.DamagePerSpent * spentHere;
            // A reaction's own blows, by the stat ReactionDamage (Fuel to It, Left in Rubble, Every Crack Rings).
            float reactionScale = ReactionScale(effect.Owner);
            eDamage *= reactionScale;
            // A reaction's own blows, ground and pulls reach further by the stat ReactionReach (Carried Far).
            float spread = ReactionScale(effect.Owner, PlayerStat.ReactionReach);
            // A condition's kept number as the damage (Collected Twice: the reaction's own damage, repeated).
            if (effect.ByPower && effect.Do != UpgradeAction.Condition) eDamage = pending.Power * Or(effect.Share, 1f);
            // A reaction's blow on a ripe seed is stronger, on an unripe one softer (In Its Season).
            if (pending.Ripe > 0f) eDamage *= pending.Ripe;
            // A whirl's cuts stagger by its shards, as they cut by them (Sickle Wind: 4 a shard).
            float eStagger = (byCount ? effect.Stagger : effect.Stagger * per) + effect.StaggerPerSpent * spentHere;
            float eAmount = effect.Amount * per;
            int eCount = byCount && effect.PerStack ? Or(effect.Count, 1) * per : effect.Count;
            int laid = effect.CarriesDepth ? Mathf.Max(1, pending.Spent) * Or(effect.Stacks, 1) : effect.Stacks;
            int eStacks = effect.CarriesDepth ? laid : effect.Do == UpgradeAction.Afflict ? Mathf.Max(1, Or(effect.Stacks, 1) * per - pending.Less) : byCount ? effect.Stacks : effect.Stacks * per;
            // Who it is done to, when it must be someone: the trigger's own enemy, or the one faced.
            Component subject = pending.Target;
            if (subject == null && target != null) subject = target.GetComponentInParent<IAfflictable>() as Component;
            // A reaction with its own bespoke look (combos 4) is seen in that look alone: its hazards bite quietly, with
            // no ring, puffs, sparks or bolt from the sky (Sickle Wind's whirl rang white rings every bite and The Bolt
            // Runs True dropped a white column of the sky's bolt over its own, the films of 2026-11-20).
            bool bespoke = ReactionLook.HasBespoke(effect.Owner);

            switch (effect.Do)
            {
                case UpgradeAction.Afflict:
                {
                    int stacks = eStacks;
                    if (!string.IsNullOrEmpty(effect.ThroughGround))
                    {
                        // Everyone standing in every patch of the named ground, once each (Low Mist: a dodge in it).
                        int patches = Hazards.AllNamed(effect.ThroughGround, groundBurstAt, groundBurstRadius);
                        fanTaken.Clear();
                        for (int i = 0; i < patches; i++)
                            combat.AfflictArea(groundBurstAt[i], groundBurstRadius[i], kind, stacks, null, laying, effect.OnlyCarrying, fanTaken);
                        fanTaken.Clear();
                        if (patches > 0) GroundsDosed++;
                    }
                    else if (effect.AlongDodge && effect.Radius > 0f)
                    {
                        // Everyone the dodge just passed, once each (Seed on the Wind).
                        Vector3 path = self - dodgeFrom;
                        path.y = 0f;
                        int steps = DodgePathSteps(path.magnitude, effect.Radius);
                        fanTaken.Clear();
                        for (int i = 0; i <= steps; i++)
                            combat.AfflictArea(dodgeFrom + path * (i / (float)steps), effect.Radius * spread, kind, stacks, null, laying, effect.OnlyCarrying, fanTaken);
                        fanTaken.Clear();
                        if (!bespoke) ElementLook.Ring(look, self + Vector3.up * 0.2f);
                    }
                    else if (effect.Radius > 0f)
                    {
                        // Sparing the trigger's own enemy, or only those carrying an affliction, when the mark says so.
                        combat.AfflictArea(centre, effect.Radius * spread, kind, stacks, effect.SparesTarget ? subject : null, laying, effect.OnlyCarrying);
                        // A reaction with its own bespoke look shows what it lays in that look (Smoulder's black flare).
                        if (!bespoke) ElementLook.Ring(look, centre + Vector3.up * 0.2f);
                    }
                    else if (subject is IAfflictable afflictable)
                    {
                        float seconds = combat.AfflictionSeconds != 1f ? Afflictions.Spec(kind).Seconds * combat.AfflictionSeconds : 0f;
                        CombatEvents.Report(subject, kind, stacks, afflictable.Afflictions.Lay(kind, stacks + combat.ExtraStacks, seconds, combat.AfflictionPower, laying, subject), laying);
                        if (!bespoke) ElementLook.Puff(look, centre + Vector3.up * 1.2f, Vector3.up, 0.6f);
                    }
                    break;
                }
                case UpgradeAction.Burst:
                {
                    var burst = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Position = centre, Radius = Or(effect.Radius, 2.5f) * spread, Seconds = 0.12f, BiteEvery = 0.1f,
                        Once = true, Burst = !effect.Unseen && !bespoke, Quiet = bespoke, Delay = effect.Delay, Blow = Unseen(Blow(look, kind, eDamage, eStagger, eStacks), bespoke), FromReaction = laying == LaySource.Reaction,
                        // A burst that starts from the one struck may leave that one alone (Earthed's fern).
                        Spares = effect.SparesTarget ? pending.Target as IDamageable : null,
                        // A star of cracks: only what stands on its seams (the ledger's seams).
                        Seams = effect.Seams, SeamWay = aim,
                        // What it does to each one standing in it as it lands (The Second Wave: thrown down if the first ring caught it).
                        Acts = effect.ActsAsGround ? effect : null, LaidFor = pending.Target, Source = effect,
                    };
                    // One burst, or one where each patch of a shape would fall.
                    int bursts = effect.Shape == HazardShape.Patch ? 1 : Or(eCount, 3);
                    // The parts of one burst are one blow: an enemy standing where two overlap is struck once.
                    if (bursts > 1) burst.Group = Hazards.NewGroup();
                    for (int i = 0; i < bursts; i++)
                    {
                        burst.Position = PatchAt(effect, i, bursts, centre, self, aim, Or(effect.Reach, 4f));
                        Hazards.Lay(burst);
                    }
                    break;
                }
                case UpgradeAction.Lane:
                {
                    float length = Or(effect.Reach, 8f) * spread, half = Or(effect.Radius, 0.6f), every = Or(effect.Interval, 0.03f);
                    if (effect.AimAt != AfflictionKind.None)
                    {
                        // To the nearest other enemy carrying what it seeks, and no farther; with none, the target alone.
                        var end = combat.EnemyCarrying(centre, length, effect.AimAt, effect.AimAt2, false, subject);
                        Vector3 toEnd = end != null ? end.transform.position - centre : Vector3.zero;
                        toEnd.y = 0f;
                        length = Mathf.Max(0.5f, toEnd.magnitude);
                        if (toEnd.sqrMagnitude > 0.04f) aim = toEnd.normalized;
                    }
                    int patches = LanePatches(length, half);
                    var run = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Radius = half, Seconds = 0.12f, BiteEvery = 0.1f, Once = true,
                        Burst = !effect.Unseen && !bespoke, Quiet = bespoke, Blow = Unseen(Blow(look, kind, eDamage, eStagger, eStacks), bespoke), FromReaction = laying == LaySource.Reaction,
                        // One blow down the whole lane: an enemy where two patches meet is struck once.
                        Group = Hazards.NewGroup(),
                        // A lane run out of the one struck (Carried Embers' column) may leave that one alone.
                        Spares = effect.SparesTarget ? pending.Target as IDamageable : null,
                    };
                    // A gust down the lane carries everything on it along it together (Hot Wind).
                    if (effect.Carry > 0f) LanesShoved += combat.ShoveLane(centre, aim, length, half, effect.Carry);
                    for (int i = 0; i < patches; i++)
                    {
                        run.Position = LanePatch(centre, aim, length, i, patches);
                        run.Delay = effect.Delay + i * every;
                        Hazards.Lay(run);
                    }
                    // The reaction's own bolt comes down again and again along the lane, running away from the player:
                    // the earlier art's bolt, unless the bespoke look draws its own (it drew a white column, 2026-11-20).
                    int pair = string.IsNullOrEmpty(effect.Reaction) ? -1 : ReactionTable.PairOf(effect.Reaction);
                    if (pair >= 0 && !bespoke)
                        for (int bolt = 1; bolt <= LaneBolts; bolt++)
                        {
                            float share = bolt / (float)LaneBolts;
                            Clover.Player.Forms.ReactionArt.PlayRole(pair, centre + aim * (length * share), aim, "strike", effect.Delay + share * (patches - 1) * every);
                        }
                    break;
                }
                case UpgradeAction.Cone:
                {
                    float reach = Or(effect.Reach, 4.5f);
                    if (effect.Behind)
                    {
                        // A half ring behind you flying out (Wings Behind): you keep facing as you were.
                        Vector3 back = -transform.forward;
                        back.y = 0f;
                        combat.StrikeConeFrom(self, back, reach, Or(effect.Arc, 180f), Blow(look, kind, eDamage, eStagger, eStacks));
                        for (int i = 1; i <= 3; i++) ElementLook.Puff(look, self + Vector3.up * 1.1f + back.normalized * (reach * i / 3.5f), back, 0.7f);
                        break;
                    }
                    transform.rotation = Quaternion.LookRotation(aim, Vector3.up);
                    combat.StrikeCone(reach, Or(effect.Arc, 90f), Blow(look, kind, eDamage, eStagger, eStacks));
                    // Amount: each enemy the cone struck is thrown straight away from the player (Drawn Back to Break's breaker),
                    // that many metres beyond the blow's own shove. A shove dies away as it goes, so it is sized by how far
                    // one carries (Enemy.ShoveTravel); an enemy mid-lunge still drives against it and falls short.
                    if (effect.Amount > 0f)
                        foreach (var struck in combat.LastStruck)
                            if (struck is IShovable thrown && struck is Component body)
                            {
                                Vector3 away = body.transform.position - self;
                                away.y = 0f;
                                if (away.sqrMagnitude > 0.0001f) thrown.Push(away.normalized * (effect.Amount * combat.StatPush / Enemy.ShoveTravel));
                            }
                    Vector3 hand = self + Vector3.up * 1.1f;
                    for (int i = 1; i <= 3; i++) ElementLook.Puff(look, hand + aim * (reach * i / 3.5f), aim, 0.7f);
                    ElementLook.Sparks(look, hand + aim * 1.5f, aim, 1.5f);
                    break;
                }
                case UpgradeAction.Hazard:
                {
                    int patches = effect.Shape == HazardShape.Patch ? 1 : Or(eCount, 4);
                    float reach = Or(effect.Reach, 4f) * spread;
                    // Laid along the dodge just taken, from where it began (Wake).
                    if (effect.AlongDodge && effect.Shape == HazardShape.Line)
                    {
                        Vector3 path = self - dodgeFrom;
                        path.y = 0f;
                        if (path.sqrMagnitude > 0.04f)
                        {
                            reach = path.magnitude;
                            aim = path / reach;
                            self = dodgeFrom;
                        }
                    }
                    var spec = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Radius = Or(effect.Radius, 1f) * spread * Stats.Multiplier(PlayerStat.GroundSize), Seconds = Or(effect.Seconds, 3f) * Stats.Multiplier(PlayerStat.GroundSeconds), BiteEvery = Or(effect.Interval, 0.5f),
                        // A limit of one effect's own ground (The Sea Comes In's four pools), or of grown things, which the marks may raise.
                        Grown = effect.Grown, Family = FamilyOf(effect), Limit = effect.Limit > 0 ? effect.Limit + (effect.Grown ? Mathf.Max(0, Stats.Whole(PlayerStat.GrownLimit)) : 0) : 0,
                        Blow = Blow(look, kind, eDamage, eStagger, eStacks), FromReaction = laying == LaySource.Reaction,
                        Spares = effect.SparesTarget ? pending.Target as IDamageable : null,
                        // Unseen ground is drawn by its reaction's look alone (Seized's film: no bolt at each bite).
                        Burst = false, Quiet = effect.Unseen || bespoke,
                        // A second affliction laid with each bite (the hollow trunk Seeding and Cursing what touches it).
                        AlsoLays = effect.BitesLay,
                        // Ground that keeps acting on whoever stands in it (the ledger's ground-states).
                        Acts = effect.ActsAsGround ? effect : null, Pull = effect.Pull, LaidFor = pending.Target, Source = effect, Name = !string.IsNullOrEmpty(effect.Ground) ? effect.Ground : effect.Named,
                        // Ground that goes with its enemy (Heavy Silver), and a stone that cracks once when stood at (Fire-Cracked).
                        Follow = effect.Follows && target != null, Target = effect.Follows ? target : null, CrackReach = effect.CracksAt,
                        // A lane of foam carries what stands in it away from the player (the ledger's lane-shove).
                        Carry = effect.Carry > 0f ? aim * GroundRules.PullStep(effect.Carry, Or(effect.Interval, 0.5f)) : Vector3.zero,
                    };
                    spec.Blow.Unseen = effect.Unseen || bespoke;
                    // A band that runs on away from you as one (Running Water).
                    if (effect.Shape == HazardShape.Band && effect.Speed > 0f) spec.Velocity = new Vector3(aim.x, 0f, aim.z) * effect.Speed;
                    for (int i = 0; i < patches; i++)
                    {
                        spec.Position = PatchAt(effect, i, patches, centre, self, aim, reach);
                        spec.Delay = effect.Delay + (effect.Shape == HazardShape.Line || effect.Shape == HazardShape.Downwind ? i * 0.06f : 0f);
                        Hazards.Lay(spec);
                    }
                    break;
                }
                case UpgradeAction.Arc:
                {
                    if (target == null) target = combat.EnemyAhead(FightRange);
                    if (target == null) break;
                    if (effect.LeapsTo != AfflictionKind.None || effect.FromTarget)
                    {
                        // From the one struck, never back into it: only to those carrying what the arc seeks (iron to iron).
                        // Answering a blow, it carries a share of that blow (Amount); else its own Damage.
                        float arcDamage = effect.Amount > 0f && pending.Power > 0f ? pending.Power * effect.Amount : eDamage;
                        ArcsLeapt += combat.StrikeChainFrom(target, Blow(look, kind, arcDamage, eStagger, eStacks), Or(eCount, 2),
                            Or(effect.Reach, 5f) * spread * Stats.Multiplier(PlayerStat.LeapReach), look, effect.LeapsTo, effect.LeapsOnFrom);
                        // What the arcs reached, and only that, folds in toward the one struck (Drawn Iron).
                        if (effect.DrawsIn > 0f) combat.DrawStruckToward(target.position, effect.DrawsIn);
                        break;
                    }
                    combat.StrikeChain(0f, Blow(look, kind, eDamage, eStagger, eStacks), Or(eCount, 2), Or(effect.Reach, 5f) * spread * Stats.Multiplier(PlayerStat.LeapReach), look, target);
                    break;
                }
                case UpgradeAction.Projectile:
                {
                    int thrown = Or(eCount, 1);
                    var spec = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Radius = Or(effect.Radius, 0.9f), Seconds = Or(effect.Seconds, 1.2f), BiteEvery = 0.05f, Once = true,
                        Flight = effect.Flight, Target = target, Blow = Blow(look, kind, eDamage, eStagger, eStacks),
                    };
                    for (int i = 0; i < thrown; i++)
                    {
                        // More than one go out in a fan.
                        Vector3 way = Quaternion.Euler(0f, (i - (thrown - 1) * 0.5f) * 14f, 0f) * aim;
                        spec.Position = self + way * 0.8f;
                        spec.Velocity = way * Or(effect.Speed, 10f);
                        Hazards.Lay(spec);
                    }
                    ElementLook.Sparks(look, self + Vector3.up * 1.1f + aim * 0.6f, aim, 1f);
                    break;
                }
                case UpgradeAction.Falling:
                {
                    int falling = Or(eCount, 1);
                    float radius = Or(effect.Radius, 2f);
                    var spec = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Radius = radius, Seconds = 0.12f, BiteEvery = 0.1f, Once = true, Burst = !bespoke, Warn = !bespoke, Quiet = bespoke,
                        Blow = Unseen(Blow(look, kind, eDamage, eStagger, eStacks), bespoke),
                    };
                    for (int i = 0; i < falling; i++)
                    {
                        // The first falls on the spot; the rest fall round it, one after another.
                        spec.Position = i == 0 ? centre : centre + Quaternion.Euler(0f, (i - 1) * 360f / Mathf.Max(1, falling - 1), 0f) * Vector3.forward * (radius * 1.3f);
                        spec.Delay = Or(effect.Delay, 0.6f) + i * 0.12f;
                        Hazards.Lay(spec);
                    }
                    break;
                }
                case UpgradeAction.Form:
                {
                    var form = SpellForms.Get(effect.Form);
                    if (form == null) break;
                    var cast = element.NewCast(effect.Spell ?? plainSpell, effect.Owner != null ? effect.Owner.Name : "", effect.Owner);
                    // Seen and laid in the way of the affliction the effect names; with none, in its mark's element.
                    cast.Element = look;
                    cast.Colour = look.Colour;
                    cast.Affliction = kind;
                    // An effect's spell is no slot cast: its blows never charge the blade.
                    cast.Cast = 0;
                    if (target != null)
                    {
                        cast.Target = target;
                        cast.Aim = target.position;
                    }
                    FormsBegun++;
                    form.Cast(cast);
                    break;
                }
                case UpgradeAction.Heal:
                    if (defence != null) defence.Heal(effect.ByTurnedWound ? PlayerDefence.TurnedMend(defence.LastTurnedWound, eAmount) : eAmount);
                    break;
                case UpgradeAction.Mana:
                    combat.Mana.Gain(eAmount);
                    break;
                case UpgradeAction.Stamina:
                    player.Stamina.Restore(eAmount);
                    break;
                case UpgradeAction.Boon:
                    // Held by the cast alone: only its own blows and bursts are made stronger (One by One's Master).
                    if (effect.ThisCast)
                    {
                        castBoons.Add(pending.CastTag, effect.Stat, effect.Amount, Time.time + Or(effect.Seconds, 5f) * Stats.Multiplier(PlayerStat.BoonSeconds));
                        ElementLook.Sparks(held, self + Vector3.up * 1.2f, Vector3.up, 0.6f);
                        break;
                    }
                    float boonSeconds = Or(effect.Seconds, 5f);
                    // By the count before it (Drawn From the Wood: 1.5 s for each grown thing withered, 10 s at most).
                    if (effect.SecondsEach > 0f)
                    {
                        if (counted <= 0) break;
                        boonSeconds = Mathf.Min(boonSeconds, effect.SecondsEach * counted);
                    }
                    Stats.AddBoon(effect.Stat, effect.Amount, boonSeconds * Stats.Multiplier(PlayerStat.BoonSeconds));
                    if (effect.UntilBlow && untilBlowCount < untilBlow.Length) untilBlow[untilBlowCount++] = effect;
                    if (effect.UntilDodge && untilDodgeCount < untilDodge.Length) untilDodge[untilDodgeCount++] = effect;
                    ElementLook.Sparks(held, self + Vector3.up * 1.2f, Vector3.up, 0.6f);
                    break;
                case UpgradeAction.Loose:
                    if (rings.Active) LooseRings();
                    break;
                case UpgradeAction.Flood:
                    if (Ebbing) LooseFlood();
                    break;
                case UpgradeAction.SwapWithShadow:
                case UpgradeAction.ReturnToShadow:
                {
                    var shadow = Forms.SpellWorks.Clone;
                    if (shadow == null || !shadow.Showing || !shadow.Anchored) break;
                    Vector3 mine = transform.position;
                    Vector3 theirs = shadow.Position;
                    ElementLook.Puff(held, mine + Vector3.up * 0.4f, Vector3.up, 1.1f);
                    ElementLook.Puff(held, theirs + Vector3.up * 0.4f, Vector3.up, 1.1f);
                    player.PassTo(theirs);
                    if (effect.Do == UpgradeAction.SwapWithShadow)
                    {
                        shadow.AnchorAt(mine);
                        PlacesSwapped++;
                        break;
                    }
                    // Sunk into it: the copy is gone, and the wounds since it was left come back as grey.
                    if (SpellForms.Get(SpellForms.Clone) is Forms.RunningForm copy) Forms.SpellWorks.EndRunsOf(copy);
                    if (defence != null) WoundsReturned += defence.Health.Return(WoundsSince(shadowHealth, defence.Health.Current));
                    break;
                }
                case UpgradeAction.StepToStanding:
                {
                    string branch = effect.Owner != null ? effect.Owner.Branch : "";
                    if (!StandingOf(branch)) break;
                    Vector3 to = standingAt;
                    to.y = self.y;
                    Vector3 way = to - self;
                    way.y = 0f;
                    if (afterimage != null) afterimage.Leave(0.35f);
                    ElementLook.Sparks(held, self + Vector3.up, way, 1f);
                    // Beside the rod, not on its point: half a metre short, along the way.
                    Vector3 landing = way.sqrMagnitude > 0.36f ? to - way.normalized * 0.5f : self;
                    player.PassTo(landing);
                    if (eDamage > 0f || eStagger > 0f)
                        combat.StrikeArea(landing, Or(effect.Radius, 2.5f), Blow(look, kind, eDamage, eStagger, effect.Affliction != AfflictionKind.None ? eStacks : 0));
                    ElementLook.Strike(look, landing, 0.5f);
                    if (!KeepsStanding(byTrigger[(int)UpgradeTrigger.SpellPressed], branch))
                    {
                        Hazards.EndGroup(standingGroup);
                        standingMark = null;
                    }
                    StepsToStanding++;
                    break;
                }
                case UpgradeAction.Pieces:
                {
                    Vector3 from = target != null ? target.position : centre;
                    from.y = self.y;
                    Vector3 away = from - self;
                    away.y = 0f;
                    away = away.sqrMagnitude > 0.01f ? away.normalized : transform.forward;
                    var left = target != null ? target.GetComponentInParent<IDamageable>() : null;
                    var pieceBlow = Blow(look, kind, eDamage, eStagger, eStacks);
                    float reach = Or(effect.Reach, 6f);
                    if (effect.Thrown == PieceAim.FarCone)
                    {
                        int crossed = combat.StrikeConeFrom(from, away, reach, Or(effect.Arc, 60f), pieceBlow, left);
                        // The second affliction it lays, on the same cone (a Mark and a Chill from Prism's rays).
                        if (effect.BitesLay != AfflictionKind.None && crossed > 0)
                            combat.StrikeConeFrom(from, away, reach, Or(effect.Arc, 60f), Blow(lookOf[(int)effect.BitesLay] ?? look, effect.BitesLay, 0f, 0f, 1), left);
                        for (int i = 0; i < 3; i++) ElementLook.Sparks(look, from + Vector3.up, Quaternion.Euler(0f, (i - 1) * Or(effect.Arc, 60f) * 0.35f, 0f) * away, 1f);
                        PiecesThrown++;
                        break;
                    }
                    float speed = Or(effect.Speed, 12f);
                    var piece = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Radius = Or(effect.Radius, 0.6f), Seconds = reach / speed, BiteEvery = 0.05f, Once = true,
                        StopsAtFirst = true, Spares = left, Blow = pieceBlow, AlsoLays = effect.BitesLay, FromReaction = laying == LaySource.Reaction,
                    };
                    int pieces = Or(eCount, 3);
                    if (effect.Thrown == PieceAim.AtNearest)
                    {
                        pieceExcept.Clear();
                        if (left != null) pieceExcept.Add(left);
                        for (int i = 0; i < pieces; i++)
                        {
                            var next = combat.EnemyNear(from, reach, pieceExcept);
                            if (!(next is IDamageable aimed)) break;
                            pieceExcept.Add(aimed);
                            Vector3 way = next.transform.position - from;
                            way.y = 0f;
                            way = way.sqrMagnitude > 0.01f ? way.normalized : away;
                            piece.Flight = Flight.Homing;
                            piece.Target = next.transform;
                            piece.Seconds = (reach + 2f) / speed;
                            piece.Position = from + way * 0.6f;
                            piece.Velocity = way * speed;
                            Hazards.Lay(piece);
                            PiecesThrown++;
                        }
                        pieceExcept.Clear();
                        break;
                    }
                    // A fan rides the way its enemy was last shoved, if lately (downwind), else away from you.
                    if (effect.Thrown == PieceAim.Fan && target != null && target.GetComponentInParent<Enemy>() is Enemy shoved)
                        away = PieceRules.Downwind(away, shoved.LastShoveWay, shoved.LastShovedAt, Time.time);
                    int made = PieceRules.Ways(effect.Thrown, away, pieces, Or(effect.Arc, 40f), pieceWays);
                    for (int i = 0; i < made; i++)
                    {
                        piece.Position = from + pieceWays[i] * 0.6f;
                        piece.Velocity = pieceWays[i] * speed;
                        Hazards.Lay(piece);
                        PiecesThrown++;
                    }
                    break;
                }
                case UpgradeAction.DropPickup:
                {
                    int dropOwner = effect.Owner != null ? effect.Owner.Index : -1;
                    int drops = effect.PerStack ? Or(effect.Count, 1) * per : Or(effect.Count, 1);
                    Vector3 enemyAt = target != null ? target.position : pending.HasAt ? pending.At : centre;
                    for (int i = 0; i < drops; i++)
                    {
                        Vector3 spot = effect.Shape == HazardShape.Patch
                            ? StandingRules.PlaceFor(effect.RaiseAt, self, transform.forward, enemyAt, Or(effect.Reach, 0f))
                            : PatchAt(effect, i, drops, new Vector3(enemyAt.x, self.y, enemyAt.z), self, aim, Or(effect.Reach, 2.5f));
                        spot.y = self.y;
                        Pickups.Drop(effect.Drops, spot, effect.Seconds, dropOwner, Time.time, effect.Limit, effect.Radius, effect);
                        PickupsDropped++;
                    }
                    break;
                }
                case UpgradeAction.DrawPickups:
                    Pickups.DrawAll(effect.Drops);
                    break;
                case UpgradeAction.PlayLook:
                    break;
                case UpgradeAction.Scatter:
                {
                    float scattered = Or(effect.Seconds, 0.4f);
                    if (player != null) player.Hidden = true;
                    if (defence != null) defence.UntouchableUntil = Mathf.Max(defence.UntouchableUntil, Time.time + scattered);
                    scatteredUntil = Time.time + scattered;
                    for (int i = 0; i < 4; i++) ElementLook.Puff(held, self + Vector3.up * (0.4f + 0.4f * i), Vector3.up, 0.8f);
                    Scatters++;
                    break;
                }
                case UpgradeAction.OwnGround:
                {
                    // Ground of your own by its name, which bites nothing: the stats and marks ask who stands in it.
                    float until = Time.time + Or(effect.Seconds, 5f) * Stats.Multiplier(PlayerStat.GroundSeconds);
                    if (effect.Shape == HazardShape.Line)
                    {
                        float length = Or(effect.Reach, 6f) * spread;
                        Vector3 way = new Vector3(aim.x, 0f, aim.z).normalized;
                        if (effect.AcrossFront)
                        {
                            Vector3 right = new Vector3(way.z, 0f, -way.x);
                            OwnGrounds.LayLane(effect.Ground, self + way * Or(effect.Amount, 1.5f) - right * (length * 0.5f), right, length, Or(effect.Radius, 0.5f), until);
                        }
                        else OwnGrounds.LayLane(effect.Ground, self, way, length, Or(effect.Radius, 1f), until);
                    }
                    else OwnGrounds.Lay(effect.Ground, centre, Or(effect.Radius, 1f) * spread, until);
                    break;
                }
                case UpgradeAction.LightCoat:
                {
                    // Every coated enemy goes up at once, harder for each blow that spread the coat (the mark's tally, emptied).
                    var owner = effect.Owner;
                    int spreads = 0;
                    if (owner != null && owner.Tally != null && owner.Index < tally.Length)
                    {
                        spreads = tally[owner.Index];
                        tally[owner.Index] = 0;
                        tallyDirty = true;
                    }
                    float burst = CoatBurst(eDamage, effect.DamagePerSpent, spreads, effect.Amount);
                    fanTaken.Clear();
                    coatStruck.Clear();
                    for (int guard = 0; guard < 32; guard++)
                    {
                        var near = combat.EnemyNear(centre, Or(effect.Reach, 30f), fanTaken);
                        if (!(near is IDamageable one)) break;
                        fanTaken.Add(one);
                        if (!(near is IAfflictable coated) || !coated.Afflictions.Conditions.Holds(effect.Condition)) continue;
                        coated.Afflictions.Conditions.EndNow(effect.Condition);
                        Vector3 at = near.transform.position;
                        combat.StrikeArea(at, Or(effect.Radius, 1.2f), Blow(look, kind, burst, eStagger, eStacks), coatStruck);
                        if (effect.Seconds > 0f) Hazards.Lay(HazardKind.Fire, at, Or(effect.Radius, 1.2f), effect.Seconds, 2f, 0f, 1f);
                        ElementLook.Puff(look, at + Vector3.up * 0.6f, Vector3.up, 1.4f);
                        CoatsLit++;
                    }
                    fanTaken.Clear();
                    coatStruck.Clear();
                    break;
                }
                case UpgradeAction.CarryGround:
                {
                    // The named ground lying on the lane ahead rides on with the spell (Riptide's wave carrying the slicks).
                    Vector3 way = new Vector3(aim.x, 0f, aim.z).normalized;
                    GroundsCarried += Hazards.Drift(effect.ThroughGround, self, way, Or(effect.Reach, 7f) * spread, Or(effect.Radius, 1.5f), way * Or(effect.Speed, 8f), Or(effect.Seconds, 0.8f));
                    break;
                }
                case UpgradeAction.CountAsBlow:
                    combat.CountAsBlowLanded();
                    break;
                case UpgradeAction.FreeDodge:
                    player.Stamina.Restore(player.Dodge.StaminaCost);
                    FreeDodges++;
                    break;
                case UpgradeAction.HeavyUnbroken:
                    heavyUnbrokenUntil = Mathf.Max(heavyUnbrokenUntil, Time.time + Or(effect.Seconds, 1f));
                    combat.HeavyUnbroken = true;
                    break;
                case UpgradeAction.BurstGround:
                {
                    // Every patch the named ground still has bursts where it lies, all at once, and is gone.
                    int burst = Hazards.TakeAllOf(effect.BurstsGround, groundBurstAt, groundBurstRadius);
                    var blow = Blow(look, kind, eDamage, eStagger, eStacks);
                    for (int i = 0; i < burst; i++)
                    {
                        combat.StrikeArea(groundBurstAt[i], groundBurstRadius[i], blow);
                        ElementLook.Strike(look, groundBurstAt[i], 0.3f);
                    }
                    GroundsBurst += burst;
                    break;
                }
                case UpgradeAction.LengthenOwn:
                    Forms.SpellWorks.LengthenRunsOf(effect.Owner, Or(effect.Seconds, 1f));
                    break;
                case UpgradeAction.CriticalNext:
                    combat.PromiseCritical(Or(effect.Seconds, 4f));
                    ElementLook.Sparks(held, self + Vector3.up * 1.2f, Vector3.up, 0.6f);
                    break;
                case UpgradeAction.Raise:
                {
                    // Of a moment that names an enemy, placed against it; of one that names only a place (a death), there.
                    Vector3 enemyAt = target != null ? target.position : pending.HasAt ? pending.At : centre;
                    Vector3 spot = StandingRules.PlaceFor(effect.RaiseAt, self, transform.forward, enemyAt, Or(effect.Reach, effect.RaiseAt == StandingPlace.At ? 0f : 1.2f));
                    spot.y = self.y;
                    int owner = effect.Owner != null ? effect.Owner.Index : -1;
                    if (effect.Limit > 0) StandingThings.KeepAtMost(owner, effect.Limit - 1);
                    Vector3 front = self - spot;
                    StandingThings.Raise(effect.Raises, spot, front, effect.Seconds, effect.Amount, Time.time, owner, effect.Delay, effect.Draws);
                    ElementLook.Puff(look, spot + Vector3.up * 0.4f, Vector3.up, 1.2f);
                    ThingsRaised++;
                    break;
                }
                case UpgradeAction.BreakStanding:
                    if (StandingThings.BreakNear(pending.HasAt ? pending.At : centre, Or(effect.Radius, 1f), effect.Owner != null ? effect.Owner.Index : -1)) StandingAnswers++;
                    break;
                case UpgradeAction.DriveBlades:
                    if (target != null) PlantedBlades.DriveAllIn(target.GetComponentInParent<IDamageable>() as Component);
                    break;
                case UpgradeAction.PlantBlades:
                {
                    var tap = SpellForms.Get(SpellForms.Tap);
                    if (tap == null) break;
                    // Each into a different enemy: never the one faced (the kit's own blade goes there), nor one already holding a blade.
                    plantExcept.Clear();
                    var faced = combat.EnemyAhead(Or(effect.Reach, 9f));
                    if (faced != null && faced.GetComponentInParent<IDamageable>() is IDamageable facedBody) plantExcept.Add(facedBody);
                    int thrownBlades = 0;
                    for (int guard = 0; guard < 12 && thrownBlades < Or(eCount, 2); guard++)
                    {
                        var next = combat.EnemyNear(self, Or(effect.Reach, 9f), plantExcept);
                        if (!(next is IDamageable body)) break;
                        plantExcept.Add(body);
                        if (PlantedBlades.CountIn(next) > 0 || !PlayerReactions.Alive(next)) continue;
                        var cast = element.NewCast(effect.Spell ?? plainSpell, effect.Owner != null ? effect.Owner.Name : "", effect.Owner);
                        cast.Element = held;
                        cast.Colour = held.Colour;
                        cast.Affliction = held.Affliction;
                        cast.Cast = 0;
                        cast.Target = next.transform;
                        cast.Aim = next.transform.position;
                        tap.Cast(cast);
                        thrownBlades++;
                        BladesPlanted++;
                    }
                    plantExcept.Clear();
                    break;
                }
                case UpgradeAction.CallBladesHome:
                {
                    int called = PlantedBlades.RecallAll(bladePlaces);
                    var home = Blow(look, kind, eDamage, eStagger, eStacks);
                    for (int i = 0; i < called && i < bladePlaces.Length; i++)
                    {
                        Vector3 from = bladePlaces[i];
                        from.y = self.y;
                        Vector3 line = self - from;
                        line.y = 0f;
                        // Through whatever stands between: every enemy on the blade's straight line home.
                        if (line.sqrMagnitude > 0.04f) combat.StrikeLane(from, line.normalized, line.magnitude, Or(effect.Radius, 0.7f), home);
                        ElementLook.Sparks(look, from + Vector3.up, line, 0.8f);
                    }
                    if (called > 0 && element.SurgeActive) element.SurgeKit.Lengthen(called * Or(effect.Seconds, 0.4f));
                    BladesCalledHome += called;
                    break;
                }
                case UpgradeAction.Regain:
                    if (effect.Seconds > 0f)
                    {
                        // Armed for a while: each weapon blow landed beside what its kin grew wins it back (OnBlow).
                        regainUntil = Time.time + effect.Seconds;
                        regainAmount = Or(effect.Amount, 3f);
                        regainNear = Or(effect.Reach, 3f);
                        regainFamily = effect.Kin > 0 ? FamilyOf(effect) : 0;
                    }
                    else if (defence != null) defence.Health.Regain(Or(effect.Amount, 3f));
                    break;
                case UpgradeAction.UndoWound:
                    if (defence != null && !defence.IsDead && Time.time - defence.LastWoundAt <= Or(effect.CastWindow, 1f) && defence.LastWound > 0f)
                    {
                        defence.Heal(defence.LastWound, false);
                        WoundsUndone++;
                    }
                    break;
                case UpgradeAction.Spill:
                    if (rings.Active && effect.Owner != null && rings.Mark.Branch == effect.Owner.Branch)
                    {
                        rings.End();
                        RingsSpilled++;
                    }
                    break;
                case UpgradeAction.FreeBase:
                    if (element != null) element.CastBaseFree(OwnElement(effect.Owner).Name);
                    break;
                case UpgradeAction.CarryOn:
                {
                    // The wave goes out again the way it came, through the player's place, and sets what it brought down beyond.
                    for (int i = 0; i < broughtCount; i++)
                    {
                        var body = brought[i];
                        brought[i] = null;
                        if (!PlayerReactions.Alive(body)) continue;
                        Vector3 to = CarriedOnTo(transform.position, broughtWay, Or(effect.Reach, 4f));
                        Vector3 shove = to - body.transform.position;
                        shove.y = 0f;
                        // A shove is spent over a moment and moves two thirds of its length: half as much again sets it down there.
                        if (body is IShovable carried) carried.Push(shove * 1.5f);
                        if (effect.State != null) ConditionsSet += ConditionCaster.Cast(effect, body, to, combat, laying, conditionScratch);
                        ElementLook.Sparks(look, body.transform.position + Vector3.up * 0.4f, shove, 1f);
                        CarriedOn++;
                    }
                    broughtCount = 0;
                    break;
                }
                case UpgradeAction.RestReaction:
                {
                    int resting = ReactionTable.PairOf(effect.Reaction);
                    if (resting >= 0 && subject is IAfflictable rester) rester.Afflictions.RestReaction(resting, Or(effect.Seconds, AfflictionPairs.Rest(resting)));
                    break;
                }
                case UpgradeAction.ShortenRest:
                {
                    int shortened = ReactionTable.PairOf(effect.Reaction);
                    if (shortened >= 0 && subject is IAfflictable quick) quick.Afflictions.ShortenRest(shortened, effect.Seconds);
                    break;
                }
                case UpgradeAction.PayNow:
                    if (reactionsFound != null && reactionsFound.PayAsEnded(subject, effect.ToState)) PaidNow++;
                    break;
                case UpgradeAction.Plate:
                    if (defence != null) defence.GivePlates(Or(eCount, 1), Or(effect.Amount, PlayerDefence.PlainPlateShare));
                    Vfx.Play(VfxKind.TintRing, self + Vector3.up * 0.2f, Vector3.up, 1f, new Color(0.62f, 0.55f, 0.45f));
                    break;
                case UpgradeAction.Ward:
                    // Kept for the trigger's enemy alone (Ash on the Brow): only its next blow is turned.
                    if (effect.OnTarget)
                    {
                        if (defence == null || subject == null) break;
                        defence.KeepWardFor(subject, Time.time + Or(effect.Seconds, 6f));
                    }
                    else if (defence != null) defence.GiveWards(Or(eCount, 1));
                    Vfx.Play(VfxKind.TintRing, self + Vector3.up * 0.2f, Vector3.up, 1f, new Color(0.7f, 0.92f, 1f));
                    break;
                case UpgradeAction.Step:
                {
                    float reach = Or(effect.Reach, 8f);
                    Vector3 to;
                    // In front of it, or through it and out the far side.
                    if (target != null) to = self + aim * Forms.FormRules.MarkStep(toward.magnitude, reach, effect.Amount);
                    else to = self + transform.forward * (reach * 0.5f);
                    if (afterimage != null) afterimage.Leave(0.35f);
                    ElementLook.Sparks(held, self + Vector3.up, Vector3.up, 1f);
                    player.StepTo(to);
                    if (target != null)
                    {
                        Vector3 back = target.position - transform.position;
                        back.y = 0f;
                        if (back.sqrMagnitude > 0.04f) transform.rotation = Quaternion.LookRotation(back, Vector3.up);
                    }
                    ElementLook.Sparks(held, transform.position + Vector3.up, Vector3.up, 1f);
                    break;
                }
                case UpgradeAction.Pull:
                case UpgradeAction.Push:
                    // The chain-mate drawn to the place of the one the chain is on (Dead Weight: to where it fell).
                    if (effect.ToChainMate && pending.HasAt && subject is IShovable drawn && target != null)
                    {
                        Vector3 toFallen = pending.At - target.position;
                        toFallen.y = 0f;
                        drawn.Push(toFallen * (effect.Do == UpgradeAction.Pull ? 1f : -1f));
                        break;
                    }
                    if (effect.Shape == HazardShape.Downwind)
                    {
                        // The target alone, along the player's line to it: thrown straight away from them, or drawn in.
                        // Sideways: across your front to your left, along a sweep from your right (Sweep of Wings).
                        Vector3 along = effect.Sideways ? PushRules.Sideways(aim) : aim;
                        if (subject is IShovable shoved)
                            shoved.Push((effect.Do == UpgradeAction.Pull ? -along : along * combat.StatPush) * Or(eAmount, 3f));
                        ElementLook.Ring(look, centre + Vector3.up * 0.2f);
                        break;
                    }
                    combat.Displace(centre, Or(effect.Radius, 5f) * spread, Or(effect.Amount, 3f), effect.Do == UpgradeAction.Pull, effect.OnlyCarrying);
                    ElementLook.Ring(held, centre + Vector3.up * 0.2f);
                    break;
                case UpgradeAction.SlowTime:
                {
                    WorldClock.SlowEnemies(Or(effect.Seconds, 2f), Or(effect.Amount, 0.35f));
                    Color breath = Color.Lerp(Color.white, held.Colour, 0.5f);
                    breath.a = 0.3f;
                    ScreenFlash.Flash(breath, 0.08f);
                    break;
                }
                case UpgradeAction.Echo:
                {
                    float delay = Or(effect.Delay, 0.3f);
                    if (afterimage != null) afterimage.Leave(delay + 0.25f);
                    Hazards.Lay(new HazardSpec
                    {
                        Kind = ElementLook.Ground(held), Tint = held.Colour, Position = self, Radius = Or(effect.Radius, 2.3f), Seconds = 0.12f, BiteEvery = 0.1f, Once = true, Burst = true,
                        Delay = delay, Blow = Blow(held, kind, combat.LastBlowDamage * Or(effect.Amount, 0.6f), eStagger, 0),
                    });
                    break;
                }
                case UpgradeAction.GrownBolts:
                {
                    if (subject == null || !PlayerReactions.Alive(subject)) break;
                    int sources = Hazards.GrownWithin(subject.transform.position, Or(effect.Reach, 8f), grownBursts);
                    sources = Mathf.Min(sources, Or(eCount, 4));
                    for (int b = 0; b < sources; b++)
                    {
                        Vector3 from = grownBursts[b];
                        if (!combat.StrikeOne(subject, Blow(look, kind, eDamage, eStagger, eStacks), from)) continue;
                        ElementLook.Sparks(look, from + Vector3.up * 0.8f, (subject.transform.position - from).normalized, 0.8f);
                        GrownBoltsThrown++;
                        Trigger(UpgradeTrigger.GrownBolt, null, from);
                    }
                    break;
                }
                case UpgradeAction.Tend:
                {
                    if (!Hazards.Tend(centre, Or(effect.Radius, 3f), effect.Seconds, effect.BitesLay, out Vector3 tended)) break;
                    GrownTended++;
                    if (eDamage > 0f || eStagger > 0f || eStacks > 0)
                    {
                        combat.StrikeArea(tended, Or(effect.Reach, 2f), Blow(look, kind, eDamage, eStagger, eStacks));
                        ElementLook.Ring(look, tended + Vector3.up * 0.2f);
                    }
                    break;
                }
                case UpgradeAction.Helper:
                {
                    // One death raises one helper, however many of the mark's rules it meets (Nothing Stays Buried's Cursed death while Risen).
                    if (target != null && helperFor == target && helperFrame == Time.frameCount && effect.Owner == helperOwner) break;
                    helperFor = target;
                    helperFrame = Time.frameCount;
                    helperOwner = effect.Owner;
                    int strikes = Or(eCount, 3);
                    float interval = Or(effect.Interval, 0.6f);
                    float walk = 0f;
                    // A helper that walks (Speed): it rises where its moment was and goes to the nearest other enemy, then strikes it
                    // (The Drops Quicken: from where the rimed enemy fell).
                    if (effect.Speed > 0f)
                    {
                        helperExcept.Clear();
                        if (target != null && target.GetComponentInParent<IDamageable>() is IDamageable fell) helperExcept.Add(fell);
                        var nearest = combat.EnemyNear(centre, Or(effect.Reach, 10f), helperExcept);
                        helperExcept.Clear();
                        if (nearest == null) break;
                        target = nearest.transform;
                        walk = effect.Speed;
                    }
                    Hazards.Lay(new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Position = centre, Radius = Or(effect.Radius, 2f), BiteEvery = interval,
                        // A walker's time to get there is its own: as long as the walk at most, then its strikes.
                        Seconds = strikes * interval + 0.2f + (walk > 0f ? Or(effect.Reach, 10f) / walk : 0f),
                        Bites = strikes, Burst = !bespoke, Quiet = bespoke, Follow = target != null, Target = target, Blow = Unseen(Blow(look, kind, eDamage, eStagger, eStacks), bespoke),
                        // Its helpers at once, at most (Nothing Stays Buried: three); one more takes the place of the one nearest its end.
                        Family = effect.Limit > 0 ? FamilyOf(effect) : 0, Limit = effect.Limit,
                        WalksTo = walk, AlsoLays = effect.BitesLay, Name = effect.Named,
                    });
                    break;
                }
                case UpgradeAction.Feed:
                {
                    // A shard torn from the trigger's enemy goes to the whirl only when a whirl stands near (Wind Sickle).
                    var torn = effect.TearsNeeds ? subject as IAfflictable : null;
                    if (effect.TearsNeeds && (torn == null || !torn.Afflictions.Has(effect.Needs))) break;
                    if (!Hazards.Feed(effect.Named, centre, Or(effect.Reach, 3f), eDamage, eStagger, effect.Affliction, Or(effect.Stacks, 1))) break;
                    if (torn != null) torn.Afflictions.Spend(effect.Needs, 1);
                    Fed++;
                    ElementLook.Sparks(look, centre + Vector3.up * 0.8f, Vector3.up, 0.6f);
                    break;
                }
                case UpgradeAction.Pass:
                {
                    passExcept.Clear();
                    if (subject is IDamageable bearer) passExcept.Add(bearer);
                    float passReach = Or(effect.Reach, 4f) * Stats.Multiplier(PlayerStat.LeapReach);
                    if (effect.ToLink)
                    {
                        // Drawn out of the joined enemy along the roots into its host, as deep as the host can take.
                        if (!(pending.Link is IAfflictable host) || pending.Link == subject || !(subject is IAfflictable giver) || !pending.Link.gameObject.activeInHierarchy) break;
                        int moved = DepthMoves.Movable(giver.Afflictions, host.Afflictions, kind, Mathf.Max(1, eStacks));
                        if (moved <= 0) break;
                        giver.Afflictions.Spend(kind, moved);
                        CombatEvents.Report(pending.Link, kind, moved, host.Afflictions.Lay(kind, moved, 0f, combat.AfflictionPower, laying, pending.Link), laying);
                        ElementLook.Sparks(look, centre + Vector3.up * 0.4f, (pending.Link.transform.position - centre).normalized, 0.8f);
                        Passed++;
                        break;
                    }
                    int give = Mathf.Max(1, eStacks);
                    Vector3 from = centre;
                    // Pushed past the taker's depth, the rest bursts it and runs on, a few bodies at most (Eaten in Turn).
                    for (int hop = 0; hop < 4 && give > 0; hop++)
                    {
                        // By the line's own order when it keeps one (Eaten in Turn), the first hop only; else the nearest in the state.
                        var next = (effect.InOrder && hop == 0 ? NextOnLine(effect.ToState, pending.Power, from, passReach, passExcept) : null)
                            ?? NearestInState(effect.ToState, from, passReach, passExcept) ?? combat.EnemyNear(from, passReach, passExcept);
                        if (!(next is IAfflictable taker) || !(next is IDamageable passedTo)) break;
                        passExcept.Add(passedTo);
                        // The reaction it may set off there starts along its count (What the Yew Keeps: the fig at two coils).
                        if (effect.CountedBlows > 0) taker.Afflictions.StartCountAt(ReactionTable.PairOf(effect.Reaction), effect.CountedBlows);
                        int over = 0, fits = give;
                        if (effect.Overflow) fits = DepthMoves.Fits(taker.Afflictions, kind, give, out over);
                        if (fits > 0) CombatEvents.Report(next, kind, fits, taker.Afflictions.Lay(kind, fits, 0f, combat.AfflictionPower, laying, next), laying);
                        ElementLook.Sparks(look, from + Vector3.up * 1.2f, (next.transform.position - from).normalized, 1f);
                        ElementLook.Puff(look, next.transform.position + Vector3.up * 1.2f, Vector3.up, 0.6f);
                        Passed++;
                        if (over <= 0) break;
                        int depth = taker.Afflictions.Stacks(kind);
                        float burst = taker.Afflictions.SetOff(kind) * Stats.Multiplier(PlayerStat.BurstDamage);
                        from = next.transform.position;
                        combat.StrikeArea(from, 1.5f * Stats.Multiplier(PlayerStat.BurstReach), Blow(look, kind, burst, 4f * depth * Stats.Multiplier(PlayerStat.BurstStagger), 0));
                        if (!bespoke) ElementLook.Strike(look, from, 0.3f);
                        Overflows++;
                        Trigger(UpgradeTrigger.SetOff, next, from, true, depth, null, kind, "", laying, burst);
                        give = over;
                    }
                    break;
                }
                case UpgradeAction.EvenDepth:
                {
                    if (!(subject is IAfflictable)) break;
                    if (!castStruck.Contains(subject) && castStruck.Count < MostStruck) castStruck.Add(subject);
                    depthScratch.Clear();
                    for (int i = 0; i < castStruck.Count; i++)
                        if (castStruck[i] != null && castStruck[i] is IAfflictable passedBy) depthScratch.Add(passedBy.Afflictions);
                    int sickest = DepthMoves.Sickest(depthScratch, kind, effect.Stacks);
                    depthScratch.Clear();
                    // Each passed so far comes up to it: the last one passed leaves every one of them at the sickest's depth.
                    for (int i = 0; i < castStruck.Count; i++)
                    {
                        var one = castStruck[i];
                        if (one == null || !(one is IAfflictable raised)) continue;
                        int lay = DepthMoves.ToRaise(raised.Afflictions, kind, sickest);
                        if (lay <= 0) continue;
                        CombatEvents.Report(one, kind, lay, raised.Afflictions.Lay(kind, lay, 0f, combat.AfflictionPower, laying, one), laying);
                        DepthsRaised++;
                        if (!bespoke) ElementLook.Puff(look, one.transform.position + Vector3.up * 1.2f, Vector3.up, 0.5f);
                    }
                    break;
                }
                case UpgradeAction.CoatBlade:
                {
                    // The depth it carries now, or a set number laid each blow.
                    int load = effect.SetsDepth ? DepthOn(kind, subject as IAfflictable) : Or(effect.Stacks, 1);
                    if (load <= 0) break;
                    Coat.Load(kind, load, effect.SetsDepth, effect.LightOnly, Or(effect.Count, 3), effect.Seconds > 0f ? Time.time + effect.Seconds : 0f);
                    ElementLook.Sparks(look, self + Vector3.up * 1.1f + transform.forward * 0.5f, transform.forward, 0.6f);
                    break;
                }
                case UpgradeAction.PerfectDodgeNow:
                {
                    var attacker = combat.BlowAboutToLand(Or(effect.Seconds, 0.3f), out float strikeIn);
                    if (attacker != null && defence != null)
                    {
                        defence.CountAsPerfectDodge(attacker.transform.position, strikeIn);
                        MovementDodges++;
                    }
                    break;
                }
                case UpgradeAction.EndCondition:
                    // Ended now, its end answered at once (Cold Snap: the running Sleet Charge strikes).
                    if (subject is IAfflictable ending && ending.Afflictions.Conditions.EndNow(effect.Condition) > 0) ConditionsEnded++;
                    break;
                case UpgradeAction.EchoReaction:
                {
                    // The reaction that answered strikes again on this enemy, at a share, a moment later (Strikes Twice, Spreading
                    // Water); or the one the effect names, where an enemy died (Kettle).
                    int pair = !string.IsNullOrEmpty(effect.Reaction) ? ReactionTable.PairOf(effect.Reaction) : pending.Pair;
                    Vector3 where = subject != null ? subject.transform.position : pending.HasAt ? pending.At : centre;
                    // What the trigger carried (the dead one's depth of Needs) stands in for what its spending would count.
                    if (pair >= 0 && reactionsFound != null && reactionsFound.Echo(subject, where, pair, Or(effect.Amount, 0.5f), effect.Delay, pending.Spent)) Echoes++;
                    break;
                }
                case UpgradeAction.Gather:
                {
                    // Every dose near runs into the next burst of this cast that carries doses on, with no limit.
                    gatherTaken.Clear();
                    float within = Or(effect.Radius, 10f);
                    for (int guard = 0; guard < 24; guard++)
                    {
                        var near = combat.EnemyNear(self, within, gatherTaken);
                        if (!(near is IDamageable one)) break;
                        gatherTaken.Add(one);
                        if (!(near is IAfflictable giving)) continue;
                        int doses = giving.Afflictions.Consume(kind);
                        if (doses <= 0) continue;
                        lineCarried += doses;
                        DosesGathered += doses;
                        Vector3 bead = near.transform.position;
                        if (!bespoke) ElementLook.Sparks(look, bead + Vector3.up * 0.3f, (centre - bead).normalized, 0.6f);
                    }
                    gatherTaken.Clear();
                    break;
                }
                case UpgradeAction.ReturnToCast:
                {
                    // Back in a straight line to where the spell was cast, cutting the line again when it says so.
                    Vector3 home = castFrom;
                    home.y = self.y;
                    Vector3 way = home - self;
                    float length = way.magnitude;
                    if (length < 0.3f) break;
                    way /= length;
                    if (eDamage > 0f || eStagger > 0f)
                        combat.StrikeLane(self, way, length, Or(effect.Radius, 0.6f), Blow(look, kind, eDamage, eStagger, effect.Affliction != AfflictionKind.None ? eStacks : 0));
                    if (afterimage != null) afterimage.Leave(0.35f);
                    ElementLook.Sparks(held, self + Vector3.up, way, 1f);
                    player.StepTo(home);
                    StepsBack++;
                    break;
                }
                case UpgradeAction.FreeSurge:
                    if (element != null && element.LightSurgeFree(effect.SurgeOf, Or(effect.Seconds, 4f))) SurgesGiven++;
                    break;
                case UpgradeAction.NextHeavy:
                    if (effect.OnTarget && subject == null) break;
                    combat.MakeNextBlowHeavy(Or(effect.Seconds, 1f), effect.OnTarget ? subject : null);
                    ElementLook.Sparks(held, self + Vector3.up * 1.2f, transform.forward, 0.5f);
                    break;
                case UpgradeAction.ShareStore:
                {
                    // Clouds and trails near are drunk, and half of each enemy's doses there; then the store is split evenly
                    // among those this cast struck, rounded up.
                    float within = Or(effect.Radius, 6f);
                    int store = Hazards.Drink(kind, self, within);
                    gatherTaken.Clear();
                    for (int guard = 0; guard < 24; guard++)
                    {
                        var near = combat.EnemyNear(self, within, gatherTaken);
                        if (!(near is IDamageable one)) break;
                        gatherTaken.Add(one);
                        if (!(near is IAfflictable giving)) continue;
                        int half = DepthMoves.Half(giving.Afflictions.Stacks(kind));
                        if (half <= 0) continue;
                        giving.Afflictions.Spend(kind, half);
                        store += half;
                        Vector3 bead = near.transform.position;
                        if (!bespoke) ElementLook.Sparks(look, bead + Vector3.up * 0.6f, (self - bead).normalized, 0.6f);
                    }
                    gatherTaken.Clear();
                    int hit = pending.CastTag != 0 && pending.CastTag == castHitsOf ? castHits.Count : 0;
                    int each = DepthMoves.ShareEach(store, hit);
                    if (each <= 0) break;
                    for (int i = 0; i < hit; i++)
                    {
                        var struck = castHits[i];
                        if (struck == null || !(struck is IAfflictable taking) || (struck is Enemy gone && gone.IsDead)) continue;
                        CombatEvents.Report(struck, kind, each, taking.Afflictions.Lay(kind, each, 0f, combat.AfflictionPower, laying, struck), laying);
                        DosesStored += each;
                    }
                    break;
                }
                case UpgradeAction.Bite:
                {
                    int bitten = effect.Radius > 0f ? combat.BiteArea(centre, effect.Radius) : subject is IAfflictable sick && sick.Afflictions.BiteSoon() ? 1 : 0;
                    if (bitten > 0)
                    {
                        if (eAmount > 0f && defence != null) defence.Heal(eAmount * bitten);
                        ElementLook.Ring(lookOf[(int)AfflictionKind.Poison] ?? look, centre + Vector3.up * 0.2f);
                    }
                    break;
                }
                case UpgradeAction.SetOff:
                {
                    if (!(subject is IAfflictable carrier)) break;
                    // A burst in a line bursts on what the one before carried to it, even with none of its own (One by One).
                    bool carries = effect.PassesOn && lineCarried > 0;
                    if (!carrier.Afflictions.Has(kind) && !carries) break;
                    int depth = carrier.Afflictions.Stacks(kind);
                    int carriedIn = effect.PassesOn ? lineCarried : 0;
                    if (effect.PassesOn) depth = DepthMoves.LineBurst(depth, ref lineCarried);
                    // A seed set off ripe bursts harder, unripe softer (In Its Season); read before it is spent.
                    bool ripeSeed = carrier.Afflictions.IsRipe(kind);
                    float ripeShare = carrier.Afflictions.RipeShare(kind);
                    float carriedDamage = carriedIn * Afflictions.Spec(kind).SetOffPerStack * combat.AfflictionPower;
                    float damage = ((carrier.Afflictions.SetOff(kind) + carriedDamage) * reactionScale + eDamage * depth) * Stats.Multiplier(PlayerStat.BurstDamage) * ripeShare
                        * castBoons.Multiplier(pending.CastTag, PlayerStat.BurstDamage, Time.time);
                    if (ripeSeed) RipeBurstAt(subject, centre, depth);
                    var discharge = Blow(look, kind, damage, eStagger * depth * Stats.Multiplier(PlayerStat.BurstStagger), 0);
                    // Through every patch of a named ground in full (Grounded: all standing in live water), or round its enemy.
                    int through = string.IsNullOrEmpty(effect.ThroughGround) ? 0 : Hazards.AllNamed(effect.ThroughGround, groundBurstAt, groundBurstRadius);
                    if (!string.IsNullOrEmpty(effect.ThroughGround))
                    {
                        groundStruck.Clear();
                        for (int i = 0; i < through; i++) combat.StrikeArea(groundBurstAt[i], groundBurstRadius[i], discharge, groundStruck, 0f, effect.OnlyCarrying);
                        if (through == 0) combat.StrikeArea(centre, 0.5f, discharge, groundStruck, 0f, effect.OnlyCarrying);
                        groundStruck.Clear();
                        GroundDischarges += through;
                    }
                    else combat.StrikeArea(centre, Or(effect.Radius, 3f) * Stats.Multiplier(PlayerStat.BurstReach), discharge, null, 0f, effect.OnlyCarrying);
                    ElementLook.Strike(look, centre, 0.3f);
                    // Whatever waits on an affliction being spent is told, with how deep it was.
                    Trigger(UpgradeTrigger.SetOff, subject, centre, true, depth, null, kind, "", laying, damage);
                    break;
                }
                case UpgradeAction.Hold:
                {
                    // PerStack: held for each stack counted (Poured Bronze's 0.4 s a notch).
                    float seconds = Or(effect.Seconds, 1.5f) * per;
                    if (effect.Radius > 0f) Holds += combat.HoldArea(centre, effect.Radius * spread, seconds, effect.OnlyCarrying);
                    else if (subject is IAfflictable gripped && gripped.Afflictions.Hold(seconds, effect.Exact)) Holds++;
                    // An unseen hold is drawn by its reaction's look alone: no ring (Seized, the owner's "remove the circles").
                    if (!effect.Unseen)
                    {
                        ElementLook.Ring(look, centre + Vector3.up * 0.2f);
                        ElementLook.Puff(look, centre + Vector3.up * 0.4f, Vector3.up, 0.8f);
                    }
                    break;
                }
                case UpgradeAction.RaiseHeat:
                    if (hasHeat)
                    {
                        Heat.Raise(Or(effect.Count, 1));
                        Vfx.Play(VfxKind.Embers, self + Vector3.up * 1.2f, Vector3.up, 1f);
                    }
                    break;
                case UpgradeAction.TurnTide:
                    if (hasTideClock) TurnTide();
                    break;
                case UpgradeAction.ClearCount:
                {
                    var owner = effect.Owner;
                    if (owner != null && owner.Tally != null && owner.Index < tally.Length && tally[owner.Index] != 0)
                    {
                        tally[owner.Index] = 0;
                        tallyDirty = true;
                    }
                    break;
                }
                case UpgradeAction.StepBack:
                {
                    Vector3 back = target != null ? -aim : -transform.forward;
                    float far = Or(effect.Reach, 3f);
                    if (afterimage != null) afterimage.Leave(0.3f);
                    ElementLook.Sparks(held, self + Vector3.up, back, 1f);
                    player.StepTo(self + back * far);
                    break;
                }
                case UpgradeAction.Untouchable:
                    if (defence != null) defence.UntouchableUntil = Mathf.Max(defence.UntouchableUntil, Time.time + Or(effect.Seconds, 0.3f));
                    break;
                case UpgradeAction.SwapPlaces:
                {
                    if (target == null || !(subject is IAfflictable placed)) break;
                    Vector3 theirs = target.position;
                    // Where the player stood as the spell was cast; with no cast known, where they stand now.
                    Vector3 mine = castOrigin.sqrMagnitude > 0.0001f ? castOrigin : self;
                    mine.y = theirs.y;
                    ElementLook.Puff(held, theirs + Vector3.up * 0.4f, Vector3.up, 1.2f);
                    ElementLook.Puff(held, mine + Vector3.up * 0.4f, Vector3.up, 1.2f);
                    if (placed.Afflictions.Rank == EnemyRank.Plain)
                    {
                        // It rises out of your water, you out of its.
                        PlaceBody(subject, mine);
                        player.PassTo(theirs);
                        PlacesSwapped++;
                    }
                    else
                    {
                        // What is too great to be moved stays: you come up behind it instead.
                        Vector3 past = theirs - mine;
                        past.y = 0f;
                        player.PassTo(theirs + (past.sqrMagnitude > 0.01f ? past.normalized : transform.forward) * 1.5f);
                    }
                    // Face what you changed places with.
                    Vector3 face = subject.transform.position - transform.position;
                    face.y = 0f;
                    if (face.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(face, Vector3.up);
                    break;
                }
                case UpgradeAction.LengthenSurge:
                    if (element.SurgeActive) element.SurgeKit.Lengthen(effect.Amount, effect.Seconds);
                    break;
                case UpgradeAction.Mend:
                    mending.Add(pending.Power * Or(effect.Share, 0.1f));
                    break;
                case UpgradeAction.Freeze:
                    if (subject is IAfflictable freezing && freezing.Afflictions.FreezeFor(Or(effect.Seconds, 1.5f), effect.Amount > 1f ? effect.Amount : 1f)) FreezesKept++;
                    break;
                case UpgradeAction.KeepFrozen:
                    if (subject is IAfflictable frozen && frozen.Afflictions.KeepFrozen(effect.Seconds)) FreezesKept++;
                    break;
                case UpgradeAction.Lift:
                {
                    float seconds = Or(effect.Seconds, 1f) + Stats.Sum(PlayerStat.AirSeconds);
                    if (effect.Radius > 0f) Lifts += combat.LiftArea(centre, effect.Radius * spread, Or(effect.Seconds, 1f));
                    else if (subject is IAfflictable thrown)
                    {
                        if (thrown.Afflictions.Lift(seconds)) Lifts++;
                        // What is too great to leave the ground reels where it stands.
                        else combat.StrikeArea(centre, 0.8f, Blow(look, kind, 0f, Or(eStagger, 40f), 0));
                    }
                    else break;
                    ElementLook.Ring(look, centre + Vector3.up * 0.2f);
                    ElementLook.Puff(look, centre + Vector3.up * 0.6f, Vector3.up, 1.2f);
                    Trigger(UpgradeTrigger.Lifted, subject, centre, true, 0, null, AfflictionKind.None, "", laying);
                    break;
                }
                case UpgradeAction.Drop:
                    if (subject is IAfflictable driven) driven.Afflictions.Drop();
                    break;
                case UpgradeAction.BurstGrown:
                {
                    // Only what stands within the effect's reach of you when it names one (Drawn From the Wood: 12 m).
                    int burst = Hazards.BurstGrown(Blow(look, kind, eDamage, eStagger, eStacks), Or(effect.Radius, 3f), self, effect.Reach, grownBursts);
                    // Counted as a Spend counts, for the effects after it in the mark (Drawn From the Wood's green blood).
                    lastSpent = burst;
                    spentBy = effect.Owner;
                    spentOn = pending.Target;
                    if (burst > 0 && defence != null && eAmount > 0f) defence.Heal(eAmount * burst);
                    // Each burst is a moment of its own, where it was (Second Sowing sows in all it reaches).
                    for (int b = 0; b < burst && b < grownBursts.Length; b++) Trigger(UpgradeTrigger.GrownBurst, null, grownBursts[b]);
                    break;
                }
                case UpgradeAction.Bank:
                {
                    var owner = effect.Owner;
                    if (owner == null || owner.Tally == null || owner.Index >= tally.Length) break;
                    int now = Mathf.Min(owner.Tally.Cap, tally[owner.Index] + Or(eCount, 1));
                    // Banked for a time: the count stands that long from this banking, then empties.
                    if (effect.Seconds > 0f && owner.Index < tallyEnds.Length) tallyEnds[owner.Index] = Time.time + effect.Seconds;
                    if (now == tally[owner.Index]) break;
                    tally[owner.Index] = now;
                    tallyDirty = true;
                    ElementLook.Sparks(held, self + Vector3.up * 1.4f, Vector3.up, 0.5f);
                    break;
                }
                case UpgradeAction.FlaskFill:
                    if (defence != null && defence.Flasks.Fill(Or(eAmount, 0.25f))) defence.Callout("Flask earned");
                    break;
                case UpgradeAction.SpendHealth:
                    if (defence != null) defence.Health.Chip(eAmount * Stats.Multiplier(PlayerStat.HealthPrices));
                    break;
                case UpgradeAction.Leech:
                    if (defence != null) defence.Heal(combat.LastBlowDamage * Or(eAmount, 0.1f), false);
                    break;
                case UpgradeAction.KeepForGood:
                    if (subject is IAfflictable keeper && kind != AfflictionKind.None) keeper.Afflictions.KeepForGood(kind);
                    break;
                case UpgradeAction.Spend:
                {
                    lastSpent = 0;
                    spentBy = effect.Owner;
                    spentOn = pending.Target;
                    if (!(subject is IAfflictable spender)) break;
                    int carried = spender.Afflictions.Stacks(kind);
                    int take = effect.Share > 0f ? Mathf.CeilToInt(carried * effect.Share - 0.0001f) : carried;
                    if (effect.Count > 0) take = Mathf.Min(take, effect.Count);
                    lastSpent = effect.CountOnly ? Mathf.Min(take, carried) : spender.Afflictions.Spend(kind, take);
                    break;
                }
                case UpgradeAction.ForceReaction:
                {
                    int pair = ReactionTable.PairOf(effect.Reaction);
                    if (effect.CountedBlows > 0 && subject is IAfflictable coiled) coiled.Afflictions.StartCountAt(pair, effect.CountedBlows);
                    if (subject is IAfflictable forced && (effect.AsLaid ? forced.Afflictions.ReactAsLaid(pair, subject) : forced.Afflictions.ForceReaction(pair, subject, effect.Alone)))
                        ForcedReactions++;
                    break;
                }
                case UpgradeAction.Flinch:
                    if (subject is IFlinchable flinching && flinching.Flinch(Or(effect.Seconds, 0.4f))) Flinches++;
                    break;
                case UpgradeAction.Condition when effect.AlongCast && effect.Radius > 0f:
                {
                    // Everyone passed on the way from where the spell was cast, once each, but its own enemy (Stone Underfoot).
                    Vector3 path = self - castFrom;
                    path.y = 0f;
                    int steps = DodgePathSteps(path.magnitude, effect.Radius);
                    fanTaken.Clear();
                    if (subject is IDamageable passedTo) fanTaken.Add(passedTo);
                    for (int i = 0; i <= steps; i++)
                    {
                        Vector3 point = castFrom + path * (i / (float)steps);
                        for (int guard = 0; guard < 12; guard++)
                        {
                            var near = combat.EnemyNear(point, effect.Radius * spread, fanTaken);
                            if (!(near is IDamageable passed)) break;
                            fanTaken.Add(passed);
                            if (ConditionCaster.CastOne(effect, near, combat, laying, per)) ConditionsSet++;
                        }
                    }
                    fanTaken.Clear();
                    break;
                }
                case UpgradeAction.Condition:
                    // It keeps the trigger's number (ByPower), or the depth counted as the moment came (CountsNeeds), or its place
                    // in the cast's line (Numbered).
                    ConditionsSet += ConditionCaster.Cast(effect, subject, centre, combat, laying, conditionScratch, per,
                        effect.State != null && effect.State.Numbered ? NextNumber(pending.CastTag) : effect.ByPower ? pending.Power : effect.CountsNeeds ? per : -1f,
                        spread, pending.From, pending.ReactionOf - 1);
                    break;
                case UpgradeAction.Shatter:
                {
                    // The ice breaks as under a heavy blow: what it held takes the breaking, and the shatter is told.
                    if (!(subject is IAfflictable icy)) break;
                    float broke = ShatterNow(icy);
                    if (broke <= 0f) break;
                    ShattersMade++;
                    CombatEvents.RaiseShattered(subject);
                    var chill = lookOf[(int)AfflictionKind.Chill] ?? look;
                    combat.StrikeOne(subject, Blow(chill, AfflictionKind.None, broke + eDamage, eStagger, 0), self);
                    if (!bespoke) ElementLook.Strike(chill, centre, 0.3f);
                    break;
                }
                case UpgradeAction.Cleanse:
                    if (defence != null) defence.Afflictions.Clear();
                    break;
                case UpgradeAction.StrikeCondition:
                    if (subject is IAfflictable split) split.Afflictions.Conditions.StrikeWhere(effect.Condition, true);
                    break;
                case UpgradeAction.CutCooldowns:
                case UpgradeAction.ResetCooldown:
                case UpgradeAction.InstantNext:
                case UpgradeAction.KeepWaitShare:
                    // The spell clocks are the slots' (Magic/BranchCooldowns): a Master's reward for its timing.
                    if (spells != null) spells.Answer(effect);
                    break;
            }
        }

        /// <summary>Counts for probes: reactions forced, flinches given, conditions set and arcs that leapt from iron to iron.</summary>
        public int ForcedReactions { get; private set; }
        public int Flinches { get; private set; }
        public int ConditionsSet { get; private set; }
        public int ArcsLeapt { get; private set; }
        /// <summary>Counts for probes: places swapped (Under the Black Water) and later presses done (Three Weasels).</summary>
        public int PlacesSwapped { get; private set; }
        public int PressesDone { get; private set; }
        // Where the player stood as the last spell was cast, for what is done where they were (a swap of places).
        private Vector3 castOrigin;

        // Puts an enemy's body at a spot at once: its controller is set aside for the move, as its own blinks do.
        private static void PlaceBody(Component body, Vector3 to)
        {
            var controller = body.GetComponent<CharacterController>();
            bool was = controller != null && controller.enabled;
            if (was) controller.enabled = false;
            body.transform.position = to;
            if (was) controller.enabled = true;
        }

        /// <summary>
        /// A later press of a spell of several presses (<see cref="Upgrade.Presses"/>): its effects are done now, aimed at the
        /// enemy faced, as the spell's own. False when there is no such press.
        /// </summary>
        public bool Press(Upgrade mark, int index)
        {
            var presses = PressesOf(mark, out _);
            if (presses == null || index < 0 || index >= presses.Length || presses[index] == null) return false;
            castSource = mark;
            castOrigin = transform.position;
            int before = queued;
            foreach (var effect in presses[index])
            {
                // Nothing gathered to loose, or no ebb to flood: the press is no press, and the slot casts again.
                if (effect.Do == UpgradeAction.Loose && !rings.Active) continue;
                if (effect.Do == UpgradeAction.Flood && !Ebbing) continue;
                Consider(effect, null, transform.position, false);
            }
            if (queued == before)
            {
                castSource = null;
                return false;
            }
            PressesDone++;
            // The press is a moment of its own branch's (Second Shadow's Master answers its change of places).
            Trigger(UpgradeTrigger.SpellPressed, null, transform.position, false);
            castSource = null;
            return true;
        }

        // Grey won back by each weapon blow beside what a mark grew, until when (Corpse Flowers).
        private float regainUntil = -100f, regainAmount, regainNear;
        private int regainFamily;
        /// <summary>Grey health won back by blows beside what was grown (Corpse Flowers), for probes.</summary>
        public float GreyWonBeside { get; private set; }

        // The thing a spell left standing (Lightning Rod's rod): whose, where, its ground's group, and until when.
        private Upgrade standingMark;
        private Vector3 standingAt;
        private int standingGroup;
        private float standingUntil;

        /// <summary>A spell of <paramref name="mark"/>'s left a thing standing at <paramref name="at"/> for <paramref name="seconds"/>, its ground laid as <paramref name="group"/>.</summary>
        public void NoteStanding(Upgrade mark, Vector3 at, int group, float seconds)
        {
            standingMark = mark;
            standingAt = at;
            standingGroup = group;
            standingUntil = Time.time + seconds;
        }

        /// <summary>True while a thing of <paramref name="branch"/>'s spell still stands.</summary>
        public bool StandingOf(string branch) => standingMark != null && Time.time < standingUntil && standingMark.Branch == branch;

        /// <summary>True when an effect in force of <paramref name="branch"/> keeps its standing thing past the step to it (UpgradeAction.KeepStanding).</summary>
        public static bool KeepsStanding(List<UpgradeEffect> inForce, string branch)
        {
            if (inForce == null) return false;
            for (int i = 0; i < inForce.Count; i++)
                if (inForce[i].Do == UpgradeAction.KeepStanding && inForce[i].Owner != null && inForce[i].Owner.Branch == branch) return true;
            return false;
        }

        /// <summary>Counts for probes: steps taken to a thing a spell left standing.</summary>
        public int StepsToStanding { get; private set; }

        // The player's health as a spell left its copy standing (Return to the Shadow gives back what was lost since).
        private float shadowHealth;

        /// <summary>The wounds taken since a moment: what health was then less what it is now, never below nothing.</summary>
        public static float WoundsSince(float then, float now) => Mathf.Max(0f, then - now);

        /// <summary>
        /// The later presses of a mark's spell as it is cast now: the worn shape's, when it adds any (ShapeDef.Presses), else the
        /// mark's own; null for none. <paramref name="window"/> is how long after the cast they may come.
        /// </summary>
        public UpgradeEffect[][] PressesOf(Upgrade mark, out float window)
        {
            window = 0f;
            if (mark == null) return null;
            string branch = !string.IsNullOrEmpty(mark.Branch) ? mark.Branch : mark.Id;
            var worn = SpellBranches.WornShape(branch, RunState.Unlocked, ReadLoadout());
            string masterId = SpellBranch.MasterId(branch);
            return PressesOf(mark, worn, Has(masterId) ? Upgrade.ById(masterId) : null, out window);
        }

        /// <summary>
        /// The rules a Flung Blade thrown now is planted and driven by: its Empower (16, three in one enemy, a heavy blow driving
        /// all), its shape (Cherry Red, The Nail) and All Blades Home (never fading). Plain for any other spell's blade.
        /// </summary>
        public BladeOrders BladeOrdersFor(in SpellCast cast)
        {
            var orders = BladeOrders.Plain;
            string branch = Trees.MetalTree.FlungBlade;
            bool flung = cast.Source == null ? cast.Element != null && cast.Element.Name == Elements.Metal : cast.Source.Id == branch || cast.Source.Branch == branch;
            if (!flung) return orders;
            orders = OrdersOf(SpellBranches.WornShape(branch, RunState.Unlocked, ReadLoadout()), Has(SpellBranch.EmpowerId(branch)), Has(Trees.MetalTree.AllBladesHome));
            return orders;
        }

        /// <summary>A Flung Blade's orders, with its shape worn, its Empower and All Blades Home given.</summary>
        public static BladeOrders OrdersOf(ShapeDef worn, bool empowered, bool neverFades)
        {
            var orders = BladeOrders.Plain;
            if (empowered)
            {
                orders.Drive = BladeRules.EmpoweredDrive;
                orders.MostInOne = BladeRules.EmpoweredMostInOne;
                orders.HeavyDrivesAll = true;
            }
            if (worn != null && worn.Name == Trees.MetalTree.CherryRedName) orders.Kind = PlantedKind.CherryRed;
            else if (worn != null && worn.Name == Trees.MetalTree.NailName) orders.Kind = PlantedKind.Nail;
            orders.NeverFades = neverFades;
            return orders;
        }

        /// <summary>A blade of yours was driven into <paramref name="host"/> (null: in the ground) for the <paramref name="drives"/>th time.</summary>
        public void OnBladeDriven(Component host, int drives)
        {
            BladesDriven++;
            if (host != null) Trigger(UpgradeTrigger.BladeDriven, host, host.transform.position, true, drives);
        }

        // ---- Standing things the marks raise (World.StandingThings): their clock, and blows on them told to the marks that raised them.
        private int standingOwner = -1;
        private bool standingHeavy;

        /// <summary>Standing things raised by marks, and blows on them answered: for probes.</summary>
        public int ThingsRaised { get; private set; }
        public int StandingAnswers { get; private set; }

        private void ReadStandingNews()
        {
            StandingLibrary.Run(Time.time, Time.frameCount);
            if (!burstHooked)
            {
                burstHooked = true;
                StandingLibrary.Burst += OnStandingBurst;
                Hazards.GroundActs += OnGroundActs;
                Hazards.GroundEnds += OnGroundEnds;
                OwnGrounds.Stopped += OnStoppedAtStones;
            }
            while (StandingThings.TakeNews(out var item))
            {
                if (item.Thing.Owner < 0 || !item.ByPlayer) continue;
                standingOwner = item.Thing.Owner;
                standingHeavy = item.Heavy;
                try { Trigger(UpgradeTrigger.StandingStruck, null, item.Thing.Position, true, item.Struck == StandingStruck.Broken ? 1 : 0); }
                finally { standingOwner = -1; }
            }
        }

        private bool burstHooked;

        // ---- Pickups the marks drop (World.Pickups): taken by walking over them, shown as a glowing sign until then.
        private PickupKind pickupKind;
        private readonly int[] pickupSigns = new int[Pickups.Most];
        private readonly bool[] pickupShown = new bool[Pickups.Most];

        /// <summary>Creature looks played by marks (Forms.CreatureLooks): for probes.</summary>
        public int CreatureLooksPlayed { get; private set; }

        /// <summary>Pickups dropped, taken and left: for probes.</summary>
        public int PickupsDropped { get; private set; }
        public int PickupsTaken { get; private set; }
        public int PickupsLeft { get; private set; }

        /// <summary>
        /// The effects that answer a pickup's taking or its being left: those on <paramref name="moment"/> written beside the
        /// effect that dropped it (in its mark's effects, or its shape's), so a reaction's herb is answered by that reaction.
        /// </summary>
        public static int AnswersOf(UpgradeEffect dropped, UpgradeTrigger moment, List<UpgradeEffect> into)
        {
            into.Clear();
            var mark = dropped != null ? dropped.Owner : null;
            if (mark == null) return 0;
            var beside = mark.Effects != null && System.Array.IndexOf(mark.Effects, dropped) >= 0 ? mark.Effects : null;
            if (beside == null && mark.Shapes != null)
                foreach (var shape in mark.Shapes)
                    if (shape.Effects != null && System.Array.IndexOf(shape.Effects, dropped) >= 0) beside = shape.Effects;
            if (beside == null) return 0;
            foreach (var effect in beside)
                if (effect.On == moment) into.Add(effect);
            return into.Count;
        }

        private readonly List<UpgradeEffect> pickupAnswers = new List<UpgradeEffect>();

        // A pickup's moment: its own effects, done as its mark's (a reaction's lay nothing that starts another), then the marks
        // that answer any of its kind.
        private void AnswerPickup(in Pickup item, UpgradeTrigger moment, int drawn)
        {
            pickupKind = item.Kind;
            var dropped = item.Source as UpgradeEffect;
            var source = dropped != null && dropped.Owner != null && (dropped.Owner.Kind == UpgradeKind.Reaction || ReactionTable.IsReactionShape(dropped.Owner)) ? LaySource.Reaction : LaySource.Effect;
            AnswersOf(dropped, moment, pickupAnswers);
            for (int i = 0; i < pickupAnswers.Count; i++) Fire(pickupAnswers[i], null, item.Position, source, 0f, drawn);
            pickupAnswers.Clear();
            Trigger(moment, null, item.Position, true, drawn);
        }

        private void ReadPickups()
        {
            Vector3 feet = transform.position;
            Pickups.StepDrawn(feet, Time.deltaTime);
            if (defence == null || !defence.IsDead)
                while (Pickups.TakeAt(feet, out var taken))
                {
                    PickupsTaken++;
                    ElementLook.Sparks(PickupLook(taken.Kind), taken.Position + Vector3.up * 0.4f, Vector3.up, 1f);
                    AnswerPickup(taken, UpgradeTrigger.PickedUp, taken.Drawn ? 1 : 0);
                }
            while (Pickups.TakeLeft(Time.time, out var left))
            {
                PickupsLeft++;
                AnswerPickup(left, UpgradeTrigger.PickupLeft, 0);
            }
            // Each lying pickup is a small sign of its element on the floor, bobbing, until it is taken or gone.
            for (int i = 0; i < Pickups.Most; i++)
            {
                var item = Pickups.Get(i);
                if (item.Active && !pickupShown[i])
                {
                    var look = PickupLook(item.Kind);
                    pickupSigns[i] = Forms.FormFx.HoldMark(Forms.FormMark.Sigil, Forms.FormKit.Lit(look != null ? look.Colour : Color.white, 2.4f));
                    pickupShown[i] = true;
                }
                else if (!item.Active && pickupShown[i])
                {
                    Forms.FormFx.ReleaseMark(pickupSigns[i], 0.15f);
                    pickupShown[i] = false;
                }
                if (item.Active) Forms.FormFx.PlaceMarkFacing(pickupSigns[i], item.Position + Vector3.up * (0.35f + 0.08f * Mathf.Sin(Time.time * 4f + i)), 0.7f, Time.time * 60f);
            }
        }

        // The element a pickup is seen in: the herb in light, the lily's fire in flame, the seed in ice.
        private ElementInfo PickupLook(PickupKind kind) =>
            lookOf[(int)(kind == PickupKind.Herb ? AfflictionKind.Marked : kind == PickupKind.LilyFire ? AfflictionKind.Burn : AfflictionKind.Chill)];

        private void OnStandingBurst(Standing thing)
        {
            if (thing.Owner < 0) return;
            standingOwner = thing.Owner;
            standingHeavy = false;
            try { Trigger(UpgradeTrigger.StandingBurst, null, thing.Position, true); }
            finally { standingOwner = -1; }
        }

        private void OnDestroy()
        {
            if (!burstHooked) return;
            StandingLibrary.Burst -= OnStandingBurst;
            Hazards.GroundActs -= OnGroundActs;
            Hazards.GroundEnds -= OnGroundEnds;
            OwnGrounds.Stopped -= OnStoppedAtStones;
        }

        // An enemy driven against the stones of your ring (The Closed Ring), told to the marks with it as the target.
        private void OnStoppedAtStones(Component body)
        {
            if (body == null) return;
            StoppedAtStones++;
            Trigger(UpgradeTrigger.StoppedAtStones, body, body.transform.position);
        }

        /// <summary>True when the cast now answered (<paramref name="cast"/>) is the one counted and has struck at least <paramref name="most"/>.</summary>
        public static bool CastStruckAtLeast(int cast, int counted, int struck, int most) => cast != 0 && cast == counted && struck >= most;

        /// <summary>A counter's stance ran out with nothing struck at it: told as that counter's own moment (Blown Through's push).</summary>
        public void OnCounterMissed(Upgrade source)
        {
            var was = castSource;
            castSource = source;
            try { Trigger(UpgradeTrigger.CounterMissed, null, transform.position, false); }
            finally { castSource = was; }
            CountersMissed++;
        }

        /// <summary>Counters that ran out with nothing struck at them: for probes.</summary>
        public int CountersMissed { get; private set; }

        /// <summary>How many steps a dodge's path of this length is searched in, for circles of this radius to cover it.</summary>
        public static int DodgePathSteps(float length, float radius) => Mathf.Clamp(Mathf.CeilToInt(length / Mathf.Max(0.2f, radius)), 1, 12);

        /// <summary>
        /// A lit coat's burst: <paramref name="first"/> and <paramref name="each"/> more for each blow that spread it, at most
        /// <paramref name="most"/> (0: no most). Black Pitch: 16, 8 a blow, 70.
        /// </summary>
        public static float CoatBurst(float first, float each, int spreads, float most)
        {
            float burst = first + each * Mathf.Max(0, spreads);
            return most > 0f ? Mathf.Min(burst, most) : burst;
        }

        /// <summary>Coated enemies that went up (UpgradeAction.LightCoat), and grounds whose standers were dosed at once (Low Mist): for probes.</summary>
        public int CoatsLit { get; private set; }
        public int GroundsDosed { get; private set; }
        /// <summary>Patches of ground carried on by a spell (UpgradeAction.CarryGround): for probes.</summary>
        public int GroundsCarried { get; private set; }
        private readonly HashSet<IDamageable> coatStruck = new HashSet<IDamageable>();

        /// <summary>Enemies driven against your ring's stones and stopped there (The Closed Ring): for probes.</summary>
        public int StoppedAtStones { get; private set; }

        /// <summary>
        /// The states of standing in ground of your own, each with the name of its ground (World.OwnGrounds): drawn by a form or
        /// laid by a mark as a named patch (Hazards.NamedUnder).
        /// </summary>
        public static readonly (StatWhen When, string Ground)[] OwnGroundStates =
        {
            (StatWhen.OnOwnIce, OwnGrounds.Sheet),
            (StatWhen.OnWetSand, OwnGrounds.WetSand),
            (StatWhen.InOwnMist, OwnGrounds.LowMist),
            (StatWhen.InOwnGlow, OwnGrounds.Glow),
            (StatWhen.InOwnBlood, OwnGrounds.Blood),
            (StatWhen.InCloud, OwnGrounds.Cloud),
        };

        /// <summary>True when you stand in ground of your own of this name now.</summary>
        public bool InOwnGround(string ground) =>
            OwnGrounds.Under(ground, transform.position, Time.time) || Hazards.NamedUnder(transform.position, ground);

        // Ground of a mark's that keeps acting, at one of its bites: its state on every enemy standing in it now, and the player
        // mended standing in it.
        private void OnGroundActs(object source, Vector3 at, float radius, float biteEvery, object laidFor)
        {
            if (!(source is UpgradeEffect effect) || combat == null) return;
            if (effect.InsideDo != null || !string.IsNullOrEmpty(effect.Ground)) ActOnStanding(effect, effect.InsideDo, effect.Ground, at, radius, laidFor);
            if (effect.Inside != null)
            {
                if (!groundStates.TryGetValue(effect, out var inside))
                {
                    inside = effect.Twin();
                    inside.Do = UpgradeAction.Condition;
                    inside.Count = 0;
                    inside.State = effect.Inside.Copy();
                    groundStates[effect] = inside;
                }
                inside.Radius = radius;
                inside.State.Seconds = Mathf.Max(effect.Inside.Seconds, GroundRules.StateSeconds(biteEvery));
                GroundStatesSet += ConditionCaster.Cast(inside, null, at, combat, LaySource.Effect, conditionScratch);
            }
            // What the ground does for you while you stand in it (The Wreck), as its mark's own.
            if (effect.YouInside != null && defence != null && !defence.IsDead && GroundRules.Within(transform.position, at, radius))
            {
                foreach (var inner in effect.YouInside)
                    if (inner != null) Fire(inner, null, transform.position, GroundSource(effect));
                GroundStoodByYou++;
            }
            if (effect.Mends > 0f && defence != null && !defence.IsDead && GroundRules.Within(transform.position, at, radius))
            {
                defence.Heal(GroundRules.MendStep(effect.Mends, biteEvery), false);
                GroundMended++;
            }
        }

        private readonly Dictionary<UpgradeEffect, UpgradeEffect> groundStates = new Dictionary<UpgradeEffect, UpgradeEffect>();
        private readonly HashSet<IDamageable> standingTaken = new HashSet<IDamageable>();
        // Where each one standing in ground that bites only the moving was at its last bite.
        private readonly Dictionary<(UpgradeEffect, Component), Vector3> standingSeen = new Dictionary<(UpgradeEffect, Component), Vector3>();
        // The ground whose bite is told as InGround now, for the trigger's filter.
        private string groundTold = "";

        // Ground of a mark's running out of time: what flares at its end is done on each enemy standing in it.
        private void OnGroundEnds(object source, Vector3 at, float radius, float biteEvery, object laidFor)
        {
            if (!(source is UpgradeEffect effect) || combat == null || effect.InsideAtEnd == null) return;
            ActOnStanding(effect, effect.InsideAtEnd, "", at, radius, laidFor);
            GroundFlares++;
        }

        /// <summary>Bites of ground that did something for you as you stood in it (UpgradeEffect.YouInside): for probes.</summary>
        public int GroundStoodByYou { get; private set; }

        /// <summary>Grounds that flared as they ran out (UpgradeEffect.InsideAtEnd): for probes.</summary>
        public int GroundFlares { get; private set; }

        // What a ground's own effects lay counts as: a reaction's (or its shape's) starts no reaction.
        private static LaySource GroundSource(UpgradeEffect effect)
        {
            var owner = effect.Owner;
            return owner != null && (owner.Kind == UpgradeKind.Reaction || ReactionTable.IsReactionShape(owner)) ? LaySource.Reaction : LaySource.Effect;
        }

        // Each enemy standing in the ground now: the ground's own effects done on it, and the marks that answer the ground told of it.
        private void ActOnStanding(UpgradeEffect effect, UpgradeEffect[] inside, string ground, Vector3 at, float radius, object laidFor)
        {
            var source = GroundSource(effect);
            standingTaken.Clear();
            for (int guard = 0; guard < 24; guard++)
            {
                var near = combat.EnemyNear(at, radius, standingTaken);
                if (!(near is IDamageable one)) break;
                standingTaken.Add(one);
                if (near is Enemy fallen && fallen.IsDead) continue;
                if (!GroundRules.Within(near.transform.position, at, radius)) continue;
                bool moved = true;
                if (effect.InsideMovingOnly)
                {
                    var key = (effect, near);
                    Vector3 now = near.transform.position;
                    moved = standingSeen.TryGetValue(key, out Vector3 before) && GroundRules.Moved(before, now);
                    if (standingSeen.Count > 256) standingSeen.Clear();
                    standingSeen[key] = now;
                }
                if (inside != null && moved && !(effect.InsideSpares && ReferenceEquals(near, laidFor)))
                    foreach (var inner in inside)
                        if (inner != null) Fire(inner, near, near.transform.position, source);
                if (!string.IsNullOrEmpty(ground))
                {
                    groundTold = ground;
                    try { Trigger(UpgradeTrigger.InGround, near, near.transform.position); }
                    finally { groundTold = ""; }
                }
                GroundStood++;
            }
            standingTaken.Clear();
        }

        // The swings landed, counted for blows that take turns (UpgradeEffect.TakesTurns), and the frame the last one landed in.
        private int blowTurn, blowTurnFrame = -1;

        /// <summary>True when a Kill effect answers a death told with <paramref name="reaction"/>'s id: any death, or only one dealt by the reaction it names.</summary>
        public static bool KillAnswers(string killedBy, string reaction) => string.IsNullOrEmpty(killedBy) || killedBy == reaction;

        /// <summary>True when an effect answers ground of the given name told of one standing in it (UpgradeTrigger.InGround).</summary>
        public static bool AnswersGround(UpgradeEffect effect, string ground) =>
            effect != null && !string.IsNullOrEmpty(ground) && effect.Condition == ground;

        // The fight each once-a-fight effect last went off in, by its slot (-1: never).
        private int[] firedInFight = new int[0];

        // Until when you are scattered (Gone to Ash): the body comes back then; 0 when not scattered.
        private float scatteredUntil;

        /// <summary>Times you scattered and stood again (UpgradeAction.Scatter): for probes.</summary>
        public int Scatters { get; private set; }

        // Heavy blows unbroken by the marks in force, and until when by ground stood in (The Wreck).
        private bool heavyUnbrokenByMarks;
        private float heavyUnbrokenUntil = -100f;

        // Each one struck once by a discharge run through several patches of ground.
        private readonly HashSet<IDamageable> groundStruck = new HashSet<IDamageable>();

        /// <summary>Patches of named ground a discharge ran through (UpgradeEffect.ThroughGround): for probes.</summary>
        public int GroundDischarges { get; private set; }

        // Where each patch a BurstGround took lay, and how wide.
        private readonly Vector3[] groundBurstAt = new Vector3[16];
        private readonly float[] groundBurstRadius = new float[16];

        /// <summary>Patches of ground burst all at once by a mark (UpgradeAction.BurstGround): for probes.</summary>
        public int GroundsBurst { get; private set; }

        /// <summary>Enemies carried along a lane by a gust (UpgradeEffect.Carry on a Lane): for probes.</summary>
        public int LanesShoved { get; private set; }

        /// <summary>Enemies found standing in acting ground with effects or answers of its own, counted at each bite: for probes.</summary>
        public int GroundStood { get; private set; }

        /// <summary>States set by ground on those standing in it, and the times it mended you: for probes.</summary>
        public int GroundStatesSet { get; private set; }
        public int GroundMended { get; private set; }

        /// <summary>True when a standing thing's moment is one an effect answers: its own mark raised the thing, and a heavy blow when it asks for one.</summary>
        public static bool AnswersStanding(UpgradeEffect effect, int owner, bool heavy) =>
            effect != null && effect.Owner != null && effect.Owner.Index == owner && (effect.By != StruckBy.Heavy || heavy);

        /// <summary>
        /// True when the enemy at <paramref name="enemy"/> has stone at its back as seen from <paramref name="from"/>: a standing
        /// thing within <paramref name="reach"/> behind it, or the level's own wall or pillar there.
        /// </summary>
        private bool StoneBehind(Vector3 from, Vector3 enemy, float reach)
        {
            for (int i = 0; i < StandingThings.Most; i++)
                if (StandingRules.AtItsBack(StandingThings.Get(i), from, enemy, reach + StandingRules.Reach(StandingThings.Get(i).Kind))) return true;
            Vector3 back = enemy - from;
            back.y = 0f;
            if (back.sqrMagnitude < 0.0001f) return false;
            // The level's own stone: a wall or pillar the ray from its chest backward meets (never an enemy or the player).
            int count = Physics.RaycastNonAlloc(enemy + Vector3.up, back.normalized, backHits, reach + 0.5f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var hit = backHits[i].collider;
                if (hit == null || hit.attachedRigidbody != null || hit.GetComponentInParent<IDamageable>() != null || hit.GetComponentInParent<CharacterController>() != null) continue;
                return true;
            }
            return false;
        }

        private readonly RaycastHit[] backHits = new RaycastHit[8];

        // ---- Two Suns: a figure of light on the enemy's far side casts the spell again back toward you.
        private bool hasTwoSuns;
        private ISpellForm echoForm;
        private SpellCast echoCast;
        private float echoAt;

        /// <summary>Two Suns' figures of light that have cast: for probes.</summary>
        public int SunsEchoed { get; private set; }

        /// <summary>
        /// A spell has just been cast by <paramref name="form"/> at an enemy: with Two Suns, a figure of light stands now on
        /// the enemy's far side and casts the same spell back toward you a moment later, at a share of its damage. A spell
        /// that moves your own body, or is held or gathered, has no figure (TwoSunsRules.Echoes).
        /// </summary>
        public void EchoLater(ISpellForm form, in SpellCast cast)
        {
            if (!hasTwoSuns || form == null || cast.Target == null || !TwoSunsRules.Echoes(form.Id, cast.Spec)) return;
            Vector3 at = TwoSunsRules.FarSide(transform.position, cast.Target.position, out Vector3 facing);
            echoForm = form;
            echoCast = cast;
            echoCast.Origin = at;
            echoCast.Forward = facing;
            echoCast.Aim = transform.position;
            echoCast.Damage *= TwoSunsRules.Share;
            echoCast.Stagger *= TwoSunsRules.Share;
            // The figure's spell is no slot cast: its blows never charge the blade, nor are counted as the cast's own.
            echoCast.Cast = 0;
            echoAt = Time.time + TwoSunsRules.Delay;
            if (afterimage != null) afterimage.LeaveAt(TwoSunsRules.Delay + 0.5f, SunFigure, at, facing);
            ElementLook.Sparks(lookOf[(int)AfflictionKind.Marked] ?? OwnElement(null), at + Vector3.up, facing, 0.8f);
        }

        // Light the colour of the sun at noon, strong enough to glow.
        private static readonly Color SunFigure = new Color(2.2f, 1.8f, 0.9f, 0.85f);

        private void CastEcho()
        {
            var form = echoForm;
            echoForm = null;
            if (form == null || combat == null) return;
            SunsEchoed++;
            form.Cast(echoCast);
        }

        /// <summary>A turning blade of yours was met by a perfect guard as it came home to the hand.</summary>
        public void OnBladeCaught(Vector3 at)
        {
            BladesCaught++;
            Trigger(UpgradeTrigger.BladeCaught, null, at);
        }

        public int BladesCaught { get; private set; }

        /// <summary>Counts for probes: blades of yours driven, thrown by a mark, and called home.</summary>
        public int BladesDriven { get; private set; }
        public int BladesPlanted { get; private set; }
        public int BladesCalledHome { get; private set; }

        /// <summary>As above, with the shape worn given: its presses stand in place of the mark's.</summary>
        public static UpgradeEffect[][] PressesOf(Upgrade mark, ShapeDef worn, out float window) => PressesOf(mark, worn, null, out window);

        /// <summary>
        /// As above, with the Master in force given: the shape's presses, else the mark's own, else the Master's (Riptide's
        /// Master: the wave sent on again by a second press).
        /// </summary>
        public static UpgradeEffect[][] PressesOf(Upgrade mark, ShapeDef worn, Upgrade master, out float window)
        {
            if (worn != null && worn.Presses != null && worn.Presses.Length > 0)
            {
                window = worn.PressWindow;
                return worn.Presses;
            }
            if (mark != null && mark.Presses != null && mark.Presses.Length > 0)
            {
                window = mark.PressWindow;
                return mark.Presses;
            }
            if (master != null && master.Presses != null && master.Presses.Length > 0)
            {
                window = master.PressWindow;
                return master.Presses;
            }
            window = mark != null ? mark.PressWindow : 0f;
            return null;
        }

        // An ebb waiting for its flood (The Tide Turns): the mark, its numbers, where it began, its cast's number and until when.
        private Upgrade ebbMark;
        private readonly SpellSpec ebbSpec = new SpellSpec();
        private Vector3 ebbFrom;
        private int ebbTag;
        private float ebbUntil;

        /// <summary>True while an ebb waits for its flood, for probes.</summary>
        public bool Ebbing => ebbMark != null && Time.time < ebbUntil;
        /// <summary>Counts for probes: ebbs drawn back and floods loosed.</summary>
        public int Ebbs { get; private set; }
        public int Floods { get; private set; }

        /// <summary>
        /// The flood's reach from where the player stands at <paramref name="here"/>: back to where the ebb began at
        /// <paramref name="from"/> and on past it by <paramref name="reach"/> less <paramref name="ebb"/>. Level.
        /// </summary>
        public static float FloodReach(Vector3 here, Vector3 from, float reach, float ebb)
        {
            Vector3 back = from - here;
            back.y = 0f;
            return back.magnitude + Mathf.Max(0f, reach - ebb);
        }

        // What the last wave brought in to the player's feet (Riptide), and the way it was rolling.
        private readonly Component[] brought = new Component[12];
        private int broughtCount;
        private Vector3 broughtWay;

        /// <summary>A wave has come in rolling along <paramref name="way"/>: what it brings is noted next (NoteBrought).</summary>
        public void BeginBrought(Vector3 way)
        {
            broughtCount = 0;
            way.y = 0f;
            broughtWay = way.sqrMagnitude > 0.0001f ? way.normalized : transform.forward;
        }

        /// <summary>The wave brought this enemy in.</summary>
        public void NoteBrought(Component body)
        {
            if (body != null && broughtCount < brought.Length) brought[broughtCount++] = body;
        }

        /// <summary>Where a body is carried on to: <paramref name="beyond"/> metres past <paramref name="from"/> along <paramref name="way"/> (Riptide's Master).</summary>
        public static Vector3 CarriedOnTo(Vector3 from, Vector3 way, float beyond)
        {
            way.y = 0f;
            return from + (way.sqrMagnitude > 0.0001f ? way.normalized : Vector3.forward) * beyond;
        }

        /// <summary>Counts for probes: enemies a wave carried on past the player (Riptide's Master).</summary>
        public int CarriedOn { get; private set; }

        // Where the player stood as the slot spell now running was cast (a later press steps back to it).
        private Vector3 castFrom;
        /// <summary>Steps taken back to where a spell was cast (Aruna's Road, There and Back), counted for probes.</summary>
        public int StepsBack { get; private set; }

        /// <summary>Counts for probes: freezes made to last longer (Six Levels Down).</summary>
        public int FreezesKept { get; private set; }

        // ---- What the enemies say of themselves.

        private void OnEnemyDied(Component who, AfflictionSet afflictions)
        {
            if (who == null) return;
            // The death is a moment a spell may be cast just after, by what the dead carried (Reap What Is Sown's Master).
            Moments.Note(CastMoment.Death, Time.time);
            for (int kind = 1; kind < AfflictionKinds; kind++)
                if (afflictions != null && afflictions.Has((AfflictionKind)kind)) Moments.NoteDeathCarrying((AfflictionKind)kind, Time.time);
            Vector3 at = who.transform.position;
            // Hollow Strength: a kill stills the draining of the Risen for a few breaths.
            if (hollowStrength && Risen) riseStilledUntil = Time.time + StilledSeconds;
            // Seeded, it bursts into thorns; cursed, its death feeds the one who killed it. What the Yew Keeps withholds the
            // thorns of a Seeded, Cursed death: its seed passes on whole instead.
            var withheld = YewWithholds(afflictions) ? AfflictionKind.Seeded : AfflictionKind.None;
            float burst = afflictions.DeathBurst(out float radius, withheld);
            if (burst > 0f) Enqueue(new Pending { Special = Special.DeathBurst, Source = laying, At = at, HasAt = true, Power = burst * Stats.Multiplier(PlayerStat.BurstDamage), Radius = radius * Stats.Multiplier(PlayerStat.BurstReach) });
            float fed = afflictions.HealOnKill;
            if (fed > 0f && defence != null) defence.Heal(fed);
            // A ripe seed in the dead bursts ripe (In Its Season): the burst above was made stronger by the set itself.
            if (withheld != AfflictionKind.Seeded && afflictions.IsRipe(AfflictionKind.Seeded)) RipeBurstAt(who, at, afflictions.Stacks(AfflictionKind.Seeded));
            // Dealt by a reaction's own blow, the death is told with that reaction's id (The Feast).
            int killer = combat != null ? combat.StrikingReaction - 1 : -1;
            Trigger(UpgradeTrigger.Kill, null, at, true, 0, afflictions, AfflictionKind.None, killer >= 0 ? AfflictionPairs.Id(killer) : "");
            // Zanshin: the turn to the next enemy (its wider perfect window is the mark's boon).
            if (combat != null && Has(Trees.Ways.KatanaWay.Zanshin)) combat.Zanshin(who);
        }

        private void OnPoisonBit(Component who, int depth, float damage)
        {
            PoisonBites++;
            // The bite's real damage goes with it, for what is given back by it (Venom's mending).
            if (who != null) Trigger(UpgradeTrigger.PoisonBite, who, who.transform.position, true, depth, null, AfflictionKind.None, "", LaySource.Effect, damage);
        }

        /// <summary>Bites of poison heard since the play began, counted for probes.</summary>
        public int PoisonBites { get; private set; }

        /// <summary>Whose spell is striking now (null: the base spell, or no one's). Set round a cast and round each step of a form that lasts.</summary>
        public void Casting(Upgrade source) => castSource = source;

        private void OnLanded(Component who)
        {
            if (who != null) Trigger(UpgradeTrigger.Landed, who, who.transform.position);
        }

        private void OnShattered(Component who)
        {
            if (who == null) return;
            Moments.Note(CastMoment.Shatter, Time.time);
            Trigger(UpgradeTrigger.Shatter, who, who.transform.position);
            // The blow that shattered it is still being counted: it is ended on the next step.
            if (hasCleanBreak && who is Enemy foe && !foe.Kind.IsBoss && foe.HealthFraction < CleanBreakShare && toFinish.Count < 4) toFinish.Add(who);
        }

        private void OnAfflictionFull(Component who, AfflictionKind kind)
        {
            if (who == null) return;
            Vector3 at = who.transform.position;
            // Filled by what a reaction laid (Seized's film, Rust), it is seen in the reaction's own look: no ring (the
            // owner, 2026-11-20: "remove the circles").
            // Nor when the new effects draw the affliction on the body: its own look shows it filling, and the tinted ring
            // round the feet was the glowing ring under every soldier a charged blade filled (Water's blue, Dark's pink,
            // Light's cream at the first blow, Life's green at the third: the swing films of 2026-11-20).
            if (CombatEvents.LaidBy != LaySource.Reaction && !Clover.Player.Forms.ElementArt.Ready)
                Vfx.Play(VfxKind.TintRing, at + Vector3.up * 0.3f, Vector3.up, 1f, Afflictions.Spec(kind).Colour);
            Trigger(UpgradeTrigger.AfflictionFull, who, at);
        }

        // An affliction laid on an enemy: whatever answers a laying is told, unless the laying is such an answer's own
        // (Grafted Edge's second notch must not lay a third).
        private void OnLaid(Component who, AfflictionKind kind, int stacks, LaySource source)
        {
            if (who == null || answeringLaid || byTrigger == null || byTrigger[(int)UpgradeTrigger.AfflictionLaid].Count == 0) return;
            Trigger(UpgradeTrigger.AfflictionLaid, who, who.transform.position, true, stacks, null, kind, "", source == LaySource.Reaction ? LaySource.Reaction : LaySource.Effect);
        }

        private void OnCarried(Component who, AfflictionKind kind, int stacks)
        {
            if (who == null) return;
            // Carried by what a reaction laid, it is the reaction's still (a reaction's blow says so itself).
            var source = CombatEvents.LaidBy == LaySource.Reaction ? LaySource.Reaction : laying;
            Enqueue(new Pending { Special = Special.Carry, Source = source, Target = who, At = who.transform.position, HasAt = true, Kind = kind, Stacks = stacks,
                Radius = Afflictions.Spec(AfflictionKind.Soaked).CarryRadius });
        }

        // ---- What the player's own systems tell this.

        /// <summary>A blow of the weapon has struck something. Nothing here may strike in turn: the swing is still being counted.</summary>
        public void OnBlow(AttackStep step, IDamageable target, HitOutcome outcome, bool wasAfflicted = false) =>
            OnBlow(step, target, outcome, wasAfflicted, step != null && step.Heavy);

        /// <summary>As above; <paramref name="heavy"/> also for a light blow a mark made heavy (UpgradeAction.NextHeavy).</summary>
        public void OnBlow(AttackStep step, IDamageable target, HitOutcome outcome, bool wasAfflicted, bool heavy)
        {
            if (outcome == HitOutcome.Blocked) return;
            var struck = target as Component;
            Vector3 at = struck != null ? struck.transform.position : transform.position;
            bool finisher = outcome == HitOutcome.Finisher;
            // A new swing landing is the next turn; the hits of one swing land in one frame and share it.
            if (Time.frameCount != blowTurnFrame)
            {
                blowTurnFrame = Time.frameCount;
                blowTurn++;
            }
            // Corpse Flowers: a blow landed beside one of the flowers wins back grey health.
            if (Time.time < regainUntil && defence != null && Hazards.GrownNear(transform.position, regainNear, regainFamily)) GreyWonBeside += defence.Health.Regain(regainAmount);
            // Each blow landed adds a ring of light up the blade to a gathered spell (Daybreak).
            if (rings.By == RingSource.Blows) AddRing();
            if (hasHeat && Heat.Landed())
            {
                Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.2f, Vector3.up, 0.8f);
                if (Heat.Level == Heat.MaximumLevel && defence != null) defence.Callout("Heat");
            }
            // The hunger of some surges: a share of each blow comes back.
            if (element != null && element.SurgeActive && element.SurgeKit.SurgeLeech > 0f && defence != null)
                defence.Heal(combat.LastBlowDamage * element.SurgeKit.SurgeLeech, false);

            // A share of every blow comes back, where a mark gives one.
            float leech = Stats.Sum(PlayerStat.Leech);
            if (leech > 0f && defence != null) defence.Heal(combat.LastBlowDamage * leech, false);

            Trigger(UpgradeTrigger.AnyHit, struck, at);
            if (combat.LastBlowCritical) Trigger(UpgradeTrigger.CritHit, struck, at);
            var chain = combat.Combo.Steps;
            if (step == combat.Combo.ChainHeavy || (chain.Count > 0 && step == chain[chain.Count - 1])) Trigger(UpgradeTrigger.ChainEnd, struck, at);
            if (finisher)
            {
                Moments.Note(CastMoment.Finisher, Time.time);
                Trigger(UpgradeTrigger.FinisherHit, struck, at);
            }
            if (step == combat.Combo.Skill) Trigger(UpgradeTrigger.SkillHit, struck, at);
            else if (heavy || finisher) Trigger(UpgradeTrigger.HeavyHit, struck, at);
            // Storm Front: the heavy blow or finisher that lands is owed a Gust, thrown in the next step once the swing has been reckoned.
            // The Gust's own short wait must not turn a heavy blow into a light one for the marks.
            if (heavy || finisher)
            {
                if (Time.time >= stormFrontAt) stormFrontOwed = true;
            }
            else Trigger(UpgradeTrigger.LightHit, struck, at);
            if (hasLeaf && ++leafBlows >= BreathBlows) GainBreath();
            sinceBlowLanded = 0f;
            if (wasAfflicted) Trigger(UpgradeTrigger.AfflictedHit, struck, at);

            var afflictable = target as IAfflictable;
            // Fed Flame: a blow on a burning enemy feeds the burning surge.
            var burning = element != null && element.SurgeActive ? element.SurgeKit : null;
            if (burning != null && burning.SurgeFeeds > 0f && afflictable != null && afflictable.Afflictions.Burn.IsBurning)
            {
                burning.Feed(burning.SurgeFeeds);
                SurgeFed++;
            }
            // What the edge carries (Carried on the Blade, Dust on the Blade) goes into what it strikes.
            if (afflictable != null && struck != null && Coat.Any)
            {
                int coats = Coat.Blow(afflictable.Afflictions, heavy || finisher, Time.time, coatKinds, coatStacks);
                for (int i = 0; i < coats; i++)
                    CombatEvents.Report(struck, coatKinds[i], coatStacks[i], afflictable.Afflictions.Lay(coatKinds[i], coatStacks[i], 0f, combat.AfflictionPower, LaySource.Weapon, struck), LaySource.Weapon);
            }
            if (hasPlasma && (heavy || finisher) && afflictable != null && afflictable.Afflictions.Burn.IsBurning)
                toBurst.Add(target);
            if (!hasStatic || afflictable == null) return;
            int stacks = afflictable.Afflictions.Stacks(AfflictionKind.Static);
            if (heavy || finisher)
            {
                if (stacks > 0) toDischarge.Add(target);
                return;
            }
            // A light blow builds Static only while Lightning is the first element; second, its Static comes from spells.
            if (!staticLays) return;
            float seconds = combat.AfflictionSeconds != 1f ? Afflictions.Spec(AfflictionKind.Static).Seconds * combat.AfflictionSeconds : 0f;
            CombatEvents.Report(struck, AfflictionKind.Static, 1, afflictable.Afflictions.Lay(AfflictionKind.Static, 1 + combat.ExtraStacks, seconds, combat.AfflictionPower, LaySource.Weapon, struck));
            Vfx.Play(VfxKind.Shock, at + Vector3.up * 1.3f, Vector3.up, 0.15f + 0.08f * stacks);
        }

        /// <summary>The swing is over, having struck this many.</summary>
        public void OnSwing(AttackStep step, int struck)
        {
            foreach (var target in toDischarge)
            {
                // Everything built up on it goes at once, into it and into everyone standing near.
                var body = target as Component;
                if (body == null || !(target is IAfflictable afflictable)) continue;
                int stacks = afflictable.Afflictions.Consume(AfflictionKind.Static);
                if (stacks <= 0) continue;
                Discharges++;
                // What it deals goes with it, for what carries the discharge on at its strength (Nothing Resists).
                Trigger(UpgradeTrigger.SetOff, body, body.transform.position, true, stacks, null, AfflictionKind.Static, "", LaySource.Effect,
                    StaticDamagePerStack * stacks * (hasStroke ? 2f : 1f));
                if (hasStroke)
                {
                    // Single Stroke: it does not arc. All of it lands on the one struck, twice as hard.
                    StrokeDischarges++;
                    LightningBolts.Strike(body.transform.position, 0.9f);
                    Vfx.Play(VfxKind.TintRing, body.transform.position + Vector3.up * 0.3f, Vector3.up, 0.9f, Afflictions.Spec(AfflictionKind.Static).Colour);
                    combat.StrikeArea(body.transform.position, 0.5f, StaticDamagePerStack * 2f * stacks, StaticStaggerPerStack * 2f * stacks, 0f, VfxKind.Shock);
                    Jolt(0.03f + 0.008f * stacks, 0.2f + 0.03f * stacks, 0.15f);
                    continue;
                }
                LightningBolts.Strike(body.transform.position, 0.5f);
                combat.StrikeArea(body.transform.position, StaticRadius * Stats.Multiplier(PlayerStat.DischargeReach), StaticDamagePerStack * stacks, StaticStaggerPerStack * stacks, 0f, VfxKind.Shock);
            }
            toDischarge.Clear();
            foreach (var target in toBurst)
            {
                // Fire and storm together: the burn goes up, and the burst lights everyone it reaches.
                var body = (Component)target;
                if (body == null) continue;
                PlasmaBursts++;
                Vector3 at = body.transform.position;
                Trigger(UpgradeTrigger.SetOff, body, at, true, 1, null, AfflictionKind.Burn);
                LightningBolts.Strike(at, 0.5f);
                Vfx.Play(VfxKind.Burst, at + Vector3.up);
                Vfx.Play(VfxKind.Ring, at + Vector3.up * 0.2f, Vector3.up);
                for (int i = 0; i < 5; i++)
                    Vfx.Play(VfxKind.Fire, at + Quaternion.Euler(0f, i * 72f, 0f) * Vector3.forward * (PlasmaRadius * 0.5f) + Vector3.up * 0.5f, Vector3.up, 0.7f);
                combat.StrikeArea(at, PlasmaRadius, PlasmaDamage, PlasmaStagger, 4f, VfxKind.Fire, null, true);
            }
            toBurst.Clear();

            if (step == combat.Combo.Skill)
            {
                Vector3 forward = transform.forward;
                if (hasCrescent)
                    Hazards.Lay(HazardKind.Fire, transform.position + forward * 1.4f, 1.5f, CrescentSeconds, CrescentDamage, 12f, 3f, 0.1f, true, 0.05f, forward * CrescentSpeed);
                if (hasCuts && struck > 0)
                    Hazards.Lay(HazardKind.Lightning, transform.position + forward * (step.Reach * 0.5f), step.Reach * 0.65f, 0.12f, step.Damage * DelayedCutsShare, 22f, 0f, 0.1f, true, DelayedCutsDelay, default, true);
            }
            // Each coat the swing used has one blow fewer.
            Coat.Swung();
            if (struck > 0 && untilBlowCount > 0)
            {
                // Boons that were for one blow have had it.
                for (int i = 0; i < untilBlowCount; i++)
                {
                    Stats.EndBoon(untilBlow[i].Stat, untilBlow[i].Amount);
                    untilBlow[i] = null;
                }
                untilBlowCount = 0;
            }
            // The swing is counted: what it set going can be done now.
            Flush();
        }

        /// <summary>A spell struck something; once for each thing it struck.</summary>
        public void OnSpellStruck(IDamageable target, bool wasAfflicted) => OnSpellStruck(target, wasAfflicted, null);

        /// <summary>
        /// Where the spell's blow now landing came from (a field's or burst's middle, a lane's start, the player for a cone), set
        /// by <see cref="PlayerCombat"/> as each strike begins and carried by the moments it sets going (Turned to the Sun).
        /// </summary>
        public Vector3 SpellFrom { get; set; }

        /// <summary>As above, with the element of the spell that struck: a base spell's Master answers only its own element's.</summary>
        public void OnSpellStruck(IDamageable target, bool wasAfflicted, string element) => OnSpellStruck(target, wasAfflicted, element, 0);

        /// <summary>As above, with the number of the cast whose blow it was (<see cref="SpellBlow.Cast"/>; 0 for none): its moments know their cast.</summary>
        public void OnSpellStruck(IDamageable target, bool wasAfflicted, string element, int cast)
        {
            var struck = target as Component;
            if (struck == null) return;
            Vector3 at = struck.transform.position;
            NoteCastHit(struck, cast);
            baseElement = element;
            int was = answeringCast;
            answeringCast = cast;
            try
            {
                Trigger(UpgradeTrigger.SpellHit, struck, at);
                if (wasAfflicted) Trigger(UpgradeTrigger.AfflictedHit, struck, at);
            }
            finally
            {
                baseElement = null;
                answeringCast = was;
            }
        }

        // Whom each cast struck, once each and in order; a blow of another cast starts the list again.
        private void NoteCastHit(Component struck, int cast)
        {
            if (cast == 0) return;
            if (cast != castHitsOf)
            {
                castHits.Clear();
                castHitsOf = cast;
            }
            if (castHits.Count < MostStruck && !castHits.Contains(struck)) castHits.Add(struck);
        }

        /// <summary>A move carrying the charged blade landed on <paramref name="target"/>; <paramref name="third"/> for the charge's last.</summary>
        public void OnChargedBlow(Component target, bool third)
        {
            if (target == null) return;
            Vector3 at = target.transform.position;
            Trigger(UpgradeTrigger.ChargedBlow, target, at);
            if (third) Trigger(UpgradeTrigger.ChargedBlowThird, target, at);
        }

        /// <summary>The element's own spell has left the hand.</summary>
        public void OnSpellCast() => OnSpellCast(null);

        /// <summary>An element's base spell, cast by its kit, has left the hand: <paramref name="element"/> is whose, so its Master answers only its own.</summary>
        public void OnSpellCast(string element)
        {
            BeginCast();
            baseElement = element;
            try { Trigger(UpgradeTrigger.SpellCast, null, transform.position, false); }
            finally { baseElement = null; }
        }

        /// <summary>The blows a charge of the blade from this spell lasts as it is cast now (its shape's and Empower's); 0 for the blade's own.</summary>
        public int ChargeBlowsOf(Upgrade mark)
        {
            var spec = mark != null ? branches.SpecOf(mark, RunState.Unlocked, ReadLoadout()) : null;
            return spec != null ? spec.ChargeBlows : 0;
        }

        /// <summary>The Empower of an element's base spell, when it is learned and in force; null otherwise (its numbers given to the kit's cast).</summary>
        public Empower BaseEmpower(string element)
        {
            if (string.IsNullOrEmpty(element)) return null;
            string id = SpellBranch.EmpowerId(Magic.SpellSlots.BaseId(element));
            var empower = Has(id) ? Upgrade.ById(id)?.Empower : null;
            return empower != null && !empower.NotForKit ? empower : null;
        }

        // While a base spell cast by its kit (or its blow) is being answered: whose element it is (the ledger's base-spell-element).
        private string baseElement;

        /// <summary>When each moment a spell may be cast just after last came (UpgradeEffect.CastAfter).</summary>
        public CastMoments Moments { get; } = new CastMoments();

        /// <summary>A form's own moment came now (Waiting Thunder's last bolt sent by a light blow).</summary>
        public void NoteMoment(CastMoment moment) => Moments.Note(moment, Time.time);

        /// <summary>
        /// True when an effect in force of <paramref name="mark"/>'s own branch quickens its cast now (UpgradeAction.Quicken):
        /// its CastAfter window holds at <paramref name="now"/>.
        /// </summary>
        public static bool Quickens(List<UpgradeEffect> inForce, Upgrade mark, CastMoments moments, float now) => Quickens(inForce, mark, moments, now, null);

        /// <summary>As above; an effect with NearGrown quickens only a cast within that many metres of something grown (<paramref name="grownWithin"/>: Elder Tree's Master).</summary>
        public static bool Quickens(List<UpgradeEffect> inForce, Upgrade mark, CastMoments moments, float now, System.Func<float, bool> grownWithin)
        {
            if (inForce == null || mark == null || moments == null) return false;
            string branch = !string.IsNullOrEmpty(mark.Branch) ? mark.Branch : mark.Id;
            for (int i = 0; i < inForce.Count; i++)
            {
                var effect = inForce[i];
                if (effect.Do != UpgradeAction.Quicken || effect.Owner == null || effect.Owner.Branch != branch) continue;
                if (effect.NearGrown > 0f && (grownWithin == null || !grownWithin(effect.NearGrown))) continue;
                if (moments.Within(effect.CastAfter, effect.CastWindow, now, effect.CastAfterCarrying)) return true;
            }
            return false;
        }
        // True when something the player grew stands within that many metres of them.
        private bool GrownWithin(float metres) => Hazards.GrownNear(transform.position, metres);

        // When the cast now running began, asked against the moments.
        private float castBeganAt = -1000f;

        // A new cast: none passed by it yet, no doses carried down its line, and whether it was cast on the poison's beat.
        private void BeginCast()
        {
            castBeganAt = Time.time;
            castStruck.Clear();
            lineCarried = 0;
            castOnBeat = combat != null && combat.AnyNearBeat(transform.position, BeatReach, BeatWindow);
        }

        /// <summary>Metres within which, and seconds within which, a bite makes a cast one timed to the poison's beat.</summary>
        public const float BeatReach = 8f, BeatWindow = 0.2f;
        // True when the cast now running was cast within a fifth of a second of a bite on any enemy within eight metres.
        private bool castOnBeat;

        /// <summary>
        /// A spell discharged an enemy's Static itself (Paper Seal's thunder): counted, and told to whatever answers a
        /// discharge with how deep it was, as a heavy blow's discharge is.
        /// </summary>
        public void Discharged(Component who, int stacks)
        {
            if (who == null || stacks <= 0) return;
            Discharges++;
            Trigger(UpgradeTrigger.SetOff, who, who.transform.position, true, stacks, null, AfflictionKind.Static, "", LaySource.Effect, StaticDamagePerStack * stacks);
        }

        // The nearest enemy within reach in the condition named (by its name or what it is), leaving out those given; null
        // when none is, or when no condition is named.
        // The numbers a numbered condition is set at, counted again for each cast.
        private int numbered, numberedCast = -1;

        private float NextNumber(int cast)
        {
            if (cast != numberedCast)
            {
                numberedCast = cast;
                numbered = 0;
            }
            return ++numbered;
        }

        /// <summary>
        /// True when an enemy numbered <paramref name="candidate"/> on a line comes after <paramref name="after"/> and before the
        /// best found so far (<paramref name="best"/>; 0 for none): the next on the line by its order.
        /// </summary>
        public static bool NextInLine(float after, float candidate, float best) => candidate > after && (best <= 0f || candidate < best);

        // The next enemy on a numbered line after the number given, within reach of the point; null when none comes after it.
        private Component NextOnLine(string state, float after, Vector3 from, float reach, HashSet<IDamageable> except)
        {
            if (string.IsNullOrEmpty(state) || combat == null || after <= 0f) return null;
            stateScratch.Clear();
            foreach (var left in except) stateScratch.Add(left);
            Component found = null;
            float best = 0f;
            for (int guard = 0; guard < 24; guard++)
            {
                var near = combat.EnemyNear(from, reach, stateScratch);
                if (!(near is IDamageable one)) break;
                stateScratch.Add(one);
                if (!(near is IAfflictable carrier) || !carrier.Afflictions.Conditions.NewsOf(state, ConditionMoment.Began, out var news)) continue;
                if (!NextInLine(after, news.Scale, best)) continue;
                best = news.Scale;
                found = near;
            }
            stateScratch.Clear();
            return found;
        }

        private Component NearestInState(string state, Vector3 from, float reach, HashSet<IDamageable> except)
        {
            if (string.IsNullOrEmpty(state) || combat == null) return null;
            stateScratch.Clear();
            foreach (var left in except) stateScratch.Add(left);
            Component found = null;
            for (int guard = 0; guard < 24; guard++)
            {
                var near = combat.EnemyNear(from, reach, stateScratch);
                if (!(near is IDamageable one)) break;
                stateScratch.Add(one);
                if (InState(state, null, near))
                {
                    found = near;
                    break;
                }
            }
            stateScratch.Clear();
            return found;
        }

        /// <summary>A blow was caught on the guard; the attacker stands at the given place.</summary>
        public void OnGuard(Vector3 attacker)
        {
            NoteGuard(attacker);
            Trigger(UpgradeTrigger.Guard, LastGuardEnemy, attacker);
            TheirBlow(LastGuardEnemy, attacker);
        }

        /// <summary>
        /// An enemy's blow aimed at the player landed or was turned (the ledger's their-blow-moment): the conditions on the
        /// attacker that answer it are told (The Grudge's cloud strikes), and the marks on <see cref="UpgradeTrigger.TheirBlow"/> answer.
        /// </summary>
        private void TheirBlow(Component attacker, Vector3 at, bool turned = false)
        {
            TheirBlows++;
            if (attacker is IAfflictable carrier) carrier.Afflictions.Conditions.Attacked(turned);
            Trigger(UpgradeTrigger.TheirBlow, attacker, at);
        }

        /// <summary>A ward turned a blow from the given place: an enemy's blow turned, as a guard turns one.</summary>
        public void OnWarded(Vector3 attacker)
        {
            // With a mark that says so, a ward's turn is a perfect guard for every mark that answers one (Splinter Ward).
            if (WardIsPerfectGuard(Stats))
            {
                WardsAsGuards++;
                OnPerfectGuard(attacker);
                return;
            }
            TheirBlow(combat != null ? combat.EnemyAt(attacker, 2.5f) : null, attacker);
        }

        /// <summary>
        /// The health a spell still waiting <paramref name="left"/> seconds costs to cast anyway at <paramref name="perSecond"/>
        /// a second (Blood Price), or -1 when it cannot be: no such price, or it would take the last point of
        /// <paramref name="present"/> health.
        /// </summary>
        public static float CooldownHealth(float left, float perSecond, float present)
        {
            if (perSecond <= 0f || left <= 0f) return left <= 0f ? 0f : -1f;
            float price = left * perSecond;
            return present - 1f >= price ? price : -1f;
        }

        /// <summary>True while the marks in force, as their conditions hold now, keep the player unshaken (PlayerStat.Unshaken).</summary>
        public static bool IsUnshaken(PlayerStats stats) => stats != null && stats.Sum(PlayerStat.Unshaken) > 0f;

        /// <summary>True when the marks in force count a blow turned by a ward as a perfect guard (PlayerStat.WardTurnsPerfect).</summary>
        public static bool WardIsPerfectGuard(PlayerStats stats) => stats != null && stats.Sum(PlayerStat.WardTurnsPerfect) > 0f;

        /// <summary>Wards whose turn was counted as a perfect guard, and red attacks turned, counted for probes.</summary>
        public int WardsAsGuards { get; private set; }
        public int RedsTurned { get; private set; }
        /// <summary>Seconds after a red attack is turned in which what answers only a red attack may answer it.</summary>
        public const float RedWindow = 1f;
        private float redTurnedAt = -100f;

        /// <summary>A red attack, which cannot be blocked, was turned by a perfect guard or a counter's own guard.</summary>
        public void RedTurned()
        {
            redTurnedAt = Time.time;
            RedsTurned++;
        }

        /// <summary>True when the enemy shows an attack begun no more than <paramref name="within"/> seconds ago.</summary>
        public static bool InWindup(IAfflictable enemy, float within) => enemy != null && enemy.Afflictions.WindingUp && enemy.Afflictions.WindupFor <= within;

        /// <summary>True within <see cref="RedWindow"/> of a red attack being turned at <paramref name="at"/>.</summary>
        public static bool RedJustTurned(float now, float at) => now >= at && now - at <= RedWindow;

        /// <summary>Enemy blows aimed at the player that landed or were turned, counted for probes.</summary>
        public int TheirBlows { get; private set; }

        /// <summary>Where the last blow caught on the guard came from, and who dealt it; for forms that answer a guard.</summary>
        public Vector3 LastGuardFrom { get; private set; }
        public Component LastGuardEnemy { get; private set; }

        private void NoteGuard(Vector3 attacker)
        {
            LastGuardFrom = attacker;
            LastGuardEnemy = combat != null ? combat.EnemyAt(attacker, 2.5f) : null;
        }

        /// <summary>A blow was turned at the last moment; the attacker stands at the given place.</summary>
        public void OnPerfectGuard(Vector3 attacker)
        {
            Moments.Note(CastMoment.PerfectGuard, Time.time);
            NoteGuard(attacker);
            // A bag open catches the attacker's wind (Bag of Winds).
            if (rings.By == RingSource.Turned) AddRing();
            // Open Furnace sets off every burning enemy near, the attacker among them, so the one detonation is not also made.
            bool detonate = hasDetonate && !hasFurnace;
            if (hasFurnace) OpenFurnace();
            if (hasAnswer)
            {
                answerUntil = Time.time + AnswerSeconds;
                Vfx.Play(VfxKind.Glint, transform.position + Vector3.up * 1.4f);
            }
            if (hasBackdraft)
            {
                Vector3 toward = attacker - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
                int before = combat.StrikeCone(BackdraftReach * Stats.Multiplier(PlayerStat.BackdraftReach), BackdraftArc, BackdraftDamage, BackdraftStagger * Stats.Multiplier(PlayerStat.BackdraftStagger), 3f, VfxKind.Fire, detonate);
                if (detonate && before > 0)
                {
                    Detonations++;
                    Trigger(UpgradeTrigger.SetOff, combat.EnemyAt(attacker, 2.5f), attacker, true, 1, null, AfflictionKind.Burn);
                }
                Vector3 hand = transform.position + Vector3.up * 1.1f;
                for (int i = 1; i <= 3; i++)
                    Vfx.Play(VfxKind.Fire, hand + transform.forward * (BackdraftReach * i / 3.5f), transform.forward, 0.7f);
                Vfx.Play(VfxKind.Embers, hand + transform.forward * 1.5f, transform.forward, 1.5f);
            }
            else if (detonate && combat.StrikeArea(attacker, 1.8f * Stats.Multiplier(PlayerStat.BackdraftReach), 0f, 0f, 0f, VfxKind.Burst, null, true) > 0)
            {
                Detonations++;
            }
            if (hasCharge)
            {
                combat.NextBlowDamage = StoredChargeMultiplier;
                // Deep Reserve: the charge is banked as well, up to three; the count is what is left, this one included.
                if (reserveIndex >= 0 && reserveIndex < tally.Length)
                {
                    tally[reserveIndex] = Mathf.Min(ReserveMost, tally[reserveIndex] + 1);
                    tallyDirty = true;
                }
                combat.NextBlowStagger = StoredChargeMultiplier * Stats.Multiplier(PlayerStat.ChargeStagger);
                Vfx.Play(VfxKind.Shock, transform.position + Vector3.up * 1.2f, Vector3.up, 0.8f);
            }
            var struckBy = combat.EnemyAt(attacker, 2.5f);
            NoteTurned(struckBy);
            // Twin Break: Burn and Static on the one who struck are spent together, before anything sets either off alone.
            if (struckBy is IAfflictable both && both.Afflictions.Has(AfflictionKind.Burn) && both.Afflictions.Has(AfflictionKind.Static) && Has(Trees.PlasmaTree.TwinBreak))
                TwinBreak(struckBy, both.Afflictions);
            Trigger(UpgradeTrigger.PerfectGuard, struckBy, attacker);
            TheirBlow(struckBy, attacker, true);
        }

        /// <summary>How many times Twin Break has spent Burn and Static together, and how many enemies have melted down: for probes.</summary>
        public int TwinBreaks { get; private set; }
        public int Meltdowns { get; private set; }

        private void TwinBreak(Component who, AfflictionSet set)
        {
            int burn = set.Stacks(AfflictionKind.Burn), charge = set.Stacks(AfflictionKind.Static);
            set.SetOff(AfflictionKind.Burn);
            set.SetOff(AfflictionKind.Static);
            var blow = Blow(lookOf[(int)AfflictionKind.Burn], AfflictionKind.None, PlasmaRules.TwinDamage(burn, charge) * Stats.Multiplier(PlayerStat.BurstDamage), PlasmaRules.TwinStagger(burn, charge) * Stats.Multiplier(PlayerStat.BurstStagger), 0);
            // A heavy blow makes what is not steadfast flinch: the attack it had begun is broken.
            blow.Heavy = true;
            blow.Spell = true;
            combat.StrikeOne(who, blow, transform.position);
            TwinBreaks++;
            Vector3 at = who.transform.position;
            Color fire = Afflictions.Spec(AfflictionKind.Burn).Colour, spark = Afflictions.Spec(AfflictionKind.Static).Colour;
            // Fire up from under it and lightning down onto it in the one moment.
            for (int i = 0; i < 3; i++)
                Forms.FormFx.Erupt(Forms.FormMesh.Flame, at + Forms.FormRules.OnRing(i, 3, 0.5f, 40f), Quaternion.Euler(0f, i * 120f, 0f), new Vector3(1.2f, 2.6f + 0.3f * burn, 1.2f), fire, 0.06f, 0.3f, 0.35f);
            Forms.FormFx.Drop(Forms.FormMesh.Bolt, at + Vector3.up * 8f, at, Quaternion.LookRotation(Vector3.down), new Vector3(3.5f, 3.5f, 8f), Color.white, 0.04f, 0.12f, 0.08f, 0.03f);
            Forms.SigKit.Column(at, Forms.FormKit.Lit(spark, 3.4f), 1.1f, 0.14f);
            Forms.FormFx.Lay(Forms.FormMark.Ring, at, 0.8f, Forms.FormKit.Lit(fire, 3.6f), 0.3f, 5f);
            Forms.FormFx.Lay(Forms.FormMark.Ring, at, 0.4f, Forms.FormKit.Lit(spark, 3.6f), 0.4f, 6.5f);
            Forms.FormFx.Face(Forms.FormMark.Star, at + Vector3.up * 1.1f, 2f, Forms.FormKit.Lit(Color.white, 3.4f), 0.1f, 5f);
            Forms.FormFx.Lay(Forms.FormMark.Crack, at, 3.4f, Forms.FormKit.Lit(spark, 2.4f), 1.4f);
            Jolt(0.1f, 0.45f, 0.25f);
        }

        // Meltdown: looked for a few times a second among the enemies near; one enemy melts at a time.
        private Component melting;
        private float meltLeft, meltPulse, meltLook;
        private Component meltedLast;
        private float meltedAt = -100f;

        private void Melt(float deltaTime)
        {
            if (melting != null)
            {
                if (!melting.gameObject.activeInHierarchy || (melting is Enemy fallen && fallen.IsDead))
                {
                    melting = null;
                    return;
                }
                Vector3 at = melting.transform.position;
                meltLeft -= deltaTime;
                meltPulse -= deltaTime;
                if (meltPulse <= 0f)
                {
                    // It shakes with light, quicker and quicker, inside the ring that says how far the burst will reach.
                    meltPulse = PlasmaRules.Pulse(meltLeft);
                    float near = 1f - Mathf.Clamp01(meltLeft / PlasmaRules.MeltSeconds);
                    Forms.FormFx.Face(Forms.FormMark.Star, at + Vector3.up * 1.2f + Random.insideUnitSphere * 0.25f, 1.2f + 1.6f * near, Forms.FormKit.Lit(Color.white, 3f + 1.5f * near), 0.07f, 0.6f, Random.value * 90f);
                    Forms.FormFx.Lay(Forms.FormMark.Ring, at, PlasmaRules.MeltRadius * 2f, Forms.FormKit.Lit(Color.Lerp(Afflictions.Spec(AfflictionKind.Burn).Colour, Color.white, near), 0.8f + 0.8f * near, 0.5f), meltPulse + 0.05f, PlasmaRules.MeltRadius * 2f * (1f - 0.12f * near));
                    if (near > 0.4f) Forms.FormFx.Drop(Forms.FormMesh.Bolt, at + Vector3.up * 2.4f + Random.insideUnitSphere, at + Vector3.up * 1f, Random.rotation, new Vector3(1.6f, 1.6f, 1.6f), Color.white, 0.03f, 0.05f, 0.05f, 0.02f);
                }
                if (meltLeft > 0f) return;
                var done = melting;
                melting = null;
                meltedLast = done;
                meltedAt = Time.time;
                Meltdowns++;
                if (done is IAfflictable spent)
                {
                    spent.Afflictions.SetOff(AfflictionKind.Burn);
                    spent.Afflictions.SetOff(AfflictionKind.Static);
                }
                var blow = Blow(lookOf[(int)AfflictionKind.Static], AfflictionKind.None, PlasmaRules.MeltDamage * Stats.Multiplier(PlayerStat.BurstDamage), PlasmaRules.MeltStagger * Stats.Multiplier(PlayerStat.BurstStagger), 0);
                blow.Heavy = true;
                blow.Spell = true;
                combat.StrikeOne(done, blow, at + Vector3.up);
                // Everything else in the ring takes half of each. The player is not harmed.
                furnaceSeen.Clear();
                if (done is IDamageable self) furnaceSeen.Add(self);
                blow.Damage *= PlasmaRules.MeltShare;
                blow.Stagger *= PlasmaRules.MeltShare;
                combat.StrikeArea(at, PlasmaRules.MeltRadius, blow, furnaceSeen);
                furnaceSeen.Clear();
                Color fire = Afflictions.Spec(AfflictionKind.Burn).Colour, spark = Afflictions.Spec(AfflictionKind.Static).Colour;
                Forms.FormFx.Lay(Forms.FormMark.Disc, at, PlasmaRules.MeltRadius * 2f, Forms.FormKit.Lit(Color.white, 2.4f, 0.8f), 0.12f);
                Forms.FormFx.Lay(Forms.FormMark.Ring, at, 1f, Forms.FormKit.Lit(Color.white, 4f), 0.35f, PlasmaRules.MeltRadius * 2.6f);
                Forms.FormFx.Lay(Forms.FormMark.Ring, at, 0.5f, Forms.FormKit.Lit(fire, 3.4f), 0.5f, PlasmaRules.MeltRadius * 2.2f);
                Forms.FormFx.Lay(Forms.FormMark.Star, at, 2f, Forms.FormKit.Lit(spark, 3.6f), 0.3f, PlasmaRules.MeltRadius * 3.4f);
                Forms.FormFx.Lay(Forms.FormMark.Crack, at, PlasmaRules.MeltRadius * 1.8f, Forms.FormKit.Lit(fire, 2.6f), 3f);
                Forms.SigKit.Column(at, Forms.FormKit.Lit(Color.white, 3.6f), 2.6f, 0.22f);
                for (int i = 0; i < 12; i++)
                {
                    Vector3 outward = Forms.FormRules.OnRing(i, 12, 1f, 11f);
                    if (i % 2 == 0) Forms.FormFx.Fly(Forms.FormMesh.Fireball, at + Vector3.up, at + outward * PlasmaRules.MeltRadius + Vector3.up * 0.4f, Quaternion.LookRotation(outward), Vector3.one * 1.2f, fire, 0.26f);
                    else Forms.FormFx.Fly(Forms.FormMesh.Bolt, at + Vector3.up, at + outward * PlasmaRules.MeltRadius + Vector3.up * 0.4f, Quaternion.LookRotation(outward), new Vector3(2.4f, 2.4f, PlasmaRules.MeltRadius), Color.white, 0.18f, 0.03f);
                    Forms.FormFx.Erupt(Forms.FormMesh.Flame, at + outward * (PlasmaRules.MeltRadius * 0.5f), Quaternion.Euler(0f, i * 30f, 0f), new Vector3(1.1f, 2f + 0.6f * (i % 3), 1.1f), fire, 0.07f, 0.4f, 0.4f);
                }
                ScreenFlash.Flash(new Color(1f, 0.95f, 0.85f, 0.5f), 0.09f);
                Jolt(0.16f, 0.8f, 0.4f);
                return;
            }
            meltLook -= deltaTime;
            if (meltLook > 0f) return;
            meltLook = 0.25f;
            if (combat == null || !Has(Trees.PlasmaTree.Meltdown)) return;
            int mostBurn = Afflictions.Spec(AfflictionKind.Burn).MaximumStacks, mostCharge = Afflictions.Spec(AfflictionKind.Static).MaximumStacks;
            furnaceSeen.Clear();
            while (furnaceSeen.Count < 12)
            {
                var near = combat.EnemyNear(transform.position, 14f, furnaceSeen);
                if (!(near is IDamageable one)) break;
                furnaceSeen.Add(one);
                if (!(near is IAfflictable carrier)) continue;
                if (ReferenceEquals(near, meltedLast) && Time.time - meltedAt < PlasmaRules.MeltAgain) continue;
                if (!PlasmaRules.Melts(carrier.Afflictions.Stacks(AfflictionKind.Burn), carrier.Afflictions.Stacks(AfflictionKind.Static), mostBurn, mostCharge)) continue;
                melting = near;
                meltLeft = PlasmaRules.MeltSeconds;
                meltPulse = 0f;
                if (defence != null) defence.Callout("Meltdown");
                break;
            }
            furnaceSeen.Clear();
        }

        /// <summary>Open Furnace: every burning enemy within reach is set off where it stands, and keeps a little of its burn.</summary>
        private void OpenFurnace()
        {
            if (combat == null) return;
            Vector3 self = transform.position;
            furnaceList.Clear();
            furnaceSeen.Clear();
            while (furnaceSeen.Count < 24)
            {
                var near = combat.EnemyNear(self, FurnaceRadius, furnaceSeen);
                if (!(near is IDamageable one)) break;
                furnaceSeen.Add(one);
                if (near is IAfflictable lit && lit.Afflictions.Burn.IsBurning) furnaceList.Add(near);
            }
            furnaceSeen.Clear();
            Color fire = Afflictions.Spec(AfflictionKind.Burn).Colour;
            Vfx.Play(VfxKind.TintRing, self + Vector3.up * 0.3f, Vector3.up, FurnaceRadius * 0.5f, fire);
            int burst = 0;
            for (int i = 0; i < furnaceList.Count; i++)
            {
                var body = furnaceList[i];
                if (body == null || !(body is IAfflictable carrier)) continue;
                Vector3 at = body.transform.position;
                int depth = carrier.Afflictions.Stacks(AfflictionKind.Burn);
                // Itself alone: each bursts where it stands, and its neighbours burst for themselves.
                if (combat.StrikeArea(at, 0.6f, 0f, 0f, 0f, VfxKind.Burst, null, true) <= 0) continue;
                burst++;
                FurnaceBursts++;
                Detonations++;
                Vfx.Play(VfxKind.Fire, at + Vector3.up * 1.1f, Vector3.up, 1.3f);
                Vfx.Play(VfxKind.Embers, at + Vector3.up * 1.4f, Vector3.up, 1.6f);
                Vfx.Play(VfxKind.TintRing, at + Vector3.up * 0.3f, Vector3.up, 1.4f, fire);
                if (body.gameObject.activeInHierarchy && FurnaceKeeps > 0)
                    CombatEvents.Report(body, AfflictionKind.Burn, FurnaceKeeps, carrier.Afflictions.Lay(AfflictionKind.Burn, FurnaceKeeps, 0f, combat.AfflictionPower, LaySource.Effect, body));
                Trigger(UpgradeTrigger.SetOff, body, at, true, Mathf.Max(1, depth), null, AfflictionKind.Burn);
            }
            furnaceList.Clear();
            if (burst > 0) Jolt(0.05f + 0.015f * Mathf.Min(burst, 4), 0.3f + 0.08f * Mathf.Min(burst, 4), 0.2f);
        }

        /// <summary>Clean Break: what was shattered with little life left is ended, a step after the blow that shattered it.</summary>
        private void FinishBroken()
        {
            for (int i = 0; i < toFinish.Count; i++)
            {
                var body = toFinish[i];
                if (body == null || !body.gameObject.activeInHierarchy || combat == null) continue;
                if (!(body is Enemy foe) || foe.Health.IsDead) continue;
                Vector3 at = body.transform.position;
                Vfx.Play(VfxKind.TintRing, at + Vector3.up * 0.3f, Vector3.up, 1.6f, Afflictions.Spec(AfflictionKind.Chill).Colour);
                Vfx.Play(VfxKind.Burst, at + Vector3.up * 1.1f, Vector3.up, 1.4f);
                combat.StrikeArea(at, 0.5f, foe.Health.Maximum * 4f, 0f, 0f, VfxKind.Burst);
                CleanBreaks++;
                Jolt(0.07f, 0.35f, 0.15f);
            }
            toFinish.Clear();
        }

        /// <summary>
        /// Spring Tide: a spell of this cost is about to be cast. If the mana for it is there, the spell is made as great as
        /// all the mana held over its cost (to <see cref="TideMost"/>; reach by the square root) for the moment of the cast.
        /// True: it rose, and <see cref="TideSpent"/> is to be called once the cast is paid for.
        /// </summary>
        public bool TideRises(float cost)
        {
            if (!hasTide || combat == null || combat.Mana.Sealed || cost <= 0f || combat.Mana.Current < cost) return false;
            float greater = Mathf.Clamp(combat.Mana.Current / cost, 1f, TideMost);
            LastTide = greater;
            if (greater > 1.001f)
            {
                Stats.AddBoon(PlayerStat.SpellDamage, greater - 1f, TideSeconds);
                Stats.AddBoon(PlayerStat.SpellReach, Mathf.Sqrt(greater) - 1f, TideSeconds);
            }
            return true;
        }

        /// <summary>Spring Tide: the cast is paid for, and whatever mana is left goes with it.</summary>
        public void TideSpent()
        {
            if (combat != null) combat.Mana.Empty();
            Tides++;
            if (LastTide > 1.5f) Jolt(0.03f, 0.2f, 0.15f);
        }

        /// <summary>Leaf on the Wind: a dodge asks for a breath. With none held there is no dodge.</summary>
        private bool SpendBreath()
        {
            if (leafIndex < 0 || leafIndex >= tally.Length || tally[leafIndex] <= 0) return false;
            tally[leafIndex]--;
            ApplyTallies();
            BreathsSpent++;
            return true;
        }

        private void GainBreath()
        {
            leafClock = 0f;
            leafBlows = 0;
            if (leafIndex < 0 || leafIndex >= tally.Length || tally[leafIndex] >= 3) return;
            tally[leafIndex]++;
            ApplyTallies();
        }

        /// <summary>For this long the player cannot be made to flinch (a form that braces them asks for it).</summary>
        public void StandFirm(float seconds) => firmUntil = Mathf.Max(firmUntil, Time.time + seconds);

        /// <summary>A blow passed through the very start of a dodge.</summary>
        public void OnPerfectDodge() => OnPerfectDodge(transform.position + transform.forward * 2.5f);

        /// <summary>A blow was dodged at the last moment; the one who threw it stands at the given place.</summary>
        public void OnPerfectDodge(Vector3 attacker)
        {
            Moments.Note(CastMoment.PerfectDodge, Time.time);
            if (rings.By == RingSource.Turned) AddRing();
            // The attacker is the target, as with a perfect guard; a thrown bolt leaves only the place it came from.
            var dodged = combat != null ? combat.EnemyAt(attacker, 2.5f) : null;
            NoteTurned(dodged);
            perfectThisDodge = true;
            if (dodged != null) Trigger(UpgradeTrigger.PerfectDodge, dodged, attacker);
            else Trigger(UpgradeTrigger.PerfectDodge, null, transform.position, false);
            if (!hasAfterimage) return;
            // What was left standing where the player stood strikes a moment later.
            Vfx.Play(VfxKind.Glint, dodgeFrom + Vector3.up * 1.2f);
            if (afterimage != null && !hasBlink) afterimage.Leave(AfterimageDelay + 0.25f);
            Hazards.Lay(HazardKind.Lightning, dodgeFrom, 2.3f, 0.12f, AfterimageDamage * Stats.Multiplier(PlayerStat.AfterimagePower), AfterimageStagger * Stats.Multiplier(PlayerStat.AfterimagePower), 0f, 0.1f, true, AfterimageDelay, default, true);
        }

        /// <summary>A dodge has begun from this place, in this direction.</summary>
        public void OnDodge(Vector3 from, Vector3 direction, float distance, float seconds)
        {
            // Boons that were for one dodge have had it (Second Wind).
            for (int i = 0; i < untilDodgeCount; i++)
            {
                Stats.EndBoon(untilDodge[i].Stat, untilDodge[i].Amount);
                untilDodge[i] = null;
            }
            untilDodgeCount = 0;
            dodgeFrom = from;
            perfectThisDodge = false;
            DodgeCount++;
            LastDodgeFrom = from;
            LastDodgeDirection = direction;
            sinceDodge = 0f;
            lastDodgeWay = WayOf(direction, from, true);
            // Begun on wet sand of your own, the dodge goes through it, whatever it ends on (The Land Was Sea).
            dodgeFromSand = InOwnGround(OwnGrounds.WetSand);
            if (dodgeFromSand) Stats.SetWhen(StatWhen.OnWetSand, true);
            if (hasIntoFire && lastDodgeWay == DodgeWay.Toward)
            {
                // Into the Fire: the roll is a lunge in flame.
                var fire = lookOf[(int)AfflictionKind.Burn];
                ElementLook.Sparks(fire, from + Vector3.up, direction, 1.4f);
                ElementLook.Puff(fire, from + Vector3.up * 0.6f, direction, 1f);
            }
            else if (hasIntoFire && lastDodgeWay == DodgeWay.Away) Vfx.Play(VfxKind.Dust, from, -direction, 1.2f);
            if (hasTrail)
            {
                // Laid behind the roll as it goes, a patch at a time.
                for (int i = 0; i < 4; i++)
                    Hazards.Lay(HazardKind.Fire, from + direction * (distance * (i + 0.5f) / 4.5f), 0.95f, TrailSeconds * Stats.Multiplier(PlayerStat.TrailSeconds), TrailDamage * Stats.Multiplier(PlayerStat.TrailDamage), 0f, 2f, 0.5f, false, seconds * i / 4f);
            }
            // The god's own step: while the surge lasts the dodge is free, and it shocks whether or not Blink was learned.
            bool stormStep = hasStormStep && element != null && element.SurgeActive;
            if (stormStep)
            {
                FreeDodges++;
                player.Stamina.Restore(player.Dodge.StaminaCost);
            }
            if (hasBlink || stormStep)
            {
                if (afterimage != null) afterimage.Leave(AfterimageSeconds);
                LightningBolts.Strike(from, 0.25f);
                Hazards.Lay(HazardKind.Lightning, from, 1.1f * Stats.Multiplier(PlayerStat.BlinkWidth), seconds, BlinkDamage * Stats.Multiplier(PlayerStat.BlinkPower), BlinkStagger * Stats.Multiplier(PlayerStat.BlinkPower), 0f, 0.05f, true, 0f, direction * (distance / Mathf.Max(0.05f, seconds)));
            }
            Trigger(UpgradeTrigger.DodgeStart, null, from, false);
        }

        /// <summary>A dodge has run its length.</summary>
        public void OnDodgeEnd()
        {
            if (hasIntoFire && lastDodgeWay == DodgeWay.Toward && combat != null)
            {
                // Into the Fire: everything the dodge passed through is set burning two deep, each once. The whole roll is
                // reckoned, from where it began along its way: its last step is not yet applied when its end is told.
                Vector3 path = LastDodgeDirection * player.Dodge.Distance;
                path.y = 0f;
                float length = path.magnitude;
                if (length > 0.2f)
                {
                    var fire = lookOf[(int)AfflictionKind.Burn];
                    furnaceSeen.Clear();
                    int passed = combat.StrikeLane(dodgeFrom, path / length, length, IntoFireWidth * 0.5f, Blow(fire, AfflictionKind.Burn, 0f, 0f, IntoFireStacks), furnaceSeen);
                    furnaceSeen.Clear();
                    if (passed > 0) DodgesThrough++;
                    for (int i = 1; i <= 3; i++) ElementLook.Puff(fire, dodgeFrom + path * (i / 3.5f) + Vector3.up * 0.8f, Vector3.up, 0.9f);
                }
            }
            Trigger(UpgradeTrigger.DodgeEnd, null, transform.position, false);
            if (perfectThisDodge) Trigger(UpgradeTrigger.PerfectDodgeEnd, null, transform.position, false);
            perfectThisDodge = false;
            // Carried to the enemy whose sight drew the dodge (Eagle's Sight): told with it as the target.
            if (carriedTo != null)
            {
                var to = carriedTo;
                carriedTo = null;
                if (to != null && to.gameObject.activeInHierarchy) Trigger(UpgradeTrigger.CarriedTo, to, to.transform.position);
            }
        }

        // The dodge under way was perfect (told again at its end), and the enemy it is carrying you to.
        private bool perfectThisDodge;
        private Component carriedTo;

        /// <summary>
        /// The terms of a dodge about to go this way (Into the Fire): toward or through the enemy faced, safe for its whole
        /// length and through bodies; away from it, twice the stamina and never perfect. Plain otherwise.
        /// </summary>
        public DodgeTerms DodgeTermsFor(Vector3 direction)
        {
            var terms = DodgeTerms.Plain;
            // Toward an enemy whose condition draws your dodge (Eagle's Sight), within 6 m: carried all the way to it.
            carriedTo = null;
            if (combat != null)
            {
                fanTaken.Clear();
                for (int guard = 0; guard < 16; guard++)
                {
                    var near = combat.EnemyNear(transform.position, DodgeRules.CarryReach, fanTaken);
                    if (!(near is IDamageable one)) break;
                    fanTaken.Add(one);
                    if (!(near is IAfflictable drawn) || !drawn.Afflictions.Conditions.Has(ConditionFlags.DrawsDodge)) continue;
                    float reach = DodgeRules.CarryTo(transform.position, direction, near.transform.position);
                    if (reach <= 0f) continue;
                    terms.Reach = reach;
                    carriedTo = near;
                    break;
                }
                fanTaken.Clear();
            }
            if (!hasIntoFire) return terms;
            switch (WayOf(direction, transform.position))
            {
                case DodgeWay.Toward:
                    terms.WholeSafe = true;
                    terms.PassesBodies = true;
                    break;
                case DodgeWay.Away:
                    terms.CostScale = 2f;
                    terms.NoPerfect = true;
                    break;
            }
            return terms;
        }

        // Which way a dodge from here goes against the enemy it is measured against. Only the dodge itself holds that enemy
        // (hold true, from OnDodge), so asking a roll's terms first holds nothing.
        private DodgeWay WayOf(Vector3 direction, Vector3 from, bool hold = false)
        {
            var against = DodgeFoe(from);
            if (hold) dodgeHold.Took(against, Time.time);
            return DodgeRules.WayOf(direction, from, against != null ? against.position : from, against != null);
        }

        // The enemy a dodge from here is measured against: the one the first dodge of the run held, while it lives, is within
        // DodgeHoldRange and the last dodge was no more than DodgeHoldSeconds ago, because a roll turns the player and the
        // enemy ahead after it is often another one in a crowd; otherwise the enemy faced (the one ahead, else the nearest,
        // within the fight).
        private Transform DodgeFoe(Vector3 from)
        {
            var held = dodgeHold.Held;
            // Asked as a plain reference: a held enemy destroyed since must still be let go.
            if (!ReferenceEquals(held, null))
            {
                bool alive = held != null && PlayerReactions.Alive(held.GetComponent<IDamageable>() as Component);
                if (dodgeHold.Keeps(from, alive, alive ? held.position : from, DodgeHoldRange, Time.time, DodgeHoldSeconds)) return held;
            }
            return combat != null ? combat.EnemyAhead(FightRange) : null;
        }

        /// <summary>High Water, Low Water: the tide turns, high to low or low to high, and its clock starts again.</summary>
        public void TurnTide()
        {
            if (!hasTideClock) return;
            tideHigh = !tideHigh;
            tideClock = 0f;
            TidesTurned++;
            var water = lookOf[(int)AfflictionKind.Soaked];
            ElementLook.Ring(water, transform.position + Vector3.up * 0.1f);
            if (tideHigh) ElementLook.Puff(water, transform.position + Vector3.up * 0.3f, Vector3.up, 1.2f);
            if (defence != null) defence.Callout(tideHigh ? "High water" : "Low water");
        }

        /// <summary>A reaction's effects are being put in the line on an enemy: a ripe seed there makes them stronger (In Its Season).</summary>
        public void ReactingOn(Component who, int pair)
        {
            ripeScale = 1f;
            // The reaction's own effects are counted as its blows from here; the enemy it went off on is among what it struck.
            // (Told before the window opens, so what answers it is not counted as the reaction's own.)
            if (pair >= 0)
            {
                reactionCast = reactionCast >= int.MaxValue - 1 ? 1 : reactionCast + 1;
                OnReactionStruck(who, pair, reactionCast);
                reactionPair = pair;
            }
            if (!(who is IAfflictable afflicted) || pair < 0) return;
            var set = afflicted.Afflictions;
            var seed = AfflictionKind.Seeded;
            if (AfflictionPairs.First(pair) != seed && AfflictionPairs.Second(pair) != seed) return;
            bool ripeNow = set.IsRipe(seed);
            float share = set.RipeShare(seed);
            if (ripeNow) RipeBurstAt(who, who.transform.position, set.Stacks(seed));
            ripeScale = share;
        }

        /// <summary>The reaction's effects are in the line: what follows is no reaction's.</summary>
        public void ReactedOn()
        {
            ripeScale = 1f;
            reactionPair = -1;
        }

        // The reaction whose own effects are being put in the line (-1: none), the times reactions have gone off, and the
        // reaction whose effect is being done now (pair plus one) with its time: what its blows carry.
        private int reactionPair = -1, reactionCast, layingReaction, layingCast;
        // True while an answer to a laying is being done: what it lays is not answered again.
        private bool answeringLaid;
        // True while an answer to an arc leaving an enemy is being done: its own arcs are not answered again (Nothing Resists).
        private bool answeringArc;

        /// <summary>
        /// An arc struck <paramref name="left"/> and leaves it for the next, carrying <paramref name="damage"/> (the ledger's
        /// arc-leaves): what answers an arc leaving an enemy is told, unless the arc is such an answer's own.
        /// </summary>
        public void OnArcLeft(Component left, float damage)
        {
            if (left == null || answeringArc || byTrigger == null || byTrigger[(int)UpgradeTrigger.ArcLeft].Count == 0) return;
            ArcsLeft++;
            Trigger(UpgradeTrigger.ArcLeft, left, left.transform.position, true, 0, null, AfflictionKind.None, "", LaySource.Effect, damage);
        }

        /// <summary>True when the mark answers an arc leaving an enemy (UpgradeTrigger.ArcLeft): its own arcs must not be answered by it again.</summary>
        public static bool AnswersArcs(Upgrade mark)
        {
            if (mark == null || mark.Effects == null) return false;
            foreach (var effect in mark.Effects)
                if (effect != null && effect.On == UpgradeTrigger.ArcLeft) return true;
            return false;
        }

        /// <summary>Arcs told as leaving an enemy (UpgradeTrigger.ArcLeft): for probes.</summary>
        public int ArcsLeft { get; private set; }
        private readonly StruckOnce struckOnce = new StruckOnce();

        /// <summary>
        /// A reaction's own blow landed on <paramref name="who"/> (or it went off on it): whatever answers what a reaction
        /// struck is told, once for each enemy each time a reaction goes off (the ledger's reaction-struck).
        /// </summary>
        public void OnReactionStruck(Component who, int pair, int cast, float damage = 0f)
        {
            if (who == null || pair < 0 || cast <= 0 || !struckOnce.First(cast, who)) return;
            var mark = ReactionTable.MarkFor(pair);
            if (mark == null) return;
            // The blow's damage goes with it, for what repeats it (Collected Twice).
            Trigger(UpgradeTrigger.ReactionStruck, who, who.transform.position, true, 0, null, AfflictionKind.None, mark.Id, LaySource.Reaction, damage);
        }

        /// <summary>
        /// What the marks in force make of the reaction of <paramref name="pair"/> as it fires on <paramref name="who"/>: its own
        /// damage multiplied, and whether it spends nothing (ReactionRules.Boost).
        /// </summary>
        /// <summary>The seconds the reaction of <paramref name="pair"/> is held in the body by the marks in force (0: not held), and the strength it lands at (The Delayed Touch).</summary>
        public float ReactionHold(int pair, out float strength)
        {
            strength = 1f;
            if (byTrigger == null || pair < 0) return 0f;
            return ReactionRules.Hold(byTrigger[(int)UpgradeTrigger.Reacted], pair, out strength);
        }

        public float ReactionBoost(Component who, int pair, out bool spares)
        {
            spares = false;
            var mark = ReactionTable.MarkFor(pair);
            if (mark == null || byTrigger == null || !(who is IAfflictable afflicted)) return 1f;
            return ReactionRules.Boost(byTrigger[(int)UpgradeTrigger.Reacted], mark.Id, afflicted.Afflictions, out spares);
        }

        /// <summary>While true, the effects put in the line are a reaction's blows struck again (PlayerReactions' echo).</summary>
        public void StrikingAgain(bool on) => strikingAgain = on;
        private bool strikingAgain;

        /// <summary>While a reaction struck again is put in the line: its damage is this share of the first time's.</summary>
        public void ScaleReaction(float share) => ripeScale *= Mathf.Max(0f, share);

        /// <summary>True when the reaction's pair has <paramref name="kind"/> in it, or none is asked.</summary>
        public static bool ReactionHas(int pair, AfflictionKind kind) =>
            kind == AfflictionKind.None || (pair >= 0 && pair < AfflictionPairs.Count && (AfflictionPairs.First(pair) == kind || AfflictionPairs.Second(pair) == kind));

        /// <summary>
        /// Remembers which enemies each going-off of a reaction has struck, so each is answered once however many of the
        /// reaction's blows reach it. Fixed size: the oldest are forgotten first.
        /// </summary>
        public sealed class StruckOnce
        {
            private const int Size = 48;
            private readonly int[] casts = new int[Size];
            private readonly object[] struck = new object[Size];
            private int next;

            /// <summary>True the first time <paramref name="who"/> is told for this going-off; false after.</summary>
            public bool First(int cast, object who)
            {
                for (int i = 0; i < Size; i++)
                    if (casts[i] == cast && ReferenceEquals(struck[i], who)) return false;
                casts[next] = cast;
                struck[next] = who;
                next = (next + 1) % Size;
                return true;
            }
        }

        // A ripe seed burst on this enemy: whatever answers that (In Its Season's thornbush) is told.
        private void RipeBurstAt(Component who, Vector3 at, int seeds)
        {
            RipeBursts++;
            Trigger(UpgradeTrigger.RipeBurst, who, at, true, Mathf.Max(1, seeds));
        }

        // Shared Roots: the share of a wound the Seeded enemies within reach carry, split among them. Returns what they
        // carry: the whole share when one is near, nothing when none is. They are struck on this component's next step.
        private float ShareWound(float share)
        {
            if (combat == null || share <= 0f) return 0f;
            if (sharerCount > 0)
            {
                // A second wound before the first share is paid: the same Seeded carry it too.
                sharedEach += share / sharerCount;
                WoundsCarried += share;
                return share;
            }
            furnaceSeen.Clear();
            for (int looked = 0; sharerCount < MostSharers && looked < 24; looked++)
            {
                var near = combat.EnemyNear(transform.position, SharedReach, furnaceSeen);
                if (!(near is IDamageable one)) break;
                furnaceSeen.Add(one);
                if (near is IAfflictable carrier && carrier.Afflictions.Has(AfflictionKind.Seeded)) sharers[sharerCount++] = near;
            }
            furnaceSeen.Clear();
            if (sharerCount == 0) return 0f;
            sharedEach = share / sharerCount;
            WoundsCarried += share;
            return share;
        }

        private void PayShares()
        {
            var life = lookOf[(int)AfflictionKind.Seeded];
            Vector3 heel = transform.position + Vector3.up * 0.1f;
            for (int i = 0; i < sharerCount; i++)
            {
                var body = sharers[i];
                sharers[i] = null;
                if (body == null || !body.gameObject.activeInHierarchy || combat == null) continue;
                var blow = Blow(life, AfflictionKind.None, sharedEach, 0f, 0);
                combat.StrikeOne(body, blow, transform.position);
                // The pulse runs down the roots from your heels to it.
                Vector3 to = body.transform.position - heel;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f) ElementLook.Sparks(life, heel + to * 0.5f, to.normalized, 1f);
                ElementLook.Puff(life, body.transform.position + Vector3.up * 1.2f, Vector3.up, 0.7f);
            }
            sharerCount = 0;
        }

        /// <summary>A blow landed on the player; the attacker stands at the given place.</summary>
        public void OnStruck(Vector3 attacker)
        {
            if (hasWhiteHeat && hasHeat && Heat.Level > baseHeatLevels)
            {
                selfBurnLeft = WhiteHeatBurnSeconds;
                if (defence != null) defence.Callout("Burning");
            }
            Heat.Struck();
            sinceStruck = 0f;
            Moments.Note(CastMoment.Struck, Time.time);
            // Fed Flame: each blow taken smothers the burning surge.
            var smothered = element != null && element.SurgeActive ? element.SurgeKit : null;
            if (smothered != null && smothered.SurgeSmothers > 0f) smothered.Smother(smothered.SurgeSmothers);
            var struckBy = combat != null ? combat.EnemyAt(attacker, 2.5f) : null;
            if (struckBy != null)
            {
                this.struckBy = struckBy;
                struckByAt = Time.time;
            }
            Trigger(UpgradeTrigger.Struck, struckBy, attacker);
            TheirBlow(struckBy, attacker);
        }

        /// <summary>The element's surge was lit.</summary>
        public void OnSurge()
        {
            Stats.ClearSurge();
            if (element != null)
                foreach (var mod in element.SurgeKit.SurgeMods) Stats.AddSurge(mod);
            if (hasHeat) Heat.Fill();
            if (hasOverclock)
            {
                WorldClock.SlowEnemies(OverclockSeconds, OverclockScale);
                ScreenFlash.Flash(new Color(0.6f, 0.85f, 1f, 0.3f), 0.08f);
            }
            Trigger(UpgradeTrigger.SurgeStart, null, transform.position, false);
        }

        /// <summary>The surge has run out, or the element was changed under it.</summary>
        public void OnSurgeEnd()
        {
            Stats.ClearSurge();
            Trigger(UpgradeTrigger.SurgeEnd, null, transform.position, false);
        }

        /// <summary>A flask was drunk and has healed.</summary>
        public void OnFlaskDrunk()
        {
            if (hasWell)
            {
                well = Mathf.Min(WellMost, well + WellPerFlask * Stats.Multiplier(PlayerStat.FlaskHeal));
                ShowWell();
            }
            Trigger(UpgradeTrigger.FlaskDrunk, null, transform.position, false);
            // The Flask Breaks: while Wellspring lasts, the flask bursts out as a ring of water 4 m across.
            var flowing = element != null && element.SurgeActive ? element.SurgeKit : null;
            if (flowing != null && flowing.SurgeFlaskBreaks && combat != null) FlaskBreaks();
        }

        /// <summary>The Flask Breaks' ring: 4 m across, 20 damage, 30 stagger and Soaked, thrown 2.5 m, and 4 s off every running wait.</summary>
        public const float FlaskRingRadius = 2f, FlaskRingDamage = 20f, FlaskRingStagger = 30f, FlaskRingThrow = 2.5f, FlaskRingCut = 4f;
        /// <summary>Surges lit by a mark at no cost (Sky Dance's Master), counted for probes.</summary>
        public int SurgesGiven { get; private set; }

        /// <summary>A dance's cuts are done, every one in time (Sky Dance): its branch's marks that ask for that answer.</summary>
        public void DancedInTime() => Trigger(UpgradeTrigger.DanceInTime, null, transform.position, false);

        /// <summary>True when a dance of <paramref name="cuts"/> rings had every one met in time.</summary>
        public static bool AllInTime(int inTime, int cuts) => cuts > 0 && inTime >= cuts;

        /// <summary>Rings The Flask Breaks has thrown, counted for probes.</summary>
        public int FlaskRings { get; private set; }
        /// <summary>Seconds of surge fed by blows on burning enemies (Fed Flame), counted for probes.</summary>
        public int SurgeFed { get; private set; }

        private void FlaskBreaks()
        {
            Vector3 at = transform.position;
            var water = lookOf[(int)AfflictionKind.Soaked] ?? Elements.ByName(Elements.Water);
            combat.StrikeArea(at, FlaskRingRadius, Blow(water, AfflictionKind.Soaked, FlaskRingDamage, FlaskRingStagger, 1));
            combat.Displace(at, FlaskRingRadius + 0.5f, FlaskRingThrow, false);
            if (spells != null) spells.Cooldowns.Cut(FlaskRingCut);
            ElementLook.Ring(water, at + Vector3.up * 0.2f);
            ElementLook.Puff(water, at + Vector3.up * 0.4f, Vector3.up, 1.2f);
            FlaskRings++;
        }

        // The well is shown as the count its mark keeps, in whole points.
        private void ShowWell()
        {
            if (wellIndex < 0 || wellIndex >= tally.Length) return;
            int shown = Mathf.CeilToInt(well);
            if (tally[wellIndex] == shown) return;
            tally[wellIndex] = shown;
            ApplyTallies();
        }

        /// <summary>A flask left the hand instead of being drunk.</summary>
        public void OnFlaskThrown()
        {
            FlasksThrown++;
            Transform enemy = combat.EnemyAhead(FirebombRange);
            Vector3 at = enemy != null ? enemy.position : transform.position + transform.forward * 5f;
            Vector3 toward = at - transform.position;
            toward.y = 0f;
            if (toward.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
            Vfx.Play(VfxKind.Glint, transform.position + Vector3.up * 1.8f);
            const float flight = 0.4f;
            ThrownThing.Throw(transform.position + Vector3.up * 1.6f + transform.forward * 0.3f, at + Vector3.up * 0.2f, flight, 2.2f);
            if (hasFirebomb)
            {
                Hazards.Lay(HazardKind.Fire, at, FirebombRadius, 0.12f, FirebombDamage, FirebombStagger, 4f, 0.1f, true, flight, default, true);
                Hazards.Lay(HazardKind.Fire, at, FirebombRadius * 0.85f, 3.5f, 5f, 0f, 2f, 0.5f, false, flight + 0.15f);
            }
            Trigger(UpgradeTrigger.FlaskThrown, enemy != null ? enemy.GetComponentInParent<IDamageable>() as Component : null, at);
        }

        /// <summary>Casts the spell in the North slot, whatever its clock says: for older probes. Returns what <see cref="Cast"/> does.</summary>
        public bool TrySpecial() => Worn != null && Cast(Worn);

        /// <summary>The mana a spell asks now: its price (larger spells only) as the marks make it.</summary>
        /// <summary>The share of a full hold the spell being loosed now was held for (SpellSpec.HoldUpTo); set by the slots round the cast.</summary>
        public float HeldShare { get; set; }

        /// <summary>How long the spell of this mark may be held on its button as it is cast now; 0 for a spell loosed at once.</summary>
        public float HoldUpToOf(Upgrade mark)
        {
            var spec = mark != null ? branches.SpecOf(mark, RunState.Unlocked, ReadLoadout()) : null;
            return spec != null ? spec.HoldUpTo : 0f;
        }

        public float PriceOf(Upgrade mark)
        {
            // The branch's Empower may make a larger spell cheaper (Meteor Leap 45 to 35); one paid in flasks asks no mana.
            var spec = mark != null ? branches.SpecOf(mark, RunState.Unlocked, ReadLoadout()) : null;
            return spec == null || spec.FlaskCost > 0 ? 0f : spec.Cost * Stats.Multiplier(PlayerStat.SpellCost);
        }

        /// <summary>The family a grown thing is counted in for its limit: its own effect's, or one shared by kin of the same mark (UpgradeEffect.Kin).</summary>
        public static int FamilyOf(UpgradeEffect effect) =>
            effect == null ? 0 : effect.Kin > 0 && effect.Owner != null ? 1000000 + effect.Owner.Index * 8 + effect.Kin : effect.Slot + 1;

        // When each branch's own spell was last cast, by its Learn's place in the catalogue (UpgradeEffect.WithinOwnCast).
        private float[] castAt = new float[0];

        // The mark whose count an effect's Spends pays from: its own, or its branch's Learn's (UpgradeEffect.PaysLearn).
        private static Upgrade PayerOf(UpgradeEffect effect)
        {
            var owner = effect.Owner;
            if (effect.PaysLearn && owner != null && !string.IsNullOrEmpty(owner.Branch)) return Upgrade.ById(owner.Branch) ?? owner;
            return owner;
        }

        // When the branch of this mark last cast its own spell; long ago when never.
        private float LastCastOf(Upgrade owner)
        {
            if (owner == null) return -100f;
            var learn = !string.IsNullOrEmpty(owner.Branch) ? Upgrade.ById(owner.Branch) ?? owner : owner;
            return learn.Index >= 0 && learn.Index < castAt.Length ? castAt[learn.Index] : -100f;
        }

        /// <summary>True when <paramref name="now"/> is within <paramref name="window"/> seconds of a cast made at <paramref name="castAt"/>.</summary>
        public static bool InCastWindow(float castAt, float now, float window) => window > 0f && now >= castAt && now - castAt <= window;

        /// <summary>True when the player can pay a cast's flasks: enough on the belt, and none at the lips.</summary>
        public static bool CanPayFlasks(int cost, int held, bool drinking) => cost <= 0 || (!drinking && held >= cost);

        /// <summary>
        /// Casts a learned (or lent) spell from a slot, through its form: the one path every slot spell takes
        /// (IMPLEMENTATION.md 3.1). Basic spells cost no mana (their slot's clock is their price, kept by
        /// <see cref="PlayerSpells"/>); a larger spell pays its mana; a spell may also ask health. False when it could
        /// not be cast now (no mana, no health to give, or its form can do nothing here).
        /// </summary>
        public bool Cast(Upgrade mark)
        {
            if (mark == null) return false;
            // The spell as its branch makes it now: the shape worn (its form and numbers) and the Empower.
            var loadout = ReadLoadout();
            var spec = branches.SpecOf(mark, RunState.Unlocked, loadout);
            if (spec == null) return false;
            // A spell held on its button is loosed with what the hold gathered (SpellSpec.HoldUpTo).
            if (spec.HoldUpTo > 0f) SpellSpec.ApplyHold(spec, HeldShare);
            var form = SpellForms.Get(SpellBranches.FormOf(mark, RunState.Unlocked, loadout));
            if (form == null) return false;
            // A spell may ask health in place of mana, or as well: it is left grey, to be won back.
            float blood = SpellSpec.HealthPrice(spec.HealthCost, spec.HealthShare, defence != null ? defence.Health.Current : 0f) * Stats.Multiplier(PlayerStat.HealthPrices);
            // Cast in its Master's moment, it lands with no stillness after it (UpgradeAction.Quicken: Seven Flashes).
            if (Quickens(byTrigger[(int)UpgradeTrigger.SpellCast], mark, Moments, Time.time, GrownWithin))
            {
                spec.Delay = 0f;
                Quickened++;
            }
            var cast = element.NewCast(spec, mark.Name, mark);
            if (spec.StaysPut && defence != null) shadowHealth = defence.Health.Current;
            // A form that can do nothing just now says so before anything is paid.
            if (form is ISpellGate gate && !gate.CanCast(cast)) return false;
            float price = PriceOf(mark);
            // A spell paid in flasks (Poured on the Edge): enough on the belt, or it is not cast.
            if (spec.FlaskCost > 0 && (defence == null || !CanPayFlasks(spec.FlaskCost, defence.Flasks.Count, defence.Flasks.IsDrinking))) return false;
            CoverWithHealth(price);
            if (blood > 0f && (defence == null || defence.Health.Current <= blood)) return false;
            // Spring Tide: the spell is made greater by all the mana held before its numbers are read, and takes it all.
            // Only a spell with a price can rise: basic spells cost none now (the mark is inert on them).
            bool tide = TideRises(price);
            if (tide) cast = element.NewCast(spec, mark.Name, mark);
            if (!combat.Mana.TrySpend(price)) return false;
            if (tide) TideSpent();
            if (blood > 0f) defence.Health.Chip(blood);
            for (int i = 0; i < spec.FlaskCost; i++) defence.Flasks.TryThrow();
            if (mark.Index >= 0 && mark.Index < castAt.Length) castAt[mark.Index] = Time.time;
            WornCasts++;
            if (spec.Rings > 0 && spec.RingsBy == RingSource.Stopped)
            {
                // The shape's own wall stands, and each blow it stops gathers a ring into the Learn's spell, loosed through the
                // Learn's form by a second press or when the wall falls (Red Before the Sun).
                var gathered = branches.PlainSpecOf(mark, RunState.Unlocked, plainGathered);
                if (gathered != null)
                {
                    gathered.Rings = spec.Rings;
                    gathered.RingsFor = spec.RingsFor;
                    gathered.RingsBy = RingSource.Stopped;
                    rings.Begin(mark, gathered, cast.Cast, Time.time + Mathf.Max(0.15f, spec.Delay) + Mathf.Max(0.5f, spec.Seconds), mark.Form);
                    gatheringWall = form as Forms.RunningForm;
                }
            }
            else if (spec.Rings > 0)
            {
                // Begun, not loosed: the blows landed from now each add a ring, and a second press (or the press window's end)
                // looses it with what it gathered (LooseRings).
                PressesOf(mark, out float window);
                rings.Begin(mark, spec, cast.Cast, Time.time + (window > 0f ? window : 8f));
                if (element != null) ElementLook.Sparks(OwnElement(mark), transform.position + Vector3.up * 1.2f, Vector3.up, 0.5f);
                castSource = mark;
                castOrigin = transform.position;
                castFrom = transform.position;
                BeginCast();
                answeringCast = cast.Cast;
                try { Trigger(UpgradeTrigger.SpellCast, null, transform.position, false); }
                finally { answeringCast = 0; }
                castSource = null;
                return true;
            }
            if (form is ISpellLanding landing)
            {
                // The form throws the player into the air; it is told again when they come down.
                Leaps++;
                landingForm = landing;
                landingCast = cast;
            }
            else if (cast.Target != null)
            {
                // Thrown at the enemy faced, as the element's own spell is.
                Vector3 toward = cast.Target.position - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.0025f)
                {
                    transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
                    cast.Forward = transform.forward;
                }
            }
            if (spec.Ebb > 0f)
            {
                // The ebb: drawn straight back from the enemy faced in a moment, untouchable, and the form waits for the flood.
                ebbMark = mark;
                SpellChange.Copy(spec, ebbSpec);
                ebbFrom = transform.position;
                ebbTag = cast.Cast;
                PressesOf(mark, out float ebbWindow);
                ebbUntil = Time.time + (ebbWindow > 0f ? ebbWindow : 3f);
                castSource = mark;
                castOrigin = transform.position;
                castFrom = transform.position;
                BeginCast();
                if (defence != null) defence.UntouchableUntil = Mathf.Max(defence.UntouchableUntil, Time.time + 0.3f);
                if (afterimage != null) afterimage.Leave(0.35f);
                Vector3 back = -cast.Forward;
                back.y = 0f;
                ElementLook.Sparks(OwnElement(mark), transform.position + Vector3.up, back, 1f);
                player.StepTo(transform.position + back.normalized * spec.Ebb);
                // NEEDS: the line of wet sand it leaves as ground (ground-states); now only its look.
                Forms.FormFx.Run(Forms.FormMark.Lane, ebbFrom, back.normalized, spec.Ebb, 1.2f, Forms.FormKit.Stain(cast.Colour, 0.4f), 3f);
                Ebbs++;
                answeringCast = cast.Cast;
                try { Trigger(UpgradeTrigger.SpellCast, null, transform.position, false); }
                finally { answeringCast = 0; }
                castSource = null;
                return true;
            }
            castSource = mark;
            castOrigin = transform.position;
            castFrom = transform.position;
            BeginCast();
            EchoLater(form, cast);
            form.Cast(cast);
            castSource = mark;
            // Its own moment knows its number, for what only this cast is given (UpgradeEffect.ThisCast, ShareStore).
            answeringCast = cast.Cast;
            try { Trigger(UpgradeTrigger.SpellCast, null, transform.position, false); }
            finally { answeringCast = 0; }
            castSource = null;
            return true;
        }

        // The Learn's spell a wall gathers into (Red Before the Sun), and the wall's form, ended when the rings are loosed.
        private readonly SpellSpec plainGathered = new SpellSpec();
        private Forms.RunningForm gatheringWall;

        /// <summary>A blow was stopped by a spell's wall standing between it and the player: a ring for a spell that gathers them so.</summary>
        public void OnStopped()
        {
            if (rings.Active && rings.By == RingSource.Stopped) AddRing();
        }

        // One more ring to the spell gathering (a blow landed, a blow turned): shown, and told to its own branch's marks.
        private void AddRing()
        {
            if (!rings.Add(Time.time)) return;
            var mark = rings.Mark;
            if (element != null) ElementLook.Sparks(OwnElement(mark), transform.position + Vector3.up * 1.2f, Vector3.up, 0.3f);
            castSource = mark;
            try { Trigger(UpgradeTrigger.RingAdded, null, transform.position, false, rings.Rings); }
            finally { castSource = null; }
        }

        // Looses an ebb's flood: its form cast from here back along the line toward where the ebb began and on past it.
        private void LooseFlood()
        {
            var mark = ebbMark;
            ebbMark = null;
            if (mark == null || element == null || combat == null) return;
            var form = SpellForms.Get(SpellBranches.FormOf(mark, RunState.Unlocked, ReadLoadout()));
            if (form == null) return;
            SpellChange.Copy(ebbSpec, loosedSpec);
            loosedSpec.Ebb = 0f;
            loosedSpec.Reach = FloodReach(transform.position, ebbFrom, ebbSpec.Reach, ebbSpec.Ebb);
            Vector3 way = ebbFrom - transform.position;
            way.y = 0f;
            if (way.sqrMagnitude > 0.09f) transform.rotation = Quaternion.LookRotation(way.normalized, Vector3.up);
            var cast = element.NewCast(loosedSpec, mark.Name, mark);
            cast.Cast = ebbTag;
            cast.Forward = transform.forward;
            castSource = mark;
            answeringCast = ebbTag;
            try { Trigger(UpgradeTrigger.SpellLoosed, null, transform.position, false); }
            finally { answeringCast = 0; }
            castSource = mark;
            castOrigin = transform.position;
            form.Cast(cast);
            castSource = null;
            Floods++;
        }

        // Looses the gathered spell now with the rings it holds, facing the enemy faced: its own branch's SpellLoosed moment is
        // told first, so what that moment gives the cast (Daybreak's Master: critical) is held before its blows land.
        private void LooseRings()
        {
            var mark = rings.Mark;
            int tag = rings.Tag;
            int gathered = rings.Rings;
            if (mark == null || element == null || combat == null)
            {
                rings.End();
                return;
            }
            string loosedAs = rings.Form;
            rings.Loose(loosedSpec);
            // The shield comes down as its rings are loosed (Red Before the Sun).
            if (loosedAs != null && gatheringWall != null) Forms.SpellWorks.EndRunsOf(gatheringWall);
            gatheringWall = null;
            var form = SpellForms.Get(loosedAs ?? SpellBranches.FormOf(mark, RunState.Unlocked, ReadLoadout()));
            if (form == null) return;
            var cast = element.NewCast(loosedSpec, mark.Name, mark);
            cast.Cast = tag;
            if (cast.Target != null)
            {
                Vector3 toward = cast.Target.position - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.0025f)
                {
                    transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
                    cast.Forward = transform.forward;
                }
            }
            castSource = mark;
            answeringCast = tag;
            try { Trigger(UpgradeTrigger.SpellLoosed, null, transform.position, false); }
            finally { answeringCast = 0; }
            castSource = mark;
            form.Cast(cast);
            castSource = null;
            RingsLoosed++;
            LastRingsLoosed = gathered;
        }

        /// <summary>
        /// With a mark that sets a blood price, mana a spell lacks is paid in health: the health is left grey, to be
        /// won back, and never the last point is taken. Does nothing when the mana is there or no such mark is learned.
        /// </summary>
        public void CoverWithHealth(float cost)
        {
            float rate = Stats.Sum(PlayerStat.BloodPrice);
            if (rate <= 0f || combat == null || defence == null || defence.IsDead || combat.Mana.Sealed) return;
            float missing = cost - combat.Mana.Current;
            if (missing <= 0f) return;
            float price = missing * rate * Stats.Multiplier(PlayerStat.HealthPrices);
            if (defence.Health.Current - 1f < price) return;
            defence.Health.Chip(price);
            combat.Mana.Gain(missing + 0.001f);
            HealthPaid += price;
        }

        /// <summary>Heartwood: a price in mana is paid in health, left grey to be won back. Never the last point: then it is not paid.</summary>
        private bool PayInHealth(float cost)
        {
            if (defence == null || defence.IsDead) return false;
            float price = cost * HeartwoodPrice * Stats.Multiplier(PlayerStat.HealthPrices);
            if (defence.Health.Current - 1f < price) return false;
            defence.Health.Chip(price);
            HealthPaid += price;
            return true;
        }

        /// <summary>Health paid in place of mana so far, for tests and probes.</summary>
        public float HealthPaid { get; private set; }
        /// <summary>Casts made with no stillness by their Master's moment (UpgradeAction.Quicken), for probes.</summary>
        public int Quickened { get; private set; }

        /// <summary>A ward turned a blow and is gone.</summary>
        public void OnWardSpent() => Trigger(UpgradeTrigger.WardSpent, null, transform.position, false);

        /// <summary>Conditions paid out early (UpgradeAction.PayNow): for probes.</summary>
        public int PaidNow { get; private set; }

        /// <summary>A plate of stone took its share of a blow and broke (Stone Skin).</summary>
        public void OnPlateBroken() => Trigger(UpgradeTrigger.PlateBroken, null, transform.position, false);

        /// <summary>The leap has come down.</summary>
        public void OnLeapLanded()
        {
            if (landingForm == null) return;
            var form = landingForm;
            landingForm = null;
            form.Landed(landingCast);
        }

        /// <summary>The jolt of something heavy landing: a stop of time and a shake of the camera. For spell forms.</summary>
        /// <summary>Everything slowed for a while, the player too: the breath before something great lands. For spell forms.</summary>
        public void Slow(float seconds, float scale)
        {
            if (hitStop != null && seconds > 0f) hitStop.Slow(seconds, scale);
        }

        /// <summary>Forms begun by marks (<see cref="UpgradeAction.Form"/>), for tests and probes.</summary>
        public int FormsBegun { get; private set; }
        /// <summary>Dodges begun, and where the last one went; for forms that answer a dodge.</summary>
        public int DodgeCount { get; private set; }
        public Vector3 LastDodgeFrom { get; private set; }
        public Vector3 LastDodgeDirection { get; private set; }

        /// <summary>
        /// The surge button pressed again while the surge lasts. A form that waits on it says so every frame it waits
        /// (<see cref="ListenForSurge"/>); while one does, the press is its own and does not light the surge again.
        /// </summary>
        public int SurgeAgain { get; private set; }
        public bool ListensForSurge => Time.frameCount - listenedAt <= 2;
        public void ListenForSurge() => listenedAt = Time.frameCount;
        public void HeardSurge() => SurgeAgain++;
        private int listenedAt = -100;
        private static readonly SpellSpec plainSpell = new SpellSpec { Cost = 0f };

        // ---- The Rise.

        /// <summary>True while the player is Risen: stood up again from a death, their health draining.</summary>
        public bool Risen
        {
            get => risen || deadAlready;
            private set => risen = value;
        }
        /// <summary>True only while the Rise itself lasts (not for one who is Dead Already).</summary>
        public bool RisenNow => risen;
        private float RiseDrainShare => Has(Trees.DarkTree.SlowRot) ? 0.7f : 1f;
        /// <summary>How fast the Risen drain, as a share of the usual.</summary>
        public float RiseDrain => Time.time < riseStilledUntil ? 0f : RiseDrainShare;
        /// <summary>The share of health the Risen stand up with.</summary>
        public float RiseHealth = 0.5f;
        private bool riseSpent;
        private static readonly SpellSpec riseSpell = new SpellSpec { Cost = 0f, Damage = 4f, Count = 50 };

        /// <summary>
        /// The blow that would have killed: with Dark's root, Curse (the owner's decision of 2026-11-20, rise-on-root), and
        /// once until a finisher on the killer buys it back (that is the gift's, A Life for a Life), the player stands
        /// again instead, Risen. True when they rose.
        /// </summary>
        public bool Rise(Vector3 attacker)
        {
            if (Risen || deadAlready || riseSpent || defence == null || element == null || combat == null || !Has(Trees.DarkTree.Curse)) return false;
            var form = SpellForms.Get(SpellForms.Rise);
            if (form == null) return false;
            riseSpent = true;
            Risen = true;
            defence.Health.Rise(defence.Health.Maximum * RiseHealth);
            var killer = combat.EnemyAt(attacker, 3f);
            // The cast names the root: the Rise lasts while the root is held, and ends with it.
            var cast = element.NewCast(riseSpell, "Risen", Upgrade.ById(Trees.DarkTree.Curse));
            cast.Target = killer != null ? killer.transform : null;
            if (killer != null) cast.Aim = killer.transform.position;
            form.Cast(cast);
            return true;
        }

        /// <summary>The Rise is over: bought back with a finisher (health returns, and the player may rise again), or lost.</summary>
        public void EndRise(bool lived, float share)
        {
            if (!risen) return;
            Risen = false;
            if (!lived || defence == null) return;
            riseSpent = false;
            defence.Health.Heal(defence.Health.Maximum * share);
            defence.Callout("A life for a life");
        }

        // Standing up after a real death, the rise is theirs to use again.
        private void OnRevived()
        {
            Risen = false;
            riseSpent = false;
        }

        public void Jolt(float stopSeconds, float shake, float shakeSeconds)
        {
            if (hitStop != null && stopSeconds > 0f) hitStop.Freeze(stopSeconds);
            if (shaker != null && shake > 0f) shaker.Shake(shake, shakeSeconds);
        }
    }
}
