using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Trees;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The spell creatures (standin_clearance_plan.md pass 12c): the cobra, the river serpent, the stone oak, and the effect-only six.</summary>
    public class CreatureFormsTests
    {
        [Test]
        public void TheCobraSpitsAtWhatBeginsAnAttackNearIt()
        {
            Assert.IsTrue(CobraRules.Spits(3, 5f, 8f, true, false, 10f), "a wind-up within eight metres");
            Assert.IsFalse(CobraRules.Spits(3, 5f, 8f, false, false, 10f), "on the shoulder it spits only at an attack begun");
            Assert.IsFalse(CobraRules.Spits(3, 9f, 8f, true, false, 10f), "out of reach");
            Assert.IsFalse(CobraRules.Spits(0, 5f, 8f, true, false, 10f), "no spits left");
            Assert.IsFalse(CobraRules.Spits(3, 5f, 8f, true, false, 0.5f), "once for each wind-up");
            Assert.IsTrue(CobraRules.Spits(3, 5f, 9f, false, true, 10f), "set down, it spits at what stands in its circle on its beat");
            var learn = Upgrade.ById(FireTree.Uraeus);
            Assert.AreEqual(SpellForms.Cobra, learn.Form, "a cobra, not a phalanx that spits by itself");
            var shapes = Upgrade.ById(SpellBranch.ShapeId(FireTree.Uraeus)).Shapes;
            var setDown = shapes.Single(each => each.Name == "Set Down");
            Assert.AreEqual(SpellForms.Cobra, setDown.Form);
            Assert.IsTrue(setDown.Spell.Planted);
            Assert.AreEqual(9f, setDown.Spell.Radius, 1e-5f);
            var master = Upgrade.ById(SpellBranch.MasterId(FireTree.Uraeus)).Effects;
            Assert.IsTrue(master.Any(each => each.Do == UpgradeAction.LengthenOwn && each.Seconds == 1f && each.Needs == AfflictionKind.Burn), "a second longer for each spit on a burning enemy");
        }

        [Test]
        public void TheSerpentBringsItsFirstEnemyToYourFeet()
        {
            Assert.AreEqual(new Vector3(0f, 0f, 1.5f), SerpentRules.DropAt(Vector3.zero, Vector3.forward), "a metre and a half in front of you");
            var shape = Upgrade.ById(SpellBranch.ShapeId(WaterTree.Riptide)).Shapes.Single(each => each.Name == "River Serpent");
            Assert.AreEqual(SpellForms.Serpent, shape.Form);
            Assert.AreEqual(8f, shape.Spell.Reach, 1e-5f);
            Assert.AreEqual(16f, shape.Spell.Speed, 1e-5f);
            Assert.IsFalse(shape.Effects.Any(each => each.Do == UpgradeAction.Pull), "carried in its jaws, not pulled four metres");
            Assert.IsTrue(shape.Effects.Any(each => each.Do == UpgradeAction.Condition && (each.State.Flags & ConditionFlags.CannotGuard) != 0), "its guard down");
        }

        [Test]
        public void TheStoneOakGrowsAnAgeForEverySixBlowsNearIt()
        {
            Assert.AreEqual(1, OakRules.AgeFor(0));
            Assert.AreEqual(1, OakRules.AgeFor(5));
            Assert.AreEqual(2, OakRules.AgeFor(6), "six blows: the young tree");
            Assert.AreEqual(3, OakRules.AgeFor(12), "twelve: the old oak");
            Assert.AreEqual(3, OakRules.AgeFor(40), "three ages at most");
            Assert.AreEqual(0f, OakRules.StrikeReach(1), "the sapling strikes nothing");
            Assert.AreEqual(5f, OakRules.StrikeReach(2), 1e-5f);
            Assert.AreEqual(6f, OakRules.StrikeReach(3), 1e-5f);
            Assert.IsFalse(OakRules.StrikesAll(2), "the young tree strikes the nearest");
            Assert.IsTrue(OakRules.StrikesAll(3));
            Assert.AreEqual(0f, OakRules.HealsInShade(2), 1e-5f);
            Assert.AreEqual(3f, OakRules.HealsInShade(3), 1e-5f, "3 a second in the old oak's shade");
            var oak = Upgrade.ById(OvergrowthTree.StoneOak);
            Assert.AreEqual(SpellForms.Oak, oak.Form);
            Assert.AreEqual(20f, oak.Spell.Seconds, 1e-5f);
            Assert.AreEqual(2f, oak.Spell.Interval, 1e-5f);
        }
    }
}
