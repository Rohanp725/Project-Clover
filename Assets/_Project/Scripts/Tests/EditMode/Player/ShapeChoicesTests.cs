using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Build;
using Clover.UI;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Reactions page's list (the owner's answer Q4 of 2026-11-21): every learned reaction's Shape mark, each with
    /// the shape worn and whether it is in force; a spell's shape is chosen on the Loadout, so none is listed here.
    /// Stepping goes round the shapes.
    /// </summary>
    public class ShapeChoicesTests
    {
        private static ShapeDef[] Two(string first, string second) => new[] { new ShapeDef { Name = first }, new ShapeDef { Name = second } };

        private static readonly Upgrade RushShape = new Upgrade
        {
            Id = "t.rush.shape", Element = Elements.Fire, Kind = UpgradeKind.Major, Part = BranchPart.Shape, Branch = "t.rush", Shapes = Two("Low", "High"),
        };
        private static readonly Upgrade SteamShape = new Upgrade
        {
            Id = "t.steam.shape", Element = Elements.Fire, Element2 = Elements.Water, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Shapes = Two("Blind", "Scald"),
        };
        private static readonly Upgrade NotLearned = new Upgrade
        {
            Id = "t.other.shape", Element = Elements.Ice, Kind = UpgradeKind.Major, Part = BranchPart.Shape, Branch = "t.other", Shapes = Two("A", "B"),
        };
        private static readonly Upgrade Plain = new Upgrade { Id = "t.plain", Element = Elements.Fire, Kind = UpgradeKind.Major };

        [Test]
        public void OnlyTheLearnedReactionShapesAreListedWithTheShapeWorn()
        {
            var marks = new List<Upgrade> { SteamShape, Plain, NotLearned, RushShape };
            var learned = new HashSet<string> { SteamShape.Id, RushShape.Id, Plain.Id };
            var loadout = new Loadout();
            loadout.Attune(Elements.Fire, Elements.Water);
            loadout.Shapes.Add(new IdChoice("t.rush", 1));

            var choices = new ShapeChoices();
            choices.Fill(marks, learned, loadout);
            Assert.AreEqual(1, choices.Count, "only learned reaction Shape marks: the spell's is on the Loadout");
            var reaction = choices.At(0);
            int steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
            Assert.IsTrue(reaction.Reaction);
            Assert.AreEqual(ReactionTable.ShapeKey(steam), reaction.Key, "a reaction's choice is kept under the reaction");
            Assert.AreEqual(AfflictionPairs.Name(steam), reaction.Title);
            Assert.AreEqual(0, reaction.Worn, "the first shape until another is chosen");
            Assert.IsTrue(reaction.InForce, "both elements carried");
        }

        [Test]
        public void TheChoiceMovesRoundTheEndsStepsRoundTheShapesAndStaysOnItsLineWhenFilledAgain()
        {
            var galeShape = new Upgrade
            {
                Id = "t.gale.shape", Element = Elements.Wind, Element2 = Elements.Water, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Shapes = Two("Wide", "Narrow"),
            };
            var marks = new List<Upgrade> { RushShape, SteamShape, galeShape };
            var learned = new HashSet<string> { SteamShape.Id, RushShape.Id, galeShape.Id };
            var loadout = new Loadout();
            loadout.Attune(Elements.Fire, Elements.Water);
            var choices = new ShapeChoices();
            choices.Fill(marks, learned, loadout);
            Assert.AreEqual(0, choices.Selected);
            Assert.AreEqual(1, choices.Step(1), "the next shape");
            Assert.AreEqual(1, choices.Step(-1), "round the ends");
            choices.Move(-1);
            Assert.AreEqual(1, choices.Selected);
            loadout.Shapes.Add(new IdChoice(choices.Current.Key, 1));
            choices.Fill(marks, learned, loadout);
            Assert.AreEqual(1, choices.Selected, "filled again after a choice, it stays on the same line");
            Assert.AreEqual(1, choices.Current.Worn);
            Assert.IsTrue(choices.Choose(0));
            Assert.IsFalse(choices.Choose(2));
            choices.Fill(marks, new HashSet<string>(), loadout);
            Assert.AreEqual(0, choices.Count, "nothing learned, nothing listed");
            Assert.AreEqual(0, choices.Step(1));
        }

        [Test]
        public void EveryShapeMarkInTheGameHasItsKeyAndTwoNamedShapes()
        {
            var learned = new HashSet<string>();
            foreach (var mark in Upgrade.All)
                if (mark.Part == BranchPart.Shape && mark.Shapes != null && mark.Shapes.Length > 0) learned.Add(mark.Id);
            var loadout = new Loadout();
            var choices = new ShapeChoices();
            choices.Fill(Upgrade.All, learned, loadout);
            Assert.AreEqual(AfflictionPairs.Count, choices.Count, "every duo branch's Shape mark, and no spell's");
            int reactions = 0;
            for (int i = 0; i < choices.Count; i++)
            {
                var row = choices.At(i);
                Assert.IsNotEmpty(row.Key, row.Mark.Id);
                Assert.IsNotEmpty(row.Title, row.Mark.Id);
                Assert.AreEqual(2, row.ShapeCount, row.Mark.Id);
                if (row.Reaction) reactions++;
            }
            Assert.AreEqual(AfflictionPairs.Count, reactions, "one reaction line for each pair");
        }
    }
}
