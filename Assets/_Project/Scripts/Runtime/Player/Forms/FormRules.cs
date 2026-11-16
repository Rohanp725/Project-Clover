using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>The plain geometry spell forms share: where the n-th of a count of things goes, and whether a wall stands between two points.</summary>
    public static class FormRules
    {
        /// <summary>Forms that must show themselves forming before they strike never wait less than this, whatever the spell's own delay.</summary>
        public const float LeastWarning = 0.22f;

        /// <summary>The delay a form waits before it strikes: the spell's own, but never so short that nothing is seen coming.</summary>
        public static float Warning(float delay) => Mathf.Max(LeastWarning, delay);

        /// <summary>
        /// Where the n-th of <paramref name="count"/> things stands in a ring of a radius: evenly round, starting
        /// straight ahead (+Z) and going clockwise seen from above, turned by <paramref name="turnDegrees"/>.
        /// </summary>
        public static Vector3 OnRing(int index, int count, float radius, float turnDegrees = 0f)
        {
            float angle = (turnDegrees + 360f * index / Mathf.Max(1, count)) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
        }

        /// <summary>
        /// Where the n-th of <paramref name="count"/> things lies when they are scattered evenly over a disc: a
        /// sunflower's seeds. The first is at the middle; none is outside the radius.
        /// </summary>
        public static Vector3 Scattered(int index, int count, float radius)
        {
            if (count <= 1 || index <= 0) return Vector3.zero;
            float distance = radius * Mathf.Sqrt(index / (float)(count - 1));
            float angle = index * 137.50776f * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(angle) * distance, 0f, Mathf.Cos(angle) * distance);
        }

        /// <summary>
        /// How far to the side the n-th of <paramref name="count"/> things in a row stands, the row centred: for two
        /// things a spacing apart, -half and +half.
        /// </summary>
        public static float InRow(int index, int count, float spacing) => (index - (count - 1) * 0.5f) * spacing;

        /// <summary>
        /// The angle, in degrees from straight behind, at which the n-th of <paramref name="count"/> things in a fan
        /// of a given full width stands. One thing stands in the middle.
        /// </summary>
        public static float InFan(int index, int count, float fullDegrees) =>
            count <= 1 ? 0f : Mathf.Lerp(-fullDegrees * 0.5f, fullDegrees * 0.5f, index / (float)(count - 1));

        /// <summary>True when the line on the ground from one point to another crosses the line between two others.</summary>
        public static bool Crosses(Vector3 from, Vector3 to, Vector3 wallA, Vector3 wallB)
        {
            float d1 = Side(wallA, wallB, from), d2 = Side(wallA, wallB, to);
            float d3 = Side(from, to, wallA), d4 = Side(from, to, wallB);
            return d1 * d2 < 0f && d3 * d4 < 0f;
        }

        // Which side of the line from a to b the point lies, on the ground: its sign is the side.
        private static float Side(Vector3 a, Vector3 b, Vector3 point) => (b.x - a.x) * (point.z - a.z) - (b.z - a.z) * (point.x - a.x);

        /// <summary>How many slabs, posts or spikes cover a length when each covers <paramref name="each"/> of it: at least one.</summary>
        public static int Pieces(float length, float each) => Mathf.Max(1, Mathf.CeilToInt(length / Mathf.Max(0.05f, each) - 0.001f));

        /// <summary>The share of a clone's echo: a spell's damage number read as a percentage of the blow it repeats.</summary>
        public static float Echo(float blowDamage, float percent) => Mathf.Max(0f, blowDamage) * Mathf.Clamp(percent, 0f, 200f) / 100f;
    }
}
