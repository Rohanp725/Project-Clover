namespace Clover.Player.Way
{
    /// <summary>
    /// What an aspect's patch asks of the player's marks (IMPLEMENTATION.md 2.5): whether one is learned and in force
    /// now. <see cref="PlayerUpgrades"/> answers it in play; <see cref="MarkSet"/> answers it from a plain set in tests
    /// and in the armoury's previews.
    /// </summary>
    public interface IMarkQuery
    {
        /// <summary>True when the mark is learned and in force under the loadout carried.</summary>
        bool Has(string id);
    }
}
