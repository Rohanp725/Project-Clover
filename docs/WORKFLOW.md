# The workflow

How this project is built with Claude Code, and how to carry the same setup into the next game. Written for the user; Claude
reads it too. The short version: **one agent, one editor bridge, scripted verification as the definition of done, a human on
feel.** The research that led here is in Project Mask's `reports/AI game dev workflow upgrades.md`.

## The shape

```
 you ── Claude Code (VS Code) ──┬── Unity CLI ──> open Unity editor (compile, tests, console, capture, scenes, assets, profiler, build)
                                ├── blender MCP ──> Blender with the MCP add-on (scripted bpy props, bakes, FBX to staging/)
                                ├── ffmpeg ──> audio measure, trim, normalise
                                └── subagents: code-reviewer, log-summariser, unity-api-checker, shot-reviewer
 hooks: SessionStart (what is reachable) · PreToolUse (no YAML writes) · Stop (no ending on compile errors)
 rules by path (.claude/rules) · skills for every repeatable job (.claude/skills) · CLAUDE.md under 100 lines
```

Two Claude Code sessions when research is involved: one that drives Unity and Blender, another that reads the web. A page
never gets to steer the hands.

## A day

1. Open Unity on the project (Claude never starts it). Open Blender and start its MCP server only on art days.
2. Start Claude Code in the project. The SessionStart hook tells it what it can reach.
3. Ask for one slice at a time. Claude follows `feature-loop`: check first, implement, `unity-check`, review, hand over with
   a screenshot path. You decide on feel in the editor or a playtest build; Claude turns your words into code and numbers.
4. Art: brief a prop, Claude builds it in Blender by script (`blender-prop`), shows a check render, you approve, then
   `asset-intake` moves it into `Assets/` with its manifest row. Textures and models generated elsewhere go through
   `staging/` the same way.
5. Audio: a recording or generated clip goes through `audio-intake` (ffmpeg measure, trim, normalise, install, loudness report).
6. Visuals: `review-shots` re-shoots every `ShotStation` and names what changed; you accept new goldens by saying so.
7. Before a build or after a batch of changes: `project-audit`. For testers: `playtest-build`.
8. Commit when a slice is verified: one sentence. Claude commits only when you ask.

## Roles

- **You**: pitch and pillars (`docs/GAME.md`), taste, approval of looks and sounds, playing the build, commits.
- **Claude (main session)**: code, builders, editor driving, verification, reports, writing decisions into `docs/GAME.md`.
- **code-reviewer** (Sonnet): a second read of every diff for the project's rules. **log-summariser** (Haiku): long logs,
  CSVs and reports in, ten lines out. **unity-api-checker** (Sonnet): verifies an API against this editor's package sources
  and docs before code relies on it. **shot-reviewer** (Sonnet): describes screenshots and lists candidate defects.
- **Hooks and tests**: the rules that must never be forgotten are enforced by machines, not remembered by people.

## What makes it effective for high detail

- **Visual**: golden frames per `ShotStation` catch regressions the moment they happen; `AssetImportRules` means a texture
  can never arrive sRGB-wrong or uncompressed; Blender work is scripted and repeatable, so a kit piece can be rebuilt with a
  new bevel in one call; every asset carries its source and licence.
- **Audio**: a BS.1770 meter inside the editor with per-folder targets, import settings by folder, ffmpeg for the fixes;
  the loudness report is the review surface, your ears decide.
- **Gameplay**: `script run` builds and rewires scenes without a domain reload; `[MethodReload]` edits a running method in
  Play Mode; `job wait` proves frames advance before a screenshot is trusted; input simulation drives probes; the profiler
  summary gives p95 against a budget.
- **Code**: four assemblies keep editor code out of the build and compile times short; conventions are tests; the Stop hook
  refuses a session that leaves the project uncompilable; the reviewer reads every diff.

## Steps left for you (in order)

1. **Restart Unity once** so the editor and its Pipeline server inherit the telemetry environment variables set today.
2. **Blender add-on**: already installed as
   `%APPDATA%\Blender Foundation\Blender\5.3\scripts\addons\blender_mcp.py` (to reinstall after a Blender upgrade:
   `$env:BLENDERMCP_ADDONS_DIR="<that folder for the new version>"; uvx mcp-for-blender==2.2.5 install-addon`; the
   success line may fail to print on a Windows console, the file still lands). Open Blender 5.3, Edit > Preferences >
   Add-ons, enable "MCP for Blender"; in the 3D viewport press N, MCP for Blender tab, Start MCP Server. Leave Auto Run
   Python Scripts off. Claude's `blender` MCP server (in `.mcp.json`) connects to it on 9876. Approve the project MCP server
   when Claude Code asks the first time.
3. **UnityMCP server scope**: done on 2026-11-14; it now lives in Project Mask's `.mcp.json`, not in the user config, so
   Clover sessions do not try to reach it.
4. **Write `docs/GAME.md`**: pitch, pillars, look, sound, budgets. Until then Claude will ask rather than guess.
5. **First scene**: ask Claude for a bootstrap scene with a `ShotStation`, add it to Build Settings, run the PlayMode smoke
   test and accept the first golden frame. Ask it to delete the template's `Assets/TutorialInfo` and `Readme.asset` through
   the editor at the same time.
6. **First commit** when you are happy: `git add -A && git commit -m "Workflow, tooling and project layout"`.
7. **Later, when the look is decided**: set up the art generation stack from `docs/TOOLS.md` (ComfyUI and models that
   fit a 10 GB card, TripoSG for blockouts, Stable Audio and Kokoro for sound) following the install rules there.

## Carrying this into the next game

Copy these from this repo into the new Unity project before the first prompt: `CLAUDE.md`, `.claude/` (settings, hooks,
rules, skills, agents), `.mcp.json`, `.gitignore`, `.gitattributes`, `docs/`, `AgentScripts/README.md`, `staging/README.md`,
and `Assets/_Project/Scripts/{Editor,Runtime/Core,Tests}` with the four `.asmdef` files. Rename `Clover` in namespaces,
asmdef names and menu paths (a find and replace), set `Packages/manifest.json` to the pinned packages, run
`Clover > Setup > Ensure Project Layout`, compile, run the EditMode tests, and start with `docs/GAME.md`. Keep the engine-
specific parts (the Unity CLI commands in CLAUDE.md, `unity-check`, `playmode-probe`) separate in your head from the
engine-agnostic parts (rules, manifest, staging, review folders, agents): for a Godot or Unreal game only the former change.

## Re-check routine (before each new game, or when bumping a pin)

Read five pages: Unity CLI release notes (`unity changelog`) and the Pipeline package changelog; Claude Code's hooks,
subagents and settings docs; the mcp-for-blender releases and OSV entry for CVE-2026-10688; the Comfy Org blog and
ComfyUI-Manager security notes if the generation stack is installed; the Hugging Face licence field of any model in use. Then
update `docs/TOOLS.md` with the new pins and dates, and run `project-audit`.
