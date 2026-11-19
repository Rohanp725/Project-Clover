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
            // A hidden mark (a reaction) is on no tree, so no tree opens it.
            if (upgrade == null || upgrade.IsHidden) return false;
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
            if (upgrade == null || upgrade.IsHidden || NotBuiltYet.IsSlot(upgrade) || !open) return false;
            if (upgrade.Signature && !playtest) return false;
            if (unlocked.Contains(upgrade.Id) || points < upgrade.Cost) return false;
            return upgrade.Signature || UpgradeTree.Reachable(upgrade, unlocked);
        }

        /// <summary>
        /// True when a learned mark is in force under a loadout (IMPLEMENTATION.md 3.6): the one place that decides it,
        /// read by <see cref="PlayerUpgrades"/>. An element's mark (its gift too) while its element is attuned; a duo
        /// branch's mark while both its elements are; a duo spell only while it also sits in a slot; a mark that names a
        /// weapon only while that weapon kind is carried (a god's weapon counts as its kind: the Storm Katana is a
        /// Katana). The rules for Devotion, spell branches, shapes, reactions, the Way, Heresies, Bond and Evolutions join
        /// here as their fields are added (phases 3 to 6).
        /// </summary>
        public static bool InForce(Upgrade mark, Build.Loadout loadout)
        {
            if (mark == null || loadout == null) return false;
            if (!WeaponCarried(mark, loadout) || !Carried(mark, loadout)) return false;
            if (mark.IsMix && mark.IsSpell && !loadout.Slotted(mark.Id)) return false;
            return true;
        }

        /// <summary>True when a spell mark may sit in a slot under the loadout: its elements and its weapon (if it names one) carried. Learning is asked elsewhere.</summary>
        public static bool Slottable(Upgrade mark, Build.Loadout loadout) =>
            mark != null && mark.IsSpell && WeaponCarried(mark, loadout) && Carried(mark, loadout);

        /// <summary>True when the mark names no weapon, or the loadout's weapon is that kind (a god's weapon counts as its kind).</summary>
        public static bool WeaponCarried(Upgrade mark, Build.Loadout loadout)
        {
            if (mark == null || loadout == null) return false;
            return string.IsNullOrEmpty(mark.Weapon) || string.IsNullOrEmpty(loadout.Weapon) || loadout.Weapon.Contains(mark.Weapon);
        }

        /// <summary>True when the mark's element is attuned; for a duo branch's mark, both of its elements.</summary>
        public static bool Carried(Upgrade mark, Build.Loadout loadout)
        {
            if (mark == null || loadout == null) return false;
            if (!loadout.Carries(mark.Element)) return false;
            return !mark.IsMix || loadout.Carries(mark.Element2);
        }
    }
}
