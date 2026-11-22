using Clover.Gameplay;

namespace Clover.Player
{
    /// <summary>
    /// One change to one of the player's numbers: a share (0.1 is ten percent more) or a flat amount, by the stat.
    /// It may hold only in a state or on a kind of blow (<see cref="When"/>), or only against a target that carries a
    /// named affliction (<see cref="Against"/>; weapon damage, stagger, critical chance and damage, and spell damage only).
    /// </summary>
    public readonly struct StatMod
    {
        public readonly PlayerStat Stat;
        public readonly float Amount;
        public readonly StatWhen When;
        public readonly AfflictionKind Against;
        /// <summary>With <see cref="Against"/>: the amount for each stack of it the target carries, not once (Weighed: critical blows 10% harder a Mark).</summary>
        public readonly bool PerDepth;

        public StatMod(PlayerStat stat, float amount)
        {
            Stat = stat;
            Amount = amount;
            When = StatWhen.Always;
            Against = AfflictionKind.None;
            PerDepth = false;
        }

        public StatMod(PlayerStat stat, float amount, StatWhen when)
        {
            Stat = stat;
            Amount = amount;
            When = when;
            Against = AfflictionKind.None;
            PerDepth = false;
        }

        public StatMod(PlayerStat stat, float amount, AfflictionKind against, bool perDepth = false)
        {
            Stat = stat;
            Amount = amount;
            When = StatWhen.Always;
            Against = against;
            PerDepth = perDepth;
        }
    }
}
