using Clover.UI;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>What the pause menu offers where, and that the things which cannot be taken back are asked twice.</summary>
    public class PauseChoicesTests
    {
        // These are the game's own menus, not the playtest's: the mode is a static a play can leave on (domain reload is
        // off), so it is set here and put back after.
        private bool wasOn;

        [SetUp]
        public void OutOfThePlaytest()
        {
            wasOn = Clover.Core.PlaytestMode.On;
            Clover.Core.PlaytestMode.Set(false);
        }

        [TearDown]
        public void PutBack() => Clover.Core.PlaytestMode.Set(wasOn);

        // Moves down to the entry, at most once round the menu, so a missing entry fails the test instead of hanging it.
        private static void MoveTo(PauseChoices choices, PauseEntry entry)
        {
            for (int i = 0; i < choices.Count && choices.Current != entry; i++) choices.Move(1);
            Assert.AreEqual(entry, choices.Current, $"the menu offers no {entry}");
        }

        private static bool Offers(string scene, PauseEntry entry)
        {
            var choices = new PauseChoices(scene);
            for (int i = 0; i < choices.Count; i++)
                if (choices.At(i) == entry) return true;
            return false;
        }

        [Test]
        public void EverySceneOffersResumeFirstThenControlsAndQuitLast()
        {
            foreach (string scene in new[] { "Hub", "Castle", "Summit", "CombatTest" })
            {
                var choices = new PauseChoices(scene);
                Assert.AreEqual(PauseEntry.Resume, choices.At(0), scene);
                Assert.AreEqual(PauseEntry.Controls, choices.At(1), scene);
                Assert.AreEqual(PauseEntry.Quit, choices.At(choices.Count - 1), scene);
            }
        }

        [Test]
        public void OnlyARunCanBeGivenUp()
        {
            Assert.IsTrue(Offers("Castle", PauseEntry.GiveUp));
            Assert.IsTrue(Offers("Summit", PauseEntry.GiveUp));
            Assert.IsFalse(Offers("Hub", PauseEntry.GiveUp));
            Assert.IsFalse(Offers("CombatTest", PauseEntry.GiveUp));
        }

        [Test]
        public void OnlyTheHubCanForget()
        {
            Assert.IsTrue(Offers("Hub", PauseEntry.Forget));
            Assert.IsFalse(Offers("Castle", PauseEntry.Forget));
            Assert.IsFalse(Offers("Summit", PauseEntry.Forget));
            Assert.IsFalse(Offers("CombatTest", PauseEntry.Forget));
            Assert.AreEqual(4, new PauseChoices("CombatTest").Count);
            Assert.AreEqual(5, new PauseChoices("Hub").Count);
        }

        [Test]
        public void ResumeAndControlsActOnTheFirstPress()
        {
            var choices = new PauseChoices("Hub");
            Assert.AreEqual(PauseEntry.Resume, choices.Press());
            choices.Move(1);
            Assert.AreEqual(PauseEntry.Controls, choices.Press());
            Assert.IsFalse(choices.Asking);
        }

        [Test]
        public void WhatCannotBeTakenBackIsAskedOnceThenDone()
        {
            foreach (var entry in new[] { PauseEntry.GiveUp, PauseEntry.Quit })
            {
                var choices = new PauseChoices("Castle");
                MoveTo(choices, entry);
                Assert.AreEqual(PauseEntry.None, choices.Press(), "the first press only asks");
                Assert.IsTrue(choices.Asking);
                Assert.AreEqual(entry, choices.Press(), "the second press does it");
                Assert.IsFalse(choices.Asking);
            }
            var hub = new PauseChoices("Hub");
            MoveTo(hub, PauseEntry.Forget);
            Assert.AreEqual(PauseEntry.None, hub.Press());
            Assert.AreEqual(PauseEntry.Forget, hub.Press());
        }

        [Test]
        public void MovingAwayOrGoingBackDropsTheQuestion()
        {
            var choices = new PauseChoices("Castle");
            choices.Move(-1);
            Assert.AreEqual(PauseEntry.Quit, choices.Current, "up from the top goes round to the bottom");
            choices.Press();
            choices.Move(-1);
            Assert.IsFalse(choices.Asking);
            choices.Move(1);
            Assert.AreEqual(PauseEntry.None, choices.Press(), "it asks again after being left");
            Assert.IsTrue(choices.Cancel());
            Assert.IsFalse(choices.Cancel(), "nothing left to drop");
            Assert.AreEqual(PauseEntry.None, choices.Press());
            choices.Reset();
            Assert.AreEqual(PauseEntry.Resume, choices.Current);
            Assert.IsFalse(choices.Asking);
        }

        [Test]
        public void EveryEntryHasWordsAndTheAskedOnesAQuestion()
        {
            foreach (var entry in new[] { PauseEntry.Resume, PauseEntry.Controls, PauseEntry.GiveUp, PauseEntry.Forget, PauseEntry.Quit })
            {
                Assert.IsNotEmpty(PauseChoices.Label(entry));
                if (PauseChoices.AsksFirst(entry)) Assert.AreNotEqual(PauseChoices.Label(entry), PauseChoices.Question(entry));
            }
        }
    }
}
