using Clover.Player.Magic;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The L2 layer as rules (IMPLEMENTATION.md 3.5): slots, swallowed face buttons, the surge on a tap, strikes.</summary>
    public class SpellLayerRulesTests
    {
        private const float Frame = 1f / 60f;

        private static LayerFrame Pad(bool layer, int slots = 0, bool light = false, bool heavy = false) =>
            new LayerFrame { DeltaTime = Frame, PadLayerHeld = layer, PadSlots = slots, Light = light, Heavy = heavy };

        [Test]
        public void APadFaceButtonIsASpellOnlyWithTheLayerHeld()
        {
            var rules = new SpellLayerRules();
            rules.Step(Pad(false, 1));
            Assert.AreEqual(-1, rules.SlotCast, "Square alone is the flask, not a spell");
            Assert.IsFalse(rules.Swallows(true));
            rules.Step(Pad(true));
            rules.Step(Pad(true, 1 << 2));
            Assert.AreEqual(2, rules.SlotCast, "L2 and Circle casts East");
            Assert.IsTrue(rules.Swallows(true), "the dodge on Circle is swallowed while L2 is held");
            rules.Step(Pad(true));
            Assert.AreEqual(-1, rules.SlotCast, "one press, one cast");
        }

        [Test]
        public void TheNumberKeysCastWithNoLayer()
        {
            var rules = new SpellLayerRules();
            for (int slot = 0; slot < 4; slot++)
            {
                rules.Step(new LayerFrame { DeltaTime = Frame, KeySlots = 1 << slot });
                Assert.AreEqual(slot, rules.SlotCast);
            }
            Assert.IsFalse(rules.Swallows(false), "a key press is never swallowed");
        }

        [Test]
        public void TheLayerSwallowsOnlyThePadsPresses()
        {
            var rules = new SpellLayerRules();
            rules.Step(Pad(true));
            Assert.IsTrue(rules.Swallows(true));
            Assert.IsFalse(rules.Swallows(false), "R on the keyboard still drinks while L2 is down");
            rules.Step(new LayerFrame { DeltaTime = Frame, KeysLayerHeld = true });
            Assert.IsFalse(rules.Swallows(false), "Left Ctrl takes only the mouse buttons, not the keys");
        }

        [Test]
        public void AShortTapOfTheLayerLightsTheSurgeOnRelease()
        {
            var rules = new SpellLayerRules();
            rules.Step(Pad(true));
            for (int i = 0; i < 5; i++)
            {
                rules.Step(Pad(true));
                Assert.IsFalse(rules.SurgeLit, "not while it is held");
            }
            rules.Step(Pad(false));
            Assert.IsTrue(rules.SurgeLit, "lit on the release of a tap");
            rules.Step(Pad(false));
            Assert.IsFalse(rules.SurgeLit, "once");
        }

        [Test]
        public void ALongHoldOrASpellOnTheLayerLightsNoSurge()
        {
            var rules = new SpellLayerRules();
            rules.Step(Pad(true));
            for (int i = 0; i < 20; i++) rules.Step(Pad(true));
            rules.Step(Pad(false));
            Assert.IsFalse(rules.SurgeLit, "a third of a second is not a tap");

            rules.Step(Pad(true));
            rules.Step(Pad(true, 1));
            rules.Step(Pad(false));
            Assert.IsFalse(rules.SurgeLit, "a quick spell is not a surge as well");
        }

        [Test]
        public void TheTapLengthIsTunable()
        {
            var rules = new SpellLayerRules { SurgeTapSeconds = 0.5f };
            rules.Step(Pad(true));
            for (int i = 0; i < 20; i++) rules.Step(Pad(true));
            rules.Step(Pad(false));
            Assert.IsTrue(rules.SurgeLit);
        }

        [Test]
        public void StrikesBelongToTheLayerOnlyOnceTheyAreBuilt()
        {
            var rules = new SpellLayerRules();
            rules.Step(Pad(true, 0, true, false));
            Assert.IsFalse(rules.StrikeFirst || rules.SwallowLight, "until the strikes exist, R1 under L2 is the light attack");

            rules = new SpellLayerRules { StrikesOn = true };
            rules.Step(Pad(true));
            rules.Step(Pad(true, 0, true, false));
            Assert.IsTrue(rules.StrikeFirst && rules.SwallowLight);
            rules.Step(Pad(true, 0, false, true));
            Assert.IsTrue(rules.StrikeSecond && rules.SwallowHeavy);
            rules.Step(Pad(false));
            Assert.IsFalse(rules.SurgeLit, "a strike spoils the tap");
            rules.Step(Pad(false, 0, true, false));
            Assert.IsFalse(rules.StrikeFirst, "without the layer R1 is the light attack");
            rules.Step(new LayerFrame { DeltaTime = Frame, KeysLayerHeld = true, Light = true });
            Assert.IsTrue(rules.StrikeFirst, "Left Ctrl and the left mouse button");
        }

        [Test]
        public void TwoSlotsInOneFrameCastTheFirst()
        {
            var rules = new SpellLayerRules();
            rules.Step(new LayerFrame { DeltaTime = Frame, KeySlots = (1 << 1) | (1 << 3) });
            Assert.AreEqual(1, rules.SlotCast);
            rules.Reset();
            Assert.AreEqual(-1, rules.SlotCast);
            Assert.IsFalse(rules.PadLayerHeld);
        }
    }
}
