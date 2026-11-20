namespace Clover.Player
{
    /// <summary>What a mark is bought with (IMPLEMENTATION.md 2.1). Marks written before the field existed are bought with Shards.</summary>
    public enum Currency
    {
        /// <summary>The element trees and the duo branches.</summary>
        Shards,
        /// <summary>The Way and the Body branch (Phase 5).</summary>
        Memories,
        /// <summary>Never bought: given (a base spell with its element, a god's gift, a Bond mark, a god's Heresy).</summary>
        None,
    }
}
