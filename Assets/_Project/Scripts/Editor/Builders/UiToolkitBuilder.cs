using System.IO;
using Clover.Player;
using Clover.UI.Toolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Clover.EditorTools
{
    /// <summary>
    /// The menus' UI Toolkit set-up (docs/design/ui_rest_fire_plan.md section 7, the editor step): the two faces as
    /// TextCore font assets under the names Fonts.uss points at, the menus' PanelSettings (scaled with the screen from
    /// 1920 by 1080, Expand, theme Clover.tss, above the HUD), the Menus object with the Loadout's UIDocument and
    /// LoadoutScreen in every scene that has a player, and Controls as the project-wide actions.
    /// </summary>
    public static class UiToolkitBuilder
    {
        public const string Folder = "Assets/_Project/Generated/UI";
        public const string FontFolder = Folder + "/Fonts";
        public const string PanelPath = Folder + "/MenuPanel.asset";
        public const string ReadingFont = FontFolder + "/AtkinsonHyperlegible_SDF.asset";
        public const string ReadingBoldFont = FontFolder + "/AtkinsonHyperlegibleBold_SDF.asset";
        public const string TitleFont = FontFolder + "/SpectralSemiBold_SDF.asset";
        public const string ThemePath = "Assets/_Project/UI/Theme/Clover.tss";
        public const string LoadoutUxml = "Assets/_Project/UI/Loadout/Loadout.uxml";
        public const string ControlsPath = "Assets/_Project/Settings/Controls.inputactions";
        public const string GlyphFolder = "Assets/_Project/Art/Textures/UI/Glyphs";
        public const string GlyphPicturesPath = Folder + "/GlyphPictures.asset";
        /// <summary>The reading face as a plain font, for the HUD's words drawn by the old Text (the affliction marks).</summary>
        public const string HudReadingFont = "Assets/_Project/Art/Fonts/atkinson_hyperlegible_bold.ttf";
        /// <summary>The menus draw over the HUD canvas.</summary>
        public const float MenuSorting = 100f;

        [MenuItem("Clover/Build/UI Toolkit")]
        public static void BuildMenu()
        {
            Build();
            int placed = PlaceMenusInPlayerScenes();
            Debug.Log($"[Clover.Build] UI Toolkit: fonts, {PanelPath}, project-wide actions, Menus in {placed} scenes");
        }

        /// <summary>The fonts, the panel and the project-wide actions (no scene is touched).</summary>
        public static void Build()
        {
            EnsureFolder(FontFolder);
            var bold = MakeFont("Assets/_Project/Art/Fonts/atkinson_hyperlegible_bold.ttf", ReadingBoldFont, null);
            MakeFont("Assets/_Project/Art/Fonts/atkinson_hyperlegible_regular.ttf", ReadingFont, bold);
            MakeFont("Assets/_Project/Art/Fonts/spectral_semibold.ttf", TitleFont, null);
            MakePanel();
            MakeGlyphPictures();
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (actions != null && InputSystem.actions != actions) InputSystem.actions = actions;
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
        }

        // A dynamic SDF font asset of the .ttf, made again from scratch under its stable name; the bold weight set when given.
        private static FontAsset MakeFont(string ttf, string path, FontAsset bold)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(ttf);
            if (font == null)
            {
                Debug.LogWarning($"[Clover.Build] UI Toolkit: no font at {ttf}");
                return null;
            }
            if (AssetDatabase.LoadAssetAtPath<FontAsset>(path) != null) AssetDatabase.DeleteAsset(path);
            var asset = FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            asset.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(asset, path);
            foreach (var atlas in asset.atlasTextures)
            {
                if (atlas == null) continue;
                atlas.name = asset.name + " Atlas";
                AssetDatabase.AddObjectToAsset(atlas, asset);
            }
            if (asset.material != null)
            {
                asset.material.name = asset.name + " Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }
            // Bold reading text is the Bold face, picked by -unity-font-style: bold (weight 700 in the weight table).
            if (bold != null && asset.fontWeightTable != null && asset.fontWeightTable.Length > 7) asset.fontWeightTable[7].regularTypeface = bold;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        // Kenney's pictures under their control ids (GlyphPictures.IdOfFile), kept under a stable name so scenes keep it.
        public static GlyphPictures MakeGlyphPictures()
        {
            var pictures = AssetDatabase.LoadAssetAtPath<GlyphPictures>(GlyphPicturesPath);
            if (pictures == null)
            {
                pictures = ScriptableObject.CreateInstance<GlyphPictures>();
                AssetDatabase.CreateAsset(pictures, GlyphPicturesPath);
            }
            var ids = new System.Collections.Generic.List<string>();
            var textures = new System.Collections.Generic.List<Texture2D>();
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { GlyphFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string id = GlyphPictures.IdOfFile(Path.GetFileNameWithoutExtension(path));
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (id.Length == 0 || texture == null) continue;
                ids.Add(id);
                textures.Add(texture);
            }
            pictures.Set(ids.ToArray(), textures.ToArray());
            EditorUtility.SetDirty(pictures);
            return pictures;
        }

        private static void MakePanel()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelPath);
            }
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1920, 1080);
            panel.screenMatchMode = PanelScreenMatchMode.Expand;
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            panel.sortingOrder = MenuSorting;
            EditorUtility.SetDirty(panel);
        }

        /// <summary>
        /// The Menus object in the open scene (called by PlayerBuilder as it builds a player in): the Loadout's UIDocument
        /// on the menus' panel and its LoadoutScreen, made again if there is one already.
        /// </summary>
        public static GameObject PlaceMenus()
        {
            var old = GameObject.Find("Menus");
            if (old != null) Object.DestroyImmediate(old);
            var menus = new GameObject("Menus");
            var document = menus.AddComponent<UIDocument>();
            document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelPath);
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LoadoutUxml);
            var screen = menus.AddComponent<LoadoutScreen>();
            var serialized = new SerializedObject(screen);
            serialized.FindProperty("glyphPictures").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GlyphPictures>(GlyphPicturesPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            // The HUD's affliction marks read in the same face as the menus.
            foreach (var hud in Object.FindObjectsByType<Clover.UI.PlayerHud>(FindObjectsInactive.Include))
            {
                var hudSerialized = new SerializedObject(hud);
                hudSerialized.FindProperty("markFont").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>(HudReadingFont);
                hudSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
            return menus;
        }

        /// <summary>Every scene that has a player gets its Menus object; returns how many.</summary>
        public static int PlaceMenusInPlayerScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return 0;
            string was = EditorSceneManager.GetActiveScene().path;
            int placed = 0;
            // Every scene under Scenes, in the build or not (the Proving Ground is a work scene).
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" }))
            {
                var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid), OpenSceneMode.Single);
                if (Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include) == null) continue;
                PlaceMenus();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                placed++;
            }
            if (!string.IsNullOrEmpty(was)) EditorSceneManager.OpenScene(was, OpenSceneMode.Single);
            return placed;
        }
    }
}
