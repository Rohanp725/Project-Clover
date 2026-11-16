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
            Disc("Floor", world, 0f, Radius, 1.2f, dark, true);
            Disc("Paving", world, 0.01f, Radius - 0.8f, 0.02f, stone, false);
            Disc("Ring Outer", world, 0.02f, 11.2f, 0.02f, pale, false);
            Disc("Paving Middle", world, 0.03f, 10.8f, 0.02f, stone, false);
            Disc("Ring Inner", world, 0.04f, 5.2f, 0.02f, rune, false);
            Disc("Paving Inner", world, 0.05f, 5f, 0.02f, stone, false);
            Disc("Seal", world, 0.06f, 1.2f, 0.02f, rune, false);
            Disc("Seal Core", world, 0.07f, 1f, 0.02f, dark, false);

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

            BossBar(boss);
            PlayerBuilder.AddRunEnd(built.Player);
            PlayerBuilder.AddTitle("Narukami", EnemyKind.Narukami().Title);
            SceneBuild.Finish(scene, ScenePath, false);
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
        private static void BossBar(Enemy boss)
        {
            var hud = GameObject.Find("HUD");
            if (hud == null) return;
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
            hud.AddComponent<BossHud>().Configure(boss, panel, health, stagger);
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
