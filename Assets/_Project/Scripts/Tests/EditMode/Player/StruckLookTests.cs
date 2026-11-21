using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Swings;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The owner's choice of 2026-11-19, "Tint and soften": when the weapon carries an element, a landed blow's flash on
    /// the body and its cut streak take the colour the element's swing trail is drawn in, at about half strength, so the
    /// trail shows through on contact. A plain weapon keeps the white flash and the white streak exactly as they were.
    /// </summary>
    public class StruckLookTests
    {
        // The cut streak's two quads as PlayerBuilder makes them: a white-hot needle on a larger one of ink.
        private static readonly Color StreakWhite = new Color(4.5f, 4.4f, 4f, 1f);
        private static readonly Color StreakInk = new Color(0.05f, 0.04f, 0.08f, 1f);

        private static float Peak(Color colour) => Mathf.Max(colour.r, Mathf.Max(colour.g, colour.b));
        private static float Palest(Color colour) => Mathf.Min(colour.r, Mathf.Min(colour.g, colour.b));

        private static void SameHue(Color expected, Color actual, string what)
        {
            float expectedPeak = Peak(expected), actualPeak = Peak(actual);
            Assert.Greater(actualPeak, 0f, what);
            Assert.AreEqual(expected.r / expectedPeak, actual.r / actualPeak, 0.01f, what + " red");
            Assert.AreEqual(expected.g / expectedPeak, actual.g / actualPeak, 0.01f, what + " green");
            Assert.AreEqual(expected.b / expectedPeak, actual.b / actualPeak, 0.01f, what + " blue");
        }

        [Test]
        public void APlainBlowKeepsTheWhiteFlashAndStreakAtFullStrength()
        {
            var plain = new Hit(10f, 5f, Vector3.forward);
            Assert.IsFalse(StruckLook.Tinted(plain.Tint), "a blow made without a tint carries none");
            Assert.AreEqual(Enemy.StruckFlash, Enemy.FlashColour(plain.Tint));
            Assert.AreEqual(StreakWhite, StruckLook.Stroke(StreakWhite, plain.Tint));
            Assert.AreEqual(StreakInk, StruckLook.Ink(StreakInk, plain.Tint));
            Assert.AreEqual(Enemy.StruckFlash, StruckLook.Flash(Enemy.StruckFlash, default));
        }

        [Test]
        public void TheSofteningIsAboutHalf()
        {
            Assert.That(StruckLook.TintStrength, Is.InRange(0.35f, 0.65f));
            Assert.That(StruckLook.FlashStrength, Is.InRange(0.3f, 0.75f));
        }

        [Test]
        public void ABlowCarryingAnElementGetsItsTrailsColourSoftened()
        {
            foreach (var look in SwingLooks.All)
            {
                Color tint = look.Tint;
                Assert.IsTrue(StruckLook.Tinted(tint), look.Name + " has no tint");
                var hit = new Hit(10f, 5f, Vector3.forward, tint: tint);
                Assert.AreEqual(tint, hit.Tint, look.Name + ": the blow keeps its tint");

                Color flash = Enemy.FlashColour(hit.Tint);
                SameHue(tint, flash, look.Name + "'s flash");
                Assert.LessOrEqual(Peak(flash), Peak(Enemy.StruckFlash) * StruckLook.FlashStrength + 1e-4f, look.Name + "'s flash is softened");
                Assert.LessOrEqual(Palest(flash), StruckLook.PalestFlash + 1e-4f, look.Name + "'s flash is lit to white");
                // Softened, not put out: either at the chosen strength or held just under white.
                Assert.That(Mathf.Abs(Peak(flash) - Peak(Enemy.StruckFlash) * StruckLook.FlashStrength) < 1e-4f || Mathf.Abs(Palest(flash) - StruckLook.PalestFlash) < 1e-4f, look.Name + "'s flash is dimmer than it needs to be");

                Color streak = StruckLook.Stroke(StreakWhite, hit.Tint);
                SameHue(tint, streak, look.Name + "'s streak");
                Assert.LessOrEqual(Peak(streak), Peak(StreakWhite) * StruckLook.TintStrength + 1e-4f, look.Name + "'s streak is softened");
                Assert.LessOrEqual(Palest(streak), StruckLook.PalestStroke + 1e-4f, look.Name + "'s streak is lit to white");
                Assert.That(Mathf.Abs(Peak(streak) - Peak(StreakWhite) * StruckLook.TintStrength) < 1e-4f || Mathf.Abs(Palest(streak) - StruckLook.PalestStroke) < 1e-4f, look.Name + "'s streak is dimmer than it needs to be");
                Assert.AreEqual(StreakWhite.a * StruckLook.TintStrength, streak.a, 1e-4f, look.Name + "'s streak lets the trail through");

                Color ink = StruckLook.Ink(StreakInk, hit.Tint);
                Assert.AreEqual(StreakInk.a * StruckLook.TintStrength, ink.a, 1e-4f, look.Name + "'s ink lets the trail through");
            }
        }

        [Test]
        public void TheColouredElementsAreNotWhite()
        {
            // Metal's trail is steel, so its tint is pale; every other one is its colour, Wind's the teal of its trail.
            foreach (var look in SwingLooks.All)
            {
                if (look.Name == Elements.Metal) continue;
                Color tint = look.Tint;
                float low = Mathf.Min(tint.r, Mathf.Min(tint.g, tint.b));
                Assert.Less(low / Peak(tint), 0.8f, look.Name + "'s tint reads as white");
            }
        }

        [Test]
        public void OnlyWindsBlowsGoRoundTheBody()
        {
            // The owner, 2026-11-20: "Wind is a line through the enemy". Its blows draw no cut streak; every other element's do.
            foreach (var look in SwingLooks.All)
                Assert.AreEqual(look.Name == Elements.Wind, look.CutsRound, look.Name);
        }
    }
}
