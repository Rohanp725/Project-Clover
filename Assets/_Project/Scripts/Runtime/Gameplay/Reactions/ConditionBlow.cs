namespace Clover.Gameplay
{
    /// <summary>
    /// Which blow on an enemy spends a condition (<see cref="ConditionSpec.EndsWith"/>): the "next heavy blow" of so many
    /// reactions. The blow is answered first, then the condition ends, and its end is not told: it was used, not run out.
    /// </summary>
    public enum ConditionBlow
    {
        /// <summary>No blow spends it; it runs its time.</summary>
        None,
        /// <summary>Any blow, of the weapon or a spell.</summary>
        Any,
        /// <summary>A blow of the weapon.</summary>
        Weapon,
        /// <summary>A heavy blow of the weapon (a finisher counts).</summary>
        Heavy,
        /// <summary>No blow of the player's: the carrier's own blow aimed at the player, landing or turned (The Grudge's cloud).</summary>
        TheirBlow,
    }
}
