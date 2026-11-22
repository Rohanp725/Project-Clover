namespace Clover.Player.Growth
{
    /// <summary>How the guard works (IMPLEMENTATION.md 3.10). A <see cref="RuleBook"/> switch; <c>Normal</c> is the standard rule.</summary>
    public enum GuardMode
    {
        /// <summary>Raised for stamina; a blow the stamina cannot pay for breaks it.</summary>
        Normal,
        /// <summary>There is no guard (phase 6).</summary>
        Off,
        /// <summary>The guard is a draw of the blade (phase 6).</summary>
        Draw,
        /// <summary>The guard covers every side (phase 6).</summary>
        AllRound,
        /// <summary>The guard never breaks: what the stamina cannot pay comes through as grey health (The Guard Holds).</summary>
        Unbreakable,
        /// <summary>
        /// A held guard blocks nothing: only the perfect guard turns a blow, and it may be pressed again sooner (Edge Guard; the
        /// ledger's guard-edge).
        /// </summary>
        Edge,
    }
}
