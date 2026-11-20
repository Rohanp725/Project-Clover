namespace Clover.Player.Growth
{
    /// <summary>How the charged blade is lit and spent (IMPLEMENTATION.md 3.3, 3.10). A <see cref="RuleBook"/> switch; <c>Normal</c> is the standard rule.</summary>
    public enum BladeMode
    {
        /// <summary>Lit by a slot spell's landing, spent in three blows.</summary>
        Normal,
        /// <summary>A critical blow lights it (The Edge Kept Bright, phase 6).</summary>
        LitByCritical,
        /// <summary>It is never spent (The Edge Drinks, phase 6).</summary>
        NeverFades,
    }
}
