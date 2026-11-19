using System.Collections.Generic;
using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// The bright crescent a swing leaves in the air. Seen from above, this is what shows the player how far and how wide
    /// an attack reaches. A few flat arc meshes are kept ready; showing one places it, then it swells a little and fades.
    /// Used by the player and by enemies alike.
    /// </summary>
    public class SlashArcs : MonoBehaviour
    {
        [SerializeField] private Material material;

        /// <summary>How many arcs can be in the air at once.</summary>
        public int Capacity = 4;
        public float Seconds = 0.2f;
        /// <summary>How much of the reach the crescent's band covers, measured in from its outer edge.</summary>
        public float BandFraction = 0.42f;
        public float Height = 1.1f;

        private static readonly int ColourId = Shader.PropertyToID("_Color");

        private Transform[] arcs;
        private MeshFilter[] filters;
        private MeshRenderer[] renderers;
        private float[] ages;
        private Color[] colours;
        private MaterialPropertyBlock block;
        private int next;
        private readonly Dictionary<int, Mesh> meshes = new Dictionary<int, Mesh>();

        public void Configure(Material arcMaterial) => material = arcMaterial;

        /// <summary>The material the arcs are drawn in (`Clover/Glow`), for effects that copy it.</summary>
        public Material Material => material;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            arcs = new Transform[Capacity];
            filters = new MeshFilter[Capacity];
            renderers = new MeshRenderer[Capacity];
            ages = new float[Capacity];
            colours = new Color[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                // The arcs live in the world, not under the swinger, so they stay where the cut was made.
                var arc = new GameObject("Slash Arc");
                arcs[i] = arc.transform;
                filters[i] = arc.AddComponent<MeshFilter>();
                renderers[i] = arc.AddComponent<MeshRenderer>();
                renderers[i].sharedMaterial = material;
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
                renderers[i].enabled = false;
                ages[i] = float.MaxValue;
            }
        }

        private void OnDestroy()
        {
            foreach (var mesh in meshes.Values) Destroy(mesh);
            if (arcs == null) return;
            foreach (var arc in arcs)
                if (arc != null) Destroy(arc.gameObject);
        }

        /// <summary>
        /// Shows a cut. <paramref name="sweep"/> is +1 for a cut travelling from the swinger's right to their left, -1 for
        /// left to right, and 0 for a straight blow (an overhead cut or a thrust), which is drawn as a narrow tongue.
        /// </summary>
        public void Show(Vector3 origin, Vector3 forward, float reach, float arcDegrees, float sweep, Color colour)
        {
            if (material == null || arcs == null) return;
            int i = next;
            next = (next + 1) % Capacity;
            bool straight = Mathf.Abs(sweep) < 0.5f;
            int degrees = straight ? 26 : Mathf.Clamp(Mathf.RoundToInt(arcDegrees), 20, 340);
            filters[i].sharedMesh = MeshFor(degrees, straight);
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            arcs[i].SetPositionAndRotation(origin + Vector3.up * Height, Quaternion.LookRotation(forward, Vector3.up));
            // Mirroring the mesh turns a right-to-left cut into a left-to-right one.
            arcs[i].localScale = new Vector3(sweep < -0.5f ? -reach : reach, 1f, reach);
            ages[i] = 0f;
            colours[i] = colour;
            renderers[i].enabled = true;
            Paint(i, 1f);
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < arcs.Length; i++)
            {
                if (ages[i] > Seconds) continue;
                ages[i] += deltaTime;
                if (ages[i] > Seconds)
                {
                    renderers[i].enabled = false;
                    continue;
                }
                float t = ages[i] / Seconds;
                Paint(i, 1f - t * t);
                var scale = arcs[i].localScale;
                float grow = 1f + deltaTime * 0.9f;
                arcs[i].localScale = new Vector3(scale.x * grow, 1f, scale.z * grow);
            }
        }

        private void Paint(int i, float alpha)
        {
            var colour = colours[i];
            colour.a *= alpha;
            block.SetColor(ColourId, colour);
            renderers[i].SetPropertyBlock(block);
        }

        // A flat band of a circle of radius one, centred on +Z. u runs along the cut, v from the inner edge to the outer.
        private Mesh MeshFor(int degrees, bool straight)
        {
            int key = straight ? -degrees : degrees;
            if (meshes.TryGetValue(key, out var mesh)) return mesh;
            const int Segments = 20;
            var vertices = new Vector3[(Segments + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[Segments * 6];
            float inner = straight ? 0.15f : 1f - BandFraction;
            for (int s = 0; s <= Segments; s++)
            {
                float t = s / (float)Segments;
                if (straight)
                {
                    // A thrust or a downward cut is a thin flash along the line of the blow, not a wedge: it must not
                    // cover the fighters it passes over.
                    float along = Mathf.Lerp(0.22f, 1f, t);
                    float half = 0.075f * Mathf.Sin(Mathf.Lerp(0.25f, 1f, t) * Mathf.PI);
                    vertices[s * 2] = new Vector3(-half, 0f, along);
                    vertices[s * 2 + 1] = new Vector3(half, 0f, along);
                    uvs[s * 2] = new Vector2(t, 0f);
                    uvs[s * 2 + 1] = new Vector2(t, 1f);
                    continue;
                }
                // A cut from right to left starts at the swinger's right, which is the positive angle.
                float angle = Mathf.Deg2Rad * Mathf.Lerp(degrees * 0.5f, -degrees * 0.5f, t);
                var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                vertices[s * 2] = direction * inner;
                vertices[s * 2 + 1] = direction;
                uvs[s * 2] = new Vector2(t, 0f);
                uvs[s * 2 + 1] = new Vector2(t, 1f);
            }
            for (int s = 0; s < Segments; s++)
            {
                int v = s * 2;
                triangles[s * 6] = v;
                triangles[s * 6 + 1] = v + 1;
                triangles[s * 6 + 2] = v + 2;
                triangles[s * 6 + 3] = v + 2;
                triangles[s * 6 + 4] = v + 1;
                triangles[s * 6 + 5] = v + 3;
            }
            mesh = new Mesh { name = "Slash Arc " + key };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            meshes[key] = mesh;
            return mesh;
        }
    }
}
