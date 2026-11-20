using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.Player.Build;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Hand's rules (STEEL_AND_ELEMENT.md 5, IMPLEMENTATION.md 3.6): only spells of the carried elements go in a slot,
    /// duo spells only with both elements, grouped by element with their roles, at most thirteen in a full case; what no
    /// longer fits is dropped when the attunements change; the surge is a carried element's; the slots open on the
    /// schedule and all at once in the playtest; R1 and R2 strike with the first and second element, R2 is the deep
    /// strike all in or with one element; the Hand changes only at a rest fire. Over a catalogue made here, so the
    /// trees being rewritten (phase 4) cannot break it.
    /// </summary>
    public class HandRulesTests
    {
        private static Upgrade Spell(string id, string element, string element2 = "") => new Upgrade
        {
            Id = id,
            Name = id,
            Element = element,
            Element2 = element2,
            Kind = UpgradeKind.Spell,
            Form = "wave",
            Spell = new SpellSpec(),
            Description = "First line. Second line.",
        };

        private static Upgrade Passive(string id, string element) => new Upgrade { Id = id, Name = id, Element = element, Kind = UpgradeKind.Major };

        // Five learned spells and the base spell for each of Fire and Lightning, and the pair's duo spell: the design's
        // full case of six and six and one.
        private static List<Upgrade> Catalogue() => new List<Upgrade>
        {
            Spell("fire.a", Elements.Fire), Spell("fire.b", Elements.Fire), Spell("fire.c", Elements.Fire), Spell("fire.d", Elements.Fire), Spell("fire.e", Elements.Fire),
            Spell("lightning.a", Elements.Lightning), Spell("lightning.b", Elements.Lightning), Spell("lightning.c", Elements.Lightning), Spell("lightning.d", Elements.Lightning), Spell("lightning.e", Elements.Lightning),
            Spell("ice.a", Elements.Ice),
            Spell("plasma.ball", Elements.Fire, Elements.Lightning),
            Spell("tempest.eye", Elements.Lightning, Elements.Wind),
            Passive("fire.root", Elements.Fire),
        };

        private static HashSet<string> AllLearned(IEnumerable<Upgrade> catalogue) => new HashSet<string>(catalogue.Select(mark => mark.Id));

        private static Loadout Hand(string first, string second)
        {
            var loadout = new Loadout();
            loadout.Attune(first, second);
            return loadout;
        }

        [Test]
        public void ASlotOffersOnlySpellsOfTheCarriedElementsAndTheirDuoSpell()
        {
            var catalogue = Catalogue();
            var rules = new HandRules(catalogue);
            var choices = rules.ChoicesFor(0, Hand(Elements.Fire, Elements.Lightning), AllLearned(catalogue));
            var ids = choices.Select(choice => choice.Id).ToList();
            CollectionAssert.Contains(ids, "fire.base");
            CollectionAssert.Contains(ids, "lightning.base");
            CollectionAssert.Contains(ids, "fire.c");
            CollectionAssert.Contains(ids, "lightning.e");
            CollectionAssert.Contains(ids, "plasma.ball", "the pair's duo spell, both elements carried");
            CollectionAssert.DoesNotContain(ids, "ice.a", "an element not carried");
            CollectionAssert.DoesNotContain(ids, "tempest.eye", "a duo spell with only one of its elements");
            CollectionAssert.DoesNotContain(ids, "fire.root", "a mark that is not a spell");
        }

        [Test]
        public void ChoicesAreGroupedByElementFirstThenSecondThenDuoWithRolesAndALine()
        {
            var catalogue = Catalogue();
            var choices = new HandRules(catalogue).ChoicesFor(2, Hand(Elements.Fire, Elements.Lightning), AllLearned(catalogue));
            int lastFire = choices.FindLastIndex(choice => choice.Element == Elements.Fire && !choice.IsDuo);
            int firstLightning = choices.FindIndex(choice => choice.Element == Elements.Lightning && !choice.IsDuo);
            int firstDuo = choices.FindIndex(choice => choice.IsDuo);
            Assert.AreEqual("fire.base", choices[0].Id, "each element's base spell leads its group");
            Assert.Less(lastFire, firstLightning);
            Assert.Less(choices.FindLastIndex(choice => choice.Element == Elements.Lightning && !choice.IsDuo), firstDuo);
            var ball = choices.First(choice => choice.Id == "plasma.ball");
            Assert.AreEqual(SpellRole.Detonator, ball.Role, "the role comes from the spell table");
            Assert.AreEqual(SpellRole.Opener, choices[0].Role, "Flame Wave opens");
            Assert.AreEqual("First line.", ball.Line, "one plain line");
            Assert.AreEqual("Flame Wave", choices[0].Name);
        }

        [Test]
        public void AFullCaseIsAtMostThirteen()
        {
            var catalogue = Catalogue();
            var choices = new HandRules(catalogue).ChoicesFor(0, Hand(Elements.Fire, Elements.Lightning), AllLearned(catalogue));
            Assert.AreEqual(13, choices.Count);
            Assert.LessOrEqual(choices.Count, HandRules.MostChoices);
        }

        [Test]
        public void OnlyLearnedSpellsAreOffered()
        {
            var catalogue = Catalogue();
            var learned = new HashSet<string> { "fire.a" };
            var ids = new HandRules(catalogue).ChoicesFor(0, Hand(Elements.Fire, Elements.Lightning), learned).Select(choice => choice.Id).ToList();
            CollectionAssert.AreEquivalent(new[] { "fire.base", "fire.a", "lightning.base" }, ids, "the base spells need no learning");
        }

        [Test]
        public void AllInOffersTheOneElementOnceAndNoDuo()
        {
            var catalogue = Catalogue();
            var rules = new HandRules(catalogue);
            var allIn = Hand(Elements.Fire, Elements.Fire);
            Assert.IsTrue(allIn.AllIn);
            var ids = rules.ChoicesFor(0, allIn, AllLearned(catalogue)).Select(choice => choice.Id).ToList();
            Assert.AreEqual(1, ids.Count(id => id == "fire.base"));
            Assert.AreEqual(6, ids.Count);
            CollectionAssert.DoesNotContain(ids, "plasma.ball");
            var one = rules.ChoicesFor(0, Hand(Elements.Fire, ""), AllLearned(catalogue));
            Assert.AreEqual(6, one.Count, "one element carried: its six");
        }

        [Test]
        public void TheSlotAChoiceSitsInIsShown()
        {
            var catalogue = Catalogue();
            var loadout = Hand(Elements.Fire, Elements.Lightning);
            loadout.Slots[2] = "lightning.b";
            var choice = new HandRules(catalogue).ChoicesFor(0, loadout, AllLearned(catalogue)).First(c => c.Id == "lightning.b");
            Assert.AreEqual(2, choice.InSlot);
        }

        [Test]
        public void ValidateDropsWhatNoLongerFitsAndKeepsWhatDoes()
        {
            var catalogue = Catalogue();
            var rules = new HandRules(catalogue);
            var learned = AllLearned(catalogue);
            var loadout = Hand(Elements.Fire, Elements.Lightning);
            loadout.Slots[0] = "fire.base";
            loadout.Slots[1] = "lightning.a";
            loadout.Slots[2] = "plasma.ball";
            loadout.Slots[3] = "fire.b";
            loadout.Surge = HandRules.SurgeId(Elements.Lightning);
            Assert.IsFalse(rules.Validate(loadout, learned), "a Hand that fits is left alone");

            // The hub changes the second element: what was Lightning's leaves, and the duo spell with it.
            loadout.Attune(Elements.Fire, Elements.Ice);
            Assert.IsTrue(rules.Validate(loadout, learned));
            Assert.AreEqual("fire.base", loadout.Slots[0]);
            Assert.AreEqual("", loadout.Slots[1]);
            Assert.AreEqual("", loadout.Slots[2], "the duo spell whose pair broke");
            Assert.AreEqual("fire.b", loadout.Slots[3]);
            Assert.AreEqual("fire.surge", loadout.Surge, "the surge falls back to the first element's");
        }

        [Test]
        public void ValidateDropsAnUnlearnedSpellADoubleAndAnUnknownIdUnlessKept()
        {
            var catalogue = Catalogue();
            var rules = new HandRules(catalogue);
            var loadout = Hand(Elements.Fire, "");
            loadout.Slots[0] = "fire.a";
            loadout.Slots[1] = "fire.a";
            loadout.Slots[2] = "lent.spell";
            loadout.Slots[3] = "lightning.base";
            Assert.IsTrue(rules.Validate(loadout, new HashSet<string>(), id => id == "lent.spell"));
            CollectionAssert.AreEqual(new[] { "", "", "lent.spell", "" }, loadout.Slots);
            var learned = new HashSet<string> { "fire.a" };
            loadout.Slots[0] = "fire.a";
            loadout.Slots[1] = "fire.a";
            rules.Validate(loadout, learned);
            Assert.AreEqual("fire.a", loadout.Slots[0]);
            Assert.AreEqual("", loadout.Slots[1], "no spell in two slots");
        }

        [Test]
        public void TheSurgeIsACarriedElementsAndAFreshHandGetsTheFirstOne()
        {
            var loadout = Hand(Elements.Fire, Elements.Lightning);
            Assert.IsTrue(HandRules.CanWearSurge("lightning.surge", loadout));
            Assert.IsFalse(HandRules.CanWearSurge("ice.surge", loadout));
            Assert.IsFalse(HandRules.CanWearSurge("", loadout));
            var surges = new HandRules(Catalogue()).SurgesFor(loadout).Select(choice => choice.Name).ToList();
            CollectionAssert.AreEqual(new[] { "Kindle", "Overdrive" }, surges);
            new HandRules(Catalogue()).Validate(loadout, new HashSet<string>());
            Assert.AreEqual("fire.surge", loadout.Surge);
            Assert.AreEqual(Elements.Lightning, HandRules.ElementOfSurge("lightning.surge"));
            Assert.AreEqual(Elements.Poison, HandRules.ElementOfBase("poison.base"));
            Assert.AreEqual("", HandRules.ElementOfBase("poison.onebyone"));
        }

        [Test]
        public void TheSlotsOpenOnTheSchedule()
        {
            var flags = new HashSet<string>();
            var start = SlotSchedule.Open(flags.Contains, 0, false);
            Assert.AreEqual(2, start.Slots, "two slots at the start");
            Assert.IsTrue(start.SlotOpen(1));
            Assert.IsFalse(start.SlotOpen(2));
            Assert.IsTrue(start.Surge);
            Assert.IsFalse(start.SecondAttunement, "one element at the start");
            flags.Add(SlotSchedule.ChampionFlag);
            Assert.AreEqual(3, SlotSchedule.Open(flags.Contains, 0, false).Slots, "the third at the Champion of the Gate");
            flags.Add(SlotSchedule.FireGodFlag);
            var afterGod = SlotSchedule.Open(flags.Contains, 1, false);
            Assert.AreEqual(4, afterGod.Slots, "the fourth at the Fire god");
            Assert.IsTrue(afterGod.SecondAttunement, "the second element after a god");
            Assert.IsTrue(SlotSchedule.Open(null, 1, false).SecondAttunement, "any god opens the second element");
        }

        [Test]
        public void EverythingIsOpenInThePlaytest()
        {
            var open = SlotSchedule.Open(new HashSet<string>().Contains, 0, true);
            Assert.AreEqual(4, open.Slots);
            Assert.IsTrue(open.Surge);
            Assert.IsTrue(open.SecondAttunement);
        }

        [Test]
        public void R1StrikesWithTheFirstAndR2WithTheSecondOrDeepAllInOrAlone()
        {
            var mix = Hand(Elements.Fire, Elements.Lightning);
            Assert.AreEqual(Elements.Fire, HandRules.StrikeElement(mix, false, out bool deep));
            Assert.IsFalse(deep);
            Assert.AreEqual(Elements.Lightning, HandRules.StrikeElement(mix, true, out deep));
            Assert.IsFalse(deep, "with two elements R2 is the second element's strike, not the deep one");
            Assert.AreEqual(Elements.Fire, HandRules.StrikeElement(Hand(Elements.Fire, Elements.Fire), true, out deep));
            Assert.IsTrue(deep, "all in, R2 is the deep strike");
            Assert.AreEqual(Elements.Ice, HandRules.StrikeElement(Hand(Elements.Ice, ""), true, out deep));
            Assert.IsTrue(deep, "one element: the deep strike, as in Phase 1");
            Assert.AreEqual("", HandRules.StrikeElement(Hand("", ""), false, out _));
        }

        [Test]
        public void TheHandChangesOnlyAtARestFire()
        {
            Assert.IsTrue(HandRules.CanChangeHere(true));
            Assert.IsFalse(HandRules.CanChangeHere(false));
        }

        [Test]
        public void ASavedHandSwapsInOnlyTheHandNeverTheAttunementsOrTheWeapon()
        {
            var saved = Hand(Elements.Ice, Elements.Water);
            saved.Slots[0] = "ice.base";
            saved.Surge = "ice.surge";
            saved.Weapon = "Katana";
            var current = Hand(Elements.Fire, Elements.Lightning);
            current.Weapon = "Fists";
            current.CopyHandFrom(saved);
            Assert.AreEqual("ice.base", current.Slots[0]);
            Assert.AreEqual("ice.surge", current.Surge);
            Assert.AreEqual(Elements.Fire, current.First, "attunements are picked in the hub");
            Assert.AreEqual("Fists", current.Weapon);
            var copy = saved.Clone();
            copy.Slots[0] = "";
            Assert.AreEqual("ice.base", saved.Slots[0], "a clone shares nothing");
        }

        [Test]
        public void TheRealCatalogueOffersFireAndLightningTheirBaseSpells()
        {
            var choices = new HandRules().ChoicesFor(0, Hand(Elements.Fire, Elements.Lightning), new HashSet<string>());
            CollectionAssert.AreEqual(new[] { "fire.base", "lightning.base" }, choices.Select(choice => choice.Id).ToArray());
            Assert.AreEqual("Thunderclap", choices[1].Name);
        }

        [Test]
        public void ADuoSpellOfAPairTwoApartSitsInTheHandOnlyWhileBothAreCarried()
        {
            // Glass Storm, Ice and Lightning's duo spell (task 4.5): not neighbours on the ring, a duo spell all the same.
            string glass = Clover.Player.Trees.Duo.SleetChargeDuo.GlassStorm;
            var rules = new HandRules();
            var learned = new HashSet<string> { glass };
            var both = Hand(Elements.Lightning, Elements.Ice);
            var choice = rules.ChoicesFor(0, both, learned).Single(offered => offered.Id == glass);
            Assert.IsTrue(choice.IsDuo);
            Assert.AreEqual(SpellRole.Opener, choice.Role);
            Assert.AreEqual(14f, choice.Cooldown, 1e-4f, "a duo spell's own cooldown, no mana");
            Assert.AreEqual(0f, choice.Mana, 1e-4f);
            Assert.AreEqual(rules.ChoicesFor(0, both, learned).Count - 1, rules.ChoicesFor(0, both, learned).FindIndex(offered => offered.Id == glass), "after both elements' own spells");
            Assert.IsTrue(rules.CanSlot(glass, Hand(Elements.Ice, Elements.Lightning), learned), "in either order");
            Assert.IsFalse(rules.CanSlot(glass, both, new HashSet<string>()), "only once learned");
            CollectionAssert.DoesNotContain(rules.ChoicesFor(0, Hand(Elements.Lightning, Elements.Fire), learned).Select(offered => offered.Id).ToList(), glass, "not with another partner");
            CollectionAssert.DoesNotContain(rules.ChoicesFor(0, Hand(Elements.Ice, Elements.Ice), learned).Select(offered => offered.Id).ToList(), glass, "not all in");
            // Slotted, it is dropped when the pair breaks at the next fire.
            both.Surge = HandRules.SurgeId(Elements.Lightning);
            both.Slots[1] = glass;
            Assert.IsFalse(rules.Validate(both, learned), "it fits");
            both.Attune(Elements.Lightning, Elements.Fire);
            Assert.IsTrue(rules.Validate(both, learned));
            Assert.AreEqual("", both.Slots[1]);
        }
    }
}
