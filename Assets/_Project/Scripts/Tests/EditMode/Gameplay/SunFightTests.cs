using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The Sun-Bearer's fight as reworked on 2026-11-18 (fire_land.md 6.4): the poise after a fall that ends the endless
    /// stagger, the hot floor's timings and the dodge roll through it, the loose sun that pays nothing and rolls into the
    /// shell, what the chooser does about repeats and pairs, and the check that fire never shuts the player off from the shape.
    /// </summary>
    public class SunFightTests
    {
        private readonly List<GameObject> made = new List<GameObject>();

        [TearDown]
        public void Clear()
        {
            foreach (var thing in made) Object.DestroyImmediate(thing);
            made.Clear();
        }

        // ---------------------------------------------------------------- the poise after a fall

        [Test]
        public void OnceUpAgainItsBalanceCannotBreakForTwelveSecondsThoughTheMeterFills()
        {
            var meter = new StaggerMeter { Maximum = 160f };
            var poise = new ShapePoise { Seconds = 12f };
            meter.Reset();
            poise.Recovered();
            Assert.IsTrue(poise.Holds);
            Assert.IsFalse(poise.Add(meter, 999f), "a heavy blow the moment it is up does not throw it down again");
            Assert.IsFalse(meter.IsStaggered);
            Assert.AreEqual(159f, meter.Value, 0.001f, "the meter fills to one short of breaking");
            for (int i = 0; i < 119; i++) poise.Tick(0.1f);
            Assert.IsTrue(poise.Holds, "still poised at 11.9 s");
            Assert.IsFalse(poise.Add(meter, 50f));
            poise.Tick(0.2f);
            Assert.IsFalse(poise.Holds);
            Assert.IsTrue(poise.Add(meter, 5f), "after twelve seconds what was owed breaks it");
        }

        [Test]
        public void GoingBackForTheSunItHoldsFullPoiseAndLyingItDoesNot()
        {
            var meter = new StaggerMeter { Maximum = 160f };
            var poise = new ShapePoise();
            meter.Reset();
            poise.Reset();
            Assert.IsFalse(poise.Holds, "lying exposed, its balance can be broken");
            poise.Returning = true;
            Assert.IsTrue(poise.Holds);
            Assert.IsFalse(poise.Add(meter, 400f));
            Assert.IsFalse(meter.IsStaggered);
            poise.Recovered();
            Assert.IsFalse(poise.Returning, "taking the sun up ends the return and starts the twelve seconds");
            Assert.AreEqual(12f, poise.Left, 0.001f);
        }

        // ---------------------------------------------------------------- hot floor

        // Seconds walked or rolled across a lane of this width at this pace, and the bites it cost.
        private static int Cross(HotFloorClock clock, float width, float pace, bool rolling, float stayAfter = 0f)
        {
            clock.Reset();
            int before = clock.Bites;
            const float Step = 1f / 60f;
            for (float t = 0f; t < width / pace; t += Step) clock.Tick(Step, true, rolling);
            for (float t = 0f; t < stayAfter; t += Step) clock.Tick(Step, true, false);
            clock.Tick(Step, false, false);
            return clock.Bites - before;
        }

        [Test]
        public void WalkingStraightAcrossALaneCostsOneBiteAtMostAndARollNone()
        {
            var clock = new HotFloorClock { FirstBite = 0.5f, Every = 0.5f };
            Assert.AreEqual(1, Cross(clock, 4f, 6f, false), "a roll's lane, 4 m, at a walk of 6 m/s");
            Assert.AreEqual(1, Cross(clock, 5f, 6f, false), "a pass's lane, 5 m");
            Assert.AreEqual(0, Cross(clock, 4f, 4.5f / 0.35f, true), "a dodge roll across it takes no burn");
            Assert.AreEqual(0, Cross(clock, 2.9f, 6f, false), "nothing in the first half second");
        }

        [Test]
        public void StandingInFireBitesEveryHalfSecondAndARollStartsTheCountAgain()
        {
            var clock = new HotFloorClock { FirstBite = 0.5f, Every = 0.5f };
            clock.Reset();
            int bites = 0;
            const float Step = 1f / 60f;
            float firstAt = -1f;
            for (float t = 0f; t < 2.01f; t += Step)
                if (clock.Tick(Step, true, false))
                {
                    bites++;
                    if (firstAt < 0f) firstAt = t;
                }
            Assert.AreEqual(4, bites, "0.5, 1.0, 1.5 and 2.0 s");
            Assert.AreEqual(0.5f, firstAt, 0.03f);
            // Rolled into the middle of it and left standing there: the first bite comes half a second after the roll ends.
            clock.Reset();
            for (float t = 0f; t < 0.35f; t += Step) Assert.IsFalse(clock.Tick(Step, true, true));
            float waited = 0f;
            while (!clock.Tick(Step, true, false) && waited < 2f) waited += Step;
            Assert.AreEqual(0.5f, waited, 0.03f);
        }

        // ---------------------------------------------------------------- the loose sun

        [Test]
        public void TheLooseSunPaysNothingBackForABlow()
        {
            var thing = new GameObject("sun reach");
            made.Add(thing);
            var part = thing.AddComponent<ColossusPart>();
            Assert.IsTrue(PlayerCombat.PaysBack(part), "an ordinary piece of a god pays as any enemy does");
            part.Inert = true;
            Assert.IsTrue(part.PaysNothing);
            Assert.IsFalse(PlayerCombat.PaysBack(part), "the loose sun earns no mana, no flask and no grey health");
        }

        [Test]
        public void AHeavyBlowRollsTheSunHardIntoTheShapeAndALightOneOnlyNudgesIt()
        {
            // The scarab lies across the way, its middle twelve metres off; the sun is struck at it.
            var tail = new Vector3(2f, 0f, -12f);
            var head = new Vector3(2f, 0f, -6.8f);
            var sun = new RollingSun { Position = new Vector3(-10f, 0f, -9f), RimAt = 18.1f };
            sun.Strike(Vector3.right, 17f, true);
            Assert.IsTrue(sun.Hard);
            bool struck = false;
            for (int i = 0; i < 300 && sun.Moving && !struck; i++)
            {
                sun.Step(1f / 60f);
                struck = sun.Hard && RollingSun.Meets(sun.Position, sun.Radius, tail, head, 1.9f);
            }
            Assert.IsTrue(struck, "rolled hard into the body, it sets the shell alight");
            Assert.Greater(sun.Reach(17f), 14f, "a heavy blow sends it a long way");

            var nudged = new RollingSun { Position = new Vector3(-10f, 0f, -9f) };
            nudged.Strike(Vector3.right, 5f, false);
            Assert.IsFalse(nudged.Hard);
            float went = 0f;
            for (int i = 0; i < 300 && nudged.Moving; i++) went += nudged.Step(1f / 60f);
            Assert.Less(went, 2f, "a light blow only nudges it");
        }

        [Test]
        public void TheRolledSunBouncesOffTheWallAndTheStoneAndNeverPassesThem()
        {
            var sun = new RollingSun { Position = new Vector3(0f, 0f, -14f), RimAt = 18.1f, StoneAt = 3.4f };
            sun.Strike(new Vector3(0.2f, 0f, -1f), 30f, true);
            float farthest = 0f, nearest = 99f;
            for (int i = 0; i < 600 && sun.Moving; i++)
            {
                sun.Step(1f / 60f);
                float out_ = new Vector2(sun.Position.x, sun.Position.z).magnitude;
                farthest = Mathf.Max(farthest, out_);
                nearest = Mathf.Min(nearest, out_);
            }
            Assert.GreaterOrEqual(sun.Bounces, 1);
            Assert.LessOrEqual(farthest, 18.1f + 0.001f);
            Assert.GreaterOrEqual(nearest, 3.4f - 0.001f);
        }

        // ---------------------------------------------------------------- what it chooses

        private static SunBearerChooser AfterTheOpening()
        {
            var chooser = new SunBearerChooser();
            chooser.Reset();
            for (int i = 0; i < chooser.Opening.Length; i++) chooser.Record(chooser.Opening[i]);
            return chooser;
        }

        [Test]
        public void TheOpeningShowsEveryAttackOnceIncludingTheNewOnes()
        {
            var chooser = new SunBearerChooser();
            foreach (var attack in new[] { SunBearerAttack.Ricochet, SunBearerAttack.Burrow, SunBearerAttack.Flares, SunBearerAttack.Gust, SunBearerAttack.Grab, SunBearerAttack.Sunfall })
                CollectionAssert.Contains(chooser.Opening, attack);
            Assert.AreEqual(SunBearerAttack.Zenith, chooser.Opening.Last());
        }

        [Test]
        public void AFewAttacksLeadIntoTheirPartnerAndInTheFuryAlways()
        {
            var chooser = AfterTheOpening();
            // The scarab's: after the Ricochet, the Ram; after the Burrow, the Flares. The chance is read from the roll's far end.
            chooser.Record(SunBearerAttack.Setting);
            chooser.Record(SunBearerAttack.Ricochet);
            Assert.AreEqual(SunBearerAttack.Ram, chooser.Next(0.9f, false, false, 0, 100f));
            Assert.IsTrue(chooser.LastWasChained);
            chooser.Record(SunBearerAttack.Rising);
            chooser.Record(SunBearerAttack.Setting);
            chooser.Record(SunBearerAttack.Burrow);
            Assert.AreEqual(SunBearerAttack.Flares, chooser.Next(0.95f, false, false, 0, 100f));
            // The falcon's: after the Gust, the Stoop.
            chooser.Record(SunBearerAttack.Rising);
            chooser.Record(SunBearerAttack.Gust);
            Assert.AreEqual(SunBearerAttack.Stoop, chooser.Next(0.99f, true, false, 0, 100f));
            // Not always outside the fury; always in it, with pairs of its own.
            chooser.Record(SunBearerAttack.Setting);
            chooser.Record(SunBearerAttack.Ricochet);
            Assert.AreNotEqual(SunBearerAttack.Ram, chooser.Next(0.05f, false, false, 0, 100f), "a low roll: no pair");
            chooser.Fury = true;
            chooser.Record(SunBearerAttack.Rising);
            chooser.Record(SunBearerAttack.Setting);
            chooser.Record(SunBearerAttack.Sparks);
            Assert.AreEqual(SunBearerAttack.Ricochet, chooser.Next(0.05f, false, false, 0, 100f), "the fury's own pair, whatever the roll");
        }

        [Test]
        public void WhatItDidLatelyIsLessLikelyToComeRoundAgain()
        {
            // Over every roll, how often the Roll is chosen fresh, and how often with the Roll two picks back.
            int Count(bool lately)
            {
                int rolls = 0;
                for (int i = 0; i < 1000; i++)
                {
                    var chooser = AfterTheOpening();
                    // Four of the falcon's first, so the opening's own Roll is out of what it remembers.
                    chooser.Record(SunBearerAttack.Stoop);
                    chooser.Record(SunBearerAttack.Pass);
                    chooser.Record(SunBearerAttack.Feathers);
                    chooser.Record(SunBearerAttack.Gust);
                    chooser.Record(SunBearerAttack.Setting);
                    if (lately) chooser.Record(SunBearerAttack.Roll);
                    chooser.Record(SunBearerAttack.Sparks);
                    if (chooser.Next((i + 0.5f) / 1000f, false, false, 0, 100f) == SunBearerAttack.Roll) rolls++;
                }
                return rolls;
            }
            int fresh = Count(false), soon = Count(true);
            Assert.Greater(fresh, 100);
            Assert.Less(soon, fresh * 0.45f, $"the Roll two picks back is chosen {soon} times in 1000 against {fresh} fresh");
        }

        [Test]
        public void OverAWholeFightNothingComesTwiceRunningAndEveryAttackIsSeen()
        {
            var chooser = AfterTheOpening();
            var random = new System.Random(11);
            bool falcon = false;
            float sinceZenith = 0f;
            var last = chooser.Last;
            var seen = new Dictionary<SunBearerAttack, int>();
            int backToBackPairs = 0;
            var before = SunBearerAttack.None;
            for (int i = 0; i < 600; i++)
            {
                chooser.Fury = i > 300;
                var pick = chooser.Next((float)random.NextDouble(), falcon, false, 0, sinceZenith);
                Assert.AreNotEqual(last, pick, $"turn {i}");
                if (!SunBearerChooser.IsChange(pick) && pick == before) backToBackPairs++;
                seen[pick] = seen.TryGetValue(pick, out int n) ? n + 1 : 1;
                sinceZenith = pick == SunBearerAttack.Zenith ? 0f : sinceZenith + 6f;
                if (SunBearerChooser.IsChange(pick)) falcon = pick == SunBearerAttack.Rising;
                before = last;
                last = pick;
            }
            foreach (var attack in new[] { SunBearerAttack.Roll, SunBearerAttack.Furrows, SunBearerAttack.Sparks, SunBearerAttack.Ricochet, SunBearerAttack.Burrow, SunBearerAttack.Flares,
                SunBearerAttack.Stoop, SunBearerAttack.Pass, SunBearerAttack.Feathers, SunBearerAttack.Gust, SunBearerAttack.Grab, SunBearerAttack.Sunfall, SunBearerAttack.Zenith })
                Assert.IsTrue(seen.ContainsKey(attack), $"{attack} is never chosen");
            Assert.Less(backToBackPairs, 75, "an attack two picks after itself is rare");
        }

        // ---------------------------------------------------------------- fire that leaves a way through

        [Test]
        public void ALaneWallToWallShutsThePlayerOffAndOneWithAGapDoesNot()
        {
            var reach = new CourtReach();
            var player = new Vector3(3f, 0f, -12f);
            var shape = new Vector3(-3f, 0f, 12f);
            reach.Clear();
            Assert.IsTrue(reach.Reaches(player, shape), "an empty court");
            // Across the whole court, wall to wall, through the stone.
            reach.Lane(new Vector3(-20f, 0f, -1f), Vector3.right, 40f, 4f);
            Assert.IsFalse(reach.Reaches(player, shape), "no way round a lane from wall to wall");
            reach.Clear();
            reach.Lane(new Vector3(-20f, 0f, -1f), Vector3.right, 30f, 4f);
            Assert.IsTrue(reach.Reaches(player, shape), "a gap at the east wall lets them round it");
        }

        [Test]
        public void FireUnderThePlayerCanBeSteppedOffButNotWalkedThrough()
        {
            var reach = new CourtReach();
            var player = new Vector3(0f, 0f, -10f);
            reach.Clear();
            // They stand in a lane running past them; the way to the shape is off its side.
            reach.Lane(new Vector3(-12f, 0f, -10f), Vector3.right, 24f, 4f);
            Assert.IsTrue(reach.Reaches(player, new Vector3(0f, 0f, -16f)));
            // Two lanes boxing them in, one across each way they could go: the far one blocks.
            reach.Lane(new Vector3(-20f, 0f, -4f), Vector3.right, 40f, 4f);
            Assert.IsFalse(reach.Reaches(player, new Vector3(0f, 0f, 8f)));
            Assert.IsTrue(reach.Reaches(player, new Vector3(0f, 0f, -16f)));
        }
    }
}
