using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Clover.EditorTools
{
    /// <summary>
    /// The second pass of the Fire land's enemies (fire_rework.md, sections 1 to 3), as calls a scene builder makes.
    /// Build the enemies with <see cref="EnemyBuilder.Build"/> as before, then hand the scene's list to
    /// <see cref="Equip"/>, which gives each kind what makes it itself (the ashen's core and burst, the wick's flame,
    /// the gilded's running gold, the priest's mirror, the thirsting's drinking). The rest are placed one by one:
    /// <see cref="Bury"/> a thirsting under the mud, <see cref="WallOfShields"/> for levies placed together,
    /// <see cref="Jar"/>, <see cref="Mirror"/> and <see cref="Target"/> for the props, <see cref="Floor"/> once per
    /// scene for the gold and footprints, <see cref="Heat"/> on the director for the noon's swell, and <see cref="Hud"/>.
    /// A scene that calls none of these is exactly as it was.
    /// </summary>
    public static class FireMechanicsKit
    {
        private const string Folder = SunCityKit.Folder;

        // ------------------------------------------------------------------ the enemies

        /// <summary>Gives every enemy in the list what its kind does in the second pass. Call once, after every enemy is built.</summary>
        public static void Equip(IList<Enemy> enemies, EnemyBuilder.Kit kit)
        {
            var all = enemies.ToArray();
            var gilded = all.Where(enemy => PrefixOf(enemy) == "gilded").ToArray();
            foreach (var enemy in all)
            {
                switch (PrefixOf(enemy))
                {
                    case "ashen": Ashen(enemy, kit); break;
                    case "wick": Wick(enemy, false); break;
                    case "gilded":
                        enemy.gameObject.AddComponent<GoldRuns>();
                        if (enemy.name == EnemyKind.GildedSpearmanName) Spearman(enemy);
                        break;
                    case "sunblind": Priest(enemy, gilded); break;
                    case "thirst": Thirsting(enemy, all); break;
                }
            }
        }

        // An enemy's kind is read in its Awake; at build time it is known by the name the builder gave its object.
        private static string PrefixOf(Enemy enemy) => EnemyKind.ByName(enemy.name).Prefix;

        /// <summary>The ashen's white-hot core (a glow that faces the camera, and a lamp) and the ring its burst shows when it dies early.</summary>
        public static AshenBurst Ashen(Enemy enemy, EnemyBuilder.Kit kit)
        {
            var root = enemy.transform;
            var model = root.Find("Model");
            var coreGlow = BuildKit.Glow(Folder, "m_ashen_core_glow", new Color(3f, 2f, 1f, 1f), 0, true, 2.2f);
            var core = BuildKit.Shape(PrimitiveType.Quad, "Core", root, new Vector3(0f, 1.25f, 0f), Vector3.one * 0.55f, coreGlow);
            core.transform.rotation = kit.TowardCamera;
            core.shadowCastingMode = ShadowCastingMode.Off;
            var lampObject = new GameObject("Core Light");
            lampObject.transform.SetParent(root, false);
            lampObject.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            var lamp = lampObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, 0.72f, 0.42f);
            lamp.range = 3f;
            lamp.intensity = 1f;
            lamp.shadows = LightShadows.None;
            var outline = WorldBars.Marker("Burst Ring Outline", root, 0.05f, kit.Marker);
            var fill = WorldBars.Marker("Burst Ring Fill", root, 0.07f, kit.Marker);
            var burst = enemy.gameObject.AddComponent<AshenBurst>();
            burst.Configure(core.transform, core, lamp, model, outline, fill);
            return burst;
        }

        /// <summary>The wick's flame: it can be lit (by a burst, a beam, another wick), and with <paramref name="burning"/> it starts alight.</summary>
        public static WickFlame Wick(Enemy enemy, bool burning)
        {
            var flame = enemy.GetComponent<WickFlame>();
            if (flame == null) flame = enemy.gameObject.AddComponent<WickFlame>();
            // Called again for a wick Equip already gave a flame: the same lamp, now burning from the start.
            var had = enemy.transform.Find("Flame Light");
            if (had != null)
            {
                flame.Configure(burning, had.GetComponent<Light>());
                return flame;
            }
            var lampObject = new GameObject("Flame Light");
            lampObject.transform.SetParent(enemy.transform, false);
            lampObject.transform.localPosition = new Vector3(0f, 1.9f, 0f);
            var lamp = lampObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, 0.5f, 0.15f);
            lamp.range = 4.5f;
            lamp.intensity = 2.6f;
            lamp.shadows = LightShadows.None;
            flame.Configure(burning, lamp);
            return flame;
        }

        /// <summary>The spearman's throw: the spear that stands glowing in the floor until he fetches it.</summary>
        public static SpearCast Spearman(Enemy enemy)
        {
            var spear = new GameObject("Thrown Spear").transform;
            spear.SetParent(enemy.transform, false);
            var shaft = SunCityKit.For("m_fire_wood");
            var head = SunCityKit.For("m_fire_molten");
            // Built standing up from its point, so it can be stood in the floor by its root.
            BuildKit.Shape(PrimitiveType.Cylinder, "Shaft", spear, new Vector3(0f, 1.25f, 0f), new Vector3(0.07f, 1.05f, 0.07f), shaft);
            BuildKit.Shape(PrimitiveType.Cylinder, "Head", spear, new Vector3(0f, 0.2f, 0f), new Vector3(0.12f, 0.22f, 0.12f), head);
            BuildKit.Shape(PrimitiveType.Sphere, "Glow", spear, new Vector3(0f, 0.05f, 0f), new Vector3(0.5f, 0.12f, 0.5f), head);
            var lampObject = new GameObject("Spear Light");
            lampObject.transform.SetParent(spear, false);
            lampObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var lamp = lampObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, 0.6f, 0.2f);
            lamp.range = 3.5f;
            lamp.intensity = 2.2f;
            lamp.shadows = LightShadows.None;
            spear.gameObject.SetActive(false);
            var cast = enemy.gameObject.AddComponent<SpearCast>();
            cast.Configure(spear, lamp, BuildKit.FindDeep(enemy.transform, "weapon.R"));
            return cast;
        }

        /// <summary>The priest's mirror: his beam drawn as light, and the gilded he calls.</summary>
        public static SunMirror Priest(Enemy enemy, Enemy[] gilded)
        {
            var (beam, lane) = BeamQuads(enemy.transform);
            var mirror = enemy.gameObject.AddComponent<SunMirror>();
            mirror.Configure(beam, lane, gilded);
            return mirror;
        }

        /// <summary>The thirsting drink the dead and broken jars: it is told whose bodies it may drink from.</summary>
        public static ThirstDrinker Thirsting(Enemy enemy, Enemy[] everyone)
        {
            var drinker = enemy.GetComponent<ThirstDrinker>();
            if (drinker == null) drinker = enemy.gameObject.AddComponent<ThirstDrinker>();
            drinker.Configure(enemy.transform.Find("Model"), everyone);
            return drinker;
        }

        /// <summary>
        /// Lays a thirsting under the mud where it stands: a low mound whose cracks breathe dust, which bursts up as
        /// someone comes within <paramref name="wakeWithin"/> metres and lunges for the belt.
        /// </summary>
        public static BuriedMound Bury(Enemy enemy, float wakeWithin = 3.2f)
        {
            enemy.MakeDormant(wakeWithin, "thirst_kneel_loop");
            var mudColour = SunCityKit.Flat("m_sun_mound", new Color(0.47f, 0.42f, 0.36f), 0.02f);
            var mound = BuildKit.Shape(PrimitiveType.Sphere, "Mound", enemy.transform, new Vector3(0f, 0.02f, 0f), new Vector3(1.5f, 0.42f, 1.25f), mudColour);
            // Two darker cracks across its back, where the dust comes out.
            var crack = SunCityKit.Flat("m_sun_mound_crack", new Color(0.2f, 0.17f, 0.15f), 0f);
            BuildKit.Shape(PrimitiveType.Cube, "Crack", mound.transform, new Vector3(0.05f, 0.47f, 0.1f), new Vector3(0.55f, 0.08f, 0.04f), crack, new Vector3(0f, 25f, 0f));
            BuildKit.Shape(PrimitiveType.Cube, "Crack", mound.transform, new Vector3(-0.12f, 0.46f, -0.12f), new Vector3(0.4f, 0.08f, 0.035f), crack, new Vector3(0f, -40f, 0f));
            var buried = enemy.gameObject.AddComponent<BuriedMound>();
            buried.Configure(mound.transform, enemy.transform.Find("Model"));
            return buried;
        }

        /// <summary>Levies placed together lock into a shield wall and advance in step. Give it two or more.</summary>
        public static ShieldWall WallOfShields(string name, Transform parent, params Enemy[] levies)
        {
            var wall = new GameObject(name);
            if (parent != null) wall.transform.SetParent(parent, false);
            var shieldWall = wall.AddComponent<ShieldWall>();
            shieldWall.Configure(levies);
            return shieldWall;
        }

        // ------------------------------------------------------------------ the props

        /// <summary>A clay water jar that breaks to any blow and leaves water the thirsting will come for. Whole again after a rest.</summary>
        public static WaterJar Jar(Transform parent, Vector3 position, AreaDirector director)
        {
            var root = new GameObject("Water Jar");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            var clay = SunCityKit.Flat("m_sun_jar_clay", new Color(0.6f, 0.36f, 0.25f), 0.02f);
            var whole = new GameObject("Whole");
            whole.transform.SetParent(root.transform, false);
            BuildKit.Shape(PrimitiveType.Sphere, "Belly", whole.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.78f, 0.9f, 0.78f), clay);
            BuildKit.Shape(PrimitiveType.Cylinder, "Neck", whole.transform, new Vector3(0f, 1.08f, 0f), new Vector3(0.36f, 0.14f, 0.36f), clay);
            var band = SunCityKit.Flat("m_sun_jar_band", new Color(0.24f, 0.3f, 0.42f), 0f);
            BuildKit.Shape(PrimitiveType.Cylinder, "Band", whole.transform, new Vector3(0f, 0.72f, 0f), new Vector3(0.8f, 0.04f, 0.8f), band);
            var wet = SunCityKit.Flat("m_sun_spill", new Color(0.16f, 0.22f, 0.3f), 0f);
            var spill = BuildKit.Shape(PrimitiveType.Cylinder, "Spill", root.transform, new Vector3(0f, 0.01f, 0f), new Vector3(2.2f, 0.01f, 1.8f), wet);
            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.6f, 0f);
            collider.radius = 0.4f;
            collider.height = 1.2f;
            var jar = root.AddComponent<WaterJar>();
            jar.Configure(whole, spill.transform, director);
            return jar;
        }

        /// <summary>
        /// A mirror on a stand that the player turns a step at a time. Its beam burns enemies, lights wicks, counts as
        /// sunlit where it falls and lights a <see cref="MirrorTarget"/>. Add it to the player's interactables.
        /// </summary>
        public static MirrorStand Mirror(Transform parent, Vector3 position, float facingDegrees)
        {
            var root = new GameObject("Mirror Stand");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            var bronze = SunCityKit.For("m_fire_bronze");
            BuildKit.Shape(PrimitiveType.Cylinder, "Base", root.transform, new Vector3(0f, 0.1f, 0f), new Vector3(0.9f, 0.1f, 0.9f), bronze);
            BuildKit.Shape(PrimitiveType.Cylinder, "Post", root.transform, new Vector3(0f, 0.7f, 0f), new Vector3(0.16f, 0.6f, 0.16f), bronze);
            var head = new GameObject("Head").transform;
            head.SetParent(root.transform, false);
            head.localPosition = new Vector3(0f, 1.4f, 0f);
            var gold = SunCityKit.For("m_fire_sun");
            // The disc stands on edge facing along the head's forward; its back is bronze.
            BuildKit.Shape(PrimitiveType.Cylinder, "Disc", head, new Vector3(0f, 0f, 0.06f), new Vector3(0.9f, 0.03f, 0.9f), gold, new Vector3(90f, 0f, 0f));
            BuildKit.Shape(PrimitiveType.Cylinder, "Back", head, new Vector3(0f, 0f, -0.01f), new Vector3(0.96f, 0.03f, 0.96f), bronze, new Vector3(90f, 0f, 0f));
            head.rotation = Quaternion.Euler(0f, facingDegrees, 0f);
            var (beam, lane) = BeamQuads(root.transform);
            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.8f, 0f);
            collider.radius = 0.45f;
            collider.height = 1.6f;
            var stand = root.AddComponent<MirrorStand>();
            stand.Prompt = "Turn the mirror";
            stand.Configure(head, beam, lane);
            return stand;
        }

        /// <summary>
        /// A sun-disc for a beam to light, standing at a point and facing a way. Lit, it puts away <paramref name="opens"/>
        /// (a door's block) for good and raises <see cref="MirrorTarget.Lit"/> for anything else to answer.
        /// </summary>
        public static MirrorTarget Target(Transform parent, Vector3 position, float facingDegrees, GameObject opens)
        {
            var root = new GameObject("Mirror Target");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, facingDegrees, 0f));
            var stone = SunCityKit.For("m_kit_stone_new");
            BuildKit.Shape(PrimitiveType.Cylinder, "Disc Stone", root.transform, Vector3.zero, new Vector3(1.3f, 0.08f, 1.3f), stone, new Vector3(90f, 0f, 0f));
            var glowMaterial = BuildKit.Glow(Folder, "m_fire_target_glow", Color.white, 0, true, 2f);
            var glow = BuildKit.Shape(PrimitiveType.Quad, "Glow", root.transform, new Vector3(0f, 0f, 0.1f), Vector3.one * 1.6f, glowMaterial, new Vector3(0f, 180f, 0f));
            glow.shadowCastingMode = ShadowCastingMode.Off;
            var box = root.AddComponent<BoxCollider>();
            box.size = new Vector3(1.3f, 1.3f, 0.3f);
            var target = root.AddComponent<MirrorTarget>();
            target.Configure(opens, null, glow);
            return target;
        }

        // The beam's two quads of light: the streak at chest height and the lane on the ground. They are laid in the world each frame.
        private static (Transform beam, Transform lane) BeamQuads(Transform parent)
        {
            // The priest's gathered sun burns: a jet of flame down the beam, and the lane under it scorching orange, not
            // a bar of white light (the owner's review, 2026-11-17).
            var beamMaterial = BuildKit.Fire(Folder, "m_fire_sunbeam_flame", 1, new Vector2(0f, 2.4f), 4f);
            var laneMaterial = BuildKit.Glow(Folder, "m_fire_sunlane_heat", new Color(1.3f, 0.42f, 0.1f, 0.5f), 3, true, 1f);
            var beam = BuildKit.Shape(PrimitiveType.Quad, "Beam", parent, Vector3.zero, Vector3.one, beamMaterial);
            var lane = BuildKit.Shape(PrimitiveType.Quad, "Beam Lane", parent, Vector3.zero, Vector3.one, laneMaterial);
            beam.shadowCastingMode = ShadowCastingMode.Off;
            lane.shadowCastingMode = ShadowCastingMode.Off;
            beam.gameObject.SetActive(false);
            lane.gameObject.SetActive(false);
            return (beam.transform, lane.transform);
        }

        // ------------------------------------------------------------------ the scene

        /// <summary>The pool the molten gold, the burning footprints and the warden's pour are drawn from. One per scene.</summary>
        public static MoltenFloor Floor(Transform parent, GameObject player, IList<Enemy> enemies, int capacity = 40)
        {
            var root = new GameObject("Molten Floor");
            root.transform.SetParent(parent, false);
            var molten = SunCityKit.For("m_fire_molten");
            var pieces = new Transform[capacity];
            for (int i = 0; i < capacity; i++)
            {
                var piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                piece.name = "Gold " + i;
                piece.transform.SetParent(root.transform, false);
                var renderer = piece.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = molten;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                piece.GetComponent<Collider>().enabled = false;
                piece.SetActive(false);
                pieces[i] = piece.transform;
            }
            var floor = root.AddComponent<MoltenFloor>();
            floor.Configure(pieces, player.GetComponent<PlayerDefence>(), player.GetComponent<PlayerController>(), enemies.ToArray());
            return floor;
        }

        /// <summary>
        /// The heat of noon on the scene's director: the rhythm of its swells, the calm rectangles where they never
        /// burn (an elite's arena), and the whitening look. Each rectangle is least x, greatest x, least z, greatest z.
        /// </summary>
        public static NoonHeat Heat(GameObject director, GameObject player, HeatSchedule rhythm, Vector4[] calm, IList<Enemy> enemies, LandHud hud)
        {
            var heat = director.AddComponent<NoonHeat>();
            var lookObject = new GameObject("Noon Swell Look");
            lookObject.transform.SetParent(director.transform, false);
            var volume = lookObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.weight = 0f;
            volume.sharedProfile = SwellProfile();
            heat.Configure(rhythm, calm, player.GetComponent<PlayerDefence>(), RenderSettings.sun, volume, hud, enemies.ToArray());
            return heat;
        }

        // The sky and ground blazing with heat: a little brighter, hotter and redder, an orange bloom, never white (the
        // owner's review of 2026-11-17: heat and scorch, not bright light). Not grey either: the
        // owner saw the first version (paler, flatter) read as a cold, washed-out ground rather than heat.
        private static VolumeProfile SwellProfile()
        {
            string path = $"{Folder}/NoonSwellProfile.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            var colour = profile.Add<ColorAdjustments>(true);
            colour.name = nameof(ColorAdjustments);
            AssetDatabase.AddObjectToAsset(colour, profile);
            colour.postExposure.Override(0.25f);
            colour.saturation.Override(12f);
            colour.contrast.Override(8f);
            colour.colorFilter.Override(new Color(1f, 0.8f, 0.6f));
            var bloom = profile.Add<Bloom>(true);
            bloom.name = nameof(Bloom);
            AssetDatabase.AddObjectToAsset(bloom, profile);
            bloom.intensity.Override(1.4f);
            bloom.threshold.Override(1f);
            bloom.tint.Override(new Color(1f, 0.55f, 0.25f));
            EditorUtility.SetDirty(profile);
            return profile;
        }

        /// <summary>The land's own HUD lines: the sun mark at the top of the screen and the embers waiting to be reclaimed under it.</summary>
        public static LandHud Hud()
        {
            var canvasObject = new GameObject("Land HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<ScreenFit>();
            var sun = UiArt.Sun();
            Image Disc(string name, Color colour)
            {
                var discObject = new GameObject(name, typeof(RectTransform));
                discObject.transform.SetParent(canvasObject.transform, false);
                var rect = (RectTransform)discObject.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -26f);
                rect.sizeDelta = new Vector2(84f, 84f);
                var image = discObject.AddComponent<Image>();
                image.sprite = sun;
                image.color = colour;
                image.raycastTarget = false;
                return image;
            }
            var ring = Disc("Sun Mark", new Color(1f, 1f, 1f, 0.35f));
            var fill = Disc("Sun Mark Fill", Color.white);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Top;
            fill.fillClockwise = true;
            var textObject = new GameObject("Embers To Reclaim", typeof(RectTransform));
            textObject.transform.SetParent(canvasObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.anchoredPosition = new Vector2(0f, -118f);
            textRect.sizeDelta = new Vector2(600f, 40f);
            var text = textObject.AddComponent<Text>();
            text.font = UiArt.Font();
            text.fontSize = 26;
            text.alignment = TextAnchor.UpperCenter;
            text.color = new Color(1f, 0.72f, 0.35f, 1f);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            var outline = textObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);
            var hud = canvasObject.AddComponent<LandHud>();
            hud.Configure(ring, fill, text);
            return hud;
        }
    }
}
