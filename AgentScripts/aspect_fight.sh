#!/usr/bin/env bash
# The Summit at every screen shape: runs DragonViewProbe's Hold and HoldChoice parts as a new player (nothing saved) and, each
# time the probe says it is holding, captures the Game View at every size (aspect_shots.sh). Sheets: aspect_fight_middle,
# aspect_fight_south, aspect_choice, aspect_death in review/shots/current/sheets.
cd "c:/Users/rohan/UnityProjects/Project Clover"
P="C:\Users\rohan\UnityProjects\Project Clover"
echo "${1:-Hold,HoldChoice}" > AgentScripts/dragon_view.txt
unity --json recompile --timeout 180 2>&1 | grep -E '"failed": true|"message"' | head -10
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command script eval "UnityEditor.SceneManagement.EditorSceneManager.OpenScene(\"Assets/_Project/Scenes/Summit.unity\"); return \"opened\";" 2>&1 | tail -1
unity command editor set_auto_tick >/dev/null 2>&1
unity command editor.playmode set_run_in_background >/dev/null 2>&1
unity command console clear >/dev/null 2>&1
unity command script eval "UnityEngine.PlayerPrefs.SetInt(\"clover.progress.sandbox\", 1); UnityEngine.PlayerPrefs.Save(); return \"sandboxed\";" >/dev/null 2>&1
unity command editor.playmode start >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command script run -- --file AgentScripts/DragonViewProbe.cs --entry DragonViewProbe.Run 2>&1 | grep -iE "error|started" | head -5
waitfor() {
  for i in $(seq 1 60); do
    if unity command console get -- --tail 60 2>/dev/null | grep -q "$1"; then return 0; fi
    timeout 2 ping -n 2 127.0.0.1 >/dev/null
  done
  echo "never saw: $1"; return 1
}
waitfor "holding middle" && bash AgentScripts/aspect_shots.sh fight_middle $SIZES
waitfor "holding south" && bash AgentScripts/aspect_shots.sh fight_south $SIZES
waitfor "holding choice" && bash AgentScripts/aspect_shots.sh choice $SIZES
waitfor "holding death" && bash AgentScripts/aspect_shots.sh death 1024x768 3840x1080
# The fall reloads the Hub and the probe goes with the scene: there is no "done" to wait for.
unity command console get -- --tail 150 2>&1 | grep -o "\[Probe\][^\"]*"
unity command console get -- --tail 30 --level warn 2>&1 | grep -E '^counts'
unity command editor.playmode stop >/dev/null 2>&1
unity command script eval "UnityEngine.PlayerPrefs.DeleteKey(\"clover.progress.sandbox\"); UnityEngine.PlayerPrefs.Save(); return \"real\";" >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
