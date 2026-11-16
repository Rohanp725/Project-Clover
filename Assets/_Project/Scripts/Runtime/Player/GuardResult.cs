namespace Clover.Player
{
    /// <summary>What the guard made of a blow.</summary>
    public enum GuardResult
    {
        /// <summary>The guard was down, or the blow cannot be blocked: it lands in full.</summary>
        NotGuarded,
        /// <summary>Blocked for stamina; a little of the damage comes through.</summary>
        Blocked,
        /// <summary>Guarded at the last moment: nothing is lost.</summary>
        Perfect,
        /// <summary>There was not the stamina to block it: the guard breaks and the blow lands in full.</summary>
        Broken,
    }
}
