using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Growth;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// Memories income (way_plan.md 0.3, task 5.6): the first defeat of each kind pays 1 and no later one; an elite 4, a
    /// named fighter 2, a god 8 once at its fall (never through its bodies), the Teacher 4, the last champion 12; found
    /// Memories 3 or 5 once each; nothing paid twice, so Memories cannot be farmed; and Shards per kill by rank and tier.
    /// </summary>
    public class IncomeTests
    {
        [Test]
        public void TheFirstDefeatOfAKindPaysOneAndNoLaterDefeatPaysAgain()
        {
            var income = new Income();
            var paid = new List<string>();
            var soldier = EnemyKind.Soldier();
            Assert.AreEqual(1, income.Defeat(soldier, paid));
            Assert.AreEqual(0, income.Defeat(EnemyKind.Soldier(), paid), "another soldier, after a rest or in another land");
            Assert.AreEqual(1, income.Defeat(EnemyKind.Acolyte(), paid), "a new kind");
            CollectionAssert.AreEquivalent(new[] { soldier.Name, EnemyKind.Acolyte().Name }, paid, "kept under each kind's name");
            Assert.AreEqual(0, income.Defeat((EnemyKind)null, paid));
            Assert.AreEqual(0, income.Defeat("", DefeatTier.Elite, paid), "no key, nothing paid");
        }

        [Test]
        public void EachRankOfTheGildedIsAKindOfItsOwn()
        {
            var income = new Income();
            var paid = new List<string>();
            Assert.AreEqual(1, income.Defeat(EnemyKind.Gilded(1), paid));
            Assert.AreEqual(1, income.Defeat(EnemyKind.Gilded(2), paid));
            Assert.AreEqual(1, income.Defeat(EnemyKind.Gilded(3), paid));
            Assert.AreEqual(0, income.Defeat(EnemyKind.Gilded(3), paid));
        }

        [Test]
        public void AnElitePaysFourOnceAsWhatItIsNotAlsoAsANewKind()
        {
            var income = new Income();
            var paid = new List<string>();
            Assert.AreEqual(DefeatTier.Elite, Income.TierOf(EnemyKind.Captain()));
            Assert.AreEqual(DefeatTier.Elite, Income.TierOf(EnemyKind.Champion()));
            Assert.AreEqual(4, income.Defeat(EnemyKind.Champion(), paid));
            Assert.AreEqual(0, income.Defeat(EnemyKind.Champion(), paid), "the Champion again after a rest");
            Assert.AreEqual(DefeatTier.Common, Income.TierOf(EnemyKind.Soldier()));
        }

        [Test]
        public void AGodPaysEightOnceWhenItFallsAndNothingThroughItsBodies()
        {
            var income = new Income();
            var paid = new List<string>();
            Assert.AreEqual(DefeatTier.God, Income.TierOf(EnemyKind.Narukami()));
            Assert.AreEqual(DefeatTier.God, Income.TierOf(EnemyKind.SunKing()));
            Assert.AreEqual(0, income.Defeat(EnemyKind.SunKing(), paid), "Khepharu as a man is one of his bodies");
            Assert.AreEqual(8, income.GodFelled(Elements.Fire, paid));
            Assert.AreEqual(0, income.GodFelled(Elements.Fire, paid), "his fight again");
            Assert.AreEqual(SlotSchedule.FireGodFlag, Income.GodKey(Elements.Fire), "paid under the flag his fall sets");
            Assert.AreEqual(8, income.GodFelled(Elements.Lightning, paid));
            Assert.AreEqual(0, income.GodFelled("", paid));
        }

        [Test]
        public void TheOtherRanksPayAsTheOwnersTableSays()
        {
            var income = new Income();
            Assert.AreEqual(1, income.MemoriesFor(DefeatTier.Common));
            Assert.AreEqual(2, income.MemoriesFor(DefeatTier.Named));
            Assert.AreEqual(4, income.MemoriesFor(DefeatTier.Elite));
            Assert.AreEqual(8, income.MemoriesFor(DefeatTier.God));
            Assert.AreEqual(4, income.MemoriesFor(DefeatTier.Teacher));
            Assert.AreEqual(12, income.MemoriesFor(DefeatTier.LastChampion));
            var paid = new List<string>();
            Assert.AreEqual(2, income.Defeat("named.t", DefeatTier.Named, paid));
            Assert.AreEqual(0, income.Defeat("named.t", DefeatTier.Named, paid));
        }

        [Test]
        public void FoundMemoriesPayThreeInALandOfOneAndFiveInALandOfTwoOnceEach()
        {
            var income = new Income();
            var found = new List<string>();
            Assert.AreEqual(3, income.Found("memory.fire.1", false, found));
            Assert.AreEqual(0, income.Found("memory.fire.1", false, found), "picked up already");
            Assert.AreEqual(5, income.Found("memory.storm.1", true, found));
            Assert.AreEqual(0, income.Found(null, true, found));
        }

        [Test]
        public void TheFireLandPaysTwentyOneAsTheTableSays()
        {
            // way_plan.md 0.3: god 8, one elite 4, four new kinds, one named fighter 2, found 3.
            var income = new Income();
            var paid = new List<string>();
            int total = income.GodFelled(Elements.Fire, paid) + income.Defeat(EnemyKind.Champion(), paid);
            foreach (var kind in new[] { EnemyKind.Gilded(1), EnemyKind.Gilded(2), EnemyKind.Gilded(3), EnemyKind.SunBlind() })
                total += income.Defeat(kind, paid);
            total += income.Defeat("named.fire", DefeatTier.Named, paid);
            total += income.Found("memory.fire", false, new List<string>());
            Assert.AreEqual(21, total);
        }

        [Test]
        public void ShardsPerKillRiseFromTierThree()
        {
            var income = new Income();
            Assert.AreEqual(1, income.ShardsFor(DefeatTier.Common, 1));
            Assert.AreEqual(3, income.ShardsFor(DefeatTier.Elite, 2));
            Assert.AreEqual(5, income.ShardsFor(DefeatTier.God, 1));
            Assert.AreEqual(2, income.ShardsFor(DefeatTier.Common, 3));
            Assert.AreEqual(5, income.ShardsFor(DefeatTier.Elite, 4));
            Assert.AreEqual(8, income.ShardsFor(DefeatTier.God, 3));
        }
    }
}
