# The Lightning and Wind land's levels (first pass)

Written by Claude on 2026-11-19 as the Lightning and Wind land's counterpart to `fire_levels.md`, `water_levels.md`,
`metal_levels.md`, `ice_levels.md` and `poison_levels.md`, before any of the land is built. **Everything here is
Proposal (Claude)** unless it is marked **Decided** with its source. The land's myth, gods, people, elites and fight
are `WORLD.md`, "Lightning and Wind" and "The land worked out (2026-11-18, with the user)"; its look is `LOOKS.md`
section 8; the rules every land follows are `WORLD.md`, "Level design rules for every land (decided 2026-11-18,
evening)". The enemies' full numbers belong in a later `lw_land.md`, as `fire_land.md` section 4 holds Fire's; this plan
gives only the numbers the rooms and the race depend on, marked as first guesses.

Axes as in Fire: x east, z north, y up; north is up the screen. Coordinates are metres. "Floor y 22" is the walking
height. The land is two scenes (names proposed): `RivalsMountain` (the shore, the two stairs, the split summit and the
screen room) and `StormRoof` (the roof approach, the roof, the heavens and the storm cloud), as `SunCity` and `SunCourt`
are two. The roof must be its own scene because it stands over the summit's halls: in one scene its walk would be a roof
over walkable floors, which the decided rules forbid. The player wakes at (0, 0, -12) in `RivalsMountain`, standing on the
footing of the great torii in the surf.

---

## 0. What the owner has decided, and how this plan reads it

