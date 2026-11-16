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

        private static Material paving, wall, wallDark, trim, wood, iron, cloth, earth, roof, ember, steel, rune;
        private static Material fireMaterial, rainMaterial, boltMaterial;
        private static Transform world;
        private static readonly List<Transform> rods = new List<Transform>();
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

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, 2f), PlayerBuilder.ControlsHint);
            if (built.Player == null) return;
            var storm = LookBuilder.Apply(LookBuilder.StormNight, built.Camera);
            if (storm != null)
            {
                // The rods bring the flashes; the sky by itself only mutters now and then.
                storm.MinSecondsBetween = 14f;
                storm.MaxSecondsBetween = 30f;
            }
            Weather(built.Player.transform, storm);

            var start = BuildKit.Group("Start Point", world);
            start.position = new Vector3(0f, 0.05f, 2f);

            var enemies = new List<Enemy>();
            var kit = EnemyBuilder.NewKit(built.Player.transform, built.Camera.transform.rotation);
            var enemyRoot = BuildKit.Group("Enemies");
            void Spawn(string kind, float x, float y, float z, float yaw) =>
                enemies.Add(EnemyBuilder.Build(kind, new Vector3(x, y + 0.05f, z), yaw, kit, enemyRoot));

            var director = new GameObject("Area Director").AddComponent<AreaDirector>();

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
            Spawn(EnemyKind.SoldierName, -5f, 4f, 108f, 180f);
            Spawn(EnemyKind.SoldierName, 5f, 4f, 108f, 180f);
            Spawn(EnemyKind.AcolyteName, 0f, 4f, 113f, 180f);
            Spawn(EnemyKind.CasterName, -11f, 4f, 119f, 160f);
            Spawn(EnemyKind.CasterName, 11f, 4f, 119f, 200f);

            var strikeOutline = WorldBars.Marker("Storm Mark Outline", director.transform, 0f, kit.Marker);
            var strikeFill = WorldBars.Marker("Storm Mark Fill", director.transform, 0f, kit.Marker);
            director.Configure(built.Player.GetComponent<PlayerController>(), enemies.ToArray(), start, strikeOutline, strikeFill);
            kit.Turns.Configure(enemies.ToArray());
            built.Player.GetComponent<PlayerInteraction>().Configure(things.ToArray());
            built.Player.GetComponent<PlayerCombat>().StartingWeapon = Weapons.FistsName;

            var rodsObject = new GameObject("Storm Rods").AddComponent<StormRods>();
            rodsObject.Configure(rods.ToArray());

            SceneBuild.Finish(scene, ScenePath, true);
        }

        private static void Materials()
        {
            var flags = ProceduralTextures.Flagstones();
            var blocks = ProceduralTextures.WallBlocks();
            var planks = ProceduralTextures.Planks();
            var ground = ProceduralTextures.Ground();
            // An illustrated night: stone in deep blues and purples, wood and moss murky, and the strong colour kept for
            // banners, fire, lightning and the living.
            paving = BuildKit.Toon(Folder, "m_castle_paving", new Color(0.3f, 0.31f, 0.4f), false, flags, 0.125f, 0f);
            wall = BuildKit.Toon(Folder, "m_castle_wall", new Color(0.36f, 0.35f, 0.47f), false, blocks, 0.2f, 0.025f);
            wallDark = BuildKit.Toon(Folder, "m_castle_wall_dark", new Color(0.22f, 0.21f, 0.32f), false, blocks, 0.2f, 0.025f);
            trim = BuildKit.Toon(Folder, "m_castle_trim", new Color(0.46f, 0.46f, 0.58f), false, ground, 0.5f, 0.02f);
            wood = BuildKit.Toon(Folder, "m_castle_wood", new Color(0.36f, 0.22f, 0.28f), false, planks, 0.5f, 0.02f);
            iron = BuildKit.Toon(Folder, "m_castle_iron", new Color(0.16f, 0.15f, 0.27f), false, null, 0f, 0.02f);
            cloth = BuildKit.Toon(Folder, "m_castle_cloth", new Color(0.82f, 0.13f, 0.22f), false, null, 0f, 0.015f);
            earth = BuildKit.Toon(Folder, "m_castle_earth", new Color(0.2f, 0.3f, 0.29f), false, ground, 0.15f, 0f);
            roof = BuildKit.Toon(Folder, "m_castle_roof", new Color(0.14f, 0.3f, 0.38f), false, planks, 0.4f, 0.03f);
            steel = BuildKit.Toon(Folder, "m_castle_steel", new Color(0.75f, 0.78f, 0.84f), false, null, 0f, 0.012f).Lit(new Color(1.4f, 1.6f, 2f), 0.6f, new Color(0.25f, 0.4f, 0.8f));
            ember = BuildKit.Toon(Folder, "m_castle_ember", new Color(0.9f, 0.45f, 0.15f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(1.6f, 0.5f, 0.1f));
            rune = BuildKit.Toon(Folder, "m_castle_rune", new Color(0.6f, 0.8f, 1f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.9f, 2f, 4.5f));
            fireMaterial = BuildKit.Glow(Folder, "m_castle_fire", new Color(5f, 1.8f, 0.35f, 1f), 0, true, 1.4f, 0.7f).Graphic(0.22f, 0.7f);
            rainMaterial = BuildKit.Glow(Folder, "m_castle_rain", new Color(0.7f, 0.8f, 1.1f, 0.4f), 1, true, 1f);
            boltMaterial = BuildKit.Glow(Folder, "m_castle_bolt", new Color(5f, 7f, 12f, 1f), 3, true, 1f);
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
                Wall("Parapet", group, new Vector3(x, 0f, -4f), new Vector3(x, 0f, 26f), 1.1f, 0.6f, wall);
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
            Rubble(group, new Vector3(14f, 0f, 45f), 4);
            StruckTree(group, new Vector3(-9f, 0f, 39f));
            Crates(group, new Vector3(15.5f, 0f, 32f));
            Crates(group, new Vector3(-16f, 0f, 58f));
            Brazier(group, new Vector3(-6f, 0f, 31f), true);
            Brazier(group, new Vector3(6f, 0f, 31f), true);
            Brazier(group, new Vector3(-7f, 0f, 54f), true);
            Brazier(group, new Vector3(7f, 0f, 54f), true);
            Banner(group, new Vector3(-18.6f, 3.6f, 40f), 90f);
            Banner(group, new Vector3(18.6f, 3.6f, 50f), -90f);
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
            Crates(group, new Vector3(-16f, 2f, 80f));
            Banner(group, new Vector3(-18.6f, 5.6f, 74f), 90f);
            Banner(group, new Vector3(18.6f, 5.6f, 74f), -90f);
        }

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
            way.Configure(director);
            things.Add(way);

            foreach (float x in new[] { -9f, 9f })
            {
                Pillar("Ward Pillar", group, new Vector3(x, 4f, 101f), 1.6f, 3.6f, wall, trim);
                Pillar("Ward Pillar", group, new Vector3(x, 4f, 116f), 1.6f, 3.6f, wall, trim);
            }
            Brazier(group, new Vector3(-5f, 4f, 123.4f), true);
            Brazier(group, new Vector3(5f, 4f, 123.4f), true);
            Brazier(group, new Vector3(-15f, 4f, 99f), true);
            Brazier(group, new Vector3(15f, 4f, 99f), true);
            Rubble(group, new Vector3(-14f, 4f, 112f), 6);
            Rubble(group, new Vector3(13f, 4f, 106f), 4);
            Banner(group, new Vector3(-6.5f, 11f, 126.3f), 0f);
            Banner(group, new Vector3(6.5f, 11f, 126.3f), 0f);
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

        private static ParticleSystem Flames(Transform parent, Vector3 localPosition, bool playing)
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
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = 60;
            var emission = system.emission;
            emission.rateOverTime = 26f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.28f;
            var colour = system.colorOverLifetime;
            colour.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.5f, 0.3f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
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
            BuildKit.Shape(PrimitiveType.Cube, "Mat", brazier, new Vector3(0f, 0.03f, -1.3f), new Vector3(1.6f, 0.05f, 1.2f), cloth);
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
        private static void Weather(Transform player, StormLight storm)
        {
            var weather = BuildKit.Group("Weather");

            var rainObject = new GameObject("Rain");
            rainObject.transform.SetParent(weather, false);
            rainObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            rainObject.AddComponent<Follow>().Configure(player, new Vector3(1.5f, 16f, 2f));
            var rain = rainObject.AddComponent<ParticleSystem>();
            rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = rain.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.75f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(20f, 26f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.05f);
            main.maxParticles = 1500;
            var emission = rain.emission;
            emission.rateOverTime = 900f;
            var shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(34f, 26f, 1f);
            var rainRenderer = rainObject.GetComponent<ParticleSystemRenderer>();
            rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            rainRenderer.velocityScale = 0.03f;
            rainRenderer.lengthScale = 6f;
            rainRenderer.sharedMaterial = rainMaterial;
            rainRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

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
                line.widthMultiplier = 0.4f;
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
