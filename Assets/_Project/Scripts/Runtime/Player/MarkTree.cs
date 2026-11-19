namespace Clover.Player
{
    /// <summary>Which chart a mark is drawn on (IMPLEMENTATION.md 2.1). Marks written before the field existed are on their element's tree.</summary>
    public enum MarkTree
    {
        /// <summary>An element's tree, or a mix branch on the ring: the chart as it is.</summary>
        Element,
        /// <summary>A duo branch in the chart's heart (Phase 4).</summary>
        Duo,
        /// <summary>A weapon's Way (Phase 5).</summary>
        Way,
        /// <summary>The Body branch at the Way's foot (Phase 5).</summary>
        Body,
        /// <summary>Never drawn and never sold: the plain reactions, and later the undiscovered Evolutions.</summary>
        Hidden,
    }
}
