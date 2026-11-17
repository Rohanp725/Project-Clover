using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.EditorTools
{
    /// <summary>
    /// The sun-king's city (docs/design/fire_land.md, section 3): the dry river where the player wakes, the city of
    /// wicks, the golden way and its hall of columns, and the last gate with its champion. One walk north, about
    /// 500 m, with five rest fires; a fall returns the player to the last one lit (<see cref="LandReturn"/>). Each
    /// district has its own ground so it reads as its own place from above: cracked mud in the riverbed only, packed
    /// earth in the streets, pale paving on the king's road. Not in the list of scenes the game is built from.
    /// </summary>
    public static class SunCityBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/SunCity.unity";

        // The hall of columns: the roof stands this far over its floor, and is open over these strips of floor.
        public const float HallFloor = 4f, HallRoof = 10.5f, HallWest = -22f, HallEast = 22f, HallSouth = 400f, HallNorth = 460f;
        /// <summary>Where the sun reaches the hall's floor (west, east, south, north): three bars across it and the nave.</summary>
        public static readonly Vector4[] HallLight =
        {
            new Vector4(-22f, 22f, 408f, 412f), new Vector4(-22f, 22f, 428f, 432f), new Vector4(-22f, 22f, 448f, 452f), new Vector4(-2f, 2f, 400f, 460f),
        };
        public const float ChampionsLine = 470f;

        private static Transform world;
        private static readonly List<Interactable> things = new List<Interactable>();
        private static readonly List<Enemy> enemies = new List<Enemy>();
        private static readonly List<RestPoint> fires = new List<RestPoint>();
        private static EnemyBuilder.Kit kit;
        private static Transform enemyRoot;
        private static System.Random dice;
        private static Material mud, silt, sand, earth, earthDark, brick, brickDark, stone, stoneNew, stoneDark, shade, hallFloor, paveA, paveB, paveC, newA, newB, gold, ember, wood, reed, cloth, blue, clay, bronze, wrap;

        [MenuItem("Clover/Build/Sun City")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the sun city")) return;
            things.Clear();
            enemies.Clear();
            fires.Clear();
            dice = new System.Random(1117);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Materials();
            ReadySoft();
            world = BuildKit.Group("World");

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, 4f), "");
            if (built.Player == null) return;
            LookBuilder.Apply(LookBuilder.Noon, built.Camera);
            // Most of the hall of columns lies under its roof's shadow: the shade must stay a place one can see and fight in.
            if (RenderSettings.sun != null) RenderSettings.sun.shadowStrength = 0.72f;
            // The owner's decision of 2026-11-17: the player enters this land with fists and no element.
            built.Player.GetComponent<PlayerCombat>().StartingWeapon = Weapons.FistsName;
            built.Player.GetComponent<PlayerElement>().StartsWithNone = true;

            var start = BuildKit.Group("Start Point", world);
            start.position = new Vector3(0f, 0.05f, 4f);
            kit = EnemyBuilder.NewKit(built.Player.transform, built.Camera.transform.rotation);
            enemyRoot = BuildKit.Group("Enemies");
            var director = new GameObject("Area Director").AddComponent<AreaDirector>();

            Beyond();
            DryRiver(director);
            WellShrine();
            CityOfWicks(director);
            PaintedTombs();
            GoldenWay(director);
            HallOfColumns(director);
            LastGate(director);

            var strikeOutline = WorldBars.Marker("Storm Mark Outline", director.transform, 0f, kit.Marker);
            var strikeFill = WorldBars.Marker("Storm Mark Fill", director.transform, 0f, kit.Marker);
            director.Configure(built.Player.GetComponent<PlayerController>(), enemies.ToArray(), start, strikeOutline, strikeFill);
            kit.Turns.Configure(enemies.ToArray());
            // Death returns to the last rest fire: no run end here. The land's own terms are on this component.
            director.gameObject.AddComponent<LandReturn>().Configure(director, built.Player.GetComponent<PlayerDefence>());
            built.Player.GetComponent<PlayerInteraction>().Configure(things.ToArray());

            // The sponsor: what it says on waking, at each fire the first time it is lit, and after the first fall (section 7.4).
            var voice = new GameObject("Land Voice").AddComponent<LandVoice>();
            voice.WakingKey = "suncity.woke";
            voice.Waking = new[]
            {
                "You are awake. Good.",
                "You do not know your name. I took it, with everything behind it. That was the price of pulling you out.",
                "This is the sun-king's country. There was a river here.",
                "A man found fire lying in the desert and named himself the sun. No god ruled here before him. He is the first, and he is a thief.",
                "I have nothing to lend you. Your hands will do. What you need, he has.",
                "If you fall I will turn the hours back. Not far. To the last fire you lit.",
            };
            voice.AfterAFall = new[] { "You fell. I turned it back as far as the fire.", "They are standing again. So are you." };
            voice.Configure(built.Player.GetComponent<PlayerDefence>(), fires.ToArray(), new[]
            {
                "Light it.\nFire answers anyone who asks. That is how he began.\nI can reach you at a fire. Nowhere else, in this light.",
                "They drank the river and are thirsty still.\nHe told them the sun would be enough.",
                "Take the spear. Its owner is done with it.\nHe carries one like it. His is better. It will be yours.",
                "Every face on this road is his.\nA man who cannot bear to see another's.",
                "Past the gate is his court. He will tell you he carried it home for them.\nLook at them. Then listen to him.",
            });

            PlayerBuilder.AddTitle("The Sun-King's City", "where a herdsman became the sun");
            PlayerBuilder.AddPauseHint();
            SceneBuild.FinishOutsideBuild(scene, ScenePath);
        }

        private static void Materials()
        {
            mud = SunCityKit.Mud;
            brick = SunCityKit.Brick;
            brickDark = SunCityKit.BrickDark;
            stone = SunCityKit.Stone;
            shade = SunCityKit.Shade;
            hallFloor = SunCityKit.Flat("m_sun_hall_floor", new Color(0.62f, 0.58f, 0.62f));
            paveA = SunCityKit.Flat("m_sun_pave_a", new Color(0.65f, 0.61f, 0.54f), 0.012f);
            paveB = SunCityKit.Flat("m_sun_pave_b", new Color(0.61f, 0.57f, 0.5f), 0.012f);
            paveC = SunCityKit.Flat("m_sun_pave_c", new Color(0.69f, 0.65f, 0.58f), 0.012f);
            newA = SunCityKit.Flat("m_sun_pave_new_a", new Color(0.8f, 0.77f, 0.7f), 0.012f);
            newB = SunCityKit.Flat("m_sun_pave_new_b", new Color(0.75f, 0.72f, 0.65f), 0.012f);
            silt = SunCityKit.Flat("m_sun_silt", new Color(0.6f, 0.56f, 0.49f));
            sand = SunCityKit.Flat("m_sun_sand", new Color(0.71f, 0.63f, 0.48f));
            earth = SunCityKit.Flat("m_sun_earth", new Color(0.6f, 0.51f, 0.39f));
            earthDark = SunCityKit.Flat("m_sun_earth_dark", new Color(0.5f, 0.42f, 0.33f));
            stoneNew = SunCityKit.For("m_kit_stone_new");
            stoneDark = SunCityKit.For("m_kit_stone_dark");
            gold = SunCityKit.For("m_kit_gold");
            ember = SunCityKit.For("m_kit_ember");
            wood = SunCityKit.For("m_kit_wood");
            reed = SunCityKit.For("m_kit_reed");
            cloth = SunCityKit.For("m_kit_cloth");
            blue = SunCityKit.For("m_kit_water");
            clay = SunCityKit.Flat("m_sun_clay", new Color(0.52f, 0.33f, 0.25f), 0.012f);
            // Bronze that holds its colour in shade: the gate of noon's doors face south, away from the sun, and were nearly black.
            bronze = SunCityKit.Flat("m_sun_bronze", new Color(0.62f, 0.47f, 0.27f), 0.014f).Lit(new Color(1.6f, 1.2f, 0.5f), 0.5f, new Color(0.34f, 0.2f, 0.06f));
            wrap = SunCityKit.Flat("m_sun_wrap", new Color(0.72f, 0.69f, 0.62f), 0.012f);
        }

        // ------------------------------------------------------------------ the land beyond the walls

        // A plain of dust under everything, so nothing ends in a void. It is cut open over the sunk tombs.
        private static void Beyond()
        {
            var group = BuildKit.Group("Beyond", world);
            void Plain(float west, float east, float south, float north)
            {
                var plain = Deco(PrimitiveType.Cube, "Plain", group, new Vector3((west + east) * 0.5f, -0.8f, (south + north) * 0.5f), new Vector3(east - west, 1f, north - south), sand);
                plain.shadowCastingMode = ShadowCastingMode.Off;
            }
            Plain(-160f, 160f, -80f, 256.5f);
            Plain(-160f, 160f, 268f, 640f);
            Plain(-160f, -58f, 256.5f, 268f);
            Plain(-20f, 160f, 256.5f, 268f);
            // Over the last gate: the obelisk of the court beyond, close now, its cap the brightest thing in the land.
            SunCityKit.Place("obelisk", group, new Vector3(0f, 4f, 516f), 180f, 1.5f, false);
        }

        // ------------------------------------------------------------------ district 1: the dry river

        private static void DryRiver(AreaDirector director)
        {
            var group = BuildKit.Group("Dry River", world);
            // Stood on: one slab. Seen: the kit's cracked plates in the bed, broken up by pans of pale silt.
            var bed = Floor("Riverbed", group, -24f, 24f, -3f, 120f, 0f, mud);
            bed.GetComponent<Renderer>().enabled = false;
            Deco(PrimitiveType.Cube, "Riverbed Under", group, new Vector3(0f, -0.65f, 58.5f), new Vector3(48f, 1f, 123f), mud).shadowCastingMode = ShadowCastingMode.Off;
            var plates = BuildKit.Group("Mud", group);
            for (int z = -1; z < 30; z++)
            {
                int half = z >= 25 ? 6 : 4;
                for (int x = -half; x < half; x++)
                {
                    var at = new Vector3(x * 4f + 2f, 0f, z * 4f + 2f);
                    SunCityKit.Place("mud_tile", plates, at + Vector3.up * -0.19f, dice.Next(4) * 90f, 1f, false);
                }
            }

            // Pans where the last water stood: pale silt lying in the mud. Each is an uneven patch whose colour thins to
            // nothing at its edge, so it stains the plates under it and has no outline of its own.
            for (int i = 0; i < 34; i++)
            {
                float z = 3f + i * 3.4f + (float)dice.NextDouble() * 3f;
                float reach = z > 101f ? 21f : 11f;
                var at = new Vector3(Mathf.Lerp(-reach, reach, (float)dice.NextDouble()), 0.016f + i * 0.0004f, z);
                // One in three is where it dried last: darker, not paler.
                Soft("Silt Pan", plates, at, 2.2f + (float)dice.NextDouble() * 2.6f, 1.5f + (float)dice.NextDouble() * 1.8f, (float)dice.NextDouble() * 360f, i % 3 == 0 ? softDamp : softSilt);
            }

            // The banks: dust on top, dark brick holding them up. The west bank is cut by the gully to the well.
            Floor("Bank West South", group, -30f, -14f, -3f, 78f, 1.5f, sand, 2f);
            Floor("Bank West Gully", group, -30f, -20f, 78f, 84f, 1.5f, earth, 2f);
            Floor("Bank West North", group, -30f, -14f, 84f, 100f, 1.5f, sand, 2f);
            Floor("Bank West Basin", group, -30f, -24f, 100f, 120f, 1.5f, sand, 2f);
            Floor("Bank East", group, 14f, 30f, -3f, 100f, 1.5f, sand, 2f);
            Floor("Bank East Basin", group, 24f, 30f, 100f, 120f, 1.5f, sand, 2f);
            Box("Retaining West A", group, -14.7f, -14f, -3f, 78f, 0f, 1.95f, brickDark);
            Box("Retaining West B", group, -14.7f, -14f, 84f, 100f, 0f, 1.95f, brickDark);
            Box("Retaining East", group, 14f, 14.7f, -3f, 100f, 0f, 1.95f, brickDark);
            Box("Basin Side West", group, -24.7f, -24f, 100f, 120f, 0f, 1.95f, brickDark);
            Box("Basin Side East", group, 24f, 24.7f, 100f, 120f, 0f, 1.95f, brickDark);
            Box("Gully Fence South", group, -30f, -14f, 77f, 78f, 1.5f, 2.3f, brickDark);
            Box("Gully Fence North", group, -30f, -14f, 84f, 85f, 1.5f, 2.6f, brickDark);
            Stairs(group, new Vector3(-14f, 0f, 81f), new Vector3(-20f, 1.5f, 81f), 6f);
            Box("Outer Wall East", group, 30f, 31.5f, -3f, 140f, 0f, 5f, brick);
            Box("Outer Wall West A", group, -31.5f, -30f, -3f, 68f, 0f, 5f, brick);
            Box("Outer Wall West B", group, -31.5f, -30f, 94f, 140f, 0f, 5f, brick);
            Box("River Mouth Wall", group, -31.5f, 31.5f, -4.5f, -3f, -0.5f, 1.2f, brickDark);
            // Dust blown down off the banks lies in drifts at their feet; a few reach out into the bed.
            for (int i = 0; i < 26; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float z = 2f + i * 3.7f + (float)dice.NextDouble() * 2f;
                if (side < 0f && z > 75f && z < 87f) continue;
                Drift(group, new Vector3(side * (12.6f - (float)dice.NextDouble() * 1.2f), 0f, z), 3f + (float)dice.NextDouble() * 3f, 2.2f + (float)dice.NextDouble() * 2.5f);
            }
            foreach (var at in new[] { new Vector3(-6f, 0f, 18f), new Vector3(8f, 0f, 35f), new Vector3(-9f, 0f, 52f), new Vector3(-2f, 0f, 70f), new Vector3(-8f, 0f, 93f) })
                Drift(group, at, 5f, 3f);

            // Reed boats lying where the water left them.
            Tilt(SunCityKit.Place("reed_boat", group, new Vector3(-7f, 0f, 9f), 70f), 0f, 9f);
            Tilt(SunCityKit.Place("reed_boat", group, new Vector3(-9.5f, 0f, 44f), 115f), 0f, -11f);
            Tilt(SunCityKit.Place("reed_boat", group, new Vector3(9f, 0f, 55f), 200f), 6f, 14f);
            Tilt(SunCityKit.Place("reed_boat", group, new Vector3(-10f, 0f, 96f), 30f), 0f, -8f);
            // A dry pool the first of the thirsting kneels at.
            Stain(group, new Vector3(-4f, 0f, 27.4f), 2.6f);
            // The great jar, tipped, a dark dry stain under its mouth.
            var jar = Deco(PrimitiveType.Cylinder, "Great Jar", group, new Vector3(0.5f, 1f, 62f), new Vector3(2f, 1.5f, 2f), clay, new Vector3(0f, 0f, 84f));
            jar.gameObject.AddComponent<BoxCollider>();
            Deco(PrimitiveType.Cylinder, "Great Jar Mouth", group, new Vector3(-1.1f, 0.84f, 62f), new Vector3(1.3f, 0.12f, 1.3f), earthDark, new Vector3(0f, 0f, 84f));
            Stain(group, new Vector3(-2.6f, 0f, 62f), 2.2f);

            // The barque, stranded: the district's landmark and its one big shade.
            // It never thins out: it is the thing to be seen here. Only the hull is solid, not the oars, posts and mast.
            var barque = SunCityKit.Place("barque", group, new Vector3(5.5f, 0f, 84f), 200f, 0.73f, false);
            Solid(barque, new Vector3(0.5f, 2.2f, -0.3f), new Vector3(6.4f, 4.4f, 29f), false);
            Carving(group, new Vector3(3.6f, 0f, 97.4f), 150f, "The barque's prow", "\"The boat of the morning. It waits for one who rises.\"");

            // The harbour: a basin wall with a gap, a dry cistern, jars, the quay and its stair.
            Box("Basin Wall West", group, -14f, -6f, 99.4f, 100.4f, 0f, 1.2f, stone);
            Box("Basin Wall East", group, 6f, 14f, 99.4f, 100.4f, 0f, 1.2f, stone);
            Disc("Dry Cistern", group, new Vector3(0f, 0f, 105.5f), 1.6f, 0.8f, stone);
            Deco(PrimitiveType.Cylinder, "Cistern Dark", group, new Vector3(0f, 0.81f, 105.5f), new Vector3(2.5f, 0.01f, 2.5f), shade);
            for (int i = 0; i < 5; i++)
            {
                var pot = Deco(PrimitiveType.Cylinder, "Jar", group, new Vector3(-19.5f + i % 3 * 1.3f, 0.55f, 102.4f + i / 3 * 1.4f), new Vector3(0.9f, 0.55f, 0.9f), clay);
                pot.gameObject.AddComponent<BoxCollider>();
            }
            Floor("Quay", group, -30f, 30f, 120f, 140f, 2f, stone, 2.6f);
            Paving(group, -30f, 30f, 120f, 140f, 2f, 2.5f, paveA, paveB, paveC, 0.22f);
            Stairs(group, new Vector3(0f, 0f, 114f), new Vector3(0f, 2f, 120f), 8f);
            for (float x = -27f; x <= 27f; x += 6f)
            {
                if (Mathf.Abs(x) < 5f) continue;
                Box("Mooring Stone", group, x - 0.4f, x + 0.4f, 120.5f, 121.3f, 2f, 2.9f, stoneDark);
            }
            Carving(group, new Vector3(7.5f, 2f, 122.6f), 180f, "A mooring stone",
                "\"Here the boats of both banks tie up. High water or low, the river gives.\"", "Scratched under it, later: \"The river is given to the sun. Drink light.\"");
            // A timber crane that lifted cargo out of boats.
            Box("Crane Post", group, 17.7f, 18.3f, 121.6f, 122.2f, 2f, 6.5f, wood);
            Deco(PrimitiveType.Cube, "Crane Arm", group, new Vector3(18f, 6.4f, 119.6f), new Vector3(0.35f, 0.35f, 6.5f), wood, new Vector3(-14f, 0f, 0f));
            Deco(PrimitiveType.Cube, "Crane Rope", group, new Vector3(18f, 4.6f, 116.9f), new Vector3(0.06f, 2.4f, 0.06f), reed);
            SunCityKit.Place("market_awning", group, new Vector3(-19f, 2f, 133f), 180f, 1f, false);
            SunCityKit.Place("market_awning", group, new Vector3(20f, 2f, 134f), 180f, 1f, false);
            Drift(group, new Vector3(-24f, 2f, 137.5f), 8f, 3f);
            Drift(group, new Vector3(22f, 2f, 138f), 9f, 2.6f);

            Rest(group, director, new Vector3(4f, 0f, 12f));
            Rest(group, director, new Vector3(-8f, 2f, 126f));

            // E1.1: on its knees at a dry pool, its back to the way in.
            Spawn(EnemyKind.ThirstName, -4f, 0f, 26f, 0f).MakeDormant(3f, "thirst_kneel_loop");
            Spawn(EnemyKind.ThirstName, 3f, 0f, 44f, 180f);
            Spawn(EnemyKind.ThirstName, -6f, 0f, 62f, 180f);
            Spawn(EnemyKind.ThirstName, 7f, 0f, 62f, 180f);
            // E1.4: three by the hull.
            Spawn(EnemyKind.ThirstName, -5f, 0f, 80f, 170f);
            Spawn(EnemyKind.ThirstName, -4.5f, 0f, 75f, 200f);
            Spawn(EnemyKind.ThirstName, -6.5f, 0f, 89f, 160f);
            // E1.5: the land's first crowd, round the cistern and at the jars.
            Spawn(EnemyKind.ThirstName, -5f, 0f, 108f, 180f);
            Spawn(EnemyKind.ThirstName, 0f, 0f, 110f, 180f);
            Spawn(EnemyKind.ThirstName, 5f, 0f, 108f, 180f);
            Spawn(EnemyKind.ThirstName, -16f, 0f, 104f, 120f);
        }

        // The side path: a sunk court with the well the king closed.
        private static void WellShrine()
        {
            var group = BuildKit.Group("Well Shrine", world);
            Floor("Well Court", group, -52f, -30f, 68f, 94f, 1.5f, earth, 2f);
            Paving(group, -47f, -35f, 78f, 90f, 1.5f, 2f, paveA, paveB, paveC, 0.3f);
            Scatter(group, -51f, -31f, 69f, 93f, 1.5f, 26);
            Box("Well Wall North", group, -53.5f, -30f, 94f, 95.5f, 1.5f, 4.8f, brick);
            Box("Well Wall West", group, -53.5f, -52f, 66.5f, 94f, 1.5f, 4.8f, brick);
            Box("Well Wall South", group, -52f, -30f, 66.5f, 68f, 1.5f, 2.7f, brickDark);
            Box("Well Wall East A", group, -31f, -30f, 68f, 78f, 1.5f, 4.8f, brick);
            Box("Well Wall East B", group, -31f, -30f, 84f, 94f, 1.5f, 4.8f, brick);
            SunCityKit.Place("well_shrine", group, new Vector3(-41f, 1.5f, 85f));
            Drift(group, new Vector3(-49f, 1.5f, 91f), 5f, 4f);
            Drift(group, new Vector3(-33.5f, 1.5f, 70.5f), 5f, 3.4f);
            Carving(group, new Vector3(-37.2f, 1.5f, 81.6f), 200f, "The well's lid",
                "On the gold bands: \"Closed by the king's word. Water is the old debt. The sun pays every debt.\"",
                "Under the rim, older, small: \"Keeper of the hearth, opener of the day: the first cup is yours.\"");
            // An offering table with a clay bowl, and in the bowl a full waterskin.
            Box("Offering Table", group, -41.8f, -40.2f, 78.4f, 79.4f, 1.5f, 2.3f, stone);
            Deco(PrimitiveType.Cylinder, "Bowl", group, new Vector3(-41f, 2.36f, 78.9f), new Vector3(0.8f, 0.07f, 0.8f), clay);
            var skin = Deco(PrimitiveType.Sphere, "Waterskin", group, new Vector3(-41f, 2.52f, 78.9f), new Vector3(0.5f, 0.3f, 0.34f), blue);
            var gift = new GameObject("Waterskin Gift").AddComponent<LandGift>();
            gift.transform.SetParent(group, false);
            gift.transform.position = new Vector3(-41f, 1.5f, 77.9f);
            gift.Prompt = "Take the waterskin";
            gift.Range = 2.2f;
            gift.FlasksAfter = 4;
            gift.Words = "A fourth flask";
            gift.Configure(skin.gameObject);
            things.Add(gift);
            // Two on their knees at the lid, scratching. They do not notice anyone beyond five metres.
            Spawn(EnemyKind.ThirstName, -43.9f, 1.5f, 82.6f, 80f).MakeDormant(5f, "thirst_kneel_loop");
            Spawn(EnemyKind.ThirstName, -39.6f, 1.5f, 81.4f, 340f).MakeDormant(5f, "thirst_kneel_loop");
        }

        // ------------------------------------------------------------------ district 2: the city of wicks

        private static void CityOfWicks(AreaDirector director)
        {
            var group = BuildKit.Group("City Of Wicks", world);
            // Packed earth, trodden darker down the middle of each street; brick only where a doorstep was laid.
            Floor("City Ground", group, -20f, 30f, 140f, 290f, 2f, earth, 2.6f);
            Floor("City Ground West A", group, -30f, -20f, 140f, 258f, 2f, earth, 2.6f);
            Floor("City Ground West B", group, -30f, -20f, 266f, 290f, 2f, earth, 2.6f);
            Trodden(group, 2.5f, 141f, 2.5f, 182f, 2.2f);
            Trodden(group, 2.5f, 182.5f, 22.5f, 182.5f, 2.2f);
            Trodden(group, 22.5f, 182.5f, 22.5f, 225f, 2.2f);
            Trodden(group, 22.5f, 225f, 0f, 225f, 3f);
            Trodden(group, 0f, 225f, 0f, 288f, 3f);

            // The Street of Lamps: houses either side, a lamp niche every eight metres, cold but where a wick stands near.
            Mass(group, -30f, -6f, 140f, 180f, 2f, 4f);
            Mass(group, 11f, 30f, 140f, 176f, 2f, 4f);
            for (int i = 0; i < 6; i++)
            {
                SunCityKit.Place(i == 4 ? "house_block" : "house_block", group, new Vector3(-3f, 2f, 143.2f + i * 6.1f), 90f);
                SunCityKit.Place("house_block", group, new Vector3(8f, 2f, 143.2f + i * 6.1f - (i == 5 ? 0.5f : 0f)), 270f);
            }
            Box("Lamp Street Corner", group, -6f, 0f, 176.2f, 185f, 2f, 6f, brick);
            for (int i = 0; i < 5; i++)
            {
                float z = 144f + i * 8f;
                bool lit = i == 1 || i == 3;
                Deco(PrimitiveType.Cube, "Lamp Niche", group, new Vector3(0.12f, 3.5f, z), new Vector3(0.2f, 0.5f, 0.5f), shade);
                Deco(PrimitiveType.Sphere, "Lamp", group, new Vector3(0.24f, 3.42f, z), new Vector3(0.16f, 0.16f, 0.22f), lit ? ember : clay);
            }
            Carving(group, new Vector3(0.75f, 2f, 159f), 90f, "A hearth niche",
                "\"A fire in every house, and it is one fire. Bank it at night.\"", "\"In the morning\" and then a name, cut away, \"lights the day from it.\"");
            Doorstep(group, new Vector3(1f, 2f, 149f));
            Doorstep(group, new Vector3(4.2f, 2f, 161.5f));
            Doorstep(group, new Vector3(1f, 2f, 168f));
            Drift(group, new Vector3(4.3f, 2f, 143f), 2.4f, 4f);
            Drift(group, new Vector3(0.9f, 2f, 174f), 2f, 3.5f);

            // Potters' Row: its south side is a broken wall, low, so the camera sees over it.
            Box("Potters Low Wall", group, 5f, 30f, 176.2f, 180f, 2f, 3.4f, brickDark);
            Rubble(group, 6f, 29f, 176.6f, 179.6f, 3.4f, 14);
            Mass(group, -30f, 20f, 191f, 208f, 2f, 4f);
            Box("Potters North Infill", group, -30f, 0f, 185f, 191f, 2f, 6f, brick);
            for (int i = 0; i < 3; i++) SunCityKit.Place("house_block", group, new Vector3(3.1f + i * 6.1f, 2f, 188.1f), 180f);
            for (int i = 0; i < 6; i++)
            {
                // Pots left out to dry, never fired.
                var pot = Deco(PrimitiveType.Cylinder, "Pot", group, new Vector3(6.5f + i * 2.3f, 2.3f, 180.6f + (i % 2) * 0.4f), new Vector3(0.5f + i % 3 * 0.12f, 0.3f, 0.5f + i % 3 * 0.12f), clay);
                pot.gameObject.isStatic = true;
            }
            // The east alley: bundles of the wrapped dead against the wall, and one of them is not still.
            Mass(group, 25f, 30f, 176f, 236f, 2f, 4.4f);
            for (int i = 0; i < 9; i++) SunCityKit.Place("wall_section", group, new Vector3(25.4f, 2f, 183f + i * 6f), 270f, 1f, false);
            for (int i = 0; i < 2; i++) SunCityKit.Place("house_block", group, new Vector3(17f, 2f, 194.2f + i * 6.1f), 90f);
            for (int i = 0; i < 7; i++)
            {
                // The wrapped dead, laid in a row along the wall and stacked where there were too many.
                bool upper = i % 3 == 2;
                var bundle = Deco(PrimitiveType.Capsule, "Wrapped Bundle", group, new Vector3(24.1f - (upper ? 0.1f : 0f), upper ? 2.72f : 2.27f, 186.5f + i * 0.74f + (i > 3 ? 8.5f : 0f) - (upper ? 0.37f : 0f)),
                    new Vector3(0.5f, 0.9f, 0.5f), wrap, new Vector3(0f, 0f, 90f));
                Deco(PrimitiveType.Cube, "Bundle Binding", group, bundle.transform.position, new Vector3(0.3f, 0.53f, 0.53f), reed);
            }
            Scatter(group, 20.5f, 24.5f, 186f, 235f, 2f, 16);
            Scatter(group, 0.5f, 4.5f, 141f, 180f, 2f, 12);
            Box("Market Low Wall", group, -12f, 20f, 208f, 214f, 2f, 3.4f, brickDark);
            Rubble(group, -11f, 19f, 208.5f, 213.5f, 3.4f, 16);
            Box("Market West Fill", group, -30f, -12f, 208f, 214f, 2f, 6f, brick);

            // The market: the district's one open space. A cold oven with grey bread on its shelf; awnings.
            Mass(group, -30f, -18f, 214f, 236f, 2f, 4f);
            for (int i = 0; i < 4; i++) SunCityKit.Place("house_block", group, new Vector3(-15f, 2f, 216.9f + i * 6.1f - (i == 3 ? 1.5f : 0f)), 90f);
            var oven = Deco(PrimitiveType.Sphere, "Cold Oven", group, new Vector3(2f, 2f, 230.5f), new Vector3(4f, 3.6f, 4f), brickDark);
            oven.gameObject.AddComponent<SphereCollider>();
            Deco(PrimitiveType.Cube, "Oven Mouth", group, new Vector3(2f, 2.5f, 228.55f), new Vector3(1.1f, 0.8f, 0.3f), shade);
            Deco(PrimitiveType.Cube, "Oven Shelf", group, new Vector3(2f, 2.9f, 228.3f), new Vector3(1.8f, 0.08f, 0.5f), stoneDark);
            for (int i = 0; i < 3; i++) Deco(PrimitiveType.Sphere, "Grey Bread", group, new Vector3(1.5f + i * 0.5f, 3.02f, 228.3f), new Vector3(0.4f, 0.16f, 0.34f), silt);
            SunCityKit.Place("market_awning", group, new Vector3(-7.5f, 2f, 232.5f), 180f, 1f, false);
            SunCityKit.Place("market_awning", group, new Vector3(13f, 2f, 233f), 180f, 1f, false);
            SunCityKit.Place("market_awning", group, new Vector3(16f, 2f, 217f), 270f, 1f, false);
            Paving(group, -6f, 10f, 218f, 228f, 2f, 1.6f, paveB, paveA, earthDark, 0.5f);
            Scatter(group, -11f, 24f, 215f, 235f, 2f, 40);
            Drift(group, new Vector3(-9.5f, 2f, 216.2f), 5f, 3f);
            Drift(group, new Vector3(18f, 2f, 234.4f), 6f, 2.6f);

            // The Embalmers' Gate: two square towers, the passage between them, timber doors standing open.
            Box("Gate Wall West", group, -30f, -11f, 236f, 246f, 2f, 5.2f, brick);
            Box("Gate Wall East", group, 11f, 25f, 236f, 246f, 2f, 5.2f, brick);
            Box("Gate Tower West", group, -11f, -6f, 236f, 246f, 2f, 9f, brick);
            Box("Gate Tower East", group, 6f, 11f, 236f, 246f, 2f, 9f, brick);
            foreach (float x in new[] { -8.5f, 8.5f })
                Deco(PrimitiveType.Cube, "Tower Cap", group, new Vector3(x, 9.15f, 241f), new Vector3(5.6f, 0.3f, 10.6f), brickDark);
            Deco(PrimitiveType.Cube, "Gate Door West", group, new Vector3(-5.8f, 4.4f, 243.6f), new Vector3(0.25f, 4.8f, 3.6f), wood);
            Deco(PrimitiveType.Cube, "Gate Door East", group, new Vector3(5.8f, 4.4f, 243.6f), new Vector3(0.25f, 4.8f, 3.6f), wood);
            Paving(group, -6f, 6f, 236f, 246f, 2f, 2f, paveB, paveA, paveC, 0.15f);
            // On the east door a gilded soldier is pinned to the wood by his own spear: a hand's width of gold on his mask and no more.
            PinnedSoldier(group, new Vector3(5.45f, 2f, 243.4f));
            Carving(group, new Vector3(5.1f, 2f, 239.2f), 270f, "A tablet in the gate",
                "\"The guard of the golden way wears the god's flesh. Whoever refuses the pouring holds the lower gate, and holds it until he is relieved.\"", "Nobody relieves him.");
            Rest(group, director, new Vector3(-4f, 2f, 240f));
            Carving(group, new Vector3(-5.2f, 2f, 247.2f), 180f, "The lintel of the yard", "\"The king says: you burn and are not consumed. Wrap them. Stand them in the sun. No one here is buried.\"");

            // The embalmers' yard: stone tables, heaps of natron, the wrapped dead standing in niches along the north wall.
            Box("Yard Wall East", group, 20f, 30f, 246f, 290f, 2f, 6f, brick);
            Box("Yard Wall West A", group, -30f, -20f, 246f, 258f, 2f, 6f, brick);
            Box("Yard Wall West B", group, -30f, -20f, 266f, 290f, 2f, 6f, brick);
            Box("Yard Wall North West", group, -20f, -6f, 286f, 290f, 2f, 6f, brick);
            Box("Yard Wall North East", group, 6f, 20f, 286f, 290f, 2f, 6f, brick);
            Paving(group, -18f, 18f, 248f, 284f, 2f, 2.4f, paveB, paveA, paveC, 0.5f);
            Scatter(group, -19f, 19f, 247f, 285f, 2f, 40);
            foreach (float x in new[] { -17f, -13.9f, -10.8f, 10.8f, 13.9f, 17f })
                SunCityKit.Place("burial_niche", group, new Vector3(x, 2f, 285.1f), 180f);
            SunCityKit.Place("burial_niche", group, new Vector3(19.1f, 2f, 272f), 270f);
            SunCityKit.Place("burial_niche", group, new Vector3(19.1f, 2f, 275.2f), 270f);
            foreach (var at in new[] { new Vector3(-10f, 2f, 276f), new Vector3(6f, 2f, 280f), new Vector3(-4f, 2f, 266f), new Vector3(12f, 2f, 258f), new Vector3(-13f, 2f, 254f), new Vector3(3f, 2f, 252f) })
            {
                // A table to lay the dead out on: low, of stone, a drain cut round it.
                Floor("Embalming Table", group, at.x - 0.75f, at.x + 0.75f, at.z - 1.4f, at.z + 1.4f, at.y + 0.34f, stone, 0.34f);
                Deco(PrimitiveType.Cube, "Table Foot", group, at + new Vector3(0f, 0.02f, 0f), new Vector3(2f, 0.04f, 3.3f), stoneDark).shadowCastingMode = ShadowCastingMode.Off;
            }
            SunCityKit.Place("sarcophagus", group, new Vector3(-4f, 2.34f, 266f), 0f, 0.8f, false);
            SunCityKit.Place("sarcophagus", group, new Vector3(12f, 2.34f, 258f), 0f, 0.8f, false);
            Deco(PrimitiveType.Capsule, "Wrapped Dead", group, new Vector3(3f, 2.62f, 252f), new Vector3(0.55f, 0.95f, 0.55f), wrap, new Vector3(90f, 0f, 0f));
            foreach (var at in new[] { new Vector3(-16.5f, 2f, 250f), new Vector3(-15f, 2f, 251.6f), new Vector3(16f, 2f, 249.6f), new Vector3(16.8f, 2f, 265f), new Vector3(-17f, 2f, 270.5f) })
            {
                // Natron, heaped white.
                var heap = Deco(PrimitiveType.Sphere, "Natron Heap", group, at, new Vector3(2.4f, 1.7f, 2.4f), wrap);
                heap.gameObject.AddComponent<SphereCollider>();
            }
            SunCityKit.Place("brazier", group, new Vector3(-7.4f, 2f, 284.6f));
            SunCityKit.Place("brazier", group, new Vector3(7.4f, 2f, 284.6f));

            // E2.1: one standing in the street; one in a doorway that steps out when the player passes.
            Spawn(EnemyKind.WickName, 2.5f, 2f, 152f, 180f);
            Spawn(EnemyKind.WickName, 0.9f, 2f, 168f, 90f).MakeDormant(3.5f);
            // E2.2: two abreast, coming west along the row.
            Spawn(EnemyKind.WickName, 12f, 2f, 182.3f, 270f);
            Spawn(EnemyKind.WickName, 18f, 2f, 183f, 270f);
            // E2.3: one among the bundles, one beyond that walks south when the first rises.
            Spawn(EnemyKind.WickName, 23.6f, 2f, 192.6f, 270f).MakeDormant(3.5f);
            Spawn(EnemyKind.WickName, 22.5f, 2f, 208f, 180f);
            // E2.4: three round the oven, and a thirsting.
            Spawn(EnemyKind.WickName, -4f, 2f, 226f, 180f);
            Spawn(EnemyKind.WickName, 6f, 2f, 222f, 180f);
            Spawn(EnemyKind.WickName, 12f, 2f, 228f, 200f);
            Spawn(EnemyKind.ThirstName, 9f, 2f, 218f, 180f);
            // E2.5: two standing in the yard, two lying wrapped on the tables.
            Spawn(EnemyKind.WickName, -8f, 2f, 258f, 180f);
            Spawn(EnemyKind.WickName, 9f, 2f, 268f, 180f);
            Spawn(EnemyKind.WickName, -10f, 2.36f, 276f, 0f).MakeDormant(4f, "wick_lie_loop");
            Spawn(EnemyKind.WickName, 6f, 2.36f, 280f, 0f).MakeDormant(4f, "wick_lie_loop");
        }

        // The side path: three painted chambers sunk west of the embalmers' yard, open to the sky for the camera.
        private static void PaintedTombs()
        {
            var group = BuildKit.Group("Painted Tombs", world);
            Stairs(group, new Vector3(-20f, 2f, 262f), new Vector3(-26f, -1f, 262f), 5f);
            Box("Tomb Stair Cheek South", group, -26f, -20f, 258f, 259.5f, -1f, 2f, brickDark);
            Box("Tomb Stair Cheek North", group, -26f, -20f, 264.5f, 266f, -1f, 2f, brickDark);
            Floor("Tomb Floor", group, -56f, -26f, 258f, 266f, -1f, shade, 1f);
            Box("Tomb Wall North", group, -57.5f, -30f, 266f, 267.5f, -1f, 2.3f, stone);
            Box("Tomb Wall West", group, -57.5f, -56f, 256.5f, 266f, -1f, 2.3f, stone);
            // Low on the camera's side.
            Box("Tomb Wall South", group, -56f, -30f, 256.5f, 258f, -1f, 0.5f, stoneDark);
            foreach (float x in new[] { -46f, -36f })
            {
                Box("Tomb Divider South", group, x - 0.5f, x + 0.5f, 258f, 260.5f, -1f, 0.5f, stoneDark);
                Box("Tomb Divider North", group, x - 0.5f, x + 0.5f, 263.5f, 266f, -1f, 2.3f, stone);
            }
            var wicks = new[]
            {
                Spawn(EnemyKind.WickName, -44.2f, -1f, 259.6f, 90f),
                Spawn(EnemyKind.WickName, -37.8f, -1f, 259.6f, 270f),
            };
            // They do not rise on the way in: only a touch wakes them, and the last painting gives it.
            foreach (var wick in wicks) wick.MakeDormant(0.05f, "wick_lie_loop");

            // The words go with what each panel shows (looked at in the game): the hearth and its people; the star
            // falling; the bearer carrying it to the city; the seated king; masons recutting a statue; the flaked wall.
            Painting(group, "tomb_1_hearth", -33.4f, null,
                "A river in flood, painted blue. People on both banks hold up small lamps. A figure with a lamp for a head walks ahead of the rising sun and lights it from theirs.");
            Painting(group, "tomb_2_star", -28.9f, null,
                "The lamp-headed figure lies on its side. Its lamp is out. Stars are falling. Beside it the plaster is left bare, in the shape of something standing.",
                "\"The morning does not come. We bank our fires and wait. We do not know what we have done.\"");
            Painting(group, "tomb_3_bearer", -43.3f, null,
                "A star lies burning in the desert. A herdsman kneels to it with open hands. He is weeping; his tears are the only blue on this wall.",
                "\"A piece of the day falls where a man can reach it.\"",
                "The herdsman carries the star home on his back, bent double, as a beetle carries its ball. People come out of dark houses.");
            Painting(group, "tomb_4_king", -38.8f, null,
                "Newer paint, and gold leaf. The same man, seated, the star for a crown, a falcon's wings behind him.");
            Painting(group, "tomb_5_recut", -53.4f, null,
                "Masons on a scaffold at a seated figure. They are cutting its face again.");
            Painting(group, "tomb_6_flaked", -48.9f, wicks,
                "Where the gold has flaked, the lamp-headed figure shows under it.", "It was never seated. It is kneeling, and holding out a light.");

            // A grave dish with what was left for the dead.
            Disc("Grave Dish", group, new Vector3(-54.4f, -1f, 259.6f), 0.6f, 0.3f, stoneDark);
            var embers = Deco(PrimitiveType.Sphere, "Grave Embers", group, new Vector3(-54.4f, -0.66f, 259.6f), new Vector3(0.5f, 0.2f, 0.5f), ember);
            var dish = new GameObject("Grave Dish Gift").AddComponent<LandGift>();
            dish.transform.SetParent(group, false);
            dish.transform.position = new Vector3(-53.4f, -1f, 260.2f);
            dish.Prompt = "Take what was left for the dead";
            dish.Range = 2.2f;
            dish.Embers = 3;
            dish.Words = "Three embers";
            dish.Configure(embers.gameObject);
            things.Add(dish);
        }

        private static void Painting(Transform parent, string panel, float x, Enemy[] wakes, params string[] lines)
        {
            var hung = SunCityKit.Panel(panel, parent, new Vector3(x, -0.85f, 265.9f));
            Notice notice;
            if (wakes != null)
            {
                var waking = hung.AddComponent<WakingNotice>();
                waking.Configure(wakes);
                notice = waking;
            }
            else notice = hung.AddComponent<Notice>();
            notice.Speaker = "A painted wall";
            notice.Lines = lines;
            notice.Prompt = "Read";
            notice.Range = 2.3f;
            things.Add(notice);
        }

        // ------------------------------------------------------------------ district 3: the golden way

        private static float AvenueLevel(float z) => z < 342f ? 2f : z < 386f ? 3f : 4f;

        private static void GoldenWay(AreaDirector director)
        {
            var group = BuildKit.Group("Golden Way", world);
            Floor("Avenue Low", group, -22f, 22f, 290f, 342f, 2f, stone, 2.6f);
            Floor("Avenue Middle", group, -22f, 22f, 342f, 386f, 3f, stone, 3.6f);
            Floor("Avenue High", group, -22f, 22f, 386f, 400f, 4f, stone, 4.6f);
            Stairs(group, new Vector3(0f, 2f, 340f), new Vector3(0f, 3f, 342f), 44f);
            Stairs(group, new Vector3(0f, 3f, 384f), new Vector3(0f, 4f, 386f), 44f);
            // The king's road: pale dressed paving, newer and paler down the middle where he walks, two lines of gold let into it.
            Paving(group, -12f, 12f, 290f, 340f, 2f, 3f, newA, newB, paveC, 0.04f);
            Paving(group, -12f, 12f, 342f, 384f, 3f, 3f, newA, newB, paveC, 0.04f);
            Paving(group, -12f, 12f, 386f, 400f, 4f, 3f, newA, newB, paveC, 0.03f);
            Paving(group, -22f, -12f, 386f, 394f, 4f, 3.3f, paveA, paveB, paveC, 0.25f);
            Paving(group, 12f, 22f, 386f, 394f, 4f, 3.3f, paveA, paveB, paveC, 0.25f);
            Paving(group, -22f, -12f, 290f, 340f, 2f, 3.3f, paveA, paveB, paveC, 0.3f);
            Paving(group, 12f, 22f, 290f, 340f, 2f, 3.3f, paveA, paveB, paveC, 0.25f);
            Paving(group, -22f, -12f, 342f, 384f, 3f, 3.3f, paveA, paveB, paveC, 0.3f);
            Paving(group, 12f, 22f, 342f, 384f, 3f, 3.3f, paveA, paveB, paveC, 0.25f);
            foreach (float x in new[] { -6f, 6f })
            {
                GoldLine(group, x - 0.11f, x + 0.11f, 291f, 339.6f, 2f);
                GoldLine(group, x - 0.11f, x + 0.11f, 342.4f, 383.6f, 3f);
                GoldLine(group, x - 0.11f, x + 0.11f, 386.4f, 393f, 4f);
            }
            Box("Avenue Wall West", group, -23.5f, -22f, 290f, 400f, 2f, 10f, stone);
            Box("Avenue Wall East", group, 22f, 23.5f, 290f, 400f, 2f, 10f, stone);
            Drift(group, new Vector3(-19.5f, 2f, 312f), 4f, 9f);
            Drift(group, new Vector3(19.8f, 2f, 331f), 3.6f, 8f);
            Drift(group, new Vector3(-20f, 3f, 371f), 3.4f, 7f);
            Drift(group, new Vector3(20f, 3f, 354f), 3.4f, 6f);

            // Five pairs of the seated king, the heads recut: paler than the bodies under them.
            foreach (float z in new[] { 302f, 322f, 346f, 362f, 380f })
            {
                float y = AvenueLevel(z);
                foreach (float side in new[] { -1f, 1f })
                {
                    float yaw = side < 0f ? 90f : 270f;
                    var at = new Vector3(side * 14.2f, y, z);
                    Floor("Statue Plinth", group, at.x - 3.2f, at.x + 3.2f, z - 3.2f, z + 3.2f, y + 0.3f, stoneDark, 0.3f);
                    SunCityKit.Place("statue_king", group, at + Vector3.up * 0.3f, yaw);
                    bool unfinished = side < 0f && z == 346f;
                    var head = at + Vector3.up * 0.3f + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 8f, 1.1f);
                    if (!unfinished)
                    {
                        SunCityKit.Place("statue_head_new", group, head, yaw, 1f, false);
                        continue;
                    }
                    // The one they had not finished: the old god's face still on it, timber scaffold round it.
                    SunCityKit.Place("statue_head_old", group, head + Vector3.down * 0.5f, yaw, 1.15f, false);
                    foreach (float dz in new[] { -3.6f, 3.6f })
                        foreach (float dx in new[] { 2.2f, 4.4f })
                            Deco(PrimitiveType.Cube, "Scaffold Pole", group, new Vector3(at.x + dx, y + 4.6f, z + dz), new Vector3(0.2f, 9.2f, 0.2f), wood);
                    foreach (float up in new[] { 3.2f, 6.2f, 9f })
                    {
                        Deco(PrimitiveType.Cube, "Scaffold Deck", group, new Vector3(at.x + 3.3f, y + up, z), new Vector3(2.5f, 0.12f, 7.6f), wood);
                        Deco(PrimitiveType.Cube, "Scaffold Rail", group, new Vector3(at.x + 4.4f, y + up + 0.5f, z), new Vector3(0.1f, 0.1f, 7.4f), wood);
                    }
                    var fallen = SunCityKit.Place("statue_head_new", group, new Vector3(at.x + 5.2f, y + 0.2f, z + 8f), 140f, 1f);
                    fallen.transform.rotation = Quaternion.Euler(-78f, 140f, 8f);
                }
            }
            Carving(group, new Vector3(-10.4f, 2f, 305.9f), 90f, "The base of the first statue",
                "\"Khepharu, who is the sun. Who is not born. Who does not set.\"", "Under it, shallow, half ground away: \"...who carries the day up from the river for us.\"");
            Carving(group, new Vector3(-8.6f, 3f, 350.9f), 90f, "Mason's marks on the scaffold", "\"The old face still shows. Cut deeper.\"");
            SunCityKit.Place("brazier", group, new Vector3(-9.5f, 2f, 292.5f));
            SunCityKit.Place("brazier", group, new Vector3(9.5f, 2f, 292.5f));
            Rest(group, director, new Vector3(-7f, 2f, 296f));

            // The pylon before the hall.
            var pylon = SunCityKit.Place("pylon_gate", group, new Vector3(0f, 4f, 396f), 180f, 1.5f, false);
            Solid(pylon, new Vector3(-5.35f, 5.8f, 0f), new Vector3(6.1f, 11.7f, 4.7f));
            Solid(pylon, new Vector3(5.35f, 5.8f, 0f), new Vector3(6.1f, 11.7f, 4.7f));
            Box("Pylon Wing West", group, -22f, -12.3f, 394.4f, 398f, 4f, 13f, stone);
            Box("Pylon Wing East", group, 12.3f, 22f, 394.4f, 398f, 4f, 13f, stone);
            Carving(group, new Vector3(5.2f, 4f, 391.4f), 180f, "Over the door of the hall", "\"Look on him and see.\"", "Below, by another hand: \"They look.\"");

            // E3.1 to E3.3: the gilded in rising ranks.
            Spawn(EnemyKind.GildedLevyName, -5f, 2f, 310f, 180f);
            Spawn(EnemyKind.GildedLevyName, 5f, 2f, 314f, 180f);
            Spawn(EnemyKind.GildedSpearmanName, -6f, 3f, 348f, 180f);
            Spawn(EnemyKind.GildedLevyName, 4f, 3f, 351f, 180f);
            Spawn(EnemyKind.GildedWardenName, 0f, 3f, 376f, 180f);
            Spawn(EnemyKind.GildedLevyName, -5f, 3f, 370f, 180f);
            Spawn(EnemyKind.GildedLevyName, 5f, 3f, 370f, 180f);
        }

        private static void HallOfColumns(AreaDirector director)
        {
            var group = BuildKit.Group("Hall Of Columns", world);
            // The floor is shade, dark and cool; where the roof is open the sun lies on it, pale.
            Floor("Hall Floor", group, -22f, 22f, 400f, 470f, HallFloor, hallFloor, 4.6f);
            foreach (var lit in HallLight)
            {
                var patch = Deco(PrimitiveType.Cube, "Sunlit Floor", group, new Vector3((lit.x + lit.y) * 0.5f, HallFloor + 0.012f, (lit.z + lit.w) * 0.5f), new Vector3(lit.y - lit.x, 0.03f, lit.w - lit.z), stoneNew);
                patch.shadowCastingMode = ShadowCastingMode.Off;
            }
            Paving(group, -22f, 22f, 460.6f, 469.6f, HallFloor, 3f, paveA, paveB, paveC, 0.15f);
            Box("Hall Wall West", group, -23.5f, -22f, 400f, 470f, 4f, 15f, stone);
            Box("Hall Wall East", group, 22f, 23.5f, 400f, 470f, 4f, 15f, stone);
            Box("Hall North Wall West", group, -22f, -4f, 459.6f, 460.6f, 4f, 11f, stone);
            Box("Hall North Wall East", group, 4f, 22f, 459.6f, 460.6f, 4f, 11f, stone);
            foreach (float x in new[] { -15f, -5f, 5f, 15f })
                foreach (float z in new[] { 405f, 415f, 425f, 435f, 445f, 455f })
                {
                    SunCityKit.Place("column_base", group, new Vector3(x, HallFloor, z), 180f, 1f, false);
                    var column = SunCityKit.Place("column_lotus", group, new Vector3(x, HallFloor + 0.5f, z), 180f, 1f, false);
                    // Round the shaft, not the spread of the capital: the aisles stay as wide as they look.
                    Solid(column, new Vector3(0f, 4.6f, 0f), new Vector3(2.1f, 10.2f, 2.1f));
                }

            // The roof is drawn only as its shadow, so the camera sees under it. Its holes and the rule the priests
            // hunt by come from the one list: each patch of light lies north of its hole by the lean of the noon sun.
            float lean = LookBuilder.NoonShadowLength(HallRoof);
            var bars = new List<Vector2>();
            foreach (var lit in HallLight)
                if (lit.y - lit.x > 20f) bars.Add(new Vector2(lit.z - lean, lit.w - lean));
            float south = HallSouth - lean - 4f;
            for (int i = 0; i <= bars.Count; i++)
            {
                float north = i < bars.Count ? bars[i].x : HallNorth - lean;
                foreach (float side in new[] { -1f, 1f })
                {
                    // The nave is open for its whole length.
                    float inner = side * 2f, outer = side * 23f;
                    var roof = BuildKit.Block("Hall Roof (shadow only)", group, new Vector3((inner + outer) * 0.5f, HallFloor + HallRoof + 0.25f, (south + north) * 0.5f),
                        new Vector3(Mathf.Abs(outer - inner), 0.5f, north - south), stone);
                    Object.DestroyImmediate(roof.GetComponent<Fadeable>());
                    Object.DestroyImmediate(roof.GetComponent<Collider>());
                    roof.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }
                if (i < bars.Count) south = bars[i].y;
            }
            new GameObject("Sunlight").AddComponent<SunlightZones>().Configure(false, null, HallLight);

            Rest(group, director, new Vector3(8f, 4f, 464f));

            // E3.4: four who cannot see, and one beside the north door who can.
            Spawn(EnemyKind.SunBlindName, 0f, 4f, 410f, 180f);
            Spawn(EnemyKind.SunBlindName, -10f, 4f, 430f, 90f);
            Spawn(EnemyKind.SunBlindName, -8f, 4f, 450f, 180f);
            Spawn(EnemyKind.SunBlindName, 8f, 4f, 450f, 180f);
            Spawn(EnemyKind.GildedSpearmanName, 7f, 4f, 456.5f, 200f);
        }

        // ------------------------------------------------------------------ the last gate

        private static void LastGate(AreaDirector director)
        {
            var group = BuildKit.Group("Last Gate", world);
            Floor("Gate Court", group, -20.5f, 20.5f, 470f, 504f, 4f, stone, 4.6f);
            Paving(group, -19f, 19f, 470.4f, 500f, 4f, 3f, newA, newB, paveC, 0.04f);
            GoldLine(group, -19f, 19f, ChampionsLine - 0.22f, ChampionsLine + 0.22f, 4f);
            Box("Court Wall West", group, -20.5f, -19f, 470f, 500f, 4f, 10f, stone);
            Box("Court Wall East", group, 19f, 20.5f, 470f, 500f, 4f, 10f, stone);
            // The gate of noon: a wall fourteen metres high, bronze doors, a band of gold over them.
            Box("Gate Wall West", group, -20.5f, -3.2f, 500f, 503.5f, 4f, 18f, stone);
            Box("Gate Wall East", group, 3.2f, 20.5f, 500f, 503.5f, 4f, 18f, stone);
            Box("Gate Lintel", group, -3.2f, 3.2f, 500f, 503.5f, 12.4f, 18f, stone);
            Box("Bronze Doors", group, -3.2f, 3.2f, 499.75f, 500.4f, 4f, 12.4f, bronze);
            Deco(PrimitiveType.Cube, "Door Seam", group, new Vector3(0f, 8.2f, 499.72f), new Vector3(0.1f, 8.4f, 0.06f), shade);
            foreach (float up in new[] { 5.2f, 8.2f, 11.2f })
                Deco(PrimitiveType.Cube, "Door Strap", group, new Vector3(0f, up, 499.7f), new Vector3(6.3f, 0.32f, 0.08f), gold);
            Deco(PrimitiveType.Cube, "Door Band", group, new Vector3(0f, 12.9f, 499.9f), new Vector3(8.4f, 0.7f, 0.3f), gold);
            Deco(PrimitiveType.Cylinder, "Door Sun", group, new Vector3(0f, 14.6f, 499.9f), new Vector3(2.2f, 0.12f, 2.2f), gold, new Vector3(90f, 0f, 0f));
            foreach (float x in new[] { -3.9f, 3.9f })
                Deco(PrimitiveType.Cube, "Door Jamb", group, new Vector3(x, 8.2f, 499.6f), new Vector3(0.9f, 8.4f, 0.5f), stoneNew);
            SunCityKit.Place("brazier", group, new Vector3(-15f, 4f, 497f));
            SunCityKit.Place("brazier", group, new Vector3(15f, 4f, 497f));
            // His fallen predecessors, cast gold, kneeling on low plinths.
            foreach (var at in new[] { new Vector3(-12f, 4f, 478f), new Vector3(12f, 4f, 478f), new Vector3(-12f, 4f, 492f), new Vector3(12f, 4f, 492f) })
            {
                Box("Plinth", group, at.x - 0.9f, at.x + 0.9f, at.z - 0.9f, at.z + 0.9f, 4f, 5f, stoneDark);
                float toward = at.x < 0f ? 1f : -1f;
                Deco(PrimitiveType.Cube, "Cast Legs", group, at + new Vector3(0f, 1.14f, 0f), new Vector3(0.75f, 0.28f, 0.6f), gold);
                Deco(PrimitiveType.Cube, "Cast Body", group, at + new Vector3(toward * 0.12f, 1.62f, 0f), new Vector3(0.4f, 0.75f, 0.6f), gold, new Vector3(0f, 0f, toward * -16f));
                Deco(PrimitiveType.Sphere, "Cast Head", group, at + new Vector3(toward * 0.3f, 2.12f, 0f), new Vector3(0.36f, 0.4f, 0.36f), gold);
            }
            Carving(group, new Vector3(6.4f, 4f, 498.6f), 180f, "The gate of noon", "\"Noon is the king's hour. There is no other hour.\"");

            var gate = BuildKit.Group("Gate Of Noon", group).gameObject.AddComponent<GateOfNoon>();
            gate.transform.position = new Vector3(0f, 4f, 498.8f);
            gate.Prompt = "Open the gate of noon";
            gate.Range = 3f;
            gate.LeadsTo = "SunCourt";
            gate.Configure(director, EnemyKind.ChampionName);
            things.Add(gate);

            // He keeps this ground as the Gate Captain keeps his: he notices nobody south of the gold line and follows nobody over it.
            Spawn(EnemyKind.ChampionName, 0f, 4f, 490f, 180f).HoldGround(-19f, 19f, ChampionsLine + 0.5f, 500f);
        }

        // ------------------------------------------------------------------ pieces

        private static Enemy Spawn(string kind, float x, float y, float z, float yaw)
        {
            var enemy = EnemyBuilder.Build(kind, new Vector3(x, y + 0.05f, z), yaw, kit, enemyRoot);
            enemies.Add(enemy);
            return enemy;
        }

        // A floor between two x and two z with its top at a height. The ground never thins out.
        private static GameObject Floor(string name, Transform parent, float west, float east, float south, float north, float top, Material material, float thickness = 1f)
        {
            var slab = BuildKit.Block(name, parent, new Vector3((west + east) * 0.5f, top - thickness * 0.5f, (south + north) * 0.5f), new Vector3(east - west, thickness, north - south), material);
            Object.DestroyImmediate(slab.GetComponent<Fadeable>());
            return slab;
        }

        // A solid mass between two x, two z and two heights. It thins out when it stands between the camera and the player.
        private static GameObject Box(string name, Transform parent, float west, float east, float south, float north, float bottom, float top, Material material) =>
            BuildKit.Block(name, parent, new Vector3((west + east) * 0.5f, (bottom + top) * 0.5f, (south + north) * 0.5f), new Vector3(east - west, top - bottom, north - south), material);

        // A block of houses: mud brick, a darker parapet round its flat roof, the roof itself dusty.
        private static void Mass(Transform parent, float west, float east, float south, float north, float bottom, float height)
        {
            Box("Houses", parent, west, east, south, north, bottom, bottom + height, brick);
            var centre = new Vector3((west + east) * 0.5f, bottom + height, (south + north) * 0.5f);
            Deco(PrimitiveType.Cube, "Roof Parapet", parent, centre + Vector3.up * 0.14f, new Vector3(east - west + 0.3f, 0.3f, north - south + 0.3f), brickDark);
            Deco(PrimitiveType.Cube, "Roof Dust", parent, centre + Vector3.up * 0.3f, new Vector3(east - west - 1.2f, 0.04f, north - south - 1.2f), sand).shadowCastingMode = ShadowCastingMode.Off;
            // Roofs were lived on: a few low walls and a hatch break up the flat.
            int things = Mathf.Max(1, Mathf.RoundToInt((east - west) * (north - south) / 160f));
            for (int i = 0; i < things; i++)
            {
                float x = Mathf.Lerp(west + 2f, east - 2f, (float)dice.NextDouble()), z = Mathf.Lerp(south + 2f, north - 2f, (float)dice.NextDouble());
                bool along = dice.Next(2) == 0;
                Deco(PrimitiveType.Cube, "Roof Wall", parent, new Vector3(x, bottom + height + 0.6f, z), along ? new Vector3(3.5f, 0.9f, 0.4f) : new Vector3(0.4f, 0.9f, 3.5f), i % 3 == 0 ? brickDark : brick);
            }
        }

        // A primitive that is only looked at: no collider, static.
        private static Material softSilt, softDamp, softDark, softSand;
        private static Mesh[] softShapes;
        private static int softCount;

        // The stains of the land's ground (silt, damp, trodden earth, blown sand) are made of these, not of discs with an edge.
        private static void ReadySoft()
        {
            softSilt = BuildKit.Glow(SunCityKit.Folder, "m_sun_silt_pan", new Color(0.8f, 0.76f, 0.66f, 0.62f), 3, false, 1f);
            softDamp = BuildKit.Glow(SunCityKit.Folder, "m_sun_damp_pan", new Color(0.36f, 0.32f, 0.28f, 0.42f), 3, false, 1f);
            softDark = BuildKit.Glow(SunCityKit.Folder, "m_sun_dark_stain", new Color(0.4f, 0.33f, 0.26f, 0.5f), 3, false, 1f);
            softSand = BuildKit.Glow(SunCityKit.Folder, "m_sun_sand_stain", new Color(0.86f, 0.77f, 0.58f, 0.7f), 3, false, 1f);
            softShapes = new Mesh[5];
            for (int i = 0; i < softShapes.Length; i++) softShapes[i] = PanMesh(i);
            softCount = 0;
        }

        // One stain lying on the ground: half as wide and half as deep as given, turned, in one of five uneven shapes.
        private static void Soft(string name, Transform parent, Vector3 at, float halfWide, float halfDeep, float yaw, Material material)
        {
            if (softShapes == null || softShapes[0] == null) ReadySoft();
            var stain = new GameObject(name);
            stain.transform.SetParent(parent, false);
            stain.transform.position = at;
            stain.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            stain.transform.localScale = new Vector3(halfWide, 1f, halfDeep);
            stain.AddComponent<MeshFilter>().sharedMesh = softShapes[softCount++ % softShapes.Length];
            var shown = stain.AddComponent<MeshRenderer>();
            shown.sharedMaterial = material;
            shown.shadowCastingMode = ShadowCastingMode.Off;
            stain.isStatic = true;
        }

        // A flat uneven patch a metre or so in radius: full colour in its middle, a lobed edge, and nothing at the rim.
        private static Mesh PanMesh(int variant)
        {
            const int Around = 32;
            var random = new System.Random(4100 + variant * 17);
            float a1 = (float)random.NextDouble() * 6.28f, a2 = (float)random.NextDouble() * 6.28f, a3 = (float)random.NextDouble() * 6.28f;
            var vertices = new Vector3[1 + Around * 3];
            var colours = new Color[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[Around * 3 + Around * 12];
            vertices[0] = Vector3.zero;
            colours[0] = Color.white;
            for (int i = 0; i < Around; i++)
            {
                float angle = i * Mathf.PI * 2f / Around;
                float lobe = 1f + 0.22f * Mathf.Sin(angle * 2f + a1) + 0.14f * Mathf.Sin(angle * 3f + a2) + 0.08f * Mathf.Sin(angle * 7f + a3);
                var way = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                vertices[1 + i] = way * (lobe * 0.5f);
                colours[1 + i] = Color.white;
                vertices[1 + Around + i] = way * (lobe * 0.8f);
                colours[1 + Around + i] = new Color(1f, 1f, 1f, 0.7f);
                vertices[1 + Around * 2 + i] = way * (lobe * 1.08f + 0.05f * Mathf.Sin(angle * 11f + a1));
                colours[1 + Around * 2 + i] = new Color(1f, 1f, 1f, 0f);
            }
            for (int i = 0; i < uvs.Length; i++) uvs[i] = new Vector2(0.5f, 0.5f);
            int t = 0;
            for (int i = 0; i < Around; i++)
            {
                int next = (i + 1) % Around;
                triangles[t++] = 0; triangles[t++] = 1 + i; triangles[t++] = 1 + next;
                for (int ring = 0; ring < 2; ring++)
                {
                    int inner = 1 + Around * ring, outer = 1 + Around * (ring + 1);
                    triangles[t++] = inner + i; triangles[t++] = outer + i; triangles[t++] = outer + next;
                    triangles[t++] = inner + i; triangles[t++] = outer + next; triangles[t++] = inner + next;
                }
            }
            var mesh = new Mesh { name = "silt_pan_" + variant };
            mesh.vertices = vertices;
            mesh.colors = colours;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string path = $"{SunCityKit.Folder}/silt_pan_{variant}.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Renderer Deco(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 size, Material material, Vector3? euler = null)
        {
            var renderer = BuildKit.Shape(type, name, parent, position, size, material, euler);
            renderer.gameObject.isStatic = true;
            return renderer;
        }

        // Steps from a foot to a head, in any direction: a ramp to walk on and step blocks to look at.
        private static void Stairs(Transform parent, Vector3 foot, Vector3 head, float width)
        {
            Vector3 along = head - foot;
            float length = along.magnitude;
            Vector3 flat = new Vector3(along.x, 0f, along.z);
            var turn = Quaternion.LookRotation(along.normalized, Vector3.up);
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Stair Ramp";
            ramp.transform.SetParent(parent, false);
            ramp.transform.SetPositionAndRotation((foot + head) * 0.5f + turn * Vector3.down * 0.27f, turn);
            ramp.transform.localScale = new Vector3(width, 0.5f, length + 0.2f);
            ramp.GetComponent<MeshRenderer>().sharedMaterial = brickDark;
            ramp.isStatic = true;
            int steps = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(along.y) / 0.22f), 3, 12);
            var facing = Quaternion.LookRotation(flat.normalized, Vector3.up);
            for (int i = 0; i < steps; i++)
            {
                // Each step's top is at the ramp's height where the step is highest, a hair under, so feet stay on the ramp.
                float low = Mathf.Min(foot.y, head.y);
                float t = (i + 0.5f) / steps;
                float top = Mathf.Lerp(foot.y, head.y, t) - 0.04f;
                var step = Deco(PrimitiveType.Cube, "Step", parent, new Vector3(foot.x + flat.x * t, (low - 0.1f + top) * 0.5f, foot.z + flat.z * t),
                    new Vector3(width, Mathf.Max(0.05f, top - low + 0.1f), flat.magnitude / steps + 0.02f), stone);
                step.transform.rotation = facing;
            }
        }

        private static void Disc(string name, Transform parent, Vector3 foot, float radius, float height, Material material)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            disc.transform.position = foot + Vector3.up * (height * 0.5f);
            disc.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            disc.AddComponent<MeshCollider>().convex = true;
            disc.GetComponent<MeshRenderer>().sharedMaterial = material;
            disc.isStatic = true;
        }

        // Flagstones over a floor: each its own slab, a little apart, in two or three tones, some missing where the
        // ground shows through. What makes a paved place read as paved, and no two stretches alike.
        private static void Paving(Transform parent, float west, float east, float south, float north, float top, float size, Material usual, Material other, Material rare, float missing)
        {
            var group = BuildKit.Group("Paving", parent);
            int across = Mathf.Max(1, Mathf.RoundToInt((east - west) / size)), up = Mathf.Max(1, Mathf.RoundToInt((north - south) / size));
            float wide = (east - west) / across, deep = (north - south) / up;
            for (int z = 0; z < up; z++)
                for (int x = 0; x < across; x++)
                {
                    if (dice.NextDouble() < missing) continue;
                    double pick = dice.NextDouble();
                    // Every other course is shifted half a stone, as laid paving is: it begins with a half stone and ends with one.
                    float from = west + x * wide, to = from + wide;
                    if (z % 2 == 1)
                    {
                        from = x == 0 ? west : from - wide * 0.5f;
                        to = to - wide * 0.5f;
                    }
                    Flag(group, from, to, south + z * deep, south + (z + 1) * deep, top, pick < 0.62 ? usual : pick < 0.9 ? other : rare);
                    if (z % 2 == 1 && x == across - 1) Flag(group, to, east, south + z * deep, south + (z + 1) * deep, top, other);
                }
        }

        private static void Flag(Transform parent, float west, float east, float south, float north, float top, Material material)
        {
            var flag = Deco(PrimitiveType.Cube, "Flag", parent, new Vector3((west + east) * 0.5f, top + 0.004f + (float)dice.NextDouble() * 0.012f, (south + north) * 0.5f),
                new Vector3(east - west - 0.07f, 0.04f, north - south - 0.07f), material);
            flag.shadowCastingMode = ShadowCastingMode.Off;
        }

        // Bare ground is never one colour: darker patches, paler dusty ones, and a few stones.
        private static void Scatter(Transform parent, float west, float east, float south, float north, float top, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var at = new Vector3(Mathf.Lerp(west, east, (float)dice.NextDouble()), top + 0.003f + i * 0.0002f, Mathf.Lerp(south, north, (float)dice.NextDouble()));
                if (i % 4 == 3)
                {
                    float size = 0.14f + (float)dice.NextDouble() * 0.2f;
                    Deco(PrimitiveType.Cube, "Stone", parent, at + Vector3.up * size * 0.3f, new Vector3(size * 1.4f, size, size), brickDark, new Vector3(0f, (float)dice.NextDouble() * 90f, 0f));
                    continue;
                }
                float wide = 1.4f + (float)dice.NextDouble() * 3f, deep = 1f + (float)dice.NextDouble() * 2f;
                Soft("Ground Patch", parent, at + Vector3.up * 0.01f, wide * 0.6f, deep * 0.6f, (float)dice.NextDouble() * 180f, i % 2 == 0 ? softDark : softSand);
            }
        }

        private static void GoldLine(Transform parent, float west, float east, float south, float north, float top)
        {
            var line = Deco(PrimitiveType.Cube, "Gold Line", parent, new Vector3((west + east) * 0.5f, top + 0.03f, (south + north) * 0.5f), new Vector3(east - west, 0.05f, north - south), gold);
            line.shadowCastingMode = ShadowCastingMode.Off;
        }

        // A drift of blown sand: a low mound, wider than it is deep.
        private static void Drift(Transform parent, Vector3 at, float wide, float deep)
        {
            // Blown sand lying thin on the ground: a stain with no edge, and a smaller, stronger one inside it where it lies deepest.
            float yaw = (float)dice.NextDouble() * 50f - 25f;
            Soft("Sand Drift", parent, at + Vector3.up * 0.022f, wide * 0.6f, deep * 0.6f, yaw, softSand);
            Soft("Sand Drift Heart", parent, at + Vector3.up * 0.026f, wide * 0.34f, deep * 0.34f, yaw + 40f, softSand);
        }

        private static void Stain(Transform parent, Vector3 at, float radius)
        {
            if (softShapes == null || softShapes[0] == null) ReadySoft();
            Soft("Dry Stain", parent, at + Vector3.up * 0.02f, radius * 1.05f, radius * 0.9f, (float)dice.NextDouble() * 180f, softDark);
        }

        // The middle of a street, walked darker than its edges.
        private static void Trodden(Transform parent, float fromX, float fromZ, float toX, float toZ, float width)
        {
            var from = new Vector3(fromX, 2.006f, fromZ);
            var to = new Vector3(toX, 2.006f, toZ);
            float length = (to - from).magnitude;
            int pieces = Mathf.Max(1, Mathf.RoundToInt(length / 5f));
            var facing = Quaternion.LookRotation((to - from).normalized, Vector3.up);
            for (int i = 0; i < pieces; i++)
            {
                if (dice.NextDouble() < 0.18) continue;
                if (softShapes == null || softShapes[0] == null) ReadySoft();
                Soft("Trodden Earth", parent, Vector3.Lerp(from, to, (i + 0.5f) / pieces) + facing * new Vector3((float)dice.NextDouble() * 0.8f - 0.4f, 0.012f, 0f),
                    width * (0.8f + (float)dice.NextDouble() * 0.4f) * 0.55f, length / pieces * 0.8f, facing.eulerAngles.y, softDark);
            }
        }

        private static void Doorstep(Transform parent, Vector3 at)
        {
            for (int i = 0; i < 6; i++)
                Deco(PrimitiveType.Cube, "Doorstep Brick", parent, at + new Vector3((i % 2) * 0.62f - 0.3f, 0.012f, (i / 2) * 0.42f - 0.42f), new Vector3(0.56f, 0.03f, 0.36f), i % 3 == 0 ? brick : brickDark).shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void Rubble(Transform parent, float west, float east, float south, float north, float top, int pieces)
        {
            for (int i = 0; i < pieces; i++)
            {
                float size = 0.4f + (float)dice.NextDouble() * 0.7f;
                Deco(PrimitiveType.Cube, "Rubble", parent, new Vector3(Mathf.Lerp(west, east, (float)dice.NextDouble()), top + size * 0.3f, Mathf.Lerp(south, north, (float)dice.NextDouble())),
                    new Vector3(size * 1.5f, size, size), i % 2 == 0 ? brick : brickDark, new Vector3((float)dice.NextDouble() * 20f, (float)dice.NextDouble() * 90f, (float)dice.NextDouble() * 20f));
            }
        }

        private static void Tilt(GameObject piece, float pitch, float roll)
        {
            if (piece == null) return;
            piece.transform.rotation = Quaternion.Euler(pitch, piece.transform.eulerAngles.y, roll);
        }

        // Gives a kit piece that was placed without a collider one box, set out in the piece's own upright frame, and
        // lets it thin out when it stands between the camera and the player.
        private static void Solid(GameObject holder, Vector3 centre, Vector3 size, bool fades = true)
        {
            if (holder == null) return;
            var model = holder.transform.Find("Model");
            if (model == null) return;
            var box = model.gameObject.AddComponent<BoxCollider>();
            box.center = model.InverseTransformPoint(holder.transform.TransformPoint(centre));
            Vector3 local = model.InverseTransformVector(holder.transform.TransformVector(size));
            box.size = new Vector3(Mathf.Abs(local.x), Mathf.Abs(local.y), Mathf.Abs(local.z));
            if (fades && model.GetComponent<Fadeable>() == null && model.GetComponent<Renderer>() != null) model.gameObject.AddComponent<Fadeable>();
        }

        // Words cut in stone, read at the player's own pace: a small pale stele with a gold cap, so it can be found.
        private static void Carving(Transform parent, Vector3 foot, float yaw, string what, params string[] lines)
        {
            var stele = BuildKit.Block("Carving: " + what, parent, foot + Vector3.up * 0.6f, new Vector3(0.9f, 1.2f, 0.26f), stoneNew, yaw);
            Object.DestroyImmediate(stele.GetComponent<Fadeable>());
            var cap = Deco(PrimitiveType.Cube, "Cap", stele.transform, Vector3.zero, Vector3.one, gold);
            cap.transform.localPosition = new Vector3(0f, 0.54f, 0f);
            cap.transform.localScale = new Vector3(1.08f, 0.08f, 1.3f);
            var notice = stele.AddComponent<Notice>();
            notice.Speaker = what;
            notice.Lines = lines;
            notice.Prompt = "Read";
            notice.Range = 2.2f;
            things.Add(notice);
        }

        // A brazier to rest at. Lighting it is resting; it becomes where a fall returns the player to.
        private static void Rest(Transform parent, AreaDirector director, Vector3 foot)
        {
            var brazier = SunCityKit.Place("brazier", parent, foot);
            brazier.name = "Rest Brazier";
            Deco(PrimitiveType.Cube, "Rest Mat", parent, foot + new Vector3(0f, 0.03f, -1.4f), new Vector3(1.5f, 0.04f, 1.1f), reed).shadowCastingMode = ShadowCastingMode.Off;
            Deco(PrimitiveType.Cylinder, "Rest Ring", parent, foot + new Vector3(0f, 0.015f, 0f), new Vector3(2.7f, 0.008f, 2.7f), gold).shadowCastingMode = ShadowCastingMode.Off;
            Deco(PrimitiveType.Cylinder, "Rest Ring Inner", parent, foot + new Vector3(0f, 0.02f, 0f), new Vector3(2.45f, 0.008f, 2.45f), stoneDark).shadowCastingMode = ShadowCastingMode.Off;
            var lightObject = new GameObject("Fire Light");
            lightObject.transform.SetParent(brazier.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            var fire = lightObject.AddComponent<Light>();
            fire.type = LightType.Point;
            fire.color = new Color(1f, 0.55f, 0.2f);
            fire.range = 7f;
            fire.shadows = LightShadows.None;
            var returnPoint = BuildKit.Group("Return Point", brazier.transform);
            returnPoint.position = foot + new Vector3(0f, 0.05f, -1.6f);
            var rest = brazier.AddComponent<RestPoint>();
            rest.Prompt = "Light the fire and rest";
            rest.FireIntensity = 6f;
            rest.Configure(director, returnPoint, fire, null);
            things.Add(rest);
            fires.Add(rest);
        }

        // The soldier who refused the pouring, pinned to the east door by his own spear; the spear is the land's plain weapon.
        private static void PinnedSoldier(Transform parent, Vector3 foot)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Models/Characters/enemy_gilded.fbx");
            if (asset != null)
            {
                var body = Object.Instantiate(asset, parent);
                body.name = "Pinned Soldier";
                body.transform.SetPositionAndRotation(foot + new Vector3(0f, 0.25f, 0f), Quaternion.Euler(0f, 270f, 0f));
                SunCityKit.Dress(body, 1);
            }
            var shown = BuildKit.Group("Pinned Spear", parent);
            shown.position = foot + new Vector3(-0.9f, 1.55f, 0f);
            shown.rotation = Quaternion.Euler(0f, 90f, -12f);
            Deco(PrimitiveType.Cube, "Shaft", shown, Vector3.zero, Vector3.one, wood);
            var shaft = shown.GetChild(0);
            shaft.localPosition = Vector3.zero;
            shaft.localRotation = Quaternion.identity;
            shaft.localScale = new Vector3(0.06f, 0.06f, 2.7f);
            var pickup = new GameObject("Spear Pickup").AddComponent<WeaponPickup>();
            pickup.transform.SetParent(parent, false);
            pickup.transform.position = foot + new Vector3(-1.2f, 0f, 0f);
            pickup.Prompt = "Pull the spear free";
            pickup.Range = 2.2f;
            pickup.Configure(Weapons.GreatSpearName, shown.gameObject);
            things.Add(pickup);
        }
    }
}
