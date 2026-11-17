using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The lines the storm dragon's tail lies along in its moves, each given by how many metres along it a point is
    /// from where the tail comes over the rim. A spine is laid on the line joint by joint, so the tail bends as the
    /// line does. Plain numbers and vectors, so tests can run them.
    /// </summary>
    public static class TailShapes
    {
        /// <summary>A flat direction turned clockwise (seen from above) by so many radians.</summary>
        public static Vector3 Turned(Vector3 direction, float radians)
        {
            float sin = Mathf.Sin(radians), cos = Mathf.Cos(radians);
            return new Vector3(direction.x * cos + direction.z * sin, 0f, -direction.x * sin + direction.z * cos);
        }

        /// <summary>How far the coil's line runs straight from the rim before it meets its ring.</summary>
        public static float CoilLine(Vector3 entry, Vector3 centre, float radius)
        {
            Vector3 from = entry - centre;
            from.y = 0f;
            float away = Mathf.Max(from.magnitude, radius + 0.01f);
            return Mathf.Sqrt(away * away - radius * radius);
        }

        /// <summary>
        /// The coil: straight from the rim to the ring round a middle, touching it, and then round it, one way or
        /// the other, drawing a little inward with every turn so the tail never lies on itself.
        /// </summary>
        public static Vector3 Coil(Vector3 entry, Vector3 centre, float radius, float sense, float metres, float inwardATurn = 1.6f)
        {
            Vector3 from = entry - centre;
            from.y = 0f;
            float away = Mathf.Max(from.magnitude, radius + 0.01f);
            Vector3 outward = from.sqrMagnitude > 0.0001f ? from.normalized : Vector3.forward;
            float line = Mathf.Sqrt(away * away - radius * radius);
            float touchAt = Mathf.Acos(Mathf.Clamp(radius / away, -1f, 1f));
            Vector3 flatCentre = new Vector3(centre.x, 0f, centre.z);
            if (metres <= line)
            {
                Vector3 touch = flatCentre + Turned(outward, sense * touchAt) * radius;
                Vector3 start = new Vector3(entry.x, 0f, entry.z);
                return Vector3.Lerp(start, touch, line > 0.001f ? metres / line : 1f);
            }
            float round = (metres - line) / radius;
            float drawn = Mathf.Max(radius * 0.4f, radius - inwardATurn * round / (Mathf.PI * 2f));
            return flatCentre + Turned(outward, sense * (touchAt + round)) * drawn;
        }

        /// <summary>
        /// The scythe's turn at a moment: it waits at one side, crosses to the other, waits, and comes back faster.
        /// Before it begins and after it ends it lies where it began.
        /// </summary>
        public static float Sweep(float seconds, float from, float to, float first, float pause, float second)
        {
            if (seconds <= 0f) return from;
            if (seconds < first) return Mathf.Lerp(from, to, Ease(seconds / first));
            seconds -= first;
            if (seconds < pause) return to;
            seconds -= pause;
            if (seconds < second) return Mathf.Lerp(to, from, Ease(seconds / second));
            return from;
        }

        /// <summary>
        /// The sting: up from the rim in a high arch and down to where the last joint is held, as a scorpion's tail
        /// stands over what it will stab. <paramref name="tipHeight"/> is how high that last joint is.
        /// </summary>
        public static Vector3 Sting(Vector3 entry, Vector3 joint, float arch, float tipHeight, float metres)
        {
            Vector3 flat = joint - entry;
            flat.y = 0f;
            float length = Mathf.Max(0.5f, flat.magnitude);
            float share = Mathf.Clamp01(metres / length);
            Vector3 at = new Vector3(entry.x, 0f, entry.z) + flat * share;
            at.y = arch * Mathf.Sin(Mathf.PI * Mathf.Pow(share, 0.75f)) + tipHeight * share * share;
            return at;
        }

        private static float Ease(float t) => t * t * (3f - 2f * t);
    }
}
