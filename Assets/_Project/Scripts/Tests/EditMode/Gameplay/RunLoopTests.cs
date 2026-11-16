using Clover.Core;
using Clover.World;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The run's end and what the sponsor says about it.</summary>
    public class RunLoopTests
    {
        [SetUp]
        public void StartClean()
        {
            RunState.Reset();
            RunState.RunsEnded = 0;
        }

        [TearDown]
        public void LeaveClean()
        {
            RunState.Reset();
            RunState.RunsEnded = 0;
            RunState.LastRunFell = false;
            RunState.LastRunGods = 0;
        }

        [Test]
        public void EndingARunRemembersHowItEndedAndGivesUpWhatItCarried()
        {
            RunState.Weapon = "Katana";
            RunState.Element = "Lightning";
            RunState.Points = 4;
            RunState.GodsFelled = 1;
            RunState.Unlocked.Add("fire.heat");
            RunState.End(true);
            Assert.AreEqual(1, RunState.RunsEnded);
            Assert.IsTrue(RunState.LastRunFell);
            Assert.AreEqual(1, RunState.LastRunGods);
            Assert.AreEqual("", RunState.Weapon);
            Assert.AreEqual("", RunState.Element);
            Assert.AreEqual(0, RunState.Points);
            Assert.AreEqual(0, RunState.Unlocked.Count);
        }

        [Test]
        public void TheSponsorSpeaksItsFirstWordsOnlyToSomeoneItHasNotMet()
        {
            Assert.AreSame(SponsorLines.FirstWaking, SponsorLines.For(false, 0, false, 0));
            Assert.AreSame(SponsorLines.FirstWaking, SponsorLines.For(false, 3, true, 0));
            Assert.AreSame(SponsorLines.NothingMore, SponsorLines.For(true, 0, false, 0));
        }

        [Test]
        public void WhatTheSponsorSaysFollowsHowTheLastRunEnded()
        {
            Assert.AreSame(SponsorLines.AfterAGod, SponsorLines.For(true, 2, false, 1));
            Assert.AreSame(SponsorLines.AfterAGod, SponsorLines.For(true, 2, true, 1));
            Assert.AreSame(SponsorLines.CameBackAlive, SponsorLines.For(true, 2, false, 0));
            var first = SponsorLines.For(true, 1, true, 0);
            var second = SponsorLines.For(true, 2, true, 0);
            Assert.AreNotSame(first, second);
            Assert.IsNotEmpty(first);
            // However many times the player falls, there is always something said.
            for (int runs = 1; runs < 40; runs++) Assert.IsNotEmpty(SponsorLines.For(true, runs, true, 0));
        }
    }
}
