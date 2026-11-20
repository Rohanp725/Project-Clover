#!/usr/bin/env bash
# Records 600 frames of the profiler while the Proving Ground crowd fights with every duo branch learned and the carried pair turned through four heart branches (HeartCrowdProbe), and prints the summary.
# usage: bash AgentScripts/profile_heart.sh
cd "c:/Users/rohan/UnityProjects/Project Clover"
P="C:\Users\rohan\UnityProjects\Project Clover"
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command script eval "UnityEditor.SceneManagement.EditorSceneManager.OpenScene(\"Assets/_Project/Scenes/ProvingGround.unity\"); return \"opened\";" 2>&1 | tail -1
unity command editor set_auto_tick >/dev/null 2>&1
unity command editor.playmode set_run_in_background >/dev/null 2>&1
unity command console clear >/dev/null 2>&1
# Saved progress is kept out of it, as in runprobe.sh.
unity command script eval "UnityEngine.PlayerPrefs.SetInt(\"clover.progress.sandbox\", 1); UnityEngine.PlayerPrefs.Save(); return \"sandboxed\";" >/dev/null 2>&1
unity command editor.playmode start >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command job wait -- --condition '{"member":"UnityEngine.Time.frameCount","op":"changed"}' --timeout 10 2>&1 | grep -E "^met"
unity command script run -- --file AgentScripts/HeartCrowdProbe.cs --entry HeartCrowdProbe.Run 2>&1 | grep -iE "error|started" | head -5
for i in $(seq 1 20); do
  if unity command console get -- --tail 40 2>/dev/null | grep -q "Probe\] soak start"; then break; fi
  timeout 2 ping -n 2 127.0.0.1 >/dev/null
done
unity command diagnostics profiler_record -- --frames 600 --output-path review/checks/heart_crowd.data 2>&1 | tail -4
for i in $(seq 1 20); do
  if unity command diagnostics profiler_record_status 2>/dev/null | grep -qiE "status: (completed|failed|interrupted)"; then break; fi
  timeout 2 ping -n 2 127.0.0.1 >/dev/null
done
unity command diagnostics profiler_record_status 2>&1 | grep -iE "status|frames" | head -6
SESSION=$(unity command diagnostics profiler_session -- --capture review/checks/heart_crowd.data --confirm true 2>&1 | grep -oiE "sessionId: *[A-Za-z0-9_-]+" | head -1 | sed 's/.*: *//')
echo "session $SESSION"
unity command diagnostics profiler_summary -- --session "$SESSION" --target-frame-ms 16.6 --spikes 3 2>&1 | cut -c1-900 | head -40
unity command diagnostics profiler_markers -- --session "$SESSION" 2>&1 | grep -iE "ReactionLook|ConditionCaster|PlayerReactions|PlayerUpgrades|Afflictions|Clover|GC\.Alloc|PlayerLoop|totalGc|gcAlloc" | cut -c1-400 | head -30
for i in $(seq 1 20); do
  if unity command console get -- --tail 40 2>/dev/null | grep -q "Probe\] done"; then break; fi
  timeout 3 ping -n 3 127.0.0.1 >/dev/null
done
unity command console get -- --tail 60 2>&1 | grep -o "\[Probe\][^\"]*"
unity command console get -- --tail 30 --level warn 2>&1 | grep -E '"message"|^counts' | head -6
unity command editor.playmode stop >/dev/null 2>&1
unity command script eval "UnityEngine.PlayerPrefs.DeleteKey(\"clover.progress.sandbox\"); UnityEngine.PlayerPrefs.Save(); return \"real\";" >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
# Let the probe finish, print its own measurement, and stop playing.
for i in $(seq 1 30); do
  if unity command console get -- --tail 60 2>/dev/null | grep -q "Probe\] done"; then break; fi
  timeout 2 ping -n 2 127.0.0.1 >/dev/null
done
unity command console get -- --tail 60 2>&1 | grep -o "\[Probe\][^\"]*"
unity command console get -- --tail 30 --level warn 2>&1 | grep -E '"message"|^counts' | head -8
unity command editor.playmode stop >/dev/null 2>&1
unity command script eval "UnityEngine.PlayerPrefs.DeleteKey(\"clover.progress.sandbox\"); UnityEngine.PlayerPrefs.Save(); return \"real\";" >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
