---
name: project-audit
description: Run the project's health checks together - Project Auditor scan, convention checks, loudness report, golden shots - and summarise what needs fixing. Use before a build, after a batch of asset changes, or when asked how the project is doing.
---

# Project audit

All four write files under `review/`; read the files, not the console. Run them in this order and hand long outputs to the
log-summariser agent instead of reading them in the main context.

1. **Compile and tests**: `unity --no-banner --json recompile`; `unity command test run -- --mode editor`.
2. **Convention checks**: `unity command editor invoke_menu -- --path "Clover/Checks/Run All"` → `review/checks/checks.md`
   (missing scripts, broken shaders, assets without manifest rows, runtime code using editor or legacy input APIs).
3. **Loudness**: `unity command editor invoke_menu -- --path "Clover/Audio/Loudness Report"` → `review/audio/loudness.md`.
4. **Golden shots**: `unity command editor invoke_menu -- --path "Clover/Review/Capture Stations"` → `review/shots/diff.md`;
   Read every PNG marked DIFFERENT and describe the change.
5. **Project Auditor**: `unity command diagnostics run_audit -- --output-path review/audit/project-auditor.csv`, poll
   `unity command diagnostics audit_status` until `completed`, then the log-summariser agent reads the CSV: counts by
   severity and area, the ten highest-severity rows. Shader warnings about Built-in RP in third-party shaders are known;
   texture, mesh and audio import findings are the ones to act on through `AssetImportRules.cs`.
6. **Performance** (when a scene exists): a play-mode probe with `diagnostics profiler_record` and `profiler_summary`,
   compared with the budget in `docs/GAME.md`.

Report in one list, most important first: what fails, where the report is, and the one-line fix for each. Say which
checks did not run and why.
