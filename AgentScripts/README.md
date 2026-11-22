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
any console warnings or errors, and stops Play Mode. Scene names: `Hub`, `Castle`, `Summit`, `CombatTest`, `SunCity`,
`SunCourt`, `ProvingGround`.

| Probe | Scene | What it plays |
|---|---|---|
| `RunProbe` | Hub | one whole run through every door: gate, castle cleared, sealed way, both forms of the god, the choice, home |
| `HubProbe` | Hub | the sponsor's words, the stands and braziers sealed and open, the gate, a fall and waking |
| `CastleProbe` | Castle | fists, spell (key 1, the West slot), surge, a finisher, the caster, a storm strike, the Katana, the rest fire and its menu, a fall |
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
| `WeaponLookProbe` | ProvingGround | the approved plain weapon models in the hand: for the Massive Sword (the hooked gsword_hooks, frames `weapons_gsword_hooks*`), Katana, Great Hammer, Twin Knives, Chain Sickle and Great Spear it equips each, stands the player facing east and, with time stopped, takes the game camera at rest (the Katana held in its guard), at the light attack's wind-up, and a close camera of its own 3 m off the right-front; first it writes where the Katana's old stick and its model lie in the hand socket. Frames `review/shots/current/weapons_<name>.png`, `_swing`, `_close`, `_swing_close`. `bash AgentScripts/runprobe.sh ProvingGround WeaponLookProbe 70` |
| `KnifeGripProbe` | ProvingGround | the Twin Knives as the game plays them: closes the guide with Escape, proves time runs, equips the knives and, with time stopped, takes a close picture of each hand at rest, at the first light cut's wind-up and in the heavy's dart, and the game camera at each (`review/anim/knife_grip/game_<moment>_<right|left|view>.png`). Its edit-mode partner `KnifeGripShots.cs` (`script run ... --entry KnifeGripShots.Run --args '["<tag>"]'`, not in Play Mode) poses every knife clip through the Forge and writes a sheet of hand close-ups per clip, `review/anim/knife_grip/<tag>_<clip>.png`. `bash AgentScripts/runprobe.sh ProvingGround KnifeGripProbe 40` |
| `BodySizeProbe` | ProvingGround | the standing size check for effects on bodies: the Knife Acolyte, Temple Soldier, Gate Captain and Champion of the Gate (no god's form can be let out here) held in a row in the castle pen; each chosen effect (afflictions laid by the game, a strike's `e3_strike_<element>@b` pieces, reactions by slug) put on all four, and at its moments, time stopped, a close picture of each body framed to its own height the way the game camera looks, and the row. It prints each body's anchors as built and as standing now. `AgentScripts/body_size_only.txt` may name effects with commas between (empty: the standard nine). Frames `review/shots/current/size/<effect>_<n>_<small|medium|large|largest|view>.png`; `python AgentScripts/body_size_sheets.py </dev/null` lays them out in `size/sheets/`. Film every new on-body effect here. `bash AgentScripts/runprobe.sh ProvingGround BodySizeProbe 90` |
| `WayProbe` | ProvingGround | the Katana's Way with the real keys, saved progress sandboxed: the guide closed with Escape and time shown to run, the Katana from the rack, Temper I and the Sheath bought from the Memories wallet; then parts (`AgentScripts/way_only.txt` names some with commas between, empty for all): `chains` (each aspect's chain played on the still dummy), `chord` (Before the Thunder on J and K), `answer` (a perfect guard, then the Riposte on J; again, Through the Gap on K), `stream` (a dodge in the second cut keeps the chain), `drums` (seven drums beaten on a penned soldier, spent by the next draw for +40), `zanshin` (a kill turns the player to the next enemy), `film` (one still each of Running Draw with Lightning and Lying Draw with Fire, `way_running_draw_lightning_<n>.png`, `way_lying_draw_fire_<n>.png`, and the finisher's held pose from the camera's side and from the left, `way_finisher_hold_<near|side>_1.png`), `body` (health 135, four flasks healing 55, one drunk) and `light` (the Light Body's flowing dodge, never a third). Every judged line ends OK or WRONG. `bash AgentScripts/runprobe.sh ProvingGround WayProbe 120` |
| `MemoriesProbe` | SunCity | Memories paid in play, saved progress sandboxed (the Fire Yard the plan names is removed; Sun City has a common kind in numbers and the Champion): the guide closed with Escape and time shown to run, Fire and Heat given so Heat's row shows, two of the commonest kind killed (+1 once), then the Champion (+4); for each, a still of the amber light leaving the body and one of the line under the Shards (`memories_<n>_light.png`, `_line.png`), the line's words checked and its box checked clear of Heat's row and under the Shards. `bash AgentScripts/runprobe.sh SunCity MemoriesProbe 60` |
| `FeedbackProbe` | Castle | death burn-away, finish mark, target brackets, bars, low-health edge, cut marks |
| `FinisherProbe` | Castle | the Katana's finisher and its camera |
| `PauseProbe` | Hub | the pause menu: every entry, pausing mid-attack and mid-roll, giving up, forgetting |
| `LoopProbe` | Hub | a second loop: the Katana and Lightning taken at the hub, the whole Lightning kit in the castle and on the summit, take and consume, each with the road home |
| `FallProbe` | Hub | a fall everywhere a run can end, the pause menu on the summit, in the air and in a finisher, giving up from the summit, the arena's standing up again |
| `TipsProbe` | Castle | the three first-fight tips |
| `SoakProbe` | any | random play for minutes; mode, seed and seconds are read from `%TEMP%/soak_config.txt` |
| `TourProbe`, `TitleProbe` | any | photographs of each scene's places and its title card, to `review/shots/current/` |
| `PerfProbe` | any | ten seconds of frame time, garbage and draw calls (in SunCity and SunCourt it first sets up the busy moment: see the Fire sections below) |
| `SunCityProbe` | SunCity | the second pass of the sun-king's city: every room and vista pictured, every stair walked, the shortcuts from both sides, the mirror, the noon, the falls, the champion and the gate; see below |
| `ProvingGroundProbe` | ProvingGround | the combat playtest's ground, with the real keys: the title read through, an altar's element, the rack (a plain weapon, the Storm Katana, the Sun-Spear, a "Not built yet" stand), both pens let out alone and together and cleared, a penned soldier struck, the dummies (still, fighting back, the afflicted row), a rest with unlimited Shards, marks and a god's gift bought, the chart walked round every tree, a mark's details (written and from data), the guide from the chart and the pause menu, a fall with nothing lost, the door into Sun City and the pause menu's way back. Frames `review/shots/current/pg_*.png`; every judged line ends OK or WRONG and the last is the tally. Set the Input System to ignore focus first (see the SunCourtProbe notes). `bash AgentScripts/runprobe.sh ProvingGround ProvingGroundProbe 120` |
| `P1aProbe` | ProvingGround | Phase 1, tasks 1.1 to 1.4, with the playtest on: Fire and the Katana, the Fire tree bought at a rest, the slots filled and the chart's "next slot"; a virtual Xbox pad casts each slot with LT held and a face button (no flask, skill, dodge or use answering), then keys 1 to 4; a slot refused while its clock runs; the clock runs with game time and is cut by a light blow (0.3), a heavy (0.6), a perfect guard (2) and a perfect dodge (1.5); a tap of LT lights the surge and a long hold does not, F on the keys; the skill on Y and on Q, a held heavy no longer the skill, X alone still the flask; mana 10 rising to 50 and stopping, 80 kept, a fight's start raising it to half, the fight ending after 8 s of calm; the controls page's Spells rows, Spell West rebound to Z (sandbox key), kept after the scene loads again and casting, then reset. Frames `review/shots/current/p1a_*.png`; every judged line ends OK or WRONG and the last is the tally. Set the Input System to ignore focus first (see the SunCourtProbe notes). `bash AgentScripts/runprobe.sh ProvingGround P1aProbe 170` |
| `P1bProbe` | ProvingGround | Phase 1, tasks 1.5 to 1.8, with the playtest on: the spell diamond small and large (L2 held), its pad names, the keys' names and a rebinding shown at once (made in memory, never saved); Fire then Ice: a slot spell lands and charges the blade, the blade shown on the weapon at rest and mid-swing at the air with the camera brought in (a swing that strikes nothing spends nothing), three blows at the still dummy each 1.2 times their move and laying the element, the fourth plain; the infused strike (LT + RB) and the deep strike (LT + RT) each landing, their 8 s clocks, the strike's own blow not cutting its own clock, a light blow cutting it 0.3 s, 20 mana paid and refused at 10, a strike never charging the blade; Frost Bite and Ice Through; the chord cancelling a light into a stand-in technique and doing nothing without one; Hunger's 25 s wait. Frames `review/shots/current/p1b_*.png` (time held still for each); every judged line ends OK or WRONG and the last is the tally. `bash AgentScripts/runprobe.sh ProvingGround P1bProbe 200` |
| `P2Probe` | ProvingGround | Phase 2, tasks 2.1 to 2.4, with the playtest on: the player arrives carrying Fire; the altars' prompts and uses (the Fire altar offers all in, the Lightning altar "Carry Lightning as second", used it carries Fire, then Lightning; with two carried the Fire altar offers Fire alone), both roots given; the rest fire opens the rest menu; the Hand set with the keys (West Flame Wave) and a virtual pad (North Thunderclap, Overdrive as the surge), Hand 1 kept and worn again, the Hand refused away from a fire, the chart reached from the rest menu and back; at the still dummy Flame Wave burns and charges the blade with Fire, Thunderclap (which lays nothing of its own) strikes and charges it with Lightning, a charged blow lays Burn and Static, plain blows Burn only (the probe first waits for Flame Wave's ground fire to go out), L2 + RB Fire's strike, L2 + RT Lightning's (no mana); then at the Fire altar: Fire alone, all in (L2 + RT the deep strike, 20 mana), alone again (the deep strike). Saved Hands and the Hand flag are cleared at the end. Frames `review/shots/current/p2_*.png`; every judged line ends OK or WRONG and the last is the tally. Set the Input System to ignore focus first. `bash AgentScripts/runprobe.sh ProvingGround P2Probe 200` |
| `P2ScheduleProbe` | SunCity | Phase 2, task 2.4: the Hand's slot schedule outside the playtest, played for real: Fire and four spells, only West and North filled; at a rest fire the Hand shows East and South shut (`p2_hand_shut.png`) and refuses them; the Champion of the Gate killed sets `elite.champion_gate` and East opens and fills; through the Gate of Noon into the Sun Court, the man and the Sun-Bearer felled set `god.fire` and South opens and fills. Sandboxed. `bash AgentScripts/runprobe.sh SunCity P2ScheduleProbe 120` |
| `BranchProbe` | ProvingGround | Phase 4, tasks 4.1 to 4.3, with the playtest on: Fire's Flame Wave and Lightning's Thunderclap learned mark by mark (Learn given, Empower, Shape, Master) and cast on the keys after each, the clocks read (the table's wait, the Empower's, a shape keeping it), the shape swapped with `PlayerBuild.ChooseShape` at a rest fire and refused away from one, each Master's timing (Flame Wave: within half a second of the charged blade's third blow no clock starts; Thunderclap: a perfect guard ends the wait and the next cast starts none), the branch out of force unslotted, and the chart filmed on both trees (`branch_*.png`). Every line ends OK or WRONG. Give it 150 polls |
| `ReactionProbe` | ProvingGround | Phase 3, task 3.2: the first six reactions on five copies of the still dummy set round a host on the east terrace (A and B 2 m west and east, C 2.2 m north, D 4.5 m south; made in play, gone with it). For each pair the player carries its two elements (`PlayerBuild.Attune`), the first affliction is laid on the host as a spell's, then the second: it must react once, write its name over the host (`ReactionWords`), rest (laid again at once, nothing), and do its effect: Flashover's discharge (A takes 21), Steam's puff (A takes 16, the Burn halved; again once its 3 s rest is out), The Ground Gives (sunk at half pace, held by a heavy blow and not a light one), Live Iron (a weapon blow arcs to the notched A, never the clean B or C, a spell's does not), Rot Roots (the three nearest joined and Seeded, a bite passed at half), Carried Embers (fires land north of D, away from the player, lighting the host and C; the host carried Unsteady and the embers' fire set nothing off). Then a reaction's laying starts nothing, and a real Flame Wave on a Soaked dummy is Steam. Sets the Input System to ignore focus itself and puts it back. Frames `review/shots/current/p3_*.png` with time stopped. `bash AgentScripts/runprobe.sh ProvingGround ReactionProbe 120` |
| `ReactionPoseDump` | ProvingGround | each designed reaction look and four composed ones (Thunder Gust, Red Hot, Windthrow, Mock Suns) played alone on a copy of the still dummy, with no afflictions laid, so their own hit art is out of the frame: a close-up strip of six moments (0.05, 0.15, 0.3, 0.45, 0.7 and 1.0 s) cropped about the body to `review/shots/current/reactpose_<name>.png`, and every shown part's mesh and world bounds about the body's feet at each moment to `review/checks/reaction_poses.md`. Set `Only` to a few names to film just those. (`ReactionArtProbe` also writes `reactart_<name>_close.png`, the same crop with the afflictions laid.) |
| `ReactionGifFrames` | ProvingGround | frames for a GIF of each of the 55 reaction looks, filmed in the game: a copy of the still dummy on the open floor at (28, 0, 0), the player still 3 m west facing east, the floor's painted words hidden; for each reaction it waits until the pool is empty, then with `Time.captureFramerate = 20` grabs every frame from 0.15 s before `ReactionLook.Play` to 0.4 s after the look's showing has ended, as the 960x540 window about the dummy cut at 1:1 from the 1920x1080 game view (set it first: `echo 1920x1080 > AgentScripts/gameview_size.txt` and run `GameViewSize.cs`; put `free` back after). Frames and `info.txt` (start, impact frame, designed or composed) go to `review/gifs/reactions/frames/<name>/`; `AgentScripts/reaction_gif_only.txt` may name a few. `python AgentScripts/reaction_gifs.py </dev/null` (not `-I`: Pillow and numpy are user packages) makes `review/gifs/reactions/react_<name>.gif` and `contact_sheet.png`. `review/gifs/` is git-ignored. `bash AgentScripts/runprobe.sh ProvingGround ReactionGifFrames 400`. Since 2026-11-20 night: each film runs at least 4.2 s, so what follows a reaction's 3 s aftermath is seen; at 3.5 and 4.0 s it logs what the struck one and the others carry and the ages of the looks on them (a look cast again shows a young age); with `AgentScripts/reaction_gif_close.txt` present each film is a close-up (the game camera drawn in to 5 m, a 1440x810 window about the struck one) to `review/gifs/reactions/close/`; and the only-list may name an affliction alone, `aff:Chill:2` (kind and depth), laid on every enemy stood |
| `LookPartDump` | (edit mode) | not a probe: `unity command script run -- --file AgentScripts/LookPartDump.cs --entry LookPartDump.Run` lists every part of the built element looks named in `AgentScripts/look_dump_only.txt` (slugs, comma separated; a slug takes its "slug@n" looks too): its mesh, window, flags, key pose and every key of its path, scale, spin and orbit, to `review/checks/look_part_dump.txt`. How the flames' wobble running on and the whirls' frozen bands were found (2026-11-20) |
| `ChainProbe` | ProvingGround | Black Chain in real play: three castle soldiers let out on the open floor at (28, 0, 0) as `ReactionGifFrames` stands them, the struck one at the middle, the second 5 m off along the player's way (3.5 m and more from where the recipe stood anyone), a third 7.5 m off (beyond the bind's 6 m); Cursed and Sundered laid for real, the bound one sent walking 1.5 m at 2.4 s. Logs every 0.5 s who the live chain (`BoundChains`) binds and how far its first and last links lie from the two chests; films 130 frames to `review/gifs/reactions/frames/black_chain_live/` (strip with `reaction_strips.py black_chain_live`). Game View 1920x1080 first. `bash AgentScripts/runprobe.sh ProvingGround ChainProbe 120` |
| `ElementGifFrames` | ProvingGround | frames for a GIF of each of the eleven elements' own moments, one element carried alone (`PlayerBuild.Attune`), the new effects on (F8 checked): the affliction laid three deep for 3 s on the main enemy and one to three deep on four more round it until it wears off, the base spell cast from its slot at the dummy 3 m away (its landing kept from charging the blade), the blade charged and its three blows landing (hit), the charged blade swung three times at the air facing away (then let go plain, as a spent one is), the infused strike (L2 + RB on a virtual pad) and the surge on F shortened to 4 s (the spell, infused and surge windows 1200x675, brought down by the script). What a moment lays on the dummy is cleared at once outside the affliction's own clip. Same set-up and capture as `ReactionGifFrames` (copy of the still dummy at (28, 0, 0), floor words, afflicted row, sparring dummy and screen HUD hidden, pools and particles empty and a second before each clip, the camera held on the player's starting spot (it still shakes), `Time.captureFramerate = 20`, 0.15 s before to 0.4 s after it has cleared, 960x540 from the 1920x1080 view: set it with `GameViewSize.cs` first). Frames and `info.txt` to `review/gifs/elements/frames/<element>_<moment>/`; `AgentScripts/element_gif_only.txt` may name `fire` or `fire_spell`. `python AgentScripts/element_gifs.py </dev/null` makes `review/gifs/elements/elem_<element>_<moment>.gif` (made smaller until under 4.9 MB) and `contact_sheet.png` (peak frames, a row an element). `bash AgentScripts/runprobe.sh ProvingGround ElementGifFrames 1200`. `AgentScripts/element_gif_style.txt` may hold `burn=`, `static=`, `overdrive=` (0 the code's look, 1 the Blender worker's, 2 both) and `tag=_kit` (put after each folder's name) to film the two makers' looks side by side; `python AgentScripts/element_strips.py <folder> ... </dev/null` lays eight frames of each film out as a strip in `review/gifs/strips/elements/` |
| `AfflictedCrowdProbe` | ProvingGround | every penned enemy let out round the player, kept still and alive, each carrying two of the looks changed by the owner's second effects review (Burn in real flame, Static, Cursed, Tremor), laid again every 2 s, while the player surges in Metal (Temper's steel) and then Lightning (Overdrive); prints `[Probe] soak start` for the profiler, then frame time, garbage a frame (editor included), the bodies burning in real flame and the steel level. `bash AgentScripts/profile_afflicted.sh` runs it, records 300 frames to `review/checks/afflicted_crowd.data` and prints the summary and the markers. Since 2026-11-20 night: exactly twelve enemies, each with two of Burn, Static, Cursed, Tremor, Unsteady and Poisoned, and the element looks' pool peak in its line |
| `BrazierGifFrames` | ProvingGround | every element brazier's contents loop, one 2.4 s loop each from a close camera of its own along the game's line and from the game camera (the player stood beside it), Fire also with Blender's tongues in place of the flame field; frames to `review/gifs/braziers/frames/<element>_<view>/`, `AgentScripts/brazier_gif_only.txt` may name elements; set the game view to 1920x1080 first; `python AgentScripts/brazier_gifs.py </dev/null` makes the GIFs and strips. `bash AgentScripts/runprobe.sh ProvingGround BrazierGifFrames 300` |
| `ChartRingShot` | ProvingGround | opens the chart, pulls back to the whole ring and says whether every drawn mark lies between the title bar and the keys' bar (`UpgradeMenu.WideFits`), then photographs it to `review/shots/current/chart_ring_all.png`. `bash AgentScripts/runprobe.sh ProvingGround ChartRingShot 30` |
| `FxProbe` | ProvingGround | the elements' approved effects in the game: for each of the eleven elements its six moments played through the game's own code on a copy of the still dummy (the affliction laid three deep, the tap spell cast with `PlayerElement.CastBase`, the hit as a charged blow landing, the charged blade swung at the air, the infused strike on a virtual pad's L2 + RB, the surge on F), each pictured at the game camera with time stopped (`review/shots/current/fx_<element>_<moment>.png`); then F8 to the old effects (`fx_fire_spell_old.png`) and back. `python AgentScripts/fx_sheets.py </dev/null` (Pillow) lays them out per element (`sheets/fx_<element>.png`), and with `compare` as `fx_compare_sheet.png`. With `%TEMP%/fxprobe_soak.txt` holding seconds it instead holds a busy moment for the profiler (six afflicted dummies, a surge, every element's spell in turn) after printing `[Probe] soak start`. Sets the Input System to ignore focus itself and puts it back. `bash AgentScripts/runprobe.sh ProvingGround FxProbe 200`. `ArtSheet` (edit mode, `script run --args '["all","top"]'` or `"game"`) draws contact sheets of the built element art, `review/shots/current/artsheet_<element>_<view>.png` |
| `ConditionProbe` | ProvingGround | Phase 3, task 3.3: enemy conditions on the castle pen's soldier: rooted (notices, does not walk, is not shoved, still strikes in reach; freed, walks), begins no attack, cannot guard (a light blow from the front lands, then is turned again), wind-up slowed by half (timed against the usual), sight cut to 2 m, damage taken and dealt shares, and Flashover laid mid-wind-up making it flinch. Frames `review/shots/current/p3_condition_*.png`. `bash AgentScripts/runprobe.sh ProvingGround ConditionProbe 90` |
| `PgShots` | ProvingGround, edit mode (`script run`) | raised pictures of the ground, the rack, the pens, the dummies, the gates and the plaza: `review/shots/current/pg_<part>.png` |
| `SunCityOverview` | SunCity, edit mode (`script run`) | a raised picture of each district: `review/shots/current/suncity2_overview_<district>.png` |
| `FireMechanicsProbe` | SunCity | the Fire land's second pass, each mechanic forced and measured on the open riverbed south of the waking spot; see below |
| `BossFxProbe` | SunCourt | the look of the Fire bosses' attacks, each pictured with time stopped at its strongest; the sun parry; the Sun-Spear's throw and ray; see below |
| `SunFightProbe` | SunCourt | the Sun-Bearer's fight as reworked on 2026-11-18: the man's ring of fire, the old endless stagger, the loose sun as a ball, the hot floor's rule, the burrow, each new attack, how far any piece reaches past the rim or into the obelisk, a player camping on the flank, and the busiest fury; see below |
| `OutfitProbe` | Castle, SunCity, CombatTest | what the player wears and whether it went on by a plain mesh swap or by bones looked up by name; pictures with time stopped of standing, running, the roll (its dive, the middle of the tuck and the rise, each with the hips' and head's height above the feet), attacking and close beside the nearest enemy with the game camera brought in (`review/shots/current/outfit_<scene>_<pose>.png`; the Castle skips the roll and attacks; in Sun City the poses are taken again closer, `_near`). In CombatTest it puts on all eight outfits in turn (`_<outfit>` on each name); in SunCity it then goes to the Hub and checks the Fire outfit stays on. It checks the real `clover.outfit.*` keys are untouched. Set the Game View to 1280x720 first (`GameViewSize`), then delete `clover.outfit.last.sandbox` and `clover.outfit.chosen.sandbox` after |
| `ClipFloorScan` | edit mode, `script run` | plays every baked player clip on a copy of the player model and lists those that take the body above the shins more than 5 cm under the floor, with when and which part; the whole table goes to `%TEMP%/clip_floor_scan.txt`. After the floor guard of 2026-11-18 only the greatsword and hammer rolls (a scarf tail, 8 to 9 cm) and the death (a hand, 6 cm) remain |

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

