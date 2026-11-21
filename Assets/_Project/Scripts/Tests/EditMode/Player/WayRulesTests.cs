using System.Collections.Generic;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Growth;
using Clover.Player.Trees;
using Clover.Player.Way;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Way's rules (IMPLEMENTATION.md 2.5 and 3.6, task 5.1), on marks made here so they hold whatever the Katana's
    /// Way says: a trunk mark with its weapon kind, a god's weapon counting as its kind; a bough's marks only while it is
    /// the active aspect, asleep but kept otherwise; one keystone of two in force, both paths' marks either way; an
    /// element infusion mark with its element; the Body always, one bearing; gates by flag, by gods felled and open in the
    /// playtest; Memories paid from Memories only; what is spent on one Way; and the temper ranks.
    /// </summary>
    public class WayRulesTests
    {
        private const string Katana = Weapons.KatanaName;
        private const string Sheath = "t.sheath";
        private const string Stream = "t.stream";

        private static readonly Upgrade Temper = new Upgrade { Id = "t.temper1", Name = "Temper I", Kind = UpgradeKind.Temper, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Cost = 1 };
        private static readonly Upgrade Temper2 = new Upgrade { Id = "t.temper2", Name = "Temper II", Kind = UpgradeKind.Temper, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Cost = 1, Requires = new[] { "t.temper1" } };
        private static readonly Upgrade Crescent = new Upgrade { Id = "t.crescent", Kind = UpgradeKind.Major, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Element = Elements.Fire, Cost = 2 };
        private static readonly Upgrade SheathAspect = new Upgrade { Id = Sheath, Kind = UpgradeKind.Aspect, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Aspect = Sheath, Cost = 2 };
        private static readonly Upgrade SheathStem = new Upgrade { Id = "t.sheath.sharp", Kind = UpgradeKind.Minor, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Aspect = Sheath, Cost = 1, Requires = new[] { Sheath } };
        private static readonly Upgrade SheathPathA = new Upgrade { Id = "t.sheath.held", Kind = UpgradeKind.Major, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Aspect = Sheath, Cost = 2, Requires = new[] { "t.sheath.sharp" } };
        private static readonly Upgrade SheathKeyA = new Upgrade { Id = "t.sheath.click", Kind = UpgradeKind.Keystone, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Aspect = Sheath, Cost = 4 };
        private static readonly Upgrade SheathKeyB = new Upgrade { Id = "t.sheath.shrine", Kind = UpgradeKind.Keystone, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Aspect = Sheath, Cost = 4 };
        private static readonly Upgrade StreamAspect = new Upgrade { Id = Stream, Kind = UpgradeKind.Aspect, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Aspect = Stream, Cost = 2 };
        private static readonly Upgrade StormStem = new Upgrade { Id = "t.storm.blade", Kind = UpgradeKind.Minor, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Aspect = WayRules.StormKatanaBough, Cost = 1 };
        private static readonly Upgrade StormAspect = new Upgrade { Id = WayRules.StormKatanaBough, Kind = UpgradeKind.Aspect, Tree = MarkTree.Way, Currency = Currency.None, Weapon = Katana, Aspect = WayRules.StormKatanaBough };
        private static readonly Upgrade Technique = new Upgrade { Id = "t.thunder", Kind = UpgradeKind.Technique, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Gate = "elite.t", Cost = 2 };
        private static readonly Upgrade SpearMark = new Upgrade { Id = "t.spear", Kind = UpgradeKind.Minor, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Weapons.GreatSpearName, Cost = 1 };
        private static readonly Upgrade Health = new Upgrade { Id = "t.health", Kind = UpgradeKind.Minor, Tree = MarkTree.Body, Currency = Currency.Memories, Cost = 2 };
        private static readonly Upgrade Enduring = new Upgrade { Id = "t.enduring", Kind = UpgradeKind.Keystone, Tree = MarkTree.Body, Currency = Currency.Memories, Cost = 4 };
        private static readonly Upgrade Light = new Upgrade { Id = "t.light", Kind = UpgradeKind.Keystone, Tree = MarkTree.Body, Currency = Currency.Memories, Cost = 4 };

        private static Loadout Carrying(string weapon, string first = "", string second = "")
        {
            var loadout = new Loadout { Weapon = weapon };
            loadout.Attune(first, second);
            return loadout;
        }

        private static readonly object[] Cases =
        {
            // mark, weapon, first element, aspect chosen, in force, why
            new object[] { Temper, Katana, "", "", true, "a trunk mark with its weapon" },
            new object[] { Temper, Weapons.StormKatanaName, "", "", true, "a trunk mark with the god's sword, which is a Katana" },
            new object[] { Temper, Weapons.GreatSpearName, "", "", false, "a trunk mark with another weapon" },
            new object[] { Temper, "", "", "", false, "a trunk mark with no weapon" },
            new object[] { SpearMark, Weapons.SunSpearName, "", "", true, "the Sun-Spear is a Great Spear" },
            new object[] { Crescent, Katana, Elements.Fire, "", true, "an element infusion mark with its element and weapon" },
            new object[] { Crescent, Katana, Elements.Ice, "", false, "an element infusion mark without its element" },
            new object[] { Crescent, Weapons.ScytheName, Elements.Fire, "", false, "an element infusion mark without its weapon" },
            new object[] { SheathStem, Katana, "", Sheath, true, "a bough's mark while it is the active aspect" },
            new object[] { SheathStem, Katana, "", Stream, false, "a bough's mark sleeps while another is active" },
            new object[] { SheathStem, Katana, "", "", false, "a bough's mark sleeps with no aspect chosen" },
            new object[] { SheathAspect, Katana, "", Sheath, true, "the aspect itself while active" },
            new object[] { StormStem, Weapons.StormKatanaName, "", "", true, "the god's bough while his sword is carried" },
            new object[] { StormStem, Weapons.StormKatanaName, "", Sheath, true, "his sword itself outranks a chosen plain aspect" },
            new object[] { StormStem, Katana, "", WayRules.StormKatanaBough, true, "the god's bough chosen on the plain Katana" },
            new object[] { SheathStem, Weapons.StormKatanaName, "", Sheath, false, "a plain bough sleeps while the god's sword is carried" },
            new object[] { Health, "", "", "", true, "a Body mark with no weapon at all" },
            new object[] { Health, Weapons.GreatHammerName, "", "", true, "a Body mark with any weapon" },
        };

        [TestCaseSource(nameof(Cases))]
        public void WayMarksAreInForceByTheirWeaponTheirBoughAndTheirElement(Upgrade mark, string weapon, string first, string aspect, bool expected, string why)
        {
            var loadout = Carrying(weapon, first);
            if (aspect != "") loadout.SetAspect(Katana, aspect);
            Assert.AreEqual(expected, MarkRules.InForce(mark, loadout), why);
            Assert.AreEqual(expected, WayRules.InForce(mark, loadout), why);
        }

        [Test]
        public void OnlyTheSetKeystoneOfABoughIsInForceAndAPathsMarksWorkUnderEither()
        {
            var loadout = Carrying(Katana);
            loadout.SetAspect(Katana, Sheath);
            Assert.IsFalse(MarkRules.InForce(SheathKeyA, loadout), "no keystone set");
            Assert.IsFalse(MarkRules.InForce(SheathKeyB, loadout));
            loadout.SetKeystone(Sheath, SheathKeyA.Id);
            Assert.IsTrue(MarkRules.InForce(SheathKeyA, loadout));
            Assert.IsFalse(MarkRules.InForce(SheathKeyB, loadout), "the other keystone waits");
            Assert.IsTrue(MarkRules.InForce(SheathPathA, loadout), "path A's mark under A's keystone");
            loadout.SetKeystone(Sheath, SheathKeyB.Id);
            Assert.IsTrue(MarkRules.InForce(SheathKeyB, loadout));
            Assert.IsFalse(MarkRules.InForce(SheathKeyA, loadout));
            Assert.IsTrue(MarkRules.InForce(SheathPathA, loadout), "path A's mark under B's keystone too (question G1)");
            loadout.SetAspect(Katana, Stream);
            Assert.IsFalse(MarkRules.InForce(SheathKeyB, loadout), "a set keystone sleeps with its bough");
            Assert.AreEqual(SheathKeyB.Id, loadout.KeystoneOf(Sheath), "and is still set when the bough wakes");
        }

        [Test]
        public void TheBodysBearingsExcludeEachOtherAndItsNumbersAlwaysCount()
        {
            var loadout = Carrying(Katana);
            Assert.IsTrue(MarkRules.InForce(Health, loadout));
            Assert.IsFalse(MarkRules.InForce(Enduring, loadout), "no bearing set");
            loadout.SetKeystone(WayRules.BodyBough, Enduring.Id);
            Assert.IsTrue(MarkRules.InForce(Enduring, loadout));
            Assert.IsFalse(MarkRules.InForce(Light, loadout));
        }

        [Test]
        public void ALearnedWayMarkIsInForceOnlyWhenBought()
        {
            var loadout = Carrying(Katana);
            var learned = new HashSet<string>();
            Assert.IsFalse(MarkRules.InForce(Temper, loadout, learned));
            learned.Add(Temper.Id);
            Assert.IsTrue(MarkRules.InForce(Temper, loadout, learned));
        }

        [Test]
        public void TheFistsTwoTechniquesTakeTheChordInTurn()
        {
            var first = new Upgrade { Id = "t.tail", Kind = UpgradeKind.Technique, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Weapons.FistsName };
            var second = new Upgrade { Id = "t.pounce", Kind = UpgradeKind.Technique, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Weapons.FistsName };
            var loadout = Carrying(Weapons.FistsName);
            Assert.IsTrue(MarkRules.InForce(first, loadout), "with none chosen, both wait on the chord");
            loadout.Technique = second.Id;
            Assert.IsFalse(MarkRules.InForce(first, loadout));
            Assert.IsTrue(MarkRules.InForce(second, loadout));
            Assert.IsTrue(MarkRules.InForce(Technique, Carrying(Katana)), "a weapon taught once ignores the Fists' choice");
            var katana = Carrying(Katana);
            katana.Technique = second.Id;
            Assert.IsTrue(MarkRules.InForce(Technique, katana), "a Fists' choice kept in the loadout never silences another weapon's technique");
        }

        [Test]
        public void TheKindOfAWeaponCountsGodsWeaponsAndTheArmament()
        {
            Assert.AreEqual(Katana, WayRules.KindOf(Weapons.StormKatanaName));
            Assert.AreEqual(Weapons.GreatSpearName, WayRules.KindOf(Weapons.SunSpearName));
            Assert.AreEqual(Weapons.FistsName, WayRules.KindOf(Weapons.ArmamentName), "the Armament is the Fists' third aspect");
            Assert.AreEqual(Weapons.ScytheName, WayRules.KindOf(Weapons.ScytheName));
            Assert.AreEqual("", WayRules.KindOf(null));
            Assert.AreEqual(WayRules.StormKatanaBough, WayRules.GodBoughOf(Weapons.StormKatanaName));
            Assert.IsNull(WayRules.GodBoughOf(Katana));
            Assert.IsTrue(WayRules.KindOwned(Katana, new[] { Weapons.StormKatanaName }), "owning the god's sword opens the Katana's Way");
            Assert.IsFalse(WayRules.KindOwned(Katana, new[] { Weapons.ScytheName }));
        }

        [Test]
        public void GatesOpenOnTheirFlagOrOnACountOfGodsAndAllInThePlaytest()
        {
            var flags = new HashSet<string> { "elite.t" };
            Assert.IsTrue(MarkRules.GateOpen("", null, 0, false), "no gate");
            Assert.IsTrue(MarkRules.GateOpen("elite.t", flags.Contains, 0, false));
            Assert.IsFalse(MarkRules.GateOpen("elite.other", flags.Contains, 0, false));
            Assert.IsFalse(MarkRules.GateOpen("elite.t", null, 0, false), "no flags to ask");
            Assert.IsFalse(MarkRules.GateOpen(MarkRules.GodsGate + "3", flags.Contains, 2, false), "two gods of three");
            Assert.IsTrue(MarkRules.GateOpen(MarkRules.GodsGate + "3", flags.Contains, 3, false));
            Assert.IsTrue(MarkRules.GateOpen("elite.other", null, 0, true), "the playtest opens every gate");
        }

        [Test]
        public void AWayMarkIsPaidInMemoriesWaitsBehindItsGateAndTheShardChartNeverSellsIt()
        {
            var learned = new HashSet<string>();
            var flags = new HashSet<string>();
            var wallet = new Wallet { Shards = 99, Memories = 1 };
            Assert.IsFalse(MarkRules.CanBuy(Technique, learned, wallet, true, false, flags.Contains, 0), "one Memory of two");
            wallet.Memories = 2;
            Assert.IsFalse(MarkRules.CanBuy(Technique, learned, wallet, true, false, flags.Contains, 0), "its teacher still stands");
            flags.Add("elite.t");
            Assert.IsTrue(MarkRules.CanBuy(Technique, learned, wallet, true, false, flags.Contains, 0));
            Assert.IsFalse(MarkRules.CanBuy(Technique, learned, 99, true, true), "Shards never buy a Way mark, in the playtest either");
            Assert.IsFalse(MarkRules.CanBuy(Technique, learned, wallet, false, false, flags.Contains, 0), "a Way not open (its weapon not owned)");
            // The bough's road still holds: the stem after the aspect.
            wallet.Memories = 10;
            Assert.IsFalse(MarkRules.CanBuy(SheathStem, learned, wallet, true, false, flags.Contains, 0));
            learned.Add(Sheath);
            Assert.IsTrue(MarkRules.CanBuy(SheathStem, learned, wallet, true, false, flags.Contains, 0));
            Assert.IsTrue(MarkRules.CanBuy(Temper2, new HashSet<string> { Temper.Id }, wallet, true, false, null, 0), "temper grows rank by rank");
            Assert.IsFalse(MarkRules.CanBuy(Temper2, new HashSet<string>(), wallet, true, false, null, 0));
        }

        [Test]
        public void TheChartsBuyAsksEachMarkInItsOwnCurrency()
        {
            var learned = new HashSet<string>();
            var shardMark = Upgrade.ById(FireTree.FireWithoutEnd);
            Assert.AreEqual(Currency.Shards, shardMark.Currency);
            var wallet = new Wallet { Shards = 0, Memories = 0 };
            // A Way mark: Memories from the wallet, however many Shards the run holds.
            Assert.IsFalse(MarkRules.CanBuyInItsCurrency(Temper, learned, 999, wallet, true, false, null, 0), "no Memories, whatever the Shards");
            wallet.Memories = Temper.Cost;
            Assert.IsTrue(MarkRules.CanBuyInItsCurrency(Temper, learned, 0, wallet, true, false, null, 0), "Memories buy it with no Shards held");
            Assert.IsTrue(wallet.Pay(Temper));
            Assert.AreEqual(0, wallet.Memories, "paid in Memories");
            Assert.IsFalse(MarkRules.CanBuyInItsCurrency(Temper, learned, 0, null, true, true, null, 0), "no wallet, no Memories mark");
            Assert.IsFalse(MarkRules.CanBuyInItsCurrency(Technique, learned, 0, new Wallet { Memories = 9 }, true, false, new HashSet<string>().Contains, 0), "its gate still holds");
            // An element mark: Shards, whatever the wallet's Memories.
            var full = new Wallet { Memories = 999 };
            Assert.AreEqual(MarkRules.CanBuy(shardMark, learned, 999, true, true), MarkRules.CanBuyInItsCurrency(shardMark, learned, 999, full, true, true, null, 0),
                "a Shards mark is asked as the Shards chart asks it");
            Assert.IsFalse(MarkRules.CanBuyInItsCurrency(shardMark, learned, 0, full, true, true, null, 0), "Memories never buy a Shards mark");
        }

        [Test]
        public void LearningTheFirstAspectOrKeystoneSwitchesItOnAndALaterOneWaits()
        {
            var learned = new HashSet<string>();
            var loadout = Carrying(Katana);
            Assert.IsTrue(MarkRules.Learn(SheathAspect, learned, loadout), "the first aspect bought is active at once");
            Assert.IsTrue(learned.Contains(Sheath));
            Assert.AreEqual(Sheath, loadout.AspectOf(Katana));
            Assert.IsFalse(MarkRules.Learn(StreamAspect, learned, loadout), "a second aspect waits for a fire");
            Assert.IsTrue(learned.Contains(Stream), "but is learned");
            Assert.AreEqual(Sheath, loadout.AspectOf(Katana));
            Assert.IsTrue(MarkRules.Learn(SheathKeyA, learned, loadout), "the first keystone of the bough is the one set");
            Assert.IsFalse(MarkRules.Learn(SheathKeyB, learned, loadout));
            Assert.AreEqual(SheathKeyA.Id, loadout.KeystoneOf(WayRules.BoughOf(SheathKeyA)));
            Assert.IsFalse(MarkRules.Learn(Temper, learned, loadout), "a trunk mark sets nothing");
            Assert.IsTrue(learned.Contains(Temper.Id));
            Assert.IsFalse(MarkRules.Learn(Temper, learned, loadout), "learned once");
            Assert.AreEqual(1, System.Linq.Enumerable.Count(learned, id => id == Temper.Id));
        }

        [Test]
        public void AGodsBoughIsGivenByTakeAndIsOnlyAPlaytestUnlock()
        {
            var learned = new HashSet<string>();
            var wallet = new Wallet();
            Assert.IsTrue(WayRules.IsGodBough(StormAspect));
            Assert.IsFalse(WayRules.IsGodBough(SheathAspect));
            Assert.IsFalse(MarkRules.CanBuy(StormAspect, learned, wallet, true, false, null, 0), "never sold in the game");
            Assert.IsTrue(MarkRules.CanBuy(StormAspect, learned, wallet, true, true, null, 0), "a playtest unlock, for nothing");
            var loadout = Carrying(Katana);
            Assert.IsFalse(WayRules.ChooseAspect(loadout, StormAspect, learned, false), "not taken");
            Assert.IsTrue(WayRules.ChooseAspect(loadout, StormAspect, learned, true), "the playtest's rack has his sword");
            Assert.AreEqual(WayRules.StormKatanaBough, WayRules.ActiveAspect(loadout, Katana));
        }

        [Test]
        public void TheFirstAspectAndKeystoneBoughtAreSetAndALaterOneIsNot()
        {
            var loadout = Carrying(Katana);
            Assert.IsTrue(WayRules.SettleBought(SheathAspect, loadout));
            Assert.AreEqual(Sheath, loadout.AspectOf(Katana));
            Assert.IsFalse(WayRules.SettleBought(StreamAspect, loadout), "a second aspect waits for a fire");
            Assert.AreEqual(Sheath, loadout.AspectOf(Katana));
            Assert.IsTrue(WayRules.SettleBought(SheathKeyB, loadout));
            Assert.IsFalse(WayRules.SettleBought(SheathKeyA, loadout));
            Assert.AreEqual(SheathKeyB.Id, loadout.KeystoneOf(Sheath));
            Assert.IsFalse(WayRules.SettleBought(SheathStem, loadout), "a stem mark sets nothing");
            Assert.IsTrue(WayRules.SettleBought(Light, loadout), "the Body's first bearing");
            Assert.AreEqual(Light.Id, loadout.KeystoneOf(WayRules.BodyBough));
        }

        [Test]
        public void AnAspectOrKeystoneIsChosenOnlyOnceLearnedAndOnlyOfItsOwnKind()
        {
            var loadout = Carrying(Katana);
            var learned = new HashSet<string> { Sheath, SheathKeyA.Id };
            Assert.IsFalse(WayRules.ChooseAspect(loadout, StreamAspect, learned, false), "not learned");
            Assert.IsTrue(WayRules.ChooseAspect(loadout, SheathAspect, learned, false));
            Assert.IsFalse(WayRules.ChooseAspect(loadout, SheathStem, learned, false), "a stem mark is no aspect");
            Assert.IsFalse(WayRules.ChooseKeystone(loadout, SheathKeyB, learned), "not learned");
            Assert.IsTrue(WayRules.ChooseKeystone(loadout, SheathKeyA, learned));
            Assert.IsFalse(WayRules.ChooseKeystone(loadout, SheathPathA, learned), "a path mark is no keystone");
            Assert.AreEqual(SheathKeyA.Id, loadout.KeystoneOf(Sheath));
        }

        [Test]
        public void SpentCountsOneWaysMemoriesSleepingBoughsIncludedAndOpensItsHeresyAtThirty()
        {
            var catalogue = new List<Upgrade> { Temper, Temper2, Crescent, SheathAspect, SheathStem, SheathKeyA, StreamAspect, SpearMark, StormAspect, Health };
            var learned = new HashSet<string> { Temper.Id, Temper2.Id, Crescent.Id, SheathAspect.Id, SheathStem.Id, SheathKeyA.Id, StreamAspect.Id, SpearMark.Id, StormAspect.Id, Health.Id };
            Assert.AreEqual(1 + 1 + 2 + 2 + 1 + 4 + 2, WayRules.Spent(Katana, learned, catalogue), "the Katana's own, a god's bough given free, no Body, no spear");
            Assert.AreEqual(1, WayRules.Spent(Weapons.GreatSpearName, learned, catalogue));
            Assert.IsFalse(WayRules.WeaponHeresyOpen(Katana, learned, catalogue));
            var dear = new Upgrade { Id = "t.dear", Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = Katana, Cost = WayRules.WeaponHeresyAt - 13 };
            catalogue.Add(dear);
            learned.Add(dear.Id);
            Assert.IsTrue(WayRules.WeaponHeresyOpen(Katana, learned, catalogue));
        }

        [Test]
        public void TemperRankCountsTheWeaponsLearnedRanks()
        {
            var catalogue = new List<Upgrade> { Temper, Temper2, SpearMark };
            Assert.AreEqual(0, WayRules.TemperRank(Katana, new HashSet<string>(), catalogue));
            Assert.AreEqual(2, WayRules.TemperRank(Katana, new HashSet<string> { Temper.Id, Temper2.Id, SpearMark.Id }, catalogue));
            Assert.AreEqual(0, WayRules.TemperRank(Weapons.GreatSpearName, new HashSet<string> { Temper.Id }, catalogue));
        }

        [Test]
        public void TheWaysPricesAreInMemoriesByKind()
        {
            Assert.AreEqual(1, Upgrade.UsualWayCost(UpgradeKind.Minor), "a Number");
            Assert.AreEqual(2, Upgrade.UsualWayCost(UpgradeKind.Major), "a Move or a Rule");
            Assert.AreEqual(4, Upgrade.UsualWayCost(UpgradeKind.Keystone));
            Assert.AreEqual(2, Upgrade.UsualWayCost(UpgradeKind.Aspect));
            Assert.AreEqual(1, Upgrade.UsualWayCost(UpgradeKind.Temper));
            Assert.AreEqual(2, Upgrade.UsualWayCost(UpgradeKind.Technique));
            Assert.AreEqual(2, Upgrade.UsualCost(new Upgrade { Kind = UpgradeKind.Major, Currency = Currency.Memories }));
            Assert.AreEqual(3, Upgrade.UsualCost(new Upgrade { Kind = UpgradeKind.Major }), "the element trees' Major is 3 Shards");
            Assert.AreEqual(0, Upgrade.UsualCost(StormAspect), "given");
        }

        [Test]
        public void AWayMarkNamingAnElementIsOnNoElementsTree()
        {
            Assert.IsFalse(Crescent.BelongsTo(Elements.Fire));
            Assert.IsTrue(Crescent.IsWay);
            Assert.IsTrue(Health.IsWay);
            Assert.IsFalse(new Upgrade { Element = Elements.Fire }.IsWay);
        }

        [Test]
        public void TheAspectsAndKeystonesChosenTravelWithTheWeaponNotWithASavedHand()
        {
            var loadout = Carrying(Katana);
            loadout.SetAspect(Katana, Sheath);
            loadout.SetKeystone(Sheath, SheathKeyA.Id);
            var copy = loadout.Clone();
            Assert.AreEqual(Sheath, copy.AspectOf(Katana));
            Assert.AreEqual(SheathKeyA.Id, copy.KeystoneOf(Sheath));
            copy.SetAspect(Katana, Stream);
            Assert.AreEqual(Sheath, loadout.AspectOf(Katana), "the copy shares nothing");
            var hand = new Loadout();
            hand.CopyHandFrom(loadout);
            Assert.AreEqual("", hand.AspectOf(Katana), "a saved Hand never carries the weapon's choices");
            loadout.SetAspect(Katana, "");
            Assert.AreEqual("", loadout.AspectOf(Katana), "taken away");
            Assert.AreEqual(1, loadout.Keystones.Count);
        }
    }
}
