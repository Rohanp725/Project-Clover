using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The solid shapes spells are made of, drawn in code from a few bold wedges each: no curve that a high camera
    /// would lose. Every face is flat (its corners are not shared), and each corner also carries, in its second set
    /// of UVs, the direction smoothed across the faces that meet there, which is what the ink line is pushed out along.
    /// Things that fly point along +Z from the origin; things that stand rise along +Y from the origin.
    /// </summary>
    public static class FormMeshes
    {
        public static Mesh Build(FormMesh which)
        {
            var made = new Maker();
            switch (which)
            {
                case FormMesh.Blade:
                    // A throwing blade: a long leaf, a cross-guard, a stub of a tang.
                    made.Bipyramid(new Vector3(0f, 0f, 0.16f), new Vector3(0f, 0f, 1f), 0.24f, Vector3.right * 0.13f, Vector3.up * 0.045f);
                    made.Bipyramid(new Vector3(-0.27f, 0f, 0.2f), new Vector3(0.27f, 0f, 0.2f), 0.5f, Vector3.forward * 0.06f, Vector3.up * 0.06f);
                    made.Bipyramid(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.22f), 0.5f, Vector3.right * 0.045f, Vector3.up * 0.045f);
                    break;
                case FormMesh.Sword:
                    // A great sword: a broad blade with a spine, a wide guard swept toward the point, a grip and a pommel.
                    made.Bipyramid(new Vector3(0f, 0f, 0.2f), new Vector3(0f, 0f, 1f), 0.2f, Vector3.right * 0.085f, Vector3.up * 0.03f);
                    made.Bipyramid(new Vector3(-0.26f, 0f, 0.26f), new Vector3(0f, 0f, 0.17f), 0.7f, Vector3.forward * 0.035f, Vector3.up * 0.035f);
                    made.Bipyramid(new Vector3(0.26f, 0f, 0.26f), new Vector3(0f, 0f, 0.17f), 0.7f, Vector3.forward * 0.035f, Vector3.up * 0.035f);
                    made.Bipyramid(new Vector3(0f, 0f, 0.03f), new Vector3(0f, 0f, 0.21f), 0.5f, Vector3.right * 0.022f, Vector3.up * 0.022f);
                    made.Bipyramid(new Vector3(0f, 0f, -0.03f), new Vector3(0f, 0f, 0.05f), 0.5f, Vector3.right * 0.05f, Vector3.up * 0.05f);
                    break;
                case FormMesh.Shard:
                    made.Bipyramid(Vector3.zero, new Vector3(0f, 0f, 1f), 0.3f, Vector3.right * 0.13f, Vector3.up * 0.13f);
                    break;
                case FormMesh.Spike:
                    // A fang of rock or ice: five sides, its point leaning a little, a splinter at its foot.
                    made.Cone(Vector3.zero, new Vector3(0.08f, 1f, 0.05f), 0.5f, 5, 0f);
                    made.Cone(new Vector3(0.3f, 0f, -0.2f), new Vector3(0.52f, 0.45f, -0.36f), 0.2f, 4, 0.6f);
                    break;
                case FormMesh.Pillar:
                    // A six-sided column with a broken top.
                    made.Prism(0.5f, 0.86f, 6);
                    made.Cone(new Vector3(0f, 0.86f, 0f), new Vector3(0.12f, 1f, -0.1f), 0.5f, 6, 0f);
                    break;
                case FormMesh.Disc:
                    made.Saw(1f, 0.7f, 0.14f, 8);
                    break;
                case FormMesh.Orb:
                    made.Gem(1f);
                    break;
                case FormMesh.Slab:
                    made.Slab();
                    break;
                case FormMesh.DragonSkull:
                    // The upper head: a long snout, a heavy brow, two horns swept back, fangs under the lip.
                    made.Hull(new[]
                    {
                        new Vector3(0f, 0.06f, 1f), new Vector3(-0.2f, 0.02f, 0.7f), new Vector3(0.2f, 0.02f, 0.7f),
                        new Vector3(0f, 0.3f, 0.62f), new Vector3(-0.36f, 0.02f, 0.05f), new Vector3(0.36f, 0.02f, 0.05f),
                        new Vector3(-0.3f, 0.42f, 0.2f), new Vector3(0.3f, 0.42f, 0.2f), new Vector3(0f, 0.5f, -0.1f),
                        new Vector3(0f, 0.05f, -0.2f),
                    }, new[]
                    {
                        0, 3, 1,  0, 2, 3,  0, 1, 2,  1, 3, 6,  2, 7, 3,  3, 7, 8,  3, 8, 6,  1, 6, 4,  2, 5, 7,
                        6, 8, 9,  7, 9, 8,  4, 6, 9,  5, 9, 7,  1, 4, 5,  1, 5, 2,  4, 9, 5,
                    });
                    made.Bipyramid(new Vector3(-0.24f, 0.4f, 0.12f), new Vector3(-0.5f, 0.92f, -0.7f), 0.25f, Vector3.right * 0.09f, new Vector3(0f, 0.06f, 0.07f));
                    made.Bipyramid(new Vector3(0.24f, 0.4f, 0.12f), new Vector3(0.5f, 0.92f, -0.7f), 0.25f, Vector3.right * 0.09f, new Vector3(0f, 0.06f, 0.07f));
                    made.Cone(new Vector3(-0.15f, 0.04f, 0.72f), new Vector3(-0.15f, -0.2f, 0.74f), 0.06f, 4, 0f);
                    made.Cone(new Vector3(0.15f, 0.04f, 0.72f), new Vector3(0.15f, -0.2f, 0.74f), 0.06f, 4, 0f);
                    made.Cone(new Vector3(-0.26f, 0.04f, 0.4f), new Vector3(-0.26f, -0.14f, 0.4f), 0.05f, 4, 0f);
                    made.Cone(new Vector3(0.26f, 0.04f, 0.4f), new Vector3(0.26f, -0.14f, 0.4f), 0.05f, 4, 0f);
                    break;
                case FormMesh.DragonJaw:
                    // The lower jaw, hinged at the origin: a wedge with two fangs standing up at its tip.
                    made.Hull(new[]
                    {
                        new Vector3(0f, 0f, 0.92f), new Vector3(-0.3f, 0f, 0f), new Vector3(0.3f, 0f, 0f), new Vector3(0f, -0.2f, 0.2f),
                    }, new[] { 0, 2, 1,  0, 1, 3,  0, 3, 2,  1, 2, 3 });
                    made.Cone(new Vector3(-0.1f, 0f, 0.74f), new Vector3(-0.1f, 0.18f, 0.76f), 0.05f, 4, 0f);
                    made.Cone(new Vector3(0.1f, 0f, 0.74f), new Vector3(0.1f, 0.18f, 0.76f), 0.05f, 4, 0f);
                    break;
                case FormMesh.DragonSpine:
                    // One joint of the neck: a thick diamond with a fin along its back.
                    made.Bipyramid(new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 0.5f), 0.55f, Vector3.right * 0.34f, Vector3.up * 0.28f);
                    made.Bipyramid(new Vector3(0f, 0.15f, 0.2f), new Vector3(0f, 0.62f, -0.4f), 0.3f, Vector3.right * 0.06f, new Vector3(0f, 0.1f, 0.14f));
                    break;
                case FormMesh.Drill:
                    // A bit: a long point, a wider collar behind it, three flukes.
                    made.Cone(new Vector3(0f, 0f, 0.3f), new Vector3(0f, 0f, 1.5f), 0.34f, 6, 0f);
                    made.Cone(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.75f), 0.62f, 6, 0.5f);
                    for (int i = 0; i < 3; i++)
                    {
                        var turn = Quaternion.Euler(0f, 0f, i * 120f);
                        made.Bipyramid(turn * new Vector3(0.3f, 0f, 0.1f), turn * new Vector3(0.95f, 0f, -0.35f), 0.3f, turn * new Vector3(0f, 0.09f, 0f), new Vector3(0f, 0f, 0.16f));
                    }
                    break;
                case FormMesh.Minion:
                    // A small helper: a body like a flame standing on its point, a head, two horns, one blade held out.
                    made.Bipyramid(new Vector3(0f, 0.05f, 0f), new Vector3(0f, 0.78f, 0f), 0.68f, Vector3.right * 0.26f, Vector3.forward * 0.2f);
                    made.Bipyramid(new Vector3(0f, 0.74f, 0.02f), new Vector3(0f, 1.12f, 0.02f), 0.45f, Vector3.right * 0.2f, Vector3.forward * 0.2f);
                    made.Cone(new Vector3(-0.12f, 1.02f, 0f), new Vector3(-0.3f, 1.38f, -0.1f), 0.07f, 4, 0f);
                    made.Cone(new Vector3(0.12f, 1.02f, 0f), new Vector3(0.3f, 1.38f, -0.1f), 0.07f, 4, 0f);
                    made.Bipyramid(new Vector3(0.24f, 0.56f, 0.05f), new Vector3(0.3f, 0.62f, 0.95f), 0.25f, Vector3.right * 0.035f, Vector3.up * 0.1f);
                    break;
            }
            return made.Finish("Form " + which);
        }

        /// <summary>A flat square on the ground, one metre across, centred, facing up.</summary>
        public static Mesh Square() => Flat("Form Mark", new Vector3(-0.5f, 0f, -0.5f), new Vector3(-0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, -0.5f),
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f));

        /// <summary>A flat strip on the ground running one metre along +Z from the origin, one metre wide; u runs along it.</summary>
        public static Mesh Strip() => Flat("Form Strip", new Vector3(-0.5f, 0f, 0f), new Vector3(-0.5f, 0f, 1f), new Vector3(0.5f, 0f, 1f), new Vector3(0.5f, 0f, 0f),
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));

        private static Mesh Flat(string name, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = new[] { a, b, c, d };
            mesh.uv = new[] { ua, ub, uc, ud };
            mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        // Collects triangles piece by piece. Every piece is convex, so each of its faces is turned to look away from
        // the piece's own middle: nobody has to get the winding right by hand.
        private class Maker
        {
            private readonly List<Vector3> corners = new List<Vector3>();

            private void Piece(List<Vector3> triangles)
            {
                Vector3 middle = Vector3.zero;
                foreach (var point in triangles) middle += point;
                middle /= Mathf.Max(1, triangles.Count);
                for (int i = 0; i + 2 < triangles.Count; i += 3)
                {
                    Vector3 a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                    Vector3 normal = Vector3.Cross(b - a, c - a);
                    if (normal.sqrMagnitude < 1e-10f) continue;
                    bool outward = Vector3.Dot(normal, (a + b + c) / 3f - middle) >= 0f;
                    corners.Add(a);
                    corners.Add(outward ? b : c);
                    corners.Add(outward ? c : b);
                }
            }

            /// <summary>Two points joined through a ring of four corners part-way between them.</summary>
            public void Bipyramid(Vector3 from, Vector3 to, float at, Vector3 side, Vector3 up)
            {
                Vector3 middle = Vector3.Lerp(from, to, at);
                var ring = new[] { middle + side, middle + up, middle - side, middle - up };
                var triangles = new List<Vector3>();
                for (int i = 0; i < 4; i++)
                {
                    triangles.Add(from); triangles.Add(ring[i]); triangles.Add(ring[(i + 1) % 4]);
                    triangles.Add(to); triangles.Add(ring[i]); triangles.Add(ring[(i + 1) % 4]);
                }
                Piece(triangles);
            }

            /// <summary>A pyramid: a ring of corners round a foot, drawn to a point. The ring lies square to the line from foot to point.</summary>
            public void Cone(Vector3 foot, Vector3 point, float radius, int sides, float turn)
            {
                Vector3 axis = (point - foot).normalized;
                Vector3 across = Vector3.Cross(axis, Mathf.Abs(axis.y) > 0.9f ? Vector3.forward : Vector3.up).normalized;
                Vector3 other = Vector3.Cross(axis, across);
                var triangles = new List<Vector3>();
                for (int i = 0; i < sides; i++)
                {
                    float a = (i + turn) * Mathf.PI * 2f / sides, b = (i + 1 + turn) * Mathf.PI * 2f / sides;
                    Vector3 first = foot + (across * Mathf.Cos(a) + other * Mathf.Sin(a)) * radius;
                    Vector3 second = foot + (across * Mathf.Cos(b) + other * Mathf.Sin(b)) * radius;
                    triangles.Add(point); triangles.Add(first); triangles.Add(second);
                    triangles.Add(foot); triangles.Add(first); triangles.Add(second);
                }
                Piece(triangles);
            }

            /// <summary>An upright column standing on the origin.</summary>
            public void Prism(float radius, float height, int sides)
            {
                var triangles = new List<Vector3>();
                Vector3 top = Vector3.up * height;
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2f / sides, b = (i + 1) * Mathf.PI * 2f / sides;
                    Vector3 first = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius, second = new Vector3(Mathf.Cos(b), 0f, Mathf.Sin(b)) * radius;
                    triangles.Add(first); triangles.Add(second); triangles.Add(second + top);
                    triangles.Add(first); triangles.Add(second + top); triangles.Add(first + top);
                    triangles.Add(Vector3.zero); triangles.Add(first); triangles.Add(second);
                }
                Piece(triangles);
            }

            /// <summary>A flat wheel of teeth lying in the ground plane, thick at the hub and sharp at the rim.</summary>
            public void Saw(float outer, float inner, float thick, int teeth)
            {
                // Not convex as a whole, so each tooth is its own piece: hub top, hub bottom, and the two corners of the tooth.
                for (int i = 0; i < teeth; i++)
                {
                    float a = i * Mathf.PI * 2f / teeth, b = (i + 1) * Mathf.PI * 2f / teeth, tip = a + (b - a) * 0.22f;
                    Vector3 low = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * inner, next = new Vector3(Mathf.Cos(b), 0f, Mathf.Sin(b)) * inner;
                    Vector3 point = new Vector3(Mathf.Cos(tip), 0f, Mathf.Sin(tip)) * outer;
                    Vector3 top = Vector3.up * thick, bottom = Vector3.down * thick;
                    var triangles = new List<Vector3>
                    {
                        top, low, point,  top, point, next,  bottom, low, point,  bottom, point, next,
                    };
                    Piece(triangles);
                }
            }

            /// <summary>A cut stone: twenty flat faces.</summary>
            public void Gem(float radius)
            {
                float t = (1f + Mathf.Sqrt(5f)) * 0.5f;
                var points = new[]
                {
                    new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                    new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                    new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
                };
                var faces = new[]
                {
                    0, 11, 5,  0, 5, 1,  0, 1, 7,  0, 7, 10,  0, 10, 11,  1, 5, 9,  5, 11, 4,  11, 10, 2,  10, 7, 6,  7, 1, 8,
                    3, 9, 4,  3, 4, 2,  3, 2, 6,  3, 6, 8,  3, 8, 9,  4, 9, 5,  2, 4, 11,  6, 2, 10,  8, 6, 7,  9, 8, 1,
                };
                var triangles = new List<Vector3>();
                foreach (int index in faces) triangles.Add(points[index].normalized * radius);
                Piece(triangles);
            }

            /// <summary>A slab of wall, one metre wide and one tall, standing on the origin across X: a broken top, thick at the foot.</summary>
            public void Slab()
            {
                var front = new[] { new Vector3(-0.5f, 0f, 0f), new Vector3(0.5f, 0f, 0f), new Vector3(0.5f, 0.72f, 0f), new Vector3(0.12f, 1f, 0f), new Vector3(-0.5f, 0.84f, 0f) };
                var triangles = new List<Vector3>();
                Vector3 thick = Vector3.forward * 0.2f, thin = Vector3.forward * 0.1f;
                for (int i = 0; i < front.Length; i++)
                {
                    Vector3 a = front[i], b = front[(i + 1) % front.Length];
                    Vector3 depthA = a.y < 0.01f ? thick : thin, depthB = b.y < 0.01f ? thick : thin;
                    triangles.Add(a - depthA); triangles.Add(b - depthB); triangles.Add(b + depthB);
                    triangles.Add(a - depthA); triangles.Add(b + depthB); triangles.Add(a + depthA);
                }
                // The two faces, each a fan pushed out to a ridge in the middle so the slab is not a board.
                Vector3 ridge = new Vector3(0f, 0.5f, 0f);
                for (int i = 0; i < front.Length; i++)
                {
                    Vector3 a = front[i], b = front[(i + 1) % front.Length];
                    Vector3 depthA = a.y < 0.01f ? thick : thin, depthB = b.y < 0.01f ? thick : thin;
                    triangles.Add(ridge + Vector3.forward * 0.26f); triangles.Add(a + depthA); triangles.Add(b + depthB);
                    triangles.Add(ridge - Vector3.forward * 0.26f); triangles.Add(a - depthA); triangles.Add(b - depthB);
                }
                Piece(triangles);
            }

            /// <summary>A convex shape given as corners and the faces between them.</summary>
            public void Hull(Vector3[] points, int[] faces)
            {
                var triangles = new List<Vector3>();
                foreach (int index in faces) triangles.Add(points[index]);
                Piece(triangles);
            }

            public Mesh Finish(string name)
            {
                int count = corners.Count;
                var normals = new Vector3[count];
                var smooth = new List<Vector3>(count);
                var indices = new int[count];
                var summed = new Dictionary<Vector3Int, Vector3>();
                for (int i = 0; i < count; i += 3)
                {
                    Vector3 normal = Vector3.Cross(corners[i + 1] - corners[i], corners[i + 2] - corners[i]).normalized;
                    for (int k = 0; k < 3; k++)
                    {
                        normals[i + k] = normal;
                        indices[i + k] = i + k;
                        var key = Key(corners[i + k]);
                        summed.TryGetValue(key, out Vector3 sum);
                        summed[key] = sum + normal;
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    Vector3 sum = summed[Key(corners[i])];
                    smooth.Add(sum.sqrMagnitude > 1e-6f ? sum.normalized : normals[i]);
                }
                var mesh = new Mesh { name = name };
                mesh.SetVertices(corners);
                mesh.normals = normals;
                mesh.SetUVs(1, smooth);
                mesh.triangles = indices;
                mesh.RecalculateBounds();
                return mesh;
            }

            private static Vector3Int Key(Vector3 point) =>
                new Vector3Int(Mathf.RoundToInt(point.x * 2000f), Mathf.RoundToInt(point.y * 2000f), Mathf.RoundToInt(point.z * 2000f));
        }
    }
}
