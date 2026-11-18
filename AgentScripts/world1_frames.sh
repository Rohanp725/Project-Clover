#!/usr/bin/env bash
# Copies the SunCityProbe walk's frames of the Fire land world pass into review/shots/current/world1_<district>_<spot>_<label>.png.
# usage: bash AgentScripts/world1_frames.sh <before|after> [source folder]   (before frames come from review/shots/before_world1)
cd "c:/Users/rohan/UnityProjects/Project Clover"
LABEL=${1:-after}
FROM=${2:-review/shots/current}
for pair in d1:narrows_0 d1:pans_0 d1:pans_1 d1:bank_1 d1:bank_0 d1:quay_0 d1:bend_0 d1:jam_0 d1:basin_0 d1:well_0 \
            d2:lamp_0 d2:lamp_1 d2:lamp_2 d2:hearth_0 d2:alley_0 d2:alley_1 d2:roofs_0 d2:roofs_1 d2:market_0 d2:market_1 d2:market_2 d2:gate_0 d2:yard_0 d2:yard_1 d2:yard_2 \
            d3:avenue_0 d3:avenue_1 d3:court_0 d3:masons_0 d3:masons_1 d3:warden_0 d3:warden_1 d3:stair_0 d3:hall_0 d3:hall_1 d3:passage_0 d3:bowl_0 d3:bowl_1; do
  district=${pair%%:*}
  spot=${pair#*:}
  [ -f "$FROM/suncity2_$spot.png" ] && cp "$FROM/suncity2_$spot.png" "review/shots/current/world1_${district}_${spot}_$LABEL.png"
done
ls review/shots/current/world1_*_$LABEL.png | wc -l
