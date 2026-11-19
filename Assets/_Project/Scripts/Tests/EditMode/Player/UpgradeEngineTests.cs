using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The plain rules under the chart's newer words: numbers that hold only in a state, on a kind of blow or against
    /// an affliction; counts that are banked; an enemy held still; an affliction laid deeper; a flask filled by parts;
    /// health as a price.
    /// </summary>
    public class UpgradeEngineTests
    {
        [TearDown]
        public void PutTheDepthBack()
        {
            Afflictions.SetDeeper(AfflictionKind.None, 0);
            Afflictions.MoreBlows = 0;
        }

        [Test]
        public void AMarkOfLightCanCarryOneMoreCriticalBlow()
        {
            Afflictions.MoreBlows = 1;
            var set = new AfflictionSet();
            set.Apply(AfflictionKind.Marked, 1);
            Assert.AreEqual(4, set.BlowsLeft(AfflictionKind.Marked));
            set.Apply(AfflictionKind.Poison, 1);
            Assert.AreEqual(0, set.BlowsLeft(AfflictionKind.Poison), "what is not spent by blows stays so");
        }

        [Test]
        public void AStatThatWaitsOnAStateJoinsTheSumOnlyWhileTheStateHolds()
        {
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.DamageTaken, -0.06f));
            stats.AddLearned(new StatMod(PlayerStat.DamageTaken, -0.15f, StatWhen.LowHealth));
            stats.AddLearned(new StatMod(PlayerStat.MoveSpeed, 0.1f, StatWhen.Guarding));
            Assert.AreEqual(0.94f, stats.Multiplier(PlayerStat.DamageTaken), 0.0001f);
            int before = stats.Version;
            stats.SetWhen(StatWhen.LowHealth, true);
            Assert.Greater(stats.Version, before, "a reader is told the sum moved");
            Assert.AreEqual(0.79f, stats.Multiplier(PlayerStat.DamageTaken), 0.0001f);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.MoveSpeed), 0.0001f, "another state's number stays out");
            before = stats.Version;
            stats.SetWhen(StatWhen.LowHealth, true);
            Assert.AreEqual(before, stats.Version, "asked again with the same answer, nothing moves");
            stats.SetWhen(StatWhen.Guarding, true);
            Assert.AreEqual(1.1f, stats.Multiplier(PlayerStat.MoveSpeed), 0.0001f);
            stats.SetWhen(StatWhen.LowHealth, false);
            Assert.AreEqual(0.94f, stats.Multiplier(PlayerStat.DamageTaken), 0.0001f);
            Assert.IsTrue(stats.Holds(StatWhen.Guarding));
            // Learned while the state already holds, it counts at once; forgotten, it is gone.
            stats.AddLearned(new StatMod(PlayerStat.MoveSpeed, 0.05f, StatWhen.Guarding));
            Assert.AreEqual(1.15f, stats.Multiplier(PlayerStat.MoveSpeed), 0.0001f);
            stats.ClearLearned();
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.MoveSpeed), 0.0001f);
        }

        [Test]
        public void AStatForOneKindOfBlowIsAskedForByTheBlowAndNeverJoinsTheStandingSum()
        {
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.StaggerDealt, 0.15f, StatWhen.HeavyBlow));
            stats.AddLearned(new StatMod(PlayerStat.WeaponDamage, 0.1f, StatWhen.LightBlow));
            stats.AddLearned(new StatMod(PlayerStat.WeaponDamage, 0.2f, StatWhen.CriticalBlow));
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.StaggerDealt), 0.0001f);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f);
            Assert.AreEqual(0.15f, stats.Blow(PlayerStat.StaggerDealt, true, false), 0.0001f);
            Assert.AreEqual(0f, stats.Blow(PlayerStat.StaggerDealt, false, false), 0.0001f);
            Assert.AreEqual(0.1f, stats.Blow(PlayerStat.WeaponDamage, false, false), 0.0001f);
            Assert.AreEqual(0.3f, stats.Blow(PlayerStat.WeaponDamage, false, true), 0.0001f);
            Assert.AreEqual(0.2f, stats.Blow(PlayerStat.WeaponDamage, true, true), 0.0001f);
            stats.SetWhen(StatWhen.HeavyBlow, true);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.StaggerDealt), 0.0001f, "a kind of blow is not a state");
        }

        [Test]
        public void AStatAgainstAnAfflictionCountsOnlyOnATargetThatCarriesIt()
        {
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.SpellDamage, 0.2f, AfflictionKind.Burn));
            stats.AddLearned(new StatMod(PlayerStat.CritChance, 0.08f, AfflictionKind.Static));
            var clean = new AfflictionSet();
            var burning = new AfflictionSet();
            burning.Apply(AfflictionKind.Burn, 1);
            var charged = new AfflictionSet();
            charged.Apply(AfflictionKind.Static, 2);
            Assert.AreEqual(0f, stats.Against(PlayerStat.SpellDamage, clean));
            Assert.AreEqual(0.2f, stats.Against(PlayerStat.SpellDamage, burning), 0.0001f);
            Assert.AreEqual(0f, stats.Against(PlayerStat.SpellDamage, charged));
            Assert.AreEqual(0.08f, stats.Against(PlayerStat.CritChance, charged), 0.0001f);
            Assert.AreEqual(0f, stats.Against(PlayerStat.CritChance, null));
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.SpellDamage), 0.0001f, "it is no part of the standing sum");
            stats.ClearLearned();
            Assert.AreEqual(0f, stats.Against(PlayerStat.SpellDamage, burning));
        }

        [Test]
        public void TheSplitStatsAreSharesAndTheNewFlatOnesAreFlat()
        {
            foreach (var stat in new[] { PlayerStat.AttackStamina, PlayerStat.GuardStamina, PlayerStat.DodgeStamina, PlayerStat.BaseSpellCooldown,
                PlayerStat.BaseSpellReach, PlayerStat.BaseSpellDamage, PlayerStat.BurstReach, PlayerStat.BurstDamage, PlayerStat.FlaskEarn })
                Assert.IsFalse(PlayerStats.IsFlat(stat), stat.ToString());
            Assert.IsTrue(PlayerStats.IsFlat(PlayerStat.AfflictionDepth));
            Assert.IsTrue(PlayerStats.IsFlat(PlayerStat.Leech));
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.GuardStamina, -0.15f));
            Assert.AreEqual(0.85f, stats.Multiplier(PlayerStat.GuardStamina), 0.0001f);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.DodgeStamina), 0.0001f, "the guard's stamina is not the dodge's");
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.StaminaCost), 0.0001f);
        }

        [Test]
        public void WhatACountAddsIsAddedPointByPointAndClearedWithIt()
        {
            var stats = new PlayerStats();
            stats.AddTally(PlayerStat.WeaponDamage, 0.015f * 4);
            Assert.AreEqual(1.06f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f);
            stats.ClearTally();
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f);
        }

        [Test]
        public void AnAttackCostsItsStaminaTimesWhatTheStatsMakeOfIt()
        {
            var combo = new WeaponCombo { BufferTime = 0.3f, ChainGrace = 0.35f, ChainDelay = 0.02f };
            combo.Steps.Add(new AttackStep { Name = "cut", Windup = 0.1f, Active = 0.1f, Recovery = 0.2f, StaminaCost = 10f });
            combo.Reset();
            var stamina = new StaminaPool();
            stamina.Fill();
            float full = stamina.Current;
            combo.StaminaScale = 0.5f;
            combo.Press();
            combo.Tick(0.01f, stamina);
            Assert.IsTrue(combo.IsAttacking);
            Assert.AreEqual(full - 5f, stamina.Current, 0.2f);
        }

        [Test]
        public void HeldStillItCannotMoveOrTurnUntilTheTimeIsUpAndWhatResistsIceResistsTheHold()
        {
            var soldier = new AfflictionSet();
            Assert.IsTrue(soldier.Hold(2f));
            Assert.IsTrue(soldier.IsHeld);
            Assert.AreEqual(0f, soldier.SpeedMultiplier);
            Assert.AreEqual(0f, soldier.TurnMultiplier);
            Assert.IsFalse(soldier.IsFrozen, "held is not frozen: a heavy blow shatters nothing");
            soldier.Tick(1.5f);
            Assert.IsTrue(soldier.IsHeld);
            soldier.Hold(1f);
            Assert.AreEqual(1f, soldier.HeldLeft, 0.0001f, "a shorter hold does not cut a longer one short, a longer one extends it");
            soldier.Tick(1.1f);
            Assert.IsFalse(soldier.IsHeld);
            Assert.AreEqual(1f, soldier.SpeedMultiplier, 0.0001f);
            var god = new AfflictionSet { FreezeScale = 0.4f };
            Assert.IsTrue(god.Hold(2f));
            Assert.AreEqual(0.8f, god.HeldLeft, 0.0001f);
            var colossus = new AfflictionSet { CanFreeze = false };
            Assert.IsFalse(colossus.Hold(2f));
            Assert.IsFalse(colossus.IsHeld);
            soldier.Hold(3f);
            soldier.Clear();
            Assert.IsFalse(soldier.IsHeld);
        }

        [Test]
        public void TheHeldElementsAfflictionCanBeLaidDeeperAndFullMeansTheDeeperDepth()
        {
            var spec = Afflictions.Spec(AfflictionKind.Poison);
            Assert.AreEqual(spec.MaximumStacks, Afflictions.Depth(AfflictionKind.Poison));
            Afflictions.SetDeeper(AfflictionKind.Poison, 2);
            Assert.AreEqual(spec.MaximumStacks + 2, Afflictions.Depth(AfflictionKind.Poison));
            Assert.AreEqual(Afflictions.Spec(AfflictionKind.Chill).MaximumStacks, Afflictions.Depth(AfflictionKind.Chill), "only the one named");
            var set = new AfflictionSet();
            Assert.IsFalse(set.Apply(AfflictionKind.Poison, spec.MaximumStacks).ReachedFull);
            Assert.IsTrue(set.Apply(AfflictionKind.Poison, 5).ReachedFull);
            Assert.AreEqual(spec.MaximumStacks + 2, set.Stacks(AfflictionKind.Poison));
            Afflictions.SetDeeper(AfflictionKind.Poison, 0);
            Assert.AreEqual(spec.MaximumStacks, Afflictions.Depth(AfflictionKind.Poison));
        }

        [Test]
        public void TheDesignsDepthsAndNumbersAreTheOnesInForce()
        {
            Assert.AreEqual(5, Afflictions.Spec(AfflictionKind.Unsteady).MaximumStacks);
            Assert.AreEqual(8, Afflictions.Spec(AfflictionKind.Poison).MaximumStacks);
            Assert.AreEqual(2f, Afflictions.Spec(AfflictionKind.Poison).DamagePerTick);
            Assert.AreEqual(6f, Afflictions.Spec(AfflictionKind.Poison).Seconds);
            Assert.AreEqual(1, Afflictions.Spec(AfflictionKind.Cursed).MaximumStacks);
            Assert.AreEqual(1, Afflictions.Spec(AfflictionKind.Soaked).MaximumStacks);
            Assert.AreEqual(0.07f, Afflictions.Spec(AfflictionKind.Chill).SlowPerStack, 0.0001f);
            Assert.AreEqual(2.5f, Afflictions.Spec(AfflictionKind.Chill).FrozenSeconds, 0.0001f);
            Assert.AreEqual(0.08f, Afflictions.Spec(AfflictionKind.Sundered).WeaponDamageTakenPerStack, 0.0001f);
            Assert.AreEqual(6f, Afflictions.Spec(AfflictionKind.Static).Seconds);
            Assert.AreEqual(8f, Afflictions.Spec(AfflictionKind.Marked).Seconds);
        }

        [Test]
        public void AFlaskIsFilledByPartsAndAWholeOneJoinsTheBeltOnlyWhereThereIsRoom()
        {
            var belt = new FlaskBelt();
            belt.Refill();
            Assert.IsFalse(belt.Fill(0.5f), "a full belt takes nothing");
            Assert.IsTrue(belt.TryThrow());
            Assert.AreEqual(2, belt.Count);
            Assert.IsFalse(belt.Fill(0.5f));
            Assert.IsTrue(belt.Fill(0.5f), "two halves make one");
            Assert.AreEqual(3, belt.Count);
            // The rate at which damage earns one back.
            while (belt.Count > 0) belt.TryThrow();
            belt.EarnScale = 2f;
            Assert.IsFalse(belt.EarnFromDamage(belt.RechargeDamage * 0.4f));
            Assert.IsTrue(belt.EarnFromDamage(belt.RechargeDamage * 0.15f));
            // A belt with no flasks at all earns none.
            var none = new FlaskBelt();
            none.SetMaximum(0);
            Assert.AreEqual(0, none.Count);
            Assert.IsFalse(none.EarnFromDamage(1000f));
            Assert.IsFalse(none.Fill(1f));
        }

        [Test]
        public void HealthSpentAsAPriceIsLeftGreyAndNeverTakesTheLastPoint()
        {
            var health = new RegainHealth();
            health.Fill();
            health.Chip(30f);
            Assert.AreEqual(70f, health.Current, 0.0001f);
            Assert.AreEqual(30f, health.Grey, 0.0001f);
            Assert.AreEqual(20f, health.Regain(20f), 0.0001f, "blows win it back");
            health.Chip(500f);
            Assert.AreEqual(1f, health.Current, 0.0001f);
            Assert.IsFalse(health.IsDead);
            var spell = new SpellSpec { Cost = 0f, HealthCost = 25f };
            Assert.Greater(spell.Cost + spell.HealthCost, 0f);
        }

        [Test]
        public void ThrownIntoTheAirItHangsHelplessBlowsHoldItUpThreeTimesAndItComesDown()
        {
            var soldier = new AfflictionSet();
            Assert.IsFalse(soldier.Runs);
            Assert.IsTrue(soldier.Lift(1f));
            Assert.IsTrue(soldier.IsLifted && soldier.IsAirborne && soldier.Runs);
            Assert.AreEqual(0f, soldier.SpeedMultiplier);
            Assert.AreEqual(0f, soldier.TurnMultiplier);
            soldier.Tick(0.3f);
            Assert.AreEqual(AfflictionSet.AirRise, soldier.AirHeight, 0.0001f, "risen and hanging");
            for (int i = 0; i < 3; i++) Assert.IsTrue(soldier.AirBlow(0.45f, 3));
            Assert.IsFalse(soldier.AirBlow(0.45f, 3), "a fourth blow holds it up no longer");
            Assert.AreEqual(0.7f + 3 * 0.45f, soldier.AirLeft, 0.0001f);
            soldier.Tick(2f);
            Assert.IsFalse(soldier.LandedThisTick);
            soldier.Tick(0.1f);
            Assert.IsTrue(soldier.LandedThisTick, "told once, on the tick it lands");
            Assert.IsFalse(soldier.IsAirborne);
            Assert.AreEqual(0f, soldier.AirHeight);
            soldier.Tick(0.1f);
            Assert.IsFalse(soldier.LandedThisTick);
            // Each blow past the third adds less, for one who has learned to keep it up.
            soldier.Lift(1f);
            soldier.AirBlow(1f, 10, 0.5f);
            soldier.AirBlow(1f, 10, 0.5f);
            Assert.AreEqual(1f + 1f + 0.5f, soldier.AirLeft, 0.0001f);
            // Driven into the ground at once.
            soldier.Drop();
            soldier.Tick(0.01f);
            Assert.IsTrue(soldier.LandedThisTick);
        }

        [Test]
        public void AGodHangsABriefWhileAndTheColossusOnlyReelsWhichStillCountsAsInTheAir()
        {
            var god = new AfflictionSet { FreezeScale = 0.4f };
            Assert.IsTrue(god.Lift(1f));
            Assert.AreEqual(AfflictionSet.SturdyAirShare, god.AirLeft, 0.0001f);
            var colossus = new AfflictionSet { CanFreeze = false };
            Assert.IsFalse(colossus.Lift(1f), "it cannot leave the ground");
            Assert.IsFalse(colossus.IsLifted);
            Assert.IsTrue(colossus.IsAirborne, "but reeling, every bonus against the airborne counts");
            Assert.AreEqual(AfflictionSet.ReelSeconds, colossus.AirLeft, 0.0001f);
            Assert.AreEqual(0f, colossus.AirHeight);
            Assert.Greater(colossus.SpeedMultiplier, 0f, "and it is not held");
            Assert.IsFalse(colossus.AirBlow(0.45f, 3));
            colossus.Tick(1.1f);
            Assert.IsFalse(colossus.LandedThisTick, "what never rose does not land");
            Assert.IsFalse(colossus.IsAirborne);
        }

        [Test]
        public void NumbersForABlowInTheAirAndForTheBlowThatEndsAChainAreAskedForByThatBlow()
        {
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.WeaponDamage, 0.2f, StatWhen.AirborneBlow));
            stats.AddLearned(new StatMod(PlayerStat.ManaFromBlows, 1f, StatWhen.AirborneBlow));
            stats.AddLearned(new StatMod(PlayerStat.WeaponDamage, 0.12f, StatWhen.ChainEndBlow));
            Assert.AreEqual(0.2f, stats.InAir(PlayerStat.WeaponDamage), 0.0001f);
            Assert.AreEqual(1f, stats.InAir(PlayerStat.ManaFromBlows), 0.0001f);
            Assert.AreEqual(0f, stats.Blow(PlayerStat.WeaponDamage, false, false, false), 0.0001f);
            Assert.AreEqual(0.12f, stats.Blow(PlayerStat.WeaponDamage, false, false, true), 0.0001f);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.WeaponDamage), 0.0001f);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.ManaFromBlows), 0.0001f);
        }

        [Test]
        public void HeatRisesOnFewerBlowsCoolsMoreSlowlyAndClimbsHigherWhenItsNumbersAreChanged()
        {
            var heat = new Heat();
            heat.Reset();
            int usual = heat.BlowsPerLevel;
            heat.BlowsPerLevel = usual - 1;
            for (int i = 0; i < usual - 1; i++) heat.Landed();
            Assert.AreEqual(1, heat.Level);
            heat.MaximumLevel = 8;
            heat.Fill();
            Assert.AreEqual(8, heat.Level);
            Assert.AreEqual(1f + 8 * heat.DamagePerLevel, heat.DamageMultiplier, 0.0001f);
            heat.CoolAfter = 9f;
            heat.Tick(8.9f);
            Assert.AreEqual(8, heat.Level);
            heat.Tick(0.2f);
            Assert.AreEqual(7, heat.Level);
        }
    }
}
