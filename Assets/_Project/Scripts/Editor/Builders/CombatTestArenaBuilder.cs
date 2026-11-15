using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.UI;
using UnityEditor;
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
        private const string GeneratedFolder = "Assets/_Project/Generated/CombatTest";

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
            // The world is muted on purpose (docs/GAME.md): only elements will carry strong colour.
            var ground = Material("Ground", new Color(0.22f, 0.23f, 0.26f));
            var stone = Material("Stone", new Color(0.33f, 0.34f, 0.38f));
            var playerBody = Material("PlayerBody", new Color(0.78f, 0.74f, 0.66f));
            var playerDark = Material("PlayerDark", new Color(0.16f, 0.15f, 0.17f));
            ground.SetTexture("_BaseMap", FloorTiles());
            // One tile of the checker is two metres, so a roll of 4.5 m crosses a little over two tiles.
            ground.SetTextureScale("_BaseMap", new Vector2(11f, 11f));
            var playerFront = Material("PlayerFront", new Color(0.95f, 0.45f, 0.12f));

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

            var player = BuildPlayer(playerBody, playerDark, playerFront);
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

        private static GameObject BuildPlayer(Material body, Material dark, Material front)
        {
            var player = new GameObject("Player");
            var capsule = player.AddComponent<CharacterController>();
            capsule.height = 1.8f;
            capsule.radius = 0.4f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            var controller = player.AddComponent<PlayerController>();

            // A blocky stand-in until there is a real character. Everything hangs off a pivot at hip height so the roll
            // can spin the whole figure about its middle.
            var pivot = Empty("Figure", player.transform, new Vector3(0f, 0.92f, 0f));
            Part("Torso", pivot, new Vector3(0f, 0.32f, 0f), new Vector3(0.5f, 0.62f, 0.28f), body);
            Part("Head", pivot, new Vector3(0f, 0.8f, 0f), new Vector3(0.3f, 0.3f, 0.3f), body);
            // Strong colour only on the front, so facing reads from above.
            Part("Headband", pivot, new Vector3(0f, 0.84f, 0.02f), new Vector3(0.33f, 0.08f, 0.34f), front);
            Part("Sash", pivot, new Vector3(0f, 0.3f, 0.13f), new Vector3(0.34f, 0.5f, 0.06f), front);
            var sheath = Part("Sheath", pivot, new Vector3(-0.3f, 0.02f, -0.12f), new Vector3(0.06f, 0.07f, 0.85f), dark);
            sheath.localRotation = Quaternion.Euler(-18f, 8f, 0f);

            var leftArm = Limb("Left Arm", pivot, new Vector3(-0.33f, 0.56f, 0f), new Vector3(0.14f, 0.56f, 0.14f), body);
            var rightArm = Limb("Right Arm", pivot, new Vector3(0.33f, 0.56f, 0f), new Vector3(0.14f, 0.56f, 0.14f), body);
            var leftLeg = Limb("Left Leg", pivot, new Vector3(-0.14f, 0f, 0f), new Vector3(0.18f, 0.9f, 0.18f), dark);
            var rightLeg = Limb("Right Leg", pivot, new Vector3(0.14f, 0f, 0f), new Vector3(0.18f, 0.9f, 0.18f), dark);

            player.AddComponent<PlayerFigure>().Configure(controller, pivot, leftArm, rightArm, leftLeg, rightLeg);
            return player;
        }

        private static Transform Empty(string name, Transform parent, Vector3 localPosition)
        {
            var empty = new GameObject(name).transform;
            empty.SetParent(parent, false);
            empty.localPosition = localPosition;
            return empty;
        }

        private static Transform Part(string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = size;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
            return part.transform;
        }

        // A limb turns about its top end, so it gets its own joint with the block hanging below it.
        private static Transform Limb(string name, Transform parent, Vector3 jointPosition, Vector3 size, Material material)
        {
            var joint = Empty(name, parent, jointPosition);
            Part(name + " Block", joint, new Vector3(0f, -size.y * 0.5f, 0f), size, material);
            return joint;
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
