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
    public class Enemy : MonoBehaviour, IDamageable, IStaggerable
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

        public float StaggeredDamageMultiplier = 1.5f;
        public float FinisherDamageMultiplier = 3f;
        public float StaggeredSeconds = 3.2f;
        /// <summary>Metres a blow shoves the enemy back.</summary>
        public float Shove = 0.7f;
        public float Gravity = 25f;
        /// <summary>Seconds after dying before the body is cleared away.</summary>
        public float CorpseSeconds = 2.2f;
        /// <summary>While charged by a bolt: how much faster it moves and harder it hits, and for how long.</summary>
        public float ChargedSpeed = 1.3f;
        public float ChargedDamage = 1.3f;
        public float ChargedSeconds = 9f;
        public Color PlainMarker = new Color(1f, 0.93f, 0.75f, 1f);
        public Color RedMarker = new Color(1f, 0.08f, 0.05f, 1f);
        public float OutlineAlpha = 0.2f;
        public float FillAlpha = 0.55f;
        [ColorUsage(true, true)] public Color SlashColour = new Color(1.5f, 0.5f, 1.9f, 0.8f);
        /// <summary>Half the width of the player, in metres: a blow that reaches their edge has reached them.</summary>
        public float TargetRadius = 0.4f;

        public EnemyKind Kind { get; private set; }
        public EnemyBrain Brain => Kind.Brain;
        public Health Health { get; } = new Health();
        public StaggerMeter Stagger { get; } = new StaggerMeter();
        public BurnStatus Burn { get; } = new BurnStatus();
        public bool IsStaggered => Brain.State == EnemyState.Staggered;
        public bool IsDead => Brain.State == EnemyState.Dead;
        public bool IsCharged => chargedLeft > 0f;
        public bool IsAware => Brain.State != EnemyState.Idle && Brain.State != EnemyState.Dead;
        public int BlowsStruck { get; private set; }
        public int BlowsGuarded { get; private set; }
        /// <summary>True while it is showing or striking an attack: this is what holds the turn.</summary>
        public bool IsStriking => Brain.State == EnemyState.Telegraph || Brain.State == EnemyState.Strike;
        public bool IsRanged => Brain.RetreatRange > 0f;

        private static readonly int MarkerColour = Shader.PropertyToID("_Color");
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private CharacterController body;
        private Renderer outlineRenderer;
        private Renderer fillRenderer;
        private MaterialPropertyBlock block;
        private Mesh[] shapes;
        private int locomotionHash, hurtHash, staggerHash, deathHash;
        private int[] attackHashes;
        private EnemyAttack shownAttack;
        private Vector3 strikePoint;
        private Vector3 shove;
        private Vector3 home;
        private Quaternion homeRotation;
        private float fallSpeed;
        private float flashLeft;
        private float chargedLeft;
        private float deadFor;
        private float untilEmber;
        private int shownGlow = -1;
        private bool wasBusy;
        private bool markerShown;
        private int lightHits;
        private float sinceLightHit;
        private int blocked;
        private float circle = 1f;
        private float untilTurnCircle;
        private Quaternion barsRotation;

        public void Configure(string kind, Transform targetToFight, Animator animatorToDrive, Renderer[] renderers,
            MeshFilter outline, MeshFilter fill, Transform barsRoot, Transform healthBar, Transform staggerBar, SlashArcs arcsToShow, EnemyTurns turnsToTake)
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
        }

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            block = new MaterialPropertyBlock();
            Kind = EnemyKind.ByName(kindName);
            home = transform.position;
            if (bars != null) barsRotation = bars.rotation;
            homeRotation = transform.rotation;

            locomotionHash = Animator.StringToHash("locomotion");
            hurtHash = Animator.StringToHash(Kind.Prefix + "_hurt");
            staggerHash = Animator.StringToHash(Kind.Prefix + "_stagger");
            deathHash = Animator.StringToHash(Kind.Prefix + "_death");
            attackHashes = new int[Brain.Attacks.Count];
            shapes = new Mesh[Brain.Attacks.Count];
            for (int i = 0; i < attackHashes.Length; i++)
            {
                var attack = Brain.Attacks[i];
                attackHashes[i] = Animator.StringToHash(attack.Name);
                shapes[i] = attack.IsCircle ? MarkerShapes.Sector(attack.Radius, 360f) : MarkerShapes.Sector(attack.Reach, attack.ArcDegrees);
            }
            if (markerOutline != null) outlineRenderer = markerOutline.GetComponent<Renderer>();
            if (markerFill != null) fillRenderer = markerFill.GetComponent<Renderer>();
            Restore();
        }

        private void OnDestroy()
        {
            if (shapes == null) return;
            foreach (var shape in shapes)
                if (shape != null) Destroy(shape);
        }

        /// <summary>Puts the enemy back where it began, alive and unaware, as when the player rests or falls.</summary>
        public void Restore()
        {
            gameObject.SetActive(true);
            body.enabled = false;
            transform.SetPositionAndRotation(home, homeRotation);
            body.enabled = true;
            Health.Maximum = Kind.Health;
            Health.Fill();
            Stagger.Maximum = Kind.StaggerMaximum;
            Stagger.StaggeredSeconds = StaggeredSeconds;
            Stagger.Reset();
            Burn.Reset();
            Brain.Reset();
            shove = Vector3.zero;
            fallSpeed = 0f;
            chargedLeft = 0f;
            deadFor = 0f;
            flashLeft = 0f;
            shownAttack = null;
            lightHits = 0;
            blocked = 0;
            ShowMarker(false, false);
            SetGlow(0);
            if (bars != null) bars.gameObject.SetActive(false);
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
            Brain.Alert();
            if (bars != null) bars.gameObject.SetActive(true);
            Burn.Ignite(hit.BurnSeconds);

            // Its guard: a light blow from the front, while it stands ready, is turned on its weapon.
            if (Kind.Guards && !hit.Heavy && !hit.Unblockable && !IsStaggered && (Brain.State == EnemyState.Stalk || Brain.State == EnemyState.Idle)
                && Vector3.Angle(transform.forward, -hit.Direction) < 75f)
            {
                BlowsGuarded++;
                Health.Damage(hit.Damage * Kind.GuardedDamage);
                Vfx.Play(VfxKind.GuardSpark, transform.position + Vector3.up * 1.2f + transform.forward * 0.5f, transform.forward);
                Vector3 back = hit.Direction;
                back.y = 0f;
                shove = back.normalized * (Shove * 0.5f);
                if (Health.IsDead)
                {
                    Die();
                    return HitOutcome.Landed;
                }
                if (Stagger.Add(hit.Stagger))
                {
                    Brain.Stagger();
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
            Health.Damage(hit.Damage * multiplier);
            flashLeft = 0.09f;
            Vector3 away = hit.Direction;
            away.y = 0f;
            shove = away.normalized * (Shove * (finisher ? 2.2f : hit.Heavy ? 1.6f : 1f));

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
            if (!IsStaggered && Stagger.Add(hit.Stagger))
            {
                Brain.Stagger();
                Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 1.4f, Vector3.up);
            }
            else
            {
                // Poise: light blows are shrugged off until enough have landed close together.
                sinceLightHit = 0f;
                if (hit.Heavy || ++lightHits >= Kind.PoiseHits)
                {
                    lightHits = 0;
                    Brain.Flinch(hit.Heavy);
                }
            }
            return HitOutcome.Landed;
        }

        private void Die()
        {
            Brain.Die();
            deadFor = 0f;
            ShowMarker(false, false);
            if (bars != null) bars.gameObject.SetActive(false);
            Vfx.Play(VfxKind.Shock, transform.position + Vector3.up, Vector3.up, 1.5f);
            // The body no longer stands in anyone's way.
            body.enabled = false;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (IsDead)
            {
                deadFor += deltaTime;
                Animate(deltaTime, 0f);
                Paint(deltaTime);
                if (deadFor >= CorpseSeconds)
                {
                    Vfx.Play(VfxKind.Shock, transform.position + Vector3.up * 0.3f, Vector3.up, 0.8f);
                    gameObject.SetActive(false);
                }
                return;
            }
            if (target == null) return;

            if (chargedLeft > 0f) chargedLeft -= deltaTime;
            float pace = IsCharged ? ChargedSpeed : 1f;

            if (Burn.IsBurning)
            {
                Health.Damage(Burn.Tick(deltaTime));
                untilEmber -= deltaTime;
                if (untilEmber <= 0f)
                {
                    untilEmber = 0.14f;
                    Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 1.2f, Vector3.up, 0.12f);
                    Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.4f, Vector3.up, 0.35f);
                }
                if (Health.IsDead)
                {
                    Die();
                    return;
                }
            }

            Stagger.Tick(deltaTime);
            if (IsStaggered && !Stagger.IsStaggered) Brain.EndStagger();

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            var before = Brain.Current;
            sinceLightHit += deltaTime;
            if (sinceLightHit > 1.6f) lightHits = 0;
            bool mayAttack = turns == null || turns.MayAttack(this);
            bool wasStriking = IsStriking;
            bool struck = Brain.Tick(deltaTime * pace, distance, Random.value, mayAttack);
            if (!wasStriking && IsStriking && turns != null) turns.Began(this);
            var attack = Brain.Current;
            if (attack != null && attack != before && Brain.State == EnemyState.Telegraph)
            {
                // A thrown attack lands where the target stood when it was begun, so moving gets clear of it.
                strikePoint = target.position;
                if (bars != null) bars.gameObject.SetActive(true);
            }

            if (Brain.Tracks && distance > 0.05f)
            {
                var facing = Quaternion.LookRotation(toTarget, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, Kind.TurnDegreesPerSecond * pace * deltaTime);
            }

            if (struck) Strike(attack);

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
                step += sideways * (Kind.WalkSpeed * 0.45f * deltaTime);
                walked = Mathf.Max(walked, Kind.WalkSpeed * 0.45f);
            }
            if (attack != null && attack.Lunge > 0f)
            {
                // The drive starts just before the blow and is spent by the time it lands.
                float from = attack.Telegraph * 0.78f, until = attack.Telegraph;
                float elapsed = Brain.AttackProgress * attack.Total;
                if (elapsed >= from && elapsed < until) step += transform.forward * (attack.Lunge * deltaTime * pace / (until - from));
            }
            step += shove * (deltaTime * 6f);
            shove = Vector3.Lerp(shove, Vector3.zero, 1f - Mathf.Exp(-9f * deltaTime));
            fallSpeed = body.isGrounded ? 1f : fallSpeed + Gravity * deltaTime;
            step.y = -fallSpeed * deltaTime;
            if (body.enabled) body.Move(step);

            ShowAttack(attack);
            Animate(deltaTime, Kind.WalkSpeed > 0f ? walked / Kind.WalkSpeed : 0f);
            Paint(deltaTime);
            // The bars face the camera however the enemy turns.
            if (bars != null) bars.rotation = barsRotation;
            if (healthFill != null) healthFill.localScale = new Vector3(Health.Fraction, 1f, 1f);
            if (staggerFill != null) staggerFill.localScale = new Vector3(IsStaggered ? 1f : Stagger.Fraction, 1f, 1f);
        }

        private void Strike(EnemyAttack attack)
        {
            BlowsStruck++;
            float damage = attack.Damage * (IsCharged ? ChargedDamage : 1f);
            Vector3 forward = transform.forward;

            if (attack.IsCircle)
            {
                Vector3 centre = attack.Ranged ? strikePoint : transform.position;
                if (attack.Ranged) LightningBolts.Strike(centre, 0.5f);
                else
                {
                    Vfx.Play(VfxKind.Shock, transform.position + Vector3.up, Vector3.up, 2.5f);
                    Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 0.3f, Vector3.up);
                }
                Vector3 offset = target.position - centre;
                offset.y = 0f;
                if (offset.magnitude <= attack.Radius + 0.35f)
                    Deal(attack, damage, offset.sqrMagnitude > 0.01f ? offset.normalized : forward, centre);
                return;
            }

            if (arcs != null) arcs.Show(transform.position, forward, attack.Reach, attack.ArcDegrees, attack.Sweep, SlashColour);
            // Enemies fight the player only, so the blow is measured against the player directly: inside the reach
            // (allowing for the player's own width) and inside the wedge, or close enough to be touching.
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float gap = Mathf.Max(0f, toTarget.magnitude - TargetRadius);
            if (gap > attack.Reach) return;
            if (gap > 0.6f && Vector3.Angle(forward, toTarget) > attack.ArcDegrees * 0.5f) return;
            Deal(attack, damage, forward, transform.position);
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
            if (!IsStaggered && Stagger.Add(Kind.PerfectGuardStagger)) Brain.Stagger();
        }

        // The mark on the ground: the whole shape faintly, and a brighter copy that grows to fill it as the blow nears.
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
            Vector3 centre = attack.Ranged ? strikePoint : transform.position;
            Quaternion facing = attack.IsCircle ? Quaternion.identity : transform.rotation;
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
            PaintMarker(fillRenderer, shown, colour, FillAlpha);
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

        // The body's glow: white for an instant when struck, a gold pulse while staggered, a blue one while charged.
        private void Paint(float deltaTime)
        {
            int glow = 0;
            if (flashLeft > 0f) glow = 1;
            else if (IsStaggered && Mathf.Repeat(Time.unscaledTime * 5f, 1f) < 0.5f) glow = 2;
            else if (IsCharged && !IsDead && Mathf.Repeat(Time.unscaledTime * 3f, 1f) < 0.6f) glow = 3;
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
                block.SetColor(Emission, glow == 1 ? new Color(1.6f, 1.6f, 1.6f) : glow == 2 ? new Color(0.9f, 0.62f, 0.12f) : new Color(0.1f, 0.24f, 0.7f));
                bodyRenderer.SetPropertyBlock(block);
            }
        }
    }
}
