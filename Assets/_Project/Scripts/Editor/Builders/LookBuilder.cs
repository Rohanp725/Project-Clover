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

        /// <summary>Adds the lights, the fog and the post-processing volume to the open scene, and sets the camera to use them.</summary>
        public static StormLight Apply(Mood mood, Camera camera)
        {
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
