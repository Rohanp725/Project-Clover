using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// Whether a mark on the chart is open and can be bought, as plain rules over plain state so they can be tested
    /// without a player: the game's own rules, and the playtest's (every tree and branch open, the gods' gifts sold).
    /// </summary>
    public static class MarkRules
    {
        /// <summary>True when the mark is on a tree or branch open now: the element held, a mix of it with an element held or consumed, or, in the playtest, any.</summary>
        public static bool Open(Upgrade upgrade, string held, ICollection<string> everHeld, ICollection<string> consumed, bool playtest)
        {
            if (upgrade == null) return false;
            if (playtest) return true;
            if (upgrade.IsMix) return UpgradeTree.MixOpen(upgrade, held, everHeld, consumed);
            return upgrade.Element == held;
        }

        /// <summary>
        /// True when the mark can be bought: open, not learned, reachable from what is learned, and paid for. A god's
        /// gift is never sold, except in the playtest, where it is a playtest unlock that needs no road to it. The
        /// playtest's "Not built yet" slots are never sold.
        /// </summary>
        public static bool CanBuy(Upgrade upgrade, ICollection<string> unlocked, int points, bool open, bool playtest)
        {
            if (upgrade == null || NotBuiltYet.IsSlot(upgrade) || !open) return false;
            if (upgrade.Signature && !playtest) return false;
            if (unlocked.Contains(upgrade.Id) || points < upgrade.Cost) return false;
            return upgrade.Signature || UpgradeTree.Reachable(upgrade, unlocked);
        }
    }
}
