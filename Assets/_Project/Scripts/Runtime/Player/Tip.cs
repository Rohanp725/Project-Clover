namespace Clover.Player
{
    /// <summary>The one-line tips the game gives, each once in a play session.</summary>
    public enum Tip
    {
        /// <summary>Blows keep landing on a lowered guard.</summary>
        Guard,
        /// <summary>An enemy keeps turning light blows on its guard.</summary>
        BreakGuard,
        /// <summary>An enemy nearby has lost its balance.</summary>
        Finish,
    }
}
