using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// A post to practise on. It takes blows, shows its health and stagger, rocks with each hit, slumps when staggered,
    /// and puts itself right again after being left alone, so it can be hit for as long as the player likes. A heavy
    /// blow while it is staggered is a finisher: much more damage, and the stagger ends.
    /// </summary>
    public class TrainingDummy : MonoBehaviour, IDamageable
    {
        [SerializeField] private Transform body;
        [SerializeField] private Renderer[] flashRenderers;
        [SerializeField] private Transform healthFill;
        [SerializeField] private Transform staggerFill;

        public Health Health = new Health { Maximum = 200f };
        public StaggerMeter Stagger = new StaggerMeter();
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

        public int HitsTaken { get; private set; }
        public int FinishersTaken { get; private set; }

        private static readonly int BaseColour = Shader.PropertyToID("_BaseColor");

        private MaterialPropertyBlock block;
        private Transform rockFrame;
        private Vector3 rock;
        private float sinceHit = float.MaxValue;
        private float flashLeft;
        private bool flashing;

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
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            HitsTaken++;
            sinceHit = 0f;
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
            if (sinceHit >= RestoreAfter && Health.Current < Health.Maximum) Health.Fill();

            rock = Vector3.Lerp(rock, Vector3.zero, 1f - Mathf.Exp(-RockRecoverPerSecond * deltaTime));
            if (body != null)
                body.localRotation = Quaternion.Euler(rock) * Quaternion.Euler(Stagger.IsStaggered ? SlumpDegrees : 0f, 0f, 0f);
            if (healthFill != null) healthFill.localScale = new Vector3(Health.Fraction, 1f, 1f);
            if (staggerFill != null) staggerFill.localScale = new Vector3(Stagger.Fraction, 1f, 1f);

            // The flash runs on real time so it is still seen through the moment time is stopped for the hit.
            bool shouldFlash = flashLeft > 0f;
            flashLeft -= Time.unscaledDeltaTime;
            if (shouldFlash != flashing) SetFlash(shouldFlash);
        }

        // A dummy switched off mid-flash would otherwise come back white.
        private void OnDisable()
        {
            flashLeft = 0f;
            if (flashing) SetFlash(false);
        }

        private void SetFlash(bool on)
        {
            flashing = on;
            if (flashRenderers == null) return;
            foreach (var flashRenderer in flashRenderers)
            {
                if (flashRenderer == null) continue;
                if (on)
                {
                    block.SetColor(BaseColour, FlashColour);
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
