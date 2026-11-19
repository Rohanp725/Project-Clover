using System.Collections;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Clover.Tests
{
    /// <summary>
    /// The reactions' look pool on a hand-made set of two looks (a solid on the body, a glow on the ground, a mark and a
    /// lamp each) and a pool smaller than two looks: the second look ends the first to take its renderers, the counts
    /// never drift, and once both have run their time every renderer and lamp is back.
    /// </summary>
    public class ReactionArtPoolTests
    {
        private GameObject holder;
        private ReactionArtSet set;
        private Mesh mesh;

        [TearDown]
        public void TearDown()
        {
            if (holder != null) Object.Destroy(holder);
            if (set != null) Object.Destroy(set);
            if (mesh != null) Object.Destroy(mesh);
        }

        private static ReactionArtSet.Part Part(byte kind, byte place, Mesh shape) =>
            new ReactionArtSet.Part { Name = "test", Kind = kind, Place = place, Mesh = shape, Size = new Vector3(1f, 0f, 1f), Triangles = 1, Pieces = 1 };

        [UnityTest]
        public IEnumerator MoreLooksThanThePoolHoldsEndTheOldestAndAllComeBack()
        {
            mesh = new Mesh { vertices = new[] { Vector3.zero, Vector3.up, Vector3.right }, triangles = new[] { 0, 1, 2 } };
            set = ScriptableObject.CreateInstance<ReactionArtSet>();
            set.Parts = new[]
            {
                Part(ReactionArtSet.Solid, ReactionArtSet.OnBody, mesh), Part(ReactionArtSet.Glow, ReactionArtSet.OnGround, mesh), Part(ReactionArtSet.Decal, ReactionArtSet.OnGround, null),
                Part(ReactionArtSet.Solid, ReactionArtSet.OnBody, mesh), Part(ReactionArtSet.Glow, ReactionArtSet.OnGround, mesh), Part(ReactionArtSet.Decal, ReactionArtSet.OnGround, null),
            };
            set.Lamps = new[]
            {
                new ReactionArtSet.Lamp { Position = Vector3.up, Colour = Color.white, Intensity = 5f, Range = 2f },
                new ReactionArtSet.Lamp { Position = Vector3.up, Colour = Color.white, Intensity = 5f, Range = 2f },
            };
            set.Looks = new[]
            {
                new ReactionArtSet.Look { Pair = 0, Key = "test_a", Name = "A", FirstPart = 0, PartCount = 3, FirstLamp = 0, LampCount = 1 },
                new ReactionArtSet.Look { Pair = 1, Key = "test_b", Name = "B", FirstPart = 3, PartCount = 3, FirstLamp = 1, LampCount = 1, Directed = true },
            };

            holder = new GameObject("Reaction Art Pool Test");
            var art = holder.AddComponent<ReactionArt>();
            art.PartCount = 4;
            art.LampCount = 1;
            art.MostLooks = 2;
            art.BodySeconds = 0.15f;
            art.GroundSeconds = 0.6f;
            art.MarkSeconds = 0.8f;
            art.LampSeconds = 0.15f;
            art.Configure(set);
            Assert.IsTrue(ReactionArt.Ready);
            Assert.IsTrue(ReactionArt.IsDesigned(0) && ReactionArt.IsDesigned(1) && !ReactionArt.IsDesigned(2));

            int ended = ReactionArt.Ended, played = ReactionArt.Played;
            Assert.IsTrue(ReactionArt.Play(0, Vector3.zero, null, Vector3.zero));
            Assert.AreEqual(3, ReactionArt.PartsShown);
            Assert.AreEqual(1, ReactionArt.LampsShown);

            // Four renderers for six parts: the second look takes the one free and ends the first for the rest.
            var body = new GameObject("Test Body").transform;
            body.SetParent(holder.transform, false);
            Assert.IsTrue(ReactionArt.Play(1, Vector3.right, body, Vector3.forward));
            Assert.AreEqual(3, ReactionArt.PartsShown);
            Assert.AreEqual(1, ReactionArt.LampsShown);
            Assert.AreEqual(ended + 1, ReactionArt.Ended, "the first look gave way");
            Assert.AreEqual(played + 2, ReactionArt.Played);
            Assert.IsFalse(ReactionArt.Play(2, Vector3.zero, null, Vector3.zero), "a pair with no look here is left to be composed");

            yield return new WaitForSeconds(0.35f);
            // The body's solid and the lamp have run their time; the ground's glow and mark are still there.
            Assert.AreEqual(2, ReactionArt.PartsShown);
            Assert.AreEqual(0, ReactionArt.LampsShown);
            yield return new WaitForSeconds(0.7f);
            Assert.AreEqual(0, ReactionArt.PartsShown);
            Assert.AreEqual(0, ReactionArt.LampsShown);
            Assert.AreEqual(ended + 1, ReactionArt.Ended, "a look that ran its time is not counted as cut short");
        }

        // A look of two parts with cues: one falls from above as the look starts, one waits its turn and then rises.
        [UnityTest]
        public IEnumerator PartsMoveByTheirCuesAndWaitTheirTurn()
        {
            mesh = new Mesh { vertices = new[] { Vector3.zero, Vector3.up, Vector3.right }, triangles = new[] { 0, 1, 2 } };
            set = ScriptableObject.CreateInstance<ReactionArtSet>();
            var falls = Part(ReactionArtSet.Solid, ReactionArtSet.OnGround, mesh);
            falls.Motion = ReactionMotion.Cue.Of(ReactionMotion.Fall, ReactionMotion.Arrive, 0f, 0.4f, 0.8f, 4f);
            var rises = Part(ReactionArtSet.Solid, ReactionArtSet.OnGround, mesh);
            rises.Motion = ReactionMotion.Cue.Of(ReactionMotion.Rise, ReactionMotion.Impact, 0.3f, 0.1f, 0.4f);
            set.Parts = new[] { falls, rises };
            set.Looks = new[] { new ReactionArtSet.Look { Pair = 0, Key = "test_a", Name = "A", FirstPart = 0, PartCount = 2, ImpactAt = 0.4f, Seconds = 0.8f } };

            holder = new GameObject("Reaction Art Motion Test");
            var art = holder.AddComponent<ReactionArt>();
            art.MostLooks = 2;
            art.Configure(set);
            var shown = new Matrix4x4[16];
            Assert.IsTrue(ReactionArt.Play(0, Vector3.zero, null, Vector3.forward));
            Assert.AreEqual(2, ReactionArt.PartsShown, "both parts hold a renderer from the start");
            ReactionArt.ShownMatrices(shown);
            float startHeight = shown[0].GetColumn(3).y;
            Assert.Greater(startHeight, 3f, "the falling part starts high above the feet");
            Assert.AreEqual(default(Matrix4x4), shown[1], "the rising part is not drawn before its turn");

            yield return new WaitForSeconds(0.2f);
            ReactionArt.ShownMatrices(shown);
            float midHeight = shown[0].GetColumn(3).y;
            Assert.Less(midHeight, startHeight - 0.2f, "it has come down");
            Assert.Greater(midHeight, 0.01f, "but not landed yet");

            yield return new WaitForSeconds(0.3f);
            ReactionArt.ShownMatrices(shown);
            Assert.AreEqual(0f, shown[0].GetColumn(3).y, 0.001f, "landed on its place");
            Assert.AreNotEqual(default(Matrix4x4), shown[1], "the second part is drawn once its turn has come");

            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(0, ReactionArt.PartsShown, "every renderer is back once both have run their time");
        }
    }
}