Last run of the whole probe (2026-11-18, after the overnight rework of the fight): 51 lines `OK`, none `WRONG`, console 0 errors and 0 warnings.
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

Phase 4 (2026-11-20) retired Flashpoint, Open Furnace, Clean Break, Heartwood, Spring Tide, Scorched Earth, Storm Front and
Unmoved, so their moves (flash, furnace, cleanbreak, heartwood, tide, scorched, front) are gone and unmoved plays Heavy Hand alone;
**sunspot** became **pitch** (Black Pitch, which replaced it), **reserve** plays Stored Charge (Deep Reserve is folded into it) and
**lodestone** plays Lightning Rod wearing its first shape. Where the paragraphs below name those moves, they describe the old marks.

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

## The Fire land's people: `FireProbe` (2026-11-17; moved to Sun City 2026-11-18)

The test yard it was written for was removed on 2026-11-18; it now runs in Sun City.
`bash AgentScripts/runprobe.sh SunCity FireProbe 250`. The player has fists and no
element. Part 0: the spell and surge buttons with full mana (nothing is cast), then the rest fire (the chart stays
shut; "Rested. Nothing to feed yet."). Then each enemy in turn, with every other enemy put back where it stood
before each trial (`Only`): each attack forced on a player standing still (`Still`), on a held guard (`Guarded`) and,
for the red ones, rolled out of sideways (`Rolled`); then its special rule; then it is killed with fists and the
embers counted. The thirsting: a drink at 7.6 m draws the snatch; the flask comes back when its balance is broken
mid-drink; left alone it finishes and is whole again; a roll late in the tell saves the flask; the kneeling one is
dormant until 3 m (in Sun City, the first thirsting found dormant that is not buried). The wick: the hold left to run (five bites, 30), struggled out of (five presses), on 9 health (it
ends at 1), a heavy blow breaking the grab while it is shown; the dormant one struck (double) and woken by walking
up. The gilded by rank: the guard from the front, heavy and from behind, every attack, the planted ring. The priest:
brought out of his roofed hall onto the open riverbed, `Sunlight.IsLit` at the sun spot and at the nearest real shade the probe finds (or a shade it makes, and says so), not found in the shade, the double blow, found in the sun after 0.4 s, the
ray on a still and a walking player, lost in shade, the search and the giving up, found at 2 m, the flare lighting
the ground. Frames go to `review/shots/current/fire_<enemy>_<n>.png` and `fire_city_<n>.png`.

