using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The rest of pass 3 (standin_clearance_plan.md): spells that take only the afflicted, a reaction struck again and its
    /// reach, critical and harder blows (reaction-stats), a guard made perfect by a condition, a reach that seeks only the
    /// afflicted, and a reaction's numbers changed by a test as it fires.
    /// </summary>
    public class ReactionStatsTests
    {
        private static AfflictionSet Carrying(params AfflictionKind[] kinds)
        {
            var set = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            foreach (var kind in kinds) set.Lay(kind, 1, 0f, 1f, LaySource.Reaction);
            return set;
        }

        private sealed class Afflicted : IAfflictable
        {
            public Afflicted(AfflictionSet set) => Afflictions = set;
            public AfflictionSet Afflictions { get; }
        }

        [Test]
        public void ASpellThatSeeksTwoTakesOnlyWhatCarriesBoth()
        {
            var both = new Afflicted(Carrying(AfflictionKind.Chill, AfflictionKind.Seeded));
            var one = new Afflicted(Carrying(AfflictionKind.Chill));
            Assert.IsTrue(SpellReach.Takes(both, AfflictionKind.Chill, AfflictionKind.Seeded));
            Assert.IsFalse(SpellReach.Takes(one, AfflictionKind.Chill, AfflictionKind.Seeded), "one of the two is not enough");
            Assert.IsTrue(SpellReach.Takes(one, AfflictionKind.None, AfflictionKind.None), "seeking nothing takes all");
            var green = Upgrade.ById(WinterSeedDuo.GreenUnderTheIce);
            Assert.AreEqual(AfflictionKind.Chill, green.Spell.OnlyCarrying);
            Assert.AreEqual(AfflictionKind.Seeded, green.Spell.OnlyCarrying2);
            Assert.AreEqual(8, green.Spell.Count, "eight at most");
            Assert.AreEqual(0.5f, green.Spell.MissedWait, 1e-5f, "half the wait back with no one reached");
            Assert.IsTrue(green.Effects.Any(effect => effect.Do == UpgradeAction.Shatter), "a Frozen one shatters");
        }

        [Test]
        public void TheCursedAloneAreCalledAndTheMarkedAloneCharged()
        {
            var collected = Upgrade.ById(SpellBranch.ShapeId(DarkTree.DebtsCalled)).Shapes.First(shape => shape.Name == "Collected");
            Assert.AreEqual(AfflictionKind.Cursed, collected.Spell.OnlyCarrying);
            var net = Upgrade.ById(TheBoltRunsTrueDuo.TheNetOfJewels).Effects.Single(effect => effect.On == UpgradeTrigger.CritHit);
            Assert.AreEqual(AfflictionKind.Marked, net.OnlyCarrying, "each critical blow charges the linked alone");
            var drawn = Upgrade.ById(LiveIronDuo.Shape).Shapes.First(shape => shape.Name == "Drawn Iron").Effects;
            Assert.IsFalse(drawn.Any(effect => effect.Do == UpgradeAction.Pull), "no pull of everything within 8 m");
            Assert.AreEqual(1.5f, drawn.Single(effect => effect.Do == UpgradeAction.Arc).DrawsIn, 1e-5f, "what the arcs reach is drawn 1.5 m");
            Assert.AreEqual(1.5f, PlayerCombat.PullStep(6f, 1.5f), 1e-5f);
            Assert.AreEqual(0.5f, PlayerCombat.PullStep(1.5f, 1.5f), 1e-5f, "never past arm's length");
            Assert.AreEqual(0f, PlayerCombat.PullStep(0f, 1.5f), 1e-5f, "the one struck stays");
        }

        [Test]
        public void AReactionStruckAgainStrikesButLaysNothing()
        {
            Assert.IsTrue(ReactionRules.StrikesAgain(UpgradeAction.Burst));
            Assert.IsTrue(ReactionRules.StrikesAgain(UpgradeAction.Lane));
            Assert.IsTrue(ReactionRules.StrikesAgain(UpgradeAction.Arc));
            Assert.IsFalse(ReactionRules.StrikesAgain(UpgradeAction.Afflict));
            Assert.IsFalse(ReactionRules.StrikesAgain(UpgradeAction.Condition));
            Assert.IsFalse(ReactionRules.StrikesAgain(UpgradeAction.Spend));
            var twice = Upgrade.ById(LightningTree.StrikesTwice).Effects.Single();
            Assert.AreEqual(UpgradeAction.EchoReaction, twice.Do);
            Assert.AreEqual(AfflictionKind.Static, twice.ReactionWith, "every reaction with Static in it");
            Assert.AreEqual(0.5f, twice.Amount, 1e-5f, "at half strength");
            Assert.AreEqual(0.5f, twice.Delay, 1e-5f, "half a second later");
            var spreading = Upgrade.ById(WaterTree.SpreadingWater).Effects.Single();
            Assert.AreEqual(UpgradeAction.EchoReaction, spreading.Do);
            Assert.AreEqual(AfflictionKind.Soaked, spreading.ReactionWith);
            Assert.AreEqual(4f, spreading.EachWithin, 1e-5f, "to every other Soaked enemy within four metres");
            Assert.IsTrue(spreading.EachOther);
            Assert.AreEqual(AfflictionKind.Soaked, spreading.OnlyCarrying);
        }

        [Test]
        public void AReactionsReachCriticalAndBlowOnTheNotchedComeFromTheMarks()
        {
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.ReactionReach, 0.5f, AfflictionKind.Unsteady));
            stats.AddLearned(new StatMod(PlayerStat.ReactionCritical, 1f, AfflictionKind.Marked));
            stats.AddLearned(new StatMod(PlayerStat.ReactionDamageOn, 0.25f, AfflictionKind.Sundered));
            Assert.AreEqual(1.5f, stats.ReactionMultiplier(PlayerStat.ReactionReach, AfflictionKind.Poison, AfflictionKind.Unsteady), 1e-5f, "half again as far");
            Assert.AreEqual(1f, stats.ReactionMultiplier(PlayerStat.ReactionReach, AfflictionKind.Poison, AfflictionKind.Burn), 1e-5f);
            Assert.Greater(stats.ReactionMultiplier(PlayerStat.ReactionCritical, AfflictionKind.Marked, AfflictionKind.Burn), 1f);
            Assert.AreEqual(1f, stats.ReactionMultiplier(PlayerStat.ReactionCritical, AfflictionKind.Soaked, AfflictionKind.Burn), 1e-5f);
            Assert.AreEqual(0.25f, stats.Against(PlayerStat.ReactionDamageOn, Carrying(AfflictionKind.Sundered)), 1e-5f, "on the notched");
            Assert.AreEqual(0f, stats.Against(PlayerStat.ReactionDamageOn, Carrying(AfflictionKind.Burn)), 1e-5f, "on no other");
            Assert.IsTrue(Upgrade.ById(WindTree.CarriedFar).Stats.Any(mod => mod.Stat == PlayerStat.ReactionReach && mod.Against == AfflictionKind.Unsteady));
            Assert.IsTrue(Upgrade.ById(LightTree.SeenPlainly).Stats.Any(mod => mod.Stat == PlayerStat.ReactionCritical && mod.Against == AfflictionKind.Marked));
            Assert.IsFalse(Upgrade.ById(MetalTree.EveryCrackRings).Stats.Any(mod => mod.Stat == PlayerStat.ReactionDamage), "no longer on every reaction with Sundered in it");
        }

        [Test]
        public void CollectedTwiceRepeatsTheReactionsOwnDamage()
        {
            var conditions = new EnemyConditions();
            int slot = conditions.Set(new ConditionSpec { Id = "test.print", Seconds = 4f, Scale = 1f }, null);
            conditions.Keep(slot, 23f);
            Assert.AreEqual(23f, conditions.Run(slot).Scale, 1e-5f, "the print keeps the damage the reaction dealt");
            var effects = Upgrade.ById(DarkTree.CollectedTwice).Effects;
            Assert.IsTrue(effects.Single(effect => effect.Do == UpgradeAction.Condition).ByPower, "the print keeps the blow's damage");
            var repeat = effects.Single(effect => effect.Do == UpgradeAction.Burst);
            Assert.IsTrue(repeat.ByPower, "the next blow repeats it");
            Assert.AreEqual(0f, repeat.Damage, 1e-5f, "no stand-in 15");
        }

        [Test]
        public void APlainGuardAgainstASnappedBladeIsPerfect()
        {
            var guard = new Guard { PerfectWindow = 0.2f, StaminaPerDamage = 1f, ChipFraction = 0.25f };
            guard.SetRaised(true);
            var stamina = new StaminaPool { Maximum = 100f, RegenerationPerSecond = 0f };
            stamina.Fill();
            Assert.AreEqual(GuardResult.Blocked, guard.Resolve(20f, false, stamina, out _), "a plain guard");
            float before = stamina.Current;
            Assert.AreEqual(GuardResult.Perfect, guard.Resolve(20f, false, stamina, out float chip, false, true), "made perfect");
            Assert.AreEqual(0f, chip, 1e-5f);
            Assert.AreEqual(before, stamina.Current, 1e-5f, "nothing paid");
            Assert.AreEqual(GuardResult.NotGuarded, guard.Resolve(20f, true, stamina, out _, false, true), "a red blow still lands");
            var snapped = Upgrade.ById(ColdShortDuo.Shape).Shapes.First(shape => shape.Name == "Snapped Blade").Effects;
            Assert.IsTrue((snapped[0].State.Flags & ConditionFlags.GuardMadePerfect) != 0);
            Assert.IsTrue(snapped.Any(effect => effect.On == UpgradeTrigger.ConditionAttacked && effect.By == StruckBy.Turned && effect.Condition == ColdShortDuo.Snapped && effect.Do == UpgradeAction.Burst && effect.Stagger == 50f),
                "its weapon breaks on the turned blow: 50 stagger");
            Assert.IsTrue(snapped.Any(effect => effect.On == UpgradeTrigger.ConditionAttacked && effect.By == StruckBy.Turned && effect.Condition == ColdShortDuo.Snapped && effect.Do == UpgradeAction.Hold && effect.Seconds == 1f),
                "held 1 s");
            var turned = new ConditionNews { Turned = true };
            Assert.IsTrue(turned.Turned && !new ConditionNews().Turned, "a landed or plainly blocked blow is not a turned one");
        }

        [Test]
        public void GoesUpIsAskedOfTheEnemyAsFireMedicineFires()
        {
            var goesUp = Upgrade.ById(PyreTree.GoesUp).Effects;
            Assert.AreEqual(1, goesUp.Length, "no second bang, no doses laid back");
            Assert.GreaterOrEqual(ReactionTable.PairOf(goesUp[0].Reaction), 0, "it names a real reaction");
            var full = Carrying();
            full.Lay(AfflictionKind.Poison, full.DepthOf(AfflictionKind.Poison), 0f, 1f, LaySource.Reaction);
            Assert.AreEqual(2f, ReactionRules.Boost(goesUp, goesUp[0].Reaction, full, out bool spares), 1e-5f, "twice as hard");
            Assert.IsTrue(spares, "spending nothing");
            var half = Carrying(AfflictionKind.Poison);
            Assert.AreEqual(1f, ReactionRules.Boost(goesUp, goesUp[0].Reaction, half, out spares), 1e-5f, "not at full depth");
            Assert.IsFalse(spares);
            Assert.AreEqual(1f, ReactionRules.Boost(goesUp, "react.steam", full, out _), 1e-5f, "another reaction");
        }
    }
}
