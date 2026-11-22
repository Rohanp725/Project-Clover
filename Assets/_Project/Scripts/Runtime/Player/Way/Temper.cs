using UnityEngine;

namespace Clover.Player.Way
{
    /// <summary>
    /// Temper, the trunk's five ranks on every Way (steel/weapons.md 0.3; way_build_plan.md 2.1; task 5.3): bought in
    /// order for 1, 1, 2, 2 and 3 Memories, each +6% weapon damage and +5% stagger (+30% and +25% at V), the only raw
    /// damage a Way gives. I and II are open from the start, III at the third god, IV the fifth, V the seventh (the
    /// playtest opens all). Temper shows nothing on the weapon; the chart shows the rank (question 4, the owner's answer).
    ///
    /// Temper V also holds the finisher's pose longer (question 5, the owner's answer): the Katana's finisher stops behind
    /// the enemy with the blade out for 0.2 s before it falls, and at Temper V for 0.4 s, its blow 15% harder. The hold is
    /// <see cref="AttackStep.HoldSeconds"/> on the finisher, after the blow (<see cref="AttackStep.HoldAt"/>).
    /// </summary>
    public static class Temper
    {
        public const int Ranks = 5;
        public const float DamagePerRank = 0.06f;
        public const float StaggerPerRank = 0.05f;
        // The Memories each rank costs, and the gods that must have fallen before it opens, in order (read only).
        private static readonly int[] Prices = { 1, 1, 2, 2, 3 };
        private static readonly int[] GodsBefore = { 0, 0, 3, 5, 7 };

        /// <summary>The finisher's held pose, and at Temper V.</summary>
        public const float FinisherHold = 0.2f, FinisherHoldAtFive = 0.4f;

        /// <summary>The weapon damage share at a rank: 1.18 at III, 1.30 at V.</summary>
        public static float DamageShare(int rank) => 1f + DamagePerRank * Mathf.Clamp(rank, 0, Ranks);

        /// <summary>The stagger share at a rank: 1.25 at V.</summary>
        public static float StaggerShare(int rank) => 1f + StaggerPerRank * Mathf.Clamp(rank, 0, Ranks);

        /// <summary>How long the finisher's pose holds at a rank.</summary>
        public static float FinisherHoldAt(int rank) => rank >= Ranks ? FinisherHoldAtFive : FinisherHold;

        /// <summary>The gate a rank (1 to 5) waits behind: "" for none, else <c>gods.N</c>.</summary>
        public static string GateOf(int rank)
        {
            if (rank < 1 || rank > Ranks) return "";
            int gods = GodsBefore[rank - 1];
            return gods > 0 ? MarkRules.GodsGate + gods : "";
        }

        /// <summary>The Memories a rank (1 to 5) costs.</summary>
        public static int PriceOf(int rank) => rank >= 1 && rank <= Ranks ? Prices[rank - 1] : 0;
    }
}
