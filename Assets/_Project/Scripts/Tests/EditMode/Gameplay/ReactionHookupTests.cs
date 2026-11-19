using System.Collections.Generic;
using Clover.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The engine's hook-ups of task 3.2 (IMPLEMENTATION.md 3.7): a blow's burn goes through the one laying entry and sets
    /// reactions off like any other affliction; a blow a reaction dealt starts nothing; a forced reaction ignores the rest
    /// once; and stacks can be spent without a burst.
    /// </summary>
    public class ReactionHookupTests
    {
        private readonly List<int> reported = new List<int>();
        private int frame;
        private ReactionEngine engine;

        private static readonly int Steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
        private static readonly int Flashover = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);

        [SetUp]
        public void Begin()
        {
            reported.Clear();
            frame = 0;
            engine = new ReactionEngine((who, pair, laid) => reported.Add(pair), () => frame);
        }

        private AfflictionSet NewEnemy() => new AfflictionSet { Reactions = engine };

        [Test]
        public void ABlowsBurnGoesThroughTheLayingAndReacts()
        {
            var enemy = NewEnemy();
            enemy.Lay(AfflictionKind.Soaked, 1, 0f, 1f, LaySource.Spell);
            enemy.IgniteFrom(new Hit(8f, 5f, Vector3.forward, burnSeconds: 3f), null);
            Assert.IsTrue(enemy.Burn.IsBurning);
            CollectionAssert.AreEqual(new[] { Steam }, reported, "a weapon's burn on a Soaked enemy is Steam");

            var other = NewEnemy();
            other.Lay(AfflictionKind.Static, 1, 0f, 1f, LaySource.Spell);
            other.IgniteFrom(new Hit(8f, 5f, Vector3.forward, burnSeconds: 3f, fromReaction: true), null);
            Assert.IsTrue(other.Burn.IsBurning, "a reaction's burn still burns");
            Assert.AreEqual(1, reported.Count, "but it starts nothing");
            other.Ignite(3f, 1, LaySource.Effect);
            CollectionAssert.AreEqual(new[] { Steam, Flashover }, reported, "a mark's own fire may set one off");
        }

        [Test]
        public void WhatAReactionsBlowLaysStartsNothing()
        {
            var enemy = NewEnemy();
            enemy.Lay(AfflictionKind.Static, 2, 0f, 1f, LaySource.Spell);
            enemy.TakeFrom(new Hit(4f, 0f, Vector3.forward, affliction: AfflictionKind.Soaked, afflictionStacks: 1, fromReaction: true), null);
            Assert.IsTrue(enemy.Has(AfflictionKind.Soaked));
            Assert.AreEqual(0, reported.Count);
            enemy.TakeFrom(new Hit(4f, 0f, Vector3.forward, affliction: AfflictionKind.Chill, afflictionStacks: 1, spell: true), null);
            Assert.AreEqual(2, reported.Count, "a spell's laying of Chill finds both Static and Soaked");
        }

        [Test]
        public void AForcedReactionIgnoresTheRestOnceAndNeedsBothPartners()
        {
            var enemy = NewEnemy();
            enemy.Lay(AfflictionKind.Static, 1, 0f, 1f, LaySource.Spell);
            Assert.IsFalse(enemy.ForceReaction(Flashover, null), "without Burn there is nothing to force");
            enemy.Lay(AfflictionKind.Burn, 1, 0f, 1f, LaySource.Weapon);
            Assert.AreEqual(1, reported.Count);
            Assert.Greater(enemy.ReactionRest(Flashover), 0f);
            Assert.IsTrue(enemy.ForceReaction(Flashover, null));
            Assert.AreEqual(2, reported.Count, "forced through the rest");
            Assert.AreEqual(engine.RestFor(Flashover), enemy.ReactionRest(Flashover), 0.001f, "and it rests again from now");
        }

        [Test]
        public void StacksAreSpentWithoutABurstAndAHalfIsRoundedUp()
        {
            var enemy = NewEnemy();
            enemy.Lay(AfflictionKind.Burn, 3, 4f, 1f, LaySource.Spell);
            Assert.AreEqual(3, enemy.Stacks(AfflictionKind.Burn));
            Assert.AreEqual(2, enemy.Spend(AfflictionKind.Burn, 2));
            Assert.AreEqual(1, enemy.Stacks(AfflictionKind.Burn));
            Assert.IsTrue(enemy.Burn.IsBurning, "what was not spent still burns");
            Assert.AreEqual(1, enemy.Spend(AfflictionKind.Burn, 5), "no more than it carries");
            Assert.IsFalse(enemy.Burn.IsBurning);
            enemy.Lay(AfflictionKind.Soaked, 1, 0f, 1f, LaySource.Spell);
            Assert.AreEqual(1, enemy.Spend(AfflictionKind.Soaked, 9));
            Assert.IsFalse(enemy.Has(AfflictionKind.Soaked));
            Assert.AreEqual(0, enemy.PresentMask, "nothing left");
        }
    }
}
