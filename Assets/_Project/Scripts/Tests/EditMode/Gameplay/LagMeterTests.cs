using Clover.Core;
using NUnit.Framework;

namespace Clover.Tests
{
    public class LagMeterTests
    {
        [Test]
        public void ThePalePieceHoldsThenDrainsToTheBar()
        {
            var meter = new LagMeter { Hold = 0.45f, DrainPerSecond = 1f };
            meter.Snap(1f);
            // The bar has dropped to half: the piece stays where the bar was while it holds.
            Assert.AreEqual(1f, meter.Tick(0.5f, 0.2f), 0.001f);
            Assert.AreEqual(1f, meter.Tick(0.5f, 0.2f), 0.001f);
            Assert.AreEqual(1f, meter.Tick(0.5f, 0.1f), 0.001f);
            // Then it drains at its rate, and stops at the bar.
            Assert.AreEqual(0.8f, meter.Tick(0.5f, 0.2f), 0.001f);
            Assert.AreEqual(0.5f, meter.Tick(0.5f, 1f), 0.001f);
            Assert.AreEqual(0.5f, meter.Tick(0.5f, 1f), 0.001f);
        }

        [Test]
        public void ARisingBarTakesThePieceWithItAndHoldsAgain()
        {
            var meter = new LagMeter { Hold = 0.4f, DrainPerSecond = 1f };
            meter.Snap(0.3f);
            Assert.AreEqual(0.7f, meter.Tick(0.7f, 0.1f), 0.001f);
            // A fresh loss after the rise is held for the whole time again.
            Assert.AreEqual(0.7f, meter.Tick(0.2f, 0.3f), 0.001f);
            Assert.AreEqual(0.7f, meter.Tick(0.2f, 0.3f), 0.001f);
            Assert.Less(meter.Tick(0.2f, 0.3f), 0.7f);
        }

        [Test]
        public void SnapPutsThePieceOnTheBar()
        {
            var meter = new LagMeter();
            meter.Tick(0.1f, 0.05f);
            meter.Snap(0.25f);
            Assert.AreEqual(0.25f, meter.Fraction, 0.001f);
        }
    }
}
