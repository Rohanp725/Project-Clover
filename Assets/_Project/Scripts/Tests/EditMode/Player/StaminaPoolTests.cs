using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    public class StaminaPoolTests
    {
        private static StaminaPool Full()
        {
            var pool = new StaminaPool { Maximum = 100f, RegenerationPerSecond = 30f, RegenerationDelay = 0.8f };
            pool.Fill();
            return pool;
        }

        [Test]
        public void SpendingTakesTheCostAndRefusesWhenThereIsNotEnough()
        {
            var pool = Full();
            Assert.IsTrue(pool.TrySpend(60f));
            Assert.AreEqual(40f, pool.Current, 0.001f);
            Assert.IsFalse(pool.TrySpend(60f));
            Assert.AreEqual(40f, pool.Current, 0.001f);
        }

        [Test]
        public void StaminaWaitsForTheDelayThenRefillsToTheMaximum()
        {
            var pool = Full();
            pool.TrySpend(50f);
            pool.Tick(0.5f);
            Assert.AreEqual(50f, pool.Current, 0.001f, "no refill inside the delay");
            pool.Tick(0.5f);
            Assert.AreEqual(65f, pool.Current, 0.001f, "refill at 30 per second once the delay has passed");
            pool.Tick(10f);
            Assert.AreEqual(100f, pool.Current, 0.001f);
        }

        [Test]
        public void ARefusedSpendDoesNotRestartTheDelay()
        {
            var pool = Full();
            pool.TrySpend(90f);
            pool.Tick(1f);
            float before = pool.Current;
            pool.TrySpend(999f);
            pool.Tick(0.1f);
            Assert.Greater(pool.Current, before);
        }
    }
}
