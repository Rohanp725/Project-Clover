using System.Collections.Generic;
using System.IO;
using System.Text;
using Clover.Player.Forms;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the elements' effect art the game draws (<see cref="ElementArtSet"/>) from the approved second-pass kits and
    /// sheets in Art/VFX/<Element>/ (staging/blender/2026-11-18_vfx/notes.md):
    /// - every solid piece of the eleven kits is baked into one mesh in the game's axes, each of its Blender material
    ///   slots painted into the vertex colours in that band's colour from the notes (chrome and foam flagged in the
    ///   alpha for Clover/Spell), with the smoothed normals the ink line needs in its second UVs;
    /// - every flat piece (char rings, rosettes, stains and pits, the sun-wheel, the kolam, clang rings, drained discs,
    ///   gas layers) is rasterised from above into a mask, one slot per channel, and given a Clover/Decal material;
    /// - each element gets its solid material (Clover/Spell with its own ink colour, four bands for Ice and Light,
    ///   chrome for Metal), and each sheet a Clover/Flipbook material;
    /// - a body's flat shadow is drawn as a mask for Light's thrown shadows and Dark's reaching ones.
    /// Everything lands under Generated/Vfx/Element/ under stable names; the set keeps its GUID across rebuilds, so
    /// the scenes that hold it never lose it. Clover > Build > Element Art Into Scenes puts it in the built scenes.
    /// </summary>
    public static class ElementArtBuilder
    {
        public const string Folder = BuildKit.GeneratedRoot + "/Vfx/Element";
        public const string SetPath = Folder + "/element_art.asset";
        private const string KitRoot = "Assets/_Project/Art/VFX";

        public static readonly string[] Elements = { "fire", "lightning", "wind", "ice", "water", "life", "earth", "metal", "light", "dark", "poison" };

        // Each element's ink, as linear colour (the notes: a dark brown-red for fire, deep indigo for lightning, navy for
        // ice, violet for dark, and so on), and how much white its lit facets take.
        private static readonly Color[] Inks =
        {
            new Color(0.09f, 0.02f, 0.01f), new Color(0.05f, 0.02f, 0.2f), new Color(0.1f, 0.09f, 0.07f), new Color(0.01f, 0.03f, 0.12f),
            new Color(0.005f, 0.03f, 0.06f), new Color(0.02f, 0.06f, 0.015f), new Color(0.035f, 0.02f, 0.012f), new Color(0.02f, 0.02f, 0.025f),
            new Color(0.2f, 0.09f, 0.01f), new Color(0.3f, 0.09f, 0.55f), new Color(0.01f, 0.06f, 0.05f),
        };
        private static readonly float[] Sheens = { 0f, 0.2f, 0.08f, 0.3f, 0.18f, 0.06f, 0f, 0.25f, 0.2f, 0f, 0.04f };

        // A slot's band: linear colour, its alpha (see-through for gas, shadow, drained floor), and the paint flag.
        private struct Band
        {
            public Color Colour;
            public float Flag;   // 1 plain, 0.7 chrome, 0.3 foam (Clover/Spell reads these from the vertex alpha)
            public bool Glow;    // drawn as a see-through unlit shape, never inked
            public Band(float r, float g, float b, float a = 1f, float flag = 1f, bool glow = false)
            {
                Colour = new Color(r, g, b, a);
                Flag = flag;
                Glow = glow;
            }
        }

        // The notes' bands by slot name, in linear colour as Blender rendered them.
        private static readonly Dictionary<string, Band> Bands = new Dictionary<string, Band>
        {
            // Fire: smoke fringe, deep red, orange, yellow heart. No white anywhere.
            ["m2_fire_red"] = new Band(0.42f, 0.04f, 0.01f),
            ["m2_fire_orange"] = new Band(1f, 0.24f, 0.02f),
            ["m2_fire_yellow"] = new Band(1f, 0.58f, 0.07f),
            ["m2_fire_char"] = new Band(0.045f, 0.022f, 0.016f, 0.92f),
            ["m2_fire_soot"] = new Band(0.03f, 0.025f, 0.02f, 0.85f),
            ["m2_fire_heat"] = new Band(1f, 0.3f, 0.04f, 0.28f, 1f, true),
            // Lightning: violet edge, cyan band, white core kept thin.
            ["m2_bolt_violet"] = new Band(0.28f, 0.08f, 0.75f),
            ["m2_bolt_cyan"] = new Band(0.25f, 0.8f, 1f),
            ["m2_bolt_white"] = new Band(1.3f, 1.32f, 1.4f),
            ["m2_bolt_faint"] = new Band(0.4f, 0.3f, 1f, 0.5f, 1f, true),
            ["m_vfx_lightning_scorch"] = new Band(0.02f, 0.015f, 0.03f, 0.9f),
            ["m_vfx_combo_violetedge"] = new Band(0.28f, 0.08f, 0.75f),
            ["m_vfx_combo_blackcore"] = new Band(0.012f, 0.008f, 0.02f),
            // Wind: pale straw-grey, off-white, dust ochre, dry leaf; no strong hue.
            ["m2_wind_pale"] = new Band(0.82f, 0.84f, 0.78f),
            ["m2_wind_leaf"] = new Band(0.5f, 0.42f, 0.16f),
            ["m2_wind_grass"] = new Band(0.42f, 0.4f, 0.2f),
            ["m2_wind_paper"] = new Band(0.9f, 0.88f, 0.8f),
            ["m2_wind_clear"] = new Band(0.1f, 0.1f, 0.09f, 0.3f),
            // Ice: teal shade, ice cyan, pale facet, white glint on top (the four-band material adds the last two).
            ["m_vfx_ice"] = new Band(0.35f, 0.75f, 0.9f),
            ["m2_ice_line"] = new Band(0.75f, 0.93f, 1f),
            ["m2_ice_crust"] = new Band(0.75f, 0.93f, 1f),
            ["m2_ice_crack"] = new Band(0.04f, 0.16f, 0.26f),
            // Water: ink, deep sea, turquoise, foam white on crests only.
            ["m2_water_teal"] = new Band(0.08f, 0.6f, 0.62f),
            ["m2_water_foam"] = new Band(0.85f, 0.95f, 0.95f, 1f, 0.3f),
            ["m2_water_deep"] = new Band(0.02f, 0.25f, 0.38f),
            ["m2_water_wet"] = new Band(0.01f, 0.05f, 0.1f, 0.5f),
            // Life: dark green, leaf green, new growth, petal pink, pollen.
            ["m2_life_vine"] = new Band(0.12f, 0.5f, 0.08f),
            ["m2_life_new"] = new Band(0.5f, 0.8f, 0.15f),
            ["m2_life_petal"] = new Band(0.95f, 0.45f, 0.6f),
            ["m2_life_boss"] = new Band(0.95f, 0.78f, 0.2f),
            ["m2_life_crack"] = new Band(0.03f, 0.025f, 0.02f, 0.9f),
            // Earth: stone grey, ochre tops, upturned soil, near-black cracks. Nothing glows.
            ["m_vfx_earth_rock"] = new Band(0.3f, 0.27f, 0.23f),
            ["m_vfx_earth_top"] = new Band(0.62f, 0.45f, 0.2f),
            ["m2_earth_ochre"] = new Band(0.36f, 0.22f, 0.08f),
            ["m2_earth_crack"] = new Band(0.025f, 0.018f, 0.012f, 0.95f),
            // Metal: chrome stripes, bronze, pale clang arcs, white-hot and orange heads, temper colours, filings.
            ["m2_metal_chrome"] = new Band(0.45f, 0.47f, 0.5f, 1f, 0.7f),
            ["m2_metal_chromeuv"] = new Band(0.45f, 0.47f, 0.5f, 1f, 0.7f),
            ["m2_metal_chromeblade"] = new Band(0.45f, 0.47f, 0.5f, 1f, 0.7f),
            ["m2_metal_bronze"] = new Band(0.5f, 0.28f, 0.08f),
            ["m2_metal_bronzeblade"] = new Band(0.5f, 0.28f, 0.08f),
            ["m2_metal_arc"] = new Band(0.88f, 0.9f, 0.94f),
            ["m2_metal_whitehot"] = new Band(1.05f, 0.85f, 0.45f),
            ["m2_metal_hot"] = new Band(1f, 0.38f, 0.05f),
            ["m_vfx_metal_straw"] = new Band(0.85f, 0.65f, 0.25f),
            ["m_vfx_metal_blue"] = new Band(0.08f, 0.16f, 0.6f),
            ["m2_metal_filings"] = new Band(0.06f, 0.06f, 0.07f, 0.9f),
            ["m2_metal_scrape"] = new Band(0.62f, 0.62f, 0.64f),
            // Light: amber shade, gold, pale gold, white at the one hottest point; lit floor and hard shadow.
            ["m2_light_pale"] = new Band(1f, 0.88f, 0.5f),
            ["m2_light_deep"] = new Band(0.35f, 0.18f, 0.03f),
            ["m2_light_white"] = new Band(1.5f, 1.45f, 1.3f),
            ["m2_light_gold"] = new Band(0.9f, 0.62f, 0.14f),
            ["m2_light_floorlit"] = new Band(1f, 0.82f, 0.42f, 0.85f),
            ["m2_light_shadow"] = new Band(0f, 0f, 0f, 0.72f),
            ["m2_light_beam"] = new Band(1f, 0.88f, 0.55f, 0.5f, 1f, true),
            // Dark: the hole is flat black; pale rims and streaks; violet only in the ink.
            ["m2_dark_hole"] = new Band(0f, 0f, 0f),
            ["m2_dark_rim"] = new Band(0.86f, 0.84f, 0.9f),
            ["m2_dark_streak"] = new Band(0.78f, 0.76f, 0.84f),
            ["m2_dark_drain"] = new Band(0f, 0f, 0.005f, 0.45f),
            ["m2_dark_eclipse"] = new Band(0.008f, 0.004f, 0.012f, 0.9f),
            ["m2_dark_ink"] = new Band(0.02f, 0.005f, 0.03f),
            // Poison: blue-green shade, blue-green, pale jade, bile; never white, never highlighter green.
            ["m2_poison_lip"] = new Band(0.35f, 0.72f, 0.55f),
            ["m2_poison_fume"] = new Band(0.35f, 0.72f, 0.55f),
            ["m2_poison_pit"] = new Band(0.02f, 0.12f, 0.1f, 0.9f),
            ["m2_poison_pitrim"] = new Band(0.08f, 0.42f, 0.33f),
            ["m2_poison_stain"] = new Band(0.02f, 0.12f, 0.1f, 0.6f),
            ["m_vfx_poison_gas"] = new Band(0.08f, 0.42f, 0.33f, 0.35f),
            ["m_vfx_poison_bilegas"] = new Band(0.7f, 0.65f, 0.12f, 0.35f),
            ["m2_poison_bile"] = new Band(0.7f, 0.65f, 0.12f),
        };

        // The sheets: which loop, and how each is cut and seen through.
        private static readonly (string file, string name, bool loops, float cutoff, float opacity)[] Sheets =
        {
            ("Fire/vfx_fire_flame_loop_flipbook.png", "fire_flame_loop", true, 0.35f, 1f),
            ("Fire/vfx_fire_smoke_puff_flipbook.png", "fire_smoke_puff", false, 0.3f, 0.9f),
            ("Lightning/vfx_lightning_bolt_strike_flipbook.png", "lightning_bolt_strike", false, 0.3f, 1f),
            ("Wind/vfx_wind_gust_flipbook.png", "wind_gust", false, 0.3f, 1f),
            ("Ice/vfx_ice_frost_creep_flipbook.png", "ice_frost_creep", false, 0.3f, 1f),
            ("Water/vfx_water_splash_flipbook.png", "water_splash", false, 0.3f, 1f),
            ("Life/vfx_life_bloom_flipbook.png", "life_bloom", false, 0.3f, 1f),
            ("Earth/vfx_earth_dust_burst_flipbook.png", "earth_dust_burst", false, 0.3f, 1f),
            ("Metal/vfx_metal_spark_burst_flipbook.png", "metal_spark_burst", false, 0.3f, 1f),
            ("Metal/vfx_metal_clang_v2_flipbook.png", "metal_clang_v2", false, 0.3f, 1f),
            ("Light/vfx_light_flash_v2_flipbook.png", "light_flash_v2", false, 0.3f, 1f),
            ("Dark/vfx_dark_smoke_sink_flipbook.png", "dark_smoke_sink", false, 0.3f, 0.92f),
            ("Dark/vfx_dark_hole_close_v2_flipbook.png", "dark_hole_close_v2", false, 0.3f, 1f),
            // Poison's gas is seen through: the notes ask for some translucency in Unity.
            ("Poison/vfx_poison_mist_roll_flipbook.png", "poison_mist_roll", true, 0.08f, 0.7f),
            ("Poison/vfx_poison_gasfront_v2_flipbook.png", "poison_gasfront_v2", true, 0.08f, 0.75f),
        };

        private static readonly StringBuilder Report = new StringBuilder();

        [MenuItem("Clover/Build/Element Art")]
        public static void Build()
        {
            Report.Clear();
            BuildKit.EnsureFolder(Folder);
            var set = AssetDatabase.LoadAssetAtPath<ElementArtSet>(SetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ElementArtSet>();
                AssetDatabase.CreateAsset(set, SetPath);
            }
            // Last build's meshes go; the set itself (and its GUID) stays.
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(SetPath))
                if (old is Mesh)
                {
                    AssetDatabase.RemoveObjectFromAsset(old);
                    Object.DestroyImmediate(old, true);
                }

            var solids = new Material[Elements.Length];
            for (int e = 0; e < Elements.Length; e++) solids[e] = SolidMaterial(e);
            var glow = BuildKit.MaterialAt(Folder, "m_element_glow", "Clover/Flipbook");
            glow.SetTexture("_MainTex", null);
            glow.SetFloat("_Frame", -1f);
            glow.SetFloat("_Cutoff", 0f);
            glow.SetFloat("_Opacity", 1f);
            glow.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            glow.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            glow.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(glow);

            var pieces = new List<ElementArtSet.Piece>();
            var masks = new List<(string name, Texture2D texture, Color[] colours, int parts)>();
            int progress = UnityEditor.Progress.Start("Element art", "Baking the kits and their floor masks");
            try
            {
            for (int e = 0; e < Elements.Length; e++)
            {
                string element = Elements[e];
                UnityEditor.Progress.Report(progress, e / (float)Elements.Length, element);
                string kitPath = $"{KitRoot}/{char.ToUpper(element[0]) + element.Substring(1)}/vfx_{element}_kit_v2.fbx";
                var kit = AssetDatabase.LoadAssetAtPath<GameObject>(kitPath);
                if (kit == null)
                {
                    Debug.LogError($"[Clover.Build] No kit at {kitPath}.");
                    continue;
                }
                foreach (var filter in kit.GetComponentsInChildren<MeshFilter>(true))
                    BakePiece(set, filter, kit.transform, solids[e], glow, pieces, masks);
            }
            }
            finally
            {
                UnityEditor.Progress.Remove(progress);
            }

            // The masks are written and imported first, then their materials point at them.
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var mask in masks) File.WriteAllBytes($"{Folder}/{mask.name}_mask.png", mask.texture.EncodeToPNG());
                File.WriteAllBytes($"{Folder}/shadow_figure_mask.png", FigureMask().EncodeToPNG());
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            foreach (var mask in masks) AssetDatabase.ImportAsset($"{Folder}/{mask.name}_mask.png", ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset($"{Folder}/shadow_figure_mask.png", ImportAssetOptions.ForceUpdate);
            for (int i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                if (piece.Kind != ElementArtSet.Decal) continue;
                var mask = masks.Find(m => m.name == piece.Name);
                piece.Material = DecalMaterial(piece.Name, AssetDatabase.LoadAssetAtPath<Texture2D>($"{Folder}/{piece.Name}_mask.png"), mask.colours);
                pieces[i] = piece;
            }
            foreach (var mask in masks) Object.DestroyImmediate(mask.texture);

            var shadow = DecalMaterial("shadow_figure", AssetDatabase.LoadAssetAtPath<Texture2D>($"{Folder}/shadow_figure_mask.png"),
                new[] { new Color(0f, 0f, 0f, 0.82f) });
            shadow.SetFloat("_RevealMode", 3f);
            EditorUtility.SetDirty(shadow);

            var sheets = new List<ElementArtSet.Sheet>();
            foreach (var sheet in Sheets)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{KitRoot}/{sheet.file}");
                if (texture == null)
                {
                    Debug.LogError($"[Clover.Build] No sheet at {KitRoot}/{sheet.file}.");
                    continue;
                }
                var material = BuildKit.MaterialAt(Folder, "m_flipbook_" + sheet.name, "Clover/Flipbook");
                material.SetTexture("_MainTex", texture);
                material.SetFloat("_Columns", 4f);
                material.SetFloat("_Rows", 4f);
                material.SetFloat("_Frame", -1f);
                material.SetFloat("_Cutoff", sheet.cutoff);
                material.SetFloat("_Opacity", sheet.opacity);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.renderQueue = (int)RenderQueue.Transparent;
                EditorUtility.SetDirty(material);
                sheets.Add(new ElementArtSet.Sheet { Name = sheet.name, Material = material, Loops = sheet.loops });
            }

            set.Pieces = pieces.ToArray();
            set.Sheets = sheets.ToArray();
            set.Solids = solids;
            set.ShadowFigure = shadow;
            set.Label = UiArt.Font();
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int solidCount = pieces.FindAll(p => p.Kind == ElementArtSet.Solid).Count, glowCount = pieces.FindAll(p => p.Kind == ElementArtSet.Glow).Count;
            Directory.CreateDirectory("review/checks");
            File.WriteAllText("review/checks/element_art.md",
                $"# Element art\n\n{pieces.Count} pieces ({solidCount} solid, {glowCount} see-through, {masks.Count} floor marks), {sheets.Count} sheets.\n\n" +
                "| Piece | Kind | Triangles | Footprint (m) |\n|---|---|---|---|\n" + Report);
            Debug.Log($"[Clover.Build] Element art: {pieces.Count} pieces ({solidCount} solid, {glowCount} see-through, {masks.Count} marks), {sheets.Count} sheets; review/checks/element_art.md");
        }

        private static Material SolidMaterial(int element)
        {
            var material = BuildKit.MaterialAt(Folder, "m_element_solid_" + Elements[element], "Clover/Spell");
            material.SetColor("_Color", Color.white);
            material.SetFloat("_Hot", 0f);
            material.SetFloat("_OutlineWidth", 0.025f);
            material.SetColor("_OutlineColor", Inks[element].gamma);
            material.SetFloat("_Sheen", Sheens[element]);
            material.SetFloat("_Thin", 0f);
            material.SetFloat("_Bands4", Elements[element] == "ice" || Elements[element] == "light" ? 1f : 0f);
            material.SetFloat("_Chrome", Elements[element] == "metal" ? 1f : 0f);
            // Many kit pieces are single sheets (flakes, streaks, petals): both faces are drawn, or one side shows only ink.
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material DecalMaterial(string name, Texture2D mask, Color[] colours)
        {
            var material = BuildKit.MaterialAt(Folder, "m_decal_" + name, "Clover/Decal");
            material.SetTexture("_MaskTex", mask);
            string[] slots = { "_ColorR", "_ColorG", "_ColorB", "_ColorA" };
            for (int k = 0; k < slots.Length; k++)
            {
                var linear = colours != null && k < colours.Length ? colours[k] : Color.clear;
                // Material colours are given as the eye sees them; the bands are linear.
                var shown = linear.gamma;
                shown.a = linear.a;
                material.SetColor(slots[k], shown);
            }
            material.SetColor("_Color", Color.white);
            material.SetFloat("_Opacity", 1f);
            material.SetFloat("_Reveal", 1f);
            // Rings and discs are drawn on round their middle; anything else as it comes.
            material.SetFloat("_RevealMode", name.Contains("kolam") || name.Contains("dial") || name.Contains("charrings") ? 1f : name.Contains("drain") || name.Contains("stain") || name.Contains("eclipse") ? 2f : 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.renderQueue = (int)RenderQueue.Transparent - 20;
            EditorUtility.SetDirty(material);
            return material;
        }

        // "vfx2_dark_inkring_affliction.001" is "dark_inkring_affliction_1".
        public static string Clean(string name)
        {
            if (name.StartsWith("vfx2_")) name = name.Substring(5);
            int dot = name.LastIndexOf('.');
            if (dot > 0 && int.TryParse(name.Substring(dot + 1), out int number)) name = name.Substring(0, dot) + "_" + number;
            return name;
        }

        private static Band BandOf(string slot)
        {
            if (slot != null && Bands.TryGetValue(slot, out var band)) return band;
            Debug.LogWarning($"[Clover.Build] Element art: no band for the slot '{slot}'; it is drawn grey.");
            return new Band(0.5f, 0.5f, 0.5f);
        }

        private static void BakePiece(ElementArtSet set, MeshFilter filter, Transform kit, Material solid, Material glow,
            List<ElementArtSet.Piece> pieces, List<(string, Texture2D, Color[], int)> masks)
        {
            var source = filter.sharedMesh;
            if (source == null) return;
            string name = Clean(filter.name);
            var renderer = filter.GetComponent<MeshRenderer>();
            var slots = renderer != null ? renderer.sharedMaterials : new Material[0];
            // Into the kit's own space, which is the game's: Y up, metres.
            var toKit = kit.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            var vertices = source.vertices;
            var normals = source.normals;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = toKit.MultiplyPoint3x4(vertices[i]);
                if (i < normals.Length) normals[i] = toKit.MultiplyVector(normals[i]).normalized;
            }
            var bounds = new Bounds(vertices.Length > 0 ? vertices[0] : Vector3.zero, Vector3.zero);
            foreach (var v in vertices) bounds.Encapsulate(v);

            var bands = new Band[source.subMeshCount];
            bool allGlow = true;
            for (int s = 0; s < bands.Length; s++)
            {
                bands[s] = BandOf(s < slots.Length && slots[s] != null ? slots[s].name : null);
                allGlow &= bands[s].Glow;
            }
            int triangles = 0;
            for (int s = 0; s < source.subMeshCount; s++) triangles += source.GetTriangles(s).Length / 3;

            bool flat = bounds.size.y < 0.06f && !allGlow;
            if (flat)
            {
                var colours = new Color[Mathf.Min(4, bands.Length)];
                for (int s = 0; s < colours.Length; s++) colours[s] = bands[s].Colour;
                Vector3 centre = bounds.center;
                // A little margin round the footprint, so its outer edge is never cut by the quad's.
                float sizeX = Mathf.Max(0.1f, bounds.size.x * 1.04f), sizeZ = Mathf.Max(0.1f, bounds.size.z * 1.04f);
                masks.Add((name, Rasterise(source, vertices, centre, sizeX, sizeZ), colours, colours.Length));
                pieces.Add(new ElementArtSet.Piece { Name = name, Kind = ElementArtSet.Decal, Centre = new Vector3(centre.x, 0f, centre.z), Size = new Vector3(sizeX, 0f, sizeZ) });
                Report.AppendLine($"| {name} | mark | {triangles} | {sizeX:F2} x {sizeZ:F2} |");
                return;
            }

            // One submesh, its slots painted into the vertex colours. A see-through floor wash inside a solid piece (the
            // wind's cleared strip, a wet sheen) cannot be drawn by the solid material, so it is left out.
            bool anyOpaque = false;
            for (int s = 0; s < bands.Length; s++) anyOpaque |= bands[s].Colour.a >= 0.8f;
            var paint = new Color[vertices.Length];
            for (int i = 0; i < paint.Length; i++) paint[i] = Color.white;
            var all = new List<int>();
            for (int s = 0; s < source.subMeshCount; s++)
            {
                var tris = source.GetTriangles(s);
                var band = bands[s];
                if (!allGlow && anyOpaque && band.Colour.a < 0.8f)
                {
                    Report.AppendLine($"| {name} (slot {s}, see-through, left out) | - | {tris.Length / 3} | - |");
                    continue;
                }
                // Clover/Spell reads the alpha as a flag; a see-through shape keeps its band's alpha.
                var colour = band.Colour;
                colour.a = allGlow ? band.Colour.a : band.Flag;
                foreach (int index in tris) paint[index] = colour;
                all.AddRange(tris);
            }
            var mesh = new Mesh { name = "mesh_" + name };
            if (vertices.Length > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors = paint;
            mesh.SetTriangles(all, 0);
            mesh.SetUVs(1, SmoothNormals(vertices, normals));
            mesh.RecalculateBounds();
            AssetDatabase.AddObjectToAsset(mesh, set);
            pieces.Add(new ElementArtSet.Piece
            {
                Name = name, Kind = allGlow ? ElementArtSet.Glow : ElementArtSet.Solid, Mesh = mesh, Material = allGlow ? glow : solid,
                Centre = bounds.center, Size = bounds.size,
            });
            Report.AppendLine($"| {name} | {(allGlow ? "see-through" : "solid")} | {triangles} | {bounds.size.x:F2} x {bounds.size.z:F2}, {bounds.size.y:F2} tall |");
        }

        // The ink line is pushed out along normals averaged over every face meeting at a point, so it stays closed at corners.
        private static List<Vector3> SmoothNormals(Vector3[] vertices, Vector3[] normals)
        {
            var sums = new Dictionary<Vector3Int, Vector3>();
            var keys = new Vector3Int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                keys[i] = Vector3Int.RoundToInt(vertices[i] * 10000f);
                sums.TryGetValue(keys[i], out var sum);
                sums[keys[i]] = sum + (i < normals.Length ? normals[i] : Vector3.up);
            }
            var smooth = new List<Vector3>(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                var n = sums[keys[i]];
                smooth.Add(n.sqrMagnitude > 1e-6f ? n.normalized : (i < normals.Length ? normals[i] : Vector3.up));
            }
            return smooth;
        }

        /// <summary>
        /// Draws a flat piece from above into a mask: each slot's triangles into its own channel (red, green, blue, alpha),
        /// sampled nine times a pixel so a thin line keeps its width. u runs along +X, v along +Z, over the footprint given.
        /// </summary>
        private static Texture2D Rasterise(Mesh source, Vector3[] vertices, Vector3 centre, float sizeX, float sizeZ)
        {
            int longest = Mathf.Max(sizeX, sizeZ) > 4.5f ? 1024 : 512;
            int width = sizeX >= sizeZ ? longest : Mathf.Max(64, Mathf.NextPowerOfTwo(Mathf.CeilToInt(longest * sizeX / sizeZ)));
            int height = sizeZ >= sizeX ? longest : Mathf.Max(64, Mathf.NextPowerOfTwo(Mathf.CeilToInt(longest * sizeZ / sizeX)));
            var cover = new float[4, width * height];
            float left = centre.x - sizeX * 0.5f, bottom = centre.z - sizeZ * 0.5f;
            const int samples = 3;
            for (int s = 0; s < source.subMeshCount; s++)
            {
                int channel = Mathf.Min(3, s);
                var tris = source.GetTriangles(s);
                for (int t = 0; t < tris.Length; t += 3)
                {
                    Vector2 a = Pixel(vertices[tris[t]]), b = Pixel(vertices[tris[t + 1]]), c = Pixel(vertices[tris[t + 2]]);
                    int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
                    int x1 = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
                    int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
                    int y1 = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
                    float area = Edge(a, b, c);
                    if (Mathf.Abs(area) < 1e-6f) continue;
                    for (int y = y0; y <= y1; y++)
                        for (int x = x0; x <= x1; x++)
                        {
                            int inside = 0;
                            for (int sy = 0; sy < samples; sy++)
                                for (int sx = 0; sx < samples; sx++)
                                {
                                    var p = new Vector2(x + (sx + 0.5f) / samples, y + (sy + 0.5f) / samples);
                                    float w0 = Edge(b, c, p) / area, w1 = Edge(c, a, p) / area, w2 = Edge(a, b, p) / area;
                                    if (w0 >= 0f && w1 >= 0f && w2 >= 0f) inside++;
                                }
                            if (inside == 0) continue;
                            int index = y * width + x;
                            cover[channel, index] = Mathf.Min(1f, cover[channel, index] + inside / (float)(samples * samples));
                        }
                }
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(cover[0, i], cover[1, i], cover[2, i], cover[3, i]);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;

            Vector2 Pixel(Vector3 v) => new Vector2((v.x - left) / sizeX * width, (v.z - bottom) / sizeZ * height);
        }

        private static float Edge(Vector2 a, Vector2 b, Vector2 p) => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);

        /// <summary>
        /// A standing body's shadow laid flat: feet at the bottom (v = 0), head at the top, legs apart, arms at the sides.
        /// Light's thrown shadows and Dark's reaching ones are this, stretched along the way they fall.
        /// </summary>
        private static Texture2D FigureMask()
        {
            const int width = 128, height = 256;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float u = (x + 0.5f) / width, v = (y + 0.5f) / height;
                    float inside = 0f;
                    // Two legs, a body, two arms, a head: capsules, in the quad's own 0..1.
                    inside = Mathf.Max(inside, Capsule(u, v, 0.36f, 0.02f, 0.42f, 0.42f, 0.075f));
                    inside = Mathf.Max(inside, Capsule(u, v, 0.64f, 0.02f, 0.58f, 0.42f, 0.075f));
                    inside = Mathf.Max(inside, Capsule(u, v, 0.5f, 0.4f, 0.5f, 0.7f, 0.16f));
                    inside = Mathf.Max(inside, Capsule(u, v, 0.3f, 0.7f, 0.18f, 0.42f, 0.06f));
                    inside = Mathf.Max(inside, Capsule(u, v, 0.7f, 0.7f, 0.82f, 0.42f, 0.06f));
                    inside = Mathf.Max(inside, Capsule(u, v, 0.5f, 0.86f, 0.5f, 0.86f, 0.12f));
                    pixels[y * width + x] = new Color(inside, 0f, 0f, 0f);
                }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        // 1 inside a capsule from (ax, ay) to (bx, by) of the radius given (the u axis counted half as wide as v).
        private static float Capsule(float u, float v, float ax, float ay, float bx, float by, float radius)
        {
            var p = new Vector2(u * 0.5f, v);
            var a = new Vector2(ax * 0.5f, ay);
            var b = new Vector2(bx * 0.5f, by);
            var ab = b - a;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            float distance = (p - (a + ab * t)).magnitude;
            return distance <= radius * 0.5f ? 1f : 0f;
        }

        /// <summary>The set, for builders that put it in a scene; null before Clover > Build > Element Art has run.</summary>
        public static ElementArtSet Set() => AssetDatabase.LoadAssetAtPath<ElementArtSet>(SetPath);

        // The scenes made by builders that hold the player and so the spell works.
        private static readonly string[] Scenes = { "Hub", "Castle", "Summit", "CombatTest", "SunCity", "SunCourt", "ProvingGround" };

        /// <summary>
        /// Puts the element art into every built scene that has spell works, beside them, without rebuilding the scene;
        /// a scene that already holds it is only handed the set again. The open scene is opened again after.
        /// </summary>
        [MenuItem("Clover/Build/Element Art Into Scenes")]
        public static void IntoScenes()
        {
            var set = Set();
            if (set == null)
            {
                Debug.LogError("[Clover.Build] Build the element art first (Clover > Build > Element Art).");
                return;
            }
            string was = EditorSceneManager.GetActiveScene().path;
            // Never ask (this runs unattended): an open scene with unsaved changes is left alone.
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                Debug.LogError("[Clover.Build] The open scene has unsaved changes; save or discard them, then put the element art into the scenes.");
                return;
            }
            int placed = 0;
            foreach (string scene in Scenes)
            {
                string path = $"Assets/_Project/Scenes/{scene}.unity";
                if (!File.Exists(path)) continue;
                var opened = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var works = Object.FindAnyObjectByType<FormFx>(FindObjectsInactive.Include);
                if (works == null) continue;
                var art = Object.FindAnyObjectByType<ElementArt>(FindObjectsInactive.Include);
                if (art == null) art = works.gameObject.AddComponent<ElementArt>();
                art.Configure(set);
                EditorUtility.SetDirty(art);
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
                placed++;
            }
            if (!string.IsNullOrEmpty(was)) EditorSceneManager.OpenScene(was, OpenSceneMode.Single);
            Debug.Log($"[Clover.Build] Element art put into {placed} scenes.");
        }
    }
}
