using Clover.Core;
using Clover.Player;
using Clover.World;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// A god in its godly form: too large to stand on the floor the player fights on, it climbs up beside it, and fights
    /// with its hands and its head. Its head, body and two hands are rigid pieces moved by this code. A hand slams down
    /// where the player stands, or sweeps the floor from one side to the other, or both close on the player in a clap,
    /// and then lie where they stopped for a moment: that is when they can be cut. The head breathes lightning down the
    /// floor, and bites at whoever stands under it, and is stuck for a moment when it has. Bolts are called down
    /// between. Blows on the hands wear down its balance; when that breaks, the head falls to the floor and can be
    /// struck, and finished. What it does next is decided by a <see cref="ColossusChooser"/>.
    /// </summary>
    public class Colossus : GodlyForm
    {
        private enum Mode { Hidden, Rising, Idle, Slam, Sweep, Bolts, Breath, Bite, Clap, Staggered, Recovering, Dying, Dead }

        [SerializeField] private Transform target;
        [SerializeField] private Transform head;
        [SerializeField] private Transform body;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private ColossusPart headPart;
        [SerializeField] private ColossusPart leftPart;
        [SerializeField] private ColossusPart rightPart;
        [SerializeField] private Renderer[] renderers = new Renderer[0];
        [SerializeField] private MeshFilter slamOutline;
        [SerializeField] private MeshFilter slamFill;
        [SerializeField] private MeshFilter sweepOutline;
        [SerializeField] private MeshFilter sweepFill;
        [SerializeField] private MeshFilter[] boltOutlines = new MeshFilter[0];
        [SerializeField] private MeshFilter[] boltFills = new MeshFilter[0];
        [SerializeField] private LineRenderer[] tethers = new LineRenderer[0];
        [SerializeField] private MeshFilter breathOutline;
        [SerializeField] private MeshFilter breathFill;
        [SerializeField] private MeshFilter biteOutline;
        [SerializeField] private MeshFilter biteFill;
        [SerializeField] private MeshFilter[] clapOutlines = new MeshFilter[0];
        [SerializeField] private MeshFilter[] clapFills = new MeshFilter[0];
        [SerializeField] private MeshFilter roarRing;
        [SerializeField] private MeshFilter[] poolMarks = new MeshFilter[0];
        [SerializeField] private LineRenderer[] beam = new LineRenderer[0];
        [SerializeField] private Renderer[] glowRenderers = new Renderer[0];
        [SerializeField] private int[] glowMaterials = new int[0];
        [SerializeField] private bool[] glowIsEye = new bool[0];

        [Header("The place")]
        /// <summary>Radius of the round floor, whose middle is the world's origin; the colossus stands beyond its north edge.</summary>
        public float FloorRadius = 17f;
        public float HandScale = 0.62f;
        public float HeadScale = 0.85f;

        [Header("Itself")]
        public float MaximumHealth = 760f;
        public float BalanceMaximum = 170f;
        public float StaggeredSeconds = 5.5f;
        /// <summary>Share of a blow's damage a hand lets through, and what the fallen head takes.</summary>
        public float HandDamage = 0.75f;
        public float HeadDamage = 1.6f;
        /// <summary>A finisher on the fallen head takes at least this share of all its health.</summary>
        public float FinisherShare = 0.22f;
        public float PerfectGuardBalance = 60f;
        public float RestBetween = 0.9f;
        /// <summary>The longer rest it owes the player after two heavy attacks: time to breathe and to drink.</summary>
        public float LullSeconds = 2.4f;
        /// <summary>What it does next and why. Its numbers can be changed here.</summary>
        public ColossusChooser Chooser = new ColossusChooser();
        /// <summary>The next attack when something outside has decided it (a probe, a scripted moment); None leaves it to the chooser.</summary>
        public ColossusAttack Queued = ColossusAttack.None;

        [Header("Where the player stands")]
        /// <summary>Within this many metres of the north rim's middle the player is under the head, and is bitten for it.</summary>
        public float BiteReach = 7f;
        /// <summary>Seconds the player must have stood under the head before it bites.</summary>
        public float BiteAfterSeconds = 1.2f;
        /// <summary>South of this line (metres along the floor, north positive) the player is far off, and is breathed on.</summary>
        public float FarSouthLine = -5f;

        [Header("The entrance")]
        /// <summary>All of it, from the first shake to the first attack. Nothing in it hurts, and nothing can hurt him.</summary>
        public float RiseSeconds = 4.2f;
        /// <summary>When each hand comes down on the floor, and when he roars.</summary>
        public float EntranceLeftSlamAt = 1f;
        public float EntranceRightSlamAt = 1.7f;
        public float EntranceRoarAt = 3.2f;
        /// <summary>Seconds a hand takes to come up over the rim before it falls, and the head to rear before the roar.</summary>
        public float EntranceHandRaise = 0.6f;
        public float EntranceRear = 0.45f;
        public float RoarSeconds = 0.7f;
        /// <summary>Seconds the roar's ring takes to cross the floor. It is only seen; it does nothing.</summary>
        public float RoarRingSeconds = 0.8f;
        /// <summary>Seconds between the bolts that fall round the rim while he climbs.</summary>
        public float EntranceBoltEvery = 0.3f;
        /// <summary>How far under the cloud he waits.</summary>
        public float RiseDepth = 16f;

        [Header("Fury")]
        /// <summary>Below this share of health he is in fury: faster, slamming twice, resting less, and the storm with him.</summary>
        public float FuryAt = 0.5f;
        public float FuryPace = 1.25f;
        /// <summary>The share of the usual rest he takes between attacks in fury. The lull after two heavy attacks is not cut.</summary>
        public float FuryRest = 0.55f;
        /// <summary>How many times brighter the eyes and the scar burn in fury.</summary>
        public float FuryEyeGlow = 2.6f;
        public float FuryScarGlow = 1.35f;
        /// <summary>Seconds between the bolts that fall round the rim in fury. They are scenery, off the floor.</summary>
        public float FuryRimBoltEvery = 0.9f;
        /// <summary>In fury each bolt of the bolts attack leaves a crackling pool: how wide, how long, how hard, how often.</summary>
        public float PoolRadius = 1.9f;
        public float PoolSeconds = 2f;
        public float PoolDamage = 8f;
        public float PoolTick = 0.5f;

        [Header("The slam")]
        public float SlamTelegraph = 1.2f;
        /// <summary>The hand stops following the player this long before it falls.</summary>
        public float SlamLock = 0.35f;
        public float SlamDrop = 0.11f;
        public float SlamRadius = 3f;
        public float SlamDamage = 28f;
        public float SlamRest = 2f;
        public float SlamHover = 7f;

        [Header("The sweep")]
        public float SweepTelegraph = 1.25f;
        public float SweepSeconds = 0.5f;
        public float SweepWidth = 4.6f;
        public float SweepDamage = 30f;
        public float SweepRest = 1.5f;

        [Header("The bolts")]
        public int BoltCount = 6;
        public float BoltEvery = 0.5f;
        public float BoltWarning = 0.95f;
        public float BoltRadius = 2.3f;
        public float BoltDamage = 16f;

        [Header("The breath")]
        public float BreathTelegraph = 1.3f;
        /// <summary>The lane stops following the player this long before the breath comes.</summary>
        public float BreathLock = 0.4f;
        /// <summary>Seconds the beam stands on the floor, and how often it hurts whoever is in it.</summary>
        public float BreathSeconds = 0.8f;
        public float BreathTick = 0.4f;
        public float BreathWidth = 5f;
        public float BreathDamage = 14f;
        /// <summary>Seconds the head hangs spent afterwards. It cannot be reached: this is rest, not an opening.</summary>
        public float BreathRest = 0.9f;
        /// <summary>How many times brighter the eyes burn as the breath gathers.</summary>
        public float BreathEyeGlow = 5f;
        public float BeamWidth = 0.4f;

        [Header("The bite")]
        public float BiteTelegraph = 1.1f;
        /// <summary>The circle stops following the player this long before the head falls.</summary>
        public float BiteLock = 0.3f;
        public float BiteDrop = 0.13f;
        public float BiteRadius = 3.8f;
        /// <summary>
        /// How far toward the rim from where the player stood the circle's middle lies. The head is longer than a man
        /// is tall many times over; it falls with its crown at the player's feet, so the player it misses, or who has
        /// guarded it, is left standing in front of it and not hidden under it.
        /// </summary>
        public float BiteLead = 3f;
        public float BiteDamage = 40f;
        /// <summary>Seconds the head lies with its face in the floor afterwards, open to blows.</summary>
        public float BiteStuck = 1.6f;
        /// <summary>What the stuck head takes of a blow: the plain damage, not what the broken head takes.</summary>
        public float BiteHeadDamage = 1f;
        /// <summary>How far behind the circle's middle, toward the rim, the head's neck comes to rest, so that its face is on the circle.</summary>
        public float BiteBack = 2.2f;

        [Header("The clap")]
        public float ClapTelegraph = 1.4f;
        /// <summary>The hands stop following the player this long before they close.</summary>
        public float ClapLock = 0.4f;
        public float ClapSeconds = 0.16f;
        /// <summary>How far to either side of the player the hands draw apart, which is also how long each mark is.</summary>
        public float ClapSpread = 9f;
        public float ClapWidth = 4.4f;
        public float ClapDamage = 36f;
        /// <summary>Seconds both hands lie stunned where they met: the longest opening on the hands.</summary>
        public float ClapRest = 2.6f;
        /// <summary>A clap turned at the last instant counts for this many perfect guards against his balance.</summary>
        public float ClapPerfectGuards = 2f;

        [Header("The death")]
        /// <summary>All of it, from the last blow to the empty rim.</summary>
        public float DeathSeconds = 5f;
        /// <summary>Real seconds everything stands still at the last blow.</summary>
        public float DeathFreeze = 0.35f;
        /// <summary>When the bolts begin to strike him, each hand breaks away, the head begins to fall, lands on the rim, and begins to slide off.</summary>
        public float DeathBoltsAt = 0.9f;
        public float DeathLeftBreaksAt = 1.5f;
        public float DeathRightBreaksAt = 2f;
        public float DeathHeadFallsAt = 2.6f;
        public float DeathHeadLandsAt = 3f;
        public float DeathHeadSlidesAt = 3.6f;
        public float DeathBoltEvery = 0.22f;
        /// <summary>How many times brighter the eyes burn, unsteadily, until the head is down.</summary>
        public float DeathEyeGlow = 3f;

        [Header("The ropes of lightning")]
        /// <summary>How wide the brightest strand is drawn, in metres. The others are thinner.</summary>
        public float TetherWidth = 0.075f;
        /// <summary>How far a strand's kinks stray from the straight line, in metres.</summary>
        public float TetherKink = 0.5f;
        /// <summary>How many times wider the strands of a hand are while that hand attacks.</summary>
        public float TetherAttackGain = 1.7f;
        /// <summary>Seconds between redraws of the strands.</summary>
        public float TetherRedraw = 0.05f;

        [Header("The view of it")]
        /// <summary>How many times further off the camera stands while it is in the fight, so its head and hands are in the picture.</summary>
        public float ViewDistance = 1.55f;
        /// <summary>The camera looks at a point this share of the way from the player to its head, up to the limit in metres.</summary>
        public float ViewLead = 0.5f;
        public float ViewLeadLimit = 3.8f;
        /// <summary>The same limit during his entrance and his death, when the rim is what there is to see.</summary>
        public float SceneLeadLimit = 7f;
        /// <summary>Degrees the camera's pitch is tipped toward the horizon, so more of what stands beyond the floor is seen.</summary>
        public float ViewPitch = -15f;

        public Color PlainMarker = new Color(1f, 0.93f, 0.75f, 1f);
        public Color RedMarker = new Color(1f, 0.08f, 0.05f, 1f);
        public Color RoarColour = new Color(0.75f, 0.9f, 1.3f, 1f);

        // Health, balance, afflictions and the three moments a director listens for are the godly form's own.
        public override bool IsStaggered => mode == Mode.Staggered;
        public override bool IsDead => mode == Mode.Dead || mode == Mode.Dying;
        public override bool HasRisen => mode != Mode.Hidden;
        /// <summary>The fight is on from the end of his entrance to the blow that kills him: the bar shows for as long.</summary>
        public override bool InFight => mode != Mode.Hidden && mode != Mode.Rising && mode != Mode.Dead && mode != Mode.Dying;
        public bool Furious => Health.Fraction <= FuryAt;
        // Counters so tests and probes can see what happened without watching the screen.
        public int Slams { get; private set; }
        public int Sweeps { get; private set; }
        public int BoltsCalled { get; private set; }
        public int Breaths { get; private set; }
        public int Bites { get; private set; }
        public int Claps { get; private set; }
        public int Lulls { get; private set; }
        public int PoolsLaid { get; private set; }
        public int PoolBurns { get; private set; }
        public int BlowsStruck { get; private set; }
        public int TimesStaggered { get; private set; }
        public override string Doing => mode.ToString();
        /// <summary>Seconds into what it is doing now, at its own pace.</summary>
        public override float DoingFor => elapsed;
        public override ColossusPart HeadPart => headPart;
        public ColossusPart LeftPart => leftPart;
        public ColossusPart RightPart => rightPart;

        private static readonly int MarkerColour = Shader.PropertyToID("_Color");
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private const int StrandsEach = 3;
        private const int StrandPoints = 12;
        private const int BeamPoints = 14;

        private Mode mode = Mode.Hidden;
        private float elapsed;
        private float rest;
        private IDamageable victim;
        private PlayerController victimBody;
        private TopDownCamera shaker;
        private HitStop hitStop;
        private MaterialPropertyBlock block;
        private Mesh slamMesh, sweepMesh, boltMesh, breathMesh, biteMesh, clapMesh, roarMesh, poolMesh;
        private Renderer slamOutlineRenderer, slamFillRenderer, sweepOutlineRenderer, sweepFillRenderer;
        private Renderer breathOutlineRenderer, breathFillRenderer, biteOutlineRenderer, biteFillRenderer, roarRenderer;
        private Renderer[] boltOutlineRenderers, boltFillRenderers, clapOutlineRenderers, clapFillRenderers, poolRenderers;
        private Vector3[] boltPoints;
        private float[] boltLeft;
        private int boltsToCall;
        private float untilBolt;
        private Vector3[] poolPoints;
        private float[] poolLeft;
        private float untilPoolBurn;
        private Transform hand;
        private ColossusPart handPart;
        private bool handIsLeft;
        private Vector3 slamPoint;
        private bool slamLanded;
        private bool secondSlamOwed;
        private float sweepZ;
        private float sweepFrom, sweepTo;
        private float lastSweepX;
        private bool sweepStruck;
        private Vector3 breathFrom, breathAlong;
        private bool breathBegun;
        private float untilBreathBurn;
        private float untilBeamRedraw;
        private Vector3 bitePoint, biteFromPosition;
        private Quaternion biteFromRotation;
        private bool biteFalling, biteLanded;
        private Vector3 clapPoint;
        private bool clapStruck;
        private float underHeadFor;
        private bool leftPlanted, rightPlanted, roared;
        private float roarRingFor = -1f;
        private float untilRimBolt;
        private bool furyShown;
        private bool leftBroken, rightBroken, headDown;
        private Vector3 leftFall, rightFall;
        private float untilDeathBolt;
        private int deathBolts;
        private float flashLeft;
        private bool flashing;
        // The colour of the element on the weapon whose blow set off the flash; clear for white.
        private Color flashTint;
        private float untilCrack;
        private float headYaw;
        private Vector3 leftRest, rightRest;
        private float untilJitter;
        private bool tethersShown;
        private readonly Vector3[] strandPoints = new Vector3[StrandPoints];
        private readonly Vector3[] beamPoints = new Vector3[BeamPoints];
        private Color[] glowBase;
        private float[] glowShown;
        [SerializeField] private StaggerMark staggerMark;

        /// <summary>Adds the mark that stands over the fallen head while it can be finished.</summary>
        public void ConfigureFeedback(StaggerMark markToShow) => staggerMark = markToShow;

        public void Configure(Transform targetToFight, Transform headPiece, Transform bodyPiece, Transform left, Transform right,
            ColossusPart headToStrike, ColossusPart leftToStrike, ColossusPart rightToStrike, Renderer[] allRenderers,
            MeshFilter slamRing, MeshFilter slamDisc, MeshFilter sweepRing, MeshFilter sweepLane, MeshFilter[] boltRings, MeshFilter[] boltDiscs, LineRenderer[] lightning)
        {
            target = targetToFight;
            head = headPiece;
            body = bodyPiece;
            leftHand = left;
            rightHand = right;
            headPart = headToStrike;
            leftPart = leftToStrike;
            rightPart = rightToStrike;
            renderers = allRenderers;
            slamOutline = slamRing;
            slamFill = slamDisc;
            sweepOutline = sweepRing;
            sweepFill = sweepLane;
            boltOutlines = boltRings;
            boltFills = boltDiscs;
            tethers = lightning;
        }

        /// <summary>
        /// The marks and the light of the head's attacks, the clap, the roar and the pools. The ropes of lightning are
        /// handed over in <see cref="Configure"/>, three strands for the left hand and then three for the right.
        /// </summary>
        public void ConfigureSetPiece(MeshFilter breathRing, MeshFilter breathLane, MeshFilter biteRing, MeshFilter biteDisc,
            MeshFilter[] clapRings, MeshFilter[] clapLanes, MeshFilter roar, MeshFilter[] pools, LineRenderer[] beamLines)
        {
            breathOutline = breathRing;
            breathFill = breathLane;
            biteOutline = biteRing;
            biteFill = biteDisc;
            clapOutlines = clapRings;
            clapFills = clapLanes;
            roarRing = roar;
            poolMarks = pools;
            beam = beamLines;
        }

        /// <summary>The parts of him that are made of light (the eyes, the scar): which renderer, which of its materials, and whether it is an eye.</summary>
        public void ConfigureGlow(Renderer[] glowing, int[] materialIndices, bool[] eyes)
        {
            glowRenderers = glowing;
            glowMaterials = materialIndices;
            glowIsEye = eyes;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            Health.Maximum = MaximumHealth;
            Health.Fill();
            Stagger.Maximum = BalanceMaximum;
            Stagger.StaggeredSeconds = StaggeredSeconds;
            Stagger.Reset();
            slamMesh = MarkerShapes.Sector(SlamRadius, 360f);
            // The sweep's lane crosses the whole floor and a third of the screen, so its inside is fainter even than a disc's.
            sweepMesh = MarkerShapes.Lane(FloorRadius * 2f, SweepWidth, MarkerShapes.InsideOfFan * 0.7f);
            boltMesh = MarkerShapes.Sector(BoltRadius, 360f);
            breathMesh = MarkerShapes.Lane(BreathLength, BreathWidth, MarkerShapes.InsideOfFan * 0.7f);
            biteMesh = MarkerShapes.Sector(BiteRadius, 360f);
            clapMesh = MarkerShapes.Lane(ClapSpread, ClapWidth, MarkerShapes.InsideOfFan);
            // The roar's ring is one metre across and grown to size, so its band widens as it goes, as a wave does.
            roarMesh = MarkerShapes.Sector(1f, 360f, 0.03f);
            // A pool has to be seen as ground that hurts for as long as it lies, so its inside is strong.
            poolMesh = MarkerShapes.Sector(PoolRadius, 360f, 0.5f);
            slamOutlineRenderer = Prepare(slamOutline, slamMesh);
            slamFillRenderer = Prepare(slamFill, slamMesh);
            sweepOutlineRenderer = Prepare(sweepOutline, sweepMesh);
            sweepFillRenderer = Prepare(sweepFill, sweepMesh);
            breathOutlineRenderer = Prepare(breathOutline, breathMesh);
            breathFillRenderer = Prepare(breathFill, breathMesh);
            biteOutlineRenderer = Prepare(biteOutline, biteMesh);
            biteFillRenderer = Prepare(biteFill, biteMesh);
            roarRenderer = Prepare(roarRing, roarMesh);
            boltOutlineRenderers = PrepareAll(boltOutlines, boltMesh);
            boltFillRenderers = PrepareAll(boltFills, boltMesh);
            clapOutlineRenderers = PrepareAll(clapOutlines, clapMesh);
            clapFillRenderers = PrepareAll(clapFills, clapMesh);
            poolRenderers = PrepareAll(poolMarks, poolMesh);
            boltPoints = new Vector3[boltOutlines.Length];
            boltLeft = new float[boltOutlines.Length];
            poolPoints = new Vector3[poolMarks.Length];
            poolLeft = new float[poolMarks.Length];
            glowBase = new Color[glowRenderers.Length];
            glowShown = new float[glowRenderers.Length];
            for (int i = 0; i < glowRenderers.Length; i++)
            {
                glowShown[i] = 1f;
                var materials = glowRenderers[i] != null ? glowRenderers[i].sharedMaterials : null;
                if (materials != null && glowMaterials[i] < materials.Length && materials[glowMaterials[i]] != null)
                    glowBase[i] = materials[glowMaterials[i]].GetColor(Emission);
            }
            foreach (var line in beam)
                if (line != null) line.enabled = false;
            leftRest = new Vector3(-7f, 4.2f, 5f);
            rightRest = new Vector3(7f, 4.2f, 5f);
            Hide();
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
            foreach (var mesh in new[] { slamMesh, sweepMesh, boltMesh, breathMesh, biteMesh, clapMesh, roarMesh, poolMesh })
                if (mesh != null) Destroy(mesh);
        }

        // It is far larger than the usual view: the camera stands back and looks north of the player, further the
        // further south the player is, so the head at the rim and the hands over the floor are seen together.
        private void FrameTheFight()
        {
            if (shaker == null || target == null) return;
            // His coming and his going happen at the rim, and nothing in them can hurt: the camera looks further north for them.
            float limit = mode == Mode.Rising || mode == Mode.Dying ? SceneLeadLimit : ViewLeadLimit;
            float north = Mathf.Clamp((HeadHome().z - target.position.z) * ViewLead, 0f, limit);
            shaker.SetFraming(ViewDistance, new Vector3(-target.position.x * 0.25f, 0f, north), ViewPitch);
        }

        private void OnDisable()
        {
            if (shaker != null) shaker.ClearFraming();
        }

        private static Renderer Prepare(MeshFilter filter, Mesh mesh)
        {
            if (filter == null) return null;
            filter.sharedMesh = mesh;
            var shown = filter.GetComponent<Renderer>();
            if (shown != null) shown.enabled = false;
            return shown;
        }

        private static Renderer[] PrepareAll(MeshFilter[] filters, Mesh mesh)
        {
            var shown = new Renderer[filters.Length];
            for (int i = 0; i < filters.Length; i++) shown[i] = Prepare(filters[i], mesh);
            return shown;
        }

        private void Hide()
        {
            SetShown(false);
            headPart.SetVulnerable(false);
            leftPart.SetVulnerable(false);
            rightPart.SetVulnerable(false);
        }

        private void SetShown(bool shown)
        {
            head.gameObject.SetActive(shown);
            body.gameObject.SetActive(shown);
            leftHand.gameObject.SetActive(shown);
            rightHand.gameObject.SetActive(shown);
            tethersShown = shown;
            if (shown) return;
            foreach (var tether in tethers)
                if (tether != null) tether.enabled = false;
        }

        /// <summary>Brings it up out of the cloud. The fight with it begins when its entrance is over.</summary>
        public override void Rise()
        {
            if (mode != Mode.Hidden) return;
            Health.Fill();
            Stagger.Reset();
            Afflictions.Clear();
            Chooser.Reset();
            leftPlanted = rightPlanted = roared = false;
            leftBroken = rightBroken = headDown = false;
            furyShown = false;
            underHeadFor = 0f;
            untilRimBolt = 0f;
            SetShown(true);
            head.localScale = Vector3.one * HeadScale;
            leftHand.localScale = new Vector3(-HandScale, HandScale, HandScale);
            rightHand.localScale = Vector3.one * HandScale;
            Enter(Mode.Rising);
            AdvanceRising(0f);
            if (shaker != null) shaker.Shake(0.3f, 0.6f);
        }

        /// <summary>A blow has landed on one of its pieces.</summary>
        public override HitOutcome TakePartHit(ColossusPart part, in Hit hit)
        {
            if (mode == Mode.Hidden || IsDead || mode == Mode.Rising) return HitOutcome.Missed;
            bool finisher = part.IsHead && IsStaggered && hit.Heavy;
            // The head stuck after a bite is only a head: it is the broken one that takes more, and can be finished.
            float share = !part.IsHead ? HandDamage : IsStaggered ? HeadDamage : BiteHeadDamage;
            Afflictions.IgniteFrom(hit, part);
            float takes = Afflictions.DamageTakenMultiplier(hit.Spell, hit.Heavy);
            float unsteadies = Afflictions.StaggerTakenFor(hit.Spell);
            Afflictions.TakeFrom(hit, part);
            float damage = finisher ? Mathf.Max(hit.Damage * 3f * takes, MaximumHealth * FinisherShare) : hit.Damage * share * takes;
            Health.Damage(damage);
            flashLeft = 0.09f;
            flashTint = hit.Tint;
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
            if (!IsStaggered && Stagger.Add(hit.Stagger * 0.7f * unsteadies)) BeginStagger();
            return HitOutcome.Landed;
        }

        /// <summary>Wears at its balance from outside a blow; enough of it breaks the balance at once.</summary>
        public override void Unbalance(float amount)
        {
            if (mode == Mode.Hidden || mode == Mode.Rising || IsDead || IsStaggered) return;
            if (Stagger.Add(amount)) BeginStagger();
        }

        private void BeginStagger()
        {
            TimesStaggered++;
            // Whatever he meant to do next is forgotten with his balance.
            secondSlamOwed = false;
            HideMarkers();
            leftPart.SetVulnerable(true);
            rightPart.SetVulnerable(true);
            headPart.SetVulnerable(true);
            Enter(Mode.Staggered);
            Vfx.Play(VfxKind.Ring, FallenHead() + Vector3.up * 2f, Vector3.up);
            ScreenFlash.Flash(new Color(1f, 0.95f, 0.8f, 0.35f), 0.08f);
            if (shaker != null) shaker.Shake(0.5f, 0.4f);
        }

        private void BeginDying()
        {
            HideMarkers();
            headPart.SetVulnerable(false);
            leftPart.SetVulnerable(false);
            rightPart.SetVulnerable(false);
            leftBroken = rightBroken = headDown = false;
            untilDeathBolt = 0f;
            deathBolts = 0;
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

        // Where things stand. North is +Z; the floor's middle is the origin.
        private Vector3 HeadHome() => new Vector3(0f, 1.2f, FloorRadius + 3.6f);
        private Vector3 FallenHead() => new Vector3(0f, 0.6f, FloorRadius - 9f);
        private Vector3 BodyHome() => new Vector3(0f, -3.2f, FloorRadius + 8.5f);
        private Vector3 Shoulder(bool left) => new Vector3(left ? -6f : 6f, 3.5f, FloorRadius + 6.5f);
        private Vector3 NorthRim() => new Vector3(0f, 0f, FloorRadius);
        // The breath's lane is long enough to cross the floor whichever way it is turned.
        private float BreathLength => FloorRadius * 2f + 2f;

        // How the pieces are turned, since the models keep their maker's axes and this code sets their turn outright.
        // The head, at Euler(pitch, 180, 0): pitch 0 lies it flat on its face with its crown to the south; pitch -90
        // stands it upright facing south; further back than that and it looks at the sky. A hand at Euler(0, 180, 0)
        // stands on its fingers with the wrist up and the back of the hand to the south.
        private void Update()
        {
            if (mode == Mode.Hidden || mode == Mode.Dead) return;
            FrameTheFight();
            float worldSeconds = Time.deltaTime * WorldClock.EnemyScale;
            // Ice or a hold stops its own clock for a moment: all its body does waits, and then goes on where it was.
            float deltaTime = OwnSeconds(worldSeconds);
            elapsed += deltaTime * (mode == Mode.Staggered || mode == Mode.Dying || mode == Mode.Rising ? 1f : Pace);
            WatchWhereThePlayerStands(deltaTime);
            // What burns or poisons it wears it down but never ends it: the last of a god is taken by a blow.
            if (Afflictions.Runs && mode != Mode.Rising && mode != Mode.Dying)
                Health.Damage(Mathf.Min(Afflictions.Tick(worldSeconds), Mathf.Max(0f, Health.Current - 1f)));
            switch (mode)
            {
                case Mode.Rising:
                    AdvanceRising(deltaTime);
                    break;
                case Mode.Idle:
                    Hover(deltaTime, true, true);
                    FaceTarget(deltaTime, 0f);
                    Stagger.Tick(deltaTime);
                    rest -= deltaTime * Pace;
                    if (rest <= 0f && MayBeginAttack) Choose();
                    break;
                case Mode.Slam:
                    AdvanceSlam(deltaTime);
                    break;
                case Mode.Sweep:
                    AdvanceSweep(deltaTime);
                    break;
                case Mode.Bolts:
                    Hover(deltaTime, true, true, 2.4f);
                    FaceTarget(deltaTime, -22f);
                    if (boltsToCall <= 0 && AllBoltsLanded()) EndAttack();
                    break;
                case Mode.Breath:
                    AdvanceBreath(deltaTime);
                    break;
                case Mode.Bite:
                    AdvanceBite(deltaTime);
                    break;
                case Mode.Clap:
                    AdvanceClap(deltaTime);
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
            // What it has already set loose (the called bolts, the pools) goes on in the world's time.
            AdvanceBolts(worldSeconds);
            AdvancePools(worldSeconds);
            AdvanceFury(deltaTime);
            AdvanceRoarRing();
            Paint();
            PaintGlow();
            DrawTethers();
            // Once the head has come down, not while it is still falling.
            if (staggerMark != null) staggerMark.Show(mode == Mode.Staggered && elapsed >= 0.5f);
        }

        // ------------------------------------------------------------------ choosing

        // The head's attacks answer where the player has chosen to stand, so that is watched all the time.
        private void WatchWhereThePlayerStands(float deltaTime)
        {
            if (target == null) return;
            underHeadFor = UnderTheHead() ? underHeadFor + deltaTime : 0f;
        }

        private bool UnderTheHead()
        {
            Vector3 from = target.position - NorthRim();
            from.y = 0f;
            return from.sqrMagnitude <= BiteReach * BiteReach;
        }

        private void Choose()
        {
            if (target == null) return;
            if (secondSlamOwed)
            {
                secondSlamOwed = false;
                BeginSlam(!handIsLeft);
                return;
            }
            ColossusAttack pick = Queued;
            Queued = ColossusAttack.None;
            if (pick == ColossusAttack.None)
                pick = Chooser.Next(Random.value, Random.value, underHeadFor >= BiteAfterSeconds, UnderTheHead(), target.position.z < FarSouthLine, Furious);
            switch (pick)
            {
                case ColossusAttack.Slam:
                    BeginSlam(target.position.x < 0f);
                    secondSlamOwed = Furious;
                    break;
                case ColossusAttack.Sweep: BeginSweep(); break;
                case ColossusAttack.Bolts: BeginBolts(); break;
                case ColossusAttack.Breath: BeginBreath(); break;
                case ColossusAttack.Bite: BeginBite(); break;
                case ColossusAttack.Clap: BeginClap(); break;
            }
        }

        // An attack is over: he rests, less in fury, and longer when the chooser says the player is owed a lull.
        private void EndAttack(float share = 1f)
        {
            rest = RestBetween * share * (Furious ? FuryRest : 1f);
            if (!secondSlamOwed && Chooser.TakeLull())
            {
                // The rest is counted at his pace, and the lull is promised in real seconds.
                rest = LullSeconds * Pace;
                Lulls++;
            }
            Enter(Mode.Idle);
        }

        private void Take(bool left)
        {
            handIsLeft = left;
            hand = left ? leftHand : rightHand;
            handPart = left ? leftPart : rightPart;
        }

        // ------------------------------------------------------------------ the entrance

        // The storm closes in, a hand comes over the rim and falls on the floor, then the other, he hauls himself up
        // on them, and when his head is over the rim he roars. Only then does the fight begin.
        private void AdvanceRising(float deltaTime)
        {
            if (shaker != null && Time.frameCount % 8 == 0) shaker.Shake(0.12f, 0.2f);
            untilRimBolt -= deltaTime;
            if (untilRimBolt <= 0f)
            {
                untilRimBolt = EntranceBoltEvery;
                RimBolt(0.7f);
            }
            bool handsFree = elapsed >= EntranceRoarAt + RoarSeconds * 0.5f;
            if (!handsFree)
            {
                PlantHand(true, EntranceLeftSlamAt, ref leftPlanted);
                PlantHand(false, EntranceRightSlamAt, ref rightPlanted);
            }
            else Hover(deltaTime, true, true);

            float rearAt = EntranceRoarAt - EntranceRear;
            float hauled = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(EntranceRightSlamAt, rearAt, elapsed));
            Vector3 under = Vector3.down * ((1f - hauled) * RiseDepth);
            if (!handsFree) body.position = BodyHome() + under;
            body.rotation = Quaternion.Euler(-8f, 180f, 0f);
            if (elapsed < rearAt)
            {
                // Climbing, he comes over the rim crown first, his face to the floor he climbs onto.
                head.position = HeadHome() + under;
                head.rotation = Quaternion.Euler(-12f, 180f, 0f);
            }
            else if (elapsed < EntranceRoarAt)
            {
                float reared = Mathf.SmoothStep(0f, 1f, (elapsed - rearAt) / EntranceRear);
                // Up off the rim until he stands over it and his face is seen for the first time.
                head.position = HeadHome() + new Vector3(0f, -0.8f, -1.6f) * reared;
                head.rotation = Quaternion.Euler(Mathf.Lerp(-12f, -92f, reared), 180f, 0f);
            }
            else if (elapsed < EntranceRoarAt + RoarSeconds)
            {
                if (!roared) Roar();
                float shudder = Mathf.Sin(FormTime * 61f) * 0.14f;
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, -1f, -3.2f), -58f, shudder * 20f, shudder * 25f, 18f);
            }
            else FaceTarget(deltaTime, 0f);

            if (elapsed < RiseSeconds) return;
            rest = RestBetween;
            Enter(Mode.Idle);
        }

        // One hand of the entrance: up from under the rim, held over the floor, and down on it. It hurts nobody, but
        // the floor is marked for it all the same, so that the first thing the fight teaches is what a mark means.
        private void PlantHand(bool left, float slamAt, ref bool planted)
        {
            Transform piece = left ? leftHand : rightHand;
            Vector3 plant = new Vector3(left ? -5.6f : 5.6f, 0f, FloorRadius - 4.8f);
            Vector3 below = plant + new Vector3(0f, -9f, 6.5f);
            Vector3 raised = plant + new Vector3(0f, 6.5f, 2.4f);
            float raiseAt = slamAt - EntranceHandRaise;
            float dropAt = slamAt - SlamDrop;
            if (elapsed < raiseAt)
            {
                piece.position = below;
                piece.rotation = Quaternion.Euler(-60f, 180f, 0f);
            }
            else if (elapsed < dropAt)
            {
                float t = Mathf.SmoothStep(0f, 1f, (elapsed - raiseAt) / (dropAt - raiseAt));
                piece.position = Vector3.Lerp(below, raised, t);
                piece.rotation = Quaternion.Euler(Mathf.Lerp(-60f, -34f, t), 180f, 0f);
                ShowMark(slamOutlineRenderer, slamFillRenderer, slamFill, plant, Quaternion.identity, (elapsed - raiseAt) / EntranceHandRaise, false);
            }
            else if (elapsed < slamAt)
            {
                float t = (elapsed - dropAt) / SlamDrop;
                piece.position = Vector3.Lerp(raised, plant, t * t);
                piece.rotation = Quaternion.Euler(Mathf.Lerp(-34f, 0f, t), 180f, 0f);
            }
            else
            {
                if (!planted)
                {
                    planted = true;
                    HideMark(slamOutlineRenderer, slamFillRenderer);
                    LandEffects(plant, SlamRadius);
                }
                // He leans on it as he climbs: the wrist comes up, the fingers stay.
                float lean = Mathf.Clamp01((elapsed - slamAt) / 0.8f);
                piece.position = plant;
                piece.rotation = Quaternion.Euler(9f * lean, 180f, 0f);
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
            Tint(roarRenderer, RoarColour, 1f - t * t);
        }

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

        // Where a bolt has landed in his fury the floor crackles for a while.
        private void LayPool(Vector3 point)
        {
            int oldest = 0;
            for (int i = 0; i < poolLeft.Length; i++)
            {
                if (poolLeft[i] <= 0f)
                {
                    oldest = i;
                    break;
                }
                if (poolLeft[i] < poolLeft[oldest]) oldest = i;
            }
            if (poolLeft.Length == 0) return;
            poolPoints[oldest] = point;
            poolLeft[oldest] = PoolSeconds;
            PoolsLaid++;
        }

        private void AdvancePools(float deltaTime)
        {
            untilPoolBurn -= deltaTime;
            bool gone = IsDead || mode == Mode.Staggered;
            for (int i = 0; i < poolLeft.Length; i++)
            {
                if (poolLeft[i] <= 0f) continue;
                poolLeft[i] = gone ? 0f : poolLeft[i] - deltaTime;
                var shown = poolRenderers[i];
                if (poolLeft[i] <= 0f)
                {
                    if (shown != null) shown.enabled = false;
                    continue;
                }
                if (shown != null)
                {
                    poolMarks[i].transform.SetPositionAndRotation(poolPoints[i] + Vector3.up * 0.06f, Quaternion.identity);
                    // It gutters as it dies: the last half second flickers out.
                    float strength = poolLeft[i] > 0.5f ? 1f : Mathf.Repeat(Time.time * 14f, 1f) < 0.5f ? 0.9f : 0.3f;
                    Tint(shown, RedMarker, MarkerShapes.OutlineAlpha * strength);
                }
                if ((Time.frameCount + i * 3) % 9 == 0)
                {
                    Vector2 spot = Random.insideUnitCircle * (PoolRadius * 0.8f);
                    Vfx.Play(VfxKind.Shock, poolPoints[i] + new Vector3(spot.x, 0.3f, spot.y), Vector3.up, 0.5f);
                }
                if (untilPoolBurn > 0f || target == null || victim == null) continue;
                Vector3 offset = target.position - poolPoints[i];
                offset.y = 0f;
                if (offset.magnitude > PoolRadius + 0.2f) continue;
                if (victimBody != null && victimBody.IsLeaping) continue;
                // One burn at a time, however many pools overlap under the player.
                untilPoolBurn = PoolTick;
                if (victim.TakeHit(new Hit(PoolDamage, 0f, offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.back, false, true, poolPoints[i])) == HitOutcome.Landed)
                    PoolBurns++;
            }
        }

        // ------------------------------------------------------------------ the slam

        private void BeginSlam(bool left)
        {
            Take(left);
            Slams++;
            slamLanded = false;
            slamPoint = OnFloor(target.position);
            Enter(Mode.Slam);
            Vfx.Play(VfxKind.Glint, hand.position + Vector3.up * 1.5f);
        }

        private void AdvanceSlam(float deltaTime)
        {
            Hover(deltaTime, !handIsLeft, handIsLeft);
            FaceTarget(deltaTime, 8f);
            float fall = SlamTelegraph + SlamDrop;
            if (elapsed < SlamTelegraph)
            {
                // It follows until just before it falls: moving late is what gets the player clear.
                if (elapsed < SlamTelegraph - SlamLock) slamPoint = OnFloor(target.position);
                float raised = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / (SlamTelegraph * 0.45f)));
                Vector3 over = slamPoint + Vector3.up * Mathf.Lerp(hand.position.y, SlamHover, raised);
                hand.position = Vector3.Lerp(hand.position, over, 1f - Mathf.Exp(-9f * deltaTime));
                Turn(hand, Quaternion.Euler(-28f * raised, 180f, 0f), deltaTime);
                ShowMark(slamOutlineRenderer, slamFillRenderer, slamFill, slamPoint, Quaternion.identity, elapsed / SlamTelegraph, false);
            }
            else if (elapsed < fall)
            {
                float t = (elapsed - SlamTelegraph) / SlamDrop;
                hand.position = new Vector3(slamPoint.x, Mathf.Lerp(SlamHover, 0f, t * t), slamPoint.z);
                hand.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if (!slamLanded)
            {
                slamLanded = true;
                hand.position = slamPoint;
                HideMark(slamOutlineRenderer, slamFillRenderer);
                Land(slamPoint, SlamRadius, SlamDamage, false);
                if (mode != Mode.Slam) return;
                handPart.SetVulnerable(true);
            }
            else if (elapsed >= fall + SlamRest)
            {
                handPart.SetVulnerable(false);
                EndAttack(secondSlamOwed ? 0.3f : 1f);
            }
            else
            {
                // Lying where it fell, it settles a little: this is the opening.
                float settle = Mathf.Clamp01((elapsed - fall) / 0.25f);
                hand.position = slamPoint + Vector3.up * (0.25f * Mathf.Sin(settle * Mathf.PI) * (1f - settle));
            }
        }

        // ------------------------------------------------------------------ the sweep

        private void BeginSweep()
        {
            // The hand on the side the player is nearer starts there and crosses the whole floor.
            Take(target.position.x < 0f);
            Sweeps++;
            sweepStruck = false;
            sweepZ = Mathf.Clamp(target.position.z, -FloorRadius + 4f, FloorRadius - 6f);
            float reach = Mathf.Sqrt(Mathf.Max(1f, FloorRadius * FloorRadius - sweepZ * sweepZ)) + 1.5f;
            sweepFrom = handIsLeft ? -reach : reach;
            sweepTo = -sweepFrom;
            lastSweepX = sweepFrom;
            Enter(Mode.Sweep);
            Vfx.Play(VfxKind.Danger, new Vector3(sweepFrom * 0.8f, 2.5f, sweepZ));
        }

        private void AdvanceSweep(float deltaTime)
        {
            Hover(deltaTime, !handIsLeft, handIsLeft);
            FaceTarget(deltaTime, 4f);
            Vector3 start = new Vector3(sweepFrom, 0.5f, sweepZ);
            // Edge on, like a blade: the back of the hand leads.
            var turned = Quaternion.Euler(0f, 180f, handIsLeft ? -70f : 70f);
            if (elapsed < SweepTelegraph)
            {
                hand.position = Vector3.Lerp(hand.position, start, 1f - Mathf.Exp(-7f * deltaTime));
                Turn(hand, turned, deltaTime);
                ShowMark(sweepOutlineRenderer, sweepFillRenderer, sweepFill, new Vector3(-FloorRadius, 0f, sweepZ), Quaternion.Euler(0f, 90f, 0f), elapsed / SweepTelegraph, true, true);
            }
            else if (elapsed < SweepTelegraph + SweepSeconds)
            {
                HideMark(sweepOutlineRenderer, sweepFillRenderer);
                float t = (elapsed - SweepTelegraph) / SweepSeconds;
                float x = Mathf.Lerp(sweepFrom, sweepTo, t);
                hand.position = new Vector3(x, 0.5f, sweepZ);
                hand.rotation = turned;
                if (Time.frameCount % 2 == 0) Vfx.Play(VfxKind.Dust, new Vector3(x, 0.1f, sweepZ), Vector3.up);
                // The player is caught if the hand passes them while they stand in its lane.
                if (!sweepStruck && target != null && victim != null)
                {
                    float low = Mathf.Min(lastSweepX, x) - 1.6f, high = Mathf.Max(lastSweepX, x) + 1.6f;
                    Vector3 at = target.position;
                    if (at.x >= low && at.x <= high && Mathf.Abs(at.z - sweepZ) <= SweepWidth * 0.5f + 0.4f)
                    {
                        sweepStruck = true;
                        BlowsStruck++;
                        victim.TakeHit(new Hit(SweepDamage, 0f, new Vector3(Mathf.Sign(sweepTo - sweepFrom), 0f, 0f), false, true, new Vector3(lastSweepX, 0f, sweepZ)));
                    }
                }
                lastSweepX = x;
            }
            else if (elapsed < SweepTelegraph + SweepSeconds + SweepRest)
            {
                // Spent, it lies open at the far edge.
                if (!handPart.Vulnerable)
                {
                    handPart.SetVulnerable(true);
                    if (shaker != null) shaker.Shake(0.25f, 0.2f);
                }
                Vector3 end = new Vector3(sweepTo * 0.82f, 0f, sweepZ);
                hand.position = Vector3.Lerp(hand.position, end, 1f - Mathf.Exp(-10f * deltaTime));
                Turn(hand, Quaternion.Euler(0f, 180f, 0f), deltaTime);
            }
            else
            {
                handPart.SetVulnerable(false);
                EndAttack();
            }
        }

        // ------------------------------------------------------------------ the bolts

        private void BeginBolts()
        {
            boltsToCall = BoltCount;
            untilBolt = 0.5f;
            Enter(Mode.Bolts);
            Vfx.Play(VfxKind.Danger, head.position + Vector3.up * 3f);
            if (shaker != null) shaker.Shake(0.2f, 0.3f);
        }

        private bool AllBoltsLanded()
        {
            foreach (float left in boltLeft)
                if (left > 0f) return false;
            return true;
        }

        private void AdvanceBolts(float deltaTime)
        {
            if (mode == Mode.Bolts && boltsToCall > 0 && target != null)
            {
                untilBolt -= deltaTime * Pace;
                if (untilBolt <= 0f)
                {
                    for (int i = 0; i < boltLeft.Length; i++)
                    {
                        if (boltLeft[i] > 0f) continue;
                        boltsToCall--;
                        BoltsCalled++;
                        untilBolt = BoltEvery;
                        boltLeft[i] = BoltWarning;
                        boltPoints[i] = OnFloor(target.position);
                        break;
                    }
                }
            }
            for (int i = 0; i < boltLeft.Length; i++)
            {
                if (boltLeft[i] <= 0f) continue;
                boltLeft[i] -= deltaTime;
                if (boltLeft[i] > 0f && !IsDead && mode != Mode.Staggered)
                {
                    ShowMark(boltOutlineRenderers[i], boltFillRenderers[i], boltFills[i], boltPoints[i], Quaternion.identity, 1f - boltLeft[i] / BoltWarning, true);
                    continue;
                }
                boltLeft[i] = 0f;
                HideMark(boltOutlineRenderers[i], boltFillRenderers[i]);
                if (IsDead || mode == Mode.Staggered) continue;
                LightningBolts.Strike(boltPoints[i], 0.5f);
                Strike(boltPoints[i], BoltRadius, BoltDamage, true);
                if (Furious && !IsDead && mode != Mode.Staggered) LayPool(boltPoints[i]);
            }
        }

        // ------------------------------------------------------------------ the breath

        private void BeginBreath()
        {
            Breaths++;
            breathBegun = false;
            untilBreathBurn = 0f;
            untilBeamRedraw = 0f;
            // From just inside the rim under his mouth, so the lane's near end is seen on the floor.
            breathFrom = NorthRim() + Vector3.back * 0.5f;
            AimBreath();
            Enter(Mode.Breath);
            Vfx.Play(VfxKind.Danger, head.position + Vector3.up * 3f);
        }

        private void AimBreath()
        {
            Vector3 to = OnFloor(target.position) - breathFrom;
            breathAlong = to.sqrMagnitude > 4f ? to.normalized : Vector3.back;
        }

        private void AdvanceBreath(float deltaTime)
        {
            Hover(deltaTime, true, true, 1.6f);
            float yaw = Mathf.Clamp(Mathf.Atan2(breathAlong.x, -breathAlong.z) * Mathf.Rad2Deg, -50f, 50f);
            var along = Quaternion.LookRotation(breathAlong, Vector3.up);
            if (elapsed < BreathTelegraph)
            {
                // The lane follows until just before the breath: stepping out late is what gets the player clear.
                if (elapsed < BreathTelegraph - BreathLock) AimBreath();
                // He rears up off the rim until his face is seen, and shakes more the nearer it is.
                float reared = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / (BreathTelegraph * 0.5f)));
                float near = elapsed / BreathTelegraph;
                float shudder = Mathf.Sin(FormTime * 53f) * 0.1f * near;
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, -1f * reared, -1.5f * reared), -28f - 56f * reared, yaw, shudder * 30f, 7f);
                ShowMark(breathOutlineRenderer, breathFillRenderer, breathFill, breathFrom, along, near, true, true);
                if (Time.frameCount % 5 == 0) Vfx.Play(VfxKind.Shock, Mouth(), Vector3.up, 0.6f + near);
            }
            else if (elapsed < BreathTelegraph + BreathSeconds)
            {
                if (!breathBegun)
                {
                    breathBegun = true;
                    Vfx.Play(VfxKind.Ring, breathFrom + Vector3.up * 0.3f, Vector3.up, 1.5f);
                    ScreenFlash.Flash(new Color(0.8f, 0.9f, 1f, 0.35f), 0.07f);
                    if (shaker != null) shaker.Shake(0.4f, BreathSeconds);
                }
                // Thrown forward and down over the rim, mouth to the floor, for as long as it lasts.
                float shudder = Mathf.Sin(FormTime * 67f) * 0.12f;
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, -0.4f, -2.6f), -38f, yaw, shudder * 30f, 16f);
                ShowMark(breathOutlineRenderer, breathFillRenderer, breathFill, breathFrom, along, 1f, true, true);
                DrawBeam();
                if (Time.frameCount % 2 == 0)
                {
                    Vector3 spot = breathFrom + breathAlong * Random.Range(1f, BreathLength - 4f)
                        + along * Vector3.right * Random.Range(-BreathWidth * 0.4f, BreathWidth * 0.4f);
                    Vfx.Play(VfxKind.Shock, spot + Vector3.up * 0.3f, Vector3.up, 0.8f);
                }
                untilBreathBurn -= deltaTime;
                if (untilBreathBurn > 0f || target == null || !InBreath(target.position)) return;
                untilBreathBurn = BreathTick;
                // Thrown out of the lane to whichever side they are nearer.
                Vector3 offset = OnFloor(target.position) - breathFrom;
                float side = Vector3.Dot(offset, along * Vector3.right) >= 0f ? 1f : -1f;
                Deal(new Hit(BreathDamage, 0f, along * Vector3.right * side, false, true, breathFrom), PerfectGuardBalance * 0.5f);
            }
            else if (elapsed < BreathTelegraph + BreathSeconds + BreathRest)
            {
                HideMark(breathOutlineRenderer, breathFillRenderer);
                HideBeam();
                // Spent, he hangs his head a moment.
                PoseHead(deltaTime, HeadHome() + new Vector3(0f, -0.6f, -0.8f), -14f, yaw, 0f, 4f);
            }
            else EndAttack();
        }

        private bool InBreath(Vector3 point)
        {
            Vector3 offset = OnFloor(point) - breathFrom;
            float along = Vector3.Dot(offset, breathAlong);
            if (along < -0.5f || along > BreathLength) return false;
            float across = (offset - breathAlong * along).magnitude;
            return across <= BreathWidth * 0.5f + 0.3f;
        }

        // The breath itself: a thick bolt from his mouth to the rim and down the lane, and thin ones wandering beside it.
        private void DrawBeam()
        {
            untilBeamRedraw -= Time.deltaTime;
            if (untilBeamRedraw > 0f) return;
            untilBeamRedraw = 0.04f;
            Vector3 mouth = Mouth();
            Vector3 across = Quaternion.LookRotation(breathAlong, Vector3.up) * Vector3.right;
            for (int strand = 0; strand < beam.Length; strand++)
            {
                var line = beam[strand];
                if (line == null) continue;
                bool core = strand == 0;
                float stray = core ? 0.25f : BreathWidth * 0.42f;
                beamPoints[0] = mouth;
                for (int i = 1; i < BeamPoints; i++)
                {
                    float t = (i - 1) / (float)(BeamPoints - 2);
                    beamPoints[i] = breathFrom + breathAlong * (t * (BreathLength - 1f)) + Vector3.up * 0.45f
                        + across * Random.Range(-stray, stray) + Vector3.up * Random.Range(0f, core ? 0.2f : 0.9f);
                }
                line.positionCount = BeamPoints;
                line.SetPositions(beamPoints);
                line.widthMultiplier = core ? BeamWidth * Random.Range(0.75f, 1.1f) : BeamWidth * 0.22f;
                line.enabled = core || Random.value > 0.2f;
            }
        }

        private void HideBeam()
        {
            foreach (var line in beam)
                if (line != null) line.enabled = false;
        }

        // Where the breath leaves him: in front of his teeth, wherever the head is turned.
        private Vector3 Mouth() => head.TransformPoint(new Vector3(0f, -2.1f, 1.4f));

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
            bitePoint = NorthRim() + to;
        }

        private void AdvanceBite(float deltaTime)
        {
            Hover(deltaTime, true, true, 2.6f);
            Vector3 toRim = OnFloor(HeadHome() - bitePoint).normalized;
            float yaw = Mathf.Atan2(-toRim.x, toRim.z) * Mathf.Rad2Deg;
            Vector3 middle = bitePoint + toRim * BiteLead;
            // Flat on his face with the circle under it, crown toward the player; the neck lies back toward the rim.
            Vector3 lie = middle + toRim * BiteBack + Vector3.up * 1.8f;
            var faceDown = Quaternion.Euler(6f, 180f - yaw, 0f);
            float fall = BiteTelegraph + BiteDrop;
            if (elapsed < BiteTelegraph)
            {
                // The circle follows until just before he falls on it.
                if (elapsed < BiteTelegraph - BiteLock) AimBite();
                // He comes in over the rim and stands his head up behind the circle, face to the player: the height he falls from.
                float reared = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / (BiteTelegraph * 0.55f)));
                Vector3 over = Vector3.Lerp(HeadHome(), middle + toRim * (BiteBack + 1.2f) + Vector3.up * 1.5f, reared);
                PoseHead(deltaTime, over, Mathf.Lerp(-28f, -72f, reared), yaw, 0f, 8f);
                ShowMark(biteOutlineRenderer, biteFillRenderer, biteFill, middle, Quaternion.identity, elapsed / BiteTelegraph, false);
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
                head.position = Vector3.Lerp(biteFromPosition, lie, t * t);
                head.rotation = Quaternion.Slerp(biteFromRotation, faceDown, t);
            }
            else if (!biteLanded)
            {
                biteLanded = true;
                head.SetPositionAndRotation(lie, faceDown);
                HideMark(biteOutlineRenderer, biteFillRenderer);
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
                float shudder = Mathf.Sin(FormTime * 41f) * 0.05f * (0.3f + pulling * pulling * 2f);
                head.SetPositionAndRotation(lie + new Vector3(shudder, 0f, shudder), faceDown * Quaternion.Euler(0f, 0f, shudder * 60f));
            }
            else
            {
                headPart.SetVulnerable(false);
                EndAttack();
            }
        }

        // ------------------------------------------------------------------ the clap

        private void BeginClap()
        {
            Claps++;
            clapStruck = false;
            AimClap();
            Enter(Mode.Clap);
            Vfx.Play(VfxKind.Glint, leftHand.position + Vector3.up * 1.5f, Vector3.up, 2f);
            Vfx.Play(VfxKind.Glint, rightHand.position + Vector3.up * 1.5f, Vector3.up, 2f);
        }

        private void AimClap()
        {
            clapPoint = OnFloor(target.position);
            clapPoint.z = Mathf.Clamp(clapPoint.z, -FloorRadius + 3f, FloorRadius - 5f);
        }

        private void AdvanceClap(float deltaTime)
        {
            // The hands are his own; Hover only keeps the body breathing here.
            Hover(deltaTime, false, false);
            FaceTarget(deltaTime, 10f);
            // Level, edge on, fingers to the south and the palms toward each other, a little off the floor.
            var leftTurn = Quaternion.LookRotation(Vector3.left, Vector3.forward);
            var rightTurn = Quaternion.LookRotation(Vector3.right, Vector3.forward);
            float close = ClapTelegraph + ClapSeconds;
            if (elapsed < ClapTelegraph)
            {
                // The line follows until just before they close.
                if (elapsed < ClapTelegraph - ClapLock) AimClap();
                // Wide apart, and in the last moment drawn back a little further still, as before any blow.
                float wound = Mathf.Clamp01((elapsed - (ClapTelegraph - ClapLock)) / ClapLock);
                float apart = ClapSpread + 1.4f * wound;
                float ease = 1f - Mathf.Exp(-7f * deltaTime);
                leftHand.position = Vector3.Lerp(leftHand.position, clapPoint + new Vector3(-apart, 1.4f, 0f), ease);
                rightHand.position = Vector3.Lerp(rightHand.position, clapPoint + new Vector3(apart, 1.4f, 0f), ease);
                Turn(leftHand, leftTurn, deltaTime);
                Turn(rightHand, rightTurn, deltaTime);
                float shown = elapsed / ClapTelegraph;
                if (clapOutlineRenderers.Length >= 2)
                {
                    // Two marks, one from each hand, that grow toward each other and meet on the player.
                    ShowMark(clapOutlineRenderers[0], clapFillRenderers[0], clapFills[0], clapPoint + Vector3.left * ClapSpread, Quaternion.Euler(0f, 90f, 0f), shown, false, true);
                    ShowMark(clapOutlineRenderers[1], clapFillRenderers[1], clapFills[1], clapPoint + Vector3.right * ClapSpread, Quaternion.Euler(0f, -90f, 0f), shown, false, true);
                }
            }
            else if (elapsed < close)
            {
                float t = (elapsed - ClapTelegraph) / ClapSeconds;
                float apart = Mathf.Lerp(ClapSpread + 1.4f, 1.1f, t * t);
                leftHand.SetPositionAndRotation(clapPoint + new Vector3(-apart, 1.4f, 0f), leftTurn);
                rightHand.SetPositionAndRotation(clapPoint + new Vector3(apart, 1.4f, 0f), rightTurn);
                if (Time.frameCount % 2 == 0)
                {
                    Vfx.Play(VfxKind.Dust, clapPoint + Vector3.left * apart, Vector3.up);
                    Vfx.Play(VfxKind.Dust, clapPoint + Vector3.right * apart, Vector3.up);
                }
            }
            else if (!clapStruck)
            {
                clapStruck = true;
                HideClapMarks();
                LandEffects(clapPoint, 2.5f);
                Vfx.Play(VfxKind.Ring, clapPoint + Vector3.up * 1.5f, Vector3.right, 1.5f);
                if (shaker != null) shaker.Shake(0.8f, 0.4f);
                if (target != null)
                {
                    Vector3 offset = target.position - clapPoint;
                    // It comes from both sides at once, so the guard holds whichever way the player faces.
                    if (Mathf.Abs(offset.z) <= ClapWidth * 0.5f + 0.35f && Mathf.Abs(offset.x) <= ClapSpread + 0.5f)
                        Deal(new Hit(ClapDamage, 0f, Vector3.zero), PerfectGuardBalance * ClapPerfectGuards);
                }
                if (mode != Mode.Clap) return;
                leftPart.SetVulnerable(true);
                rightPart.SetVulnerable(true);
            }
            else if (elapsed < close + ClapRest)
            {
                // Stunned, they fall open side by side where they met.
                float ease = 1f - Mathf.Exp(-12f * deltaTime);
                leftHand.position = Vector3.Lerp(leftHand.position, clapPoint + new Vector3(-2.3f, 0f, 0.4f), ease);
                rightHand.position = Vector3.Lerp(rightHand.position, clapPoint + new Vector3(2.3f, 0f, 0.4f), ease);
                Turn(leftHand, Quaternion.Euler(0f, 166f, 0f), deltaTime * 2f);
                Turn(rightHand, Quaternion.Euler(0f, 194f, 0f), deltaTime * 2f);
            }
            else
            {
                leftPart.SetVulnerable(false);
                rightPart.SetVulnerable(false);
                EndAttack();
            }
        }

        private void HideClapMarks()
        {
            for (int i = 0; i < clapOutlineRenderers.Length; i++) HideMark(clapOutlineRenderers[i], clapFillRenderers[i]);
        }

        // ------------------------------------------------------------------ broken, and falling

        private void AdvanceStagger(float deltaTime)
        {
            // The head comes down on the floor and the hands fall open beside it.
            float fallen = Mathf.Clamp01(elapsed / 0.5f);
            float eased = fallen * fallen;
            head.position = Vector3.Lerp(head.position, FallenHead(), eased);
            // On its back, face to the sky and chin toward the player: from above, the face is what is seen.
            Turn(head, Quaternion.Euler(-64f, 180f, 7f), deltaTime * 2f);
            leftHand.position = Vector3.Lerp(leftHand.position, new Vector3(-6.2f, 0f, FloorRadius - 9.5f), eased);
            rightHand.position = Vector3.Lerp(rightHand.position, new Vector3(6.2f, 0f, FloorRadius - 9.5f), eased);
            Turn(leftHand, Quaternion.Euler(0f, 160f, 0f), deltaTime * 2f);
            Turn(rightHand, Quaternion.Euler(0f, 200f, 0f), deltaTime * 2f);
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
            headPart.SetVulnerable(false);
            leftPart.SetVulnerable(false);
            rightPart.SetVulnerable(false);
            float t = Mathf.Clamp01(elapsed / 0.9f);
            head.position = Vector3.Lerp(head.position, HeadHome(), t * t);
            Hover(deltaTime, true, true);
            FaceTarget(deltaTime, -12f);
            if (elapsed >= 0.9f)
            {
                rest = RestBetween * 0.6f;
                Enter(Mode.Idle);
            }
        }

        // The head rears, light breaks out all over him, the sky strikes him, the ropes part and the hands fall away
        // one after the other, and the head comes down on the rim, lies there, and slides off into the cloud.
        private void AdvanceDying(float deltaTime)
        {
            float shudder = Mathf.Sin(FormTime * 47f) * Mathf.Lerp(0.06f, 0.2f, Mathf.Clamp01(elapsed / DeathHeadFallsAt));
            if (shaker != null && Time.frameCount % 6 == 0 && elapsed < DeathHeadFallsAt) shaker.Shake(0.22f, 0.2f);

            // The sky strikes him: the head, then a hand, then the other, and round again.
            untilDeathBolt -= deltaTime;
            if (elapsed >= DeathBoltsAt && elapsed < DeathHeadLandsAt && untilDeathBolt <= 0f)
            {
                untilDeathBolt = DeathBoltEvery;
                int which = deathBolts++ % 3;
                Transform struck = which == 0 ? head : which == 1 ? (leftBroken ? body : leftHand) : (rightBroken ? body : rightHand);
                Vector3 at = struck == body ? Shoulder(which == 1) : struck.position + Vector3.up * 2f;
                LightningBolts.Strike(at, 0.8f);
                Vfx.Play(VfxKind.Burst, at);
            }

            DyingHand(true, DeathLeftBreaksAt, ref leftBroken, ref leftFall, deltaTime);
            DyingHand(false, DeathRightBreaksAt, ref rightBroken, ref rightFall, deltaTime);

            if (elapsed < DeathHeadFallsAt)
            {
                // Stood up over the rim and thrown back until it looks at the sky, shaking harder all the while.
                PoseHead(deltaTime, HeadHome() + new Vector3(shudder, -1.2f, -2.4f), -104f, shudder * 40f, shudder * 50f, 5f);
                body.position = Vector3.Lerp(body.position, BodyHome() + new Vector3(-shudder, 0f, 0f), 1f - Mathf.Exp(-6f * deltaTime));
                return;
            }

            // The body goes down behind the rim as the head comes forward over it.
            float sunk = Mathf.Clamp01((elapsed - DeathHeadFallsAt) / (DeathSeconds - DeathHeadFallsAt));
            body.position = BodyHome() + Vector3.down * (sunk * sunk * RiseDepth);
            // Flat on its face across the rim's edge, crown toward the floor's middle.
            Vector3 onRim = new Vector3(0f, 1.8f, FloorRadius + 0.8f);
            var faceDown = Quaternion.Euler(7f, 180f, 5f);
            if (elapsed < DeathHeadLandsAt)
            {
                float t = (elapsed - DeathHeadFallsAt) / (DeathHeadLandsAt - DeathHeadFallsAt);
                head.position = Vector3.Lerp(head.position, onRim, t * t);
                head.rotation = Quaternion.Slerp(head.rotation, faceDown, t * t);
            }
            else if (elapsed < DeathHeadSlidesAt)
            {
                if (!headDown)
                {
                    headDown = true;
                    LandEffects(NorthRim() + Vector3.back * 2.5f, 4f);
                    if (shaker != null) shaker.Shake(1f, 0.5f);
                    ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.45f), 0.08f);
                }
                head.SetPositionAndRotation(onRim, faceDown);
            }
            else if (elapsed < DeathSeconds)
            {
                // Its own weight takes it: back off the edge, slowly and then all at once.
                float t = (elapsed - DeathHeadSlidesAt) / (DeathSeconds - DeathHeadSlidesAt);
                head.position = onRim + new Vector3(0f, -RiseDepth * t * t * t, 7f * t * t);
                head.rotation = faceDown * Quaternion.Euler(-60f * t * t, 0f, 0f);
                if (Time.frameCount % 4 == 0 && t < 0.5f) Vfx.Play(VfxKind.Dust, NorthRim() + new Vector3(Random.Range(-2f, 2f), 0.1f, -0.5f), Vector3.up);
            }
            else
            {
                mode = Mode.Dead;
                if (shaker != null) shaker.ClearFraming();
                foreach (var shown in renderers)
                    if (shown != null) shown.SetPropertyBlock(null);
                SetShown(false);
                RaiseDied();
            }
        }

        // One hand of the death: drawn back to him while the rope holds, and when it parts, falling away into the cloud.
        private void DyingHand(bool left, float breaksAt, ref bool broken, ref Vector3 fall, float deltaTime)
        {
            Transform piece = left ? leftHand : rightHand;
            float side = left ? -1f : 1f;
            if (elapsed < breaksAt)
            {
                // Just outside the rim where it can still be seen, so that when it falls it falls past the floor and not through it.
                Vector3 held = new Vector3(side * 8.5f, 3.5f, FloorRadius - 1.5f);
                piece.position = Vector3.Lerp(piece.position, held, 1f - Mathf.Exp(-4.5f * deltaTime));
                Turn(piece, Quaternion.Euler(-50f, 180f, side * 20f), deltaTime);
                return;
            }
            if (!broken)
            {
                broken = true;
                fall = new Vector3(side * 4f, 3f, 1.5f);
                LightningBolts.Strike(piece.position, 1f);
                Vfx.Play(VfxKind.Burst, piece.position, Vector3.up, 2f);
                Vfx.Play(VfxKind.Impact, Shoulder(left));
                ScreenFlash.Flash(new Color(0.85f, 0.92f, 1f, 0.3f), 0.06f);
                if (shaker != null) shaker.Shake(0.5f, 0.25f);
            }
            fall += Vector3.down * (26f * deltaTime);
            piece.position += fall * deltaTime;
            piece.Rotate(new Vector3(140f, 30f, side * 90f) * deltaTime, Space.Self);
        }

        // ------------------------------------------------------------------ moving the pieces

        // Hands not in use hang in the air on either side of the player, a little north, rising and falling.
        private void Hover(float deltaTime, bool left, bool right, float higher = 0f)
        {
            Vector3 about = target != null ? target.position : Vector3.zero;
            about.y = 0f;
            about.z = Mathf.Min(about.z + 4.5f, FloorRadius - 4f);
            float bob = Mathf.Sin(FormTime * 1.7f) * 0.35f;
            if (left)
            {
                Vector3 want = about + new Vector3(leftRest.x, leftRest.y + bob + higher, 0f);
                leftHand.position = Vector3.Lerp(leftHand.position, want, 1f - Mathf.Exp(-2.4f * deltaTime));
                Turn(leftHand, Quaternion.Euler(-18f, 168f, 8f), deltaTime);
            }
            if (right)
            {
                Vector3 want = about + new Vector3(rightRest.x, rightRest.y - bob + higher, 0f);
                rightHand.position = Vector3.Lerp(rightHand.position, want, 1f - Mathf.Exp(-2.4f * deltaTime));
                Turn(rightHand, Quaternion.Euler(-18f, 192f, -8f), deltaTime);
            }
            body.position = Vector3.Lerp(body.position, BodyHome() + Vector3.up * (bob * 0.3f), 1f - Mathf.Exp(-3f * deltaTime));
        }

        // The head stays at the north edge, face tipped up to be seen, and turns to keep its eyes on the player.
        private void FaceTarget(float deltaTime, float nod)
        {
            float want = 0f;
            if (target != null)
            {
                Vector3 to = target.position - HeadHome();
                want = Mathf.Clamp(Mathf.Atan2(to.x, -to.z) * Mathf.Rad2Deg, -38f, 38f);
            }
            headYaw = Mathf.Lerp(headYaw, want, 1f - Mathf.Exp(-3f * deltaTime));
            float breath = Mathf.Sin(FormTime * 1.3f);
            head.position = Vector3.Lerp(head.position, HeadHome() + Vector3.up * (breath * 0.25f), 1f - Mathf.Exp(-4f * deltaTime));
            Turn(head, Quaternion.Euler(-28f + nod + breath * 2f, 180f - headYaw, 0f), deltaTime);
        }

        // The head taken somewhere for an attack of its own: where, tipped how far down (as FaceTarget counts it), and turned how far.
        private void PoseHead(float deltaTime, Vector3 at, float pitch, float yaw, float roll, float quickness)
        {
            headYaw = yaw;
            head.position = Vector3.Lerp(head.position, at, 1f - Mathf.Exp(-quickness * deltaTime));
            head.rotation = Quaternion.Slerp(head.rotation, Quaternion.Euler(pitch, 180f - yaw, roll), 1f - Mathf.Exp(-quickness * 1.5f * deltaTime));
        }

        private static void Turn(Transform piece, Quaternion toward, float deltaTime) =>
            piece.rotation = Quaternion.Slerp(piece.rotation, toward, 1f - Mathf.Exp(-8f * deltaTime));

        private static Vector3 OnFloor(Vector3 point) => new Vector3(point.x, 0f, point.z);

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

        // A hand has come down: the ground shows it, and the player is struck if under it.
        private void Land(Vector3 point, float radius, float damage, bool unblockable)
        {
            LandEffects(point, radius);
            Strike(point, radius, damage, unblockable);
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
            // A god lays its land's affliction on the player its blow lands on (the stand-in plan's question 1).
            if (outcome == HitOutcome.Landed && victim is Clover.Player.PlayerDefence struckPlayer) struckPlayer.StruckInLand(EnemyRank.God, gameObject.scene.name);
            if (outcome != HitOutcome.PerfectGuard || IsStaggered || IsDead) return;
            flashLeft = 0.09f;
            flashTint = default;
            if (Stagger.Add(balanceIfTurned)) BeginStagger();
        }

        private void ShowMark(Renderer outline, Renderer fill, MeshFilter fillFilter, Vector3 at, Quaternion facing, float progress, bool red, bool lane = false)
        {
            if (outline == null || fill == null) return;
            outline.transform.SetPositionAndRotation(at + Vector3.up * 0.05f, facing);
            fill.transform.SetPositionAndRotation(at + Vector3.up * 0.07f, facing);
            float shown = Mathf.Clamp01(progress);
            // A circle fills from its middle; a lane fills along its length.
            fillFilter.transform.localScale = lane ? new Vector3(1f, 1f, shown) : new Vector3(shown, 1f, shown);
            Color colour = red ? RedMarker : PlainMarker;
            Tint(outline, colour, MarkerShapes.OutlineAlpha);
            Tint(fill, MarkerShapes.Growing(colour), MarkerShapes.GrowingAlpha);
        }

        private void Tint(Renderer shown, Color colour, float alpha)
        {
            shown.enabled = true;
            colour.a = alpha;
            block.Clear();
            block.SetColor(MarkerColour, colour);
            shown.SetPropertyBlock(block);
        }

        private static void HideMark(Renderer outline, Renderer fill)
        {
            if (outline != null) outline.enabled = false;
            if (fill != null) fill.enabled = false;
        }

        private void HideMarkers()
        {
            HideMark(slamOutlineRenderer, slamFillRenderer);
            HideMark(sweepOutlineRenderer, sweepFillRenderer);
            HideMark(breathOutlineRenderer, breathFillRenderer);
            HideMark(biteOutlineRenderer, biteFillRenderer);
            HideClapMarks();
            HideBeam();
            for (int i = 0; i < boltLeft.Length; i++)
            {
                boltLeft[i] = 0f;
                HideMark(boltOutlineRenderers[i], boltFillRenderers[i]);
            }
            boltsToCall = 0;
        }

        // White for an instant when struck, a gold pulse for as long as it lies broken, pale blue while ice has it,
        // and in its death a cold light that breaks out on one piece and then another, more of them the nearer the end.
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
            bool iced = Iced;
            bool lit = flash || pulse || iced;
            if (!lit && !flashing) return;
            flashing = lit;
            foreach (var shown in renderers)
            {
                if (shown == null) continue;
                if (!lit)
                {
                    shown.SetPropertyBlock(null);
                    continue;
                }
                block.Clear();
                block.SetColor(Emission, flash ? StruckLook.Flash(new Color(1.5f, 1.5f, 1.5f), flashTint) : iced ? FrozenGlow : new Color(0.8f, 0.55f, 0.12f));
                shown.SetPropertyBlock(block);
            }
        }

        // The eyes and the scar are made of light, and say what he is about: dark until he roars, flaring as a breath
        // gathers, burning steadily brighter in his fury, guttering as he dies.
        private void PaintGlow()
        {
            float eyes = 1f, scar = 1f;
            if (Furious && mode != Mode.Dying)
            {
                float pulse = 0.85f + 0.3f * Mathf.Sin(FormTime * 7f);
                eyes = FuryEyeGlow * pulse;
                scar = FuryScarGlow * pulse;
            }
            if (mode == Mode.Rising)
            {
                float since = elapsed - EntranceRoarAt;
                eyes = since < 0f ? 0.15f : Mathf.Lerp(BreathEyeGlow, 1f, Mathf.Clamp01(since / (RoarSeconds + 0.3f)));
                scar = since < 0f ? 0.4f : Mathf.Min(eyes, 2.2f);
            }
            else if (mode == Mode.Breath && elapsed < BreathTelegraph + BreathSeconds)
            {
                eyes = Mathf.Max(eyes, Mathf.Lerp(1f, BreathEyeGlow, Mathf.Clamp01(elapsed / BreathTelegraph)));
            }
            else if (mode == Mode.Dying)
            {
                // Brighter than it ever was, unsteady, and out when the head is down.
                float flicker = Mathf.Repeat(FormTime * 17f, 1f) < 0.5f ? 1f : 0.35f;
                eyes = elapsed < DeathHeadLandsAt ? DeathEyeGlow * flicker : 0.05f;
                scar = elapsed < DeathHeadLandsAt ? FuryScarGlow * flicker : 0.05f;
            }
            for (int i = 0; i < glowRenderers.Length; i++)
            {
                float want = glowIsEye[i] ? eyes : scar;
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

        private bool HandAttacking(bool left)
        {
            if (mode == Mode.Clap || mode == Mode.Rising) return true;
            return (mode == Mode.Slam || mode == Mode.Sweep) && handIsLeft == left;
        }

        // What holds each hand to him: thin bolts from the shoulder to the wrist, kinked, never the same twice, and
        // never all there at once. A hand that is attacking has more of them, and brighter.
        private void DrawTethers()
        {
            untilJitter -= Time.unscaledDeltaTime;
            if (untilJitter > 0f) return;
            untilJitter = TetherRedraw;
            for (int side = 0; side < 2; side++)
            {
                bool left = side == 0;
                bool held = tethersShown && !(left ? leftBroken : rightBroken);
                bool attacking = HandAttacking(left);
                Transform piece = left ? leftHand : rightHand;
                Vector3 from = body.position + (Shoulder(left) - BodyHome());
                // The cuff at the wrist, which in the hand's own axes is its +Y end.
                Vector3 to = piece.TransformPoint(new Vector3(0f, 3.3f, 1f));
                float length = (to - from).magnitude;
                for (int strand = 0; strand < StrandsEach; strand++)
                {
                    int index = side * StrandsEach + strand;
                    if (index >= tethers.Length || tethers[index] == null) continue;
                    var line = tethers[index];
                    // The first strand is the rope and is nearly always there; the others come and go, the third only for a blow.
                    bool shown = held && (strand == 0 ? Random.value > 0.06f : strand == 1 ? Random.value > 0.3f : attacking && Random.value > 0.35f);
                    line.enabled = shown;
                    if (!shown) continue;
                    float kink = TetherKink * (strand == 0 ? 1f : 1.9f) * Mathf.Clamp(length / 12f, 0.5f, 1.4f);
                    for (int i = 0; i < StrandPoints; i++)
                    {
                        float t = i / (float)(StrandPoints - 1);
                        Vector3 point = Vector3.Lerp(from, to, t);
                        // Hard corners, not a curve: each point is thrown its own way, and the ends are held.
                        if (i > 0 && i < StrandPoints - 1) point += Random.onUnitSphere * (kink * Random.Range(0.35f, 1f));
                        strandPoints[i] = point;
                    }
                    line.positionCount = StrandPoints;
                    line.SetPositions(strandPoints);
                    line.widthMultiplier = TetherWidth * (strand == 0 ? 1f : 0.6f) * (attacking ? TetherAttackGain : 1f) * Random.Range(0.8f, 1.2f);
                }
            }
        }
    }
}
