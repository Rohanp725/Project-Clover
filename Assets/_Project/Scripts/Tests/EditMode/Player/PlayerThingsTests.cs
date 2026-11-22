using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.World;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The player's own things in the world: what they grew, their helpers (standin_clearance_plan.md pass 10, and pass 4's grown-bursts).</summary>
    public class PlayerThingsTests
    {
        [Test]
        public void AWitheringReachesOnlyWhatStandsWithinItsReach()
        {
            Assert.IsTrue(GrownRules.Within(new Vector3(11f, 0f, 0f), Vector3.zero, 12f));
            Assert.IsFalse(GrownRules.Within(new Vector3(13f, 0f, 0f), Vector3.zero, 12f), "beyond twelve metres it is left standing");
            Assert.IsTrue(GrownRules.Within(new Vector3(90f, 0f, 0f), Vector3.zero, 0f), "no reach named: everything");
            var learn = Upgrade.ById(LifeTree.DrawnFromTheWood);
            Assert.IsTrue(learn.Effects.Any(each => each.Do == UpgradeAction.BurstGrown && each.Reach == 12f), "Drawn From the Wood withers only within twelve metres");
        }

        [Test]
        public void WhatGrowsBackComesUpWhenItsTimeComes()
        {
            var queue = new RegrowQueue();
            Assert.IsFalse(queue.TakeDue(100f, out _));
            queue.Add(new HazardSpec { Radius = 2f, Position = new Vector3(1f, 0f, 1f) }, 16f);
            queue.Add(new HazardSpec { Radius = 3f }, 12f);
            Assert.AreEqual(2, queue.Count);
            Assert.IsFalse(queue.TakeDue(11f, out _), "not yet");
            Assert.IsTrue(queue.TakeDue(12f, out var first));
            Assert.AreEqual(3f, first.Radius, 1e-5f, "the one due first");
            Assert.IsFalse(queue.TakeDue(15f, out _));
            Assert.IsTrue(queue.TakeDue(16f, out var second));
            Assert.AreEqual(new Vector3(1f, 0f, 1f), second.Position, "where it stood");
            for (int i = 0; i < RegrowQueue.Most + 3; i++) queue.Add(new HazardSpec(), 20f + i);
            Assert.AreEqual(RegrowQueue.Most, queue.Count, "never more than it holds");
            queue.Clear();
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(6f, LifeTree.GrowsBackSeconds, 1e-5f, "Wood Grows Back: six seconds");
        }

        [Test]
        public void ReapReachesTheWholeFightAndSecondSowingAnswersEachBurst()
        {
            var learn = Upgrade.ById(LifeTree.ReapWhatIsSown);
            var fanned = learn.Effects.Where(each => each.EachWithin > 0f).ToArray();
            Assert.AreEqual(2, fanned.Length, "the bursts and the mending");
            Assert.IsTrue(fanned.All(each => each.EachWithin >= LifeTree.FightReach && each.OnlyCarrying == AfflictionKind.Seeded && each.On == UpgradeTrigger.SpellCast),
                "every Seeded enemy in the fight, wherever it stands");
            var sowing = Upgrade.ById(SpellBranch.ShapeId(LifeTree.ReapWhatIsSown)).Shapes.Single(each => each.Name == "Second Sowing").Effects;
            Assert.IsTrue(sowing.Any(each => each.On == UpgradeTrigger.GrownBurst && each.Do == UpgradeAction.Afflict && each.Affliction == AfflictionKind.Seeded), "each grown thing's burst sows");
            Assert.AreNotEqual("its moment comes", TriggerWords.Of(UpgradeTrigger.GrownBurst), "the moment has words of its own");
        }
    }
}
