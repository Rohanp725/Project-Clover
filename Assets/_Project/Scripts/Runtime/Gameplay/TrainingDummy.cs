using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// A post to practise on. It takes blows, shows its health and stagger, rocks with each hit, slumps when staggered,
    /// and puts itself right again after being left alone, so it can be hit for as long as the player likes. A heavy
    /// blow while it is staggered is a finisher: much more damage, and the stagger ends.
    /// </summary>
    public class TrainingDummy : MonoBehaviour, IDamageable, IStaggerable, IBurnable, IAfflictable, IShovable, IWeighted
    {
        [SerializeField] private Transform body;
        [SerializeField] private Renderer[] flashRenderers;
        [SerializeField] private Transform healthFill;
        [SerializeField] private Transform staggerFill;
        [SerializeField] private StaggerMark staggerMark;

        public Health Health = new Health { Maximum = 200f };
        public StaggerMeter Stagger = new StaggerMeter();
        public AfflictionSet Afflictions { get; } = new AfflictionSet();
        public BurnStatus Burn => Afflictions.Burn;
        /// <summary>The light it wears while frozen.</summary>
        public Color FrozenColour = new Color(0.22f, 0.5f, 0.85f);
        /// <summary>How many times it has been pulled or pushed, and the last such shove, so probes can see them.</summary>
        public int Shoves { get; private set; }
        public Vector3 LastShove { get; private set; }
        /// <summary>Damage is multiplied by this while the dummy is staggered.</summary>
        public float StaggeredDamageMultiplier = 1.5f;
        /// <summary>A heavy blow on the staggered dummy is multiplied by this instead.</summary>
        public float FinisherDamageMultiplier = 3f;
        /// <summary>Seconds without a blow before health is restored.</summary>
        public float RestoreAfter = 4f;
        public float RockDegrees = 16f;
        public float RockRecoverPerSecond = 9f;
        public float SlumpDegrees = 28f;
        public float FlashSeconds = 0.09f;
        /// <summary>The light a blow adds to the dummy for an instant. It is added to its own colours, which stay under it with their ink line; painting it flat white made it a blank shape.</summary>
        public Color FlashColour = new Color(0.62f, 0.56f, 0.46f);
        /// <summary>While staggered the dummy pulses with this light, to say: finish it now.</summary>
        public Color StaggeredColour = new Color(0.9f, 0.62f, 0.12f);
        public float StaggeredPulsesPerSecond = 5f;

        public int HitsTaken { get; private set; }
        public int FinishersTaken { get; private set; }
        /// <summary>What the last blow took, after everything that multiplies it, and whether it was critical.</summary>
        public float LastDamage { get; private set; }
        public bool LastCritical { get; private set; }
        public bool IsStaggered => Stagger.IsStaggered;

        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock block;
        private Transform rockFrame;
        private Vector3 rock;
        private float sinceHit = float.MaxValue;
        private float flashLeft;
        // The flash of the last blow: FlashColour for a plain one, the element's colour softened under one.
        private Color flashTint;
        private float untilEmber;
        private int shownFlash;

        public void Configure(Transform bodyToRock, Renderer[] renderersToFlash, Transform healthBar, Transform staggerBar)
        {
            body = bodyToRock;
            flashRenderers = renderersToFlash;
            healthFill = healthBar;
            staggerFill = staggerBar;
        }

        /// <summary>Adds the mark that shows over it while it is staggered.</summary>
        public void ConfigureFeedback(StaggerMark markToShow) => staggerMark = markToShow;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            // The body rocks inside whatever it hangs from, which may itself be turned to face the player.
            rockFrame = body != null && body.parent != null ? body.parent : transform;
            Health.Fill();
            Stagger.Reset();
            Afflictions.Clear();
            var shown = healthFill != null ? healthFill.GetComponentInParent<EnemyBars>() : null;
            if (shown != null) shown.Watch(Afflictions);
        }

        /// <summary>A post set in the ground is not dragged; a probe may make it light to see a drag.</summary>
        public float Weight { get; set; } = 10f;

        public void Push(Vector3 impulse)
        {
            Shoves++;
            LastShove = impulse;
            if (impulse.sqrMagnitude > 0.0001f) Rock(impulse.normalized, 1f);
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            if (hit.Tint != flashTint) shownFlash = -1;
            flashTint = hit.Tint;
            HitsTaken++;
            sinceHit = 0f;
            float detonated = hit.Detonate ? Burn.Detonate() : 0f;
            if (detonated > 0f) Health.Damage(detonated);
            Afflictions.IgniteFrom(hit, this);
            if (hit.Heavy && Afflictions.IsFrozen)
            {
                Health.Damage(Afflictions.Shatter());
                CombatEvents.RaiseShattered(this);
            }
            float takes = Afflictions.DamageTakenMultiplier(hit.Spell);
            float unsteadies = Afflictions.StaggerTakenMultiplier;
            Afflictions.TakeFrom(hit, this);
            // As an enemy: ground that only lays an affliction is not a blow, so the last blow's numbers stay as they were.
            if (hit.OnlyLays && detonated <= 0f) return HitOutcome.Landed;
            bool finisher = Stagger.IsStaggered && hit.Heavy;
            float multiplier = finisher ? FinisherDamageMultiplier : Stagger.IsStaggered ? StaggeredDamageMultiplier : 1f;
            LastDamage = hit.Damage * multiplier * takes;
            LastCritical = hit.Critical;
            Health.Damage(LastDamage);
            if (finisher)
            {
                FinishersTaken++;
                Stagger.Reset();
            }
            else
            {
                Stagger.Add(hit.Stagger * unsteadies);
            }
            Rock(hit.Direction, finisher ? 2.2f : hit.Heavy ? 1.5f : 1f);
            return finisher ? HitOutcome.Finisher : HitOutcome.Landed;
        }

        /// <summary>The dummy's own blow was turned by a perfect guard: it is thrown back and its stagger rises.</summary>
        public void Rebuff(float stagger, Vector3 direction)
        {
            sinceHit = 0f;
            if (StruckLook.Tinted(flashTint)) shownFlash = -1;
            flashTint = default;
            Stagger.Add(stagger);
            Rock(direction, 1.6f);
        }

        // Lean away from the blow: turn about the ground axis square to its direction.
        private void Rock(Vector3 direction, float strength)
        {
            Vector3 axis = Vector3.Cross(Vector3.up, direction);
            rock = rockFrame.InverseTransformDirection(axis) * (RockDegrees * strength);
            flashLeft = FlashSeconds;
        }

        private bool lifted;
        private float groundHeight;

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Stagger.Tick(deltaTime, Afflictions.BalanceRecovers);
            sinceHit += deltaTime;
            if (Afflictions.Runs)
            {
                bool wasBurning = Burn.IsBurning;
                bool wasUp = Afflictions.IsLifted;
                if (!wasUp && !lifted) groundHeight = transform.position.y;
                Health.Damage(Afflictions.Tick(deltaTime));
                // Thrown into the air it rises off its post, hangs and comes down, as an enemy does.
                lifted = Afflictions.IsLifted;
                if (lifted || wasUp)
                {
                    Vector3 at = transform.position;
                    at.y = groundHeight + Afflictions.AirHeight;
                    transform.position = at;
                }
                if (Afflictions.LandedThisTick)
                {
                    Stagger.Add(20f);
                    CombatEvents.RaiseLanded(this);
                }
                if (wasBurning) sinceHit = 0f;
                if (Afflictions.BitThisTick)
                {
                    Clover.Core.Vfx.Play(Clover.Core.VfxKind.TintPuff, transform.position + Vector3.up * 1.3f, Vector3.up, 0.7f, Gameplay.Afflictions.Spec(AfflictionKind.Poison).Colour);
                    CombatEvents.RaisePoisonBit(this, Afflictions.BitDepth, Afflictions.BitDamage);
                }
                untilEmber -= deltaTime;
                if (wasBurning && untilEmber <= 0f)
                {
                    untilEmber = 0.15f;
                    Clover.Core.Vfx.Play(Clover.Core.VfxKind.Embers, transform.position + Vector3.up * 1.3f, Vector3.up, 0.4f);
                    Clover.Core.Vfx.Play(Clover.Core.VfxKind.Fire, transform.position + Vector3.up * 1.2f, Vector3.up, 0.12f);
                }
            }
            if (sinceHit >= RestoreAfter && Health.Current < Health.Maximum) Health.Fill();

            rock = Vector3.Lerp(rock, Vector3.zero, 1f - Mathf.Exp(-RockRecoverPerSecond * deltaTime));
            if (body != null)
                body.localRotation = Quaternion.Euler(rock) * Quaternion.Euler(Stagger.IsStaggered ? SlumpDegrees : 0f, 0f, 0f);
            if (healthFill != null) healthFill.localScale = new Vector3(Health.Fraction, 1f, 1f);
            if (staggerFill != null) staggerFill.localScale = new Vector3(Stagger.Fraction, 1f, 1f);
            if (staggerMark != null) staggerMark.Show(Stagger.IsStaggered);

            // The flash runs on real time so it is still seen through the moment time is stopped for the hit.
            // 0 is the dummy's own colours, 1 the flare of a hit, 2 the pulse of an open stagger.
            int flash = flashLeft > 0f ? 1
                : Afflictions.IsFrozen ? 3
                : Stagger.IsStaggered && Mathf.Repeat(Time.unscaledTime * StaggeredPulsesPerSecond, 1f) < 0.5f ? 2 : 0;
            flashLeft -= Time.unscaledDeltaTime;
            if (flash != shownFlash) SetFlash(flash);
        }

        // A dummy switched off mid-flash would otherwise come back white.
        private void OnDisable()
        {
            flashLeft = 0f;
            if (shownFlash != 0) SetFlash(0);
        }

        private void SetFlash(int flash)
        {
            shownFlash = flash;
            if (flashRenderers == null) return;
            foreach (var flashRenderer in flashRenderers)
            {
                if (flashRenderer == null) continue;
                if (flash != 0)
                {
                    block.SetColor(Emission, flash == 1 ? StruckLook.Flash(FlashColour, flashTint) : flash == 3 ? FrozenColour : StaggeredColour);
                    flashRenderer.SetPropertyBlock(block);
                }
                else
                {
                    flashRenderer.SetPropertyBlock(null);
                }
            }
        }
    }
}
