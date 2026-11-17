using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// What the thirsting will drink (fire_rework.md, section 1): a broken water jar's spill, the body of anyone fallen,
    /// for a while after. A fixed list of spots, each drunk once, each drying up after its own seconds.
    /// </summary>
    public static class Waters
    {
        public const int Most = 24;
        private static readonly Vector3[] at = new Vector3[Most];
        private static readonly float[] until = new float[Most];
        private static readonly object[] owner = new object[Most];

        // Domain reload is off: the last play's spills are dry.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            for (int i = 0; i < Most; i++)
            {
                until[i] = 0f;
                owner[i] = null;
            }
        }

        /// <summary>Water to drink at a point for this many seconds. One spot per owner: a second call from the same owner moves it.</summary>
        public static void Add(Vector3 point, float seconds, object from)
        {
            int free = -1;
            for (int i = 0; i < Most; i++)
            {
                if (from != null && ReferenceEquals(owner[i], from)) { free = i; break; }
                if (free < 0 && until[i] <= Time.time) free = i;
            }
            if (free < 0) return;
            at[free] = point;
            until[free] = Time.time + seconds;
            owner[free] = from;
        }

        /// <summary>The nearest water within reach of a point, if any.</summary>
        public static bool Nearest(Vector3 point, float reach, out Vector3 found, out int index)
        {
            found = point;
            index = -1;
            float best = reach * reach;
            for (int i = 0; i < Most; i++)
            {
                if (until[i] <= Time.time) continue;
                float x = at[i].x - point.x, z = at[i].z - point.z;
                float d = x * x + z * z;
                if (d > best) continue;
                best = d;
                found = at[i];
                index = i;
            }
            return index >= 0;
        }

        /// <summary>Drinks a spot dry. False if it was already gone.</summary>
        public static bool Drink(int index)
        {
            if (index < 0 || index >= Most || until[index] <= Time.time) return false;
            until[index] = 0f;
            owner[index] = null;
            return true;
        }

        public static bool Wet(int index) => index >= 0 && index < Most && until[index] > Time.time;

        public static int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Most; i++)
                    if (until[i] > Time.time) count++;
                return count;
            }
        }
    }
}
