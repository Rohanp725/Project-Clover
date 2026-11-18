using System.Collections.Generic;
using Clover.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds Khepharu, the Sun-Bearer, from boss_sunform.fbx: twenty rigid pieces (and the falcon's two boned wings)
    /// set round the thorax where the modeller's notes put them, the sun and its dull twin, the parts a blow can land
    /// on, and every mark its attacks draw. The scarab's pieces and the falcon's hang under two holders so the fight
    /// shows one shape at a time; the thorax and the wing cases belong to both. The model's mask of the king's face is
    /// left out: the owner's word (2026-11-17) is that the scarab has no face at all.
    /// </summary>
    public static class SunBearerBuilder
    {
        // The thorax's origin is the middle of its underside, this far above the floor when the scarab stands.
        private static readonly Vector3 Thorax = new Vector3(0f, 0.7f, 0f);

        public static SunBearer Build(Transform target, float floorRadius, Material marker)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(SunCityKit.SunFormPath);
            if (asset == null)
            {
                Debug.LogError($"[Clover.Build] No model at {SunCityKit.SunFormPath}; the Sun-Bearer was not built.");
                return null;
            }
            var root = new GameObject("Sun Bearer");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            SunCityKit.Dress(model);

            var body = BuildKit.Group("Body", root.transform);
            var scarab = BuildKit.Group("Scarab", body);
            var falcon = BuildKit.Group("Falcon", body);

            // Shared by both shapes.
            Piece(model, "thorax", body, Thorax);
            var caseLeft = Piece(model, "wing_case_left", body, Thorax + new Vector3(-0.03f, 1.6f, -0.7f));
            var caseRight = Piece(model, "wing_case_right", body, Thorax + new Vector3(0.03f, 1.6f, -0.7f));

            // The scarab.
            Piece(model, "scarab_abdomen", scarab, Thorax + new Vector3(0f, -0.35f, -1f));
            Piece(model, "scarab_head", scarab, Thorax + new Vector3(0f, 0.05f, 1.15f));
            var legs = new List<Transform>();
            foreach (var (name, hip) in new[] { ("foreleg", new Vector3(1.45f, 0.5f, 0.75f)), ("midleg", new Vector3(1.6f, 0.5f, -0.2f)), ("hindleg", new Vector3(1.5f, 0.4f, -1.9f)) })
            {
                legs.Add(Piece(model, $"scarab_{name}_left", scarab, Thorax + new Vector3(-hip.x, hip.y, hip.z)));
                legs.Add(Piece(model, $"scarab_{name}_right", scarab, Thorax + hip));
            }

            // The falcon.
            Piece(model, "falcon_head", falcon, Thorax + new Vector3(0f, 1.75f, 0.9f));
            Piece(model, "falcon_tail", falcon, Thorax + new Vector3(0f, 0.6f, -1.1f));
            Piece(model, "falcon_talon_left", falcon, Thorax + new Vector3(-0.7f, 0.15f, -0.2f));
            Piece(model, "falcon_talon_right", falcon, Thorax + new Vector3(0.7f, 0.15f, -0.2f));
            var wingLeft = Piece(model, "falcon_wing_left", falcon, Thorax + new Vector3(-1.5f, 1.5f, -0.1f));
            var wingRight = Piece(model, "falcon_wing_right", falcon, Thorax + new Vector3(1.5f, 1.5f, -0.1f));

            // The sun, burning and dull, under one holder that the fight moves.
            var sun = BuildKit.Group("Sun", root.transform);
            var burning = Piece(model, "sun", sun, Vector3.zero);
            var dull = Piece(model, "sun_loose", sun, Vector3.zero);
            foreach (var renderer in sun.GetComponentsInChildren<Renderer>(true)) renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // A ball of burning matter, not a white lamp with gold rays (the owner's review, 2026-11-17): the ball is
            // molten and churning, and its gold spikes are not drawn; flames lick off its rim instead (SunBearerLook).
            var ball = burning.GetComponentInChildren<Renderer>(true);
            if (ball != null)
            {
                var molten = BuildKit.Molten(BuildKit.GeneratedRoot + "/Enemies", "m_sunform_burning", Color.white, 0.38f, 0.26f);
                var shown = ball.sharedMaterials;
                for (int i = 0; i < shown.Length; i++) shown[i] = i == 0 ? molten : SunCityKit.Hidden();
                ball.sharedMaterials = shown;
            }
            Object.DestroyImmediate(model);

            var renderers = new List<Renderer>(body.GetComponentsInChildren<Renderer>(true));
            foreach (var renderer in renderers)
                if (renderer is SkinnedMeshRenderer skinned) skinned.updateWhenOffscreen = true;

            var bearer = root.AddComponent<SunBearer>();
            bearer.FloorRadius = floorRadius;

            // What a blow can land on: the head, the trunk, and the sun when it lies loose.
            var headReach = BuildKit.Group("Head Reach", body);
            var skull = headReach.gameObject.AddComponent<SphereCollider>();
            skull.center = new Vector3(0f, 1.3f, 2f);
            skull.radius = 1.6f;
            var headPart = headReach.gameObject.AddComponent<ColossusPart>();
            headPart.Configure(bearer, skull, true, bearer.HeadDamage);
            var trunkReach = BuildKit.Group("Body Reach", body);
            var trunk = trunkReach.gameObject.AddComponent<BoxCollider>();
            // Tall, so the falcon standing on the sun can be reached from the floor.
            trunk.center = new Vector3(0f, 0.4f, -1.1f);
            trunk.size = new Vector3(4f, 6.4f, 5.6f);
            var bodyPart = trunkReach.gameObject.AddComponent<ColossusPart>();
            bodyPart.Configure(bearer, trunk, false, 1f);
            var sunReach = BuildKit.Group("Sun Reach", sun);
            var reach = sunReach.gameObject.AddComponent<SphereCollider>();
            reach.radius = 1.5f;
            var sunPart = sunReach.gameObject.AddComponent<ColossusPart>();
            sunPart.Configure(bearer, reach, false, 1f);
            // The scarab on the floor is solid: nobody walks through it.
            var bulk = BuildKit.Group("Bulk", body).gameObject.AddComponent<BoxCollider>();
            bulk.center = new Vector3(0f, 1.4f, -0.7f);
            bulk.size = new Vector3(3.2f, 2.8f, 4.8f);
            bulk.enabled = false;

            // The falcon's shadow on the floor, the first tell of everything it does; and the column of fire the sun comes down.
            var blob = BuildKit.Shape(PrimitiveType.Cylinder, "Falcon Shadow", root.transform, Vector3.zero, new Vector3(5f, 0.004f, 5f), SunCityKit.Shade);
            blob.GetComponent<MeshFilter>().sharedMesh = BuildKit.RoundMesh();
            blob.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            blob.enabled = false;
            // A stream of fire standing on the spear's point: a quad turned to the camera, which never turns. The fight
            // scales its holder as it did the old thread of light, its width across and half its height up; the beam
            // shape runs along the quad's length, so it is turned to stand.
            var line = BuildKit.Group("Light Line", root.transform);
            var column = BuildKit.Shape(PrimitiveType.Quad, "Fire Column", line, Vector3.zero, new Vector3(2f, 2.6f, 1f),
                BuildKit.Fire(BuildKit.GeneratedRoot + "/Enemies", "m_sunform_column", 12, new Vector2(1.6f, 0f), 5f), new Vector3(0f, 0f, -90f));
            column.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            column.receiveShadows = false;
            column.enabled = false;

            bearer.Configure(target, body, scarab, falcon, caseLeft, caseRight, legs.ToArray(), Chain(wingLeft, "wing_l_"), Chain(wingRight, "wing_r_"),
                sun, burning.gameObject, dull.gameObject, blob.transform, line, bulk, headPart, bodyPart, sunPart, renderers.ToArray());

            var marks = BuildKit.Group("Marks", root.transform);
            // Six lanes: the Flares draw five at once in the fury, the Ricochet three. Hot floor is drawn as fire by the
            // Scorch below, not as marks, so it has no pairs of its own any more.
            bearer.ConfigureMarks(Pairs("Lane", 6, marks, marker), Pairs("Circle", 3, marks, marker), Pairs("Fall", 18, marks, marker),
                new MeshFilter[0], new MeshFilter[0], Pairs("Floor", 1, marks, marker), Pairs("Shadow", 1, marks, marker),
                WorldBars.Marker("Roar Ring", marks, 0f, marker));
            bearer.ConfigureAim(Pairs("Aim", 1, marks, marker));
            // Burning ground: eight lanes and eight craters of char, ember cracks and standing flame.
            ScorchBuilder.Build(root.transform, "Scorch", 8, 8, 0);
            // Flames standing on the shell when its own sun is rolled into it.
            ScorchBuilder.Build(root.transform, "Shell Fire", 0, 1, 0);

            // Drawn through whatever is in front of it: where the player is, while the falcon or the sun covers them.
            var finderLight = BuildKit.Glow(BuildKit.GeneratedRoot + "/Enemies", "m_player_finder", new Color(1.6f, 1.45f, 0.9f, 0.95f), 4, false, 2f, 0f, true);
            finderLight.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay;
            var finder = BuildKit.Shape(PrimitiveType.Quad, "Player Finder", root.transform, Vector3.zero, Vector3.one * 2.2f, finderLight);
            finder.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            finder.receiveShadows = false;
            finder.enabled = false;
            var towardCamera = Camera.main != null ? Camera.main.transform.rotation : Quaternion.Euler(55f, 0f, 0f);
            bearer.ConfigureFeedback(finder, WorldBars.FinishMark(root.transform, new Vector3(0f, 4f, 0f), 4f, towardCamera));
            Look(root, sun, burning);
            return bearer;
        }

        // The effects that are not particles (fire_boss_visuals.md 2.3): the sun's light, the stoop's streak and its shock
        // ring. Since the owner's review (2026-11-17) every one of them is fire: flame, smoke and orange-red heat.
        private static void Look(GameObject root, Transform sun, Transform burning)
        {
            string folder = BuildKit.GeneratedRoot + "/Enemies";
            var lightObject = new GameObject("Sun Light");
            // Under the root, not the sun: the sun spins as it rolls, and the light is kept over it by the look.
            lightObject.transform.SetParent(root.transform, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            // Firelight, a deep orange: the court is lit by a fire, not by a lamp.
            light.color = new Color(1f, 0.46f, 0.1f);
            light.range = 9f;
            light.intensity = 0f;
            light.shadows = LightShadows.None;

            // The dive is a fall of flame tearing down through the air, not a streak of gold light.
            var streakLight = BuildKit.Fire(folder, "m_sunform_dive_fire", 1, new Vector2(-2.4f, 0f), 4f);
            var streak = new Renderer[2];
            for (int i = 0; i < streak.Length; i++)
            {
                streak[i] = BuildKit.Shape(PrimitiveType.Quad, "Dive Streak " + (i + 1), root.transform, Vector3.zero, Vector3.one, streakLight);
                streak[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                streak[i].receiveShadows = false;
                streak[i].enabled = false;
            }
            var shockLight = BuildKit.Fire(folder, "m_sunform_shock_fire", 1, new Vector2(1.2f, 0f), 6f);
            var shock = new GameObject("Shock Ring");
            shock.transform.SetParent(root.transform, false);
            var ring = shock.AddComponent<MeshFilter>();
            var ringRenderer = shock.AddComponent<MeshRenderer>();
            ringRenderer.sharedMaterial = shockLight;
            ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ringRenderer.receiveShadows = false;
            ringRenderer.enabled = false;

            // The parry: a trail of fire behind the struck sun, and stars over the stunned form.
            var trail = sun.gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.4f;
            trail.widthMultiplier = 2.4f;
            trail.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.2f));
            trail.minVertexDistance = 0.3f;
            trail.emitting = false;
            // Fire behind it: the trail's alpha falling along it cools it from a yellow heart through red to smoke.
            trail.sharedMaterial = BuildKit.Fire(folder, "m_sunform_trail_fire", 12, new Vector2(-2f, 0f), 3f);
            trail.textureMode = LineTextureMode.Stretch;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.75f, 0.45f), new GradientAlphaKey(0.2f, 1f) });
            trail.colorGradient = fade;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            var starLight = BuildKit.Fire(folder, "m_sunform_star_fire", 5, new Vector2(0f, -0.6f), 2f, 0.88f);
            var stars = new Renderer[5];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = BuildKit.Shape(PrimitiveType.Quad, "Stun Star " + (i + 1), root.transform, Vector3.zero, Vector3.one * 0.7f, starLight);
                stars[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                stars[i].receiveShadows = false;
                stars[i].enabled = false;
            }
            var look = root.AddComponent<SunBearerLook>();
            look.Configure(light, sun, streak, ring, trail, stars);
            // The burning rim: one quad behind the ball, turned to the camera, flames licking outward from under its edge.
            var corona = BuildKit.Shape(PrimitiveType.Quad, "Corona", burning, Vector3.zero, Vector3.one * 5.8f,
                BuildKit.Fire(folder, "m_sunform_corona", 15, new Vector2(0.15f, -0.4f), 5f));
            corona.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            corona.receiveShadows = false;
            look.ConfigureBurning(burning.gameObject, corona.transform, sun.Find("sun_loose").gameObject);
            // The Zenith's blaze: the whole court hotter, orange and fierce while the floor fills and burns.
            var blazeObject = new GameObject("Zenith Blaze Look");
            blazeObject.transform.SetParent(root.transform, false);
            var blaze = blazeObject.AddComponent<UnityEngine.Rendering.Volume>();
            blaze.isGlobal = true;
            blaze.priority = 11f;
            blaze.weight = 0f;
            blaze.sharedProfile = BlazeProfile();
            look.ConfigureBlaze(blaze);
            // One falling spark or feather for each of the eighteen falling marks.
            var fallLight = BuildKit.Fire(folder, "m_sunform_faller_fire", 1, new Vector2(0f, 2f), 3f);
            var fallers = new Renderer[18];
            for (int i = 0; i < fallers.Length; i++)
            {
                fallers[i] = BuildKit.Shape(PrimitiveType.Quad, "Faller " + (i + 1), root.transform, Vector3.zero, Vector3.one, fallLight);
                fallers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                fallers[i].receiveShadows = false;
                fallers[i].enabled = false;
            }
            look.ConfigureFallers(fallers);

            // The burrowing scarab's ridge: a heave of paving, slabs of it tipped up round it, and the cracks it leaves.
            var heave = BuildKit.Shape(PrimitiveType.Sphere, "Ridge Heave", root.transform, Vector3.zero, Vector3.one, SunCityKit.Mud);
            heave.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            heave.enabled = false;
            var slabs = new Renderer[6];
            for (int i = 0; i < slabs.Length; i++)
            {
                slabs[i] = BuildKit.Shape(PrimitiveType.Cube, "Ridge Slab " + (i + 1), root.transform, Vector3.zero, new Vector3(0.9f + 0.15f * (i % 3), 0.16f, 0.7f + 0.1f * (i % 2)), SunCityKit.Stone);
                slabs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                slabs[i].enabled = false;
            }
            // Laid over the floor, not added to it: dark cracks, or ember red with the sun under them (the tint says which).
            var crackLook = BuildKit.Glow(folder, "m_sunform_crack", Color.white, 10, false, 1f);
            var cracks = new Renderer[22];
            for (int i = 0; i < cracks.Length; i++)
            {
                cracks[i] = BuildKit.Shape(PrimitiveType.Quad, "Crack " + (i + 1), root.transform, Vector3.zero, Vector3.one, crackLook);
                cracks[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                cracks[i].receiveShadows = false;
                cracks[i].enabled = false;
            }
            // The Flares: an arc of flame for each line, drawn as a line through the air.
            var arcFire = BuildKit.Fire(folder, "m_sunform_flare_arc", 1, new Vector2(-3f, 0f), 3f);
            var arcs = new LineRenderer[5];
            for (int i = 0; i < arcs.Length; i++)
            {
                var holder = BuildKit.Group("Flare Arc " + (i + 1), root.transform);
                var arc = holder.gameObject.AddComponent<LineRenderer>();
                arc.sharedMaterial = arcFire;
                arc.useWorldSpace = true;
                arc.widthMultiplier = 1f;
                arc.widthCurve = new AnimationCurve(new Keyframe(0f, 0.7f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0.6f));
                arc.textureMode = LineTextureMode.Stretch;
                arc.numCapVertices = 3;
                arc.numCornerVertices = 2;
                arc.alignment = LineAlignment.View;
                arc.positionCount = 0;
                arc.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                arc.receiveShadows = false;
                arc.enabled = false;
                arcs[i] = arc;
            }
            // The Ember Gust: rows of flames driven across the band at its front.
            look.ConfigureGround(heave, slabs, cracks, arcs, ScorchBuilder.Build(root.transform, "Gust Fire", 2, 0, 0));
        }

        // One piece of the model under a holder of its own, at the point the game moves it from. The mesh keeps the
        // turn the import gave it; a boned piece brings its rig with it.
        private static Transform Piece(GameObject model, string modelName, Transform parent, Vector3 at)
        {
            var holder = BuildKit.Group(modelName, parent);
            holder.localPosition = at;
            foreach (string name in new[] { modelName + "_rig", modelName })
            {
                var mesh = BuildKit.FindDeep(model.transform, name);
                if (mesh == null)
                {
                    if (name == modelName) Debug.LogError($"[Clover.Build] The Sun-Bearer's model lacks {modelName}.");
                    continue;
                }
                var turn = mesh.localRotation;
                mesh.SetParent(holder, false);
                mesh.localPosition = Vector3.zero;
                mesh.localRotation = turn;
                mesh.localScale = Vector3.one;
            }
            return holder;
        }

        // The blaze is heat, not whiteness (the owner's review, 2026-11-17): the court goes orange and fierce, a little
        // brighter and redder, with an orange bloom; never washed out to white.
        private static UnityEngine.Rendering.VolumeProfile BlazeProfile()
        {
            string path = BuildKit.GeneratedRoot + "/Enemies/ZenithBlazeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
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
            colour.postExposure.Override(0.15f);
            colour.saturation.Override(14f);
            colour.contrast.Override(8f);
            colour.colorFilter.Override(new Color(1f, 0.76f, 0.56f));
            var bloom = profile.Add<UnityEngine.Rendering.Universal.Bloom>(true);
            bloom.name = "Bloom";
            AssetDatabase.AddObjectToAsset(bloom, profile);
            bloom.intensity.Override(1.5f);
            bloom.threshold.Override(1f);
            bloom.tint.Override(new Color(1f, 0.5f, 0.2f));
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static Transform[] Chain(Transform wing, string prefix)
        {
            var bones = new List<Transform>();
            for (int i = 1; i <= 3; i++)
            {
                var bone = BuildKit.FindDeep(wing, prefix + i);
                if (bone != null) bones.Add(bone);
            }
            if (bones.Count < 3) Debug.LogError($"[Clover.Build] The falcon's wing has no bones {prefix}1..3; it will not beat.");
            return bones.ToArray();
        }

        private static MeshFilter[] Pairs(string name, int count, Transform marks, Material marker)
        {
            var pairs = new MeshFilter[count * 2];
            for (int i = 0; i < count; i++)
            {
                pairs[i * 2] = WorldBars.Marker($"{name} Outline {i + 1}", marks, 0f, marker);
                pairs[i * 2 + 1] = WorldBars.Marker($"{name} Fill {i + 1}", marks, 0f, marker);
            }
            return pairs;
        }
    }
}
