# AgentScripts

C# files here are **not** part of the Unity project (the folder is outside `Assets/`). The Unity CLI compiles one in memory
and runs a named static entry point, with no domain reload:

```
unity command script run --file AgentScripts/Example.cs --entry Example.Run
unity command script run --file AgentScripts/Example.cs --entry Example.Run --args '[3, "Green"]'
unity command script run --file AgentScripts/Example.cs --dry-run true      # compile only
```

Use this for bulk construction (build a test scene, place a hundred props, wire fields), one-off probes in Play Mode, and
anything longer than a `script eval` one-liner. The code sees the project's assemblies (`Clover.Runtime`, `Clover.Editor`),
`UnityEditor` and the active editor defines. Exceptions come back with `file:line` in your source.

Rules: one static class per file, one entry point, delete the file when the job is done or move it into
`Assets/_Project/Scripts/Editor/` as a proper menu tool once it is worth keeping. Compiled leftovers (`*.dll`, `*.pdb`) are
git-ignored.

## Probes

`runprobe.sh <SceneName> <ProbeName> [maxPolls]` recompiles, opens the scene, enters Play Mode, runs
`AgentScripts/<ProbeName>.cs` (entry `<ProbeName>.Run`), waits for its `[Probe] done` line, prints the probe's lines and
any console warnings or errors, and stops Play Mode. Scene names: `Hub`, `Castle`, `Summit`, `CombatTest`.

