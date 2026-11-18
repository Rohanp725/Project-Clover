#!/usr/bin/env bash
# Rebuilds every scene the boss effects and the Sun-Spear touch, one menu at a time, waiting for the editor between them.
cd "c:/Users/rohan/UnityProjects/Project Clover"
P="C:\Users\rohan\UnityProjects\Project Clover"
for scene in "Sun City" "Sun Court" "Hub" "Castle" "Summit" "Combat Test Arena"; do
  unity status --until-ready --project-path "$P" >/dev/null 2>&1
  start=$(date +%s)
  unity command editor invoke_menu --timeout 600 -- --path "Clover/Build/$scene" 2>&1 | tail -1
  unity status --until-ready --project-path "$P" >/dev/null 2>&1
  echo "$scene built in $(( $(date +%s) - start )) s"
done
unity command console get -- --tail 40 --level warn 2>&1 | grep -E '"message"|^counts' | cut -c1-300 | head -12
