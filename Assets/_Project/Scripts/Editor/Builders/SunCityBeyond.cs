using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.EditorTools
{
    /// <summary>
    /// What lies beyond the walls of the sun-king's city and his court (the visual overview, step 8). This camera sees
    /// far only where the ground falls away below it, so the king's ground stands on a raised mound (a tell, as old
    /// cities do), and below it lies the lower town: flat-roofed houses packed along lanes, a hearth glowing here and
    /// there, smoke going up, palms, and beyond the houses the dunes. Cheap by design: boxes with the land's own
    /// surfaces, no colliders, no shadows, merged with the rest of the scenery; only what the eye needs at forty metres.
    /// </summary>
    public static class SunCityBeyond
    {
        // The land's own surfaces for walls seen from far: plaster, brick, a darker brick.
        private static Material[] fronts;
        private static Material roofTop, doorway, hearth, sand, face, faceBand, ground;

        private static void Ready()
        {
            fronts = new[]
            {
                SunCityKit.Flat("m_sun_plaster", new Color(0.68f, 0.62f, 0.53f)),
                SunCityKit.Flat("m_sun_far_plaster", new Color(0.62f, 0.56f, 0.48f)),
                SunCityKit.Brick,
                SunCityKit.Flat("m_sun_far_brick", new Color(0.5f, 0.44f, 0.38f), 0.02f),
            };
            roofTop = SunCityKit.Flat("m_sun_far_roof", new Color(0.72f, 0.66f, 0.56f));
            doorway = SunCityKit.Flat("m_sun_far_door", new Color(0.16f, 0.13f, 0.13f));
            hearth = SunCityKit.For("m_kit_ember");
            sand = SunCityKit.Flat("m_sun_sand", new Color(0.71f, 0.63f, 0.48f));
            face = SunCityKit.Flat("m_sun_bank_face", new Color(0.47f, 0.4f, 0.33f));
            faceBand = SunCityKit.Flat("m_sun_bank_band", new Color(0.41f, 0.35f, 0.3f));
            ground = SunCityKit.Flat("m_sun_earth", new Color(0.6f, 0.51f, 0.39f));
        }

        /// <summary>
        /// The sun city's surroundings: its plain becomes a mound 11 m above the lower town, whose faces are dried mud
        /// in strata, and the lower town spreads east and west of it with the dunes beyond. Call after everything else
        /// is placed (it draws the ground's dice last).
        /// </summary>
        public static void City(Transform world)
        {
            Ready();
            var group = BuildKit.Group("Lower Town", world);
            // The old sheet of sand under the whole land is replaced by the mound and the lower ground.
            var plain = BuildKit.FindDeep(world, "Plain");
            if (plain != null) Object.DestroyImmediate(plain.gameObject);
            // In two lengths that overlap (the same stone, so the overlap does not show): one of 640 m makes triangles too
            // long for the physics to take quietly.
            foreach (var (south, north) in new[] { (-16f, 320f), (290f, 626f) })
            {
                var tell = SunCityGround.Plateau("Tell", group, SunCityGround.Rect(-80f, 48f, south, north), -0.8f, -12f, sand, face, faceBand, 1.4f, 14f, 1.6f, 3f);
                // Nobody walks on it: the land's own floors are under every foot.
                Object.DestroyImmediate(tell.GetComponent<MeshCollider>());
            }
            const float Low = -12f;
            var under = BuildKit.Shape(PrimitiveType.Cube, "Lower Ground", group, new Vector3(-16f, Low - 0.5f, 300f), new Vector3(700f, 1f, 1100f), ground);
            under.shadowCastingMode = ShadowCastingMode.Off;
            under.gameObject.isStatic = true;
            var dice = new System.Random(6061);
            var keep = new[] { new Vector4(-96f, 64f, -30f, 640f) };
            // On the mound outside the west wall, the river town's gardens: plots of dark worked earth in rows, and palms.
            var field = SunCityKit.Flat("m_sun_earth_dark", new Color(0.5f, 0.42f, 0.33f));
            foreach (var (south, north) in new[] { (-12f, 60f), (94f, 132f) })
                for (float z = south; z < north - 6f; z += 9f + (float)dice.NextDouble() * 3f)
                    for (float x = -76f; x < -48f; x += 8f + (float)dice.NextDouble() * 2f)
                    {
                        if (dice.NextDouble() < 0.3)
                        {
                            SunCityKit.City(dice.NextDouble() < 0.5 ? "palm_a" : "palm_c", group, new Vector3(x + 3f, -0.8f, z + 3f), (float)dice.NextDouble() * 360f, Vector3.one * (0.85f + (float)dice.NextDouble() * 0.3f));
                            continue;
                        }
                        var plot = BuildKit.Shape(PrimitiveType.Cube, "Garden Plot", group, new Vector3(x + 3f, -0.78f, z + 3.5f), new Vector3(5.6f, 0.06f, 7f), field, new Vector3(0f, (float)dice.NextDouble() * 6f - 3f, 0f));
                        plot.shadowCastingMode = ShadowCastingMode.Off;
                        plot.gameObject.isStatic = true;
                    }
            FarTown(group, new Vector4(-190f, -96f, -40f, 600f), Low, keep, dice, 0.55f);
            FarTown(group, new Vector4(64f, 160f, -40f, 600f), Low, keep, dice, 0.55f);
            Dunes(group, new Vector4(-330f, -200f, -100f, 700f), Low, 26, dice);
            Dunes(group, new Vector4(170f, 300f, -100f, 700f), Low, 22, dice);
        }

        /// <summary>
        /// The sun court's surroundings: the court stands on a platform 18 m over the city below, which spreads round
        /// it on every side; a stair goes down west from the gate the sun sets through, and a road runs on from its
        /// foot toward the dunes on the western horizon, along the line of the sun's road. The court's ground slab is
        /// kept to stand on and no longer drawn.
        /// </summary>
        public static void Court(Transform world, GameObject groundSlab, float radius, float westGate)
        {
            Ready();
            var group = BuildKit.Group("Below The Court", world);
            if (groundSlab != null) groundSlab.GetComponent<Renderer>().enabled = false;
            const float Low = -18f;
            var stone = SunCityKit.Flat("m_sun_ashlar", new Color(0.6f, 0.56f, 0.52f));
            var band = SunCityKit.Flat("m_sun_ashlar_band", new Color(0.54f, 0.5f, 0.48f));
            var ledge = SunCityKit.Flat("m_sun_stone_worked", new Color(0.66f, 0.62f, 0.55f), 0.02f);
            SunCityGround.Plateau("Court Platform", group, SunCityGround.Oval(Vector2.zero, radius + 4f, radius + 4f, 56), 0f, Low, ledge, stone, band, 0f, 3f, 2.2f, 2f);
            SunCityGround.Plateau("Forecourt Platform", group, SunCityGround.Rect(-9f, 9f, -40f, -21.5f), 0f, Low, ledge, stone, band, 0f, 3f, 2.2f, 2f);
            var under = BuildKit.Shape(PrimitiveType.Cube, "Lower Ground", group, new Vector3(0f, Low - 0.5f, 0f), new Vector3(560f, 1f, 560f), ground);
            under.shadowCastingMode = ShadowCastingMode.Off;
            under.gameObject.isStatic = true;

            // The stair down west from the gate: wide, between low walls, from the court's ledge to the town below.
            var stair = BuildKit.Group("West Stair", group);
            float from = -(radius + 4f), run = 40f;
            int steps = 72;
            for (int i = 0; i < steps; i++)
            {
                float t = (i + 0.5f) / steps;
                float x = from - t * run, top = Mathf.Lerp(0f, Low, (i + 1f) / steps);
                var step = BuildKit.Shape(PrimitiveType.Cube, "Step", stair, new Vector3(x, (top + Low) * 0.5f, 0f), new Vector3(run / steps + 0.02f, top - Low + 0.25f, 7f), i % 9 == 4 ? band : stone);
                step.shadowCastingMode = ShadowCastingMode.Off;
                step.gameObject.isStatic = true;
            }
            foreach (float side in new[] { -1f, 1f })
            {
                var wall = BuildKit.Shape(PrimitiveType.Cube, "Stair Wall", stair, new Vector3(from - run * 0.5f, Low * 0.5f + 0.4f, side * 3.9f), new Vector3(run + 0.4f, 1f, 0.8f), stone);
                // Falling west: the east end, toward the gate, is the high one.
                wall.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-Low, run) * Mathf.Rad2Deg);
                wall.transform.localScale = new Vector3(Mathf.Sqrt(run * run + Low * Low), 1.1f, 0.8f);
                wall.gameObject.isStatic = true;
            }
            // The road on from the stair's foot, west into the low sun, paved and lined with braziers going out of sight.
            var road = BuildKit.Shape(PrimitiveType.Cube, "West Road", group, new Vector3(from - run - 60f, Low + 0.05f, 0f), new Vector3(120f, 0.1f, 7f), ledge);
            road.shadowCastingMode = ShadowCastingMode.Off;
            road.gameObject.isStatic = true;
            for (float x = from - run - 6f; x > from - run - 110f; x -= 14f)
                foreach (float side in new[] { -1f, 1f })
                    SunCityKit.Smoulder(group, new Vector3(x, Low + 0.1f, side * 5f), 0.55f, false, 0f, false);

            var dice = new System.Random(7123);
            var keep = new[]
            {
                new Vector4(-(radius + 8f), radius + 8f, -(radius + 8f), radius + 8f), new Vector4(-13f, 13f, -46f, -16f),
                new Vector4(from - run - 130f, from + 2f, -6f, 6f),
            };
            FarTown(group, new Vector4(-150f, 150f, -150f, 130f), Low, keep, dice, 0.6f);
            // The dry river's line south-east of the court, and the dunes to the west where the sun goes down.
            var river = BuildKit.Shape(PrimitiveType.Cube, "Dry River", group, new Vector3(70f, Low + 0.03f, -40f), new Vector3(14f, 0.06f, 260f), SunCityKit.MudWorked, new Vector3(0f, 18f, 0f));
            river.shadowCastingMode = ShadowCastingMode.Off;
            river.gameObject.isStatic = true;
            Dunes(group, new Vector4(-330f, -170f, -200f, 200f), Low, 30, dice);
            SceneryMerge.Prepare(group, "court");
        }

        /// <summary>Lanes of flat-roofed houses in a rectangle (west, east, south, north) on ground at a height, kept out of the given rectangles.</summary>
        public static void FarTown(Transform parent, Vector4 area, float ground, Vector4[] keepOut, System.Random dice, float density)
        {
            var group = BuildKit.Group("Houses", parent);
            float z = area.z;
            while (z < area.w)
            {
                float blockDeep = 10f + (float)dice.NextDouble() * 8f;
                float x = area.x;
                while (x < area.y)
                {
                    float blockWide = 12f + (float)dice.NextDouble() * 14f;
                    float lane = 3f + (float)dice.NextDouble() * 2.5f;
                    if (dice.NextDouble() < density) Block(group, x, Mathf.Min(area.y, x + blockWide), z, z + blockDeep, ground, keepOut, dice);
                    else if (dice.NextDouble() < 0.3)
                    {
                        // A yard with a palm or two where a block is not built.
                        var at = new Vector3(x + blockWide * 0.5f, ground, z + blockDeep * 0.5f);
                        if (!Out(at.x - 3f, at.x + 3f, at.z - 3f, at.z + 3f, keepOut))
                            SunCityKit.City(dice.NextDouble() < 0.5 ? "palm_a" : "palm_b", group, at, (float)dice.NextDouble() * 360f, Vector3.one * (0.9f + (float)dice.NextDouble() * 0.3f), null, false);
                    }
                    x += blockWide + lane;
                }
                z += blockDeep + 3f + (float)dice.NextDouble() * 3f;
            }
        }

        private static bool Out(float west, float east, float south, float north, Vector4[] keepOut)
        {
            foreach (var r in keepOut)
                if (east > r.x && west < r.y && north > r.z && south < r.w) return true;
            return false;
        }

        // A block split into houses along its length, each its own height, some with an upper room, a doorway on the
        // south (the side the camera sees), now and then a hearth glowing in it and smoke going up.
        private static void Block(Transform parent, float west, float east, float south, float north, float ground, Vector4[] keepOut, System.Random dice)
        {
            float x = west;
            while (x < east - 2.5f)
            {
                float wide = Mathf.Min(east - x, 4f + (float)dice.NextDouble() * 4f);
                float w = x, e = x + wide;
                x = e;
                if (Out(w, e, south, north, keepOut)) continue;
                float high = 3f + (float)dice.NextDouble() * 3.6f;
                var front = fronts[dice.Next(fronts.Length)];
                Piece(parent, "House", new Vector3((w + e) * 0.5f, ground + high * 0.5f, (south + north) * 0.5f), new Vector3(wide - 0.05f, high, north - south), front);
                Piece(parent, "Roof", new Vector3((w + e) * 0.5f, ground + high + 0.02f, (south + north) * 0.5f), new Vector3(wide - 0.35f, 0.08f, north - south - 0.35f), roofTop);
                Piece(parent, "Parapet", new Vector3((w + e) * 0.5f, ground + high + 0.2f, south + 0.1f), new Vector3(wide - 0.05f, 0.4f, 0.2f), front);
                if (dice.NextDouble() < 0.35)
                {
                    // An upper room at the back of the roof.
                    float up = 2.2f + (float)dice.NextDouble() * 0.8f, deep = (north - south) * 0.45f;
                    Piece(parent, "Upper Room", new Vector3((w + e) * 0.5f, ground + high + up * 0.5f, north - deep * 0.5f), new Vector3(wide * 0.7f, up, deep), front);
                }
                if (dice.NextDouble() < 0.6)
                {
                    float door = w + wide * (0.3f + (float)dice.NextDouble() * 0.4f);
                    bool lit = dice.NextDouble() < 0.18;
                    Piece(parent, "Door", new Vector3(door, ground + 1.1f, south - 0.03f), new Vector3(1.1f, 2.2f, 0.06f), lit ? hearth : doorway);
                    if (lit && dice.NextDouble() < 0.5)
                    {
                        // A hearth on the roof: its fire, and now and then its smoke going up.
                        var roof = new Vector3((w + e) * 0.5f, ground + high + 0.1f, (south + north) * 0.5f + 1f);
                        SunCityKit.Smoulder(parent, roof, 0.5f, false, 0f, false);
                        if (dice.NextDouble() < 0.35) SunCityKit.SmokeColumn(parent, roof + Vector3.up * 0.4f, 1.2f);
                    }
                }
            }
        }

        private static void Piece(Transform parent, string name, Vector3 at, Vector3 size, Material material)
        {
            var shown = BuildKit.Shape(PrimitiveType.Cube, name, parent, at, size, material);
            shown.shadowCastingMode = ShadowCastingMode.Off;
            shown.gameObject.isStatic = true;
        }

        // Dunes: long low mounds of rippled sand, lying across the wind.
        private static void Dunes(Transform parent, Vector4 area, float ground, int count, System.Random dice)
        {
            var group = BuildKit.Group("Dunes", parent);
            for (int i = 0; i < count; i++)
            {
                var at = new Vector3(Mathf.Lerp(area.x, area.y, (float)dice.NextDouble()), ground, Mathf.Lerp(area.z, area.w, (float)dice.NextDouble()));
                float length = 40f + (float)dice.NextDouble() * 70f, high = 6f + (float)dice.NextDouble() * 10f;
                var dune = BuildKit.Shape(PrimitiveType.Sphere, "Dune", group, at, new Vector3(length * 0.45f, high, length), sand, new Vector3(0f, 70f + (float)dice.NextDouble() * 40f, 0f));
                dune.shadowCastingMode = ShadowCastingMode.Off;
                dune.gameObject.isStatic = true;
            }
        }
    }
}
