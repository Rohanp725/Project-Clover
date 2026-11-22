using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// A mark that asks for an enemy in a condition another mark set (the ledger's condition-needs,
    /// standin_clearance_plan.md pass 3): a shape's state known by what it is, the test of it after the blow that spent it,
    /// a condition that lasts only while another holds, a blow told to one condition alone, the shares of heavy blows and of
    /// weapon stagger, and the marks that now read the real state.
    /// </summary>
    public class ConditionNeedsTests
    {
        private static AfflictionSet NewEnemy(List<ConditionNews> heard = null)
        {
            var set = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            set.Conditions.Report = (who, news) => heard?.Add(news);
            return set;
        }

        [Test]
        public void AShapesStateIsFoundByWhatItIs()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.shards", Seconds = 5f, Is = Reactions.Cracked }, null, 1);
            Assert.IsTrue(enemy.Conditions.Holds("test.shards"), "by its own name");
            Assert.IsTrue(enemy.Conditions.Holds(Reactions.Cracked), "by what it is");
            Assert.IsFalse(enemy.Conditions.Holds(Reactions.Brittle));
            Assert.IsFalse(enemy.Conditions.Holds(""), "an empty name asks nothing that holds");
        }

        [Test]
        public void TheBlowThatSpentAStateStillFindsItUntilTheNextStep()
        {
            var enemy = NewEnemy();
            enemy.Conditions.Set(new ConditionSpec { Id = "test.plate", Seconds = 5f, EndsWith = ConditionBlow.Heavy, Is = Reactions.Brittle }, null, 1);
            enemy.Conditions.Struck(20f, true, false);
            Assert.IsFalse(enemy.Conditions.Holds(Reactions.Brittle), "the heavy blow cracked it");
            Assert.IsTrue(enemy.Conditions.Held(Reactions.Brittle), "but that blow landed on a brittle enemy");
            Assert.IsTrue(PlayerUpgrades.InState(Reactions.Brittle, enemy, null), "asked of the set as a mark asks");
            enemy.Tick(0.1f);
            Assert.IsFalse(enemy.Conditions.Held(Reactions.Brittle), "forgotten at the next step");
            Assert.IsFalse(PlayerUpgrades.InState(Reactions.Brittle, null, null), "no one, no state");
        }

        [Test]
        public void ABlowToldToOneConditionReachesOnlyThatOne()
        {
            var heard = new List<ConditionNews>();
            var enemy = NewEnemy(heard);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.crack", Seconds = 5f, EndsWith = ConditionBlow.Heavy, Is = Reactions.Cracked }, null, 1, null, true);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.rocking", Seconds = 5f, Blows = 3 }, null, 2, null, true);
            heard.Clear();
            Assert.AreEqual(1, enemy.Conditions.StrikeWhere(Reactions.Cracked, true));
            Assert.AreEqual(1, heard.Count, "only the crack is told");
            Assert.AreEqual(ConditionMoment.Struck, heard[0].Moment);
            Assert.IsTrue(heard[0].Heavy);
            Assert.AreEqual(Reactions.Cracked, heard[0].Is, "the news says what it is");
            Assert.IsFalse(enemy.Conditions.Holds(Reactions.Cracked), "a heavy blow spends the crack");
            Assert.IsTrue(enemy.Conditions.Holds("test.rocking"));
            Assert.AreEqual(0, enemy.Conditions.StrikeWhere(Reactions.Cracked, true), "nothing left to split");
        }

        [Test]
        public void AConditionKeptWhileAnotherEndsWithIt()
        {
            var heard = new List<ConditionNews>();
            var enemy = NewEnemy(heard);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.felled", Seconds = 1f, Is = Reactions.Felled }, null, 1);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.face", Seconds = 4f, WhileIn = Reactions.Felled }, null, 2);
            enemy.Tick(0.5f);
            Assert.IsTrue(enemy.Conditions.Holds("test.face"), "kept while it lies");
            enemy.Tick(0.6f);
            Assert.IsFalse(enemy.Conditions.Holds("test.felled"));
            enemy.Tick(0.1f);
            Assert.IsFalse(enemy.Conditions.Holds("test.face"), "gone once it is up, long before its own 4 s");
            Assert.IsTrue(heard.Any(news => news.Moment == ConditionMoment.Ended && news.Id == "test.face"), "its end is told");
        }

        [Test]
        public void HeavyBlowsAndWeaponStaggerTakeTheirOwnShares()
        {
            var enemy = NewEnemy();
            float weapon = enemy.DamageTakenMultiplier(false), heavy = enemy.DamageTakenMultiplier(false, true), spell = enemy.DamageTakenMultiplier(true, true);
            float weaponStagger = enemy.StaggerTakenFor(false), spellStagger = enemy.StaggerTakenFor(true);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.gasping", Seconds = 5f, Flags = ConditionFlags.HeavyTakenShare, Scale = 1.5f }, null, 1);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.lamp", Seconds = 5f, Flags = ConditionFlags.WeaponStaggerShare, Scale = 1.25f }, null, 2);
            Assert.AreEqual(heavy * 1.5f, enemy.DamageTakenMultiplier(false, true), 1e-4f, "half again from a heavy blow");
            Assert.AreEqual(weapon, enemy.DamageTakenMultiplier(false), 1e-4f, "a light blow as written");
            Assert.AreEqual(spell, enemy.DamageTakenMultiplier(true, true), 1e-4f, "a spell as written");
            Assert.AreEqual(weaponStagger * 1.25f, enemy.StaggerTakenFor(false), 1e-4f, "a quarter more balance broken by the weapon");
            Assert.AreEqual(spellStagger, enemy.StaggerTakenFor(true), 1e-4f, "a spell's stagger as written");
        }

        [Test]
        public void AFinisherIsToldApartFromAnyOtherHeavyBlow()
        {
            var heard = new List<ConditionNews>();
            var enemy = NewEnemy(heard);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.swollen", Seconds = 5f, EndsWith = ConditionBlow.Finisher }, null, 1, null, true);
            var heavy = new Clover.Gameplay.Hit(20f, 10f, UnityEngine.Vector3.forward, true);
            enemy.TakeFrom(heavy, null);
            Assert.IsTrue(enemy.Conditions.Holds("test.swollen"), "a plain heavy blow does not burst it");
            Assert.IsTrue(heard[heard.Count - 1].Heavy);
            Assert.IsFalse(heard[heard.Count - 1].Finisher);
            Assert.IsTrue(PlayerReactions.Answers(StruckBy.Heavy, heard[heard.Count - 1]));
            Assert.IsFalse(PlayerReactions.Answers(StruckBy.Finisher, heard[heard.Count - 1]));
            var finisher = new Clover.Gameplay.Hit(40f, 30f, UnityEngine.Vector3.forward, true, finisher: true);
            Assert.IsTrue(finisher.Finisher);
            enemy.TakeFrom(finisher, null);
            Assert.IsTrue(heard[heard.Count - 1].Finisher);
            Assert.IsTrue(PlayerReactions.Answers(StruckBy.Finisher, heard[heard.Count - 1]));
            Assert.IsTrue(PlayerReactions.Answers(StruckBy.Heavy, heard[heard.Count - 1]), "a finisher is heavy too");
            Assert.IsFalse(enemy.Conditions.Holds("test.swollen"), "the finisher bursts it");
        }

        [Test]
        public void TheFinisherMarksAnswerTheFinisherAlone()
        {
            var swollen = EveryEffect().Where(effect => effect.State != null && (effect.State.Id == Reactions.Swollen || effect.State.Is == Reactions.Swollen)).ToList();
            Assert.GreaterOrEqual(swollen.Count, 2, "the plain swelling and Drawn Under's");
            Assert.IsTrue(swollen.All(effect => effect.State.EndsWith == ConditionBlow.Finisher || effect.State.EndsWith == ConditionBlow.None), "only a finisher bursts it");
            var plain = ReactionTable.MarkFor(AfflictionPairs.Index(AfflictionKind.Soaked, AfflictionKind.Cursed));
            Assert.IsTrue(plain.Effects.Where(effect => effect.On == UpgradeTrigger.ConditionStruck).All(effect => effect.By == StruckBy.Finisher));
            var sowing = EffectsOf(LightningFlowerDuo.SowingThunder).Where(effect => effect.Do == UpgradeAction.Falling).ToList();
            Assert.AreEqual(1, sowing.Count(effect => effect.By == StruckBy.Heavy));
            Assert.AreEqual(1, sowing.Count(effect => effect.By == StruckBy.Finisher), "the finisher's second bolt");
        }

        [Test]
        public void AReactionForcedAloneNeedsOnlyOneOfItsPair()
        {
            var reported = new List<int>();
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => reported.Add(pair), () => 0) };
            int ring = AfflictionPairs.Index(AfflictionKind.Marked, AfflictionKind.Cursed);
            Assert.IsFalse(enemy.ForceReaction(ring, null, true), "carrying neither, nothing");
            enemy.Lay(AfflictionKind.Marked, 1, 0f, 1f, LaySource.Spell);
            Assert.IsFalse(enemy.ForceReaction(ring, null), "as written, both are needed");
            Assert.IsTrue(enemy.ForceReaction(ring, null, true), "alone, one is enough");
            Assert.AreEqual(1, reported.Count);
            Assert.AreEqual(ring, reported[0]);
            Assert.IsTrue(enemy.Has(AfflictionKind.Marked), "nothing is spent by the forcing");
            var dusk = EffectsOf(EclipseTree.DuskAndDawn).Single(effect => effect.Do == UpgradeAction.ForceReaction);
            Assert.IsTrue(dusk.Alone);
            var grip = EffectsOf(BlightTree.WastingGrip).Single(effect => effect.Do == UpgradeAction.ForceReaction);
            Assert.IsTrue(grip.Alone);
            Assert.AreEqual(AfflictionKind.None, grip.Needs, "either of the two");
        }

        [Test]
        public void AnEffectShattersOnlyWhatIsFrozen()
        {
            var warm = new Afflicted();
            Assert.AreEqual(0f, PlayerUpgrades.ShatterNow(warm), 1e-5f, "nothing to break");
            Assert.AreEqual(0f, PlayerUpgrades.ShatterNow(null), 1e-5f);
            var frozen = new Afflicted();
            Assert.IsTrue(frozen.Afflictions.FreezeFor(2f));
            float broke = PlayerUpgrades.ShatterNow(frozen);
            Assert.Greater(broke, 0f, "the ice breaks as under a heavy blow");
            Assert.IsFalse(frozen.Afflictions.IsFrozen);
            var core = EffectsOf(ColdShortDuo.BrittleToTheCore).Single();
            Assert.AreEqual(UpgradeAction.Shatter, core.Do);
            Assert.AreEqual(UpgradeTrigger.Reacted, core.On);
            Assert.IsFalse(core.NeedsFull, "asks for frozen, not a full Chill");
        }

        [Test]
        public void ACriticalOnlyConditionLetsEveryOtherBlowPass()
        {
            var heard = new List<ConditionNews>();
            var enemy = NewEnemy(heard);
            enemy.Conditions.Set(new ConditionSpec { Id = "test.leaf", Seconds = 6f, EndsWith = ConditionBlow.Weapon, Blows = 3, CriticalOnly = true }, null, 1, null, true);
            enemy.Conditions.Struck(10f, false, false);
            enemy.Conditions.Struck(10f, true, true, false, true);
            Assert.IsTrue(enemy.Conditions.Holds("test.leaf"), "a plain blow and a spell pass it by");
            Assert.AreEqual(0, heard.Count(news => news.Moment == ConditionMoment.Struck));
            enemy.Conditions.Struck(10f, false, false, false, true);
            Assert.AreEqual(1, heard.Count(news => news.Moment == ConditionMoment.Struck && news.Critical && news.Blows == 1), "the critical is counted and told");
            Assert.IsFalse(enemy.Conditions.Holds("test.leaf"), "and it spends it");
        }

        [Test]
        public void TheMarksThatMakeABlowCriticalSayHow()
        {
            var mirror = Upgrade.ById(HaloTree.Shape).Shapes.First(shape => shape.Name == "Burning Mirror").Effects;
            Assert.IsTrue(mirror.Any(effect => effect.State != null && (effect.State.Flags & ConditionFlags.TakesCritical) != 0), "the white-hot point makes the blow critical");
            Assert.IsFalse(mirror.Any(effect => effect.Do == UpgradeAction.Burst), "no stand-in burst of 10");
            var kindling = Upgrade.ById(KindlingPointDuo.Shape).Shapes.SelectMany(shape => shape.Effects).Where(effect => effect.Do == UpgradeAction.Condition && effect.On == UpgradeTrigger.Reacted).ToList();
            Assert.AreEqual(2, kindling.Count(effect => effect.State.Blows == 3 && effect.State.CriticalOnly), "both shapes count criticals alone");
            var hands = EffectsOf(KindlingPointDuo.HandsOfTheSun);
            var hand = hands.Single(effect => effect.Do == UpgradeAction.Condition).State;
            Assert.IsTrue((hand.Flags & ConditionFlags.TakesCritical) != 0);
            Assert.AreEqual(ConditionBlow.Weapon, hand.EndsWith, "it lets go at the first blow");
            Assert.IsTrue(hands.Any(effect => effect.On == UpgradeTrigger.ConditionStruck && effect.Condition == KindlingPointDuo.Hand && effect.Stacks == 2));
            var herb = Upgrade.ById(ShiningHerbDuo.Shape).Shapes.SelectMany(shape => shape.Effects).ToList();
            Assert.IsTrue(herb.Where(effect => effect.On == UpgradeTrigger.Reacted && effect.State != null).All(effect => effect.State.CriticalOnly), "picked or burst by a critical alone");
            Assert.IsTrue(herb.Any(effect => effect.State != null && effect.State.Id == ShiningHerbDuo.Picked && (effect.State.Flags & ConditionFlags.TakesCritical) != 0), "the next blow after the picking");
        }

        private sealed class Afflicted : IAfflictable
        {
            public AfflictionSet Afflictions { get; } = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
        }

        private static IEnumerable<UpgradeEffect> EveryEffect() => Upgrade.All.SelectMany(upgrade => upgrade.Effects
            .Concat(upgrade.Shapes == null ? Enumerable.Empty<UpgradeEffect>() : upgrade.Shapes.Where(shape => shape?.Effects != null).SelectMany(shape => shape.Effects)));

        private static UpgradeEffect[] EffectsOf(string id) => Upgrade.ById(id).Effects;

        [Test]
        public void EveryStateAMarkAsksForIsOneTheCatalogueSets()
        {
            var names = new HashSet<string>();
            foreach (var effect in EveryEffect())
                if (effect.State != null)
                {
                    names.Add(effect.State.Id);
                    if (!string.IsNullOrEmpty(effect.State.Is)) names.Add(effect.State.Is);
                }
            foreach (var effect in EveryEffect())
            {
                if (!string.IsNullOrEmpty(effect.InState)) Assert.IsTrue(names.Contains(effect.InState), effect.Owner.Id + " asks for " + effect.InState);
                if (effect.State != null && !string.IsNullOrEmpty(effect.State.WhileIn)) Assert.IsTrue(names.Contains(effect.State.WhileIn), effect.Owner.Id + " lasts while " + effect.State.WhileIn);
                if (effect.AnyMark) Assert.IsTrue(names.Contains(effect.Condition), effect.Owner.Id + " answers " + effect.Condition);
            }
        }

        [Test]
        public void EachShapeOfTheStatesSaysWhatItIs()
        {
            var shapes = EveryEffect().Where(effect => effect.State != null && !string.IsNullOrEmpty(effect.State.Is)).Select(effect => effect.State.Is).ToList();
            foreach (var state in new[] { Reactions.Cracked, Reactions.Brittle, Reactions.Glowing, Reactions.Draw, Reactions.IronLive, Reactions.Swollen, Reactions.Winded, Reactions.Rocking, Reactions.Felled, Reactions.Damp })
                Assert.IsTrue(shapes.Contains(state), "a shape's " + state);
            Assert.GreaterOrEqual(shapes.Count(state => state == Reactions.Brittle), 2, "Shed Plate and Snapped Blade");
            Assert.GreaterOrEqual(shapes.Count(state => state == Reactions.Glowing), 2, "Slag and The Burning Shirt");
        }

        [Test]
        public void TheMarksReadTheRealState()
        {
            var bell = EffectsOf(ColdShortDuo.GlassBell);
            Assert.AreEqual(2, bell.Length);
            Assert.IsTrue(bell.All(effect => effect.On == UpgradeTrigger.AnyHit && effect.InState == Reactions.Brittle), "Glass Bell: each weapon blow on a brittle enemy");

            var hot = EffectsOf(RedHotDuo.StrikeWhileHot);
            Assert.IsTrue(hot.All(effect => effect.On == UpgradeTrigger.HeavyHit && effect.InState == Reactions.Glowing), "Strike While Hot");
            var shortened = EffectsOf(RedHotDuo.RedShort);
            Assert.IsTrue(shortened.All(effect => effect.On == UpgradeTrigger.FinisherHit && effect.InState == Reactions.Glowing), "Red-Short: a finisher on the red-hot");
            Assert.IsTrue(shortened.Any(effect => effect.Do == UpgradeAction.Spend) && shortened.Any(effect => effect.DamagePerSpent == 15f));

            var hel = EffectsOf(StolenWarmthDuo.HelsHalf);
            Assert.IsTrue(hel.All(effect => effect.On == UpgradeTrigger.Kill && effect.InState == Reactions.Draw && effect.Needs == AfflictionKind.None), "Hel's Half");

            var smiths = EffectsOf(LiveIronDuo.ThreeSmiths).Single();
            Assert.AreEqual(Reactions.IronLive, smiths.InState);
            Assert.AreEqual(3, smiths.EveryOnEnemy);
            Assert.AreEqual(AfflictionKind.None, smiths.Needs);

            Assert.IsTrue(EffectsOf(DrownedWeightDuo.HeldUnder).All(effect => effect.On == UpgradeTrigger.PerfectGuard && effect.InState == Reactions.Swollen), "Held Under");

            var stone = EffectsOf(ThermalShockDuo.HotStoneColdWater);
            Assert.IsTrue(stone.Any(effect => effect.Do == UpgradeAction.StrikeCondition && effect.Condition == Reactions.Cracked && effect.InState == Reactions.Cracked));
            Assert.IsTrue(stone.Any(effect => effect.InState == ThermalShockDuo.Crazing && effect.Do == UpgradeAction.Burst), "Crazing's crack splits once");

            var breath = EffectsOf(BreathTakenDuo.ShortOfBreath);
            var gasping = breath.Single(effect => effect.Do == UpgradeAction.Condition).State;
            Assert.AreEqual(ConditionFlags.HeavyTakenShare, gasping.Flags);
            Assert.AreEqual(1.5f, gasping.Scale, 1e-5f);
            Assert.AreEqual(Reactions.Winded, gasping.WhileIn);
            Assert.IsTrue(breath.Any(effect => effect.On == UpgradeTrigger.HeavyHit && effect.InState == Reactions.Winded && effect.Do == UpgradeAction.CutCooldowns));
            Assert.IsTrue(EffectsOf(BreathTakenDuo.TheLastBreath).All(effect => effect.On == UpgradeTrigger.Kill && effect.InState == Reactions.Winded), "The Last Breath");

            var face = EffectsOf(LoganStoneDuo.NewFace).Single(effect => effect.Do == UpgradeAction.Condition);
            Assert.IsTrue(face.AnyMark);
            Assert.AreEqual(Reactions.Rocking, face.Condition);
            Assert.AreEqual(3, face.AtCount);
            Assert.AreEqual(Reactions.Felled, face.State.WhileIn);
            Assert.IsTrue((face.State.Flags & ConditionFlags.TakesCritical) != 0);

            var lamp = EffectsOf(ChokedampDuo.TheLampGoesOut).Single().State;
            Assert.AreEqual(ConditionFlags.WeaponStaggerShare, lamp.Flags);
            Assert.AreEqual(Reactions.Damp, lamp.WhileIn);
        }
    }
}
