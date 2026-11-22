using System.Linq;
using Clover.EditorTools;
using Clover.EditorTools.Forge;
using Clover.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The Twin Knives are held in a fist, across the palm, through every one of their clips: the clip's hand stays near
    /// the line of the forearm, and no hand turns over between two frames while its arm and the blade it was asked for
    /// hold still (the owner's note of 2026-11-21: wrong angles, hands not closed on the handles). Since the hands have
    /// fingers (2026-11-22) each knife sits along the closed grip's handle line, and the hand turned about it in play
    /// (<see cref="HandGrip"/>) points it exactly where the clip aims it.
    /// </summary>
    public class KnifeGripTests
    {
        [Test]
        public void TheKnivesHangAcrossThePalmOnBothHands()
        {
            var spec = WeaponBuilder.SpecFor("knife");
            Assert.AreEqual(new Vector3(-WeaponBuilder.KnifeGripAngle, 0f, 0f), spec.InHandEuler);
            Assert.IsTrue(spec.OffHandEuler.HasValue);
            Assert.AreEqual(spec.InHandEuler, spec.OffHandEuler.Value, "the left knife is aimed as the right is");
            // Seated back along its own length, so the guard clears the knuckles and the pommel shows under the fist.
            Vector3 along = Quaternion.Euler(spec.InHandEuler) * Vector3.forward;
            Assert.AreEqual(-WeaponBuilder.KnifeSeat, Vector3.Dot(spec.InHandPosition, along), 1e-4f);
            Assert.Less(Vector3.ProjectOnPlane(spec.InHandPosition, along).magnitude, 1e-4f);
            Assert.AreEqual(HandGrip.Pose.Closed, spec.Grip, "a forward grip: the closed fist");
            Assert.AreEqual(WeaponBuilder.KnifeSeat, spec.HeldAt.z, 1e-5f);

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath);
            using (var rig = new ForgeRig(asset))
            {
                WeaponBuilder.Hang(rig.Model, asset);
                Assert.IsNotNull(rig.Model.GetComponent<HandGrip>(), "the hands are given their grips");
                string[] parts = { "knife_hand", "knife_hand_L" };
                for (int side = 0; side < 2; side++)
                {
                    var knife = WeaponRack.FindSocket(rig.Model.transform, parts[side]);
                    var hand = rig[side == 0 ? "hand.R" : "hand.L"].Transform;
                    var grip = HandGrips.Of(HandGrip.Pose.Closed, side);
                    Assert.Less(Vector3.Angle(hand.InverseTransformDirection(knife.forward), grip.Axis), 0.5f, $"{parts[side]} lies along the handle's line");
                    Vector3 held = hand.InverseTransformPoint(knife.TransformPoint(spec.HeldAt));
                    Assert.Less((held - grip.Centre).magnitude, 0.002f, $"{parts[side]}'s grip is in the middle of the fist");
                }
            }
        }

        [Test]
        public void EveryKnifeClipHoldsTheKnivesInAFistNearTheForearmsLine()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath);
            Assert.IsNotNull(asset);
            var clips = PlayerAnimations.All().Where(clip => clip.Name.Contains("knife")).ToList();
            Assert.GreaterOrEqual(clips.Count, 17);
            var spec = WeaponBuilder.SpecFor("knife");
            using (var rig = new ForgeRig(asset))
            {
                var rack = WeaponBuilder.Hang(rig.Model, asset);
                rack.Show("knife", true);
                // The Katana is shown by the animator, not the rack: put away, as it is while the knives are carried.
                var katana = WeaponRack.FindSocket(rig.Model.transform, WeaponBuilder.KatanaDrawn);
                if (katana != null) katana.gameObject.SetActive(false);
                var grip = rig.Model.GetComponent<HandGrip>();
                var knifeR = WeaponRack.FindSocket(rig.Model.transform, "knife_hand");
                var knifeL = WeaponRack.FindSocket(rig.Model.transform, "knife_hand_L");
                Assert.IsNotNull(knifeR);
                Assert.IsNotNull(knifeL);
                string[] hands = { "hand.R", "hand.L" }, forearms = { "forearm.R", "forearm.L" };
                Transform[] knives = { knifeR, knifeL };
                Quaternion[] aims = { Quaternion.Euler(spec.InHandEuler), Quaternion.Euler(spec.OffHandEuler.Value) };
                foreach (var clip in clips)
                {
                    int frames = Mathf.Max(1, Mathf.RoundToInt(clip.Seconds * 60f));
                    var lastHand = new Quaternion[2];
                    var lastAsked = new Vector3[2];
                    var lastAlong = new Vector3[2];
                    for (int f = 0; f <= frames; f++)
                    {
                        float t = f / (float)frames;
                        clip.Pose(rig, t);
                        var pose = clip.PoseAt(t);
                        var unturn = Quaternion.Inverse(rig.Turned(rig["root"].Transform));
                        // Where the clip aims each knife, and its wrist, before the grip turns the hand about the knife.
                        var aimed = new Vector3[2];
                        for (int side = 0; side < 2; side++)
                        {
                            var hand = rig[hands[side]].Transform;
                            var socket = hand.Find(side == 0 ? WeaponRack.HandRight : WeaponRack.HandLeft);
                            aimed[side] = hand.rotation * (socket.localRotation * aims[side]) * Vector3.forward;
                            Vector3 along = (hand.position - rig[forearms[side]].Transform.position).normalized;
                            Assert.LessOrEqual(Vector3.Angle(along, socket.forward), ForgePose.WristBend + 1f, $"{clip.Name} at {t:0.00}: the {hands[side]} wrist is bent past what a wrist does");
                        }
                        grip.Apply(1f);
                        for (int side = 0; side < 2; side++)
                        {
                            var hand = rig[hands[side]].Transform;
                            Assert.Less(Vector3.Angle(aimed[side], knives[side].forward), 0.5f, $"{clip.Name} at {t:0.00}: the {hands[side]} knife does not point where the clip aims it");
                            Vector3 along = (hand.position - rig[forearms[side]].Transform.position).normalized;
                            Quaternion turned = unturn * hand.rotation;
                            Vector3 asked = (side == 0 ? pose.Blade : pose.BladeL).normalized;
                            Vector3 forearm = unturn * along;
                            if (f > 0)
                            {
                                float moved = Mathf.Max(Vector3.Angle(lastAsked[side], asked), Vector3.Angle(lastAlong[side], forearm));
                                // A still arm and a still asked blade: anything more than a flourish of the wrist is a flip.
                                if (moved < 8f)
                                    Assert.Less(Quaternion.Angle(lastHand[side], turned), 90f, $"{clip.Name} at {t:0.00}: the {hands[side]} turned over while its arm held still");
                            }
                            lastHand[side] = turned;
                            lastAsked[side] = asked;
                            lastAlong[side] = forearm;
                        }
                    }
                }
            }
        }
    }
}
