using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The meshes burning ground is drawn with (clover_scorch.shader and clover_flame_field.shader): a quad for the char
    /// of a lane or a disc, and fields of flames, each flame four corners at its own foot with its own seed, height,
    /// width and place, scattered irregularly (clumped, with gaps and the odd tall tongue) so a burning patch never reads
    /// as a row of the same puff. Made once, when a burning patch's owner wakes, never during play.
    /// </summary>
    public static class FlameField
    {
        // Flames stand up out of the floor toward the camera: the bounds are kept wide so they are not culled early.
        private static readonly Bounds Wide = new Bounds(new Vector3(0f, 2f, 0.5f), new Vector3(3f, 6f, 3f));

        /// <summary>A lane's char: a quad one wide and one long, from its start (z 0) to its end (z 1); u across, v along.</summary>
        public static Mesh LaneGround()
        {
            var mesh = new Mesh { name = "Scorch Lane" };
            mesh.vertices = new[] { new Vector3(-0.5f, 0f, 0f), new Vector3(0.5f, 0f, 0f), new Vector3(-0.5f, 0f, 1f), new Vector3(0.5f, 0f, 1f) };
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>A disc's char: a quad two across, round its middle, which the shader cuts round.</summary>
        public static Mesh DiscGround()
        {
            var mesh = new Mesh { name = "Scorch Disc" };
            // Wider than the disc by a little, so the ragged char may spill past the edge that bites.
            const float Out = 1.25f;
            mesh.vertices = new[] { new Vector3(-Out, 0f, -Out), new Vector3(Out, 0f, -Out), new Vector3(-Out, 0f, Out), new Vector3(Out, 0f, Out) };
            float lo = 0.5f - Out * 0.5f, hi = 0.5f + Out * 0.5f;
            mesh.uv = new[] { new Vector2(lo, lo), new Vector2(hi, lo), new Vector2(lo, hi), new Vector2(hi, hi) };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Flames over a lane one wide and one long: <paramref name="count"/> of them, denser in the middle, taller there.</summary>
        public static Mesh Lane(int count, int seed)
        {
            var random = new System.Random(seed);
            var feet = new Vector3[count];
            var sizes = new Vector2[count];
            var data = new Color[count];
            // Stratified along the lane so it burns its whole length, jittered and clumped so it is never a row.
            for (int i = 0; i < count; i++)
            {
                float along = Mathf.Clamp01((i + (float)random.NextDouble()) / count);
                float across = ((float)random.NextDouble() - 0.5f) * 0.96f;
                // Clumps: half the flames lean toward a few hot runs down the lane.
                float run = Mathf.Sin(along * 23f + seed) * 0.22f;
                if (random.NextDouble() < 0.45) across = Mathf.Lerp(across, run, 0.6f);
                float edge = Mathf.Clamp01(Mathf.Abs(across) * 2f + ((float)random.NextDouble() - 0.5f) * 0.18f);
                feet[i] = new Vector3(across, 0.02f, along);
                float tall = Mathf.Lerp(0.45f, 1.05f, (float)random.NextDouble()) * Mathf.Lerp(1.15f, 0.7f, edge);
                if (random.NextDouble() < 0.08) tall *= 1.6f;
                sizes[i] = new Vector2(tall, Mathf.Lerp(0.6f, 1.15f, (float)random.NextDouble()));
                data[i] = new Color((float)random.NextDouble(), edge, (float)random.NextDouble(), along);
            }
            return Build("Flame Lane", feet, sizes, data);
        }

        /// <summary>Flames over a disc of radius one: spread evenly over its area, lower toward the rim.</summary>
        public static Mesh Disc(int count, int seed)
        {
            var random = new System.Random(seed);
            var feet = new Vector3[count];
            var sizes = new Vector2[count];
            var data = new Color[count];
            for (int i = 0; i < count; i++)
            {
                // A sunflower spiral, jittered: even over the disc without the holes of plain chance.
                float radius = Mathf.Sqrt((i + 0.5f) / count) * 0.97f;
                float turn = i * 2.39996f + (float)random.NextDouble() * 0.6f;
                radius = Mathf.Clamp01(radius + ((float)random.NextDouble() - 0.5f) * 0.08f);
                feet[i] = new Vector3(Mathf.Sin(turn) * radius, 0.02f, Mathf.Cos(turn) * radius);
                float edge = Mathf.Clamp01(radius + ((float)random.NextDouble() - 0.5f) * 0.12f);
                float tall = Mathf.Lerp(0.5f, 1.05f, (float)random.NextDouble()) * Mathf.Lerp(1.1f, 0.75f, edge);
                if (random.NextDouble() < 0.07) tall *= 1.5f;
                sizes[i] = new Vector2(tall, Mathf.Lerp(0.6f, 1.15f, (float)random.NextDouble()));
                data[i] = new Color((float)random.NextDouble(), edge, (float)random.NextDouble(), 0f);
            }
            return Build("Flame Disc", feet, sizes, data);
        }

        /// <summary>
        /// A wall of flames round a circle of radius one: two rows, the outer taller and denser, each tongue its own
        /// height. Its place round the ring is its "along", so the wall can rise and sink one tongue after another;
        /// its edge share grows round the ring for the same reason.
        /// </summary>
        public static Mesh Ring(int count, int seed)
        {
            var random = new System.Random(seed);
            var feet = new Vector3[count];
            var sizes = new Vector2[count];
            var data = new Color[count];
            for (int i = 0; i < count; i++)
            {
                bool outer = i % 3 != 0;
                float round = (i + (float)random.NextDouble() * 0.8f) / count;
                float turn = round * Mathf.PI * 2f;
                float radius = (outer ? 1f : 0.93f) + ((float)random.NextDouble() - 0.5f) * 0.04f;
                feet[i] = new Vector3(Mathf.Sin(turn) * radius, 0.02f, Mathf.Cos(turn) * radius);
                float tall = Mathf.Lerp(0.7f, 1.15f, (float)random.NextDouble()) * (outer ? 1f : 0.7f);
                if (random.NextDouble() < 0.12) tall *= 1.3f;
                sizes[i] = new Vector2(tall, Mathf.Lerp(0.75f, 1.2f, (float)random.NextDouble()));
                data[i] = new Color((float)random.NextDouble(), round * 0.6f, (float)random.NextDouble(), round);
            }
            return Build("Flame Ring", feet, sizes, data);
        }

        private static Mesh Build(string name, Vector3[] feet, Vector2[] sizes, Color[] data)
        {
            int count = feet.Length;
            var vertices = new Vector3[count * 4];
            var corners = new Vector2[count * 4];
            var size = new Vector2[count * 4];
            var colours = new Color[count * 4];
            var triangles = new int[count * 6];
            for (int i = 0; i < count; i++)
            {
                for (int k = 0; k < 4; k++)
                {
                    int v = i * 4 + k;
                    vertices[v] = feet[i];
                    corners[v] = new Vector2((k & 1) == 0 ? -0.5f : 0.5f, (k & 2) == 0 ? 0f : 1f);
                    size[v] = sizes[i];
                    colours[v] = data[i];
                }
                int t = i * 6, b = i * 4;
                triangles[t] = b; triangles[t + 1] = b + 2; triangles[t + 2] = b + 1;
                triangles[t + 3] = b + 1; triangles[t + 4] = b + 2; triangles[t + 5] = b + 3;
            }
            var mesh = new Mesh { name = name };
            mesh.vertices = vertices;
            mesh.uv = corners;
            mesh.uv2 = size;
            mesh.colors = colours;
            mesh.triangles = triangles;
            mesh.bounds = Wide;
            return mesh;
        }
    }
}
