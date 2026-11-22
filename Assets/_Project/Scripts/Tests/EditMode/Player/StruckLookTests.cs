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

        [Test]
        public void APlainBlowFlashesInTheWeaponsColourBelowWhite()
        {
            // The owner, 2026-11-22: a plain Katana blow lit the soldier's shield as a white disc. Every weapon's plain
            // blow now carries its slash colour, and the flash it gives is that hue, held below white.
            foreach (var weapon in new[] { Weapons.Katana(), Weapons.MassiveSword(), Weapons.Scythe(), Weapons.GreatSpear(), Weapons.GreatHammer(), Weapons.TwinKnives(), Weapons.ChainSickle(), Weapons.Fists() })
            {
                Color tint = PlayerCombat.CutTintOf(weapon, null);
                Assert.IsTrue(StruckLook.Tinted(tint), weapon.Name + " tints its blow");
                Color flash = Enemy.FlashColour(tint);
                Assert.LessOrEqual(Mathf.Min(flash.r, Mathf.Min(flash.g, flash.b)), StruckLook.PalestFlash + 1e-4f, weapon.Name + " flash is not white");
                Assert.Less(flash.maxColorComponent, 1f, weapon.Name + " flash stays below white");
            }
        }

        [Test]
        public void NoLandedBlowFlashesABodyWhite()
        {
            // The owner bans pure white. The Great Bow's arrows (2026-11-22) and a Massive Sword Life strike's spread
            // still lit whole soldiers white: they reached the body with no tint. Whatever a blow carries, the flash is a
            // hue held below white.
            var hits = new System.Collections.Generic.List<(string, Hit)>
            {
                ("a blow with no tint", new Hit(10f, 5f, Vector3.forward)),
                ("a reaction's blow with no tint", new Hit(10f, 5f, Vector3.forward, fromReaction: true)),
            };
            foreach (var look in SwingLooks.All) hits.Add((look.Name + "'s blow", new Hit(10f, 5f, Vector3.forward, tint: look.Tint)));
            foreach (var (what, hit) in hits)
            {
                Color flash = Enemy.FlashFor(hit);
                Assert.AreNotEqual(Enemy.StruckFlash, flash, what + " flashes plain white");
                Assert.LessOrEqual(Palest(flash), StruckLook.PalestFlash + 1e-4f, what + "'s flash is lit to white");
                Assert.Less(Peak(flash), 1f, what + "'s flash stays below white");
            }
        }

        [Test]
        public void TheGreatBowsShotsCarryTheWeaponsOrTheElementsTint()
        {
            var bow = Weapons.GreatBow();
            Assert.IsTrue(StruckLook.Tinted(PlayerCombat.BlowTintOf(default, bow, null)), "a plain shot carries the bow's colour");
            foreach (var info in Elements.All)
            {
                Color tint = PlayerCombat.BlowTintOf(default, bow, info);
                Assert.IsTrue(StruckLook.Tinted(tint), info.Name + ": the shot carries a tint");
                SameHue(info.Colour, tint, info.Name + ": the shot's tint is the element's");
            }
            foreach (var look in SwingLooks.All)
                Assert.AreEqual(look.Tint, PlayerCombat.BlowTintOf(look.Tint, bow, null), look.Name + ": the element look's tint wins");
        }

        [Test]
        public void NoSparkIsLitToWhite()
        {
            // The bow's arrow star, Ice's shatter, Lightning's crackle and the burn-away bolt were thrown in pale or plain
            // colours under materials lit past white, and their cores (3.2 times the hue) burned pure white. A spark's
            // colour keeps one channel low in its core and in what the brightest spark material (Burst, 6) makes of it.
            var colours = new System.Collections.Generic.List<(string, Color)> { ("white", Color.white), ("the chill", Clover.Gameplay.Afflictions.Spec(AfflictionKind.Chill).Colour) };
            foreach (var info in Elements.All) colours.Add((info.Name, info.Colour));
            foreach (var look in SwingLooks.All) colours.Add((look.Name + "'s trail", look.Tint));
            foreach (var weapon in new[] { Weapons.GreatBow(), Weapons.Katana(), Weapons.MassiveSword(), Weapons.Fists() }) colours.Add((weapon.Name, PlayerCombat.CutTintOf(weapon, null)));
            foreach (var (what, colour) in colours)
                foreach (float brightness in new[] { PlayerCombat.ImpactGlow, 0.25f, 0.3f })
                {
                    Color spark = StruckLook.Spark(colour, brightness);
                    Assert.AreEqual(brightness, Peak(spark), 1e-4f, what + ": lit to the brightness asked");
                    Assert.LessOrEqual(Palest(StruckLook.CoreOf(spark)), 0.7f, what + "'s spark core is lit to white");
                    Assert.LessOrEqual(Palest(spark) * 6f, 0.5f, what + "'s spark is lit to white by its material");
                }
            Assert.LessOrEqual(Palest(StruckLook.CoreOf(Enemy.SpiritSpark)), 0.7f, "the burn-away bolt's core is lit to white");
            // The ice crystals a blow throws up are lit by the sun: their palest channel stays low enough that a lit face keeps its blue.
            Assert.LessOrEqual(Palest(Clover.Player.Swings.SwingImpacts.IceCrystal), 0.5f, "the ice crystals are pale enough to burn white");
            // The spark keeps the hue's lean: Ice stays blue over red, Light gold over blue.
            Color ice = StruckLook.Spark(Clover.Gameplay.Afflictions.Spec(AfflictionKind.Chill).Colour, 0.22f);
            Assert.Greater(ice.b, ice.r, "Ice's spark is blue");
            Color light = StruckLook.Spark(Elements.ByName(Elements.Light).Colour, 0.22f);
            Assert.Greater(light.r, light.b, "Light's spark is gold");
        }

        [Test]
        public void TheAttackArcIsHeldBelowWhite()
        {
            // A surge's glow and every weapon's HDR slash colour drew a pure-white crescent (the surge-while-moving films of
            // 2026-11-22). Whatever the arc is drawn in, it keeps a channel low and is no brighter than one.
            var colours = new System.Collections.Generic.List<(string, Color)>();
            foreach (var info in Elements.All)
            {
                colours.Add((info.Name + "'s surge glow", info.Glow));
                colours.Add((info.Name + "'s lit arc", new Color(info.Colour.r * 1.9f, info.Colour.g * 1.9f, info.Colour.b * 1.9f, 0.95f)));
            }
            foreach (var weapon in new[] { Weapons.Katana(), Weapons.MassiveSword(), Weapons.GreatHammer(), Weapons.TwinKnives(), Weapons.Scythe(), Weapons.GreatSpear(), Weapons.ChainSickle(), Weapons.GreatBow(), Weapons.Fists() })
            {
                colours.Add((weapon.Name + "'s slash", weapon.SlashColour));
                colours.Add((weapon.Name + "'s tip", new Color(weapon.SlashColour.r * 1.4f, weapon.SlashColour.g * 1.4f, weapon.SlashColour.b * 1.4f, weapon.SlashColour.a)));
            }
            foreach (var (what, colour) in colours)
            {
                Color arc = StruckLook.Arc(colour);
                Assert.LessOrEqual(Palest(arc), StruckLook.ArcPalest + 1e-4f, what + " draws a white arc");
                Assert.LessOrEqual(Peak(arc), StruckLook.ArcPeak + 1e-4f, what + "'s arc is lit past one");
                Assert.AreEqual(colour.a, arc.a, 1e-4f, what + " keeps its see-through");
            }
            // The faint arc under a swing look is already below white and is left exactly as it was.
            var faint = new Color(0.66f, 0.63f, 0.56f, 0.18f);
            Assert.AreEqual(faint, StruckLook.Arc(faint));
            // The hue's lean is kept: a gold slash stays warmer than it is blue.
            Color gold = StruckLook.Arc(new Color(2.6f, 2.1f, 1.1f, 0.9f));
            Assert.Greater(gold.r, gold.b);
            // The glow shader's core is lit from the mesh's white, not the arc's colour: the player's arcs go without it.
            Assert.That(System.IO.File.ReadAllText("Assets/_Project/Scripts/Runtime/Player/PlayerCombat.cs").Contains("arcs.NoHotCore = true"), "the player's arcs keep a white core");
        }

        [Test]
        public void TheBowsSparksTheShatterTheCrackleAndTheBurnAwayAreNeverThrownPlain()
        {
            // The sources the films of 2026-11-22 caught throwing white: each now names a Spark colour.
            var plain = new System.Text.RegularExpressions.Regex(@"Vfx\.Play\(VfxKind\.(Shock|Bolt|Wisp|TintSpark)\b[^;]*;");
            foreach (var file in new[] { "Gameplay/Enemy.cs", "Player/PlantedArrows.cs", "Player/WeaponShots.cs", "Player/Trees/ElementSteel.cs", "Player/Swings/SwingImpacts.cs" })
            {
                string text = System.IO.File.ReadAllText("Assets/_Project/Scripts/Runtime/" + file);
                foreach (System.Text.RegularExpressions.Match call in plain.Matches(text))
                    Assert.That((call.Value.Contains("StruckLook.Spark(") || call.Value.Contains("SpiritSpark")) && !call.Value.Contains("Color.white"), file + " throws a spark not held below white: " + call.Value);
            }
        }

        [Test]
        public void EveryBlowThePlayerMakesIsGivenATint()
        {
            // Each Hit the player's code builds names its tint, so no new path lands untinted and falls to the backstop
            // (PlayerDefence's own hits are the player's, not blows on a body).
            foreach (var file in System.IO.Directory.GetFiles("Assets/_Project/Scripts/Runtime/Player", "*.cs", System.IO.SearchOption.AllDirectories))
            {
                if (file.EndsWith("PlayerDefence.cs")) continue;
                string text = System.IO.File.ReadAllText(file);
                for (int at = text.IndexOf("new Hit("); at >= 0; at = text.IndexOf("new Hit(", at + 1))
                {
                    int end = text.IndexOf(';', at);
                    string statement = text.Substring(at, (end < 0 ? text.Length : end) - at);
                    Assert.That(statement.Contains("tint:"), System.IO.Path.GetFileName(file) + ": a Hit made with no tint: " + statement.Split('\n')[0]);
                }
            }
        }
    }
}