`FireDump.cs` writes the hierarchy, bounds and material counts of the Fire land's models to
`review/checks/fire_dump.txt`; it was used once at intake and can be deleted.

Heredocs with an apostrophe in them fail in this shell tool: write a patch script with the Write tool and run it
with `python -I script.py </dev/null` (never `python - <<EOF </dev/null`, which reads nothing).

### FireMechanicsProbe

`bash AgentScripts/runprobe.sh SunCity FireMechanicsProbe 220`. The second pass of the Fire land's enemies, brought to
the open riverbed south of Sun City's waking spot (the test yard they were first tried in was removed on 2026-11-18). The
noon test gives the city, which never swells on its own, the yard's old rhythm for its length, and looks for real shade
near the riverbed and a calm spot in the hall. The shield wall is the city's own, with its own levies. Each
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

`PerfProbe`'s SunCourt branch (new) reads the king's words, fells the man, waits for the Sun-Bearer to stand in the fight, asks a
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
(Sun City, Sun Court, Hub, Castle, Summit, the combat test arena). The arena's builder puts `CombatTest` into
the build list: take it out again afterwards so the list stays Hub, Castle, Summit. `bash AgentScripts/check_fire.sh <folder>`
runs `SunCourtProbe`, `FireMechanicsProbe`, `SunCityProbe` and the hub's `RunProbe` one after another, each into its own
file in the folder (about twenty minutes).

