using Clover.Player;
using Clover.Player.Build;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Proving Ground's element altars (Phase 2, task 2.4): with nothing carried an altar gives its element; with one
    /// carried another altar adds its element second, and the same altar again goes all in; with two carried, or all in,
    /// an altar starts again with its element alone. The prompt says which before the press.
    /// </summary>
    public class AltarChoiceTests
    {
        private static (string, string) After(string first, string second, string altar)
        {
            AltarChoice.Next(first, second, altar, out string newFirst, out string newSecond);
            return (newFirst, newSecond);
        }

        [Test]
        public void WithNothingCarriedTheAltarGivesTheFirst()
        {
            Assert.AreEqual((Elements.Fire, ""), After("", "", Elements.Fire));
            Assert.AreEqual("Carry Fire", AltarChoice.Prompt("", "", Elements.Fire));
            Assert.AreEqual("Fire", AltarChoice.Said(Elements.Fire, ""));
        }

        [Test]
        public void WithOneCarriedAnotherAltarAddsTheSecond()
        {
            Assert.AreEqual((Elements.Fire, Elements.Lightning), After(Elements.Fire, "", Elements.Lightning));
            Assert.AreEqual("Carry Lightning as second", AltarChoice.Prompt(Elements.Fire, "", Elements.Lightning));
            Assert.AreEqual("Fire, then Lightning", AltarChoice.Said(Elements.Fire, Elements.Lightning));
        }

        [Test]
        public void TheSameAltarAgainGoesAllIn()
        {
            Assert.AreEqual((Elements.Fire, Elements.Fire), After(Elements.Fire, "", Elements.Fire));
            Assert.AreEqual("Carry Fire all in", AltarChoice.Prompt(Elements.Fire, "", Elements.Fire));
            Assert.AreEqual("Fire all in", AltarChoice.Said(Elements.Fire, Elements.Fire));
        }

        [Test]
        public void WithTwoCarriedOrAllInAnAltarStartsAgainAlone()
        {
            Assert.AreEqual((Elements.Ice, ""), After(Elements.Fire, Elements.Lightning, Elements.Ice));
            Assert.AreEqual((Elements.Fire, ""), After(Elements.Fire, Elements.Lightning, Elements.Fire), "the first's own altar");
            Assert.AreEqual((Elements.Lightning, ""), After(Elements.Fire, Elements.Lightning, Elements.Lightning), "the second's own altar");
            Assert.AreEqual((Elements.Ice, ""), After(Elements.Fire, Elements.Fire, Elements.Ice), "all in");
            Assert.AreEqual("Carry Ice alone", AltarChoice.Prompt(Elements.Fire, Elements.Lightning, Elements.Ice));
            Assert.AreEqual("Carry Fire alone", AltarChoice.Prompt(Elements.Fire, Elements.Fire, Elements.Fire));
        }

        [Test]
        public void NullsReadAsNothingCarried()
        {
            Assert.AreEqual((Elements.Water, ""), After(null, null, Elements.Water));
            Assert.AreEqual("Carry Water", AltarChoice.Prompt(null, null, Elements.Water));
        }
    }
}
