namespace Clover.Player.Growth
{
    /// <summary>How the dodge works (IMPLEMENTATION.md 3.10). A <see cref="RuleBook"/> switch; <c>Normal</c> is the standard rule.</summary>
    public enum DodgeMode
    {
        /// <summary>The roll.</summary>
        Normal,
        /// <summary>There is no dodge (phase 6).</summary>
        Off,
        /// <summary>A short step in place of the roll (phase 6).</summary>
        Step,
    }
}
