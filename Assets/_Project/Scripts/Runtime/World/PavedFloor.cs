using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.World
{
    /// <summary>
    /// A stretch of paving laid by <see cref="PavingLayout"/> when the scene opens, in the editor as well as in play: only
    /// the rectangle, the slab sizes, the worn patches and a seed are kept in the scene, so a paved city costs the scene
    /// a few lines and not thousands of slabs. Four materials: the joints' bed, the stone, the stone a shade darker, and
    /// the paler recut slabs.
    /// </summary>
    [ExecuteAlways]
    public class PavedFloor : MonoBehaviour
    {
        [SerializeField] private Vector4 area;
        [SerializeField] private float top;
        [SerializeField] private Vector2 slab = new Vector2(0.7f, 1.2f);
        [SerializeField] private float joint = 0.04f;
        [SerializeField] private float loss = 0.02f;
        [SerializeField] private Vector4[] bare = new Vector4[0];
        [SerializeField] private Vector4 hole;
        [SerializeField] private int seed;
        [SerializeField] private Material[] materials = new Material[4];
        [SerializeField] private Color[] tints = new Color[0];

        private GameObject shown;
        private Mesh mesh;

        /// <summary>With <paramref name="colours"/> (four, linear), <paramref name="used"/> is one material that takes
        /// its colour from the points (clover_toon's _VertexColour), and the whole stretch is one draw.</summary>
        public void Configure(Vector4 rectangle, float height, Vector2 slabSize, float jointWidth, float lost, Vector4[] worn, Vector4 unpaved, int layoutSeed, Material[] used, Color[] colours = null)
        {
            area = rectangle;
            top = height;
            slab = slabSize;
            joint = jointWidth;
            loss = lost;
            bare = worn ?? new Vector4[0];
            hole = unpaved;
            seed = layoutSeed;
            materials = used;
            tints = colours ?? new Color[0];
            Lay();
        }

        private void OnEnable() => Lay();

        private void OnDisable() => Clear();

        private void Clear()
        {
            if (shown != null)
            {
                if (Application.isPlaying) Destroy(shown);
                else DestroyImmediate(shown);
            }
            if (mesh != null)
            {
                if (Application.isPlaying) Destroy(mesh);
                else DestroyImmediate(mesh);
            }
            shown = null;
            mesh = null;
        }

        private void Lay()
        {
            Clear();
            if (area.y <= area.x || area.w <= area.z) return;
            mesh = PavingLayout.Build(new PavingLayout.Settings
            {
                West = area.x, East = area.y, South = area.z, North = area.w, Top = top,
                SlabMin = slab.x, SlabMax = slab.y, Joint = joint, Loss = loss, Bare = bare, Hole = hole, Seed = seed,
                Tints = tints.Length >= 4 ? tints : null,
            });
            mesh.hideFlags = HideFlags.HideAndDontSave;
            mesh.UploadMeshData(true);
            shown = new GameObject("Slabs") { hideFlags = HideFlags.HideAndDontSave, layer = gameObject.layer };
            shown.transform.SetParent(transform, false);
            shown.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            shown.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = shown.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            // Slabs a few centimetres high cast nothing worth a shadow pass.
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }
    }
}
