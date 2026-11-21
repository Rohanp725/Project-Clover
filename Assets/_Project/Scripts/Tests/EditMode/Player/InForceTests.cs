using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Magic;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// What is in force under a loadout (IMPLEMENTATION.md 3.6), table-driven: an element's marks while it is attuned,
    /// first or second; a duo branch's marks with both elements; a duo spell only in a slot; a weapon's marks with that
    /// weapon kind, a god's weapon counting as its kind; nothing with no element; a spell branch's Empower, Shape and
    /// Master only while its spell is slotted; a Devotion only bought and all in on its element, where L2 with R2 is the
    /// deep strike (Phase 4, task 4.4). The rules for reactions, the Way,
    /// Heresies, Bond and Evolutions are added with their fields (phases 3 to 6).
    /// </summary>
    public class InForceTests
    {
        private static readonly Upgrade FireMark = new Upgrade { Id = "t.fire", Element = Elements.Fire, Kind = UpgradeKind.Major };
        private static readonly Upgrade FireGift = new Upgrade { Id = "t.gift", Element = Elements.Fire, Kind = UpgradeKind.Signature };
        private static readonly Upgrade LightningSpell = new Upgrade { Id = "t.bolt", Element = Elements.Lightning, Kind = UpgradeKind.Spell, Form = "bolt", Spell = new SpellSpec() };
        private static readonly Upgrade PlasmaMark = new Upgrade { Id = "t.plasma", Element = Elements.Fire, Element2 = Elements.Lightning, Kind = UpgradeKind.Mix };
        private static readonly Upgrade PlasmaSpell = new Upgrade { Id = "t.ball", Element = Elements.Fire, Element2 = Elements.Lightning, Kind = UpgradeKind.Mix, Form = "ball", Spell = new SpellSpec() };
        private static readonly Upgrade KatanaFireMark = new Upgrade { Id = "t.crescent", Element = Elements.Fire, Kind = UpgradeKind.Major, Weapon = Weapons.KatanaName };
        // A spell branch's marks (Phase 4) and an all-in passive.
        private static readonly Upgrade RushLearn = new Upgrade { Id = "t.rush", Element = Elements.Fire, Kind = UpgradeKind.Spell, Part = BranchPart.Learn, Branch = "t.rush", Form = "drill", Spell = new SpellSpec() };
        private static readonly Upgrade RushEmpower = new Upgrade { Id = "t.rush.empower", Element = Elements.Fire, Kind = UpgradeKind.Minor, Part = BranchPart.Empower, Branch = "t.rush" };
        private static readonly Upgrade RushShape = new Upgrade { Id = "t.rush.shape", Element = Elements.Fire, Kind = UpgradeKind.Major, Part = BranchPart.Shape, Branch = "t.rush" };
        private static readonly Upgrade RushMaster = new Upgrade { Id = "t.rush.master", Element = Elements.Fire, Kind = UpgradeKind.Major, Part = BranchPart.Master, Branch = "t.rush" };
        private static readonly Upgrade BaseMaster = new Upgrade { Id = "fire.base.master", Element = Elements.Fire, Kind = UpgradeKind.Major, Part = BranchPart.Master, Branch = "fire.base" };
        private static readonly Upgrade FireDevotion = new Upgrade { Id = "t.devotion", Element = Elements.Fire, Kind = UpgradeKind.Devotion };
        private static readonly Upgrade SpareSpell = new Upgrade { Id = "t.lance", Element = Elements.Fire, Element2 = Elements.Lightning, Kind = UpgradeKind.Mix, Tree = MarkTree.Hidden, Form = "bolt", Spell = new SpellSpec() };

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
            new object[] { RushLearn, Elements.Fire, "", "", "", true, "a branch's Learn need not be slotted" },
            new object[] { RushEmpower, Elements.Fire, "", "", "t.rush", true, "a branch's Empower with its spell slotted" },
            new object[] { RushEmpower, Elements.Fire, "", "", "", false, "a branch's Empower with its spell in no slot" },
            new object[] { RushShape, Elements.Lightning, Elements.Fire, "", "t.rush", true, "a branch's Shape with its spell slotted, its element second" },
            new object[] { RushShape, Elements.Fire, "", "", "t.other", false, "a branch's Shape with another spell slotted" },
            new object[] { RushMaster, Elements.Fire, "", "", "t.rush", true, "a branch's Master with its spell slotted" },
            new object[] { RushMaster, Elements.Ice, "", "", "t.rush", false, "a branch's Master with its element not carried" },
            new object[] { BaseMaster, Elements.Fire, "", "", "fire.base", true, "a base spell's Master with the base spell slotted" },
            new object[] { BaseMaster, Elements.Fire, "", "", "", false, "a base spell's Master with the base spell in no slot" },
            new object[] { FireDevotion, Elements.Fire, Elements.Fire, "", "", true, "a Devotion all in" },
            new object[] { FireDevotion, Elements.Fire, "", "", "", false, "a Devotion with its element once" },
            new object[] { FireDevotion, Elements.Fire, Elements.Lightning, "", "", false, "a Devotion in a mix" },
            new object[] { FireDevotion, Elements.Lightning, Elements.Lightning, "", "", false, "a Devotion all in on another element" },
            new object[] { SpareSpell, Elements.Fire, Elements.Lightning, "", "t.lance", false, "a hidden spare spell, even with both carried and slotted" },
        };

        [TestCaseSource(nameof(Cases))]
        public void InForceFollowsTheLoadout(Upgrade mark, string first, string second, string weapon, string slotted, bool expected, string why)
        {
            Assert.AreEqual(expected, MarkRules.InForce(mark, Hand(first, second, weapon, slotted)), why);
        }

        // All in (task 4.4): the real Devotions of the two trees probed first, bought or not, under each way of carrying.
        private static readonly object[] AllInCases =
        {
            // Devotion, first, second, bought, in force, why
            new object[] { FireTree.FireWithoutEnd, Elements.Fire, Elements.Fire, true, true, "bought, all in on Fire" },
            new object[] { FireTree.FireWithoutEnd, Elements.Fire, Elements.Fire, false, false, "all in on Fire, not bought" },
            new object[] { FireTree.FireWithoutEnd, Elements.Fire, "", true, false, "bought, Fire carried once" },
            new object[] { FireTree.FireWithoutEnd, Elements.Fire, Elements.Lightning, true, false, "bought, Fire first in a mix" },
            new object[] { FireTree.FireWithoutEnd, Elements.Lightning, Elements.Fire, true, false, "bought, Fire second in a mix" },
            new object[] { FireTree.FireWithoutEnd, Elements.Lightning, Elements.Lightning, true, false, "bought, all in on Lightning" },
            new object[] { FireTree.FireWithoutEnd, "", "", true, false, "bought, nothing carried" },
            new object[] { LightningTree.InOneBreath, Elements.Lightning, Elements.Lightning, true, true, "bought, all in on Lightning" },
            new object[] { LightningTree.InOneBreath, Elements.Lightning, Elements.Lightning, false, false, "all in on Lightning, not bought" },
            new object[] { LightningTree.InOneBreath, Elements.Lightning, "", true, false, "bought, Lightning carried once" },
            new object[] { LightningTree.InOneBreath, Elements.Fire, Elements.Lightning, true, false, "bought, Lightning second in a mix" },
            new object[] { LightningTree.InOneBreath, Elements.Fire, Elements.Fire, true, false, "bought, all in on Fire" },
        };

        [TestCaseSource(nameof(AllInCases))]
        public void ADevotionIsInForceOnlyBoughtAndAllInOnItsElement(string id, string first, string second, bool bought, bool expected, string why)
        {
            var mark = Upgrade.ById(id);
            Assert.IsNotNull(mark, id);
            Assert.AreEqual(UpgradeKind.Devotion, mark.Kind, id);
            var learned = new HashSet<string>();
            if (bought) learned.Add(id);
            Assert.AreEqual(expected, MarkRules.InForce(mark, Hand(first, second), learned), why);
        }

        [Test]
        public void EveryElementsOneDevotionIsInForceAllInOnItAndOnNoOther()
        {
            foreach (var element in Elements.All)
            {
                var devotions = Upgrade.All.Where(mark => mark.Kind == UpgradeKind.Devotion && mark.Element == element.Name).ToArray();
                Assert.AreEqual(1, devotions.Length, element.Name + " has one Devotion");
                var devotion = devotions[0];
                var learned = new HashSet<string> { devotion.Id };
                Assert.IsTrue(MarkRules.InForce(devotion, Hand(element.Name, element.Name), learned), devotion.Id + " all in");
                Assert.IsFalse(MarkRules.InForce(devotion, Hand(element.Name, ""), learned), devotion.Id + " with its element once");
                foreach (var other in Elements.All)
                {
                    if (other == element) continue;
                    Assert.IsFalse(MarkRules.InForce(devotion, Hand(other.Name, other.Name), learned), devotion.Id + " all in on " + other.Name);
                    Assert.IsFalse(MarkRules.InForce(devotion, Hand(element.Name, other.Name), learned), devotion.Id + " mixed with " + other.Name);
                }
            }
        }

        [Test]
        public void ALearnedMarkIsInForceOnlyWhenBought()
        {
            var learned = new HashSet<string>();
            Assert.IsFalse(MarkRules.InForce(FireMark, Hand(Elements.Fire, ""), learned), "not bought");
            learned.Add(FireMark.Id);
            Assert.IsTrue(MarkRules.InForce(FireMark, Hand(Elements.Fire, ""), learned));
            Assert.IsFalse(MarkRules.InForce(FireMark, Hand(Elements.Fire, ""), null), "nothing learned at all");
        }

        [Test]
        public void AllInTheSecondStrikeIsTheDeepStrike()
        {
            // L2 with R2: the deep strike all in (and with one element), once the element's Devotion is bought (the owner's
            // answer of 2026-11-20); the second element's strike in a mix.
            Assert.IsTrue(StrikeRules.IsDeep(true, Hand(Elements.Fire, Elements.Fire).Mixed, true), "all in on Fire with its Devotion: Burnt Offering");
            Assert.IsTrue(StrikeRules.IsDeep(true, Hand(Elements.Lightning, Elements.Lightning).Mixed, true), "all in on Lightning with its Devotion: Down the Channel");
            Assert.IsTrue(StrikeRules.IsDeep(true, Hand(Elements.Fire, "").Mixed, true), "one element carried, its Devotion bought");
            Assert.IsFalse(StrikeRules.IsDeep(true, Hand(Elements.Fire, Elements.Fire).Mixed, false), "all in without the Devotion: the plain infused strike");
            Assert.IsFalse(StrikeRules.IsDeep(true, Hand(Elements.Fire, "").Mixed, false), "one element without the Devotion: the plain infused strike");
            Assert.IsFalse(StrikeRules.IsDeep(true, Hand(Elements.Fire, Elements.Lightning).Mixed, true), "a mix: Lightning's Flash Cut");
            Assert.IsFalse(StrikeRules.IsDeep(false, Hand(Elements.Fire, Elements.Fire).Mixed, true), "R1 is never the deep strike");
            // Every element's Devotion is found by the strike.
            foreach (var element in Elements.All)
                Assert.AreEqual(UpgradeKind.Devotion, Upgrade.ById(StrikeRules.DevotionOf(element.Name))?.Kind, element.Name + "'s Devotion");
            Assert.AreEqual(FireTree.FireWithoutEnd, StrikeRules.DevotionOf(Elements.Fire));
            Assert.AreEqual("Burnt Offering", ElementSteel.StrikeName(Elements.Fire, true));
            Assert.AreEqual("Down the Channel", ElementSteel.StrikeName(Elements.Lightning, true));
            var rules = new StrikeRules();
            Assert.AreEqual(20f, rules.Price(true), "the deep strike costs 20 mana");
            Assert.AreEqual(0f, rules.Price(false));
            Assert.AreEqual(StrikeRefusal.NoMana, rules.Check(new SpellCooldowns(), true, true, 19f, true));
            Assert.AreEqual(StrikeRefusal.None, rules.Check(new SpellCooldowns(), true, true, 20f, true));
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

        [Test]
        public void WeaponBlowsLayOnlyTheFirstElementsAfflictionExceptFromAMixedBranch()
        {
            // The owner's rules: of 2026-11-18 (blows lay only the first element's affliction) and 2026-11-19 (a mix mark's blows lay both).
            Assert.IsTrue(MarkRules.BlowMayLay(FireMark, UpgradeTrigger.AnyHit, AfflictionKind.Burn, AfflictionKind.Burn), "the first element's own affliction");
            Assert.IsFalse(MarkRules.BlowMayLay(FireMark, UpgradeTrigger.AnyHit, AfflictionKind.Burn, AfflictionKind.Static), "Fire carried second lays no Burn with the weapon");
            Assert.IsFalse(MarkRules.BlowMayLay(FireMark, UpgradeTrigger.HeavyHit, AfflictionKind.Burn, AfflictionKind.Static), "nor with a heavy blow");
            Assert.IsFalse(MarkRules.BlowMayLay(FireGift, UpgradeTrigger.ChainEnd, AfflictionKind.Burn, AfflictionKind.Static), "nor a god's gift's");
            Assert.IsTrue(MarkRules.BlowMayLay(FireMark, UpgradeTrigger.SpellHit, AfflictionKind.Burn, AfflictionKind.Static), "a spell lays the second element's");
            Assert.IsTrue(MarkRules.BlowMayLay(FireMark, UpgradeTrigger.PerfectGuard, AfflictionKind.Burn, AfflictionKind.Static), "a guard is not a blow");
            Assert.IsTrue(MarkRules.BlowMayLay(FireMark, UpgradeTrigger.AnyHit, AfflictionKind.None, AfflictionKind.Static), "laying nothing is never stopped");
            Assert.IsTrue(MarkRules.BlowMayLay(PlasmaMark, UpgradeTrigger.AnyHit, AfflictionKind.Burn, AfflictionKind.Burn), "a mix mark's first affliction");
            Assert.IsTrue(MarkRules.BlowMayLay(PlasmaMark, UpgradeTrigger.LightHit, AfflictionKind.Static, AfflictionKind.Burn), "a mix mark's second affliction too");
            Assert.IsTrue(MarkRules.BlowMayLay(PlasmaMark, UpgradeTrigger.AnyHit, AfflictionKind.Burn, AfflictionKind.Static), "either way round");
        }

        [Test]
        public void EveryMixedBranchsFirstMarkLaysBothAfflictionsWithTheWeapon()
        {
            // Each first mark that says its blows lay two afflictions, with either of its elements first. Plasma's is cut: a duo
            // branch reshaped into the heart lays both by carrying both (pairs_d1_d2.md); its first mark is the reaction's shapes.
            string[] firstMarks = { ForgeTree.HammerAndAnvil };
            foreach (string id in firstMarks)
            {
                var mark = Upgrade.ById(id);
                Assert.IsNotNull(mark, id);
                Assert.AreEqual(UpgradeKind.Mix, mark.Kind, id);
                var laid = mark.Effects.Where(effect => effect.Do == UpgradeAction.Afflict && MarkRules.IsBlow(effect.On)).ToArray();
                Assert.AreEqual(2, laid.Select(effect => effect.Affliction).Distinct().Count(), id + " lays two afflictions with its blows");
                foreach (var weapon in laid.Select(effect => effect.Affliction))
                    foreach (var effect in laid)
                        Assert.IsTrue(MarkRules.BlowMayLay(mark, effect.On, effect.Affliction, weapon), $"{id} lays {effect.Affliction} when the weapon lays {weapon}");
            }
        }
    }
}
