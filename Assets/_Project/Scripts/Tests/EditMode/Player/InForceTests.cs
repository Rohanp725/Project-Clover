using Clover.Player;
using Clover.Player.Build;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// What is in force under a loadout (IMPLEMENTATION.md 3.6), table-driven: an element's marks while it is attuned,
    /// first or second; a duo branch's marks with both elements; a duo spell only in a slot; a weapon's marks with that
    /// weapon kind, a god's weapon counting as its kind; nothing with no element. The rules for Devotion, spell
    /// branches, shapes, reactions, the Way, Heresies, Bond and Evolutions are added with their fields (phases 3 to 6).
    /// </summary>
    public class InForceTests
    {
        private static readonly Upgrade FireMark = new Upgrade { Id = "t.fire", Element = Elements.Fire, Kind = UpgradeKind.Major };
        private static readonly Upgrade FireGift = new Upgrade { Id = "t.gift", Element = Elements.Fire, Kind = UpgradeKind.Signature };
        private static readonly Upgrade LightningSpell = new Upgrade { Id = "t.bolt", Element = Elements.Lightning, Kind = UpgradeKind.Spell, Form = "bolt", Spell = new SpellSpec() };
        private static readonly Upgrade PlasmaMark = new Upgrade { Id = "t.plasma", Element = Elements.Fire, Element2 = Elements.Lightning, Kind = UpgradeKind.Mix };
        private static readonly Upgrade PlasmaSpell = new Upgrade { Id = "t.ball", Element = Elements.Fire, Element2 = Elements.Lightning, Kind = UpgradeKind.Mix, Form = "ball", Spell = new SpellSpec() };
        private static readonly Upgrade KatanaFireMark = new Upgrade { Id = "t.crescent", Element = Elements.Fire, Kind = UpgradeKind.Major, Weapon = Weapons.KatanaName };

        private static Loadout Hand(string first, string second, string weapon = "", string slotted = "")
        {
            var loadout = new Loadout { Weapon = weapon };
            loadout.Attune(first, second);
            if (slotted != "") loadout.Slots[3] = slotted;
            return loadout;
        }

        private static readonly object[] Cases =
        {
            // mark, first, second, weapon, slotted, in force, why
            new object[] { FireMark, Elements.Fire, "", "", "", true, "its element first" },
            new object[] { FireMark, Elements.Lightning, Elements.Fire, "", "", true, "its element second" },
            new object[] { FireMark, Elements.Fire, Elements.Fire, "", "", true, "all in" },
            new object[] { FireMark, Elements.Ice, Elements.Lightning, "", "", false, "its element not carried" },
            new object[] { FireMark, "", "", "", "", false, "no element at all" },
            new object[] { FireGift, Elements.Lightning, Elements.Fire, "", "", true, "a god's gift is its element's mark" },
            new object[] { LightningSpell, Elements.Fire, Elements.Lightning, "", "", true, "an element's spell need not be slotted to be in force" },
            new object[] { LightningSpell, Elements.Fire, "", "", "", false, "an element's spell with its element not carried" },
            new object[] { PlasmaMark, Elements.Fire, Elements.Lightning, "", "", true, "a duo mark with both" },
            new object[] { PlasmaMark, Elements.Lightning, Elements.Fire, "", "", true, "a duo mark with both, either order" },
            new object[] { PlasmaMark, Elements.Fire, "", "", "", false, "a duo mark with one" },
            new object[] { PlasmaMark, Elements.Fire, Elements.Fire, "", "", false, "a duo mark all in" },
            new object[] { PlasmaSpell, Elements.Fire, Elements.Lightning, "", "t.ball", true, "a duo spell with both, in a slot" },
            new object[] { PlasmaSpell, Elements.Fire, Elements.Lightning, "", "", false, "a duo spell not in a slot" },
            new object[] { PlasmaSpell, Elements.Fire, "", "", "t.ball", false, "a duo spell in a slot with one element" },
            new object[] { KatanaFireMark, Elements.Fire, "", Weapons.KatanaName, "", true, "a weapon's mark with that weapon" },
            new object[] { KatanaFireMark, Elements.Fire, "", Weapons.StormKatanaName, "", true, "the god's sword counts as the Katana" },
            new object[] { KatanaFireMark, Elements.Fire, "", Weapons.GreatSpearName, "", false, "a weapon's mark with another weapon" },
            new object[] { KatanaFireMark, Elements.Lightning, "", Weapons.KatanaName, "", false, "a weapon's mark with its element not carried" },
        };

        [TestCaseSource(nameof(Cases))]
        public void InForceFollowsTheLoadout(Upgrade mark, string first, string second, string weapon, string slotted, bool expected, string why)
        {
            Assert.AreEqual(expected, MarkRules.InForce(mark, Hand(first, second, weapon, slotted)), why);
        }

        [Test]
        public void NothingIsInForceWithoutAMarkOrALoadout()
        {
            Assert.IsFalse(MarkRules.InForce(null, Hand(Elements.Fire, "")));
            Assert.IsFalse(MarkRules.InForce(FireMark, null));
        }

        [Test]
        public void CarriedIgnoresTheSlotButNotThePair()
        {
            Assert.IsTrue(MarkRules.Carried(PlasmaSpell, Hand(Elements.Fire, Elements.Lightning)), "a duo spell can be offered before it is slotted");
            Assert.IsFalse(MarkRules.Carried(PlasmaSpell, Hand(Elements.Fire, Elements.Ice)));
        }

        [Test]
        public void AllInAndMixedReadTheAttunements()
        {
            Assert.IsTrue(Hand(Elements.Ice, Elements.Ice).AllIn);
            Assert.IsFalse(Hand(Elements.Ice, Elements.Ice).Mixed);
            Assert.IsTrue(Hand(Elements.Ice, Elements.Water).Mixed);
            Assert.IsFalse(Hand(Elements.Ice, "").AllIn);
            Assert.IsFalse(Hand("", "").AllIn, "two empty attunements are not all in");
        }
    }
}