### SunFightProbe

`bash AgentScripts/runprobe.sh SunCourt SunFightProbe 300` (about twelve minutes for everything). `AgentScripts/sunfight_only.txt`
may hold part names with spaces between; empty or missing runs them all, in this order. Frames are
`review/shots/current/sunfight_<what>.png`, each taken with time stopped for one frame.

- `Ring` (before the form rises): the man's planted spear forced, pictured just after it rises (`ring_1`), standing
  (`ring_2`), dying round the ring (`ring_dying`) and as char (`ring_char`).
- `Stagger`: the sun knocked away, then thirty seconds of the great spear's heavy blow on whatever can be struck every
  0.55 s, the form's health kept up: how many times its balance broke and its sun was knocked, and the order of what it
  did (`*` marks the poise). This is the loop the owner found; before the rework it read Exposed > Staggered > Exposed > ...
- `Sun`: the loose sun struck light with no flask and no mana (nothing may be earned; the aim line must show, `aim_1`),
  then heavy straight at the scarab (`rolled_1`, `shellfire_1`, `_2`): the strike, the balance and the health.
- `Hot`: a roll's lane: when its edge and then all of it stop burning, the bites walking straight across and rolling
  across, frames at 0.45 s (`lane_0`), 2.6 s (`lane_3`), as the edge goes out (`lane_cooling`) and as char
  (`lane_char`); then the furrows (`furrows_hot`).
