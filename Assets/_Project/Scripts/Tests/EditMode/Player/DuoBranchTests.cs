using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Build;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The duo branches and the heart (IMPLEMENTATION.md 2.3, 2.4, 4.2; task 4.5): a duo branch of any pair opens while both
    /// its elements are carried; the heart shows the carried pair, or in the playtest the pair picked; a reaction wears the
    /// shape chosen at the rest fire instead of its plain self once the pair's Shape mark is learned and both are carried,
    /// and a swap of the shape changes what it runs.
    /// </summary>
    public class DuoBranchTests
    {
        private static Upgrade Duo(string id, string one, string other, MarkTree tree = MarkTree.Duo) =>
            new Upgrade { Id = id, Name = id, Element = one, Element2 = other, Kind = UpgradeKind.Mix, Tree = tree, Description = "A test mark." };

        private static Loadout Hand(string first, string second)
        {
            var loadout = new Loadout();
            loadout.Attune(first, second);
            return loadout;
        }

        [TearDown]
        public void GatherTheGameAgain() => Upgrade.Gather(null);

        [Test]
        public void AMarkOfAnyPairOpensWhileBothItsElementsAreCarried()
        {
            var glass = Duo("t.glass", Elements.Ice, Elements.Lightning);
            Assert.IsFalse(Elements.AreNeighbours(Elements.Ice, Elements.Lightning), "a pair two apart, with no ring gap of its own");
            Assert.IsTrue(MarkRules.Open(glass, Hand(Elements.Lightning, Elements.Ice), false), "both carried");
            Assert.IsTrue(MarkRules.Open(glass, Hand(Elements.Ice, Elements.Lightning), false), "in either order");
            Assert.IsFalse(MarkRules.Open(glass, Hand(Elements.Lightning, ""), false), "one alone");
            Assert.IsFalse(MarkRules.Open(glass, Hand(Elements.Lightning, Elements.Lightning), false), "all in on one");
            Assert.IsFalse(MarkRules.Open(glass, Hand(Elements.Fire, Elements.Lightning), false), "with another partner");
            Assert.IsTrue(MarkRules.Open(glass, Hand("", ""), true), "the playtest's free chart opens every branch");
            Assert.IsFalse(MarkRules.Open(glass, null, false));
            var fire = new Upgrade { Id = "t.fire", Element = Elements.Fire, Kind = UpgradeKind.Major };
            Assert.IsTrue(MarkRules.Open(fire, Hand(Elements.Lightning, Elements.Fire), false), "an element's mark while its element is carried, first or second");
            Assert.IsFalse(MarkRules.Open(fire, Hand(Elements.Lightning, Elements.Ice), false));
            Assert.IsFalse(MarkRules.Open(new Upgrade { Id = "t.react", Element = Elements.Fire, Element2 = Elements.Water, Kind = UpgradeKind.Reaction, Tree = MarkTree.Hidden }, Hand(Elements.Fire, Elements.Water), true),
                "a reaction is on no chart");
        }

        [Test]
        public void TheHeartDrawsOnlyItsDuoBranchesAndKnowsTheirPair()
        {
            var heart = HeartLayout.Instance;
            var glass = Duo("t.glass", Elements.Ice, Elements.Lightning);
            var ringMix = Duo("t.tempest", Elements.Lightning, Elements.Wind, MarkTree.Element);
            Assert.IsTrue(heart.Draws(glass));
            Assert.IsFalse(heart.Draws(ringMix), "a mix not reshaped yet stays on the ring");
            Assert.IsFalse(RingLayout.Instance.Draws(glass), "and the ring leaves the heart's branches to it");
            Assert.IsFalse(heart.Draws(new Upgrade { Id = "t.fire", Element = Elements.Fire, Tree = MarkTree.Duo }), "a mark of one element is never in the heart");
            int sleet = AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Chill);
            Assert.AreEqual(sleet, HeartLayout.PairOf(glass));
            Assert.AreEqual(sleet, HeartLayout.PairOf(Elements.Lightning, Elements.Ice), "either order");
            Assert.AreEqual(-1, HeartLayout.PairOf(Elements.Fire, Elements.Fire));
            Assert.IsTrue(heart.Shows(glass, sleet));
            Assert.IsFalse(heart.Shows(glass, HeartLayout.PairOf(Elements.Fire, Elements.Lightning)));
            Assert.AreEqual("Ice and Lightning: Sleet Charge", HeartLayout.Title(Elements.Ice, Elements.Lightning));
            HeartPattern.Put(ringMix, HeartPattern.Spell);
            Assert.AreEqual(MarkTree.Duo, ringMix.Tree, "a mark put in the heart's pattern is drawn in the heart");
            Assert.AreEqual(HeartPattern.Of(HeartPattern.Spell).Y, heart.Place(ringMix).y, 1e-5f);
        }

        [Test]
        public void TheHeartShowsTheCarriedPairOrInThePlaytestThePickedOne()
        {
            var marks = new List<Upgrade>
            {
                Duo("t.plasma", Elements.Fire, Elements.Lightning),
                Duo("t.glass", Elements.Ice, Elements.Lightning),
                Duo("t.tempest", Elements.Lightning, Elements.Wind, MarkTree.Element),
            };
            // The game: the other element carried, built or not; a Hand of one shows nothing.
            Assert.AreEqual(Elements.Ice, HeartLayout.PartnerFor(Elements.Lightning, Hand(Elements.Lightning, Elements.Ice), false, "", marks));
            Assert.AreEqual(Elements.Water, HeartLayout.PartnerFor(Elements.Fire, Hand(Elements.Water, Elements.Fire), false, "", marks), "an unbuilt pair is still the pair carried");
            Assert.AreEqual("", HeartLayout.PartnerFor(Elements.Fire, Hand(Elements.Fire, ""), false, "", marks));
            Assert.AreEqual("", HeartLayout.PartnerFor(Elements.Wind, Hand(Elements.Fire, Elements.Lightning), false, "", marks), "a tree not carried");
            // The playtest: the pick, when its branch is built; else the first built partner round the ring.
            Assert.AreEqual(Elements.Ice, HeartLayout.PartnerFor(Elements.Lightning, Hand(Elements.Fire, ""), true, Elements.Ice, marks));
            Assert.AreEqual(Elements.Ice, HeartLayout.PartnerFor(Elements.Lightning, Hand(Elements.Fire, ""), true, "", marks), "Ice is the first built partner after Lightning");
            Assert.AreEqual(Elements.Lightning, HeartLayout.PartnerFor(Elements.Fire, Hand(Elements.Fire, ""), true, Elements.Wind, marks), "a pick with nothing built falls back");
            Assert.AreEqual(Elements.Fire, HeartLayout.PartnerFor(Elements.Lightning, Hand(Elements.Lightning, Elements.Fire), true, "", marks), "with no pick, the pair carried");
            // The shoulder buttons step round the built partners only.
            Assert.AreEqual(Elements.Fire, HeartLayout.NextPartner(Elements.Lightning, Elements.Ice, 1, marks));
            Assert.AreEqual(Elements.Ice, HeartLayout.NextPartner(Elements.Lightning, Elements.Fire, 1, marks));
            Assert.AreEqual(Elements.Ice, HeartLayout.NextPartner(Elements.Lightning, Elements.Fire, -1, marks));
            Assert.AreEqual(Elements.Lightning, HeartLayout.NextPartner(Elements.Fire, Elements.Lightning, 1, marks), "one built partner: it stays");
            Assert.AreEqual("", HeartLayout.NextPartner(Elements.Wind, "", 1, marks), "Wind's only branch is still on the ring");
        }

        [Test]
        public void AReactionWearsItsChosenShapeInsteadOfThePlainOnceTheShapeIsLearnedAndBothAreCarried()
        {
            // A Shape mark for Steam (Fire and Water), whose duo branch is not built: the plain Steam is the game's.
            var first = new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 1f };
            var second = new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 2f };
            var shapeMark = HeartPattern.Put(new Upgrade
            {
                Id = "t.steam.shape", Name = "Steam: Kettle or Geyser", Element = Elements.Fire, Element2 = Elements.Water, Kind = UpgradeKind.Mix, Part = BranchPart.Shape,
                Description = "A test shape.",
                Shapes = new[] { new ShapeDef { Name = "Kettle", Effects = new[] { first } }, new ShapeDef { Name = "Geyser", Effects = new[] { second } } },
            }, HeartPattern.Foot);
            Upgrade.Gather(list => list.Add(shapeMark));
            int steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
            var plain = ReactionTable.MarkFor(steam);
            Assert.IsNotNull(plain, "the plain Steam is built");
            Assert.IsTrue(ReactionTable.IsReactionShape(shapeMark));
            Assert.AreSame(shapeMark, ReactionTable.ShapeMarkFor(steam));
            Assert.AreEqual("react.steam", ReactionTable.ShapeKey(steam));
            Assert.AreSame(shapeMark, first.Owner, "a shape's effects belong to its Shape mark");

            var hand = Hand(Elements.Water, Elements.Fire);
            var learned = new HashSet<string>();
            Assert.AreSame(plain.Effects, ReactionTable.EffectsFor(steam, hand, learned), "before the Shape is bought, the plain reaction");
            learned.Add(shapeMark.Id);
            var worn = ReactionTable.EffectsFor(steam, hand, learned);
            Assert.AreEqual(1, worn.Count);
            Assert.AreSame(first, worn[0], "bought, the first shape runs instead");
            Assert.AreEqual("Kettle", ReactionTable.WornShape(steam, hand, learned).Name);

            SpellBranches.Choose(hand.Shapes, ReactionTable.ShapeKey(steam), 1);
            Assert.AreSame(second, ReactionTable.EffectsFor(steam, hand, learned)[0], "the shape swapped at the rest fire changes what the reaction runs");
            Assert.AreEqual("Geyser", ReactionTable.WornShape(steam, hand, learned).Name);
            Assert.AreNotEqual(ReactionTable.EffectsFor(steam, hand, learned)[0].Do, ReactionTable.EffectsFor(steam, Hand(Elements.Water, Elements.Fire), learned)[0].Do,
                "a Hand with no choice wears the first shape");

            Assert.AreSame(plain.Effects, ReactionTable.EffectsFor(steam, Hand(Elements.Fire, Elements.Lightning), learned), "with the pair broken, the plain reaction (from the world's water)");
            Assert.AreSame(plain.Effects, ReactionTable.EffectsFor(steam, hand), "asked without what is learned, the plain reaction");
            // A reaction's shape is never a standing effect: it runs only when the pair reacts.
            Assert.IsNull(SpellBranches.Worn(shapeMark, hand));
            Assert.IsTrue(MarkRules.InForce(shapeMark, hand, learned), "its Shape mark needs no slot");
        }

        [Test]
        public void EveryReactionShapeRidesAReactionsMomentsAndAnswersOnlyTheConditionsItSets()
        {
            var reactionTriggers = new[] { UpgradeTrigger.Reacted, UpgradeTrigger.ConditionBeat, UpgradeTrigger.ConditionStruck, UpgradeTrigger.ConditionEnded, UpgradeTrigger.ConditionDied };
            var shapeMarks = Upgrade.All.Where(ReactionTable.IsReactionShape).ToList();
            Assert.GreaterOrEqual(shapeMarks.Count, 2, "Flashover's and Sleet Charge's at least");
            var conditionIds = new HashSet<string>();
            foreach (var mark in shapeMarks)
            {
                Assert.AreEqual(MarkTree.Duo, mark.Tree, mark.Id + " is drawn in the heart");
                Assert.IsEmpty(mark.Effects, mark.Id + ": a reaction's Shape mark does nothing of its own; its shapes do");
                Assert.IsNotNull(ReactionTable.MarkFor(HeartLayout.PairOf(mark)), mark.Id + " shapes a reaction that is built");
                foreach (var shape in mark.Shapes)
                {
                    var sets = new HashSet<string>(shape.Effects.Where(effect => effect.Do == UpgradeAction.Condition).Select(effect => effect.State.Id));
                    foreach (string id in sets) Assert.IsTrue(conditionIds.Add(id), $"{mark.Id}'s {shape.Name} sets '{id}', which another shape sets too");
                    foreach (var effect in shape.Effects)
                    {
                        CollectionAssert.Contains(reactionTriggers, effect.On, $"{mark.Id}'s {shape.Name} has an effect on {effect.On}");
                        Assert.AreSame(mark, effect.Owner);
                        if (effect.On != UpgradeTrigger.Reacted)
                            Assert.IsTrue(sets.Contains(effect.Condition), $"{mark.Id}'s {shape.Name} answers '{effect.Condition}', which it never sets");
                    }
                }
            }
        }

        [Test]
        public void FlashoverAndSleetChargeChangeWithTheShapeWorn()
        {
            // The two built branches (task 4.5), on the game's own catalogue.
            int flashover = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);
            int sleet = AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Chill);
            var learned = new HashSet<string> { Clover.Player.Trees.PlasmaTree.Shape, Clover.Player.Trees.Duo.SleetChargeDuo.Shape };
            var plasma = Hand(Elements.Fire, Elements.Lightning);
            Assert.AreEqual("Corona", ReactionTable.WornShape(flashover, plasma, learned).Name);
            Assert.IsTrue(ReactionTable.EffectsFor(flashover, plasma, learned).Any(effect => effect.Do == UpgradeAction.Condition && effect.State.Id == Clover.Player.Trees.PlasmaTree.Corona));
            Assert.IsFalse(ReactionTable.EffectsFor(flashover, plasma, learned).Any(effect => effect.Do == UpgradeAction.SetOff), "Corona spends nothing");
            SpellBranches.Choose(plasma.Shapes, ReactionTable.ShapeKey(flashover), 1);
            Assert.AreEqual("Fulgurite", ReactionTable.WornShape(flashover, plasma, learned).Name);
            Assert.AreEqual(2, ReactionTable.EffectsFor(flashover, plasma, learned).Count(effect => effect.Do == UpgradeAction.Hazard), "Fulgurite lays its patch");
            Assert.IsTrue(ReactionTable.EffectsFor(flashover, Hand(Elements.Fire, ""), learned).Any(effect => effect.Do == UpgradeAction.SetOff), "with Fire alone, the plain Flashover discharges");

            var storm = Hand(Elements.Ice, Elements.Lightning);
            Assert.AreEqual("Thundersnow", ReactionTable.WornShape(sleet, storm, learned).Name);
            SpellBranches.Choose(storm.Shapes, ReactionTable.ShapeKey(sleet), 1);
            var spear = ReactionTable.EffectsFor(sleet, storm, learned);
            Assert.IsTrue(spear.Any(effect => effect.On == UpgradeTrigger.ConditionEnded && effect.Affliction == AfflictionKind.Chill && effect.Stacks >= Afflictions.Depth(AfflictionKind.Chill)),
                "Glass Spear freezes as the charge ends");
            Assert.AreEqual(0, storm.ShapeOf(ReactionTable.ShapeKey(flashover)), "each reaction keeps its own choice");
        }
    }
}
