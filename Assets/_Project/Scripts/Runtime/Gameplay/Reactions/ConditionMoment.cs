namespace Clover.Gameplay
{
    /// <summary>What happened to an enemy condition, as <see cref="CombatEvents.Condition"/> tells it.</summary>
    public enum ConditionMoment
    {
        /// <summary>It was set on an enemy that did not have it (a refresh is not a beginning).</summary>
        Began,
        /// <summary>Its beat came round.</summary>
        Beat,
        /// <summary>A blow landed on the enemy that carries it (only for a condition that answers blows).</summary>
        Struck,
        /// <summary>Its time ran out.</summary>
        Ended,
        /// <summary>
        /// The enemy carrying it struck at the player and the blow landed or was turned (guarded, perfectly guarded or
        /// warded): only for a condition that answers this (the ledger's their-blow-moment; The Grudge's cloud).
        /// </summary>
        Attacked,
    }
}
