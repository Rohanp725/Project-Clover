using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clover.Player
{
    /// <summary>
    /// Dresses the player for the scene: swaps the body's mesh and materials for the outfit's on the same skinned
    /// renderer and the same skeleton, so the animations, the weapons on their sockets and everything that holds the
    /// body renderer (afterimages, spell clones, hiding) carry on unchanged. Which outfit comes from <see cref="Outfits"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerOutfit : MonoBehaviour
    {
        [SerializeField] private OutfitSet outfits;
        [SerializeField] private SkinnedMeshRenderer body;
        [SerializeField] private Transform rig;

        /// <summary>The id of the outfit on now, or empty before the first is put on.</summary>
        public string Worn { get; private set; } = "";
        /// <summary>Whether the last outfit put on needed its bones looked up by name, rather than taking the rig's as they were.</summary>
        public bool Rebound { get; private set; }
        public OutfitSet Set => outfits;

        private Dictionary<string, Transform> bonesByName;

        public void Configure(OutfitSet set, SkinnedMeshRenderer bodyRenderer, Transform rigRoot)
        {
            outfits = set;
            body = bodyRenderer;
            rig = rigRoot;
        }

        // Awake, before anything that copies the body (afterimages, spell clones) reads it in Start or later.
        private void Awake()
        {
            if (outfits == null || body == null) return;
            string id = Outfits.Choose(SceneManager.GetActiveScene().name, Outfits.Chosen, Outfits.Last, known => outfits.Find(known) != null);
            Wear(id);
        }

        /// <summary>Puts an outfit on by id and remembers it as the last worn. False when the set has no such outfit.</summary>
        public bool Wear(string id)
        {
            var outfit = outfits != null ? outfits.Find(id) : null;
            if (outfit == null || body == null)
            {
                Debug.LogWarning($"[Clover.Player] There is no outfit '{id}' to wear.");
                return false;
            }
            if (body.sharedMesh != outfit.Mesh)
            {
                Rebound = !SameBones(outfit.BoneNames, body.bones);
                // An outfit skinned to the rig in the same bone order takes the rig's bones as they are; otherwise its
                // bind poses are matched to the rig's bones by name.
                if (Rebound) body.bones = BonesFor(outfit.BoneNames);
                body.sharedMesh = outfit.Mesh;
                body.localBounds = outfit.Bounds;
            }
            body.sharedMaterials = outfit.Materials;
            Worn = outfit.Id;
            Outfits.Last = outfit.Id;
            return true;
        }

        private static bool SameBones(string[] names, Transform[] bones)
        {
            if (names == null || bones == null || names.Length != bones.Length) return false;
            for (int i = 0; i < names.Length; i++)
                if (bones[i] == null || bones[i].name != names[i]) return false;
            return true;
        }

        private Transform[] BonesFor(string[] names)
        {
            if (bonesByName == null)
            {
                bonesByName = new Dictionary<string, Transform>();
                var root = rig != null ? rig : body.transform.parent;
                foreach (var bone in root.GetComponentsInChildren<Transform>(true))
                    if (!bonesByName.ContainsKey(bone.name)) bonesByName[bone.name] = bone;
            }
            var bones = new Transform[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                if (!bonesByName.TryGetValue(names[i], out bones[i]))
                {
                    Debug.LogWarning($"[Clover.Player] The rig has no bone '{names[i]}' for the outfit; that part stays at the root.");
                    bones[i] = body.rootBone;
                }
            }
            return bones;
        }
    }
}
