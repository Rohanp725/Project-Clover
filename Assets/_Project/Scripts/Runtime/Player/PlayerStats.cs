using System;

namespace Clover.Player
{
    /// <summary>
    /// The sum of everything that changes the player's numbers: what has been learned, what the surge adds while it
    /// lasts, and boons that run out. The weapon, the guard, the dodge, the flasks and the spells read their numbers
    /// through this. Plain rules with no scene in them.
    /// </summary>
    public class PlayerStats
    {
        public const int Count = (int)PlayerStat.SwingSpeed + 1;
        /// <summary>How many timed boons can run at once; a new one past that takes the place of the one nearest its end.</summary>
        public const int BoonCapacity = 16;
        /// <summary>No sum of shares takes a number below this share of itself.</summary>
        public const float LeastMultiplier = 0.1f;

        private struct Boon
        {
            public PlayerStat Stat;
            public float Amount;
            public float Left;
        }

        private readonly float[] learned = new float[Count];
        private readonly float[] surge = new float[Count];
        private readonly float[] boon = new float[Count];
        private readonly Boon[] boons = new Boon[BoonCapacity];

        /// <summary>Goes up whenever any sum changes, so a reader can tell that it must look again.</summary>
        public int Version { get; private set; }

        /// <summary>True for the stats that are plain amounts; the rest are shares.</summary>
        public static bool IsFlat(PlayerStat stat) =>
            stat == PlayerStat.AfflictionStacks || stat == PlayerStat.Health || stat == PlayerStat.FlaskCount || stat == PlayerStat.CritChance;

        public void Clear()
        {
            Array.Clear(learned, 0, Count);
            Array.Clear(surge, 0, Count);
            Array.Clear(boon, 0, Count);
            for (int i = 0; i < boons.Length; i++) boons[i].Left = 0f;
            Version++;
        }

        public void ClearLearned()
        {
            Array.Clear(learned, 0, Count);
            Version++;
        }

        public void AddLearned(StatMod mod)
        {
            learned[(int)mod.Stat] += mod.Amount;
            Version++;
        }

        public void ClearSurge()
        {
            Array.Clear(surge, 0, Count);
            Version++;
        }

        public void AddSurge(StatMod mod)
        {
            surge[(int)mod.Stat] += mod.Amount;
            Version++;
        }

        /// <summary>
        /// Starts a boon that lasts some seconds. The same boon given again (same stat, same amount) starts its time
        /// over and does not stack with itself.
        /// </summary>
        public void AddBoon(PlayerStat stat, float amount, float seconds)
        {
            if (seconds <= 0f || amount == 0f) return;
            int free = -1, soonest = 0;
            for (int i = 0; i < boons.Length; i++)
            {
                if (boons[i].Left > 0f && boons[i].Stat == stat && boons[i].Amount == amount)
                {
                    boons[i].Left = Math.Max(boons[i].Left, seconds);
                    return;
                }
                if (boons[i].Left <= 0f && free < 0) free = i;
                if (boons[i].Left < boons[soonest].Left) soonest = i;
            }
            int slot = free >= 0 ? free : soonest;
            if (boons[slot].Left > 0f) boon[(int)boons[slot].Stat] -= boons[slot].Amount;
            boons[slot] = new Boon { Stat = stat, Amount = amount, Left = seconds };
            boon[(int)stat] += amount;
            Version++;
        }

        /// <summary>How many boons are running now.</summary>
        public int BoonsRunning
        {
            get
            {
                int count = 0;
                for (int i = 0; i < boons.Length; i++)
                    if (boons[i].Left > 0f) count++;
                return count;
            }
        }

        /// <summary>Runs the boons down.</summary>
        public void Tick(float deltaTime)
        {
            for (int i = 0; i < boons.Length; i++)
            {
                if (boons[i].Left <= 0f) continue;
                boons[i].Left -= deltaTime;
                if (boons[i].Left > 0f) continue;
                boon[(int)boons[i].Stat] -= boons[i].Amount;
                Version++;
            }
        }

        /// <summary>Everything added together for one stat.</summary>
        public float Sum(PlayerStat stat) => learned[(int)stat] + surge[(int)stat] + boon[(int)stat];

        /// <summary>What a share stat multiplies its number by: one plus the sum, and never less than a tenth.</summary>
        public float Multiplier(PlayerStat stat) => Math.Max(LeastMultiplier, 1f + Sum(stat));

        /// <summary>A flat stat's sum as whole things.</summary>
        public int Whole(PlayerStat stat) => (int)Math.Floor(Sum(stat) + 0.0001f);
    }
}
