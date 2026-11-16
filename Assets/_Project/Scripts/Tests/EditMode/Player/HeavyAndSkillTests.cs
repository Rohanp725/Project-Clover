using System.Collections.Generic;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The heavy button: a tap, a hold, and a heavy placed at the end of a light chain.</summary>
    public class HeavyAndSkillTests
    {
        private const float Frame = 1f / 60f;

        private static StaminaPool Stamina(float amount = 100f)
        {
            var pool = new StaminaPool { Maximum = amount, RegenerationPerSecond = 0f };
            pool.Fill();
            return pool;
        }

        private static ManaPool Mana(float amount)
        {
            var pool = new ManaPool { Maximum = 100f };
            pool.Empty();
            pool.Gain(amount);
            return pool;
        }

        private static WeaponCombo Weapon()
        {
            var combo = new WeaponCombo { BufferTime = 0.3f, ChainGrace = 0.35f, ChainDelay = 0.02f, ChargeTime = 0.4f, SkillManaCost = 30f };
            for (int i = 0; i < 3; i++)
                combo.Steps.Add(new AttackStep { Name = "light" + i, Windup = 0.1f, Active = 0.1f, Recovery = 0.2f, StaminaCost = 10f });
            combo.Heavy = new AttackStep { Name = "heavy", Windup = 0.3f, Active = 0.1f, Recovery = 0.4f, StaminaCost = 20f, Heavy = true };
            combo.ChainHeavy = new AttackStep { Name = "chain heavy", Windup = 0.2f, Active = 0.1f, Recovery = 0.4f, StaminaCost = 20f, Heavy = true };
            combo.Finisher = new AttackStep { Name = "finisher", Windup = 0.3f, Active = 0.1f, Recovery = 0.5f, StaminaCost = 0f, Heavy = true };
            combo.Skill = new AttackStep { Name = "skill", Windup = 0.1f, Active = 0.1f, Recovery = 0.5f, StaminaCost = 15f, Heavy = true };
            combo.Reset();
            return combo;
        }

        // Runs the weapon for a time and records the name of every attack that started.
        private static ComboEvents Run(WeaponCombo combo, StaminaPool stamina, ManaPool mana, float seconds, List<string> started)
        {
            var all = ComboEvents.None;
            for (float t = 0f; t < seconds; t += Frame)
            {
                var events = combo.Tick(Frame, stamina, mana);
                all |= events;
                if ((events & ComboEvents.Started) != 0) started?.Add(combo.Current.Name);
            }
            return all;
        }

        [Test]
        public void ATapGivesTheHeavyOnRelease()
        {
            var combo = Weapon(); var stamina = Stamina(); var mana = Mana(100f);
            var started = new List<string>();
            combo.PressHeavy();
            Run(combo, stamina, mana, 0.1f, started);
            Assert.AreEqual(AttackPhase.Charge, combo.Phase, "held, the player stands ready");
            Assert.IsEmpty(started);
            combo.ReleaseHeavy();
            Run(combo, stamina, mana, 0.05f, started);
            Assert.AreEqual(new[] { "heavy" }, started.ToArray());
            Assert.AreEqual(100f, mana.Current, 0.001f, "a tap costs no mana");
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void AFullHoldGivesTheSkillOnReleaseAndSpendsTheMana()
        {
            var combo = Weapon(); var stamina = Stamina(); var mana = Mana(50f);
            var started = new List<string>();
            combo.PressHeavy();
            var events = Run(combo, stamina, mana, 0.5f, started);
            Assert.IsTrue((events & ComboEvents.Charged) != 0, "the charge reports that it is ready");
            Assert.IsTrue(combo.IsCharged);
            Assert.IsEmpty(started, "it waits for the release");
            combo.ReleaseHeavy();
            Run(combo, stamina, mana, 0.05f, started);
            Assert.AreEqual(new[] { "skill" }, started.ToArray());
            Assert.AreEqual(20f, mana.Current, 0.001f);
        }

        [Test]
        public void WithoutTheManaAHoldBecomesTheHeavyByItself()
        {
            var combo = Weapon(); var stamina = Stamina(); var mana = Mana(10f);
            var started = new List<string>();
            combo.PressHeavy();
            Run(combo, stamina, mana, 0.6f, started);
            Assert.AreEqual(new[] { "heavy" }, started.ToArray(), "the player is not left holding for nothing");
            Assert.AreEqual(10f, mana.Current, 0.001f);
        }

        [Test]
        public void AHeavyAfterALightIsTheChainHeavyAtOnceAndEndsTheChain()
        {
            var combo = Weapon(); var stamina = Stamina(); var mana = Mana(100f);
            var started = new List<string>();
            combo.Press();
            Run(combo, stamina, mana, 0.15f, started);
            combo.PressHeavy();
            Run(combo, stamina, mana, 0.15f, started);
            Assert.AreEqual(new[] { "light0", "chain heavy" }, started.ToArray(), "no charge inside a chain, even with the button held");
            Run(combo, stamina, mana, 1f, started);
            combo.Press();
            Run(combo, stamina, mana, 0.05f, started);
            Assert.AreEqual("light0", started[2], "after the heavy the chain starts again");
        }

        [Test]
        public void TheChargeCanBeCancelledAndAHeavySwingCannot()
        {
            var combo = Weapon(); var stamina = Stamina(); var mana = Mana(100f);
            combo.PressHeavy();
            Run(combo, stamina, mana, 0.1f, null);
            Assert.IsTrue(combo.CanBeCancelled);
            combo.Cancel();
            Assert.IsFalse(combo.IsAttacking);
            Assert.AreEqual(100f, stamina.Current, 0.001f, "a charge that is abandoned costs nothing");

            combo.ReleaseHeavy();
            combo.PressHeavy();
            combo.ReleaseHeavy();
            Run(combo, stamina, mana, 0.1f, null);
            Assert.AreEqual(AttackPhase.Windup, combo.Phase);
            Assert.IsFalse(combo.CanBeCancelled);
        }

        [Test]
        public void AHeavyPressWithAStaggeredEnemyInReachIsTheFinisherAtOnce()
        {
            var combo = Weapon(); var stamina = Stamina(); var mana = Mana(100f);
            var started = new List<string>();
            combo.PressHeavy(true);
            Run(combo, stamina, mana, 0.05f, started);
            Assert.AreEqual(new[] { "finisher" }, started.ToArray(), "no charge, even with the button held");
            Assert.IsTrue(combo.IsFinishing);
            Assert.AreEqual(100f, mana.Current, 0.001f);
        }

        [Test]
        public void TheFinisherComesOutOfAnyRecoveryIncludingAHeavys()
        {
            var combo = Weapon(); var stamina = Stamina(); var mana = Mana(100f);
            var started = new List<string>();
            combo.PressHeavy();
            combo.ReleaseHeavy();
            Run(combo, stamina, mana, 0.45f, started);
            Assert.AreEqual(AttackPhase.Recovery, combo.Phase);
            combo.PressHeavy(true);
            Run(combo, stamina, mana, 0.1f, started);
            Assert.AreEqual(new[] { "heavy", "finisher" }, started.ToArray());
        }

        [Test]
        public void ALightPressDoesNothingToACharge()
        {
            var combo = Weapon(); var stamina = Stamina(); var mana = Mana(100f);
            var started = new List<string>();
            combo.PressHeavy();
            Run(combo, stamina, mana, 0.1f, started);
            combo.Press();
            Run(combo, stamina, mana, 0.5f, started);
            Assert.IsEmpty(started);
            Assert.IsTrue(combo.IsCharging);
        }
    }
}
