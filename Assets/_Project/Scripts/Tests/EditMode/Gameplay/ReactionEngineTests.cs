using System.Collections.Generic;
using Clover.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The reaction engine (IMPLEMENTATION.md 3.7 and 6.1): a laying on an enemy that carries another affliction makes
    /// their pair react, once, and then the pair rests on that enemy; partners react together; a frame reports at most
    /// its budget and the rest wait; what a reaction lays starts nothing; and the set knows what it carries.
    /// </summary>
    public class ReactionEngineTests
    {
        private struct Reported
        {
            public Component Who;
            public int Pair;
            public AfflictionKind Laid;
        }

        private readonly List<Reported> reported = new List<Reported>();
        private int frame;
        private ReactionEngine engine;

        private static readonly int Flashover = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);
        private static readonly int ThermalShock = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Chill);

        [SetUp]
        public void Begin()
        {
            reported.Clear();
            frame = 0;
            // Its own engine, on a frame count the test turns, so nothing here touches the shared one or CombatEvents.
            engine = new ReactionEngine((who, pair, laid) => reported.Add(new Reported { Who = who, Pair = pair, Laid = laid }), () => frame);
        }

        private AfflictionSet NewEnemy() => new AfflictionSet { Reactions = engine };

        private static void Lay(AfflictionSet set, AfflictionKind kind, LaySource source = LaySource.Weapon) => set.Lay(kind, 1, 0f, 1f, source);

        [Test]
        public void APresentPartnerReactsOnceAndThenThePairRests()
        {
            var enemy = NewEnemy();
            Lay(enemy, AfflictionKind.Static);
            Assert.AreEqual(0, reported.Count, "alone, Static has nothing to react with");
            Lay(enemy, AfflictionKind.Burn);
            Assert.AreEqual(1, reported.Count);
            Assert.AreEqual(Flashover, reported[0].Pair);
            Assert.AreEqual(AfflictionKind.Burn, reported[0].Laid, "the kind that landed second set it off");
            Assert.AreEqual(4f, enemy.ReactionRest(Flashover), 0.0001f, "a distance-1 pair rests the usual 4 s");

            Lay(enemy, AfflictionKind.Static);
            Lay(enemy, AfflictionKind.Burn);
            frame++;
            Lay(enemy, AfflictionKind.Burn);
            Assert.AreEqual(1, reported.Count, "resting, the pair does not react again on this enemy, whatever is laid and in whatever frame");

            var another = NewEnemy();
            Lay(another, AfflictionKind.Static);
            Lay(another, AfflictionKind.Burn);
            Assert.AreEqual(2, reported.Count, "the rest is kept per enemy");
        }

        [Test]
        public void TheRestRunsOutEvenWithNothingLeftOnTheEnemyAndThenItReactsAgain()
        {
            var enemy = NewEnemy();
            Lay(enemy, AfflictionKind.Static);
            Lay(enemy, AfflictionKind.Burn);
            Assert.AreEqual(1, reported.Count);
            enemy.Consume(AfflictionKind.Static);
            enemy.Consume(AfflictionKind.Burn);
            Assert.IsFalse(enemy.Any);
            Assert.IsTrue(enemy.Runs, "a resting pair keeps the set running so its owner ticks the rest away");

            enemy.Tick(3.9f);
            Assert.AreEqual(0.1f, enemy.ReactionRest(Flashover), 0.001f);
            Lay(enemy, AfflictionKind.Static);
            Lay(enemy, AfflictionKind.Burn);
            Assert.AreEqual(1, reported.Count, "still resting");

            enemy.Consume(AfflictionKind.Static);
            enemy.Consume(AfflictionKind.Burn);
            enemy.Tick(0.2f);
            Assert.AreEqual(0f, enemy.ReactionRest(Flashover));
            Assert.IsFalse(enemy.Runs, "nothing on it and nothing resting");
            Lay(enemy, AfflictionKind.Static);
            Lay(enemy, AfflictionKind.Burn);
            Assert.AreEqual(2, reported.Count, "the rest is over");
            Assert.AreEqual(Flashover, reported[1].Pair);
        }

        [Test]
        public void TwoPartnersReactAtOnceOnTheirTwoPairs()
        {
            var enemy = NewEnemy();
            // Laid without the engine, so the two partners do not react with each other first.
            enemy.Apply(AfflictionKind.Static, 1);
            enemy.Apply(AfflictionKind.Chill, 1);
            Lay(enemy, AfflictionKind.Burn);
            Assert.AreEqual(2, reported.Count);
            Assert.AreEqual(Flashover, reported[0].Pair, "partners in the order of their kinds");
            Assert.AreEqual(ThermalShock, reported[1].Pair);
            Assert.AreEqual(AfflictionKind.Burn, reported[0].Laid);
            Assert.AreEqual(AfflictionKind.Burn, reported[1].Laid);
            Assert.AreEqual(4f, enemy.ReactionRest(Flashover), 0.0001f);
            Assert.AreEqual(6f, enemy.ReactionRest(ThermalShock), 0.0001f, "the distance-3 pairs rest 6 s");
        }

        [Test]
        public void AFrameReportsItsBudgetAndTheRestWaitForTheNext()
        {
            Assert.AreEqual(8, engine.MostPerFrame);
            var enemies = new AfflictionSet[5];
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i] = NewEnemy();
                enemies[i].Apply(AfflictionKind.Static, 1);
                enemies[i].Apply(AfflictionKind.Chill, 1);
            }
            // Five enemies, two pairs each: ten reactions found in one frame.
            foreach (var enemy in enemies) Lay(enemy, AfflictionKind.Burn, LaySource.Spell);
            Assert.AreEqual(8, reported.Count, "eight in one frame");
            Assert.AreEqual(2, engine.Waiting);
            Assert.AreEqual(4f, enemies[4].ReactionRest(Flashover), 0.0001f, "a waiting reaction already rests its pair");
            Lay(enemies[4], AfflictionKind.Burn);
            Assert.AreEqual(2, engine.Waiting, "so it is not found twice while it waits");

            engine.Pump();
            Assert.AreEqual(8, reported.Count, "the same frame: still waiting");
            frame++;
            engine.Pump();
            Assert.AreEqual(10, reported.Count, "the next frame reports what waited");
            Assert.AreEqual(0, engine.Waiting);
            Assert.AreEqual(Flashover, reported[8].Pair, "in the order they were found");
            Assert.AreEqual(ThermalShock, reported[9].Pair);
        }

        [Test]
        public void WhatAReactionLaysStartsNothingUnlessItCascades()
        {
            var enemy = NewEnemy();
            enemy.Apply(AfflictionKind.Static, 1);
            Assert.IsTrue(enemy.Lay(AfflictionKind.Burn, 1, 0f, 1f, LaySource.Reaction).Applied, "it is still laid");
            Assert.IsTrue(enemy.Has(AfflictionKind.Burn));
            Assert.AreEqual(0, reported.Count, "laid by a reaction, it starts no reaction");
            Assert.AreEqual(0f, enemy.ReactionRest(Flashover), "and rests nothing");

            // An effect that cascades on purpose lays as an effect.
            Lay(enemy, AfflictionKind.Burn, LaySource.Effect);
            Assert.AreEqual(1, reported.Count);
            Assert.AreEqual(Flashover, reported[0].Pair);
        }

        [Test]
        public void LayingAKindAlreadyThereDoesNotReactWithItself()
        {
            var enemy = NewEnemy();
            Lay(enemy, AfflictionKind.Static);
            Lay(enemy, AfflictionKind.Static);
            Lay(enemy, AfflictionKind.Static, LaySource.Spell);
            Assert.AreEqual(3, enemy.Stacks(AfflictionKind.Static), "each laying took");
            Assert.AreEqual(0, reported.Count);
        }

        [Test]
        public void ALayingThatDoesNotTakeStartsNothing()
        {
            var enemy = NewEnemy();
            enemy.Apply(AfflictionKind.Chill, 5);
            Assert.IsTrue(enemy.IsFrozen);
            enemy.Apply(AfflictionKind.Static, 1);
            Assert.IsFalse(enemy.Lay(AfflictionKind.Chill, 1, 0f, 1f, LaySource.Weapon).Applied, "what is frozen cannot be chilled further");
            Assert.IsFalse(enemy.Lay(AfflictionKind.Burn, 0, 0f, 1f, LaySource.Weapon).Applied, "no stacks, no laying");
            Assert.AreEqual(0, reported.Count);
        }

        [Test]
        public void ABlowThatLaysBurnOnAChargedEnemyReactsThroughTakeFrom()
        {
            var enemy = NewEnemy();
            enemy.Apply(AfflictionKind.Static, 2);
            enemy.TakeFrom(new Hit(10f, 5f, Vector3.forward, affliction: AfflictionKind.Burn, afflictionStacks: 1), null);
            Assert.AreEqual(1, reported.Count);
            Assert.AreEqual(Flashover, reported[0].Pair);
            Assert.AreEqual(AfflictionKind.Burn, reported[0].Laid);

            // A charged blade's blow lays two afflictions, and the second reacts with the first.
            var other = NewEnemy();
            other.TakeFrom(new Hit(10f, 5f, Vector3.forward, affliction: AfflictionKind.Static, afflictionStacks: 1,
                affliction2: AfflictionKind.Chill, affliction2Stacks: 1), null);
            Assert.AreEqual(2, reported.Count);
            Assert.AreEqual(AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Chill), reported[1].Pair);
            Assert.AreEqual(AfflictionKind.Chill, reported[1].Laid);
        }

        [Test]
        public void APairThatWaitsForDepthReactsOnlyOnceItLiesThatDeepInEitherOrder()
        {
            // Seized: "when a Soaked enemy carries two Static or more" (task 3.4).
            int seized = AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Soaked);
            engine.SetDepth(seized, AfflictionKind.Static, 2);
            Assert.AreEqual(AfflictionKind.Static, engine.DepthKind(seized));
            Assert.AreEqual(2, engine.DepthFor(seized));

            var enemy = NewEnemy();
            Lay(enemy, AfflictionKind.Soaked);
            Lay(enemy, AfflictionKind.Static);
            Assert.AreEqual(0, reported.Count, "one Static on the wet body is not enough");
            Assert.AreEqual(0f, enemy.ReactionRest(seized), "and nothing rests, so the next Static can set it off");
            Assert.IsFalse(engine.Force(enemy, null, seized, AfflictionKind.Static), "forced, it still waits for the depth");
            Lay(enemy, AfflictionKind.Static);
            Assert.AreEqual(1, reported.Count, "the second Static sets it off");
            Assert.AreEqual(seized, reported[0].Pair);
            Assert.Greater(enemy.ReactionRest(seized), 0f);

            var charged = NewEnemy();
            charged.Lay(AfflictionKind.Static, 2, 0f, 1f, LaySource.Weapon);
            Lay(charged, AfflictionKind.Soaked);
            Assert.AreEqual(2, reported.Count, "already two deep, the water sets it off");
            var shallow = NewEnemy();
            Lay(shallow, AfflictionKind.Static);
            Lay(shallow, AfflictionKind.Soaked);
            Assert.AreEqual(2, reported.Count, "one deep, the water does not");

            engine.SetDepth(seized, AfflictionKind.Static, 1);
            Assert.AreEqual(AfflictionKind.None, engine.DepthKind(seized), "a depth of one asks for nothing");
            var plain = NewEnemy();
            Lay(plain, AfflictionKind.Static);
            Lay(plain, AfflictionKind.Soaked);
            Assert.AreEqual(3, reported.Count);
        }

        [Test]
        public void RemovingTheLastStackClearsTheMask()
        {
            var enemy = NewEnemy();
            Assert.AreEqual(0, (int)enemy.PresentMask);
            Lay(enemy, AfflictionKind.Static);
            enemy.Apply(AfflictionKind.Soaked, 1);
            Assert.AreEqual((1 << (int)AfflictionKind.Static) | (1 << (int)AfflictionKind.Soaked), (int)enemy.PresentMask);
            enemy.Consume(AfflictionKind.Static);
            Assert.AreEqual(1 << (int)AfflictionKind.Soaked, (int)enemy.PresentMask, "consumed, its bit goes");
            enemy.Tick(Afflictions.Spec(AfflictionKind.Soaked).Seconds + 0.1f);
            Assert.AreEqual(0, (int)enemy.PresentMask, "faded, its bit goes");

            enemy.Apply(AfflictionKind.Burn, 1);
            Assert.AreEqual(1 << (int)AfflictionKind.Burn, (int)enemy.PresentMask);
            enemy.Tick(enemy.Burn.MaximumSeconds + 0.1f);
            Assert.AreEqual(0, (int)enemy.PresentMask, "burnt out, its bit goes");

            enemy.Apply(AfflictionKind.Marked, 1);
            enemy.Apply(AfflictionKind.Chill, 5);
            enemy.Shatter();
            // A mark is spent by its blows (three as written; a mark of the tree may give more).
            for (int blow = 0; blow < 20 && enemy.Has(AfflictionKind.Marked); blow++) enemy.BlowLanded();
            Assert.AreEqual(0, (int)enemy.PresentMask, "shattered and spent by blows, their bits go");

            enemy.Apply(AfflictionKind.Poison, 2);
            enemy.Clear();
            Assert.AreEqual(0, (int)enemy.PresentMask);
        }
    }
}
