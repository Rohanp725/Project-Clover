namespace Clover.Player.Growth
{
    /// <summary>What breaks an enemy's balance (IMPLEMENTATION.md 3.10). A <see cref="RuleBook"/> switch; <c>Blows</c> is the standard rule.</summary>
    public enum StaggerSource
    {
        /// <summary>Blows, as written.</summary>
        Blows,
        /// <summary>Burn instead of blows (phase 6).</summary>
        Burn,
    }
}
