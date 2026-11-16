using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// A post to practise on. It takes blows, shows its health and stagger, rocks with each hit, slumps when staggered,
    /// and puts itself right again after being left alone, so it can be hit for as long as the player likes. A heavy
    /// blow while it is staggered is a finisher: much more damage, and the stagger ends.
    /// </summary>
    public class TrainingDummy : MonoBehaviour, IDamageable, IStaggerable, IBurnable
    {
        [SerializeField] private Transform body;
        [SerializeField] private Renderer[] flashRenderers;
        [SerializeField] private Transform healthFill;
        [SerializeField] private Transform staggerFill;

        public Health Health = new Health { Maximum = 200f };
        public StaggerMeter Stagger = new StaggerMeter();
        public BurnStatus Burn { get; } = new BurnStatus();
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
        public Color FlashColour = Color.white;
        /// <summary>While staggered the dummy pulses this colour, to say: finish it now.</summary>
        public Color StaggeredColour = new Color(1f, 0.82f, 0.25f);
        public float StaggeredPulsesPerSecond = 5f;

        public int HitsTaken { get; private set; }
        public int FinishersTaken { get; private set; }
        public bool IsStaggered => Stagger.IsStaggered;

        private static readonly int BaseColour = Shader.PropertyToID("_BaseColor");

        private MaterialPropertyBlock block;
        private Transform rockFrame;
        private Vector3 rock;
        private float sinceHit = float.MaxValue;
        private float flashLeft;
        private float untilEmber;
        private int shownFlash;

        public void Configure(Transform bodyToRock, Renderer[] renderersToFlash, Transform healthBar, Transform staggerBar)
        {
            body = bodyToRock;
            flashRenderers = renderersToFlash;
            healthFill = healthBar;
            staggerFill = staggerBar;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            // The body rocks inside whatever it hangs from, which may itself be turned to face the player.
            rockFrame = body != null && body.parent != null ? body.parent : transform;
            Health.Fill();
            Stagger.Reset();
            Burn.Reset();
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            HitsTaken++;
            sinceHit = 0f;
            float detonated = hit.Detonate ? Burn.Detonate() : 0f;
            if (detonated > 0f) Health.Damage(detonated);
            Burn.Ignite(hit.BurnSeconds, hit.BurnStacks);
            bool finisher = Stagger.IsStaggered && hit.Heavy;
            float multiplier = finisher ? FinisherDamageMultiplier : Stagger.IsStaggered ? StaggeredDamageMultiplier : 1f;
            Health.Damage(hit.Damage * multiplier);
            if (finisher)
            {
                FinishersTaken++;
                Stagger.Reset();
            }
            else
            {
                Stagger.Add(hit.Stagger);
            }
            Rock(hit.Direction, finisher ? 2.2f : hit.Heavy ? 1.5f : 1f);
            return finisher ? HitOutcome.Finisher : HitOutcome.Landed;
        }

        /// <summary>The dummy's own blow was turned by a perfect guard: it is thrown back and its stagger rises.</summary>
        public void Rebuff(float stagger, Vector3 direction)
        {
            sinceHit = 0f;
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

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Stagger.Tick(deltaTime);
            sinceHit += deltaTime;
            if (Burn.IsBurning)
            {
                Health.Damage(Burn.Tick(deltaTime));
                sinceHit = 0f;
                untilEmber -= deltaTime;
                if (untilEmber <= 0f)
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

            // The flash runs on real time so it is still seen through the moment time is stopped for the hit.
            // 0 is the dummy's own colours, 1 the white of a hit, 2 the pulse of an open stagger.
            int flash = flashLeft > 0f ? 1
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
                    block.SetColor(BaseColour, flash == 1 ? FlashColour : StaggeredColour);
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
