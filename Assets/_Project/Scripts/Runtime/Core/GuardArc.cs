using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// The guard made visible from above: a bright curved band on the ground in front of whoever holds it, as wide as
    /// what the guard covers and brightest dead ahead. It is there while the guard is up and gone the moment it drops,
    /// so an opening can be seen. A blow on the guard makes the band jump outward and flare in the colour of what
    /// happened: white for a block, gold for a perfect guard, red for a guard that breaks.
    /// </summary>
    public class GuardArc : MonoBehaviour
    {
        [SerializeField] private Material material;

        /// <summary>Metres from the holder's centre to the middle of the band, and the band's width.</summary>
        public float Radius = 1.2f;
        public float Thickness = 0.2f;
        /// <summary>Full width in degrees of what the guard covers.</summary>
        public float ArcDegrees = 200f;
        public float Height = 0.07f;
        [ColorUsage(true, true)] public Color Colour = new Color(0.9f, 1.5f, 2.6f, 0.85f);
        /// <summary>Seconds the band takes to appear and to go.</summary>
        public float ShowSeconds = 0.08f;

        private static readonly int ColourId = Shader.PropertyToID("_Color");

        private Transform band;
        private MeshRenderer bandRenderer;
        private Mesh mesh;
        private MaterialPropertyBlock block;
        private bool up;
        private float shown;
        private float pulseLeft;
        private float pulseSeconds;
        private float pulseGrow;
        private Color pulseColour;

        public void Configure(Material arcMaterial, float radius, float arcDegrees, Color colour)
        {
            material = arcMaterial;
            Radius = radius;
            ArcDegrees = arcDegrees;
            Colour = colour;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            var bandObject = new GameObject("Guard Arc");
            band = bandObject.transform;
            band.SetParent(transform, false);
            band.localPosition = new Vector3(0f, Height, 0f);
            mesh = Build();
            bandObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            bandRenderer = bandObject.AddComponent<MeshRenderer>();
            bandRenderer.sharedMaterial = material;
            bandRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            bandRenderer.receiveShadows = false;
            bandRenderer.enabled = false;
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }

        /// <summary>Raises or lowers the band with the guard.</summary>
        public void Show(bool guardUp) => up = guardUp;

        /// <summary>A blow met the guard: the band jumps outward and flares in this colour, then settles.</summary>
        public void Pulse(Color colour, float seconds, float grow)
        {
            pulseColour = colour;
            pulseSeconds = seconds;
            pulseLeft = seconds;
            pulseGrow = grow;
        }

        private void LateUpdate()
        {
            // Real time: the flare must be seen through the stop that comes with the blow.
            float deltaTime = Time.unscaledDeltaTime;
            shown = Mathf.MoveTowards(shown, up ? 1f : 0f, ShowSeconds > 0f ? deltaTime / ShowSeconds : 1f);
            float pulse = 0f;
            if (pulseLeft > 0f)
            {
                pulseLeft -= deltaTime;
                pulse = Mathf.Clamp01(pulseLeft / pulseSeconds);
            }
            bool visible = shown > 0.01f || pulse > 0f;
            if (bandRenderer.enabled != visible) bandRenderer.enabled = visible;
            if (!visible) return;

            var colour = Color.Lerp(Colour, pulseColour, pulse);
            colour.a = Mathf.Max(Colour.a * shown, pulseColour.a * pulse);
            block.SetColor(ColourId, colour);
            bandRenderer.SetPropertyBlock(block);
            // The band rises into place as it appears and jumps outward on a blow.
            float scale = (0.85f + 0.15f * shown) * (1f + pulseGrow * pulse * pulse);
            band.localScale = new Vector3(scale, 1f, scale);
        }

        // A flat band of a circle, centred on +Z. u runs along the band, v across it.
        private Mesh Build()
        {
            const int Segments = 28;
            var vertices = new Vector3[(Segments + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[Segments * 6];
            float inner = Radius - Thickness * 0.5f, outer = Radius + Thickness * 0.5f;
            for (int s = 0; s <= Segments; s++)
            {
                float t = s / (float)Segments;
                float angle = Mathf.Deg2Rad * Mathf.Lerp(-ArcDegrees * 0.5f, ArcDegrees * 0.5f, t);
                var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                vertices[s * 2] = direction * inner;
                vertices[s * 2 + 1] = direction * outer;
                uvs[s * 2] = new Vector2(t, 0f);
                uvs[s * 2 + 1] = new Vector2(t, 1f);
            }
            for (int s = 0; s < Segments; s++)
            {
                int v = s * 2;
                triangles[s * 6] = v;
                triangles[s * 6 + 1] = v + 2;
                triangles[s * 6 + 2] = v + 1;
                triangles[s * 6 + 3] = v + 2;
                triangles[s * 6 + 4] = v + 3;
                triangles[s * 6 + 5] = v + 1;
            }
            var built = new Mesh { name = "Guard Arc" };
            built.vertices = vertices;
            built.uv = uvs;
            built.triangles = triangles;
            built.RecalculateBounds();
            return built;
        }
    }
}
