using System.Collections.Generic;
using Clover.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    public class DragonChooserTests
    {
        private static DragonChooser Chooser()
        {
            var chooser = new DragonChooser();
            chooser.Reset();
            return chooser;
        }

        // Past the opening, with the player out of reach of the head, taking lulls as they come due.
        private static DragonChooser PastTheOpening()
        {
            var chooser = Chooser();
            for (int i = 0; i < chooser.Opening.Length; i++)
            {
                chooser.Next(0f, false, false, false);
                chooser.TakeLull();
            }
            return chooser;
        }

        [Test]
        public void TheFightOpensWithItsSetPatternWhereverThePlayerStands()
        {
            var chooser = Chooser();
            var expected = (DragonAttack[])chooser.Opening.Clone();
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], chooser.Next(0.99f, i % 2 == 0, i % 2 == 0, false), "opening step " + i);
            CollectionAssert.DoesNotContain(expected, DragonAttack.Bite);
            // It shows the mouth, the tail and the magic before anything is left to chance.
            Assert.IsTrue(System.Array.Exists(expected, DragonChooser.IsMouth));
            Assert.IsTrue(System.Array.Exists(expected, DragonChooser.IsTail));
            Assert.IsTrue(System.Array.Exists(expected, DragonChooser.IsMagic));
        }

        [Test]
        public void ThePlayerWhoLingersInReachIsBittenButNotTwiceRunning()
        {
            var chooser = PastTheOpening();
            // The opening ends on the tail, so the mouth is free to come.
            Assert.AreEqual(DragonAttack.Bite, chooser.Next(0.99f, true, true, false));
            Assert.AreNotEqual(DragonAttack.Bite, chooser.Next(0.99f, true, true, false));
            Assert.AreEqual(DragonAttack.Bite, chooser.Next(0.99f, true, true, false));
        }

        [Test]
        public void ThePlayerOutOfReachIsNeverBitten()
        {
            var chooser = PastTheOpening();
            var random = new System.Random(3);
            for (int i = 0; i < 2000; i++)
            {
                Assert.AreNotEqual(DragonAttack.Bite, chooser.Next((float)random.NextDouble(), false, false, random.Next(2) == 0));
                chooser.TakeLull();
            }
        }

        [Test]
        public void NothingIsDoneTwiceRunningAndEverythingComesUp()
        {
            var chooser = PastTheOpening();
            var random = new System.Random(7);
            var history = new List<DragonAttack>();
            for (int i = 0; i < 4000; i++)
            {
                // Sometimes the same roll again and again, which is what would repeat an attack.
                float roll = i % 50 < 10 ? 0.05f : i % 50 < 20 ? 0.6f : (float)random.NextDouble();
                bool inReach = random.Next(3) == 0;
                history.Add(chooser.Next(roll, inReach && random.Next(2) == 0, inReach, false));
                chooser.TakeLull();
            }
            for (int i = 1; i < history.Count; i++) Assert.AreNotEqual(history[i - 1], history[i], $"{history[i]} twice at {i}");
            foreach (DragonAttack attack in System.Enum.GetValues(typeof(DragonAttack)))
                if (attack != DragonAttack.None) CollectionAssert.Contains(history, attack);
        }

        [Test]
        public void AfterTheTailOrASpellTheMouthComesAndSoTheMouthIsMostOfTheFight()
        {
            var chooser = PastTheOpening();
            var random = new System.Random(11);
            var last = chooser.Last;
            int mouth = 0;
            const int Turns = 4000;
            for (int i = 0; i < Turns; i++)
            {
                bool inReach = random.Next(3) == 0;
                var pick = chooser.Next((float)random.NextDouble(), false, inReach, random.Next(4) == 0);
                chooser.TakeLull();
                if (!DragonChooser.IsMouth(last)) Assert.IsTrue(DragonChooser.IsMouth(pick), $"{pick} after {last} at {i}");
                if (DragonChooser.IsMouth(pick)) mouth++;
                last = pick;
            }
            Assert.Greater(mouth, Turns / 2, "the mouth is more than half of what he does");
        }

        [Test]
        public void NoStormIsCalledWhileOneStillBurns()
        {
            var chooser = PastTheOpening();
            for (int i = 0; i < 500; i++)
            {
                // A roll of one is always the storm, when the storm is allowed.
                Assert.AreNotEqual(DragonAttack.Storm, chooser.Next(1f, false, false, true));
                chooser.TakeLull();
            }
            // After the mouth, with no storm burning, a roll of one is the storm.
            chooser.Reset();
            chooser.Opening = new[] { DragonAttack.Beam };
            chooser.Next(0f, false, false, false);
            Assert.AreEqual(DragonAttack.Storm, chooser.Next(1f, false, false, false));
        }

        [Test]
        public void ALullIsOwedAfterTwoHeavyAttacksAndTakenOnce()
        {
            var chooser = Chooser();
            // The opening: beam, tail slam, breath, bolts, beam, tail sweep.
            chooser.Next(0f, false, false, false);
            Assert.IsFalse(chooser.LullOwed, "one heavy attack is not two");
            chooser.Next(0f, false, false, false);
            chooser.Next(0f, false, false, false);
            chooser.Next(0f, false, false, false);
            Assert.IsFalse(chooser.LullOwed, "the slam, the breath and the bolts are not heavy");
            chooser.Next(0f, false, false, false);
            Assert.IsTrue(chooser.LullOwed, "two beams make two");
            Assert.IsTrue(chooser.TakeLull());
            Assert.IsFalse(chooser.TakeLull(), "it is taken once");
            Assert.IsFalse(chooser.LullOwed);
        }

        [Test]
        public void AnAttackDecidedElsewhereIsCountedByTheRules()
        {
            var chooser = PastTheOpening();
            chooser.Record(DragonAttack.Storm);
            Assert.AreEqual(DragonAttack.Storm, chooser.Last);
            Assert.IsTrue(DragonChooser.IsMouth(chooser.Next(0.99f, false, false, false)), "after the storm that opens his fury, the mouth");
        }

        [Test]
        public void EveryArchOfTheBodyGoesWhollyUnderAndComesWhollyUpAndMostStandAtOnce()
        {
            var winding = new SerpentWinding();
            const int Arches = 10;
            var lowest = new float[Arches];
            var highest = new float[Arches];
            for (int i = 0; i < Arches; i++) lowest[i] = 1f;
            int fewest = Arches;
            for (float time = 0f; time < winding.RoundSeconds * 2f; time += 0.05f)
            {
                int standing = 0;
                for (int i = 0; i < Arches; i++)
                {
                    float risen = winding.Risen(i, time);
                    Assert.That(risen, Is.InRange(0f, 1f));
                    lowest[i] = Mathf.Min(lowest[i], risen);
                    highest[i] = Mathf.Max(highest[i], risen);
                    if (risen >= 0.5f) standing++;
                }
                fewest = Mathf.Min(fewest, standing);
            }
            for (int i = 0; i < Arches; i++)
            {
                Assert.AreEqual(0f, lowest[i], 0.0001f, $"arch {i} never goes wholly under");
                Assert.AreEqual(1f, highest[i], 0.0001f, $"arch {i} never comes wholly up");
            }
            Assert.GreaterOrEqual(fewest, Arches / 2 - 1, "the body is seen all round the fight, not one arch at a time");
        }

        [Test]
        public void AnArchSlidesForwardWhileItStandsAndBeginsAgainOnlyUnderTheCloud()
        {
            var winding = new SerpentWinding();
            float before = winding.Slide(0, 0f);
            for (float time = 0.05f; time < winding.RoundSeconds * 3f; time += 0.05f)
            {
                float now = winding.Slide(0, time);
                // It only ever goes back to where it began while it is wholly under.
                if (now < before - 0.0001f) Assert.AreEqual(0f, winding.Risen(0, time), 0.02f, $"it jumped back in sight at {time:0.00}");
                Assert.That(now, Is.InRange(-0.5f, 0.5f));
                before = now;
            }
        }

        [Test]
        public void TheBeamsLaneEndsWhereTheFloorDoes()
        {
            const float Radius = 17f;
            var pivot = new Vector3(0f, 0f, 11f);
            // Due south from six metres inside the north rim, the floor runs on for the radius and eleven more.
            Assert.AreEqual(28f, StormDragon.Chord(pivot, StormDragon.Southward(0f), Radius), 0.001f);
            // Turned aside it is shorter, and the same either way.
            float east = StormDragon.Chord(pivot, StormDragon.Southward(60f), Radius);
            float west = StormDragon.Chord(pivot, StormDragon.Southward(-60f), Radius);
            Assert.Less(east, 28f);
            Assert.AreEqual(east, west, 0.001f);
            // Wherever it ends, it ends on the rim.
            Vector3 end = pivot + StormDragon.Southward(60f) * east;
            Assert.AreEqual(Radius, new Vector2(end.x, end.z).magnitude, 0.001f);
            Assert.Greater(StormDragon.Southward(60f).x, 0f, "more degrees is further east");
        }
    }
}
