# The Metal land's levels (first pass)

Written by Claude on 2026-11-18 as the room-by-room plan for the lame smith's island, in the form of
`fire_levels.md`. **Everything here is Proposal (Claude)** unless it is marked **Decided (owner)** with its source.
The owner's points come from `WORLD.md` ("How a climb is structured", "Level design rules for every land (decided
2026-11-18, evening)" and "Metal's land: the lame smith") and from `LOOKS.md` section 5. The enemies, the elite and
the god are specified here only as far as the rooms need them; their full sheets belong in a `metal_land.md` written
the way `fire_land.md` is.

Axes as in Fire: x east, z north, y up; north is up the screen. Coordinates are metres. "Floor y 4" is the walking
height. The island's centre, the anvil in the forge room, is (0, 0, 0). The player wakes at (88, -6, -169).

---

## 0. What the owner has decided for this land

| Decided (owner) | Source |
|---|---|
| The map is a ring road along the giant's round, with the labyrinth in the middle | `WORLD.md`, level design rules |
| Rest fires in Fire's rhythm: about one every 6 to 8 minutes of a new player's walk, always one at the god's door, a shortcut loop per district back to its fire, 5 to 7 fires | same |
| Two kinds of optional discovery: "see it, then find the way" (the main kind, holding the plain weapon and lore), and one optional trial, different in every land, guarding a Memory or a Body piece | same |
| Up to three walkable levels per district, joined by ramps, stairs and ladders seen from above; nothing walkable under a roof, roofs and overhangs only fade | same |
| Metal's verb is **watch**: the giant is always in sight, and the player times a crossing by where it is | same |
| The labyrinth shifts on the giant's footsteps: each time its round passes overhead the tread shakes the maze into its next layout; a few fixed layouts, all shown on a bronze map plaque in each hub room | same |
| The land's rule is that the giant walks the island, plus traps everywhere | `WORLD.md`, Metal's land |
| Three kinds of corruption: the automata, the bronze-veined, the chained | same |
| The land nods to Daedalus and the labyrinth | same |
| The elite is the first golden handmaiden, who starts to parry an attack the player keeps using | same |
| The god's arena is a forge room at the labyrinth's centre; wounded, the smith calls the giant, which crashes through the roof, and he gets inside it | same |
| The giant is a new, detailed Greek design; beaten, it explodes | same |
| The ride out is a bronze chariot pulled by bronze horses | same, and the rides table |
| Metal is in tier 2 with Water; other early solo lands are about 40 minutes for a new player's clean pass (Proposal in `WORLD.md`, used here as the target) | `WORLD.md`, the climb |
| The look: volcanic dusk, soot-grey marble, verdigris, molten bronze as the strong colour, Aegean blue as the rare accent, straight lines and Greek key; the separation from Fire is enough | `LOOKS.md` section 5 |

---

## 1. Principles

### 1.1 What the camera sees: what stays, what changes

Section 1.1 of `fire_levels.md` holds here unchanged: pitch 55 degrees, distance 12 m, the picture runs from 4.3 m
south of the player to **7.2 m north**, 15.5 m wide at the player. Nothing beyond about 7 m north is ever on screen;
rises face south toward the camera; the south-side rule (nothing more than 1.4 m above a floor within 4 m south of it
unless it fades); roofs fade or are shadow-only; enemies walk straight at the player with no navmesh; ranged attackers
stay in frame. What this land changes:

1. **The main path climbs north wherever it can.** The quarries and the forge both rise northward in terraces whose
   faces look south. The labyrinth cannot: a tour of a maze turns every way. So in the labyrinth no southward leg of the
   main path is longer than about 18 m between turns, and **no fight is placed where an enemy comes at a player who
   is walking south**. Enemies in south-going legs stand at the turns.
2. **The labyrinth's walls are 3.5 m high, so they break the south-side rule by design.** A wall south of the player
   within 5 m is drawn **cut away**: everything above 1.2 m dithers out and its top edge stays as an ink line in the
   air, so the wall still reads as a wall (the "walls cutaway" of isometric games: The Sims, the Diablo series). The
   south rampart's inner face fades whole when the player is within 8 m north of it. See 4.4 for how the maze reads.
3. **The giant is 20 m tall and never fits the picture.** Something H metres up is in frame only within (5.0 - H) / 0.7
   m north: the giant's knees are never on screen, only its feet when they come down close. When it stands between
   the camera and the player (within about 20 m south), it dithers out above its shins, as a roof fades. How it is
   nevertheless "always in sight" is section 1.2.
4. **The light is dusk, low and from the sea** (`LOOKS.md`). The key light comes from the south-west at about 30
   degrees, so shadows are long (1.7 times a thing's height) and point north-east, away from the camera. There is no
   heat or shade rule here; the land's own light-and-shadow rule is the giant's gaze (1.2).
5. **One scene, not two.** Fire split the god's court into its own scene. Here the god's second phase spills out into
   the labyrinth and the giant of the fight is the giant of the round, so the whole island is one scene (working name
   `BronzeIsle`, built by `BronzeIsleBuilder`, menu `Clover/Build/Bronze Isle`).

### 1.2 The giant is always in sight (how, concretely)

The owner's "always in sight" cannot mean seeing it far off. It is solved with seven tells, from far to near. Every
point of the main path outside a fight has at least three of them in the picture or in the ear.

| Tell | Range | What the player gets |
|---|---|---|
| **Footfalls** | The whole island | A deep bronze thud every 2.4 s, placed in stereo and louder as it nears. The land's metronome: the player always knows it is walking and roughly where. |
| **The anvil** | The whole island, once a lap | Three rings from the heart, then three stamps on the Tread (section 4). The only sound that means "the walls are about to move". |
| **The watchdogs** | Wherever one is in frame | Bronze hounds on 1.2 m plinths (a nod to the gold and silver dogs Hephaestus made to guard Alcinous's palace in the *Odyssey*). Their heads always turn toward the giant; within 40 m of it they stand and their eyes glow. One stands within 8 m of every point of the main path, so one is nearly always in the picture: a compass to the giant that needs no HUD. |
| **The tremor** | Within 30 m of a footfall | A small camera shake (off if the player turns shake off), ash shivering off wall tops and parapets, rings on slag puddles. |
| **The gaze** | Within 40 m | A warm red-gold light from its visor, a 40 degree wedge sweeping across its front. It throws real shadows, and what it lights is what it sees (1.3). The first thing of the giant the player meets is its light sliding across the floor. |
| **The shadow and the feet** | Within about 20 m | Its long dusk shadow sliding over whole rooms; on the Round, the oval prints where its feet will come down. |
| **Vista zones** | Six chosen places | The camera eases back and turns to frame the giant or what it overlooks (section 10). V1 is the reveal. |

### 1.3 The gaze, the throw, and what counts as cover

The giant is not hunting the player; it is a clock to learn (`WORLD.md`, Proposal). It looks, and it throws at what it
sees.

- **The light.** A spot light from 18 m above its feet (y 26 on the Round), reaching 40 m, 40 degrees wide, sweeping
  180 degrees across its front, from inward over the maze to outward over the land, every 8 s. The same point is the
  origin of the "seen" ray, so **what is lit is exactly what is seen**: a player standing in a wall's shadow from that
  light is hidden, and can see that they are.
- **Seen.** Lit for 0.5 s without a break, the player is seen: a struck bronze note, the sweep stops, and the light
  narrows to a 4 m spot on the player for 1.0 s. Then it throws.
- **The throw.** A 2.5 m stone. Its shadow grows on the floor inside a 3 m ring for 1.6 s and it lands where the player
  was 0.6 s into the flight, so two steps or a roll clear it. To the player: 35 damage and a 1 s knockdown. To enemies
  in the ring: 60 and a stagger, so a throw can be drawn onto them. Seen to landing: 3.1 s. One throw at most every
  9 s.
- **It throws only what it carries.** Three stones, taken from a **cradle** at three stations of its round (3.2).
- **Where stones end up.** On open ground (quarries, forge, field) a stone stays as cover until the player rests,
  six at most (the oldest crumbles). In the labyrinth it shatters into walkable rubble, so it never blocks a way.
- **What it targets.** The player; otherwise a chained caught moving in its light. The chained know this and freeze
  against the nearest face when the light comes, which is how the land teaches the rule before it is ever spoken.
- **Lee.** A wall h high throws a shadow from the gaze about h x d / (26 - h) long at d metres from the giant's line.
  For a 3.5 m maze wall that is 0.16 x d: 2 m deep at 13 m, 4 m at 25 m, the whole 5 m corridor beyond 30 m. Two rules
  of thumb fall out, and the rooms are built on them:
  - **Under it is safe.** The rampart's own inner face shades the outermost corridor when the giant is directly above.
  - **The danger is the middle distance and the long view.** Corridors 10 to 25 m from its line have only a strip of
    lee along the wall nearer the giant, and a corridor that points at it has none at all.
- **No throws** within 4 m of a rest fire, into the elite's hall, or during the Tread ritual.

### 1.4 Measures (what is new; Fire's walk, roll, rises, steps, stairs and ramps stand)

| What | Measure | Why |
|---|---|---|
| Labyrinth corridor | 5 m between walls 1 m thick (a 6 m module) | Fire's one-on-one corridor (3.5 to 5 m); a third of the picture's width, so at most three corridors side by side are ever on screen |
| Labyrinth wall | 3.5 m high, top 1 m wide, pale marble with a Greek key inlay | From 12 m above, the tops read as bright printed lines; 3.5 m is enough lee beyond 20 m (1.3) |
| Leaf (a wall that moves) | 5 m long, 3.5 m high, 0.6 m thick, verdigris bronze, rivets, a bright top edge; sinks into a bronze-lined groove 0.8 m across | Told apart from fixed marble at a glance; the groove shows every place a leaf can stand |
| Rampart | 8 m above the maze floor, 14 m thick; the Round on top 12 m wide between 0.6 m parapets | The giant's 4 m feet on a road three times as wide |
| Rails | Bronze strips 1.2 m apart set in the floor | Every cart and every automaton runs on rails, so they need no pathfinding in a maze, and their routes read from above (`LOOKS.md`: "the floors carry the routes") |
| A chained's reach | Chain 6 m: a 12 m circle round its cart | Fits a group fight space; across a 5 m corridor it is a wall |
| Giant | 20 m tall, eye at 18 m, 3.1 m/s, a step every 2.4 s, 7.4 m stride | Slow enough to outrun (6 m/s), fast enough that a side of the Round is never safe for long |
| Footprint on the Round | A 4 m oval shown 0.8 s before the foot lands; under it: 60 damage, knocked off the road | Only ever met on the Round |
| Lap | 150 s; one layout change per lap | Three layouts make a 7.5 minute cycle |
| Exposure of a crossing | At most 15 m of open ground between lee on the main path | 2.5 s of running; the gaze needs 0.5 s to see and 3.1 s more to land |

---

## 2. The whole land

The island is a square of soot-grey marble on a sea cliff. A rampart 84 m square on the inside walls in the labyrinth,
and on top of the rampart runs **the Round**, the giant's road. Outside the Round, three belts of land: the
**quarries** on the east, climbing north from the harbour; the **volcano forge** on the south, climbing north from the
sea to the rampart's foot; the **slag marsh of the bronze birds** on the west (the trial). The north is sea cliff. The main
path is a spiral: up the east, in through the labyrinth from the east, round it to the south-west, out over the Round
and down to the forge's foot, up the forge, and back in from the south along the cooled bronze channel to the heart.
About 720 m of walking (Fire: 560; `WORLD.md` lets later lands grow a little).

```
  z     x: -104       -56 -42            0             42 56                      115
  62    sea cliff      ###### THE ROUND, north side y8  N cradle (0,8,49) ######      sea cliff
  42                   #+-----------------[ H2 COURT OF THE THRONE ]-----------+#
                       #|                    F3          y0                     |#
  24                   #|                                                       |#
        FIELD OF THE   #|[ H3 COURT ]         [ HEART:       ]   [ H1 COURT OF ]|#  CRADLE YARD y4  F2
        BRONZE BIRDS   #|[ OF THE   ]         [ FORGE ROOM r11]  [ THE DIAL    ]|#  E cradle (60,4,-30)
   0    y4 (trial)TREAD#|[ TREAD    ]         [ anvil (0,0,0)]   [  <-East Stair|#  Yard Stair up
        <- sally stair #|                         |PW|                          |#
 -27                   #|[STAIR COURT]-Slag Stair->|  |   (the Dead Quarter)     |#  face 4 m z-62
 -42                   #+--------------------------|  |-------------------------+#
 -49                   ###### THE ROUND, south side  culvert  S cradle ######      UPPER TERRACES y0
 -56   SLAG ROAD       CASTING FLOOR y2   crucible(0,2,-64)  [HALL OF THE           face 6 m z-104
       4 legs y8->-6   face 4 m z-78   West Run x-14  East Run x14  HANDMAIDENS] F5 LOWER PIT y-6
 -78                   BELLOWS HALL y-2                                            spoil ridge, Sledge Cut
 -100                  face 4 m                                                    THE LANDING y-6  F1
 -104  F4 landing      THE FOOT y-6 (sea forges)                                   Rocks + wreck (side)
 -124                  sea                                                         Sea Road (broken) -> wake
```

**Heights.** The maze floor y 0; the undercroft y -6; the Round y 8. Quarries: the Landing and the Lower Pit y -6,
the Upper Terraces y 0, the Cradle Yard y 4. Forge: the Foot y -6, the Bellows Hall y -2, the Casting Floor y 2. The
Field y 4. Each district keeps to three walkable levels (decided); the Round is the land's spine, shared by all.

**Rest fires (six).**

| Fire | Where | Foot | Why there |
|---|---|---|---|
| F1 | The Landing, in the lee of one of the giant's old stones | (70, -6, -140) | In sight of waking; the lift S1 comes down 35 m away, through the Sledge Cut |
| F2 | The Cradle Yard, at the head of the incline | (63, 4, -56) | After the quarries, before the reveal and the Round (about 7.5 min) |
| F3 | The Court of the Throne (H2) | (6, 0, 38) | The middle of the labyrinth (about 14.5 min); S2 opens a fixed way to it from H3 |
| F4 | The foot of the Slag Road | (-36, -6, -108) | After the maze and the descent (about 22.5 min); S3 comes out 30 m up the road |
| F5 | Before the Hall of the Handmaidens | (19, 2, -70) | Before the elite (about 29.5 min) |
| F6 | The god's door, in a sluice niche of the Pour Way | (4.5, 0, -18) | Decided: one at the god's door (about 35 min) |

**The route.** Wake on the broken Sea Road; the Landing (F1); the Sledge Cut; east across the Lower Pit; up the Face
Stair; west across the Upper Terraces under the first light; up to the Cradle Yard (F2) and the reveal; up the Yard
Stair and across the Round in the giant's wake; down the East Stair into the Court of the Dial (H1); round the
north-east to the Court of the Throne (H2, F3); round the north-west to the Court of the Tread (H3); south to the Stair
Court and up the Slag Stair; across the Round and down the Slag Road (F4); east along the Foot, west across the Bellows
Hall, east across the Casting Floor to the handmaiden's hall (F5); the sluice; north along the Pour Way (F6) to the
forge room.

**Branches and loops.**
- The Rocks and the wreck (see it from the Landing; reach it by sending a cart down the Old Incline).
- Pandora's cell (see it from the Round; open in one layout).
- The Cyclopes' undercroft and the plain Great Hammer (see it through the grate in H1's floor; open in another layout).
- The marsh of the bronze birds (the trial), off the Round's west side past the Tread.
- Loops: the lift (F2's terraces to F1), the Thread pin (H3 to F3), the Slag Road gate (the Casting Floor to F4).

---

## 3. The giant's round

### 3.1 Route and clock

The Round is a square: centreline at x or z = +-49, sides 98 m, corners at (+-49, 8, +-49). The giant walks it
**clockwise seen from above**: east along the north side, south along the east side, west along the south side, north
along the west side. It stops at three cradles to take stones and once at the **Tread**, the bronze plate in the road
at the west side's middle (-49, 8, 0).

| t (s) | Where the giant is | Exposed now | Quiet now |
|---|---|---|---|
| 0 - 16 | North side, west half, heading east | H2 and the north rings; the Field's north end | The forge, the quarries |
| 16 - 19 | **N cradle** (0, 8, 49): takes three stones | H2 directly below is in the rampart's lee | |
| 19 - 36 | North side, east half; NE corner | The north-east quarter of the maze | |
| 36 - 62 | East side heading south; passes the East Stair head (43, 8, -27) at t 61 | H1, the NE and SE quarters' east rings; the Cradle Yard; the Round's east side | The west: H3, the Field, the Slag Road |
| 62 - 65 | **E cradle** (60, 4, -30) | The Cradle Yard (V1) | |
| 65 - 72 | East side to the SE corner | The Upper Terraces' north-west corner | |
| 72 - 82 | South side heading west, east part | The Casting Floor's east end, the handmaiden's hall (exempt) | The north: H2 |
| 82 - 85 | **S cradle** (20, 2, -60) | | |
| 85 - 107 | South side, over the culvert at t 91; SW corner | The Pour Way, the Casting Floor, the Slag Road, the Stair Court | |
| 107 - 124 | West side heading north to the Tread | H3 (except the lee under the rampart), the Field | The east: H1, the Cradle Yard |
| **124 - 133** | **The Tread ritual** (section 4.2): the anvil rings three times, the giant stops, turns its head toward the heart, stamps three times; the layout changes | Nothing: no throws | Everywhere |
| 133 - 150 | West side to the NW corner; turns | The Field's north end, H3's north rings | |

**The clock starts** with the giant at the NW corner when the player wakes, and the layout at Axe. **It may be nudged
forward, never back**, and only while the giant is more than 60 m from the player and out of every frame, by at most
40 s. Three nudges are used, all on a first visit: at the Face Stair's head (so its light reaches the Upper Terraces'
west end about 20 s after the player tops the stair), at the Cradle Yard's entrance (so the giant is 12 to 25 s from
the E cradle: the reveal), and none in the maze, where the timing of the first shift falls out by itself (5.2, 2A).

