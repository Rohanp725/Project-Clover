---
name: review-shots
description: Re-shoot every ShotStation, compare with the golden frames, describe each difference, and update goldens only on the user's word. Use after visual changes - materials, lighting, post-processing, models, UI - or when asked for a visual review.
---

# Review shots

Golden frames are the agreed look. The tool says which stations changed; a person decides whether the change is wanted.

1. Open the scenes the stations live in (`unity command scene open -- --path Assets/_Project/Scenes/<scene>.unity`) if they
   are not open; `scene list_open` to check.
2. `unity command editor invoke_menu -- --path "Clover/Review/Capture Stations"`. Read `review/shots/diff.md`.
3. For each station marked DIFFERENT or size differs: Read `review/shots/current/<name>.png` and `review/shots/golden/<name>.png`
   and describe the difference in words (what moved, what colour changed, what is missing). Flag anything that looks like a
   defect: z-fighting, missing texture (magenta), stretched UVs, text overflow, clipping, a light leaking through a wall.
   For many stations, hand the pairs to the shot-reviewer agent and relay its list.
4. Stations with no golden image are new: show the user the current frame and ask whether it becomes the golden.
5. Only when the user says the new look is right: `unity command editor invoke_menu -- --path "Clover/Review/Accept Current As Golden"`
   (it copies every current frame, so run the capture right before it). Golden frames are committed; current frames are not.
6. Adding a station: `unity command gameobject create -- --name "Shot_<name>"`, position it with `set_transform`, add
   `Clover.Core.ShotStation` with `component add`, set `shotName` with `component set_field`, save the scene with `scene save`.

Report: the stations that changed, with your description of each and the PNG paths; the ones accepted; the defects found.