- `Burrow`: knocked off its sun, which is pushed away, it must dig (`dig_1`), go under a ridge (`ridge_1`) and come up
  (`emerge_1`, `_2`) without ever vanishing; then the Burrow attack chasing a walking player (`burrow_chase`,
  `burrow_locked`, `burrow_erupt`, `burrow_halfout`).
- `Attacks`: the Ricochet (`ricochet_1` the path, `_2`, `_3` the bounces; a blow on the cool shell), the Flares standing
  still (`flares_1` to `_3`) and in a gap, the Ember Gust standing still (shoved) and with the guard held (holds ground;
  `gust_1`, `_2`), the Talon Grab caught (`grab_1`, `grab_2` up in the talons; the player must be free and walking
  after) and rolled out of (`grab_missed`), the Sunfall (`sunfall_1` to `_3`).
- `Clip`: every attack from three places by the rim and one by the obelisk, then the exposed tumble three times, the
  broken balance at the rim three times and the falcon brought down three times: for each, the worst reach of any piece
  (the corners of each piece's own box) past the rim below the wall's top, into the obelisk, and under the floor (not
  counting digging in or coming up). The last line is the worst of all. `AgentScripts/sunfight_clip.txt` may name only some attacks (spaces between), and `NoStates` to leave out the tumble and the falls; the whole part takes about twenty-five minutes.
- `Camp`: twenty seconds keeping three metres off the scarab's right flank and attacking (three light blows to a
  heavy), the form left to choose: time on the flank, blows landed, health it lost, what struck the player and the
  order of what it did.
