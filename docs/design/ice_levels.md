# The Ice land's levels (first pass)

Written by Claude on 2026-11-19 as the Ice land's counterpart to `fire_levels.md` and `water_levels.md`, before any of
the land is built. **Everything here is Proposal (Claude)** unless it is marked **Decided** with its source. The land's
myth, people, elite and fight are `WORLD.md`, "Ice's land: the huntress of the snows"; its look is `LOOKS.md` section 6;
the rules every land follows are `WORLD.md`, "Level design rules for every land (decided 2026-11-18, evening)". The
enemies' numbers (health, telegraphs, damage) belong in a later `ice_land.md`, as `fire_land.md` section 4 holds Fire's;
this plan states only what each room needs each enemy to do, with first values where a room's measures depend on them.

Axes as in Fire: x east, z north, y up; north is up the screen. Coordinates are metres. "Floor y 7" is the walking
height. The land scene is `HuntersFjord` and the god's lake is its own scene, `SkullLake` (names proposed), as
`SunCity` and `SunCourt` are two. The player wakes at (0, 0, 0) on the fjord ice.

---

## 0. What the owner has decided, and how this plan reads it

| Decided | Source | How this plan uses it |
|---|---|---|
| The map is a funnel climbing from the harbour through the forest to the giant's bones | `WORLD.md`, level design rules, 2026-11-18 evening | The fjord is the funnel's mouth (60 m wide), the forest valley its throat (30 m), the ribcage a single climb, the neck a ridge 6 m wide (section 2). The shape is also the hunt's: the Norse mountains are full of reindeer drive systems, guide fences converging on a pit, and this land drives the player up into the skull |
| The verb is *stealth and sight lines*: the player breaks the huntress's line of sight and can strike back; a hit makes her fall back | same | Section 1.2: her sight is a line from a perch to the player's chest; hard and soft cover break it; three ways to strike back; a hit empties the room of her |
| Her arrows on the walk are heavy but always warned: about a second ahead, a glint on a ridge and a thin line on the ground show where the arrow will land; a hit deals heavy damage and pins the player so the pack catches up | same | Section 1.2 and 1.3. The camera never shows a ridge more than 6 m above the player (1.1), so the glint is read as drawn where her line meets the edge of the picture, joined to the ground line (question 1) |
| Rest fires in Fire's rhythm: one every 6 to 8 minutes of a new player's walk, always one at the god's door, a shortcut loop per district back to its fire, 5 to 7 fires | same | Six fires, three shortcuts and a side loop (sections 2 and 4) |
| Two kinds of optional discovery: "see it, then find the way" (the main kind, holding the land's plain weapon and lore) and one optional trial invented for this land, each land's different | same | The jarl's ship (the plain Great Bow), the frozen heart; the trial is the Sleepers' Hollow, a stealth trial (3.4) |
| Up to three walkable levels per district, joined by ramps, stairs and ladders seen from above; nothing walkable under a roof; roofs and overhangs only fade | same | Every district names its three levels; every perch is an open platform; the town's halls are roofless or shut |
| Being hunted is the land's main rule; one pattern per district: the warning shot and arrows from the masts (harbour), herding with the pack toward snares a careful player can see (forest), the sight-line gauntlet from rib to rib (bones) | `WORLD.md`, "Ice's land", the land (the patterns are Proposal there; the districts' shapes follow them) | Sections 3.1 to 3.3, one pattern each |
| The enemies: the pack (on all fours, in groups; one howls, the others flank), the brittle (ice flesh that shatters into shrapnel under heavy hits), the buried (seen only as a moving line of disturbed snow); the elite is the pack's alpha | `WORLD.md`, "Ice's land" | Section 1.5, each with a mechanic of its own (the owner's bar: no reskins) |
| Tracks in the snow appear only in the wolf phase | `WORLD.md`, `LOOKS.md` | No enemy leaves footprints on the walk. Trodden paths, ski runs and the buried's moving line are not tracks |
| The fight: through the horns of the skull to a frozen lake in a crater; she skates; the reflect; the wolf hidden in a blizzard; the lake cracks only once the blizzard has mostly thinned; the wolf crashes through and a longship rises; the player leaves on it alone | `WORLD.md`, "The fight"; "Rides out of each land" | Section 6 gives the approach, the lake as a room and the ship's place under the ice |
| The alpha teaches the Fists' gated technique, Bear Them Down: a pounce out of the pack and the snow, answered by a late dodge toward him | `steel/weapons.md` section 9 (Proposal, approved with the steel designs on 2026-11-18) | Section 5 |
| The land's look: overcast mountain daylight softened by blowing snow; blood red for the hunt only, a deep crimson | `LOOKS.md` section 6 (Decided for the light; the rest Proposal) | Her line on the snow is the hunt's crimson, which is also the game's red for what cannot be guarded |
| Ice is in tier 3; a clean pass about 45 minutes for a new player (Proposal) | `WORLD.md`, "The order of the climb", "How a climb is structured" | Section 8: 44.5 |

One conflict to settle: `LOOKS.md` places the sun's bright patch "low over the fjord mouth"; the approved mood frame
`ice_warning_shot` puts it at the upper left. In this plan the fjord's mouth is south, behind the camera, so the plan
follows the frame: the sun stands in the north-west, behind her harbour perches (1.2).

---

## 1. Principles

### 1.1 What the camera sees: what changes for this land

The camera is Fire's (`fire_levels.md` 1.1): pitch 55 degrees, 12 m, a vertical field of 40 degrees; the picture
reaches 7.2 m north and 4.3 m south of the player on flat ground and is 15.5 m wide at the player. Landmarks are rooms
the player arrives in or vista zones; the south-side rule, faded roofs, enemies that walk straight at the player with
no navmesh and ranged attackers kept in frame all hold as written there. What this land must add is the sniper.

**Where a raised thing is on screen.** Working Fire's numbers through for a point H metres above the player's floor:

| Height above the player | In frame if it is between | And east or west of the player by at most |
|---|---|---|
| 0 | 4.3 m south and 7.1 m north | 7.8 m (at the player's own z) |
| 2 m | 4.8 m south and 4.2 m north | 6.7 m |
| 4 m | 5.4 m south and 1.4 m north | 5.6 m |
| 6 m | 5.9 m south and 1.5 m **south** | 3.5 m |
| 8 m | 6.4 m south and 4.3 m south | 1.7 m |

(Top edge: z = 7.1 - 1.43 H; bottom edge: z = -4.3 - 0.27 H; half-width 0.65 of the depth along the view.) The camera
itself stands 9.8 m up, so anything near that height is never in the picture.

What follows from it, and is held to below:

1. **She is never on screen while she shoots.** Her perches are 6 to 12 m above the ground she watches and 15 to 45 m
   away. By the table, nothing more than about 5 m up is seen unless it is within a few metres of the player, so a
   sniper on a mast or a ridge is out of the picture by geometry, not by choice. Every tell must therefore be on the
   floor, at the edge of the frame, or in sound. She is seen only (a) when the player reaches her perch, (b) in vista
   zones, which never open while she is drawing, and (c) at two scripted glimpses (V4, V6).
2. **The glint at the edge.** The decided "glint on a ridge" is drawn where her line of fire leaves the picture: a
   small cold star (ice cyan-white with a crimson fleck, the fletching) pinned to the frame's edge on her bearing, and
   from it the thin crimson line runs across the snow to where the arrow will strike. The eye goes from the star down
   the line to its end. Where the perch itself is in frame (a reached perch, a vista), the glint is drawn on her, in
   the world. With it, the draw's creak comes from her side of the stereo field. (Question 1 asks whether to add a
   quiet mark of her state at the same edge, as Metal Gear Solid V's suspicion arc does.)
3. **The arrow itself is not the tell.** It crosses the picture in about a tenth of a second. The line is the warning.
4. **Cover is north of the player.** The land rises north and her perches stand ahead and above, so what hides the
   player from her mostly stands north of them, where the camera sees it. The south-side rule therefore costs this
   land little: the ground to the south is lower and behind.
5. **The wind blows down the funnel**, from the mountain to the fjord (north to south), as cold air drains down a
   valley. Snow streams across the floor toward the camera (`LOOKS.md`: streams show the wind's direction), and drifts
   pile on the lee, the **south** side of everything. So a trench dug east-west through the town has a high north wall
   (the drift against the house beyond) and a low, scoured south wall: the south-side rule and the weather agree.
6. **Pines.** The forest is of narrow spruce, trunks bare to 3 m; every canopy fades (`Fadeable`) within 6 m of the
   player, and no canopy stands within 4 m south of a trail. The trail and the clearings are the floors; deep snow under
   the trees is walkable but slow (1.4).
7. **The pack stays in frame.** A pack member circling south of the player keeps within 4 m (the bottom edge is 4.3 m
   away); the howler stands east or west of the fight, at most 6 m north and 1.5 m up.

### 1.2 The huntress on the walk

She is one person. She moves between perches off screen, ahead of the player, and hunts only in the rooms listed for
each perch (section 2's table). In the rest of the land (the trenches, the ravine floor, the alpha's ground, the
hollow, the neck, every fire's surroundings) she cannot see the player, by the room's making, not by a switch.

**Her sight.** She sees the player when a straight line from her eye to a point 1.2 m above the player's feet is clear,
and the player is within her **gaze** (a cone of 70 degrees on where she is looking) and within her range (45 m;
30 m in the forest's falling snow). What breaks the line:
- **Hard cover**: rock, hull, mast-foot, longhouse, bone, a trunk at least 1.2 m thick, a dormant brittle (until it
  breaks). Always.
- **Soft cover**: a snow veil (the spindrift in the bones, a dumped spruce load, the alpha's squall), a hanging sail, a
  rack of hides. Unless the player is **marked** (below).
- **Under her feet**: within 4 m of the foot of her perch she cannot see or shoot the player (she would have to lean
  out over the edge).

A blocker of height h hides a player standing d metres behind it from an eye E metres above the player's floor at
horizontal distance D when h is at least 1.2 + (E - 1.2) d / D. So the **lee** of a blocker, the strip behind it where
she cannot see, is (h - 1.2) D / (E - 1.2) deep. For her eye 11.6 m up: a 3 m hull 40 m from her has a lee 7 m deep;
the same hull 17 m from her, 3 m. **Cover shrinks as the player nears her**, which is the escalation of every room.
The builder works the lees out by casting from each perch over a 0.5 m grid (build list C2); the rectangles below are
that answer rounded, and the snow's soft blue shadows are laid to agree with them (the district's sun stands behind her
main perches, so a lee mostly is a shadow: `LOOKS.md` already asks for "the ribs' shadows... the cover the player runs
between").

**Her states.**

| State | What she does | Becomes |
|---|---|---|
| Unaware | Watches the room's entrance | Hunting after 1.0 s of continuous sight |
| Hunting | Has sight; draws and shoots at her rate | Searching when sight is lost for 0.3 s |
| Searching | Watches the last point she saw the player (her gaze stays on it for 8 s, then sweeps) | Hunting after 0.6 s of sight inside her gaze |
| Fallen back | Struck: gone from the room | Unaware at the first perch of the next room on her list |

**The shot.** The draw is 1.0 s. When it starts, the glint shows at the edge and the line is laid on the snow: it
ends where the player will be in 1.0 s if they hold their course (at most 4 m ahead; at their feet if they stand
still), and **it does not move**. At 0.4 s before the strike the glint flares and the bowstring thrums (the loose);
the arrow lands at 1.0 s. The strike zone is the last 2 m of the line, 0.6 m either side of it, drawn brighter. A player
in it is struck: **40 damage (of 100) and pinned 1.0 s** (rooted, the guard can be raised, a roll breaks the pin after
0.6 s). If sight is lost before the loose, the draw is cancelled: the line goes out and the glint dims. Her arrow strikes
whatever is in the zone: a pack member takes 40, a brittle shatters. A missed arrow stands in the snow (12 s).

**Striking back** (question 2). Any of these is a hit, and a hit makes her **fall back**:
1. **Reach her unseen.** Every district has one perch the player can climb to (P1a, P2b, P3b). Seen on the way, she
   leaves it for her next perch in the room (a move, not a fall back). Reached while she is unaware or searching, she
   is there, facing where she last saw the player: a blow lands.
2. **Turn her arrow.** A perfect guard on her arrow (the standard 0.18 s window before it strikes) sends it back up the
   line, and she has not moved yet (she holds the perch 1.2 s after loosing). While her line is on the player, raising
   the guard turns them to her bearing, not to the nearest enemy. This is the second lesson of the reflect arc
   (`WORLD.md`, Proposal: "Ice: reflecting the huntress's arrows back at her"), learned on the walk before her fight.
3. **Shoot along the glint.** While the glint shows, it is a target at the frame's edge: a Great Bow shot or a spell
   that flies, loosed within 10 degrees of her bearing, strikes her at any range. Steel and element both answer her
   (pillar 2).

**Falling back.** She cries out, the glint goes out, a pale ski line streaks off the far snow; no arrow comes for the
rest of the room, and the pack's howls stop marking the player. Her arrow, broken, is left where she was struck (on the
perch, or on the line's end after a reflect): picked up, 50 Shards (question 3).

**The pack is her eyes.** A howler that can see the player howls and **marks** them for 4 s: while marked, soft cover
does not hide them from her. Kill the howler and the snow is the player's again.

### 1.3 Fair to the hunted

The decided pin exists "so the pack catches up". These rules keep that a threat and never a lock:
1. **One arrow at a time.** At least 6 s between strikes in the harbour, 5 s in the forest, 3.5 s in the gauntlet; at
   least 8 s while two or more enemies are within 6 m of the player.
2. **Her arrow takes the pack's token.** The pack attacks one at a time (1.5); while her line is on the player, no
   pack member starts a lunge, and while a lunge or any red attack is on the player she does not start a draw. Two
   things that cannot be guarded never land within 1.2 s of each other.
3. **No arrow on a climb or a narrow footing**: ladders, shrouds, ropes, the deadfall trunk, rib tops. Dark Souls'
   Anor Londo archers, whose arrows knock the player off a narrow buttress, are the cautionary case; here the pin
   holds the player in place and never pushes, and no footing that she covers is less than 4 m wide or unrimmed.
4. **No arrow at a pinned, snared, flinching or knocked-down player.**
5. **The pin brings the pack, it is not a free bite.** The pack that follows the player waits 20 to 30 m off, out of
   frame, heard; a pin sends them in at 8 m/s, so they arrive as the pin ends. The pin costs position, not a combo.
6. **Never at a fire.** No perch sees within 12 m of a rest fire, and after a fall she is unaware for the first 10 s.
7. **Always heard.** Every draw creaks from her side; every howl comes from its howler's side.
8. **Vistas are safe.** A vista zone opens only while she is unaware or fallen back, and no enemy has noticed the player.

### 1.4 Measures

| What | Measure | Why |
|---|---|---|
| Walk, roll | 6 m/s; roll 4.5 m (as Fire) | |
| Deep snow | 4.5 m/s, roll 3.5 m, every step a noise | Hidden under the trees, but slow and heard: sight against sound (1.5, the buried) |
| Hard ground | Trails, ski runs, ice, rock, timber, bone, frozen hides: 6 m/s, silent | |
| Her shot | Draw 1.0 s; loose at -0.4 s; lead up to 4 m; zone 2 m by 1.2 m; 40 and a 1.0 s pin | 1.2 |
| Her sight | Eye on the perch's platform + 1.6 m; gaze 70 degrees; range 45 m (forest 30 m); found after 1.0 s (unaware) or 0.6 s (searching) | So the earliest arrow is 1.6 to 2.0 s after the player steps into sight |
| Open stretches where she hunts | **Up to 9 m**: safe if crossed without stopping (1.5 s). **10 to 16 m**: one arrow to read. **Over 16 m**: the gauntlet's, with the wind | From the line above |
| Cover where she hunts | At least 2.4 m tall; a lee at least 3 by 3 m at the ranges used; no point of the walk more than 12 m from a lee, lees no more than 20 m apart | 1.6 s found and drawn is 9.6 m of running; a player in a fight needs about 2 s to break off |
| Her perches | Platforms at least 2.5 by 2.5 m with a 0.6 m rail; 6 to 12 m above the ground she watches | A reached perch is a small stage |
| The pack | Runners 8 m/s in, circling at 4 to 5 m at 3 m/s, a lunge of 3.5 m; leap anything under 1.5 m (the player cannot) | Faster than the player, so terrain and the fight, not running, are the answers |
| The howl | 1.2 s, heard; then the pincer: two lunges 0.4 s apart from opposite sides; and the mark | The one time two attack together, and it is announced |
| The brittle's shatter | A ring of shrapnel 3 m: 18 to the player, 40 to the pack, shatters any brittle within 2 m | |
| The buried | In deep snow only, 5 m/s toward the last noise; surfaces with a 0.6 s bulge (red); a blow on its line forces it up, stunned 2 s | |
| Noise, heard by the buried and sleepers | A step in deep snow 6 m; a roll 8 m; a blow landed 10 m; an arrow strike 10 m; a shatter 14 m | Question 4 asks whether to draw these as rings |
| A snare | Springs on a foot within 0.6 m of its noose; holds 1.5 s (the guard can be raised, a blow on the noose cuts it after 0.5 s); holds an enemy 3 s, hung and open to a finisher | |
| A deadfall | Trip line across the trail; the log falls on a strip 1.5 m wide after 0.8 s: 35 to anyone under it | |
| Spindrift (the bones) | Every 18 s; warned 2 s by the wind's rise and snow tearing off the rib tops; 4 s of soft cover over every trough | The wind is the player's ally here, the reverse of Fire's swell |
| A group fight; rises; steps; stairs; ramps | 12 to 16 m by 10 to 12 m; a rise 1.5 m, a drop 2 m; a step 0.3 m; stairs 27 to 32 degrees; ramps 8 degrees fought on, 15 walked | As Fire |

### 1.5 The land's people, as the rooms use them

All three corruptions and the alpha are **Decided**; how each fights below is **Proposal**, each mechanic its own, each
tied to sight or sound.

- **The pack** (her household's hunters, who learned to run on all fours). Two kinds in every group:
  - *Runners* do what wolves are found to do in simulation (Muro and others, 2011: approach the prey to a safe
    distance, then move away from the other wolves near it; encirclement, relays and ambush emerge from those two rules
    alone). Each runner steers in a straight line to a slot on a ring 4 to 5 m round the player, spaced from its
    packmates; a slot whose straight line is blocked by something taller than 1.5 m is not taken, so **a tall rock at
    the player's back denies a flank**. Only one runner attacks at a time (an attack token, as Doom 2016's demons ask
    for one before attacking), and the one that attacks is the one behind: the guard covers the front 200 degrees, and
    the pack is built to find the rest. A runner falls in two or three blows.
  - *The howler* hangs back on a rise and conducts. Its howl (1.2 s, heard from its side) sends the pincer: two
    runners from opposite sides, 0.4 s apart, so one gets past the guard. The answers: roll out of the ring as the howl
    ends (the pincer's second lunge is the perfect-dodge window), or kill the howler first. Without a howler, runners
    come one at a time from the front. The howl also marks the player for the huntress (1.2).
  - Runners run straight, so **a runner whose line crosses a snare is caught**. The pack does not see the hunters'
    snares any better than the player does.
- **The brittle** (the fjord's people, frozen where they stood). Slow, heavy, a club or axe of ice, and a grip that
  chills (slowed 2 s). Light blows chip (a crack runs further each time); a heavy blow, a finisher or her arrow
  **shatters** one: the shrapnel ring hurts everyone in 3 m, the player and the pack alike, and breaks any brittle within
  2 m, so they go in chains. Most stand **dormant** like statues and are hard cover, until struck, until the player
  passes within 2 m, or until her arrow finds one in its zone: then the cover is gone and the shrapnel is in the lee.
- **The buried** (lurkers under the snow, seen only as a moving line of disturbed snow, **Decided**). They move only
  in deep snow and toward the **last noise** (1.4). Where the line reaches the player the snow bulges (0.6 s, red: a
  grab from below, dodge it) and it bursts up, drags the player down (held 1 s, 20) and sinks again. A blow on the
  moving line forces it up, stunned and open for 2 s. On hard ground the player cannot be reached, except by a lunge of
  2 m from the edge of the deep snow, which can be guarded. Her arrow striking the snow is a noise: **a missed arrow
  calls the buried**.
- **The snares** (the huntress's own, set for the player). Spring snares (a bent sapling, a noose on the snow) and
  deadfalls (a propped log, a trip line). From above they are what `LOOKS.md` asks for: dark lines a careful eye can
  find on the brightest floor in the land, a sapling bent wrong, snow shaken off it. A blow on an unsprung snare springs
  it harmlessly.

---

## 2. The whole land

The land climbs out of the frozen fjord, through the buried harbour town, up the hunting forest's valley to the pack's
clearing, up the giant's ribcage on its west flank, along its neck, and through the horns of its skull to the lake
(its own scene). Six rest fires. The main path is about 640 m of walking.

```
 z
540 ..................... [ THE HORNS ] -> SkullLake (own scene)
520       NECK: six drums y42 -> 54                    V6
476   rib 6 ===== neck's foot (-3,42,476) ===========|
452   T5 y40   P3b Broken Rib (-36,46,450)           | S3 the spinal groove (x -4..-2)
430   T4 y38                                         |      spine x 0..4
420   T3 y36                   F5 * (-8,36,420) <----'      [east flank: the frozen heart] (side)
388   T2 y34                                 P3a spine (2,42,396)
366   T1 y32
344   STERNUM y30   V5
330   ALPHA'S GROUND y28, centre (0,28,330)          F4 * (12,25,312)  S2 head (20,24,306)
280   SWITCHBACK y16 -> 25       P2c Antler Rock (16,30,300)           |
252   RAVINE y10  P2b stand (-24,20,250)   deadfall north              | S2 the long run
226   SNARE GLADE y14 x-26..-2 | RUN 1 x0..6, P2a Watch Rock (4,24,266) |   SLEEPERS' HOLLOW (42,9,232) trial
216                                F3 * (10,13,216) <------------------'
176   forest edge; cliff-top trail west -> the frozen falls (-24,14,150) (side)
170   HUNTERS' GATE (32,9,170)  V3
150   JARL'S YARD y7 | HEAD POOL (open water), the Long Serpent, P1c (-6,12,134)
124   TRENCHES y3 -> 7 | nausts | QUAY y3, F2 * (28,3,112)
 96   THE RAVEN P1a (-8,0,98) | the lead (open water) | THE JARL'S SHIP (-23,0,96) on the west ice (side)
 48   THE FROZEN FLEET y0
 24   pressure wall (west) | the warning shot
  0   @ wake (0,0,0)   F1 * (5,0,8)   THE FJORD MOUTH
      x: -40        -20         0          20         40
```

**Heights, south to north.** Fjord ice 0; quay 3; trenches 3 to 7; jarl's yard 7; gate 9. Forest trail 9 to 25; glade
14; ravine floor 10; switchback 16 to 25; alpha's ground 28. Sternum 30; troughs T1 to T5 at 32, 34, 36, 38, 40, each rib
top 2.5 m above the trough south of it; neck 42 to 54; horns 56. Perches: P1a 10, P1c 12, P2a 24, P2b 20, P2c 30, P3a 42,
P3b 46 (platform heights; her eye is 1.6 m above).

**The funnel.** The ice is 56 m wide at the mouth with two ways round every hull; the town narrows it to three
trenches; the forest to one trail with runs crossing it; the ribcage to one trough at a time; the neck to 6 m. The
player is driven, like the reindeer the fjord's people once drove between their guide fences, toward the pit at the
top.

**Rest fires (six).** A fire here is a fire in a ring of stones under a windbreak of spruce boughs, never under a roof.

| Fire | Where | Foot | Why there |
|---|---|---|---|
| F1 | The fjord mouth, in the lee of an overturned boat | (5, 0, 8) | In sight of waking; the side loop's drop lands 20 m from it |
| F2 | The quay, beside the first boathouse | (28, 3, 112) | Relief after the fleet; S1 comes down to it |
| F3 | The run's east edge, under a boulder | (10, 13, 216) | Before the first crossing and the glade; S2 comes down to it |
| F4 | Below the alpha's ground | (12, 25, 312) | The alpha's door; S2's head is 9 m from it |
| F5 | Trough 3, under the great vertebra | (-8, 36, 420) | Halfway up the bones; S3 comes down to it |
| F6 | Under the left horn, in `SkullLake` | see section 6 | The god's door (Decided) |

**Her perches.**

| Perch | Where | Platform y | Watches | Reachable |
|---|---|---|---|---|
| P0 | Unseen (the Raven, unaware) | | The warning shot only | |
| P1a | The Raven's mast-top platform | (-8, 10, 98) | The fleet; the oarsmen if she is not struck | **Yes**: the shrouds |
| P1c | The Long Serpent's mast, in the head pool | (-6, 12, 134) | The jarl's yard | No |
| P2a | The Watch Rock, at the top of run 1 | (4, 24, 266) | The run 1 crossing (after the glimpse) | No |
| P2b | The hunting stand in the great spruce | (-24, 20, 250) | The snare glade | **Yes**: the ladder from the ravine rim |
| P2c | The Antler Rock | (16, 30, 300) | The switchback | No |
| P3a | A spinous blade of the spine | (2, 42, 396) | Troughs 1 and 2 | No |
| P3b | The Broken Rib, rib 5's raised west stump | (-36, 46, 450) | Troughs 4 and 5 | **Yes**: the climbers' rope |

**The route.** Wake on the ice; north up the sledge track to the warning shot; through the frozen fleet from lee to
lee (the Raven's shrouds a choice); past the frozen oarsmen and up to the quay (F2); up the snow trenches to the jarl's
yard; through the hunters' gate into the forest; along the trail in the deep snow to F3; across run 1 into the snare
glade; down into the ravine and up the deadfall; up the switchback to F4; the alpha's ground; out onto the sternum; up
the troughs of the ribcage to F5; up the last two troughs to the neck's foot; along the neck; through the horns (F6).

**Branches and loops.**
- The jarl's ship (see it, then find the way): seen from the Raven's platform; reached from the forest's edge by the
  cliff-top trail and the frozen falls; holds the plain Great Bow; loops home to F1 over the pressure wall.
- The Raven's shrouds (a high branch on the main path): the first strike back.
- The hunting stand (a high branch): the way to it is the ravine.
- The Sleepers' Hollow (the trial), off the long run once S2 is open.
- The frozen heart (see it, then find the way): seen from the sternum; reached from the spinal groove once S3 is open.

---

## 3. The districts

Each district: the idea, its three levels, a plan, then each space in walking order. Encounters are numbered E1.1 and
so on, as in Fire. For every room where she hunts: her perch, her eye above the room's floor, the distances, the lees
(west, east, south, north), and the open stretches.

### 3.1 District 1: the ice-locked harbour (z -10 to 172)

**The idea.** A fjord frozen in a winter that never ended, with the town's fleet locked in it at angles: the outer ships
shoved over by the ice from the north, so their bottoms face the mountain and their decks tip toward the camera, the
inner ones still upright with their masts standing. A stream under the frozen falls keeps the ice open at the fjord's
head and down its west side: a black **head pool** and a **lead** of open water. The town on the east shore is buried to
the eaves; its streets are trenches dug through the snow. The pattern of being hunted: **the warning shot, then arrows
from the masts.** **Levels:** the ice (y 0); the quay and the trenches (y 3 to 5); the jarl's yard (y 7). Perch
platforms are not levels: only the Raven's is ever stood on.

```
 z     x: -34  -24   -14   -4    6    16    26    36    46
172                                        [GATE] (32,9,170)
166                       head    |  JARL'S YARD y7   hall shell
150   falls(-24,0..14,150) pool   |S1|  R2  boat  wood     |
142   ice stair       Long Serpent|  | TRENCH C y5..6 ---- |
134   down to   ~~~~~ P1c(-6,12,134)  |  (hw)hump   TRENCH B|
124   west ice  ~~~~~~~~~~~~~~~~~~~~| nausts =====TRENCH A=|
112        ~lead~                    QUAY y3  F2*(28,3,112)
104   JARL'S  ~~      THE RAVEN      (o)(o) stair up
 96   SHIP    ~~    P1a(-8,10,98)  oarsmen (o)(o)
 86   (-23,0,96)~~         H3 the Wave (4,0,86)
 72   west ice  ~~    H2 the Elk (-4,0,72)
 60              ~~           H1 the Whale (10,0,60)
 44    ##### pressure wall ##### track ...  marker poles
 24   (-34..-6, z24..28) drop S  warning shot at z32
  8                                F1*(5,0,8)  overturned boat
  0                           @ wake
      west cliff                                 east shore
```
`(o)` dormant brittle (the oarsmen); `(hw)` howler; `~~` open water.

**The ice's outline.** West cliff x -34 (z -10 to 40), -30 (z 80), -26 (z 120). East shore x 22 (z -10 to 60), then the
quay's 3 m face at x 16 (z 96 to 124). The **lead** runs from the head pool at (-23, 0, 124) south to (-8, 0, 28), 4 to 6 m
wide, black water edged by a rim of heaved ice 0.6 m high (nothing walks into it; `FallCatch` under it all the same).
The **head pool** fills z 124 to 150 from the west cliff to x 14. South of z 28 the lead ends against the **pressure
wall**, heaved blocks 2.5 m high from (-34, 0, 24) to (-6, 0, 28): from the south it cannot be climbed; from the west ice
it can be dropped (2 m, facing south) at (-14, 0, 24). The ice everywhere is snow-covered and grips: only the lake's ice
slides (Decided for her fight). Its floor from above: wind-scoured snow with patches of clear black ice showing frozen
fish and weed, pressure ridges in broken lines, the sledge track and its marker poles.

#### 1A. The fjord mouth (z -10 to 22, x -34 to 22, y 0)
- *Shape:* the widest the land ever is. Pressure ridges 1.2 m high (the camera sees over them) cross the ice east-west
  in broken lines; the sledge track runs north between them, 4 m wide, packed, a marker pole every 8 m.
- *Sees on waking:* a small boat lying overturned 10 m ahead, a fire in its lee; the poles leading north; snow
  streaming past toward the camera.
- *Rest:* **F1** at (5, 0, 8), in the lee of the overturned boat at (3, 0, 10).
- *Vista:* **V1** at (0, 0, 16), 8 by 4 m: the track between the ridges, the pressure wall's blocks on the left, and at
  the top the first hull's dark bottom standing out of the ice.
- *Enemies:* none. The sponsor speaks on waking.
- *Kit:* `faering_overturned`, `fire_ring`, `marker_pole` x10, pressure ridges (build list I2).

#### 1B. The warning shot (z 22 to 44)
- *Shape:* the track between two pressure ridges, the pressure wall 6 m to the west.
- *The shot:* when the player crosses z 32, the glint shows at the top-left edge and the line runs across the track,
  ending **2 m ahead of the player**, not on them; one second; the arrow strikes and stands in the track, red fletching
  quivering (`LOOKS.md`'s one image). Status line, once: "An arrow, from the masts. Keep something between you and
  them." Then nothing for 16 m: she watches.
- *Why here:* the track is open, the ridges are too low to hide behind (the player learns what is not cover), and the
  next lee is in sight.
- *Enemies:* none.

#### 1C. The frozen fleet (z 44 to 104, the ice east of the lead)
- *Shape:* three heeled hulls and the upright Raven, set so the track spirals in toward the Raven:
  - **H1, the Whale**, a broad cargo ship at (10, 0, 60), 18 by 5 m, its bottom a curved wall 3 m high facing
    north-north-west.
  - **H2, the Elk** at (-4, 0, 72), 16 by 4.5 m, its bottom 2.6 m high.
  - **H3, the Wave** at (4, 0, 86), 16 by 4.5 m, 2.8 m.
  - **The Raven**, upright, 22 m long, its axis north-south, centred (-8, 0, 98), gunwale 1.4 m; its mast carries a
    lookout platform 2.5 m square at y 10 with a 0.6 m rail; its shrouds hang to its deck on the east side (a ladder,
    "Climb the shrouds", 2.4 s), and a heeled plank from the ice to its deck at (-3, 0, 92).
  - A capsized boat at (-2, 0, 64), 1.2 m: not cover, and it looks it.
- *Her perch:* **P1a**, on the Raven's platform; eye y 11.6, 10.4 m above the chest point.
- *The lees* (she is north-north-west):

  | Lee | Rectangle | Her distance | Depth |
  |---|---|---|---|
  | H1 | (8, 18, 50, 57) | 41 m | 7 m |
  | H2 | (-6, 2, 65, 69) | 26 m | 3.5 m |
  | H3 | (2, 10, 80, 83.5) | 17 m | 2.6 m |

- *Open stretches:* the end of 1B to H1's lee, 10 m; H1 to H2, 17 m; H2 to H3, 12 m. She becomes hunting as the player
  passes z 48, so the first and third cost one arrow each to read, the second one or two. The lees shrink as the
  player nears her: the room's arc.
- *Striking back (the land's first lesson of it):* from H3's lee, she searches the lee. Leave it by its **west** end and
  the 13 m dash to the plank and the shrouds is outside her gaze (44 degrees off the lee's bearing); inside 4 m of the
  mast she cannot see the player at all. Climb, and she is there with her back turned: strike, and she falls back,
  leaping to the west ice and skiing off north up the lead's far side (in frame from the platform). Her broken arrow lies
  on the platform. Seen on the way, she drops from the mast before the player climbs and is next on P1c, unstruck.
- *Vista:* **V2** on the Raven's platform, 3 by 3 m, looking west-south-west (`LandZones` with a direction, Water's W19):
  across the lead, the west ice, and the jarl's ship lying in it with a frozen archer at its stern, a bow in his hands
  catching the light. Out of reach: the lead is 5 m of black water. (See it, then find the way: 3.1, side path.)
- *Enemies:* none but her. Arrows first, alone.
- *Kit:* `longship_heeled` x3 (B1 below), `longship_upright` (the Raven), `faering_overturned`.

#### 1D. The frozen oarsmen (z 98 to 112, x 2 to 16, y 0)
- *Shape:* a knarr frozen against the quay face at (10, 0, 104), its crew frozen where they stood hauling it in: four
  figures on the ice at its side. The quay stair goes up beyond them (x 16 to 20, z 104 to 108, 3 m over 5 m, running
  east).
- *Enemies:* E1.1 four **brittle**, dormant, at (6, 0, 100), (8, 0, 102.5), (12, 0, 101) and (13, 0, 106). The one at
  (8, 0, 102.5) wakes as the player passes within 2 m, or when struck. The lesson: light blows chip, a heavy shatters,
  and its shrapnel breaks the two within 2 m. Kill it beside the others from outside 3 m (a heavy at the edge of the
  ring, then a roll back), or wake them one by one with light blows.
- *If she is still on the Raven (the player went past it):* the oarsmen are the only cover between H3 and the stair, their lees
  2 m deep; her arrow shatters the one the player hides behind, and the shrapnel is in the lee. Striking her at the
  Raven is not required, but this is where skipping it is felt.
- *Lore:* none.

#### 1E. The quay and the boathouses (x 16 to 40, z 96 to 124, y 3)
- *Shape:* timber and stone, swept clear by the wind. Along its north edge, three **nausts** (Norse boathouses, long
  stone-walled sheds) with turf roofs under snow, 4 m to the ridge, x 16 to 40, z 116 to 124, their doors shut and
  drifted (not walkable inside). Their long roofs stand between the head pool and the town: P1c cannot see past them.
- *Rest:* **F2** at (28, 3, 112), beside the first naust's gable.
- *Enemies:* none. The quay is relief.
- *Out:* north-east into Trench A at (40, 3, 124); or, once open, S1's run at the quay's north-west corner (14, 3, 126).

#### 1F. The snow trenches (x 16 to 50, z 124 to 150, y 3 to 6)
- *Shape:* the town's longhouses are humps of snow with turf showing at the eaves; between them the streets are
  trenches 3.5 m wide dug down to the old paving. Each east-west trench has a north wall 2.2 to 2.6 m (the drift
  against the house beyond) and a south wall 1.2 m (scoured), so the camera sees into it. **Trench A** runs east along
  the nausts' backs (z 126 to 129.5, x 40 to 48, y 3); **Trench B** north (x 44 to 47.5, z 129 to 142), a ramp up to y 5;
  **Trench C** west (z 142 to 145.5, x 20 to 47, y 5 rising to 6); **Trench D** north (x 20 to 23.5, z 145 to 150), a
  stair to the yard at y 7. Drift ramps (2 m over 9 m) climb onto the humps at Trench C's two ends.
- *Her:* none. She cannot see into the trenches: the nausts and the humps stand between them and the head pool. The
  pack, alone, first.
- *Enemies:* E1.2 the first **pack**: a howler on the hump south of Trench C at (32, 6.2, 140), 1.2 m above the
  trench's floor and 3 m south of its middle (in frame); a runner coming down Trench C from its west end; a runner that
  leaps down from the hump behind the player. A trench is too narrow to circle: the pack comes from the two ends, and the
  howl sends both at once. *The answers:* back into Trench B's corner so only one end is open; climb the east drift ramp
  and kill the howler on its hump; or fight with the guard toward one end and roll at the howl.
- *Why it suits them:* a corridor turns the flank into a front and a back; the howler on the hump shows the player who
  conducts before they meet a pack in the open.
- *Lore:* **R1**, a runestone in a niche of Trench B's west wall at (44, 4, 136). Words (Claude's, new): "The fjord gave
  fish. Then it gave nothing, and she came down from the mountain to feed us." / "She fed us the hunt."
- *Kit:* `longhouse_buried` x7, trench walls (I5), drift ramps, `runestone`.

#### 1G. The jarl's yard (x 14 to 44, z 150 to 166, y 7)
- *Shape:* the open yard before the jarl's hall. The **hall** is a roofless shell (its roof fell in under the snow),
  x 26 to 42, z 154 to 164, walls 2.5 m, two doors (south and west), snow on its floor, the two high-seat pillars still
  standing inside. In the yard: a woodstack at (18, 7, 156), 2 m; a boat on trestles at (22, 7, 162), 2.4 m; the runestone
  R2 at (20, 7, 152), 2.2 m; a well. The yard's west edge is a wall 1.2 m over the drop to the head pool.
- *Her perch:* **P1c**, the Long Serpent's mast, on a heaved floe in the head pool at (-6, 12, 134); eye 13.6, 6.6 m above
  the yard; 25 to 45 m off, west-south-west. Low, so the lees are long:

  | Lee | Rectangle | Depth |
  |---|---|---|
  | The hall's west wall (and all inside) | (26, 42, 154, 164) and (42, 46, 154, 164) | 8 m beyond the wall |
  | The woodstack | (18, 22.5, 155, 158) | 4.4 m |
  | The boat | (22, 28, 161, 164) | 6 m |
  | R2 | (20, 24.8, 151, 154) | 4.8 m |

  Open: the yard's west strip (x 14 to 18) along the low wall and the gate's approach.
- *Enemies:* E1.3, the district's peak: a howler on the hall's south threshold at (34, 7.6, 154) and three runners who
  come in through the gate as the player reaches mid-yard; her arrows every 8 s while the pack is close. *The choice:*
  fight in the open yard, where the pack flanks and she sees; or in the hall shell, where she cannot see, but its 6 m
  breadth and two doors make the howler's pincer come through both.
- *Teaching the reflect:* R2 shows, cut in the stone, a man with a round shield turning an arrow back at a figure on a
  mast. Words (Claude's, new): "Turn her arrow and it goes home."
- *Shortcut:* **S1**, the sledge run (section 4).
- *Out:* the hunters' gate at the yard's north-east, (32, 7 to 9, 166 to 172): a timber gate in a palisade of sharpened
  logs, antlers nailed over it, a ramp 2 m over 8 m beyond.
- *Vista:* **V3** at the gate (32, 9, 170), 6 by 3 m: the forest's edge, the first spruce, the trail going in.
- *Kit:* `hall_shell`, `woodstack`, `boat_trestles`, `runestone`, `palisade_gate`.

#### Side path: the jarl's ship and the plain Great Bow (the west ice; from the forest's edge)
- *Seen first:* from V2 on the Raven's platform, the ship and the frozen archer's bow; it cannot be reached across the
  lead.
- *The way:* just inside the hunters' gate, a cliff-top trail runs west from (24, 9, 178) along the fjord's rim
  (y 9 rising to 14, z 174 to 180) to the head of the **frozen falls** at (-22, 14, 156), a curtain of blue ice falling
  into the head pool. Beside the falls, an **ice stair** cut by the jarl's men goes down the west cliff, running south,
  from (-26, 14, 156) to (-28, 0, 128): 14 m over 28 m, 27 degrees, a rope rail on its east side. It lands on the **west
  ice** (x -30 to -16, z 28 to 128).
- *The ship:* the jarl's longship at (-23, 0, 96), the largest in the fjord, upright, frozen with her crew. At her stern,
  the **frozen archer**, a brittle (dormant), holding the bow: struck, he shatters and the bow falls to the deck. **The
  plain Great Bow** (`WeaponPickup`), in this land's make: a plain yew longbow 1.9 m long after the Hedeby bow, horn
  nocks, no carving (question 6). Two more brittle, dormant at the oars, wake when the archer shatters (his shrapnel
  reaches one of them if the player strikes from the wrong side).
- *Lore:* **R3** on the ship's prow post. Words (Claude's, new): "The jarl tried to sail out. The fjord closed on him like
  a hand." / "She was already on the mast."
- *Home:* south along the west ice to the pressure wall, and the drop at (-14, 0, 24) (2 m, facing south) onto the
  fjord mouth, 20 m from F1. She does not hunt the west ice.
- *Minutes:* 4 for a new player.

### 3.2 District 2: the hunting forest (z 172 to 340)

**The idea.** A valley of black spruce climbing to the mountain, cut by the huntress's ski runs, which are long clean
lanes down the slope and her sight lines; the snares she has set for the player wait at their edges. Under the trees the
snow is deep: hidden from her, but slow, and heard by the buried. On the runs and the trails the snow is packed: silent,
and seen. **Sight against sound** is the district's rule. The pattern of being hunted: **herding**: she drives the pack at
the player and her arrows push them toward the snares. **Levels:** the trail and the runs (y 9 to 25); the ravine floor
(y 10, below the trail at 14 to 16); the stand and the rocks (y 20 to 30, her ground; only the stand is stood on).

```
 z     x: -30   -20   -10    0     10    20    30    40    50
340          ALPHA'S GROUND (0,28,330), stake gate N
312                                      F4*(12,25,312) [boom](20,24,306)
302    switchback leg 3 y22-25 ===========>      P2c(16,30,300)
292    <=========== leg 2 y19-22                   |
282    switchback leg 1 y16-19 ===========>        | S2 THE LONG RUN
274          deadfall top (-6,16,274)              |  x14..20, 7 degrees
260   ~~RAVINE floor y10~~ deadfall foot (-6,10,258)|
252   rim path y14 [P2b stand (-24,20,250)]         |      cut E -> SLEEPERS' HOLLOW
248   +--- SNARE GLADE y14 ----+ RUN 1 x0..6       |      (31..53, 223..241) y9
236   | s     s   boulder      |   P2a Watch Rock (4,24,266) at the top
226   | s  s      racks  boulder| crossing z218..224 |
216   +------------------------+    F3*(10,13,216) <-'
200            deep snow   trail   (bu)  cache (36,10,206)
176   cliff-top trail W <- (24,9,178)       forest edge
170                                   GATE (32,9,170)
```
`s` snare; `(bu)` buried.

#### 2A. The forest's edge (z 170 to 186, y 9)
- *Shape:* through the gate the trail turns north-west into the trees. The first spruce, a woodcutters' clearing,
  stumps. The cliff-top trail (side path) leaves west at (24, 9, 178).
- *Enemies:* none.

#### 2B. The deep snow (z 186 to 212, y 9 to 13)
- *Shape:* the trail, 3 m wide and packed, winds north-west between the trunks; either side is deep snow under the
  canopy. A hunter's cache, a bundle tied to a stake, stands in the deep snow 8 m east of the trail at (36, 10, 206), in
  plain sight.
- *Enemies:* E2.1 the first **buried**, lying at (30, 10, 200). Walk the trail and it never stirs. Wade out for the
  cache and every step calls it: its line comes through the snow toward the player. *The answers:* get back onto the
  trail (it cannot follow onto packed snow, and its 2 m lunge from the edge can be guarded), or strike its line as it
  comes and kill it while it is stunned.
- *Why here:* she cannot see under the canopy, so the buried are learned alone; the cache is the reason to wade.
- *Gift:* the cache (Shards).

#### 2C. The first run (z 212 to 226)
- *Shape:* **run 1**, a ski run 6 m wide (x 0 to 6) cut straight down the slope from the Watch Rock at its top (4, 24, 266)
  to (3, 10, 196): packed snow, one long clean line. The trail meets it from the east at F3 and crosses it at z 218 to
  224 into the glade's edge.
- *Rest:* **F3** at (10, 13, 216), under a boulder on the run's east side, out of her sight (the boulder stands between
  F3 and the run's top).
- *The glimpse:* **V4** at the run's edge (4, 13, 218), 6 by 3 m, looking north up the run. The first time the player
  steps into it, she is **there**, 16 m up the run on a fallen tree's root plate 2 m high at (3, 15, 234): a tall figure in
  a crimson cloak, bow drawn. She looses one arrow down the run (the line laid on the run, not on the player), turns
  and skis away up it into the snow. The only time she is seen whole on the walk before the horns.
- *Then:* she hunts from **P2a**, the Watch Rock at the top of the run: eye 25.6, 12 m above the crossing, 44 m off (inside
  her forest range only while the player is on the run). Crossing 6 m is safe at a run; walking up or down the run is
  not. The run is the forest's first sight lane, and the player learns to cross lanes, not walk them.
- *Enemies:* none but her.

#### 2D. The snare glade (x -26 to -2, z 226 to 248, y 14)
- *Shape:* an open glade 24 by 22 m in the trees: two boulders 2.4 m high at (-16, 14, 236) and (-6, 14, 244); racks of
  drying hides at (-10, 14, 232), 2.2 m (soft cover); stumps; a laden spruce at each corner (a blow on its trunk dumps its
  snow: soft cover 4 m round for 4 s, a loud noise, and a runner under it is stunned 1 s). The glade's west and north
  edges are where the snares are, and the open way out west at (-26, 14, 232) looks like the way to run.
- *Her perch:* **P2b**, the hunting stand: a platform in the great spruce on the glade's north-west corner, (-24, 20, 250);
  eye 7.6 m above the glade, 8 to 26 m off. Lees: the boulders' east and south-east sides, (-15, -11, 232, 236) and
  (-5, -1, 240, 244), about 3 m; the racks' south-east side (soft).
- *Enemies:* E2.2 **the drive**: a howler at the glade's east edge on a stump at (-3, 15, 230) (in frame from the glade's
  middle) and three runners that come in from the east, behind the player, from the direction of run 1. The howl drives
  them as a line, not a ring (beaters), and she lays her lines across the north; the open side left is west, where the
  snares are:
  - spring snares at (-22, 14, 230), (-20, 14, 238), (-18, 14, 244), (-12, 14, 247), and in the west exit at
    (-26, 14, 232);
  - a careful player sees each one: a noose dark on the bright snow, a sapling bent wrong.
  *The answers:* turn and fight through the beaters (the boulder at the back denies a flank); stand still behind a snare
  so a runner's straight line goes through it (hung 3 s, open to a finisher); step over her line just after it strikes,
  north, toward her and away from the snares; dump a laden spruce for 4 s of snow; or shut her eyes by going up to her
  (the ravine).
- *Why it suits them:* the glade is a hunt laid out for the player, beaters, a funnel, a trap; seeing it is the way out.
- *Lore:* **R4**, her tally cut into the stand's post, seen from the platform: rows of strokes under pictures of elk,
  bear, wolf; the last rows under pictures of men.

#### 2E. The ravine and the stand (x -30 to 10, z 250 to 262; floor y 10)
- *Shape:* a frozen stream in a ravine running east-west under the glade's north edge: its north wall is sheer rock 6 m
  high (it faces the camera), its south side a slope of drifted snow at 35 degrees (so the camera sees the floor). The
  floor is the stream's ice (silent) between banks of deep snow. A path goes down the slope from the glade's north-east
  corner at (-2, 14, 248) to the floor at (-2, 10, 254). At the ravine's west end a slope climbs back to a **rim path**
  (y 14) along the south edge, x -30 to -20, and the stand's ladder hangs on the great spruce's north side above it at
  (-24, 14, 252): "Climb" (6 m, 1.8 s).
- *Her:* she cannot see into the ravine. A player who drops into it from the glade is lost to her (searching the glade).
  Walk the ice west, climb the rim path and the ladder outside her gaze (she watches the glade, south-east of her), and
  she is on the platform with her back turned: strike, and she falls back. The district's strike back.
- *Enemies:* E2.3 two **buried** in the deep banks at (-16, 10, 256) and (2, 10, 258); a **brittle**, dormant, a frozen
  hunter on the ice at (-10, 10, 255). The ice path between the banks is 2 m wide: silent if walked; the brittle stands
  on it. Shattering it is the loudest noise in the ravine (14 m), and both buried come.
- *Out north:* **the deadfall**, a giant spruce fallen across the ravine as a ramp from the floor at (-6, 10, 258) to the
  north rim at (-6, 16, 274): 1.6 m wide, 24 m long, 14 degrees, bark and snow. One at a time on it; no arrows on it
  (1.3, rule 3). Falling off lands on the ravine floor.

#### 2F. The switchback (x -8 to 14, z 276 to 308, y 16 to 25)
- *Shape:* the trail climbs the valley's last slope in three legs running east and west (leg 1 z 282, y 16 to 19; leg 2
  z 292, y 19 to 22; leg 3 z 302, y 22 to 25), each 4 m wide and 20 m long (8.5 degrees), joined by turns at x -8 and
  x 12. Between the legs, rock steps 3 m high face south (a drop off a leg's south edge lands on the leg below). Big
  boulders on the legs; two laden spruces.
- *Her perch:* **P2c**, the Antler Rock at (16, 30, 300), east-north-east of the legs: she looks along them (an alley).
  Cover is the boulders on the legs: lees on their west sides, 2.5 to 4 m.
- *Enemies:* E2.4 the **following pair**: two runners who have trailed the player since the glade (heard, off screen)
  close in on any pin. A **buried** in the deep snow on the turn at x -8 (leg 1 to leg 2). Two **deadfalls**: trip lines
  across leg 2 at x 4 and across leg 3 at x -2, the logs propped on the uphill side. Her lines are laid to push the
  player off the trail's middle onto the trip lines or into the turn's deep snow. A careful player springs a deadfall
  on the following pair (a runner's straight line crosses the trip line too).
- *Why it suits them:* the district's lessons together, on a climb, before the alpha: her lanes, the buried's snow, the
  pack's pin, the snares.
- *Rest:* **F4** at (12, 25, 312), past the third leg's east end, in the lee of the Antler Rock's foot (inside 4 m of it,
  and out of her sight).
- *Shortcut:* **S2**, the long run's head (section 4), 9 m from F4.

#### 2G. The alpha's ground
See section 5.

### 3.3 District 3: the giant's bones (z 340 to 540)

**The idea.** The mountain is the first frost giant's corpse (the Ymir nod, `WORLD.md`); here it stops pretending. The
ridges are ribs, curved bone under ice, rising out of the snow one behind another across the slope, with the spine
running up the middle as a ridge of standing blades; the player climbs the ribcage's west flank, trough by trough, while
she shoots down from the spine and from a broken rib. The pattern of being hunted: **the sight-line gauntlet**. The wind
is the hardest in the land, and every 18 s it tears the snow off the ribs in sheets (spindrift): 4 s in which she sees
nothing soft-covered. **Levels:** the troughs (y 32 to 40), the rib tops and ramps (2.5 m above each trough), the neck
(y 42 to 54).

```
 z     x: -40  -36   -28   -20   -12    -4    0  4      12    20
540                                  [ HORNS ]  y56
520                                  neck drums y42->54   V6
476   ===================== rib 6 ==== neck's foot (-3,42,476)
470   T5 y40   (b)(b)  bone blocks          |S3|
452   P3b(-36,46,450)  rope N side  <======= ramp E   spine (blades every 8 m)
448   T4 y38  (hw)on rib5 (r)(r)  (bu)      |  |
430   == rib 4 == ramp E (-12..-8) ======== |  |
420   T3 y36                     F5*(-8,36,420) vertebra drum (-6..2, 412..422)
410   == rib 3 == ramp W (-36..-32) ========|
406   T2 y34  (o)    block   (o)     (o)   <- P3a (2,42,396) looks W down T2
388   == rib 2 ======================== ramp E (-10..-6)
384   T1 y32   (bu)        (r)(r) following pair from the S
366   == rib 1 == ramp W (-36..-32) ==========
362   STERNUM y30   V5 (-6,30,358)            gap E -> east flank (side)
344   ............ stake gate from the alpha's ground (0,28,343)
```
`(o)` dormant brittle; `(b)` brittle; `(hw)` howler; `(r)` runner; `(bu)` buried.

**The ribcage.** Six ribs on the west flank, each a curved bone ridge 4 m thick from the spine's foot (x -4) out to
x -40, its south face 2.5 m (faces the camera: a band of pale bone and blue shade), its top snow-packed bone (hard,
silent), its north side stepping down 0.5 m into the next trough. A rib is crossed only at its **drift ramp**, 2.5 m over
10 m (14 degrees), and the ramps alternate ends, so every trough is walked along its length: rib 1 west (x -36 to -32),
rib 2 east (x -10 to -6), rib 3 west, rib 4 east, rib 5 west, rib 6 east. The troughs are 16 to 18 m deep (z) and 36 m long
(x -40 to -4): group-fight size. The **spine** runs up the middle (x 0 to 4), its crest 6 m above the troughs, a blade
of bone 4 m tall standing every 8 m.

#### 3A. The breastbone (x -24 to 20, z 344 to 362, y 30)
- *Shape:* a broad shelf of pale bone under thin snow, its tip pointing south at the alpha's ground. Rib 1's south face
  across its north edge.
- *Vista:* **V5** at (-6, 30, 358), 10 by 3 m, looking north: rib 1, the troughs beyond as long soft blue bars, the
  spine's blades to the right, a pale ski line along the spine where she went up; snow tearing off the rib tops. Through
  a gap east between rib 1's root and the spine, the ribcage's east side, and in it something dark and huge under the
  ice: the frozen heart (side path, below).
- *Enemies:* none.

#### 3B. Trough 1 (z 366 to 384, y 32)
- *Way:* up rib 1's west ramp, along T1 east to rib 2's east ramp.
- *Her perch:* **P3a** on a spine blade at (2, 42, 396): eye 11.6 m above T1, north-north-east. Rib 2's lee along T1's
  north edge: 5 m deep at the west end (40 m from her), 2.3 m at the east end (18 m). The west half is easy; the ramp
  at the east end is the exposed crossing (12 m from the lee's end, up the ramp and over: two arrows unless the wind).
- *Enemies:* E3.1 the **following pair** comes up rib 1's ramp behind the player (from the south, in frame). A
  **buried** in T1's deep snow at (-20, 32, 374). Waiting in the lee for the wind, the player is found by the pack;
  running the open, by her. The spindrift is the way: the wind rises (2 s), go as the snow tears off the ribs.

#### 3C. Trough 2 (z 388 to 406, y 34): the alley
- *Way:* down off rib 2 at the east end, along T2 **west** to rib 3's west ramp.
- *Her perch:* P3a again, now **due east**, 9.6 m above T2: the trough points straight at her, and the ribs give no
  lee. The cover is what stands in the trough: three dormant **brittle** (climbers frozen where they fell, 2.1 m) at
  (-10, 34, 398), (-20, 34, 394), (-32, 34, 400), and two fallen blocks of bone 3 m high at (-16, 34, 402) and (-26, 34, 392).
  Lees on their west sides: the brittle 1.5 to 3.6 m, the blocks 4.3 to 6.4 m, growing as the player goes away from her.
- *The trap:* her arrow shatters a brittle the player stands behind, and its shrapnel is in the lee. Hide behind bone,
  not people.
- *Enemies:* the three brittle (as cover; they wake if passed within 2 m). The floor is wind-scoured hard: no buried.
  The following pair, if alive.

#### 3D. Trough 3 and the great vertebra (z 410 to 426, y 36)
- *Shape:* up rib 3's west ramp, east along T3 to the spine's foot, where the widest vertebra stands as a drum 8 m across
  and 6 m above the trough (x -6 to 2, z 412 to 422).
- *Rest:* **F5** at (-8, 36, 420), on the drum's west side: hidden from P3a by the drum, from P3b by rib 4's root.
- *Enemies:* none. *Her:* T3 is on neither perch's list.
- *Shortcut:* **S3**, the spinal groove, comes down to F5 from the north (section 4).

#### 3E. Trough 4 (z 430 to 448, y 38): toward her
- *Way:* up rib 4's east ramp beside F5, along T4 **west** to rib 5's west ramp, which passes under the Broken Rib.
- *Her perch:* **P3b**, the Broken Rib: rib 5's west end, snapped and raised like a horn, its stump top a platform at
  (-36, 46, 450); eye 9.6 m above T4, west-north-west. The player walks toward her; the lees (rib 5's south face, two
  bone blocks) shorten from 4 m to under 2 m as they near, and the last 8 m to the ramp's foot are open, until the ramp
  passes inside 4 m of the stump's foot, where she cannot see.
- *Enemies:* E3.2 the last pack: a **howler** on rib 5's top at (-20, 40.5, 449) (2.5 m up, on the trough's north edge,
  in frame within 4 m), two **runners** in T4, a **buried** in its deep snow at (-14, 38, 438). While the howler lives, its
  howl marks the player and **the spindrift does not hide them**. Kill the howler first, then use the wind.
- *Why it suits them:* the howler's mark is the reason the pack matters to the gauntlet; T4 is where that is learned.

#### 3F. Trough 5 and the Broken Rib (z 452 to 470, y 40)
- *Way:* over rib 5 at its west end, under her stump; then T5 **east** to rib 6's east ramp and the neck's foot.
- *Her:* from P3b she looks east down T5 (6 m above it): the second alley, the player running away from her. Cover:
  two bone blocks and two **brittle** (awake, slow, at (-24, 40, 460) and (-12, 40, 464)), which come at the player and
  are worth shattering beside each other.
- *Striking back:* on the stump's north side, in T5's west end, hangs the **climbers' rope**, left by the frozen climbers
  ("Climb", 6 m, 1.8 s). The ramp took the player under her feet, so she lost them and searches T4. Turn at the ramp's
  foot, climb the rope, and she is on the stump with her back turned: strike, and T5 is quiet. The land's last strike
  back, and the one the gauntlet is built to reward: get under her, then up.
- *Lore:* **R5** carved on the stump's foot by the climbers. Words (Claude's, new): "We climbed to see what the mountain
  was." / "It was a man."

#### 3G. The neck and the climb to the god (z 476 to 540)
- *Shape:* past rib 6, the neck: six vertebral drums each 2 m higher than the last (y 42 to 54), joined by drift ramps
  2 m over 7.5 m, the path along their tops 6 m wide, the drop on either side a slope of ice. No ribs, no cover, and no
  need of it: she has gone ahead.
- *Enemies:* none. The wind, the snow tearing past, the drums underfoot. Dread.
- *Vista:* **V6** on the last drum (-2, 54, 520), 6 by 3 m, looking north: the skull's base, its two great horns curving
  up out of the snow, and between them, small and dark, **her**, who looks back once and goes through (the second
  glimpse).
- *Out:* between the horns at (0, 56, 540): `SceneDoor` to `SkullLake`.

#### Side path: the frozen heart (the ribcage's east side; from the spinal groove)
- *Seen first:* from V5, through the gap east of rib 1's root: the ribcage's inside on the east, and under clear ice
  something dark and huge.
- *The way:* the spinal groove (S3) passes a broken disc between two vertebrae at (-3, 40, 444); through it, a slope down
  east (3 m over 9 m) into the east side's hollow, x 6 to 30, z 436 to 456, y 37. In the hollow, under a skin of clear ice,
  the giant's **heart**, the size of a longhouse, cracked through.
- *Enemies:* a **buried** in the hollow's drifts; a dormant **brittle** kneeling at the heart.
- *Lore:* **R6**, the climbers' last stone, at the heart. Words (Claude's, new): "He was the first cold. He slept in
  it." / "Something older than winter came, and he lay down." (Each land's first god was killed by Time: `WORLD.md`,
  2026-11-18. The stone hints, and names no one.) A Shards cache.
- *Minutes:* 2.

### 3.4 The trial: the Sleepers' Hollow (off the long run, x 31 to 53, z 223 to 241)

**Proposal** (question 5 offers two others). Each land's trial is invented for it (Decided); the Water land's is a
duel, so this one is not. Here the player is the hunter for once: **a stealth trial**, the land's verb turned round,
which prepares the reversal of the god's second phase (she hunted the player all land; the player hunts the wolf)
without spending its blizzard.

- *Seen first:* from the long run (S2) at (23, 14, 238), a rock lip looks down east into a hollow sunk 4 m in the forest:
  the pack asleep in the snow round a frozen elk, and on the root plate of a great uprooted spruce, something glinting.
  The player reaches the run's lip only after opening S2 from its head, so the trial comes on the way back down, after
  the pack has been learned.
- *The place:* the hollow, floor y 9, 22 by 18 m; the root plate stands 7 m on its north side (it faces the camera); a
  stair goes down east from the run's cut at (31, 13, 232) to the floor (4 m over 7 m). The floor is deep snow, crossed by
  **quiet ways**: frozen hides laid down by the pack, the elk's antlers and ribs, fallen branches, a patch of clear ice,
  rock from the root plate. A dormant **brittle**, a frozen hunter, kneels at the elk.
- *The sleepers:* six runners and a howler, curled with their backs to the elk. A sleeper hears noise within its range
  (1.4): a step in deep snow 6 m, a roll 8 m, a blow 10 m, a shatter 14 m; noise reaching it twitches its ears (once is
  a warning, twice it wakes).
- *The kill:* a blow from behind a sleeper (the 120 degrees behind it) kills it at once and silently, as Sekiro's
  stealth deathblow; unless another sleeper is within 3 m, which hears it whimper and wakes. A woken sleeper howls after
  1.0 s: kill it in that second and the howl is cut; otherwise the hollow wakes, and it is the harder mix the rules ask
  for: seven of the pack at once in a sunk floor, the howler conducting, and the brittle's shatter waiting to be used on
  them.
- *The prize:* a **Memory** (or a Body piece; `WORLD.md`) on the root plate's ledge (1.5 m up, a drift ramp at its
  foot). Taking it is the win: the pack can be killed, avoided or fought. A player who wakes them on the way out has
  still won, if they live.
- *After a fall or a rest:* the pack is asleep again until the prize is taken; then the hollow is empty.
- *Minutes:* 4 (stealth) to 6 (the fight).

---

## 4. Shortcuts and loops

| | Opens from | To | How | Saves |
|---|---|---|---|---|
| S1 | The jarl's yard, north-west corner (14, 7, 150) | The quay, (14, 3, 126); F2 19 m on | **The sledge run**: a sledge frozen at the run's head; a heavy blow frees it and it runs down and breaks the drift plug at the foot. Then a cut 4 m wide, 24 m, 9.5 degrees, walkable both ways, its west bank 2.6 m (hidden from P1c) | F2 to the yard without the trenches and their pack |
| S2 | The long run's head (20, 24, 306), 9 m from F4 | F3's side of run 1, (14, 13, 220) | **The log boom**: felled trunks lashed across the head of the hunters' own run; "Cut the rope" from the head sends them thundering down (they knock over any pack below). Then 86 m of packed snow, 7 degrees, both ways. No perch watches it: it is the hunters' run, not hers | F3 to F4 without the glade, the ravine and the switchback; and the way to the trial |
| S3 | The neck's foot (-3, 42, 476) | F5, (-6, 36, 422) | **The spinal groove**: an iced channel at the spine's west foot, 2.5 m deep between the vertebrae, 54 m, 6 degrees. Plugged at its north end by a frozen climber (a brittle) on a 1.5 m shelf of ice: from the south he is out of reach; from the north, a heavy blow knocks him onto the groove's floor and he shatters, and the shelf with him. P3b cannot see into it (its west wall, at that range, hides 6 m) | F5 to the neck without T4, T5, the last pack or her; and the way to the heart |
| Drop | The west ice at the pressure wall, (-14, 0, 24) | The fjord mouth; F1 20 m on | One way, 2 m, facing south | The side path's way home |
| Loop | The cliff-top trail, the falls, the west ice, the drop | F1 | Walking | The side path is also D1's loop: F3's side of the gate to F1 |

S1 to S3 stay open after a fall and across sittings (progress keys, as Fire's). She never hunts a shortcut.

---

## 5. The alpha's ground (the elite's arena)

**Decided** (`WORLD.md`): the elite is the pack's alpha. **Proposal** (`WORLD.md`): a huge man-wolf who leads the pack and
shows the wolf's patterns before the god does. **Proposal** (`LOOKS.md`): a clearing at the top of the forest before the
first rib where the pack beds down, snow trampled to grey, bones and torn hides, the pines at their thickest, the
darkest place in the land, so his blood-red markings are the strongest thing in it. **Proposal, approved with the steel
designs** (`steel/weapons.md` 9): he teaches the Fists' gated technique, Bear Them Down; the broken fetter on his wrist is
his look and lore (the wolf broke his first two chains), not a weapon.

**Shape.** An oval clearing 26 m (x) by 22 m (z), centre (0, 28, 330), x -13 to 13, z 319 to 341: the trampled floor is
hard and silent (no buried). The ring of trunks round it is close, with four gaps where the pack comes in (west,
north-west, north-east, east). The way in is from F4 at the south-east gap (8, 27, 319); the way out is a **stake gate** in
the north gap (0, 28, 341), the end of an old drive fence, barred until he falls (as the Gash's throat and the
champion's gate). On the floor:
- a fallen spruce across the north-west quarter, from (-12, 28, 334) to (-2, 28, 340), 1.2 m: the pack and he leap it,
  the player cannot, so it is a wall only to the player's rolls;
- a boulder on the east, (9, 28, 330), 3 by 3 m, 2.5 m high: his high ground;
- four beds, hollows 2 m across and 0.4 m deep (deep snow: slow);
- bone piles and hides at the edges.

```
 z    x: -13        0        13
343        [ stake gate ]
340    fallen spruce    gap NE
330  gap W   (A)      [boulder]  gap E
326         beds
319        gap SE <- F4*(12,25,312)
```

**How it plays with his moves** (numbers belong in `ice_land.md`):
- *The pounce*, Bear Them Down's source: from 6 to 10 m he drops to all fours (0.5 s, the fetter chain clinking), a red
  ring where he will land, and he leaps. The answer he teaches: a late dodge **toward** him, under the leap; he lands
  behind the player and is open 1.2 s.
- *The rake*: three claw blows, the third a sweep of 220 degrees; guardable.
- *Bite and shake*: a grab (red); caught, shaken three times and thrown 3 m.
- *The howl*: two runners come in through the gaps, never more than two on the floor. He circles while they attack in
  turn (the token), and never starts a red attack while one of theirs is on the player.
- *High ground*: he leaps onto the boulder and dives from it onto the player, a long red lane: the wolf's "leap to high
  ground and dive back down", plainer.
- *The squall* (below half health, at most twice): his howl calls the snow, 6 s of soft white inside the clearing in
  which his runners show only as breath and eyes and he pounces out of it. A small, short form of the wolf's blizzard,
  so the god's stays the god's (`steel/depth_elements.md` hints the same: "the alpha's howl calls the snow").
- *No arrows.* The canopy closes over the clearing; she is not here. This fight is his.
- *Why the room suits him:* the oval is a group-fight size for him and two runners; the boulder is the dive's launch and
  the player's one tall back-stop against flanks; the fallen spruce punishes a roll made without looking; the beds slow
  whoever stands in them.
- *His ground:* `HoldGround(-13, 13, 319, 341)`. Beaten: a Memory, Bear Them Down, and the On All Fours aspect's source
  (`steel/depth_weapons.md`); the stake gate opens.

---

## 6. The god's door, the lake and the longship (`SkullLake`, its own coordinates)

The fight's design belongs to `ice_land.md`; this section gives its room and approach, as `water_levels.md` section 6
does for the beach.

**The horns (the god's door).** Through the horns the player comes out on the skull's brow at (0, 20, -40): the crater
opens below, 72 m across. **F6** at (-6, 20, -36), under the left horn, in its lee (Decided: a fire at the god's door).
After a fall the player wakes here and walks down again, about 15 s. **V7** at the brow's lip (0, 20, -34), 10 by 3 m:
the lake from above, its ice dark and glassy, the things frozen under it (fish, ship hulls, the giant's teeth: `LOOKS.md`),
and at its centre the long dark shape of a ship lying under the ice, keel north-south. The cloud is torn open over the
crater; for the first time a real sun and hard blue shadows (`LOOKS.md`).

**The way down.** The brow descends north into the crater by two ice stairs (each 10 m over 18 m, 29 degrees), with a
landing between, along the skull's inner wall: every way down north is a stair (as Water's rule). It lands on the
lake's south shore at (0, 0, -6).

**The lake.** Centre (0, 0, 24), radius 34 m; the crater rim 14 m above it all round, with eight icefalls she can break
(Decided: she breaks ice off the edges; Proposal: the chunks lie as rough patches with grip and block her lanes). Her skate
loops (Proposal: set loops a player can cut across) are pale curves on the dark ice. The shore is a ring of snow 4 m wide
inside the rim, where the player's footing is sure; on the ice the player slides a little (Decided).

**The ship under the ice.** The burial longship (Decided: a longship rises through the broken lake; Proposal: a burial
ship, perhaps the murdered giant's own) lies at (0, -3, 24), 30 m long, keel north-south, frost-white, seen through the
ice from V7 onward. Over it runs the **keel line**, an old scar of pressure cracks where the ice is thinnest. The lake
cracks only once the blizzard has mostly thinned (Decided); the wolf's weight breaks holes of black water (Proposal); the
last plunge, the one that ends the fight, is placed on the keel line, so that the wolf goes down where the ship is and
the ship comes up through the same hole (Decided: the wolf crashes through and a longship comes up).

**After.** The snow blows off the huntress's body by the hole (Proposal); the choice is made over it (Decided); her skis
stand in the snow beside her (`steel/depth_elements.md`: the Endless Slope is earned from them). The prompt: "Board the
longship". The wind drops for the first time in the land; a low sun under the lifting cloud lays white-gold light along
the lake and the frosted hull, the only warm light in the land (`LOOKS.md`). Where the ship goes is question 7.

**Her ground:** `HoldGround(-34, 34, -10, 58)`: the lake inside the shore ring.

---

## 7. Build list

New pieces are made in code by a `FjordBuilder` (menu `Clover/Build/Fjord`) and a `SkullLakeBuilder`, as the Fire and
Water builders are: static meshes with `MeshCollider`s, ink outlines where the toon material carries one. Each piece
names its parameters; sizes are the defaults the plan uses.

### 7.1 New geometry

| | Piece | What it is | Dimensions and parameters |
|---|---|---|---|
| I1 | **Longships** | Three forms: *heeled* (the bottom a curved wall facing the mountain, the deck tipped toward the camera, gunwale at most 1.4 m on the camera side), *upright* (mast, platform 2.5 m square with a 0.6 m rail, shrouds as a ladder on one side), *frozen in* (hull sunk to 1.4 m) | `Longship(form, length 16 to 22, beam 4.5 to 5, yaw, faceHeight 2.6 to 3)`. The Whale, the Elk, the Wave (heeled); the Raven, the Long Serpent, the jarl's ship (upright); the oarsmen's knarr (frozen in) |
| I2 | **Fjord ice** | The ice floor with wind-scoured snow, clear patches with frozen things under them, pressure ridges (broken lines of heaved blocks, 1.2 m) and the pressure wall (2.5 m) | Section 3.1 |
| I3 | **Open water** | The head pool and the lead: black water, slow floes, a rim of heaved ice 0.6 m along every edge | Lead 4 to 6 m wide from (-23, 0, 124) to (-8, 0, 28) |
| I4 | **Naust** | A long stone-walled boathouse with a turf roof under snow that fades, its doors drifted shut (not walkable inside) | Three, 6 by 20 m, ridge 4 m |
| I5 | **Buried longhouse and trench** | A house as a hump of snow with turf at the eaves; trenches dug between: north walls 2.2 to 2.6 m, south walls 1.2 m, paving at the floor, drift ramps onto the humps | Seven humps; Trenches A to D |
| I6 | **Hall shell** | A roofless hall, walls 2.5 m, two doors, snow floor, two high-seat pillars | The jarl's hall |
| I7 | **Frozen falls and ice stair** | A curtain of ice 14 m high over the head pool; a stair cut beside it with a rope rail | Section 3.1 side path |
| I8 | **Spruce** | A narrow spruce, trunk bare to 3 m, a canopy that fades within 6 m of the player; a **laden** variant whose load drops on a blow (soft cover 4 m for 4 s, a noise of 10 m, stuns a runner under it 1 s) | The forest; laden ones in the glade (4) and the switchback (2) |
| I9 | **Snow surfaces** | Deep snow (4.5 m/s, roll 3.5 m, noise on every step) and hard ground (packed, ice, rock, timber, bone, hides), as a surface tag on every floor | Everywhere; the trails and runs hard, the ground under the trees deep |
| I10 | **Ski run** | A cut lane of packed snow down a slope with two clean ski lines in it | Run 1 (6 m by 70 m); the long run (6 m by 86 m) |
| I11 | **Hunting stand** | A platform 2.5 m square in a great spruce, a rail, a ladder on one side | P2b |
| I12 | **Ravine** | An east-west cleft: sheer north wall, a snow slope of at most 40 degrees on the south, a frozen stream floor between deep banks | Section 3.2, 2E |
| I13 | **Deadfall bridge** | A fallen giant spruce as a ramp, 1.6 m wide | Ravine to its north rim |
| I14 | **Rib** | A curved bone ridge 4 m thick: south face 2.5 m (bone and blue shade), top of packed bone, north side stepping down 0.5 m; a drift ramp at one end; a broken variant with a raised stump platform | Ribs 1 to 6; the Broken Rib |
| I15 | **Spine and vertebrae** | The crest ridge with a blade every 8 m (4 m tall); vertebral drums; the great vertebra; the spinal groove between the vertebrae and their processes | Section 3.3; S3 |
| I16 | **Neck drums and horns** | Six drums stepping 2 m with drift ramps; the skull's base; two horns curving up 18 m | Section 3.3, 3G |
| I17 | **The alpha's ground** | The oval clearing with its ring of trunks and gaps, the fallen spruce, the boulder, the beds, the stake gate | Section 5 |
| I18 | **The hollow** | A sunk floor with a root plate 7 m high, quiet ways laid across deep snow, the elk | Section 3.4 |
| I19 | **The lake** | The crater with its rim and icefalls, the glassy ice with things under it, the keel line, the ship below, the brow and its stairs | Section 6 |
| I20 | **Cast and freed shortcuts** | The frozen sledge (S1), the log boom (S2), the climber's plug (S3), each opened from one side and kept by a progress key | Section 4 |

### 7.2 New behaviour the rooms rely on (for the code worker)

| | What | Where it is used |
|---|---|---|
| C1 | **Huntress**: one actor moving between perches by room; states (unaware, hunting, searching, fallen back); the gaze; the rate per district; the rules of 1.3 | Every room in section 2's perch table |
| C2 | **Sight**: a line test from her eye to the player's chest against hard cover always and soft cover unless marked; the foot zone; the builder's lee map per perch (a 0.5 m grid cast at build time, checked against the plan's rectangles, flagging any that differ by more than 1 m) | Same |
| C3 | **Her shot**: the edge glint, the fixed ground line with its lead, the loose, the strike zone, the pin; cancelled on lost sight; strikes enemies and brittle in the zone | Same |
| C4 | **Striking back**: the reflect (guard turned to her bearing while her line is on the player), the glint as a target for flying shots and spells, the blow on a reached perch; the fall back | Same |
| C5 | **Edge marker** (question 1): her glint pinned to the frame's edge on her bearing; optionally her state | Same |
| C6 | **Noise**: events with radii (1.4), heard by the buried and sleepers; drawn or not (question 4) | The forest, the bones, the hollow |
| C7 | **The pack**: ring slots by Muro's two rules, straight-line steering, slots dropped when blocked, the attack token (shared with her shot), leaping under 1.5 m; the howler's howl, pincer and mark; the following pair that waits off screen and closes on a pin | Everywhere |
| C8 | **The brittle**: chips, shatter by heavy, finisher or her arrow; the shrapnel ring and chains; dormant as hard cover | Harbour, ravine, bones |
| C9 | **The buried**: deep-snow movement toward the last noise, the bulge and grab, forced up by a blow on its line, the edge lunge | Forest, bones |
| C10 | **Snares and deadfalls**: spring, hold, cut free; catch enemies whose straight line crosses them; deadfalls' strip | Glade, switchback |
| C11 | **Spindrift**: the 18 s cycle with its 2 s warning and 4 s veil over the troughs; the squall (the alpha's) | The bones; section 5 |
| C12 | **Sleepers**: asleep, ears that twitch and wake, the silent kill from behind, the cut howl | The hollow |
| C13 | **The alpha** | Section 5 |

### 7.3 What is reused

| Piece or code | Where |
|---|---|
| `FallCatch` | Under the whole scene, the head pool and the lead included |
| `Ladder` (Fire's B6) | The Raven's shrouds, the stand's ladder, the climbers' rope |
| `LandZones` (vistas), with Water's direction (W19), `TopDownCamera.SetFraming` | V1 to V7 |
| `Fadeable`, `OcclusionFader` | Naust roofs, spruce canopies, the hall's walls where they stand south of a floor |
| `RestPoint` | F1 to F6, in stone rings under windbreaks |
| `WeaponPickup` | The plain Great Bow on the jarl's ship |
| `LandVoice` progress keys | S1 to S3, the trial's prize, the frozen archer |
| `Enemy.MakeDormant`, `Enemy.HoldGround` | Dormant brittle, sleepers; the alpha's ground, her lake |
| `DistrictMood` | The light by district (`LOOKS.md`): calm and flat on the fjord, dim and blue under the pines, bright and torn in the bones |
| `SceneDoor` | The horns to `SkullLake` |
| `SunCityBuilder` helpers; Fire's `Plateau`, `Bank`, `Ramp`, `Stairs` | Every terrace, shore, trench floor, ramp and stair |

### 7.4 New kit (Blender, through the `blender-prop` skill)

Longships in three forms and the knarr; the overturned færing; marker poles; nausts; buried longhouses; the hall shell
and high-seat pillars; runestones (knotwork, `LOOKS.md`); woodstack, boat on trestles, well; the palisade gate with
antlers; narrow spruce and its laden variant; the hunting stand; drying racks of hides; snares (sapling and noose) and
deadfalls; the elk carcass; the uprooted spruce's root plate; ribs, vertebrae, spinal blades, neck drums, the skull's
base and horns; the frozen heart; the plain yew bow; the frozen archer and climbers (brittle poses). None of the earlier
lands' kit carries over except as grey-box stand-ins.

---

## 8. Pacing

Minutes for a new player on a clean pass, counted as `fire_land.md` 3.1 counts them (a weak enemy about 30 s, a strong
one 45 s, a group a little less each), with each arrow read and each wait for the wind counted in.

| Stretch | Minutes (new) | Experienced | Tension | What it teaches |
|---|---|---|---|---|
| The fjord mouth, F1 | 1 | 0.5 | Rest | The sponsor; the track |
| The warning shot | 0.7 | 0.3 | Dread | The glint, the line; what is not cover |
| The frozen fleet and the Raven | 2.5 | 1.0 | Rising, then a small peak | Lee to lee; the line does not move; **strike her from her blind side** |
| The frozen oarsmen | 1.5 | 0.6 | Rising | The brittle: chip, shatter, chain |
| The quay, F2 | 0.5 | 0.3 | Rest | |
| The snow trenches | 1.5 | 0.7 | Peak (small) | The pack: the howler conducts; front and back |
| The jarl's yard, the gate | 2.3 | 1.0 | Peak | The pack and her together; the hall's choice; the reflect shown |
| **District 1** | **10** | **4.4** | | |
| The forest's edge, the deep snow | 1.8 | 0.7 | Rising | Sight against sound; the buried |
| The first run, F3 | 1.0 | 0.5 | Wonder, then rising | **She is seen**; cross lanes, do not walk them |
| The snare glade | 2.5 | 1.0 | Peak | The drive; see the snares; snare a runner |
| The ravine and the stand | 2.0 | 0.8 | Stealth | Hidden ground; the climb to her; quiet ice |
| The switchback, F4 | 2.2 | 0.9 | Peak | Everything in the forest on a climb |
| **District 2 (before the alpha)** | **9.5** | **3.9** | | |
| **The alpha** | **4** | **2** | Peak | The pounce and the late dodge toward him; the squall; Bear Them Down |
| The breastbone | 0.5 | 0.3 | Vista | The ribs as bars of shadow |
| Trough 1 | 2.0 | 0.8 | Rising | Lees that thin; the wind's rhythm |
| Trough 2 | 1.5 | 0.7 | Peak | The alley; hide behind bone, not people |
| Trough 3, F5 | 0.3 | 0.2 | Rest | |
| Trough 4 | 2.5 | 1.0 | Peak | The howler's mark beats the wind |
| Trough 5 and the Broken Rib | 1.5 | 0.6 | Spike | Get under her, then up |
| The neck | 1.0 | 0.6 | Dread, vista | She goes through the horns |
| **District 3** | **9.3** | **4.2** | | |
| The brow, F6, the stairs | 0.7 | 0.4 | Wonder | The lake and the ship under it |
| The huntress (both phases, the lake and the ship) | 11 | 7 | Climax | (`ice_land.md`, later) |
| **Clean pass** | **44.5** | **21.9** | | |
| Side paths: the jarl's ship 4, the frozen heart 2, the trial 4 to 6 | + 10 to 12 | + 5 | | |

That sits on the 45 of `WORLD.md`'s proposal. Levers if play runs long, in order: the third runner of the jarl's yard
(0.4), the buried on the switchback's turn (0.4), the following pair in trough 1 (0.6).

**Fires apart (new player's minutes).** F1 to F2 about 6; F2 to F3 about 5.6; F3 to F4 about 7; F4 to F5 about 8.3 with
the alpha; F5 to F6 about 5.7. Within the decided 6 to 8 or close to it: the second is short because the forest wants a
fire before its first lane, the fourth a little long because the alpha's door wants its own fire.

**Enemy count.**

| District | Main path | Count |
|---|---|---|
| D1 | 4 brittle (the oarsmen), 2 packs (a howler and 2 runners; a howler and 3 runners) | 11 |
| D2 | 4 buried, 1 brittle, 1 pack (a howler and 3 runners), the following pair, snares and deadfalls | 11 |
| Elite | The alpha (and up to 4 runners by his howls) | 1 |
| D3 | 2 buried, 5 brittle, the following pair, a pack (a howler and 2 runners) | 12 |
| Side paths | The jarl's ship 3 brittle; the frozen heart 1 buried, 1 brittle; the groove's plug 1 brittle; the trial 6 runners, a howler and a brittle | 14 |
| **All** | | **34 on the main path, the alpha, and the huntress** (Fire 40, Water 33) |

Fewer than Fire's because she is the land's other opponent: about fifteen arrows on a clean pass, each one a read.

---

## 9. Questions for the owner

**Answered (owner, 2026-11-19).**

1. **The glint** is drawn at the edge of the frame, with a crimson line on the snow, plus a quiet mark showing whether
   she has found the player.
2. **Striking back** has three ways:
   - reach her perch unseen;
   - shoot along the glint;
   - **perfect guards with a bar**: each perfect guard of her arrow fills a bar on her. When it is full she falls
     back.
   The owner's conditions: her arrows must not be easy to perfect guard, so the window is tighter than a blade's, and
   it takes several to fill the bar. The numbers are Claude's to propose and tune.
3. **What a hit wins:** she simply falls back. The owner is not sure that rewarding the player for driving her off
   is good, and 50 Shards is too many. If there is a reward at all it must be much smaller. Default: no Shards, or a
   token few, to be tuned.
4. **Noise** shows as rings on the snow.
5. **The trial** is the Sleepers' Hollow.
6. **The plain Great Bow** is a yew longbow on the jarl's frozen ship.
7. **The longship** rises through the broken lake and **sails out down the fjord**, past the frozen ships, now
   thawing. It does not rise into the torn cloud.

Where the plan below differs from these answers, the answers win; the plan is to be brought into line.



Each has two or three options, the recommended one first. The rest of the plan stays Proposal (Claude) until the owner
walks it.

1. **How does her glint show, when the camera never shows her perch?**
   - **(a) The glint pinned to the frame's edge on her bearing, joined to the line on the snow, the creak from her side;
     plus a quiet mark at the same edge of her state (an open ring while she is in the room and has not found the
     player, filling as she finds them, gone when she falls back).** Recommended. Metal Gear Solid V's white arc points
     at a guard who is becoming aware, off screen or not, and fills as his suspicion grows; Nuclear Throne's snipers
     make their aiming line the warning itself; Metal Gear Solid 3's The End is found by his scope's glint. Together
     they show off-screen snipers stay fair when the tell is a direction and a line.
   - (b) The glint and the line only, at the draw; no mark of her state. Purer, and harder: the player cannot tell
     hidden from unseen until an arrow comes.
   - (c) A "hunted framing": while she draws, the camera eases back to show farther toward her, so the ridge and the
     glint are in the world. The decided words kept literally, but the camera moves in the middle of fights with the
     pack.
2. **How can the player strike back?**
   - **(a) Three ways: reach her perch unseen, turn her arrow with a perfect guard, or loose a shot or spell along her
     glint.** Recommended: stealth, the guard and the element each have an answer (pillar 2), and the reflect is learned
     before her fight (the reflect arc). The End in Metal Gear Solid 3 can be fought by sniping back or by sneaking close,
     and guides find the close ambush the better way: both are real choices.
   - (b) Reach and reflect only. A ranged build loses its edge against her; the verb stays stealth.
   - (c) Reach only; the reflect kept for her fight. Simplest, and the guard lesson comes cold in the boss room.
3. **What does a hit on the walk win?**
   - **(a) The room's quiet (no arrows until the next room, no marks from the howls) and her broken arrow (50 Shards).**
     Recommended: a clear, local reward that cannot unbalance her fight.
   - (b) Also a mark in her fight: for each district in which she was struck, one arrow fewer in each of her volleys in
     phase 1 (at most three). Metal Gear Solid 3 lets The End be killed before his fight and the fight is skipped; this is
     a gentler version. Rewarding, but the god fight's tuning then has four states.
   - (c) The quiet only.
4. **Is noise shown?**
   - **(a) Yes: deep snow is slow and loud, hard ground silent, and every noise draws a faint ring on the snow as far as
     it carries.** Recommended. Mark of the Ninja draws every audible noise as a ring and turns stealth from guessing
     into reading; it removes the rings in New Game+, which fits this game's harder replay (`WORLD.md`: the second climb
     is harder).
   - (b) The same rules, not drawn: heard only. More atmosphere, much more guessing, and the buried and the trial get
     harder to read from above.
   - (c) No noise system: the buried follow the player directly and the trial is a plain fight.
5. **What is the land's trial?**
   - **(a) The Sleepers' Hollow: the player hunts a sleeping pack, killing from behind in silence or waking them to
     fight; the Memory on the root plate is the prize.** Recommended. It turns the land's verb round, as the god's second
     phase does, without spending her blizzard; Sekiro's stealth deathblows show an instant kill from behind on the
     unaware sitting well beside hard combat.
   - (b) The Whiteout: a horn on a stone calls a blizzard and the pack hunts the player in it, seen only by breath and
     eyes. Strong, but it is the wolf phase's set piece played early (the reason Water's survival-ring option was not
     recommended either).
   - (c) The Holmgang: a duel on a hide pegged on the ice, where stepping off costs a shield, against a brittle champion
     whose three ice shields shatter. Very Norse; but the Water land's trial is already a duel.
6. **The land's plain weapon: the plain Great Bow, on the jarl's ship.**
   - **(a) There, in this land's make: a plain yew longbow after the Hedeby bow (about 1.9 m, horn nocks), playing exactly
     as the plain Great Bow.** Recommended, as the owner chose for the Chain Sickle in the Water land: the look changes,
     not the weapon. Her own bow of yew and antler stays the god's.
   - (b) There, in the Great Bow's standard look.
   - (c) Dropped by the alpha, on the main path. Sure to be found; the land's "see it, then find the way" loses its prize.
7. **Where does the longship go?**
   - **(a) It rises through the hole, the ice parting, and sails up into the torn cloud over the crater, the giant's
     thoughts in the myth, as the sky of his skull opens.** Recommended: one shot, mythic, and it ends on the land's only
     warm light.
   - (b) It sails across the lake and over the crater's spill, down the mountain, and the land is seen once more from the
     deck as it goes. The biggest payoff, and the most to build.
   - (c) It rises; the player boards; cut at once. Cheapest; the ride is felt only in the hub.

---

## Sources

- The End's scope glint and his moves to a new perch when struck (Metal Gear Solid 3):
  [Game8, beating The End](https://game8.co/games/Metal-Gear-Solid-Delta-Snake-Eater/archives/537926),
  [Game8, sneaking up on him](https://game8.co/games/Metal-Gear-Solid-Delta-Snake-Eater/archives/548404),
  [Destructoid](https://www.destructoid.com/the-memory-card-31-the-end-of-the-end/),
  [Wikipedia](https://en.wikipedia.org/wiki/The_End_(Metal_Gear)).
- Metal Gear Solid V's suspicion indicator, pointing at threats on or off screen and filling as they notice:
  [the official manual](http://mgstpp-app.konamionline.com/manual/ps4/na/en/ps4_02.html),
  [Gamer Guides](https://www.gamerguides.com/metal-gear-solid-v-the-phantom-pain/guide/the-basics-6/gameplay-9/stealth-and-combat).
- Nuclear Throne's sniper, whose laser line is the warning: [Nuclear Throne wiki](https://nuclear-throne.fandom.com/wiki/Sniper).
- Wolf-pack hunting from two simple rules (Muro, Escobedo, Spector and Coppinger, *Behavioural Processes* 88, 2011):
  [ScienceDirect](https://www.sciencedirect.com/science/article/abs/pii/S0376635711001884).
- The attack token in Doom (2016), limiting how many enemies attack at once:
  [Game Developer, "Cyber Demons"](https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-).
- The Anor Londo archers on the buttress, the case against knockback on narrow footing:
  [WhatCulture](https://whatculture.com/gaming/10-most-infuriatingly-awkward-video-game-levels-of-all-time?page=10),
  [Level Warp](https://levelwarp.wordpress.com/2013/07/29/anor-londo/).
- Mark of the Ninja's visible noise rings, and their removal in New Game+:
  [Destructoid interview](https://www.destructoid.com/interview-defining-2d-stealth-in-mark-of-the-ninja/),
  [Wikipedia](https://en.wikipedia.org/wiki/Mark_of_the_Ninja).
- Sekiro's stealth deathblow on the unaware: [Fextralife wiki](https://sekiroshadowsdietwice.wiki.fextralife.com/Stealth).
- Norwegian reindeer drive systems, guide fences converging on pits and enclosures, the model for the land's funnel:
  [Wikipedia, trapping pit](https://en.wikipedia.org/wiki/Trapping_pit),
  [Smithsonian, the Aurlandsfjellet find](https://www.smithsonianmag.com/smart-news/a-hiker-discovered-a-trove-of-artifacts-in-norways-melting-ice-the-site-turned-out-to-be-a-1500-year-old-reindeer-trap-180987710/).
- The Hedeby bow, a Viking Age yew longbow, the model for the plain Great Bow's make:
  [Hurstwic](https://www.hurstwic.org/history/articles/manufacturing/text/viking_bow.htm),
  [The Viking Age Compendium](https://vikingage.org/wiki/index.php?mobileaction=toggle_view_desktop&title=Catalogue_of_Archery_Evidence).
