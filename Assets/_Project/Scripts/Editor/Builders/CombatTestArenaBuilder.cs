using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
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
    /// player, the top-down camera, the player's bars, dummies to hit and one dummy that hits back. Rebuilding replaces the scene and reuses the materials by name.
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
            ("steel", new Color(0.72f, 0.76f, 0.82f)),
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

            var player = BuildPlayer(follow);
            var controller = player.GetComponent<PlayerController>();
            controller.Configure(controls, cameraObject.transform);
            follow.Configure(player.transform);
            follow.SnapToTarget();

            BuildHud(controller, player.GetComponent<PlayerCombat>(), player.GetComponent<PlayerDefence>());

            // One dummy alone, and a pair close together to see a swing catch both.
            var wood = ToonMaterial("DummyWood", new Color(0.42f, 0.33f, 0.24f));
            var straw = ToonMaterial("DummyStraw", new Color(0.66f, 0.56f, 0.34f));
            var barBack = UnlitMaterial("BarBack", new Color(0.03f, 0.03f, 0.04f));
            var barHealth = UnlitMaterial("BarHealth", new Color(0.78f, 0.14f, 0.12f));
            var barStagger = UnlitMaterial("BarStagger", new Color(0.96f, 0.76f, 0.2f));
            foreach (var position in new[] { new Vector3(0f, 0f, 4f), new Vector3(5f, 0f, -2f), new Vector3(6.5f, 0f, -1.1f) })
                BuildDummy(position, cameraObject.transform.rotation, wood, straw, barBack, barHealth, barStagger, null);
            // And one that hits back, off to the west, to practise the guard and the dodge on. It leaves the player
            // alone until they walk up to it.
            BuildDummy(new Vector3(-7f, 0f, 3.5f), cameraObject.transform.rotation, wood, straw, barBack, barHealth, barStagger, player.transform);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError($"[Clover.Build] The arena could not be saved to {ScenePath}.");
                return;
            }
            PutFirstInBuild(ScenePath);
            AssetDatabase.SaveAssets();
            // Open the saved scene afresh. Entering Play Mode straight from the scene as it was built in memory lost the
            // player's reference to the controls asset every time; the same scene loaded from disk never did.
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log($"[Clover.Build] Combat test arena built at {ScenePath}.");
        }

        private static GameObject BuildPlayer(TopDownCamera follow)
        {
            var player = new GameObject("Player");
            var capsule = player.AddComponent<CharacterController>();
            capsule.height = 1.8f;
            capsule.radius = 0.4f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            var controller = player.AddComponent<PlayerController>();
            var combat = player.AddComponent<PlayerCombat>();
            var defence = player.AddComponent<PlayerDefence>();
            var hitStop = player.AddComponent<HitStop>();
            combat.Configure(hitStop, follow);
            defence.Configure(hitStop, follow);

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
            animator.runtimeAnimatorController = PlayerAnimatorController(modelAsset);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var drawnSword = FindDeep(model.transform, "katana_drawn");
            var sheathedHilt = FindDeep(model.transform, "katana_hilt");
            if (drawnSword == null || sheathedHilt == null)
                Debug.LogWarning("[Clover.Build] The character model has no katana_drawn or katana_hilt object; the sword will not move between sheath and hand.");
            player.AddComponent<PlayerAnimator>().Configure(controller, combat, defence, animator,
                drawnSword != null ? drawnSword.gameObject : null, sheathedHilt != null ? sheathedHilt.gameObject : null);
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

        private static AnimatorController PlayerAnimatorController(GameObject modelAsset)
        {
            string path = $"{PlayerFolder}/PlayerAnimator.controller";
            AssetDatabase.DeleteAsset(path);
            var clips = AssetDatabase.LoadAllAssetsAtPath(PlayerModelPath).OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__")).ToDictionary(clip => clip.name);
            foreach (string needed in new[] { "idle", "run", "roll", PlayerAnimator.ChargeState, PlayerAnimator.HurtState, PlayerAnimator.GuardState, PlayerAnimator.DrinkState })
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

            // One state per attack clip, named after the clip; AttackStep.Name picks the state.
            foreach (var attack in clips.Values.Where(clip => clip.name.StartsWith("attack_")).OrderBy(clip => clip.name))
                machine.AddState(attack.name).motion = attack;
            var katana = PlayerCombat.Katana();
            foreach (var step in katana.Steps.Concat(new[] { katana.Heavy, katana.ChainHeavy, katana.Skill }))
                if (!clips.ContainsKey(step.Name)) Debug.LogWarning($"[Clover.Build] The character model has no '{step.Name}' clip; that attack will not be animated.");

            foreach (string name in new[] { PlayerAnimator.ChargeState, PlayerAnimator.HurtState })
                machine.AddState(name).motion = clips.TryGetValue(name, out var clip) ? clip : null;

            // The guard and the drink move the body from the spine up and leave the legs to walk.
            string maskPath = $"{PlayerFolder}/UpperBody.mask";
            AssetDatabase.DeleteAsset(maskPath);
            var mask = new AvatarMask();
            mask.AddTransformPath(modelAsset.transform, true);
            for (int i = 0; i < mask.transformCount; i++)
                mask.SetTransformActive(i, ("/" + mask.GetTransformPath(i) + "/").Contains("/spine/"));
            AssetDatabase.CreateAsset(mask, maskPath);
            var upperMachine = new AnimatorStateMachine { name = PlayerAnimator.UpperBodyLayer, hideFlags = HideFlags.HideInHierarchy };
            AssetDatabase.AddObjectToAsset(upperMachine, controller);
            controller.AddLayer(new AnimatorControllerLayer
            {
                name = PlayerAnimator.UpperBodyLayer,
                // The player's animator raises the weight while guarding or drinking.
                defaultWeight = 0f,
                avatarMask = mask,
                blendingMode = AnimatorLayerBlendingMode.Override,
                stateMachine = upperMachine,
            });
            upperMachine.defaultState = upperMachine.AddState(PlayerAnimator.UpperIdleState);
            foreach (string name in new[] { PlayerAnimator.GuardState, PlayerAnimator.DrinkState })
                upperMachine.AddState(name).motion = clips.TryGetValue(name, out var clip) ? clip : null;
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

        private static void BuildHud(PlayerController player, PlayerCombat combat, PlayerDefence defence)
        {
            var canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            Transform hud = canvasObject.transform;

            var health = Bar("Health", hud, new Vector2(48f, -40f), new Vector2(420f, 20f), new Color(0.78f, 0.14f, 0.12f));
            // The grey part sits behind the red and shows where a guarded blow took health that can still be won back.
            var grey = Panel("Health Grey", health.parent, new Color(0.74f, 0.7f, 0.66f));
            grey.anchorMin = Vector2.zero;
            grey.anchorMax = Vector2.one;
            grey.offsetMin = grey.offsetMax = Vector2.zero;
            grey.SetSiblingIndex(0);
            var stamina = Bar("Stamina", hud, new Vector2(48f, -68f), new Vector2(360f, 14f), new Color(0.45f, 0.85f, 0.4f));
            // Mana takes the colour of the player's element; fire for now.
            var mana = Bar("Mana", hud, new Vector2(48f, -90f), new Vector2(300f, 12f), new Color(1f, 0.5f, 0.1f));

            var flasks = new RectTransform[new FlaskBelt().Maximum];
            for (int i = 0; i < flasks.Length; i++)
                flasks[i] = Bar("Flask " + (i + 1), hud, new Vector2(48f + i * 30f, -112f), new Vector2(22f, 30f), new Color(0.95f, 0.76f, 0.32f));

            var callout = Label("Callout", hud, 40, TextAnchor.MiddleCenter, Color.white);
            callout.rectTransform.anchorMin = callout.rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
            callout.rectTransform.sizeDelta = new Vector2(900f, 60f);
            callout.text = "";

            // The arena is for testing, so it says what the buttons are.
            var hint = Label("Controls", hud, 20, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.55f));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = hint.rectTransform.pivot = Vector2.zero;
            hint.rectTransform.anchoredPosition = new Vector2(48f, 36f);
            hint.rectTransform.sizeDelta = new Vector2(1500f, 60f);
            hint.text = "R1 light attack    R2 heavy attack, hold then release for the Iaido Slash (30 mana)    L1 guard    Circle dodge    Square flask\n"
                + "Keyboard: WASD move    J light    K heavy    L guard    Space dodge    R flask";

            canvasObject.AddComponent<PlayerHud>().Configure(player, combat, defence, health, grey, stamina, mana, flasks, callout);
        }

        private static Text Label(string name, Transform parent, int size, TextAnchor anchor, Color colour)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.alignment = anchor;
            label.color = colour;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = labelObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);
            return label;
        }

        private static RectTransform Bar(string name, Transform parent, Vector2 position, Vector2 size, Color colour)
        {
            var back = Panel(name + " Back", parent, new Color(0f, 0f, 0f, 0.6f));
            back.anchorMin = back.anchorMax = back.pivot = new Vector2(0f, 1f);
            back.anchoredPosition = position;
            back.sizeDelta = size;

            var fill = Panel(name + " Fill", back, colour);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            return fill;
        }

        // With a target the dummy hits back: it gets a sword arm, a red headband and the ground marker for its blow.
        private static void BuildDummy(Vector3 position, Quaternion towardCamera, Material wood, Material straw,
            Material barBack, Material barHealth, Material barStagger, Transform target)
        {
            bool fights = target != null;
            var dummy = new GameObject(fights ? "Sparring Dummy" : "Training Dummy");
            dummy.transform.position = position;
            var capsule = dummy.AddComponent<CapsuleCollider>();
            capsule.radius = 0.45f;
            capsule.height = 1.9f;
            capsule.center = new Vector3(0f, 0.95f, 0f);

            // The pivot turns to face the player; everything that rocks hangs off the body inside it, at the ground.
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(dummy.transform, false);
            var body = new GameObject("Body").transform;
            body.SetParent(pivot, false);
            var parts = new List<Renderer>
            {
                Shape(PrimitiveType.Cylinder, "Post", body, new Vector3(0f, 0.75f, 0f), new Vector3(0.22f, 0.75f, 0.22f), wood),
                Shape(PrimitiveType.Cylinder, "Torso", body, new Vector3(0f, 1.2f, 0f), new Vector3(0.62f, 0.38f, 0.62f), straw),
                Shape(PrimitiveType.Cube, "Arms", body, new Vector3(0f, 1.38f, 0f), new Vector3(1.5f, 0.12f, 0.12f), wood),
                Shape(PrimitiveType.Sphere, "Head", body, new Vector3(0f, 1.82f, 0f), new Vector3(0.44f, 0.44f, 0.44f), straw),
            };
            Shape(PrimitiveType.Cylinder, "Base", dummy.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.9f, 0.04f, 0.9f), wood);

            // Bars above the head, turned to face the camera, which never rotates.
            var bars = new GameObject("Bars").transform;
            bars.SetParent(dummy.transform, false);
            bars.localPosition = new Vector3(0f, 2.45f, 0f);
            bars.rotation = towardCamera;
            Shape(PrimitiveType.Cube, "Back", bars, new Vector3(0f, 0.05f, 0.01f), new Vector3(1.26f, 0.3f, 0.01f), barBack);
            var health = BarFill("Health", bars, 0.11f, 0.12f, barHealth);
            var stagger = BarFill("Stagger", bars, -0.03f, 0.07f, barStagger);

            Transform arm = null;
            MeshFilter outline = null, fill = null;
            if (fights)
            {
                var blade = ToonMaterial("DummyBlade", new Color(0.2f, 0.18f, 0.17f));
                var band = ToonMaterial("DummyBand", new Color(0.62f, 0.14f, 0.08f));
                parts.Add(Shape(PrimitiveType.Cylinder, "Headband", body, new Vector3(0f, 1.9f, 0f), new Vector3(0.47f, 0.035f, 0.47f), band));
                arm = new GameObject("Sword Arm").transform;
                arm.SetParent(body, false);
                arm.localPosition = new Vector3(0.62f, 1.38f, 0f);
                parts.Add(Shape(PrimitiveType.Cube, "Wooden Sword", arm, new Vector3(0f, 0f, 0.8f), new Vector3(0.1f, 0.1f, 1.6f), blade));

                var marker = MarkerMaterial();
                outline = Marker("Marker Outline", pivot, 0.03f, marker);
                fill = Marker("Marker Fill", pivot, 0.04f, marker);
            }

            var training = dummy.AddComponent<TrainingDummy>();
            training.Configure(body, parts.ToArray(), health, stagger);
            if (fights) dummy.AddComponent<DummyAttacker>().Configure(pivot, arm, target, outline, fill);
        }

        // The attack marker's shape is made by the dummy when the game starts, from its own reach and arc.
        private static MeshFilter Marker(string name, Transform parent, float height, Material material)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = new Vector3(0f, height, 0f);
            var filter = marker.AddComponent<MeshFilter>();
            var renderer = marker.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.enabled = false;
            return filter;
        }

        private static Material MarkerMaterial()
        {
            var material = UnlitMaterial("AttackMarker", new Color(1f, 0.93f, 0.75f, 0.4f));
            // See-through, drawn after the floor, visible from both sides.
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        // A bar that shrinks toward its left end: the pivot sits at the left and the block hangs to its right.
        private static Transform BarFill(string name, Transform parent, float height, float thickness, Material material)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = new Vector3(-0.6f, height, 0f);
            Shape(PrimitiveType.Cube, name + " Block", pivot, new Vector3(0.6f, 0f, 0f), new Vector3(1.2f, thickness, 0.01f), material);
            return pivot;
        }

        private static Renderer Shape(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 size, Material material)
        {
            var shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            Object.DestroyImmediate(shape.GetComponent<Collider>());
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = localPosition;
            shape.transform.localScale = size;
            var renderer = shape.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return renderer;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (var child in parent.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }

        private static Material ToonMaterial(string name, Color colour)
        {
            string path = $"{GeneratedFolder}/{name}.mat";
            var shader = Shader.Find(ToonShaderName);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", colour);
            // Unity's own shapes have ordinary tangents, so their outline follows the normals.
            material.SetFloat("_SmoothNormalsInTangents", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material UnlitMaterial(string name, Color colour)
        {
            string path = $"{GeneratedFolder}/{name}.mat";
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", colour);
            EditorUtility.SetDirty(material);
            return material;
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
