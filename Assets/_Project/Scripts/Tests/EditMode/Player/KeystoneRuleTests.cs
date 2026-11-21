using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Trees;
using NUnit.Framework;
using UnityEngine;

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
            Afflictions.BurnBiteOff = false;
            Afflictions.Rules.Clear();
        }

        // ---- The six keystones of 2026-11-20 (keystones_fire_water_life.md) and the pieces they stand on.

        [Test]
        public void TheSixNewKeystonesStandInTheEmptyPlacesOfTheirBoughs()
        {
            var places = new (string id, string tree, string place, string from)[]
            {
                (FireTree.IntoTheFire, Elements.Fire, BranchPattern.LeftCrown, Upgrade.FlameTrail),
                (FireTree.BladeDrinksFire, Elements.Fire, BranchPattern.RightCrown, FireTree.CatchingFire),
                (WaterTree.HighWaterLowWater, Elements.Water, BranchPattern.TrunkLeft, WaterTree.SpreadingWater),
                (WaterTree.DrawnBackToBreak, Elements.Water, BranchPattern.LeftCrown, WaterTree.HeavyWater),
                (LifeTree.SharedRoots, Elements.Life, BranchPattern.RightCrown, LifeTree.BriarCoat),
                (LifeTree.InItsSeason, Elements.Life, BranchPattern.TrunkLeft, LifeTree.RootAndBranch),
            };
            foreach (var (id, tree, place, from) in places)
            {
                var mark = Upgrade.ById(id);
                Assert.IsNotNull(mark, id);
                Assert.AreEqual(UpgradeKind.Keystone, mark.Kind, id);
                Assert.AreEqual(tree, mark.Element, id);
                Assert.AreEqual(6, mark.Cost, id);
                CollectionAssert.AreEqual(new[] { from }, mark.Requires, id);
                var at = BranchPattern.Of(place);
                Assert.AreEqual(at.X, mark.X, 0.001f, id + " stands at " + place);
                Assert.AreEqual(at.Y, mark.Y, 0.001f, id + " stands at " + place);
                Assert.IsFalse(RetiredMarks.IsRetired(id), id + " is a new id, never a retired one");
                Assert.IsNotEmpty(mark.Description, id);
            }
            // Each of the three trees now has three keystones beside its Devotion.
            foreach (string tree in new[] { Elements.Fire, Elements.Water, Elements.Life })
                Assert.AreEqual(3, Upgrade.All.Count(mark => mark.Element == tree && !mark.IsMix && mark.Kind == UpgradeKind.Keystone), tree);
            Assert.IsTrue(RetiredMarks.IsRetired("life.roots"), "the old Shared Roots stays retired and refunded");
        }

        [Test]
        public void ADodgeIsTowardAwayOrAcrossTheEnemyFaced()
        {
            var from = Vector3.zero;
            var enemy = new Vector3(0f, 0f, 5f);
            Assert.AreEqual(DodgeWay.Toward, DodgeRules.WayOf(Vector3.forward, from, enemy, true));
            Assert.AreEqual(DodgeWay.Toward, DodgeRules.WayOf(new Vector3(0.6f, 0f, 1f), from, enemy, true), "within 60 degrees is toward");
            Assert.AreEqual(DodgeWay.Away, DodgeRules.WayOf(Vector3.back, from, enemy, true));
            Assert.AreEqual(DodgeWay.Any, DodgeRules.WayOf(Vector3.right, from, enemy, true), "across its line is neither");
            Assert.AreEqual(DodgeWay.Any, DodgeRules.WayOf(Vector3.forward, from, enemy, false), "with no enemy, neither");
        }

        [Test]
        public void TheEnemyFirstDodgedFromStillCountsInACrowd()
        {
            // The owner's answer of 2026-11-20, as the Phase 4b probe found it: the host stands north at z 6, the player at
            // z 3.5 facing it, and other enemies stand south at z -5. A roll south is away from the host and turns the
            // player to face south, where the crowd is now the enemy ahead.
            var hold = new DodgeHold<string>();
            Vector3 host = new Vector3(0f, 0f, 6f), crowd = new Vector3(0f, 0f, -5f);
            Vector3 from = new Vector3(0f, 0f, 3.5f);
            string against = hold.Against(from, "host", false, Vector3.zero, DodgeRules.HoldRange);
            Assert.AreEqual("host", against, "the first dodge takes the enemy faced");
            Assert.AreEqual(DodgeWay.Away, DodgeRules.WayOf(Vector3.back, from, host, true));
            hold.Took(against);
            Assert.AreEqual("host", hold.Held);

            // The second roll south: measured against the crowd ahead it would be toward; held to the host it is away again.
            from = new Vector3(0f, 0f, -1f);
            Assert.AreEqual(DodgeWay.Toward, DodgeRules.WayOf(Vector3.back, from, crowd, true), "the crowd ahead would make it toward");
            against = hold.Against(from, "crowd", true, host, DodgeRules.HoldRange);
            Assert.AreEqual("host", against, "the host is held, not the crowd ahead");
            Assert.AreEqual(DodgeWay.Away, DodgeRules.WayOf(Vector3.back, from, host, true), "so the second roll south is away too");
            hold.Took("crowd");
            Assert.AreEqual("host", hold.Held, "a later dodge never takes the hold from the first one's enemy");

            // A third roll, still near the host: still the host.
            from = new Vector3(0f, 0f, -5.5f);
            Assert.AreEqual("host", hold.Against(from, "crowd", true, host, DodgeRules.HoldRange));
        }

        [Test]
        public void TheHeldEnemyIsLetGoWhenItDiesOrThePlayerGetsFarFromIt()
        {
            Assert.AreEqual(16f, DodgeRules.HoldRange, "the fight's 12 m and most of a 4.5 m roll more");
            var hold = new DodgeHold<string>();
            hold.Took("host");
            Vector3 host = new Vector3(0f, 0f, 6f);

            // Dead: the next dodge takes the enemy faced then, and holds it.
            Assert.AreEqual("crowd", hold.Against(Vector3.zero, "crowd", false, host, DodgeRules.HoldRange));
            Assert.IsNull(hold.Held);
            hold.Took("crowd");
            Assert.AreEqual("crowd", hold.Held);

            // Far: 16 m on the ground still holds, a step more lets go. Height does not count.
            hold.Clear();
            hold.Took("host");
            Assert.IsTrue(hold.Keeps(new Vector3(0f, 3f, 6f - 16f), true, host, DodgeRules.HoldRange), "16 m off, on a ledge below, still held");
            Assert.AreEqual("other", hold.Against(new Vector3(0f, 0f, 6f - 16.5f), "other", true, host, DodgeRules.HoldRange), "16.5 m off, let go");
            Assert.IsNull(hold.Held);

            // With nothing held and no enemy faced, the dodge holds nothing.
            Assert.IsNull(hold.Against(Vector3.zero, null, false, Vector3.zero, DodgeRules.HoldRange));
            hold.Took(null);
            Assert.IsNull(hold.Held);
        }

        [Test]
        public void IntoTheFireMakesADodgeSafeWholeAndThroughBodiesOrDearAndNeverPerfect()
        {
            var stamina = new StaminaPool();
            stamina.Fill();
            var roll = new DodgeRoll();
            Assert.IsTrue(roll.TryStart(Vector3.forward, stamina, new DodgeTerms { CostScale = 1f, WholeSafe = true, PassesBodies = true }));
            Assert.AreEqual(stamina.Maximum - roll.StaminaCost, stamina.Current, 0.01f);
            Assert.IsTrue(roll.PassesBodies);
            for (int i = 0; i < 3; i++) roll.Tick(0.1f);
            Assert.Greater(roll.Elapsed, roll.InvulnerableUntil, "past the usual window");
            Assert.IsTrue(roll.IsInvulnerable, "toward the enemy: safe for the whole roll");
            while (roll.IsRolling) roll.Tick(0.1f);
            for (int i = 0; i < 5; i++) roll.Tick(0.1f);
            stamina.Fill();
            Assert.IsTrue(roll.TryStart(Vector3.back, stamina, new DodgeTerms { CostScale = 2f, NoPerfect = true }));
            Assert.AreEqual(stamina.Maximum - roll.StaminaCost * 2f, stamina.Current, 0.01f, "away: twice the stamina");
            Assert.IsTrue(roll.NoPerfect);
            Assert.IsFalse(roll.PassesBodies, "the terms are this roll's alone");
            for (int i = 0; i < 3; i++) roll.Tick(0.1f);
            Assert.IsFalse(roll.IsInvulnerable, "away: only the usual window");
            // The plain roll as written.
            while (roll.IsRolling) roll.Tick(0.1f);
            for (int i = 0; i < 5; i++) roll.Tick(0.1f);
            stamina.Fill();
            Assert.IsTrue(roll.TryStart(Vector3.right, stamina));
            Assert.IsFalse(roll.NoPerfect || roll.WholeSafe || roll.PassesBodies);
            // A perfect dodge toward the enemy raises Heat.
            var into = Upgrade.ById(FireTree.IntoTheFire);
            Assert.IsTrue(into.Effects.Any(effect => effect.On == UpgradeTrigger.PerfectDodge && effect.Way == DodgeWay.Toward && effect.Do == UpgradeAction.RaiseHeat));
        }

        [Test]
        public void TheBladeDrinksFireStopsBurnsBitingButLetsThemRunDown()
        {
            var burning = new AfflictionSet();
            burning.Apply(AfflictionKind.Burn, 3);
            float bitten = 0f;
            for (int i = 0; i < 10; i++) bitten += burning.Tick(0.1f);
            Assert.Greater(bitten, 0f, "a burn bites as written");
            Afflictions.BurnBiteOff = true;
            var drunk = new AfflictionSet();
            drunk.Apply(AfflictionKind.Burn, 3);
            float none = 0f;
            for (int i = 0; i < 10; i++) none += drunk.Tick(0.1f);
            Assert.AreEqual(0f, none, 0.0001f, "burns no longer bite by themselves");
            Assert.IsTrue(drunk.Burn.IsBurning, "the fuel is still there for the blade");
            Assert.AreEqual(2, drunk.Spend(AfflictionKind.Burn, 2), "a blow drinks what it takes");
            Assert.AreEqual(1, drunk.Stacks(AfflictionKind.Burn));
            Assert.IsEmpty(Upgrade.ById(FireTree.BladeDrinksFire).Effects, "built by hand, with no stand-in effects");
        }

        [Test]
        public void HighWaterLowWaterSwapsSpellAndWeaponByTheTide()
        {
            var stats = new PlayerStats();
            foreach (var mod in Upgrade.ById(WaterTree.HighWaterLowWater).Stats) stats.AddLearned(mod);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.SpellDamage), 0.001f, "neither half holds before the clock runs");
            stats.SetWhen(StatWhen.HighWater, true);
            Assert.AreEqual(1.33f, stats.Multiplier(PlayerStat.SpellDamage), 0.001f, "high water: spells a third harder");
            Assert.AreEqual(0.8f, stats.Multiplier(PlayerStat.WeaponDamage), 0.001f, "and the weapon a fifth softer");
            stats.SetWhen(StatWhen.HighWater, false);
            stats.SetWhen(StatWhen.LowWater, true);
            Assert.AreEqual(1.33f, stats.Multiplier(PlayerStat.WeaponDamage), 0.001f, "low water: the other way round");
            Assert.AreEqual(0.8f, stats.Multiplier(PlayerStat.SpellDamage), 0.001f);
            var effects = Upgrade.ById(WaterTree.HighWaterLowWater).Effects;
            Assert.IsTrue(effects.Any(effect => effect.On == UpgradeTrigger.PerfectGuard && effect.Do == UpgradeAction.TurnTide));
            Assert.IsTrue(effects.Any(effect => effect.On == UpgradeTrigger.PerfectDodge && effect.Do == UpgradeAction.TurnTide));
        }

        [Test]
        public void DrawnBackToBreakBanksAWaveForEachDodgeAwayAndBreaksThemAllOnTheNextBlow()
        {
            var mark = Upgrade.ById(WaterTree.DrawnBackToBreak);
            Assert.AreEqual(3, mark.Tally.Cap);
            Assert.AreEqual(PlayerStat.WeaponDamage, mark.Tally.EmptyStat);
            Assert.AreEqual(-0.2f, mark.Tally.EmptyAmount, 0.001f, "with no wave drawn, blows a fifth softer");
            var bank = mark.Effects.Single(effect => effect.Do == UpgradeAction.Bank);
            Assert.AreEqual(UpgradeTrigger.DodgeEnd, bank.On);
            Assert.AreEqual(DodgeWay.Away, bank.Way, "only a dodge away from the enemy faced");
            var breaker = mark.Effects.Single(effect => effect.Do == UpgradeAction.Cone);
            Assert.AreEqual(UpgradeTrigger.AnyHit, breaker.On);
            Assert.IsTrue(breaker.SpendsAll && breaker.PerStack, "all the waves break at once, each counted");
            Assert.AreEqual(4f, breaker.Reach);
            Assert.AreEqual(12f, breaker.Damage);
            Assert.AreEqual(20f, breaker.Stagger);
            Assert.AreEqual(AfflictionKind.Soaked, breaker.Affliction);
            Assert.AreEqual(2f, breaker.Amount, "thrown 2 m away");
        }

        [Test]
        public void ASeedLeftAloneRipensStopsFadingAndBurstsThreeTimesAsHard()
        {
            var book = new Clover.Player.Growth.RuleBook();
            Assert.IsTrue(book.Apply(Upgrade.ById(LifeTree.InItsSeason)));
            book.Publish();
            Assert.AreEqual(LifeTree.RipensAfter, Afflictions.Rules.RipensAfter(AfflictionKind.Seeded), 0.001f);
            var seed = new AfflictionSet();
            seed.Apply(AfflictionKind.Seeded, 2);
            Assert.AreEqual(LifeTree.UnripeShare, seed.RipeShare(AfflictionKind.Seeded), 0.001f, "unripe: half as strong");
            float unripeBurst = seed.DeathBurst(out _);
            for (int i = 0; i < 61; i++) seed.Tick(0.1f);
            Assert.IsTrue(seed.IsRipe(AfflictionKind.Seeded), "six seconds alone");
            Assert.AreEqual(LifeTree.RipeShare, seed.RipeShare(AfflictionKind.Seeded), 0.001f);
            Assert.AreEqual(unripeBurst * 6f, seed.DeathBurst(out _), 0.01f, "the ripe death burst is six times the unripe one (three against a half)");
            for (int i = 0; i < 60; i++) seed.Tick(0.1f);
            Assert.AreEqual(2, seed.Stacks(AfflictionKind.Seeded), "a ripe seed stops fading (Seeded lasts 8 s)");
            seed.Apply(AfflictionKind.Seeded, 1);
            Assert.IsFalse(seed.IsRipe(AfflictionKind.Seeded), "a fresh seed starts the ripening over");
            for (int i = 0; i < 61; i++) seed.Tick(0.1f);
            Assert.IsTrue(seed.IsRipe(AfflictionKind.Seeded), "ripe again after six seconds more");
            // Without the rule a seed fades and bursts as written; one ripe when the mark leaves force fades again.
            Afflictions.Rules.Clear();
            for (int i = 0; i < 90; i++) seed.Tick(0.1f);
            Assert.IsFalse(seed.Has(AfflictionKind.Seeded), "a ripe seed fades once the mark is gone");
            var plain = new AfflictionSet();
            plain.Apply(AfflictionKind.Seeded, 2);
            Assert.AreEqual(1f, plain.RipeShare(AfflictionKind.Seeded), 0.001f);
            for (int i = 0; i < 90; i++) plain.Tick(0.1f);
            Assert.IsFalse(plain.Has(AfflictionKind.Seeded));
            // A ripe burst grows a thornbush.
            Assert.IsTrue(Upgrade.ById(LifeTree.InItsSeason).Effects.Any(effect => effect.On == UpgradeTrigger.RipeBurst && effect.Do == UpgradeAction.Hazard && effect.Grown));
        }

        [Test]
        public void SharedRootsIsBuiltByHandWithNoStandIn()
        {
            var mark = Upgrade.ById(LifeTree.SharedRoots);
            Assert.IsEmpty(mark.Effects, "the wound is passed by PlayerDefence and PlayerUpgrades, not by an arc stand-in");
            StringAssert.Contains("third", mark.Description);
            StringAssert.Contains("no longer heal", mark.Description);
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
            // Open Furnace was not carried over into the reshaped Fire tree (Phase 4).
            Assert.IsNull(Upgrade.ById(FireTree.OpenFurnace));

            var stroke = Upgrade.ById(LightningTree.SingleStroke);
            Assert.IsTrue(Has(stroke, PlayerStat.AfflictionDepth, 5f), "Single Stroke lets Static lie ten deep.");

            var once = Upgrade.ById(IceTree.AllAtOnce);
            Assert.IsTrue(Has(once, PlayerStat.ShatterDamage, 2f), "All at Once triples the shatter.");

            // Unmoved, which was Earth's Heavy Hand word for word, is replaced by Sent Sliding in the reshaped Ice tree (Phase 4).
            Assert.IsNull(Upgrade.ById(IceTree.Unmoved));
            Assert.IsTrue(RetiredMarks.IsRetired(IceTree.Unmoved));
            Assert.AreEqual(UpgradeKind.Keystone, Upgrade.ById(IceTree.SentSliding).Kind);
            Assert.IsTrue(Has(Upgrade.ById(EarthTree.HeavyHand), PlayerStat.DamageTaken, -0.35f, StatWhen.HeavySwing));

            var well = Upgrade.ById(WaterTree.DeepWell);
            Assert.IsNotNull(well.Tally, "Deep Well shows its well as a count.");
            Assert.AreEqual(140, well.Tally.Cap);
            Assert.IsTrue(Has(well, PlayerStat.FlaskCount, 1f));
            Assert.AreEqual(3, Upgrade.ById(WindTree.LeafOnTheWind).Tally.Cap, "Three breaths.");
            // Spring Tide was not carried over into the reshaped Water tree (Phase 4): every spell spending all the mana is broken by cooldowns.
            Assert.IsNull(Upgrade.ById(WaterTree.SpringTide));
            Assert.IsTrue(RetiredMarks.IsRetired(WaterTree.SpringTide));
            Assert.AreEqual(0, Upgrade.ById(MetalTree.MastersAnswer).Effects.Length, "Master's Answer changes the skill's price, and gives no mana.");

            // Heartwood was cut with Life's reshaping (Phase 4): with spells on cooldowns it has nothing to pay for, and Drawn
            // From the Wood's Feed the Ground carries health as the price now.
            Assert.IsNull(Upgrade.ById(LifeTree.Heartwood));
            Assert.IsTrue(RetiredMarks.IsRetired(LifeTree.Heartwood));
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
