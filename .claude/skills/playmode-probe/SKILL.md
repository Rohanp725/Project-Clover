---
name: playmode-probe
description: Enter Play Mode through the Unity CLI, prove the game is actually running, drive it with simulated input, capture the Game View and read the console, then stop. Use to see a gameplay change working, not just compiling.
---

# Play Mode probe

Read the play state first: `unity command editor status`. If `playMode` is not `stopped`, the session is the user's; only
read (console, capture) and report, do not start, stop or tune.

1. `unity command editor set_auto_tick` (keeps the editor ticking while unfocused; otherwise the game freezes at frame 1).
   `unity command editor.playmode set_run_in_background` so Windows does not pause the player loop when unfocused.
2. `unity command console clear`, then `unity command editor.playmode start`, then
   `unity status --until-ready --project-path "C:\Users\rohan\UnityProjects\Project Clover"`.
3. Prove frames advance: `unity command job wait -- --condition '{"member":"UnityEngine.Time.frameCount","op":"changed"}' --timeout 10`.
   `met: true` is the proof; `timedOut: true` means the player loop is stalled, go back to step 1.
4. Set up the situation: `script eval` one-liners against the running game (find the player, teleport, set a flag), or a
   prepared `AgentScripts/Probe.cs` with `script run --entry Probe.Setup`. Keep `Time.timeScale` normal unless the probe is
   about timing.
5. Drive: `unity command runtime.input simulate_key -- --key w --action hold --duration 1.5` style calls and
   `simulate_pointer` (see `unity command --query simulate --detail full` for the exact parameters). Wait with `job wait`
   on a member that should change, not with sleeps.
6. Look: `unity command capture game_view -- --output-path review/shots/current/probe_<what>.png --width 1280 --height 720`
   and Read the PNG. Describe what is visible, in words, before judging it.
7. Console: `unity command console get -- --tail 40 --level warn`. Exceptions during the probe are findings.
8. `unity command editor.playmode stop`, then `editor set_auto_tick -- --enabled false` and
   `editor.playmode set_run_in_background -- --enabled false`.

Tuning in place: a method tagged `[MethodReload]` before play can be edited on disk and applied with
`unity command script.methodreload reload -- --filename Assets/_Project/Scripts/Runtime/<file>.cs` without leaving Play Mode.

Report: what was set up, what input was sent, what the frame showed (with the PNG path), and every console error.