### 3.2 The cradles, and starving the giant

The cradles are bronze baskets 5 m across on stone bases at the rampart's outer foot. The giant reaches out over the
parapet and takes three stones (3 s); it throws only those.
- **E cradle** (60, 4, -30), filled by the quarry's chained, who drag carts of rough-cut stone up the incline.
- **S cradle** (20, 2, -60), filled by the bronze-veined with lumps of slag.
- **N cradle** (0, 8, 49), on the north cliff, kept full by automata; out of the player's reach.
- **Tipping (Proposal, a question for the owner).** The E and S cradles each have a lever ("Tip the cradle"): the load
  rolls down into the quarry or the slag heap, and the giant's hands are empty from that cradle to the next. The E
  cradle tipped makes the Round's east side and H1 quiet for a lap; the S cradle tipped makes the south side, the Pour
  Way and the Slag Road quiet. They refill when the player rests (the chained and the bronze-veined come back too).
  One lever over the clock, as Sen's Fortress gives the player a lever over its boulders.

### 3.3 Where the stones land, and what is cover, place by place

| Place | When it is thrown at | Cover |
|---|---|---|
| Upper Terraces (west half) | t 55 - 90 | The foot of the 4 m Lip face; the south-east sides of the cut-block stacks (2.5 m); the crane's counterweight tower; stones already landed |
| The Cradle Yard | t 36 - 72 | The rampart's outer face (4 m); the winch house; the incline's cart; the cradle's base |
| The Round, east side | t 36 - 72, if ahead of it | None on the road: walk in its wake, or not at all |
| The labyrinth | Whichever side it is on | The lee of walls on the giant's side (1.3); the rampart's inner face when it is overhead; the hubs' stelae and daises; the Pour Way's sluice niches and the culvert |
| The Slag Road | t 85 - 107 | The inner (north) edge of each leg, under the next face up |
| The Casting Floor | t 72 - 107 | The south side of the crucible's lip, of the mould rims and of the slag heaps; the foot of the face down to the Bellows Hall |
| The Field (trial) | t 107 - 124 and 133 - 150 | The gong frames; the dead chimney; the causeways |

---

## 4. The labyrinth

### 4.1 Its shape

The labyrinth is the classical square one (the Cretan coins show it as concentric square circuits): **five square ring
corridors** round the heart, walled in marble, joined in a few places by doors and cut in a few by cross walls. Ring
walls stand at half-widths 42 (the rampart's inner face), 36, 30, 24, 18 and 12 (the heart's wall); the corridors
between them are R1 (outermost, 36 to 42) to R5 (12 to 18). The forge room is a round room of radius 11 inside the
heart's wall.

- **Three hub courts**, each 18 by 18 m where the walls between R1, R2 and R3 are left out: **H1, the Court of the
  Dial** (east middle, x 24 to 42, z -9 to 9), **H2, the Court of the Throne** (north middle, x -9 to 9, z 24 to 42),
  **H3, the Court of the Tread** (west middle, x -42 to -24, z -9 to 9). A fourth, smaller court, **the Stair Court**
  (south-west corner, x -42 to -24, z -42 to -27), holds the way out.
