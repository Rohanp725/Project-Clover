using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Shows whom the next attack would turn toward: thin brackets on the ground under that enemy, found with the same
    /// rule the attack itself uses, looked up a few times a second. It is meant to be barely there until a blow lands on
    /// that enemy, when it flares for an instant. With no enemy in reach it shows nothing.
    /// </summary>
    [RequireComponent(typeof(PlayerCombat))]
    public class TargetReticle : MonoBehaviour
    {
        [SerializeField] private Transform ring;
        [SerializeField] private Renderer ringRenderer;

        public float LooksPerSecond = 6f;
        public Color Quiet = new Color(0.78f, 0.84f, 1f, 0.34f);
        [ColorUsage(true, true)] public Color Flare = new Color(1.7f, 1.7f, 1.6f, 0.95f);
        /// <summary>Real seconds the brackets stay bright after a blow lands.</summary>
        public float FlareSeconds = 0.14f;
        public float FlareSwell = 0.22f;
        public float SpinDegreesPerSecond = 24f;
        /// <summary>Metres across, and how far off the ground it lies.</summary>
        public float Width = 2.1f;
        public float Height = 0.05f;

        /// <summary>The enemy the brackets are under now, or null.</summary>
        public Transform Target { get; private set; }
        public bool IsFlaring => flaring;

        private static readonly int ColourId = Shader.PropertyToID("_Color");

        private PlayerCombat combat;
        private PlayerDefence defence;
        private MaterialPropertyBlock block;
        private float untilLook;
        private float flareLeft;
        private float spun;
        private bool flaring;

        public void Configure(Transform ringOnTheGround, Renderer rendererOfTheRing)
        {
            ring = ringOnTheGround;
            ringRenderer = rendererOfTheRing;
        }

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            block = new MaterialPropertyBlock();
            if (ringRenderer != null) ringRenderer.enabled = false;
            Paint(Quiet);
        }

        /// <summary>A blow has landed on this enemy: if it is the one marked, the brackets flare.</summary>
        public void Struck(Transform enemy) => Struck(enemy, default);

        /// <summary>
        /// The same, flaring in the blow's <paramref name="tint"/> (the weapon's or the element's), held below white (the
        /// owner, 2026-11-22: no white flash where a blow lands). A clear tint flares as before.
        /// </summary>
        public void Struck(Transform enemy, Color tint)
        {
            if (enemy == null || enemy != Target) return;
            flareLeft = FlareSeconds;
            flaring = true;
            Paint(FlareOf(tint));
        }

        /// <summary>The flare for a blow of this tint: its hue, no channel past the brackets' flare, the palest well under it.</summary>
        public Color FlareOf(Color tint)
        {
            if (!Clover.Core.StruckLook.Tinted(tint)) return Flare;
            Color lit = Clover.Core.StruckLook.Glow(tint, FlareBrightness);
            lit.a = Flare.a;
            return lit;
        }

        /// <summary>How bright a tinted flare is at its strongest channel.</summary>
        public float FlareBrightness = 0.95f;

        private void LateUpdate()
        {
            if (ring == null || ringRenderer == null) return;
            untilLook -= Time.unscaledDeltaTime;
            if (untilLook <= 0f)
            {
                untilLook = LooksPerSecond > 0f ? 1f / LooksPerSecond : 0f;
                Target = defence != null && defence.IsDead ? null : combat.AimTarget();
            }
            if (flaring)
            {
                flareLeft -= Time.unscaledDeltaTime;
                if (flareLeft <= 0f)
                {
                    flaring = false;
                    Paint(Quiet);
                }
            }
            // A target that has gone since the last look is dropped at once, not at the next look.
            bool shown = Target != null && Target.gameObject.activeInHierarchy;
            if (ringRenderer.enabled != shown) ringRenderer.enabled = shown;
            if (!shown) return;
            spun += SpinDegreesPerSecond * Time.unscaledDeltaTime;
            float width = Width * (flaring ? 1f + FlareSwell * (flareLeft / FlareSeconds) : 1f);
            ring.SetPositionAndRotation(Target.position + Vector3.up * Height, Quaternion.Euler(90f, spun, 0f));
            ring.localScale = new Vector3(width, width, 1f);
        }

        private void Paint(Color colour)
        {
            if (ringRenderer == null) return;
            block.SetColor(ColourId, colour);
            ringRenderer.SetPropertyBlock(block);
        }
    }
}
