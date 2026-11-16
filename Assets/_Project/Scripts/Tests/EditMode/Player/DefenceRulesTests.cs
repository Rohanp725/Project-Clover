using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The rules behind being hit: grey health and regain, the guard and its perfect window, and the flasks.</summary>
    public class DefenceRulesTests
    {
        private static StaminaPool Stamina(float amount = 100f)
        {
            var pool = new StaminaPool { Maximum = amount, RegenerationPerSecond = 0f };
            pool.Fill();
            return pool;
        }

        private static RegainHealth Health()
        {
            var health = new RegainHealth { Maximum = 100f, GreyDelay = 2f, GreyFadePerSecond = 10f };
            health.Fill();
            return health;
        }

        [Test]
        public void AGuardedBlowLeavesGreyHealthThatHitsWinBack()
        {
            var health = Health();
            health.Chip(20f);
            Assert.AreEqual(80f, health.Current, 0.001f);
            Assert.AreEqual(20f, health.Grey, 0.001f);
            Assert.AreEqual(8f, health.Regain(8f), 0.001f);
            Assert.AreEqual(88f, health.Current, 0.001f);
            Assert.AreEqual(12f, health.Regain(50f), 0.001f, "no more than was grey");
            Assert.AreEqual(100f, health.Current, 0.001f);
        }

        [Test]
        public void GreyHealthFadesAfterBeingLeftAndAnUnguardedBlowTakesItAll()
        {
            var health = Health();
            health.Chip(20f);
            health.Tick(1.9f);
            Assert.AreEqual(20f, health.Grey, 0.001f);
            health.Tick(0.1f);
            health.Tick(1f);
            Assert.AreEqual(10f, health.Grey, 1.1f);

            health.Damage(30f);
            Assert.AreEqual(0f, health.Grey, 0.001f);
            Assert.AreEqual(50f, health.Current, 0.001f);
        }

        [Test]
        public void GuardingAloneNeverKills()
        {
            var health = Health();
            health.Damage(95f);
            health.Chip(50f);
            Assert.IsFalse(health.IsDead);
            Assert.AreEqual(1f, health.Current, 0.001f);
            health.Damage(1f);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void HealingDoesNotLeaveGreyAboveTheMaximum()
        {
            var health = Health();
            health.Chip(30f);
            health.Heal(20f);
            Assert.AreEqual(90f, health.Current, 0.001f);
            Assert.AreEqual(10f, health.Grey, 0.001f);
        }

        [Test]
        public void AHeldGuardBlocksForStaminaAndLetsALittleThrough()
        {
            var guard = new Guard { PerfectWindow = 0.2f, StaminaPerDamage = 1f, ChipFraction = 0.25f };
            guard.Reset();
            var stamina = Stamina();
            Assert.AreEqual(GuardResult.NotGuarded, guard.Resolve(20f, false, stamina, out _));

            guard.Press();
            guard.SetRaised(true);
            guard.Tick(0.5f);
            Assert.AreEqual(GuardResult.Blocked, guard.Resolve(20f, false, stamina, out float chip));
            Assert.AreEqual(5f, chip, 0.001f);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void AGuardPressedJustBeforeTheBlowIsPerfectAndFree()
        {
            var guard = new Guard { PerfectWindow = 0.2f };
            guard.Reset();
            var stamina = Stamina();
            guard.Press();
            guard.SetRaised(true);
            guard.Tick(0.1f);
            Assert.AreEqual(GuardResult.Perfect, guard.Resolve(20f, false, stamina, out float chip));
            Assert.AreEqual(0f, chip, 0.001f);
            Assert.AreEqual(100f, stamina.Current, 0.001f);
        }

        [Test]
        public void ARedBlowGoesThroughAHeldGuardButNotAPerfectOne()
        {
            var guard = new Guard { PerfectWindow = 0.2f };
            guard.Reset();
            var stamina = Stamina();
            guard.Press();
            guard.SetRaised(true);
            guard.Tick(0.1f);
            Assert.AreEqual(GuardResult.Perfect, guard.Resolve(20f, true, stamina, out _));
            guard.Tick(0.5f);
            Assert.AreEqual(GuardResult.NotGuarded, guard.Resolve(20f, true, stamina, out _));
            Assert.AreEqual(100f, stamina.Current, 0.001f, "a blow that cannot be blocked costs no stamina either");
        }

        [Test]
        public void HammeringTheGuardGivesNoPerfectWindow()
        {
            var guard = new Guard { PerfectWindow = 0.2f, SpamLockout = 0.4f };
            guard.Reset();
            var stamina = Stamina();
            guard.Press();
            guard.SetRaised(true);
            guard.Tick(0.25f);
            guard.Press();
            guard.Tick(0.05f);
            Assert.AreEqual(GuardResult.Blocked, guard.Resolve(10f, false, stamina, out _));
            guard.Tick(0.5f);
            guard.Press();
            guard.Tick(0.05f);
            Assert.AreEqual(GuardResult.Perfect, guard.Resolve(10f, false, stamina, out _), "a press after a pause is perfect again");
        }

        [Test]
        public void APressMadeWhileUnableToGuardEarnsNoPerfectWindow()
        {
            var guard = new Guard { PerfectWindow = 0.2f };
            guard.Reset();
            var stamina = Stamina();
            guard.Press(false);
            guard.Tick(0.1f);
            guard.SetRaised(true);
            Assert.AreEqual(GuardResult.Blocked, guard.Resolve(10f, false, stamina, out _));
        }

        [Test]
        public void TheGuardCoversTheFrontOnly()
        {
            var guard = new Guard { ArcDegrees = 200f };
            Assert.IsTrue(guard.Covers(0f));
            Assert.IsTrue(guard.Covers(95f));
            Assert.IsFalse(guard.Covers(120f));
            Assert.IsFalse(guard.Covers(180f), "a blow from behind is not guarded");
        }

        [Test]
        public void ABlowTheStaminaCannotPayForBreaksTheGuard()
        {
            var guard = new Guard { StaminaPerDamage = 1f };
            guard.Reset();
            var stamina = Stamina(15f);
            guard.Press();
            guard.SetRaised(true);
            guard.Tick(1f);
            Assert.AreEqual(GuardResult.Broken, guard.Resolve(20f, false, stamina, out _));
            Assert.AreEqual(0f, stamina.Current, 0.001f);
        }

        [Test]
        public void AFlaskHealsPartWayThroughTheDrinkAndIsOnlyUsedThen()
        {
            var flasks = new FlaskBelt { Maximum = 3, DrinkSeconds = 0.8f, HealAt = 0.4f };
            flasks.Refill();
            Assert.IsTrue(flasks.TryStartDrink());
            Assert.IsFalse(flasks.TryStartDrink(), "one drink at a time");
            Assert.IsFalse(flasks.Tick(0.3f));
            Assert.AreEqual(3, flasks.Count);
            Assert.IsTrue(flasks.Tick(0.2f), "this is the moment to heal");
            Assert.AreEqual(2, flasks.Count);
            Assert.IsFalse(flasks.Tick(0.2f));
            Assert.IsTrue(flasks.IsDrinking);
            flasks.Tick(0.2f);
            Assert.IsFalse(flasks.IsDrinking);
        }

        [Test]
        public void AnInterruptedDrinkWastesNoFlask()
        {
            var flasks = new FlaskBelt { Maximum = 3, DrinkSeconds = 0.8f, HealAt = 0.4f };
            flasks.Refill();
            flasks.TryStartDrink();
            flasks.Tick(0.3f);
            flasks.Interrupt();
            Assert.IsFalse(flasks.Tick(0.5f));
            Assert.AreEqual(3, flasks.Count);
        }

        [Test]
        public void WithNoFlasksLeftDamageDealtEarnsOneBack()
        {
            var flasks = new FlaskBelt { Maximum = 1, DrinkSeconds = 0.2f, HealAt = 0.1f, RechargeDamage = 100f };
            flasks.Refill();
            Assert.IsFalse(flasks.EarnFromDamage(500f), "nothing is earned while a flask is left");
            flasks.TryStartDrink();
            flasks.Tick(0.3f);
            Assert.AreEqual(0, flasks.Count);
            Assert.IsFalse(flasks.TryStartDrink());
            Assert.IsFalse(flasks.EarnFromDamage(60f));
            Assert.AreEqual(0.6f, flasks.RechargeFraction, 0.001f);
            Assert.IsTrue(flasks.EarnFromDamage(60f));
            Assert.AreEqual(1, flasks.Count);
        }
    }
}
