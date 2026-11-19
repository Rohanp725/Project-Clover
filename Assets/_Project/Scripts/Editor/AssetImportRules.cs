using System.Collections.Generic;
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
    /// lights, read/write off, materials in the prefab. Characters: a generic rig, clips named after the part of the take
    /// name behind the bar, looping when the clip is idle, run, walk or ends in _loop, imported materials kept only
    /// for their names (builders swap in the toon materials by name), and smoothed normals stored in the tangents for the outline. Audio: streaming Vorbis for music and ambience, ADPCM for short sounds.
    /// </summary>
    public class AssetImportRules : AssetPostprocessor
    {
        public const string ArtDir = "Assets/_Project/Art/";
        public const string TexturesDir = ArtDir + "Textures/";
        public const string ModelsDir = ArtDir + "Models/";
        public const string CharactersDir = ModelsDir + "Characters/";
        public const string WeaponsDir = ModelsDir + "Weapons/";
        public const string VfxDir = ArtDir + "VFX/";
        public const string VfxCombosDir = VfxDir + "Combos/";
        public const string VfxMasksDir = "Assets/_Project/Generated/Vfx/Element/";
        public const string ReactionMasksDir = "Assets/_Project/Generated/Vfx/Reaction/";
        public const string AudioDir = "Assets/_Project/Audio/";
        public const int MaxTextureSize = 2048;
        public const int MaxUiTextureSize = 1024;

        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith(VfxDir) || assetPath.StartsWith(VfxMasksDir) || assetPath.StartsWith(ReactionMasksDir))
            {
                PreprocessEffectTexture((TextureImporter)assetImporter);
                return;
            }
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

        /// <summary>
        /// The elements' effect sheets (Art/VFX/<Element>/*_flipbook.png: 4x4 frames of hard cel bands with alpha) and the
        /// floor-mark masks baked from the kits (Generated/Vfx/Element/*_mask.png). Both are seen small and must keep
        /// their hard edges: no mipmaps (a blurred mip turns a band edge into a soft glow), bilinear, clamped so a frame
        /// never bleeds into its neighbour. Sheets are sRGB colour; masks are linear data, one shape per channel.
        /// </summary>
        private void PreprocessEffectTexture(TextureImporter importer)
        {
            string name = Path.GetFileNameWithoutExtension(assetPath);
            WarnName(name);
            bool mask = name.ToLowerInvariant().EndsWith("_mask");
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = !mask;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = !mask;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 0;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;   // BC7 on PC keeps the alpha edges
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
            if (assetPath.StartsWith(VfxDir))
            {
                // The elements' effect kits: many small pieces in one file. They are read once by the element-art builder,
                // which bakes normalised copies and floor masks from them, so they must be readable; they carry no rig,
                // animation, camera or light, and their imported materials are kept only for their names.
                var kit = (ModelImporter)assetImporter;
                WarnName(Path.GetFileNameWithoutExtension(assetPath));
                kit.globalScale = 1f;
                kit.useFileUnits = true;
                kit.importCameras = false;
                kit.importLights = false;
                kit.importAnimation = false;
                kit.animationType = ModelImporterAnimationType.None;
                kit.importBlendShapes = false;
                kit.isReadable = true;
                kit.meshCompression = ModelImporterMeshCompression.Off;
                kit.weldVertices = true;
                kit.importNormals = ModelImporterNormals.Import;
                kit.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                kit.materialLocation = ModelImporterMaterialLocation.InPrefab;
                kit.generateSecondaryUV = false;
                // The reactions' kits are placed by a recipe written in Unity's axes on the assumption that each piece's
                // own axes are Unity's (combos_recipe.json, "axes"): the axis turn is baked into the meshes.
                kit.bakeAxisConversion = assetPath.StartsWith(VfxCombosDir);
                return;
            }
            if (!assetPath.StartsWith(ModelsDir)) return;
            var importer = (ModelImporter)assetImporter;
            WarnName(Path.GetFileNameWithoutExtension(assetPath));
            importer.globalScale = 1f;
            importer.useFileUnits = true;
            importer.importCameras = false;
            importer.importLights = false;
            // The Fire land's kits are read when a scene opens: their pieces are merged into a few meshes (MergedScenery).
            importer.isReadable = assetPath.StartsWith(ModelsDir + "Environment/fire_") || assetPath.StartsWith(ModelsDir + "Environment/tomb_");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.weldVertices = true;
            importer.importBlendShapes = assetPath.Contains("/Characters/");
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.generateSecondaryUV = false;
            // The weapon files are exported without Apply Transform: baking the axes keeps each one's business end along
            // +Z once WeaponBuilder has turned it about, as the stand-ins and every pose take it to be.
            if (assetPath.StartsWith(WeaponsDir)) importer.bakeAxisConversion = true;

            if (!assetPath.StartsWith(CharactersDir)) return;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = true;
            // The imported materials are placeholders: builders read their names to know which toon material goes where.
            // The outline pass needs one direction per point, not per face; it is written into the tangents after import.
            importer.importTangents = ModelImporterTangents.None;
            importer.weldVertices = false;
        }

        private void OnPreprocessAnimation()
        {
            if (!assetPath.StartsWith(CharactersDir)) return;
            var importer = (ModelImporter)assetImporter;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                // Blender names takes "rig|action"; the game wants the action.
                int bar = clip.name.LastIndexOf('|');
                if (bar >= 0) clip.name = clip.name.Substring(bar + 1);
                clip.loopTime = IsLoopingClip(clip.name);
            }
            importer.clipAnimations = clips;
        }

        public static bool IsLoopingClip(string name) =>
            name == "idle" || name == "run" || name == "walk" || name.EndsWith("_loop");

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(CharactersDir)) return;
            foreach (var skinned in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) StoreSmoothNormals(skinned.sharedMesh);
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true)) StoreSmoothNormals(filter.sharedMesh);
        }

        /// <summary>
        /// Averages the normals of all vertices that share a position and stores the result in the tangents. Hard-edged
        /// meshes have several normals per point, and an outline pushed along those splits open at every edge; pushed
        /// along the average it stays closed. Tangents are used because skinning carries them along with the pose.
        /// </summary>
        public static void StoreSmoothNormals(Mesh mesh)
        {
            if (mesh == null) return;
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            if (normals.Length != vertices.Length) return;
            var sums = new Dictionary<Vector3Int, Vector3>();
            var keys = new Vector3Int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                // A tenth of a millimetre is far below anything modelled, and well above export rounding.
                keys[i] = Vector3Int.RoundToInt(vertices[i] * 10000f);
                sums.TryGetValue(keys[i], out var sum);
                sums[keys[i]] = sum + normals[i];
            }
            var tangents = new Vector4[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 smooth = sums[keys[i]].normalized;
                if (smooth == Vector3.zero) smooth = normals[i];
                tangents[i] = new Vector4(smooth.x, smooth.y, smooth.z, 1f);
            }
            mesh.tangents = tangents;
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
