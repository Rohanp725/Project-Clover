using Clover.Core;
using Clover.World;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// An ordinary enemy in the world. The deciding is done by <see cref="EnemyBrain"/> and the numbers come from
    /// <see cref="EnemyKind"/>; this moves the body, shows each attack on the ground before it lands, deals the blow,
    /// takes blows (health, stagger, burning, the finisher), and dies. Its lightning infection can be fed: a bolt from
    /// the storm charges it for a while, making it faster and harder-hitting.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Enemy : MonoBehaviour, IDamageable, IStaggerable, IBurnable, IAfflictable, IShovable, IBoss, IWeighted, IFlinchable
    {
        [SerializeField] private string kindName = EnemyKind.SoldierName;
        [SerializeField] private Transform target;
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer[] bodyRenderers = new Renderer[0];
        [SerializeField] private MeshFilter markerOutline;
        [SerializeField] private MeshFilter markerFill;
        [SerializeField] private Transform bars;
        [SerializeField] private Transform healthFill;
        [SerializeField] private Transform staggerFill;
        [SerializeField] private SlashArcs arcs;
        [SerializeField] private EnemyTurns turns;
        [SerializeField] private GuardArc guardArc;
        [SerializeField] private StaggerMark staggerMark;
        // The ground it keeps, as the least and greatest x and z of a rectangle; unused unless it holds one.
        [SerializeField] private bool holdsGround;
        [SerializeField] private Vector2 groundLeast;
        [SerializeField] private Vector2 groundMost;
        // Placed dormant: it does not notice anyone until they come this near or strike it, and then takes a moment to rise.
        [SerializeField] private bool dormant;
        [SerializeField] private float wakeDistance = 3.5f;
        [SerializeField] private string dormantClip = "";

        public float StaggeredDamageMultiplier = 1.5f;
        public float FinisherDamageMultiplier = 3f;
        /// <summary>Metres a blow shoves the enemy back.</summary>
        public float Shove = 0.7f;
        public float Gravity = 25f;
        /// <summary>Seconds after dying before the body is cleared away.</summary>
        public float CorpseSeconds = 2.2f;
        /// <summary>Seconds after dying, once it has fallen, before the body starts to burn away; it is gone by <see cref="CorpseSeconds"/>.</summary>
        public float BurnAwayAt = 0.95f;
        /// <summary>What the body turns to as the lightning leaves it: ink.</summary>
        public Color BurntColour = new Color(0.02f, 0.015f, 0.04f, 1f);
        /// <summary>True when something else draws the ground its planted circle leaves burning (SpearLook's fire), so it throws no puffs of its own there.</summary>
        [System.NonSerialized] public bool BurnDrawn;
        /// <summary>While charged by a bolt: how much faster it moves and harder it hits, and for how long.</summary>
        public float ChargedSpeed = 1.3f;
        public float ChargedDamage = 1.3f;
        public float ChargedSeconds = 9f;
        public Color PlainMarker = new Color(1f, 0.93f, 0.75f, 1f);
        public Color RedMarker = new Color(1f, 0.08f, 0.05f, 1f);
        public float OutlineAlpha = MarkerShapes.OutlineAlpha;
        public float FillAlpha = MarkerShapes.GrowingAlpha;
        [ColorUsage(true, true)] public Color SlashColour = new Color(1.6f, 0.45f, 1.5f, 0.6f);
        /// <summary>Half the width of the player, in metres: a blow that reaches their edge has reached them.</summary>
        public float TargetRadius = 0.4f;
        /// <summary>Metres short of the marked spot a leaping attacker comes down, so it lands in front of the target and not inside it.</summary>
        public float LeapShort = 1.5f;
        /// <summary>Seconds its bars stay up after a blow when it has lost sight of the fight.</summary>
        public float BarsLingerSeconds = 4f;
        /// <summary>Metres from the player beyond which its bars are put away: they are drawn over walls, and far off they say nothing.</summary>
        public float BarsDistance = 16f;
        /// <summary>How far through the telegraph a leap leaves the ground.</summary>
        public float LeapFrom = 0.62f;

        public EnemyKind Kind { get; private set; }
        public EnemyBrain Brain => Kind.Brain;
        /// <summary>Where a ranged or leaping circle was aimed: for the look of the blow.</summary>
        public Vector3 StrikePoint => strikePoint;
        /// <summary>Where a passing lunge began and which way it went: for the look of the blow.</summary>
        public Vector3 LungeFrom => lungeFrom;
        public Vector3 LungeForward => lungeForward;
        public Health Health { get; } = new Health();
        public StaggerMeter Stagger { get; } = new StaggerMeter();
        /// <summary>Everything the elements have laid on it. The burn is one of them.</summary>
        public AfflictionSet Afflictions { get; } = new AfflictionSet();
        public BurnStatus Burn => Afflictions.Burn;
        /// <summary>What it glows while frozen: it stands still in ice until it thaws or a heavy blow breaks it.</summary>
        public Color FrozenGlow = new Color(0.22f, 0.5f, 0.85f);
        /// <summary>The white flash of a plain blow landing.</summary>
        public static readonly Color StruckFlash = new Color(1.6f, 1.6f, 1.6f);
        /// <summary>The flash a landed blow gives the body: white for a plain one, the element's colour softened under one (<see cref="StruckLook"/>).</summary>
        public static Color FlashColour(Color tint) => StruckLook.Flash(StruckFlash, tint);
        public bool IsStaggered => Brain.State == EnemyState.Staggered;
        public bool IsDead => Brain.State == EnemyState.Dead;
        public bool IsCharged => chargedLeft > 0f;
        public bool IsAware => Brain.State != EnemyState.Idle && Brain.State != EnemyState.Dead;
        public bool InFight => IsAware && !IsDead;
        public float HealthFraction => Health.Fraction;
        public float BalanceFraction => IsStaggered ? 1f : Stagger.Fraction;
        public int BlowsStruck { get; private set; }
        public int BlowsGuarded { get; private set; }
        /// <summary>True while it is showing or striking an attack: this is what holds the turn.</summary>
        public bool IsStriking => Brain.State == EnemyState.Telegraph || Brain.State == EnemyState.Strike || busy == BusyHold;
        /// <summary>True while it lies or stands dormant, before anyone has woken it.</summary>
        public bool IsDormant => asleep;
        public bool IsRising => busy == BusyRise;
        /// <summary>True while it has hold of the player.</summary>
        public bool IsHolding => busy == BusyHold;
        /// <summary>True while it stands drinking a flask it took; helpless until it is done.</summary>
        public bool IsDrinking => busy == BusyDrink;
        public bool HoldsFlask => Snatch.Holding;
        /// <summary>For one that hunts by light: whether it knows where the player is. Always true for the sighted.</summary>
        public bool HasFound => !Kind.HuntsByLight || !Sunlight.InUse || Sense.Found;
        public bool IsSearching => searching;
        /// <summary>Whether it would answer a drink begun now: near, aware, and not in the middle of anything.</summary>
        public bool CanAnswerDrink => Kind != null && Kind.SnatchAttack.Length > 0 && !asleep && target != null
            && (Brain.State == EnemyState.Stalk || Brain.State == EnemyState.Recover)
            && (target.position - transform.position).sqrMagnitude <= Kind.SnatchRange * Kind.SnatchRange;
        public SunSense Sense { get; } = new SunSense();
        public Clover.Player.Grip Grip { get; } = new Clover.Player.Grip();
        public FlaskSnatch Snatch { get; } = new FlaskSnatch();
        public int FlasksTaken { get; private set; }
        public int FlasksDrunk { get; private set; }
        public int Holds { get; private set; }

        private const int BusyNone = 0, BusyDrink = 1, BusyHold = 2, BusyThrown = 3, BusyRise = 4, BusySip = 5;
        private int busy;
        private bool asleep;
        private bool searching;
        private float searchedFor;
        private Vector3 lastKnown;
        private float basePreferred;
        private Clover.Player.PlayerController targetController;
        private Clover.Player.PlayerDefence targetDefence;
        private int drinksSeen;
        private int drinkHash, holdHash, thrownHash, riseHash, dormantHash, searchHash;
        // Knocked flat (task 3.7): the fall, the lying breath and getting up; whether the animator has them is learned
        // the first time it goes down (0 not yet, 1 it has, -1 baked before they were made).
        private int knockdownHash, downLoopHash, getUpHash, downClips;
        private bool downDust;
        private Vector3 burnAt;
        private float burnLeft, burnRadius, burnDamage, burnEvery, untilBurnBite, untilBurnShow, untilHoldEmber;
        private static bool breakFreeSaid;

        // Domain reload is off: the prompt is said once in a sitting, and a new play is a new sitting.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => breakFreeSaid = false;

        /// <summary>
        /// Places it dormant: it notices no one until they come within <paramref name="wakeWithin"/> metres or strike
        /// it. <paramref name="clip"/> is the animation it holds meanwhile (lying, kneeling); its own dormant loop when empty.
        /// </summary>
        public void MakeDormant(float wakeWithin, string clip = "")
        {
            dormant = true;
            wakeDistance = wakeWithin;
            dormantClip = clip;
        }
        public bool IsRanged => Brain.RetreatRange > 0f;
        /// <summary>Raised once when it dies.</summary>
        public event System.Action<Enemy> Died;
        /// <summary>Raised the instant one of its own attacks lands its blow, whether or not it reaches anyone.</summary>
        public event System.Action<Enemy, EnemyAttack> AttackStruck;
        /// <summary>Raised after it has taken a blow: the blow, and what came of it.</summary>
        public event System.Action<Enemy, Hit, HitOutcome> Struck;
        /// <summary>Raised when it wakes from lying dormant.</summary>
        public event System.Action<Enemy> Woke;
        /// <summary>Raised when it is put back where it began, alive (a rest, a fall).</summary>
        public event System.Action<Enemy> Restored;

        /// <summary>A multiplier on how fast it moves and acts, set from outside (the noon's swell, cooling gold underfoot). One leaves it as it is.</summary>
        public float SpeedScale => SwellPace * FlamePace * GroundPace;
        /// <summary>The noon's swell feeding the corruption; one outside a swell.</summary>
        [System.NonSerialized] public float SwellPace = 1f;
        /// <summary>A wick set alight moves faster; one otherwise.</summary>
        [System.NonSerialized] public float FlamePace = 1f;
        /// <summary>The ground underfoot (cooling gold); one on plain ground.</summary>
        [System.NonSerialized] public float GroundPace = 1f;
        /// <summary>Seconds left during which its guard is down whatever it is doing (a broken shield wall).</summary>
        [System.NonSerialized] public float GuardOpenFor;
        // Somewhere it has been sent by something other than the fight (water to drink, its place in a line).
        private bool steered;
        private Vector3 steerTo;
        private float steerSpeed;
        private bool steerFacesTarget;

        /// <summary>The one it fights.</summary>
        public Transform Target => target;

        /// <summary>
        /// Sends it walking to a point instead of at the player, while it is not attacking; it faces where it walks.
        /// Lasts until <see cref="ClearSteer"/>. A speed of zero uses its own walking speed.
        /// </summary>
        public void Steer(Vector3 to, float speed = 0f, bool faceTarget = false)
        {
            steered = true;
            steerTo = to;
            steerSpeed = speed;
            steerFacesTarget = faceTarget;
        }

        public void ClearSteer() => steered = false;
        public bool IsSteered => steered;

        /// <summary>Kills it outright, as its own fire does.</summary>
        public void Kill()
        {
            if (IsDead) return;
            Health.Damage(Health.Current + 1f);
            Die();
        }

        /// <summary>Wakes it if it lies dormant, as fire or a burst nearby does.</summary>
        public void WakeNow()
        {
            if (asleep) Wake();
            else if (!IsDead) Brain.Alert();
        }

        /// <summary>Gives it health back, as a drink does; <paramref name="swell"/> also raises its most by that share.</summary>
        public void Heal(float amount, float swell = 0f)
        {
            if (IsDead) return;
            if (swell > 0f) Health.Maximum *= 1f + swell;
            Health.Heal(amount);
        }

        /// <summary>
        /// Lets it stand sipping for a while, helpless, as a drink taken from the player does. False if it could not stop now.
        /// </summary>
        public bool Sip(float seconds)
        {
            if (IsDead || asleep || !Brain.Occupy(seconds)) return false;
            busy = BusySip;
            return true;
        }

        public bool IsSipping => busy == BusySip;
        /// <summary>Raised when the fight enters its second phase.</summary>
        public event System.Action<Enemy> PhaseChanged;

        private static readonly int MarkerColour = Shader.PropertyToID("_Color");
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int BaseColour = Shader.PropertyToID("_BaseColor");
        private static readonly int RimColour = Shader.PropertyToID("_RimColor");
        private static readonly int Solidity = Shader.PropertyToID("_Fade");

        private CharacterController body;
        private Renderer outlineRenderer;
        private Renderer fillRenderer;
        private MaterialPropertyBlock block;
        private Mesh[] shapes;
        private int locomotionHash, hurtHash, staggerHash, deathHash;
        private int[] attackHashes;
        private EnemyAttack shownAttack;
        private Vector3 strikePoint;
        private Vector3 lungeFrom;
        private Vector3 lungeForward = Vector3.forward;
        private bool lunging;
        private Vector3 leapTo;
        private Vector3 shove;
        private Vector3 home;
        // The model's place under the body, kept so a shaken body is put back exactly (Tremor's shake).
        private Vector3 modelRest;
        private bool shaking;

        /// <summary>How far a body carrying Tremor is shaken, metres, and how fast (the owner's review, 2026-11-19: the enemy is seen shaken).</summary>
        public const float TremorShake = 0.045f, TremorRate = 31f;
        private Quaternion homeRotation;
        private float fallSpeed;
        private bool inAir;
        private float airGround;
        /// <summary>Stagger taken on coming down from being thrown into the air.</summary>
        public float LandingStagger = 20f;
        private float flashLeft;
        private Color flashColour = StruckFlash;
        private float chargedLeft;
        private float deadFor;
        private float untilEmber;
        private int shownGlow = -1;
        private bool wasBusy;
        private bool markerShown;
        private bool passing;
        private Collider targetBody;
        private int lightHits;
        private float sinceLightHit;
        private int blocked;
        private float circle = 1f;
        private float untilTurnCircle;
        private Quaternion barsRotation;
        // The bars have been called up by a blow taken or an attack begun, and how long ago it was last hurt.
        private bool barsCalled;
        private float sinceHurt = float.MaxValue;
        // The body's own colours, one row for each renderer and one entry for each of its materials, and the ones bright enough to make a flake of.
        private Color[][] ownColours;
        private Color[] flakeColours;
        private int flakes;
        private bool burning;
        private float untilFlake;
        private Transform emberTaker;
        private int embersOwed;
        // A kept body: the pose it is held in (0 for none) and how far through it, and whether it is lit from within.
        private int heldPose;
        private float heldAt;
        private bool shining;

        /// <summary>
        /// A god's body is kept when he falls (WORLD.md, "Every god transforms"): it is not burnt away or cleared, the
        /// Shards it owed fly at once, and it holds whatever pose its keeper gives it with <see cref="HoldPose"/>.
        /// </summary>
        [System.NonSerialized] public bool KeepsBody;

        /// <summary>Holds a kept body in this clip, this far through it (0 to 1). An empty name lets it fall as the dead fall.</summary>
        public void HoldPose(string clip, float at)
        {
            heldPose = string.IsNullOrEmpty(clip) ? 0 : Animator.StringToHash(clip);
            heldAt = Mathf.Clamp(at, 0f, 0.999f);
        }

        /// <summary>Lights the body from within with this glow; black puts it back to its own colours.</summary>
        public void Shine(Color glow)
        {
            shining = glow.maxColorComponent > 0.001f;
            shownGlow = -1;
            foreach (var bodyRenderer in bodyRenderers)
            {
                if (bodyRenderer == null) continue;
                if (!shining)
                {
                    bodyRenderer.SetPropertyBlock(null);
                    continue;
                }
                block.Clear();
                block.SetColor(Emission, glow);
                bodyRenderer.SetPropertyBlock(block);
            }
        }

        /// <summary>True while it keeps to a ground of its own and the target is standing on it, or it keeps none.</summary>
        public bool TargetOnGround => !holdsGround || target == null || OffGround(target.position) <= 0f;

        /// <summary>
        /// Gives it a ground to keep: a rectangle on the map. It notices only a target standing on that ground, does
        /// not walk off it, lets a target that leaves go after a moment, and then walks back to where it stood.
        /// </summary>
        public void HoldGround(float leastX, float mostX, float leastZ, float mostZ)
        {
            holdsGround = true;
            groundLeast = new Vector2(leastX, leastZ);
            groundMost = new Vector2(mostX, mostZ);
        }

        // Metres a point lies outside the ground; zero anywhere on it.
        private float OffGround(Vector3 point) =>
            Mathf.Max(0f, Mathf.Max(Mathf.Max(groundLeast.x - point.x, point.x - groundMost.x), Mathf.Max(groundLeast.y - point.z, point.z - groundMost.y)));

        /// <summary>Adds the mark that shows over it while it is staggered.</summary>
        public void ConfigureFeedback(StaggerMark markToShow) => staggerMark = markToShow;

        /// <summary>
        /// Embers have just been paid for this death. Nothing is paid here: the body only remembers to send that many
        /// motes to the one who earned them when it burns away, and tells the HUD they are still on their way.
        /// </summary>
        public void CarryEmbers(Transform to, int count)
        {
            if (to == null || count <= 0) return;
            emberTaker = to;
            embersOwed += count;
            EmberMotes.Promise(count);
        }

        public void Configure(string kind, Transform targetToFight, Animator animatorToDrive, Renderer[] renderers,
            MeshFilter outline, MeshFilter fill, Transform barsRoot, Transform healthBar, Transform staggerBar, SlashArcs arcsToShow, EnemyTurns turnsToTake, GuardArc arcOfItsGuard)
        {
            kindName = kind;
            target = targetToFight;
            animator = animatorToDrive;
            bodyRenderers = renderers;
            markerOutline = outline;
            markerFill = fill;
            bars = barsRoot;
            healthFill = healthBar;
            staggerFill = staggerBar;
            arcs = arcsToShow;
            turns = turnsToTake;
            guardArc = arcOfItsGuard;
        }

        // Shaken while it carries Tremor: the model only, side to side and a little up and down, never the body that
        // moves and is struck, so nothing it does changes. Deeper Tremor shakes it harder.
        private void LateUpdate()
        {
            if (animator == null || animator.transform == transform) return;
            int tremor = IsDead ? 0 : Afflictions.Stacks(AfflictionKind.Tremor);
            if (tremor <= 0)
            {
                if (shaking) animator.transform.localPosition = modelRest;
                shaking = false;
                return;
            }
            if (!shaking) modelRest = animator.transform.localPosition;
            shaking = true;
            float t = Time.time * TremorRate, size = TremorShake * (1f + 0.25f * (tremor - 1));
            animator.transform.localPosition = modelRest + new Vector3(Mathf.Sin(t) * size, Mathf.Abs(Mathf.Sin(t * 1.7f + 1f)) * size * 0.5f, Mathf.Sin(t * 1.3f + 2f) * size * 0.6f);
        }

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            block = new MaterialPropertyBlock();
            Kind = EnemyKind.ByName(kindName);
            home = transform.position;
            targetBody = target != null ? target.GetComponent<Collider>() : null;
            targetController = target != null ? target.GetComponent<Clover.Player.PlayerController>() : null;
            targetDefence = target != null ? target.GetComponent<Clover.Player.PlayerDefence>() : null;
            basePreferred = Brain.PreferredRange;
            // The brain reads the conditions on this body once a step.
            Brain.Conditions = Afflictions.Conditions;
            Grip.Seconds = Kind.HoldSeconds;
            Grip.BiteEvery = Kind.HoldBiteEvery;
            Grip.BiteDamage = Kind.HoldBiteDamage;
            Grip.PressTakes = Kind.HoldPressTakes;
            Snatch.DrinkSeconds = Kind.DrinkSeconds;
            drinkHash = Animator.StringToHash(Kind.Prefix + "_drink");
            holdHash = Animator.StringToHash(Kind.Prefix + "_hold_loop");
            thrownHash = Animator.StringToHash(Kind.Prefix + "_thrown_off");
            riseHash = Animator.StringToHash(Kind.Prefix + (dormantClip != null && dormantClip.EndsWith("_lie_loop") ? "_rise_lying" : "_rise"));
            dormantHash = Animator.StringToHash(string.IsNullOrEmpty(dormantClip) ? Kind.Prefix + "_dormant_loop" : dormantClip);
            searchHash = Animator.StringToHash(Kind.Prefix + "_search_loop");
            knockdownHash = Animator.StringToHash(Kind.Prefix + "_knockdown");
            downLoopHash = Animator.StringToHash(Kind.Prefix + "_down_loop");
            getUpHash = Animator.StringToHash(Kind.Prefix + "_getup");
            // The Fire land's blows are drawn in its own two colours, not the storm's violet.
            if (Kind.Ashen) SlashColour = new Color(1.9f, 0.95f, 0.3f, 0.6f);
            if (bars != null)
            {
                barsRotation = bars.rotation;
                var shown = bars.GetComponent<EnemyBars>();
                if (shown != null) shown.Watch(Afflictions);
            }
            homeRotation = transform.rotation;
            // The great are held by ice only for a breath.
            if (Kind.IsBoss || Kind.IsElite) Afflictions.FreezeScale = 0.4f;
            // A god in this body ranks as a god, not by how briefly ice holds it.
            Afflictions.IsGod = Kind.IsGod;

            locomotionHash = Animator.StringToHash("locomotion");
            hurtHash = Animator.StringToHash(Kind.Prefix + "_hurt");
            staggerHash = Animator.StringToHash(Kind.Prefix + "_stagger");
            deathHash = Animator.StringToHash(Kind.Prefix + "_death");
            attackHashes = new int[Brain.Attacks.Count];
            shapes = new Mesh[Brain.Attacks.Count];
            for (int i = 0; i < attackHashes.Length; i++)
            {
                var attack = Brain.Attacks[i];
                // Each attack has a clip of its own name, even when it shares a motion with another.
                attackHashes[i] = Animator.StringToHash(attack.Name);
                shapes[i] = attack.IsCircle ? MarkerShapes.Sector(attack.Radius, 360f)
                    : attack.IsLine ? MarkerShapes.Lane(attack.Reach, attack.LineWidth) : MarkerShapes.Sector(attack.Reach, attack.ArcDegrees);
            }
            if (markerOutline != null) outlineRenderer = markerOutline.GetComponent<Renderer>();
            if (markerFill != null) fillRenderer = markerFill.GetComponent<Renderer>();
            ReadOwnColours();
            Restore();
        }

        private void OnDestroy()
        {
            if (shapes == null) return;
            foreach (var shape in shapes)
                if (shape != null) Destroy(shape);
        }

        // Read once, so the burning body can go from its own colours to ink and break into flakes of them.
        private void ReadOwnColours()
        {
            ownColours = new Color[bodyRenderers.Length][];
            int bright = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                if (pass == 1) flakeColours = new Color[Mathf.Max(1, bright)];
                int at = 0;
                for (int i = 0; i < bodyRenderers.Length; i++)
                {
                    var materials = bodyRenderers[i] != null ? bodyRenderers[i].sharedMaterials : new Material[0];
                    if (pass == 0) ownColours[i] = new Color[materials.Length];
                    for (int m = 0; m < materials.Length; m++)
                    {
                        Color colour = materials[m] != null && materials[m].HasProperty(BaseColour) ? materials[m].GetColor(BaseColour) : Color.grey;
                        // A part that is not drawn (gold a lower rank has not been given) stays undrawn as the body burns: marked by an alpha below zero.
                        bool undrawn = materials[m] != null && materials[m].HasProperty(Solidity) && materials[m].GetFloat(Solidity) < 0.01f;
                        if (undrawn) colour.a = -1f;
                        if (pass == 0) ownColours[i][m] = colour;
                        if (undrawn) continue;
                        // Cloth, plate and skin make flakes that can be seen; the near-black parts would make none.
                        if (colour.maxColorComponent < 0.3f) continue;
                        if (pass == 0) bright++;
                        else flakeColours[at++] = colour;
                    }
                }
            }
            if (bright == 0) flakeColours[0] = new Color(0.9f, 0.5f, 0.3f);
        }

        /// <summary>Puts the enemy back where it began, alive and unaware, as when the player rests or falls.</summary>
        public void Restore()
        {
            gameObject.SetActive(true);
            if (burning) Unburn();
            if (embersOwed > 0)
            {
                // Put back before it burned: the motes will never fly, so the HUD stops waiting for them.
                EmberMotes.Withdraw(embersOwed);
                embersOwed = 0;
            }
            if (staggerMark != null) staggerMark.Show(false);
            heldPose = 0;
            if (shining) Shine(Color.black);
            body.enabled = false;
            transform.SetPositionAndRotation(home, homeRotation);
            body.enabled = true;
            Health.Maximum = Kind.Health;
            Health.Fill();
            Stagger.Maximum = Kind.StaggerMaximum;
            Stagger.StaggeredSeconds = Kind.StaggeredSeconds;
            Stagger.Reset();
            Afflictions.Clear();
            Brain.Reset();
            if (busy == BusyHold) Grip.Release();
            Snatch.Drop();
            busy = BusyNone;
            asleep = dormant;
            searching = false;
            searchedFor = 0f;
            steered = false;
            GuardOpenFor = 0f;
            FlamePace = 1f;
            GroundPace = 1f;
            Sense.Reset();
            lastKnown = home;
            burnLeft = 0f;
            drinksSeen = targetDefence != null ? targetDefence.DrinksBegun : 0;
            shove = Vector3.zero;
            fallSpeed = 0f;
            chargedLeft = 0f;
            deadFor = 0f;
            flashLeft = 0f;
            shownAttack = null;
            lunging = false;
            SetPassing(false);
            lightHits = 0;
            blocked = 0;
            ShowMarker(false, false);
            SetGlow(0);
            barsCalled = false;
            sinceHurt = float.MaxValue;
            if (bars != null) bars.gameObject.SetActive(false);
            if (guardArc != null) guardArc.Show(false);
            // The animator finds its own way back: the next frame it is not busy, it blends to standing.
            wasBusy = true;
            Restored?.Invoke(this);
        }

        /// <summary>A bolt from the storm feeds the infection: faster and harder-hitting for a while.</summary>
        public void Charge()
        {
            if (IsDead) return;
            chargedLeft = ChargedSeconds;
            Brain.Alert();
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            if (IsDead) return HitOutcome.Missed;
            var outcome = TakeHitInner(hit);
            Struck?.Invoke(this, hit, outcome);
            return outcome;
        }

        private HitOutcome TakeHitInner(in Hit hit)
        {
            // A blow that only sets a burn off has nothing to do to an enemy that is not burning.
            if (hit.Detonate && hit.Damage <= 0f && !Burn.IsBurning) return HitOutcome.Missed;
            // Struck by someone standing off its ground: the ground stops holding it for a while.
            // Struck before it has risen, or by someone a blind hunter has not found: the blow lands the harder for it.
            float harder = 1f;
            if (asleep || busy == BusyRise) harder = Kind.DormantDamage;
            if (Kind.HuntsByLight && Sunlight.InUse && !Sense.Found) harder *= Kind.UnfoundDamage;
            if (asleep) Wake();
            if (Kind.HuntsByLight) Sense.Fight(0f);
            if (holdsGround && !TargetOnGround) Brain.Provoke();
            else Brain.Alert();
            barsCalled = true;
            sinceHurt = 0f;
            // Shown at once when near, so the blow and the bar it shortens are seen together; the rest is decided each frame.
            if (bars != null && target != null && (target.position - transform.position).sqrMagnitude <= BarsDistance * BarsDistance) bars.gameObject.SetActive(true);
            float detonated = 0f;
            if (hit.Detonate && Burn.IsBurning)
            {
                detonated = Burn.Detonate();
                Vfx.Play(VfxKind.Burst, transform.position + Vector3.up * 1.2f);
                Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 1.2f, Vector3.up, 2f);
                Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.4f, Vector3.up, 2f);
            }
            Afflictions.IgniteFrom(hit, this);
            // Ice breaks under a heavy blow, and what it held takes the breaking.
            if (hit.Heavy && Afflictions.IsFrozen)
            {
                detonated += Afflictions.Shatter();
                CombatEvents.RaiseShattered(this);
                Vfx.Play(VfxKind.Burst, transform.position + Vector3.up * 1.2f);
                Vfx.Play(VfxKind.TintSpark, transform.position + Vector3.up * 1.2f, Vector3.up, 2.5f, Gameplay.Afflictions.Spec(AfflictionKind.Chill).Colour);
                Vfx.Play(VfxKind.TintRing, transform.position + Vector3.up * 0.3f, Vector3.up, 1f, Gameplay.Afflictions.Spec(AfflictionKind.Chill).Colour);
            }
            // Read before this blow lays anything of its own: the blow is judged against what was already there.
            float takes = Afflictions.DamageTakenMultiplier(hit.Spell) * harder;
            float unsteadies = Afflictions.StaggerTakenMultiplier * harder;
            float shoved = Afflictions.ShoveMultiplier;
            Afflictions.TakeFrom(hit, this);
            // Ground that only lays an affliction is not a blow: it does not shove, flinch, wear poise or meet the guard.
            if (hit.OnlyLays && detonated <= 0f) return HitOutcome.Landed;

            // Its guard: a light blow from the front, while it stands ready, is turned on its weapon.
            if (Kind.Guards && GuardOpenFor <= 0f && !Afflictions.Conditions.Has(ConditionFlags.CannotGuard) && (!hit.Heavy || Kind.HeavyGuardedDamage < 1f) && !hit.Unblockable && !IsStaggered && (Brain.State == EnemyState.Stalk || Brain.State == EnemyState.Idle)
                && Vector3.Angle(transform.forward, -hit.Direction) < 75f)
            {
                BlowsGuarded++;
                if (guardArc != null) guardArc.Pulse(new Color(3f, 3.4f, 4f, 1f), 0.18f, 0.25f);
                Health.Damage(hit.Damage * takes * (hit.Heavy ? Kind.HeavyGuardedDamage : Kind.GuardedDamage));
                Vfx.Play(VfxKind.GuardSpark, transform.position + Vector3.up * 1.2f + transform.forward * 0.5f, transform.forward);
                Vector3 back = hit.Direction;
                back.y = 0f;
                shove = back.normalized * (Shove * 0.5f);
                if (Health.IsDead)
                {
                    Die();
                    return HitOutcome.Landed;
                }
                // A heavy blow met on the guard still costs it all the balance the blow carries.
                if (Stagger.Add((hit.Heavy ? hit.Stagger : hit.Stagger * Kind.GuardedStagger) * unsteadies))
                {
                    Brain.Stagger();
                    Clover.Player.Tips.EnemyStaggered(transform.position);
                    Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 1.4f, Vector3.up);
                }
                else if (++blocked >= Kind.BlocksBeforeCounter)
                {
                    blocked = 0;
                    Brain.Counter();
                }
                return HitOutcome.Blocked;
            }

            bool finisher = IsStaggered && hit.Heavy;
            // Drinking, it is as helpless as if its balance were broken.
            float multiplier = finisher ? FinisherDamageMultiplier : IsStaggered || busy == BusyDrink ? StaggeredDamageMultiplier : 1f;
            // A god is not cut down by one blow, however good: a finisher takes a fixed share of him.
            Health.Damage(detonated + (finisher && Kind.FinisherHealthShare > 0f ? Mathf.Max(hit.Damage * multiplier * takes, Kind.Health * Kind.FinisherHealthShare) : hit.Damage * multiplier * takes));
            Flash(FlashColour(hit.Tint));
            Vector3 away = hit.Direction;
            away.y = 0f;
            // Flat on its back it is not slid along the floor by the blows that follow the one that felled it.
            shove = Brain.IsDown ? Vector3.zero : away.normalized * (Shove * shoved * (finisher ? 2.2f : hit.Heavy ? 1.6f : 1f));

            if (Health.IsDead)
            {
                Die();
                return finisher ? HitOutcome.Finisher : HitOutcome.Landed;
            }
            if (finisher)
            {
                Stagger.Reset();
                Brain.EndStagger();
                Brain.Flinch(true);
                return HitOutcome.Finisher;
            }
            if (!IsStaggered && Stagger.Add(hit.Stagger * unsteadies))
            {
                Brain.Stagger();
                Clover.Player.Tips.EnemyStaggered(transform.position);
                Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 1.4f, Vector3.up);
            }
            else
            {
                // Poise: light blows are shrugged off until enough have landed close together.
                sinceLightHit = 0f;
                if (hit.Heavy || ++lightHits >= Kind.PoiseHits)
                {
                    lightHits = 0;
                    Brain.Flinch(hit.Heavy && !Kind.Steadfast);
                }
            }
            return HitOutcome.Landed;
        }

        private void Die()
        {
            Brain.Die();
            BusyBroken();
            burnLeft = 0f;
            if (guardArc != null) guardArc.Show(false);
            deadFor = 0f;
            ShowMarker(false, false);
            if (bars != null) bars.gameObject.SetActive(false);
            if (staggerMark != null) staggerMark.Show(false);
            if (Kind.Ashen)
            {
                Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.2f, Vector3.up, 1.5f);
                Vfx.Play(VfxKind.Dust, transform.position, Vector3.up);
            }
            else Vfx.Play(VfxKind.Shock, transform.position + Vector3.up, Vector3.up, 1.5f);
            // The body no longer stands in anyone's way.
            SetPassing(false);
            body.enabled = false;
            CombatEvents.RaiseDied(this, Afflictions);
            Afflictions.Clear();
            Died?.Invoke(this);
        }

        public float Weight => Kind != null ? Kind.Weight : 1f;

        /// <summary>A pull or a push from outside a blow. The great are moved less.</summary>
        public void Push(Vector3 impulse)
        {
            if (IsDead) return;
            impulse.y = 0f;
            shove += impulse * (Kind.IsBoss ? 0.3f : Kind.IsElite ? 0.6f : 1f);
        }

        /// <summary>Times an effect (a reaction) has made it flinch, for probes.</summary>
        public int EffectFlinches { get; private set; }

        /// <summary>Made to flinch by an effect, not a blow: what it had begun is broken, unless it is steadfast.</summary>
        public bool Flinch(float seconds)
        {
            if (IsDead || asleep) return false;
            if (!Brain.Flinch(!Kind.Steadfast, seconds)) return false;
            EffectFlinches++;
            return true;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (IsDead)
            {
                deadFor += deltaTime;
                if (KeepsBody)
                {
                    // Kept, the body stays as its keeper poses it; what it owed is shown leaving it now, not at a burning that never comes.
                    for (int i = 0; i < embersOwed; i++) EmberMotes.Fly(transform.position + Vector3.up * 1.2f, emberTaker, i);
                    embersOwed = 0;
                    if (animator != null && heldPose != 0) animator.Play(heldPose, 0, heldAt);
                    else Animate(deltaTime, 0f);
                    if (!shining) Paint(deltaTime);
                    return;
                }
                Animate(deltaTime, 0f);
                if (deadFor >= BurnAwayAt) BurnAway(deltaTime);
                else Paint(deltaTime);
                if (deadFor >= CorpseSeconds) gameObject.SetActive(false);
                return;
            }
            if (target == null) return;
            // The enemies' own time can be made to drag while the player's does not.
            deltaTime *= WorldClock.EnemyScale;

            if (asleep)
            {
                // Dormant: it holds its pose and notices nothing until someone is near enough to wake it.
                Vector3 apart = target.position - transform.position;
                apart.y = 0f;
                if (apart.sqrMagnitude <= wakeDistance * wakeDistance) Wake();
                else
                {
                    if (animator != null) animator.Play(dormantHash, 0, Mathf.Repeat(Time.time / 3f, 1f));
                    wasBusy = true;
                    Paint(deltaTime);
                    return;
                }
            }
            // Whatever it was taken up with has ended, by its time or by a broken balance.
            if (busy != BusyNone && Brain.State != EnemyState.Busy) BusyBroken();
            if (busy == BusyDrink && Snatch.Tick(deltaTime))
            {
                // It finished the flask: the flask is gone and the thirsting is whole again.
                FlasksDrunk++;
                Health.Fill();
                busy = BusyNone;
                Brain.EndBusy();
                if (targetDefence != null) targetDefence.Callout("It drinks.");
            }
            if (busy == BusyHold) TickHold(deltaTime);
            if (burnLeft > 0f) TickBurn(deltaTime);

            var before = Brain.Current;
            bool wasStriking = IsStriking;
            if (chargedLeft > 0f) chargedLeft -= deltaTime;
            // Cold and shaken ground slow everything it does; frozen, it does nothing at all.
            if (GuardOpenFor > 0f) GuardOpenFor -= deltaTime;
            float pace = SpeedScale * (IsCharged ? ChargedSpeed : 1f) * (Brain.Phase > 0 ? Kind.SecondPhasePace : 1f) * Afflictions.SpeedMultiplier;
            // Knocked flat, the second phase waits for it to be on its feet.
            if (Brain.Phase == 0 && Kind.SecondPhaseAt > 0f && Health.Fraction <= Kind.SecondPhaseAt && !IsStaggered && !Brain.IsDown)
            {
                // Worn down to the mark: the second phase opens with its own attack.
                Brain.Phase = 1;
                Brain.Force(Kind.SecondPhaseAttack);
                // A god's second phase is staged by his own summit; anyone else is simply struck by the storm he carries.
                if (!Kind.IsBoss) LightningBolts.Strike(transform.position, 0.4f);
                PhaseChanged?.Invoke(this);
            }

            if (Afflictions.Runs)
            {
                bool wasBurning = Burn.IsBurning;
                Health.Damage(Afflictions.Tick(deltaTime));
                if (Afflictions.LandedThisTick && !IsDead)
                {
                    // Down hard: the landing shakes its balance.
                    TakeHit(new Hit(0f, LandingStagger, Vector3.down, false, true));
                    CombatEvents.RaiseLanded(this);
                }
                untilEmber -= deltaTime;
                // A flame gone live (Flashover) is all its reaction's violet: no plain fire off the body beside it.
                bool plainFire = wasBurning && !Afflictions.Conditions.Has(ConditionFlags.FlameLive);
                // The body burns in real flame (the owner's review, 2026-11-20); the old puffs only where it cannot.
                bool realFlame = plainFire && Clover.Player.Forms.BodyFlames.Keep(transform, Afflictions.Stacks(AfflictionKind.Burn));
                // What else it carries plays its second-pass look on it (Soaked's drop, Sundered's dagger, Chill's ice...).
                Clover.Player.Forms.AfflictionArt.KeepLooks(Afflictions, transform);
                if (plainFire && untilEmber <= 0f)
                {
                    untilEmber = 0.14f;
                    if (!realFlame) Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 1.2f, Vector3.up, 0.12f);
                    Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.4f, Vector3.up, 0.35f);
                }
                // The poison's bite is seen when it lands.
                if (Afflictions.BitThisTick)
                {
                    Vfx.Play(VfxKind.TintPuff, transform.position + Vector3.up * 1.2f, Vector3.up, 0.7f, Gameplay.Afflictions.Spec(AfflictionKind.Poison).Colour);
                    CombatEvents.RaisePoisonBit(this, Afflictions.BitDepth, Afflictions.BitDamage);
                }
                if (Health.IsDead)
                {
                    Die();
                    return;
                }
            }

            Stagger.Tick(deltaTime, Afflictions.BalanceRecovers);
            if (IsStaggered && !Stagger.IsStaggered) Brain.EndStagger();

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            float brainDistance = distance;
            bool found = true;
            searching = false;
            if (Kind.HuntsByLight && Sunlight.InUse)
            {
                // It cannot see: it knows where the player is only by the sun on them, their heat, or a fight.
                found = Sense.Tick(deltaTime, Sunlight.IsLit(target.position), distance);
                Brain.TargetLit = Sense.FoundLit;
                Brain.PreferredRange = Sense.FoundLit ? Kind.LitPreferredRange : basePreferred;
                Brain.RetreatRange = Sense.FoundLit ? Kind.LitRetreatRange : 0f;
                if (found)
                {
                    lastKnown = target.position;
                    searchedFor = 0f;
                }
                else
                {
                    // Lost: it goes to where they were last found, searches there a while, and gives up.
                    toTarget = lastKnown - transform.position;
                    toTarget.y = 0f;
                    distance = toTarget.magnitude;
                    searching = Brain.State == EnemyState.Stalk && distance < 0.7f;
                    if (searching) searchedFor += deltaTime;
                    brainDistance = Brain.State == EnemyState.Idle || searchedFor >= Kind.SearchSeconds ? float.MaxValue : Brain.LeashRange - 1f;
                }
            }
            else Brain.TargetLit = true;
            Brain.TargetFlasks = targetDefence != null ? targetDefence.Flasks.Count : 0;
            if (targetDefence != null && targetDefence.DrinksBegun != drinksSeen)
            {
                // A drink draws the nearest of the thirsting at once, whoever's turn it is.
                drinksSeen = targetDefence.DrinksBegun;
                if (CanAnswerDrink && IsAware && (turns == null || turns.AnswersDrink(this, drinksSeen, target.position))) Brain.Force(Kind.SnatchAttack);
            }
            sinceLightHit += deltaTime;
            if (sinceLightHit > 1.6f) lightHits = 0;
            bool mayAttack = turns == null || turns.MayAttack(this);
            bool struck = Brain.Tick(deltaTime * pace, brainDistance, Random.value, mayAttack, TargetOnGround);
            // Knocked flat, the back meets the floor near the end of the fall: dust there, once each fall.
            if (Brain.IsDown && !Brain.IsGettingUp)
            {
                if (!downDust && Brain.StateSeconds >= EnemyBrain.FallSeconds * 0.8f)
                {
                    downDust = true;
                    Vfx.Play(VfxKind.Dust, transform.position, Vector3.up);
                }
            }
            else downDust = false;
            if (!wasStriking && IsStriking && turns != null) turns.Began(this);
            var attack = Brain.Current;
            if (attack != null && attack != before && Brain.State == EnemyState.Telegraph)
            {
                // A thrown attack lands where the target stood when it was begun, so moving gets clear of it.
                // In the sun-court a shaft of sun cannot be aimed at someone the obelisk's shadow hides: it is laid at the shadow's edge.
                strikePoint = attack.Ranged ? Clover.World.Sunlight.OutOfHand(target.position, attack.Radius + 0.5f) : target.position;
                // A leap comes down just short of that spot, on the attacker's side of it.
                leapTo = distance > LeapShort ? strikePoint - toTarget / distance * LeapShort : transform.position;
                barsCalled = true;
                if (attack.BlinkTo > 0f) Blink(attack.BlinkTo);
                // The instant it commits: a white glint for a blow that can be guarded, a red flare for one that cannot.
                Vfx.Play(attack.Unblockable ? VfxKind.Danger : VfxKind.Glint, transform.position + Vector3.up * 2.3f);
            }

            if (bars != null)
            {
                // The bars are drawn over whatever stands in front of them, walls included, so they are only up for an
                // enemy that is in the fight or was hurt a moment ago, and never for one far off. An elite's name and
                // bars are up for as long as it is in the fight; anyone else's wait for a blow taken or an attack begun.
                if (sinceHurt < BarsLingerSeconds) sinceHurt += deltaTime;
                bool fighting = IsAware || sinceHurt < BarsLingerSeconds;
                if (!fighting) barsCalled = false;
                bool show = fighting && (Kind.IsElite || barsCalled) && distance <= BarsDistance;
                if (bars.gameObject.activeSelf != show) bars.gameObject.SetActive(show);
            }

            if (Brain.Tracks && distance > 0.05f && !searching && !(steered && Brain.State == EnemyState.Stalk))
            {
                var facing = Quaternion.LookRotation(toTarget, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, Kind.TurnDegreesPerSecond * pace * Afflictions.TurnMultiplier * deltaTime);
            }

            if (struck) Strike(attack);
            // It goes through the player only while showing or striking a blow that passes through, and at no other time.
            SetPassing(attack != null && attack.PassThrough && IsStriking);

            // Movement: walking as the brain asks, the drive forward of a lunge, and the shove of a blow taken.
            Vector3 step = Vector3.zero;
            float walked = 0f;
            if (Brain.MoveWish != 0 && distance > 0.05f && !searching)
            {
                walked = (Kind.LitWalkSpeed > 0f && Sense.FoundLit ? Kind.LitWalkSpeed : Kind.WalkSpeed) * pace * (Brain.MoveWish > 0 ? 1f : 0.75f);
                step = toTarget / distance * (Brain.MoveWish * walked * deltaTime);
            }
            if (Brain.IsWaiting && distance > 0.05f)
            {
                // Waiting its turn it does not stand still: it circles, changing direction now and then.
                untilTurnCircle -= deltaTime;
                if (untilTurnCircle <= 0f)
                {
                    untilTurnCircle = Random.Range(1.5f, 3.5f);
                    circle = Random.value < 0.5f ? -1f : 1f;
                }
                Vector3 sideways = Vector3.Cross(Vector3.up, toTarget / distance) * circle;
                step += sideways * (Kind.WalkSpeed * 0.45f * Afflictions.SpeedMultiplier * deltaTime);
                walked = Mathf.Max(walked, Kind.WalkSpeed * 0.45f * Afflictions.SpeedMultiplier);
            }
            if (steered && (Brain.State == EnemyState.Stalk || Brain.State == EnemyState.Idle || Brain.State == EnemyState.Recover))
            {
                // Sent somewhere by something other than the fight: it walks there and faces the way it walks.
                Vector3 toSpot = steerTo - transform.position;
                toSpot.y = 0f;
                float fromSpot = toSpot.magnitude;
                walked = 0f;
                step = Vector3.zero;
                if (fromSpot > 0.15f)
                {
                    walked = (steerSpeed > 0f ? steerSpeed : Kind.WalkSpeed) * pace;
                    step = toSpot / fromSpot * Mathf.Min(fromSpot, walked * deltaTime);
                    if (!steerFacesTarget) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toSpot, Vector3.up), Kind.TurnDegreesPerSecond * deltaTime);
                }
                // Kept in a line facing the enemy, it turns to the target while it steps.
                if (steerFacesTarget && distance > 0.05f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget, Vector3.up), Kind.TurnDegreesPerSecond * deltaTime);
            }
            if (holdsGround || (Kind.HuntsByLight && Brain.State == EnemyState.Idle))
            {
                if (Brain.State == EnemyState.Idle)
                {
                    // Nothing to fight: back to where it stands guard, and facing the way it faced.
                    Vector3 toHome = home - transform.position;
                    toHome.y = 0f;
                    float fromHome = toHome.magnitude;
                    if (fromHome > 0.3f)
                    {
                        walked = Kind.WalkSpeed * 0.8f;
                        step = toHome / fromHome * Mathf.Min(fromHome, walked * deltaTime);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toHome, Vector3.up), Kind.TurnDegreesPerSecond * deltaTime);
                    }
                    else transform.rotation = Quaternion.RotateTowards(transform.rotation, homeRotation, Kind.TurnDegreesPerSecond * deltaTime);
                }
                else if (holdsGround && !Brain.IsProvoked)
                {
                    // Its own feet never carry it farther off its ground; a lunge or a shove still may.
                    float off = OffGround(transform.position);
                    if (OffGround(transform.position + step) > off + 0.0001f)
                    {
                        step = Vector3.zero;
                        walked = 0f;
                    }
                }
            }
            if (attack != null && attack.Leap && Brain.State == EnemyState.Telegraph)
            {
                // The leap: off the ground late in the telegraph, and down beside the marked spot as the blow falls.
                float elapsed = Brain.AttackProgress * attack.Total;
                float left = attack.Telegraph - elapsed;
                if (elapsed >= attack.Telegraph * LeapFrom)
                {
                    Vector3 toLanding = leapTo - transform.position;
                    toLanding.y = 0f;
                    float share = left > 0.0001f ? Mathf.Clamp01(deltaTime * pace / left) : 1f;
                    step += toLanding * share;
                }
            }
            if (attack != null && attack.Lunge > 0f)
            {
                // The drive starts just before the blow and is spent by the time it lands.
                float from = attack.Telegraph * 0.78f, until = attack.Telegraph;
                float elapsed = Brain.AttackProgress * attack.Total;
                bool driving = elapsed >= from && elapsed < until;
                if (driving && !lunging)
                {
                    lungeFrom = transform.position;
                    lungeForward = transform.forward;
                }
                lunging = driving;
                if (driving) step += transform.forward * (attack.Lunge * deltaTime * pace / (until - from));
            }
            // Rooted, its feet go nowhere, by its own will, a lunge or a shove; it still turns and strikes.
            bool rooted = Afflictions.Conditions.Has(ConditionFlags.Rooted);
            if (rooted) step = Vector3.zero;
            // Ice holds it where it stands, whatever shoves.
            if (!Afflictions.IsFrozen && !rooted) step += shove * (deltaTime * 6f);
            shove = Vector3.Lerp(shove, Vector3.zero, 1f - Mathf.Exp(-9f * deltaTime));
            if (Afflictions.IsLifted)
            {
                // Thrown into the air: it goes nowhere of its own will, rises, hangs and comes down over where it left the ground.
                if (!inAir)
                {
                    inAir = true;
                    airGround = transform.position.y;
                }
                step = Vector3.zero;
                step.y = airGround + Afflictions.AirHeight - transform.position.y;
                fallSpeed = 0f;
            }
            else
            {
                inAir = false;
                fallSpeed = body.isGrounded ? 1f : fallSpeed + Gravity * deltaTime;
                step.y = -fallSpeed * deltaTime;
            }
            if (body.enabled) body.Move(step);

            // Its guard is up only while it stands ready and knows the player is there: the band going out is the opening.
            if (guardArc != null) guardArc.Show(Kind.Guards && Brain.State == EnemyState.Stalk && !Afflictions.Conditions.Has(ConditionFlags.CannotGuard));
            ShowAttack(attack);
            Animate(deltaTime, Kind.WalkSpeed > 0f ? walked / Kind.WalkSpeed : 0f);
            Paint(deltaTime);
            if (staggerMark != null) staggerMark.Show(IsStaggered);
            // The bars face the camera however the enemy turns.
            if (bars != null) bars.rotation = barsRotation;
            if (healthFill != null) healthFill.localScale = new Vector3(Health.Fraction, 1f, 1f);
            if (staggerFill != null) staggerFill.localScale = new Vector3(IsStaggered ? 1f : Stagger.Fraction, 1f, 1f);
        }

        private void Strike(EnemyAttack attack)
        {
            BlowsStruck++;
            AttackStruck?.Invoke(this, attack);
            if (IsDead) return;
            float damage = attack.Damage * (IsCharged ? ChargedDamage : 1f) * Afflictions.DamageDealtMultiplier;
            Vector3 forward = transform.forward;

            if (attack.IsCircle)
            {
                Vector3 centre = attack.Ranged || attack.Leap ? strikePoint : transform.position;
                if (attack.Fiery)
                {
                    // The sun brought down, or fire thrown up round a planted spear.
                    Vfx.Play(VfxKind.Burst, centre + Vector3.up * 0.6f, Vector3.up, 1.6f);
                    Vfx.Play(VfxKind.Fire, centre + Vector3.up * 0.4f, Vector3.up, 2.2f);
                    // A ring of fire: fiery blows are the Fire land's, whose scenes draw the ring as flame, not light.
                    Vfx.Play(VfxKind.Ring, centre + Vector3.up * 0.3f, Vector3.up, 1f);
                    if (attack.LightsSeconds > 0f) Sunlight.Flare(centre, attack.Radius, attack.LightsSeconds);
                    if (attack.BurnSeconds > 0f)
                    {
                        burnAt = centre;
                        burnRadius = attack.Radius;
                        burnLeft = attack.BurnSeconds;
                        burnDamage = attack.BurnDamage;
                        burnEvery = Mathf.Max(0.1f, attack.BurnEvery);
                        untilBurnBite = burnEvery;
                        untilBurnShow = 0f;
                    }
                }
                else if (attack.Ranged) LightningBolts.Strike(centre, 0.5f);
                else
                {
                    Vfx.Play(VfxKind.Shock, centre + Vector3.up, Vector3.up, 2.5f);
                    Vfx.Play(VfxKind.Ring, centre + Vector3.up * 0.3f, Vector3.up);
                }
                Vector3 offset = target.position - centre;
                offset.y = 0f;
                if (offset.magnitude > attack.Radius + 0.35f) return;
                if (!ReachesHeight(attack)) return;
                // A leaper's blow comes from the leaper, who has landed at the circle's edge: a guard must face him, not the middle of the circle.
                if (attack.Leap) Deal(attack, damage, forward, transform.position);
                else Deal(attack, damage, offset.sqrMagnitude > 0.01f ? offset.normalized : forward, centre);
                return;
            }

            if (arcs != null) arcs.Show(transform.position, forward, attack.Reach, attack.ArcDegrees, attack.Sweep, SlashColour);
            if (attack.IsLine)
            {
                // A lane: the player is struck if they stand within its width, anywhere along its length. A blow that
                // passes through is measured from where it started, since the attacker is already beyond.
                Vector3 start = attack.PassThrough ? lungeFrom : transform.position;
                Vector3 along = attack.PassThrough ? lungeForward : forward;
                Vector3 offset = target.position - start;
                offset.y = 0f;
                float ahead = Vector3.Dot(offset, along);
                float aside = (offset - along * ahead).magnitude;
                if (ahead >= -TargetRadius && ahead <= attack.Reach + TargetRadius && aside <= attack.LineWidth * 0.5f + TargetRadius && ReachesHeight(attack))
                    Deal(attack, damage, along, start);
                return;
            }

            // Enemies fight the player only, so the blow is measured against the player directly: inside the reach
            // (allowing for the player's own width) and inside the wedge, or close enough to be touching.
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float gap = Mathf.Max(0f, toTarget.magnitude - TargetRadius);
            if (gap > attack.Reach) return;
            if (gap > 0.6f && Vector3.Angle(forward, toTarget) > attack.ArcDegrees * 0.5f) return;
            if (!ReachesHeight(attack)) return;
            Deal(attack, damage, forward, transform.position);
        }

        /// <summary>
        /// A blow struck by hand does not reach a player standing well above or below the striker (on a roof, a deck, the
        /// floor under a ledge). Thrown and called blows, and a god's, reach as they always did.
        /// </summary>
        public bool ReachesHeight(EnemyAttack attack) =>
            attack.Ranged || attack.Casts || attack.Beams || Kind.IsBoss || target == null || HeightReach.Within(transform.position.y, target.position.y);

        private void SetPassing(bool pass)
        {
            if (pass == passing) return;
            passing = pass;
            if (targetBody != null && body != null) Physics.IgnoreCollision(body, targetBody, pass);
        }

        // Gone in a crack of light, and there again: to one side of the target or behind it, never in front.
        // Only to a place with ground under it and room to stand; with no such place it stays where it is.
        private void Blink(float distance)
        {
            Vector3 away = transform.position - target.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = -target.forward;
            float side = Random.value < 0.5f ? -1f : 1f;
            Vector3 to = transform.position;
            bool found = false;
            for (int attempt = 0; attempt < 6 && !found; attempt++)
            {
                // Each try is on the other side from the last, and the later ones come in closer.
                float turn = Random.Range(95f, 165f) * (attempt % 2 == 0 ? side : -side);
                float reach = distance * (attempt < 4 ? 1f : 0.6f);
                Vector3 spot = target.position + Quaternion.AngleAxis(turn, Vector3.up) * away.normalized * reach;
                spot.y = transform.position.y;
                if (!Physics.Raycast(spot + Vector3.up * 1.5f, Vector3.down, out RaycastHit ground, 3f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (Mathf.Abs(ground.point.y - transform.position.y) > 0.6f) continue;
                if (Physics.CheckSphere(spot + Vector3.up * (body.radius + 0.6f), body.radius, ~0, QueryTriggerInteraction.Ignore)) continue;
                to = spot;
                found = true;
            }
            if (!found) return;
            LightningBolts.Strike(transform.position, 0.3f);
            body.enabled = false;
            transform.position = to;
            body.enabled = true;
            Vector3 look = target.position - to;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look, Vector3.up);
            LightningBolts.Strike(to, 0.3f);
        }

        private void Deal(EnemyAttack attack, float damage, Vector3 direction, Vector3 from)
        {
            var struck = target.GetComponentInParent<IDamageable>();
            if (struck == null) return;
            var outcome = struck.TakeHit(new Hit(damage, 0f, direction, false, attack.Unblockable, from));
            // A blow that lands near him is a fight he can feel; a ray that lands far off tells him nothing new.
            if (Kind.HuntsByLight && outcome != HitOutcome.Missed) Sense.Fight(Vector3.Distance(transform.position, target.position));
            if (outcome == HitOutcome.Landed) Took(attack);
            // A perfect guard turns a blow struck by hand back on the one who struck it. A thrown bolt has no one to turn on.
            if (outcome != HitOutcome.PerfectGuard || attack.Ranged) return;
            Flash(StruckFlash);
            shove = -transform.forward * (Shove * 1.6f);
            if (!IsStaggered && Stagger.Add(Kind.PerfectGuardStagger * Afflictions.StaggerTakenMultiplier))
            {
                Brain.Stagger();
                Clover.Player.Tips.EnemyStaggered(transform.position);
            }
        }

        // A blow of its own has landed: the snatch takes a flask and stops to drink it, the grab takes hold.
        private void Took(EnemyAttack attack)
        {
            if (attack.Snatches && targetDefence != null && (turns == null || !turns.AnyHoldsFlask()) && Snatch.Take(targetDefence.Flasks) && Brain.Occupy())
            {
                FlasksTaken++;
                busy = BusyDrink;
            }
            else if (attack.Grabs && targetController != null && !targetController.IsHeld && targetDefence != null && !targetDefence.IsDead && Brain.Occupy())
            {
                Holds++;
                busy = BusyHold;
                Grip.Begin();
                targetController.BeginHeld(Grip);
                untilHoldEmber = 0f;
                if (!breakFreeSaid)
                {
                    breakFreeSaid = true;
                    targetDefence.Callout("Break free: strike or dodge");
                }
            }
        }

        private void Wake()
        {
            asleep = false;
            Woke?.Invoke(this);
            Brain.Alert();
            if (Brain.Occupy(Kind.RiseSeconds)) busy = BusyRise;
        }

        // The hold: it burns at intervals, never to the death, and ends by its time or by the struggle against it.
        private void TickHold(float deltaTime)
        {
            int bites = Grip.Tick(deltaTime);
            for (int i = 0; i < bites && targetDefence != null; i++)
            {
                float take = Mathf.Min(Kind.HoldBiteDamage, targetDefence.Health.Current - 1f);
                if (take > 0f) targetDefence.Health.Damage(take);
                Vfx.Play(VfxKind.Embers, target.position + Vector3.up * 1.1f, Vector3.up, 1.2f);
                Vfx.Play(VfxKind.Fire, target.position + Vector3.up * 0.9f, Vector3.up, 0.22f);
                // Being held to death teaches nothing: the bite that would kill lets go.
                if (targetDefence.Health.Current <= 1.01f) Grip.Release();
            }
            untilHoldEmber -= deltaTime;
            if (untilHoldEmber <= 0f)
            {
                untilHoldEmber = 0.16f;
                Vfx.Play(VfxKind.Embers, Vector3.Lerp(transform.position, target.position, 0.6f) + Vector3.up * 1.3f, Vector3.up, 0.4f);
            }
            if (targetDefence != null && targetDefence.IsDead) Grip.Release();
            if (Grip.IsHeld) return;
            // Thrown off: flung back and open, and shaken if it was the struggle that did it.
            Vector3 away = transform.position - target.position;
            away.y = 0f;
            shove = (away.sqrMagnitude > 0.001f ? away.normalized : -transform.forward) * (Kind.ThrownOff * 1.5f);
            busy = BusyThrown;
            Brain.Occupy(Kind.ThrownOffSeconds);
            if (Grip.BrokenByStruggle && Stagger.Add(Kind.StruggleStagger * Afflictions.StaggerTakenMultiplier))
            {
                busy = BusyNone;
                Brain.Stagger();
            }
        }

        // Ground left burning by a planted spear: it bites whoever stands in it, and pushes them toward its edge.
        private void TickBurn(float deltaTime)
        {
            burnLeft -= deltaTime;
            untilBurnShow -= deltaTime;
            if (untilBurnShow <= 0f && !BurnDrawn)
            {
                untilBurnShow = 0.1f;
                Vector2 spot = Random.insideUnitCircle * burnRadius;
                Vfx.Play(VfxKind.Fire, burnAt + new Vector3(spot.x, 0.2f, spot.y), Vector3.up, 0.7f);
            }
            untilBurnBite -= deltaTime;
            if (untilBurnBite > 0f) return;
            untilBurnBite += burnEvery;
            Vector3 offset = target.position - burnAt;
            offset.y = 0f;
            if (offset.magnitude > burnRadius || targetDefence == null) return;
            targetDefence.TakeHit(new Hit(burnDamage, 0f, offset.sqrMagnitude > 0.01f ? offset.normalized : transform.forward, false, true));
        }

        // Whatever it was busy with is over. A flask not yet drunk goes back; a hold lets go.
        private void BusyBroken()
        {
            if (busy == BusyDrink && Snatch.GiveBack(targetDefence != null ? targetDefence.Flasks : null) && targetDefence != null) targetDefence.Callout("Taken back");
            if (busy == BusyHold) Grip.Release();
            busy = BusyNone;
        }

        // The mark on the ground: the shape's bold edge with a faint inside, and a brighter copy of it that grows to meet that edge as the blow nears.
        private void ShowAttack(EnemyAttack attack)
        {
            if (attack == null || Brain.State != EnemyState.Telegraph)
            {
                ShowMarker(false, false);
                return;
            }
            if (attack != shownAttack)
            {
                shownAttack = attack;
                int index = Brain.Attacks.IndexOf(attack);
                if (markerOutline != null) markerOutline.sharedMesh = shapes[index];
                if (markerFill != null) markerFill.sharedMesh = shapes[index];
                markerShown = false;
            }
            ShowMarker(true, attack.Unblockable);
            float shown = Brain.TelegraphProgress;
            Vector3 centre = attack.Ranged || attack.Leap ? strikePoint : transform.position;
            Quaternion facing = attack.IsCircle ? Quaternion.identity : transform.rotation;
            // Once it has begun to drive through, the lane stays where it was drawn.
            if (attack.PassThrough && lunging)
            {
                centre = lungeFrom;
                facing = Quaternion.LookRotation(lungeForward, Vector3.up);
            }
            // A mark that starts at the attacker's feet lies at the height of whichever of the two stands higher (within reason),
            // so a step between them cannot bury the part of it the target is standing on.
            if (!attack.Ranged && !attack.Leap) centre.y = Mathf.Max(centre.y, Mathf.Min(target.position.y, centre.y + 0.5f));
            if (markerOutline != null) markerOutline.transform.SetPositionAndRotation(centre + Vector3.up * 0.04f, facing);
            if (markerFill != null)
            {
                markerFill.transform.SetPositionAndRotation(centre + Vector3.up * 0.06f, facing);
                markerFill.transform.localScale = new Vector3(shown, 1f, shown);
            }
        }

        private void ShowMarker(bool shown, bool red)
        {
            if (shown == markerShown && shown == false) return;
            markerShown = shown;
            if (!shown) shownAttack = null;
            Color colour = red ? RedMarker : PlainMarker;
            PaintMarker(outlineRenderer, shown, colour, OutlineAlpha);
            PaintMarker(fillRenderer, shown, MarkerShapes.Growing(colour), FillAlpha);
        }

        private void PaintMarker(Renderer markerRenderer, bool shown, Color colour, float alpha)
        {
            if (markerRenderer == null) return;
            markerRenderer.enabled = shown;
            if (!shown) return;
            colour.a = alpha;
            block.Clear();
            block.SetColor(MarkerColour, colour);
            markerRenderer.SetPropertyBlock(block);
        }

        private void Animate(float deltaTime, float speedFraction)
        {
            if (animator == null) return;
            const float ClipEnd = 0.99f;
            bool taken = true;
            switch (Brain.State)
            {
                case EnemyState.Dead:
                    animator.Play(deathHash, 0, Mathf.Min(ClipEnd, deadFor / 0.8f));
                    break;
                case EnemyState.Staggered:
                    animator.Play(staggerHash, 0, Mathf.Min(ClipEnd, Brain.StateSeconds / 0.55f));
                    break;
                case EnemyState.Flinch:
                    animator.Play(hurtHash, 0, Mathf.Min(ClipEnd, Brain.FlinchProgress));
                    break;
                case EnemyState.Busy:
                    if (busy == BusyDrink) animator.Play(drinkHash, 0, Mathf.Min(ClipEnd, Snatch.Progress));
                    else if (busy == BusyHold) animator.Play(holdHash, 0, Mathf.Repeat(Brain.StateSeconds / 0.8f, 1f));
                    else if (busy == BusyThrown) animator.Play(thrownHash, 0, Mathf.Min(ClipEnd, Brain.BusyProgress));
                    else if (busy == BusyRise) animator.Play(riseHash, 0, Mathf.Min(ClipEnd, Brain.BusyProgress));
                    else if (busy == BusySip) animator.Play(drinkHash, 0, Mathf.Min(ClipEnd, Brain.BusyProgress));
                    break;
                case EnemyState.Down:
                    if (downClips == 0) downClips = animator.HasState(0, knockdownHash) ? 1 : -1;
                    if (downClips < 0)
                    {
                        // Baked before the Forge made the fall: the broken-balance kneel stands in, down and back up.
                        float kneel = Brain.IsGettingUp ? 1f - Brain.GetUpProgress : Mathf.Min(1f, Brain.StateSeconds / EnemyBrain.FallSeconds);
                        animator.Play(staggerHash, 0, Mathf.Min(ClipEnd, kneel));
                    }
                    else if (Brain.IsGettingUp) animator.Play(getUpHash, 0, Mathf.Min(ClipEnd, Brain.GetUpProgress));
                    else if (Brain.StateSeconds < EnemyBrain.FallSeconds) animator.Play(knockdownHash, 0, Brain.StateSeconds / EnemyBrain.FallSeconds);
                    else animator.Play(downLoopHash, 0, Mathf.Repeat((Brain.StateSeconds - EnemyBrain.FallSeconds) / EnemyBrain.DownBreathSeconds, 1f));
                    break;
                case EnemyState.Telegraph:
                case EnemyState.Strike:
                case EnemyState.Recover:
                    int index = Brain.Attacks.IndexOf(Brain.Current);
                    if (index >= 0) animator.Play(attackHashes[index], 0, Mathf.Min(ClipEnd, Brain.AttackProgress));
                    break;
                default:
                    if (searching)
                    {
                        animator.Play(searchHash, 0, Mathf.Repeat(searchedFor / 3f, 1f));
                        break;
                    }
                    taken = false;
                    if (wasBusy) animator.CrossFadeInFixedTime(locomotionHash, 0.12f);
                    animator.SetFloat(SpeedHash, Mathf.Clamp01(speedFraction), 0.1f, deltaTime);
                    break;
            }
            wasBusy = taken;
        }

        // The fallen body burns away: the lightning that was in it leaves upward, what is left goes to ink, and the ink
        // breaks up into flakes of the colours it wore. The ember it owed leaves with the lightning.
        private void BurnAway(float deltaTime)
        {
            Vector3 lying = transform.position;
            if (!burning)
            {
                burning = true;
                untilFlake = 0f;
                if (Kind.Ashen)
                {
                    Vfx.Play(VfxKind.Fire, lying + Vector3.up * 0.3f, Vector3.up, 1.4f);
                    Vfx.Play(VfxKind.Embers, lying + Vector3.up * 0.5f, Vector3.up, 1.2f);
                }
                else
                {
                    Vfx.Play(VfxKind.Bolt, lying + Vector3.up * 0.2f);
                    Vfx.Play(VfxKind.Wisp, lying + Vector3.up * 0.5f);
                }
                foreach (var bodyRenderer in bodyRenderers)
                {
                    if (bodyRenderer == null) continue;
                    // Whatever glow it died wearing is dropped: from here each of its materials is painted on its own.
                    bodyRenderer.SetPropertyBlock(null);
                    bodyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                for (int i = 0; i < embersOwed; i++) EmberMotes.Fly(lying + Vector3.up * 0.6f, emberTaker, i);
                embersOwed = 0;
            }
            float burnt = Mathf.InverseLerp(BurnAwayAt, CorpseSeconds, deadFor);
            // Ink quickly, then thinner and thinner until nothing is left.
            float ink = Mathf.Clamp01(burnt / 0.18f);
            float solid = 1f - Mathf.Clamp01((burnt - 0.22f) / 0.78f);
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                var bodyRenderer = bodyRenderers[i];
                if (bodyRenderer == null) continue;
                for (int m = 0; m < ownColours[i].Length; m++)
                {
                    if (ownColours[i][m].a < 0f) continue;
                    block.Clear();
                    block.SetColor(BaseColour, Color.Lerp(ownColours[i][m], BurntColour, ink));
                    block.SetColor(Emission, Color.black);
                    block.SetColor(RimColour, Color.black);
                    block.SetFloat(Solidity, solid);
                    bodyRenderer.SetPropertyBlock(block, m);
                }
            }
            shownGlow = -1;

            untilFlake -= deltaTime;
            if (untilFlake > 0f || burnt < 0.12f) return;
            untilFlake = 0.045f;
            Vector2 spread = Random.insideUnitCircle * 0.7f;
            Vfx.Play(VfxKind.Ash, lying + new Vector3(spread.x, 0.35f, spread.y), Vector3.up, 1f, flakeColours[flakes++ % flakeColours.Length]);
        }

        // Gives the body back its colours, its solidity and its shadow.
        private void Unburn()
        {
            burning = false;
            foreach (var bodyRenderer in bodyRenderers)
            {
                if (bodyRenderer == null) continue;
                bodyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                int count = bodyRenderer.sharedMaterials.Length;
                for (int m = 0; m < count; m++) bodyRenderer.SetPropertyBlock(null, m);
            }
        }

        // A colour new to a flash already showing is painted at once: the glow is only repainted when it changes.
        private void Flash(Color colour)
        {
            flashLeft = 0.09f;
            if (colour != flashColour) shownGlow = -1;
            flashColour = colour;
        }

        // The body's glow: white for an instant when struck (the element's colour, softened, under an element), a steady pale blue while frozen, a gold pulse while staggered, a blue one while charged.
        private void Paint(float deltaTime)
        {
            int glow = 0;
            if (flashLeft > 0f) glow = 1;
            else if (Afflictions.IsFrozen && !IsDead) glow = 5;
            else if (IsStaggered && Mathf.Repeat(Time.unscaledTime * 5f, 1f) < 0.5f) glow = 2;
            else if (IsCharged && !IsDead && Mathf.Repeat(Time.unscaledTime * 3f, 1f) < 0.6f) glow = 3;
            // Close to losing its balance, it flickers amber: one or two more blows will open it.
            else if (!IsDead && !IsStaggered && Stagger.Fraction >= 0.7f && Mathf.Repeat(Time.unscaledTime * 8f, 1f) < 0.35f) glow = 4;
            flashLeft -= Time.unscaledDeltaTime;
            // Chill's ice (and Rime's frost) climbing the body band by band, from its look (Clover/Toon's _Ice).
            float ice = 0f, iceReveal = 1f;
            byte shell = 0;
            if (!IsDead && Clover.Player.Forms.ElementLooks.ShellOn(transform, out shell, out float reveal, out float amount)
                && (shell == Clover.Player.Forms.ElementLookSet.Ice || shell == Clover.Player.Forms.ElementLookSet.Rime))
            {
                ice = amount;
                iceReveal = reveal;
            }
            bool iceMoved = Mathf.Abs(ice - shownIce) > 0.004f || Mathf.Abs(iceReveal - shownIceReveal) > 0.004f;
            if (glow != shownGlow || iceMoved)
            {
                shownIce = ice;
                shownIceReveal = iceReveal;
                shownFrost = shell == Clover.Player.Forms.ElementLookSet.Rime ? 1f : 0f;
                SetGlow(glow);
            }
        }

        private float shownIce, shownIceReveal = 1f, shownFrost;
        private static readonly int IceId = Shader.PropertyToID("_Ice");
        private static readonly int IceRevealId = Shader.PropertyToID("_IceReveal");
        private static readonly int IceFrostId = Shader.PropertyToID("_IceFrost");
        private static readonly int RevealBaseId = Shader.PropertyToID("_RevealBase");
        private static readonly int RevealHeightId = Shader.PropertyToID("_RevealHeight");

        private void SetGlow(int glow)
        {
            shownGlow = glow;
            foreach (var bodyRenderer in bodyRenderers)
            {
                if (bodyRenderer == null) continue;
                if (glow == 0 && shownIce <= 0f)
                {
                    bodyRenderer.SetPropertyBlock(null);
                    continue;
                }
                block.Clear();
                if (glow != 0)
                    block.SetColor(Emission, glow == 1 ? flashColour : glow == 2 ? new Color(0.9f, 0.62f, 0.12f) : glow == 3 ? new Color(0.1f, 0.24f, 0.7f) : glow == 5 ? FrozenGlow : new Color(0.7f, 0.3f, 0.03f));
                if (shownIce > 0f)
                {
                    float feet = transform.position.y;
                    block.SetFloat(IceId, shownIce);
                    block.SetFloat(IceRevealId, shownIceReveal);
                    block.SetFloat(IceFrostId, shownFrost);
                    block.SetFloat(RevealBaseId, feet);
                    block.SetFloat(RevealHeightId, Mathf.Max(0.8f, bodyRenderer.bounds.max.y - feet));
                }
                bodyRenderer.SetPropertyBlock(block);
            }
        }
    }
}
