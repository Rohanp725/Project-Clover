# Tools: pinned manifest and decisions

Every tool the workflow depends on, with the version as pinned, the licence as read on the date, the install surface that was
reviewed, its telemetry switch, the port it listens on, why it was chosen and what would make us drop it. Re-check this file
before each new game and whenever a pin is bumped (see "Re-check routine" in WORKFLOW.md).

| Tool | Pinned | Licence | Reviewed | Telemetry | Port | Why | Drop when |
|---|---|---|---|---|---|---|---|
| Unity Editor | 6000.6.5f1 | Unity Personal/Pro terms | n/a | Unity's own | n/a | URP 17, Awaitable, built-in Project Auditor | 6000.7 LTS is out and URP and the Pipeline package work on it |
| Unity CLI (`unity`) | 1.0.0 (updated 2026-11-14 with `unity self-update`; the Hub keeps it current) | Unity terms | `unity analytics` off; no login used | `unity analytics disable`; env `UNITY_NO_CLI_INVOKED_TELEMETRY` | none (client) | one bridge for compile, tests, console, capture, scene edits, script run, profiler, audit, build | it starts requiring `unity auth login` for local editor commands |
| com.unity.pipeline | 1.0.0 (registry) | Unity Companion | package source in `Library/PackageCache` | via Unity | 7800-7849 editor, 7900-7949 dev player, localhost | the editor side of the CLI; `script run` compiles builders without a domain reload | a release binds beyond localhost by default or adds auth we cannot satisfy locally |
| com.unity.project-auditor-rules | 3.2.0 | Unity Companion | package manifest only | none | none | asset, settings and shader findings for `diagnostics run_audit` | superseded inside the editor |
| Unity Claude Code plugin `unity@unity-agent-plugin` | 0.1.9-beta (user scope) | Unity Companion | `hooks/` none, `.mcp.json` none, skills only | none | none | current-API skills (UI Toolkit, URP post, Shader Graph, CLI) | it starts shipping hooks or MCP servers we did not ask for |
| Claude Code | 2.1.x (>= 2.0.65 required) | Anthropic terms | project `.claude/` reviewed | Anthropic's | none | the agent; hooks, rules, skills, subagents | n/a |
| Blender | 5.3.0 (winget `BlenderFoundation.Blender`, MSI from download.blender.org) | GPL-2.0+ | official installer only | off by default | none | scripted `bpy` props, bakes, FBX export | n/a; Auto Run Python Scripts stays off |
| mcp-for-blender (ahujasid) | 2.2.5 (PyPI, 2026-10-28; via `uvx` in `.mcp.json`) | MIT | README, env vars; add-on `blender_mcp.py` copied by `install-addon` | `DISABLE_TELEMETRY=true` set in `.mcp.json` | 9876 localhost, unauthenticated, only while the add-on server runs | the only maintained open Blender bridge; `BLENDER_MCP_SAFE_MODE=1` blocks file, process and network access from scripts | CVE-2026-10688 (unauthenticated socket) stays unanswered and an authenticated fork is maintained; or telemetry stops honouring the switch |
| ffmpeg | 9.0.2 (winget `Gyan.FFmpeg`) | GPL/LGPL build | official gyan.dev build | none | none | loudness measurement and normalisation, trims, format conversion | n/a |
| uv / uvx | 0.13.0 | MIT/Apache-2.0 | n/a | none | none | runs pinned Python MCP servers without a global install | n/a |
| Git + Git LFS | 2.39 / 3.3.0 | GPL / MIT | n/a | none | none | version control; LFS for binary art and audio | n/a |

## Not installed, on purpose

- **MCP for Unity (CoplayDev)**: used in Project Mask; here the Unity CLI covers every operation it provided, so a second
  unauthenticated code-execution port on 8080 buys nothing. Revisit if the CLI loses a capability the project needs.
- **hatayama unity-cli-loop**: redundant with the Unity CLI.
- **ComfyUI, TripoSG, Stable Audio, Kokoro/Dia**: planned for art and audio generation once `docs/GAME.md` says what the
  game looks and sounds like. Install rules when the time comes: one venv per tool, `python -I`, safetensors only, never
  `--listen 0.0.0.0`, ComfyUI >= 0.3.76 with Manager >= 3.38 at `security_level=normal`, read every node's requirements.
  Shippable licences only (FLUX.1 schnell, FLUX.2 klein 4B, Z-Image-Turbo, SDXL; Stable Audio Open 1.0; ACE-Step; Kokoro).
- **Unity AI Assistant / Generators, Meshy, Tripo, Freesound API, IvanMurzak Unity-MCP cloud mode**: need accounts or
  subscriptions.
- **Agent frameworks with dozens of agents and unpinned `npx @latest`** (claude-flow, claude-code-game-studios, ECC):
  borrow single files after reading, never install.

## Security floor (applied)

- Edit/Write denied on Unity serialized files; Bash guard hook; Stop hook that refuses to end on compile errors.
- Pinned versions everywhere; `npm config set ignore-scripts true` at user scope; `DISABLE_TELEMETRY`, `DO_NOT_TRACK`,
  `UNITY_NO_CLI_INVOKED_TELEMETRY`, `UNITY_NO_CRASH_REPORT` set as user environment variables.
- Servers on localhost only; the Blender add-on server is started only for a Blender session and stopped after.
- Downloads are extracted into their own folder under `staging/` and read before use; `.blend` files never auto-run scripts.
- One Claude Code session drives the editor; web research happens in a different session so a page cannot steer the hands.
- Never `--dangerously-skip-permissions`. A cloned repo's `.claude/`, `.mcp.json` and hooks are read before opening it.