- `Perf`: after the fury's blaze, thirty seconds of its fieriest run asked in turn (Furrows, Flares, Ricochet, Sunfall,
  Pass) with the player running round the court: frame time, garbage and the most burning patches at once.
  `bash AgentScripts/profile_court.sh` runs this part alone and records 1200 frames of the profiler in the middle of it
  (`review/checks/sunfight/court_fury.data`), printing the summary; the probe's own garbage figure is then swollen
  by the recording, so read the profiler's.

The last line says how many lanes were moved aside, and how many laid without fire, because they would have shut the
player off from the shape.

## The Fire land's world pass (2026-11-18): `World1Shot`, `world1_frames.sh`, `profile_city.sh`

- `World1Shot` (any scene): puts the player at each line of `AgentScripts/world1_shots.txt` (`name x y z`) with everyone
  calmed and writes `review/shots/current/world1_<name>.png`; no file means one frame where the player stands.
  `bash AgentScripts/runprobe.sh SunCourt World1Shot 30`.
- `world1_frames.sh before|after [folder]` copies SunCityProbe's walk frames (`suncity_only.txt` holding `walk`) into
  `review/shots/current/world1_<district>_<spot>_<label>.png`; the before frames of the pass are kept in
  `review/shots/before_world1/`.
- `profile_city.sh <label>` runs PerfProbe's SunCity branch (the market, everyone near awake, a swell) and records 600
  profiler frames over it into `review/checks/world1/city_<label>.data`, printing the summary. PerfProbe now also
  reports set-pass calls and triangles.

