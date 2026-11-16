using System;
using System.Collections.Generic;
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

        /// <summary>Puts the rig in the clip's pose for a moment, without the trailing parts.</summary>
        public void Pose(ForgeRig rig, float t)
        {
            PoseAt(t).ApplyTo(rig);
            After?.Invoke(rig, t);
        }

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

            int hipsIndex = -1;
            for (int b = 0; b < bones.Count; b++)
                if (bones[b].Name == "hips") hipsIndex = b;

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
                if (bones[b].Transform.parent == rig.Model.transform) continue;
                string bonePath = AnimationUtility.CalculateTransformPath(bones[b].Transform, rig.Model.transform);
                for (int c = 0; c < 4; c++) clip.SetCurve(bonePath, typeof(Transform), "localRotation." + axis[c], curves[b, c]);
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
