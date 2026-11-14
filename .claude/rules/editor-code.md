---
paths:
  - "Assets/_Project/Scripts/Editor/**"
  - "AgentScripts/**"
---

# Editor tools, builders and agent scripts

- Namespace `Clover.EditorTools`, assembly `Clover.Editor`, menu root `Clover/<Area>/<Verb Noun>`. Every menu item is a
  `public static void` method with no parameters so it can also run through `unity command editor invoke_menu -- --path "Clover/..."`
  or `script eval "Clover.EditorTools.X.Y();"`.
- Tools write their findings to `review/<area>/<name>.md` (and `.json` when a machine reads it back), then one `Debug.Log`
  line with the counts and the path. Claude reads the file, not the console.
- Builders are idempotent: a rebuild deletes what it generated last time (by stable name under `Assets/_Project/Generated/`
  or a marker component) and creates it again. Generated assets keep stable names so references survive; when a generator
  changes and the asset "didn't take", bump the name.
- Create assets through the API (`AssetDatabase.CreateAsset`, `PrefabUtility.SaveAsPrefabAsset`, `ScriptableObject.CreateInstance`,
  `AssetDatabase.CreateFolder`), inside `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` for batches, then
  `SaveAssets` and `Refresh`. Never write YAML as text.
- Long work (hundreds of assets, bakes, builds) reports progress with `Progress.Start`/`Progress.Report` and must not block a
  `script eval` call: run it from a menu method or `script run` entry point and poll a status file or the console.
- Import rules live in `AssetImportRules.cs` by folder and name suffix. A new kind of asset means a new rule, not a hand-set
  importer.
- Loudness targets per folder are in `Loudness.cs`; review stations and golden thresholds in `ShotStations.cs`;
  convention checks in `ProjectChecks.cs`. Extend those rather than adding a parallel tool.
- `AgentScripts/*.cs` are outside `Assets/`, compiled in memory by `unity command script run --file ... --entry Type.Method`.
  They may reference project assemblies and `UnityEditor`. Keep each one a static class with one entry point, delete it when
  the job is done, or move it into `Scripts/Editor` as a proper tool once it is worth keeping.
- Screenshots in edit mode come from `ShotStations.Render`; in play mode from `unity command capture game_view`. Never read
  the desktop screen.