| Decided | Source | How this plan uses it |
|---|---|---|
| The map is the two criss-crossing stairs | `WORLD.md`, level design rules, 2026-11-18 evening | The two stairs braid up the mountain's face in diagonal legs and cross **at grade** five times; between crossings they enclose four **diamonds** (1.1, 2). Nothing passes over or under anything else |
| The verb is *rhythm*: the rivals' race passes on a drumbeat heard coming; a well-timed dodge through it is rewarded, not only sheltering under a gate | same | The race is a count of eight beats with the strike on the ninth, sounded by the gates the racers pass; shelter is a gate's sill; a perfect dodge through it gives one shared reward, the same for both racers (1.2; Decided, owner, 2026-11-19, answer 1) |
| Every so often Narukami and Kazehaya streak past up the stairs; a lightning strike and a gust sweep the steps; the player dodges or shelters under a gate; at each crossing a small shrine lets the player rest or change stairs | `WORLD.md`, "the rivals on the stairs" | Every crossing is a rest fire and the one calm place on the mountain (1.2, 2). The race never sweeps a crossing |
| The stairs: scorched gates on one, wind-split gates on the other, swapping the lead at each crossing (**Proposal** in `WORLD.md`) | `WORLD.md`; `LOOKS.md` 8 | Used. The swap of lead is heard: the leader's stair strikes on the beat, the trailer's half a beat later (1.2; Decided, owner, 2026-11-19, answer 2) |
| The shore is the drowned shrine of the two murdered first gods, a great torii standing in the surf as the first gate of the stair, a wind-scoured shrine town falling into the sea | `WORLD.md` | District 1. The player wakes under the great torii and the race starts there |
| The mountain is full of life by height: shore town; bamboo and cedar foothills with a village and dry terraced fields; maple and pine slopes with temples, lantern graveyards, waterfalls and rope bridges; high crags with pagodas and wind-bent pines into the cloud (the heights are **Proposal** in `WORLD.md`) | `WORLD.md`, "the mountain is not barren" | One band per diamond, a settlement at every crossing (village, temple, pagoda, the summit's gate), life kept off the racecourse and made useful as warnings (1.4) |
| The split summit: one great hall in two mirrored halves, equal; the castle half with a drum tower and an armour hall; the monastery half with a court of wind bells and a hall of fans | `WORLD.md`; `LOOKS.md` 8 | District 4, two halves of equal size, height and weight (3.4) |
| The elites, one in each half, taken in either order: the Last Retainer (Katana, armour hall) and the Hermit of the Pass (War Fan, court of wind bells) | `WORLD.md`; `LOOKS.md` 8 | Section 6. Their techniques are the drafts Before the Thunder and Turn the Spear (`steel/weapons.md`) |
| The screen room where the halves meet: the two first gods painted as one storm, the last quiet room before the roof; then the stairs to the roof and phase 1 | `WORLD.md`; `LOOKS.md` 8 | Section 7.1; its roof is never drawn while the player is inside (Decided, owner, 2026-11-19, answer 6) |
| The fight is a race to the player; the summit's top breaks away between the transformations; the beasts on the clouds | `WORLD.md`, "the fight" | Section 7 gives the rooms, not the fight, which belongs to `lw_land.md` |
| The ride out is a storm cloud, a mix of both; the first rain falls behind it | `WORLD.md`, rides out; `LOOKS.md` 8 | Section 7.5 |
| The ordinary enemies by area: Heike crabs and ship-ghosts on the shore; the messengers (raiju against crow-tengu runners) and fox-masked shrine guards that wake as the player passes on the stairs; samurai in lightning-struck armour and taiko drummers whose beats call down bolts in the castle half; yamabushi with conch and staff and crow-tengu with feather fans in the monastery half | `WORLD.md` | Section 1.6, each with a mechanic of its own (the owner's bar: no reskins) |
| Two plain weapons on side paths, one for each god's weapon; **Proposal**: a plain Katana in a wreck on the shore, a plain War Fan in a hermit's cave off the stairs | `WORLD.md` | Both used, in this land's make, the meditating hermit kept (3.1, 3.3; Decided, owner, 2026-11-19, answer 7) |
| Rest fires in Fire's rhythm: about one every 6 to 8 minutes, always one at the god's door, one shortcut loop per district back to its fire, roughly 5 to 7 fires, more in the duo lands | level design rules | Nine fires: five of them crossing shrines (decided), one at each elite's door, one at the god's door (2, 8) |
| Two kinds of optional discovery: "see it, then find the way" (holding the plain weapons and lore), and one optional trial invented for this land; the owner's own example: "a race in the Lightning land" | level design rules | The wreck, the hermit's cave, the old straight way's stretches, the lantern graveyard; the trial: the Messengers' Course (section 4; Decided, owner, 2026-11-19, answer 5) |
| Up to three walkable levels per district, joined by ramps, stairs and ladders seen from above; nothing walkable under a roof; roofs and overhangs only fade | level design rules | 1.1: the racecourse, the terrace below its parapet and the terrace above its face; every hall on the walk is roofless or shut |
| Dry storm night, no rain; the cloud lit from inside as the land's main light, ground bolts rare; lightning cyan-violet; wind as pale streaming dust; shrine vermilion the accent; floors combed by the wind | `LOOKS.md` 8 and question 7 (Decided 2026-11-18) | The race's lanes are a branching scorch and a strip of flattened grass; the crossing shrines are the only whole vermilion gates bar the great torii |
| Lightning and Wind is tier 4, a duo land: 55 to 70 minutes for a new player on a clean pass (Proposal in `WORLD.md`) | `WORLD.md`, "How a climb is structured" | Section 9: 60 minutes |

The owner's answers to the earlier plans carry over where they are rules of the game and not of one land: no swimming
(deep water throws the player back, `water_levels.md` 9); marks in the world over HUD marks; at most one held camera
shot per district, rides out not counted (`kit_list.md` Q1, Decided 2026-11-19); small rewards for beating a land's
sweeping threat, not big ones (`ice_levels.md` 9, answer 3).

---

## 1. Principles

### 1.1 What the camera sees: a mountain of two stairs from 12 m up

The camera is Fire's (`fire_levels.md` 1.1): pitch 55 degrees, 12 m, a vertical field of 40 degrees. The picture reaches
7.2 m north of the player and 4.3 m south on flat ground, and is 15.5 m wide at the player and 22 m at the top edge.
Something H metres above the player's floor is in frame only within (5.0 - H) / 0.7 m north. Landmarks are rooms the
player arrives in, or vista zones (`LandZones`, with Water's look-ahead direction). The south-side rule, faded roofs,
enemies that walk straight at the player with no navmesh, and ranged attackers kept in frame all hold as written there.

**The shape: a braid of diagonal legs.** The two stairs climb the mountain's south face in legs that run north-east and
north-west at about 45 degrees. Each stair turns at the edges of the face (the **turns**, at x -32 and x +32) and crosses
the other on the centre line (the **crossings**, at x 0). Between two crossings the two stairs enclose a **diamond**: a
crossing at its foot, a turn on each side, a crossing at its head. Each stair takes one side of each diamond and the
other side of the next, so from high up the face is a chain of X shapes (`LOOKS.md` 8: "the mountainside reads as a chain
of X shapes"):

- the scorched stair (Narukami's): X0, the west turn W1, X1, the east turn E2, X2, W3, X3, E4, X4;
- the wind-split stair (Kazehaya's): X0, E1, X1, W2, X2, E3, X3, W4, X4.

The player climbs one side of each diamond and may change sides at every crossing (Decided: "rest or change stairs").

**Why diagonal legs.** A leg running north-east climbs toward the top-right corner of the picture. Its treads show their
risers to the camera at an angle, so the climb reads as climbing; its uphill side is a face turned toward the camera,
which reads as the next level up; its downhill side is a parapet over lower ground. Along a leg the player sees about 10
m ahead (the frame's top edge is 22 m wide, and a north-east line meets it 10 m out), cut to about 6 m by the leg's rise.
That is enough to see the next gate on a leg, and not the one after: gates come one at a time, which is the rhythm.

**The crossings are at grade.** The two stairs meet on one landing; neither ever bridges the other. A stair over a stair
is a roof over a walkable floor and would hide the player (the Sun City lesson). So every crossing is a level precinct
about 20 by 14 m where four legs and the old straight way meet.

**How the player reads which stair they are on.** Six cues, any two of which are always in frame:
1. *The gates.* Scorched gates are charred black, upright and split, cyan crackle in the cracks of the newest; wind-split
   gates lean downwind, crossbeams snapped and hanging, paint scoured to silver-grey wood, streamers flying (`LOOKS.md`).
   A gate stands every 8 m, so one is always in frame.
2. *The treads.* Cracked and char-black on the scorched stair; scoured pale and sanded smooth on the wind stair.
3. *The lane.* Up the middle of every leg runs the race's own track, laid by a thousand races: a branching scorch line on
   the scorched stair, a strip of grass combed flat and dust ripples on the wind stair. It is the brightest line on the
   floor and points the way up.
4. *The emblem.* The dragon is carved on the scorched gates' posts, the tiger on the wind gates' (`WORLD.md`: the emblem is
   carved, never painted).
5. *The sound.* The scorched gates crackle and hum; the wind gates' loose beams knock and their ropes whistle.
6. *The side.* A stair is always on the opposite side of the face from the last diamond. At a crossing, the shrine's two
   votive tablets name the stairs and point up each (written, click-through; `CLAUDE.md`).

**Height, three levels, and roofs.** The face rises from the surf at y 0 to the summit at y 80: about 80 m over 420 m,
the steepest land so far, but the camera only ever sees a few metres up or down. So the decided "up to three walkable
levels per district" is held as a rule about what is in frame: **within 12 m of any point of a leg, only three floors are
walkable: the racecourse (level 1), the terrace below its downhill parapet (level 0), and the terrace above its uphill
face (level 2).** Faces are 2 to 3 m (a rise that reads); parapets 0.6 m. Interiors and flanks step in terraces of 1.5 m,
each face turned south toward the camera. Every drop faces south or south-west or south-east and is at most 3 m; drops
from an upper leg's platform into a diamond's interior are one way and land within 30 m of a stair back up.

Nothing walkable lies under a roof:
- **Gates are not roofs.** A gate's crossbeams (3.8 m up on the stairs) are drawn while they are north of the player and
  fade within 4 m south of the player and while the player stands under them; the sill under them stays visible. The
  great torii's beam (16 m up) is above the camera and is never in the picture except in the held shot.
- **Houses, halls and shrines** on the walk are shut (not entered) or roofless. The village's struck houses, the temple's
  burned gate, the armour hall and the hall of fans are roofless; every thatch, tile and bark roof that stands fades
  (`Fadeable`) and has no walkable floor under it.
- **The pagoda** is never entered; it fades as the player nears its foot.
- **The screen room** is the one interior. Its roof is never drawn while the player is inside (Decided, owner,
  2026-11-19, answer 6).
- **Rope bridges** span ravines whose floors are not walkable (white water, or cloud with `FallCatch`).

### 1.2 The race

**Decided**: the rivals streak past up the stairs every so often; the race passes on a drumbeat heard coming; a strike and
a gust sweep the steps; the player shelters under a gate or dodges through it, and the dodge is rewarded. **Proposal**:
how, in these parts.

**The racecourse.** The race runs from the great torii in the surf, up the shrine's central corridor, through the parting
court X0, and up both stairs to the summit, Narukami on the scorched stair and Kazehaya on the wind stair, and ends over
the summit wall onto the roof. It is always run the same way, the same leads, the same pace: they are copies who can only
race each other (`WORLD.md`), and the race repeats like a thing caught in time. The race sweeps only the legs (the treads,
the landings, the platforms and the turns, from parapet to face). It never sweeps a crossing, the interiors, the flanks, a
rope bridge, a ladder or a chain: at a crossing the racers leap the precinct and the streak passes overhead.

**The drum is the race.** Every gate the racers pass sounds: the scorched gates boom like a taiko, the wind gates' beams
knock like wooden clappers. The gates stand at an even 8 m on every leg, whole or broken, and the racers keep an even pace,
so the sounds are a steady beat; for the last four gates they sprint, so the beat doubles. What the player hears is the
**count**: the eight gates below them, then the strike on the ninth.

| Beats | What it is | What the player hears and sees |
|---|---|---|
| 1 to 4 (far) | The racers passing the gates far below | The gate sounds from below (the speakers to the south), louder each time; the cloud's inner glow pulses brighter on each; on the first, crows lift off every gate top in frame and deer in the fields bolt uphill (1.4) |
| 5 to 8 (near) | The last four gates below the player | Twice as fast. On each beat every gate in frame answers: the scorched beams crackle cyan, the wind gates' ropes snap taut and their streamers flick uphill; and **the lane on the floor pulses**, a band of light (cyan on the scorched stair, pale dust on the wind stair) running up the lane from the bottom of the frame to the player's feet on beat 8 |
| 9 (the strike) | The sweep | Narukami's bolt or Kazehaya's gust runs up the lane through the frame in a quarter of a second |

The count is the same on every leg of a district, and the tempo rises with the climb, as a taiko piece runs *jo-ha-kyū*,
slow, then breaking, then rushing (sources):

| Where | Far beat | Near beat | Strike after the first beat | Trailer's strike | Race every | Whole gates (with beams) |
|---|---|---|---|---|---|---|
| The shore (the corridor line only) | 1.0 s | 0.5 s | 5.5 s | +0.25 s | 60 s | all |
| District 2: diamonds 1 and 2 (the foothills) | 1.0 s | 0.5 s | 5.5 s | +0.25 s | 50 s | two of three |
| District 3: diamond 3 (the slopes) | 0.8 s | 0.4 s | 4.4 s | +0.2 s | 40 s | one of two |
| District 3: diamond 4 (the crags) | 0.6 s | 0.3 s | 3.3 s | +0.15 s | 32 s | one of two |
| The roof approach (`StormRoof`) | 0.5 s | 0.25 s | 2.75 s | +0.125 s | 15 s | (no gates; the tier stairs) |

**The lead is heard.** Both racers run every race; they swap the lead at each crossing (Decided, as Proposal in `WORLD.md`).
On the stair whose racer leads in that diamond, the strike falls **on** the ninth beat; on the stair whose racer trails,
half a near beat later, **on the "and"**. The trailer's gates sound on the off-beats, so the two counts interleave (boom-
knock, boom-knock) and the player follows their own stair's sound. Narukami leads in diamonds 1 and 3, Kazehaya in 2 and
4, every race. Near the crossings, where both stairs are in frame, both sweeps are seen half a beat apart: a scorch, then a
gust. (Decided, owner, 2026-11-19, answer 2: the count of eight, the half-beat lead and the tempo rising by district.)

**The two sweeps.** Each racer's own, as the two gods are their own:

| | Narukami's bolt (scorched stair) | Kazehaya's gust (wind stair) |
|---|---|---|
| Shape | A forked bolt along the lane, the full width of the leg from parapet to face; its front runs at 60 m/s | A wall of dust 2 m deep rolling up the leg, grass flattening just ahead of it; its front runs at 30 m/s |
| Caught in it | **28 damage and struck**: 0.5 s stunned, crackling | **10 damage, shoved 4 m back down the leg** (along it, never sideways off it) and knocked down 0.6 s |
| After | The lane crackles for 1.5 s: a step on it costs 4 | A trailing curtain of dust 1 s; the grass springs back |
| Enemies in it | Struck the same, except the raiju, who rides it | Shoved the same, a light enemy twice as far, except the crow-tengu runners, who run inside it |

**Shelter.** Every **whole** gate stands on a pale stone **sill** 1.6 m deep across the leg; the strip of floor under its
beams is untouched (Decided, `LOOKS.md`). The bolt jumps from beam to beam over the gates, the way lightning goes to the
high points, and the gust breaks on the posts and the sill, so the sill is safe from both. A broken gate (posts only, no
beam) keeps the beat but gives no shelter, and it looks it: its beam lies in the grass beside the leg. Also safe: the
crossings, a turn's great gate (a sill 3 m deep), stone-lantern alcoves cut into a few uphill faces (2 by 1.5 m, marked as
such), and anywhere off the legs.

**The perfect dodge through the race.** The sweep is unguardable, like a red attack. A roll whose first 0.2 s contains the
front's passing is a **perfect dodge through the race** (the window is wider than an enemy's 0.13 s because the count
tells the player exactly when). A roll whose later invulnerable part (to 0.25 s, `DodgeRoll`) contains it avoids the
sweep with no reward. What a perfect dodge gives is **one shared gift, the same through the bolt and through the gust**
(Decided, owner, 2026-11-19, answer 1; no racer gives a gift of its own):
- **The ordinary perfect dodge**: its 12 mana and its third of a second of slowed time, in which the racer is seen inside
  the streak for a frame (Narukami's face in the bolt, Kazehaya's in the dust), the only sight of them before the summit.
- **Keeping Pace**: for 4 s the player moves 30% faster and rolls 1 m farther.

So the two stairs differ in their sweeps (the bolt stuns where it strikes, the gust shoves the player back down the leg),
not in their gifts, and choosing a stair at a crossing is choosing which racer's rhythm to read. The land teaches the
dodge twice without words: a crow-tengu runner is seen running inside a gust in diamond 1 (3.2), and the first perfect
dodge shows a status line once ("You kept pace with the storm.").

**The first race** is placed: it starts as the player wakes (the racers burst out of the great torii above them and up the
corridor line; the player stands on the torii's footing, under its beam: shelter taught by being given it). The first race
on a leg is forced as the player steps onto the first flight of either stair at X0, with the leg empty of enemies, and
shows a status line once: "A drum below. Under a gate, or through it." After that the clock runs at the district's rate.

### 1.3 Fair on the racecourse

The race is a hazard the player reads, not a trap. These rules keep it so with enemies about:

1. **The near count belongs to the race.** During beats 5 to 8 no enemy starts a red attack, a grab or a ranged attack.
   The only exception is a messenger riding its own racer (1.6): its blow lands on the strike, in the lane, where the sweep
   lands, so one dodge on the beat answers both.
2. **Shelter is never more than a short run from a fight.** Every fight on a leg is placed on a **platform** (a landing 10 by
   10 m) with a whole gate on it; no point of a platform is more than 8 m from a whole sill. Between platforms, whole sills
   are at most 16 m apart in the foothills and 24 m in the slopes and crags. At 6 m/s, with 2 s to break off a fight, a
   player has 21 m in the foothills, 14 m on the slopes and 8 m in the crags: in the crags the dodge stops being a choice at
   the worst places, which is the land's arc.
3. **One sweep per race per stair**, and a struck or knocked-down player is never struck again by that race.
4. **No race on a narrow footing**: ladders, chains, rope bridges and the crags' cliff ledges under 4 m wide are outside the
   lane. A gust shoves only down a leg, and every downhill edge has a 0.6 m parapet (Dark Souls' archers on Anor Londo's
   buttress are the case against knockback over a drop; `ice_levels.md` 1.3).
5. **Waking is quiet.** After a rest or a fall the first race is at least 20 s away; the clock restarts at every fire.
6. **Races at least 30 s apart on the walk**, so a fight has time to be a fight.
7. **Vistas are safe.** A vista opens only between races and while no enemy has noticed the player.
8. **Always heard, the same everywhere.** The count is heard on every leg and turn, from below; it is the same count on
   both stairs of a district, offset only by the lead.
9. **Enemies obey the race.** Every enemy but the riding messengers is struck or shoved by a sweep it stands in, takes the
   same damage, and knows the count as the player does: fox guards go for shelter (1.6), the rest hold still for the near
   beats. A player who staggers an enemy in the lane on beat 7 lets the race finish it.

### 1.4 Life on the mountain without clutter

**Decided**: the mountain is full of life and places, like Sekiro's, and the trees show the wind (`WORLD.md`, `LOOKS.md`).
From 12 m above, life can easily become clutter that hides the lanes the race needs. The rules:

1. **The racecourse is bare.** Within 2.5 m of a leg's lane nothing stands but gates, sills, lanterns in alcoves and the
   race's own marks; nothing within 4 m south of a leg is taller than 1.4 m unless it fades. The life is on the terraces
   above and below and in the flanks, where it frames the leg.
2. **Life lives in the settlements and the bands.** Each crossing is a settlement (the parting court, the village, the
   temple, the pagoda, the Gate of Both), each diamond a band of vegetation with one floor of its own (`LOOKS.md` rule 4):
   grey rippled sand on the shore; dry grass and cracked paddy mud in the foothills; red-brown maple litter streaming
   downhill on the slopes; bare rock and silver grass in the crags; stone and raked gravel on the summit.
3. **Living things are tells.** Crows sit on gate tops and lift together on the first far beat; deer in the dry fields and
   macaques on the crags bolt uphill on it; bamboo thrashes harder in a diamond Kazehaya leads; cedars scarred by raiju
   claws (a real belief: struck trees were scratched by the thunder beast, sources) glow along their scars a beat before a
   raiju blinks onto them. The mountain's life is the race's first warning, as Ghost of Tsushima lets the wind, the grass and
   its birds do the HUD's work (sources).
4. **Everything moves.** Grass in waves combed downhill, dust in streams, paper streamers, shimenawa ropes, leaves, crows,
   bamboo, hearth smoke from the village's one still-burning hearth streaming downwind (the wind's direction drawn in the
   air). A still frame of an empty leg still moves (`LOOKS.md` rule 7).
5. **Canopies fade.** Cedar, maple and pine canopies fade within 6 m of the player and none stands within 4 m south of a
   leg (Ice's spruce rule); bamboo is drawn as a band of culms at a flank's edge, never over a floor.
6. **Props come in clusters at a room's edges**, with open floor in the middle: a scarecrow and a stack of rice-straw
   bundles at a terrace's corner, a jizo row along a graveyard wall, a charcoal kiln and its woodpile in one corner of a
   hollow. No stretch of floor wider than 4 m is one tone (`LOOKS.md` rule 1), but it gets its variety from the floor's own
   marks, not from props.

### 1.5 Measures

| What | Measure | Why |
|---|---|---|
| Walk, roll | 6 m/s; roll 4.5 m, 0.35 s, invulnerable 0.03 to 0.25 s (`DodgeRoll`) | As built |
| A leg | 5 m wide from parapet to face; flights of 1.5 m rise over 2.6 m run (30 degrees) between landings; a leg rises 8 m (foothills) or 9 m (slopes, crags) over about 48 m | A stair that climbs, not a ladder; the average is 10 to 11 degrees |
| A gate | Every 8 m along a leg; posts 4.4 m apart; beams at 3.8 m; a pale sill 1.6 m deep; a turn's great gate a sill 3 m deep | The beat is the spacing; the sill is the shelter |
| A platform | 10 by 10 m on a leg, one whole gate on it, a parapet on its downhill side with one gap (a 2 to 2.5 m drop into the interior) | A fight on the racecourse with shelter in reach (1.3) |
| A turn | 14 by 14 m, the great gate across it | A small room at the face's edge, and the door to a flank |
| A crossing | 20 by 14 m, level, calm: the wind drops, the grass stands up, the shrine's lantern burns still | Rest and the choice of stair |
| The count | 1.2 table | |
| Perfect dodge through the race | 0.2 s window; the ordinary perfect dodge and Keeping Pace | 1.2 (the gift Decided, owner, 2026-11-19, answer 1) |
| A rise that reads; a drop; a step; stairs fought on; ramps | 1.5 m; 2 m; 0.3 m; 27 to 32 degrees; 8 degrees fought on, 15 walked | As Fire |
| A group fight | 12 to 16 m across, 10 to 12 m deep | As Fire (the summit's courts, the shore's streets) |
| The surf's wash (shore) | A wave every 7 s: 4 s breaking and washing to a wet line, 3 s drawing back; walking in the wash 20% slower | The shore's own rhythm, the land's first count (1.6, the crabs) |

### 1.6 The land's people, as the rooms use them

All are **Decided** as kinds (`WORLD.md`); how each fights is **Proposal**, each its own mechanic, each tied to a rhythm.

**The shore**
- **Heike crabs** (crabs carrying the faces of drowned samurai, scuttling from the ruined shrine). They **move sideways**:
  a crab runs only along the line its shell faces across, and turns (0.6 s) to change line, so a player standing north or
  south of a crab's line makes it turn first; from above, the samurai face on the shell shows its facing. They **keep the
  surf's time**: as a wave draws back (3 s) they scuttle out and pinch (a claw grab, 1.4 m, red, held 1 s, 14); as it breaks
  over them (4 s) they **clamp down**, shell sealed, still. Light blows glance off a clamped crab; a heavy **flips** it, belly
  up and open 2 s (the face staring at the sky). Small crabs (0.6 m) die in three light blows whenever they are open; large
  Heike crabs (1.6 m) must be flipped; the **general**, a crab the size of a cart, carries a broken sword in one claw and cuts
  a sideways lane 4 m long with it. Away from the wash (the upper town) they keep a slower count of their own, clamping
  every 6 s.
- **Ship-ghosts (funayūrei)**, pale drowned sailors with ladles, who try to sink the player. They rise only in water knee-deep
  or deeper and never leave it. The **scoop**: the ladle raised dripping (0.8 s), then a lane of seawater thrown 8 m, 1.2 m
  wide. Struck, the player is **soaked** one level (and takes 4): each level makes rolls 0.8 m shorter and walking 10% slower;
  at three the player is **sunk**, knelt 1.2 s, 18 damage, while the ghosts close in with cold hands. Soak drains a level
  every 4 s on dry ground and at once at a fire. The legend's answer is a ladle with its bottom knocked out (sources): **a
  blow on the ladle during the scoop's wind-up, or a perfect guard on the throw, knocks its bottom out**: the ghost scoops
  nothing, bewildered, open 3 s; killed then, it is at peace and does not rise again. Killed any other way, it sinks and rises
  elsewhere in the same water 8 s later.

**The stairs**
- **Fox-masked shrine guards** (white masks, the palest things on the stairs, `LOOKS.md`). They stand as statues in pairs
  flanking a gate and **wake when the player passes through that gate** (or strikes one). One of the pair is real; the other
  is a **fox-fire double**, identical except that it **casts no shadow** and its eye-slits glow blue. They fight to the bell
  on the real guard's staff, a **four-count**, one beat every 0.8 s: three jingles, and both strike on four, the double's blow half a beat
  after the real one's (an echo; 60% damage; guardable). A blow on the double bursts it into blue fox-fire (no harm to anyone)
  and it reforms on the next four. Kill the real one and its double goes out. **They keep the gates**: when a race's near count
  begins, the real guard steps onto the nearest whole sill and **shuts it**, its staff across the posts, so the player cannot
  step onto that sill; the double stays where it is and bursts in the sweep. So the race shows which is real, and a pair can
  take the shelter the player was running for. A guard staggered before the near count cannot take a gate.
- **The messengers**, the gods' runners, **racing each other**. They come as a **raiju** and a **crow-tengu runner**, and when
  both are within 12 m of the player they **race to the player**, the gods' rule in small (Decided for the fight; Proposal
  here): each commits to its attack and its path shows on the floor 0.6 s ahead (the raiju's a short jagged cyan line, the
  runner's a pale streak); the shorter path arrives first and its blow lands; the other's is cut short and it skids past, open
  0.8 s; and **if their paths cross they crash**, both staggered 1.5 s. A player who stands so that the two paths must cross
  gets two staggered messengers. This is the fight's lesson taught twenty times before the summit, as the hook brother shows
  the fisher's moves.
  - The **raiju** (the thunder beast that falls with lightning; sources). It moves only between **high points**: gate beams,
    stone lanterns, the tops of scarred cedars and bamboo; it **blinks** from one to the next in a crackle (one blink a
    second), a beat before it lands the high point glows. From a perch it **pounces** on the player (a ring marked 0.6 s ahead,
    red), lands, rakes (two claw blows that leave scorched claw marks crackling for 2 s, 4 a step), then blinks up again after
    2 s on the ground. On a perch it is out of reach; on the ground it is open. On the scorched stair during a race it **rides
    the bolt**: it blinks with the sweep and its pounce lands on the strike.
  - The **crow-tengu runner**, black, straw-sandalled, faster than the player. Its attack is the **run-through**: an 8 m pale
    lane cut as it passes, never stopping or turning mid-run; it turns 10 m on and runs back. Step out of the lane and its back
    is open as it goes by. On the wind stair during a race it **runs inside the gust**, its run-through landing on the strike.
    Downhill it runs farther (10 m).

**The castle half**
- **Struck samurai** in black lacquered armour split by lightning. The armour takes light blows at 25% and no stagger; heavies
  dent it. A bolt landing within 2 m **charges** a samurai for 4 s: it crackles, moves a third faster, and its cuts shock (a
  guarded cut costs double stamina); **a charged samurai charges any other within 3 m** (an arc jumps between them, and through
  the player if they stand in the gap). After 4 s it **discharges**: the armour bursts open, the samurai is stunned 2 s and
  fights on unarmoured. The way to fight them is to bring a bolt down on them and outlast the four seconds.
- **Taiko drummers** on drum stands 1 m up at a court's east and west edges (in frame). Each plays a five-beat phrase, 0.5 s a
  beat: on beat 3 a scorch ring (2.5 m) marks the player's floor, on beat 5 a bolt falls in it. Two drummers play call and
  response, so their bolts alternate. A drummer struck loses the phrase and starts again; killed, its drum falls silent. A
  silent drum's skin can be slit (one press), and a slit drum stays silent after a rest (a progress key). The tower's great
  drum, in the drum court, booms when the player strikes it with a heavy: a shove ring 3 m and one bolt on the nearest
  samurai. So the bolt that hunts the player is also the thing that opens the samurai.

**The monastery half**
- **Yamabushi**, mountain ascetics with conch and ringed staff. The **conch**: three jingles of the staff, then a 1.2 s blast,
  and for 8 s **the wind blows the way the conch faced**, across the whole court: everyone not braced drifts 1.5 m/s downwind,
  rolls downwind go 6.5 m and upwind 2.5 m, thrown things bend, and the raked gravel combs itself into lines that show it. The
  blower is braced (staff planted) and cannot be moved while it blows; a blow during the 1.2 s cuts the blast short and leaves
  him coughing, open 1.5 s. His staff sweeps 2.5 m and rings.
- **Crow-tengu with feather fans**, lighter than the runners. They perch on bell frames, screens and walls at most 1 m up, flap
  twice, and **swoop**: a pale lane 6 m long, 10 m downwind in a conch wind, landing behind the player with a fan cut. From a
  perch they throw a **fan crescent** (a wind blade, 10 m, guardable, shoves 1 m). In a conch wind they drift twice as far as
  anyone else, and anything that shoves them (a gust, a War Fan) sends them twice as far.

---

## 2. The whole land

The land climbs out of the surf through the great torii, round the drowned shrine town to the shrine's back court, up the
braid of the two stairs through the foothills, the village, the slopes, the temple, the crags and the pagoda, to the Gate of
Both; through the two halves of the summit to their elites; through the screen room; up onto the roof, through the cloud and
off on the storm. Nine rest fires. The main path is about 760 m of walking on one side of each diamond.

```
 RivalsMountain
 z
424 .................. SCREEN ROOM y78  F9 (0,78,412) ............ -> StormRoof
412  ARMOUR HALL y80 (Retainer)  |   CENTRAL  |  BELL COURT y80 (Hermit)
392  S4c door ->                 |   COURT    |  <- S4m door
388  F7 (-46,78,388)  DRUM COURT y78 [tower]  y76 seam |  HALL OF FANS y78  F8 (46,78,388)
376  BAILEY y76                  |            |  GRAVEL COURT y76
350 ===================== GATE OF BOTH (0,74..76,350) ======================
340  X4 the Gate of Both y74, F6 (0,74,340)                 trial's finish bell (48,74,336)
322     P8w (wind)                  P8s (scorched)          | THE MESSENGERS' COURSE
304  W4 (-32,65) pines, macaques  [crag, the chains]  E4 (32,65)  (trial, E flank)
288     P7w           PAGODA (0,58,284)         P7s         | start bell (48,60,280)
268  X3 the pagoda crossing y56, F5 (0,56,272)
250     P6s (scorched)                          P6w (wind)
232  W3 (-32,47) | ravine, waterfall | [temple hall] | E3 (32,47) | LANTERN GRAVEYARD (E flank)
     HERMIT'S CAVE (-64,52,238) (side, W flank)
216     P5s           temple court y42            P5w
196  X2 the temple crossing y38, F4 (0,38,198), burned sanmon N
178     P4w (wind)                                  P4s (scorched)
160  W2 (-32,30) bamboo, kilns | [cedar avenue, grandfather cedar] | E2 (32,30) struck cedars
144     P3w                                         P3s
124  X1 the village crossing y22, F3 (0,22,125); the village x -36..36, z 106..146
106     P2s (scorched)                              P2w (wind)
 88  W1 (-32,14) cedars, cliff path W | [dry terraced fields] | E1 (32,14) bamboo
 73     P1s                                         P1w
 52  X0 the parting court y6, F2 (-8,6,56)
 40  haiden rock y4 | central corridor (broken z24..30) | upper town y3.5..6, Upper Street ---->
 14  stage y1.5 ==== east corridor y1.5 ====> waterfront (wash) y0.4, boat basin, slipway
 -12 @ wake under the GREAT TORII (0,0,-12), F1 (-5,0,-9)     wreck on its sandbar (-52,0,-8) (side)
      x: -64      -32          0          32         64
```

**Heights.** Surf floor -0.4 (deep water south of z -26); the torii's footing and the sand spit 0 to 0.3; the stage and the
corridors 1.5; the waterfront 0.4; the upper town 3.5 to 6; the haiden 4; X0 6. Foothills: W1/E1 14, X1 22, the village 18 to
28, W2/E2 30, X2 38. Slopes and crags: the temple court 42, W3/E3 47, X3 56, the pagoda's foot 58, W4/E4 65, X4 74. Summit:
the central court and the outer courts 76, the drum court and the hall of fans 78, the armour hall and the bell court 80, the
screen room 78. `StormRoof`: its own heights (section 7).

**The four diamonds.**

| Diamond | From, to | Turns | Scorched side | Wind side | Leads | Band (interior; flanks) |
|---|---|---|---|---|---|---|
| 1 | X0 (0,6,52) to X1 (0,22,124) | W1 (-32,14,88), E1 (32,14,88) | via W1 | via E1 | Narukami | Dry terraced fields; cedars and the sea cliff (W), bamboo (E) |
| 2 | X1 to X2 (0,38,196) | W2 (-32,30,160), E2 (32,30,160) | via E2 | via W2 | Kazehaya | The cedar avenue; struck cedars (E), bamboo and charcoal kilns (W) |
| 3 | X2 to X3 (0,56,268) | W3 (-32,47,232), E3 (32,47,232) | via W3 | via E3 | Narukami | The temple's upper precinct; the waterfall ravine (W), the lantern graveyard (E) |
| 4 | X3 to X4 (0,74,340) | W4 (-32,65,304), E4 (32,65,304) | via E4 | via W4 | Kazehaya | The pagoda's crag and the chains; wind-bent pines and macaques (W), the Messengers' Course (E) |

**Rest fires (nine).** In this land a rest fire is a stone lantern whose flame burns still while the wind tears at
everything round it; the wind drops in a ring 4 m round it.

| Fire | Where | Foot | Why there |
|---|---|---|---|
| F1 | The great torii's footing, under its west post | (-5, 0, -9) | In sight of waking |
| F2 | X0, the parting court behind the drowned shrine | (-8, 6, 56) | The foot of both stairs; S1 comes back toward F1 from beside it |
| F3 | X1, the village crossing's shrine | (0, 22, 125) | Crossing shrine (Decided) |
| F4 | X2, the temple crossing's shrine | (0, 38, 198) | Crossing shrine; S2 comes down to it |
| F5 | X3, the pagoda crossing's shrine | (0, 56, 272) | Crossing shrine; S3 comes down to it |
| F6 | X4, the Gate of Both | (0, 74, 340) | The last crossing; both halves' back doors come back toward it |
| F7 | The castle half: the drum court's north end, at the armour hall's door | (-46, 78, 388) | The Retainer's door |
| F8 | The monastery half: the hall of fans' north end, at the bell court's door | (46, 78, 388) | The Hermit's door |
| F9 | The screen room | (0, 78, 412) | The gods' door (Decided: always one there) |

Five of the nine are the decided crossing shrines; the elites' doors take one each, as `ice_levels.md` and
`water_levels.md` gave their elites; the gods' door its own. Nine sits above Fire's six because the land is half as long
again and holds two elites and two gods ("more in the duo lands", Decided). Lever: if the owner wants fewer, F7 and F8 go and
the halves' back doors (S4) carry the runbacks.

**The route.** Wake under the great torii (F1); the race bursts out over the player. North along the spit to the stage; east
along the east corridor among the ship-ghosts; along the drowned waterfront in the wash with the crabs; up the slipway by the
boat basin; west through the sake storehouse yard and along the Upper Street; through the shrine's east gate and its fox pair
into the parting court (F2). Up either stair through diamond 1 to the village (F3); either stair through diamond 2 to the
temple (F4); either through diamond 3 to the pagoda (F5); either through diamond 4 to the Gate of Both (F6). Through the gate
into the central court; one half, its rooms, its fire (F7 or F8), its elite, out by its back door; the other half the same;
through the two seals into the screen room (F9); up into `StormRoof`: the roof approach, the roof, the break, the heavens, the
storm cloud.

**Branches and loops.**
- The wreck and the plain Katana (see it, then find the way): seen from the stage; reached from the village by the cliff path.
- The hermit's cave and the plain War Fan (see it, then find the way): seen from W3; reached from the pagoda by the ravine rim
  and the chain behind the waterfall.
- The old straight way: the first gods' pilgrim stair up the middle of every diamond, choked since the race braided the
  mountain. Each stretch is opened from above (three of them are the shortcuts) and goes only down. Its lore is the land's.
- The other side of each diamond: every diamond's second side is optional and is the way back down to explore.
- The lantern graveyard (lore; a race-free maze of lanterns), off E3.
- The Messengers' Course (the trial), on the east flank of diamond 4.

**Vista zones** (`LandZones`; the look-ahead may point any way, Water's W19).

| | Where | Box | Looks | Shows |
|---|---|---|---|---|
| V1 | The torii's footing, (0, 0, -12) | 8 by 4 m | North | **Held shot 1** (below) |
| V2 | The stage's west end, (-12, 1.5, 14) | 3 by 6 m | West | Across the deep channel: the wreck on its side on a sandbar, its mast pointing up the mountain, a glint at its stern |
| V3 | X0's north edge, (0, 6, 58) | 10 by 3 m | North | The parting: the first scorched gate going north-west and the first wind gate north-east, the dry fields stepping up between them |
| V4 | The turns W1 and E1, (-30, 14, 88) and (30, 14, 88) | 4 by 6 m each | East from W1, west from E1 | Across the fields to the other stair's leg, its gates, and a race running up it half a beat off: the two stairs read as two |
| V5 | X2's north edge, (0, 38, 203) | 10 by 3 m | North | The burned temple gate, the temple court, the bell |
| V6 | W3's bridgehead, (-40, 47, 233) | 3 by 4 m | West | The rope bridge hanging broken into the ravine, the waterfall, and in a cave mouth across, a seated figure with a fan on its knees |
| V7 | X3's south lip, (0, 56, 263) | 12 by 3 m | South | **Held shot 2** (below) |
| V8 | X4's north edge, (0, 74, 347) | 12 by 3 m | North | The Gate of Both, its left leaf charred and its right leaf scoured; the two halves' walls, sloped stone and swept bark |
| V9 | The central court's north end, (0, 76, 397) | 10 by 3 m | North | The screen room's doors and their two seals (the frame `lw_split_summit`) |
| V10 | The roof walk's south end (`StormRoof`) | 12 by 3 m | North | The ridge walk, the finials, the two gods at its far end |

**Three held shots**, two in the land and the ride (`kit_list.md` Q1, Decided 2026-11-19: at most one per district,
rides out not counted; this plan keeps the two it had and the ride):
1. *The start* (V1): as the player wakes, the camera eases back to 25 m and up, the great torii's beam in frame for the only
   time, and the first race bursts out of the gate over the player, a bolt and a gust running north up the corridor line, over
   the drowned shrine and up the two stairs into the cloud. 6 s.
2. *The braid* (V7): at the pagoda crossing the camera pulls back to 30 m and looks **south down the mountain**: the chain of X
   shapes, the five crossing lanterns burning still, and a race running up it below the player (forced as the player enters
   X3; it does not sweep the crossing). 6 s. It is the land's one image, `LOOKS.md`'s "two stairs of weathered gates
   zigzagging up... crossing and recrossing", seen from inside.
3. *The ride* (section 7.5).

---

## 3. The districts

Each district: the idea, its three levels, a plan, then each space in walking order. Encounters are numbered E1.1 and so on,
as in Fire; on the stairs each diamond's two sides are given as *s* (scorched) and *w* (wind), and a new player meets one of
each pair.

### 3.1 District 1: the drowned shore (z -30 to 60)

**The idea.** The shrine of the two first gods was built on piles over a bay, as Itsukushima's is, so that worshippers need not
set foot on holy ground (sources). The land has sunk and the sea has come in: the great torii stands in the surf, the stage and
corridors stand on bare piles with their roofs blown off, the main hall's roof has slid into the sea, and the shrine town along
the bay's east side is going under, its lower street washed by every wave. The race starts here, at the first gate. The player
walks a U: east along the corridor, up through the town, west along its upper street to the shrine's back, where the stairs
begin. The shore's own rhythm is the surf's: a wave every 7 s, and the crabs keep its time. **Levels:** the surf and the
waterfront (y -0.4 to 0.4); the stage and the corridors (y 1.5); the upper town and the shrine's rock (y 3.5 to 6).

```
 z     x: -64   -40   -20     0     20     40     60     72
 60          cliff      | X0 PARTING COURT y6  F2*(-8,6,56) |<== Upper Street y6 -> 3.5 ==|
 50                     |  [honden, shut]          east gate (14,6,50) fox pair      houses N
 44                     |  haiden y4 (roofless)    |        storehouse yard y4.5 (36..52, 32..46)
 34                     |  corridor N half, beam   |                       boat   |slipway
 24          deep channel  ~~ gap z24..30 ~~       |  upper terrace y3..4  basin  |y0.4->3
 20     WRECK on   ~~~~  STAGE y1.5 (-14..14, 8..20)  sea wall 2.6 m ~~~~~~~~~~~~~~~~~
 12     sandbar   ~~~~   ===== EAST CORRIDOR y1.5 (14..40, 10..14) ====> WATERFRONT y0.4 (wash)
  0     (-52,0,-8)  ~~~  spit y0..0.3  ~~  toppled lanterns  ~~   drowned houses tilted
-12        ~~~~~~~~~  [ GREAT TORII (0,-0.4,-12) ] @ F1*(-5,0,-9)  ~~~~~~~~~~~~~~~~~~
-26   ......................... deep water: throw-back ..........................
```

**The water.** South of z -26 the sea is deep (thrown back to firm ground, 1.2 s, 10 damage, `water_levels.md` 1.2). From z -26
to the shore the surf is knee-deep over grey rippled sand (y -0.4), walkable, ghost water; it is wading by Water's one depth rule
(4 m/s, rolls 3 m; `kit_list.md` Q6, Decided 2026-11-19), where this plan first had it 20% slow. The deep channel west of the
spit and under the broken corridor (y -1.5, 1 m deeper than the surf) is not walkable. The **wash**, kept as this land's own
named surface (20% slower; Q6): every 7 s a wave breaks across the waterfront's paving to its wet line and draws back (1.5 table).

#### 1A. The great torii (x -7 to 7, z -15 to -9, y 0)
- *Shape:* the torii's footing, a platform of dressed stone just above the surf, the two great posts at (-5, -12) and (5, -12)
  with their four leaning auxiliary legs, foam round them. The gate is 16 m tall, vermilion, whole: the one whole vermilion
  thing in the land (`LOOKS.md`).
- *Sees on waking:* the posts either side, the spit running north out of the frame, toppled stone lanterns in the surf; and the
  held shot (V1).
- *Rest:* **F1** at (-5, 0, -9), a stone lantern at the west post's foot.
- *Enemies:* none. The sponsor speaks on waking.
- *The race:* every 60 s it starts here. The footing is the great torii's sill: the player waking under it is sheltered.

#### 1B. The spit and the stage (z -9 to 20)
- *Shape:* a spit of sand 12 m wide (x -6 to 6) running north 17 m, toppled lanterns either side in the surf; three steps up
  (1.5 m over 2.6 m) at z 5 to 8 onto the **stage** (hirabutai), x -14 to 14, z 8 to 20, y 1.5, a dancing floor of grey boards on
  bare piles, vermilion posts round it, a low rail (0.6 m) on its south edge. North of it the **central corridor** (x -2 to 2)
  runs on toward the main hall and breaks off at z 24: six metres of it lie in the water.
- *Vista:* **V2** at the stage's west end: the wreck (see it, 3.1 side path).
- *Enemies:* none. The corridor line north is the racecourse: the player watches the race run up it, over the gap, and on.
- *Lore:* **L1**, votive boards (ema) hung on the stage's east rail at (13, 1.5, 15), each painted with one figure: a drum on some,
  a bag of wind on others, and on the oldest, worn nearly blank, the two together as one cloud. Words (Claude's, new, the
  oldest board): "For the storm that comes in from the sea: thunder in it, wind under it, rain behind it. Pass us by."

#### 1C. The east corridor (x 14 to 40, z 10 to 14, y 1.5)
- *Shape:* a corridor on piles, 4 m wide, 26 m long, running east; its roof gone, its vermilion posts standing every 2.4 m on both
  sides (a rhythm the eye counts), surf on both sides. At x 40 it steps down (1.1 m over 2 m) onto the waterfront.
- *Enemies:* **E1.1** two ship-ghosts: one in the surf south of the corridor at (24, -0.4, 7) (within the frame's bottom edge),
  one in the cove north of it at (33, -0.4, 18). The south one scoops first. The lesson: the soak, and the ladle's bottom
  knocked out (struck in the wind-up). The corridor is too narrow to sidestep a 1.2 m lane comfortably and the ghosts cannot
  follow onto it: kill the ladle, or run.
- *Why it suits them:* they need water and a player who cannot leave the line of their throw; the corridor is both.
- *Kit:* corridor on piles (M7), `stone_lantern` toppled x4, `ship_ghost` x2.

#### 1D. The waterfront (x 40 to 72, z 4 to 16, y 0.4)
- *Shape:* the town's lower street, now the beach: paving under a skin of sand, washed by every wave to its **wet line** at z 14
  (a dark band of wet paving and a line of foam and weed, so the reach is always shown). South of it the lower houses lie tilted
  into the sea, roofs in the water (not walkable; their roofs fade within 4 m south of the player). North of it the **sea wall**,
  2.6 m of fitted stone facing the camera, with the upper town on top. Fishing nets hung on poles along the wall, a boat on
  its side, a stone water trough.
- *Enemies:* **E1.2** five small crabs and one Heike crab, coming out of the drowned houses on the south side as the player steps
  off the corridor. The wash teaches their time: they scuttle and pinch in the backwash and clamp on the break. Fight them on the
  break (flip the big one with a heavy while it is sealed, finish it before the backwash), sidestep their lines in the backwash.
- *Why it suits them:* the wash is a count the player can see coming (the wave's foam line enters at the bottom of the frame and
  runs north past the player to the wet line), and the street is long and east-west, so the crabs' sideways lines run along it.
- *Kit:* drowned house (M9) x4 tilted, sea wall (B2's ashlar face), nets, `heike_crab` small x5, large x1.

#### 1E. The boat basin and the slipway (x 52 to 72, z 16 to 30)
- *Shape:* at the waterfront's east end a stone **slipway** (x 62 to 72) climbs north from y 0.4 to y 3 over 14 m (10 degrees) to
  the upper terrace. West of it the **boat basin** (x 52 to 62, z 16 to 28), a square harbour of black water knee-deep at its
  edges and deep in the middle, two half-sunk boats in it.
- *Enemies:* **E1.3**, the district's peak: two ship-ghosts in the basin at (54, -0.4, 22) and (60, -0.4, 26), west of the
  slipway (in frame, at most 6 m north as the player climbs), a Heike crab on the slipway at (67, 1.5, 22) and three small crabs
  at its foot. The crabs press; the ghosts soak; a sunk player on a slope is in trouble. The answers: fight up the slipway out of
  the ghosts' 8 m, or knock the ladles' bottoms out first. The slipway is above the wash: its crabs keep their own slower count
  (6 s).
- *Kit:* slipway (M8), basin, `boat_sunk` x2.

#### 1F. The upper town and the sake storehouse yard (x 14 to 72, z 18 to 47, y 3 to 4.5)
- *Shape:* the upper terrace of the town, behind the sea wall: the stone yard of a sake brewery (x 36 to 52, z 32 to 46, y 4.5),
  casks stacked 1.4 m along its walls, its storehouse shut (white plaster, fading roof); garden plots gone to dry grass; a small
  street shrine with a fox pair on it (dormant; see 1G). Broken low walls (1.2 m) along the south.
- *Enemies:* **E1.4** the **general**, the crab the size of a cart, in the storehouse yard at (44, 4.5, 40), clamped among the
  casks until the player enters. Away from the wash it keeps its 6 s count: it scuttles and cuts its sideways lane, then clamps.
  A heavy flips it; it rights itself after 2 s; it takes three flips. A mini-peak after the basin, alone.
- *Lore:* none.
- *Kit:* casks, storehouse (fading), `heike_crab` general.

#### 1G. The Upper Street and the shrine's east gate (x 14 to 66, z 47 to 53; y 3.5 at the east end, 6 at the west)
- *Shape:* the town's main street, 6 m wide, climbing west over 52 m (3 degrees) between one-storey shops on its north side (shut,
  roofs fade) and the low walls of the upper terrace on its south. Paper lanterns burst on their poles; a shop curtain (noren)
  streams downwind. At its west end the **east gate** of the shrine precinct, a stone torii at (14, 6, 50), its two fox statues on
  plinths either side.
- *Enemies:* **E1.5** the first **fox pair**, waking as the player passes through the east gate: one real, one a double. The race
  runs 30 m away on the corridor line (the parting court is on it), so the count is heard; when it comes, the real guard runs for
  the precinct's nearest sill and the double stays: the first time the race shows which is real.
- *Kit:* `shop_row` (M9), `stone_torii`, `fox_guard` x2.

#### 1H. The parting court, X0 (x -14 to 14, z 44 to 60, y 6)
- *Shape:* the shrine's back court, cut into the rock behind the main hall. In its south part the **honden** (the inner sanctuary,
  shut, its roof fading) with its back to the court; the **haiden** below it to the south (y 4, roofless, its roof lying in the
  sea), reached by side stairs (x ±10 to ±14, z 40 to 44). At the court's north corners the **two stairs' first gates**: the
  scorched stair's at (-10, 6, 60) leading north-west, the wind stair's at (10, 6, 60) leading north-east; between them the old
  straight way's first flight, collapsed into a 2.5 m face (it is a way down, 3.2).
- *Rest:* **F2** at (-8, 6, 56).
- *Vista:* **V3** at the north edge.
- *Lore:* **L2**, the honden's doors, carved with the dragon and the tiger facing each other, and between them, where the shrine's
  god should be named, a smooth square chiselled blank. Words (Claude's, new): "The race goes up both ways. The old way went up
  one." / "Before there were two, the storm climbed straight."
- *The race:* the racecourse runs through the court on the line x 0 (the corridor line) and parts at the first gates. The court
  itself is a crossing: calm.

#### Side path: the wreck and the plain Katana (the sandbar, x -64 to -40, z -20 to 4)
- *Seen first:* from V2 on the stage: a warship of the old wars lying on its side on a sandbar across the deep channel, its deck
  tipped toward the camera, its mast pointing up the mountain (`LOOKS.md`), and a glint at the stern. It cannot be reached: the
  channel is deep.
- *The way:* from the village (X1) a **cliff path** runs south-west from the village's west lane at (-36, 20, 120) along the top of
  the sea cliff, past the outside of the turn W1 (it joins W1's landing at (-40, 14, 92)), and down a **fishermen's stair** cut in
  the cliff, running south, from (-48, 14, 40) to (-52, 0, 6) (14 m over 26 m in five flights with landings, 28 degrees). A
  player on the scorched side of diamond 1 meets it at W1 sooner.
- *The wreck:* 24 m long, keel to the west, its deck a walkable floor tipped 18 degrees toward the camera, a deckhouse split open
  (roofless). The sandbar round it is wash and crab ground.
- *Enemies:* a crab swarm (six small, two large) from the hold; two ship-ghosts in the channel on the sandbar's east side.
- *The prize:* **the plain Katana** (`WeaponPickup`) in the stern, across the knees of an empty suit of armour seated against the
  rail, in this land's make: a plain uchigatana in a salt-whitened black scabbard (Decided, owner, 2026-11-19, answer 7).
- *Lore:* **L3** on the deckhouse's lintel. Words (Claude's, new): "We prayed to the storm to carry us home. It answered in two
  voices, and they argued, and we sank between them."
- *Home:* the stage is 26 m east across the deep channel and cannot be reached; the way home is back up the fishermen's stair
  to the cliff path, W1's landing 50 m on and the village beyond. The wreck is a dead end on purpose: it is the land's first
  reward for looking, and its way back climbs past the place it was found from.
- *Minutes:* 4 for a new player.

### 3.2 District 2: the foothills (diamonds 1 and 2, X0 to X2; z 52 to 196)

**The idea.** The stairs leave the shrine and climb through the foothills' fields, bamboo and cedar to a farming village at the
first crossing, and on through the cedar forest to the mountain temple. The race is learned here: slow counts, two whole gates in
every three, the messengers met one at a time, then racing. **Levels:** the racecourse (level 1); the terraces below its parapets
(the fields, the village's lower yards; level 0); the terraces above its faces (the bamboo and cedar slopes, the village's upper
yards; level 2).

```
 z     x: -48  -32     -16      0      16     32     48
196   ........................ X2 TEMPLE y38 F4 ...........................
188              P4w (-18,34,178)  \  cedar  /   P4s (18,34,178)
170     bamboo,    \               | GRANDFATHER CEDAR fallen (drop) |       struck
160     kilns   W2 (-32,30)        |  CEDAR AVENUE (straight way)    |  E2 (32,30)  cedars
144              P3w (-18,26,144) /                 \ P3s (18,26,144)
124   .......... village ..... X1 VILLAGE CROSSING y22 F3 ........ village .......
106              P2s (-18,18,106) \  fields, scarecrows  / P2w (18,18,106)
 88   cliff path  W1 (-32,14)      |  DRY TERRACED FIELDS |      E1 (32,14)  bamboo
 73   to wreck   P1s (-20,10,73)  /   field stair (down)  \ P1w (20,10,73)
 52   ..................... X0 PARTING COURT y6 F2 ........................
```

#### 2.0 The legs of district 2 (both diamonds)
- Every leg is 5 m wide, rises 8 m over about 48 m in flights and landings, carries a gate every 8 m (six per leg, two in three
  whole), and has one **platform** at its middle. Lower legs (from a crossing up to a turn) have the diamond's interior **above**
  their uphill face; upper legs (from a turn up to a crossing) have it **below** their parapet. Each upper leg's platform has a
  gap in its parapet: a 2 m drop south into the interior (one way).
- The **scorched stair** in this district: charred gates, the burst lanterns in its alcoves, black treads, the scorch lane. The
  **wind stair**: leaning silver-grey gates with streamers, pale treads, the combed lane.

#### Diamond 1 (X0 to X1; Narukami leads: the scorched stair strikes on the beat, the wind stair on the "and")

**The first race.** Forced as the player steps onto the first flight of either stair at X0 (1.2); the first gate is 3 m up the
leg, whole; no enemy on the leg below its platform.

**Scorched side (via W1).**
- *Lower leg, X0 to W1:* cedars on the downhill (west) side, their canopies 5 m clear of the parapet; above the face on the east,
  the first terrace of the fields. **P1s** (-20, 10, 73).
  - **E2.1s** a lone **raiju** on P1s's gate beam (-20, 13.8, 74). It blinks between that beam and the next gate up (8 m on),
    pounces, rakes, blinks up again. The lesson: watch the perch glow, read the ring, strike it on the ground. In the first race
    it meets, it blinks with the bolt and pounces on the strike: a dodge on the beat clears both.
- *W1, the turn* (x -40 to -26, z 82 to 96, y 14): a great gate across the landing (two beams, a 3 m sill), a lightning-split
  cedar standing at its west corner with raiju scars, and the **cliff path** leaving west (3.1 side path). **V4** looks east from
  here across the fields at the wind stair. No enemies.
- *Upper leg, W1 to X1:* the fields below the parapet on the east, the village's first houses above the face on the north.
  **P2s** (-18, 18, 106).
  - **E2.2s** a **fox pair** flanking P2s's gate, statues until the player passes through it. On a platform the double's echo and
    the race's count are learned together.

**Wind side (via E1).**
- *Lower leg, X0 to E1:* a band of bamboo on the downhill (east) side thrashing in every gust; the fields above. **P1w** (20, 10,
  73).
  - **E2.1w** a lone **crow-tengu runner** coming down the leg from the gate above P1w: run-through, turn, back up. On a stair it
    runs farther downhill than up: let it pass going down, strike it as it climbs. The first time the player meets a race on this
    side, the runner is seen **running inside the gust** up the leg from below and past them (its own rule, 1.6).
- *E1, the turn* (x 26 to 40, z 82 to 96, y 14): the great gate, a charcoal kiln and its woodpile at the east corner, a burner's
  hut (shut). **V4** looks west across the fields. No enemies.
- *Upper leg, E1 to X1:* **P2w** (18, 18, 106). **E2.2w** a **fox pair** at P2w's gate.

**The interior: the dry terraced fields** (x -22 to 22, z 62 to 112, y 7.5 to 20).
- Rice terraces stepping up in faces of 1.5 m toward the camera, their mud cracked in plates, dry straw stubble combed flat, two
  scarecrows, rice-straw bundles stacked at the terrace corners, a water wheel standing still on a dry channel. Deer graze here and
  bolt uphill on every count's first beat.
- *The old straight way* runs up the middle (x -2 to 2), a stone stair of the old pilgrims. From the fields it is walked down only:
  its lowest flight is fallen into a 2.5 m face at X0.
- Reached by the drops from P2s and P2w, and from the village above. No enemies: the interior is where the race is not.
- *Lore:* **L4**, a stone at the field stair's middle (0, 13, 88), worn. Words (Claude's, new): "The storm watered these fields.
  Then it began to race itself, and forgot to rain."

#### X1, the village crossing (crossing x -10 to 10, z 118 to 132, y 22; the village x -36 to 36, z 106 to 146, y 18 to 28)
- *Shape:* the crossing is the village's square. The four legs enter at its corners; in its middle, the **crossing shrine**: a
  small shrine on a stone base 1 m high, a small whole vermilion gate before it facing south, a stone lantern burning still. Round
  the square, farmhouses on terraces: thick thatch roofs (fading, nothing under them), shut; two struck by lightning and burned to
  their posts (roofless, walkable). Persimmons hung to dry under one eave, swinging. Hearth smoke streams downwind from one house
  whose hearth still burns.
- *Rest:* **F3** at (0, 22, 125), the shrine's lantern.
- *Enemies:* none in the square (a crossing is calm). The village's own small shrine on its upper terrace at (24, 26, 140) keeps a
  dormant fox pair: an optional fight for the offering box behind them (20 Shards).
- *Lore:* **L5**, in the headman's burned house at (-24, 22, 132): a family altar scorched to the shape of the beam that fell on it.
  Words (Claude's, new): "Every year we climbed the old way to thank the storm. The year it split, the stairs came up around the
  old way like two hands around a throat."
- *The cliff path* leaves west (3.1).
- *The wind drops here* (`LOOKS.md`): the grass stands up, the dust settles, the smoke rises straight for the only time.

#### Diamond 2 (X1 to X2; Kazehaya leads: the wind stair strikes on the beat, the scorched stair on the "and")

**Scorched side (via E2): the struck cedars.**
- *Lower leg, X1 to E2:* the cedar forest on both sides, each trunk scarred by raiju claws, glowing along the scar a beat before a
  raiju blinks there. **P3s** (18, 26, 144).
  - **E2.3s** the first **messenger pair**: a raiju on the scarred cedar east of the platform at (24, 31, 146) and a crow-tengu
    runner coming down the leg from above. When both are within 12 m, they race to the player: two paths on the floor, the shorter
    lands. Stand so their paths cross (the raiju pounces from the east, the runner comes down the leg from the north-east: stand
    south-west of the platform's middle, near the gate) and they crash.
- *E2, the turn* (x 26 to 40, z 154 to 168, y 30):
  - **E2.4s** a **fox pair** at the turn's great gate and a **raiju** on its upper beam. When the count comes, the real guard shuts
    the great sill; the raiju rides the bolt (on the "and" here). Shelter denied: break the guard before the near count, or dodge.
- *Upper leg, E2 to X2: the broken run.* Two of its six gates broken in a row (z 172 to 188): a 16 m stretch with no whole sill.
  **P4s** (18, 34, 178) is the run's middle. No enemies: a count met on the open stair, on the off-beat, before the temple.

**Wind side (via W2): the bamboo and the kilns.**
- *Lower leg, X1 to W2:* bamboo on both sides. **P3w** (-18, 26, 144).
  - **E2.3w** a **messenger pair**: a raiju perched in the bamboo tops west of the platform (the culms bend in the gusts: its
    pounce ring drifts 1 m downwind, south, while it is marked) and a runner from above.
- *W2, the turn* (x -40 to -26, z 154 to 168, y 30): the turn's landing opens west into the **kiln hollow** (a flank: three
  charcoal kilns, woodpiles, the burners' sheds; race-free).
  - **E2.4w** two **crow-tengu runners**, one coming down the upper leg, one up the lower leg: their runs cross on the turn's
    landing. Two runners of the same god do not race each other; they run their lanes, and two lanes crossing on one landing is a
    floor to read. A runner that runs into the other's lane collides with it (both down 1 s).
- *Upper leg, W2 to X2: the broken run* (the wind form): **P4w** (-18, 34, 178), two broken gates in a row. The gust shoves down
  the leg: caught twice in a row, the player loses 8 m of climb.

**The interior: the cedar avenue** (x -14 to 14, z 136 to 188, y 24 to 36).
- The old straight way climbs between two rows of great cedars (a nod to the cedar avenues of the mountain pilgrim paths, sources),
  their canopies fading overhead. At z 170 the **grandfather cedar**, struck a long time ago, lies across the avenue, its trunk 2.5
  m thick: from the north it is a drop (2.5 m, facing south), from the south a wall. **S2** (section 5).
- *Lore:* **L6**, a shrine rope round the fallen cedar's stump at (0, 36, 186). Words (Claude's, new): "The oldest tree on the
  mountain was struck on the day the storm split. It fell across the old way, and nobody has climbed it since."

#### X2, the temple crossing (crossing x -14 to 14, z 190 to 204, y 38)
- *Shape:* the crossing is the temple's forecourt. Its **gate** (sanmon), on the straight way at the crossing's north side (x -6 to
  6, z 204 to 210), was a two-storey gate; a strike burned its upper storey and roof away, and its posts, its two guardian niches
  (empty, the statues fallen and broken) and its great doors stand. The doors are barred from the inside: **S3** (section 5). The
  crossing shrine stands in the forecourt's west corner, small and whole, a Shinto shrine inside a Buddhist temple's gate, as
  mountain temples were.
- *Rest:* **F4** at (0, 38, 198).
- *Vista:* **V5** at the north edge.
- *Enemies:* none.

### 3.3 District 3: the slopes and the crags (diamonds 3 and 4, X2 to X4; z 196 to 340)

**The idea.** Above the temple the forest turns to maple and pine, the face steepens, and the stairs pass a waterfall ravine and a
graveyard of lanterns; above the pagoda they climb onto bare crags with wind-bent pines into the cloud's base, where wisps stream
across the floor and strikes come often (`LOOKS.md`). The count quickens twice and half the gates are broken. The messengers come
with the fox guards; the mixes are the district's peaks. **Levels:** as district 2; in the crags the uphill faces become cliffs
(3 m and more) and the downhill side a parapet over cloud.

```
 z     x: -64   -48  -32    -16     0     16    32    48    64
340   ..................... X4 GATE OF BOTH y74 F6 .........  finish bell (48,74,336)
322      pines,    P8w (-18,70,322)  [crag]    P8s (18,70,322)        |  MESSENGERS'
304      macaques W4 (-32,65)  [the chains, the straight way]  E4 (32,65) |  COURSE (trial)
288   rope bridge  P7w (-18,60,288)  PAGODA (0,58,284) P7s (18,60,288)  |
280   (racecourse)                                                 start bell (48,60,280)
268   ravine rim ......... X3 PAGODA CROSSING y56 F5 ..............
250   chain down   P6s (-18,52,250)  [temple hall, shut]  P6w (18,52,250)   LANTERN
238   HERMIT'S  W3 (-32,47) bridgehead     [bell tower]        E3 (32,47)  GRAVEYARD
232   CAVE (-64,52,238)  ~waterfall~                                  (x 42..72)
216   ~~ravine~~   P5s (-18,42,216)   TEMPLE COURT y42   P5w (18,42,216)
196   ......................... X2 TEMPLE y38 F4 (sanmon) .............
```

#### Diamond 3, the slopes (X2 to X3; Narukami leads; count 0.8 s / 0.4 s; races every 40 s; one gate in two whole)

**Scorched side (via W3): the ravine.**
- *Lower leg, X2 to W3:* maples on the downhill side shedding red leaves that stream downhill across the treads; the temple's upper
  terrace above the face. **P5s** (-18, 42, 216).
  - **E3.1s** two **fox pairs** on P5s's gate and the next gate up (8 m on): four masks, two real. On the near count the real
    guards take both sills; the doubles stand in the lane and burst. With one whole gate in two on this leg, two shut sills mean
    the nearest open shelter is 16 m away: the dodge, or a guard staggered first.
- *W3, the bridgehead* (x -40 to -26, z 226 to 240, y 47): the turn stands on the ravine's lip; west of it the ravine drops 20 m to
  white water, the **waterfall** falling into it from the west rim at y 70. A **rope bridge** led west from the landing's edge at
  (-40, 47, 233) to a ledge across the ravine; it hangs broken into the gorge. Spray drifts across the landing (the only wet floor
  above the shore: dark stone, no shine). **V6** (the hermit's cave). No enemies.
- *Upper leg, W3 to X3:* **P6s** (-18, 52, 250). **E3.2s** the full mix: a **messenger pair** (a raiju on the gate, a runner from
  above) and a **fox pair** at the next gate down, which the player has just passed through (they wake behind). The answers are
  the district's: bait the messengers across each other, use the race on the fox doubles, take or keep a sill.

**Wind side (via E3): the graveyard.**
- *Lower leg, X2 to E3:* the **lantern graveyard** lies along the downhill (east) side, its lanterns' tops level with the
  parapet. **P5w** (18, 42, 216).
  - **E3.1w** two **raiju** on the graveyard's lanterns east of the platform, (24, 43.6, 214) and (25, 43.8, 220), and a **runner**
    on the leg. The two raiju take turns (one perched while the other is down); with the runner they race. Lanterns are perches,
    not shelter.
- *E3, the turn* (x 26 to 40, z 226 to 240, y 47): the graveyard's gate east (race-free, below).
- *Upper leg, E3 to X3:* **P6w** (18, 52, 250). **E3.2w** a gauntlet: **two fox pairs** at successive gates up the flight above the
  platform, each pair waking as the player passes through its gate; and a race arriving as the second pair wakes (placed:
  `RivalsRace.Hold` until the second gate, 1.3 rule 6 kept).

**The interior: the temple's upper precinct** (x -16 to 16, z 210 to 262, y 42 to 52).
- The temple court (y 42, z 210 to 220) behind the burned gate: raked sand gone to dust, a stone basin, the **bell tower** (a frame
  with a great bronze bell, roof fading) at (10, 42, 216). The **main hall** (hondō) on a terrace above (x -14 to 14, z 222 to 238,
  y 44), shut, its doors sealed with paper, its roof fading. Behind it the temple stair (the straight way) climbs to X3's south lip.
- *The bell:* struck with a heavy, it tolls, and every race on the mountain within the next 60 s comes 2 s later (the count is
  stretched): a calm the player can buy before a hard leg. Once a rest.
- *Lore:* **L7**, a sutra stone by the main hall's steps at (-8, 44, 222). Words (Claude's, new): "The priests rang the bell for
  the storm's two halves to hear each other. They only heard the bell."
- No enemies.

**The east flank: the lantern graveyard** (x 42 to 72, z 212 to 262, y 44 to 54; race-free).
- Rows of stone lanterns (1.6 to 2.2 m), gravestones and **jizo** statues in red bibs (the vermilion scraps here are bibs, faded
  brown, not paint) on terraces, maples, a narrow maze of paths between them; leaves pinned against the windward side of every
  stone. A place to walk, and a place with an answer: the graves are the pilgrims of the old way.
- *Enemies:* two raiju among the lanterns (they blink lantern to lantern; with no stair, no race to ride).
- *Lore:* **L8**, the oldest grave at (66, 54, 258), two names on one stone. Words (Claude's, new): "The first priests of the
  storm were twins. They were buried as one. It was the custom then."
- *The trial's option (b)* would be here (section 4).

**The west flank: the ravine and the hermit's cave** (side path, below).

#### X3, the pagoda crossing (crossing x -14 to 14, z 262 to 276, y 56)
- *Shape:* the crossing sits at the foot of a crag; on the crag, north of it, the **five-storeyed pagoda** (x -6 to 6, z 278 to
  290, base y 58), bare unpainted wood with roofs all of one size (a nod to the mountain pagoda on Haguro, sources), never entered,
  fading as the player nears. The crossing shrine at its foot.
- *Rest:* **F5** at (0, 56, 272).
- *Vista:* **V7**, held shot 2: the braid seen from above.
- *Enemies:* none.
- *Paths off:* west along the ravine rim to the hermit's cave (side path); east along a ledge (x 14 to 46, z 272 to 280) to the
  trial's start bell.

#### Diamond 4, the crags (X3 to X4; Kazehaya leads; count 0.6 s / 0.3 s; races every 32 s; one gate in two whole)

The cloud's base comes down onto the stairs: wisps stream down across the treads, the light comes from all round, strikes fall
on the crags between races (not on the legs). Uphill faces are cliffs of 3 to 6 m; parapets stand over a drop into cloud
(`FallCatch` under it: thrown back to the last firm floor, 15 damage).

**Scorched side (via E4): the cliff ledge.**
- *Lower leg, X3 to E4:* for its middle 16 m the leg narrows to a **ledge** 4 m wide cut into the cliff (still on the racecourse:
  4 m is the narrowest the race sweeps). **P7s** (18, 60, 288) is the ledge's wide end.
  - **E3.3s** two **runners** down the ledge one after the other, and a **raiju** on the ledge's one whole gate. A runner on a
    ledge cannot be sidestepped widely: step into the uphill face's lantern alcove (2 by 1.5 m) and strike its back.
- *E4, the turn* (x 26 to 40, z 298 to 312, y 65): a spur of rock with macaques, who bolt on the first far beat; the path east to the
  trial's course. No enemies.
- *Upper leg, E4 to X4:* **P8s** (18, 70, 322). **E3.4s** the land's last stair test: a **fox pair** and a **messenger pair**
  together on the platform, at the crags' tempo (3.3 s from the first beat to the strike, on the "and" here).

**Wind side (via W4): the bridge and the pines.**
- *Lower leg, X3 to W4:* at its middle the leg crosses a cleft between two crags on a **rope bridge** 16 m long, 3 m wide
  (x -10 to -22, z 280 to 292), planks and rope rails; the race leaps the cleft, so the bridge is outside the lane (a calm island
  mid-leg: 1.3 rule 4). **P7w** (-18, 60, 288) is the bridge's far landing.
  - **E3.3w** a **messenger pair** waiting on P7w: the player arrives off the bridge into their race.
- *W4, the turn* (x -40 to -26, z 298 to 312, y 65): a great pine bent flat by the wind across the landing's west side, its trunk a
  1.2 m wall; macaques on the crags beyond. No enemies.
- *Upper leg, W4 to X4:* **P8w** (-18, 70, 322). **E3.4w** a **fox pair** and **two raiju** on the leg's two whole gates (the
  raiju ride nothing here: the wind stair is not Narukami's; they pounce in turn).

**The interior: the crag and the chains** (x -14 to 14, z 290 to 334).
- Above the pagoda a crag rises 10 m to the summit's forecourt. The old straight way climbs it as a **chain route** (kusari-ba, as
  holy mountains in Japan still climb their crags; from memory), iron chains bolted to the rock, rusted, their upper ends coiled at
  X4's south lip. From X4 they are kicked down (one press): a two-way climb (`Ladder`, 3 s) from X4 to the pagoda's terrace beside
  X3. Not a shortcut of the district (S3 is): a way back down to the trial and the cave.

#### Side path: the hermit's cave and the plain War Fan (the west flank of diamond 3)
- *Seen first:* from **V6** at W3's bridgehead: across the ravine, a cave mouth halfway up the far cliff beside the waterfall, at
  (-64, 52, 238), and in it a seated figure with an open fan on its knees, its floor the only ground on the mountain the wind has
  not combed (`LOOKS.md`). The bridge that led there hangs broken.
- *The way:* from X3, west along the **ravine rim** (x -14 to -60, z 266 to 272, y 56 to 60, a path between wind-bent pines, the
  ravine on its south); at the waterfall's head a **chain** down the cliff beside the fall (6 m, `Ladder`) to a **ledge behind the
  falling water** (x -66 to -58, z 240 to 262, y 52), walked south behind the curtain of the fall (the fall fades within 4 m
  south of the player: the camera sees through it as a moving veil) to the cave.
- *The cave* (x -70 to -58, z 232 to 240, y 52; open to the sky through a split in the rock, so no roof over its floor): a calm
  hollow, the uncombed dust lying smooth, the waterfall's noise suddenly far. The **hermit** sits on a stone facing the ravine, a
  man who sat down in meditation here and did not get up again, long ago, the fan across his knees (Decided, owner,
  2026-11-19, answer 7: the hermit is kept).
- *Enemies:* the ledge behind the fall has a **crow-tengu runner** (a ledge 3 m wide: its run-through cannot be sidestepped; the
  fall's curtain is cover for one stride at the ledge's middle, where the rock steps back 1.5 m). The cave itself is quiet.
- *The prize:* **the plain War Fan** (`WeaponPickup`), a plain iron-ribbed folding war fan (tessen) with a paper leaf gone brown,
  playing exactly as the plain War Fan.
- *Lore:* **L9**, carved on the cave wall. Words (Claude's, new): "I kept the old way. Up and down it every day, one straight line,
  the whole storm at my back." / "Then they braided it. I sat down to wait for them to finish."
- *Home:* the broken rope bridge's ropes lie coiled at the cave mouth; "Make fast the bridge" (one press, a progress key) hauls the
  bridge up and pegs it, and it spans the ravine to W3 (16 m, x -40 to -56, y 47 to 52: a slope of 18 degrees, walked only). A loop
  to the scorched stair's turn, F4 and F5 either way.
- *Minutes:* 4 for a new player.

### 3.4 District 4: the split summit (z 340 to 424, y 74 to 80)

**The idea.** One great hall in two mirrored halves (Decided): a walled compound on the summit, split down the middle by a
**seam** that runs north from the Gate of Both, through a long central court, to the screen room. West of it Narukami's castle,
east of it Kazehaya's monastery: the same footprint, the same heights, the same number of rooms and enemies, told apart by light
(stroboscopic against steady), floor (indigo stone against raked gravel) and shape (angles against curves) (`LOOKS.md`). The race
ends here: its last strike goes over the summit wall onto the roof. Inside, each half has its own beat: the castle's drums call the
bolts, the monastery's conches turn the wind and its bells ring the gusts. **Levels:** the courts (y 76), the halves' middle rooms
(y 78), the elites' rooms (y 80): each half climbs 2 m a room, north and inward.

```
 z     x: -64       -40   -24  -8   0   8   24    40        64
424                           [ SCREEN ROOM y78 F9*(0,78,412) ]
410    ARMOUR HALL y80 (-50..-24)        |     |    BELL COURT y80 (24..50)
404     8 suits, dais N   (Retainer)     |seam |   4 bell frames  (Hermit)
394     door E -> S4c ===> central court |  y76| <=== S4m <- door W
388   F7*(-46,78,388)  door N            |     |            door N  F8*(46,78,388)
376   [DRUM TOWER]  DRUM COURT y78        |     |   HALL OF FANS y78  (roofless)
372   (-64..-40) great drum, 2 stands    |     |   (40..64) fan screens, conch master
362              BAILEY y76 (-40..-8)  gate W | gate E   GRAVEL COURT y76 (8..40)
352                                      |     |
350 ========================== GATE OF BOTH (0,74->76,350) ==========================
340                          X4 forecourt y74, F6*(0,74,340)
```

#### 4A. X4, the Gate of Both (forecourt x -24 to 24, z 334 to 350, y 74)
- *Shape:* the last crossing: both stairs arrive at its south corners (the scorched from the south-east, the wind from the
  south-west); the chains' heads are at its south lip; the trial's finish ledge is above its east end (a 2 m drop into it). The
  north side is the summit's outer wall, sloped stone on the west half and plastered with a bark-shingle coping on the east, and in
  its middle the **Gate of Both**, two leaves, the west one charred, the east one scoured silver.
- *Rest:* **F6** at (0, 74, 340), the last crossing shrine.
- *Vista:* **V8**.
- *The race ends* over the wall: every race's last strike and gust go up the outer wall's face and over.

#### 4B. The central court (x -8 to 8, z 352 to 400, y 76)
- *Shape:* a long court on the seam, up a 2 m stair through the gate. Its west half is indigo stone with scorch figures, its east
  half raked gravel the wind keeps raking; the seam is a straight line of pale stone down its middle. The castle's gate opens in its
  west wall at (-8, 76, 362); the monastery's in its east wall at (8, 76, 362). At its north end a stair (2 m) to the **screen room's
  doors** (z 402), carrying two **seals**: a bronze drum disc on the left leaf, a knotted cord of a wind bag on the right. Each
  elite's fall breaks one, and both must fall before the doors open (Decided, owner, 2026-11-19, answer 4).
- *Vista:* **V9** at its north end.
- *Light:* the west half strobes, the east half glows steadily; standing on the seam, the player is lit by both.
- *Enemies:* none. A calm corridor between the two halves' beats.

#### 4C. The castle half (x -64 to -8, z 352 to 412)

**The bailey** (x -40 to -8, z 352 to 378, y 76).
- *Shape:* an outer court of indigo stone inside battered walls that slope out at the foot (the castle's own shape, `LOOKS.md`; the
  south wall's top fades where it stands within 4 m south of the player). A well, a rack of spears, scorch figures branching across
  the stones. Strikes fall here between enemies (a bolt every 12 s on a marked spot, a ring 1.5 s ahead: the castle half is "inside
  the cloud, striking").
- *Enemies:* **E4.1c** two **struck samurai** at (-28, 76, 368) and (-18, 76, 372), and a **drummer** on a portable drum stand at
  the bailey's west end (-38, 77, 366), 1 m up, in frame. The lesson: stand so the drummer's ring falls on a samurai; let it charge;
  step back for four seconds; punish the discharge. Kill the drummer last if the samurai are the problem, first if the bolts are.
- *Out:* a stair 2 m up at the bailey's west end into the drum court.

**The drum court** (x -64 to -40, z 352 to 390, y 78).
- *Shape:* the court at the **drum tower's** foot. The tower (x -64 to -54, z 368 to 382) is never entered; it fades. Before it, the
  **great drum** (o-daiko) on its stand at (-50, 78, 372), 2 m across, its skin scorched. On the court's east and west edges two
  **drum stands**, 1 m up, at (-60, 79, 360) and (-42, 79, 380).
- *Enemies:* **E4.2c**, the half's peak: two **drummers** on the stands, playing call and response (their bolts alternate every 1.25
  s), and three **struck samurai**. The court is 24 by 38 m, a group fight; the drums are the beat. The great drum is the player's:
  a heavy on it calls a bolt onto the nearest samurai. Slit the silent drums (a progress key each): **every slit drum slows the
  armour hall's strikes** (6 below).
- *Rest:* **F7** at (-46, 78, 388), a stone lantern at the armour hall's door, beyond the fight.
- *Lore:* **L10**, on the great drum's rim. Words (Claude's, new): "We beat the drums so the lightning would know where we were. It
  knew."

**The armour hall** (x -50 to -24, z 390 to 412, y 80): the Last Retainer's arena (section 6.1). Its back door in the east wall at
(-24, 80, 394) is **S4c**.

#### 4D. The monastery half (x 8 to 64, z 352 to 412)

**The gravel court** (x 8 to 40, z 352 to 378, y 76).
- *Shape:* an outer court of raked gravel inside low plastered walls with bark-shingle copings (fading); a stone lantern, a water
  basin, two pines bent east. The gravel's lines show the wind; between enemies, a gust crosses the court every 10 s (the bells on
  the hall's eaves ring a beat before). The monastery half is "inside the cloud, never striking".
- *Enemies:* **E4.1m** two **yamabushi**, at (20, 76, 366) and (30, 76, 372), and a **crow-tengu with a feather fan** perched on the
  court's east wall coping at (38, 77, 364), 1 m up, in frame. The lesson: the conch turns the court's wind; the fan-bearer rides it.
  Roll with the wind to close on a blower; cut the blast.
- *Out:* a stair 2 m up at the court's east end into the hall of fans.

**The hall of fans** (x 40 to 64, z 352 to 390, y 78; roofless).
- *Shape:* a long hall whose bark roof the wind tore away; its walls and a row of low folding screens (1.2 m) down its middle are
  pinned with folding fans opened in arcs, all flapping: the fans show the wind's direction at a glance. The screens are cover and
  perches.
- *Enemies:* **E4.2m**, the half's peak: two **yamabushi** (one a **conch master** standing on a dais at the hall's north end at
  (52, 78.6, 384), 0.6 m up, whose blasts last 12 s), two **crow-tengu fan-bearers** on the screens. The conch master's wind blows
  the length of the hall. Break the conches: **every conch broken (a heavy on a dead blower's conch, a progress key) gentles the bell
  court's gusts** (6 below).
- *Rest:* **F8** at (46, 78, 388), at the bell court's door.
- *Lore:* **L11**, painted on the largest fan at (63, 79, 370). Words (Claude's, new): "She carried every wind there was in one bag,
  and she let them out one at a time, so that each would be first."

**The court of wind bells** (x 24 to 50, z 390 to 412, y 80): the Hermit of the Pass's arena (section 6.2). Its back door in the
west wall at (24, 80, 394) is **S4m**.

---

## 4. The trial

**Decided**: each land's trial is different and invented for it; the owner's own example for this land is "a race"
(`WORLD.md`). **Decided** (owner, 2026-11-19, answer 5): the trial is the Messengers' Course; the course and its rules
below are **Proposal (Claude)**. The two options not taken are kept after it for the record.

### 4.1 The Messengers' Course (the east flank of diamond 4)

- *Seen first:* from X3, east along the ledge: at the foot of the east crag a **start bell** in a frame at (48, 60, 280), and a
  straight stair going up the crag's spine, gates on it alternating charred and silver, and at the top, against the cloud, a second
  bell. Two figures wait by the start bell, a raiju with two tails and a crow-tengu taller than the runners: the messengers'
  champions. Also reached from E4's turn by the spur path.
- *The course:* a straight stair 4.5 m wide from (48, 60, 282) to (48, 74, 334): 52 m, rising 14 m (15 degrees) in flights, with
  **seven gates**, alternating scorched and wind-split, every second one whole. Its downhill side (south and both edges) has a
  parapet; its top is a ledge with the **finish bell** at (48, 74, 336) and a drop of 2 m west into X4's forecourt.
- *The race:* "Ring the bell" starts it. The champions run the course with the player, and the course has its own sweeps, at the
  roof's tempo (2.75 s from the first beat), alternating a bolt count and a gust count, every 8 s. The champions race each other and
  the player: they attack the player and each other by the messengers' rule (paths shown, the shorter lands, crossing paths crash),
  each is immune to its own racer's sweep and struck by the other's. Whoever rings the finish bell first wins.
- *How the player wins:* by the verb. A player who only shelters loses: the champions do not stop. Every perfect dodge, through
  either sweep, gives Keeping Pace (30% faster for 4 s, rolls 1 m longer) and the perfect dodge's 12 mana; chain them and the player
  can outrun both. Or fight: shove a champion into the other racer's lane on the beat, bait them across each other, and climb while they
  lie staggered.
- *Lose:* a champion rings the bell; both bow mockingly; the course resets (the player walks down or drops into X4).
- *The prize:* a **Memory** (or a Body piece, `WORLD.md`) hung on the finish bell's frame. Won once; then the champions are gone and
  the course is a quiet stair down to the crags.
- *Why this one:* the owner named a race; it is the land's verb at its fastest, with no fire nearby to lean on; it previews the gods'
  race to the player from the other side (the player is a racer); and no other land could hold it. *Hi-Fi Rush* makes the beat the way
  through its levels: a player who understands the beat already knows when to move (sources).
- *Minutes:* 3 to 5.

### 4.2 Not taken: option (b), the Thunder Hunt (the lantern graveyard)
- A raiju of the old storm, larger and pale, loose in the lantern graveyard, blinking lantern to lantern on a steady beat. The
  villagers once went thunder-hunting to where lightning had struck (*kaminari-gari*, sources). The player has 3 minutes to catch it:
  it can be struck only on the ground, and it comes down only onto a lantern the player is standing beside, on the beat after the
  lantern glows; read the order (it follows the rows, one lantern a beat) and be under the next one. Two graveyard raiju harry.
- Strong and readable; it is a chase, not a race, and its rhythm is the raiju's alone, not the race's.

### 4.3 Not taken: option (c), the Waterfall Austerity (the ravine's foot)
- A yamabushi master stands under the waterfall in the ravine's pool (*takigyō*, the ascetics' practice), a lone mini-elite on a
  floor of wet rock. Standing in the fall halves anyone's stamina regeneration and doubles their poise; he fights from in it and
  lures the player in. A good duel; but Water's trial is already a duel, and it does not use the race.

---

## 5. Shortcuts and loops

| | Opens from | To | How | Saves |
|---|---|---|---|---|
| S1 | The haiden's north half of the central corridor, (0, 1.5, 32), reached from X0 by the haiden's side stairs | The stage, then F1 30 m on | **The beam**: a fallen roof beam lies along the corridor's north half; "Push the beam across" from the north bridges the 6 m gap. Walked both ways. It is on the racecourse: the corridor's two small gates (z 22 and z 36) are its shelters | F1 to X0 without the corridor, the town and its crabs: 40 m instead of 220 |
| S2 | The cedar avenue's north end, (0, 36, 186), from X2 | X1, F3 (0, 22, 125) | **The grandfather cedar**: from the north, the fallen cedar is a drop of 2.5 m (facing south); a vine rope hangs from its top, cut loose from above (one press) it becomes a climb (`Ladder`, 2 s). Then the avenue (walked both ways) | X2 to X1 without either side of diamond 2: the way back to the village, the cliff path and the other side |
| S3 | The temple court, inside the burned gate, (0, 38, 205) | X2, F4 | **The temple doors**: barred on the inside; "Lift the bar" from the temple court (reached from X3 down the temple stair, or by the drops from P5s and P5w into the interior) | X3 to X2 straight down the temple stair: the way back to the graveyard and the ravine |
| S4c | The armour hall's east door, (-24, 80, 394) | The central court (-8, 76, 394); F6 60 m on | **The war door**: barred on the hall side; opened after the Retainer falls (the hall is held while he stands) | Out of the castle half without walking back through it |
| S4m | The bell court's west door, (24, 80, 394) | The central court (8, 76, 394) | The same, after the Hermit falls | Out of the monastery half |
| Bridge | The hermit's cave | W3's bridgehead | "Make fast the bridge" from the cave side | The cave as a loop to the scorched stair |
| Chains | X4's south lip | The pagoda's terrace beside X3 | Kicked down from above; climbed both ways (3 s) | X4 to X3 and the trial and cave without either side of diamond 4 |
| Drops | Every upper leg's platform | The diamond's interior below, 2 to 2.5 m | One way, facing south | Into the interiors from above |
| Loop | Both sides of every diamond | | Walking | Every diamond is a loop: up one side, down the other |

S1 to S4 and the others stay open after a fall and across sittings (progress keys, as Fire's). None of the old straight way's
stretches can be climbed except by the opened ropes and chains: the old way goes down, the race goes up.

---

## 6. The elites' arenas

**Decided** (`WORLD.md`): one elite in each half, each the champion of its god's speed, taken in either order; the Last Retainer
(castle half, Katana; the last of Narukami's samurai, the fastest draw in the land; teaches Before the Thunder) and the Hermit of
the Pass (monastery half, War Fan; a tengu master of the fastest step who turns blows aside with a fan; teaches Turn the Spear).
**Decided** (`LOOKS.md`): the Retainer waits in the armour hall, the one figure among the empty suits that moves; the Hermit waits in
the court of bells. **Draft** (`steel/weapons.md`): Before the Thunder is a draw that breaks an attack in its wind-up; the teacher
stands sheathed and never moves first, cutting the player's heavies, charges and skills before they land. Turn the Spear is a step
along a thrust's lane to strike its bearer; the teacher does it to every lane the player throws. Their numbers belong in
`lw_land.md`.

### Taken in either order (how)
- **Equal halves.** Each half is three rooms climbing 2 m a room, one fire at the elite's door, one back door out to the central
  court, the same count of enemies (three in the outer court, five in the middle room) and the same walk (about 80 m). Neither
  elite is harder for being first or second; neither half changes when the other is cleared. Dark Souls' four Lordvessel bosses
  and Mega Man's eight stages are the models for a free order that stays fair (from memory).
- **Each half can be prepared.** Slitting the drum court's drums slows the armour hall's strikes; breaking the hall of fans'
  conches gentles the bell court's gusts. Both are kept after a fall (progress keys). A player who fought the middle room
  carefully meets an easier arena; one who ran through does not.
- **Each technique answers the other elite.** The Retainer's great draw is a lane (Turn the Spear steps along it); the Hermit's
  pass begins with a wind-up (Before the Thunder breaks it). A player carrying the Katana who beats the Retainer first, or one
  carrying the War Fan who beats the Hermit first, has a new answer for the second. Both plain weapons are found in this land, so the
  answer is there for whoever looks.
- **Two seals** on the screen room's doors; both elites must fall (Decided, owner, 2026-11-19, answer 4).

### 6.1 The armour hall (the Last Retainer)

**Shape.** A hall 26 by 22 m, x -50 to -24, z 390 to 412, floor y 80, its roof burned off by strikes long ago: charred posts
stand along its walls to 4 m and nothing spans the floor. The floor is black boards branched all over with lightning's fern
figures. **Eight suits** of black lacquered armour on stands (1.8 m, 0.8 m square bases) in two rows, at x -45, -40, -34 and -29 on
z 397 and z 405. On the north, the **lord's dais** (x -42 to -32, z 408 to 412, 0.6 m) with an empty seat and a sword rack. The door
from F7 at (-37, 80, 390); the war door (S4c) at (-24, 80, 394).

```
 z    x: -50       -40       -32       -24
412     [   dais, empty seat   ]
405      A        A        A        A
400                 (R)
397      A        A        A        A      war door (S4c) at z394
390   ........... door from F7 ............
```
`A` a suit on its stand; `(R)` where he stands, the third suit of the first row that is not empty.

**The light.** Stroboscopic, as the castle half: dark, then a hard cyan-violet flash. In the dark he is seen by his cyan rim and his
contact shadow; between flashes nobody can see his hands. The strikes are the hall's beat.

**How it plays with his moves.**
- *He never moves first* (the draft). Within 5 m, a heavy, a charged blow or a weapon skill begun by the player is cut before it
  lands (his draw, quicker than anything). Light blows, guards and dodges are safe to start. The fight's first lesson is patience,
  as Ghost of Tsushima's standoffs are won by waiting for the other's move (from memory).
- *He moves when the player waits.* Three seconds of no attack and he walks in, a step on each flash.
- *The great draw* (red): on the hall's beat (every third strike) he crouches sheathed for 0.8 s, then draws along a lane 7 m long and
  1 m wide, crossing the room in a cut. The answers: dodge through it on the beat, perfect-guard it, step along it (Turn the Spear, if
  the player has it), or strike him in the crouch with a light blow (the move he teaches, done to him: Before the Thunder breaks it).
- *The suits stop the draw.* A suit in the lane is cut in half and falls, and the lane ends there. Each draw that hits a suit clears
  the hall a little: the arena opens as the fight goes on, as the champion's bowl fills with gold in Fire's.
- *The strikes.* Every 6 s (8 s with one drum slit, 10 s with both) a bolt strikes the suit nearest the player that still stands. The
  struck suit **crackles for 3 s**: an arc 1.5 m round it bites anyone in it (8). If he draws within 3 m of a crackling suit, his blade
  carries the bolt: a fork jumps 3 m off the end of the cut. Fight him away from struck suits; let him cut down the suits near you.
- *His ground:* `HoldGround(-50, -24, 390, 412)`. Beaten: a Memory, Before the Thunder, the drum seal on the screen room's doors
  breaks with a single deep boom heard across the summit; the war door opens.

### 6.2 The court of wind bells (the Hermit of the Pass)

**Shape.** An open court 26 by 22 m, x 24 to 50, z 390 to 412, floor y 80, raked gravel combed into long lines. **Four bell frames**
(torii-like frames 2.4 m high, 3 m wide, hung with dark iron wind bells and paper strips) at (30, 396), (44, 396), (30, 405) and
(44, 405), their crossbars facing north. A **meditation stone** at the centre (37, 80, 400), 1.2 m high, 2 m across. The door from
F8 at (37, 80, 390); the war door (S4m) at (24, 80, 394). The court's north wall is low (1.2 m): beyond it, cloud.

```
 z    x: 24        30        37        44        50
412   ...... low north wall, cloud beyond .......
405           [bells]             [bells]
400                     (stone)
396           [bells]   (H)       [bells]
390   ........... door from F8 ...........   war door (S4m) at z394, west wall
```

**The light.** Steady and even, pale violet-grey from every side, no flashes. Everything is seen; the wind is the trouble.

**How it plays with his moves.**
- *The gusts.* Every 7 s (8.5 s with one conch broken, 10 s with both, and a third weaker) a gust crosses the court from the north,
  downhill: the north frames' bells ring 1 s before it, then the south frames' a half-second later, a running beat the player hears
  coming down the court. In a gust (1.5 s) everyone drifts south, rolls south go 6.5 m and north 2.5 m, and the gravel combs itself
  south. The frames and the stone are lees (the strip behind each, 2 m deep, is still).
- *He steps along every lane* (the draft). A thrust, a draw, a knife's crossing, any lane the player throws: he steps along it at the
  instant it would land, the fan turning the point aside, and strikes the player's body. Sweeps and arcs are not lanes: they are how
  he is hit. The first lesson is what the player's weapon throws.
- *The pass* (red): he shuts the fan and steps through the player along a long lane (8 m), on a gust's beat, riding it: downwind it is
  10 m. Its wind-up is a half-second with the fan shut at his shoulder: Before the Thunder breaks it (if the player has it); otherwise
  dodge on the beat, or stand in a frame's lee where the gust does not carry him.
- *The turn of the fan.* His guard is a fan: blows from the front turn aside (a perfect guard of his own) unless they come during a
  gust from upwind of him (he is braced against the wind and cannot turn).
- *The thrown fan.* From range he throws the fan in an arc that returns (a curved lane, guardable), which the bell frames stop.
- *His ground:* `HoldGround(24, 50, 390, 412)`. Beaten: a Memory, Turn the Spear, the cord seal on the screen room's doors comes
  unknotted with a long breath of wind across the summit; the war door opens.

---

## 7. The screen room, the roof, the heavens and the ride

The fight's design belongs to `lw_land.md`; this section gives the rooms, as `ice_levels.md` section 6 does for the lake.

### 7.1 The screen room (x -12 to 12, z 402 to 422, y 78)

**Decided**: the two first gods painted on the screens as one storm, the last quiet room before the roof, where the story fold
lands; the screens as a low ring of folding screens opened in a zigzag round the room's centre, angled so their panels face up
toward the camera, the polished dark floor reflecting their gold; the dragon and the tiger on the two end panels; one lamp; the
doors shut, nothing moving but the flame (`WORLD.md`; `LOOKS.md`).
- *Shape:* a room 24 by 20 m, its walls dark wood with high paper windows the storm glows through, its roof never drawn while the
  player is inside (Decided, owner, 2026-11-19, answer 6). The ring of screens, twelve panels in a zigzag (the stairs' braid again) round a clear centre 8 m
  across; a gap at the south and north for the doors' line.
- *The screens* (the composition is the famous screens' nod, not their painting: the thunder and the wind across a gold field,
  sources): on the west panels the thunder in cyan-violet ink with his drums, on the east the wind in the pale of dust with his bag,
  facing each other across old dull gold. **Proposal**: along the panels' lower edge, under both of them, a painted sea whose waves
  stand still mid-break, exactly as the hub's sea stands (`WORLD.md`: the clue that the sponsor is Time). And on the two north panels,
  the last of the twelve, the one storm torn in two, the two halves turned to face each other: the copies who race.
- *Rest:* **F9** at (0, 78, 412), the room's one lamp.
- *Lore:* **L12**, on the screen frame's foot at the north gap. Words (Claude's, new, the land's last): "They were one storm. Someone
  stopped it in the middle, the way you stop a wave." / "What was left could not stop. It could only race."
- *Out:* the north doors at (0, 78, 422) and a stair up; `SceneDoor` to `StormRoof`.

### 7.2 The roof approach (`StormRoof`, local coordinates)

Walked once; after a fall the player wakes at F9 and climbs again (about 40 s).
```
 z
 56   ===== RIDGE WALK y8, x -7..7 (finials at both ends) =====   the gods
  0   ---- stair 4 m ----
 -8   TIER 2 y4, x -10..10, z -16..-8
-16   ---- stair 4 m ----
-22   TIER 1 y0, x -12..12, z -30..-22, arrival at (0,0,-30)
```
- *Shape:* the hall's stacked roofs as floors (roofs are floors in this land, `LOOKS.md`): the lower eave's flat walk (tier 1), the
  middle roof's walk (tier 2), the ridge walk (phase 1), each joined by an outside stair 4 m high (27 degrees) on the south face,
  tile on the west half, bark shingle on the east, the seam a line of pale capstones.
- *The race at its fastest:* every 15 s, 2.75 s from the first beat: the gods race round the roofs, the bolt and the gust sweeping the
  stairs (no gates here: the stairs' landings are 1 m deep, and the shelter is the tiers' parapet niches, 2 by 1.5 m, two on each
  tier). The perfect dodge is the way up, and its last practice before they race for the player himself.
- *No enemies.* Wind at its strongest; arcs between the finials; the cloud right overhead.
- *Vista:* **V10** at the ridge walk's south end: the walk, and the two of them.

### 7.3 The roof, phase 1 (`StormRoof`, x -11 to 11, z 0 to 56, y 8)
- **Decided** (`LOOKS.md`): a broad flat walk along the ridge of the split hall, the two slopes falling away into cloud, grey tile on
  the castle side and bark shingle on the monastery side, the seam along the walk, the hardest strikes, arcs between the finials, the
  strongest wind, the walk's floor plain stone flags in large slabs so both race paths read.
- *Proposal:* the walk is 14 m wide (x -7 to 7) and 56 m long; beyond each edge a strip of roof 4 m wide at 10 degrees is still
  walkable (x -11 to -7 tile, 7 to 11 bark), and beyond it 35 degrees and cloud (`FallCatch`: thrown back to the walk, 15 damage).
  Four **finials** (the dragon at the west, the tiger at the east) on posts at (-9, 8, 14), (9, 8, 14), (-9, 8, 42), (9, 8, 42): arcs
  jump between each pair across the walk on the roof's beat, two lines on the floor the gods' paths can be baited across.
- *For the race to the player* (Decided: every attack a race to reach the player; both paths shown; the shorter lands; crossing paths
  crash): the walk is long north-south and narrow east-west, so a player who keeps one god north and one south makes their paths
  long and parallel (no crash, the nearer one lands); a player who keeps them east and west makes their paths cross over the seam.
  The arena teaches the bait by its shape.

### 7.4 The break and the heavens (`StormRoof`, above the cloud)
- **Decided**: when one is beaten down it becomes its beast and the other follows; the summit's top breaks away between the two
  transformations; the walk tears off with the player on it and rises through the cloud; the player walks on clouds; the dragon coils
  through the cloud layers and strikes through holes it bursts in the floor; the tiger bounds from cloud to cloud and her roar shoves
  whole clouds sideways; they still race and can be baited into each other. The owner wants this phase taken much deeper, designed
  when the fight is made.
- *The room, Proposal:* the torn-off ridge walk becomes the central island (14 by 40 m of stone flags with cloud round its edges),
  and round it a ring of eight cloud islands 12 to 20 m across, the gaps between them 3 to 6 m, all at one height (no island over
  another: a cloud over a walkable cloud would hide the player, the rule this land keeps everywhere). The tiger's roar moves an island
  6 m sideways in 1 s (closing or opening a gap); the dragon's burst opens a hole 4 m across in an island for 6 s. A fall through a gap
  or hole throws the player back to the last firm cloud (15 damage). Pieces of the roof drift between islands, too small to stand on
  (`LOOKS.md`).
- *His and her ground:* `HoldGround` over the ring.

### 7.5 The storm cloud (the ride out)
- **Decided**: a storm cloud, a mix of both; lightning carried inside wind-driven cloud; the first rain falls behind it on the
  mountain; a grey dawn ahead (`WORLD.md`; `LOOKS.md`).
- *Proposal:* the choice is made over each body separately (Decided). Where the two beasts went down together, on the central island,
  the dragon's cloud and the tiger's wind do not part: a small dense cloud gathers at the island's south edge with lightning running
  inside it and sparks streaming along a gust. "Step onto the storm." **Held shot 3**: the cloud carries the player south down the
  mountain's face in one long shot, over the summit, the pagoda, the temple, the village and the shrine, over the braid of the two
  stairs with no race on them for the first time, the gates standing still and the grass upright; out over the great torii and the
  sea. Behind, the storm over the mountain breaks into rain. Ahead, a grey dawn.

---

## 8. Build list

New pieces are made in code by a `RivalsMountainBuilder` (menu `Clover/Build/Rivals Mountain`) and a `StormRoofBuilder`, as the Fire
and Ice builders are: static meshes with `MeshCollider`s, ink outlines where the toon material carries one. Each piece names its
parameters; sizes are the defaults the plan uses.

### 8.1 New geometry

| | Piece | What it is | Dimensions and parameters |
|---|---|---|---|
| M1 | **Racecourse leg** | A diagonal stair from one landing to another: flights and landings, the uphill face (2 to 6 m, ashlar or cliff), the downhill parapet (0.6 m, with one gap at the platform), the lane laid in the treads (scorch figure or combed strip), gate sockets every 8 m, lantern alcoves | `Leg(from, to, width 5, kind scorched/wind, wholeEvery 3 or 2, platformAt 0.5)`. Sixteen legs |
| M2 | **Gates** | Torii in five forms: *scorched* whole and broken, *wind-split* whole and broken (beam lying beside), *crossing* (small, whole, fresh vermilion), *great* (a turn's, two beams, 3 m sill), *the great torii* (16 m, in the surf, with auxiliary legs). Each on a pale sill; beams fade within 4 m south of the player and while under them; the dragon or the tiger carved on the posts | `Gate(kind, span 4.4, beam 3.8, sill 1.6)`; sills registered as shelter by M24 |
| M3 | **Platform** | A 10 by 10 m landing on a leg with its whole gate and the parapet gap's drop | Section 3, P1s to P8w |
| M4 | **Turn** | A 14 by 14 m landing with a great gate and a door to its flank | W1 to W4, E1 to E4 |
| M5 | **Crossing precinct** | A level 20 by 14 m precinct where four legs and the straight way meet, a shrine on a 1 m base, a crossing gate, a stone lantern rest point; a calm zone (C17) | X0 to X4 |
| M6 | **Terraces** | Fire's `Plateau` with 1.5 m faces turned south: paddy mud cracked in plates, stubble, maple litter, silver grass, by band | Every interior and flank |
| M7 | **Shrine on piles** | The stage, the central corridor (with its gap and the pushable beam), the east corridor (posts every 2.4 m, no roof), the haiden (roofless), the honden (shut) | Section 3.1 |
| M8 | **Surf and wash** | Knee-deep surf (slow, ghost water), deep water (throw-back), the wash every 7 s with its wet line, the slipway, the boat basin | Section 3.1 |
| M9 | **Drowned town** | Houses tilted into the sea (roofs in the water), one-storey shops with fading roofs, the sea wall, the storehouse yard | Section 3.1 |
| M10 | **Village** | Farmhouses with thick thatch (fading, shut), two burned to their posts (walkable), the small village shrine, drying persimmons, a hearth that smokes | X1 |
| M11 | **Trees** | Cedar (with raiju scars that glow, C7), maple (shedding streaming leaves), pine (wind-bent, a flat-bent variant as a 1.2 m wall), bamboo bands that thrash; canopies fade within 6 m | By band (1.4) |
| M12 | **Temple** | The burned gate (posts, niches, barred doors: S3), the court, the bell tower (the bell, C16), the main hall (shut, fading) | Section 3.3 |
| M13 | **Lantern graveyard** | Stone lanterns 1.6 to 2.2 m (raiju perches), gravestones, jizo rows, terraces, maples | East flank of diamond 3 |
| M14 | **Ravine, waterfall, bridges, chains** | The ravine (white water, not walkable), the fall (a veil that fades within 4 m south), the ledge behind it, the broken rope bridge and its repaired form, the racecourse rope bridge in diamond 4, chains (`Ladder`) | Sections 3.3 |
| M15 | **Pagoda** | Five storeys, bare wood, equal roofs; never entered; fades | X3 |
| M16 | **Crags** | Cliff faces 3 to 6 m, the ledge leg (4 m), rock spurs, parapets over cloud with `FallCatch` | Diamond 4 |
| M17 | **Summit** | The outer wall (sloped stone west, plaster and bark coping east), the Gate of Both, the central court with its seam, the bailey, the drum tower (fades) and court, the roofless armour hall with its suits (breakable: cut in half by his draw), the gravel court, the roofless hall of fans with fan screens, the bell court with its four frames and stone, the war doors | Section 3.4, 6 |
| M18 | **Screen room** | Walls, high paper windows glowing, a roof never drawn while the player is inside, twelve screen panels in a zigzag angled up, a polished floor reflecting them, one lamp | Section 7.1 |
| M19 | **StormRoof** | Roof tiers as floors, outside stairs, the ridge walk with flags, tile and bark strips, finials; the cloud islands (cel lobes with inked rims, mid violet-grey, never white), movable and holed | Section 7 |
| M20 | **The wreck** | A warship on its side, its deck tipped 18 degrees as a floor, the split deckhouse, the seated empty armour | Section 3.1 side path |
| M21 | **The hermit's cave** | A cave open to the sky through a split, its uncombed floor, the seated hermit | Section 3.3 side path |
| M22 | **The Messengers' Course** | A straight stair with seven alternating gates, a start bell and a finish bell in frames | Section 4 |
| M23 | **Vista zones** | `LandZones` with a direction, opened only between races (1.3 rule 7) | V1 to V10 |
| M24 | **Shelter list** | One list per scene of shelter rectangles, each from the piece that makes it (gate sills, alcoves, crossing precincts, the roof's niches), so the race's test and the drawn sills cannot disagree (as Fire's B12) | Every leg |

### 8.2 New behaviour the rooms rely on (for the code worker)

| | What | Where it is used |
|---|---|---|
| C1 | **The race** (`RivalsRace`): a clock per district (interval, tempos, lead per diamond); the count sounded at the gates (positional, from below), the lane's pulse and the gates' answer on the near beats; the two sweeps along each stair's legs (the lane's polygon per leg, excluding crossings, bridges, ladders and chains); the shelter test against M24; damage, stun, the gust's shove along the leg; enemies struck; the first races placed; `Hold` until a trigger; restart at fires; the near-count rule for enemies (1.3 rule 1) | Every leg; the shore's corridor; the roof approach; the trial's course (its own clock) |
| C2 | **Through the race**: the 0.2 s perfect window against the front's passing; one shared gift for both racers, the ordinary perfect dodge (12 mana, slowed time) and Keeping Pace, with no per-racer gift (Decided, owner, 2026-11-19, answer 1); the frame's glimpse; the once-only status line. Builds on `PlayerDefence`'s perfect dodge | Everywhere C1 runs |
| C3 | **The wash**: the 7 s wave, its wet line, the 20% slow; the crabs' clamp and scuttle keyed to it | The waterfront, the sandbar |
| C4 | **Heike crabs**: sideways lines and turning, the pinch, clamp and flip, the general's sword lane, the slow count away from the wash | The shore |
| C5 | **Ship-ghosts**: water-bound, the scoop lane, soak levels and sunk, drain on dry ground and at fires, the bottomless ladle and peace | The shore |
| C6 | **Fox guards**: the pair, the double (no shadow, blue eyes, bursts and reforms), the four-count with the echo, waking through the gate, taking and shutting a sill | Stairs, shore's east gate, the village shrine |
| C7 | **Raiju**: perches on high points (gate beams, lanterns, scarred cedars, bamboo tops), the glow before a blink, the pounce ring, the rake's scorch, riding the bolt | Stairs, graveyard |
| C8 | **Crow-tengu runners**: the run-through lane, the turn, farther downhill, running inside the gust | Stairs, the cave ledge |
| C9 | **The messengers' race**: paths shown, the shorter lands, the loser skids open, crossing paths crash; also the trial's champions | Stairs, the trial |
| C10 | **Struck samurai**: armour, charge by a bolt, the arc between the charged, discharge | Castle half |
| C11 | **Drummers**: the phrase, ring and bolt, call and response, silencing, slit drums kept, the great drum | Castle half |
| C12 | **Yamabushi**: the conch's wind (direction, drift, rolls, bending, the gravel and fans showing it), bracing, the cut blast; broken conches kept | Monastery half |
| C13 | **Crow-tengu fan-bearers**: perches, the swoop, the crescent, double drift and shove | Monastery half |
| C14 | **The armour hall**: strikes on the nearest standing suit at a rate set by slit drums, crackling suits, suits cut by the draw | Section 6.1 |
| C15 | **The bell court**: the gust cycle with the bells' running beat, set by broken conches; the lees of frames and stone | Section 6.2 |
| C16 | **Bells and seals**: the temple bell's stretched races; the two seals on the screen room's doors broken by the elites | X2's interior; section 4C |
| C17 | **Calm zones**: crossings and fires drop the wind (grass upright, dust settled, the flame still), excluded from the race | Crossings, fires |
| C18 | **Life as tells**: crows lift, deer and macaques bolt on the first far beat; bamboo thrash with the lead; scar glow before a blink | 1.4 |

### 8.3 What is reused

| Piece or code | Where |
|---|---|
| `FallCatch` | The sea, the ravine, the crags' cloud, the roof's edges, the heavens' gaps |
| `Ladder` (Fire's B6) | Chains, the cedar's rope |
| `LandZones` (vistas, with Water's direction), `TopDownCamera.SetFraming` | V1 to V10; the held shots |
| `Fadeable`, `OcclusionFader` | Gate beams, roofs, canopies, the pagoda, the waterfall's veil, the summit's south wall |
| `RestPoint` | F1 to F9 as stone lanterns |
| `WeaponPickup` | The plain Katana, the plain War Fan |
| `LandVoice` progress keys | S1 to S4, the bridge, the chains, slit drums, broken conches, the seals, the trial's prize |
| `Enemy.MakeDormant`, `Enemy.HoldGround` | Fox statues, the clamped general, the Retainer among the suits; both elites' rooms |
| `DistrictMood` | The light by area (`LOOKS.md` 8): far and dim on the shore, the cloud's underside on the stairs, strobing west and steady east on the summit, warm in the screen room, from below on the clouds |
| `SceneDoor` | The screen room to `StormRoof` |
| `DodgeRoll`, `PlayerDefence` (the perfect dodge event) | C2 |
| Fire's `Plateau`, `Bank`, `Ramp`, `Stairs`; the `SunCityBuilder` helpers | Every terrace, face, flight and ramp |

### 8.4 New kit (Blender, through the `blender-prop` skill)

Torii in all five forms with the carved emblems; the great torii; stone lanterns (standing, toppled, the rest-point form); the stage,
corridors on piles, haiden and honden; tilted drowned houses, shop rows, the sea wall, casks and the storehouse; fishing nets and
boats; farmhouses with thatch, burned frames, the village shrine; scarecrows, rice-straw bundles, the water wheel; charcoal kilns;
cedars (scarred), maples, pines (bent, flat-bent), bamboo bands; the burned temple gate, the bell tower and bell, the main hall;
gravestones and jizo; the rope bridges and chains; the pagoda; the wreck and the seated armour; the seated hermit; the summit's walls,
gate, drum tower, great drum and drum stands, armour suits and stands, the dais, fan screens and wall fans, bell frames and the
meditation stone; the screen panels; roof tiers and finials; cloud islands. The people: Heike crabs (three sizes), ship-ghosts with
ladles, fox guards (real and double share a mesh), raiju (and the two-tailed champion), crow-tengu runners and fan-bearers (and the
champion), struck samurai, drummers, yamabushi with conch and staff. The plain uchigatana and the plain tessen.

---

## 9. Pacing

Minutes for a new player on a clean pass, counted as `fire_land.md` 3.1 counts them (a weak enemy about 30 s, a strong one 45 s, a
group a little less each), with each race met counted in, on one side of each diamond.

| Stretch | Minutes (new) | Experienced | Tension | What it teaches |
|---|---|---|---|---|
| The great torii, F1, the first race | 1.0 | 0.5 | Wonder | The race begins here; a gate shelters |
| The spit, the stage, the east corridor | 1.5 | 0.6 | Rising | The soak; knock the ladle's bottom out |
| The waterfront | 2.0 | 0.8 | Rising | The wash's count; crabs move sideways; flip on the break |
| The boat basin and the slipway | 1.5 | 0.7 | Peak | Crabs and ghosts together; get out of the water's reach |
| The storehouse yard, the general | 1.0 | 0.5 | Peak (small) | Flip it three times |
| The Upper Street, the fox pair, F2 | 1.0 | 0.4 | Rising, rest | The double has no shadow; the race shows the real one |
| **District 1** | **8.0** | **3.5** | | |
| Diamond 1, either side (first race, a raiju or a runner, a fox pair) | 3.3 | 1.4 | Rising | **The count, the sill, the dodge on the beat**; perches; the run-through; the runner inside the gust |
| The village, F3 | 0.5 | 0.2 | Rest | |
| Diamond 2, either side (a messenger pair, the turn's mix, the broken run) | 4.0 | 1.8 | Peak | **The messengers race to you**; a shut sill; the off-beat |
| The temple crossing, F4 | 0.3 | 0.1 | Rest | |
| **District 2** | **8.1** | **3.5** | | |
| Diamond 3, either side (two fox pairs or the graveyard raiju, the full mix) | 3.8 | 1.6 | Peak | The quicker count; one gate in two |
| The pagoda, F5, the braid | 0.6 | 0.3 | Wonder | The land seen whole |
| Diamond 4, either side (the ledge or the bridge, the last test) | 3.8 | 1.6 | Peak | The crags' count: dodging is no longer a choice |
| The Gate of Both, F6 | 0.3 | 0.1 | Rest | |
| **District 3** | **8.5** | **3.6** | | |
| The central court | 0.5 | 0.2 | Dread | Two beats meet |
| The castle half: the bailey, the drum court, F7 | 4.0 | 1.6 | Peak | Bring the bolt to the samurai; drums are the beat |
| **The Last Retainer** | **4.0** | **2.0** | Peak | Never start a heavy near him; break a wind-up |
| Out by the war door | 0.3 | 0.2 | | |
| The monastery half: the gravel court, the hall of fans, F8 | 4.0 | 1.6 | Peak | The conch turns the wind; roll with it |
| **The Hermit of the Pass** | **4.0** | **2.0** | Peak | Do not throw lanes at him; break the pass |
| Out by the war door, the screen room, F9 | 1.2 | 0.5 | Quiet | The story fold |
| **District 4** | **18.0** | **8.1** | | |
| The roof approach | 1.0 | 0.5 | Dread | The race at its fastest |
| Narukami and Kazehaya (the roof, the break, the heavens) | 16 | 10 | Climax | (`lw_land.md`, later) |
| The ride | 0.5 | 0.5 | Release | |
| **Clean pass** | **60.1** | **29.7** | | |
| Side paths: the wreck 4, the hermit's cave 4, the graveyard 2, the village shrine 1, the trial 3 to 5, the other side of each diamond about 3 each (12) | + 26 to 28 | + 12 | | |

That sits inside `WORLD.md`'s 55 to 70 for a duo land, near its low end, which leaves room for the god fight to grow (the owner wants
the heavens deeper). Levers if play runs long, in order: the second fox pair of E3.1s and E3.2w (0.6), the drummer in the bailey
(0.4), one samurai of the drum court (0.5), one yamabushi of the hall of fans (0.5).

**Fires apart (new player's minutes).** F1 to F2 about 8; F2 to F3 3.3; F3 to F4 4; F4 to F5 3.8 (with the vista); F5 to F6 4;
F6 to F7 or F8 about 4.5; F7 or F8 to the elite 0; the elite and back to F6 about 4.5; F6 to F9 about 1. The crossings are closer than
the decided 6 to 8 because they are decided rest places and the stairs are the land's hardest walking; the race is the cost that keeps
them from feeling short (about five races between each pair).

**Enemy count** (one side of each diamond; a fox pair counts as one enemy, its double not counted).

| District | Main path | Count |
|---|---|---|
| D1 | 4 ship-ghosts, 8 small crabs, 2 Heike crabs, the general, 1 fox pair | 16 |
| D2 | Either side: 2 fox pairs and 4 messengers (raiju and runners, alone and in pairs) | 6 |
| D3 | Either side: 3 to 4 fox pairs and 6 to 7 messengers | 10 to 11 |
| D4 | 5 struck samurai, 3 drummers, 4 yamabushi, 3 fan-bearers | 15 |
| Elites | The Last Retainer, the Hermit of the Pass | 2 |
| Side paths | The wreck 8 crabs, 2 ghosts; the village shrine's fox pair; the graveyard 2 raiju; the cave's runner; the other sides of the diamonds about 16; the trial's champions | about 30 |
| **All** | | **about 48 on the main path, two elites, and the two gods** (Fire 40, Ice 34, Poison 57) |

Fewer on the stairs than anywhere else in the climb because the race is the stairs' other opponent: about 25 races on a clean pass,
each a read and a choice.

---

## 10. Questions for the owner

**Answered (owner, 2026-11-19).**

1. **A perfect dodge through the race** gives one shared gift, the same for both racers: the ordinary perfect dodge
   (12 mana, slowed time) and Keeping Pace. This is option (b), not the recommendation: there is no Charged, no Carried
   and no extra mana.
2. **The count** is eight sounds from the player's own stair's gates with the strike on the ninth; the trailing stair
   strikes half a beat late; the tempo rises by district.
3. **Both sides of every diamond** are fully built, each with its own encounters.
4. **Both elites** must fall before the screen room opens: two seals, either order.
5. **The trial** is the Messengers' Course.
6. **The screen room's roof** is never drawn while the player is inside.
7. **The plain weapons** are the uchigatana in the wreck and the iron-ribbed tessen on the knees of the meditating
   hermit, who is kept.

The body of the plan has been brought into line with these answers (2026-11-19). The rest of the plan stays Proposal
(Claude) until the owner walks it.

Each has two or three options, the recommended one first.

1. **What does a perfect dodge through the race give?** (Decided: it is rewarded, not only sheltering.)
   - **(a) Each racer its own gift, on top of a shared one.** Shared: Keeping Pace (4 s faster, longer rolls), 20 mana and a
     glimpse of the racer in the slowed third of a second. The bolt gives *Charged* (the next three blows fork); the gust, held,
     gives *Carried* (ridden 8 m up the lane). Window 0.2 s. Recommended: the gift pulls the player into the race instead of away
     from it, as Bayonetta's Witch Time rewards the last-moment dodge with an opening rather than mere safety and so teaches each
     attack's timing; two different gifts make the decided "change stairs" a real choice (fight or move).
   - (b) One shared gift for both: the ordinary perfect dodge (12 mana, slowed time) and Keeping Pace. Simpler; the two stairs
     differ only in their sweeps.
   - (c) Only the ordinary perfect dodge's mana, nothing more. Smallest, in line with the owner's caution about rewards in Ice; the
     decided "rewarded" is met only barely.
2. **How does the count read, and does the lead matter?**
   - **(a) Eight sounds from the player's own stair's gates, four far and four near, the strike on the ninth; the stair whose racer
     trails strikes half a beat late; the tempo rises from district to district.** Recommended. Crypt of the NecroDancer was built so
     that enemies moving on fixed beats would be "fair and very predictable", and its later zones raise the tempo to make it harder;
     the taiko form *jo-ha-kyū* (slow, breaking, rushing) gives the climb its arc. The half-beat lead turns the decided swap of lead
     into something heard.
   - (b) The same count, both stairs on the beat; the lead is only seen (who is ahead in the streak). Easier to learn, the same on
     every leg.
   - (c) No fixed count: the cloud brightens and a roar rises, and the player reads the sweep itself. Most natural; much harder to
     time a dodge to, and the "drumbeat heard coming" is weakened.
3. **How much is built on both sides of every diamond?**
   - **(a) Both sides fully built, each with its own encounters; the player picks a side at every crossing; the other side is
     optional and is the way back down.** Recommended: it makes the decided "change stairs" a choice every time, and the second side
     is the land's exploration; Sekiro's Ashina castle is praised for many ways up the same height, its roofs and its ground both
     real routes.
   - (b) Both sides built, but the second side of each diamond gets one encounter instead of three. About a third less work; the
     choice stays, the reward for exploring shrinks.
   - (c) One open side per diamond, alternating stairs, the other fenced as the race's. Half the building; the player still meets
     both stairs, but never chooses.
4. **Must both elites fall before the screen room opens?**
   - **(a) Yes: two seals on its doors, a drum and a knotted cord, each broken by one elite, in either order.** Recommended: each
     elite teaches what its god does, and the decided "taken in either order" implies both are taken; Dark Souls' four Lordvessel
     souls show a free order with a fixed sum working well, and Earth and Life's meeting ground is proposed with two seals the same way.
   - (b) Either elite opens the doors; the other is optional with its technique and Memory. Faster for a new player; the god fight
     loses one of its two previews.
5. **What is the land's trial?** (The owner's own example was "a race in the Lightning land".)
   - **(a) The Messengers' Course: race the messengers' champions up a straight crag stair with its own sweeps; perfect dodges carry
     the player; ring the top bell first.** Recommended: the owner's example, the verb at its fastest, and the player as a racer before
     the gods race to the player. *Hi-Fi Rush* shows hazards on the beat as a way through a level, not only an obstacle.
   - (b) The Thunder Hunt: catch a great raiju blinking lantern to lantern in the graveyard by reading its order. Strong; a chase, not a
     race.
   - (c) The Waterfall Austerity: a duel with a yamabushi master standing under the fall. A good fight; Water's trial is a duel already.
6. **How is the screen room's roof drawn?** (Decided: nothing walkable under a roof; and the screen room is a shut, still room.)
   - **(a) The room's roof is never drawn while the player is inside (a cut-away, as the tomb's vault is never drawn in Poison); from
     outside it is an ordinary roof over a room the player is not in.** Recommended: no player is ever hidden, which is what the rule
     is for, and the room keeps its stillness and its glow through the high paper windows. Diablo and The Sims cut walls away the same
     way.
   - (b) Roofless: the storm broke it open. The rule kept literally; the room is no longer "shut out" and still, the one quiet place.
   - (c) The room in its own scene. The rule kept; an extra load between the summit and the roof.
7. **The two plain weapons: where and in what make?**
   - **(a) The plain Katana in the wreck, a plain uchigatana in a salt-whitened scabbard across the knees of an empty armour; the
     plain War Fan in the hermit's cave behind the waterfall, an iron-ribbed tessen on the knees of a hermit who sat down to meditate
     and never rose (a quiet nod to the self-mummified monks of the Dewa mountains).** Recommended: both are "see it, then find the
     way" places with the best views in the land, and the look changes, not the weapons, as the owner chose for the Chain Sickle and
     the Great Bow.
   - (b) The same places, the hermit simply gone: an empty cave, the fan on his stone. Avoids any echo of a real devotional practice;
     loses the image.
   - (c) The same places and people, the weapons in their standard looks. Less modelling; two odd objects in the land.

---

## Sources

- Itsukushima's shrine on piles, its corridors and the great torii in the tide:
  [Get Hiroshima, Itsukushima Shrine](https://gethiroshima.com/museums-attractions/miyajima/itsukushima-shrine/),
  [Get Hiroshima, Miyajima tide times](https://gethiroshima.com/museums-attractions/miyajima-tide-times/),
  [Japan Royal Service, Miyajima on both tides](https://japanroyalservice.com/blog/miyajima-on-both-tides-hiroshima-for-residents-of-the-world).
- Mount Haguro's stone stair through the cedars, its five-storeyed pagoda of bare wood with roofs of one size, and the yamabushi:
  [japan-guide](https://www.japan-guide.com/e/e7902.html), [Dewa Sanzan](https://thedewasanzan.com/haguro/),
  [Wikipedia, Three Mountains of Dewa](https://en.wikipedia.org/wiki/Three_Mountains_of_Dewa).
- Sōtatsu's wind god and thunder god screens (the composition across the gold field):
  [Wikipedia, Tawaraya Sōtatsu](https://en.wikipedia.org/wiki/Tawaraya_S%C5%8Dtatsu),
  [Wikimedia Commons](https://commons.wikimedia.org/wiki/File:Wind_God_and_Thunder_God_Screens_by_Tawaraya_Sotatsu_hi-res.png).
- *Jo-ha-kyū* in taiko and court music: [Wikipedia](https://en.wikipedia.org/wiki/Jo-ha-ky%C5%AB).
- The raijū, the beast that falls with lightning and scratches struck trees, and thunder-hunting:
  [Wikipedia](https://en.wikipedia.org/wiki/Raij%C5%AB), [yokai.jp](https://yokai.jp/en/yokai/raijuu).
- The funayūrei and the bottomless ladle: [Wikipedia](https://en.wikipedia.org/wiki/Funay%C5%ABrei),
  [Wikipedia, Umibōzu](https://en.wikipedia.org/wiki/Umib%C5%8Dzu).
- The Heike crab and Dan-no-ura: [Wikipedia, Heikegani](https://en.wikipedia.org/wiki/Heikegani),
  [Wikipedia, Battle of Dan-no-ura](https://en.wikipedia.org/wiki/Battle_of_Dan-no-ura).
- Crypt of the NecroDancer's beat-based fairness and rising tempos:
  [Game Developer, Road to the IGF](https://www.gamedeveloper.com/business/road-to-the-igf-brace-yourself-games-i-crypt-of-the-necrodancer-i-),
  [The Gemsbok](https://thegemsbok.com/art-reviews-and-articles/mid-week-mission-crypt-necrodancer-brace-yourself-games/),
  [NecroDancer wiki, gameplay basics](https://necrodancer.miraheze.org/wiki/Gameplay_Basics).
- Hi-Fi Rush's hazards on the beat: [Destructoid review](https://www.destructoid.com/reviews/review-hi-fi-rush-destructoid-tango-gameworks/),
  [Noisy Pixel review](https://noisypixel.net/hi-fi-rush-review-ps5/), [Wikipedia](https://en.wikipedia.org/wiki/Hi-Fi_Rush).
- Bayonetta's Witch Time, the dodge rewarded with an opening: [PC Gamer review](https://www.pcgamer.com/bayonetta-review/),
  [Bayonetta wiki](https://bayonetta.fandom.com/wiki/Witch_Time).
- Ghost of Tsushima's Guiding Wind and the world's own guidance (grass, birds, foxes):
  [Game Developer, vorticles](https://www.gamedeveloper.com/design/using-vorticles-to-simulate-wind-in-i-ghost-of-tsushima-i-),
  [Ludonode, guiding without a HUD](https://ludonodestudios.medium.com/%EF%B8%8F-the-invisible-hand-how-ghost-of-tsushima-guides-players-without-a-traditional-hud-70f6772fcacc).
- Sekiro's vertical world and Ashina castle's many ways up:
  [World design lessons from FromSoftware](https://medium.com/@Jamesroha/world-design-lessons-from-fromsoftware-78cadc8982df),
  [TheGamer, Sekiro's areas](https://www.thegamer.com/sekiro-best-areas-ranked/).
- Soulslike loops and shortcuts opened from the far side: [The Level Design Book, Undead Burg](https://book.leveldesignbook.com/studies/sp/undead-burg).
- From memory, not looked up this session: Dark Souls' Lordvessel bosses and Anor Londo's archers; Mega Man's free stage order;
  Ghost of Tsushima's standoffs; the chain routes (kusari-ba) of Japan's holy mountains; the self-mummified monks (sokushinbutsu) of
  the Dewa mountains; the cut-away walls of Diablo and The Sims.
