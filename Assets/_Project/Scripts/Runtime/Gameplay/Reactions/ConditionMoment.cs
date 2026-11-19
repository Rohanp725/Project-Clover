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
    }
}
