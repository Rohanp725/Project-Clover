#!/usr/bin/env bash
# Opens the Hub (so a recompile does not remake the merged city twice), recompiles, rebuilds Sun City (and the court
# with "court"), and prints the build's lines and any errors.  usage: bash AgentScripts/rebuild_city.sh [court]
cd "c:/Users/rohan/UnityProjects/Project Clover"
P="C:\Users\rohan\UnityProjects\Project Clover"
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command script eval --timeout 60 "UnityEditor.SceneManagement.EditorSceneManager.OpenScene(\"Assets/_Project/Scenes/Hub.unity\"); return \"hub\";" 2>&1 | tail -1
unity --json recompile --timeout 300 2>&1 | grep -E '"errors": [0-9]|"message"|"file"|"line"' | head -12
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command console clear >/dev/null 2>&1
unity command editor invoke_menu --timeout 900 -- --path "Clover/Build/Sun City" 2>&1 | tail -1
if [ "$1" = "court" ]; then unity command editor invoke_menu --timeout 900 -- --path "Clover/Build/Sun Court" 2>&1 | tail -1; fi
unity status --until-ready --project-path "$P" >/dev/null 2>&1
unity command console get -- --tail 40 2>&1 | grep -o 'Merged [0-9][^"]*\|Built [^"]*'
unity command console get -- --tail 30 --level warn 2>&1 | grep -E '"message"|^counts' | head -8
ls -la Assets/_Project/Scenes/SunCity.unity | awk '{print $5}'
