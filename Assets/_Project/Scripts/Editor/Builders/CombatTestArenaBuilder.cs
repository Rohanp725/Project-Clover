using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the grey-box arena where combat feel is tried out: a floor, pillars to test the camera near tall things, the
    /// player, the top-down camera and a stamina bar. Rebuilding replaces the scene and reuses the materials by name.
    /// </summary>
    public static class CombatTestArenaBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/CombatTest.unity";
        public const string ControlsPath = "Assets/_Project/Settings/Controls.inputactions";
        public const string PlayerModelPath = "Assets/_Project/Art/Models/Characters/player.fbx";
        public const string ToonShaderName = "Clover/Toon";
        private const string GeneratedFolder = "Assets/_Project/Generated/CombatTest";
        private const string PlayerFolder = "Assets/_Project/Generated/Player";

        // The character's colours as authored in Blender (linear values), by the name behind "m_player_" in the model.
        private static readonly (string name, Color linear)[] PlayerPalette =
        {
            ("skin", new Color(0.70f, 0.46f, 0.33f)),
            ("jacket", new Color(0.075f, 0.10f, 0.155f)),
            ("trouser", new Color(0.10f, 0.09f, 0.095f)),
            ("wrap", new Color(0.58f, 0.53f, 0.43f)),
            ("leather", new Color(0.20f, 0.12f, 0.075f)),
            ("iron", new Color(0.06f, 0.065f, 0.08f)),
            ("accent", new Color(0.58f, 0.13f, 0.06f)),
            ("hair", new Color(0.035f, 0.03f, 0.04f)),
            ("straw", new Color(0.55f, 0.43f, 0.22f)),
        };


        [MenuItem("Clover/Build/Combat Test Arena")]
        public static void Build()
        {
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (controls == null)
            {
                Debug.LogError($"[Clover.Build] No input actions at {ControlsPath}; the arena was not built.");
                return;
            }

            // Building replaces the open scene, so unsaved work in it would be lost without a word.
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                if (!EditorSceneManager.GetSceneAt(i).isDirty) continue;
                Debug.LogError("[Clover.Build] The open scene has unsaved changes; save or discard them, then build the arena again.");
                return;
            }

            EnsureFolder(GeneratedFolder);
            EnsureFolder(PlayerFolder);
            // Materials of the blocky stand-in figure this builder used to make.
            foreach (string old in new[] { "PlayerBody", "PlayerDark", "PlayerFront" })
                AssetDatabase.DeleteAsset($"{GeneratedFolder}/{old}.mat");
            // The world is muted on purpose (docs/GAME.md): only elements will carry strong colour.
            var ground = Material("Ground", new Color(0.22f, 0.23f, 0.26f));
            var stone = Material("Stone", new Color(0.33f, 0.34f, 0.38f));
            ground.SetTexture("_BaseMap", FloorTiles());
            // One tile of the checker is two metres, so a roll of 4.5 m crosses a little over two tiles.
            ground.SetTextureScale("_BaseMap", new Vector2(11f, 11f));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Box("Ground", new Vector3(0f, -0.5f, 0f), new Vector3(44f, 1f, 44f), ground);
            Box("Pillar North East", new Vector3(7f, 2.5f, 7f), new Vector3(2f, 5f, 2f), stone);
            Box("Pillar North West", new Vector3(-7f, 2.5f, 7f), new Vector3(2f, 5f, 2f), stone);
            Box("Pillar South East", new Vector3(7f, 2.5f, -7f), new Vector3(2f, 5f, 2f), stone);
            Box("Pillar South West", new Vector3(-7f, 2.5f, -7f), new Vector3(2f, 5f, 2f), stone);
            // Walls are low on the camera's side so they never hide the player.
            Box("Wall North", new Vector3(0f, 2f, 21f), new Vector3(44f, 4f, 2f), stone);
            Box("Wall South", new Vector3(0f, 0.5f, -21f), new Vector3(44f, 1f, 2f), stone);
            Box("Wall East", new Vector3(21f, 1f, 0f), new Vector3(2f, 2f, 40f), stone);
            Box("Wall West", new Vector3(-21f, 1f, 0f), new Vector3(2f, 2f, 40f), stone);

            var lightObject = new GameObject("Key Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.8f, 0.86f, 1f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.25f, 0.27f, 0.33f);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 40f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<TopDownCamera>();

            var player = BuildPlayer();
            var controller = player.GetComponent<PlayerController>();
            controller.Configure(controls, cameraObject.transform);
            follow.Configure(player.transform);
            follow.SnapToTarget();

            BuildStaminaBar(controller);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError($"[Clover.Build] The arena could not be saved to {ScenePath}.");
                return;
            }
            PutFirstInBuild(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Clover.Build] Combat test arena built at {ScenePath}.");
        }

        private static GameObject BuildPlayer()
        {
            var player = new GameObject("Player");
            var capsule = player.AddComponent<CharacterController>();
            capsule.height = 1.8f;
            capsule.radius = 0.4f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            var controller = player.AddComponent<PlayerController>();

            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
            if (modelAsset == null)
            {
                Debug.LogError($"[Clover.Build] No character model at {PlayerModelPath}; the player was built without a body.");
                return player;
            }

            var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            model.name = "Model";
            model.transform.SetParent(player.transform, false);

            var materials = PlayerMaterials();
            var skinned = model.GetComponentInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => ToonFor(imported, materials)).ToArray();

            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = PlayerAnimatorController();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            player.AddComponent<PlayerAnimator>().Configure(controller, animator);
            return player;
        }

        // The model's own materials only carry names; each is replaced by the toon material of the same name.
        private static Material ToonFor(Material imported, Dictionary<string, Material> materials)
        {
            string name = imported != null ? imported.name.Replace("m_player_", "") : "";
            if (materials.TryGetValue(name, out var toon)) return toon;
            Debug.LogWarning($"[Clover.Build] The character uses a material '{name}' that the palette does not know.");
            return imported;
        }

        private static Dictionary<string, Material> PlayerMaterials()
        {
            var shader = Shader.Find(ToonShaderName);
            if (shader == null) Debug.LogError($"[Clover.Build] Shader {ToonShaderName} was not found; the character will be pink.");
            var materials = new Dictionary<string, Material>();
            foreach (var (name, linear) in PlayerPalette)
            {
                string path = $"{PlayerFolder}/m_player_{name}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, path);
                }
                material.shader = shader;
                // Material colours are stored as the eye sees them; Blender's numbers are light values.
                material.SetColor("_BaseColor", linear.gamma);
                EditorUtility.SetDirty(material);
                materials[name] = material;
            }
            return materials;
        }

        private static AnimatorController PlayerAnimatorController()
        {
            string path = $"{PlayerFolder}/PlayerAnimator.controller";
            AssetDatabase.DeleteAsset(path);
            var clips = AssetDatabase.LoadAllAssetsAtPath(PlayerModelPath).OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__")).ToDictionary(clip => clip.name);
            foreach (string needed in new[] { "idle", "run", "roll" })
                if (!clips.ContainsKey(needed)) Debug.LogError($"[Clover.Build] The character model has no '{needed}' clip.");

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter(PlayerAnimator.SpeedParameter, AnimatorControllerParameterType.Float);
            var machine = controller.layers[0].stateMachine;

            var locomotion = controller.CreateBlendTreeInController(PlayerAnimator.LocomotionState, out BlendTree tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = PlayerAnimator.SpeedParameter;
            tree.useAutomaticThresholds = false;
            if (clips.TryGetValue("idle", out var idle)) tree.AddChild(idle, 0f);
            if (clips.TryGetValue("run", out var run)) tree.AddChild(run, 1f);
            machine.defaultState = locomotion;

            var roll = machine.AddState(PlayerAnimator.RollState);
            if (clips.TryGetValue("roll", out var rollClip)) roll.motion = rollClip;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static Texture2D FloorTiles()
        {
            string path = $"{GeneratedFolder}/FloorTiles.asset";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = "FloorTiles" };
                AssetDatabase.CreateAsset(texture, path);
            }
            // Two close greys in a checker: enough to see ground go by without the floor competing with the fight.
            var light = new Color(1f, 1f, 1f);
            var shade = new Color(0.82f, 0.82f, 0.84f);
            texture.SetPixels(new[] { light, shade, shade, light });
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();
            EditorUtility.SetDirty(texture);
            return texture;
        }

        private static void BuildStaminaBar(PlayerController player)
        {
            var canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var back = Panel("Stamina Back", canvasObject.transform, new Color(0f, 0f, 0f, 0.6f));
            back.anchorMin = back.anchorMax = back.pivot = new Vector2(0f, 1f);
            back.anchoredPosition = new Vector2(48f, -48f);
            back.sizeDelta = new Vector2(360f, 18f);

            var fill = Panel("Stamina Fill", back, new Color(0.45f, 0.85f, 0.4f));
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;

            canvasObject.AddComponent<StaminaBar>().Configure(player, fill);
        }

        private static RectTransform Panel(string name, Transform parent, Color colour)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = colour;
            image.raycastTarget = false;
            return (RectTransform)panel.transform;
        }

        private static void Box(string name, Vector3 position, Vector3 size, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = position;
            box.transform.localScale = size;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            box.isStatic = true;
        }

        private static Material Material(string name, Color colour)
        {
            string path = $"{GeneratedFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", colour);
            material.SetFloat("_Smoothness", 0.1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }

        private static void PutFirstInBuild(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(scenePath, true) };
            scenes.AddRange(EditorBuildSettings.scenes.Where(existing => existing.path != scenePath));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
