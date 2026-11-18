using System.Collections.Generic;
using Clover.Core;
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
    /// Builds the sun-court (fire_land.md, sections 3.6 and 6): a round court of radius 20 under the white noon, the
    /// obelisk at its centre with the shadow that turns, twelve hours cut in the floor, the seated colossus against
    /// the north wall, a forecourt to the south with the land's last rest fire outside the fight, the gold line the
    /// fight begins at, Khepharu the man, and what he becomes. In the west wall a gilded gate stands shut until the sun
    /// sets through it after the fight; its beam is the way out. The player comes with fists or the spear and no
    /// element. The scene is not in the list the game is built from; the gate of noon loads it in the editor.
    /// </summary>
    public static class SunCourtBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/SunCourt.unity";
        public const float Radius = 20f;
        public const float LineZ = -20f;
        /// <summary>Degrees round from north of the west gate the sun sets through.</summary>
        public const float WestGate = 270f;

        [MenuItem("Clover/Build/Sun Court")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the sun court")) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = BuildKit.Group("World");
            // The gold laid in the court is worn and dull, lit only by what falls on it: it must not glow like light.
            var gold = SunCityKit.FloorGold;
            var stone = SunCityKit.Stone;
            var pale = SunCityKit.Flat("m_sun_court_pale", new Color(0.78f, 0.75f, 0.68f));
            var dark = SunCityKit.Flat("m_sun_court_dark", new Color(0.46f, 0.42f, 0.38f));

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, -33f), "", true);
            if (built.Player == null) return;
            LookBuilder.Apply(LookBuilder.Noon, built.Camera);
            built.Player.GetComponent<PlayerCombat>().StartingWeapon = Weapons.FistsName;
            built.Player.GetComponent<PlayerElement>().StartsWithNone = true;

            // The ground: one slab to stand on under everything, plain mud beyond the walls.
            var ground = BuildKit.Block("Ground", world, new Vector3(0f, -0.5f, -4f), new Vector3(120f, 1f, 120f), SunCityKit.Mud);
            Object.DestroyImmediate(ground.GetComponent<Fadeable>());

            // The court's floor, laid in rings: pale stone, a gold ring the hours are cut on, darker stone round the obelisk.
            var floor = BuildKit.Group("Court Floor", world);
            Disc("Paving", floor, 0.004f, Radius, stone);
            Disc("Hour Ring Outer", floor, 0.008f, 17.5f, gold);
            Disc("Hour Ring", floor, 0.012f, 17.3f, pale);
            Disc("Hour Ring Inner", floor, 0.016f, 16.4f, gold);
            Disc("Paving Middle", floor, 0.02f, 16.2f, stone);
            Disc("Inner Ring", floor, 0.024f, 8.2f, dark);
            Disc("Paving Inner", floor, 0.028f, 8f, pale);
            Disc("Foot Ring", floor, 0.032f, 3.2f, gold);
            Disc("Foot", floor, 0.036f, 3f, dark);
            // Twelve hours: a gold bar across the ring at each, the four quarters longer; and joints in the paving.
            for (int hour = 0; hour < 12; hour++)
            {
                bool quarter = hour % 3 == 0;
                Vector3 at = Polar(hour * 30f, quarter ? 16.6f : 16.85f, 0.03f);
                var bar = BuildKit.Shape(PrimitiveType.Cube, "Hour " + (hour == 0 ? 12 : hour), floor, at, new Vector3(0.34f, 0.02f, quarter ? 2.6f : 1.5f), gold, new Vector3(0f, hour * 30f, 0f));
                bar.shadowCastingMode = ShadowCastingMode.Off;
            }
            // Each hour is counted out in gold pips inside its bar, one for the first hour to twelve for noon at the north:
            // the floor is a clock face, and the shadow is its hand.
            for (int hour = 1; hour <= 12; hour++)
            {
                float angle = hour % 12 * 30f;
                for (int pip = 0; pip < hour; pip++)
                {
                    float across = (pip - (hour - 1) * 0.5f) * 0.42f;
                    Vector3 at = Quaternion.Euler(0f, angle, 0f) * new Vector3(across, 0.031f, 15.3f);
                    BuildKit.Shape(PrimitiveType.Cube, "Hour Pip", floor, at, new Vector3(0.24f, 0.012f, 0.24f), gold, new Vector3(0f, angle + 45f, 0f)).shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            // A second, fine ring half-way in, and the joints of the inner paving.
            Disc("Half Ring Outer", floor, 0.0205f, 12.2f, dark);
            Disc("Half Ring", floor, 0.021f, 12.08f, stone);
            for (int spoke = 0; spoke < 12; spoke++)
            {
                var joint = BuildKit.Shape(PrimitiveType.Cube, "Inner Joint", floor, Polar(spoke * 30f + 15f, 5.6f, 0.03f), new Vector3(0.06f, 0.008f, 4.6f), dark, new Vector3(0f, spoke * 30f + 15f, 0f));
                joint.shadowCastingMode = ShadowCastingMode.Off;
            }
            for (int spoke = 0; spoke < 24; spoke++)
            {
                var joint = BuildKit.Shape(PrimitiveType.Cube, "Joint", floor, Polar(spoke * 15f + 7.5f, 12.2f, 0.031f), new Vector3(0.07f, 0.008f, 7.6f), dark, new Vector3(0f, spoke * 15f + 7.5f, 0f));
                joint.shadowCastingMode = ShadowCastingMode.Off;
            }

            // The wall: three metres on the north, east and west; low on the south for the camera; open at the forecourt.
            var walls = BuildKit.Group("Walls", world);
            for (int i = 0; i < 36; i++)
            {
                float angle = i * 10f + 5f;
                float fromSouth = Mathf.Abs(Mathf.DeltaAngle(angle, 180f));
                if (fromSouth < 17f) continue;
                // The west gate stands here.
                if (Mathf.Abs(Mathf.DeltaAngle(angle, WestGate)) < 6f) continue;
                float height = fromSouth < 75f ? 1.2f : 3f;
                var block = BuildKit.Block("Court Wall", walls, Polar(angle, Radius + 1.1f, height * 0.5f), new Vector3(3.9f, height, 1.4f), SunCityKit.Brick, angle);
                if (height < 2f) Object.DestroyImmediate(block.GetComponent<Fadeable>());
                // A pale coping on the tall wall.
                if (height > 2f) BuildKit.Shape(PrimitiveType.Cube, "Coping", block.transform, new Vector3(0f, 0.5f + 0.06f, 0f), new Vector3(1.04f, 0.1f, 1.2f), pale).shadowCastingMode = ShadowCastingMode.Off;
            }

            // The west gate: two pylons and a gilded leaf in the tall wall, shut all through the fight. When the sun sets
            // the leaf sinks into the floor and the low sun shines through it across the court (WORLD.md, the ride out of Fire).
            var gate = BuildKit.Group("West Gate", world);
            foreach (float side in new[] { -8f, 8f })
            {
                var pylon = BuildKit.Block("Gate Pylon", gate, Polar(WestGate + side, Radius + 1.1f, 2f), new Vector3(2.4f, 4f, 1.8f), SunCityKit.Brick, WestGate + side);
                BuildKit.Shape(PrimitiveType.Cube, "Pylon Cap", pylon.transform, new Vector3(0f, 0.5f + 0.03f, 0f), new Vector3(1.08f, 0.06f, 1.1f), gold).shadowCastingMode = ShadowCastingMode.Off;
            }
            var leaf = BuildKit.Block("Gate Leaf", gate, Polar(WestGate, Radius + 1.1f, 1.5f), new Vector3(3.7f, 3f, 0.5f), gold, WestGate);
            // It moves, so it may not be batched with the walls.
            leaf.isStatic = false;
            BuildKit.Shape(PrimitiveType.Cube, "Leaf Sun", leaf.transform, new Vector3(0f, 0.12f, -0.52f), new Vector3(0.3f, 0.36f, 0.1f), SunCityKit.For("m_sunform_molten")).shadowCastingMode = ShadowCastingMode.Off;
            // Beyond the open gate the way is only for the eye: the player goes west by the gate's mark, not out onto the mud.
            var beyond = new GameObject("Beyond The Gate");
            beyond.transform.SetParent(gate, false);
            beyond.transform.SetPositionAndRotation(Polar(WestGate, Radius + 2.6f, 2f), Quaternion.Euler(0f, WestGate, 0f));
            beyond.AddComponent<BoxCollider>().size = new Vector3(6f, 4f, 0.6f);
            // The low sun's beam through the open gate, laid on the floor from the gate to the foot of the obelisk.
            // It runs on out of the gate to the sun on the horizon, so the way out is seen to go into it.
            var road = BuildKit.Shape(PrimitiveType.Quad, "Sun Road", world, new Vector3(-22f, 0.06f, 0f), new Vector3(38f, 3.4f, 1f),
                BuildKit.Glow(SunCityKit.Folder, "m_sun_road", new Color(3.2f, 1.6f, 0.45f, 1f), 12, true, 1f), new Vector3(90f, 0f, 0f));
            road.shadowCastingMode = ShadowCastingMode.Off;
            road.receiveShadows = false;
            road.enabled = false;
            // The sun itself on the western horizon, beyond the gate: a soft disc turned to the camera, which never turns.
            var lowSun = BuildKit.Shape(PrimitiveType.Quad, "Low Sun", world, new Vector3(-40f, 3f, 0f), new Vector3(12f, 12f, 1f),
                BuildKit.Glow(SunCityKit.Folder, "m_sun_low", new Color(6f, 2.6f, 0.7f, 1f), 0, true, 0.6f), new Vector3(55f, 0f, 0f));
            lowSun.shadowCastingMode = ShadowCastingMode.Off;
            lowSun.receiveShadows = false;
            lowSun.enabled = false;

            // The seated colossus against the north wall, with the face he had cut on it.
            var north = BuildKit.Group("Colossus", world);
            SunCityKit.Place("statue_king", north, new Vector3(0f, 0f, Radius + 5.2f), 180f, 1.35f, false);
            SunCityKit.Place("statue_head_new", north, new Vector3(0f, 8f * 1.35f, Radius + 5.2f - 1.1f * 1.35f), 180f, 1.35f, false);
            foreach (float x in new[] { -9f, 9f })
            {
                SunCityKit.Place("column_base", north, new Vector3(x, 0f, Radius + 3.2f - Mathf.Abs(x) * 0.18f), 180f, 1f, false);
                SunCityKit.Place("column_lotus", north, new Vector3(x, 0.5f, Radius + 3.2f - Mathf.Abs(x) * 0.18f), 180f, 1f, false);
            }

            // The obelisk: solid, at the centre. Its drawn shadow is turned by the court's director.
            // Solid, with a box round the whole of it; it thins out when it stands between the camera and the player, as walls do.
            SunCityKit.Place("obelisk", world, Vector3.zero, 180f, 1f, true);
            var shade = BuildKit.Group("Shadow Hand", world);
            shade.position = new Vector3(0f, 0.045f, 0f);
            var strip = BuildKit.Shape(PrimitiveType.Cube, "Shade", shade, new Vector3(0f, 0f, 9.6f), new Vector3(3.6f, 0.01f, 16.8f), SunCityKit.Shade);
            strip.shadowCastingMode = ShadowCastingMode.Off;

            // The forecourt: walls either side, the door the player came through, the rest fire, the gold line.
            var fore = BuildKit.Group("Forecourt", world);
            BuildKit.Block("Forecourt Floor", fore, new Vector3(0f, -0.04f, -28.6f), new Vector3(12f, 0.1f, 15.6f), stone);
            Object.DestroyImmediate(fore.GetChild(0).GetComponent<Fadeable>());
            BuildKit.Block("Forecourt Wall West", fore, new Vector3(-6.7f, 1.5f, -28.4f), new Vector3(1.4f, 3f, 16.4f), SunCityKit.Brick);
            BuildKit.Block("Forecourt Wall East", fore, new Vector3(6.7f, 1f, -28.4f), new Vector3(1.4f, 2f, 16.4f), SunCityKit.Brick);
            var south = BuildKit.Block("Forecourt Wall South", fore, new Vector3(0f, 0.6f, -37f), new Vector3(14.8f, 1.2f, 1.2f), SunCityKit.BrickDark);
            Object.DestroyImmediate(south.GetComponent<Fadeable>());
            BuildKit.Shape(PrimitiveType.Cube, "Door Behind", fore, new Vector3(0f, 0.7f, -36.35f), new Vector3(3f, 1.4f, 0.14f), SunCityKit.For("m_kit_wood")).shadowCastingMode = ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cube, "Gold Line", fore, new Vector3(0f, 0.045f, LineZ), new Vector3(11.4f, 0.02f, 0.44f), gold).shadowCastingMode = ShadowCastingMode.Off;
            // The wall of heat that closes behind whoever crosses the line: off until the fight begins.
            var heat = new GameObject("Wall Of Heat");
            heat.transform.SetParent(fore, false);
            heat.transform.position = new Vector3(0f, 0f, LineZ - 0.5f);
            var heatWall = heat.AddComponent<BoxCollider>();
            heatWall.center = new Vector3(0f, 2f, 0f);
            heatWall.size = new Vector3(13f, 4f, 0.6f);
            var heatGlow = BuildKit.Shape(PrimitiveType.Cube, "Heat", heat.transform, new Vector3(0f, 0.9f, 0f), new Vector3(11.6f, 1.8f, 0.12f),
                BuildKit.Flame(SunCityKit.Folder, "m_sun_heat_wall", new Color(2.6f, 1.1f, 0.25f, 0.55f)));
            heatGlow.shadowCastingMode = ShadowCastingMode.Off;

            // The god as a man, at the far side of the court.
            var kit = EnemyBuilder.NewKit(built.Player.transform, built.Camera.transform.rotation);
            var boss = EnemyBuilder.Build(EnemyKind.SunKingName, new Vector3(0f, 0.05f, 8f), 180f, kit, null);
            kit.Turns.Configure(new[] { boss });

            var start = BuildKit.Group("Start Point", world);
            start.position = new Vector3(0f, 0.05f, -33f);
            var director = new GameObject("Area Director").AddComponent<AreaDirector>();
            director.StormStrikeMin = 100000f;
            director.StormStrikeMax = 200000f;
            director.Configure(built.Player.GetComponent<PlayerController>(), new[] { boss }, start, null, null);
            // A fall returns to the forecourt's fire, as everywhere in this land.
            var land = director.gameObject.AddComponent<LandReturn>();
            land.Configure(director, built.Player.GetComponent<PlayerDefence>());
            land.SleepBeyond = 0f;

            var things = new List<Interactable>();
            things.Add(Rest(fore, director, new Vector3(3.6f, 0f, -29f), gold, dark));

            // What he leaves is the fire in him, taken from his body where he lies. It can only be taken.
            var fire = HisFire();
            things.Add(fire);

            // The way out of the land: west, after the setting sun, from the open gate (it leads where the barque once did).
            var door = new GameObject("West After The Sun").AddComponent<SceneDoor>();
            // Inside the gate, not in it: from the camera the gate's south pylon would stand in front of whoever stands there.
            door.transform.position = Polar(WestGate, Radius - 3.6f, 0f);
            door.Prompt = "Go west with the sun";
            door.LeadsTo = "Hub";
            door.EndsRun = true;
            door.Range = 2.8f;
            BuildKit.Shape(PrimitiveType.Cylinder, "Sunset Mark", door.transform, new Vector3(0f, 0.07f, 0f), new Vector3(2.6f, 0.01f, 2.6f), gold).shadowCastingMode = ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cylinder, "Sunset Stone", door.transform, new Vector3(0f, 0.075f, 0f), new Vector3(2.3f, 0.01f, 2.3f), dark).shadowCastingMode = ShadowCastingMode.Off;
            things.Add(door);

            // The spear as it stands in the floor after a throw, and the ray it comes back as.
            // The Sun-Spear itself, the one the player is given when Fire is taken, at his size, stood on its butt with
            // the white-hot blade up.
            var spear = BuildKit.Group("Standing Spear");
            var sunSpear = (GameObject)PrefabUtility.InstantiatePrefab(WeaponBuilder.Prefab("sunspear"));
            PrefabUtility.UnpackPrefabInstance(sunSpear, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            sunSpear.name = "Sun-Spear";
            sunSpear.transform.SetParent(spear, false);
            sunSpear.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            sunSpear.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            sunSpear.transform.localScale = Vector3.one * 1.4f;
            foreach (var shown in sunSpear.GetComponentsInChildren<Renderer>()) shown.shadowCastingMode = ShadowCastingMode.Off;
            // The fire it trails in flight, from the blade: a yellow heart at the spear, red behind, cooling to smoke.
            var trailFrom = BuildKit.Group("Trail From", spear);
            trailFrom.localPosition = new Vector3(0f, 3.6f, 0f);
            var spearTrail = trailFrom.gameObject.AddComponent<TrailRenderer>();
            spearTrail.time = 0.4f;
            spearTrail.widthMultiplier = 0.7f;
            spearTrail.minVertexDistance = 0.25f;
            spearTrail.emitting = false;
            spearTrail.sharedMaterial = BuildKit.Fire(SunCityKit.Folder, "m_sun_spear_trail_fire", 12, new Vector2(-2.4f, 0f), 4f);
            var trailFade = new Gradient();
            trailFade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.7f, 0.5f), new GradientAlphaKey(0.15f, 1f) });
            spearTrail.colorGradient = trailFade;
            spearTrail.shadowCastingMode = ShadowCastingMode.Off;
            spearTrail.receiveShadows = false;
            spear.gameObject.SetActive(false);
            var rayObject = new GameObject("Returning Ray");
            var ray = rayObject.AddComponent<LineRenderer>();
            ray.useWorldSpace = true;
            ray.positionCount = 2;
            // The call is a stream of fire from the standing blade to his hand, not a thread of light.
            ray.sharedMaterial = BuildKit.Fire(SunCityKit.Folder, "m_sun_ray_stream", 12, new Vector2(-3f, 0f), 4f);
            ray.widthMultiplier = 0.6f;
            ray.shadowCastingMode = ShadowCastingMode.Off;
            ray.receiveShadows = false;
            ray.enabled = false;
            var rayLook = RayLook(rayObject);

            var bossHud = SummitBuilder.BossBar(boss, $"{EnemyKind.SunKingName},  {EnemyKind.SunKing().Title}");
            var form = SunBearerBuilder.Build(built.Player.transform, Radius, kit.Marker);

            Light key = null;
            foreach (var light in Object.FindObjectsByType<Light>())
                if (light.type == LightType.Directional && (key == null || light.intensity > key.intensity)) key = light;

            // The sunset's grade: warm, deeper, the shadows violet. It is weighed in by the court as the sun goes down.
            var sunsetObject = new GameObject("Sunset Look");
            var sunsetLook = sunsetObject.AddComponent<Volume>();
            sunsetLook.isGlobal = true;
            sunsetLook.priority = 10f;
            sunsetLook.weight = 0f;
            sunsetLook.sharedProfile = SunsetProfile();

            var court = new GameObject("Sun Court Director").AddComponent<SunCourtDirector>();
            court.Configure(boss, built.Player.GetComponent<PlayerDefence>(), built.Player.GetComponent<HitStop>(), form, bossHud, fire, door);
            court.ConfigureSunset(sunsetLook, leaf.transform, leaf.GetComponent<Collider>(), road, lowSun);
            court.ConfigureCourt(heatWall, heatGlow, key != null ? key.transform : null, shade, spear, BuildKit.FindDeep(boss.transform, "weapon.R"), ray);
            court.ConfigureRay(rayLook);
            court.ConfigureThrow(spear.GetComponentInChildren<TrailRenderer>(true));
            court.LineZ = LineZ;
            // Section 7.5 and 7.4 of the land's document: every word here is a proposal of Claude's until the owner passes it.
            court.Greeting = new[]
            {
                "Another, sent with empty hands.",
                "I know what sends you. It was here before the star fell.",
                "I carried the day home on my back. They were sitting in the dark. Look at them now. Warm.",
                "Come. It is noon.",
            };
            // His one line, as the owner wrote it (WORLD.md, "Every god transforms").
            court.Regret = new[] { "I promised them gold. I gave them fire." };
            court.Barque = new[]
            {
                "You have it.",
                "It burns the hand that holds it. You will learn where to hold it.",
                "The sun is moving. That is your doing. Do not stay to watch it set.",
                "The sun knows the way down the sky. Follow it west.",
            };
            fire.Configure(court);
            built.Player.GetComponent<PlayerInteraction>().Configure(things.ToArray());

            PlayerBuilder.AddTitle("The Court of Noon", "Khepharu, God of Fire");
            PlayerBuilder.AddPauseHint();
            SceneBuild.FinishOutsideBuild(scene, ScenePath);
        }

        // The ray as a beam of light (fire_boss_visuals.md 2.4): the thread is the plain line made above; then a white core
        // with the beam's ragged edge, two gold strands, the spear's white streak, a light that rides it and a line of heat on the floor.
        private static SpearRay RayLook(GameObject rayObject)
        {
            LineRenderer Line(string name, Material material, float width)
            {
                var lineObject = new GameObject(name);
                lineObject.transform.SetParent(rayObject.transform, false);
                var line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.sharedMaterial = material;
                line.widthMultiplier = width;
                line.numCornerVertices = 0;
                line.positionCount = 2;
                line.shadowCastingMode = ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.enabled = false;
                return line;
            }
            // A jet of fire (the owner's review, 2026-11-17): a churning core of flame in the beam's shape with the
            // smallest white heart, two tongues of fire kinking round it, the spear a burning streak, its light firelight.
            var core = Line("Ray Core", BuildKit.Fire(SunCityKit.Folder, "m_sun_ray_jet", 12, new Vector2(-4f, 0f), 5f, 0.96f), 1f);
            var strandLight = BuildKit.Fire(SunCityKit.Folder, "m_sun_ray_tongue", 1, new Vector2(-3f, 0f), 4f);
            var strands = new[] { Line("Ray Strand 1", strandLight, 0.5f), Line("Ray Strand 2", strandLight, 0.5f) };
            var streak = Line("Ray Spear", BuildKit.Fire(SunCityKit.Folder, "m_sun_ray_spear_fire", 1, new Vector2(-3f, 0f), 3f, 0.9f), 0.55f);
            var lightObject = new GameObject("Ray Light");
            lightObject.transform.SetParent(rayObject.transform, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.48f, 0.12f);
            light.range = 6f;
            light.shadows = LightShadows.None;
            light.enabled = false;
            var heat = BuildKit.Shape(PrimitiveType.Quad, "Ray Heat", rayObject.transform, Vector3.zero, Vector3.one, BuildKit.Fire(SunCityKit.Folder, "m_sun_ray_scorch", 1, new Vector2(0.4f, 0f), 6f));
            heat.shadowCastingMode = ShadowCastingMode.Off;
            heat.receiveShadows = false;
            heat.enabled = false;
            var look = rayObject.AddComponent<SpearRay>();
            look.Configure(rayObject.GetComponent<LineRenderer>(), core, strands, streak, light, heat);
            return look;
        }

        private static Vector3 Polar(float angle, float radius, float height = 0f) => Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, height, radius);

        // A thin round of floor, drawn only: the ground slab is what is stood on.
        private static void Disc(string name, Transform parent, float top, float radius, Material material)
        {
            var disc = BuildKit.Shape(PrimitiveType.Cylinder, name, parent, new Vector3(0f, top - 0.005f, 0f), new Vector3(radius * 2f, 0.005f, radius * 2f), material);
            disc.GetComponent<MeshFilter>().sharedMesh = BuildKit.RoundMesh();
            disc.shadowCastingMode = ShadowCastingMode.Off;
            disc.gameObject.isStatic = true;
        }

        private static RestPoint Rest(Transform parent, AreaDirector director, Vector3 foot, Material gold, Material dark)
        {
            var brazier = SunCityKit.Place("brazier", parent, foot);
            brazier.name = "Rest Brazier";
            BuildKit.Shape(PrimitiveType.Cylinder, "Rest Ring", parent, foot + new Vector3(0f, 0.025f, 0f), new Vector3(2.7f, 0.008f, 2.7f), gold).shadowCastingMode = ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cylinder, "Rest Ring Inner", parent, foot + new Vector3(0f, 0.03f, 0f), new Vector3(2.45f, 0.008f, 2.45f), dark).shadowCastingMode = ShadowCastingMode.Off;
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
            rest.Configure(director, returnPoint, fire, SunCityKit.RestFlames(brazier));
            return rest;
        }

        // What is left of the sun is the fire in his chest: a warm light over his body where he lies, the place the
        // fire is taken from him. The court puts it on him when the disc cracks and shows it once he has spoken.
        private static GodReward HisFire()
        {
            var root = new GameObject("His Fire");
            var lightObject = new GameObject("Fire Light");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.18f);
            light.range = 4.5f;
            light.intensity = 6f;
            light.shadows = LightShadows.None;
            var reward = root.AddComponent<GodReward>();
            reward.Prompt = "Take his fire";
            reward.Element = Elements.Fire;
            reward.Takes = true;
            reward.Range = 2.6f;
            return reward;
        }

        // The sun's last light on the court: a warm filter, a little deeper, the shade violet and the light orange.
        private static VolumeProfile SunsetProfile()
        {
            string path = BuildKit.GeneratedRoot + "/Look/SunsetProfile.asset";
            BuildKit.EnsureFolder(BuildKit.GeneratedRoot + "/Look");
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            foreach (var component in profile.components.ToArray())
            {
                profile.components.Remove(component);
                Object.DestroyImmediate(component, true);
            }
            var colour = profile.Add<UnityEngine.Rendering.Universal.ColorAdjustments>(true);
            colour.name = "ColorAdjustments";
            AssetDatabase.AddObjectToAsset(colour, profile);
            colour.postExposure.Override(-0.1f);
            colour.saturation.Override(4f);
            colour.contrast.Override(10f);
            colour.colorFilter.Override(new Color(1f, 0.9f, 0.8f));
            var split = profile.Add<UnityEngine.Rendering.Universal.SplitToning>(true);
            split.name = "SplitToning";
            AssetDatabase.AddObjectToAsset(split, profile);
            split.shadows.Override(new Color(0.38f, 0.3f, 0.62f));
            split.highlights.Override(new Color(0.66f, 0.46f, 0.3f));
            split.balance.Override(-20f);
            var vignette = profile.Add<UnityEngine.Rendering.Universal.Vignette>(true);
            vignette.name = "Vignette";
            AssetDatabase.AddObjectToAsset(vignette, profile);
            vignette.intensity.Override(0.4f);
            vignette.smoothness.Override(0.5f);
            vignette.color.Override(new Color(0.18f, 0.06f, 0.04f));
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }
    }
}
