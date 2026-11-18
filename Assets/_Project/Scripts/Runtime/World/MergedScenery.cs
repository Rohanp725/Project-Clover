using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.World
{
    /// <summary>
    /// Scenery a builder stood piece by piece (a city of modelled house fronts, roofs, paving and clutter), drawn as a
    /// few large meshes: one for each material in each square of ground, and one for each material of each group of
    /// fronts that thins out together with the box it is stood on. A piece of the kit has up to seven materials, so the
    /// city drawn piece by piece cost two draws for every one it costs merged. The builder keeps only a list of what
    /// stood where (mesh, materials, place), not the pieces themselves, so the scene stays small; the meshes are made
    /// when the scene opens, in the editor as well as in play, and never saved.
    /// </summary>
    [ExecuteAlways]
    public class MergedScenery : MonoBehaviour
    {
        /// <summary>One piece: what it is, where it stands, its materials (a run of <see cref="materials"/>), how it
        /// casts a shadow (0 none, 1 yes, 2 only its shadow), and the fade group it thins out with (-1 none).</summary>
        [System.Serializable]
        public struct Piece
        {
            public Mesh Mesh;
            public Matrix4x4 Matrix;
            public int FirstMaterial;
            public int MaterialCount;
            public int Shadows;
            public int Group;
        }

        [SerializeField] private Piece[] pieces = new Piece[0];
        [SerializeField] private Material[] materials = new Material[0];
        // The colour each material slot gives its points, when that slot's material is one that carries many
        // materials' colours in the points (clover_toon's _VertexColour): one draw for every piece of that kind.
        [SerializeField] private Color[] tints = new Color[0];
        [SerializeField] private Fadeable[] groups = new Fadeable[0];
        [SerializeField] private float cellSize = 24f;

        private readonly List<GameObject> made = new List<GameObject>();
        private readonly List<Mesh> meshes = new List<Mesh>();

        /// <summary>How many meshes the pieces were merged into.</summary>
        public int MergedCount => made.Count;
        public int PieceCount => pieces.Length;

        public void Configure(Piece[] stood, Material[] used, Color[] colours, Fadeable[] fadeGroups, float cell)
        {
            pieces = stood;
            materials = used;
            tints = colours;
            groups = fadeGroups;
            cellSize = cell;
            Rebuild();
        }

        private void OnEnable() => Rebuild();

        private void OnDisable() => Clear();

        private void Clear()
        {
            foreach (var shown in made)
                if (shown != null) DestroyNow(shown);
            foreach (var mesh in meshes)
                if (mesh != null) DestroyNow(mesh);
            made.Clear();
            meshes.Clear();
        }

        private static void DestroyNow(Object thing)
        {
            if (Application.isPlaying) Destroy(thing);
            else DestroyImmediate(thing);
        }

        // What is merged into one object: the pieces of one square of ground (or one fade group) that cast the same
        // shadow, each material's parts kept together so each becomes one submesh, one draw.
        private sealed class Bucket
        {
            public int Shadows;
            public int Group;
            public int Points;
            public readonly List<Material> Materials = new List<Material>();
            public readonly List<List<CombineInstance>> Parts = new List<List<CombineInstance>>();
        }

        /// <summary>Makes the merged meshes afresh from the list of pieces.</summary>
        public void Rebuild()
        {
            Clear();
            if (pieces == null || pieces.Length == 0) return;
            // Each submesh of each source is cut out once as a small mesh of its own, so the merge copies only the points a
            // material uses. (CombineMeshes turns a mirrored piece's faces round itself.)
            var parts = new Dictionary<(Mesh, int, Color), Mesh>();
            var buckets = new Dictionary<(long, int), Bucket>();
            var cut = new List<Mesh>();
            foreach (var piece in pieces)
            {
                if (piece.Mesh == null) continue;
                if (!piece.Mesh.isReadable && !Application.isEditor)
                {
                    Debug.LogWarning($"[Clover.World] {piece.Mesh.name} cannot be read in a build, so it is not drawn in the merged scenery.");
                    continue;
                }
                Matrix4x4 place = piece.Matrix;
                long key;
                if (piece.Group >= 0) key = -1L - piece.Group;
                else
                {
                    Vector3 centre = place.MultiplyPoint3x4(piece.Mesh.bounds.center);
                    key = Mathf.FloorToInt(centre.x / cellSize) * 100003L + Mathf.FloorToInt(centre.z / cellSize);
                }
                if (!buckets.TryGetValue((key, piece.Shadows), out var bucket))
                {
                    bucket = new Bucket { Shadows = piece.Shadows, Group = piece.Group };
                    buckets[(key, piece.Shadows)] = bucket;
                }
                int count = Mathf.Min(piece.MaterialCount, piece.Mesh.subMeshCount);
                for (int sub = 0; sub < count; sub++)
                {
                    var material = materials[piece.FirstMaterial + sub];
                    if (material == null) continue;
                    Color tint = piece.FirstMaterial + sub < tints.Length ? tints[piece.FirstMaterial + sub] : Color.white;
                    if (!parts.TryGetValue((piece.Mesh, sub, tint), out var part))
                    {
                        part = Cut(piece.Mesh, sub, tint);
                        parts[(piece.Mesh, sub, tint)] = part;
                        cut.Add(part);
                    }
                    if (part.vertexCount == 0) continue;
                    int slot = bucket.Materials.IndexOf(material);
                    if (slot < 0)
                    {
                        slot = bucket.Materials.Count;
                        bucket.Materials.Add(material);
                        bucket.Parts.Add(new List<CombineInstance>());
                    }
                    bucket.Parts[slot].Add(new CombineInstance { mesh = part, subMeshIndex = 0, transform = place });
                    bucket.Points += part.vertexCount;
                }
            }

            var byGroup = new Dictionary<int, List<Renderer>>();
            int index = 0;
            foreach (var bucket in buckets.Values)
            {
                if (bucket.Materials.Count == 0) continue;
                // All parts merged in one go, material by material, then the index list cut back into a submesh a material.
                var all = new List<CombineInstance>();
                var ranges = new List<int>();
                foreach (var list in bucket.Parts)
                {
                    int indices = 0;
                    foreach (var part in list) indices += (int)part.mesh.GetIndexCount(0);
                    all.AddRange(list);
                    ranges.Add(indices);
                }
                var mesh = new Mesh { name = "merged_" + index, hideFlags = HideFlags.HideAndDontSave };
                if (bucket.Points > 65000) mesh.indexFormat = IndexFormat.UInt32;
                mesh.CombineMeshes(all.ToArray(), true, true, false);
                var descriptors = new SubMeshDescriptor[ranges.Count];
                int start = 0;
                for (int i = 0; i < ranges.Count; i++)
                {
                    descriptors[i] = new SubMeshDescriptor(start, ranges[i]);
                    start += ranges[i];
                }
                mesh.SetSubMeshes(descriptors);
                mesh.RecalculateBounds();
                mesh.UploadMeshData(true);
                var shown = new GameObject("Merged " + index++);
                shown.hideFlags = HideFlags.HideAndDontSave;
                shown.transform.SetParent(transform, false);
                shown.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                shown.layer = gameObject.layer;
                shown.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = shown.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = bucket.Materials.ToArray();
                renderer.shadowCastingMode = bucket.Shadows == 0 ? ShadowCastingMode.Off : bucket.Shadows == 2 ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
                renderer.receiveShadows = true;
                made.Add(shown);
                meshes.Add(mesh);
                if (bucket.Group >= 0)
                {
                    if (!byGroup.TryGetValue(bucket.Group, out var list)) byGroup[bucket.Group] = list = new List<Renderer>();
                    list.Add(renderer);
                }
            }
            foreach (var part in cut) DestroyNow(part);
            // Only in play: in the editor the fronts are made afresh on every opening and are never saved, so a saved
            // list pointing at them would point at nothing.
            if (!Application.isPlaying) return;
            foreach (var pair in byGroup)
                if (pair.Key < groups.Length && groups[pair.Key] != null) groups[pair.Key].AlsoFadeMerged(pair.Value.ToArray());
        }

        // One submesh of a mesh as a mesh of its own, holding only the points it uses, each coloured with the tint.
        private static Mesh Cut(Mesh mesh, int sub, Color tint)
        {
            var points = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;
            bool hasNormals = normals != null && normals.Length == points.Length;
            bool hasUvs = uvs != null && uvs.Length == points.Length;
            var triangles = mesh.GetTriangles(sub);
            var remap = new Dictionary<int, int>();
            var keptPoints = new List<Vector3>();
            var keptNormals = new List<Vector3>();
            var keptUvs = new List<Vector2>();
            var keptTriangles = new int[triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                int vertex = triangles[i];
                if (!remap.TryGetValue(vertex, out int at))
                {
                    at = keptPoints.Count;
                    remap[vertex] = at;
                    keptPoints.Add(points[vertex]);
                    keptNormals.Add(hasNormals ? normals[vertex] : Vector3.up);
                    keptUvs.Add(hasUvs ? uvs[vertex] : Vector2.zero);
                }
                keptTriangles[i] = at;
            }
            var part = new Mesh { name = mesh.name + "_" + sub, hideFlags = HideFlags.HideAndDontSave };
            if (keptPoints.Count > 65000) part.indexFormat = IndexFormat.UInt32;
            part.SetVertices(keptPoints);
            part.SetNormals(keptNormals);
            part.SetUVs(0, keptUvs);
            var colours = new Color[keptPoints.Count];
            for (int i = 0; i < colours.Length; i++) colours[i] = tint;
            part.colors = colours;
            part.SetTriangles(keptTriangles, 0, true);
            return part;
        }
    }
}
