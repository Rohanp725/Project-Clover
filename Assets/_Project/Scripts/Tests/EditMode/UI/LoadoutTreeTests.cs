using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Clover.UI.Toolkit;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Loadout's tree and the tokens (ui_rest_fire_plan.md 5.3 item 1), read as text so they run without the editor:
    /// every place the stick map names is in Loadout.uxml, focusable, and nothing else is; every text-on-background pair
    /// is 4.5 to 1 or more from the colours in Tokens.uss; every type size is 24 or more and grows at 125% and 150%.
    /// </summary>
    public class LoadoutTreeTests
    {
        private const string UxmlPath = "Assets/_Project/UI/Loadout/Loadout.uxml";
        private const string TokensPath = "Assets/_Project/UI/Theme/Tokens.uss";

        private static XmlDocument Tree()
        {
            var document = new XmlDocument();
            document.Load(UxmlPath);
            return document;
        }

        private static IEnumerable<XmlElement> Elements(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (!(child is XmlElement element)) continue;
                yield return element;
                foreach (var below in Elements(element)) yield return below;
            }
        }

        private static bool HasClass(XmlElement element, string name) =>
            Array.IndexOf(element.GetAttribute("class").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), name) >= 0;

        [Test]
        public void EveryPlaceOfTheStickMapIsInTheTreeFocusableAndNothingElseIs()
        {
            var named = new Dictionary<string, XmlElement>();
            var focusable = new List<string>();
            foreach (var element in Elements(Tree()))
            {
                string name = element.GetAttribute("name");
                if (name.Length > 0)
                {
                    Assert.IsFalse(named.ContainsKey(name), "two elements are named " + name);
                    named[name] = element;
                }
                if (element.GetAttribute("focusable") == "true") focusable.Add(name);
            }
            foreach (string place in LoadoutPlaces.Names)
            {
                Assert.IsTrue(named.TryGetValue(place, out var element), place + " is not in the tree");
                Assert.AreEqual("true", element.GetAttribute("focusable"), place + " is not focusable");
                Assert.IsTrue(HasClass(element, "place"), place + " has no place class");
                bool framed = false;
                foreach (XmlNode child in element.ChildNodes)
                    if (child is XmlElement inner && HasClass(inner, "focus-frame")) framed = true;
                Assert.IsTrue(framed, place + " has no focus frame");
            }
            CollectionAssert.AreEquivalent(LoadoutPlaces.Names, focusable, "only the places take focus");
            foreach (string part in new[] { "weapon-picture", "pane", "pane-scroll", "pane-lines", "spell-list", "list-columns", "aspect-cards", "steel-folded", "element-folded", "footer-left", "footer-right", "infused-line", "role-cells", "temper-pips" })
                Assert.IsTrue(named.ContainsKey(part), part + " is not in the tree");
        }

        [Test]
        public void EverySlotCardHasItsPartsAndItsButtonGlyph()
        {
            var named = new Dictionary<string, XmlElement>();
            foreach (var element in Elements(Tree()))
                if (element.GetAttribute("name").Length > 0) named[element.GetAttribute("name")] = element;
            foreach (string slot in new[] { "slot-west", "slot-north", "slot-east", "slot-south" })
            {
                var parts = new HashSet<string>();
                foreach (var inner in Elements(named[slot]))
                    foreach (string name in inner.GetAttribute("class").Split(' '))
                        parts.Add(name);
                foreach (string part in new[] { "slot-icon", "slot-name", "slot-role", "slot-role-icon", "slot-shape", "slot-glyph" })
                    Assert.IsTrue(parts.Contains(part), slot + " has no " + part);
            }
        }

        // ---- The tokens.

        private static Dictionary<string, string> Tokens(string block)
        {
            string text = File.ReadAllText(TokensPath);
            int start = text.IndexOf(block + " {", StringComparison.Ordinal);
            Assert.GreaterOrEqual(start, 0, "no " + block + " block");
            int end = text.IndexOf('}', start);
            var found = new Dictionary<string, string>();
            foreach (Match match in Regex.Matches(text.Substring(start, end - start), @"(--[a-z0-9-]+):\s*([^;]+);"))
                found[match.Groups[1].Value] = match.Groups[2].Value.Trim();
            return found;
        }

        private static double[] Colour(string value)
        {
            if (value.StartsWith("#", StringComparison.Ordinal))
                return new double[] { Convert.ToInt32(value.Substring(1, 2), 16), Convert.ToInt32(value.Substring(3, 2), 16), Convert.ToInt32(value.Substring(5, 2), 16), 1 };
            var parts = value.Substring(value.IndexOf('(') + 1).TrimEnd(')').Split(',');
            return new[] { double.Parse(parts[0], CultureInfo.InvariantCulture), double.Parse(parts[1], CultureInfo.InvariantCulture), double.Parse(parts[2], CultureInfo.InvariantCulture), double.Parse(parts[3], CultureInfo.InvariantCulture) };
        }

        private static double Luminance(double[] rgb)
        {
            double Linear(double c)
            {
                c /= 255.0;
                return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * Linear(rgb[0]) + 0.7152 * Linear(rgb[1]) + 0.0722 * Linear(rgb[2]);
        }

        private static double Contrast(double[] a, double[] b)
        {
            double la = Luminance(a), lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        // The panel is 96% over the dimmed world; the darkest-and-brightest worry is a lit world, taken as the rest fire's glow.
        private static double[] Over(double[] top, double[] under) =>
            new[] { top[0] * top[3] + under[0] * (1 - top[3]), top[1] * top[3] + under[1] * (1 - top[3]), top[2] * top[3] + under[2] * (1 - top[3]), 1 };

        [Test]
        public void EveryTextOnEveryBackgroundIsFourAndAHalfToOneOrMore()
        {
            var tokens = Tokens(":root");
            var fireGlow = new double[] { 120, 60, 30, 1 };
            var panel = Over(Colour(tokens["--color-panel"]), fireGlow);
            var raised = Colour(tokens["--color-raised"]);
            var focus = Colour(tokens["--color-focus-fill"]);
            var texts = new List<string> { "--color-text", "--color-text-2", "--color-gold", "--color-loss", "--color-memories", "--color-shards" };
            foreach (string key in tokens.Keys)
                if (key.StartsWith("--el-", StringComparison.Ordinal) && key.EndsWith("-text", StringComparison.Ordinal)) texts.Add(key);
            foreach (string text in texts)
            {
                var colour = Colour(tokens[text]);
                Assert.GreaterOrEqual(Contrast(colour, panel), 4.5, text + " on the panel");
                Assert.GreaterOrEqual(Contrast(colour, raised), 4.5, text + " on a card");
                Assert.GreaterOrEqual(Contrast(colour, focus), 4.5, text + " on the focus fill");
            }
            // Locked words sit on cards and the panel; focused, they take the lighter grey.
            Assert.GreaterOrEqual(Contrast(Colour(tokens["--color-text-off"]), raised), 4.5, "text off on a card");
            Assert.GreaterOrEqual(Contrast(Colour(tokens["--color-text-off"]), panel), 4.5, "text off on the panel");
            Assert.GreaterOrEqual(Contrast(Colour(tokens["--color-text-off-focus"]), focus), 4.5, "text off on the focus fill");
            Assert.GreaterOrEqual(Contrast(Colour(tokens["--color-text"]), Colour(tokens["--color-tag"])), 4.5, "a Set tag");
        }

        [Test]
        public void NoPureWhiteIsUsedInAMenu()
        {
            foreach (string path in new[] { TokensPath, "Assets/_Project/UI/Theme/Base.uss", "Assets/_Project/UI/Loadout/Loadout.uss" })
            {
                string text = File.ReadAllText(path).ToUpperInvariant();
                StringAssert.DoesNotContain("#FFFFFF", text, path);
                StringAssert.DoesNotContain("RGB(255, 255, 255)", text, path);
                Assert.IsFalse(Regex.IsMatch(text, @":\s*WHITE\s*;"), path + " names white");
            }
        }

        [Test]
        public void EveryTypeSizeIsTwentyFourOrMoreAndGrowsAtTheLargerSizes()
        {
            var normal = Tokens(":root");
            var larger = Tokens(".size-125");
            var largest = Tokens(".size-150");
            int checkedSizes = 0;
            foreach (var pair in normal)
            {
                if (!pair.Key.StartsWith("--size-", StringComparison.Ordinal)) continue;
                double at100 = double.Parse(pair.Value.Replace("px", ""), CultureInfo.InvariantCulture);
                Assert.IsTrue(larger.ContainsKey(pair.Key) && largest.ContainsKey(pair.Key), pair.Key + " does not grow");
                double at125 = double.Parse(larger[pair.Key].Replace("px", ""), CultureInfo.InvariantCulture);
                double at150 = double.Parse(largest[pair.Key].Replace("px", ""), CultureInfo.InvariantCulture);
                Assert.AreEqual(at100 * 1.25, at125, 1.0, pair.Key + " at 125%");
                Assert.AreEqual(at100 * 1.5, at150, 1.0, pair.Key + " at 150%");
                // The glyph's letter is a stand-in for Kenney's picture (5.2), not words to read.
                if (pair.Key == "--size-glyph-text") continue;
                Assert.GreaterOrEqual(at100, 24.0, pair.Key + " is under 24 at 100%");
                checkedSizes++;
            }
            Assert.GreaterOrEqual(checkedSizes, 9);
        }
    }
}
