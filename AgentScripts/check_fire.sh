#!/usr/bin/env bash
# Runs the Fire land's probes and the hub's whole run one after another, each into its own file under the folder given.
cd "c:/Users/rohan/UnityProjects/Project Clover"
out="${1:-review/checks}"
mkdir -p "$out"
bash AgentScripts/runprobe.sh SunCourt SunCourtProbe 200 > "$out/fire_SunCourtProbe.txt" 2>&1
bash AgentScripts/runprobe.sh FireYard FireMechanicsProbe 200 > "$out/fire_FireMechanicsProbe.txt" 2>&1
bash AgentScripts/runprobe.sh SunCity SunCityProbe 220 > "$out/fire_SunCityProbe.txt" 2>&1
bash AgentScripts/runprobe.sh Hub RunProbe 200 > "$out/fire_RunProbe.txt" 2>&1
echo "all done" > "$out/fire_done.txt"
