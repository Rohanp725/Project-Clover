using System;
using Clover.Gameplay;

namespace Clover.Player
{
    /// <summary>
    /// The afflictions laid on the player (standin_clearance_plan.md question 1, decided 2026-11-21): elites and gods lay
    /// their land's affliction when a blow of theirs lands (Burn in Sun City, Static on the Castle's heights). Each lies
    /// as deep as its spec allows and for its spec's time, scaled by <see cref="SecondsScale"/> (Tremor, Earth's root: half
    /// as long), and bites as it would bite an enemy: Burn by its burning, the rest by their own tick. While
    /// <see cref="Shedding"/> holds (Bedrock) nothing stays on; High Noon clears all at once. Plain C#, owned by
    /// <see cref="PlayerDefence"/>, which applies the bites; nothing is allocated after it is made.
    /// </summary>
    // What each does to the player mirrors what it does to an enemy, where that makes sense for a player (the owner's answer
    // of 2026-11-22): Chill and Tremor slow the step, Burn and Poison bite, Soaked makes the dodge slower to come back,
    // Sundered and Marked let blows on you cut deeper, Cursed makes your blows weaker, Unsteady throws you farther. Static
    // and Seeded do nothing more to the player than show. NEEDS: the light look on the body (looks).
    public sealed class PlayerAfflictions
    {
        private static readonly int Kinds = Enum.GetValues(typeof(AfflictionKind)).Length;
        private readonly int[] stacks = new int[Kinds];
        private readonly float[] left = new float[Kinds];
        private readonly float[] untilTick = new float[Kinds];
        private readonly BurnStatus burn = new BurnStatus();

        /// <summary>What every affliction's time on the player is multiplied by (Tremor: 0.5).</summary>
        public float SecondsScale = 1f;

        private bool shedding;
        /// <summary>While true nothing is laid and whatever was on the player is shed (Bedrock while it lasts).</summary>
        public bool Shedding
        {
            get => shedding;
            set
            {
                shedding = value;
                if (value && Any) Clear();
            }
        }

        /// <summary>Seconds a dodge takes longer to come back for each Soaked on the player.</summary>
        public const float SoakedDodgeSeconds = 0.1f;
        /// <summary>How much harder blows land on the player for each Mark (an enemy's blow on a marked player strikes as a critical would).</summary>
        public const float MarkedTakenPerStack = 0.15f;

        /// <summary>Changes whenever what is on the player changes, so its numbers are pushed again (PlayerUpgrades.ApplyStats).</summary>
        public int Version { get; private set; }

        /// <summary>The share of its speed the player keeps (Chill and Tremor, by their spec's slow a stack).</summary>
        public float MoveScale
        {
            get
            {
                float pace = 1f;
                for (int i = 0; i < Kinds; i++)
                    if (stacks[i] > 0) pace *= Math.Max(0.2f, 1f - Afflictions.Spec((AfflictionKind)i).SlowPerStack * stacks[i]);
                return pace;
            }
        }

        /// <summary>What blows on the player are multiplied by (Sundered, by its spec; Marked).</summary>
        public float TakenScale
        {
            get
            {
                float more = 0f;
                for (int i = 0; i < Kinds; i++)
                    if (stacks[i] > 0) more += Afflictions.Spec((AfflictionKind)i).WeaponDamageTakenPerStack * stacks[i];
                return 1f + more + MarkedTakenPerStack * Stacks(AfflictionKind.Marked);
            }
        }

        /// <summary>What the player's own blows are multiplied by (Cursed, by its spec; never below 0.3).</summary>
        public float DealtScale
        {
            get
            {
                float less = 0f;
                for (int i = 0; i < Kinds; i++)
                    if (stacks[i] > 0) less += Afflictions.Spec((AfflictionKind)i).DealsLessPerStack * stacks[i];
                return Math.Max(0.3f, 1f - less);
            }
        }

        /// <summary>What a blow's push on the player is multiplied by (Unsteady, by its spec).</summary>
        public float KnockbackScale
        {
            get
            {
                float more = 0f;
                for (int i = 0; i < Kinds; i++)
                    if (stacks[i] > 0) more += Afflictions.Spec((AfflictionKind)i).ShovePerStack * stacks[i];
                return 1f + more;
            }
        }

        /// <summary>Seconds added to the dodge's wait (Soaked).</summary>
        public float DodgeRecoveryAdd => SoakedDodgeSeconds * Stacks(AfflictionKind.Soaked);

