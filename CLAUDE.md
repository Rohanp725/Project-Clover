# Project Clover

A Unity game by Floopbob Studios, at the start of development. What the game is, its pillars and its look live in
`docs/GAME.md`: read it before designing anything and write to it when the user decides something. Unity **6000.6.5f1**,
URP 17.6, Input System 1.20, linear colour space, domain reload off, Windows 11. Scenes and prefabs are built by code where
practical (builders under `Assets/_Project/Scripts/Editor/Builders`), so nothing hand-placed is irreplaceable.

Path-scoped conventions live in `.claude/rules/`. Repeatable procedures are skills in `.claude/skills/`. The workflow, the
tools it uses and why each was chosen are in `docs/WORKFLOW.md` and `docs/TOOLS.md`.

## Non-negotiable rules

- **Never hand-edit Unity's serialized files**: `.unity`, `.prefab`, `.meta`, `.mat`, `.anim`, `.controller`, and `.asset`
  under `Assets/`. Settings deny Edit/Write on them and a hook blocks shell writes. Change them through the Unity CLI
  (`unity command ...`) or editor code so Unity writes the YAML and the `.meta` itself.
- **Verification is the definition of done**: compile, read the console, run the tests, look at a screenshot, and read the
  profiler for anything that touches performance. The `unity-check` skill is the procedure. Report what the tools said.
- **New Input System only** (`UnityEngine.InputSystem`; actions in `Assets/_Project/Settings/Controls.inputactions`).
  **URP only**: no Built-in RP APIs, no `OnRenderImage`, no legacy post-processing stack.
- **Domain reload is OFF**: every mutable static gets a reset in a
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` method.
- **Assemblies**: runtime code in `Clover.Runtime`, editor code in `Clover.Editor`, tests in `Clover.Tests.EditMode` and
  `Clover.Tests.PlayMode`. `using UnityEditor` never appears in the runtime assembly. Legacy `Input.*` never appears anywhere.
- **Assets have provenance**: every file under `Art/` and `Audio/` has a row in `Assets/_Project/ASSETS.csv` (source,
  licence, prompt and seed when generated). Generated or downloaded files land in `staging/` first; only approved files
  enter `Assets/`, through `unity command assets import` or the asset-intake skill.
- **Import settings come from `AssetImportRules.cs`**, by folder and name suffix. Fix the rule, not the asset.
- **Audio** has a loudness target per folder in `Loudness.cs`; `Clover > Audio > Loudness Report` lists the clips that miss it.
- **Visuals** have reference frames in `review/shots/golden/`; `Clover > Review > Capture Stations` re-shoots every
  `ShotStation` and writes which ones changed. A golden image is replaced only when the user accepts the new look.
- **Text the player reads** at their own pace is click-through; timed messages are for status lines and scares only.
- No paid tools, accounts or sign-ups. Pin every external package to an exact version. Servers bind to localhost only.
- Some files are CRLF. Patch large files with a Python script run as `python -I script.py <path> </dev/null` that keeps
  the file's own endings; never let a shell command wait on stdin.

## Driving Unity

One bridge: the **Unity CLI** (`unity` on PATH; Pipeline package 1.0.0 in the project; editor server on 7800-7849,
localhost only). MCP for Unity is not installed here: the CLI covers scene, asset, script, test, capture and profiler work.
`unity command` lists every editor command; `unity command --query <word>` searches them.

- Health: `unity status`; `unity command editor status`; `unity command console status` (carries `compilationFailed`).
- Compile: `unity --json recompile --timeout 120` blocks and returns errors with file and line. Or
  `unity command script recompile` then poll `unity command script recompile_status`.
- Console: `unity command console get -- --tail 30 --level error`; `unity command console clear` first for only new entries.
- Tests: `unity command test run -- --mode editor --filter <Fixture>`; PlayMode only as
  `test run -- --mode playmode --async-tests true` followed by polling `test status`; `test list` shows names.
- Look: `unity command capture game_view -- --output-path review/shots/current/<name>.png --width 1280 --height 720`,
  then Read the PNG. Also `capture scene_view` and `capture editor_window`.
- Play: `editor set_auto_tick` first, then `editor.playmode start`, then `unity status --until-ready`; prove frames advance
  with `job wait -- --condition '{"member":"UnityEngine.Time.frameCount","op":"changed"}' --timeout 10`; drive with
  `runtime.input simulate_key` and `simulate_pointer`; finish with `editor.playmode stop`. The `playmode-probe` skill is the loop.
- Code in the editor: one-liners with `script eval "return ...;"`. Anything longer is a file under `AgentScripts/` run with
  `script run --file AgentScripts/X.cs --entry X.Run` (in-memory compile, no domain reload). Menus:
  `editor invoke_menu -- --path "Clover/..."`.
- Assets and scenes: `assets import`, `assets set_import_settings`, `assets find`, `prefab create`, `gameobject create`,
  `component set_field`, `material set_properties`; `job batch` for many operations in one undo step.
- Audit and performance: `diagnostics run_audit` then `audit_status`; `diagnostics profiler_record` then `profiler_summary`.
- Flags meant for the editor command go after a `--` fence so the CLI's own `--timeout` does not swallow them.
  Exit 2 means fix the command line. Exit 6 means the editor failed or is unreachable: check `unity status`, do not rewrite.
- Never run `Unity.exe -batchmode` while the editor is open. Compile errors present at editor start put it in Safe Mode,
  where the Pipeline does not load; `unity status` then lists the errors, fix them in the files.

## Where things are

| What | Where |
|---|---|
| Runtime code | `Assets/_Project/Scripts/Runtime/<Core,Player,World,Gameplay,UI,Audio>` (namespace `Clover.*`) |
| Editor tools and builders | `Assets/_Project/Scripts/Editor/` (namespace `Clover.EditorTools`, menu root `Clover/`) |
| Tests | `Assets/_Project/Scripts/Tests/EditMode`, `Assets/_Project/Scripts/Tests/PlayMode` |
| Art | `Assets/_Project/Art/<Models,Textures,Materials,Shaders,VFX,Animation,Source>`; Blender files in `Source/` |
| Audio | `Assets/_Project/Audio/<Music,Ambience,SFX,Voice,UI,Mixers>` (48 kHz WAV sources; Unity compresses on import) |
| Scenes, prefabs, settings | `Assets/_Project/Scenes`, `Assets/_Project/Prefabs`, `Assets/_Project/Settings` |
| Generated assets | `Assets/_Project/Generated/` (rebuilt by builders under stable names) |
| Staging for generated or downloaded files | `staging/<source>/` (git-ignored except its README) |
| Scripts for `script run` | `AgentScripts/` (outside Assets, so Unity does not compile them) |
| Reports | `review/<audio,shots,checks,audit>/` |
| Builds | `Builds/` (git-ignored), from `Clover > Build > Windows Playtest` |

## Menus that matter

Clover > Setup > Ensure Project Layout; Clover > Checks > Run All; Clover > Audio > Loudness Report;
Clover > Review > Capture Stations and Accept Current As Golden; Clover > Build > Windows Playtest.

## Style

Commit messages are one plain sentence saying what changed. Code comments say why, not what, in plain words. Names in code
are full words. In-game text follows the voice set in `docs/GAME.md`.
