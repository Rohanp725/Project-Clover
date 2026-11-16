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
            pale = BuildKit.Toon(Folder, "m_hub_pale", new Color(0.62f, 0.62f, 0.74f), false, null, 0f, 0.02f);
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

            var built = PlayerBuilder.Build(new Vector3(0f, 0.05f, -4.6f), PlayerBuilder.ControlsHint);
            if (built.Player == null) return;
            LookBuilder.Apply(LookBuilder.Stillness, built.Camera);
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
            Disc("Gate Step", root.transform, 0.07f, 2.3f, 0.06f, pale, false, new Vector3(at.x, 0f, at.z - 1.6f));

            var door = root.AddComponent<SceneDoor>();
            door.Prompt = "Go through, to the castle under the storm";
            door.LeadsTo = "Castle";
            door.Range = 3f;
            return door;
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
            var renderer = disc.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            if (!solid) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            disc.isStatic = true;
        }
    }
}
