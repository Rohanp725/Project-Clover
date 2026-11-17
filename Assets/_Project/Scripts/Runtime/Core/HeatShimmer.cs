using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// Heat shimmer drawn as squiggles, in the comic style of the impact stars (the owner's choice over real refraction,
    /// fire_boss_visuals.md section 5, question 2): pale wavy lines that rise off hot ground, sway, and thin away. The
    /// scene holds one pool of lines, made by the builder; <see cref="Play"/> lifts the oldest one at a point, so
    /// nothing is created during play. With no shimmer in the scene the calls do nothing.
    /// </summary>
    public class HeatShimmer : MonoBehaviour
    {
        [SerializeField] private LineRenderer[] lines = new LineRenderer[0];

        public float RiseSpeed = 0.8f, Lifetime = 0.9f, Width = 0.09f, Sway = 0.12f, Waves = 2.5f;
        /// <summary>The squiggle's colour at its strongest, times the material's warm light; its alpha is its strength.</summary>
        public Color Colour = new Color(1f, 0.95f, 0.85f, 0.85f);

        private const int Points = 8;
        private static HeatShimmer instance;

        // Domain reload is off: the scene's shimmer from the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        private Vector3[] from;
        private float[] age, height, phase;
        private readonly Vector3[] points = new Vector3[Points];
        private int next;

        public void Configure(LineRenderer[] pool) => lines = pool;

        private void Awake()
        {
            from = new Vector3[lines.Length];
            age = new float[lines.Length];
            height = new float[lines.Length];
            phase = new float[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                age[i] = 999f;
                if (lines[i] == null) continue;
                lines[i].positionCount = Points;
                lines[i].enabled = false;
            }
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        /// <summary>One squiggle rising from a point on hot ground, this tall.</summary>
        public static void Play(Vector3 at, float tall = 1f)
        {
            if (instance == null || instance.lines.Length == 0) return;
            instance.Lift(at, tall);
        }

        private void Lift(Vector3 at, float tall)
        {
            int i = next;
            next = (next + 1) % lines.Length;
            from[i] = at;
            age[i] = 0f;
            height[i] = tall;
            phase[i] = Random.value * Mathf.PI * 2f;
            if (lines[i] != null) lines[i].enabled = true;
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < lines.Length; i++)
            {
                if (age[i] >= Lifetime) continue;
                age[i] += deltaTime;
                var line = lines[i];
                if (line == null) continue;
                if (age[i] >= Lifetime)
                {
                    line.enabled = false;
                    continue;
                }
                float t = age[i] / Lifetime;
                // In, then thinning away: strongest a third of the way through its rise.
                float strength = Mathf.Sin(Mathf.Min(1f, t * 1.5f) * Mathf.PI * 0.5f) * (1f - t);
                Vector3 foot = from[i] + Vector3.up * (RiseSpeed * age[i]);
                for (int p = 0; p < Points; p++)
                {
                    float u = p / (float)(Points - 1);
                    float wave = Mathf.Sin(u * Waves * Mathf.PI * 2f + phase[i] + age[i] * 9f) * Sway * (0.4f + u);
                    points[p] = foot + new Vector3(wave, u * height[i], wave * 0.4f);
                }
                line.SetPositions(points);
                line.widthMultiplier = Width;
                var tint = new Color(Colour.r, Colour.g, Colour.b, Colour.a * strength);
                line.startColor = tint;
                line.endColor = new Color(Colour.r, Colour.g, Colour.b, 0f);
            }
        }
    }
}
