using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Water land's people: toon materials for the m_water_* slots their models carry (the reef-grown in two ranks, the
    /// drowned, the hooked and the bait's jaws and lure), in the palette the owner approved on 2026-11-20
    /// (staging/comfyui/2026-11-20_water_enemies/notes.md, sections 4 and 5). <see cref="EnemyBuilder"/> paints any model
    /// with these slots through <see cref="For"/>, as <see cref="SunCityKit"/> paints the Fire land's.
    /// </summary>
    public static class WaterEnemyKit
    {
        public const string Folder = BuildKit.GeneratedRoot + "/WaterEnemies";
        private const string ModelFolder = "Assets/_Project/Art/Models/Characters";

        /// <summary>The reef-grown is drawn a little larger than the rig it shares (the owner, 2026-11-20); its FBX stays at 1.</summary>
        public const float ReefGrownScale = 1.05f;

        /// <summary>The Water land's enemy models, as imported.</summary>
        public static readonly string[] Models =
            { "enemy_reefgrown", "enemy_reefgrown_young", "enemy_drowned", "enemy_hooked", "enemy_bait_jaws", "enemy_bait_lure" };

        // The approved slot colours, written as the Blender pass gave them (sRGB, 0 to 255).
        private static readonly Dictionary<string, (int r, int g, int b)> palette = new Dictionary<string, (int, int, int)>
        {
            { "skin", (112, 122, 134) },
            { "coral", (226, 214, 190) },
            { "coral_red", (200, 58, 46) },
            { "wrap", (98, 70, 48) },
            { "rope", (150, 122, 78) },
            { "hair", (30, 26, 28) },
            { "dark", (14, 14, 20) },
            { "drowned_skin", (96, 116, 104) },
            { "kelp", (40, 70, 40) },
            { "stone", (40, 42, 46) },
            { "eye", (206, 216, 206) },
            { "tapa", (176, 156, 120) },
            { "hooked_skin", (140, 140, 142) },
            { "bone", (222, 212, 182) },
            { "pouch", (84, 60, 44) },
            { "wood", (92, 64, 44) },
            { "tooth", (236, 230, 210) },
            { "bait_flesh", (166, 140, 138) },
            { "bait_stalk", (96, 70, 54) },
            { "bait_skin", (84, 74, 70) },
            { "bait_gullet", (110, 22, 24) },
            { "lure_skin", (150, 104, 76) },
        };

        /// <summary>True for a slot name this land knows how to paint.</summary>
        public static bool Knows(string slot) => slot != null && slot.StartsWith("m_water_");

        // The lure's face is thin shapes laid 2.5 mm over its skin in these two slots. An ink hull round each of them smears
        // the brows, eyes and mouth, so on the lure they draw none, as Blender's outline left them out (notes.md, 6).
        private const string LureMesh = "enemy_bait_lure";
        private static readonly string[] lureFace = { "eye", "dark" };

        /// <summary>
        /// The toon material for an imported m_water_* slot, made or updated under its own name. <paramref name="mesh"/>
        /// is the renderer's name: the lure's face slots get inkless copies of their own.
        /// </summary>
        public static Material For(string slot, string mesh = "")
        {
            string name = slot.Substring("m_water_".Length);
            if (mesh == LureMesh && lureFace.Contains(name))
            {
                var face = For(slot);
                var bare = BuildKit.Toon(Folder, $"m_water_lure_face_{name}", face.GetColor("_BaseColor"), true);
                bare.SetFloat("_OutlineWidth", 0f);
                return bare.Lit(face.GetColor("_RimColor"), face.GetFloat("_RimWidth"));
            }
            Color colour;
            if (palette.TryGetValue(name, out var rgb)) colour = new Color(rgb.r / 255f, rgb.g / 255f, rgb.b / 255f);
            else
            {
                Debug.LogWarning($"[Clover.Build] The Water land has no colour for the slot '{slot}'; it is drawn grey.");
                colour = Color.grey;
            }
            var material = BuildKit.Toon(Folder, slot, colour, true);
            // A cool sea-light rim, weaker on the near-black slots so they stay dark.
            bool dark = name == "dark" || name == "hair" || name == "stone";
            return material.Lit(dark ? new Color(0.4f, 0.5f, 0.55f) : new Color(0.6f, 0.75f, 0.85f), 0.3f);
        }

        /// <summary>Paints every slot of a model that this land knows, and leaves the others as they are.</summary>
        public static void Dress(GameObject model)
        {
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => imported != null && Knows(imported.name) ? For(imported.name, renderer.name) : imported).ToArray();
        }

        [MenuItem("Clover/Build/Water Enemy Materials")]
        public static void BuildMaterials()
        {
            var slots = new SortedSet<string>();
            foreach (string model in Models)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelFolder}/{model}.fbx");
                if (asset == null) { Debug.LogWarning($"[Clover.Build] No Water enemy model at {ModelFolder}/{model}.fbx."); continue; }
                foreach (var renderer in asset.GetComponentsInChildren<Renderer>(true))
                    foreach (var material in renderer.sharedMaterials)
                        if (material != null && Knows(material.name))
                        {
                            slots.Add(material.name);
                            For(material.name, renderer.name);
                        }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Clover.Build] Water enemy materials: {slots.Count} slots in {Folder} ({string.Join(", ", slots)}).");
        }
    }
}
