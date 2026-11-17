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
| `PerfProbe` | any | ten seconds of frame time, garbage and draw calls |

Probes that touch saved progress forget it before and after, so the first real start still shows the first waking.

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
