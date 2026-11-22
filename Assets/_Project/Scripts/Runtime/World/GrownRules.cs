using UnityEngine;

namespace Clover.World
{
    /// <summary>The plain rules of ground that keeps acting on what stands in it (the ledger's ground-states, pull-to-point, mending-ground).</summary>
    public static class GroundRules
    {
        /// <summary>
        /// The shove each bite gives toward the middle for a pull of <paramref name="metresPerSecond"/>: a shove is spent over a
        /// moment and moves about two thirds of its length, so half as much again of the seconds between bites.
        /// </summary>
        public static float PullStep(float metresPerSecond, float biteEvery) => metresPerSecond * Mathf.Max(0.05f, biteEvery) * 1.5f;

        /// <summary>The health a bite of mending ground gives a player standing in it, for <paramref name="perSecond"/>.</summary>
        public static float MendStep(float perSecond, float biteEvery) => perSecond * Mathf.Max(0.05f, biteEvery);

        /// <summary>
        /// The seconds a state set by ground lasts on whoever stands in it: a little past the next bite, so it holds while they
        /// stay and goes soon after they step out.
        /// </summary>
        public static float StateSeconds(float biteEvery) => Mathf.Max(0.05f, biteEvery) + 0.25f;

        /// <summary>
        /// True when <paramref name="point"/> stands on a lane <paramref name="length"/> long and <paramref name="half"/> either side
        /// of its line, running from <paramref name="from"/> along <paramref name="way"/> (flat; the ends rounded by its width).
        /// </summary>
        public static bool OnLane(Vector3 point, Vector3 from, Vector3 way, float length, float half)
        {
            way.y = 0f;
            if (way.sqrMagnitude < 0.0001f) return Within(point, from, half);
            way.Normalize();
            Vector3 gap = point - from;
            gap.y = 0f;
            float along = Mathf.Clamp(Vector3.Dot(gap, way), 0f, Mathf.Max(0f, length));
            return (gap - way * along).magnitude <= half;
        }

        /// <summary>True when a point within the ground's radius of its middle stands in it.</summary>
        public static bool Within(Vector3 at, Vector3 middle, float radius)
        {
            Vector3 gap = at - middle;
            gap.y = 0f;
            return gap.magnitude <= radius;
        }
    }

    /// <summary>The plain rules of the player's grown things (the ledger's grown-bursts and grows-back), kept apart from the scene.</summary>
    public static class GrownRules
    {
        /// <summary>True when a grown thing at <paramref name="at"/> stands within <paramref name="reach"/> of <paramref name="from"/>; a reach of 0 reaches everything.</summary>
        public static bool Within(Vector3 at, Vector3 from, float reach)
        {
            if (reach <= 0f) return true;
            Vector3 offset = at - from;
            offset.y = 0f;
            return offset.sqrMagnitude <= reach * reach;
        }
    }

    /// <summary>
    /// Grown things waiting to grow back where they stood (Wood Grows Back), each with the time it comes up. Fixed in size:
    /// when it is full, the one due soonest is given up for the new one.
    /// </summary>
    public sealed class RegrowQueue
    {
        public const int Most = 16;
        private readonly HazardSpec[] specs = new HazardSpec[Most];
        private readonly float[] dueAt = new float[Most];
        private readonly bool[] waiting = new bool[Most];

        /// <summary>How many wait to grow back.</summary>
        public int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Most; i++)
                    if (waiting[i]) count++;
                return count;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Most; i++) waiting[i] = false;
        }

        /// <summary>Puts a grown thing in line to come up again at <paramref name="at"/>.</summary>
        public void Add(in HazardSpec spec, float at)
        {
            int slot = -1;
            for (int i = 0; i < Most && slot < 0; i++)
                if (!waiting[i]) slot = i;
            if (slot < 0)
            {
                slot = 0;
                for (int i = 1; i < Most; i++)
                    if (dueAt[i] < dueAt[slot]) slot = i;
            }
            specs[slot] = spec;
            dueAt[slot] = at;
            waiting[slot] = true;
        }

        /// <summary>Takes one whose time has come by <paramref name="now"/>; false when none has.</summary>
        public bool TakeDue(float now, out HazardSpec spec)
        {
            for (int i = 0; i < Most; i++)
            {
                if (!waiting[i] || dueAt[i] > now) continue;
                waiting[i] = false;
                spec = specs[i];
                return true;
            }
            spec = default;
            return false;
        }
    }
}
