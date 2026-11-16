#!/usr/bin/env bash
# usage: runprobe.sh <SceneName> <ProbeName> [maxPolls]
cd "c:/Users/rohan/UnityProjects/Project Clover"
P="C:\Users\rohan\UnityProjects\Project Clover"
unity --json recompile --timeout 180 2>&1 | grep -E '"failed": true|"message"' | head -10
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command script eval "UnityEditor.SceneManagement.EditorSceneManager.OpenScene(\"Assets/_Project/Scenes/$1.unity\"); return \"opened\";" 2>&1 | tail -1
unity command editor set_auto_tick >/dev/null 2>&1
unity command editor.playmode set_run_in_background >/dev/null 2>&1
unity command console clear >/dev/null 2>&1
unity command editor.playmode start >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command job wait -- --condition '{"member":"UnityEngine.Time.frameCount","op":"changed"}' --timeout 10 2>&1 | grep -E "^met"
unity command script run -- --file AgentScripts/$2.cs --entry $2.Run 2>&1 | grep -iE "error|started" | head -5
for i in $(seq 1 ${3:-40}); do
  if unity command console get -- --tail 120 2>/dev/null | grep -q "Probe\] done"; then break; fi
  timeout 4 ping -n 4 127.0.0.1 >/dev/null
done
unity command console get -- --tail 150 2>&1 | grep -o "\[Probe\][^\"]*"
unity command console get -- --tail 30 --level warn 2>&1 | grep -E '"message"|^counts' | head -12
unity command editor.playmode stop >/dev/null 2>&1
unity status --until-ready --project-path "$P" >/dev/null 2>&1
