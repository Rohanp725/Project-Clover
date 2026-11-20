namespace Clover.Player.Growth
{
    /// <summary>How spells are kept from being cast again (IMPLEMENTATION.md 3.1, 3.10). A <see cref="RuleBook"/> switch; <c>Normal</c> is the standard rule.</summary>
    public enum CooldownMode
    {
        /// <summary>Each slot waits on its own clock, cut by weapon play.</summary>
        Normal,
        /// <summary>No clock starts at all (Given Fire, phase 6).</summary>
        None,
        /// <summary>No clocks; every basic spell costs mana instead and weapon play no longer cuts (Old Magic, phase 6).</summary>
        ManaPriced,
    }
}
