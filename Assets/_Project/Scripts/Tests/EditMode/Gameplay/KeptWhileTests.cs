using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Growth;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// An affliction kept from fading, or slowed, while its enemy carries another (the ledger's fades-never,
    /// standin_clearance_plan.md pass 1): the rule itself, what it does on an enemy's set, and the six marks that did
    /// nothing until it was built (Permafrost, Neither Wins, Cinnabar, Long Memory, Brine, Rust in the Curse). The rules are the static the
    /// enemies read, which these tests leave clear.
    /// </summary>
    public class KeptWhileTests
    {
        private const float Frame = 0.1f;

        [SetUp]
        [TearDown]
        public void LeaveTheAfflictionsAsWritten() => Afflictions.Rules.Clear();

        private static AfflictionSet NewEnemy() => new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };

        private static void Lay(AfflictionSet set, AfflictionKind kind) => set.Lay(kind, 1, 0f, 1f, LaySource.Spell);

        private static void Run(AfflictionSet set, float seconds)
        {
            int steps = UnityEngine.Mathf.RoundToInt(seconds / Frame);
            for (int i = 0; i < steps; i++) set.Tick(Frame);
        }

        private static int Mask(params AfflictionKind[] kinds)
        {
            int mask = 0;
            foreach (var kind in kinds) mask |= 1 << (int)kind;
            return mask;
        }

        [Test]
        public void TheRuleKeepsOnlyWhileItsPartnerIsCarried()
        {
            var rules = new AfflictionRules();
            rules.KeepWhile(AfflictionKind.Tremor, AfflictionKind.Chill);
            Assert.IsTrue(rules.Any);
            Assert.IsTrue(rules.KeepsWith(AfflictionKind.Tremor, Mask(AfflictionKind.Tremor, AfflictionKind.Chill)));
            Assert.IsFalse(rules.KeepsWith(AfflictionKind.Tremor, Mask(AfflictionKind.Tremor)), "no Chill, no keeping");
            Assert.IsFalse(rules.KeepsWith(AfflictionKind.Chill, Mask(AfflictionKind.Tremor, AfflictionKind.Chill)), "one way only");
            Assert.IsFalse(rules.Keeps(AfflictionKind.Tremor), "not kept outright");
            rules.FadeSlowerWhile(AfflictionKind.Soaked, AfflictionKind.Sundered, 0.5f);
            Assert.AreEqual(0.5f, rules.FadePace(AfflictionKind.Soaked, Mask(AfflictionKind.Soaked, AfflictionKind.Sundered)), 1e-5f);
            Assert.AreEqual(1f, rules.FadePace(AfflictionKind.Soaked, Mask(AfflictionKind.Soaked)), 1e-5f);
            var copy = new AfflictionRules();
            copy.CopyFrom(rules);
            Assert.IsTrue(copy.KeepsWith(AfflictionKind.Tremor, Mask(AfflictionKind.Chill)));
            Assert.AreEqual(0.5f, copy.FadePace(AfflictionKind.Soaked, Mask(AfflictionKind.Sundered)), 1e-5f);
            rules.Clear();
            Assert.IsFalse(rules.KeepsWith(AfflictionKind.Tremor, Mask(AfflictionKind.Chill)));
            Assert.AreEqual(1f, rules.FadePace(AfflictionKind.Soaked, Mask(AfflictionKind.Sundered)), 1e-5f);
        }

        [Test]
        public void OnAnEnemyTheKeptKindOutlastsItsTimeOnlyBesideItsPartner()
        {
            Afflictions.Rules.Keep(AfflictionKind.Chill);
            Afflictions.Rules.KeepWhile(AfflictionKind.Tremor, AfflictionKind.Chill);
            var chilled = NewEnemy();
            Lay(chilled, AfflictionKind.Chill);
            Lay(chilled, AfflictionKind.Tremor);
            var plain = NewEnemy();
            Lay(plain, AfflictionKind.Tremor);
            Run(chilled, 60f);
            Run(plain, 60f);
            Assert.IsTrue(chilled.Has(AfflictionKind.Tremor), "Permafrost: Tremor does not fade from a chilled enemy");
            Assert.IsFalse(plain.Has(AfflictionKind.Tremor), "with no Chill it fades as written");
        }

        [Test]
        public void ABurnIsKeptBesideItsPartnerToo()
        {
            Afflictions.Rules.Keep(AfflictionKind.Chill);
            Afflictions.Rules.KeepWhile(AfflictionKind.Burn, AfflictionKind.Chill);
            var set = NewEnemy();
            set.Ignite(2f, 1, LaySource.Spell);
            Lay(set, AfflictionKind.Chill);
            Run(set, 30f);
            Assert.IsTrue(set.Burn.IsBurning, "Neither Wins: the burn goes on while the Chill lasts");
        }

        [Test]
        public void ASlowedKindFadesAtItsRulesPace()
        {
            Afflictions.Rules.Keep(AfflictionKind.Sundered);
            Afflictions.Rules.FadeSlowerWhile(AfflictionKind.Soaked, AfflictionKind.Sundered, 0.5f);
            var notched = NewEnemy();
            Lay(notched, AfflictionKind.Sundered);
            Lay(notched, AfflictionKind.Soaked);
            var plain = NewEnemy();
            Lay(plain, AfflictionKind.Soaked);
            float before = notched.SecondsLeft(AfflictionKind.Soaked);
            Assert.AreEqual(before, plain.SecondsLeft(AfflictionKind.Soaked), 1e-4f);
            Run(notched, 1f);
            Run(plain, 1f);
            Assert.AreEqual(0.5f, before - notched.SecondsLeft(AfflictionKind.Soaked), 0.02f, "Brine: half a second gone in a second");
            Assert.AreEqual(1f, before - plain.SecondsLeft(AfflictionKind.Soaked), 0.02f);
        }

        [Test]
        public void WhileItRustsNotchesRunPastThreeToFive()
        {
            var set = NewEnemy();
            int usual = Afflictions.Depth(AfflictionKind.Sundered);
            set.Lay(AfflictionKind.Sundered, 9, 0f, 1f, LaySource.Spell);
            Assert.AreEqual(usual, set.Stacks(AfflictionKind.Sundered), "three at most as written");
            set.Conditions.Set(new ConditionSpec { Id = "test.rust", Seconds = 10f, Deepens = AfflictionKind.Sundered, DeepensBy = 2 }, null, 1);
            Assert.AreEqual(usual + 2, set.DepthOf(AfflictionKind.Sundered));
            Assert.AreEqual(Afflictions.Depth(AfflictionKind.Soaked), set.DepthOf(AfflictionKind.Soaked), "only the kind it names");
            var result = set.Lay(AfflictionKind.Sundered, 1, 0f, 1f, LaySource.Spell);
            Assert.AreEqual(usual + 1, set.Stacks(AfflictionKind.Sundered));
            Assert.IsFalse(result.ReachedFull, "four is not full while it rusts");
            result = set.Lay(AfflictionKind.Sundered, 5, 0f, 1f, LaySource.Spell);
            Assert.AreEqual(usual + 2, set.Stacks(AfflictionKind.Sundered), "five at most");
            Assert.IsTrue(result.ReachedFull, "full at five, where the plate falls");
        }

        [Test]
        public void AKindKeptForGoodNeverFadesFromThatEnemyUntilItIsCleared()
        {
            var stone = NewEnemy();
            var other = NewEnemy();
            Lay(stone, AfflictionKind.Tremor);
            Lay(stone, AfflictionKind.Cursed);
            Lay(other, AfflictionKind.Tremor);
            stone.KeepForGood(AfflictionKind.Tremor);
            stone.KeepForGood(AfflictionKind.Cursed);
            Run(stone, 120f);
            Run(other, 120f);
            Assert.IsTrue(stone.Has(AfflictionKind.Tremor) && stone.Has(AfflictionKind.Cursed), "kept two minutes and on");
            Assert.IsFalse(other.Has(AfflictionKind.Tremor), "only on the enemy turned to stone");
            Assert.AreEqual(1, stone.Spend(AfflictionKind.Tremor, 1), "it can still be spent");
            stone.Clear();
            Assert.IsFalse(stone.KeptForGood(AfflictionKind.Cursed), "a new life forgets it");
        }

        [Test]
        public void StoneDoesNotForgiveKeepsTremorAndCurseAtEachClimbsEnd()
        {
            var effects = Upgrade.ById(TurnedToStoneDuo.StoneDoesNotForgive).Effects;
            foreach (var climbed in new[] { Clover.Player.Trees.Reactions.StoneClimbed, TurnedToStoneDuo.KingsMen + ".climbed", TurnedToStoneDuo.GraveMarker + ".climbed" })
                foreach (var kind in new[] { AfflictionKind.Tremor, AfflictionKind.Cursed })
                    Assert.IsTrue(effects.Any(effect => effect.On == UpgradeTrigger.ConditionEnded && effect.Condition == climbed && effect.Do == UpgradeAction.KeepForGood && effect.Affliction == kind),
                        climbed + " " + kind);
            // The climbs it answers are the ones the stone follows.
            Assert.IsTrue(Upgrade.All.SelectMany(upgrade => upgrade.Effects).Any(effect => effect.On == UpgradeTrigger.ConditionEnded && effect.Condition == Clover.Player.Trees.Reactions.StoneClimbed
                && effect.Do == UpgradeAction.Condition && effect.State != null && effect.State.Id == Clover.Player.Trees.Reactions.Stone));
        }

        [Test]
        public void ASlowSetByTheStacksCountedIsFifteenPercentAStone()
        {
            var spec = new ConditionSpec { Id = "test.heap", Seconds = 4f, Pace = 0.7f, SlowPerStack = 0.15f };
            Assert.AreEqual(0.85f, ConditionCaster.PaceFor(spec, 1), 1e-5f);
            Assert.AreEqual(0.55f, ConditionCaster.PaceFor(spec, 3), 1e-5f);
            Assert.AreEqual(0.05f, ConditionCaster.PaceFor(spec, 99), 1e-5f, "never stopped dead");
            Assert.AreEqual(-1f, ConditionCaster.PaceFor(new ConditionSpec { Pace = 0.7f }, 3), "a spec with no per-stack slow keeps its own pace");
            var set = NewEnemy();
            set.Conditions.Set(spec, null, 1, null, false, false, 1f, false, false, ConditionCaster.PaceFor(spec, 3));
            Assert.AreEqual(0.55f, set.Conditions.Pace, 1e-4f, "three stones, 45% slower");
            var cairn = Upgrade.All.SelectMany(upgrade => upgrade.Effects).Single(effect => effect.State != null && effect.State.Id == EarthTree.Heaped);
            Assert.IsTrue(cairn.PerStack);
            Assert.AreEqual(0.15f, cairn.State.SlowPerStack, 1e-5f);
            var bronze = Upgrade.ById(MetalTree.PouredBronze).Effects.Single(effect => effect.Do == UpgradeAction.Hold);
            Assert.IsTrue(bronze.PerStack, "Poured Bronze holds for each notch");
            Assert.AreEqual(0.4f, bronze.Seconds, 1e-5f);
        }

        [Test]
        public void RustAndItsShapesLetTheNotchesRunDeeper()
        {
            var rusting = Upgrade.All.SelectMany(upgrade => upgrade.Effects.Concat(upgrade.Shapes == null ? Enumerable.Empty<UpgradeEffect>() : upgrade.Shapes.Where(shape => shape?.Effects != null).SelectMany(shape => shape.Effects)))
                .Where(effect => effect.State != null && effect.State.Keeps == AfflictionKind.Sundered && effect.State.While == AfflictionKind.Soaked).ToList();
            Assert.GreaterOrEqual(rusting.Count, 3, "the plain Rust and its two shapes");
            Assert.IsTrue(rusting.All(effect => effect.State.Deepens == AfflictionKind.Sundered && effect.State.DeepensBy == 2));
        }

        [Test]
        public void TheSixMarksWriteTheirRules()
        {
            var book = new RuleBook();
            Assert.IsTrue(book.Apply(Upgrade.ById(FrostHeaveDuo.Permafrost)));
            Assert.IsTrue(book.Apply(Upgrade.ById(ThermalShockDuo.NeitherWins)));
            Assert.IsTrue(book.Apply(Upgrade.ById(QuicksilverDuo.Cinnabar)));
            Assert.IsTrue(book.Apply(Upgrade.ById(TheGrudgeDuo.LongMemory)));
            Assert.IsTrue(book.Apply(Upgrade.ById(RustDuo.Brine)));
            Assert.IsTrue(book.Apply(Upgrade.ById(BlackChainDuo.RustInTheCurse)));
            var rules = book.Afflictions;
            Assert.IsTrue(rules.KeepsWith(AfflictionKind.Tremor, Mask(AfflictionKind.Chill)), "Permafrost");
            Assert.IsTrue(rules.KeepsWith(AfflictionKind.Burn, Mask(AfflictionKind.Chill)), "Neither Wins, the Burn");
            Assert.IsTrue(rules.KeepsWith(AfflictionKind.Chill, Mask(AfflictionKind.Burn)), "Neither Wins, the Chill");
            Assert.IsTrue(rules.KeepsWith(AfflictionKind.Sundered, Mask(AfflictionKind.Poison)), "Cinnabar");
            Assert.IsTrue(rules.KeepsWith(AfflictionKind.Cursed, Mask(AfflictionKind.Static)), "Long Memory");
            Assert.IsTrue(rules.KeepsWith(AfflictionKind.Sundered, Mask(AfflictionKind.Cursed)), "Rust in the Curse");
            Assert.AreEqual(0.5f, rules.FadePace(AfflictionKind.Soaked, Mask(AfflictionKind.Sundered)), 1e-5f, "Brine");
        }
    }
}
