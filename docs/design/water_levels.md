# The Water land's levels (first pass)

Written by Claude on 2026-11-18 as the Water land's counterpart to `fire_levels.md`, before any of the land is built.
**Everything here is Proposal (Claude)** unless it is marked **Decided** with its source. The land's myth, people,
elite and fight are `WORLD.md`, "Water's land: the fisher of islands"; its look is `LOOKS.md` section 4; the rules every
land follows are `WORLD.md`, "Level design rules for every land (decided 2026-11-18, evening)". The enemies' numbers
(health, telegraphs, damage) do not exist yet: they belong in a later `water_land.md`, as `fire_land.md` section 4 holds
Fire's. This plan states only what each room needs each enemy to do.

Axes as in Fire: x east, z north, y up; north is up the screen. Coordinates are metres. "Floor y 2.6" is the walking
height. The land scene is `FisherLagoon` and the god's beach is its own scene, `FisherBeach` (names proposed), as
`SunCity` and `SunCourt` are two. The player wakes at (0, 2.6, 0).

---

## 0. What the owner has decided, and how this plan reads it

| Decided | Source | How this plan uses it |
|---|---|---|
| The map is islands round a lagoon, joined by causeways that open at low tide | `WORLD.md`, level design rules, 2026-11-18 evening | The main path goes round the lagoon's south, east and north sides; a reef road across the middle is the side path and the great loop (section 2) |
| The verb is *turn*: tide stones flip the whole lagoon between low and high; low opens the reef paths and wrecks, and the bait does best in the shallows; high raises the drowned and floats the player over walls and up to high ledges; stones stand only at certain places, so a chosen tide holds until the next stone | same | Section 1.2. The tide never moves by itself: the earlier "rises and falls on a cycle" (2026-11-17) is read as replaced by the verb, except in the god's fight, where the fisher pulls it |
| Rest fires in Fire's rhythm: one every 6 to 8 minutes of a new player's walk, always one at the god's door, a shortcut loop per district back to its fire, 5 to 7 fires | same | Six fires, three shortcuts and a loop (sections 2 and 4) |
| Two kinds of optional discovery: "see it, then find the way" (the main kind, holding the land's plain weapon and lore), and one optional trial, invented for this land | same | The west decks, the drained pond, the reef road and its wreck; the trial on the centre islet (section 3.4) |
| Up to three walkable levels per district, joined by ramps, stairs and ladders seen from above; nothing walkable under a roof; roofs and overhangs only fade | same | Every district names its three levels. Stilt decks count as roofs over the mud below them (1.1) |
| The four corruptions: the bait, the hooked, the drowned, the reef-grown; fishermen's weapons | `WORLD.md`, "The land" | Section 1.5 |
| The brothers, a duo elite of blade and hook, in the deepest wound of the fished-up island: narrow, red wet rock that still breathes, tools stuck in the walls; the sea god's remains seen where they cut deepest | `WORLD.md`, "The elite: the brothers" | Section 5 |
| The fight starts with a duel on the beach; the canoe is the fisher's own, waiting on the beach, and he lets the player take it | `WORLD.md`, "The fight" | Section 6 |
| Tropical sun with a storm coming in from the sea, nearer the deeper you go | `LOOKS.md` section 4 | The sea and the storm are to the north, where vistas look; the storm takes more sky in each district |
| The bone hook is the Chain Sickle's god's bough; the hook brother teaches Hook and Hold (Chain Sickle), the blade brother Fillet (Twin Knives) | `GAME.md` and `steel/weapons.md`, 2026-11-18 | So the land's plain weapon is the plain Chain Sickle (section 3.4, question 7) |
| Water is in tier 2; a clean pass for a new player about 40 minutes (Proposal in `WORLD.md`) | `WORLD.md`, "How a climb is structured" | Section 8 |

`WORLD.md`'s caution stands over everything here: Maui and the sea gods are living traditions. The plan keeps names
generic ("the meeting ground", not a real sacred word), keeps carvings vague and respectful, and copies nothing from
*Moana*. A cultural consultant before the land is dressed is worth the cost; *Tchia* was made largely by people from New
Caledonia, with local voice actors and Kanak musicians (sources at the end).

---

## 1. Principles

### 1.1 What the camera sees: what changes for this land

The camera is Fire's (`fire_levels.md` 1.1): the picture reaches 7.2 m north of the player and 4.3 m south, 15.5 m
wide at the player; nothing farther north is ever on screen; landmarks are rooms the player arrives in, or vista zones
(`LandZones`, which calls `TopDownCamera.SetFraming`). The south-side rule, faded roofs, enemies that walk straight at
the player with no navmesh and ranged attackers kept in frame all hold as written there. What this land changes:

1. **A ring, not a march.** The path goes round the lagoon, so long stretches run east or west. The land still rises
   northward overall (the village y 0.4 to 5, the island to y 14, the cliffs to y 20), and wherever the path runs east
   or west, rises stand on its north side, so every face the player must read faces south.
2. **Going down northward.** A stair or ramp descending north at 30 degrees or less is fully seen (the top ray is 35
   degrees below the horizon); a sheer drop on the north is not. Every way down to the north is a stair: into the
   Gash, and the cleft down to the beach.
3. **Cuts that run east-west.** The brothers' cuts and the gills cross the island east-west, so their south walls would
   stand between the camera and the floor. The brothers cut in **benches** (0.7 m high, 0.8 m deep: the walls step back
   at about 40 degrees, like a quarry face), and the gills' south sides are ribbed slopes of at most 45 degrees. North
   walls stay sheer: they face the camera and read as the next level up.
4. **Under the decks.** "Nothing walkable lies under a roof" (Decided) includes stilt decks: the mud under every deck
   and stilt house is closed by a screen of stilts and hanging fish traps, so at low tide nobody walks under a floor.
5. **Water you can see into.** Shallow water (to about 1.5 m) shows the floor through it, tinted turquoise, and anything
   walking on that floor as a dark shape; deeper water is ink. **Nothing attacks out of ink.** Every drowned, bait body
   and reef-grown placed below stands in water shallow enough to show it, or in a pool whose edge the player sees.
6. **Edges.** Every raised walkable edge over land is bounded by a rim (a rock lip, a deck rail, a stilt fence) at
   most 0.6 m high on its south side. An edge over water is caught by the water: the player wades, or meets the
   throw-back (1.2). Nothing falls out of the world (the owner's Sun City playtest); `FallCatch` stays under the whole
   scene.
7. **Ranged attackers in water.** Harpooners keep Fire's rule (east or west of the fight, at most 6 m north, at most
   1 m up). A net-drowned may stand south of the player only within 4 m, inside the bottom edge of the frame.

### 1.2 The tide

**Two states for the whole lagoon (Decided).** At **low** the water stands at y 0; at **high**, y 2.0. The sea outside
the reef follows it. What a floor is at each tide follows from its height:

| Floor height | At low | At high | What uses it |
|---|---|---|---|
| Below -1.0 | Deep water | Deep water | Channels, the fishpond, pools that hold water at both tides: the reef-grown's and the net-drowned's ground |
| -1.0 to 0 | A pool (wading) | Deep | Tide pools: where the bait's lures stand |
| 0 to 1.0 | Dry or wet mud, sand and reef | Deep | **Low-only ground**: the village road, the causeway, trench floors, the reef road, the head beach |
| 1.0 to 2.0 | Dry | Wading, 0.3 to 1.0 deep | Wading floors, where a high-tide fight is slowed (Cut 2's floor) |
| 2.2 and above | Dry | Dry | **Both-tide ground**. Decks, platforms, docks and landings are all at **y 2.6** |

**The player in water.** Ankle-deep (under 0.3 m): as on land. **Wading** (0.3 to 1.0 m): walk 4 m/s, roll 3 m and
0.1 s slower to recover, every step splashes. **Deep** (over 1.0 m): not entered. A player who steps, rolls or is
dragged into deep water meets **the throw-back**: hands from under take them down and they are put back on the last firm
ground they stood on (`FallCatch.LastFirm`) 1.2 s later, with 10 damage, no Shards lost, every enemy as it was. It is
the drowned's land saying no, not a death. An enemy knocked, yanked or fallen into deep water is taken under and counts
as killed, with its Shards; the drowned and the reef-grown are at home there and are not.

**No swimming: floats.** "High floats the player over walls and up to high ledges" (Decided) is read as: rafts,
pontoons and old hulls, each tethered to posts so it rises and settles in place, carry the player. A float's deck is
0.3 m above the water when afloat and 0.3 m above the floor when resting on it. At high its deck is at y 2.3, a 0.3 m
step below every deck, landing and wall-top at y 2.6: a float lifts the player over a 2 m sea wall standing on low
ground, or up to a ledge 1.4 m above a trench floor. At low it lies on the mud and the same wall or ledge is out of
reach. (Question 1 asks whether the owner wants swimming instead.)

**The turn.** Hold Use for 1.5 s at a tide stone ("Turn the tide"); a blow interrupts it. The water then moves for 6 s,
its edge a line of foam travelling across the floor; floats rise or settle with it; the light dims and greens a little
toward high and dries and pales toward low (`LOOKS.md`). A status line: "The tide goes out." / "The tide comes in."
The first turn in the land is a landmark moment: the camera eases back 1.6 times for those 6 s (V0).

**The tide holds** through a fall, a rest, a trip to the hub and back, and across sittings (a progress key,
`water.tide`, as `LandVoice` keeps others). The land starts at **high**. (Question 3.)

**Reading it.** Every post, wall and face the tide reaches carries a band of dark weed and barnacle up to y 2.0 and a
wet dark line at y 0, so anywhere in the land the player can see what high water covers. Where a route opens at one
tide only, a carved tide post stands at its mouth, its weed band showing which. (Question 2.)

### 1.3 Never stranded

1. The tide changes only at a stone, and only while the player stands at it, on both-tide ground.
2. So a player can be cut off from every stone only by arriving somewhere at a tide they did not choose there: waking
   at a fire, landing from a one-way drop, or the throw-back.
3. The plan therefore holds three rules, checked in the tables below:
   - every rest fire stands on both-tide ground and reaches a stone at **either** tide without crossing tide-gated
     ground;
   - every one-way drop lands where a stone can be reached at either tide;
   - the throw-back returns the player to ground they reached at the present tide, so the way they came is open.
4. Nothing else changes the tide (the god's beach is its own scene). A puzzle is never "turn and hope".

### 1.4 Measures

| What | Measure | Why |
|---|---|---|
| Walk, wade, roll | 6 m/s; wading 4 m/s, roll 3 m | 1.2 |
| The causeway | 4 m wide, water or reef either side | One enemy at a time; a sidestep, not a circle; a roll the wrong way is the throw-back |
| Decks and landings | 3 to 4 m deep, y 2.6, a 0.6 m rail on the south | One-on-one; a drowned's grab reaches 1.5 m in from an edge, so the middle of a 4 m deck is safe |
| A raft; the pontoon; a hull bridge | 3 by 4 m; 3 by 10 m; 2.5 by 9 m | A crossing, not a fight floor. Nobody but the player and one enemy fits |
| A group fight | 12 to 16 m across, 10 to 12 m deep | As Fire |
| The drowned's grab | from water 0.6 m deep or more, at a player within 1.5 m of it | The danger is edges, and it reads from where the water is |
| A bait | its body triggers at 3 m; its lure stands up to 5 m from the body | Walking to the lure passes the body; walking past at 4 m does not |
| The harpoon; the net | lane 12 m, out and back; a cast of 6 m | Dodged twice; thrown from where the frame shows the thrower |
| A hooked's yank | 2 to 4 m along its line's bearing, 0.4 s of warning | Stand off the line's bearing |
| A rise that reads; a drop; a step; stairs; ramps | 1.5 m; 2 m; 0.3 m; 27 to 32 degrees; 8 degrees fought on, 15 walked | As Fire |

### 1.5 The land's people, as the rooms use them

All four corruptions are **Decided**; how each one fights below is **Proposal**, each mechanic its own (the owner's bar:
no reskins), each tied to its lore and to the tide.

- **The bait** (a lure on a stalk, like an anglerfish). A figure waist-deep in a pool, waving and calling ("Help me...
  the water..."), is the lure. The body that owns it lies buried in wet mud or sand up to 5 m away, the stalk running
  under the ground between them as a ridge, with bubbles over the body. Come within 3 m of the **body** and the mud
  bulges for 0.7 s, then jaws burst up in a ring 2.5 m round it (red: roll out). A missed bite leaves it up for 2.5 s,
  gaping and helpless. Strike the **lure** instead and the body tears up at once and lunges along the stalk at the
  player (a lane, red). A bait swallows anything that comes near its body, enemies too, so a hooked yanked across it is
  eaten. **Tide:** it hunts only at low, in the shallows (Decided); at high its pool is deep, the lure drifts silent and
  the body lies still.
- **The hooked** (people on lines pulled from somewhere unseen). Each has a thin line running from its back off the
  screen, always toward the sea. It does not walk so much as get jerked: every few seconds the line goes taut and
  thrums (white, 0.4 s) and it is yanked 2 to 4 m along the line's bearing, striking whoever is in the way. Between
  yanks it fights with its weapon. **Cut the line** (any blow that crosses the line within 2 m of its back) and it goes
  still and kneels: one blow kills it, or it can be left. Two weapons: the **gaffman** (a short hooked pole that drags
  the player 2 m in; guardable) and the **harpooner** (a harpoon on a line thrown down a 12 m lane, then reeled back
  along it: dodge it twice; a harpoon that strikes a stone or post stands in it and the reel drags the harpooner to it,
  which is the opening). **Tide:** at high, more are reeled in from the sea: when a stone is turned to high, any
  *reel-in point* tied to it drags in its hooked, once per rest.
- **The drowned** (they walk under the water and drag the player under). **At high** they walk along the bottom at
  3.5 m/s as dark shapes, straight toward the nearest water to the player; they cannot leave the water. At an edge they
  surface in a ring of foam 1.4 m round the player's feet (0.9 s, red): caught, the player is held and dragged 2 m
  toward the water, into the throw-back if that water is deep. A missed grab leaves the drowned on the surface for 2 s,
  helpless. **At low** they are stranded wherever the water left them: lying in the mud, crawling at 1 m/s, reaching
  only 1 m. Turn the tide and the land's drowned are easy kills; turn it back and the living ones rise again.
- **The net-drowned** (the drowned's netman). Stands in water that is deep at both tides and casts a weighted net 6 m:
  a caught player is slowed to half speed and cannot roll until they cut free (three blows, or 3 s). Never alone: the
  net is there to pin the player for something else.
- **The reef-grown** (coral grows over them; the older, the more armoured). Plates of coral, coral red at the edges (the
  land's accent, `LOOKS.md`): the young carry one, the old three. A plate is broken by heavy blows; light blows glance.
  **In water** (wading depth or more) they move fast, 7 m/s in surges, and regrow a broken plate every 8 s; **on land**
  they are slow (1.8 m/s) and grow nothing. Their weapon is the shark-tooth club: close cuts that leave a bleed.
  **Tide:** dangerous in water at any tide (Decided), so they live in channels and pools that hold water at both; the
  answer is to draw them onto dry ground.

**What the tide does to them, at a glance:**

| | Low | High |
|---|---|---|
| Bait | Hunts in the shallows | Dormant |
| Hooked | As placed | Reel-in points fire, once per rest |
| Drowned | Stranded, crawling | Walk under the water; grab at every edge |
| Net-drowned | In deep pools and channels | Anywhere deep |
| Reef-grown | In channels and pools | More water to be fast in |

So every turn costs something: low wakes the bait and opens the reef; high raises the drowned and brings the hooked.
(Question 4.)

---

## 2. The whole land

Round the lagoon from the south: the drowned village on the reef flat, a causeway to the fished-up island, up the
island's flank and through its wounds to the brothers, over the fish's jaw to the navigators' cliffs, across their canoe
harbour and up to the seat where the star stones stand, and down a cleft to the beach (its own scene). Across the
lagoon's middle, at low tide only, runs the old reef road: the side path, the trial and the great loop. The main path
is about 620 m of walking.

```
 z
394 ............. [hook arch] -> FisherBeach (own scene)
       NAVIGATORS' SEAT y20 (star stones)           storm and sea to the north
368    west headland stair y2.6->14 ==S3 rope bridge y14== east headland y14
352    WEST SHORE y2.6 T6 |raft slip| HEAD BEACH y0.6 | EAST SHORE y2.6 F5 T5
330    sea wall, landing  |  CANOE HARBOUR (inlet)  | slipways, sheds   <- switchback  JAW BRIDGE y14
320    ~~~~~~~~~~~~~ sand bar y0.6 (reef road ends) ~~~~~~~~                     (70..92, z328)
310                                                         THE GASH floor y6 (x88-96, z280-310)
262                                                         F4 (88,14,262) ridge y14
236                    reef road      wreck (31,2.4,225)| Cut 2 (z222-236), terrace 3 y11, S2 net bridge
                         |              |channel x37-42 | terrace 2 y8
207      LAGOON         |                              | HOLLOW y5 F3 (70,5,207) T4
192                     |                Gill 1 (36,0.4,194)->(62,5,204)
174     CENTRE ISLET y2.6 (the trial)    Cut 1 (z174-186)   flank shore y2.6, terrace 1 y5
148                     |                tail landing y2.6 T3 (56,2.6,148)
110     reef road     MEETING GROUND y2.6 F2 T2 ==CAUSEWAY y0.6==> (12,110)->(46,136)   pond (SE)
 54     (low only)    VILLAGE ROAD y0.4 | west decks y2.6 | east decks y2.6 -S1-> platform
 40                   LANDING-STONE PLATFORM y2.6 T1 (8,2.6,46)
 26                   ridge y5 (canoes in the trees)
  0                   LANDING BEACH y2.4-3, wake (0,2.6,0), F1 (4,3,10)
      x: -40       -20        0        20        40        60        80        100
```

**Heights.** Sea and lagoon: low y 0, high y 2.0. Village road 0.4; decks, platform, meeting ground 2.6; ridge 5.
Causeway 0.6. Island: shore 2.6, terrace 1 y 5 (the hollow), terrace 2 y 8, terrace 3 y 11, ridge 14; Cut 1 floor 0.5
to 2.2; Cut 2 floor 0.4 to 4 (1.2 at the crossing); the Gash 6. Cliffs: headlands 14, harbour shores 2.6, head beach and
sand bar 0.6, the seat 20. Reef road 0.3; the centre islet's rim 2.6, its pool 1.2.

**Rest fires (six).** A fire here is a fire in a ring of basalt stones, not a brazier.

| Fire | Where | Foot | Why there | Stone at either tide |
|---|---|---|---|---|
| F1 | The landing beach, under a leaning palm | (4, 3, 10) | In sight of waking | T1, 36 m, over the ridge (both-tide ground) |
| F2 | The meeting ground | (2, 2.6, 100) | Relief after the village road; S1 brings the player back to F1 | T2, 16 m |
| F3 | The breathing hollow, mid-island | (70, 5, 207) | After the gill; S2 comes back to it | T4, 8 m |
| F4 | The ridge's end, at the Gash's lip | (88, 14, 262) | The brothers' door | T4: at high by Cut 2's hull or S2; at low by the drop from Cut 2's north ledge, or S2 |
| F5 | The east shore of the canoe harbour | (37, 2.6, 336) | Before the harbour's crossing; S3 comes back to it | T5, 16 m |
| F6 | The head of the cleft, in `FisherBeach` | see section 6 | The god's door (Decided: one always at it) | The beach's tide is his |

**The tide stones, and the order the main path asks for.** The land wakes at high.

| Stone | Where | Main path asks | What low gives | What high gives |
|---|---|---|---|---|
| T1 | Landing-stone platform, (8, 2.6, 46) | **Low** (the first turn) | The village road; the drowned stranded | Nothing new: the platform is a dead end in water |
| T2 | Meeting ground, (-14, 2.6, 98) | (Low already) | The causeway; the reef road | The west decks' rafts (the side path) |
| T3 | Tail landing, (56, 2.6, 148) | Nothing (optional) | Cut 1 by its floor; Gill 1 | Cut 1 by the pontoon; reel-in point R1 |
| T4 | Breathing hollow, (77, 5, 211) | **High** | Cut 2's floor to the wreck's view | Cut 2's hull bridge; reel-in R2 |
| T5 | East shore, (34, 2.6, 352) | **Low** | The head beach crossing; the sand bar | The drowned at the slipways |
| T6 | West shore, (4, 2.6, 344) | **High** | The way back east | The raft over the sea wall; reel-in R3 |
| T7, T8 | The centre islet's west and east rims, (-24, 2.6, 174) and (2, 2.6, 174) | (side) | The reef road home | The trial's own use (3.4) |

Main path: high, then low (T1), high (T4), low (T5), high (T6). Four turns, each one asked for by a place the player has
just seen.

**The route.** From the landing beach over the ridge to the landing-stone platform; turn low; down the village road to
the meeting ground (F2); along the causeway to the island's tail; up the flank shore, across Cut 1, up Gill 1 to the
hollow (F3); turn high; across Cut 2 on the floating hull, up to terrace 3 and the ridge (F4); down into the Gash and the
brothers; up the throat to the fish's head; over the jaw bridge to the east headland; down to the harbour's east shore
(F5); turn low; across the head beach to the west shore; turn high; ride the raft over the sea wall; up the navigators'
stair to the seat; through the hook arch and down the cleft to the beach (F6).

**Branches and loops.**
- The west decks (see it, then find the way; high tide): the village's lore.
- The fishpond, drained through its sluice (low tide): a carving under the water.
- The reef road (see it, then find the way; low tide): the plain Chain Sickle on a wreck, the trial on the centre
  islet, and the great loop from F2 to F5.
- Cut 2's floor to its mouth (low): where the wreck is first seen.

---

## 3. The districts

Each district: the idea, its three levels, a plan, then each space in walking order. Encounters are numbered E1.1 and
so on, as in Fire.

### 3.1 District 1: the drowned village (z -10 to 136)

**The idea.** A village of stilt houses on the reef flat inside the lagoon, drowned by a tide that comes too high. The
player first sees it at high: roofs and decks standing in turquoise water, dark shapes standing still under it. The
first stone turns the tide out, the water draws back across the whole frame, and a muddy road appears between the
houses with the drowned lying on it. `LOOKS.md`'s "crossable only at low tide" is kept: the road and the causeway are
low-only ground. **Levels:** the road and the reef (y 0.4), the decks, platform and meeting ground (y 2.6), the ridge
(y 5).

```
 z     x: -30   -20   -10    0    10    20    30    40
136                                    ......(46,136) tail landing
124                         CAUSEWAY  (24,120) sluice [hut]  POND (SE of wall, y-1.5)
112   reef  +--[GREAT HOUSE]--+  /(12,110)                    net-drowned, old reef-grown
104   road<-|  T2*     F2*    |-'   V2
 92  (west) +==== stair ======+  <- plank from east decks
 88   [deck D]  ~~channel~~ (rg)   road  |[deck]
 80   [deck C]                (h)       |[deck]   east decks y2.6
 70   [deck B] P1(lure) (b)  road y0.4 (h)|[deck]   (fixed planks between)
 60   [deck A]                          |[deck] <-S1 push plank
 50          +====== PLATFORM y2.6 ===========+  T1*(8,46)
 40          |   (d)  (d) under water at high |
 36               ## stair 5 -> 2.6 ##
 30               RIDGE y5  (h)  canoes in the trees   V1
 10        F1*(4,3,10)
  0        @ wake (0,2.6,0)    LANDING BEACH, surf to the south
```
`(h)` hooked; `(d)` drowned; `(b)` the bait's body, its lure in P1; `(rg)` reef-grown.

#### 1A. The landing beach (z -10 to 16, x -20 to 22, y 2.4 rising to 3.0)
- *Shape:* black sand between two arms of dark rock, the outer sea's surf at the bottom of the frame, a broken outrigger
  lying on its side. The sand rises gently north to a belt of pandanus and palms.
- *Sees on waking:* the fire under a leaning palm 10 m ahead and to the right; foam sliding in behind.
- *Enemies:* none. *Lore:* the sponsor speaks on waking.
- *Kit:* `fire_ring` (F1), `outrigger_broken` at (-6, 2.4, 6) yaw 70, palms and pandanus.

#### 1B. The ridge of the hanging canoes (z 16 to 35, path 6 to 12 m wide, crest y 5 at z 26 to 34)
- *Shape:* a low ridge of sand and rock across the island, the path climbing between palm groves. Two canoes hang in
  the trees at about y 8.5, at (-8, 8.5, 24) and (7, 8, 30), where a tide once left them: the first sign of how high
  the water came.
- *Sees on entering:* a figure on the crest, jerking as if on strings, and a thin line running from its back off the top
  of the frame.
- *Enemies:* E1.1 one **hooked gaffman** at (-1, 5, 30), facing south, its line running north-west toward the lagoon.
  The first lesson of the hooked: the yanks come along the line's bearing, the line can be cut, the gaff drags.
- *Vista:* **V1** on the crest at (0, 5, 33), 10 by 3 m: the camera eases back and shows the drowned village at high
  tide, roofs and decks in the water, dark shapes standing under it, the platform with its stone.
- *Lore:* W1, the prow of the higher canoe carved with a hook and a wave (no words): seen, not read.
- *Kit:* `canoe_hung` x2 in `palm_tall`, `pandanus`.

#### 1C. The landing-stone platform (x -8 to 14, z 40 to 50, y 2.6)
- *Shape:* a platform of fitted basalt at the village's south edge, reached by a stair down from the crest (x -3 to 3,
  z 35 to 40, 2.4 m over 5 m). At its north edge a stair runs on down into the village (x -2 to 2, z 50 to 54, to the
  road at y 0.4). At high that stair goes into water 1.6 m deep. **T1** stands on its east end at (8, 2.6, 46).
- *Sees on entering:* the stone, a carved basalt upright 2.2 m tall with its weed band; the water at the platform's
  edge; two dark shapes under it, still.
- *Enemies:* E1.2 two **drowned** standing under the water at (-2, 0.4, 55) and (6, 0.4, 57). Walk to the platform's
  north edge and they come to it and grab (the foam ring). The platform is 10 m deep: the stone is safe; the edge is
  not. Turn the tide and they are left lying on the road below, crawling: the land's first gift of the turn. The
  lesson in that order: the water is dangerous, and you can take it away.
- *The first turn:* V0, the camera eases back 1.6 times for 6 s and the whole village drains in one shot (question 6).
- *Kit:* the platform (`Plateau`), `tide_stone`, `tide_post` at the north stair's head.

#### 1D. The village road (z 54 to 90, the road 6 to 8 m wide, y 0.4; decks y 2.6 either side)
- *Shape:* a muddy road that winds north between two rows of stilt houses: from (0, 0.4, 54) to (-7, 0.4, 64),
  (-6, 0.4, 76), (-2, 0.4, 88). Mud with the curved lines of the last water, fish traps, a canoe lying across a fence,
  pools at the edges. The **west row**: house bodies x -28 to -20, their decks x -20 to -14 in four pieces (A z 54 to
  60, B 64 to 70, C 74 to 80, D 84 to 90) with gaps of 4 m between them; each gap is a pool at low with a raft lying in
  it. The **east row**: decks x 6 to 10 with the house bodies east of them (x 10 to 18), from z 54 to 90, joined by
  fixed planks. The houses' roofs fade; nothing under a deck is walkable (1.1).
- *Sees on entering (at low):* the road, the two drowned crawling in the mud below the stair, and a voice from the
  left.
- *Enemies:*
  - E1.2 the two drowned, stranded (or, at high, not here at all: the road is water).
  - E1.3 **the first bait.** The gap between decks A and B is a pool (floor y -0.6) that spreads east to the road's
    verge. The lure stands waist-deep in it at (-14, -0.6, 62), calling for help, right under deck B's rail. Its body
    lies under the road's west verge at (-10, 0.4, 64), 4.5 m away, a ridge in the mud running from the pool to it and
    bubbles over it. The road there runs x -10.5 to -3.5: a player who keeps to its east half passes more than 3 m from
    the body; one who goes to help walks over it.
  - E1.4 two **hooked gaffmen** at (-4, 0.4, 74) and (0, 0.4, 80), lines running west. Yanked across the bait's body,
    a hooked is eaten: the room's trick, for a player who read the mud.
- *Why it suits them:* the road is the bait's shallows; the decks' gaps make pools for its lure; the hooked are jerked
  about in a space with one buried danger in it.
- *See it, then find the way:* on deck B's south rail, 4 to 6 m west of the road, a carved board (W2) and a bundle on the
  deck, plainly out of reach: the deck is 2.2 m above the mud and has no ladder. The way to it is the west decks at high
  tide, from the meeting ground (side path below).
- *Kit:* `stilt_house` x8, `stilt_deck`, `fish_trap_fence`, `raft` x3 (in the west gaps), `mud_tile`.

#### 1E. The meeting ground (x -22 to 10, z 92 to 112, y 2.6)
- *Shape:* a great raised platform of fitted stone, the village's heart: open paving, carved posts (kept vague), the
  great house on its north-west (x -20 to -4, z 104 to 112; its roof fades). Its south face is 2.2 m of stone with a
  broad stair (x -4 to 2, z 88 to 92) from the road's end. Along that face, west of the stair, runs a **canoe channel**
  (x -22 to -6, z 86 to 91, floor y -1.5, deep at both tides) out to the lagoon. On the west edge a stair goes down to
  the reef road (x -22 to -26, z 98 to 102, running west). The east decks join its south-east corner by a fixed plank at
  (8, 2.6, 91); deck D of the west row joins its south-west corner by another at (-16, 2.6, 91).
- *Sees on entering:* off the stair, F2 straight ahead, the stone T2 to the left, the carved posts.
- *Enemies:* E1.5 a **young reef-grown** (one plate) in the canoe channel at (-10, -1.5, 88). When the player is within
  8 m on the road, it surges out of the channel onto the road at (-5, 0.4, 85), fast for the first 2 m, then slow on the
  mud. The lesson: out of the water it is slow and grows nothing; push it back into the channel and it is fast and whole
  again. None on the platform itself: relief.
- *Rest:* **F2** at (2, 2.6, 100). **T2** at (-14, 2.6, 98).
- *Vista:* **V2** at the north-east corner (6, 2.6, 109), 6 by 3 m, looking north-east: the causeway curving away over
  the water, the sluice hut, the island's tail beyond.
- *Lore:* W4 on the great house's central post: the village's story in one carving: a man in a canoe hauling up a fish
  as large as the island, and people on the shore waving to him (no words).
- *Kit:* `meeting_ground` platform, `great_house`, `carved_post` x6, `fire_ring`, `tide_stone`.

#### 1F. The causeway and the fishpond (z 104 to 140)
- *Shape:* the village's fishpond wall, built of stones laid dry and angled down so the waves draw them tighter (after
  the walled shore fishponds of Hawai'i), is the causeway. A stair (x 8 to 12, z 108 to 112, 2 m over 4 m) goes down
  east off the meeting ground onto its top, and the top, 4 m wide at y 0.6, curves north-east: (12, 0.6, 110),
  (24, 0.6, 120), (36, 0.6, 129), (46, 0.6, 136), then a ramp up to the tail landing (to (50, 2.6, 144), 2 m over 9 m).
  To its **south-east** is the pond (floor y -1.5, held full at low by the sluice: deep at both tides). To its
  **north-west**, at low, 6 m of dry reef flat (y 0.2) and then the deep lagoon. At high the whole wall is 1.4 m under:
  the causeway opens at low tide only (Decided). Halfway along, the **sluice** (a gate of upright slats in a channel
  through the wall) at (24, 0.6, 120), and on the pond side the watchman's hut on a stone stand (y 2.6, 3 by 3 m) at
  (27, 2.6, 117).
- *Sees on entering:* the wall running into the top of the frame, water on the right, dry reef on the left, a shape
  standing in the pond.
- *Enemies:* E1.6 an **old reef-grown** (three plates) in the pond at (32, -1.5, 121); when the player passes the sluice
  it surges up onto the wall at (30, 0.6, 125), in front of them. E1.7 a **net-drowned** in the pond 3 m east of the
  wall's second leg at (35, -1.5, 125), casting at the player on the wall as the reef-grown comes. The district's peak:
  pinned on a 4 m wall with a club coming. *The answers:* step down onto the dry reef on the north-west (0.4 m) and fight
  it there, slow and growing nothing; or knock it back into the pond, where it heals; or fight it on the wall with the
  net-drowned still casting. Cut free of the net and the net-drowned's cast takes 4 s again.
- *Lore:* the watchman's hut holds W3a, a tally of fish on its lintel that stops mid-row.
- *Kit:* `kuapa_wall` (build list), `sluice_gate`, `watch_hut` on a stand, `tide_post` at each end.

#### Side path: the west decks (high tide; from the meeting ground)
- *How:* at T2 turn the tide high. Deck D joins the meeting ground by its fixed plank; the gaps C-D, B-C and A-B now hold
  floating rafts at y 2.3, each a step down and up. Walk south along the decks to deck B: W2, the carved board, and the
  bundle: a canoe-maker's cache (Shards). Deck A at the far end looks out over the platform and T1.
- *Enemies:* E1.8 two **drowned** under the high water at (-10, 0.4, 72) and (-11, 0.4, 82), following the player
  along the decks' east edges and grabbing at the rafts, where every side is an edge. A raft is 3 by 4 m: stand in its
  middle and the grab cannot reach, step to its edge and it can.
- *Then:* back to T2 and turn low for the causeway. The side path is about 1.5 minutes.
- *Lore:* W2 (Claude's, new): "When the sea breathed out we walked to the fish. When it breathed in we went home." /
  "Then he pulled, and it breathed in, and did not breathe out."

#### Side path: the drained pond (low tide; from the causeway)
- *How:* at low, "Lift the sluice" at (24, 0.6, 120) drains the pond into the lagoon over 10 s (only at low; the next
  high tide refills it). Its floor is mud with pools; fishermen's steps lead down from the wall at (16, 0.6, 113). The
  net-drowned, if alive, is stranded.
- *Seen first:* from the wall, through clear water, a carved stone face on the pond's floor at (28, -1.5, 108).
- *Lore:* W3, the face of the sea's old one, worn smooth (the murdered god, a nod to Tangaroa; his name is the owner's
  to choose, and must not be a real deity's name). Words (Claude's, new): "We fed him the first fish of every catch." /
  "He gave the tide back every time."

### 3.2 District 2: the fished-up island (x 30 to 100, z 136 to 270)

**The idea.** Raw land that is still a fish (`LOOKS.md`). It lies with its tail at the causeway (south-west) and its
head toward the cliffs (north-east). Its flank, facing the lagoon, climbs in terraces patterned like scales; gills open
in its side and breathe; the brothers' cuts run into it from the lagoon, each deeper than the last, red wet rock
showing, their tools left in the faces. The main path climbs the flank: in through a gill at low tide, across the
deepest cut at high. **Levels:** the shore and the cut and gill floors (y 0.4 to 2.6), the terraces (y 5 to 11), the
ridge (y 14).

```
 z     x: 30    40    50    60    70    80    90   100
266                                    [ridge y14 ] F4*(88,14,262) -> Gash stair
246                       terrace 3 y11  (rg)  (h)      ^ ramp
237                  ..net bridge S2 (82,11)->(82,8)..
236    =mouth=|ledge y2.6 (h)R2 (h)  ##stair up east##|
230   (b)  ~~~CUT 2~~~ floor 1.2  [hull]  (d) (d)     |   head y4
222    benches                 terrace 2 y8           |
216                       ##steps 5->8##
207               HOLLOW y5   F3*(70,5,207) T4*(77,5,211)
200    GILL 1 (36,0.4,194) -> (62,5,204)  (rg) pool (rg)
190    path ##stair down##
186    ======= CUT 1 north face =========
180  (a)  ~~CUT 1~~ floor 0.5->2.2 [pontoon x42-45]  (h) (h) on north bank
174    benches
164  lure  (b)   FLANK SHORE y2.6  | scale wall (terrace 1 y5)
148       TAIL LANDING y2.6 T3*(56,2.6,148)  (h)(h) sledge
136  causeway arrives (46,0.6,136)
```
`(a)` harpooner; `(h)` hooked; `(d)` drowned; `(b)` bait; `(rg)` reef-grown; `R2` reel-in point.

#### 2A. The tail landing (x 42 to 62, z 136 to 152, y 2.6)
- *Shape:* the fish's tail: dark ridges of rock fanning out from the island like the rays of a fin, each a low wall
  1 m high, with wet-scaled ground between. The causeway's ramp comes up from the south-west.
- *Sees on entering:* two hooked hauling a sledge loaded with blocks of red cut rock, their lines running off north-west.
- *Enemies:* E2.1 two **hooked gaffmen** at (52, 2.6, 150) and (55, 2.6, 152), hauling (they drop the sledge rope and
  turn). The fin ridges cut the landing into lanes: a yanked hooked strikes a ridge and stumbles.
- *Stone:* **T3** at (56, 2.6, 148). Nothing on the main path needs it. It is there so that the island side of the
  causeway always has a stone, and it offers a choice at Cut 1. Turning it high fires **R1**: two hooked dragged up onto
  the landing's west shore at (43, 2.6, 146).
- *Lore:* none.

#### 2B. The flank shore (x 34 to 52, z 152 to 174)
- *Shape:* a shore path at y 2.6, x 40 to 50, between the lagoon and the island. West of it a beach slope runs down
  from y 2.6 to the reef flat at y 0.3; east of it rises the **scale wall**, terrace 1's face, 2.4 m of rock patterned
  in overlapping scales (it reads as the next level and cannot be climbed).
- *Enemies:* E2.2 a **bait**: its lure on the reef flat at (33, 0.3, 164) calling, its body under the beach slope at
  (37, 1.0, 167). Its body is 3 m from the path's west edge: keep to the path and it lies quiet. At high it sleeps.
- *Lore:* none.

#### 2C. Cut 1, the first wound (x 30 to 66, z 174 to 186)
- *Shape:* a trench 12 m wide cut east into the island from the lagoon. Its south side steps down in benches (z 174 to
  178); its floor (z 178 to 183) rises from y 0.5 at the lagoon end to y 2.2 at its head, against terrace 1; its north
  face (z 183 to 186) is sheer red wet rock with the brothers' adzes stuck in it. Hacked steps go down the south benches
  and up the north face at x 42 to 46. Moored across it there, a **pontoon** of lashed logs, 3 by 10 m.
- *The tide:* at low the floor at the steps (y 0.8) is dry: down, across 5 m, up. At high the floor there is under
  1.2 m: the pontoon floats at 2.3 and bridges bank to bank. Both tides cross; each has its danger (low: the floor; high:
  the drowned under the pontoon). This is where floats are taught.
- *Enemies:* E2.3 a **harpooner** on the south bank's west rock at (38, 2.6, 174), west of the player as they come up
  the path, throwing along the bank and recalling: the first harpoon, dodged twice. A **gaffman** on the north bank at
  (45, 2.6, 188) at the head of the steps, waiting to drag the player up into him. At high, add one **drowned** in the
  trench water at (44, 0.5, 180).
- *Lore:* W5, the brothers' tally of blocks cut, scored in the north face beside the steps, rows and rows of it; the
  last rows cut deeper and messier.
- *Kit:* `benched_cut` (build list), `pontoon`, `adze_in_wall` x3.

#### 2D. Gill 1 (from (36, 0.4, 194) at the lagoon to (62, 5, 204) at the hollow)
- *Shape:* a cleft 6 m wide in the fish's side, open to the sky, its north wall sheer, its south side a slope of curved
  ribs like gill rakers. Its floor rises from the lagoon at y 0.4 to the hollow at y 5: (36, 0.4, 194), (44, 0.8, 195),
  (52, 2.0, 198), (58, 3.6, 201), (62, 5, 204). The shore path meets it at z 190 and a stair goes down north into it
  (from (44, 2.6, 190) to (44, 0.8, 194)). A **gill pool** (3 by 5 m, floor y -0.8, sealed: water at both tides) lies
  beside the floor path at (48, -0.8, 196).
- *The tide:* at high the gill's lower 10 m is under 1.2 to 1.6 m of water: shut. At low, the way in. Since the
  causeway delivers the player at low, a player who has not turned T3 walks straight in; one who has must go back to it.
- *The breath:* every 8 s the gill **exhales**: a gust of warm mist rolls down it from the hollow for 1.5 s, pushing
  the player 2 m toward the mouth and hiding the lower half of the floor; then it **inhales**, drawing 1 m in. The mist
  is the cover the reef-grown move under.
- *Enemies:* E2.4 two **young reef-grown**: one in the gill pool (fast, healing), one lying on the floor at the inner
  bend (56, 3, 200), on land and slow. The breath pushes the player toward the pool; the pool is what makes the first
  one dangerous.
- *Kit:* `gill_cleft` (build list) with the breath (a mist emitter and a push volume on an 8 s rhythm).

#### 2E. The breathing hollow (x 58 to 82, z 198 to 216, y 5)
- *Shape:* a bowl in the fish's back where the gill comes up, its scaled floor rising and falling a few centimetres
  like a sleeping chest (drawn only; the collider keeps still). Its north face is terrace 2's, 3 m, with hacked steps up
  it (x 68 to 72, z 214 to 217).
- *Rest:* **F3** at (70, 5, 207). **T4** at (77, 5, 211).
- *Sees on entering:* the fire; the stone; the steps going up to a terrace from which comes the sound of water in a
  cut.
- *Enemies:* none.

#### 2F. Terrace 2 and Cut 2, the second wound (x 44 to 86, z 217 to 236)
- *Shape:* terrace 2 (y 8, z 217 to 222) is a strip along the south lip of **Cut 2**, the deeper wound: 14 m wide,
  its south side benched from y 8 down to y 2.6 (z 222 to 228), its floor (z 228 to 233) at y 1.2 at the crossing,
  falling west to y 0.4 at its mouth on the lagoon (x 44) and rising east to y 4 at its head (x 84). Along its north
  wall runs a **ledge** (y 2.6, z 233 to 236, x 50 to 80), cut by the brothers as a working shelf, and above it the
  sheer face to terrace 3 (y 11), climbed by a stair cut into it running east, from (70, 2.6, 235) to (82, 11, 237)
  (8.4 m over 16 m, 28 degrees). Hacked steps come down the south benches at x 64 to 68 to a last step at the waterline
  (y 2.4, z 228). Moored across the floor there lies the brothers' **hull**, an old canoe hull they used as a barge,
  2.5 by 9 m, half-loaded with red cut rock.
- *The tide (the district's puzzle):* at low the hull lies on the floor, its deck at 1.5; the ledge is 1.4 m above the
  floor: shut. At high the floor is under 0.8 m (wading), the hull floats at 2.3 between the last step and the ledge,
  and the player walks across it: **the only way on is high tide**, so at F3 the player turns T4.
- *Seen first, at low:* the player reaches Cut 2 first at low (the gill required it), climbs down the benches and finds
  the ledge out of reach and the hull lying in the mud below it. The cut's floor runs west to its mouth at the lagoon:
  see 2F's side walk below.
- *Enemies (at high):* E2.5 two **drowned** in the cut's water at (60, 1.2, 230) and (72, 1.2, 231), coming for the
  hull's edges; and reel-in point **R2**: when T4 turns high, two **hooked gaffmen** are dragged up the cut from the
  lagoon and onto the ledge at (58, 2.6, 234) and (76, 2.6, 234). The player steps off a moving hull onto a 3 m ledge
  with a hooked either side and the drowned at the edge behind. The district's peak before the elite. Falling off the
  hull at high is not deep water: it is wading among the drowned, and the way back up is the south steps.
- *One-way drop:* from the ledge down into the cut's floor (1.4 m, facing south). It lands where T4 is reachable at
  either tide (south steps, terrace 2, the hollow).
- *Lore:* W6, in the cut's north face above the ledge, the first glimpse of what lies under the island: a pale curve
  like the edge of a great bone, wet, showing through the red.
- *Kit:* `benched_cut`, `hull_float`, `ledge`, `adze_in_wall` x2, `red_rock_blocks`.

#### Side walk: Cut 2's floor to its mouth (low tide)
- From the hacked steps' foot, west along the floor (y 1.2 falling to 0.4) to the mouth at (44, 0.4, 230). E2.6 a
  **bait**: lure in a pool at the mouth, (47, -0.6, 231), body at (51, 0.6, 229), on the floor the player walks.
- **V3** at the mouth (45, 0.4, 230), 4 by 4 m, looking **west** (the framing's look-ahead pointed west 6 m, not north):
  across a deep channel (x 37 to 42) lies **the wreck** of a great double-hulled voyaging canoe on the reef, and on its
  tilted deck something glints like a shell lure: the plain Chain Sickle. It cannot be reached from here: the channel is
  deep at both tides. The way to it is the reef road (3.4).

#### 2G. Terrace 3 and the ridge (x 60 to 96, z 236 to 266)
- *Shape:* terrace 3 (y 11, z 236 to 246) is a broad shelf on the fish's back, rough with scales and seaweed tufts
  instead of grass. A ramp (3 m over 12 m) climbs north-east to the ridge (y 14), the fish's spine, which narrows to the
  head. From the ridge's end a stair goes down north into the Gash.
- *Enemies:* E2.7 an **old reef-grown** (three plates) on terrace 3 at (72, 11, 240), far from any water: slow, growing
  nothing, the place to learn to take its plates; and a **gaffman** at (66, 11, 242) whose gaff drags the player into
  the club's reach. Up here the land helps the player; the hooked is the one who undoes it.
- *Shortcut:* **S2**, a net bridge. On terrace 3's south-east corner at (82, 11, 237) lies a coiled net of heavy cord
  with plank treads, its far ends staked on terrace 2 across Cut 2's head. "Cast the net bridge" (from this side only)
  throws it across: a bridge 16 m long, falling 3 m to terrace 2 at (82, 8, 221) (11 degrees), walkable both ways after.
  F3 to terrace 3 without the hull, the drowned, the reeled-in hooked or a turn of the tide.
- *Vista:* **V4** on the ridge's end at (90, 14, 262), 6 by 3 m: the camera looks north over the Gash's lip, its red
  depth, and beyond it the head of the fish and the dark cliffs, the storm taking half the sky.
- *Rest:* **F4** at (88, 14, 262), on the ridge's end beside the Gash's stair.

### 3.3 District 3: the navigators' cliffs (z 326 to 394)

**The idea.** High dark headlands where the navigators kept their canoes and read the stars, now the edge of the storm.
The hooked's lines all run north over these cliffs toward the beach: the player is walking up the lines toward whatever
pulls them. The harbour between the headlands is the last tide puzzle, the hardest: low to cross, high to climb.
**Levels:** the harbour's shores and beaches (y 0.6 to 2.6), the headlands (y 14), the seat (y 20).

```
 z     x: -30   -20   -10    0    10    20    30    40    50    60    70    90
394     [hook arch] -> FisherBeach
382      ( star stones )  NAVIGATORS' SEAT y20   V5
368   ramp ^  stair landing y14 =====S3 rope bridge===== east headland NW corner y14
362        stair  [landing y2.6]  gorge                   hook stones (a)(h)
352       WEST SHORE T6* |raft slip| HEAD BEACH y0.6 (b)(b) | T5*  EAST SHORE
340        y2.6  (h)R3   |  ~~~ CANOE HARBOUR (inlet) ~~~   | F5* sheds  <- switchback
330                      |        (d)   (d)  slipways       |              JAW BRIDGE y14 (a) on stack
320               ~~~~~~~~~~~~~~~ SAND BAR y0.6 ~~~~~~~~~~~~~                  <- throat from the Gash
```

#### 3A. The jaw bridge (from (92, 14, 328) west to (70, 14, 332), 4 m wide)
- *Shape:* the fish's lower jaw, a natural arch of rock over a sea channel 14 m below, running west from the throat's
  head to the east headland. A rim of rough rock 0.6 m high along both sides. The first strong wind; spray comes up.
- *Sees on entering:* off the throat stair, the arch going left (west), and a figure on a sea stack just beyond it.
- *Enemies:* E3.1 a **harpooner** on a sea stack 2 m off the bridge's north rim at (80, 14.6, 335): 5 m north of the
  bridge's middle, in frame, out of reach. His harpoon crosses the 4 m bridge north to south; the reel back drags a
  hooked player toward the rim. Strike the harpoon's line while it stands in the rock (it is a line like the hooked's)
  and he is reeled away by whatever holds his own: gone.
- *Vista:* none: the arch is short, and the arrival on the headland is the reveal.

#### 3B. The east headland (x 28 to 72, z 330 to 372, y 14)
- *Shape:* a windswept top of short grass and black rock. Eight **hook stones**, carved uprights 2 m tall with
  hook-shaped heads, stand in two lines pointing out to sea, marking star paths. The way down is a **switchback** cut in
  the headland's south-west face: three runs of 16 m running east and west at 13 degrees, from (60, 14, 334) down to the
  east shore at (38, 2.6, 330). The headland's north-west corner (x 28 to 40, z 360 to 372) looks across a gorge to the
  west headland's stair: there, two posts with a cut rope hanging (S3's far end).
- *Enemies:* E3.2 a **harpooner** among the stones at (50, 14, 352) and a **gaffman** at (56, 14, 344). The stones are
  the room: a harpoon that strikes a stone stands in it, and the reel drags the harpooner to the stone, open.
- *Lore:* W7 on the tallest hook stone, at (46, 14, 360): the navigators' words (Claude's, new): "These stones point
  the way out." / "He took the canoes and said no one leaves the fish."

#### 3C. The canoe harbour, east shore (x 30 to 46, z 326 to 356, y 2.6)
- *Shape:* a stone shore with two long canoe sheds (bodies x 40 to 46, roofs steep thatch that fades) and three log
  slipways running west down into the inlet (from x 30 at y 2.6 to x 24 at y -0.5) at z 332, 340 and 348. The inlet
  (x 10 to 30, z 320 to 352) is deep at both tides (floor y -1.5). At its north end the **head beach** (x 12 to 30,
  z 352 to 360, y 0.6) runs across to the west side, below the gorge; a stair goes down to it from the east shore at
  (32, 2.6, 352) to (30, 0.6, 356). At its south end, the **sand bar** (z 318 to 324, y 0.6) where the reef road ends.
- *Sees on entering (at high, as the player comes from Cut 2):* the sheds, F5, the slipways going down into water, and
  across the inlet, 20 m off and out of frame, nothing: only the tide post at the head beach's stair, weed to its top.
- *Enemies:* E3.3 two **drowned** in the inlet at (26, -0.5, 340) and (27, -0.5, 348), coming to the slipway heads.
  The shore is 16 m deep: F5 stands 5.5 m from the water's edge, out of reach.
- *Rest:* **F5** at (37, 2.6, 336). **T5** at (34, 2.6, 352), beside the head beach's stair.
- *The tide:* at high the head beach is under 1.4 m: shut. Turn T5 low.

#### 3D. The head beach crossing (low tide; x 12 to 30, z 352 to 360, y 0.6)
- *Shape:* a strip of black sand 8 m deep below the gorge's mouth, with tide pools. The head beach's west part (x 2 to
  12) is **the raft slip**, a pit between the west shore's north edge and the navigators' landing, each 2 m above it,
  with a raft lying in it; from the beach a stair goes up south-west onto the west shore at (12, 0.6, 352) to
  (10, 2.6, 348).
- *Enemies:* E3.4 two **bait**, the land's last and hardest reading: lures in pools at (22, -0.6, 357) and
  (16, -0.6, 354), calling to each other as well as to the player, bodies under the sand at (25, 0.6, 353) and
  (14, 0.6, 358), on both lines a player would take across. The strip is 8 m deep: there is a way between the two
  bodies, 3 m from each, shown by the ridges.
- *Lore:* none.

#### 3E. The west shore and the sea wall (x -10 to 10, z 326 to 352, y 2.6; the landing x 2 to 12, z 360 to 366)
- *Shape:* the west shore is a working floor of the navigators: a half-built double-hulled canoe on trestles, lashing
  frames, coils of cord. At its north edge the raft slip (x 2 to 12, z 352 to 360), and across it the **sea wall** of
  fitted stone, 2 m from the slip's floor to its top at y 2.6, behind which is the **navigators' landing** where their
  stair starts. The raft in the slip is a canoe-deck platform, 3 by 8 m.
- *The tide (the last puzzle):* at low the wall is 2 m above the slip: shut. At high the raft floats at 2.3 from the
  west shore's edge to the wall's top: **the tide lifts the player over the wall** (Decided, read as 1.2). **T6** at
  (4, 2.6, 344): turn it high.
- *Enemies (at high):* reel-in point **R3**: two **hooked gaffmen** dragged up from the inlet onto the west shore at
  (0, 2.6, 330) and (8, 2.6, 334); and E3.5 a **net-drowned** in the raft slip, 3 m east of the raft at (9, 0.6, 356),
  casting at the player on the raft. On a 3 by 8 m float, a net is a long few seconds.
- *Never stranded:* the west shore is both-tide ground with T6 on it; at high its only way east (the head beach) is shut,
  but T6 is there to turn back.

#### 3F. The navigators' stair and the seat (x -36 to 12, z 360 to 394)
- *Shape:* from the landing (6, 2.6, 363), three flights of stairs climb the west headland's south face, running west
  and east alternately (each 4 m up over 8 m, 4 m wide, a rim on the south), with landings at y 6.6, y 10.6 and y 14.
  The y-14 landing at (8, 14, 368) is S3's west end. From it a ramp climbs north-west (6 m over 30 m, 11 degrees) to
  **the seat** (y 20, x -36 to 0, z 372 to 394): a bare top with a ring of sixteen star stones (radius 8 m, centre
  (-18, 20, 382)) and, on its north edge, the **hook arch** at (-18, 20, 394): two carved posts bent toward each other
  like a hook's curve. Through it, the cleft down to the beach (`FisherBeach`).
- *Enemies:* E3.6 an **old reef-grown** on the y-10.6 landing at (-4, 10.6, 366), the one that climbed highest out of
  the sea, slow and plated, on a landing 4 m deep with the stair below it. None on the seat.
- *Shortcut:* **S3**, the navigators' rope bridge. At the y-14 landing, a coil of plaited cord and plank treads; "Cast
  the bridge" throws it across the gorge (16 m, x 10 to 28, at y 14, running east-west) to the posts on the east
  headland's north-west corner. From F5: up the switchback, across the bridge, up the ramp to the seat, without the
  harbour's two turns of the tide.
- *Vista:* **V5** at (-18, 20, 390), 10 by 3 m, looking north: the storm filling the sky, rain curtains, and below the
  cliffs, the long black beach, the islets offshore and a small figure at the waterline. (In this scene the beach and
  sea are a stand-in, as Fire's `Beyond` is.)
- *Lore:* W8 at the star stones: the navigators' last word (Claude's, new): "We read the stars to go home." / "There
  is no home for a fish but the sea."

### 3.4 The reef road and the centre islet (side path, the trial, the great loop; low tide only)

**The idea.** Before the fisher, the lagoon's people walked between their islands at low tide on a road of reef. It
still runs across the lagoon's middle from the meeting ground to the canoe harbour, past the islet where the oldest of
the reef-grown waits and the wreck that holds the land's plain weapon. Seen first from Cut 2's mouth (V3); reached from
the village. At high it is all under water.

- *The way:* from the meeting ground's west stair (-24, 0.4, 100) north over the reef flat (y 0.3, coral heads, pools)
  by (-24, 0.3, 120) and (-18, 0.3, 150) to the centre islet's south ramp (-12, 0.3 to 2.6, z 158 to 162). Off the islet's
  north-east corner (2, 2.6, 184) a ramp down to the reef spine, by (6, 0.3, 190) and (20, 0.3, 206) to the wreck. From
  the wreck north by (28, 0.3, 240), (22, 0.3, 280) and (18, 0.3, 310) to the sand bar (z 318 to 324) and up the east
  shore's south stair at (30, 0.6 to 2.6, z 322 to 326), 6 m from F5. About 330 m in all.
- *Enemies:* a **bait** at (-22, 0.3, 128) (lure in a pool by the road, body on it); two **drowned** stranded on the reef
  by the islet's ramp; a **young reef-grown** in the deep channel by the wreck at (39, -1.5, 226), surging out onto the
  reef at the mast; a **bait** at (20, 0.3, 290).
- *The wreck:* two hulls 14 m long lying heeled on the reef (x 26 to 36, z 218 to 232), the deck platform between them
  tilted 10 degrees with its low corner at y 2.4; the fallen mast is a ramp from the reef at (24, 0.4, 220) to the
  deck at (28, 2.4, 223). On the deck at (31, 2.4, 225), **the plain Chain Sickle** (`WeaponPickup`): in this land's make,
  a hooked blade of basalt on a hardwood haft, on a plaited cord with a stone sinker (question 7). Its shell inlay
  flashes like a lure: that was the glint seen from V3.
- *Lore:* W9 on the wreck's sternpost: "The last canoe that tried to leave." / "He hooked it on the reef and let the tide
  do the rest."
- *The great loop:* F2 to F5 at low tide, or the other way. A player who dies at the harbour and holds low can walk
  back to the village by it. Never stranded: the islet's two stones stand halfway.

#### The trial: the Old Reef (the centre islet, x -26 to 4, z 160 to 186)

**Proposal** (question 5 offers two others). A **duel with a lone mini-elite in which the player owns the tide**, which
no other land's trial can be. The fisher takes the tide from the player in his own fight; here the player learns to use
it under pressure first.

- *The place:* a ring of rock 4 m wide (y 2.6) round a pool 18 by 16 m (x -20 to -2, z 166 to 182) whose floor is
  y 1.2: dry at low, wading at high. Four ramps 3 m wide down from the rim (north, south, east, west). Two tide stones on
  the rim, **T7** west at (-24, 2.6, 174) and **T8** east at (2, 2.6, 174), 26 m apart. A dead canoe-hull stands upright in
  the pool's middle as a pillar. A Memory lies under the coral at his feet (`WORLD.md`: "a Memory or a Body piece").
- *Him:* a reef-grown from before the fisher, so old that he is a coral head on legs, 3 m tall, five plates, a
  shark-tooth club in each hand. He wakes when the player steps down a ramp.
- *The rule of the fight:*
  - **At low** his coral is dry and hard: light blows glance off, heavies crack a plate in two. He is slow (1.8 m/s) and
    regrows nothing, but dry coral sheds: each plate cracked drops a spur that stands 6 s and cuts a player who rolls
    through it.
  - **At high** his coral is wet and soft: light blows break a plate in three. But he is in water: fast (surges of
    8 m), and he regrows a plate every 6 s; the player wades.
  - So: turn high to strip plates fast, standing on the rim (dry, where his surges stop at the ramps) and coming down
    only to strike; turn low to punish his bare body while he is slow. The stones are on opposite rims, the turn takes
    1.5 s of holding and a blow breaks it, and he follows. Three or four turns in a fight is the expected rhythm.
- *Reward:* the Memory. *Lore:* he wears a navigator's breast ornament under the coral, seen when the last plate breaks.

---

## 4. Shortcuts and loops

| | Opens from | To | How | Saves |
|---|---|---|---|---|
| S1 | The east decks' south end, (8, 2.6, 54) | The landing-stone platform, (10, 2.6, 50); F1 36 m on | A long plank stored on the deck, "Push the plank": it falls across the 4 m gap, one press, from the deck side only | F1 to the meeting ground along the decks at either tide, without the road, its bait and hooked, or a turn |
| S2 | Terrace 3's south-east corner, (82, 11, 237) | Terrace 2, (82, 8, 221); F3 by the hacked steps | "Cast the net bridge", from terrace 3 only | F3 to F4's ridge without Cut 2, its turn, the drowned and the reeled-in hooked |
| S3 | The navigators' y-14 landing, (8, 14, 368) | The east headland's north-west corner, (28, 14, 368); F5 by the switchback | "Cast the bridge", from the landing only | F5 to the seat without the harbour's two turns, the bait or R3 |
| Drop | Cut 2's north ledge, (60, 2.6, 234) | Cut 2's floor (1.4 m, facing south) | One way | A way back south at low |
| Loop | The meeting ground's west stair | The reef road, the islet, the wreck, the sand bar, F5 | Walking, at low | The side path is also the great loop; F2 and F5 are 330 m apart by it |
| Pond | The sluice, (24, 0.6, 120) | The pond's floor | "Lift the sluice" at low | Not a shortcut: the way to W3 |

S1 to S3, the sluice's lore and the tide stay as left after a fall and across sittings (progress keys, as Fire's S1 to
S4). Every shortcut works at both tides: it is the tide's way round, as Fire's were the sun's.

---

## 5. The brothers' Gash (the elite's arena)

**Decided** (`WORLD.md`): the deepest wound in the fished-up island, a raw gash with red, wet rock that still breathes,
their tools stuck in the walls; narrow, so it is hard to get out from between them; the sea god's remains seen where
they cut deep enough; the hook brother uses plainer versions of the fisher's hook moves; about weapon play rather than
spectacle. The hook brother teaches Hook and Hold (Chain Sickle) and the blade brother Fillet (Twin Knives)
(`steel/weapons.md`, 2026-11-18).

**Shape.** A cleft running north-south into the fish's head, open to the sky, its floor at y 6, 8 m below the ridge.
From F4 a stair goes down north (x 88 to 96, z 266 to 280, 8 m over 14 m) into it; the arena runs from z 280 to 310,
**8 m wide** (x 88 to 96), walls of red wet rock rising 8 to 10 m east and west, with two shallow **bays**, 3 m deep and
6 m long, one in the west wall (x 85 to 88, z 286 to 292) and one in the east (x 96 to 99, z 298 to 304), staggered so
that no line across the room is wider than 11 m. At the north end, **the window**: where they cut deepest the floor is a
glassy pane of the sea god's flesh and bone 3 m across at (92, 6, 306), and under it, far down in dark water, an eye the
size of a house that does not move (walkable, solid). Beyond it the **throat** stair climbs north (z 310 to 326, 8 m
over 16 m) to the fish's head and the jaw bridge.

```
 z    x: 85   88       92       96  99
326        ## throat stair up to y14 ##
310   +----+----------------------+----+
306   |    |      ( window )      |    |
304   |    |                      |bay |  <- tools: great saw, adze
298   |    |        (H)           |E   |
292   |bay |                      |    |
286   |W   |        (B)           |    |
280   +----+---- stair down from F4 ---+
266              F4*(88,14,262)
```
`(H)` the hook brother at (92, 6, 300); `(B)` the blade brother at (91, 6, 288).

**How it plays with their moves.**
- *Narrow by design.* 8 m is wider than Fire's one-on-one corridors (3.5 to 5 m) because there are two of them, and
  narrower than a group fight (12 m), so the player cannot circle both. The blade brother starts south, between the
  player and the room; the hook brother north, with the whole length for his cast (a 12 m lane along the Gash). The
  first frame on the stair shows both.
- *The hook pins, the blade carves* (Proposal, `WORLD.md`). The hook brother's cast hooks and reels; his Hook and Hold
  pins a foot to the floor for 1.5 s with the line staked; that is when his brother comes in with Fillet's carving pass.
  Answering the pin: guard the carving (it is pale), or cut the staked line (it is a line, as the hooked's are) to free
  the foot.
- *The tools in the walls* (Decided: "their tools stuck in the walls") are **anchors**: a great shark-tooth saw and an
  adze in the east bay, two adzes in the west bay, a chisel by the window. The hook brother casts round one to swing
  across the room behind the player (as the Chain Sickle's Round the Post, the line wrapping the haft); a player with
  the Chain Sickle can do the same. The bays are the only places out of the line between them, for a few seconds.
- *Kill one first* (Proposal, `WORLD.md`): the other takes the dead brother's weapon in his off hand and changes style.
  Killing the hook brother first leaves a blade brother who throws a hook to drag the player in for every carving pass;
  killing the blade brother first leaves a hook brother who carves from the end of his line. The room is the same
  either way; the bays matter more in the second half.
- *No tide here.* The Gash is the island's inside, above the water: its breath is look only (the walls swell and
  settle; a warm mist lies on the floor near the window). Weapon play is the whole fight.
- *Their ground:* `HoldGround(88, 96, 280, 310)`. Beaten, both lie at the window, and the throat is open.

---

## 6. The approach to the god's beach, and the canoe (`FisherBeach`, its own coordinates)

The fight's design belongs to a later `water_land.md`; this section gives its room and approach, as `fire_levels.md`
section 3.5 does for the sun-court.

**The cleft (the god's door).** Through the hook arch the player comes out at the head of a cleft in the cliffs at
(0, 10, -56) in the beach scene's coordinates: a cut 6 m wide between walls of wet basalt east and west, descending north
in two flights of stairs (each 4.4 m over 9 m) with a landing 12 m long between, to the beach at (0, 1.2, -18). Rain
drives across it; the sea's noise grows with each step.
- **F6** at the cleft's head, (-2, 10, -52), in the lee of the east wall: the god's door (Decided). After a fall the
  player wakes here and walks the cleft again, about 15 s.
- No enemies in the cleft. The hooked's lines, which the player has seen running off north all land, now run past them
  down the cleft and out over the beach to the sea: here they end.
- **V6** at the cleft's mouth, (0, 1.2, -18), 8 by 4 m, looking north: the beach, the islets offshore, the fisher
  standing at the waterline with his back to the player, casting.

**The beach.** Black sand from the cliff foot (z -16) to the waterline (z 16 at the fight's start), x -36 to 36. The
cliff foot is a slope of fallen rock (at most 45 degrees), not a wall, so the south-side rule holds. Five **islets** stand
offshore at z 20 to 36: arena pieces for both sides (Decided: he hooks them to swing himself about; the player hides
behind them; the octopus drags them under in phase 2). A line of the fisher's lines, staked across the beach at z -10,
is this fight's threshold: crossing it begins the duel. A tide stone stands by the stakes **broken in half**: on this
beach the tide is his. Through phase 1 he pulls it in (Decided), and the waterline comes up the beach in three steps,
from z 16 to z 2.

**The canoe** (Decided: his own, waiting on the beach). On a rock shelf at the beach's west end (x -24 to -12, z -14 to
-4, y 3), behind the stakes and outside his ground, his voyaging canoe rests on log rollers in an open shed whose thatch
fades, its prow toward the sea, its crab-claw sail furled. It is in V6's frame on arrival: the first thing the player
sees after the fisher. During the fight nothing reaches it: his swells and surges stop at the stakes. When he falls
and the choice is made, he turns his head toward it; the prompt is "Take the canoe"; it runs down the rollers into the
water as the storm breaks (`LOOKS.md`).

**His ground:** `HoldGround(-8, 36, -10, 36)`: the beach east of the shelf and the shallows to the islets.

---

## 7. Build list

New pieces are made in code by a `LagoonBuilder` (menu `Clover/Build/Lagoon`) and a `FisherBeachBuilder`, as the Fire
builders are: static meshes with `MeshCollider`s, ink outlines where the toon material carries one. Each piece names
its parameters; sizes are the defaults the plan uses.

### 7.1 New geometry

| | Piece | What it is | Dimensions and parameters |
|---|---|---|---|
| W1 | **Lagoon water** | One water surface for the whole lagoon and the sea at y 0 or y 2.0, moving between them over 6 s; a foam line where it meets the floor; depth tint from turquoise to ink by 2 m, so shapes on the floor show to about 1.5 m | `TideWater(lowY 0, highY 2, seconds 6)`. Separate held bodies: the fishpond (y 0 at low, joins the lagoon at high), the gill pool, the islet's pool |
| W2 | **Tide stone** | A basalt upright 2.2 m tall on a socket, a carved face with an ebbing spiral on one side and a flowing one on the other; held Use 1.5 s turns it a quarter round and turns the tide | T1 to T8 (section 2) |
| W3 | **Tide band and tide post** | A decal band of weed and barnacle from y 0 to y 2.0 on every post, wall and face below 2.2; a carved post at the mouth of every one-tide route | Posts at T1's north stair, the causeway's ends, Cut 1, Gill 1's stair, Cut 2's steps, the head beach's stair, the raft slip, the reef road's ends |
| W4 | **Float** | A tethered body whose deck sits at water + 0.3 when afloat and floor + 0.3 when resting; rises and settles with W1; carries the player and enemies standing on it | Raft 3 by 4 m (the west decks' gaps, x3); pontoon 3 by 10 m (Cut 1); hull 2.5 by 9 m (Cut 2); canoe-deck raft 3 by 8 m (the raft slip) |
| W5 | **Fishpond wall (causeway)** | A curved wall of dry-laid stones, top 4 m wide, faces battered; a sluice in it; a hut on a stand | Section 3.1, 1F. Built on `Ramp` and `Plateau` (Fire's B1, B3) along a curve |
| W6 | **Stilt house and deck** | A house body on posts with a steep thatch roof that fades; a deck at y 2.6 beside it with a 0.6 m rail; the mud under both closed by a stilt-and-fish-trap screen | Eight in the village; planks between the east row's decks |
| W7 | **Benched cut** | A trench of any outline: south side in benches 0.7 m high and 0.8 m deep, north face sheer red wet rock, a sloping floor, hacked steps (0.22 m) where named | Cut 1 (12 m wide), Cut 2 (14 m wide, with its ledge) |
| W8 | **Gill cleft** | An open cleft with a sheer north wall, a ribbed south slope (at most 45 degrees), a rising floor, a breath emitter (mist out for 1.5 s every 8 s, a push of 2 m out and 1 m in) | Gill 1 |
| W9 | **Scale terrace** | A `Plateau` face patterned as overlapping scales, wet sheen, seaweed tufts on its lip | Terrace 1 to 3 and the flank's scale wall |
| W10 | **The Gash** | The elite's room: a north-south cleft with bays, tools set in the walls as anchors (tagged for the Chain Sickle), the window of bone and the eye under it | Section 5 |
| W11 | **Cast bridge** | An interactable lying coiled on one side: "Cast the bridge" throws it across and it stays (progress key). A walkable span of cord and treads with rope rails, sagging slightly | S2 (net, 16 m, falling 3 m), S3 (rope, 16 m, level) |
| W12 | **Push plank** | A long plank on a deck: "Push the plank" drops it across a gap, one way, then it stays | S1 |
| W13 | **Sea wall and raft slip** | A stone wall 2 m tall standing on low ground, the landing behind it at its top | The navigators' landing |
| W14 | **Jaw bridge** | A natural arch 4 m wide, 20 m long, with a 0.6 m rock rim, over a channel; a sea stack beside it | Section 3.3, 3A |
| W15 | **Canoe shed and slipway** | A long open shed on posts with a fading thatch roof; log slipways down into water | Harbour east shore (2 sheds, 3 slipways); the beach shelf |
| W16 | **Wreck** | The double-hulled canoe lying heeled on the reef, its deck tilted, its mast fallen as a ramp | The reef road |
| W17 | **Hook stones, star stones, hook arch** | Carved basalt uprights with hook-shaped heads; a ring of sixteen; the arch of two bent posts | The east headland (8), the seat (16), the arch |
| W18 | **Canoes in trees** | A canoe lodged in palms at 8 m | The ridge (2) |
| W19 | **Vistas with a direction** | Fire's vista (B13, now in `LandZones`) with the look-ahead pointed any way, not only north | V1, V2 (north-east), V3 (west), V4, V5, V6; V0 is the first turn's |
| W20 | **Shores, reef and beds** | `Plateau` and `Bank` (Fire's B1, B2) with wet black sand, mud and reef materials; coral heads; tide pools | Everywhere |

### 7.2 New behaviour the rooms rely on (for the code worker)

| | What | Where it is used |
|---|---|---|
| C1 | **TideState**: low or high, one for the scene; events on turning; saved as `water.tide`; reel-in points tied to a stone, firing once per rest when it turns high | Every stone |
| C2 | **Depth query**: depth of water at a point for the present tide (the W1 surface less the floor), used by the player, enemies and floats | Everything below |
| C3 | **Wading**: 4 m/s, roll 3 m, splash | Cut 2's floor at high; the islet's pool at high |
| C4 | **The throw-back**: deep water returns the player to `FallCatch.LastFirm` with 10 damage after 1.2 s; enemies in deep water are taken (killed) unless drowned or reef-grown | Every edge over deep water |
| C5 | **Moving floors**: the player and enemies ride floats (the `CharacterController` carried by the deck it stands on) | W4 |
| C6 | **The drowned**: walk only in water at least 0.6 m deep, straight on the bottom; the grab and drag at edges; stranded crawl at low; rise again at high | D1, D2, D3, the reef road |
| C7 | **The net-drowned**: the net cast and cutting free | Causeway, raft slip |
| C8 | **The bait**: buried body and lure; the jaws ring; the lunge when the lure is struck; eats enemies near its body; dormant at high | Village road, flank, Cut 2's mouth, head beach, reef road |
| C9 | **The hooked**: line-driven yanks with the taut tell; cutting the line; gaffman's drag; harpooner's throw and reel, standing in stones and posts | Everywhere |
| C10 | **The reef-grown**: plates broken by heavies; fast and healing in water, slow on land; the shark-tooth bleed | Channels, pond, gill, terraces, stair |
| C11 | **The Old Reef**: the trial's rule (dry hard coral at low, soft at high) | The islet |
| C12 | **The brothers**: their moves, the pin and carve, the weapon taken in the off hand; anchors in the walls | The Gash |
| C13 | **Breath**: the gill's mist and push; the island's floor drawn rising and falling | Gill 1, the hollow, the Gash |

### 7.3 What is reused

| Piece or code | Where |
|---|---|
| `FallCatch` | Under the whole scene, and as the throw-back's "last firm ground" |
| `Ladder`, `BarredDoor` | Not needed by the main plan (floats and cast bridges take their place); available for dressing |
| `LandZones` (vistas), `TopDownCamera.SetFraming` | V0 to V6, with W19's direction |
| `Fadeable`, `OcclusionFader` | Every roof, shed and great house |
| `RestPoint` | F1 to F6, in fire rings |
| `WeaponPickup` | The plain Chain Sickle on the wreck |
| `LandVoice` progress keys | S1 to S3, the sluice, the tide |
| `Enemy.MakeDormant`, `Enemy.HoldGround` | The drowned standing under water at T1 before they notice; the brothers' and the fisher's grounds |
| `DistrictMood`, `CirclingBirds` | The storm nearing by district; frigate birds overhead |
| `SceneDoor` | The hook arch to `FisherBeach` |
| `SunCityBuilder` helpers | `Floor`, `Box`, `Stairs`, `Disc`, `Scatter`, `Rubble`, `Carving`, `Stain`, `Soft`, `Rest` |
| Fire's B1 `Plateau`, B2 `Bank`, B3 `Ramp` | Every terrace, shore, causeway and ramp |

### 7.4 New kit (Blender, through the `blender-prop` skill)

Palms and pandanus; stilt house and deck; great house; carved posts (vague, respectful); fire ring; tide stone;
outrigger (broken) and hung canoes; raft, pontoon, hull, canoe-deck raft; voyaging double canoe (whole, half-built and
wrecked); canoe shed; sluice and watch hut; fish traps and nets; adzes, saw and chisel for the Gash; hook stones, star
stones and the hook arch; coral heads; red cut-rock blocks. None of the Fire kit carries over except as stand-ins for
grey-box.

---

## 8. Pacing

Minutes for a new player on a clean pass, counted as `fire_land.md` 3.1 counts them (a weak enemy about 30 s, a strong
one 45 s, a group a little less each), with each turn of the tide and its 6 s counted in.

| Stretch | Minutes (new) | Experienced | Tension | What it teaches |
|---|---|---|---|---|
| The landing, the ridge, the first hooked | 1.5 | 0.7 | Rest, rising | The hooked: yanks along the line, cutting it |
| The landing stone and the first turn | 1.5 | 0.6 | Dread, then wonder | The drowned at an edge; **the turn**; stranded drowned |
| The village road | 2.5 | 1.0 | Rising | The bait: read the mud, not the voice; a hooked eaten |
| The meeting ground, F2 | 1 | 0.5 | Small peak, rest | The reef-grown: slow on land, whole in water |
| The causeway | 2 | 0.8 | Peak | A net and a club on a 4 m wall; step off onto the reef |
| **District 1** | **8.5** | **3.6** | | |
| The tail landing and the flank shore | 1.5 | 0.7 | Rising | The island breathes; a bait from the path |
| Cut 1 | 2 | 0.8 | Peak (small) | The harpoon, dodged twice; floats |
| Gill 1 | 1.5 | 0.6 | Rising | The breath and the pool |
| The hollow, F3, the turn | 0.5 | 0.3 | Rest | Choosing a tide for a place you have seen |
| Cut 2 | 2.5 | 1.0 | Peak | The tide as a puzzle; a reel-in; a fight off a float |
| Terrace 3, the ridge, F4 | 1.5 | 0.6 | Rising, vista | Plates on land; S2 |
| The brothers | 4 | 2 | Peak | Pin and carve; the hook's plain moves before the god's |
| **District 2** | **13.5** | **6.0** | | |
| The jaw bridge | 0.8 | 0.4 | Spike | A harpoon over a drop |
| The east headland | 1.5 | 0.7 | Rising | Stones against harpoons |
| The harbour at high, F5, the turn | 1.2 | 0.5 | Rest | |
| The head beach crossing | 1.5 | 0.6 | Peak (tense) | Two bait at once |
| The west shore, the raft, the wall | 1.5 | 0.6 | Peak | The tide lifts you over; a net on a float |
| The navigators' stair and the seat | 1.2 | 0.5 | Rising, then vista | The storm and the beach seen |
| **District 3** | **7.7** | **3.3** | | |
| The cleft, F6 | 0.5 | 0.3 | Dread | |
| The fisher (both phases and their set pieces) | 10 | 7 | Climax | (`water_land.md`, later) |
| **Clean pass** | **40.2** | **20.2** | | |
| Side paths: west decks 1.5, drained pond 1, reef road and wreck 4, the trial 4 | + 10.5 | + 5 | | |

That sits on the 40 of `WORLD.md`'s proposal. Fire's plan came to 41. Levers if play runs long, in order: R1's hooked
are already off the main path; take E2.1 to one hooked (0.3), drop the gaffman from E2.7 (0.5), drop the harpooner on
the jaw bridge's stack (0.5).

**Fires apart (new player's minutes).** F1 to F2 about 7; F2 to F3 about 6; F3 to F4 about 4.5; F4 to F5 about 7.3 with
the brothers; F5 to F6 about 5. Within the decided 6 to 8, the third a little short because the Gash's door wants a
fire of its own.

**Enemy count.**

| District | Main path | Count |
|---|---|---|
| D1 | 3 hooked, 2 drowned, 1 net-drowned, 1 bait, 2 reef-grown | 9 |
| D2 | 7 hooked (2 reeled in at Cut 2), 2 drowned, 1 bait, 3 reef-grown | 13 |
| D3 | 5 hooked (2 reeled in), 2 drowned, 1 net-drowned, 2 bait, 1 reef-grown | 11 |
| Elite | The brothers | 2 |
| Side paths | West decks 2 drowned; Cut 2's mouth 1 bait; reef road 2 bait, 2 drowned, 1 reef-grown; R1 2 hooked if turned; the Old Reef | 10 and the Old Reef |
| **All** | | **33 on the main path, the brothers, and the fisher** (Fire: 40) |

Fewer than Fire's because the tide is the land's other opponent: four turns, each with its own cost.

---

## 9. Questions for the owner

**Answered (owner, 2026-11-18): the recommended option (a) in all seven.**
1. No swimming.
2. The stones read by marks in the world.
3. The tide holds on a fall.
4. Turning the tide wakes the land's people.
5. The trial is the Old Reef duel.
6. Three special camera shots.
7. The Chain Sickle in the land's own make, on the wreck.
The rest of the plan stays Proposal (Claude) until the owner walks it.

Each has two or three options, the recommended one first.

1. **How does the player meet deep water?**
   - **(a) No swimming: floats carry the player; deep water throws them back to firm ground with a little damage.**
     Recommended. Fights stay on footing, which is the game; FromSoftware's Souls games and Elden Ring have no swimming
     at all and treat deep water as a boundary, and Sekiro's swimming is a traversal mode with no combat. The decided
     words "floats the player over walls" are kept literally.
   - (b) The player swims in deep water: slow, no attacks or guard, climbs out at any edge. More freedom, but at high
     tide the whole lagoon becomes one route, so every tide puzzle needs a fence; and the drowned would be fought
     helpless.
   - (c) Wading at every depth, only slower. Simple, but the tide stops shutting anything.
2. **How do the tide stones read, and how does the player know what a turn will open?**
   - **(a) A carved basalt stone turned on its socket, and the land's own tide marks as the guide: a weed and barnacle
     band to the high-water line on every post and wall, and a carved tide post at the mouth of every one-tide route.**
     Recommended. The Water Temple of *Ocarina of Time* was the series' most criticised dungeon until the 3DS version
     added coloured lines leading to each water-level switch; this does the same job inside the world.
   - (b) The same stones, plus a mark on the HUD or map showing which routes are open at the present tide. Clearer,
     less of a place.
   - (c) Conch trumpets blown instead of stones, the tide led by sound. Fits the islands; harder to read from above.
3. **What happens to the tide on a fall?**
   - **(a) It holds: the world keeps the tide the player left, as Dark Souls keeps its levers, lifts and doors; every
     fire has a stone it reaches at either tide.** Recommended: the player's choices stay theirs, and the
     never-stranded rule (1.3) makes it safe.
   - (b) Each fire puts back the tide it was lit at. Tidier after a death; the player's last turn is undone.
   - (c) Always back to high, the fisher's tide. Strong for the story; tedious, because the main path asks for low
     three times.
4. **Does turning the tide cost something?**
   - **(a) Yes: high raises the drowned and fires that stone's reel-in point (its hooked dragged in, once per rest);
     low wakes the bait.** Recommended. A turn is then a choice with weight, not a door handle, and the cost is the
     land's own people. Skyward Sword's timeshift stones work the same way: the shifted past brings its machines, and
     the remains of enemies, back to life.
   - (b) Turns are free; the tide is a pure puzzle. Cleaner; the people and the tide stop talking to each other.
   - (c) Each turn costs a flask or health. Simple and harsh, and it punishes experiment.
5. **What is the land's trial?**
   - **(a) The Old Reef: a duel with one ancient reef-grown on the centre islet, with a tide stone on each rim; dry,
     he is slow and hard; wet, he is fast and soft.** Recommended. It is the only trial that could be in no other land,
     and it teaches the verb under pressure just before the fisher takes the tide away. Elden Ring's evergaols show a
     lone foe in a small marked arena working well as optional content.
   - (b) The Rising Ring: survive waves of the land's people on the islet while the water rises in three steps and the
     floor shrinks, as Hollow Knight's Colosseum changes its arena between waves. Strong, but it spends the god's own
     set piece (the shrinking beach) before the god.
   - (c) The Calling Flats: a reef at low tide full of calling lures, one of them a living person; find them and lead
     them out. The most original, but an escort is a new system and hard to make fair from above.
6. **Which moments get the land's special camera?**
   - **(a) Three: the first turn at T1 (the whole village draining in one shot), the eye under the Gash's window, and
     the storm and the beach from the navigators' seat.** Recommended. One per district; the rest are plain vistas, so
     the three stay special.
   - (b) Those, plus the canoes in the trees and the jaw bridge. Richer; the camera starts to feel like a tour.
   - (c) Plain vistas only, no held shots. Cheapest; the first turn loses its wonder.
7. **The land's plain weapon: the plain Chain Sickle, on the wreck by the reef road.**
   - **(a) There, in this land's make (a basalt hooked blade, a plaited cord, a stone sinker), playing exactly as the
     plain Chain Sickle.** Recommended: a chain would look foreign in the lagoon; the look changes, not the weapon.
   - (b) There, in the Chain Sickle's standard look. Less modelling; one odd object in the land.
   - (c) Dropped by the hook brother instead, on the main path. Sure to be found; the land's "see it, then find the
     way" loses its best prize.

---

## Sources

- The Ocarina of Time 3D Water Temple's coloured guide lines: [Zelda Universe](https://zeldauniverse.net/2011/05/14/ocarina-of-time-3ds-improved-water-temple/),
  [Zelda Dungeon](https://www.zeldadungeon.net/ocarina_of_time_3d_changes_to_the_water_temple_and_items/).
- Skyward Sword's timeshift stones, which revive an area's past and its machines and enemies around the stone:
  [Zelda Dungeon wiki](https://www.zeldadungeon.net/wiki/Timeshift_Stone).
- Soulslike loops and shortcuts opened from the far side: [The Level Design Book, Undead Burg](https://book.leveldesignbook.com/studies/sp/undead-burg),
  [Socratopia, chapter 15](https://www.socratopia.app/library/game-design-compelling-en/chapter-15).
- Optional arenas: [Hollow Knight's Colosseum of Fools](https://hollowknight.fandom.com/wiki/Colosseum_of_Fools),
  [Elden Ring's evergaols](https://www.digitaltrends.com/gaming/elden-ring-what-are-evergaols/).
- Hawaiian walled shore fishponds (loko kuapā), their dry-laid walls and tide-worked sluice gates, the model for the
  causeway: [NPS, Kaloko Fishpond](https://www.nps.gov/places/kaloko-fishpond.htm),
  [Hawaii History, shore ponds](http://www.hawaiihistory.org/index.cfm?fuseaction=ig.page&PageID=516).
- An island game made with the culture it draws on: [Tchia, Unreal Engine interview](https://www.unrealengine.com/developer-interviews/tchia-is-a-beautiful-ambitious-undertaking-from-a-small-indie-studio?lang=en-US),
  [ALA Games](https://games.ala.org/aapi-month-featured-video-game-tchia-2023/).
