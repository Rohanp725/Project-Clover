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
        public void ThreeShadesAtMostCountedTogether()
        {
            var shades = Upgrade.ById(DarkTree.NothingStaysBuried).Effects.Where(each => each.Do == UpgradeAction.Helper).ToArray();
            Assert.AreEqual(2, shades.Length, "a Cursed death, and any death while Risen");
            Assert.IsTrue(shades.All(each => each.Limit == 3), "three at most");
            Assert.AreEqual(PlayerUpgrades.FamilyOf(shades[0]), PlayerUpgrades.FamilyOf(shades[1]), "both rules count against the same three");
        }

        [Test]
        public void KindledGuardAndTheHostTendWhatYouGrew()
        {
            var guard = Upgrade.ById(Clover.Player.Trees.Duo.FirebloomDuo.KindledGuard).Effects.Single();
            Assert.AreEqual(UpgradeAction.Tend, guard.Do, "the lily itself flares");
            Assert.AreEqual(3f, guard.Radius, 1e-5f, "within 3 m of you");
            Assert.AreEqual(3f, guard.Seconds, 1e-5f, "and stands 3 s longer");
            Assert.AreEqual(12f, guard.Damage, 1e-5f);
            Assert.AreEqual(StatWhen.Always, guard.While, "no longer a flare round you while near");
            var host = Upgrade.ById(Clover.Player.Trees.Duo.RotRootsDuo.TheHost).Effects.Single();
            Assert.AreEqual(UpgradeAction.Tend, host.Do);
            Assert.AreEqual(AfflictionKind.Poison, host.BitesLay, "every bite doses");
            Assert.AreEqual(AfflictionKind.Poison, host.Needs, "fed by a poisoned death");
            Assert.AreEqual(0f, host.Damage, 1e-5f, "it does not flare");
        }

        [Test]
        public void GrownThingsThrowBoltsAndBiteWithTheOtherElement()
        {
            var flower = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Seeded))).Effects;
            var bolts = flower.Single(each => each.Do == UpgradeAction.GrownBolts);
            Assert.AreEqual(4, bolts.Count, "four at most");
            Assert.AreEqual(8f, bolts.Reach, 1e-5f, "from within 8 m");
            Assert.AreEqual(8f, bolts.Damage, 1e-5f, "the fern's own numbers");
            Assert.AreEqual(AfflictionKind.Static, bolts.Affliction);
            Assert.IsTrue(flower.Any(each => each.Do == UpgradeAction.Arc), "and the fern still throws its own");
            var rain = Upgrade.ById(Clover.Player.Trees.Duo.LightningFlowerDuo.RainOnTheField).Effects;
            Assert.IsTrue(rain.All(each => each.On == UpgradeTrigger.GrownBolt), "for every bolt a grown thing throws");
            Assert.IsTrue(rain.Any(each => each.Do == UpgradeAction.Heal && each.Amount == 3f));
            Assert.IsTrue(rain.Any(each => each.Do == UpgradeAction.Tend && each.Seconds == 3f), "the thrower stands 3 s longer");
            Assert.IsEmpty(Upgrade.ById(Clover.Player.Trees.Duo.IronThornsDuo.OreInTheRoot).Effects, "Ore in the Root is a rule of the ground, not a blow of yours");
            Assert.AreNotEqual("its moment comes", TriggerWords.Of(UpgradeTrigger.GrownBolt));
        }

        [Test]
        public void TheTreeMendsOnlyInItsRingAndTheGraveOnlyWhileItsBladesStand()
        {
            var mend = Upgrade.ById(LifeTree.ElderTree).Effects.Single(each => each.Do == UpgradeAction.Heal);
            Assert.IsTrue(mend.WhileOwnStands, "only while the tree stands");
            Assert.AreEqual(3f, mend.WithinOwn, 1e-5f, "and only inside its ring");
            var grave = Upgrade.ById(SpellBranch.MasterId(MetalTree.GraveOfSwords)).Effects.Single(each => each.Do == UpgradeAction.Mana);
            Assert.IsTrue(grave.WhileOwnStands, "only while the blades stand");
            Assert.AreEqual(0f, grave.WithinOwn, 1e-5f, "wherever you stand");
            Assert.IsFalse(Clover.Player.Forms.SpellWorks.StandsOf(Upgrade.ById(LifeTree.ElderTree), Vector3.zero, 3f), "nothing cast, nothing stands");
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
