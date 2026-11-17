using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds one enemy into the open scene: its model in toon materials, an animator made from the model's clips, the
    /// mark it draws on the ground for its attacks, and its bars. The numbers and the behaviour come from
    /// <see cref="EnemyKind"/> by name, so an enemy in a scene is only a kind, a place and a facing.
    /// </summary>
    public static class EnemyBuilder
    {
        private const string Folder = BuildKit.GeneratedRoot + "/Enemies";
        private const string ModelFolder = "Assets/_Project/Art/Models/Characters";

        // Against a world of deep blue the enemies are warm and strong: bronze plate, crimson cloth, violet robes, with
        // the lightning that has got into them burning cyan on top.
        private static readonly (string name, Color colour)[] Palette =
        {
            ("cloth", new Color(0.56f, 0.13f, 0.3f)),
            ("armour", new Color(0.82f, 0.6f, 0.24f)),
            ("skin", new Color(0.62f, 0.6f, 0.68f)),
            ("dark", new Color(0.07f, 0.06f, 0.13f)),
            ("glow", new Color(0.7f, 0.9f, 1f)),
            ("wood", new Color(0.36f, 0.2f, 0.22f)),
            ("steel", new Color(0.72f, 0.76f, 0.82f)),
            ("wrap", new Color(0.74f, 0.7f, 0.56f)),
            ("accent", new Color(0.95f, 0.15f, 0.34f)),
            ("robe", new Color(0.66f, 0.2f, 0.78f)),
            ("bone", new Color(0.92f, 0.88f, 0.74f)),
        };

        // The god of Lightning: storm-grey skin and white hair, black and gold, red drums, and the lightning itself.
        private static readonly (string name, Color colour)[] BossPalette =
        {
            ("skin", new Color(0.62f, 0.68f, 0.82f)),
            ("hair", new Color(0.95f, 0.96f, 1f)),
            ("cloth", new Color(0.13f, 0.11f, 0.26f)),
            ("haori", new Color(0.92f, 0.92f, 0.98f)),
            ("gold", new Color(0.98f, 0.72f, 0.16f)),
            ("glow", new Color(0.75f, 0.95f, 1f)),
            ("dark", new Color(0.06f, 0.05f, 0.12f)),
            ("steel", new Color(0.8f, 0.85f, 0.95f)),
            ("drum", new Color(0.82f, 0.12f, 0.16f)),
            ("rope", new Color(0.86f, 0.72f, 0.4f)),
        };

        // The Gate Captain wears the soldier's body in his own colours, so that he is known at a glance from the far end
        // of the ward: black iron where they wear bronze, dull gold cloth where they wear crimson, a bone-white banner
        // at his back, and the lightning in him burning far brighter than in any of them.
        private static readonly (string name, Color colour)[] CaptainPalette =
        {
            ("cloth", new Color(0.6f, 0.43f, 0.14f)),
            ("armour", new Color(0.15f, 0.15f, 0.23f)),
            ("skin", new Color(0.5f, 0.55f, 0.7f)),
            ("dark", new Color(0.05f, 0.045f, 0.1f)),
            ("glow", new Color(0.85f, 0.97f, 1f)),
            ("wood", new Color(0.12f, 0.1f, 0.16f)),
            ("steel", new Color(0.86f, 0.9f, 1f)),
            ("accent", new Color(0.95f, 0.93f, 0.86f)),
        };

        // One animator per kind for the whole scene, made once per build: making it again would orphan the earlier enemies.
        private static readonly Dictionary<string, AnimatorController> controllers = new Dictionary<string, AnimatorController>();
        // How many enemies this scene has so far: neighbours carry their bars at different heights, so a crowd's bars do not lie on one another.
        private static int built;

        public struct Kit
        {
            public Transform Target;
            public Quaternion TowardCamera;
            public SlashArcs Arcs;
            public EnemyTurns Turns;
            public Material Marker, BarBack, BarHealth, BarStagger;
        }

        /// <summary>What every enemy in a scene shares: whom they fight, the camera's facing for their bars, and the materials.</summary>
        public static Kit NewKit(Transform target, Quaternion towardCamera)
        {
            controllers.Clear();
            built = 0;
            var arcsObject = new GameObject("Enemy Slash Arcs");
            var arcs = arcsObject.AddComponent<SlashArcs>();
            arcs.Capacity = 8;
            arcs.Configure(VfxBuilder.SlashMaterial());
            return new Kit
            {
                Target = target,
                TowardCamera = towardCamera,
                Arcs = arcs,
                Turns = new GameObject("Enemy Turns").AddComponent<EnemyTurns>(),
                Marker = VfxBuilder.MarkerMaterial(),
                BarBack = BuildKit.Glow(Folder, "m_bar_back", new Color(0.03f, 0.03f, 0.04f, 0.85f), 3, false),
                BarHealth = BuildKit.Glow(Folder, "m_bar_health", new Color(0.75f, 0.09f, 0.08f, 1f), 3, false),
                BarStagger = BuildKit.Glow(Folder, "m_bar_stagger", new Color(1.4f, 1.1f, 0.3f, 1f), 3, false, 2f, 0f, true),
            };
        }

        public static Enemy Build(string kindName, Vector3 position, float yawDegrees, Kit kit, Transform parent)
        {
            var kind = EnemyKind.ByName(kindName);
            var root = new GameObject(kind.Name);
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yawDegrees, 0f));
            var capsule = root.AddComponent<CharacterController>();
            capsule.height = 1.9f;
            capsule.radius = 0.42f;
            capsule.center = new Vector3(0f, 0.95f, 0f);
            // How much bigger than a soldier it is drawn: what stands over and around it is sized to match.
            float size = Mathf.Max(1f, kind.Scale / 1.12f);
            if (size > 1.1f)
            {
                capsule.height = 1.9f * size;
                capsule.radius = 0.42f * size;
                capsule.center = new Vector3(0f, 0.95f * size, 0f);
            }

            Animator animator = null;
            var renderers = new Renderer[0];
            string modelPath = $"{ModelFolder}/{kind.ModelName}.fbx";
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                Debug.LogError($"[Clover.Build] No enemy model at {modelPath}; {kind.Name} was built without a body.");
            }
            else
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localScale = Vector3.one * kind.Scale;
                var materials = Materials();
                renderers = model.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => ToonFor(imported, materials, kind.Palette)).ToArray();
                animator = model.GetComponent<Animator>();
                if (animator == null) animator = model.AddComponent<Animator>();
                animator.runtimeAnimatorController = Controller(kind, modelPath);
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            var outline = WorldBars.Marker("Marker Outline", root.transform, 0.04f, kit.Marker);
            var fill = WorldBars.Marker("Marker Fill", root.transform, 0.06f, kit.Marker);
            var bars = WorldBars.Build(root.transform, 2.3f * size + (built++ % 2) * 0.24f, kit.TowardCamera, kit.BarBack, kit.BarHealth, kit.BarStagger);
            if (kind.IsElite) WorldBars.NamePlate(bars.root, kind.Name);
            var finishMark = WorldBars.FinishMark(root.transform, new Vector3(0f, 1.25f * size, 0f), 2.1f * size, kit.TowardCamera);

            GuardArc guardArc = null;
            if (kind.Guards)
            {
                guardArc = root.AddComponent<GuardArc>();
                guardArc.Configure(VfxBuilder.GuardArcMaterial(), 1.25f * size, 150f, new Color(2.4f, 1.8f, 0.8f, 0.85f));
            }
            var enemy = root.AddComponent<Enemy>();
            enemy.Configure(kind.Name, kit.Target, animator, renderers, outline, fill, bars.root, bars.health, bars.stagger, kit.Arcs, kit.Turns, guardArc);
            enemy.ConfigureFeedback(finishMark);
            return enemy;
        }

        /// <summary>Puts the enemies' toon materials on a copy of one of their models, as the game does; a kind with colours of its own gets those.</summary>
        public static void Dress(GameObject model, string palette = "")
        {
            var materials = Materials();
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => ToonFor(imported, materials, palette)).ToArray();
        }

        private static Material ToonFor(Material imported, Dictionary<string, Material> materials, string palette)
        {
            // The Fire land's people are painted by its own kit; a gilded soldier's palette carries his rank.
            if (imported != null && SunCityKit.Knows(imported.name))
                return SunCityKit.For(imported.name, palette != null && palette.StartsWith("gilded") && palette.Length == 7 ? palette[6] - '0' : 3);
            string name = imported != null ? imported.name.Replace("m_enemy_", "").Replace("m_boss_", "boss:") : "";
            // A kind with its own colours takes them where it has them and the shared ones where it does not.
            if (!string.IsNullOrEmpty(palette) && materials.TryGetValue(palette + ":" + name, out var own)) return own;
            if (materials.TryGetValue(name, out var toon)) return toon;
            Debug.LogWarning($"[Clover.Build] An enemy uses a material '{name}' that the palette does not know.");
            return imported;
        }

        private static Dictionary<string, Material> Materials()
        {
            var materials = new Dictionary<string, Material>();
            foreach (var (name, colour) in Palette)
            {
                var material = BuildKit.Toon(Folder, $"m_enemy_{name}", colour, true);
                // The infection glows far past white, so the bloom makes it the brightest thing on the body.
                if (name == "glow") material.Lit(new Color(0.6f, 0.9f, 1.6f), 0.5f, new Color(1.1f, 2.4f, 5.5f));
                else material.Lit(new Color(0.6f, 0.75f, 1.2f), 0.32f);
                materials[name] = material;
            }
            foreach (var (name, colour) in BossPalette)
            {
                var material = BuildKit.Toon(Folder, $"m_boss_{name}", colour, true);
                if (name == "glow") material.Lit(new Color(0.8f, 1f, 1.6f), 0.5f, new Color(1.6f, 3.2f, 6f));
                else if (name == "gold") material.Lit(new Color(1.2f, 0.9f, 0.4f), 0.4f, new Color(0.25f, 0.14f, 0.02f));
                else material.Lit(new Color(0.7f, 0.85f, 1.3f), 0.34f);
                materials["boss:" + name] = material;
            }
            foreach (var (name, colour) in CaptainPalette)
            {
                var material = BuildKit.Toon(Folder, $"m_captain_{name}", colour, true);
                if (name == "glow") material.Lit(new Color(0.8f, 1f, 1.6f), 0.5f, new Color(2.2f, 4.6f, 9f));
                else if (name == "accent") material.Lit(new Color(0.9f, 1f, 1.3f), 0.4f, new Color(0.16f, 0.17f, 0.2f));
                // Black iron would be lost against the night without a strong cold edge to it.
                else if (name == "armour" || name == "wood" || name == "dark") material.Lit(new Color(0.9f, 1.2f, 2f), 0.5f);
                else material.Lit(new Color(0.7f, 0.85f, 1.3f), 0.34f);
                materials["captain:" + name] = material;
            }
            return materials;
        }

        private static AnimatorController Controller(EnemyKind kind, string modelPath)
        {
            if (controllers.TryGetValue(kind.Prefix, out var made)) return made;
            string path = $"{Folder}/{kind.Prefix}.controller";
            AssetDatabase.DeleteAsset(path);
            // The animations are written as code and baked by Clover > Forge > Bake Enemy Animations.
            var clips = AssetDatabase.FindAssets("t:AnimationClip", new[] { Forge.EnemyAnimations.ClipFolder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(clip => clip != null && clip.name.StartsWith(kind.Prefix + "_")).ToDictionary(clip => clip.name);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var machine = controller.layers[0].stateMachine;
            var locomotion = controller.CreateBlendTreeInController("locomotion", out BlendTree tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            if (clips.TryGetValue(kind.Prefix + "_idle_loop", out var idle)) tree.AddChild(idle, 0f);
            if (clips.TryGetValue(kind.Prefix + "_walk_loop", out var walk)) tree.AddChild(walk, 1f);
            machine.defaultState = locomotion;

            var needed = new List<string> { kind.Prefix + "_idle_loop", kind.Prefix + "_walk_loop", kind.Prefix + "_hurt", kind.Prefix + "_stagger", kind.Prefix + "_death" };
            needed.AddRange(kind.Brain.Attacks.Select(attack => attack.Name));
            foreach (string name in needed)
                if (!clips.ContainsKey(name)) Debug.LogWarning($"[Clover.Build] The model of {kind.Name} has no '{name}' clip; that will not be animated.");
            // Every clip of the prefix gets a state: kinds that share a prefix (the gilded ranks) share one animator,
            // and the clips that are not attacks (drinking, holding, rising, lying dormant, searching) are played by name too.
            foreach (var pair in clips.OrderBy(pair => pair.Key))
            {
                if (pair.Key == kind.Prefix + "_idle_loop" || pair.Key == kind.Prefix + "_walk_loop") continue;
                machine.AddState(pair.Key).motion = pair.Value;
            }
            EditorUtility.SetDirty(controller);
            controllers[kind.Prefix] = controller;
            return controller;
        }
    }
}
