using System;
using Clover.Gameplay;

namespace Clover.Player
{
    /// <summary>
    /// The sum of everything that changes the player's numbers: what has been learned, what the surge adds while it
    /// lasts, and boons that run out. The weapon, the guard, the dodge, the flasks and the spells read their numbers
    /// through this. Plain rules with no scene in them.
    ///
    /// A learned change may be conditional. One that holds in a state of the player (guarding, low health) joins the
    /// stat's sum while <see cref="SetWhen"/> says the state holds. One that holds on a kind of blow, or against a
    /// target carrying a named affliction, is asked for as the blow lands (<see cref="Blow"/>, <see cref="Against"/>).
    /// </summary>
    public class PlayerStats
    {
        public const int Count = (int)PlayerStat.Unshaken + 1;
        public const int WhenCount = (int)StatWhen.StoodStill + 1;
        /// <summary>How many timed boons can run at once; a new one past that takes the place of the one nearest its end.</summary>
        public const int BoonCapacity = 16;
        /// <summary>How many learned changes can be tied to an affliction on the target; more are dropped.</summary>
        public const int AgainstCapacity = 48;
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
        // One row of stats for each condition; the rows whose state holds are summed into held.
        private readonly float[] conditional = new float[WhenCount * Count];
        private readonly float[] held = new float[Count];
        private readonly bool[] holds = new bool[WhenCount];
        private readonly StatMod[] against = new StatMod[AgainstCapacity];
        private int againstCount;
        // What the counts kept by marks add, a point at a time.
        private readonly float[] tally = new float[Count];

        /// <summary>Goes up whenever any sum changes, so a reader can tell that it must look again.</summary>
        public int Version { get; private set; }

        public static bool IsFlat(PlayerStat stat) =>
            stat == PlayerStat.AfflictionStacks || stat == PlayerStat.Health || stat == PlayerStat.FlaskCount || stat == PlayerStat.CritChance
            || stat == PlayerStat.AfflictionDepth || stat == PlayerStat.Leech || stat == PlayerStat.PlantedSooner || stat == PlayerStat.BloodPrice
            || stat == PlayerStat.GreyStrength || stat == PlayerStat.BaseSpellCount || stat == PlayerStat.HeatBlows || stat == PlayerStat.HeatCap
            || stat == PlayerStat.AirSeconds || stat == PlayerStat.AirBlows || stat == PlayerStat.GrownLimit || stat == PlayerStat.MarkBlows
            || stat == PlayerStat.StaminaMost || stat == PlayerStat.StaminaDelay || stat == PlayerStat.Poise || stat == PlayerStat.DodgeMetres
            || stat == PlayerStat.DodgeRecovery || stat == PlayerStat.DodgeFlow || stat == PlayerStat.FlinchSeconds || stat == PlayerStat.GreyDelay;

        /// <summary>True for a condition asked of each blow, not held as a state.</summary>
        public static bool IsBlow(StatWhen when) => when == StatWhen.HeavyBlow || when == StatWhen.LightBlow || when == StatWhen.CriticalBlow || when == StatWhen.ChainEndBlow || when == StatWhen.AirborneBlow || when == StatWhen.AirborneHeavyBlow;

        public void Clear()
        {
            Array.Clear(surge, 0, Count);
            Array.Clear(boon, 0, Count);
            for (int i = 0; i < boons.Length; i++) boons[i].Left = 0f;
            Array.Clear(holds, 0, WhenCount);
            Array.Clear(tally, 0, Count);
            ClearLearned();
        }

        public void ClearLearned()
        {
            Array.Clear(learned, 0, Count);
            Array.Clear(conditional, 0, conditional.Length);
            Array.Clear(held, 0, Count);
            againstCount = 0;
            Version++;
        }

        public void AddLearned(StatMod mod)
        {
            if (mod.Against != AfflictionKind.None)
            {
                if (againstCount < AgainstCapacity) against[againstCount++] = mod;
            }
            else if (mod.When == StatWhen.Always) learned[(int)mod.Stat] += mod.Amount;
            else
            {
                conditional[(int)mod.When * Count + (int)mod.Stat] += mod.Amount;
                if (holds[(int)mod.When] && !IsBlow(mod.When)) held[(int)mod.Stat] += mod.Amount;
            }
            Version++;
        }

