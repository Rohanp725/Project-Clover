using UnityEngine;

namespace Clover.World
{
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
