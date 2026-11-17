using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Fire land's ground as made meshes (fire_levels.md, section 6.1): a plateau of any outline with a broken edge
    /// and faces that lean back, banded in strata if asked; a ramp with its sides down to the ground beside it; and the
    /// champion's sunk oval. Each carries a mesh collider of exactly what is drawn, so what is seen is what is stood on.
    /// </summary>
    public static class SunCityGround
    {
        private static System.Random dice = new System.Random(5150);

        public static void Seed(int seed) => dice = new System.Random(seed);

        private static float Roll(float half) => ((float)dice.NextDouble() * 2f - 1f) * half;

        /// <summary>
        /// A floor of any outline (x, z points in order round it) whose top is flat at <paramref name="top"/> and whose
        /// faces fall to <paramref name="bottom"/>, leaning back by <paramref name="leanDegrees"/>. Long edges are broken
        /// by up to <paramref name="jitter"/> metres in and out. With <paramref name="band"/> above zero the faces are
        /// drawn in strata that many metres high, alternating the two side materials.
        /// </summary>
        public static GameObject Plateau(string name, Transform parent, Vector2[] outline, float top, float bottom, Material topMaterial, Material sideMaterial,
            Material bandMaterial = null, float jitter = 0.4f, float leanDegrees = 0f, float band = 0f, float segment = 1.6f)
        {
            var ring = Broken(outline, jitter, segment);
            bool counter = SignedArea(ring) > 0f;
            var outward = Outwards(ring, counter);
            float lean = Mathf.Tan(leanDegrees * Mathf.Deg2Rad) * (top - bottom);

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var topTriangles = new List<int>();
            var sideA = new List<int>();
            var sideB = new List<int>();

            // The top: the broken outline, triangulated by cutting off ears.
            int start = vertices.Count;
            foreach (var point in ring)
            {
                vertices.Add(new Vector3(point.x, top, point.y));
                uvs.Add(new Vector2(point.x, point.y) * 0.25f);
            }
            foreach (var triangle in Ears(ring))
            {
                Vector3 a = vertices[start + triangle.x], b = vertices[start + triangle.y], c = vertices[start + triangle.z];
                bool up = Vector3.Cross(b - a, c - a).y > 0f;
                topTriangles.Add(start + triangle.x);
                topTriangles.Add(start + (up ? triangle.y : triangle.z));
                topTriangles.Add(start + (up ? triangle.z : triangle.y));
            }

            // The faces: one quad per edge, or a stack of bands.
            int bands = band > 0f ? Mathf.Max(1, Mathf.RoundToInt((top - bottom) / band)) : 1;
            float run = 0f;
            for (int i = 0; i < ring.Count; i++)
            {
                int j = (i + 1) % ring.Count;
                Vector2 p = ring[i], q = ring[j];
                Vector2 pFoot = p + outward[i] * lean, qFoot = q + outward[j] * lean;
                float length = (q - p).magnitude;
                Vector2 edgeOut = EdgeOut(p, q, counter);
                for (int k = 0; k < bands; k++)
                {
                    float t0 = (float)k / bands, t1 = (float)(k + 1) / bands;
                    // Strata wander a little in height so they are not ruled lines.
                    float y0 = Mathf.Lerp(top, bottom, t0), y1 = Mathf.Lerp(top, bottom, t1);
                    Vector2 p0 = Vector2.Lerp(p, pFoot, t0), p1 = Vector2.Lerp(p, pFoot, t1);
                    Vector2 q0 = Vector2.Lerp(q, qFoot, t0), q1 = Vector2.Lerp(q, qFoot, t1);
                    int at = vertices.Count;
                    vertices.Add(new Vector3(p0.x, y0, p0.y));
                    vertices.Add(new Vector3(q0.x, y0, q0.y));
                    vertices.Add(new Vector3(q1.x, y1, q1.y));
                    vertices.Add(new Vector3(p1.x, y1, p1.y));
                    uvs.Add(new Vector2(run, y0) * 0.3f);
                    uvs.Add(new Vector2(run + length, y0) * 0.3f);
                    uvs.Add(new Vector2(run + length, y1) * 0.3f);
                    uvs.Add(new Vector2(run, y1) * 0.3f);
                    var list = (k % 2 == 1 && bandMaterial != null) ? sideB : sideA;
                    Vector3 normal = Vector3.Cross(vertices[at + 1] - vertices[at], vertices[at + 3] - vertices[at]);
                    bool facesOut = normal.x * edgeOut.x + normal.z * edgeOut.y > 0f;
                    if (facesOut) { list.Add(at); list.Add(at + 1); list.Add(at + 2); list.Add(at); list.Add(at + 2); list.Add(at + 3); }
                    else { list.Add(at); list.Add(at + 2); list.Add(at + 1); list.Add(at); list.Add(at + 3); list.Add(at + 2); }
                }
                run += length;
            }

            var materials = new List<Material> { topMaterial, sideMaterial };
            var lists = new List<List<int>> { topTriangles, sideA };
            if (bandMaterial != null && sideB.Count > 0) { materials.Add(bandMaterial); lists.Add(sideB); }
            return Made(name, parent, vertices, uvs, lists, materials);
        }

        /// <summary>A walkway sloping from a foot to a head with no steps: its top, its two sides and its ends down to the lower ground.</summary>
        public static GameObject Ramp(string name, Transform parent, Vector3 foot, Vector3 head, float width, Material topMaterial, Material sideMaterial, float below = 0f)
        {
            Vector3 flat = head - foot;
            flat.y = 0f;
            Vector3 across = Vector3.Cross(Vector3.up, flat.normalized) * (width * 0.5f);
            float low = Mathf.Min(foot.y, head.y) - Mathf.Max(0.05f, below);
            Vector3 a = foot - across, b = foot + across, c = head + across, d = head - across;
            Vector3 aLow = new Vector3(a.x, low, a.z), bLow = new Vector3(b.x, low, b.z), cLow = new Vector3(c.x, low, c.z), dLow = new Vector3(d.x, low, d.z);
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var topList = new List<int>();
            var sideList = new List<int>();
            void Quad(List<int> into, Vector3 p, Vector3 q, Vector3 r, Vector3 s, Vector3 wantedNormal)
            {
                int at = vertices.Count;
                vertices.Add(p); vertices.Add(q); vertices.Add(r); vertices.Add(s);
                float l = (q - p).magnitude, h = (s - p).magnitude;
                uvs.Add(Vector2.zero); uvs.Add(new Vector2(l, 0f) * 0.3f); uvs.Add(new Vector2(l, h) * 0.3f); uvs.Add(new Vector2(0f, h) * 0.3f);
                bool right = Vector3.Dot(Vector3.Cross(q - p, s - p), wantedNormal) > 0f;
                if (right) { into.Add(at); into.Add(at + 1); into.Add(at + 2); into.Add(at); into.Add(at + 2); into.Add(at + 3); }
                else { into.Add(at); into.Add(at + 2); into.Add(at + 1); into.Add(at); into.Add(at + 3); into.Add(at + 2); }
            }
            Quad(topList, a, b, c, d, Vector3.up);
            Quad(sideList, b, c, cLow, bLow, across);
            Quad(sideList, d, a, aLow, dLow, -across);
            Quad(sideList, a, b, bLow, aLow, -flat);
            Quad(sideList, c, d, dLow, cLow, flat);
            return Made(name, parent, vertices, uvs, new List<List<int>> { topList, sideList }, new List<Material> { topMaterial, sideMaterial });
        }

        /// <summary>
        /// A ring of ground at <paramref name="top"/> round an inner outline, out to a rectangle, with the inner faces falling
        /// to <paramref name="floor"/>: how a sunk place (the champion's bowl) is cut into a level. The inner points go round once.
        /// </summary>
        public static GameObject Ring(string name, Transform parent, Vector2[] inner, Vector4 outer, float top, float floor, Material topMaterial, Material sideMaterial)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var topList = new List<int>();
            var sideList = new List<int>();
            Vector2 centre = Vector2.zero;
            foreach (var point in inner) centre += point;
            centre /= inner.Length;
            int n = inner.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                Vector2 p = inner[i], q = inner[j];
                Vector2 po = ToRectangle(centre, p - centre, outer), qo = ToRectangle(centre, q - centre, outer);
                int at = vertices.Count;
                vertices.Add(new Vector3(p.x, top, p.y)); vertices.Add(new Vector3(q.x, top, q.y));
                vertices.Add(new Vector3(qo.x, top, qo.y)); vertices.Add(new Vector3(po.x, top, po.y));
                foreach (var v in new[] { p, q, qo, po }) uvs.Add(v * 0.25f);
                bool up = Vector3.Cross(vertices[at + 1] - vertices[at], vertices[at + 3] - vertices[at]).y > 0f;
                if (up) { topList.Add(at); topList.Add(at + 1); topList.Add(at + 2); topList.Add(at); topList.Add(at + 2); topList.Add(at + 3); }
                else { topList.Add(at); topList.Add(at + 2); topList.Add(at + 1); topList.Add(at); topList.Add(at + 3); topList.Add(at + 2); }
                // The inner face, looking toward the middle.
                at = vertices.Count;
                vertices.Add(new Vector3(p.x, top, p.y)); vertices.Add(new Vector3(q.x, top, q.y));
                vertices.Add(new Vector3(q.x, floor, q.y)); vertices.Add(new Vector3(p.x, floor, p.y));
                float l = (q - p).magnitude;
                uvs.Add(new Vector2(0f, top) * 0.3f); uvs.Add(new Vector2(l, top) * 0.3f); uvs.Add(new Vector2(l, floor) * 0.3f); uvs.Add(new Vector2(0f, floor) * 0.3f);
                Vector2 mid = (p + q) * 0.5f, toward = centre - mid;
                Vector3 normal = Vector3.Cross(vertices[at + 1] - vertices[at], vertices[at + 3] - vertices[at]);
                bool inward = normal.x * toward.x + normal.z * toward.y > 0f;
                if (inward) { sideList.Add(at); sideList.Add(at + 1); sideList.Add(at + 2); sideList.Add(at); sideList.Add(at + 2); sideList.Add(at + 3); }
                else { sideList.Add(at); sideList.Add(at + 2); sideList.Add(at + 1); sideList.Add(at); sideList.Add(at + 3); sideList.Add(at + 2); }
            }
            return Made(name, parent, vertices, uvs, new List<List<int>> { topList, sideList }, new List<Material> { topMaterial, sideMaterial });
        }

        private static Vector2 ToRectangle(Vector2 centre, Vector2 way, Vector4 r)
        {
            float best = float.MaxValue;
            if (Mathf.Abs(way.x) > 0.0001f) { float t = ((way.x > 0f ? r.y : r.x) - centre.x) / way.x; if (t > 0f) best = Mathf.Min(best, t); }
            if (Mathf.Abs(way.y) > 0.0001f) { float t = ((way.y > 0f ? r.w : r.z) - centre.y) / way.y; if (t > 0f) best = Mathf.Min(best, t); }
            return centre + way * best;
        }

        /// <summary>An oval outline, the long way east-west.</summary>
        public static Vector2[] Oval(Vector2 centre, float halfWide, float halfDeep, int points)
        {
            var ring = new Vector2[points];
            for (int i = 0; i < points; i++)
            {
                float angle = i * Mathf.PI * 2f / points;
                ring[i] = centre + new Vector2(Mathf.Cos(angle) * halfWide, Mathf.Sin(angle) * halfDeep);
            }
            return ring;
        }

        /// <summary>A rectangle as an outline, for a plateau that is meant to be square (a built thing).</summary>
        public static Vector2[] Rect(float west, float east, float south, float north) =>
            new[] { new Vector2(west, south), new Vector2(west, north), new Vector2(east, north), new Vector2(east, south) };

        private static GameObject Made(string name, Transform parent, List<Vector3> vertices, List<Vector2> uvs, List<List<int>> lists, List<Material> materials)
        {
            var mesh = new Mesh { name = name };
            if (vertices.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = lists.Count;
            for (int i = 0; i < lists.Count; i++) mesh.SetTriangles(lists[i], i);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var made = new GameObject(name);
            made.transform.SetParent(parent, false);
            made.AddComponent<MeshFilter>().sharedMesh = mesh;
            var shown = made.AddComponent<MeshRenderer>();
            shown.sharedMaterials = materials.ToArray();
            made.AddComponent<MeshCollider>().sharedMesh = mesh;
            made.isStatic = true;
            return made;
        }

        // The outline with its long edges cut into short ones and the cuts pushed in and out a little. Corners stay put.
        private static List<Vector2> Broken(Vector2[] outline, float jitter, float segment)
        {
            var ring = new List<Vector2>();
            bool counter = SignedArea(new List<Vector2>(outline)) > 0f;
            for (int i = 0; i < outline.Length; i++)
            {
                Vector2 p = outline[i], q = outline[(i + 1) % outline.Length];
                ring.Add(p);
                float length = (q - p).magnitude;
                int cuts = jitter > 0f ? Mathf.FloorToInt(length / segment) : 0;
                Vector2 normal = EdgeOut(p, q, counter);
                for (int k = 1; k < cuts; k++)
                {
                    float t = (float)k / cuts;
                    // Less wander near the corners, so a corner does not fold over itself.
                    float room = Mathf.Min(1f, Mathf.Min(t, 1f - t) * length / (jitter * 3f + 0.01f));
                    ring.Add(Vector2.Lerp(p, q, t) + normal * Roll(jitter) * room);
                }
            }
            return ring;
        }

        private static float SignedArea(List<Vector2> ring)
        {
            float area = 0f;
            for (int i = 0; i < ring.Count; i++)
            {
                Vector2 p = ring[i], q = ring[(i + 1) % ring.Count];
                area += p.x * q.y - q.x * p.y;
            }
            return area * 0.5f;
        }

        private static Vector2 EdgeOut(Vector2 p, Vector2 q, bool counter)
        {
            Vector2 edge = (q - p).normalized;
            // For a ring going counter-clockwise (in x, z) the outside is to the right of each edge.
            return counter ? new Vector2(edge.y, -edge.x) : new Vector2(-edge.y, edge.x);
        }

        private static Vector2[] Outwards(List<Vector2> ring, bool counter)
        {
            var result = new Vector2[ring.Count];
            for (int i = 0; i < ring.Count; i++)
            {
                Vector2 before = ring[(i - 1 + ring.Count) % ring.Count], here = ring[i], after = ring[(i + 1) % ring.Count];
                Vector2 n = (EdgeOut(before, here, counter) + EdgeOut(here, after, counter)).normalized;
                result[i] = n;
            }
            return result;
        }

        // Ear clipping: a simple polygon (no holes) cut into triangles.
        private static List<Vector3Int> Ears(List<Vector2> ring)
        {
            var result = new List<Vector3Int>();
            var left = new List<int>();
            for (int i = 0; i < ring.Count; i++) left.Add(i);
            bool counter = SignedArea(ring) > 0f;
            int guard = ring.Count * ring.Count + 10;
            while (left.Count > 3 && guard-- > 0)
            {
                bool cut = false;
                for (int i = 0; i < left.Count; i++)
                {
                    int a = left[(i - 1 + left.Count) % left.Count], b = left[i], c = left[(i + 1) % left.Count];
                    Vector2 pa = ring[a], pb = ring[b], pc = ring[c];
                    float cross = (pb.x - pa.x) * (pc.y - pa.y) - (pb.y - pa.y) * (pc.x - pa.x);
                    if (counter ? cross <= 0f : cross >= 0f) continue;
                    bool empty = true;
                    for (int k = 0; k < left.Count && empty; k++)
                    {
                        int o = left[k];
                        if (o == a || o == b || o == c) continue;
                        if (InTriangle(ring[o], pa, pb, pc)) empty = false;
                    }
                    if (!empty) continue;
                    result.Add(new Vector3Int(a, b, c));
                    left.RemoveAt(i);
                    cut = true;
                    break;
                }
                // A ring that has folded over itself: take what is left as a fan rather than stop with a hole.
                if (!cut)
                {
                    for (int i = 1; i + 1 < left.Count; i++) result.Add(new Vector3Int(left[0], left[i], left[i + 1]));
                    return result;
                }
            }
            if (left.Count == 3) result.Add(new Vector3Int(left[0], left[1], left[2]));
            return result;
        }

        private static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
            float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
            float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
            bool negative = d1 < 0f || d2 < 0f || d3 < 0f, positive = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(negative && positive);
        }
    }
}
