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
    /// The Twin Knives are held in a fist, across the palm, through every one of their clips: the hand stays near the
    /// line of the forearm, the knife leaves the fist at the grip angle, and no hand turns over between two frames
    /// while its arm and the blade it was asked for hold still (the owner's note of 2026-11-21: wrong angles, hands not
    /// closed on the handles).
    /// </summary>
    public class KnifeGripTests
    {
        [Test]
        public void TheKnivesHangAcrossThePalmOnBothHands()
        {
            var spec = WeaponBuilder.SpecFor("knife");
            Assert.AreEqual(new Vector3(-WeaponBuilder.KnifeGripAngle, 0f, 0f), spec.InHandEuler);
            Assert.IsTrue(spec.OffHandEuler.HasValue);
            Assert.AreEqual(spec.InHandEuler, spec.OffHandEuler.Value, "the left knife is held as the right is");
            // Seated back along its own length, so the guard clears the knuckles and the pommel shows under the fist.
            Vector3 along = Quaternion.Euler(spec.InHandEuler) * Vector3.forward;
            Assert.AreEqual(-WeaponBuilder.KnifeSeat, Vector3.Dot(spec.InHandPosition, along), 1e-4f);
            Assert.Less(Vector3.ProjectOnPlane(spec.InHandPosition, along).magnitude, 1e-4f);
        }

        [Test]
        public void EveryKnifeClipHoldsTheKnivesInAFistNearTheForearmsLine()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath);
            Assert.IsNotNull(asset);
            var clips = PlayerAnimations.All().Where(clip => clip.Name.Contains("knife")).ToList();
            Assert.GreaterOrEqual(clips.Count, 17);
            using (var rig = new ForgeRig(asset))
            {
                WeaponBuilder.Hang(rig.Model, asset);
                var knifeR = WeaponRack.FindSocket(rig.Model.transform, "knife_hand");
                var knifeL = WeaponRack.FindSocket(rig.Model.transform, "knife_hand_L");
                Assert.IsNotNull(knifeR);
                Assert.IsNotNull(knifeL);
                string[] hands = { "hand.R", "hand.L" }, forearms = { "forearm.R", "forearm.L" };
                Transform[] knives = { knifeR, knifeL };
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
                        for (int side = 0; side < 2; side++)
                        {
                            var hand = rig[hands[side]].Transform;
                            Vector3 along = (hand.position - rig[forearms[side]].Transform.position).normalized;
                            // The hand's length is its socket's +Z (the line the rack's sockets are made along).
                            Vector3 handAxis = hand.Find(side == 0 ? WeaponRack.HandRight : WeaponRack.HandLeft).forward;
                            Vector3 blade = knives[side].forward;
                            Assert.AreEqual(WeaponBuilder.KnifeGripAngle, Vector3.Angle(handAxis, blade), 1f, $"{clip.Name} at {t:0.00}: the {hands[side]} knife has slipped in the fist");
                            Assert.LessOrEqual(Vector3.Angle(along, handAxis), ForgePose.WristBend + 1f, $"{clip.Name} at {t:0.00}: the {hands[side]} wrist is bent past what a wrist does");

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
