using System.Reflection;
using Clover.Core;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The hit stop gives time back. A menu that pauses during a stop and puts back, when it closes, the nearly-frozen scale
    /// it found must not leave the world at three per cent speed (found by BranchProbe, 2026-11-20: resting straight after
    /// a blow left every spell's cast taking thirty times as long).
    /// </summary>
    public class HitStopTests
    {
        private GameObject holder;
        private HitStop stop;
        private float scaleWas;

        private static readonly MethodInfo Tick = typeof(HitStop).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void Make()
        {
            scaleWas = Time.timeScale;
            Time.timeScale = 1f;
            holder = new GameObject("Hit Stop Test");
            stop = holder.AddComponent<HitStop>();
        }

        [TearDown]
        public void Clean()
        {
            Object.DestroyImmediate(holder);
            Time.timeScale = scaleWas;
        }

        [Test]
        public void AMenuThatRestoresTheFrozenScaleDoesNotLeaveTimeSlowed()
        {
            stop.Freeze(5f);
            Assert.AreEqual(stop.FrozenTimeScale, Time.timeScale, 1e-4f, "the blow nearly stops time");
            // The rest menu opens: it saves what it finds (the frozen scale) and pauses.
            float saved = Time.timeScale;
            Time.timeScale = 0f;
            Tick.Invoke(stop, null);
            Assert.AreEqual(0f, Time.timeScale, "a pause is left alone");
            // It closes and puts back what it saved.
            Time.timeScale = saved;
            Tick.Invoke(stop, null);
            Assert.AreEqual(1f, Time.timeScale, 1e-4f, "the stop's own frozen scale is not kept once the stop has been let go");
        }

        [Test]
        public void AScaleSomeoneElseSetIsLeftAlone()
        {
            stop.Freeze(5f);
            Time.timeScale = 0.5f;
            Tick.Invoke(stop, null);
            Tick.Invoke(stop, null);
            Assert.AreEqual(0.5f, Time.timeScale, 1e-4f, "a slow motion of someone else's is theirs");
        }

        [Test]
        public void ABlowDuringARestoredStopStillReturnsToOne()
        {
            stop.Freeze(5f);
            float saved = Time.timeScale;
            Time.timeScale = 0f;
            Tick.Invoke(stop, null);
            Time.timeScale = saved;
            // A new blow lands before the stop has reclaimed the scale (real time does not run inside an edit-mode test,
            // so the new stop is read while it holds: what it will give back when it ends).
            stop.Freeze(0.2f);
            Assert.AreEqual(stop.FrozenTimeScale, Time.timeScale, 1e-4f, "the new blow nearly stops time");
            Assert.AreEqual(1f, stop.Unbent, 1e-4f, "and will give back the scale from before the first stop, not the frozen one");
        }
    }
}
