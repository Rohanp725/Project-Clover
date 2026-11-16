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

        private static readonly Dictionary<string, float> Scales = new Dictionary<string, float>
        {
            { "soldier", 1.12f }, { "acolyte", 0.95f }, { "caster", 1.03f },
        };

        // One animator per kind for the whole scene, made once per build: making it again would orphan the earlier enemies.
        private static readonly Dictionary<string, AnimatorController> controllers = new Dictionary<string, AnimatorController>();

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
                BarStagger = BuildKit.Glow(Folder, "m_bar_stagger", new Color(1.4f, 1.1f, 0.3f, 1f), 3, false),
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

            Animator animator = null;
            var renderers = new Renderer[0];
            string modelPath = $"{ModelFolder}/enemy_{kind.Prefix}.fbx";
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
                model.transform.localScale = Vector3.one * Scales[kind.Prefix];
                var materials = Materials();
                renderers = model.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => ToonFor(imported, materials)).ToArray();
                animator = model.GetComponent<Animator>();
                if (animator == null) animator = model.AddComponent<Animator>();
                animator.runtimeAnimatorController = Controller(kind, modelPath);
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            var outline = WorldBars.Marker("Marker Outline", root.transform, 0.04f, kit.Marker);
            var fill = WorldBars.Marker("Marker Fill", root.transform, 0.06f, kit.Marker);
            var bars = WorldBars.Build(root.transform, 2.5f, kit.TowardCamera, kit.BarBack, kit.BarHealth, kit.BarStagger);

            var enemy = root.AddComponent<Enemy>();
            enemy.Configure(kind.Name, kit.Target, animator, renderers, outline, fill, bars.root, bars.health, bars.stagger, kit.Arcs, kit.Turns);
            return enemy;
        }

        private static Material ToonFor(Material imported, Dictionary<string, Material> materials)
        {
            string name = imported != null ? imported.name.Replace("m_enemy_", "") : "";
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
            return materials;
        }

        private static AnimatorController Controller(EnemyKind kind, string modelPath)
        {
            if (controllers.TryGetValue(kind.Prefix, out var made)) return made;
            string path = $"{Folder}/{kind.Prefix}.controller";
            AssetDatabase.DeleteAsset(path);
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__") && clip.name.StartsWith(kind.Prefix + "_")).ToDictionary(clip => clip.name);

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
            {
                if (!clips.TryGetValue(name, out var clip))
                {
                    Debug.LogWarning($"[Clover.Build] The model of {kind.Name} has no '{name}' clip; that will not be animated.");
                    continue;
                }
                if (name.EndsWith("_loop")) continue;
                machine.AddState(name).motion = clip;
            }
            EditorUtility.SetDirty(controller);
            controllers[kind.Prefix] = controller;
            return controller;
        }
    }
}
