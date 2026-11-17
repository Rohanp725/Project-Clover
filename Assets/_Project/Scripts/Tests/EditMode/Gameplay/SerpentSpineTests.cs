using Clover.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    public class SerpentSpineTests
    {
        private const int Count = 24;
        private const float Spacing = 1.1f;

        // A point on a round of ten metres, so many metres along it.
        private static Vector3 OnRound(float metres) => new Vector3(Mathf.Sin(metres / 10f), 0f, Mathf.Cos(metres / 10f)) * 10f;

        private static SerpentSpine LedRoundACurve(float metres)
        {
            var spine = new SerpentSpine(Count, Spacing);
            spine.Seed(OnRound(0f), Vector3.left);
            for (float at = 0.1f; at <= metres; at += 0.1f) spine.Lead(OnRound(at));
            spine.PlaceAlongTrail();
            return spine;
        }

        [Test]
        public void TheLengthsBetweenThePointsHoldHoweverItIsMoved()
        {
            var spine = LedRoundACurve(40f);
            spine.Snap();
            for (int frame = 0; frame < 240; frame++)
            {
                // Thrown about: led on, waved, and every so often lifted bodily.
                spine.Lead(OnRound(40f + frame * 0.3f));
                spine.PlaceAlongTrail();
                spine.Wave(frame / 60f, 2.5f, 8f, 9f);
                if (frame % 40 < 20)
                    for (int i = 0; i < spine.Count; i++) spine.Targets[i] += Vector3.up * (i * 0.3f);
                spine.Step(1f / 60f);
                for (int i = 1; i < spine.Count; i++)
                    Assert.AreEqual(Spacing, Vector3.Distance(spine.Points[i - 1], spine.Points[i]), 0.001f, $"frame {frame} between {i - 1} and {i}");
            }
        }

        [Test]
        public void EveryPartOfTheBodyLiesWhereTheTipHasBeen()
        {
            var spine = LedRoundACurve(40f);
            for (int i = 0; i < spine.Count; i++)
                Assert.AreEqual(10f, spine.Targets[i].magnitude, 0.02f, $"point {i} has left the path the tip took");
            Assert.AreEqual(0f, Vector3.Distance(spine.Targets[spine.Count - 1], spine.LeadPoint), 0.0001f, "the tip is the lead");
            // And each lies its spacing further back along that path than the one ahead of it.
            for (int i = 1; i < spine.Count; i++)
                Assert.AreEqual(Spacing, Vector3.Distance(spine.Targets[i - 1], spine.Targets[i]), 0.02f);
        }

        [Test]
        public void DrawnBackItGoesOutTheWayItCameIn()
        {
            var spine = LedRoundACurve(40f);
            spine.Retreat(15f);
            spine.PlaceAlongTrail();
            Assert.AreEqual(0f, Vector3.Distance(OnRound(25f), spine.LeadPoint), 0.15f, "the tip is back where it was fifteen metres ago");
            for (int i = 0; i < spine.Count; i++) Assert.AreEqual(10f, spine.Targets[i].magnitude, 0.02f, $"point {i} left the path going out");
            // Drawn back further than it ever came, it lies straight, the way it was first laid.
            spine.Retreat(100f);
            spine.PlaceAlongTrail();
            for (int i = 1; i < spine.Count; i++)
                Assert.AreEqual(0f, Vector3.Angle(Vector3.left, spine.Targets[i - 1] - spine.Targets[i]), 0.5f);
        }

        [Test]
        public void TheWaveIsNothingAtTheRootAndLargestTowardTheTip()
        {
            var spine = new SerpentSpine(Count, Spacing);
            spine.Seed(Vector3.forward * 30f, Vector3.back);
            float nearRoot = 0f, nearTip = 0f;
            for (float time = 0f; time < 2f; time += 0.02f)
            {
                spine.PlaceAlongTrail();
                spine.Wave(time, 2f, 8f, 9f);
                Assert.AreEqual(0f, spine.Targets[0].x, 0.0001f, "the root does not move");
                nearRoot = Mathf.Max(nearRoot, Mathf.Abs(spine.Targets[3].x));
                nearTip = Mathf.Max(nearTip, Mathf.Abs(spine.Targets[Count - 1].x));
            }
            Assert.Greater(nearTip, nearRoot * 4f);
            Assert.AreEqual(2f, nearTip, 0.1f);
        }

        [Test]
        public void TheTipLagsBehindAQuickMoveAndThenGoesPastIt()
        {
            var spine = new SerpentSpine(Count, Spacing) { HoldLengths = false };
            spine.Seed(Vector3.forward * 30f, Vector3.back);
            // The whole body is asked, all at once, to be a metre to one side.
            float tipAtFirst = 0f, nearRootAtFirst = 0f, tipFurthest = 0f;
            for (int frame = 0; frame < 180; frame++)
            {
                spine.PlaceAlongTrail();
                for (int i = 0; i < spine.Count; i++) spine.Targets[i] += Vector3.right;
                spine.Step(1f / 60f);
                if (frame == 5)
                {
                    tipAtFirst = spine.Points[Count - 1].x;
                    nearRootAtFirst = spine.Points[2].x;
                }
                tipFurthest = Mathf.Max(tipFurthest, spine.Points[Count - 1].x);
            }
            Assert.Less(tipAtFirst, nearRootAtFirst * 0.6f, "a tenth of a second on, the tip has gone much less far than the body near the root");
            Assert.Greater(tipFurthest, 1.05f, "it overshoots");
            Assert.AreEqual(1f, spine.Points[Count - 1].x, 0.02f, "and settles where it was asked to be");
        }
    }
}
