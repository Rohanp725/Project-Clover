using UnityEngine;

namespace Clover.UI
{
    /// <summary>
    /// The path of the amber light a payment of Memories sends from the fallen into the player (way_build_plan.md question
    /// 10): it rises off the body first, hangs for a moment, then is drawn in fast to the chest, in 0.8 s, as the Shards'
    /// embers fly. Plain, so the path is tested without a scene; <see cref="MemoryGain"/> moves the light along it.
    /// </summary>
    public static class MemoryFlight
    {
        /// <summary>Seconds from the body to the chest.</summary>
        public const float Seconds = 0.8f;
        /// <summary>How high above the body the light's path bends before it turns in.</summary>
        public const float Rise = 2.2f;
        /// <summary>How high on the player the light is taken in: the chest.</summary>
        public const float ChestHeight = 1.2f;

        /// <summary>Where the light is at a share (0 to 1) of its flight from <paramref name="from"/> to <paramref name="chest"/>.</summary>
        public static Vector3 At(Vector3 from, Vector3 chest, float share)
        {
            float t = Mathf.Clamp01(share);
            // Slow off the body and fast into the chest: it rises and hangs, then is drawn in.
            float eased = t * t * (3f - 2f * t);
            eased *= eased;
            Vector3 bend = from + Vector3.up * Rise;
            Vector3 a = Vector3.Lerp(from, bend, eased);
            Vector3 b = Vector3.Lerp(bend, chest, eased);
            return Vector3.Lerp(a, b, eased);
        }

        /// <summary>
        /// The share of the flight done after <paramref name="age"/> seconds, when it waits <paramref name="delay"/> first
        /// (a god's light waits for the fall's own moment to pass): below zero while waiting, one or more once landed.
        /// </summary>
        public static float Share(float age, float delay) => (age - Mathf.Max(0f, delay)) / Seconds;
    }
}