## The Fire land's world pass, second half (2026-11-18): `rebuild_city.sh`, `CityCount`

- `rebuild_city.sh [court]` opens the Hub first (with Sun City open, every recompile remakes its merged scenery), recompiles,
  rebuilds Sun City (and the court with `court`) and prints the merge's line and any warnings.
- `CityCount` (edit mode, the scene open, `script run`) writes to `review/checks/world2/count.txt` how many objects,
  renderers, triangles, submeshes and materials the open scene draws, by group. Merged scenery and laid paving are made
  when the scene opens and hidden, so they are not counted there.
- `profile_city.sh <label> [folder]` now takes the folder under `review/checks/` (default `world1`; the second half used
  `world2`). `world1_frames.sh - review/shots/current world2` names the walk's frames `world2_<district>_<spot>.png`.

## The owner's Sun City bugs (2026-11-18): `CityScan`, `CityFixProbe`, `CityLook`

- `CityScan` (edit mode, Sun City open): `unity command script run --file AgentScripts/CityScan.cs --entry CityScan.Run --args '["label"]'`
  writes `review/checks/cityfix/scan_<label>.md`: dressing that reaches past the surface holding it up (roof tiles left
  out: their spouts and eaves stand out by design), pieces lying on a box that thins out without thinning out with it,
  anything in the body's space (0.3 to 1.8 m) over floor the player can reach, pieces standing more than 0.2 m inside a
  wall, a building or the face of a rise, and, from a flood over every floor the player can walk to (stairs, ramps,
  drops and ladders), every place where they can walk off into nothing, and every drop of more than 2 m.
