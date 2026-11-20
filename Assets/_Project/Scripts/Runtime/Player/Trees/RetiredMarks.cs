using System.Collections.Generic;
using Clover.Core.Save;

namespace Clover.Player
{
    /// <summary>
    /// Marks cut from the trees when they were reshaped into spell branches (Phase 4; IMPLEMENTATION.md 2.8, 5.2, 8.6), with
    /// what each cost then. A save that learned one is refunded and the id is taken out (<see cref="SaveMigration"/>). An id
    /// listed here is never used again by a new mark (<c>UpgradeChartTests</c> holds the catalogue to that).
    ///
    /// Two lists: <see cref="All"/>, the marks cut outright or folded into another mark (refunded); and <see cref="Moving"/>,
    /// the weapon-bound marks that leave the element trees for the Katana's Way (steel/README.md, "Weapons and the Way"),
    /// whose ids the Way will take back in phase 5, so a save keeps them and nothing is refunded.
    /// In namespace Clover.Player, not Trees, because it is no tree file.
    /// </summary>
    public static class RetiredMarks
    {
        private static readonly RetiredMark[] retired =
        {
            // Fire (elements_a.md, "What to cut from the built content"): the plain minors, the dead "costs less" minor,
            // Flashpoint and Wildfire, the keystones not carried over, and the spells folded or replaced.
            new RetiredMark("fire.stoked", 1), new RetiredMark("fire.banked", 1), new RetiredMark("fire.searing", 1), new RetiredMark("fire.fuse", 1),
            new RetiredMark("fire.tinder", 1), new RetiredMark("fire.flint", 1), new RetiredMark("fire.breath", 1), new RetiredMark("fire.hearth", 1),
            new RetiredMark("fire.embers", 1), new RetiredMark("fire.footing", 1), new RetiredMark("fire.broadwave", 1), new RetiredMark("fire.shortbreath", 1),
            new RetiredMark("fire.coals", 1), new RetiredMark("fire.longkindle", 1), new RetiredMark("fire.kindling", 1),
            new RetiredMark("fire.flash", 3), new RetiredMark("fire.wildfire", 3),
            // Burn and Detonate is folded into Breath of the Lioness (once Backdraft), which keeps its id.
            new RetiredMark("fire.detonate", 3),
            new RetiredMark("fire.furnace", 6), new RetiredMark("fire.scorched", 6),
            // Sunspot is replaced by Black Pitch; Cinder Fan is folded into Flame Wave as a shape.
            new RetiredMark("fire.sunspot", 4), new RetiredMark("fire.fan", 4),
        };

        // NEEDS: the owner's word on whether a Shards-bought mark that moves to the Way (paid in Memories) is refunded then.
        private static readonly RetiredMark[] moving =
        {
            new RetiredMark(Upgrade.BurningCrescent, 3),
        };

        /// <summary>The marks cut, with their prices: a save that holds one is refunded.</summary>
        public static IReadOnlyList<RetiredMark> All => retired;

        /// <summary>The marks leaving the element trees for a weapon's Way: kept in a save, not refunded, their ids waiting for the Way.</summary>
        public static IReadOnlyList<RetiredMark> Moving => moving;

        /// <summary>True when the id was cut (refunded); a moving mark is not.</summary>
        public static bool IsRetired(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < retired.Length; i++)
                if (retired[i].Id == id) return true;
            return false;
        }

        /// <summary>True when the id waits for a weapon's Way.</summary>
        public static bool IsMoving(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < moving.Length; i++)
                if (moving[i].Id == id) return true;
            return false;
        }
    }
}
