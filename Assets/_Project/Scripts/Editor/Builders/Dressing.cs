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
        private static Material stone, stoneDark, stonePale, footing, worn, moss, ivy, ivyDark, canvas, canvasDark, canvasStripe, rope, shide,
            scorch, ash, hollow, carved, carvedDark, carvedPale, slitWarm, slitCold, bark, foliage, foliageDark, shieldRed, shieldBlue, rock, far, mist, coil, bannerHeavy, bannerEdge;
        private static Mesh taperPoint, taperHalf, taperWide, star, ivyLeaves, edge, ridge;

        /// <summary>Makes the materials and shapes the pieces share. Call once before placing any.</summary>
        public static void Prepare()
        {
            grass = BuildKit.Toon(Folder, "m_dress_grass", new Color(0.2f, 0.4f, 0.3f), false, null, 0f, 0f);
            grassDark = BuildKit.Toon(Folder, "m_dress_grass_dark", new Color(0.17f, 0.34f, 0.31f), false, null, 0f, 0f);
            leafRed = BuildKit.Toon(Folder, "m_dress_leaf_red", new Color(0.58f, 0.17f, 0.15f), false, null, 0f, 0f);
            leafGold = BuildKit.Toon(Folder, "m_dress_leaf_gold", new Color(0.66f, 0.42f, 0.14f), false, null, 0f, 0f);
            // Standing water, drawn flat: a dark sheet, a paler line round it where the wet stone shows, and a glint of sky.
            // It takes light like the ground does, so a lantern's pool lies across it.
            puddle = BuildKit.Toon(Folder, "m_dress_water", new Color(0.07f, 0.085f, 0.19f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.008f, 0.012f, 0.04f));
            puddleRim = BuildKit.Toon(Folder, "m_dress_water_edge", new Color(0.2f, 0.22f, 0.36f), false, null, 0f, 0f);
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

            // Stone for set pieces, in the castle's own blues, and the muted things that grow on it and hang from it.
            var blocks = ProceduralTextures.WallBlocks();
            stone = BuildKit.Toon(Folder, "m_dress_stone", new Color(0.36f, 0.35f, 0.47f), false, blocks, 0.2f, 0.025f);
            stoneDark = BuildKit.Toon(Folder, "m_dress_stone_dark", new Color(0.22f, 0.21f, 0.32f), false, blocks, 0.2f, 0.025f);
            stonePale = BuildKit.Toon(Folder, "m_dress_stone_pale", new Color(0.46f, 0.46f, 0.58f), false, ProceduralTextures.Ground(), 0.5f, 0.02f);
            // Carved things are too small for the wall's blocks to be drawn on them: one plain tone each.
            carved = BuildKit.Toon(Folder, "m_dress_carved", new Color(0.37f, 0.36f, 0.49f), false, null, 0f, 0.025f);
            carvedDark = BuildKit.Toon(Folder, "m_dress_carved_dark", new Color(0.23f, 0.22f, 0.34f), false, null, 0f, 0.025f);
            carvedPale = BuildKit.Toon(Folder, "m_dress_carved_pale", new Color(0.47f, 0.47f, 0.6f), false, null, 0f, 0.02f);
            footing = BuildKit.Toon(Folder, "m_dress_footing", new Color(0.17f, 0.16f, 0.27f), false, blocks, 0.11f, 0.02f);
            worn = BuildKit.Toon(Folder, "m_dress_worn", new Color(0.23f, 0.23f, 0.35f), false, ProceduralTextures.Flagstones(), 0.3f, 0f);
            moss = BuildKit.Toon(Folder, "m_dress_moss", new Color(0.13f, 0.23f, 0.25f), false, null, 0f, 0f);
            ivy = BuildKit.Toon(Folder, "m_dress_ivy", new Color(0.14f, 0.3f, 0.24f), false, null, 0f, 0f);
            ivyDark = BuildKit.Toon(Folder, "m_dress_ivy_dark", new Color(0.1f, 0.2f, 0.21f), false, null, 0f, 0f);
            canvas = BuildKit.Toon(Folder, "m_dress_canvas", new Color(0.27f, 0.25f, 0.34f), false, null, 0f, 0.02f);
            canvasDark = BuildKit.Toon(Folder, "m_dress_canvas_dark", new Color(0.19f, 0.17f, 0.26f), false, null, 0f, 0.02f);
            canvasStripe = BuildKit.Toon(Folder, "m_dress_canvas_stripe", new Color(0.4f, 0.14f, 0.21f), false, null, 0f, 0f);
            rope = BuildKit.Toon(Folder, "m_dress_rope", new Color(0.5f, 0.42f, 0.32f), false, null, 0f, 0.015f);
            shide = BuildKit.Toon(Folder, "m_dress_paper_white", new Color(0.74f, 0.74f, 0.76f), false, null, 0f, 0.01f);
            scorch = BuildKit.Toon(Folder, "m_dress_scorch", new Color(0.06f, 0.055f, 0.11f), false, null, 0f, 0f);
            ash = BuildKit.Toon(Folder, "m_dress_ash", new Color(0.15f, 0.14f, 0.22f), false, null, 0f, 0f);
            hollow = BuildKit.Toon(Folder, "m_dress_hollow", new Color(0.035f, 0.03f, 0.075f), false, null, 0f, 0f);
            slitWarm = BuildKit.Toon(Folder, "m_dress_slit_warm", new Color(0.8f, 0.48f, 0.22f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(1.1f, 0.45f, 0.12f));
            slitCold = BuildKit.Toon(Folder, "m_dress_slit_cold", new Color(0.35f, 0.5f, 0.85f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.2f, 0.45f, 1.1f));
            bark = BuildKit.Toon(Folder, "m_dress_bark", new Color(0.21f, 0.16f, 0.22f), false, null, 0f, 0.02f);
            foliage = BuildKit.Toon(Folder, "m_dress_foliage", new Color(0.15f, 0.31f, 0.28f), false, null, 0f, 0.025f);
            foliageDark = BuildKit.Toon(Folder, "m_dress_foliage_dark", new Color(0.11f, 0.23f, 0.25f), false, null, 0f, 0.025f);
            shieldRed = BuildKit.Toon(Folder, "m_dress_shield_red", new Color(0.42f, 0.14f, 0.2f), false, null, 0f, 0.015f);
            shieldBlue = BuildKit.Toon(Folder, "m_dress_shield_blue", new Color(0.2f, 0.25f, 0.42f), false, null, 0f, 0.015f);
            rock = BuildKit.Toon(Folder, "m_dress_rock", new Color(0.17f, 0.16f, 0.28f), false, null, 0f, 0.03f);
            // What stands far off is one flat tone a step above the black of the sky; the fog does the rest.
            far = BuildKit.Toon(Folder, "m_dress_far", new Color(0.05f, 0.045f, 0.1f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.035f, 0.032f, 0.08f));
            mist = BuildKit.Glow(Folder, "m_dress_mist", new Color(0.13f, 0.14f, 0.32f, 0.55f), 0, false, 1f);
            coil = BuildKit.Toon(Folder, "m_dress_coil", new Color(0.55f, 0.75f, 1f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.6f, 1.4f, 3.4f));
            bannerHeavy = BuildKit.Toon(Folder, "m_dress_banner_heavy", new Color(0.4f, 0.09f, 0.17f), false, null, 0f, 0.015f);
            bannerEdge = BuildKit.Toon(Folder, "m_dress_banner_edge", new Color(0.17f, 0.14f, 0.27f), false, null, 0f, 0f);
            taperPoint = TaperMesh("taper_point", 0.02f);
            taperHalf = TaperMesh("taper_half", 0.5f);
            taperWide = TaperMesh("taper_wide", 0.75f);
            star = StarMesh();
            ivyLeaves = IvyMesh();
            edge = EdgeMesh();
            ridge = RidgeMesh();
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
                var rim = BuildKit.Shape(PrimitiveType.Cylinder, "Edge", group, offset + Vector3.up * 0.014f, new Vector3(width + 0.09f, 0.001f, depth + 0.09f), puddleRim, new Vector3(0f, yaw, 0f));
                rim.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rim.gameObject.isStatic = true;
                var sheet = BuildKit.Shape(PrimitiveType.Cylinder, "Water", group, offset + Vector3.up * 0.019f, new Vector3(width, 0.001f, depth), puddle, new Vector3(0f, yaw, 0f));
                sheet.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                sheet.gameObject.isStatic = true;
                // The sky in it: two or three short broken strokes, never one clean line.
                int strokes = 2 + random.Next(2);
                for (int stroke = 0; stroke < strokes; stroke++)
                {
                    var spot = offset + Quaternion.Euler(0f, yaw, 0f) * new Vector3(width * (-0.22f + stroke * 0.14f), 0.024f, depth * (0.2f - stroke * 0.12f - (float)random.NextDouble() * 0.05f));
                    var glint = BuildKit.Shape(PrimitiveType.Cylinder, "Glint", group, spot,
                        new Vector3(width * (0.1f + (float)random.NextDouble() * 0.16f), 0.001f, depth * 0.035f), puddleGlint, new Vector3(0f, yaw + 8f, 0f));
                    glint.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    glint.gameObject.isStatic = true;
                }
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

        /// <summary>
        /// Breaks the ruled edge of a rectangle of ground laid on another: rounds of the same ground spill over the
        /// line all the way round it, and grass grows along it.
        /// </summary>
        public static void RaggedEdge(Transform parent, float west, float east, float south, float north, float height, Material ground, int seed)
        {
            var random = new System.Random(seed);
            var group = BuildKit.Group("Ragged Edge", parent);
            void Along(Vector3 from, Vector3 to)
            {
                float length = (to - from).magnitude;
                int count = Mathf.Max(2, Mathf.RoundToInt(length / 1.1f));
                for (int i = 0; i < count; i++)
                {
                    Vector3 at = Vector3.Lerp(from, to, (i + (float)random.NextDouble()) / count);
                    float size = 0.9f + (float)random.NextDouble() * 1.5f;
                    var spill = BuildKit.Shape(PrimitiveType.Cylinder, "Spill", group, new Vector3(at.x, height + 0.004f, at.z),
                        new Vector3(size, 0.001f, size * (0.6f + (float)random.NextDouble() * 0.5f)), ground, new Vector3(0f, (float)random.NextDouble() * 180f, 0f));
                    spill.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    spill.gameObject.isStatic = true;
                    if (random.NextDouble() < 0.5) Tuft(group, new Vector3(at.x, height, at.z) + new Vector3(((float)random.NextDouble() - 0.5f) * 0.8f, 0f, ((float)random.NextDouble() - 0.5f) * 0.8f), 0.4f + (float)random.NextDouble() * 0.4f, seed * 90 + i);
                }
            }
            Along(new Vector3(west, 0f, south), new Vector3(east, 0f, south));
            Along(new Vector3(east, 0f, south), new Vector3(east, 0f, north));
            Along(new Vector3(east, 0f, north), new Vector3(west, 0f, north));
            Along(new Vector3(west, 0f, north), new Vector3(west, 0f, south));
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

        // ------------------------------------------------------------------ shorthand for composed pieces

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static float Next(System.Random random, float low, float high) => low + (float)random.NextDouble() * (high - low);

        private static Renderer Box(Transform parent, string name, Vector3 at, Vector3 size, Material material, Vector3? euler = null) =>
            BuildKit.Shape(PrimitiveType.Cube, name, parent, at, size, material, euler);

        // A drum of the given width and whole height (Unity's own cylinder is two high).
        private static Renderer Drum(Transform parent, string name, Vector3 at, float width, float height, Material material, Vector3? euler = null) =>
            BuildKit.Shape(PrimitiveType.Cylinder, name, parent, at, new Vector3(width, height * 0.5f, width), material, euler);

        private static Renderer Ball(Transform parent, string name, Vector3 at, Vector3 size, Material material, Vector3? euler = null) =>
            BuildKit.Shape(PrimitiveType.Sphere, name, parent, at, size, material, euler);

        /// <summary>A box whose top is narrower than its foot: 0 comes to a point, 0.5 and 0.75 are battered blocks.</summary>
        public static Renderer Taper(Transform parent, string name, Vector3 at, Vector3 size, Material material, float top, Vector3? euler = null)
        {
            var shape = Box(parent, name, at, size, material, euler);
            shape.GetComponent<MeshFilter>().sharedMesh = top < 0.25f ? taperPoint : top < 0.65f ? taperHalf : taperWide;
            return shape;
        }

        // A square bar from one point to another.
        private static Renderer Beam(Transform parent, string name, Vector3 from, Vector3 to, float width, float height, Material material)
        {
            Vector3 along = to - from;
            var beam = Box(parent, name, (from + to) * 0.5f, new Vector3(width, height, along.magnitude), material);
            Vector3 direction = along.normalized;
            beam.transform.localRotation = Mathf.Abs(direction.y) > 0.98f ? Quaternion.FromToRotation(Vector3.forward, direction) : Quaternion.LookRotation(direction, Vector3.up);
            return beam;
        }

        // A round bar from one point to another.
        private static Renderer Pole(Transform parent, string name, Vector3 from, Vector3 to, float width, Material material)
        {
            Vector3 along = to - from;
            var pole = Drum(parent, name, (from + to) * 0.5f, width, along.magnitude, material);
            pole.transform.localRotation = Quaternion.FromToRotation(Vector3.up, along.normalized);
            return pole;
        }

        // Where a piece stands and which way it faces. Pieces made for walls look out from the wall along their own forward.
        private static Transform Piece(string name, Transform parent, Vector3 foot, float yaw)
        {
            var group = BuildKit.Group(name, parent);
            group.SetPositionAndRotation(foot, Quaternion.Euler(0f, yaw, 0f));
            return group;
        }

        // Nothing in a finished piece moves; small and flat things throw no shadow of their own.
        private static void Settle(Transform piece, bool shadows = true)
        {
            foreach (var renderer in piece.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.gameObject.isStatic = true;
                if (!shadows) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        private static Light WarmLight(Transform parent, Vector3 at, float range, float intensity)
        {
            var lightObject = new GameObject("Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = at;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.6f, 0.26f);
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            return light;
        }

        // ------------------------------------------------------------------ walls and the ground at their feet

        /// <summary>A stepped buttress against a wall. Solid.</summary>
        public static void Buttress(Transform parent, Vector3 foot, float yaw, float height, float width = 1.3f, float depth = 0.9f)
        {
            var group = Piece("Buttress", parent, foot, yaw);
            Box(group, "Plinth", V(0f, 0.3f, depth * 0.5f + 0.06f), V(width + 0.3f, 0.6f, depth + 0.12f), stoneDark);
            Box(group, "Lower", V(0f, height * 0.3f, depth * 0.5f), V(width, height * 0.6f, depth), stone);
            Box(group, "Weathering", V(0f, height * 0.6f + 0.02f, depth * 0.56f), V(width + 0.12f, 0.14f, depth * 1.05f), stonePale, V(24f, 0f, 0f));
            Box(group, "Upper", V(0f, height * 0.8f, depth * 0.3f), V(width * 0.8f, height * 0.4f, depth * 0.6f), stone);
            Box(group, "Cap", V(0f, height + 0.02f, depth * 0.34f), V(width * 0.8f + 0.12f, 0.12f, depth * 0.7f), stonePale, V(24f, 0f, 0f));
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, height * 0.5f, depth * 0.5f);
            body.size = V(width, height, depth);
            Settle(group);
        }

        /// <summary>A course of bigger, darker stone along the foot of a wall face, with the court on the outward side.</summary>
        public static void Footing(Transform parent, Vector3 from, Vector3 to, Vector3 outward, float height = 0.5f, float depth = 0.22f)
        {
            float length = (to - from).magnitude;
            var group = Piece("Footing", parent, (from + to) * 0.5f, Mathf.Atan2(outward.x, outward.z) * Mathf.Rad2Deg);
            Box(group, "Course", V(0f, height * 0.5f, depth * 0.5f), V(length, height, depth), footing);
            Box(group, "Lip", V(0f, height + 0.03f, depth * 0.5f + 0.02f), V(length, 0.07f, depth + 0.06f), stoneDark);
            Settle(group, false);
        }

        /// <summary>
        /// Breaks the ruled line where a floor meets a wall: a ragged strip of darker, worn stones with moss creeping
        /// over it and fallen bits of the wall lying on it. Nothing here is solid.
        /// </summary>
        public static void FloorEdge(Transform parent, Vector3 from, Vector3 to, Vector3 outward, float width, int seed, bool mossy = false)
        {
            var random = new System.Random(seed);
            float length = (to - from).magnitude;
            var group = Piece("Floor Edge", parent, (from + to) * 0.5f, Mathf.Atan2(outward.x, outward.z) * Mathf.Rad2Deg);
            // The strip's pattern is laid end to end along a long wall so its stones stay the size of stones.
            int repeats = Mathf.Max(1, Mathf.RoundToInt(length / 17f));
            float span = length / repeats;
            for (int i = 0; i < repeats; i++)
            {
                float x = -length * 0.5f + span * (i + 0.5f);
                float flip = i % 2 == 0 ? 1f : -1f;
                Box(group, "Worn Stones", V(x, 0.006f, 0f), V(span * flip, 1f, width), mossy ? ivy : worn).GetComponent<MeshFilter>().sharedMesh = edge;
                Box(group, "Moss", V(x, 0.01f, 0f), V(-span * flip, 1f, width * 0.45f), mossy ? ivyDark : moss).GetComponent<MeshFilter>().sharedMesh = edge;
            }
            int stones = Mathf.RoundToInt(length / 3.4f);
            for (int i = 0; i < stones; i++)
            {
                float size = Next(random, 0.18f, 0.5f);
                Box(group, "Fallen Stone", V(Next(random, -0.5f, 0.5f) * (length - 0.6f), size * 0.3f, Next(random, 0.15f, width * 0.75f)),
                    V(size, size * 0.65f, size * 0.85f), i % 3 == 0 ? stoneDark : stone, V(Next(random, -10f, 10f), Next(random, 0f, 360f), Next(random, -10f, 10f)));
            }
            Settle(group, false);
        }

        /// <summary>An arrow slit in a wall face. Glow: 0 dark, 1 a warm light behind it, 2 a cold one.</summary>
        public static void ArrowSlit(Transform parent, Vector3 centre, float yaw, int glow)
        {
            var group = Piece("Arrow Slit", parent, centre, yaw);
            Box(group, "Slit", V(0f, 0f, 0.02f), V(0.3f, 1.3f, 0.05f), hollow);
            Box(group, "Cross Slit", V(0f, 0.18f, 0.02f), V(0.72f, 0.2f, 0.05f), hollow);
            if (glow > 0) Box(group, "Light Behind", V(0f, -0.08f, 0.035f), V(0.13f, 0.95f, 0.04f), glow == 1 ? slitWarm : slitCold);
            Box(group, "Lintel", V(0f, 0.76f, 0.07f), V(0.7f, 0.18f, 0.16f), stonePale);
            Box(group, "Sill", V(0f, -0.74f, 0.09f), V(0.6f, 0.12f, 0.2f), stonePale, V(18f, 0f, 0f));
            Settle(group, false);
        }

        /// <summary>A small shrine cut into a wall: a worn stone figure under a little roof, with a candle left burning.</summary>
        public static void Niche(Transform parent, Vector3 sill, float yaw)
        {
            var group = Piece("Wall Shrine", parent, sill, yaw);
            Box(group, "Recess", V(0f, 0.55f, 0.02f), V(0.9f, 1.1f, 0.05f), hollow);
            // The candle lights the back of the recess, and the figure stands dark against it.
            Box(group, "Candlelight", V(0.08f, 0.42f, 0.035f), V(0.62f, 0.72f, 0.04f), slitWarm);
            Box(group, "Sill", V(0f, 0f, 0.16f), V(1.15f, 0.12f, 0.34f), carvedPale);
            Taper(group, "Roof", V(0f, 1.26f, 0.14f), V(1.3f, 0.34f, 0.36f), carvedPale, 0.5f);
            Drum(group, "Figure", V(-0.08f, 0.3f, 0.14f), 0.3f, 0.5f, carvedDark);
            Ball(group, "Figure Head", V(-0.08f, 0.66f, 0.14f), V(0.26f, 0.26f, 0.26f), carvedDark);
            Box(group, "Bib", V(-0.08f, 0.48f, 0.27f), V(0.24f, 0.14f, 0.03f), canvasStripe);
            Drum(group, "Candle", V(0.32f, 0.13f, 0.2f), 0.07f, 0.14f, shide);
            Taper(group, "Candle Flame", V(0.32f, 0.27f, 0.2f), V(0.08f, 0.14f, 0.08f), paper, 0f);
            Settle(group, false);
        }

        /// <summary>A torch in an iron bracket on a wall: a small warm light. The flame's place is returned for a fire to be lit there.</summary>
        public static Transform WallTorch(Transform parent, Vector3 at, float yaw)
        {
            var group = Piece("Wall Torch", parent, at, yaw);
            Box(group, "Plate", V(0f, 0f, 0.03f), V(0.24f, 0.55f, 0.06f), iron);
            Beam(group, "Arm", V(0f, -0.15f, 0.04f), V(0f, 0.1f, 0.5f), 0.08f, 0.08f, iron);
            Beam(group, "Stay", V(0f, 0.2f, 0.04f), V(0f, 0.12f, 0.42f), 0.05f, 0.05f, iron);
            Drum(group, "Cup", V(0f, 0.14f, 0.5f), 0.28f, 0.12f, iron);
            Drum(group, "Torch", V(0f, 0.36f, 0.5f), 0.13f, 0.4f, wood);
            Drum(group, "Pitch", V(0f, 0.6f, 0.5f), 0.2f, 0.14f, band);
            var ember = Taper(group, "Flame Heart", V(0f, 0.8f, 0.5f), V(0.2f, 0.34f, 0.2f), paper, 0f, V(0f, 45f, 0f));
            ember.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            WarmLight(group, V(0f, 0.9f, 0.9f), 8.5f, 12f);
            var tip = BuildKit.Group("Flame Point", group);
            tip.localPosition = V(0f, 0.68f, 0.5f);
            return tip;
        }

        /// <summary>Ivy on a wall face, as flat dark leaves climbing from the foot.</summary>
        public static void IvyPatch(Transform parent, Vector3 foot, float yaw, float width, float height, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Ivy", parent, foot, yaw);
            float flip = random.Next(2) == 0 ? 1f : -1f;
            Box(group, "Old Growth", V(0f, 0f, 0.03f), V(width * flip, height, 1f), ivyDark).GetComponent<MeshFilter>().sharedMesh = ivyLeaves;
            Box(group, "New Growth", V(Next(random, -0.2f, 0.2f) * width, 0f, 0.05f), V(-width * 0.7f * flip, height * Next(random, 0.6f, 0.8f), 1f), ivy)
                .GetComponent<MeshFilter>().sharedMesh = ivyLeaves;
            Settle(group, false);
        }

        /// <summary>A block gone from a wall face, the cracks running from the hole, and the block itself lying below.</summary>
        public static void MissingBlock(Transform parent, Vector3 centre, float yaw, float drop, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Missing Block", parent, centre, yaw);
            float width = Next(random, 0.8f, 1.1f), height = Next(random, 0.5f, 0.65f);
            Box(group, "Hole", V(0f, 0f, 0.015f), V(width, height, 0.04f), hollow);
            Box(group, "Chipped Corner", V(width * 0.55f, -height * 0.4f, 0.015f), V(width * 0.5f, height * 0.5f, 0.04f), hollow, V(0f, 0f, 9f));
            Vector3 at = V(-width * 0.3f, -height * 0.5f, 0.015f);
            for (int i = 0; i < 3; i++)
            {
                Vector3 next = at + V(Next(random, -0.35f, 0.35f), -Next(random, 0.35f, 0.6f), 0f);
                Beam(group, "Crack", at, next, 0.05f - i * 0.01f, 0.03f, hollow);
                at = next;
            }
            Box(group, "Fallen Block", V(Next(random, -0.5f, 0.5f), -drop + height * 0.42f, Next(random, 0.6f, 1f)), V(width * 0.9f, height * 0.85f, 0.5f), stone,
                V(Next(random, -8f, 8f), Next(random, 20f, 70f), 0f));
            Settle(group, false);
        }

        // ------------------------------------------------------------------ the causeway

        /// <summary>Hewn stones thrown down from a broken wall: bigger and squarer than rubble. Not solid.</summary>
        public static void FallenStones(Transform parent, Vector3 centre, int count, float spread, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Fallen Stones", parent, centre, 0f);
            for (int i = 0; i < count; i++)
            {
                float size = Next(random, 0.45f, 0.75f);
                Box(group, "Hewn Stone", V(Next(random, -spread, spread), size * 0.3f, Next(random, -spread, spread) * 1.6f), V(size * 1.3f, size * 0.62f, size * 0.8f),
                    i % 2 == 0 ? stone : stoneDark, V(Next(random, -14f, 14f), Next(random, 0f, 360f), Next(random, -14f, 14f)));
            }
            // One still wears a piece of the wall's coping.
            Box(group, "Coping", V(spread * 0.4f, 0.1f, -spread), V(0.9f, 0.2f, 0.6f), stonePale, V(0f, 38f, 8f));
            Settle(group);
        }

        /// <summary>An iron chain hanging between two points, its middle sagging by the given drop. Thin, and not solid.</summary>
        public static void Chain(Transform parent, Vector3 from, Vector3 to, float sag, int links)
        {
            var group = BuildKit.Group("Chain", parent);
            Vector3 last = from;
            for (int i = 1; i <= links; i++)
            {
                float t = i / (float)links;
                Vector3 at = Vector3.Lerp(from, to, t) + Vector3.down * (sag * 4f * t * (1f - t));
                // Links turn a quarter each time, as chain does.
                Beam(group, "Link", last, Vector3.Lerp(last, at, 1.12f), i % 2 == 0 ? 0.11f : 0.045f, i % 2 == 0 ? 0.045f : 0.11f, iron);
                last = at;
            }
            Settle(group, false);
        }

        /// <summary>
        /// A stone guardian on a plinth, leaning on its sword and looking along its own forward. Toppled, it has broken at
        /// the ankles and the waist and lies in pieces in front of the plinth. The plinth is solid, and so is the fallen trunk.
        /// </summary>
        public static void Statue(Transform parent, Vector3 foot, float yaw, bool toppled)
        {
            var group = Piece(toppled ? "Fallen Statue" : "Statue", parent, foot, yaw);
            Box(group, "Plinth Base", V(0f, 0.12f, 0f), V(1.5f, 0.24f, 1.5f), carvedDark);
            Box(group, "Plinth", V(0f, 0.5f, 0f), V(1.2f, 0.56f, 1.2f), carved);
            Box(group, "Plinth Cap", V(0f, 0.84f, 0f), V(1.4f, 0.14f, 1.4f), carvedPale);
            const float Top = 0.91f, Size = 0.82f;
            var plinth = group.gameObject.AddComponent<BoxCollider>();
            plinth.center = V(0f, 0.45f, 0f);
            plinth.size = V(1.4f, 0.9f, 1.4f);

            var legs = BuildKit.Group("Legs", group);
            Taper(legs, "Robe", V(0f, 0.6f, 0f), V(0.95f, 1.2f, 0.72f), carved, 0.75f);
            Box(legs, "Belt", V(0f, 1.2f, 0f), V(0.82f, 0.14f, 0.62f), carvedDark);
            var upper = BuildKit.Group("Trunk", group);
            Box(upper, "Chest", V(0f, 0.4f, 0f), V(0.85f, 0.75f, 0.5f), carved);
            Box(upper, "Shoulders", V(0f, 0.8f, 0f), V(1.45f, 0.26f, 0.62f), carvedPale);
            Box(upper, "Arms", V(0f, 0.4f, 0.3f), V(1.15f, 0.3f, 0.26f), carvedPale);
            Box(upper, "Hilt", V(0f, 0.28f, 0.44f), V(0.56f, 0.1f, 0.1f), carvedDark);
            var head = BuildKit.Group("Head", group);
            Ball(head, "Face", V(0f, 0.22f, 0f), V(0.46f, 0.5f, 0.46f), carved);
            Box(head, "Helm Brim", V(0f, 0.38f, 0f), V(0.66f, 0.09f, 0.66f), carvedPale);
            Box(head, "Crest", V(0f, 0.58f, 0f), V(0.1f, 0.32f, 0.56f), carvedDark);
            legs.localScale = upper.localScale = head.localScale = Vector3.one * Size;

            if (!toppled)
            {
                legs.localPosition = V(0f, Top, 0f);
                upper.localPosition = V(0f, Top + 1.27f * Size, 0f);
                head.localPosition = V(0f, Top + 2.2f * Size, 0f);
                Box(group, "Sword", V(0f, Top + 0.78f * Size, 0.45f * Size), V(0.13f, 1.56f * Size, 0.07f), carvedDark);
                // A round shield slung on the left shoulder: from above it is the one shape that says this is a figure.
                Drum(group, "Shield", V(-0.62f, Top + 1.55f * Size, 0.1f), 0.86f, 0.1f, carvedDark, V(62f, -35f, 0f));
                Drum(group, "Shield Boss", V(-0.66f, Top + 1.58f * Size, 0.16f), 0.3f, 0.12f, carvedPale, V(62f, -35f, 0f));
            }
            else
            {
                Box(group, "Ankles", V(0f, Top + 0.11f, 0f), V(0.6f, 0.22f, 0.42f), carved, V(0f, 0f, 7f));
                // It lies on its back beside the plinth, heading away along the plinth's right: broken at the ankles, the
                // waist and the neck, the pieces a little apart, so that from above it is still plainly a figure.
                var lie = Quaternion.Euler(0f, -90f, 0f) * Quaternion.Euler(-90f, 0f, 0f);
                legs.localPosition = V(0.45f, 0.3f, 1.3f);
                legs.localRotation = Quaternion.Euler(0f, -6f, 0f) * lie;
                upper.localPosition = V(1.6f, 0.3f, 1.42f);
                upper.localRotation = Quaternion.Euler(0f, 12f, 0f) * lie;
                head.localPosition = V(2.6f, 0.2f, 1.1f);
                head.localRotation = Quaternion.Euler(0f, -35f, 0f) * lie * Quaternion.Euler(0f, 0f, -24f);
                Box(group, "Broken Sword", V(1.6f, 0.04f, 0.7f), V(1.1f, 0.07f, 0.13f), carvedDark, V(0f, 14f, 0f));
                Box(group, "Sword Point", V(0.3f, 0.04f, 2.1f), V(0.5f, 0.07f, 0.13f), carvedDark, V(0f, -50f, 0f));
                foreach (var part in new[] { legs, upper, head })
                    foreach (var renderer in part.GetComponentsInChildren<MeshRenderer>())
                        if (renderer.sharedMaterial == carved) renderer.sharedMaterial = carvedPale;
                var trunk = upper.gameObject.AddComponent<BoxCollider>();
                trunk.center = V(0f, 0.4f, 0.1f);
                trunk.size = V(1.4f, 1f, 0.7f);
            }
            Settle(group);
        }

        /// <summary>A crag standing up out of the dark below, seen from above. Far off and never walked on.</summary>
        public static void Crag(Transform parent, Vector3 foot, float width, float height, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Crag", parent, foot, Next(random, 0f, 360f));
            Taper(group, "Rock", V(0f, height * 0.5f, 0f), V(width, height, width * 0.8f), rock, 0.5f);
            Taper(group, "Shoulder", V(width * 0.55f, height * 0.36f, width * 0.2f), V(width * 0.7f, height * 0.72f, width * 0.7f), rock, 0f, V(0f, 30f, 0f));
            Taper(group, "Shoulder", V(-width * 0.4f, height * 0.3f, -width * 0.35f), V(width * 0.6f, height * 0.6f, width * 0.6f), rock, 0.5f, V(0f, -20f, 0f));
            Settle(group, false);
        }

        /// <summary>A sheet of mist lying far below.</summary>
        public static void Mist(Transform parent, Vector3 at, float width, float depth, bool upright = false)
        {
            var sheet = BuildKit.Shape(PrimitiveType.Quad, "Mist", parent, at, V(width, depth, 1f), mist, upright ? Vector3.zero : V(90f, 0f, 0f));
            sheet.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sheet.receiveShadows = false;
        }

        /// <summary>An iron gate grille torn from its arch and left lying on the ground. Not solid.</summary>
        public static void Portcullis(Transform parent, Vector3 foot, float yaw)
        {
            var group = Piece("Fallen Portcullis", parent, foot, yaw);
            group.localRotation *= Quaternion.Euler(0f, 0f, 3f);
            for (int i = 0; i < 5; i++)
            {
                float x = -1.2f + i * 0.6f;
                Box(group, "Bar", V(x, 0.07f, 0f), V(0.1f, 0.1f, 3.3f), iron);
                Taper(group, "Spike", V(x, 0.07f, 1.83f), V(0.14f, 0.36f, 0.12f), iron, 0f, V(90f, 0f, 0f));
            }
            for (int i = 0; i < 4; i++) Box(group, "Cross Bar", V(0f, 0.11f, -1.3f + i * 0.85f), V(2.8f, 0.07f, 0.1f), iron);
            Settle(group, false);
        }

        // ------------------------------------------------------------------ the garrison's yard

        // A round shield lying with its face along the group's up.
        private static Transform Shield(Transform parent, Vector3 at, Vector3 euler, Material face, float size = 0.84f)
        {
            var shield = BuildKit.Group("Shield", parent);
            shield.localPosition = at;
            shield.localRotation = Quaternion.Euler(euler);
            Drum(shield, "Rim", Vector3.zero, size + 0.09f, 0.04f, band);
            Drum(shield, "Face", V(0f, 0.025f, 0f), size, 0.04f, face);
            Ball(shield, "Boss", V(0f, 0.05f, 0f), V(size * 0.3f, 0.12f, size * 0.3f), iron);
            return shield;
        }

        /// <summary>A rack of spears against a wall, with shields leaning at its foot. Solid.</summary>
        public static void WeaponRack(Transform parent, Vector3 foot, float yaw, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Weapon Rack", parent, foot, yaw);
            foreach (float x in new[] { -1.15f, 1.15f })
            {
                Box(group, "Post", V(x, 0.85f, 0.3f), V(0.13f, 1.7f, 0.13f), wood);
                Box(group, "Foot", V(x, 0.06f, 0.42f), V(0.16f, 0.12f, 0.9f), wood);
                Beam(group, "Brace", V(x, 0.1f, 0.8f), V(x, 0.95f, 0.32f), 0.07f, 0.07f, wood);
            }
            Box(group, "Top Rail", V(0f, 1.55f, 0.3f), V(2.6f, 0.1f, 0.1f), wood);
            Box(group, "Low Rail", V(0f, 0.45f, 0.36f), V(2.4f, 0.08f, 0.08f), wood);
            int missing = random.Next(5);
            for (int i = 0; i < 5; i++)
            {
                // One spear has been taken, and not put back.
                if (i == missing) continue;
                float x = -0.92f + i * 0.46f;
                Vector3 butt = V(x + Next(random, -0.08f, 0.08f), 0f, Next(random, 0.62f, 0.76f));
                Vector3 tip = V(x + Next(random, -0.16f, 0.16f), Next(random, 2.2f, 2.5f), 0.2f);
                Vector3 neck = Vector3.Lerp(butt, tip, 0.87f);
                Beam(group, "Shaft", butt, neck, 0.05f, 0.05f, shaft);
                Beam(group, "Head", neck, tip, 0.1f, 0.025f, steel);
            }
            Shield(group, V(-0.55f, 0.4f, 0.74f), V(68f, 0f, 0f), shieldRed);
            Shield(group, V(0.42f, 0.4f, 0.78f), V(64f, 0f, 10f), shieldBlue);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 0.85f, 0.45f);
            body.size = V(2.6f, 1.7f, 0.95f);
            Settle(group);
        }

        /// <summary>Shields stacked and left, a helmet on top of them. Low, and not solid.</summary>
        public static void ShieldStack(Transform parent, Vector3 foot, float yaw, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Shield Stack", parent, foot, yaw);
            for (int i = 0; i < 4; i++)
                Shield(group, V(Next(random, -0.1f, 0.1f), 0.03f + i * 0.085f, Next(random, -0.1f, 0.1f)), V(Next(random, -5f, 5f), Next(random, 0f, 360f), Next(random, -5f, 5f)),
                    i % 2 == 0 ? shieldBlue : shieldRed);
            Shield(group, V(0.72f, 0.36f, 0.1f), V(0f, 0f, -62f), shieldRed);
            Shield(group, V(-0.2f, 0.03f, 1f), V(3f, 40f, 0f), shieldBlue, 0.76f);
            Ball(group, "Helmet", V(0.05f, 0.5f, 0f), V(0.36f, 0.3f, 0.4f), band, V(0f, 30f, 12f));
            Box(group, "Helmet Crest", V(0.05f, 0.66f, 0f), V(0.05f, 0.12f, 0.36f), canvasStripe, V(0f, 30f, 12f));
            Settle(group);
        }

        /// <summary>A striking post dressed as a man: straw bound to a stake, a crossbar for arms, an old helmet. Solid.</summary>
        public static void StrikingPost(Transform parent, Vector3 foot, float yaw, bool shield)
        {
            var group = Piece("Striking Post", parent, foot, yaw);
            Box(group, "Foot", V(0f, 0.07f, 0f), V(1.2f, 0.14f, 0.18f), wood);
            Box(group, "Foot", V(0f, 0.07f, 0f), V(0.18f, 0.14f, 1.2f), wood);
            Drum(group, "Stake", V(0f, 0.85f, 0f), 0.2f, 1.7f, wood);
            Drum(group, "Straw", V(0f, 1.1f, 0f), 0.46f, 0.8f, sack);
            Drum(group, "Binding", V(0f, 0.86f, 0f), 0.5f, 0.07f, rope);
            Drum(group, "Binding", V(0f, 1.34f, 0f), 0.5f, 0.07f, rope);
            Box(group, "Arms", V(0f, 1.48f, 0f), V(1.4f, 0.1f, 0.1f), wood, V(0f, 0f, 5f));
            Ball(group, "Helmet", V(0f, 1.82f, 0f), V(0.38f, 0.34f, 0.4f), band);
            Box(group, "Helmet Brim", V(0f, 1.72f, 0f), V(0.5f, 0.05f, 0.52f), band);
            // Hacked at for years: pale cuts in the straw.
            Box(group, "Cut", V(0.1f, 1.2f, -0.23f), V(0.3f, 0.04f, 0.03f), shide, V(0f, 0f, 28f));
            Box(group, "Cut", V(-0.08f, 1f, -0.23f), V(0.26f, 0.04f, 0.03f), shide, V(0f, 0f, -20f));
            if (shield) Shield(group, V(-0.66f, 1.2f, -0.08f), V(-90f, 0f, 0f), shieldRed, 0.7f);
            var body = group.gameObject.AddComponent<CapsuleCollider>();
            body.radius = 0.3f;
            body.height = 1.9f;
            body.center = V(0f, 0.95f, 0f);
            Settle(group);
        }

        /// <summary>
        /// A lean-to against a wall: posts, a beam, and canvas that the wind has torn, with a bench and bedrolls under it.
        /// Solid over its whole floor, so that nobody is ever standing under the cloth where the camera cannot see.
        /// </summary>
        public static void Awning(Transform parent, Vector3 foot, float yaw, float length)
        {
            var group = Piece("Awning", parent, foot, yaw);
            const float Depth = 2.3f, Back = 2.45f, Front = 1.9f;
            float half = length * 0.5f, bay = length / 3f;
            float slope = Mathf.Atan2(Back - Front, Depth) * Mathf.Rad2Deg;
            Box(group, "Wall Rail", V(0f, Back, 0.08f), V(length, 0.12f, 0.12f), wood);
            Box(group, "Front Beam", V(0f, Front, Depth), V(length + 0.3f, 0.12f, 0.12f), wood);
            for (int i = 0; i < 4; i++)
            {
                float x = -half + i * bay;
                if (i == 0 || i == 3) x += i == 0 ? 0.1f : -0.1f;
                Box(group, "Post", V(x, Front * 0.5f, Depth), V(0.14f, Front, 0.14f), wood, V(0f, 0f, i == 2 ? 4f : 0f));
                Beam(group, "Rafter", V(x, Back, 0.08f), V(x, Front + 0.04f, Depth), 0.08f, 0.08f, wood);
            }
            // Three cloths: one whole, one torn off halfway with its rag hanging from the beam, one whole with a faded stripe.
            Box(group, "Cloth", V(-bay, (Back + Front) * 0.5f + 0.07f, Depth * 0.5f + 0.05f), V(bay - 0.08f, 0.04f, Depth + 0.25f), canvas, V(slope, 0f, 0f));
            Box(group, "Torn Cloth", V(-0.15f, Back - 0.08f, Depth * 0.24f), V(bay - 0.4f, 0.04f, Depth * 0.45f), canvasDark, V(slope, 0f, 0f));
            Box(group, "Rag", V(0.3f, Front - 0.42f, Depth + 0.02f), V(bay * 0.45f, 0.8f, 0.03f), canvasDark, V(0f, 0f, 6f));
            Box(group, "Rag", V(-0.5f, Front - 0.25f, Depth + 0.02f), V(bay * 0.2f, 0.45f, 0.03f), canvasDark, V(0f, 0f, -8f));
            Box(group, "Cloth", V(bay, (Back + Front) * 0.5f + 0.07f, Depth * 0.5f + 0.05f), V(bay - 0.08f, 0.04f, Depth + 0.25f), canvas, V(slope, 0f, 0f));
            Box(group, "Stripe", V(bay, (Back + Front) * 0.5f + 0.1f, Depth * 0.5f + 0.05f), V(bay * 0.28f, 0.03f, Depth + 0.27f), canvasStripe, V(slope, 0f, 0f));
            // What it sheltered, seen through the tear.
            Box(group, "Bench", V(-0.1f, 0.45f, 0.7f), V(bay * 0.9f, 0.08f, 0.5f), wood);
            Box(group, "Bench Leg", V(-0.1f - bay * 0.38f, 0.22f, 0.7f), V(0.08f, 0.44f, 0.46f), wood);
            Box(group, "Bench Leg", V(-0.1f + bay * 0.38f, 0.22f, 0.7f), V(0.08f, 0.44f, 0.46f), wood);
            Drum(group, "Bedroll", V(-0.3f, 0.62f, 0.7f), 0.3f, 0.9f, canvasStripe, V(0f, 12f, 90f));
            Drum(group, "Bedroll", V(0.5f, 0.17f, 1.5f), 0.34f, 1f, canvas, V(0f, -30f, 90f));
            Box(group, "Guy Rope Peg", V(half + 0.5f, 0.1f, Depth + 0.7f), V(0.07f, 0.24f, 0.07f), wood, V(-20f, 0f, 20f));
            Beam(group, "Guy Rope", V(half + 0.1f, Front, Depth), V(half + 0.5f, 0.16f, Depth + 0.7f), 0.03f, 0.03f, rope);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 1f, Depth * 0.5f + 0.05f);
            body.size = V(length, 2f, Depth + 0.2f);
            Settle(group);
        }

        /// <summary>
        /// The yard's well: a round stone kerb on a step, a windlass between two posts, a bucket left on the rim and a lamp
        /// hung from the beam. Solid. Returns the lamp's light.
        /// </summary>
        public static Light Well(Transform parent, Vector3 foot, float yaw)
        {
            var group = Piece("Well", parent, foot, yaw);
            Drum(group, "Step", V(0f, 0.08f, 0f), 3.3f, 0.16f, stoneDark);
            Drum(group, "Kerb", V(0f, 0.55f, 0f), 2.2f, 0.9f, stone);
            Drum(group, "Rim", V(0f, 1.03f, 0f), 2.42f, 0.12f, stonePale);
            Drum(group, "Shaft", V(0f, 1.095f, 0f), 1.7f, 0.01f, hollow).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Drum(group, "Water", V(0.1f, 1.1f, -0.15f), 1.1f, 0.01f, puddle, V(0f, 20f, 0f)).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Drum(group, "Glint", V(0.2f, 1.105f, -0.3f), 0.07f, 0.01f, puddleGlint).transform.localScale = V(0.5f, 0.005f, 0.06f);
            foreach (float x in new[] { -1.22f, 1.22f })
            {
                Box(group, "Post", V(x, 1.2f, 0f), V(0.17f, 2.4f, 0.17f), wood);
                Beam(group, "Brace", V(x, 1.75f, 0f), V(x * 0.62f, 2.34f, 0f), 0.09f, 0.09f, wood);
            }
            Box(group, "Beam", V(0f, 2.42f, 0f), V(3.3f, 0.15f, 0.17f), wood);
            Drum(group, "Windlass", V(0f, 1.85f, 0f), 0.22f, 2.3f, wood, V(0f, 0f, 90f));
            Box(group, "Crank", V(1.4f, 1.72f, 0f), V(0.06f, 0.4f, 0.06f), iron);
            Box(group, "Crank Handle", V(1.5f, 1.55f, 0f), V(0.26f, 0.06f, 0.06f), iron);
            Drum(group, "Rope Turns", V(-0.1f, 1.85f, 0f), 0.28f, 0.5f, rope, V(0f, 0f, 90f));
            Box(group, "Rope", V(-0.1f, 1.45f, 0.1f), V(0.035f, 0.8f, 0.035f), rope);
            Drum(group, "Bucket", V(0.72f, 1.25f, -0.74f), 0.36f, 0.34f, wood, V(0f, 0f, 6f));
            Drum(group, "Bucket Hoop", V(0.72f, 1.3f, -0.74f), 0.39f, 0.05f, band, V(0f, 0f, 6f));
            // The lamp hangs outside the west post, where it lights the kerb and the ground before it.
            Box(group, "Lamp Chain", V(-1.5f, 2.22f, 0f), V(0.03f, 0.26f, 0.03f), iron);
            Box(group, "Lamp Roof", V(-1.5f, 2.08f, 0f), V(0.4f, 0.07f, 0.4f), iron, V(0f, 45f, 0f));
            Box(group, "Lamp", V(-1.5f, 1.87f, 0f), V(0.25f, 0.34f, 0.25f), paper).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Box(group, "Lamp Floor", V(-1.5f, 1.67f, 0f), V(0.32f, 0.05f, 0.32f), iron);
            var light = WarmLight(group, V(-1.5f, 2f, -0.3f), 9f, 15f);
            var body = group.gameObject.AddComponent<CapsuleCollider>();
            body.radius = 1.3f;
            body.height = 2.6f;
            body.center = V(0f, 1.2f, 0f);
            Settle(group);
            return light;
        }

        /// <summary>A plank trough of dark water. Solid, and low.</summary>
        public static void Trough(Transform parent, Vector3 foot, float yaw)
        {
            var group = Piece("Trough", parent, foot, yaw);
            Taper(group, "Trough", V(0f, 0.38f, 0f), V(2.1f, 0.5f, 0.8f), wood, 0.75f, V(180f, 0f, 0f));
            Box(group, "Water", V(0f, 0.64f, 0f), V(1.9f, 0.01f, 0.62f), puddle).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Box(group, "Glint", V(-0.3f, 0.648f, 0.05f), V(0.5f, 0.005f, 0.04f), puddleGlint, V(0f, 8f, 0f));
            foreach (float x in new[] { -0.8f, 0.8f }) Box(group, "Trestle", V(x, 0.14f, 0f), V(0.14f, 0.28f, 0.95f), wood);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 0.35f, 0f);
            body.size = V(2.1f, 0.7f, 0.85f);
            Settle(group);
        }

        // ------------------------------------------------------------------ the shrine

        /// <summary>
        /// A stone lantern: foot, shaft, a fire box with paper windows under a broad roof. Lit, it throws a small warm pool.
        /// Toppled, its parts lie where they fell and it is dark. Returns the light, or null.
        /// </summary>
        public static Light Toro(Transform parent, Vector3 foot, float yaw, bool lit, bool toppled = false)
        {
            var group = Piece(toppled ? "Fallen Stone Lantern" : "Stone Lantern", parent, foot, yaw);
            Taper(group, "Foot", V(0f, 0.16f, 0f), V(0.95f, 0.32f, 0.95f), carvedDark, 0.75f);
            var body = group.gameObject.AddComponent<CapsuleCollider>();
            body.radius = 0.42f;
            if (toppled)
            {
                Drum(group, "Shaft Stump", V(0f, 0.42f, 0f), 0.34f, 0.24f, carved, V(0f, 0f, 8f));
                Drum(group, "Shaft", V(0.85f, 0.17f, 0.15f), 0.34f, 0.7f, carved, V(0f, 20f, 90f));
                Box(group, "Fire Box", V(1.55f, 0.28f, -0.35f), V(0.56f, 0.56f, 0.56f), carvedDark, V(14f, 25f, 0f));
                Box(group, "Dark Window", V(1.55f, 0.3f, -0.35f), V(0.6f, 0.34f, 0.26f), hollow, V(14f, 25f, 0f));
                Taper(group, "Roof", V(1.1f, 0.42f, 0.95f), V(1.05f, 0.5f, 1.05f), carved, 0f, V(0f, 15f, -62f));
                Ball(group, "Finial", V(2.1f, 0.11f, 0.5f), V(0.22f, 0.22f, 0.22f), carvedPale);
                body.height = 0.9f;
                body.center = V(0f, 0.3f, 0f);
                Settle(group);
                return null;
            }
            Drum(group, "Shaft", V(0f, 0.62f, 0f), 0.34f, 0.64f, carved);
            Taper(group, "Table", V(0f, 1f, 0f), V(0.9f, 0.16f, 0.9f), carvedPale, 0.75f, V(180f, 0f, 0f));
            // The roof is kept narrow so the camera, looking down from the south, still sees the lit windows under it.
            Box(group, "Fire Box", V(0f, 1.46f, 0f), V(0.56f, 0.76f, 0.56f), carvedDark);
            var window = lit ? paper : hollow;
            Box(group, "Window", V(0f, 1.44f, 0f), V(0.6f, 0.54f, 0.3f), window).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Box(group, "Window", V(0f, 1.44f, 0f), V(0.3f, 0.54f, 0.6f), window).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Box(group, "Eaves", V(0f, 1.88f, 0f), V(1.06f, 0.09f, 1.06f), carvedPale);
            Taper(group, "Roof", V(0f, 2.14f, 0f), V(0.8f, 0.44f, 0.8f), carvedDark, 0f);
            Ball(group, "Finial", V(0f, 2.4f, 0f), V(0.22f, 0.26f, 0.22f), carvedPale);
            body.height = 2.4f;
            body.center = V(0f, 1.15f, 0f);
            Settle(group);
            return lit ? WarmLight(group, V(0f, 1.4f, -0.5f), 7.5f, 11f) : null;
        }

        /// <summary>
        /// A sacred rope strung between two points: thick straw, thicker in the middle, with folded paper streamers and
        /// a straw tassel hanging from it. Thin, and not solid.
        /// </summary>
        public static void Shimenawa(Transform parent, Vector3 from, Vector3 to, float sag)
        {
            var group = BuildKit.Group("Sacred Rope", parent);
            const int Lengths = 6;
            Vector3 At(float t) => Vector3.Lerp(from, to, t) + Vector3.down * (sag * 4f * t * (1f - t));
            for (int i = 0; i < Lengths; i++)
            {
                float middle = 1f - Mathf.Abs((i + 0.5f) / Lengths - 0.5f) * 2f;
                Pole(group, "Rope", At(i / (float)Lengths), At((i + 1.06f) / Lengths), 0.11f + middle * 0.08f, rope);
            }
            float yaw = Mathf.Atan2(to.x - from.x, to.z - from.z) * Mathf.Rad2Deg + 90f;
            foreach (float t in new[] { 0.27f, 0.73f })
            {
                // A streamer is one strip folded into a zigzag: three squares, each stepped aside from the one above.
                var streamer = BuildKit.Group("Streamer", group);
                streamer.localPosition = At(t);
                streamer.localRotation = Quaternion.Euler(0f, yaw, 0f);
                for (int fold = 0; fold < 3; fold++)
                    Box(streamer, "Fold", V(fold % 2 == 0 ? 0f : 0.12f, -0.24f - fold * 0.24f, 0f), V(0.24f, 0.27f, 0.02f), shide, V(0f, 0f, fold % 2 == 0 ? 8f : -8f));
            }
            Taper(group, "Tassel", At(0.5f) + Vector3.down * 0.3f, V(0.16f, 0.46f, 0.16f), rope, 0.5f, V(180f, 0f, 0f));
            Settle(group, false);
        }

        /// <summary>A low table of offerings: a bowl of fruit, a flask, a candle on a strip of red cloth. Solid, and small.</summary>
        public static void OfferingTable(Transform parent, Vector3 foot, float yaw)
        {
            var group = Piece("Offering Table", parent, foot, yaw);
            Box(group, "Top", V(0f, 0.46f, 0f), V(1.3f, 0.08f, 0.62f), wood);
            foreach (float x in new[] { -0.52f, 0.52f })
            {
                Box(group, "Leg", V(x, 0.21f, 0f), V(0.09f, 0.42f, 0.52f), wood);
                Box(group, "Upturned End", V(x * 1.22f, 0.53f, 0f), V(0.08f, 0.1f, 0.62f), wood);
            }
            Box(group, "Runner", V(0f, 0.505f, 0f), V(0.42f, 0.012f, 0.66f), canvasStripe);
            Drum(group, "Bowl", V(-0.36f, 0.54f, 0f), 0.32f, 0.08f, stonePale);
            Ball(group, "Fruit", V(-0.4f, 0.63f, 0.03f), V(0.15f, 0.13f, 0.15f), leafGold);
            Ball(group, "Fruit", V(-0.29f, 0.63f, -0.04f), V(0.14f, 0.12f, 0.14f), leafGold);
            Drum(group, "Flask", V(0.04f, 0.61f, 0.02f), 0.15f, 0.2f, shide);
            Drum(group, "Flask Neck", V(0.04f, 0.76f, 0.02f), 0.06f, 0.1f, shide);
            Drum(group, "Candle", V(0.4f, 0.57f, -0.05f), 0.07f, 0.14f, shide);
            Taper(group, "Candle Flame", V(0.4f, 0.71f, -0.05f), V(0.08f, 0.14f, 0.08f), paper, 0f).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 0.3f, 0f);
            body.size = V(1.3f, 0.6f, 0.62f);
            Settle(group);
        }

        /// <summary>A low, old tree in a stone planter: a bent trunk and three flat pads of leaves. The planter is solid.</summary>
        public static void GnarledTree(Transform parent, Vector3 foot, float yaw)
        {
            var group = Piece("Gnarled Tree", parent, foot, yaw);
            Box(group, "Planter", V(0f, 0.3f, 0f), V(1.9f, 0.6f, 1.9f), carved);
            Box(group, "Planter Rim", V(0f, 0.63f, 0f), V(2.14f, 0.12f, 2.14f), carvedPale);
            Box(group, "Soil", V(0f, 0.7f, 0f), V(1.7f, 0.02f, 1.7f), ivyDark);
            Vector3 root = V(-0.1f, 0.68f, 0f), knee = V(0.28f, 1.2f, 0.1f), elbow = V(-0.2f, 1.68f, -0.05f), crown = V(0.12f, 2.06f, 0.1f);
            Pole(group, "Trunk", root, knee + V(0.03f, 0.05f, 0f), 0.34f, bark);
            Pole(group, "Trunk", knee, elbow + V(-0.03f, 0.04f, 0f), 0.26f, bark);
            Pole(group, "Trunk", elbow, crown, 0.2f, bark);
            Pole(group, "Bough", knee, V(0.9f, 1.46f, -0.2f), 0.15f, bark);
            Pole(group, "Bough", elbow, V(-0.82f, 1.82f, 0.28f), 0.13f, bark);
            Ball(group, "Leaves", V(0.15f, 2.2f, 0.1f), V(1.3f, 0.5f, 1.15f), foliage);
            Ball(group, "Leaves", V(1f, 1.56f, -0.25f), V(1f, 0.4f, 0.86f), foliageDark);
            Ball(group, "Leaves", V(-0.92f, 1.92f, 0.32f), V(0.92f, 0.38f, 0.82f), foliage);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 0.35f, 0f);
            body.size = V(2f, 0.7f, 2f);
            Settle(group);
        }

        /// <summary>Moss on paving: two or three flat rounds, dark with a brighter middle.</summary>
        public static void MossPatch(Transform parent, Vector3 at, float size, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Moss", parent, at, Next(random, 0f, 360f));
            // Many small rounds run together, not one big one: it grows in the joints between the stones.
            int rounds = 5 + random.Next(3);
            for (int i = 0; i < rounds; i++)
            {
                float width = size * Next(random, 0.22f, 0.5f);
                Drum(group, "Moss", V(i == 0 ? 0f : Next(random, -0.5f, 0.5f) * size, 0.008f + i * 0.002f, i == 0 ? 0f : Next(random, -0.5f, 0.5f) * size), width, 0.002f,
                    i == rounds - 1 ? ivyDark : moss).transform.localScale = V(width, 0.001f, width * Next(random, 0.5f, 0.9f));
            }
            Settle(group, false);
        }

        // ------------------------------------------------------------------ the execution ground

        /// <summary>
        /// A tall iron rod on a stone foot, built to call the lightning down. What the camera sees of it is its foot: the
        /// collars and the coils that hold the charge, glowing cold. Stands against a wall; solid at the foot.
        /// </summary>
        public static void LightningRod(Transform parent, Vector3 foot, float height)
        {
            var group = Piece("Lightning Rod", parent, foot, 0f);
            Taper(group, "Foot", V(0f, 0.25f, 0f), V(1.05f, 0.5f, 1.05f), carvedDark, 0.75f);
            Box(group, "Foot Cap", V(0f, 0.54f, 0f), V(0.86f, 0.1f, 0.86f), carvedPale);
            Drum(group, "Mast", V(0f, 0.6f + height * 0.5f, 0f), 0.15f, height + 0.4f, iron);
            for (int i = 0; i < 3; i++) Drum(group, "Collar", V(0f, 0.75f + i * 0.5f, 0f), 0.56f - i * 0.07f, 0.09f, iron);
            for (int i = 0; i < 2; i++)
                Drum(group, "Coil", V(0f, 1f + i * 0.5f, 0f), 0.32f - i * 0.05f, 0.26f, coil).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Box(group, "Cross Tree", V(0f, 0.8f + height * 0.86f, 0f), V(1.1f, 0.07f, 0.07f), iron);
            Taper(group, "Point", V(0f, 0.8f + height + 0.4f, 0f), V(0.22f, 1f, 0.22f), iron, 0f);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 1.2f, 0f);
            body.size = V(0.9f, 2.4f, 0.9f);
            Settle(group);
        }

        /// <summary>A thread of lightning caught between two rods: a few crooked strokes of cold light.</summary>
        public static void Arc(Transform parent, Vector3 from, Vector3 to, int seed)
        {
            var random = new System.Random(seed);
            var group = BuildKit.Group("Arc", parent);
            Vector3 last = from;
            const int Strokes = 4;
            for (int i = 1; i <= Strokes; i++)
            {
                Vector3 at = Vector3.Lerp(from, to, i / (float)Strokes);
                if (i < Strokes) at += V(0f, Next(random, -0.3f, 0.3f), Next(random, -0.12f, 0.12f));
                var stroke = Beam(group, "Stroke", last, at, 0.045f, 0.045f, coil);
                stroke.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                last = at;
            }
        }

        // One bolt for a siege engine: an iron-headed shaft as long as a man, fletched with leather.
        private static void Bolt(Transform parent, Vector3 from, Vector3 to)
        {
            Beam(parent, "Bolt", from, to, 0.07f, 0.07f, shaft);
            Beam(parent, "Bolt Head", Vector3.Lerp(from, to, 0.84f), Vector3.LerpUnclamped(from, to, 1.04f), 0.17f, 0.05f, steel);
            Beam(parent, "Bolt Fletching", from, Vector3.Lerp(from, to, 0.14f), 0.22f, 0.03f, canvasStripe);
        }

        /// <summary>Spent bolts: some lying where they skidded, some standing in the cracks between the stones. Not solid.</summary>
        public static void Bolts(Transform parent, Vector3 centre, int count, float spread, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Bolts", parent, centre, 0f);
            for (int i = 0; i < count; i++)
            {
                Vector3 at = V(Next(random, -spread, spread), 0f, Next(random, -spread, spread));
                float turn = Next(random, 0f, 360f);
                if (i % 3 == 0)
                {
                    // Head down in the ground, the tail in the air.
                    Vector3 lean = Quaternion.Euler(0f, turn, 0f) * Quaternion.Euler(-Next(random, 50f, 70f), 0f, 0f) * Vector3.forward;
                    Bolt(group, at + lean * 1.5f, at - lean * 0.1f);
                }
                else
                {
                    Vector3 along = Quaternion.Euler(0f, turn, 0f) * Vector3.forward;
                    Bolt(group, at + V(0f, 0.05f, 0f), at + along * 1.6f + V(0f, 0.05f, 0f));
                }
            }
            Settle(group, false);
        }

        /// <summary>A ballista broken where it stood: one arm snapped off, the cord slack, a wheel lying beside it. Solid, and low.</summary>
        public static void Ballista(Transform parent, Vector3 foot, float yaw)
        {
            var group = Piece("Broken Ballista", parent, foot, yaw);
            foreach (float x in new[] { -0.52f, 0.52f }) Box(group, "Sled", V(x, 0.12f, 0f), V(0.17f, 0.24f, 2.7f), wood);
            foreach (float z in new[] { -0.95f, 0.85f }) Box(group, "Cross Tie", V(0f, 0.3f, z), V(1.35f, 0.14f, 0.17f), wood);
            Box(group, "Trestle", V(0f, 0.55f, 0.85f), V(0.22f, 0.5f, 0.22f), wood);
            Beam(group, "Stock", V(0.04f, 0.42f, -1.5f), V(-0.04f, 0.95f, 1.55f), 0.26f, 0.2f, wood);
            Box(group, "Bow Head", V(-0.04f, 0.96f, 1.4f), V(0.5f, 0.3f, 0.22f), iron);
            Beam(group, "Bow Arm", V(-0.2f, 0.96f, 1.4f), V(-1.6f, 0.86f, 0.95f), 0.11f, 0.13f, wood);
            Beam(group, "Bow Stub", V(0.2f, 0.96f, 1.4f), V(0.75f, 0.92f, 1.24f), 0.11f, 0.13f, wood);
            Beam(group, "Broken Arm", V(1f, 0.06f, 1.75f), V(1.95f, 0.06f, 1.2f), 0.11f, 0.12f, wood);
            Beam(group, "Cord", V(-1.6f, 0.86f, 0.95f), V(-0.02f, 0.6f, -0.5f), 0.035f, 0.035f, rope);
            Beam(group, "Slack Cord", V(-0.02f, 0.6f, -0.5f), V(0.8f, 0.03f, 0.3f), 0.035f, 0.035f, rope);
            Beam(group, "Slack Cord", V(0.8f, 0.03f, 0.3f), V(1.5f, 0.03f, 1.4f), 0.035f, 0.035f, rope);
            Drum(group, "Winch", V(0f, 0.52f, -1.2f), 0.3f, 1f, wood, V(0f, 0f, 90f));
            Box(group, "Winch Bar", V(0.58f, 0.52f, -1.2f), V(0.06f, 0.62f, 0.06f), iron, V(35f, 0f, 0f));
            Drum(group, "Wheel", V(-1.35f, 0.07f, -0.9f), 0.95f, 0.1f, wood, V(3f, 0f, 0f));
            Drum(group, "Hub", V(-1.35f, 0.12f, -0.9f), 0.24f, 0.12f, band);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 0.5f, 0f);
            body.size = V(1.4f, 1f, 2.8f);
            Settle(group);
        }

        /// <summary>A siege shield of planks, thrown down on its prop and stuck with bolts. Solid, and low.</summary>
        public static void Mantlet(Transform parent, Vector3 foot, float yaw)
        {
            var group = Piece("Fallen Mantlet", parent, foot, yaw);
            var board = BuildKit.Group("Board", group);
            board.localPosition = V(0f, 0.42f, 0f);
            board.localRotation = Quaternion.Euler(-68f, 0f, 4f);
            Box(board, "Planks", Vector3.zero, V(2.5f, 1.7f, 0.1f), wood);
            foreach (float y in new[] { -0.55f, 0.5f }) Box(board, "Batten", V(0f, y, -0.08f), V(2.6f, 0.14f, 0.08f), band);
            Box(board, "Split", V(0.5f, 0.3f, -0.055f), V(0.05f, 1.1f, 0.03f), hollow, V(0f, 0f, 5f));
            Bolt(board, V(-0.6f, 0.2f, -1.3f), V(-0.5f, 0.1f, 0.1f));
            Bolt(board, V(0.9f, -0.35f, -1.2f), V(0.7f, -0.3f, 0.1f));
            Beam(group, "Prop", V(0.2f, 0.05f, 1.4f), V(-0.1f, 0.5f, 0.2f), 0.11f, 0.11f, wood);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 0.4f, 0f);
            body.size = V(2.5f, 0.8f, 1.7f);
            Settle(group);
        }

        /// <summary>Where a bolt of lightning struck the paving: a black star burned into the stone, grey ash inside it.</summary>
        public static void ScorchMark(Transform parent, Vector3 at, float radius, int seed)
        {
            var random = new System.Random(seed);
            var group = Piece("Scorch Mark", parent, at, Next(random, 0f, 360f));
            Box(group, "Burn", V(0f, 0.012f, 0f), V(radius, 1f, radius * Next(random, 0.8f, 1f)), scorch).GetComponent<MeshFilter>().sharedMesh = star;
            Box(group, "Ash", V(0f, 0.016f, 0f), V(radius * 0.55f, 1f, radius * 0.5f), ash, V(0f, Next(random, 20f, 60f), 0f)).GetComponent<MeshFilter>().sharedMesh = star;
            Box(group, "Heart", V(0f, 0.02f, 0f), V(radius * 0.24f, 1f, radius * 0.24f), scorch, V(0f, Next(random, 0f, 90f), 0f)).GetComponent<MeshFilter>().sharedMesh = star;
            Settle(group, false);
        }

        /// <summary>An iron stake with shackles hanging from its ring, set where the lightning comes down. Thin; solid at the stake.</summary>
        public static void Stake(Transform parent, Vector3 foot, float yaw, int seed)
        {
            var group = Piece("Stake", parent, foot, yaw);
            Taper(group, "Footing", V(0f, 0.14f, 0f), V(0.7f, 0.28f, 0.7f), stoneDark, 0.75f);
            Box(group, "Stake", V(0f, 1.2f, 0f), V(0.14f, 2.4f, 0.14f), iron, V(0f, 0f, (seed % 3 - 1) * 3f));
            Drum(group, "Ring", V(0f, 2.1f, 0f), 0.36f, 0.06f, iron);
            Chain(group, V(0.16f, 2.08f, 0f), V(0.42f, 1.05f, -0.12f), -0.08f, 4);
            Chain(group, V(-0.16f, 2.08f, 0f), V(-0.36f, 0.9f - (seed % 2) * 0.3f, -0.1f), 0.08f, 4);
            Box(group, "Shackle", V(0.44f, 0.98f, -0.12f), V(0.2f, 0.16f, 0.07f), iron);
            var body = group.gameObject.AddComponent<CapsuleCollider>();
            body.radius = 0.16f;
            body.height = 2.4f;
            body.center = V(0f, 1.2f, 0f);
            Settle(group);
        }

        /// <summary>An iron cage that hung from the wall once, lying on its side with its door sprung. Solid, and low.</summary>
        public static void Cage(Transform parent, Vector3 foot, float yaw)
        {
            var group = Piece("Fallen Cage", parent, foot, yaw);
            var cage = BuildKit.Group("Cage", group);
            cage.localPosition = V(0f, 0.5f, 0f);
            cage.localRotation = Quaternion.Euler(0f, 0f, 86f);
            foreach (float y in new[] { -0.85f, 0f, 0.85f }) Drum(cage, "Hoop", V(0f, y, 0f), 1f, 0.07f, iron);
            for (int i = 0; i < 7; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Box(cage, "Bar", V(Mathf.Cos(angle) * 0.47f, 0f, Mathf.Sin(angle) * 0.47f), V(0.05f, 1.8f, 0.05f), iron);
            }
            Taper(cage, "Crown", V(0f, 1.1f, 0f), V(0.9f, 0.5f, 0.9f), iron, 0f);
            Chain(group, V(-1.4f, 0.5f, 0f), V(-2.6f, 0.05f, 0.7f), 0.1f, 5);
            var body = group.gameObject.AddComponent<BoxCollider>();
            body.center = V(0f, 0.5f, 0f);
            body.size = V(2f, 1f, 1f);
            Settle(group);
        }

        /// <summary>A heavy banner hung low on a wall face: dark red with a bound edge, a forked foot and the storm's mark.</summary>
        public static void HeavyBanner(Transform parent, Vector3 top, float yaw, float width, float drop)
        {
            var group = Piece("Heavy Banner", parent, top, yaw);
            Box(group, "Rod", V(0f, 0f, 0.14f), V(width + 0.6f, 0.13f, 0.13f), iron);
            foreach (float side in new[] { -1f, 1f })
            {
                Box(group, "Bracket", V(side * (width * 0.5f + 0.16f), 0f, 0.07f), V(0.1f, 0.24f, 0.14f), iron);
                Box(group, "Binding", V(side * (width * 0.5f - 0.09f), -drop * 0.5f, 0.125f), V(0.17f, drop, 0.03f), bannerEdge);
                Box(group, "Fork", V(side * width * 0.27f, -drop - 0.3f, 0.1f), V(width * 0.44f, 0.62f, 0.05f), bannerHeavy, V(0f, 0f, side * -5f));
            }
            Box(group, "Cloth", V(0f, -drop * 0.5f, 0.1f), V(width, drop, 0.05f), bannerHeavy);
            Box(group, "Band", V(0f, -drop + 0.22f, 0.13f), V(width, 0.14f, 0.03f), bannerEdge);
            Box(group, "Mark Field", V(0f, -drop * 0.46f, 0.13f), V(0.74f, 0.74f, 0.03f), bannerEdge, V(0f, 0f, 45f));
            Box(group, "Mark", V(0.05f, -drop * 0.46f + 0.14f, 0.15f), V(0.11f, 0.42f, 0.02f), leafGold, V(0f, 0f, 22f));
            Box(group, "Mark", V(-0.05f, -drop * 0.46f - 0.14f, 0.15f), V(0.11f, 0.42f, 0.02f), leafGold, V(0f, 0f, 22f));
            Settle(group);
        }

        // ------------------------------------------------------------------ what stands beyond the walls

        /// <summary>A jagged line of mountains as one flat shape, far outside the play space.</summary>
        public static void Ridge(Transform parent, Vector3 centre, float yaw, float length, float height)
        {
            var line = Box(parent, "Far Ridge", centre, V(length, height, 1f), far, V(0f, yaw, 0f));
            line.GetComponent<MeshFilter>().sharedMesh = ridge;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        /// <summary>A tower far off: a flat dark shape with a pointed roof, and no more.</summary>
        public static void FarTower(Transform parent, Vector3 foot, float width, float height)
        {
            var group = Piece("Far Tower", parent, foot, 0f);
            Box(group, "Shaft", V(0f, height * 0.5f, 0f), V(width, height, width), far);
            Box(group, "Gallery", V(0f, height - 0.6f, 0f), V(width * 1.25f, 1.2f, width * 1.25f), far);
            Taper(group, "Roof", V(0f, height + width * 0.6f, 0f), V(width * 1.35f, width * 1.2f, width * 1.35f), far, 0f);
            Settle(group, false);
        }

        // ------------------------------------------------------------------ shapes made here

        // A four-cornered face, wound so that it looks away from the middle of the shape it belongs to.
        private static void Face(List<Vector3> vertices, List<Vector3> normals, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            if (Vector3.Dot(normal, (a + b + c + d) * 0.25f) < 0f)
            {
                (b, d) = (d, b);
                normal = -normal;
            }
            int start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            for (int i = 0; i < 4; i++) normals.Add(normal);
            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        // A flat face seen from both sides and lit as if it faced up, so it is one tone.
        private static void Leaf(List<Vector3> vertices, List<Vector3> normals, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            for (int i = 0; i < 4; i++) normals.Add(Vector3.up);
            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3, start + 2, start + 1, start, start + 3, start + 2, start });
        }

        // A unit box whose top is the given fraction of its foot.
        private static Mesh TaperMesh(string name, float top)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            float t = 0.5f * top;
            Vector3[] low = { V(-0.5f, -0.5f, -0.5f), V(0.5f, -0.5f, -0.5f), V(0.5f, -0.5f, 0.5f), V(-0.5f, -0.5f, 0.5f) };
            Vector3[] high = { V(-t, 0.5f, -t), V(t, 0.5f, -t), V(t, 0.5f, t), V(-t, 0.5f, t) };
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                Face(vertices, normals, triangles, low[i], low[j], high[j], high[i]);
            }
            Face(vertices, normals, triangles, low[0], low[1], low[2], low[3]);
            Face(vertices, normals, triangles, high[0], high[1], high[2], high[3]);
            return SaveMesh(name, vertices, normals, triangles);
        }

        // A burn mark: a flat star of uneven points, radius one.
        private static Mesh StarMesh()
        {
            var random = new System.Random(21);
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            const int Points = 9;
            var rim = new Vector3[Points * 2];
            for (int i = 0; i < rim.Length; i++)
            {
                float angle = (i + Next(random, -0.2f, 0.2f)) * Mathf.PI / Points;
                float reach = i % 2 == 0 ? Next(random, 0.6f, 1f) : Next(random, 0.22f, 0.36f);
                rim[i] = V(Mathf.Cos(angle) * reach, 0f, Mathf.Sin(angle) * reach);
            }
            for (int i = 0; i < rim.Length; i++) Leaf(vertices, normals, triangles, Vector3.zero, rim[i], rim[(i + 1) % rim.Length], Vector3.zero);
            return SaveMesh("scorch_star", vertices, normals, triangles);
        }

        // Ivy on a wall: leaves in an upright sheet one wide and one high, thick at the foot and climbing in tongues.
        private static Mesh IvyMesh()
        {
            var random = new System.Random(5);
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            void Add(float x, float y, float size)
            {
                var turn = Quaternion.Euler(0f, 0f, Next(random, -50f, 50f));
                Vector3 centre = V(x, y, 0f);
                Leaf(vertices, normals, triangles, centre + turn * V(-size, 0f, 0f), centre + turn * V(0f, size * 0.9f, 0f), centre + turn * V(size, 0f, 0f), centre + turn * V(0f, -size * 1.3f, 0f));
            }
            for (int i = 0; i < 16; i++) Add(Next(random, -0.42f, 0.42f), Next(random, 0.05f, 0.24f), Next(random, 0.09f, 0.15f));
            for (int i = 0; i < 44; i++)
            {
                float x = Next(random, -0.47f, 0.47f);
                float tongue = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Cos(x * 14f + 1f));
                Add(x, Mathf.Pow(Next(random, 0f, 1f), 1.4f) * tongue * 0.95f + 0.04f, Next(random, 0.035f, 0.075f));
            }
            return SaveMesh("ivy_sheet", vertices, normals, triangles);
        }

        // The broken edge of a floor: a row of stones one long in all, each reaching a different way out from the wall.
        private static Mesh EdgeMesh()
        {
            var random = new System.Random(12);
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            const int Stones = 26;
            float x = -0.5f;
            for (int i = 0; i < Stones; i++)
            {
                float next = i == Stones - 1 ? 0.5f : Mathf.Min(0.5f, x + Next(random, 0.6f, 1.4f) / Stones);
                float reach = random.Next(5) == 0 ? Next(random, 0.1f, 0.3f) : Next(random, 0.45f, 1f);
                float skew = Next(random, -0.08f, 0.08f);
                Leaf(vertices, normals, triangles, V(x, 0f, 0f), V(x, 0f, reach + skew), V(next, 0f, reach - skew), V(next, 0f, 0f));
                x = next;
                if (x >= 0.5f) break;
            }
            return SaveMesh("floor_edge", vertices, normals, triangles);
        }

        // A line of peaks: one wide and at most one high, standing upright.
        private static Mesh RidgeMesh()
        {
            var random = new System.Random(31);
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            const int Steps = 22;
            float last = 0.3f;
            for (int i = 0; i < Steps; i++)
            {
                float height = i % 2 == 0 ? Next(random, 0.5f, 1f) : Next(random, 0.2f, 0.5f);
                if (i == Steps - 1) height = 0.25f;
                float x0 = i / (float)Steps - 0.5f, x1 = (i + 1) / (float)Steps - 0.5f;
                Leaf(vertices, normals, triangles, V(x0, 0f, 0f), V(x0, last, 0f), V(x1, height, 0f), V(x1, 0f, 0f));
                last = height;
            }
            return SaveMesh("far_ridge", vertices, normals, triangles);
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

        // ------------------------------------------------------------------ many shapes cast as one

        /// <summary>
        /// Gathers boxes, drums, balls and flat inlaid shapes and casts them as one mesh in one material, so a whole
        /// balustrade or a whole floor emblem is a single thing to draw. Shapes are added in the mould's own space, moved
        /// by <see cref="Frame"/>. Flat shapes lie on the ground: when <see cref="Top"/> is set it gives the height of the
        /// floor at a distance from the origin, and each flat shape is laid <see cref="Lift"/> above the highest floor under it.
        /// </summary>
        public sealed class Mould
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector3> normals = new List<Vector3>();
            private readonly List<int> triangles = new List<int>();

            public Matrix4x4 Frame = Matrix4x4.identity;
            public System.Func<float, float> Top;
            public float Lift;

            public bool Empty => vertices.Count == 0;

            public void Home() => Frame = Matrix4x4.identity;
            public void Place(Vector3 at, float yaw, float scale = 1f) => Frame = Matrix4x4.TRS(at, Quaternion.Euler(0f, yaw, 0f), Vector3.one * scale);
            public void Place(Vector3 at, Quaternion turn, float scale = 1f) => Frame = Matrix4x4.TRS(at, turn, Vector3.one * scale);

            private int Add(Vector3 point, Vector3 normal)
            {
                vertices.Add(Frame.MultiplyPoint3x4(point));
                normals.Add(Frame.MultiplyVector(normal).normalized);
                return vertices.Count - 1;
            }

            /// <summary>One triangle with a normal at each corner; it is wound to face the way its normals point.</summary>
            public void Tri(Vector3 a, Vector3 b, Vector3 c, Vector3 na, Vector3 nb, Vector3 nc)
            {
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), na + nb + nc) < 0f)
                {
                    (b, c) = (c, b);
                    (nb, nc) = (nc, nb);
                }
                int start = Add(a, na);
                Add(b, nb);
                Add(c, nc);
                triangles.Add(start);
                triangles.Add(start + 1);
                triangles.Add(start + 2);
            }

            /// <summary>A flat four-cornered face looking the way <paramref name="outward"/> points.</summary>
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 outward)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a);
                if (normal.sqrMagnitude < 1e-10f) normal = Vector3.Cross(c - a, d - a);
                if (normal.sqrMagnitude < 1e-10f) return;
                normal.Normalize();
                if (Vector3.Dot(normal, outward) < 0f)
                {
                    (b, d) = (d, b);
                    normal = -normal;
                }
                int start = Add(a, normal);
                Add(b, normal);
                Add(c, normal);
                Add(d, normal);
                triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
            }

            /// <summary>A box. Its top is the given fraction of its foot: 1 square, 0 a point.</summary>
            public void Box(Vector3 centre, Vector3 size, Quaternion? turn = null, float top = 1f)
            {
                Quaternion q = turn ?? Quaternion.identity;
                Vector3 h = size * 0.5f;
                Vector3[] low = { V(-h.x, -h.y, -h.z), V(h.x, -h.y, -h.z), V(h.x, -h.y, h.z), V(-h.x, -h.y, h.z) };
                Vector3[] high = { V(-h.x * top, h.y, -h.z * top), V(h.x * top, h.y, -h.z * top), V(h.x * top, h.y, h.z * top), V(-h.x * top, h.y, h.z * top) };
                for (int i = 0; i < 4; i++)
                {
                    low[i] = centre + q * low[i];
                    high[i] = centre + q * high[i];
                }
                for (int i = 0; i < 4; i++)
                {
                    int j = (i + 1) % 4;
                    Vector3 middle = (low[i] + low[j]) * 0.5f - (low[0] + low[2]) * 0.5f;
                    Quad(low[i], low[j], high[j], high[i], middle);
                }
                Quad(low[0], low[1], low[2], low[3], q * Vector3.down);
                if (top > 0.01f) Quad(high[0], high[1], high[2], high[3], q * Vector3.up);
            }

            /// <summary>A square bar from one point to another.</summary>
            public void Beam(Vector3 from, Vector3 to, float width, float height)
            {
                Vector3 along = to - from;
                if (along.sqrMagnitude < 1e-8f) return;
                Vector3 direction = along.normalized;
                Quaternion turn = Mathf.Abs(direction.y) > 0.98f ? Quaternion.FromToRotation(Vector3.forward, direction) : Quaternion.LookRotation(direction, Vector3.up);
                Box((from + to) * 0.5f, V(width, height, along.magnitude), turn);
            }

            /// <summary>A round drum of the given width and whole height, its axis along its own up.</summary>
            public void Drum(Vector3 centre, float width, float height, Quaternion? turn = null, int sides = 10, float top = 1f)
            {
                Quaternion q = turn ?? Quaternion.identity;
                float r = width * 0.5f, h = height * 0.5f;
                Vector3 up = q * Vector3.up;
                for (int i = 0; i < sides; i++)
                {
                    float a0 = i * Mathf.PI * 2f / sides, a1 = (i + 1) * Mathf.PI * 2f / sides;
                    Vector3 n0 = q * V(Mathf.Cos(a0), 0f, Mathf.Sin(a0)), n1 = q * V(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                    Vector3 l0 = centre + n0 * r - up * h, l1 = centre + n1 * r - up * h;
                    Vector3 h0 = centre + n0 * (r * top) + up * h, h1 = centre + n1 * (r * top) + up * h;
                    Tri(l0, l1, h1, n0, n1, n1);
                    if (top > 0.01f) Tri(l0, h1, h0, n0, n1, n0);
                    Tri(centre - up * h, l0, l1, -up, -up, -up);
                    if (top > 0.01f) Tri(centre + up * h, h0, h1, up, up, up);
                }
            }

            /// <summary>A round bar from one point to another.</summary>
            public void Pole(Vector3 from, Vector3 to, float width, int sides = 6, float top = 1f)
            {
                Vector3 along = to - from;
                if (along.sqrMagnitude < 1e-8f) return;
                Drum((from + to) * 0.5f, width, along.magnitude, Quaternion.FromToRotation(Vector3.up, along.normalized), sides, top);
            }

            /// <summary>A ball, or an egg: <paramref name="size"/> is its whole width, height and depth.</summary>
            public void Ball(Vector3 centre, Vector3 size, Quaternion? turn = null, int sides = 10, int rings = 6)
            {
                Quaternion q = turn ?? Quaternion.identity;
                Vector3 half = size * 0.5f;
                Vector3 Unit(int ring, int side)
                {
                    float latitude = (-0.5f + ring / (float)rings) * Mathf.PI, longitude = side * Mathf.PI * 2f / sides;
                    return V(Mathf.Cos(latitude) * Mathf.Cos(longitude), Mathf.Sin(latitude), Mathf.Cos(latitude) * Mathf.Sin(longitude));
                }
                Vector3 Point(Vector3 unit) => centre + q * Vector3.Scale(unit, half);
                Vector3 Normal(Vector3 unit) => q * V(unit.x / Mathf.Max(half.x, 1e-4f), unit.y / Mathf.Max(half.y, 1e-4f), unit.z / Mathf.Max(half.z, 1e-4f)).normalized;
                for (int ring = 0; ring < rings; ring++)
                {
                    for (int side = 0; side < sides; side++)
                    {
                        Vector3 a = Unit(ring, side), b = Unit(ring, side + 1), c = Unit(ring + 1, side + 1), d = Unit(ring + 1, side);
                        if (ring > 0) Tri(Point(a), Point(b), Point(c), Normal(a), Normal(b), Normal(c));
                        if (ring < rings - 1) Tri(Point(a), Point(c), Point(d), Normal(a), Normal(c), Normal(d));
                    }
                }
            }

            /// <summary>A shape turned on a lathe about its own up: each pair is a radius and a height, from the foot upward.</summary>
            public void Lathe(Vector3 centre, Quaternion? turn, int sides, params float[] profile)
            {
                Quaternion q = turn ?? Quaternion.identity;
                for (int step = 0; step + 3 < profile.Length; step += 2)
                {
                    float r0 = profile[step], y0 = profile[step + 1], r1 = profile[step + 2], y1 = profile[step + 3];
                    Vector2 slope = new Vector2(y1 - y0, r0 - r1).normalized;
                    for (int i = 0; i < sides; i++)
                    {
                        float a0 = i * Mathf.PI * 2f / sides, a1 = (i + 1) * Mathf.PI * 2f / sides;
                        Vector3 d0 = V(Mathf.Cos(a0), 0f, Mathf.Sin(a0)), d1 = V(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                        Vector3 n0 = q * (d0 * slope.x + Vector3.up * slope.y), n1 = q * (d1 * slope.x + Vector3.up * slope.y);
                        Vector3 l0 = centre + q * (d0 * r0 + Vector3.up * y0), l1 = centre + q * (d1 * r0 + Vector3.up * y0);
                        Vector3 h0 = centre + q * (d0 * r1 + Vector3.up * y1), h1 = centre + q * (d1 * r1 + Vector3.up * y1);
                        if (r0 > 1e-4f) Tri(l0, l1, h1, n0, n1, n1);
                        if (r1 > 1e-4f) Tri(l0, h1, h0, n0, n1, n0);
                    }
                }
            }

            // ---- flat shapes, lying on the ground and looking up

            private float Height(params Vector3[] points)
            {
                if (Top == null) return Lift;
                float least = float.MaxValue;
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 a = points[i], b = points[(i + 1) % points.Length];
                    least = Mathf.Min(least, new Vector2(a.x, a.z).magnitude, new Vector2((a.x + b.x) * 0.5f, (a.z + b.z) * 0.5f).magnitude);
                }
                return Top(Mathf.Max(0f, least - 0.05f)) + Lift;
            }

            /// <summary>A flat patch with the given corners, in order round its edge. It must bulge outward everywhere.</summary>
            public void Patch(params Vector3[] points)
            {
                float y = Height(points);
                Vector3 middle = Vector3.zero;
                foreach (var point in points) middle += point;
                middle /= points.Length;
                middle.y = y;
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 a = points[i], b = points[(i + 1) % points.Length];
                    a.y = b.y = y;
                    Tri(middle, a, b, Vector3.up, Vector3.up, Vector3.up);
                }
            }

            /// <summary>A flat stroke from one point to another, as wide at its end as <paramref name="endWidth"/> (or the same all along).</summary>
            public void Line(Vector3 from, Vector3 to, float width, float endWidth = -1f)
            {
                if (endWidth < 0f) endWidth = width;
                Vector3 along = to - from;
                along.y = 0f;
                float length = along.magnitude;
                if (length < 1e-4f) return;
                Vector3 aside = Vector3.Cross(Vector3.up, along / length) * 0.5f;
                // Cut into short lengths, so that a long stroke steps down with the floor it crosses.
                int pieces = Mathf.Max(1, Mathf.CeilToInt(length / 0.9f));
                for (int i = 0; i < pieces; i++)
                {
                    float t0 = i / (float)pieces, t1 = (i + 1) / (float)pieces;
                    Vector3 p0 = Vector3.Lerp(from, to, t0), p1 = Vector3.Lerp(from, to, t1);
                    float w0 = Mathf.Lerp(width, endWidth, t0), w1 = Mathf.Lerp(width, endWidth, t1);
                    Patch(p0 - aside * w0, p0 + aside * w0, p1 + aside * w1, p1 - aside * w1);
                }
            }

            /// <summary>Part of a flat ring about a point. Angles are in degrees clockwise from north, as a yaw is.</summary>
            public void Arc(Vector3 centre, float inner, float outer, float fromDegrees, float toDegrees, float stepDegrees = 3f)
            {
                int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(toDegrees - fromDegrees) / stepDegrees));
                for (int i = 0; i < steps; i++)
                {
                    float a0 = Mathf.Lerp(fromDegrees, toDegrees, i / (float)steps) * Mathf.Deg2Rad, a1 = Mathf.Lerp(fromDegrees, toDegrees, (i + 1) / (float)steps) * Mathf.Deg2Rad;
                    Vector3 d0 = V(Mathf.Sin(a0), 0f, Mathf.Cos(a0)), d1 = V(Mathf.Sin(a1), 0f, Mathf.Cos(a1));
                    Patch(centre + d0 * inner, centre + d0 * outer, centre + d1 * outer, centre + d1 * inner);
                }
            }

            /// <summary>A flat round, or an oval when squashed, turned by a yaw.</summary>
            public void Round(Vector3 centre, float radius, int sides = 14, float squash = 1f, float yaw = 0f)
            {
                var points = new Vector3[sides];
                Quaternion turn = Quaternion.Euler(0f, yaw, 0f);
                for (int i = 0; i < sides; i++)
                {
                    float angle = i * Mathf.PI * 2f / sides;
                    points[i] = centre + turn * V(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius * squash);
                }
                Patch(points);
            }

            /// <summary>A flat star of uneven points, as a burn is, or a splash.</summary>
            public void Burst(Vector3 centre, float radius, int seed, int points = 9, float inner = 0.3f)
            {
                var random = new System.Random(seed);
                var rim = new Vector3[points * 2];
                float turn = Next(random, 0f, Mathf.PI * 2f);
                for (int i = 0; i < rim.Length; i++)
                {
                    float angle = turn + (i + Next(random, -0.2f, 0.2f)) * Mathf.PI / points;
                    float reach = radius * (i % 2 == 0 ? Next(random, 0.6f, 1f) : inner * Next(random, 0.75f, 1.2f));
                    rim[i] = centre + V(Mathf.Cos(angle) * reach, 0f, Mathf.Sin(angle) * reach);
                }
                for (int i = 0; i < rim.Length; i++) Patch(centre, rim[i], rim[(i + 1) % rim.Length]);
            }

            /// <summary>
            /// A crooked flat stroke, as a crack runs or a burn: widest at its start and thinning to nothing. Returns the
            /// points it passed through, so that another may fork from one of them.
            /// </summary>
            public List<Vector3> Jag(Vector3 from, Vector3 to, float width, float wobble, int seed, float stride = 0.6f)
            {
                var random = new System.Random(seed);
                Vector3 along = to - from;
                along.y = 0f;
                float length = along.magnitude;
                var path = new List<Vector3> { from };
                if (length < 1e-4f) return path;
                Vector3 aside = Vector3.Cross(Vector3.up, along / length);
                int pieces = Mathf.Max(2, Mathf.RoundToInt(length / stride));
                for (int i = 1; i <= pieces; i++)
                {
                    Vector3 at = Vector3.Lerp(from, to, i / (float)pieces);
                    if (i < pieces) at += aside * Next(random, -wobble, wobble) + along / length * Next(random, -0.2f, 0.2f) * stride;
                    path.Add(at);
                }
                for (int i = 0; i < pieces; i++)
                {
                    float w0 = width * (1f - i / (float)pieces), w1 = width * (1f - (i + 1) / (float)pieces);
                    Vector3 step = (path[i + 1] - path[i]).normalized;
                    // Each length runs a little past its joint, so the corners close.
                    Line(path[i] - step * (w0 * 0.3f), path[i + 1] + step * (w1 * 0.3f), Mathf.Max(w0, 0.012f), Mathf.Max(w1, 0.012f));
                }
                return path;
            }

            /// <summary>
            /// Casts everything gathered as one mesh, saved under a stable name, on a new child of the parent. Returns
            /// null when nothing was gathered.
            /// </summary>
            public Renderer Cast(string name, Transform parent, Material material, bool shadows = true)
            {
                if (Empty) return null;
                string folder = BuildKit.GeneratedRoot + "/Meshes";
                BuildKit.EnsureFolder(folder);
                string path = $"{folder}/cast_{name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null)
                {
                    mesh = new Mesh { name = "cast_" + name };
                    AssetDatabase.CreateAsset(mesh, path);
                }
                mesh.Clear();
                mesh.indexFormat = vertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetTriangles(triangles, 0);
                // The toon shader's outline reads the tangents; these shapes have none of their own.
                var tangents = new List<Vector4>(vertices.Count);
                for (int i = 0; i < vertices.Count; i++) tangents.Add(new Vector4(0f, 1f, 0f, 1f));
                mesh.SetTangents(tangents);
                mesh.RecalculateBounds();
                EditorUtility.SetDirty(mesh);

                var made = new GameObject(name);
                made.transform.SetParent(parent, false);
                made.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = made.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                if (!shadows) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                made.isStatic = true;
                return renderer;
            }
        }
    }
}
