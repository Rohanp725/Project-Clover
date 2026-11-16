using System.Collections.Generic;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The loose parts of a character: scarf tails, a headband's ends, a ponytail, a cloak. Each is a chain of bones
    /// hanging from the body. Baking an animation simulates them frame by frame as points on a string, so they lag
    /// behind a turn, swing past where the body stops and settle, and stream back in a run. This follow-through is a
    /// large part of what makes a keyed pose read as movement.
    /// </summary>
    public class ForgeTails
    {
        private class Chain
        {
            public ForgeRig.Bone[] Bones;
            public Vector3[] Points;
            public Vector3[] Before;
            public float[] Lengths;
            /// <summary>Each joint's rest offset from the joint before it, in the space of the chain's parent at rest.</summary>
            public Vector3[] RestOffsets;
            public Quaternion ParentRest;
            public float Stiffness;
        }

        /// <summary>How much of its speed a point keeps each step.</summary>
        public float Damping = 0.9f;
        public float Gravity = 7f;

        private readonly List<Chain> chains = new List<Chain>();

        /// <summary>
        /// Adds a chain by its bone names from the body outward. Stiffness from 0 to 1 is how strongly it springs back to
        /// its rest shape: hair is stiffer than a scarf.
        /// </summary>
        public ForgeTails Add(ForgeRig rig, float stiffness, params string[] boneNames)
        {
            foreach (string name in boneNames)
                if (!rig.Has(name)) return this;
            var chain = new Chain { Bones = new ForgeRig.Bone[boneNames.Length], Stiffness = stiffness };
            for (int i = 0; i < boneNames.Length; i++) chain.Bones[i] = rig[boneNames[i]];
            int joints = boneNames.Length + 1;
            chain.Points = new Vector3[joints];
            chain.Before = new Vector3[joints];
            chain.Lengths = new float[joints - 1];
            chain.RestOffsets = new Vector3[joints - 1];
            var parent = chain.Bones[0].Parent;
            chain.ParentRest = Quaternion.identity;
            foreach (var b in rig.Bones)
                if (b.Transform == parent) chain.ParentRest = b.RestRotation;
            for (int i = 0; i < joints - 1; i++)
            {
                Vector3 from = chain.Bones[i].RestPosition;
                // The last bone has no child to mark its end: it is taken to be as long as the bone before it.
                Vector3 to = i + 1 < chain.Bones.Length ? chain.Bones[i + 1].RestPosition
                    : from + (i > 0 ? (from - chain.Bones[i - 1].RestPosition) : Vector3.down * 0.15f);
                chain.Lengths[i] = Mathf.Max(0.01f, Vector3.Distance(from, to));
                chain.RestOffsets[i] = Quaternion.Inverse(chain.ParentRest) * (to - from);
            }
            chains.Add(chain);
            return this;
        }

        /// <summary>Puts every chain at rest on the body as posed at the given moment of the clip.</summary>
        public void Reset(ForgeRig rig, ForgeClip clip, float t)
        {
            clip.Pose(rig, t);
            foreach (var chain in chains)
            {
                Target(chain, chain.Points);
                for (int i = 0; i < chain.Points.Length; i++) chain.Before[i] = chain.Points[i];
            }
        }

        private readonly Vector3[] scratch = new Vector3[16];

        /// <summary>Advances every chain by one frame on the rig as now posed, then turns the chain's bones to follow their points.</summary>
        public void Step(ForgeRig rig, float deltaTime, Vector3 wind)
        {
            foreach (var chain in chains)
            {
                var target = scratch;
                Target(chain, target);
                int joints = chain.Points.Length;
                chain.Points[0] = target[0];
                chain.Before[0] = target[0];
                for (int i = 1; i < joints; i++)
                {
                    Vector3 velocity = (chain.Points[i] - chain.Before[i]) * Damping;
                    chain.Before[i] = chain.Points[i];
                    Vector3 p = chain.Points[i] + velocity + (Vector3.down * Gravity + wind) * (deltaTime * deltaTime);
                    // The pull back toward the rest shape, weaker toward the tip.
                    float pull = chain.Stiffness * (1f - 0.5f * i / (joints - 1));
                    p = Vector3.Lerp(p, target[i], pull);
                    // Keep it from passing through the ground.
                    if (p.y < 0.04f) p.y = 0.04f;
                    chain.Points[i] = p;
                }
                for (int pass = 0; pass < 3; pass++)
                {
                    for (int i = 1; i < joints; i++)
                    {
                        Vector3 from = chain.Points[i - 1];
                        Vector3 offset = chain.Points[i] - from;
                        float length = offset.magnitude;
                        chain.Points[i] = length > 1e-5f ? from + offset * (chain.Lengths[i - 1] / length) : from + Vector3.down * chain.Lengths[i - 1];
                    }
                }
                for (int i = 0; i < chain.Bones.Length; i++)
                {
                    var bone = chain.Bones[i];
                    rig.TurnOnParent(bone.Name, Quaternion.identity);
                    Vector3 restDirection = (rig.Turned(bone.Transform) * chain.ParentRest) * chain.RestOffsets[i];
                    Vector3 wanted = chain.Points[i + 1] - chain.Points[i];
                    if (wanted.sqrMagnitude < 1e-8f || restDirection.sqrMagnitude < 1e-8f) continue;
                    bone.Transform.rotation = Quaternion.FromToRotation(restDirection, wanted) * bone.Transform.rotation;
                }
            }
        }

        // Where each joint would be if the chain hung rigidly off the body in its rest shape.
        private static void Target(Chain chain, Vector3[] into)
        {
            var first = chain.Bones[0];
            Quaternion carried = first.Parent != null ? first.Parent.rotation : Quaternion.identity;
            // The chain's first joint is carried by its parent bone exactly.
            first.Transform.localPosition = first.RestLocalPosition;
            Vector3 at = first.Transform.position;
            into[0] = at;
            for (int i = 0; i < chain.RestOffsets.Length; i++)
            {
                at += carried * chain.RestOffsets[i];
                into[i + 1] = at;
            }
        }
    }
}
