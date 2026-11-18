#!/usr/bin/env bash
# The controls probe of 2026-11-18 (ControlsProbe.cs): two plays, sandboxed, so the owner's progress and bindings are
# never read or written; the real bindings key is shown before and after to prove it.
#   play 1 (Hub): a virtual Xbox pad, the menus, two rebinds (one by pad, one by simulate_key T).
#   play 2 (CombatTest): the rebinds came back; keys and mouse: a walk held with simulate_key, the cursor put with
#   simulate_pointer, and the clicks and menu keys pressed from the game's frames (ControlsProbe "click", "keys"):
#   the CLI's own presses reach held state but are never WasPressedThisFrame in the player loop while the editor is
#   in the background, so an attack or a menu press sent through it does nothing there. A mouse-aimed attack on a
#   dummy and one at open ground; the controls page in keys names; reset to defaults through the page.
cd "c:/Users/rohan/UnityProjects/Project Clover"
P="C:\Users\rohan\UnityProjects\Project Clover"
TMPFILE="${TEMP:-.}/controls_probe_dummy.txt"
# An eval prints its value bare, then lines of its own about the compile: keep the value.
ev() { unity command script eval -- --code "$1" 2>&1 | grep -vE "^(diagnostics|compileMs|executeMs|assemblyName|result: *$)" | head -3; }
part() { unity command script run -- --file AgentScripts/ControlsProbe.cs --entry ControlsProbe.Run --args "[\"$1\"]" 2>&1 | grep -E "^result|rror" | head -3; }
probe_lines() { unity command console get -- --tail 200 2>&1 | grep -o "\[Probe\][^\"]*"; }
wait_line() { for i in $(seq 1 ${2:-60}); do unity command console get -- --tail 200 2>/dev/null | grep -q "$1" && return 0; timeout 2 ping -n 2 127.0.0.1 >/dev/null; done; return 1; }
key() { unity command runtime simulate_key -- --key "$1" --action "${2:-press}" 2>&1 | grep -iE "error|fail" | head -2; }
pointer() { unity command runtime simulate_pointer -- --x "$1" --y "$2" --action "${3:-click}" --button "${4:-left}" 2>&1 | grep -iE "error|fail" | head -2; }
settle() { timeout ${1:-1} ping -n ${1:-1} 127.0.0.1 >/dev/null; }

begin_play() {
  unity command script eval "UnityEditor.SceneManagement.EditorSceneManager.OpenScene(\"Assets/_Project/Scenes/$1.unity\"); return \"opened\";" 2>&1 | tail -1
  unity command console clear >/dev/null 2>&1
  unity command script eval "UnityEngine.PlayerPrefs.SetInt(\"clover.progress.sandbox\", 1); UnityEngine.PlayerPrefs.Save(); return \"sandboxed\";" >/dev/null 2>&1
  unity command editor.playmode start >/dev/null 2>&1
  unity status --until-ready --project-path "$P" >/dev/null 2>&1
  unity command job wait -- --condition '{"member":"UnityEngine.Time.frameCount","op":"changed"}' --timeout 10 2>&1 | grep -E "^met"
  # Keys and pointer events reach the game with the editor in the background only with these (put back at the end).
  ev 'UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus; UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; return "focus ignored";'
}

end_play() {
  unity command console get -- --tail 40 --level warn 2>&1 | grep -E '"message"|^counts' | head -12
  unity command editor.playmode stop >/dev/null 2>&1
  unity status --until-ready --project-path "$P" >/dev/null 2>&1
}

unity --json recompile --timeout 180 2>&1 | grep -E '"failed": true|"message"' | head -5
unity status --until-ready --project-path "$P" >/dev/null 2>&1
echo "== before: real bindings key held, sandbox key held"
ev 'return UnityEngine.PlayerPrefs.HasKey("clover.controls.bindings") + " " + UnityEngine.PlayerPrefs.HasKey("clover.controls.bindings.sandbox") + " real=" + UnityEngine.PlayerPrefs.GetString("clover.controls.bindings","(none)").Length;'
ev 'UnityEngine.PlayerPrefs.DeleteKey("clover.controls.bindings.sandbox"); UnityEngine.PlayerPrefs.Save(); return "sandbox bindings cleared";'
unity command editor set_auto_tick >/dev/null 2>&1
unity command editor.playmode set_run_in_background >/dev/null 2>&1

