using System.Collections.Generic;
using Clover.Gameplay;
using NUnit.Framework;

namespace Clover.Tests
{
    public class ColossusChooserTests
    {
        private static ColossusChooser Chooser()
        {
            var chooser = new ColossusChooser();
            chooser.Reset();
            return chooser;
        }

        // Past the opening, with the player in the middle of the floor, taking lulls as they come due.
        private static ColossusChooser PastTheOpening()
        {
            var chooser = Chooser();
            for (int i = 0; i < chooser.Opening.Length; i++)
            {
                chooser.Next(0f, 0f, false, false, false);
                chooser.TakeLull();
            }
            return chooser;
        }

        [Test]
        public void TheFightOpensWithItsSetPatternWhereverThePlayerStands()
        {
            var chooser = Chooser();
            var expected = (ColossusAttack[])chooser.Opening.Clone();
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], chooser.Next(0.99f, 0.99f, false, false, i % 2 == 0), "opening step " + i);
            CollectionAssert.DoesNotContain(expected, ColossusAttack.Bite);
        }

        [Test]
        public void ThePlayerUnderTheHeadIsBittenButNotTwiceRunning()
        {
            var chooser = PastTheOpening();
            Assert.AreEqual(ColossusAttack.Bite, chooser.Next(0f, 0f, true, true, false));
            Assert.AreNotEqual(ColossusAttack.Bite, chooser.Next(0f, 0f, true, true, false));
            Assert.AreEqual(ColossusAttack.Bite, chooser.Next(0f, 0f, true, true, false));
        }

        [Test]
        public void ThePlayerNearTheHeadIsNeverBreathedOn()
        {
            var chooser = PastTheOpening();
            for (int i = 0; i < 200; i++)
                Assert.AreNotEqual(ColossusAttack.Breath, chooser.Next(0.999f, 0f, false, true, false));
        }

        [Test]
        public void ThePlayerFarSouthIsBreathedOnWhenTheSecondRollSaysSo()
        {
            var chooser = PastTheOpening();
            // The opening ends on the clap, so the breath is free to come.
            Assert.AreEqual(ColossusAttack.Breath, chooser.Next(0f, 0f, false, false, true));
            // Not twice running, even far south.
            Assert.AreNotEqual(ColossusAttack.Breath, chooser.Next(0.999f, 0f, false, false, true));
            // And not when the second roll is against it and the first roll picks something else.
            Assert.AreEqual(ColossusAttack.Slam, chooser.Next(0f, 0.99f, false, false, true));
        }

        [Test]
        public void NothingIsDoneThreeTimesRunningAndOnlyTheSlamTwice()
        {
            var chooser = PastTheOpening();
            var random = new System.Random(7);
            var history = new List<ColossusAttack>();
            for (int i = 0; i < 3000; i++)
            {
                // Sometimes the same roll again and again, which is what would repeat an attack.
                float roll = i % 50 < 10 ? 0.05f : i % 50 < 20 ? 0.6f : (float)random.NextDouble();
                history.Add(chooser.Next(roll, (float)random.NextDouble(), random.Next(4) == 0, random.Next(3) == 0, random.Next(3) == 0));
                chooser.TakeLull();
            }
            for (int i = 2; i < history.Count; i++)
            {
                Assert.IsFalse(history[i] == history[i - 1] && history[i] == history[i - 2], $"{history[i]} three times at {i}");
                if (history[i] == history[i - 1]) Assert.AreEqual(ColossusAttack.Slam, history[i], "only the slam comes twice running");
            }
            foreach (ColossusAttack attack in System.Enum.GetValues(typeof(ColossusAttack)))
                if (attack != ColossusAttack.None) CollectionAssert.Contains(history, attack);
        }

        [Test]
        public void WhenSlamsComeInPairsEvenTheSlamIsNotChosenTwiceRunning()
        {
            var chooser = PastTheOpening();
            // A roll of nothing is always the slam, when the slam is allowed.
            Assert.AreEqual(ColossusAttack.Slam, chooser.Next(0f, 0.99f, false, false, false, true));
            Assert.AreNotEqual(ColossusAttack.Slam, chooser.Next(0f, 0.99f, false, false, false, true));
            Assert.AreEqual(ColossusAttack.Slam, chooser.Next(0f, 0.99f, false, false, false, true));
        }

        [Test]
        public void ALullIsOwedAfterTwoHeavyAttacksAndTakenOnce()
        {
            var chooser = Chooser();
            // The opening: slam, sweep, breath, clap.
            chooser.Next(0f, 0f, false, false, false);
            Assert.IsFalse(chooser.LullOwed, "a slam is not heavy");
            chooser.Next(0f, 0f, false, false, false);
            Assert.IsFalse(chooser.LullOwed, "one heavy attack is not two");
            chooser.Next(0f, 0f, false, false, false);
            Assert.IsTrue(chooser.LullOwed, "sweep and breath make two");
            Assert.IsTrue(chooser.TakeLull());
            Assert.IsFalse(chooser.TakeLull(), "the lull is taken once");
            chooser.Next(0f, 0f, false, false, false);
            Assert.IsFalse(chooser.LullOwed, "the count begins again after a lull");
        }

        [Test]
        public void HeavyMeansTheOnesWithNoEasyAnswer()
        {
            Assert.IsFalse(ColossusChooser.IsHeavy(ColossusAttack.Slam));
            Assert.IsFalse(ColossusChooser.IsHeavy(ColossusAttack.Bolts));
            Assert.IsTrue(ColossusChooser.IsHeavy(ColossusAttack.Sweep));
            Assert.IsTrue(ColossusChooser.IsHeavy(ColossusAttack.Breath));
            Assert.IsTrue(ColossusChooser.IsHeavy(ColossusAttack.Bite));
            Assert.IsTrue(ColossusChooser.IsHeavy(ColossusAttack.Clap));
        }

        [Test]
        public void ResetBeginsTheOpeningAgain()
        {
            var chooser = PastTheOpening();
            chooser.Next(0.5f, 0.5f, false, false, false);
            chooser.Reset();
            Assert.AreEqual(0, chooser.Chosen);
            Assert.AreEqual(chooser.Opening[0], chooser.Next(0.99f, 0.99f, true, true, true));
        }
    }
}
