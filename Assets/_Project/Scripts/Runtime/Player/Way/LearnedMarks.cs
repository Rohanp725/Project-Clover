using System.Collections.Generic;
using Clover.Player.Build;

namespace Clover.Player.Way
{
    /// <summary>
    /// The marks in force under a loadout that is not yet the player's: what is learned, asked through
    /// <see cref="MarkRules.InForce"/> as <see cref="PlayerUpgrades"/> asks it in play. The armoury arms a weapon with it
    /// before the weapon is in hand, so the marks are judged for the weapon about to be carried, not the one put down.
    /// </summary>
    public sealed class LearnedMarks : IMarkQuery
    {
        private readonly Loadout loadout;
        private readonly ICollection<string> learned;

        public LearnedMarks(Loadout carried, ICollection<string> learnedIds)
        {
            loadout = carried;
            learned = learnedIds;
        }

        public bool Has(string id) => learned != null && id != null && learned.Contains(id) && MarkRules.InForce(Upgrade.ById(id), loadout);
    }
}
