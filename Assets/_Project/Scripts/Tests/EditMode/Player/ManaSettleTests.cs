using Clover.Player;
using Clover.Player.Magic;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>Mana for the big moves (STEEL_AND_ELEMENT.md 3.2): out of a fight it settles toward half; a fight starts at least half full.</summary>
    public class ManaSettleTests
    {
        private static ManaPool Pool(float start)
        {
            var pool = new ManaPool();
            pool.Gain(start);
            return pool;
        }

        [Test]
        public void OutOfAFightManaRisesToHalfAndStops()
        {
            var pool = Pool(10f);
            pool.Settle(2f, false);
            Assert.AreEqual(10f + 2f * pool.SettleRate, pool.Current, 1e-3f);
            for (int i = 0; i < 100; i++) pool.Settle(0.5f, false);
            Assert.AreEqual(50f, pool.Current, 1e-3f, "it stops at half");
        }

        [Test]
        public void ManaAboveHalfIsKept()
        {
            var pool = Pool(80f);
            for (int i = 0; i < 100; i++) pool.Settle(0.5f, false);
            Assert.AreEqual(80f, pool.Current, 1e-3f);
        }

        [Test]
        public void InAFightNothingSettles()
        {
            var pool = Pool(10f);
            pool.Settle(10f, true);
            Assert.AreEqual(10f, pool.Current, 1e-3f);
        }

        [Test]
        public void AtLeastRaisesButNeverLowers()
        {
            var pool = Pool(10f);
            pool.AtLeast(pool.SettleLevel);
            Assert.AreEqual(50f, pool.Current, 1e-3f);
            pool.Gain(30f);
            pool.AtLeast(pool.SettleLevel);
            Assert.AreEqual(80f, pool.Current, 1e-3f);
            pool.AtLeast(500f);
            Assert.AreEqual(100f, pool.Current, 1e-3f, "never past the most");
        }

        [Test]
        public void WithNoManaThereIsNothingToSettle()
        {
            var pool = Pool(0f);
            pool.Instead = cost => true;
            pool.Settle(10f, false);
            pool.AtLeast(50f);
            Assert.AreEqual(0f, pool.Fraction);
            pool.Instead = null;
            Assert.AreEqual(0f, pool.Current, 1e-3f);
        }

        [Test]
        public void AFightBeginsWithAnAwareEnemyAndEndsAfterEightCalmSeconds()
        {
            var clock = new FightClock();
            Assert.IsFalse(clock.InFight);
            Assert.IsTrue(clock.Tick(0.25f, true), "the tick it begins is reported");
            Assert.IsTrue(clock.InFight);
            Assert.IsFalse(clock.Tick(0.25f, true), "only once");
            for (int i = 0; i < 31; i++) clock.Tick(0.25f, false);
            Assert.IsTrue(clock.InFight, "7.75 s calm is still the fight");
            clock.Tick(0.25f, false);
            Assert.IsFalse(clock.InFight, "8 s calm ends it");
        }

        [Test]
        public void AnAwareEnemyAgainStartsTheCalmOver()
        {
            var clock = new FightClock();
            clock.Tick(0.1f, true);
            for (int i = 0; i < 7; i++) clock.Tick(1f, false);
            clock.Tick(0.1f, true);
            for (int i = 0; i < 7; i++) clock.Tick(1f, false);
            Assert.IsTrue(clock.InFight);
            clock.Reset();
            Assert.IsFalse(clock.InFight);
        }

        [Test]
        public void AfterAFightManaAtTenRisesToFiftyAndManaAtEightyStays()
        {
            // The probe's acceptance (IMPLEMENTATION.md task 1.4), as rules.
            var clock = new FightClock();
            var low = Pool(10f);
            var high = Pool(80f);
            clock.Tick(0.25f, true);
            float t = 0f;
            while (t < 30f)
            {
                clock.Tick(0.25f, false);
                low.Settle(0.25f, clock.InFight);
                high.Settle(0.25f, clock.InFight);
                if (t + 0.25f < 7.9f) Assert.AreEqual(10f, low.Current, 1e-3f, "nothing settles until the fight is over");
                t += 0.25f;
            }
            Assert.AreEqual(50f, low.Current, 1e-3f);
            Assert.AreEqual(80f, high.Current, 1e-3f);
        }
    }
}
