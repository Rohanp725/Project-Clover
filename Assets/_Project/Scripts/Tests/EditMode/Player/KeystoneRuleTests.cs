using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The rules written for the keystones finished on the night of 2026-11-16, and the marks that stand on them.</summary>
    public class KeystoneRuleTests
    {
        [SetUp]
        [TearDown]
        public void LeaveChillAsItWas()
        {
            Afflictions.ChillSlowOff = false;
            Afflictions.BrittleSeconds = 0f;
            Afflictions.BrittleShare = 0.2f;
            Afflictions.BuckledShare = 0f;
            Afflictions.FlashFreezeAt = 0;
        }

        [Test]
        public void WhatWasFrozenStaysBrittleForAWhile()
        {
            Afflictions.BrittleSeconds = 5f;
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Chill, 5);
            Assert.IsTrue(set.IsFrozen);
            Assert.IsFalse(set.IsBrittle, "Not while the ice holds.");
            set.Shatter();
            Assert.IsTrue(set.IsBrittle);
            Assert.AreEqual(1.2f, set.DamageTakenMultiplier(false), 0.001f, "The weapon cuts a fifth deeper.");
            Assert.AreEqual(1f, set.DamageTakenMultiplier(true), 0.001f, "Spells do not.");
            Assert.AreEqual(1.2f, set.StaggerTakenMultiplier, 0.001f);
            for (int i = 0; i < 52; i++) set.Tick(0.1f);
            Assert.IsFalse(set.IsBrittle, "It passes after five seconds.");
            // A chill that only fades, never having frozen, leaves nothing brittle.
            var mild = new AfflictionSet();
            mild.Apply(AfflictionKind.Chill, 2);
            for (int i = 0; i < 200; i++) mild.Tick(0.1f);
            Assert.IsFalse(mild.IsBrittle);
        }

        [Test]
        public void ShakenAndNotchedTogetherBuckle()
        {
            var both = new AfflictionSet();
            both.Apply(AfflictionKind.Tremor, 1);
            both.Apply(AfflictionKind.Sundered, 1);
            var one = new AfflictionSet();
            one.Apply(AfflictionKind.Tremor, 1);
            float plain = both.StaggerTakenMultiplier;
            float alone = one.StaggerTakenMultiplier;
            Afflictions.BuckledShare = 0.4f;
            Assert.AreEqual(plain * 1.4f, both.StaggerTakenMultiplier, 0.001f);
            Assert.AreEqual(alone, one.StaggerTakenMultiplier, 0.001f, "One of the two is not enough.");
        }

        [Test]
        public void ASoakedEnemyFreezesAtThreeAndTheWaterIsSpent()
        {
            var dry = new AfflictionSet();
            dry.Apply(AfflictionKind.Chill, 3);
            var wet = new AfflictionSet();
            wet.Apply(AfflictionKind.Soaked, 1);
            wet.Apply(AfflictionKind.Chill, 2);
            Afflictions.FlashFreezeAt = 3;
            dry.Apply(AfflictionKind.Chill, 1);
            Assert.IsFalse(dry.IsFrozen, "Without water it still takes five.");
            wet.Apply(AfflictionKind.Chill, 1);
            Assert.IsTrue(wet.IsFrozen);
            Assert.IsFalse(wet.Has(AfflictionKind.Soaked), "The freeze spends the Soaked.");
        }

        [Test]
        public void ChillSlowsUntilAllAtOnceTakesTheSlowAway()
        {
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Chill, 3);
            Assert.Less(set.SpeedMultiplier, 1f, "Chill slows, to begin with.");
            Afflictions.ChillSlowOff = true;
            Assert.AreEqual(1f, set.SpeedMultiplier, 0.0001f, "With the slow off, a chilled enemy moves freely.");
            set.Apply(AfflictionKind.Chill, 5);
            Assert.IsTrue(set.IsFrozen, "It still freezes at full depth.");
            Assert.AreEqual(0f, set.SpeedMultiplier, 0.0001f);
        }

        [Test]
        public void APoolWithNoManaAsksItsPriceElsewhereAndSetsItsGainsAside()
        {
            var pool = new ManaPool();
            pool.Gain(40f);
            Assert.IsTrue(pool.TrySpend(30f));
            Assert.AreEqual(10f, pool.Current, 0.001f);
            Assert.IsFalse(pool.TrySpend(30f), "A pool with mana refuses what it cannot cover.");

            float asked = 0f;
            bool pay = true;
            pool.Instead = cost => { asked += cost; return pay; };
            Assert.IsTrue(pool.Sealed);
            Assert.AreEqual(0f, pool.Fraction, 0.0001f, "The bar reads empty.");
            Assert.AreEqual(pool.Maximum, pool.Current, 0.0001f, "Every price may be asked.");
            Assert.IsTrue(pool.TrySpend(30f));
            Assert.AreEqual(30f, asked, 0.001f);
            pay = false;
            Assert.IsFalse(pool.TrySpend(30f), "When the other price cannot be paid, nothing is spent.");
            pool.Gain(30f);
            Assert.AreEqual(30f, pool.Diverted, 0.001f);
            Assert.AreEqual(30f, pool.TakeDiverted(), 0.001f);
            Assert.AreEqual(0f, pool.Diverted, 0.001f);

            pool.Instead = null;
            Assert.AreEqual(10f, pool.Current, 0.001f, "Its own mana is as it was left.");
        }

        private static bool Has(Upgrade mark, PlayerStat stat, float amount, StatWhen when = StatWhen.Always) =>
            mark.Stats != null && mark.Stats.Any(mod => mod.Stat == stat && mod.When == when && System.Math.Abs(mod.Amount - amount) < 0.0001f);

        [Test]
        public void TheKeystonesStandOnTheirRulesAndNotOnStandIns()
        {
            var furnace = Upgrade.ById(FireTree.OpenFurnace);
            Assert.IsTrue(Has(furnace, PlayerStat.GuardGrey, 0.71f), "Open Furnace lets more of a guarded blow through as grey.");
            Assert.IsTrue(furnace.Effects == null || furnace.Effects.Length == 0, "Its detonation is written by hand, not a burst round the player.");

            var stroke = Upgrade.ById(LightningTree.SingleStroke);
            Assert.IsTrue(Has(stroke, PlayerStat.AfflictionDepth, 5f), "Single Stroke lets Static lie ten deep.");

            var once = Upgrade.ById(IceTree.AllAtOnce);
            Assert.IsTrue(Has(once, PlayerStat.ShatterDamage, 2f), "All at Once triples the shatter.");

            Assert.IsTrue(Has(Upgrade.ById(IceTree.Unmoved), PlayerStat.DamageTaken, -0.5f, StatWhen.HeavySwing));
            Assert.IsTrue(Has(Upgrade.ById(EarthTree.HeavyHand), PlayerStat.DamageTaken, -0.35f, StatWhen.HeavySwing));

            var well = Upgrade.ById(WaterTree.DeepWell);
            Assert.IsNotNull(well.Tally, "Deep Well shows its well as a count.");
            Assert.AreEqual(140, well.Tally.Cap);
            Assert.IsTrue(Has(well, PlayerStat.FlaskCount, 1f));
            Assert.AreEqual(3, Upgrade.ById(WindTree.LeafOnTheWind).Tally.Cap, "Three breaths.");
            Assert.AreEqual(0, Upgrade.ById(WaterTree.SpringTide).Stats.Length, "Spring Tide's growth is worked at the cast, not a standing stat.");
            Assert.AreEqual(0, Upgrade.ById(MetalTree.MastersAnswer).Effects.Length, "Master's Answer changes the skill's price, and gives no mana.");

            var heart = Upgrade.ById(LifeTree.Heartwood);
            Assert.IsTrue(heart.Effects == null || heart.Effects.Length == 0, "Heartwood's mending comes from the mana it turns aside.");
        }

        [Test]
        public void ClosingCircleAndEyeOfTheStormHaveFormsOfTheirOwn()
        {
            Assert.IsInstanceOf<CircleForm>(SpellForms.Get(SpellForms.Circle));
            Assert.IsInstanceOf<EyeForm>(SpellForms.Get(SpellForms.Eye));
            Assert.AreEqual(SpellForms.Circle, Upgrade.ById(WindTree.ClosingCircle).Form);
            Assert.AreEqual(SpellForms.Eye, Upgrade.ById(TempestTree.EyeOfTheStorm).Form);
            Assert.AreEqual(5, Upgrade.ById(WindTree.ClosingCircle).Spell.Stacks);
        }
    }
}
