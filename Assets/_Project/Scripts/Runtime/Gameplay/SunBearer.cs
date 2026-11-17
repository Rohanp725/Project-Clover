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
    public class SunBearer : GodlyForm
    {
        private enum Mode { Hidden, Entering, Idle, Roll, Ram, Sparks, Change, Stoop, Pass, Feathers, Zenith, Spent, Exposed, Staggered, Dying, Dead }
        private enum SunState { Held, Left, High, Loose }

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
        public float FuryAt = 0.5f;
        public float FuryPace = 1.25f;
        public float FuryRest = 0.55f;
        public float RestBetween = 0.9f;
        public float LullSeconds = 2.4f;
        public float EntranceSeconds = 4.2f;
        public float RoarAt = 3.4f;
        public float DeathSeconds = 5f;
        public float FlightHeight = 12f;
        public float FlightRadius = 15f;
        public float WalkSpeed = 4f;

        public float RollWidth = 4f, RollSpeed = 12f, RollDamage = 28f, FurrowDamage = 24f, RollFollow = 0.9f, RollHold = 0.4f, FurrowFollow = 0.6f, FurrowHold = 0.3f;
        public float HotSeconds = 10f, FurrowHotSeconds = 14f, HotDamage = 6f, HotEvery = 0.5f, HotAfter = 0.3f;
        public float RamReach = 9f, RamAfterSeconds = 1.2f, RamRadius = 1.7f, RamFollow = 0.8f, RamHold = 0.3f, RamDamage = 34f, RamStuck = 1.8f;
        public float SparkRadius = 1.1f, SparkDamage = 16f, SparkWarning = 1f, SparkOut = 7f;
        public float FlareRadius = 6f, FlareDamage = 20f;
        public float StoopRadius = 2.5f, StoopFollow = 1f, StoopHold = 0.5f, StoopDamage = 38f, StoopStands = 1.6f, StoopHotRadius = 2f;
        public float PassWidth = 5f, PassShown = 1.2f, PassSeconds = 1f, PassDamage = 26f, PassHeight = 3.6f;
        public float FeatherRadius = 0.9f, FeatherDamage = 14f, FeatherWarning = 1f;
        public float ZenithFill = 3f, ZenithBlaze = 6f, ZenithFuryBlaze = 8f, BlazeDamage = 12f, BlazeEvery = 0.5f, SpentSeconds = 2.5f;
        public float SunTouchDamage = 12f;
        public float ExposedSeconds = 4f, ExposedMost = 9f, ExposedMostInFury = 7f;
        public float ViewDistance = 2f, ViewPitch = -8f, ViewLead = 0.3f, ViewLeadLimit = 4.5f;
        public Color PlainMarker = new Color(1f, 0.93f, 0.75f, 1f);
        public Color RedMarker = new Color(1f, 0.08f, 0.05f, 1f);
        public Color HotMarker = new Color(1f, 0.45f, 0.06f, 1f);

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
        public override bool InFight => mode != Mode.Hidden && mode != Mode.Entering && mode != Mode.Dead && mode != Mode.Dying;
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
        public int SunPushes { get; private set; }
        public int TimesStaggered { get; private set; }
        public int BlowsStruck { get; private set; }
        public int HotBites { get; private set; }
        public int BlazeBites { get; private set; }
        public int BlazeSpared { get; private set; }
        public int Lulls { get; private set; }
        public int FallsLaid { get; private set; }
        public bool PlayerHidden { get; private set; }
        /// <summary>What the last blow it gave the player was: for probes.</summary>
        public string LastBlow { get; private set; } = "";
        /// <summary>The middle of the mark of whatever it is about to do: for probes.</summary>
        public Vector3 MarkAt { get; private set; }
        /// <summary>The red lane as it is drawn: where it starts, which way it runs and how long: for probes.</summary>
        public Vector3 LaneFrom => laneFrom;
        public Vector3 LaneAlong => laneAlong;
        public float LaneLength => laneLength;

        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private static readonly int MarkerColour = Shader.PropertyToID("_Color");
        // Read every frame by the bar and the probes: the names are made once.
        private static readonly string[] ModeNames = System.Enum.GetNames(typeof(Mode));
        private const int HotMost = 6;
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
        private SunState sunState;

        // Falling things: sparks and feathers.
        private Vector3[] fallPoints;
        private float[] fallLeft, fallWarning, fallRadius, fallDamage;

        // Floor left hot.
        private readonly bool[] hotIsLane = new bool[HotMost];
        private readonly Vector3[] hotFrom = new Vector3[HotMost], hotAlong = new Vector3[HotMost];
        private readonly float[] hotLength = new float[HotMost], hotWide = new float[HotMost], hotLeft = new float[HotMost];
        private float onHotFor, untilHotBite, untilFlame;

        private Vector3 laneFrom, laneAlong, markPoint, moveFrom, landAt;
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
            shadowRenderer = falconShadow != null ? falconShadow.GetComponent<Renderer>() : null;
            lineRenderer = lightLine != null ? lightLine.GetComponent<Renderer>() : null;
            legRest = new Quaternion[legs.Length];
            for (int i = 0; i < legs.Length; i++) legRest[i] = legs[i].localRotation;
            // The wings' bones are turned about the body's own forward line, wherever the file left their axes.
            body.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            RestOf(wingLeft, out wingLeftRest, out wingLeftAxis);
            RestOf(wingRight, out wingRightRest, out wingRightAxis);
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
            HideMarkers();
            ClearHot();
            Blazing = false;
            if (shaker != null) shaker.ClearFraming();
            if (staggerMark != null) staggerMark.Show(false);
            if (roarRenderer != null) roarRenderer.enabled = false;
            Hide();
            Enter(Mode.Hidden);
        }

        public override void Rise()
        {
            if (mode != Mode.Hidden) return;
            Health.Fill();
            Stagger.Reset();
            Afflictions.Clear();
            Chooser.Reset();
            Queued = SunBearerAttack.None;
            falcon = false;
            furyShown = zenithOwed = saidLoose = saidBlaze = roared = false;
            sinceZenith = 100f;
            nearFor = 0f;
            ClearHot();
            Vector3 from = Keep(OnFloor(RiseFrom), FloorRadius - 6f);
            // It gathers itself on the far side of the sun from whoever is watching.
            Vector3 away = target != null ? OnFloor(from - target.position) : Vector3.forward;
            away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.forward;
            at = Keep(from + away * 4.4f, FloorRadius - 3f);
            yaw = YawOf(-away);
            pitch = roll = 0f;
            lift = 0f;
            size = 0.01f;
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
                // The loose sun is only a ball: a blow rolls it away from whoever struck it.
                if (sunState != SunState.Loose) return HitOutcome.Missed;
                Vector3 push = OnFloor(hit.Direction);
                if (push.sqrMagnitude < 0.01f && target != null) push = OnFloor(sunAt - target.position);
                sunRollTo = KeepSun(OnFloor(sunAt) + push.normalized * (hit.Heavy ? 3f : 1.5f));
                sunRollLeft = 0.3f;
                SunPushes++;
                return HitOutcome.Landed;
            }
            bool isHead = part == headPart;
            bool finisher = isHead && IsStaggered && hit.Heavy;
            bool open = mode == Mode.Exposed || IsStaggered || (isHead && headOpen);
            float share = IsStaggered ? (isHead ? HeadDamage : 1f) : open ? 1f : HotShellShare;
            Afflictions.Burn.Ignite(hit.BurnSeconds, hit.BurnStacks);
            float takes = Afflictions.DamageTakenMultiplier(hit.Spell);
            float unsteadies = Afflictions.StaggerTakenMultiplier;
            if (hit.Affliction != AfflictionKind.None && hit.AfflictionStacks > 0)
                CombatEvents.Report(part, hit.Affliction, hit.AfflictionStacks, Afflictions.Apply(hit.Affliction, hit.AfflictionStacks, hit.AfflictionSeconds, hit.AfflictionPower));
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
            if (!open && sunState != SunState.Loose && sunState != SunState.High)
            {
                // The shell is hot and little gets through, but all of it counts toward breaking its hold on the sun.
                knockCount += hit.Damage * takes;
                if (knockCount >= KnockDamage)
                {
                    Vector3 way = OnFloor(hit.Direction);
                    KnockSun(way.sqrMagnitude > 0.01f ? way.normalized : Facing(yaw), false);
                    return HitOutcome.Landed;
                }
            }
            if (!IsStaggered && Stagger.Add(hit.Stagger * (open ? 0.7f : 0.35f) * unsteadies)) BeginStagger();
            return HitOutcome.Landed;
        }

        public override void Unbalance(float amount)
        {
            if (mode == Mode.Hidden || mode == Mode.Entering || IsDead || IsStaggered) return;
            if (Stagger.Add(amount)) BeginStagger();
        }

        // The sun is out of its hold: thrown off at right angles to the way it was going (or the way the blow went), dull and harmless.
        private void KnockSun(Vector3 going, bool squareToIt)
        {
            if (sunState == SunState.Loose || IsDead) return;
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
            ScreenFlash.Flash(new Color(1f, 0.95f, 0.8f, 0.35f), 0.08f);
            if (shaker != null) shaker.Shake(0.4f, 0.3f);
            if (!saidLoose && victimDefence != null)
            {
                saidLoose = true;
                victimDefence.Callout("The sun is loose");
            }
            if (Stagger.Add(KnockBalance))
            {
                BeginStagger();
                return;
            }
            BeginExposed(false);
        }

        private void LoseSun(Vector3 rollsTo, float seconds)
        {
            sunState = SunState.Loose;
            sunRollTo = rollsTo;
            sunRollLeft = seconds;
            sunPart.SetVulnerable(true);
            Vfx.Play(VfxKind.Burst, sunAt, Vector3.up, 2f);
            Vfx.Play(VfxKind.Embers, sunAt, Vector3.up, 2f);
        }

        private void BeginExposed(bool rightAway)
        {
            headOpen = false;
            landed = false;
            SetOpen(true, true);
            Enter(Mode.Exposed);
            // After the broken balance it has lain long enough: it gets up at once and goes for the sun, still open.
            if (rightAway) elapsed = ExposedSeconds;
        }

        private void BeginStagger()
        {
            TimesStaggered++;
            HideMarkers();
            Blazing = false;
            if (sunState != SunState.Loose) LoseSun(KeepSun(OnFloor(sunAt) + Facing(yaw + 70f) * 3f), 0.5f);
            SetOpen(true, true);
            headOpen = false;
            Enter(Mode.Staggered);
            Vfx.Play(VfxKind.Ring, at + Vector3.up * 2f, Vector3.up);
            ScreenFlash.Flash(new Color(1f, 0.95f, 0.8f, 0.35f), 0.08f);
            if (shaker != null) shaker.Shake(0.5f, 0.4f);
        }

        private void BeginDying()
        {
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
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.6f), 0.12f);
            if (shaker != null) shaker.Shake(0.5f, 0.5f);
        }

        private void Enter(Mode next)
        {
            mode = next;
            elapsed = 0f;
            step = 0;
            struck = false;
            Locked = false;
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
                flat = Clear(flat + way / distance * pace, ObeliskClear, FloorRadius - 2f);
                legPhase += pace * 1.6f;
                if (turns) Turn(way, 220f, deltaTime);
            }
            at = new Vector3(flat.x, Mathf.MoveTowards(at.y, 0f, 14f * deltaTime), flat.z);
            return distance <= 0.15f;
        }

        private bool Fly(Vector3 to, float speed, float deltaTime, bool turns = true)
        {
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
            }
            if (mode == Mode.Dead || mode == Mode.Hidden) return;
            AdvanceFalls(deltaTime);
            AdvanceHot(deltaTime);
            AdvanceSun(deltaTime);
            AdvanceFury();
            AdvanceRoarRing();
            Pose();
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
            // While it comes, the camera leans toward where the sun is coming down, so the entrance is in the picture.
            if (mode == Mode.Entering) toMiddle = Vector3.ClampMagnitude((markPoint - OnFloor(target.position)) * 0.55f, 9f);
            shaker.SetFraming(ViewDistance, toMiddle, ViewPitch);
        }

        private void AdvanceFury()
        {
            if (furyShown || !Furious || IsDead) return;
            furyShown = true;
            zenithOwed = true;
            if (Sunlight.Hand != null) Sunlight.Hand.DegreesPerSecond = Mathf.Sign(Sunlight.Hand.DegreesPerSecond) * ShadowHand.InFury;
            ScreenFlash.Flash(new Color(1f, 0.8f, 0.4f, 0.4f), 0.1f);
            if (shaker != null) shaker.Shake(0.5f, 0.5f);
            RaiseEnraged();
        }

        // ------------------------------------------------------------------ the entrance

        private void AdvanceEntering(float deltaTime)
        {
            Vector3 from = markPoint;
            // A line of light where the spear stood; the sun comes down it; the scarab comes round it from behind.
            if (lineRenderer != null)
            {
                lineRenderer.enabled = elapsed < 2.4f;
                lightLine.position = from + Vector3.up * 20f;
                float thick = elapsed < 1.4f ? Mathf.Lerp(0.05f, 0.5f, elapsed / 1.4f) : Mathf.Lerp(0.5f, 0f, (elapsed - 1.4f) / 1f);
                lightLine.localScale = new Vector3(thick, 20f, thick);
            }
            float down = Mathf.Clamp01((elapsed - 1.4f) / 0.8f);
            sunAt = from + Vector3.up * Mathf.Lerp(40f, 1.5f, down * down);
            if (elapsed >= 2.2f && step == 0)
            {
                step = 1;
                Vfx.Play(VfxKind.Impact, from + Vector3.up);
                Vfx.Play(VfxKind.Ring, from + Vector3.up * 0.2f, Vector3.up);
                Vfx.Play(VfxKind.Fire, from + Vector3.up, Vector3.up, 3f);
                ScreenFlash.Flash(new Color(1f, 0.9f, 0.6f, 0.45f), 0.1f);
                if (shaker != null) shaker.Shake(0.7f, 0.5f);
            }
            float up = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - 2.2f) / 0.8f));
            size = Mathf.Max(0.01f, up);
            float rear = Mathf.Clamp01((elapsed - 3f) / 0.4f) * (1f - Mathf.Clamp01((elapsed - 3.8f) / 0.4f));
            pitch = -24f * rear;
            lift = 0.85f * rear;
            if (elapsed >= RoarAt && !roared)
            {
                roared = true;
                roarLeft = 0.9f;
                if (shaker != null) shaker.Shake(0.6f, 0.6f);
                Vfx.Play(VfxKind.Burst, at + Vector3.up * 2f, Vector3.up, 3f);
                RaiseRoared();
            }
            if (elapsed < EntranceSeconds) return;
            size = 1f;
            sunState = SunState.Held;
            if (bulk != null) bulk.enabled = true;
            SetOpen(false, true);
            if (Sunlight.Hand != null) Sunlight.Hand.DegreesPerSecond = Mathf.Sign(Sunlight.Hand.DegreesPerSecond) * ShadowHand.AgainstTheForm;
            rest = RestBetween;
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
            if (OnlyWhenAsked && Queued == SunBearerAttack.None && !zenithOwed) return;
            if (rest <= 0f || Queued != SunBearerAttack.None || zenithOwed) Choose();
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
            elapsed = 0f;
            step = 0;
            struck = false;
            moveFrom = OnFloor(at);
            Vector3 flat = OnFloor(at);
            Vector3 out_ = flat.sqrMagnitude > 1f ? flat.normalized : Vector3.forward;
            laneFrom = out_ * (FloorRadius - 3f);
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
            if (step == 1) lanes[0].Show(laneFrom, laneFacing, 1f, RedMarker, true, RollWidth, laneLength / (LaneUnit * RollWidth));
            if (step == 1 && Time.frameCount % 3 == 0) Vfx.Play(VfxKind.Fire, sunAt + Vector3.down * 1.2f - laneAlong, Vector3.up, 1.6f);
            if (step == 1 && !struck && SunMeets(sunAlong, RollWidth * 0.5f, 1.7f))
            {
                struck = true;
                if (Deal(new Hit(furrows ? FurrowDamage : RollDamage, 0f, laneAlong, false, true, sunAt), 0f) == HitOutcome.PerfectGuard)
                {
                    LayLane(sunAlong, RollWidth, furrows ? FurrowHotSeconds : HotSeconds);
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
                LayLane(laneLength, RollWidth, furrows ? FurrowHotSeconds : HotSeconds);
                laneDone = laneLength;
                elapsed = 100f;
                if (shaker != null) shaker.Shake(0.35f, 0.25f);
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
            markPoint = Clear(moveFrom + to, 3.6f, FloorRadius - 1.5f);
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
                Vfx.Play(VfxKind.Ring, markPoint + Vector3.up * 0.2f, Vector3.up);
                for (int i = 0; i < 6; i++) Vfx.Play(VfxKind.Dust, markPoint + Facing(i * 60f) * RamRadius, Vector3.up);
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
            // Back to the sun it left.
            Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
            Turn(toSun, 200f, deltaTime);
            bool there = Walk(OnFloor(sunAt) - toSun.normalized * 4.4f, WalkSpeed * 1.5f, deltaTime, false) || stuck > RamStuck + 2.5f;
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
                    falls[i].Show(fallPoints[i], Quaternion.identity, 1f - fallLeft[i] / fallWarning[i], RedMarker, false, fallRadius[i]);
                    continue;
                }
                fallLeft[i] = 0f;
                falls[i].Hide();
                if (gone) continue;
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
                    Vfx.Play(VfxKind.Ring, OnFloor(at) + Vector3.up * 0.2f, Vector3.up);
                    ScreenFlash.Flash(new Color(1f, 0.9f, 0.6f, 0.35f), 0.08f);
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
                EndAttack();
                return;
            }
            // The Setting: down on the rim furthest from the player, folded, the sun set down in front of it.
            if (step == 0)
            {
                bool there = Fly(landAt + Vector3.up * 0.2f, 15f, deltaTime);
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
                Vfx.Play(VfxKind.Dust, at, Vector3.up, 2f);
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
        }

        private void AimStoop() => markPoint = Clear(PlayerOnFloor, 4.6f, FloorRadius - 2f);

        private void AdvanceStoop(float deltaTime)
        {
            float tell = StoopFollow + StoopHold, drop = 0.24f;
            Vector3 stand = markPoint + Vector3.up * 3.6f;
            if (elapsed < tell)
            {
                if (elapsed < StoopFollow) AimStoop();
                Locked = elapsed >= StoopFollow;
                // Over the circle, wings high, looking straight down into it.
                Fly(markPoint + Vector3.up * (FlightHeight + 1f), 26f, deltaTime, false);
                Turn(Vector3.back, 200f, deltaTime);
                roll = Mathf.MoveTowards(roll, 0f, 80f * deltaTime);
                pitch = Mathf.MoveTowards(pitch, 30f, 80f * deltaTime);
                flap = Mathf.Lerp(Mathf.Sin(Time.time * 8f) * 30f, 55f, Mathf.Clamp01((elapsed - StoopFollow) / StoopHold));
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
                return;
            }
            if (!struck)
            {
                struck = true;
                at = stand;
                pitch = 0f;
                circles[0].Hide();
                Vfx.Play(VfxKind.Impact, markPoint + Vector3.up * 0.6f);
                Vfx.Play(VfxKind.Ring, markPoint + Vector3.up * 0.2f, Vector3.up);
                Vfx.Play(VfxKind.Fire, markPoint + Vector3.up * 0.5f, Vector3.up, 4f);
                for (int i = 0; i < 6; i++) Vfx.Play(VfxKind.Dust, markPoint + Facing(i * 60f) * StoopRadius, Vector3.up);
                if (shaker != null) shaker.Shake(0.9f, 0.4f);
                ScreenFlash.Flash(new Color(1f, 0.85f, 0.5f, 0.25f), 0.06f);
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
            Enter(Mode.Pass);
        }

        // A lane from rim to rim through the player, laid across the line from the obelisk to them so that neither
        // the sun nor a wing goes through the stone.
        private void AimPass(float turned)
        {
            Vector3 player = PlayerOnFloor;
            Vector3 out_ = player.sqrMagnitude > 1f ? player.normalized : Vector3.back;
            Vector3 through = out_ * Mathf.Max(player.magnitude, 5.5f);
            laneAlong = Quaternion.Euler(0f, turned, 0f) * new Vector3(out_.z, 0f, -out_.x);
            // From the end nearer the falcon.
            if (Vector3.Dot(OnFloor(at) - through, laneAlong) > 0f) laneAlong = -laneAlong;
            float back = StormDragon.Chord(through, -laneAlong, FloorRadius - 0.5f);
            laneFrom = through - laneAlong * back;
            laneLength = back + StormDragon.Chord(through, laneAlong, FloorRadius - 0.5f);
        }

        private void AdvancePass(float deltaTime)
        {
            var laneFacing = Quaternion.LookRotation(laneAlong);
            Vector3 start = laneFrom + Vector3.up * PassHeight;
            // It leans away from the obelisk, the inner wing high.
            float lean = (laneFrom.x * laneAlong.z - laneFrom.z * laneAlong.x) > 0f ? -32f : 32f;
            if (elapsed < PassShown)
            {
                // The lane is laid once, where the player stood when it began, and shown for its whole warning.
                Locked = true;
                Fly(start - laneAlong * 3f, 40f, deltaTime, false);
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
                at = laneFrom + laneAlong * along + Vector3.up * PassHeight;
                flap = 18f;
                sunAt = HeldSunPoint();
                lanes[0].Show(laneFrom, laneFacing, 1f, RedMarker, true, PassWidth, laneLength / (LaneUnit * PassWidth));
                if (Time.frameCount % 2 == 0) Vfx.Play(VfxKind.Fire, OnFloor(sunAt) + Vector3.up * 0.3f, Vector3.up, 1.8f);
                if (!struck && SunMeets(along, PassWidth * 0.5f, 2.6f))
                {
                    struck = true;
                    if (Deal(new Hit(PassDamage, 0f, laneAlong, false, true, sunAt), PassGuardBalance) == HitOutcome.PerfectGuard)
                    {
                        LayLane(Mathf.Max(2f, along), PassWidth, HotSeconds);
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
                LayLane(laneLength, PassWidth, HotSeconds);
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
                return;
            }
            float burning = elapsed - Climb - ZenithFill;
            if (burning < blaze)
            {
                if (!Blazing)
                {
                    Blazing = true;
                    untilBlaze = 0f;
                    ScreenFlash.Flash(new Color(1f, 0.9f, 0.6f, 0.5f), 0.12f);
                    if (shaker != null) shaker.Shake(0.5f, 0.6f);
                }
                // In its fury the shadow stops half-way through, and comes back the other way.
                if (Furious && !turnedBack && burning >= blaze * 0.5f && hand != null)
                {
                    turnedBack = true;
                    hand.StopAndTurnBack(0.5f);
                }
                float beat = 0.75f + 0.25f * Mathf.Sin(Time.time * 17f);
                floorMark.Show(Vector3.zero, Quaternion.identity, 1f, HotMarker, false, 1f, 1f, beat);
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
                        if (!Sunlight.InHand(point)) Vfx.Play(VfxKind.Fire, point + Vector3.up * 0.2f, Vector3.up, 2.2f);
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
            if (Blazing)
            {
                Blazing = false;
                floorMark.Hide();
                shadowMark.Hide();
                sunSize = 1f;
                sinceZenith = 0f;
                Vector3 along = hand != null ? hand.Direction : Vector3.forward;
                landAt = Clear(along * 9f + new Vector3(along.z, 0f, -along.x) * 7f, 6f, FloorRadius - 6f);
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
                bool there = Fly(landAt + Vector3.up * 0.2f, 13f, deltaTime);
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
                Vfx.Play(VfxKind.Dust, at, Vector3.up, 2.5f);
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
            float most = Furious ? ExposedMostInFury : ExposedMost;
            flap = Mathf.MoveTowards(flap, 0f, 200f * deltaTime);
            if (elapsed < ExposedSeconds)
            {
                // The scarab on its back with its legs working; the falcon flat on the floor with its wings spread.
                at.y = Mathf.MoveTowards(at.y, falcon ? 0.5f : 0f, 30f * deltaTime);
                roll = Mathf.MoveTowards(roll, falcon ? 0f : 180f, 520f * deltaTime);
                pitch = Mathf.MoveTowards(pitch, falcon ? 8f : 0f, 120f * deltaTime);
                lift = Mathf.MoveTowards(lift, falcon ? 1f : 0.2f, 3f * deltaTime);
                legPhase += 14f * deltaTime;
                if (bulk != null) bulk.enabled = true;
                return;
            }
            roll = Mathf.MoveTowards(roll, 0f, 400f * deltaTime);
            pitch = Mathf.MoveTowards(pitch, 0f, 60f * deltaTime);
            Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
            float reach = falcon ? 2.6f : 4.6f;
            if (step == 0)
            {
                // It goes to the sun, still open. If the sun lies too far, the sun comes to it.
                if (toSun.magnitude > reach) Walk(OnFloor(sunAt) - toSun.normalized * (reach - 0.2f), 3f, deltaTime);
                if (elapsed >= most && sunRollLeft <= 0f)
                {
                    sunRollTo = KeepSun(OnFloor(at) + (toSun.sqrMagnitude > 0.01f ? toSun.normalized : Facing(yaw)) * (reach - 0.4f));
                    sunRollLeft = 0.8f;
                }
                if (toSun.magnitude > reach + 0.2f) return;
                step = 1;
                prepare = elapsed;
                Turn(toSun, 3600f, 1f);
            }
            // It takes the sun up.
            Turn(toSun, 200f, deltaTime);
            if (elapsed - prepare < 1f) return;
            sunState = SunState.Held;
            sunPart.SetVulnerable(false);
            Vfx.Play(VfxKind.Fire, sunAt, Vector3.up, 3f);
            if (falcon && bulk != null) bulk.enabled = false;
            EndAttack();
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
                Vfx.Play(VfxKind.Dust, at + Facing(yaw) * 2.5f, Vector3.up, 2f);
                if (shaker != null) shaker.Shake(0.6f, 0.35f);
            }
            if (elapsed < StaggeredSeconds) return;
            Stagger.Reset();
            BeginExposed(true);
        }

        // It reaches for the sun and cannot lift it. The gold goes dark from the edges in; what is left is small.
        // The sun rolls to the foot of the obelisk and, in its shadow, shrinks to a coal.
        private void AdvanceDying(float deltaTime)
        {
            at.y = Mathf.MoveTowards(at.y, 0f, 20f * deltaTime);
            float t = Mathf.Clamp01(elapsed / DeathSeconds);
            float shudder = Mathf.Sin(Time.time * 47f) * (1f - t);
            Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
            if (elapsed < 1.6f)
            {
                Turn(toSun, 120f, deltaTime);
                Walk(OnFloor(at) + (toSun.sqrMagnitude > 0.01f ? toSun.normalized : Facing(yaw)), 1.2f, deltaTime, false);
            }
            pitch = Mathf.MoveTowards(pitch, 14f, 40f * deltaTime) + shudder;
            roll = shudder * 3f;
            lift = Mathf.MoveTowards(lift, 0.6f, deltaTime);
            flap = Mathf.MoveTowards(flap, -20f, 30f * deltaTime);
            size = Mathf.Lerp(1f, 0.12f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - 1.2f) / 2.4f)));
            if (Time.frameCount % 5 == 0) Vfx.Play(VfxKind.Ash, at + new Vector3(Random.Range(-2f, 2f), Random.Range(0.5f, 2.5f), Random.Range(-2f, 2f)) * size, Vector3.up, 2f);
            if (Time.frameCount % 9 == 0) Vfx.Play(VfxKind.Embers, at + Vector3.up * 1.5f, Vector3.up, 1.5f);
            if (elapsed >= 1.2f && step == 0)
            {
                step = 1;
                var hand = Sunlight.Hand;
                sunRollTo = (hand != null ? hand.Direction : Vector3.forward) * 3.4f;
                sunRollLeft = 2.6f;
            }
            sunSize = Mathf.Lerp(1f, 0.14f, Mathf.Clamp01((elapsed - 2.6f) / 2f));
            if (elapsed < DeathSeconds) return;
            body.gameObject.SetActive(false);
            if (shadowRenderer != null) shadowRenderer.enabled = false;
            if (finder != null) finder.enabled = false;
            if (shaker != null) shaker.ClearFraming();
            sun.gameObject.SetActive(false);
            mode = Mode.Dead;
            RaiseDied();
        }

        // ------------------------------------------------------------------ the sun, and the floor it leaves hot

        private void AdvanceSun(float deltaTime)
        {
            if (sunState == SunState.Held && mode != Mode.Roll && mode != Mode.Pass)
                sunAt = Vector3.MoveTowards(sunAt, HeldSunPoint(), 40f * deltaTime);
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
                sunAt = new Vector3(flat.x, Mathf.MoveTowards(sunAt.y, 1.5f * sunSize, 30f * deltaTime), flat.z);
            }
            // The burning sun on the floor burns whoever touches it.
            untilTouch -= deltaTime;
            bool burns = sunState != SunState.Loose && sunAt.y < 3.2f && mode != Mode.Entering && mode != Mode.Roll && !IsDead;
            if (burns && untilTouch <= 0f && target != null && (PlayerOnFloor - OnFloor(sunAt)).magnitude <= 2.1f)
            {
                untilTouch = 0.7f;
                blowName = "the sun touched";
                Vector3 off = PlayerOnFloor - OnFloor(sunAt);
                Deal(new Hit(SunTouchDamage, 0f, off.sqrMagnitude > 0.01f ? off.normalized : Vector3.back, false, true, sunAt), 0f);
            }
        }

        private void LayLane(float length, float width, float seconds)
        {
            int slot = FreeHot();
            hotIsLane[slot] = true;
            hotFrom[slot] = laneFrom;
            hotAlong[slot] = laneAlong;
            hotLength[slot] = length;
            hotWide[slot] = width;
            hotLeft[slot] = seconds;
        }

        private void LayCrater(Vector3 centre, float radius, float seconds)
        {
            int slot = FreeHot();
            hotIsLane[slot] = false;
            hotFrom[slot] = centre;
            hotWide[slot] = radius;
            hotLeft[slot] = seconds;
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
            onHotFor = 0f;
            if (hotLanes == null) return;
            foreach (var mark in hotLanes) mark.Hide();
            foreach (var mark in hotCircles) mark.Hide();
        }

        /// <summary>True for a point on floor that is still hot (whether or not the shadow lies across it).</summary>
        public bool OnHotFloor(Vector3 point)
        {
            for (int i = 0; i < HotMost; i++)
            {
                if (hotLeft[i] <= 0f) continue;
                Vector3 offset = OnFloor(point) - hotFrom[i];
                if (!hotIsLane[i])
                {
                    if (offset.sqrMagnitude <= hotWide[i] * hotWide[i]) return true;
                    continue;
                }
                float along = offset.x * hotAlong[i].x + offset.z * hotAlong[i].z;
                float across = Mathf.Abs(offset.x * hotAlong[i].z - offset.z * hotAlong[i].x);
                if (along >= 0f && along <= hotLength[i] && across <= hotWide[i] * 0.5f) return true;
            }
            return false;
        }

        private void AdvanceHot(float deltaTime)
        {
            int lanesHot = 0, patches = 0, laneMark = 0, circleMark = 0;
            untilFlame -= deltaTime;
            bool flame = untilFlame <= 0f;
            if (flame) untilFlame = 0.16f;
            for (int i = 0; i < HotMost; i++)
            {
                if (hotLeft[i] <= 0f) continue;
                hotLeft[i] -= deltaTime;
                if (hotLeft[i] <= 0f) continue;
                patches++;
                // It cools: strong, then fading over its last two seconds.
                float strength = Mathf.Clamp01(hotLeft[i] / 2f) * (0.8f + 0.2f * Mathf.Sin(Time.time * 9f + i));
                if (hotIsLane[i])
                {
                    lanesHot++;
                    if (laneMark < hotLanes.Length)
                        hotLanes[laneMark++].Show(hotFrom[i], Quaternion.LookRotation(hotAlong[i]), 1f, HotMarker, true, hotWide[i], hotLength[i] / (LaneUnit * hotWide[i]), strength);
                    if (flame)
                    {
                        Vector3 point = hotFrom[i] + hotAlong[i] * (Random.value * hotLength[i]) + new Vector3(hotAlong[i].z, 0f, -hotAlong[i].x) * ((Random.value - 0.5f) * hotWide[i]);
                        if (!Sunlight.InHand(point)) Vfx.Play(VfxKind.Fire, point + Vector3.up * 0.15f, Vector3.up, 1.1f);
                    }
                }
                else
                {
                    if (circleMark < hotCircles.Length) hotCircles[circleMark++].Show(hotFrom[i], Quaternion.identity, 1f, HotMarker, false, hotWide[i], 1f, strength);
                    if (flame && !Sunlight.InHand(hotFrom[i])) Vfx.Play(VfxKind.Fire, hotFrom[i] + Facing(Random.value * 360f) * (Random.value * hotWide[i]) + Vector3.up * 0.15f, Vector3.up, 1.1f);
                }
            }
            for (int i = laneMark; i < hotLanes.Length; i++)
                if (hotLanes[i].Shown) hotLanes[i].Hide();
            for (int i = circleMark; i < hotCircles.Length; i++)
                if (hotCircles[i].Shown) hotCircles[i].Hide();
            HotLanes = lanesHot;
            HotPatches = patches;
            if (target == null || victim == null) return;
            // Hot floor bites whoever stands on it, a moment after they step on; under the obelisk's shadow it is cold.
            bool standsOn = patches > 0 && OnHotFloor(target.position) && !Sunlight.InHand(target.position);
            if (!standsOn)
            {
                onHotFor = 0f;
                return;
            }
            if (onHotFor <= 0f) untilHotBite = HotAfter;
            onHotFor += deltaTime;
            untilHotBite -= deltaTime;
            if (untilHotBite > 0f) return;
            untilHotBite = HotEvery;
            HotBites++;
            blowName = "hot floor";
            Deal(new Hit(HotDamage, 0f, Vector3.down, false, true), 0f);
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
            if (Stagger.Add(balanceIfTurned)) BeginStagger();
            return outcome;
        }

        private void HideMarkers()
        {
            foreach (var mark in lanes) mark.Hide();
            foreach (var mark in circles) mark.Hide();
            floorMark.Hide();
            shadowMark.Hide();
            for (int i = 0; i < fallLeft.Length; i++)
            {
                fallLeft[i] = 0f;
                falls[i].Hide();
            }
            landed = false;
            sunSize = 1f;
        }

        // ------------------------------------------------------------------ the body

        private void Pose()
        {
            body.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(pitch, 0f, roll));
            body.localScale = Vector3.one * size;
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
                if (high) falconShadow.position = OnFloor(at) + Vector3.up * 0.045f;
            }
            bool loose = sunState == SunState.Loose;
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
            bool dark = mode == Mode.Exposed || mode == Mode.Dying;
            bool lit = flash || pulse || dark;
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
                block.SetColor(Emission, flash ? new Color(1.5f, 1.5f, 1.5f) : pulse ? new Color(0.8f, 0.55f, 0.12f) : Color.black);
                shown.SetPropertyBlock(block);
            }
        }
    }
}