- `CityFixProbe`: `bash AgentScripts/runprobe.sh SunCity CityFixProbe 90` plays the fixes: a thirsting's blows by hand
  at a player on the barque's deck over it (none may land) and beside it (they must), and the player's own chain both
  ways; the boat-sheds' backs; the old open edges (the whole fishers' bank walked south from its ramp, the banks' ends,
  the quay's west end, the lane level's east gap, the roofs' east edge); the safety net (`FallCatch`) with the player
  put under the land; the barque's shrine, prow and stern walked into. Frames `review/shots/current/fix_<n>_<what>.png`.
- `CityLook` (edit mode, any scene): one frame from a point toward a point, `--args '["name", fx, fy, fz, ax, ay, az, fov]'`,
  to `review/shots/current/look_<name>.png`.

## The controls (2026-11-18): `ControlsProbe`, `controls_probe.sh`

`bash AgentScripts/controls_probe.sh` (about six minutes) plays three sandboxed plays and puts the editor back after
(Hub open, background input settings as they were, sandbox flag and sandbox bindings cleared). It prints whether the
owner's own bindings key (`clover.controls.bindings`) is held before and after; nothing in it writes there.

1. Hub: a virtual Xbox pad (`XInputControllerWindows`) reads the waking words with A, walks with the stick and the
   d-pad, strikes with RB, opens the pause menu with Menu and walks it with the d-pad and the stick, opens the controls
   page (`controls_xbox.png`), rebinds the flask to Y (a clash with the spell, swapped; `controls_rebind_pad.png`),
   asks for the spell's key and waits for `simulate_key T` (`controls_rebind_listening.png`, `controls_rebound.png`),
   then walks the chart and leaves it (`chart_xbox.png`).
2. CombatTest: the rebinds came back from PlayerPrefs; a walk held with `simulate_key W`; the cursor put beside a
   dummy with `simulate_pointer` and a click (`controls_mouse_aim_before.png`, `controls_mouse_aim.png`: the attack
   must turn onto the dummy), then at open ground (`controls_mouse_open.png`: it must turn to the cursor); the pause
   menu walked with keys (`controls_keys_walked.png`); the page in keys names (`controls_keys.png`); reset to defaults
   through the page (`controls_reset_ask.png`, `controls_reset_done.png`).
3. Hub again: nothing kept after the reset.

The CLI's `simulate_key` and `simulate_pointer` presses reach held state (a held W walks; a rebind's listener hears
T) but never count as `WasPressedThisFrame` in the player loop while the editor is in the background, so attacks
and menu presses are queued from the game's own frames (`ControlsProbe` parts `click` and `keys`). A
`simulate_pointer` move jumps without a delta, so mouse aim begins at the click rather than at the move.

## Phase 2 (2026-11-18): the rest menu and the altars in the probes

A rest fire now opens the rest menu (Set the Hand, The chart, Leave the fire) and stops the world, so a probe that
rested and expected the chart now chooses "The chart" there with the keys (`ChartFromRest`) and leaves the rest menu
once the chart closes back to it (`LeaveRest`): `P1aProbe`, `ProvingGroundProbe`, `CastleProbe`, `LoopProbe` and
`RunProbe`. `SunCityProbe` leaves the rest menu at once when a fire opens it (a scaled wait under it would never end),
`SunCourtProbe` closes it directly, `FallProbe` falls with it open, and `SoakProbe` counts it and the Hand as menus. The
Proving Ground's altars now set either attunement (carrying one element, another altar adds its element second), so the
probes that want one element use `HoldAlone`, which uses the altar again until its element is carried alone.
