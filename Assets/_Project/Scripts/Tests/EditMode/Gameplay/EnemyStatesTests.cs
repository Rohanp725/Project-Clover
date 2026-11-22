using System;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>What an enemy may not do (standin_clearance_plan.md pass 9).</summary>
    public class EnemyStatesTests
    {
        private static Upgrade ReactionOf(AfflictionKind one, AfflictionKind other) => Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(one, other)));

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
        public void AnEnemyThatCannotTurnGoesWhereItFaced()
        {
            var enemy = NewEnemy();
            Assert.AreEqual(1f, enemy.TurnMultiplier, 1e-5f);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.seized", Seconds = 4f, Flags = ConditionFlags.TurnSlowed, Scale = 0.6f }, null, 1);
            Assert.AreEqual(0.6f, enemy.TurnMultiplier, 1e-5f, "turning 40% slower");
            enemy.Conditions.Set(new ConditionSpec { Id = "test.stuck", Seconds = 1f, Flags = ConditionFlags.CannotTurn }, null, 2);
            Assert.AreEqual(0f, enemy.TurnMultiplier, 1e-5f, "cannot turn at all");
            enemy.Conditions.Tick(2f);
            Assert.AreEqual(0.6f, enemy.TurnMultiplier, 1e-5f, "the hold on its turning gone, the slowing left");
            var sea = Upgrade.ById(Clover.Player.Trees.Duo.RustDuo.TheSeaTakesIt).Effects;
            var seized = sea.Single(each => each.Do == UpgradeAction.Condition && each.On == UpgradeTrigger.SpellHit).State;
            Assert.AreEqual(4f, seized.Seconds, 1e-5f);
            Assert.AreEqual(0.6f, seized.Scale, 1e-5f);
            Assert.IsTrue((seized.Flags & ConditionFlags.TurnSlowed) != 0);
            Assert.IsTrue(sea.Any(each => each.On == UpgradeTrigger.ConditionStruck && each.By == StruckBy.Heavy && each.State != null && (each.State.Flags & ConditionFlags.WeaponTakenShare) != 0), "a heavy blow knocks a plate off");
        }

        [Test]
        public void SeizedHoldsAGodForItsMoment()
        {
            // The ledger's god-hold: Seized's hold reaches a god by GodHolds (the owner's rule of 2026-11-19), not "what
            // cannot be frozen is not held".
            GodHolds.Restore();
            var hold = ReactionOf(AfflictionKind.Static, AfflictionKind.Soaked).Effects.Single(each => each.Do == UpgradeAction.Hold);
            var man = NewEnemy();
            man.IsGod = true;
            Assert.IsTrue(man.Hold(hold.Seconds), "the man is held");
            Assert.AreEqual(Math.Min(hold.Seconds * GodHolds.ManShare, GodHolds.ManCap), man.HeldLeft, 1e-5f);
            var godly = NewEnemy();
            godly.IsGod = true;
            godly.InGodlyForm = true;
            Assert.IsTrue(godly.Hold(hold.Seconds), "the godly form takes a hitch");
            Assert.AreEqual(Math.Min(hold.Seconds * GodHolds.GodlyShare, GodHolds.GodlyCap), godly.HeldLeft, 1e-5f);
            var plain = NewEnemy();
            Assert.IsTrue(plain.Hold(hold.Seconds));
            Assert.AreEqual(hold.Seconds, plain.HeldLeft, 1e-5f, "a plain enemy the whole second");
        }

        [Test]
        public void RedPaperLetsEveryBlowPassItsGuard()
        {
            var enemy = NewEnemy();
            var light = new Hit(10f, 5f, Vector3.back);
            var heavy = new Hit(20f, 10f, Vector3.back, heavy: true);
            Assert.IsTrue(Enemy.TurnsAside(true, 0f, enemy.Conditions, light, 0.5f, false, EnemyState.Stalk, 10f), "a light blow from the front is turned on its guard");
            Assert.IsTrue(Enemy.TurnsAside(true, 0f, enemy.Conditions, heavy, 0.5f, false, EnemyState.Stalk, 10f), "and a heavy one, on what guards heavy blows");
            var paper = Upgrade.ById(SpellBranch.ShapeId(PoisonTree.FiveNeedles)).Shapes.Single(each => each.Name == "Red Paper").Effects.Single(each => each.Do == UpgradeAction.Condition);
            enemy.Conditions.Set(paper.State, null, 1);
            Assert.IsFalse(Enemy.TurnsAside(true, 0f, enemy.Conditions, light, 0.5f, false, EnemyState.Stalk, 10f), "pinned, it cannot turn a light blow");
            Assert.IsFalse(Enemy.TurnsAside(true, 0f, enemy.Conditions, heavy, 0.5f, false, EnemyState.Idle, 0f), "nor a heavy one, nor in a shield wall's line");
            enemy.Conditions.Tick(paper.State.Seconds + 0.1f);
            Assert.IsTrue(Enemy.TurnsAside(true, 0f, enemy.Conditions, light, 0.5f, false, EnemyState.Stalk, 10f), "after six seconds the guard is back");
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
