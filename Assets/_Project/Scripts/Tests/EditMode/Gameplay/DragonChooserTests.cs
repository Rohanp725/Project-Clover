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
            // It shows everything but the bite, which is an answer, and the storm, which opens his fury.
            foreach (DragonAttack attack in System.Enum.GetValues(typeof(DragonAttack)))
                if (attack != DragonAttack.None && attack != DragonAttack.Bite && attack != DragonAttack.Storm) CollectionAssert.Contains(expected, attack);
            // Mouth, tail and magic take turns in it.
            for (int i = 1; i < expected.Length; i++)
                Assert.IsFalse(SameKind(expected[i - 1], expected[i]), $"{expected[i - 1]} and then {expected[i]} in the opening");
        }

        private static bool SameKind(DragonAttack one, DragonAttack other) =>
            (DragonChooser.IsMouth(one) && DragonChooser.IsMouth(other)) || (DragonChooser.IsTail(one) && DragonChooser.IsTail(other))
            || (DragonChooser.IsMagic(one) && DragonChooser.IsMagic(other));

        [Test]
        public void ThePlayerWhoLingersInReachIsBittenButNotTwiceRunning()
        {
            var chooser = PastTheOpening();
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
        public void MouthTailAndMagicTakeTurnsAndShareTheFight()
        {
            var chooser = PastTheOpening();
            var random = new System.Random(11);
            var last = chooser.Last;
            var beforeLast = DragonAttack.None;
            int mouth = 0, tail = 0, magic = 0;
            const int Turns = 4000;
            for (int i = 0; i < Turns; i++)
            {
                bool inReach = random.Next(3) == 0;
                var pick = chooser.Next((float)random.NextDouble(), false, inReach, random.Next(4) == 0);
                chooser.TakeLull();
                Assert.IsFalse(SameKind(last, pick), $"{pick} after {last} at {i}");
                Assert.AreNotEqual(beforeLast, pick, $"{pick}, {last} and {pick} again at {i}");
                beforeLast = last;
                if (DragonChooser.IsMouth(pick)) mouth++;
                else if (DragonChooser.IsTail(pick)) tail++;
                else magic++;
                last = pick;
            }
            Assert.Less(mouth, Turns / 2, "the mouth is not most of what he does");
            Assert.Greater(mouth, Turns / 4, "the mouth has its share");
            Assert.Greater(tail, Turns / 4, "the tail has its share");
            Assert.Greater(magic, Turns / 4, "the magic has its share");
        }

        [Test]
        public void EveryTailMoveComesInAnyStretchOfAnOrdinaryFight()
        {
            var chooser = PastTheOpening();
            var random = new System.Random(5);
            var history = new List<DragonAttack>();
            for (int i = 0; i < 3000; i++)
            {
                bool inReach = random.Next(3) == 0;
                history.Add(chooser.Next((float)random.NextDouble(), inReach && random.Next(2) == 0, inReach, random.Next(4) == 0));
                chooser.TakeLull();
            }
            // The tail comes at least every fourth turn and its six moves are dealt round, so thirty turns hold them all.
            const int Stretch = 30;
            for (int from = 0; from + Stretch <= history.Count; from += 7)
            {
                var stretch = history.GetRange(from, Stretch);
                foreach (DragonAttack attack in System.Enum.GetValues(typeof(DragonAttack)))
                    if (DragonChooser.IsTail(attack)) CollectionAssert.Contains(stretch, attack, $"no {attack} in the thirty turns from {from}");
            }
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
            // The opening: beam, tail slam, bolts, tail sweep.
            chooser.Next(0f, false, false, false);
            Assert.IsFalse(chooser.LullOwed, "one heavy attack is not two");
            chooser.Next(0f, false, false, false);
            chooser.Next(0f, false, false, false);
            Assert.IsFalse(chooser.LullOwed, "the slam and the bolts are not heavy");
            chooser.Next(0f, false, false, false);
            Assert.IsTrue(chooser.LullOwed, "the beam and the slither make two");
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
            Assert.IsFalse(DragonChooser.IsMagic(chooser.Next(0.99f, false, false, false)), "after the storm that opens his fury, not more magic");
        }

        [Test]
        public void TheCoilRunsStraightFromTheRimAndThenRoundItsRingDrawingInward()
        {
            var entry = new Vector3(17f, 0f, 0f);
            var centre = new Vector3(6f, 0f, 0f);
            const float Radius = 5f;
            float line = TailShapes.CoilLine(entry, centre, Radius);
            Assert.AreEqual(Mathf.Sqrt(11f * 11f - 25f), line, 0.001f);
            Assert.AreEqual(0f, (TailShapes.Coil(entry, centre, Radius, 1f, 0f) - entry).magnitude, 0.001f, "it begins at the rim");
            // Where the straight meets the ring it touches it: on the ring, and square to the spoke.
            Vector3 touch = TailShapes.Coil(entry, centre, Radius, 1f, line);
            Assert.AreEqual(Radius, (touch - centre).magnitude, 0.01f);
            Assert.AreEqual(0f, Vector3.Dot((touch - centre).normalized, (touch - entry).normalized), 0.01f);
            // No joint has to turn a corner there, and round the ring it never leaves it outward.
            Vector3 before = TailShapes.Coil(entry, centre, Radius, 1f, line - 0.5f), after = TailShapes.Coil(entry, centre, Radius, 1f, line + 0.5f);
            Assert.Less(Vector3.Angle(touch - before, after - touch), 10f);
            for (float metres = line; metres < line + 40f; metres += 0.5f)
            {
                float away = (TailShapes.Coil(entry, centre, Radius, 1f, metres) - centre).magnitude;
                Assert.That(away, Is.InRange(Radius * 0.4f - 0.001f, Radius + 0.001f), $"at {metres:0.0} m");
            }
            // The other way round is its mirror.
            Vector3 one = TailShapes.Coil(entry, centre, Radius, 1f, line + 3f), other = TailShapes.Coil(entry, centre, Radius, -1f, line + 3f);
            Assert.AreEqual(one.x, other.x, 0.001f);
            Assert.AreEqual(one.z, -other.z, 0.001f);
        }

        [Test]
        public void TheScytheCrossesWaitsAndComesBackFaster()
        {
            Assert.AreEqual(-70f, TailShapes.Sweep(-1f, -70f, 70f, 1.3f, 0.5f, 0.8f), 0.001f, "before it begins it lies where it starts");
            Assert.AreEqual(0f, TailShapes.Sweep(0.65f, -70f, 70f, 1.3f, 0.5f, 0.8f), 0.001f, "half way across at half time");
            Assert.AreEqual(70f, TailShapes.Sweep(1.5f, -70f, 70f, 1.3f, 0.5f, 0.8f), 0.001f, "it waits at the far side");
            Assert.AreEqual(0f, TailShapes.Sweep(2.2f, -70f, 70f, 1.3f, 0.5f, 0.8f), 0.001f, "and is half way back sooner");
            Assert.AreEqual(-70f, TailShapes.Sweep(9f, -70f, 70f, 1.3f, 0.5f, 0.8f), 0.001f, "it ends where it began");
            float last = -70f;
            for (float seconds = 0f; seconds < 1.3f; seconds += 0.02f)
            {
                float now = TailShapes.Sweep(seconds, -70f, 70f, 1.3f, 0.5f, 0.8f);
                Assert.GreaterOrEqual(now, last - 0.0001f, "it never turns back in a pass");
                last = now;
            }
        }

        [Test]
        public void TheStingArchesHighFromTheRimAndEndsOverWhatItStabs()
        {
            var entry = new Vector3(0f, 0f, -17f);
            var joint = new Vector3(0f, 0f, -7f);
            Assert.AreEqual(0f, TailShapes.Sting(entry, joint, 6f, 5f, 0f).y, 0.001f, "it leaves the rim at the floor");
            Vector3 end = TailShapes.Sting(entry, joint, 6f, 5f, 10f);
            Assert.AreEqual(0f, (new Vector3(end.x, 0f, end.z) - joint).magnitude, 0.001f, "its last joint is over the place");
            Assert.AreEqual(5f, end.y, 0.01f, "as high as it is held");
            Assert.AreEqual(2f, TailShapes.Sting(entry, joint, 6f, 2f, 10f).y, 0.01f, "and down when it stabs");
            float highest = 0f;
            for (float metres = 0f; metres <= 10f; metres += 0.25f) highest = Mathf.Max(highest, TailShapes.Sting(entry, joint, 6f, 5f, metres).y);
            Assert.Greater(highest, 6f, "the arch stands higher than a player");
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
