using System.Collections.Generic;
using System.Linq;
using Clover.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Clover.EditorTools
{
    /// <summary>
    /// The sun city's life (the visual overview, steps 6 and 7): something always moves (palms and reeds in the wind,
    /// hanging linen, dust, ash, embers, smoke drifting over the roofs, kites wheeling so high only their shadows
    /// cross the street) and each district has its own air and light. Called by <see cref="SunCityBuilder"/> after
    /// everything else is placed, with a dice of its own, so nothing the first passes placed moves.
    /// </summary>
    public static class SunCityLife
    {
        private const string Folder = SunCityKit.Folder;
        private static System.Random dice;

        public static void Build(Transform world, Transform player, LandZones zones)
        {
            dice = new System.Random(4242);
            var group = BuildKit.Group("Life", world);
            Palms(group);
            Swaying(world);
            Air(group, player, zones);
            HallMotes(group);
            Kites(group, player);
            Beacons(world, group);
        }

        // ------------------------------------------------------------------ palms and reeds

        private static Dictionary<string, Material> treePaint, reedPaint, clothPaint;

        // Date palms where people kept them: along the fishers' walk and the banks, in the well's court, at the quay's
        // ends, one in the lamp square, and in pairs on the king's ground, set in the stone as he would have them.
        private static void Palms(Transform parent)
        {
            treePaint = SunCityKit.Swaying("tree", 0.0028f);
            reedPaint = SunCityKit.Swaying("reed", 0.02f);
            var group = BuildKit.Group("Palms", parent);
            foreach (var (x, y, z, kind) in new[]
            {
                // District 1: the banks, the well, the quay.
                (-33.5f, 3f, 20f, "a"), (-36.6f, 3f, 58f, "b"), (-37f, 3f, 95f, "c"), (-13f, 3f, 5f, "b"), (22f, 3f, 40f, "b"),
                (28f, 3f, 72f, "a"), (31f, 3f, 104f, "c"), (-63.5f, 1f, 66f, "a"), (-48f, 1f, 88f, "b"), (-37f, 4f, 132f, "b"), (35f, 4f, 121f, "a"),
                // District 2: a palm left in the lamp square and one at the market's corner.
                (13f, 6f, 159f, "a"), (-32.5f, 6f, 178.5f, "b"),
                // District 3: in pairs, the king's planting.
                // (Kept to the walls: a palm in the middle of the way is a green blot over the player from this camera.)
                (-21.5f, 10f, 322f, "c"), (21.5f, 10f, 321f, "c"), (-22.5f, 16f, 389f, "b"), (22.5f, 16f, 389f, "b"),
            })
                Palm(group, new Vector3(x, y, z), kind, (float)dice.NextDouble() * 360f, 0.85f + (float)dice.NextDouble() * 0.3f);
            // Reeds and papyrus grown up dry along the foot of the banks, where the water stood last.
            foreach (var (x, z) in new[] { (-6.4f, 12f), (-7.5f, 19f), (-14f, 26f), (-19.6f, 36f), (-21f, 50f), (-17f, 62f), (-12f, 78f), (-8f, 90f), (5.6f, 20f), (7.4f, 28f), (6f, 38f), (3.8f, 50f), (1.2f, 60f), (6f, 72f), (15f, 82f), (19f, 94f) })
            {
                string piece = dice.NextDouble() < 0.6 ? "reeds" : "papyrus";
                var at = new Vector3(x + ((float)dice.NextDouble() - 0.5f) * 1.2f, 0f, z + ((float)dice.NextDouble() - 0.5f) * 2f);
                var clump = SunCityKit.City(piece, group, at, (float)dice.NextDouble() * 360f, Vector3.one * (0.75f + (float)dice.NextDouble() * 0.45f), reedPaint, false);
                Loose(clump);
            }
        }

        private static void Palm(Transform parent, Vector3 foot, string kind, float yaw, float scale)
        {
            var palm = SunCityKit.City("palm_" + kind, parent, foot, yaw, Vector3.one * scale, treePaint);
            if (palm == null) return;
            Loose(palm);
            var trunk = palm.AddComponent<CapsuleCollider>();
            trunk.center = new Vector3(0f, 1.5f, 0f);
            trunk.radius = 0.32f;
            trunk.height = 3f;
        }

        // A piece that moves is drawn as itself: not static, so neither the merge nor Unity's batching takes it, and its
        // sway is measured from its own foot.
        private static void Loose(GameObject piece)
        {
            if (piece == null) return;
            foreach (var t in piece.GetComponentsInChildren<Transform>(true)) GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
        }

        // What hangs and grows already in the city sways: the river's reed clumps, the yard's linen strips and the roofs'
        // drying lines.
        private static void Swaying(Transform world)
        {
            clothPaint = SunCityKit.Swaying("cloth", 0.012f);
            var hung = SunCityKit.Flat("m_sun_wrap_hung", new Color(0.72f, 0.69f, 0.62f), 0.012f);
            hung.SetFloat("_Sway", -0.14f);
            EditorUtility.SetDirty(hung);
            foreach (var t in world.GetComponentsInChildren<Transform>(true).ToArray())
            {
                if (t == null) continue;
                if (t.name == "reeds" && t.parent != null && t.parent.name != "Palms") Repaint(t.gameObject, reedPaint);
                else if (t.name == "prop_linenline") Repaint(t.gameObject, clothPaint);
                else if (t.name == "Linen Strip")
                {
                    t.GetComponent<Renderer>().sharedMaterial = hung;
                    Loose(t.gameObject);
                }
            }
        }

        private static void Repaint(GameObject piece, Dictionary<string, Material> paint)
        {
            foreach (var renderer in piece.GetComponentsInChildren<Renderer>(true))
            {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i] == null) continue;
                    string slot = slots[i].name;
                    // The slot as the kit names it, whatever a district repainted it to: m_city_<name>.
                    int last = slot.LastIndexOf('_');
                    string key = "m_city_" + slot.Substring(last + 1);
                    if (paint.TryGetValue(key, out var instead)) slots[i] = instead;
                }
                renderer.sharedMaterials = slots;
            }
            Loose(piece);
        }

        // ------------------------------------------------------------------ the air and the light of each district

        private static void Air(Transform parent, Transform player, LandZones zones)
        {
            var holder = BuildKit.Group("District Air", parent);
            holder.position = player.position;
            var mote = BuildKit.Glow(Folder, "m_city_air_mote", new Color(1f, 0.9f, 0.72f, 0.55f), 0, false, 2.2f);
            var glare = BuildKit.Glow(Folder, "m_city_air_glare", new Color(1.25f, 1f, 0.7f, 0.5f), 0, true, 2.6f);
            // Seen against a pale floor, so dust is a warm spark of light and ash a dark ragged flake, both big enough to read
            // from twelve metres (the first try, at a few centimetres and soft, did not show at all).
            var sunMote = BuildKit.Glow(Folder, "m_city_air_sun_mote", new Color(1.2f, 1f, 0.75f, 0.45f), 0, true, 2.4f);
            var sand = BuildKit.Glow(Folder, "m_city_air_sand", new Color(0.72f, 0.6f, 0.42f, 0.22f), 0, false, 1.4f);
            var ash = BuildKit.Glow(Folder, "m_city_air_ash", new Color(0.2f, 0.18f, 0.17f, 0.95f), 0, false, 2.2f, 0.35f);
            var ember = BuildKit.Glow(Folder, "m_city_ember", new Color(3.2f, 1.7f, 0.5f, 1f), 0, false, 2.6f);
            var smoke = BuildKit.Smoke(VfxBuilder.Folder, "m_vfx_smoke", new Color(0.85f, 0.8f, 0.76f, 0.75f));

            // The boxes sit where the camera looks at their height: the higher the air, the nearer the camera and the
            // further south of the player the middle of the picture is (at 5 m up, about 3 m south).
            // District 1: the river town. Dust motes hanging in the light, and sand blown low along the ground, east.
            var river = BuildKit.Group("Air River", holder);
            Drift(river, "Dust", sunMote, new Vector3(0f, 3f, 0f), new Vector3(26f, 6f, 18f), 18f, new Vector2(6f, 9f), new Vector2(0.08f, 0.14f), new Vector3(0.6f, 0.05f, 0.15f), 0.35f, Color.white, 200);
            var blown = Drift(river, "Blown Sand", sand, new Vector3(-6f, 0.25f, 3f), new Vector3(24f, 0.4f, 22f), 14f, new Vector2(1.2f, 2.2f), new Vector2(0.35f, 0.8f), new Vector3(4.2f, 0.1f, 0.4f), 0.4f, Color.white, 60);
            Stretch(blown, 0.22f);
            // District 2: the city of wicks. Ash falling, sparks rising from the streets, smoke drifting across between
            // the camera and the roofs.
            var wicks = BuildKit.Group("Air Wicks", holder);
            Drift(wicks, "Ash", ash, new Vector3(0f, 5f, -1f), new Vector3(24f, 3f, 16f), 30f, new Vector2(5f, 8f), new Vector2(0.1f, 0.18f), new Vector3(0.35f, -0.55f, 0.1f), 0.5f, Color.white, 220);
            var sparks = Drift(wicks, "Sparks", ember, new Vector3(0f, 0.6f, 3f), new Vector3(28f, 1f, 22f), 5f, new Vector2(1.8f, 3.2f), new Vector2(0.04f, 0.08f), new Vector3(0.2f, 1.2f, 0.1f), 0.6f, new Color(1f, 0.85f, 0.55f, 1f), 40);
            Stretch(sparks, 0.08f);
            Cooling(sparks);
            var wisps = Drift(wicks, "Smoke", smoke, new Vector3(-4f, 4.6f, -2f), new Vector3(20f, 1.2f, 12f), 2.6f, new Vector2(8f, 11f), new Vector2(2.5f, 4.5f), new Vector3(0.6f, 0.1f, 0.2f), 0.2f, new Color(0.26f, 0.23f, 0.22f, 0.34f), 34);
            Puff(wisps);
            // District 3: the king's ground. Glare motes in the white light, and a little blown sand.
            var king = BuildKit.Group("Air King", holder);
            Drift(king, "Glare", glare, new Vector3(0f, 2.6f, 0f), new Vector3(26f, 5f, 18f), 12f, new Vector2(5f, 8f), new Vector2(0.08f, 0.14f), new Vector3(0.25f, 0.06f, 0.1f), 0.3f, Color.white, 110);
            // The court's smoke drifting south over the king's ground, between the camera and the stone: the god is
            // north, out of the picture, and this is how he is in it.
            var pall = Drift(king, "Pall", smoke, new Vector3(0f, 5f, 0f), new Vector3(22f, 1.2f, 18f), 1.5f, new Vector2(13f, 17f), new Vector2(4f, 7f), new Vector3(0.25f, 0f, -0.9f), 0.15f, new Color(0.24f, 0.21f, 0.2f, 0.22f), 24);
            Puff(pall);
            var fine = Drift(king, "Fine Sand", sand, new Vector3(-6f, 0.2f, 3f), new Vector3(24f, 0.3f, 22f), 5f, new Vector2(1f, 1.8f), new Vector2(0.3f, 0.6f), new Vector3(3.6f, 0.05f, 0.3f), 0.3f, Color.white, 30);
            Stretch(fine, 0.2f);
            // The hall: only a little dust here; the motes are in its bars of light (HallMotes).
            var hall = BuildKit.Group("Air Hall", holder);
            var hallPall = Drift(hall, "Pall", smoke, new Vector3(0f, 5f, 0f), new Vector3(22f, 1.2f, 18f), 1.6f, new Vector2(13f, 17f), new Vector2(4.5f, 7.5f), new Vector3(0.2f, 0f, -1f), 0.15f, new Color(0.22f, 0.19f, 0.18f, 0.24f), 26);
            Puff(hallPall);
            Drift(hall, "Dust", mote, new Vector3(0f, 2.5f, 3f), new Vector3(30f, 5f, 22f), 3f, new Vector2(7f, 10f), new Vector2(0.04f, 0.07f), new Vector3(0.1f, 0.04f, 0.05f), 0.2f, new Color(1f, 1f, 1f, 0.6f), 50);

            var looks = new[]
            {
                // Warm ochre and a little haze over the river town.
                Look(0, 0.15f, 20f, -6f, new Color(1f, 0.95f, 0.85f), 1.1f),
                // Sooty in the city of wicks: darker, harder, greyer.
                Look(1, 0.02f, 27f, -16f, new Color(0.95f, 0.9f, 0.86f), 1.1f),
                // Glaring on the king's ground: brighter, and the gold catches.
                Look(2, 0.25f, 20f, -10f, new Color(1f, 0.98f, 0.93f), 1.35f),
                // Cool deep shade in the hall, the bars of light the brighter for it.
                Look(3, 0.08f, 30f, -14f, new Color(0.9f, 0.92f, 1f), 1.25f),
            };
            var volumes = new Volume[looks.Length];
            for (int i = 0; i < looks.Length; i++)
            {
                var volumeObject = new GameObject("District Look " + (i + 1));
                volumeObject.transform.SetParent(parent, false);
                var volume = volumeObject.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 5f;
                volume.weight = 0f;
                volume.sharedProfile = looks[i];
                volumes[i] = volume;
            }
            var mood = holder.gameObject.AddComponent<DistrictMood>();
            mood.Configure(zones, player, new[] { river.gameObject, wicks.gameObject, king.gameObject, hall.gameObject }, volumes, new[] { 0.016f, 0.015f, 0.01f, 0.012f });
        }

        private static VolumeProfile Look(int district, float exposure, float contrast, float saturation, Color filter, float bloomIntensity)
        {
            string path = $"{Folder}/District{district + 1}Profile.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            var colour = profile.Add<ColorAdjustments>(true);
            colour.name = nameof(ColorAdjustments);
            AssetDatabase.AddObjectToAsset(colour, profile);
            // Whole values, not changes: a volume's setting stands in for the base look's while it is in force.
            colour.postExposure.Override(exposure);
            colour.contrast.Override(contrast);
            colour.saturation.Override(saturation);
            colour.colorFilter.Override(filter);
            var bloom = profile.Add<Bloom>(true);
            bloom.name = nameof(Bloom);
            AssetDatabase.AddObjectToAsset(bloom, profile);
            bloom.intensity.Override(bloomIntensity);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        // Motes turning slowly in the hall's bars of light: the light reads as light falling through dust.
        private static void HallMotes(Transform parent)
        {
            var group = BuildKit.Group("Hall Motes", parent);
            var mote = BuildKit.Glow(Folder, "m_city_air_hall_mote", new Color(1.3f, 1.05f, 0.75f, 0.6f), 0, true, 2.4f);
            foreach (var lit in SunCityBuilder.HallLight)
            {
                float wide = lit.y - lit.x, deep = lit.w - lit.z;
                var system = Drift(group, "Motes", mote, new Vector3((lit.x + lit.y) * 0.5f, SunCityBuilder.HallFloor + 3f, (lit.z + lit.w) * 0.5f), new Vector3(wide, 5f, deep),
                    wide * deep * 0.05f, new Vector2(6f, 10f), new Vector2(0.04f, 0.08f), new Vector3(0.05f, 0.05f, 0.02f), 0.15f, Color.white, 120);
                var main = system.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }
        }

        // A box of air that particles drift through: dust, ash, sparks, smoke.
        private static ParticleSystem Drift(Transform parent, string name, Material material, Vector3 centre, Vector3 box, float rate, Vector2 life, Vector2 size,
            Vector3 velocity, float noise, Color colour, int most)
        {
            var host = new GameObject(name);
            host.transform.SetParent(parent, false);
            host.transform.localPosition = centre;
            var system = host.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = true;
            main.prewarm = true;
            main.playOnAwake = true;
            main.duration = 6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = most;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life.x, life.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startSpeed = 0f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            main.startColor = colour;
            var emission = system.emission;
            emission.rateOverTime = rate;
            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = box;
            var move = system.velocityOverLifetime;
            move.enabled = true;
            move.space = ParticleSystemSimulationSpace.World;
            move.x = new ParticleSystem.MinMaxCurve(velocity.x * 0.6f, velocity.x * 1.4f);
            move.y = new ParticleSystem.MinMaxCurve(velocity.y * 0.6f, velocity.y * 1.4f);
            move.z = new ParticleSystem.MinMaxCurve(velocity.z * 0.6f, velocity.z * 1.4f);
            if (noise > 0f)
            {
                var wander = system.noise;
                wander.enabled = true;
                wander.strength = noise;
                wander.frequency = 0.35f;
                wander.scrollSpeed = 0.15f;
            }
            var fade = system.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var renderer = host.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return system;
        }

        private static void Stretch(ParticleSystem system, float scale)
        {
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = scale;
            renderer.lengthScale = 1.2f;
        }

        // Sparks cool from yellow through red to dark as they rise.
        private static void Cooling(ParticleSystem system)
        {
            var fade = system.colorOverLifetime;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0f), new GradientColorKey(new Color(1f, 0.38f, 0.1f), 0.45f), new GradientColorKey(new Color(0.25f, 0.06f, 0.03f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
        }

        // Smoke grows as it drifts and spreads.
        private static void Puff(ParticleSystem system)
        {
            var grow = system.sizeOverLifetime;
            grow.enabled = true;
            grow.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.4f));
            var spin = system.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        }

        // ------------------------------------------------------------------ the tall things, burning

        // Fire on the tops of the tall things the vistas look toward, each with a column of smoke leaning off it: the
        // embalmers' gate towers, the pylon before the hall, the gate of noon's wall and the obelisk of the court beyond.
        private static void Beacons(Transform world, Transform parent)
        {
            var group = BuildKit.Group("Beacons", parent);
            foreach (float x in new[] { -8.5f, 8.5f }) Beacon(group, new Vector3(x, 15.3f, 220f), 1.3f);
            var pylon = BuildKit.FindDeep(world, "pylon_gate");
            if (pylon != null)
            {
                var bounds = Bounds(pylon);
                foreach (float side in new[] { -1f, 1f }) Beacon(group, new Vector3(bounds.center.x + side * bounds.extents.x * 0.62f, bounds.max.y, 414f), 1.6f);
            }
            foreach (float x in new[] { -6.2f, 6.2f }) Beacon(group, new Vector3(x, 30f, 523.75f), 1.5f);
            var obelisk = BuildKit.FindDeep(world, "obelisk");
            if (obelisk != null)
            {
                var bounds = Bounds(obelisk);
                SunCityKit.SmokeColumn(group, new Vector3(bounds.center.x, bounds.max.y + 0.5f, bounds.center.z), 2.2f);
            }
        }

        private static Bounds Bounds(Transform piece)
        {
            var renderers = piece.GetComponentsInChildren<Renderer>();
            var bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(piece.position, Vector3.one);
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        // A fire bowl on a high place: the kit's brazier, its fire burning in the bowl, and a plume of smoke.
        private static void Beacon(Transform parent, Vector3 foot, float scale)
        {
            SunCityKit.City("brazier", parent, foot, 180f, Vector3.one * scale);
            SunCityKit.Smoulder(parent, foot + Vector3.up * 1.76f * scale, 0.42f * scale, false, 0f);
            SunCityKit.SmokeColumn(parent, foot + Vector3.up * (1.9f * scale), scale);
        }

        // ------------------------------------------------------------------ kites

        // Three kites wheeling high over wherever the player is: drawn only as their shadows, which cross the ground.
        private static void Kites(Transform parent, Transform player)
        {
            var group = BuildKit.Group("Kites", parent);
            var dark = SunCityKit.Shade;
            var birds = new Transform[3];
            for (int i = 0; i < birds.Length; i++)
            {
                var bird = BuildKit.Group("Kite", group);
                float span = 1.5f + i * 0.25f;
                foreach (var (at, size, euler) in new[]
                {
                    (Vector3.zero, new Vector3(0.18f, 0.1f, 0.75f), Vector3.zero),
                    (new Vector3(-span * 0.27f, 0f, 0.05f), new Vector3(span * 0.55f, 0.03f, 0.34f), new Vector3(0f, -12f, 0f)),
                    (new Vector3(span * 0.27f, 0f, 0.05f), new Vector3(span * 0.55f, 0.03f, 0.34f), new Vector3(0f, 12f, 0f)),
                    (new Vector3(0f, 0f, -0.45f), new Vector3(0.3f, 0.03f, 0.28f), Vector3.zero),
                })
                {
                    var part = BuildKit.Shape(PrimitiveType.Cube, "Shape", bird, at, size, dark, euler);
                    part.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }
                birds[i] = bird;
            }
            group.gameObject.AddComponent<CirclingBirds>().Configure(player, birds);
        }
    }
}
