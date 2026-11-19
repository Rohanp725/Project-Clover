using Clover.Core;
using Clover.Player;
using Clover.World;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The god of Lightning in his godly form: a storm dragon, a long serpent winding in and out of the cloud all
    /// round the summit. Its arches rise and sink through the cloud line the whole fight; its head rears over the
    /// north rim on its neck, two small forefeet grip the rim beside it, and its tail comes onto the floor from the
    /// side. It fights mostly with its mouth (a beam that crosses the floor, forking breath, a bite), with marked
    /// magic (bolts in patterns, a ring of sigils that closes, drifting orbs, a storm over quarters of the floor), and
    /// with its tail (a low sweep and a slam). It is hurt on the head while that lies on the floor after a bite or a
    /// beam, on the tail while that lies after a slam, and a little on the forefeet at any time; blows and perfect
    /// guards wear down its balance, and when that breaks the head falls and can be finished. The pieces are rigid
    /// models moved by this code. What it does next is decided by a <see cref="DragonChooser"/>.
    /// </summary>
    public class StormDragon : GodlyForm
    {
        private enum Mode { Hidden, Rising, Idle, Beam, Breath, Bite, Bolts, Sigils, Orbs, Storm, TailSweep, TailSlam, TailCoil, TailScythe, TailFlops, TailSting, Staggered, Recovering, Dying, Dead }

        [SerializeField] private Transform target;
        [SerializeField] private Transform head;
        [SerializeField] private Transform jaw;
        [SerializeField] private Transform body;
        [SerializeField] private Transform leftClaw;
        [SerializeField] private Transform rightClaw;
        [SerializeField] private Transform shortTail;
        [SerializeField] private Transform tail;
        [SerializeField] private SpineBones tailBones;
        [SerializeField] private SpineBones[] lengths = new SpineBones[0];
        [SerializeField] private ColossusPart headPart;
        [SerializeField] private ColossusPart tailPart;
        [SerializeField] private ColossusPart leftClawPart;
        [SerializeField] private ColossusPart rightClawPart;
        [SerializeField] private Renderer[] renderers = new Renderer[0];
        [SerializeField] private MeshFilter[] beamMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] breathMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] biteMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] fallMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] orbMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] stormMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] tailSweepMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] tailSlamMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter roarRing;
        [SerializeField] private Transform[] orbs = new Transform[0];
        [SerializeField] private LineRenderer[] beam = new LineRenderer[0];
        [SerializeField] private Renderer[] glowRenderers = new Renderer[0];
        [SerializeField] private int[] glowMaterials = new int[0];
        [SerializeField] private bool[] glowIsEye = new bool[0];
        [SerializeField] private StaggerMark staggerMark;

        [Header("The place")]
        /// <summary>Radius of the round floor, whose middle is the origin of the world; the head rears beyond its north edge.</summary>
        public float FloorRadius = 17f;
        public float HeadScale = 0.85f;
        /// <summary>How high the chin hangs over the floor at rest, and how far the snout is tipped down at the player.</summary>
        public float HeadHeight = 1.7f;
        public float HeadPitch = 14f;
        /// <summary>How far inside the north rim the chin hangs: he leans in through the gate, under its beam, so the head is seen from above.</summary>
        public float HeadInset = 2.8f;
        /// <summary>Degrees the jaw hangs open at rest, and wide. The sign is the way the model opens.</summary>
        public float JawSnarl = 13f;
        public float JawWide = 40f;
        public float JawSign = 1f;
        /// <summary>Degrees either side of north the forefeet grip the rim, and how far inside it.</summary>
        public float ClawAngle = 25.7f;
        public float ClawInset = 1.7f;

        [Header("The body in the cloud")]
        /// <summary>The top of the summit's sea of cloud: whatever of him is under it is not seen.</summary>
        public float CloudLine = -3f;
        /// <summary>The lengths of body round the summit. How far out each ring of them goes round (one near behind the rail, the others further off; far enough apart that two bodies never touch), and the metres a second the body on each travels.</summary>
        public float[] LengthRings = { 20.8f, 24.8f, 28.8f };
        public float[] LengthSpeeds = { 5.2f, 4.1f, 6f };
        /// <summary>How deep the middle of the body runs where it is not seen: its fins stand 2.4 m above that and have to be under the cloud.</summary>
        public float LengthDeep = -6.2f;
        /// <summary>The loops a length may make as it comes round: how high the middle of the body stands at the top of each, and over how many metres from either end it climbs to that. The fourth is a long low stretch of back rolling along the top of the cloud.</summary>
        public float[] LoopPeaks = { 6f, 3.4f, 4.8f, -0.9f, 7f };
        public float[] LoopRamps = { 15f, 13f, 14f, 9.5f, 15f };
        /// <summary>Degrees from north between which the body comes out of the cloud: not behind the gate, where the neck is, nor round the south, where it would stand between the camera and the floor.</summary>
        public float LengthFromNorth = 10f;
        public float LengthToNorth = 140f;
        /// <summary>Degrees either side of where the tail comes over the rim within which the body stays under, so the two never cross.</summary>
        public float LengthTailRoom = 16f;
        /// <summary>How low the lines of light down the body are turned while he shows what he is about to do.</summary>
        public float TellingBodyGlow = 0.3f;
        /// <summary>How many times faster the body winds in its fury.</summary>
        public float FuryWinding = 1.6f;

        [Header("Itself")]
        public float MaximumHealth = 760f;
        public float BalanceMaximum = 170f;
        public float StaggeredSeconds = 5.5f;
        /// <summary>What a forefoot lets through of a blow; what the tail does while it lies; the head while it lies stuck or spent; and the broken head.</summary>
        public float ClawDamage = 0.35f;
        public float TailDamage = 1f;
        public float OpenHeadDamage = 1f;
        public float HeadDamage = 1.6f;
        /// <summary>A finisher on the fallen head takes at least this share of all its health.</summary>
        public float FinisherShare = 0.22f;
        public float PerfectGuardBalance = 60f;
        public float RestBetween = 0.9f;
        /// <summary>The longer rest it owes the player after two heavy attacks: time to breathe and to drink.</summary>
        public float LullSeconds = 2.4f;
        public DragonChooser Chooser = new DragonChooser();
        /// <summary>The next attack when something outside has decided it (a probe, a scripted moment); None leaves it to the chooser.</summary>
        public DragonAttack Queued = DragonAttack.None;

        [Header("Where the player stands")]
        /// <summary>Within this many metres of the middle of the north rim the player is within reach of the head.</summary>
        public float BiteReach = 10f;
        /// <summary>Seconds the player must have stood there before it bites for that alone.</summary>
        public float BiteAfterSeconds = 1.2f;

        [Header("The entrance")]
        /// <summary>All of it, from the first arch breaking the cloud to the first attack. Nothing in it hurts, and nothing can hurt him.</summary>
        public float RiseSeconds = 4.2f;
        public float EntranceLeftClawAt = 1f;
        public float EntranceRightClawAt = 1.7f;
        public float EntranceRoarAt = 3.2f;
        public float EntranceRear = 0.45f;
        public float RoarSeconds = 0.7f;
        public float RoarRingSeconds = 0.8f;
        public float EntranceBoltEvery = 0.3f;
        public float RiseDepth = 16f;

        [Header("Fury")]
        /// <summary>Below this share of health he is in fury: faster, the beam comes back the way it went, a fourth wave of bolts, resting less, and it opens with the storm.</summary>
        public float FuryAt = 0.5f;
        public float FuryPace = 1.25f;
        public float FuryRest = 0.55f;
        public float FuryEyeGlow = 2.6f;
        public float FuryMarkGlow = 1.5f;
        public float FuryRimBoltEvery = 0.9f;

        [Header("The beam")]
        public float BeamTelegraph = 1.5f;
        /// <summary>Seconds the beam takes to cross the floor once.</summary>
        public float BeamSeconds = 2.1f;
        /// <summary>Degrees either side of south, from under his mouth, that the beam starts and ends at.</summary>
        public float BeamArc = 66f;
        public float BeamWidth = 3f;
        public float BeamDamage = 24f;
        /// <summary>Seconds after a touch of the beam before it can touch again: one crossing is one blow.</summary>
        public float BeamTick = 0.7f;
        /// <summary>Seconds the head lies spent on the floor afterwards, open to blows.</summary>
        public float BeamSpent = 2.2f;
        /// <summary>How many times brighter the eyes and the marks along the whole body burn as the beam gathers.</summary>
        public float BeamGlow = 5f;
        public float BeamCoreWidth = 0.9f;

        [Header("The breath")]
        public float BreathTelegraph = 1.2f;
        public float BreathLock = 0.35f;
        /// <summary>Seconds the forks stand on the floor. They strike once, as they come.</summary>
        public float BreathSeconds = 0.45f;
        /// <summary>Degrees between one fork and the next.</summary>
        public float BreathFork = 27f;
        public float BreathWidth = 2.8f;
        public float BreathDamage = 18f;
        public float BreathRest = 0.8f;

        [Header("The bite")]
        public float BiteTelegraph = 1.1f;
        public float BiteLock = 0.3f;
        public float BiteDrop = 0.13f;
        public float BiteRadius = 3.6f;
        public float BiteDamage = 40f;
        /// <summary>Seconds the head lies with its jaws in the floor afterwards, open to blows.</summary>
        public float BiteStuck = 1.8f;
        /// <summary>How far behind where the player stood, toward the rim, the chin comes down: the snout reaches the rest of the way.</summary>
        public float BiteBack = 2.4f;
        /// <summary>How much of the neck's own length the head may be from its root before the body rears forward after it, and how far forward the body may come.</summary>
        public float NeckReach = 0.84f;
        public float NeckLeanLimit = 7f;

        [Header("The bolts")]
        public float BoltWaveEvery = 0.85f;
        public float BoltWarning = 1f;
        public float BoltRadius = 2.3f;
        /// <summary>How far from the middle of the pattern its ring of six stands.</summary>
        public float BoltRing = 6f;
        public float BoltDamage = 16f;

        [Header("The sigils")]
        public float SigilStepEvery = 0.85f;
        public float SigilWarning = 1f;
        public float SigilRadius = 2f;
        public float SigilOuter = 7.5f;
        public float SigilInner = 4.3f;
        public float SigilHeart = 3.2f;
        public float SigilDamage = 18f;

        [Header("The orbs")]
        public float OrbFlight = 0.7f;
        public float OrbSpeed = 2.6f;
        public float OrbFirstBurst = 2.6f;
        public float OrbBurstEvery = 0.6f;
        public float OrbRadius = 2.6f;
        public float OrbDamage = 20f;
        /// <summary>How far from the player each orb comes down before it begins to drift.</summary>
        public float OrbSpawnRing = 6.5f;

        [Header("The storm")]
        public float StormCast = 1.5f;
        public float StormSeconds = 8f;
        public float StormDamage = 10f;
        public float StormTick = 0.5f;

        [Header("The tail")]
        /// <summary>How far past the rim the last joint of the tail comes in the flop. (In the slither it crosses the whole floor.)</summary>
        public float FlopReach = 20f;
        /// <summary>How far the fan reaches beyond the last joint, at the size the tail is built at.</summary>
        public float TailFan = 4.2f;
        /// <summary>How much of the root end of the tail is kept off the floor in a slither that crosses it: the cut end is never seen.</summary>
        public float TailRootKept = 14.5f;
        /// <summary>How many times its made length the finned part of the tail is drawn out to: the plain thick root of the model is half its length, and drawn out the finned part reaches across the floor while all of the root stays over the rim and under the cloud.</summary>
        public float TailStretch = 1.5f;
        /// <summary>How much of the tail's made length, from its root, is the plain thick part with no fins: it is left as made, and only the finned part beyond it is drawn out.</summary>
        public float TailPlainShare = 0.55f;
        /// <summary>Degrees from north within which the tail never comes over the rim: a sacred rope hangs between the tall posts there, and his head and forefeet are in the gate. And degrees from north beyond which it never does: the steps come up at the south.</summary>
        public float TailNeverNorthOf = 100f;
        public float TailNeverSouthOf = 160f;
        /// <summary>How high the tail's belly is carried where it crosses the rim, to lie over the balustrade there, and how far inside the rim it begins to rise.</summary>
        public float RimLift = 1.5f;
        public float RimLiftWithin = 3.5f;
        /// <summary>The gate-posts round the floor, which the tail goes round and comes in between: how many (the first half a bay clockwise of north), how far out, how thick, within how many degrees of north they are tall, and how high the stubs elsewhere are.</summary>
        public int GatePosts = 14;
        public float PostRing = 1.2f;
        public float PostRadius = 0.75f;
        public float TallPostsWithin = 95f;
        public float StubTop = 0.9f;
        /// <summary>How wide his own head and forefeet are to his tail.</summary>
        public float HeadRound = 3.1f;
        public float ClawRound = 1.7f;
        /// <summary>How far either side of its middle line the tail strikes.</summary>
        public float TailHalfWidth = 1.7f;
        /// <summary>Seconds it takes to draw back off the floor the way it came.</summary>
        public float TailSlide = 1f;
        /// <summary>The slither: how long its path is shown, how long the tail takes to wind down it, how far it winds to either side and over what length, and how long before it comes the path stops following.</summary>
        public float TailSweepTelegraph = 1.3f;
        public float TailSweepSeconds = 1.3f;
        public float SlitherWidth = 3f;
        public float SlitherWave = 20f;
        public float SlitherLock = 0.4f;
        public float TailSweepDamage = 30f;
        /// <summary>Seconds it lies at the end of its path before it draws back.</summary>
        public float TailSweepRest = 0.8f;
        /// <summary>The flop: all of its warning, of which this much is spent coming out over the rim; how high the tip hangs; how long the smack takes to run from the root end to the tip; and how high the tip bounces after.</summary>
        public float TailSlamTelegraph = 1.4f;
        public float TailSlamExtend = 0.6f;
        public float FlopHeight = 6.5f;
        public float TailSlamDrop = 0.3f;
        public float FlopBounce = 1.1f;
        public float TailSlamWidth = 4f;
        public float TailSlamDamage = 34f;
        /// <summary>Seconds the tail lies where it fell, open to blows.</summary>
        public float TailSlamRest = 2.4f;
        /// <summary>Degrees clockwise from north the tail comes over the rim at, when under 360: for whoever is trying its moves. Otherwise where the player stands decides.</summary>
        public float TailFromDegrees = 999f;

        [Header("The coil")]
        public float CoilTelegraph = 1.5f;
        /// <summary>How wide the ring is laid, and how tight it draws.</summary>
        public float CoilRadius = 5.5f;
        public float CoilTight = 2.6f;
        /// <summary>Degrees round its ring the tail lies when it has drawn tight.</summary>
        public float CoilWrap = 385f;
        public float CoilSqueeze = 0.85f;
        public float CoilHold = 0.5f;
        /// <summary>Seconds it lies loose afterwards, open to a blow.</summary>
        public float CoilRest = 2f;
        public float CoilDamage = 36f;
        /// <summary>What its body does to whoever walks into it while it tightens.</summary>
        public float CoilTouchDamage = 16f;
        /// <summary>Damage to the tail while it is coiled that makes it spring open.</summary>
        public float CoilBreak = 40f;

        [Header("The scythe")]
        public float ScytheTelegraph = 1.5f;
        /// <summary>Degrees either side of straight across that it sweeps between.</summary>
        public float ScytheSpread = 76f;
        /// <summary>How far it swings toward the side where the posts are tall: it stops with all of it still on the floor.</summary>
        public float ScytheSpreadNorth = 35f;
        public float ScytheFirst = 1.3f;
        public float ScythePause = 0.5f;
        public float ScytheSecond = 0.85f;
        /// <summary>Seconds the tip comes behind the root.</summary>
        public float ScytheLag = 0.25f;
        /// <summary>How high it is arched where it comes over the rim: under there it passes overhead.</summary>
        public float ScytheArch = 3.4f;
        public float ScytheRootKept = 14.5f;
        public float ScytheDamage = 26f;
        public float ScytheRest = 0.6f;

        [Header("The walking flops")]
        public int FlopsCount = 3;
        public float FlopsFirst = 1.3f;
        public float FlopsNext = 0.95f;
        /// <summary>Degrees round from one lane to the next.</summary>
        public float FlopsStep = 30f;
        public float FlopsDamage = 26f;
        public float RippleDamage = 14f;
        public float RippleRadius = 1.8f;
        public float FlopsRest = 2.4f;

        [Header("The sting")]
        public int StingCount = 4;
        public float StingRise = 1.1f;
        public float StingEvery = 0.75f;
        /// <summary>Seconds before a stab that its mark stops following.</summary>
        public float StingLock = 0.35f;
        public float StingStab = 0.1f;
        public float StingHold = 0.2f;
        public float StingRadius = 1.9f;
        public float StingDamage = 20f;
        /// <summary>What one stab turned by a guard at the last instant costs his balance.</summary>
        public float StingBalance = 30f;
        public float StingArch = 6f;
        /// <summary>How high the last joint is held before a stab, and how far short of the mark: the fan reaches the rest of the way.</summary>
        public float StingCocked = 7f;
        public float StingFanBack = 3.2f;
        public float StingReach = 22f;
        /// <summary>Seconds the last stab stays stuck in the stone, open to a blow.</summary>
        public float StingStuck = 2.4f;

        [Header("The death")]
        public float DeathSeconds = 5f;
        public float DeathFreeze = 0.35f;
        public float DeathBoltsAt = 0.9f;
        public float DeathCoilsAt = 1.2f;
        public float DeathLeftLetsGoAt = 1.5f;
        public float DeathRightLetsGoAt = 2f;
        public float DeathHeadFallsAt = 2.6f;
        public float DeathHeadLandsAt = 3f;
        public float DeathHeadSlidesAt = 3.6f;
        public float DeathBoltEvery = 0.22f;
        public float DeathEyeGlow = 3f;

        [Header("The view of it")]
        /// <summary>How many times further off the camera stands while he is in the fight, and how far it is tipped toward the horizon: further and lower than for anything else in the game, because his body is all round the summit and has to be seen there.</summary>
        public float ViewDistance = 1.9f;
        /// <summary>The same for a player at the south rim, where the floor would otherwise fill the picture and the body beside it be cut off: further still.</summary>
        public float ViewDistanceSouth = 2.2f;
        public float ViewLead = 0.5f;
        public float ViewLeadLimit = 3.8f;
        public float SceneLeadLimit = 7f;
        public float ViewPitch = -21f;

        public Color PlainMarker = new Color(1f, 0.93f, 0.75f, 1f);
        public Color RedMarker = new Color(1f, 0.08f, 0.05f, 1f);
        public Color RoarColour = new Color(0.75f, 0.9f, 1.3f, 1f);

        public override bool IsStaggered => mode == Mode.Staggered;
        public override bool IsDead => mode == Mode.Dead || mode == Mode.Dying;
        public override bool HasRisen => mode != Mode.Hidden;
        public override bool InFight => mode != Mode.Hidden && mode != Mode.Rising && mode != Mode.Dead && mode != Mode.Dying;
        public bool Furious => Health.Fraction <= FuryAt;
        public override string Doing => mode.ToString();
        public override float DoingFor => elapsed;
        public override ColossusPart HeadPart => headPart;
        public ColossusPart TailPart => tailPart;
        public ColossusPart LeftClawPart => leftClawPart;
        public ColossusPart RightClawPart => rightClawPart;
        /// <summary>True while quarters of the floor crackle.</summary>
        public bool StormOn => stormLeft > 0f;
        // Counters so tests and probes can see what happened without watching the screen.
        public int Beams { get; private set; }
        public int Breaths { get; private set; }
        public int Bites { get; private set; }
        public int BoltsCalled { get; private set; }
        public int SigilsDrawn { get; private set; }
        public int OrbsCast { get; private set; }
        public int Storms { get; private set; }
        public int TailSweeps { get; private set; }
        public int TailSlams { get; private set; }
        public int TailCoils { get; private set; }
        public int CoilsSprung { get; private set; }
        public int TailScythes { get; private set; }
        public int TailFlopRuns { get; private set; }
        public int TailStings { get; private set; }
        /// <summary>Where on the rim the tail last came over, and the way it lay from there.</summary>
        public Vector3 TailEntry => tailEntry;
        /// <summary>The least room there has been between the tail's body and a post, his head or a forefoot in the move it is making or last made: below nothing, it went into one.</summary>
        public float TailLeastRoom { get; private set; }
        public Vector3 TailAlong => tailAlong;
        /// <summary>The middle of the coil's ring, and where the sting will next stab.</summary>
        public Vector3 CoilCentre => coilCentre;
        public Vector3 StingTarget => stingTarget;
        public int Lulls { get; private set; }
        public int BlowsStruck { get; private set; }
        public int TimesStaggered { get; private set; }
        /// <summary>How many arches of the body stand at least half out of the cloud now.</summary>
        /// <summary>How many loops of the body stand out of the cloud now.</summary>
        public int LoopsRisen { get; private set; }
        /// <summary>Degrees the jaw hangs open now.</summary>
        public float JawOpen => jawAngle;

        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private const int BeamPoints = 14;
        private const int OrbCount = 3;

        private Mode mode = Mode.Hidden;
        private float elapsed;
        private float rest;
        private IDamageable victim;
        private PlayerController victimBody;
        private TopDownCamera shaker;
        private HitStop hitStop;
        private MaterialPropertyBlock block;
        private Mesh beamMesh, breathMesh, biteMesh, fallMesh, orbMesh, stormMesh, tailSlamMesh, roarMesh;
        private GroundMark beamLane, beamGhost, biteMark, tailSlamMark;
        private GroundMark[] breathLanes, falls, orbRings, stormQuarters;
        private Renderer roarRenderer;
        private Renderer[] orbRenderers;
        private readonly Vector3[] beamPoints = new Vector3[BeamPoints];
        private Color[] glowBase;
        private float[] glowShown;
        private float serpentTime;
        private float jawAngle, jawWant;
        private float headYaw;
        private float nearHeadFor;
        private bool furyShown, stormOwed;
        private float untilRimBolt;
        private float roarRingFor = -1f;
        private bool leftGripped, rightGripped, roared;
        // The beam.
        private float beamFrom, beamTo, beamAngle, lastBeamAngle, untilBeamTouch, untilBeamRedraw;
        private bool beamBegun, beamSpentShown;
        // The breath.
        private float breathAngle;
        private bool breathStruck;
        // The bite.
        private Vector3 bitePoint, biteFromPosition;
        private Quaternion biteFromRotation;
        private bool biteFalling, biteLanded;
        // Whatever falls on a marked circle: the bolts and the sigils.
        private Vector3[] fallPoints;
        private float[] fallLeft, fallWarning, fallRadius, fallDamage;
        private Vector3 patternMiddle;
        private int patternStep, patternSteps;
        private float untilStep;
        // The orbs.
        private readonly Vector3[] orbAt = new Vector3[OrbCount];
        private readonly Vector3[] orbFrom = new Vector3[OrbCount];
        private readonly Vector3[] orbTo = new Vector3[OrbCount];
        private readonly float[] orbAge = new float[OrbCount];
        private readonly float[] orbLife = new float[OrbCount];
        // The storm.
        private int stormPair;
        private float stormLeft, untilStormBurn, untilStormBolt;
        // The tail.
        private SerpentSpine tailSpine;
        private Vector3 tailEntry, tailAlong = Vector3.left;
        private float tailLead, tailWave, tailWaveLength = 8f, tailWaveSpeed = 6f;
        private bool tailWinds, tailOut;
        private float slitherThrough, tailReach = 20f, tailChord = 34f, tailJoints = 30f;
        private int tailTurns;
        private float flopRaised, flopFront = -100f, flopBounce = -1f;
        private bool sweepStruck, slamLanded, slamStruck;
        private GroundMark[] slitherMarks;
        private enum TailShape { Trail, Lane, Coil, Scythe, Sting }
        private const int RibbonSections = 44;
        private const float RibbonRim = 0.22f;
        private TailShape tailShape;
        private float tailAway;
        private Vector3[] thingAt;
        private float[] thingRadius, thingTop;
        private GroundMark ribbonMark;
        private Mesh ribbonMesh;
        private Vector3[] ribbonVertices;
        private bool[] glowOnHead;
        private Vector3 coilCentre;
        private float coilRadius = 5f, coilSense = 1f, coilHurt, coilSprungFor;
        private bool coilCrushed, coilTouched, coilSprung;
        private float scytheFrom, scytheTo, scytheClock;
        private int scythePass;
        private bool scytheStruck;
        private int flopsDone;
        private float flopsClock, flopsYawFrom, flopsYawTo, flopsWay = 1f;
        private Vector3 stingJoint, stingTarget;
        private float stingArch, stingTip, stingClock;
        private int stingsDone;
        // The death.
        private bool headDown;
        private float untilDeathBolt;
        private int deathBolts;
        private float flashLeft, glowFlash;
        private bool flashing;
        private float untilCrack;

        public void Configure(Transform targetToFight, Transform headPiece, Transform jawPiece, Transform bodyPiece, Transform left, Transform right,
            Transform shortTailPiece, Transform whipPiece, ColossusPart headToStrike, ColossusPart tailToStrike, ColossusPart leftToStrike, ColossusPart rightToStrike,
            Renderer[] allRenderers)
        {
            target = targetToFight;
            head = headPiece;
            jaw = jawPiece;
            body = bodyPiece;
            leftClaw = left;
            rightClaw = right;
            shortTail = shortTailPiece;
            tail = whipPiece;
            tailBones = whipPiece != null ? whipPiece.GetComponent<SpineBones>() : null;
            headPart = headToStrike;
            tailPart = tailToStrike;
            leftClawPart = leftToStrike;
            rightClawPart = rightToStrike;
            renderers = allRenderers;
        }

        /// <summary>The marks its attacks draw, each an outline and the copy that fills it, in pairs; the orbs; and the lines the beam and the breath are drawn with.</summary>
        public void ConfigureMarks(MeshFilter[] beamPairs, MeshFilter[] breathPairs, MeshFilter[] bitePair, MeshFilter[] fallPairs, MeshFilter[] orbPairs,
            MeshFilter[] stormPairs, MeshFilter[] tailSweepPair, MeshFilter[] tailSlamPair, MeshFilter roar, Transform[] orbBodies, LineRenderer[] beamLines)
        {
            beamMarks = beamPairs;
            breathMarks = breathPairs;
            biteMarks = bitePair;
            fallMarks = fallPairs;
            orbMarks = orbPairs;
            stormMarks = stormPairs;
            tailSweepMarks = tailSweepPair;
            tailSlamMarks = tailSlamPair;
            roarRing = roar;
            orbs = orbBodies;
            beam = beamLines;
        }

        /// <summary>The parts of him that are made of light (the eyes; the marks on the brow and down every flank): which renderer, which of its materials, and whether it is an eye.</summary>
        public void ConfigureGlow(Renderer[] glowing, int[] materialIndices, bool[] eyes)
        {
            glowRenderers = glowing;
            glowMaterials = materialIndices;
            glowIsEye = eyes;
        }

        /// <summary>The boned lengths of body that travel round the summit through the cloud. When there are any, the rigid arches are not shown.</summary>
        public void ConfigureLengths(SpineBones[] bodyLengths) => lengths = bodyLengths;

        /// <summary>Adds the ring drawn over everything round the player while the tail lies between them and the camera.</summary>
        public void ConfigureFinder(Renderer ring) => finder = ring;

        /// <summary>True while the tail is between the camera and the player, and the ring that finds them is shown.</summary>
        public bool PlayerHidden { get; private set; }

        [SerializeField] private Renderer finder;

        // The tail is thick and the camera low in this fight: coiled near the south rim it can cover the player
        // entirely. While any joint of it is on the line from the camera to them, a ring is drawn through it where they stand.
        private void FindPlayer()
        {
            bool hidden = false;
            Vector3 to = target != null ? target.position + Vector3.up : Vector3.zero;
            if (tailOut && tailSpine != null && shaker != null && target != null)
            {
                Vector3 eye = shaker.transform.position;
                Vector3 line = to - eye;
                float length = line.magnitude;
                if (length > 1f)
                {
                    line /= length;
                    var points = tailSpine.Points;
                    int last = points.Length - 1;
                    for (int i = 0; i <= last && !hidden; i++)
                    {
                        float body = Mathf.Lerp(1.5f, 0.6f, i / (float)last);
                        Vector3 middle = points[i] + Vector3.up * (body * 0.6f);
                        float along = Mathf.Clamp(Vector3.Dot(middle - eye, line), 0f, length - 0.4f);
                        hidden = (eye + line * along - middle).sqrMagnitude < body * body;
                    }
                }
            }
            PlayerHidden = hidden;
            if (finder == null) return;
            if (finder.enabled != hidden) finder.enabled = hidden;
            if (hidden) finder.transform.SetPositionAndRotation(to, shaker.transform.rotation);
        }

        /// <summary>Adds the mark that stands over the fallen head while it can be finished.</summary>
        public void ConfigureFeedback(StaggerMark markToShow) => staggerMark = markToShow;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            neckBones = body != null ? body.GetComponent<NeckBones>() : null;
            Health.Maximum = MaximumHealth;
            Health.Fill();
            Stagger.Maximum = BalanceMaximum;
            Stagger.StaggeredSeconds = StaggeredSeconds;
            Stagger.Reset();
            // Lanes are a metre long and stretched to the length the floor leaves them, so none runs out over the cloud.
            beamMesh = MarkerShapes.Lane(1f, BeamWidth, MarkerShapes.InsideOfFan * 0.7f);
            breathMesh = MarkerShapes.Lane(1f, BreathWidth, MarkerShapes.InsideOfLane);
            biteMesh = MarkerShapes.Sector(BiteRadius, 360f);
            fallMesh = MarkerShapes.Sector(1f, 360f);
            orbMesh = MarkerShapes.Sector(OrbRadius, 360f);
            // The storm has to be seen as ground that hurts for as long as it lasts, so its inside is strong.
            stormMesh = MarkerShapes.Sector(FloorRadius - 0.4f, 90f, 0.4f);
            tailSlamMesh = MarkerShapes.Lane(1f, TailSlamWidth, MarkerShapes.InsideOfFan);
            roarMesh = MarkerShapes.Sector(1f, 360f, 0.03f);
            beamLane = Pair(beamMarks, 0, beamMesh);
            beamGhost = Pair(beamMarks, 1, beamMesh);
            biteMark = Pair(biteMarks, 0, biteMesh);
            // The round marks the tail's moves share: the coil's ring, the sting's spot, the scythe's whole floor. The fourth pair is the slither's lane.
            slitherMarks = Pairs(tailSweepMarks, fallMesh);
            ribbonMesh = RibbonMesh();
            // What the tail cannot pass through: the gate-posts, and after them his own head and two forefeet, which move.
            thingAt = new Vector3[GatePosts + 3];
            thingRadius = new float[GatePosts + 3];
            thingTop = new float[GatePosts + 3];
            for (int i = 0; i < GatePosts; i++)
            {
                float angle = (i + 0.5f) * 360f / GatePosts;
                thingAt[i] = Polar(angle, FloorRadius - PostRing);
                thingRadius[i] = PostRadius;
                thingTop[i] = Mathf.Abs(Mathf.DeltaAngle(angle, 0f)) < TallPostsWithin ? 100f : StubTop;
            }
            thingRadius[GatePosts] = HeadRound;
            thingRadius[GatePosts + 1] = thingRadius[GatePosts + 2] = ClawRound;
            thingTop[GatePosts] = thingTop[GatePosts + 1] = thingTop[GatePosts + 2] = 100f;
            ribbonMark = Pair(tailSweepMarks, 3, ribbonMesh);
            tailSlamMark = Pair(tailSlamMarks, 0, tailSlamMesh);
            breathLanes = Pairs(breathMarks, breathMesh);
            falls = Pairs(fallMarks, fallMesh);
            orbRings = Pairs(orbMarks, orbMesh);
            stormQuarters = Pairs(stormMarks, stormMesh);
            if (roarRing != null)
            {
                roarRing.sharedMesh = roarMesh;
                roarRenderer = roarRing.GetComponent<Renderer>();
                roarRenderer.enabled = false;
            }
            fallPoints = new Vector3[falls.Length];
            fallLeft = new float[falls.Length];
            fallWarning = new float[falls.Length];
            fallRadius = new float[falls.Length];
            fallDamage = new float[falls.Length];
            orbRenderers = new Renderer[orbs.Length];
            for (int i = 0; i < orbs.Length; i++)
            {
                orbRenderers[i] = orbs[i] != null ? orbs[i].GetComponent<Renderer>() : null;
                if (orbRenderers[i] != null) orbRenderers[i].enabled = false;
            }
            glowBase = new Color[glowRenderers.Length];
            glowShown = new float[glowRenderers.Length];
            for (int i = 0; i < glowRenderers.Length; i++)
            {
                glowShown[i] = 1f;
                var materials = glowRenderers[i] != null ? glowRenderers[i].sharedMaterials : null;
                if (materials != null && glowMaterials[i] < materials.Length && materials[glowMaterials[i]] != null)
                    glowBase[i] = materials[glowMaterials[i]].GetColor(Emission);
            }
            glowOnHead = new bool[glowRenderers.Length];
            for (int i = 0; i < glowRenderers.Length; i++) glowOnHead[i] = glowRenderers[i] != null && glowRenderers[i].transform.IsChildOf(head);
            foreach (var line in beam)
                if (line != null) line.enabled = false;
            Hide();
        }

        private GroundMark Pair(MeshFilter[] pairs, int index, Mesh mesh) =>
            pairs.Length >= index * 2 + 2 ? new GroundMark(pairs[index * 2], pairs[index * 2 + 1], mesh, block) : new GroundMark(null, null, mesh, block);

        private GroundMark[] Pairs(MeshFilter[] pairs, Mesh mesh)
        {
            var marks = new GroundMark[pairs.Length / 2];
            for (int i = 0; i < marks.Length; i++) marks[i] = Pair(pairs, i, mesh);
            return marks;
        }

        private void Start()
        {
            if (target != null)
            {
                victim = target.GetComponentInParent<IDamageable>();
                victimBody = target.GetComponentInParent<PlayerController>();
                hitStop = target.GetComponentInParent<HitStop>();
            }
            var main = Camera.main;
            shaker = main != null ? main.GetComponent<TopDownCamera>() : null;
        }

        private void OnDestroy()
        {
            Scrap(beamMesh);
            Scrap(breathMesh);
            Scrap(biteMesh);
            Scrap(fallMesh);
            Scrap(orbMesh);
            Scrap(stormMesh);
            Scrap(tailSlamMesh);
            Scrap(ribbonMesh);
            Scrap(roarMesh);
        }

        private static void Scrap(Mesh mesh)
        {
            if (mesh != null) Destroy(mesh);
        }

        private void OnDisable()
        {
            if (shaker != null) shaker.ClearFraming();
        }

        // It is far larger than the usual view: the camera stands back and looks north of the player, further the
        // further south the player is, so the head at the rim and the body in the cloud are seen with the floor.
        private void FrameTheFight()
        {
            if (shaker == null || target == null) return;
            float limit = mode == Mode.Rising || mode == Mode.Dying ? SceneLeadLimit : ViewLeadLimit;
            float north = Mathf.Clamp((HeadHome().z - target.position.z) * ViewLead, 0f, limit);
            float south = Mathf.InverseLerp(0f, -FloorRadius * 0.7f, target.position.z);
            shaker.SetFraming(Mathf.Lerp(ViewDistance, ViewDistanceSouth, south), new Vector3(-target.position.x * 0.25f, 0f, north), ViewPitch);
        }

        private void Hide()
        {
            SetShown(false);
            SetOpen(false, false, false);
        }

        private void SetShown(bool shown)
        {
            head.gameObject.SetActive(shown);
            body.gameObject.SetActive(shown);
            leftClaw.gameObject.SetActive(shown);
            rightClaw.gameObject.SetActive(shown);
            shortTail.gameObject.SetActive(shown);
            tail.gameObject.SetActive(shown && tailOut);
            foreach (var length in lengths) length.gameObject.SetActive(shown);
        }

        private void SetOpen(bool headOpen, bool tailOpen, bool clawsOpen)
        {
            headPart.SetVulnerable(headOpen);
            tailPart.SetVulnerable(tailOpen);
            leftClawPart.SetVulnerable(clawsOpen);
            rightClawPart.SetVulnerable(clawsOpen);
        }

        public override void Rise()
        {
            if (mode != Mode.Hidden) return;
            Health.Fill();
            Stagger.Reset();
            Afflictions.Clear();
            Chooser.Reset();
            leftGripped = rightGripped = roared = false;
            headDown = false;
            furyShown = stormOwed = false;
            nearHeadFor = 0f;
            untilRimBolt = 0f;
            stormLeft = 0f;
            tailOut = false;
            jawAngle = jawWant = JawSnarl;
            SetShown(true);
            head.localScale = Vector3.one * HeadScale;
            leftClaw.localScale = new Vector3(-1f, 1f, 1f);
            rightClaw.localScale = Vector3.one;
            head.SetPositionAndRotation(new Vector3(0f, -RiseDepth, FloorRadius + 3.5f), Quaternion.LookRotation(Vector3.back) * Quaternion.Euler(HeadPitch, 0f, 0f));
            Enter(Mode.Rising);
            AdvanceRising(0f);
            MoveSerpent(0f);
            if (shaker != null) shaker.Shake(0.3f, 0.6f);
        }

        public override HitOutcome TakePartHit(ColossusPart part, in Hit hit)
        {
            if (mode == Mode.Hidden || IsDead || mode == Mode.Rising) return HitOutcome.Missed;
            bool isHead = part == headPart;
            bool finisher = isHead && IsStaggered && hit.Heavy;
            // The head stuck after a bite or spent after a beam is only a head: it is the broken one that takes more, and can be finished.
            float share = isHead ? (IsStaggered ? HeadDamage : OpenHeadDamage) : part == tailPart ? TailDamage : ClawDamage;
            Afflictions.IgniteFrom(hit, part);
            float takes = Afflictions.DamageTakenMultiplier(hit.Spell);
            float unsteadies = Afflictions.StaggerTakenMultiplier;
            Afflictions.TakeFrom(hit, part);
            float damage = finisher ? Mathf.Max(hit.Damage * 3f * takes, MaximumHealth * FinisherShare) : hit.Damage * share * takes;
            Health.Damage(damage);
            flashLeft = 0.09f;
            glowFlash = 0.3f;
            if (Health.IsDead)
            {
                BeginDying();
                return finisher ? HitOutcome.Finisher : HitOutcome.Landed;
            }
            if (finisher)
            {
                // Finished, it tears itself up off the floor.
                Stagger.Reset();
                Enter(Mode.Recovering);
                return HitOutcome.Finisher;
            }
            if (part == tailPart && mode == Mode.TailCoil && !coilSprung && elapsed >= CoilTelegraph && (coilHurt += damage) >= CoilBreak)
            {
                SpringCoil();
                if (IsStaggered) return HitOutcome.Landed;
            }
            if (!IsStaggered && Stagger.Add(hit.Stagger * 0.7f * unsteadies)) BeginStagger();
            return HitOutcome.Landed;
        }

        public override void Unbalance(float amount)
        {
            if (mode == Mode.Hidden || mode == Mode.Rising || IsDead || IsStaggered) return;
            if (Stagger.Add(amount)) BeginStagger();
        }

        private void BeginStagger()
        {
            TimesStaggered++;
            HideMarkers();
            SetOpen(true, false, true);
            Enter(Mode.Staggered);
            Vfx.Play(VfxKind.Ring, FallenHead() + Vector3.up * 2f, Vector3.up);
            ScreenFlash.Flash(new Color(1f, 0.95f, 0.8f, 0.35f), 0.08f);
            if (shaker != null) shaker.Shake(0.5f, 0.4f);
        }

        private void BeginDying()
        {
            HideMarkers();
            SetOpen(false, false, false);
            headDown = false;
            untilDeathBolt = 0f;
            deathBolts = 0;
            stormLeft = 0f;
            Enter(Mode.Dying);
            // Everything stops on the blow that kills him, and then moves slowly while he understands it.
            if (hitStop != null)
            {
                hitStop.Slow(1.5f, 0.3f);
                hitStop.Freeze(DeathFreeze);
            }
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.6f), 0.12f);
            if (shaker != null) shaker.Shake(0.5f, 0.5f);
        }

        private void Enter(Mode next)
        {
            mode = next;
            elapsed = 0f;
        }

        private float Pace => Furious ? FuryPace : 1f;

        // Where things stand. North is +Z; the middle of the floor is the origin. Every piece faces along its own +Z
        // with +Y up, so a piece is aimed by looking it along the way it should face.
        private Vector3 HeadHome() => new Vector3(0f, HeadHeight, FloorRadius - HeadInset);
        private Vector3 FallenHead() => new Vector3(0f, 0f, FloorRadius - 9f);
        private Vector3 SpentHead() => new Vector3(0f, 0f, FloorRadius - 7.5f);
        private Vector3 BodyHome() => new Vector3(0f, -2.4f, FloorRadius + 5.5f) + bodyLean;

        // The neck is only so long. When the head goes further out over the floor than it reaches (the longest
        // bites), the body it grows from rears forward through the gate after it, by what is wanting and no more,
        // so the neck is never drawn out thin nor bent sharp to make up the distance.
        private void LeanAfterHead()
        {
            if (neckBones == null || neckBones.Length <= 0f) return;
            Vector3 to = neckBones.SkullPoint - (neckBones.Root - bodyLean);
            float height = to.y;
            to.y = 0f;
            float flat = to.magnitude;
            float reach = neckBones.Length * NeckReach;
            float within = Mathf.Sqrt(Mathf.Max(0f, reach * reach - height * height));
            Vector3 lean = flat > 0.01f ? to / flat * Mathf.Clamp(flat - within, 0f, NeckLeanLimit) : Vector3.zero;
            // At once, not eased: the head falls in an eighth of a second and the neck must not be seen to lag it.
            body.position += lean - bodyLean;
            bodyLean = lean;
        }

        private NeckBones neckBones;
        private Vector3 bodyLean;
        private Vector3 NorthRim() => new Vector3(0f, 0f, FloorRadius);
        // The point on the floor under his mouth that the beam and the breath turn about.
        private Vector3 MouthPivot() => new Vector3(0f, 0f, FloorRadius - HeadInset - 2.6f);
        // Where the beam leaves him: the middle of the front of the mouth, under the upper teeth, wherever the head is turned.
        private Vector3 Mouth() => head.TransformPoint(new Vector3(0f, 1.15f, 2.5f));

        /// <summary>The way along the ground that lies so many degrees round from due south, toward the east for more.</summary>
        public static Vector3 Southward(float degrees)
        {
            float angle = degrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(angle), 0f, -Mathf.Cos(angle));
        }

        /// <summary>How far a line from a point on a round floor runs along a level direction before it leaves the floor.</summary>
        public static float Chord(Vector3 from, Vector3 along, float floorRadius)
        {
            float lead = from.x * along.x + from.z * along.z;
            float inside = lead * lead - (from.x * from.x + from.z * from.z - floorRadius * floorRadius);
            return inside <= 0f ? 0f : Mathf.Max(0f, -lead + Mathf.Sqrt(inside));
        }

        private static Vector3 OnFloor(Vector3 point) => new Vector3(point.x, 0f, point.z);

        private static Vector3 Polar(float angle, float radius, float height = 0f)
        {
            float turn = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(turn) * radius, height, Mathf.Cos(turn) * radius);
        }

        private static Vector3 Within(Vector3 point, float radius)
        {
            var flat = new Vector3(point.x, 0f, point.z);
            return flat.magnitude > radius ? flat.normalized * radius : flat;
        }

        private void Update()
        {
            if (mode == Mode.Hidden || mode == Mode.Dead) return;
            FrameTheFight();
            float deltaTime = Time.deltaTime * WorldClock.EnemyScale;
            elapsed += deltaTime * (mode == Mode.Staggered || mode == Mode.Dying || mode == Mode.Rising ? 1f : Pace);
            if (target != null) nearHeadFor = InReach() ? nearHeadFor + deltaTime : 0f;
            // What burns or poisons it wears it down but never ends it: the last of a god is taken by a blow.
            if (Afflictions.Runs && mode != Mode.Rising && mode != Mode.Dying)
                Health.Damage(Mathf.Min(Afflictions.Tick(deltaTime), Mathf.Max(0f, Health.Current - 1f)));
            switch (mode)
            {
                case Mode.Rising:
                    AdvanceRising(deltaTime);
                    break;
                case Mode.Idle:
                    Settle(deltaTime);
                    FaceTarget(deltaTime, 0f, JawSnarl);
                    Stagger.Tick(deltaTime);
                    rest -= deltaTime * Pace;
                    // What has been decided from outside does not wait for his rest, and nor does the storm his fury opens with.
                    if ((rest <= 0f && MayBeginAttack) || Queued != DragonAttack.None || stormOwed) Choose();
                    break;
                case Mode.Beam:
                    AdvanceBeam(deltaTime);
                    break;
                case Mode.Breath:
                    AdvanceBreath(deltaTime);
                    break;
                case Mode.Bite:
                    AdvanceBite(deltaTime);
                    break;
                case Mode.Bolts:
                case Mode.Sigils:
                    AdvancePattern(deltaTime);
                    break;
                case Mode.Orbs:
                    Settle(deltaTime);
                    FaceTarget(deltaTime, -10f, elapsed < OrbFlight + 0.3f ? JawWide : JawSnarl);
                    if (elapsed > OrbFlight && NoOrbs()) EndAttack();
                    break;
                case Mode.Storm:
                    AdvanceStormCast(deltaTime);
                    break;
                case Mode.TailSweep:
                    AdvanceTailSweep(deltaTime);
                    break;
                case Mode.TailSlam:
                    AdvanceTailSlam(deltaTime);
                    break;
                case Mode.TailCoil:
                    AdvanceTailCoil(deltaTime);
                    break;
                case Mode.TailScythe:
                    AdvanceTailScythe(deltaTime);
                    break;
                case Mode.TailFlops:
                    AdvanceTailFlops(deltaTime);
                    break;
                case Mode.TailSting:
                    AdvanceTailSting(deltaTime);
                    break;
                case Mode.Staggered:
                    AdvanceStagger(deltaTime);
                    break;
                case Mode.Recovering:
                    AdvanceRecover(deltaTime);
                    break;
                case Mode.Dying:
                    AdvanceDying(deltaTime);
                    break;
            }
            if (mode == Mode.Dead) return;
            AdvanceFalls(deltaTime);
            AdvanceOrbs(deltaTime);
            AdvanceStorm(deltaTime);
            AdvanceFury(deltaTime);
            AdvanceRoarRing();
            LeanAfterHead();
            MoveSerpent(deltaTime);
            MoveJaw(deltaTime);
            MoveTail(deltaTime);
            FindPlayer();
            Paint();
            PaintGlow();
            // Once the head has come down, not while it is still falling.
            if (staggerMark != null) staggerMark.Show(mode == Mode.Staggered && elapsed >= 0.5f);
        }

        // ------------------------------------------------------------------ choosing

        private bool InReach()
        {
            Vector3 from = OnFloor(target.position) - NorthRim();
            return from.sqrMagnitude <= BiteReach * BiteReach;
        }

        private void Choose()
        {
            if (target == null) return;
            DragonAttack pick = Queued;
            Queued = DragonAttack.None;
            if (pick == DragonAttack.None && stormOwed)
            {
                // His fury opens with the storm, whatever came before it.
                pick = DragonAttack.Storm;
                Chooser.Record(pick);
            }
            stormOwed = false;
            if (pick == DragonAttack.None) pick = Chooser.Next(Random.value, nearHeadFor >= BiteAfterSeconds, InReach(), StormOn);
            switch (pick)
            {
                case DragonAttack.Beam: BeginBeam(); break;
                case DragonAttack.Breath: BeginBreath(); break;
                case DragonAttack.Bite: BeginBite(); break;
                case DragonAttack.Bolts: BeginBolts(); break;
                case DragonAttack.Sigils: BeginSigils(); break;
                case DragonAttack.Orbs: BeginOrbs(); break;
                case DragonAttack.Storm: BeginStorm(); break;
                case DragonAttack.TailSweep: BeginTailSweep(); break;
                case DragonAttack.TailSlam: BeginTailSlam(); break;
                case DragonAttack.TailCoil: BeginTailCoil(); break;
                case DragonAttack.TailScythe: BeginTailScythe(); break;
                case DragonAttack.TailFlops: BeginTailFlops(); break;
                case DragonAttack.TailSting: BeginTailSting(); break;
            }
        }

        // An attack is over: he rests, less in fury, and longer when the chooser says the player is owed a lull.
        private void EndAttack()
        {
            rest = RestBetween * (Furious ? FuryRest : 1f);
            if (Chooser.TakeLull())
            {
                // The rest is counted at his pace, and the lull is promised in real seconds.
                rest = LullSeconds * Pace;
                Lulls++;
            }
            Enter(Mode.Idle);
        }

        // ------------------------------------------------------------------ the body in the cloud

        // Loops of the one long body travel round the summit through the cloud, and the short tail waves far off.
        private void MoveSerpent(float deltaTime)
        {
            serpentTime += deltaTime * (Furious && mode != Mode.Dying ? FuryWinding : 1f);
            // Where the tail comes over the rim the body goes under first, quickly, and comes back slowly after.
            tailRoom = Mathf.MoveTowards(tailRoom, tailOut ? 1f : 0f, deltaTime * (tailOut ? 1.8f : 0.5f));
            LoopsRisen = MoveLengths(deltaTime);
            float sway = Mathf.Sin(serpentTime * 1.3f);
            // There is one tail: while the long one is out on the floor the end that waves in the cloud is under it,
            // and the long one is not drawn until that end has gone.
            tailAway = Mathf.MoveTowards(tailAway, tailOut ? 1f : 0f, deltaTime * (tailOut ? 8f : 0.8f));
            if (tail != null && mode != Mode.Hidden) tail.gameObject.SetActive(tailOut && tailAway >= 0.95f);
            float under = (1f - Surfaced(8)) * 12f + tailAway * 14f;
            // Its base is kept well under the top of the cloud sea however it leans: it wears no cloud of its own.
            shortTail.SetPositionAndRotation(tailHome + Vector3.up * (CloudLine - 1.6f + sway * 0.4f - under),
                Quaternion.Euler(sway * 9f, tailTurn + sway * 14f, Mathf.Cos(serpentTime * 0.9f) * 11f));
        }

        // The body round the summit is boned lengths, two to a ring, each ring further out than the last and
        // travelling at its own pace. Each length is one loop: it comes up out of the cloud sea beside the floor,
        // arcs over and dives under again, and the loop goes round the summit with the body, lead following lead.
        // Both ends of every length are held deep under the cloud always. Each time a length passes behind the gate
        // or round the south, where none of it is seen, it takes another kind of loop: tall, middling, or a long low
        // stretch of back along the top of the cloud. Returns how many loops stand out of the cloud.
        private int MoveLengths(float deltaTime)
        {
            if (lengths.Length == 0) return 0;
            if (lengthSpines == null)
            {
                // Made once the bones have woken and measured themselves.
                if (lengths[0].Lengths.Length == 0) return 0;
                lengthSpines = new SerpentSpine[lengths.Length];
                lengthPeak = new float[lengths.Length];
                lengthRamp = new float[lengths.Length];
                for (int i = 0; i < lengths.Length; i++)
                {
                    lengthSpines[i] = new SerpentSpine(lengths[i].Lengths, 8);
                    lengths[i].Spine = lengthSpines[i];
                    lengthPeak[i] = LoopPeaks[i % LoopPeaks.Length];
                    lengthRamp[i] = LoopRamps[i % LoopRamps.Length];
                }
            }
            int risen = 0;
            int rings = Mathf.Max(1, Mathf.Min(LengthRings.Length, LengthSpeeds.Length));
            float tailFrom = YawOf(tailEntry);
            // The slither goes out over the far rim too.
            bool crosses = tailShape == TailShape.Trail && tailWinds;
            float tailTo = crosses ? YawOf(tailEntry + tailAlong * tailChord) : tailFrom;
            float room = tailShape == TailShape.Scythe ? LengthTailRoom + 30f : LengthTailRoom;
            for (int i = 0; i < lengthSpines.Length; i++)
            {
                var spine = lengthSpines[i];
                var targets = spine.Targets;
                int last = spine.Count - 1;
                float span = spine.Spacing * last;
                int ring = i % rings;
                float radius = LengthRings[ring];
                float round = Mathf.PI * 2f * radius;
                // The lengths that share a ring are spread round it, not quite evenly, and no ring begins where another does.
                int sharing = (lengthSpines.Length - ring + rings - 1) / rings;
                float from = serpentTime * LengthSpeeds[ring] + round * ((i / rings) / (float)sharing + 0.07f * (i / rings)) + ring * 31f;
                float surfaced = Surfaced(i);
                // Which loop it makes this time round: chosen anew each time its middle passes due north or due south.
                int pass = Mathf.FloorToInt((from + span * 0.5f) / (round * 0.5f));
                int kind = (pass * 3 + i * 2 + (pass / 5)) % LoopPeaks.Length;
                lengthPeak[i] = Mathf.MoveTowards(lengthPeak[i], LoopPeaks[kind], deltaTime * 1.5f);
                lengthRamp[i] = Mathf.MoveTowards(lengthRamp[i], LoopRamps[Mathf.Min(kind, LoopRamps.Length - 1)], deltaTime * 1.5f);
                float ramp = Mathf.Max(4f, lengthRamp[i]);
                bool high = false;
                for (int j = 0; j <= last; j++)
                {
                    float metres = j * spine.Spacing;
                    float at = from + metres;
                    float degrees = at / radius * Mathf.Rad2Deg;
                    float fromNorth = Mathf.Abs(Mathf.DeltaAngle(0f, degrees));
                    float seen = Mathf.SmoothStep(0f, 1f, (fromNorth - LengthFromNorth) / 40f) * Mathf.SmoothStep(0f, 1f, (LengthToNorth - fromNorth) / 40f) * surfaced;
                    if (tailRoom > 0f)
                    {
                        float clear = Mathf.Min(Mathf.Abs(Mathf.DeltaAngle(degrees, tailFrom)), Mathf.Abs(Mathf.DeltaAngle(degrees, tailTo)));
                        seen *= Mathf.Lerp(1f, Mathf.SmoothStep(0f, 1f, (clear - room) / 18f), tailRoom);
                    }
                    float held = Mathf.SmoothStep(0f, 1f, metres / ramp) * Mathf.SmoothStep(0f, 1f, (span - metres) / ramp);
                    float height = LengthDeep + seen * held * (lengthPeak[i] - LengthDeep);
                    targets[j] = Polar(degrees, radius + 0.5f * Mathf.Sin(at * 0.21f + i), height);
                    if (height > CloudLine + 2.5f) high = true;
                }
                spine.Snap();
                if (high) risen++;
            }
            return risen;
        }

        private SerpentSpine[] lengthSpines;
        private float[] lengthPeak, lengthRamp;
        private float tailRoom;

        [SerializeField] private Vector3 tailHome = new Vector3(-24f, 0f, 22f);
        [SerializeField] private float tailTurn = 140f;

        /// <summary>Where the short tail stands in the cloud (its height is the cloud line), and how it is turned.</summary>
        public void ConfigureTail(Vector3 home, float turn)
        {
            tailHome = home;
            tailTurn = turn;
        }

        // How far arch i has come up in the entrance, one after another round the ring, or gone under in the death.
        private float Surfaced(int index)
        {
            if (mode == Mode.Rising) return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - index * 0.3f) / 1.1f));
            if (mode == Mode.Dying) return 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - DeathCoilsAt - index * 0.22f) / 1.1f));
            return 1f;
        }

        private void MoveJaw(float deltaTime)
        {
            jawAngle = Mathf.Lerp(jawAngle, jawWant, 1f - Mathf.Exp(-12f * deltaTime));
            jaw.localRotation = Quaternion.Euler(jawAngle * JawSign, 0f, 0f);
        }

        // The body breathes, and the forefeet grip the rim.
        private void Settle(float deltaTime)
        {
            float bob = Mathf.Sin(Time.time * 1.7f) * 0.35f;
            body.SetPositionAndRotation(Vector3.Lerp(body.position, BodyHome() + Vector3.up * (bob * 0.3f), 1f - Mathf.Exp(-3f * deltaTime)), Quaternion.Euler(0f, 180f, 0f));
            Grip(leftClaw, true, bob);
            Grip(rightClaw, false, -bob);
        }

        private Vector3 ClawPlant(bool left) => Polar(left ? -ClawAngle : ClawAngle, FloorRadius - ClawInset, 0.02f);

        private void Grip(Transform claw, bool left, float flex)
        {
            Vector3 plant = ClawPlant(left);
            claw.SetPositionAndRotation(plant, Quaternion.LookRotation(-OnFloor(plant).normalized) * Quaternion.Euler(flex * 4f, 0f, 0f));
        }

        // The head stays over the north edge, snout tipped down at the floor so its brow and eyes are seen from above,
        // and turns to keep its eyes on the player.
        private void FaceTarget(float deltaTime, float nod, float jawOpen)
        {
            float want = 0f;
            if (target != null)
            {
                Vector3 to = target.position - HeadHome();
                want = Mathf.Clamp(Mathf.Atan2(to.x, -to.z) * Mathf.Rad2Deg, -40f, 40f);
            }
            headYaw = Mathf.Lerp(headYaw, want, 1f - Mathf.Exp(-3f * deltaTime));
            float breath = Mathf.Sin(Time.time * 1.3f);
            head.position = Vector3.Lerp(head.position, HeadHome() + new Vector3(headYaw * 0.03f, breath * 0.25f, 0f), 1f - Mathf.Exp(-4f * deltaTime));
            var turned = Quaternion.LookRotation(Southward(headYaw)) * Quaternion.Euler(HeadPitch + nod + breath * 2f, 0f, 0f);
            head.rotation = Quaternion.Slerp(head.rotation, turned, 1f - Mathf.Exp(-8f * deltaTime));
            jawWant = jawOpen;
        }

        // The head taken somewhere for an attack of its own: where, facing which way along the ground, tipped how far
        // down (snout to the floor is more), rolled how far, and with the jaw how wide.
        private void PoseHead(float deltaTime, Vector3 at, Vector3 facing, float pitch, float roll, float quickness, float jawOpen)
        {
            head.position = Vector3.Lerp(head.position, at, 1f - Mathf.Exp(-quickness * deltaTime));
            var want = Quaternion.LookRotation(facing) * Quaternion.Euler(pitch, 0f, roll);
            head.rotation = Quaternion.Slerp(head.rotation, want, 1f - Mathf.Exp(-quickness * 1.5f * deltaTime));
            headYaw = Mathf.Atan2(facing.x, -facing.z) * Mathf.Rad2Deg;
            jawWant = jawOpen;
        }

        // ------------------------------------------------------------------ the entrance

        // Arch after arch breaks the cloud round the summit; a forefoot comes over the rim and grips it, then the
        // other; the neck rises behind them, and when the head is over the rim it roars. Only then does the fight begin.
        private void AdvanceRising(float deltaTime)
        {
            if (shaker != null && Time.frameCount % 8 == 0) shaker.Shake(0.12f, 0.2f);
            untilRimBolt -= deltaTime;
            if (untilRimBolt <= 0f)
            {
                untilRimBolt = EntranceBoltEvery;
                RimBolt(0.7f);
            }
            ClimbClaw(leftClaw, true, EntranceLeftClawAt, ref leftGripped);
            ClimbClaw(rightClaw, false, EntranceRightClawAt, ref rightGripped);

            float rearAt = EntranceRoarAt - EntranceRear;
            float hauled = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(EntranceRightClawAt, rearAt, elapsed));
            Vector3 under = Vector3.down * ((1f - hauled) * RiseDepth);
            body.SetPositionAndRotation(BodyHome() + under, Quaternion.Euler(0f, 180f, 0f));
            if (elapsed < rearAt)
            {
                // Up the outside of the rim, and then in through the gate.
                Vector3 outside = new Vector3(0f, HeadHeight - 0.6f, FloorRadius + 3.5f);
                Vector3 at = hauled < 0.7f ? outside + Vector3.down * ((1f - hauled / 0.7f) * RiseDepth) : Vector3.Lerp(outside, HeadHome(), Mathf.SmoothStep(0f, 1f, (hauled - 0.7f) / 0.3f));
                head.SetPositionAndRotation(at, Quaternion.LookRotation(Vector3.back) * Quaternion.Euler(HeadPitch + 14f, 0f, 0f));
                jawWant = JawSnarl;
            }
            else if (elapsed < EntranceRoarAt)
            {
                // Up and back off the rim, the jaws beginning to part: the breath before the roar.
                float reared = Mathf.SmoothStep(0f, 1f, (elapsed - rearAt) / EntranceRear);
                head.SetPositionAndRotation(HeadHome() + new Vector3(0f, 1.8f, -0.5f) * reared, Quaternion.LookRotation(Vector3.back) * Quaternion.Euler(Mathf.Lerp(HeadPitch + 14f, -24f, reared), 0f, 0f));
                jawWant = Mathf.Lerp(JawSnarl, 22f, reared);
            }
            else if (elapsed < EntranceRoarAt + RoarSeconds)
            {
                if (!roared) Roar();
                float shudder = Mathf.Sin(Time.time * 61f) * 0.14f;
                // Thrown forward over the rim with the jaws wide.
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, 0.6f, -2.2f), Vector3.back, 6f, shudder * 25f, 18f, JawWide);
            }
            else FaceTarget(deltaTime, 0f, JawSnarl);

            if (elapsed < RiseSeconds) return;
            rest = RestBetween;
            SetOpen(false, false, true);
            Enter(Mode.Idle);
        }

        // One forefoot of the entrance: up from under the rim, over it, and down on its edge. It hurts nobody.
        private void ClimbClaw(Transform claw, bool left, float gripAt, ref bool gripped)
        {
            Vector3 plant = ClawPlant(left);
            Vector3 outward = OnFloor(plant).normalized;
            var turned = Quaternion.LookRotation(-outward);
            float raiseAt = gripAt - 0.55f;
            if (elapsed < raiseAt)
            {
                claw.SetPositionAndRotation(plant + outward * 3.5f + Vector3.down * 7f, turned * Quaternion.Euler(-50f, 0f, 0f));
            }
            else if (elapsed < gripAt)
            {
                float t = (elapsed - raiseAt) / 0.55f;
                Vector3 below = plant + outward * 3.5f + Vector3.down * 7f;
                Vector3 over = plant + Vector3.up * 2.2f + outward * 0.6f;
                // Up in an arc and then down hard.
                Vector3 at = t < 0.75f ? Vector3.Lerp(below, over, Mathf.SmoothStep(0f, 1f, t / 0.75f)) : Vector3.Lerp(over, plant, (t - 0.75f) / 0.25f);
                claw.SetPositionAndRotation(at, turned * Quaternion.Euler(Mathf.Lerp(-50f, 0f, t), 0f, 0f));
            }
            else
            {
                if (!gripped)
                {
                    gripped = true;
                    Vfx.Play(VfxKind.Impact, plant + Vector3.up * 0.4f);
                    Vfx.Play(VfxKind.Dust, plant - outward, Vector3.up);
                    if (shaker != null) shaker.Shake(0.35f, 0.25f);
                }
                Grip(claw, left, 0f);
            }
        }

        private void Roar()
        {
            roared = true;
            roarRingFor = 0f;
            Vector3 mouth = Mouth();
            Vfx.Play(VfxKind.Ring, mouth, Vector3.back, 2f);
            Vfx.Play(VfxKind.Burst, mouth, Vector3.back, 2f);
            for (int i = -2; i <= 2; i++)
                Vfx.Play(VfxKind.Dust, new Vector3(i * 3.5f, 0.1f, FloorRadius - 3f - Mathf.Abs(i)), Vector3.back, 1.5f);
            // Hard and short: a frame of white at the edges, not a fade.
            ScreenFlash.Flash(new Color(0.9f, 0.96f, 1f, 0.7f), 0.1f);
            if (shaker != null) shaker.Shake(0.9f, RoarSeconds + 0.2f);
            LightningBolts.Strike(new Vector3(-FloorRadius * 0.75f, 0f, FloorRadius * 0.8f), 1f);
            LightningBolts.Strike(new Vector3(FloorRadius * 0.75f, 0f, FloorRadius * 0.8f), 1f);
            glowFlash = RoarSeconds;
            RaiseRoared();
        }

        // The roar is seen crossing the floor: a ring that widens from under his mouth until it has passed the far rim.
        private void AdvanceRoarRing()
        {
            if (roarRingFor < 0f || roarRenderer == null) return;
            roarRingFor += Time.deltaTime;
            float t = roarRingFor / RoarRingSeconds;
            if (t >= 1f)
            {
                roarRingFor = -1f;
                roarRenderer.enabled = false;
                return;
            }
            float radius = Mathf.Lerp(1.5f, FloorRadius * 2f + 2f, 1f - (1f - t) * (1f - t));
            roarRing.transform.SetPositionAndRotation(NorthRim() + Vector3.up * 0.08f, Quaternion.identity);
            roarRing.transform.localScale = new Vector3(radius, 1f, radius);
            roarRenderer.enabled = true;
            var colour = RoarColour;
            colour.a = 1f - t * t;
            block.Clear();
            block.SetColor(RoarTint, colour);
            roarRenderer.SetPropertyBlock(block);
        }

        private static readonly int RoarTint = Shader.PropertyToID("_Color");

        // A bolt somewhere round the northern rim, off the floor: the storm about him, not an attack.
        private void RimBolt(float skyFlash)
        {
            float angle = Random.Range(-115f, 115f) * Mathf.Deg2Rad;
            float distance = FloorRadius + Random.Range(1.5f, 4.5f);
            LightningBolts.Strike(new Vector3(Mathf.Sin(angle) * distance, -0.5f, Mathf.Cos(angle) * distance), skyFlash);
        }

        // ------------------------------------------------------------------ fury

        private void AdvanceFury(float deltaTime)
        {
            if (!Furious || IsDead || mode == Mode.Rising) return;
            if (!furyShown && mode != Mode.Staggered)
            {
                furyShown = true;
                stormOwed = true;
                Vfx.Play(VfxKind.Ring, head.position + Vector3.up * 2f, Vector3.back, 2f);
                Vfx.Play(VfxKind.Danger, head.position + Vector3.up * 3f);
                ScreenFlash.Flash(new Color(0.7f, 0.85f, 1f, 0.5f), 0.09f);
                if (shaker != null) shaker.Shake(0.6f, 0.5f);
                RaiseEnraged();
            }
            untilRimBolt -= deltaTime;
            if (untilRimBolt > 0f) return;
            untilRimBolt = FuryRimBoltEvery * Random.Range(0.7f, 1.3f);
            RimBolt(0.5f);
        }

        // ------------------------------------------------------------------ the beam

        private void BeginBeam()
        {
            Beams++;
            beamBegun = false;
            beamSpentShown = false;
            untilBeamTouch = 0f;
            untilBeamRedraw = 0f;
            // It begins on the side away from the player, so it is seen coming.
            float side = target.position.x >= 0f ? 1f : -1f;
            beamFrom = -side * BeamArc;
            beamTo = side * BeamArc;
            beamAngle = lastBeamAngle = beamFrom;
            Enter(Mode.Beam);
            Vfx.Play(VfxKind.Danger, head.position + Vector3.up * 3f);
        }

        private void ShowLane(GroundMark lane, Vector3 pivot, float degrees, float progress, float strength = 1f)
        {
            Vector3 along = Southward(degrees);
            lane.Show(pivot, Quaternion.LookRotation(along), progress, RedMarker, true, 1f, Chord(pivot, along, FloorRadius - 0.3f), strength);
        }

        private void AdvanceBeam(float deltaTime)
        {
            Settle(deltaTime);
            Vector3 pivot = MouthPivot();
            int passes = Furious ? 2 : 1;
            float sweepEnds = BeamTelegraph + BeamSeconds * passes;
            if (elapsed < BeamTelegraph)
            {
                // He rears up and back with his face to the floor and his jaws coming wide, the throat alight; and
                // shakes more the nearer it is.
                float near = elapsed / BeamTelegraph;
                float reared = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / (BeamTelegraph * 0.45f)));
                float shudder = Mathf.Sin(Time.time * 53f) * 0.1f * near;
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, 1.9f * reared, -0.6f * reared), Southward(beamFrom * 0.5f),
                    Mathf.Lerp(HeadPitch, -30f, reared), shudder * 25f, 7f, Mathf.Lerp(JawSnarl, JawWide, reared));
                ShowLane(beamLane, pivot, beamFrom, near);
                // Where it will go: a faint lane that runs the whole crossing ahead of time, twice.
                float ghost = Mathf.Repeat(elapsed / (BeamTelegraph * 0.5f), 1f);
                ShowLane(beamGhost, pivot, Mathf.Lerp(beamFrom, beamTo, ghost), 0f, 0.45f);
                if (Time.frameCount % 5 == 0) Vfx.Play(VfxKind.Shock, Mouth(), Vector3.up, 0.6f + near);
            }
            else if (elapsed < sweepEnds)
            {
                if (!beamBegun)
                {
                    beamBegun = true;
                    beamGhost.Hide();
                    Vfx.Play(VfxKind.Ring, pivot + Vector3.up * 0.3f, Vector3.up, 1.5f);
                    ScreenFlash.Flash(new Color(0.8f, 0.9f, 1f, 0.35f), 0.07f);
                    if (shaker != null) shaker.Shake(0.4f, BeamSeconds * passes);
                }
                float into = (elapsed - BeamTelegraph) / BeamSeconds;
                int pass = Mathf.Min(passes - 1, (int)into);
                float t = Mathf.Clamp01(into - pass);
                beamAngle = pass % 2 == 0 ? Mathf.Lerp(beamFrom, beamTo, t) : Mathf.Lerp(beamTo, beamFrom, t);
                Vector3 along = Southward(beamAngle);
                float reach = Chord(pivot, along, FloorRadius - 0.3f);
                // Thrown forward and down over the rim, the open mouth along the lane, for as long as it lasts.
                float shudder = Mathf.Sin(Time.time * 67f) * 0.1f;
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, 0.2f, -1.4f), along, 16f, shudder * 20f, 16f, JawWide);
                ShowLane(beamLane, pivot, beamAngle, 1f);
                DrawBeam(Mouth(), pivot + along * reach + Vector3.up * 0.5f, BeamCoreWidth, BeamWidth * 0.5f);
                if (Time.frameCount % 2 == 0)
                {
                    Vector3 spot = pivot + along * Random.Range(1f, Mathf.Max(1.5f, reach));
                    Vfx.Play(VfxKind.Shock, spot + Vector3.up * 0.3f, Vector3.up, 0.9f);
                    if (Time.frameCount % 6 == 0) Vfx.Play(VfxKind.Dust, pivot + along * reach, Vector3.up);
                }
                untilBeamTouch -= deltaTime;
                if (untilBeamTouch <= 0f && target != null && BeamCrosses(pivot, reach))
                {
                    untilBeamTouch = BeamTick;
                    // Thrown the way the beam is going.
                    float going = Mathf.Sign(beamAngle - lastBeamAngle);
                    Vector3 across = Quaternion.LookRotation(along) * Vector3.right * (going == 0f ? 1f : going);
                    Deal(new Hit(BeamDamage, 0f, across, false, true, pivot), PerfectGuardBalance * 0.5f);
                    if (mode != Mode.Beam) return;
                }
                lastBeamAngle = beamAngle;
            }
            else if (elapsed < sweepEnds + BeamSpent)
            {
                if (!beamSpentShown)
                {
                    beamSpentShown = true;
                    beamLane.Hide();
                    HideBeam();
                    headPart.SetVulnerable(true);
                    Vfx.Play(VfxKind.Dust, SpentHead() + Vector3.back * 2f, Vector3.up, 1.5f);
                    if (shaker != null) shaker.Shake(0.3f, 0.25f);
                }
                // Spent, the head lies on the floor inside the rim with its jaw slack. He pulls at it harder as the
                // moment runs out, which is how the player knows it is ending.
                float pulling = Mathf.Clamp01((elapsed - sweepEnds) / BeamSpent);
                float shudder = Mathf.Sin(Time.time * 41f) * 0.05f * (pulling * pulling * 2f);
                PoseHead(deltaTime, SpentHead() + new Vector3(shudder, 0f, 0f), Southward(beamAngle * 0.25f), 3f, 9f + shudder * 40f, 9f, 20f);
            }
            else
            {
                headPart.SetVulnerable(false);
                EndAttack();
            }
        }

        // True when the beam has passed over the player since the last frame. The beam is a line from the pivot, so
        // what is asked is whether the angle the player stands at lies between where it was and where it is.
        private bool BeamCrosses(Vector3 pivot, float reach)
        {
            Vector3 off = OnFloor(target.position) - pivot;
            float distance = off.magnitude;
            if (distance < 0.4f) return true;
            if (distance > reach + 0.5f) return false;
            float at = Mathf.Atan2(off.x, -off.z) * Mathf.Rad2Deg;
            float half = Mathf.Atan2(BeamWidth * 0.5f + 0.3f, distance) * Mathf.Rad2Deg;
            return at >= Mathf.Min(lastBeamAngle, beamAngle) - half && at <= Mathf.Max(lastBeamAngle, beamAngle) + half;
        }

        // The beam itself: one thick straight bolt from the mouth to where the lane ends, and thin ones kinking round it.
        private void DrawBeam(Vector3 from, Vector3 to, float width, float stray)
        {
            untilBeamRedraw -= Time.deltaTime;
            if (untilBeamRedraw > 0f) return;
            untilBeamRedraw = 0.04f;
            Vector3 along = (to - from).normalized;
            Vector3 across = Vector3.Cross(Vector3.up, along).normalized;
            for (int strand = 0; strand < beam.Length; strand++)
            {
                var line = beam[strand];
                if (line == null) continue;
                bool core = strand == 0;
                for (int i = 0; i < BeamPoints; i++)
                {
                    float t = i / (float)(BeamPoints - 1);
                    Vector3 point = Vector3.Lerp(from, to, t);
                    bool end = i == 0 || i == BeamPoints - 1;
                    if (!end) point += across * Random.Range(-1f, 1f) * (core ? 0.08f : stray) + Vector3.up * Random.Range(-0.1f, core ? 0.1f : 0.8f);
                    beamPoints[i] = point;
                }
                line.positionCount = BeamPoints;
                line.SetPositions(beamPoints);
                line.widthMultiplier = core ? width * Random.Range(0.85f, 1.1f) : width * 0.16f;
                line.enabled = core || Random.value > 0.2f;
            }
        }

        private void HideBeam()
        {
            foreach (var line in beam)
                if (line != null) line.enabled = false;
        }

        // ------------------------------------------------------------------ the breath

        private void BeginBreath()
        {
            Breaths++;
            breathStruck = false;
            untilBeamRedraw = 0f;
            AimBreath();
            Enter(Mode.Breath);
            Vfx.Play(VfxKind.Glint, head.position + Vector3.up * 3f, Vector3.up, 2f);
        }

        private void AimBreath()
        {
            Vector3 off = OnFloor(target.position) - MouthPivot();
            breathAngle = off.sqrMagnitude > 1f ? Mathf.Clamp(Mathf.Atan2(off.x, -off.z) * Mathf.Rad2Deg, -55f, 55f) : 0f;
        }

        private float ForkAngle(int fork) => breathAngle + (fork - (breathLanes.Length - 1) * 0.5f) * BreathFork;

        private void AdvanceBreath(float deltaTime)
        {
            Settle(deltaTime);
            Vector3 pivot = MouthPivot();
            if (elapsed < BreathTelegraph)
            {
                // The lanes follow until just before the breath: stepping out late is what gets the player clear.
                if (elapsed < BreathTelegraph - BreathLock) AimBreath();
                float near = elapsed / BreathTelegraph;
                // Low over the rim, mouth to the floor.
                PoseHead(deltaTime, HeadHome() + new Vector3(0f, -0.5f, -1.2f), Southward(breathAngle), 30f, 0f, 8f, Mathf.Lerp(JawSnarl, 30f, near));
                for (int i = 0; i < breathLanes.Length; i++) ShowLane(breathLanes[i], pivot, ForkAngle(i), near);
                if (Time.frameCount % 5 == 0) Vfx.Play(VfxKind.Shock, Mouth(), Vector3.up, 0.5f + near * 0.6f);
            }
            else if (elapsed < BreathTelegraph + BreathSeconds)
            {
                float shudder = Mathf.Sin(Time.time * 67f) * 0.1f;
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, -0.9f, -2f), Southward(breathAngle), 34f, shudder * 20f, 16f, JawWide);
                DrawForks(pivot);
                if (!breathStruck)
                {
                    breathStruck = true;
                    ScreenFlash.Flash(new Color(0.8f, 0.9f, 1f, 0.3f), 0.06f);
                    if (shaker != null) shaker.Shake(0.35f, BreathSeconds);
                    for (int i = 0; i < breathLanes.Length; i++)
                    {
                        Vector3 along = Southward(ForkAngle(i));
                        float reach = Chord(pivot, along, FloorRadius - 0.3f);
                        for (float at = 2f; at < reach; at += 4f) Vfx.Play(VfxKind.Shock, pivot + along * at + Vector3.up * 0.3f, Vector3.up, 1f);
                        if (target == null || !InLane(target.position, pivot, along, reach, BreathWidth)) continue;
                        Vector3 offset = OnFloor(target.position) - pivot;
                        Vector3 right = Quaternion.LookRotation(along) * Vector3.right;
                        Deal(new Hit(BreathDamage, 0f, right * (Vector3.Dot(offset, right) >= 0f ? 1f : -1f), false, true, pivot), PerfectGuardBalance * 0.5f);
                        if (mode != Mode.Breath) return;
                        break;
                    }
                }
            }
            else if (elapsed < BreathTelegraph + BreathSeconds + BreathRest)
            {
                HideBreath();
                PoseHead(deltaTime, HeadHome() + new Vector3(0f, -0.3f, -0.6f), Southward(breathAngle), HeadPitch + 8f, 0f, 4f, JawSnarl);
            }
            else EndAttack();
        }

        private void HideBreath()
        {
            foreach (var lane in breathLanes) lane.Hide();
            HideBeam();
        }

        // The breath: one kinked bolt down each fork, from the mouth to the floor under it and out along the ground.
        private void DrawForks(Vector3 pivot)
        {
            untilBeamRedraw -= Time.deltaTime;
            if (untilBeamRedraw > 0f) return;
            untilBeamRedraw = 0.04f;
            Vector3 mouth = Mouth();
            for (int fork = 0; fork < beam.Length && fork < breathLanes.Length; fork++)
            {
                var line = beam[fork];
                if (line == null) continue;
                Vector3 along = Southward(ForkAngle(fork));
                Vector3 across = new Vector3(-along.z, 0f, along.x);
                float reach = Chord(pivot, along, FloorRadius - 0.3f);
                beamPoints[0] = mouth;
                for (int i = 1; i < BeamPoints; i++)
                {
                    float t = (i - 1) / (float)(BeamPoints - 2);
                    beamPoints[i] = pivot + along * (t * reach) + across * Random.Range(-BreathWidth * 0.4f, BreathWidth * 0.4f) + Vector3.up * Random.Range(0.25f, 0.9f);
                }
                line.positionCount = BeamPoints;
                line.SetPositions(beamPoints);
                line.widthMultiplier = 0.34f * Random.Range(0.7f, 1.15f);
                line.enabled = true;
            }
        }

        private static bool InLane(Vector3 point, Vector3 from, Vector3 along, float length, float width)
        {
            Vector3 offset = OnFloor(point) - OnFloor(from);
            float lengthways = Vector3.Dot(offset, along);
            if (lengthways < -0.5f || lengthways > length + 0.3f) return false;
            return (offset - along * lengthways).magnitude <= width * 0.5f + 0.3f;
        }

        // ------------------------------------------------------------------ the bite

        private void BeginBite()
        {
            Bites++;
            biteFalling = false;
            biteLanded = false;
            AimBite();
            Enter(Mode.Bite);
            Vfx.Play(VfxKind.Glint, head.position + Vector3.up * 3f, Vector3.up, 2f);
        }

        // At the player, but no further from the rim than his neck will stretch.
        private void AimBite()
        {
            Vector3 to = OnFloor(target.position) - NorthRim();
            float limit = BiteReach + 1.5f;
            if (to.magnitude > limit) to = to.normalized * limit;
            if (to.sqrMagnitude < 9f) to = Vector3.back * 3f;
            bitePoint = NorthRim() + to;
        }

        private void AdvanceBite(float deltaTime)
        {
            Settle(deltaTime);
            Vector3 toRim = OnFloor(HeadHome() - bitePoint).normalized;
            // The chin comes down behind where the player stood and the snout reaches the rest of the way, so one it
            // misses, or who has guarded it, is left standing at its teeth and not hidden under it.
            Vector3 lie = bitePoint + toRim * BiteBack;
            Vector3 middle = bitePoint + toRim * 0.9f;
            var lying = Quaternion.LookRotation(-toRim) * Quaternion.Euler(4f, 0f, 0f);
            float fall = BiteTelegraph + BiteDrop;
            if (elapsed < BiteTelegraph)
            {
                // The circle follows until just before he falls on it.
                if (elapsed < BiteTelegraph - BiteLock) AimBite();
                // He comes in over the rim and hangs above the circle, jaws wide, looking down into it: the height he falls from.
                float reared = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / (BiteTelegraph * 0.55f)));
                Vector3 over = Vector3.Lerp(HeadHome(), lie + toRim * 2.2f + Vector3.up * 5.5f, reared);
                PoseHead(deltaTime, over, -toRim, Mathf.Lerp(HeadPitch, 48f, reared), 0f, 8f, Mathf.Lerp(JawSnarl, JawWide, reared));
                biteMark.Show(middle, Quaternion.identity, elapsed / BiteTelegraph, PlainMarker, false);
            }
            else if (elapsed < fall)
            {
                if (!biteFalling)
                {
                    biteFalling = true;
                    biteFromPosition = head.position;
                    biteFromRotation = head.rotation;
                }
                float t = (elapsed - BiteTelegraph) / BiteDrop;
                head.SetPositionAndRotation(Vector3.Lerp(biteFromPosition, lie, t * t), Quaternion.Slerp(biteFromRotation, lying, t));
                jawWant = JawWide;
            }
            else if (!biteLanded)
            {
                biteLanded = true;
                head.SetPositionAndRotation(lie, lying);
                // The jaws shut on the floor.
                jawAngle = jawWant = 3f;
                biteMark.Hide();
                LandEffects(middle, BiteRadius);
                if (shaker != null) shaker.Shake(0.8f, 0.4f);
                Vector3 offset = target != null ? target.position - middle : Vector3.forward * 100f;
                offset.y = 0f;
                // He comes from the rim: the guard has to face him, wherever in the circle the player stands.
                if (offset.magnitude <= BiteRadius + 0.35f) Deal(new Hit(BiteDamage, 0f, -toRim), PerfectGuardBalance);
                if (mode != Mode.Bite) return;
                headPart.SetVulnerable(true);
            }
            else if (elapsed < fall + BiteStuck)
            {
                // Stuck. He pulls at it harder as the moment runs out, which is how the player knows it is ending.
                float pulling = Mathf.Clamp01((elapsed - fall) / BiteStuck);
                float shudder = Mathf.Sin(Time.time * 41f) * 0.05f * (0.3f + pulling * pulling * 2f);
                head.SetPositionAndRotation(lie + new Vector3(shudder, 0f, shudder), lying * Quaternion.Euler(0f, 0f, shudder * 60f));
                jawWant = 3f;
            }
            else
            {
                headPart.SetVulnerable(false);
                EndAttack();
            }
        }

        // ------------------------------------------------------------------ the bolts and the sigils

        // Something will fall on a circle: the circle is drawn now, and struck when its warning has run.
        private void Fall(Vector3 point, float radius, float warning, float damage)
        {
            for (int i = 0; i < fallLeft.Length; i++)
            {
                if (fallLeft[i] > 0f) continue;
                fallPoints[i] = point;
                fallRadius[i] = radius;
                fallWarning[i] = warning;
                fallLeft[i] = warning;
                fallDamage[i] = damage;
                return;
            }
        }

        private bool NoFalls()
        {
            foreach (float left in fallLeft)
                if (left > 0f) return false;
            return true;
        }

        private void AdvanceFalls(float deltaTime)
        {
            bool gone = IsDead || mode == Mode.Staggered;
            for (int i = 0; i < fallLeft.Length; i++)
            {
                if (fallLeft[i] <= 0f) continue;
                fallLeft[i] -= deltaTime;
                if (fallLeft[i] > 0f && !gone)
                {
                    falls[i].Show(fallPoints[i], Quaternion.identity, 1f - fallLeft[i] / fallWarning[i], RedMarker, false, fallRadius[i]);
                    continue;
                }
                fallLeft[i] = 0f;
                falls[i].Hide();
                if (gone) continue;
                LightningBolts.Strike(fallPoints[i], 0.4f);
                Strike(fallPoints[i], fallRadius[i], fallDamage[i], true);
            }
        }

        // The bolts: the middle, then a ring of six round it, then the middle again with the ring turned to where
        // its gaps were. Whoever reads it steps out, steps in, and stands between.
        private void BeginBolts()
        {
            BoltsCalled++;
            patternMiddle = Within(target.position, FloorRadius - BoltRing - BoltRadius);
            patternStep = 0;
            patternSteps = Furious ? 4 : 3;
            untilStep = 0.4f;
            Enter(Mode.Bolts);
            Vfx.Play(VfxKind.Danger, head.position + Vector3.up * 3f);
            if (shaker != null) shaker.Shake(0.2f, 0.3f);
        }

        // The sigils: a ring of eight round the player with gaps between, then a closed ring inside it, then the heart.
        private void BeginSigils()
        {
            SigilsDrawn++;
            patternMiddle = Within(target.position, FloorRadius - SigilOuter - SigilRadius);
            patternStep = 0;
            patternSteps = 3;
            untilStep = 0.3f;
            Enter(Mode.Sigils);
            Vfx.Play(VfxKind.Danger, head.position + Vector3.up * 3f);
        }

        private void AdvancePattern(float deltaTime)
        {
            Settle(deltaTime);
            // Head thrown up to the sky he is calling on, jaws parted.
            FaceTarget(deltaTime, -34f, 26f);
            untilStep -= deltaTime;
            if (patternStep < patternSteps && untilStep <= 0f)
            {
                if (mode == Mode.Bolts) BoltWave(patternStep);
                else SigilStep(patternStep);
                patternStep++;
                untilStep = mode == Mode.Bolts ? BoltWaveEvery : SigilStepEvery;
            }
            if (patternStep >= patternSteps && NoFalls()) EndAttack();
        }

        private void BoltWave(int wave)
        {
            if (wave == 0 || wave == 2) Fall(patternMiddle, BoltRadius, BoltWarning, BoltDamage);
            if (wave == 0) return;
            float turned = wave == 2 ? 30f : 0f;
            for (int i = 0; i < 6; i++) Fall(patternMiddle + Polar(i * 60f + turned, BoltRing), BoltRadius, BoltWarning, BoltDamage);
        }

        private void SigilStep(int step)
        {
            if (step == 2)
            {
                Fall(patternMiddle, SigilHeart, SigilWarning * 0.8f, SigilDamage);
                return;
            }
            float ring = step == 0 ? SigilOuter : SigilInner;
            for (int i = 0; i < 8; i++) Fall(patternMiddle + Polar(i * 45f + step * 22.5f, ring), SigilRadius, SigilWarning * (step == 0 ? 1f : 0.9f), SigilDamage);
        }

        // ------------------------------------------------------------------ the orbs

        private void BeginOrbs()
        {
            OrbsCast++;
            Vector3 mouth = Mouth();
            Vector3 about = OnFloor(target.position);
            float turn = Random.Range(0f, 120f);
            for (int i = 0; i < OrbCount && i < orbs.Length; i++)
            {
                orbAge[i] = 0f;
                orbLife[i] = OrbFirstBurst + i * OrbBurstEvery;
                orbFrom[i] = mouth;
                orbTo[i] = Within(about + Polar(turn + i * 120f, OrbSpawnRing), FloorRadius - 1.5f) + Vector3.up * 1.5f;
                orbAt[i] = mouth;
            }
            Enter(Mode.Orbs);
            Vfx.Play(VfxKind.Burst, mouth, Vector3.back, 1.5f);
        }

        private bool NoOrbs()
        {
            foreach (float life in orbLife)
                if (life > 0f) return false;
            return true;
        }

        // Each orb is spat out to its place, then drifts after the player with its circle under it, and bursts when
        // the circle has filled.
        private void AdvanceOrbs(float deltaTime)
        {
            bool gone = IsDead || mode == Mode.Staggered;
            for (int i = 0; i < OrbCount && i < orbs.Length; i++)
            {
                if (orbLife[i] <= 0f) continue;
                orbAge[i] += deltaTime;
                if (gone || orbAge[i] >= orbLife[i])
                {
                    orbLife[i] = 0f;
                    orbRings[i].Hide();
                    if (orbRenderers[i] != null) orbRenderers[i].enabled = false;
                    if (gone) continue;
                    Vector3 burst = OnFloor(orbAt[i]);
                    Vfx.Play(VfxKind.Burst, orbAt[i], Vector3.up, 1.6f);
                    Vfx.Play(VfxKind.Ring, burst + Vector3.up * 0.2f, Vector3.up, 1.3f);
                    LightningBolts.Strike(burst, 0.4f);
                    Strike(burst, OrbRadius, OrbDamage, true);
                    continue;
                }
                if (orbAge[i] < OrbFlight)
                {
                    float t = orbAge[i] / OrbFlight;
                    orbAt[i] = Vector3.Lerp(orbFrom[i], orbTo[i], t * (2f - t)) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * 2f);
                }
                else if (target != null)
                {
                    Vector3 toward = Vector3.MoveTowards(OnFloor(orbAt[i]), OnFloor(target.position), OrbSpeed * deltaTime);
                    orbAt[i] = toward + Vector3.up * (1.5f + Mathf.Sin(Time.time * 3f + i * 2f) * 0.2f);
                }
                float near = orbAge[i] / orbLife[i];
                if (orbRenderers[i] != null)
                {
                    orbRenderers[i].enabled = true;
                    orbs[i].position = orbAt[i];
                    // It beats faster and swells as it comes to bursting.
                    orbs[i].localScale = Vector3.one * (1.1f + near * 0.5f + Mathf.Sin(Time.time * (8f + near * 22f)) * 0.12f);
                }
                orbRings[i].Show(OnFloor(orbAt[i]), Quaternion.identity, near, RedMarker, false);
                if ((Time.frameCount + i * 2) % 6 == 0) Vfx.Play(VfxKind.Shock, orbAt[i], Vector3.up, 0.5f + near * 0.5f);
            }
        }

        // ------------------------------------------------------------------ the storm

        // Two opposite quarters of the floor, the one the player stands in and the one across from it, are marked,
        // and then crackle for a while: the fight goes on in the other two.
        private void BeginStorm()
        {
            Storms++;
            stormPair = target.position.x * target.position.z >= 0f ? 0 : 1;
            stormLeft = 0f;
            Enter(Mode.Storm);
            Vfx.Play(VfxKind.Danger, head.position + Vector3.up * 3f);
            if (shaker != null) shaker.Shake(0.25f, StormCast);
        }

        // Pair 0 is the north-east and south-west quarters, pair 1 the south-east and north-west.
        private float QuarterAngle(int which) => (stormPair == 0 ? 45f : 135f) + which * 180f;

        /// <summary>True when a point on the floor lies in a quarter the storm has marked or is burning.</summary>
        public bool InStorm(Vector3 point)
        {
            if (point.x * point.x + point.z * point.z > FloorRadius * FloorRadius) return false;
            float angle = Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
            return Mathf.Abs(Mathf.DeltaAngle(angle, QuarterAngle(0))) <= 45f || Mathf.Abs(Mathf.DeltaAngle(angle, QuarterAngle(1))) <= 45f;
        }

        private void AdvanceStormCast(float deltaTime)
        {
            Settle(deltaTime);
            // Reared up with his face to the sky and his jaws wide, calling it down.
            float shudder = Mathf.Sin(Time.time * 47f) * 0.08f;
            PoseHead(deltaTime, HeadHome() + new Vector3(shudder, 2.2f, -1.2f), Vector3.back, -52f, shudder * 25f, 6f, JawWide);
            if (elapsed < StormCast) return;
            stormLeft = StormSeconds;
            untilStormBurn = 0f;
            untilStormBolt = 0f;
            ScreenFlash.Flash(new Color(0.8f, 0.9f, 1f, 0.3f), 0.07f);
            if (shaker != null) shaker.Shake(0.5f, 0.4f);
            EndAttack();
        }

        private void AdvanceStorm(float deltaTime)
        {
            bool casting = mode == Mode.Storm;
            if (!casting && stormLeft <= 0f) return;
            if (!casting)
            {
                stormLeft -= deltaTime;
                if (IsDead || mode == Mode.Staggered) stormLeft = 0f;
                if (stormLeft <= 0f)
                {
                    foreach (var quarter in stormQuarters) quarter.Hide();
                    return;
                }
            }
            // While it is called the quarters fill; while it burns they beat; in its last second they gutter.
            float strength = casting ? 1f : stormLeft < 1f ? (Mathf.Repeat(Time.time * 12f, 1f) < 0.5f ? 0.9f : 0.3f) : 0.8f + 0.2f * Mathf.Sin(Time.time * 9f);
            for (int i = 0; i < stormQuarters.Length && i < 2; i++)
                stormQuarters[i].Show(Vector3.zero, Quaternion.Euler(0f, QuarterAngle(i), 0f), casting ? elapsed / StormCast : 1f, RedMarker, false, 1f, 1f, strength);
            if (casting) return;
            if (Time.frameCount % 3 == 0)
            {
                Vector3 spot = Polar(QuarterAngle(Time.frameCount % 2) + Random.Range(-42f, 42f), Random.Range(2f, FloorRadius - 1f), 0.3f);
                Vfx.Play(VfxKind.Shock, spot, Vector3.up, 0.8f);
            }
            untilStormBolt -= deltaTime;
            if (untilStormBolt <= 0f)
            {
                untilStormBolt = 0.55f;
                LightningBolts.Strike(Polar(QuarterAngle(Random.value < 0.5f ? 0 : 1) + Random.Range(-40f, 40f), Random.Range(3f, FloorRadius - 1.5f)), 0.25f);
            }
            untilStormBurn -= deltaTime;
            if (untilStormBurn > 0f || target == null || victim == null || !InStorm(target.position)) return;
            untilStormBurn = StormTick;
            Vector3 away = OnFloor(target.position);
            Deal(new Hit(StormDamage, 0f, away.sqrMagnitude > 0.01f ? away.normalized : Vector3.back, false, true, Vector3.zero), 0f);
        }

        // ------------------------------------------------------------------ the tail

        // The tail is a chain of bones on a spine. Its root never comes onto the floor: the body runs off the rim and
        // down under the outer cloud, and only the last of it is ever seen, so it reads as the end of the one long
        // serpent. It comes onto the floor tip first along a path, every joint passing where the one ahead passed,
        // and goes out the way it came.
        private bool EnsureTail()
        {
            if (tailBones == null) return false;
            tail.gameObject.SetActive(true);
            if (tailSpine == null)
            {
                // Made once, the first time the tail is wanted: the bones are measured when they first wake.
                var made = tailBones.Lengths;
                var drawnOut = new float[made.Length];
                tailJoints = 0f;
                for (int i = 0; i < made.Length; i++)
                {
                    drawnOut[i] = made[i] * (i < made.Length * TailPlainShare ? 1f : Mathf.Max(1f, TailStretch));
                    tailJoints += drawnOut[i];
                }
                tailSpine = new SerpentSpine(drawnOut);
                tailBones.Spine = tailSpine;
            }
            return true;
        }

        // The tail comes over the east or the west rim, whichever the player is nearer, toward where they stand.
        private void BeginTail(bool winds)
        {
            // It comes over the rim square to the line from the middle of the floor to the player, from one hand and
            // then the other, so every side of the summit is used; for a player in the middle, from the east or the west.
            BeginTailFrom(TailShape.Trail, winds, SideAngle());
            SeedTail();
        }

        // The tail is made ready to come over the rim so many degrees clockwise from north, to lie on the line a move
        // gives it. Whoever calls this lays it (SeedTail) once the move has said what its line is.
        private void BeginTailFrom(TailShape shape, bool winds, float angle, bool fromTheSouth = false)
        {
            EnsureTail();
            tailShape = shape;
            tailWinds = winds;
            tailOut = true;
            flopRaised = 0f;
            flopFront = -100f;
            flopBounce = -1f;
            TailLeastRoom = 100f;
            tailSpine.TipStiffness = 70f;
            if (TailFromDegrees < 360f) angle = TailFromDegrees;
            // Never out of the gate, where his head is, nor through a bay with a rope across it.
            float fromNorth = Mathf.DeltaAngle(0f, angle);
            if (Mathf.Abs(fromNorth) < TailNeverNorthOf) fromNorth = fromNorth >= 0f ? TailNeverNorthOf : -TailNeverNorthOf;
            // Where the posts are stubs, so a tail that lies out along the rim lies over them and not through them.
            if (fromTheSouth && Mathf.Abs(fromNorth) < 125f) fromNorth = fromNorth >= 0f ? 125f : -125f;
            // In through the middle of a bay, between two posts.
            float bay = 360f / Mathf.Max(1, GatePosts);
            float snapped = Mathf.Round(fromNorth / bay) * bay;
            if (Mathf.Abs(snapped) < TailNeverNorthOf) snapped += Mathf.Sign(fromNorth) * bay;
            if (fromTheSouth && Mathf.Abs(snapped) < 125f) snapped += Mathf.Sign(fromNorth) * bay;
            // Never up the steps at the south.
            if (Mathf.Abs(snapped) > TailNeverSouthOf) snapped = Mathf.Sign(snapped) * Mathf.Floor(TailNeverSouthOf / bay) * bay;
            tailEntry = Polar(snapped, FloorRadius);
            AimTail();
        }

        // Each frame, any joint that has come inside a post, his head or a forefoot is put out of it the shortest
        // way, so the body slides round what stands on the summit; what a blow lands on follows the same joints.
        private void AvoidThings()
        {
            FindThings();
            var points = tailSpine.Points;
            int last = points.Length - 1;
            // More than once: putting the lengths right after a joint is moved can carry its neighbour back in.
            for (int pass = 0; pass < 3; pass++)
            {
            for (int i = 1; i <= last; i++)
            {
                float body = Mathf.Lerp(1.5f, 0.5f, i / (float)last);
                Vector3 point = points[i];
                for (int k = 0; k < thingAt.Length; k++)
                {
                    if (point.y >= thingTop[k]) continue;
                    float x = point.x - thingAt[k].x, z = point.z - thingAt[k].z;
                    float need = thingRadius[k] + body;
                    float away = x * x + z * z;
                    if (away >= need * need) continue;
                    away = Mathf.Sqrt(away);
                    if (away < 0.001f)
                    {
                        x = 1f;
                        z = 0f;
                        away = 1f;
                    }
                    point.x = thingAt[k].x + x / away * need;
                    point.z = thingAt[k].z + z / away * need;
                }
                points[i] = point;
            }
            tailSpine.Settle();
            }
            // Putting the lengths right can carry a joint back in: how near the body came to going into anything is kept for whoever tests it.
            for (int i = 1; i <= last; i++)
            {
                float body = Mathf.Lerp(1.5f, 0.5f, i / (float)last);
                for (int k = 0; k < thingAt.Length; k++)
                {
                    if (points[i].y >= thingTop[k]) continue;
                    float x = points[i].x - thingAt[k].x, z = points[i].z - thingAt[k].z;
                    float room = Mathf.Sqrt(x * x + z * z) - thingRadius[k] - body;
                    if (room < TailLeastRoom) TailLeastRoom = room;
                }
            }
        }

        // Where his head and forefeet are now; the posts do not move.
        private void FindThings()
        {
            thingAt[GatePosts] = OnFloor(head.position + head.forward * 1.2f);
            thingAt[GatePosts + 1] = OnFloor(leftClaw.position);
            thingAt[GatePosts + 2] = OnFloor(rightClaw.position);
        }

        // Through the player, but never so far round that it lies along the rim.
        private void AimTail()
        {
            float inward = YawOf(-tailEntry);
            float toPlayer = Mathf.DeltaAngle(inward, YawOf(OnFloor(target.position) - tailEntry));
            tailAlong = Quaternion.Euler(0f, inward + Mathf.Clamp(toPlayer, -60f, 60f), 0f) * Vector3.forward;
            slitherThrough = Vector3.Dot(OnFloor(target.position) - tailEntry, tailAlong);
            tailChord = Chord(tailEntry + tailAlong * 0.05f, tailAlong, FloorRadius) + 0.05f;
            // The slither crosses the whole floor, as far as the tail will go with its root still off it; the flop comes part of the way.
            tailReach = tailWinds ? Mathf.Min(tailChord + 1f, tailJoints - TailRootKept) : Mathf.Min(FlopReach, tailChord - 2f, tailJoints - TailRootKept);
            // Where its way would leave the floor between the tall posts, under their rope, its tip stops short of them: it goes out over the far rim only where the posts are stubs it can lie over.
            if (Mathf.Abs(YawOf(tailEntry + tailAlong * tailChord)) < TallPostsWithin + 8f) tailReach = Mathf.Min(tailReach, tailChord - PostRing - PostRadius - 1.6f);
        }

        // Laid afresh with its tip under the cloud outside the rim and the rest of it straight on down behind.
        private void SeedTail()
        {
            tailLead = -5f;
            Vector3 tip = TailPath(tailLead);
            tailSpine.Seed(tip, TailPath(tailLead - 1f) - tip);
            if (tailShape == TailShape.Trail) return;
            LayOnCurve();
            tailSpine.Snap();
        }

        private static float YawOf(Vector3 direction) => Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        /// <summary>
        /// The way the tail takes, by metres past the rim: before the rim, up from under the cloud in a curve no
        /// joint has to bend far for; on the floor, straight for the flop or winding from side to side for the slither.
        /// </summary>
        private Vector3 TailPath(float along)
        {
            Vector3 at = TailLine(along);
            // Where it crosses the rim it is carried over the balustrade, not through it; and wherever it lies out
            // past the rim (the scythe at either end of its sweep, the slither's far end) it lies over the rail.
            if (along < 0f)
            {
                if (tailShape != TailShape.Scythe) at.y += RimLift * (1f - Mathf.SmoothStep(0f, 1f, -along / 4f));
                return at;
            }
            float beyond = OnFloor(at).magnitude - (FloorRadius - RimLiftWithin);
            if (beyond > 0f) at.y = Mathf.Max(at.y, RimLift * Mathf.SmoothStep(0f, 1f, beyond / RimLiftWithin));
            return at;
        }

        private Vector3 TailLine(float along)
        {
            if (along < 0f)
            {
                float outside = -along;
                // Steeply down once it is clear of the rim: its root end has to be under the cloud, never seen.
                float drop = outside < 4f ? 0.15f * outside * outside : 2.4f + (outside - 4f) * 3.2f;
                // The scythe comes over the rim arched up, and that is given up on the way down.
                float arch = tailShape == TailShape.Scythe ? ScytheArch * (1f - Mathf.SmoothStep(0f, 1f, outside / 5f)) : 0f;
                return tailEntry + tailAlong * along + Vector3.down * (drop - arch);
            }
            switch (tailShape)
            {
                case TailShape.Coil:
                    return TailShapes.Coil(tailEntry, coilCentre, coilRadius, coilSense, along, Mathf.Min(1.6f, coilRadius * 0.3f));
                case TailShape.Scythe:
                {
                    // Every stretch of it is where the root was a little while ago, later the further out: so it bends, and the tip whips.
                    float yaw = TailShapes.Sweep(scytheClock - ScytheLag * along / Mathf.Max(1f, tailReach), scytheFrom, scytheTo, ScytheFirst, ScythePause, ScytheSecond);
                    Vector3 at = tailEntry + Facing(yaw) * along;
                    at.y = ScytheArch * (1f - Mathf.SmoothStep(0f, 1f, (along - 7f) / 6f));
                    return at;
                }
                case TailShape.Sting:
                    return TailShapes.Sting(tailEntry, stingJoint, stingArch, stingTip, along);
                case TailShape.Lane:
                    return tailEntry + tailAlong * along;
            }
            if (!tailWinds) return tailEntry + tailAlong * along;
            if (along > tailChord)
            {
                // Over the far rim and down into the cloud again.
                float beyond = along - tailChord;
                return tailEntry + tailAlong * along + Vector3.down * (beyond < 4f ? 0.15f * beyond * beyond : 2.4f + (beyond - 4f) * 2.4f);
            }
            var side = new Vector3(-tailAlong.z, 0f, tailAlong.x);
            // It winds about the line through where the player stood, crosses that line where they stood, and straightens at either rim.
            float wind = SlitherWidth * Mathf.SmoothStep(0f, 1f, along / 5f) * Mathf.SmoothStep(0f, 1f, (tailChord - along) / 5f)
                * Mathf.Sin((along - slitherThrough) / SlitherWave * Mathf.PI * 2f);
            return tailEntry + tailAlong * along + side * wind;
        }

        // The tip goes on along its path to so many metres past the rim, laying its trail in short steps.
        private void LeadTail(float to)
        {
            while (tailLead < to)
            {
                tailLead = Mathf.Min(to, tailLead + 0.25f);
                tailSpine.Lead(TailPath(tailLead));
            }
        }

        // Drawn back out along its own trail. True when the last of it is off the floor and under the cloud.
        private bool WithdrawTail(float deltaTime)
        {
            float back = (Mathf.Max(tailReach, 14f) + 6f) / Mathf.Max(0.1f, TailSlide) * deltaTime;
            if (tailShape == TailShape.Trail) tailSpine.Retreat(back);
            tailLead -= back;
            if (tailLead > -6f) return false;
            ParkTail();
            return true;
        }

        private void ParkTail()
        {
            tailOut = false;
            if (ribbonMark != null) ribbonMark.Hide();
            if (tailPart != null) tailPart.SetVulnerable(false);
            if (tail != null) tail.gameObject.SetActive(false);
        }

        // Every frame the tail is out: where the trail puts each joint, lifted where it is flopping, a wave run down
        // it toward the tip, and then the springs, which are what make the tip come late and go past.
        private void MoveTail(float deltaTime)
        {
            if (!tailOut || tailSpine == null) return;
            if (tailShape == TailShape.Trail) tailSpine.PlaceAlongTrail();
            else LayOnCurve();
            var targets = tailSpine.Targets;
            int last = tailSpine.Count - 1;
            if (flopRaised > 0f || flopBounce >= 0f)
            {
                float along = tailLead;
                for (int i = last; i >= 0 && along > 0f; i--)
                {
                    targets[i].y += FlopLift(along);
                    if (i > 0) along -= tailSpine.Stretch(i - 1);
                }
            }
            tailSpine.Wave(Time.time, tailWave, tailWaveLength, tailWaveSpeed, 2f);
            tailSpine.Step(deltaTime);
            AvoidThings();
        }

        // How high a joint so far past the rim is held: an arch that climbs toward the tip while the tail hangs over
        // its lane, let go from the root end as the smack runs down it, and one low bounce of the tip after.
        private float FlopLift(float along)
        {
            float share = Mathf.Clamp01(along / Mathf.Max(1f, tailReach));
            float hang = FlopHeight * Mathf.Pow(Mathf.Sin(share * Mathf.PI * 0.5f), 1.3f) * flopRaised;
            float held = Mathf.SmoothStep(0f, 1f, (along - flopFront + 1.5f) / 3f);
            float bounce = flopBounce >= 0f ? FlopBounce * Mathf.Sin(Mathf.Clamp01(flopBounce) * Mathf.PI) * share * share : 0f;
            return hang * held + bounce;
        }

        // True when a point on the floor is under the tail as it lies now: the shape is the chain itself, joint to
        // joint, and the fan beyond the last of them.
        private bool TailTouches(Vector3 point, float reach)
        {
            var points = tailSpine.Points;
            int last = tailSpine.Count - 1;
            Vector3 flat = OnFloor(point);
            float along = tailLead;
            for (int i = last; i > 0 && along > -1f; i--)
            {
                if (points[i].y < 2.2f && FromSegment(flat, OnFloor(points[i - 1]), OnFloor(points[i])) <= reach) return true;
                along -= tailSpine.Stretch(i - 1);
            }
            Vector3 fan = points[last] + (points[last] - points[last - 1]).normalized * TailFan;
            return points[last].y < 2.2f && FromSegment(flat, OnFloor(points[last]), OnFloor(fan)) <= reach + 0.4f;
        }

        private static float FromSegment(Vector3 point, Vector3 from, Vector3 to)
        {
            Vector3 along = to - from;
            float length = along.sqrMagnitude;
            float t = length > 0.0001f ? Mathf.Clamp01(Vector3.Dot(point - from, along) / length) : 0f;
            return (point - (from + along * t)).magnitude;
        }

        // The slither: the tail winds onto the floor along a marked path and strikes whoever is on it as it passes.
        private void BeginTailSweep()
        {
            TailSweeps++;
            sweepStruck = false;
            BeginTail(true);
            Enter(Mode.TailSweep);
            Vfx.Play(VfxKind.Danger, tailEntry * 0.85f + Vector3.up * 2.5f);
        }

        private void AdvanceTailSweep(float deltaTime)
        {
            Settle(deltaTime);
            FaceTarget(deltaTime, 4f, JawSnarl);
            float strikeEnds = TailSweepTelegraph + TailSweepSeconds;
            float all = tailReach + TailFan;
            if (elapsed < TailSweepTelegraph)
            {
                // The path follows the player until just before the tail comes: only its tip is over the rim yet, feeling about.
                float peek = Mathf.Lerp(-1.5f, 2.5f, Mathf.SmoothStep(0f, 1f, elapsed / 0.5f));
                if (elapsed < TailSweepTelegraph - SlitherLock)
                {
                    AimTail();
                    SeedTail();
                }
                LeadTail(peek);
                tailWave = 0.5f;
                tailWaveLength = 6f;
                tailWaveSpeed = 11f;
                // Its way draws itself out from the rim to the far side: a winding lane as wide as the body.
                float shown = elapsed / TailSweepTelegraph;
                DrawRibbon(all * Mathf.Clamp01(shown * 1.7f));
                ribbonMark.Show(Vector3.zero, Quaternion.identity, 1f, RedMarker, true, 1f, 1f, 0.55f + 0.45f * shown);
            }
            else if (elapsed < strikeEnds)
            {
                float t = (elapsed - TailSweepTelegraph) / TailSweepSeconds;
                // Slow to start and then all at once, as a snake strikes.
                LeadTail(Mathf.Lerp(2.5f, tailReach, t * t * (3f - 2f * t)));
                tailWave = 0.45f;
                tailWaveLength = 7f;
                tailWaveSpeed = 16f;
                if (Time.frameCount % 2 == 0) Vfx.Play(VfxKind.Dust, OnFloor(tailSpine.LeadPoint) + Vector3.up * 0.1f, Vector3.up);
                if (!sweepStruck && target != null && victim != null && TailTouches(target.position, TailHalfWidth + 0.35f))
                {
                    sweepStruck = true;
                    Deal(new Hit(TailSweepDamage, 0f, tailAlong, false, true, tailEntry), PerfectGuardBalance);
                    if (mode != Mode.TailSweep) return;
                }
            }
            else if (elapsed < strikeEnds + TailSweepRest)
            {
                ribbonMark.Hide();
                tailWave = 0.2f;
                tailWaveSpeed = 4f;
            }
            else if (WithdrawTail(deltaTime)) EndAttack();
        }

        // The flop: the tail comes over the rim arched up off the floor, hangs over its lane, and smacks down along
        // it from the root end to the tip, as a fish's tail does; then it lies slack, and can be cut.
        private void BeginTailSlam()
        {
            TailSlams++;
            slamLanded = false;
            slamStruck = false;
            BeginTail(false);
            flopRaised = 1f;
            Enter(Mode.TailSlam);
            Vfx.Play(VfxKind.Glint, tailEntry * 0.9f + Vector3.up * 3f, Vector3.up, 2f);
        }

        /// <summary>When, in the flop's own seconds, the smack reaches a point in its lane: the root end lands first and the tip last.</summary>
        public float TailSlamLandsAt(Vector3 point)
        {
            float along = Mathf.Clamp(Vector3.Dot(OnFloor(point) - tailEntry, tailAlong), 0f, tailReach + TailFan);
            return TailSlamTelegraph + TailSlamDrop * (along + 1.5f) / (tailReach + TailFan + 3f);
        }

        private void AdvanceTailSlam(float deltaTime)
        {
            Settle(deltaTime);
            FaceTarget(deltaTime, 4f, JawSnarl);
            float all = tailReach + TailFan;
            float fall = TailSlamTelegraph + TailSlamDrop;
            if (elapsed < TailSlamTelegraph)
            {
                // Out over the rim with its tip climbing, and then hanging there, swaying, over the lane it will fall on.
                LeadTail(Mathf.Lerp(-5f, tailReach, Mathf.SmoothStep(0f, 1f, elapsed / TailSlamExtend)));
                tailWave = 0.9f;
                tailWaveLength = 10f;
                tailWaveSpeed = 5f;
                tailSlamMark.Show(tailEntry, Quaternion.LookRotation(tailAlong), elapsed / TailSlamTelegraph, PlainMarker, true, 1f, all);
            }
            else if (elapsed < fall)
            {
                // Let go from the root end: the ground comes up the tail to the tip.
                float t = (elapsed - TailSlamTelegraph) / TailSlamDrop;
                flopFront = Mathf.Lerp(-1.5f, all + 1.5f, t);
                tailWave = 0.2f;
                if (Time.frameCount % 2 == 0) Vfx.Play(VfxKind.Dust, tailEntry + tailAlong * Mathf.Clamp(flopFront, 0f, all), Vector3.up);
                if (!slamStruck && target != null)
                {
                    Vector3 offset = OnFloor(target.position) - tailEntry;
                    float along = Vector3.Dot(offset, tailAlong);
                    float across = (offset - tailAlong * along).magnitude;
                    if (along >= -0.5f && along <= all + 0.3f && across <= TailSlamWidth * 0.5f + 0.3f && flopFront >= along)
                    {
                        slamStruck = true;
                        // It comes from its root: the guard has to face that way.
                        Deal(new Hit(TailSlamDamage, 0f, tailAlong), PerfectGuardBalance);
                        if (mode != Mode.TailSlam) return;
                    }
                }
            }
            else if (!slamLanded)
            {
                slamLanded = true;
                flopRaised = 0f;
                flopFront = -100f;
                flopBounce = 0f;
                tailSlamMark.Hide();
                LandEffects(tailEntry + tailAlong * (all * 0.8f), 2.5f);
                Vfx.Play(VfxKind.Impact, tailEntry + tailAlong * (all * 0.4f) + Vector3.up * 0.6f);
                for (float at = 2f; at < all; at += 3f)
                    Vfx.Play(VfxKind.Dust, tailEntry + tailAlong * at + new Vector3(-tailAlong.z, 0f, tailAlong.x) * (at % 2f < 1f ? 1.8f : -1.8f), Vector3.up);
                if (shaker != null) shaker.Shake(0.8f, 0.4f);
                tailPart.SetVulnerable(true);
            }
            else if (elapsed < fall + TailSlamRest)
            {
                // The tip bounces once, and then it lies slack where it fell, breathing: this is the opening. It
                // stirs harder as the moment runs out.
                if (flopBounce >= 0f)
                {
                    flopBounce += deltaTime / 0.32f;
                    if (flopBounce >= 1f) flopBounce = -1f;
                }
                float pulling = Mathf.Clamp01((elapsed - fall) / TailSlamRest);
                // Slack is never straight: it lies in a slow curve.
                tailWave = 0.7f + 0.5f * pulling * pulling;
                tailWaveLength = 9f;
                tailWaveSpeed = 2f + 9f * pulling * pulling;
            }
            else
            {
                tailPart.SetVulnerable(false);
                if (WithdrawTail(deltaTime)) EndAttack();
            }
        }

        // ------------------------------------------------------------------ the tail's other moves

        // Each of these lays the tail on a line of its own that changes as the move goes on (TailPath), and the spine's
        // springs do the rest: no part of it is ever turned as one stiff piece.

        // Lays every joint on the tail's line, the tip where it has come to and each joint its own stretch behind the
        // one ahead, feeling back along the line in short steps.
        private void LayOnCurve()
        {
            var targets = tailSpine.Targets;
            int last = tailSpine.Count - 1;
            float at = tailLead;
            targets[last] = TailPath(at);
            for (int i = last - 1; i >= 0; i--)
            {
                float need = tailSpine.Stretch(i);
                Vector3 ahead = targets[i + 1];
                float nearAway = 0f;
                for (int step = 0; step < 40; step++)
                {
                    float farAt = at - 0.4f;
                    float farAway = (TailPath(farAt) - ahead).magnitude;
                    if (farAway >= need)
                    {
                        at = Mathf.Lerp(at, farAt, Mathf.InverseLerp(nearAway, farAway, need));
                        break;
                    }
                    at = farAt;
                    nearAway = farAway;
                }
                targets[i] = TailPath(at);
            }
        }

        // Where on the rim the tail comes from: to one hand of the player and then the other; the rim nearest them; the rim furthest from them.
        private float SideAngle()
        {
            tailTurns++;
            Vector3 stands = OnFloor(target.position);
            float hand = tailTurns % 2 == 0 ? 90f : -90f;
            return stands.magnitude > 3f ? YawOf(stands) + hand : hand;
        }

        private float NearAngle()
        {
            tailTurns++;
            Vector3 stands = OnFloor(target.position);
            return stands.magnitude > 3f ? YawOf(stands) : tailTurns % 2 == 0 ? 90f : -90f;
        }

        private static Vector3 Facing(float yaw) => Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

        private float FlatDistance(Vector3 from, Vector3 to) => OnFloor(from - to).magnitude;

        // The coil: the tail comes over the nearest rim, hooks round where the player stands and curls shut on them.
        // Whoever is inside when it closes is crushed, and its body burns whoever walks into it while it tightens:
        // the way out is the gap before it closes, or a roll through the body. Cut enough while it lies there, it
        // springs open and his balance pays for it.
        private void BeginTailCoil()
        {
            TailCoils++;
            coilCrushed = coilTouched = coilSprung = false;
            coilHurt = 0f;
            BeginTailFrom(TailShape.Coil, false, NearAngle());
            coilSense = tailTurns % 2 == 0 ? 1f : -1f;
            AimCoil();
            SeedTail();
            Enter(Mode.TailCoil);
            Vfx.Play(VfxKind.Danger, tailEntry * 0.85f + Vector3.up * 2.5f);
        }

        private void AimCoil()
        {
            coilRadius = CoilRadius;
            Vector3 stands = Within(target.position, FloorRadius - 2f);
            // Never so near the rim it comes over that there is no room to hook round.
            Vector3 from = stands - tailEntry;
            float least = CoilRadius + 2f;
            if (from.magnitude < least) stands = Within(tailEntry + (from.sqrMagnitude > 0.01f ? from.normalized : -tailEntry.normalized) * least, FloorRadius - 2f);
            // The ring is laid where it goes round nothing that stands tall: clear of the tall posts, his head and his forefeet. It is drawn on the floor, so the player sees where it will close.
            FindThings();
            for (int pass = 0; pass < 2; pass++)
                for (int k = 0; k < thingAt.Length; k++)
                {
                    if (thingTop[k] < 50f) continue;
                    Vector3 off = OnFloor(stands - thingAt[k]);
                    float need = CoilRadius + thingRadius[k] + 1.3f;
                    if (off.magnitude >= need) continue;
                    stands = thingAt[k] + (off.sqrMagnitude > 0.01f ? off.normalized : -thingAt[k].normalized) * need;
                    stands.y = 0f;
                }
            coilCentre = stands;
            CoilHeading();
            tailReach = Mathf.Min(TailShapes.CoilLine(tailEntry, coilCentre, CoilRadius) + CoilRadius * 5.6f, tailJoints - TailRootKept);
        }

        // As the ring draws tight the tail is drawn back out over the rim, as a noose is pulled: it goes once round and a little more, never into a heap.
        private float CoilDrawn() => Mathf.Min(tailReach, TailShapes.CoilLine(tailEntry, coilCentre, coilRadius) + coilRadius * CoilWrap * Mathf.Deg2Rad);

        private void CoilHeading()
        {
            Vector3 first = TailShapes.Coil(tailEntry, coilCentre, coilRadius, coilSense, 1f) - tailEntry;
            if (first.sqrMagnitude > 0.0001f) tailAlong = first.normalized;
        }

        private void AdvanceTailCoil(float deltaTime)
        {
            Settle(deltaTime);
            FaceTarget(deltaTime, 4f, JawSnarl);
            float squeezed = CoilTelegraph + CoilSqueeze;
            var coilMark = slitherMarks[0];
            if (coilSprung)
            {
                // Cut open: it flies wide, thrashing, and is dragged off.
                coilSprungFor += deltaTime;
                coilRadius = Mathf.Lerp(coilRadius, CoilRadius * 1.7f, 1f - Mathf.Exp(-9f * deltaTime));
                CoilHeading();
                tailWave = 1.4f;
                tailWaveLength = 7f;
                tailWaveSpeed = 18f;
                if (coilSprungFor > 0.7f && WithdrawTail(deltaTime)) EndAttack();
                return;
            }
            if (elapsed < CoilTelegraph)
            {
                // The ring follows the player while the tip runs out, and is fixed before it closes.
                if (elapsed < CoilTelegraph * 0.5f) AimCoil();
                tailLead = Mathf.Lerp(-5f, tailReach, Mathf.SmoothStep(0f, 1f, elapsed / (CoilTelegraph * 0.85f)));
                tailWave = 0.4f;
                tailWaveLength = 7f;
                tailWaveSpeed = 12f;
                coilMark.Show(coilCentre, Quaternion.identity, elapsed / CoilTelegraph, RedMarker, false, coilRadius + 0.8f);
                if (Time.frameCount % 2 == 0) Vfx.Play(VfxKind.Dust, OnFloor(tailSpine.Points[tailSpine.Count - 1]) + Vector3.up * 0.1f, Vector3.up);
            }
            else if (elapsed < squeezed)
            {
                float t = (elapsed - CoilTelegraph) / CoilSqueeze;
                coilRadius = Mathf.Lerp(CoilRadius, CoilTight, t * t);
                CoilHeading();
                tailLead = CoilDrawn();
                tailWave = 0.15f;
                tailPart.SetVulnerable(true);
                coilMark.Show(coilCentre, Quaternion.identity, 1f, RedMarker, false, coilRadius + 0.8f);
                // The body burns whoever walks into it from outside. Whoever is where it will close is left for the
                // crush, which a guard at the last instant turns: the touch never comes first and takes that away.
                if (!coilTouched && target != null && victim != null && FlatDistance(target.position, coilCentre) > CoilTight + 1.3f && TailTouches(target.position, TailHalfWidth * 0.7f))
                {
                    coilTouched = true;
                    Vector3 away = OnFloor(target.position) - coilCentre;
                    Deal(new Hit(CoilTouchDamage, 0f, away.sqrMagnitude > 0.01f ? away.normalized : tailAlong, false, true, OnFloor(target.position)), 0f);
                    if (mode != Mode.TailCoil) return;
                }
            }
            else if (!coilCrushed)
            {
                coilCrushed = true;
                coilRadius = CoilTight;
                coilMark.Hide();
                LandEffects(coilCentre, CoilTight);
                if (target != null && victim != null && FlatDistance(target.position, coilCentre) <= CoilTight + 1.3f)
                {
                    // It closes from every side at once: a guard at the last instant turns it whichever way the player faces.
                    Deal(new Hit(CoilDamage, 0f, tailAlong, false, true, OnFloor(target.position)), PerfectGuardBalance);
                    if (mode != Mode.TailCoil) return;
                }
            }
            else if (elapsed < squeezed + CoilHold)
            {
                tailWave = 0.1f;
            }
            else if (elapsed < squeezed + CoilHold + CoilRest)
            {
                // It lets go and lies loose in its ring: this is the opening.
                coilRadius = Mathf.Lerp(coilRadius, CoilTight * 1.8f, 1f - Mathf.Exp(-3f * deltaTime));
                CoilHeading();
                tailLead = CoilDrawn();
                float pulling = Mathf.Clamp01((elapsed - squeezed - CoilHold) / CoilRest);
                tailWave = 0.5f + 0.5f * pulling * pulling;
                tailWaveLength = 9f;
                tailWaveSpeed = 2f + 9f * pulling * pulling;
            }
            else
            {
                tailPart.SetVulnerable(false);
                if (WithdrawTail(deltaTime)) EndAttack();
            }
        }

        // The coil has been cut through: it flies open, and the dragon's balance takes what a turned blow would give.
        private void SpringCoil()
        {
            coilSprung = true;
            coilSprungFor = 0f;
            CoilsSprung++;
            tailPart.SetVulnerable(false);
            slitherMarks[0].Hide();
            Vfx.Play(VfxKind.Ring, coilCentre + Vector3.up * 1.5f, Vector3.up);
            Vfx.Play(VfxKind.Burst, coilCentre + Vector3.up * 1f);
            if (shaker != null) shaker.Shake(0.5f, 0.3f);
            if (Stagger.Add(PerfectGuardBalance)) BeginStagger();
        }

        // The scythe: the tail lies in over the rim furthest from the player, arched up where it comes over, and
        // sweeps round the whole floor like the hand of a clock, but bending: every stretch of it comes later than the
        // one nearer the root, so the tip trails and then whips through. It waits, and comes back faster. Roll
        // through the body, turn it with a guard at the last instant, or stand under the arch by its root.
        // How far to one hand of straight inward the scythe may swing: all the way if it then lies out over the rim among the stubs, less if that would lay it among the tall posts.
        private float ScytheSpreadToward(float inward, float hand)
        {
            Vector3 way = Facing(inward + hand * ScytheSpread);
            float across = Chord(tailEntry + way * 0.05f, way, FloorRadius);
            return Mathf.Abs(YawOf(tailEntry + way * across)) >= TallPostsWithin + 8f ? ScytheSpread : ScytheSpreadNorth;
        }

        private void BeginTailScythe()
        {
            TailScythes++;
            scythePass = 0;
            scytheStruck = false;
            scytheClock = -1f;
            BeginTailFrom(TailShape.Scythe, false, NearAngle() + 180f, true);
            float inward = YawOf(-tailEntry);
            float toPlayer = Mathf.DeltaAngle(inward, YawOf(OnFloor(target.position) - tailEntry));
            // It begins on the side the player is not on, so they see it come.
            float side = Mathf.Abs(toPlayer) > 3f ? Mathf.Sign(toPlayer) : tailTurns % 2 == 0 ? 1f : -1f;
            // Toward the south it lies out along the rim over the stubs; toward the tall posts and their rope it stops while all of it is still on the floor.
            scytheFrom = inward - side * ScytheSpreadToward(inward, -side);
            scytheTo = inward + side * ScytheSpreadToward(inward, side);
            tailAlong = Facing(scytheFrom);
            // No longer than the floor is wide where it stops short of the tall posts, so its tip never swings through them.
            tailReach = Mathf.Min(tailJoints - ScytheRootKept, 2f * FloorRadius * Mathf.Cos(ScytheSpreadNorth * Mathf.Deg2Rad) - PostRing - PostRadius - 1.6f);
            SeedTail();
            Enter(Mode.TailScythe);
            Vfx.Play(VfxKind.Danger, tailEntry * 0.85f + Vector3.up * 2.5f);
        }

        private void AdvanceTailScythe(float deltaTime)
        {
            Settle(deltaTime);
            FaceTarget(deltaTime, 4f, JawSnarl);
            var wideMark = slitherMarks[2];
            float whole = ScytheFirst + ScythePause + ScytheSecond + ScytheLag;
            if (elapsed < ScytheTelegraph)
            {
                scytheClock = -1f;
                tailLead = Mathf.Lerp(-5f, tailReach, Mathf.SmoothStep(0f, 1f, elapsed / (ScytheTelegraph * 0.75f)));
                tailWave = 0.7f;
                tailWaveLength = 9f;
                tailWaveSpeed = 9f;
                // The whole floor is its mark: there is nowhere on it the tail does not pass but under its root.
                wideMark.Show(Vector3.zero, Quaternion.identity, elapsed / ScytheTelegraph, RedMarker, false, FloorRadius - 0.4f, 1f, 0.7f);
                return;
            }
            scytheClock = elapsed - ScytheTelegraph;
            tailAlong = Facing(TailShapes.Sweep(scytheClock, scytheFrom, scytheTo, ScytheFirst, ScythePause, ScytheSecond));
            if (scytheClock < whole)
            {
                tailLead = tailReach;
                tailWave = 0.25f;
                tailWaveLength = 8f;
                tailWaveSpeed = 14f;
                wideMark.Show(Vector3.zero, Quaternion.identity, 1f, RedMarker, false, FloorRadius - 0.4f, 1f, 0.45f);
                int pass = scytheClock < ScytheFirst + ScythePause ? 0 : 1;
                if (pass != scythePass)
                {
                    scythePass = pass;
                    scytheStruck = false;
                }
                bool sweeping = pass == 0 ? scytheClock < ScytheFirst + ScytheLag + 0.1f : true;
                if (Time.frameCount % 2 == 0 && sweeping) Vfx.Play(VfxKind.Dust, OnFloor(tailSpine.Points[tailSpine.Count - 1]) + Vector3.up * 0.1f, Vector3.up);
                if (sweeping && !scytheStruck && target != null && victim != null && TailTouches(target.position, TailHalfWidth + 0.3f))
                {
                    scytheStruck = true;
                    // It comes from the side it is sweeping from: the guard has to face that way.
                    Vector3 reach = OnFloor(target.position) - tailEntry;
                    float way = Mathf.Sign(scytheTo - scytheFrom) * (pass == 0 ? 1f : -1f);
                    Vector3 going = new Vector3(reach.z, 0f, -reach.x).normalized * way;
                    Deal(new Hit(ScytheDamage, 0f, going, false, true, OnFloor(target.position) - going * 3f), PerfectGuardBalance * 0.5f);
                    if (mode != Mode.TailScythe) return;
                }
            }
            else if (scytheClock < whole + ScytheRest)
            {
                wideMark.Hide();
                tailWave = 0.3f;
                tailWaveSpeed = 4f;
            }
            else if (WithdrawTail(deltaTime)) EndAttack();
        }

        // The walking flops: three fish-tail smacks one after another, each lane a step round from the last toward
        // where the player has gone, and from each a ripple of lightning runs out along the stone to either side.
        // After the third it lies slack and can be cut.
        private void BeginTailFlops()
        {
            TailFlopRuns++;
            slamStruck = false;
            BeginTailFrom(TailShape.Lane, false, SideAngle());
            tailReach = Mathf.Min(tailChord - 1f, tailJoints - TailRootKept);
            flopsDone = 0;
            flopsClock = 0f;
            flopRaised = 1f;
            flopsYawFrom = flopsYawTo = YawOf(tailAlong);
            flopsWay = Mathf.DeltaAngle(YawOf(-tailEntry), flopsYawTo) > 0f ? -1f : 1f;
            SeedTail();
            Enter(Mode.TailFlops);
            Vfx.Play(VfxKind.Glint, tailEntry * 0.9f + Vector3.up * 3f, Vector3.up, 2f);
        }

        private void AdvanceTailFlops(float deltaTime)
        {
            Settle(deltaTime);
            FaceTarget(deltaTime, 4f, JawSnarl);
            flopsClock += deltaTime * Pace;
            if (flopBounce >= 0f)
            {
                flopBounce += deltaTime / 0.32f;
                if (flopBounce >= 1f) flopBounce = -1f;
            }
            float all = tailReach + TailFan;
            if (flopsDone >= FlopsCount)
            {
                if (flopsClock < FlopsRest)
                {
                    float pulling = Mathf.Clamp01(flopsClock / FlopsRest);
                    tailWave = 0.7f + 0.5f * pulling * pulling;
                    tailWaveLength = 9f;
                    tailWaveSpeed = 2f + 9f * pulling * pulling;
                }
                else
                {
                    tailPart.SetVulnerable(false);
                    if (WithdrawTail(deltaTime)) EndAttack();
                }
                return;
            }
            float rise = flopsDone == 0 ? FlopsFirst : FlopsNext;
            if (flopsClock < rise)
            {
                // Up off the stone and over to its next lane, swaying, the root end leading and the tip following.
                float t = flopsClock / rise;
                tailLead = flopsDone == 0 ? Mathf.Lerp(-5f, tailReach, Mathf.SmoothStep(0f, 1f, flopsClock / TailSlamExtend)) : Mathf.MoveTowards(tailLead, tailReach, 20f * deltaTime);
                flopRaised = Mathf.MoveTowards(flopRaised, 1f, deltaTime * 4f);
                flopFront = -100f;
                tailAlong = Facing(Mathf.LerpAngle(flopsYawFrom, flopsYawTo, Mathf.SmoothStep(0f, 1f, t / 0.65f)));
                tailWave = 0.9f;
                tailWaveLength = 10f;
                tailWaveSpeed = 5f;
                tailSlamMark.Show(tailEntry, Quaternion.Euler(0f, flopsYawTo, 0f), t, PlainMarker, true, 1f, all);
            }
            else if (flopsClock < rise + TailSlamDrop)
            {
                float t = (flopsClock - rise) / TailSlamDrop;
                flopFront = Mathf.Lerp(-1.5f, all + 1.5f, t);
                tailWave = 0.2f;
                if (!slamStruck && target != null)
                {
                    Vector3 offset = OnFloor(target.position) - tailEntry;
                    float along = Vector3.Dot(offset, tailAlong);
                    float across = (offset - tailAlong * along).magnitude;
                    if (along >= -0.5f && along <= all + 0.3f && across <= TailSlamWidth * 0.5f + 0.3f && flopFront >= along)
                    {
                        slamStruck = true;
                        Deal(new Hit(FlopsDamage, 0f, tailAlong), PerfectGuardBalance * 0.5f);
                        if (mode != Mode.TailFlops) return;
                    }
                }
            }
            else
            {
                flopRaised = 0f;
                flopFront = -100f;
                flopBounce = 0f;
                tailSlamMark.Hide();
                LandEffects(tailEntry + tailAlong * (all * 0.8f), 2.5f);
                Vfx.Play(VfxKind.Impact, tailEntry + tailAlong * (all * 0.4f) + Vector3.up * 0.6f);
                Ripple(all);
                flopsDone++;
                flopsClock = 0f;
                slamStruck = false;
                if (flopsDone >= FlopsCount)
                {
                    tailPart.SetVulnerable(true);
                    return;
                }
                // The next lane: a step round toward the player, or onto them if they are nearer than a step; if
                // they have not moved, on across the floor the way it was going.
                float inward = YawOf(-tailEntry);
                float now = Mathf.DeltaAngle(inward, YawOf(tailAlong));
                float toPlayer = Mathf.DeltaAngle(inward, YawOf(OnFloor(target.position) - tailEntry)) - now;
                if (Mathf.Abs(toPlayer) > 4f) flopsWay = Mathf.Sign(toPlayer);
                float next = Mathf.Clamp(now + flopsWay * FlopsStep, -62f, 62f);
                if (Mathf.Abs(next - now) < 8f)
                {
                    flopsWay = -flopsWay;
                    next = Mathf.Clamp(now + flopsWay * FlopsStep, -62f, 62f);
                }
                if (Mathf.Abs(toPlayer) > 4f && Mathf.Abs(toPlayer) < FlopsStep) next = Mathf.Clamp(now + toPlayer, -62f, 62f);
                flopsYawFrom = inward + now;
                flopsYawTo = inward + next;
                Vector3 lane = Facing(flopsYawTo);
                tailReach = Mathf.Min(Chord(tailEntry + lane * 0.05f, lane, FloorRadius) - 1f, tailJoints - TailRootKept);
            }
        }

        // Lightning runs out along the stone from where the tail has landed: two rows of bolts either side of its lane, the nearest first.
        private void Ripple(float all)
        {
            var side = new Vector3(-tailAlong.z, 0f, tailAlong.x);
            for (int row = 0; row < 2; row++)
                for (int k = 1; k <= 3; k++)
                    for (int hand = -1; hand <= 1; hand += 2)
                    {
                        Vector3 at = tailEntry + tailAlong * (all * (row == 0 ? 0.4f : 0.8f)) + side * (hand * (1.2f + 5.2f * k));
                        if (at.magnitude > FloorRadius - 1f) continue;
                        Fall(at, RippleRadius, 0.45f + 0.17f * k, RippleDamage);
                    }
        }

        // The sting: the tail rises over the nearest rim in a high arch with its fan hanging over the player like a
        // scorpion's, and stabs at marked spots one after another. Each can be guarded or stepped off. The last stab
        // sticks in the stone, the arch sags to the floor behind it, and it can be cut until it tears itself free.
        private void BeginTailSting()
        {
            TailStings++;
            stingsDone = 0;
            stingClock = 0f;
            stingArch = StingArch;
            stingTip = StingCocked;
            BeginTailFrom(TailShape.Sting, false, NearAngle());
            tailSpine.TipStiffness = 600f;
            AimSting();
            tailAlong = OnFloor(stingTarget - tailEntry).normalized;
            stingJoint = stingTarget - tailAlong * StingFanBack;
            tailReach = FlatDistance(stingJoint, tailEntry);
            SeedTail();
            Enter(Mode.TailSting);
            Vfx.Play(VfxKind.Glint, tailEntry * 0.9f + Vector3.up * 4f, Vector3.up, 2f);
        }

        // Where it will stab: where the player stands, as far as the tail reaches from the rim it came over.
        private void AimSting()
        {
            Vector3 from = Within(target.position, FloorRadius - 0.5f) - tailEntry;
            float away = Mathf.Clamp(from.magnitude, 6f, StingReach);
            stingTarget = tailEntry + (from.sqrMagnitude > 0.01f ? from.normalized : -tailEntry.normalized) * away;
        }

        private void AdvanceTailSting(float deltaTime)
        {
            Settle(deltaTime);
            FaceTarget(deltaTime, 4f, JawSnarl);
            var stingMark = slitherMarks[1];
            tailWave = 0.35f;
            tailWaveLength = 9f;
            tailWaveSpeed = 7f;
            stingClock += deltaTime * Pace;
            if (stingsDone >= StingCount)
            {
                // Stuck fast: the arch sags, and the tail lies on the stone behind its buried fan.
                stingArch = Mathf.Lerp(stingArch, 0.9f, 1f - Mathf.Exp(-4f * deltaTime));
                if (stingClock < StingStuck)
                {
                    tailLead = tailReach;
                    tailWave = 0.3f + 0.6f * Mathf.Clamp01(stingClock / StingStuck);
                    tailWaveSpeed = 3f + 10f * Mathf.Clamp01(stingClock / StingStuck);
                }
                else
                {
                    tailPart.SetVulnerable(false);
                    if (WithdrawTail(deltaTime)) EndAttack();
                }
                return;
            }
            float wait = stingsDone == 0 ? StingRise : StingEvery;
            if (stingClock < wait - StingLock) AimSting();
            Vector3 flat = OnFloor(stingTarget - tailEntry);
            tailAlong = flat.normalized;
            // The fan reaches on past the last joint: the joint is held short of the place and above it.
            stingJoint = Vector3.Lerp(stingJoint, stingTarget - tailAlong * StingFanBack, 1f - Mathf.Exp(-12f * deltaTime));
            tailReach = FlatDistance(stingJoint, tailEntry);
            tailLead = stingsDone == 0 ? Mathf.Lerp(-5f, tailReach, Mathf.SmoothStep(0f, 1f, stingClock / (StingRise * 0.6f))) : tailReach;
            if (stingClock < wait)
            {
                // It stays in the stone a moment before it is pulled out and cocked again.
                if (stingsDone == 0 || stingClock > StingHold) stingTip = Mathf.Lerp(stingTip, StingCocked, 1f - Mathf.Exp(-12f * deltaTime));
                stingMark.Show(stingTarget, Quaternion.identity, stingClock / wait, PlainMarker, false, StingRadius);
            }
            else if (stingClock < wait + StingStab)
            {
                stingTip = Mathf.Lerp(StingCocked, StingFanBack * 0.7f, (stingClock - wait) / StingStab);
            }
            else
            {
                stingTip = StingFanBack * 0.7f;
                stingMark.Hide();
                Vfx.Play(VfxKind.Impact, stingTarget + Vector3.up * 0.4f);
                Vfx.Play(VfxKind.Ring, stingTarget + Vector3.up * 0.2f, Vector3.up);
                for (int i = 0; i < 4; i++) Vfx.Play(VfxKind.Dust, stingTarget + Quaternion.Euler(0f, i * 90f, 0f) * Vector3.forward * 1.4f, Vector3.up);
                if (shaker != null) shaker.Shake(0.35f, 0.2f);
                stingsDone++;
                stingClock = 0f;
                bool last = stingsDone >= StingCount;
                if (last)
                {
                    LandEffects(stingTarget, StingRadius);
                    tailPart.SetVulnerable(true);
                }
                if (target != null && victim != null && FlatDistance(target.position, stingTarget) <= StingRadius + 0.3f)
                {
                    Deal(new Hit(StingDamage, 0f, tailAlong, false, false, tailEntry), StingBalance);
                    if (mode != Mode.TailSting) return;
                }
            }
        }

        // True while he is showing what he is about to do: the lines of light down the body in the cloud are turned low then, so the mark on the floor is what the eye finds.
        private bool Telling()
        {
            switch (mode)
            {
                case Mode.Beam: return elapsed < BeamTelegraph;
                case Mode.Breath: return elapsed < BreathTelegraph;
                case Mode.Bite: return elapsed < BiteTelegraph;
                case Mode.TailSweep: return elapsed < TailSweepTelegraph;
                case Mode.TailSlam: return elapsed < TailSlamTelegraph;
                case Mode.TailCoil: return elapsed < CoilTelegraph + CoilSqueeze;
                case Mode.TailScythe: return elapsed < ScytheTelegraph;
                case Mode.TailFlops: return flopsDone < FlopsCount;
                case Mode.TailSting: return stingsDone < StingCount;
                default: return false;
            }
        }

        // The slither's mark: a winding lane as wide as the body, drawn out from the rim along the way the tail will take.
        private void DrawRibbon(float shownLength)
        {
            float half = TailHalfWidth + 0.3f;
            float whole = Mathf.Min(tailReach + TailFan, tailChord - 0.3f);
            for (int i = 0; i < RibbonSections; i++)
            {
                float at = Mathf.Min(shownLength, i * whole / (RibbonSections - 1));
                Vector3 here = TailPath(at);
                Vector3 along = TailPath(Mathf.Min(at + 0.4f, tailChord)) - TailPath(Mathf.Max(0f, at - 0.4f));
                Vector3 side = new Vector3(-along.z, 0f, along.x).normalized;
                int v = i * 6;
                ribbonVertices[v] = here - side * half;
                ribbonVertices[v + 1] = ribbonVertices[v + 2] = here - side * (half - RibbonRim);
                ribbonVertices[v + 3] = ribbonVertices[v + 4] = here + side * (half - RibbonRim);
                ribbonVertices[v + 5] = here + side * half;
            }
            ribbonMesh.SetVertices(ribbonVertices);
        }

        private Mesh RibbonMesh()
        {
            ribbonVertices = new Vector3[RibbonSections * 6];
            var colours = new Color[ribbonVertices.Length];
            var uvs = new Vector2[ribbonVertices.Length];
            var triangles = new int[(RibbonSections - 1) * 18];
            var faint = new Color(1f, 1f, 1f, MarkerShapes.InsideOfLane);
            for (int i = 0; i < RibbonSections; i++)
            {
                int v = i * 6;
                colours[v] = colours[v + 1] = colours[v + 4] = colours[v + 5] = Color.white;
                colours[v + 2] = colours[v + 3] = faint;
                for (int k = 0; k < 6; k++) uvs[v + k] = new Vector2(0.5f, 0.5f);
                if (i == RibbonSections - 1) break;
                int t = i * 18;
                for (int band = 0; band < 3; band++)
                {
                    int a = v + band * 2, b = a + 1, c = a + 6, d = a + 7;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
                }
            }
            var mesh = new Mesh { name = "Slither Lane" };
            mesh.MarkDynamic();
            mesh.vertices = ribbonVertices;
            mesh.colors = colours;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            // It is redrawn wherever on the floor the tail aims: its bounds are the floor's.
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(FloorRadius * 2.4f, 1f, FloorRadius * 2.4f));
            return mesh;
        }

        // ------------------------------------------------------------------ broken, and falling

        private void AdvanceStagger(float deltaTime)
        {
            Settle(deltaTime);
            // The head comes down on the floor and lies on its cheek with the jaw hanging.
            float fallen = Mathf.Clamp01(elapsed / 0.5f);
            head.position = Vector3.Lerp(head.position, FallenHead(), fallen * fallen);
            var lying = Quaternion.LookRotation(Southward(8f)) * Quaternion.Euler(2f, 0f, 24f);
            head.rotation = Quaternion.Slerp(head.rotation, lying, 1f - Mathf.Exp(-16f * deltaTime));
            jawWant = 24f;
            if (elapsed >= 0.5f && elapsed - deltaTime < 0.5f)
            {
                Vfx.Play(VfxKind.Impact, FallenHead() + Vector3.up);
                Vfx.Play(VfxKind.Dust, FallenHead() + Vector3.back * 2.5f, Vector3.up);
                if (shaker != null) shaker.Shake(0.6f, 0.35f);
            }
            if (elapsed >= StaggeredSeconds)
            {
                Stagger.Reset();
                Enter(Mode.Recovering);
            }
        }

        private void AdvanceRecover(float deltaTime)
        {
            SetOpen(false, false, true);
            Settle(deltaTime);
            float t = Mathf.Clamp01(elapsed / 0.9f);
            head.position = Vector3.Lerp(head.position, HeadHome(), t * t);
            var up = Quaternion.LookRotation(Vector3.back) * Quaternion.Euler(HeadPitch - 12f, 0f, 0f);
            head.rotation = Quaternion.Slerp(head.rotation, up, 1f - Mathf.Exp(-8f * deltaTime));
            jawWant = JawWide * 0.6f;
            if (elapsed >= 0.9f)
            {
                rest = RestBetween * 0.6f;
                Enter(Mode.Idle);
            }
        }

        // The head rears with its jaws wide, light breaks out all along him, the sky strikes him, the arches go under
        // one after another round the summit, the forefeet lose the rim, and the head comes down on it, lies there,
        // and slides off into the cloud.
        private void AdvanceDying(float deltaTime)
        {
            float shudder = Mathf.Sin(Time.time * 47f) * Mathf.Lerp(0.06f, 0.2f, Mathf.Clamp01(elapsed / DeathHeadFallsAt));
            if (shaker != null && Time.frameCount % 6 == 0 && elapsed < DeathHeadFallsAt) shaker.Shake(0.22f, 0.2f);

            // The sky strikes him: the head, then an arch of the body, and round again.
            untilDeathBolt -= deltaTime;
            if (elapsed >= DeathBoltsAt && elapsed < DeathHeadLandsAt && untilDeathBolt <= 0f)
            {
                untilDeathBolt = DeathBoltEvery;
                int which = deathBolts++;
                Vector3 at = which % 3 == 0 || lengthSpines == null ? head.position + Vector3.up * 2f : lengthSpines[which % lengthSpines.Length].Points[lengthSpines[0].Count / 2] + Vector3.up * 2f;
                LightningBolts.Strike(at, 0.8f);
                Vfx.Play(VfxKind.Burst, at);
            }

            LetGo(leftClaw, true, DeathLeftLetsGoAt);
            LetGo(rightClaw, false, DeathRightLetsGoAt);

            if (elapsed < DeathHeadFallsAt)
            {
                // Reared up over the rim and thrown back until it looks at the sky, shaking harder all the while.
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, 2.4f, -1.2f), Vector3.back, -62f, shudder * 50f, 5f, JawWide);
                body.position = Vector3.Lerp(body.position, BodyHome() + new Vector3(-shudder, 0f, 0f), 1f - Mathf.Exp(-6f * deltaTime));
                return;
            }

            // The neck goes down behind the rim as the head comes forward over it.
            float sunk = Mathf.Clamp01((elapsed - DeathHeadFallsAt) / (DeathSeconds - DeathHeadFallsAt));
            body.position = BodyHome() + Vector3.down * (sunk * sunk * RiseDepth);
            // On its chin across the edge of the rim, snout toward the middle of the floor.
            Vector3 onRim = new Vector3(0f, 0.15f, FloorRadius - 1.5f);
            var lying = Quaternion.LookRotation(Southward(-6f)) * Quaternion.Euler(3f, 0f, 14f);
            jawWant = 16f;
            if (elapsed < DeathHeadLandsAt)
            {
                float t = (elapsed - DeathHeadFallsAt) / (DeathHeadLandsAt - DeathHeadFallsAt);
                head.SetPositionAndRotation(Vector3.Lerp(head.position, onRim, t * t), Quaternion.Slerp(head.rotation, lying, t * t));
            }
            else if (elapsed < DeathHeadSlidesAt)
            {
                if (!headDown)
                {
                    headDown = true;
                    LandEffects(NorthRim() + Vector3.back * 3.5f, 4f);
                    if (shaker != null) shaker.Shake(1f, 0.5f);
                    ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.45f), 0.08f);
                }
                head.SetPositionAndRotation(onRim, lying);
            }
            else if (elapsed < DeathSeconds)
            {
                // Its own weight takes it: back off the edge, slowly and then all at once.
                float t = (elapsed - DeathHeadSlidesAt) / (DeathSeconds - DeathHeadSlidesAt);
                head.SetPositionAndRotation(onRim + new Vector3(0f, -RiseDepth * t * t * t, 8f * t * t), lying * Quaternion.Euler(-60f * t * t, 0f, 0f));
                if (Time.frameCount % 4 == 0 && t < 0.5f) Vfx.Play(VfxKind.Dust, NorthRim() + new Vector3(Random.Range(-2f, 2f), 0.1f, -0.5f), Vector3.up);
            }
            else
            {
                mode = Mode.Dead;
                if (shaker != null) shaker.ClearFraming();
                foreach (var shown in renderers)
                    if (shown != null) shown.SetPropertyBlock(null);
                if (staggerMark != null) staggerMark.Show(false);
                SetShown(false);
                RaiseDied();
            }
        }

        // One forefoot of the death: it holds the rim until its moment, and then is dragged back over the edge.
        private void LetGo(Transform claw, bool left, float at)
        {
            Vector3 plant = ClawPlant(left);
            Vector3 outward = OnFloor(plant).normalized;
            if (elapsed < at)
            {
                Grip(claw, left, Mathf.Sin(Time.time * 43f) * 0.6f);
                return;
            }
            float t = Mathf.Clamp01((elapsed - at) / 0.9f);
            if (elapsed - Time.deltaTime < at) Vfx.Play(VfxKind.Dust, plant, Vector3.up);
            claw.SetPositionAndRotation(plant + outward * (4.5f * t) + Vector3.down * (9f * t * t), Quaternion.LookRotation(-outward) * Quaternion.Euler(-70f * t, 0f, 0f));
        }

        // ------------------------------------------------------------------ blows, marks, light

        // Something of his has come down on the floor: the ground shows it.
        private void LandEffects(Vector3 point, float radius)
        {
            Vfx.Play(VfxKind.Impact, point + Vector3.up * 0.6f);
            Vfx.Play(VfxKind.Ring, point + Vector3.up * 0.2f, Vector3.up);
            Vfx.Play(VfxKind.Burst, point + Vector3.up * 0.5f);
            for (int i = 0; i < 6; i++)
                Vfx.Play(VfxKind.Dust, point + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * (radius * 0.8f), Vector3.up);
            if (shaker != null) shaker.Shake(0.55f, 0.3f);
            ScreenFlash.Flash(new Color(0.85f, 0.9f, 1f, 0.22f), 0.06f);
        }

        private void Strike(Vector3 point, float radius, float damage, bool unblockable)
        {
            if (target == null || victim == null) return;
            Vector3 offset = target.position - point;
            offset.y = 0f;
            if (offset.magnitude > radius + 0.35f) return;
            Deal(new Hit(damage, 0f, offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.back, false, unblockable, point), PerfectGuardBalance);
        }

        // The blow is given to the player. One turned at the last instant is thrown back on his balance, and may break it:
        // whoever calls this must look at what he is doing afterwards before going on.
        private void Deal(in Hit hit, float balanceIfTurned)
        {
            if (victim == null) return;
            // In the air on a leap, nothing on the ground reaches the player.
            if (victimBody != null && victimBody.IsLeaping) return;
            BlowsStruck++;
            var outcome = victim.TakeHit(hit);
            if (outcome != HitOutcome.PerfectGuard || IsStaggered || IsDead || balanceIfTurned <= 0f) return;
            flashLeft = 0.09f;
            glowFlash = 0.3f;
            if (Stagger.Add(balanceIfTurned)) BeginStagger();
        }

        private void HideMarkers()
        {
            beamLane.Hide();
            beamGhost.Hide();
            biteMark.Hide();
            foreach (var mark in slitherMarks) mark.Hide();
            ribbonMark.Hide();
            tailSlamMark.Hide();
            HideBreath();
            for (int i = 0; i < fallLeft.Length; i++)
            {
                fallLeft[i] = 0f;
                falls[i].Hide();
            }
            patternStep = patternSteps;
            foreach (var quarter in stormQuarters) quarter.Hide();
            stormLeft = 0f;
            for (int i = 0; i < OrbCount && i < orbs.Length; i++)
            {
                orbLife[i] = 0f;
                orbRings[i].Hide();
                if (orbRenderers[i] != null) orbRenderers[i].enabled = false;
            }
            // Whatever the tail was doing, it is gone at once: his balance is what holds it up.
            if (tailOut) ParkTail();
        }

        // White for an instant when struck, a gold pulse for as long as it lies broken, and in its death a cold light
        // that breaks out on one piece and then another, more of them the nearer the end.
        private void Paint()
        {
            bool pulse = IsStaggered && Mathf.Repeat(Time.unscaledTime * 4f, 1f) < 0.5f;
            bool flash = flashLeft > 0f;
            flashLeft -= Time.unscaledDeltaTime;
            bool cracking = mode == Mode.Dying && elapsed < DeathHeadSlidesAt;
            if (cracking)
            {
                untilCrack -= Time.unscaledDeltaTime;
                if (untilCrack > 0f) return;
                untilCrack = 0.06f;
                flashing = true;
                float spread = Mathf.Clamp01(0.25f + elapsed / DeathHeadFallsAt);
                foreach (var shown in renderers)
                {
                    if (shown == null) continue;
                    if (Random.value > spread * 0.6f)
                    {
                        shown.SetPropertyBlock(null);
                        continue;
                    }
                    // Enough to see a piece light up, not so much that its shape is lost in the glare.
                    float strength = Random.Range(0.3f, 1f);
                    block.Clear();
                    block.SetColor(Emission, new Color(0.22f, 0.4f, 0.85f) * strength);
                    shown.SetPropertyBlock(block);
                }
                return;
            }
            bool lit = flash || pulse;
            if (!lit && !flashing) return;
            flashing = lit;
            for (int i = 0; i < renderers.Length; i++)
            {
                var shown = renderers[i];
                if (shown == null) continue;
                // The gold pulse of the broken balance is the head's alone (the head and its jaw are the first two pieces): it says where to strike.
                if (!lit || (!flash && i >= 2))
                {
                    shown.SetPropertyBlock(null);
                    continue;
                }
                block.Clear();
                block.SetColor(Emission, flash ? new Color(1.5f, 1.5f, 1.5f) : new Color(0.8f, 0.55f, 0.12f));
                shown.SetPropertyBlock(block);
            }
        }

        // The eyes, the mark on the brow and the lines down every flank are made of light, and say what he is about:
        // dark until he roars, climbing as a beam or a breath gathers, flaring when he is struck or turned, beating
        // in his fury, guttering as he dies.
        private void PaintGlow()
        {
            float eyes = 1f, marks = 1f + 0.12f * Mathf.Sin(Time.time * 2.3f);
            if (Furious && mode != Mode.Dying)
            {
                float pulse = 0.85f + 0.3f * Mathf.Sin(Time.time * 7f);
                eyes = FuryEyeGlow * pulse;
                marks = FuryMarkGlow * pulse;
            }
            if (mode == Mode.Rising)
            {
                float since = elapsed - EntranceRoarAt;
                eyes = since < 0f ? 0.15f : Mathf.Lerp(BeamGlow, 1f, Mathf.Clamp01(since / (RoarSeconds + 0.3f)));
                marks = since < 0f ? 0.4f : Mathf.Min(eyes, 2.4f);
            }
            else if (mode == Mode.Beam && !beamSpentShown)
            {
                float gathered = Mathf.Lerp(1f, BeamGlow, Mathf.Clamp01(elapsed / BeamTelegraph));
                eyes = Mathf.Max(eyes, gathered);
                // The lines down the whole body climb less than the eyes: at the eyes' strength the picture whites out.
                marks = Mathf.Max(marks, 1f + (gathered - 1f) * 0.4f);
            }
            else if (mode == Mode.Beam)
            {
                // Spent: nearly out.
                eyes = 0.3f;
                marks = 0.25f;
            }
            else if (mode == Mode.Breath && elapsed < BreathTelegraph + BreathSeconds)
            {
                float gathered = Mathf.Lerp(1f, BeamGlow * 0.6f, Mathf.Clamp01(elapsed / BreathTelegraph));
                eyes = Mathf.Max(eyes, gathered);
                marks = Mathf.Max(marks, gathered * 0.7f);
            }
            else if (mode == Mode.Storm || mode == Mode.Bolts || mode == Mode.Sigils || mode == Mode.Orbs)
            {
                marks = Mathf.Max(marks, 2.2f + 0.5f * Mathf.Sin(Time.time * 11f));
            }
            else if (mode == Mode.Staggered)
            {
                eyes = 0.2f;
                marks = 0.3f;
            }
            else if (mode == Mode.Dying)
            {
                // Brighter than it ever was, unsteady, and out when the head is down.
                float flicker = Mathf.Repeat(Time.time * 17f, 1f) < 0.5f ? 1f : 0.35f;
                eyes = elapsed < DeathHeadLandsAt ? DeathEyeGlow * flicker : 0.05f;
                marks = elapsed < DeathHeadLandsAt ? FuryMarkGlow * 1.5f * flicker : 0.05f;
            }
            if (glowFlash > 0f)
            {
                glowFlash -= Time.unscaledDeltaTime;
                eyes = Mathf.Max(eyes, 3.5f);
                marks = Mathf.Max(marks, 3.5f);
            }
            float bodyMarks = Telling() ? Mathf.Min(marks, TellingBodyGlow) : marks;
            for (int i = 0; i < glowRenderers.Length; i++)
            {
                float want = glowIsEye[i] ? eyes : glowOnHead[i] ? marks : bodyMarks;
                if (glowRenderers[i] == null || Mathf.Abs(want - glowShown[i]) < 0.03f) continue;
                glowShown[i] = want;
                if (Mathf.Abs(want - 1f) < 0.03f)
                {
                    glowRenderers[i].SetPropertyBlock(null, glowMaterials[i]);
                    continue;
                }
                block.Clear();
                block.SetColor(Emission, glowBase[i] * want);
                glowRenderers[i].SetPropertyBlock(block, glowMaterials[i]);
            }
        }
    }
}
