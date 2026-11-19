using Clover.Core;
using Clover.Player;
using Clover.World;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Khepharu, the Sun-Bearer (fire_land.md, section 6.4): what the sun-king is when the man has fallen. One
    /// creature in two shapes, only one of them in the court at a time: the scarab that rolls the sun across the
    /// floor and the falcon that carries it up. The sun is a real thing in the fight. It is the blow in the roll,
    /// the stoop and the pass; a perfect guard against it, or enough damage to the body that holds it during one
    /// attack, knocks it away, and then the shape lies exposed until it has fetched it. When the falcon carries it
    /// to noon the whole court burns, all but the obelisk's shadow, which is the same shade the sun-blind hunt by
    /// (<see cref="Sunlight.InHand"/>). Rigid pieces moved by code, as the storm dragon's are.
    /// </summary>
    public partial class SunBearer : GodlyForm
    {
        // The newer modes are added after Dead; ModeNames reads them by number, so their order is their names'.
        private enum Mode { Hidden, Entering, Idle, Roll, Ram, Sparks, Change, Stoop, Pass, Feathers, Zenith, Spent, Exposed, Staggered, Dying, Dead,
            Ricochet, Burrow, Flares, Gust, Grab, Sunfall, ShellBurst, Kick }
        // Under: carried underground by the burrowing scarab, not drawn.
        private enum SunState { Held, Left, High, Loose, Under }

        [SerializeField] private Transform target;
        [SerializeField] private Transform body;
        [SerializeField] private Transform scarab;
        [SerializeField] private Transform falconParts;
        [SerializeField] private Transform caseLeft, caseRight;
        [SerializeField] private Transform[] legs = new Transform[0];
        [SerializeField] private Transform[] wingLeft = new Transform[0];
        [SerializeField] private Transform[] wingRight = new Transform[0];
        [SerializeField] private Transform sun;
        [SerializeField] private GameObject sunBurning, sunDull;
        [SerializeField] private Transform falconShadow;
        [SerializeField] private Transform lightLine;
        [SerializeField] private Collider bulk;
        [SerializeField] private ColossusPart headPart, bodyPart, sunPart;
        [SerializeField] private Renderer[] renderers = new Renderer[0];
        [SerializeField] private MeshFilter[] laneMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] circleMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] fallMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] hotLaneMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] hotCircleMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] floorMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter[] shadowMarks = new MeshFilter[0];
        [SerializeField] private MeshFilter roarRing;
        [SerializeField] private MeshFilter[] aimMarks = new MeshFilter[0];
        [SerializeField] private Renderer finder;
        [SerializeField] private StaggerMark staggerMark;

        public float FloorRadius = 20f;
        /// <summary>How near the court's centre anything of his may come: the obelisk stands there.</summary>
        public float ObeliskClear = 4.2f;
        public float MaximumHealth = 560f;
        public float BalanceMaximum = 160f;
        public float StaggeredSeconds = 5.5f;
        public float HeadDamage = 1.6f;
        public float FinisherShare = 0.22f;
        /// <summary>The share of a blow that reaches its health while it holds the sun and its shell is hot.</summary>
        public float HotShellShare = 0.25f;
        /// <summary>Damage to the body during one attack that knocks the sun out of its hold, and what that costs its balance.</summary>
        public float KnockDamage = 30f;
        public float KnockBalance = 40f;
        public float PerfectGuardBalance = 60f;
        public float PassGuardBalance = 30f;
        /// <summary>
        /// The parry made for the sun (fire_boss_visuals.md section 3), a proposal for the owner to judge: a perfect guard
        /// against the sun strikes it back like a ball into the form, which is stunned for as long as a knock stuns it today.
        /// Off, a perfect guard throws the sun aside as before.
        /// </summary>
        public bool SunParry = true;
        /// <summary>The parry: the freeze at contact, then time slowed while the sun flies back (real seconds and the share of speed).</summary>
        public float ParryFreeze = 0.18f, ParrySlowSeconds = 0.35f, ParrySlowScale = 0.35f;
        /// <summary>The sun's flight back into the form in game seconds (slowed, so about half a second to the eye), the height of its arc, and the freeze as it strikes.</summary>
        public float ParryFlight = 0.13f, ParryArc = 2f, ParryStrikeFreeze = 0.08f;
        /// <summary>What striking the form with its own sun does, and how far beyond the form it bounces.</summary>
        public float ParryDamage = 30f, ParryRebound = 6f;
        public float FuryAt = 0.5f;
        public float FuryPace = 1.25f;
        public float FuryRest = 0.55f;
        public float RestBetween = 0.9f;
        public float LullSeconds = 2.4f;
        public float EntranceSeconds = 4.6f;
        public float RoarAt = 3.5f;
        /// <summary>The entrance: when the sun starts down the thread of light, and when it hangs over where he fell.</summary>
        public float SunDownAt = 1.1f, SunLandsAt = 2.1f;
        /// <summary>
        /// Metres above the floor the sun comes down to: onto the point of the spear he lifts, so he is seen under it,
        /// burning from inside as it pours down the spear into him; in the flash as it arrives he is gone into it.
        /// </summary>
        public float SunHangs = 4.6f;
        /// <summary>Metres above the floor the thread of light comes down to: the point of the lifted spear, not through him.</summary>
        public float ThreadFoot = 3.8f;
        public float DeathSeconds = 5.4f;
        /// <summary>
        /// The death: when the sun goes out, when the body starts to come apart, when it is gone, and when the dull sun,
        /// rolled into the obelisk's shadow, cracks open and leaves the man lying where it lay. What is left of
        /// <see cref="DeathSeconds"/> is quiet.
        /// </summary>
        public float SunOutAt = 1.6f, ApartAt = 2.6f, GoneAt = 3.9f, CrackAt = 4.3f;
        /// <summary>How far from the obelisk the dull sun rolls along its shadow, and how big it is when it cracks.</summary>
        public float SunRestsAt = 6f, CrackSize = 0.7f;
        /// <summary>How far off the camera stands, in its usual distances, while the entrance and the death are shown.</summary>
        public float ShowDistance = 1.5f;
        public float FlightHeight = 12f;
        public float FlightRadius = 15f;
        public float WalkSpeed = 4f;

        public float RollWidth = 4f, RollSpeed = 12f, RollDamage = 28f, FurrowDamage = 24f, RollFollow = 0.9f, RollHold = 0.4f, FurrowFollow = 0.6f, FurrowHold = 0.3f;
        // Hot floor burns 6 s (the owner's review, 2026-11-18: 10 to 14 trapped the player), bites first 0.5 s after
        // stepping on, never through a dodge roll, and leaves char for a few seconds more that does nothing.
        public float HotSeconds = 6f, FurrowHotSeconds = 6f, HotDamage = 6f, HotEvery = 0.5f, HotAfter = 0.5f, HotResidual = 2.5f;
        public float RamReach = 9f, RamAfterSeconds = 1.2f, RamRadius = 1.7f, RamFollow = 0.8f, RamHold = 0.3f, RamDamage = 34f, RamStuck = 1.8f;
        public float SparkRadius = 1.1f, SparkDamage = 16f, SparkWarning = 1f, SparkOut = 7f;
        public float FlareRadius = 6f, FlareDamage = 20f;
        public float StoopRadius = 2.5f, StoopFollow = 1f, StoopHold = 0.5f, StoopDamage = 38f, StoopStands = 1.6f, StoopHotRadius = 2f;
        public float PassWidth = 5f, PassShown = 1.2f, PassSeconds = 1f, PassDamage = 26f, PassHeight = 3.6f, PassRadius = 15.5f, PassClimb = 6f;
        public float FeatherRadius = 0.9f, FeatherDamage = 14f, FeatherWarning = 1f;
        public float ZenithFill = 3f, ZenithBlaze = 6f, ZenithFuryBlaze = 8f, BlazeDamage = 12f, BlazeEvery = 0.5f, SpentSeconds = 2.5f;
        public float SunTouchDamage = 12f;
        /// <summary>The poise after a fall (<see cref="ShapePoise"/>): no balance broken for this long once it holds its sun again.</summary>
        public float PoiseSeconds = 12f;
        /// <summary>The loose sun struck: how fast a heavy blow and a light one send it rolling.</summary>
        public float SunHeavySpeed = 17f, SunLightSpeed = 5f;
        /// <summary>The rolled sun striking the scarab or the grounded falcon: the shell catches fire.</summary>
        public float ShellFireBalance = 60f, ShellFireDamage = 24f, ShellFireSeconds = 3.2f;
        /// <summary>The line drawn on the floor where a heavy blow would send the loose sun: how near the player must stand, how far round they may face, and how long and wide it is.</summary>
        public float AimReach = 3.6f, AimDegrees = 55f, AimLength = 7f, AimWidth = 0.7f;
        /// <summary>Flank-camping: how near the scarab's shell counts as on it, how long before it heats, how long it heats before it answers, and what that costs.</summary>
        public float CampReach = 2.5f, CampAfter = 2.5f, CampHeat = 1.5f, CampFrontDegrees = 50f, CampBehindDegrees = 125f;
        public float BurstRadius = 4.6f, BurstTell = 0.55f, BurstDamage = 16f, BurstShove = 3f;
        public float KickReach = 4.2f, KickTell = 0.45f, KickDamage = 14f, KickShove = 4.5f;
        /// <summary>The burrow: digging in, the ridge's pace underground (chasing, and going back for the sun), the chase, the circle it bursts up under, and the time half out of the ground.</summary>
        public float DigSeconds = 0.7f, RidgeSpeed = 9f, ChaseSpeed = 6.2f, ChaseSeconds = 2f, ChaseFurySeconds = 1.5f, EruptRadius = 2.6f, EruptTell = 0.75f, EruptDamage = 32f, EruptShove = 3f, HalfOutSeconds = 1.8f, EmergeSeconds = 0.55f;
        /// <summary>The Ricochet: lanes as wide as the sun, its pace, its damage, how long its lanes burn, and how long the shell lies cool after the shove.</summary>
        public float RicochetWidth = 3.4f, RicochetSpeed = 19f, RicochetDamage = 26f, RicochetHotSeconds = 3.5f, RicochetTell = 1.5f;
        /// <summary>The Flares: lines from the planted sun, how long, how wide, the time between them, and their damage.</summary>
        public float FlareLength = 15f, FlareWidth = 1.8f, FlareEvery = 0.45f, FlareFuryEvery = 0.34f, FlareFirstAt = 1.45f, FlaresDamage = 22f;
        /// <summary>The Ember Gust: the band's width, the wall's pace, its damage and how far it shoves.</summary>
        public float GustWidth = 7f, GustTell = 1.1f, GustSpeed = 24f, GustDamage = 8f, GustShove = 4.5f;
        /// <summary>The Talon Grab: the circle, how long it follows and holds, the carry, the height, the drop, and how long a miss leaves it grounded.</summary>
        public float GrabRadius = 2.2f, GrabFollow = 1.1f, GrabHold = 0.45f, GrabCarry = 1.25f, GrabHeight = 8.5f, GrabDropDamage = 30f, GrabCatchDamage = 6f, GrabMissed = 2.2f;
        /// <summary>The Sunfall: how many pieces (more in the fury), their circles, warning, damage, and the little fire each leaves.</summary>
        public float SunfallRadius = 1.7f, SunfallWarning = 1.2f, SunfallDamage = 22f, SunfallEvery = 0.3f, SunfallHotRadius = 1.3f, SunfallHotSeconds = 3f;
        public int SunfallCount = 3, SunfallFuryCount = 5;
        public float ExposedSeconds = 4f, ExposedMost = 9f, ExposedMostInFury = 7f;
        public float ViewDistance = 2f, ViewPitch = -8f, ViewLead = 0.3f, ViewLeadLimit = 4.5f;
        public Color PlainMarker = new Color(1f, 0.93f, 0.75f, 1f);
        public Color RedMarker = new Color(1f, 0.08f, 0.05f, 1f);
        public Color HotMarker = new Color(1f, 0.45f, 0.06f, 1f);
        /// <summary>What hot floor is inside its orange edge: scorched, a char wash laid over the paving, not orange paint.</summary>
        public Color BurntFloor = new Color(0.17f, 0.075f, 0.045f, 0.62f);
        /// <summary>The whole court through the Zenith's blaze: scorched red-brown, thin enough that the shadow still reads through it.</summary>
        public Color BlazeFloor = new Color(0.42f, 0.13f, 0.04f, 0.3f);

        public SunBearerChooser Chooser = new SunBearerChooser();
        /// <summary>What it does next, whatever the chooser would say: for probes. A falcon's attack asked of the scarab brings the Rising first.</summary>
        [System.NonSerialized] public SunBearerAttack Queued;
        /// <summary>When true it rests until something is asked of it through <see cref="Queued"/>: for probes, so each move is seen alone.</summary>
        [System.NonSerialized] public bool OnlyWhenAsked;
        /// <summary>Where the sun comes down and it gathers itself: where the man fell.</summary>
        [System.NonSerialized] public Vector3 RiseFrom = new Vector3(0f, 0f, 8f);

        public override bool IsStaggered => mode == Mode.Staggered;
        public override bool IsDead => mode == Mode.Dead || mode == Mode.Dying;
        public override bool HasRisen => mode != Mode.Hidden;
        // From the roar of its entrance: that is when its name and its bar are shown.
        public override bool InFight => mode != Mode.Hidden && (mode != Mode.Entering || roared) && mode != Mode.Dead && mode != Mode.Dying;
        public override string Doing => mode == Mode.Roll && rollsInAll > 1 ? "Furrows" : mode == Mode.Change ? (toFalcon ? "Rising" : "Setting") : ModeNames[(int)mode];
        /// <summary>True once the mark of what it is doing has stopped following the player: from then it lands where it lies.</summary>
        public bool Locked { get; private set; }
        public override float DoingFor => elapsed;
        public override ColossusPart HeadPart => headPart;
        public ColossusPart BodyPart => bodyPart;
        public ColossusPart SunPart => sunPart;
        public bool Furious => Health.Fraction <= FuryAt;
        public bool IsFalcon => falcon;
        public bool SunLoose => sunState == SunState.Loose;
        public bool Exposed => mode == Mode.Exposed;
        /// <summary>True from the climb to the end of the blaze.</summary>
        public bool AtZenith => mode == Mode.Zenith;
        /// <summary>True while the court burns: the blaze of the Zenith, after the floor has filled.</summary>
        public bool Blazing { get; private set; }
        public Vector3 SunAt => sunAt;
        public Vector3 BodyAt => at;
        public int HotLanes { get; private set; }
        public int HotPatches { get; private set; }
        public int SunKnocks { get; private set; }
        /// <summary>Perfect guards that struck the sun back into the form: for probes.</summary>
        public int Parries { get; private set; }
        /// <summary>True while the parried sun is in the air on its way back: for probes.</summary>
        public bool SunFlyingBack => parryLeft >= 0f;
        public int SunPushes { get; private set; }
        public int TimesStaggered { get; private set; }
        public int BlowsStruck { get; private set; }
        public int HotBites { get; private set; }
        public int BlazeBites { get; private set; }
        public int BlazeSpared { get; private set; }
        public int Lulls { get; private set; }
        public int FallsLaid { get; private set; }
        public bool PlayerHidden { get; private set; }
        /// <summary>True once the dull sun has cracked open in its death; <see cref="CrackedAt"/> is where, on the floor.</summary>
        public bool SunCracked { get; private set; }
        public Vector3 CrackedAt { get; private set; }
        /// <summary>What the last blow it gave the player was: for probes.</summary>
        public string LastBlow { get; private set; } = "";
        /// <summary>The middle of the mark of whatever it is about to do: for probes.</summary>
        public Vector3 MarkAt { get; private set; }
        /// <summary>The red lane as it is drawn: where it starts, which way it runs and how long: for probes.</summary>
        public Vector3 LaneFrom => laneFrom;
        public Vector3 LaneAlong => laneAlong;
        public float LaneLength => laneLength;
        /// <summary>The poise after a fall: while it holds, no balance is broken (fire_land.md 6.4, reworked).</summary>
        public ShapePoise Poise { get; } = new ShapePoise();
        /// <summary>Times the rolled sun set the shell alight; flank heat answered by a burst and by a kick; burrows dug.</summary>
        public int SunStrikes { get; private set; }
        public int ShellBursts { get; private set; }
        public int Kicks { get; private set; }
        public int Burrows { get; private set; }
        /// <summary>Talon grabs that caught the player, and that missed and left it grounded.</summary>
        public int Grabbed { get; private set; }
        public int GrabsMissed { get; private set; }
        /// <summary>Lanes that would have shut the player off from the shape: moved aside, or laid without fire.</summary>
        public int LanesShifted { get; private set; }
        public int LanesUnlit { get; private set; }
        /// <summary>How hot the shell under a flank-camping player is, 0 to 1.</summary>
        public float CampHeatNow => campHeat;
        /// <summary>True while the scarab is underground, and where the head of its ridge is.</summary>
        public bool Underground => underground;
        public Vector3 RidgeAt => burrowAt;
        /// <summary>True while the aiming line of the loose sun is drawn.</summary>
        public bool Aiming { get; private set; }
        /// <summary>True while the sun rolls hard enough to set the shell alight.</summary>
        public bool SunRollingHard => rolling.Hard;
        /// <summary>True while the player is held in the talons.</summary>
        public bool Carrying => carrying;

        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private static readonly int MarkerColour = Shader.PropertyToID("_Color");
        // The land's colours for what it throws: sand dust and grey smoke (fire_boss_visuals.md, the palette).
        // Multipliers on the Fire land's effects, which are fire of their own colour: these only warm them a little.
        private static readonly Color Gold = new Color(1f, 0.82f, 0.62f, 1f);
        private static readonly Color Sand = new Color(1f, 0.88f, 0.64f, 1f);
        private static readonly Color Smoke = new Color(0.24f, 0.22f, 0.21f, 0.85f);
        // What a burnt body comes apart into: dark red-brown cinders, not pale flakes that read as light.
        private static readonly Color Cinder = new Color(0.36f, 0.13f, 0.06f, 1f);
        private static readonly Color SunWhite = new Color(1f, 0.92f, 0.78f, 1f);
        // Read every frame by the bar and the probes: the names are made once.
        private static readonly string[] ModeNames = System.Enum.GetNames(typeof(Mode));
        private const int HotMost = 12;
        private const float LaneUnit = 10f;

        private Mode mode = Mode.Hidden;
        private float elapsed, rest;
        private bool falcon, toFalcon;
        private IDamageable victim;
        private PlayerController victimBody;
        private PlayerDefence victimDefence;
        private TopDownCamera shaker;
        private HitStop hitStop;
        private MaterialPropertyBlock block;
        private Mesh laneMesh, discMesh, floorMesh, roarMesh;
        private GroundMark[] lanes, circles, falls, hotLanes, hotCircles;
        private GroundMark floorMark, shadowMark;
        private Renderer roarRenderer, shadowRenderer, lineRenderer;
        private Quaternion[] wingLeftRest, wingRightRest, legRest;
        private Vector3[] wingLeftAxis, wingRightAxis;

        // The body as it stands: where, which way, how tipped, how far the cases are lifted and the wings raised.
        private Vector3 at;
        private float yaw, pitch, roll, lift, flap, size = 1f, legPhase, flyAngle, flyTurn = 1f;
        private Vector3 sunAt, sunRollTo;
        private float sunSpin, sunSize = 1f, sunRollLeft;
        // How flat the body lies as it comes apart: 1 is whole.
        private float squash = 1f;
        private OcclusionFader fader;
        private bool holding;
        private SunState sunState;

        // Falling things: sparks and feathers.
        private Vector3[] fallPoints;
        private float[] fallLeft, fallWarning, fallRadius, fallDamage;
        private Vector3[] fallFrom;
        private bool[] fallFeather, fallMeteor;
        private float untilMeteorSmoke;

        // Floor left hot. Each patch burns for its seconds and then lies as char for HotResidual more; while it burns it
        // narrows from its edges in as it cools, and only what still burns bites. A patch that does not bite (the
        // Flares' lines) is drawn the same and never counted as hot floor.
        private readonly bool[] hotIsLane = new bool[HotMost], hotBites = new bool[HotMost];
        private readonly Vector3[] hotFrom = new Vector3[HotMost], hotAlong = new Vector3[HotMost];
        private readonly float[] hotLength = new float[HotMost], hotFull = new float[HotMost], hotWide = new float[HotMost], hotLeft = new float[HotMost], hotSeconds = new float[HotMost];
        private float untilFlame;
        private bool smokeTurn;
        private readonly HotFloorClock hotClock = new HotFloorClock();
        private ScorchedGround scorch;
        private readonly CourtReach reach = new CourtReach();
        // The hot lane the sun is laying now, which grows behind it as it goes (-1 when none); and metres or seconds
        // to the next puff of what it throws, so the effects come at the same rate whatever the frame rate.
        private int growing = -1;
        // The parry: the sun's flight back left (-1 when none), where it left the guard, and whether the form lies stunned by it.
        private float parryLeft = -1f;
        private Vector3 parryFrom;
        private bool parried;
        private float untilPuff, untilEmber;
        private SunBearerLook look;

        private Vector3 laneFrom, laneAlong, markPoint, moveFrom, landAt;
        private float stoopFollow = 1f;
        private bool laneChecked, laneUnlit;
        private float laneLength, laneDone, prepare, nearFor, sinceZenith = 100f, knockCount, untilTouch, untilBlaze, flashLeft, roarLeft;
        private int rollsLeft, rollsInAll, step, passes;
        private string blowName;
        private bool struck, headOpen, furyShown, zenithOwed, saidLoose, saidBlaze, roared, flashing, turnedBack, landed;

        public void Configure(Transform player, Transform bodyRoot, Transform scarabParts, Transform falconPieces, Transform leftCase, Transform rightCase, Transform[] scarabLegs,
            Transform[] leftWing, Transform[] rightWing, Transform sunRoot, GameObject burning, GameObject dull, Transform shadowBlob, Transform line, Collider bulkCollider,
            ColossusPart head, ColossusPart trunk, ColossusPart sunToPush, Renderer[] allRenderers)
        {
            target = player;
            body = bodyRoot;
            scarab = scarabParts;
            falconParts = falconPieces;
            caseLeft = leftCase;
            caseRight = rightCase;
            legs = scarabLegs;
            wingLeft = leftWing;
            wingRight = rightWing;
            sun = sunRoot;
            sunBurning = burning;
            sunDull = dull;
            falconShadow = shadowBlob;
            lightLine = line;
            bulk = bulkCollider;
            headPart = head;
            bodyPart = trunk;
            sunPart = sunToPush;
            renderers = allRenderers;
        }

        /// <summary>The marks it draws, each an outline and the copy that fills it, in pairs.</summary>
        public void ConfigureMarks(MeshFilter[] lanePairs, MeshFilter[] circlePairs, MeshFilter[] fallPairs, MeshFilter[] hotLanePairs, MeshFilter[] hotCirclePairs,
            MeshFilter[] floorPair, MeshFilter[] shadowPair, MeshFilter roar)
        {
            laneMarks = lanePairs;
            circleMarks = circlePairs;
            fallMarks = fallPairs;
            hotLaneMarks = hotLanePairs;
            hotCircleMarks = hotCirclePairs;
            floorMarks = floorPair;
            shadowMarks = shadowPair;
            roarRing = roar;
        }

        /// <summary>The short line drawn from the loose sun where a heavy blow would send it.</summary>
        public void ConfigureAim(MeshFilter[] pair) => aimMarks = pair;

        /// <summary>The ring drawn over everything round the player while its body lies between them and the camera, and the mark over the fallen head.</summary>
        public void ConfigureFeedback(Renderer ring, StaggerMark markToShow)
        {
            finder = ring;
            staggerMark = markToShow;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            Health.Maximum = MaximumHealth;
            Health.Fill();
            Stagger.Maximum = BalanceMaximum;
            Stagger.StaggeredSeconds = StaggeredSeconds;
            Stagger.Reset();
            // A lane ten metres long and a metre wide, and a disc a metre in radius: each is stretched to what it marks.
            // (Ten, not one: the bold edge is stretched with the rest, and at one metre the two ends became slabs.)
            laneMesh = MarkerShapes.Lane(LaneUnit, 1f, MarkerShapes.InsideOfLane);
            discMesh = MarkerShapes.Sector(1f, 360f, 0.22f);
            floorMesh = MarkerShapes.Sector(FloorRadius, 360f, 0.4f);
            roarMesh = MarkerShapes.Sector(1f, 360f, 0.03f);
            lanes = Pairs(laneMarks, laneMesh);
            circles = Pairs(circleMarks, discMesh);
            falls = Pairs(fallMarks, discMesh);
            hotLanes = Pairs(hotLaneMarks, laneMesh);
            hotCircles = Pairs(hotCircleMarks, discMesh);
            floorMark = Pairs(floorMarks, floorMesh)[0];
            shadowMark = Pairs(shadowMarks, laneMesh)[0];
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
            fallFrom = new Vector3[falls.Length];
            fallFeather = new bool[falls.Length];
            fallMeteor = new bool[falls.Length];
            shadowRenderer = falconShadow != null ? falconShadow.GetComponent<Renderer>() : null;
            lineRenderer = lightLine != null ? lightLine.GetComponentInChildren<Renderer>(true) : null;
            aimMark = Pairs(aimMarks, laneMesh)[0];
            // The burning floor's own pieces (the Gust's wall of fire has pieces of its own, under another name).
            var scorchHolder = transform.Find("Scorch");
            scorch = scorchHolder != null ? scorchHolder.GetComponent<ScorchedGround>() : null;
            var shellHolder = transform.Find("Shell Fire");
            shellFire = shellHolder != null ? shellHolder.GetComponent<ScorchedGround>() : null;
            // The loose sun is a ball to be struck at the god, not a post to farm: blows on it pay nothing back.
            if (sunPart != null) sunPart.Inert = true;
            Poise.Seconds = PoiseSeconds;
            hotClock.FirstBite = HotAfter;
            hotClock.Every = HotEvery;
            rolling.Radius = 1.5f;
            rolling.RimAt = FloorRadius - 1.85f;
            legRest = new Quaternion[legs.Length];
            for (int i = 0; i < legs.Length; i++) legRest[i] = legs[i].localRotation;
            // The wings' bones are turned about the body's own forward line, wherever the file left their axes.
            body.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            RestOf(wingLeft, out wingLeftRest, out wingLeftAxis);
            RestOf(wingRight, out wingRightRest, out wingRightAxis);
            look = GetComponent<SunBearerLook>();
            Hide();
        }

        private static void RestOf(Transform[] bones, out Quaternion[] rests, out Vector3[] axes)
        {
            rests = new Quaternion[bones.Length];
            axes = new Vector3[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                rests[i] = bones[i].localRotation;
                axes[i] = Quaternion.Inverse(bones[i].rotation) * Vector3.forward;
            }
        }

        private GroundMark[] Pairs(MeshFilter[] pairs, Mesh mesh)
        {
            var marks = new GroundMark[Mathf.Max(1, pairs.Length / 2)];
            for (int i = 0; i < marks.Length; i++)
                marks[i] = pairs.Length >= i * 2 + 2 ? new GroundMark(pairs[i * 2], pairs[i * 2 + 1], mesh, block) : new GroundMark(null, null, mesh, block);
            return marks;
        }

        private void Start()
        {
            if (target != null)
            {
                victim = target.GetComponentInParent<IDamageable>();
                victimBody = target.GetComponentInParent<PlayerController>();
                victimDefence = target.GetComponentInParent<PlayerDefence>();
                hitStop = target.GetComponentInParent<HitStop>();
            }
            var main = Camera.main;
            shaker = main != null ? main.GetComponent<TopDownCamera>() : null;
            fader = main != null ? main.GetComponent<OcclusionFader>() : null;
        }

        // The player is let go and the obelisk is thinned for them alone again.
        private void Release()
        {
            if (holding && victimBody != null) victimBody.Suspended = false;
            holding = false;
            if (fader != null) fader.WatchAlso = false;
        }

        private void OnDestroy()
        {
            if (laneMesh != null) Destroy(laneMesh);
            if (discMesh != null) Destroy(discMesh);
            if (floorMesh != null) Destroy(floorMesh);
            if (roarMesh != null) Destroy(roarMesh);
        }

        private void OnDisable()
        {
            Release();
            EndCarry(false);
            ScorchedGround.Shade(null);
            if (shaker != null) shaker.ClearFraming();
        }

        private void Hide()
        {
            body.gameObject.SetActive(false);
            sun.gameObject.SetActive(false);
            if (shadowRenderer != null) shadowRenderer.enabled = false;
            if (lineRenderer != null) lineRenderer.enabled = false;
            if (finder != null) finder.enabled = false;
            if (bulk != null) bulk.enabled = false;
            SetOpen(false, false);
            sunPart.SetVulnerable(false);
            growing = -1;
            parryLeft = -1f;
            parried = false;
            underground = false;
            rolling.Stop();
            campFor = campHeat = 0f;
            shellFireLeft = 0f;
            Poise.Reset();
            if (aimMark != null) aimMark.Hide();
            Aiming = false;
            EndCarry(false);
            if (scorch != null) scorch.HideAll();
            if (shellFire != null) shellFire.HideAll();
            ScorchedGround.Shade(null);
            if (look != null) look.Clear();
            // (Clear also takes the court's whitening off.)
        }

        // True once every so many seconds (or metres) of what is passed in: effects thrown at a steady rate whatever the frame rate.
        private static bool Every(ref float clock, float interval, float passed)
        {
            clock -= passed;
            if (clock > 0f) return false;
            clock = clock < -interval ? interval : clock + interval;
            return true;
        }

        // How brightly the sun lights the court: held, carried at speed, overhead at the Zenith, or out.
        private float SunLightWanted()
        {
            if (parryLeft >= 0f) return 8f;
            if (sunState == SunState.Loose || sunState == SunState.Under || mode == Mode.Dying) return 0f;
            if (mode == Mode.Zenith) return 6f;
            if ((mode == Mode.Roll && step == 1) || (mode == Mode.Pass && elapsed >= PassShown)) return 4.5f;
            return 3f;
        }

        private void SetOpen(bool head, bool trunk)
        {
            headPart.SetVulnerable(head);
            bodyPart.SetVulnerable(trunk);
        }

        /// <summary>Back out of the court as if it had never risen: the player fell and the fight begins again from the man.</summary>
        public override void Retire()
        {
            if (mode == Mode.Hidden) return;
            SunCracked = false;
            HideMarkers();
            ClearHot();
            Blazing = false;
            if (shaker != null) shaker.ClearFraming();
            if (staggerMark != null) staggerMark.Show(false);
            if (roarRenderer != null) roarRenderer.enabled = false;
            Release();
            Hide();
            Enter(Mode.Hidden);
        }

        public override void Rise()
        {
            if (mode != Mode.Hidden) return;
            SunCracked = false;
            Health.Fill();
            Stagger.Reset();
            Afflictions.Clear();
            Chooser.Reset();
            Chooser.Fury = false;
            Poise.Reset();
            knockCount = 0f;
            rolling.Stop();
            underground = false;
            campFor = campHeat = shellFireLeft = 0f;
            Queued = SunBearerAttack.None;
            falcon = false;
            furyShown = zenithOwed = saidLoose = saidBlaze = roared = false;
            sinceZenith = 100f;
            nearFor = 0f;
            ClearHot();
            // Where he stands, so the sun comes down into him wherever the duel ended (no nearer the wall than the scarab needs).
            Vector3 from = Keep(OnFloor(RiseFrom), FloorRadius - 2.5f);
            // It gathers itself on the far side of the sun from whoever is watching.
            Vector3 away = target != null ? OnFloor(from - target.position) : Vector3.forward;
            away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.forward;
            at = Keep(from + away * 4.4f, FloorRadius - 3f);
            yaw = YawOf(-away);
            pitch = roll = 0f;
            lift = 0f;
            size = 0.01f;
            squash = 1f;
            // Nothing can reach the player while it comes, and they stand and watch it.
            if (victimBody != null && !victimBody.Suspended)
            {
                victimBody.Suspended = true;
                holding = true;
            }
            sunState = SunState.Left;
            sunAt = from + Vector3.up * 40f;
            sunSize = 1f;
            markPoint = from;
            body.gameObject.SetActive(true);
            sun.gameObject.SetActive(true);
            Enter(Mode.Entering);
            Pose();
        }

        public override HitOutcome TakePartHit(ColossusPart part, in Hit hit)
        {
            if (mode == Mode.Hidden || mode == Mode.Entering || IsDead) return HitOutcome.Missed;
            if (part == sunPart)
            {
                // The loose sun is a ball: a blow rolls it the way the blow went, hard for a heavy one, and rolled into
                // the shape it sets the shell alight. It pays the striker nothing back (ColossusPart.Inert).
                if (sunState != SunState.Loose || parryLeft >= 0f) return HitOutcome.Missed;
                Vector3 push = OnFloor(hit.Direction);
                if (push.sqrMagnitude < 0.01f && target != null) push = OnFloor(sunAt - target.position);
                sunRollLeft = 0f;
                rolling.Position = OnFloor(sunAt);
                rolling.Strike(push, hit.Heavy ? SunHeavySpeed : SunLightSpeed, hit.Heavy);
                SunPushes++;
                if (hit.Heavy)
                {
                    Vfx.Play(VfxKind.Impact, sunAt, push, 1.4f);
                    Vfx.Play(VfxKind.Embers, sunAt, push + Vector3.up, 1.6f);
                    Vfx.Play(VfxKind.Dust, OnFloor(sunAt) - push.normalized * 1.2f, -push, 1.4f, Sand);
                    if (shaker != null) shaker.Shake(0.3f, 0.2f);
                }
                return HitOutcome.Landed;
            }
            bool isHead = part == headPart;
            bool finisher = isHead && IsStaggered && hit.Heavy;
            // Without its sun in its hold (shoved away in the Ricochet, planted for the Flares) the shell is cool and takes blows in full.
            bool open = mode == Mode.Exposed || IsStaggered || (isHead && headOpen) || shellCool;
            float share = IsStaggered ? (isHead ? HeadDamage : 1f) : open ? 1f : HotShellShare;
            Afflictions.IgniteFrom(hit, part);
            float takes = Afflictions.DamageTakenMultiplier(hit.Spell);
            float unsteadies = Afflictions.StaggerTakenMultiplier;
            Afflictions.TakeFrom(hit, part);
            float damage = finisher ? Mathf.Max(hit.Damage * 3f * takes, MaximumHealth * FinisherShare) : hit.Damage * share * takes;
            Health.Damage(damage);
            flashLeft = 0.09f;
            if (Health.IsDead)
            {
                BeginDying();
                return finisher ? HitOutcome.Finisher : HitOutcome.Landed;
            }
            if (finisher)
            {
                // Finished, it drags itself up and goes for the sun.
                Stagger.Reset();
                BeginExposed(true);
                return HitOutcome.Finisher;
            }
            if (!open && !Poise.Holds && (sunState == SunState.Held || sunState == SunState.Left) && !underground)
            {
                // The shell is hot and little gets through, but all of it counts toward breaking its hold on the sun.
                // Not while it is poised after a fall: then the count would only throw it down again as it got up.
                knockCount += hit.Damage * takes;
                if (knockCount >= KnockDamage)
                {
                    Vector3 way = OnFloor(hit.Direction);
                    KnockSun(way.sqrMagnitude > 0.01f ? way.normalized : Facing(yaw), false);
                    return HitOutcome.Landed;
                }
            }
            if (!IsStaggered && Poise.Add(Stagger, hit.Stagger * (open ? 0.7f : 0.35f) * unsteadies)) BeginStagger();
            return HitOutcome.Landed;
        }

        public override void Unbalance(float amount)
        {
            if (mode == Mode.Hidden || mode == Mode.Entering || IsDead || IsStaggered) return;
            if (Poise.Add(Stagger, amount)) BeginStagger();
        }

        /// <summary>Breaks its balance at once, whatever its poise: for probes.</summary>
        public void ForceBreak()
        {
            if (mode == Mode.Hidden || mode == Mode.Entering || IsDead || IsStaggered) return;
            Poise.Reset();
            if (Stagger.Add(Stagger.Maximum * 2f)) BeginStagger();
        }

        // The sun is out of its hold: thrown off at right angles to the way it was going (or the way the blow went), dull and harmless.
        private void KnockSun(Vector3 going, bool squareToIt)
        {
            if (sunState == SunState.Loose || IsDead) return;
            // Every square knock is a perfect guard's; with the parry on, the sun goes back where it came from.
            if (squareToIt && SunParry)
            {
                StrikeBack();
                return;
            }
            SunKnocks++;
            Vector3 way = going;
            if (squareToIt)
            {
                way = new Vector3(going.z, 0f, -going.x);
                // To whichever side has the more floor.
                if (OnFloor(sunAt + way * 8f).sqrMagnitude > OnFloor(sunAt - way * 8f).sqrMagnitude) way = -way;
            }
            LoseSun(KeepSun(OnFloor(sunAt) + way * 8f), 0.6f);
            HideMarkers();
            Blazing = false;
            ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.28f), 0.08f);
            if (shaker != null) shaker.Shake(0.4f, 0.3f);
            if (!saidLoose && victimDefence != null)
            {
                saidLoose = true;
                victimDefence.Callout("The sun is loose");
            }
            knockCount = 0f;
            if (Poise.Add(Stagger, KnockBalance))
            {
                BeginStagger();
                return;
            }
            BeginExposed(false);
        }

        // The parry, beat 1 (section 3): contact. Time stops, the sun goes white-hot and leaves the guard for the form's face.
        private void StrikeBack()
        {
            SunKnocks++;
            Parries++;
            parried = true;
            sunState = SunState.Loose;
            sunPart.SetVulnerable(true);
            parryFrom = sunAt;
            parryLeft = ParryFlight;
            untilEmber = 0f;
            HideMarkers();
            Blazing = false;
            if (hitStop != null)
            {
                hitStop.Slow(ParryFreeze + ParrySlowSeconds, ParrySlowScale);
                hitStop.Freeze(ParryFreeze);
            }
            ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.44f), 0.08f);
            Vfx.Play(VfxKind.Critical, sunAt, Vector3.up, 1.5f, Gold);
            Vfx.Play(VfxKind.PerfectGuard, sunAt, Vector3.up, 1.5f);
            if (look != null) look.SunTrail(true);
            if (!saidLoose && victimDefence != null)
            {
                saidLoose = true;
                victimDefence.Callout("The sun is loose");
            }
            knockCount = 0f;
            if (Poise.Add(Stagger, KnockBalance))
            {
                BeginStagger();
                return;
            }
            BeginExposed(false);
        }

        // Where the struck sun is aimed: the scarab's thorax, the falcon's breast (straight up into it after a stoop).
        private Vector3 FaceOfForm() => falcon ? at + Vector3.up * 0.6f : at + Facing(yaw) * 1.6f + Vector3.up * 1.6f;

        // Beats 2 to 5: the flight back on its arc, the blow to the form, and the rebound beyond it.
        private void AdvanceParry(float deltaTime)
        {
            parryLeft -= deltaTime;
            float t = Mathf.Clamp01(1f - parryLeft / ParryFlight);
            Vector3 to = FaceOfForm();
            Vector3 was = sunAt;
            sunAt = Vector3.Lerp(parryFrom, to, t) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * ParryArc);
            sunSpin += (sunAt - was).magnitude / 1.5f * Mathf.Rad2Deg * 2f;
            if (Every(ref untilEmber, 0.5f, (sunAt - was).magnitude)) Vfx.Play(VfxKind.Embers, sunAt, Vector3.up, 0.8f);
            if (parryLeft > 0f) return;
            parryLeft = -1f;
            // It strikes the form.
            if (hitStop != null) hitStop.Freeze(ParryStrikeFreeze);
            Vfx.Play(VfxKind.Impact, to, Vector3.up, 3f, SunWhite);
            Vfx.Play(VfxKind.Burst, to, Vector3.up, 2f);
            Vfx.Play(VfxKind.Ring, OnFloor(to) + Vector3.up * 0.3f, Vector3.up, 1.6f, Gold);
            ScreenFlash.Flash(new Color(1f, 0.6f, 0.25f, 0.35f), 0.06f);
            if (shaker != null) shaker.Shake(0.6f, 0.35f);
            flashLeft = 0.12f;
            // The fire in it goes out: smoke from its glowing edges.
            for (int i = 0; i < 5; i++) Vfx.Play(VfxKind.Smoke, at + Facing(i * 72f) * 1.6f + Vector3.up * 1.2f, Vector3.up, 1f, Smoke);
            Health.Damage(ParryDamage);
            if (Health.IsDead)
            {
                BeginDying();
                return;
            }
            // The sun bounces on past the form, slowing, and rolls out dull: the form must go round itself to fetch it.
            Vector3 onward = OnFloor(to - parryFrom);
            onward = onward.sqrMagnitude > 0.01f ? onward.normalized : Facing(yaw);
            sunRollTo = KeepSun(OnFloor(at) + onward * ParryRebound);
            sunRollLeft = 0.85f;
            if (look != null) look.SunTrail(false);
        }

        private void LoseSun(Vector3 rollsTo, float seconds)
        {
            // Lost underground, it comes up out of the floor where the scarab is.
            if (sunState == SunState.Under) sunAt = OnFloor(at) + Facing(yaw) * 4.4f + Vector3.up * 1.5f;
            if (!sun.gameObject.activeSelf) sun.gameObject.SetActive(true);
            rolling.Stop();
            sunState = SunState.Loose;
            sunRollTo = rollsTo;
            sunRollLeft = seconds;
            sunPart.SetVulnerable(true);
            Vfx.Play(VfxKind.Burst, sunAt, Vector3.up, 2f);
            Vfx.Play(VfxKind.Embers, sunAt, Vector3.up, 2f);
        }

        private void BeginExposed(bool rightAway)
        {
            Surface();
            EndCarry(true);
            headOpen = false;
            shellCool = false;
            landed = false;
            knockCount = 0f;
            campFor = 0f;
            // Lying, its balance can be worn and broken; once it is up and going back for the sun it holds firm.
            Poise.Returning = false;
            SetOpen(true, true);
            Enter(Mode.Exposed);
            // After the broken balance it has lain long enough: it gets up at once and goes for the sun, still open.
            if (rightAway) elapsed = ExposedSeconds;
        }

        private void BeginStagger()
        {
            TimesStaggered++;
            Surface();
            EndCarry(true);
            shellCool = false;
            campFor = 0f;
            Poise.Returning = false;
            HideMarkers();
            Blazing = false;
            if (sunState != SunState.Loose) LoseSun(KeepSun(OnFloor(sunAt) + Facing(yaw + 70f) * 3f), 0.5f);
            SetOpen(true, true);
            headOpen = false;
            Enter(Mode.Staggered);
            Vfx.Play(VfxKind.Ring, at + Vector3.up * 2f, Vector3.up, 1f, Gold);
            ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.28f), 0.08f);
            if (shaker != null) shaker.Shake(0.5f, 0.4f);
        }

        private void BeginDying()
        {
            Surface();
            EndCarry(true);
            shellCool = false;
            HideMarkers();
            ClearHot();
            Blazing = false;
            SetOpen(false, false);
            headOpen = false;
            if (sunState != SunState.Loose) LoseSun(KeepSun(OnFloor(sunAt)), 0.2f);
            sunPart.SetVulnerable(false);
            if (bulk != null) bulk.enabled = false;
            Enter(Mode.Dying);
            if (hitStop != null)
            {
                hitStop.Slow(1.5f, 0.3f);
                hitStop.Freeze(0.12f);
            }
            ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.48f), 0.12f);
            if (shaker != null) shaker.Shake(0.5f, 0.5f);
        }

        private void Enter(Mode next)
        {
            mode = next;
            elapsed = 0f;
            step = 0;
            struck = false;
            Locked = false;
            growing = -1;
        }

        private float Pace => Furious ? FuryPace : 1f;

        // ------------------------------------------------------------------ where things are

        private static Vector3 OnFloor(Vector3 point) => new Vector3(point.x, 0f, point.z);
        private static float YawOf(Vector3 direction) => Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        private static Vector3 Facing(float degrees) => Quaternion.Euler(0f, degrees, 0f) * Vector3.forward;

        private static Vector3 Polar(float angle, float radius, float height = 0f)
        {
            float turn = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(turn) * radius, height, Mathf.Cos(turn) * radius);
        }

        // On the floor, no further out than this.
        private static Vector3 Keep(Vector3 point, float radius)
        {
            var flat = new Vector3(point.x, 0f, point.z);
            return flat.sqrMagnitude > radius * radius ? flat.normalized * radius : flat;
        }

        // On the floor and clear of the obelisk, which stands at the middle and is solid.
        private Vector3 Clear(Vector3 point, float clear, float radius)
        {
            Vector3 flat = Keep(point, radius);
            if (flat.sqrMagnitude >= clear * clear) return flat;
            return (flat.sqrMagnitude > 0.01f ? flat.normalized : Vector3.forward) * clear;
        }

        private Vector3 KeepSun(Vector3 point) => Clear(point, 3.6f, FloorRadius - 2.5f);

        /// <summary>How far a lane runs from a point along a level line before it meets the rim or the obelisk.</summary>
        public static float Run(Vector3 from, Vector3 along, float floorRadius, float obeliskRadius)
        {
            float length = StormDragon.Chord(from, along, floorRadius);
            float lead = -(from.x * along.x + from.z * along.z);
            if (lead <= 0f) return length;
            float square = from.x * from.x + from.z * from.z - lead * lead;
            if (square >= obeliskRadius * obeliskRadius) return length;
            return Mathf.Min(length, Mathf.Max(0f, lead - Mathf.Sqrt(obeliskRadius * obeliskRadius - square)));
        }

        // Where the sun is while it is held: between the scarab's forelegs, or in the falcon's talons.
        private Vector3 HeldSunPoint() => falcon ? at + Vector3.down * 2.1f - Facing(yaw) * 0.3f : at + Facing(yaw) * 4.4f + Vector3.up * 1.5f;

        private Vector3 PlayerOnFloor => target != null ? OnFloor(target.position) : Vector3.zero;

        private void Turn(Vector3 toward, float degreesPerSecond, float deltaTime)
        {
            if (toward.sqrMagnitude < 0.0001f) return;
            yaw = Mathf.MoveTowardsAngle(yaw, YawOf(toward), degreesPerSecond * deltaTime);
        }

        // The scarab walks on the floor, round the obelisk and never through it. True when it is there.
        private bool Walk(Vector3 to, float speed, float deltaTime, bool turns = true)
        {
            Vector3 flat = OnFloor(at);
            Vector3 way = OnFloor(to) - flat;
            float distance = way.magnitude;
            float pace = Mathf.Min(distance, speed * deltaTime);
            if (distance > 0.001f)
            {
                flat = Clear(flat + way / distance * pace, ObeliskClear, FloorRadius - 3.4f);
                legPhase += pace * 1.6f;
                if (turns) Turn(way, 220f, deltaTime);
            }
            at = new Vector3(flat.x, Mathf.MoveTowards(at.y, 0f, 14f * deltaTime), flat.z);
            return distance <= 0.15f;
        }

        private bool Fly(Vector3 to, float speed, float deltaTime, bool turns = true)
        {
            // Crossing the middle of the court on its way somewhere further out, the falcon goes over the obelisk's
            // point, not through it: its wings reach seven metres either side.
            if (falcon && OnFloor(at).magnitude < 9.5f && OnFloor(to).magnitude > 9.5f) to.y = Mathf.Max(to.y, 19.5f);
            Vector3 way = to - at;
            float distance = way.magnitude;
            at = Vector3.MoveTowards(at, to, speed * deltaTime);
            if (turns && OnFloor(way).sqrMagnitude > 0.04f) Turn(OnFloor(way), 260f, deltaTime);
            return distance <= 0.2f;
        }

        // ------------------------------------------------------------------ every frame

        private void Update()
        {
            if (mode == Mode.Hidden || mode == Mode.Dead) return;
            FrameTheFight();
            float deltaTime = Time.deltaTime * WorldClock.EnemyScale;
            bool plain = mode == Mode.Staggered || mode == Mode.Dying || mode == Mode.Entering || mode == Mode.Exposed;
            elapsed += deltaTime * (plain ? 1f : Pace);
            sinceZenith += deltaTime;
            Poise.Tick(deltaTime);
            Chooser.Fury = Furious;
            // Hot floor under the obelisk's shadow is cold, and is drawn so (fire_land.md 6.3, rule 2).
            ScorchedGround.Shade(mode == Mode.Dying ? null : Sunlight.Hand);
            if (target != null && !falcon && mode == Mode.Idle)
                nearFor = (PlayerOnFloor - OnFloor(at)).sqrMagnitude <= RamReach * RamReach ? nearFor + deltaTime : 0f;
            if (Afflictions.Runs && mode != Mode.Entering && mode != Mode.Dying)
                Health.Damage(Mathf.Min(Afflictions.Tick(deltaTime), Mathf.Max(0f, Health.Current - 1f)));
            switch (mode)
            {
                case Mode.Entering: AdvanceEntering(deltaTime); break;
                case Mode.Idle: AdvanceIdle(deltaTime); break;
                case Mode.Roll: AdvanceRoll(deltaTime); break;
                case Mode.Ram: AdvanceRam(deltaTime); break;
                case Mode.Sparks: AdvanceSparks(deltaTime); break;
                case Mode.Change: AdvanceChange(deltaTime); break;
                case Mode.Stoop: AdvanceStoop(deltaTime); break;
                case Mode.Pass: AdvancePass(deltaTime); break;
                case Mode.Feathers: AdvanceFeathers(deltaTime); break;
                case Mode.Zenith: AdvanceZenith(deltaTime); break;
                case Mode.Spent: AdvanceSpent(deltaTime); break;
                case Mode.Exposed: AdvanceExposed(deltaTime); break;
                case Mode.Staggered: AdvanceStagger(deltaTime); break;
                case Mode.Dying: AdvanceDying(deltaTime); break;
                case Mode.Ricochet: AdvanceRicochet(deltaTime); break;
                case Mode.Burrow: AdvanceBurrow(deltaTime); break;
                case Mode.Flares: AdvanceFlares(deltaTime); break;
                case Mode.Gust: AdvanceGust(deltaTime); break;
                case Mode.Grab: AdvanceGrab(deltaTime); break;
                case Mode.Sunfall: AdvanceSunfall(deltaTime); break;
                case Mode.ShellBurst: AdvanceShellBurst(deltaTime); break;
                case Mode.Kick: AdvanceKick(deltaTime); break;
            }
            if (mode == Mode.Dead || mode == Mode.Hidden) return;
            AdvanceFalls(deltaTime);
            AdvanceHot(deltaTime);
            AdvanceSun(deltaTime);
            AdvanceCamp(deltaTime);
            AdvanceShellFire(deltaTime);
            AdvanceAim();
            if (look != null)
            {
                look.SunShines(SunLightWanted());
                // Out of the Zenith by any road (spent, knocked, broken, dying) the court comes back to its own light.
                if (mode != Mode.Zenith) look.Blaze(0f);
                // The sun's trail of fire is only for its flights: thrown up in the Rising, or struck back by a parry.
                if (mode != Mode.Change && parryLeft < 0f) look.SunTrail(false);
                // A parried form lies stunned under circling stars for as long as it lies.
                if (parried && mode != Mode.Exposed && mode != Mode.Staggered) parried = false;
                look.Stunned(parried && (mode == Mode.Staggered || elapsed < ExposedSeconds), at + Vector3.up * (falcon ? 2.6f : 3.4f));
            }
            AdvanceFury();
            AdvanceRoarRing();
            Pose();
            // Nothing of it passes into the court's wall or the obelisk, nor under the floor while it lies there: the
            // pose is measured piece by piece and the whole moved back if any of it went too far.
            if (KeepInside()) Pose();
            AdvanceCarry();
            FindPlayer();
            Paint();
            if (staggerMark != null)
            {
                staggerMark.transform.position = at + Facing(yaw) * 1.6f + Vector3.up * 4f;
                staggerMark.Show(mode == Mode.Staggered && elapsed >= 0.5f);
            }
        }

        // The court is larger than the usual view and the falcon is overhead: the camera stands back, looks down
        // less steeply, and leans toward the middle of the court so the obelisk and its shadow stay in the picture.
        private void FrameTheFight()
        {
            if (shaker == null || target == null) return;
            Vector3 toMiddle = Vector3.ClampMagnitude(-OnFloor(target.position) * ViewLead, ViewLeadLimit);
            // While it comes and while it dies the camera leaves the player and looks at it, nearer than in the fight,
            // and the obelisk is thinned if it stands in the way: both are seen from wherever the player stands.
            if (mode == Mode.Entering || mode == Mode.Dying)
            {
                // Whoever watches stays in the picture too: earlier the view went to the form alone and lost them.
                // Before the sun arrives it is him under it that matters; after, the scarab rising behind him, which is tall.
                Vector3 seen = mode == Mode.Entering ? Vector3.Lerp(markPoint, OnFloor(at), elapsed < SunLandsAt ? 0f : 0.7f)
                    : SunCracked ? CrackedAt : Vector3.Lerp(OnFloor(at), OnFloor(sunAt), 0.4f);
                shaker.FrameBoth(seen, ShowDistance, ViewPitch, 3f, mode == Mode.Entering ? 5.5f : 2.5f);
                if (fader != null)
                {
                    fader.WatchAlso = true;
                    fader.AlsoAt = mode == Mode.Entering ? markPoint + Vector3.up * 2f : at + Vector3.up * 1.5f;
                    fader.AndAt = sunAt;
                }
                return;
            }
            shaker.SetFraming(ViewDistance, toMiddle, ViewPitch);
        }

        private void AdvanceFury()
        {
            if (furyShown || !Furious || IsDead) return;
            furyShown = true;
            zenithOwed = true;
            if (Sunlight.Hand != null) Sunlight.Hand.DegreesPerSecond = Mathf.Sign(Sunlight.Hand.DegreesPerSecond) * ShadowHand.InFury;
            ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.32f), 0.1f);
            if (shaker != null) shaker.Shake(0.5f, 0.5f);
            RaiseEnraged();
        }

        // ------------------------------------------------------------------ the entrance

        private void AdvanceEntering(float deltaTime)
        {
            Vector3 from = markPoint;
            // The first beat: where he stands with the spear lifted a ring of fire closes on the floor and a stream of fire
            // comes down out of the sky onto the point of the spear, spitting flame where it pours onto it.
            if (elapsed < SunLandsAt)
            {
                circles[0].Show(from, Quaternion.identity, Mathf.Clamp01(elapsed / SunDownAt), HotMarker, false, 4.5f);
                if (Every(ref untilEmber, 0.066f, deltaTime))
                {
                    Vfx.Play(VfxKind.Embers, from + Facing(Random.Range(0f, 360f)) * Random.Range(0.4f, 4f) + Vector3.up * 0.3f, Vector3.up, 1.4f);
                    Vfx.Play(VfxKind.Fire, from + Vector3.up * ThreadFoot, Vector3.up, 0.5f + Mathf.Clamp01(elapsed / SunDownAt));
                }
            }
            if (lineRenderer != null)
            {
                lineRenderer.enabled = elapsed < SunLandsAt + 0.7f;
                float half = (40f - ThreadFoot) * 0.5f;
                lightLine.position = from + Vector3.up * (ThreadFoot + half);
                float thick = elapsed < SunDownAt ? Mathf.Lerp(0.05f, 0.5f, elapsed / SunDownAt)
                    : elapsed < SunLandsAt ? 0.5f : Mathf.Lerp(0.5f, 0f, (elapsed - SunLandsAt) / 0.7f);
                lightLine.localScale = new Vector3(thick, half, thick);
            }
            // The second: the sun comes down the thread, down the spear, into him; in the flash as it arrives he is gone into it.
            float down = Mathf.Clamp01((elapsed - SunDownAt) / (SunLandsAt - SunDownAt));
            float hangs = Mathf.Lerp(40f, SunHangs, 1f - (1f - down) * (1f - down));
            if (elapsed >= SunLandsAt && step == 0)
            {
                step = 1;
                circles[0].Hide();
                Vfx.Play(VfxKind.Impact, from + Vector3.up);
                Vfx.Play(VfxKind.Ring, from + Vector3.up * 0.2f, Vector3.up, 1f, Gold);
                // He is consumed: a burst of flame where he stood and a pall of smoke rolling up off it.
                Vfx.Play(VfxKind.Fire, from + Vector3.up, Vector3.up, 5f);
                Vfx.Play(VfxKind.Burst, from + Vector3.up * 6f, Vector3.up, 3f);
                for (int i = 0; i < 6; i++) Vfx.Play(VfxKind.Smoke, from + Facing(i * 60f) * 1.2f + Vector3.up * (1.5f + 0.4f * i), Vector3.up, 2.2f, Smoke);
                ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.36f), 0.1f);
                if (shaker != null) shaker.Shake(0.7f, 0.5f);
            }
            // The third: the scarab comes up out of the floor under it with its cases thrown wide, and the sun
            // settles into its forelegs as the cases close.
            float grown = RoarAt - 0.3f;
            float up = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - SunLandsAt) / (grown - SunLandsAt)));
            size = Mathf.Max(0.01f, up);
            float taken = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - SunLandsAt - 0.3f) / (RoarAt - SunLandsAt - 0.3f)));
            sunAt = Vector3.Lerp(from + Vector3.up * hangs, HeldSunPoint(), taken);
            if (elapsed >= SunLandsAt && elapsed < RoarAt && Every(ref untilPuff, 0.05f, deltaTime))
                Vfx.Play(VfxKind.Fire, at + Facing(Random.Range(0f, 360f)) * Random.Range(0.5f, 2.6f) * size + Vector3.up * 0.4f, Vector3.up, 1.6f);
            // The last: it rears with the sun held, and is named.
            float rear = Mathf.Clamp01((elapsed - grown) / 0.3f) * (1f - Mathf.Clamp01((elapsed - RoarAt - 0.5f) / 0.5f));
            pitch = -24f * rear;
            lift = Mathf.Max(up * (1f - taken), 0.85f * rear);
            if (elapsed >= RoarAt && !roared)
            {
                roared = true;
                roarLeft = 0.9f;
                if (shaker != null) shaker.Shake(0.6f, 0.6f);
                Vfx.Play(VfxKind.Burst, at + Vector3.up * 2f, Vector3.up, 3f);
                Vfx.Play(VfxKind.Ring, OnFloor(at) + Vector3.up * 0.2f, Vector3.up, 1f, Gold);
                ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.28f), 0.09f);
                RaiseRoared();
            }
            if (elapsed < EntranceSeconds) return;
            size = 1f;
            sunState = SunState.Held;
            if (bulk != null) bulk.enabled = true;
            SetOpen(false, true);
            if (Sunlight.Hand != null) Sunlight.Hand.DegreesPerSecond = Mathf.Sign(Sunlight.Hand.DegreesPerSecond) * ShadowHand.AgainstTheForm;
            rest = RestBetween;
            Release();
            Enter(Mode.Idle);
        }

        private void AdvanceRoarRing()
        {
            if (roarRenderer == null) return;
            if (roarLeft <= 0f)
            {
                if (roarRenderer.enabled) roarRenderer.enabled = false;
                return;
            }
            roarLeft -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(roarLeft / 0.9f);
            roarRenderer.enabled = true;
            roarRing.transform.position = OnFloor(at) + Vector3.up * 0.08f;
            roarRing.transform.localScale = new Vector3(t * FloorRadius, 1f, t * FloorRadius);
            block.Clear();
            block.SetColor(MarkerColour, new Color(PlainMarker.r, PlainMarker.g, PlainMarker.b, 1f - t));
            roarRenderer.SetPropertyBlock(block);
        }

        // ------------------------------------------------------------------ resting and choosing

        private void AdvanceIdle(float deltaTime)
        {
            Stagger.Tick(deltaTime);
            if (falcon) Circle(deltaTime);
            else
            {
                // It keeps the sun between itself and the player, and backs toward the rim if it has been left in the middle.
                Vector3 toPlayer = PlayerOnFloor - OnFloor(at);
                Turn(toPlayer, 70f, deltaTime);
                if (OnFloor(at).magnitude < 9f) Walk(Clear(at, 11f, FloorRadius - 4f), WalkSpeed * 0.6f, deltaTime, false);
                pitch = Mathf.MoveTowards(pitch, 0f, 60f * deltaTime);
                roll = Mathf.MoveTowards(roll, 0f, 200f * deltaTime);
                lift = Mathf.MoveTowards(lift, 0f, 1.5f * deltaTime);
            }
            rest -= deltaTime * Pace;
            // Someone has stood on its flank long enough for the shell under them to burn: it answers them first.
            if (!falcon && CampRipe)
            {
                AnswerCamper();
                return;
            }
            if (OnlyWhenAsked && Queued == SunBearerAttack.None && !zenithOwed) return;
            if ((rest <= 0f && MayBeginAttack) || Queued != SunBearerAttack.None || zenithOwed) Choose();
        }

        // The falcon rides round the north of the court, high up, and its shadow goes round the floor under it.
        private void Circle(float deltaTime)
        {
            flyAngle += flyTurn * 22f * deltaTime;
            if (flyAngle > 115f) flyTurn = -1f;
            if (flyAngle < -115f) flyTurn = 1f;
            Vector3 to = Polar(flyAngle, FlightRadius, FlightHeight);
            Fly(to, 12f, deltaTime, false);
            Turn(Facing(flyAngle + 90f * flyTurn), 120f, deltaTime);
            pitch = Mathf.MoveTowards(pitch, 0f, 60f * deltaTime);
            roll = Mathf.MoveTowards(roll, -14f * flyTurn, 40f * deltaTime);
            lift = Mathf.MoveTowards(lift, 1f, 2f * deltaTime);
            flap = Mathf.Sin(Time.time * 3.2f) * 22f + 6f;
        }

        private void Choose()
        {
            if (target == null) return;
            SunBearerAttack pick = Queued;
            if (pick != SunBearerAttack.None)
            {
                // What is asked of the other shape waits for the change.
                if (SunBearerChooser.IsFalcons(pick) && !falcon) pick = SunBearerAttack.Rising;
                else if (SunBearerChooser.IsScarabs(pick) && falcon) pick = SunBearerAttack.Setting;
                else Queued = SunBearerAttack.None;
                Chooser.Record(pick);
            }
            else if (zenithOwed)
            {
                // Its fury opens with the blaze, whatever came before; the scarab must rise first.
                pick = falcon ? SunBearerAttack.Zenith : SunBearerAttack.Rising;
                if (falcon) zenithOwed = false;
                Chooser.Record(pick);
            }
            else pick = Chooser.Next(Random.value, falcon, nearFor >= RamAfterSeconds, HotLanes, sinceZenith);
            knockCount = 0f;
            nearFor = 0f;
            // The sun left lying (shoved away in the Ricochet) is fetched first, underground, unless what comes is the Ram.
            if (!falcon && sunState == SunState.Left && pick != SunBearerAttack.Ram && (OnFloor(sunAt) - OnFloor(HeldSunPoint())).magnitude > 1.5f)
            {
                BeginFetch(pick);
                return;
            }
            Begin(pick);
        }

        // Starts what was chosen.
        private void Begin(SunBearerAttack pick)
        {
            switch (pick)
            {
                case SunBearerAttack.Roll: BeginRoll(1); break;
                case SunBearerAttack.Furrows: BeginRoll(3); break;
                case SunBearerAttack.Ram: BeginRam(); break;
                case SunBearerAttack.Sparks: Enter(Mode.Sparks); break;
                case SunBearerAttack.Rising: toFalcon = true; Enter(Mode.Change); break;
                case SunBearerAttack.Setting: BeginSetting(); break;
                case SunBearerAttack.Stoop: BeginStoop(); break;
                case SunBearerAttack.Pass: BeginPass(); break;
                case SunBearerAttack.Feathers: markPoint = PlayerOnFloor; Enter(Mode.Feathers); break;
                case SunBearerAttack.Zenith: BeginZenith(); break;
                case SunBearerAttack.Ricochet: BeginRicochet(); break;
                case SunBearerAttack.Burrow: BeginBurrow(); break;
                case SunBearerAttack.Flares: BeginFlares(); break;
                case SunBearerAttack.Gust: BeginGust(); break;
                case SunBearerAttack.Grab: BeginGrab(); break;
                case SunBearerAttack.Sunfall: BeginSunfall(); break;
            }
        }

        private void EndAttack()
        {
            rest = RestBetween * (Furious ? FuryRest : 1f);
            if (Chooser.TakeLull())
            {
                rest = LullSeconds * Pace;
                Lulls++;
            }
            headOpen = false;
            shellCool = false;
            SetOpen(false, !falcon);
            Enter(Mode.Idle);
        }

        // ------------------------------------------------------------------ the scarab: the roll and the furrows

        private void BeginRoll(int rolls)
        {
            rollsLeft = rollsInAll = rolls;
            Enter(Mode.Roll);
            StartLane(true);
        }

        // It goes to the rim nearest it, and the lane is drawn from there through the player.
        private void StartLane(bool first)
        {
            laneChecked = false;
            laneUnlit = false;
            elapsed = 0f;
            step = 0;
            struck = false;
            moveFrom = OnFloor(at);
            Vector3 flat = OnFloor(at);
            Vector3 out_ = flat.sqrMagnitude > 1f ? flat.normalized : Vector3.forward;
            // Far enough in that the scarab behind the sun, six metres of it, is inside the wall.
            laneFrom = out_ * (FloorRadius - 4.5f);
            prepare = first ? Mathf.Clamp((laneFrom - flat).magnitude / 8f, 0.3f, 1.4f) : 1f;
            AimLane(0f);
        }

        private void AimLane(float aside)
        {
            Vector3 to = PlayerOnFloor - laneFrom;
            if (to.sqrMagnitude < 4f) to = -laneFrom;
            laneAlong = to.normalized;
            // The later furrows are laid to the side the player has gone.
            if (aside != 0f)
            {
                var square = new Vector3(laneAlong.z, 0f, -laneAlong.x);
                Vector3 through = PlayerOnFloor + square * aside;
                laneAlong = (through - laneFrom).normalized;
            }
            laneLength = Run(laneFrom, laneAlong, FloorRadius - 1.5f, 3.4f);
        }

        private void AdvanceRoll(float deltaTime)
        {
            bool furrows = rollsInAll > 1;
            float follow = furrows ? FurrowFollow : RollFollow, hold = furrows ? FurrowHold : RollHold;
            float tell = prepare + follow + hold;
            var laneFacing = Quaternion.LookRotation(laneAlong);
            if (elapsed < tell)
            {
                if (elapsed < prepare)
                {
                    // To the rim, the sun pushed ahead of it.
                    Walk(laneFrom - laneAlong * 0.5f, Mathf.Max(8f, (laneFrom - moveFrom).magnitude / prepare), deltaTime, false);
                    Turn(laneAlong, 300f, deltaTime);
                }
                else
                {
                    at = Vector3.MoveTowards(at, laneFrom - laneAlong * 0.5f, 10f * deltaTime);
                    Turn(laneAlong, 300f, deltaTime);
                }
                if (elapsed < prepare + follow && rollsLeft == rollsInAll) AimLane(0f);
                Locked = elapsed >= prepare + follow || rollsLeft != rollsInAll;
                if (Locked && !laneChecked) CheckRollLane();
                sunAt = Vector3.MoveTowards(sunAt, HeldSunPoint(), 40f * deltaTime);
                // It gathers itself behind the sun: down at the front, hind legs braced.
                pitch = Mathf.MoveTowards(pitch, elapsed > prepare ? 9f : 0f, 40f * deltaTime);
                lanes[0].Show(laneFrom, laneFacing, Mathf.Clamp01(elapsed / tell), RedMarker, true, RollWidth, laneLength / (LaneUnit * RollWidth));
                MarkAt = laneFrom + laneAlong * (laneLength * 0.5f);
                laneDone = 4.4f;
                return;
            }
            if (step == 0)
            {
                step = 1;
                yaw = YawOf(laneAlong);
                if (shaker != null) shaker.Shake(0.3f, 0.3f);
                // The furrow follows the sun: laid now with no length, it grows behind the sun as it goes.
                growing = LayLane(0.01f, RollWidth, laneUnlit ? 0.8f : furrows ? FurrowHotSeconds : HotSeconds, !laneUnlit);
                untilPuff = 0f;
            }
            // The sun goes down the lane and the scarab behind it, pushing.
            float sunAlong = Mathf.Min(laneDone, laneLength - 1.5f);
            if (step == 1)
            {
                laneDone += RollSpeed * deltaTime * Pace;
                sunAlong = Mathf.Max(4.4f, Mathf.Min(laneDone, laneLength - 1.5f));
                at = laneFrom + laneAlong * (sunAlong - 4.4f);
                legPhase += RollSpeed * deltaTime * 1.6f;
                pitch = 7f;
                sunAt = laneFrom + laneAlong * sunAlong + Vector3.up * 1.5f;
                sunSpin += RollSpeed * deltaTime * Pace / 1.5f * Mathf.Rad2Deg;
            }
            // The red is only what is still to come: behind the sun the floor is already scorched, and shows it.
            if (step == 1)
            {
                float behind = Mathf.Clamp(sunAlong - 1.2f, 0f, laneLength - 0.5f);
                lanes[0].Show(laneFrom + laneAlong * behind, laneFacing, 1f, RedMarker, true, RollWidth, (laneLength - behind) / (LaneUnit * RollWidth));
            }
            if (step == 1)
            {
                Grown(sunAlong - 1.2f);
                // A bow wave at the sun's front foot, one flame and one throw of sand a metre; embers spun off its top.
                if (Every(ref untilPuff, 1f, RollSpeed * deltaTime * Pace)) BowWave(OnFloor(sunAt), RollWidth, 1.3f);
            }
            if (step == 1 && !struck && SunMeets(sunAlong, RollWidth * 0.5f, 1.7f))
            {
                struck = true;
                if (Deal(new Hit(furrows ? FurrowDamage : RollDamage, 0f, laneAlong, false, true, sunAt), 0f) == HitOutcome.PerfectGuard)
                {
                    Grown(sunAlong);
                    KnockSun(laneAlong, true);
                    return;
                }
            }
            if (mode != Mode.Roll) return;
            if (step == 1 && laneDone < laneLength - 1.5f) return;
            // The far end. The lane it came down is left hot.
            if (step == 1)
            {
                step = 2;
                lanes[0].Hide();
                Grown(laneLength);
                growing = -1;
                laneDone = laneLength;
                elapsed = 100f;
                if (shaker != null) shaker.Shake(0.35f, 0.25f);
                // At the far rim it stops hard: sand thrown up in a ring round the sun.
                Vfx.Play(VfxKind.TintRing, OnFloor(sunAt) + Vector3.up * 0.3f, Vector3.up, 1f, Sand);
                for (int i = 0; i < 6; i++) Vfx.Play(VfxKind.Dust, OnFloor(sunAt) + Facing(i * 60f) * 1.6f, Vector3.up, 1.2f, Sand);
            }
            float after = elapsed - 100f;
            rollsLeft = Mathf.Max(rollsLeft, 0);
            if (rollsLeft > 1)
            {
                // It turns where it is and comes back down a lane to the side the player has gone.
                // The sun is swung round with it as it turns.
                Turn(-laneAlong, 200f, deltaTime);
                sunAt = Vector3.MoveTowards(sunAt, HeldSunPoint(), 40f * deltaTime);
                if (after < 1f) return;
                rollsLeft--;
                Vector3 offset = PlayerOnFloor - laneFrom;
                float side = offset.x * laneAlong.z - offset.z * laneAlong.x;
                laneFrom = OnFloor(at);
                elapsed = 0f;
                step = 0;
                struck = false;
                laneChecked = false;
                laneUnlit = false;
                prepare = 0.25f;
                moveFrom = OnFloor(at);
                AimLane(0f);
                // Not the lane it has just made: five metres to the player's side of it.
                if (Mathf.Abs(side) < 2.5f) AimLane(side >= 0f ? 5f : -5f);
                return;
            }
            // After the furrows it rests on the sun, and can be struck.
            if (after < (furrows ? 2f : 1f))
            {
                pitch = Mathf.MoveTowards(pitch, furrows ? 12f : 0f, 30f * deltaTime);
                return;
            }
            EndAttack();
        }

        // True when the sun, so far down the lane, has reached the player standing in the lane.
        private bool SunMeets(float sunAlong, float halfWidth, float reach)
        {
            if (target == null) return false;
            Vector3 offset = PlayerOnFloor - laneFrom;
            float along = offset.x * laneAlong.x + offset.z * laneAlong.z;
            float across = Mathf.Abs(offset.x * laneAlong.z - offset.z * laneAlong.x);
            return across <= halfWidth + 0.35f && Mathf.Abs(along - sunAlong) <= reach;
        }

        // ------------------------------------------------------------------ the scarab: the ram

        private void BeginRam()
        {
            sunState = SunState.Left;
            moveFrom = OnFloor(at);
            AimRam();
            Enter(Mode.Ram);
        }

        private void AimRam()
        {
            Vector3 to = PlayerOnFloor - moveFrom;
            if (to.magnitude > RamReach + 2f) to = to.normalized * (RamReach + 2f);
            if (to.sqrMagnitude < 9f) to = Facing(yaw) * 3f;
            markPoint = Clear(moveFrom + to, 3.6f, FloorRadius - 2.2f);
        }

        private void AdvanceRam(float deltaTime)
        {
            float tell = RamFollow + RamHold, drop = 0.2f;
            Vector3 way = OnFloor(markPoint - moveFrom).normalized;
            // The horn's tip is nearly three metres ahead of the body's middle.
            Vector3 lie = markPoint - way * 2.7f;
            if (elapsed < tell)
            {
                if (elapsed < RamFollow) AimRam();
                Locked = elapsed >= RamFollow;
                Turn(way, 260f, deltaTime);
                // It leaves the sun, draws back and lowers the horn.
                float back = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / (tell * 0.7f)));
                at = Vector3.Lerp(at, moveFrom - way * 1.4f * back, 1f - Mathf.Exp(-10f * deltaTime));
                pitch = Mathf.Lerp(0f, -16f, back);
                lift = 0.25f * back;
                circles[0].Show(markPoint, Quaternion.identity, elapsed / tell, PlainMarker, false, RamRadius);
                MarkAt = markPoint;
                return;
            }
            if (elapsed < tell + drop)
            {
                float t = (elapsed - tell) / drop;
                yaw = YawOf(way);
                at = Vector3.Lerp(moveFrom - way * 1.4f, lie, t * t);
                pitch = Mathf.Lerp(-16f, 15f, t);
                legPhase += 30f * deltaTime;
                return;
            }
            if (!struck)
            {
                struck = true;
                at = lie;
                pitch = 15f;
                circles[0].Hide();
                Vfx.Play(VfxKind.Impact, markPoint + Vector3.up * 0.6f);
                Vfx.Play(VfxKind.Ring, markPoint + Vector3.up * 0.2f, Vector3.up, 1f, Gold);
                for (int i = 0; i < 6; i++) Vfx.Play(VfxKind.Dust, markPoint + Facing(i * 60f) * RamRadius, Vector3.up, 1f, Sand);
                if (shaker != null) shaker.Shake(0.7f, 0.35f);
                Vector3 offset = PlayerOnFloor - markPoint;
                if (offset.magnitude <= RamRadius + 0.35f) Deal(new Hit(RamDamage, 0f, way, false, false, at), PerfectGuardBalance);
                if (mode != Mode.Ram) return;
                // The horn is in the paving: the head is open.
                headOpen = true;
                SetOpen(true, true);
                return;
            }
            float stuck = elapsed - tell - drop;
            if (stuck < RamStuck)
            {
                float pulling = Mathf.Clamp01(stuck / RamStuck);
                float shudder = Mathf.Sin(Time.time * 41f) * (0.3f + pulling * pulling * 2f);
                pitch = 15f + shudder * 1.5f;
                roll = shudder * 2f;
                return;
            }
            headOpen = false;
            SetOpen(false, true);
            roll = 0f;
            pitch = Mathf.MoveTowards(pitch, 0f, 60f * deltaTime);
            // Back to the sun it left: on foot when it is near, underground when it lies far off (after a Ricochet).
            Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
            if (toSun.magnitude > 9f)
            {
                BeginFetch(SunBearerAttack.None);
                return;
            }
            Turn(toSun, 200f, deltaTime);
            bool there = Walk(SunSide(), WalkSpeed * 1.5f, deltaTime, false) || stuck > RamStuck + 2.5f;
            if (!there) return;
            sunState = SunState.Held;
            EndAttack();
        }

        // ------------------------------------------------------------------ the scarab: the sparks

        private void AdvanceSparks(float deltaTime)
        {
            // The wing cases lifted and shaken.
            float shake = Mathf.Sin(Time.time * 38f);
            lift = Mathf.MoveTowards(lift, 0.55f + shake * 0.06f, 4f * deltaTime);
            roll = shake * 1.5f;
            Turn(PlayerOnFloor - OnFloor(at), 60f, deltaTime);
            int waves = Furious ? 3 : 2;
            if (step < waves && elapsed >= 0.35f + step * 0.7f)
            {
                float toward = YawOf(PlayerOnFloor - OnFloor(at));
                // An arc of five on the player's side; the second wave falls in the gaps of the first.
                float first = step % 2 == 0 ? -50f : -62.5f;
                for (int i = 0; i < 5; i++)
                {
                    Vector3 point = OnFloor(at) + Facing(toward + first + i * 25f) * SparkOut;
                    if (OnFloor(point).magnitude > FloorRadius - 1f) continue;
                    Fall(point, SparkRadius, SparkWarning, SparkDamage);
                }
                Vfx.Play(VfxKind.Embers, at + Vector3.up * 3f, Vector3.up, 2.5f);
                MarkAt = OnFloor(at) + Facing(toward) * SparkOut;
                step++;
            }
            if (elapsed < 0.35f + (waves - 1) * 0.7f + SparkWarning + 0.8f) return;
            roll = 0f;
            EndAttack();
        }

        // Something will fall on a circle: the circle is drawn now, and struck when its warning has run.
        private void Fall(Vector3 point, float radius, float warning, float damage, bool meteor = false)
        {
            for (int i = 0; i < fallLeft.Length; i++)
            {
                if (fallLeft[i] > 0f) continue;
                fallPoints[i] = point;
                fallRadius[i] = radius;
                fallWarning[i] = warning;
                fallLeft[i] = warning;
                fallDamage[i] = damage;
                // Where the thing that falls leaves the body: under the scarab's lifted cases, or the falcon itself.
                // A feather is let go seven metres over its mark, a little toward the falcon (it flies above the top of the
                // picture, so one let go from its own height would be unseen for most of its fall).
                fallFrom[i] = falcon ? point + Vector3.up * 7f + Vector3.ClampMagnitude(OnFloor(at) - OnFloor(point), 2f) : at + Vector3.up * 2.6f + Facing(yaw) * -0.8f;
                fallFeather[i] = falcon && !meteor;
                fallMeteor[i] = meteor;
                // A piece of the sun is torn from it where the falcon holds it, and thrown high.
                if (meteor) fallFrom[i] = sunAt;
                FallsLaid++;
                return;
            }
        }

        private void AdvanceFalls(float deltaTime)
        {
            bool gone = IsDead || mode == Mode.Staggered || mode == Mode.Exposed;
            for (int i = 0; i < fallLeft.Length; i++)
            {
                if (fallLeft[i] <= 0f) continue;
                fallLeft[i] -= deltaTime;
                if (fallLeft[i] > 0f && !gone)
                {
                    float through = 1f - fallLeft[i] / fallWarning[i];
                    falls[i].Show(fallPoints[i], Quaternion.identity, through, RedMarker, false, fallRadius[i]);
                    if (look != null) look.Falling(i, fallFrom[i], fallPoints[i] + Vector3.up * 0.3f, through, fallMeteor[i] ? 2 : fallFeather[i] ? 1 : 0);
                    // A burning piece of the sun trails smoke as it comes down.
                    if (fallMeteor[i] && look != null && Every(ref untilMeteorSmoke, 0.05f, deltaTime))
                        Vfx.Play(VfxKind.Smoke, look.FallerAt(i), Vector3.up, 0.8f, Smoke);
                    continue;
                }
                fallLeft[i] = 0f;
                falls[i].Hide();
                if (look != null) look.Fallen(i);
                if (gone) continue;
                if (fallMeteor[i])
                {
                    // It strikes like a stone from a sling and burns where it lies: a small fire, quickly out.
                    Vfx.Play(VfxKind.Fire, fallPoints[i] + Vector3.up * 0.3f, Vector3.up, 2.6f);
                    Vfx.Play(VfxKind.Burst, fallPoints[i] + Vector3.up * 0.5f, Vector3.up, 1.4f);
                    Vfx.Play(VfxKind.TintRing, fallPoints[i] + Vector3.up * 0.2f, Vector3.up, 0.7f, Sand);
                    for (int k = 0; k < 4; k++) Vfx.Play(VfxKind.Dust, fallPoints[i] + Facing(k * 90f + 45f) * fallRadius[i], Facing(k * 90f + 45f), 1f, Sand);
                    Vfx.Play(VfxKind.Smoke, fallPoints[i] + Vector3.up * 1.2f, Vector3.up, 1.4f, Smoke);
                    if (shaker != null) shaker.Shake(0.25f, 0.2f);
                    LayCrater(fallPoints[i], SunfallHotRadius, SunfallHotSeconds);
                    blowName = "a piece of the sun";
                    Strike(fallPoints[i], fallRadius[i], fallDamage[i]);
                    continue;
                }
                Vfx.Play(VfxKind.Fire, fallPoints[i] + Vector3.up * 0.3f, Vector3.up, 1.4f);
                Vfx.Play(VfxKind.Burst, fallPoints[i] + Vector3.up * 0.4f, Vector3.up, 0.8f);
                Strike(fallPoints[i], fallRadius[i], fallDamage[i]);
            }
        }

        // ------------------------------------------------------------------ the change of shape

        private void AdvanceChange(float deltaTime)
        {
            if (toFalcon)
            {
                // The Rising: it rears with the sun over its head, throws it up, and goes after it.
                const float Rear = 1.2f, Climb = 1.1f;
                if (elapsed < Rear)
                {
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / Rear);
                    pitch = -42f * t;
                    lift = 0.5f * t;
                    // Near the wall it turns as it rears to face the court, so the falcon's wings open along the rim
                    // and not into the wall.
                    if (OnFloor(at).magnitude > 11f) Turn(-OnFloor(at), 160f, deltaTime);
                    sunState = SunState.Left;
                    sunAt = Vector3.Lerp(sunAt, at + Vector3.up * (3.5f + 2f * t) + Facing(yaw) * 2f, 1f - Mathf.Exp(-8f * deltaTime));
                    if (Furious)
                    {
                        circles[0].Show(OnFloor(at), Quaternion.identity, elapsed / Rear, RedMarker, false, FlareRadius);
                        MarkAt = OnFloor(at);
                    }
                    return;
                }
                if (step == 0)
                {
                    step = 1;
                    circles[0].Hide();
                    Vfx.Play(VfxKind.Burst, sunAt, Vector3.up, 3f);
                    Vfx.Play(VfxKind.Ring, OnFloor(at) + Vector3.up * 0.2f, Vector3.up, 1f, Gold);
                    ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.28f), 0.08f);
                    // The throw: a pillar of light from the floor up past the sun, a ring of air, and the sun goes up
                    // trailing fire, embers shed from the opening wings.
                    if (look != null)
                    {
                        look.Dived(sunAt + Vector3.up * 10f, OnFloor(at));
                        look.SunTrail(true);
                    }
                    Vfx.Play(VfxKind.Embers, at + Vector3.up * 2.5f, Vector3.up, 3f);
                    if (Furious)
                    {
                        Vfx.Play(VfxKind.Fire, at + Vector3.up, Vector3.up, 4f);
                        Strike(OnFloor(at), FlareRadius, FlareDamage);
                    }
                    // In the flare the cases open and it is the falcon.
                    falcon = true;
                    if (bulk != null) bulk.enabled = false;
                    SetOpen(false, false);
                    flyAngle = Mathf.Clamp(YawOf(OnFloor(at)), -110f, 110f);
                }
                float up = Mathf.Clamp01((elapsed - Rear) / Climb);
                sunAt = Vector3.MoveTowards(sunAt, HeldSunPoint(), 30f * deltaTime);
                Fly(Polar(flyAngle, FlightRadius, FlightHeight), 16f, deltaTime, false);
                pitch = Mathf.Lerp(-42f, 0f, up);
                lift = Mathf.MoveTowards(lift, 1f, 3f * deltaTime);
                flap = Mathf.Sin(Time.time * 9f) * 34f;
                if (up < 1f) return;
                sunState = SunState.Held;
                if (look != null) look.SunTrail(false);
                EndAttack();
                return;
            }
            // The Setting: down on the rim furthest from the player, folded, the sun set down in front of it.
            if (step == 0)
            {
                bool there = Fly(landAt + Vector3.up * 1.4f, 15f, deltaTime);
                flap = Mathf.Sin(Time.time * 7f) * 30f + 10f;
                pitch = Mathf.MoveTowards(pitch, -18f, 60f * deltaTime);
                roll = Mathf.MoveTowards(roll, 0f, 60f * deltaTime);
                if (!there && elapsed < 3f) return;
                step = 1;
                elapsed = 0f;
                at = landAt;
                falcon = false;
                Turn(-landAt, 3600f, 1f);
                sunState = SunState.Held;
                if (bulk != null) bulk.enabled = true;
                Vfx.Play(VfxKind.Dust, at, Vector3.up, 2f, Sand);
                Vfx.Play(VfxKind.TintRing, OnFloor(at) + Vector3.up * 0.2f, Vector3.up, 1.2f, Sand);
                // The fire of its wings goes out: a last shower of embers, and smoke from the folding cases.
                Vfx.Play(VfxKind.Embers, at + Vector3.up * 2f, Vector3.up, 2f);
                for (int i = 0; i < 4; i++) Vfx.Play(VfxKind.Smoke, at + Facing(yaw + 90f * i + 45f) * 1.4f + Vector3.up * 1.8f, Vector3.up, 0.8f, Smoke);
                if (shaker != null) shaker.Shake(0.3f, 0.3f);
            }
            pitch = Mathf.MoveTowards(pitch, 0f, 60f * deltaTime);
            lift = Mathf.MoveTowards(lift, 0f, 2f * deltaTime);
            if (elapsed < 0.6f) return;
            EndAttack();
        }

        private void BeginSetting()
        {
            toFalcon = false;
            Vector3 away = -PlayerOnFloor;
            if (away.sqrMagnitude < 1f) away = Vector3.forward;
            landAt = away.normalized * (FloorRadius - 6f);
            Enter(Mode.Change);
        }

        // ------------------------------------------------------------------ the falcon: the stoop

        private void BeginStoop()
        {
            AimStoop();
            Enter(Mode.Stoop);
            stoopFollow = Chooser.LastWasChained ? StoopFollow * 0.7f : StoopFollow;
        }

        private void AimStoop() => markPoint = Clear(PlayerOnFloor, 4.6f, FloorRadius - 3f);

        private void AdvanceStoop(float deltaTime)
        {
            float tell = stoopFollow + StoopHold, drop = 0.24f;
            Vector3 stand = markPoint + Vector3.up * 3.6f;
            if (elapsed < tell)
            {
                if (elapsed < stoopFollow) AimStoop();
                Locked = elapsed >= stoopFollow;
                // Over the circle, wings high, looking straight down into it.
                Fly(markPoint + Vector3.up * (FlightHeight + 1f), 26f, deltaTime, false);
                // Facing the obelisk: its wings, fourteen metres across, then lie along the rim and not into the wall.
                Turn(-markPoint, 200f, deltaTime);
                roll = Mathf.MoveTowards(roll, 0f, 80f * deltaTime);
                pitch = Mathf.MoveTowards(pitch, 30f, 80f * deltaTime);
                flap = Mathf.Lerp(Mathf.Sin(Time.time * 8f) * 30f, 55f, Mathf.Clamp01((elapsed - stoopFollow) / StoopHold));
                circles[0].Show(markPoint, Quaternion.identity, elapsed / tell, RedMarker, false, StoopRadius);
                MarkAt = markPoint;
                moveFrom = at;
                return;
            }
            if (elapsed < tell + drop)
            {
                float t = (elapsed - tell) / drop;
                at = Vector3.Lerp(moveFrom, stand, t * t);
                flap = 62f;
                // The dive tears the air: a streak from where it left the sky down to the floor it strikes.
                if (look != null) look.Diving(moveFrom + Vector3.up * 1.5f, markPoint);
                return;
            }
            if (!struck)
            {
                struck = true;
                at = stand;
                pitch = 0f;
                circles[0].Hide();
                if (look != null) look.Dived(moveFrom + Vector3.up * 1.5f, markPoint);
                Vfx.Play(VfxKind.Impact, markPoint + Vector3.up * 0.6f, Vector3.up, 1.5f, SunWhite);
                Vfx.Play(VfxKind.Ring, markPoint + Vector3.up * 0.2f, Vector3.up, 1f, Gold);
                Vfx.Play(VfxKind.Fire, markPoint + Vector3.up * 0.5f, Vector3.up, 4f);
                // Fire thrown outward along the floor, and sand at the edge of what it struck.
                for (int i = 0; i < 10; i++) Vfx.Play(VfxKind.Fire, markPoint + Facing(i * 36f + 18f) * 1.6f + Vector3.up * 0.3f, Facing(i * 36f + 18f) + Vector3.up * 0.4f, 0.5f);
                for (int i = 0; i < 8; i++) Vfx.Play(VfxKind.Dust, markPoint + Facing(i * 45f) * StoopRadius, Facing(i * 45f), 1.2f, Sand);
                if (shaker != null) shaker.Shake(0.9f, 0.4f);
                ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.2f), 0.06f);
                LayCrater(markPoint, StoopHotRadius, HotSeconds);
                Vector3 offset = PlayerOnFloor - markPoint;
                if (offset.magnitude <= StoopRadius + 0.35f
                    && Deal(new Hit(StoopDamage, 0f, offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.back, false, true, markPoint), 0f) == HitOutcome.PerfectGuard)
                {
                    KnockSun(Facing(yaw), true);
                    return;
                }
                if (mode != Mode.Stoop) return;
                // It stands on the sun with its wings spread: the body can be struck.
                SetOpen(false, true);
                return;
            }
            float stood = elapsed - tell - drop;
            if (stood < StoopStands)
            {
                flap = Mathf.MoveTowards(flap, 4f, 200f * deltaTime);
                return;
            }
            // Up again.
            SetOpen(false, false);
            flap = Mathf.Sin(Time.time * 9f) * 34f;
            if (at.y < FlightHeight - 3f && stood < StoopStands + 1.2f)
            {
                Fly(new Vector3(at.x, FlightHeight, at.z), 12f, deltaTime, false);
                return;
            }
            EndAttack();
        }

        // ------------------------------------------------------------------ the falcon: the pass

        private void BeginPass()
        {
            passes = Furious ? 2 : 1;
            AimPass(0f);
            CheckPassLane(0f);
            Enter(Mode.Pass);
        }

        // A lane from rim to rim through the player, laid across the line from the obelisk to them so that neither
        // the sun nor a wing goes through the stone.
        private void AimPass(float turned)
        {
            Vector3 player = PlayerOnFloor;
            Vector3 out_ = player.sqrMagnitude > 1f ? player.normalized : Vector3.back;
            // It flies low only well inside the wall, so its wings never sweep into it: the lane runs from a circle of
            // PassRadius, and someone nearer the rim than that is beside the lane, not under it.
            // And never so near the obelisk that a wing, seven metres out, would sweep through it.
            Vector3 through = out_ * Mathf.Clamp(player.magnitude, 8.6f, PassRadius - 2.5f);
            laneAlong = Quaternion.Euler(0f, turned, 0f) * new Vector3(out_.z, 0f, -out_.x);
            // From the end nearer the falcon.
            if (Vector3.Dot(OnFloor(at) - through, laneAlong) > 0f) laneAlong = -laneAlong;
            float back = StormDragon.Chord(through, -laneAlong, PassRadius);
            laneFrom = through - laneAlong * back;
            laneLength = back + StormDragon.Chord(through, laneAlong, PassRadius);
        }

        private void AdvancePass(float deltaTime)
        {
            var laneFacing = Quaternion.LookRotation(laneAlong);
            Vector3 start = laneFrom + Vector3.up * PassHeight;
            // It leans away from the obelisk, the inner wing high.
            float lean = (laneFrom.x * laneAlong.z - laneFrom.z * laneAlong.x) > 0f ? -24f : 24f;
            if (elapsed < PassShown)
            {
                // The lane is laid once, where the player stood when it began, and shown for its whole warning.
                Locked = true;
                // It waits high off the lane's end and comes down into it.
                Fly(start - laneAlong * 3f + Vector3.up * PassClimb, 40f, deltaTime, false);
                Turn(laneAlong, 400f, deltaTime);
                pitch = Mathf.MoveTowards(pitch, 8f, 60f * deltaTime);
                roll = Mathf.MoveTowards(roll, lean, 120f * deltaTime);
                flap = Mathf.Sin(Time.time * 8f) * 26f + 10f;
                lanes[0].Show(laneFrom, laneFacing, elapsed / PassShown, RedMarker, true, PassWidth, laneLength / (LaneUnit * PassWidth));
                MarkAt = laneFrom + laneAlong * (laneLength * 0.5f);
                return;
            }
            float t = (elapsed - PassShown) / PassSeconds;
            if (t < 1f)
            {
                yaw = YawOf(laneAlong);
                roll = lean;
                float along = Mathf.Lerp(-3f, laneLength + 3f, t);
                // Down into the lane over its first three metres and up out of it over its last.
                float climb = Mathf.Max(Mathf.Clamp01(-along / 3f), Mathf.Clamp01((along - laneLength) / 3f));
                at = laneFrom + laneAlong * along + Vector3.up * (PassHeight + PassClimb * climb * climb);
                flap = 18f;
                sunAt = HeldSunPoint();
                float passed = Mathf.Clamp(along - 0.5f, 0f, laneLength - 0.5f);
                lanes[0].Show(laneFrom + laneAlong * passed, laneFacing, 1f, RedMarker, true, PassWidth, (laneLength - passed) / (LaneUnit * PassWidth));
                // The lane catches fire in a wave behind the sun, as the roll's furrow does.
                if (growing < 0 && !landed)
                {
                    growing = LayLane(0.01f, PassWidth, laneUnlit ? 0.8f : HotSeconds, !laneUnlit);
                    untilPuff = 0f;
                }
                Grown(along - 0.5f);
                if (along > 0f && along < laneLength && Every(ref untilPuff, 1f, (laneLength + 6f) / PassSeconds * deltaTime))
                    BowWave(OnFloor(sunAt), PassWidth, 0.8f);
                if (!struck && SunMeets(along, PassWidth * 0.5f, 2.6f))
                {
                    struck = true;
                    if (Deal(new Hit(PassDamage, 0f, laneAlong, false, true, sunAt), PassGuardBalance) == HitOutcome.PerfectGuard)
                    {
                        Grown(Mathf.Max(2f, along));
                        if (mode == Mode.Pass) KnockSun(laneAlong, true);
                        return;
                    }
                }
                return;
            }
            if (!landed)
            {
                landed = true;
                lanes[0].Hide();
                Grown(laneLength);
                growing = -1;
            }
            // Up off the end of the lane; in its fury it turns and comes back down another.
            float after = elapsed - PassShown - PassSeconds;
            Fly(new Vector3(at.x, FlightHeight, at.z) + laneAlong * 2f, 14f, deltaTime, false);
            roll = Mathf.MoveTowards(roll, 0f, 90f * deltaTime);
            flap = Mathf.Sin(Time.time * 9f) * 30f;
            if (passes > 1 && HotLanes < Chooser.MostHotLanes + 1)
            {
                if (after < 0.9f) return;
                passes--;
                elapsed = 0f;
                step = 1;
                struck = false;
                landed = false;
                AimPass(30f);
                CheckPassLane(30f);
                return;
            }
            if (after < 0.7f) return;
            landed = false;
            EndAttack();
        }

        // ------------------------------------------------------------------ the falcon: the feathers

        private void AdvanceFeathers(float deltaTime)
        {
            Circle(deltaTime);
            // It beats its wings over the court.
            flap = Mathf.Sin(Time.time * 14f) * 42f;
            if (step < 3 && elapsed >= 0.3f + step * 0.85f)
            {
                // The middle, then a ring of six, then the middle again with the ring turned to its gaps.
                if (step != 1) Feather(step == 0 ? markPoint : PlayerOnFloor);
                if (step >= 1)
                    for (int i = 0; i < 6; i++) Feather(markPoint + Facing(i * 60f + (step == 2 ? 30f : 0f)) * 5f);
                Vfx.Play(VfxKind.Embers, at + Vector3.down * 2f, Vector3.down, 3f);
                MarkAt = markPoint;
                step++;
            }
            if (elapsed < 0.3f + 2 * 0.85f + FeatherWarning + 0.5f) return;
            EndAttack();
        }

        // No feather is marked in the obelisk's shadow, nor on the stone, nor off the floor.
        private void Feather(Vector3 point)
        {
            if (Sunlight.InHand(point) || point.magnitude > FloorRadius - 1f || point.magnitude < 2f) return;
            Fall(point, FeatherRadius, FeatherWarning, FeatherDamage);
        }

        // ------------------------------------------------------------------ the falcon: the zenith

        private void BeginZenith()
        {
            turnedBack = false;
            sinceZenith = 0f;
            Enter(Mode.Zenith);
            if (!saidBlaze && victimDefence != null)
            {
                saidBlaze = true;
                victimDefence.Callout("The court blazes");
            }
        }

        private void AdvanceZenith(float deltaTime)
        {
            const float Climb = 1.2f;
            float blaze = Furious ? ZenithFuryBlaze : ZenithBlaze;
            // It climbs with the sun to the top of the sky over the obelisk.
            Fly(new Vector3(0f, 19f, 0.5f), 18f, deltaTime, false);
            Turn(Vector3.back, 200f, deltaTime);
            roll = Mathf.MoveTowards(roll, 0f, 60f * deltaTime);
            pitch = Mathf.MoveTowards(pitch, -30f, 60f * deltaTime);
            flap = Mathf.Sin(Time.time * 5f) * 16f + 30f;
            sunState = SunState.High;
            sunAt = Vector3.MoveTowards(sunAt, at + Vector3.up * 4.5f, 30f * deltaTime);
            var hand = Sunlight.Hand;
            MarkAt = Vector3.zero;
            if (elapsed < Climb + ZenithFill)
            {
                float fill = Mathf.Clamp01((elapsed - Climb) / ZenithFill);
                floorMark.Show(Vector3.zero, Quaternion.identity, fill, RedMarker, false);
                ShowShadow(hand, 1f);
                sunSize = 1f + fill * 0.5f;
                // The court grows hotter and redder as the floor fills: the sun at the top of the sky is felt by its heat.
                if (look != null) look.Blaze(fill);
                return;
            }
            float burning = elapsed - Climb - ZenithFill;
            if (burning < blaze)
            {
                if (!Blazing)
                {
                    Blazing = true;
                    untilBlaze = 0f;
                    ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.4f), 0.12f);
                    if (shaker != null) shaker.Shake(0.5f, 0.6f);
                }
                // In its fury the shadow stops half-way through, and comes back the other way.
                if (Furious && !turnedBack && burning >= blaze * 0.5f && hand != null)
                {
                    turnedBack = true;
                    hand.StopAndTurnBack(0.5f);
                }
                // The whole floor scorched under its orange edge, fire standing up out of it and smoke over it: burning, not painted.
                if (look != null) look.Blaze(1f);
                float beat = (0.75f + 0.25f * Mathf.Sin(Time.time * 17f)) * 0.5f;
                floorMark.ShowBurnt(Vector3.zero, Quaternion.identity, HotMarker, BlazeFloor, false, 1f, 1f, beat * 2f);
                // Heat shimmers off all the lit floor round the player, and none of it in the shadow.
                if (Every(ref untilPuff, 0.025f, deltaTime))
                {
                    Vector3 near = PlayerOnFloor + Polar(Random.value * 360f, Mathf.Sqrt(Random.value) * 10f);
                    if (near.magnitude < FloorRadius - 0.5f && !Sunlight.InHand(near)) HeatShimmer.Play(near + Vector3.up * 0.1f, 2f);
                }
                ShowShadow(hand, 1f);
                sunSize = 1.5f + 0.1f * Mathf.Sin(Time.time * 23f);
                untilBlaze -= deltaTime;
                if (untilBlaze <= 0f)
                {
                    untilBlaze = BlazeEvery;
                    // Fire stands up all over the floor, never in the shadow.
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 point = Polar(Random.value * 360f, Mathf.Sqrt(Random.value) * (FloorRadius - 1f));
                        if (Sunlight.InHand(point)) continue;
                        Vfx.Play(VfxKind.Fire, point + Vector3.up * 0.2f, Vector3.up, 2.2f);
                        Vfx.Play(VfxKind.Smoke, point + Vector3.up * 1.2f, Vector3.up, 0.7f, Smoke);
                    }
                    if (target != null && victim != null)
                    {
                        if (Sunlight.InHand(target.position)) BlazeSpared++;
                        else if (PlayerOnFloor.magnitude <= FloorRadius + 0.5f)
                        {
                            BlazeBites++;
                            blowName = "the blaze";
                            Deal(new Hit(BlazeDamage, 0f, Vector3.down, false, true), 0f);
                        }
                    }
                }
                return;
            }
            // Spent. It comes down beside the shadow, folds, and lies on the sun.
            if (look != null) look.Blaze(0f);
            if (Blazing)
            {
                Blazing = false;
                floorMark.Hide();
                shadowMark.Hide();
                sunSize = 1f;
                sinceZenith = 0f;
                Vector3 along = hand != null ? hand.Direction : Vector3.forward;
                landAt = Clear(along * 9f + new Vector3(along.z, 0f, -along.x) * 7f, 9.5f, FloorRadius - 6f);
                sunState = SunState.Held;
                toFalcon = false;
                Enter(Mode.Spent);
            }
        }

        private void ShowShadow(ShadowHand hand, float strength)
        {
            if (hand == null) return;
            Vector3 along = hand.Direction;
            float wide = hand.HalfWidth * 2f;
            // A little outside the shade itself, so its pale edge shows all round the dark.
            wide += 0.5f;
            shadowMark.Show(hand.Centre + along * (hand.From - 0.25f) + Vector3.up * 0.07f, Quaternion.LookRotation(along), 0f, PlainMarker, true, wide, (hand.To - hand.From + 0.5f) / (LaneUnit * wide), strength);
        }

        private void AdvanceSpent(float deltaTime)
        {
            if (step == 0)
            {
                bool there = Fly(landAt + Vector3.up * 1.4f, 13f, deltaTime);
                flap = Mathf.MoveTowards(flap, -8f, 60f * deltaTime);
                pitch = Mathf.MoveTowards(pitch, 10f, 60f * deltaTime);
                sunAt = Vector3.MoveTowards(sunAt, HeldSunPoint(), 40f * deltaTime);
                if (!there && elapsed < 3f) return;
                step = 1;
                elapsed = 0f;
                at = landAt;
                falcon = false;
                Turn(PlayerOnFloor - landAt, 3600f, 1f);
                if (bulk != null) bulk.enabled = true;
                Vfx.Play(VfxKind.Dust, at, Vector3.up, 2.5f, Sand);
                if (shaker != null) shaker.Shake(0.5f, 0.35f);
                // Surviving the blaze is always paid: the head lies open.
                headOpen = true;
                SetOpen(true, true);
            }
            lift = Mathf.MoveTowards(lift, 0.15f, 2f * deltaTime);
            pitch = Mathf.MoveTowards(pitch, 13f, 60f * deltaTime);
            if (elapsed < 1.5f + SpentSeconds) return;
            EndAttack();
        }

        // ------------------------------------------------------------------ exposed, broken, dying

        private void AdvanceExposed(float deltaTime)
        {
            flap = Mathf.MoveTowards(flap, 0f, 200f * deltaTime);
            if (elapsed < ExposedSeconds)
            {
                // The scarab on its back with its legs working; the falcon flat on the floor with its wings spread.
                // Lying on its back it lies on the floor, not in it: KeepInside lifts it by what went under.
                at.y = Mathf.MoveTowards(at.y, falcon ? 0.5f : 0f, 30f * deltaTime);
                roll = Mathf.MoveTowards(roll, falcon ? 0f : 180f, 520f * deltaTime);
                pitch = Mathf.MoveTowards(pitch, falcon ? 8f : 0f, 120f * deltaTime);
                lift = Mathf.MoveTowards(lift, falcon ? 1f : 0.2f, 3f * deltaTime);
                // Wings spread flat on the paving, not through it.
                if (falcon) flap = Mathf.MoveTowards(flap, 10f, 200f * deltaTime);
                legPhase += 14f * deltaTime;
                if (bulk != null) bulk.enabled = true;
                return;
            }
            // Up, and going back for the sun: it holds firm now (ShapePoise), though blows still hurt it.
            Poise.Returning = true;
            if (falcon)
            {
                ReturnOnFoot(deltaTime);
                return;
            }
            // The scarab rights itself, digs into the court, goes to the sun underground under a ridge of cracking
            // paving, bursts up beside it and takes it up: it never vanishes and never pops back.
            float up = elapsed - ExposedSeconds;
            if (step == 0)
            {
                roll = Mathf.MoveTowards(roll, 0f, 420f * deltaTime);
                pitch = Mathf.MoveTowards(pitch, 0f, 80f * deltaTime);
                lift = Mathf.MoveTowards(lift, 0f, 2f * deltaTime);
                at.y = Mathf.MoveTowards(at.y, 0f, 6f * deltaTime);
                legPhase += 10f * deltaTime;
                if (up < 0.5f && Mathf.Abs(roll) > 1f) return;
                // A sun lying within a few steps is walked to; further off it digs.
                Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
                step = toSun.magnitude > 7.5f ? 1 : 10;
                if (step == 1) BeginDig(false);
                prepare = elapsed;
                return;
            }
            if (step == 10)
            {
                // Near enough to walk: round to the sun's far side and take it up.
                Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
                Turn(toSun, 200f, deltaTime);
                bool there = toSun.magnitude <= 4.6f || Walk(SunSide(), 4.5f, deltaTime, false) || elapsed - prepare > 3f;
                if (!there) return;
                step = 11;
                prepare = elapsed;
                return;
            }
            if (step == 11)
            {
                Turn(OnFloor(sunAt) - OnFloor(at), 300f, deltaTime);
                if (elapsed - prepare < 0.8f) return;
                TakeSunUp();
                return;
            }
            if (step == 1)
            {
                if (!DigIn(deltaTime)) return;
                step = 2;
                return;
            }
            if (step == 2)
            {
                if (!Tunnel(SunSide(), RidgeSpeed, deltaTime)) return;
                // Up beside the sun, facing it.
                yaw = YawOf(OnFloor(sunAt) - burrowAt);
                BeginComingUp(false);
                step = 3;
                return;
            }
            if (step == 3)
            {
                if (!ComeUp(deltaTime, 0f, EmergeSeconds)) return;
                SetOpen(true, true);
                step = 4;
                prepare = elapsed;
                return;
            }
            // It takes the sun up between its forelegs.
            Turn(OnFloor(sunAt) - OnFloor(at), 300f, deltaTime);
            if (elapsed - prepare < 0.8f) return;
            TakeSunUp();
        }

        // The falcon down on the floor hops to the sun, still open; if the sun lies too far it rolls the sun to itself.
        private void ReturnOnFoot(float deltaTime)
        {
            float most = Furious ? ExposedMostInFury : ExposedMost;
            roll = Mathf.MoveTowards(roll, 0f, 400f * deltaTime);
            pitch = Mathf.MoveTowards(pitch, 0f, 60f * deltaTime);
            Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
            const float Reach = 2.6f;
            if (step == 0)
            {
                if (toSun.magnitude > Reach) Walk(OnFloor(sunAt) - toSun.normalized * (Reach - 0.2f), 3f, deltaTime);
                if (elapsed >= most && sunRollLeft <= 0f && !rolling.Moving)
                {
                    sunRollTo = KeepSun(OnFloor(at) + (toSun.sqrMagnitude > 0.01f ? toSun.normalized : Facing(yaw)) * (Reach - 0.4f));
                    sunRollLeft = 0.8f;
                }
                if (toSun.magnitude > Reach + 0.2f) return;
                step = 1;
                prepare = elapsed;
                Turn(toSun, 3600f, 1f);
            }
            Turn(toSun, 200f, deltaTime);
            if (elapsed - prepare < 1f) return;
            TakeSunUp();
        }

        // Where the scarab stands to take the sun up: beside it, on whichever side keeps both clear of the wall and the
        // stone, facing it.
        private Vector3 SunSide()
        {
            Vector3 sunFlat = OnFloor(sunAt);
            Vector3 from = underground ? burrowAt : OnFloor(at);
            Vector3 toSun = sunFlat - from;
            Vector3 way = toSun.sqrMagnitude > 0.01f ? toSun.normalized : Facing(yaw);
            // Near the stone it comes up on the sun's outer side; near the wall, on its inner side.
            if (sunFlat.magnitude < 8f) way = -sunFlat.normalized;
            else if (sunFlat.magnitude > FloorRadius - 7f) way = sunFlat.normalized;
            return Clear(sunFlat - way * 4.4f, ObeliskClear, FloorRadius - 3.6f);
        }

        // It has the sun again: the fight goes on, and for a while its balance cannot be broken.
        private void TakeSunUp()
        {
            sunState = SunState.Held;
            rolling.Stop();
            sunPart.SetVulnerable(false);
            if (!sun.gameObject.activeSelf) sun.gameObject.SetActive(true);
            Vfx.Play(VfxKind.Fire, sunAt, Vector3.up, 3f);
            if (falcon && bulk != null) bulk.enabled = false;
            knockCount = 0f;
            Poise.Recovered();
            var next = afterFetch;
            afterFetch = SunBearerAttack.None;
            EndAttack();
            if (next != SunBearerAttack.None) Begin(next);
        }

        private void AdvanceStagger(float deltaTime)
        {
            // Down on its face, the head on the floor.
            at.y = Mathf.MoveTowards(at.y, falcon ? 0.5f : 0f, 30f * deltaTime);
            pitch = Mathf.MoveTowards(pitch, 16f, 160f * deltaTime);
            roll = Mathf.MoveTowards(roll, 6f, 200f * deltaTime);
            flap = Mathf.MoveTowards(flap, -6f, 200f * deltaTime);
            lift = Mathf.MoveTowards(lift, falcon ? 1f : 0.3f, 2f * deltaTime);
            if (bulk != null) bulk.enabled = true;
            if (elapsed >= 0.4f && step == 0)
            {
                step = 1;
                Vfx.Play(VfxKind.Impact, at + Facing(yaw) * 2f + Vector3.up);
                Vfx.Play(VfxKind.Dust, at + Facing(yaw) * 2.5f, Vector3.up, 2f, Sand);
                if (shaker != null) shaker.Shake(0.6f, 0.35f);
            }
            if (elapsed < StaggeredSeconds) return;
            Stagger.Reset();
            BeginExposed(true);
        }

        // It drags itself at the sun, which gutters and goes out. Then it is still, the gold gone dark; then the cases
        // fall open and it sinks into ash. The dull sun rolls into the obelisk's shadow, shudders with light, and cracks open:
        // the man is lying where it lay. Then nothing, for a breath, before he speaks.
        private void AdvanceDying(float deltaTime)
        {
            at.y = Mathf.MoveTowards(at.y, 0f, 20f * deltaTime);
            if (elapsed < SunOutAt)
            {
                float shudder = Mathf.Sin(Time.time * 47f);
                Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
                Turn(toSun, 120f, deltaTime);
                Walk(OnFloor(at) + (toSun.sqrMagnitude > 0.01f ? toSun.normalized : Facing(yaw)), 1.2f, deltaTime, false);
                pitch = 10f * elapsed / SunOutAt + shudder * 1.5f;
                roll = shudder * 3f;
                lift = Mathf.MoveTowards(lift, 0.45f, deltaTime);
                if (Every(ref untilEmber, 0.1f, deltaTime)) Vfx.Play(VfxKind.Embers, sunAt, Vector3.up, 1.5f);
            }
            else
            {
                if (step == 0)
                {
                    // The sun goes out.
                    step = 1;
                    Vfx.Play(VfxKind.Burst, sunAt, Vector3.up, 3f);
                    Vfx.Play(VfxKind.Ring, OnFloor(sunAt) + Vector3.up * 0.2f, Vector3.up, 1f, Gold);
                    Vfx.Play(VfxKind.Ash, sunAt, Vector3.up, 3f, Cinder);
                    ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.32f), 0.1f);
                    if (shaker != null) shaker.Shake(0.5f, 0.4f);
                    var hand = Sunlight.Hand;
                    sunRollTo = KeepSun((hand != null ? hand.Centre + hand.Direction * SunRestsAt : Vector3.forward * SunRestsAt));
                    sunRollLeft = 2f;
                }
                // The shadow goes on turning while the sun rolls: it rolls to where the shadow is, not where it was.
                if (sunRollLeft > 0f && Sunlight.Hand != null) sunRollTo = KeepSun(Sunlight.Hand.Centre + Sunlight.Hand.Direction * SunRestsAt);
                float apart = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - ApartAt) / (GoneAt - ApartAt)));
                pitch = Mathf.MoveTowards(pitch, 14f, 30f * deltaTime);
                roll = Mathf.MoveTowards(roll, 0f, 30f * deltaTime);
                flap = Mathf.MoveTowards(flap, -20f, 30f * deltaTime);
                lift = Mathf.Max(lift, apart);
                squash = Mathf.Lerp(1f, 0.05f, apart);
                size = Mathf.Lerp(1f, 0.85f, apart);
                if (apart > 0f && step == 1 && Every(ref untilPuff, 0.033f, deltaTime))
                    Vfx.Play(VfxKind.Ash, at + new Vector3(Random.Range(-2.4f, 2.4f), Random.Range(0.3f, 2.2f) * squash, Random.Range(-2.4f, 2.4f)), Vector3.up, 2f, Cinder);
                if (elapsed >= GoneAt && step == 1)
                {
                    step = 2;
                    body.gameObject.SetActive(false);
                    if (shadowRenderer != null) shadowRenderer.enabled = false;
                    Vfx.Play(VfxKind.Ash, at + Vector3.up * 0.4f, Vector3.up, 4f, Cinder);
                    Vfx.Play(VfxKind.Embers, at + Vector3.up * 0.4f, Vector3.up, 2f);
                }
                // Light shows through it before it goes: a shudder, then it cracks open in a flash, and he is lying there.
                if (step == 2 && elapsed >= CrackAt - 0.6f && Every(ref untilPuff, 0.08f, deltaTime))
                    Vfx.Play(VfxKind.HeavySpark, sunAt + Random.onUnitSphere * 0.9f * sunSize * 1.5f, Vector3.up, 1f, SunWhite);
                if (elapsed >= CrackAt && step == 2)
                {
                    step = 3;
                    SunCracked = true;
                    CrackedAt = OnFloor(sunAt);
                    sun.gameObject.SetActive(false);
                    Vfx.Play(VfxKind.Burst, sunAt, Vector3.up, 3f);
                    Vfx.Play(VfxKind.Impact, sunAt, Vector3.up, 2f, SunWhite);
                    Vfx.Play(VfxKind.Ring, CrackedAt + Vector3.up * 0.2f, Vector3.up, 1.6f, Gold);
                    for (int i = 0; i < 6; i++)
                        Vfx.Play(VfxKind.HeavySpark, sunAt, Quaternion.Euler(0f, i * 60f + 20f, 0f) * (Vector3.forward + Vector3.up * 0.4f), 1.4f, Gold);
                    Vfx.Play(VfxKind.Ash, CrackedAt + Vector3.up * 0.5f, Vector3.up, 2.5f, Cinder);
                    ScreenFlash.Flash(new Color(1f, 0.5f, 0.16f, 0.56f), 0.14f);
                    if (shaker != null) shaker.Shake(0.45f, 0.4f);
                    if (hitStop != null) hitStop.Slow(0.8f, 0.35f);
                }
            }
            // It rolls into the shadow and settles there, still the size of a man, not shrunk to a coal.
            sunSize = Mathf.Lerp(1f, CrackSize, Mathf.Clamp01((elapsed - SunOutAt) / 2f));
            if (elapsed < DeathSeconds) return;
            body.gameObject.SetActive(false);
            if (shadowRenderer != null) shadowRenderer.enabled = false;
            if (finder != null) finder.enabled = false;
            Release();
            if (shaker != null) shaker.ClearFraming();
            sun.gameObject.SetActive(false);
            mode = Mode.Dead;
            RaiseDied();
        }

        // ------------------------------------------------------------------ the sun, and the floor it leaves hot

        private void AdvanceSun(float deltaTime)
        {
            if (parryLeft >= 0f)
            {
                AdvanceParry(deltaTime);
                return;
            }
            if (sunState == SunState.Held && mode != Mode.Roll && mode != Mode.Pass)
                sunAt = Vector3.MoveTowards(sunAt, HeldSunPoint(), 40f * deltaTime);
            if (sunState == SunState.Under)
            {
                // Carried underground: kept under the ridge's head, out of sight.
                sunAt = burrowAt + Vector3.down * 4f;
                return;
            }
            if (sunState == SunState.Loose)
            {
                Vector3 flat = OnFloor(sunAt);
                if (sunRollLeft > 0f)
                {
                    float pace = (sunRollTo - flat).magnitude / sunRollLeft * deltaTime;
                    Vector3 next = Vector3.MoveTowards(flat, sunRollTo, pace);
                    sunSpin += (next - flat).magnitude / 1.5f * Mathf.Rad2Deg;
                    flat = next;
                    sunRollLeft -= deltaTime;
                }
                else if (rolling.Moving) flat = RollLooseSun(flat, deltaTime);
                sunAt = new Vector3(flat.x, Mathf.MoveTowards(sunAt.y, 1.5f * sunSize, 30f * deltaTime), flat.z);
            }
            // The burning sun on the floor burns whoever touches it.
            untilTouch -= deltaTime;
            bool burns = sunState != SunState.Loose && sunAt.y < 3.2f && mode != Mode.Entering && mode != Mode.Roll && mode != Mode.Ricochet && mode != Mode.Grab && mode != Mode.Burrow && !IsDead;
            if (burns && untilTouch <= 0f && target != null && (PlayerOnFloor - OnFloor(sunAt)).magnitude <= 2.1f)
            {
                untilTouch = 0.7f;
                blowName = "the sun touched";
                Vector3 off = PlayerOnFloor - OnFloor(sunAt);
                Deal(new Hit(SunTouchDamage, 0f, off.sqrMagnitude > 0.01f ? off.normalized : Vector3.back, false, true, sunAt), 0f);
            }
        }

        // A lane of hot floor from laneFrom along laneAlong, laneLength long when it has all caught, of which length has
        // caught so far. One that does not bite is only drawn (the Flares' lines, or a lane laid without fire because it
        // would have shut the player off from the shape).
        private int LayLane(float length, float width, float seconds, bool bites = true)
        {
            int slot = FreeHot();
            hotIsLane[slot] = true;
            hotBites[slot] = bites;
            hotFrom[slot] = laneFrom;
            hotAlong[slot] = laneAlong;
            hotFull[slot] = Mathf.Max(0.1f, laneLength);
            hotLength[slot] = Mathf.Clamp(length, 0.01f, hotFull[slot]);
            hotWide[slot] = width;
            hotSeconds[slot] = seconds;
            hotLeft[slot] = seconds + HotResidual;
            return slot;
        }

        // The growing lane reaches this far down from its start: the sun has passed that far.
        private void Grown(float length)
        {
            if (growing < 0 || hotLeft[growing] <= 0f || !hotIsLane[growing]) return;
            hotLength[growing] = Mathf.Clamp(length, 0.01f, hotFull[growing]);
        }

        // What the sun throws at its front foot as it scorches its way along the lane: flame and sand to either side, and embers off its top.
        private void BowWave(Vector3 foot, float width, float ahead)
        {
            var side = new Vector3(laneAlong.z, 0f, -laneAlong.x);
            float sign = Random.value < 0.5f ? -1f : 1f;
            Vector3 front = foot + laneAlong * ahead;
            Vfx.Play(VfxKind.Fire, front + side * (sign * Random.Range(0.2f, width * 0.4f)) + Vector3.up * 0.25f, Vector3.up, 1.1f);
            Vfx.Play(VfxKind.Dust, front + side * (sign * width * 0.45f) + Vector3.up * 0.1f, (side * sign + laneAlong).normalized, 1f, Sand);
            Vfx.Play(VfxKind.Embers, foot + Vector3.up * 2.6f, -laneAlong + Vector3.up, 0.5f);
        }

        private void LayCrater(Vector3 centre, float radius, float seconds, bool bites = true)
        {
            int slot = FreeHot();
            hotIsLane[slot] = false;
            hotBites[slot] = bites;
            hotFrom[slot] = centre;
            hotWide[slot] = radius;
            hotSeconds[slot] = seconds;
            hotLeft[slot] = seconds + HotResidual;
        }

        // An empty place for a hot patch, or the one nearest cold.
        private int FreeHot()
        {
            int best = 0;
            for (int i = 0; i < HotMost; i++)
                if (hotLeft[i] < hotLeft[best]) best = i;
            return best;
        }

        private void ClearHot()
        {
            for (int i = 0; i < HotMost; i++) hotLeft[i] = 0f;
            HotLanes = HotPatches = 0;
            hotClock.Reset();
            if (scorch != null) scorch.HideAll();
            if (hotLanes == null) return;
            foreach (var mark in hotLanes) mark.Hide();
            foreach (var mark in hotCircles) mark.Hide();
        }

        // How hot a patch still is, 1 lit to 0 cold, and the share of its width that still burns: the fire dies from its
        // edges in, and only what still burns bites (the flames over it die by the same rule, clover_flame_field.shader).
        private float HeatOf(int i) => hotSeconds[i] > 0f ? Mathf.Clamp01((hotLeft[i] - HotResidual) / hotSeconds[i]) : 0f;
        private static float BurningShare(float heat) => Mathf.Clamp01(heat * 1.35f - 0.04f);

        /// <summary>True for a point on floor that still burns and bites (whether or not the shadow lies across it).</summary>
        public bool OnHotFloor(Vector3 point)
        {
            for (int i = 0; i < HotMost; i++)
            {
                if (hotLeft[i] <= HotResidual || !hotBites[i]) continue;
                float share = BurningShare(HeatOf(i));
                if (share <= 0f) continue;
                Vector3 offset = OnFloor(point) - hotFrom[i];
                if (!hotIsLane[i])
                {
                    float radius = hotWide[i] * share;
                    if (offset.sqrMagnitude <= radius * radius) return true;
                    continue;
                }
                float along = offset.x * hotAlong[i].x + offset.z * hotAlong[i].z;
                float across = Mathf.Abs(offset.x * hotAlong[i].z - offset.z * hotAlong[i].x);
                if (along >= 0f && along <= hotLength[i] && across <= hotWide[i] * 0.5f * share) return true;
            }
            return false;
        }

        // Whether a lane of fire laid here would still leave the player a way to the shape over floor that is not
        // burning (CourtReach): what burns now, and the new lane, on a grid of one-metre cells.
        private bool FireLeavesWay(Vector3 from, Vector3 along, float length, float width, Vector3 goal)
        {
            if (target == null) return true;
            ReachOverHotFloor();
            reach.Lane(from, along, length, width);
            return reach.Reaches(PlayerOnFloor, goal);
        }

        // The grid of the court with what burns now blocked; lanes about to be laid are added after.
        private void ReachOverHotFloor()
        {
            reach.Clear();
            for (int i = 0; i < HotMost; i++)
            {
                // Floor that will have gone out within a second does not shut anyone in.
                if (!hotBites[i] || hotLeft[i] - HotResidual < 1f) continue;
                float share = BurningShare(HeatOf(i));
                if (hotIsLane[i]) reach.Lane(hotFrom[i], hotAlong[i], hotFull[i], hotWide[i] * share);
                else reach.Disc(hotFrom[i], hotWide[i] * share);
            }
        }

        private void AdvanceHot(float deltaTime)
        {
            int lanesHot = 0, patches = 0, laneSlot = 0, discSlot = 0;
            untilFlame -= deltaTime;
            bool puff = untilFlame <= 0f;
            if (puff) untilFlame = 0.22f;
            if (puff) smokeTurn = !smokeTurn;
            for (int i = 0; i < HotMost; i++)
            {
                if (hotLeft[i] <= 0f) continue;
                hotLeft[i] -= deltaTime;
                if (hotLeft[i] <= 0f) continue;
                float heat = HeatOf(i);
                bool burning = hotLeft[i] > HotResidual;
                // After it has burnt out it lies as char with fading cracks, and then is gone.
                float fade = burning ? 1f : Mathf.Clamp01(hotLeft[i] / HotResidual);
                if (burning && hotBites[i])
                {
                    patches++;
                    if (hotIsLane[i]) lanesHot++;
                }
                // The Flares' lines are slashes of flame, not a burning floor: their fire is lower.
                float flames = hotBites[i] ? 1f : 0.7f;
                if (hotIsLane[i])
                {
                    if (scorch != null && laneSlot < scorch.Lanes)
                        scorch.Lane(laneSlot++, hotFrom[i], hotAlong[i], hotWide[i], hotFull[i], hotLength[i] / hotFull[i], heat, fade, flames);
                    if (puff && burning)
                    {
                        // Smoke off it and heat shimmering over it; the flames themselves are drawn by the field.
                        float along = Random.value * hotLength[i];
                        float across = (Random.value - 0.5f) * hotWide[i] * BurningShare(heat);
                        HotPuff(hotFrom[i] + hotAlong[i] * along + new Vector3(hotAlong[i].z, 0f, -hotAlong[i].x) * across, hotLeft[i] - HotResidual);
                    }
                }
                else
                {
                    if (scorch != null && discSlot < scorch.Discs)
                        scorch.Disc(discSlot++, hotFrom[i], hotWide[i], heat, fade, flames);
                    if (puff && burning) HotPuff(hotFrom[i] + Facing(Random.value * 360f) * (Random.value * hotWide[i] * BurningShare(heat)), hotLeft[i] - HotResidual);
                }
            }
            if (scorch != null)
            {
                for (int i = laneSlot; i < scorch.Lanes; i++) scorch.HideLane(i);
                for (int i = discSlot; i < scorch.Discs; i++) scorch.HideDisc(i);
            }
            HotLanes = lanesHot;
            HotPatches = patches;
            if (target == null || victim == null) return;
            // Hot floor bites whoever stands on it, half a second after they step on and every half second after; never
            // through a dodge roll, never in the air, and under the obelisk's shadow it is cold.
            bool standsOn = patches > 0 && target.position.y < 1f && !carrying && OnHotFloor(target.position) && !Sunlight.InHand(target.position);
            bool rollingThrough = victimBody != null && victimBody.Dodge.IsRolling;
            if (!hotClock.Tick(deltaTime, standsOn, rollingThrough)) return;
            HotBites++;
            blowName = "hot floor";
            Deal(new Hit(HotDamage, 0f, Vector3.down, false, true), 0f);
        }

        // A wisp of smoke and a shimmer of heat off burning floor; cold under the obelisk's shadow, where only the smoke rises.
        private void HotPuff(Vector3 point, float left)
        {
            bool shaded = Sunlight.InHand(point);
            if (!shaded && left > 1f) HeatShimmer.Play(point + Vector3.up * 0.1f, 1f);
            if (smokeTurn || shaded || left < 1.5f)
                Vfx.Play(VfxKind.Smoke, point + Vector3.up * 0.6f, Vector3.up, left < 1.5f ? 0.9f : 0.7f, Smoke);
        }

        // ------------------------------------------------------------------ blows

        private void Strike(Vector3 point, float radius, float damage)
        {
            if (target == null || victim == null) return;
            Vector3 offset = target.position - point;
            offset.y = 0f;
            if (offset.magnitude > radius + 0.35f) return;
            blowName = "something falling";
            Deal(new Hit(damage, 0f, offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.back, false, true, point), 0f);
        }

        // The blow is given to the player. One turned at the last instant is thrown back on its balance, and may break it:
        // whoever calls this must look at what it is doing afterwards before going on.
        private HitOutcome Deal(in Hit hit, float balanceIfTurned)
        {
            if (victim == null) return HitOutcome.Missed;
            if (victimBody != null && victimBody.IsLeaping) return HitOutcome.Missed;
            BlowsStruck++;
            LastBlow = blowName ?? mode.ToString();
            blowName = null;
            var outcome = victim.TakeHit(hit);
            if (outcome != HitOutcome.PerfectGuard || IsStaggered || IsDead || balanceIfTurned <= 0f) return outcome;
            flashLeft = 0.09f;
            if (Poise.Add(Stagger, balanceIfTurned)) BeginStagger();
            return outcome;
        }

        private void HideMarkers()
        {
            if (look != null)
            {
                look.FlaresDone();
                look.GustWall(false, Vector3.zero, Vector3.forward, 0f, 0f);
            }
            foreach (var mark in lanes) mark.Hide();
            foreach (var mark in circles) mark.Hide();
            floorMark.Hide();
            shadowMark.Hide();
            for (int i = 0; i < fallLeft.Length; i++)
            {
                fallLeft[i] = 0f;
                falls[i].Hide();
                if (look != null) look.Fallen(i);
            }
            landed = false;
            sunSize = 1f;
        }

        // ------------------------------------------------------------------ the body

        private void Pose()
        {
            body.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(pitch, 0f, roll));
            body.localScale = new Vector3(size, size * squash, size);
            if (scarab.gameObject.activeSelf == falcon) scarab.gameObject.SetActive(!falcon);
            if (falconParts.gameObject.activeSelf != falcon) falconParts.gameObject.SetActive(falcon);
            // The wing cases: the scarab's back when closed, the falcon's shoulders when lifted and splayed.
            float raised = Mathf.Lerp(-5f, 70f, lift), splay = 25f * lift;
            caseLeft.localRotation = Quaternion.Euler(raised, splay, 0f);
            caseRight.localRotation = Quaternion.Euler(raised, -splay, 0f);
            // Six legs in two threes, as a beetle walks: each three swings forward lifted and back planted, the left
            // side mirrored. The pieces are rigid, so the whole leg turns at the hip.
            for (int i = 0; i < legs.Length; i++)
            {
                bool left = i % 2 == 0;
                bool firstThree = i == 0 || i == 3 || i == 4;
                float phase = legPhase + (firstThree ? 0f : Mathf.PI);
                float reach = Mathf.Sin(phase) * 17f * (left ? -1f : 1f);
                float lifted = Mathf.Max(0f, Mathf.Cos(phase)) * 12f * (left ? -1f : 1f);
                legs[i].localRotation = legRest[i] * Quaternion.Euler(0f, reach, lifted);
            }
            if (falcon)
            {
                for (int i = 0; i < wingLeft.Length; i++)
                {
                    // The outer lengths trail the inner one, so the wing bends as it beats.
                    float bend = i == 0 ? flap : flap * 0.35f;
                    wingLeft[i].localRotation = wingLeftRest[i] * Quaternion.AngleAxis(-bend, wingLeftAxis[i]);
                    if (i < wingRight.Length) wingRight[i].localRotation = wingRightRest[i] * Quaternion.AngleAxis(bend, wingRightAxis[i]);
                }
            }
            bool high = falcon && at.y > 2.5f && mode != Mode.Zenith;
            if (shadowRenderer != null)
            {
                if (shadowRenderer.enabled != high) shadowRenderer.enabled = high;
                if (high)
                {
                    falconShadow.position = OnFloor(at) + Vector3.up * 0.045f;
                    // Over the stoop's hold the shadow tightens on whoever it is about to fall on.
                    float tight = mode == Mode.Stoop ? Mathf.Clamp01((elapsed - stoopFollow) / StoopHold) : mode == Mode.Grab ? Mathf.Clamp01((elapsed - GrabFollow) / GrabHold) : 0f;
                    float across = Mathf.Lerp(5f, 3.6f, tight);
                    falconShadow.localScale = new Vector3(across, 0.004f, across);
                }
            }
            // Struck back by a parry it burns white until it strikes the form, and only then goes dull.
            bool loose = sunState == SunState.Loose && parryLeft < 0f;
            // Dying, it gutters faster and faster before it goes out.
            if (mode == Mode.Dying && elapsed < SunOutAt) loose = Mathf.Repeat(elapsed * Mathf.Lerp(3f, 12f, elapsed / SunOutAt), 1f) > 0.6f;
            // Before it cracks the light inside shows through, faster and faster.
            else if (mode == Mode.Dying && elapsed >= CrackAt - 0.6f && elapsed < CrackAt) loose = Mathf.Repeat((elapsed - CrackAt + 0.6f) * Mathf.Lerp(4f, 16f, (elapsed - CrackAt + 0.6f) / 0.6f), 1f) < 0.55f;
            if (sunBurning.activeSelf == loose) sunBurning.SetActive(!loose);
            if (sunDull.activeSelf != loose) sunDull.SetActive(loose);
            sun.SetPositionAndRotation(sunAt, Quaternion.Euler(sunSpin, yaw, 0f));
            sun.localScale = Vector3.one * sunSize;
        }

        // The falcon is fourteen metres across and the camera low: over the south of the court it can cover the
        // player entirely. While its body or the sun is on the line from the camera to them, a ring is drawn
        // through it where they stand.
        private void FindPlayer()
        {
            bool hidden = false;
            Vector3 to = target != null ? target.position + Vector3.up : Vector3.zero;
            if (shaker != null && target != null && !IsDead)
            {
                Vector3 eye = shaker.transform.position;
                hidden = Covers(eye, to, at + Vector3.up, falcon ? 4.6f : 2.6f) || Covers(eye, to, sunAt, 1.7f * sunSize);
            }
            PlayerHidden = hidden;
            if (finder == null) return;
            if (finder.enabled != hidden) finder.enabled = hidden;
            if (hidden) finder.transform.SetPositionAndRotation(to, shaker.transform.rotation);
        }

        private static bool Covers(Vector3 eye, Vector3 to, Vector3 middle, float round)
        {
            Vector3 line = to - eye;
            float length = line.magnitude;
            if (length < 1f) return false;
            line /= length;
            float along = Vector3.Dot(middle - eye, line);
            // Only what is nearer the camera than the player hides them.
            if (along <= 0f || along >= length - 0.6f) return false;
            return (eye + line * along - middle).sqrMagnitude < round * round;
        }

        // White for an instant when struck, a gold pulse while it lies broken, and its fire out while it lies exposed.
        private void Paint()
        {
            bool flash = flashLeft > 0f;
            flashLeft -= Time.unscaledDeltaTime;
            bool pulse = IsStaggered && Mathf.Repeat(Time.unscaledTime * 4f, 1f) < 0.5f;
            bool dark = mode == Mode.Exposed || (mode == Mode.Dying && elapsed >= SunOutAt);
            // The shell heating under a player who has stood on its flank, from dull red to bright, where they stand;
            // and the whole shell burning where the rolled sun struck it.
            float camping = mode == Mode.ShellBurst ? 1f : campHeat;
            bool hot = camping > 0.01f || shellFireLeft > 0f;
            bool lit = flash || pulse || dark || hot;
            if (!lit && !flashing) return;
            flashing = lit;
            Vector3 towardPlayer = target != null ? OnFloor(target.position - at) : Vector3.zero;
            towardPlayer = towardPlayer.sqrMagnitude > 0.01f ? towardPlayer.normalized : Vector3.zero;
            float flicker = 0.8f + 0.2f * Mathf.Sin(Time.time * 23f) * Mathf.Sin(Time.time * 7f);
            foreach (var shown in renderers)
            {
                if (shown == null) continue;
                if (!lit)
                {
                    shown.SetPropertyBlock(null);
                    continue;
                }
                Color glow = flash ? new Color(1.5f, 1.5f, 1.5f) : pulse ? new Color(0.8f, 0.55f, 0.12f) : Color.black;
                if (!flash && hot)
                {
                    // Nearer the side the player stands on, hotter; a burning shell glows all over.
                    Vector3 side = OnFloor(shown.bounds.center - at);
                    float facing = side.sqrMagnitude > 0.01f ? Vector3.Dot(side.normalized, towardPlayer) : 0f;
                    float here = camping * Mathf.Clamp01(facing * 1.4f + 0.15f);
                    float heat = Mathf.Max(here, shellFireLeft > 0f ? 0.75f : 0f) * flicker;
                    glow += Color.Lerp(new Color(0.32f, 0.03f, 0f), new Color(2.4f, 0.75f, 0.1f), heat * heat) * Mathf.Clamp01(heat * 3f);
                }
                block.Clear();
                block.SetColor(Emission, glow);
                shown.SetPropertyBlock(block);
            }
        }
    }
}
