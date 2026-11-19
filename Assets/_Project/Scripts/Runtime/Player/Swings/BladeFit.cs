using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// The measuring behind a blade line: the base and tip read off the vertices of a weapon's edge, a bend when the edge
    /// curves, and points found along a line by the share of its length. Plain maths, so the builder and the tests use
    /// the same thing the game does.
    /// </summary>
    public static class BladeFit
    {
        /// <summary>How far off its chord an edge must reach, as a share of the chord, before its bend is kept as a third point.</summary>
        public const float BendShare = 0.12f;

        /// <summary>
        /// An edge's line from a cloud of its vertices: the two that lie furthest apart are its ends, the one nearer
        /// <paramref name="grip"/> the base; and when some vertex stands off that chord by more than <see cref="BendShare"/>
        /// of it (a crescent), the one furthest off is put between them. Returns how many points were written (0, 2 or 3).
        /// </summary>
        public static int Fit(IReadOnlyList<Vector3> cloud, Vector3 grip, Vector3[] into)
        {
            if (cloud == null || cloud.Count < 2 || into == null || into.Length < 3) return 0;
            int first = 0, second = 1;
            float widest = -1f;
            for (int i = 0; i < cloud.Count; i++)
                for (int j = i + 1; j < cloud.Count; j++)
                {
                    float apart = (cloud[i] - cloud[j]).sqrMagnitude;
                    if (apart > widest)
                    {
                        widest = apart;
                        first = i;
                        second = j;
                    }
                }
            if (widest < 1e-6f) return 0;
            Vector3 one = cloud[first], other = cloud[second];
            bool oneNearer = (one - grip).sqrMagnitude <= (other - grip).sqrMagnitude;
            Vector3 from = oneNearer ? one : other, to = oneNearer ? other : one;
            float chord = Mathf.Sqrt(widest);
            int bend = -1;
            float furthest = chord * BendShare;
            for (int i = 0; i < cloud.Count; i++)
            {
                float off = OffLine(cloud[i], from, to);
                if (off > furthest)
                {
                    furthest = off;
                    bend = i;
                }
            }
            into[0] = from;
            if (bend < 0)
            {
                into[1] = to;
                return 2;
            }
            into[1] = cloud[bend];
            into[2] = to;
            return 3;
        }

        /// <summary>How far a point lies from the segment between two others.</summary>
        public static float OffLine(Vector3 point, Vector3 from, Vector3 to)
        {
            Vector3 along = to - from;
            float length = along.sqrMagnitude;
            if (length < 1e-8f) return (point - from).magnitude;
            float t = Mathf.Clamp01(Vector3.Dot(point - from, along) / length);
            return (point - (from + along * t)).magnitude;
        }

        /// <summary>The length along the first <paramref name="count"/> points.</summary>
        public static float Length(Vector3[] points, int count)
        {
            float total = 0f;
            for (int i = 1; i < count; i++) total += (points[i] - points[i - 1]).magnitude;
            return total;
        }

        /// <summary>
        /// The point at a share of the line's length from its base (0) to its tip (1). Below 0 and beyond 1 it carries on
        /// straight along the first or the last piece, so a trail can reach a little past the point or down the haft.
        /// </summary>
        public static Vector3 Along(Vector3[] points, int count, float share)
        {
            if (count <= 0) return Vector3.zero;
            if (count == 1) return points[0];
            float total = Length(points, count);
            if (total < 1e-6f) return points[0];
            float wanted = share * total;
            if (wanted <= 0f) return points[0] + (points[1] - points[0]).normalized * wanted;
            float walked = 0f;
            for (int i = 1; i < count; i++)
            {
                float piece = (points[i] - points[i - 1]).magnitude;
                if (walked + piece >= wanted || i == count - 1)
                {
                    float t = piece > 1e-6f ? (wanted - walked) / piece : 0f;
                    return Vector3.LerpUnclamped(points[i - 1], points[i], t);
                }
                walked += piece;
            }
            return points[count - 1];
        }

        /// <summary>The way the line runs at a share of its length, base toward tip.</summary>
        public static Vector3 Tangent(Vector3[] points, int count, float share)
        {
            if (count < 2) return Vector3.forward;
            float total = Length(points, count);
            float wanted = Mathf.Clamp01(share) * total, walked = 0f;
            for (int i = 1; i < count; i++)
            {
                Vector3 piece = points[i] - points[i - 1];
                float length = piece.magnitude;
                if (walked + length >= wanted || i == count - 1) return length > 1e-6f ? piece / length : Vector3.forward;
                walked += length;
            }
            return Vector3.forward;
        }

        /// <summary>
        /// A head's line: along the haft (+Z) through the middle of its bounds, from the end nearer the grip to the end
        /// further from it. A corner-to-corner pair would lie across the block, not along the swing's radius.
        /// </summary>
        public static int FitHead(Bounds bounds, Vector3[] into)
        {
            if (into == null || into.Length < 2 || bounds.size.z < 1e-4f) return 0;
            Vector3 centre = bounds.center;
            into[0] = new Vector3(centre.x, centre.y, bounds.min.z);
            into[1] = new Vector3(centre.x, centre.y, bounds.max.z);
            return 2;
        }
    }
}
