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

            // Lightning: the plain minors and the dead "costs less" minor, the majors not kept, Overclock, and Lodestone
            // folded into Lightning Rod as a shape.
            new RetiredMark("lightning.bitingstep", 1), new RetiredMark("lightning.forked", 1), new RetiredMark("lightning.return", 1),
            new RetiredMark("lightning.echo", 1), new RetiredMark("lightning.chargedstep", 1), new RetiredMark("lightning.heavyair", 1),
            new RetiredMark("lightning.longarc", 1), new RetiredMark("lightning.hardcharge", 1), new RetiredMark("lightning.keen", 1),
            new RetiredMark("lightning.seam", 1), new RetiredMark("lightning.quickclap", 1), new RetiredMark("lightning.thirdleap", 1),
            new RetiredMark("lightning.longdrive", 1), new RetiredMark("lightning.current", 1), new RetiredMark("lightning.quick", 1),
            new RetiredMark("lightning.capacitor", 1),
            // Deep Reserve is folded into Stored Charge, which keeps its id.
            new RetiredMark("lightning.reserve", 3),
            new RetiredMark("lightning.fulldischarge", 3), new RetiredMark("lightning.conductor", 3), new RetiredMark("lightning.wire", 3),
            new RetiredMark("lightning.grounding", 3), new RetiredMark("lightning.slipstream", 3),
            new RetiredMark("lightning.overclock", 6),
            new RetiredMark("lightning.lodestone", 4),

            // Wind: the plain minors and the dead "costs less" minor (Easy Breath), the majors not kept (Rushing Wake, Empty
            // Air, Hammer Down), and Storm Front, a keystone not carried over.
            new RetiredMark("wind.stride", 1), new RetiredMark("wind.thinair", 1), new RetiredMark("wind.draft", 1), new RetiredMark("wind.cleanpass", 1),
            new RetiredMark("wind.lightfeet", 1), new RetiredMark("wind.loose", 1), new RetiredMark("wind.offbalance", 1), new RetiredMark("wind.hang", 1),
            new RetiredMark("wind.openair", 1), new RetiredMark("wind.widegust", 1), new RetiredMark("wind.easybreath", 1), new RetiredMark("wind.strongarm", 1),
            new RetiredMark("wind.fairwind", 1), new RetiredMark("wind.runningstart", 1),
            new RetiredMark("wind.slip", 3), new RetiredMark("wind.emptyair", 3), new RetiredMark("wind.hammer", 3),
            new RetiredMark("wind.front", 6),

            // Ice: the plain minors and the dead "costs less" minor (Thin Ice, whose name a shape of Ski Run takes), the majors
            // not kept (Gathering Frost, Clean Break), Black Ice folded into Hoarfrost as a shape, Hollow Statue cut outright,
            // Unmoved (replaced by Sent Sliding) and Second Skin, a keystone not carried over.
            new RetiredMark("ice.patient", 1), new RetiredMark("ice.braced", 1), new RetiredMark("ice.snow", 1), new RetiredMark("ice.slowpulse", 1),
            new RetiredMark("ice.deepcold", 1), new RetiredMark("ice.longwinter", 1), new RetiredMark("ice.coldiron", 1), new RetiredMark("ice.heldfast", 1),
            new RetiredMark("ice.sharpbreak", 1), new RetiredMark("ice.thinice", 1), new RetiredMark("ice.longfrost", 1), new RetiredMark("ice.thick", 1),
            new RetiredMark("ice.clearice", 1), new RetiredMark("ice.bitter", 1),
            new RetiredMark("ice.gather", 3), new RetiredMark("ice.cleanbreak", 3), new RetiredMark("ice.blackice", 3),
            new RetiredMark("ice.statue", 4),
            new RetiredMark("ice.unmoved", 6), new RetiredMark("ice.skin", 6),

            // Water: the plain minors and the dead "costs less" minor (Low Water), the majors not kept (Flowing Guard,
            // Unbroken Chain, Spilled Water, and Catch the Rain, replaced by Drawn From the Drowned), River Serpent folded into
            // Riptide as a shape, Standing Rain and Moon on Water cut outright (Rip Current and Blowhole took their places),
            // and Spring Tide and River Without End, keystones not carried over.
            new RetiredMark("water.easy", 1), new RetiredMark("water.wading", 1), new RetiredMark("water.greytide", 1), new RetiredMark("water.longflood", 1),
            new RetiredMark("water.longpull", 1), new RetiredMark("water.lowwater", 1), new RetiredMark("water.soakedthrough", 1), new RetiredMark("water.weight", 1),
            new RetiredMark("water.clear", 1), new RetiredMark("water.earned", 1), new RetiredMark("water.stillpool", 1), new RetiredMark("water.second", 1),
            new RetiredMark("water.longdraught", 1),
            new RetiredMark("water.flowing", 3), new RetiredMark("water.unbroken", 3), new RetiredMark("water.spilled", 3), new RetiredMark("water.catch", 3),
            new RetiredMark("water.serpent", 4), new RetiredMark("water.rain", 4), new RetiredMark("water.moon", 4),
            new RetiredMark("water.spring", 6), new RetiredMark("water.river", 6),

            // Life: the plain minors and the dead "costs less" minor (Ready Soil), Paid in Blood and Heartwood (both rest on
            // spells costing mana; Feed the Ground carries health as the price now), the majors not kept (Thin Blood, which
            // was Dark's Near the Grave, replaced by The Garden Holds You, and Shade), Thorn Ring cut outright (Burrs took its
            // shape slot), and Shared Roots, a keystone not carried over.
            new RetiredMark("life.deeproots", 1), new RetiredMark("life.sweetsap", 1), new RetiredMark("life.stubborn", 1), new RetiredMark("life.quickening", 1),
            new RetiredMark("life.season", 1), new RetiredMark("life.sharp", 1), new RetiredMark("life.crowded", 1), new RetiredMark("life.spreading", 1),
            new RetiredMark("life.soil", 1), new RetiredMark("life.deepseed", 1), new RetiredMark("life.wideburst", 1), new RetiredMark("life.bark", 1),
            new RetiredMark("life.longgrowth", 1), new RetiredMark("life.greenwood", 1),
            new RetiredMark("life.price", 3), new RetiredMark("life.thin", 3), new RetiredMark("life.shade", 3),
            new RetiredMark("life.ring", 4),
            new RetiredMark("life.heartwood", 6), new RetiredMark("life.roots", 6),
        };

        // NEEDS: the owner's word on whether a Shards-bought mark that moves to the Way (paid in Memories) is refunded then.
        private static readonly RetiredMark[] moving =
        {
            new RetiredMark(Upgrade.BurningCrescent, 3),
            new RetiredMark(Upgrade.DelayedCuts, 3),
            // Wind's Drawing Wind, the Iaido Slash's pull.
            new RetiredMark("wind.drawing", 3),
            // Ice's Winter Draw, the Iaido Slash's lane of ice.
            new RetiredMark("ice.winterdraw", 3),
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
