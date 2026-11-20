namespace Clover.Player.Growth
{
    /// <summary>How the player's health is kept (IMPLEMENTATION.md 3.10). A <see cref="RuleBook"/> switch; <c>Bar</c> is the standard rule.</summary>
    public enum HealthMode
    {
        /// <summary>The health bar.</summary>
        Bar,
        /// <summary>A count of stones, one lost a blow (Count the Stones, phase 6).</summary>
        Stones,
    }
}
