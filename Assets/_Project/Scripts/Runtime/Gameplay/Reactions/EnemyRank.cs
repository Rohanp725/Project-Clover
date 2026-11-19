using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// How hard an enemy is to hold (<see cref="AfflictionSet.Rank"/>), for the reactions that give elites and gods their
    /// own numbers (Supercooled's 3 Chill for an elite, Capsize's stagger for a god). Read from what already decides
    /// holding: what cannot be frozen is a god, what is frozen for less than the whole time an elite. An effect asking
    /// for none of them answers all three.
    /// </summary>
    [Flags]
    public enum EnemyRank
    {
        Any = 0,
        Plain = 1 << 0,
        Elite = 1 << 1,
        God = 1 << 2,
    }
}
