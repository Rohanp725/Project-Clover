using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the slice's first area: the lower wards of the castle under the storm. The player crosses a causeway to the
    /// gate, fights through the outer court, climbs to the shrine terrace where the Katana waits, and climbs again to the
    /// upper ward and the sealed door to the summit. North is up the screen and up the hill. Walls on the camera's side
    /// are kept low so they never hide the fight; the tall walls and towers stand to the north as a backdrop.
    /// </summary>
    public static class CastleBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Castle.unity";
        private const string Folder = BuildKit.GeneratedRoot + "/Castle";

        private static Material paving, path, wall, wallDark, trim, wood, iron, cloth, earth, roof, ember, steel, rune;
        private static Material fireMaterial, rainMaterial, boltMaterial;
        private static Transform world;
        internal static readonly List<Transform> rods = new List<Transform>();
        private static readonly List<Interactable> things = new List<Interactable>();

        [MenuItem("Clover/Build/Castle")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the castle")) return;
            rods.Clear();
            things.Clear();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Materials();
            world = BuildKit.Group("World");

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, 2f), "");
            if (built.Player == null) return;
            var storm = LookBuilder.Apply(LookBuilder.StormNight, built.Camera);
            if (storm != null)
            {
                // The rods bring the flashes; the sky by itself only mutters now and then.
                storm.MinSecondsBetween = 7f;
                storm.MaxSecondsBetween = 16f;
            }
            Weather(built.Player.transform, storm);

            var start = BuildKit.Group("Start Point", world);
            start.position = new Vector3(0f, 0.05f, 2f);

            var enemies = new List<Enemy>();
            var kit = EnemyBuilder.NewKit(built.Player.transform, built.Camera.transform.rotation);
            var enemyRoot = BuildKit.Group("Enemies");
            Enemy Spawn(string kind, float x, float y, float z, float yaw)
            {
                var enemy = EnemyBuilder.Build(kind, new Vector3(x, y + 0.05f, z), yaw, kit, enemyRoot);
                enemies.Add(enemy);
                return enemy;
            }

            var director = new GameObject("Area Director").AddComponent<AreaDirector>();

            Dressing.Prepare();
            Causeway(director);
            Gatehouse();
            OuterCourt();
            Spawn(EnemyKind.SoldierName, -3f, 0f, 40f, 180f);
            Spawn(EnemyKind.SoldierName, 6f, 0f, 50f, 200f);
            Spawn(EnemyKind.AcolyteName, -9f, 0f, 53f, 150f);

            ShrineTerrace(director);
            Spawn(EnemyKind.AcolyteName, -6f, 2f, 78f, 170f);
            Spawn(EnemyKind.AcolyteName, 7f, 2f, 80f, 190f);
            Spawn(EnemyKind.CasterName, 0f, 2f, 85f, 180f);

            UpperWard(director);
            // The ward is two grounds with a line of pale stone between them. The five who hold its south end keep to
            // that end (and its stair): they notice nobody north of the line and follow nobody over it. The captain
            // keeps the north end, before the door, and is fought there alone.
            foreach (var enemy in new[]
            {
                Spawn(EnemyKind.SoldierName, -6f, 4f, 104f, 180f),
                Spawn(EnemyKind.SoldierName, 6f, 4f, 104f, 180f),
                Spawn(EnemyKind.AcolyteName, 0f, 4f, 108f, 180f),
                Spawn(EnemyKind.CasterName, -11f, 4f, 110f, 160f),
                Spawn(EnemyKind.CasterName, 11f, 4f, 108f, 200f),
            })
                enemy.HoldGround(-19f, 19f, 86f, CaptainsLine - 0.5f);
            Spawn(EnemyKind.CaptainName, 0f, 4f, 119.5f, 180f).HoldGround(-19f, 19f, CaptainsLine + 0.5f, 126f);

            var strikeOutline = WorldBars.Marker("Storm Mark Outline", director.transform, 0f, kit.Marker);
            var strikeFill = WorldBars.Marker("Storm Mark Fill", director.transform, 0f, kit.Marker);
            director.Configure(built.Player.GetComponent<PlayerController>(), enemies.ToArray(), start, strikeOutline, strikeFill);
            kit.Turns.Configure(enemies.ToArray());
            built.Player.GetComponent<PlayerInteraction>().Configure(things.ToArray());
            built.Player.GetComponent<PlayerCombat>().StartingWeapon = Weapons.FistsName;
            PlayerBuilder.AddRunEnd(built.Player);
            PlayerBuilder.AddTitle("The Castle Under the Storm", "where a swordsman became a god");

            var rodsObject = new GameObject("Storm Rods").AddComponent<StormRods>();
            rodsObject.Configure(rods.ToArray());

            SceneBuild.Finish(scene, ScenePath, false);
        }

        internal static void Materials()
        {
            var flags = ProceduralTextures.Flagstones();
            var blocks = ProceduralTextures.WallBlocks();
            var planks = ProceduralTextures.Planks();
            var ground = ProceduralTextures.Ground();
            // An illustrated night: stone in deep blues and purples, wood and moss murky, and the strong colour kept for
            // banners, fire, lightning and the living.
            paving = BuildKit.Toon(Folder, "m_castle_paving", new Color(0.3f, 0.31f, 0.4f), false, flags, 0.125f, 0f);
            // The way through the middle of each court is laid in smaller, paler stone: it leads the eye north.
            path = BuildKit.Toon(Folder, "m_castle_path", new Color(0.4f, 0.38f, 0.47f), false, flags, 0.21f, 0f);
            wall = BuildKit.Toon(Folder, "m_castle_wall", new Color(0.36f, 0.35f, 0.47f), false, blocks, 0.2f, 0.025f);
            wallDark = BuildKit.Toon(Folder, "m_castle_wall_dark", new Color(0.22f, 0.21f, 0.32f), false, blocks, 0.2f, 0.025f);
            trim = BuildKit.Toon(Folder, "m_castle_trim", new Color(0.46f, 0.46f, 0.58f), false, ground, 0.5f, 0.02f);
            wood = BuildKit.Toon(Folder, "m_castle_wood", new Color(0.36f, 0.22f, 0.28f), false, planks, 0.5f, 0.02f);
            iron = BuildKit.Toon(Folder, "m_castle_iron", new Color(0.16f, 0.15f, 0.27f), false, null, 0f, 0.02f);
            cloth = BuildKit.Toon(Folder, "m_castle_cloth", new Color(0.82f, 0.13f, 0.22f), false, null, 0f, 0.015f);
            earth = BuildKit.Toon(Folder, "m_castle_earth", new Color(0.2f, 0.3f, 0.29f), false, ground, 0.15f, 0f);
            roof = BuildKit.Toon(Folder, "m_castle_roof", new Color(0.14f, 0.3f, 0.38f), false, ProceduralTextures.RoofTiles(), 0.35f, 0.03f);
            steel = BuildKit.Toon(Folder, "m_castle_steel", new Color(0.75f, 0.78f, 0.84f), false, null, 0f, 0.012f).Lit(new Color(1.4f, 1.6f, 2f), 0.6f, new Color(0.25f, 0.4f, 0.8f));
            ember = BuildKit.Toon(Folder, "m_castle_ember", new Color(0.9f, 0.45f, 0.15f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(1.6f, 0.5f, 0.1f));
            rune = BuildKit.Toon(Folder, "m_castle_rune", new Color(0.6f, 0.8f, 1f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.9f, 2f, 4.5f));
            fireMaterial = BuildKit.Flame(Folder, "m_castle_flame", new Color(1.6f, 0.5f, 0.1f, 1f));
            rainMaterial = BuildKit.Glow(Folder, "m_castle_rain", new Color(0.8f, 0.9f, 1.3f, 0.7f), 1, true, 1f);
            boltMaterial = BuildKit.Glow(Folder, "m_castle_bolt", new Color(3f, 4.4f, 8f, 1f), 3, true, 1f);
        }

        // ------------------------------------------------------------------ the parts of the castle

        // A stone bridge over the drop, with a brazier to rest at before the gate.
        private static void Causeway(AreaDirector director)
        {
            var group = BuildKit.Group("Causeway", world);
            Slab("Deck", group, -5f, 5f, -4f, 26f, 0f, paving);
            Slab("Deck Underside", group, -5.5f, 5.5f, -4.5f, 26f, -1.2f, wallDark, 6f);
            foreach (float x in new[] { -5.4f, 5.4f })
            {
                // The storm has bitten a length out of each parapet: on the west the stones lie on the deck, on the east
                // a guardian fell through it.
                float gapSouth = x < 0f ? 12.3f : 6.4f, gapNorth = x < 0f ? 16.2f : 10.4f;
                Wall("Parapet", group, new Vector3(x, 0f, -4f), new Vector3(x, 0f, gapSouth), 1.1f, 0.6f, wall);
                Wall("Parapet", group, new Vector3(x, 0f, gapNorth), new Vector3(x, 0f, 26f), 1.1f, 0.6f, wall);
                BrokenParapet(group, x, gapSouth, gapNorth);
                for (float z = -2f; z <= 24f; z += 6.5f)
                {
                    Pillar("Parapet Post", group, new Vector3(x, 0f, z), 0.9f, 2.2f, wall, trim);
                }
            }
            Wall("Parapet End", group, new Vector3(-5.4f, 0f, -4.2f), new Vector3(5.4f, 0f, -4.2f), 1.1f, 0.6f, wall);
            Brazier(group, new Vector3(-3.8f, 0f, 8f), true);
            Brazier(group, new Vector3(3.8f, 0f, 20f), true);
            Rest(group, director, new Vector3(3.4f, 0f, 3.2f), new Vector3(1.6f, 0.05f, 2.4f));
            Rubble(group, new Vector3(-3.6f, 0f, 15f), 5);

            var dress = BuildKit.Group("Dressing", group);
            Dressing.Puddles(dress, -4f, 4f, -2f, 25f, 0f, 5, 11);
            Dressing.GrassLine(dress, new Vector3(-4.95f, 0f, -3f), new Vector3(-4.95f, 0f, 25f), 0.35f, 12);
            Dressing.GrassLine(dress, new Vector3(4.95f, 0f, 25f), new Vector3(4.95f, 0f, -3f), 0.35f, 13);
            Dressing.Lantern(dress, new Vector3(4.5f, 0f, 12f), 180f);
            Dressing.Lantern(dress, new Vector3(-4.5f, 0f, 22f), 0f);
            Dressing.Spears(dress, new Vector3(2.6f, 0f, 16.5f), 3, 14, cloth);
            Dressing.Barrel(dress, new Vector3(-4.2f, 0f, 3.6f), 20f);
            Dressing.Sacks(dress, new Vector3(3.6f, 0f, -2.6f), 160f);
            CausewayArt(group, dress);
        }

        // The gate: two towers, the wall between them, and the open arch the player walks through.
        private static void Gatehouse()
        {
            var group = BuildKit.Group("Gatehouse", world);
            foreach (float side in new[] { -1f, 1f })
            {
                // The gate towers rise from the drop beside the causeway, south of where the camera ever stands while the
                // player is in the court: nothing as high as the camera may stand where the camera goes.
                Tower(group, new Vector3(side * 9.5f, -6f, 16f), 6f, 17f);
                // The curtain wall runs out from each tower along the south side of the court; low, for the camera.
                Wall("Curtain South", group, new Vector3(side * 5.5f, 0f, 28f), new Vector3(side * 19f, 0f, 28f), 2.2f, 1.6f, wall);
                // The arch over the gate fell long ago; what is left of its jambs is low enough to see past.
                Block("Gate Jamb", group, new Vector3(side * 4.3f, side > 0f ? 1.6f : 1.1f, 28f), new Vector3(2.4f, side > 0f ? 3.2f : 2.2f, 3f), wall);
            }
            Rubble(group, new Vector3(-4.5f, 0f, 31f), 6);
            Block("Gate Sill", group, new Vector3(0f, -0.05f, 28f), new Vector3(6.2f, 0.12f, 3.4f), trim);
            Banner(group, new Vector3(-9.5f, 8f, 12.9f), 0f);
            Banner(group, new Vector3(9.5f, 8f, 12.9f), 0f);

            // The gate itself lies where it was thrown down, and one torch still burns on the taller jamb.
            var dress = BuildKit.Group("Dressing", group);
            Dressing.Portcullis(dress, new Vector3(8.6f, 0f, 31.4f), 24f);
            Torch(dress, new Vector3(4.3f, 2.3f, 26.5f), 180f);
            Dressing.Chain(dress, new Vector3(3.6f, 3.1f, 26.45f), new Vector3(5.1f, 1.2f, 26.45f), 0.5f, 7);
            Dressing.IvyPatch(dress, new Vector3(-4.3f, 0f, 26.5f), 180f, 2.2f, 2f, 201);
        }

        // The first fights: an open paved court with cover, a struck tree, and the stair north.
        private static void OuterCourt()
        {
            var group = BuildKit.Group("Outer Court", world);
            Slab("Court Floor", group, -19f, 19f, 26f, 62f, 0f, paving);
            Slab("Court Earth", group, -14f, -4f, 33f, 45f, 0.01f, earth, 0.02f);
            Wall("Court West", group, new Vector3(-19.5f, 0f, 28f), new Vector3(-19.5f, 0f, 62f), 4.5f, 1.6f, wall);
            Wall("Court East", group, new Vector3(19.5f, 0f, 28f), new Vector3(19.5f, 0f, 62f), 4.5f, 1.6f, wall);
            Crenels(group, new Vector3(-19.5f, 4.5f, 28f), new Vector3(-19.5f, 4.5f, 62f), wall);
            Crenels(group, new Vector3(19.5f, 4.5f, 28f), new Vector3(19.5f, 4.5f, 62f), wall);
            // The retaining wall of the terrace above, with the stair through its middle.
            Wall("Terrace Wall West", group, new Vector3(-19f, 0f, 63f), new Vector3(-4.2f, 0f, 63f), 2f, 2f, wallDark);
            Wall("Terrace Wall East", group, new Vector3(4.2f, 0f, 63f), new Vector3(19f, 0f, 63f), 2f, 2f, wallDark);
            Stair(group, new Vector3(0f, 0f, 56f), 8f, 6f, 2f);
            foreach (float x in new[] { -4.6f, 4.6f })
                Pillar("Stair Post", group, new Vector3(x, 0f, 55.4f), 1.2f, 3f, wall, trim);

            Pillar("Broken Pillar", group, new Vector3(-10f, 0f, 47f), 1.6f, 2.4f, wall, trim);
            Pillar("Pillar", group, new Vector3(11f, 0f, 38f), 1.6f, 3.4f, wall, trim);
            Pillar("Pillar", group, new Vector3(13f, 0f, 50f), 1.6f, 3.4f, wall, trim);
            Rubble(group, new Vector3(-10.5f, 0f, 49f), 7);
            StruckTree(group, new Vector3(-9f, 0f, 39f));
            Crates(group, new Vector3(15.5f, 0f, 32f));
            Brazier(group, new Vector3(-6f, 0f, 31f), true);
            Brazier(group, new Vector3(6f, 0f, 31f), true);
            Brazier(group, new Vector3(-7f, 0f, 54f), true);
            Brazier(group, new Vector3(7f, 0f, 54f), true);
            Banner(group, new Vector3(-18.6f, 3.6f, 40f), 90f);
            Banner(group, new Vector3(18.6f, 3.6f, 52f), -90f);

            var dress = BuildKit.Group("Dressing", group);
            Slab("Court Path", dress, -3.3f, 3.3f, 28f, 56f, 0.012f, path, 0.02f);
            Dressing.RaggedEdge(dress, -14f, -4f, 33f, 45f, 0.01f, earth, 35);
            Dressing.Puddles(dress, -18f, 18f, 28f, 61f, 0f, 16, 21);
            Dressing.GrassLine(dress, new Vector3(-18.6f, 0f, 28.5f), new Vector3(-18.6f, 0f, 61.5f), 0.6f, 22);
            Dressing.GrassLine(dress, new Vector3(18.6f, 0f, 61.5f), new Vector3(18.6f, 0f, 28.5f), 0.6f, 23);
            Dressing.GrassLine(dress, new Vector3(-4.6f, 0f, 61.9f), new Vector3(-18.6f, 0f, 61.9f), 0.5f, 24);
            Dressing.GrassLine(dress, new Vector3(18.6f, 0f, 61.9f), new Vector3(4.6f, 0f, 61.9f), 0.5f, 25);
            Dressing.GrassPatch(dress, new Vector3(-9f, 0.01f, 39f), 5.2f, 34, 26);
            Dressing.GrassPatch(dress, new Vector3(-10.4f, 0f, 48.6f), 2.2f, 9, 27);
            Dressing.GrassPatch(dress, new Vector3(11f, 0f, 38f), 1.6f, 7, 28);
            Dressing.GrassPatch(dress, new Vector3(13f, 0f, 50f), 1.6f, 7, 29);
            Dressing.Leaves(dress, new Vector3(-9f, 0.01f, 39f), 4.2f, 30);
            Dressing.Leaves(dress, new Vector3(-6.5f, 0f, 43f), 2.6f, 31);
            foreach (float z in new[] { 37f, 47f })
            {
                Dressing.Lantern(dress, new Vector3(-4f, 0f, z), 0f);
                Dressing.Lantern(dress, new Vector3(4f, 0f, z), 180f);
            }
            Dressing.Barrel(dress, new Vector3(13.6f, 0f, 31f), 15f);
            Dressing.Barrel(dress, new Vector3(14.1f, 0f, 33.6f), 70f, true);
            Dressing.Barrel(dress, new Vector3(-17.4f, 0f, 60f), 0f);
            Dressing.Sacks(dress, new Vector3(-14.4f, 0f, 59.6f), 20f);
            Dressing.Cart(dress, new Vector3(-15.4f, 0f, 30.6f), 35f);
            Dressing.Spears(dress, new Vector3(7.2f, 0f, 43f), 4, 32, cloth);
            Dressing.Spears(dress, new Vector3(-5.6f, 0f, 52f), 3, 33, null);
            Dressing.Spears(dress, new Vector3(15f, 0f, 57f), 5, 34, cloth);
            OuterCourtArt(dress);
        }

        // The middle of the area: the Katana on its stone, a place to rest, and the second stair.
        private static void ShrineTerrace(AreaDirector director)
        {
            var group = BuildKit.Group("Shrine Terrace", world);
            Slab("Terrace Floor", group, -19f, 19f, 62f, 94f, 2f, paving, 2.2f);
            Wall("Terrace West", group, new Vector3(-19.5f, 2f, 62f), new Vector3(-19.5f, 2f, 94f), 4.5f, 1.6f, wall);
            Wall("Terrace East", group, new Vector3(19.5f, 2f, 62f), new Vector3(19.5f, 2f, 94f), 4.5f, 1.6f, wall);
            Crenels(group, new Vector3(-19.5f, 6.5f, 62f), new Vector3(-19.5f, 6.5f, 94f), wall);
            Crenels(group, new Vector3(19.5f, 6.5f, 62f), new Vector3(19.5f, 6.5f, 94f), wall);
            Wall("Ward Wall West", group, new Vector3(-19f, 2f, 95f), new Vector3(-4.2f, 2f, 95f), 2f, 2f, wallDark);
            Wall("Ward Wall East", group, new Vector3(4.2f, 2f, 95f), new Vector3(19f, 2f, 95f), 2f, 2f, wallDark);
            Stair(group, new Vector3(0f, 2f, 88f), 8f, 6f, 2f);

            // The shrine: a round dais with a ring of standing stones, the sword in the middle of it.
            Disc("Shrine Dais", group, new Vector3(0f, 2f, 74f), 5.2f, 0.25f, trim);
            Disc("Shrine Step", group, new Vector3(0f, 2f, 74f), 6f, 0.12f, wallDark);
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (i * 60f + 30f);
                var at = new Vector3(Mathf.Sin(angle) * 5.6f, 2f, 74f + Mathf.Cos(angle) * 5.6f);
                Pillar("Standing Stone", group, at, 0.9f, 2.6f + (i % 2) * 0.9f, wallDark, trim);
                var mark = BuildKit.Shape(PrimitiveType.Cube, "Rune", group, at + new Vector3(-Mathf.Sin(angle) * 0.47f, 1.5f, -Mathf.Cos(angle) * 0.47f),
                    new Vector3(0.12f, 0.9f, 0.04f), rune, new Vector3(0f, i * 60f + 30f, 0f));
                mark.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            KatanaStone(group, new Vector3(0f, 2.25f, 74f));

            Rest(group, director, new Vector3(-13f, 2f, 66f), new Vector3(-11.4f, 2.05f, 67f));
            Brazier(group, new Vector3(13f, 2f, 66f), true);
            Brazier(group, new Vector3(-13f, 2f, 88f), true);
            Brazier(group, new Vector3(13f, 2f, 88f), true);
            Rubble(group, new Vector3(12f, 2f, 82f), 5);
            Banner(group, new Vector3(-18.6f, 5.6f, 74f), 90f);
            Banner(group, new Vector3(18.6f, 5.6f, 74f), -90f);

            var dress = BuildKit.Group("Dressing", group);
            Slab("Terrace Path South", dress, -3.3f, 3.3f, 62f, 68.2f, 2.012f, path, 0.02f);
            Slab("Terrace Path North", dress, -3.3f, 3.3f, 79.8f, 88f, 2.012f, path, 0.02f);
            Dressing.Puddles(dress, -18f, -7f, 63f, 93f, 2f, 6, 41);
            Dressing.Puddles(dress, 7f, 18f, 63f, 93f, 2f, 6, 42);
            Dressing.GrassLine(dress, new Vector3(-18.6f, 2f, 62.5f), new Vector3(-18.6f, 2f, 93.5f), 0.6f, 43);
            Dressing.GrassLine(dress, new Vector3(18.6f, 2f, 93.5f), new Vector3(18.6f, 2f, 62.5f), 0.6f, 44);
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (i * 60f + 30f);
                Dressing.GrassPatch(dress, new Vector3(Mathf.Sin(angle) * 6.4f, 2f, 74f + Mathf.Cos(angle) * 6.4f), 1.1f, 6, 45 + i);
            }
            Dressing.Leaves(dress, new Vector3(-12f, 2f, 71f), 3f, 52);
            Dressing.Spears(dress, new Vector3(9.4f, 2f, 78f), 5, 53, cloth);
            TerraceArt(dress);
        }

        // How far up the ward the captain's ground begins: a course of pale stone is laid across the floor there.
        private const float CaptainsLine = 113f;

        // The last and hardest fight, under the keep's wall, and the sealed door to the summit.
        private static void UpperWard(AreaDirector director)
        {
            var group = BuildKit.Group("Upper Ward", world);
            Slab("Ward Floor", group, -19f, 19f, 94f, 126f, 4f, paving, 4.2f);
            Wall("Ward West", group, new Vector3(-19.5f, 4f, 94f), new Vector3(-19.5f, 4f, 126f), 5f, 1.6f, wall);
            Wall("Ward East", group, new Vector3(19.5f, 4f, 94f), new Vector3(19.5f, 4f, 126f), 5f, 1.6f, wall);
            Crenels(group, new Vector3(-19.5f, 9f, 94f), new Vector3(-19.5f, 9f, 126f), wall);
            Crenels(group, new Vector3(19.5f, 9f, 94f), new Vector3(19.5f, 9f, 126f), wall);

            // The keep: the tall backdrop of the whole area, with the great door in it.
            Block("Keep Wall", group, new Vector3(0f, 12f, 129f), new Vector3(42f, 16f, 5f), wall);
            Crenels(group, new Vector3(-20.5f, 20f, 127.2f), new Vector3(20.5f, 20f, 127.2f), wall);
            Tower(group, new Vector3(-17f, 4f, 128f), 7f, 20f);
            Tower(group, new Vector3(17f, 4f, 128f), 7f, 20f);
            Block("Keep Roof", group, new Vector3(0f, 21.5f, 131f), new Vector3(30f, 3f, 6f), roof);
            Block("Door Frame", group, new Vector3(0f, 7.5f, 126.4f), new Vector3(7.5f, 7.5f, 0.8f), wallDark);
            Block("Great Door", group, new Vector3(0f, 7f, 126.1f), new Vector3(5.6f, 6f, 0.5f), wood);
            var seal = BuildKit.Shape(PrimitiveType.Cube, "Door Seal", group, new Vector3(0f, 7.2f, 125.8f), new Vector3(0.5f, 4.4f, 0.1f), rune);
            seal.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cube, "Door Seal Bar", group, new Vector3(0f, 7.6f, 125.8f), new Vector3(3.4f, 0.4f, 0.1f), rune);
            Disc("Door Step", group, new Vector3(0f, 4f, 124.6f), 4.4f, 0.18f, trim);
            var way = BuildKit.Group("Sealed Way", group).gameObject.AddComponent<SealedWay>();
            way.transform.position = new Vector3(0f, 4f, 124.2f);
            way.Prompt = "Open the way to the summit";
            way.Range = 3f;
            way.LeadsTo = "Summit";
            way.Configure(director);
            things.Add(way);

            foreach (float x in new[] { -9f, 9f })
            {
                Pillar("Ward Pillar", group, new Vector3(x, 4f, 101f), 1.6f, 3.6f, wall, trim);
                // The second pair stands on the captain's line, as its gateposts.
                Pillar("Ward Pillar", group, new Vector3(x, 4f, CaptainsLine), 1.6f, 3.6f, wall, trim);
            }
            Brazier(group, new Vector3(-5f, 4f, 123.4f), true);
            Brazier(group, new Vector3(5f, 4f, 123.4f), true);
            Brazier(group, new Vector3(-15f, 4f, 99f), true);
            Brazier(group, new Vector3(15f, 4f, 99f), true);
            Rubble(group, new Vector3(-14f, 4f, 112f), 6);
            Rubble(group, new Vector3(13f, 4f, 106f), 4);
            Banner(group, new Vector3(-6.5f, 11f, 126.3f), 0f);
            Banner(group, new Vector3(6.5f, 11f, 126.3f), 0f);

            var dress = BuildKit.Group("Dressing", group);
            Slab("Ward Path", dress, -3.3f, 3.3f, 96f, 121f, 4.012f, path, 0.02f);
            Slab("Captain Line", dress, -18.4f, 18.4f, CaptainsLine - 0.22f, CaptainsLine + 0.22f, 4.016f, trim, 0.02f);
            Dressing.Puddles(dress, -18f, 18f, 95f, 123f, 4f, 14, 61);
            Dressing.GrassLine(dress, new Vector3(-18.6f, 4f, 94.5f), new Vector3(-18.6f, 4f, 125.5f), 0.6f, 62);
            Dressing.GrassLine(dress, new Vector3(18.6f, 4f, 125.5f), new Vector3(18.6f, 4f, 94.5f), 0.6f, 63);
            Dressing.GrassLine(dress, new Vector3(-16f, 4f, 126.2f), new Vector3(-5f, 4f, 126.2f), -0.5f, 64);
            Dressing.GrassLine(dress, new Vector3(5f, 4f, 126.2f), new Vector3(16f, 4f, 126.2f), -0.5f, 65);
            foreach (float x in new[] { -9f, 9f })
            {
                Dressing.GrassPatch(dress, new Vector3(x, 4f, 101f), 1.6f, 6, 66 + (int)x);
                Dressing.GrassPatch(dress, new Vector3(x, 4f, CaptainsLine), 1.6f, 6, 86 + (int)x);
            }
            Dressing.Lantern(dress, new Vector3(-4f, 4f, 105f), 0f);
            Dressing.Lantern(dress, new Vector3(4f, 4f, 105f), 180f);
            Dressing.Spears(dress, new Vector3(-6.4f, 4f, 109f), 5, 71, cloth);
            Dressing.Spears(dress, new Vector3(-13f, 4f, 119f), 3, 73, cloth);
            WardArt(dress);
            Beyond();
        }

        // ------------------------------------------------------------------ what makes each place its own

        // A torch on a wall, burning.
        private static void Torch(Transform parent, Vector3 at, float yaw) => Flames(Dressing.WallTorch(parent, at, yaw), Vector3.zero, true, 0.42f);

        // The foot of a wall face: a darker course of stone, and the floor broken and overgrown where it meets it.
        private static void WallFoot(Transform parent, Vector3 from, Vector3 to, Vector3 outward, int seed, bool mossy = false, float width = 1.25f)
        {
            Dressing.Footing(parent, from, to, outward);
            Dressing.FloorEdge(parent, from + outward * 0.22f, to + outward * 0.22f, outward, width, seed, mossy);
        }

        // A length of parapet that has gone into the drop: the stumps of it, and nothing the player can walk through.
        private static void BrokenParapet(Transform parent, float x, float south, float north)
        {
            float middle = (south + north) * 0.5f, length = north - south;
            Block("Parapet Stump", parent, new Vector3(x, 0.15f, middle), new Vector3(0.6f, 0.3f, length), wallDark);
            Block("Parapet Break", parent, new Vector3(x, 0.4f, south + 0.28f), new Vector3(0.62f, 0.8f, 0.6f), wall, 5f);
            Block("Parapet Break", parent, new Vector3(x, 0.27f, south + 0.85f), new Vector3(0.6f, 0.54f, 0.55f), wall, -4f);
            Block("Parapet Break", parent, new Vector3(x, 0.36f, north - 0.3f), new Vector3(0.62f, 0.72f, 0.62f), wall, -6f);
            var guard = new GameObject("Parapet Guard");
            guard.transform.SetParent(parent, false);
            guard.transform.position = new Vector3(x, 1.5f, middle);
            guard.AddComponent<BoxCollider>().size = new Vector3(0.6f, 3f, length);
        }

        // Something big and far down that is only ever looked at.
        private static void Mass(string name, Transform parent, Vector3 centre, Vector3 size, Material material)
        {
            var mass = BuildKit.Shape(PrimitiveType.Cube, name, parent, centre, size, material);
            mass.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mass.gameObject.isStatic = true;
        }

        // The approach: two guardians flanked the way and one has fallen through the parapet; below, the piers of the
        // bridge go down into the dark among the crags.
        private static void CausewayArt(Transform group, Transform dress)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Mass("Statue Pier", group, new Vector3(side * 6.1f, -8.1f, 8f), new Vector3(1.8f, 16f, 2f), wall);
                Mass("Statue Pier Cap", group, new Vector3(side * 6.1f, -0.1f, 8f), new Vector3(2.1f, 0.2f, 2.3f), trim);
            }
            Dressing.Statue(dress, new Vector3(-5.7f, 0f, 8f), 90f, false);
            Dressing.Statue(dress, new Vector3(5.7f, 0f, 8f), -90f, true);
            Dressing.FallenStones(dress, new Vector3(-4.3f, 0f, 14.3f), 4, 0.45f, 7);
            Dressing.Chain(dress, new Vector3(-5.4f, 1.05f, 12.3f), new Vector3(-5.4f, 1.05f, 16.2f), 0.55f, 9);
            Dressing.Chain(dress, new Vector3(5.4f, 1.05f, 6.4f), new Vector3(4.9f, 0.06f, 7.1f), 0.05f, 4);

            var below = BuildKit.Group("Below", group);
            Mass("Bridge Pier", below, new Vector3(0f, -16f, 8f), new Vector3(16.6f, 27.6f, 4.4f), wallDark);
            Mass("Bridge Pier Ledge", below, new Vector3(0f, -2.3f, 8f), new Vector3(17.2f, 0.3f, 5f), trim);
            Mass("Bridge Pier", below, new Vector3(0f, -17f, -2.5f), new Vector3(14f, 27f, 4f), wallDark);
            Mass("Bridge Pier Ledge", below, new Vector3(0f, -3.6f, -2.5f), new Vector3(14.6f, 0.3f, 4.6f), trim);
            foreach (float side in new[] { -1f, 1f })
            {
                Mass("Gate Tower Foot", below, new Vector3(side * 9.5f, -22f, 16f), new Vector3(7.6f, 32f, 7.6f), wallDark);
                // The court stands on a wall of its own, going down beside the last of the causeway.
                Mass("Court Foundations", below, new Vector3(side * 12.6f, -16.5f, 27.6f), new Vector3(15f, 32f, 3.6f), wallDark);
                Mass("Court Foundation Ledge", below, new Vector3(side * 12.6f, -3.2f, 27.3f), new Vector3(15.4f, 0.4f, 4.6f), trim);
            }
            Dressing.Crag(below, new Vector3(13.5f, -40f, 1f), 8f, 30f, 1);
            Dressing.Crag(below, new Vector3(-14f, -40f, 3f), 9f, 32f, 2);
            Dressing.Crag(below, new Vector3(16.5f, -40f, 23f), 7f, 28f, 3);
            Dressing.Crag(below, new Vector3(-16f, -40f, 21.5f), 8f, 33f, 4);
            Dressing.Crag(below, new Vector3(-12f, -40f, 13.5f), 6f, 26f, 5);
            Dressing.Crag(below, new Vector3(20f, -40f, 9f), 10f, 35f, 6);
            Dressing.Crag(below, new Vector3(-21f, -40f, 10f), 10f, 34f, 7);
            Dressing.Mist(below, new Vector3(0f, -22f, 10f), 110f, 80f);
        }

        // A garrison's yard: racks of spears on the walls, striking posts in the earth, a torn lean-to, and the well.
        private static void OuterCourtArt(Transform dress)
        {
            const float W = -18.7f, E = 18.7f;
            WallFoot(dress, new Vector3(W, 0f, 28.8f), new Vector3(W, 0f, 62f), Vector3.right, 101);
            WallFoot(dress, new Vector3(E, 0f, 28.8f), new Vector3(E, 0f, 62f), Vector3.left, 102);
            WallFoot(dress, new Vector3(-18.7f, 0f, 62f), new Vector3(-4.9f, 0f, 62f), Vector3.back, 103);
            WallFoot(dress, new Vector3(4.9f, 0f, 62f), new Vector3(18.7f, 0f, 62f), Vector3.back, 104);
            Dressing.FloorEdge(dress, new Vector3(-18.7f, 0f, 28.8f), new Vector3(-5.6f, 0f, 28.8f), Vector3.forward, 1f, 105);
            Dressing.FloorEdge(dress, new Vector3(5.6f, 0f, 28.8f), new Vector3(18.7f, 0f, 28.8f), Vector3.forward, 1f, 106);
            foreach (float z in new[] { 33.5f, 45f, 56.5f }) Dressing.Buttress(dress, new Vector3(W, 0f, z), 90f, 3.7f);
            foreach (float z in new[] { 33.5f, 49.2f, 58.6f }) Dressing.Buttress(dress, new Vector3(E, 0f, z), -90f, 3.7f);
            foreach (float x in new[] { -12.3f, 12.3f }) Dressing.Buttress(dress, new Vector3(x, 0f, 62f), 180f, 1.95f, 1.2f, 0.7f);

            Torch(dress, new Vector3(W, 2.5f, 36.8f), 90f);
            Torch(dress, new Vector3(E, 2.5f, 38.4f), -90f);
            Dressing.ArrowSlit(dress, new Vector3(W, 3f, 30.6f), 90f, 2);
            Dressing.ArrowSlit(dress, new Vector3(W, 3f, 42.6f), 90f, 1);
            Dressing.ArrowSlit(dress, new Vector3(W, 3f, 52.8f), 90f, 0);
            Dressing.ArrowSlit(dress, new Vector3(E, 3f, 36.2f), -90f, 1);
            Dressing.ArrowSlit(dress, new Vector3(E, 3f, 57.2f), -90f, 2);
            Dressing.IvyPatch(dress, new Vector3(W, 0f, 30.4f), 90f, 3.4f, 2.6f, 131);
            Dressing.IvyPatch(dress, new Vector3(W, 0f, 54.3f), 90f, 2.6f, 2f, 132);
            Dressing.IvyPatch(dress, new Vector3(W, 0f, 60f), 90f, 3f, 3f, 133);
            Dressing.IvyPatch(dress, new Vector3(E, 0f, 35.4f), -90f, 2.6f, 2.2f, 134);
            Dressing.IvyPatch(dress, new Vector3(E, 0f, 50.8f), -90f, 2f, 2.4f, 135);
            Dressing.IvyPatch(dress, new Vector3(E, 0f, 60.6f), -90f, 2.4f, 3f, 136);
            Dressing.IvyPatch(dress, new Vector3(-15.5f, 0f, 62f), 180f, 4f, 1.8f, 137);
            Dressing.IvyPatch(dress, new Vector3(-6.4f, 0f, 62f), 180f, 2.4f, 1.7f, 138);
            Dressing.IvyPatch(dress, new Vector3(8f, 0f, 62f), 180f, 3.4f, 1.8f, 139);
            Dressing.IvyPatch(dress, new Vector3(16.6f, 0f, 62f), 180f, 3f, 1.7f, 140);
            Dressing.MissingBlock(dress, new Vector3(W, 3.2f, 58.5f), 90f, 3.2f, 141);
            Dressing.MissingBlock(dress, new Vector3(E, 3.2f, 31.2f), -90f, 3.2f, 142);

            Dressing.WeaponRack(dress, new Vector3(W + 0.22f, 0f, 49f), 90f, 151);
            Dressing.WeaponRack(dress, new Vector3(E - 0.22f, 0f, 55.2f), -90f, 152);
            Dressing.WeaponRack(dress, new Vector3(-9f, 0f, 61.78f), 180f, 153);
            Dressing.ShieldStack(dress, new Vector3(-16.2f, 0f, 58.2f), 30f, 154);
            Dressing.ShieldStack(dress, new Vector3(8.8f, 0f, 60.3f), 200f, 155);
            Dressing.StrikingPost(dress, new Vector3(-12.2f, 0f, 35.6f), 150f, true);
            Dressing.StrikingPost(dress, new Vector3(-12.8f, 0f, 41.8f), 200f, false);
            Dressing.StrikingPost(dress, new Vector3(-6.8f, 0f, 43.4f), 170f, true);
            Dressing.Awning(dress, new Vector3(E - 0.05f, 0f, 43.9f), -90f, 7f);

            // The well is what the yard is built round: east of the path, with its own lamp.
            Dressing.Well(dress, new Vector3(10f, 0f, 44.6f), 20f);
            Dressing.Trough(dress, new Vector3(12.9f, 0f, 47.3f), 70f);
            Dressing.MossPatch(dress, new Vector3(9.2f, 0.012f, 46.5f), 1.6f, 156);
            Dressing.MossPatch(dress, new Vector3(11.7f, 0.012f, 43.1f), 1.3f, 157);
            Dressing.Puddle(dress, new Vector3(8f, 0f, 43f), 1.5f, 777);
        }

        // Older than the castle, and holy: stone lanterns along the way to the sword, ropes and paper on the standing
        // stones, offerings, two old trees, moss over everything.
        private static void TerraceArt(Transform dress)
        {
            const float W = -18.7f, E = 18.7f, Y = 2f;
            WallFoot(dress, new Vector3(W, Y, 62.2f), new Vector3(W, Y, 94f), Vector3.right, 111, true, 1.5f);
            WallFoot(dress, new Vector3(E, Y, 62.2f), new Vector3(E, Y, 94f), Vector3.left, 112, true, 1.5f);
            WallFoot(dress, new Vector3(-18.7f, Y, 94f), new Vector3(-4.9f, Y, 94f), Vector3.back, 113, true);
            WallFoot(dress, new Vector3(4.9f, Y, 94f), new Vector3(18.7f, Y, 94f), Vector3.back, 114, true);
            foreach (float z in new[] { 68.5f, 86.5f })
            {
                Dressing.Buttress(dress, new Vector3(W, Y, z), 90f, 3.7f);
                Dressing.Buttress(dress, new Vector3(E, Y, z), -90f, 3.7f);
            }
            foreach (float x in new[] { -12.3f, 12.3f }) Dressing.Buttress(dress, new Vector3(x, Y, 94f), 180f, 1.95f, 1.2f, 0.7f);
            foreach (float z in new[] { 79.5f, 91f })
            {
                Dressing.Niche(dress, new Vector3(W, Y + 1.3f, z), 90f);
                Dressing.Niche(dress, new Vector3(E, Y + 1.3f, z), -90f);
            }
            Dressing.IvyPatch(dress, new Vector3(W, Y, 64.8f), 90f, 4.5f, 3.2f, 161);
            Dressing.IvyPatch(dress, new Vector3(W, Y, 71.6f), 90f, 2.4f, 2f, 162);
            Dressing.IvyPatch(dress, new Vector3(W, Y, 82.8f), 90f, 4f, 3.4f, 163);
            Dressing.IvyPatch(dress, new Vector3(E, Y, 66f), -90f, 3.6f, 3f, 164);
            Dressing.IvyPatch(dress, new Vector3(E, Y, 83f), -90f, 4.6f, 3.6f, 165);
            Dressing.IvyPatch(dress, new Vector3(E, Y, 89f), -90f, 2.4f, 2.2f, 166);
            Dressing.IvyPatch(dress, new Vector3(-15.6f, Y, 94f), 180f, 5f, 1.8f, 167);
            Dressing.IvyPatch(dress, new Vector3(-7.2f, Y, 94f), 180f, 3f, 1.7f, 168);
            Dressing.IvyPatch(dress, new Vector3(8.4f, Y, 94f), 180f, 4f, 1.8f, 169);
            Dressing.IvyPatch(dress, new Vector3(16.4f, Y, 94f), 180f, 3f, 1.7f, 170);
            Dressing.MissingBlock(dress, new Vector3(W, Y + 3.1f, 76.4f), 90f, 3.1f, 171);
            Dressing.MissingBlock(dress, new Vector3(E, Y + 3.2f, 71.2f), -90f, 3.2f, 172);

            // Four lanterns burn along the way; a fifth fell long ago and nobody stood it up.
            foreach (float z in new[] { 64.6f, 82.4f })
            {
                Dressing.Toro(dress, new Vector3(-4.3f, Y, z), 0f, true);
                Dressing.Toro(dress, new Vector3(4.3f, Y, z), 0f, true);
            }
            Dressing.Toro(dress, new Vector3(-12.6f, Y, 84.4f), 40f, false, true);

            // The stones are roped east and west; the way in from the south and out to the north is left open.
            Vector3 StoneTop(int index)
            {
                float angle = Mathf.Deg2Rad * (index * 60f + 30f);
                return new Vector3(Mathf.Sin(angle) * 5.6f, Y + 2.45f, 74f + Mathf.Cos(angle) * 5.6f);
            }
            foreach (int first in new[] { 0, 1, 3, 4 })
            {
                Vector3 from = StoneTop(first), to = StoneTop(first + 1);
                Vector3 along = (to - from).normalized;
                Dressing.Shimenawa(dress, from + along * 0.45f, to - along * 0.45f, 0.38f);
            }
            Dressing.OfferingTable(dress, new Vector3(-2.4f, Y + 0.25f, 75.6f), 25f);
            Dressing.OfferingTable(dress, new Vector3(2.4f, Y + 0.25f, 75.6f), -25f);
            Dressing.GnarledTree(dress, new Vector3(-10f, Y, 72.4f), 0f);
            Dressing.GnarledTree(dress, new Vector3(10f, Y, 70.8f), 140f);

            float green = Y + 0.015f;
            Dressing.MossPatch(dress, new Vector3(-6.8f, green, 70.4f), 2f, 181);
            Dressing.MossPatch(dress, new Vector3(6.9f, green, 77.9f), 1.8f, 182);
            Dressing.MossPatch(dress, new Vector3(-5.4f, green, 79.6f), 1.6f, 183);
            Dressing.MossPatch(dress, new Vector3(5.2f, green, 68.2f), 1.4f, 184);
            Dressing.MossPatch(dress, new Vector3(-11.4f, green, 74.2f), 1.8f, 185);
            Dressing.MossPatch(dress, new Vector3(11.6f, green, 72.6f), 1.7f, 186);
            Dressing.MossPatch(dress, new Vector3(-2.3f, green, 66.2f), 1.1f, 187);
            Dressing.MossPatch(dress, new Vector3(2.6f, green, 84.6f), 1.2f, 188);
            Dressing.MossPatch(dress, new Vector3(3.3f, green + 0.25f, 71.5f), 1.2f, 189);
            Dressing.MossPatch(dress, new Vector3(-3.5f, green + 0.25f, 77f), 1.3f, 190);
        }

        // An execution ground: iron rods along the keep wall to call the lightning down, stakes to chain the condemned
        // to, the paving burned black where it struck, and the engines of whoever last tried the door.
        private static void WardArt(Transform dress)
        {
            const float W = -18.7f, E = 18.7f, Y = 4f;
            WallFoot(dress, new Vector3(W, Y, 94.2f), new Vector3(W, Y, 124.5f), Vector3.right, 121);
            WallFoot(dress, new Vector3(E, Y, 94.2f), new Vector3(E, Y, 124.5f), Vector3.left, 122);
            foreach (float side in new[] { -1f, 1f })
            {
                Dressing.Footing(dress, new Vector3(side * 13.5f, Y, 126.5f), new Vector3(side * 3.75f, Y, 126.5f), Vector3.back, 0.8f, 0.22f);
                Dressing.FloorEdge(dress, new Vector3(side * 13.5f, Y, 126.28f), new Vector3(side * 4.7f, Y, 126.28f), Vector3.back, 1.1f, 123 + (int)side);
                float face = side < 0f ? W : E, yaw = side < 0f ? 90f : -90f;
                foreach (float z in new[] { 98.5f, 109.5f, 120.5f }) Dressing.Buttress(dress, new Vector3(face, Y, z), yaw, 4.3f, 1.4f);
                Dressing.ArrowSlit(dress, new Vector3(face, Y + 3.1f, 104f), yaw, 2);
                Dressing.ArrowSlit(dress, new Vector3(face, Y + 3.1f, 115f), yaw, 2);

                // The rods: three each side of the door, and the banners of the house hung low between them and it.
                float[] heights = { 10f, 12.5f, 9f };
                for (int i = 0; i < 3; i++) Dressing.LightningRod(dress, new Vector3(side * (8.3f + i * 2.3f), Y, 125.5f), heights[i]);
                Dressing.Arc(dress, new Vector3(side * 8.3f, Y + 1.5f, 125.5f), new Vector3(side * 10.6f, Y + 1f, 125.5f), 31 + (int)side);
                Dressing.Arc(dress, new Vector3(side * 10.6f, Y + 1.5f, 125.5f), new Vector3(side * 12.9f, Y + 1.5f, 125.5f), 35 + (int)side);
                Mass("Rod Cable", dress, new Vector3(side * 10.6f, Y + 0.05f, 124.85f), new Vector3(5.6f, 0.08f, 0.1f), iron);
                Dressing.HeavyBanner(dress, new Vector3(side * 6.4f, Y + 3.4f, 126.3f), 180f, 1.7f, 2.6f);

                var glowObject = new GameObject("Rod Light");
                glowObject.transform.SetParent(dress, false);
                glowObject.transform.position = new Vector3(side * 10.6f, Y + 1.5f, 124.4f);
                var glow = glowObject.AddComponent<Light>();
                glow.type = LightType.Point;
                glow.color = new Color(0.45f, 0.65f, 1f);
                glow.range = 7f;
                glow.intensity = 7f;
                glow.shadows = LightShadows.None;
            }
            Dressing.Chain(dress, new Vector3(W + 0.08f, Y + 3f, 100.2f), new Vector3(W + 0.08f, Y + 3f, 107.8f), 1.1f, 11);
            Dressing.Chain(dress, new Vector3(E - 0.08f, Y + 3f, 111.2f), new Vector3(E - 0.08f, Y + 3f, 118.8f), 1.1f, 11);
            Dressing.MissingBlock(dress, new Vector3(W, Y + 3f, 117.4f), 90f, 3f, 191);
            Dressing.MissingBlock(dress, new Vector3(E, Y + 3f, 102f), -90f, 3f, 192);

            Dressing.Ballista(dress, new Vector3(13.2f, Y, 110.6f), -14f);
            Dressing.Bolts(dress, new Vector3(10.8f, Y, 113.4f), 5, 1.6f, 5);
            Dressing.Bolts(dress, new Vector3(15.4f, Y, 117.6f), 4, 1.7f, 6);
            Dressing.Mantlet(dress, new Vector3(-13.4f, Y, 106.4f), 165f);
            Dressing.Cage(dress, new Vector3(-15.2f, Y, 121.6f), 25f);
            float[] stakes = { 13f, 15.2f, 17.1f };
            for (int i = 0; i < stakes.Length; i++)
            {
                var at = new Vector3(stakes[i], Y, 122.3f + (i % 2) * 0.25f);
                Dressing.Stake(dress, at, 180f, i);
                Dressing.ScorchMark(dress, at + Vector3.up * 0.014f, 1.05f, 40 + i);
            }
            float burn = Y + 0.014f;
            Dressing.ScorchMark(dress, new Vector3(0f, burn, 109.6f), 2.4f, 51);
            Dressing.ScorchMark(dress, new Vector3(-2.2f, burn, 100.2f), 1.5f, 52);
            Dressing.ScorchMark(dress, new Vector3(6.4f, burn, 103.4f), 1.3f, 53);
            Dressing.ScorchMark(dress, new Vector3(-7.4f, burn, 112.8f), 1.7f, 54);
            Dressing.ScorchMark(dress, new Vector3(2.8f, burn, 117f), 1.4f, 55);
            Dressing.ScorchMark(dress, new Vector3(12.2f, burn, 116.4f), 1.6f, 56);
            Dressing.ScorchMark(dress, new Vector3(-10f, burn, 122.2f), 1.2f, 57);
        }

        // What stands outside the walls, so the world does not stop at them: a line of mountains north of the keep and
        // beyond each long wall, and a few far towers. All of it is one flat tone and none of it is ever walked on.
        private static void Beyond()
        {
            var beyond = BuildKit.Group("Beyond", world);
            Dressing.Ridge(beyond, new Vector3(0f, -20f, 188f), 0f, 280f, 80f);
            Dressing.Ridge(beyond, new Vector3(-72f, -20f, 70f), 90f, 230f, 58f);
            Dressing.Ridge(beyond, new Vector3(72f, -20f, 62f), -90f, 230f, 64f);
            Dressing.FarTower(beyond, new Vector3(-31f, -20f, 46f), 6f, 34f);
            Dressing.FarTower(beyond, new Vector3(-37f, -20f, 84f), 8f, 44f);
            Dressing.FarTower(beyond, new Vector3(33f, -20f, 68f), 7f, 40f);
            Dressing.FarTower(beyond, new Vector3(39f, -20f, 106f), 6f, 46f);
            Dressing.FarTower(beyond, new Vector3(28f, -30f, 20f), 5f, 32f);
            Dressing.FarTower(beyond, new Vector3(-27f, -30f, 12f), 6f, 30f);
            Dressing.Mist(beyond, new Vector3(-35f, 58f, 186f), 70f, 70f, true);
        }

        // ------------------------------------------------------------------ pieces

        // A floor between two x and two z, its top at the given height.
        private static void Slab(string name, Transform parent, float west, float east, float south, float north, float top, Material material, float thickness = 1f)
        {
            var slab = BuildKit.Block(name, parent, new Vector3((west + east) * 0.5f, top - thickness * 0.5f, (south + north) * 0.5f),
                new Vector3(east - west, thickness, north - south), material);
            // The ground is never thinned out, however thick the slab that makes it.
            var fade = slab.GetComponent<Fadeable>();
            if (fade != null) Object.DestroyImmediate(fade);
        }

        private static GameObject Block(string name, Transform parent, Vector3 centre, Vector3 size, Material material, float yaw = 0f) =>
            BuildKit.Block(name, parent, centre, size, material, yaw);

        // A straight wall between two points on the ground, with a lighter cap along its top.
        private static void Wall(string name, Transform parent, Vector3 from, Vector3 to, float height, float thickness, Material material)
        {
            Vector3 along = to - from;
            float length = along.magnitude;
            float yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg;
            Vector3 middle = (from + to) * 0.5f;
            Block(name, parent, middle + Vector3.up * (height * 0.5f), new Vector3(thickness, height, length), material, yaw);
            Block(name + " Cap", parent, middle + Vector3.up * (height + 0.1f), new Vector3(thickness + 0.3f, 0.2f, length + 0.1f), trim, yaw);
        }

        // The teeth along the top of a wall.
        private static void Crenels(Transform parent, Vector3 from, Vector3 to, Material material)
        {
            Vector3 along = to - from;
            float length = along.magnitude;
            float yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg;
            int count = Mathf.Max(1, Mathf.FloorToInt(length / 2.4f));
            for (int i = 0; i < count; i++)
            {
                Vector3 at = Vector3.Lerp(from, to, (i + 0.5f) / count);
                Block("Crenel", parent, at + Vector3.up * 0.55f, new Vector3(1.7f, 0.9f, 1.2f), material, yaw);
            }
        }

        private static void Pillar(string name, Transform parent, Vector3 foot, float width, float height, Material shaft, Material cap)
        {
            Block(name, parent, foot + Vector3.up * (height * 0.5f), new Vector3(width, height, width), shaft);
            Block(name + " Base", parent, foot + Vector3.up * 0.15f, new Vector3(width + 0.4f, 0.3f, width + 0.4f), cap);
            Block(name + " Cap", parent, foot + Vector3.up * (height + 0.12f), new Vector3(width + 0.35f, 0.24f, width + 0.35f), cap);
        }

        // A square tower with a pitched cap; its tip is somewhere the storm strikes.
        private static void Tower(Transform parent, Vector3 foot, float width, float height, float roofStep = 1.3f, bool spire = true)
        {
            Block("Tower", parent, foot + Vector3.up * (height * 0.5f), new Vector3(width, height, width), wall);
            Block("Tower Band", parent, foot + Vector3.up * (height * 0.62f), new Vector3(width + 0.3f, 0.4f, width + 0.3f), trim);
            Block("Tower Top", parent, foot + Vector3.up * (height + 0.5f), new Vector3(width + 1f, 1f, width + 1f), wallDark);
            for (int i = 0; i < 3; i++)
            {
                float shrink = i * (width * 0.3f);
                Block("Tower Roof", parent, foot + Vector3.up * (height + 1f + (i + 0.5f) * roofStep), new Vector3(width + 0.6f - shrink, roofStep, width + 0.6f - shrink), roof);
            }
            float top = height + 1f + 3f * roofStep;
            if (spire)
            {
                Block("Tower Spire", parent, foot + Vector3.up * (top + 1.5f), new Vector3(0.25f, 3f, 0.25f), iron);
                top += 3f;
            }
            var rod = BuildKit.Group("Rod", parent);
            rod.position = foot + Vector3.up * top;
            rods.Add(rod);
        }

        // A flight of steps rising to the north, made as a ramp to walk on with step blocks to look at.
        private static void Stair(Transform parent, Vector3 southFoot, float width, float run, float rise)
        {
            float length = Mathf.Sqrt(run * run + rise * rise);
            float pitch = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Stair Ramp";
            ramp.transform.SetParent(parent, false);
            ramp.transform.position = southFoot + new Vector3(0f, rise * 0.5f - 0.26f, run * 0.5f);
            ramp.transform.rotation = Quaternion.Euler(-pitch, 0f, 0f);
            ramp.transform.localScale = new Vector3(width, 0.5f, length);
            ramp.GetComponent<MeshRenderer>().sharedMaterial = wallDark;
            ramp.isStatic = true;
            const int Steps = 8;
            for (int i = 0; i < Steps; i++)
            {
                float t = (i + 0.5f) / Steps;
                var step = BuildKit.Shape(PrimitiveType.Cube, "Step", parent,
                    southFoot + new Vector3(0f, rise * (i + 1f) / Steps - rise / Steps * 0.5f - 0.02f, run * t),
                    new Vector3(width, rise / Steps, run / Steps + 0.02f), trim);
                step.gameObject.isStatic = true;
            }
            foreach (float side in new[] { -1f, 1f })
                Block("Stair Cheek", parent, southFoot + new Vector3(side * (width * 0.5f + 0.3f), rise * 0.5f + 0.3f, run * 0.5f), new Vector3(0.6f, rise + 1.2f, run + 0.4f), wall);
        }

        private static void Disc(string name, Transform parent, Vector3 foot, float radius, float height, Material material)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            disc.transform.position = foot + Vector3.up * (height * 0.5f);
            disc.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            var collider = disc.AddComponent<MeshCollider>();
            collider.convex = true;
            disc.GetComponent<MeshRenderer>().sharedMaterial = material;
            disc.isStatic = true;
        }

        // An iron bowl on a stone post. Lit, it burns and throws a warm pool of light.
        private static (Light light, ParticleSystem flames) Brazier(Transform parent, Vector3 foot, bool lit)
        {
            var group = BuildKit.Group("Brazier", parent);
            group.position = foot;
            BuildKit.Shape(PrimitiveType.Cube, "Post", group, new Vector3(0f, 0.55f, 0f), new Vector3(0.45f, 1.1f, 0.45f), wallDark);
            BuildKit.Shape(PrimitiveType.Cylinder, "Bowl", group, new Vector3(0f, 1.25f, 0f), new Vector3(1f, 0.16f, 1f), iron);
            BuildKit.Shape(PrimitiveType.Cylinder, "Coals", group, new Vector3(0f, 1.4f, 0f), new Vector3(0.8f, 0.04f, 0.8f), lit ? ember : iron);
            var post = group.gameObject.AddComponent<BoxCollider>();
            post.center = new Vector3(0f, 0.7f, 0f);
            post.size = new Vector3(0.8f, 1.4f, 0.8f);

            var lightObject = new GameObject("Fire Light");
            lightObject.transform.SetParent(group, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.56f, 0.24f);
            light.range = 11f;
            light.intensity = 14f;
            light.shadows = LightShadows.None;
            light.enabled = lit;

            var flames = Flames(group, new Vector3(0f, 1.45f, 0f), lit);
            return (light, flames);
        }

        private static ParticleSystem Flames(Transform parent, Vector3 localPosition, bool playing, float scale = 1f)
        {
            var flamesObject = new GameObject("Flames");
            flamesObject.transform.SetParent(parent, false);
            flamesObject.transform.localPosition = localPosition;
            flamesObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var system = flamesObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = true;
            main.playOnAwake = playing;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f * scale, 1.5f * scale);
            main.startSize = new ParticleSystem.MinMaxCurve(0.6f * scale, 1.15f * scale);
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 1f, 1f, 1f), Color.white);
            main.maxParticles = 40;
            var emission = system.emission;
            emission.rateOverTime = 11f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.28f * scale;
            var colour = system.colorOverLifetime;
            colour.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.6f, 0.45f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            colour.color = gradient;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.25f));
            var renderer = flamesObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = fireMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return system;
        }

        // A cold brazier with a worn mat before it: a place to rest. Lighting it is resting.
        private static void Rest(Transform parent, AreaDirector director, Vector3 foot, Vector3 returnTo)
        {
            var (light, flames) = Brazier(parent, foot, false);
            var brazier = light.transform.parent;
            brazier.name = "Rest Brazier";
            // A worn mat to kneel on: dull red, bound at both ends, with a paler band woven across it.
            var matCloth = BuildKit.Toon(Folder, "m_castle_mat", new Color(0.42f, 0.11f, 0.15f), false, null, 0f, 0.012f);
            var matBand = BuildKit.Toon(Folder, "m_castle_mat_band", new Color(0.62f, 0.5f, 0.34f), false, null, 0f, 0f);
            BuildKit.Shape(PrimitiveType.Cube, "Mat", brazier, new Vector3(0f, 0.025f, -1.3f), new Vector3(1.5f, 0.04f, 1.1f), matCloth);
            foreach (float z in new[] { -0.36f, 0.36f })
                BuildKit.Shape(PrimitiveType.Cube, "Mat Band", brazier, new Vector3(0f, 0.03f, -1.3f + z), new Vector3(1.5f, 0.042f, 0.07f), matBand);
            foreach (float x in new[] { -0.78f, 0.78f })
            {
                BuildKit.Shape(PrimitiveType.Cube, "Mat Binding", brazier, new Vector3(x, 0.03f, -1.3f), new Vector3(0.08f, 0.05f, 1.14f), wood);
                for (int tassel = 0; tassel < 5; tassel++)
                    BuildKit.Shape(PrimitiveType.Cube, "Mat Tassel", brazier, new Vector3(x + Mathf.Sign(x) * 0.1f, 0.015f, -1.74f + tassel * 0.22f), new Vector3(0.14f, 0.02f, 0.03f), matBand);
            }
            var ring = BuildKit.Shape(PrimitiveType.Cylinder, "Rune Ring", brazier, new Vector3(0f, 0.02f, 0f), new Vector3(2.6f, 0.01f, 2.6f), rune);
            ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cylinder, "Rune Ring Inner", brazier, new Vector3(0f, 0.025f, 0f), new Vector3(2.4f, 0.01f, 2.4f), paving);
            var returnPoint = BuildKit.Group("Return Point", brazier);
            returnPoint.position = returnTo;
            var rest = brazier.gameObject.AddComponent<RestPoint>();
            rest.Prompt = "Light the fire and rest";
            rest.Configure(director, returnPoint, light, flames);
            things.Add(rest);
        }

        // The Katana, driven into a block of stone, humming with a cold light.
        private static void KatanaStone(Transform parent, Vector3 foot)
        {
            var group = BuildKit.Group("Katana Stone", parent);
            group.position = foot;
            BuildKit.Shape(PrimitiveType.Cube, "Stone", group, new Vector3(0f, 0.3f, 0f), new Vector3(1.1f, 0.6f, 0.9f), wallDark, new Vector3(0f, 18f, 0f));
            var stone = group.gameObject.AddComponent<BoxCollider>();
            stone.center = new Vector3(0f, 0.3f, 0f);
            stone.size = new Vector3(1.1f, 0.6f, 0.9f);

            var sword = BuildKit.Group("Katana", group);
            sword.localPosition = new Vector3(0f, 0.55f, 0f);
            sword.localRotation = Quaternion.Euler(0f, 30f, 8f);
            BuildKit.Shape(PrimitiveType.Cube, "Blade", sword, new Vector3(0f, 0.45f, 0f), new Vector3(0.05f, 0.95f, 0.012f), steel);
            BuildKit.Shape(PrimitiveType.Cube, "Guard", sword, new Vector3(0f, 0.94f, 0f), new Vector3(0.14f, 0.025f, 0.1f), iron);
            BuildKit.Shape(PrimitiveType.Cube, "Grip", sword, new Vector3(0f, 1.1f, 0f), new Vector3(0.045f, 0.3f, 0.035f), cloth);

            var glowObject = new GameObject("Sword Light");
            glowObject.transform.SetParent(sword, false);
            glowObject.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            var glow = glowObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.55f, 0.75f, 1f);
            glow.range = 7f;
            glow.intensity = 4f;
            glow.shadows = LightShadows.None;

            var pickup = group.gameObject.AddComponent<WeaponPickup>();
            pickup.Prompt = "Draw the Katana from the stone";
            pickup.Range = 2.8f;
            pickup.Configure(Weapons.KatanaName, sword.gameObject);
            things.Add(pickup);
        }

        // A dead tree split by a bolt, still glowing at the wound.
        private static void StruckTree(Transform parent, Vector3 foot)
        {
            var group = BuildKit.Group("Struck Tree", parent);
            group.position = foot;
            BuildKit.Shape(PrimitiveType.Cylinder, "Trunk", group, new Vector3(0f, 1.6f, 0f), new Vector3(0.7f, 1.6f, 0.7f), wood, new Vector3(0f, 0f, 4f));
            BuildKit.Shape(PrimitiveType.Cylinder, "Limb", group, new Vector3(0.9f, 3.3f, 0.2f), new Vector3(0.28f, 1.2f, 0.28f), wood, new Vector3(10f, 0f, -52f));
            BuildKit.Shape(PrimitiveType.Cylinder, "Limb", group, new Vector3(-0.7f, 3.5f, -0.3f), new Vector3(0.22f, 1f, 0.22f), wood, new Vector3(-14f, 0f, 44f));
            BuildKit.Shape(PrimitiveType.Cube, "Wound", group, new Vector3(0.05f, 2.2f, -0.33f), new Vector3(0.1f, 1.7f, 0.06f), ember, new Vector3(0f, 0f, 4f));
            var trunk = group.gameObject.AddComponent<CapsuleCollider>();
            trunk.center = new Vector3(0f, 1.5f, 0f);
            trunk.radius = 0.4f;
            trunk.height = 3f;
            var rod = BuildKit.Group("Rod", group);
            rod.localPosition = new Vector3(0f, 4.4f, 0f);
            rods.Add(rod);
        }

        private static void Crates(Transform parent, Vector3 foot)
        {
            Block("Crate", parent, foot + new Vector3(0f, 0.6f, 0f), new Vector3(1.2f, 1.2f, 1.2f), wood, 12f);
            Block("Crate", parent, foot + new Vector3(1.3f, 0.45f, 0.4f), new Vector3(0.9f, 0.9f, 0.9f), wood, -20f);
            Block("Crate", parent, foot + new Vector3(0.2f, 1.6f, 0.1f), new Vector3(0.8f, 0.8f, 0.8f), wood, 34f);
        }

        // Fallen stone: a scatter of blocks of different sizes, placed the same way every build.
        private static void Rubble(Transform parent, Vector3 centre, int pieces)
        {
            var random = new System.Random((int)(centre.x * 31f + centre.z * 17f));
            for (int i = 0; i < pieces; i++)
            {
                float size = 0.3f + (float)random.NextDouble() * 0.7f;
                var at = centre + new Vector3((float)random.NextDouble() * 3f - 1.5f, size * 0.35f, (float)random.NextDouble() * 3f - 1.5f);
                var piece = BuildKit.Shape(PrimitiveType.Cube, "Rubble", parent, at, new Vector3(size, size * 0.7f, size * 0.9f), i % 2 == 0 ? wall : wallDark,
                    new Vector3((float)random.NextDouble() * 24f - 12f, (float)random.NextDouble() * 360f, (float)random.NextDouble() * 24f - 12f));
                piece.gameObject.isStatic = true;
            }
        }

        // A long banner hanging on a wall, torn at the foot.
        private static void Banner(Transform parent, Vector3 top, float yaw)
        {
            var group = BuildKit.Group("Banner", parent);
            group.SetPositionAndRotation(top, Quaternion.Euler(0f, yaw, 0f));
            BuildKit.Shape(PrimitiveType.Cube, "Rod", group, Vector3.zero, new Vector3(1.5f, 0.08f, 0.08f), iron);
            BuildKit.Shape(PrimitiveType.Cube, "Cloth", group, new Vector3(0f, -1.3f, 0f), new Vector3(1.2f, 2.6f, 0.04f), cloth);
            BuildKit.Shape(PrimitiveType.Cube, "Tail", group, new Vector3(-0.3f, -2.9f, 0f), new Vector3(0.5f, 0.7f, 0.04f), cloth);
            BuildKit.Shape(PrimitiveType.Cube, "Mark", group, new Vector3(0f, -1f, -0.03f), new Vector3(0.12f, 1.1f, 0.02f), rune, new Vector3(0f, 0f, 18f));
        }

        // The storm itself: rain that follows the player, and the bolts.
        internal static void Weather(Transform player, StormLight storm)
        {
            var weather = BuildKit.Group("Weather");

            var rainObject = new GameObject("Rain");
            rainObject.transform.SetParent(weather, false);
            // Driven a little sideways by the wind.
            rainObject.transform.rotation = Quaternion.Euler(78f, 62f, 0f);
            rainObject.AddComponent<Follow>().Configure(player, new Vector3(-2f, 16f, 0.5f));
            var rain = rainObject.AddComponent<ParticleSystem>();
            rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = rain.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.75f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(20f, 26f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.055f);
            main.maxParticles = 4000;
            var emission = rain.emission;
            emission.rateOverTime = 2000f;
            var shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(34f, 26f, 1f);
            var rainRenderer = rainObject.GetComponent<ParticleSystemRenderer>();
            rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            rainRenderer.velocityScale = 0.03f;
            rainRenderer.lengthScale = 9f;
            rainRenderer.sharedMaterial = rainMaterial;
            rainRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Where the rain lands: small rings that open and vanish all over the ground near the player.
            var splashObject = new GameObject("Rain Splashes");
            splashObject.transform.SetParent(weather, false);
            splashObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            splashObject.AddComponent<Follow>().Configure(player, new Vector3(0f, 0.08f, 2f));
            var splash = splashObject.AddComponent<ParticleSystem>();
            splash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var splashMain = splash.main;
            splashMain.loop = true;
            splashMain.playOnAwake = true;
            splashMain.simulationSpace = ParticleSystemSimulationSpace.World;
            splashMain.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.3f);
            splashMain.startSpeed = 0f;
            splashMain.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            splashMain.maxParticles = 600;
            var splashEmission = splash.emission;
            splashEmission.rateOverTime = 420f;
            var splashShape = splash.shape;
            splashShape.shapeType = ParticleSystemShapeType.Box;
            splashShape.scale = new Vector3(34f, 24f, 0.01f);
            var splashSize = splash.sizeOverLifetime;
            splashSize.enabled = true;
            splashSize.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.3f, 1f, 1f));
            var splashColour = splash.colorOverLifetime;
            splashColour.enabled = true;
            var splashFade = new Gradient();
            splashFade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            splashColour.color = splashFade;
            var splashRenderer = splashObject.GetComponent<ParticleSystemRenderer>();
            splashRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            splashRenderer.sharedMaterial = BuildKit.Glow(Folder, "m_castle_splash", new Color(0.75f, 0.85f, 1.3f, 0.5f), 4, true, 1f).Graphic(0.35f);
            splashRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var boltsObject = new GameObject("Lightning");
            boltsObject.transform.SetParent(weather, false);
            var lines = new LineRenderer[3];
            for (int i = 0; i < lines.Length; i++)
            {
                var lineObject = new GameObject("Bolt " + (i + 1));
                lineObject.transform.SetParent(boltsObject.transform, false);
                var line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.sharedMaterial = boltMaterial;
                line.widthMultiplier = 0.2f;
                line.numCornerVertices = 2;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.enabled = false;
                lines[i] = line;
            }
            var flashObject = new GameObject("Bolt Flash");
            flashObject.transform.SetParent(boltsObject.transform, false);
            var flash = flashObject.AddComponent<Light>();
            flash.type = LightType.Point;
            flash.color = new Color(0.7f, 0.82f, 1f);
            flash.range = 16f;
            flash.intensity = 0f;
            flash.shadows = LightShadows.None;
            flash.enabled = false;
            boltsObject.AddComponent<LightningBolts>().Configure(lines, storm, flash);
        }
    }
}
