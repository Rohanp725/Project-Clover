using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The small things that make a built place look lived in and rained on: puddles, tufts of grass, fallen leaves,
    /// lantern posts with their pools of warm light, barrels, spears left standing in the ground. Every builder may
    /// use them. Nothing here is solid unless it says so, and nothing is tall enough to hide the player.
    /// </summary>
    public static class Dressing
    {
        private const string Folder = BuildKit.GeneratedRoot + "/Dressing";

        private static Material grass, grassDark, leafRed, leafGold, puddle, puddleRim, puddleGlint, iron, wood, band, paper, flame, sack, shaft, steel;
        private static Mesh tuft, scatter;

        /// <summary>Makes the materials and shapes the pieces share. Call once before placing any.</summary>
        public static void Prepare()
        {
            grass = BuildKit.Toon(Folder, "m_dress_grass", new Color(0.2f, 0.4f, 0.3f), false, null, 0f, 0f);
            grassDark = BuildKit.Toon(Folder, "m_dress_grass_dark", new Color(0.17f, 0.34f, 0.31f), false, null, 0f, 0f);
            leafRed = BuildKit.Toon(Folder, "m_dress_leaf_red", new Color(0.58f, 0.17f, 0.15f), false, null, 0f, 0f);
            leafGold = BuildKit.Toon(Folder, "m_dress_leaf_gold", new Color(0.66f, 0.42f, 0.14f), false, null, 0f, 0f);
            // Standing water, drawn flat: a dark sheet, a paler line round it where the wet stone shows, and a glint of sky.
            // It takes light like the ground does, so a lantern's pool lies across it.
            puddle = BuildKit.Toon(Folder, "m_dress_water", new Color(0.12f, 0.15f, 0.3f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.015f, 0.02f, 0.06f));
            puddleRim = BuildKit.Toon(Folder, "m_dress_water_edge", new Color(0.34f, 0.37f, 0.56f), false, null, 0f, 0f);
            puddleGlint = BuildKit.Toon(Folder, "m_dress_water_glint", new Color(0.5f, 0.6f, 0.9f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.1f, 0.14f, 0.3f));
            iron = BuildKit.Toon(Folder, "m_dress_iron", new Color(0.15f, 0.14f, 0.24f), false, null, 0f, 0.02f);
            wood = BuildKit.Toon(Folder, "m_dress_wood", new Color(0.36f, 0.24f, 0.24f), false, ProceduralTextures.Planks(), 0.9f, 0.02f);
            band = BuildKit.Toon(Folder, "m_dress_band", new Color(0.22f, 0.2f, 0.3f), false, null, 0f, 0.015f);
            paper = BuildKit.Toon(Folder, "m_dress_lantern", new Color(1f, 0.72f, 0.36f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(2.6f, 1.25f, 0.35f));
            flame = BuildKit.Glow(Folder, "m_dress_flame", new Color(2.2f, 0.9f, 0.2f, 1f), 0, true, 1.4f, 0.7f).Graphic(0.22f, 0.3f);
            sack = BuildKit.Toon(Folder, "m_dress_sack", new Color(0.5f, 0.44f, 0.36f), false, null, 0f, 0.02f);
            shaft = BuildKit.Toon(Folder, "m_dress_shaft", new Color(0.3f, 0.2f, 0.2f), false, null, 0f, 0.015f);
            steel = BuildKit.Toon(Folder, "m_dress_steel", new Color(0.66f, 0.7f, 0.8f), false, null, 0f, 0.012f).Lit(new Color(1.2f, 1.4f, 1.8f), 0.5f);
            tuft = TuftMesh();
            scatter = ScatterMesh();
        }

        /// <summary>A puddle: one to three overlapping pools lying on the ground at the given height.</summary>
        public static void Puddle(Transform parent, Vector3 at, float size, int seed)
        {
            var random = new System.Random(seed);
            var group = BuildKit.Group("Puddle", parent);
            group.position = at;
            int pools = 1 + random.Next(3);
            for (int i = 0; i < pools; i++)
            {
                float width = size * (0.6f + (float)random.NextDouble() * 0.6f), depth = size * (0.45f + (float)random.NextDouble() * 0.5f);
                var offset = i == 0 ? Vector3.zero : new Vector3(((float)random.NextDouble() - 0.5f) * size * 1.1f, 0f, ((float)random.NextDouble() - 0.5f) * size * 0.9f);
                float yaw = (float)random.NextDouble() * 180f;
                // Every pool's edge lies under every pool's water, so pools that overlap read as one shape with one edge.
                var rim = BuildKit.Shape(PrimitiveType.Cylinder, "Edge", group, offset + Vector3.up * 0.014f, new Vector3(width + 0.12f, 0.001f, depth + 0.12f), puddleRim, new Vector3(0f, yaw, 0f));
                rim.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rim.gameObject.isStatic = true;
                var sheet = BuildKit.Shape(PrimitiveType.Cylinder, "Water", group, offset + Vector3.up * 0.019f, new Vector3(width, 0.001f, depth), puddle, new Vector3(0f, yaw, 0f));
                sheet.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                sheet.gameObject.isStatic = true;
                if (i > 0) continue;
                var glint = BuildKit.Shape(PrimitiveType.Cylinder, "Glint", group, offset + Quaternion.Euler(0f, yaw, 0f) * new Vector3(-width * 0.18f, 0.024f, depth * 0.16f),
                    new Vector3(width * 0.3f, 0.001f, depth * 0.07f), puddleGlint, new Vector3(0f, yaw + 12f, 0f));
                glint.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                glint.gameObject.isStatic = true;
            }
        }

        /// <summary>A scatter of puddles over a rectangle of ground, the same every build.</summary>
        public static void Puddles(Transform parent, float west, float east, float south, float north, float height, int count, int seed)
        {
            var random = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                var at = new Vector3(Mathf.Lerp(west, east, (float)random.NextDouble()), height, Mathf.Lerp(south, north, (float)random.NextDouble()));
                Puddle(parent, at, 1.1f + (float)random.NextDouble() * 1.6f, seed * 100 + i);
            }
        }

        /// <summary>A tuft of rough grass.</summary>
        public static void Tuft(Transform parent, Vector3 at, float size, int seed)
        {
            var random = new System.Random(seed);
            var blade = BuildKit.Shape(PrimitiveType.Cube, "Grass", parent, at, new Vector3(size, size * (0.8f + (float)random.NextDouble() * 0.5f), size),
                random.Next(3) == 0 ? grassDark : grass, new Vector3(0f, (float)random.NextDouble() * 360f, 0f));
            blade.GetComponent<MeshFilter>().sharedMesh = tuft;
            blade.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            blade.gameObject.isStatic = true;
        }

        /// <summary>Grass growing along the foot of a wall, between two points.</summary>
        public static void GrassLine(Transform parent, Vector3 from, Vector3 to, float spread, int seed)
        {
            var random = new System.Random(seed);
            float length = (to - from).magnitude;
            Vector3 aside = Vector3.Cross(Vector3.up, (to - from).normalized);
            int count = Mathf.Max(1, Mathf.RoundToInt(length / 0.9f));
            for (int i = 0; i < count; i++)
            {
                // In clumps with gaps, not a hedge.
                if (random.NextDouble() < 0.35) continue;
                var at = Vector3.Lerp(from, to, (i + (float)random.NextDouble()) / count) + aside * ((float)random.NextDouble() * spread);
                Tuft(parent, at, 0.45f + (float)random.NextDouble() * 0.5f, seed * 50 + i);
            }
        }

        /// <summary>Grass in a rough patch around a point.</summary>
        public static void GrassPatch(Transform parent, Vector3 centre, float radius, int count, int seed)
        {
            var random = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f, reach = Mathf.Sqrt((float)random.NextDouble()) * radius;
                Tuft(parent, centre + new Vector3(Mathf.Cos(angle) * reach, 0f, Mathf.Sin(angle) * reach), 0.4f + (float)random.NextDouble() * 0.6f, seed * 70 + i);
            }
        }

        /// <summary>Fallen leaves lying flat over a round of ground.</summary>
        public static void Leaves(Transform parent, Vector3 centre, float radius, int seed)
        {
            var random = new System.Random(seed);
            for (int layer = 0; layer < 2; layer++)
            {
                var leaves = BuildKit.Shape(PrimitiveType.Cube, "Leaves", parent, centre + Vector3.up * (0.015f + layer * 0.004f), new Vector3(radius, 1f, radius),
                    layer == 0 ? leafRed : leafGold, new Vector3(0f, (float)random.NextDouble() * 360f, 0f));
                leaves.GetComponent<MeshFilter>().sharedMesh = scatter;
                leaves.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                leaves.gameObject.isStatic = true;
            }
        }

        /// <summary>A lantern on an iron post: a small warm pool of light. Solid only at the post.</summary>
        public static Light Lantern(Transform parent, Vector3 foot, float yaw, float height = 2.4f)
        {
            var group = BuildKit.Group("Lantern", parent);
            group.SetPositionAndRotation(foot, Quaternion.Euler(0f, yaw, 0f));
            BuildKit.Shape(PrimitiveType.Cube, "Foot", group, new Vector3(0f, 0.12f, 0f), new Vector3(0.5f, 0.24f, 0.5f), band);
            BuildKit.Shape(PrimitiveType.Cube, "Post", group, new Vector3(0f, height * 0.5f, 0f), new Vector3(0.14f, height, 0.14f), iron);
            BuildKit.Shape(PrimitiveType.Cube, "Arm", group, new Vector3(0.32f, height - 0.08f, 0f), new Vector3(0.78f, 0.08f, 0.08f), iron);
            BuildKit.Shape(PrimitiveType.Cube, "Brace", group, new Vector3(0.18f, height - 0.3f, 0f), new Vector3(0.5f, 0.06f, 0.06f), iron, new Vector3(0f, 0f, 40f));
            BuildKit.Shape(PrimitiveType.Cube, "Chain", group, new Vector3(0.62f, height - 0.24f, 0f), new Vector3(0.03f, 0.26f, 0.03f), iron);
            BuildKit.Shape(PrimitiveType.Cube, "Roof", group, new Vector3(0.62f, height - 0.4f, 0f), new Vector3(0.42f, 0.07f, 0.42f), iron, new Vector3(0f, 45f, 0f));
            var glass = BuildKit.Shape(PrimitiveType.Cube, "Light", group, new Vector3(0.62f, height - 0.62f, 0f), new Vector3(0.26f, 0.36f, 0.26f), paper);
            glass.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cube, "Floor", group, new Vector3(0.62f, height - 0.83f, 0f), new Vector3(0.34f, 0.05f, 0.34f), iron);
            var post = group.gameObject.AddComponent<CapsuleCollider>();
            post.radius = 0.2f;
            post.height = height;
            post.center = new Vector3(0f, height * 0.5f, 0f);

            var lightObject = new GameObject("Lantern Light");
            lightObject.transform.SetParent(group, false);
            lightObject.transform.localPosition = new Vector3(0.62f, height - 0.5f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.6f, 0.26f);
            light.range = 10f;
            light.intensity = 17f;
            light.shadows = LightShadows.None;
            return light;
        }

        /// <summary>A barrel, standing or on its side. Solid.</summary>
        public static void Barrel(Transform parent, Vector3 foot, float yaw, bool fallen = false)
        {
            var group = BuildKit.Group("Barrel", parent);
            group.SetPositionAndRotation(foot + (fallen ? Vector3.up * 0.42f : Vector3.zero), Quaternion.Euler(fallen ? 90f : 0f, yaw, 0f));
            float lift = fallen ? 0f : 0.55f;
            BuildKit.Shape(PrimitiveType.Cylinder, "Staves", group, new Vector3(0f, lift, 0f), new Vector3(0.8f, 0.55f, 0.8f), wood);
            foreach (float y in new[] { -0.36f, 0f, 0.36f })
                BuildKit.Shape(PrimitiveType.Cylinder, "Hoop", group, new Vector3(0f, lift + y, 0f), new Vector3(0.86f, 0.04f, 0.86f), band);
            BuildKit.Shape(PrimitiveType.Cylinder, "Lid", group, new Vector3(0f, lift + 0.56f, 0f), new Vector3(0.7f, 0.01f, 0.7f), band);
            var body = group.gameObject.AddComponent<CapsuleCollider>();
            body.radius = 0.42f;
            body.height = 1.1f;
            body.center = new Vector3(0f, lift, 0f);
        }

        /// <summary>A heap of sacks. Solid, and low.</summary>
        public static void Sacks(Transform parent, Vector3 foot, float yaw)
        {
            var group = BuildKit.Group("Sacks", parent);
            group.SetPositionAndRotation(foot, Quaternion.Euler(0f, yaw, 0f));
            BuildKit.Shape(PrimitiveType.Sphere, "Sack", group, new Vector3(0f, 0.25f, 0f), new Vector3(0.9f, 0.5f, 0.6f), sack);
            BuildKit.Shape(PrimitiveType.Sphere, "Sack", group, new Vector3(0.7f, 0.22f, 0.15f), new Vector3(0.8f, 0.44f, 0.55f), sack, new Vector3(0f, 30f, 0f));
            BuildKit.Shape(PrimitiveType.Sphere, "Sack", group, new Vector3(0.3f, 0.62f, 0.05f), new Vector3(0.8f, 0.42f, 0.55f), sack, new Vector3(0f, -20f, 8f));
            var heap = group.gameObject.AddComponent<BoxCollider>();
            heap.center = new Vector3(0.35f, 0.4f, 0.05f);
            heap.size = new Vector3(1.5f, 0.8f, 0.8f);
        }

        /// <summary>Spears left standing in the ground where their owners fell, with a torn pennant on one. Not solid.</summary>
        public static void Spears(Transform parent, Vector3 centre, int count, int seed, Material pennant)
        {
            var random = new System.Random(seed);
            var group = BuildKit.Group("Spears", parent);
            group.position = centre;
            for (int i = 0; i < count; i++)
            {
                var at = new Vector3(((float)random.NextDouble() - 0.5f) * 1.8f, 0f, ((float)random.NextDouble() - 0.5f) * 1.8f);
                var lean = new Vector3(((float)random.NextDouble() - 0.5f) * 36f, (float)random.NextDouble() * 360f, ((float)random.NextDouble() - 0.5f) * 36f);
                var spear = BuildKit.Group("Spear", group);
                spear.localPosition = at;
                spear.localRotation = Quaternion.Euler(lean);
                float length = 1.7f + (float)random.NextDouble() * 0.6f;
                BuildKit.Shape(PrimitiveType.Cube, "Shaft", spear, new Vector3(0f, length * 0.5f, 0f), new Vector3(0.05f, length, 0.05f), shaft);
                BuildKit.Shape(PrimitiveType.Cube, "Head", spear, new Vector3(0f, length + 0.14f, 0f), new Vector3(0.09f, 0.3f, 0.02f), steel);
                if (i == 0 && pennant != null)
                    BuildKit.Shape(PrimitiveType.Cube, "Pennant", spear, new Vector3(0.2f, length - 0.2f, 0f), new Vector3(0.4f, 0.26f, 0.015f), pennant, new Vector3(0f, 0f, -12f));
            }
        }

        /// <summary>A two-wheeled cart, broken and tipped on its shafts. Solid.</summary>
        public static void Cart(Transform parent, Vector3 foot, float yaw)
        {
            var group = BuildKit.Group("Cart", parent);
            group.SetPositionAndRotation(foot, Quaternion.Euler(0f, yaw, 0f));
            var bed = BuildKit.Group("Bed", group);
            bed.localPosition = new Vector3(0f, 0.62f, 0f);
            bed.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            BuildKit.Shape(PrimitiveType.Cube, "Boards", bed, Vector3.zero, new Vector3(1.4f, 0.1f, 2.2f), wood);
            foreach (float side in new[] { -1f, 1f })
            {
                BuildKit.Shape(PrimitiveType.Cube, "Side", bed, new Vector3(side * 0.68f, 0.25f, 0f), new Vector3(0.07f, 0.42f, 2.2f), wood);
                BuildKit.Shape(PrimitiveType.Cube, "Shaft", bed, new Vector3(side * 0.5f, -0.08f, 1.9f), new Vector3(0.08f, 0.08f, 1.7f), shaft);
            }
            BuildKit.Shape(PrimitiveType.Cube, "Tail", bed, new Vector3(0f, 0.25f, -1.08f), new Vector3(1.4f, 0.42f, 0.07f), wood);
            BuildKit.Shape(PrimitiveType.Cylinder, "Wheel", group, new Vector3(-0.82f, 0.55f, -0.2f), new Vector3(1.1f, 0.05f, 1.1f), wood, new Vector3(0f, 0f, 90f));
            BuildKit.Shape(PrimitiveType.Cylinder, "Hub", group, new Vector3(-0.86f, 0.55f, -0.2f), new Vector3(0.24f, 0.07f, 0.24f), band, new Vector3(0f, 0f, 90f));
            // The other wheel has come off and lies beside it.
            BuildKit.Shape(PrimitiveType.Cylinder, "Wheel", group, new Vector3(1.5f, 0.06f, 0.6f), new Vector3(1.1f, 0.05f, 1.1f), wood, new Vector3(4f, 0f, 0f));
            BuildKit.Shape(PrimitiveType.Cylinder, "Hub", group, new Vector3(1.5f, 0.1f, 0.6f), new Vector3(0.24f, 0.07f, 0.24f), band);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = new Vector3(0f, 0.6f, 0f);
            body.size = new Vector3(1.5f, 1.2f, 2.3f);
        }

        // A clump of tapering blades fanning out from a point, each seen from both sides.
        private static Mesh TuftMesh()
        {
            var random = new System.Random(3);
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            const int Blades = 9;
            for (int i = 0; i < Blades; i++)
            {
                float yaw = i * 360f / Blades + (float)random.NextDouble() * 30f;
                float lean = 12f + (float)random.NextDouble() * 34f;
                float height = 0.6f + (float)random.NextDouble() * 0.5f, width = 0.09f + (float)random.NextDouble() * 0.05f;
                var turn = Quaternion.Euler(0f, yaw, 0f);
                Vector3 root = turn * new Vector3(0f, 0f, 0.08f + (float)random.NextDouble() * 0.16f);
                Vector3 up = turn * Quaternion.Euler(lean, 0f, 0f) * Vector3.up;
                Vector3 side = turn * Vector3.right;
                // The blade bends over as it rises.
                Vector3 middle = root + up * (height * 0.55f);
                Vector3 tip = middle + (up + turn * Vector3.forward * 0.5f).normalized * (height * 0.45f);
                int start = vertices.Count;
                vertices.Add(root - side * width);
                vertices.Add(root + side * width);
                vertices.Add(middle - side * (width * 0.7f));
                vertices.Add(middle + side * (width * 0.7f));
                vertices.Add(tip);
                // Lit as if facing up, so a tuft is one flat tone and does not flicker as it turns.
                for (int n = 0; n < 5; n++) normals.Add(Vector3.up);
                int[] faces = { 0, 2, 1, 1, 2, 3, 2, 4, 3 };
                foreach (int index in faces) triangles.Add(start + index);
                for (int f = faces.Length - 1; f >= 0; f--) triangles.Add(start + faces[f]);
            }
            return SaveMesh("grass_tuft", vertices, normals, triangles);
        }

        // Small flat leaf shapes strewn over a disc of radius one, thicker toward the middle.
        private static Mesh ScatterMesh()
        {
            var random = new System.Random(8);
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i < 34; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f, reach = Mathf.Pow((float)random.NextDouble(), 0.7f);
                Vector3 centre = new Vector3(Mathf.Cos(angle) * reach, 0f, Mathf.Sin(angle) * reach);
                var turn = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                float length = 0.022f + (float)random.NextDouble() * 0.022f, width = length * 0.5f;
                int start = vertices.Count;
                vertices.Add(centre + turn * new Vector3(-length, 0f, 0f));
                vertices.Add(centre + turn * new Vector3(0f, 0f, width));
                vertices.Add(centre + turn * new Vector3(length, 0f, 0f));
                vertices.Add(centre + turn * new Vector3(0f, 0f, -width));
                for (int n = 0; n < 4; n++) normals.Add(Vector3.up);
                triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
            }
            return SaveMesh("leaf_scatter", vertices, normals, triangles);
        }

        private static Mesh SaveMesh(string name, List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
        {
            string folder = BuildKit.GeneratedRoot + "/Meshes";
            BuildKit.EnsureFolder(folder);
            string path = $"{folder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = name };
                AssetDatabase.CreateAsset(mesh, path);
            }
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            // The toon shader's outline reads the tangents; these shapes have none of their own.
            var tangents = new List<Vector4>();
            for (int i = 0; i < vertices.Count; i++) tangents.Add(new Vector4(0f, 1f, 0f, 1f));
            mesh.SetTangents(tangents);
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }
    }
}