        /// <summary>
        /// Fills <paramref name=kinds/> and <paramref name=seconds/> with what is on the player now, in the order of the
        /// kinds, for the HUD's marks by the health bar; returns how many.
        /// </summary>
        public int Shown(AfflictionKind[] kinds, float[] seconds)
        {
            int count = 0;
            for (int i = 0; i < Kinds && count < kinds.Length && count < seconds.Length; i++)
            {
                var kind = (AfflictionKind)i;
                if (!Has(kind)) continue;
                kinds[count] = kind;
                seconds[count] = SecondsLeft(kind);
                count++;
            }
            return count;
        }

        /// <summary>Times something was laid on the player, for probes and tests.</summary>
        public int Laid { get; private set; }

        public bool Any
        {
            get
            {
                if (burn.IsBurning) return true;
                for (int i = 0; i < Kinds; i++)
                    if (stacks[i] > 0) return true;
                return false;
            }
        }

        public bool Has(AfflictionKind kind) => kind == AfflictionKind.Burn ? burn.IsBurning : Valid(kind) && stacks[(int)kind] > 0;

        public int Stacks(AfflictionKind kind) => kind == AfflictionKind.Burn ? (burn.IsBurning ? Math.Max(1, burn.Stacks) : 0) : Valid(kind) ? stacks[(int)kind] : 0;

        public float SecondsLeft(AfflictionKind kind) => kind == AfflictionKind.Burn ? burn.SecondsLeft : Valid(kind) ? left[(int)kind] : 0f;

        /// <summary>Lays <paramref name="count"/> stacks for the spec's time (or <paramref name="seconds"/>), scaled. False when nothing took.</summary>
        public bool Lay(AfflictionKind kind, int count, float seconds = 0f)
        {
            if (shedding || !Valid(kind) || kind == AfflictionKind.None || count <= 0) return false;
            var spec = Afflictions.Spec(kind);
            float time = (seconds > 0f ? seconds : spec.Seconds) * Math.Max(0f, SecondsScale);
            if (time <= 0f) return false;
            Laid++;
            Version++;
            if (kind == AfflictionKind.Burn)
            {
                burn.Ignite(time, Math.Max(burn.Stacks, 0) + count);
                return true;
            }
            int index = (int)kind;
            if (stacks[index] == 0) untilTick[index] = spec.TickSeconds;
            stacks[index] = Math.Min(Math.Max(1, spec.MaximumStacks), stacks[index] + count);
            left[index] = Math.Max(left[index], time);
            return true;
        }

        /// <summary>Runs the clocks; returns the damage the afflictions bite the player for in this step.</summary>
        public float Tick(float deltaTime)
        {
            bool wasBurning = burn.IsBurning;
            float bite = wasBurning ? burn.Tick(deltaTime) : 0f;
            if (wasBurning && !burn.IsBurning) Version++;
            for (int i = 0; i < Kinds; i++)
            {
                if (stacks[i] <= 0) continue;
                var spec = Afflictions.Spec((AfflictionKind)i);
                if (spec.DamagePerTick > 0f && spec.TickSeconds > 0f)
                {
                    untilTick[i] -= deltaTime;
                    while (untilTick[i] <= 0f)
                    {
                        untilTick[i] += spec.TickSeconds;
                        bite += spec.DamagePerTick * stacks[i];
                    }
                }
                left[i] -= deltaTime;
                if (left[i] <= 0f) End(i);
            }
            return bite;
        }

        /// <summary>Everything on the player goes at once (High Noon, and the shedding of Bedrock).</summary>
        public void Clear()
        {
            if (burn.IsBurning) Version++;
            burn.Reset();
            for (int i = 0; i < Kinds; i++) End(i);
        }

        /// <summary>True for the ranks that lay their land's affliction when they hit: elites and gods.</summary>
        public static bool Lays(EnemyRank rank) => (rank & (EnemyRank.Elite | EnemyRank.God)) != 0;

        /// <summary>The affliction of a land, by its element's name (Outfits.LandOf); None outside every land.</summary>
        public static AfflictionKind OfLand(string land)
        {
            if (string.IsNullOrEmpty(land)) return AfflictionKind.None;
            var element = Elements.ByName(land);
            return element != null ? element.Affliction : AfflictionKind.None;
        }

        private void End(int index)
        {
            if (stacks[index] != 0) Version++;
            stacks[index] = 0;
            left[index] = 0f;
            untilTick[index] = 0f;
        }

        private static bool Valid(AfflictionKind kind) => (int)kind >= 0 && (int)kind < Kinds;
    }
}
