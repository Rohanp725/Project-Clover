namespace Clover.Player.Growth
{
    /// <summary>Where mana comes from and what pays for it (IMPLEMENTATION.md 3.2, 3.10). A <see cref="RuleBook"/> switch; <c>Normal</c> is the standard rule.</summary>
    public enum ManaMode
    {
        /// <summary>The mana bar, earned by blows, perfect guards and dodges, finishers.</summary>
        Normal,
        /// <summary>No mana: every price is paid in health (Heartwood's rule, today kept by ManaPool.Instead).</summary>
        Sealed,
        /// <summary>No bar: mana is the Static on your enemies (Drawn From the Cloud, phase 6).</summary>
        FromStatic,
    }
}
