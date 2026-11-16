using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The shape the player leaves behind: a copy of their body as it stood at one instant, made of light, that stays
    /// where it was and fades. Lightning's dodge leaves one, and so does a dodge made at the last moment.
    /// </summary>
    public class Afterimage : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer[] bodies = new SkinnedMeshRenderer[0];
        [SerializeField] private Material material;

        /// <summary>How many can stand at once; the oldest is taken for the next.</summary>
        public int Copies = 3;
        [ColorUsage(true, true)] public Color Colour = new Color(0.5f, 1.2f, 3f, 0.8f);

        public int Shown { get; private set; }

        private static readonly int ColourId = Shader.PropertyToID("_Color");

        private Mesh[][] meshes;
        private MeshRenderer[][] renderers;
        private float[] left;
        private float[] lasts;
        private MaterialPropertyBlock block;
        private int next;

        public void Configure(SkinnedMeshRenderer[] bodiesToCopy, Material light)
        {
            bodies = bodiesToCopy;
            material = light;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            meshes = new Mesh[Copies][];
            renderers = new MeshRenderer[Copies][];
            left = new float[Copies];
            lasts = new float[Copies];
            for (int copy = 0; copy < Copies; copy++)
            {
                meshes[copy] = new Mesh[bodies.Length];
                renderers[copy] = new MeshRenderer[bodies.Length];
                for (int i = 0; i < bodies.Length; i++)
                {
                    // Not a child of the player: it must stay where it was made.
                    var part = new GameObject("Afterimage");
                    meshes[copy][i] = new Mesh { name = "Afterimage" };
                    meshes[copy][i].MarkDynamic();
                    part.AddComponent<MeshFilter>().sharedMesh = meshes[copy][i];
                    var shown = part.AddComponent<MeshRenderer>();
                    shown.sharedMaterial = material;
                    shown.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    shown.receiveShadows = false;
                    shown.enabled = false;
                    renderers[copy][i] = shown;
                }
            }
        }

        private void OnDestroy()
        {
            if (meshes == null) return;
            for (int copy = 0; copy < meshes.Length; copy++)
            {
                for (int i = 0; i < meshes[copy].Length; i++)
                {
                    if (meshes[copy][i] != null) Destroy(meshes[copy][i]);
                    if (renderers[copy][i] != null) Destroy(renderers[copy][i].gameObject);
                }
            }
        }

        /// <summary>Leaves a copy of the body as it is now, to fade over the given seconds.</summary>
        public void Leave(float seconds)
        {
            if (material == null || bodies.Length == 0 || seconds <= 0f) return;
            int copy = next;
            next = (next + 1) % Copies;
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] == null) continue;
                bodies[i].BakeMesh(meshes[copy][i], true);
                var place = renderers[copy][i].transform;
                place.SetPositionAndRotation(bodies[i].transform.position, bodies[i].transform.rotation);
                renderers[copy][i].enabled = true;
            }
            left[copy] = seconds;
            lasts[copy] = seconds;
            Shown++;
            Paint(copy, 1f);
        }

        private void Update()
        {
            for (int copy = 0; copy < Copies; copy++)
            {
                if (left[copy] <= 0f) continue;
                left[copy] -= Time.deltaTime;
                if (left[copy] <= 0f)
                {
                    foreach (var shown in renderers[copy]) shown.enabled = false;
                    continue;
                }
                Paint(copy, left[copy] / lasts[copy]);
            }
        }

        private void Paint(int copy, float strength)
        {
            var colour = Colour;
            colour.a *= strength;
            block.SetColor(ColourId, colour);
            foreach (var shown in renderers[copy]) shown.SetPropertyBlock(block);
        }
    }
}
