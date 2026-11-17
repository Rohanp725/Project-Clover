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
    /// A yard to look at the Fire land in before its city is built: a patch of cracked mud under the white noon, a
    /// wall, a gate, a roofed porch that throws real shade, a burial niche with a wick dormant beside it, a brazier to
    /// rest at; the player with fists and no element; one of each of the land's people and each rank of the gilded,
    /// spaced out; and the big pieces of the scenery kit standing where the game's light falls on them. It is not in
    /// the list of scenes the game is built from.
    /// </summary>
    public static class FireYardBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/FireYard.unity";

        // The porch: two columns and a roof that is drawn only as its shadow, so the camera sees under it.
        public const float PorchWest = 22f, PorchEast = 32f, PorchSouth = 27f, PorchNorth = 33f, PorchRoof = 10.5f;

        /// <summary>The shade the porch roof throws on the ground: its own rectangle, moved north by the noon sun.</summary>
        public static Vector4 PorchShade
        {
            get
            {
                float north = LookBuilder.NoonShadowLength(PorchRoof);
                return new Vector4(PorchWest, PorchEast, PorchSouth + north, PorchNorth + north);
            }
        }

        // The east awning: a shade island in the open for the swell.
        public const float AwningWest = 52f, AwningEast = 58f, AwningSouth = 14f, AwningNorth = 19f, AwningRoof = 5f;

        public static Vector4 AwningShade
        {
            get
            {
                float north = LookBuilder.NoonShadowLength(AwningRoof);
                return new Vector4(AwningWest, AwningEast, AwningSouth + north, AwningNorth + north);
            }
        }

        /// <summary>The warden's gate is spared the swell, as an elite's arena is: a calm rectangle to try the rule on.</summary>
        public static readonly Vector4 GateCalm = new Vector4(-4f, 8f, 32f, 40f);

        /// <summary>The noon's rhythm in the yard: the first swell 25 seconds in, then one a minute.</summary>
        public static HeatSchedule YardHeat => new HeatSchedule(25f, 60f, 5f, 15f);

        [MenuItem("Clover/Build/Fire Yard")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the fire yard")) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = BuildKit.Group("World");

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, 0f), "");
            if (built.Player == null) return;
            LookBuilder.Apply(LookBuilder.Noon, built.Camera);
            // The owner's decision of 2026-11-17: the player enters this land with fists and no element.
            built.Player.GetComponent<PlayerCombat>().StartingWeapon = Weapons.FistsName;
            built.Player.GetComponent<PlayerElement>().StartsWithNone = true;

            // The ground: a slab to stand on, and the kit's cracked mud laid over it tile by tile.
            // The slab is only stood on: what is seen is the tiles, and beyond them a plain of the same mud.
            var ground = BuildKit.Block("Ground", world, new Vector3(0f, -0.5f, 30f), new Vector3(160f, 1f, 200f), SunCityKit.Mud);
            Object.DestroyImmediate(ground.GetComponent<Fadeable>());
            ground.GetComponent<Renderer>().enabled = false;
            var plain = BuildKit.Block("Far Ground", world, new Vector3(0f, -0.65f, 30f), new Vector3(160f, 1f, 200f), SunCityKit.Mud);
            Object.DestroyImmediate(plain.GetComponent<Fadeable>());
            Object.DestroyImmediate(plain.GetComponent<Collider>());
            var mud = BuildKit.Group("Mud", world);
            for (int x = -6; x < 20; x++)
                for (int z = -3; z < 11; z++)
                    SunCityKit.Place("mud_tile", mud, new Vector3(x * 4f + 2f, -0.19f, z * 4f + 2f), (x * 7 + z * 13) % 4 * 90f, 1f, false);

            // The north side: the pylon gate with the wall running off either way; a breach in the west run.
            var north = BuildKit.Group("North Wall", world);
            SunCityKit.Place("pylon_gate", north, new Vector3(2f, 0f, 41f), 180f, 1f, false);
            foreach (float x in new[] { -3.3f, 7.3f })
            {
                // The towers either side of the opening are solid; the way between them is open.
                var tower = new GameObject("Pylon Tower");
                tower.transform.SetParent(north, false);
                tower.transform.position = new Vector3(x, 0f, 41f);
                var box = tower.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 5f, 0f);
                box.size = new Vector3(6f, 10f, 3f);
            }
            SunCityKit.Place("wall_section", north, new Vector3(-9.3f, 0f, 41f));
            SunCityKit.Place("wall_broken", north, new Vector3(-15.3f, 0f, 41f));
            SunCityKit.Place("wall_section", north, new Vector3(-21.3f, 0f, 41f));
            SunCityKit.Place("wall_section", north, new Vector3(13.3f, 0f, 41f));
            SunCityKit.Place("wall_section", north, new Vector3(19.3f, 0f, 41f));
            SunCityKit.Place("wall_section", north, new Vector3(25.3f, 0f, 41f));
            SunCityKit.Place("wall_section", north, new Vector3(31.3f, 0f, 41f));
            // West and east: mud brick, tall on the west where the niches are; low on the south, the camera's side.
            BuildKit.Block("Wall West", world, new Vector3(-21f, 1.6f, 17f), new Vector3(2f, 3.2f, 48f), SunCityKit.Brick);
            // The east wall has a gap in it: through it is the trial ground of the second pass (EastYard below).
            BuildKit.Block("Wall East", world, new Vector3(35.4f, 1f, 0.5f), new Vector3(2f, 2f, 15f), SunCityKit.Brick);
            BuildKit.Block("Wall East North", world, new Vector3(35.4f, 1f, 27.5f), new Vector3(2f, 2f, 27f), SunCityKit.Brick);
            BuildKit.Block("Wall South", world, new Vector3(7.2f, 0.5f, -8f), new Vector3(58.4f, 1f, 2f), SunCityKit.BrickDark);

            // The porch. Its roof is drawn as a shadow only: the shade under it is real, and the rule the sun-blind
            // hunt by is given the same rectangle, moved north as the sun moves it.
            var porch = BuildKit.Group("Porch", world);
            foreach (float x in new[] { PorchWest + 1f, PorchEast - 1f })
            {
                SunCityKit.Place("column_base", porch, new Vector3(x, 0f, PorchSouth + 1f));
                SunCityKit.Place("column_lotus", porch, new Vector3(x, 0.5f, PorchSouth + 1f));
            }
            var roof = BuildKit.Block("Porch Roof (shadow only)", porch, new Vector3((PorchWest + PorchEast) * 0.5f, PorchRoof + 0.25f, (PorchSouth + PorchNorth) * 0.5f),
                new Vector3(PorchEast - PorchWest, 0.5f, PorchNorth - PorchSouth), SunCityKit.Stone);
            Object.DestroyImmediate(roof.GetComponent<Fadeable>());
            Object.DestroyImmediate(roof.GetComponent<Collider>());
            roof.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            // A second roof drawn only as its shadow, east of the gap: a shade island for the noon's swell.
            var awning = BuildKit.Block("East Awning (shadow only)", world, new Vector3((AwningWest + AwningEast) * 0.5f, AwningRoof + 0.25f, (AwningSouth + AwningNorth) * 0.5f),
                new Vector3(AwningEast - AwningWest, 0.5f, AwningNorth - AwningSouth), SunCityKit.Stone);
            Object.DestroyImmediate(awning.GetComponent<Fadeable>());
            Object.DestroyImmediate(awning.GetComponent<Collider>());
            awning.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            var zones = new GameObject("Sunlight").AddComponent<SunlightZones>();
            zones.Configure(true, new[] { PorchShade, AwningShade }, null);

            // The west wall: two burial niches with their dead still in them, and between them one that is not still.
            var niches = BuildKit.Group("Niches", world);
            SunCityKit.Place("burial_niche", niches, new Vector3(-19.2f, 0f, 16.5f), 90f);
            SunCityKit.Place("burial_niche", niches, new Vector3(-19.2f, 0f, 22.5f), 90f);
            SunCityKit.Place("sarcophagus", niches, new Vector3(-16.5f, 0f, 28f), 200f);

            // Things to look at in the game's light: the seated king with his recut head on and the old god's head
            // lying beside him; an obelisk; and far off over the wall, the barque.
            var sights = BuildKit.Group("Sights", world);
            SunCityKit.Place("statue_king", sights, new Vector3(-11f, 0f, 34.5f));
            SunCityKit.Place("statue_head_new", sights, new Vector3(-11f, 8f, 34.5f - 1.1f), 180f, 1f, false);
            var oldHead = SunCityKit.Place("statue_head_old", sights, new Vector3(-5.6f, 0.3f, 33f), 150f);
            oldHead.transform.rotation = Quaternion.Euler(-70f, 150f, 12f);
            SunCityKit.Place("obelisk", sights, new Vector3(-30f, 0f, 62f), 180f, 1f, false);
            SunCityKit.Place("barque", sights, new Vector3(44f, 0f, 82f), 200f, 0.73f, false);
            SunCityKit.Place("reed_boat", sights, new Vector3(15f, 0f, 3f), 140f);
            SunCityKit.Place("market_awning", sights, new Vector3(-15f, 0f, 2f), 180f, 1f, false);
            SunCityKit.Place("house_block", sights, new Vector3(42f, 0f, 30f), 180f, 1f, false);

            // The people of the land, spaced out so each can be met alone.
            var enemies = new List<Enemy>();
            var kit = EnemyBuilder.NewKit(built.Player.transform, built.Camera.transform.rotation);
            var enemyRoot = BuildKit.Group("Enemies");
            Enemy Spawn(string kind, float x, float z, float yaw)
            {
                var enemy = EnemyBuilder.Build(kind, new Vector3(x, 0.05f, z), yaw, kit, enemyRoot);
                enemies.Add(enemy);
                return enemy;
            }
            Spawn(EnemyKind.ThirstName, -6f, 10f, 180f);
            // One on its knees at a dry pool with its back to the way in: it does not turn until someone is close.
            Spawn(EnemyKind.ThirstName, 12f, 5f, 20f).MakeDormant(3f, "thirst_kneel_loop");
            Spawn(EnemyKind.WickName, 9f, 17f, 180f);
            // Standing between the niches as the dead in them stand; it steps out when the player passes.
            Spawn(EnemyKind.WickName, -19f, 19.5f, 90f).MakeDormant(3.5f);
            Spawn(EnemyKind.GildedLevyName, -14f, 19f, 180f);
            Spawn(EnemyKind.GildedSpearmanName, -7f, 29f, 180f);
            // The warden keeps the gate.
            Spawn(EnemyKind.GildedWardenName, 2f, 37f, 180f);
            // In the sun, a little south of the porch's shade.
            Spawn(EnemyKind.SunBlindName, 27f, 22f, 180f);

            // The east yard, through the gap: the second pass's people, each where its own trick can be tried.
            // The alley of the dead: two wicks dormant side by side, and the ashen coming down it.
            var dormantWicks = new[] { Spawn(EnemyKind.WickName, 44f, 3f, 270f), Spawn(EnemyKind.WickName, 44f, 5f, 270f) };
            foreach (var wick in dormantWicks) wick.MakeDormant(2.5f);
            Spawn(EnemyKind.AshenName, 47f, 9f, 270f);
            Spawn(EnemyKind.AshenName, 50f, 4f, 270f);
            // Open mud with a jar standing in it: one of the thirsting buried near it, one awake beyond.
            var buried = Spawn(EnemyKind.ThirstName, 62f, 8f, 270f);
            Spawn(EnemyKind.ThirstName, 70f, 14f, 250f);
            // Three levies who lock shields.
            var levies = new[] { Spawn(EnemyKind.GildedLevyName, 47f, 30f, 180f), Spawn(EnemyKind.GildedLevyName, 48.3f, 30f, 180f), Spawn(EnemyKind.GildedLevyName, 49.6f, 30f, 180f) };

            var start = BuildKit.Group("Start Point", world);
            start.position = new Vector3(0f, 0.05f, 0f);
            var director = new GameObject("Area Director").AddComponent<AreaDirector>();
            var strikeOutline = WorldBars.Marker("Storm Mark Outline", director.transform, 0f, kit.Marker);
            var strikeFill = WorldBars.Marker("Storm Mark Fill", director.transform, 0f, kit.Marker);
            director.Configure(built.Player.GetComponent<PlayerController>(), enemies.ToArray(), start, strikeOutline, strikeFill);
            kit.Turns.Configure(enemies.ToArray());

            // The second pass: each kind's own trick, the floor its fire leaves, the noon's swell and the land's HUD.
            FireMechanicsKit.Equip(enemies, kit);
            FireMechanicsKit.Bury(buried);
            var east = BuildKit.Group("East Yard", world);
            FireMechanicsKit.WallOfShields("Shield Wall", east, levies);
            FireMechanicsKit.Jar(east, new Vector3(64f, 0f, 10f), director);
            FireMechanicsKit.Jar(east, new Vector3(66.5f, 0f, 16f), director);
            FireMechanicsKit.Jar(east, new Vector3(41f, 0f, 12f), director);
            FireMechanicsKit.Floor(world, built.Player, enemies);
            var landHud = FireMechanicsKit.Hud();
            FireMechanicsKit.Heat(director.gameObject, built.Player, YardHeat, new[] { GateCalm }, enemies, landHud);
            var land = director.gameObject.AddComponent<LandReturn>();
            land.Configure(director, built.Player.GetComponent<PlayerDefence>());
            land.ConfigureHud(landHud);
            // A mirror on a stand in the north-east corner, and across from it a sun-disc over a barred door.
            var mirror = FireMechanicsKit.Mirror(east, new Vector3(70f, 0f, 34f), 270f);
            var door = BuildKit.Block("Sun Door", east, new Vector3(70f, 1.5f, 40.5f), new Vector3(3f, 3f, 0.8f), SunCityKit.BrickDark);
            BuildKit.Block("Sun Door Wall West", east, new Vector3(66f, 1.5f, 40.5f), new Vector3(5f, 3f, 0.8f), SunCityKit.Brick);
            BuildKit.Block("Sun Door Wall East", east, new Vector3(74f, 1.5f, 40.5f), new Vector3(5f, 3f, 0.8f), SunCityKit.Brick);
            FireMechanicsKit.Target(east, new Vector3(64f, 1.4f, 39.9f), 180f, door);

            // A brazier to rest at. With no element there is no tree to open: the fire says so.
            var brazier = SunCityKit.Place("brazier", world, new Vector3(4f, 0f, -3f));
            brazier.name = "Rest Brazier";
            var lightObject = new GameObject("Fire Light");
            lightObject.transform.SetParent(brazier.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            var fire = lightObject.AddComponent<Light>();
            fire.type = LightType.Point;
            fire.color = new Color(1f, 0.55f, 0.2f);
            fire.range = 7f;
            fire.shadows = LightShadows.None;
            var returnPoint = BuildKit.Group("Return Point", brazier.transform);
            returnPoint.position = new Vector3(4f, 0.05f, -1.4f);
            var rest = brazier.AddComponent<RestPoint>();
            rest.Prompt = "Rest at the fire";
            rest.FireIntensity = 6f;
            rest.Configure(director, returnPoint, fire, null);
            built.Player.GetComponent<PlayerInteraction>().Configure(new Interactable[] { rest, mirror });

            PlayerBuilder.AddTitle("The Sun-King's City", "a yard under the noon");
            PlayerBuilder.AddPauseHint();
            SceneBuild.FinishOutsideBuild(scene, ScenePath);
        }
    }
}
