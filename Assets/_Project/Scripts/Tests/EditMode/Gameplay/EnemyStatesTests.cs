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
        public void SunkItNeitherLeapsNorStepsBack()
        {
            var enemy = NewEnemy();
            var brain = new EnemyBrain { RetreatRange = 4f, PreferredRange = 2f, Conditions = enemy.Conditions };
            brain.Attacks.Add(new EnemyAttack { Name = "leap", Telegraph = 0.5f, Strike = 0.1f, Recovery = 0.5f, MinRange = 0f, MaxRange = 10f, Leap = true });
            brain.Reset();
            var sand = ReactionOf(AfflictionKind.Soaked, AfflictionKind.Tremor).Effects.First(each => each.Do == UpgradeAction.Condition).State;
            Assert.IsTrue((sand.Flags & ConditionFlags.Mired) != 0, "The Ground Gives sinks it");
            enemy.Conditions.Set(sand, null, 1);
            Assert.AreEqual(sand.Sink, enemy.Conditions.SinkDepth, 1e-5f, "drawn down into the sand");
            Assert.That(sand.Sink, Is.GreaterThan(0f));
            brain.Tick(0.1f, 3f, 0f);
            brain.Tick(0.1f, 3f, 0f);
            Assert.AreEqual(EnemyState.Stalk, brain.State, "no leap begun while sunk");
            Assert.AreEqual(0, brain.MoveWish, "and it does not back away though the target is inside its retreat range");
            enemy.Conditions.Tick(sand.Seconds + 0.1f);
            Assert.AreEqual(0f, enemy.Conditions.SinkDepth, 1e-5f);
            brain.Tick(0.1f, 3f, 0f);
            Assert.AreEqual(EnemyState.Telegraph, brain.State, "free of the sand, it leaps");
            var girdled = ReactionOf(AfflictionKind.Seeded, AfflictionKind.Cursed).Effects.Where(each => each.Do == UpgradeAction.Condition && each.On == UpgradeTrigger.Reacted);
            Assert.IsTrue(girdled.Any(each => (each.State.Flags & ConditionFlags.Mired) != 0), "the coils hold its feet too");
        }

        [Test]
        public void TheBogAndDrivenDownAnswerTheSunk()
        {
            var shapes = Upgrade.ById(Clover.Player.Trees.Duo.TheGroundGivesDuo.Shape).Shapes;
            foreach (var shape in shapes)
            {
                var state = shape.Effects.First(each => each.Do == UpgradeAction.Condition).State;
                Assert.AreEqual(Reactions.SunkInSand, state.Is, shape.Name + " sinks as the plain sand does");
                Assert.IsTrue((state.Flags & ConditionFlags.Mired) != 0, shape.Name);
            }
            var bog = Upgrade.ById(Clover.Player.Trees.Duo.TheGroundGivesDuo.GivenToTheBog).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.Kill, bog.On);
            Assert.AreEqual(Reactions.SunkInSand, bog.InState, "one that dies sunk");
            var driven = Upgrade.ById(Clover.Player.Trees.Duo.TheGroundGivesDuo.DrivenDown).Effects;
            var open = driven.Single(each => each.Do == UpgradeAction.Condition).State;
            var enemy = NewEnemy();
            var light = new Hit(10f, 5f, Vector3.back);
            var heavy = new Hit(20f, 10f, Vector3.back, heavy: true);
            enemy.Conditions.Set(ReactionOf(AfflictionKind.Soaked, AfflictionKind.Tremor).Effects.First(each => each.Do == UpgradeAction.Condition).State, null, 1);
            enemy.Conditions.Set(open, null, 2);
            Assert.IsFalse(Enemy.TurnsAside(true, 0f, enemy.Conditions, heavy, 0.5f, false, EnemyState.Stalk, 0f), "a heavy blow passes the sunk one's guard");
            Assert.IsTrue(Enemy.TurnsAside(true, 0f, enemy.Conditions, light, 0.5f, false, EnemyState.Stalk, 0f), "a light one is still turned");
            Assert.IsTrue(driven.Any(each => each.On == UpgradeTrigger.HeavyHit && each.InState == Reactions.SunkInSand && each.Do == UpgradeAction.Hold));
        }

        [Test]
        public void TurnedToFaceAPointItStandsAndShowsItsBack()
        {
            var enemy = NewEnemy();
            var brain = new EnemyBrain { PreferredRange = 2f, Conditions = enemy.Conditions };
            brain.Reset();
            Assert.IsFalse(enemy.Conditions.TurnedToward(out _));
            var herb = new Vector3(4f, 0f, 0f);
            int slot = enemy.Conditions.Set(new ConditionSpec { Id = "test.turned", Seconds = 1.5f, Flags = ConditionFlags.FacesPoint | ConditionFlags.BackCritical }, null, 1);
            enemy.Conditions.FaceToward(slot, herb);
            Assert.IsTrue(enemy.Conditions.TurnedToward(out Vector3 point));
            Assert.AreEqual(herb, point, "it faces the herb");
            brain.Tick(0.1f, 6f, 0f);
            brain.Tick(0.1f, 6f, 0f);
            Assert.AreEqual(0, brain.MoveWish, "and walks nowhere while it does");
            enemy.Conditions.Tick(2f);
            Assert.IsFalse(enemy.Conditions.TurnedToward(out _), "its moment over");

            // The player behind it: a blow on its back. In front of it: not.
            Assert.IsTrue(PlayerCombat.OnItsBack(Vector3.zero, Vector3.right, new Vector3(-2f, 0f, 0f), -0.3f));
            Assert.IsFalse(PlayerCombat.OnItsBack(Vector3.zero, Vector3.right, new Vector3(2f, 0f, 0f), -0.3f));
            // Turned About faces it straight away from the player.
            var away = ConditionCaster.FacePoint(new ConditionSpec { FaceAway = true }, Vector3.zero, new Vector3(0f, 0f, -3f), herb);
            Assert.That(away.z, Is.GreaterThan(5f), "away from the player");
            Assert.AreEqual(herb, ConditionCaster.FacePoint(new ConditionSpec(), Vector3.zero, new Vector3(0f, 0f, -3f), herb), "toward the spell's middle otherwise");
        }

        [Test]
        public void TurnedAboutSpinsWithoutLiftingAndTurnedToTheSunTurnsToTheHerb()
        {
            var about = Upgrade.ById(SpellBranch.ShapeId(WindTree.LiftingWind)).Shapes.Single(each => each.Name == "Turned About");
            Assert.IsTrue(about.Spell != null && about.Spell.Quiet, "the column lifts nothing");
            Assert.AreEqual(40f, about.Spell.Stagger, 1e-5f, "and still takes 40 stagger");
            var spin = about.Effects.Single(each => each.Do == UpgradeAction.Condition).State;
            Assert.IsTrue(spin.FaceAway && (spin.Flags & ConditionFlags.FacesPoint) != 0 && (spin.Flags & ConditionFlags.BackCritical) != 0);
            Assert.AreEqual(1.2f, spin.Seconds, 1e-5f);
            var elite = NewEnemy();
            elite.FreezeScale = 0.4f;
            elite.Conditions.Set(spin, null, 1, null, false, false, ConditionCaster.ShareOf(elite));
            elite.Conditions.Tick(0.6f);
            Assert.IsFalse(elite.Conditions.Holds(WindTree.TurnedAbout), "an elite is turned only half a second");
            var god = NewEnemy();
            god.IsGod = true;
            Assert.IsFalse(ConditionCaster.Takes(spin, god), "a god is not swung round");
            var sun = Upgrade.ById(Clover.Player.Trees.Duo.ShiningHerbDuo.TurnedToTheSun).Effects.Single(each => each.Do == UpgradeAction.Condition).State;
            Assert.IsFalse(sun.FaceAway, "toward the herb");
            Assert.IsTrue((sun.Flags & ConditionFlags.FacesPoint) != 0 && (sun.Flags & ConditionFlags.BackCritical) != 0);
        }

        [Test]
        public void ALeashHoldsItNearAndDrawsItBack()
        {
            var anchor = Vector3.zero;
            var inside = EnemyChains.Leash(new Vector3(2f, 0f, 0f), new Vector3(0.5f, -0.1f, 0f), anchor, 4f, 0.1f);
            Assert.AreEqual(new Vector3(0.5f, -0.1f, 0f), inside, "inside its leash it goes where it will");
            var outward = EnemyChains.Leash(new Vector3(3.9f, 0f, 0f), new Vector3(0.5f, -0.1f, 0f), anchor, 4f, 0.1f);
            Assert.AreEqual(4f, 3.9f + outward.x, 1e-4f, "stopped at the leash's end");
            Assert.AreEqual(-0.1f, outward.y, 1e-5f, "its fall its own");
            var beyond = EnemyChains.Leash(new Vector3(6f, 0f, 0f), Vector3.zero, anchor, 4f, 0.5f);
            Assert.AreEqual(-EnemyChains.DrawRate * 0.5f, beyond.x, 1e-4f, "set beyond its leash, it is drawn back");
            Assert.AreEqual(new Vector3(1f, 0f, 0f), EnemyChains.Leash(new Vector3(6f, 0f, 0f), new Vector3(1f, 0f, 0f), anchor, 0f, 0.5f), "no leash: free");

            // Tethered: the player holds the leash, five metres.
            var tether = Upgrade.ById(SpellBranch.ShapeId(DarkTree.Grasp)).Shapes.Single(each => each.Name == "Tethered").Effects.First(each => each.Do == UpgradeAction.Condition).State;
            var enemy = NewEnemy();
            Assert.IsFalse(enemy.Conditions.Leashed(out _, out _, out _, out _));
            enemy.Conditions.Set(tether, null, 1);
            Assert.IsTrue(enemy.Conditions.Leashed(out var link, out _, out float leash, out bool toPlayer));
            Assert.IsTrue(toPlayer && link == null);
            Assert.AreEqual(5f, leash, 1e-5f);
            enemy.Conditions.Tick(tether.Seconds + 0.1f);
            Assert.IsFalse(enemy.Conditions.Leashed(out _, out _, out _, out _), "free after four seconds");
        }

        [Test]
        public void BlackChainPassesBlowsAndItsMarksFindTheMate()
        {
            var chain = ReactionOf(AfflictionKind.Sundered, AfflictionKind.Cursed).Effects.Single(each => each.Do == UpgradeAction.Condition).State;
            Assert.IsTrue((chain.Flags & ConditionFlags.Chained) != 0 && chain.ToOthers);
            Assert.AreEqual(0.3f, chain.Scale, 1e-5f, "30% of each weapon blow");
            Assert.IsFalse(chain.SharesSpells, "weapon blows only");
            Assert.AreEqual(4f, chain.Leash, 1e-5f);
            Assert.AreEqual(2f, chain.AloneLeash, 1e-5f, "alone, chained to the floor");
            var wound = Upgrade.ById(Clover.Player.Trees.Duo.BlackChainDuo.Shape).Shapes.Single(each => each.Name == "Shared Wound").Effects.Single().State;
            Assert.IsTrue((wound.Flags & ConditionFlags.Chained) != 0 && wound.SharesSpells, "half of every blow, spells too");
            Assert.AreEqual(0f, wound.Leash, 1e-5f, "and no leash");
            var dead = Upgrade.ById(Clover.Player.Trees.Duo.BlackChainDuo.DeadWeight).Effects;
            Assert.IsTrue(dead.All(each => each.On == UpgradeTrigger.Kill && each.ToChainMate), "the chain-mate answers the death");
            Assert.IsTrue(dead.Any(each => each.Do == UpgradeAction.Pull) && dead.Any(each => each.Do == UpgradeAction.Hold) && dead.Any(each => each.Do == UpgradeAction.Afflict));
            var noose = Upgrade.ById(Clover.Player.Trees.Duo.BlackChainDuo.OneNooseForTwo).Effects.Single();
            Assert.IsTrue(noose.ToChainMate && noose.ByPower);
            Assert.AreEqual(0.5f, noose.Share, 1e-5f, "half the finisher");
            var enemy = NewEnemy();
            Assert.IsNull(enemy.Conditions.ChainMate(out _, out _), "chained to no one");
            Assert.IsNull(EnemyChains.MateOf(null));
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