| Probe | Scene | What it plays |
|---|---|---|
| `RunProbe` | Hub | one whole run through every door: gate, castle cleared, sealed way, both forms of the god, the choice, home |
| `HubProbe` | Hub | the sponsor's words, the stands and braziers sealed and open, the gate, a fall and waking |
| `CastleProbe` | Castle | fists, spell, surge, a finisher, the caster, a storm strike, the Katana, the rest fire and its menu, a fall |
| `CaptainProbe` | Castle | the Gate Captain: every attack, guard, perfect guard, second phase, finisher, death. Give it 110 polls: 70 end it before its last lines |
| `SummitProbe` | Summit | the swordsman, then the storm dragon: rising, attacks, a forefoot cut, the head finished, the rewards |
| `DragonProbe` | Summit | the storm dragon in detail: starts the godly form directly; entrance; the body in the cloud from four places and six pictures of the rim a second apart (`dragon_rim_<n>.png`; `DragonViewProbe` looks at it from every place); each attack standing still, having left the mark, rolled and perfectly guarded, with pictures `review/shots/current/dragon_<attack>_<n>.png`; each of the tail's six moves pictured six times across as it comes in a fight (`dragon_slither_<n>`, `flop`, `coil`, `scythe`, `flops`, `sting`), then tried from a set side (`StormDragon.TailFromDegrees`) so its timing repeats: standing still (which measures when its first blow lands), answered by where one stands, rolled, and guarded facing where it comes from; the coil cut open; the storm; the openings on head, tail (after the flop, the walking flops, the sting and the coil) and forefoot; stagger and finisher; an ordinary fight of 80 s left to himself against a player running round the floor, with the order of his attacks; fury; death; the consume choice. `AgentScripts/dragon_only.txt` may hold part names with commas between (attacks such as Beam or TailCoil, or Look, Storm, Openings, Stagger, Fight, Fury) to run those alone; leave it empty for everything. Give it 300 polls. Do not edit any file under `Assets/` while it runs: the editor recompiles in play and everything goes null |
| `DragonViewProbe` | Summit | what the fight's camera sees of the storm dragon from the middle, the four corners and the four sides. Parts, named in `AgentScripts/dragon_view.txt` with commas between (empty runs all): `Rim` (the body in the cloud, four frames 1.5 s apart from each of nine places), `Ends` (each tail move from each of its six bays, time all but stopped when it is furthest out, pictured from the eight places: no cut end may show), `Things` (each move from each bay aimed at a player near the posts, three frames, and the least room there was between the tail and a post, the head or a forefoot; below nothing means it went in). Add move names (`TailScythe`, `TailCoil`, ...) to keep Ends and Things to those. Frames are `review/shots/current/dragon_view_*.png`; `python AgentScripts/dragon_sheets.py </dev/null` (not `-I`: it needs Pillow from the user's packages) lays them out as sheets in `review/shots/current/sheets/`. All three parts take about ten minutes: give it 200 polls |
| `StormProbe` | none | reference only: the old colossus fight in detail. No scene holds the colossus now (the large robot is an idea held for the Metal area), so this probe has nothing to run against until that boss is built |
| `DragonSoakProbe` | Summit | holds the storm dragon doing its tail's moves (scythe, coil, slither, sting, flops) and then the beam, round and round with its body winding for forty seconds, for the profiler; `bash AgentScripts/profile_dragon.sh` runs it, records 300 frames to `review/checks/dragon_beam.data` and prints the summary |
| `UpgradeProbe` | CombatTest | the chart (close, the whole ring, a sealed mix), every first-slice Fire and Lightning upgrade, Storm Step and Plasma, each affliction measured on the dummy, the stats layer, effects that are only data, the worn spell, a mix mark bought and its first mark not double-laying, the eleven kits; then (V) the real trees: Wind, Ice, Earth and Poison each buy their root, the road to their first spell and one more mark with embers, and use them; (W) the gifts from the two debug shrines, a mix opened, a gifted spell worn; (X) each piece of the newer engine: a stat on a state, Hold, a stat against an affliction, a base-spell-only cost, a kill that knows what the dead carried, a count banked and spent with its HUD words, health as a price, a flask filled by parts, a shatter, a set-off with its depth, the last blow of a chain, the air, grown things, Heat's numbers. Give it 130 polls |
| `FormsProbe` | CombatTest | every spell form: gathers three dummies, takes the element that suits the form's sample spell, casts it, photographs it forming, landing and after (`review/shots/current/form_<id>_<n>.png`), and writes what each dummy lost and what was laid on it; then everything twice over on top of itself, and a fall with spells in the air. In Summit or Castle it casts each form once at the boss or nearest enemy instead. `%TEMP%/forms_probe.txt` may hold a comma list of ids to play only those, or `soak=<seconds>` to keep casting for the profiler. Give it 110 polls |
| `ChartScaleProbe` | CombatTest | the real chart (428 marks): for every element it learns the road to the first spell with a run's nineteen embers, photographs the tree close and at its crown (`review/shots/current/chart_<element>_close.png`, `_crown.png`) and, for Fire, Wind, Water and Dark, the whole ring (`_ring.png`); walks it with the stick; counts garbage per frame with the chart open and with afflictions ticking; then holds each for eight seconds so `diagnostics profiler_record` can be pointed at it. Give it 70 polls |
| `BrazierProbe` | Hub | the eleven braziers: which are lit, an element carried from one of the nine new ones, a photograph of the row |
| `CardProbe` | CombatTest | the cards over the form stones, element stone and both shrines (what each says, that only the near one shows, that a shrine's words follow the element), and that no rack stand is empty |
| `DumpMarks` | edit mode, `script run` | writes every mark of the chart as markdown tables to `docs/marks.generated.md`, for the tables in `docs/GUIDE.md` section 7 |
| `RackShot` | any, edit mode (`script run`, no play) | one picture of every weapon stand in the open scene, from a camera of its own that is thrown away: `review/shots/current/weapon_stands_<scene>.png`. A row along a wall is seen from in front, a ring from above |
| `WeaponsProbe` | CombatTest, Castle | the newer weapons (Massive Sword, Scythe, Armament, Great Spear, Great Hammer, Twin Knives, Great Bow, Chain Sickle; prefixes for `WeaponsProbe.only`: gsword scythe arm spear hammer knife bow sickle). The bow adds: arrows left standing by the stab and the shots, the fourth arrow, the swat on three arrows (39 and 56 by design; a held element's stacks add to the damage), a shot at nothing standing in the ground and an arrow in the ground set off, the kick shot's spring back, the full draw, Landmines, the guard's arrow, and Nailed on a dummy carrying arrows. The sickle adds: how far the chain's end went, the weight from range, the yank on the post (the player goes) and on the post made light (`TrainingDummy.Weight`; it comes), Entangle and the heavy button while it holds, the bind let go by time and by a roll, the guard's yank, and where Unwinding leaves the player. The knives add: where the player stands after the fifth cut and the crossing step (through the dummy, turned to it), one cut on the dummy's front against the same on its back (the dummy faces north, so the usual place to stand is its back), the cut out of the sidestep, the Fan of Knives from one step and from six metres, the guard's perfect window and a block's cost, and where Three Crossings leaves the player; `knife pass` in `WeaponsProbe.only` begins at the passing cut and traces it frame by frame. In Castle the knives fight first. In CombatTest, for each: the whole light chain, heavy, charged heavy, chain heavy (all four enders for the Armament), skill and finisher on a training dummy, with damage, stagger, distance and timing written beside the design's numbers; the sword's poise, the scythe's three bands, the crescent hanging and biting, Energy Reflect with and without mana; then the held element's spell cast with the weapon in hand and an affliction laid by a blow. Frames go to `review/shots/current/weapon_<name>_<move>_<n>.png`. The spear adds its haft band, the thrust from behind the guard (with a blow taken on the guard inside it), the vault, the couched charge and the Flying Spear three ways (into the dummy, into empty ground, refused with the stick); the hammer adds its toss timed out and back and its late perfect guard tried early and on time. In Castle it fights a soldier once with each weapon instead (any living enemy when the soldiers run out), the two newest first. To run only some weapons in CombatTest, write their clip prefixes on one line in `AgentScripts/WeaponsProbe.only` (for example `spear hammer`); delete the file to run them all. Give it 250 polls for all five (60 for two, 40 in Castle) |
| `FeedbackProbe` | Castle | death burn-away, finish mark, target brackets, bars, low-health edge, cut marks |
| `FinisherProbe` | Castle | the Katana's finisher and its camera |
| `PauseProbe` | Hub | the pause menu: every entry, pausing mid-attack and mid-roll, giving up, forgetting |
| `LoopProbe` | Hub | a second loop: the Katana and Lightning taken at the hub, the whole Lightning kit in the castle and on the summit, take and consume, each with the road home |
| `FallProbe` | Hub | a fall everywhere a run can end, the pause menu on the summit, in the air and in a finisher, giving up from the summit, the arena's standing up again |
| `TipsProbe` | Castle | the three first-fight tips |
| `SoakProbe` | any | random play for minutes; mode, seed and seconds are read from `%TEMP%/soak_config.txt` |
| `TourProbe`, `TitleProbe` | any | photographs of each scene's places and its title card, to `review/shots/current/` |
| `PerfProbe` | any | ten seconds of frame time, garbage and draw calls (in FireYard, SunCity and SunCourt it first sets up the busy moment: see the Fire sections below) |
| `SunCityProbe` | SunCity | the second pass of the sun-king's city: every room and vista pictured, every stair walked, the shortcuts from both sides, the mirror, the noon, the falls, the champion and the gate; see below |
| `SunCityOverview` | SunCity, edit mode (`script run`) | a raised picture of each district: `review/shots/current/suncity2_overview_<district>.png` |
| `FireMechanicsProbe` | FireYard | the Fire land's second pass, each mechanic forced and measured; see below |
| `BossFxProbe` | SunCourt | the look of the Fire bosses' attacks, each pictured with time stopped at its strongest; the sun parry; the Sun-Spear's throw and ray; see below |

Probes that touch saved progress forget it before and after, so the first real start still shows the first waking.

### SunCityProbe

`bash AgentScripts/runprobe.sh SunCity SunCityProbe 200` (about eight minutes). The scene is built by
`Clover > Build > Sun City` (the second pass, `docs/design/fire_levels.md`) and is not in the build list, so the runner
opens it by path. Every line that can be judged ends `OK` or `WRONG`, and the last lines are the tally. In order:

0. The sponsor's waking words read to their end; counts of each kind, buried mounds, water jars, mirrors, ladders,
   barred doors and shield walls.
1. The walk: the player is put down, everyone calmed, at 58 places from the Narrows to the champion's bowl, every room
   and the five vistas, with a frame at each: `review/shots/current/suncity2_<place>_<n>.png` (`narrows`, `vista_v1`
   to `_v5`, `pans`, `jam`, `bend`, `deck`, `basin`, `quay`, `bank`, `well`, `lamp`, `hearth`, `roofs`, `alley`,
   `market`, `gate`, `yard`, `tombs`, `avenue`, `haul`, `court`, `masons`, `scaffold`, `stair`, `warden`, `hall`,
   `chapel`, `passage`, `vestibule`, `bowl`). A vista line is `OK` when the camera's framing is in force there, or when
   someone has noticed the player (vistas give way then: V4 at the shield wall usually does).
2. Every stair and ramp walked with the keys from its foot, with the height reached: the quay stair, the bank ramp, the
   gangplank, the gully, the Lamp Street stair, the market stair and cart ramp, the gallery stair, the tomb stair both
   ways, the yard's north gate, the Levy Ramp, a colonnade side stair, the whole haul-ramp, the Spearman's Stair, the
   pylon into the hall, the aisle steps, the bowl's steps down.
3. The ladders: S1 used from below while it lies (must refuse), kicked down from the top, climbed down and up; the roof
   ladder and the scaffold ladder climbed. The three barred doors, each tried from its near side (must stay shut and say
   so) and then its far side (must open), then walked through. The cracked wall: a light blow must not break it, a
   heavy one must (`suncity2_hearth_1`).
4. The mirror M1 turned with the use button until it faces west: its beam must light the disc and the sun door must open
   (`suncity2_mirror_0`); then the player walks through into the chapel.
5. The noon: each district's rhythm (100, 70, 50 s, none in the hall), then a swell brought on at once with the player
   in the open in the Pans, under the boat on trestles, in a boat-shed, in the open on the avenue, under the court's
   colonnade and in the champion's bowl: burnt in the open, not in the planned shade, not in the calm bowl
   (`suncity2_swell_0`). Then the forced first swell at z 31.
6. Every carving and painting read to its end; the last painting wakes the tomb's wicks; the grave dish, the waterskin,
   the hearth's embers and the spear taken.
7. Each of the five fires lit (return point beside it). A rest at the quay, nine embers given, two of the basin's
   thirsting killed, a fall in the riverbed: the player must wake at the quay's fire, carry 0, and the embers wait on
   the two who were killed; killed again they pay them back. A fall on the masons' terrace after resting at F4 must
   wake at F4.
8. The champion: the gate tried while he stands (sealed); not aware of a player on the porch; four of his attacks forced
   on a still player (`suncity2_champion_<n>`); his death; the bowl's north stair walked; a fall in the bowl after resting
   at F5 (he stays dead); the gate opened, which loads the sun-court.

`AgentScripts/suncity_only.txt` holding `walk` runs only the walk and its frames (about two minutes); any other word skips
the walk and runs the rest. Delete it to run everything. The raised overviews of each district are made in edit mode by
`unity command script run --file AgentScripts/SunCityOverview.cs --entry SunCityOverview.Run` (the scene open):
`review/shots/current/suncity2_overview_<district>.png`.

`PerfProbe` has a `SunCity` branch: the Market of the Cold Oven with everyone within 60 m struck awake and a swell
brought on; a file `AgentScripts/perf_nosleep.txt` makes it measure with distance sleeping off.

### SunCourtProbe

`bash AgentScripts/runprobe.sh SunCourt SunCourtProbe 200` (about seven minutes). The scene is built by
`Clover > Build > Sun Court` and is not in the build list. Every line that can be judged ends `OK` or `WRONG`. In order:

1. The forecourt: the Great Spear is put in the hand, the rest fire is lit (it must say there is nothing to feed).
2. The gold line: the king's four lines are read to their end, the wall of heat must be closed.
3. The man: thrusts, reach, sweep, butt, throw (with the ray), planted spear and noon, each forced with
   `EnemyBrain.Force` on a player standing still (blows must land) and again answered (guard held for the pale ones,
   out of the mark for the red ones; nothing must land). Frames `suncourt_sunking_<move>_<n>.png`: 1 at the tell, 2
   at the blow, 3 at the end of a chain. Then the thrown spear standing in the floor and the ray
   (`suncourt_spear_stands_1`, `_2`), noon forced at a player standing in the obelisk's shadow (nothing must land),
   and half health (the planted spear and "It is always noon.").
4. His beating and the transformation, with the player south of the obelisk and the king north of it: he must be
   kept (not burnt away), get up and lift the spear with both him and the player on screen, still be drawn as the sun
   comes down into him, and be gone into it once it arrives, the sun over where he stood. Then the form's entrance
   (`suncourt_entrance_1` to `_7`, at 0.6, 1.5, 2.1, 2.6, 3.0, 3.6 and 4.3 s of it); after the roar the scarab and the
   player must both be on screen. The line must say the player was held, the bar was not shown before the roar, and
   the player was free after. The first time through, `fireend_transform_1` (lifting the spear) to `_5` (the flash as
   the sun arrives) are taken.
5. The form, with `SunBearer.OnlyWhenAsked` set so it does nothing but what is asked through `SunBearer.Queued`: Roll,
   Ram, Sparks, Furrows, Stoop, Feathers, Pass and Zenith, standing still and then answered (out of the lane or
   circle, guard held for the Ram, on unmarked floor for the falling things, walking with the shadow for the Zenith).
   `(last: ...)` names the last blow taken. Frames `suncourt_<move>_1` (tell), `_2` (blow), `_3` (after), and
   `suncourt_spent_1`. The line `perf over the Zenith` is frame time and garbage over the blaze with lanes hot.
6. The sun knocked out of the falcon's hold by 30 damage, pushed by a heavy blow, and fetched; the scarab knocked off
   it mid-roll (`suncourt_exposed_falcon_1`, `suncourt_exposed_scarab_1`). Then a perfect guard on the rolling sun:
   the player stands round the court from the scarab (a lane ends at the obelisk), the guard is raised facing the
   sun as it comes within 3.1 m; the line must count one perfect guard, one knock,
   the sun loose and no blow taken (`suncourt_perfect_guard_1`).
7. Balance broken and the finisher (`suncourt_broken_1`). 8. Fury: the blaze that opens it, the shadow's stop and
   turn-back (`suncourt_fury_blaze_1`).
9. A fall mid-fight: the line must say `AT THE FIRE`, form risen False, king health 480 of 480, begun False.
10. The fight again to the form's death: the disc cracking (`fireend_crack_1` to `_3`) and the man lying in the
    obelisk's shadow (`fireend_fallen_1`): laid, drawn, inside the shadow's strip, as near square to it as lying
    within 35 degrees of east and west allows, and on screen with the player.
11. His one line through the reading box (`fireend_line_1`): the speaker Khepharu and the words "I promised them gold.
    I gave them fire.", his fire not offered until it is read, and not said twice.
12. The Take over his body (`fireend_take_1` with the prompt, `_2` after): his fire within 1.2 m of him and offered as
    "Take his fire", taken with the interact key; Fire held, the gift owned, the Sun-Spear in hand, his own spear gone.
13. The sponsor's four lines read (none may name a boat, the barque or casting off) while the sun sets
    (`fireend_sunset_1` early, `_2` half-way, `_3` set): the shadow pointing east (90), the sun low, the key light
    orange, the west gate sunk and its wall open, the way out open as "Go west with the sun". A spell is cast.
14. Into the beam (`fireend_exit_1` on the road, `_2` at the gate's mark with the prompt), the interact key, and the
    hub must load.

`AgentScripts/suncourt_only.txt` may hold `End` to play only the transformation and steps 10 to 14 (about three
minutes, 70 polls), `Show` to play only the entrance and the death from two other places
(the player north-east for the entrance, south-west across the obelisk for the death: `suncourt_entrance_8` to
`_14`, `suncourt_death_8` to `_14`; about a minute), `Man` or `Form` to play one half, or `Hold` to go straight to a Pass and
then the blaze and hold each with the game all but stopped. `bash AgentScripts/aspect_court.sh` uses `Hold` to write
`review/shots/current/sheets/aspect_court_pass.png` and `aspect_court_blaze.png` at every screen shape. Delete the
file to run everything.

Last run of the whole probe (2026-11-17 evening, after the fire-not-light pass): 51 lines `OK`, none `WRONG`, console 0 errors and 0 warnings.
If the editor is not the active application while a probe runs, the keys it queues are dropped (the Input System's
background behaviour) and every walk, read and use fails; set `InputSystem.settings.backgroundBehavior` to
`IgnoreFocus` and `editorInputBehaviorInPlayMode` to `AllDeviceInputAlwaysGoesToGameView` with `script eval` for the
run and put them back after (they are not saved to disk). `PerfProbe` runs in this
scene as in any other (the forecourt, nothing risen): it is the baseline the `perf over the Zenith` line is read against.

`SunCityProbe` now ends in the sun-court: its last step opens the Gate of Noon, which in the editor loads
`SunCourt.unity` (see `Editor/Builders/EditorSceneLoads.cs`), and the probe goes with the player to say where it is.

## Trying the gifts by hand: the debug shrines

Only the Lightning god exists, so the practice yard (the `CombatTest` scene, built by `Clover > Build > Combat Test
Arena`) has two shrines beside the tall element stone, south of the middle, in front of the rack of form stones:

| Shrine | Where | What using it does |
|---|---|---|
| **GOD GIFT** (gold) | 3 m west of the element stone | gives the Signature of the element you hold, as taking that element from its god would; a gift that is a spell is worn at once |
| **CONSUME NEXT** (violet) | 3 m east of the element stone | marks the next element round the ring as consumed: the branch you share with it opens on the chart and its crown is given, as consuming that god would (no embers) |

To try any gift: use the element stone until you hold the element you want (it gives the next round the ring each
time), then a shrine. To try a mix from its other side, hold the later element and note that CONSUME NEXT always takes
the one after it, so the earlier side is reached by holding the earlier element. Embers for the chart: `script eval
"UnityEngine.Object.FindAnyObjectByType<Clover.Player.PlayerUpgrades>().Earn(40); return 1;"` in Play Mode, or the
rest fire's menu after felling dummies. Nothing the shrines give is written to saved progress.

## The gifts and crowns: `SignaturesProbe`

`bash AgentScripts/runprobe.sh CombatTest SignaturesProbe 200` plays every god's gift, every crown, Seven Flashes, Ball Lightning, Pawned Heart and
The Wave Stands Still at three dummies: it takes the element, is given the mark as a shrine gives it (nothing else
learned), does what the words ask (lights the surge with a short surge time, presses it again, guards, lands a
finisher, holds the spell button), photographs the telling moments to `review/shots/current/sig_<name>_<n>.png` and
logs what each dummy lost and carries. Names: fire, wind, ice, water, life, earth, metal, light, dark, poison, tempest,
blizzard, glacier, bloom, overgrowth, forge, halo, eclipse, blight, pyre, flashes, ball, heart, wave. The whole run is about four
minutes.

- `%TEMP%/sig_probe.txt` with a comma list of names plays only those; `soak=<seconds>` keeps raising and dropping
  Fire's sun for that long (for `diagnostics profiler_record`). Delete the file afterwards.
- In any other scene (`runprobe.sh Castle SignaturesProbe 60`) it plays fire, ice, tempest, halo and pyre at the
  nearest real enemy, only to see that nothing throws: the enemy fights back, so the numbers there mean little.
- By hand in the yard: the element stone until you hold the element, then GOD GIFT (the Signature) or CONSUME NEXT
  (the crown). Six Mirrors and Light the Pyres are worn at once (hold the spell button). Seven Flashes and The Wave
  Stands Still are bought on the chart.

## The marks finished last: `MarksProbe`

`bash AgentScripts/runprobe.sh CombatTest MarksProbe 60` (about two minutes) plays, at three dummies: **skydance**
(twice: every cut pressed in time, then no press at all), **beat** (Catching's leaping dose; In Time on and off the
beat), **lance** (Lance the Wound: a perfect guard makes all three bite), **volatile** (the beat stilled; Slow
Death's depth and keeping), **dead** (Dead Already's drain and refusal to rise) and **lotus** (Great Lotus closed by
five perfect guards), and **flash** (Flashpoint: a perfect dodge, then the boon gone after one blow). Frames go to `review/shots/current/mark_<name>_<n>.png`. `%TEMP%/marks_probe.txt` with a comma
list of names plays only those; delete it afterwards. In another scene (`runprobe.sh Castle MarksProbe 40`) it plays
skydance, beat and lance at the nearest real enemy to see that nothing throws; only Sky Dance's numbers mean anything
there (the dummies cannot be thrown, a soldier can).

`DumpMarks.Open` (`unity command script run -- --file AgentScripts/DumpMarks.cs --entry DumpMarks.Open`) writes
`docs/design/marks_open.md`, the list of marks that do not yet do all their words say, from
`docs/design/marks_open_notes.tsv`.

`MarksProbe` also plays the keystones and marks finished late on the night of 2026-11-16: **circle** (Closing
Circle), **eye** (Eye of the Storm; `EyeForm.HoldForProbe` stands in for the held button), **furnace** (Open
Furnace: four Burn on each dummy, then a perfect guard), **stroke** (Single Stroke: Static ten deep, one heavy
blow), **allatonce** (a frozen dummy shattered; a chilled one not slowed), **heartwood** (the base spell paid in
health, thirty mana mending ten), **unmoved** (Unmoved and Heavy Hand: struck inside a heavy swing) and **leaf**
(Leaf on the Wind: four dodges on three breaths, breaths won back), **tide** (Spring Tide: the base spell with all the mana, then with only its cost), **answer** (Master's Answer: the skill's price before, inside and after the window) and **well** (Deep Well: a flask drunk, then blows). **cleanbreak** needs a real enemy and is played
only elsewhere: `runprobe.sh Castle MarksProbe 40` plays circle, eye, furnace and cleanbreak at the nearest enemy
(cleanbreak kills it).

`ElementsProbe` plays each element's own moves. `bash AgentScripts/runprobe.sh CombatTest ElementsProbe 250` takes
each of the eleven elements in turn, taps its spell at three clustered dummies with the real button, photographs three
moments (`review/shots/current/element_<name>_tap_<n>.png`; Ice gets a fourth after its crystals are shattered), lights
the surge and photographs it (`element_<name>_surge.png`), and logs damage, stacks and the sum and direction of the
shoves each dummy was given (dummies are posts and do not travel). `%TEMP%/elements_probe.txt` may hold a comma list of
element names to play only those. In any other scene (`runprobe.sh Castle ElementsProbe 60`) it casts Riptide, Gust,
Grasp and Stone Rise at the nearest real enemy from four metres and logs how near and how far that enemy came.
`AfflictionsProbe` (`runprobe.sh CombatTest AfflictionsProbe 40`) lays each of the eleven afflictions on the dummies,
three at a time, and photographs them (`affliction_<n>.png`).

`MarksProbe` also plays the marks built to their designs on 2026-11-17 (frames `mark_<name>_<n>.png`): **rush**
(Wildfire Rush: distance, Heat, patches of ground fire, what the line does 1.2 s on), **sunspot** (three light blows
fed, a heavy blow sets it off; then one left alone until its time runs out), **scorched** (Scorched Earth: Kindle's
clock in and out of the player's own fire, mana from blows, a 4 s patch still burning at 5 s), **thunder** (Waiting
Thunder: nothing flies in 1.8 s, a bolt with each light blow, the rest with a heavy one), **lodestone** (Static four
deep on the first dummy; dummies are posts, so the shoves are counted, not travelled), **kiln** (Kiln Stance: the
blow caught, perfect guards counted, the mana), **twin** (Twin Break: three Burn and two Static, a perfect guard,
66 expected), **meltdown** (five and four does nothing; five and five bursts 3 s later) and **front** (Storm Front:
a heavy blow throws a free Gust, light blows none). In any other scene rush, sunspot and lodestone are also played at
the nearest real enemy, before cleanbreak, which kills it.

`SignaturesProbe`'s **fire** now follows Falling Sun along its road: it casts from eight metres so the dawn has a
road to roll, steps in close for the noon and the setting (`sig_fire_1` to `_7`), calls a second sun down while it
is still being rolled (`_8`), and has a third struck at noon with time left (it goes under and rises, `_9`, `_10`)
and again with too little left (it does not).

## Screen shapes (2026-11-17)

`capture game_view --width W --height H` only stretches whatever size the Game View already is, so a capture at
another shape proves nothing by itself. `GameViewSize.cs` sets the Game View to the fixed size written in
`AgentScripts/gameview_size.txt` ("1024x768"; "free" for Free Aspect), adding a size named "probe WxH" to the
editor's list when it is not there. `bash AgentScripts/aspect_shots.sh <what> [sizes...]` does that for each of
1280x720, 1920x1200, 1280x800, 1024x768, 2560x1080, 3840x1080, 2560x1440 and 3840x2160, captures
`review/shots/current/aspect_<what>_<w>x<h>.png`, lays them on `review/shots/current/sheets/aspect_<what>.png`
(`aspect_sheet.py`) and leaves the Game View at 1920x1080. The game must already be in the situation wanted and
holding it. `source AgentScripts/ev.sh` gives `ev '<C#; return ...;>'` (a `script eval`) and `key E` (a tap of a
key in the running game), for setting situations up by hand: the reading box, `PauseMenu.Instance.Open()`,
`UpgradeMenu.Instance.Open()` and `ToggleWide()`.

`bash AgentScripts/aspect_fight.sh` does the Summit: it plays as a new player (nothing saved), runs
`DragonViewProbe`'s new parts **Hold** (the slither from the east at its furthest with time all but stopped, the
player in the middle and then at the south rim, 45 real seconds each) and **HoldChoice** (the dragon felled, the
player before the take-or-consume choice for 45 s; then a fall), and captures every shape each time the console
says "holding ...". `SIZES="3840x1080 1024x768"` before it limits the shapes. The "death" sheet it makes is in fact
the waking in the Hub: the dark screen has gone by the time it is held.

`DragonViewProbe` also has **Finder**: the coil and the slither from the south-west with the player in that corner;
it counts the frames in which the tail is between the camera and the player (`StormDragon.PlayerHidden`) and pictures
the first (`dragon_view_finder_<move>.png`), where the gold ring should be seen through the tail. The probe reads its
parts from `AgentScripts/dragon_view.txt`. `dragon_sheets.py` no longer deletes the `aspect_*` sheets.

## The Fire land's yard: `FireProbe` (2026-11-17)

`bash AgentScripts/runprobe.sh FireYard FireProbe 130` (about three and a half minutes). The player has fists and no
element. Part 0: the spell and surge buttons with full mana (nothing is cast), then the rest fire (the chart stays
shut; "Rested. Nothing to feed yet."). Then each enemy in turn, with every other enemy put back where it stood
before each trial (`Only`): each attack forced on a player standing still (`Still`), on a held guard (`Guarded`) and,
for the red ones, rolled out of sideways (`Rolled`); then its special rule; then it is killed with fists and the
embers counted. The thirsting: a drink at 7.6 m draws the snatch; the flask comes back when its balance is broken
mid-drink; left alone it finishes and is whole again; a roll late in the tell saves the flask; the kneeling one is
dormant until 3 m. The wick: the hold left to run (five bites, 30), struggled out of (five presses), on 9 health (it
ends at 1), a heavy blow breaking the grab while it is shown; the dormant one struck (double) and woken by walking
up. The gilded by rank: the guard from the front, heavy and from behind, every attack, the planted ring. The priest:
`Sunlight.IsLit` at three spots, not found in the porch's shade, the double blow, found in the sun after 0.4 s, the
ray on a still and a walking player, lost in shade, the search and the giving up, found at 2 m, the flare lighting
the ground. Frames go to `review/shots/current/fire_<enemy>_<n>.png` and `fire_yard_<n>.png`.

`PerfProbe` knows the yard: `bash AgentScripts/runprobe.sh FireYard PerfProbe 30` wakes every enemy with the player in
the middle and reports ten seconds of frame times and garbage.

`FireDump.cs` writes the hierarchy, bounds and material counts of the Fire land's models to
`review/checks/fire_dump.txt`; it was used once at intake and can be deleted.

Heredocs with an apostrophe in them fail in this shell tool: write a patch script with the Write tool and run it
with `python -I script.py </dev/null` (never `python - <<EOF </dev/null`, which reads nothing).

### FireMechanicsProbe

`bash AgentScripts/runprobe.sh FireYard FireMechanicsProbe 200` (about a minute and a half). The second pass of the Fire
land's enemies in the yard's east part (`Clover > Build > Fire Yard` first if the scene is older than the kit). Each
part puts everyone back first, then forces and measures one thing, with frames
`review/shots/current/fire2_<what>_<n>.png` at its tell and its hit:

1. `ashen_swell_0/1`, `ashen_burst_2/3`, `wick_lit_0`: an ashen swells beside a dormant wick and a levy and bursts; what
   it struck by name, the player's health, whether the wick caught.
2. `ashen_early_0/1`: an ashen killed outright bursts where it fell 0.7 s later; its ring meanwhile.
3. `wick_spread_0/1`: a lit wick walks into a dormant one and lights it; footprints counted.
4. `noon_warning_0`, `noon_swell_1`, `noon_shade_2`: the swell brought on by `NoonHeat.SetClock`; health burnt in the
   sun, in the east awning's shade (none) and in the gate's calm rectangle (none from heat; the warden there may still hit).
5. `embers_carrier_0`, `embers_reclaimed_1`: a rest, two kills, a fall with 11 embers; who carries what; a carrier
   killed again; a second fall and what is forfeited.
6. `gold_splash_0`, `gold_cool_1`, `gold_many_2`: blows on a levy splash gold; hot gold burns, cooled gold slows
   (`PlayerController.GroundFactor`); 200 more splashes stay within the pool's 40.
7. `wall_0`, `wall_break_1`: three levies lock into a line; a light blow from the front is `Blocked`, a heavy one
   `Landed` and breaks the line; a light blow lands while it is open.
8. `pour_0/1/2`: the warden's pour; the player walking at him through it, and the nearest they got.
9. `buried_0`, `buried_burst_1`, `buried_lunge_2`: the breathing mound, the burst as the player comes within 3.2 m,
   and the lunge it opens with.
10. `jar_break_0`, `jar_drink_1`: a hurt thirsting goes to a broken jar, drinks, and its health and most after.
11. `beam_0/1`, `beam_lane_2`, `calling_0`: the priest's beam forced on a still player; the lane lit; the calling
    waking a levy put near him.
12. `mirror_0/1`: the mirror stand's beam burning a wick put in it; the player turning it (the use key) until the
    sun-disc is lit and the door goes.

13. `spear_0/1/2`: the spearman's throw forced on a player 8 m off; the spear standing in the floor; a light blow on
    him from the front while bare-handed; him fetching it. The health is read the moment after the throw, before the
    probe heals the player again (it used to be read a frame later, healed, which made the throw look harmless: 100 to
    100); the line ends `OK` when the throw cost health, and says what else was so at the frame of the throw (blows,
    dodges, blocks, flinch, leap, wards, spell walls, where the player stood). It waits for a throw after the count
    it read before forcing one, since the spearman may already have thrown during an earlier section.

`PerfProbe`'s FireYard branch now also brings the swell on and strews 30 splashes of gold round the player. Its
SunCourt branch (new) reads the king's words, fells the man, waits for the Sun-Bearer to stand in the fight, asks a
roll of it and measures the ten seconds after, with the form left to its own choices.

### BossFxProbe

`bash AgentScripts/runprobe.sh SunCourt BossFxProbe 150` (about four minutes for everything). The effects of
`docs/design/fire_boss_visuals.md` as built: each picture is taken with the time scale set to 0 for one frame, so it
shows the instant asked for, then time is set back to 1 (which also lets go of any hit-stop in force). Frames are
`review/shots/current/bossfx_<what>_<n>.png`. `AgentScripts/bossfx_only.txt` may hold any of these words to run only
those parts (delete it or leave it empty for all):

- `thrust` (`thrust_1` to `_3`, `reach_1`, each the frame after the blow lands, and `_after`), `sweep`, `plant`, `noon`
  (`noon_tell` with the column of fire standing over the filling circle, then each blow): Khepharu forced on a player
  standing still.
- `throw`, `ray`: his throw (`throw_1` in flight, `throw_2` standing and burning), the call (`ray_1`, `ray_2`: the stream of fire) and the
  strike (`ray_3`: the jet of fire), `ray_after`.
- `sunspear`: the player's Sun-Spear thrown at open floor; Khepharu is held on the line home while it stands; the ray
  must strike him (`sunspear_1` standing, `_2` the ray, `_3` back in the hand; the line ends `OK` or `WRONG`).
- then the man is killed and the form waits for what is asked (`SunBearer.OnlyWhenAsked`): `roll` (`roll_1`, `_2` mid
  roll, the furrow growing behind the sun, `_3` after, `_cooling` five seconds on), `parry` (a perfect guard on the
  rolling sun: `parry_1` at contact, `_2` in the slowed flight back, `_3` as it strikes, `_4` the form stunned and the sun
  rebounded; these four are taken without stopping time, and say the time scale they were taken at), `rising`
  (`rising_1` the column as the sun is thrown up, `_2` its climb trailing fire), `stoop` (`stoop_1` the hold,
  `_2` the dive's fall of flame, `_3` the ring of fire, `_4` after), `pass`, `zenith` (`zenith_fill` the floor filling red as the
  court goes hot and orange, `zenith_1` in the open, `_2` from the shadow: the shimmer squiggles and the still shadow), `sparks` and
  `feathers` (`_1`, `_2`: the things that fall, in the air over their marks).

`bash AgentScripts/rebuild_fire.sh` rebuilds, one menu at a time, every scene the Fire bosses' effects and the Sun-Spear touch
(Sun City, Fire Yard, Sun Court, Hub, Castle, Summit, the combat test arena). The arena's builder puts `CombatTest` into
the build list: take it out again afterwards so the list stays Hub, Castle, Summit. `bash AgentScripts/check_fire.sh <folder>`
runs `SunCourtProbe`, `FireMechanicsProbe`, `SunCityProbe` and the hub's `RunProbe` one after another, each into its own
file in the folder (about twenty minutes).
