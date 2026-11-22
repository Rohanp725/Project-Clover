using Clover.Player.Forms;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The anchors that carry the element looks from the clay stand-in to each real enemy (the owner, 2026-11-21: effects
    /// misplaced on smaller and bigger enemies): the heights map joint to joint, the size is held to 0.6x to 1.8x, the
    /// torso's reach is an ellipse, the soldier the looks were approved on keeps them unchanged, and the bodies measure
    /// small, medium and large in that order.
    /// </summary>
    public class BodyAnchorsTests
    {
        private static BodyAnchors.Anchors Body(float waist, float chest, float head, float crown) =>
            new BodyAnchors.Anchors { Waist = waist, Chest = chest, Head = head, Crown = crown, Width = 0.2f, Front = 0.2f, Back = 0.2f };

        [Test]
        public void EachStandInJointLandsOnTheBodysOwn()
        {
            var body = Body(0.8f, 1.3f, 1.6f, 1.8f);
            var standIn = BodyAnchors.StandIn;
            Assert.AreEqual(0f, BodyAnchors.HeightOn(body, 0f), 1e-5f, "feet to feet");
            Assert.AreEqual(body.Waist, BodyAnchors.HeightOn(body, standIn.Waist), 1e-4f);
            Assert.AreEqual(body.Chest, BodyAnchors.HeightOn(body, standIn.Chest), 1e-4f);
            Assert.AreEqual(body.Head, BodyAnchors.HeightOn(body, standIn.Head), 1e-4f);
            Assert.AreEqual(body.Crown, BodyAnchors.HeightOn(body, standIn.Crown), 1e-4f);
            // Straight between joints: half way from waist to chest on the stand-in is half way on the body.
            Assert.AreEqual(0.5f * (body.Waist + body.Chest), BodyAnchors.HeightOn(body, 0.5f * (standIn.Waist + standIn.Chest)), 1e-4f);
            // Over the crown it goes on by the body's size.
            Assert.AreEqual(body.Crown + 0.3f * BodyAnchors.SizeOf(body), BodyAnchors.HeightOn(body, standIn.Crown + 0.3f), 1e-4f);
        }

        [Test]
        public void HeightsNeverFoldBackOnThemselves()
        {
            var body = Body(0.9f, 1.35f, 1.65f, 1.84f);
            float last = float.MinValue;
            for (float y = -0.5f; y < 3.5f; y += 0.01f)
            {
                float on = BodyAnchors.HeightOn(body, y);
                Assert.Greater(on, last, $"at {y:0.00} on the stand-in");
                last = on;
            }
        }

        [Test]
        public void TheSoldierTheLooksWereApprovedOnKeepsThemAsTheyWere()
        {
            // Before the anchors every look on a castle soldier was grown by 2.0 m over 1.75 m: they still are.
            var soldier = BodyAnchors.Soldier;
            Assert.AreEqual(BodyAnchors.SoldierGrowth, BodyAnchors.SizeOf(soldier), 1e-4f);
            foreach (float y in new[] { 0.3f, 0.9f, 1.2f, 1.5f, 1.8f })
                Assert.AreEqual(y * BodyAnchors.SoldierGrowth, BodyAnchors.HeightOn(soldier, y), 1e-4f, $"at {y} m");
        }

        [Test]
        public void SizeIsHeldBetweenSixTenthsAndOneAndFourFifths()
        {
            var standIn = BodyAnchors.StandIn;
            Assert.AreEqual(1f, BodyAnchors.SizeOf(standIn), 1e-4f);
            Assert.AreEqual(BodyAnchors.LeastSize, BodyAnchors.SizeOf(BodyAnchors.Scaled(standIn, 0.3f)), 1e-4f);
            Assert.AreEqual(BodyAnchors.MostSize, BodyAnchors.SizeOf(BodyAnchors.Scaled(standIn, 3f)), 1e-4f);
            Assert.AreEqual(1.3f, BodyAnchors.SizeOf(BodyAnchors.Scaled(standIn, 1.3f)), 1e-4f);
            Assert.AreEqual(0.6f, BodyAnchors.LeastSize);
            Assert.AreEqual(1.8f, BodyAnchors.MostSize);
        }

        [Test]
        public void TheTorsoReachesAsAnEllipse()
        {
            Assert.AreEqual(0.3f, BodyAnchors.ReachToward(0.3f, 0.2f, 0.1f, Vector2.right), 1e-4f, "to the side");
            Assert.AreEqual(0.3f, BodyAnchors.ReachToward(0.3f, 0.2f, 0.1f, Vector2.left), 1e-4f);
            Assert.AreEqual(0.2f, BodyAnchors.ReachToward(0.3f, 0.2f, 0.1f, Vector2.up), 1e-4f, "ahead");
            Assert.AreEqual(0.1f, BodyAnchors.ReachToward(0.3f, 0.2f, 0.1f, Vector2.down), 1e-4f, "behind");
            float slant = BodyAnchors.ReachToward(0.3f, 0.2f, 0.1f, new Vector2(1f, 1f));
            Assert.Greater(slant, 0.2f);
            Assert.Less(slant, 0.3f);
            Assert.AreEqual(0.5f * 0.3f + 0.25f * (0.2f + 0.1f), BodyAnchors.ReachToward(0.3f, 0.2f, 0.1f, Vector2.zero), 1e-4f, "no way: the mean");
        }

        [Test]
        public void APieceOnTheStandInsChestLandsOnTheBodysChestAtItsSize()
        {
            var body = Measured("enemy_acolyte", 0.95f, out var root, out int slot);
            try
            {
                var part = new ElementLookSet.Part { Kind = ElementLookSet.Solid, Follows = true };
                // On the stand-in's chest, a little out past its surface toward the camera (the recipe's -z).
                Vector3 position = new Vector3(0f, BodyAnchors.StandIn.Chest, -0.5f);
                ElementLooks.FitOnEnemy(part, slot, Quaternion.identity, Vector3.zero, ref position, out Vector3 stretch, out Vector3 lean);
                Assert.IsTrue(BodyAnchors.TryPosed(slot, out var posed));
                Assert.AreEqual(posed.Anchors.Chest, position.y, 1e-3f, "on the chest as it stands now");
                Assert.AreEqual(posed.ChestShift.x, lean.x, 1e-4f, "carried with the chest's lean");
                Assert.AreEqual(posed.ChestShift.z, lean.z, 1e-4f);
                float size = BodyAnchors.SizeOf(body) * BodyAnchors.FoldOf(body, posed.Anchors);
                Assert.AreEqual(size, stretch.x, 1e-4f);
                Assert.AreEqual(size, stretch.y, 1e-4f);
                // Out from the upright by as much past the body's own reach that way as it stood past the stand-in's.
                float reach = BodyAnchors.ReachAt(slot, posed.Anchors.Chest, posed.Anchors.Crown, Vector3.back);
                float expected = 0.5f * Mathf.Clamp(reach / BodyAnchors.StandInChest, BodyAnchors.LeastReach, BodyAnchors.MostReach);
                Assert.AreEqual(-expected, position.z, 1e-3f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ACrouchingBodysAnchorsComeDownWithItsJoints()
        {
            var bound = BodyAnchors.Soldier;
            Vector3 feet = new Vector3(2f, 0f, 3f);
            // Standing: hips, chest, neck and head over the feet at their bound heights (the chest's anchor is half way to the neck).
            var standing = BodyAnchors.PoseOf(bound, feet, feet + Vector3.up * 1.1f, feet + Vector3.up * 1.45f, feet + Vector3.up * 1.65f, feet + Vector3.up * 1.8f);
            Assert.AreEqual(1.1f, standing.Anchors.Waist, 1e-4f);
            Assert.AreEqual(1.55f, standing.Anchors.Chest, 1e-4f);
            Assert.AreEqual(1.95f, standing.Anchors.Head, 1e-4f, "the face as far past the skull's joint as the neck runs to it");
            Assert.AreEqual(1.95f + (bound.Crown - bound.Head), standing.Anchors.Crown, 1e-4f);
            Assert.AreEqual(Vector3.zero, standing.ChestShift);
            // Crouched and leaning forward: every anchor lower, and carried ahead with the lean.
            Vector3 ahead = new Vector3(0f, 0f, 0.3f);
            var crouched = BodyAnchors.PoseOf(bound, feet, feet + Vector3.up * 0.7f, feet + Vector3.up * 1.0f + ahead, feet + Vector3.up * 1.15f + ahead, feet + Vector3.up * 1.3f + ahead);
            Assert.Less(crouched.Anchors.Chest, standing.Anchors.Chest);
            Assert.Less(crouched.Anchors.Head, standing.Anchors.Head);
            Assert.AreEqual(0.3f, crouched.ChestShift.z, 1e-4f);
            Assert.AreEqual(0.3f, BodyAnchors.ShiftAt(crouched, BodyAnchors.StandIn.Head + 0.2f).z, 1e-4f, "over the head it goes with the head");
            Assert.AreEqual(0f, BodyAnchors.ShiftAt(crouched, 0f).z, 1e-4f, "at the feet, nothing");
            // A folded body draws its pieces a little smaller, never under three quarters; standing, at full size.
            Assert.AreEqual(1f, BodyAnchors.FoldOf(bound, standing.Anchors), 0.06f);
            Assert.Less(BodyAnchors.FoldOf(bound, crouched.Anchors), 1f);
            Assert.GreaterOrEqual(BodyAnchors.FoldOf(bound, crouched.Anchors), BodyAnchors.LeastFold);
            // Folded flat (knocked down), the anchors still run upward from the feet in order.
            var down = BodyAnchors.PoseOf(bound, feet, feet + Vector3.up * 0.2f, feet + Vector3.up * 0.15f, feet + Vector3.up * 0.15f, feet + Vector3.up * 0.1f);
            Assert.Less(down.Anchors.Waist, down.Anchors.Chest);
            Assert.Less(down.Anchors.Chest, down.Anchors.Head);
            Assert.LessOrEqual(down.Anchors.Head, down.Anchors.Crown);
        }

        [Test]
        public void SmallMediumAndLargeBodiesMeasureInThatOrder()
        {
            var acolyte = Measured("enemy_acolyte", 0.95f, out var a, out _);
            var soldier = Measured("enemy_soldier", 1.12f, out var s, out _);
            var captain = Measured("enemy_soldier", 1.45f, out var c, out _);
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(s);
            Object.DestroyImmediate(c);
            // The castle soldier as it is built (its kind's 1.12) is the soldier the looks were approved on.
            Assert.AreEqual(BodyAnchors.Soldier.Waist, soldier.Waist, 0.01f);
            Assert.AreEqual(BodyAnchors.Soldier.Chest, soldier.Chest, 0.01f);
            Assert.AreEqual(BodyAnchors.Soldier.Head, soldier.Head, 0.01f);
            Assert.AreEqual(BodyAnchors.Soldier.Crown, soldier.Crown, 0.01f);
            Assert.AreEqual(BodyAnchors.Soldier.Girth, soldier.Girth, 0.01f);
            Assert.Less(BodyAnchors.SizeOf(acolyte), BodyAnchors.SizeOf(soldier));
            Assert.Less(BodyAnchors.SizeOf(soldier), BodyAnchors.SizeOf(captain));
            Assert.Less(acolyte.Chest, soldier.Chest);
            Assert.Less(soldier.Chest, captain.Chest);
            // The Gate Captain is the soldier's body drawn 1.45 / 1.12 the size: every anchor grows by that much.
            Assert.AreEqual(soldier.Head * 1.45f / 1.12f, captain.Head, 0.01f);
            foreach (var body in new[] { acolyte, soldier, captain })
            {
                Assert.Less(body.Waist, body.Chest);
                Assert.Less(body.Chest, body.Head);
                Assert.LessOrEqual(body.Head, body.Crown);
                Assert.Greater(body.Girth, 0.05f);
            }
        }

        // One enemy model at its kind's scale under a body root, measured as the game measures it.
        private static BodyAnchors.Anchors Measured(string model, float scale, out GameObject root, out int slot)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Art/Models/Characters/{model}.fbx");
            Assert.IsNotNull(asset, model);
            root = new GameObject("Measured " + model) { hideFlags = HideFlags.HideAndDontSave };
            var body = Object.Instantiate(asset, root.transform);
            body.transform.localScale = Vector3.one * scale;
            slot = BodyAnchors.Slot(root.transform);
            Assert.IsTrue(BodyAnchors.TryAnchors(slot, out var anchors), $"{model} has no anchors");
            return anchors;
        }
    }
}
