using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The combat playtest's rules (docs/GAME.md, "The evolving combat playtest"): unlimited Shards at a rest, the free
    /// chart with the gods' gifts sold, nothing lost on a fall, the way home leading to the Proving Ground, the pause
    /// menu's entries, the slots for what is not built yet and the marks' details; and the game's own rules, unchanged,
    /// with the mode off.
    /// </summary>
    public class PlaytestModeTests
    {
        private bool wasOn;

        [SetUp]
        public void Remember() => wasOn = PlaytestMode.On;

        [TearDown]
        public void PutBack() => PlaytestMode.Set(wasOn);

        private static Upgrade Mark(UpgradeKind kind, string element = Elements.Fire) =>
            Upgrade.All.First(mark => mark.Kind == kind && !mark.IsMix && mark.Element == element);

        [Test]
        public void ARestGivesUnlimitedShardsOnlyInThePlaytest()
        {
            PlaytestMode.Set(true);
            Assert.GreaterOrEqual(PlaytestMode.ShardsAfterRest(3), PlaytestMode.RestShards);
            int whole = Upgrade.All.Sum(mark => mark.Cost);
            Assert.Greater(PlaytestMode.RestShards, whole, "a rest pays for the whole chart at once");
            PlaytestMode.Set(false);
            Assert.AreEqual(3, PlaytestMode.ShardsAfterRest(3), "outside the playtest a rest gives nothing");
            Assert.AreEqual(0, PlaytestMode.ShardsAfterRest(0));
        }

        [Test]
        public void TheChartIsFreeInThePlaytest()
        {
            var learned = new HashSet<string>();
            var ice = Mark(UpgradeKind.Root, Elements.Ice);
            var plasma = Upgrade.All.First(mark => mark.IsMix && !mark.Signature && mark.BelongsTo(Elements.Fire) && mark.BelongsTo(Elements.Lightning));
            var gift = Gifts.SignatureOf(Elements.Water);
            var none = new HashSet<string>();
            // Holding Fire, another tree, a sealed branch and a god's gift are all open and for sale.
            Assert.IsTrue(MarkRules.Open(ice, Elements.Fire, none, none, true));
            Assert.IsTrue(MarkRules.Open(plasma, Elements.Fire, none, none, true));
            Assert.IsTrue(MarkRules.CanBuy(ice, learned, PlaytestMode.RestShards, true, true));
            Assert.IsTrue(MarkRules.CanBuy(gift, learned, PlaytestMode.RestShards, true, true), "a god's gift is a playtest unlock");
            // The tree's roads still hold: a mark is bought from what it grows from.
            var grown = Upgrade.All.First(mark => mark.Element == Elements.Ice && !mark.IsMix && mark.Requires.Length > 0 && !mark.Signature);
            Assert.IsFalse(MarkRules.CanBuy(grown, learned, PlaytestMode.RestShards, true, true));
            learned.Add(grown.Requires[0]);
            Assert.IsTrue(MarkRules.CanBuy(grown, learned, PlaytestMode.RestShards, true, true));
            // A slot for what is not built yet is never sold.
            PlaytestMode.Set(true);
            var slot = NotBuiltYet.ChartSlots[0];
            Assert.IsTrue(NotBuiltYet.IsSlot(slot));
            Assert.IsFalse(MarkRules.CanBuy(slot, learned, PlaytestMode.RestShards, true, true));
        }

        [Test]
        public void OutsideThePlaytestTheChartIsAsItWas()
        {
            var none = new HashSet<string>();
            var ice = Mark(UpgradeKind.Root, Elements.Ice);
            var fire = Mark(UpgradeKind.Root, Elements.Fire);
            var plasma = Upgrade.All.First(mark => mark.IsMix && !mark.Signature && mark.BelongsTo(Elements.Fire) && mark.BelongsTo(Elements.Lightning));
            Assert.IsFalse(MarkRules.Open(ice, Elements.Fire, none, none, false), "another element's tree is shut");
            Assert.IsTrue(MarkRules.Open(fire, Elements.Fire, none, none, false));
            Assert.IsFalse(MarkRules.Open(plasma, Elements.Fire, none, none, false), "a mix is sealed until its other element is held or consumed");
            Assert.IsTrue(MarkRules.Open(plasma, Elements.Fire, new HashSet<string> { Elements.Lightning }, none, false));
            Assert.IsFalse(MarkRules.CanBuy(Gifts.SignatureOf(Elements.Fire), none, 999, true, false), "a god's gift is never sold");
            Assert.IsTrue(MarkRules.CanBuy(fire, none, fire.Cost, true, false));
            Assert.IsFalse(MarkRules.CanBuy(fire, none, fire.Cost - 1, true, false), "it must be paid for");
            Assert.IsFalse(MarkRules.CanBuy(fire, new HashSet<string> { fire.Id }, 99, true, false), "not twice");
        }

        [Test]
        public void AFallLosesNothingInThePlaytestAndWhatItDidOutside()
        {
            Assert.AreEqual(12, ReturnRule.EmbersAfterFall(12, true, true));
            Assert.AreEqual(0, ReturnRule.EmbersAfterFall(12, true, false), "a land's fall loses the Shards carried");
            Assert.AreEqual(12, ReturnRule.EmbersAfterFall(12, false, false));
            Assert.AreEqual(ReturnRule.EmbersAfterFall(7, true), ReturnRule.EmbersAfterFall(7, true, false));
        }

        [Test]
        public void TheWayHomeLeadsToTheProvingGroundInThePlaytest()
        {
            PlaytestMode.Set(true);
            Assert.AreEqual(PlaytestMode.ProvingGroundScene, SceneRoutes.Resolve(SceneRoutes.HubScene));
            Assert.AreEqual(PlaytestMode.ProvingGroundScene, SceneRoutes.Home);
            Assert.AreEqual("Castle", SceneRoutes.Resolve("Castle"));
            PlaytestMode.Set(false);
            Assert.AreEqual(SceneRoutes.HubScene, SceneRoutes.Resolve(SceneRoutes.HubScene), "outside the playtest the hub is the hub");
            Assert.AreEqual(SceneRoutes.HubScene, SceneRoutes.Home);
        }

        [Test]
        public void ThePauseMenuOffersTheGuideAndTheWayBackOnlyInThePlaytest()
        {
            foreach (string scene in new[] { "Hub", "Castle", "Summit", "CombatTest", "SunCity" })
            {
                var plain = new PauseChoices(scene, false);
                for (int i = 0; i < plain.Count; i++)
                    Assert.IsFalse(plain.At(i) == PauseEntry.Guide || plain.At(i) == PauseEntry.WhatsNew || plain.At(i) == PauseEntry.ProvingGround, scene);
            }
            var castle = new PauseChoices("Castle", true);
            var entries = Enumerable.Range(0, castle.Count).Select(castle.At).ToList();
            Assert.AreEqual(PauseEntry.Resume, entries[0]);
            Assert.AreEqual(PauseEntry.Controls, entries[1]);
            Assert.AreEqual(PauseEntry.Quit, entries[entries.Count - 1]);
            CollectionAssert.Contains(entries, PauseEntry.Guide);
            CollectionAssert.Contains(entries, PauseEntry.WhatsNew);
            CollectionAssert.Contains(entries, PauseEntry.ProvingGround);
            CollectionAssert.DoesNotContain(entries, PauseEntry.GiveUp, "in the playtest nothing is given up");
            var ground = new PauseChoices(PlaytestMode.ProvingGroundScene, true);
            for (int i = 0; i < ground.Count; i++) Assert.AreNotEqual(PauseEntry.ProvingGround, ground.At(i), "no way back to where one is");
            foreach (var entry in new[] { PauseEntry.Guide, PauseEntry.WhatsNew, PauseEntry.ProvingGround }) Assert.IsNotEmpty(PauseChoices.Label(entry));
        }

        [Test]
        public void EveryGodIsListedAndEachTreeHasASlotBesideItsCrown()
        {
            PlaytestMode.Set(true);
            foreach (var element in Elements.All)
                Assert.IsNotEmpty(NotBuiltYet.GodOf(element.Name).God, element.Name);
            var built = NotBuiltYet.Gods.Where(god => god.WeaponBuilt).Select(god => god.BuiltWeapon).ToList();
            CollectionAssert.AreEquivalent(new[] { Weapons.StormKatanaName, Weapons.SunSpearName }, built, "the two gods' weapons that are built");
            foreach (string weapon in built) Assert.AreEqual(weapon, Weapons.ByName(weapon).Name);
            Assert.AreEqual(Elements.Count, NotBuiltYet.ChartSlots.Count);
            foreach (var slot in NotBuiltYet.ChartSlots)
            {
                Assert.IsNull(Upgrade.ById(slot.Id), "a slot is not a mark of the game");
                StringAssert.StartsWith(NotBuiltYet.Label, slot.Name);
            }
        }

        [Test]
        public void EveryHighLevelMarkHasDetails()
        {
            foreach (var mark in Upgrade.All)
            {
                if (!MarkDetails.HasDetails(mark)) continue;
                var details = MarkDetails.For(mark, null);
                Assert.IsNotNull(details, mark.Id);
                StringAssert.Contains("The numbers.", details.Body, mark.Id);
                StringAssert.Contains("When it happens.", details.Body, mark.Id);
            }
            Assert.IsFalse(MarkDetails.HasDetails(Mark(UpgradeKind.Minor)), "a minor is only a number");
        }

        [Test]
        public void TheWrittenTextsNameOnlyMarksThatExist()
        {
            var texts = UnityEditor.AssetDatabase.LoadAssetAtPath<PlaytestTexts>(Clover.EditorTools.PlaytestTextsBuilder.AssetPath);
            if (texts == null) Assert.Ignore("The playtest's texts have not been built (Clover > Build > Playtest Texts).");
            Assert.IsNotEmpty(texts.Version);
            Assert.IsNotEmpty(texts.WhatsNew);
            Assert.GreaterOrEqual(texts.Effects.Count, 20);
            foreach (string id in new[] { "burn", "static", "chill", "poisoned", "staggered", "grey-health", "perfect-guard", "perfect-dodge", "health", "stamina", "mana", "flasks", "shards" })
                Assert.IsTrue(texts.Effects.Any(entry => entry.Id == id), "the guide has " + id);
            foreach (var entry in texts.Marks) Assert.IsNotNull(Upgrade.ById(entry.Id), "a mark's details name " + entry.Id);
            // Fire and Lightning, and their three mixes, are written in full.
            foreach (var mark in Upgrade.All)
            {
                bool ours = mark.BelongsTo(Elements.Fire) || mark.BelongsTo(Elements.Lightning);
                if (!ours || !MarkDetails.HasDetails(mark)) continue;
                Assert.IsNotNull(texts.MarkDetails(mark.Id), "written details for " + mark.Id);
            }
        }
    }
}
