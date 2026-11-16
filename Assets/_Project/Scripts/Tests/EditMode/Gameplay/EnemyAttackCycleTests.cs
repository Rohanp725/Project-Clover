using Clover.Gameplay;
using NUnit.Framework;

namespace Clover.Tests
{
    public class EnemyAttackCycleTests
    {
        private static EnemyAttackCycle Cycle()
        {
            var cycle = new EnemyAttackCycle { RestSeconds = 1f, TelegraphSeconds = 0.5f, StrikeSeconds = 0.2f, RedEvery = 3 };
            cycle.Reset();
            return cycle;
        }

        // Runs the cycle and returns how many blows landed.
        private static int Run(EnemyAttackCycle cycle, float seconds, bool mayAttack)
        {
            int blows = 0;
            for (float t = 0f; t < seconds; t += 0.01f)
                if (cycle.Tick(0.01f, mayAttack)) blows++;
            return blows;
        }

        [Test]
        public void ItRestsShowsTheAttackThenStrikesOnce()
        {
            var cycle = Cycle();
            Assert.AreEqual(0, Run(cycle, 0.9f, true));
            Assert.AreEqual(EnemyAttackPhase.Rest, cycle.Phase);
            Assert.AreEqual(0, Run(cycle, 0.3f, true));
            Assert.AreEqual(EnemyAttackPhase.Telegraph, cycle.Phase);
            Assert.AreEqual(1, Run(cycle, 0.4f, true));
            Assert.AreEqual(EnemyAttackPhase.Strike, cycle.Phase);
        }

        [Test]
        public void ItWaitsForATargetButCarriesThroughAnAttackAlreadyShown()
        {
            var cycle = Cycle();
            Assert.AreEqual(0, Run(cycle, 5f, false));
            Assert.AreEqual(EnemyAttackPhase.Rest, cycle.Phase);
            Run(cycle, 0.05f, true);
            Assert.AreEqual(EnemyAttackPhase.Telegraph, cycle.Phase, "it was ready the moment the target came in range");
            Assert.AreEqual(1, Run(cycle, 0.6f, false), "the blow lands even though the target has left");
        }

        [Test]
        public void EveryThirdAttackIsRed()
        {
            var cycle = Cycle();
            var red = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                while (cycle.Phase != EnemyAttackPhase.Telegraph) cycle.Tick(0.01f, true);
                red[i] = cycle.IsRed;
                while (cycle.Phase == EnemyAttackPhase.Telegraph) cycle.Tick(0.01f, true);
            }
            Assert.AreEqual(new[] { false, false, true, false, false, true }, red);
        }

        [Test]
        public void AResetDropsTheAttackInProgress()
        {
            var cycle = Cycle();
            Run(cycle, 1.2f, true);
            Assert.AreEqual(EnemyAttackPhase.Telegraph, cycle.Phase);
            cycle.Reset();
            Assert.AreEqual(EnemyAttackPhase.Rest, cycle.Phase);
            Assert.AreEqual(0, Run(cycle, 0.9f, true));
        }
    }
}
