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
            BuildFeedback(built.Player, cameraObject.transform);
            // The player's lingering effects on the ground live in the scene, not on the player.
            new GameObject("Hazards").AddComponent<Clover.World.Hazards>();
            // What spell forms are drawn with and run by: shapes and marks kept ready, and the casts in progress.
            var (spellSolid, spellMarks) = VfxBuilder.FormMaterials();
            var works = new GameObject("Spell Works");
            works.AddComponent<Clover.Player.Forms.FormFx>().Configure(spellSolid, spellMarks);
            works.AddComponent<Clover.Player.Forms.SpellWorks>().Configure(built.Player.GetComponent<Afterimage>());
            // A flask in the air, for the eye only; it waits unseen until one is thrown.
            var thrown = new GameObject("Thrown Flask");
            var flask = BuildKit.Group("Flask", thrown.transform);
            BuildKit.Shape(PrimitiveType.Sphere, "Belly", flask, Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f),
                BuildKit.Toon(PlayerFolder, "m_flask_thrown", new Color(1f, 0.6f, 0.2f), false, null, 0f, 0.02f).Lit(Color.black, 0.3f, new Color(1.8f, 0.6f, 0.1f)));
            BuildKit.Shape(PrimitiveType.Cube, "Neck", flask, new Vector3(0f, 0.16f, 0f), new Vector3(0.08f, 0.14f, 0.08f),
                BuildKit.Toon(PlayerFolder, "m_flask_neck", new Color(0.4f, 0.28f, 0.2f), false, null, 0f, 0.02f));
            var trail = flask.gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0f;
            trail.sharedMaterial = BuildKit.Glow(PlayerFolder, "m_flask_trail", new Color(2.4f, 0.9f, 0.2f, 0.8f), 3, true, 1f);
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            thrown.AddComponent<Clover.World.ThrownThing>().Configure(flask, trail);
            return built;
        }

        // What answers the player's blows and choices in the world: the brackets under whoever the next attack would turn
        // to, the cut lines blows leave, and the embers that fly in from the fallen. All wait unseen until wanted.
        private static void BuildFeedback(GameObject player, Transform camera)
        {
            // Thinner and dimmer than any attack marker: it is a hint, not a warning.
            var brackets = BuildKit.Glow(PlayerFolder, "m_target_brackets", Color.white, 8, false, 1f).Graphic(0.74f);
            var ring = BuildKit.Shape(PrimitiveType.Quad, "Target Brackets", null, Vector3.zero, Vector3.one * 2f, brackets, new Vector3(90f, 0f, 0f));
            ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ring.receiveShadows = false;
            ring.enabled = false;
            var reticle = player.AddComponent<TargetReticle>();
            reticle.Configure(ring.transform, ring);
            player.GetComponent<PlayerCombat>().ConfigureFeedback(reticle);

            // Each stroke is a white-hot needle on a slightly larger one of ink, drawn over whatever it crosses.
            var strokeInk = BuildKit.Glow(PlayerFolder, "m_cut_ink", UiArt.Ink, 1, false, 1f, 0f, true).Graphic(0.2f);
            var strokeWhite = BuildKit.Glow(PlayerFolder, "m_cut_white", new Color(4.5f, 4.4f, 4f, 1f), 1, false, 1f, 0f, true).Graphic(0.34f);
            var cuts = new GameObject("Cut Marks");
            var strokes = new Transform[6];
            for (int i = 0; i < strokes.Length; i++)
            {
                strokes[i] = BuildKit.Group("Stroke " + (i + 1), cuts.transform);
                Unlit(BuildKit.Shape(PrimitiveType.Quad, "Ink", strokes[i], new Vector3(0f, 0f, 0.02f), new Vector3(1.05f, 1.7f, 1f), strokeInk), 40);
                Unlit(BuildKit.Shape(PrimitiveType.Quad, "White", strokes[i], Vector3.zero, Vector3.one, strokeWhite), 41);
            }
            cuts.AddComponent<CutMarks>().Configure(strokes, camera);

            // An ember in the air is the HUD's ember: an upright diamond with a bright heart, and a tail of light behind it.
            var flame = BuildKit.Glow(PlayerFolder, "m_ember_mote", new Color(5f, 1.9f, 0.3f, 1f), 3, false, 1f, 0f, true);
            var heart = BuildKit.Glow(PlayerFolder, "m_ember_heart", new Color(6f, 5f, 3f, 1f), 3, false, 1f, 0f, true);
            var tail = BuildKit.Glow(PlayerFolder, "m_ember_tail", new Color(3.2f, 1.1f, 0.15f, 0.9f), 3, true, 1f, 0f, true);
            var embers = new GameObject("Ember Motes");
            var motes = new Transform[8];
            var tails = new TrailRenderer[motes.Length];
            for (int i = 0; i < motes.Length; i++)
            {
                motes[i] = BuildKit.Group("Mote " + (i + 1), embers.transform);
                motes[i].rotation = camera.rotation;
                Unlit(BuildKit.Shape(PrimitiveType.Quad, "Flame", motes[i], Vector3.zero, Vector3.one, flame, new Vector3(0f, 0f, 45f)), 42);
                Unlit(BuildKit.Shape(PrimitiveType.Quad, "Heart", motes[i], new Vector3(0f, 0f, -0.01f), Vector3.one * 0.5f, heart, new Vector3(0f, 0f, 45f)), 43);
                tails[i] = motes[i].gameObject.AddComponent<TrailRenderer>();
                tails[i].time = 0.2f;
                // Narrower than the mote, and thinning to nothing behind it.
                tails[i].widthMultiplier = 0.2f;
                tails[i].widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
                tails[i].sharedMaterial = tail;
                tails[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                tails[i].receiveShadows = false;
            }
            embers.AddComponent<EmberMotes>().Configure(motes, tails);
        }

        private static Renderer Unlit(Renderer renderer, int order)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>Takes the low-health warning off the HUD of the scene being built: for places where nothing can hurt.</summary>
        public static void RemoveLowHealthWarning()
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return;
            var warning = hud.GetComponent<LowHealthVignette>();
            if (warning != null) Object.DestroyImmediate(warning);
            var edge = hud.transform.Find("Low Health Edge");
            if (edge != null) Object.DestroyImmediate(edge.gameObject);
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
            // The shape of light a lightning dodge leaves behind.
            var ghost = BuildKit.Glow(PlayerFolder, "m_player_afterimage", new Color(0.5f, 1.2f, 3f, 0.8f), 3, true, 1f);
            player.AddComponent<Afterimage>().Configure(model.GetComponentsInChildren<SkinnedMeshRenderer>(), ghost);
            var shown = player.AddComponent<PlayerAnimator>();
            shown.Configure(controller, combat, defence, element, animator,
                drawnSword != null ? drawnSword.gameObject : null, sheathedHilt != null ? sheathedHilt.gameObject : null,
                sheath != null ? sheath.gameObject : null);
            // Every other weapon is a prefab hung on the skeleton's sockets, in the hand and at rest.
            shown.ConfigureRack(WeaponBuilder.Hang(model, modelAsset));
            // What weapons throw and raise: a crescent of light, spikes out of the ground.
            player.AddComponent<WeaponShots>().Configure(VfxBuilder.SlashMaterial());
            player.AddComponent<WeaponSpikes>().Configure(BuildKit.Toon(PlayerFolder, "m_weapon_spike", new Color(0.6f, 0.58f, 0.66f), false, null, 0f, 0.02f));
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

            // Every other weapon brings its own by name: idle_p and run_p make locomotion_p; roll_p and charge_p are
            // states of their own; guard_p, guard_hit_p and guard_perfect_p play on the upper body.
            foreach (var idleClip in clips.Values.Where(clip => clip.name.StartsWith("idle_") && clip.name != "idle_fist").OrderBy(clip => clip.name))
            {
                string prefix = idleClip.name.Substring("idle_".Length);
                controller.CreateBlendTreeInController("locomotion_" + prefix, out BlendTree ownTree, 0);
                ownTree.blendType = BlendTreeType.Simple1D;
                ownTree.blendParameter = PlayerAnimator.SpeedParameter;
                ownTree.useAutomaticThresholds = false;
                ownTree.AddChild(idleClip, 0f);
                if (clips.TryGetValue("run_" + prefix, out var ownRun)) ownTree.AddChild(ownRun, 1f);
            }
            foreach (var own in clips.Values.Where(clip => clip.name.StartsWith("roll_") || (clip.name.StartsWith("charge_") && clip.name != PlayerAnimator.FistChargeState)).OrderBy(clip => clip.name))
                machine.AddState(own.name).motion = own;
            foreach (var weapon in Weapons.All)
                foreach (string state in new[] { weapon.ChargeAnimation, weapon.LocomotionState == PlayerAnimator.LocomotionState ? "idle" : "idle_" + weapon.Prefix })
                    if (!clips.ContainsKey(state)) Debug.LogWarning($"[Clover.Build] {weapon.Name} has no '{state}' clip.");

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
            foreach (var own in clips.Values.Where(clip => clip.name.StartsWith("guard_") && clip.name != PlayerAnimator.FistGuardState).OrderBy(clip => clip.name))
                upperMachine.AddState(own.name).motion = own;
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

            // The impact frame: spikes of light from the screen's edges that blink on a great blow, the middle left clear. It lies under everything else on the HUD.
            var wash = Panel("Impact Frame", hud, Color.white);
            wash.anchorMin = Vector2.zero;
            wash.anchorMax = Vector2.one;
            wash.offsetMin = wash.offsetMax = Vector2.zero;
            wash.GetComponent<Image>().sprite = UiArt.ImpactFrame();
            wash.GetComponent<Image>().enabled = false;
            canvasObject.AddComponent<ScreenFlash>().Configure(wash.GetComponent<Image>());

            // The warning of low health: a dark red edge, under everything, the impact frame included.
            var edge = Panel("Low Health Edge", hud, new Color(0.3f, 0.0f, 0.02f, 1f));
            edge.anchorMin = Vector2.zero;
            edge.anchorMax = Vector2.one;
            edge.offsetMin = edge.offsetMax = Vector2.zero;
            edge.SetSiblingIndex(0);
            var edgeImage = edge.GetComponent<Image>();
            edgeImage.sprite = UiArt.Vignette();
            edgeImage.enabled = false;
            var edgeGroup = edge.gameObject.AddComponent<CanvasGroup>();
            edgeGroup.alpha = 0f;
            edgeGroup.interactable = false;
            edgeGroup.blocksRaycasts = false;
            canvasObject.AddComponent<LowHealthVignette>().Configure(defence, edgeImage, edgeGroup);

            var health = Bar("Health", hud, new Vector2(44f, -34f), new Vector2(470f, 30f), new Color(0.86f, 0.16f, 0.14f));
            // Behind the red: the pale piece that lingers where health was just lost, and behind that the grey that a
            // guarded blow took and that can still be won back.
            var lag = Fill("Health Lag", health.parent, new Color(1f, 0.9f, 0.75f));
            lag.SetSiblingIndex(0);
            var grey = Fill("Health Grey", health.parent, new Color(0.62f, 0.58f, 0.6f));
            grey.SetSiblingIndex(0);
            var stamina = Bar("Stamina", hud, new Vector2(40f, -66f), new Vector2(390f, 20f), new Color(0.5f, 0.86f, 0.42f));
            // Mana takes the colour of the player's element.
            var mana = Bar("Mana", hud, new Vector2(37f, -88f), new Vector2(320f, 18f), new Color(1f, 0.5f, 0.1f));

            // More gourds than a new player carries: an upgrade may add to the three, and the HUD shows only those carried.
            var flasks = new RectTransform[new FlaskBelt().Maximum + 3];
            for (int i = 0; i < flasks.Length; i++)
                flasks[i] = Flask("Flask " + (i + 1), hud, new Vector2(44f + i * 38f, -112f));

            var callout = Label("Callout", hud, 54, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.9f));
            callout.rectTransform.anchorMin = callout.rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
            callout.rectTransform.sizeDelta = new Vector2(900f, 60f);
            callout.text = "";

            var status = Label("Status", hud, 28, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.62f, 0.95f));
            status.rectTransform.anchorMin = status.rectTransform.anchorMax = status.rectTransform.pivot = new Vector2(0f, 1f);
            status.rectTransform.anchoredPosition = new Vector2(168f, -120f);
            status.rectTransform.sizeDelta = new Vector2(700f, 30f);
            status.text = "";

            if (hintText == PauseHint)
            {
                // The controls live in the pause menu; all the screen keeps is one quiet line saying where.
                var quiet = Label("Controls", hud, 22, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.55f));
                quiet.rectTransform.anchorMin = quiet.rectTransform.anchorMax = quiet.rectTransform.pivot = Vector2.zero;
                quiet.rectTransform.anchoredPosition = new Vector2(48f, 26f);
                quiet.rectTransform.sizeDelta = new Vector2(900f, 30f);
                quiet.text = hintText;
            }
            else if (!string.IsNullOrEmpty(hintText))
            {
                // A band of ink behind the hint, so it reads over whatever floor is under it.
                var band = Panel("Controls Band", hud, new Color(0.02f, 0.015f, 0.05f, 0.62f));
                band.anchorMin = new Vector2(0f, 0f);
                band.anchorMax = new Vector2(1f, 0f);
                band.pivot = new Vector2(0.5f, 0f);
                band.anchoredPosition = Vector2.zero;
                band.sizeDelta = new Vector2(0f, 96f);
                var hint = Label("Controls", hud, 21, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.72f));
                hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = hint.rectTransform.pivot = Vector2.zero;
                hint.rectTransform.anchoredPosition = new Vector2(48f, 30f);
                hint.rectTransform.sizeDelta = new Vector2(1700f, 60f);
                hint.text = hintText;
            }

            var prompt = Label("Prompt", hud, 34, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.85f, 1f));
            prompt.rectTransform.anchorMin = prompt.rectTransform.anchorMax = new Vector2(0.5f, 0.2f);
            prompt.rectTransform.sizeDelta = new Vector2(900f, 44f);
            prompt.text = "";
            // A stroke of ink behind the words, shown and hidden with them.
            var stroke = Panel("Stroke", prompt.transform, new Color(0.03f, 0.02f, 0.06f, 0.72f));
            stroke.anchorMin = stroke.anchorMax = new Vector2(0.5f, 0.5f);
            stroke.sizeDelta = new Vector2(760f, 64f);
            var strokeImage = stroke.GetComponent<Image>();
            strokeImage.sprite = UiArt.Brush();
            strokeImage.type = Image.Type.Sliced;
            var promptWords = Label("Words", prompt.transform, 34, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.85f, 1f));
            promptWords.rectTransform.anchorMin = Vector2.zero;
            promptWords.rectTransform.anchorMax = Vector2.one;
            promptWords.rectTransform.offsetMin = promptWords.rectTransform.offsetMax = Vector2.zero;
            // The first label only switches the pair on and off; the words are drawn by the one on top of the stroke.
            prompt.color = new Color(0f, 0f, 0f, 0f);
            Object.DestroyImmediate(prompt.GetComponent<Outline>());
            prompt.gameObject.AddComponent<Clover.UI.MirrorText>().Configure(prompt, promptWords, stroke.gameObject);

            var playerHud = canvasObject.AddComponent<PlayerHud>();
            playerHud.Configure(player, combat, defence, health, grey, stamina, mana, flasks, callout, status, prompt);
            playerHud.ConfigureJuice(lag, mana.GetComponent<Image>());

            // Embers held, top right; Heat's five levels under the flasks, shown only once Heat is learned.
            var embers = Label("Embers", hud, 32, TextAnchor.UpperRight, new Color(1f, 0.78f, 0.36f, 1f));
            embers.rectTransform.anchorMin = embers.rectTransform.anchorMax = embers.rectTransform.pivot = new Vector2(1f, 1f);
            embers.rectTransform.anchoredPosition = new Vector2(-48f, -30f);
            embers.rectTransform.sizeDelta = new Vector2(400f, 40f);
            embers.text = "";
            var emberIcon = Panel("Ember Icon", embers.transform, new Color(1f, 0.6f, 0.16f));
            emberIcon.anchorMin = emberIcon.anchorMax = emberIcon.pivot = new Vector2(1f, 0.5f);
            emberIcon.anchoredPosition = new Vector2(-138f, 0f);
            emberIcon.sizeDelta = new Vector2(34f, 34f);
            emberIcon.GetComponent<Image>().sprite = UiArt.Ember();
            var heatRow = new GameObject("Heat", typeof(RectTransform));
            heatRow.transform.SetParent(hud, false);
            var heatRect = (RectTransform)heatRow.transform;
            heatRect.anchorMin = heatRect.anchorMax = heatRect.pivot = new Vector2(0f, 1f);
            heatRect.anchoredPosition = Vector2.zero;
            var pips = new RectTransform[new Heat().MaximumLevel];
            for (int i = 0; i < pips.Length; i++)
                pips[i] = Bar("Heat " + (i + 1), heatRow.transform, new Vector2(42f + i * 40f, -166f), new Vector2(44f, 16f), new Color(1f, 0.5f, 0.1f));
            heatRow.SetActive(false);
            playerHud.ConfigureUpgrades(player.GetComponent<PlayerUpgrades>(), embers, heatRow, pips);

            BuildUpgradeMenu(player, hud);
            BuildReadingBox(player, hud);
            // Last, so it lies over everything else on the HUD.
            BuildPauseMenu(player, hud);
        }

        // Words read at the player's own pace: a band across the bottom with who speaks, the line, and a mark when there is more.
        private static void BuildReadingBox(PlayerController player, Transform hud)
        {
            var panel = Framed("Reading Box", hud);
            panel.anchorMin = new Vector2(0.5f, 0f);
            panel.anchorMax = new Vector2(0.5f, 0f);
            panel.pivot = new Vector2(0.5f, 0f);
            panel.anchoredPosition = new Vector2(0f, 120f);
            panel.sizeDelta = new Vector2(1240f, 190f);
            var speaker = Label("Speaker", panel, 28, TextAnchor.UpperLeft, new Color(1f, 0.82f, 0.5f, 1f));
            speaker.rectTransform.anchorMin = speaker.rectTransform.anchorMax = speaker.rectTransform.pivot = new Vector2(0f, 1f);
            speaker.rectTransform.anchoredPosition = new Vector2(44f, -22f);
            speaker.rectTransform.sizeDelta = new Vector2(600f, 30f);

            var line = Label("Line", panel, 38, TextAnchor.UpperLeft, new Color(0.96f, 0.95f, 0.92f, 1f));
            line.rectTransform.anchorMin = line.rectTransform.anchorMax = line.rectTransform.pivot = new Vector2(0f, 1f);
            line.rectTransform.anchoredPosition = new Vector2(44f, -62f);
            line.rectTransform.sizeDelta = new Vector2(1160f, 110f);
            line.horizontalOverflow = HorizontalWrapMode.Wrap;
            line.lineSpacing = 1.1f;

            var more = Label("More", panel, 22, TextAnchor.LowerRight, new Color(1f, 1f, 1f, 0.6f));
            more.rectTransform.anchorMin = more.rectTransform.anchorMax = more.rectTransform.pivot = new Vector2(1f, 0f);
            more.rectTransform.anchoredPosition = new Vector2(-36f, 20f);
            more.rectTransform.sizeDelta = new Vector2(300f, 28f);
            more.text = "Cross or E";

            hud.gameObject.AddComponent<ReadingBox>().Configure(player, panel.gameObject, speaker, line, more);
        }

        /// <summary>Shows a place's name, or its master's, across the screen for a few seconds as the scene opens.</summary>
        public static void AddTitle(string title, string subtitle)
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return;
            var root = new GameObject("Title Card", typeof(RectTransform));
            root.transform.SetParent(hud.transform, false);
            var area = (RectTransform)root.transform;
            // High on the screen: the player stands in its middle, and the card must never lie across them.
            area.anchorMin = area.anchorMax = new Vector2(0.5f, 0.83f);
            area.sizeDelta = new Vector2(1400f, 240f);
            var group = root.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            // The stroke lies behind the name and the line under it alike, and is nearly solid, so nothing bright on the
            // floor shows through the words.
            var stroke = Panel("Stroke", root.transform, new Color(0.02f, 0.015f, 0.05f, 0.94f));
            stroke.anchorMin = stroke.anchorMax = new Vector2(0.5f, 0.41f);
            stroke.sizeDelta = new Vector2(1250f, 236f);
            var strokeImage = stroke.GetComponent<Image>();
            strokeImage.sprite = UiArt.Brush();
            strokeImage.type = Image.Type.Sliced;

            var name = Label("Title", root.transform, 92, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.86f, 1f));
            name.rectTransform.anchorMin = name.rectTransform.anchorMax = new Vector2(0.5f, 0.58f);
            name.rectTransform.sizeDelta = new Vector2(1400f, 120f);
            name.text = title;
            var line = Panel("Line", root.transform, new Color(0.93f, 0.78f, 0.47f, 0.9f));
            line.anchorMin = line.anchorMax = new Vector2(0.5f, 0.3f);
            line.sizeDelta = new Vector2(420f, 3f);
            var under = Label("Subtitle", root.transform, 40, TextAnchor.MiddleCenter, new Color(0.93f, 0.78f, 0.47f, 1f));
            under.rectTransform.anchorMin = under.rectTransform.anchorMax = new Vector2(0.5f, 0.14f);
            under.rectTransform.sizeDelta = new Vector2(1400f, 60f);
            under.text = subtitle;
            hud.AddComponent<TitleCard>().Configure(group, stroke, name, under);
            MenusOnTop(hud.transform);
        }

        /// <summary>
        /// Says where the controls are, in the same quiet line the hub keeps, for the first seconds after the scene
        /// opens; then it fades, since a fight needs the screen more than the line does.
        /// </summary>
        public static void AddPauseHint()
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return;
            var quiet = Label("Controls", hud.transform, 22, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.55f));
            quiet.rectTransform.anchorMin = quiet.rectTransform.anchorMax = quiet.rectTransform.pivot = Vector2.zero;
            quiet.rectTransform.anchoredPosition = new Vector2(48f, 26f);
            quiet.rectTransform.sizeDelta = new Vector2(900f, 30f);
            quiet.text = PauseHint;
            var group = quiet.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            quiet.gameObject.AddComponent<FadeAway>().Configure(group, 8f, 1f);
            MenusOnTop(hud.transform);
        }

        // Whatever a scene adds to the HUD after it is built must still lie under the menus: the fire's tree, then
        // spoken words, then the pause menu over all of it.
        private static void MenusOnTop(Transform hud)
        {
            foreach (string name in new[] { "Upgrade Menu", "Reading Box", "Pause Menu" })
            {
                var menu = hud.Find(name);
                if (menu != null) menu.SetAsLastSibling();
            }
        }

        /// <summary>Makes a fall in this scene end the run: the screen goes dark and the player wakes in the hub.</summary>
        public static void AddRunEnd(GameObject player)
        {
            var hud = GameObject.Find("HUD");
            if (hud == null || player == null) return;
            var dark = Panel("Run End Dark", hud.transform, new Color(0f, 0f, 0f, 0f));
            dark.anchorMin = Vector2.zero;
            dark.anchorMax = Vector2.one;
            dark.offsetMin = dark.offsetMax = Vector2.zero;
            var words = Label("Run End Words", dark, 64, TextAnchor.MiddleCenter, new Color(0.75f, 0.1f, 0.1f, 1f));
            words.rectTransform.anchorMin = Vector2.zero;
            words.rectTransform.anchorMax = Vector2.one;
            words.rectTransform.offsetMin = words.rectTransform.offsetMax = Vector2.zero;
            words.text = "The hours turn back";
            new GameObject("Run Ender").AddComponent<Clover.World.RunEnder>().Configure(player.GetComponent<PlayerDefence>(), dark.GetComponent<Image>(), words);
            MenusOnTop(hud.transform);
        }

        // The great chart: a night sky over the whole screen, hiding the game and its bars; the trees are drawn across
        // it by the menu itself, and what the chosen mark does is set out in a panel on the right.
        private static void BuildUpgradeMenu(PlayerController player, Transform hud)
        {
            // Solid, so nothing of the HUD or the world shows through it.
            var night = Panel("Upgrade Menu", hud, new Color(0.012f, 0.012f, 0.04f, 1f));
            night.anchorMin = Vector2.zero;
            night.anchorMax = Vector2.one;
            night.offsetMin = night.offsetMax = Vector2.zero;
            // The stars lie on a sheet a little larger than the screen, so it can drift against the chart without showing an edge.
            var sky = Panel("Sky", night, Color.white);
            sky.anchorMin = Vector2.zero;
            sky.anchorMax = Vector2.one;
            sky.offsetMin = new Vector2(-70f, -70f);
            sky.offsetMax = new Vector2(70f, 70f);
            sky.GetComponent<Image>().sprite = UiArt.Starfield();

            // The chart: an empty thing in the middle of the screen that the menu fills, slides about, turns and draws
            // nearer or further. It is a canvas of its own, so moving it does not make the rest of the HUD be drawn again.
            var chart = new GameObject("Chart", typeof(RectTransform)).GetComponent<RectTransform>();
            chart.SetParent(night, false);
            chart.anchorMin = chart.anchorMax = chart.pivot = new Vector2(0.5f, 0.5f);
            chart.sizeDelta = Vector2.zero;
            chart.gameObject.AddComponent<Canvas>();
            var cursor = Panel("Cursor", chart, new Color(1f, 0.82f, 0.4f, 1f));
            cursor.GetComponent<Image>().sprite = UiArt.MarkCursor();
            cursor.GetComponent<Image>().type = Image.Type.Sliced;
            cursor.anchorMin = cursor.anchorMax = cursor.pivot = new Vector2(0.5f, 0.5f);
            cursor.sizeDelta = new Vector2(100f, 100f);
            // The cursor beats; on a canvas of its own its beating redraws only itself.
            cursor.gameObject.AddComponent<Canvas>();

            // A band of ink under the title and another under the hint, so both read over the stars and over any mark that passes behind.
            var top = Panel("Title Band", night, new Color(0.012f, 0.012f, 0.04f, 1f));
            top.anchorMin = new Vector2(0f, 1f);
            top.anchorMax = new Vector2(1f, 1f);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0f, 104f);
            var foot = Panel("Hint Band", night, new Color(0.012f, 0.012f, 0.04f, 1f));
            foot.anchorMin = new Vector2(0f, 0f);
            foot.anchorMax = new Vector2(1f, 0f);
            foot.pivot = new Vector2(0.5f, 0f);
            foot.sizeDelta = new Vector2(0f, 84f);

            var title = Label("Title", night, 44, TextAnchor.UpperLeft, new Color(1f, 0.8f, 0.45f, 1f));
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = title.rectTransform.pivot = new Vector2(0f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(64f, -30f);
            title.rectTransform.sizeDelta = new Vector2(1000f, 44f);

            // The words lie on solid ink: marks of the neighbouring trees pass behind them and must not show through.
            var words = Panel("Words", night, new Color(0.02f, 0.018f, 0.05f, 1f));
            words.anchorMin = words.anchorMax = words.pivot = new Vector2(1f, 0.5f);
            words.anchoredPosition = new Vector2(-48f, -6f);
            words.sizeDelta = new Vector2(580f, 600f);
            var wordsFrame = Framed("Frame", words);
            wordsFrame.anchorMin = Vector2.zero;
            wordsFrame.anchorMax = Vector2.one;
            wordsFrame.offsetMin = wordsFrame.offsetMax = Vector2.zero;
            var name = Label("Name", words, 46, TextAnchor.UpperLeft, new Color(1f, 0.8f, 0.45f, 1f));
            name.rectTransform.anchorMin = name.rectTransform.anchorMax = name.rectTransform.pivot = new Vector2(0f, 1f);
            name.rectTransform.anchoredPosition = new Vector2(44f, -36f);
            name.rectTransform.sizeDelta = new Vector2(492f, 50f);
            // Two lines: what kind of mark it is and its cost, then how it stands with the player.
            var state = Label("State", words, 27, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.74f));
            state.rectTransform.anchorMin = state.rectTransform.anchorMax = state.rectTransform.pivot = new Vector2(0f, 1f);
            state.rectTransform.anchoredPosition = new Vector2(44f, -98f);
            state.rectTransform.sizeDelta = new Vector2(492f, 100f);
            state.horizontalOverflow = HorizontalWrapMode.Wrap;
            state.lineSpacing = 1.12f;
            var edge = Panel("Edge", words, new Color(0.8f, 0.72f, 0.55f, 0.7f));
            edge.anchorMin = new Vector2(0f, 1f);
            edge.anchorMax = new Vector2(1f, 1f);
            edge.pivot = new Vector2(0.5f, 1f);
            edge.sizeDelta = new Vector2(-80f, 2f);
            edge.anchoredPosition = new Vector2(0f, -212f);
            var description = Label("Description", words, 33, TextAnchor.UpperLeft, new Color(0.95f, 0.93f, 0.9f, 1f));
            description.rectTransform.anchorMin = description.rectTransform.anchorMax = description.rectTransform.pivot = new Vector2(0f, 1f);
            description.rectTransform.anchoredPosition = new Vector2(44f, -234f);
            description.rectTransform.sizeDelta = new Vector2(492f, 340f);
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            description.lineSpacing = 1.15f;

            var hint = Label("Hint", night, 22, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.66f));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = hint.rectTransform.pivot = Vector2.zero;
            hint.rectTransform.anchoredPosition = new Vector2(64f, 30f);
            hint.rectTransform.sizeDelta = new Vector2(1700f, 30f);
            hint.text = "";

            var sheet = UiArt.ChartSheet();
            var frames = new Sprite[UiArt.FrameCount];
            var pictures = new Sprite[sheet.Length - UiArt.FrameCount];
            System.Array.Copy(sheet, frames, frames.Length);
            System.Array.Copy(sheet, frames.Length, pictures, 0, pictures.Length);
            hud.gameObject.AddComponent<UpgradeMenu>().Configure(player, night.gameObject, chart, title, name, state, description, cursor, words.gameObject, hint,
                sky, frames, pictures, UiArt.Emblem());
        }

        // The pause menu: the same inked panel as the fire's tree, with a short list, and a second page that holds the controls.
        private static void BuildPauseMenu(PlayerController player, Transform hud)
        {
            // Dark enough that the player's bars behind it read as background, not as part of the menu.
            var dim = Panel("Pause Menu", hud, new Color(0.01f, 0.008f, 0.03f, 0.8f));
            dim.anchorMin = Vector2.zero;
            dim.anchorMax = Vector2.one;
            dim.offsetMin = dim.offsetMax = Vector2.zero;
            var panel = Framed("Panel", dim);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(1320f, 800f);
            var edge = Panel("Edge", panel, new Color(0.8f, 0.72f, 0.55f, 0.7f));
            edge.anchorMin = new Vector2(0f, 1f);
            edge.anchorMax = new Vector2(1f, 1f);
            edge.pivot = new Vector2(0.5f, 1f);
            edge.sizeDelta = new Vector2(-80f, 2f);
            edge.anchoredPosition = new Vector2(0f, -84f);

            var title = Label("Title", panel, 44, TextAnchor.UpperLeft, new Color(1f, 0.8f, 0.45f, 1f));
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = title.rectTransform.pivot = new Vector2(0f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(44f, -24f);
            title.rectTransform.sizeDelta = new Vector2(1000f, 44f);
            title.text = "Paused";

            var list = Page("List", panel);
            var cursor = Panel("Cursor", list, new Color(1f, 0.62f, 0.2f, 0.3f));
            cursor.GetComponent<Image>().sprite = UiArt.Brush();
            cursor.GetComponent<Image>().type = Image.Type.Sliced;
            cursor.anchorMin = cursor.anchorMax = cursor.pivot = new Vector2(0f, 1f);
            cursor.sizeDelta = new Vector2(540f, 54f);
            cursor.anchoredPosition = new Vector2(30f, -120f);

            // No scene offers more than four: resume, controls, one way out of the scene, quit.
            const int count = 4;
            var rows = new Text[count];
            for (int i = 0; i < count; i++)
            {
                rows[i] = Label("Row " + (i + 1), list, 38, TextAnchor.MiddleLeft, Color.white);
                rows[i].rectTransform.anchorMin = rows[i].rectTransform.anchorMax = rows[i].rectTransform.pivot = new Vector2(0f, 1f);
                rows[i].rectTransform.anchoredPosition = new Vector2(48f, -120f - i * 68f);
                rows[i].rectTransform.sizeDelta = new Vector2(520f, 54f);
            }
            var note = Label("Note", list, 34, TextAnchor.UpperLeft, new Color(0.95f, 0.93f, 0.9f, 1f));
            note.rectTransform.anchorMin = note.rectTransform.anchorMax = note.rectTransform.pivot = new Vector2(0f, 1f);
            note.rectTransform.anchoredPosition = new Vector2(660f, -128f);
            note.rectTransform.sizeDelta = new Vector2(600f, 400f);
            note.horizontalOverflow = HorizontalWrapMode.Wrap;
            note.lineSpacing = 1.15f;

            var controls = Page("Controls", panel);
            var gold = new Color(1f, 0.8f, 0.45f, 1f);
            var pale = new Color(0.95f, 0.93f, 0.9f, 1f);
            Column("Names", controls, 48f, 250f, pale, "\nMove\nLight blow\nHeavy blow\nGuard\nDodge\nFlask\nSpell\nSurge\nUse\nPause");
            Column("Pad", controls, 300f, 200f, pale, "Pad\nLeft stick\nR1\nR2\nL1\nCircle\nSquare\nTriangle\nL2\nCross\nStart");
            Column("Keys", controls, 500f, 160f, pale, "Keys\nW A S D\nJ\nK\nL\nSpace\nR\nU\nI\nE\nEsc");
            var holds = Column("Holds", controls, 48f, 600f, gold,
                "Heavy held: the weapon's skill, 30 mana\nHeavy on a staggered enemy: finisher\nFlask held: throw it, once learned\nSpell held: the element's own move, once learned");
            holds.rectTransform.anchoredPosition = new Vector2(48f, -576f);
            var fight = Column("Fight", controls, 700f, 570f, pale,
                "Guard turns blows from the front.\n\nA guard raised at the last instant is a perfect guard. It costs the enemy its balance.\n\nRed attacks cannot be guarded.\n\nBreak an enemy's balance, then finish it with a heavy blow.\n\nEmbers are spent at rest fires.");
            fight.horizontalOverflow = HorizontalWrapMode.Wrap;
            fight.lineSpacing = 1.1f;
            var rule = Panel("Rule", controls, new Color(0.8f, 0.72f, 0.55f, 0.35f));
            rule.anchorMin = rule.anchorMax = rule.pivot = new Vector2(0f, 1f);
            rule.anchoredPosition = new Vector2(668f, -112f);
            rule.sizeDelta = new Vector2(2f, 620f);
            controls.gameObject.SetActive(false);

            var hint = Label("Hint", panel, 22, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.6f));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = hint.rectTransform.pivot = Vector2.zero;
            hint.rectTransform.anchoredPosition = new Vector2(48f, 26f);
            hint.rectTransform.sizeDelta = new Vector2(1200f, 30f);
            hint.text = "Stick or W S: choose     Cross or E: pick     Circle or Space: back     Start or Esc: resume";

            hud.gameObject.AddComponent<PauseMenu>().Configure(player, dim.gameObject, list.gameObject, controls.gameObject, rows, note, cursor);
        }

        // An unseen sheet that fills its panel, so a whole page of a menu can be shown and hidden as one.
        private static RectTransform Page(string name, Transform parent)
        {
            var page = new GameObject(name, typeof(RectTransform));
            page.transform.SetParent(parent, false);
            var rect = (RectTransform)page.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        // Lines of text set one under another from the top of a menu page.
        private static Text Column(string name, Transform parent, float left, float width, Color colour, string lines)
        {
            var column = Label(name, parent, 28, TextAnchor.UpperLeft, colour);
            column.rectTransform.anchorMin = column.rectTransform.anchorMax = column.rectTransform.pivot = new Vector2(0f, 1f);
            column.rectTransform.anchoredPosition = new Vector2(left, -112f);
            column.rectTransform.sizeDelta = new Vector2(width, 400f);
            column.lineSpacing = 1.08f;
            column.text = lines;
            return column;
        }

        /// <summary>The one line the hub keeps on screen now that the controls live in the pause menu.</summary>
        public const string PauseHint = "Start or Esc: pause and controls";

        public const string ControlsHint =
            "R1 light   R2 heavy (hold: weapon skill, 30 mana; on a staggered enemy: finisher)   L1 guard   Circle dodge   Square flask   Triangle element spell   L2 element surge   Cross use\n"
            + "Keyboard: WASD move   J light   K heavy   L guard   Space dodge   R flask   U spell   I surge   E use";

        private static Text Label(string name, Transform parent, int size, TextAnchor anchor, Color colour)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.AddComponent<Text>();
            label.font = UiArt.Font();
            // Sizes are given as for a narrow face; this one is wider.
            label.fontSize = Mathf.RoundToInt(size * 0.86f);
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
            var back = Panel(name + " Back", parent, Color.white);
            back.anchorMin = back.anchorMax = back.pivot = new Vector2(0f, 1f);
            back.anchoredPosition = position;
            back.sizeDelta = size;
            var backImage = back.GetComponent<Image>();
            backImage.sprite = UiArt.BarBack();
            backImage.type = Image.Type.Sliced;
            return Fill(name + " Fill", back, colour);
        }

        // The coloured part of a bar: it fills its frame, and the HUD shortens it from the right.
        private static RectTransform Fill(string name, Transform parent, Color colour)
        {
            var fill = Panel(name, parent, colour);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            var image = fill.GetComponent<Image>();
            image.sprite = UiArt.BarFill();
            image.type = Image.Type.Sliced;
            return fill;
        }

        // A flask: a gourd that empties from the top.
        private static RectTransform Flask(string name, Transform parent, Vector2 position)
        {
            var back = Panel(name + " Back", parent, Color.white);
            back.anchorMin = back.anchorMax = back.pivot = new Vector2(0f, 1f);
            back.anchoredPosition = position;
            back.sizeDelta = new Vector2(36f, 48f);
            back.GetComponent<Image>().sprite = UiArt.FlaskBack();
            var fill = Panel(name + " Fill", back, new Color(1f, 0.8f, 0.36f));
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            var image = fill.GetComponent<Image>();
            image.sprite = UiArt.FlaskFill();
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Vertical;
            image.fillOrigin = (int)Image.OriginVertical.Bottom;
            return fill;
        }

        // A panel for menus and spoken words, with its inked frame.
        private static RectTransform Framed(string name, Transform parent)
        {
            var panel = Panel(name, parent, Color.white);
            var image = panel.GetComponent<Image>();
            image.sprite = UiArt.Panel();
            image.type = Image.Type.Sliced;
            return panel;
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
