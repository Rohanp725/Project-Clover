using Clover.Core;
using Clover.Player;
using Clover.Player.Growth;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The wallet (IMPLEMENTATION.md 3.12, task 5.1): the two currencies kept apart, a mark paid and refunded in its own,
    /// a given mark free, the playtest's rest topping both up and nothing given outside it, and a death taking only the
    /// carried Shards, never a Memory.
    /// </summary>
    public class WalletTests
    {
        private bool wasOn;

        [SetUp]
        public void Remember() => wasOn = PlaytestMode.On;

        [TearDown]
        public void PutBack() => PlaytestMode.Set(wasOn);

        private static readonly Upgrade ShardMark = new Upgrade { Id = "t.shard", Kind = UpgradeKind.Major, Cost = 3 };
        private static readonly Upgrade MemoryMark = new Upgrade { Id = "t.memory", Kind = UpgradeKind.Major, Tree = MarkTree.Way, Currency = Currency.Memories, Cost = 2 };
        private static readonly Upgrade GivenMark = new Upgrade { Id = "t.given", Kind = UpgradeKind.Aspect, Tree = MarkTree.Way, Currency = Currency.None };

        [Test]
        public void EarningAndSpendingKeepEachCurrencyApart()
        {
            var wallet = new Wallet();
            wallet.Earn(Currency.Shards, 5);
            wallet.Earn(Currency.Memories, 3);
            wallet.Earn(Currency.None, 7);
            wallet.Earn(Currency.Shards, -4);
            Assert.AreEqual(5, wallet.Of(Currency.Shards));
            Assert.AreEqual(3, wallet.Of(Currency.Memories));
            Assert.AreEqual(0, wallet.Of(Currency.None));
            Assert.IsFalse(wallet.Spend(Currency.Memories, 4), "only three");
            Assert.AreEqual(3, wallet.Memories, "nothing taken");
            Assert.IsTrue(wallet.Spend(Currency.Memories, 3));
            Assert.AreEqual(0, wallet.Memories);
            Assert.AreEqual(5, wallet.Shards, "Shards untouched");
            Assert.IsFalse(wallet.Spend(Currency.None, 0));
            Assert.IsFalse(wallet.Spend(Currency.Shards, -1));
        }

        [Test]
        public void AMarkIsPaidAndRefundedInItsOwnCurrencyAndAGivenOneIsFree()
        {
            var wallet = new Wallet { Shards = 2, Memories = 9 };
            Assert.IsFalse(wallet.CanAfford(ShardMark), "three Shards, two held, however many Memories");
            Assert.IsFalse(wallet.Pay(ShardMark));
            Assert.IsTrue(wallet.CanAfford(MemoryMark));
            Assert.IsTrue(wallet.Pay(MemoryMark));
            Assert.AreEqual(7, wallet.Memories);
            Assert.AreEqual(2, wallet.Shards);
            wallet.Refund(MemoryMark);
            Assert.AreEqual(9, wallet.Memories);
            Assert.IsTrue(wallet.CanAfford(GivenMark));
            Assert.IsTrue(wallet.Pay(GivenMark));
            wallet.Refund(GivenMark);
            Assert.AreEqual(9, wallet.Memories);
            Assert.AreEqual(2, wallet.Shards);
            Assert.IsFalse(wallet.CanAfford(null));
        }

        [Test]
        public void ARestTopsBothUpInThePlaytestAndGivesNothingOutsideIt()
        {
            PlaytestMode.Set(false);
            var wallet = new Wallet { Shards = 3, Memories = 4 };
            wallet.Rested();
            Assert.AreEqual(3, wallet.Shards);
            Assert.AreEqual(4, wallet.Memories);
            Assert.AreEqual(4, PlaytestMode.MemoriesAfterRest(4));
            PlaytestMode.Set(true);
            wallet.Rested();
            Assert.AreEqual(PlaytestMode.RestShards, wallet.Shards);
            Assert.AreEqual(PlaytestMode.RestMemories, wallet.Memories);
            wallet.Memories = PlaytestMode.RestMemories + 5;
            wallet.Rested();
            Assert.AreEqual(PlaytestMode.RestMemories + 5, wallet.Memories, "a rest never takes away");
        }

        [Test]
        public void ARestInThePlaytestPaysForEveryWayMarkAtOnce()
        {
            int whole = 0;
            foreach (var mark in Upgrade.All)
                if (mark.Currency == Currency.Memories) whole += mark.Cost;
            Assert.Greater(PlaytestMode.RestMemories, whole);
        }

        [Test]
        public void ADeathTakesOnlyTheCarriedShards()
        {
            var wallet = new Wallet { Shards = 12, Memories = 30, ShardsBanked = 8 };
            Assert.AreEqual(12, wallet.TakeCarriedShards());
            Assert.AreEqual(0, wallet.Shards);
            Assert.AreEqual(30, wallet.Memories, "Memories are kept through death");
            Assert.AreEqual(8, wallet.ShardsBanked, "and so is what lies banked in a fire");
            Assert.AreEqual(0, wallet.TakeCarriedShards(), "a second death finds nothing");
        }

        [Test]
        public void TheWalletIsWrittenWholeIntoASave()
        {
            var wallet = new Wallet { Shards = 1, Memories = 2, ShardsBanked = 3 };
            var back = JsonUtility.FromJson<Wallet>(JsonUtility.ToJson(wallet));
            Assert.AreEqual(1, back.Shards);
            Assert.AreEqual(2, back.Memories);
            Assert.AreEqual(3, back.ShardsBanked);
        }
    }
}
