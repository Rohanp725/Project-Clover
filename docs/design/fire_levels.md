# The Fire land's levels (second pass)

Written by Claude on 2026-11-17 as the plan that `fire_rework.md` section 4 points to. **Everything here is
Proposal (Claude)** until the owner has walked it. It replaces the layout of `SunCity` (the three districts, the two
side paths, the Champion's gate) and gives the `SunCourt` forecourt an approach. Enemy rules are `fire_rework.md`
sections 1 to 3; their numbers are `fire_land.md` section 4. Lore objects are `fire_land.md` section 7; every one of
them keeps a place below.

Axes as before: x east, z north, y up; north is up the screen. Coordinates are metres. "Floor y 6" is the walking
height. The player wakes at (0, 0, 4).

---

## 1. Principles

### 1.1 What the camera sees (measured from the code)

`TopDownCamera`: pitch 55 degrees, distance 12 m, vertical field 40 degrees (`PlayerBuilder`), widened only below
16:9. So the camera stands 9.8 m above and 6.9 m south of the player, and on flat ground the picture is a trapezoid:

| Edge of the picture | Ground distance from the player | Width of ground seen |
|---|---|---|
| Bottom | 4.3 m south | 13 m |
| At the player | | 15.5 m |
| Top | **7.2 m north** | 22 m |

The frames bear it out: the 5 m Street of Lamps fills a third of `suncity_wicks_0.png`, a wick 6 m ahead stands at the
top edge, and the hall's 10 m column spacing spans the whole height of `suncity_hall_2.png`.

What follows from it, and is held to everywhere below:

1. **Nothing beyond about 7 m north is ever on screen, whatever its height.** The top ray is 35 degrees below
   the horizon; it reaches the ground 7.2 m ahead. A thing standing *above* the player's floor is seen only while it
   is close: something H metres up is in frame only within (5.0 - H) / 0.7 m north. A ledge 2 m up shows its edge
   from 4 m away and nothing from further. So **landmarks cannot be seen from far off.** They work in two ways:
   - *A landmark is a room you arrive in.* The barque, the oven, the gate, the colossus are shaped so that the
     player's first frame of them is the whole of them, or the part that matters.
   - *Vista zones.* At five places a trigger asks the camera for a framing (`TopDownCamera.SetFraming`, which exists):
     1.5 times the distance, pitch 10 degrees flatter, looking 6 m ahead. That shows about 20 m ahead for as long as
     the player stands on the spot (a stair head, a terrace edge), then eases back. Named V1 to V5 below.
2. **The land rises northward**, from the riverbed at y 0 to the hall at y 16. Every rise then faces south, toward
   the camera, where its face is drawn in the grey-violet shade colour with an ink edge: a terrace face 2 m high shows
   as a band about 1.2 m of ground deep on screen, unmistakable. A face that looks north is never seen, only its lip
   and a shadow 0.21 times its height (the sun is at 78 degrees: a 3 m drop casts 0.6 m). **Drops therefore face
   south**: the player jumps down toward the camera, and one-way drops lead back south, toward fires, which is where
   shortcuts go anyway.
3. **South-side rule** (kept from the castle): nothing more than 1.4 m above a walkable floor stands within 4 m south
   of it, unless it fades (`Box` gives `Fadeable`; kit pieces with `solid` too). East-west streets have their south
   side as broken walls 1.4 m high, burnt-out ruins, or one-storey houses whose roofs fade. A rising land mostly keeps
   this rule by itself: the ground to the south is the lower terrace.
4. **Roofs.** A roof a player walks under is drawn shadow-only (`ShadowCastingMode.ShadowsOnly`, as the hall is) or
   fades; the camera sees under it, the floor shows its shade. A walkable roof is a floor like any other and its
   parapet on the south side is at most 0.6 m.
5. **Enemies walk straight at the player** (`Enemy` moves a `CharacterController` with gravity; there is no navmesh).
   Every encounter is placed so that the straight line from each enemy to where the player will be is open, or so
   that a wall in it is the point (the ledge they drop from, the wall the thirsting cannot cross). Enemies do fall off
   ledges, which the spearman's leap and the deck thirsting use.
6. **Ranged attackers stay in frame.** A priest throws from 18 m; placed north of the player he fires from off
   screen. Priests and anything that throws stand east or west of where the player will fight (the frame is 15 to 22 m
   wide) and at most 6 m north, never on a ledge more than 1 m up.

### 1.2 Measures

| What | Measure | Why |
|---|---|---|
| Walk, roll | 6 m/s; roll 4.5 m in 0.35 s (fists; the spear's is 4.8) | `PlayerController.MoveSpeed`, `DodgeRoll` |
| A one-on-one corridor (thirsting, wick) | 3.5 to 5 m wide | Room to guard and to roll sideways out of a 1.6 m grab lane, no room to circle |
| A gilded duel | 6 m wide at least | Thrust lane 4.4 m by 1 m, sweep 3.4 m at 170 degrees: a sidestep must clear the lane with a step to spare |
| The shield wall's ramp | 7 m wide | Three levies at 2.5 m spacing fill it; one step off the side is the flank |
| A group fight | 12 to 16 m across, 10 to 12 m deep | Fits the 15.5 m width of the picture; nobody attacks from off screen |
| A rise that reads | **1.5 m** facing the camera; **2 m or more** for any edge that must read as "a drop" | 1 m reads only as a kerb |
| A climbable step | 0.3 m (`CharacterController` step offset) | Anything taller is a wall to the player and to enemies |
| Stairs | 27 to 32 degrees (rise 1 over run 1.6 to 2), steps 0.22 m (`Stairs` makes them) | Steeper reads as a ladder and plays as a choke |
| Ramps fought on | at most 8 degrees (1 in 7) | Marks lie on the slope; footing reads as level |
| Ramps only walked | at most 15 degrees | |
| A ledge an enemy leaps from | 2 to 4.5 m above the floor below, within 4 m north of where the player stands when it leaps | Seen as it jumps, by the mark on the floor first |
| A shade island | at least 3 by 3 m (a few are door recesses 2 by 1.5 m, marked as such) | The player plus one corrective step; an enemy can follow in |
| Distance to shade | **no point of a fight space more than 15 m from shade; islands no more than 25 m apart along the walk** | The warning is 5 s: 30 m of running. A player in a fight needs about 2 s to break off, leaving 3 s, 18 m |
| A swell | 5 s warning, 15 s burning (`fire_rework.md` 2); as being built, `NoonHeat` bites 3 every 0.8 s, so a whole swell in the sun is 56 of 100 health and each second late to shade is about 4 | Late by 3 s is a hit; caught in the open is half a life |
| Swell rhythm | D1 every 100 s (first one placed by trigger); D2 every 70 s; D3 every 50 s; hall and champion none (under roof, and the elite's ground is exempt); the forecourt's approach every 40 s | Rarer first, hardest last; the court is its own rule |

Shade is registered as rectangles (`Sunlight.AddShade`, open sky). The hall, now under an open-sky scene, registers
its shade as the rectangles *between* its light bars (shade wins over light, so the old closed-sky `AddLight` list
cannot be reused as is; see the build list). Every rectangle below is (west, east, south, north) and already includes
the lean of the noon shadow (a roof h above the floor shades the floor `LookBuilder.NoonShadowLength(h)` = 0.21 h
north of its footprint).

---

## 2. The whole land

The land climbs out of the dead river, up through the city, up the king's terraces to his hall and his gate. Six
rest fires, five in `SunCity` and F6 in `SunCourt`. The main path is about 560 m of walking (the first pass's was
about 520), most of it no longer in a straight line.

```
 z
526 ...........................[ GATE OF NOON ] y16  -> SunCourt (its own scene)
       CHAMPION'S BOWL  floor y14.5, rim y16      (gold line z488)
488 ......................... porch y16  F5 * ....................
478 +---------------- HALL OF COLUMNS y16 (aisles y17.2) -------+
    |  [sun door W] --S4--> west passage y17.2/16 ---.          |
418 +---------------------- PYLON  z410..418 -------------------+ |
    |  UPPER TERRACE y16  warden's court, caller's dais         | |
386 +========= spearman's stair 13->16 =========================+ |
    |  MIDDLE TERRACE y13  masons' yard, the unfinished colossus| |
    |  [S3 masons' gate W]<-------------------------------------'(S4 lands here, west portico)
346 +========= retaining wall 3 m; LEVY RAMP up the middle =====+
    |  LOWER COURT y10 + porticoes                              |
318 |  AVENUE OF THE SEATED KING y10        S3 haul-ramp (W) |   |
280 |  F4 *                                    down to F4  <-'   |
272 +==== yard's north gate (from gallery y10) ===========+
    |  EMBALMERS' YARD y8, north gallery y10      [tomb stair W] --> PAINTED TOMBS y4 (side)
226 +==== EMBALMERS' GATE y8  F3 * spear ====+
214 |  MARKET upper terrace y8  /  lower y6   |   rooftops y10 (side, east)
176 |  [S2 barred door] OIL ALLEY y6  <-- Lamp Street leg 2 y6
152 |  lane y4 |   LAMP SQUARE y6 / stair 4->6
134 +== QUAY y4  F2 * ==== boat-sheds under its face ==========+
116 |  HARBOUR BASIN y0                                         |
100 |  [ramp up W] |  THE BARQUE'S BEND y0 (deck y3.5)   |east  |
    |  FISHERS'    |  THE BOAT JAM y0                     |levee |
 62 |  BANK y3     |  THE PANS y0 (buried thirsting)      |y3    |
    | [well gully W -> SEALED WELL y1 (side)]             |      |
 18 |  [S1 ladder] |  THE NARROWS y0  F1 *                |      |
 -6 +------------- river mouth wall 1.2 m -----------------------+
      x -40              x 0                               x 36
```

**Heights, south to north:** riverbed 0; banks 3; quay 4; lower lane 4; Lamp Square, alley, lower market 6;
upper market, gate, yard 8; yard gallery 10; avenue and lower court 10; middle terrace 13; upper terrace, hall,
porch 16; hall aisles 17.2; bowl 14.5; rim and gate 16. Walkable roofs: city 10, scaffold decks 16 and 19.

**Rest fires (six).**

| Fire | Where | Foot | Why there |
|---|---|---|---|
| F1 | The Narrows, under the fishers' awning | (2.5, 0, 9) | In sight of waking; the first shade |
| F2 | The quay, west, under the harbour-master's awning | (-22, 4, 127) | Relief after the basin; S2 comes out 6 m from it |
| F3 | The Embalmers' Gate passage, west side | (-4, 8, 220) | Beside the spear, as built |
| F4 | The foot of the avenue | (-6, 10, 280) | Before the gilded; S3 comes out 12 m from it |
| F5 | The porch north of the hall | (8, 16, 482) | Before the champion, as built |
| F6 | The court's forecourt (in `SunCourt`) | (-5, 0, -30) | As built, moved 5 m west to clear the new stair head |

**The route.** Bed north through the Narrows, the Pans, the Jam, round the barque, through the basin and up to the
quay (F2). Up the Lamp Street, which climbs and turns east, then north; west along the Oil Alley into the market;
up to its terrace and through the Embalmers' Gate (F3); across the yard and up onto its gallery; out to the avenue
(F4). Up the Levy Ramp to the middle terrace; up the spearman's stair to the warden's court; through the pylon and
the hall; the porch (F5); the bowl and the champion; the gate.

**Branches and loops.**
- The Fishers' Bank (side path and return loop): basin to well to F1.
- The barque's deck (a high branch): gangplank up, prow drop down into the basin.
- The rooftops (a high branch): from the Lamp Street, over the houses, dropping into the market's upper terrace.
- The breakable wall in the Lamp Street (secret): the one hearth they missed.
- The painted tombs (side path), down from the yard.
- The mirror and the sun door in the hall (puzzle branch): it opens S4.

---

## 3. The districts

Each section gives the plan, then each space in walking order. Enemy kinds are the rework's names: thirsting,
buried thirsting (a thirsting under a mound, `MakeDormant` plus the burst), wick (burning, standing), dormant wick,
ashen, gilded levy, spearman, warden, sun-blind priest.

### 3.1 District 1: the dry river (z -6 to 134)

**The idea.** A river that wound, not a ditch: the bed swings west round the Pans, is choked by a jam of boats,
swings east round the barque, and opens into the harbour basin under the quay. Its banks are 3 m of crumbling brick
and dried mud, high enough that the bed always reads as *down*. The bed is the thirsting's ground: open mud with
mounds in it that breathe dust. The west bank is the fishers' quarter, a parallel walk in shade that the player
only reaches from the far end and that loops home.

```
 z    x: -40  -30  -20  -10    0   10   20   30
134   +========================================+  QUAY y4 (z116..134)   F2*(-22,127)
      |  F2*  awning      C1 stone(7,118)  crane(18,120)
116   +--[shed]--[shed]---###stair###--[shed]--[shed]--+   quay face 4 m, boat-sheds in it
      |ramp|      (t)buried      cistern       (t)   |   BASIN y0, x -26..24
104   |up W|    jars        (t)  O(0,104)          |
100   +----+---------.            ______  prow     |  <- prow drop 3.5 m into basin
      |  BANK y3      \  (t)b    / BARQUE \ (t)    |  levee y3
 88   |  huts, racks   \ west   /  deck    \ east  |
      | gully W         |chan. | (t)y3.5  |chan.   |
 80   |<- well          \      \__________/        |
 74   |                  \  (t)  (t)              /
 70   |                   \ [JAM]gap  .----------'     the Jam: boats piled x-16..-6, gap x-6..-2
 64   |                    \__________/
 58   |  huts         .---'  (t)b jar (t)b        |
 46   |          .---'       Pans y0              |
 34   |    .----'   [boat]   rack                 |
 26   |   /   (t)k pool                            |
 22   |  '---.  rack                               |
 18   |      [S1]|  F1*  awning   THE NARROWS     |
  4   |          |   @ wake    x -5..5             |
 -6   +----------+-- mouth wall 1.2 m ---+---------+
```
`(t)` thirsting standing; `(t)b` buried; `(t)k` kneeling; `O` cistern; `[S1]` the ladder.

**The bed's outline** (west edge / east edge at each z; straight between; floor y 0; outside it the banks at y 3):

| z | -6 | 16 | 22 | 30 | 46 | 58 | 64 | 72 | 80 | 96 | 100 | 116 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| west | -5 | -5 | -12 | -18 | -22 | -20 | -16 | -16 | -10 | -6 | -26 | -26 |
| east | 5 | 5 | 6 | 7 | 4 | 0 | 0 | 4 | 16 | 20 | 24 | 24 |

The bank faces are the "Bank" piece of the build list (3 m, leaning back 12 degrees, an edge that wanders 0.5 m,
dried-mud strata, broken brick facing in patches, slumps of fallen mud at the foot every 10 to 15 m). The south
mouth wall (z -6, x -5 to 5) stays 1.2 m.

#### 1A. The Narrows (z -6 to 18, x -5 to 5, y 0)
- *Shape:* a cut 10 m wide between 3 m banks, the tightest the land ever is. The player wakes looking north into
  the bed widening ahead; the banks' faces either side are the first thing that teaches "height".
- *Sees on entering:* F1's brazier under a reed awning on the west, a reed boat lying tilted on the east, the bed
  opening out of the top of the frame. V1 (vista) at (0, 0, 15): the camera eases back and shows the Pans to z 35.
- *Enemies:* none.
- *Shade:* the fishers' awning, posts on the bank top, mat cantilevered over the bed: (-5, -1, 7, 12). F1 stands
  in it.
- *Swell:* none here.
- *Lore:* none; the sponsor speaks on waking.
- *Kit:* `brazier` (F1), `reed_boat` at (3.5, 0, 14) yaw 200 tilted, `market_awning` as the fishers' awning at
  (-3, 3, 9) yaw 90 (its mat over the bed), `mud_tile` floor.
- *Built:* the bank faces; the ladder S1 at (-5, 0, 13) lying on the bank top until kicked down (build list B6).

#### 1B. The Pans (z 18 to 62, the bed 24 to 29 m wide, y 0)
- *Shape:* the bed swings west and opens: a wide pale floor of cracked mud with silt pans, three water jars, an
  old boat on trestles and two fishers' drying racks. Open on purpose: it is where the thirsting lie under the
  mud, and where the first swell comes.
- *Sees on entering:* the kneeling thirsting at its dry pool, its back turned, 4 m ahead and a little west.
- *Enemies:*
  - E1.1 thirsting kneeling at the dry pool, (-3, 0, 27), facing north (`MakeDormant(3, "thirst_kneel_loop")`). The
    free first blow and the first finisher, as before.
  - E1.2 two buried thirsting, mounds at (-12, 0, 43) and (-5, 0, 48), either side of an intact water jar at
    (-8, 0, 45). Walking within 3.5 m bursts a mound. A player who breaks the jar (one blow) from outside 3.5 m draws
    both up to drink, helpless for 1.6 s each. The lesson is in the ground: the mounds breathe dust.
- *Why it suits them:* the buried need open mud that looks safe; the jar needs to be seen before the mounds are,
  so it stands on the walking line and the mounds are off it by 3 m either side.
- *Shade:* rack (-8, -4, 24, 27); the boat on trestles (-16, -11, 38, 41); rack (-6, -2, 52, 55). The widest open
  stretch is z 27 to 38, 11 m.
- *Swell:* **the first swell of the game** is forced when the player crosses z 31 (a trigger), whatever the clock.
  The boat on trestles is 8 m ahead and in frame. Status line once: "The sun swells. Find shade." Then D1's 100 s
  rhythm starts from it. A buried thirsting goes wild in a swell: its mound bursts at 6 m, not 3.5.
- *Lore:* none new; the dry pool stain (as built) at E1.1.
- *Kit:* `mud_tile` floor, `reed_boat` on trestles (two `Block` trestles 1.6 m), two `market_awning` as racks,
  water jars (cylinders, as the built `Jar`, 0.9 m) made breakable (build list B9).

#### 1C. The Boat Jam (z 62 to 74)
- *Shape:* the bed narrows to 16 m and is choked: three reed boats piled across it with a heap of dried mud, from
  the west face to x -6, 1.4 m high (low: the camera sees over it), and a broken mast lying from (-2, 0, 66) to
  (0, 0, 70). The way through is **a gap 4 m wide, x -6 to -2**, at z 66 to 70. A chokepoint with an ambush behind.
- *Sees on entering:* the pile and the gap; the tipped great jar just beyond the gap at (-4, 0, 73), its dark stain.
- *Enemies:* E1.3 two thirsting standing behind the pile, at (-10, 0, 71) and (-1, 0, 73), facing the gap. They
  take turns (as built). Through the gap the player meets them one at a time; the red pounce comes down the gap's
  line, so stepping out of the gap's mouth sideways is the answer.
- *Shade:* the lean-to on the north side of the pile, (-15, -8, 68, 71): it is behind the enemies, which is the
  point. A swell here asks the player to push through, not back.
- *Lore:* none.
- *Kit:* three `reed_boat` (yaws 70, 115, 160; tilted 20 to 35 degrees, stacked), `Rubble` heap; the great jar
  (as built: a clay cylinder on its side) moved to (-4, 0, 73).

#### 1D. The Barque's Bend (z 74 to 100)
- *Shape:* the bed swings east, 26 m wide, and the barque lies stranded down the middle of the bend: centre
  (4, 0, 88), yaw 200, scale 0.73 (as built), heeled 8 degrees east so its deck tips toward the camera. It splits the
  bed into a **west channel** (x -10 to -1, 9 m wide) and an **east channel** (x 10 to 18). The main way is the
  west channel; the hull hides nothing there because the hull is east of the player.
- *Sees on entering:* the stern posts and the gangplank, a reed ramp 3 m wide from (1, 0, 72) up to the deck at
  (2, 3.5, 78).
- *The deck (branch):* walkable at y 3.5, 4.5 m wide, from z 78 to 98 (build list B4: a deck floor and rails).
  Cabin awning on the deck at (4, 3.5, 88). At the prow, C2, and a drop of 3.5 m off the prow's north end into the
  basin at (6, 0, 101): one-way, lands behind the basin's thirsting.
- *Enemies:* E1.4 three thirsting:
  - one on the deck at (4, 3.5, 90) that drops on a player in the west channel when the player is within 6 m of the
    hull: its pounce lane begins on the deck and it lands on the mud (`thirst_pounce` from a ledge, build list C3).
  - one buried in the west channel at (-6, 0, 86) beside a water jar at (-8, 0, 84).
  - one standing in the hull's shade off the prow's east side at (11, 0, 97). The hull blocks its straight line to
    most of the west channel, so it comes round the prow when the player reaches the channel's north end (z 92 or
    more), and it is the one waiting below a player who takes the deck and drops. This is where a hurt player first
    drinks: the snatch.
- *Shade:* the hull's heeled overhang over the east channel (9, 11, 80, 96); the deck cabin (2, 6, 86, 91); a sail
  lying over two spars in the west channel (-9, -5, 92, 95).
- *Lore:* C2 at the prow, (6.5, 3.5, 97.6) on the deck (as built text).
- *Kit:* `barque` 0.73 (solid hull as built, the deck added), `reed_boat` as the gangplank's side, sail as a cloth
  slab on two `wood` spars.

#### 1E. The Harbour Basin (z 100 to 116, x -26 to 24, y 0)
- *Shape:* a squared harbour inside the curved river, the only regular shape in the district because it was built:
  stone floor under the mud near the quay, the dry cistern at (0, 0, 104), jars stacked at the west, and the quay's
  4 m face to the north with **four boat-sheds** cut into it (arched cells 4 m wide, 3 m deep, 3 m high, at x -18,
  -8, 8, 18). The quay stair is in the middle, x -3 to 3, from (0, 0, 108) to (0, 4, 116): 8 m run, 27 degrees.
- *Sees on entering:* the cistern, two thirsting at it, the stair beyond. V2 at the stair head (0, 4, 118): the
  camera shows the quay and the Lamp Street's mouth.
- *Enemies:* E1.5 three: standing at (-5, 0, 104) and (5, 0, 106) round the cistern, and one buried among the jars
  at (-16, 0, 111). The land's first crowd: do not drink here, two of them are within 8 m of anywhere.
- *Shade:* the four boat-sheds, each (x - 2, x + 2, 113, 116): a player can back into one and fight one at a time
  (the shed's mouth is 4 m: a natural choke). The quay's arcade is the reward for a swell caught in the open.
- *Lore:* none (C1 is on the quay).
- *Kit:* `Disc` cistern (as built), jars, `mud_tile`, stone paving near the face; the boat-sheds and the quay face
  are built (build list B5).
- *Out:* up the stair to the quay (F2); or west, up the ramp onto the Fishers' Bank: from (-23, 0, 113) south to
  (-23, 3, 101), 12 m, 14 degrees, along the basin's west wall.

#### 1F. The quay (z 116 to 134, x -36 to 30, y 4)
- Stone, paved (as built). F2 at (-22, 4, 127) under an awning (-25, -19, 125, 130). C1 on a mooring stone at
  (7, 4, 118.6) (as built text). The timber crane at (18, 4, 120) over the basin. The Lamp Street opens north at
  x -2 to 3, z 134; S2's lane comes out at x -30 to -27. No enemies: the quay is relief.

#### Side path: the Fishers' Bank and the sealed well (west bank, y 3, and the well court, y 1)
- *Shape:* the bank top is 10 to 30 m wide, x -40 to the bed's edge, z -6 to 100: packed earth, four fishers' huts
  (`house_block` single-storey) at x -34 to -28, z 24 to 56, drying racks, nets on poles, a path along the edge
  where the bed is always in view below. At z 74 to 80 a gully cuts west through the outer wall: a stair down
  (x -40 to -46, rise -2 m) into the **sunk well court** x -66 to -46, z 64 to 90, floor y 1 (2 m below the bank,
  walls 3 m on north and west, 1.4 m on the south). The well (`well_shrine`) at (-56, 1, 80).
- *The loop:* walking south from the basin ramp, past the gully, to the bank's south end over the Narrows, where a
  ladder lies on the bank top at (-6, 3, 13): kicked down, it reaches F1. **S1.**
- *Enemies:* one buried thirsting on the bank path at (-24, 3, 60) by a fishers' water jar; the two kneeling
  thirsting at the well's lid, (-58.5, 1, 77.6) and (-54, 1, 76.4), `MakeDormant(5, "thirst_kneel_loop")` (as built).
- *Shade:* hut eaves (-34, -28, 26, 28) and (-34, -28, 48, 50); the well court's north wall throws 0.6 m, too
  thin; a reed canopy over the offering table (-58, -54, 72, 75).
- *Lore and gifts:* C3 at the lid (as built text), the waterskin on the offering table (fourth flask, as built).
- *Minutes:* 3 for a new player, the loop home included.

### 3.2 District 2: the city of wicks (z 134 to 272)

**The idea.** The city is built up the slope above the harbour, so it is climbed: a street that turns as it climbs,
an alley of oil, a market on two levels, a gate, a yard with a gallery. Narrow where the wicks are, open where the
gold comes. A roof route runs over the streets for those who look for ladders.

```
 z     x: -40  -30  -20  -10    0    10   20   30
272   +=========== yard north gate (0,10,272) -> avenue ==========+
266   |  gallery y10  [racks of the standing dead]  {w}  gallery  |
258   |  ##stair##        YARD y8                     ##stair##   |
250   |<= tomb  {w}table   (w)      natron  [table]               |
      |   stair W     (w)         [table]           shed           |
226   +======= EMBALMERS' GATE x-6..6, y8  F3*  spear  C6 ========+
216   |      MARKET UPPER TERRACE y8   (a)                        |
206   |  ramp  ....... ##stair 6->8## .......  | east arm y8 <-- drop from roofs (y10)
196   |  W    (w)        oven O    awning      |#####################
186   |  (t) jars   (w)    MARKET LOWER y6     |# rooftops y10 #####
180   |[S2 door]  ......................... OIL ALLEY y6 x-10..14 ...  (a)
176   |  lane    |  burnt houses (1.4 m walls)    | leg2 |# east block #
      |  y6->4   |                                | (a)  |# ladder up  #
166   |  lane y4 |                     bridge-house>| x9-14|
160   |          |  LAMP SQUARE y6  x-2..14  {w} awning   |
152   |          |####|##stair 4->6##|#####################
142   | lower    |[cracked wall]| LAMP STREET leg1 x-2..3 y4
134   +=lane exit==========QUAY y4==================================+
```
`(w)` wick; `{w}` dormant wick; `(a)` ashen; `(t)` thirsting.

#### 2A. The Lamp Street (z 134 to 176)
- *Shape:* leg 1 north from the quay, 5 m wide (x -2 to 3), z 134 to 148 at y 4, houses 4 m either side; a stair
  of 2 m rise at z 148 to 152 (4 m run would be 27 degrees: use 4 m); the **Lamp Square** at y 6, x -2 to 14,
  z 152 to 160, an irregular little square with a dry fountain at (6, 6, 156); leg 2 north again, 5 m wide (x 9 to
  14), z 160 to 176 at y 6. A **bridge-house** spans leg 2 at z 166 to 170 (an upper room on two walls; its floor
  4 m over the street, drawn shadow-only from below).
- *Sees on entering:* the gatehouse arch at the quay mouth (z 134 to 137, roofed, shadow-only), a wick in the street
  8 m on, the lamps.
- *Enemies:*
  - E2.1 a wick at (0.5, 4, 144) facing south: the swipe can be guarded, the grab cannot. And a dormant wick in a
    doorway on the square's west side at (-1.4, 6, 156), wake 3.5 m: doorways hold hands.
  - E2.2 **the first ashen**, alone, in leg 2 at (11.5, 6, 172), facing south. A 5 m street with nothing else in it
    is where its swell and burst are learned: back off, or kill it and step away.
- *Why it suits them:* wicks are slow; a 5 m street makes their grab lane the whole width. The ashen needs a street
  long enough (16 m) that backing away from it works.
- *Shade:* gatehouse arch (-2, 3, 134, 137.6); Lamp Square awning (8, 12, 155, 158); bridge-house (9, 14, 166, 170.8).
  Doorway recesses 2 by 1.5 m at (-2, -0.5, 140, 142) and (3, 4.5, 145, 147).
- *Swell:* 70 s rhythm from entering the district.
- *Lore:* C4 at a hearth niche by a door on the square, (−1.25, 6, 158) (as built text); lamp niches every 6 m
  along leg 1's west wall and the square (two lit).
- *Kit:* `house_block` both sides (the south side of the square is leg 1's houses: they fade), `house_doorway`
  for every door with a dormant wick, `brazier` none, lamp niches as built.
- *The secret:* leg 1's west wall at (-2, 4, 141) is **cracked** (a breakable wall, build list B8): a heavy blow, or
  an ashen bursting within 2 m, opens it on a small house, x -10 to -2, z 138 to 146, roofless (seen from above),
  where the hearth niche still has its name. Words (Claude's, new): "A fire in every house, and it is one fire."
  / "In the morning Ahu lights the day from it." / "This one they missed." Five embers in the hearth.
  (*Ahu* is a placeholder for the old god's name, for the owner to choose; it must not be a real deity's name.)
- *The roof route:* a ladder on leg 2's east wall at (14.2, 6, 163) climbs to the east block's roofs (y 10, x 14 to
  30, z 152 to 200). Walk north and west across roofs (low parapets, roof walls, hatches as `Mass` makes) over the
  Oil Alley's north houses (x -10 to 14, z 180 to 196, roof y 10) and drop 2 m off their north edge onto the
  market's east arm (y 8) at (-2, 8, 198). One dormant wick lies on a roof at (20, 10, 186) among drying linen; one
  oil jar beside it. A side route: it skips the Oil Alley and lands behind the market's ashen.

#### 2B. The Oil Alley (z 176 to 180, x -10 to 14, y 6)
- *Shape:* an alley 4 m wide, 24 m long, running west. North side: houses (roof y 10) with three door recesses,
  and **oil jars** along the wall, eleven of them, in clusters. South side: the **burnt houses**, roofless shells
  with walls 1.4 m high, x -10 to 9, z 160 to 176, their floors black: you see into them over the wall. The alley's
  west end (x -10 to -6) is roofed and opens into the market.
- *Sees on entering:* turning west out of leg 2, the alley ahead, jars glinting, a wick walking toward you and an
  ashen stumbling behind it.
- *Enemies:* E2.3:
  - a wick at (-2, 6, 178) walking east.
  - an ashen at (-7, 6, 178.4) behind it.
  - a second ashen standing in the middle door recess at (4, 6, 181), stepping out when the player passes 3 m.
  The lesson: kill the ashen beside the wick, not beside the jars or yourself. An ashen's burst lights jars within
  its ring; a lit jar bursts into burning ground 2 m round for 6 s and lights any wick it touches.
- *Why it suits them:* the narrowness means a burst cannot be sidestepped, only outrun; the jars make its fire
  spread; the burnt houses show what happened here and let the camera in.
- *Shade:* the three door recesses (-4, -2, 180, 181.5), (3, 5, 180, 181.5), (9, 11, 180, 181.5); the roofed west
  end (-10, -6, 176, 180.8). A swell in the alley sends the player into a recess while the wick comes: that is the
  intended bad moment.
- *Kit:* `house_block`, `house_doorway`, `wall_broken` for the burnt houses' walls, oil jars (build list B9).

#### 2C. The Market of the Cold Oven (lower y 6, x -34 to -10, z 176 to 204; upper y 8)
- *Shape:* a market on two levels. **Lower**: an irregular open floor 24 by 28 m, the cold oven at (-16, 6, 196),
  three awnings, stalls, water jars, the thirsting's drinking place at the west. **Upper**: an L-shaped terrace 2 m
  up along the north (x -34 to 4, z 204 to 214) and down the east (the east arm, x -10 to 4, z 196 to 204), with
  a broad stair in the middle of the north edge, x -11 to -5, z 200 to 204 (2 m over 4 m), and a cart ramp on the
  west, x -34 to -30, z 192 to 204 (2 m over 12 m). The face of the upper terrace is 2 m of brick facing south: it
  reads as the next level from the moment the alley opens.
- *Sees on entering:* from the roofed end of the alley, the oven's dome and two wicks either side of it, the terrace
  face beyond with its stair.
- *Enemies:* E2.4:
  - wicks at (-18, 6, 186) and (-24, 6, 196).
  - a thirsting at the water jars (-29, 6, 184), drinking (it heals from the jars while they last: three jars).
  - an ashen on the upper terrace at (-6, 8, 210) that comes down the stair when the fight starts.
  The two corruptions together, as before (a held player cannot guard a flask), and an ashen arriving into a fight
  that already has two wicks in it: let it burst among them.
- *Shade:* awnings (-20, -16, 190, 193.6), (-30, -26, 198, 201.6), (-14, -10, 182, 185.6); the oven's north side
  (-18, -14, 198, 200); the upper terrace's colonnade along the gate wall (-34, 4, 212, 214). No point of the lower
  floor is more than 10 m from one.
- *Lore:* none new; the grey bread on the oven's shelf (as built).
- *Kit:* `market_awning` x3, the oven (as built sphere, moved), `house_block` round the west and south, stalls as
  `Block`s.
- *Shortcut:* **S2**, a barred door in the market's south-west corner at (-28.5, 6, 176), barred on the market side.
  Opened, it leads down a covered stair (x -30 to -27, z 176 to 168, y 6 to 4) and a lane at y 4 (x -30 to -27,
  z 168 to 134) to the quay, 6 m from F2.

#### 2D. The Embalmers' Gate (x -6 to 6, z 214 to 226, y 8)
- As built (12 m passage, towers 5 m wide and 7 m high, the open doors), moved 26 m south and up to y 8, and
  roofed with a shadow-only roof: the passage is shade, F3 is relief. F3 at (-4, 8, 220); the pinned soldier and the
  spear on the east door at (5.4, 8, 223.4); C6 at (5.1, 8, 219.2); C5 on the yard's lintel at (-5.2, 8, 227.2).
- Shade: (-6, 6, 214, 227).

#### 2E. The Embalmers' Yard (x -22 to 22, z 226 to 272; floor y 8, north gallery y 10)
- *Shape:* not a square: a floor whose south half is the working yard (tables, natron, a shed in the east, drains
  cut in the paving) and whose north is a **gallery 2 m up**, x -22 to 22, z 262 to 272, where the wrapped dead
  stand in racks facing the sun. Stairs up to the gallery at x -20 to -16 and 16 to 20, z 258 to 262. The gallery's
  face is a row of burial niches in the 2 m wall. The way on is the yard's north gate at (0, 10, 272), on the
  gallery.
- *Sees on entering:* through the gate, the tables in two staggered rows, a wick standing at the first, the
  gallery face across the yard with its niches.
- *Enemies:* E2.5 four:
  - burning wicks standing at (-6, 8, 238) and (10, 8, 248).
  - a dormant wick on a table at (-10, 8.4, 250), wake 4 m.
  - a dormant wick in a gallery rack at (14, 10, 266) among bundles that are only bundles.
  **The flame spreads:** a burning wick that walks within 1 m of a dormant one lights it. The two standing wicks
  walk toward the player across the yard, past the table, and if the player hangs back the table wick is lit.
  Kill the walker before it gets there, or lure it the other way. A knocked-over brazier (two on the gallery) lights
  the rack.
- *Why it suits them:* the spear (taken two strides back) outreaches the grab; the yard is wide enough (44 m) to use
  that reach, and the tables make lanes.
- *Shade:* the gallery's portico (-22, 22, 262, 266) at the foot of the gallery wall (roofed at y 12, shadow-only);
  the natron shed (14, 20, 232, 238); table canopies of reed over two tables (-12, -8, 249, 252.6) and (4, 8, 241,
  244.6).
- *Lore:* C5 on the lintel (above). The racks.
- *Kit:* `burial_niche` x8 along the gallery face, `sarcophagus` x2, natron heaps, `brazier` x2 on the gallery at
  (-7.4, 10, 269) and (7.4, 10, 269).
- *Out:* the north gate, onto the avenue at y 10. To the tombs: west.

#### Side path: the painted tombs (y 4, below the yard)
- Down a stair from the yard's west wall, (-22, 8, 250) to (-30, 4, 250), 5 m wide (4 m over 8 m, 27 degrees),
  between brick cheeks. The three chambers are **as built**, moved by (-2, +5, -12): chambers x -58 to -28,
  z 246 to 254, floor y 4; north wall 3.3 m, south wall 0.5 m (camera), dividers as built. Panels, paintings,
  captions, the grave dish (three embers), the two wrapped wicks that rise after the last painting and the scare:
  all as built.
- The tombs are sunk 4 m below the yard: from the stair head the camera looks down into the first chamber.

### 3.3 District 3: the golden way (z 272 to 418)

**The idea.** The king's road is an avenue of his statues leading to a temple built in **three terraces**, each
stepped 3 m above the last with a colonnade along its front, climbed by a ramp up the middle (the shape of the
terraced temples of the river's west bank). From the camera, every terrace face is a band of shade with columns in
it, and every climb is a reveal. The gilded hold it by rank: levies on the ramp, spearmen on the high places, the
warden before the pylon.

```
 z     x: -30  -20  -10    0    10   20   30
418   +=============== PYLON (z410..418) ===============+
408   | portico|   W(warden)                    [dais]P  | UPPER TERRACE y16
398   | W side |  (l)   [SHRINE 6x4 h1.2]              | x -24..24
388   |        |  #plinth#|##stair##|#plinth# (s)y17.5   |
386   +=======[west passage S4 down]=====================+  face 3 m
380   | portico ..............  ....................   |
366   |  [COLOSSUS]  scaffold(s)y16   blocks   awning    | MIDDLE TERRACE y13
356   |   unfinished    decks y16,y19     sledge         |
346   +===[S3 gate W]==== retaining wall 3 m ===|ramp|===+
341   | portico ..........  ..........|ramp|.........   |
330   |  LOWER COURT y10           |(l)(l)(l)| y13 top    |
320   |                            |ramp foot y10         |
318   +---------------------------------------------------+
306   |   [S]      (l)                         [S]       |  AVENUE y10, road x -10..10
290   |   [S]   stall          stall           [S]       |  statues at x +-13
280   | S3 lane | F4*                                    |
272   +======= yard north gate =================+
```

#### 3A. The Avenue of the Seated King (z 272 to 318, y 10)
- *Shape:* a road 20 m wide (x -10 to 10) of pale paving with the two gold lines (as built), sides to x -20 and 20.
  **Two** pairs of statues, not five, at z 286 and 302, at x -13 and 13 so their laps and plinths are in frame (as
  built for x 14.2). The road is not flat: it steps up 0.5 m at z 296 (a single broad step, kerb-high, the whole
  width) to say "the king's ground". At the east and west, masons' stalls and a vendors' row.
- *Sees on entering:* F4's two braziers, the first statue pair, the first gilded standing guard between them.
- *Enemies:* E3.1 one levy at (0, 10, 300) facing south. A duel on open ground: the guard, the perfect guard, the
  heavy, and getting behind.
- *Shade:* the statues' north shadows (9 m statues: 1.9 m), (10.5, 15.5, 289, 291) and (-15.5, -10.5, 289, 291),
  the same at z 305 to 307; stall awnings (-8, -4, 293, 296.6) and (4, 8, 309, 312.6). Swell rhythm 50 s.
- *Lore:* C7 at the base of the first statue, (-9.4, 10, 289.9) (as built text).
- *Kit:* `statue_king` with `statue_head_new` (offsets as built) x4, plinths, `brazier` x2, `market_awning` x2.
- *S3 comes out here:* the masons' haul-ramp lands at (-18, 10, 286) through a gate in the west side, 12 m from F4.

#### 3B. The Lower Court and the Levy Ramp (z 318 to 346)
- *Shape:* a court 48 m wide (x -24 to 24) at y 10, its north side the middle terrace's retaining wall, 3 m of
  pale stone with a **colonnade** in front (columns every 4 m at z 341, roof at y 13, the terrace's lip; shadow-only
  from the camera's side so the columns read and the floor under them is shade). Up the middle runs **the Levy
  Ramp**, a causeway 7 m wide (x -3.5 to 3.5) from z 318 at y 10 to z 346 at y 13: 28 m long, 6 degrees. Its sides
  are open: from z 318 to 326 its edge is under 1 m above the court; by z 346 it is 3 m. The court either side of the
  ramp is open paving with a few fallen blocks.
- *Sees on entering:* the ramp's foot, its rising edges as two shade-coloured wedges, and at its head the three
  levies in a line, shields locked. V3 at (0, 10, 320): the camera eases back to show the whole ramp and the
  colonnade.
- *Enemies:* E3.2 the **shield wall**: three levies at (-2.4, 13, 344), (0, 13, 344.4), (2.4, 13, 344), facing
  south, locked. When the player is on the ramp or within 10 m of its foot, they advance down it in step.
  - *Why the room suits them:* on the ramp the wall fills the width; from the front only a heavy blow opens it.
  - *The flank:* near the foot the ramp's edge is low, so the player steps off and lets the wall come down past;
    once the levies are on the court (they follow down the ramp, straight at the player), the court is wide and they
    can be circled. Or: the colonnade's two side stairs (x -22 to -20 and 20 to 22, 2 m wide, z 336 to 341, 3 m
    over 5 m, steep, 31 degrees) climb to the middle terrace, and from there the player comes down the ramp **behind**
    the wall. Three answers; the room shows all three from V3.
- *Shade:* the colonnade, (-24, -4, 341.6, 346) and (4, 24, 341.6, 346): 20 m from the ramp's foot. Swell: **the
  district's first is forced** as the wall begins to advance; the colonnade is the shade and the wall comes on.
- *Lore:* none new.
- *Kit:* `column_lotus` on `column_base` at a smaller scale (0.6, 5.4 m) for the colonnade, the ramp and the
  retaining wall built (build list B2, B3), `Rubble` blocks.

#### 3C. The Masons' Terrace (z 346 to 386, y 13)
- *Shape:* the middle terrace, 48 by 40 m, a working site: the **unfinished colossus** seated against the west, at
  (-15, 13, 364) yaw 90 (facing east), the kit's `statue_king` at 1.6 (about 14 m), its head half recut (the old
  face, `statue_head_old`, as built for the unfinished statue). Round it the **scaffold**: decks at y 16 and y 19
  (3 m and 6 m up), each 3 m deep along the statue's east face from z 356 to 372, reached by ladders, with reed mats
  on the upper deck. Squared blocks lie about (1.5 m high cover), a sledge with a recut head on it at (6, 13, 360),
  the fallen new head (as built). A masons' awning at the east.
- *Sees on entering:* off the ramp's head: the scaffold to the left, the blocks, the stair up between its two
  plinths straight ahead at z 380.
- *Enemies:* E3.3:
  - a **spearman on the lower scaffold deck**, (-10, 16, 362), facing east over the path: when the player is within
    7 m of the scaffold's foot and south-east of it, he leaps down spear first (the leap, build list C3) and fights on.
    From the ramp's head, the deck's edge is 3 m up and 16 m north-west: out of frame until the player is near,
    and his mark on the floor is the warning.
  - a levy at (6, 13, 372) by the sledge, facing south.
- *Shade:* under the scaffold decks (-12, -6, 356, 372.6); the masons' awning (10, 16, 360, 363.6); the north
  colonnade at the foot of the upper terrace (-24, -9, 381, 386) and (9, 24, 381, 386).
- *Lore:* C8 on the scaffold (as built text) at (-9, 13, 368); the `tomb_5_recut` panel's subject made real.
- *Kit:* `statue_king` 1.6 + `statue_head_old`, scaffold (as built poles and decks, made walkable and larger),
  `statue_head_new` on the sledge, blocks.
- *Shortcuts from here:* **S3**, the masons' gate in the terrace's west wall at (-24, 13, 350): barred on the
  terrace side; opened, the haul-ramp (x -30 to -26, from z 350 at y 13 down to z 290 at y 10, 60 m, 3 degrees, with
  sledge runners in it) runs south behind the avenue's west wall to the gate at (-20, 10, 286) beside F4. **S4**
  lands on this terrace's west colonnade at (-24, 13, 382) (see the hall).

#### 3D. The Spearman's Stair (x -4 to 4, z 380 to 386; y 13 to 16)
- *Shape:* a stair 8 m wide, 3 m up over 6 m (27 degrees), framed by two great plinths (x -10 to -4 and 4 to 10,
  z 378 to 390), their tops at y 17.5, carrying seated falcons (kit `statue_king` scaled 0.5 as a stand-in, or a new
  piece). The stair is the only way up.
- *Enemies:* E3.4 a **spearman on the east plinth top** at (6, 17.5, 384), facing west over the stair. He leaps onto
  the player on the stair (1.5 to 4.5 m down, depending on where the player stands). This is the ledge-over-a-stair
  of the brief: footing on a stair, a mark that lands across it, nowhere sideways to go. Roll up or down.
- *Shade:* none on the stair; the colonnade below.

#### 3E. The Warden's Court (the upper terrace, z 386 to 410, y 16)
- *Shape:* a court 48 by 24 m before the pylon, not open: in its middle the **barque shrine**, a stone plinth 6 m
  (x) by 4 m (z) and 1.2 m high at (0, 16, 398) carrying a gilded reed barque (the kit's `reed_boat`, gold). Side
  porticoes along x -24 to -18 and 18 to 24 (roof y 22, shadow-only). On the east, a **dais** 1 m high, 4 by 4 m,
  at (15, 16, 404), with a mirror stand on it.
- *Sees on entering:* off the stair head, the shrine dead ahead, the warden standing beyond it before the pylon's
  door, a levy left of the shrine; the priest on the dais to the right.
- *Enemies:* E3.5:
  - the **warden** at (0, 16, 404) facing south, holding back (`WaitRange` 8) until the levy falls or the player
    reaches the shrine.
  - a levy at (-7, 16, 396).
  - a **sun-blind priest, "the caller"**, on the dais at (15, 17, 404): he cannot see, but the court is open sky:
    **a player lit for 0.4 s within 18 m is found**. Found, he raises the mirror and *calls*: the player is lit up
    for 3 s and the gilded turn toward them and come. He throws the mirror's beam along a line.
  - *Why the room suits them:* the shrine is the warden's pillar: his **pour** (an arc of gold, a low wall for some
    seconds) is placed to cut one side of the shrine, so the player who circles is cut off and must go the other way
    round, past him. His planted ring (3.2 m) is smaller than the shrine's clearances (5 m each side) so it never
    traps against it. The caller is in frame (east, 1 m up) while the player fights round the shrine, and the side
    porticoes are where his light cannot find you: the swell and the caller push the same way.
- *Shade:* the side porticoes (-24, -18, 387.3, 409) and (18, 24, 387.3, 409); the shrine's north side
  (-3, 3, 400, 400.3) too thin, not registered.
- *Lore:* C9 on the pylon over the hall's door, (5.2, 16, 409.4) (as built text).
- *Kit:* `pylon_gate` 1.5 at (0, 16, 414) (as built, moved), `reed_boat` gilded on the shrine, colonnade columns,
  mirror stand (build list B10).

#### 3F. The Hall of Columns (x -22 to 22, z 418 to 478, floor y 16)
- *The as-built hall, moved 18 m north and 12 m up*, keeps its 24 columns, its shadow-only roof and its light bars
  (now z 426 to 430, 446 to 450, 466 to 470, and the nave x -2 to 2 for the whole length). What changes:
  - **The aisles are raised.** The two outer aisles, x -22 to -12 and 12 to 22, are platforms at y 17.2 (1.2 m up),
    with three steps down to the central hall every 10 m between the columns. Priests on them are 1.2 m up: in frame
    (rule 1.1.6), and the platforms' edges are lines the player must cross to reach them.
  - **A fallen column** lies across the east aisle from (13, 17.2, 437) to (21, 17.2, 451), 2 m thick, blocking it:
    the east aisle is cut in two. Where it fell the roof broke: a lit patch (14, 20, 440, 446).
  - **Two mirror stands** (build list B10): M1 at (-8, 16, 448) inside bar 2; M2 at (8, 16, 428) inside bar 1.
    Turned with the use button (eight steps of 45 degrees), a stand in sunlight throws a beam 2 m wide along its
    facing for 30 m: the ground in it counts as lit, it burns enemies in it, and it lights a gold disc it falls on.
  - **The sun door** in the west wall at (-22, 17.2, 448), with a gold disc above it: M1 turned due west lights it and
    it opens (it stays open). Behind it the **west chapel** (x -32 to -22, z 440 to 456, y 17.2) and a passage
    south outside the hall's west wall (x -32 to -28, z 440 down to z 390, falling from y 17.2 to y 16), then a stair
    down (x -32 to -28, z 390 to 384, y 16 to 13) into a small vestibule (x -32 to -24, z 378 to 384, y 13) whose
    door at (-24, 13, 381) opens only from inside onto the middle terrace's west colonnade: **S4**. In the chapel a carving (Claude's, new): "The door opens to the sun. He made it so." /
    "Under the gold the lintel is older: a lamp, and a hand holding it out." 
- *Sees on entering:* through the pylon door, the nave's bright strip running north, the first bar crossing it, a
  priest standing where they cross.
- *Enemies:* E3.6, the four priests and one spearman who sees:
  - priest at (0, 16, 428), where bar 1 crosses the nave: struck from the shade first (double damage).
  - priest on the west aisle at (-17, 17.2, 438).
  - priest on the east aisle north of the fallen column at (17, 17.2, 458).
  - priest at (-6, 16, 460), facing a mirror's line: M1 turned north-east burns him.
  - spearman by the north door at (7, 16, 474), in shade (as built: he can see; his fight in the nave's light brings
    the priests).
- *Shade:* the hall under its roof, registered as the rectangles between the light, generated from the same list
  that cuts the roof (build list B12). Between bar 1 and bar 2, for example: (-22, -2, 430, 446) and (2, 22, 430,
  440), (2, 14, 440, 446), (20, 22, 440, 446), the breach (14, 20, 440, 446) being lit. No swells under the roof.
- *Lore:* the chapel's words; C9 outside.
- *Kit:* as built (`column_lotus`, `column_base`), one column laid on its side.

#### 3G. The porch (z 478 to 488, y 16)
- As built, moved: paving, F5 at (8, 16, 482). V5 at (0, 16, 486): the camera eases back over the bowl and shows
  the champion standing, the plinths and the gate.

### 3.4 The champion's bowl and the gate of noon (z 488 to 526)
See section 5.

### 3.5 The sun-court's approach (`SunCourt`, local coordinates)

The court (radius 20, centre (0, 0, 0)) is untouched. South of it, the forecourt now has an approach, walked once:
after a fall the player wakes at F6.

```
 z
 -20 ===== gold line ===== court
 -24   [C11 obelisk faces in view]
 -36   FORECOURT y0, x -10..10, F6*(-5,0,-30)    awning (4,9,-34,-30)
 -38   ### flight 3: y -2 -> 0, 8 m wide ###
 -46   landing 2 y-2  (braziers, sphinx stumps)  shade: west wall niche (-9,-6,-46,-42)
 -50   ### flight 2: y -4 -> -2 ###
 -58   landing 1 y-4  V6 at (0,-4,-56)
 -62   ### flight 1: y -6 -> -4 ###
 -70   door landing y-6, x -5..5: arrival from the gate of noon
```

- **The Way of Noon:** a sunk processional stair between walls 5 m high on the east and west (they fade) and 1.2 m
  on the south. Three flights of 2 m rise over 4 m run (27 degrees), 8 m wide, with landings 8 m deep, climbing
  6 m from the arrival door at (0, -6, -70) to the forecourt at y 0. The walls are lined with recut sphinxes
  (stand-ins: `statue_head_new` on blocks), and the steps get paler toward the top (new stone).
- *Sees on arrival:* the first flight, and at its head (V6) the camera eases back 1.6 times and shows the obelisk's
  foot and the gold line. The obelisk's cap is never in frame from here; its **shadow** is: the court's turning
  shadow reaches the gold line at certain hours and is the first thing of the court the player sees.
- *Swells:* every 40 s along the way (the court's rule pushed hardest); shade: niches in the west wall on each
  landing ((-9, -6, -58, -54) on landing 1, (-9, -6, -46, -42) on landing 2) and F6's awning. Walking straight up
  takes 8 s; a player who stops to read C11 is caught.
- *No enemies.* Tension, a vista, the lore, the fire.
- *Lore:* C11 stays where it is (the obelisk's foot).

---

## 4. Shortcuts and loops

| | Opens from | To | How | Saves |
|---|---|---|---|---|
| S1 | The Fishers' Bank's south end, (-6, 3, 13) | The Narrows beside F1, (-5, 0, 13) | A ladder lying on the bank top, kicked down (one press); then climbed both ways | After a fall at the basin: F1 to the basin along the bank (85 m, one buried thirsting) instead of the bed (six thirsting) |
| — | The barque's prow, (6, 3.5, 99) | The basin, (6, 0, 101) | A drop of 3.5 m, one way | A second way into the basin's fight |
| S2 | The market's south-west corner, (-28.5, 6, 176) | The quay, (-28.5, 4, 134), 6 m from F2 | A barred door, barred on the market side; then a covered stair and a lane | F2 to the market without the Lamp Street and the Oil Alley |
| — | The roofs' north edge, (-2, 10, 196) | The market's east arm, (-2, 8, 198) | A drop of 2 m, one way | A way into the market behind its ashen |
| S3 | The Masons' Terrace's west wall, (-24, 13, 350) | The avenue's west gate, (-20, 10, 286), 12 m from F4 | A barred gate at the top of the masons' haul-ramp | F4 to the middle terrace without the shield wall |
| S4 | The hall's sun door, (-22, 17.2, 448), opened by mirror M1 | The middle terrace's west colonnade, (-24, 13, 382) | A door that opens only from inside, at the bottom of the chapel's passage | With S3: F4 to the hall without the shield wall, the spearmen or the warden |
| Loop | The basin ramp | The bank, the well, S1, F1 | Walking | The side path is also the way home |

S1 to S4 stay open after a fall and across sittings (a progress key each, as `LandVoice` keeps `suncity.woke`).

---

## 5. The champion's bowl

**Shape.** Not a rectangle: an oval bowl 36 m east-west by 30 m north-south, centre (0, 14.5, 505), its floor 1.5 m
below the porch (y 14.5), sunk in the stone like a threshing floor. The rim is y 16 all round. The south edge is
**three steps the whole width of the porch** (z 486 to 488.4, 1.5 m over 2.4 m); the gold line is the top step's
edge, z 488. The north rim carries the gate wall; a stair of 1.5 m (x -4 to 4, z 518 to 520.4) climbs to the gate.
East and west, the bowl's sides are 1.5 m faces of gold-veined stone (the rim walls behind them 6 m, east and
west only). From the camera the south steps face it: the bowl reads as down from the porch.

```
 z    x: -18   -9    0    9    18
526        ====[ GATE OF NOON ]====   doors at (0,16,524)
520            ##stair 14.5->16##
514    (    P              P    )      P = plinth 1.8 x 1.8, 1.2 m high, kneeling predecessor
505   (       drum    (C)   drum   )   drum = broken column drum, 2.4 across, 1.6 high
496    (    P              P    )      (C) the champion at (0,14.5,508)
488   ======= three steps; gold line z488 =======
              porch y16, F5 *(8,16,482)
```

**Pieces.** The four kneeling predecessors on plinths (as built, now 1.2 m high) at (-10, 14.5, 496), (10, 14.5,
496), (-10, 14.5, 514), (10, 14.5, 514). Two broken column drums at (-14, 14.5, 505) and (14, 14.5, 505). Nothing
else on the floor: the middle 20 by 18 m is clear.

**How they play with his moves** (`fire_land.md` 5, and the as-built stand-ins):
- *The thrown spear (lane 12 m).* A plinth or drum in its lane **stops the spear**: it stands in the stone where
  the lane meets it, and the throw's lane is drawn only that far. A player behind a plinth at 6 to 12 m draws the
  throw into it, and the recall then pulls straight back from the plinth to his hand (along the line, as specified):
  the bare-handed opening comes with him on the far side of the stone. Learn to stand beside the line, not behind
  the plinth, to get the opening close.
- *The ring of fire (5.5 m round the planted spear).* The clear middle (20 by 18 m) holds the ring with 4 m to spare
  each way when he plants it there; planted near a side, the ring meets the bowl's 1.5 m face, which cannot be
  climbed: **the bowl's edges are where the ring traps.** He plants it on opening his second phase wherever he
  stands; a player who has fought him into the middle has room.
- *The sweep (200 degrees, 3.8 m).* Punishes circling; the plinths are 2.5 m from a player hugging them, inside its
  reach, so they are not safety from it.
- *The second phase: the gold runs.* Every blow that lands splashes gold (`fire_rework.md` 1); a long fight fills the
  middle with puddles that burn and slow. The bowl's floor is slightly dished (the build makes it 0.3 m lower at its
  centre), so the gold is drawn running to the middle: the fight drifts outward, toward the plinths and the walls,
  toward the ring's trap. That is the arena's arc.
- *No heat swells here* (the brief).
- *His ground:* `HoldGround(-18, 18, 488.5, 520)`; the oval is inside it.

**The door of noon.** The gate as built (bronze doors 6 by 8 m, gold straps, the sun disc), moved to the north rim at
(0, 16, 524), its wall 14 m high from z 522 to 526. The `GateOfNoon` interactable at (0, 16, 522.8). C10 on the wall
beside it at (6.4, 16, 522.6). The obelisk of the court beyond, as built (`Beyond`), at (0, 16, 540).

---

## 6. Build list

Everything here is plain geometry made in code (a builder helper that writes a mesh with `MeshFilter`, `MeshRenderer`
and `MeshCollider`, static, and an ink outline where a toon material carries one). Each piece names its parameters;
sizes are the defaults the plan above uses.

### 6.1 New geometry

| | Piece | What it is | Dimensions and parameters |
|---|---|---|---|
| B1 | **Plateau** | A floor of any outline: a polygon (list of x,z points, clockwise) extruded from a bottom height to a top height, top face flat, side faces vertical or leaning back, the outline's edges broken by a jitter. Collider: the mesh (convex pieces if needed) | `Plateau(name, Vector2[] outline, top, bottom, material, sideMaterial, jitter 0.4, lean 0)`. Used for the banks' tops, the bed (as a depression: the bed is the ground at 0, the banks are plateaus at 3), every terrace, the market's upper level, the bowl's rim. Faces facing south get `m_sun_brick_dark` or stone in shade colour, top faces paving |
| B2 | **Bank / retaining face** | The face of a plateau where it is made to look built or eroded: strata bands every 0.6 m, a crumbling top edge (the outline jittered 0.5 m, a 0.3 m lip of loose blocks), slumps of mud at the foot every 10 to 15 m (low wedges 2 m long, 1 m high), brick facing in patches on the river banks; clean ashlar with a cornice on the terraces | Height 1.5 to 4 m. Faces lean back 12 degrees (river) or 0 (terraces). The river banks: 3 m. The quay face: 4 m with the boat-sheds cut in (B5). Terrace faces: 3 m |
| B3 | **Ramp / causeway** | A sloped walkway with no steps, a top face and two side faces down to the ground beside it (so its edge rises out of the court) | `Ramp(foot, head, width, material)`. The Levy Ramp: 7 m wide, 28 m long, y 10 to 13. The basin's bank ramp: 4 m, 12 m, 0 to 3. The cart ramp in the market: 4 m, 12 m. The haul-ramp (S3): 4 m, 60 m, 13 to 10, sledge runners (two dark strips) on it |
| B4 | **Deck** | A walkable floor laid on the barque's hull: a slab 4.5 by 20 m at y 3.5 following the hull's heading (yaw 200, heeled 8 degrees about its long axis), rails 0.6 m on the east side only (the camera's west side has none) | Gangplank: a `Ramp` 3 m wide from (1, 0, 72) to (2, 3.5, 78). Prow drop: the deck's north end open |
| B5 | **Arcaded face** | A retaining face with arched cells cut into it: each cell w wide, d deep, h high, a flat ceiling (shadow-only for the camera, solid to walk under), the floor of the cell the lower level | Quay: four cells 4 wide, 3 deep, 3 high at x -18, -8, 8, 18 in the 4 m face at z 116. Terrace colonnades are B11, not this |
| B6 | **Ladder** | An interactable: "Climb" moves the player up or down it in 1.2 s (held still, as `Suspended`); a ladder that starts **lying** on the top ("Kick down the ladder", usable only from the top) drops into place with a thud and stays down (progress key) | S1 (3 m), the roof ladder (4 m, at (14.2, 6, 163)), the scaffold ladders (3 m and 3 m, at (-8, 13, 358) and (-8, 16, 370)) |
| B7 | **Barred door / gate** | A door 3 to 4 m wide whose bar is on one side: from that side "Lift the bar" opens it for good (progress key); from the other "Barred from the other side." (status line) | S2 at (-28.5, 6, 176), 3 m; S3 at (-24, 13, 350), 4 m; S4's door at (-24, 13, 381), 3 m (opens from the vestibule's side only) |
| B8 | **Breakable wall** | A wall section 4 m wide, 3.5 m high, 0.6 m thick, of mud brick with cracks drawn on it (a decal or dark lines); broken by one heavy blow, or by an ashen's burst within 2 m: it falls into rubble (pieces as `Rubble`), the collider goes, a progress key keeps it broken | The hearth room at (-2, 4, 141), facing east into the Lamp Street |
| B9 | **Jar** | A clay jar 0.9 m high, 0.6 m across, breakable by one blow. *Water jar:* breaking it leaves a wet stain 1.5 m across for 20 s that thirsting go to and drink at (and heal). *Oil jar:* fire (an ashen's ring, a burning wick touching it, a burst jar beside it) bursts it into burning ground 2 m round for 6 s that bites player and enemies and lights wicks. A struck oil jar spills (a dark stain) without burning | Water jars: Pans 1, west channel 1, basin 3, bank 1, market 3 (`World/WaterJar.cs` and `Gameplay/Waters.cs` are being built by another worker; place those). Oil jars: Oil Alley 11, roofs 1 (new) |
| B10 | **Mirror stand** | A bronze disc 1.2 m across on a post 1.8 m high on a square foot; "Turn the mirror" turns it 45 degrees; if it stands in a lit rectangle it throws a beam: a lane 2 m wide, 30 m long (stopped by the first solid in it), drawn as a band of light on the floor and a shaft in the air. The beam: counts as lit (`Sunlight` gets a beam rectangle, oriented), burns enemies in it 6 every 0.5 s, and lights a **gold disc** it reaches (an object with an event) | M1 (-8, 16, 448) facing north at start; M2 (8, 16, 428) facing north; the caller's mirror on the dais (his own: it turns with him, not the player). Sun door disc at (-22, 18.5, 448) |
| B11 | **Colonnade** | A row of columns along a terrace face with a roof slab from the columns back to the face, the roof shadow-only from the camera side (it casts and is not drawn), shade rectangle registered from the same numbers | Lower court: columns at z 341 every 4 m from x -24 to -4 and 4 to 24, roof at y 13 (3 m), 5 m deep. Middle terrace north: z 381, the same. Upper terrace sides: x -18 and 18, z 388 to 408 every 4 m, roof y 22. Yard gallery portico: z 262, roof y 12 |
| B12 | **Shade list** | One list of shade rectangles per district, each with its source (awning, roof, overhang), from which the awning or roof is placed and `Sunlight.AddShade` is registered, so they cannot disagree (as `HallLight` does for the hall). The hall's shade is generated as the complement of its light bars inside its walls | A `SunlightZones.Configure(true, shade, null)` for the whole scene. The rectangles are flat (x and z only): the bridge-house's shade also covers the roof walk above it at z 166 to 170. Either give rectangles a height band (a small change to `Sunlight.Add`) or accept it; the plan keeps the roof route clear of that strip |
| B13 | **Vista zone** | A box trigger that, while the player is in it, calls `SetFraming(1.5, (0, 0, 6), -10)` on the camera, and `ClearFraming` on leaving. Not while an enemy has noticed the player | V1 (0, 0, 15), 10 by 4 m; V2 (0, 4, 118), 8 by 3; V3 (0, 10, 320), 8 by 4; V4 at the ramp head (0, 13, 348), 8 by 3; V5 (0, 16, 486), 14 by 3; V6 in the court's approach (0, -4, -56), 8 by 4 |
| B14 | **Heat swell by district** | `NoonHeat` and `HeatSchedule` are being built by another worker (`HeatSchedule(firstAt, every)`, calm rectangles). What this plan needs on top: a trigger per district line that calls `NoonHeat.SetSchedule` with that district's rhythm, and triggers that force a district's first swell (D1 at z 31, D3 at the shield wall's advance) by setting the clock to the warning's start. The hall, porch and bowl are calm rectangles | D1 z -6 to 134: 100 s. D2 z 134 to 272: 70 s. D3 z 272 to 410 (open sky only): 50 s. Hall, porch, bowl: none. Forecourt approach: 40 s |
| B15 | **Bowl** | The champion's oval: an elliptical hole 36 by 30 m in a plateau at y 16, floor at y 14.5 dished to 14.2 at its centre, three steps on the south as a `Stairs` 30 m wide, a stair north | Section 5 |
| B16 | **Bridge-house and roofed passages** | A room over a street on two walls: its underside shadow-only, its body fades | Lamp Street leg 2 at z 166 to 170; the gatehouse arch at the quay; the Oil Alley's west end; the Embalmers' Gate passage |
| B17 | **The burnt houses** | Roofless house shells, walls 1.4 m (south) to 2.5 m (north), floors black char, a few charred beams | x -10 to 9, z 160 to 176, y 6 |

### 6.2 New enemy behaviour the rooms rely on (for the code worker, not geometry)

On 2026-11-17 another worker had `BuriedMound`, `AshenBurst`, `WickFlame`, `GoldRuns`, `ThirstDrinker` and
`MoltenFloor` in progress; check those before writing C1, C2, C4 and the gold puddles again. What the rooms need from
each is below.

| | What | Where it is used |
|---|---|---|
| C1 | Buried thirsting: a mound, bursting at 3.5 m (6 m in a swell); drinking at water stains | Pans, west channel, basin, bank |
| C2 | Ashen; its burst lights oil jars and breaks a breakable wall | Lamp Street, Oil Alley, market, the hearth wall |
| C3 | A leap from a ledge: the thirsting's pounce from the deck, the spearman's leap from the scaffold and the plinth. The mark is drawn where he lands first | Barque, Masons' Terrace, Spearman's Stair |
| C4 | Flame spreads: a burning wick within 1 m lights a dormant one; a knocked brazier lights what is within 2 m | Yard, roofs |
| C5 | Shield wall: levies within 3 m of each other lock and advance in step | Levy Ramp |
| C6 | Warden's pour: an arc of gold 5 m long, 0.6 m high, for 6 s, laid across the side of the shrine the player is circling to | Warden's Court |
| C7 | The caller: a priest who has found the player lights them for 3 s and the gilded within 20 m turn and come | Warden's Court |
| C8 | Champion: the throw stops at the first solid (plinth, drum) | The bowl |

### 6.3 Kit pieces reused, and where

| Piece | Where now |
|---|---|
| `mud_tile` | Riverbed floor everywhere in D1, over a `Plateau` at y 0 whose renderer is off (as built) |
| `reed_boat` | Narrows (1), Pans on trestles (1), the Jam (3), the barque's gangplank sides (as rails), the barque shrine (gilded) |
| `barque` | The bend, 0.73, as built place and yaw, with B4 deck |
| `well_shrine` | The sunk well court (-56, 1, 80) |
| `market_awning` | Fishers' awning at F1, two racks in the Pans, the Lamp Square, the market (3), the yard's table canopies (2), the avenue's stalls (2), the masons' awning, F2's awning |
| `house_block`, `house_doorway` | Lamp Street, the Oil Alley's north side, the market's west and south, the fishers' huts (single storey) |
| `wall_section`, `wall_broken` | The burnt houses, the low south walls of every east-west street, the market's south edge |
| `burial_niche` | The yard gallery face (8) |
| `sarcophagus` | The yard (2) |
| `brazier` | Rest fires, the yard gallery (2), F4 (2), the bowl (2) |
| `statue_king` + `statue_head_new` | The avenue (2 pairs); the Spearman's Stair's falcons (scaled 0.5, stand-in) |
| `statue_king` 1.6 + `statue_head_old` | The unfinished colossus, the Masons' Terrace |
| `column_lotus` + `column_base` | The hall (24, as built, one laid on its side); colonnades at 0.6 scale |
| `pylon_gate` | Before the hall, 1.5 (as built) |
| `obelisk` | `Beyond`, over the gate (as built) |
| `tomb_*` panels | The painted tombs, as built |

### 6.4 What becomes of the first pass's code

`SunCityBuilder` keeps its helpers (`Floor`, `Box`, `Mass`, `Stairs`, `Disc`, `Paving`, `Scatter`, `Drift`, `Stain`,
`Trodden`, `Soft`, `Rubble`, `Tilt`, `Solid`, `Carving`, `Rest`, `PinnedSoldier`, `Painting`), `Beyond` (its plain
raised by district), `PaintedTombs` (moved), the hall's columns and roof code (moved, with the shade list made from
the light list), `LastGate`'s doors and gate (moved to the bowl), the sponsor's words and the title. The bodies of
`DryRiver`, `WellShrine`, `CityOfWicks`, `GoldenWay` are rewritten to this plan.

---

## 7. Pacing

Minutes for a new player on a clean pass (the walk and the fights won), as `fire_land.md` 3.1 counts them: a weak
enemy about 30 s, a strong one 45 s, a group a little less each than alone.

| Stretch | Minutes (new) | Experienced | Tension | What it teaches |
|---|---|---|---|---|
| The Narrows: waking, F1 | 1 | 0.5 | Rest | The sponsor; the banks: height |
| The Pans | 2 | 1 | Rising | The chain and the finisher; the ground's mounds; **the first swell and shade** |
| The Jam | 1 | 0.5 | Peak (small) | A chokepoint; turns; the red pounce from a gap |
| The Barque's Bend (and deck) | 1.5 | 0.7 | Rising | Look up; the snatch; a high branch and a drop |
| The basin, the quay, F2 | 2 | 1 | Peak, then relief | A crowd; a shed's mouth as a choke; do not drink in a crowd |
| **District 1** | **7.5** | **3.7** | | |
| The Lamp Street | 2 | 1 | Rising | The grab is red; doorways; **the ashen alone** |
| The Oil Alley | 1.5 | 0.7 | Peak | Ashen near wicks, not near you; fire spreads in oil |
| The market | 3 | 1.3 | Peak | Two levels; the corruptions together; an ashen arriving |
| The gate, F3, the spear | 1 | 0.5 | Rest | The spear; rest |
| The yard | 2.5 | 1.2 | Peak | Reach; the flame spreads to the dormant |
| **District 2** | **10** | **4.7** | | |
| The avenue, F4 | 1.5 | 0.7 | Rising | The gilded's guard, one at a time |
| The Levy Ramp | 2.5 | 1 | Peak | The shield wall and three ways round it; a swell on a slope |
| The Masons' Terrace | 1.5 | 0.7 | Rising | A leap from above; the first barred gate opened from behind (S3) |
| The Spearman's Stair | 0.7 | 0.3 | Spike | No sideways on a stair |
| The Warden's Court | 2 | 1 | Peak | The warden's ring and pour round a pillar; the caller; porticoes against light |
| The hall | 3 | 1.5 | Stealth, then peak | Shade is safe; the strike from shade; mirrors; the sun door (S4) |
| The porch, F5 | 0.3 | 0.2 | Rest, vista | |
| **District 3** | **11.5** | **5.4** | | |
| The Champion of the Gate | 3 | 1.5 | Peak | The god's words: the lane, the throw and recall, the ring |
| The Way of Noon, F6 | 1 | 0.5 | Dread | The swell at its hardest; the obelisk's shadow seen first |
| The sun-court | 8 | 6 | Climax | (as built) |
| **Clean pass** | **41** | **23** | | |
| Side paths (bank and well, roofs, hearth room, tombs) | + 6 | + 2.5 | | |

That is a minute over the owner's 30 to 40 on a clean pass. The levers, in order: the levy by the sledge on the
Masons' Terrace (0.7), the second ashen in the Oil Alley (0.5), the basin's buried thirsting (0.4); with all three
it is 39.5.

**Enemy count.**

| District | Main path | Count |
|---|---|---|
| D1 | 11 thirsting (one kneeling, four buried) | 11 |
| D2 | 9 wicks (3 of them dormant), 4 ashen, 1 thirsting | 14 |
| D3 | 6 levies, 3 spearmen, 1 warden, 5 sun-blind priests | 15 |
| Side paths | 3 thirsting (bank, well), 1 dormant wick (roofs), 2 tomb wicks | 6 |
| **All** | | **46 and the champion** (first pass: 42) |

40 on the main path against the first pass's 38. Taking the three levers above brings it to 37.
