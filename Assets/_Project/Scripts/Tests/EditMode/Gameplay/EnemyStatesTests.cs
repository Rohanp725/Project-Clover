using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>What an enemy may not do (standin_clearance_plan.md pass 9).</summary>
    public class EnemyStatesTests
    {
        private static AfflictionSet NewEnemy() => new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };

        [Test]
        public void ABalanceHeldDoesNotRecoverWhileItLasts()
        {
            var enemy = NewEnemy();
            Assert.IsTrue(enemy.BalanceRecovers);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.ash", Seconds = 1f, Flags = ConditionFlags.BalanceHeld }, null, 1);
            Assert.IsFalse(enemy.BalanceRecovers, "held where it is");
            enemy.Conditions.Tick(1.5f);
            Assert.IsTrue(enemy.BalanceRecovers, "its time is up: the balance mends again");
        }

        [Test]
        public void WhirlpoolAndAshFallHoldTheBalance()
        {
            var churn = Upgrade.ById(WaterTree.Whirlpool).Effects.Single(each => each.Do == UpgradeAction.Condition);
            Assert.IsTrue((churn.State.Flags & ConditionFlags.BalanceHeld) != 0, "every bite of the whirlpool holds it");
            Assert.That(churn.State.Seconds, Is.GreaterThan(Upgrade.ById(WaterTree.Whirlpool).Spell.Interval), "and lasts until the next bite, so it holds while they are inside");
            var ash = Upgrade.ById(SpellBranch.ShapeId(Upgrade.MeteorLeap)).Shapes.First(each => each.Name == "Ash Fall").Effects
                .Single(each => each.Do == UpgradeAction.Condition);
            Assert.AreEqual(5f, ash.State.Seconds, 1e-5f, "for the ash's five seconds");
            Assert.IsTrue((ash.State.Flags & ConditionFlags.BalanceHeld) != 0);
        }
    }
}
