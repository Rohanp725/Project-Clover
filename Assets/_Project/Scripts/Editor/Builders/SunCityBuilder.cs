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
    /// The sun-king's city, second pass (docs/design/fire_levels.md): a land that climbs out of a dead river, up through
    /// a city of wicks, up the king's terraces to his hall and the champion's sunk bowl before the gate of noon. The
    /// ground is made meshes with broken edges and faces that lean (<see cref="SunCityGround"/>), every rise faces south
    /// toward the camera, and the rooms are placed for the second pass's enemies and props
    /// (<see cref="FireMechanicsKit"/>). Five rest fires; a fall returns the player to the last one lit
    /// (<see cref="LandReturn"/>). Not in the list of scenes the game is built from.
    /// </summary>
    public static class SunCityBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/SunCity.unity";

        // The hall of columns: the roof stands this far over its floor, and is open over these strips of floor.
        public const float HallFloor = 16f, HallRoof = 10.5f, HallWest = -22f, HallEast = 22f, HallSouth = 418f, HallNorth = 478f, AisleTop = 17.2f;
        /// <summary>Where the sun reaches the hall's floor (west, east, south, north): three bars across it and the nave.</summary>
        public static readonly Vector4[] HallLight =
        {
            new Vector4(-22f, 22f, 426f, 430f), new Vector4(-22f, 22f, 446f, 450f), new Vector4(-22f, 22f, 466f, 470f), new Vector4(-2f, 2f, 418f, 478f),
        };
        /// <summary>The gold line the champion keeps: the top step down into his bowl.</summary>
        public const float ChampionsLine = 488f;
        public const float BowlFloor = 14.5f, Rim = 16f;

        // The heights of the land, south to north (fire_levels.md, section 2).
        public const float Bed = 0f, Bank = 3f, Quay = 4f, Square = 6f, Terrace = 8f, Gallery = 10f, Avenue = 10f, Masons = 13f, Upper = 16f;

        // The progress keys of the four shortcuts and the hearth wall: once open, open for good.
        public const string LadderKey = "suncity.s1.ladder", MarketDoorKey = "suncity.s2.door", MasonsGateKey = "suncity.s3.gate", SunDoorKey = "suncity.s4.door", HearthKey = "suncity.hearth";

        private static Transform world;
        private static readonly List<Interactable> things = new List<Interactable>();
        private static readonly List<Enemy> enemies = new List<Enemy>();
        private static readonly List<RestPoint> fires = new List<RestPoint>();
        private static readonly List<Vector4> shadeList = new List<Vector4>();
        private static readonly List<(Enemy enemy, float within)> buried = new List<(Enemy, float)>();
        private static readonly List<Enemy> burning = new List<Enemy>();
        private static readonly List<Enemy[]> walls = new List<Enemy[]>();
        private static readonly List<(Transform parent, Vector3 at)> jars = new List<(Transform, Vector3)>();
        private static Transform playerTransform;
        private static EnemyBuilder.Kit kit;
        private static Transform enemyRoot;
        private static System.Random dice;
        private static Material mud, silt, sand, earth, earthDark, brick, brickDark, stone, stoneNew, stoneDark, shade, hallFloor, paveA, paveB, paveC, newA, newB, gold, ember, wood, reed, cloth, blue, clay, bronze, wrap, char_, bankFace, bankBand, ashlar, ashlarBand, goldVein;

        [MenuItem("Clover/Build/Sun City")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the sun city")) return;
            things.Clear();
            enemies.Clear();
            fires.Clear();
            shadeList.Clear();
            buried.Clear();
            burning.Clear();
            walls.Clear();
            jars.Clear();
            dice = new System.Random(1117);
            SunCityGround.Seed(5150);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Materials();
            ReadySoft();
            world = BuildKit.Group("World");

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, 4f), "");
            if (built.Player == null) return;
            playerTransform = built.Player.transform;
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
            FishersBank(director);
            CityOfWicks(director);
            PaintedTombs();
            GoldenWay(director);
            HallOfColumns(director);
            LastGate(director);

            var strikeOutline = WorldBars.Marker("Storm Mark Outline", director.transform, 0f, kit.Marker);
            var strikeFill = WorldBars.Marker("Storm Mark Fill", director.transform, 0f, kit.Marker);
            director.Configure(built.Player.GetComponent<PlayerController>(), enemies.ToArray(), start, strikeOutline, strikeFill);
            kit.Turns.Configure(enemies.ToArray());

            // The second pass: each kind's trick, then the ones placed one by one, the props, the gold floor and the noon.
            FireMechanicsKit.Equip(enemies, kit);
            foreach (var (enemy, within) in buried) FireMechanicsKit.Bury(enemy, within);
            foreach (var wick in burning) FireMechanicsKit.Wick(wick, true);
            for (int i = 0; i < walls.Count; i++) FireMechanicsKit.WallOfShields("Shield Wall " + (i + 1), world, walls[i]);
            foreach (var (parent, at) in jars) FireMechanicsKit.Jar(parent, at, director);
            FireMechanicsKit.Floor(world, built.Player, enemies);
            var landHud = FireMechanicsKit.Hud();
            // The hall, the porch and the champion's bowl never swell: under a roof, and the elite's own ground.
            var calm = new[] { new Vector4(-34f, 26f, HallSouth, 530f) };
            FireMechanicsKit.Heat(director.gameObject, built.Player, HeatSchedule.Never, calm, enemies, landHud);
            // Death returns to the last rest fire: no run end here. The land's own terms are on this component.
            var land = director.gameObject.AddComponent<LandReturn>();
            land.Configure(director, built.Player.GetComponent<PlayerDefence>());
            land.ConfigureHud(landHud);
            new GameObject("Sunlight").AddComponent<SunlightZones>().Configure(true, shadeList.ToArray(), null);
            Zones(director, built.Camera.GetComponent<TopDownCamera>());
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

        /// <summary>Every shade island of the land, as registered for the sun-blind and the swell (filled as the land is built).</summary>
        public static Vector4[] Shade => shadeList.ToArray();

        // The places that change something while stood in: vistas, the districts' rhythms of noon, the forced first swells.
        private static void Zones(AreaDirector director, TopDownCamera view)
        {
            var zones = new GameObject("Land Zones").AddComponent<LandZones>();
            zones.Configure(playerTransform, view, director);
            zones.SetVistas(new[]
            {
                new Vector4(-5f, 5f, 13f, 17f), new Vector4(-4f, 4f, 116.5f, 119.5f), new Vector4(-4f, 4f, 318f, 322f),
                new Vector4(-4f, 4f, 346.5f, 349.5f), new Vector4(-7f, 7f, 484.5f, 487.5f),
            }, new[] { new Vector2(-1f, 1.5f), new Vector2(3.5f, 5.5f), new Vector2(9.5f, 11.5f), new Vector2(12.5f, 14.5f), new Vector2(15.5f, 17.5f) });
            zones.SetDistricts(new[]
            {
                new Vector4(-90f, 60f, -30f, 134f), new Vector4(-90f, 60f, 134f, 272f), new Vector4(-90f, 60f, 272f, HallSouth), new Vector4(-90f, 60f, HallSouth, 640f),
            }, new[] { 100f, 70f, 50f, 0f });
            zones.SetForced(new[] { new Vector4(-30f, 12f, 30f, 33f), new Vector4(-12f, 12f, 320f, 330f) }, new[] { new Vector2(-1f, 2f), new Vector2(9f, 11.5f) });
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
            // The faces of the land: dried mud in strata on the river's banks, dressed stone in courses on the king's terraces.
            bankFace = SunCityKit.Flat("m_sun_bank_face", new Color(0.47f, 0.4f, 0.33f));
            bankBand = SunCityKit.Flat("m_sun_bank_band", new Color(0.41f, 0.35f, 0.3f));
            ashlar = SunCityKit.Flat("m_sun_ashlar", new Color(0.6f, 0.56f, 0.52f));
            ashlarBand = SunCityKit.Flat("m_sun_ashlar_band", new Color(0.54f, 0.5f, 0.48f));
            goldVein = SunCityKit.Flat("m_sun_gold_vein", new Color(0.62f, 0.55f, 0.42f));
            char_ = SunCityKit.Flat("m_sun_char", new Color(0.18f, 0.16f, 0.15f));
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

        // A plain of dust under everything, so nothing ends in a void; beyond the gate the ground the court stands on.
        private static void Beyond()
        {
            var group = BuildKit.Group("Beyond", world);
            var plain = Deco(PrimitiveType.Cube, "Plain", group, new Vector3(0f, -1.3f, 280f), new Vector3(320f, 1f, 720f), sand);
            plain.shadowCastingMode = ShadowCastingMode.Off;
            SunCityGround.Plateau("Beyond The Gate", group, SunCityGround.Rect(-60f, 60f, 526f, 620f), Upper, -1f, sand, ashlar, null, 0f);
            // Over the last gate: the obelisk of the court beyond, close now, its cap the brightest thing in the land.
            SunCityKit.Place("obelisk", group, new Vector3(0f, Upper, 542f), 180f, 1.5f, false);
        }

        // ------------------------------------------------------------------ district 1: the dry river

        // The bed's edges (fire_levels.md 3.1): west from the mouth north, then east from the quay south.
        private static readonly Vector2[] BedWest =
        {
            new Vector2(-5f, -6f), new Vector2(-5f, 16f), new Vector2(-12f, 22f), new Vector2(-18f, 30f), new Vector2(-22f, 46f), new Vector2(-20f, 58f),
            new Vector2(-16f, 64f), new Vector2(-16f, 72f), new Vector2(-10f, 80f), new Vector2(-6f, 96f), new Vector2(-26f, 100f), new Vector2(-26f, 116f),
        };
        private static readonly Vector2[] BedEast =
        {
            new Vector2(24f, 116f), new Vector2(24f, 100f), new Vector2(20f, 96f), new Vector2(16f, 80f), new Vector2(4f, 72f), new Vector2(0f, 64f),
            new Vector2(0f, 58f), new Vector2(4f, 46f), new Vector2(7f, 30f), new Vector2(6f, 22f), new Vector2(5f, 16f), new Vector2(5f, -6f),
        };

        private static bool InBed(Vector2 p)
        {
            var ring = new List<Vector2>(BedWest);
            ring.AddRange(BedEast);
            bool inside = false;
            for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
                if ((ring[i].y > p.y) != (ring[j].y > p.y) && p.x < (ring[j].x - ring[i].x) * (p.y - ring[i].y) / (ring[j].y - ring[i].y) + ring[i].x) inside = !inside;
            return inside;
        }

        private static void DryRiver(AreaDirector director)
        {
            var group = BuildKit.Group("Dry River", world);
            // Stood on: one slab at the bed's level under the whole district. Seen: the kit's cracked plates where the bed is.
            var bed = Floor("Riverbed", group, -80f, 50f, -12f, 118f, Bed, mud, 2f);
            bed.GetComponent<Renderer>().enabled = false;
            Deco(PrimitiveType.Cube, "Riverbed Under", group, new Vector3(0f, -0.65f, 55f), new Vector3(60f, 1f, 124f), mud).shadowCastingMode = ShadowCastingMode.Off;
            var plates = BuildKit.Group("Mud", group);
            for (int z = -2; z < 29; z++)
                for (int x = -7; x < 7; x++)
                {
                    var at = new Vector3(x * 4f + 2f, 0f, z * 4f + 2f);
                    // Plates only in the bed, and not in the basin's paved strip under the quay.
                    if (!InBed(new Vector2(at.x, at.z)) && !InBed(new Vector2(at.x + 1.6f, at.z)) && !InBed(new Vector2(at.x - 1.6f, at.z))) continue;
                    if (at.z > 108f) continue;
                    SunCityKit.Place("mud_tile", plates, at + Vector3.up * -0.19f, dice.Next(4) * 90f, 1f, false);
                }
            // Pans where the last water stood: pale silt lying in the mud.
            for (int i = 0; i < 40; i++)
            {
                float z = 2f + i * 2.7f + (float)dice.NextDouble() * 2f;
                var at = new Vector3(Mathf.Lerp(-24f, 22f, (float)dice.NextDouble()), 0.016f + i * 0.0004f, z);
                if (!InBed(new Vector2(at.x, at.z))) continue;
                Soft("Silt Pan", plates, at, 2.2f + (float)dice.NextDouble() * 2.6f, 1.5f + (float)dice.NextDouble() * 1.8f, (float)dice.NextDouble() * 360f, i % 3 == 0 ? softDamp : softSilt);
            }

            // The banks: 3 m of dried mud in strata leaning back, their edges wandering. The west bank is the fishers' walk.
            var west = new List<Vector2> { new Vector2(-40f, -6f) };
            west.AddRange(BedWest);
            west.Add(new Vector2(-40f, 116f));
            SunCityGround.Plateau("Bank West", group, west.ToArray(), Bank, -1f, earth, bankFace, bankBand, 0.5f, 12f, 0.75f);
            var eastRing = new List<Vector2> { new Vector2(40f, 116f) };
            eastRing.AddRange(BedEast);
            eastRing.Add(new Vector2(40f, -6f));
            SunCityGround.Plateau("Bank East", group, eastRing.ToArray(), Bank, -1f, earth, bankFace, bankBand, 0.5f, 12f, 0.75f);
            Scatter(group, -38f, -26f, -4f, 112f, Bank, 46);
            Scatter(group, -24f, -8f, -4f, 20f, Bank, 10);
            Scatter(group, 20f, 38f, -4f, 112f, Bank, 40);
            Scatter(group, 8f, 20f, -4f, 70f, Bank, 16);
            for (int i = 0; i < 14; i++)
            {
                // Sand blown up over the banks' tops, thickest against the outer walls.
                float z = -2f + i * 8.4f;
                Drift(group, new Vector3(-37f + (float)dice.NextDouble() * 4f, Bank, z), 5f + (float)dice.NextDouble() * 3f, 3f);
                Drift(group, new Vector3(35f - (float)dice.NextDouble() * 4f, Bank, z + 4f), 5f + (float)dice.NextDouble() * 3f, 3f);
            }
            // The levee's broken fence of mud brick along the east bank's lip, and the reed bundles the fishers left.
            foreach (var (x, z, yaw) in new[] { (8.6f, 26f, 4f), (9.6f, 40f, -12f), (5.8f, 52f, 70f), (9f, 64f, 40f), (20f, 86f, -14f), (24.8f, 98f, 60f) })
                Box("Levee Wall", group, x - 0.3f, x + 0.3f, z - 2.5f, z + 2.5f, Bank, Bank + 0.6f + (float)dice.NextDouble() * 0.6f, brickDark);
            foreach (var at in new[] { new Vector3(-16f, Bank, 14f), new Vector3(-24f, Bank, 25f), new Vector3(14f, Bank, 12f), new Vector3(-30f, Bank, 66f), new Vector3(-12f, Bank, 90f) })
                Deco(PrimitiveType.Cylinder, "Reed Bundle", group, at + new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 1.6f, 0.6f), reed, new Vector3(90f, (float)dice.NextDouble() * 180f, 0f));
            // Slumps of fallen mud at the banks' feet, and loose brick on their lips.
            Slumps(group, BedWest, 1f);
            Slumps(group, BedEast, -1f);
            Box("River Mouth Wall", group, -5f, 5f, -7f, -6f, -0.5f, 1.2f, brickDark);
            Box("Outer Wall East", group, 38f, 40f, -6f, 134f, Bank, 7f, brick);
            Rest(group, director, new Vector3(-2.8f, Bed, 9f));
            Canopy(group, "Fishers' Awning", -5f, -0.5f, 6.5f, 11.5f, Bed, 2.6f, reed);
            Tilt(SunCityKit.Place("reed_boat", group, new Vector3(3.2f, 0f, 14f), 200f), 0f, 14f);
            // The ladder lying on the bank top over the Narrows: kicked down, the fishers' walk is a way home (S1).
            Ladder(group, "Fishers' Ladder", new Vector3(-3.3f, Bed, 13f), new Vector3(-6.2f, Bank, 13f), LadderKey);

            // 1B, the Pans: wide and open, mounds in the mud, a jar on the walking line, the first swell.
            Stain(group, new Vector3(-3f, 0f, 28.4f), 2.6f);
            Spawn(EnemyKind.ThirstName, -3f, Bed, 27f, 0f).MakeDormant(3f, "thirst_kneel_loop");
            Bury(Spawn(EnemyKind.ThirstName, -12f, Bed, 43f, 120f));
            Bury(Spawn(EnemyKind.ThirstName, -5f, Bed, 48f, 220f));
            jars.Add((group, new Vector3(-8f, Bed, 45f)));
            Canopy(group, "Drying Rack", -8f, -4f, 23.6f, 25.6f, Bed, 2.2f, reed);
            Canopy(group, "Drying Rack", -6f, -2f, 51.6f, 53.6f, Bed, 2.2f, reed);
            // The old boat on trestles: the shade the first swell sends the player to.
            Box("Trestle", group, -15.5f, -15f, 38.5f, 40.5f, Bed, 1.6f, wood);
            Box("Trestle", group, -12f, -11.5f, 38.5f, 40.5f, Bed, 1.6f, wood);
            var boat = SunCityKit.Place("reed_boat", group, new Vector3(-13.5f, 1.6f, 39.5f), 90f, 1.1f, false);
            Tilt(boat, 0f, 4f);
            AddShade(-16f, -11f, 38f, 41.6f);

            // 1C, the Jam: boats and mud piled across the bed, a gap of 4 m at its west, two waiting behind.
            Box("Jam Heap West", group, -17f, -6f, 66f, 70f, Bed, 1.4f, earthDark);
            Box("Jam Heap East", group, -2f, 6f, 66f, 70f, Bed, 1.4f, earthDark);
            foreach (var (x, yaw, roll) in new[] { (-14f, 70f, 28f), (-10.5f, 115f, -22f), (2f, 160f, 33f) })
                Tilt(SunCityKit.Place("reed_boat", group, new Vector3(x, 1.2f, 68f), yaw, 1f, false), 0f, roll);
            Deco(PrimitiveType.Cylinder, "Broken Mast", group, new Vector3(-1f, 0.3f, 68f), new Vector3(0.35f, 2.3f, 0.35f), wood, new Vector3(90f, 26f, 0f));
            Rubble(group, -16f, -7f, 65.5f, 70.5f, 1.2f, 10);
            Rubble(group, -1.5f, 5f, 65.5f, 70.5f, 1.2f, 6);
            var jar = Deco(PrimitiveType.Cylinder, "Great Jar", group, new Vector3(-8.6f, 1f, 73.4f), new Vector3(2f, 1.5f, 2f), clay, new Vector3(0f, 0f, 84f));
            jar.gameObject.AddComponent<BoxCollider>();
            Stain(group, new Vector3(-6.6f, 0f, 73.4f), 2f);
            Canopy(group, "Lean-to", -15f, -8f, 70f, 72.4f, Bed, 2.2f, reed);
            Spawn(EnemyKind.ThirstName, -10f, Bed, 71.5f, 160f);
            Spawn(EnemyKind.ThirstName, -1f, Bed, 73f, 200f);

            // 1D, the Barque's Bend: the boat of the morning stranded down the middle, its deck a high branch. Scenery only: the
            // land's way out is the sunset in the sun-court, not this boat (WORLD.md, rides out of each land).
            var barque = SunCityKit.Place("barque", group, new Vector3(4f, 0f, 88f), 200f, 0.73f, false);
            Solid(barque, new Vector3(0.5f, 2.2f, -0.3f), new Vector3(6.4f, 4.4f, 29f), false);
            var deck = BuildKit.Block("Barque Deck", group, new Vector3(4f, 3.35f, 88f), new Vector3(4.4f, 0.3f, 20f), wood, 200f);
            Object.DestroyImmediate(deck.GetComponent<Fadeable>());
            var rail = BuildKit.Block("Deck Rail", group, new Vector3(4f, 0f, 88f) + Quaternion.Euler(0f, 200f, 0f) * new Vector3(-2.1f, 3.8f, 0f), new Vector3(0.15f, 0.6f, 18f), wood, 200f);
            Object.DestroyImmediate(rail.GetComponent<Fadeable>());
            SunCityGround.Ramp("Gangplank", group, new Vector3(0.2f, Bed, 72.4f), new Vector3(0.9f, 3.5f, 79f), 3f, reed, wood);
            Canopy(group, "Deck Cabin", 2.6f, 5.8f, 86f, 89.5f, 3.5f, 2.2f, cloth);
            Carving(group, new Vector3(6.6f, 3.5f, 96.4f), 200f, "The barque's prow", "\"The boat of the morning. It stopped when the river did.\"");
            Spawn(EnemyKind.ThirstName, 3.4f, 3.5f, 90f, 250f);
            Bury(Spawn(EnemyKind.ThirstName, -6f, Bed, 86f, 160f));
            jars.Add((group, new Vector3(-8f, Bed, 84f)));
            Spawn(EnemyKind.ThirstName, 11f, Bed, 97f, 200f);
            // A sail lying over two spars in the west channel.
            Box("Spar", group, -9.2f, -8.9f, 92.5f, 94.5f, Bed, 1.8f, wood);
            Box("Spar", group, -5.3f, -5f, 92.5f, 94.5f, Bed, 1.8f, wood);
            Canopy(group, "Sail", -9.5f, -4.7f, 92f, 95f, Bed, 1.9f, cloth, false);
            Tilt(SunCityKit.Place("reed_boat", group, new Vector3(-12f, 0f, 58f), 30f), 0f, -8f);

            // 1E, the Harbour Basin: squared, paved near its face, a dry cistern, jars, four boat-sheds in the quay's face.
            Paving(group, -26f, 24f, 108f, 116f, Bed, 2.4f, paveA, paveB, earthDark, 0.35f);
            Disc("Dry Cistern", group, new Vector3(0f, 0f, 104f), 1.6f, 0.8f, stone);
            Deco(PrimitiveType.Cylinder, "Cistern Dark", group, new Vector3(0f, 0.81f, 104f), new Vector3(2.5f, 0.01f, 2.5f), shade);
            jars.Add((group, new Vector3(-18f, Bed, 104f)));
            jars.Add((group, new Vector3(-16.6f, Bed, 105.4f)));
            jars.Add((group, new Vector3(-19.4f, Bed, 106f)));
            Spawn(EnemyKind.ThirstName, -5f, Bed, 104f, 180f);
            Spawn(EnemyKind.ThirstName, 5f, Bed, 106f, 180f);
            Bury(Spawn(EnemyKind.ThirstName, -16f, Bed, 111f, 160f));
            // The ramp up the basin's west wall onto the fishers' bank.
            SunCityGround.Ramp("Bank Ramp", group, new Vector3(-23.5f, Bed, 112f), new Vector3(-23.5f, Bank, 98.8f), 4f, earth, brickDark);

            // 1F, the quay, its face cut with four boat-sheds; its stair up the middle.
            var quay = new List<Vector2> { new Vector2(-40f, 116f) };
            foreach (float x in new[] { -18f, -8f, 8f, 18f })
            {
                quay.Add(new Vector2(x - 2f, 116f));
                quay.Add(new Vector2(x - 2f, 119f));
                quay.Add(new Vector2(x + 2f, 119f));
                quay.Add(new Vector2(x + 2f, 116f));
            }
            quay.Add(new Vector2(40f, 116f));
            quay.Add(new Vector2(40f, 134f));
            quay.Add(new Vector2(-40f, 134f));
            SunCityGround.Plateau("Quay", group, quay.ToArray(), Quay, -1f, stone, ashlar, ashlarBand, 0f, 0f, 1f);
            Paving(group, -38f, 38f, 116f, 134f, Quay, 2.5f, paveA, paveB, paveC, 0.22f);
            Cornice(group, -26f, -20f, 116f, Quay, Bed);
            Cornice(group, -16f, -10f, 116f, Quay, Bed);
            Cornice(group, -6f, -3.2f, 116f, Quay, Bed);
            Cornice(group, 3.2f, 6f, 116f, Quay, Bed);
            Cornice(group, 10f, 16f, 116f, Quay, Bed);
            Cornice(group, 20f, 24f, 116f, Quay, Bed);
            foreach (float x in new[] { -18f, -8f, 8f, 18f })
            {
                // The shed's ceiling is the quay walked over above it; it thins out for a player inside.
                Box("Boat-shed Ceiling", group, x - 2f, x + 2f, 116f, 119f, 3f, Quay, stone);
                Deco(PrimitiveType.Cube, "Boat-shed Dark", group, new Vector3(x, 1.4f, 118.9f), new Vector3(3.9f, 2.8f, 0.1f), shade);
                AddShade(x - 2f, x + 2f, 116f, 119f);
            }
            Stairs(group, new Vector3(0f, Bed, 108f), new Vector3(0f, Quay, 116f), 6f);
            for (float x = -36f; x <= 36f; x += 6f)
            {
                if (Mathf.Abs(x) < 5f) continue;
                Box("Mooring Stone", group, x - 0.4f, x + 0.4f, 116.4f, 117.2f, Quay, 4.9f, stoneDark);
            }
            Carving(group, new Vector3(7f, Quay, 118.6f), 180f, "A mooring stone",
                "\"Here the boats of both banks tie up. High water or low, the river gives.\"", "Scratched under it, later: \"The river is given to the sun. Drink light.\"");
            // A timber crane that lifted cargo out of boats.
            Box("Crane Post", group, 17.7f, 18.3f, 120.6f, 121.2f, Quay, 8.5f, wood);
            Deco(PrimitiveType.Cube, "Crane Arm", group, new Vector3(18f, 8.4f, 118.6f), new Vector3(0.35f, 0.35f, 6.5f), wood, new Vector3(-14f, 0f, 0f));
            Deco(PrimitiveType.Cube, "Crane Rope", group, new Vector3(18f, 6.6f, 115.9f), new Vector3(0.06f, 2.4f, 0.06f), reed);
            Rest(group, director, new Vector3(-22f, Quay, 127f));
            Canopy(group, "Harbour-master's Awning", -25f, -19f, 124.6f, 129.6f, Quay, 2.6f, cloth);
            Drift(group, new Vector3(-34f, Quay, 131f), 8f, 3f);
            Drift(group, new Vector3(28f, Quay, 131f), 9f, 2.6f);
            // The cargo that never left: crates, bales and jars stacked on the quay, a shade among them.
            foreach (var (x, z, count) in new[] { (-12f, 125f, 5), (11f, 127f, 6), (27f, 122f, 4), (-33f, 121f, 3) })
                Cargo(group, new Vector3(x, Quay, z), count);
            Canopy(group, "Cargo Awning", 9f, 14f, 124.6f, 128.6f, Quay, 2.8f, reed);
            Paving(group, -2f, 3f, 116f, 134f, Quay, 1.6f, newB, paveA, paveC, 0.05f);
        }

        // Mud slumped off a bank's face lying at its foot, every 10 to 15 m, and loose brick on the lip above.
        private static void Slumps(Transform parent, Vector2[] edge, float outward)
        {
            for (int i = 0; i + 1 < edge.Length; i++)
            {
                Vector2 p = edge[i], q = edge[i + 1];
                float length = (q - p).magnitude;
                if (length < 6f) continue;
                Vector2 along = (q - p) / length;
                // The bed lies to the east of the west bank's edge, to the west of the east bank's.
                Vector2 into = new Vector2(along.y, -along.x) * outward;
                for (float s = 4f + (float)dice.NextDouble() * 4f; s < length - 3f; s += 10f + (float)dice.NextDouble() * 5f)
                {
                    Vector2 at = p + along * s + into * 1.6f;
                    float yaw = Mathf.Atan2(along.x, along.y) * Mathf.Rad2Deg;
                    var slump = Deco(PrimitiveType.Sphere, "Mud Slump", parent, new Vector3(at.x, 0.05f, at.y), new Vector3(1.6f, 1f, 2.4f), bankFace, new Vector3(0f, yaw, 0f));
                    slump.shadowCastingMode = ShadowCastingMode.Off;
                    Vector2 lip = p + along * (s + 3f) - into * 0.6f;
                    Deco(PrimitiveType.Cube, "Loose Brick", parent, new Vector3(lip.x, Bank + 0.15f, lip.y), new Vector3(0.7f, 0.3f, 0.45f), brickDark, new Vector3(4f, yaw + 20f, 3f));
                }
            }
        }

        // The side path: the fishers' walk on the west bank, its huts and racks, and the sunk court with the well the king closed.
        private static void FishersBank(AreaDirector director)
        {
            var group = BuildKit.Group("Fishers' Bank", world);
            Box("Bank Outer Wall South", group, -42f, -40f, -6f, 74f, Bank, 6f, brick);
            Box("Bank Outer Wall North", group, -42f, -40f, 80f, 116f, Bank, 6f, brick);
            foreach (float z in new[] { 27f, 39f, 51f })
            {
                SunCityKit.Place("house_block", group, new Vector3(-34f, Bank, z), 90f);
                AddShade(-35f, -30f, z - 1f, z + 1.2f);
            }
            Canopy(group, "Net Rack", -27f, -24f, 33f, 34.4f, Bank, 2f, reed, false);
            Canopy(group, "Net Rack", -29f, -26f, 44f, 45.4f, Bank, 2f, reed, false);
            Trodden(group, -27f, 0f, -27f, 98f, 2f, Bank);
            Bury(Spawn(EnemyKind.ThirstName, -24f, Bank, 60f, 120f));
            jars.Add((group, new Vector3(-26.5f, Bank, 61.8f)));

            // The gully west through the outer wall and the stair down into the well's sunk court.
            Stairs(group, new Vector3(-40f, Bank, 77f), new Vector3(-46f, 1f, 77f), 6f);
            Box("Gully Cheek South", group, -46f, -40f, 64f, 74f, -1f, Bank, brickDark);
            Box("Gully Cheek North", group, -46f, -40f, 80f, 90f, -1f, Bank, brickDark);
            Floor("Well Court", group, -66f, -40f, 64f, 90f, 1f, earth, 2.4f);
            Paving(group, -62f, -50f, 72f, 86f, 1f, 2f, paveA, paveB, paveC, 0.3f);
            Scatter(group, -65f, -47f, 65f, 89f, 1f, 24);
            Box("Well Wall North", group, -68f, -40f, 90f, 92f, 1f, 4f, brick);
            Box("Well Wall West", group, -68f, -66f, 62f, 92f, 1f, 4f, brick);
            // Low on the camera's side.
            Box("Well Wall South", group, -66f, -46f, 62.6f, 64f, 1f, 2.4f, brickDark);
            SunCityKit.Place("well_shrine", group, new Vector3(-56f, 1f, 80f));
            Drift(group, new Vector3(-64f, 1f, 87f), 5f, 4f);
            Carving(group, new Vector3(-52.2f, 1f, 76.6f), 200f, "The well's lid",
                "On the gold bands: \"Closed by the king's word. Water is the old debt. The sun pays every debt.\"",
                "Under the rim, older, small: \"Keeper of the hearth, opener of the day: the first cup is yours.\"");
            // An offering table under a reed canopy, a clay bowl, and in the bowl a full waterskin.
            Box("Offering Table", group, -56.8f, -55.2f, 73.4f, 74.4f, 1f, 1.8f, stone);
            Canopy(group, "Offering Canopy", -58f, -54f, 72f, 74.6f, 1f, 2.4f, reed);
            Deco(PrimitiveType.Cylinder, "Bowl", group, new Vector3(-56f, 1.86f, 73.9f), new Vector3(0.8f, 0.07f, 0.8f), clay);
            var skin = Deco(PrimitiveType.Sphere, "Waterskin", group, new Vector3(-56f, 2.02f, 73.9f), new Vector3(0.5f, 0.3f, 0.34f), blue);
            var gift = new GameObject("Waterskin Gift").AddComponent<LandGift>();
            gift.transform.SetParent(group, false);
            gift.transform.position = new Vector3(-56f, 1f, 72.9f);
            gift.Prompt = "Take the waterskin";
            gift.Range = 2.2f;
            gift.FlasksAfter = 4;
            gift.Words = "A fourth flask";
            gift.Configure(skin.gameObject);
            things.Add(gift);
            // Two on their knees at the lid, scratching. They do not notice anyone beyond five metres.
            Spawn(EnemyKind.ThirstName, -58.9f, 1f, 77.6f, 80f).MakeDormant(5f, "thirst_kneel_loop");
            Spawn(EnemyKind.ThirstName, -54.6f, 1f, 76.4f, 340f).MakeDormant(5f, "thirst_kneel_loop");
        }

        // ------------------------------------------------------------------ district 2: the city of wicks

        private static void CityOfWicks(AreaDirector director)
        {
            var group = BuildKit.Group("City Of Wicks", world);

            // The ground the city climbs: the lane level at 4 off the quay, the square and the alley at 6, the market's
            // terrace, the gate and the yard at 8, with the yard's gallery at 10. Each rise faces south and reads as a band.
            SunCityGround.Plateau("Lower Town", group, SunCityGround.Rect(-40f, 40f, 134f, 150f), Quay, -1f, earth, brickDark, null, 0f);
            SunCityGround.Plateau("Middle Town", group, new[]
            {
                new Vector2(-40f, 148f), new Vector2(-40f, 204f), new Vector2(40f, 204f), new Vector2(40f, 148f), new Vector2(3f, 148f), new Vector2(3f, 152f),
                new Vector2(-2f, 152f), new Vector2(-2f, 148f), new Vector2(-27f, 148f), new Vector2(-27f, 176f), new Vector2(-30f, 176f), new Vector2(-30f, 148f),
            }, Square, -1f, earth, brick, brickDark, 0.25f, 0f, 0.7f);
            SunCityGround.Plateau("Upper Town", group, new[]
            {
                new Vector2(-62f, 204f), new Vector2(-62f, 240f), new Vector2(-22f, 240f), new Vector2(-22f, 272f), new Vector2(40f, 272f), new Vector2(40f, 204f),
                new Vector2(4f, 204f), new Vector2(4f, 196f), new Vector2(-4f, 196f), new Vector2(-4f, 204f),
            }, Terrace, -1f, earth, brick, brickDark, 0.25f, 0f, 0.7f);
            Trodden(group, 0.5f, 134f, 0.5f, 148f, 2.2f, Quay);
            Trodden(group, 0.5f, 152f, 11.5f, 158f, 2.2f, Square);
            Trodden(group, 11.5f, 160f, 11.5f, 178f, 2.2f, Square);
            Trodden(group, 11.5f, 178f, -10f, 178f, 2f, Square);
            Trodden(group, -10f, 180f, -8f, 199f, 2.6f, Square);
            Trodden(group, 0f, 214f, 0f, 270f, 3f, Terrace);

            // 2A, the Lamp Street, leg one: north off the quay through a gatehouse arch, houses either side.
            Box("Gatehouse West", group, -4f, -2f, 134f, 137f, Quay, 8f, brick);
            Box("Gatehouse East", group, 3f, 5f, 134f, 137f, Quay, 8f, brick);
            ShadeRoof(group, "Gatehouse Arch", -2f, 3f, 134f, 137f, Quay, 4f);
            Mass(group, -40f, -30f, 134f, 148f, Quay, 4f);
            Mass(group, -26f, -10f, 137f, 148f, Quay, 4f);
            Mass(group, 3f, 40f, 137f, 148f, Quay, 4f);
            SunCityKit.Place("house_block", group, new Vector3(5.2f, Quay, 141f), 270f);
            SunCityKit.Place("house_block", group, new Vector3(5.2f, Quay, 146.2f), 270f);
            for (int i = 0; i < 2; i++)
            {
                float z = 141f + i * 6f;
                Deco(PrimitiveType.Cube, "Lamp Niche", group, new Vector3(-1.9f, 5.5f, z + 3f), new Vector3(0.2f, 0.5f, 0.5f), shade);
                Deco(PrimitiveType.Sphere, "Lamp", group, new Vector3(-1.78f, 5.42f, z + 3f), new Vector3(0.16f, 0.16f, 0.22f), i == 1 ? ember : clay);
            }
            AddShade(-2f, -0.5f, 140f, 142f);
            AddShade(3f, 4.5f, 145f, 147f);
            Spawn(EnemyKind.WickName, 0.5f, Quay, 144f, 180f);
            Stairs(group, new Vector3(0.5f, Quay, 148f), new Vector3(0.5f, Square, 152f), 5f);

            // The secret: the hearth room behind a cracked wall on the street's west side, roofless, its fire still banked.
            Box("Hearth Room North", group, -10f, -2f, 146f, 148f, Quay, 7.5f, brick);
            Box("Hearth Room South", group, -10f, -2f, 137f, 138f, Quay, 5f, brickDark);
            Box("Hearth Room Wall South", group, -2.6f, -2f, 138f, 139f, Quay, 7.5f, brick);
            Box("Hearth Room Wall North", group, -2.6f, -2f, 143f, 146f, Quay, 7.5f, brick);
            HearthWall(group, new Vector3(-2.3f, Quay, 141f));
            Scatter(group, -9.5f, -3f, 138.5f, 145.5f, Quay, 8);
            Box("Hearth", group, -9.8f, -8.6f, 140.6f, 143.4f, Quay, 5.2f, brickDark);
            Deco(PrimitiveType.Cube, "Hearth Mouth", group, new Vector3(-8.58f, 4.5f, 142f), new Vector3(0.05f, 0.7f, 1.4f), shade);
            var hearthEmbers = Deco(PrimitiveType.Sphere, "Banked Embers", group, new Vector3(-8.3f, 4.12f, 142f), new Vector3(0.7f, 0.22f, 0.9f), ember);
            var hearth = new GameObject("Hearth Gift").AddComponent<LandGift>();
            hearth.transform.SetParent(group, false);
            hearth.transform.position = new Vector3(-7.4f, Quay, 142f);
            hearth.Prompt = "Take the Shards they missed";
            hearth.Range = 2.2f;
            hearth.Embers = 5;
            hearth.Words = "Five Shards";
            hearth.Configure(hearthEmbers.gameObject);
            things.Add(hearth);
            // Claude's words, new in the second pass; "Ahu" stands in for the old god's name until the owner gives it.
            Carving(group, new Vector3(-6.6f, Quay, 145.2f), 180f, "The hearth's name", "\"A fire in every house, and it is one fire.\"", "\"In the morning Ahu lights the day from it.\"", "\"This one they missed.\"");

            // The Lamp Square at 6, a dry fountain; leg two north from it under the bridge-house.
            Mass(group, -27f, -2f, 148f, 160f, Square, 4f);
            Mass(group, 14f, 40f, 148f, 204f, Square, 4f);
            Box("Square South Houses", group, 3f, 14f, 148f, 152f, Square, 7.4f, brick);
            SunCityKit.Place("house_doorway", group, new Vector3(-2.2f, Square, 156f), 90f, 1f, false);
            Spawn(EnemyKind.WickName, -1.4f, Square, 156f, 90f).MakeDormant(3.5f);
            Carving(group, new Vector3(-1.25f, Square, 158.4f), 90f, "A hearth niche",
                "\"A fire in every house, and it is one fire. Bank it at night.\"", "\"In the morning\" and then a name, cut away, \"lights the day from it.\"");
            Disc("Dry Fountain", group, new Vector3(6f, Square, 156f), 1.5f, 0.7f, stone);
            Deco(PrimitiveType.Cylinder, "Fountain Dark", group, new Vector3(6f, Square + 0.71f, 156f), new Vector3(2.4f, 0.01f, 2.4f), shade);
            Canopy(group, "Square Awning", 8f, 12f, 154.6f, 157.6f, Square, 2.6f, cloth);
            Paving(group, -2f, 14f, 152f, 160f, Square, 1.8f, paveB, paveA, earthDark, 0.45f);
            Doorstep(group, new Vector3(9.6f, Square, 163f));
            // The bridge-house: a room over leg two; its body thins out, the street under it is shade.
            Box("Bridge-house", group, 9f, 14f, 166f, 170f, 10f, 13f, brick);
            ShadeRoof(group, "Bridge-house Underside", 9f, 14f, 166f, 170f, Square, 4f);
            Ladder(group, "Roof Ladder", new Vector3(13.4f, Square, 163f), new Vector3(15.1f, 10f, 163f), "");
            Spawn(EnemyKind.AshenName, 11.5f, Square, 172f, 180f);

            // The burnt houses south of the alley: roofless shells, black floors, walls low enough to see over.
            var burnt = BuildKit.Group("Burnt Houses", group);
            Deco(PrimitiveType.Cube, "Char", burnt, new Vector3(-0.5f, Square + 0.01f, 168f), new Vector3(19f, 0.02f, 16f), char_).shadowCastingMode = ShadowCastingMode.Off;
            Box("Burnt Wall South", burnt, -10f, 9f, 160f, 160.6f, Square, 7.4f, brickDark);
            Box("Burnt Wall North", burnt, -10f, 9f, 175.4f, 176f, Square, 7.4f, brickDark);
            Box("Burnt Wall West", burnt, -10f, -9.4f, 160f, 176f, Square, 8.5f, brickDark);
            Box("Burnt Wall East", burnt, 8.4f, 9f, 160f, 176f, Square, 8.5f, brickDark);
            Box("Burnt Divider", burnt, -1.3f, -0.7f, 160.6f, 175.4f, Square, 7.8f, brickDark);
            Box("Burnt Divider", burnt, 3.7f, 4.3f, 160.6f, 168f, Square, 7.2f, brickDark);
            foreach (var at in new[] { new Vector3(-6f, 6.3f, 166f), new Vector3(2f, 6.2f, 171f), new Vector3(6f, 6.4f, 163f) })
                Deco(PrimitiveType.Cube, "Charred Beam", burnt, at, new Vector3(0.3f, 0.3f, 4.2f), char_, new Vector3(8f, (float)dice.NextDouble() * 180f, 14f));
            Rubble(burnt, -9f, 8f, 161f, 175f, Square, 14);

            // 2B, the Oil Alley: west along the houses' doors, jars of oil against the north wall, its end roofed.
            Box("Alley Houses A", group, -10f, -4f, 180f, 196f, Square, 10f, brick);
            Box("Alley Recess A", group, -4f, -2f, 181.5f, 196f, Square, 10f, brick);
            Box("Alley Houses B", group, -2f, 3f, 180f, 196f, Square, 10f, brick);
            Box("Alley Recess B", group, 3f, 5f, 181.5f, 196f, Square, 10f, brick);
            Box("Alley Houses C", group, 5f, 9f, 180f, 196f, Square, 10f, brick);
            Box("Alley Recess C", group, 9f, 11f, 181.5f, 196f, Square, 10f, brick);
            Box("Alley Houses D", group, 11f, 14f, 180f, 196f, Square, 10f, brick);
            Box("Arm Houses", group, 4f, 14f, 196f, 204f, Terrace, 10f, brick);
            RoofTop(group, -10f, 14f, 180f, 196f, 10f);
            RoofTop(group, 14f, 40f, 148f, 204f, 10f);
            foreach (var (w, e) in new[] { (-4f, -2f), (3f, 5f), (9f, 11f) }) AddShade(w, e, 180f, 181.5f);
            ShadeRoof(group, "Alley End Roof", -10f, -6f, 176f, 180f, Square, 4f);
            for (int i = 0; i < 11; i++)
            {
                // Jars of lamp oil, in clusters against the wall: dark clay, a black lip.
                float x = new[] { -8.6f, -7.8f, -0.8f, 0.2f, 1.1f, 6f, 6.9f, 7.8f, 12f, 12.9f, -9.2f }[i];
                var oil = Deco(PrimitiveType.Cylinder, "Oil Jar", group, new Vector3(x, Square + 0.42f, 180.45f), new Vector3(0.6f, 0.42f, 0.6f), clay);
                Deco(PrimitiveType.Cylinder, "Oil Lip", group, new Vector3(x, Square + 0.86f, 180.45f), new Vector3(0.36f, 0.03f, 0.36f), char_);
                oil.gameObject.AddComponent<CapsuleCollider>();
            }
            Spawn(EnemyKind.WickName, -2f, Square, 178f, 90f);
            Spawn(EnemyKind.AshenName, -7f, Square, 178.4f, 90f);
            Spawn(EnemyKind.AshenName, 4f, Square, 181f, 180f);

            // The roofs at 10: a dormant wick among drying linen; the north edge drops onto the market's east arm.
            Spawn(EnemyKind.WickName, 20f, 10f, 186f, 270f).MakeDormant(3.5f, "wick_lie_loop");
            // The roofs are not one floor: upper rooms stand on them, so the way over winds between walls, north along
            // the east block's edge and then west over the alley's houses to the drop.
            foreach (var (w, e, s2, n2, h) in new[]
            {
                (22f, 40f, 148f, 160f, 2.6f), (26f, 40f, 164f, 178f, 2.2f), (21.5f, 25f, 167f, 175f, 1.8f), (28f, 40f, 182f, 204f, 2.8f),
                (14f, 22f, 196.5f, 204f, 2.4f), (5f, 11.5f, 186.5f, 195.6f, 2.2f), (-10f, -5.5f, 189f, 196f, 2f),
            })
            {
                Box("Upper Room", group, w, e, s2, n2, 10f, 10f + h, brick);
                Deco(PrimitiveType.Cube, "Upper Room Parapet", group, new Vector3((w + e) * 0.5f, 10f + h + 0.12f, (s2 + n2) * 0.5f), new Vector3(e - w + 0.2f, 0.24f, n2 - s2 + 0.2f), brickDark);
            }
            foreach (var (x, z, along) in new[] { (17f, 158f, true), (24f, 181f, false), (2f, 184f, true), (-4f, 188f, false) })
                Box("Roof Wall", group, x - (along ? 1.6f : 0.2f), x + (along ? 1.6f : 0.2f), z - (along ? 0.2f : 1.6f), z + (along ? 0.2f : 1.6f), 10f, 10.7f, brickDark);
            var roofOil = Deco(PrimitiveType.Cylinder, "Oil Jar", group, new Vector3(21.6f, 10.42f, 187.6f), new Vector3(0.6f, 0.42f, 0.6f), clay);
            roofOil.gameObject.AddComponent<CapsuleCollider>();
            foreach (float z in new[] { 184f, 188.5f })
                Canopy(group, "Drying Linen", 18.5f, 23f, z, z + 0.3f, 10f, 1.6f, wrap, false);

            // 2C, the Market of the Cold Oven: an open lower floor at 6, an L-shaped terrace at 8 up a broad stair or a cart ramp.
            Mass(group, -40f, -34f, 160f, 204f, Square, 4f);
            Mass(group, -34f, -30f, 160f, 176f, Square, 4f);
            Mass(group, -27f, -10f, 160f, 176f, Square, 4f);
            ShadeRoof(group, "Lane Roof", -30f, -27f, 140f, 176f, Quay, 5f);
            Floor("Lane Floor", group, -30f, -27f, 148f, 168.5f, Quay, earthDark, 3f);
            Stairs(group, new Vector3(-28.5f, Quay, 168f), new Vector3(-28.5f, Square, 176f), 3f);
            Barred(group, "Market Door", new Vector3(-28.5f, Square, 176f), 3f, Vector3.forward, MarketDoorKey, Square);
            Paving(group, -33f, -11f, 177f, 203f, Square, 2.2f, paveA, earthDark, paveC, 0.55f);
            Scatter(group, -33f, -11f, 177f, 203f, Square, 30);
            var oven = Deco(PrimitiveType.Sphere, "Cold Oven", group, new Vector3(-16f, Square, 196f), new Vector3(4f, 3.6f, 4f), brickDark);
            oven.gameObject.AddComponent<SphereCollider>();
            Deco(PrimitiveType.Cube, "Oven Mouth", group, new Vector3(-16f, Square + 0.5f, 194.05f), new Vector3(1.1f, 0.8f, 0.3f), shade);
            Deco(PrimitiveType.Cube, "Oven Shelf", group, new Vector3(-16f, Square + 0.9f, 193.8f), new Vector3(1.8f, 0.08f, 0.5f), stoneDark);
            for (int i = 0; i < 3; i++) Deco(PrimitiveType.Sphere, "Grey Bread", group, new Vector3(-16.5f + i * 0.5f, Square + 1.02f, 193.8f), new Vector3(0.4f, 0.16f, 0.34f), silt);
            AddShade(-18f, -14f, 198f, 200f);
            Canopy(group, "Market Awning", -20f, -16f, 189.6f, 192.6f, Square, 2.6f, cloth);
            Canopy(group, "Market Awning", -30f, -26f, 197.6f, 200.6f, Square, 2.6f, reed);
            Canopy(group, "Market Awning", -14f, -10f, 181.6f, 184.6f, Square, 2.6f, cloth);
            foreach (var at in new[] { new Vector3(-23f, Square, 190f), new Vector3(-12f, Square, 188f), new Vector3(-26f, Square, 180f) })
                Box("Stall", group, at.x - 1.2f, at.x + 1.2f, at.z - 0.5f, at.z + 0.5f, Square, Square + 0.9f, wood);
            jars.Add((group, new Vector3(-30.5f, Square, 184f)));
            jars.Add((group, new Vector3(-31.2f, Square, 185.6f)));
            jars.Add((group, new Vector3(-29.6f, Square, 186.4f)));
            Spawn(EnemyKind.WickName, -18f, Square, 186f, 90f);
            Spawn(EnemyKind.WickName, -24f, Square, 196f, 120f);
            Spawn(EnemyKind.ThirstName, -28f, Square, 183.6f, 250f);
            Spawn(EnemyKind.AshenName, -6f, Terrace, 210f, 180f);
            Stairs(group, new Vector3(-8f, Square, 199.6f), new Vector3(-8f, Terrace, 204.2f), 6f);
            SunCityGround.Ramp("Cart Ramp", group, new Vector3(-32f, Square, 191.6f), new Vector3(-32f, Terrace, 204.3f), 4f, earth, brickDark);
            Paving(group, -34f, 4f, 204f, 214f, Terrace, 2.4f, paveB, paveA, paveC, 0.3f);
            Paving(group, -4f, 4f, 196f, 204f, Terrace, 2.4f, paveB, paveA, paveC, 0.3f);
            // The terrace's colonnade along the gate wall: pillars and a roof drawn only as its shadow.
            for (float x = -32f; x <= 2f; x += 4.25f)
                Box("Terrace Pillar", group, x - 0.3f, x + 0.3f, 211.4f, 212f, Terrace, 11.4f, stone);
            ShadeRoof(group, "Terrace Colonnade Roof", -34f, 4f, 211f, 214f, Terrace, 3.4f);
            Mass(group, -40f, -34f, 204f, 214f, Terrace, 4f);
            Mass(group, 6f, 40f, 204f, 214f, Terrace, 4f);

            // 2D, the Embalmers' Gate: two square towers, the passage between them roofed, timber doors standing open.
            Box("Gate Wall West", group, -40f, -11f, 214f, 226f, Terrace, 13f, brick);
            Box("Gate Wall East", group, 11f, 40f, 214f, 226f, Terrace, 13f, brick);
            Box("Gate Tower West", group, -11f, -6f, 214f, 226f, Terrace, 15f, brick);
            Box("Gate Tower East", group, 6f, 11f, 214f, 226f, Terrace, 15f, brick);
            foreach (float x in new[] { -8.5f, 8.5f })
                Deco(PrimitiveType.Cube, "Tower Cap", group, new Vector3(x, 15.15f, 220f), new Vector3(5.6f, 0.3f, 12.6f), brickDark);
            ShadeRoof(group, "Gate Passage Roof", -6f, 6f, 214f, 226f, Terrace, 6.4f);
            Deco(PrimitiveType.Cube, "Gate Door West", group, new Vector3(-5.8f, Terrace + 2.4f, 223.6f), new Vector3(0.25f, 4.8f, 3.6f), wood);
            Deco(PrimitiveType.Cube, "Gate Door East", group, new Vector3(5.8f, Terrace + 2.4f, 223.6f), new Vector3(0.25f, 4.8f, 3.6f), wood);
            Paving(group, -6f, 6f, 214f, 226f, Terrace, 2f, paveB, paveA, paveC, 0.15f);
            // On the east door a gilded soldier is pinned to the wood by his own spear: a hand's width of gold on his mask and no more.
            PinnedSoldier(group, new Vector3(5.45f, Terrace, 223.4f));
            Carving(group, new Vector3(5.1f, Terrace, 219.2f), 270f, "A tablet in the gate",
                "\"The guard of the golden way wears the god's flesh. Whoever refuses the pouring holds the lower gate, and holds it until he is relieved.\"", "Nobody relieves him.");
            Rest(group, director, new Vector3(-4f, Terrace, 220f));
            Carving(group, new Vector3(-5.2f, Terrace, 227.2f), 180f, "The lintel of the yard", "\"The king says: you burn and are not consumed. Wrap them. Stand them in the sun. No one here is buried.\"");

            // 2E, the Embalmers' Yard: a working floor at 8, tables in staggered rows, and the gallery of the standing dead at 10.
            Box("Yard Wall West", group, -24f, -22f, 226f, 247.5f, 4.5f, 13f, brick);
            Box("Yard Wall West North", group, -24f, -22f, 252.5f, 272f, Terrace, 13f, brick);
            Box("Yard Wall East", group, 22f, 24f, 226f, 272f, Terrace, 13f, brick);
            Mass(group, 24f, 40f, 226f, 272f, Terrace, 5f);
            Paving(group, -22f, 22f, 226f, 262f, Terrace, 2.4f, paveB, paveA, paveC, 0.5f);
            Scatter(group, -21f, 21f, 227f, 261f, Terrace, 36);
            // Drains cut in the paving between the rows of tables, running to a sump in the middle.
            foreach (float x in new[] { -8f, 3.5f, 9f })
                Deco(PrimitiveType.Cube, "Drain", group, new Vector3(x, Terrace + 0.012f, 244f), new Vector3(0.35f, 0.02f, 30f), char_).shadowCastingMode = ShadowCastingMode.Off;
            Deco(PrimitiveType.Cube, "Drain", group, new Vector3(0f, Terrace + 0.012f, 244f), new Vector3(34f, 0.02f, 0.35f), char_).shadowCastingMode = ShadowCastingMode.Off;
            Disc("Sump", group, new Vector3(0f, Terrace, 244f), 1f, 0.15f, stoneDark);
            foreach (var at in new[] { new Vector3(-17f, Terrace, 235f), new Vector3(-16f, Terrace, 246f), new Vector3(17.5f, Terrace, 246f), new Vector3(-6f, Terrace, 256f) })
            {
                // Baskets of natron and linen ready for the next.
                Deco(PrimitiveType.Cylinder, "Basket", group, at + new Vector3(0f, 0.3f, 0f), new Vector3(0.9f, 0.3f, 0.9f), reed);
                Deco(PrimitiveType.Cylinder, "Basket", group, at + new Vector3(1.1f, 0.25f, 0.5f), new Vector3(0.7f, 0.25f, 0.7f), reed);
                Deco(PrimitiveType.Cube, "Linen", group, at + new Vector3(-1f, 0.15f, 0.3f), new Vector3(1.2f, 0.3f, 0.8f), wrap);
            }
            foreach (var at in new[] { new Vector3(6f, Terrace + 0.62f, 243f), new Vector3(-4f, Terrace + 0.62f, 233f), new Vector3(12f, Terrace + 0.62f, 254f) })
                Deco(PrimitiveType.Capsule, "Wrapped Dead", group, at, new Vector3(0.55f, 0.95f, 0.55f), wrap, new Vector3(90f, 0f, 0f));
            SunCityGround.Plateau("Yard Gallery", group, SunCityGround.Rect(-22f, 22f, 262f, 272f), Gallery, 7f, stone, ashlar, ashlarBand, 0f, 0f, 0.5f);
            Cornice(group, -16f, 16f, 262f, Gallery, Terrace);
            Stairs(group, new Vector3(-18f, Terrace, 257.8f), new Vector3(-18f, Gallery, 262.2f), 4f);
            Stairs(group, new Vector3(18f, Terrace, 257.8f), new Vector3(18f, Gallery, 262.2f), 4f);
            foreach (float x in new[] { -13f, -9.4f, -5.8f, -2.2f, 2.2f, 5.8f, 9.4f, 13f })
                SunCityKit.Place("burial_niche", group, new Vector3(x, Terrace, 261.6f), 180f, 0.62f);
            for (float x = -20f; x <= 20f; x += 4f)
                Box("Gallery Pillar", group, x - 0.25f, x + 0.25f, 265.5f, 266f, Gallery, 13f, stone);
            ShadeRoof(group, "Gallery Portico Roof", -22f, 22f, 262f, 266f, Gallery, 3f);
            for (int i = 0; i < 12; i++)
            {
                // The wrapped dead stand in racks along the gallery, facing the sun.
                float x = -19f + i * 3.4f;
                if (Mathf.Abs(x) < 3.5f) continue;
                Box("Rack", group, x - 1.3f, x + 1.3f, 270.6f, 271f, Gallery, Gallery + 2f, wood);
                for (int k = 0; k < 3; k++)
                    Deco(PrimitiveType.Capsule, "Standing Dead", group, new Vector3(x - 0.8f + k * 0.8f, Gallery + 0.95f, 270.2f), new Vector3(0.5f, 0.92f, 0.45f), wrap);
            }
            SunCityKit.Place("brazier", group, new Vector3(-7.4f, Gallery, 269f));
            SunCityKit.Place("brazier", group, new Vector3(7.4f, Gallery, 269f));
            Box("Yard North Wall West", group, -24f, -3f, 272f, 274f, Gallery, 16f, brick);
            Box("Yard North Wall East", group, 3f, 24f, 272f, 274f, Gallery, 16f, brick);
            foreach (var at in new[] { new Vector3(-10f, Terrace, 250f), new Vector3(6f, Terrace, 243f), new Vector3(-4f, Terrace, 233f), new Vector3(12f, Terrace, 254f), new Vector3(-15f, Terrace, 240f), new Vector3(1f, Terrace, 252f) })
            {
                // A table to lay the dead out on: low, of stone, a drain cut round it.
                Floor("Embalming Table", group, at.x - 0.75f, at.x + 0.75f, at.z - 1.4f, at.z + 1.4f, at.y + 0.34f, stone, 0.34f);
                Deco(PrimitiveType.Cube, "Table Foot", group, at + new Vector3(0f, 0.02f, 0f), new Vector3(2f, 0.04f, 3.3f), stoneDark).shadowCastingMode = ShadowCastingMode.Off;
            }
            Canopy(group, "Table Canopy", -12f, -8f, 248.6f, 251.6f, Terrace, 2.6f, reed);
            Canopy(group, "Table Canopy", 4f, 8f, 241.6f, 244.6f, Terrace, 2.6f, reed);
            SunCityKit.Place("sarcophagus", group, new Vector3(-4f, Terrace + 0.34f, 233f), 0f, 0.8f, false);
            SunCityKit.Place("sarcophagus", group, new Vector3(12f, Terrace + 0.34f, 254f), 0f, 0.8f, false);
            // The natron shed in the east, heaps of it white.
            foreach (var (x, z) in new[] { (14.3f, 232.3f), (19.7f, 232.3f), (14.3f, 237.7f), (19.7f, 237.7f) }) Box("Shed Post", group, x - 0.2f, x + 0.2f, z - 0.2f, z + 0.2f, Terrace, 11f, wood);
            ShadeRoof(group, "Natron Shed Roof", 14f, 20f, 232f, 238f, Terrace, 3f);
            foreach (var at in new[] { new Vector3(16f, Terrace, 235.5f), new Vector3(18.2f, Terrace, 234f), new Vector3(-18f, Terrace, 230f) })
            {
                var heap = Deco(PrimitiveType.Sphere, "Natron Heap", group, at, new Vector3(2.4f, 1.7f, 2.4f), wrap);
                heap.gameObject.AddComponent<SphereCollider>();
            }
            // Frames where the linen dries, strips hanging from them: lanes across the yard the camera sees through.
            foreach (var (x, z) in new[] { (-16f, 232f), (-12f, 258f), (16f, 252f), (9f, 232f) })
            {
                Box("Drying Frame Post", group, x - 2.4f, x - 2.2f, z - 0.1f, z + 0.1f, Terrace, Terrace + 2.6f, wood);
                Box("Drying Frame Post", group, x + 2.2f, x + 2.4f, z - 0.1f, z + 0.1f, Terrace, Terrace + 2.6f, wood);
                Deco(PrimitiveType.Cube, "Drying Frame Bar", group, new Vector3(x, Terrace + 2.55f, z), new Vector3(4.9f, 0.1f, 0.1f), wood);
                for (int k = 0; k < 5; k++)
                    Deco(PrimitiveType.Cube, "Linen Strip", group, new Vector3(x - 1.8f + k * 0.9f, Terrace + 1.75f + (k % 2) * 0.15f, z), new Vector3(0.5f, 1.5f - (k % 2) * 0.3f, 0.03f), wrap);
            }
            burning.Add(Spawn(EnemyKind.WickName, -6f, Terrace, 238f, 180f));
            burning.Add(Spawn(EnemyKind.WickName, 10f, Terrace, 248f, 180f));
            Spawn(EnemyKind.WickName, -10f, Terrace + 0.36f, 250f, 0f).MakeDormant(4f, "wick_lie_loop");
            Spawn(EnemyKind.WickName, 14f, Gallery, 268.5f, 180f).MakeDormant(3.5f);
        }

        // A cornice along a face's top edge, standing proud of it toward the camera; a block missing here and there, fallen
        // below. Drawn only: it never catches a foot.
        private static void Cornice(Transform parent, float west, float east, float face, float top, float foot)
        {
            float x = west;
            while (x < east - 0.2f)
            {
                float length = Mathf.Min(east - x, 2.2f + (float)dice.NextDouble() * 3f);
                if (dice.NextDouble() < 0.14 && length < east - west)
                {
                    // A gap where the cornice fell, and the block lying at the foot of the face.
                    Deco(PrimitiveType.Cube, "Fallen Cornice", parent, new Vector3(x + length * 0.5f, foot + 0.2f, face - 1.2f - (float)dice.NextDouble()), new Vector3(Mathf.Min(length, 1.4f), 0.4f, 0.5f), stoneNew, new Vector3(0f, (float)dice.NextDouble() * 40f - 20f, 6f));
                }
                else
                {
                    Deco(PrimitiveType.Cube, "Cornice", parent, new Vector3(x + length * 0.5f, top - 0.14f, face - 0.16f), new Vector3(length - 0.04f, 0.28f, 0.36f), stoneNew);
                    Deco(PrimitiveType.Cube, "Architrave", parent, new Vector3(x + length * 0.5f, top - 0.5f, face - 0.06f), new Vector3(length - 0.04f, 0.44f, 0.14f), ashlarBand);
                }
                x += length;
            }
        }

        // Crates, bales and jars stacked round a point: cover a body can stand behind, cargo nobody came for.
        private static void Cargo(Transform parent, Vector3 at, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = at.x + (i % 3) * 1.3f - 1.3f + (float)dice.NextDouble() * 0.3f, z = at.z + (i / 3) * 1.4f + (float)dice.NextDouble() * 0.3f;
                if (i % 3 == 2)
                {
                    var jar = Deco(PrimitiveType.Cylinder, "Cargo Jar", parent, new Vector3(x, at.y + 0.5f, z), new Vector3(0.7f, 0.5f, 0.7f), clay);
                    jar.gameObject.AddComponent<CapsuleCollider>();
                    continue;
                }
                float high = i % 3 == 0 ? 1.1f : 0.8f;
                var crate = BuildKit.Block(i % 2 == 0 ? "Crate" : "Bale", parent, new Vector3(x, at.y + high * 0.5f, z), new Vector3(1.1f, high, 1.1f), i % 2 == 0 ? wood : reed, (float)dice.NextDouble() * 20f - 10f);
                Object.DestroyImmediate(crate.GetComponent<Fadeable>());
                if (i == 0) Deco(PrimitiveType.Cube, "Crate On Top", parent, new Vector3(x, at.y + high + 0.35f, z), new Vector3(0.8f, 0.7f, 0.8f), wood, new Vector3(0f, 25f, 0f));
            }
        }

        // A walkable flat roof: dust and a parapet, low on the south so the camera sees whoever walks it.
        private static void RoofTop(Transform parent, float west, float east, float south, float north, float top)
        {
            Deco(PrimitiveType.Cube, "Roof Dust", parent, new Vector3((west + east) * 0.5f, top + 0.01f, (south + north) * 0.5f), new Vector3(east - west - 0.8f, 0.02f, north - south - 0.8f), sand).shadowCastingMode = ShadowCastingMode.Off;
            Deco(PrimitiveType.Cube, "Roof Parapet North", parent, new Vector3((west + east) * 0.5f, top + 0.3f, north - 0.15f), new Vector3(east - west, 0.6f, 0.3f), brickDark);
            // Low on the south, the camera's side; along the sides as high as the north.
            Deco(PrimitiveType.Cube, "Roof Parapet South", parent, new Vector3((west + east) * 0.5f, top + 0.2f, south + 0.15f), new Vector3(east - west, 0.4f, 0.3f), brickDark);
            Deco(PrimitiveType.Cube, "Roof Parapet West", parent, new Vector3(west + 0.15f, top + 0.3f, (south + north) * 0.5f), new Vector3(0.3f, 0.6f, north - south), brickDark);
            Deco(PrimitiveType.Cube, "Roof Parapet East", parent, new Vector3(east - 0.15f, top + 0.3f, (south + north) * 0.5f), new Vector3(0.3f, 0.6f, north - south), brickDark);
            // The line between one house's roof and the next, every eight metres or so.
            for (float z = south + 8f; z < north - 2f; z += 7f + (float)dice.NextDouble() * 3f)
                Deco(PrimitiveType.Cube, "Roof Seam", parent, new Vector3((west + east) * 0.5f, top + 0.08f, z), new Vector3(east - west, 0.16f, 0.25f), brick);
            for (int i = 0; i < Mathf.Max(1, Mathf.RoundToInt((east - west) * (north - south) / 120f)); i++)
            {
                float x = Mathf.Lerp(west + 2f, east - 2f, (float)dice.NextDouble()), z = Mathf.Lerp(south + 2f, north - 2f, (float)dice.NextDouble());
                Deco(PrimitiveType.Cube, "Roof Hatch", parent, new Vector3(x, top + 0.05f, z), new Vector3(1.2f, 0.1f, 1.2f), wood);
            }
        }

        // The cracked wall of the hearth room: one heavy blow, or a burst beside it, brings it down.
        private static void HearthWall(Transform parent, Vector3 foot)
        {
            var wall = BuildKit.Block("Cracked Wall", parent, foot + new Vector3(0f, 1.75f, 0f), new Vector3(0.6f, 3.5f, 4f), brick);
            Object.DestroyImmediate(wall.GetComponent<Fadeable>());
            var shown = new GameObject("Whole").transform;
            shown.SetParent(wall.transform, false);
            for (int i = 0; i < 5; i++)
            {
                // Cracks drawn on its street face.
                var crack = Deco(PrimitiveType.Cube, "Crack", shown, foot + new Vector3(0.31f, 0.6f + i * 0.6f, -1.4f + (float)dice.NextDouble() * 2.8f), new Vector3(0.02f, 0.06f, 0.6f + (float)dice.NextDouble() * 0.8f), char_, new Vector3((float)dice.NextDouble() * 70f - 35f, 0f, 0f));
                crack.transform.SetParent(shown, true);
            }
            var rubble = BuildKit.Group("Fallen", parent);
            Rubble(rubble, foot.x - 1.5f, foot.x + 1.2f, foot.z - 2f, foot.z + 2f, foot.y, 12);
            var breakable = wall.AddComponent<BreakableWall>();
            breakable.BrokenKey = HearthKey;
            breakable.Configure(shown.gameObject, rubble.gameObject);
        }

        // The side path: three painted chambers sunk 4 m below the yard's west side, down a stair from its wall.
        // As built in the first pass, moved by (-2, +5, -12): chambers x -58 to -28, z 246 to 254, floor at 4.
        private static readonly Vector3 TombShift = new Vector3(-2f, 5f, -12f);
        private const float TombFloor = 4f;

        private static void PaintedTombs()
        {
            var group = BuildKit.Group("Painted Tombs", world);
            // The ground round the pit: low on its south (the camera's side), at the yard's height on its north and west.
            SunCityGround.Plateau("Tomb South Ground", group, SunCityGround.Rect(-62f, -22f, 240f, 246f), TombFloor + 0.5f, -1f, earth, brickDark, null, 0.3f);
            SunCityGround.Plateau("Tomb North Ground", group, SunCityGround.Rect(-62f, -22f, 254f, 272f), Terrace, -1f, earth, stone, stoneDark, 0.3f, 0f, 1.1f);
            SunCityGround.Plateau("Tomb West Ground", group, SunCityGround.Rect(-62f, -58f, 246f, 254f), Terrace, -1f, earth, stone, null, 0f);
            SunCityGround.Plateau("Tomb Stair Cheek South", group, SunCityGround.Rect(-28f, -22f, 246f, 247.5f), Terrace, -1f, earth, brickDark, null, 0f);
            SunCityGround.Plateau("Tomb Stair Cheek North", group, SunCityGround.Rect(-28f, -22f, 252.5f, 254f), Terrace, -1f, earth, brickDark, null, 0f);
            Floor("Tomb Floor", group, -58f, -28f, 246f, 254f, TombFloor, shade, 1f);
            Stairs(group, new Vector3(-21.8f, Terrace, 250f), new Vector3(-30f, TombFloor, 250f), 5f);
            foreach (float x in new[] { -48f, -38f })
            {
                Box("Tomb Divider South", group, x - 0.5f, x + 0.5f, 246f, 248.5f, TombFloor, TombFloor + 0.5f, stoneDark);
                Box("Tomb Divider North", group, x - 0.5f, x + 0.5f, 251.5f, 254f, TombFloor, Terrace, stone);
            }
            var wicks = new[]
            {
                Spawn(EnemyKind.WickName, -46.2f, TombFloor, 247.6f, 90f),
                Spawn(EnemyKind.WickName, -39.8f, TombFloor, 247.6f, 270f),
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
            Disc("Grave Dish", group, new Vector3(-54.4f, -1f, 259.6f) + TombShift, 0.6f, 0.3f, stoneDark);
            var embers = Deco(PrimitiveType.Sphere, "Grave Embers", group, new Vector3(-54.4f, -0.66f, 259.6f) + TombShift, new Vector3(0.5f, 0.2f, 0.5f), ember);
            var dish = new GameObject("Grave Dish Gift").AddComponent<LandGift>();
            dish.transform.SetParent(group, false);
            dish.transform.position = new Vector3(-53.4f, -1f, 260.2f) + TombShift;
            dish.Prompt = "Take what was left for the dead";
            dish.Range = 2.2f;
            dish.Embers = 3;
            dish.Words = "Three Shards";
            dish.Configure(embers.gameObject);
            things.Add(dish);
        }

        private static void Painting(Transform parent, string panel, float x, Enemy[] wakes, params string[] lines)
        {
            var hung = SunCityKit.Panel(panel, parent, new Vector3(x, -0.85f, 265.9f) + TombShift);
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

        private static void GoldenWay(AreaDirector director)
        {
            var group = BuildKit.Group("Golden Way", world);
            // The king's ground climbs in three terraces, each face a band of dressed stone toward the camera.
            SunCityGround.Plateau("Avenue And Court", group, SunCityGround.Rect(-40f, 40f, 272f, 346f), Avenue, -1f, stone, ashlar, null, 0f);
            SunCityGround.Plateau("Masons' Terrace", group, SunCityGround.Rect(-32f, 40f, 346f, 386f), Masons, 9f, stone, ashlar, ashlarBand, 0.12f, 0f, 1f);
            SunCityGround.Plateau("Upper Terrace", group, SunCityGround.Rect(-24f, 24f, 386f, HallSouth), Upper, 12f, stone, ashlar, ashlarBand, 0.12f, 0f, 1f);
            Box("Masons' Side East", group, 26f, 40f, 346f, 386f, Masons, 20f, stone);
            Box("Terrace Side East", group, 24f, 40f, 386f, 418f, Masons, 24f, stone);

            // 3A, the Avenue of the Seated King: pale paving with two gold lines, two pairs of statues, stalls at its sides.
            Paving(group, -10f, 10f, 274f, 318f, Avenue, 3f, newA, newB, paveC, 0.04f);
            Paving(group, -20f, -10f, 274f, 318f, Avenue, 3.3f, paveA, paveB, paveC, 0.3f);
            Paving(group, 10f, 20f, 274f, 318f, Avenue, 3.3f, paveA, paveB, paveC, 0.25f);
            foreach (float x in new[] { -6f, 6f }) GoldLine(group, x - 0.11f, x + 0.11f, 275f, 345.4f, Avenue);
            Box("Avenue Wall West", group, -24f, -20f, 274f, 284f, Avenue, 16f, stone);
            Box("Avenue Wall West North", group, -24f, -20f, 288f, 318f, Avenue, 16f, stone);
            Box("Avenue Wall East", group, 20f, 24f, 274f, 318f, Avenue, 16f, stone);
            Drift(group, new Vector3(-17.5f, Avenue, 312f), 4f, 9f);
            Drift(group, new Vector3(17.8f, Avenue, 296f), 3.6f, 8f);
            foreach (float z in new[] { 286f, 302f })
                foreach (float side in new[] { -1f, 1f })
                {
                    float yaw = side < 0f ? 90f : 270f;
                    var at = new Vector3(side * 11.5f, Avenue, z);
                    Floor("Statue Plinth", group, at.x - 3.2f, at.x + 3.2f, z - 3.2f, z + 3.2f, Avenue + 0.3f, stoneDark, 0.3f);
                    SunCityKit.Place("statue_king", group, at + Vector3.up * 0.3f, yaw);
                    var head = at + Vector3.up * 0.3f + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 8f, 1.1f);
                    SunCityKit.Place("statue_head_new", group, head, yaw, 1f, false);
                    AddShade(at.x - 2.5f, at.x + 2.5f, z + 3.2f, z + 5.1f);
                }
            Carving(group, new Vector3(-9.4f, Avenue, 289.9f), 90f, "The base of the first statue",
                "\"Khepharu, who is the sun. Who is not born. Who does not set.\"", "Under it, shallow, half ground away: \"...who carries the day up from the river for us.\"");
            SunCityKit.Place("brazier", group, new Vector3(-9.5f, Avenue, 276.5f));
            SunCityKit.Place("brazier", group, new Vector3(9.5f, Avenue, 276.5f));
            Rest(group, director, new Vector3(-6f, Avenue, 280f));
            Canopy(group, "Stall Awning", -8f, -4f, 292.6f, 295.6f, Avenue, 2.6f, cloth);
            Canopy(group, "Stall Awning", 4f, 8f, 308.6f, 311.6f, Avenue, 2.6f, reed);
            Box("Stall", group, -7.6f, -4.4f, 293f, 293.8f, Avenue, Avenue + 0.9f, wood);
            Box("Stall", group, 4.4f, 7.6f, 309f, 309.8f, Avenue, Avenue + 0.9f, wood);
            Spawn(EnemyKind.GildedLevyName, 0f, Avenue, 300f, 180f);
            // The vendors' row along the avenue's sides: stalls under awnings, and the masons' offcuts.
            foreach (float side in new[] { -1f, 1f })
                for (float z = 278f; z < 316f; z += 9.5f)
                {
                    if (Mathf.Abs(z - 286f) < 4f || Mathf.Abs(z - 302f) < 4f) continue;
                    float x = side * 17.5f;
                    Box("Vendor Stall", group, x - 1.4f, x + 1.4f, z - 0.6f, z + 0.6f, Avenue, Avenue + 0.95f, wood);
                    Canopy(group, "Vendor Awning", x - 1.8f, x + 1.8f, z - 1.6f, z + 1.4f, Avenue, 2.5f, side < 0f ? cloth : reed);
                    Deco(PrimitiveType.Cylinder, "Jar", group, new Vector3(x + side * -2f, Avenue + 0.4f, z + 0.4f), new Vector3(0.6f, 0.4f, 0.6f), clay);
                }
            Box("Offcut", group, -15.5f, -13.5f, 314f, 315.2f, Avenue, Avenue + 0.9f, stoneNew);
            Box("Offcut", group, 13.8f, 15.2f, 280.5f, 282f, Avenue, Avenue + 0.7f, stoneNew);

            // S3: the masons' haul-ramp behind the avenue's west wall, from the terrace's barred gate down to beside F4.
            Box("Haul Wall West", group, -34f, -30f, 284f, 352f, Avenue, 18f, stone);
            Box("Haul Wall East", group, -26f, -24f, 290f, 346f, Avenue, 16f, stone);
            Box("Haul Wall South", group, -30f, -20f, 282f, 284f, Avenue, 16f, stone);
            SunCityGround.Ramp("Haul-ramp", group, new Vector3(-28f, Avenue, 290f), new Vector3(-28f, Masons, 346.6f), 4f, stoneDark, ashlar);
            foreach (float x in new[] { -28.9f, -27.1f })
            {
                // Sledge runners worn into it.
                var runner = Deco(PrimitiveType.Cube, "Sledge Runner", group, Vector3.zero, Vector3.one, stoneDark);
                runner.transform.SetPositionAndRotation(new Vector3(x, (Avenue + Masons) * 0.5f + 0.02f, 318.3f), Quaternion.LookRotation(new Vector3(0f, Masons - Avenue, 56.6f)));
                runner.transform.localScale = new Vector3(0.25f, 0.02f, 56.6f);
                runner.shadowCastingMode = ShadowCastingMode.Off;
            }

            // 3B, the Lower Court and the Levy Ramp: a causeway up the middle to the masons' terrace, a colonnade either side.
            Paving(group, -24f, 24f, 318f, 341f, Avenue, 3f, paveA, paveB, paveC, 0.2f);
            Box("Court Wall East", group, 24f, 26f, 318f, 346f, Avenue, 16f, stone);
            SunCityGround.Ramp("Levy Ramp", group, new Vector3(0f, Avenue, 318f), new Vector3(0f, Masons, 346.4f), 7f, newA, ashlar);
            Colonnade(group, "Court Colonnade West", -24f, -3.5f, 341f, 346f, Avenue, Masons - Avenue);
            Colonnade(group, "Court Colonnade East", 3.5f, 24f, 341f, 346f, Avenue, Masons - Avenue);
            Stairs(group, new Vector3(-21f, Avenue, 335.4f), new Vector3(-21f, Masons, 341.2f), 2f);
            Stairs(group, new Vector3(21f, Avenue, 335.4f), new Vector3(21f, Masons, 341.2f), 2f);
            foreach (var at in new[] { new Vector3(-14f, Avenue, 326f), new Vector3(12f, Avenue, 330f), new Vector3(-8f, Avenue, 336f), new Vector3(16f, Avenue, 322f) })
                Box("Fallen Block", group, at.x - 1f, at.x + 1f, at.z - 0.7f, at.z + 0.7f, Avenue, Avenue + 1.2f, stoneNew);
            // What the court has shed: a toppled statue of the king face down, column drums rolled against the colonnade.
            var toppled = SunCityKit.Place("statue_head_new", group, new Vector3(13f, Avenue + 0.4f, 327f), 60f, 1.2f);
            if (toppled != null) toppled.transform.rotation = Quaternion.Euler(-84f, 60f, 10f);
            foreach (var (x, z, yaw) in new[] { (-17f, 338.6f, 80f), (-11.5f, 339f, 100f), (17.5f, 338.4f, 75f) })
            {
                var drum = Deco(PrimitiveType.Cylinder, "Column Drum", group, new Vector3(x, Avenue + 0.9f, z), new Vector3(1.8f, 0.9f, 1.8f), stoneNew, new Vector3(0f, yaw, 90f));
                drum.gameObject.AddComponent<CapsuleCollider>();
            }
            Rubble(group, 9f, 16f, 324f, 330f, Avenue, 9);
            Drift(group, new Vector3(-18f, Avenue, 323f), 6f, 4f);
            Drift(group, new Vector3(19f, Avenue, 335f), 5f, 3f);
            walls.Add(new[]
            {
                Spawn(EnemyKind.GildedLevyName, -1.3f, 12.8f, 344.4f, 180f), Spawn(EnemyKind.GildedLevyName, 0f, 12.8f, 344.6f, 180f), Spawn(EnemyKind.GildedLevyName, 1.3f, 12.8f, 344.4f, 180f),
            });

            // 3C, the Masons' Terrace: the unfinished colossus with the old face still on it, its scaffold, the blocks.
            Paving(group, -24f, 24f, 346.6f, 380f, Masons, 3.2f, paveA, paveB, stoneNew, 0.3f);
            var colossus = SunCityKit.Place("statue_king", group, new Vector3(-17f, Masons, 364f), 90f, 1.6f);
            var oldHead = new Vector3(-17f, Masons, 364f) + Quaternion.Euler(0f, 90f, 0f) * new Vector3(0f, 12.8f, 1.76f);
            SunCityKit.Place("statue_head_old", group, oldHead + Vector3.down * 0.8f, 90f, 1.8f, false);
            foreach (float z in new[] { 356f, 364f, 372f })
                foreach (float x in new[] { -10f, -7f })
                    Box("Scaffold Pole", group, x - 0.12f, x + 0.12f, z - 0.12f, z + 0.12f, Masons, 20.2f, wood);
            foreach (float up in new[] { 16f, 19f })
            {
                var deckFloor = BuildKit.Block("Scaffold Deck", group, new Vector3(-8.5f, up - 0.1f, 364f), new Vector3(3f, 0.2f, 16f), wood);
                Object.DestroyImmediate(deckFloor.GetComponent<Fadeable>());
                Deco(PrimitiveType.Cube, "Scaffold Rail", group, new Vector3(-7.05f, up + 0.55f, 364f), new Vector3(0.1f, 0.1f, 16f), wood);
            }
            Canopy(group, "Reed Mats", -10f, -7f, 356f, 372f, 19f, 1.6f, reed, false);
            AddShade(-12f, -6f, 356f, 372.6f);
            Ladder(group, "Scaffold Ladder Low", new Vector3(-6.3f, Masons, 358f), new Vector3(-8.4f, 16f, 358f), "");
            Ladder(group, "Scaffold Ladder High", new Vector3(-7.6f, 16f, 370f), new Vector3(-8.6f, 19f, 370.6f), "");
            Carving(group, new Vector3(-5.8f, Masons, 366.6f), 90f, "Mason's marks on the scaffold", "\"The old face still shows. Cut deeper.\"");
            Box("Sledge", group, 4.6f, 7.4f, 358.6f, 361.4f, Masons, Masons + 0.5f, wood);
            SunCityKit.Place("statue_head_new", group, new Vector3(6f, Masons + 0.5f, 360f), 120f, 1f, false);
            var fallen = SunCityKit.Place("statue_head_new", group, new Vector3(-15f, Masons + 0.2f, 377f), 140f, 1f);
            if (fallen != null) fallen.transform.rotation = Quaternion.Euler(-78f, 140f, 8f);
            foreach (var at in new[] { new Vector3(2f, Masons, 352f), new Vector3(-2f, Masons, 368f), new Vector3(10f, Masons, 370f), new Vector3(16f, Masons, 352f), new Vector3(14f, Masons, 376f) })
                Box("Squared Block", group, at.x - 1.1f, at.x + 1.1f, at.z - 0.8f, at.z + 0.8f, Masons, Masons + 1.5f, stoneNew);
            Canopy(group, "Masons' Awning", 10f, 16f, 359.6f, 362.6f, Masons, 3f, cloth);
            Spawn(EnemyKind.GildedSpearmanName, -8.5f, 16f, 362f, 90f);
            Spawn(EnemyKind.GildedLevyName, 6f, Masons, 372f, 180f);

            // S3's gate in the terrace's west wall, barred on the terrace side.
            Box("Masons' Wall", group, -32f, -23.6f, 352f, 378f, Masons, 18f, stone);
            Box("Masons' Gate South", group, -24.4f, -23.6f, 346f, 348f, Masons, 18f, stone);
            Barred(group, "Masons' Gate", new Vector3(-24f, Masons, 350f), 4f, Vector3.right, MasonsGateKey, Masons);

            // 3D, the Spearman's Stair between its two great plinths, a falcon on each.
            Colonnade(group, "Terrace Colonnade West", -24f, -10f, 381f, 386f, Masons, Upper - Masons);
            Colonnade(group, "Terrace Colonnade East", 10f, 24f, 381f, 386f, Masons, Upper - Masons);
            Stairs(group, new Vector3(0f, Masons, 379.8f), new Vector3(0f, Upper, 386.2f), 8f);
            foreach (float x in new[] { -7f, 7f })
            {
                SunCityGround.Plateau("Great Plinth", group, SunCityGround.Rect(x - 3f, x + 3f, 378f, 390f), 17.5f, Masons, stoneNew, ashlar, ashlarBand, 0f, 0f, 0.9f);
                SunCityKit.Place("statue_king", group, new Vector3(x, 17.5f, 388f), 180f, 0.45f);
            }
            Spawn(EnemyKind.GildedSpearmanName, 6f, 17.5f, 384f, 270f);

            // S4's way down from the hall's chapel: a passage outside the hall's west wall, a stair, a vestibule, a door from inside.
            Box("Passage Wall West", group, -34f, -32f, 378f, 458f, Masons, 26f, stone);
            Box("Passage Wall East", group, -28f, -24f, 386f, 418f, Masons, 24f, stone);
            Box("Passage Wall East North", group, -28f, -23.5f, 418f, 440f, Upper, 26f, stone);
            SunCityGround.Ramp("West Passage", group, new Vector3(-30f, Upper, 390f), new Vector3(-30f, AisleTop, 440.4f), 4f, stone, ashlar, 4f);
            Stairs(group, new Vector3(-30f, Masons, 383.8f), new Vector3(-30f, Upper, 390.2f), 4f);
            Box("Vestibule Door South", group, -24.4f, -23.6f, 378f, 379.5f, Masons, 18f, stone);
            Box("Vestibule Door North", group, -24.4f, -23.6f, 382.5f, 386f, Masons, 18f, stone);
            Barred(group, "Vestibule Door", new Vector3(-24f, Masons, 381f), 3f, Vector3.left, SunDoorKey, Masons);
            // The passage is roofed in lengths with slits between, so its floor is shade crossed by bars of sun.
            for (float z = 392f; z < 440f; z += 9f)
                ShadeRoof(group, "Passage Roof", -32f, -28f, z, Mathf.Min(z + 7f, 440f), Upper, 4.5f);
            for (float z = 394f; z < 438f; z += 9f)
            {
                Deco(PrimitiveType.Cube, "Lamp Niche", group, new Vector3(-31.9f, Upper + 2.2f + (z - 390f) * 0.024f, z + 2f), new Vector3(0.2f, 0.6f, 0.6f), shade);
                Deco(PrimitiveType.Sphere, "Lamp", group, new Vector3(-31.75f, Upper + 2.1f + (z - 390f) * 0.024f, z + 2f), new Vector3(0.18f, 0.18f, 0.24f), clay);
            }
            Drift(group, new Vector3(-30f, Upper + 0.4f, 402f), 3f, 6f);
            AddShade(-32f, -24f, 378f, 384f);

            // 3E, the Warden's Court: the barque shrine for a pillar, porticoes either side, the caller on his dais.
            Paving(group, -18f, 18f, 386.4f, 410f, Upper, 3f, newA, newB, paveC, 0.04f);
            Box("Barque Shrine", group, -3f, 3f, 396f, 400f, Upper, Upper + 1.2f, stoneNew);
            var shrineBoat = SunCityKit.Place("reed_boat", group, new Vector3(0f, Upper + 1.2f, 398f), 90f, 0.9f, false);
            if (shrineBoat != null) foreach (var shown in shrineBoat.GetComponentsInChildren<Renderer>()) shown.sharedMaterial = gold;
            foreach (float side in new[] { -1f, 1f })
            {
                float inner = side * 18f, outer = side * 24f;
                for (float z = 389f; z <= 407f; z += 4.5f)
                {
                    SunCityKit.Place("column_base", group, new Vector3(inner, Upper, z), 180f, 0.6f, false);
                    var column = SunCityKit.Place("column_lotus", group, new Vector3(inner, Upper + 0.3f, z), 180f, 0.6f, false);
                    Solid(column, new Vector3(0f, 4.6f, 0f), new Vector3(2.1f, 10.2f, 2.1f));
                }
                ShadeRoof(group, "Side Portico Roof", Mathf.Min(inner, outer), Mathf.Max(inner, outer), 387.3f, 409f, Upper, 6f, false);
                AddShade(Mathf.Min(inner, outer), Mathf.Max(inner, outer), 387.3f, 409f);
            }
            Box("Caller's Dais", group, 13f, 17f, 402f, 406f, Upper, Upper + 1f, stoneNew);
            Spawn(EnemyKind.GildedWardenName, 0f, Upper, 404f, 180f);
            Spawn(EnemyKind.GildedLevyName, -7f, Upper, 396f, 180f);
            Spawn(EnemyKind.SunBlindName, 15f, Upper + 1f, 404f, 270f);

            // The pylon before the hall.
            var pylon = SunCityKit.Place("pylon_gate", group, new Vector3(0f, Upper, 414f), 180f, 1.5f, false);
            Solid(pylon, new Vector3(-5.35f, 5.8f, 0f), new Vector3(6.1f, 11.7f, 4.7f));
            Solid(pylon, new Vector3(5.35f, 5.8f, 0f), new Vector3(6.1f, 11.7f, 4.7f));
            Box("Pylon Wing West", group, -24f, -12.3f, 412.4f, 416f, Upper, 25f, stone);
            Box("Pylon Wing East", group, 12.3f, 24f, 412.4f, 416f, Upper, 25f, stone);
            Carving(group, new Vector3(5.2f, Upper, 409.4f), 180f, "Over the door of the hall", "\"Look on him and see.\"", "Below, by another hand: \"They look.\"");
        }

        // A row of columns along a terrace face carrying a roof slab back to the face: the roof is walked on from above and
        // thins out from below; the floor under it is shade.
        private static void Colonnade(Transform parent, string name, float west, float east, float front, float back, float floor, float height)
        {
            var group = BuildKit.Group(name, parent);
            float scale = height / 9.2f;
            for (float x = west + 1.5f; x <= east - 1f; x += 4f)
            {
                SunCityKit.Place("column_base", group, new Vector3(x, floor, front + 0.6f), 180f, scale, false);
                var column = SunCityKit.Place("column_lotus", group, new Vector3(x, floor + 0.5f * scale, front + 0.6f), 180f, scale, false);
                Solid(column, new Vector3(0f, 4.6f, 0f), new Vector3(2.1f, 10.2f, 2.1f));
            }
            Box(name + " Roof", group, west, east, front, back, floor + height - 0.4f, floor + height, stone);
            // The roof's front: an architrave over the columns and a cornice standing proud of it, broken where blocks fell.
            Cornice(group, west, east, front, floor + height, floor);
            AddShade(west, east, front + 0.4f, back);
        }

        private static void HallOfColumns(AreaDirector director)
        {
            var group = BuildKit.Group("Hall Of Columns", world);
            // The floor is shade, dark and cool; where the roof is open the sun lies on it, pale. The aisles stand 1.2 m up.
            SunCityGround.Plateau("Hall Floor", group, SunCityGround.Rect(-24f, 24f, HallSouth, HallNorth), HallFloor, 12f, hallFloor, ashlar, null, 0f);
            foreach (float side in new[] { -1f, 1f })
            {
                float inner = side * 12f, outer = side * 22f;
                SunCityGround.Plateau("Raised Aisle", group, SunCityGround.Rect(Mathf.Min(inner, outer), Mathf.Max(inner, outer), HallSouth, HallNorth - 1f), AisleTop, HallFloor - 0.5f, hallFloor, ashlar, null, 0f);
                foreach (float z in new[] { 428f, 448f, 468f })
                    Stairs(group, new Vector3(side * 12.1f, AisleTop, z), new Vector3(side * 9.8f, HallFloor, z), 3f);
            }
            foreach (var lit in HallLight)
            {
                float y = lit.y - lit.x > 20f ? HallFloor : HallFloor;
                var patch = Deco(PrimitiveType.Cube, "Sunlit Floor", group, new Vector3((lit.x + lit.y) * 0.5f, y + 0.012f, (lit.z + lit.w) * 0.5f), new Vector3(lit.y - lit.x, 0.03f, lit.w - lit.z), stoneNew);
                patch.shadowCastingMode = ShadowCastingMode.Off;
                if (lit.y - lit.x > 20f)
                    foreach (float side in new[] { -1f, 1f })
                    {
                        var aisle = Deco(PrimitiveType.Cube, "Sunlit Aisle", group, new Vector3(side * 17f, AisleTop + 0.012f, (lit.z + lit.w) * 0.5f), new Vector3(10f, 0.03f, lit.w - lit.z), stoneNew);
                        aisle.shadowCastingMode = ShadowCastingMode.Off;
                    }
            }
            Box("Hall Wall West", group, -23.5f, -22f, HallSouth, 446.5f, HallFloor, 31f, stone);
            Box("Hall Wall West North", group, -23.5f, -22f, 449.5f, HallNorth, HallFloor, 31f, stone);
            Box("Sun Door Lintel", group, -23.5f, -22f, 446.5f, 449.5f, 20.4f, 31f, stone);
            Box("Hall Wall East", group, 22f, 23.5f, HallSouth, HallNorth, HallFloor, 31f, stone);
            Box("Hall North Wall West", group, -22f, -4f, HallNorth - 1f, HallNorth, HallFloor, 27f, stone);
            Box("Hall North Wall East", group, 4f, 22f, HallNorth - 1f, HallNorth, HallFloor, 27f, stone);
            foreach (float x in new[] { -15f, -5f, 5f, 15f })
                foreach (float z in new[] { 423f, 433f, 443f, 453f, 463f, 473f })
                {
                    // The east aisle's column at 443 is the one that fell.
                    if (x > 10f && z == 443f) continue;
                    float floor = Mathf.Abs(x) > 10f ? AisleTop : HallFloor;
                    SunCityKit.Place("column_base", group, new Vector3(x, floor, z), 180f, 1f, false);
                    var column = SunCityKit.Place("column_lotus", group, new Vector3(x, floor + 0.5f, z), 180f, 1f, false);
                    // Round the shaft, not the spread of the capital: the aisles stay as wide as they look.
                    Solid(column, new Vector3(0f, 4.6f, 0f), new Vector3(2.1f, 10.2f, 2.1f));
                }
            // The fallen column, lying across the east aisle and cutting it in two.
            var drum = Deco(PrimitiveType.Cylinder, "Fallen Column", group, new Vector3(17f, AisleTop + 1f, 444f), new Vector3(2f, 8.1f, 2f), stone, new Vector3(90f, Mathf.Atan2(8f, 14f) * Mathf.Rad2Deg, 0f));
            var drumBox = drum.gameObject.AddComponent<BoxCollider>();
            drumBox.size = new Vector3(1f, 2f, 1f);
            Rubble(group, 13f, 21f, 440f, 448f, AisleTop, 10);

            // The roof is drawn only as its shadow, so the camera sees under it. Its holes, the shade the sun-blind hunt by
            // and the swell reads all come from the one list: each patch of light lies north of its hole by the lean of the noon sun.
            float lean = LookBuilder.NoonShadowLength(HallRoof);
            var bars = new List<Vector2>();
            foreach (var lit in HallLight)
                if (lit.y - lit.x > 20f) bars.Add(new Vector2(lit.z, lit.w));
            float south = HallSouth;
            for (int i = 0; i <= bars.Count; i++)
            {
                float north = i < bars.Count ? bars[i].x : HallNorth;
                foreach (float side in new[] { -1f, 1f })
                {
                    // The nave is open for its whole length.
                    float inner = side * 2f, outer = side * 23f;
                    var roof = BuildKit.Block("Hall Roof (shadow only)", group, new Vector3((inner + outer) * 0.5f, HallFloor + HallRoof + 0.25f, (south + north) * 0.5f - lean),
                        new Vector3(Mathf.Abs(outer - inner), 0.5f, north - south), stone);
                    Object.DestroyImmediate(roof.GetComponent<Fadeable>());
                    Object.DestroyImmediate(roof.GetComponent<Collider>());
                    roof.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    AddShade(Mathf.Min(inner, side * 22f), Mathf.Max(inner, side * 22f), south, north);
                }
                if (i < bars.Count) south = bars[i].y;
            }

            // The mirrors and the sun door: M1 turned due west throws the sun on the disc over the door, and it opens (S4).
            things.Add(FireMechanicsKit.Mirror(group, new Vector3(-8f, HallFloor, 449.6f), 0f));
            things.Add(FireMechanicsKit.Mirror(group, new Vector3(8f, HallFloor, 428f), 0f));
            var sunDoor = BuildKit.Block("Sun Door", group, new Vector3(-22.75f, (AisleTop + 20.4f) * 0.5f, 448f), new Vector3(1.5f, 20.4f - AisleTop, 3f), bronze);
            // On the door's north jamb, not in the doorway, so the way through is clear once it opens; M1 stands on its line.
            FireMechanicsKit.Target(group, new Vector3(-21.85f, HallFloor + 1.4f, 449.9f), 90f, sunDoor);
            // The doorway's sill at the aisle's height, so the way through to the chapel is level.
            Floor("Sun Door Sill", group, -23.7f, -21.8f, 446.5f, 449.5f, AisleTop, stoneNew, 1.4f);
            Deco(PrimitiveType.Cylinder, "Sun Door Disc", group, new Vector3(-21.9f, 19.6f, 448f), new Vector3(1.4f, 0.05f, 1.4f), gold, new Vector3(0f, 0f, 90f));
            // The west chapel behind it, and its words.
            SunCityGround.Plateau("West Chapel", group, SunCityGround.Rect(-32f, -23.5f, 440f, 456f), AisleTop, 12f, hallFloor, ashlar, null, 0f);
            Box("Chapel Wall North", group, -34f, -22f, 456f, 458f, AisleTop, 27f, stone);
            AddShade(-32f, -22f, 440f, 456f);
            // The chapel: an altar under a hole in its roof, and two columns of the older stone.
            ShadeRoof(group, "Chapel Roof", -32f, -22f, 440f, 446f, AisleTop, 5f, false);
            ShadeRoof(group, "Chapel Roof", -32f, -22f, 450f, 456f, AisleTop, 5f, false);
            Box("Altar", group, -31f, -28.6f, 446.6f, 449.4f, AisleTop, AisleTop + 1.1f, stoneNew);
            Deco(PrimitiveType.Cylinder, "Altar Bowl", group, new Vector3(-29.8f, AisleTop + 1.16f, 448f), new Vector3(0.9f, 0.06f, 0.9f), clay);
            Deco(PrimitiveType.Sphere, "Altar Lamp", group, new Vector3(-29.8f, AisleTop + 1.3f, 448f), new Vector3(0.3f, 0.22f, 0.3f), ember);
            foreach (float z in new[] { 443f, 453f })
            {
                SunCityKit.Place("column_base", group, new Vector3(-26f, AisleTop, z), 180f, 0.5f, false);
                var older = SunCityKit.Place("column_lotus", group, new Vector3(-26f, AisleTop + 0.25f, z), 180f, 0.5f, false);
                Solid(older, new Vector3(0f, 4.6f, 0f), new Vector3(2.1f, 10.2f, 2.1f));
            }
            Carving(group, new Vector3(-30.6f, AisleTop, 452f), 90f, "The chapel's lintel", "\"The door opens to the sun. He made it so.\"", "\"Under the gold the lintel is older: a lamp, and a hand holding it out.\"");

            Spawn(EnemyKind.SunBlindName, 0f, HallFloor, 428f, 180f);
            Spawn(EnemyKind.SunBlindName, -17f, AisleTop, 438f, 90f);
            Spawn(EnemyKind.SunBlindName, 17f, AisleTop, 458f, 270f);
            Spawn(EnemyKind.SunBlindName, -6f, HallFloor, 460f, 180f);
            Spawn(EnemyKind.GildedSpearmanName, 7f, HallFloor, 474f, 200f);
        }

        // ------------------------------------------------------------------ the champion's bowl and the gate

        private static void LastGate(AreaDirector director)
        {
            var group = BuildKit.Group("Last Gate", world);
            // 3G, the porch, and F5.
            SunCityGround.Plateau("Porch", group, SunCityGround.Rect(-24f, 24f, HallNorth, 486.2f), Upper, 12f, stone, ashlar, null, 0f);
            Paving(group, -22f, 22f, HallNorth, 486f, Upper, 3f, newA, newB, paveC, 0.04f);
            Box("Porch Wall West", group, -26f, -22f, HallNorth, 486f, Upper, 24f, stone);
            Box("Porch Wall East", group, 22f, 26f, HallNorth, 486f, Upper, 24f, stone);
            Rest(group, director, new Vector3(8f, Upper, 482f));

            // The bowl: an oval sunk 1.5 m in the stone, three steps down from the porch the width of it, a stair up to the gate.
            var oval = SunCityGround.Oval(new Vector2(0f, 505f), 18f, 15f, 64);
            for (int i = 0; i < oval.Length; i++)
                if (oval[i].y < 505f && Mathf.Abs(oval[i].x) < 10.5f) oval[i].y = 486.3f;
            SunCityGround.Ring("Bowl Rim", group, oval, new Vector4(-26f, 26f, 486.2f, 522f), Rim, BowlFloor, stone, goldVein);
            SunCityGround.Plateau("Bowl Floor", group, SunCityGround.Rect(-20f, 20f, 486f, 521f), BowlFloor, 12f, newB, ashlar, null, 0f);
            Paving(group, -16f, 16f, 491f, 518f, BowlFloor, 2.6f, newA, newB, goldVein, 0.06f);
            Stairs(group, new Vector3(0f, Rim, 486.4f), new Vector3(0f, BowlFloor, 490.6f), 20.6f);
            GoldLine(group, -10.3f, 10.3f, 486.3f, 486.6f, Rim);
            Stairs(group, new Vector3(0f, BowlFloor, 516f), new Vector3(0f, Rim, 519.9f), 8f);
            Box("Rim Wall West", group, -26f, -21f, 486f, 522f, Rim, 22f, stone);
            Box("Rim Wall East", group, 21f, 26f, 486f, 522f, Rim, 22f, stone);
            // His fallen predecessors, cast gold, kneeling on plinths; two broken drums.
            foreach (var at in new[] { new Vector3(-10f, BowlFloor, 496f), new Vector3(10f, BowlFloor, 496f), new Vector3(-10f, BowlFloor, 514f), new Vector3(10f, BowlFloor, 514f) })
            {
                Box("Plinth", group, at.x - 0.9f, at.x + 0.9f, at.z - 0.9f, at.z + 0.9f, BowlFloor, BowlFloor + 1.2f, stoneDark);
                float toward = at.x < 0f ? 1f : -1f;
                var top = at + Vector3.up * 0.2f;
                Deco(PrimitiveType.Cube, "Cast Legs", group, top + new Vector3(0f, 1.14f, 0f), new Vector3(0.75f, 0.28f, 0.6f), gold);
                Deco(PrimitiveType.Cube, "Cast Body", group, top + new Vector3(toward * 0.12f, 1.62f, 0f), new Vector3(0.4f, 0.75f, 0.6f), gold, new Vector3(0f, 0f, toward * -16f));
                Deco(PrimitiveType.Sphere, "Cast Head", group, top + new Vector3(toward * 0.3f, 2.12f, 0f), new Vector3(0.36f, 0.4f, 0.36f), gold);
            }
            Disc("Broken Drum", group, new Vector3(-14f, BowlFloor, 505f), 1.2f, 1.6f, stone);
            Disc("Broken Drum", group, new Vector3(14f, BowlFloor, 505f), 1.2f, 1.6f, stone);
            SunCityKit.Place("brazier", group, new Vector3(-17f, Rim, 488.5f));
            SunCityKit.Place("brazier", group, new Vector3(17f, Rim, 488.5f));

            // The gate of noon: a wall fourteen metres high on the north rim, bronze doors, a band of gold over them.
            const float dz = 22f, dy = 12f;
            Box("Gate Wall West", group, -26f, -3.2f, 500f + dz, 503.5f + dz, Rim, 18f + dy, stone);
            Box("Gate Wall East", group, 3.2f, 26f, 500f + dz, 503.5f + dz, Rim, 18f + dy, stone);
            Box("Gate Lintel", group, -3.2f, 3.2f, 500f + dz, 503.5f + dz, 12.4f + dy, 18f + dy, stone);
            Box("Bronze Doors", group, -3.2f, 3.2f, 499.75f + dz, 500.4f + dz, Rim, 12.4f + dy, bronze);
            Deco(PrimitiveType.Cube, "Door Seam", group, new Vector3(0f, 8.2f + dy, 499.72f + dz), new Vector3(0.1f, 8.4f, 0.06f), shade);
            foreach (float up in new[] { 5.2f, 8.2f, 11.2f })
                Deco(PrimitiveType.Cube, "Door Strap", group, new Vector3(0f, up + dy, 499.7f + dz), new Vector3(6.3f, 0.32f, 0.08f), gold);
            Deco(PrimitiveType.Cube, "Door Band", group, new Vector3(0f, 12.9f + dy, 499.9f + dz), new Vector3(8.4f, 0.7f, 0.3f), gold);
            Deco(PrimitiveType.Cylinder, "Door Sun", group, new Vector3(0f, 14.6f + dy, 499.9f + dz), new Vector3(2.2f, 0.12f, 2.2f), gold, new Vector3(90f, 0f, 0f));
            foreach (float x in new[] { -3.9f, 3.9f })
                Deco(PrimitiveType.Cube, "Door Jamb", group, new Vector3(x, 8.2f + dy, 499.6f + dz), new Vector3(0.9f, 8.4f, 0.5f), stoneNew);
            Carving(group, new Vector3(6.4f, Rim, 498.6f + dz), 180f, "The gate of noon", "\"Noon is the king's hour. There is no other hour.\"");

            var gate = BuildKit.Group("Gate Of Noon", group).gameObject.AddComponent<GateOfNoon>();
            gate.transform.position = new Vector3(0f, Rim, 498.8f + dz);
            gate.Prompt = "Open the gate of noon";
            gate.Range = 3f;
            gate.LeadsTo = "SunCourt";
            gate.Configure(director, EnemyKind.ChampionName);
            things.Add(gate);

            // He keeps this ground as the Gate Captain keeps his: he notices nobody south of the gold line and follows nobody over it.
            Spawn(EnemyKind.ChampionName, 0f, BowlFloor, 508f, 180f).HoldGround(-18f, 18f, ChampionsLine + 0.5f, 520f);
        }

        // ------------------------------------------------------------------ the second pass's pieces

        private static void AddShade(float west, float east, float south, float north) => shadeList.Add(new Vector4(west, east, south, north));

        private static void Bury(Enemy enemy, float within = 3.5f) => buried.Add((enemy, within));

        // A roof drawn only as its shadow, this far over a floor: what is under it is seen, and is shade, its rectangle moved
        // north by the lean of the noon sun as the shadow is.
        private static void ShadeRoof(Transform parent, string name, float west, float east, float south, float north, float floor, float height, bool register = true)
        {
            var roof = BuildKit.Block(name + " (shadow only)", parent, new Vector3((west + east) * 0.5f, floor + height + 0.2f, (south + north) * 0.5f), new Vector3(east - west, 0.4f, north - south), stone);
            Object.DestroyImmediate(roof.GetComponent<Fadeable>());
            Object.DestroyImmediate(roof.GetComponent<Collider>());
            roof.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            if (register) AddShade(west, east, south + LookBuilder.NoonShadowLength(height), north + LookBuilder.NoonShadowLength(height));
        }

        // A mat on four posts over a floor: drawn, and thinning out when it is between the camera and the player; the shade
        // it throws is registered for the swell and the sun-blind.
        private static void Canopy(Transform parent, string name, float west, float east, float south, float north, float floor, float height, Material material, bool posts = true)
        {
            var group = BuildKit.Group(name, parent);
            if (posts)
                foreach (var (x, z) in new[] { (west + 0.15f, south + 0.15f), (east - 0.15f, south + 0.15f), (west + 0.15f, north - 0.15f), (east - 0.15f, north - 0.15f) })
                    Deco(PrimitiveType.Cube, "Post", group, new Vector3(x, floor + height * 0.5f, z), new Vector3(0.14f, height, 0.14f), wood);
            var mat = BuildKit.Block("Mat", group, new Vector3((west + east) * 0.5f, floor + height, (south + north) * 0.5f), new Vector3(east - west + 0.3f, 0.08f, north - south + 0.3f), material);
            Object.DestroyImmediate(mat.GetComponent<Collider>());
            if (posts)
            {
                // An awning on posts is drawn as its frame and a few slats; the mat itself only as its shadow, so the
                // camera always sees who stands under it and the shade on the floor is what reads.
                Object.DestroyImmediate(mat.GetComponent<Fadeable>());
                mat.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                float y = floor + height + 0.05f;
                Deco(PrimitiveType.Cube, "Rail", group, new Vector3((west + east) * 0.5f, y, south + 0.15f), new Vector3(east - west + 0.2f, 0.12f, 0.12f), wood);
                Deco(PrimitiveType.Cube, "Rail", group, new Vector3((west + east) * 0.5f, y, north - 0.15f), new Vector3(east - west + 0.2f, 0.12f, 0.12f), wood);
                Deco(PrimitiveType.Cube, "Rail", group, new Vector3(west + 0.15f, y, (south + north) * 0.5f), new Vector3(0.12f, 0.12f, north - south), wood);
                Deco(PrimitiveType.Cube, "Rail", group, new Vector3(east - 0.15f, y, (south + north) * 0.5f), new Vector3(0.12f, 0.12f, north - south), wood);
                for (float x = west + 0.9f; x < east - 0.5f; x += 1.1f)
                    Deco(PrimitiveType.Cube, "Slat", group, new Vector3(x, y + 0.06f, (south + north) * 0.5f), new Vector3(0.22f, 0.04f, north - south + 0.2f), material);
            }
            float lean = LookBuilder.NoonShadowLength(height);
            AddShade(west, east, south + lean, north + lean);
        }

        // A ladder up a face between a foot and a head; with a key it lies on the top until kicked down from there.
        private static void Ladder(Transform parent, string name, Vector3 foot, Vector3 head, string key)
        {
            var group = BuildKit.Group(name, parent);
            var model = BuildKit.Group("Ladder Model", group);
            float length = (head - foot).magnitude + 0.4f;
            foreach (float x in new[] { -0.28f, 0.28f })
            {
                var rail = Deco(PrimitiveType.Cube, "Rail", model, Vector3.zero, Vector3.one, wood);
                rail.transform.localPosition = new Vector3(x, length * 0.5f, 0f);
                rail.transform.localScale = new Vector3(0.08f, length, 0.08f);
            }
            for (float up = 0.3f; up < length; up += 0.36f)
            {
                var rung = Deco(PrimitiveType.Cube, "Rung", model, Vector3.zero, Vector3.one, wood);
                rung.transform.localPosition = new Vector3(0f, up, 0f);
                rung.transform.localScale = new Vector3(0.56f, 0.05f, 0.06f);
            }
            foreach (Transform child in model) child.gameObject.isStatic = false;
            Vector3 flat = head - foot;
            flat.y = 0f;
            var standing = Quaternion.LookRotation(flat.sqrMagnitude > 0.01f ? flat.normalized : Vector3.forward, Vector3.up);
            var leaning = Quaternion.FromToRotation(Vector3.up, (head - foot).normalized) * standing;
            Vector3 lyingAt = head + new Vector3(0f, 0.06f, 0f);
            var lying = Quaternion.FromToRotation(Vector3.up, flat.sqrMagnitude > 0.01f ? flat.normalized : Vector3.right);
            var ladder = group.gameObject.AddComponent<Clover.World.Ladder>();
            group.position = new Vector3((foot.x + head.x) * 0.5f, head.y, (foot.z + head.z) * 0.5f);
            ladder.Foot = foot + Vector3.up * 0.05f;
            ladder.Head = head + Vector3.up * 0.05f;
            ladder.DownKey = key;
            ladder.Range = 2f;
            ladder.Configure(playerTransform, model, lyingAt, lying.eulerAngles, foot, leaning.eulerAngles);
            model.SetPositionAndRotation(string.IsNullOrEmpty(key) ? foot : lyingAt, string.IsNullOrEmpty(key) ? leaning : lying);
            things.Add(ladder);
        }

        // A door barred on one side: the door block, a beam across it on the bar's side, and the bar itself.
        private static void Barred(Transform parent, string name, Vector3 foot, float width, Vector3 barSide, string key, float floor)
        {
            var group = BuildKit.Group(name, parent);
            group.position = foot;
            bool alongX = Mathf.Abs(barSide.z) > 0.5f;
            var size = alongX ? new Vector3(width, 3.6f, 0.6f) : new Vector3(0.6f, 3.6f, width);
            var door = BuildKit.Block(name + " Leaf", group, foot + Vector3.up * 1.8f, size, wood);
            Object.DestroyImmediate(door.GetComponent<Fadeable>());
            var barSize = alongX ? new Vector3(width + 0.4f, 0.22f, 0.22f) : new Vector3(0.22f, 0.22f, width + 0.4f);
            var bar = Deco(PrimitiveType.Cube, "Bar", group, foot + Vector3.up * 1.6f + barSide * 0.42f, barSize, stoneDark);
            bar.transform.SetParent(door.transform, true);
            var barred = group.gameObject.AddComponent<BarredDoor>();
            barred.BarSide = barSide;
            barred.OpenKey = key;
            barred.Range = 2.4f;
            barred.Configure(door, null);
            things.Add(barred);
        }

        // The middle of a street, walked darker than its edges.
        private static void Trodden(Transform parent, float fromX, float fromZ, float toX, float toZ, float width, float top)
        {
            var from = new Vector3(fromX, top + 0.006f, fromZ);
            var to = new Vector3(toX, top + 0.006f, toZ);
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
