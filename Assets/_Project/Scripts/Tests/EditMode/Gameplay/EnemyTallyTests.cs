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
    /// The counts and clocks the player's marks keep on each enemy (standin_clearance_plan.md pass 1): a clock that runs
    /// on each enemy apart, a count that comes round on every n-th time, the fixed places reused when full, cleared with
    /// the enemy's afflictions; and the marks that once stood in with chance or one shared clock now read them.
    /// </summary>
    public class EnemyTallyTests
    {
        [Test]
        public void AClockRunsOnEachEnemyApart()
        {
            var first = new EnemyTally();
            var second = new EnemyTally();
            Assert.IsTrue(first.Ready(7, 0f), "never started is ready");
            first.Start(7, 10f, 4f);
            Assert.IsFalse(first.Ready(7, 12f));
            Assert.AreEqual(2f, first.Left(7, 12f), 1e-5f);
            Assert.IsTrue(second.Ready(7, 12f), "the other elite's clock was never started");
            Assert.IsTrue(first.Ready(8, 12f), "another mark's clock on the same enemy is its own");
            Assert.IsTrue(first.Ready(7, 14f), "ready again when its 4 s are out");
        }

        [Test]
        public void ACountComesRoundOnEveryNthTime()
        {
            var tally = new EnemyTally();
            var came = new List<bool>();
            for (int i = 0; i < 10; i++) came.Add(tally.AddAndCome(3, 5));
            CollectionAssert.AreEqual(new[] { false, false, false, false, true, false, false, false, false, true }, came, "every fifth bite, exactly");
            Assert.AreEqual(10, tally.Count(3));
            Assert.AreEqual(0, tally.Count(4));
            Assert.IsTrue(tally.AddAndCome(4, 1), "one is every time");
            tally.Reset(3);
            Assert.AreEqual(0, tally.Count(3));
        }

        [Test]
        public void WhenFullTheStalestPlaceIsReused()
        {
            var tally = new EnemyTally();
            for (int key = 0; key < EnemyTally.Places; key++) tally.Add(key);
            tally.Add(0);
            tally.Add(EnemyTally.Places);
            Assert.AreEqual(1, tally.Count(EnemyTally.Places), "the ninth count found a place");
            Assert.AreEqual(2, tally.Count(0), "the count touched last is kept");
            Assert.AreEqual(0, tally.Count(1), "the one touched longest ago gave its place");

            for (int key = 0; key < EnemyTally.Places; key++) tally.Start(key, 0f, 10f + key);
            tally.Start(99, 1f, 5f);
            Assert.IsFalse(tally.Ready(99, 2f), "the ninth clock found a place");
            Assert.IsTrue(tally.Ready(0, 2f), "it took the place of the clock nearest its end");
            Assert.IsFalse(tally.Ready(1, 2f));
        }

        [Test]
        public void AnEnemysTallyIsClearedWithItsAfflictions()
        {
            var set = new AfflictionSet();
            set.Tally.Add(2);
            set.Tally.Start(2, 0f, 30f);
            set.Clear();
            Assert.AreEqual(0, set.Tally.Count(2));
            Assert.IsTrue(set.Tally.Ready(2, 1f));
        }

        private static IEnumerable<UpgradeEffect> EffectsOf(string id)
        {
            var upgrade = Upgrade.ById(id);
            Assert.IsNotNull(upgrade, id);
            var shapes = upgrade.Shapes == null ? Enumerable.Empty<UpgradeEffect>() : upgrade.Shapes.Where(shape => shape?.Effects != null).SelectMany(shape => shape.Effects);
            return upgrade.Effects.Concat(shapes);
        }

        private static IEnumerable<UpgradeEffect> EffectsOnCondition(string condition) =>
            Upgrade.All.SelectMany(upgrade => upgrade.Effects).Where(effect => effect.Condition == condition);

        [TestCase(NerveFireDuo.FiveThunders, 5)]
        [TestCase(SleetChargeDuo.GlassStorm, 2)]
        [TestCase(TheGrudgeDuo.AshAndThunder, 3)]
        [TestCase(TheBoltRunsTrueDuo.TheDiamondFalls, 3)]
        [TestCase(LiveIronDuo.ThreeSmiths, 3)]
        [TestCase(LoganStoneDuo.TheProphecyBroken, 4)]
        public void MarksThatCountedByChanceNowCountOnTheEnemy(string id, int every)
        {
            var counted = EffectsOf(id).Where(effect => effect.EveryOnEnemy > 1).ToList();
            Assert.AreEqual(1, counted.Count, id + " has one counted effect");
            Assert.AreEqual(every, counted[0].EveryOnEnemy, id);
            Assert.AreEqual(1f, counted[0].Chance, id + " leaves nothing to chance");
        }

        [Test]
        public void AFightsGrowthIsEmptiedAtARestFireAndNothingElseIs()
        {
            var folds = new Upgrade { Id = "t.folds", Tally = new TallySpec { Cap = 10, Kept = true, EmptiesAtRest = true } };
            var plates = new Upgrade { Id = "t.plates", Tally = new TallySpec { Cap = 3, Kept = true } };
            var plain = new Upgrade { Id = "t.plain" };
            var catalogue = new List<Upgrade> { folds, plates, plain };
            var counts = new[] { 7, 2, 0 };
            var kept = new Dictionary<string, int> { { folds.Id, 7 }, { plates.Id, 2 } };
            Assert.AreEqual(1, TallySpec.EmptyAtRest(catalogue, counts, kept));
            CollectionAssert.AreEqual(new[] { 0, 2, 0 }, counts, "the folds are gone, the plates stay");
            Assert.IsFalse(kept.ContainsKey(folds.Id), "nor do the folds come back with the next scene");
            Assert.AreEqual(2, kept[plates.Id]);
            Assert.AreEqual(0, TallySpec.EmptyAtRest(catalogue, counts, kept), "nothing left to empty");

            var folded = Upgrade.ById(ForgeTree.FoldedSteel);
            Assert.IsTrue(folded.Tally.EmptiesAtRest, "Folded Steel's folds last to the next rest fire");
            Assert.IsTrue(folded.Tally.Kept, "and from scene to scene until then");
        }

        [Test]
        public void TheClocksTheLedgerNamedRunOnEachEnemy()
        {
            Assert.IsTrue(EffectsOnCondition(Reactions.Nerves).Where(effect => effect.Do == UpgradeAction.Flinch && effect.Cooldown > 0f).All(effect => effect.CooldownPerEnemy),
                "an elite flinches once every 4 s on each elite");
            Assert.IsTrue(EffectsOnCondition(Reactions.Beads).Where(effect => effect.Cooldown > 0f).All(effect => effect.CooldownPerEnemy), "the half second is each enemy's");
            // The per-blow draw (the freeze's last warmth, StruckBy.Frozen, comes once and keeps no clock).
            var draws = EffectsOnCondition(Reactions.Draw).Where(effect => effect.Do == UpgradeAction.Heal && effect.On == UpgradeTrigger.ConditionStruck).ToList();
            Assert.AreEqual(2, draws.Count);
            Assert.IsTrue(draws.All(effect => effect.CooldownPerEnemy), "8 a second from each enemy");
            Assert.IsTrue(EffectsOf(BurningSapDuo.BaredToTheSun).Single().CooldownPerEnemy, "once on each enemy in a cast");
            Assert.IsTrue(EffectsOf(NerveFireDuo.Shape).Where(effect => effect.Do == UpgradeAction.Flinch && effect.Cooldown > 0f).All(effect => effect.CooldownPerEnemy));
            var silver = EffectsOf(QuicksilverDuo.Shape).Where(effect => effect.Do == UpgradeAction.Bite && effect.Cooldown > 0f).ToList();
            Assert.AreEqual(2, silver.Count, "both of Quicksilver's shapes bite on a blow");
            Assert.IsTrue(silver.All(effect => effect.CooldownPerEnemy), "as the plain Quicksilver");
        }
    }
}
