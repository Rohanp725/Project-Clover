using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// A character's skeleton held ready for posing by code. Animations here are not keyed by hand in Blender: they are
    /// written as poses in the character's own space (x to its right, y up, z ahead) and this turns each pose into bone
    /// rotations, placing feet and hands exactly with two-bone reach solving. Everything is measured from the rest pose
    /// the mesh was bound in, which is read from the mesh itself, since the model file's own pose is whatever frame it
    /// was exported on.
    /// </summary>
    public sealed class ForgeRig : IDisposable
    {
        public sealed class Bone
        {
            public string Name;
            public Transform Transform;
            public Transform Parent;
            public Quaternion RestLocalRotation;
            public Vector3 RestLocalPosition;
            public Quaternion RestRotation;
            public Vector3 RestPosition;
        }

        public readonly GameObject Model;
        public readonly List<Bone> Bones = new List<Bone>();
        private readonly Dictionary<string, Bone> byName = new Dictionary<string, Bone>();
        private readonly Dictionary<Transform, Bone> byTransform = new Dictionary<Transform, Bone>();

        /// <summary>Which way the weapon in the right hand points at rest, in character space.</summary>
        public Vector3 RestBlade { get; private set; } = Vector3.down;
        /// <summary>
        /// Which way the back of that weapon faces at rest (its edge faces the other way), in character space: chosen
        /// so that a blade pointed straight ahead with no roll is held edge down. Weapon sockets are set to the same.
        /// </summary>
        public Vector3 RestBladeUp => Quaternion.Inverse(Quaternion.FromToRotation(RestBlade, Vector3.forward)) * Vector3.up;

        public ForgeRig(GameObject modelAsset)
        {
            Model = UnityEngine.Object.Instantiate(modelAsset);
            Model.hideFlags = HideFlags.HideAndDontSave;
            Model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            Model.transform.localScale = Vector3.one;

            var skinned = Model.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinned == null) throw new InvalidOperationException($"{modelAsset.name} has no skinned mesh; it cannot be posed.");
            var bind = skinned.sharedMesh.bindposes;
            var skinBones = skinned.bones;
            var bound = new Dictionary<Transform, Matrix4x4>();
            for (int i = 0; i < skinBones.Length && i < bind.Length; i++)
                if (skinBones[i] != null) bound[skinBones[i]] = skinned.transform.localToWorldMatrix * bind[i].inverse;

            // Put every bone the mesh is bound to back where it was bound, parents before children. Bones the mesh does
            // not use keep the transform they came with.
            var rigRoot = FindRigRoot(skinBones);
            var ordered = new List<Transform>();
            Collect(rigRoot, ordered);
            foreach (var t in ordered)
            {
                if (!bound.TryGetValue(t, out var world)) continue;
                t.SetPositionAndRotation(world.GetColumn(3), world.rotation);
            }
            foreach (var t in ordered)
            {
                var bone = new Bone
                {
                    Name = t.name,
                    Transform = t,
                    Parent = t.parent,
                    RestLocalRotation = t.localRotation,
                    RestLocalPosition = t.localPosition,
                    RestRotation = t.rotation,
                    RestPosition = t.position,
                };
                Bones.Add(bone);
                byName[t.name] = bone;
                byTransform[t] = bone;
            }
            if (Has("hand.R") && Has("weapon.R"))
            {
                Vector3 along = this["weapon.R"].RestPosition - this["hand.R"].RestPosition;
                if (along.sqrMagnitude > 1e-6f) RestBlade = along.normalized;
            }
        }

        public void Dispose()
        {
            if (Model != null) UnityEngine.Object.DestroyImmediate(Model);
        }

        public bool Has(string name) => byName.ContainsKey(name);

        public Bone this[string name]
        {
            get
            {
                if (!byName.TryGetValue(name, out var bone)) throw new KeyNotFoundException($"The rig has no bone '{name}'.");
                return bone;
            }
        }

        /// <summary>Every bone back to the rest pose.</summary>
        public void Rest()
        {
            foreach (var bone in Bones)
            {
                bone.Transform.localRotation = bone.RestLocalRotation;
                bone.Transform.localPosition = bone.RestLocalPosition;
            }
        }

        /// <summary>How far the bone is turned from rest right now, as a rotation in character space.</summary>
        public Quaternion Turned(Transform bone)
        {
            return byTransform.TryGetValue(bone, out var b) ? bone.rotation * Quaternion.Inverse(b.RestRotation) : Quaternion.identity;
        }

        /// <summary>Turns a bone from rest by a rotation given in character space, whatever its parent is doing.</summary>
        public void TurnAbsolute(string name, Quaternion turn)
        {
            var bone = this[name];
            bone.Transform.rotation = turn * bone.RestRotation;
        }

        /// <summary>Turns a bone by a rotation given in character space, on top of whatever its parent has been turned by.</summary>
        public void TurnOnParent(string name, Quaternion turn)
        {
            var bone = this[name];
            Quaternion parentTurn = bone.Parent != null ? Turned(bone.Parent) : Quaternion.identity;
            bone.Transform.rotation = parentTurn * turn * bone.RestRotation;
        }

        /// <summary>
        /// Bends a two-bone limb (thigh and shin, or upper arm and forearm) so its end reaches the target. The pole is the
        /// direction the knee or elbow should point. A target out of reach is reached toward, at full stretch.
        /// </summary>
        public void Reach(string upperName, string lowerName, string endName, Vector3 target, Vector3 pole)
        {
            var upper = this[upperName];
            var lower = this[lowerName];
            var end = this[endName];
            float upperLength = Vector3.Distance(upper.RestPosition, lower.RestPosition);
            float lowerLength = Vector3.Distance(lower.RestPosition, end.RestPosition);

            // Start each bone from where its parent carries it, so the twist along the limb is always the same.
            TurnOnParent(upperName, Quaternion.identity);
            TurnOnParent(lowerName, Quaternion.identity);

            Vector3 from = upper.Transform.position;
            Vector3 toTarget = target - from;
            float distance = Mathf.Clamp(toTarget.magnitude, Mathf.Abs(upperLength - lowerLength) + 0.002f, upperLength + lowerLength - 0.002f);
            Vector3 direction = toTarget.sqrMagnitude > 1e-8f ? toTarget.normalized : Vector3.down;
            Vector3 bend = pole - direction * Vector3.Dot(pole, direction);
            if (bend.sqrMagnitude < 1e-6f) bend = Vector3.Cross(direction, Vector3.right);
            bend.Normalize();
            float cosine = Mathf.Clamp((upperLength * upperLength + distance * distance - lowerLength * lowerLength) / (2f * upperLength * distance), -1f, 1f);
            float sine = Mathf.Sqrt(1f - cosine * cosine);
            Vector3 joint = from + direction * (cosine * upperLength) + bend * (sine * upperLength);
            Vector3 reached = from + direction * distance;

            upper.Transform.rotation = Quaternion.FromToRotation(lower.Transform.position - from, joint - from) * upper.Transform.rotation;
            lower.Transform.rotation = Quaternion.FromToRotation(end.Transform.position - lower.Transform.position, reached - lower.Transform.position) * lower.Transform.rotation;
        }

        /// <summary>Swings a bone so that it points from its own joint along the given direction; its child end follows.</summary>
        public void PointAlong(string name, string childName, Vector3 direction)
        {
            var bone = this[name];
            var child = this[childName];
            Vector3 now = child.Transform.position - bone.Transform.position;
            if (now.sqrMagnitude < 1e-8f || direction.sqrMagnitude < 1e-8f) return;
            bone.Transform.rotation = Quaternion.FromToRotation(now, direction) * bone.Transform.rotation;
        }

        private static Transform FindRigRoot(Transform[] skinBones)
        {
            // The highest ancestor that is itself a bone, or failing that the first bone's parent chain's top bone.
            Transform top = null;
            foreach (var bone in skinBones)
            {
                if (bone == null) continue;
                var t = bone;
                while (t.parent != null && t.parent.parent != null) t = t.parent;
                top = t;
                break;
            }
            if (top == null) throw new InvalidOperationException("The skinned mesh has no bones.");
            return top;
        }

        private static void Collect(Transform t, List<Transform> into)
        {
            into.Add(t);
            for (int i = 0; i < t.childCount; i++) Collect(t.GetChild(i), into);
        }
    }
}
