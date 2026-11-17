#!/usr/bin/env bash
# The sun-court at every screen shape: runs SunCourtProbe's Hold part as a new player (nothing saved) and, each time
# the probe says it is holding, captures the Game View at every size (aspect_shots.sh). Sheets: aspect_court_pass and
# aspect_court_blaze in review/shots/current/sheets.
cd "c:/Users/rohan/UnityProjects/Project Clover"
P="C:\Users\rohan\UnityProjects\Project Clover"
echo "Hold" > AgentScripts/suncourt_only.txt
unity --json recompile --timeout 180 2>&1 | grep -E '"failed": true|"message"' | head -10
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command script eval "UnityEditor.SceneManagement.EditorSceneManager.OpenScene(\"Assets/_Project/Scenes/SunCourt.unity\"); return \"opened\";" 2>&1 | tail -1
unity command editor set_auto_tick >/dev/null 2>&1
unity command editor.playmode set_run_in_background >/dev/null 2>&1
unity command console clear >/dev/null 2>&1
unity command script eval "UnityEngine.PlayerPrefs.SetInt(\"clover.progress.sandbox\", 1); UnityEngine.PlayerPrefs.Save(); return \"sandboxed\";" >/dev/null 2>&1
unity command editor.playmode start >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command script run -- --file AgentScripts/SunCourtProbe.cs --entry SunCourtProbe.Run 2>&1 | grep -iE "error|started" | head -5
waitfor() {
  for i in $(seq 1 60); do
    if unity command console get -- --tail 60 2>/dev/null | grep -q "$1"; then return 0; fi
    timeout 2 ping -n 2 127.0.0.1 >/dev/null
  done
  echo "never saw: $1"; return 1
}
SIZES="1280x720 1280x800 1024x768 2560x1080"
waitfor "holding pass" && bash AgentScripts/aspect_shots.sh court_pass $SIZES
waitfor "holding blaze" && bash AgentScripts/aspect_shots.sh court_blaze $SIZES
waitfor "Probe\] done"
unity command console get -- --tail 150 2>&1 | grep -o "\[Probe\][^\"]*"
unity command console get -- --tail 30 --level warn 2>&1 | grep -E '^counts'
unity command editor.playmode stop >/dev/null 2>&1
unity command script eval "UnityEngine.PlayerPrefs.DeleteKey(\"clover.progress.sandbox\"); UnityEngine.PlayerPrefs.Save(); return \"real\";" >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
rm -f AgentScripts/suncourt_only.txt
