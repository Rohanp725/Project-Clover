using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Clover.EditorTools
{
    /// <summary>
    /// The editor step for the stand-in art of 2026-11-22 (standin_clearance_plan.md passes 11 and 12): brings the Blender
    /// models from staging into Art once the owner has approved them, builds a prefab for each with its toon materials,
    /// collider and NavMesh carve, and wires the standing things' prefabs into the open scene's StandingLibrary. The runtime
    /// side (World.StandingThings, Player.Forms.CreatureForms) works without them, showing its own stand-in shapes.
    /// Order: Intake Staged Models (the owner's word first), Build Standing Prefabs, Wire Standing Library In Open Scene.
    /// </summary>
    public static class StagedArtBuilder
    {
        public const string StagingFolder = "staging/blender/2026-11-22_standin_art";
        public const string ModelFolder = "Assets/_Project/Art/Models/Standing";
        public const string MaterialFolder = "Assets/_Project/Art/Materials/Standing";
        public const string PrefabFolder = "Assets/_Project/Generated/Standing";

        /// <summary>The models in the staging folder, each a subfolder with its FBX and notes.md.</summary>
        public static readonly string[] Models = { "anvil_stone", "rimed_mound", "hollow_trunk", "ice_wall", "stone_pillar", "cobra", "river_serpent", "stone_oak" };

        /// <summary>Each material slot's lit colour in linear RGB, from the models' notes.md (the toon material takes it as the eye sees it).</summary>
        public static readonly Dictionary<string, Color> SlotColours = new Dictionary<string, Color>
        {
            { "m_anvil_stone_stone", new Color(0.36f, 0.34f, 0.30f) }, { "m_anvil_stone_lichen", new Color(0.33f, 0.36f, 0.25f) },
            { "m_anvil_stone_iron", new Color(0.13f, 0.14f, 0.17f) }, { "m_anvil_stone_steel", new Color(0.40f, 0.43f, 0.48f) },
            { "m_rimed_mound_soil", new Color(0.25f, 0.19f, 0.13f) }, { "m_rimed_mound_rime", new Color(0.60f, 0.67f, 0.72f) },
            { "m_rimed_mound_ice", new Color(0.42f, 0.74f, 0.84f) },
            { "m_hollow_trunk_bark", new Color(0.17f, 0.13f, 0.10f) }, { "m_hollow_trunk_wood", new Color(0.55f, 0.47f, 0.36f) },
            { "m_hollow_trunk_hollow", new Color(0.07f, 0.03f, 0.09f) }, { "m_hollow_trunk_moss", new Color(0.24f, 0.42f, 0.16f) },
            { "m_hollow_trunk_rot", new Color(0.30f, 0.12f, 0.38f) }, { "m_hollow_trunk_root", new Color(0.34f, 0.23f, 0.15f) },
            { "m_ice_wall_ice", new Color(0.40f, 0.70f, 0.82f) }, { "m_ice_wall_deep", new Color(0.20f, 0.42f, 0.58f) },
            { "m_ice_wall_rime", new Color(0.62f, 0.69f, 0.74f) }, { "m_ice_wall_crack", new Color(0.05f, 0.13f, 0.22f) },
            { "m_stone_pillar_stone", new Color(0.38f, 0.36f, 0.32f) }, { "m_stone_pillar_dark", new Color(0.25f, 0.24f, 0.22f) },
            { "m_stone_pillar_ochre", new Color(0.66f, 0.44f, 0.16f) }, { "m_stone_pillar_earth", new Color(0.24f, 0.18f, 0.12f) },
            { "m_cobra_scales", new Color(0.78f, 0.22f, 0.04f) }, { "m_cobra_belly", new Color(0.98f, 0.62f, 0.12f) },
            { "m_cobra_mark", new Color(0.38f, 0.05f, 0.02f) }, { "m_cobra_flame", new Color(1.0f, 0.78f, 0.22f) }, { "m_cobra_eye", new Color(0.06f, 0.02f, 0.01f) },
            { "m_river_serpent_deep", new Color(0.05f, 0.18f, 0.44f) }, { "m_river_serpent_water", new Color(0.10f, 0.48f, 0.70f) },
            { "m_river_serpent_foam", new Color(0.58f, 0.84f, 0.90f) }, { "m_river_serpent_eye", new Color(0.55f, 0.95f, 1.0f) },
            { "m_river_serpent_mouth", new Color(0.02f, 0.07f, 0.18f) },
            { "m_stone_oak_stone", new Color(0.40f, 0.39f, 0.35f) }, { "m_stone_oak_lichen", new Color(0.36f, 0.40f, 0.26f) },
            { "m_stone_oak_leaf", new Color(0.20f, 0.50f, 0.14f) }, { "m_stone_oak_leaf_light", new Color(0.42f, 0.68f, 0.20f) },
            { "m_stone_oak_gold", new Color(0.80f, 0.58f, 0.14f) }, { "m_stone_oak_ochre", new Color(0.62f, 0.42f, 0.16f) },
        };

        /// <summary>
        /// Copies each staged model not yet in Art into <see cref="ModelFolder"/>, bakes its axes, and writes its manifest row
        /// with approved_by "pending". For the owner's word only: nothing enters Art before the owner has looked at the sheets.
        /// </summary>
        [MenuItem("Clover/Standing Things/Intake Staged Models")]
        public static void IntakeStaged()
        {
            BuildKit.EnsureFolder(ModelFolder);
            const string manifest = "Assets/_Project/ASSETS.csv";
            string rows = File.ReadAllText(manifest);
            string ending = rows.Contains("\r\n") ? "\r\n" : "\n";
            int taken = 0;
            foreach (string model in Models)
            {
                string source = $"{StagingFolder}/{model}/{model}.fbx", target = $"{ModelFolder}/{model}.fbx";
                if (File.Exists(target)) continue;
                if (!File.Exists(source))
                {
                    Debug.LogWarning($"[Clover.Build] No staged model at {source}.");
                    continue;
                }
                File.Copy(source, target, true);
                AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceUpdate);
                if (AssetImporter.GetAtPath(target) is ModelImporter importer)
                {
                    // Exported with -Z forward and Y up: baked so the model's front is Unity's +Z, as the notes give the facings.
                    importer.bakeAxisConversion = true;
                    importer.SaveAndReimport();
                }
                if (!rows.Contains(target + ","))
                {
                    if (!rows.EndsWith("\n")) rows += ending;
                    rows += $"{target},model,blender,own,Floopbob Studios,\"stand-in art for passes 11 and 12; made in Blender 5.3 through the Blender MCP connection on 2026-11-22; source file {StagingFolder}/standin_art.blend, notes {StagingFolder}/{model}/notes.md\",-,{DateTime.Now:yyyy-MM-dd},pending{ending}";
                }
                taken++;
            }
            File.WriteAllText(manifest, rows);
            AssetDatabase.Refresh();
            Debug.Log($"[Clover.Build] Took {taken} staged stand-in models into {ModelFolder}.");
        }

        /// <summary>Builds a prefab for each standing kind from its imported model: StandingThingBody, its pieces, collider and carve.</summary>
        [MenuItem("Clover/Standing Things/Build Standing Prefabs")]
        public static void BuildStandingPrefabs()
        {
            BuildKit.EnsureFolder(PrefabFolder);
            int built = 0;
            foreach (StandingKind kind in Enum.GetValues(typeof(StandingKind)))
                if (StandingPrefab(kind) != null) built++;
            AssetDatabase.SaveAssets();
            Debug.Log($"[Clover.Build] Built {built} standing-thing prefabs under {PrefabFolder}.");
        }

        /// <summary>The prefab of a standing kind, built from its model; null when the model has not been taken in.</summary>
        public static StandingThingBody StandingPrefab(StandingKind kind)
        {
            var shape = StandingShape.For(kind);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelFolder}/{shape.Model}.fbx");
            if (source == null)
            {
                Debug.LogWarning($"[Clover.Build] {kind}: no model at {ModelFolder}/{shape.Model}.fbx; run Intake Staged Models first.");
                return null;
            }
            var root = new GameObject(kind.ToString());
            var model = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            // One file holds both walls: only this kind's whole and cracked objects are kept, with the shared shards.
            var keep = new HashSet<string> { shape.Whole, shape.Cracked };
            Transform whole = null, cracked = null;
            var pieces = new List<Transform>();
            foreach (var part in model.GetComponentsInChildren<Transform>(true).ToArray())
            {
                if (part == model.transform || part == null) continue;
                string name = part.name;
                if (name == shape.Whole) whole = part;
                else if (name == shape.Cracked && shape.Cracked.Length > 0) cracked = part;
                else if (name.Contains("_chunk_") || name.Contains("_shard_")) pieces.Add(part);
                else if (name.EndsWith("_lod0") && !keep.Contains(name)) UnityEngine.Object.DestroyImmediate(part.gameObject);
            }
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = renderer.sharedMaterials.Select(slot => SlotMaterial(slot != null ? slot.name : "")).ToArray();
            var body = root.AddComponent<StandingThingBody>();
            body.Kind = kind;
            body.Whole = whole != null ? whole.gameObject : null;
            body.Cracked = cracked != null ? cracked.gameObject : null;
            body.Pieces = pieces.ToArray();
            // The collider and the carve from its notes (StandingShape.For); enemies also walk round it by its footprint.
            var obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            if (shape.Box)
            {
                var box = root.AddComponent<BoxCollider>();
                box.size = shape.Size;
                box.center = Vector3.up * shape.CentreUp;
                obstacle.shape = NavMeshObstacleShape.Box;
                obstacle.size = shape.Size;
                obstacle.center = box.center;
            }
            else
            {
                var capsule = root.AddComponent<CapsuleCollider>();
                capsule.radius = shape.Size.x;
                capsule.height = Mathf.Max(shape.Size.y, shape.Size.x * 2f);
                capsule.center = Vector3.up * shape.CentreUp;
                obstacle.shape = NavMeshObstacleShape.Capsule;
                obstacle.radius = shape.Carve > 0f ? shape.Carve : shape.Size.x;
                obstacle.height = capsule.height;
                obstacle.center = capsule.center;
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{kind}.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab != null ? prefab.GetComponent<StandingThingBody>() : null;
        }

        /// <summary>Puts a StandingLibrary in the open scene (or updates the one there) with every built standing prefab.</summary>
        [MenuItem("Clover/Standing Things/Wire Standing Library In Open Scene")]
        public static void WireLibrary()
        {
            var library = UnityEngine.Object.FindAnyObjectByType<StandingLibrary>();
            if (library == null) library = new GameObject("Standing Library").AddComponent<StandingLibrary>();
            var entries = new List<StandingLibrary.Entry>();
            foreach (StandingKind kind in Enum.GetValues(typeof(StandingKind)))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<StandingThingBody>($"{PrefabFolder}/{kind}.prefab");
                if (prefab != null) entries.Add(new StandingLibrary.Entry { Kind = kind, Prefab = prefab });
            }
            library.Prefabs = entries.ToArray();
            EditorUtility.SetDirty(library);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(library.gameObject.scene);
            Debug.Log($"[Clover.Build] Standing Library in {library.gameObject.scene.name} wired with {entries.Count} prefabs.");
        }

        /// <summary>The toon material for an imported slot name ("m_anvil_stone_iron", perhaps with a ".001" after it).</summary>
        public static Material SlotMaterial(string slot)
        {
            int dot = slot.IndexOf('.');
            if (dot > 0) slot = slot.Substring(0, dot);
            BuildKit.EnsureFolder(MaterialFolder);
            // The notes give the lit band in linear colour; the toon material takes the colour as the eye sees it.
            var colour = SlotColours.TryGetValue(slot, out var linear) ? linear.gamma : new Color(0.5f, 0.5f, 0.5f);
            return BuildKit.Toon(MaterialFolder, string.IsNullOrEmpty(slot) ? "m_standing_plain" : slot, colour);
        }
    }
}
