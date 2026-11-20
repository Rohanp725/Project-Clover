using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// The small sums under a swing trail, kept apart so the tests can hold them to their promises: a fade that reaches
    /// nothing smoothly (a trail is never cut off), a tip speed that fades a trail in, and the in-between points that
    /// bend round the swinger rather than cutting the corner when a fast swing moves far between two frames.
    /// </summary>
    public static class RibbonMath
    {
        /// <summary>
        /// How much of a part of a trail is left at <paramref name="t"/> of its life (0 new, 1 gone): whole for the
        /// share <paramref name="hold"/>, then down to nothing along a smooth curve whose slope is zero at the end, so the
        /// tail thins away and is never cut.
        /// </summary>
        public static float Fade(float t, float hold = 0f)
        {
            if (t <= hold) return 1f;
            if (t >= 1f) return 0f;
            float s = (t - hold) / Mathf.Max(1e-4f, 1f - hold);
            return 1f - s * s * (3f - 2f * s);
        }

        /// <summary>How whole a trail is drawn for a tip moving at <paramref name="speed"/>: nothing below the least, whole from the full, smooth between.</summary>
        public static float Gate(float speed, float least, float full)
        {
            if (full <= least) return speed >= least ? 1f : 0f;
            float s = Mathf.Clamp01((speed - least) / (full - least));
            return s * s * (3f - 2f * s);
        }

        /// <summary>How many steps to take between two offsets from the swinger so no step turns more than <paramref name="degreesPerStep"/>.</summary>
        public static int Steps(Vector3 from, Vector3 to, float degreesPerStep, int most)
        {
            if (from.sqrMagnitude < 1e-8f || to.sqrMagnitude < 1e-8f) return 1;
            float angle = Vector3.Angle(from, to);
            return Mathf.Clamp(Mathf.CeilToInt(angle / Mathf.Max(0.5f, degreesPerStep)), 1, Mathf.Max(1, most));
        }

        /// <summary>
        /// A point between two samples of a swing, each given as a pivot (the swinger's chest) and an offset from it:
        /// the pivot moves straight and the offset turns, so the in-between lies on the arc the blade swept.
        /// </summary>
        public static Vector3 Between(Vector3 pivotFrom, Vector3 offsetFrom, Vector3 pivotTo, Vector3 offsetTo, float s) =>
            Vector3.Lerp(pivotFrom, pivotTo, s) + Vector3.Slerp(offsetFrom, offsetTo, s);

        /// <summary>Where along the shape's window a part of the trail lies: the head (new) at <paramref name="head"/>, the tail (gone) at <paramref name="tail"/>.</summary>
        public static float Along(float t, float tail, float head) => Mathf.Lerp(head, tail, Mathf.Clamp01(t));

        /// <summary>A number from 0 to 1 that is always the same for the same whole number.</summary>
        public static float Hash(int n)
        {
            float s = Mathf.Sin(n * 12.9898f + 4.1414f) * 43758.547f;
            return s - Mathf.Floor(s);
        }

        /// <summary>
        /// How tall a tongue of flame stands at a part of a trail, 0 to 1: a smooth rise and fall over every
        /// <paramref name="every"/> parts, each tongue its own height, so a burning edge licks up unevenly.
        /// </summary>
        public static float Tongue(int part, float every)
        {
            float x = part / Mathf.Max(1f, every);
            int n = Mathf.FloorToInt(x);
            float f = x - n;
            float height = Mathf.Lerp(0.35f, 1f, Hash(n * 7 + 3));
            // Round at its root and pointed at its top: the tongue is a hump with a sharp peak.
            float hump = Mathf.Sin(f * Mathf.PI);
            return height * hump * hump;
        }

        /// <summary>The sawtooth of the frost teeth: rising along each tooth from 0 to 1 and dropping at its back.</summary>
        public static float Tooth(float along) => along - Mathf.Floor(along);
    }
}
