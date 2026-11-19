using Clover.Player.Magic;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>R1 and R2 together (IMPLEMENTATION.md 3.5): a chord only in the first moment of a light attack.</summary>
    public class ChordRulesTests
    {
        [Test]
        public void HeavyJustAfterLightInEarlyWindupIsTheChord()
        {
            var chord = new ChordRules();
            chord.Light();
            chord.Tick(0.03f);
            Assert.IsTrue(chord.Heavy(0.03f));
            Assert.IsFalse(chord.Heavy(0.03f), "one chord a light press");
        }

        [Test]
        public void HeavyLaterIsNotTheChord()
        {
            var chord = new ChordRules();
            chord.Light();
            chord.Tick(0.12f);
            Assert.IsFalse(chord.Heavy(0.12f), "too long after the light press");

            chord.Light();
            chord.Tick(0.02f);
            Assert.IsFalse(chord.Heavy(0.2f), "the light step is past its first moment");
            Assert.IsFalse(chord.Heavy(-1f), "no light step is winding up");
        }

        [Test]
        public void HeavyWithNoLightIsNeverTheChord()
        {
            var chord = new ChordRules();
            Assert.IsFalse(chord.Heavy(0f));
            chord.Light();
            chord.Reset();
            Assert.IsFalse(chord.Heavy(0f));
        }
    }
}
