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
    /// Builds the Proving Ground, the combat playtest's starting place (docs/GAME.md, "The evolving combat playtest").
    /// It stands in the same still black water as the hub and is made of the hub's stone: the patron's own ground,
    /// where everything taken from the gods is kept. A round plaza holds the rest fire with the eleven element altars
    /// round it. West, a terrace with the rack of every weapon, the gods' weapons and the empty stands of those still to
    /// come. East, a terrace of training dummies: one still, one that fights back, one carrying each element's
    /// affliction. North, two pens, the castle's in its stone and the Fire land's in its mud brick, with posts that let
    /// out one enemy or a group and clear the pen. Furthest north, under a broken ring, the gates to the two gods and the
    /// doors to the levels. Not in the editor's build list; the Combat Playtest build starts here.
    /// </summary>
    public static class ProvingGroundBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/ProvingGround.unity";
        private const string Folder = BuildKit.GeneratedRoot + "/ProvingGround";
        private const string HubFolder = BuildKit.GeneratedRoot + "/Hub";
        private const string CastleFolder = BuildKit.GeneratedRoot + "/Castle";

        private static Font font;
        private static Material chosen;
        private static List<Interactable> things;

        // The ground's parts, in metres: x across (east positive), z north.
        private const float PlazaRadius = 11f;
        private static readonly Rect West = Rect.MinMaxRect(-38f, -10f, -16f, 10f);
        private static readonly Rect East = Rect.MinMaxRect(16f, -10f, 38f, 10f);
        private static readonly Rect Pens = Rect.MinMaxRect(-25f, 18f, 25f, 38f);
        private static readonly Rect Landing = Rect.MinMaxRect(-24f, 44f, 24f, 55f);
        private static readonly Rect CastlePen = Rect.MinMaxRect(-24f, 22f, -4f, 37f);
        private static readonly Rect FirePen = Rect.MinMaxRect(4f, 22f, 24f, 37f);

        [MenuItem("Clover/Build/Proving Ground")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the Proving Ground")) return;
            // The words the guide and the chart show are read from docs/design/playtest first.
            PlaytestTextsBuilder.Build();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            HubBuilder.Materials();
            Dressing.Prepare();
            SunCityKit.BeginBuild();
            font = UiArt.Font();
            chosen = Load(HubFolder, "m_hub_chosen") ?? BuildKit.Glow(HubFolder, "m_hub_chosen", new Color(2.4f, 1.9f, 0.9f, 0.9f), 4, true, 2f, 0f);
            things = new List<Interactable>();

            var world = BuildKit.Group("World");
            Ground(world);
            Ring(world, new Vector3(0f, 5.2f, 63f), 9.5f);
            Drift(world);
            FrozenRain(world);

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, -6.5f), PlayerBuilder.PauseHint);
            if (built.Player == null) return;
            LookBuilder.Apply(LookBuilder.Stillness, built.Camera);
            var combat = built.Player.GetComponent<PlayerCombat>();
            var element = built.Player.GetComponent<PlayerElement>();
            combat.StartingWeapon = Weapons.FistsName;

            var start = BuildKit.Group("Start Point", world);
            start.position = new Vector3(0f, 0.05f, -6.5f);
            var director = new GameObject("Area Director").AddComponent<AreaDirector>();
            director.Configure(built.Player.GetComponent<PlayerController>(), new Enemy[0], start, null, null);

            RestFire(world, director);
            Altars(world, combat, element);
            Rack(world, combat, element);
            Dummies(world, built);
            BuildPens(world, built, director);
            Gates(world);
            Signs(world);
            Lamps(world);

            new GameObject("Proving Ground").AddComponent<ProvingGround>();
            built.Player.GetComponent<PlayerInteraction>().Configure(things.ToArray());
            PlayerBuilder.AddTitle("The Proving Ground", "where all that was taken is kept");
            SceneBuild.FinishOutsideBuild(scene, ScenePath);
        }

        private static Material Load(string folder, string name) => AssetDatabase.LoadAssetAtPath<Material>($"{folder}/{name}.mat");

        private static Vector3 Polar(float bearing, float radius, float height = 0f) => Quaternion.Euler(0f, bearing, 0f) * new Vector3(0f, height, radius);

        // ------------------------------------------------------------------ the ground

        private static void Ground(Transform world)
        {
            var group = BuildKit.Group("Ground", world);
            HubBuilder.Disc("Water", group, -0.5f, 170f, 0.2f, HubBuilder.Water, false, new Vector3(0f, 0f, 20f));
            // One unseen floor under everything that can be walked on; what is seen are the slabs laid over it.
            var floor = new GameObject("Walkable Floor");
            floor.transform.SetParent(group, false);
            floor.transform.position = new Vector3(0f, -0.5f, 22f);
            floor.AddComponent<BoxCollider>().size = new Vector3(80f, 1f, 76f);
            floor.isStatic = true;

            // The plaza: a round of dark stone with paving in it, an inlaid rim and a heart, as the hub's clock is laid.
            HubBuilder.Disc("Plaza", group, 0f, PlazaRadius, 1.6f, HubBuilder.Dark, false);
            HubBuilder.Disc("Plaza Paving", group, 0.01f, PlazaRadius - 0.6f, 0.02f, HubBuilder.Stone, false);
            HubBuilder.Disc("Plaza Rim Line", group, 0.02f, PlazaRadius - 2.0f, 0.02f, HubBuilder.Inlay, false);
            HubBuilder.Disc("Plaza Paving Inner", group, 0.03f, PlazaRadius - 2.1f, 0.02f, HubBuilder.Stone, false);
            HubBuilder.Disc("Plaza Heart Line", group, 0.04f, 2.6f, 0.02f, HubBuilder.Inlay, false);
            HubBuilder.Disc("Plaza Heart", group, 0.05f, 2.5f, 0.02f, HubBuilder.Dark, false);

            // The terraces, causeways and the landing: dark footings with paving laid in them, edged in pale stone.
            Slab(group, "West Terrace", West);
            Slab(group, "East Terrace", East);
            Slab(group, "Pen Ground", Pens);
            Slab(group, "Gate Landing", Landing);
            Slab(group, "West Causeway", Rect.MinMaxRect(-16.4f, -2.5f, -10.4f, 2.5f));
            Slab(group, "East Causeway", Rect.MinMaxRect(10.4f, -2.5f, 16.4f, 2.5f));
            Slab(group, "North Causeway", Rect.MinMaxRect(-3f, 10.4f, 3f, 18.4f));
            Slab(group, "Gate Causeway", Rect.MinMaxRect(-3f, 37.6f, 3f, 44.4f));
            Inlays(group);
            Edges(group);
        }

        private static void Slab(Transform parent, string name, Rect area)
        {
            var footing = BuildKit.Shape(PrimitiveType.Cube, name, parent, new Vector3(area.center.x, -0.8f, area.center.y), new Vector3(area.width + 0.5f, 1.6f, area.height + 0.5f), HubBuilder.Dark);
            footing.gameObject.isStatic = true;
            var paving = BuildKit.Shape(PrimitiveType.Cube, name + " Paving", parent, new Vector3(area.center.x, 0.0f, area.center.y), new Vector3(area.width - 0.3f, 0.02f, area.height - 0.3f), HubBuilder.Stone);
            paving.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            paving.gameObject.isStatic = true;
        }

        // The floor carries the place's lines: hour marks round the plaza, a border inlaid round each terrace, a line
        // down each causeway, and the rings of the altars, so no stretch of floor is one plain tone.
        private static void Inlays(Transform parent)
        {
            var gold = new Dressing.Mould { Lift = 0.03f };
            var cut = new Dressing.Mould { Lift = 0.028f };
            for (int hour = 0; hour < 12; hour++)
            {
                float bearing = hour * 30f;
                bool quarter = hour % 3 == 0;
                gold.Line(Polar(bearing, PlazaRadius - 1.6f), Polar(bearing, PlazaRadius - (quarter ? 0.5f : 0.9f)), quarter ? 0.3f : 0.14f);
            }
            cut.Arc(Vector3.zero, 4.6f, 4.65f, 0f, 360f);
            cut.Arc(Vector3.zero, 6.4f, 6.44f, 0f, 360f);
            for (int i = 0; i < 24; i++) cut.Line(Polar(i * 15f, 4.65f), Polar(i * 15f, i % 2 == 0 ? 4.95f : 4.8f), 0.035f);
            foreach (var area in new[] { West, East, Pens, Landing })
            {
                float inset = 0.9f;
                Vector3 a = new Vector3(area.xMin + inset, 0f, area.yMin + inset), b = new Vector3(area.xMax - inset, 0f, area.yMin + inset);
                Vector3 c = new Vector3(area.xMax - inset, 0f, area.yMax - inset), d = new Vector3(area.xMin + inset, 0f, area.yMax - inset);
                gold.Line(a, b, 0.06f);
                gold.Line(b, c, 0.06f);
                gold.Line(c, d, 0.06f);
                gold.Line(d, a, 0.06f);
                // A second, engraved line inside the gold, and a lozenge at each corner.
                foreach (var corner in new[] { a, b, c, d }) cut.Round(corner, 0.22f, 4, 1f, 45f);
            }
            gold.Line(new Vector3(-15.8f, 0f, 0f), new Vector3(-10.8f, 0f, 0f), 0.08f);
            gold.Line(new Vector3(10.8f, 0f, 0f), new Vector3(15.8f, 0f, 0f), 0.08f);
            gold.Line(new Vector3(0f, 0f, 10.8f), new Vector3(0f, 0f, 44f), 0.08f);
            // Engraved cracks with a little light in them, as on the clock: the break ran on out here.
            var seam = new Dressing.Mould { Lift = 0.026f };
            var seep = new Dressing.Mould { Lift = 0.029f };
            (Vector3 from, Vector3 to)[] cracks =
            {
                (new Vector3(-37f, 0f, -9f), new Vector3(-31f, 0f, -6.6f)), (new Vector3(34f, 0f, -8.5f), new Vector3(27f, 0f, 1f)),
                (new Vector3(-20f, 0f, 47f), new Vector3(-9f, 0f, 45f)), (new Vector3(7f, 0f, -9.8f), new Vector3(3f, 0f, -4f)),
            };
            for (int i = 0; i < cracks.Length; i++)
            {
                seam.Jag(cracks[i].from, cracks[i].to, 0.2f, 0.35f, 300 + i, 0.55f);
                seep.Jag(cracks[i].from, cracks[i].to, 0.05f, 0.35f, 300 + i, 0.55f);
            }
            gold.Cast("pg_inlay_gold", parent, HubBuilder.Inlay, false);
            cut.Cast("pg_inlay_cut", parent, Load(HubFolder, "m_hub_engraved") ?? HubBuilder.Dark, false);
            seam.Cast("pg_cracks", parent, Load(HubFolder, "m_hub_crack") ?? HubBuilder.Dark, false);
            seep.Cast("pg_crack_light", parent, Load(HubFolder, "m_hub_crack_light") ?? HubBuilder.Inlay, false);
        }

        // Unseen walls round every edge of the ground, with the ways between its parts left open; a low pale kerb shows each edge.
        private static void Edges(Transform parent)
        {
            var group = BuildKit.Group("Edges", parent);
            // The plaza's round edge, open to the west, east and north causeways.
            for (int i = 0; i < 36; i++)
            {
                float bearing = i * 10f + 5f;
                float fromNorth = Mathf.Abs(Mathf.DeltaAngle(bearing, 0f)), fromEast = Mathf.Abs(Mathf.DeltaAngle(bearing, 90f)), fromWest = Mathf.Abs(Mathf.DeltaAngle(bearing, 270f));
                if (fromNorth < 17f || fromEast < 15f || fromWest < 15f) continue;
                Wall(group, Polar(bearing - 5f, PlazaRadius - 0.2f), Polar(bearing + 5f, PlazaRadius - 0.2f));
            }
            RectWalls(group, West, east: new[] { (-2.5f, 2.5f) });
            RectWalls(group, East, west: new[] { (-2.5f, 2.5f) });
            RectWalls(group, Pens, south: new[] { (-3f, 3f) }, north: new[] { (-3f, 3f) });
            RectWalls(group, Landing, south: new[] { (-3f, 3f) });
            // The causeways' sides.
            foreach (float z in new[] { -2.5f, 2.5f })
            {
                Wall(group, new Vector3(-16f, 0f, z), new Vector3(-10.9f, 0f, z));
                Wall(group, new Vector3(10.9f, 0f, z), new Vector3(16f, 0f, z));
            }
            foreach (float x in new[] { -3f, 3f })
            {
                Wall(group, new Vector3(x, 0f, 10.7f), new Vector3(x, 0f, 18f));
                Wall(group, new Vector3(x, 0f, 38f), new Vector3(x, 0f, 44f));
            }
        }

        // An area's four sides, each solid but for the gaps given (from, to along the side).
        private static void RectWalls(Transform group, Rect area, (float, float)[] south = null, (float, float)[] north = null, (float, float)[] west = null, (float, float)[] east = null)
        {
            Side(group, new Vector3(area.xMin, 0f, area.yMin), new Vector3(area.xMax, 0f, area.yMin), south, true);
            Side(group, new Vector3(area.xMin, 0f, area.yMax), new Vector3(area.xMax, 0f, area.yMax), north, true);
            Side(group, new Vector3(area.xMin, 0f, area.yMin), new Vector3(area.xMin, 0f, area.yMax), west, false);
            Side(group, new Vector3(area.xMax, 0f, area.yMin), new Vector3(area.xMax, 0f, area.yMax), east, false);
        }

        private static void Side(Transform group, Vector3 from, Vector3 to, (float, float)[] gaps, bool alongX)
        {
            float start = alongX ? from.x : from.z, end = alongX ? to.x : to.z;
            var spans = new List<(float, float)> { (start, end) };
            if (gaps != null)
                foreach (var (gapFrom, gapTo) in gaps)
                {
                    var next = new List<(float, float)>();
                    foreach (var (a, b) in spans)
                    {
                        if (gapTo <= a || gapFrom >= b) { next.Add((a, b)); continue; }
                        if (gapFrom > a) next.Add((a, gapFrom));
                        if (gapTo < b) next.Add((gapTo, b));
                    }
                    spans = next;
                }
            foreach (var (a, b) in spans)
            {
                Vector3 p = alongX ? new Vector3(a, 0f, from.z) : new Vector3(from.x, 0f, a);
                Vector3 q = alongX ? new Vector3(b, 0f, from.z) : new Vector3(from.x, 0f, b);
                Wall(group, p, q);
                // A low pale kerb along the edge, so where the ground ends is seen.
                Vector3 mid = (p + q) * 0.5f;
                var kerb = BuildKit.Shape(PrimitiveType.Cube, "Kerb", group, mid + Vector3.up * 0.08f, alongX ? new Vector3(b - a, 0.16f, 0.32f) : new Vector3(0.32f, 0.16f, b - a), HubBuilder.Pale);
                kerb.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        private static void Wall(Transform group, Vector3 from, Vector3 to)
        {
            var wall = new GameObject("Edge");
            wall.transform.SetParent(group, false);
            Vector3 along = to - from;
            wall.transform.SetPositionAndRotation((from + to) * 0.5f + Vector3.up * 1.5f, Quaternion.LookRotation(along.sqrMagnitude > 0.0001f ? along.normalized : Vector3.forward, Vector3.up));
            wall.AddComponent<BoxCollider>().size = new Vector3(0.6f, 3f, along.magnitude + 0.3f);
            wall.isStatic = true;
        }

        // The landmark over the gates: a broken ring standing in the water, as over the hub's gate.
        private static void Ring(Transform world, Vector3 centre, float radius)
        {
            var group = BuildKit.Group("Great Ring", world);
            const int Segments = 44;
            for (int i = 0; i < Segments; i++)
            {
                float angle = i * 360f / Segments;
                if (angle > 200f && angle < 236f) continue;
                Vector3 offset = Quaternion.Euler(0f, 0f, -angle) * Vector3.up * radius;
                var piece = BuildKit.Shape(PrimitiveType.Cube, "Ring Stone", group, centre + offset, new Vector3(1.45f, 1.1f, 1.1f), i % 5 == 0 ? HubBuilder.Gold : HubBuilder.Dark, new Vector3(0f, 0f, -angle));
                piece.gameObject.isStatic = true;
                var inner = BuildKit.Shape(PrimitiveType.Cube, "Ring Light", group, centre + offset * ((radius - 0.6f) / radius) - Vector3.forward * 0.3f, new Vector3(1.3f, 0.12f, 0.2f), HubBuilder.RingLight, new Vector3(0f, 0f, -angle));
                inner.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var random = new System.Random(23);
            for (int i = 0; i < 8; i++)
            {
                float angle = 202f + (float)random.NextDouble() * 32f;
                Vector3 at = centre + Quaternion.Euler(0f, 0f, -angle) * Vector3.up * (radius + 0.6f + (float)random.NextDouble() * 3f);
                float size = 0.3f + (float)random.NextDouble() * 0.6f;
                BuildKit.Shape(PrimitiveType.Cube, "Ring Shard", group, at, new Vector3(size * 1.3f, size, size), i % 3 == 0 ? HubBuilder.Gold : HubBuilder.Dark,
                    new Vector3((float)random.NextDouble() * 90f, (float)random.NextDouble() * 90f, (float)random.NextDouble() * 90f));
            }
            var lightObject = new GameObject("Ring Light");
            lightObject.transform.SetParent(group, false);
            lightObject.transform.position = new Vector3(0f, 6f, 56f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.85f, 0.55f);
            light.range = 22f;
            light.intensity = 12f;
            light.shadows = LightShadows.None;
        }

        // Stones adrift round the ground, stopped in the air; those on the camera's side lie low.
        private static void Drift(Transform world)
        {
            var group = BuildKit.Group("Drift", world);
            var random = new System.Random(9);
            for (int i = 0; i < 70; i++)
            {
                Vector3 at = new Vector3(-62f + (float)random.NextDouble() * 124f, 0f, -30f + (float)random.NextDouble() * 110f);
                bool onGround = at.x > -40f && at.x < 40f && at.z > -14f && at.z < 59f;
                if (onGround) continue;
                bool south = at.z < -12f;
                at.y = south ? -0.3f + (float)random.NextDouble() * 0.5f : -0.2f + (float)random.NextDouble() * 6f;
                float size = 0.6f + (float)random.NextDouble() * 2.4f;
                var rock = BuildKit.Shape(PrimitiveType.Cube, "Drift Stone", group, at, new Vector3(size, size * (0.5f + (float)random.NextDouble() * 0.8f), size * (0.6f + (float)random.NextDouble() * 0.7f)),
                    i % 4 == 0 ? HubBuilder.Stone : HubBuilder.Dark, new Vector3((float)random.NextDouble() * 40f - 20f, (float)random.NextDouble() * 360f, (float)random.NextDouble() * 40f - 20f));
                rock.gameObject.isStatic = true;
            }
        }

        // Rain that does not fall, hanging over the whole ground as it hangs over the clock.
        private static void FrozenRain(Transform world)
        {
            var rainObject = new GameObject("Frozen Rain");
            rainObject.transform.SetParent(world, false);
            rainObject.transform.position = new Vector3(0f, 2.9f, 22f);
            var system = rainObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = false;
            main.playOnAwake = true;
            main.duration = 1f;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = 0.018f;
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
            main.startSizeZ = 0.018f;
            main.startRotation3D = true;
            main.startRotationZ = 8f * Mathf.Deg2Rad;
            main.gravityModifier = 0f;
            main.maxParticles = 2400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 2400) });
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(84f, 5.2f, 80f);
            var renderer = rainObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            renderer.alignment = ParticleSystemRenderSpace.World;
            renderer.sharedMaterial = Load(HubFolder, "m_hub_rain") ?? BuildKit.Glow(HubFolder, "m_hub_rain", new Color(0.8f, 0.9f, 1.3f, 0.55f), 3, true, 1f, 0f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // ------------------------------------------------------------------ the plaza

        private static void RestFire(Transform world, AreaDirector director)
        {
            var flame = Load(HubFolder, "m_hub_flame") ?? BuildKit.Flame(HubFolder, "m_hub_flame", new Color(1.5f, 0.45f, 0.09f, 1f));
            var root = HubBuilder.Brazier(world, "Rest Fire", new Vector3(0f, 0f, 0.6f), chosen, flame, new Color(1f, 0.6f, 0.28f), out Transform shown, 11f, 12f);
            // A rest fire is not something chosen: it has no ring of its own, and it waits cold until rested at.
            var ring = root.transform.Find("Chosen");
            if (ring != null) Object.DestroyImmediate(ring.gameObject);
            var flames = shown.GetComponentInChildren<ParticleSystem>();
            if (flames != null)
            {
                var main = flames.main;
                main.playOnAwake = false;
            }
            var returnPoint = BuildKit.Group("Return Point", root.transform);
            returnPoint.position = new Vector3(0f, 0.05f, -1.5f);
            var rest = root.AddComponent<RestPoint>();
            rest.Prompt = "Rest at the fire";
            rest.Range = 2.6f;
            rest.FireIntensity = 12f;
            rest.Configure(director, returnPoint, shown.GetComponentInChildren<Light>(), flames);
            things.Add(rest);
            Words(world, "REST FIRE", new Vector3(0f, 0.06f, -1.0f), 0.05f, new Color(1f, 0.82f, 0.5f));
        }

        // The eleven altars on the plaza's ring, in the ring's order from the south-west round to the south-east. Using one
        // carries its element: first with nothing carried, second with one carried (all in at the same altar again), and
        // alone again with two carried. The south, where the player arrives, is left open.
        private static readonly float[] AltarBearings = { -150f, -130f, -110f, -70f, -50f, -30f, 30f, 50f, 70f, 110f, 130f };

        private static void Altars(Transform world, PlayerCombat combat, PlayerElement element)
        {
            var group = BuildKit.Group("Element Altars", world);
            for (int i = 0; i < Elements.All.Count && i < AltarBearings.Length; i++)
            {
                var kind = Elements.All[i];
                string flameName = kind.Name == Elements.Fire ? "m_hub_flame" : kind.Name == Elements.Lightning ? "m_hub_flame_storm" : "m_hub_flame_" + kind.Name.ToLowerInvariant();
                var flame = Load(HubFolder, flameName) ?? BuildKit.Flame(HubFolder, flameName, new Color(kind.Colour.r * 1.7f, kind.Colour.g * 1.7f, kind.Colour.b * 1.7f, 1f));
                Vector3 at = Polar(AltarBearings[i], 8.3f);
                var bowl = HubBuilder.Brazier(group, kind.Name, at, chosen, flame, kind.Colour, out Transform shown, 4.5f, 5f, HubBuilder.ContentsOf(kind.Name));
                var choice = HubBuilder.Choice(bowl, true, kind.Name, true, shown.gameObject, $"Carry {kind.Name} (lays {Afflictions.Spec(kind.Affliction).Name})", "", "", combat, element);
                choice.Range = 1.8f;
                // Phase 2: an altar sets either attunement, and its prompt says which (AltarChoice).
                choice.SetsEitherAttunement = true;
                choice.AltarNote = "lays " + Afflictions.Spec(kind.Affliction).Name;
                things.Add(choice);
                // Its name on the floor on the side toward the middle, so it reads from the fire.
                Vector3 inward = -at.normalized;
                Words(group, kind.Name.ToUpperInvariant(), at + inward * 2.05f + Vector3.up * 0.06f, 0.034f, Color.Lerp(kind.Colour, Color.white, 0.3f));
            }
        }

        // ------------------------------------------------------------------ the rack

        private static void Rack(Transform world, PlayerCombat combat, PlayerElement element)
        {
            var group = BuildKit.Group("Weapon Rack", world);
            var offered = Weapons.Offered;
            // The front row: every weapon there is to be taken up.
            for (int i = 0; i < offered.Length; i++)
            {
                string name = offered[i];
                Vector3 at = new Vector3(West.center.x + (i - (offered.Length - 1) * 0.5f) * 2.05f, 0f, 4.4f);
                var stand = HubBuilder.Stand(group, "PG " + name, at, 180f, chosen, out Transform shown);
                WeaponBuilder.Display(Weapons.ByName(name).Carry, shown);
                var choice = HubBuilder.Choice(stand, false, name, true, shown.gameObject, name == Weapons.ArmamentName ? "Wear the Armament" : name == Weapons.FistsName ? "Fight with your hands" : "Take up the " + name, "", "", combat, element);
                choice.Range = 1.3f;
                things.Add(choice);
                Words(group, name.ToUpperInvariant(), at + new Vector3(0f, 0.06f, -1.25f), 0.032f, new Color(0.95f, 0.92f, 0.84f));
            }
            // The back row: the gods' weapons that are built, then an empty, shrouded stand for each still to come.
            int slot = 0;
            var godRow = new List<GodToCome>();
            foreach (var god in NotBuiltYet.Gods)
            {
                if (godRow.Exists(other => other.GodWeapon == god.GodWeapon)) continue;
                godRow.Add(god);
            }
            godRow.Sort((a, b) => b.WeaponBuilt.CompareTo(a.WeaponBuilt));
            var godLight = new Color(1f, 0.85f, 0.5f);
            foreach (var god in godRow)
            {
                Vector3 at = new Vector3(West.center.x + (slot - (godRow.Count - 1) * 0.5f) * 2.05f, 0f, -2.6f);
                slot++;
                if (god.WeaponBuilt)
                {
                    var stand = HubBuilder.Stand(group, "PG " + god.BuiltWeapon, at, 180f, chosen, out Transform shown);
                    WeaponBuilder.Display(Weapons.ByName(god.BuiltWeapon).Carry, shown);
                    var choice = HubBuilder.Choice(stand, false, god.BuiltWeapon, true, shown.gameObject, $"Take up the {god.BuiltWeapon} ({god.God.Split(',')[0]}'s)", "", "", combat, element);
                    choice.Range = 1.3f;
                    things.Add(choice);
                    // A god's weapon burns in its element's colour over its stand.
                    var glow = new GameObject("God Light").AddComponent<Light>();
                    glow.transform.SetParent(stand.transform, false);
                    glow.transform.localPosition = new Vector3(0f, 2.4f, 0f);
                    glow.type = LightType.Point;
                    glow.color = Elements.Colour(god.Element);
                    glow.range = 3.5f;
                    glow.intensity = 4f;
                    glow.shadows = LightShadows.None;
                    Words(group, god.BuiltWeapon.ToUpperInvariant() + "\n" + god.God.Split(',')[0].ToUpperInvariant() + "'S", at + new Vector3(0f, 0.06f, -1.3f), 0.03f, Color.Lerp(Elements.Colour(god.Element), godLight, 0.4f));
                    continue;
                }
                var empty = HubBuilder.Stand(group, "PG Not Built " + god.Element, at, 180f, chosen, out Transform nothing);
                var ring = empty.transform.Find("Chosen");
                if (ring != null) Object.DestroyImmediate(ring.gameObject);
                // A frame with a sheet over it, and nothing under the sheet yet.
                BuildKit.Shape(PrimitiveType.Cube, "Frame", nothing, new Vector3(0f, 0.95f, 0f), new Vector3(0.08f, 1.3f, 0.08f), HubBuilder.Wood);
                BuildKit.Shape(PrimitiveType.Cube, "Shroud", nothing, new Vector3(0f, 1.15f, 0.02f), new Vector3(0.62f, 0.9f, 0.05f), HubBuilder.Cloth, new Vector3(0f, 0f, 4f));
                BuildKit.Shape(PrimitiveType.Cube, "Shroud Fold", nothing, new Vector3(0.12f, 0.62f, 0.06f), new Vector3(0.5f, 0.26f, 0.05f), HubBuilder.Cloth, new Vector3(18f, 0f, -8f));
                var notice = empty.AddComponent<Notice>();
                notice.Lines = new[] { $"{NotBuiltYet.Label}: {god.GodWeapon}.", NotBuiltYet.SlotWords(god) };
                notice.Prompt = "Not built yet: read";
                notice.Range = 1.3f;
                things.Add(notice);
                Words(group, NotBuiltYet.Label.ToUpperInvariant() + "\n" + Wrapped(god.GodWeapon.Split('(')[0].Trim().ToUpperInvariant(), 13), at + new Vector3(0f, 0.06f, -1.45f), 0.026f, new Color(0.62f, 0.62f, 0.7f));
            }
            Words(group, "WEAPONS", new Vector3(West.center.x, 0.06f, 7.6f), 0.07f, new Color(1f, 0.82f, 0.5f));
            Words(group, "THE GODS' WEAPONS", new Vector3(West.center.x, 0.06f, 0.4f), 0.05f, new Color(1f, 0.82f, 0.5f));
        }

        // ------------------------------------------------------------------ the dummies

        private static void Dummies(Transform world, PlayerBuilder.Built built)
        {
            var group = BuildKit.Group("Dummies", world);
            var kit = new CombatTestArenaBuilder.DummyKit(built.Camera.transform.rotation);
            var still = CombatTestArenaBuilder.BuildDummy(new Vector3(21f, 0f, 4.5f), kit, null);
            still.transform.SetParent(group, true);
            Words(group, "STILL", new Vector3(21f, 0.06f, 3.1f), 0.045f, new Color(0.95f, 0.92f, 0.84f));
            var fights = CombatTestArenaBuilder.BuildDummy(new Vector3(30f, 0f, 4.5f), kit, built.Player.transform);
            fights.transform.SetParent(group, true);
            Words(group, "FIGHTS BACK\npale blows: guard  ·  red blows: roll", new Vector3(30f, 0.06f, 2.6f), 0.034f, new Color(1f, 0.7f, 0.6f));
            // One for each element, carrying its affliction always, in ring order along the south of the terrace.
            for (int i = 0; i < Elements.All.Count; i++)
            {
                var kind = Elements.All[i];
                Vector3 at = new Vector3(East.xMin + 2.2f + i * 1.75f, 0f, -5f);
                var dummy = CombatTestArenaBuilder.BuildDummy(at, kit, null);
                dummy.name = "Afflicted Dummy " + kind.Name;
                dummy.transform.SetParent(group, true);
                dummy.AddComponent<AfflictedDummy>().Kind = kind.Affliction;
                var body = BuildKit.FindDeep(dummy.transform, "Body");
                var band = BuildKit.Toon(Folder, "m_pg_band_" + kind.Name.ToLowerInvariant(), kind.Colour, false, null, 0f, 0.02f).Lit(Color.black, 0.3f, kind.Colour * 0.6f);
                BuildKit.Shape(PrimitiveType.Cylinder, "Element Band", body != null ? body : dummy.transform, new Vector3(0f, 1.2f, 0f), new Vector3(0.66f, 0.08f, 0.66f), band);
                Words(group, kind.Name.ToUpperInvariant() + "\n" + Afflictions.Spec(kind.Affliction).Name, at + new Vector3(0f, 0.06f, -1.25f), 0.026f, Color.Lerp(kind.Colour, Color.white, 0.3f));
            }
            Words(group, "TRAINING DUMMIES", new Vector3(East.center.x, 0.06f, 8.2f), 0.07f, new Color(1f, 0.82f, 0.5f));
            Words(group, "EACH CARRIES ITS ELEMENT'S AFFLICTION", new Vector3(East.center.x, 0.06f, -2.3f), 0.04f, new Color(1f, 0.82f, 0.5f));
        }

        // ------------------------------------------------------------------ the pens

        private static void BuildPens(Transform world, PlayerBuilder.Built built, AreaDirector director)
        {
            var group = BuildKit.Group("Pens", world);
            var kit = EnemyBuilder.NewKit(built.Player.transform, built.Camera.transform.rotation);
            var enemyRoot = BuildKit.Group("Penned Enemies");
            var all = new List<Enemy>();
            var defence = built.Player.GetComponent<PlayerDefence>();

            Enemy Pen(string kind, Rect pen, float x, float z)
            {
                var enemy = EnemyBuilder.Build(kind, new Vector3(x, 0.05f, z), 180f, kit, enemyRoot);
                // Kept to their pen: they notice nobody outside it and follow nobody out.
                enemy.HoldGround(pen.xMin + 0.5f, pen.xMax - 0.5f, pen.yMin + 0.2f, pen.yMax - 0.3f);
                all.Add(enemy);
                return enemy;
            }

            // The castle's pen: its stone, a banner, spears and a lantern; the storm's rod in a corner.
            CastlePenLook(group);
            var castle = new[]
            {
                Pen(EnemyKind.SoldierName, CastlePen, -19f, 29f),
                Pen(EnemyKind.AcolyteName, CastlePen, -14f, 29f),
                Pen(EnemyKind.CasterName, CastlePen, -9f, 32f),
                Pen(EnemyKind.CaptainName, CastlePen, -14f, 33.5f),
            };
            var castlePen = group.gameObject.AddComponent<EnemyPen>();
            castlePen.Configure("The castle", castle, defence, director);
            Posts(group, castlePen, CastlePen, new (string, int[])[]
            {
                (EnemyKind.SoldierName, new[] { 0 }), (EnemyKind.AcolyteName, new[] { 1 }), (EnemyKind.CasterName, new[] { 2 }),
                (EnemyKind.CaptainName + " (elite)", new[] { 3 }), ("The castle's three together", new[] { 0, 1, 2 }), ("", new int[0]),
            });
            Words(group, "THE CASTLE", new Vector3(CastlePen.center.x, 0.06f, 23.4f), 0.06f, new Color(0.6f, 0.75f, 1f));

            // The Fire land's pen: mud brick, a column pair, a brazier, the old god's head lying in the dust.
            var firePenRoot = BuildKit.Group("Fire Pen", group);
            FirePenLook(firePenRoot);
            var fire = new[]
            {
                Pen(EnemyKind.ThirstName, FirePen, 8f, 28f),
                Pen(EnemyKind.WickName, FirePen, 12f, 28f),
                Pen(EnemyKind.GildedLevyName, FirePen, 16f, 28f),
                Pen(EnemyKind.GildedSpearmanName, FirePen, 20f, 28f),
                Pen(EnemyKind.GildedWardenName, FirePen, 9f, 33f),
                Pen(EnemyKind.SunBlindName, FirePen, 14f, 33f),
                Pen(EnemyKind.AshenName, FirePen, 19f, 33f),
                Pen(EnemyKind.ChampionName, FirePen, 14f, 35f),
            };
            var firePen = firePenRoot.gameObject.AddComponent<EnemyPen>();
            firePen.Configure("The Fire land", fire, defence, director);
            Posts(firePenRoot, firePen, FirePen, new (string, int[])[]
            {
                (EnemyKind.ThirstName, new[] { 0 }), (EnemyKind.WickName, new[] { 1 }), (EnemyKind.GildedLevyName, new[] { 2 }),
                (EnemyKind.GildedSpearmanName, new[] { 3 }), (EnemyKind.GildedWardenName, new[] { 4 }), (EnemyKind.SunBlindName, new[] { 5 }),
                (EnemyKind.AshenName, new[] { 6 }), (EnemyKind.ChampionName + " (elite)", new[] { 7 }), ("The city's people together", new[] { 0, 1, 2, 6 }), ("", new int[0]),
            });
            Words(group, "THE FIRE LAND", new Vector3(FirePen.center.x, 0.06f, 23.4f), 0.06f, new Color(1f, 0.6f, 0.3f));

            kit.Turns.Configure(all.ToArray());
            // Each kind's own trick (the thirsting's snatch, the wick's grip, the gilded's gold, the priest's mirror,
            // the ashen's burst) and the burning floor their fire leaves.
            FireMechanicsKit.Equip(fire, kit);
            FireMechanicsKit.Floor(world, built.Player, all);
        }

        // A row of posts along a pen's open south side, one for each choice; the last, with nothing named, clears it.
        private static void Posts(Transform group, EnemyPen pen, Rect area, (string what, int[] which)[] choices)
        {
            float step = (area.width - 2f) / Mathf.Max(1, choices.Length - 1);
            var cap = HubBuilder.Gold;
            for (int i = 0; i < choices.Length; i++)
            {
                var (what, which) = choices[i];
                bool clears = which.Length == 0;
                Vector3 at = new Vector3(area.xMin + 1f + i * step, 0f, 20.2f);
                var post = new GameObject(clears ? "Pen Post Clear" : "Pen Post " + what);
                post.transform.SetParent(group, false);
                post.transform.position = at;
                BuildKit.Shape(PrimitiveType.Cube, "Post", post.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.28f, 1.1f, 0.28f), clears ? HubBuilder.Pale : HubBuilder.Dark);
                BuildKit.Shape(PrimitiveType.Cube, "Cap", post.transform, new Vector3(0f, 1.14f, 0f), new Vector3(0.38f, 0.08f, 0.38f), cap);
                var collider = post.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0f, 0.6f, 0f);
                collider.radius = 0.2f;
                collider.height = 1.2f;
                var use = post.AddComponent<PenPost>();
                use.Configure(pen, which, what);
                use.Range = 0.95f;
                things.Add(use);
                Words(group, clears ? "CLEAR" : Short(what), at + new Vector3(0f, 0.06f, -0.8f), 0.024f, clears ? new Color(0.8f, 0.8f, 0.86f) : new Color(0.95f, 0.92f, 0.84f));
            }
        }

        // Words broken into lines of about the given length, at spaces.
        private static string Wrapped(string words, int width)
        {
            var lines = new List<string>();
            string line = "";
            foreach (string word in words.Split(' '))
            {
                if (line.Length > 0 && line.Length + 1 + word.Length > width)
                {
                    lines.Add(line);
                    line = word;
                }
                else line = line.Length == 0 ? word : line + " " + word;
            }
            if (line.Length > 0) lines.Add(line);
            return string.Join("\n", lines);
        }

        // A post's label in two short lines.
        private static string Short(string what)
        {
            string upper = what.ToUpperInvariant().Replace("THE ", "");
            int space = upper.IndexOf(' ', upper.Length / 2 > 0 ? upper.Length / 2 - 2 : 0);
            return upper.Length > 11 && space > 0 ? upper.Substring(0, space) + "\n" + upper.Substring(space + 1) : upper;
        }

        private static void CastlePenLook(Transform group)
        {
            var paving = Load(CastleFolder, "m_castle_paving") ?? HubBuilder.Stone;
            var wall = Load(CastleFolder, "m_castle_wall") ?? HubBuilder.Dark;
            var trim = Load(CastleFolder, "m_castle_trim") ?? HubBuilder.Pale;
            var cloth = Load(CastleFolder, "m_castle_cloth") ?? HubBuilder.Cloth;
            var floor = BuildKit.Shape(PrimitiveType.Cube, "Castle Pen Floor", group, new Vector3(CastlePen.center.x, 0.02f, CastlePen.center.y), new Vector3(CastlePen.width, 0.02f, CastlePen.height), paving);
            floor.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cube, "Castle Pen Kerb", group, new Vector3(CastlePen.center.x, 0.06f, CastlePen.yMin), new Vector3(CastlePen.width, 0.1f, 0.3f), trim);
            BuildKit.Block("Castle Pen Wall North", group, new Vector3(CastlePen.center.x, 1.4f, CastlePen.yMax + 0.4f), new Vector3(CastlePen.width + 0.8f, 2.8f, 0.8f), wall);
            BuildKit.Block("Castle Pen Wall West", group, new Vector3(CastlePen.xMin - 0.4f, 1.4f, CastlePen.center.y), new Vector3(0.8f, 2.8f, CastlePen.height), wall);
            BuildKit.Block("Castle Pen Wall East", group, new Vector3(CastlePen.xMax + 0.4f, 1.1f, CastlePen.center.y), new Vector3(0.8f, 2.2f, CastlePen.height), wall);
            Dressing.HeavyBanner(group, new Vector3(-14f, 2.7f, CastlePen.yMax - 0.05f), 180f, 1.6f, 2.2f);
            Dressing.Spears(group, new Vector3(-22.6f, 0f, 35.6f), 5, 3, cloth);
            Dressing.Barrel(group, new Vector3(-22.8f, 0f, 24.2f), 30f);
            Dressing.Barrel(group, new Vector3(-21.9f, 0f, 23.6f), 80f, true);
            Dressing.Mantlet(group, new Vector3(-5.6f, 0f, 35.4f), 200f);
            Dressing.LightningRod(group, new Vector3(-23f, 0f, 36.6f), 4.2f);
            Dressing.Lantern(group, new Vector3(-5.2f, 0f, 23.2f), 0f);
        }

        private static void FirePenLook(Transform group)
        {
            var floor = BuildKit.Shape(PrimitiveType.Cube, "Fire Pen Floor", group, new Vector3(FirePen.center.x, 0.02f, FirePen.center.y), new Vector3(FirePen.width, 0.02f, FirePen.height), SunCityKit.Mud);
            floor.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            BuildKit.Shape(PrimitiveType.Cube, "Fire Pen Kerb", group, new Vector3(FirePen.center.x, 0.06f, FirePen.yMin), new Vector3(FirePen.width, 0.1f, 0.3f), SunCityKit.BrickDark);
            foreach (float x in new[] { 7f, 13f, 19f })
                SunCityKit.Place("wall_section", group, new Vector3(x, 0f, FirePen.yMax + 0.5f), 180f, 1f, false);
            BuildKit.Block("Fire Pen Wall North", group, new Vector3(FirePen.center.x, 1.4f, FirePen.yMax + 0.5f), new Vector3(FirePen.width + 0.8f, 2.8f, 0.8f), SunCityKit.Brick).GetComponent<Renderer>().enabled = false;
            BuildKit.Block("Fire Pen Wall East", group, new Vector3(FirePen.xMax + 0.4f, 1.4f, FirePen.center.y), new Vector3(0.8f, 2.8f, FirePen.height), SunCityKit.Brick);
            BuildKit.Block("Fire Pen Wall West", group, new Vector3(FirePen.xMin - 0.4f, 1.1f, FirePen.center.y), new Vector3(0.8f, 2.2f, FirePen.height), SunCityKit.BrickDark);
            var head = SunCityKit.Place("statue_head_old", group, new Vector3(21.6f, 0.3f, 34.6f), 150f, 0.7f);
            if (head != null) head.transform.rotation = Quaternion.Euler(-70f, 150f, 12f);
            SunCityKit.Brazier(group, new Vector3(5.4f, 0f, 35.6f));
        }

        // ------------------------------------------------------------------ the gates

        private static void Gates(Transform world)
        {
            var group = BuildKit.Group("Gates", world);
            Gate(group, new Vector3(-16f, 0f, 52.5f), "Castle", "THE CASTLE", "under the storm", "Go through, to the castle under the storm", new Color(0.22f, 0.4f, 0.95f));
            Gate(group, new Vector3(-6f, 0f, 52.5f), "Summit", "NARUKAMI", "the god of Lightning, on the summit", "Go up, to fight Narukami on the summit", new Color(0.45f, 0.75f, 1.6f));
            Gate(group, new Vector3(6f, 0f, 52.5f), "SunCourt", "KHEPHARU", "the god of Fire, in the sun-court", "Go in, to fight Khepharu in the sun-court", new Color(1.6f, 0.7f, 0.15f));
            Gate(group, new Vector3(16f, 0f, 52.5f), "SunCity", "SUN CITY", "the sun-king's city", "Go through, to the sun-king's city", new Color(1.1f, 0.5f, 0.2f));
            Words(group, "GODS", new Vector3(0f, 0.06f, 47.4f), 0.05f, new Color(1f, 0.82f, 0.5f));
            Words(group, "LEVELS", new Vector3(-16f, 0.06f, 47.4f), 0.04f, new Color(0.85f, 0.85f, 0.95f));
            Words(group, "LEVELS", new Vector3(16f, 0.06f, 47.4f), 0.04f, new Color(0.85f, 0.85f, 0.95f));
        }

        private static void Gate(Transform group, Vector3 at, string scene, string name, string under, string prompt, Color colour)
        {
            var root = new GameObject("Gate " + scene);
            root.transform.SetParent(group, false);
            root.transform.position = at;
            foreach (float side in new[] { -1f, 1f })
            {
                BuildKit.Block("Gate Post", root.transform, at + new Vector3(side * 2.1f, 2.4f, 0f), new Vector3(0.8f, 4.8f, 0.8f), HubBuilder.Dark);
                BuildKit.Block("Gate Foot", root.transform, at + new Vector3(side * 2.1f, 0.2f, 0f), new Vector3(1.2f, 0.4f, 1.2f), HubBuilder.Pale);
            }
            BuildKit.Shape(PrimitiveType.Cube, "Lintel", root.transform, new Vector3(0f, 5f, 0f), new Vector3(6.2f, 0.6f, 1f), HubBuilder.Dark);
            BuildKit.Shape(PrimitiveType.Cube, "Lintel Cap", root.transform, new Vector3(0f, 5.4f, 0f), new Vector3(7f, 0.22f, 1.2f), HubBuilder.Gold);
            var glow = BuildKit.Glow(Folder, "m_pg_gate_" + scene.ToLowerInvariant(), new Color(colour.r, colour.g, colour.b, 0.6f), 3, true, 1f, 0.5f);
            var sheet = BuildKit.Shape(PrimitiveType.Quad, "Beyond", root.transform, new Vector3(0f, 2.4f, 0.05f), new Vector3(3.4f, 4.6f, 1f), glow);
            sheet.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lightObject = new GameObject("Gate Light");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.4f, -1.2f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(Mathf.Min(1f, colour.r), Mathf.Min(1f, colour.g), Mathf.Min(1f, colour.b));
            light.range = 8f;
            light.intensity = 6f;
            light.shadows = LightShadows.None;
            HubBuilder.GateDetail(root.transform, at);
            var door = root.AddComponent<SceneDoor>();
            door.Prompt = prompt;
            door.LeadsTo = scene;
            door.Range = 2.6f;
            things.Add(door);
            Words(root.transform, name, at + new Vector3(0f, 0.06f, -3.1f), 0.05f, Color.Lerp(new Color(Mathf.Min(1f, colour.r), Mathf.Min(1f, colour.g), Mathf.Min(1f, colour.b)), Color.white, 0.35f));
            Words(root.transform, under, at + new Vector3(0f, 0.06f, -3.75f), 0.028f, new Color(0.85f, 0.85f, 0.9f));
        }

        // ------------------------------------------------------------------ signs and light

        private static void Signs(Transform world)
        {
            things.Add(HubBuilder.Sign(world, "Welcome Notice", new Vector3(3.6f, 0f, -8.6f), 180f,
                "This is the Proving Ground, where the playtest begins. Everything here is open: try anything.",
                "Rest at the fire to heal. Its menu sets the Hand (your four spells and the surge) and opens the chart. In the playtest a rest gives unlimited Shards, and the chart can be walked into every tree and branch.",
                "A fall puts you back at the last fire you rested at, with nothing lost.",
                "West: the weapons. Round the fire: the elements. East: the training dummies. North: the pens, and beyond them the gates to the gods and the doors to the levels.",
                "Use an altar to carry its element. With one carried, another altar carries its element as your second, and the same altar again carries it all in. With two carried, an altar starts again with its element alone. Each altar's prompt says which it will do.",
                "{Pause} pauses. The guide to effects is there, and from any level the way back here."));
            things.Add(HubBuilder.Sign(world, "Rack Notice", new Vector3(-17.6f, 0f, -8.4f), 180f,
                "Use a stand to take up its weapon. The front row is every weapon there is.",
                "The back row is the gods' weapons: the Storm Katana of Narukami and the Sun-Spear of Khepharu. The shrouded stands are gods' weapons not built yet."));
            things.Add(HubBuilder.Sign(world, "Dummy Notice", new Vector3(17.6f, 0f, -8.4f), 180f,
                "The still dummy takes blows and stands again. The one that fights back strikes when you come near: guard its pale blows, roll its red ones, and perfect-guard either to break its balance.",
                "Each of the row along the south always carries one element's affliction, so you can see it and try what your marks do to an enemy carrying it."));
            things.Add(HubBuilder.Sign(world, "Pen Notice", new Vector3(-1.9f, 0f, 19.2f), 180f,
                "Use a post to let out the enemy it names, or a group. The pale post at the end of the row clears the pen. A fall or a rest clears the pens too.",
                "Penned enemies give no Shards and keep to their pen."));
        }

        // Lamp pillars standing in the water at the corners of the terraces and the landing, so every part is lit.
        private static void Lamps(Transform world)
        {
            var group = BuildKit.Group("Lamps", world);
            var lamp = Load(HubFolder, "m_hub_lamp") ?? HubBuilder.Gold;
            foreach (var at in new[]
            {
                new Vector3(-38.6f, 0f, 10.6f), new Vector3(-38.6f, 0f, -10.6f), new Vector3(38.6f, 0f, 10.6f), new Vector3(38.6f, 0f, -10.6f),
                new Vector3(-25.6f, 0f, 38.6f), new Vector3(25.6f, 0f, 38.6f), new Vector3(-24.6f, 0f, 55.6f), new Vector3(24.6f, 0f, 55.6f),
            })
            {
                BuildKit.Shape(PrimitiveType.Cube, "Lamp Pillar", group, at + new Vector3(0f, 0.6f, 0f), new Vector3(0.7f, 2.4f, 0.7f), HubBuilder.Dark);
                BuildKit.Shape(PrimitiveType.Cube, "Lamp Cap", group, at + new Vector3(0f, 1.86f, 0f), new Vector3(0.6f, 0.09f, 0.6f), HubBuilder.Gold);
                var glass = BuildKit.Shape(PrimitiveType.Cube, "Lamp", group, at + new Vector3(0f, 2.15f, 0f), new Vector3(0.36f, 0.46f, 0.36f), lamp);
                glass.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            foreach (var (at, range, intensity) in new[]
            {
                (new Vector3(-27f, 6f, 0f), 22f, 9f), (new Vector3(27f, 6f, 0f), 22f, 9f), (new Vector3(-14f, 6f, 29f), 18f, 7f),
                (new Vector3(14f, 6f, 29f), 18f, 7f), (new Vector3(0f, 6f, 49f), 24f, 8f), (new Vector3(0f, 7f, 0f), 18f, 5f),
            })
            {
                var lightObject = new GameObject("Area Light");
                lightObject.transform.SetParent(group, false);
                lightObject.transform.position = at;
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.86f, 0.62f);
                light.range = range;
                light.intensity = intensity;
                light.shadows = LightShadows.None;
            }
        }

        // Words lying back toward the camera on the floor, in the game's own face.
        private static TextMesh Words(Transform parent, string words, Vector3 at, float size, Color colour)
        {
            var holder = new GameObject("Label " + words.Split('\n')[0]);
            holder.transform.SetParent(parent, true);
            holder.transform.SetPositionAndRotation(at, Quaternion.Euler(55f, 0f, 0f));
            var text = holder.AddComponent<TextMesh>();
            text.font = font;
            holder.GetComponent<MeshRenderer>().sharedMaterial = font != null ? font.material : null;
            holder.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            text.text = words;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 64;
            text.characterSize = size;
            text.color = colour;
            return text;
        }
    }
}
