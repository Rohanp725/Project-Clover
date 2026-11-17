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
    public class Enemy : MonoBehaviour, IDamageable, IStaggerable, IBurnable, IAfflictable, IShovable, IBoss, IWeighted
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
        public Health Health { get; } = new Health();
        public StaggerMeter Stagger { get; } = new StaggerMeter();
        /// <summary>Everything the elements have laid on it. The burn is one of them.</summary>
        public AfflictionSet Afflictions { get; } = new AfflictionSet();
        public BurnStatus Burn => Afflictions.Burn;
        /// <summary>What it glows while frozen: it stands still in ice until it thaws or a heavy blow breaks it.</summary>
        public Color FrozenGlow = new Color(0.22f, 0.5f, 0.85f);
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
        public bool IsStriking => Brain.State == EnemyState.Telegraph || Brain.State == EnemyState.Strike;
        public bool IsRanged => Brain.RetreatRange > 0f;
        /// <summary>Raised once when it dies.</summary>
        public event System.Action<Enemy> Died;
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
        private Quaternion homeRotation;
        private float fallSpeed;
        private bool inAir;
        private float airGround;
        /// <summary>Stagger taken on coming down from being thrown into the air.</summary>
        public float LandingStagger = 20f;
        private float flashLeft;
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

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            block = new MaterialPropertyBlock();
            Kind = EnemyKind.ByName(kindName);
            home = transform.position;
            targetBody = target != null ? target.GetComponent<Collider>() : null;
            if (bars != null)
            {
                barsRotation = bars.rotation;
                var shown = bars.GetComponent<EnemyBars>();
                if (shown != null) shown.Watch(Afflictions);
            }
            homeRotation = transform.rotation;
            // The great are held by ice only for a breath.
            if (Kind.IsBoss || Kind.IsElite) Afflictions.FreezeScale = 0.4f;

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
                        if (pass == 0) ownColours[i][m] = colour;
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
            // A blow that only sets a burn off has nothing to do to an enemy that is not burning.
            if (hit.Detonate && hit.Damage <= 0f && !Burn.IsBurning) return HitOutcome.Missed;
            // Struck by someone standing off its ground: the ground stops holding it for a while.
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
            Burn.Ignite(hit.BurnSeconds, hit.BurnStacks);
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
            float takes = Afflictions.DamageTakenMultiplier(hit.Spell);
            float unsteadies = Afflictions.StaggerTakenMultiplier;
            float shoved = Afflictions.ShoveMultiplier;
            if (hit.Affliction != AfflictionKind.None && hit.AfflictionStacks > 0)
                CombatEvents.Report(this, hit.Affliction, hit.AfflictionStacks, Afflictions.Apply(hit.Affliction, hit.AfflictionStacks, hit.AfflictionSeconds, hit.AfflictionPower));

            // Its guard: a light blow from the front, while it stands ready, is turned on its weapon.
            if (Kind.Guards && (!hit.Heavy || Kind.HeavyGuardedDamage < 1f) && !hit.Unblockable && !IsStaggered && (Brain.State == EnemyState.Stalk || Brain.State == EnemyState.Idle)
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
            float multiplier = finisher ? FinisherDamageMultiplier : IsStaggered ? StaggeredDamageMultiplier : 1f;
            // A god is not cut down by one blow, however good: a finisher takes a fixed share of him.
            Health.Damage(detonated + (finisher && Kind.FinisherHealthShare > 0f ? Mathf.Max(hit.Damage * multiplier * takes, Kind.Health * Kind.FinisherHealthShare) : hit.Damage * multiplier * takes));
            flashLeft = 0.09f;
            Vector3 away = hit.Direction;
            away.y = 0f;
            shove = away.normalized * (Shove * shoved * (finisher ? 2.2f : hit.Heavy ? 1.6f : 1f));

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
            if (guardArc != null) guardArc.Show(false);
            deadFor = 0f;
            ShowMarker(false, false);
            if (bars != null) bars.gameObject.SetActive(false);
            if (staggerMark != null) staggerMark.Show(false);
            Vfx.Play(VfxKind.Shock, transform.position + Vector3.up, Vector3.up, 1.5f);
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

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (IsDead)
            {
                deadFor += deltaTime;
                Animate(deltaTime, 0f);
                if (deadFor >= BurnAwayAt) BurnAway(deltaTime);
                else Paint(deltaTime);
                if (deadFor >= CorpseSeconds) gameObject.SetActive(false);
                return;
            }
            if (target == null) return;
            // The enemies' own time can be made to drag while the player's does not.
            deltaTime *= WorldClock.EnemyScale;

            var before = Brain.Current;
            bool wasStriking = IsStriking;
            if (chargedLeft > 0f) chargedLeft -= deltaTime;
            // Cold and shaken ground slow everything it does; frozen, it does nothing at all.
            float pace = (IsCharged ? ChargedSpeed : 1f) * (Brain.Phase > 0 ? Kind.SecondPhasePace : 1f) * Afflictions.SpeedMultiplier;
            if (Brain.Phase == 0 && Kind.SecondPhaseAt > 0f && Health.Fraction <= Kind.SecondPhaseAt && !IsStaggered)
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
                if (wasBurning && untilEmber <= 0f)
                {
                    untilEmber = 0.14f;
                    Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 1.2f, Vector3.up, 0.12f);
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
            sinceLightHit += deltaTime;
            if (sinceLightHit > 1.6f) lightHits = 0;
            bool mayAttack = turns == null || turns.MayAttack(this);
            bool struck = Brain.Tick(deltaTime * pace, distance, Random.value, mayAttack, TargetOnGround);
            if (!wasStriking && IsStriking && turns != null) turns.Began(this);
            var attack = Brain.Current;
            if (attack != null && attack != before && Brain.State == EnemyState.Telegraph)
            {
                // A thrown attack lands where the target stood when it was begun, so moving gets clear of it.
                strikePoint = target.position;
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

            if (Brain.Tracks && distance > 0.05f)
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
            if (Brain.MoveWish != 0 && distance > 0.05f)
            {
                walked = Kind.WalkSpeed * pace * (Brain.MoveWish > 0 ? 1f : 0.75f);
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
            if (holdsGround)
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
                else if (!Brain.IsProvoked)
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
            // Ice holds it where it stands, whatever shoves.
            if (!Afflictions.IsFrozen) step += shove * (deltaTime * 6f);
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
            if (guardArc != null) guardArc.Show(Kind.Guards && Brain.State == EnemyState.Stalk);
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
            float damage = attack.Damage * (IsCharged ? ChargedDamage : 1f) * Afflictions.DamageDealtMultiplier;
            Vector3 forward = transform.forward;

            if (attack.IsCircle)
            {
                Vector3 centre = attack.Ranged || attack.Leap ? strikePoint : transform.position;
                if (attack.Ranged) LightningBolts.Strike(centre, 0.5f);
                else
                {
                    Vfx.Play(VfxKind.Shock, centre + Vector3.up, Vector3.up, 2.5f);
                    Vfx.Play(VfxKind.Ring, centre + Vector3.up * 0.3f, Vector3.up);
                }
                Vector3 offset = target.position - centre;
                offset.y = 0f;
                if (offset.magnitude > attack.Radius + 0.35f) return;
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
                if (ahead >= -TargetRadius && ahead <= attack.Reach + TargetRadius && aside <= attack.LineWidth * 0.5f + TargetRadius)
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
            Deal(attack, damage, forward, transform.position);
        }

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
            // A perfect guard turns a blow struck by hand back on the one who struck it. A thrown bolt has no one to turn on.
            if (outcome != HitOutcome.PerfectGuard || attack.Ranged) return;
            flashLeft = 0.09f;
            shove = -transform.forward * (Shove * 1.6f);
            if (!IsStaggered && Stagger.Add(Kind.PerfectGuardStagger * Afflictions.StaggerTakenMultiplier))
            {
                Brain.Stagger();
                Clover.Player.Tips.EnemyStaggered(transform.position);
            }
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
            bool busy = true;
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
                case EnemyState.Telegraph:
                case EnemyState.Strike:
                case EnemyState.Recover:
                    int index = Brain.Attacks.IndexOf(Brain.Current);
                    if (index >= 0) animator.Play(attackHashes[index], 0, Mathf.Min(ClipEnd, Brain.AttackProgress));
                    break;
                default:
                    busy = false;
                    if (wasBusy) animator.CrossFadeInFixedTime(locomotionHash, 0.12f);
                    animator.SetFloat(SpeedHash, Mathf.Clamp01(speedFraction), 0.1f, deltaTime);
                    break;
            }
            wasBusy = busy;
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
                Vfx.Play(VfxKind.Bolt, lying + Vector3.up * 0.2f);
                Vfx.Play(VfxKind.Wisp, lying + Vector3.up * 0.5f);
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

        // The body's glow: white for an instant when struck, a steady pale blue while frozen, a gold pulse while staggered, a blue one while charged.
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
            if (glow != shownGlow) SetGlow(glow);
        }

        private void SetGlow(int glow)
        {
            shownGlow = glow;
            foreach (var bodyRenderer in bodyRenderers)
            {
                if (bodyRenderer == null) continue;
                if (glow == 0)
                {
                    bodyRenderer.SetPropertyBlock(null);
                    continue;
                }
                block.Clear();
                block.SetColor(Emission, glow == 1 ? new Color(1.6f, 1.6f, 1.6f) : glow == 2 ? new Color(0.9f, 0.62f, 0.12f) : glow == 3 ? new Color(0.1f, 0.24f, 0.7f) : glow == 5 ? FrozenGlow : new Color(0.7f, 0.3f, 0.03f));
                bodyRenderer.SetPropertyBlock(block);
            }
        }
    }
}
