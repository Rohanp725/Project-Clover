using Clover.Player;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    public class DodgeRollTests
    {
        private static StaminaPool Stamina(float amount = 100f)
        {
            var pool = new StaminaPool { Maximum = amount };
            pool.Fill();
            return pool;
        }

        private static DodgeRoll Roll() => new DodgeRoll
        {
            Distance = 4f, Duration = 0.4f, StaminaCost = 25f, InvulnerableFrom = 0.05f, InvulnerableUntil = 0.25f, Recovery = 0.1f,
        };

        [Test]
        public void ARollCoversItsDistanceInItsDurationAlongTheGivenDirection()
        {
            var roll = Roll();
            Assert.IsTrue(roll.TryStart(new Vector3(0f, 0f, 3f), Stamina()));
            Vector3 travelled = Vector3.zero;
            for (int i = 0; i < 100 && roll.IsRolling; i++) travelled += roll.Tick(0.016f);
            Assert.IsFalse(roll.IsRolling);
            Assert.AreEqual(4f, travelled.z, 0.001f);
            Assert.AreEqual(0f, travelled.x, 0.001f);
        }

        [Test]
        public void StartingCostsStaminaAndFailsWithoutEnough()
        {
            var stamina = Stamina(30f);
            var roll = Roll();
            Assert.IsTrue(roll.TryStart(Vector3.forward, stamina));
            Assert.AreEqual(5f, stamina.Current, 0.001f);
            while (roll.IsRolling) roll.Tick(0.05f);
            roll.Tick(1f);
            Assert.IsFalse(roll.TryStart(Vector3.forward, stamina));
            Assert.AreEqual(5f, stamina.Current, 0.001f);
        }

        [Test]
        public void HitsPassThroughOnlyInsideTheWindow()
        {
            var roll = Roll();
            roll.TryStart(Vector3.right, Stamina());
            Assert.IsFalse(roll.IsInvulnerable, "not at the very start");
            roll.Tick(0.1f);
            Assert.IsTrue(roll.IsInvulnerable);
            roll.Tick(0.2f);
            Assert.IsFalse(roll.IsInvulnerable, "the tail of the roll can be hit");
            Assert.IsTrue(roll.IsRolling);
        }

        [Test]
        public void ASecondRollCannotStartDuringTheRollOrItsRecovery()
        {
            var stamina = Stamina();
            var roll = Roll();
            roll.TryStart(Vector3.forward, stamina);
            Assert.IsFalse(roll.TryStart(Vector3.forward, stamina));
            while (roll.IsRolling) roll.Tick(0.05f);
            Assert.IsFalse(roll.TryStart(Vector3.forward, stamina), "still recovering");
            roll.Tick(0.11f);
            Assert.IsTrue(roll.TryStart(Vector3.forward, stamina));
            Assert.AreEqual(50f, stamina.Current, 0.001f, "the refused attempts cost nothing");
        }

        [Test]
        public void ARollWithNoDirectionDoesNotStart()
        {
            var stamina = Stamina();
            Assert.IsFalse(Roll().TryStart(Vector3.zero, stamina));
            Assert.AreEqual(100f, stamina.Current, 0.001f);
        }
    }
}
