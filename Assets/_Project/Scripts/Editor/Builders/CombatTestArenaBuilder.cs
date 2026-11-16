using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the walled yard where combat feel is tried out: paving, pillars to test the camera near tall things, the
    /// player with the Katana, dummies to hit and one dummy that hits back. Rebuilding replaces the scene and reuses the
    /// generated assets by name.
    /// </summary>
    public static class CombatTestArenaBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/CombatTest.unity";
        public const string ControlsPath = PlayerBuilder.ControlsPath;
        private const string Folder = BuildKit.GeneratedRoot + "/CombatTest";

        [MenuItem("Clover/Build/Combat Test Arena")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the arena")) return;
            // Materials of things this builder used to make.
            foreach (string old in new[] { "PlayerBody", "PlayerDark", "PlayerFront", "Ground", "Stone", "AttackMarker" })
                AssetDatabase.DeleteAsset($"{Folder}/{old}.mat");
            AssetDatabase.DeleteAsset($"{Folder}/FloorTiles.asset");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // The world is muted on purpose (docs/GAME.md): only elements carry strong colour.
            var paving = BuildKit.Toon(Folder, "m_yard_paving", new Color(0.32f, 0.33f, 0.43f), false, ProceduralTextures.Flagstones(), 0.125f, 0f);
            var stone = BuildKit.Toon(Folder, "m_yard_stone", new Color(0.38f, 0.37f, 0.5f), false, ProceduralTextures.WallBlocks(), 0.2f, 0.02f);
            var world = BuildKit.Group("World");
            Object.DestroyImmediate(BuildKit.Block("Ground", world, new Vector3(0f, -0.5f, 0f), new Vector3(44f, 1f, 44f), paving).GetComponent<Clover.World.Fadeable>());
            foreach (var (name, x, z) in new[] { ("North East", 7f, 7f), ("North West", -7f, 7f), ("South East", 7f, -7f), ("South West", -7f, -7f) })
                BuildKit.Block("Pillar " + name, world, new Vector3(x, 2.5f, z), new Vector3(2f, 5f, 2f), stone);
            // Walls are low on the camera's side so they never hide the player.
            BuildKit.Block("Wall North", world, new Vector3(0f, 2f, 21f), new Vector3(44f, 4f, 2f), stone);
            BuildKit.Block("Wall South", world, new Vector3(0f, 0.5f, -21f), new Vector3(44f, 1f, 2f), stone);
            BuildKit.Block("Wall East", world, new Vector3(21f, 1f, 0f), new Vector3(2f, 2f, 40f), stone);
            BuildKit.Block("Wall West", world, new Vector3(-21f, 1f, 0f), new Vector3(2f, 2f, 40f), stone);

            var built = PlayerBuilder.Build(Vector3.zero, PlayerBuilder.ControlsHint);
            if (built.Player == null) return;
            // The yard is where the Katana is practised; the castle starts the player with bare hands.
            built.Player.GetComponent<PlayerCombat>().StartingWeapon = Weapons.KatanaName;
            LookBuilder.Apply(LookBuilder.Yard, built.Camera);

            // One dummy alone, and a pair close together to see a swing catch both.
            var kit = new DummyKit(built.Camera.transform.rotation);
            foreach (var position in new[] { new Vector3(0f, 0f, 4f), new Vector3(5f, 0f, -2f), new Vector3(6.5f, 0f, -1.1f) })
                BuildDummy(position, kit, null);
            // And one that hits back, off to the west, to practise the guard and the dodge on. It leaves the player
            // alone until they walk up to it.
            BuildDummy(new Vector3(-7f, 0f, 3.5f), kit, built.Player.transform);

            SceneBuild.Finish(scene, ScenePath, false);
        }

        private class DummyKit
        {
            public readonly Quaternion TowardCamera;
            public readonly Material Wood = BuildKit.Toon(Folder, "DummyWood", new Color(0.36f, 0.25f, 0.22f)).Ink(0.028f, BuildKit.CharacterInk);
            public readonly Material Straw = BuildKit.Toon(Folder, "DummyStraw", new Color(0.6f, 0.47f, 0.3f)).Ink(0.028f, BuildKit.CharacterInk);
            public readonly Material Blade = BuildKit.Toon(Folder, "DummyBlade", new Color(0.2f, 0.18f, 0.17f)).Ink(0.028f, BuildKit.CharacterInk);
            public readonly Material Band = BuildKit.Toon(Folder, "DummyBand", new Color(0.62f, 0.14f, 0.08f)).Ink(0.028f, BuildKit.CharacterInk);
            public readonly Material BarBack = BuildKit.Glow(Folder, "BarBack", new Color(0.03f, 0.03f, 0.04f, 0.85f), 3, false);
            public readonly Material BarHealth = BuildKit.Glow(Folder, "BarHealth", new Color(0.75f, 0.09f, 0.08f, 1f), 3, false);
            public readonly Material BarStagger = BuildKit.Glow(Folder, "BarStagger", new Color(1.4f, 1.1f, 0.3f, 1f), 3, false);

            public DummyKit(Quaternion towardCamera) => TowardCamera = towardCamera;
        }

        // With a target the dummy hits back: it gets a sword arm, a red headband and the ground marker for its blow.
        private static void BuildDummy(Vector3 position, DummyKit kit, Transform target)
        {
            bool fights = target != null;
            var dummy = new GameObject(fights ? "Sparring Dummy" : "Training Dummy");
            dummy.transform.position = position;
            var capsule = dummy.AddComponent<CapsuleCollider>();
            capsule.radius = 0.45f;
            capsule.height = 1.9f;
            capsule.center = new Vector3(0f, 0.95f, 0f);

            // The pivot turns to face the player; everything that rocks hangs off the body inside it, at the ground.
            var pivot = BuildKit.Group("Pivot", dummy.transform);
            var body = BuildKit.Group("Body", pivot);
            var parts = new List<Renderer>
            {
                BuildKit.Shape(PrimitiveType.Cylinder, "Post", body, new Vector3(0f, 0.75f, 0f), new Vector3(0.22f, 0.75f, 0.22f), kit.Wood),
                BuildKit.Shape(PrimitiveType.Cylinder, "Torso", body, new Vector3(0f, 1.2f, 0f), new Vector3(0.62f, 0.38f, 0.62f), kit.Straw),
                BuildKit.Shape(PrimitiveType.Cube, "Arms", body, new Vector3(0f, 1.38f, 0f), new Vector3(1.5f, 0.12f, 0.12f), kit.Wood),
                BuildKit.Shape(PrimitiveType.Sphere, "Head", body, new Vector3(0f, 1.82f, 0f), new Vector3(0.44f, 0.44f, 0.44f), kit.Straw),
            };
            BuildKit.Shape(PrimitiveType.Cylinder, "Base", dummy.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.9f, 0.04f, 0.9f), kit.Wood);

            var bars = WorldBars.Build(dummy.transform, 2.45f, kit.TowardCamera, kit.BarBack, kit.BarHealth, kit.BarStagger);

            Transform arm = null;
            MeshFilter outline = null, fill = null;
            if (fights)
            {
                parts.Add(BuildKit.Shape(PrimitiveType.Cylinder, "Headband", body, new Vector3(0f, 1.9f, 0f), new Vector3(0.47f, 0.035f, 0.47f), kit.Band));
                arm = BuildKit.Group("Sword Arm", body);
                arm.localPosition = new Vector3(0.62f, 1.38f, 0f);
                parts.Add(BuildKit.Shape(PrimitiveType.Cube, "Wooden Sword", arm, new Vector3(0f, 0f, 0.8f), new Vector3(0.1f, 0.1f, 1.6f), kit.Blade));
                var marker = VfxBuilder.MarkerMaterial();
                outline = WorldBars.Marker("Marker Outline", pivot, 0.03f, marker);
                fill = WorldBars.Marker("Marker Fill", pivot, 0.04f, marker);
            }

            var training = dummy.AddComponent<TrainingDummy>();
            training.Configure(body, parts.ToArray(), bars.health, bars.stagger);
            training.ConfigureFeedback(WorldBars.FinishMark(dummy.transform, new Vector3(0f, 1.2f, 0f), 2.1f, kit.TowardCamera));
            if (fights) dummy.AddComponent<DummyAttacker>().Configure(pivot, arm, target, outline, fill);
        }
    }
}
