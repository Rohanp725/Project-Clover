using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.EditorTools
{
    /// <summary>
    /// The sun city's props from primitives to shapes (the visual overview, step 10), and its edges (step 11). The
    /// first passes placed cylinders, spheres and capsules as jars, heaps and the wrapped dead; each is drawn now as
    /// a made thing (the city kit's jars, baskets, rubble and blocks, or a shape made here: a lumpy heap of natron, a
    /// body bound in linen) while the primitive stays where it was as the thing bumped into, undrawn. Called after
    /// everything is placed, with a dice of its own, so nothing else moves.
    /// </summary>
    public static class SunCityProps
    {
        private static System.Random dice;
        private static Material natron, linen, band, gold;

        public static void Dress(Transform world)
        {
            dice = new System.Random(5353);
            natron = SunCityKit.Flat("m_sun_natron", new Color(0.86f, 0.84f, 0.78f));
            linen = SunCityKit.Flat("m_sun_wrap", new Color(0.72f, 0.69f, 0.62f), 0.012f);
            band = SunCityKit.Flat("m_sun_wrap_band", new Color(0.55f, 0.5f, 0.43f), 0.012f);
            // Cast gold gone dull, as the floor's gold is: metal, not light.
            gold = SunCityKit.FloorGold;
            var all = world.GetComponentsInChildren<Transform>(true).ToArray();
            foreach (var t in all)
            {
                if (t == null || Owned(t, world)) continue;
                switch (t.name)
                {
                    case "Oil Jar":
                    case "Jar":
                        Instead(t, "prop_jar", Foot(t), 1f);
                        break;
                    case "Cargo Jar":
                        Instead(t, dice.NextDouble() < 0.5 ? "prop_amphora" : "prop_jar", Foot(t), 1.15f);
                        break;
                    case "Oil Lip":
                        Object.DestroyImmediate(t.gameObject);
                        break;
                    case "Great Jar":
                    {
                        // The great jar lies on its side, its mouth toward the east.
                        var jar = Instead(t, "prop_jar", t.position + Vector3.down * 0.95f, 3.3f);
                        if (jar != null) jar.transform.rotation = Quaternion.Euler(0f, 90f, 84f);
                        break;
                    }
                    case "Natron Heap":
                        Shape(t, "natron_heap", () => Heap(17), new[] { natron }, Foot(t), new Vector3(t.lossyScale.x * 0.55f, t.lossyScale.y * 0.5f, t.lossyScale.z * 0.55f), (float)dice.NextDouble() * 360f);
                        break;
                    case "Wrapped Dead":
                        // Laid on its table along the capsule's line, its foot at the capsule's south end.
                        Shape(t, "wrapped_body", Body, new[] { linen, band }, t.position + new Vector3(0f, -0.12f, -0.9f), Vector3.one, 0f, true);
                        break;
                    case "Standing Dead":
                        Shape(t, "wrapped_body", Body, new[] { linen, band }, Foot(t), Vector3.one * 0.98f, 180f + ((float)dice.NextDouble() - 0.5f) * 20f);
                        break;
                    case "Squared Block":
                    case "Offcut":
                        Instead(t, "block_fallen", Foot(t), 1f, new Vector3(t.lossyScale.x / 3.5f, t.lossyScale.y / 1.62f, t.lossyScale.z / 2.93f), dice.Next(2) * 180f);
                        break;
                    case "Broken Drum":
                        Instead(t, "column_broken", Foot(t), 0.42f, null, (float)dice.NextDouble() * 360f);
                        break;
                    case "Cast Body":
                    {
                        // The kneeling predecessors: the kit's seated king, cast in gold at a kneeling man's size, on the
                        // plinth's top (the body stood 1.62 m over the plinth's foot, whose top is 1.2 m up).
                        var at = new Vector3(Mathf.Round(t.position.x), t.position.y - 0.42f, t.position.z);
                        var holder = t.parent;
                        Hide(t);
                        var statue = SunCityKit.Place("statue_king", holder, at, at.x < 0f ? 90f : 270f, 0.17f, false);
                        if (statue != null) foreach (var shown in statue.GetComponentsInChildren<Renderer>()) shown.sharedMaterials = shown.sharedMaterials.Select(_ => gold).ToArray();
                        break;
                    }
                    case "Cast Legs":
                    case "Cast Head":
                        Hide(t);
                        break;
                }
            }
            Rubble(world);
        }

        // Under a behaviour (a gift, a jar that breaks, a wall that falls): left as it is.
        private static bool Owned(Transform t, Transform world)
        {
            for (var at = t; at != null && at != world; at = at.parent)
                foreach (var behaviour in at.GetComponents<MonoBehaviour>())
                    if (behaviour != null && !(behaviour is Clover.World.Fadeable)) return true;
            return false;
        }

        // Where a primitive's foot is: the bottom of its bounds under its middle.
        private static Vector3 Foot(Transform t)
        {
            var shown = t.GetComponent<Renderer>();
            if (shown == null) return t.position;
            var bounds = shown.bounds;
            return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }

        // The primitive keeps its collider and is no longer drawn; one with nothing to bump is taken away.
        private static void Hide(Transform t)
        {
            var shown = t.GetComponent<Renderer>();
            if (t.GetComponent<Collider>() != null) { if (shown != null) shown.enabled = false; }
            else Object.DestroyImmediate(t.gameObject);
        }

        private static GameObject Instead(Transform t, string piece, Vector3 foot, float scale, Vector3? stretch = null, float? yaw = null)
        {
            var parent = t.parent;
            float turn = yaw ?? (float)dice.NextDouble() * 360f;
            Hide(t);
            return SunCityKit.City(piece, parent, foot, turn, stretch.HasValue ? stretch.Value * scale : Vector3.one * scale);
        }

        private static void Shape(Transform t, string meshName, System.Func<Mesh> make, Material[] materials, Vector3 at, Vector3 scale, float yaw, bool lying = false)
        {
            var parent = t.parent;
            Hide(t);
            var shown = new GameObject(meshName);
            shown.transform.SetParent(parent, false);
            shown.transform.SetPositionAndRotation(at, Quaternion.Euler(lying ? 90f : 0f, yaw, 0f));
            shown.transform.localScale = scale;
            shown.AddComponent<MeshFilter>().sharedMesh = SunCityKit.SavedMesh(meshName, make);
            var renderer = shown.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            shown.isStatic = true;
        }

        // The first passes' rubble cubes: the kit's rubble and brick heaps lie where the cubes were, a few for many.
        private static void Rubble(Transform world)
        {
            var cubes = world.GetComponentsInChildren<Transform>(true).Where(t => t != null && t.name == "Rubble").ToList();
            var taken = new List<Vector3>();
            foreach (var cube in cubes)
            {
                var at = Foot(cube);
                var parent = cube.parent;
                Object.DestroyImmediate(cube.gameObject);
                if (taken.Any(p => (p - at).sqrMagnitude < 4.5f)) continue;
                taken.Add(at);
                SunCityKit.City(dice.NextDouble() < 0.6 ? "rubble" : "ground_bricks", parent, at, (float)dice.NextDouble() * 360f, Vector3.one * (0.6f + (float)dice.NextDouble() * 0.35f), null, false);
            }
        }

        // A heap of natron: a low dome, lumpy, slumped on one side, a metre across before scaling.
        private static Mesh Heap(int seed)
        {
            var random = new System.Random(seed);
            const int Around = 18, Up = 7;
            var points = new List<Vector3>();
            var triangles = new List<int>();
            float a1 = (float)random.NextDouble() * 6f, a2 = (float)random.NextDouble() * 6f;
            for (int r = 0; r <= Up; r++)
            {
                float v = r / (float)Up;
                float height = Mathf.Sin(v * Mathf.PI * 0.5f);
                for (int i = 0; i < Around; i++)
                {
                    float angle = i * Mathf.PI * 2f / Around;
                    float lump = 1f + 0.16f * Mathf.Sin(angle * 3f + a1 + v * 2f) + 0.1f * Mathf.Sin(angle * 5f + a2) + ((float)random.NextDouble() - 0.5f) * 0.12f;
                    float reach = Mathf.Cos(v * Mathf.PI * 0.5f) * lump * (1f + 0.25f * Mathf.Max(0f, Mathf.Sin(angle)) * (1f - v));
                    points.Add(new Vector3(Mathf.Sin(angle) * reach, height * (0.9f + 0.2f * (float)random.NextDouble()) * (r == Up ? 1f : 1f), Mathf.Cos(angle) * reach));
                }
            }
            for (int r = 0; r < Up; r++)
                for (int i = 0; i < Around; i++)
                {
                    int a = r * Around + i, b = r * Around + (i + 1) % Around, c = a + Around, d = b + Around;
                    triangles.AddRange(new[] { a, b, c, b, d, c });
                }
            return Faceted(points, triangles, null);
        }

        // A body bound in linen for the grave: a long tapered bundle, head and shoulders, narrowing to the feet, the
        // bindings standing a little proud in a darker linen. About 1.8 m tall, its foot at the origin.
        private static Mesh Body()
        {
            var profile = new[] { (0f, 0.13f), (0.12f, 0.16f), (0.5f, 0.19f), (1.0f, 0.22f), (1.3f, 0.24f), (1.42f, 0.2f), (1.5f, 0.13f), (1.58f, 0.15f), (1.72f, 0.14f), (1.8f, 0.06f) };
            const int Around = 10;
            var points = new List<Vector3>();
            var cloth = new List<int>();
            var bands = new List<int>();
            for (int r = 0; r < profile.Length; r++)
                for (int i = 0; i < Around; i++)
                {
                    float angle = i * Mathf.PI * 2f / Around;
                    // Flattened front to back, as a wrapped body is.
                    points.Add(new Vector3(Mathf.Sin(angle) * profile[r].Item2, profile[r].Item1, Mathf.Cos(angle) * profile[r].Item2 * 0.72f));
                }
            for (int r = 0; r + 1 < profile.Length; r++)
                for (int i = 0; i < Around; i++)
                {
                    int a = r * Around + i, b = r * Around + (i + 1) % Around, c = a + Around, d = b + Around;
                    cloth.AddRange(new[] { a, b, c, b, d, c });
                }
            // The bindings: rings a little proud of the bundle.
            foreach (float y in new[] { 0.25f, 0.55f, 0.85f, 1.15f, 1.38f })
            {
                float radius = 0.2f;
                for (int r = 0; r + 1 < profile.Length; r++)
                    if (y >= profile[r].Item1 && y <= profile[r + 1].Item1)
                        radius = Mathf.Lerp(profile[r].Item2, profile[r + 1].Item2, (y - profile[r].Item1) / (profile[r + 1].Item1 - profile[r].Item1));
                radius += 0.015f;
                int start = points.Count;
                for (int k = 0; k < 2; k++)
                    for (int i = 0; i < Around; i++)
                    {
                        float angle = i * Mathf.PI * 2f / Around;
                        points.Add(new Vector3(Mathf.Sin(angle) * radius, y + k * 0.05f, Mathf.Cos(angle) * radius * 0.72f));
                    }
                for (int i = 0; i < Around; i++)
                {
                    int a = start + i, b = start + (i + 1) % Around, c = a + Around, d = b + Around;
                    bands.AddRange(new[] { a, b, c, b, d, c });
                }
            }
            return Faceted(points, cloth, bands);
        }

        // Each face its own points, so the toon's bands fall face by face, as the kit's flat-shaded pieces do.
        private static Mesh Faceted(List<Vector3> points, List<int> first, List<int> second)
        {
            var vertices = new List<Vector3>();
            var lists = new List<List<int>> { new List<int>(), new List<int>() };
            var sources = second != null ? new[] { first, second } : new[] { first };
            for (int s = 0; s < sources.Length; s++)
                foreach (int index in sources[s])
                {
                    lists[s].Add(vertices.Count);
                    vertices.Add(points[index]);
                }
            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.subMeshCount = sources.Length;
            for (int s = 0; s < sources.Length; s++) mesh.SetTriangles(lists[s], s);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.uv = new Vector2[vertices.Count];
            return mesh;
        }
    }
}
