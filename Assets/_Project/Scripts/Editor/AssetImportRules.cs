using System.IO;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Import settings by folder and name, applied when a file arrives, so a texture, model or clip cannot come in wrong by
    /// accident. When a kind of asset needs something else, change the rule here; never fix single assets by hand.
    /// Textures: the suffix says what the map is (_albedo/_color sRGB; _normal a normal map; _mask/_rough/_metal/_ao/_height
    /// linear; _emissive sRGB); textures under Art/Textures/UI are sprites without mipmaps. Models: metres, no cameras or
    /// lights, read/write off, materials in the prefab. Audio: streaming Vorbis for music and ambience, ADPCM for short sounds.
    /// </summary>
    public class AssetImportRules : AssetPostprocessor
    {
        public const string ArtDir = "Assets/_Project/Art/";
        public const string TexturesDir = ArtDir + "Textures/";
        public const string ModelsDir = ArtDir + "Models/";
        public const string AudioDir = "Assets/_Project/Audio/";
        public const int MaxTextureSize = 2048;
        public const int MaxUiTextureSize = 1024;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(TexturesDir)) return;
            var importer = (TextureImporter)assetImporter;
            string name = Path.GetFileNameWithoutExtension(assetPath);
            WarnName(name);
            name = name.ToLowerInvariant();

            if (assetPath.StartsWith(TexturesDir + "UI/"))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                if (importer.maxTextureSize > MaxUiTextureSize) importer.maxTextureSize = MaxUiTextureSize;
                return;
            }

            bool normal = name.EndsWith("_normal") || name.EndsWith("_n");
            bool linear = normal || name.EndsWith("_mask") || name.EndsWith("_rough") || name.EndsWith("_metal")
                          || name.EndsWith("_ao") || name.EndsWith("_height") || name.EndsWith("_m") || name.EndsWith("_r");
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !linear;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = true;
            importer.anisoLevel = 4;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;   // BC7 on PC
            if (importer.maxTextureSize > MaxTextureSize) importer.maxTextureSize = MaxTextureSize;
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            if (!assetPath.StartsWith(TexturesDir)) return;
            if (!Mathf.IsPowerOfTwo(texture.width) || !Mathf.IsPowerOfTwo(texture.height))
                Debug.LogWarning($"[Clover.Import] {assetPath} is {texture.width}x{texture.height}; textures are powers of two so mipmaps and compression work.");
        }

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ModelsDir)) return;
            var importer = (ModelImporter)assetImporter;
            WarnName(Path.GetFileNameWithoutExtension(assetPath));
            importer.globalScale = 1f;
            importer.useFileUnits = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.weldVertices = true;
            importer.importBlendShapes = assetPath.Contains("/Characters/");
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.generateSecondaryUV = false;
        }

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(AudioDir)) return;
            var importer = (AudioImporter)assetImporter;
            WarnName(Path.GetFileNameWithoutExtension(assetPath));
            var s = importer.defaultSampleSettings;
            if (assetPath.StartsWith(AudioDir + "Music/") || assetPath.StartsWith(AudioDir + "Ambience/"))
            {
                s.loadType = AudioClipLoadType.Streaming;
                s.compressionFormat = AudioCompressionFormat.Vorbis;
                s.quality = 0.7f;
                importer.forceToMono = false;
            }
            else if (assetPath.StartsWith(AudioDir + "Voice/"))
            {
                s.loadType = AudioClipLoadType.CompressedInMemory;
                s.compressionFormat = AudioCompressionFormat.Vorbis;
                s.quality = 0.6f;
                importer.forceToMono = true;
            }
            else
            {
                // Short sounds: cheap to start, a little memory each. 3D sound effects are mono; UI stays as authored.
                s.loadType = AudioClipLoadType.DecompressOnLoad;
                s.compressionFormat = AudioCompressionFormat.ADPCM;
                importer.forceToMono = assetPath.StartsWith(AudioDir + "SFX/");
            }
            s.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = s;
            importer.loadInBackground = s.loadType != AudioClipLoadType.DecompressOnLoad;
        }

        private static void WarnName(string name)
        {
            if (name != name.ToLowerInvariant() || name.Contains(" ") || name.Contains("-"))
                Debug.LogWarning($"[Clover.Import] '{name}': asset names are lowercase snake_case (thing_variant01_albedo) so scripts and the manifest find them.");
        }
    }
}
