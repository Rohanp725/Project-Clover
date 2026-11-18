#!/usr/bin/env bash
# Records the profiler over the Sun City's busiest moment (PerfProbe's SunCity branch: the Market of the Cold Oven with
# everyone within 60 m awake and a swell on) and prints PerfProbe's line and the profiler's summary.
# usage: bash AgentScripts/profile_city.sh <label> [folder]   (writes review/checks/<folder, default world1>/city_<label>.data)
cd "c:/Users/rohan/UnityProjects/Project Clover"
P="C:\Users\rohan\UnityProjects\Project Clover"
LABEL=${1:-now}
OUT=review/checks/${2:-world1}
mkdir -p $OUT
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command script eval "UnityEditor.SceneManagement.EditorSceneManager.OpenScene(\"Assets/_Project/Scenes/SunCity.unity\"); return \"opened\";" 2>&1 | tail -1
unity command editor set_auto_tick >/dev/null 2>&1
unity command editor.playmode set_run_in_background >/dev/null 2>&1
unity command console clear >/dev/null 2>&1
unity command script eval "UnityEngine.PlayerPrefs.SetInt(\"clover.progress.sandbox\", 1); UnityEngine.PlayerPrefs.Save(); return \"sandboxed\";" >/dev/null 2>&1
unity command editor.playmode start >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command job wait -- --condition '{"member":"UnityEngine.Time.frameCount","op":"changed"}' --timeout 10 2>&1 | grep -E "^met"
unity command script run -- --file AgentScripts/PerfProbe.cs --entry PerfProbe.Run 2>&1 | grep -iE "error|started" | head -5
timeout 3 ping -n 3 127.0.0.1 >/dev/null
unity command diagnostics profiler_record -- --frames 600 --output-path $OUT/city_$LABEL.data 2>&1 | tail -2
for i in $(seq 1 30); do
  if unity command diagnostics profiler_record_status 2>/dev/null | grep -qiE "status: (completed|failed|interrupted)"; then break; fi
  timeout 2 ping -n 2 127.0.0.1 >/dev/null
done
SESSION=$(unity command diagnostics profiler_session -- --capture $OUT/city_$LABEL.data --confirm true 2>&1 | grep -oiE "sessionId: *[A-Za-z0-9_-]+" | head -1 | sed 's/.*: *//')
echo "session $SESSION"
unity command diagnostics profiler_summary -- --session "$SESSION" --target-frame-ms 16.6 --spikes 2 2>&1 | cut -c1-600 | head -60 > $OUT/city_$LABEL.summary.txt
cat $OUT/city_$LABEL.summary.txt | head -60
for i in $(seq 1 20); do
  if unity command console get -- --tail 40 2>/dev/null | grep -q "Probe\] done"; then break; fi
  timeout 3 ping -n 3 127.0.0.1 >/dev/null
done
unity command console get -- --tail 60 2>&1 | grep -o "\[Probe\][^\"]*"
unity command console get -- --tail 30 --level warn 2>&1 | grep -E '"message"|^counts' | head -6
unity command editor.playmode stop >/dev/null 2>&1
unity command script eval "UnityEngine.PlayerPrefs.DeleteKey(\"clover.progress.sandbox\"); UnityEngine.PlayerPrefs.Save(); return \"real\";" >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
