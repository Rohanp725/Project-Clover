---
name: unity-check
description: Verify a change in the open Unity editor through the Unity CLI - compile, read the console, run the tests, capture a frame. Use after any C#, builder, asset or settings edit, before saying something works.
---

# Unity check

The editor must be open with the project. `unity status` lists it (state `ready`). If none, tell the user to open Unity;
never start `Unity.exe -batchmode` while an editor may be open. Exit 2 from the CLI means fix the command line; exit 6
means the editor failed or is unreachable, so do not rewrite the command.

## Levels, in order

1. **Compile** (always after C#): `unity --no-banner --json recompile --timeout 120`. `data.summary.errors` must be 0;
   `data.errors[]` has file, line and message. `status: up_to_date` means nothing changed. If the call times out while the
   editor imports, poll `unity command script recompile_status` until `completed` or `up_to_date`.
2. **Console**: `unity command console status` for the counts and `compilationFailed`; then
   `unity command console get -- --tail 30 --level error`. `unity command console clear` before a probe when only new entries
   matter. Lines about the Pipeline transport reconnecting after a domain reload are noise.
3. **Tests**: `unity command test run -- --mode editor` after code or asset changes; add `--filter <Fixture>` to go fast.
   PlayMode tests cannot run synchronously (entering play mode drops the request): run
   `unity command test run -- --mode playmode --async-tests true`, then poll `unity command test status` every ten seconds
   until `status` is `completed` and read `passed`, `failed` and each result's message.
4. **Look**: for edit-mode content, `unity command editor invoke_menu -- --path "Clover/Review/Capture Stations"` then read
   `review/shots/diff.md` and the PNGs that differ. For play-mode content, use the `playmode-probe` skill and
   `capture game_view -- --output-path review/shots/current/<name>.png`. Read the PNG with the Read tool and say what is in it.
5. **Performance** (when the change could cost frames): in Play Mode, `diagnostics profiler_record -- --frames 300`, poll
   `profiler_record_status`, then `profiler_session -- --capture <path> --confirm true` and `profiler_summary`. Compare the
   median and p95 against the budget in `docs/GAME.md`.
6. **Audit** (before a build or when import settings changed): `diagnostics run_audit -- --output-path review/audit/project-auditor.csv`,
   poll `audit_status`, then hand the CSV to the log-summariser agent.

## Rules

- After editing a builder, compile first and wait, then rebuild through its menu, or the scene is rebuilt from old code.
- Report the actual output: counts, the first failing message, the path of the screenshot. If the editor was unreachable,
  say the check did not run.
