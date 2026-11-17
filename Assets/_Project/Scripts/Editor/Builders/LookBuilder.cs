using Clover.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Clover.EditorTools
{
    /// <summary>
    /// The scene's mood: the lights, the fog, and the post-processing that turns bright colour into glow. The look in
    /// docs/GAME.md is a muted, dark world in which the elements are the only vivid thing, so the grade keeps the scene
    /// cool and low and lets anything brighter than white bloom.
    /// </summary>
    public static class LookBuilder
    {
        private const string Folder = BuildKit.GeneratedRoot + "/Look";

        /// <summary>What a scene's light is like. Colours are as the eye sees them.</summary>
        public struct Mood
        {
            public string Name;
            public Color KeyColour;
            public float KeyIntensity;
            public Vector3 KeyEuler;
            public Color Ambient;
            public Color Fog;
            public float FogDensity;
            public Color Background;
            /// <summary>True for a sky that flashes with lightning.</summary>
            public bool Storm;
            public float Exposure;
            public float Saturation;
        }

        /// <summary>A walled yard under a heavy evening sky: the practice arena.</summary>
        public static Mood Yard => new Mood
        {
            Name = "Yard",
            KeyColour = new Color(0.95f, 0.93f, 1f),
            KeyIntensity = 1.3f,
            KeyEuler = new Vector3(50f, -35f, 0f),
            Ambient = new Color(0.24f, 0.2f, 0.36f),
            Fog = new Color(0.08f, 0.07f, 0.17f),
            FogDensity = 0.012f,
            Background = new Color(0.04f, 0.03f, 0.08f),
            Storm = false,
            Exposure = 0.1f,
            Saturation = 8f,
        };

        /// <summary>Night under the storm: the castle. Dim blue moonlight, deep fog, and the sky flashing.</summary>
        public static Mood StormNight => new Mood
        {
            Name = "StormNight",
            KeyColour = new Color(0.78f, 0.86f, 1f),
            KeyIntensity = 1.2f,
            KeyEuler = new Vector3(48f, -30f, 0f),
            Ambient = new Color(0.2f, 0.17f, 0.33f),
            Fog = new Color(0.07f, 0.06f, 0.17f),
            FogDensity = 0.022f,
            Background = new Color(0.03f, 0.025f, 0.075f),
            Storm = true,
            Exposure = 0.35f,
            Saturation = 8f,
        };

        /// <summary>Above the storm: clear cold moonlight on stone and cloud, the lightning now below and all around.</summary>
        public static Mood Heavens => new Mood
        {
            Name = "Heavens",
            KeyColour = new Color(0.86f, 0.9f, 1f),
            KeyIntensity = 1.35f,
            KeyEuler = new Vector3(52f, -28f, 0f),
            Ambient = new Color(0.27f, 0.25f, 0.44f),
            Fog = new Color(0.2f, 0.2f, 0.4f),
            FogDensity = 0.011f,
            Background = new Color(0.1f, 0.1f, 0.24f),
            Storm = true,
            Exposure = 0.25f,
            Saturation = 10f,
        };

        /// <summary>
        /// White noon in the sun-king's city (docs/design/fire_land.md, section 2): one hard light nearly straight
        /// down, leaning so that every shadow falls north, the way the player walks; short black shadows, a cool
        /// grey-violet in the shade, a pale haze and a sky with no blue in it. A 9 m roof throws its edge 1.9 m north.
        /// </summary>
        public static Mood Noon => new Mood
        {
            Name = "Noon",
            KeyColour = new Color(1f, 0.95f, 0.84f),
            KeyIntensity = 1.5f,
            KeyEuler = new Vector3(NoonSunDegrees, 0f, 0f),
            Ambient = new Color(0.36f, 0.33f, 0.38f),
            Fog = new Color(0.62f, 0.56f, 0.47f),
            FogDensity = 0.012f,
            Background = new Color(0.78f, 0.74f, 0.66f),
            Storm = false,
            Exposure = 0.15f,
            Saturation = -10f,
        };

        /// <summary>Degrees the noon sun stands above the ground. Shadows fall north, their length the caster's height divided by the tangent of this.</summary>
        public const float NoonSunDegrees = 78f;

        /// <summary>Metres north of its caster's edge that a noon shadow reaches, for a caster this tall.</summary>
        public static float NoonShadowLength(float height) => height / Mathf.Tan(NoonSunDegrees * Mathf.Deg2Rad);

        /// <summary>The place outside the hours: a still, dim, warm light on pale stone, and black all round.</summary>
        public static Mood Stillness => new Mood
        {
            Name = "Stillness",
            KeyColour = new Color(0.9f, 0.9f, 1f),
            KeyIntensity = 1.1f,
            KeyEuler = new Vector3(55f, -25f, 0f),
            Ambient = new Color(0.2f, 0.18f, 0.32f),
            Fog = new Color(0.05f, 0.04f, 0.11f),
            FogDensity = 0.016f,
            Background = new Color(0.02f, 0.015f, 0.05f),
            Storm = false,
            Exposure = 0.25f,
            Saturation = -4f,
        };

        /// <summary>
        /// Sets how the key light's shadows are drawn, on the render pipeline asset the game runs with. The camera looks
        /// down from about twelve metres and sees ten to twenty metres of ground (thirty-odd when it stands back for the
        /// colossus), so the shadow map is spent on that and no further: a larger map, cut into four parts that are
        /// finest where the fight is, filtered soft. Unity writes the asset; nothing here is typed into it by hand.
        /// </summary>
        [MenuItem("Clover/Build/Shadow Quality")]
        public static void ApplyShadowQuality()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
            {
                Debug.LogWarning("[Clover.Build] No URP asset is in use; shadow quality was not set.");
                return;
            }
            var asset = new SerializedObject(pipeline);
            bool changed = false;
            void Whole(string name, int value)
            {
                var property = asset.FindProperty(name);
                if (property == null) Debug.LogWarning($"[Clover.Build] The URP asset has no setting named {name}.");
                else if (property.intValue != value) { property.intValue = value; changed = true; }
            }
            void Number(string name, float value)
            {
                var property = asset.FindProperty(name);
                if (property == null) Debug.LogWarning($"[Clover.Build] The URP asset has no setting named {name}.");
                else if (!Mathf.Approximately(property.floatValue, value)) { property.floatValue = value; changed = true; }
            }
            Whole("m_MainLightShadowmapResolution", 4096);
            Number("m_ShadowDistance", ShadowDistance);
            Whole("m_ShadowCascadeCount", 4);
            var splits = asset.FindProperty("m_Cascade4Split");
            if (splits != null && splits.vector3Value != ShadowSplits) { splits.vector3Value = ShadowSplits; changed = true; }
            Number("m_CascadeBorder", 0.15f);
            // Finer texels need less pushing off the surface; too much and a shadow leaves the foot of what casts it.
            Number("m_ShadowDepthBias", 0.5f);
            Number("m_ShadowNormalBias", 0.4f);
            var soft = asset.FindProperty("m_SoftShadowsSupported");
            if (soft != null && !soft.boolValue) { soft.boolValue = true; changed = true; }
            // 3 is the widest of URP's three soft filters.
            Whole("m_SoftShadowQuality", 3);
            if (!changed) return;
            asset.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssetIfDirty(pipeline);
            Debug.Log($"[Clover.Build] Shadow quality set on {AssetDatabase.GetAssetPath(pipeline)}.");
        }

        private const float ShadowDistance = 44f;
        // Where the four parts end, as shares of the distance: 16, 24 and 34 metres from the camera. The first holds the whole usual view.
        private static readonly Vector3 ShadowSplits = new Vector3(0.364f, 0.545f, 0.773f);

        /// <summary>Adds the lights, the fog and the post-processing volume to the open scene, and sets the camera to use them.</summary>
        public static StormLight Apply(Mood mood, Camera camera)
        {
            ApplyShadowQuality();
            var root = BuildKit.Group("Look");

            var keyObject = new GameObject("Key Light");
            keyObject.transform.SetParent(root, false);
            var key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = mood.KeyColour;
            key.intensity = mood.KeyIntensity;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.9f;
            keyObject.transform.rotation = Quaternion.Euler(mood.KeyEuler);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = mood.Ambient;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = mood.Fog;
            RenderSettings.fogDensity = mood.FogDensity;
            RenderSettings.sun = key;

            StormLight storm = null;
            if (mood.Storm)
            {
                // A second light that is dark except in a flash. It comes from the other side, so a flash throws the
                // scene's shapes into a different relief for an instant.
                var flashObject = new GameObject("Storm Flash");
                flashObject.transform.SetParent(root, false);
                var flash = flashObject.AddComponent<Light>();
                flash.type = LightType.Directional;
                flash.color = new Color(0.8f, 0.88f, 1f);
                flash.intensity = 0f;
                flash.shadows = LightShadows.None;
                flashObject.transform.rotation = Quaternion.Euler(62f, 140f, 0f);
                storm = flashObject.AddComponent<StormLight>();
                storm.Configure(flash);
            }

            var volumeObject = new GameObject("Post Processing");
            volumeObject.transform.SetParent(root, false);
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = Profile(mood);

            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = mood.Background;
                camera.allowHDR = true;
                var data = camera.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                data.antialiasingQuality = AntialiasingQuality.High;
            }
            return storm;
        }

        private static VolumeProfile Profile(Mood mood)
        {
            BuildKit.EnsureFolder(Folder);
            string path = $"{Folder}/{mood.Name}Profile.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var bloom = Add<Bloom>(profile);
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(1.1f);
            bloom.scatter.Override(0.72f);
            bloom.highQualityFiltering.Override(true);

            var tone = Add<Tonemapping>(profile);
            tone.mode.Override(TonemappingMode.Neutral);

            var colour = Add<ColorAdjustments>(profile);
            colour.postExposure.Override(mood.Exposure);
            colour.contrast.Override(20f);
            colour.saturation.Override(mood.Saturation);

            // Shadows lean cold and highlights a touch warm, which keeps the dark from going flat grey.
            var split = Add<SplitToning>(profile);
            split.shadows.Override(new Color(0.43f, 0.4f, 0.62f));
            split.highlights.Override(new Color(0.54f, 0.52f, 0.5f));
            split.balance.Override(-10f);

            var vignette = Add<Vignette>(profile);
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.45f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var component = profile.Add<T>(true);
            component.name = typeof(T).Name;
            // A profile's settings are separate objects that must be saved inside its asset.
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
