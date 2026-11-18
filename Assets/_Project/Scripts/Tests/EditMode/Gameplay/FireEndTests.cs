using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The end of the Fire land's god as built (WORLD.md, "Every god transforms" and "Fire's land: changes"): the
    /// sunset that is the way out, the man lying across the obelisk's shadow, his one line, the camera that keeps
    /// both the player and what happens in the picture, and the currency called Shards.
    /// </summary>
    public class FireEndTests
    {
        [Test]
        public void TheSunDoesNotMoveUntilItIsBegun()
        {
            var sun = new Sunset();
            sun.Tick(5f);
            Assert.IsFalse(sun.Begun);
            Assert.AreEqual(0f, sun.Progress);
            Assert.IsFalse(sun.Done);
            Assert.AreEqual(0f, sun.GateOpen, "the west gate stays shut through the fight");
            Assert.AreEqual(1f, sun.ShadowStretch, 1e-4f);
        }

        [Test]
        public void TheSunSetsInTheWestSoTheShadowEndsLongAndPointingEast()
        {
            var sun = new Sunset { Seconds = 8f };
            sun.Begin(200f, 50f);
            Assert.AreEqual(200f, sun.ShadowAngle, 0.01f, "it starts from where the shadow stood");
            Assert.AreEqual(50f, sun.Pitch, 0.01f, "and from as high as the sun stood");
            float lastPitch = sun.Pitch, lastStretch = sun.ShadowStretch, lastAngle = sun.ShadowAngle;
            for (int i = 0; i < 82; i++)
            {
                sun.Tick(0.1f);
                Assert.LessOrEqual(sun.Pitch, lastPitch + 1e-4f, "it only ever goes down");
                Assert.GreaterOrEqual(sun.ShadowStretch, lastStretch - 1e-4f, "the shadow only ever lengthens");
                Assert.Less(Mathf.Abs(Mathf.DeltaAngle(lastAngle, sun.ShadowAngle)), 6f, "the shadow swings round, it does not jump");
                lastPitch = sun.Pitch;
                lastStretch = sun.ShadowStretch;
                lastAngle = sun.ShadowAngle;
            }
            Assert.IsTrue(sun.Done);
            Assert.AreEqual(90f, sun.ShadowAngle, 0.01f, "lit from the west, the shadow points east");
            Assert.AreEqual(sun.LowPitch, sun.Pitch, 0.01f);
            Assert.Less(sun.Pitch, 15f, "low on the horizon");
            Assert.AreEqual(sun.LongestShadow, sun.ShadowStretch, 0.01f);
            Assert.Greater(sun.LongestShadow, 2f, "the last shadow is long");
            Assert.AreEqual(1f, sun.GateOpen, 0.001f);
            Assert.AreEqual(1f, sun.Warmth, 0.001f);
        }

        [Test]
        public void TheWestGateOpensOnlyAsTheSunGetsLow()
        {
            var sun = new Sunset { Seconds = 10f, GateFrom = 0.6f };
            sun.Begin(0f, 78f);
            sun.Tick(5.9f);
            Assert.AreEqual(0f, sun.GateOpen, "shut while the sun is still high");
            sun.Tick(2f);
            Assert.Greater(sun.GateOpen, 0f);
            Assert.Less(sun.GateOpen, 1f);
            sun.Reset();
            Assert.IsFalse(sun.Begun);
            Assert.AreEqual(0f, sun.GateOpen);
        }

        [Test]
        public void TheShadowHandCanBePointedOutright()
        {
            var hand = new ShadowHand();
            hand.Reset();
            hand.StopAndTurnBack(2f);
            hand.PointTo(450f);
            Assert.AreEqual(90f, hand.Angle, 1e-4f);
            Assert.AreEqual(0f, hand.Stopped, "pointed, it is not left waiting");
            Assert.AreEqual(1f, hand.Direction.x, 1e-4f, "ninety degrees is east");
        }

        [Test]
        public void HeLiesAcrossTheShadowWithHisFeetTowardWhoeverWatches()
        {
            foreach (float angle in new[] { 0f, 37f, 90f, 181f, 300f })
            {
                Vector3 shadow = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                foreach (var watcher in new[] { Vector3.back, Vector3.right * 3f, new Vector3(-2f, 0f, 5f) })
                {
                    Quaternion lying = SunCourtDirector.LyingAcross(shadow, watcher, 35f);
                    Vector3 length = lying * Vector3.forward;
                    float fromLevel = Mathf.Abs(Mathf.Asin(Mathf.Clamp(length.z, -1f, 1f)) * Mathf.Rad2Deg);
                    Assert.LessOrEqual(fromLevel, 35.01f, "never seen end on by the camera, which looks north");
                    // Square to the shadow whenever that is near enough to lying east and west; else as near as it may be.
                    float squareFromLevel = Mathf.Abs(Mathf.Repeat(angle + 90f, 180f) - 90f);
                    if (squareFromLevel <= 35f) Assert.AreEqual(0f, Vector3.Dot(length, shadow), 1e-4f, $"square to a shadow at {angle}");
                    else Assert.AreEqual(35f, fromLevel, 0.05f, $"turned as far toward square as allowed, for a shadow at {angle}");
                    Assert.AreEqual(0f, length.y, 1e-4f, "lying flat");
                    Assert.GreaterOrEqual(Vector3.Dot(length, watcher), -1e-4f, "his feet toward whoever watches");
                }
            }
        }

        [Test]
        public void HisOneLineIsTheOwners()
        {
            var holder = new GameObject("court");
            try
            {
                var court = holder.AddComponent<SunCourtDirector>();
                Assert.AreEqual(new[] { "I promised them gold. I gave them fire." }, court.Regret, "WORLD.md: the regret lines stay as written");
                Assert.AreEqual("sunking_transform", court.RisingClip);
                Assert.AreEqual("sunking_fallen", court.FallenClip);
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void TheCameraHoldsBothThePlayerAndWhatHappensInThePicture()
        {
            var holder = new GameObject("lens");
            try
            {
                var lens = holder.AddComponent<Camera>();
                lens.fieldOfView = 40f;
                lens.aspect = 16f / 9f;
                const float Pitch = 47f, Distance = 12f;
                var pairs = new[]
                {
                    (new Vector3(0f, 0f, -6f), new Vector3(0f, 0f, 8f)),
                    (new Vector3(-12f, 0f, -10f), new Vector3(10f, 0f, 9f)),
                    (new Vector3(5f, 0f, 14f), new Vector3(-3f, 0f, -15f)),
                    (new Vector3(1f, 0f, 1f), new Vector3(2f, 0f, 1.5f)),
                };
                float lastNeed = 0f;
                foreach (var (first, second) in pairs)
                {
                    Vector3 look = TopDownCamera.Hold(first, second, Pitch, lens.fieldOfView, lens.aspect, Distance, 1.2f, 4f, out float multiplier);
                    Assert.GreaterOrEqual(multiplier, 1.2f, "never nearer than asked");
                    var rotation = Quaternion.Euler(Pitch, 0f, 0f);
                    holder.transform.SetPositionAndRotation(look - rotation * Vector3.forward * Distance * multiplier, rotation);
                    foreach (var point in new[] { first, second })
                    {
                        Vector3 seen = lens.WorldToViewportPoint(point);
                        Assert.IsTrue(seen.z > 0f && seen.x > 0f && seen.x < 1f && seen.y > 0f && seen.y < 1f, $"{point} is in the picture holding {first} and {second} (seen at {seen})");
                    }
                    lastNeed = multiplier;
                }
                // The court's own case: the player south-west, the man north with the spear lifted five metres over him.
                {
                    var watcher = new Vector3(-6f, 0f, -6f);
                    var man = new Vector3(0f, 0f, 8f);
                    Vector3 look = TopDownCamera.Hold(watcher, man, Pitch, 40f, 16f / 9f, Distance, 1.5f, 3f, out float multiplier, 5f);
                    var rotation = Quaternion.Euler(Pitch, 0f, 0f);
                    holder.transform.SetPositionAndRotation(look - rotation * Vector3.forward * Distance * multiplier, rotation);
                    foreach (var point in new[] { watcher, man, man + Vector3.up * 5f })
                    {
                        Vector3 seen = lens.WorldToViewportPoint(point);
                        Assert.IsTrue(seen.x > 0f && seen.x < 1f && seen.y > 0f && seen.y < 1f, $"{point} is in the picture (seen at {seen})");
                    }
                }
                TopDownCamera.Hold(Vector3.zero, new Vector3(0f, 0f, 4f), Pitch, 40f, 16f / 9f, Distance, 1f, 4f, out float near);
                TopDownCamera.Hold(Vector3.zero, new Vector3(0f, 0f, 24f), Pitch, 40f, 16f / 9f, Distance, 1f, 4f, out float far);
                Assert.Greater(far, near, "further apart, the camera stands further back");
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void TheSponsorCallsTheCurrencyShards()
        {
            var lines = SponsorLines.FirstWaking.Concat(SponsorLines.AfterAGod).Concat(SponsorLines.CameBackAlive).Concat(SponsorLines.NothingMore);
            for (int runs = 0; runs < 6; runs++)
                foreach (bool fell in new[] { false, true })
                    lines = lines.Concat(SponsorLines.For(true, runs, fell, 0));
            foreach (string line in lines)
                Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(line, @"\bembers?\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase), $"'{line}' calls the currency embers");
        }
    }
}
