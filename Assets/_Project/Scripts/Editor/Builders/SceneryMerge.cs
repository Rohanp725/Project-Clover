using System.Collections.Generic;
using System.Linq;
using Clover.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.EditorTools
{
    /// <summary>
    /// Turns a built place's still scenery into a <see cref="MergedScenery"/>: every static piece drawn with the toon
    /// material that nothing moves, hides or switches is taken out of the scene and written down as mesh, materials and
    /// place, and is drawn merged when the scene opens. A builder opts in by calling <see cref="Prepare"/> last.
    /// Left as they were: anything with a <see cref="Fadeable"/> of its own, anything under a behaviour (a door, a
    /// ladder, a rest fire, a gift), anything a behaviour points at, and anything not static. The fronts a Fadeable
    /// thins out with are merged per Fadeable, so they still thin out with it.
    /// </summary>
    public static class SceneryMerge
    {
        public static MergedScenery Prepare(Transform root, string tag, float cellSize = 24f)
        {
            var toon = Shader.Find(BuildKit.ToonShaderName);
            var fadeables = root.GetComponentsInChildren<Fadeable>(true);
            var groupOf = new Dictionary<Renderer, int>();
            var groups = new List<Fadeable>();
            foreach (var fadeable in fadeables)
            {
                var shown = new SerializedObject(fadeable).FindProperty("alsoShown");
                if (shown == null || shown.arraySize == 0) continue;
                int group = groups.Count;
                groups.Add(fadeable);
                for (int i = 0; i < shown.arraySize; i++)
                    if (shown.GetArrayElementAtIndex(i).objectReferenceValue is Renderer renderer) groupOf[renderer] = group;
            }

            // What a behaviour in the scene points at is kept: a gift that hides what is taken, a spear pulled free.
            var kept = new HashSet<Transform>();
            foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                if (behaviour == null || behaviour is Fadeable || behaviour is MergedScenery) continue;
                kept.Add(behaviour.transform);
                var walk = new SerializedObject(behaviour).GetIterator();
                while (walk.NextVisible(true))
                {
                    if (walk.propertyType != SerializedPropertyType.ObjectReference || walk.objectReferenceValue == null) continue;
                    Transform target = walk.objectReferenceValue is GameObject go ? go.transform
                        : walk.objectReferenceValue is Component component && !(component is Fadeable) ? component.transform : null;
                    // A behaviour that only needs to know the whole place (to put things in it) keeps nothing.
                    if (target != null && target != root && target.parent != null) kept.Add(target);
                }
            }
            kept.Remove(root);

            var pieces = new List<MergedScenery.Piece>();
            var materials = new List<Material>();
            var tints = new List<Color>();
            var carriers = new Dictionary<string, Material>();
            var hosts = new HashSet<GameObject>();
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(false))
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                var slots = renderer.sharedMaterials;
                // A swaying material bends from its own object's foot: merged, it would bend from the world's origin.
                if (slots.Length == 0 || slots.Any(material => material == null || material.shader != toon || material.GetFloat("_Sway") != 0f)) continue;
                // A mesh a player build cannot read would vanish there: it stays as it is.
                if (!filter.sharedMesh.isReadable) continue;
                if (renderer.GetComponent<Fadeable>() != null) continue;
                if (GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) == 0) continue;
                if (Kept(renderer.transform, root, kept)) continue;
                int group = groupOf.TryGetValue(renderer, out var g) ? g : -1;
                pieces.Add(new MergedScenery.Piece
                {
                    Mesh = filter.sharedMesh,
                    Matrix = renderer.transform.localToWorldMatrix,
                    FirstMaterial = materials.Count,
                    MaterialCount = slots.Length,
                    Shadows = renderer.shadowCastingMode == ShadowCastingMode.Off ? 0 : renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly ? 2 : 1,
                    Group = group,
                });
                foreach (var slot in slots)
                {
                    var (carrier, tint) = Carried(slot, carriers, tag);
                    materials.Add(carrier);
                    tints.Add(tint);
                }
                hosts.Add(renderer.gameObject);
                Object.DestroyImmediate(renderer);
                Object.DestroyImmediate(filter);
            }
            foreach (var fadeable in groups) fadeable.AlsoFade(new Renderer[0]);

            // What thins out keeps its own drawing, but its shadow is merged too: a box or a column casts into each of the
            // sun's four cascades, a draw each. Its own renderer casts nothing now; a merged copy casts for it, which
            // stays when it thins out (as a Fadeable's shadow always has).
            Material shadowCarrier = null;
            foreach (var fadeable in fadeables)
            {
                var renderer = fadeable.GetComponent<MeshRenderer>();
                var filter = fadeable.GetComponent<MeshFilter>();
                if (renderer == null || filter == null || filter.sharedMesh == null || !renderer.enabled) continue;
                if (renderer.shadowCastingMode == ShadowCastingMode.Off) continue;
                if (GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) == 0) continue;
                var slots = renderer.sharedMaterials;
                if (slots.Length == 0 || slots.Any(material => material == null || material.shader != toon || material.GetFloat("_Sway") != 0f)) continue;
                // A door that opens or a wall that breaks takes its shadow with it: those keep their own.
                if (Kept(renderer.transform, root, kept)) continue;
                if (shadowCarrier == null) shadowCarrier = Carried(slots[0], carriers, tag).Item1;
                pieces.Add(new MergedScenery.Piece
                {
                    Mesh = filter.sharedMesh,
                    Matrix = renderer.transform.localToWorldMatrix,
                    FirstMaterial = materials.Count,
                    MaterialCount = slots.Length,
                    Shadows = 2,
                    Group = -1,
                });
                for (int i = 0; i < slots.Length; i++)
                {
                    materials.Add(shadowCarrier);
                    tints.Add(Color.white);
                }
                if (renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly) renderer.enabled = false;
                else renderer.shadowCastingMode = ShadowCastingMode.Off;
            }

            // A piece that was only a picture leaves nothing behind it: its object goes, and the holder it stood in.
            var empty = new Queue<GameObject>(hosts);
            int removed = 0;
            while (empty.Count > 0)
            {
                var go = empty.Dequeue();
                if (go == null || go.transform == root) continue;
                if (go.transform.childCount > 0 || go.GetComponents<Component>().Length > 1) continue;
                var parent = go.transform.parent;
                Object.DestroyImmediate(go);
                removed++;
                if (parent != null && parent != root && parent.childCount == 0) empty.Enqueue(parent.gameObject);
            }

            var merged = new GameObject("Merged Scenery").AddComponent<MergedScenery>();
            merged.transform.SetParent(root, false);
            merged.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            merged.Configure(pieces.ToArray(), materials.ToArray(), tints.ToArray(), groups.ToArray(), cellSize);
            Debug.Log($"[Clover.Build] Merged {pieces.Count} pieces of scenery into {merged.MergedCount} meshes and {carriers.Count} materials; {removed} objects left the scene.");
            return merged;
        }

        public const string CarrierFolder = BuildKit.GeneratedRoot + "/Merged";

        /// <summary>
        /// The material that draws a slot merged: materials alike in everything but their colour share one white copy
        /// whose points carry the colour (clover_toon's _VertexColour), so a square of city in fifty colours is a
        /// handful of draws. The tint is the slot's colour in linear terms, as the shader adds it.
        /// </summary>
        public static (Material, Color) Carried(Material material, Dictionary<string, Material> carriers, string tag)
        {
            if (material == null || !material.HasProperty("_VertexColour")) return (material, Color.white);
            string key = Signature(material);
            if (!carriers.TryGetValue(key, out var carrier))
            {
                BuildKit.EnsureFolder(CarrierFolder);
                carrier = BuildKit.MaterialAt(CarrierFolder, $"m_merged_{tag}_{carriers.Count}", BuildKit.ToonShaderName);
                carrier.CopyPropertiesFromMaterial(material);
                carrier.SetColor("_BaseColor", Color.white);
                carrier.SetFloat("_VertexColour", 1f);
                carrier.renderQueue = material.renderQueue;
                carrier.enableInstancing = false;
                // A material with no line draws nothing in the outline pass: it is not asked to draw at all.
                carrier.SetShaderPassEnabled("SRPDefaultUnlit", material.GetFloat("_OutlineWidth") > 0.00001f);
                EditorUtility.SetDirty(carrier);
                carriers[key] = carrier;
            }
            Color colour = material.GetColor("_BaseColor").linear;
            colour.a = 1f;
            return (carrier, colour);
        }

        // Everything a material sets but its colour.
        private static string Signature(Material material)
        {
            var text = new System.Text.StringBuilder(material.shader.name).Append('|').Append(material.renderQueue);
            var shader = material.shader;
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string name = shader.GetPropertyName(i);
                if (name == "_BaseColor") continue;
                text.Append('|').Append(name).Append('=');
                switch (shader.GetPropertyType(i))
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color: text.Append(material.GetColor(name)); break;
                    case UnityEngine.Rendering.ShaderPropertyType.Vector: text.Append(material.GetVector(name)); break;
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        var texture = material.GetTexture(name);
                        text.Append(texture != null ? AssetDatabase.GetAssetPath(texture) + texture.name : "none").Append(material.GetTextureScale(name)).Append(material.GetTextureOffset(name));
                        break;
                    default: text.Append(material.GetFloat(name).ToString("R")); break;
                }
            }
            return text.ToString();
        }

        private static bool Kept(Transform piece, Transform root, HashSet<Transform> kept)
        {
            for (var at = piece; at != null && at != root; at = at.parent)
                if (kept.Contains(at)) return true;
            return false;
        }
    }
}
