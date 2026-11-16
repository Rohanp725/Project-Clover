using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the summit: the top of the keep, above the storm, where the god of Lightning waits. A round stone floor
    /// inlaid with rings stands in a sea of cloud; a ring of gate-posts hung with thunder drums stands round its far
    /// side, and bolts fall into the cloud all about it. The player comes up at the south and the god stands to the
    /// north. Nothing tall stands on the camera's side of the floor.
    /// </summary>
    public static class SummitBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Summit.unity";
        private const string Folder = BuildKit.GeneratedRoot + "/Summit";
        private const float Radius = 17f;

        [MenuItem("Clover/Build/Summit")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the summit")) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CastleBuilder.Materials();
            CastleBuilder.rods.Clear();

            var stone = BuildKit.Toon(Folder, "m_summit_stone", new Color(0.4f, 0.4f, 0.56f), false, ProceduralTextures.Flagstones(), 0.125f, 0f);
            var dark = BuildKit.Toon(Folder, "m_summit_dark", new Color(0.22f, 0.21f, 0.36f), false, ProceduralTextures.WallBlocks(), 0.2f, 0.02f);
            var pale = BuildKit.Toon(Folder, "m_summit_pale", new Color(0.62f, 0.62f, 0.8f), false, null, 0f, 0.02f);
            var gold = BuildKit.Toon(Folder, "m_summit_gold", new Color(0.95f, 0.7f, 0.18f), false, null, 0f, 0.02f).Lit(new Color(1.2f, 0.9f, 0.4f), 0.4f, new Color(0.3f, 0.16f, 0.02f));
            var red = BuildKit.Toon(Folder, "m_summit_red", new Color(0.8f, 0.12f, 0.16f), false, null, 0f, 0.02f);
            var rune = BuildKit.Toon(Folder, "m_summit_rune", new Color(0.6f, 0.85f, 1f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(0.8f, 1.9f, 4.2f));
            var cloud = BuildKit.Toon(Folder, "m_summit_cloud", new Color(0.4f, 0.4f, 0.64f), false, null, 0f, 0f).Lit(new Color(0.3f, 0.34f, 0.6f), 0.5f);
            cloud.SetColor("_ShadeColor", new Color(0.42f, 0.4f, 0.74f));
            var cloudDeep = BuildKit.Toon(Folder, "m_summit_cloud_deep", new Color(0.3f, 0.3f, 0.55f), false, null, 0f, 0f);
            var spirit = BuildKit.Flame(Folder, "m_summit_spirit_flame", new Color(0.3f, 0.7f, 1.8f, 1f));

            var world = BuildKit.Group("World");

            // The floor: a round of stone with rings let into it, the innermost alight.
            // The layers lie within three centimetres of one another, so the marks attacks draw on the ground (four
            // centimetres up and more) are always above every one of them.
            Disc("Floor", world, 0f, Radius, 1.2f, dark, true);
            Disc("Paving", world, 0.004f, Radius - 0.8f, 0.02f, stone, false);
            Disc("Ring Outer", world, 0.008f, 11.2f, 0.02f, pale, false);
            Disc("Paving Middle", world, 0.012f, 10.8f, 0.02f, stone, false);
            Disc("Ring Inner", world, 0.016f, 5.2f, 0.02f, rune, false);
            Disc("Paving Inner", world, 0.02f, 5f, 0.02f, stone, false);
            Disc("Seal", world, 0.024f, 1.2f, 0.02f, rune, false);
            Disc("Seal Core", world, 0.028f, 1f, 0.02f, dark, false);

            // An unseen wall round the edge, so no one walks off into the cloud.
            for (int i = 0; i < 36; i++)
            {
                float angle = i * 10f;
                var wall = new GameObject("Edge");
                wall.transform.SetParent(world, false);
                wall.transform.SetPositionAndRotation(Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 1.5f, Radius - 0.3f), Quaternion.Euler(0f, angle, 0f));
                wall.AddComponent<BoxCollider>().size = new Vector3(3.2f, 3f, 0.6f);
            }

            // The gate-posts: tall and joined by beams round the north, where they are the backdrop; stubs at the south.
            var posts = BuildKit.Group("Posts", world);
            const int Count = 14;
            for (int i = 0; i < Count; i++)
            {
                float angle = (i + 0.5f) * 360f / Count;
                Vector3 at = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, Radius - 1.2f);
                // North is toward +Z: the angle from north decides how tall a post may stand.
                float fromNorth = Mathf.Abs(Mathf.DeltaAngle(angle, 0f));
                bool tall = fromNorth < 95f;
                float height = tall ? Mathf.Lerp(6.5f, 4.2f, fromNorth / 95f) : 1.1f;
                // The low ones by the camera are stone stubs with a red band: seen from above, a red post is only a red square.
                BuildKit.Block("Post", posts, at + Vector3.up * (height * 0.5f), new Vector3(0.9f, height, 0.9f), tall ? red : dark, angle);
                if (!tall) BuildKit.Block("Post Band", posts, at + Vector3.up * (height - 0.2f), new Vector3(1f, 0.16f, 1f), red, angle);
                BuildKit.Block("Post Foot", posts, at + Vector3.up * 0.2f, new Vector3(1.4f, 0.4f, 1.4f), dark, angle);
                // Gold caps only on the tall posts: on the low ones by the camera they would be slabs of yellow across the view.
                if (tall) BuildKit.Block("Post Cap", posts, at + Vector3.up * (height + 0.12f), new Vector3(1.2f, 0.24f, 1.2f), gold, angle);
                if (!tall) continue;
                // A thunder drum hangs on each tall post, facing the floor, and a blue flame burns on top.
                var drum = BuildKit.Shape(PrimitiveType.Cylinder, "Drum", posts, at + Vector3.up * (height * 0.62f) - at.normalized * 0.6f,
                    new Vector3(1.3f, 0.16f, 1.3f), red, new Vector3(90f, angle, 0f));
                BuildKit.Shape(PrimitiveType.Cylinder, "Drum Skin", posts, drum.transform.position - at.normalized * 0.17f, new Vector3(1.1f, 0.02f, 1.1f), gold, new Vector3(90f, angle, 0f));
                BuildKit.Shape(PrimitiveType.Cylinder, "Drum Mark", posts, drum.transform.position - at.normalized * 0.2f, new Vector3(0.45f, 0.02f, 0.45f), rune, new Vector3(90f, angle, 0f));
                SpiritFlame(posts, at + Vector3.up * (height + 0.3f), spirit);
                var rod = BuildKit.Group("Rod", posts);
                rod.position = at + Vector3.up * (height + 0.6f) + at.normalized * 8f;
                CastleBuilder.rods.Add(rod);
            }
            // Beams across the tops of the tall posts.
            for (int i = 0; i < Count; i++)
            {
                float a0 = (i + 0.5f) * 360f / Count, a1 = (i + 1.5f) * 360f / Count;
                float n0 = Mathf.Abs(Mathf.DeltaAngle(a0, 0f)), n1 = Mathf.Abs(Mathf.DeltaAngle(a1, 0f));
                if (n0 >= 95f || n1 >= 95f) continue;
                Vector3 p0 = Quaternion.Euler(0f, a0, 0f) * new Vector3(0f, Mathf.Lerp(6.5f, 4.2f, n0 / 95f), Radius - 1.2f);
                Vector3 p1 = Quaternion.Euler(0f, a1, 0f) * new Vector3(0f, Mathf.Lerp(6.5f, 4.2f, n1 / 95f), Radius - 1.2f);
                var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beam.name = "Beam";
                beam.transform.SetParent(posts, false);
                beam.transform.position = (p0 + p1) * 0.5f - Vector3.up * 0.5f;
                beam.transform.rotation = Quaternion.LookRotation(p1 - p0, Vector3.up);
                beam.transform.localScale = new Vector3(0.5f, 0.5f, (p1 - p0).magnitude + 1.6f);
                beam.GetComponent<MeshRenderer>().sharedMaterial = red;
                Object.DestroyImmediate(beam.GetComponent<Collider>());
                beam.isStatic = true;
            }

            // The sea of cloud: a floor of it far below, and banks of it heaped all round.
            var clouds = BuildKit.Group("Clouds", world);
            Disc("Cloud Sea", clouds, -3f, 160f, 0.5f, cloudDeep, false);
            var random = new System.Random(7);
            for (int i = 0; i < 70; i++)
            {
                float angle = (float)random.NextDouble() * 360f;
                float size = 5f + (float)random.NextDouble() * 9f;
                // No bank's shoulder comes within reach of the floor's edge, where the camera would look through it.
                float distance = Radius + 1.5f + size * 0.8f + (float)random.NextDouble() * 42f;
                // Banks on the camera's side stay low, so they never cover the floor; so do any that stand close.
                bool south = Mathf.Abs(Mathf.DeltaAngle(angle, 180f)) < 80f;
                bool near = distance < Radius + 14f;
                float height = (south ? -3.6f : -2.4f) + (float)random.NextDouble() * (south ? 1f : near ? 1.2f : 3.5f) - (near ? size * 0.2f : 0f);
                Vector3 at = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, height, distance);
                var puff = BuildKit.Shape(PrimitiveType.Sphere, "Cloud", clouds, at, new Vector3(size * 1.5f, size * 0.7f, size * 1.2f), cloud);
                puff.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                puff.gameObject.isStatic = true;
                if (i % 5 == 0)
                {
                    var rod = BuildKit.Group("Rod", clouds);
                    rod.position = at + Vector3.up * (size * 0.3f);
                    CastleBuilder.rods.Add(rod);
                }
            }

            // What makes it a place: the crest and the scars in the floor, the built edge and what stands on it, and the
            // nearer cloud with the keep's roofs coming up through it.
            Dressing.Prepare();
            FloorInlay(world, dark);
            Rim(world, dark, rune, spirit, Count);
            CloudBanks(clouds, cloud);

            // The player, the look, the storm (no rain: this is above it).
            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, -12.5f), "");
            if (built.Player == null) return;
            built.Player.GetComponent<PlayerCombat>().StartingWeapon = Weapons.KatanaName;
            var storm = LookBuilder.Apply(LookBuilder.Heavens, built.Camera);
            if (storm != null)
            {
                storm.MinSecondsBetween = 9f;
                storm.MaxSecondsBetween = 20f;
            }
            CastleBuilder.Weather(built.Player.transform, storm);
            foreach (string wet in new[] { "Rain", "Rain Splashes" })
            {
                var found = GameObject.Find(wet);
                if (found != null) Object.DestroyImmediate(found);
            }
            var rods = new GameObject("Storm Rods").AddComponent<StormRods>();
            rods.Configure(CastleBuilder.rods.ToArray());

            var start = BuildKit.Group("Start Point", world);
            start.position = new Vector3(0f, 0.05f, -12.5f);

            // The god, alone, at the far side of the floor.
            var kit = EnemyBuilder.NewKit(built.Player.transform, built.Camera.transform.rotation);
            var boss = EnemyBuilder.Build(EnemyKind.NarukamiName, new Vector3(0f, 0.05f, 8f), 180f, kit, null);
            kit.Turns.Configure(new[] { boss });

            var director = new GameObject("Area Director").AddComponent<AreaDirector>();
            // No storm strikes on him: he calls his own.
            director.StormStrikeMin = 100000f;
            director.StormStrikeMax = 200000f;
            director.Configure(built.Player.GetComponent<PlayerController>(), new[] { boss }, start, null, null);

            // What he leaves: his element to take, on the left, or to consume, on the right; and the way back down.
            var things = new List<Interactable>();
            var take = Reward("Take the Storm", new Vector3(-3f, 0.1f, 2f), rune, gold, "Take Lightning: lose Fire, gain the storm and his sword");
            var consume = Reward("Consume the Storm", new Vector3(3f, 0.1f, 2f), BuildKit.Toon(Folder, "m_summit_ember", new Color(1f, 0.5f, 0.15f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(4f, 1.3f, 0.2f)), gold, "Consume it: keep Fire, and make it burn hotter");
            things.Add(take);
            things.Add(consume);

            var door = new GameObject("Way Down").AddComponent<SceneDoor>();
            door.transform.position = new Vector3(0f, 0f, -14.5f);
            door.Prompt = "Leave the summit. The hours turn back";
            door.LeadsTo = "Hub";
            door.EndsRun = true;
            door.Range = 2.6f;
            BuildKit.Shape(PrimitiveType.Cylinder, "Way Down Mark", door.transform, new Vector3(0f, 0.08f, 0f), new Vector3(2.4f, 0.01f, 2.4f), rune);
            BuildKit.Shape(PrimitiveType.Cylinder, "Way Down Stone", door.transform, new Vector3(0f, 0.09f, 0f), new Vector3(2.1f, 0.01f, 2.1f), dark);
            things.Add(door);

            var summit = new GameObject("Summit Director").AddComponent<SummitDirector>();
            summit.Configure(boss, built.Player.GetComponent<PlayerDefence>(), built.Player.GetComponent<HitStop>(), rods, take, consume, door);
            take.Configure(summit, true);
            consume.Configure(summit, false);
            built.Player.GetComponent<PlayerInteraction>().Configure(things.ToArray());

            var bossHud = BossBar(boss);
            // What he became: under the cloud beyond the north edge until the swordsman falls.
            var colossus = ColossusBuilder.Build(built.Player.transform, Radius, kit.Marker);
            if (colossus != null) summit.ConfigureSecondForm(colossus, bossHud);
            PlayerBuilder.AddRunEnd(built.Player);
            PlayerBuilder.AddTitle("Narukami", EnemyKind.Narukami().Title);
            SceneBuild.Finish(scene, ScenePath, false);
        }

        // ------------------------------------------------------------------ the floor's crest and scars, the rim, the near cloud

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static float Next(System.Random random, float low, float high) => low + (float)random.NextDouble() * (high - low);

        // A point on the round: an angle clockwise from north, a distance out, and optionally a step to the right of that line.
        private static Vector3 Polar(float angle, float radius, float height = 0f) => Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, height, radius);
        private static Vector3 Polar(float angle, float radius, float across, float height) => Quaternion.Euler(0f, angle, 0f) * new Vector3(across, height, radius);

        // How high the floor's own layers stand at a distance from its middle.
        private static float FloorTop(float radius) => radius < 1f ? 0.028f : radius < 1.2f ? 0.024f : radius < 5f ? 0.02f : radius < 5.2f ? 0.016f
            : radius < 10.8f ? 0.012f : radius < 11.2f ? 0.008f : radius < Radius - 0.8f ? 0.004f : 0f;

        private static Dressing.Mould Layer(float lift) => new Dressing.Mould { Top = FloorTop, Lift = lift };
        private static Material Flat(string name, Color colour) => BuildKit.Toon(Folder, name, colour, false, null, 0f, 0f);

        private static Vector3 Within(Vector3 point, float radius)
        {
            Vector3 flat = new Vector3(point.x, 0f, point.z);
            return flat.magnitude > radius ? flat.normalized * radius : flat;
        }

        /// <summary>
        /// Everything let into the floor or lying flat on it: the storm crest, the burns where bolts have landed, cracks and
        /// missing paving toward the edge, old sword cuts, puddles and blown paper. All of it lies under three centimetres, so
        /// the marks attacks draw on the ground are always above it, and none of it is solid.
        /// </summary>
        private static void FloorInlay(Transform world, Material dark)
        {
            var group = BuildKit.Group("Floor Inlay", world);
            var random = new System.Random(101);
            var low = Layer(0.002f);
            var bare = Layer(0.002f);
            var paleLayer = Layer(0.004f);
            var goldLayer = Layer(0.004f);
            var burn = Layer(0.006f);
            var ashLayer = Layer(0.008f);
            var nick = Layer(0.0095f);
            var white = Layer(0.012f);
            var redLayer = Layer(0.0135f);

            // The crest. Eight lengths of darker stone make a broken ring; a ray runs out through each gap, long on the
            // compass lines and short between; a bolt is let into each length in dull gold.
            for (int i = 0; i < 8; i++)
            {
                float middle = 22.5f + i * 45f;
                low.Arc(Vector3.zero, 6.9f, 9.3f, middle - 19f, middle + 19f);
                goldLayer.Arc(Vector3.zero, 6.45f, 6.58f, middle - 17.5f, middle + 17.5f);
                goldLayer.Arc(Vector3.zero, 9.62f, 9.74f, middle - 18.5f, middle + 18.5f);
                Bolt(goldLayer, middle, 7.3f, 1.65f);
                float line = i * 45f;
                bool compass = i % 2 == 0;
                paleLayer.Line(Polar(line, 5.6f), Polar(line, compass ? 10.5f : 9.3f), compass ? 0.56f : 0.4f, 0.04f);
            }
            // The thunder god's own mark at the heart: three commas chasing one another, as on his drums.
            goldLayer.Arc(Vector3.zero, 4.55f, 4.66f, 0f, 360f);
            for (int comma = 0; comma < 3; comma++)
            {
                float head = 30f + comma * 120f;
                low.Round(Polar(head, 2.7f), 1.2f, 20);
                const int Steps = 25;
                for (int step = 0; step < Steps; step++)
                {
                    float t0 = step / (float)Steps, t1 = (step + 1) / (float)Steps;
                    float a0 = head + 150f * t0, a1 = head + 150f * t1;
                    float in0 = Mathf.Lerp(1.75f, 4.35f, Mathf.Pow(t0, 0.8f)), in1 = Mathf.Lerp(1.75f, 4.35f, Mathf.Pow(t1, 0.8f));
                    float out0 = Mathf.Lerp(3.9f, 4.35f, t0), out1 = Mathf.Lerp(3.9f, 4.35f, t1);
                    low.Patch(Polar(a0, in0), Polar(a0, out0), Polar(a1, out1), Polar(a1, in1));
                }
            }
            // Outside the pale ring: a thin broken line, a mark at each of sixteen points, short dark rays between them.
            for (int i = 0; i < 16; i++)
            {
                float between = 11.25f + i * 22.5f, on = i * 22.5f;
                paleLayer.Arc(Vector3.zero, 12.55f, 12.67f, between - 8.5f, between + 8.5f);
                low.Line(Polar(between, 11.5f), Polar(between, 12.3f), 0.3f, 0.03f);
                (i % 4 == 0 ? goldLayer : paleLayer).Patch(Polar(on, 12.2f), Polar(on, 12.61f, 0.17f, 0f), Polar(on, 13f), Polar(on, 12.61f, -0.17f, 0f));
            }
            for (int i = 0; i < 14; i++)
            {
                float start = (i + 0.5f) * 360f / 14f;
                low.Arc(Vector3.zero, 14.4f, 14.62f, start + 3.5f, start + 360f / 14f - 3.5f);
            }

            // Where bolts have landed: a black star, ash in the middle of it, and the long burns that ran out along the stone.
            (float radius, float angle, float size)[] strikes =
            {
                (12.6f, 70f, 1.5f), (8.3f, 205f, 1.2f), (13.4f, 300f, 1.7f), (11.9f, 158f, 1.1f), (14.1f, 14f, 1.3f),
                (9.9f, 262f, 1f), (6f, 32f, 0.9f), (13f, 235f, 1.2f), (10.2f, 118f, 1.4f),
            };
            for (int i = 0; i < strikes.Length; i++)
            {
                Vector3 centre = Polar(strikes[i].angle, strikes[i].radius);
                float size = strikes[i].size;
                burn.Burst(centre, size, 300 + i);
                ashLayer.Burst(centre, size * 0.5f, 350 + i, 7, 0.4f);
                int scars = 1 + i % 2;
                for (int scar = 0; scar < scars; scar++)
                {
                    Vector3 heading = Quaternion.Euler(0f, Next(random, 0f, 360f), 0f) * Vector3.forward;
                    Vector3 end = Within(centre + heading * size * Next(random, 2.2f, 3.8f), Radius - 1.1f);
                    if (end.magnitude < 5.6f) continue;
                    var path = burn.Jag(centre + heading * size * 0.4f, end, 0.26f * size, 0.3f, 400 + i * 4 + scar);
                    Vector3 fork = path[path.Count / 2];
                    Vector3 forkEnd = Within(fork + Quaternion.Euler(0f, Next(random, 0f, 1f) < 0.5f ? 38f : -38f, 0f) * heading * size * 1.3f, Radius - 1.1f);
                    if (forkEnd.magnitude > 5.6f) burn.Jag(fork, forkEnd, 0.12f * size, 0.18f, 440 + i * 4 + scar);
                }
            }

            // Cracks running in from the edge, and stones gone from the paving there with the keep's own blocks showing under them.
            for (int i = 0; i < 13; i++)
            {
                float angle = i * 360f / 13f + Next(random, -8f, 8f);
                Vector3 from = Polar(angle, Radius - 0.75f), to = Polar(angle + Next(random, -16f, 16f), Next(random, 11.6f, 14.4f));
                var path = burn.Jag(from, to, 0.085f, 0.28f, 500 + i, 0.5f);
                Vector3 fork = path[path.Count / 3];
                burn.Jag(fork, fork + Quaternion.Euler(0f, angle + Next(random, 100f, 260f), 0f) * Vector3.forward * Next(random, 0.8f, 1.6f), 0.05f, 0.15f, 530 + i, 0.4f);
            }
            burn.Jag(Polar(96f, 10.6f), Polar(128f, 6.2f), 0.07f, 0.3f, 561, 0.5f);
            burn.Jag(Polar(318f, 10.4f), Polar(292f, 5.6f), 0.07f, 0.3f, 562, 0.5f);
            for (int i = 0; i < 22; i++)
            {
                float angle = Next(random, 0f, 360f);
                float radius = Next(random, 15.25f, 16.05f), width = Next(random, 0.8f, 1.5f), depth = Next(random, 0.55f, 0.95f);
                float J() => Next(random, -0.12f, 0.12f);
                bare.Patch(Polar(angle, radius - depth * 0.5f + J(), -width * 0.5f + J(), 0f), Polar(angle, radius - depth * 0.5f + J(), width * 0.5f + J(), 0f),
                    Polar(angle, radius + depth * 0.5f + J(), width * 0.5f + J(), 0f), Polar(angle, radius + depth * 0.5f + J(), -width * 0.5f + J(), 0f));
                // The stone beside a hole is chipped.
                burn.Jag(Polar(angle, radius - depth * 0.5f, width * 0.3f, 0f), Polar(angle + Next(random, -3f, 3f), radius - depth * 0.5f - Next(random, 0.5f, 1.1f)), 0.05f, 0.12f, 600 + i, 0.35f);
            }

            // Old sword cuts: a dark line where the edge went in, a pale one where the stone flaked beside it.
            (float radius, float angle)[] duels = { (7.8f, 150f), (6f, 282f), (9.6f, 22f), (12f, 198f), (8.5f, 332f), (13.5f, 100f), (11.6f, 250f), (10.1f, 186f), (6.4f, 95f), (13.8f, 48f) };
            for (int i = 0; i < duels.Length; i++)
            {
                float heading = Next(random, 0f, 180f);
                int cuts = 2 + random.Next(2);
                for (int cut = 0; cut < cuts; cut++)
                {
                    Vector3 middle = Polar(duels[i].angle, duels[i].radius) + V(Next(random, -0.5f, 0.5f), 0f, Next(random, -0.5f, 0.5f));
                    Vector3 along = Quaternion.Euler(0f, heading + Next(random, -30f, 30f) + (cut == 2 ? 70f : 0f), 0f) * Vector3.forward;
                    Vector3 aside = Vector3.Cross(Vector3.up, along);
                    float reach = Next(random, 0.5f, 1.25f);
                    burn.Line(middle, middle + along * reach, 0.06f, 0.006f);
                    burn.Line(middle, middle - along * reach * 0.8f, 0.06f, 0.006f);
                    nick.Line(middle + aside * 0.05f - along * reach * 0.5f, middle + aside * 0.05f + along * reach * 0.6f, 0.03f, 0.008f);
                }
            }

            // Prayer slips and torn streamers, blown about and lying where the wind dropped them.
            (float radius, float angle)[] drifts = { (14.7f, 26f), (14.9f, 310f), (15f, 77f), (14.3f, 262f), (9f, 140f), (11.7f, 340f), (13.6f, 205f), (12.4f, 118f) };
            for (int i = 0; i < drifts.Length; i++)
            {
                Vector3 centre = Polar(drifts[i].angle, drifts[i].radius);
                int slips = 4 + random.Next(4);
                for (int slip = 0; slip < slips; slip++)
                    Slip(slip % 4 == 3 ? redLayer : white, centre + V(Next(random, -1.3f, 1.3f), 0f, Next(random, -1.1f, 1.1f)), Next(random, 0f, 360f), 0.13f, Next(random, 0.3f, 0.4f));
                // A streamer is one strip folded into a zigzag; here it lies flat.
                Vector3 streamer = centre + V(Next(random, -0.8f, 0.8f), 0f, Next(random, -0.8f, 0.8f));
                float turn = Next(random, 0f, 360f);
                for (int fold = 0; fold < 3; fold++)
                    Slip(white, streamer + Quaternion.Euler(0f, turn, 0f) * V(fold % 2 == 0 ? 0f : 0.11f, 0f, fold * 0.24f), turn + (fold % 2 == 0 ? 10f : -10f), 0.2f, 0.27f);
            }

            // The darker stone is the same paving, so its joints run on through the crest.
            low.Cast("summit_crest_dark", group, BuildKit.Toon(Folder, "m_summit_crest_dark", new Color(0.27f, 0.27f, 0.44f), false, ProceduralTextures.Flagstones(), 0.125f, 0f), false);
            bare.Cast("summit_paving_gone", group, dark, false);
            paleLayer.Cast("summit_crest_pale", group, Flat("m_summit_crest_pale", new Color(0.52f, 0.52f, 0.7f)), false);
            goldLayer.Cast("summit_crest_gold", group, Flat("m_summit_crest_gold", new Color(0.58f, 0.45f, 0.22f)), false);
            burn.Cast("summit_burns", group, Flat("m_summit_burn", new Color(0.08f, 0.075f, 0.15f)), false);
            ashLayer.Cast("summit_ash", group, Flat("m_summit_ash", new Color(0.22f, 0.21f, 0.33f)), false);
            nick.Cast("summit_cuts", group, Flat("m_summit_cut", new Color(0.56f, 0.56f, 0.74f)), false);
            white.Cast("summit_slips", group, Flat("m_summit_slip_white", new Color(0.72f, 0.72f, 0.78f)), false);
            redLayer.Cast("summit_slips_red", group, Flat("m_summit_slip_red", new Color(0.6f, 0.14f, 0.19f)), false);

            // Rain does not fall here, but the cloud leaves water standing in the hollows.
            (float radius, float angle, float size)[] pools =
            {
                (13.3f, 40f, 1.6f), (12.9f, 135f, 2f), (14.2f, 216f, 1.5f), (13.1f, 286f, 1.9f), (14.7f, 172f, 1.2f), (14.4f, 338f, 1.4f),
                (8f, 62f, 1.3f), (8.6f, 242f, 1.4f),
            };
            for (int i = 0; i < pools.Length; i++)
                Dressing.Puddle(group, Polar(pools[i].angle, pools[i].radius, pools[i].radius < 11.4f ? 0.003f : 0f), pools[i].size, 700 + i);
        }

        // A bolt drawn as three strokes, running outward along a line from the middle.
        private static void Bolt(Dressing.Mould layer, float angle, float from, float length)
        {
            Vector3 a = Polar(angle, from, -0.16f, 0f), b = Polar(angle, from + length * 0.45f, 0.2f, 0f);
            Vector3 c = Polar(angle, from + length * 0.52f, -0.18f, 0f), d = Polar(angle, from + length, 0.12f, 0f);
            layer.Line(a, b, 0.2f, 0.17f);
            layer.Line(b, c, 0.17f, 0.17f);
            layer.Line(c, d, 0.17f, 0.02f);
        }

        // A slip of paper lying flat.
        private static void Slip(Dressing.Mould layer, Vector3 centre, float yaw, float width, float length)
        {
            if (new Vector2(centre.x, centre.z).magnitude > Radius - 1f) return;
            Quaternion turn = Quaternion.Euler(0f, yaw, 0f);
            Vector3 w = turn * V(width * 0.5f, 0f, 0f), l = turn * V(0f, 0f, length * 0.5f);
            layer.Patch(centre - w - l, centre + w - l, centre + w + l, centre - w + l);
        }

        private static Dressing.Mould stone, shade, pale, glow, timber, banner, bannerEdge, gilt, straw, paper, cord, redCloth, tile, window;
        private static Dressing.Mould[] moulds;

        private static void SetAt(Vector3 at, Quaternion turn, float scale = 1f)
        {
            foreach (var mould in moulds) mould.Place(at, turn, scale);
        }

        private static void SetHome()
        {
            foreach (var mould in moulds) mould.Home();
        }

        /// <summary>
        /// The edge of the floor and what stands on it, all beyond where anyone walks: a stepped plinth going down into
        /// the cloud, a low broken balustrade, stone lanterns with spirit flames round the south, guardians and banners
        /// round the north, a sacred rope between the gate-posts, offerings, and the keep's roofs coming up through the cloud.
        /// </summary>
        private static void Rim(Transform world, Material dark, Material rune, Material spirit, int postCount)
        {
            var group = BuildKit.Group("Rim", world);
            var random = new System.Random(31);
            var plinth = BuildKit.Toon(Folder, "m_summit_plinth", new Color(0.29f, 0.29f, 0.45f), false, ProceduralTextures.WallBlocks(), 0.2f, 0.02f);
            stone = new Dressing.Mould();
            shade = new Dressing.Mould();
            pale = new Dressing.Mould();
            glow = new Dressing.Mould();
            timber = new Dressing.Mould();
            banner = new Dressing.Mould();
            bannerEdge = new Dressing.Mould();
            gilt = new Dressing.Mould();
            straw = new Dressing.Mould();
            paper = new Dressing.Mould();
            cord = new Dressing.Mould();
            redCloth = new Dressing.Mould();
            tile = new Dressing.Mould();
            window = new Dressing.Mould();
            moulds = new[] { stone, shade, pale, glow, timber, banner, bannerEdge, gilt, straw, paper, cord, redCloth, tile, window };

            // The floor stands on three steps of masonry that go down into the cloud.
            Disc("Plinth Upper", group, -0.45f, 18f, 1.2f, plinth, false);
            Disc("Plinth Middle", group, -1.1f, 19f, 1.4f, dark, false);
            Disc("Plinth Lower", group, -1.9f, 19.9f, 1.8f, plinth, false);
            for (int i = 0; i < 28; i++)
            {
                float angle = (i + 0.5f) * 360f / 28f;
                if (Mathf.Abs(Mathf.DeltaAngle(angle, 180f)) < 10f) continue;
                SetAt(Vector3.zero, Quaternion.Euler(0f, angle, 0f));
                stone.Box(V(0f, -0.24f, 17.3f), V(1.1f, 0.44f, 0.9f), null, 0.75f);
                shade.Box(V(0f, -0.8f, 18.3f), V(1.3f, 0.64f, 0.9f), null, 0.75f);
                stone.Box(V(0f, -1.52f, 19.25f), V(1.5f, 0.8f, 0.8f), null, 0.75f);
            }

            // The balustrade, on the outermost half metre of the floor. Lengths of it have gone over the edge, and all
            // of it is gone at the north, where something far bigger than a man has come up over it.
            const float Rail = 16.72f;
            const int Spans = 4;
            float sweep = 360f / postCount;
            bool Broken(float angle) => Mathf.Abs(Mathf.DeltaAngle(angle, 0f)) < 21f;
            for (int bay = 0; bay < postCount; bay++)
            {
                float start = (bay + 0.5f) * sweep;
                if (!Broken(start))
                {
                    SetAt(Polar(start, Rail), Quaternion.Euler(0f, start, 0f));
                    stone.Box(V(0f, 0.38f, 0f), V(0.46f, 0.76f, 0.46f));
                    pale.Box(V(0f, 0.8f, 0f), V(0.58f, 0.1f, 0.58f));
                    shade.Box(V(0f, 0.93f, 0f), V(0.4f, 0.16f, 0.4f), null, 0f);
                }
                SetHome();
                for (int span = 0; span < Spans; span++)
                {
                    float a0 = start + sweep * span / Spans, a1 = start + sweep * (span + 1) / Spans, middle = (a0 + a1) * 0.5f;
                    // The way down is at the south: no rail across it.
                    if (Mathf.Abs(Mathf.DeltaAngle(middle, 180f)) < 7f) continue;
                    double roll = random.NextDouble();
                    bool gone = Broken(middle) || roll < 0.2;
                    Vector3 p0 = Polar(a0, Rail), p1 = Polar(a1, Rail);
                    if (gone)
                    {
                        shade.Beam(p0 + Vector3.up * 0.07f, Vector3.Lerp(p0, p1, 0.3f) + Vector3.up * 0.07f, 0.34f, 0.14f);
                        for (int i = 0; i < 2; i++)
                        {
                            float size = Next(random, 0.25f, 0.5f);
                            stone.Box(Polar(middle + Next(random, -2.5f, 2.5f), Next(random, 17.25f, 17.85f), -0.45f + size * 0.28f), V(size * 1.3f, size * 0.6f, size * 0.8f),
                                Quaternion.Euler(Next(random, -12f, 12f), Next(random, 0f, 360f), Next(random, -12f, 12f)));
                        }
                        continue;
                    }
                    shade.Beam(p0 + Vector3.up * 0.07f, p1 + Vector3.up * 0.07f, 0.34f, 0.14f);
                    for (int i = 0; i < 3; i++) shade.Drum(Vector3.Lerp(p0, p1, (i + 0.5f) / 3f) + Vector3.up * 0.32f, 0.17f, 0.38f, null, 6);
                    if (roll > 0.34) pale.Beam(p0 + Vector3.up * 0.56f, p1 + Vector3.up * 0.56f, 0.26f, 0.12f);
                    // The rail of this length lies broken on the step below.
                    else pale.Beam(Polar(middle - 2f, 17.45f, -0.39f), Polar(middle + 1.4f, 17.7f, -0.39f), 0.26f, 0.12f);
                }
            }
            // The way down: newels either side, and a flight of steps that goes down into the cloud.
            foreach (float side in new[] { -1f, 1f })
            {
                SetAt(Polar(180f + side * sweep * 0.25f, Rail), Quaternion.Euler(0f, 180f, 0f));
                stone.Box(V(0f, 0.45f, 0f), V(0.52f, 0.9f, 0.52f));
                pale.Box(V(0f, 0.94f, 0f), V(0.64f, 0.1f, 0.64f));
                shade.Box(V(0f, 1.09f, 0f), V(0.46f, 0.2f, 0.46f), null, 0f);
            }
            SetAt(Polar(180f, 17f), Quaternion.Euler(0f, 180f, 0f));
            for (int i = 0; i < 6; i++)
            {
                stone.Box(V(0f, -0.4f - i * 0.22f, 0.25f + i * 0.5f), V(3.3f, 0.6f, 0.5f));
                pale.Box(V(0f, -0.115f - i * 0.22f, 0.46f + i * 0.5f), V(3.3f, 0.05f, 0.09f));
            }
            foreach (float side in new[] { -1f, 1f })
            {
                shade.Box(V(side * 1.9f, -0.35f, 0.75f), V(0.5f, 1f, 1.5f));
                pale.Box(V(side * 1.9f, 0.18f, 0.75f), V(0.6f, 0.07f, 1.6f));
                shade.Box(V(side * 1.9f, -1.05f, 2.25f), V(0.5f, 1.2f, 1.5f));
                pale.Box(V(side * 1.9f, -0.42f, 2.25f), V(0.6f, 0.07f, 1.6f));
            }

            // Stone lanterns round the south, standing on the upper step so their tops are below a man's waist seen from
            // the floor. A spirit flame burns in each.
            foreach (float angle in new[] { 103f, 128.6f, 154.3f, 205.7f, 231.4f, 257f })
            {
                const float Scale = 0.72f;
                Vector3 foot = Polar(angle, 17.45f, -0.45f);
                SetAt(foot, Quaternion.Euler(0f, angle, 0f), Scale);
                shade.Box(V(0f, 0.125f, 0f), V(0.74f, 0.25f, 0.74f), null, 0.75f);
                stone.Drum(V(0f, 0.5f, 0f), 0.28f, 0.5f, null, 8);
                pale.Box(V(0f, 0.8f, 0f), V(0.64f, 0.1f, 0.64f));
                foreach (float x in new[] { -0.19f, 0.19f })
                    foreach (float z in new[] { -0.19f, 0.19f })
                        shade.Box(V(x, 1.07f, z), V(0.08f, 0.44f, 0.08f));
                glow.Box(V(0f, 1.02f, 0f), V(0.17f, 0.3f, 0.17f), Quaternion.Euler(0f, 45f, 0f), 0f);
                pale.Box(V(0f, 1.32f, 0f), V(0.84f, 0.07f, 0.84f));
                shade.Box(V(0f, 1.5f, 0f), V(0.68f, 0.3f, 0.68f), null, 0f);
                stone.Ball(V(0f, 1.68f, 0f), V(0.15f, 0.15f, 0.15f), null, 6, 4);
                Wisp(group, foot + Vector3.up * (1.02f * Scale), spirit, 0.3f, true);
            }

            // Guardians round the north: the thunder god's own attendants, squat, horned, a ring of drums at their backs.
            float[] guardians = { 51.4f, -51.4f, 77.1f, -77.1f };
            for (int g = 0; g < guardians.Length; g++)
            {
                float angle = guardians[g];
                // One leans, where the step has settled under it.
                SetAt(Polar(angle, 18.15f, -0.45f), Quaternion.Euler(g == 3 ? 5f : 0f, angle + 180f, g == 3 ? -4f : 0f));
                shade.Box(V(0f, 0.25f, 0f), V(1.8f, 0.5f, 1.8f), null, 0.75f);
                stone.Box(V(0f, 0.95f, 0f), V(1.3f, 0.9f, 1.3f));
                pale.Box(V(0f, 1.46f, 0f), V(1.52f, 0.12f, 1.52f));
                const float T = 1.52f;
                stone.Box(V(0f, T + 0.5f, 0f), V(1.05f, 1f, 0.82f), null, 0.75f);
                shade.Box(V(0f, T + 1.02f, 0f), V(0.88f, 0.14f, 0.68f));
                stone.Box(V(0f, T + 1.4f, 0f), V(0.9f, 0.62f, 0.56f));
                pale.Box(V(0f, T + 1.76f, 0f), V(1.5f, 0.24f, 0.62f));
                // One arm raised with the drumstick, the other fist on the knee.
                stone.Beam(V(-0.68f, T + 1.8f, 0f), V(-1.05f, T + 2.38f, 0.25f), 0.24f, 0.24f);
                shade.Pole(V(-1.05f, T + 2.3f, 0.25f), V(-1.05f, T + 2.8f, 0.62f), 0.09f);
                shade.Ball(V(-1.05f, T + 2.83f, 0.65f), V(0.26f, 0.26f, 0.26f), null, 6, 4);
                stone.Beam(V(0.68f, T + 1.74f, 0f), V(0.72f, T + 1.12f, 0.42f), 0.24f, 0.24f);
                if (g != 2)
                {
                    stone.Ball(V(0f, T + 2.14f, 0.05f), V(0.52f, 0.54f, 0.52f), null, 8, 5);
                    foreach (float side in new[] { -1f, 1f }) pale.Box(V(side * 0.16f, T + 2.48f, 0.08f), V(0.11f, 0.3f, 0.11f), Quaternion.Euler(0f, 0f, side * -14f), 0f);
                    shade.Box(V(0f, T + 2.06f, 0.26f), V(0.36f, 0.1f, 0.1f));
                }
                else
                {
                    // Its head came off long ago and lies at its feet.
                    stone.Box(V(0f, T + 1.93f, 0f), V(0.3f, 0.12f, 0.3f), Quaternion.Euler(0f, 0f, 9f));
                    stone.Ball(V(0.75f, 0.26f, 1.25f), V(0.52f, 0.54f, 0.52f), Quaternion.Euler(60f, 30f, 0f), 8, 5);
                    pale.Box(V(0.95f, 0.3f, 1.42f), V(0.11f, 0.3f, 0.11f), Quaternion.Euler(70f, 30f, 0f), 0f);
                }
                Vector3 last = Vector3.zero;
                for (int d = 0; d < 7; d++)
                {
                    float around = (-105f + d * 35f) * Mathf.Deg2Rad;
                    Vector3 centre = V(Mathf.Sin(around) * 1.22f, T + 1.72f + Mathf.Cos(around) * 1.22f, -0.4f);
                    bool lost = g == 1 && d >= 5 || g == 2 && d == 0;
                    if (!lost)
                    {
                        shade.Drum(centre, 0.4f, 0.16f, Quaternion.Euler(90f, 0f, 0f), 8);
                        pale.Drum(centre + V(0f, 0f, 0.09f), 0.28f, 0.03f, Quaternion.Euler(90f, 0f, 0f), 8);
                        if (d > 0 && last != Vector3.zero) stone.Beam(last, centre, 0.08f, 0.08f);
                    }
                    last = lost ? Vector3.zero : centre;
                }
            }

            // Banner poles, north of the east-west line only: tall, and so never where the camera travels.
            foreach (float angle in new[] { 25.7f, -25.7f })
            {
                // The cloth is turned toward the south as well as the middle, so the camera sees its face and not its edge.
                SetAt(Polar(angle, 18.3f, -0.45f), Quaternion.Euler(0f, 180f + angle * 0.45f, 0f));
                shade.Box(V(0f, 0.3f, 0f), V(0.72f, 0.6f, 0.72f), null, 0.75f);
                timber.Pole(V(0f, 0.5f, 0f), V(0f, 6.3f, 0f), 0.14f, 6);
                timber.Beam(V(-0.75f, 6f, 0.04f), V(0.75f, 6f, 0.04f), 0.09f, 0.09f);
                gilt.Ball(V(0f, 6.38f, 0f), V(0.24f, 0.28f, 0.24f), null, 6, 4);
                Quaternion blown = Quaternion.Euler(-5f, 0f, 0f);
                banner.Box(V(0f, 4.25f, 0.2f), V(1.15f, 3.4f, 0.04f), blown);
                bannerEdge.Box(V(0f, 5.84f, 0.09f), V(1.2f, 0.18f, 0.06f), blown);
                bannerEdge.Box(V(0f, 2.75f, 0.36f), V(1.16f, 0.12f, 0.05f), blown);
                foreach (float side in new[] { -1f, 1f })
                    banner.Box(V(side * 0.3f, 2.32f, 0.38f), V(0.5f, 0.56f, 0.04f), blown * Quaternion.Euler(0f, 0f, side * -7f));
                gilt.Box(V(0.06f, 4.75f, 0.2f), V(0.13f, 0.52f, 0.02f), blown * Quaternion.Euler(0f, 0f, 22f));
                gilt.Box(V(-0.06f, 4.38f, 0.23f), V(0.13f, 0.52f, 0.02f), blown * Quaternion.Euler(0f, 0f, 22f));
                gilt.Box(V(0f, 3.5f, 0.3f), V(0.5f, 0.07f, 0.02f), blown);
            }

            // A sacred rope from gate-post to gate-post round the north, hung with folded paper.
            SetHome();
            for (int i = 0; i < postCount; i++)
            {
                float a0 = (i + 0.5f) * sweep, a1 = (i + 1.5f) * sweep;
                float n0 = Mathf.Abs(Mathf.DeltaAngle(a0, 0f)), n1 = Mathf.Abs(Mathf.DeltaAngle(a1, 0f));
                if (n0 >= 95f || n1 >= 95f) continue;
                Vector3 from = Polar(a0, Radius - 1.75f, Mathf.Lerp(6.5f, 4.2f, n0 / 95f) * 0.6f), to = Polar(a1, Radius - 1.75f, Mathf.Lerp(6.5f, 4.2f, n1 / 95f) * 0.6f);
                Vector3 At(float t) => Vector3.Lerp(from, to, t) + Vector3.down * (0.6f * 4f * t * (1f - t));
                const int Lengths = 8;
                for (int length = 0; length < Lengths; length++)
                {
                    float thick = 1f - Mathf.Abs((length + 0.5f) / Lengths - 0.5f) * 2f;
                    cord.Pole(At(length / (float)Lengths), At((length + 1.06f) / Lengths), 0.1f + thick * 0.09f, 6);
                }
                foreach (float t in new[] { 0.14f, 0.32f, 0.68f, 0.86f })
                {
                    Vector3 hang = At(t);
                    Quaternion face = Quaternion.LookRotation(-new Vector3(hang.x, 0f, hang.z).normalized, Vector3.up);
                    for (int fold = 0; fold < 3; fold++)
                        paper.Box(hang + face * V(fold % 2 == 0 ? 0f : 0.11f, -0.2f - fold * 0.23f, 0f), V(0.23f, 0.26f, 0.02f), face * Quaternion.Euler(0f, 0f, fold % 2 == 0 ? 8f : -8f));
                }
                cord.Box(At(0.5f) + Vector3.down * 0.34f, V(0.18f, 0.5f, 0.18f), Quaternion.Euler(180f, 0f, 0f), 0.5f);
            }

            // Offerings left on the upper step: casks, bales, a bowl of coin, candles gone cold.
            Offerings(Polar(43f, 17.5f, -0.45f), 43f + 180f + 20f);
            Offerings(Polar(-59.5f, 17.5f, -0.45f), -59.5f + 180f - 15f);
            Offerings(Polar(116f, 17.45f, -0.45f), 116f + 180f + 30f);
            Offerings(Polar(-116f, 17.45f, -0.45f), -116f + 180f - 25f);

            // The keep's roofs, close by, standing up through the cloud, their windows lit from inside by the storm.
            Tower(V(22.9f, 0f, 4.5f), 20f, 2.6f, 1.5f, 0f);
            Tower(V(-23.2f, 0f, -1f), -15f, 3f, 0.6f, 5f);
            Tower(V(-20f, 0f, 14.8f), 30f, 3.2f, 4.6f, 0f);
            Tower(V(21f, 0f, 13f), -20f, 2.6f, 3.3f, -3f);
            Tower(V(16.4f, 0f, -17.2f), 45f, 2.3f, -1.1f, 0f);
            Tower(V(-18.2f, 0f, -14.8f), -40f, 2.6f, -0.9f, -7f);
            Ridge(V(22.8f, -1.5f, -6.5f), 80f, 6.5f, 2.6f);
            Ridge(V(-23f, -0.9f, 7.2f), -70f, 7f, 2.8f);
            foreach (var at in new[] { V(21.4f, 0.4f, 1.6f), V(-21.6f, -0.2f, 1.8f) })
            {
                var lightObject = new GameObject("Storm Light");
                lightObject.transform.SetParent(group, false);
                lightObject.transform.position = at;
                var storm = lightObject.AddComponent<Light>();
                storm.type = LightType.Point;
                storm.color = new Color(0.45f, 0.62f, 1f);
                storm.range = 9f;
                storm.intensity = 12f;
                storm.shadows = LightShadows.None;
            }

            stone.Cast("summit_rim_stone", group, BuildKit.Toon(Folder, "m_summit_rim_stone", new Color(0.34f, 0.34f, 0.5f), false, null, 0f, 0.02f));
            shade.Cast("summit_rim_shade", group, BuildKit.Toon(Folder, "m_summit_rim_shade", new Color(0.21f, 0.21f, 0.35f), false, null, 0f, 0.02f));
            pale.Cast("summit_rim_pale", group, BuildKit.Toon(Folder, "m_summit_rim_pale", new Color(0.48f, 0.48f, 0.66f), false, null, 0f, 0.02f));
            glow.Cast("summit_rim_glow", group, rune, false);
            timber.Cast("summit_rim_timber", group, BuildKit.Toon(Folder, "m_summit_timber", new Color(0.3f, 0.21f, 0.23f), false, null, 0f, 0.015f));
            banner.Cast("summit_rim_banner", group, BuildKit.Toon(Folder, "m_summit_banner", new Color(0.42f, 0.1f, 0.2f), false, null, 0f, 0.015f));
            bannerEdge.Cast("summit_rim_banner_edge", group, Flat("m_summit_banner_edge", new Color(0.16f, 0.14f, 0.28f)), false);
            gilt.Cast("summit_rim_gilt", group, Flat("m_summit_crest_gold", new Color(0.58f, 0.45f, 0.22f)), false);
            straw.Cast("summit_rim_straw", group, BuildKit.Toon(Folder, "m_summit_straw", new Color(0.5f, 0.45f, 0.34f), false, null, 0f, 0.015f));
            paper.Cast("summit_rim_paper", group, Flat("m_summit_slip_white", new Color(0.72f, 0.72f, 0.78f)), false);
            cord.Cast("summit_rim_rope", group, BuildKit.Toon(Folder, "m_summit_rope", new Color(0.52f, 0.45f, 0.34f), false, null, 0f, 0.012f));
            redCloth.Cast("summit_rim_red", group, Flat("m_summit_slip_red", new Color(0.6f, 0.14f, 0.19f)), false);
            tile.Cast("summit_keep_roofs", group, BuildKit.Toon(Folder, "m_summit_roof", new Color(0.19f, 0.25f, 0.39f), false, null, 0f, 0.025f));
            window.Cast("summit_keep_windows", group, Flat("m_summit_window", new Color(0.4f, 0.55f, 0.9f)).Lit(Color.black, 0.3f, new Color(0.3f, 0.6f, 1.4f)), false);
            moulds = null;
        }

        // A heap of offerings, about a metre and a half across and under a metre high.
        private static void Offerings(Vector3 foot, float yaw)
        {
            SetAt(foot, Quaternion.Euler(0f, yaw, 0f));
            void Cask(Vector3 at, Quaternion turn)
            {
                straw.Drum(at, 0.56f, 0.6f, turn, 10);
                shade.Drum(at + turn * V(0f, 0.2f, 0f), 0.6f, 0.07f, turn, 10);
                shade.Drum(at + turn * V(0f, -0.2f, 0f), 0.6f, 0.07f, turn, 10);
                paper.Box(at + turn * V(0f, 0f, 0.275f), V(0.3f, 0.28f, 0.04f), turn);
                redCloth.Box(at + turn * V(0f, 0f, 0.3f), V(0.1f, 0.18f, 0.02f), turn);
            }
            Cask(V(0f, 0.3f, 0f), Quaternion.identity);
            Cask(V(0.63f, 0.3f, 0.08f), Quaternion.Euler(0f, 25f, 0f));
            Cask(V(0.3f, 0.86f, 0.03f), Quaternion.Euler(8f, -15f, 6f));
            foreach (var bale in new[] { V(-0.72f, 0.2f, 0.24f), V(-0.72f, 0.2f, -0.2f), V(-0.72f, 0.54f, 0.02f) })
            {
                Quaternion lying = Quaternion.Euler(0f, 8f, 90f);
                straw.Drum(bale, 0.4f, 0.72f, lying, 8);
                cord.Drum(bale, 0.43f, 0.08f, lying, 8);
            }
            redCloth.Box(V(0.1f, 0.012f, 0.8f), V(1f, 0.024f, 0.52f));
            pale.Drum(V(0.1f, 0.07f, 0.8f), 0.38f, 0.1f, null, 10);
            gilt.Drum(V(0.1f, 0.125f, 0.8f), 0.28f, 0.03f, null, 10);
            for (int i = 0; i < 3; i++)
            {
                paper.Drum(V(-0.32f + i * 0.09f, 0.1f + i % 2 * 0.03f, 0.86f - i * 0.1f), 0.07f, 0.16f + i % 2 * 0.06f, null, 6);
                glow.Box(V(-0.32f + i * 0.09f, 0.24f + i % 2 * 0.06f, 0.86f - i * 0.1f), V(0.05f, 0.1f, 0.05f), null, 0f);
            }
        }

        // One of the keep's towers, measured down from the point of its roof, which stands at the given height.
        private static void Tower(Vector3 at, float yaw, float width, float peak, float lean)
        {
            float w = width;
            SetAt(V(at.x, peak, at.z), Quaternion.Euler(lean, yaw, lean * 0.6f));
            gilt.Pole(V(0f, -0.1f * w, 0f), V(0f, 0.55f * w, 0f), 0.07f * w, 6, 0.3f);
            tile.Box(V(0f, -0.45f * w, 0f), V(1.5f * w, 0.9f * w, 1.5f * w), null, 0f);
            pale.Box(V(0f, -0.93f * w, 0f), V(1.62f * w, 0.07f * w, 1.62f * w));
            foreach (float x in new[] { -1f, 1f })
                foreach (float z in new[] { -1f, 1f })
                {
                    pale.Beam(V(0f, 0f, 0f), V(x * 0.75f * w, -0.9f * w, z * 0.75f * w), 0.07f * w, 0.07f * w);
                    pale.Beam(V(x * 0.52f * w, -1.95f * w, z * 0.52f * w), V(x * 1.15f * w, -2.55f * w, z * 1.15f * w), 0.07f * w, 0.07f * w);
                }
            // Courses of tile, drawn as darker lines round the roof: from above they are what says roof and not slab.
            foreach (var course in new[] { V(0.27f, -0.32f, 0f), V(0.52f, -0.62f, 0f), V(0.83f, -2.25f, 0f) })
            {
                float half = course.x * w + 0.02f * w, y = course.y * w + 0.03f * w;
                shade.Beam(V(-half, y, -half), V(half, y, -half), 0.05f * w, 0.05f * w);
                shade.Beam(V(-half, y, half), V(half, y, half), 0.05f * w, 0.05f * w);
                shade.Beam(V(-half, y, -half), V(-half, y, half), 0.05f * w, 0.05f * w);
                shade.Beam(V(half, y, -half), V(half, y, half), 0.05f * w, 0.05f * w);
            }
            shade.Box(V(0f, -1.5f * w, 0f), V(0.9f * w, 1.1f * w, 0.9f * w));
            window.Box(V(0f, -1.42f * w, 0f), V(0.93f * w, 0.36f * w, 0.2f * w));
            window.Box(V(0f, -1.42f * w, 0f), V(0.2f * w, 0.36f * w, 0.93f * w));
            tile.Box(V(0f, -2.25f * w, 0f), V(2.3f * w, 0.6f * w, 2.3f * w), null, 0.45f);
            pale.Box(V(0f, -2.58f * w, 0f), V(2.42f * w, 0.07f * w, 2.42f * w));
            shade.Box(V(0f, -4.6f * w, 0f), V(1.3f * w, 4f * w, 1.3f * w));
            window.Box(V(0.3f * w, -3.1f * w, 0f), V(0.16f * w, 0.4f * w, 1.33f * w));
            window.Box(V(-0.3f * w, -3.1f * w, 0f), V(0.16f * w, 0.4f * w, 1.33f * w));
            window.Box(V(0f, -3.1f * w, 0f), V(1.33f * w, 0.4f * w, 0.16f * w));
        }

        // A long hall roof with its ridge just clear of the cloud.
        private static void Ridge(Vector3 ridge, float yaw, float length, float width)
        {
            SetAt(ridge, Quaternion.Euler(0f, yaw, 0f));
            tile.Box(V(0f, -0.6f, 0f), V(width * 2f, 1.2f, length), null, 0.12f);
            pale.Box(V(0f, 0.02f, 0f), V(0.3f, 0.16f, length * 0.2f));
            pale.Box(V(0f, -1.24f, 0f), V(width * 2.1f, 0.1f, length * 1.04f));
            shade.Box(V(0f, -3.3f, 0f), V(width * 1.5f, 4f, length * 0.85f));
            foreach (float along in new[] { -0.3f, 0f, 0.3f })
                window.Box(V(0f, -1.75f, along * length * 0.85f), V(width * 1.53f, 0.4f, 0.3f));
            gilt.Box(V(0f, 0.2f, length * 0.1f), V(0.14f, 0.34f, 0.14f), null, 0.3f);
            gilt.Box(V(0f, 0.2f, -length * 0.1f), V(0.14f, 0.34f, 0.14f), null, 0.3f);
        }

        /// <summary>
        /// Cloud close under the rim, shaped: each bank a dark flat underside, a heap of mid tone, and lit tops; and
        /// wisps of it lying over the steps of the plinth. None stands at the north, where the god's second form comes up.
        /// </summary>
        private static void CloudBanks(Transform clouds, Material cloud)
        {
            var group = BuildKit.Group("Near Banks", clouds);
            var random = new System.Random(57);
            var brightM = Flat("m_summit_cloud_bright", new Color(0.41f, 0.42f, 0.69f));
            var bodyM = Flat("m_summit_cloud_body", new Color(0.3f, 0.3f, 0.53f));
            bodyM.SetColor("_ShadeColor", new Color(0.42f, 0.4f, 0.74f));
            var wispM = BuildKit.Glow(Folder, "m_summit_wisp", new Color(0.46f, 0.5f, 0.84f, 0.5f), 0, false, 1.3f, 0.25f);
            brightM.SetColor("_ShadeColor", new Color(0.5f, 0.5f, 0.82f));
            var underM = Flat("m_summit_cloud_under", new Color(0.17f, 0.17f, 0.37f));
            var litM = Flat("m_summit_cloud_lit", new Color(0.34f, 0.39f, 0.7f)).Lit(Color.black, 0.3f, new Color(0.03f, 0.07f, 0.2f));
            var bright = new Dressing.Mould();
            var body = new Dressing.Mould();
            var under = new Dressing.Mould();
            var lit = new Dressing.Mould();

            void Bank(float angle, float distance, float top, float size, bool stormLit)
            {
                Vector3 centre = Polar(angle, distance, top - size * 0.36f);
                Quaternion turn = Quaternion.Euler(0f, angle + 90f + Next(random, -25f, 25f), 0f);
                under.Ball(centre + Vector3.down * (size * 0.2f), V(size * 1.8f, size * 0.36f, size * 1.3f), turn, 12, 6);
                int lumps = 3 + random.Next(2);
                for (int i = 0; i < lumps; i++)
                {
                    float along = (i - (lumps - 1) * 0.5f) / lumps * 1.5f;
                    Vector3 at = centre + turn * V(along * size, Next(random, -0.04f, 0.08f) * size, Next(random, -0.22f, 0.22f) * size);
                    Vector3 lump = V(Next(random, 0.62f, 0.9f), Next(random, 0.36f, 0.5f), Next(random, 0.55f, 0.8f)) * size;
                    (stormLit && i == 1 ? lit : body).Ball(at, lump, turn, 12, 6);
                    // The lit top sits toward the light, which comes from the south-east.
                    bright.Ball(at + V(0.05f * size, lump.y * 0.3f, -0.07f * size), V(lump.x * 0.66f, lump.y * 0.6f, lump.z * 0.66f), turn, 12, 6);
                }
            }

            for (int i = 0; i < 18; i++)
            {
                float angle = i * 20f + Next(random, -6f, 6f);
                if (Mathf.Abs(Mathf.DeltaAngle(angle, 0f)) < 38f) continue;
                bool south = Mathf.Abs(Mathf.DeltaAngle(angle, 180f)) < 80f;
                float size = Next(random, 4f, 6f);
                Bank(angle, Next(random, 22.5f, 25.5f), south ? Next(random, -1.7f, -1.1f) : Next(random, -0.8f, -0.2f), size, i % 4 == 1);
            }
            // Taller heaps behind the east and west of the gate-posts, as a backdrop.
            foreach (float angle in new[] { 48f, 72f, 96f, -44f, -70f, -98f })
                Bank(angle + Next(random, -5f, 5f), Next(random, 28f, 31f), Next(random, 0.8f, 2.4f), Next(random, 6f, 8f), angle == 72f || angle == -70f);

            // Wisps lying over the steps.
            for (int i = 0; i < 18; i++)
            {
                float angle = i * 20f + Next(random, -7f, 7f);
                if (Mathf.Abs(Mathf.DeltaAngle(angle, 0f)) < 26f || Mathf.Abs(Mathf.DeltaAngle(angle, 180f)) < 12f) continue;
                float distance = Next(random, 18.1f, 19.8f);
                float step = distance < 19f ? -0.45f : -1.1f;
                if (i % 3 == 0) step = distance < 18.6f ? -0.45f : -1.1f;
                // Soft, flat and see-through: mist lying on the stone, not a thing standing on it.
                float length = Next(random, 3f, 5f);
                for (int sheet = 0; sheet < 2; sheet++)
                {
                    Vector3 at = Polar(angle + sheet * Next(random, 2f, 5f), distance + sheet * 0.5f, step + 0.07f + sheet * 0.05f);
                    var wisp = BuildKit.Shape(PrimitiveType.Quad, "Wisp", group, at, V(length * (1f - sheet * 0.35f), length * 0.42f, 1f), wispM, V(90f, angle + Next(random, -15f, 15f), 0f));
                    wisp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    wisp.receiveShadows = false;
                }
            }

            foreach (var made in new[] { bright.Cast("summit_cloud_bright", group, brightM, false), body.Cast("summit_cloud_body", group, bodyM, false),
                under.Cast("summit_cloud_under", group, underM, false), lit.Cast("summit_cloud_lit", group, litM, false) })
                if (made != null) made.receiveShadows = false;
        }

        // A small blue flame that burns without fuel, and the little cold light it gives.
        private static void Wisp(Transform parent, Vector3 at, Material material, float size, bool lit)
        {
            var flameObject = new GameObject("Spirit Wisp");
            flameObject.transform.SetParent(parent, false);
            flameObject.transform.position = at;
            flameObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var system = flameObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.7f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.8f, size * 1.5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 1f, 1f, 1f), Color.white);
            main.maxParticles = 16;
            var emission = system.emission;
            emission.rateOverTime = 7f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = size * 0.2f;
            var over = system.sizeOverLifetime;
            over.enabled = true;
            over.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));
            var renderer = flameObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            if (!lit) return;
            var lightObject = new GameObject("Wisp Light");
            lightObject.transform.SetParent(flameObject.transform, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.45f, 0.7f, 1f);
            light.range = 5.5f;
            light.intensity = 4.5f;
            light.shadows = LightShadows.None;
        }

        private static GodReward Reward(string name, Vector3 at, Material heart, Material metal, string prompt)
        {
            var root = new GameObject(name);
            root.transform.position = at;
            BuildKit.Shape(PrimitiveType.Cylinder, "Plinth", root.transform, new Vector3(0f, 0.15f, 0f), new Vector3(1.2f, 0.15f, 1.2f), metal);
            var orb = BuildKit.Shape(PrimitiveType.Sphere, "Heart", root.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.7f, 0.7f, 0.7f), heart);
            orb.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cube, "Spike", root.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.16f, 1.4f, 0.16f), heart, new Vector3(0f, 45f, 0f));
            var lightObject = new GameObject("Heart Light");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = heart.GetColor("_EmissionColor").maxColorComponent > 0f ? (Color)(heart.GetColor("_EmissionColor") / heart.GetColor("_EmissionColor").maxColorComponent) : Color.white;
            light.range = 7f;
            light.intensity = 9f;
            light.shadows = LightShadows.None;
            var reward = root.AddComponent<GodReward>();
            reward.Prompt = prompt;
            reward.Range = 2.4f;
            return reward;
        }

        // A blue flame that burns without fuel on top of a post.
        private static void SpiritFlame(Transform parent, Vector3 at, Material material)
        {
            var flameObject = new GameObject("Spirit Flame");
            flameObject.transform.SetParent(parent, false);
            flameObject.transform.position = at;
            flameObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var system = flameObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.4f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 1f, 1f, 1f), Color.white);
            main.maxParticles = 40;
            var emission = system.emission;
            emission.rateOverTime = 9f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.25f;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));
            var renderer = flameObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var lightObject = new GameObject("Spirit Light");
            lightObject.transform.SetParent(flameObject.transform, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.45f, 0.7f, 1f);
            light.range = 10f;
            light.intensity = 7f;
            light.shadows = LightShadows.None;
        }

        private static void Disc(string name, Transform parent, float top, float radius, float thickness, Material material, bool solid)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            disc.transform.position = new Vector3(0f, top - thickness * 0.5f, 0f);
            disc.transform.localScale = new Vector3(radius * 2f, thickness * 0.5f, radius * 2f);
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            if (solid)
            {
                // A box under the whole floor is a simpler thing to stand on than a many-sided drum.
                var floor = disc.AddComponent<BoxCollider>();
                floor.size = new Vector3(1.1f, 2f, 1.1f);
            }
            // Unity's own cylinder shows its twenty corners at this size.
            disc.GetComponent<MeshFilter>().sharedMesh = BuildKit.RoundMesh();
            var renderer = disc.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            if (!solid) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            disc.isStatic = true;
        }

        // The god's name and bars across the bottom of the screen.
        private static BossHud BossBar(Enemy boss)
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return null;
            var panel = new GameObject("Boss", typeof(RectTransform));
            panel.transform.SetParent(hud.transform, false);
            var area = (RectTransform)panel.transform;
            area.anchorMin = area.anchorMax = new Vector2(0.5f, 0f);
            area.pivot = new Vector2(0.5f, 0f);
            area.anchoredPosition = new Vector2(0f, 110f);
            area.sizeDelta = new Vector2(980f, 80f);

            var name = new GameObject("Name", typeof(RectTransform));
            name.transform.SetParent(panel.transform, false);
            var label = name.AddComponent<Text>();
            label.font = UiArt.Font();
            label.fontSize = 34;
            label.alignment = TextAnchor.LowerLeft;
            label.color = new Color(1f, 0.95f, 0.85f);
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.text = $"{boss.name},  {EnemyKind.Narukami().Title}";
            var outline = name.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
            var labelArea = (RectTransform)name.transform;
            labelArea.anchorMin = new Vector2(0f, 0f);
            labelArea.anchorMax = new Vector2(1f, 1f);
            labelArea.offsetMin = new Vector2(4f, 44f);
            labelArea.offsetMax = Vector2.zero;

            var health = Bar("Health", panel.transform, new Vector2(0f, 18f), new Vector2(980f, 26f), new Color(0.86f, 0.14f, 0.16f));
            var stagger = Bar("Balance", panel.transform, new Vector2(-4f, 0f), new Vector2(980f, 16f), new Color(1f, 0.8f, 0.25f));
            var bossHud = hud.AddComponent<BossHud>();
            bossHud.Configure(boss, panel, health, stagger, label);
            return bossHud;
        }

        private static RectTransform Bar(string name, Transform parent, Vector2 position, Vector2 size, Color colour)
        {
            var back = new GameObject(name + " Back", typeof(RectTransform));
            back.transform.SetParent(parent, false);
            var backImage = back.AddComponent<Image>();
            backImage.sprite = UiArt.BarBack();
            backImage.type = Image.Type.Sliced;
            backImage.raycastTarget = false;
            var backArea = (RectTransform)back.transform;
            backArea.anchorMin = backArea.anchorMax = backArea.pivot = new Vector2(0f, 0f);
            backArea.anchoredPosition = position;
            backArea.sizeDelta = size;
            var fill = new GameObject(name + " Fill", typeof(RectTransform));
            fill.transform.SetParent(back.transform, false);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = colour;
            fillImage.sprite = UiArt.BarFill();
            fillImage.type = Image.Type.Sliced;
            fillImage.raycastTarget = false;
            var fillArea = (RectTransform)fill.transform;
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = fillArea.offsetMax = Vector2.zero;
            return fillArea;
        }
    }
}