        /// <summary>Says whether a state of the player holds now. Cheap to call every frame: nothing moves unless it changed.</summary>
        public void SetWhen(StatWhen when, bool now)
        {
            int row = (int)when;
            if (row <= 0 || IsBlow(when) || holds[row] == now) return;
            holds[row] = now;
            Array.Clear(held, 0, Count);
            for (int w = 1; w < WhenCount; w++)
            {
                if (!holds[w]) continue;
                for (int s = 0; s < Count; s++) held[s] += conditional[w * Count + s];
            }
            Version++;
        }

        public bool Holds(StatWhen when) => when == StatWhen.Always || holds[(int)when];

        /// <summary>What the learned changes add to a stat for one blow of the weapon, by its weight and whether it is critical.</summary>
        public float Blow(PlayerStat stat, bool heavy, bool critical, bool endsChain = false)
        {
            float sum = conditional[(int)(heavy ? StatWhen.HeavyBlow : StatWhen.LightBlow) * Count + (int)stat];
            if (critical) sum += conditional[(int)StatWhen.CriticalBlow * Count + (int)stat];
            if (endsChain) sum += conditional[(int)StatWhen.ChainEndBlow * Count + (int)stat];
            return sum;
        }

        /// <summary>What the learned changes add to a stat for a blow on an enemy in the air.</summary>
        public float InAir(PlayerStat stat) => conditional[(int)StatWhen.AirborneBlow * Count + (int)stat];

        /// <summary>What holds on a heavy blow that lands on an enemy in the air, on top of <see cref="InAir"/> (Hammer Down).</summary>
        public float InAirHeavy(PlayerStat stat) => conditional[(int)StatWhen.AirborneHeavyBlow * Count + (int)stat];

        /// <summary>What the learned changes add to a stat against a target carrying these afflictions.</summary>
        public float Against(PlayerStat stat, AfflictionSet target)
        {
            if (againstCount == 0 || target == null) return 0f;
            float sum = 0f;
            for (int i = 0; i < againstCount; i++)
                if (against[i].Stat == stat && target.Has(against[i].Against)) sum += against[i].Amount;
            return sum;
        }

        /// <summary>
        /// What multiplies a reaction's own damage for the pair <paramref name="first"/> and <paramref name="second"/>: the
        /// stat's sum, and every learned change tied to either affliction (a reaction with it in it). Never below the least.
        /// </summary>
        public float ReactionMultiplier(PlayerStat stat, AfflictionKind first, AfflictionKind second)
        {
            float sum = Sum(stat);
            for (int i = 0; i < againstCount; i++)
                if (against[i].Stat == stat && (against[i].Against == first || against[i].Against == second)) sum += against[i].Amount;
            return Math.Max(LeastMultiplier, 1f + sum);
        }

        public void ClearTally()
        {
            Array.Clear(tally, 0, Count);
            Version++;
        }

        public void AddTally(PlayerStat stat, float amount)
        {
            tally[(int)stat] += amount;
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

        /// <summary>A timed change. The same stat by the same amount given again starts its time over and does not stack with itself.</summary>
        /// <summary>Ends a boon before its time (one that lasts only until the next blow). False when no such boon runs.</summary>
        public bool EndBoon(PlayerStat stat, float amount)
        {
            for (int i = 0; i < boons.Length; i++)
            {
                if (boons[i].Left <= 0f || boons[i].Stat != stat || boons[i].Amount != amount) continue;
                boons[i].Left = 0f;
                boon[(int)stat] -= amount;
                Version++;
                return true;
            }
            return false;
        }

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

        public float Sum(PlayerStat stat) => learned[(int)stat] + surge[(int)stat] + boon[(int)stat] + held[(int)stat] + tally[(int)stat];

        public float Multiplier(PlayerStat stat) => Math.Max(LeastMultiplier, 1f + Sum(stat));

        public int Whole(PlayerStat stat) => (int)Math.Floor(Sum(stat) + 0.0001f);
    }
}
