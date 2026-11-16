using System.Collections.Generic;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    public class WeaponComboTests
    {
        private const float Frame = 1f / 60f;

        private static StaminaPool Stamina(float amount = 100f)
        {
            var pool = new StaminaPool { Maximum = amount, RegenerationPerSecond = 0f };
            pool.Fill();
            return pool;
        }

        private static WeaponCombo Chain()
        {
            var combo = new WeaponCombo { BufferTime = 0.3f, ChainGrace = 0.35f, ChainDelay = 0.02f };
            for (int i = 0; i < 3; i++)
                combo.Steps.Add(new AttackStep { Name = "a" + i, Windup = 0.1f, Active = 0.1f, Recovery = 0.2f, StaminaCost = 10f });
            combo.Reset();
            return combo;
        }

        // Runs the chain for a time and records the index of every attack that started and every hit that landed.
        private static void Run(WeaponCombo combo, StaminaPool stamina, float seconds, List<int> started, List<int> hits)
        {
            for (float t = 0f; t < seconds; t += Frame)
            {
                var events = combo.Tick(Frame, stamina);
                if ((events & ComboEvents.Started) != 0) started?.Add(combo.StepIndex);
                if ((events & ComboEvents.Hit) != 0) hits?.Add(combo.StepIndex);
            }
        }

        [Test]
        public void APressStartsTheFirstAttackWhichHitsOnceAndThenEnds()
        {
            var combo = Chain(); var stamina = Stamina();
            var started = new List<int>(); var hits = new List<int>();
            combo.Press();
            Run(combo, stamina, 0.05f, started, hits);
            Assert.AreEqual(AttackPhase.Windup, combo.Phase);
            Assert.IsEmpty(hits, "nothing lands during the wind-up");
            Run(combo, stamina, 1f, started, hits);
            Assert.AreEqual(new[] { 0 }, started.ToArray());
            Assert.AreEqual(new[] { 0 }, hits.ToArray());
            Assert.IsFalse(combo.IsAttacking);
            Assert.AreEqual(90f, stamina.Current, 0.001f);
        }

        [Test]
        public void APressDuringAnAttackChainsIntoTheNextAsSoonAsTheSwingIsOver()
        {
            var combo = Chain(); var stamina = Stamina();
            var started = new List<int>();
            combo.Press();
            Run(combo, stamina, 0.15f, started, null);
            combo.Press();
            Run(combo, stamina, 0.1f, started, null);
            Assert.AreEqual(new[] { 0, 1 }, started.ToArray(), "the second attack starts in the first one's recovery, not after it");
        }

        [Test]
        public void ThreePressesGiveThreeAttacksAndAFourthStartsTheChainAgain()
        {
            var combo = Chain(); var stamina = Stamina();
            var started = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                combo.Press();
                Run(combo, stamina, 0.3f, started, null);
            }
            Run(combo, stamina, 1f, started, null);
            Assert.AreEqual(new[] { 0, 1, 2, 0 }, started.ToArray());
        }

        [Test]
        public void WaitingLongerThanTheGraceStartsFromTheFirstAttack()
        {
            var combo = Chain(); var stamina = Stamina();
            var started = new List<int>();
            combo.Press();
            Run(combo, stamina, 0.4f + 0.2f, started, null);
            combo.Press();
            Run(combo, stamina, 0.05f, started, null);
            Assert.AreEqual(new[] { 0, 1 }, started.ToArray(), "inside the grace the chain continues");

            combo = Chain(); started.Clear();
            combo.Press();
            Run(combo, stamina, 0.4f + 0.6f, started, null);
            combo.Press();
            Run(combo, stamina, 0.05f, started, null);
            Assert.AreEqual(new[] { 0, 0 }, started.ToArray(), "after the grace it starts again");
        }

        [Test]
        public void APressIsForgottenAfterTheBufferTime()
        {
            var combo = Chain(); var stamina = Stamina();
            var started = new List<int>();
            combo.Press();
            Run(combo, stamina, 0.02f, started, null);
            // Pressed at the very start of the attack: by the time the swing is over, 0.2 s later, it is still remembered.
            combo.Press();
            Run(combo, stamina, 0.3f, started, null);
            Assert.AreEqual(2, started.Count);

            combo = Chain(); started.Clear();
            combo.BufferTime = 0.05f;
            combo.Press();
            Run(combo, stamina, 0.02f, started, null);
            combo.Press();
            Run(combo, stamina, 1f, started, null);
            Assert.AreEqual(1, started.Count, "a press too early is dropped");
        }

        [Test]
        public void AnAttackDoesNotStartWithoutStamina()
        {
            var combo = Chain(); var stamina = Stamina(5f);
            var started = new List<int>();
            combo.Press();
            Run(combo, stamina, 0.5f, started, null);
            Assert.IsEmpty(started);
            Assert.AreEqual(5f, stamina.Current, 0.001f);
        }

        [Test]
        public void OnlyTheRecoveryCanBeCancelledAndCancellingKeepsTheChain()
        {
            var combo = Chain(); var stamina = Stamina();
            var started = new List<int>();
            combo.Press();
            Run(combo, stamina, 0.05f, started, null);
            Assert.IsFalse(combo.CanBeCancelled, "not in the wind-up");
            Run(combo, stamina, 0.1f, started, null);
            Assert.IsFalse(combo.CanBeCancelled, "not during the swing");
            Run(combo, stamina, 0.1f, started, null);
            Assert.IsTrue(combo.CanBeCancelled);
            combo.Cancel();
            Assert.IsFalse(combo.IsAttacking);
            combo.Press();
            Run(combo, stamina, 0.05f, started, null);
            Assert.AreEqual(new[] { 0, 1 }, started.ToArray());
        }

        [Test]
        public void APressMadeBeforeACancelDoesNotFireAfterIt()
        {
            var combo = Chain(); var stamina = Stamina();
            var started = new List<int>();
            combo.Press();
            Run(combo, stamina, 0.25f, started, null);
            combo.Press();
            combo.Cancel();
            Run(combo, stamina, 0.5f, started, null);
            Assert.AreEqual(new[] { 0 }, started.ToArray());
        }

        [Test]
        public void WhileIdleAPressFadesAndTheChainGraceRunsOut()
        {
            var combo = Chain(); var stamina = Stamina();
            var started = new List<int>();
            combo.Press();
            Run(combo, stamina, 0.25f, started, null);
            combo.Cancel();
            combo.Press();
            combo.Idle(0.5f);
            Run(combo, stamina, 0.2f, started, null);
            Assert.AreEqual(new[] { 0 }, started.ToArray(), "the press faded during the roll");
            combo.Press();
            Run(combo, stamina, 0.05f, started, null);
            Assert.AreEqual(new[] { 0, 0 }, started.ToArray(), "and the chain starts again from the first attack");
        }

        [Test]
        public void ProgressRunsFromZeroToOneAcrossTheWholeAttack()
        {
            var combo = Chain(); var stamina = Stamina();
            combo.Press();
            combo.Tick(Frame, stamina);
            Assert.Less(combo.StepProgress, 0.1f);
            Run(combo, stamina, 0.2f, null, null);
            Assert.AreEqual(0.5f, combo.StepProgress, 0.06f);
        }
    }
}
