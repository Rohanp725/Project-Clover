using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Player;
using Clover.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds everything that travels with the player from scene to scene: the camera, the player with its model,
    /// animator and weapon, the effects object, and the bars on screen. Every scene builder calls this, so the player
    /// is the same everywhere.
    /// </summary>
    public static class PlayerBuilder
    {
        public const string ControlsPath = "Assets/_Project/Settings/Controls.inputactions";
        public const string PlayerModelPath = "Assets/_Project/Art/Models/Characters/player.fbx";
        private const string PlayerFolder = BuildKit.GeneratedRoot + "/Player";

        // The character's colours as authored in Blender (linear values), by the name behind "m_player_" in the model.
        private static readonly (string name, Color linear)[] PlayerPalette =
        {
            ("skin", new Color(0.78f, 0.5f, 0.34f)),
            ("jacket", new Color(0.05f, 0.2f, 0.42f)),
            ("trouser", new Color(0.1f, 0.07f, 0.17f)),
            ("wrap", new Color(0.78f, 0.7f, 0.5f)),
            ("leather", new Color(0.3f, 0.14f, 0.07f)),
            ("iron", new Color(0.09f, 0.1f, 0.2f)),
            ("accent", new Color(0.95f, 0.13f, 0.03f)),
            ("hair", new Color(0.035f, 0.03f, 0.04f)),
            ("straw", new Color(0.82f, 0.58f, 0.16f)),
            ("steel", new Color(0.72f, 0.76f, 0.82f)),
        };

        public struct Built
        {
            public GameObject Player;
            public Camera Camera;
            public TopDownCamera Follow;
            public Vfx Effects;
        }

        /// <summary>Builds the camera, the player at the given spot, the effects and the HUD into the open scene.</summary>
        public static Built Build(Vector3 position, string hint)
        {
            var built = new Built();
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (controls == null)
            {
                Debug.LogError($"[Clover.Build] No input actions at {ControlsPath}; the player was not built.");
                return built;
            }

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            built.Camera = cameraObject.AddComponent<Camera>();
            built.Camera.fieldOfView = 40f;
            built.Camera.farClipPlane = 200f;
            cameraObject.AddComponent<AudioListener>();
            built.Follow = cameraObject.AddComponent<TopDownCamera>();

            built.Effects = VfxBuilder.Build();
            built.Player = BuildPlayer(built.Follow);
            built.Player.transform.position = position;
            var controller = built.Player.GetComponent<PlayerController>();
            controller.Configure(controls, cameraObject.transform);
            built.Follow.Configure(built.Player.transform);
            built.Follow.SnapToTarget();
            cameraObject.AddComponent<Clover.World.OcclusionFader>().Configure(built.Player.transform);

            BuildHud(controller, built.Player.GetComponent<PlayerCombat>(), built.Player.GetComponent<PlayerDefence>(), hint);
            // The player's lingering effects on the ground live in the scene, not on the player.
            new GameObject("Hazards").AddComponent<Clover.World.Hazards>();
            return built;
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
            var arcs = player.AddComponent<SlashArcs>();
            arcs.Configure(VfxBuilder.SlashMaterial());
            combat.Configure(hitStop, follow, arcs);
            var guardArc = player.AddComponent<GuardArc>();
            guardArc.Configure(VfxBuilder.GuardArcMaterial(), 1.2f, new Guard().ArcDegrees, new Color(0.9f, 1.5f, 2.6f, 0.85f));
            defence.Configure(hitStop, follow, guardArc);

            // The element's light: dark until a spell or the surge, then it throws the element's colour on everything near.
            var glowObject = new GameObject("Element Glow");
            glowObject.transform.SetParent(player.transform, false);
            glowObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var glow = glowObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.5f, 0.16f);
            glow.range = 8f;
            glow.intensity = 0f;
            glow.shadows = LightShadows.None;
            player.AddComponent<PlayerInteraction>();
            var element = player.AddComponent<PlayerElement>();
            element.Configure(glow, arcs, follow);
            player.AddComponent<PlayerUpgrades>();

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
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => ToonFor(imported, materials)).ToArray();

            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = AnimatorController(modelAsset);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var drawnSword = BuildKit.FindDeep(model.transform, "katana_drawn");
            var sheathedHilt = BuildKit.FindDeep(model.transform, "katana_hilt");
            if (drawnSword == null || sheathedHilt == null)
                Debug.LogWarning("[Clover.Build] The character model has no katana_drawn or katana_hilt object; the sword will not move between sheath and hand.");
            var sheath = BuildKit.FindDeep(model.transform, "katana_sheath");
            player.AddComponent<PlayerAnimator>().Configure(controller, combat, defence, element, animator,
                drawnSword != null ? drawnSword.gameObject : null, sheathedHilt != null ? sheathedHilt.gameObject : null,
                sheath != null ? sheath.gameObject : null);
            return player;
        }

        /// <summary>Puts the character's toon materials on a copy of its model, as the game does.</summary>
        public static void Dress(GameObject model)
        {
            var materials = PlayerMaterials();
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => ToonFor(imported, materials)).ToArray();
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
            var materials = new Dictionary<string, Material>();
            foreach (var (name, linear) in PlayerPalette)
            {
                // Material colours are stored as the eye sees them; Blender's numbers are light values.
                var material = BuildKit.Toon(PlayerFolder, $"m_player_{name}", linear.gamma, true);
                // A cool rim lifts the dark clothes off a dark floor; the blade catches the light outright.
                material.Lit(name == "steel" ? new Color(1.6f, 1.8f, 2.2f) : new Color(0.75f, 0.85f, 1.2f), name == "steel" ? 0.6f : 0.34f);
                materials[name] = material;
            }
            return materials;
        }

        private static AnimatorController AnimatorController(GameObject modelAsset)
        {
            string path = $"{PlayerFolder}/PlayerAnimator.controller";
            AssetDatabase.DeleteAsset(path);
            // The animations are written as code and baked by Clover > Forge > Bake Player Animations.
            var clips = AssetDatabase.FindAssets("t:AnimationClip", new[] { Forge.PlayerAnimations.ClipFolder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(clip => clip != null).ToDictionary(clip => clip.name);
            if (clips.Count == 0) Debug.LogError("[Clover.Build] No baked player animations; run Clover > Forge > Bake Player Animations first.");
            foreach (string needed in new[] { "idle", "run", "roll", PlayerAnimator.ChargeState, PlayerAnimator.HurtState, PlayerAnimator.GuardState, PlayerAnimator.DrinkState })
                if (!clips.ContainsKey(needed)) Debug.LogError($"[Clover.Build] The character model has no '{needed}' clip.");

            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter(PlayerAnimator.SpeedParameter, AnimatorControllerParameterType.Float);
            var machine = controller.layers[0].stateMachine;

            var locomotion = controller.CreateBlendTreeInController(PlayerAnimator.LocomotionState, out BlendTree tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = PlayerAnimator.SpeedParameter;
            tree.useAutomaticThresholds = false;
            if (clips.TryGetValue("idle", out var idle)) tree.AddChild(idle, 0f);
            if (clips.TryGetValue("run", out var run)) tree.AddChild(run, 1f);
            machine.defaultState = locomotion;

            // Bare hands have their own way of standing and running.
            controller.CreateBlendTreeInController(PlayerAnimator.FistLocomotionState, out BlendTree fistTree, 0);
            fistTree.blendType = BlendTreeType.Simple1D;
            fistTree.blendParameter = PlayerAnimator.SpeedParameter;
            fistTree.useAutomaticThresholds = false;
            if (clips.TryGetValue("idle_fist", out var idleFist)) fistTree.AddChild(idleFist, 0f);
            if (clips.TryGetValue("run_fist", out var runFist)) fistTree.AddChild(runFist, 1f);

            var roll = machine.AddState(PlayerAnimator.RollState);
            if (clips.TryGetValue("roll", out var rollClip)) roll.motion = rollClip;

            // One state per attack clip, named after the clip; AttackStep.Name picks the state.
            foreach (var attack in clips.Values.Where(clip => clip.name.StartsWith("attack_")).OrderBy(clip => clip.name))
                machine.AddState(attack.name).motion = attack;
            foreach (var weapon in Weapons.All)
                foreach (var step in weapon.AllSteps())
                    if (!clips.ContainsKey(step.Name)) Debug.LogWarning($"[Clover.Build] The character model has no '{step.Name}' clip; that attack will not be animated.");

            foreach (string name in new[] { PlayerAnimator.ChargeState, PlayerAnimator.FistChargeState, PlayerAnimator.HurtState, PlayerAnimator.CastState, PlayerAnimator.DeathState, PlayerAnimator.LeapState })
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
            foreach (string name in new[] { PlayerAnimator.GuardState, PlayerAnimator.FistGuardState, PlayerAnimator.DrinkState })
                upperMachine.AddState(name).motion = clips.TryGetValue(name, out var clip) ? clip : null;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void BuildHud(PlayerController player, PlayerCombat combat, PlayerDefence defence, string hintText)
        {
            var canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            Transform hud = canvasObject.transform;

            // The impact frame: a full-screen wash that blinks on a great blow. It lies under everything else on the HUD.
            var wash = Panel("Impact Frame", hud, Color.white);
            wash.anchorMin = Vector2.zero;
            wash.anchorMax = Vector2.one;
            wash.offsetMin = wash.offsetMax = Vector2.zero;
            wash.GetComponent<Image>().enabled = false;
            canvasObject.AddComponent<ScreenFlash>().Configure(wash.GetComponent<Image>());

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

            var status = Label("Status", hud, 22, TextAnchor.UpperLeft, new Color(1f, 0.82f, 0.6f, 0.95f));
            status.rectTransform.anchorMin = status.rectTransform.anchorMax = status.rectTransform.pivot = new Vector2(0f, 1f);
            status.rectTransform.anchoredPosition = new Vector2(150f, -112f);
            status.rectTransform.sizeDelta = new Vector2(700f, 30f);
            status.text = "";

            if (!string.IsNullOrEmpty(hintText))
            {
                var hint = Label("Controls", hud, 20, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.55f));
                hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = hint.rectTransform.pivot = Vector2.zero;
                hint.rectTransform.anchoredPosition = new Vector2(48f, 36f);
                hint.rectTransform.sizeDelta = new Vector2(1700f, 60f);
                hint.text = hintText;
            }

            var prompt = Label("Prompt", hud, 28, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.85f, 0.95f));
            prompt.rectTransform.anchorMin = prompt.rectTransform.anchorMax = new Vector2(0.5f, 0.2f);
            prompt.rectTransform.sizeDelta = new Vector2(900f, 40f);
            prompt.text = "";

            var playerHud = canvasObject.AddComponent<PlayerHud>();
            playerHud.Configure(player, combat, defence, health, grey, stamina, mana, flasks, callout, status, prompt);

            // Embers held, top right; Heat's five levels under the flasks, shown only once Heat is learned.
            var embers = Label("Embers", hud, 26, TextAnchor.UpperRight, new Color(1f, 0.72f, 0.3f, 0.95f));
            embers.rectTransform.anchorMin = embers.rectTransform.anchorMax = embers.rectTransform.pivot = new Vector2(1f, 1f);
            embers.rectTransform.anchoredPosition = new Vector2(-48f, -36f);
            embers.rectTransform.sizeDelta = new Vector2(400f, 34f);
            embers.text = "";
            var heatRow = new GameObject("Heat", typeof(RectTransform));
            heatRow.transform.SetParent(hud, false);
            var heatRect = (RectTransform)heatRow.transform;
            heatRect.anchorMin = heatRect.anchorMax = heatRect.pivot = new Vector2(0f, 1f);
            heatRect.anchoredPosition = Vector2.zero;
            var pips = new RectTransform[new Heat().MaximumLevel];
            for (int i = 0; i < pips.Length; i++)
                pips[i] = Bar("Heat " + (i + 1), heatRow.transform, new Vector2(48f + i * 34f, -150f), new Vector2(30f, 9f), new Color(1f, 0.45f, 0.08f));
            heatRow.SetActive(false);
            playerHud.ConfigureUpgrades(player.GetComponent<PlayerUpgrades>(), embers, heatRow, pips);

            BuildUpgradeMenu(player, hud);
        }

        // The element's tree: a dark panel over the middle of the screen with a list on the left and what the chosen thing does on the right.
        private static void BuildUpgradeMenu(PlayerController player, Transform hud)
        {
            var panel = Panel("Upgrade Menu", hud, new Color(0.03f, 0.03f, 0.07f, 0.9f));
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(1180f, 640f);
            var edge = Panel("Edge", panel, new Color(1f, 0.62f, 0.2f, 0.9f));
            edge.anchorMin = new Vector2(0f, 1f);
            edge.anchorMax = new Vector2(1f, 1f);
            edge.pivot = new Vector2(0.5f, 1f);
            edge.sizeDelta = new Vector2(0f, 4f);
            edge.anchoredPosition = new Vector2(0f, -78f);

            var title = Label("Title", panel, 36, TextAnchor.UpperLeft, new Color(1f, 0.8f, 0.45f, 1f));
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = title.rectTransform.pivot = new Vector2(0f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(44f, -24f);
            title.rectTransform.sizeDelta = new Vector2(1000f, 44f);

            var cursor = Panel("Cursor", panel, new Color(1f, 0.62f, 0.2f, 0.22f));
            cursor.anchorMin = cursor.anchorMax = cursor.pivot = new Vector2(0f, 1f);
            cursor.sizeDelta = new Vector2(500f, 50f);
            cursor.anchoredPosition = new Vector2(30f, -110f);

            const int count = 7;
            var rows = new Text[count];
            var costs = new Text[count];
            for (int i = 0; i < count; i++)
            {
                rows[i] = Label("Row " + (i + 1), panel, 28, TextAnchor.MiddleLeft, Color.white);
                rows[i].rectTransform.anchorMin = rows[i].rectTransform.anchorMax = rows[i].rectTransform.pivot = new Vector2(0f, 1f);
                rows[i].rectTransform.anchoredPosition = new Vector2(44f, -110f - i * 58f);
                rows[i].rectTransform.sizeDelta = new Vector2(360f, 50f);
                costs[i] = Label("Cost " + (i + 1), rows[i].transform, 26, TextAnchor.MiddleRight, Color.white);
                costs[i].rectTransform.anchorMin = costs[i].rectTransform.anchorMax = costs[i].rectTransform.pivot = new Vector2(0f, 0.5f);
                costs[i].rectTransform.anchoredPosition = new Vector2(340f, 0f);
                costs[i].rectTransform.sizeDelta = new Vector2(130f, 50f);
            }

            var description = Label("Description", panel, 28, TextAnchor.UpperLeft, new Color(0.95f, 0.93f, 0.9f, 1f));
            description.rectTransform.anchorMin = description.rectTransform.anchorMax = description.rectTransform.pivot = new Vector2(0f, 1f);
            description.rectTransform.anchoredPosition = new Vector2(580f, -116f);
            description.rectTransform.sizeDelta = new Vector2(560f, 400f);
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            description.lineSpacing = 1.15f;

            var hint = Label("Hint", panel, 22, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.6f));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = hint.rectTransform.pivot = Vector2.zero;
            hint.rectTransform.anchoredPosition = new Vector2(44f, 26f);
            hint.rectTransform.sizeDelta = new Vector2(1000f, 30f);
            hint.text = "Stick or W S: choose     Cross or E: learn     Circle or Space: leave";

            hud.gameObject.AddComponent<UpgradeMenu>().Configure(player, panel.gameObject, title, description, rows, costs, cursor);
        }

        public const string ControlsHint =
            "R1 light   R2 heavy (hold: weapon skill, 30 mana; on a staggered enemy: finisher)   L1 guard   Circle dodge   Square flask   Triangle element spell   L2 element surge   Cross use\n"
            + "Keyboard: WASD move   J light   K heavy   L guard   Space dodge   R flask   U spell   I surge   E use";

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

        private static RectTransform Panel(string name, Transform parent, Color colour)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = colour;
            image.raycastTarget = false;
            return (RectTransform)panel.transform;
        }
    }
}
