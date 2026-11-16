namespace Clover.Gameplay
{
    /// <summary>What became of a blow, told back to whoever struck it so they can answer in kind.</summary>
    public enum HitOutcome
    {
        /// <summary>Nothing was there to take it.</summary>
        Missed,
        Landed,
        /// <summary>A heavy blow on a staggered target.</summary>
        Finisher,
        Blocked,
        /// <summary>Turned aside at the last moment; the attacker is the one shaken.</summary>
        PerfectGuard,
        /// <summary>Passed through a dodge.</summary>
        Dodged,
        /// <summary>Passed through a dodge made at the last moment.</summary>
        PerfectDodge,
    }
}
