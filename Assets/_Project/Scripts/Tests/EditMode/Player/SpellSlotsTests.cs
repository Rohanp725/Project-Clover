using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.Player.Magic;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The four slots before the Hand exists, and how every built spell is paid for (the shared rules of
    /// docs/design/steel/README.md): a cooldown or a mana price, never both.
    /// </summary>
    public class SpellSlotsTests
    {
        [Test]
        public void TheBaseSpellComesFirstThenTheLearnedSpellsInOrder()
        {
            var slots = new SpellSlots();
            var learned = new List<string> { "fire.rush", "fire.kiln", "fire.meteor", "fire.fan" };
            Assert.IsTrue(slots.Settle("fire.base", learned.Contains, learned));
            Assert.AreEqual("fire.base", slots[0]);
            Assert.AreEqual("fire.rush", slots[1]);
            Assert.AreEqual("fire.kiln", slots[2]);
            Assert.AreEqual("fire.meteor", slots[3]);
            Assert.IsFalse(slots.Settle("fire.base", learned.Contains, learned), "settled slots stay put");
        }

        [Test]
        public void ANewElementTakesTheBaseSlotAndDropsWhatIsNoLongerHeld()
        {
            var slots = new SpellSlots();
            var fire = new List<string> { "fire.rush" };
            slots.Settle("fire.base", fire.Contains, fire);
            var lightning = new List<string> { "lightning.lance", "lightning.rod" };
            slots.Settle("lightning.base", lightning.Contains, lightning);
            Assert.AreEqual("lightning.base", slots[0], "the base spell keeps its slot in the new element");
            Assert.AreEqual(-1, slots.SlotOf("fire.rush"));
            Assert.AreEqual("lightning.lance", slots[1]);
            Assert.AreEqual("lightning.rod", slots[2]);

            slots.Settle("", id => false, null);
            Assert.AreEqual(-1, slots.SlotOf("lightning.base"), "with no element there is no base spell");
        }

        [Test]
        public void PutInNextGoesRoundTheFourAndNeverDoublesASpell()
        {
            var slots = new SpellSlots();
            Assert.AreEqual(0, slots.PutInNext("a"));
            Assert.AreEqual(1, slots.PutInNext("b"));
            Assert.AreEqual(2, slots.PutInNext("c"));
            Assert.AreEqual(3, slots.PutInNext("d"));
            Assert.AreEqual(0, slots.PutInNext("b"), "round again");
            Assert.AreEqual("b", slots[0]);
            Assert.AreEqual("", slots[1], "b left its old slot");
            slots.Set(2, "d");
            Assert.AreEqual(-1, System.Array.IndexOf(new[] { slots[0], slots[1], slots[3] }, "d"));
        }

        [Test]
        public void TheSlotsCarryToTheNextScene()
        {
            var slots = new SpellSlots();
            slots.Set(2, "ice.wall");
            var carried = new string[SpellSlots.Count];
            slots.CopyTo(carried);
            var next = new SpellSlots();
            next.CopyFrom(carried);
            Assert.AreEqual("ice.wall", next[2]);
            Assert.AreEqual("", next[0]);
            next.CopyFrom(null);
            Assert.IsTrue(next.IsEmpty(2));
        }

        [Test]
        public void BaseIdsAreTheElementInLowerCase()
        {
            Assert.AreEqual("fire.base", SpellSlots.BaseId(Elements.Fire));
            Assert.IsTrue(SpellSlots.IsBase("poison.base"));
            Assert.IsFalse(SpellSlots.IsBase("poison.onebyone"));
            Assert.AreEqual("", SpellSlots.BaseId(""));
        }

        [Test]
        public void EveryBuiltSpellHasACooldownOrAPriceNotBoth()
        {
            foreach (var mark in Upgrade.All.Where(mark => mark.IsSpell))
            {
                Assert.IsTrue(SpellTimings.Knows(mark.Id), $"{mark.Id} ({mark.Name}) is not in the table");
                Assert.IsNotNull(mark.Spell, mark.Id);
                bool cooldown = mark.Spell.Cooldown > 0f, mana = mark.Spell.Cost > 0f;
                Assert.IsTrue(cooldown ^ mana, $"{mark.Name}: cooldown {mark.Spell.Cooldown}, mana {mark.Spell.Cost}");
            }
            foreach (string id in SpellTimings.Ids) Assert.IsNotNull(Upgrade.ById(id), $"the table names {id}, which no mark has");
        }

        [Test]
        public void BasicSpellsRunSixToTwelveSecondsAndLargerSpellsCostThirtyToSixty()
        {
            foreach (var mark in Upgrade.All.Where(mark => mark.IsSpell))
            {
                var timing = SpellTimings.For(mark.Id);
                if (mark.Id == "dark.pawnedheart") continue; // paid in health, with its own long wait (elements_b.md)
                if (timing.IsLarger) Assert.That(timing.Mana, Is.InRange(30f, 60f), mark.Name);
                else Assert.That(timing.Cooldown, Is.InRange(6f, 12f), mark.Name);
            }
            foreach (string element in Elements.All.Select(info => info.Name))
                Assert.That(SpellTimings.Base(element).Cooldown, Is.InRange(6f, 9f), element + "'s base spell");
        }

        [Test]
        public void EachElementHasAtMostOneLargerSpellAndAllButEarthHaveOne()
        {
            foreach (string element in Elements.All.Select(info => info.Name))
            {
                int larger = Upgrade.All.Count(mark => mark.IsSpell && !mark.IsMix && mark.Element == element && SpellTimings.For(mark.Id).IsLarger);
                // Earth's larger spell is the new Cairn, not built yet; Dark's is Pawned Heart, paid in health.
                int expected = element == Elements.Earth || element == Elements.Dark ? 0 : 1;
                Assert.AreEqual(expected, larger, element);
            }
        }
    }
}
