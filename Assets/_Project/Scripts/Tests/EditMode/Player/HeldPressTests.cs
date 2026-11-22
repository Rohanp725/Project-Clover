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
        public void AShapesSecondPressTakesYouBackWhereYouCastIt()
        {
            var road = Upgrade.ById(SpellBranch.ShapeId(LightTree.StepIntoLight)).Shapes.First(each => each.Name == "Aruna's Road");
            var presses = PlayerUpgrades.PressesOf(Upgrade.ById(LightTree.StepIntoLight), road, out float window);
            Assert.AreEqual(3f, window, 1e-5f, "within three seconds");
            Assert.AreEqual(UpgradeAction.ReturnToCast, presses.Single().Single().Do, "back along the road to where you began");
            var back = Upgrade.ById(SpellBranch.ShapeId(MetalTree.ToTheBlade)).Shapes.First(each => each.Name == "There and Back");
            var flight = PlayerUpgrades.PressesOf(Upgrade.ById(MetalTree.ToTheBlade), back, out window).Single().Single();
            Assert.AreEqual(2f, window, 1e-5f, "within two seconds of arriving");
            Assert.AreEqual(UpgradeAction.ReturnToCast, flight.Do);
            Assert.AreEqual(10f, flight.Damage, 1e-5f, "the line cut a second time");
            Assert.IsEmpty(back.Effects, "no stand-in cut a second later");
            Assert.IsNull(PlayerUpgrades.PressesOf(Upgrade.ById(MetalTree.ToTheBlade), null, out window), "no shape worn: no second press");
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

        [Test]
        public void DaybreakIsGatheredRingByRingAndLoosedWithWhatItHolds()
        {
            var learn = Upgrade.ById(LightTree.Daybreak);
            Assert.AreEqual(8, learn.Spell.Rings, "eight rings at most");
            Assert.AreEqual(6, learn.Spell.RingsFor, "its numbers are written for six");
            var gather = new RingGather();
            Assert.IsFalse(gather.Add(), "nothing gathering");
            gather.Begin(learn, learn.Spell, 7, 18f);
            Assert.IsTrue(gather.Active);
            for (int i = 0; i < 10; i++) gather.Add();
            Assert.AreEqual(8, gather.Rings, "no more than eight");
            Assert.IsFalse(gather.Due(17.9f));
            Assert.IsTrue(gather.Due(18f), "loosed by itself when its window is over");
            var loosed = new SpellSpec();
            Assert.IsTrue(gather.Loose(loosed));
            Assert.IsFalse(gather.Active);
            Assert.AreEqual(16f, loosed.Damage, 1e-4f, "eight rings: 8 a ring over four bites");
            Assert.AreEqual(12f, loosed.Stagger, 1e-4f, "6 stagger a ring over four bites");
            Assert.AreEqual(0, loosed.Rings, "what is loosed gathers no more");
            gather.Begin(learn, learn.Spell, 8, 30f);
            gather.Add();
            gather.Add();
            gather.Loose(loosed);
            Assert.AreEqual(4f, loosed.Damage, 1e-4f, "two rings");
            Assert.AreEqual(0f, RingGather.Scale(0, 6), 1e-5f);
            var presses = PlayerUpgrades.PressesOf(learn, null, out float window);
            Assert.AreEqual(8f, window, 1e-5f);
            Assert.AreEqual(UpgradeAction.Loose, presses[0].Single().Do, "the second press looses it");
            var master = Upgrade.ById(SpellBranch.MasterId(LightTree.Daybreak)).Effects;
            Assert.IsTrue(master.All(each => each.On == UpgradeTrigger.SpellLoosed && each.BlowLanding == 0.2f), "as it is loosed, a blow a fifth of a second from landing");
            Assert.IsTrue(master.Any(each => each.Do == UpgradeAction.Ward), "the blow is turned");
            Assert.IsTrue(master.Any(each => each.Do == UpgradeAction.Boon && each.ThisCast && each.Stat == PlayerStat.CritChance), "the beam is critical");
            var noon = Upgrade.ById(SpellBranch.ShapeId(LightTree.Daybreak)).Shapes.Single(each => each.Name == "High Noon");
            Assert.AreEqual(8, noon.Spell.Rings, "the column is gathered too");
            Assert.AreEqual(UpgradeTrigger.SpellLoosed, noon.Effects.Single().On, "cleansed as it comes down");
        }
    }
}
