using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The rest of pass 4 (standin_clearance_plan.md, combat moments): an enemy freezing, a condition ended on demand, a
    /// condition drifting on when its carrier dies, a ward's turn as a perfect guard, and a red attack turned.
    /// </summary>
    public class CombatMomentsTests
    {
        private static AfflictionSet Quiet() => new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };

        [Test]
        public void AFreezeEndsWhatEndsOnAFreezeAndSaysSo()
        {
            var set = Quiet();
            var heard = new System.Collections.Generic.List<ConditionNews>();
            set.Conditions.Report = (who, news) => heard.Add(news);
            set.Conditions.Set(new ConditionSpec { Id = "test.charging", Seconds = 3f, EndsOnFreeze = true }, null, -1, null, false, true);
            set.Conditions.Set(new ConditionSpec { Id = "test.stays", Seconds = 3f }, null, -1, null, false, true);
            Assert.AreEqual(float.MaxValue, set.FrozenFor, "not frozen");
            set.Lay(AfflictionKind.Chill, set.DepthOf(AfflictionKind.Chill), 0f, 1f, LaySource.Spell);
            Assert.IsTrue(set.IsFrozen);
            Assert.AreEqual(0f, set.FrozenFor, 1e-5f, "it has just frozen");
            var ended = heard.Where(news => news.Moment == ConditionMoment.Ended).ToArray();
            Assert.AreEqual(1, ended.Length, "only what ends on a freeze ended");
            Assert.AreEqual("test.charging", ended[0].Id);
            Assert.IsTrue(ended[0].Frozen, "told as the freeze's end");
            Assert.IsTrue(set.Conditions.Held("test.stays"));
            set.Tick(0.2f);
            Assert.AreEqual(0.2f, set.FrozenFor, 1e-4f);
        }

        [Test]
        public void AConditionEndedOnDemandIsToldAsItsEnd()
        {
            var set = Quiet();
            var heard = new System.Collections.Generic.List<ConditionNews>();
            set.Conditions.Report = (who, news) => heard.Add(news);
            set.Conditions.Set(new ConditionSpec { Id = "test.shape", Is = Reactions.Charging, Seconds = 3f }, null, -1, null, false, true);
            Assert.AreEqual(1, set.Conditions.EndNow(Reactions.Charging), "found by what it is");
            Assert.IsFalse(set.Conditions.Held("test.shape"));
            Assert.IsTrue(heard.Any(news => news.Moment == ConditionMoment.Ended && news.Id == "test.shape" && !news.Frozen));
            Assert.AreEqual(0, set.Conditions.EndNow(Reactions.Charging), "nothing left to end");
            var snap = Upgrade.ById(SleetChargeDuo.ColdSnap).Effects;
            Assert.IsTrue(snap.Any(effect => effect.Do == UpgradeAction.EndCondition && effect.Condition == Reactions.Charging), "Cold Snap ends the running charge");
            Assert.IsFalse(snap.Any(effect => effect.Do == UpgradeAction.SetOff), "no set-off beside it");
        }

        [Test]
        public void ADriftingConditionGoesWholeToAnother()
        {
            var dying = Quiet();
            int slot = dying.Conditions.Set(new ConditionSpec { Id = "test.cloud", Seconds = 8f, DriftsTo = 6f }, null);
            dying.Conditions.Struck(10f, false, false);
            var run = dying.Conditions.Run(slot);
            Assert.AreEqual(6f, run.DriftsTo, 1e-5f);
            var next = Quiet();
            Assert.GreaterOrEqual(next.Conditions.Adopt(run, null), 0);
            Assert.IsTrue(next.Conditions.Held("test.cloud"), "it carries the cloud now");
            Assert.AreEqual(run.Blows, next.Conditions.Run(0).Blows, "kept whole: its blows");
            Assert.AreEqual(run.Left, next.Conditions.Run(0).Left, 1e-5f, "and its time");
            var grudge = ReactionTable.MarkFor(AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Static));
            Assert.AreEqual(6f, grudge.Effects.First(effect => effect.State != null && effect.State.Id == Reactions.Cloud).State.DriftsTo, 1e-5f, "The Grudge's cloud drifts 6 m");
        }

        [Test]
        public void TheFreezeMarksAnswerTheFreezeAndTheShatter()
        {
            var fire = Upgrade.ById(ThermalShockDuo.FireUnderTheIce).Effects;
            Assert.IsTrue(fire.Any(effect => effect.On == UpgradeTrigger.AnyHit && effect.NeedsFrozen && effect.Needs == AfflictionKind.Burn && effect.Do == UpgradeAction.Shatter), "any weapon blow");
            Assert.IsTrue(fire.Any(effect => effect.On == UpgradeTrigger.SpellHit && effect.NeedsFrozen && effect.Do == UpgradeAction.Shatter), "or spell");
            var spring = Upgrade.ById(WinterSeedDuo.Shape).Shapes.First(shape => shape.Name == "Hard Spring").Effects;
            Assert.IsTrue(spring.Where(effect => effect.Do == UpgradeAction.Burst).All(effect => effect.On == UpgradeTrigger.ConditionStruck && effect.By == StruckBy.Shatter), "wakes only on a shatter");
            var frost = Upgrade.ById(VenomRimeDuo.Shape).Shapes.First(shape => shape.Name == "Frost Over Frost").Effects;
            Assert.IsTrue(frost.Any(effect => effect.State != null && effect.State.Id == VenomRimeDuo.FrostWatch && effect.State.EndsOnFreeze));
            Assert.AreEqual(2, frost.Count(effect => effect.Condition == VenomRimeDuo.FrostWatch && effect.By == StruckBy.Frozen && effect.Do == UpgradeAction.Hazard), "the rime spreads as it freezes");
            var shade = Upgrade.ById(StolenWarmthDuo.Shape).Shapes.First(shape => shape.Name == "Cold Shade").Effects;
            Assert.IsTrue(shade.Any(effect => effect.By == StruckBy.Shatter && effect.Do == UpgradeAction.Hazard), "the shade is left when it is shattered");
            Assert.IsFalse(shade.Any(effect => effect.On == UpgradeTrigger.ConditionDied));
        }

        [Test]
        public void AWardsTurnCountsAsAPerfectGuardWithSplinterWard()
        {
            var stats = new PlayerStats();
            Assert.IsFalse(PlayerUpgrades.WardIsPerfectGuard(stats));
            foreach (var mod in Upgrade.ById(IceTree.SplinterWard).Stats) stats.AddLearned(mod);
            Assert.IsTrue(PlayerUpgrades.WardIsPerfectGuard(stats));
        }

        [Test]
        public void TheCountersMastersAnswerOnlyARedAttackTurned()
        {
            Assert.IsTrue(PlayerUpgrades.RedJustTurned(10.5f, 10f));
            Assert.IsFalse(PlayerUpgrades.RedJustTurned(11.5f, 10f), "a second has passed");
            Assert.IsFalse(PlayerUpgrades.RedJustTurned(10f, -100f), "none turned");
            foreach (var branch in new[] { EarthTree.MountainStance, MetalTree.CircleOfSteel, DarkTree.GoneToAsh })
            {
                var master = Upgrade.ById(SpellBranch.MasterId(branch)).Effects;
                Assert.IsTrue(master.All(effect => effect.RedOnly), branch + "'s Master answers only a red attack");
                Assert.IsTrue(master.All(effect => effect.Cooldown < 20f), branch + ": no stand-in twenty seconds");
            }
        }
    }
}
