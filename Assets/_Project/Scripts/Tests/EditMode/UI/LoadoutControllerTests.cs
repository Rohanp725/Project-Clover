using System;
using System.Collections.Generic;
using Clover.UI.Toolkit;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Loadout's logic driven without a screen (ui_rest_fire_plan.md 3.5.4 and 3.5.5): presses, lists, preview then
    /// commit, Y for shapes, the held X, the halves, the text size and its folding, and the pane's words.
    /// </summary>
    public class LoadoutControllerTests
    {
        private FakeLoadoutSource source;
        private LoadoutController loadout;
        private List<MenuSound> sounds;

        [SetUp]
        public void SetUp()
        {
            LoadoutController.ForgetLastChanged();
            source = new FakeLoadoutSource();
            loadout = new LoadoutController(source);
            sounds = new List<MenuSound>();
            loadout.Sounded += sounds.Add;
            loadout.Open();
        }

        [TearDown]
        public void TearDown() => LoadoutController.ForgetLastChanged();

        private void Go(params NavDirection[] path)
        {
            foreach (var direction in path) loadout.Move(direction);
        }

        private string Pane => loadout.Pane.AllText();

        [Test]
        public void ItOpensOnTheWestSlotTheFirstTimeAndOnTheLastPlaceChangedAfter()
        {
            Assert.AreEqual(LoadoutPlace.West, loadout.Focus);
            Assert.AreEqual("Flame Wave", loadout.Pane.Title);
            loadout.JumpHalf(true);
            Go(NavDirection.Down, NavDirection.Down);
            Assert.AreEqual(LoadoutPlace.Keystone2, loadout.Focus);
            loadout.Submit();
            loadout.Close();
            loadout.Open();
            Assert.AreEqual(LoadoutPlace.Keystone2, loadout.Focus, "opens where the last change was made");
        }

        [Test]
        public void TheStickFollowsTheMapAndAPushToNowhereIsADullTick()
        {
            Go(NavDirection.Right);
            Assert.AreEqual(LoadoutPlace.Surge, loadout.Focus);
            Go(NavDirection.Up, NavDirection.Right);
            Assert.AreEqual(LoadoutPlace.RoleStrip, loadout.Focus);
            sounds.Clear();
            Go(NavDirection.Right);
            Assert.AreEqual(LoadoutPlace.RoleStrip, loadout.Focus);
            CollectionAssert.AreEqual(new[] { MenuSound.Blocked }, sounds);
        }

        [Test]
        public void ChoosingASpellPreviewsThenCommitsOnA()
        {
            loadout.Submit();
            Assert.AreEqual(LoadoutMode.SpellList, loadout.Mode);
            Assert.AreEqual(0, loadout.ListSlot);
            Assert.AreEqual(new[] { "Fire", "Water", "Both" }, loadout.Columns.ConvertAll(c => c.Element).ToArray());
            Assert.AreEqual("fire.base", loadout.FocusedRow.Id, "focus opens on the spell worn here");
            Assert.AreEqual("Leave empty", loadout.Columns[0].Rows[loadout.Columns[0].Rows.Count - 1].Name, "the last row of the first column");
            Assert.AreEqual("Choosing for {SpellWest} West", loadout.ListTitle);
            StringAssert.Contains("Worn here now.", Pane);

            Go(NavDirection.Down);
            Assert.AreEqual("fire.kiln", loadout.FocusedRow.Id);
            StringAssert.Contains("Worn here now: Flame Wave, Opener, 7 s", Pane);
            StringAssert.Contains("It moves here from South. South gets Flame Wave.", Pane);
            Assert.AreEqual(1, loadout.RolesPreview[(int)Clover.Player.SpellRole.Opener], "Flame Wave moves to South, so an opener stays");

            Go(NavDirection.Down);
            Assert.AreEqual("fire.dash", loadout.FocusedRow.Id);
            StringAssert.Contains("You will have no opener.", Pane);
            Assert.AreEqual(0, loadout.RolesPreview[(int)Clover.Player.SpellRole.Opener]);
            Assert.AreEqual(1, loadout.Roles[(int)Clover.Player.SpellRole.Opener], "the worn Hand is unchanged before A");
            Assert.AreEqual("fire.base", source.Board.Slots[0].Id, "nothing changes until A");

            loadout.Submit();
            Assert.AreEqual(LoadoutMode.Board, loadout.Mode);
            Assert.AreEqual(LoadoutPlace.West, loadout.Focus, "focus goes back to the slot");
            Assert.AreEqual("fire.dash", source.Board.Slots[0].Id);
            Assert.AreEqual(LoadoutPlace.West, LoadoutController.LastChanged);
            Assert.AreEqual(MenuSound.Set, sounds[sounds.Count - 1]);
        }

        [Test]
        public void ASpellFromAnotherSlotChangesPlacesWithWhatWasHere()
        {
            loadout.Submit();
            Go(NavDirection.Down);
            loadout.Submit();
            Assert.AreEqual("fire.kiln", source.Board.Slots[0].Id);
            Assert.AreEqual("fire.base", source.Board.Slots[3].Id);
        }

        [Test]
        public void BInAListLeavesWithNothingChangedAndBOnTheBoardGoesBackToTheFire()
        {
            loadout.Submit();
            Go(NavDirection.Down, NavDirection.Down);
            Assert.IsFalse(loadout.Cancel(), "B closes the list, not the screen");
            Assert.AreEqual(LoadoutMode.Board, loadout.Mode);
            Assert.AreEqual("fire.base", source.Board.Slots[0].Id);
            Assert.IsFalse(source.Calls.Exists(c => c.StartsWith("WearSpell", StringComparison.Ordinal)));
            Assert.IsTrue(loadout.Cancel(), "B on the board goes back to the fire");
        }

        [Test]
        public void ListsMoveDownColumnsAcrossGroupsAndStopAtTheirEnds()
        {
            loadout.Submit();
            Go(NavDirection.Up);
            Assert.AreEqual("fire.base", loadout.FocusedRow.Id, "the top stops");
            Go(NavDirection.Right);
            Assert.AreEqual("Water", loadout.Columns[loadout.Column].Element);
            loadout.JumpHalf(false);
            Assert.AreEqual("Both", loadout.Columns[loadout.Column].Element, "RB jumps to the next group");
            Assert.AreEqual(0, loadout.Row, "a shorter column takes its last row");
            Go(NavDirection.Right);
            Assert.AreEqual("Both", loadout.Columns[loadout.Column].Element, "the last group stops");
            loadout.JumpHalf(true);
            Assert.AreEqual("Water", loadout.Columns[loadout.Column].Element, "LB jumps back a group");
        }

        [Test]
        public void TheSurgeListHasNoLeaveEmptyRow()
        {
            Go(NavDirection.Right);
            loadout.Submit();
            Assert.AreEqual(4, loadout.ListSlot);
            Assert.AreEqual("Choosing the surge", loadout.ListTitle);
            foreach (var column in loadout.Columns)
                foreach (var row in column.Rows) Assert.IsFalse(row.IsEmpty);
            Go(NavDirection.Right);
            loadout.Submit();
            Assert.AreEqual("water.surge", source.Board.Surge.Id);
        }

        [Test]
        public void YOnASlotCyclesItsShapeAndOnASpellWithOneShapeSaysWhereToBuyIt()
        {
            loadout.Shape();
            CollectionAssert.Contains(source.Calls, "ChooseShape fire.base 1");
            Assert.AreEqual(1, source.Board.Slots[0].Shape);
            StringAssert.Contains("Shape: Long Wave", Pane);
            loadout.Shape();
            CollectionAssert.Contains(source.Calls, "ChooseShape fire.base 0", "round to the first shape again");

            source.Board.Slots[0] = source.Find("fire.dash");
            loadout.Reread();
            sounds.Clear();
            loadout.Shape();
            Assert.AreEqual(MenuSound.Blocked, sounds[0]);
            StringAssert.Contains("Buy Ember Dash's Shape on the element chart. 3 Shards.", Pane);
            Go(NavDirection.Right);
            Assert.AreEqual("", loadout.Notice, "the notice goes with the next move");
        }

        [Test]
        public void YOnAListRowPreviewsTheShapeAndAWearsItInThatShape()
        {
            loadout.Submit();
            Go(NavDirection.Down);
            loadout.Shape();
            Assert.AreEqual(1, loadout.ShapeOf(loadout.FocusedRow));
            StringAssert.Contains("Shape: Open Kiln", Pane);
            Assert.IsFalse(source.Calls.Exists(c => c.StartsWith("ChooseShape", StringComparison.Ordinal)), "a row's shape is only a preview");
            loadout.Submit();
            CollectionAssert.Contains(source.Calls, "WearSpell 0 fire.kiln 1");
        }

        [Test]
        public void AKeystoneNotSetSaysWhatIsSetAndWhatYouLoseThenASetsIt()
        {
            loadout.JumpHalf(true);
            Go(NavDirection.Down, NavDirection.Down);
            Assert.AreEqual(LoadoutPlace.Keystone2, loadout.Focus);
            Assert.AreEqual("A Hundred Days at the Shrine", loadout.Pane.Title);
            Assert.AreEqual("Bought for 4 Memories", loadout.Pane.Price);
            StringAssert.Contains("Set now: The Cut Lands at the Click.", Pane);
            StringAssert.Contains("Setting this one turns that one off.", Pane);
            StringAssert.Contains("You lose: the long held draw that cuts through guards.", Pane);
            Assert.AreEqual("Submit", loadout.Pane.Action);
            Assert.AreEqual("Set this keystone", loadout.Pane.ActionText);
            StringAssert.Contains("Draw: a cut made straight from the sheath.", Pane, "the keyword's line at the foot");

            loadout.Submit();
            CollectionAssert.Contains(source.Calls, "SetPair False katana.sheath.hundreddays");
            Assert.IsTrue(source.Board.Keystones[1].Set);
            Assert.AreEqual("", loadout.Pane.Action, "the set one has nothing to press");
            sounds.Clear();
            loadout.Submit();
            Assert.AreEqual(MenuSound.Blocked, sounds[0]);
        }

        [Test]
        public void ABearingIsSetWithOnePress()
        {
            loadout.JumpHalf(true);
            Go(NavDirection.Down, NavDirection.Right);
            Assert.AreEqual(LoadoutPlace.Bearing1, loadout.Focus);
            StringAssert.Contains("You lose: the second dodge.", Pane);
            loadout.Submit();
            CollectionAssert.Contains(source.Calls, "SetPair True body.enduring");
        }

        [Test]
        public void ALockedKeystoneSaysWhereToBuyItAndAIsRefused()
        {
            source.Board.Keystones[1].Bought = false;
            source.Board.Keystones[1].Lock = "Buy it on the Way chart. 4 Memories.";
            loadout.Reread();
            loadout.JumpHalf(true);
            Go(NavDirection.Down, NavDirection.Down);
            Assert.AreEqual("Buy it on the Way chart. 4 Memories.", loadout.Pane.Lines[0].Text, "the reason is the first line");
            Assert.AreEqual("", loadout.Pane.Action);
            loadout.Submit();
            Assert.IsFalse(source.Calls.Exists(c => c.StartsWith("SetPair", StringComparison.Ordinal)));
        }

        [Test]
        public void ChoosingAnAspectPreviewsItsLookAndKeystonesThenCommits()
        {
            loadout.JumpHalf(true);
            Assert.AreEqual(LoadoutPlace.Aspect, loadout.Focus);
            loadout.Submit();
            Assert.AreEqual(LoadoutMode.AspectCards, loadout.Mode);
            Assert.AreEqual("katana.sheath", loadout.FocusedCard.Id, "focus opens on the active aspect");
            Go(NavDirection.Right);
            Assert.AreEqual("katana.stream", loadout.PreviewLook);
            Assert.AreEqual("No Last Cut", loadout.PreviewKeystones[0].Name);
            StringAssert.Contains("Set now: The Sheath.", Pane);
            StringAssert.Contains("You lose: The Sheath's 6 marks go to sleep.", Pane);
            Assert.AreEqual("katana.sheath", source.Board.Aspect.Id, "nothing changes until A");
            Go(NavDirection.Right);
            StringAssert.Contains("Buy it on the Way chart. 2 Memories.", Pane);
            loadout.Submit();
            Assert.AreEqual(LoadoutMode.AspectCards, loadout.Mode, "an aspect not bought cannot be chosen");
            Go(NavDirection.Left);
            loadout.Submit();
            CollectionAssert.Contains(source.Calls, "SetAspect katana.stream");
            Assert.AreEqual(LoadoutPlace.Aspect, loadout.Focus);
            Assert.AreEqual("katana.stream", loadout.PreviewLook);
        }

        [Test]
        public void HandsAreWornWithAAndKeptWithXHeldWhenOneIsThere()
        {
            Go(NavDirection.Down, NavDirection.Right, NavDirection.Right);
            Assert.AreEqual(LoadoutPlace.Hand2, loadout.Focus);
            Assert.AreEqual(0, loadout.RolesPreview[(int)Clover.Player.SpellRole.Opener], "the role strip previews the saved Hand");
            StringAssert.Contains("West: Whirlpool (now Flame Wave)", Pane);
            loadout.Submit();
            CollectionAssert.Contains(source.Calls, "WearHand 2");

            Go(NavDirection.Right);
            loadout.KeepDown();
            CollectionAssert.Contains(source.Calls, "KeepHand 3", "an empty Hand is kept with one press");

            Go(NavDirection.Left, NavDirection.Left);
            Assert.AreEqual(LoadoutPlace.Hand1, loadout.Focus);
            loadout.KeepDown();
            loadout.KeepHeld(0.3f);
            Assert.AreEqual(0.5f, loadout.KeepProgress, 0.01f);
            loadout.KeepUp();
            Assert.IsFalse(source.Calls.Contains("KeepHand 1"), "let go early, nothing is kept");
            loadout.KeepDown();
            loadout.KeepHeld(0.61f);
            CollectionAssert.Contains(source.Calls, "KeepHand 1");

            Go(NavDirection.Left);
            sounds.Clear();
            loadout.KeepDown();
            Assert.AreEqual(MenuSound.Blocked, sounds[0], "the Starter cannot be overwritten");
        }

        [Test]
        public void BumpersJumpBetweenTheHalvesAndEachHalfRemembersItsPlace()
        {
            loadout.JumpHalf(true);
            Assert.AreEqual(LoadoutPlace.Aspect, loadout.Focus);
            Go(NavDirection.Down);
            loadout.JumpHalf(false);
            Assert.AreEqual(LoadoutPlace.West, loadout.Focus);
            Go(NavDirection.Down);
            loadout.JumpHalf(true);
            Assert.AreEqual(LoadoutPlace.Keystone1, loadout.Focus);
            loadout.JumpHalf(false);
            Assert.AreEqual(LoadoutPlace.Starter, loadout.Focus);
        }

        [Test]
        public void TheTextSizeStepsAndTheHalfWithoutFocusFolds()
        {
            int saved = 0;
            loadout.TextSizeChanged += size => saved = size;
            Assert.AreEqual(LoadoutHalf.None, loadout.Folded, "nothing folds at 100%");
            loadout.StepTextSize();
            Assert.AreEqual(125, loadout.TextSize);
            Assert.AreEqual(125, saved);
            Assert.AreEqual(LoadoutHalf.Steel, loadout.Folded, "focus is in the Element half");
            loadout.JumpHalf(true);
            Assert.AreEqual(LoadoutHalf.Element, loadout.Folded);
            loadout.Submit();
            Assert.AreEqual(LoadoutHalf.Element, loadout.Folded, "the aspect cards open over the folded half");
            loadout.Cancel();
            loadout.JumpHalf(false);
            loadout.Submit();
            Assert.AreEqual(LoadoutHalf.Steel, loadout.Folded, "a spell list opens over the folded Steel half");
            loadout.Cancel();
            loadout.StepTextSize();
            Assert.AreEqual(150, loadout.TextSize);
            loadout.StepTextSize();
            Assert.AreEqual(100, loadout.TextSize);
            Assert.AreEqual(LoadoutHalf.None, loadout.Folded);
        }

        [Test]
        public void TheFoldedSummariesAndTheRoleStripReadAsTheMockup()
        {
            Assert.AreEqual("Katana · The Sheath · The Cut Lands at the Click · The Light Body · Temper II", loadout.SteelSummary());
            Assert.AreEqual("Fire + Water · O1 B1 D1 C1 M0", loadout.ElementSummary());
            Assert.AreEqual("No movement spell", loadout.MissingRoles);
        }

        [Test]
        public void ThePointerFocusesWhatItIsOverAndAClickActsOnIt()
        {
            loadout.PointAt(LoadoutPlace.Temper);
            Assert.AreEqual(LoadoutPlace.Temper, loadout.Focus);
            Assert.AreEqual("Temper II", loadout.Pane.Title);
            loadout.Click(LoadoutPlace.Hand2);
            CollectionAssert.Contains(source.Calls, "WearHand 2");
            loadout.Click(LoadoutPlace.North);
            Assert.AreEqual(LoadoutMode.SpellList, loadout.Mode);
            loadout.ClickRow(1, 0);
            CollectionAssert.Contains(source.Calls, "WearSpell 1 water.base 0");
        }

        [Test]
        public void AOnTheElementHeaderAsksForTheReactionsPage()
        {
            bool asked = false;
            loadout.ReactionsAsked += () => asked = true;
            Go(NavDirection.Up);
            Assert.AreEqual(LoadoutPlace.ElementHeader, loadout.Focus);
            Assert.AreEqual("Fire + Water", loadout.Pane.Title);
            loadout.Submit();
            Assert.IsTrue(asked);
        }

        [Test]
        public void AShutSlotSaysWhenItOpensAndCannotBeChosen()
        {
            source.Board.Slots[2] = new SpellCard { Open = false, ShutReason = "Opens at the Champion", ShutDetail = "This slot opens when the Champion of the Gate falls." };
            loadout.Reread();
            Go(NavDirection.Right, NavDirection.Right);
            Assert.AreEqual(LoadoutPlace.East, loadout.Focus);
            Assert.AreEqual("Shut", loadout.Pane.Title);
            StringAssert.Contains("Champion of the Gate", Pane);
            loadout.Submit();
            Assert.AreEqual(LoadoutMode.Board, loadout.Mode);
            loadout.Shape();
            Assert.IsFalse(source.Calls.Exists(c => c.StartsWith("ChooseShape", StringComparison.Ordinal)));
        }

        [Test]
        public void EveryPlaceHasAPaneWithANameAndWords()
        {
            foreach (LoadoutPlace place in Enum.GetValues(typeof(LoadoutPlace)))
            {
                loadout.PointAt(place);
                Assert.AreEqual(place, loadout.Focus);
                Assert.IsNotEmpty(loadout.Pane.Title, place.ToString());
                Assert.IsNotEmpty(loadout.Pane.Kind, place.ToString());
                Assert.Greater(loadout.Pane.Lines.Count, 0, place + " says nothing");
            }
        }

        [Test]
        public void TheBindingsReadTheBoardAsTheSourceGivesIt()
        {
            Assert.AreEqual(1, source.Calls.FindAll(c => c == "Read").Count, "read once on opening");
            Assert.AreEqual("Flame Wave", loadout.Board.Slots[0].Name);
            Assert.AreEqual("Opener", Clover.UI.Toolkit.LoadoutWords.RoleName(loadout.Board.Slots[0].Role));
            Assert.AreEqual("7 s", loadout.Board.Slots[0].CostShort);
            Assert.AreEqual("Cinder Fan", loadout.Board.Slots[0].ShapeName);
            Assert.AreEqual("Kindle", loadout.Board.Surge.Name);
            Assert.AreEqual(2, loadout.Board.Temper);
            Assert.AreEqual(12, loadout.Board.Memories);
            source.Board.Memories = 20;
            loadout.Reread();
            Assert.AreEqual(20, loadout.Board.Memories);
        }
    }
}
