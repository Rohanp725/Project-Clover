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
        public void AnEnemyInItsWindupIsKnownFromItsStart()
        {
            var set = Quiet();
            var enemy = new Afflicted(set);
            Assert.IsFalse(PlayerUpgrades.InWindup(enemy, 99f), "showing nothing");
            set.NoteWindup(true, 0.1f);
            Assert.AreEqual(0f, set.WindupFor, 1e-5f, "counted from its start");
            set.NoteWindup(true, 0.2f);
            Assert.IsTrue(PlayerUpgrades.InWindup(enemy, 0.3f), "in the instant its attack mark shows");
            set.NoteWindup(true, 0.2f);
            Assert.IsFalse(PlayerUpgrades.InWindup(enemy, 0.3f), "past that instant");
            Assert.IsTrue(PlayerUpgrades.InWindup(enemy, 99f), "still in its wind-up");
            set.NoteWindup(false, 0.1f);
            Assert.IsFalse(PlayerUpgrades.InWindup(enemy, 99f), "the blow has come");
            var quake = Upgrade.ById(EarthTree.QuakeStep).Effects;
            Assert.IsTrue(quake.All(effect => effect.NeedsWindup > 0f), "only an enemy showing an attack is broken and staggered twice");
            Assert.IsTrue(quake.Any(effect => effect.Do == UpgradeAction.Burst && effect.Stagger == 30f), "double stagger");
            Assert.IsTrue(Upgrade.ById(SpellBranch.MasterId(EarthTree.QuakeStep)).Effects.All(effect => effect.NeedsWindup > 0f && effect.Cooldown < 18f));
            Assert.IsTrue(Upgrade.ById(SpellBranch.MasterId(DarkTree.DevouringMaw)).Effects.All(effect => effect.NeedsWindup > 0f));
            Assert.AreEqual(0.3f, Upgrade.ById(SpellBranch.MasterId(WaterTree.Whirlpool)).Effects.Single().NeedsWindup, 1e-5f, "in the instant its attack mark shows");
        }

        [Test]
        public void AMovementSpellStartedAsABlowWouldLandIsAPerfectDodge()
        {
            var set = Quiet();
            Assert.AreEqual(float.MaxValue, set.StrikeIn, "no blow shown");
            set.NoteWindup(true, 0f, 0.25f);
            Assert.AreEqual(0.25f, set.StrikeIn, 1e-5f);
            Assert.IsTrue(PlayerCombat.AboutToLand(set.StrikeIn, 0.3f), "about to land");
            set.NoteWindup(true, 0.1f, 0.6f);
            Assert.IsFalse(PlayerCombat.AboutToLand(set.StrikeIn, 0.3f), "too early");
            set.NoteWindup(false, 0.1f, 0.1f);
            Assert.AreEqual(float.MaxValue, set.StrikeIn, "the blow has come");
            var masters = Upgrade.All.Where(mark => mark.Effects != null && mark.Effects.Any(effect => effect.Do == UpgradeAction.PerfectDodgeNow)).ToList();
            Assert.AreEqual(8, masters.Count, "the eight Masters of the shared movement rule");
            foreach (var master in masters)
            {
                Assert.IsTrue(master.Effects.Where(effect => effect.Do == UpgradeAction.PerfectDodgeNow).All(effect => effect.BlowLanding == 0.3f && effect.On == UpgradeTrigger.SpellCast), master.Id);
                Assert.IsFalse(master.Effects.Any(effect => effect.Do == UpgradeAction.Mana && effect.Amount == 12f), master.Id + ": no stand-in perfect dodge's mana on every cast");
            }
            Assert.IsTrue(Upgrade.All.Any(mark => mark.Effects != null && mark.Effects.Any(effect => effect.Do == UpgradeAction.RaiseHeat && effect.Count == 5 && effect.BlowLanding > 0f)), "Wildfire Rush: Heat to five");
        }

        [Test]
        public void AShoveIsAMomentOnlyWhatAnswersAShoveHears()
        {
            Assert.IsTrue(ShoveRules.IsMoment(1f, 5f, -100f), "a Gust's metre");
            Assert.IsFalse(ShoveRules.IsMoment(0.1f, 5f, -100f), "a pull's drag each step");
            Assert.IsFalse(ShoveRules.IsMoment(3f, 5.1f, 5f), "the same shove");
            var set = Quiet();
            var heard = new System.Collections.Generic.List<ConditionNews>();
            set.Conditions.Report = (who, news) => heard.Add(news);
            set.Conditions.Set(new ConditionSpec { Id = "test.rooted", Seconds = 2f, EndsWith = ConditionBlow.Shove }, null, -1, null, true);
            set.Conditions.Set(new ConditionSpec { Id = "test.loose", Seconds = 30f }, null, -1, null, true);
            heard.Clear();
            Assert.AreEqual(2, set.Conditions.Shoved());
            Assert.IsTrue(heard.Count == 2 && heard.All(news => news.Shoved && news.Moment == ConditionMoment.Struck), "both told, as shoves");
            bool Running(string id) => Enumerable.Range(0, EnemyConditions.Slots).Any(slot => set.Conditions.Run(slot).Active && set.Conditions.Run(slot).Id == id);
            Assert.IsFalse(Running("test.rooted"), "torn out by the shove");
            Assert.IsTrue(Running("test.loose"));
            var shove = new ConditionNews { Shoved = true };
            Assert.IsTrue(PlayerReactions.Answers(StruckBy.Shove, shove));
            Assert.IsFalse(PlayerReactions.Answers(StruckBy.Weapon, shove), "a shove is no weapon blow");
            Assert.IsFalse(PlayerReactions.Answers(StruckBy.Any, shove));
            Assert.IsTrue(PlayerReactions.Answers(StruckBy.Shove, new ConditionNews { Heavy = true }), "a heavy blow shoves");
            Assert.IsFalse(PlayerReactions.Answers(StruckBy.Shove, new ConditionNews { Heavy = true, Spell = true }));
            var ill = ReactionTable.MarkFor(AfflictionPairs.Index(AfflictionKind.Poison, AfflictionKind.Unsteady)).Effects;
            Assert.IsTrue(ill.Where(effect => effect.On == UpgradeTrigger.ConditionStruck).All(effect => effect.By == StruckBy.Shove), "Ill Wind sheds on every shove");
            var wind = ReactionTable.MarkFor(AfflictionPairs.Index(AfflictionKind.Unsteady, AfflictionKind.Seeded)).Effects;
            Assert.IsTrue(wind.Where(effect => effect.On == UpgradeTrigger.ConditionStruck).All(effect => effect.By == StruckBy.Shove), "Windthrow tears out on every shove");
            var still = Upgrade.ById(IllWindDuo.NoStillAir).Effects;
            Assert.IsFalse(still.Any(effect => effect.Do == UpgradeAction.Spend), "the shove itself sheds");
        }

        [Test]
        public void TheDeathsAndBreaksOfAConditionAreAnsweredByWhatItIs()
        {
            Assert.GreaterOrEqual(ReactionTable.PairOf(DrownedWeightDuo.DrownedWeight), 0, "a real reaction");
            var debt = Upgrade.ById(DrownedWeightDuo.TheDebtPasses).Effects;
            Assert.IsTrue(debt.All(effect => effect.InState == Reactions.Swollen), "only a swollen enemy's burst");
            Assert.IsTrue(debt.Any(effect => effect.On == UpgradeTrigger.Kill && effect.Do == UpgradeAction.Heal && effect.Amount == 6f));
            foreach (var on in new[] { UpgradeTrigger.Kill, UpgradeTrigger.FinisherHit })
            {
                var swell = debt.Single(effect => effect.On == on && effect.Do == UpgradeAction.ForceReaction);
                Assert.IsTrue(swell.Alone && swell.EachWithin == 4f && swell.Count == 1 && swell.EachOther, on + ": the nearest the burst reaches swells");
            }
            var idol = Upgrade.ById(TurnedToStoneDuo.BrokenIdol).Effects;
            Assert.IsTrue(idol.All(effect => effect.On == UpgradeTrigger.FinisherHit && effect.InState == Reactions.Stone), "a finisher that breaks it out of stone");
            var set = Quiet();
            set.Conditions.Set(new ConditionSpec { Id = "test.shape.stone", Is = Reactions.Stone, Seconds = 1.5f, EndsWith = ConditionBlow.Heavy }, null);
            set.Conditions.Struck(30f, true, false, false, false, true);
            Assert.IsTrue(set.Conditions.Held(Reactions.Stone), "just broken out of a shape's stone is still known as stone");
        }

        [Test]
        public void TheBeatMastersAskTheCastNotEachEnemy()
        {
            foreach (var branch in new[] { PoisonTree.Miasma, PoisonTree.OneByOne, PoisonTree.ShareAlike })
            {
                var master = Upgrade.ById(SpellBranch.MasterId(branch)).Effects;
                Assert.IsTrue(master.Where(effect => effect.On != UpgradeTrigger.ConditionBeat).All(effect => effect.OnCastBeat && effect.OnBeat == 0f), branch + ": the cast is timed, not each enemy struck");
            }
            var snap = Upgrade.ById(SpellBranch.MasterId(PoisonTree.OneByOne)).Effects.Single();
            Assert.AreEqual(PlayerStat.BurstDamage, snap.Stat);
            Assert.AreEqual(0.5f, snap.Amount, 1e-5f, "every burst half again as hard");
            Assert.AreEqual(8f, PlayerUpgrades.BeatReach, 1e-5f);
            Assert.AreEqual(0.2f, PlayerUpgrades.BeatWindow, 1e-5f);
        }

        [Test]
        public void NothingResistsCarriesTheDischargeAtFullStrength()
        {
            var resists = Upgrade.ById(SleetChargeDuo.NothingResists).Effects.Single();
            Assert.AreEqual(UpgradeAction.Arc, resists.Do);
            Assert.AreEqual(1f, resists.Amount, 1e-5f, "the discharge's whole damage");
            Assert.GreaterOrEqual(resists.Count, 12, "every chilled enemy within 5 m of the last");
            Assert.AreEqual(AfflictionKind.Chill, resists.LeapsTo);
        }

        private sealed class Afflicted : IAfflictable
        {
            public Afflicted(AfflictionSet set) => Afflictions = set;
            public AfflictionSet Afflictions { get; }
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
