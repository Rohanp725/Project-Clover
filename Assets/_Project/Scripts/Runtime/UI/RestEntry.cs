namespace Clover.UI
{
    /// <summary>One thing a rest fire offers (IMPLEMENTATION.md 4.1). <see cref="None"/> is what a press gives on nothing.</summary>
    public enum RestEntry
    {
        None,
        /// <summary>The Hand: the four spells and the surge from the elements carried.</summary>
        Hand,
        /// <summary>The chart of the element trees, where Shards are spent.</summary>
        Chart,
        /// <summary>The Way, the weapon's tree, where Memories are spent (phase 5).</summary>
        Way,
        /// <summary>The road to the hub, to change elements and weapon (the linear game, Track L).</summary>
        Hub,
        Leave,
    }
}
