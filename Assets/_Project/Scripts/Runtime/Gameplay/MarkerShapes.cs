using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The flat shapes drawn on the ground to show where an attack will land. Each is a bold band along its edge with a
    /// much fainter inside (the vertex colours carry the difference), so the shape reads without painting over whoever
    /// stands in it. The same shape drawn a second time, brighter and growing, shows how near the blow is.
    /// </summary>
    public static class MarkerShapes
    {
        /// <summary>How wide, in metres, the bold band along a shape's edge is.</summary>
        public const float Rim = 0.09f;
        /// <summary>How much of the edge's strength the inside of a fan or disc keeps.</summary>
        public const float InsideOfFan = 0.1f;
        /// <summary>A lane is narrow and has to be seen along its whole length, so its inside keeps more.</summary>
        public const float InsideOfLane = 0.3f;
        /// <summary>How strongly the still outline is drawn, and the growing copy inside it.</summary>
        public const float OutlineAlpha = 0.8f;
        public const float GrowingAlpha = 1f;
        /// <summary>The growing copy is this much brighter than the outline it grows to meet.</summary>
        public const float GrowingGain = 1.45f;

        /// <summary>The colour of the growing copy of a mark: the mark's own colour, brighter.</summary>
        public static Color Growing(Color colour) =>
            new Color(colour.r * GrowingGain, colour.g * GrowingGain, colour.b * GrowingGain, colour.a);

        /// <summary>A flat lane lying on the ground, running from the origin along +Z.</summary>
        public static Mesh Lane(float length, float width, float inside = InsideOfLane)
        {
            float half = width * 0.5f;
            var edge = new[] { new Vector3(-half, 0f, 0f), new Vector3(-half, 0f, length), new Vector3(half, 0f, length), new Vector3(half, 0f, 0f) };
            return Banded(edge, new Vector3(0f, 0f, length * 0.5f), inside);
        }

        /// <summary>A flat fan lying on the ground, pointing along +Z and facing up. 360 degrees makes a disc.</summary>
        public static Mesh Sector(float radius, float arcDegrees, float inside = InsideOfFan)
        {
            const int Segments = 40;
            bool whole = arcDegrees >= 359.9f;
            // A disc's edge is the circle alone; a fan's also runs in to its point and out again.
            var edge = new Vector3[whole ? Segments : Segments + 2];
            int first = whole ? 0 : 1;
            if (!whole) edge[0] = Vector3.zero;
            for (int i = 0; i < (whole ? Segments : Segments + 1); i++)
            {
                float angle = Mathf.Deg2Rad * Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, i / (float)Segments);
                edge[first + i] = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
            }
            return Banded(edge, whole ? Vector3.zero : new Vector3(0f, 0f, radius * 0.35f), inside);
        }

        // A closed outline made into a mesh: a band of full strength just inside the edge, and a faint fan filling the rest
        // from a point that can see the whole of it.
        private static Mesh Banded(Vector3[] edge, Vector3 within, float inside)
        {
            int count = edge.Length;
            var vertices = new Vector3[count * 3 + 1];
            var colours = new Color[count * 3 + 1];
            var uvs = new Vector2[count * 3 + 1];
            var triangles = new int[count * 9];
            var bold = Color.white;
            var faint = new Color(1f, 1f, 1f, inside);
            for (int i = 0; i < count; i++)
            {
                Vector3 before = edge[(i + count - 1) % count], here = edge[i], after = edge[(i + 1) % count];
                Vector3 inwardA = Inward(before, here, within), inwardB = Inward(here, after, within);
                Vector3 mitre = (inwardA + inwardB).normalized;
                // At a sharp corner the band's inner corner lies further in; not without limit, or a thin fan's point becomes a spike.
                float stretch = Mathf.Min(1f / Mathf.Max(Vector3.Dot(mitre, inwardA), 0.3f), 3f);
                Vector3 inner = here + mitre * (Rim * stretch);
                vertices[i] = here;
                colours[i] = bold;
                // The band's inner edge twice over: once at full strength for the band, once faint for the inside.
                vertices[count + i] = inner;
                colours[count + i] = bold;
                vertices[count * 2 + i] = inner;
                colours[count * 2 + i] = faint;
            }
            int centre = count * 3;
            vertices[centre] = within;
            colours[centre] = faint;
            for (int i = 0; i < vertices.Length; i++) uvs[i] = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                int t = i * 9;
                triangles[t] = i; triangles[t + 1] = next; triangles[t + 2] = count + i;
                triangles[t + 3] = next; triangles[t + 4] = count + next; triangles[t + 5] = count + i;
                triangles[t + 6] = centre; triangles[t + 7] = count * 2 + i; triangles[t + 8] = count * 2 + next;
            }
            // The marks are drawn without culling, so which way round a triangle is wound does not matter.
            var mesh = new Mesh { name = "Attack Marker" };
            mesh.vertices = vertices;
            mesh.colors = colours;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        // The flat direction square to an edge that points to the shape's inside.
        private static Vector3 Inward(Vector3 from, Vector3 to, Vector3 within)
        {
            Vector3 along = (to - from).normalized;
            var square = new Vector3(-along.z, 0f, along.x);
            return Vector3.Dot(square, within - (from + to) * 0.5f) < 0f ? -square : square;
        }
    }
}
