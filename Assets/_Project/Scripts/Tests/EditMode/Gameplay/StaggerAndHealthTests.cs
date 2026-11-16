using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    public class StaggerAndHealthTests
    {
        [Test]
        public void HealthLosesOnlyWhatItHas()
        {
            var health = new Health { Maximum = 30f };
            health.Fill();
            Assert.AreEqual(20f, health.Damage(20f), 0.001f);
            Assert.AreEqual(10f, health.Damage(20f), 0.001f);
            Assert.IsTrue(health.IsDead);
            Assert.AreEqual(0f, health.Damage(5f), 0.001f);
        }

        [Test]
        public void BlowsFillTheMeterAndTheOneThatFillsItStaggers()
        {
            var meter = new StaggerMeter { Maximum = 30f };
            meter.Reset();
            Assert.IsFalse(meter.Add(12f));
            Assert.IsFalse(meter.Add(12f));
            Assert.IsTrue(meter.Add(12f));
            Assert.IsTrue(meter.IsStaggered);
            Assert.IsFalse(meter.Add(12f), "no stagger is added while staggered");
        }

        [Test]
        public void TheStaggerEndsAfterItsTimeAndTheMeterStartsEmpty()
        {
            var meter = new StaggerMeter { Maximum = 10f, StaggeredSeconds = 3f };
            meter.Reset();
            meter.Add(10f);
            meter.Tick(2.9f);
            Assert.IsTrue(meter.IsStaggered);
            meter.Tick(0.2f);
            Assert.IsFalse(meter.IsStaggered);
            Assert.AreEqual(0f, meter.Value, 0.001f);
        }

        [Test]
        public void TheMeterDrainsOnlyAfterBeingLeftAlone()
        {
            var meter = new StaggerMeter { Maximum = 100f, DrainDelay = 2f, DrainPerSecond = 25f };
            meter.Reset();
            meter.Add(50f);
            meter.Tick(1.9f);
            Assert.AreEqual(50f, meter.Value, 0.001f);
            meter.Tick(0.1f);
            meter.Tick(1f);
            Assert.AreEqual(25f, meter.Value, 2.6f);
            meter.Tick(10f);
            Assert.AreEqual(0f, meter.Value, 0.001f);
        }

        [Test]
        public void ManaIsOnlyGainedAndSpentNeverRefilled()
        {
            var mana = new ManaPool { Maximum = 20f };
            mana.Empty();
            Assert.IsFalse(mana.TrySpend(5f));
            mana.Gain(15f);
            mana.Gain(15f);
            Assert.AreEqual(20f, mana.Current, 0.001f, "capped at the maximum");
            Assert.IsTrue(mana.TrySpend(5f));
            Assert.AreEqual(15f, mana.Current, 0.001f);
        }
    }
}
