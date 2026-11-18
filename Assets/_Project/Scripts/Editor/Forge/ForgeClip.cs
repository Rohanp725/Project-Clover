using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// One animation written as code: a list of key poses with the moment each is reached and how it is arrived at, or a
    /// function that gives the pose for any moment. Baking plays it through on a rig, lets the loose parts (scarf, hair,
    /// headband, cloak) trail behind the body by simulation, and writes the result as an animation clip asset.
    /// </summary>
    public class ForgeClip
    {
        public string Name;
        public float Seconds = 1f;
        public bool Loops;
        public int FramesPerSecond = 60;
        /// <summary>The wind the loose parts feel, in character space, for each moment from 0 to 1: a run blows backward.</summary>
        public Func<float, Vector3> Wind;
        /// <summary>Something done to the rig after the pose is applied, for what a pose cannot say (a full turn in a roll).</summary>
        public Action<ForgeRig, float> After;
        /// <summary>
        /// When true the skeleton's own root bone is baked too, turn and place, so that what <see cref="After"/> does to
        /// it (a full turn on the spot, a walk round an enemy) is in the clip the game plays and not only on the sheet.
        /// </summary>
        public bool MovesRoot;
        /// <summary>
        /// Something laid over the keyed pose at every moment: the shudder of a blow landing, a breath. Given the
        /// moment and the pose, it returns the pose to use. It is what keeps a held pose from being a frozen one.
        /// </summary>
        public Func<float, ForgePose, ForgePose> Shape;
        /// <summary>The moments worth looking at on the contact sheet; evenly spread when null.</summary>
        public float[] SheetMoments;

        private readonly List<(float time, ForgePose pose, Ease ease)> keys = new List<(float, ForgePose, Ease)>();
        private Func<float, ForgePose> function;

        public ForgeClip(string name, float seconds, bool loops = false)
        {
            Name = name;
            Seconds = seconds;
            Loops = loops;
        }

        /// <summary>Adds a key pose reached at this moment (0 to 1 through the clip).</summary>
        public ForgeClip Key(float time, ForgePose pose, Ease ease = Ease.InOut)
        {
            keys.Add((time, pose, ease));
            return this;
        }

        /// <summary>The whole clip as a function of the moment from 0 to 1, for cycles better written as waves than as keys.</summary>
        public ForgeClip From(Func<float, ForgePose> poseAt)
        {
            function = poseAt;
            return this;
        }

        public ForgePose PoseAt(float t)
        {
            var pose = Keyed(t);
            return Shape != null ? Shape(t, pose.Copy()) : pose;
        }

        /// <summary>
        /// The shudder of a blow landing: from the moment of the hit the body rings like something struck, driving on
        /// a little past its pose and coming back, each swing smaller, until it is still.
        /// </summary>
        public static Func<float, ForgePose, ForgePose> Impact(float hit, float seconds, float strength = 1f) => (t, pose) =>
        {
            float since = (t - hit) * seconds;
            if (since <= 0f) return pose;
            float ring = Mathf.Exp(-since * 10f) * Mathf.Sin(since * 40f) * strength;
            pose.Hips += new Vector3(0f, -0.03f * ring, 0.05f * ring);
            pose.HipLean += 2.5f * ring;
            pose.WaistLean += 6f * ring;
            pose.HeadLean -= 4f * ring;
            pose.ShoulderLUp += 5f * ring;
            pose.ShoulderRUp += 5f * ring;
            return pose;
        };

        /// <summary>The shake of a body that has been struck: from the given moment it rocks back and forth and from side to side, dying away.</summary>
        public static Func<float, ForgePose, ForgePose> Shudder(float from, float seconds, float strength = 1f) => (t, pose) =>
        {
            float since = (t - from) * seconds;
            if (since <= 0f) return pose;
            float ring = Mathf.Exp(-since * 9f) * strength;
            float fore = ring * Mathf.Sin(since * 34f), side = ring * Mathf.Cos(since * 27f);
            pose.Hips += new Vector3(0.02f * side, -0.02f * Mathf.Abs(fore), -0.04f * fore);
            pose.WaistLean -= 7f * fore;
            pose.WaistRoll += 5f * side;
            pose.HeadLean -= 8f * fore;
            pose.HeadRoll += 6f * side;
            return pose;
        };

        private ForgePose Keyed(float t)
        {
            if (function != null) return function(t);
            if (keys.Count == 0) return new ForgePose();
            if (t <= keys[0].time) return keys[0].pose;
            for (int i = 1; i < keys.Count; i++)
            {
                if (t > keys[i].time) continue;
                float span = keys[i].time - keys[i - 1].time;
                float u = span > 1e-5f ? (t - keys[i - 1].time) / span : 1f;
                return ForgePose.Blend(keys[i - 1].pose, keys[i].pose, ForgePose.Apply(keys[i].ease, u));
            }
            return keys[keys.Count - 1].pose;
        }

        /// <summary>
        /// When true the body is kept on top of the floor: a kneel deep enough to push a knee through it raises the hips
        /// until the knee rests on it, and a clip that moves the root (a roll, a somersault) is lifted whole while any
        /// part of the trunk, head or knees would be under it.
        /// </summary>
        public bool KeepsAboveFloor;

        // How high the knee joint must stay for the kneecap around it to rest on the floor and not go through.
        private const float KneeClearance = 0.13f;

        /// <summary>Puts the rig in the clip's pose for a moment, without the trailing parts.</summary>
        public void Pose(ForgeRig rig, float t)
        {
            var pose = PoseAt(t);
            pose.ApplyTo(rig);
            if (KeepsAboveFloor)
            {
                // Raising the hips with the feet held where they are lifts the knee by less than the hips, so a few rounds.
                for (int round = 0; round < 6; round++)
                {
                    float knee = Mathf.Min(Height(rig, "shin.L"), Height(rig, "shin.R"));
                    if (knee >= KneeClearance - 0.002f) break;
                    if (round == 0) pose = pose.Copy();
                    pose.Hips.y += KneeClearance - knee;
                    pose.ApplyTo(rig);
                }
            }
            After?.Invoke(rig, t);
            if (KeepsAboveFloor && MovesRoot)
            {
                // A body turned over has its hair and back below its joints, so the skinned body itself is measured.
                float lowest = LowestPoint(rig);
                if (lowest < 0f) rig["root"].Transform.position -= rig.Model.transform.up * lowest;
            }
        }

        private static Mesh measured;
        private static Mesh measuredSource;
        private static bool[] measuredCounts;

        // The lowest point of the skinned body above the floor, leaving out what a foot carries (a sole may touch it).
        private static float LowestPoint(ForgeRig rig)
        {
            var body = rig.Model.GetComponentInChildren<SkinnedMeshRenderer>();
            if (body == null || body.sharedMesh == null) return 0f;
            if (measured == null) measured = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            if (measuredSource != body.sharedMesh)
            {
                measuredSource = body.sharedMesh;
                var names = body.bones.Select(bone => bone != null ? bone.name : "").ToArray();
                measuredCounts = body.sharedMesh.boneWeights.Select(weight => !names[weight.boneIndex0].StartsWith("foot")).ToArray();
            }
            body.BakeMesh(measured, true);
            var toModel = rig.Model.transform.worldToLocalMatrix * body.transform.localToWorldMatrix;
            var vertices = measured.vertices;
            float lowest = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
                if (measuredCounts[i]) lowest = Mathf.Min(lowest, toModel.MultiplyPoint3x4(vertices[i]).y);
            return lowest == float.MaxValue ? 0f : lowest;
        }

        // A joint's height above the floor the character stands on.
        private static float Height(ForgeRig rig, string bone) =>
            rig.Model.transform.InverseTransformPoint(rig[bone].Transform.position).y;

        /// <summary>Bakes the clip on the rig and saves it under the folder, replacing the curves of an existing clip in place.</summary>
        public AnimationClip Bake(ForgeRig rig, string folder, ForgeTails tails)
        {
            int frames = Mathf.Max(1, Mathf.RoundToInt(Seconds * FramesPerSecond));
            var bones = rig.Bones;
            var curves = new AnimationCurve[bones.Count, 4];
            var hipsCurves = new[] { new AnimationCurve(), new AnimationCurve(), new AnimationCurve() };
            var last = new Quaternion[bones.Count];
            for (int b = 0; b < bones.Count; b++)
                for (int c = 0; c < 4; c++) curves[b, c] = new AnimationCurve();

            // The loose parts need a run-up to be where they would be: a loop is played through first, a single action
            // starts from its first pose held still.
            float step = 1f / FramesPerSecond;
            if (tails != null)
            {
                tails.Reset(rig, this, 0f);
                int warm = Loops ? frames * 2 : 40;
                for (int f = 0; f < warm; f++)
                {
                    float t = Loops ? (f % frames) / (float)frames : 0f;
                    Pose(rig, t);
                    tails.Step(rig, step, Wind != null ? Wind(t) : Vector3.zero);
                }
            }

            int hipsIndex = -1, rootIndex = -1;
            for (int b = 0; b < bones.Count; b++)
            {
                if (bones[b].Name == "hips") hipsIndex = b;
                if (bones[b].Name == "root") rootIndex = b;
            }
            var rootCurves = new[] { new AnimationCurve(), new AnimationCurve(), new AnimationCurve() };

            for (int f = 0; f <= frames; f++)
            {
                float t = f / (float)frames;
                // A loop's last frame is its first again.
                float poseTime = Loops && f == frames ? 0f : t;
                Pose(rig, poseTime);
                if (tails != null) tails.Step(rig, step, Wind != null ? Wind(poseTime) : Vector3.zero);
                float time = t * Seconds;
                for (int b = 0; b < bones.Count; b++)
                {
                    Quaternion q = bones[b].Transform.localRotation;
                    if (f > 0 && Quaternion.Dot(last[b], q) < 0f) q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
                    last[b] = q;
                    curves[b, 0].AddKey(time, q.x);
                    curves[b, 1].AddKey(time, q.y);
                    curves[b, 2].AddKey(time, q.z);
                    curves[b, 3].AddKey(time, q.w);
                }
                if (rootIndex >= 0)
                {
                    Vector3 r = bones[rootIndex].Transform.localPosition;
                    rootCurves[0].AddKey(time, r.x);
                    rootCurves[1].AddKey(time, r.y);
                    rootCurves[2].AddKey(time, r.z);
                }
                if (hipsIndex >= 0)
                {
                    Vector3 p = bones[hipsIndex].Transform.localPosition;
                    hipsCurves[0].AddKey(time, p.x);
                    hipsCurves[1].AddKey(time, p.y);
                    hipsCurves[2].AddKey(time, p.z);
                }
            }
            if (Loops && tails != null)
            {
                // The trailing parts never land exactly where they began; close the loop on them.
                for (int b = 0; b < bones.Count; b++)
                    for (int c = 0; c < 4; c++) CloseLoop(curves[b, c]);
            }

            BuildKit.EnsureFolder(folder);
            string path = $"{folder}/{Name}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            bool created = clip == null;
            if (created) clip = new AnimationClip { name = Name };
            clip.ClearCurves();
            clip.frameRate = FramesPerSecond;
            string[] axis = { "x", "y", "z", "w" };
            for (int b = 0; b < bones.Count; b++)
            {
                // The rig's own root object is not animated; only what hangs under it.
                if (bones[b].Transform.parent == rig.Model.transform && !(MovesRoot && b == rootIndex)) continue;
                string bonePath = AnimationUtility.CalculateTransformPath(bones[b].Transform, rig.Model.transform);
                for (int c = 0; c < 4; c++) clip.SetCurve(bonePath, typeof(Transform), "localRotation." + axis[c], curves[b, c]);
                if (MovesRoot && b == rootIndex)
                    for (int c = 0; c < 3; c++) clip.SetCurve(bonePath, typeof(Transform), "localPosition." + axis[c], rootCurves[c]);
                if (b == hipsIndex)
                    for (int c = 0; c < 3; c++) clip.SetCurve(bonePath, typeof(Transform), "localPosition." + axis[c], hipsCurves[c]);
            }
            clip.EnsureQuaternionContinuity();
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = Loops;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            if (created) AssetDatabase.CreateAsset(clip, path);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        // Eases the last tenth of a curve onto its first value, so the loop has no pop.
        private static void CloseLoop(AnimationCurve curve)
        {
            int count = curve.length;
            if (count < 4) return;
            float first = curve[0].value;
            int blend = Mathf.Max(2, count / 10);
            float drift = curve[count - 1].value - first;
            for (int i = count - blend; i < count; i++)
            {
                float u = (i - (count - blend) + 1) / (float)blend;
                var key = curve[i];
                key.value -= drift * u * u * (3f - 2f * u);
                curve.MoveKey(i, key);
            }
        }
    }
}
