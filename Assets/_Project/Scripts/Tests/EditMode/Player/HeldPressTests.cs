using System.IO;
using System.Linq;
using Clover.Player;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// A spell held on its button and loosed on letting go (standin_clearance_plan.md pass 6, the ledger's held-press), and the
    /// Hold interaction on the slot buttons that tells a hold from a tap.
    /// </summary>
    public class HeldPressTests
    {
        private const string ControlsPath = "Assets/_Project/Settings/Controls.inputactions";

        // The interactions written on an action of the asset, read from its text (the editor loads the same text).
        private static string InteractionsOf(string text, string action)
        {
            int at = text.IndexOf("\"name\": \"" + action + "\",", System.StringComparison.Ordinal);
            Assert.That(at, Is.GreaterThanOrEqualTo(0), action + " is in the asset");
            const string key = "\"interactions\": \"";
            int field = text.IndexOf(key, at, System.StringComparison.Ordinal);
            int next = text.IndexOf("\"name\": ", at + 10, System.StringComparison.Ordinal);
            Assert.That(field, Is.GreaterThan(at));
            Assert.That(next < 0 || field < next, action + "'s own interactions");
            int start = field + key.Length;
            return text.Substring(start, text.IndexOf('"', start) - start);
        }

        [Test]
        public void TheSlotButtonsTellAHoldFromATap()
        {
            Assume.That(File.Exists(ControlsPath), "run from the project folder");
            string text = File.ReadAllText(ControlsPath);
            foreach (var action in LayeredButtons.SlotActions)
                Assert.AreEqual("Hold(duration=0.2)", InteractionsOf(text, action), action + " is a hold after a fifth of a second");
            Assert.AreEqual("", InteractionsOf(text, "SpellLayer"), "the layer itself is only held, never asked for a hold");
            Assert.AreEqual(text.Count(c => c == '{'), text.Count(c => c == '}'), "the asset's braces still pair");
        }

        [Test]
        public void AHoldGathersAndALetGoLoosesWithWhatItGathered()
        {
            Assert.AreEqual(0f, SpellSpec.HeldShare(0f, 1f), 1e-5f);
            Assert.AreEqual(0.5f, SpellSpec.HeldShare(0.5f, 1f), 1e-5f);
            Assert.AreEqual(1f, SpellSpec.HeldShare(3f, 1f), 1e-5f, "never more than a full hold");
            Assert.AreEqual(0f, SpellSpec.HeldShare(1f, 0f), 1e-5f, "a spell not held gathers nothing");
            var spec = new SpellSpec { Damage = 8f, Stagger = 22f, HeldStagger = 60f };
            SpellSpec.ApplyHold(spec, 0.5f);
            Assert.AreEqual(41f, spec.Stagger, 1e-4f, "half a hold: half way from 22 to 60");
            Assert.AreEqual(8f, spec.Damage, 1e-5f, "no held damage set: its own");
            var copy = new SpellSpec();
            SpellChange.Copy(new SpellSpec { HoldUpTo = 1.5f, HeldDamage = 3f, HeldStagger = 4f }, copy);
            Assert.AreEqual(1.5f, copy.HoldUpTo, 1e-5f);
            Assert.AreEqual(3f, copy.HeldDamage, 1e-5f);
            Assert.AreEqual(4f, copy.HeldStagger, 1e-5f);
        }

        [Test]
        public void GatheredAndDiamondThunderAreHeldOnTheirButtons()
        {
            var gathered = Upgrade.ById(SpellBranch.ShapeId(LightTree.Glare)).Shapes.First(each => each.Name == "Gathered");
            Assert.IsTrue(gathered.CastsItsOwn, "cast through its mark, so it can be held");
            Assert.AreEqual(1f, gathered.Spell.HoldUpTo, 1e-5f, "held up to a second");
            Assert.AreEqual(22f, gathered.Spell.Stagger, 1e-5f);
            Assert.AreEqual(60f, gathered.Spell.HeldStagger, 1e-5f);
            Assert.IsFalse(gathered.Effects != null && gathered.Effects.Any(each => each.Do == UpgradeAction.Burst), "no stand-in burst at a full hold");
            Assert.AreEqual(1.5f, Upgrade.ById(TheBoltRunsTrueDuo.DiamondThunder).Spell.HoldUpTo, 1e-5f, "the line held up to 1.5 s");
        }
    }
}
