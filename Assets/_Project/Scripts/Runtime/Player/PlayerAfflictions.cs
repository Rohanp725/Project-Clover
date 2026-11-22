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
    // NEEDS: what each affliction does to the player beyond its bite (a Chill slowing the step, a Soaked, Cursed or Marked
    // one's own numbers), a look on the body and a mark on the HUD (afflictions-on-player; the HUD needs the editor).
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
            float bite = burn.IsBurning ? burn.Tick(deltaTime) : 0f;
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
            stacks[index] = 0;
            left[index] = 0f;
            untilTick[index] = 0f;
        }

        private static bool Valid(AfflictionKind kind) => (int)kind >= 0 && (int)kind < Kinds;
    }
}
