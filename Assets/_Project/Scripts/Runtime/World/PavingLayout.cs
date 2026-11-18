using System.Collections.Generic;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Lays a stretch of paving as one mesh: courses of slabs running across the whole stretch (no tile repeats), each
    /// slab its own length within the course, a little higher or lower than its neighbours, some with a corner chipped
    /// off or cracked through, a few recut in paler stone. Under each slab is a bed the width of its joint, a shade
    /// darker than the stone, so the joints read as thin lines and not as a board of tiles. Slabs are missing where the
    /// paving has worn through (patches given by the builder, and the odd single one); there the ground it lies on
    /// shows. Submeshes: 0 the joints' bed, 1 the stone, 2 the stone a shade darker, 3 the paler recut slabs.
    /// </summary>
    public static class PavingLayout
    {
        public struct Settings
        {
            public float West, East, South, North, Top;
            public float SlabMin, SlabMax;
            public float Joint;
            /// <summary>The share of slabs missing one by one, anywhere.</summary>
            public float Loss;
            /// <summary>Worn-through patches: x, z, radius (and w unused).</summary>
            public Vector4[] Bare;
            /// <summary>A rectangle left unpaved (west, east, south, north), or zero for none.</summary>
            public Vector4 Hole;
            public int Seed;
            /// <summary>With four colours (linear), one submesh whose points carry them instead of four.</summary>
            public Color[] Tints;
        }

        public static Mesh Build(Settings settings)
        {
            var random = new System.Random(settings.Seed);
            float Next() => (float)random.NextDouble();
            var points = new List<Vector3>();
            var normals = new List<Vector3>();
            var lists = new[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
            float joint = Mathf.Max(0.01f, settings.Joint);
            float bed = settings.Top + 0.008f;
            float noiseX = Next() * 100f, noiseZ = Next() * 100f;
            float z = settings.South;
            while (z < settings.North - 0.2f)
            {
                float depth = Mathf.Lerp(settings.SlabMin, settings.SlabMax, 0.25f + Next() * 0.5f);
                if (settings.North - z - depth < settings.SlabMin * 0.6f) depth = settings.North - z;
                float x = settings.West;
                // Each course starts with a part slab of its own length, so joints never line up course to course.
                float first = Mathf.Lerp(settings.SlabMin, settings.SlabMax, Next()) * (0.3f + 0.7f * Next());
                bool start = true;
                while (x < settings.East - 0.05f)
                {
                    float length = start ? first : Mathf.Lerp(settings.SlabMin, settings.SlabMax * 1.25f, Next());
                    start = false;
                    if (settings.East - x - length < settings.SlabMin * 0.45f) length = settings.East - x;
                    float x0 = x, x1 = Mathf.Min(settings.East, x + length), z0 = z, z1 = z + depth;
                    x = x1;
                    float cx = (x0 + x1) * 0.5f, cz = (z0 + z1) * 0.5f;
                    float roll = Next(), shade = Next(), height = 0.04f + Next() * 0.03f;
                    float chip = Next(), crack = Next(), corner = Next();
                    if (Lost(settings, cx, cz, roll, noiseX, noiseZ)) continue;
                    // The bed under the slab, out to the middle of each joint.
                    Quad(points, normals, lists[0], new Vector3(x0, bed, z0), new Vector3(x1, bed, z1));
                    int tone = shade < 0.08f ? 3 : shade < 0.5f ? 1 : 2;
                    var outline = new List<Vector2>
                    {
                        new Vector2(x0 + joint * 0.5f, z0 + joint * 0.5f), new Vector2(x1 - joint * 0.5f, z0 + joint * 0.5f),
                        new Vector2(x1 - joint * 0.5f, z1 - joint * 0.5f), new Vector2(x0 + joint * 0.5f, z1 - joint * 0.5f),
                    };
                    if (chip < 0.3f) Chip(outline, (int)(corner * 4f) % 4, 0.07f + 0.12f * Next());
                    float top = bed + height;
                    if (crack < 0.07f && x1 - x0 > 0.5f)
                    {
                        // Cracked through: two pieces with a hair of a gap, one sunk a little.
                        float t = 0.35f + 0.3f * Next();
                        var (a, b) = Split(outline, Mathf.Lerp(x0, x1, t), Next() > 0.5f, joint * 0.4f);
                        Slab(points, normals, lists[tone], a, bed, top);
                        Slab(points, normals, lists[tone], b, bed, top - 0.012f);
                    }
                    else Slab(points, normals, lists[tone], outline, bed, top);
                }
                z += depth;
            }
            var mesh = new Mesh { name = "paving" };
            if (points.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(points);
            mesh.SetNormals(normals);
            if (settings.Tints != null && settings.Tints.Length >= 4)
            {
                var colours = new Color[points.Count];
                var all = new List<int>();
                for (int i = 0; i < 4; i++)
                {
                    foreach (int vertex in lists[i]) colours[vertex] = settings.Tints[i];
                    all.AddRange(lists[i]);
                }
                mesh.colors = colours;
                mesh.SetTriangles(all, 0, false);
            }
            else
            {
                mesh.subMeshCount = 4;
                for (int i = 0; i < 4; i++) mesh.SetTriangles(lists[i], i, false);
            }
            mesh.RecalculateBounds();
            return mesh;
        }

        private static bool Lost(Settings settings, float x, float z, float roll, float noiseX, float noiseZ)
        {
            var hole = settings.Hole;
            if (hole.y > hole.x && x > hole.x && x < hole.y && z > hole.z && z < hole.w) return true;
            if (roll < settings.Loss) return true;
            if (settings.Bare == null) return false;
            // A worn patch has a wandering edge: its radius swells and shrinks with the noise round it.
            float wobble = Mathf.PerlinNoise(x * 0.6f + noiseX, z * 0.6f + noiseZ) - 0.5f;
            foreach (var patch in settings.Bare)
            {
                float dx = x - patch.x, dz = z - patch.y;
                float reach = patch.z * (1f + wobble * 0.9f);
                if (dx * dx + dz * dz < reach * reach) return true;
            }
            return false;
        }

        private static void Quad(List<Vector3> points, List<Vector3> normals, List<int> triangles, Vector3 low, Vector3 high)
        {
            int at = points.Count;
            points.Add(new Vector3(low.x, low.y, low.z));
            points.Add(new Vector3(low.x, low.y, high.z));
            points.Add(new Vector3(high.x, low.y, high.z));
            points.Add(new Vector3(high.x, low.y, low.z));
            for (int i = 0; i < 4; i++) normals.Add(Vector3.up);
            triangles.Add(at); triangles.Add(at + 1); triangles.Add(at + 2);
            triangles.Add(at); triangles.Add(at + 2); triangles.Add(at + 3);
        }

        // A slab: its top as a fan over its outline (counter-clockwise from above), and the sides the camera can see
        // (those facing south, east and west; it looks north, so never a north side).
        private static void Slab(List<Vector3> points, List<Vector3> normals, List<int> triangles, List<Vector2> outline, float bottom, float top)
        {
            int at = points.Count;
            foreach (var p in outline)
            {
                points.Add(new Vector3(p.x, top, p.y));
                normals.Add(Vector3.up);
            }
            for (int i = 1; i + 1 < outline.Count; i++)
            {
                triangles.Add(at);
                triangles.Add(at + i + 1);
                triangles.Add(at + i);
            }
            for (int i = 0; i < outline.Count; i++)
            {
                Vector2 a = outline[i], b = outline[(i + 1) % outline.Count];
                Vector2 along = b - a;
                if (along.sqrMagnitude < 0.0001f) continue;
                // Outward for a counter-clockwise outline seen from above (x right, z up) is (along.y, -along.x).
                var outward = new Vector3(along.y, 0f, -along.x).normalized;
                if (outward.z > 0.5f) continue;
                int side = points.Count;
                points.Add(new Vector3(a.x, bottom, a.y));
                points.Add(new Vector3(b.x, bottom, b.y));
                points.Add(new Vector3(b.x, top, b.y));
                points.Add(new Vector3(a.x, top, a.y));
                for (int k = 0; k < 4; k++) normals.Add(outward);
                triangles.Add(side); triangles.Add(side + 2); triangles.Add(side + 1);
                triangles.Add(side); triangles.Add(side + 3); triangles.Add(side + 2);
            }
        }

        // Cuts a corner off a four-cornered outline.
        private static void Chip(List<Vector2> outline, int corner, float size)
        {
            Vector2 p = outline[corner], previous = outline[(corner + 3) % 4], next = outline[(corner + 1) % 4];
            var a = p + (previous - p).normalized * Mathf.Min(size, (previous - p).magnitude * 0.4f);
            var b = p + (next - p).normalized * Mathf.Min(size * (0.7f + 0.6f * ((corner * 7) % 3) / 2f), (next - p).magnitude * 0.4f);
            outline.RemoveAt(corner);
            outline.Insert(corner, b);
            outline.Insert(corner, a);
        }

        // Splits a convex outline along a slanting line through x at its middle, leaving a gap between the halves.
        private static (List<Vector2>, List<Vector2>) Split(List<Vector2> outline, float x, bool leanRight, float gap)
        {
            float minZ = float.MaxValue, maxZ = float.MinValue;
            foreach (var p in outline) { minZ = Mathf.Min(minZ, p.y); maxZ = Mathf.Max(maxZ, p.y); }
            float lean = (maxZ - minZ) * 0.35f * (leanRight ? 1f : -1f);
            var from = new Vector2(x - lean, minZ - 1f);
            var to = new Vector2(x + lean, maxZ + 1f);
            var normal = new Vector2(to.y - from.y, -(to.x - from.x)).normalized;
            return (Clip(outline, from, normal, gap * 0.5f), Clip(outline, from, -normal, gap * 0.5f));
        }

        // The part of a convex outline on the side of a line its normal points away from, the line moved back by inset.
        private static List<Vector2> Clip(List<Vector2> outline, Vector2 through, Vector2 normal, float inset)
        {
            var kept = new List<Vector2>();
            for (int i = 0; i < outline.Count; i++)
            {
                Vector2 a = outline[i], b = outline[(i + 1) % outline.Count];
                float da = Vector2.Dot(a - through, normal) + inset, db = Vector2.Dot(b - through, normal) + inset;
                if (da <= 0f) kept.Add(a);
                if ((da < 0f) != (db < 0f)) kept.Add(Vector2.Lerp(a, b, da / (da - db)));
            }
            return kept;
        }
    }
}