- **The Pour Way** runs straight up the south middle (x -3 to 3, from z -42 to the heart's door at z -12), walled 4 m
  on both sides. While the bronze runs it is a molten channel and cuts every ring's south side: the maze is a
  horseshoe. The south-east quarter, cut off by the channel and by the East Stair, is **the Dead Quarter**: dead ends,
  jars, chests, two side ways.
- **R5 is sealed in every layout**: the wall at 18 has no door. **The heart cannot be walked to through the maze.**
  It is reached only down the Pour Way, after the elite has closed the sluice (section 7). The plaques make this plain
  to the careful player: none of the three engraved layouts has a road to the middle.
- **R4 is always reachable from H2** by a fixed door at (0, 0, 24), so it can never become a sealed pocket.

### 4.2 The Tread and the shift

**Decided (owner)**: the tread shakes the maze into its next layout each time the round passes overhead. **Proposal**
for how: once a lap the giant reaches the Tread, the bronze plate in the Round above H3, and performs a ritual that
the player learns by ear before by sight.
1. t +0, +1.2, +2.4 s: **the anvil rings three times** from the heart, heard everywhere (the smith winding his maze).
2. The giant stops on the Tread and turns its head toward the heart. Its light stops sweeping; it throws nothing.
3. t +4, +5.5, +7 s: **three stamps** on the Tread. At each, every leaf that is changing moves a third of its travel:
   leaves that are rising grind up, leaves that are sinking drop, with a cloud of ash off their tops. Done by +8 s;
   the giant walks on at +9 s.
4. Within 10 m of the west rampart (H3's west half, R1 and R2 west), each stamp **staggers enemies**; the player only
   flinches (0.4 s) unless rolling or guarding.
5. **A rising leaf never crushes and never traps.** Anything standing in its groove is shoved out to whichever side
   it is nearer, without damage; a cart or an automaton on a rail stops at a closed leaf.
6. The first time, one status line: "Three rings. The walls will move."

The cycle is **Axe, Bull, Wing, Axe ...** (the double axe, *labrys*, from which "labyrinth" is said to come; the bull
of the myth; Daedalus's wings). One change every 150 s, so each layout holds for 150 s and the player never waits more
than one lap.

### 4.3 The three layouts

Eleven leaves. Each main-path leg has three ways, and each layout opens exactly one of them.

| Leaf | Where (x, y, z) | Axe | Bull | Wing |
|---|---|---|---|---|
| N1 | R1, across, north of H1 (39, 0, 12) | open | shut | shut |
| N3 | R3, across, north of H1 (27, 0, 12) | shut | shut | open |
| E4 | Door in the 24 wall, H1 to R4 (24, 0, 0) | shut | open | shut |
| W1 | R1, across, north of H3 (-39, 0, 12) | shut | open | shut |
| W2 | R2, across, north of H3 (-33, 0, 12); **the Thread leaf** (S2) | open | shut | shut |
| W4 | Door in the 24 wall, R4 to H3 (-24, 0, 0) | shut | shut | open |
| S1 | R1, across, south of H3 (-39, 0, -12) | open | shut | shut |
| S2 | R2, across, south of H3 (-33, 0, -12) | shut | open | shut |
| S3 | R3, across, south of H3 (-27, 0, -12) | shut | shut | open |
| D1 | Door in the 36 wall into Pandora's cell (36, 0, -33) | shut | shut | open |
| D2 | R2, across, south of H1 (33, 0, -15): the Cyclopes' Stair | shut | open | shut |

| Layout | H1 to H2 | H2 to H3 | H3 to the Stair Court | Side way open |
|---|---|---|---|---|
| **Axe** | R1 north-east (the outer way, under the rampart) | R2 north-west | R1 west | none |
| **Bull** | R4 (door E4, then round the inner ring's north-east to H2's south door) | R1 north-west | R2 west | The Cyclopes' Stair (D2) |
| **Wing** | R3 north-east | R4 (H2's south door, then round the inner ring's north-west to door W4) | R3 west | Pandora's cell (D1) |

E4 and W4 are never open together, so R4 never joins H1 to H3 behind H2's back. Fixed cross walls cut R2 north-east,
R3 north-west and the Dead Quarter's rings into dead ends; the dead ends hold the jars, the chests and the traps.
Every way between hubs has one corner. The maze's difficulty is in **choosing the ring and timing the light**, not in
twisting corridors: the strong shifting mazes keep the number of states small (Ocarina of Time's Water Temple has
three water levels; Dark Souls' Duke's Archives has two stair positions).

**Safety rules, checked by an EditMode test over all three layouts and the grid of corridor cells**: every cell
reaches a hub in every layout; consecutive hubs are joined in every layout; no leaf rises under a rest fire; and the
two cells that could be shut in (Pandora's cell, the Cyclopes' trench) have a fixed way out (a ladder; a grate).

### 4.4 How the maze reads from 12 m above at 55 degrees

1. **Wall tops are the drawing.** Bright marble tops 1 m wide with Greek key, over dark meander paving: from above the
   maze is a printed line drawing, as `LOOKS.md` wants ("the camera sees down into them, so the wall tops carry Greek
   key and the floors carry the routes").
2. **What can move is bronze; what cannot is marble.** Every leaf is verdigris bronze with a bright edge; when sunk it
   lies flush as a bronze strip across the floor, so a player sees "a wall can stand here" even when it does not.
3. **The cutaway** keeps the player and the floor south of them visible (1.1.2).
4. **Each quarter has its own floor**: the north-east verdigris tile, the north-west grey meander, the south-west black
   slag paving with ore streaks, the Dead Quarter rough Cyclopean blocks; the hubs pale marble with a bronze compass
   inlay. Grates set in the floor glow warmer toward the heart, so "warmer is inward".
5. **Rails down every corridor** show where automata can come from.
6. **The plaques are tables, read from above without stopping.** Each is a round bronze table 2.4 m across on a dais
   1.5 m high: three small engraved maps (Axe, Bull, Wing) in a triangle, the current one lit from beneath, a stud
   for "you are here", and round the rim a small bronze giant that travels with the real one, with a notch at the
   Tread. A glance tells the layout now, the layout next, and how long until it changes (the Antikythera mechanism is
   the nod: Greek bronze clockwork). "Read the plaque" opens a close view, click-through, at the player's own pace.
7. **Ariadne's thread (Proposal, a question for the owner).** A skeleton at the East Stair's foot holds a ball of
   thread in the land's rare accent, Aegean blue. Taken, it unwinds a fine blue line on the floor behind the player
   while inside the labyrinth (the last 250 m). A rising leaf cuts it, and the cut ends fray, so the player also sees
   where the maze moved under their own path.
8. **The watchdogs** on every dais and at every corner of the main path say where the giant is, and so which rings
   are lit.

Why this much: the Water Temple's trouble was never its layout but that players could not tell which state they were
in or where the switches were; the 3DS remake fixed it with coloured lines on the walls leading to each water-level
switch, and its creators said that was the reason for the remake.

---

## 5. The districts

Each section gives the plan, then each space in walking order. Enemy kinds are this plan's names (section 11):
the chained, the wheeler and the mender (the automata), the bronze-veined. Encounters are numbered E1.x, E2.x, E3.x.

### 5.1 District 1: the quarries (east belt, x 56 to 122, z -186 to -18)

**The idea.** The island was cut out of its own east side to build the labyrinth's walls. The quarries step up north
from the harbour in three great benches with faces that look south, a Z-shaped climb: east across the pit, north up a
face, west across the terraces toward the Round, north up to the Cradle Yard under the rampart. The chained work on
rails. The giant is heard from the first frame and seen only at the top. Teaching order: the chain's circle; carts and
brakes; the light and the lee; what it throws at; break the mender first.

```
  z    x: 56   64   72   80   88   96  104  112  120
 -18   #  +--------------------------+
       #  | CRADLE YARD y4  winch    |                 rampart outer face 4 m (x 56)
 -30   #  |[E cradle] lever  V1      |
 -36  Yard Stair ^   F2 (63,4,-56)   |
 -62   #  +==incline==+==== LIP FACE 4 m ============+
       #  |   lift S1 |   blocks   (c)   (c)   wheeler loop
 -84   #  |  UPPER TERRACES y0   Rail C ================  Old Incline
 -102  #  +===========+====== FACE 6 m =====+ Face Stair|   (down to the Rocks)
 -104  #  | LOWER PIT y-6   (c)A1    (c)A2   [crane]    |
 -116  #  | Rail A ==========================  (c)B      |
 -122  #  +-cut-+======== SPOIL RIDGE 3 m ==============+
       #  |    | F1                                     +--  ROCKS: the wreck (side)
 -134  #  | THE LANDING y-6 (quay)    old stones          |
 -158  #  +------------ sea wall -----+SR+--------------+
 -172                                  | | Sea Road
 -186                                  (broken)  @ wake (88,-6,-169)
```
`(c)` a chained at its cart; `SR` the Sea Road.

#### 1A. The Sea Road and the Landing (y -6)
- *Shape:* the Sea Road, a causeway 6 m wide (x 85 to 91), runs south from the quay into the sea and is broken off at
  z -172, a 14 m gap of water and its far stump beyond. The player wakes at its broken end facing north. The Landing
  is a stone quay x 64 to 112, z -158 to -134, with a sea wall 0.6 m high along its south edge. In the harbour either
  side of the causeway, two sunk ships, masts and stern posts above the water; three of the giant's old stones lie
  on the quay where they fell, the paving cracked round them.
- *Sees on waking:* the gap at the bottom of the frame (no way back), the causeway ahead, the first watchdog on a post
  at (80, -6, -150) slowly turning its head north-west. The footfalls. The first shot of the land shows its rule
  three ways and explains none of them.
- *Enemies:* none.
- *Giant:* out of range (more than 100 m). The stones say what lands.
- *Lore:* C1 on the causeway's last post.
- *See it:* from the quay's east end, across 6 m of water, the Rocks and a beached Greek ship with its figurehead
  (side path 5.4.1).
- *Kit:* quay paving, sea wall, `greek_ship` hulls (sunk), three `giant_stone`, `watchdog`.
- *Out:* north-west through the Sledge Cut. F1 at (70, -6, -140) in the lee of the westmost stone.

#### 1B. The Sledge Cut (x 64 to 70, z -134 to -122, y -6)
- *Shape:* a cut 6 m wide and 12 m long through the spoil ridge (3 m heaps of broken stone), sledge runners worn into
  its floor, a short spur rail down its middle.
- *Sees on entering:* a chained at its cart at the cut's north end, (67, -6, -126), facing south, a ring scraped in
  the ash round the cart where its chain drags.
- *Enemies:* E1.1, one chained. Its 12 m circle is wider than the cut: there is no way round, only through. It
  teaches the circle (the scraped ring is its reach), the swing at the circle's edge, the lash down its middle.
- *Out:* north into the Lower Pit's west end. The lift S1 lands beside the cut's north mouth (35 m from F1).

#### 1C. The Lower Pit (x 60 to 115, z -122 to -104, y -6)
- *Shape:* the oldest working floor, 55 m east to west and 18 deep, its north side the 6 m face of the Upper Terraces
  with tool marks in bands. Rail A runs east-west along z -116, falling 1 in 50 to the west; Rail B along z -108 from
  x 76 to 112. Cut blocks lie about (1.5 m). A timber crane at (106, -6, -112) holds a 2 m block on a rope over Rail A
  at (104, -6, -116).
- *Sees on entering:* two circles in the ash along Rail A, the carts, the crane's block hanging in the top of the frame.
- *Enemies:* E1.2, three chained: at carts on Rail A at (80, -6, -116) and (98, -6, -116), and on Rail B at
  (92, -6, -108). Their circles leave gaps of 6 m to thread between.
  - *Brakes and tangles:* a heavy blow on a cart (inside its circle) knocks its brake off; it rolls west down Rail A,
    dragging its chained off its feet (2 s), and if it reaches another cart or circle both chains tangle (both
    stagger 3 s). The cart at x 98 released rolls into the one at x 80.
  - *The dropped block:* the chained at x 98 can reach the crane's rope with its swing. Drawn to swing at a player
    standing by the rope, it cuts it: the block falls in a 3 m ring (marked by its growing shadow, 1 s).
- *Why it suits them:* rails make their reach move along readable lines; a sloping rail makes the cart a weapon.
- *Giant:* out of range (56 m and more). Heard, and the watchdogs on the face's top lip turn.
- *Lore:* C2, tally marks scratched by the chained on the face by the crane.
- *Out:* the Face Stair at the east end (x 106 to 112): two flights of 3 m up the face, landing at (109, 0, -102).

#### 1D. The Upper Terraces (x 60 to 115, z -104 to -62, y 0)
- *Shape:* a broad bench 42 m deep, stacked with squared blocks waiting to go up (2.5 m stacks in rows), Rail C along
  z -84 with a cart incline at its east end, the 4 m Lip face to the north, the island's east edge a low wall over the
  sea. The walk is **west, toward the rampart**: the light grows as the player goes.
- *Sees on entering:* off the stair, two chained working Rail C, the stacks, and the north-west corner bright with a
  warm light sliding across it.
- *Enemies:* E1.3:
  - two chained at carts on Rail C at (90, 0, -84) and (74, 0, -84);
  - **the first wheeler**, patrolling a rail loop round the stacks at x 64 to 80, z -98 to -88.
  When the light comes the chained drop their tools and press themselves against the nearest stack or face; a chained
  caught moving is thrown at. The player who fights in the open beside one draws the throw onto both.
- *Giant:* **the first gaze.** The clock is nudged at the stair head so the light reaches the west end about 20 s
  later and stays for about 30 s while the giant rounds the SE corner. Only the west half (x < 85) is within 40 m.
  Lee: the foot of the Lip face, the south-east sides of the stacks, the crane's counterweight tower.
- *Status line, once:* "Its light finds what moves."
- *See it, then find the way:* at the east end the Old Incline, a rail running south down the island's edge to the
  Rocks, plugged with rubble at its foot. A cart at its head released ("Release the cart") runs down and smashes the
  plug (5.4.1).
- *Shortcut:* **S1, the lift**, at the west end (62, 0, -100): a counterweight platform 4 by 4 m, sent down once from
  the top to the Lower Pit's west end, then ridden both ways.
- *Out:* the incline up the Lip face at x 60 to 66 (a stair of 4 m with a cart rail beside it) into the Cradle Yard.

#### 1E. The Cradle Yard (x 58 to 100, z -62 to -18, y 4): the reveal
- *Shape:* a yard along the foot of the rampart's outer face (4 m), the incline's winch house at its south end, carts
  of rough stone, the **E cradle** at (60, 4, -30) against the rampart, the tip lever at (64, 4, -26). F2 at the head of
  the incline, (63, 4, -56).
- *Sees on entering:* the yard, a chained tipping a cart into the cradle, a wheeler on the yard's rail, a mender at the
  winch, and the watchdog on the cradle's base **standing, eyes lit**.
- *The reveal (scripted once, V1):* when the giant comes to the cradle the camera eases back and up the rampart: its
  shadow first, then the feet coming down on the Round, then the bronze hand reaching over the parapet into the cradle.
  It takes three stones. It straightens, its light sweeps the yard, and the chained loader, still moving, is seen:
  the bell, the spot, the throw. The stone crushes the chained and its cart. Then it walks on south. The player has
  now seen everything the land's verb is: where it is, how it looks, what it does to what moves.
- *Enemies:* E1.4: the chained loader (the throw's victim, unless the player has killed it first), **a wheeler and the
  first mender**: the mender keeps back and, if the wheeler is broken, goes to it and rebuilds it in 4 s. Break the
  mender first.
- *Giant:* exposed t 36 to 72. Lee: the rampart's face (a strip about 1.5 m), the winch house, the cradle's base.
- *Lore:* C3 on the cradle's rim.
- *Out:* the Yard Stair up the rampart's outer face, rising north from (57, 4, -44) to the Round at (55, 8, -36), then
  across the road to the East Stair head at (43, 8, -27): 15 m on the Round. Taken right after the reveal, the giant
  is walking away south and the crossing is in its wake. That is the lesson of the Round: **walk behind it**.
- *Kit:* `cradle`, `ore_cart`, rails, `winch_house`, `crane`, `watchdog`.

### 5.2 District 2: the labyrinth (x -42 to 42, z -42 to 42, y 0)

**The idea.** Daedalus's maze under the giant's road, its walls moved by the giant's step. Three courts with plaques,
one at each of the maze's three open sides; between them, three ways each, one open at a time. The automata run it on
rails; the traps are in its dead ends. The verb is double here: watch the giant's light, and watch the clock that
moves the walls.

```
  z    x: -42  -36  -30  -24 -18 -12  -3 0 3  12  18  24  30  36  42
  42   +----------------------[ H2 THRONE ]----------------------+  rampart (Round above)
       | R1 NW  (W1 shut in Axe) | F3  P2 |      R1 NE           |
  36   |  +----- R2 NW ----------|        |------ R2 NE (dead) --|
  30   |  |  +-- R3 NW (dead)    |        |------ R3 NE ------+  |
  24   |  |  |  +---- R4 N ------+-door---+------ R4 N -----+  |  |
       |  |  |  |  [Bull's Corner NE of R4 is at x18..24,z18..24]
  12   | W1 W2 |  |      R5 (sealed)              |  |    N3 N1|
   9   +===========+  |   +-------------------+   |  +===========+
       | H3 COURT  |W4|   |  HEART r11        |   |E4| H1 COURT  |<- East Stair
   0   | OF THE    |  |   |  anvil, furnace N  |   |  | OF THE    |  (R1 E, z -27..-9)
       | TREAD P3  |  |   |                    |   |  | DIAL  P1  |
  -9   +===========+  |   +-------door---------+   |  +===========+
 -12   | S1 S2 S3  |  |          |PW|           |  | D2 (R2 SE)    |
       |  R1 R2 R3 west          |  |    THE DEAD QUARTER         |
 -27   +===========+             |  |       Pandora's cell D1 (R1 SE)
       | STAIR COURT  Slag Stair rises east in R1 S ->            |
 -42   +-------------------------|  |------------------------------+
```

#### 2A. The East Stair and the Court of the Dial (H1)
- *The stair:* in R1 east, x 37 to 42, from the head at (39.5, 8, -27) down north to (39.5, 0, -9): 18 m, 24 degrees,
  a balustrade 0.6 m on its west side over the drop to R2. Walking north, down into the maze, the court opening below.
- *The clew:* a skeleton at the stair's foot, (40, 0, -7), holding the blue thread (if chosen, 4.4.7).
- *Shape:* the court, x 24 to 42, z -9 to 9. In its middle **the Cyclops' Eye**, a round iron grate 12 m across at
  (33, 0, 0), glowing from beneath: through it, 6 m down, an anvil with a plain hammer on it and someone sitting by
  it (5.4.3). The plaque P1 on its dais at the north-west, (27, 1.5, 7). A watchdog on the dais.
- *Exits:* north into R1, R2, R3 (N1, a fixed wall, N3); west by E4 into R4; south into the Dead Quarter's R2 and R3.
- *Enemies:* none. The court is where the maze is explained.
- *The first shift:* taken straight from the reveal, the player comes down the stair about t 100 and the anvil rings
  at t 124: the first shift comes within half a minute of arriving, while the player is beside the plaque, and two of
  the court's own exits change in front of them (N1 rises, E4 sinks, in the change from Axe to Bull). Nothing is
  scripted: it falls out of the reveal's timing. A player who dawdles sees it later, somewhere else; the plaque has
  shown it already.
- *Giant:* exposed when it walks the east side (t 36 to 72). Lee: the R1 strip under the rampart face, the dais.
- *Lore:* C4 round the plaque's rim. V3 on the dais.

#### 2B. From the Dial to the Throne (the north-east quarter): one of three
- **Axe, R1 north-east** (63 m, one corner, along the rampart). E2.1a: a wheeler on R1's rail, patrolling from the
  NE corner to H2. The 33 m east-west stretch is its run lane: two door recesses 2 m deep in the corridor's south wall
  at (24, 0, 35.5) and (15, 0, 35.5) are where to step out of it. A **net** across R1 at (39, 0, 28): a faint shimmer,
  ash caught on its threads. Under the rampart when the giant is overhead; looked along from the NE corner (t 30 to 40).
- **Bull, R4** (door E4, north up R4 east, west along R4 north to H2's south door; 55 m). **The Bull's Corner** at
  R4's north-east corner (x 18 to 24, z 18 to 24): a great pair of bull's horns on the wall, the Minotaur's skull.
  E2.1b: a mender beside two wrecked wheelers, sparking; it rebuilds one every 4 s while the player is in sight. Jaw
  plates at (21, 0, 14) and (12, 0, 21). R4 is in long shadow from every side: the inner way is the quiet way.
- **Wing, R3 north-east** (55 m). E2.1c: a chained hauling a cart along R3's rail at (27, 0, 22). Its circle fills the
  corridor. The rail falls toward H1: a heavy blow on the brake sends the cart back down the corridor dragging it.
  R3 is 13 to 25 m from the north side: a lee strip 2 to 4 m wide along its north wall when the giant is there.
- *Dead ends:* R2 north-east is cut by a fixed wall at (33, 0, 30): two pithoi of Shards and one that bites.

#### 2C. The Court of the Throne (H2)
- *Shape:* x -9 to 9, z 24 to 42, the north rampart behind it. Against the rampart in the middle, **the golden
  throne** on a step, (0, 1, 40), facing south, a bundle of Shards on its seat. The plaque P2 at (-6, 1.5, 38). A rail
  loop runs round the court 3 m in from its walls. F3 at (6, 0, 38). Exits: east and west into R1 to R3, and the fixed
  door south into R4 at (0, 0, 24).
- *Sees on entering:* two wheelers running the loop, a mender between them, the throne.
- *Enemies:* E2.2: two wheelers on the loop and a mender. The loop means a wheeler's run comes round the court's
  edge, and the middle (the throne's front, the plaque's dais) is where its runs do not reach.
- *The throne (a trap, after Hera's):* "Take the offering" closes golden bands round the player for 2.5 s (mashing
  halves it) and rings a bell that calls the automata within 20 m. It is shown here, where the fight is over and
  F3 is 6 m away, so the lesson costs little. It still gives the Shards.
- *Giant:* exposed when it walks the north side (t 0 to 36), apart from the lee under the rampart, which is where the
  throne and F3 stand. The N cradle is above the court (t 16 to 19).
- *Lore:* the throne's back: C5.

#### 2D. From the Throne to the Tread (the north-west quarter): one of three
- **Axe, R2 north-west** (48 m). E2.3a: a chained at a cart at (-22, 0, 33), and off the corridor a cell with **two
  chests**, one verdigris and ashen, one clean with bright hinges (the binding one; section 9). The real one holds
  Shards.
- **Bull, R1 north-west** (63 m). E2.3b: a wheeler and a net at (-28, 0, 39).
- **Wing, R4** (H2's south door, west along R4 north, south down R4 west to W4; 55 m). E2.3c: a wheeler and a mender
  at R4's north-west corner, jaw plates at (-21, 0, 14).
- *Dead ends:* R3 north-west is cut by a fixed wall at (-27, 0, 30): a jar that bites, a pit plate.

#### 2E. The Court of the Tread (H3)
- *Shape:* x -42 to -24, z -9 to 9, the west rampart above with **the Tread** set in the Round at (-49, 8, 0), its
  bronze edge just visible over the parapet's lip. Three stelae 1.5 by 1.5 m and 4 m high at (-33, 0, -4),
  (-33, 0, 4) and (-28, 0, 0): the giant's light comes from the west above, so their east sides are lee. The plaque P3
  at (-27, 1.5, -6). A rail crosses the court east-west at z 0.
- *Sees on entering:* the stelae, a chained at its cart in the court's middle, the rampart rising behind.
- *Enemies:* E2.4: a chained at a cart at (-30, 0, 0), two wheelers on R1's and R2's rails meeting at the court, a
  mender. **The stamps are the room's ally**: a fight still going when the anvil rings can be pulled west, where each
  stamp staggers every enemy within 10 m of the rampart.
- *Shortcut:* **S2, the Thread pin**, a bronze pin through a bracket beside W2 at (-31, 0, 10), on H3's side. "Drive
  out the pin": W2 sinks and stays sunk in every layout, so R2 north-west becomes a fixed way between H3 and F3.
- *Giant:* exposed while it walks the west side (t 107 to 124, 133 to 150), except under the rampart. During the
  ritual it is directly overhead and throws nothing. V4 on the dais frames the rampart's top during the ritual: the
  feet on the Tread, the stamps.
- *Lore:* C6 cut in the rampart's face under the Tread.

#### 2F. From the Tread to the Stair Court (the south-west quarter)
- **Axe** R1 west (S1), **Bull** R2 west (S2), **Wing** R3 west (S3): each 18 m south, then the Stair Court. These are
  southward legs, so they hold **traps, not enemies**: Axe a pit plate (a 6 m drop into a short trench that climbs
  back into the Stair Court), Bull a net, Wing two jaw plates.
- **The Stair Court** (x -42 to -24, z -42 to -27): the foot of **the Slag Stair**, which rises east along R1 south
  from (-38, 0, -39) to (-22, 8, -39), 16 m at 27 degrees, with a cart incline on its north side. E2.5: a chained at
  the incline's foot cart, (-36, 0, -33). Lee: the stair's own mass when the giant is on the south side.
- *Out:* the Slag Stair's head opens onto the Round at (-22, 8, -43); across the road (12 m, south) to the gate in
  the outer parapet at (-22, 8, -55). Exposed while the giant walks the south side (t 72 to 107). From here the Round
  also leads west and north past the Tread to the Field (the trial).

### 5.3 District 3: the volcano forge (south belt, x -56 to 56, z -124 to -56)

**The idea.** The volcano's vent rises under the island's south side, and the smith built his forge on its flank: a
crucible of molten bronze at the rampart's foot, its overflow running south down three terraces to the sea in two
glowing channels (the West Run at x -14, the East Run at x 14) that fall over each face as bronze-falls. Below the
smith's work are the older forges of the Cyclopes, round and single-eyed. The player comes down the Slag Road to the
Foot, then climbs north back up through the forge to the crucible. The brightest place in the land, lit from below.

```
  z    x: -56   -40   -20  -14   0    14   20   36   56
 -56   ####### rampart outer face 6 m ####### S cradle #####
       |SLAG |  CASTING FLOOR y2  [CRUCIBLE] moulds | [HALL OF THE  ]
 -64   |ROAD |  moulds  gantry ======  (0,2,-64)  F5| [HANDMAIDENS ]
       |leg1 |S3 gate            West Run|  East Run| [ r12 (36,2,-70)]
 -78   |leg2 |=== FACE 4 m (Cyclopean furnace mouths) ===============
       |leg3 |  BELLOWS HALL y-2   [bellows W]   [bellows E]
 -100  |leg4 |=== FACE 4 m ======================================
 -104  |F4 ->|  THE FOOT y-6   quench troughs   bridges over the Runs
 -124  ======== sea (the Runs hiss into it) ==========================
```

#### 3A. The Slag Road (x -54 to -22, from y 8 at z -57 to y -6 at z -104)
- *Shape:* four ramps 18 m long down the cliff below the south rampart, each 3.5 m lower than the last (11 degrees),
  running west, east, west, east, joined by landings; each leg's south edge is a face 3.5 m high looking south (seen),
  with a kerb 0.6 m and two gaps where the player may drop to the leg below (one-way drops face south, as in Fire).
- *Giant:* in range of the upper two legs while it walks the south side. Lee: the inner (north) edge of each leg,
  under the face of the leg above.
- *Enemies:* none. A breather after the maze, and the first sight of the forge's glow below.
- *Out:* F4 at the foot, (-36, -6, -108).

#### 3B. The Foot (x -56 to 56, z -124 to -100, y -6)
- *Shape:* a sea-level shelf of the old forges: in the face to the north, **round furnace mouths of the Cyclopes**,
  3 m across, cold, single-eyed (facing south: seen). Quench troughs of black water, slag heaps, the two Runs crossing
  south into the sea under steam, bronze footbridges over them. The walk is east.
- *Enemies:* E3.1, two bronze-veined: one at a quench trough at (0, -6, -110), its nail on its back; one on the East
  Run's bridge at (14, -6, -112), its nail at the left of its neck. The first teaches **circling to the nail**; the
  second, on a bridge, teaches that it turns to keep the nail away and that a bridge is narrow.
- *Giant:* out of range. The tremor and the dogs only.
- *Lore:* C7 in the first Cyclopean furnace mouth.
- *Out:* a stair at the east end (x 30 to 36) up the face into the Bellows Hall.

#### 3C. The Bellows Hall (x -20 to 56, z -100 to -78, y -2)
- *Shape:* two great bellows (8 m long, leather and bronze) against the north face at x -6 and x 24, their nozzles
  pointing south into the hall. They breathe on every second footfall of the giant (every 4.8 s): the island's machines
  run on its tread. The walk is west.
- *The gust:* 0.8 s of warning (the leather swells, sparks gather at the nozzle), then a blast down a lane 3 m wide and
  14 m long running south: it pushes the player 2 m toward the camera, does 8, and **heats any bronze-veined in it**.
- *Enemies:* E3.2, three bronze-veined working: at (-6, -2, -90) and (24, -2, -90), in the lanes, and one by the West
  Run at (-10, -2, -94). Quench troughs at (8, -2, -92) and (40, -2, -88). The room's answer: fight them out of the
  lanes or they come at you red-hot; knock one into a trough and it is brittle.
- *Giant:* the north edge (the foot of the Casting Floor's face) is within range while it walks the south side; the
  face's foot is lee.
- *Out:* a stair at the west end (x -20 to -14) up to the Casting Floor.

#### 3D. The Casting Floor (x -20 to 56, z -78 to -56, y 2)
- *Shape:* the smith's working floor under the rampart. **The crucible** at (0, 2, -64), a round basin 8 m across with
  a 1 m lip, molten, fed from the vent beneath. A gantry crane along z -72 from x -18 to x 12 carries a ladle: every
  20 s it dips at the crucible and travels west to pour into one of three **moulds** (pits 4 by 6 m, 2 m deep, at x -16,
  -8 and 8), moulds for the giant's spare parts: a hand, a face, a nail. The pour is a 2 m column for 1.5 s, its ring
  shown 1.2 s ahead; the ladle drips glowing beads along its way. A mould is a pit: falling in costs 15 and a ladder
  climbs out. East of the crucible the S cradle at (20, 2, -60) and slag heaps. The walk is east.
- *Enemies:* E3.3: two bronze-veined casters at (-12, 2, -68) and (6, 2, -74); a mender and a wheeler on the gantry's
  rail along z -76. Lure a bronze-veined under the pour.
- *Giant:* exposed while it walks the south side. Lee: the south sides of the crucible's lip, the mould rims, the
  heaps.
- *Shortcut:* **S3**, a barred gate in the west wall at (-21, 2, -74), barred on this side, level with the Slag Road's
  second leg where it turns. Opened, F4 is two legs down.
- *Out:* east to F5 at (19, 2, -70), past the East Run's bridge, and the hall.

### 5.4 The side paths: see it, then find the way

#### 5.4.1 The Rocks and the wreck (Proposal)
- *Seen:* from the Landing's east end (1A), across 6 m of water: a beached Greek ship on a reef, x 104 to 122,
  z -150 to -128, its figurehead turned toward the island (the Argo's nod: Talos threw stones at the Argonauts).
- *Found:* from the Upper Terraces' east end, by releasing a cart down the Old Incline; it smashes the rubble plug at
  the foot, (118, -6, -128). Walk down the incline.
- *There:* two chained anchored not to carts but to the ship's stone anchors (fixed circles, 12 m, overlapping: the
  space between them is a puzzle of timing their swings). On the figurehead's beam, C8. Shards in the hold.
- *Back:* a plank on the Rocks kicked across the water gap to the quay ("Kick the plank"; Fire's ladder, B6), 20 m
  from F1.
- *Minutes:* 2 for a new player.

#### 5.4.2 Pandora's cell (Proposal)
- *Seen:* from the Round, crossing from the Yard Stair to the East Stair (1E), the walled cell in R1 south-east lies
  8 m below at the frame's west edge, x 36 to 42, z -39 to -27, sealed on all sides, a great pithos 2 m high in it.
  V2 there frames it.
- *Found:* in the Wing layout, door D1 in the 36 wall at (36, 0, -33) opens from the Dead Quarter's R2. A player who
  reads the plaque knows when.
- *There:* the great jar, sealed; four small jars round it (two bite: their lids show a thin red line of heat). The
  great jar broken lets out "the evils": a ring of hot rivets bursting outward (a pale ring 0.8 s ahead). Under the rim
  at the bottom, what stayed: **Hope**, lore and a gift (the gift is the owner's to choose, as the waterskin at Fire's
  well was a fourth flask). The words: C9.
- *Back:* a ladder up the rampart face to the Round at (40, 8, -35), so the cell can never shut a player in.
- *Minutes:* 2, plus the wait for Wing.

#### 5.4.3 The Cyclopes' undercroft and the plain Great Hammer (Proposal)
- *Seen:* through the Cyclops' Eye, the round grate in H1's floor: an anvil 6 m below, a hammer on it, a hunched
  figure beside it. **The land's plain weapon is here** (decided: the side paths hold it). Seen again on the way in:
  R2 south-east's floor from z -12 to -32 is grating, and under it the player sees the stair and the trench they will
  take.
- *Found:* in the Bull layout, leaf D2 sinks at (33, 0, -15). R2 south-east runs on to the stair's head at (33, 0, -32);
  **the Cyclopes' Stair** goes down north to (33, -6, -22), and a trench 5 m wide runs north under the grating to the
  chamber under the Eye. (Nothing walkable here is under anything but grating, which the camera sees through; it is
  drawn at a third of its strength while the player is beneath.)
- *There:* the Cyclopes' anvil chamber, round, 12 m across, under the Eye. **Kedalion** (a named fighter; in the myth
  the servant of Hephaestus on Lemnos who carried blind Orion): a bronze-veined master smith whose eyes the pour put
  out, with a mender riding on his shoulders that sees for him and calls where the player is. He strikes where it
  points. Knock the mender off (from behind, or in a stagger) and he swings blind: wide, wild, toward any sound (rolls
  are loud). Two nails. The plain Great Hammer on the anvil after. C10 on the anvil's foot.
- *Back:* a ladder up through a hinged grate into H1 at (33, 0, 5), opened from below.
- *Minutes:* 4.

---

## 6. The elite's arena: the Hall of the Handmaidens

**Decided (owner)**: the first golden handmaiden; she learns, and parries an attack the player keeps using.
**Proposal (`WORLD.md`)**: she is the land's one sane voice and speaks as she fights.

**Shape.** A round hall of radius 12 at (36, 2, -70), on the Casting Floor's east end against the rampart's foot. Its
north half is walled 5 m with eight niches, each holding an **unfinished sister** on a plinth (headless, half-cast,
gold showing through the mould's crust); its south half is walled 1.2 m (the camera). The roof fell long ago (none).
The floor is marble with the meander, and set in it a ring of eight golden plates (the smith's binding plates, which
she can wake). On the north wall, **the sluice wheel**, its shaft running under the floor to the crucible's gate. The
door is on the west, at (24, 2, -70); F5 is 5 m outside it.

**How it plays with her.** In the myth the golden maidens held the lame god up as he walked. She fights with his old
staff-crutch, in wide arcs that sweep his weight round (a preview of the smith's crutch-swings), and wakes the binding
plates under the player (a preview of his golden plates). Her rule:
- She remembers the last eight attacks that reached her (each light in a chain by its place, the heavy, the charged
  heavy, the weapon skill, each spell, each infused strike). One used three times in that memory is **learned**: from
  then she perfect-guards it, and the player is staggered for 0.8 s and answered. She holds three learned attacks at
  most, and forgets the oldest each time three different attacks land.
- **It is shown, not told.** A learned attack winds a gold thread round her wrist (up to three) and she says so:
  "That one again. I have it." / "Change your hand, or I keep it." From above the threads read as three bright rings.
- **The room supports variety.** Her guard covers her front, so the plinths are the other answer: a blow from behind a
  plinth's line, as she comes round it, is never parried. The plates make standing still the wrong answer. The hall's
  size (24 m across) lets ranged spells and the hammer's toss be mixed in.
- **No throws from the giant here** (the elite's ground is exempt, as Fire's bowl was from the heat).
- `HoldGround(24, 48, -82, -58)`.

Her lines (Proposal, to be set in the voice of `GAME.md`): on meeting, "You walked here. He has not walked in an age.";
at half health, "He made us to lean on. Then he made a body that needs no one."; falling, "Go down. Tell him I held."

When she falls, the sluice wheel can be turned ("Turn the sluice", held 3 s).

---

## 7. The approach to the forge room, and the room

**The sluice.** The wheel shuts the crucible's north gate. The Pour Way's bronze stops; its glow fades back along the
channel from the gate to the heart over 20 s (the player sees it go out north of them, the way they are about to walk),
leaving a black crust with red seams that is warm underfoot.

**The Pour Way** (x -3 to 3), walked north, the forge room ahead:
- From the gate (0, 2, -58) through **the culvert** under the Round's south side (z -56 to -42; floor y 2, the road's
  deck 5 m above it, fading while the player is under it): the one long walk under something in the land, allowed as
  the owner's rule allows, because it fades. The culvert is also cover from the giant.
- Into the maze at z -42, sloping down to y 0 by z -30 (9 degrees), then level to **the heart's door** at
  (0, 0, -11). Its walls are 4 m, bronze-faced, with **grilles** where each ring crosses: the player sees, through
  them, the corridors they walked, now still.
- **Sluice niches**, 3 m by 2 m cut into the walls, at z -36 (west), -30 (east), -24 (west) and -18 (east: F6, the
  god's door). Cover.
- **The last watch.** When the giant walks the south side (t 85 to 107) it stands over the culvert and looks straight
  down the channel: a corridor that points at it, no lee but the niches. Walk it when it is elsewhere, or niche to
  niche. No enemies. Tension, the view, the fire.
- **V6** at the culvert's north mouth (0, 1.5, -41): the camera eases back and looks up the channel to the heart's
  door, and the furnace's glow coming out of it.

**The forge room** (Decided: at the labyrinth's centre; `LOOKS.md`: low and round, lit by the anvil and a furnace
mouth like a single eye).
- Round, radius 11, wall 5 m, a low dome drawn shadow-only. The door on the south; **the anvil** at (0, 0, 0); **the
  furnace** at the north, (0, 0, 8), a round brick oven 6 m across whose mouth, 3 m round, faces south: the camera
  sees the eye from the door. A grated runnel down the floor from the door to the furnace's foot, where the bronze
  used to run, still glowing faintly: the line of approach.
- Phase 1 (Proposal in `WORLD.md`: he does not chase; the arena comes to him) needs the room clear but for the anvil
  and the runnel.
- **Phase 2's ground** (Decided: the giant crashes through the roof, and its landing smashes the walls so the fight
  spills out into the labyrinth). The dome, the heart's wall and the 18 wall are built as breakable sections 6 m long;
  the 24 wall holds and all leaves freeze shut. The fight's ground is then the heart, R5 and R4, about 48 m square,
  with stubs of wall as cover that the giant knocks down one by one. It comes in from the point of its round nearest
  when called, leaving **a breach** in the rampart's inner face there (the chariot's way out). How the camera frames a
  20 m enemy is open in `GAME.md`; this plan assumes the boss camera's pull-back.
- The heart's door: C11.

---

## 8. The optional trial: the bronze birds (decided by the owner, 2026-11-19; the details are Proposal)

**Decided (owner)**:
- One optional trial per land, different in every land, guarding a Memory or a Body piece.
- For this land, the bronze birds. The owner turned down the Yoke of the Bronze Bulls and the other first options.

**The nod.** Heracles' sixth labour: the Stymphalian birds had bronze beaks and feathers they could shoot like darts.
Heracles could not reach them in their marsh, so Athena gave him bronze clappers (krotala) made by Hephaestus. Their
din drove the flock into the air, where he shot them. Here the smith cast the birds for the old one-eyed smiths'
marsh foundry, and they never left it.

**Where.** The Field, the west belt outside the Round (x -104 to -62, z -40 to 30, y 4), made into a slag marsh.
- It is reached by the sally stair down the rampart's outer face at (-56, 8, -14), from the Round's west side.
- Shallow black pools are cut by causeways of cinder 3 m wide.
- **V5** at the stair's head frames the marsh and the flock wheeling over it.
- **Five bronze gongs** hang on frames at the corners and the middle of the causeway cross: (-94, 4, -30),
  (-72, 4, -30), (-94, 4, 20), (-72, 4, 20) and (-83, 4, -5).
- A dead foundry chimney stands at (-83, 4, 26).

**How it plays.**
1. *The flock.* Twelve birds circle above the marsh. Only their shadows show inside the frame: a ring of dark marks
   turning on the ground.
   - Every few seconds a bird stoops and looses a volley of three bronze feathers along its shadow's line.
   - Each feather's landing is shown as a thin line on the ground 0.8 s ahead. The feathers stick in the cinder for
     a few seconds, as spikes that slow.
   - High in the air, the birds cannot be reached.
2. *The gongs.* A blow on a gong rings it, and the nearest three or four birds within 10 m drop out of the air in
   fright. They land and flap on the causeway for 4 s, grounded and open to blows: two blows break a grounded bird.
   - The ring also turns the flock: the next volley comes at the player.
   - A gong is dead for 8 s after it rings.
   - Ringing well means choosing the gong under the most shadows, then getting out of the volley line before
     striking.
3. *The lead bird.* When eight are broken, the lead bird comes down from the chimney. It is twice the size and walks
   the causeways, beating its wings to throw a fan of feathers.
   - It grounds only when two gongs ring within 2 s of each other, so the player runs between the pairs and lands one
     blow on each.
   - It breaks after three groundings.
- **Reward:** a Body piece in the lead bird's nest at the chimney's foot.
- **Minutes:** about 5 to 6.
- **Why it suits this land:** Metal is learned by its sounds (footfalls, the anvil, the tread), and here the player
  makes the sound. The other trials of the climb are fights about distance, tide and footing; this one is about timing
  a noise. It uses parts that exist: the feather lines are the huntress's warned arrows of the Ice land made small,
  and the gongs are a struck prop like the anvil. The build list's bulls, plough and sown men are replaced by twelve
  birds, a lead bird and five gongs.

---

## 9. The traps (Decided: traps everywhere, nodding to the golden throne and the net; Proposal: their forms)

All traps share one tell family, because the ash falls on everything: **true things gather ash; things that move
are clean.** Menders polish the binding chests and re-arm sprung plates, so a mender walking to a spot is a tell too
(the careful eye in `LOOKS.md`: "a careful eye sees which plates are newer"; Dark Souls 3's mimics are told the same
way, by a chest's chain lying the wrong way).

| Trap | The nod | Tell | Effect | Where |
|---|---|---|---|---|
| Binding chest | The golden throne | No ash, bright hinges | Golden bands 2.5 s (mash halves), a bell calling automata in 20 m; in the giant's light the player is a sitting target | R2 north-west's cell; the Dead Quarter (2) |
| The golden throne | Hera's throne | It is a throne | As the chest, once, as the lesson | H2 |
| Jaw plate | "floors that close" | A clean plate with a hairline seam | 18 damage, held 1.2 s | The Bull's Corner, R4 north-west, the Wing leg south of H3 |
| Pit plate | "floors that close" | Clean, and it rings hollow when an automaton rolls over it | A 6 m drop into a short trench, 10 damage, a climb back up | The Axe leg south of H3, R3 north-west's dead end |
| The net | The invisible net | A shimmer near, ash caught on its threads | Slowed by 60% for 2 s; rolled into, wrapped for 3 s. **Teaches the smith's net**, whose only tell is a shimmer | R1 north-east (Axe), R1 north-west (Bull), the Bull leg south of H3 |
| Biting jar | Pandora's jar | A thin red line of heat under the lid | A ring of hot rivets, 0.8 s pale ring, 15 | Dead ends; Pandora's cell |
| The crane's block, the pour, the gust | The forge itself | Shadow, ring, swelling leather | Section 5 | Lower Pit, Casting Floor, Bellows Hall |

---

## 10. The chariot (Decided: bronze chariot, bronze horses; Proposal: where and how)

When the giant is torn open in phase 2 the forge room's north wall falls, and behind the furnace is the smith's
casting bed: **a bronze chariot and two bronze horses in their moulds**, finished, never used. (He was the god who fell
from the sky for a whole day; his line is "I built a body that wouldn't limp. I never left it.") After the choice over
his body, the horses wake, still red from the casting and cooling to bronze as they move (`LOOKS.md`). In one ride the
player is carried up through the breach the giant made, **once round the Round**, the round run at last by something
alive, down the quarry haul roads to the Landing and out along the Sea Road, and the horses leap the break where the
player woke. Then the hub.

---

## 11. The land's people as the rooms use them (Proposal; full sheets for `metal_land.md`)

Each does one thing no other enemy in the game does, out of its corruption. Numbers are first values.

**The chained** (they mine the quarries). Chained by 6 m to an ore cart on a rail: **their reach is the circle**, shown
by the ring their chain scrapes in the ash. *Swing*: a full sweep at the circle's edge (0.9 s, pale). *Lash*: the
chain's end thrown straight down a lane to the circle's edge (0.7 s, red). *Haul*: they drag the cart 3 m toward the
player along the rail in 1.5 s, moving the circle (the rail shows where it can go). *Brake*: a heavy blow on the cart
knocks the brake off; on a slope it rolls, dragging them down (2 s), and carts or chains that meet tangle (both stagger
3 s). *Hide*: in the giant's light they freeze against a face and do not attack: a free blow, taken in the light.
Health 70, no ranged attack, cannot leave the circle.

**The wheeler** (an automaton: a cauldron of glowing slag on three wheeled legs, 1.6 m; the tripods that rolled
themselves to the gods' gatherings). Rides rails only, so it needs no pathfinding in a maze. *Run*: tucks its legs and
rolls down its rail at 9 m/s to the next junction or wall (a red lane the width of the rail's corridor, 0.9 s, glowing
along the rail); then 1.5 s unfolding: the opening. *Tip*: rears and pours a 3 m wedge of slag that stays hot for 5 s
(red). *Kick*: a leg, 2 m (pale). Broken, it falls as a **wreck that sparks for 20 s**; a heavy blow or a finisher on a
wreck scraps it at once.

**The mender** (an automaton, 1 m, tongs and a bellows-snout). Keeps 6 m from the player. *Repair*: goes to a wreck and
rebuilds it in 4 s (a ring of sparks; any hit breaks it off). *Re-arm*: walks to sprung traps and sets them in 2 s.
Cornered, a puff of the bellows that pushes the player back 2 m. Two blows kill it. **Break the repairer first**
(`WORLD.md`, Proposal).

**The bronze-veined** (their blood turned to molten bronze; the forge's workers). Each has **one nail** (Talos's
nod), a glowing gold stud on one side: back, front, or left or right of the neck. They turn to keep it from the player
at 120 degrees a second, slower than a player circling close. A blow from the nail's side (within 50 degrees) loosens
it; two pull it, or one heavy: they kneel and drain for 3 s, a hot pool spreading 3 m, then set as a crust statue.
Blows elsewhere do 40%. *Heat*: every blow given or taken, and every bellows gust, heats them; red-hot (about 10 s of
fighting) their blows burn and they gain *the embrace* (Talos crushed men in a red-hot embrace): a red grab lane 2.5 m,
30 over 1 s and thrown. A perfect guard against the embrace cracks their crust and swings the nail round to the
player. *Quench*: knocked into a trough (the hammer's lift, any shove), they go dark and brittle: 5 s of double damage,
no turning.

---

## 12. Shortcuts and loops

| | Opens from | To | How | Saves |
|---|---|---|---|---|
| S1 | The Upper Terraces' west end (62, 0, -100) | The Lower Pit's west end, through the Sledge Cut to F1 (35 m) | A counterweight lift, sent down once from the top | F2's district to F1 and the Rocks without the pit's fights |
| — | The Rocks | The Landing's east end | A plank kicked across | The way home from the wreck |
| S2 | H3, the Thread pin at (-31, 0, 10) | W2 stays sunk: H3 to H2 (F3) by R2 north-west in every layout | A pin driven out | After a fall past H3: F3 to H3 with no wait on the clock |
| — | Pandora's cell | The Round | A ladder | Never shut in |
| — | The Cyclops' chamber | H1 | A grate ladder, opened from below | The way out of the undercroft |
| S3 | The Casting Floor's west wall (-21, 2, -74) | The Slag Road's second leg, two legs above F4 | A barred gate | F4 to the Casting Floor without the Foot and the Bellows Hall |
| Loop | The Round | Everywhere it touches | Walking, in the giant's wake | The Round itself joins the districts: the Yard Stair, the East Stair, the Slag Stair, the Field's stair |

S1 to S3 stay open after a fall and across sittings (a progress key each, as Fire's). Tipped cradles and the layout do
not: they come back with the enemies at a rest.

---

## 13. Vista zones

As Fire's B13 (`TopDownCamera.SetFraming`: a distance factor, a look offset, a pitch change), not while an enemy has
noticed the player. Two of these turn the look toward the giant instead of only ahead.

| | Where | Size | Framing | What it shows |
|---|---|---|---|---|
| V1 | The Cradle Yard (66, 4, -34) | 10 by 6 m | 1.8, look (-6, 6, 4), -15 | The rampart's top, the giant at the cradle (the reveal, once scripted, then whenever it passes) |
| V2 | The Round's east side (45, 8, -32) | 8 by 4 m | 1.5, look (-6, -4, 2), -5 | The maze below: R1 to R3 east and Pandora's cell |
| V3 | H1's dais (27, 1.5, 7) | 4 by 4 m | 1.6, look (2, 0, -2), -10 | The court and its exits; the Eye |
| V4 | H3's dais (-27, 1.5, -6) | 4 by 4 m | 1.8, look (-10, 6, 4), -15 | The rampart's top and the Tread; the stamps |
| V5 | The Field's stair head (-56, 8, -14) | 6 by 4 m | 1.6, look (-12, -4, 4), -10 | The marsh, the flock wheeling over it, the gongs |
| V6 | The Pour Way's culvert mouth (0, 1.5, -41) | 6 by 4 m | 1.8, look (0, 0, 10), -10 | Up the channel to the heart's door and the furnace's glow |

---

## 14. Lore (Proposal: places and first words, to be set in the voice of `GAME.md` by the owner)

The sponsor's story is told aloud; the island quietly tells the murdered first god's, a nod to the Cyclopes, the
one-eyed smiths who forged the thunder, whose forges the usurper took (`WORLD.md`, Proposal).

| | Where | Words (first draft) |
|---|---|---|
| C1 | The Sea Road's last post | "Nothing comes in. Nothing goes out. He calls it keeping." |
| C2 | The Lower Pit's face, tally marks | "Cut for the walls. Cut for the walls. Cut for the walls." |
| C3 | The E cradle's rim | "Fill it and it throws. Empty it and it only walks." |
| C4 | The plaque's rim in H1 | "Three roads. One heart. No road to it." |
| C5 | The golden throne's back | "A gift for his mother, who let him fall." |
| C6 | Under the Tread | "He walks it so he never has to." |
| C7 | The first Cyclopean furnace | "One eye each, and it saw the fire true." |
| C8 | The wreck's figurehead | "Fifty oars came for a fleece. The round came for the oars." |
| C9 | Pandora's jar | Outside: "Every gift he made has a hinge." At the bottom: "One thing he forgot to make bite." |
| C10 | The Cyclopes' anvil | "We forged the thunder. He forged a lock." |
| C11 | The heart's door | "He fell for a whole day. He has not stopped." |

---

## 15. Build list

Plain geometry made in code as in Fire (`MeshFilter`, `MeshRenderer`, `MeshCollider`, static, inked). Fire's B1
(Plateau), B2 (Bank face), B3 (Ramp), B6 (Ladder), B7 (Barred door), B8 (Breakable wall), B9 (Jar) and B13 (Vista
zone) are reused as named; where Fire's pieces are not yet in code, they are built once for both lands.

### 15.1 New geometry

| | Piece | What it is | Dimensions and parameters |
|---|---|---|---|
| M1 | **Rampart and the Round** | A square ring wall: inner face 8 m above y 0 at half-width 42, outer face down to each belt's level at 56, the road on top 12 m between parapets 0.6 m; the Tread plate; corner bastions; breakable section where the giant breaches | The south inner face fades whole when the player is within 8 m north of it |
| M2 | **Maze wall** | Marble wall 3.5 m high, 1 m thick, top with Greek key; **cutaway** material (above 1.2 m dithers out within 5 m south of the player, the top edge kept as an ink line) | Laid from the ring and cross-wall list in 4.1 and 4.3 |
| M3 | **Leaf** | Bronze panel 5 by 3.5 by 0.6 m in a lined groove; rises or sinks in thirds on the stamps; a shove volume; sunk, a flush strip | Eleven, from the table in 4.3 |
| M4 | **Rails and carts** | Bronze rail strips as splines with a graph; ore carts with a brake that roll down slopes; a cart incline; a chained's anchor point | Rails A, B, C, the incline, the Old Incline, the maze's corridor rails, the court loops, the gantry rail |
| M5 | **Quarry benches** | Plateaus with south faces of cut stone in tool-marked bands (B2 with a "quarry" style), block stacks, the spoil ridge with the Sledge Cut | Section 5.1 |
| M6 | **Lift** | A counterweight platform 4 by 4 m on chains; "Send the lift down" from the top once (progress key), then rides both ways | S1, 6 m |
| M7 | **Crane with a hanging block** | Timber crane; a rope with a 2 m block; the rope breaks when a chained's swing crosses it | The Lower Pit |
| M8 | **Cradle** | Bronze basket 5 m on a stone base, holds three stones, tip lever, refill on rest | E, S, N |
| M9 | **Hub court** | An 18 m court, a dais 1.5 m with the plaque table, a watchdog, lee pieces (stelae) | H1, H2, H3 |
| M10 | **Plaque table** | Round bronze table 2.4 m: three engraved maps lit by layout, a "you are here" stud, a travelling giant figure on the rim with the Tread notch; interact opens a click-through close view | P1, P2, P3 |
| M11 | **Grate floor and trench** | Walkable iron grating over a lit trench or chamber, drawn at a third while the player is beneath; the Cyclops' Eye (12 m round) | H1, R2 south-east, the undercroft |
| M12 | **The Pour Way** | A channel 6 m wide with 4 m walls, two states (molten; cooled crust with red seams), grilles at ring crossings, sluice niches, the culvert (its deck fades) | Section 7 |
| M13 | **Forge pieces** | Crucible basin with lip; the two Runs (molten channels with bronze-falls over faces); quench troughs; slag heaps; moulds (pits with ladders); gantry crane with ladle and pour cycle; bellows with a gust lane; Cyclopean furnace mouths | Section 5.3 |
| M14 | **The Slag Road** | Four ramps of 18 m at 11 degrees with landings, kerbs and drop gaps | Section 5.3 |
| M15 | **Hall of the Handmaidens** | Round hall r 12, the north half walled 5 m with eight niches and plinths, the south half 1.2 m, eight floor plates, the sluice wheel | Section 6 |
| M16 | **The forge room** | Round room r 11, shadow-only dome, the anvil, the south-facing furnace, the runnel, breakable sections of the dome and the 12 and 18 walls, the casting bed behind the north wall | Section 7 |
| M17 | **The Field** | Slag pools, cinder causeways, five gong frames, the dead chimney and the nest | Section 8 |
| M18 | **The Landing** | Quay, sea wall, the broken causeway, sunk hulls, the Rocks and the wreck, the rubble plug (B8, broken by a cart) | Section 5.1 |

### 15.2 Behaviour the rooms rely on (for the code worker)

| | What | Where |
|---|---|---|
| G1 | **The round**: the giant walking the square on a clock (150 s), corners, cradle stops, footfall events (sound, tremor, footprints on the Round), the nudge rule (forward only, unseen, at most 40 s), dithering when between camera and player | Everywhere |
| G2 | **The gaze**: one real-time shadowed spot light from the visor, sweeping; the seen test as a ray from the same point; the bell, the hold, the spot | Everywhere in range |
| G3 | **Stones**: carried count, throw, growing-shadow mark, landing damage to all, stay (open ground, six at most) or shatter (maze), stagger bulls, derail carts | Everywhere in range |
| G4 | **The Tread and the layouts**: the anvil, the stamps, the leaf moves, the layout table, the plaque state, the stamp stagger near the west rampart; the EditMode test of 4.3 | The labyrinth |
| G5 | **Watchdogs**: look at the giant; stand and glow within 40 m | Every hub, corner and stretch of the main path |
| G6 | **Ariadne's thread** (if chosen): a line on the floor behind the player inside the maze, cut by rising leaves | The labyrinth |
| G7 | **Traps**: the binding (chest and throne), jaw plate, pit plate, net, biting jar; the mender's re-arm | Section 9 |
| G8 | **Forge rhythms**: bellows on every second footfall; the ladle's pour cycle | The Bellows Hall, the Casting Floor |
| C1 | The chained (section 11) | Quarries, maze, Rocks |
| C2 | The wheeler and the mender, moving on the rail graph | Everywhere but the Rocks |
| C3 | The bronze-veined (nail side, turning, drain, heat, embrace, quench) | The forge; the sown men; Kedalion |
| C4 | The handmaiden's memory and parry | Section 6 |
| C5 | The bronze birds (circling shadows, warned feather volleys, grounded by a gong), the lead bird (two gongs at once) | Section 8 |
| C6 | Kedalion and his shoulder mender | 5.4.3 |

### 15.3 Kit and code reused, and new kit

- **Code from Fire**: `SunCityBuilder`'s helpers (`Floor`, `Box`, `Mass`, `Stairs`, `Disc`, `Paving`, `Scatter`,
  `Rubble`, `Solid`, `Carving`, `Rest`), Fire's B-pieces above, `TopDownCamera.SetFraming`, `HoldGround`, progress keys,
  `AreaDirector.OnRevived` for return to the last fire. Fire's shade list and heat schedule are not used.
- **New kit models** (through staging and the owner's approval, as every asset): the giant (Decided: new, detailed,
  Greek), `watchdog`, `wheeler`, `mender`, `chained`, `bronze_veined`, `handmaiden`, `bronze_bull`, `ore_cart`,
  `cradle`, `leaf`, `maze_wall` tops with Greek key, `plaque_table`, `crane`, `bellows`, `crucible`, `ladle_gantry`,
  `anvil`, `forge_furnace`, `cyclops_furnace_mouth`, `greek_ship`, `pithos`, `bronze_chest`, `golden_throne`,
  `tripod_brazier` (Metal's rest fire), `chariot` and `bronze_horse`.

---

## 16. Pacing

Minutes for a new player on a clean pass, as Fire counts them: a weak enemy about 30 s, a strong one 45 s, a group a
little less each than alone.

| Stretch | Minutes (new) | Experienced | Tension | What it teaches |
|---|---|---|---|---|
| The Sea Road, the Landing, F1 | 1 | 0.5 | Rest | The round heard; what lands; no way back |
| The Sledge Cut | 1 | 0.4 | Rising | The chain's circle |
| The Lower Pit | 2.5 | 1.2 | Peak | Circles on rails; brakes and tangles; the dropped block |
| The Face Stair, the Upper Terraces | 2.5 | 1.2 | Rising, peak | The light; lee; the chained hide; the first automaton |
| The Cradle Yard, F2, the reveal | 2 | 1 | Reveal, peak | The giant seen; it throws at what moves; break the mender first |
| **District 1** | **9** | **4.3** | | |
| The Round, the East Stair, H1 | 1.5 | 0.7 | Dread | Walk in its wake; the plaque; the first shift |
| H1 to H2 (one of three ways) | 2 | 1 | Peak | That way's guard |
| H2, the throne, F3 | 2.5 | 1.2 | Peak, rest | A fight on rails; the bands |
| H2 to H3 | 1.5 | 0.7 | Rising | Chests that bind, nets |
| H3 under the Tread | 2.5 | 1.2 | Peak | The stamps; the pin (S2) |
| H3 to the Slag Stair | 1 | 0.4 | Rising | A chained on the incline |
| Waiting on a shift (average) | 1 | 0.3 | | Reading the plaque |
| **District 2** | **12** | **5.5** | | |
| The Round, the Slag Road, F4 | 1.5 | 0.7 | Relief | The light over the switchbacks |
| The Foot | 2 | 1 | Rising | The nail; the quench |
| The Bellows Hall | 2.5 | 1.2 | Peak | Heat; gusts on the tread |
| The Casting Floor | 2.5 | 1.2 | Peak | The pour; the mender among workers |
| F5, the Hall of the Handmaidens | 3.5 | 1.8 | Peak | Vary, or be parried |
| The sluice, the Pour Way, F6 | 1.5 | 0.7 | Dread, vista | The last watch; the eye ahead |
| **District 3** | **13.5** | **6.6** | | |
| The forge room: the smith, then the giant | 9 | 6 | Climax | (the god's fight) |
| **Clean pass** | **43.5** | **22.4** | | |
| Side paths (the Rocks 2, Pandora's cell 2, the undercroft and Kedalion 4) and the trial (5) | + 13 | + 7 | | |

That is three and a half minutes over the 40 proposed for an early solo land. The levers, in order: the third chained
in the Lower Pit (0.5), one bronze-veined in the Bellows Hall (0.7), the wheeler on the Casting Floor (0.5), the
chained at the Slag Stair (0.5), one wheeler fewer in H2 (0.5), the Slag Road at three legs (0.3), the Sledge Cut's
chained moved into the pit (0.4). With all of them, 40.1.

**Rest-fire spacing** (decided rhythm, 6 to 8 minutes): F1 0, F2 7.5, F3 14.5, F4 22.5, F5 29.5, F6 35; the gaps are
7.5, 7, 8, 7 and 5.5.

**Enemy count.**

| District | Main path | Count |
|---|---|---|
| D1 | 7 chained, 2 wheelers, 1 mender | 10 |
| D2 | One way per leg, so it varies by layout: Axe 3 chained, 5 wheelers, 2 menders; Bull 2, 7 (2 of them wrecks), 3; Wing 3, 5, 3 | 10 to 12 |
| D3 | 7 bronze-veined, 1 wheeler, 1 mender | 9 |
| Side paths and the trial | 2 chained (the Rocks), Kedalion and his mender, 2 bulls, 6 sown men | 11 |
| **All** | | **40 to 42, the handmaiden and the god** (Fire: 46) |

29 to 31 on the main path (Fire: 40). Fewer bodies because the giant is a constant extra threat and its throws stand
in for an enemy in most open rooms.

---

## 17. Questions for the owner

**Answered (owner, 2026-11-19):**

1. **The trial: none of the first three.** The owner chose a new one, **the bronze birds**, from Heracles' sixth
   labour. It replaces the Yoke of the Bronze Bulls; section 8 is rewritten to match.
   - A flock of birds with bronze feathers circles a marsh foundry and rains feathers down like darts, shown by
     their shadows on the ground.
   - Bronze gongs stand round the arena. Ringing one scares the nearest birds down where they can be struck, but
     draws the flock's next volley.
   - In the myth Heracles drove the birds into the open with bronze clappers the smith god made.
2. The maze reads by bronze leaves, grooves, plaques and Ariadne's thread.
3. The giant's sight is a real light from its visor.
4. The maze shifts once a lap: three anvil rings, then three stamps.
5. The heart is sealed in every layout.
6. The first sight: the giant is heard, then seen killing at the E cradle.
7. The E and S cradles can be tipped.
8. The chariot is the smith's own, never used.

The rest of the plan stays Proposal (Claude) until the owner walks it.

Each has the recommended option first.

1. **The trial.**
   - (a) **The Yoke of the Bronze Bulls** (section 8): stagger and yoke two bronze bulls, using the giant's throws as
     the tool, then turn the sown men on each other with a stone. *Recommended*: it tests the land's own verb in
     reverse, as Sen's Fortress's best moment is when the player turns the boulders on the fortress's own enemies; and
     it is a set piece nobody else's land can have.
   - (b) **The full round**: walk a whole lap of the Round with the giant, lighting the four corner braziers in turn.
     Pure watching; weaker because the owner's trial is "a harder mix of enemies or a mini-elite", and this has no
     fight.
   - (c) **The Cyclopes' last furnace**: waves of automata in the undercroft while keeping a furnace fed. Familiar
     survival waves; it would fit any land.
2. **How the labyrinth reads.**
   - (a) **Bronze leaves against marble walls, grooves on the floor, plaque tables readable from above, and Ariadne's
     blue thread** behind the player (4.4). *Recommended*: the Water Temple's trouble was not knowing which state the
     dungeon was in; the remake's in-world coloured guides fixed it without changing the layout, and the thread is the
     myth's own answer to being lost in this maze.
   - (b) The same without the thread: purer, more to remember, more getting lost.
   - (c) A map screen that fills in as the player walks: certain, but it takes the eyes off the giant, which is the
     land's verb.
3. **How the giant's sight is shown.**
   - (a) **A real light from its visor, with real shadows from walls** (1.3). *Recommended*: Phantom Hourglass made its
     patrolling Phantoms fair by showing where they look and drawing safe zones; a light that casts shadows shows both
     in the world itself, and the lee is exactly the cover.
   - (b) A drawn cone on the ground, like a sentry's view cone: clearer at a glance, but it ignores walls unless it is
     cut by them, and then it is the light again.
   - (c) No shown sight: only the bell and the falling stone's shadow warn. Harsher; the clock becomes guesswork.
4. **How often the maze shifts, and its warning.**
   - (a) **Once a lap (150 s) at the Tread: three rings of the anvil, then three stamps** (4.2). *Recommended*: one
     known cause and few states is what makes a shifting place learnable (Dark Souls' Duke's Archives: two stair
     positions and one visible lever; in *The Maze Runner* the walls move on a schedule the runners map). It also
     teaches the sound the smith uses to call the giant in the fight.
   - (b) Quarter by quarter as the giant passes each side: four small shifts a lap. More often, more to track, and the
     plaque would need four clocks.
   - (c) Once a lap, and a stone wedged in the Tread holds the layout for one lap. A player tool over the clock; it
     could make the maze trivial once found.
5. **The heart.**
   - (a) **Sealed in every layout; reached only down the Pour Way after the handmaiden** (section 7). *Recommended*:
     the goal is shown early (the plaque's "no road to it") and earned late, the soulslike way; it keeps the maze to
     three hubs and gives the god's approach a straight, quiet walk of its own.
   - (b) One layout opens a long way in: more labyrinth, a fourth state for the plaque, and the elite becomes optional.
6. **The first sight of the giant.**
   - (a) **Heard from waking, its light met on the terraces, then revealed at the E cradle, where its first throw
     crushes a chained who kept moving** (1E). *Recommended*: Dark Souls shows its drake over the Undead Burg's bridge
     in a set moment before it ever threatens the player there; a reveal that teaches the rule by showing it done to
     someone else is clear and costs the player nothing.
   - (b) Its shadow at the Landing in the first minute: earlier, but the first frame already carries three new things.
   - (c) No scripted moment: met wherever it is. Risks the player meeting it first as a stone out of nowhere.
7. **Starving the giant.**
   - (a) **Yes: the E and S cradles can be tipped, quieting that side for a lap until the next rest** (3.2).
     *Recommended*: Sen's Fortress lets the player redirect its boulders with a lever; one lever over a clock turns
     the clock into a machine to master rather than a wall.
   - (b) No: the round cannot be touched, and the clock is only to be read.
8. **The chariot.**
   - (a) **The smith's own, cast and never used, behind his furnace; one lap of the Round, then out along the Sea
     Road, leaping the break where the player woke** (section 10). *Recommended*: it answers his regret line and closes
     the land's first frame.
   - (b) Waiting at the Landing: simpler, and the ride is cut off from the fight.
   - (c) Cast from the exploded giant's bronze: striking, but it takes the explosion's finality away.

**Sources.** The Water Temple and its 3DS guides:
[Wikipedia](https://en.wikipedia.org/wiki/Water_Temple_(Ocarina_of_Time)),
[Zelda Universe](https://zeldauniverse.net/2011/05/14/ocarina-of-time-3ds-improved-water-temple/). Dark Souls 3's
mimic tells: [Fextralife](https://darksouls3.wiki.fextralife.com/Mimic). Sen's Fortress (the boulders, the worn
stairs as a clue, the lever): [Wikipedia](https://en.wikipedia.org/wiki/Sen's_Fortress). Phantom Hourglass's Temple
of the Ocean King (safe zones, sight cones, patrol timing): [Zelda Dungeon
wiki](https://www.zeldadungeon.net/wiki/Temple_of_the_Ocean_King). From memory, not looked up this session: Dark
Souls' Undead Burg drake and Duke's Archives stairs; *The Maze Runner* (2009); the walls cutaway of The Sims and
Diablo; Hephaestus's dogs (*Odyssey* 7), his golden maidens (*Iliad* 18), Kedalion and Orion, the Khalkotauroi and the
sown men (Apollonius, *Argonautica* 3).
