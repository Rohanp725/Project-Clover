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
            BuildFormRack(world, built.Player, BuildWeaponRack(world, built.Player));

            SceneBuild.Finish(scene, ScenePath, false);
        }

        // The rack of stones along the south side: one for every new spell form, each lending its sample spell, and a
        // taller one in front of them that gives the next element round the ring. Only this yard has them.
        // The rack of weapons along the north side: every weapon that is built, to be taken up and tried.
        private static List<Clover.World.Interactable> BuildWeaponRack(Transform world, GameObject player)
        {
            var rack = BuildKit.Group("Weapon Rack", world);
            var plinth = BuildKit.Toon(Folder, "m_rack_plinth", new Color(0.3f, 0.3f, 0.4f), false, null, 0f, 0.02f);
            var chosen = BuildKit.Glow(Folder, "m_rack_chosen", new Color(1.6f, 1.3f, 0.6f, 0.8f), 4, true, 2f);
            var combat = player.GetComponent<PlayerCombat>();
            var element = player.GetComponent<PlayerElement>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var stands = new List<Clover.World.Interactable>();
            var names = Weapons.Offered;
            for (int i = 0; i < names.Length; i++)
            {
                var stand = new GameObject(names[i] + " Rack Stand");
                stand.transform.SetParent(rack, false);
                stand.transform.position = new Vector3((i - (names.Length - 1) * 0.5f) * 3.2f, 0f, 12.5f);
                stand.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                BuildKit.Shape(PrimitiveType.Cylinder, "Foot", stand.transform, new Vector3(0f, 0.12f, 0f), new Vector3(1.3f, 0.12f, 1.3f), plinth);
                var shown = BuildKit.Group("Shown", stand.transform);
                WeaponBuilder.Display(Weapons.ByName(names[i]).Carry, shown);
                var mark = BuildKit.Group("Chosen", stand.transform);
                BuildKit.Shape(PrimitiveType.Quad, "Chosen Ring", mark, new Vector3(0f, 0.3f, 0f), new Vector3(2.6f, 2.6f, 1f), chosen, new Vector3(90f, 0f, 0f));
                var words = new GameObject("Label");
                words.transform.SetParent(stand.transform, false);
                words.transform.position = stand.transform.position + new Vector3(0f, 0.5f, -1.3f);
                words.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
                var text = words.AddComponent<TextMesh>();
                text.font = font;
                words.GetComponent<MeshRenderer>().sharedMaterial = font.material;
                text.text = names[i].ToUpperInvariant();
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.fontSize = 48;
                text.characterSize = 0.06f;
                text.color = new Color(0.9f, 0.88f, 0.8f);
                var choice = stand.AddComponent<Clover.World.LoadoutChoice>();
                choice.Configure(false, names[i], true, shown.gameObject, mark.gameObject);
                choice.Watch(combat, element);
                choice.OpenPrompt = "Take up: " + names[i];
                choice.Prompt = choice.OpenPrompt;
                choice.Range = 1.7f;
                stands.Add(choice);
            }
            return stands;
        }

        private static void BuildFormRack(Transform world, GameObject player, List<Clover.World.Interactable> stones)
        {
            var rack = BuildKit.Group("Form Rack", world);
            var plinth = BuildKit.Toon(Folder, "m_rack_plinth", new Color(0.3f, 0.3f, 0.4f), false, null, 0f, 0.02f);
            var samples = Clover.Player.Forms.FormSamples.All;
            var upgrades = player.GetComponent<PlayerUpgrades>();
            var combat = player.GetComponent<PlayerCombat>();
            for (int i = 0; i < samples.Count; i++)
            {
                var sample = samples[i];
                var colour = Elements.Colour(sample.Element);
                var stone = new GameObject("Form Stone " + sample.Form);
                stone.transform.SetParent(rack, false);
                stone.transform.position = new Vector3((i - (samples.Count - 1) * 0.5f) * 1.5f, 0f, -16f);
                BuildKit.Shape(PrimitiveType.Cube, "Plinth", stone.transform, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.6f, 0.6f), plinth);
                var gem = BuildKit.Toon(Folder, "m_rack_gem_" + sample.Element.ToLowerInvariant(), colour, false, null, 0f, 0.02f)
                    .Lit(Color.black, 0.3f, new Color(colour.r * 1.6f, colour.g * 1.6f, colour.b * 1.6f));
                BuildKit.Shape(PrimitiveType.Cube, "Gem", stone.transform, new Vector3(0f, 0.85f, 0f), new Vector3(0.3f, 0.3f, 0.3f), gem, new Vector3(45f, 35f, 0f));
                var lends = stone.AddComponent<Clover.World.FormStone>();
                lends.Configure(sample.Form, $"Try {sample.Name} ({sample.Form}, {sample.Element})", i == 0);
                lends.Range = 1f;
                lends.Watch(upgrades, combat);
                stones.Add(lends);
                Words(stone.transform, sample.Name, new Vector3(0f, 0.05f, -0.75f), 0.028f, Color.Lerp(colour, Color.white, 0.35f));
                Card(stone.transform, lends, 1.1f, $"{sample.Name.ToUpperInvariant()} ({sample.Element}, the '{sample.Form}' form)\n{sample.Description}\nPress use (E) to wear it. {TriggerWords.HowTo(sample)}\nMana comes back by itself while a stone's spell is worn.");
            }
            var turn = new GameObject("Element Stone");
            turn.transform.SetParent(rack, false);
            turn.transform.position = new Vector3(0f, 0f, -12.5f);
            BuildKit.Shape(PrimitiveType.Cylinder, "Plinth", turn.transform, new Vector3(0f, 0.5f, 0f), new Vector3(0.8f, 0.5f, 0.8f), plinth);
            BuildKit.Shape(PrimitiveType.Sphere, "Gem", turn.transform, new Vector3(0f, 1.3f, 0f), new Vector3(0.45f, 0.45f, 0.45f),
                BuildKit.Toon(Folder, "m_rack_gem_any", new Color(0.9f, 0.9f, 0.95f), false, null, 0f, 0.02f).Lit(Color.black, 0.3f, new Color(1.4f, 1.4f, 1.5f)));
            var changes = turn.AddComponent<Clover.World.FormStone>();
            changes.Configure("", "Take the next element", false);
            changes.Range = 1.6f;
            stones.Add(changes);
            Words(turn.transform, "ELEMENT", new Vector3(0f, 2.1f, 0f), 0.07f, new Color(0.9f, 0.9f, 0.95f));
            Card(turn.transform, changes, 2.4f, "");
            // Two debug shrines beside the element stone, for trying the gods' gifts while only one god exists.
            stones.Add(BuildShrine(rack, plinth, new Vector3(-3f, 0f, -12.5f), false, "GOD GIFT", "Debug shrine: take the god's gift of the element you hold", new Color(1f, 0.85f, 0.35f)));
            stones.Add(BuildShrine(rack, plinth, new Vector3(3f, 0f, -12.5f), true, "CONSUME NEXT", "Debug shrine: consume the next element round the ring (opens the mix, gives its crown)", new Color(0.85f, 0.45f, 1f)));
            player.GetComponent<PlayerInteraction>().Configure(stones.ToArray());
        }

        private static Clover.World.Interactable BuildShrine(Transform rack, Material plinth, Vector3 at, bool consumes, string label, string prompt, Color colour)
        {
            var shrine = new GameObject(consumes ? "Debug Shrine Consume" : "Debug Shrine Gift");
            shrine.transform.SetParent(rack, false);
            shrine.transform.position = at;
            BuildKit.Shape(PrimitiveType.Cube, "Plinth", shrine.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.7f, 1.2f, 0.7f), plinth);
            var gem = BuildKit.Toon(Folder, consumes ? "m_shrine_consume" : "m_shrine_gift", colour, false, null, 0f, 0.02f).Lit(Color.black, 0.3f, new Color(colour.r * 1.6f, colour.g * 1.6f, colour.b * 1.6f));
            BuildKit.Shape(PrimitiveType.Cube, "Gem", shrine.transform, new Vector3(0f, 1.55f, 0f), new Vector3(0.4f, 0.4f, 0.4f), gem, new Vector3(45f, 35f, 0f));
            // Its name stands over it, turned to the camera, so it cannot be mistaken for a form stone.
            var words = new GameObject("Label");
            words.transform.SetParent(shrine.transform, false);
            words.transform.localPosition = new Vector3(0f, 2.3f, 0f);
            words.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
            var text = words.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.font = font;
            words.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            text.text = label;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 48;
            text.characterSize = 0.07f;
            text.color = colour;
            var gives = shrine.AddComponent<Clover.World.GiftShrine>();
            gives.Configure(consumes, prompt);
            gives.Range = 1.4f;
            Card(shrine.transform, gives, 2.2f, "");
            return gives;
        }

        // A line of words standing in the world, leaned back toward the camera.
        private static TextMesh Words(Transform over, string words, Vector3 localAt, float size, Color colour)
        {
            var holder = new GameObject("Label");
            holder.transform.SetParent(over, false);
            holder.transform.localPosition = localAt;
            holder.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            var text = holder.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.font = font;
            holder.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            text.text = words;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 48;
            text.characterSize = size;
            text.color = colour;
            return text;
        }

        // The card over a thing: what it gives, what that does and what sets it going, shown while the player is near.
        // With no words of its own the thing is asked, since what a shrine gives depends on the element held.
        private static void Card(Transform over, Clover.World.Interactable thing, float showWithin, string words)
        {
            var whole = BuildKit.Group("Card", over);
            whole.localPosition = new Vector3(0f, 3.9f, 0f);
            whole.rotation = Quaternion.Euler(55f, 0f, 0f);
            var back = BuildKit.Shape(PrimitiveType.Quad, "Back", whole, Vector3.zero, new Vector3(3f, 1.2f, 1f),
                BuildKit.Glow(Folder, "m_card_back", new Color(0.03f, 0.03f, 0.05f, 0.9f), 3, false, 2f, 0f, true));
            var text = Words(whole, "", new Vector3(0f, 0f, -0.03f), 0.03f, new Color(0.95f, 0.93f, 0.86f));
            text.transform.localRotation = Quaternion.identity;
            text.alignment = TextAlignment.Left;
            // The words are drawn after their backing, whatever the distance sorting says.
            text.GetComponent<MeshRenderer>().sortingOrder = 10;
            var card = over.gameObject.AddComponent<Clover.World.StoneCard>();
            card.Words = words;
            card.ShowWithin = showWithin;
            card.Configure(thing, text, back.transform, whole.gameObject, 0.17f, 0.075f);
        }

        private class DummyKit
        {
            public readonly Quaternion TowardCamera;
            public readonly Material Wood = BuildKit.Toon(Folder, "DummyWood", new Color(0.36f, 0.25f, 0.22f)).Ink(0.028f, BuildKit.CharacterInk);
            public readonly Material Straw = BuildKit.Toon(Folder, "DummyStraw", new Color(0.6f, 0.47f, 0.3f)).Ink(0.028f, BuildKit.CharacterInk);
            public readonly Material Blade = BuildKit.Toon(Folder, "DummyBlade", new Color(0.2f, 0.18f, 0.17f)).Ink(0.028f, BuildKit.CharacterInk);
            public readonly Material Band = BuildKit.Toon(Folder, "DummyBand", new Color(0.62f, 0.14f, 0.08f)).Ink(0.028f, BuildKit.CharacterInk);
            public readonly Material BarBack = BuildKit.Glow(Folder, "BarBack", new Color(0.03f, 0.03f, 0.04f, 0.85f), 3, false, 2f, 0f, true);
            public readonly Material BarHealth = BuildKit.Glow(Folder, "BarHealth", new Color(0.75f, 0.09f, 0.08f, 1f), 3, false, 2f, 0f, true);
            public readonly Material BarStagger = BuildKit.Glow(Folder, "BarStagger", new Color(1.4f, 1.1f, 0.3f, 1f), 3, false, 2f, 0f, true);

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
