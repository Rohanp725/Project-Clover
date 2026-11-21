namespace Clover.Gameplay
{
    /// <summary>
    /// Counts and clocks the player's marks keep on one enemy (the ledger's counts and clocks, standin_clearance_plan.md
    /// pass 1): a mark's wait that runs on each enemy apart, so an elite's flinch waits 4 s on each elite and not on all
    /// of them; and a count of how often a mark has come to this enemy, so "every fifth bite" is the fifth on it, not one
    /// in five by chance. Fixed in size and made with the enemy (IMPLEMENTATION 8.1 budgets the reaction rest at about
    /// 220 bytes an enemy), so nothing is allocated in a fight. Keys are the marks' effect slots.
    /// </summary>
    public sealed class EnemyTally
    {
        /// <summary>How many marks' clocks and counts one enemy holds at once; past that the stalest place is reused.</summary>
        public const int Places = 8;

        private readonly int[] clockKey = new int[Places];
        private readonly float[] clockReady = new float[Places];
        private readonly int[] countKey = new int[Places];
        private readonly int[] counts = new int[Places];
        private readonly int[] countAge = new int[Places];
        private int countStamp;

        public EnemyTally() => Clear();

        /// <summary>Forgets every clock and count: a new fight with this enemy.</summary>
        public void Clear()
        {
            for (int i = 0; i < Places; i++)
            {
                clockKey[i] = -1;
                clockReady[i] = 0f;
                countKey[i] = -1;
                counts[i] = 0;
                countAge[i] = 0;
            }
            countStamp = 0;
        }

        /// <summary>True when the mark's clock on this enemy has run out, or was never started.</summary>
        public bool Ready(int key, float now)
        {
            int place = Find(clockKey, key);
            return place < 0 || now >= clockReady[place];
        }

        /// <summary>Seconds left on the mark's clock on this enemy; zero when ready.</summary>
        public float Left(int key, float now)
        {
            int place = Find(clockKey, key);
            return place < 0 ? 0f : System.Math.Max(0f, clockReady[place] - now);
        }

        /// <summary>Starts the mark's clock on this enemy: it is not ready again for <paramref name="seconds"/>.</summary>
        public void Start(int key, float now, float seconds)
        {
            int place = Find(clockKey, key);
            if (place < 0)
            {
                // A free place, or one whose clock has run out; failing both, the one that runs out soonest, so a
                // ninth mark at worst cuts short the wait nearest its end.
                place = 0;
                for (int i = 0; i < Places; i++)
                {
                    if (clockKey[i] < 0 || clockReady[i] <= now) { place = i; break; }
                    if (clockReady[i] < clockReady[place]) place = i;
                }
                clockKey[place] = key;
            }
            clockReady[place] = now + seconds;
        }

        /// <summary>How many times the mark has been counted on this enemy.</summary>
        public int Count(int key)
        {
            int place = Find(countKey, key);
            return place < 0 ? 0 : counts[place];
        }

        /// <summary>Counts the mark once more on this enemy (or <paramref name="by"/> more) and gives the new count.</summary>
        public int Add(int key, int by = 1)
        {
            int place = Find(countKey, key);
            if (place < 0)
            {
                // A free place, else the count touched longest ago.
                place = 0;
                for (int i = 0; i < Places; i++)
                {
                    if (countKey[i] < 0) { place = i; break; }
                    if (countAge[i] < countAge[place]) place = i;
                }
                countKey[place] = key;
                counts[place] = 0;
            }
            counts[place] += by;
            countAge[place] = ++countStamp;
            return counts[place];
        }

        /// <summary>
        /// Counts the mark once more and says whether this is one of every <paramref name="every"/>: true on the
        /// every-th, twice every and so on. One or less is every time.
        /// </summary>
        public bool AddAndCome(int key, int every)
        {
            int count = Add(key);
            return every <= 1 || count % every == 0;
        }

        /// <summary>Empties the mark's count on this enemy.</summary>
        public void Reset(int key)
        {
            int place = Find(countKey, key);
            if (place >= 0) counts[place] = 0;
        }

        private static int Find(int[] keys, int key)
        {
            if (key < 0) return -1;
            for (int i = 0; i < Places; i++)
                if (keys[i] == key) return i;
            return -1;
        }
    }
}
