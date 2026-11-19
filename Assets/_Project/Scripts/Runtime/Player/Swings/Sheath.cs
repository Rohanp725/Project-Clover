using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// The element along the edge while the blade holds a charge, laid out as a strip that follows the blade line point
    /// by point and is turned to the camera, its outer side toward the cutting edge. Ice's strip is cut into frost teeth
    /// that run along the edge; Metal's is only the short window of a glint. Plain maths into the caller's arrays.
    /// </summary>
    public static class Sheath
    {
        /// <summary>Pieces along a plain sheath, and the most teeth along an icy one.</summary>
        public const int Pieces = 24;
        public const int MostTeeth = 36;
        /// <summary>The most vertices a sheath can need.</summary>
        public const int MostVertices = (MostTeeth * 2 + 2) * 2;

        /// <summary>
        /// Writes the strip along <paramref name="points"/> (world space, base to tip). <paramref name="strength"/> is how
        /// strongly it shows (dim between swings, whole in one); <paramref name="glintAt"/> is where along the edge a glint
        /// is now (0 to 1), or below 0 for none. Returns the vertices written.
        /// </summary>
        public static int Build(Vector3[] points, int count, Vector3 edgeSide, Vector3 view, in SheathStyle style, float width, float strength, float now, float glintAt,
            Vector3[] vertices, Vector2[] uvs, Color[] colours, out Bounds bounds)
        {
            bounds = default;
            if (count < 2 || !style.Shown || strength <= 0.001f) return 0;
            float length = BladeFit.Length(points, count);
            if (length < 0.01f) return 0;
            float beat = Mathf.Floor(now * Mathf.Max(1f, style.Rebolt > 0f ? style.Rebolt : 18f));
            float alpha = strength * (1f - style.Flicker * RibbonMath.Hash((int)beat));
            Color colour = new Color(1f, 1f, 1f, alpha);
            if (style.Style.Shape > 6.5f) colour.r = 0.55f + 0.45f * RibbonMath.Hash((int)beat * 5 + 1);

            int written = 0;
            Vector3 low = Vector3.zero, high = Vector3.zero;
            if (style.Teeth)
            {
                // Teeth set along the edge in metres, all sliding toward the point: each rises slowly and drops sharply at its back.
                float spacing = Mathf.Max(length / MostTeeth, style.ToothEvery);
                float slide = (now * style.ToothRun) % spacing;
                int teeth = Mathf.Min(MostTeeth, Mathf.CeilToInt(length / spacing) + 1);
                for (int j = 0; j <= teeth && written + 4 <= vertices.Length; j++)
                {
                    float start = (j - 1) * spacing + slide;
                    Put(points, count, length, Mathf.Clamp(start, 0f, length), 0.25f, edgeSide, view, style, width, colour, vertices, uvs, colours, ref written, ref low, ref high);
                    Put(points, count, length, Mathf.Clamp(start + spacing * 0.9f, 0f, length), 1f, edgeSide, view, style, width, colour, vertices, uvs, colours, ref written, ref low, ref high);
                }
            }
            else
            {
                float from = 0f, to = 1f;
                if (style.Glint)
                {
                    if (glintAt < 0f) return 0;
                    from = Mathf.Clamp01(glintAt - style.GlintSpan * 0.5f);
                    to = Mathf.Clamp01(glintAt + style.GlintSpan * 0.5f);
                    if (to - from < 0.01f) return 0;
                }
                for (int k = 0; k <= Pieces && written + 2 <= vertices.Length; k++)
                {
                    float share = Mathf.Lerp(from, to, k / (float)Pieces);
                    Put(points, count, length, share * length, 1f, edgeSide, view, style, width, colour, vertices, uvs, colours, ref written, ref low, ref high, (share - from) / Mathf.Max(1e-4f, to - from));
                }
            }
            if (written < 4) return 0;
            bounds = new Bounds((low + high) * 0.5f, high - low + Vector3.one * 0.05f);
            return written;
        }

        // One pair across the edge at a distance along it: the inner side behind the edge, the outer reaching out by the share given.
        private static void Put(Vector3[] points, int count, float length, float metres, float reach, Vector3 edgeSide, Vector3 view, in SheathStyle style, float width, Color colour,
            Vector3[] vertices, Vector2[] uvs, Color[] colours, ref int written, ref Vector3 low, ref Vector3 high, float u = -1f)
        {
            float share = metres / length;
            Vector3 at = BladeFit.Along(points, count, share);
            Vector3 along = BladeFit.Tangent(points, count, share);
            Vector3 across = Vector3.Cross(along, view);
            across = across.sqrMagnitude > 1e-8f ? across.normalized : edgeSide;
            if (Vector3.Dot(across, edgeSide) < 0f) across = -across;
            Vector3 a = at - across * (width * style.Inside);
            Vector3 b = at + across * (width * (1f - style.Inside) * reach);
            var look = style.Style;
            float along01 = u >= 0f ? u : share;
            float uAlong = Mathf.Lerp(look.U0, look.U1, along01);
            vertices[written] = a;
            vertices[written + 1] = b;
            uvs[written] = look.Swap ? new Vector2(look.V0, uAlong) : new Vector2(uAlong, look.V0);
            uvs[written + 1] = look.Swap ? new Vector2(look.V1, uAlong) : new Vector2(uAlong, look.V1);
            colours[written] = colour;
            colours[written + 1] = colour;
            if (written == 0)
            {
                low = Vector3.Min(a, b);
                high = Vector3.Max(a, b);
            }
            else
            {
                low = Vector3.Min(low, Vector3.Min(a, b));
                high = Vector3.Max(high, Vector3.Max(a, b));
            }
            written += 2;
        }

    }
}
