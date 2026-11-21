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

        private static float Damage(AfflictionSet set, float seconds)
        {
            float dealt = 0f;
            int steps = UnityEngine.Mathf.RoundToInt(seconds / Frame);
            for (int i = 0; i < steps; i++) dealt += set.Tick(Frame);
            return dealt;
        }

        [Test]
        public void BurnsBiteTwiceAsHardWhileItGlows()
        {
            var glowing = NewEnemy();
            var plain = NewEnemy();
            glowing.Ignite(4f, 2, LaySource.Spell);
            plain.Ignite(4f, 2, LaySource.Spell);
            glowing.Conditions.Set(new ConditionSpec { Id = "test.glow", Seconds = 5f, BitesHarder = AfflictionKind.Burn, BiteScale = 2f }, null, 1);
            float hot = Damage(glowing, 2f), cold = Damage(plain, 2f);
            Assert.Greater(cold, 0f);
            Assert.AreEqual(cold * 2f, hot, cold * 0.01f + 1e-4f);
            Assert.AreEqual(1f, glowing.Conditions.BiteScale(AfflictionKind.Poison, false), "only the kind it names");
        }

        [Test]
        public void AHarderBiteWhileFrozenWaitsForTheFreeze()
        {
            var set = NewEnemy();
            set.Conditions.Set(new ConditionSpec { Id = "test.cold", Seconds = 30f, BitesHarder = AfflictionKind.Poison, BiteScale = 2f, BitesHarderFrozen = true }, null, 1);
            Assert.AreEqual(1f, set.Conditions.BiteScale(AfflictionKind.Poison, false));
            Assert.AreEqual(2f, set.Conditions.BiteScale(AfflictionKind.Poison, true));
        }

        [Test]
        public void RedHotAndColdDamageCarryTheirHarderBites()
        {
            var all = Upgrade.All.SelectMany(upgrade => upgrade.Effects.Concat(upgrade.Shapes == null ? Enumerable.Empty<UpgradeEffect>() : upgrade.Shapes.Where(shape => shape?.Effects != null).SelectMany(shape => shape.Effects)))
                .Where(effect => effect.State != null).ToList();
            foreach (var id in new[] { Clover.Player.Trees.Reactions.Glowing, RedHotDuo.Slag, RedHotDuo.Shirt })
            {
                var spec = all.First(effect => effect.State.Id == id).State;
                Assert.AreEqual(AfflictionKind.Burn, spec.BitesHarder, id);
                Assert.AreEqual(2f, spec.BiteScale, 1e-5f, id);
                Assert.IsFalse(spec.BitesHarderFrozen, id);
            }
            var cold = all.First(effect => effect.State.Id == VenomRimeDuo.ColdDamage).State;
            Assert.AreEqual(AfflictionKind.Poison, cold.BitesHarder);
            Assert.AreEqual(2f, cold.BiteScale, 1e-5f);
            Assert.IsTrue(cold.BitesHarderFrozen);
            var frost = all.First(effect => effect.State.Id == VenomRimeDuo.FrostOverFrost).State;
            Assert.AreEqual(1f, frost.BiteScale, 1e-5f, "Frost Over Frost bites as written");
        }

        private sealed class Afflicted : IAfflictable
        {
            public AfflictionSet Afflictions { get; } = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
        }

        [Test]
        public void NightSunHealsOneForEachBurnOnTheEnemyStruck()
        {
            var enemy = new Afflicted();
            Assert.AreEqual(0, PlayerUpgrades.DepthOn(AfflictionKind.Burn, enemy), "no Burn, no heal");
            enemy.Afflictions.Ignite(4f, 3, LaySource.Spell);
            Assert.AreEqual(3, PlayerUpgrades.DepthOn(AfflictionKind.Burn, enemy));
            Assert.AreEqual(0, PlayerUpgrades.DepthOn(AfflictionKind.Burn, null));
            Assert.AreEqual(0, PlayerUpgrades.DepthOn(AfflictionKind.None, enemy));
            var shape = Upgrade.ById(SmoulderDuo.Shape).Shapes.First(one => one.Name == "Night Sun");
            var heal = shape.Effects.Single(effect => effect.Do == UpgradeAction.Heal);
            Assert.AreEqual(AfflictionKind.Burn, heal.PerDepthOf);
            Assert.AreEqual(1f, heal.Amount, 1e-5f);
        }

        [Test]
        public void AFreezeForASetTimeTakesWhateverTheChillAndRunsItsOwnTime()
        {
            var set = NewEnemy();
            Assert.IsTrue(set.FreezeFor(1.5f), "frozen with no Chill at all");
            Assert.IsTrue(set.IsFrozen);
            Assert.AreEqual(1.5f, set.FrozenLeft, 1e-4f);
            Assert.AreEqual(Afflictions.Depth(AfflictionKind.Chill), set.Stacks(AfflictionKind.Chill), "the Chill is full, ready for a heavy blow");
            Assert.IsTrue(set.FreezeFor(0.5f));
            Assert.AreEqual(1.5f, set.FrozenLeft, 1e-4f, "a shorter freeze does not cut a longer one");
            var stone = new AfflictionSet { CanFreeze = false, Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            Assert.IsFalse(stone.FreezeFor(1.5f), "what cannot be frozen is not");
        }

        [Test]
        public void AShakenEnemysFreezeIsHeldTwiceAsLong()
        {
            Afflictions.Rules.FreezeLongerWith(AfflictionKind.Tremor, 2f);
            var shaken = NewEnemy();
            Lay(shaken, AfflictionKind.Tremor);
            var plain = NewEnemy();
            shaken.FreezeFor(1.5f);
            plain.FreezeFor(1.5f);
            Assert.AreEqual(3f, shaken.FrozenLeft, 1e-4f);
            Assert.AreEqual(1.5f, plain.FrozenLeft, 1e-4f);
            var chilled = NewEnemy();
            Lay(chilled, AfflictionKind.Tremor);
            chilled.Lay(AfflictionKind.Chill, 9, 0f, 1f, LaySource.Spell);
            var cold = NewEnemy();
            cold.Lay(AfflictionKind.Chill, 9, 0f, 1f, LaySource.Spell);
            Assert.IsTrue(chilled.IsFrozen && cold.IsFrozen);
            Assert.AreEqual(cold.FrozenLeft * 2f, chilled.FrozenLeft, 1e-4f, "a freeze by Chill is held longer too");

            var book = new RuleBook();
            Assert.IsTrue(book.Apply(Upgrade.ById(FrostHeaveDuo.WinterInTheBones)));
            Assert.AreEqual(2f, book.Afflictions.FreezeScale(Mask(AfflictionKind.Tremor)), 1e-5f, "Winter in the Bones");
            Assert.AreEqual(1f, book.Afflictions.FreezeScale(Mask(AfflictionKind.Chill)), 1e-5f);
            var spear = Upgrade.ById(SleetChargeDuo.Shape).Shapes.First(shape => shape.Name == "Glass Spear").Effects;
            Assert.IsTrue(spear.Any(effect => effect.Do == UpgradeAction.Freeze && effect.Ranks == EnemyRank.Plain && UnityEngine.Mathf.Approximately(effect.Seconds, 1.5f)), "Glass Spear freezes 1.5 s");
        }

        [Test]
        public void RunningSilverPassesOnEveryDoseAndNotchItCarried()
        {
            var shape = Upgrade.ById(QuicksilverDuo.Shape).Shapes.First(one => one.Name == "Running Silver");
            var passes = shape.Effects.Where(effect => effect.On == UpgradeTrigger.ConditionDied && effect.Do == UpgradeAction.Pass).ToList();
            Assert.AreEqual(2, passes.Count);
            foreach (var kind in new[] { AfflictionKind.Poison, AfflictionKind.Sundered })
            {
                var pass = passes.Single(effect => effect.Affliction == kind);
                Assert.AreEqual(kind, pass.Needs, "its depth is read from what the dead one carried");
                Assert.IsTrue(pass.CarriesDepth, "and laid whole");
                Assert.AreEqual(1, pass.Stacks);
            }
        }

        [Test]
        public void HeldBitesLandAllAtOnceDoubledWhenTheHoldEnds()
        {
            var held = NewEnemy();
            var plain = NewEnemy();
            held.Lay(AfflictionKind.Poison, 3, 30f, 1f, LaySource.Spell);
            plain.Lay(AfflictionKind.Poison, 3, 30f, 1f, LaySource.Spell);
            held.Conditions.Set(new ConditionSpec { Id = "test.sap", Seconds = 3f, HoldsBites = AfflictionKind.Poison, HeldScale = 2f }, null, 1);
            float during = Damage(held, 2.9f), plainDuring = Damage(plain, 2.9f);
            Assert.Greater(plainDuring, 0f);
            Assert.AreEqual(0f, during, 1e-5f, "nothing bites while it is held");
            Assert.Greater(held.HeldBite, 0f);
            float landed = Damage(held, 0.3f);
            Assert.AreEqual(0f, held.HeldBite, 1e-5f, "all of it let go");
            Assert.GreaterOrEqual(landed, plainDuring * 2f - 1e-3f, "every held bite lands at once, doubled");

            Afflictions.Rules.HeldBitesStronger(0.15f);
            var bright = NewEnemy();
            bright.Lay(AfflictionKind.Poison, 3, 30f, 1f, LaySource.Spell);
            bright.Conditions.Set(new ConditionSpec { Id = "test.sap", Seconds = 3f, HoldsBites = AfflictionKind.Poison, HeldScale = 2f }, null, 1);
            Damage(bright, 2.5f);
            float heldBefore = bright.HeldBite;
            for (int i = 0; i < 20 && bright.HeldBite > 0f; i++)
            {
                heldBefore = bright.HeldBite;
                bright.Tick(Frame);
            }
            Assert.AreEqual(heldBefore * 2f * 1.15f, bright.BitDamage, 1e-3f, "Bright Venom: 15% harder");
        }

        [Test]
        public void BurningSapAndItsShapesHoldThePoisonAndBrightVenomStrengthensIt()
        {
            var all = Upgrade.All.SelectMany(upgrade => upgrade.Effects.Concat(upgrade.Shapes == null ? Enumerable.Empty<UpgradeEffect>() : upgrade.Shapes.Where(shape => shape?.Effects != null).SelectMany(shape => shape.Effects)))
                .Where(effect => effect.State != null).ToList();
            foreach (var id in new[] { Clover.Player.Trees.Reactions.SapWaits, BurningSapDuo.Found, BurningSapDuo.Blistered })
            {
                var spec = all.First(effect => effect.State.Id == id).State;
                Assert.AreEqual(AfflictionKind.Poison, spec.HoldsBites, id);
                Assert.AreEqual(2f, spec.HeldScale, 1e-5f, id);
            }
            var book = new RuleBook();
            Assert.IsTrue(book.Apply(Upgrade.ById(BurningSapDuo.BrightVenom)));
            Assert.AreEqual(0.15f, book.Afflictions.HeldStronger, 1e-5f);
        }

        [Test]
        public void IchorsBleedingBeatsThePoisonTwiceAsFastWithoutHasteningItsFade()
        {
            var bleeding = NewEnemy();
            var plain = NewEnemy();
            bleeding.Lay(AfflictionKind.Poison, 3, 30f, 1f, LaySource.Spell);
            plain.Lay(AfflictionKind.Poison, 3, 30f, 1f, LaySource.Spell);
            bleeding.Conditions.Set(new ConditionSpec { Id = "test.bleed", Seconds = 4f, Quickens = AfflictionKind.Poison, QuickenBy = 2f }, null, 1);
            Assert.AreEqual(2f, bleeding.Conditions.Quickening(AfflictionKind.Poison), 1e-5f);
            Assert.AreEqual(1f, bleeding.Conditions.Quickening(AfflictionKind.Burn), 1e-5f);
            float fast = Damage(bleeding, 3.95f), slow = Damage(plain, 3.95f);
            Assert.Greater(slow, 0f);
            Assert.AreEqual(slow * 2f, fast, slow * 0.3f, "about twice the bites in the same time");
            Assert.AreEqual(plain.SecondsLeft(AfflictionKind.Poison), bleeding.SecondsLeft(AfflictionKind.Poison), 0.05f, "its fading runs as written");

            var ichor = Upgrade.ById(QuicksilverDuo.Ichor);
            var bleed = ichor.Effects.Single(effect => effect.State != null && effect.State.Id == QuicksilverDuo.Bleeding).State;
            Assert.AreEqual(4f, bleed.Seconds, 1e-5f);
            Assert.AreEqual(AfflictionKind.Poison, bleed.Quickens);
            Assert.AreEqual(2f, bleed.QuickenBy, 1e-5f);
            Assert.IsTrue(ichor.Effects.Any(effect => effect.On == UpgradeTrigger.ConditionStruck && effect.Condition == QuicksilverDuo.Bleeding && effect.Do == UpgradeAction.Afflict), "each weapon blow adds a dose");
            Assert.IsTrue(ichor.Effects.Any(effect => effect.On == UpgradeTrigger.ConditionEnded && effect.Condition == QuicksilverDuo.Bleeding && effect.Do == UpgradeAction.SetOff), "every dose at once at the end");
            Assert.AreEqual(0.5f, ichor.Spell.MissedWait, 1e-5f, "half the wait when nothing strikes");
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
