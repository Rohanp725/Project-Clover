using Clover.Core;
using Clover.Player;
using Clover.World;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// A god in its godly form: too large to stand on the floor the player fights on, it rises beside it, and fights
    /// with its hands. Its head, body and two hands are rigid pieces moved by this code. A hand slams down where the
    /// player stands, or sweeps the floor from one side to the other, and then lies where it stopped for a moment:
    /// that is when it can be cut. Bolts are called down between. Blows on the hands wear down its balance; when that
    /// breaks, the head falls to the floor and can be struck, and finished.
    /// </summary>
    public class Colossus : MonoBehaviour, IBoss
    {
        private enum Mode { Hidden, Rising, Idle, Slam, Sweep, Bolts, Staggered, Recovering, Dying, Dead }

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
        /// <summary>Below this share of health it acts faster and slams twice.</summary>
        public float FuryAt = 0.5f;
        public float FuryPace = 1.25f;
        public float RiseSeconds = 2.6f;
        public float RestBetween = 0.9f;

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

        public Color PlainMarker = new Color(1f, 0.93f, 0.75f, 1f);
        public Color RedMarker = new Color(1f, 0.08f, 0.05f, 1f);

        public Health Health { get; } = new Health();
        public StaggerMeter Stagger { get; } = new StaggerMeter();
        public bool IsStaggered => mode == Mode.Staggered;
        public bool IsDead => mode == Mode.Dead || mode == Mode.Dying;
        public bool HasRisen => mode != Mode.Hidden;
        public bool InFight => mode != Mode.Hidden && mode != Mode.Dead;
        public float HealthFraction => Health.Fraction;
        public float BalanceFraction => IsStaggered ? 1f : Stagger.Fraction;
        /// <summary>Raised once, when it has fallen and gone.</summary>
        public event System.Action<Colossus> Died;
        // Counters so tests and probes can see what happened without watching the screen.
        public int Slams { get; private set; }
        public int Sweeps { get; private set; }
        public int BoltsCalled { get; private set; }
        public int BlowsStruck { get; private set; }
        public int TimesStaggered { get; private set; }
        public string Doing => mode.ToString();

        private static readonly int MarkerColour = Shader.PropertyToID("_Color");
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");

        private Mode mode = Mode.Hidden;
        private float elapsed;
        private float rest;
        private IDamageable victim;
        private PlayerController victimBody;
        private TopDownCamera shaker;
        private HitStop hitStop;
        private MaterialPropertyBlock block;
        private Mesh slamMesh, sweepMesh, boltMesh;
        private Renderer slamOutlineRenderer, slamFillRenderer, sweepOutlineRenderer, sweepFillRenderer;
        private Renderer[] boltOutlineRenderers, boltFillRenderers;
        private Vector3[] boltPoints;
        private float[] boltLeft;
        private int boltsToCall;
        private float untilBolt;
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
        private Mode lastAttack = Mode.Idle;
        private float flashLeft;
        private bool flashing;
        private float headYaw;
        private Vector3 leftRest, rightRest;
        private float untilJitter;
        private readonly Vector3[] tetherPoints = new Vector3[9];
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

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            Health.Maximum = MaximumHealth;
            Health.Fill();
            Stagger.Maximum = BalanceMaximum;
            Stagger.StaggeredSeconds = StaggeredSeconds;
            Stagger.Reset();
            slamMesh = MarkerShapes.Sector(SlamRadius, 360f);
            sweepMesh = MarkerShapes.Lane(FloorRadius * 2f, SweepWidth);
            boltMesh = MarkerShapes.Sector(BoltRadius, 360f);
            slamOutlineRenderer = Prepare(slamOutline, slamMesh);
            slamFillRenderer = Prepare(slamFill, slamMesh);
            sweepOutlineRenderer = Prepare(sweepOutline, sweepMesh);
            sweepFillRenderer = Prepare(sweepFill, sweepMesh);
            boltOutlineRenderers = new Renderer[boltOutlines.Length];
            boltFillRenderers = new Renderer[boltFills.Length];
            boltPoints = new Vector3[boltOutlines.Length];
            boltLeft = new float[boltOutlines.Length];
            for (int i = 0; i < boltOutlines.Length; i++)
            {
                boltOutlineRenderers[i] = Prepare(boltOutlines[i], boltMesh);
                boltFillRenderers[i] = Prepare(boltFills[i], boltMesh);
            }
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
            if (slamMesh != null) Destroy(slamMesh);
            if (sweepMesh != null) Destroy(sweepMesh);
            if (boltMesh != null) Destroy(boltMesh);
        }

        private static Renderer Prepare(MeshFilter filter, Mesh mesh)
        {
            if (filter == null) return null;
            filter.sharedMesh = mesh;
            var shown = filter.GetComponent<Renderer>();
            if (shown != null) shown.enabled = false;
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
            foreach (var tether in tethers)
                if (tether != null) tether.enabled = shown;
        }

        /// <summary>Brings it up out of the cloud. The fight with it begins when it has risen.</summary>
        public void Rise()
        {
            if (mode != Mode.Hidden) return;
            Health.Fill();
            Stagger.Reset();
            SetShown(true);
            Enter(Mode.Rising);
            Place(0f);
        }

        /// <summary>A blow has landed on one of its pieces.</summary>
        public HitOutcome TakePartHit(ColossusPart part, in Hit hit)
        {
            if (mode == Mode.Hidden || IsDead || mode == Mode.Rising) return HitOutcome.Missed;
            bool finisher = part.IsHead && IsStaggered && hit.Heavy;
            float damage = finisher
                ? Mathf.Max(hit.Damage * 3f, MaximumHealth * FinisherShare)
                : hit.Damage * (part.IsHead ? HeadDamage : HandDamage);
            Health.Damage(damage);
            flashLeft = 0.09f;
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
            if (!IsStaggered && Stagger.Add(hit.Stagger * 0.7f)) BeginStagger();
            return HitOutcome.Landed;
        }

        /// <summary>Wears at its balance from outside a blow; enough of it breaks the balance at once.</summary>
        public void Unbalance(float amount)
        {
            if (mode == Mode.Hidden || mode == Mode.Rising || IsDead || IsStaggered) return;
            if (Stagger.Add(amount)) BeginStagger();
        }

        public ColossusPart HeadPart => headPart;
        public ColossusPart LeftPart => leftPart;
        public ColossusPart RightPart => rightPart;

        private void BeginStagger()
        {
            TimesStaggered++;
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
            Enter(Mode.Dying);
            if (hitStop != null) hitStop.Slow(1.6f, 0.25f);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.7f), 0.14f);
        }

        private void Enter(Mode next)
        {
            mode = next;
            elapsed = 0f;
        }

        private float Pace => Health.Fraction <= FuryAt ? FuryPace : 1f;

        // Where things stand. North is +Z; the floor's middle is the origin.
        private Vector3 HeadHome() => new Vector3(0f, 1.2f, FloorRadius + 3.6f);
        private Vector3 FallenHead() => new Vector3(0f, 0.6f, FloorRadius - 9f);
        private Vector3 BodyHome() => new Vector3(0f, -3.2f, FloorRadius + 8.5f);
        private Vector3 Shoulder(bool left) => new Vector3(left ? -6f : 6f, 3.5f, FloorRadius + 6.5f);

        private void Update()
        {
            if (mode == Mode.Hidden || mode == Mode.Dead) return;
            float deltaTime = Time.deltaTime * WorldClock.EnemyScale;
            elapsed += deltaTime * (mode == Mode.Staggered || mode == Mode.Dying || mode == Mode.Rising ? 1f : Pace);
            switch (mode)
            {
                case Mode.Rising:
                    Place(Mathf.Clamp01(elapsed / RiseSeconds));
                    if (shaker != null && Time.frameCount % 8 == 0) shaker.Shake(0.12f, 0.2f);
                    if (elapsed >= RiseSeconds)
                    {
                        rest = RestBetween;
                        Enter(Mode.Idle);
                    }
                    break;
                case Mode.Idle:
                    Hover(deltaTime, true, true);
                    FaceTarget(deltaTime, 0f);
                    Stagger.Tick(deltaTime);
                    rest -= deltaTime * Pace;
                    if (rest <= 0f) Choose();
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
                    if (boltsToCall <= 0 && AllBoltsLanded())
                    {
                        rest = RestBetween;
                        Enter(Mode.Idle);
                    }
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
            AdvanceBolts(deltaTime);
            Paint();
            DrawTethers();
            // Once the head has come down, not while it is still falling.
            if (staggerMark != null) staggerMark.Show(mode == Mode.Staggered && elapsed >= 0.5f);
        }

        // ------------------------------------------------------------------ choosing

        private void Choose()
        {
            if (target == null) return;
            if (secondSlamOwed)
            {
                secondSlamOwed = false;
                BeginSlam(!handIsLeft);
                return;
            }
            float roll = Random.value;
            Mode pick = roll < 0.45f ? Mode.Slam : roll < 0.75f ? Mode.Sweep : Mode.Bolts;
            // Never the bolts twice running, and never three of anything.
            if (pick == Mode.Bolts && lastAttack == Mode.Bolts) pick = Mode.Slam;
            lastAttack = pick;
            if (pick == Mode.Slam)
            {
                BeginSlam(target.position.x < 0f);
                secondSlamOwed = Health.Fraction <= FuryAt;
            }
            else if (pick == Mode.Sweep) BeginSweep();
            else BeginBolts();
        }

        private void Take(bool left)
        {
            handIsLeft = left;
            hand = left ? leftHand : rightHand;
            handPart = left ? leftPart : rightPart;
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
                handPart.SetVulnerable(true);
            }
            else if (elapsed >= fall + SlamRest)
            {
                handPart.SetVulnerable(false);
                rest = RestBetween * (secondSlamOwed ? 0.3f : 1f);
                Enter(Mode.Idle);
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
                rest = RestBetween;
                Enter(Mode.Idle);
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
            }
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

        private void AdvanceDying(float deltaTime)
        {
            const float Seconds = 3f;
            float t = Mathf.Clamp01(elapsed / Seconds);
            // It comes apart: the pieces shudder, drift from one another, and go down into the cloud.
            float shudder = Mathf.Sin(Time.time * 47f) * 0.12f * (1f - t);
            float sink = t * t * 16f;
            head.position = HeadHome() + new Vector3(shudder, -sink, t * 2f);
            head.rotation = Quaternion.Euler(-28f - 30f * t, 180f + shudder * 40f, shudder * 30f);
            body.position = BodyHome() + new Vector3(-shudder, -sink * 0.7f, 0f);
            leftHand.position += new Vector3(-0.8f, -1.2f - sink * 0.3f, 0.6f) * deltaTime * 3f;
            rightHand.position += new Vector3(0.8f, -1.2f - sink * 0.3f, 0.6f) * deltaTime * 3f;
            if (Time.frameCount % 9 == 0)
            {
                Vector2 spot = Random.insideUnitCircle * 8f;
                LightningBolts.Strike(new Vector3(spot.x, 0f, FloorRadius - 4f + spot.y), 0.3f);
            }
            if (shaker != null && Time.frameCount % 6 == 0) shaker.Shake(0.2f * (1f - t), 0.2f);
            if (elapsed < Seconds) return;
            mode = Mode.Dead;
            SetShown(false);
            Died?.Invoke(this);
        }

        // ------------------------------------------------------------------ moving the pieces

        // The whole of it, coming up out of the cloud: 0 is under, 1 is risen.
        private void Place(float risen)
        {
            float up = 1f - (1f - risen) * (1f - risen);
            float under = (1f - up) * 16f;
            head.position = HeadHome() + Vector3.down * under;
            head.rotation = Quaternion.Euler(-28f, 180f, 0f);
            head.localScale = Vector3.one * HeadScale;
            body.position = BodyHome() + Vector3.down * under;
            body.rotation = Quaternion.Euler(-8f, 180f, 0f);
            leftHand.position = leftRest + new Vector3(0f, -under * 0.9f, FloorRadius - 12f);
            rightHand.position = rightRest + new Vector3(0f, -under * 0.9f, FloorRadius - 12f);
            leftHand.rotation = rightHand.rotation = Quaternion.Euler(-20f, 180f, 0f);
            leftHand.localScale = new Vector3(-HandScale, HandScale, HandScale);
            rightHand.localScale = Vector3.one * HandScale;
        }

        // Hands not in use hang in the air on either side of the player, a little north, rising and falling.
        private void Hover(float deltaTime, bool left, bool right, float higher = 0f)
        {
            Vector3 about = target != null ? target.position : Vector3.zero;
            about.y = 0f;
            about.z = Mathf.Min(about.z + 4.5f, FloorRadius - 4f);
            float bob = Mathf.Sin(Time.time * 1.7f) * 0.35f;
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
            float breath = Mathf.Sin(Time.time * 1.3f);
            head.position = Vector3.Lerp(head.position, HeadHome() + Vector3.up * (breath * 0.25f), 1f - Mathf.Exp(-4f * deltaTime));
            Turn(head, Quaternion.Euler(-28f + nod + breath * 2f, 180f - headYaw, 0f), deltaTime);
        }

        private static void Turn(Transform piece, Quaternion toward, float deltaTime) =>
            piece.rotation = Quaternion.Slerp(piece.rotation, toward, 1f - Mathf.Exp(-8f * deltaTime));

        private static Vector3 OnFloor(Vector3 point) => new Vector3(point.x, 0f, point.z);

        // ------------------------------------------------------------------ blows, marks, light

        // A hand has come down: the ground shows it, and the player is struck if under it.
        private void Land(Vector3 point, float radius, float damage, bool unblockable)
        {
            Vfx.Play(VfxKind.Impact, point + Vector3.up * 0.6f);
            Vfx.Play(VfxKind.Ring, point + Vector3.up * 0.2f, Vector3.up);
            Vfx.Play(VfxKind.Burst, point + Vector3.up * 0.5f);
            for (int i = 0; i < 6; i++)
                Vfx.Play(VfxKind.Dust, point + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * (radius * 0.8f), Vector3.up);
            if (shaker != null) shaker.Shake(0.55f, 0.3f);
            ScreenFlash.Flash(new Color(0.85f, 0.9f, 1f, 0.22f), 0.06f);
            Strike(point, radius, damage, unblockable);
        }

        private void Strike(Vector3 point, float radius, float damage, bool unblockable)
        {
            if (target == null || victim == null) return;
            Vector3 offset = target.position - point;
            offset.y = 0f;
            if (offset.magnitude > radius + 0.35f) return;
            // In the air on a leap, nothing on the ground reaches the player.
            if (victimBody != null && victimBody.IsLeaping) return;
            BlowsStruck++;
            var outcome = victim.TakeHit(new Hit(damage, 0f, offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.back, false, unblockable, point));
            // A hand turned at the last instant is thrown back up, and its balance goes with it.
            if (outcome != HitOutcome.PerfectGuard || IsStaggered || IsDead) return;
            flashLeft = 0.09f;
            if (Stagger.Add(PerfectGuardBalance)) BeginStagger();
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
            // A lane covers a third of the screen, so it is drawn fainter than a circle is.
            Tint(outline, colour, lane ? 0.14f : 0.2f);
            Tint(fill, colour, lane ? 0.3f : 0.5f);
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
            for (int i = 0; i < boltLeft.Length; i++)
            {
                boltLeft[i] = 0f;
                HideMark(boltOutlineRenderers[i], boltFillRenderers[i]);
            }
            boltsToCall = 0;
        }

        // White for an instant when struck, and a gold pulse for as long as it lies broken.
        private void Paint()
        {
            bool pulse = IsStaggered && Mathf.Repeat(Time.unscaledTime * 4f, 1f) < 0.5f;
            bool flash = flashLeft > 0f;
            flashLeft -= Time.unscaledDeltaTime;
            bool lit = flash || pulse;
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
                block.SetColor(Emission, flash ? new Color(1.5f, 1.5f, 1.5f) : new Color(0.8f, 0.55f, 0.12f));
                shown.SetPropertyBlock(block);
            }
        }

        // What holds each hand to him: a rope of lightning from the shoulder to the wrist, never the same twice.
        private void DrawTethers()
        {
            untilJitter -= Time.unscaledDeltaTime;
            if (untilJitter > 0f) return;
            untilJitter = 0.05f;
            for (int side = 0; side < tethers.Length && side < 2; side++)
            {
                var tether = tethers[side];
                if (tether == null || !tether.enabled) continue;
                bool left = side == 0;
                Transform piece = left ? leftHand : rightHand;
                Vector3 from = body.position + (Shoulder(left) - BodyHome());
                Vector3 to = piece.position + piece.rotation * new Vector3(0f, 0.8f * HandScale, -3f * HandScale);
                float length = (to - from).magnitude;
                for (int i = 0; i < tetherPoints.Length; i++)
                {
                    float t = i / (float)(tetherPoints.Length - 1);
                    Vector3 point = Vector3.Lerp(from, to, t);
                    // Slack in the middle, and jagged.
                    point.y += Mathf.Sin(t * Mathf.PI) * length * 0.12f;
                    if (i > 0 && i < tetherPoints.Length - 1) point += Random.insideUnitSphere * 0.55f;
                    tetherPoints[i] = point;
                }
                tether.positionCount = tetherPoints.Length;
                tether.SetPositions(tetherPoints);
            }
        }
    }
}
