using System;

namespace Clover.Player.Growth
{
    /// <summary>
    /// The two currencies (IMPLEMENTATION.md 3.12), as the save keeps them. Only the numbers for now: <c>Earn</c>,
    /// <c>Spend</c> and <c>CanAfford</c> come with task 5.1, when <c>RunState.Points</c> moves here.
    /// </summary>
    [Serializable]
    public class Wallet
    {
        /// <summary>Carried Shards, lost to a death and found again where it happened.</summary>
        public int Shards;
        /// <summary>Memories, the Way's currency.</summary>
        public int Memories;
        /// <summary>Shards laid in a rest fire once every carried tree is full: safe from death.</summary>
        public int ShardsBanked;
    }
}
