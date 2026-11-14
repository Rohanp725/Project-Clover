---
name: playtest-build
description: Make the Windows playtest build, smoke-test it, zip it and clean up the test's traces. Use when the user asks for a build or a zip for testers.
---

# Playtest build

1. Compile clean (`unity --no-banner --json recompile`) and run the EditMode tests. Scenes the build needs are enabled in
   Build Settings (`unity command build get_settings`; add one with `scene add_to_build`).
2. Build: `unity command editor invoke_menu -- --path "Clover/Build/Windows Playtest"` (`PlaytestBuild.cs`: development
   build, `Builds/<Product>_Playtest_v<version>_<yyyyMMdd>/<Product>.exe`, a `READ ME FIRST.txt` beside it). Or
   `unity command build run -- --target StandaloneWindows64 --output-path Builds/... --confirm true` and poll `build status`.
   The build can take minutes: poll `unity command build status` or grep `Logs/Editor.log` for `Build Finished`; do not
   retry the menu call.
3. Zip from PowerShell: `Compress-Archive -Path "Builds/<dir>/*" -DestinationPath "Builds/<dir>.zip" -Force`.
4. Smoke test: run the exe for a short while with `-screen-fullscreen 0 -screen-width 1280 -screen-height 720`, stop it,
   then read `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\Player.log` for exceptions and the `[Clover]` boot lines.
   A development build also exposes the Pipeline runtime server: `unity command --runtime <Product> runtime status`
   and `capture game_view --runtime <Product>` work while it runs.
5. Clean up: delete `HKCU\Software\<Company>\<Product>` (the test leaves window size and prefs) and the `Player*.log`
   files, not the LocalLow folder itself.
6. Tell the user the folder, the zip, the Player.log result, and anything seen.

Builds from editors released before October 2025 are affected by Unity's CVE-2025-59489; never ship one.