echo "== play 1: Hub, the Xbox pad"
begin_play Hub
part pad
wait_line "5 listening keys" 90 || echo "never listened for keys"
settle 1
key T
wait_line "\[Probe\] done" 60 || echo "pad part did not finish"
probe_lines
end_play

echo "== play 2: CombatTest, kept, keys and mouse, reset"
begin_play CombatTest
settle 1
part kept
# Walk with the keys, from the device being the pad's last time to keys now.
key W down; settle 1; key W up
part facing
part dummy > "$TMPFILE" 2>&1; cat "$TMPFILE"
read DX DZ CX CZ < <(grep -o "dummy .* click .*" "$TMPFILE" | awk '{print $2, $3, $5, $6}')
read SX SY < <(part "aim $CX $CZ" | awk '{print $2, $3}')
echo "cursor at screen $SX $SY for ground $CX $CZ (dummy at $DX $DZ)"
pointer $((SX - 40)) $((SY - 40)) move; pointer $SX $SY move
settle 1
unity command capture game_view -- --output-path review/shots/current/controls_mouse_aim_before.png --width 1280 --height 720 2>&1 | grep -iE "error" | head -2
part "click mouse_aim"
wait_line "click done" 20
settle 2
# Away from every dummy: the cursor four metres south of the player, where nothing stands in the cone.
read PX PZ < <(ev 'var p = UnityEngine.Object.FindAnyObjectByType<Clover.Player.PlayerController>().transform.position; return p.x.ToString("0.00") + " " + p.z.ToString("0.00");' | tail -1)
echo "player at $PX $PZ"
read EX EY < <(part "aim $PX $(python -I -c "import sys; print(float(sys.argv[1]) - 4)" "$PZ" </dev/null)" | awk '{print $2, $3}')
pointer $EX $EY move; settle 1
part "click mouse_open"
wait_line "click mouse_open" 20
settle 2
part keys
wait_line "keys done" 20
part keysmenu
wait_line "keysmenu done" 20
part reset
wait_line "reset done" 20
probe_lines
end_play

echo "== play 3: Hub again, nothing kept after the reset"
begin_play Hub
part kept
end_play

echo "== putting the editor back"
ev 'UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.ResetAndDisableNonBackgroundDevices; UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.PointersAndKeyboardsRespectGameViewFocus; return "focus as it was";'
ev 'UnityEngine.PlayerPrefs.DeleteKey("clover.progress.sandbox"); UnityEngine.PlayerPrefs.DeleteKey("clover.controls.bindings.sandbox"); UnityEngine.PlayerPrefs.Save(); return "sandbox cleared";'
unity command script eval "UnityEditor.SceneManagement.EditorSceneManager.OpenScene(\"Assets/_Project/Scenes/Hub.unity\"); return \"opened\";" 2>&1 | tail -1
unity command editor set_auto_tick -- --enabled false >/dev/null 2>&1
unity command editor.playmode set_run_in_background -- --enabled false >/dev/null 2>&1
echo "== after: real bindings key held, sandbox key held, sandbox flag"
ev 'return UnityEngine.PlayerPrefs.HasKey("clover.controls.bindings") + " " + UnityEngine.PlayerPrefs.HasKey("clover.controls.bindings.sandbox") + " " + UnityEngine.PlayerPrefs.HasKey("clover.progress.sandbox") + " real=" + UnityEngine.PlayerPrefs.GetString("clover.controls.bindings","(none)").Length + " bg=" + UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior + " ed=" + UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode;'
unity command editor status 2>&1 | grep -E "playMode|runInBackground"
