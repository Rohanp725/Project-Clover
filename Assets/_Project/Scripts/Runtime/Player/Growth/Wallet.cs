using System;
using Clover.Core;

namespace Clover.Player.Growth
{
    /// <summary>
    /// The two currencies (IMPLEMENTATION.md 3.12), as the save keeps them: Shards for the element trees and the duo
    /// branches, Memories for the Way and the Body. Plain, so the save writes it whole and tests build one. What a mark
    /// costs is read from the mark (<see cref="Upgrade.Currency"/>, <see cref="Upgrade.Cost"/>), so nothing here knows
    /// the prices.
    ///
    /// Memories are kept through death (the owner's decision, docs/GAME.md); only the carried Shards are at risk, and
    /// <see cref="TakeCarriedShards"/> is the one place they are taken. The run's Shards still live in
    /// <c>RunState.Points</c>, which the chart spends, until the death rule (task L.3) moves them here
    /// (docs/design/steel/phase5_questions.md, question 1).
    /// </summary>
    [Serializable]
    public class Wallet
    {
        /// <summary>Carried Shards, lost to a death and found again where it happened.</summary>
        public int Shards;
        /// <summary>Memories, the Way's currency. Never lost.</summary>
        public int Memories;
        /// <summary>Shards laid in a rest fire once every carried tree is full: safe from death.</summary>
        public int ShardsBanked;

        /// <summary>How much of one currency is held; nothing of <see cref="Currency.None"/>.</summary>
        public int Of(Currency currency)
        {
            switch (currency)
            {
                case Currency.Shards: return Shards;
                case Currency.Memories: return Memories;
                default: return 0;
            }
        }

        /// <summary>Adds to one currency. Nothing happens for none or for a count of zero or less.</summary>
        public void Earn(Currency currency, int amount)
        {
            if (amount <= 0) return;
            if (currency == Currency.Shards) Shards += amount;
            else if (currency == Currency.Memories) Memories += amount;
        }

        /// <summary>Takes from one currency. False, and nothing taken, when there is not that much.</summary>
        public bool Spend(Currency currency, int amount)
        {
            if (amount < 0 || currency == Currency.None) return false;
            if (Of(currency) < amount) return false;
            if (currency == Currency.Shards) Shards -= amount;
            else Memories -= amount;
            return true;
        }

        /// <summary>True when the mark's price can be paid in its own currency. A given mark costs nothing.</summary>
        public bool CanAfford(Upgrade mark) => mark != null && (mark.Given || Of(mark.Currency) >= mark.Cost);

        /// <summary>Pays the mark's price in its own currency. False, and nothing paid, when it cannot be afforded.</summary>
        public bool Pay(Upgrade mark)
        {
            if (!CanAfford(mark)) return false;
            if (mark.Given || mark.Cost <= 0) return true;
            return Spend(mark.Currency, mark.Cost);
        }

        /// <summary>Gives the mark's price back in its own currency, as it was paid.</summary>
        public void Refund(Upgrade mark)
        {
            if (mark == null || mark.Given) return;
            Earn(mark.Currency, mark.Cost);
        }

        /// <summary>
        /// A rest: in the playtest both currencies are topped up to more than the whole of each chart costs
        /// (<see cref="PlaytestMode.RestShards"/>, <see cref="PlaytestMode.RestMemories"/>); outside it a rest gives
        /// nothing.
        /// </summary>
        public void Rested()
        {
            Shards = PlaytestMode.ShardsAfterRest(Shards);
            Memories = PlaytestMode.MemoriesAfterRest(Memories);
        }

        /// <summary>
        /// A death takes the carried Shards (the death rule keeps them for the killer, task L.3) and returns how many.
        /// The banked Shards and every Memory stay.
        /// </summary>
        public int TakeCarriedShards()
        {
            int taken = Shards;
            Shards = 0;
            return taken;
        }
    }
}
