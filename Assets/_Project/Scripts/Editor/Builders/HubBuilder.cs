using System.Collections.Generic;
using Clover.Player;
using Clover.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the hub: the place outside the hours where the sponsor keeps the player between runs. A round island of
    /// pale stone, laid out as a stopped clock, in black water under nothing. Rain hangs in the air where it was when
    /// time was turned back. The sponsor's light hangs over the middle; the weapons a run can begin with stand to the
    /// west, the elements to the east, and the gate to the castle to the north under a great broken ring. Nothing tall
    /// stands on the camera's side.
    /// </summary>
    public static class HubBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Hub.unity";
        private const string Folder = BuildKit.GeneratedRoot + "/Hub";
        private const float Radius = 10.5f;

        private static Material stone, dark, pale, gold, inlay, voice, wood, cloth, steel, ring, water;

        [MenuItem("Clover/Build/Hub")]
        public static void Build()
        {
            if (!SceneBuild.Begin("the hub")) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            stone = BuildKit.Toon(Folder, "m_hub_stone", new Color(0.33f, 0.33f, 0.4f), false, ProceduralTextures.Flagstones(), 0.125f, 0f);
            inlay = BuildKit.Toon(Folder, "m_hub_inlay", new Color(0.78f, 0.62f, 0.3f), false, null, 0f, 0f);
            voice = BuildKit.Toon(Folder, "m_hub_voice_body", new Color(1f, 0.86f, 0.55f), false, null, 0f, 0.02f).Lit(new Color(1.4f, 1.1f, 0.6f), 0.5f, new Color(1.1f, 0.8f, 0.35f));
            dark = BuildKit.Toon(Folder, "m_hub_dark", new Color(0.2f, 0.19f, 0.28f), false, ProceduralTextures.WallBlocks(), 0.2f, 0.02f);
            // The island's worked stone: paler than its floor, but under the figures that stand on it.
            pale = BuildKit.Toon(Folder, "m_hub_pale", new Color(0.4f, 0.395f, 0.49f), false, ProceduralTextures.DressedStone(), 0.5f, 0.02f);
            gold = BuildKit.Toon(Folder, "m_hub_gold", new Color(0.92f, 0.7f, 0.25f), false, null, 0f, 0.02f).Lit(new Color(1.2f, 0.9f, 0.4f), 0.4f, new Color(0.35f, 0.2f, 0.03f));
            wood = BuildKit.Toon(Folder, "m_hub_wood", new Color(0.3f, 0.2f, 0.16f), false, ProceduralTextures.Planks(), 0.5f, 0.02f);
            cloth = BuildKit.Toon(Folder, "m_hub_cloth", new Color(0.78f, 0.72f, 0.6f), false, null, 0f, 0.02f);
            steel = BuildKit.Toon(Folder, "m_hub_steel", new Color(0.75f, 0.8f, 0.9f), false, null, 0f, 0.02f).Lit(new Color(1f, 1f, 1.2f), 0.3f);
            ring = BuildKit.Toon(Folder, "m_hub_ring", new Color(1f, 0.9f, 0.6f), false, null, 0f, 0f).Lit(Color.black, 0.3f, new Color(2.6f, 2f, 1f));
            water = BuildKit.Toon(Folder, "m_hub_water", new Color(0.05f, 0.045f, 0.1f), false, null, 0f, 0f);
            var rain = BuildKit.Glow(Folder, "m_hub_rain", new Color(0.8f, 0.9f, 1.3f, 0.55f), 3, true, 1f, 0f);
            var light = BuildKit.Flame(Folder, "m_hub_flame_voice", new Color(1.2f, 0.9f, 0.45f, 0.9f));
            var fire = BuildKit.Flame(Folder, "m_hub_flame", new Color(1.5f, 0.45f, 0.09f, 1f));
            var spark = BuildKit.Flame(Folder, "m_hub_flame_storm", new Color(0.25f, 0.6f, 1.6f, 1f));
            var chosen = BuildKit.Glow(Folder, "m_hub_chosen", new Color(2.4f, 1.9f, 0.9f, 0.9f), 4, true, 2f, 0f);
            var gateGlow = BuildKit.Glow(Folder, "m_hub_gate", new Color(0.22f, 0.4f, 0.95f, 0.6f), 3, true, 1f, 0.5f);

            var world = BuildKit.Group("World");
            Floor(world);
            Ring(world);
            Drift(world);
            FrozenRain(world, rain);
            ClockDetail(world);
            StoppedThings(world);

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, -4.6f), PlayerBuilder.ControlsHint);
            if (built.Player == null) return;
            LookBuilder.Apply(LookBuilder.Stillness, built.Camera);
            // Nothing here can hurt: a player who comes back from a run badly wounded is not warned about it.
            PlayerBuilder.RemoveLowHealthWarning();
            var combat = built.Player.GetComponent<PlayerCombat>();
            var element = built.Player.GetComponent<PlayerElement>();
            combat.StartingWeapon = Weapons.FistsName;

            var things = new List<Interactable>();
            things.Add(Voice(world, Vector3.zero, light));

            var fists = Stand(world, "Bare Hands", new Vector3(-5.7f, 0f, -0.9f), 70f, chosen, out Transform fistsShown);
            Wraps(fistsShown);
            things.Add(Choice(fists, false, Weapons.FistsName, true, fistsShown.gameObject, "Fight with your hands", "", "", combat, element));
            var katana = Stand(world, "Katana", new Vector3(-4.1f, 0f, -5.2f), 40f, chosen, out Transform katanaShown);
            Sword(katanaShown);
            things.Add(Choice(katana, false, Weapons.KatanaName, false, katanaShown.gameObject, "Take up the Katana", "An empty stand", "Draw it from the stone first", combat, element));

            var fireBowl = Brazier(world, "Fire", new Vector3(5.7f, 0f, -0.9f), chosen, fire, new Color(1f, 0.55f, 0.2f), out Transform fireShown);
            things.Add(Choice(fireBowl, true, ElementKit.FireName, true, fireShown.gameObject, "Carry Fire", "", "", combat, element));
            var stormBowl = Brazier(world, "Lightning", new Vector3(4.1f, 0f, -5.2f), chosen, spark, new Color(0.45f, 0.7f, 1f), out Transform stormShown);
            things.Add(Choice(stormBowl, true, ElementKit.LightningName, false, stormShown.gameObject, "Carry Lightning", "A cold brazier", "Take it from its god first", combat, element));

            things.Add(Gate(world, new Vector3(0f, 0f, 8.4f), gateGlow));
            built.Player.GetComponent<PlayerInteraction>().Configure(things.ToArray());

            SceneBuild.Finish(scene, ScenePath, true);
        }

        // The island: a stopped clock. Twelve marks round the rim, two hands inlaid, and the middle left clear to stand in.
        private static void Floor(Transform world)
        {
            Disc("Water", world, -0.5f, 140f, 0.2f, water, false);
            Disc("Island", world, 0f, Radius, 1.6f, dark, true);
            Disc("Paving", world, 0.01f, Radius - 0.7f, 0.02f, stone, false);
            Disc("Rim Line", world, 0.02f, Radius - 2.4f, 0.02f, inlay, false);
            Disc("Paving Inner", world, 0.03f, Radius - 2.5f, 0.02f, stone, false);
            Disc("Heart Line", world, 0.04f, 2.2f, 0.02f, inlay, false);
            Disc("Heart", world, 0.05f, 2.1f, 0.02f, dark, false);
            Disc("Heart Seal", world, 0.06f, 0.95f, 0.02f, inlay, false);
            Disc("Heart Core", world, 0.07f, 0.85f, 0.02f, dark, false);
            for (int hour = 0; hour < 12; hour++)
            {
                float angle = hour * 30f;
                bool quarter = hour % 3 == 0;
                Vector3 at = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0.045f, Radius - 1.55f);
                var mark = BuildKit.Shape(PrimitiveType.Cube, "Hour", world, at, new Vector3(quarter ? 0.3f : 0.14f, 0.03f, quarter ? 1.3f : 0.8f), inlay, new Vector3(0f, angle, 0f));
                mark.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            // The hands, stopped a little before the hour.
            Hand(world, -24f, 5.4f, 0.16f);
            Hand(world, -96f, 3.6f, 0.24f);
            // An unseen wall round the edge.
            for (int i = 0; i < 30; i++)
            {
                float angle = i * 12f;
                var wall = new GameObject("Edge");
                wall.transform.SetParent(world, false);
                wall.transform.SetPositionAndRotation(Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 1.5f, Radius - 0.2f), Quaternion.Euler(0f, angle, 0f));
                wall.AddComponent<BoxCollider>().size = new Vector3(2.9f, 3f, 0.6f);
            }
        }

        private static void Hand(Transform world, float angle, float length, float width)
        {
            Vector3 along = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            var hand = BuildKit.Shape(PrimitiveType.Cube, "Hand", world, along * (2.3f + length * 0.5f) + Vector3.up * 0.045f, new Vector3(width, 0.03f, length), inlay, new Vector3(0f, angle, 0f));
            hand.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // The great ring behind the gate: a wheel of stone and gold standing on its edge, broken at the upper right.
        private static void Ring(Transform world)
        {
            var group = BuildKit.Group("Great Ring", world);
            Vector3 centre = new Vector3(0f, 4.4f, 13.5f);
            const int Segments = 40;
            const float RingRadius = 8.4f;
            for (int i = 0; i < Segments; i++)
            {
                float angle = i * 360f / Segments;
                // The break: a run of it is gone, and the ends either side of the gap have slipped.
                if (angle > 24f && angle < 62f) continue;
                float slip = angle >= 62f && angle < 80f ? 0.5f : angle <= 24f && angle > 8f ? -0.3f : 0f;
                Vector3 offset = Quaternion.Euler(0f, 0f, -angle) * Vector3.up * (RingRadius + slip);
                var piece = BuildKit.Shape(PrimitiveType.Cube, "Ring Stone", group, centre + offset, new Vector3(1.45f, 1.1f, 1.1f), i % 5 == 0 ? gold : dark, new Vector3(0f, 0f, -angle));
                piece.gameObject.isStatic = true;
                var inner = BuildKit.Shape(PrimitiveType.Cube, "Ring Light", group, centre + offset * ((RingRadius + slip - 0.6f) / (RingRadius + slip)) - Vector3.forward * 0.3f,
                    new Vector3(1.3f, 0.12f, 0.2f), ring, new Vector3(0f, 0f, -angle));
                inner.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            // Pieces of the break, hanging where they were when the hours stopped.
            var random = new System.Random(11);
            for (int i = 0; i < 9; i++)
            {
                float angle = 26f + (float)random.NextDouble() * 40f;
                float reach = RingRadius + 0.6f + (float)random.NextDouble() * 3.6f;
                Vector3 at = centre + Quaternion.Euler(0f, 0f, -angle) * Vector3.up * reach + Vector3.forward * ((float)random.NextDouble() * 2f - 1f);
                float size = 0.3f + (float)random.NextDouble() * 0.6f;
                BuildKit.Shape(PrimitiveType.Cube, "Ring Shard", group, at, new Vector3(size * 1.3f, size, size), i % 3 == 0 ? gold : dark,
                    new Vector3((float)random.NextDouble() * 90f, (float)random.NextDouble() * 90f, (float)random.NextDouble() * 90f));
            }
            // A soft light from the ring on the gate and the floor before it.
            var lightObject = new GameObject("Ring Light");
            lightObject.transform.SetParent(group, false);
            lightObject.transform.position = new Vector3(0f, 5f, 10f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.85f, 0.55f);
            light.range = 20f;
            light.intensity = 14f;
            light.shadows = LightShadows.None;
        }

        // Stones adrift round the island, stopped in the air. Those on the camera's side lie low.
        private static void Drift(Transform world)
        {
            var group = BuildKit.Group("Drift", world);
            var random = new System.Random(5);
            for (int i = 0; i < 46; i++)
            {
                float angle = (float)random.NextDouble() * 360f;
                float distance = Radius + 2.5f + (float)random.NextDouble() * 26f;
                bool south = Mathf.Abs(Mathf.DeltaAngle(angle, 180f)) < 75f;
                float height = south ? -0.3f + (float)random.NextDouble() * 0.5f : -0.2f + (float)random.NextDouble() * 6.5f;
                float size = 0.6f + (float)random.NextDouble() * 2.4f;
                Vector3 at = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, height, distance);
                var rock = BuildKit.Shape(PrimitiveType.Cube, "Drift Stone", group, at, new Vector3(size, size * (0.5f + (float)random.NextDouble() * 0.8f), size * (0.6f + (float)random.NextDouble() * 0.7f)),
                    i % 4 == 0 ? stone : dark, new Vector3((float)random.NextDouble() * 40f - 20f, (float)random.NextDouble() * 360f, (float)random.NextDouble() * 40f - 20f));
                rock.gameObject.isStatic = true;
            }
        }

        // Rain that does not fall: every drop a thin streak of light, hanging.
        private static void FrozenRain(Transform world, Material material)
        {
            var rainObject = new GameObject("Frozen Rain");
            rainObject.transform.SetParent(world, false);
            rainObject.transform.position = new Vector3(0f, 2.9f, 1f);
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
            main.startRotationX = 0f;
            main.startRotationY = 0f;
            main.startRotationZ = 8f * Mathf.Deg2Rad;
            main.gravityModifier = 0f;
            main.maxParticles = 700;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 700) });
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 5.2f, 36f);
            var renderer = rainObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            renderer.alignment = ParticleSystemRenderSpace.World;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // The sponsor: a spindle of light over the middle of the clock, with splinters turning slowly round it.
        private static Sponsor Voice(Transform world, Vector3 at, Material flame)
        {
            var root = new GameObject("The Voice");
            root.transform.SetParent(world, false);
            root.transform.position = at;
            var body = BuildKit.Group("Body", root.transform);
            body.localPosition = new Vector3(0f, 2.3f, 0f);
            var core = BuildKit.Shape(PrimitiveType.Cube, "Core", body, Vector3.zero, new Vector3(0.26f, 1.15f, 0.26f), voice, new Vector3(0f, 45f, 0f));
            core.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            core.gameObject.AddComponent<Spin>().DegreesPerSecond = 22f;
            var halo = BuildKit.Group("Halo", body);
            halo.gameObject.AddComponent<Spin>().DegreesPerSecond = -14f;
            for (int i = 0; i < 6; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, i * 60f, 0f) * new Vector3(0f, (i % 2 == 0 ? 0.35f : -0.3f), 1.05f);
                var shard = BuildKit.Shape(PrimitiveType.Cube, "Splinter", halo, offset, new Vector3(0.07f, 0.4f + (i % 3) * 0.12f, 0.07f), voice, new Vector3(12f, i * 60f, 20f));
                shard.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            Flame(body, new Vector3(0f, -0.3f, 0f), flame, 0.34f, 9f);

            var lightObject = new GameObject("Voice Light");
            lightObject.transform.SetParent(body, false);
            var glow = lightObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.88f, 0.6f);
            glow.range = 12f;
            glow.intensity = 9f;
            glow.shadows = LightShadows.None;

            // It casts no shadow of its own, but the floor under it is marked.

            var sponsor = root.AddComponent<Sponsor>();
            sponsor.Configure(body, glow);
            sponsor.Prompt = "Speak";
            sponsor.Range = 3.2f;
            return sponsor;
        }

        // A stand for a weapon: a stone foot, a post, and a ring of light on the floor when it is the one carried.
        private static GameObject Stand(Transform world, string name, Vector3 at, float yaw, Material chosenRing, out Transform shown)
        {
            var root = new GameObject(name + " Stand");
            root.transform.SetParent(world, false);
            root.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            BuildKit.Shape(PrimitiveType.Cylinder, "Foot", root.transform, new Vector3(0f, 0.12f, 0f), new Vector3(1.7f, 0.12f, 1.7f), dark);
            BuildKit.Shape(PrimitiveType.Cylinder, "Foot Top", root.transform, new Vector3(0f, 0.26f, 0f), new Vector3(1.4f, 0.03f, 1.4f), pale);
            root.AddComponent<CapsuleCollider>().radius = 0.7f;
            root.GetComponent<CapsuleCollider>().height = 2.4f;
            root.GetComponent<CapsuleCollider>().center = new Vector3(0f, 1.2f, 0f);
            shown = BuildKit.Group("Shown", root.transform);
            ChosenMark(root.transform, chosenRing, new Color(1f, 0.85f, 0.55f));
            Craft(root.transform, name + " stand", 1.7f, false);
            return root;
        }

        private static void ChosenMark(Transform root, Material ringMaterial, Color lightColour)
        {
            var mark = BuildKit.Group("Chosen", root);
            var quad = BuildKit.Shape(PrimitiveType.Quad, "Chosen Ring", mark, new Vector3(0f, 0.07f, 0f), new Vector3(3.4f, 3.4f, 1f), ringMaterial, new Vector3(90f, 0f, 0f));
            quad.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lightObject = new GameObject("Chosen Light");
            lightObject.transform.SetParent(mark, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightColour;
            light.range = 5.5f;
            light.intensity = 7f;
            light.shadows = LightShadows.None;
        }

        // Bare hands: a striking post bound with rope, and the wraps hung over it.
        private static void Wraps(Transform shown)
        {
            BuildKit.Shape(PrimitiveType.Cylinder, "Post", shown, new Vector3(0f, 1.05f, 0f), new Vector3(0.36f, 0.8f, 0.36f), wood);
            for (int i = 0; i < 4; i++)
                BuildKit.Shape(PrimitiveType.Cylinder, "Rope", shown, new Vector3(0f, 1.15f + i * 0.17f, 0f), new Vector3(0.43f, 0.055f, 0.43f), cloth);
            BuildKit.Shape(PrimitiveType.Cube, "Arm", shown, new Vector3(0f, 1.62f, 0f), new Vector3(1.1f, 0.12f, 0.14f), wood);
            BuildKit.Shape(PrimitiveType.Cube, "Wrap", shown, new Vector3(-0.42f, 1.3f, 0.02f), new Vector3(0.12f, 0.62f, 0.03f), cloth, new Vector3(0f, 0f, 4f));
            BuildKit.Shape(PrimitiveType.Cube, "Wrap", shown, new Vector3(0.42f, 1.24f, 0.02f), new Vector3(0.12f, 0.74f, 0.03f), cloth, new Vector3(0f, 0f, -5f));
        }

        // The Katana: laid across two forked uprights, edge up.
        private static void Sword(Transform shown)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                BuildKit.Shape(PrimitiveType.Cube, "Upright", shown, new Vector3(side * 0.5f, 0.78f, 0f), new Vector3(0.1f, 1.05f, 0.14f), wood);
                BuildKit.Shape(PrimitiveType.Cube, "Fork", shown, new Vector3(side * 0.5f, 1.34f, 0.07f), new Vector3(0.1f, 0.16f, 0.05f), wood);
                BuildKit.Shape(PrimitiveType.Cube, "Fork", shown, new Vector3(side * 0.5f, 1.34f, -0.07f), new Vector3(0.1f, 0.16f, 0.05f), wood);
            }
            BuildKit.Shape(PrimitiveType.Cube, "Rail", shown, new Vector3(0f, 0.5f, 0f), new Vector3(1.1f, 0.08f, 0.1f), wood);
            BuildKit.Shape(PrimitiveType.Cube, "Blade", shown, new Vector3(0.14f, 1.32f, 0f), new Vector3(1.05f, 0.05f, 0.016f), steel, new Vector3(0f, 0f, 3f));
            BuildKit.Shape(PrimitiveType.Cube, "Guard", shown, new Vector3(-0.4f, 1.3f, 0f), new Vector3(0.03f, 0.13f, 0.1f), gold);
            BuildKit.Shape(PrimitiveType.Cube, "Grip", shown, new Vector3(-0.58f, 1.29f, 0f), new Vector3(0.34f, 0.045f, 0.04f), dark, new Vector3(0f, 0f, 3f));
        }

        // A brazier for an element: lit with its fire once the element has been won.
        private static GameObject Brazier(Transform world, string name, Vector3 at, Material chosenRing, Material flame, Color lightColour, out Transform shown)
        {
            var root = new GameObject(name + " Brazier");
            root.transform.SetParent(world, false);
            root.transform.position = at;
            BuildKit.Shape(PrimitiveType.Cylinder, "Foot", root.transform, new Vector3(0f, 0.12f, 0f), new Vector3(1.5f, 0.12f, 1.5f), dark);
            BuildKit.Shape(PrimitiveType.Cylinder, "Leg", root.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.3f, 0.45f, 0.3f), dark);
            BuildKit.Shape(PrimitiveType.Cylinder, "Bowl", root.transform, new Vector3(0f, 1.12f, 0f), new Vector3(1.25f, 0.13f, 1.25f), gold);
            BuildKit.Shape(PrimitiveType.Cylinder, "Coals", root.transform, new Vector3(0f, 1.24f, 0f), new Vector3(1.05f, 0.03f, 1.05f), dark);
            var collider = root.AddComponent<CapsuleCollider>();
            collider.radius = 0.6f;
            collider.height = 2.2f;
            collider.center = new Vector3(0f, 1.1f, 0f);
            shown = BuildKit.Group("Shown", root.transform);
            Flame(shown, new Vector3(0f, 1.3f, 0f), flame, 0.7f, 11f);
            var lightObject = new GameObject("Brazier Light");
            lightObject.transform.SetParent(shown, false);
            lightObject.transform.localPosition = new Vector3(0f, 2f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightColour;
            light.range = 8f;
            light.intensity = 8f;
            light.shadows = LightShadows.None;
            ChosenMark(root.transform, chosenRing, lightColour);
            Craft(root.transform, name + " brazier", 1.5f, true);
            return root;
        }

        private static LoadoutChoice Choice(GameObject root, bool isElement, string choice, bool alwaysOpen, GameObject shown, string open, string sealedPrompt, string sealedWords,
            PlayerCombat combat, PlayerElement element)
        {
            var made = root.AddComponent<LoadoutChoice>();
            made.Configure(isElement, choice, alwaysOpen, shown, BuildKit.FindDeep(root.transform, "Chosen").gameObject);
            made.Watch(combat, element);
            made.OpenPrompt = open;
            made.SealedPrompt = sealedPrompt;
            made.SealedWords = sealedWords;
            made.Prompt = open;
            made.Range = 2.5f;
            return made;
        }

        // The gate: two posts and a lintel under the ring, and between them the storm, seen as through a door left ajar.
        private static SceneDoor Gate(Transform world, Vector3 at, Material glow)
        {
            var root = new GameObject("Gate");
            root.transform.SetParent(world, false);
            root.transform.position = at;
            foreach (float side in new[] { -1f, 1f })
            {
                BuildKit.Block("Gate Post", root.transform, at + new Vector3(side * 2.1f, 2.4f, 0f), new Vector3(0.8f, 4.8f, 0.8f), dark);
                BuildKit.Block("Gate Foot", root.transform, at + new Vector3(side * 2.1f, 0.2f, 0f), new Vector3(1.2f, 0.4f, 1.2f), pale);
            }
            BuildKit.Shape(PrimitiveType.Cube, "Lintel", root.transform, new Vector3(0f, 5f, 0f), new Vector3(6.2f, 0.6f, 1f), dark);
            BuildKit.Shape(PrimitiveType.Cube, "Lintel Cap", root.transform, new Vector3(0f, 5.4f, 0f), new Vector3(7f, 0.22f, 1.2f), gold);
            var sheet = BuildKit.Shape(PrimitiveType.Quad, "Beyond", root.transform, new Vector3(0f, 2.4f, 0.05f), new Vector3(3.4f, 4.6f, 1f), glow);
            sheet.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lightObject = new GameObject("Gate Light");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.4f, -1.2f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 0.7f, 1f);
            light.range = 9f;
            light.intensity = 7f;
            light.shadows = LightShadows.None;
            // The landing before the gate: a worked slab on a darker footing a little wider than itself, which draws its edge.
            Disc("Gate Step Footing", root.transform, 0.035f, 2.42f, 0.04f, dark, false, new Vector3(at.x, 0f, at.z - 1.6f));
            Disc("Gate Step", root.transform, 0.07f, 2.3f, 0.06f, pale, false, new Vector3(at.x, 0f, at.z - 1.6f));
            Disc("Gate Step Line", root.transform, 0.074f, 2.02f, 0.01f, dark, false, new Vector3(at.x, 0f, at.z - 1.6f));
            Disc("Gate Step Middle", root.transform, 0.078f, 1.96f, 0.01f, pale, false, new Vector3(at.x, 0f, at.z - 1.6f));

            GateDetail(root.transform, at);

            var door = root.AddComponent<SceneDoor>();
            door.Prompt = "Go through, to the castle under the storm";
            door.LeadsTo = "Castle";
            door.Range = 3f;
            return door;
        }

        // ------------------------------------------------------------------ the clock's fine work, and what stopped with it

        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static float Next(System.Random random, float low, float high) => low + (float)random.NextDouble() * (high - low);

        // A point on the round: an angle clockwise from north, a distance out, and optionally a step to the right of that line.
        private static Vector3 Polar(float angle, float radius, float height = 0f) => Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, height, radius);
        private static Vector3 Polar(float angle, float radius, float across, float height) => Quaternion.Euler(0f, angle, 0f) * new Vector3(across, height, radius);

        // How high the island's own layers stand at a distance from its middle.
        private static float FloorTop(float radius) => radius < 0.85f ? 0.07f : radius < 0.95f ? 0.06f : radius < 2.1f ? 0.05f : radius < 2.2f ? 0.04f
            : radius < Radius - 2.5f ? 0.03f : radius < Radius - 2.4f ? 0.02f : radius < Radius - 0.7f ? 0.01f : 0f;

        private static Dressing.Mould Layer(float lift) => new Dressing.Mould { Top = FloorTop, Lift = lift };
        private static Material Flat(string name, Color colour) => BuildKit.Toon(Folder, name, colour, false, null, 0f, 0f);

        /// <summary>
        /// The fine work of the clock face: minute marks, numerals, inner rings and engraved lines, two small dials; the
        /// ring worn into the stone by someone walking round the middle; cracks with a little light in them; and a film of
        /// still water at the rim, its ripples stopped as rings. All of it is flat and none of it is solid.
        /// </summary>
        private static void ClockDetail(Transform world)
        {
            var group = BuildKit.Group("Clock Detail", world);
            var random = new System.Random(77);
            var cut = Layer(0.002f);
            var worn = Layer(0.003f);
            var fine = Layer(0.004f);
            var split = Layer(0.006f);
            var seep = Layer(0.008f);
            var film = Layer(0.012f);
            var filmEdge = Layer(0.014f);
            var ripple = Layer(0.016f);
            var drops = new Dressing.Mould();

            for (int minute = 0; minute < 60; minute++)
                if (minute % 5 != 0) fine.Line(Polar(minute * 6f, 9.28f), Polar(minute * 6f, 9.52f), 0.05f);
            fine.Arc(Vector3.zero, 6.3f, 6.36f, 0f, 360f);
            fine.Arc(Vector3.zero, 4.2f, 4.25f, 0f, 360f);
            for (int i = 0; i < 24; i++) fine.Line(Polar(i * 15f, 4.25f), Polar(i * 15f, i % 2 == 0 ? 4.46f : 4.36f), 0.035f);
            cut.Arc(Vector3.zero, 2.55f, 2.585f, 0f, 360f);
            cut.Arc(Vector3.zero, 6.5f, 6.535f, 0f, 360f);
            cut.Arc(Vector3.zero, 9.66f, 9.7f, 0f, 360f);
            for (int i = 0; i < 12; i++)
            {
                float between = 15f + i * 30f;
                cut.Line(Polar(between, 2.7f), Polar(between, 4.1f), 0.03f);
                cut.Line(Polar(between, 6.62f), Polar(between, 7.9f), 0.03f);
            }
            string[] numerals = { "XII", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI" };
            for (int hour = 0; hour < 12; hour++) Numeral(fine, numerals[hour], hour * 30f, 6.92f, 0.5f);
            SmallDial(fine, cut, Polar(160f, 5.3f), 0.75f, 40f, 3);
            SmallDial(fine, cut, Polar(200f, 5.3f), 0.75f, 250f, 5);

            // Round and round the middle, for longer than there are hours to count it in.
            for (int i = 0; i < 10; i++)
            {
                float start = i * 36f + Next(random, -4f, 4f);
                worn.Arc(Vector3.zero, 2.95f + Next(random, 0f, 0.14f), 3.52f - Next(random, 0f, 0.14f), start, start + Next(random, 24f, 33f));
            }

            // The break that took the ring ran on across the face. There is light down in it.
            (float a0, float r0, float a1, float r1)[] cracks = { (40f, 9.75f, 72f, 2.7f), (250f, 9.75f, 228f, 5.2f), (150f, 9.75f, 172f, 6.7f), (318f, 9.75f, 334f, 7f) };
            for (int i = 0; i < cracks.Length; i++)
            {
                Vector3 from = Polar(cracks[i].a0, cracks[i].r0), to = Polar(cracks[i].a1, cracks[i].r1);
                float width = i == 0 ? 0.3f : 0.18f;
                var path = split.Jag(from, to, width, 0.35f, 900 + i, 0.55f);
                seep.Jag(from, to, width * 0.2f, 0.35f, 900 + i, 0.55f);
                Vector3 fork = path[path.Count / 2];
                Vector3 end = fork + Quaternion.Euler(0f, Next(random, 0f, 360f), 0f) * Vector3.forward * Next(random, 1.2f, 2f);
                if (new Vector2(end.x, end.z).magnitude > 9.7f || new Vector2(end.x, end.z).magnitude < 2.4f) continue;
                split.Jag(fork, end, width * 0.45f, 0.2f, 950 + i, 0.4f);
                seep.Jag(fork, end, width * 0.09f, 0.2f, 950 + i, 0.4f);
            }

            // Water lying on the rim, a finger deep, and never to dry.
            (float from, float to)[] lobes = { (116f, 176f), (190f, 254f), (282f, 334f), (20f, 68f), (78f, 108f) };
            for (int lobe = 0; lobe < lobes.Length; lobe++)
            {
                float Inner(float angle)
                {
                    float edge = Mathf.Clamp01(Mathf.Min(angle - lobes[lobe].from, lobes[lobe].to - angle) / 13f);
                    return Mathf.Lerp(10.44f, 9.25f + 0.42f * Mathf.Sin(angle * 0.23f + lobe * 1.7f), Mathf.SmoothStep(0f, 1f, edge));
                }
                for (float angle = lobes[lobe].from; angle < lobes[lobe].to - 0.01f; angle += 2f)
                {
                    float next = Mathf.Min(angle + 2f, lobes[lobe].to);
                    film.Patch(Polar(angle, Inner(angle)), Polar(angle, 10.46f), Polar(next, 10.46f), Polar(next, Inner(next)));
                    filmEdge.Patch(Polar(angle, Inner(angle) - 0.05f), Polar(angle, Inner(angle)), Polar(next, Inner(next)), Polar(next, Inner(next) - 0.05f));
                }
                // The sky that is not there, in it: a few short pale strokes.
                for (float angle = lobes[lobe].from + 12f; angle < lobes[lobe].to - 12f; angle += Next(random, 7f, 12f))
                {
                    float radius = Next(random, 9.85f, 10.3f);
                    ripple.Line(Polar(angle, radius), Polar(angle + Next(random, 1.2f, 2.6f), radius + Next(random, -0.04f, 0.04f)), 0.03f, 0.01f);
                }
                int rings = lobes[lobe].to - lobes[lobe].from > 40f ? 3 : 1;
                for (int i = 0; i < rings; i++)
                {
                    Vector3 centre = Polar(Mathf.Lerp(lobes[lobe].from + 14f, lobes[lobe].to - 14f, (i + Next(random, 0.2f, 0.8f)) / rings), 10.06f);
                    for (int ring = 0; ring < 3; ring++)
                    {
                        float radius = 0.1f + ring * 0.11f, start = Next(random, 0f, 360f);
                        ripple.Arc(centre, radius - 0.012f, radius + 0.012f, start, start + (ring == 0 ? 360f : Next(random, 200f, 320f)), 18f);
                    }
                    // The drop that made them, a hand above the water, and the one after it.
                    drops.Ball(centre + Vector3.up * Next(random, 0.2f, 0.4f), V(0.07f, 0.1f, 0.07f), null, 6, 4);
                    drops.Ball(centre + Vector3.up * Next(random, 0.8f, 1.3f), V(0.06f, 0.09f, 0.06f), null, 6, 4);
                }
            }

            var dim = Flat("m_hub_inlay_dim", new Color(0.56f, 0.46f, 0.26f));
            cut.Cast("hub_clock_engraved", group, Flat("m_hub_engraved", new Color(0.17f, 0.16f, 0.25f)), false);
            // Worn, not laid: the same paving, its joints still running through it, only paler.
            worn.Cast("hub_clock_worn", group, BuildKit.Toon(Folder, "m_hub_worn", new Color(0.41f, 0.41f, 0.5f), false, ProceduralTextures.Flagstones(), 0.125f, 0f), false);
            fine.Cast("hub_clock_fine", group, dim, false);
            split.Cast("hub_clock_cracks", group, Flat("m_hub_crack", new Color(0.05f, 0.045f, 0.1f)), false);
            seep.Cast("hub_clock_crack_light", group, Flat("m_hub_crack_light", new Color(0.26f, 0.4f, 0.56f)).Lit(Color.black, 0.3f, new Color(0.03f, 0.1f, 0.18f)), false);
            // A finger of water does not hide the stone under it: the same paving, darker and bluer.
            var sheet = film.Cast("hub_water_film", group, BuildKit.Toon(Folder, "m_hub_shallows", new Color(0.15f, 0.18f, 0.36f), false, ProceduralTextures.Flagstones(), 0.125f, 0f), false);
            if (sheet != null) sheet.receiveShadows = false;
            filmEdge.Cast("hub_water_film_edge", group, Flat("m_hub_film_edge", new Color(0.25f, 0.27f, 0.43f)), false);
            ripple.Cast("hub_water_ripples", group, Flat("m_hub_ripple", new Color(0.44f, 0.52f, 0.78f)), false);
            drops.Cast("hub_water_drops", group, Flat("m_hub_drop", new Color(0.55f, 0.66f, 0.95f)).Lit(Color.black, 0.3f, new Color(0.12f, 0.18f, 0.4f)), false);
        }

        // A Roman numeral let into the face, its feet toward the middle as on any clock.
        private static void Numeral(Dressing.Mould layer, string numeral, float angle, float radius, float height)
        {
            const float Gap = 0.07f, Stroke = 0.05f;
            float total = -Gap;
            foreach (char letter in numeral) total += (letter == 'I' ? 0.06f : 0.26f) + Gap;
            Vector3 At(float across, float up) => Polar(angle, radius + up, across, 0f);
            float cursor = -total * 0.5f;
            foreach (char letter in numeral)
            {
                if (letter == 'I')
                {
                    layer.Line(At(cursor + 0.03f, 0f), At(cursor + 0.03f, height), Stroke);
                    cursor += 0.06f + Gap;
                }
                else if (letter == 'V')
                {
                    layer.Line(At(cursor, height), At(cursor + 0.13f, 0f), Stroke);
                    layer.Line(At(cursor + 0.26f, height), At(cursor + 0.13f, 0f), Stroke);
                    cursor += 0.26f + Gap;
                }
                else
                {
                    layer.Line(At(cursor, 0f), At(cursor + 0.26f, height), Stroke);
                    layer.Line(At(cursor, height), At(cursor + 0.26f, 0f), Stroke);
                    cursor += 0.26f + Gap;
                }
            }
            layer.Line(At(-total * 0.5f - 0.05f, -0.01f), At(total * 0.5f + 0.05f, -0.01f), 0.03f);
            layer.Line(At(-total * 0.5f - 0.05f, height + 0.01f), At(total * 0.5f + 0.05f, height + 0.01f), 0.03f);
        }

        // A small dial set into the face, for something other than hours.
        private static void SmallDial(Dressing.Mould fine, Dressing.Mould cut, Vector3 centre, float radius, float hand, int marks)
        {
            fine.Arc(centre, radius - 0.045f, radius, 0f, 360f, 9f);
            cut.Arc(centre, radius * 0.56f, radius * 0.56f + 0.025f, 0f, 360f, 12f);
            for (int i = 0; i < 12; i++)
            {
                Vector3 out_ = Quaternion.Euler(0f, i * 30f, 0f) * Vector3.forward;
                fine.Line(centre + out_ * (radius - 0.17f), centre + out_ * (radius - 0.07f), 0.03f);
            }
            for (int i = 0; i < marks; i++)
            {
                Vector3 out_ = Quaternion.Euler(0f, i * 360f / marks + 20f, 0f) * Vector3.forward;
                cut.Round(centre + out_ * (radius * 0.36f), 0.045f + (i % 2) * 0.02f, 8);
            }
            fine.Line(centre, centre + Quaternion.Euler(0f, hand, 0f) * Vector3.forward * (radius * 0.52f), 0.06f, 0.012f);
            fine.Round(centre, 0.07f, 8);
        }

        /// <summary>
        /// What was in the air or the water when the hours stopped, and is there still: gears and a great bell from the
        /// clock half sunk, a pendulum hanging from nothing at the top of its swing, a flight of steps that climbs away
        /// from the island and comes apart, paper lanterns adrift at different heights, a crane between two wingbeats,
        /// drops and leaves. Each has its dim shape lying on the black water under it. All of it is beyond the rim, and
        /// what is tall is to the north or the sides.
        /// </summary>
        private static void StoppedThings(Transform world)
        {
            var group = BuildKit.Group("Stopped Things", world);
            var random = new System.Random(19);
            var bronze = new Dressing.Mould();
            var bronzeDark = new Dressing.Mould();
            var step = new Dressing.Mould();
            var stepDark = new Dressing.Mould();
            var lamp = new Dressing.Mould();
            var lampCap = new Dressing.Mould();
            var white = new Dressing.Mould();
            var black = new Dressing.Mould();
            var red = new Dressing.Mould();
            var leaf = new Dressing.Mould();
            var drops = new Dressing.Mould();
            var reflect = new Dressing.Mould { Lift = -0.49f };
            var reflectWarm = new Dressing.Mould { Lift = -0.485f };
            var all = new[] { bronze, bronzeDark, step, stepDark, lamp, lampCap, white, black, red, leaf, drops };
            void SetAt(Vector3 at, Quaternion turn, float scale = 1f)
            {
                foreach (var mould in all) mould.Place(at, turn, scale);
            }
            // The dim shape a thing leaves on the water: toward the camera from under it, by how high it hangs.
            Vector3 Under(Vector3 at) => V(at.x, 0f, at.z - 0.7f * Mathf.Max(0f, at.y + 0.5f));

            void Gear(Vector3 centre, Quaternion turn, float radius, float thick, int teeth)
            {
                SetAt(centre, turn);
                bronze.Drum(Vector3.zero, radius * 1.74f, thick, null, 22);
                for (int tooth = 0; tooth < teeth; tooth++)
                {
                    Quaternion q = Quaternion.Euler(0f, tooth * 360f / teeth, 0f);
                    bronze.Box(q * V(0f, 0f, radius * 0.93f), V(radius * 2.7f / teeth, thick, radius * 0.2f), q);
                }
                bronzeDark.Drum(Vector3.zero, radius * 0.5f, thick * 1.5f, null, 12);
                bronze.Drum(Vector3.zero, radius * 0.2f, thick * 2.4f, null, 8);
                // Where the wheel is cut away between its spokes, drawn as darker rounds standing just proud of it.
                for (int hole = 0; hole < 5; hole++)
                    bronzeDark.Drum(Quaternion.Euler(0f, hole * 72f + 36f, 0f) * V(0f, 0f, radius * 0.56f), radius * 0.27f, thick * 1.08f, null, 8);
                reflect.Round(Under(centre), radius * 1.05f, 14, 0.5f, 0f);
            }
            Gear(Polar(52f, 13.2f, 0.1f), Quaternion.Euler(-52f, 30f, 0f), 2.2f, 0.34f, 16);
            Gear(Polar(-104f, 12.7f, -0.44f), Quaternion.Euler(12f, 0f, 8f), 1.4f, 0.26f, 12);
            Gear(Polar(117f, 12f, -0.25f), Quaternion.Euler(-58f, -50f, 0f), 0.9f, 0.2f, 10);
            Gear(Polar(-36f, 15f, 0.2f), Quaternion.Euler(-42f, -20f, 10f), 1.7f, 0.3f, 14);
            Gear(Polar(101f, 13.2f, 1.9f), Quaternion.Euler(-66f, 60f, 20f), 0.6f, 0.14f, 10);

            // The bell, half under, mouth toward the island.
            {
                Vector3 at = Polar(-47f, 13.4f, -0.6f);
                Quaternion turn = Quaternion.Euler(0f, 24f, 0f) * Quaternion.Euler(0f, 0f, -60f);
                SetAt(at, turn, 1.25f);
                bronze.Lathe(Vector3.zero, null, 16, 1.62f, 0f, 1.4f, 0.3f, 1.12f, 0.95f, 0.95f, 1.6f, 0.8f, 2.1f, 0.44f, 2.42f, 0f, 2.48f);
                bronzeDark.Lathe(Vector3.zero, null, 16, 1.66f, 0f, 1.66f, 0.1f, 1.52f, 0.2f);
                bronzeDark.Lathe(Vector3.zero, null, 16, 1.17f, 0.9f, 1.13f, 1.02f);
                bronzeDark.Lathe(Vector3.zero, null, 16, 0.84f, 2.02f, 0.8f, 2.14f);
                bronzeDark.Drum(V(0f, 2.5f, 0f), 0.34f, 0.2f, null, 8);
                bronzeDark.Drum(V(0f, 2.82f, 0f), 0.62f, 0.14f, Quaternion.Euler(90f, 0f, 0f), 12);
                // Cracked from the lip up, as a bell is that was rung too hard once.
                bronzeDark.Beam(V(0.2f, 0.02f, -1.5f), V(0.05f, 0.7f, -1.3f), 0.07f, 0.05f);
                bronzeDark.Beam(V(0.05f, 0.7f, -1.3f), V(0.22f, 1.25f, -1.1f), 0.05f, 0.05f);
                bronzeDark.Drum(V(0f, 0.04f, 0f), 2.9f, 0.05f, null, 16);
                reflect.Round(V(at.x + 1.2f, 0f, at.z - 1.3f), 2.2f, 14, 0.45f, 24f);
            }

            // The pendulum: hung from nothing now, and stopped at the top of its swing.
            {
                Vector3 pivot = Polar(27f, 13.8f, 5.8f);
                SetAt(pivot, Quaternion.Euler(0f, 0f, 34f));
                bronzeDark.Box(V(0f, 0.1f, 0f), V(0.5f, 0.2f, 0.3f));
                bronzeDark.Pole(V(0f, 0f, 0f), V(0f, -4.4f, 0f), 0.09f, 6);
                bronze.Drum(V(0f, -4.9f, 0f), 1.5f, 0.2f, Quaternion.Euler(90f, 0f, 0f), 18);
                bronzeDark.Drum(V(0f, -4.9f, 0f), 0.9f, 0.26f, Quaternion.Euler(90f, 0f, 0f), 14);
                bronze.Drum(V(0f, -4.9f, 0f), 0.3f, 0.34f, Quaternion.Euler(90f, 0f, 0f), 8);
                Vector3 bob = pivot + Quaternion.Euler(0f, 0f, 34f) * V(0f, -4.9f, 0f);
                reflect.Round(Under(bob), 0.8f, 12, 0.5f, 0f);
            }

            // Steps that lead up from the west of the island to nowhere, the last of them no longer joined to anything.
            {
                const float Bearing = -78f;
                Vector3 foot = Polar(Bearing, Radius, 0f);
                float along = 0.3f, rise = 0.02f;
                for (int i = 0; i < 8; i++)
                {
                    bool loose = i >= 3;
                    Quaternion tilt = loose ? Quaternion.Euler(Next(random, -7f, 7f), Next(random, -9f, 9f), Next(random, -6f, 6f)) : Quaternion.identity;
                    SetAt(foot, Quaternion.Euler(0f, Bearing, 0f));
                    step.Box(V(loose ? Next(random, -0.15f, 0.15f) : 0f, rise - 0.11f, along), V(1.7f - i * 0.04f, 0.22f, 0.6f), tilt);
                    stepDark.Box(V(0f, rise - 0.3f - (loose ? 0.02f : 0.45f), along + 0.02f), V(1.45f - i * 0.04f, loose ? 0.2f : 1.1f, 0.5f), tilt, loose ? 0.6f : 1f);
                    if (i == 7 || i == 4) reflect.Round(Under(foot + Quaternion.Euler(0f, Bearing, 0f) * V(0f, rise, along)), 0.8f, 10, 0.5f, Bearing);
                    along += 0.62f + (loose ? (i - 2) * 0.16f : 0f);
                    rise += 0.3f + (loose ? 0.05f : 0f);
                }
            }

            // Paper lanterns, let go once, at whatever height they had reached.
            (float angle, float radius, float height)[] lanterns =
            {
                (-20f, 12.3f, 2.6f), (14f, 12.3f, 1.5f), (57f, 11.7f, 3.1f), (-52f, 11.5f, 1f), (88f, 12.2f, 0.9f), (-93f, 12.5f, 1.9f),
                (8f, 15.2f, 3.8f), (112f, 11.6f, 0.45f), (-113f, 11.7f, 0.55f), (-38f, 17f, 4.4f), (44f, 16.5f, 4.8f),
            };
            for (int i = 0; i < lanterns.Length; i++)
            {
                Vector3 at = Polar(lanterns[i].angle, lanterns[i].radius, lanterns[i].height);
                SetAt(at, Quaternion.Euler(Next(random, -9f, 9f), Next(random, 0f, 360f), Next(random, -9f, 9f)));
                // A paper lantern: a round belly of lit paper, swelling in the middle, in a dark frame of ribs and hoops,
                // under a peaked cap with a loop to carry it by, and a red tassel hanging from its foot.
                lamp.Drum(V(0f, 0.1f, 0f), 0.34f, 0.2f, null, 8, 0.8f);
                lamp.Drum(V(0f, -0.1f, 0f), 0.27f, 0.2f, null, 8, 1.26f);
                for (int rib = 0; rib < 4; rib++)
                {
                    Quaternion round = Quaternion.Euler(0f, rib * 90f + 22.5f, 0f);
                    lampCap.Beam(round * V(0.125f, -0.2f, 0f), round * V(0.172f, 0f, 0f), 0.022f, 0.022f);
                    lampCap.Beam(round * V(0.172f, 0f, 0f), round * V(0.128f, 0.2f, 0f), 0.022f, 0.022f);
                }
                lampCap.Drum(V(0f, 0f, 0f), 0.365f, 0.022f, null, 8);
                lampCap.Drum(V(0f, 0.235f, 0f), 0.42f, 0.09f, null, 8, 0.45f);
                lampCap.Drum(V(0f, 0.21f, 0f), 0.3f, 0.03f, null, 8);
                lampCap.Drum(V(0f, -0.22f, 0f), 0.26f, 0.05f, null, 8, 0.8f);
                lampCap.Beam(V(-0.05f, 0.28f, 0f), V(-0.05f, 0.38f, 0f), 0.018f, 0.018f);
                lampCap.Beam(V(0.05f, 0.28f, 0f), V(0.05f, 0.38f, 0f), 0.018f, 0.018f);
                lampCap.Beam(V(-0.06f, 0.38f, 0f), V(0.06f, 0.38f, 0f), 0.018f, 0.018f);
                red.Beam(V(0f, -0.24f, 0f), V(0f, -0.4f, 0f), 0.018f, 0.018f);
                red.Drum(V(0f, -0.42f, 0f), 0.06f, 0.045f, null, 6);
                red.Drum(V(0f, -0.53f, 0f), 0.085f, 0.19f, null, 6, 0.55f);
                reflectWarm.Round(Under(at), 0.34f, 10, 0.55f, 0f);
                if (i > 2) continue;
                var lightObject = new GameObject("Lantern Glow");
                lightObject.transform.SetParent(group, false);
                lightObject.transform.position = at;
                var glow = lightObject.AddComponent<Light>();
                glow.type = LightType.Point;
                glow.color = new Color(1f, 0.66f, 0.32f);
                glow.range = 5f;
                glow.intensity = 4f;
                glow.shadows = LightShadows.None;
            }

            // A crane, crossing from the west toward the ring, wings spread on a glide and banked a little.
            {
                Vector3 at = Polar(-64f, 11.9f, 1.3f);
                SetAt(at, Quaternion.Euler(-4f, 64f, -13f), 0.92f);
                void Feather(Dressing.Mould mould, Vector3 a, Vector3 b2, Vector3 c, Vector3 d)
                {
                    mould.Quad(a, b2, c, d, Vector3.up);
                    mould.Quad(a, b2, c, d, Vector3.down);
                }
                white.Ball(Vector3.zero, V(0.26f, 0.22f, 0.66f), null, 8, 5);
                white.Pole(V(0f, 0.02f, 0.24f), V(0f, 0.06f, 0.78f), 0.07f, 6);
                black.Pole(V(0f, 0.035f, 0.42f), V(0f, 0.055f, 0.7f), 0.076f, 6);
                white.Ball(V(0f, 0.07f, 0.82f), V(0.13f, 0.12f, 0.18f), null, 6, 4);
                red.Box(V(0f, 0.135f, 0.82f), V(0.07f, 0.03f, 0.09f));
                black.Box(V(0f, 0.06f, 1.02f), V(0.035f, 0.24f, 0.035f), Quaternion.Euler(90f, 0f, 0f), 0f);
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector3 root0 = V(side * 0.08f, 0.07f, 0.2f), root1 = V(side * 0.08f, 0.07f, -0.2f);
                    Vector3 wrist0 = V(side * 0.78f, 0.2f, 0.27f), wrist1 = V(side * 0.72f, 0.18f, -0.2f);
                    Vector3 hand0 = V(side * 1.42f, 0.13f, 0.06f), hand1 = V(side * 1.28f, 0.11f, -0.3f);
                    Vector3 tip = V(side * 1.78f, 0.06f, -0.2f);
                    Feather(white, root0, wrist0, wrist1, root1);
                    Feather(white, wrist0, hand0, hand1, wrist1);
                    Feather(black, hand0, tip, tip + V(side * -0.28f, 0f, -0.24f), hand1);
                    Feather(black, root1, wrist1, wrist1 + V(0f, -0.01f, -0.12f), root1 + V(0f, -0.01f, -0.1f));
                    Feather(black, wrist1, hand1, hand1 + V(0f, 0f, -0.1f), wrist1 + V(0f, -0.01f, -0.12f));
                    black.Pole(V(side * 0.04f, -0.05f, -0.3f), V(side * 0.05f, -0.06f, -0.98f), 0.024f, 4);
                }
                black.Box(V(0f, 0.01f, -0.42f), V(0.2f, 0.025f, 0.26f));
                Vector3 under = Under(at);
                reflect.Round(under, 0.4f, 10, 0.4f, 64f);
                reflect.Round(under, 1.5f, 10, 0.14f, 64f + 90f);
            }

            // Drops that have not landed, and leaves from no tree that stands here.
            for (int i = 0; i < 30; i++)
            {
                SetAt(Polar(Next(random, 0f, 360f), Next(random, 8.9f, 11.8f), Next(random, 0.15f, 1.7f)), Quaternion.identity);
                float size = Next(random, 0.05f, 0.1f);
                drops.Ball(Vector3.zero, V(size, size * 1.5f, size), null, 6, 4);
            }
            for (int i = 0; i < 16; i++)
            {
                SetAt(Polar(Next(random, 0f, 360f), Next(random, 9.2f, 12.6f), Next(random, 0.3f, 2.2f)), Quaternion.Euler(Next(random, -60f, 60f), Next(random, 0f, 360f), Next(random, -60f, 60f)));
                (i % 3 == 0 ? red : leaf).Box(Vector3.zero, V(0.2f, 0.012f, 0.1f), Quaternion.Euler(0f, 45f, 0f), 0.4f);
            }

            // The great ring, on the water before the gate, as a few dull strokes.
            reflectWarm.Arc(V(0f, 0f, 14.6f), 3.1f, 3.3f, 148f, 172f, 4f);
            reflectWarm.Arc(V(0f, 0f, 14.6f), 3.1f, 3.3f, 188f, 214f, 4f);
            reflectWarm.Arc(V(0f, 0f, 14.6f), 3.7f, 3.82f, 160f, 200f, 4f);

            bronze.Cast("hub_stopped_bronze", group, BuildKit.Toon(Folder, "m_hub_bronze", new Color(0.33f, 0.29f, 0.27f), false, null, 0f, 0.025f));
            bronzeDark.Cast("hub_stopped_bronze_dark", group, BuildKit.Toon(Folder, "m_hub_bronze_dark", new Color(0.22f, 0.19f, 0.2f), false, null, 0f, 0.02f));
            step.Cast("hub_stopped_steps", group, BuildKit.Toon(Folder, "m_hub_step", new Color(0.3f, 0.3f, 0.39f), false, ProceduralTextures.DressedStone(), 0.5f, 0.03f));
            stepDark.Cast("hub_stopped_steps_dark", group, BuildKit.Toon(Folder, "m_hub_step_dark", new Color(0.2f, 0.19f, 0.29f), false, null, 0f, 0.02f));
            // Lit paper, not a bulb: warm, but dim enough that the dark frame round it is still seen against it.
            lamp.Cast("hub_stopped_lamps", group, Flat("m_hub_lamp", new Color(0.95f, 0.6f, 0.3f)).Lit(Color.black, 0.3f, new Color(0.85f, 0.4f, 0.12f)), false);
            lampCap.Cast("hub_stopped_lamp_caps", group, BuildKit.Toon(Folder, "m_hub_lamp_cap", new Color(0.15f, 0.13f, 0.21f), false, null, 0f, 0.012f), false);
            white.Cast("hub_stopped_crane", group, BuildKit.Toon(Folder, "m_hub_crane", new Color(0.72f, 0.72f, 0.8f), false, null, 0f, 0.02f), false);
            black.Cast("hub_stopped_crane_dark", group, BuildKit.Toon(Folder, "m_hub_crane_dark", new Color(0.1f, 0.09f, 0.16f), false, null, 0f, 0.012f), false);
            red.Cast("hub_stopped_red", group, Flat("m_hub_red", new Color(0.6f, 0.16f, 0.18f)), false);
            leaf.Cast("hub_stopped_leaves", group, Flat("m_hub_leaf", new Color(0.6f, 0.44f, 0.2f)), false);
            drops.Cast("hub_stopped_drops", group, Flat("m_hub_drop", new Color(0.55f, 0.66f, 0.95f)).Lit(Color.black, 0.3f, new Color(0.12f, 0.18f, 0.4f)), false);
            foreach (var made in new[] { reflect.Cast("hub_water_shapes", group, Flat("m_hub_reflection", new Color(0.1f, 0.095f, 0.19f)), false),
                reflectWarm.Cast("hub_water_shapes_warm", group, Flat("m_hub_reflection_warm", new Color(0.3f, 0.2f, 0.12f)).Lit(Color.black, 0.3f, new Color(0.1f, 0.05f, 0.015f)), false) })
                if (made != null) made.receiveShadows = false;
        }

        // The mason's and the smith's work on a stand or a brazier: bands and studs on the foot, a ring and four marks let
        // into the floor round it. Nothing here changes where the thing stands or how big it is.
        private static void Craft(Transform root, string name, float footWidth, bool brazier)
        {
            string key = name.ToLower().Replace(' ', '_');
            var goldWork = new Dressing.Mould();
            var paleWork = new Dressing.Mould();
            var darkWork = new Dressing.Mould();
            var floor = new Dressing.Mould { Lift = 0.036f };
            var engraved = new Dressing.Mould { Lift = 0.293f };
            float half = footWidth * 0.5f;
            goldWork.Drum(V(0f, 0.2f, 0f), footWidth + 0.05f, 0.035f, null, 22);
            goldWork.Drum(V(0f, 0.045f, 0f), footWidth + 0.06f, 0.03f, null, 22);
            for (int i = 0; i < 8; i++)
            {
                Quaternion q = Quaternion.Euler(0f, i * 45f + 22.5f, 0f);
                paleWork.Box(q * V(0f, 0.125f, half + 0.005f), V(0.14f, 0.08f, 0.05f), q);
            }
            floor.Arc(Vector3.zero, half + 0.2f, half + 0.25f, 0f, 360f, 8f);
            for (int i = 0; i < 4; i++)
            {
                Vector3 out_ = Quaternion.Euler(0f, i * 90f, 0f) * Vector3.forward;
                floor.Line(out_ * (half + 0.31f), out_ * (half + 0.58f), 0.07f, 0.02f);
            }
            if (!brazier)
            {
                engraved.Arc(Vector3.zero, 0.52f, 0.55f, 0f, 360f, 10f);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 out_ = Quaternion.Euler(0f, i * 45f, 0f) * Vector3.forward;
                    engraved.Line(out_ * 0.58f, out_ * 0.66f, 0.03f);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Quaternion q = Quaternion.Euler(0f, i * 120f + 60f, 0f);
                    darkWork.Beam(q * V(0f, 0.24f, 0.62f), q * V(0f, 1.02f, 0.4f), 0.07f, 0.07f);
                    goldWork.Box(q * V(0f, 0.27f, 0.62f), V(0.16f, 0.07f, 0.16f), q);
                }
                goldWork.Drum(V(0f, 0.62f, 0f), 0.4f, 0.06f, null, 10);
                goldWork.Drum(V(0f, 0.36f, 0f), 0.44f, 0.05f, null, 10);
                darkWork.Drum(V(0f, 0.95f, 0f), 0.56f, 0.1f, null, 12, 1.5f);
                for (int i = 0; i < 8; i++)
                {
                    Quaternion q = Quaternion.Euler(0f, i * 45f, 0f);
                    darkWork.Box(q * V(0f, 1.12f, 0.63f), V(0.1f, 0.14f, 0.04f), q);
                }
            }
            goldWork.Cast($"hub_{key}_gold", root, gold, false);
            paleWork.Cast($"hub_{key}_pale", root, pale, false);
            darkWork.Cast($"hub_{key}_dark", root, Flat("m_hub_iron", new Color(0.16f, 0.15f, 0.24f)));
            floor.Cast($"hub_{key}_floor", root, Flat("m_hub_inlay_dim", new Color(0.56f, 0.46f, 0.26f)), false);
            engraved.Cast($"hub_{key}_engraved", root, Flat("m_hub_engraved", new Color(0.17f, 0.16f, 0.25f)), false);
        }

        // What makes the gate a gate and not two posts: bands and carved panels on the posts, a tie beam hung with
        // paper slips, a wider landing under the step, and two lamp pillars standing in the water either side.
        private static void GateDetail(Transform root, Vector3 at)
        {
            var random = new System.Random(41);
            var darkWork = new Dressing.Mould();
            var paleWork = new Dressing.Mould();
            var goldWork = new Dressing.Mould();
            var panel = new Dressing.Mould();
            var marks = new Dressing.Mould();
            var slips = new Dressing.Mould();
            var lampWork = new Dressing.Mould();
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * 2.1f;
                goldWork.Box(V(x, 0.95f, 0f), V(0.9f, 0.14f, 0.9f));
                goldWork.Box(V(x, 3.85f, 0f), V(0.9f, 0.14f, 0.9f));
                paleWork.Box(V(x, 4.66f, 0f), V(1.02f, 0.16f, 1.02f));
                panel.Box(V(x, 2.4f, -0.405f), V(0.46f, 2.5f, 0.03f));
                marks.Box(V(x, 2.4f, -0.425f), V(0.04f, 2.2f, 0.015f));
                for (int mark = 0; mark < 5; mark++)
                    marks.Box(V(x + (mark % 2 == 0 ? -0.06f : 0.06f), 1.5f + mark * 0.45f, -0.425f), V(0.26f, 0.055f, 0.015f), Quaternion.Euler(0f, 0f, mark % 2 == 0 ? 20f : -20f));
                darkWork.Box(V(x - side * 0.72f, 4.52f, 0f), V(0.64f, 0.34f, 0.56f), Quaternion.Euler(0f, 0f, side * -12f));
                // A lamp pillar, rising out of the water beside the gate and a little behind it.
                float px = side * 4.1f, pz = 2.1f;
                darkWork.Box(V(px, -0.4f, pz), V(1.2f, 1.7f, 1.2f), null, 0.75f);
                paleWork.Box(V(px, 0.5f, pz), V(1f, 0.12f, 1f));
                darkWork.Box(V(px, 1.35f, pz), V(0.62f, 1.6f, 0.62f), null, 0.75f);
                goldWork.Box(V(px, 2.19f, pz), V(0.6f, 0.09f, 0.6f));
                lampWork.Box(V(px, 2.47f, pz), V(0.36f, 0.46f, 0.36f));
                foreach (float cx in new[] { -0.19f, 0.19f })
                    foreach (float cz in new[] { -0.19f, 0.19f })
                        darkWork.Box(V(px + cx, 2.47f, pz + cz), V(0.06f, 0.48f, 0.06f));
                darkWork.Box(V(px, 2.8f, pz), V(0.74f, 0.2f, 0.74f), null, 0.4f);
            }
            darkWork.Box(V(0f, 4.1f, 0f), V(5.4f, 0.32f, 0.5f));
            goldWork.Box(V(0f, 4.1f, -0.26f), V(0.46f, 0.42f, 0.04f));
            marks.Box(V(0f, 4.1f, -0.285f), V(0.26f, 0.05f, 0.015f), Quaternion.Euler(0f, 0f, 45f));
            marks.Box(V(0f, 4.1f, -0.285f), V(0.26f, 0.05f, 0.015f), Quaternion.Euler(0f, 0f, -45f));
            for (int i = 0; i < 7; i++)
            {
                float x = -1.5f + i * 0.5f, drop = 0.2f + (i * 37 % 5) * 0.07f;
                slips.Box(V(x, 3.94f - drop * 0.5f, -0.2f), V(0.015f, drop, 0.015f));
                // Each was lifting in a wind when the wind stopped.
                slips.Box(V(x, 3.94f - drop - 0.22f, -0.2f), V(0.15f, 0.46f, 0.015f), Quaternion.Euler(Next(random, -22f, 6f), Next(random, -25f, 25f), Next(random, -10f, 10f)));
            }
            darkWork.Cast("hub_gate_dark", root, dark);
            paleWork.Cast("hub_gate_pale", root, pale);
            goldWork.Cast("hub_gate_gold", root, gold, false);
            panel.Cast("hub_gate_panels", root, Flat("m_hub_engraved", new Color(0.17f, 0.16f, 0.25f)), false);
            marks.Cast("hub_gate_marks", root, Flat("m_hub_inlay_dim", new Color(0.56f, 0.46f, 0.26f)), false);
            slips.Cast("hub_gate_slips", root, cloth, false);
            lampWork.Cast("hub_gate_lamps", root, Flat("m_hub_lamp", new Color(1f, 0.74f, 0.42f)).Lit(Color.black, 0.3f, new Color(1.5f, 0.75f, 0.24f)), false);
            Disc("Gate Landing Edge", root, 0.04f, 3.1f, 0.02f, inlay, false, new Vector3(at.x, 0f, at.z - 1.6f));
            Disc("Gate Landing", root, 0.045f, 3f, 0.02f, dark, false, new Vector3(at.x, 0f, at.z - 1.6f));
        }

        // A flame that needs no fuel.
        private static void Flame(Transform parent, Vector3 localPosition, Material material, float size, float rate)
        {
            var flameObject = new GameObject("Flame");
            flameObject.transform.SetParent(parent, false);
            flameObject.transform.localPosition = localPosition;
            flameObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var system = flameObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.95f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 1.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.9f, size * 1.7f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 1f, 1f, 1f), Color.white);
            main.maxParticles = 60;
            var emission = system.emission;
            emission.rateOverTime = rate;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 9f;
            shape.radius = size * 0.4f;
            var sizeOver = system.sizeOverLifetime;
            sizeOver.enabled = true;
            sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.15f));
            var renderer = flameObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void Disc(string name, Transform parent, float top, float radius, float thickness, Material material, bool solid, Vector3? centre = null)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            Vector3 at = centre ?? Vector3.zero;
            disc.transform.position = new Vector3(at.x, top - thickness * 0.5f, at.z);
            disc.transform.localScale = new Vector3(radius * 2f, thickness * 0.5f, radius * 2f);
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            if (solid)
            {
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
    }
}
