using System.Collections.Generic;
using Clover.Player;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The hands with fingers (staging/blender/2026-11-22_hands/notes.md): where each grip holds its handle, in the hand
    /// bone's own space as Unity reads it, the five finger poses read from the grip file's takes, and the seating of a
    /// weapon in a fist. A weapon keeps the frame the clips aim it by (<see cref="WeaponBuilder.Spec.InHandEuler"/> on
    /// the old socket); it is moved to the grip by the least turn that lays its length along the handle's line, and the
    /// hand is turned back by that much in play (<see cref="HandGrip"/>), so the weapon points where it always pointed.
    /// </summary>
    public static class HandGrips
    {
        public const string GripsPath = "Assets/_Project/Art/Animation/player_hand_grips.fbx";

        /// <summary>The finger bones of one hand, in the order <see cref="HandGrip"/> keeps them.</summary>
        public static readonly string[] FingerNames =
        {
            "thumb1", "thumb2", "thumb3", "index1", "index2", "index3", "middle1", "middle2", "middle3",
            "ring1", "ring2", "ring3", "pinky1", "pinky2", "pinky3",
        };

        /// <summary>The grip file's take for each <see cref="HandGrip.Pose"/>, in its order.</summary>
        public static readonly string[] PoseTakes = { "hand_open", "grip_closed", "grip_knife_reverse", "grip_bow", "fist" };

        /// <summary>A handle held in a fist: its centre and its line (toward the thumb) in the hand bone's space.</summary>
        public struct Grip
        {
            public Vector3 Centre, Axis;
        }

        // notes.md, "Unity bone space": the X flip was checked on import (middle1.R reads (0.0079, 0.1078, -0.0072)).
        private static readonly Grip ClosedRight = new Grip { Centre = new Vector3(-0.0284f, 0.0960f, -0.0205f), Axis = new Vector3(0.5500f, 0.3420f, -0.7619f) };
        private static readonly Grip BowRight = new Grip { Centre = new Vector3(-0.0317f, 0.0840f, -0.0229f), Axis = new Vector3(0.4793f, 0.5736f, -0.6643f) };

        /// <summary>The grip of a pose in a hand (0 right, 1 left; the left is the right mirrored across the hand's X).</summary>
        public static Grip Of(HandGrip.Pose pose, int hand)
        {
            var grip = pose == HandGrip.Pose.Bow ? BowRight : ClosedRight;
            if (hand == 1)
            {
                grip.Centre.x = -grip.Centre.x;
                grip.Axis.x = -grip.Axis.x;
            }
            grip.Axis.Normalize();
            return grip;
        }

        /// <summary>
        /// Where a weapon sits in a fist, in the hand bone's space: <paramref name="aimed"/> is its frame as the clips aim it,
        /// <paramref name="seat"/> how far from its origin (in its own axes) the handle's centre is held.
        /// </summary>
        public static (Vector3 position, Quaternion rotation) Seat(Quaternion aimed, Grip grip, Vector3 seat)
        {
            Quaternion seated = Quaternion.FromToRotation(aimed * Vector3.forward, grip.Axis) * aimed;
            return (grip.Centre - seated * seat, seated);
        }

        /// <summary>
        /// Moves a part hung on a hand's socket into the fist and returns the hand's turn that keeps it aimed:
        /// <paramref name="aimedRotation"/> and <paramref name="aimedPosition"/> are where the clips were made with it on the socket.
        /// </summary>
        public static Quaternion SeatInHand(Transform socket, Transform part, Quaternion aimedRotation, Vector3 aimedPosition, Grip grip, Vector3 seat)
        {
            Quaternion aimed = socket.localRotation * aimedRotation;
            var (position, rotation) = Seat(aimed, grip, seat);
            Quaternion unSocket = Quaternion.Inverse(socket.localRotation);
            part.localRotation = unSocket * rotation;
            part.localPosition = unSocket * (position - socket.localPosition);
            return HandGrip.TurnFor(aimed, rotation);
        }

        /// <summary>The finger bones of both hands (right then left), or null when the model has none.</summary>
        public static Transform[] FingersOf(Transform root)
        {
            var found = new Dictionary<string, Transform>();
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) found[t.name] = t;
            var fingers = new Transform[HandGrip.FingersPerHand * 2];
            for (int hand = 0; hand < 2; hand++)
                for (int i = 0; i < FingerNames.Length; i++)
                {
                    if (!found.TryGetValue(FingerNames[i] + (hand == 0 ? ".R" : ".L"), out var bone)) return null;
                    fingers[hand * HandGrip.FingersPerHand + i] = bone;
                }
            return fingers;
        }

        /// <summary>
        /// The finger poses, read from each take of the grip file: the local rotation of every finger (right then left) for
        /// each pose, pose after pose. The takes' paths start at the rig's root bone, so curves are matched by bone name; a
        /// finger a take does not key keeps its rest. Null when the file, a take or a finger bone is missing.
        /// </summary>
        public static Quaternion[] ReadPoses(GameObject modelAsset)
        {
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(GripsPath))
                if (asset is AnimationClip clip && !clip.name.StartsWith("__")) clips[clip.name.Substring(clip.name.LastIndexOf('|') + 1)] = clip;
            var fingers = FingersOf(modelAsset.transform);
            if (fingers == null) return null;
            var poses = new Quaternion[HandGrip.PoseCount * fingers.Length];
            for (int pose = 0; pose < HandGrip.PoseCount; pose++)
            {
                if (!clips.TryGetValue(PoseTakes[pose], out var clip))
                {
                    Debug.LogWarning($"[Clover.Build] {GripsPath} has no take '{PoseTakes[pose]}'.");
                    return null;
                }
                var curves = new Dictionary<string, AnimationCurve>();
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    if (!binding.propertyName.StartsWith("m_LocalRotation.")) continue;
                    string bone = binding.path.Substring(binding.path.LastIndexOf('/') + 1);
                    curves[bone + binding.propertyName.Substring(binding.propertyName.Length - 1)] = AnimationUtility.GetEditorCurve(clip, binding);
                }
                for (int i = 0; i < fingers.Length; i++)
                {
                    string name = fingers[i].name;
                    bool keyed = curves.TryGetValue(name + "x", out var x) & curves.TryGetValue(name + "y", out var y)
                        & curves.TryGetValue(name + "z", out var z) & curves.TryGetValue(name + "w", out var w);
                    var turned = keyed ? new Quaternion(x.Evaluate(0f), y.Evaluate(0f), z.Evaluate(0f), w.Evaluate(0f)) : fingers[i].localRotation;
                    poses[pose * fingers.Length + i] = Quaternion.Normalize(turned);
                }
            }
            return poses;
        }

        /// <summary>
        /// Gives the model its <see cref="HandGrip"/> with these holds: the hands, the fingers, the poses read from the grip
        /// file. Returns null (and adds nothing) when the model has no finger bones.
        /// </summary>
        public static HandGrip Configure(GameObject model, GameObject modelAsset, HandGrip.Hold[] holds)
        {
            var fingers = FingersOf(model.transform);
            var poses = fingers != null ? ReadPoses(modelAsset) : null;
            if (fingers == null || poses == null) return null;
            var grip = model.GetComponent<HandGrip>();
            if (grip == null) grip = model.AddComponent<HandGrip>();
            grip.Configure(BuildKit.FindDeep(model.transform, "hand.R"), BuildKit.FindDeep(model.transform, "hand.L"), fingers, poses, holds);
            EditorUtility.SetDirty(grip);
            return grip;
        }
    }
}
