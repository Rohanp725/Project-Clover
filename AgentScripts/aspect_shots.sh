#!/usr/bin/env bash
# usage: aspect_shots.sh <what> [sizes...]   Sets the Game View to each screen shape the demo must hold (GameViewSize.cs), captures it
# into review/shots/current/aspect_<what>_<w>x<h>.png, lays them on one sheet (review/shots/current/sheets/aspect_<what>.png)
# and puts the Game View back to 1920x1080. The game must already be in the situation wanted, and holding it.
cd "c:/Users/rohan/UnityProjects/Project Clover"
what=$1; shift
sizes="${@:-1280x720 1920x1200 1280x800 1024x768 2560x1080 3840x1080 2560x1440 3840x2160}"
for s in $sizes 1920x1080; do
  w=${s%x*}; h=${s#*x}
  echo $s > AgentScripts/gameview_size.txt
  unity command script run -- --file AgentScripts/GameViewSize.cs --entry GameViewSize.Run 2>&1 | grep -iE "error|exception" | head -3
  timeout 2 ping -n 2 127.0.0.1 >/dev/null
  [ "$s" = "1920x1080" ] && [ -z "$KEEP1080" ] && break
  unity command capture game_view -- --output-path review/shots/current/aspect_${what}_$s.png --width $w --height $h >/dev/null 2>&1 || echo "capture failed $s"
done
python AgentScripts/aspect_sheet.py $what </dev/null
