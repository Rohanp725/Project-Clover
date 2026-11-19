# The Earth and Life land's levels (first pass)

Written by Claude on 2026-11-19 as the Earth and Life land's counterpart to `fire_levels.md`, `water_levels.md`,
`metal_levels.md`, `ice_levels.md` and `poison_levels.md`, before any of the land is built. **Everything here is Proposal
(Claude)** unless it is marked **Decided** with its source. The land's myth, people, elites and fight are `WORLD.md`,
"Earth and Life's land: the stones and the grove"; its look is `LOOKS.md` section 9; the rules every land follows are
`WORLD.md`, "Level design rules for every land (decided 2026-11-18, evening)". The enemies' full numbers belong in a later
`earth_life_land.md`, as `fire_land.md` section 4 holds Fire's; this plan gives only the numbers the rooms depend on,
marked as first guesses.

Axes as in Fire: x east, z north, y up; north is up the screen. Coordinates are metres. "Floor y 12" is the walking
height. The land is two scenes (names proposed): `StoneGrove` (the bog, both branches, the barrows, the plain and the
rise) and `MeetingGround` (the gate barrow, the bowl, the gods and the solstice passage), as `SunCity` and `SunCourt`
are two. The player wakes at (0, 0.5, 0) in `StoneGrove`, on the plank road.

---

## 0. What the owner has decided, and how this plan reads it

| Decided | Source | How this plan uses it |
|---|---|---|
| The map's shape is the fork: the bog, then Earth's branch and Life's branch taken in either order, then the plain and the meeting ground | `WORLD.md`, level design rules and "The land" | The Fork (section 2) is a knoll at the bog's head with three ways: west (Earth), east (Life) and north (the barrow road to the plain, sealed from its far end). Each branch ends at its seat; each seat's barrow comes out on the plain. The two branches are built to the same length and pressure (section 11), so neither order is the wrong one |
| Rest fires in Fire's rhythm: one every 6 to 8 minutes of a new player's walk, always one at the god's door, one shortcut loop per district back to its fire; more fires in the duo lands | same | **Eight fires** (Fire six); five shortcuts, one of them the barrow road home between branches, and the two seats' barrows (section 4) |
| "See it, then find the way" side paths hold the plain weapons and lore; one optional trial invented for the land (no steer from the owner) | same | Section 8 (the stag's hollow and the plain Scythe, the eagle's lintel and the plain Massive Sword, the crannog, the owl, the salmon and the oath stone); section 7 offers three trials and proposes one |
| Up to three walkable levels per district, joined by ramps, stairs and ladders seen from above; nothing walkable under a roof; roofs and overhangs only fade | same | Every district names its levels. Barrow passages are walked with their mound **never drawn** above the player (1.1), as Metal's culvert fades and Poison's tomb vault is never drawn |
| The verb for the Wild Hunt is **route-finding**: one set piece of choosing which standing stones to run between | same | Section 3.5: three bands, two routes through the middle one, a horn clock, rings of untouched heather |
| The barrows age Life's side a year going one way and make it a year younger going back; no path is ever lost for good | same | Section 1.3: four years, read from flat entrance stones with cup marks and a spiral's turn, and from a year hazel at every mouth |
| The gods' brawl is felt everywhere (distant thunder of stone, the roar of the wild, birds lifting in flocks, now and then a tremor) and seen only from the two seats and on the last walk | same; `WORLD.md`, "The land" | Section 1.4 (the felt beats) and section 9 (three held shots for the brawl: sitting in each empty seat, and the brow of the rise; an arrival shot besides is question 6) |
| Stone remembers, wood forgets: on Earth's side what is broken stays broken; on Life's side cut paths and broken things grow back at each rest | `WORLD.md`, "The land" | Section 1.2: what persists, what regrows, and the rooms that teach each |
| The dead take root: corpses on Earth's side set into stone, on Life's side they sprout thorns; respawned enemies climb out of their own stone or thicket | same | Section 1.2 |
| The mounds turn the years; the Hunt is one set piece on the plain before the meeting ground; teaching order: Earth's side, Life's side, the bog, the mounds, the Hunt | same | Read as the order in which the *rules* are layered. The bog shows each rule once in small (stone first, then wood, then its own dead), the mounds are first used in earnest in Life's village, the Hunt comes last (section 2) |
| The areas: the border bog crossed by an oak plank road; Earth's branch (the King's Men's hill, then the Giant's Dance, Brannoch's seat, with its elite); Life's branch (the wildwood village, then the sacred grove, Carnwyn's seat, with its elite); the barrows under the hills linking the land; the Wild Hunt's plain; the meeting ground | same | Sections 3 to 6 |
| The Hunt: a horn sounds, the Hunt rides across the plain with Carnwyn's herd and the dead; it cannot pass stone, so the player runs from standing stone to standing stone; caught in the open, the player is not killed but carried back to the plain's edge, hurt | same | Section 3.5; where it carries the player is question 2 |
| Earth's people: the King's Men (move only while the player is not facing them), the countless (cairns that rebuild unless the one stone that differs is struck), the boar's litter (led by Grugyn Silver-bristle; they root up the ground), the haulers (kill them and the stone rolls back) | `WORLD.md`, "The enemies" | Section 1.7, each with a mechanic of its own (the owner's bar: no reskins) |
| Life's people: the Green Men (root and heal when hurt; strike them uprooted or cut the roots), the mute reborn (no sound, no telegraph noise; a cauldron in their district brings them back until it is broken from inside), the wicker (walking cages with a living person inside; the wicker tries to regrow round them), the overgrown herd (a charge that tangles so the player cannot roll; shed antlers take root as thorns) | same | Section 1.7 |
| Two elites: the Bargain at the Giant's Dance (the beheading game, his axe previews Brannoch's blade; teaches The Return Blow) and the Wicker Man in the sacred grove (its health is its captives; it regrows; burning it is not a choice; teaches Over the Shoulder) | `WORLD.md`, "The two elites"; `steel/weapons.md` 14 | Section 5 |
| The Oldest Animals side path gives the land's plain weapons and a way to make the god fight easier (the oath stone) | `WORLD.md`, "The Oldest Animals" | Section 8 |
| The fight: both gods at once in phase 1, both transform together, both beasts on the field in phase 2, taking turns; the ride out is the solstice passage | `WORLD.md`, "The fight at the meeting ground" | Section 6 gives the room, its approach and the passage, not the fight |
| The look: grey dawn mist on a moor, a low sun that never clears the horizon, always in the same quarter; a cooler grey-green land with ochre kept to cracks and Earth's worked stone; life's green the brightest colour; gold as worked metal | `LOOKS.md` section 9 (the mood Decided 2026-11-18; the rest Proposal) | 1.1: the sun stands a little east of due north, so every shadow is long and runs down the screen |
| Earth and Life are in tier 4; a duo land is one and a half to almost twice the size; a clean pass about 55 to 70 minutes for a new player (Proposal in `WORLD.md`) | `WORLD.md`, "How a climb is structured" | Section 11: 64 minutes |

The owner's answers to the earlier lands carry over where they are rules of the game, not of one land: no swimming (deep
bog water throws the player back to firm ground, `water_levels.md` 1.2), the world's own marks over HUD marks, the
world's state kept honest after a fall (the year holds, as the tide holds), and three held camera shots per land.

---

## 1. Principles

### 1.1 What the camera sees: what changes for this land

The camera is Fire's (`fire_levels.md` 1.1): pitch 55 degrees, 12 m, a vertical field of 40 degrees; the picture
reaches 7.2 m north and 4.3 m south of the player on flat ground and is 15.5 m wide at the player. Nothing farther north
is on screen; landmarks are rooms the player arrives in, or vista zones (`LandZones`); rises face south; the south-side
rule, faded roofs, enemies that walk straight at the player with no navmesh, and ranged attackers kept in frame all hold
as written there. What this land changes:

1. **The sun is the compass.** `LOOKS.md` puts the sun "straight ahead" over the bog, never more than a hand's width
   up, in the quarter the meeting ground faces. This plan fixes it at **azimuth 15 degrees (a little east of north),
   elevation 5 degrees**. Every shadow is then 11 times its caster's height and runs SSW, down the screen and a little
   left. A 3 m stone's shadow is 34 m long: from far beyond the top of the frame it reaches into the picture and points
   back at its stone. On the plain this is how the player sees stones the camera cannot (3.5). South faces stand in
   shade toward the camera, as Fire's do, and everything facing north wears a gold rim.
   - One reconciliation: `LOOKS.md` has the sun "behind the barrow mound" at the meeting ground. Seen from the brow of
     the rise it is exactly that (the gate barrow stands between the brow and the sun); inside the bowl, the barrow's
     passage mouth faces it, which is what the solstice needs (section 6).
   - Midsummer, not midwinter: Stonehenge's axis and its horseshoe of trilithons open toward the midsummer sunrise in the
     north-east, Newgrange's passage toward the midwinter sunrise in the south-east. A sun ahead of the camera needs the
     northern quarter, so the never-finished dawn is a midsummer dawn, and the Giant's Dance's horseshoe opens toward it.
2. **Inside a barrow the mound is never drawn.** "Nothing walkable lies under a roof" (Decided) is kept by drawing a
   passage as a cutaway: the corbelled walls rise to their full 2.2 m, their cut tops inked, and above them only dark.
   The mound's shell fades out as the player steps into the mouth and back in as they leave (`Fadeable`, as Metal's
   culvert under the Round fades; Poison's tomb vault is never drawn at all). Mound tops are never walkable: kerbed, with
   heather slopes of 35 degrees. Light inside comes only from behind the player and from the far mouth (`LOOKS.md`).
3. **The far brawl is a set the game camera can never reach.** The meeting ground, the two gods and the plumes over them
   are built 200 to 300 m from the seats as a low-detail far set on its own layer, drawn only by the three held shots
   (section 9). From anywhere else the camera's 7 m reach means it is not merely hidden but out of the world the
   picture covers, so "seen only from the seats and the last walk" (Decided) is true by construction.
4. **Long south legs are avoided.** As Metal found in its maze, a player walking south sees 4.3 m ahead. The branches
   climb north; the only south-going walks are the barrow road home (a cutaway passage with nothing in it) and the
   seats' barrows (no enemies). No fight is placed where an enemy comes at a player walking south.
5. **Branches mirror in length, not in shape.** Earth's branch climbs in straight terraces (lynchets, ranks, a hollow
   way); Life's climbs through round, tangled places (a green, a ravine, a grove on a terrace). `LOOKS.md`: upright and
   straight on Earth's side, tangled and round on Life's.
6. **The plain has its own framing.** On the Hunt's plain only, the camera stands back to 1.3 times its distance and
   looks 2 m ahead of the player: the picture then reaches about **11 m north, 3.5 m south and 19 m wide**. A set piece
   of choosing routes needs the next stones in view; the pull-back is held all the way across, so the camera never
   moves in the middle of a run (3.5).

### 1.2 The two rules: stone remembers, wood forgets

**Decided** (`WORLD.md`): on Earth's side what is broken stays broken; on Life's side cut paths and broken things grow
back at each rest; the dead take root; respawned enemies climb out of their own stone or thicket. **Proposal** for what
each means, thing by thing:

| | Earth's side (the bog's west half, the King's Men's hill, the Dance) | Life's side (the bog's east margin, the wildwood, the grove) |
|---|---|---|
| Walls, stones, slabs a player or an enemy breaks | Stay broken for good, across rests, falls and sittings (a progress key each) | Living things (briar, hurdles, wicker, thickets) grow back at the next rest; stone and bronze stay broken (the cauldron, the clapper slab) |
| Cut paths | Nothing on Earth's side is cut; it is broken | Briar cut open is whole again after any rest anywhere |
| An enemy killed | Its body sets into stone where it fell: a grey shape in the floor, flat, kept (at most eight per room; the oldest weathers away) | Its body sprouts a low thorn mound (1.5 m, slows) that stays until the next rest |
| An enemy back after a rest | Climbs out of its own stone shape, where it died, not where it stood | Pushes up out of its thorn mound, then walks to its post |
| The land's own time | None: Earth's side is the same in every year | The year (1.3) |

Rules for the rooms:
- **Earth's breaks are permanent, so they are offered, never forced.** No main-path wall, slab or cover on Earth's side
  breaks unless the player or an enemy the player chose to fight breaks it; where breaking costs the player cover (a
  boar ploughing through a field wall), the room has more than it needs.
- **Life's regrowth is visible from a fire.** F5 has a briar-choked pen 6 m from it with Shards in it: cut in, take
  them, rest, and the briar is back in the same frame. That is the lesson, shown once, where it cannot be missed (3.3).
- **The bog is the border.** Its west causeway is Earth's, its east hurdle way and alder carr Life's: the first five
  minutes show each rule once (3.1).
- The plain and the meeting ground belong to neither: nothing there breaks for good or grows back.

### 1.3 The years (the barrows)

**Decided** (`WORLD.md`, level design rules): going through a barrow one way ages Life's side a year; going back the
other way makes it a year younger; no path is ever lost for good. "Each trip under ages Life's side by a year, so some
paths close and roots bridge others" (`WORLD.md`, "The land").

**Four years** (Proposal; question 3). The land's year is one number for the whole of Life's side, I to IV, held across
falls, rests and sittings (progress key `earthlife.year`), as the Water land holds its tide. The land starts at **II**,
so the first barrow a player walks can go either way.

| | I, the sapling year | II, the thicket year (start) | III, the high wood | IV, the windfall |
|---|---|---|---|---|
| Each Life mouth's **year hazel** | A switch-thin sapling | A dense bush | A tall young tree | Fallen, a stump with shoots |
| Briar on Life's lanes (cuttable; regrows at rest) | Low, off the lanes | Thick across lanes | Shaded out under the trees: lanes open | Back in the gaps the fallen trees left |
| The root bridge over the Alder Ravine | None | None | **Across**, 3 m wide | Torn up: the great alder has fallen |
| The great alder | A sapling | Young | Great; its roots bridge the ravine | **Fallen along the ravine floor**: a ramp down at its west end (the stag's hollow, side path) |
| The windfall oak at the grove terrace's foot | Standing | Standing | Standing | **Fallen north onto the terrace**: a ramp up (main path) |
| The druids' stair at the terrace's east end | **Clear** (a second way up) | Choked by roots | Choked | Choked |
| The alder roots to the crannog (the bog) | None | None | **Across** | **Across** |
| The village's old well | **Open** (Shards at the bottom) | A hazel grows in its mouth | Same | Same |
| The light on Life's side | Paler, blossom on the thorns | As `LOOKS.md` | Deeper green, the shafts narrower | Wider shafts through the broken canopy, grey deadwood |

So the main path through Life's branch asks for **III** to cross the ravine and then **IV** (or I) to climb to the grove.
Earth's side, the plain and the meeting ground are the same in every year (stone remembers; the plain is the Hunt's).

**A barrow passage.** Each has two mouths and one direction: walked in at one mouth, the year goes up by one when the
player comes out of the other; walked the other way, it goes down by one. At the ends of the scale the passage *sleeps*:
it can be walked, and the year does not change (question 3 asks about a wheel instead).

**Reading it from above** (question 4). Every mouth has an **entrance stone** lying flat at its foot, as Newgrange's lies
across its entrance: 2.5 by 1 m, its top fully in the camera's view as the player walks up to the mouth.
- On it, a carved **spiral**. At the mouth where walking in **ages** the wood the spiral turns **sunwise** (clockwise
  from above, the way the sun goes, the Gaelic *deiseal*); at the mouth where walking in **turns the year back** it
  turns **widdershins**. The rule is the same at every barrow in the land, and the first barrow teaches it (3.3).
- Beside the spiral, **four cup marks** in a row, one per year, joined by a groove. The cup of the present year holds
  a gold glint (worked gold is the land's accent: `LOOKS.md`); the cup of the year the player will come out into
  glows faint living green. A sleeping passage shows the gold cup alone and its spiral dark.
- A **year hazel** stands at every mouth on Life's side: the same tree in four forms (the table), so the year can be read
  from any mouth before going under, and the change seen on coming out (`LOOKS.md`: "coming out frames the year that
  passed").
- Coming out, a status line: "A year has passed." or "A year is undone." (timed text is for status lines; `CLAUDE.md`).
  The first time out of each barrow, the camera eases back for 3 s on the mouth and its hazel (a vista zone, V-type).

**Never lost.** Every rest fire on Life's side reaches a barrow mouth in every year without crossing ground that only
one year opens (1.5); every barrow can be walked both ways in every year; nothing a year closes is the only way to a fire.

### 1.4 The brawl, felt everywhere

**Decided**: distant thunder of stone, the roar of the wild, birds lifting in flocks, and now and then a tremor;
everywhere. **Proposal** for how:

- A **brawl clock** runs while the player is in `StoneGrove`: a beat every 45 to 75 s (random in that span, never two
  of the same kind in a row). Each beat plays one cue, chosen by where the player is. Its sound is placed in stereo from
  the meeting ground's bearing, so it always comes from ahead and a little to the side, the same compass as the sun.
- **Each side hears the other god.** On Earth's side the beats are mostly the wild: the stag's roar rolling over the
  hill, a gust carrying leaves and seed-down across stone floors that have no trees, starlings in a black ribbon. On
  Life's side they are mostly stone: a far crack like a quarry blast, a tremor that shakes dew off the canopy and sets
  the wicker cages creaking, rooks lifting from somewhere unseen. In the bog and on the plain, both.

| Cue | What the player gets | Where it is strongest |
|---|---|---|
| Stone thunder | A deep crack and a long rumble; dust trickles off stone tops in frame | Life's side, the plain |
| The roar of the wild | A bellow and an answering roar of leaves; a gust across the floor, the heather flattening in a line | Earth's side, the plain |
| Birds lifting | A flock crosses the frame from north to south, low, its shadows sliding over the floor first (rooks on Earth's side, starlings on Life's) | Everywhere |
| A tremor | 0.4 s of small camera shake (off with shake off), rings on every bog pool and puddle, a loose stone rolling off a wall top, cages swinging | Grows the nearer the meeting ground: none in the bog's south, strong on the rise |

- The beats come **faster and louder** toward the meeting ground: 75 s in the bog, 60 s in the branches, 45 s on the
  plain, every 20 s on the rise, and continuous at the gate.
- **Never in an arena** (the Dance's ring, the grove, the trial, the bowl), and never while a red attack is on the
  player. A beat changes nothing in play: no stone falls on anyone, no enemy reacts. It is weather.
- The seats are the exception: sitting in one plays the held shot (section 9).

### 1.5 Never stranded

1. Deep bog water and the plain's mere are never entered: the throw-back of `water_levels.md` 1.2 (back to
   `FallCatch.LastFirm` after 1.2 s, 10 damage).
2. Every one-way drop faces south, is 2 to 4 m, and lands where a barrow or a two-way route is within 30 m.
3. **The year.** Every fire on Life's side reaches a barrow mouth in every year (F5: the Hollow Hill on the green; F6:
   the two drops off the terrace's east end to the north bank, then the Long Cairn or the clapper bridge). The terrace
   is reached only in IV (or I by the druids' stair), but it is left in any year.
4. **The Hunt never kills.** Caught, the player is set down hurt; the damage cannot take the last point of health.
5. Earth's permanent breaks never close a way: walls only open when broken; stones that fall only fall into places
   that were not paths.
6. Waking is always at a fire, in the year the player left (the year holds), with Life's grown things grown back.

### 1.6 Measures

| What | Measure | Why |
|---|---|---|
| Walk, roll, step, rises, stairs, ramps, group fight | As Fire: 6 m/s, roll 4.5 m; a step 0.3 m; a rise that reads 1.5 m, a drop 2 m; stairs 27 to 32 degrees; ramps 8 degrees fought on, 15 walked; 12 to 16 by 10 to 12 m | `fire_levels.md` 1.2 |
| The plank road | 3.2 m wide (Corlea's oak planks are 3 to 3.5 m long, laid across runners), y 0.5; sunk stretches 0.3 m under water: wading 4 m/s, roll 3 m | One-on-one; the bog either side is not a floor |
| A lynchet (the boar fields) | A field terrace 2 to 2.5 m high, facing south, climbed by cart ways (ramps 4 m wide, 14 degrees) | Reads as "up"; the boars cannot plough up it |
| A field wall | 1.2 m, dry stone, in sections 4 m long; a section breaks for good when a boar ploughs into it | Under the south-side rule; cover that can be spent |
| A hollow way | 6 to 8 m wide between banks 2.5 m | The haulers' track; a lane a rolling stone fills |
| The King's Men's sight | The player's facing, 110 degrees, 15 m; shown on the heather as a faint pale fan while a King's Man is within 15 m | Wide enough to hold two, never three standing apart |
| A barrow passage | 2.4 m wide, walls 2.2 m, corbelled; chambers 5 to 7 m across | One at a time; nothing fights in a passage |
| The Hunt's stones | Rings of untouched heather r 1.5 (small), 2 to 3 (standing stones), 4 (great stones, the Cove), the Last Ring's whole inside (r 6) | Small rings hold the player alone; great rings hold a fight |
| The Hunt's clock | Band A every 12 s, band B 10 s, band C 8 s; first horn 5 s before, second 1.5 s before; the front 16 m/s, 6 m deep | 3.5 |
| Caught by the Hunt | 25 damage (cannot kill), knocked down 1 s, set down behind the plain's west wall at the same latitude | Question 2 |

### 1.7 The land's people, as the rooms use them

All eight corruptions are **Decided** (`WORLD.md`, "The enemies"); how each fights here is **Proposal**, each mechanic
its own (the owner's bar), each tied to its side's rule. Numbers are first values.

**Earth's side.**
- **The King's Men** (an army turned to stone mid-march; the Rollright Stones). They move only while the player is not
  facing them.
  - *Sight.* A King's Man inside the player's facing (110 degrees, 15 m) is stone: still, grey, lit, its ink edge hard.
    Outside it, it moves at 3.5 m/s with a grinding sound and a dust trail, darker, edge soft. While one is within 15 m,
    the player's facing is drawn as a faint pale fan on the heather (`LOOKS.md` rule 10: a mark made of the land, here
    dew lifting off the heather), so the rule is read, not guessed.
  - *The frozen blade.* Within 1.6 m an unwatched King's Man swings its stone sword (0.6 s, guardable). Turned to
    mid-swing, it freezes **mid-swing** and completes the blow the moment it is unwatched again: a frozen blade at the
    player's back is the land's cruellest lesson, and the reason to kill what you face.
  - *Breaking.* Light blows chip (no stagger), a heavy cracks; three heavies or a finisher shatter it. Struck while
    frozen, it cannot guard. Shattered, it stays as rubble (Earth remembers); after a rest it climbs out of its own
    rubble.
  - *The ranks.* Most stones on the hill are only stones; six of them are men. From above the men are told by one thing
    only: a sword held across the body, its line pale on the stone. The stones cannot be counted (the legend), and the
    player learns to read the swords, not to count.
- **The countless** (cairns that stand up as bodies). A heap of about twelve stones on two legs that throws stones
  (from 10 m, in frame) and slams. Brought to nothing, it **scatters**: its stones bounce out 2 m and roll back together
  over 3 s. Among them lies **the one stone that differs**: freshly broken, ochre, unweathered, among grey lichened
  ones (`LOOKS.md`: ochre is fresh-broken rock), the only warm thing in the scatter. Strike it within the 3 s and the
  cairn dies for good (a low cairn left where it fell); miss it and the countless stands again whole, the ochre stone
  somewhere new inside it.
- **The boar's litter** (Twrch Trwyth's young, led by Grugyn Silver-bristle). They root up the ground.
  - *The furrow charge.* Head down, a boar ploughs straight at the player at 9 m/s; the turf heaves ahead of it in an
    ochre crack line 0.7 s before (Earth's lane, `LOOKS.md`). Caught: 22 and thrown 3 m.
  - *Furrows stay* (Earth remembers): every charge leaves a furrow 1 m wide and 0.3 m deep. The player crosses a furrow
    at half speed; **a boar runs along an old furrow at double speed**, like a rail. A field fought in long fills with
    furrows and the litter gets faster: kill quickly, or bait charges across each other so the lines are short.
  - *Walls.* A boar that ploughs into a field wall is stunned 1.5 s, and the wall section breaks for good: cover spent to
    buy an opening.
  - *Grugyn* is larger, silver-bristled, and the others follow his furrows. Kill him and the rest charge only straight
    at the player, never along furrows.
- **The haulers** (gangs dragging standing stones). Four men on two ropes, hauling a stone on rollers up the hollow way
  toward the Dance. They do not stop hauling unless struck. Struck, a rope's pair drops it and whirls the slack as a
  **lasso**: a pale loop on the ground 1 s ahead; caught, the player is dragged 3 m toward the stone and held 0.8 s. The
  stone creeps uphill while two or more haul; when the last rope falls slack (two dead on each rope, or both ropes'
  pairs), it **rolls back** down the track: anyone in the track's lane below it takes 50 and is thrown aside. The stone
  stays where it comes to rest (Earth remembers). After a rest the haulers climb out of their own stones where they
  died, walk down the track to the stone and start hauling it up again: a player using the track later meets them on
  it.

**Life's side.**
- **The Green Men** (people with the forest growing through them). A branch-arm sweep (3 m, guardable) and a root lane
  (a line of thorns sprouting along the floor toward the player, 6 m, red at its head). After about a third of their
  health is lost they **root**: 0.8 s to sink, three roots spreading out 1.5 m on the floor, then healing 6 a second
  under bark (blows do 30%). A blow on a root cuts it; with all three cut it is **uprooted**, staggered 1.5 s and open.
  Left alone, it pulls itself up at full health. They root at most twice.
- **The mute reborn** (warriors from the cauldron). They make no sound at all: no footstep, no grunt, no swing. Their
  wind-ups are visible and a little longer than a living swordsman's, but nothing is heard; the land's other sounds
  (the brawl, the Hunt's horn) never wait for them. Killed in a cauldron's district, a reborn sinks into the earth and
  climbs out of the cauldron 8 s later, wet and grey. The cauldron is **broken from inside** (`WORLD.md`): see the Feast
  Yard (3.3). On the plain they are the Hunt's grey dead.
- **The wicker** (walking cages with a living person inside). A wicker walks at the player and swings its cage-body
  (heavy, slow, 2.5 m). Blows break the cage, not the person. At nothing the cage splits and the captive tumbles out; for
  3 s strands of willow reach to close round them again (a ring of green shoots on the floor, drawing in). Two blows on
  the strands, or stepping into the ring, and the captive is free: they run to the nearest dark lamp and light it, and
  the Shards are the player's. Left, the cage weaves shut with the captive inside at half strength.
- **The overgrown herd** (stags with thicket antlers). The charge comes in a straight line with its antlers lowered
  (red); caught, the player is **tangled** 1.2 s: slowed 50%, no roll, and the stag turns and tosses. A stag sheds its
  antlers once below half health; they take root as a thorn patch 2.5 m across (bites 4 a second, slows), which stays
  until the next rest. A thorn patch cuts across straight lines: on the plain, it changes routes (3.5).

---

## 2. The whole land

The land runs north from the border bog to the Fork; Earth's branch climbs west and north to the Giant's Dance, Life's
branch east and north to the sacred grove; between them the Hollow Hills, full of barrows, with the barrow road under
them to the plain; both seats look out over the plain from its south-west and south-east; beyond the plain the rise, the
gate barrow and the meeting ground. Eight rest fires. The main path is about 720 m of walking (Fire about 560).

```
 z
600                ( MEETING GROUND: bowl, centre (6,18,572), r 28 )        -> MeetingGround (own scene)
542                     gate barrow: passage 19 m NNE; chamber (-3,20,524)
516                     F8 * forecourt (-4,22,516), two seals
510      oath stone (-18,18,500)   BROW (0,22,510) H3   salmon pool (18,18,496)
482                     THE RISE y14 -> 22
476   [W lane]   THE LAST RING (0,14,476)
448   [  S5  ]   band C: the Cove (-4,13,456)            WILD HUNT'S PLAIN
404   [ pens ]   band B: Broken Avenue W | the mere | the Scatter E      x -56..56, y 10 -> 14
360   [      ]   band A: the Hunt Stone (0,10,368)
356  Dance barrow (-62,10,356) ===== F7 * (-4,10,356) ===== Grove barrow (62,10,356)
340  GIANT'S DANCE (-100,22,340)         HOLLOW WAY y6->10          SACRED GROVE (100,12,346)
318  F4 * (-88,20,318)  cairns          (barrow mounds)            F6 * (104,10,318) terrace y10
300  haulers' head                       |                          feast yard, cauldron; Long Cairn
276  KING'S MEN'S HILL y12->19   [trial] |                          ALDER RAVINE y0 (banks y4)
250  F3 * (-92,12,246)  track foot       FORK MOUND passage          F5 * (118,4,262) Hollow Hill
220  BOAR FIELDS (lynchets) y5->12       (owl's chamber, z 218)      VILLAGE GREEN y4; wildwood lanes
186  causeway west y3->5 <-------- THE FORK y3, F2 *(-4,3,162) --------> alder carr, Hazel Gate y3
140                            bog cuttings y0.5 -> 2.5
 52   stone causeway W  |  THE BLACK POOL  |  hurdle way E   (crannog (36,1.5,78))
 12                            plank road y0.5
  0                            @ wake (0,0.5,0)  F1 *(-4,1,6)
      x: -160   -100    -50      0      50     100    160
```

**Heights.** Bog 0 to 0.5 (pools -0.6, banks 2.5); the Fork 3. Earth: causeway 3 to 5, fields 5 to 12, hill 12 to 19,
hilltop 20, the Dance 22 (lintel tops 27). Life: carr 3, green and banks 4, ravine floor 0, terrace 10, grove 12. Hollow
Way 6 to 10; plain 10 to 14; rise 14 to 22; the bowl 18 (rim banks 27).

**Rest fires (eight).** A fire here is a fire in a ring of stones on Earth's side, in a ring of roots on Life's.

| Fire | Where | Foot | Why there |
|---|---|---|---|
| F1 | The landing, by a sunk post (the blackbird) | (-4, 1, 6) | In sight of waking; S1 comes down to it |
| F2 | The Fork | (-4, 3, 162) | Where both branches start; the barrow road comes out 24 m from it |
| F3 | The top of the boar fields, at the hollow way's walled foot | (-92, 12, 246) | Before the King's Men; S2's stone comes down to it |
| F4 | By the King Stone, the Dance's door | (-88, 20, 318) | Before the Bargain |
| F5 | The village green's north side, by the Hollow Hill | (118, 4, 262) | Life's rule taught here; the first barrow beside it |
| F6 | The grove terrace | (104, 10, 318) | Before the Wicker Man; the grove's door |
| F7 | The plain's south terrace, at the barrow road's head | (-4, 10, 356) | Before the Hunt; both seats' barrows come out on this terrace |
| F8 | The gate barrow's forecourt (in `MeetingGround`) | (-4, 22, 516) | The god's door (Decided) |

**The route, Earth first.** Wake on the plank road; north past the Black Pool (either side) and up the cuttings to the
Fork (F2). West along the causeway, up the boar fields (F3), up through the King's Men, across the haulers' head and the
cairn field to the King Stone (F4); the Bargain in the Dance. Sit in the seat (the brawl, the first seal breaking far
off). Down the Dance barrow to the plain's terrace (F7); the plain is quiet (3.5). Home under the Hollow Hills by the
barrow road, opening it from its north end, to the Fork. East through the alder carr and the Hazel Gate, the wildwood
lanes, the green (F5); under the Hollow Hill to the high wood; over the root bridge; the Feast Yard and its cauldron;
under the Long Cairn to the windfall; up the fallen oak to the terrace (F6); the Wicker Man in the grove. Sit in the
seat (the brawl; the second seal bursts and the Hunt pours out). Down the grove barrow to F7. The Hunt. Up the rise (the
brawl seen), the gate (F8), the meeting ground.

**Life first** is the same with the branches swapped: the grove barrow brings the player to the quiet plain, the barrow
road home, Earth's branch, the Dance barrow to the Hunt.

**Why the plain is quiet between the seals** (question 1). The gods' seals are on the meeting ground's gate (Proposal
in `WORLD.md`); the Hunt is Carnwyn's herd and the dead, shut in the meeting ground with the gods while either seal
holds. When the second breaks, the gate stands open and the Hunt rides out. So a player between branches may walk the
plain, learn its stones in peace, climb the rise and see the gate with one seal broken: the goal shown early, earned
late (the owner chose the same shape for Metal's sealed heart). The Hunt is still one set piece, met once.

**The barrows.**

| Barrow | Mouths | Walking in here **ages** (sunwise spiral) | Opens | Section |
|---|---|---|---|---|
| The barrow road (the Fork Mound, a long gallery with the owl's chamber) and the Hollow Way | The Fork (0, 3, 186); the Hollow Way's foot (0, 6, 248) | The Fork's mouth (going north) | From its north mouth only: a kerbstone rolled aside | 3.4 |
| The Hollow Hill (a horseshoe passage) | The green's west (109, 4, 270) and east (131, 4, 270) | The west mouth | Open | 3.3 |
| The Long Cairn | The north bank's east (94, 4, 302) and west (64, 4, 302) | The east mouth | Open | 3.3 |
| The Dance barrow | The Dance's north-east gap (-86, 22, 354); the plain's terrace (-62, 10, 356) | The Dance's mouth (going down) | When the Bargain falls | 5.1 |
| The grove barrow | The grove's north-west (86, 12, 360); the plain's terrace (62, 10, 356) | The grove's mouth (going down) | When the Wicker Man falls | 5.2 |

Either first branch nets out: Earth first goes II, +1 by the Dance barrow, -1 home by the barrow road, and enters Life
at II. Life first ends at IV (the windfall), the grove barrow sleeps at IV, the barrow road home makes it III: the bog's
crannog roots are across when the player passes it (section 8).

**Branches and loops.**
- The two ways round the Black Pool (west: stone; east: wood): both on the main path.
- The bog's west bank (S1), the hollow way (S2), the clapper bridge (S3), the barrow road (S4), the plain's west lane (S5).
- The crannog (see it from the hurdle way; reach it in III or IV), the stag's hollow (see it from the root bridge in III;
  reach it in IV), the eagle's lintel (see it from inside the Dance; reach it by a fallen upright), the druids' stair
  (Year I), the owl's chamber, the salmon's pool and the oath stone (section 8).
- The trial (section 7).

---

## 3. The districts

Each section gives the idea and the levels, then each space in walking order. Enemy names are section 1.7's.

### 3.1 District 1: the border bog (z -10 to 186)

**The idea.** A black bog between two lands, crossed by an oak road laid on it (the nod is the Corlea trackway, oak
planks 3 to 3.5 m long on runners, built in 148 BC and sunk into the peat within ten years). The road is the brightest
line in the frame (`LOOKS.md`). Halfway, the Black Pool parts it into a road of stone on the west and a road of woven
hazel on the east: the land's two rules, one on each hand, before either branch. **Levels:** pools and peat (y -0.6 to 0;
the pools are never entered), the roads (y 0.4 to 0.8), the cutters' banks and the Fork (y 2.5 to 3).

```
 z     x: -45   -30   -15     0     15     30    45
186   =========== THE FORK y3 =====[ barrow road mouth (0,3,186) ]=====
170   causeway W <-  F2*(-4,3,162)  Fork Stone (0,3,170)  -> alder carr E
150   |cutters'|            .--- cuttings: two cut faces ---.
122   | bank   |[marker stone (-38,2.5,122) / drain]       |
118   |  y2.5  |       (c)  peat stacks   (g)             |
100   |  S1    |  ==========  road rejoins  ============   |
 72   |        | (c)causeway W |  THE BLACK POOL  | hurdle way E (g)  ~~ (36,1.5,78) CRANNOG
 60   |        |    y0.8       |     (deep)       | briar  y0.4 (g)
 52   |        |  ====== the road splits (0,0.5,52) ======   |
 47   |        |                       (g) island            |
 38   |        |   (c) island    plank road y0.5             |
 26   |        |          ~~ sunk stretch z26..32 ~~         |
 22   |        |                bog body                     |
  6   '--ramp--> F1*(-4,1,6)   @ wake (0,0.5,0)   the landing knoll y1
```
`(c)` countless; `(g)` Green Man.

#### 1A. The landing (z -10 to 12, x -12 to 12, y 1)
- *Shape:* a dry knoll of heather at the bog's south edge where the road begins. The player wakes on the first planks
  looking north along the road into the mist, the low sun a pale disc dead ahead (`LOOKS.md`).
- *Rest:* **F1** at (-4, 1, 6), by a sunk post with **the blackbird** on it (section 8).
- *The arrival shot* (H0, question 6): the first time only, the camera starts high and far behind the player and holds
  6 s on the land in one image (`LOOKS.md` frame `el_plank_road`): the road winding north, the stone circle on its hill
  to the left, the great oak over the forest to the right, a rise between them hiding the meeting ground. It settles to
  the game camera as the sponsor speaks.
- *Enemies:* none.

#### 1B. The plank road (z 12 to 52)
- *Shape:* the road, 3.2 m wide, y 0.5, winds north-north-west then north-east between black pools and small peat
  islands (y 0, walkable, joined to the road by a plank or two). At z 26 to 32 it has **sunk**: 0.3 m of black water over
  the planks, wading (1.6). Bog cotton, bones half out of the peat.
- *Lore:* **L1**, a body in the peat beside the road at (5, 0, 22), a hand still on the stake he was pinned with.
- *Enemies:* E1.1 a **countless** on a peat island west of the road at (-8, 0, 38) (the island 8 by 6 m): the first
  scatter, the ochre stone among the grey, and the low cairn it leaves for good when struck. E1.2 a **Green Man** rooted
  as a sapling on an island east at (8, 0, 47); it pulls up as the player passes within 4 m: rooting and cut roots.
  Earth's lesson first, then Life's (the decided teaching order).

#### 1C. The Black Pool (z 52 to 100)
- *Shape:* the road meets a long pool (x -12 to 12, deep) and parts round it; both ways rejoin at (0, 0.5, 100).
  - **West, the stone causeway** (x -24 to -16, y 0.8, 4 m): older slabs on a bank, the shapes of bodies worn into
    them (the dead set into stone, long ago). E1.3 a **countless** at a cracked slab (-20, 0.8, 72). What breaks here
    stays: its low cairn, the slab its slam breaks.
  - **East, the hurdle way** (x 14 to 22, y 0.4, 3 m): woven hazel hurdles laid on the peat at the alder carr's edge,
    choked with briar at z 60 to 64 (cut: three blows a metre; it grows back at any rest). E1.4 two **Green Men**: one
    rooted on the hurdles at (18, 0.4, 76), one at the carr's edge (28, 0, 86) that wakes as the player passes.
- *See it:* from the hurdle way at (24, 0.4, 78), across 10 m of black water, **the crannog**: a timber island with a
  roundhouse's ring of posts and something gold on its hearthstone (section 8). In III and IV the carr's alder roots
  reach it from (25, 0.4, 82).

#### 1D. The cuttings (z 100 to 140, x -20 to 20)
- *Shape:* the road climbs out of the bog through old peat cuttings: two cut faces of 1.25 m facing south (black,
  layered, their edges crumbling: they read as steps up), climbed by peat ramps, with stacks of drying turves (1.2 m) on
  the cut floors as cover.
- *Enemies:* E1.5 the two sides together, a small peak: a **countless** behind a turf stack at (-6, 1.7, 118), a
  **Green Man** at (8, 1.7, 120), a second at (2, 2.9, 132) on the upper floor. Killed side by side, the countless leaves
  stone and the Green Man thorns: the land's two deaths in one frame.

#### 1E. The Fork (z 140 to 186, x -24 to 24, y 3)
- *Shape:* a heathery knoll at the bog's head. **F2** at (-4, 3, 162). **The Fork Stone** at (0, 3, 170): a standing
  stone 2.4 m high whose south face (toward the camera) is carved with a ring of stones on its west half and an oak on
  its east, and at its foot a double spiral. The stone causeway leaves west at (-24, 3, 165), the carr path east at
  (24, 3, 165). On the knoll's north side, the south mouth of **the barrow road** (0, 3, 186), its entrance stone lying
  flat at its foot (a sunwise spiral, the four cups); through the mouth, 6 m in, the round kerbstone that shuts it from
  the far side.
- **V1** at the Fork Stone (0, 3, 168), 8 by 4 m: the camera eases back and shows both ways out, the causeway's stones
  going west, the alder carr going east.
- *Enemies:* none.

#### The cutters' bank (the bog's loop, S1)
- The west edge of the bog is a raised bank of uncut peat (x -45 to -32, y 2.5) running from the Fork (-24, 3, 150)
  south to F1. At z 120 a drain 3 m wide cuts it. On the drain's north lip leans a **marker stone** (-38, 2.5, 122):
  from the north, "Topple the stone"; it falls south across the drain and lies there as a bridge for good (Earth
  remembers). The bank's south end comes down a peat ramp to (-12, 1, 8), 8 m from F1. No enemies. **S1.**

### 3.2 District 2: Earth's branch (x -160 to -40, z 160 to 356)

**The idea.** A hill climbed in straight lines: a causeway, terraced fields, ranks of stone men, a hollow way, a ring
on the top. Everything that breaks here stays broken, so the player's own fights write themselves into the hill. The
light is the clearest in the land, low and raking, the mist pushed down into the bog behind (`LOOKS.md`). **Levels:** the
causeway and the fields (y 3 to 12), the hill's slope and the hollow way (y 12 to 20), the hilltop and the Dance (y 20 to
22; the lintels at 27 are the eagle's side path only).

```
 z     x: -160  -140  -120  -100   -80   -60   -40
356        GIANT'S DANCE (-100,22,340) r16      -> Dance barrow NE (-86,22,354)
324        ring's SE gap (-90,22,326)
318        F4*(-88,20,318)  King Stone (-82,20,320)
304   CAIRN FIELD y20 (c)   (c)          | haulers' head (H)(H)  track x-80..-72
292   [King's back]  rank 4 y17.5  (k)   | stone (-76,18.5,290)
278   rank 3  (k)         (k)            |       bank gap E (-72,15,276) -> trial
268   rank 2     (k)          (k)        |  HOLLOW WAY y12->20 (S2)
258   rank 1          (k)  KING'S MEN'S HILL y12->19
246   field 4 y12   V2(-110,12,246)  F3*(-92,12,246)  walled foot (-76,12,248)
232   field 3 y10   (G)(b)  <- cart way W
218   field 2 y7.5     (b) (b)       cart way E ->
202   field 1 y5     <- cart way W         gate (-96,5,192) <-- CAUSEWAY WEST y3->5 <- (-24,3,165)
```
`(k)` King's Man; `(b)` boar; `(G)` Grugyn; `(H)` haulers; `(c)` countless.

#### 2A. The causeway west (from (-24, 3, 165) to (-96, 5, 192))
- *Shape:* a stone causeway 5 m wide on a low bank across heather, climbing gently west and north. Ochre cracks spread
  from its kerbstones; two deep ruts cross it where stones were dragged north. Cairns stand by it at intervals; most are
  only cairns.
- *Enemies:* E2.1 a **countless** at (-60, 4, 176) beside a cairn that is not one: telling them apart (the countless's
  stones are set in a body's shape, its ochre stone shows through a gap).
- *Lore:* the dead set in the causeway's slabs, old and worn smooth.

#### 2B. The boar fields (x -150 to -96, z 186 to 244)
- *Shape:* four **lynchet** fields, each a terrace 2 to 2.5 m above the last with its face toward the camera (a lynchet
  is the bank a hillside field builds up over centuries of ploughing; they read as steps of a stair from above). Each
  field is split by dry-stone walls 1.2 m into plots with gaps. The way up zig-zags by cart ways at alternate ends: field
  1 (y 5, z 186 to 202) entered at its east gate (-96, 5, 192), up at the west (-146); field 2 (y 7.5, z 202 to 218), up
  at the east (-102); field 3 (y 10, z 218 to 232), up at the west (-140); field 4 (y 12, z 232 to 244).
- *Enemies:* E2.2 two **boars** rooting in field 2 at (-126, 7.5, 210) and (-112, 7.5, 214), a few old furrows already
  in the field. E2.3 **Grugyn Silver-bristle** and a boar in field 3 at (-128, 10, 226) and (-116, 10, 224); field 3 is
  criss-crossed with his old furrows, so his litter is fast here from the start, and the walls are the answer: draw a
  charge into a wall to stun it (the wall stays broken).
- *Why it suits them:* fields are what boars root up; walls give stuns at a price that lasts; the furrows written into
  the ground are the land's rule as a fight that gets worse the longer it goes.
- *Rest:* **F3** at (-92, 12, 246), field 4's east end, beside the hollow way's foot, which is walled across (-76, 12,
  248): through a gap in the wall the player sees the track climbing north out of frame, ruts and rollers on it.
- **V2** at field 4's top (-110, 12, 246): the camera eases back up the hill. The first two ranks of the King's Men
  stand in frame, their long shadows laid down the slope toward the player like the rungs of a ladder.

#### 2C. The King's Men's hill (x -150 to -86, z 248 to 298, y 12 to 19)
- *Shape:* an open slope of short heather and lichened rock, 8 degrees. Across it, **four ranks** of stone soldiers
  frozen mid-march eastward, at z 258, 268, 278 and 288, a stone every 3 m, about sixty stones (the Rollright legend:
  they cannot be counted). Each rank has gaps where stones fell long ago (they lie in the heather); the way up threads
  the gaps, which never line up. At the top of rank 4's west end, **the King's back**: a bank of fallen stones 1.4 m high
  from (-136, 17.5, 292) to (-124, 17.5, 292).
- *Enemies:* six of the stones are men.
  - E2.4 **the first** stands in rank 1's gap at (-116, 13, 258), the gap the player must use. Face it and it is stone;
    the floor's pale fan shows it held. Pass it and turn away, and it follows.
  - E2.5 **the march**, five in ranks 2 to 4 at (-128, 14.5, 268), (-104, 14.5, 268), (-120, 16, 278), (-96, 16, 278)
    and (-112, 17.5, 288). A player climbing north faces north: the men in ranks already passed close in from behind.
    *The answers:* turn and kill what you face (struck while frozen, they cannot guard); raise the guard, which squares
    up to the nearest and holds it frozen; put your back to the King's back so the ranks can come only from the front,
    where they stop; or move so two stand inside one fan.
- *Why it suits them:* ranks and gaps make lines of approach the player can read from above; climbing north means
  turning away from what is behind, the one thing the King's Men need.
- *The light:* each rank's long shadow falls across the rank below; the slope reads as bars of shadow and lit heather.

#### 2D. The haulers' head (the hollow way, x -80 to -72, z 248 to 306, y 12 to 20)
- *Shape:* the hollow way, a track 8 m wide sunk between banks 2.5 m, climbing north at 8 degrees from its walled foot
  by F3 to the hilltop. The player comes onto it near its head from the hill's east end, through a gap in its west bank
  at (-82, 19, 296).
- *Enemies:* E2.6 **the haulers**: four at (-78, 19.5, 298), (-74, 19.5, 299), (-78, 20, 303) and (-74, 20, 304), on two
  ropes to a standing stone 4 m tall on rollers at (-76, 18.5, 290), hauling it north. The stone is south of where the
  player enters, so when the last rope falls slack it rolls **away** down the track: 42 m, through the wall at the foot
  with a crash heard across the hill, coming to rest beside F3. **S2** (section 4).
- *Why it suits them:* the track is a lane the stone fills; entering beside the haulers, not below the stone, is what
  makes their rule a reward and not a trap the first time.
- *Off it:* a gap in the east bank at (-72, 15, 276) leads east to the trial (section 7).

#### 2E. The cairn field and the King Stone (x -122 to -80, z 304 to 322, y 20)
- *Shape:* the hilltop's south shoulder, scattered with six cairns. E2.7 two of them are **countless**, at (-112, 20, 308)
  and (-96, 20, 312), among four that are only cairns.
- *Rest:* **F4** at (-88, 20, 318), beside **the King Stone** (-82, 20, 320), a lone leaning stone apart from the ring.
- *Lore:* **L3**, cut in the King Stone (Claude's draft, to be set in the voice): "He took seven strides to see the
  land." / "On the seventh the hill rose. He stands here still, and cannot see."
- *Out:* the Dance's south-east gap (-90, 22, 326), up a short stair of turf.

#### 2F. The Giant's Dance
See section 5.1.

### 3.3 District 3: Life's branch (x 20 to 160, z 160 to 364)

**The idea.** A wood that changes. Lanes close behind the player at every rest, the dead sprout thorns, and under the
barrows the wood can be walked a year older or younger. Round and tangled where Earth's branch is straight: a green,
roundhouses, a ravine, a terrace grown over, a grove. Dim under the canopy, with gold shafts falling through
(`LOOKS.md`). **Levels:** the ravine floor (y 0, the side path), the carr, lanes, green and banks (y 3 to 4), the terrace
and the grove (y 10 to 12).

```
 z     x: 20    40    60    80    100   120   140   160
364                     SACRED GROVE (100,12,346) r18  -> grove barrow NW (86,12,360)
328                 windfall ramp top (72,10,328)  root steps (96..108)
318   TERRACE y10   (g)      F6*(104,10,318)        drops (140) | druids' stair (150) (I)
310   ====== terrace face 6 m ======================================
302         [LONG CAIRN W(64) .... E(94)]  FEAST YARD  (r)  CAULDRON (126,4,303)  (r)
296   ~~~~~~ ALDER RAVINE floor y0 ~~~ root bridge (122..126) (III) ~~~ clapper (150..156) S3
290      stag's hollow (78,0,290)  <- fallen alder (IV)
284   north lane (134..142) ^
276                        [HOLLOW HILL W(109)   E(131)]  F5*(118,4,262)  briar pen (110,4,266)
250                  VILLAGE GREEN y4  (w)  (g)     (g)  (w)   well (116,4,244)
224                     glade (s)
210               lanes  (w) lamp
184         HAZEL GATE (62,3,182) (g) (g)
170   <- (24,3,165) ALDER CARR y3  briar
```
`(g)` Green Man; `(w)` wicker; `(s)` stag; `(r)` mute reborn.

#### 3A. The alder carr and the Hazel Gate (x 24 to 72, z 165 to 192, y 3)
- *Shape:* a hurdle path 3 m wide through wet alder woodland, black pools either side (the throw-back), briar on the
  hurdles at (40, 3, 172) to cut. At x 58 to 62 an old field wall runs north-south, 1.6 m, grown over with briar so
  thick that nothing passes it (the hint for Thorn Wall in `steel/depth_elements.md`); its gateway at (62, 3, 182) is
  choked: cut through.
- *Enemies:* E3.1 two **Green Men** rooted as saplings on the lane beyond the gate, at (74, 3, 184) and (80, 3, 192).

#### 3B. The wildwood lanes (x 72 to 112, z 190 to 232, y 3 to 4)
- *Shape:* lanes 4 to 5 m wide winding north-east between hazel stools and oak trunks (1.2 to 2 m thick: cover); roots
  across the lanes as 0.3 m steps; every canopy fades within 6 m of the player and none stands within 4 m south of a lane
  (as Ice's pines).
- *Enemies:* E3.2 a **wicker** with a captive at a lane crossing (92, 3.5, 210); a dark lamp on a post at (96, 3.5, 214)
  that the freed captive lights. E3.3 an **overgrown stag** in a glade 14 by 12 m at (104, 4, 224): the tangle, and the
  thorn patch its shed antlers leave in the glade until the next rest.

#### 3C. The village green (x 90 to 150, z 232 to 272, y 4)
- *Shape:* a green 30 by 24 m ringed by six roundhouses with trees grown up through them (an oak, an ash, a hawthorn
  among them), their roofs long gone, so each is a walled yard: walls 1.4 m on the south, 2 m elsewhere, walkable
  inside. The old well at (116, 4, 244). Four lamps on posts round the green, dark.
- *Enemies:* E3.4 the district's first peak: two **Green Men** rooted as saplings in the green at (106, 4, 250) and (128,
  4, 254), two **wicker** walking at (100, 4, 240) and (138, 4, 258). Green Men root to heal while the wicker come on;
  freeing a captive lights a lamp and the green brightens.
- *Rest:* **F5** at (118, 4, 262), on the green's north side.
- *The lesson of wood forgetting:* a briar-choked hurdle pen at (110, 4, 266), 6 m from F5, Shards on a stone inside.
  Cut in, take them, rest: the briar is whole again in the same frame (1.2).

#### 3D. The Hollow Hill (centre (120, 4, 276), radius 11)
- *Shape:* a round barrow on the green's north side with a kerb of carved stones. Two mouths on the green: **west**
  (109, 4, 270), its entrance stone's spiral **sunwise**, and **east** (131, 4, 270), **widdershins**. Between them a
  horseshoe passage 36 m long under the mound, with a small chamber at its apex (120, 4, 284). A year hazel at each
  mouth.
- *The teaching:* the first barrow a player walks. Both mouths open on the same green, so going under and coming out
  frames the very place a year on. In at the west mouth (9 m from F5) and out at the east: II becomes III. **V3** at the
  east mouth (131, 4, 268), the first time only: the camera eases back for 3 s on the green. The hazel by the mouth is a
  tall young tree; the briar across the north lane has gone, shaded out; the cup marks on the entrance stone at the
  player's feet show the gold in the third cup. Back through, east to west, and it is II again.
- *Enemies:* none in the passage, none ever (1.6).

#### 3E. The north lane and the Alder Ravine (x 70 to 160, z 270 to 296)
- *The north lane* (x 134 to 142, z 270 to 284) goes round the Hollow Hill's east side to the ravine's south bank. Briar
  closes it in II and IV (cut), open in I and III.
- *The ravine:* a stream in a cleft running east-west, floor y 0, both banks y 4. Its north wall is sheer, roots and old
  stone, facing the camera; its south side is a slope of ferns and roots at 40 degrees (seen, not walkable).
- *The crossing:* **the great alder** on the north bank at (124, 4, 298). In **III** its roots span the ravine as a
  **root bridge** 3 m wide (122 to 126, z 284 to 296): the way across. **V4** at its middle (124, 4, 290): the camera
  eases back and looks west down the ravine to **the stag's hollow** (78, 0, 290), where an old stag stands by a stone
  with a scythe hung in a hawthorn over it (section 8). In **IV** the alder has fallen: the bridge is torn up and the
  trunk lies west along the ravine from the south bank's lip (122, 4, 290) down to the floor (94, 0, 290), a ramp
  8 degrees, the way down to the stag. In I and II there is no crossing.
- *The clapper bridge* at the ravine's east end (x 150 to 156): three long stone slabs on two piers; the middle slab has
  fallen and leans from its pier into the stream. From the north bank, "Lever the slab" (with a fallen post): it grinds
  up onto its pier and stays (stone remembers, even here). **S3.**

#### 3F. The Feast Yard and the cauldron (x 100 to 150, z 296 to 310, y 4)
- *Shape:* on the north bank, a roofless feast hall: walls 1.2 m on the south, 2.5 m on the north, stone benches, the
  stumps of its posts. In its middle **the cauldron** (126, 4, 303), the Dagda's nod: a bronze bowl 6 m across sunk into
  the floor and furred with moss (`LOOKS.md`: the one dark metal shape in a green place). Its rim is level with the floor
  on the south, so the camera sees into it and a root ramp 1.5 m wide goes down into it; it is 2 m deep, black water in
  its bottom. Round its inside run **three mended seams**, bands of brighter bronze, seen only from within.
- *Enemies:* E3.5 three **mute reborn** at (116, 4, 300), (136, 4, 302) and (126, 4, 308). Each killed sinks and climbs
  out of the cauldron 8 s later, wet and grey. The yard cannot be cleared from outside.
- *Breaking it from inside* (Decided: it brings them back until it is broken from inside): the player goes down into the
  bowl and lands a heavy blow on each of the three seams; the reborn come down after them into a fight 6 m across. At the
  third the cauldron splits, its water runs out across the yard floor, and the reborn there fall and stay down. Bronze
  stays broken for good. In the myth Efnisien lay down in the cauldron and burst it, and died of it; here the player
  stands in it and lives.
- *Lore:* **L6** round the cauldron's rim (Claude's draft): "Never empty." / "What goes in comes out again, and does
  not speak of it."

#### 3G. The Long Cairn and the windfall (x 64 to 94, z 298 to 328)
- *The Long Cairn:* a long cairn on the north bank west of the yard, mouths **east** (94, 4, 302), sunwise, and **west**
  (64, 4, 302), widdershins; a straight passage 30 m. From III, east to west: **IV**.
- *The windfall:* coming out of the west mouth in IV, the **windfall oak** that stood at (70, 4, 308) lies fallen north,
  its trunk 2.5 m wide a ramp from (70, 4, 306) to the terrace at (72, 10, 328): 23 m, 15 degrees, bark and moss, its
  crown spread on the terrace. In I to III it stands, and the 6 m face beside it cannot be climbed. The first exit in IV
  is a V-type framing like V3 (the oak falling is not shown; it is found fallen).
- *The other way up:* in **I**, the druids' stair at the terrace's east end (150 to 154, z 300 to 310, y 4 to 10) is clear
  of roots. Reaching I from the north bank means the Long Cairn backward twice: slower, and a real choice for a player
  who likes the sapling year.

#### 3H. The grove terrace (x 60 to 160, z 310 to 328, y 10)
- *Shape:* a long terrace under the grove's hill, its face 6 m of roots and old stone toward the camera. Ferns, a moss
  carpet, flowers open in a row of hoofprints (`LOOKS.md`).
- *Enemies:* E3.6 one **Green Man** rooted beside the oak's crown at (80, 10, 324), the last before the door.
- *Rest:* **F6** at (104, 10, 318).
- *Down in any year:* two drops of 3 m facing south at the terrace's east end, (140, 10, 311) to a ledge (140, 7, 308)
  and on to the north bank (140, 4, 305) in the Feast Yard's corner (1.5).
- *Out:* root steps (96 to 108, z 328 to 332, y 10 to 12) up into the grove.

#### 3I. The sacred grove
See section 5.2.

### 3.4 The barrows: the barrow road and the Hollow Way (x -10 to 10, z 186 to 352)

**Decided**: the barrows lie under the hills and link the land; each trip under turns the year. The Hollow Hill and the
Long Cairn are Life's (3.3); the seats' barrows belong to their arenas (5.1, 5.2). The one that joins the land is here.

- **The barrow road** (the Fork Mound): a long gallery under the south end of the Hollow Hills, from its south mouth at
  the Fork (0, 3, 186) to its north mouth (0, 6, 248). 2.4 m wide, corbelled, rising 3 m by three short flights of steps.
  Going north (in at the Fork) ages the wood; going south turns it back. A round **kerbstone** stands across the gallery
  6 m inside the south mouth, shutting it: from the north side, "Roll the kerbstone" (S4). Until then the player sees it
  from the Fork as a dark round shape inside the mouth.
- **The owl's chamber** halfway, at (0, 4.5, 218): a cruciform chamber 7 m across with three recesses; in the west
  recess's niche **the owl** (section 8); spirals on the recess stones catching what little light comes down the gallery.
- **The Hollow Way**, from the north mouth to the plain: a sunken lane 6 m wide between banks 2.5 m, threading north
  104 m between the barrow mounds of the Hollow Hills (six round barrows, one a bell barrow with its ditch), from y 6 to
  the plain's south terrace at (0, 10, 352). Spirals on the mounds' kerbs; nothing else. The beats of the brawl (1.4)
  come every 45 s here and the tremors are the first strong ones. **No enemies**: it is walked south only going home, and
  a south walk with nothing in it costs only the view ahead (1.1, rule 4).

### 3.5 District 4: the Wild Hunt's plain (x -72 to 56, z 352 to 482)

**Decided**: a horn sounds and the Hunt rides across the plain with Carnwyn's herd and the dead; it cannot pass stone,
so the player runs from standing stone to standing stone; caught in the open the player is not killed but carried back
to the plain's edge, hurt; the verb is route-finding, one set piece of choosing which stones to run between; the stones
read from above by a gold-lit spiral and a ring of untouched heather that the wave parts round (`LOOKS.md`).

**The idea.** A moor of heather rising gently north to the rise, scattered with standing stones, with a long black mere
in its middle that forces two ways round. The Hunt is a wave that sweeps the whole plain from east to west on a horn's
clock, and the stones are islands in it. Planning happens on the great stones, where the camera eases back; running
happens between horns. **Levels:** the south terrace (y 10), the moor (y 10 to 14), the west lane behind the briar wall
(y 10 to 13).

```
 z     x: -72  -56   -40   -20     0     20    40   56
482   lane |wall| ================ the rise -> brow ===========
476   S5 slab-> |          LAST RING (0,14,476) r6 (all safe)          band C
466        |    |      C2(-14) o           o C3(8)                      every 8 s
456   pen3 gate |            COVE (-4,13,456) r4  V7
446        |    |    :  :  avenue end        o B4(20)    <- the Hunt
432   pen2 gate |    :  :  G2 gap  ~~~~~~~~~~~ o B3(10)       rides in   band B
424        |    |    :  :  G1 gap  ~~ MERE ~~    o B2(24)      from the  every 10 s
404        |    |    :  :  avenue  ~~~~~~~~~~~ o B1(12)       east
398        |    |              A3 (-4,11,398) r3  V6                          band A
382   pen1 gate |     A1(-14) o           o A2(12)                     every 12 s
368        |    |         HUNT STONE (0,10,368) r4  V5
356   Dance barrow (-62) ==== F7*(-4,10,356) === Hollow Way head (0,10,352) === grove barrow (62)
```

#### The quiet plain (before the second seal)
While either seal holds, the Hunt is shut in the meeting ground with the gods (section 2). The plain is empty but for
two of the herd grazing in band B, who lift their heads and go east when the player comes near, and the bodies of the
dead lying in the heather sprouting thorns where the Hunt last rode. The stones, the mere, the avenue and its gaps can
be learned in peace. The rise, the brow (H3) and the gate with one seal broken can be reached. Nothing here fights.

#### The release
When the second elite falls, the player sitting in that seat sees it (section 9): the second seal bursts, the gate stands
open, and the Hunt pours out of the meeting ground and round onto the plain. From then until the gods fall, the Hunt
rides. A player walking out of the seat's barrow onto the terrace hears the horn for the first time.

#### The wave
- **The horn's clock**, by band (the band the player is in): A every 12 s, B every 10 s, C every 8 s; the Hunt draws in
  toward its lord. The first wave after the release is placed, as Fire placed its first swell: its first horn sounds as
  the player first steps off the terrace onto the moor, with the Hunt Stone's ring 12 m ahead. Status line once: "The
  horn. Find the stones."
- **First horn**, 5 s before: one long note from the east. The light dims a step; mist streams in along the floor from the
  east edge of the frame toward the west, so the direction is drawn on the ground (`LOOKS.md`).
- **Second horn**, 1.5 s before: a short note and the hoofbeats; the east edge of the frame darkens with flung peat.
- **The front**: a line from the plain's south to its north edge, sweeping west at 16 m/s, 6 m deep: Carnwyn's stags
  with thicket antlers, and the grey dead riding among them, heather flattened and torn green growth whipped along under
  them. It crosses the plain's 112 m in 7 s and the frame in about 1.2 s.
- **Behind it**, for 3 s, the whipped growth lies across the moor (slowed 40%): the right moment to leave a stone is a
  breath after the wave, not the instant it passes.
- **Safe**: inside a stone's ring of untouched heather (rings in the table); inside the Broken Avenue between its rows,
  except at its gaps; inside the Last Ring; on the south terrace and in the west lane. The wave parts round them: its
  stags swerve, the dead lean away.
- **Enemies in the open** when it passes are swept up and carried off: they count as killed, Shards and all. Luring a
  straggler out of a ring as the horn sounds is a way to fight the plain with the plain.

#### The stones

| Stone | Where | Ring (r) | Band | Notes |
|---|---|---|---|---|
| A0 **the Hunt Stone** | (0, 10, 368) | 4 | A | A great stone; the first shelter, 12 m from the terrace; V5 |
| A1 | (-14, 10.5, 382) | 2.5 | A | |
| A2 | (12, 10.5, 386) | 2.5 | A | |
| A3 a pair | (-4, 11, 398) | 3 | A | The fork of routes; V6 |
| The Broken Avenue | Two rows 6 m apart from (-20, 11, 404) to (-34, 12.5, 446), a stone every 4 m | Between the rows | B | Its east row lacks stones at **G1** (z 414 to 424, 10 m) and **G2** (z 432 to 446, 14 m), taken by the haulers (lore, below) |
| B1 | (12, 11, 410) | 2 | B | The Scatter |
| B2 | (24, 11.5, 422) | 1.5 | B | Holds the player alone |
| B3 | (10, 12, 432) | 2 | B | |
| B4 | (20, 12.5, 446) | 2 | B | |
| The mere | x -12 to 6, z 404 to 444 | | B | Deep black water: the throw-back; no way through the middle |
| C1 **the Cove** | (-4, 13, 456) | 4 | C | Three great stones in a box (Avebury's Cove), where both routes meet; V7 |
| C2 | (-14, 13.5, 466) | 2 | C | |
| C3 | (8, 13.5, 464) | 2 | C | |
| **The Last Ring** | Centre (0, 14, 476), r 6, twelve stones | Its whole inside | C | The end of the Hunt's reach; the rise beyond |

Distances: terrace to A0 12 m; A0 to A1 or A2 about 19; A1 or A2 to A3 about 18; A3 to the avenue's mouth 18, the
avenue 42 m (its gaps 10 and 14); A3 to B1 20, then hops of about 17 to B4; the avenue's end or B4 to the Cove or C3
about 20; the Cove to the Last Ring 20. At 6 m/s a hop of 18 m is 3 s.

#### The two routes through band B (the choice)
- **West, the Broken Avenue.** A corridor of stones, safe for its whole length but for the two gaps on its east side.
  E4.1 two **mute reborn**, the Hunt's grey dead, stand in it at (-24, 11.5, 410) and (-30, 12, 438): silent fights in a
  safe lane, with the gaps to time and the horn coming whether the fight is done or not. A reborn followed into a gap as
  the horn sounds is taken by the wave; the player in the gap with it is too.
- **East, the Scatter.** Single stones 17 m apart with small rings: two hops per wave for a quick player, one for a
  careful one. E4.2 two **overgrown stags** grazing between B1 and B3, at (16, 11.5, 418) and (8, 12, 426). Their charge
  tangles; their shed antlers root as thorn patches that sit across the straight line between two stones and change the
  route for the rest of the crossing. **At each first horn every stag in the open turns and runs east to join the wave**
  (they are Carnwyn's herd), so a tangle never runs into a wave (1.3's rule for Ice, applied here: two things that
  cannot be answered never land together).
- **The middle** is the mere. The straight line north is the one that does not exist.

Neither route is the right one: the avenue asks for fighting in rhythm, the Scatter for reading hops and dodging stags.

#### Band C: the dead set down
Each wave in band C sets one of the grey dead down on the moor (at most two at once), at the Cove's west or C3's east.
They come at the player between horns. Fight in a great ring (the Cove, the Last Ring) and the wave does not touch the
fight; fight in the open and the next wave takes both.

#### Reading it at this camera
1. **The plain's framing** (1.1): about 11 m north and 19 m wide, held all the way across.
2. **Long shadows**: every stone beyond the frame throws its shadow down the screen into it (1.1); the far end of each
   dark bar is its stone. The player sees the next band's stones by their shadows before they see the stones.
3. **The gold spirals** on the stones catch the low sun and glint; the rings of untouched heather read as round pale
   islands in the flattened grey-green.
4. **Vista zones on the great stones** (V5 on the Hunt Stone, V6 on A3, V7 on the Cove): standing in the ring, the camera
   eases back to show about 20 m ahead, the next band's stones and its route. They open only between horns (never during
   the 5 s warning). Plan on the island, run between horns: the shape of Inside's shockwave field, where the explosion
   comes on a fixed rhythm and the level is built as cover to run between.
5. **The ear**: the horn from the east, the hoofbeats, the rhythm by band. A player who has crossed band A knows B's
   clock from its first two notes.

#### Caught
**Proposal** (question 2): the wave carries a player caught in the open **west along its line** and sets them down
behind the plain's west wall at the **same latitude**, 25 damage (never the last point of health), knocked down 1 s. The
Welsh Hunt set the people it swept up down far away; here, at the far edge, not back at the start.
- The west edge is a wall of field stones grown over with briar so thick that the Hunt cannot pass (x -56, z 360 to
  480), with three **pens** behind it (x -66 to -56), one per band, each with a gate back onto the plain at z 382, 424
  and 462. A set-down player walks out of their pen's gate onto the band they were in.
- The pens do not connect to each other or to the lane behind them (S5), so being caught is never a way round.
- The decided words "carried back to the plain's edge" are kept; whether the edge is the far side (this proposal) or the
  plain's start is question 2.

#### After a fall or a rest
The clock starts again: no wave comes sooner than 12 s after the player steps off the terrace. The stags graze again;
the grey dead stand again in the avenue.

#### Lore
- **L7**, on the Hunt Stone (Claude's draft): "He rides at dawn. It is always dawn."
- The avenue's gaps: the stones' sockets are fresh, with drag marks going south-west toward Earth's hill. The haulers
  have been taking the Hunt's stones to the giant's ring.

### 3.6 The last walk (z 480 to 512)

**Decided**: the brawl is seen on the walk to the meeting ground; from the top of the rise the meeting ground lies
ahead, half the far sky thick with ochre dust, the other half heaving canopy, the ground shuddering (`LOOKS.md`).

- **The rise**: from the Last Ring's north side (0, 14, 482), a processional way 8 m wide between two low banks (as the
  Stonehenge Avenue runs between banks), 30 m long up to the brow at (0, 22, 510): 15 degrees, turf over packed chalk,
  hoofprints with flowers in them (the Hunt used to ride it). Beats every 20 s; every tremor now shakes the camera, dust
  hangs over the banks, birds go over in a stream.
- **The brow** (0, 22, 510): **H3**, the third held shot (section 9). Then the ground beyond: a short walk down to the gate.
- **The salmon's pool** on a shelf east of the rise, reached by a path from (8, 18, 494): a spring pool 6 m across under
  nine hazels at (18, 18, 496) (the Well of Segais nod, where the salmon ate the hazelnuts of knowledge). **The oath stone**
  on a shelf west, reached from (-8, 18, 498): (-18, 18, 500). Both section 8.
- **The door**: walking north past the brow at (0, 22, 512) loads `MeetingGround` (`SceneDoor`), where the forecourt and
  F8 are. The rise and the brow stay in `StoneGrove`, so H3 is the land's last shot of the far set.

---

## 4. Shortcuts and loops

| | Opens from | To | How | Saves |
|---|---|---|---|---|
| S1 | The cutters' bank, the drain's north lip (-38, 2.5, 122) | F1, by the bank's south ramp (-12, 1, 8) | "Topple the stone": the marker stone falls across the drain and stays (Earth remembers) | F2 to F1 by the bank, without the Black Pool and the cuttings |
| S2 | The hollow way's head (-76, 20, 300) | F3, through the broken wall at its foot (-76, 12, 248) | Kill the haulers: the stone rolls back down the track and bursts the wall; the wall stays broken | F3 to the hilltop without the King's Men (the haulers come back after a rest, hauling up the track: 1.7) |
| S3 | The Alder Ravine's north bank (153, 4, 296) | The south bank, then the north lane to F5 | "Lever the slab": the clapper bridge's fallen middle slab back on its pier; stone, so it stays | F6's side to F5 in any year, without a barrow |
| S4 | The barrow road's kerbstone, from the north (0, 3, 192) | The Fork, F2 | "Roll the kerbstone" | The plain to the Fork, under the Hollow Hills: the way home between branches. It turns the year back as it is walked south |
| S5 | The rise's foot, beyond the Hunt's reach: a bank walk west from the Last Ring (z 482 to 488) to a slab in the briar wall's north end (-56, 14, 484) | F7, by the lane behind the pens (x -72 to -66) | "Push the slab", from the north | The north of the plain back to F7 without crossing the Hunt again |
| Drop | The grove terrace's east end (140, 10, 311) | The north bank (140, 4, 305) | Two drops of 3 m facing south, one way | The terrace left in any year |
| Seat barrows | The Dance; the grove | The plain's south terrace, 58 and 66 m from F7 | Open when each elite falls | The seats' way on |

S1 to S5 are kept after a fall and across sittings (a progress key each, as Fire's). The year is kept too (1.3). Life's
briar, hurdles and wicker are not: they grow back at every rest, which is why no shortcut on Life's side is made of them.

---

## 5. The elites' arenas

### 5.1 The Giant's Dance (centre (-100, 22, 340), the Bargain)

**Decided**: Brannoch's seat, a nod to Stonehenge, with its elite; the Bargain, a huge churl who sits on a fallen stone
with an axe across his knees, offers his neck for one free blow, picks up his head and sets it back on; partway through
he plants his axe and holds out his hand (take it: three blows with the flat, then a fair duel; roll away: "oath-breaker",
full fury, no second bargain); his heavy axe previews Brannoch's blade; he teaches The Return Blow (`WORLD.md`,
`steel/weapons.md` 14); from the Dance the forest surging at the meeting ground is seen (Decided). **Proposal**
(`LOOKS.md`): the camera sees the lintels' tops, with lichen, rook perches and tool marks; the empty seat a great stone
chair facing the meeting ground; older stones lying flat round the ring that from the hill make the profile of a sleeping
woman, the Cailleach whose stones the giant stood on end.

**Shape.** An outer ring of 30 uprights, radius 16 m, capped by a continuous ring of lintels at y 27 (5 m up), with gaps
3 m wide between uprights; inside it a **horseshoe** of five great trilithons (7 m) opening to the north-north-east, toward
the sun and the meeting ground, as Stonehenge's horseshoe opens to the midsummer sunrise. In the horseshoe's mouth, **the
empty seat**: a slab chair 3 m high at (-96, 22, 348) facing north-north-east. The Bargain's **fallen stone** lies on the
south-west of the floor at (-108, 22, 334), 5 m long, 1.2 m high. The floor: short turf over chalk, ochre cracks running
out from every stone, nothing else. Uprights and trilithons south of any point of the floor within 4 m fade above 1.4 m
(the south-side rule); their lintels fade with them.

```
 z    x: -116      -100      -84
356        o  o  o  o  o  o           outer ring r16, lintels y27
350     o    [  ] seat [  ]    o       horseshoe of five trilithons, open NNE
340    o   [  ]   floor    [  ] o      -> Dance barrow at the NE gap (-86,22,354)
334     o  [fallen stone]    o
326        o  o  o  o gap SE <- F4
```

**How it plays with him.**
- *The free blow:* he sits on the fallen stone; the player strikes; his head rolls toward the seat; he walks after it.
- *His axe plants stones* (the preview of Brannoch's sword: a heavy blow plants a short stone where it lands). Each heavy
  that misses leaves a stone 1.6 m high where its head struck. They stay for the fight and **after a fall** (stone
  remembers), seven at most, the oldest crumbling when an eighth is planted. They are cover from his sweep and walls to
  be thrown into (his barge shoves the player 3 m; into a stone, 20 more and a stagger), so the arena of a long fight, or
  a third attempt, is a different arena from the first.
- *The bargain:* he plants the axe and holds out his hand: a clear stance, a clear pause, no stone placed. The room adds
  nothing: the choice is between the player and him.
- *The lintels* are not part of the fight (`HoldGround(-116, -84, 324, 356)` keeps it on the floor).
- *No brawl beats* in the ring until he falls.

**Beaten.** The Return Blow and a Memory; the stone seal at the meeting ground's gate breaks far off (a crack rolls over
the hill and every rook on the lintels lifts at once). The seat can be sat in: H1 (section 9). Behind the horseshoe, the
**Dance barrow** opens at the ring's north-east gap (-86, 22, 354): its entrance stone shows a sunwise spiral; a passage
of 50 m goes down by four flights of steps to the plain's south terrace at (-62, 10, 356). **The eagle's lintel** is the
side path (section 8).

### 5.2 The sacred grove (centre (100, 12, 346), the Wicker Man)

**Decided**: Carnwyn's seat, with its elite; the Wicker Man, a giant wicker figure walking through the grove with living
people inside its chest and limbs; its health is the people (each part broken frees one, every freed captive weakens it,
free them all and it collapses); life regrows it (damaged wicker weaves back; a bundle cut open weaves shut if the player
goes to free someone else); a stamp that sets the cages hung from its arms swinging; a grab into an empty cage the player
breaks out of; it sheds ordinary wicker as adds; burning it is not a choice (green living willow does not catch); it
teaches Over the Shoulder; from the grove the stone side is seen breaking the ground at the meeting ground. **Proposal**
(`WORLD.md`, `LOOKS.md`): freed captives light the grove's lamps; the great oak with mistletoe; the empty seat grown from
the roots with a crown of antler-like branches; wicker cages hung in the grove.

**Shape.** A round clearing of radius 18 on the hill above the terrace, the floor old paving broken up by roots. At its
centre **the great oak** (100, 12, 346), its trunk 5 m across: the pillar of the fight. Its crown fades over the floor
(it is a canopy, not a roof), its lower boughs hold **six hung cages** on chains, swinging 1.5 m above the floor, four
empty and two holding the grove's own captives. **The seat of roots** on the oak's north side (99, 12, 351), facing
north-north-west. **Five lamps** on stone posts round the edge, dark. The roots: three great roots run out from the trunk
across the floor as low ridges 0.5 m high (crossed at a step's cost: no roll over them, as Ice's fallen spruce). On the
east edge **the reapers' stone** (116, 12, 342): a slab carved with a field where every stalk has a face (the hint for
Harvest Home).

**How it plays with it.**
- *The oak is what it walks round.* The Wicker Man is 9 m tall and slow; it circles the trunk after the player. The trunk
  blocks its reach and its grab, so the fight is a chase round a pillar with the roots as hurdles.
- *The cages swing.* Its stamp sets every hung cage swinging: arcs 4 m long across the floor under each bough, drawn as
  pale arcs on the paving 0.6 s before each pass (they strike 18 and knock down). Under the oak's north side the cages are
  densest: the seat is the most dangerous place in the grove.
- *The grab into a cage.* Caught, the player is lifted into one of its empty arm cages and must break out (mash, 2 s);
  inside, the cage is 2.5 m above the floor and every blow from inside also breaks the bundle round the cage, freeing its
  neighbour's captive. Being grabbed is a cost and an opening.
- *The captives* are in its chest and four limbs; each bundle cut open frees one, who runs to the nearest dark lamp and
  lights it. **Proposal**: a lit lamp's warmth slows the wicker's regrowth within 6 m of it to half. Freeing in an order
  that lights the lamps round where the fight is makes the rest of it easier; the grove grows warmer as it is won
  (`LOOKS.md`).
- *Adds:* it sheds a wicker from its body at each half of its captives freed (two in all), never more than one on the
  floor.
- *The year:* the grove's floor is the same in every year. Only the ways to it change.
- `HoldGround(82, 118, 328, 364)`.

**Beaten.** Over the Shoulder and a Memory; the thorn seal breaks far off. In the cut field at the reapers' stone the
Scythe Walks Alone stands upright (`steel/depth_weapons.md`). The seat can be sat in: H2. The **grove barrow** opens at
the clearing's north-west (86, 12, 360): a sunwise spiral; a passage of 30 m and a short gallery west, level, to the
plain's south terrace at (62, 10, 356).

**The second seat.** Whichever elite falls second, its held shot shows the second seal bursting and the Hunt pouring out
(section 9); that elite's last line acknowledges the other seal (Claude's drafts, to be set in the voice): the Bargain,
"The thorn is broken. Then mine is due."; the Wicker Man has no words, and its freed captives turn their heads north.

---

## 6. The meeting ground: the gate barrow, the bowl and the solstice passage (`MeetingGround`)

The fight's design belongs to `earth_life_land.md`; this section gives its room, its approach and the passage, as
`ice_levels.md` section 6 does for the lake. `MeetingGround` keeps the land's coordinates, so the far set seen from the
seats and the room itself agree.

**The forecourt (the god's door).** Past the brow the way bends west to the forecourt of **the gate barrow**, a great
round mound (centre (-2, 22, 528), radius 14) on the meeting ground's south rim, kerbed with carved stones. **F8** at
(-14, 22, 516) in the forecourt (Decided: a fire at the god's door). The gate itself is a door in the kerb on the mound's
west-south-west side, (-12, 21, 522), and across it the **two seals** (Proposal in `WORLD.md`): a **stone** slab that
stood across the door, cracked and fallen inward when the Bargain fell, and a **thorn** of blackthorn grown across it,
withered and torn when the Wicker Man fell. Seen with one whole, the player knows which branch is left. Here the brawl
is no longer beats: a continuous roar and grind through the mound, dust sifting off the kerb.

**The gate barrow.** From the door a passage 8 m east into a **cruciform chamber** at (-3, 20, 524), 7 m across, with
three recesses. In the **south recess**, a carved **triple spiral** (Newgrange's), dark the first time, passed in the
gloom. From the chamber the **long passage** runs **19 m** (Newgrange's length) north-north-east, bearing 15 degrees,
the sun's own bearing, to its mouth on the bowl's south rim at (2, 20, 542). Walked north, toward the noise and a line of
gold light at its end. The mound is not drawn while the player is inside (1.1).

**The bowl.** Centre (6, 18, 572), radius 28 (`LOOKS.md`: a broad shallow bowl, its floor carved with a great double
spiral). Three steps down from the passage mouth to the floor at y 18.
- **The double spiral** (the motif proposed in `WORLD.md`) is carved across the whole floor, its two arms wound round two
  eyes at (-6, 18, 572) and (18, 18, 572), and **the seam between the arms runs from the passage mouth north-north-east
  across the bowl**, on the sun's line. The west arm is Earth's (heaved stone, bare furrows), the east arm Life's
  (tangled growth), and the line between them shifts as the gods fight (`LOOKS.md`).
- **The rim** is a henge bank: on the south, the bowl's floor ends in a kerb 1.2 m high and the bank climbs at 30 degrees
  to a crest 6 m behind it, so nothing over 1.4 m stands within 4 m south of the floor; east, west and north the bank's
  inner faces are 3 m, its crest 9 m above the floor. Beyond the north crest, the sun on the horizon.
- Room for the proposals in `WORLD.md`: a **bullaun stone** (an Irish basin stone that holds water) on the west arm at
  (-10, 18, 574) as Brannoch's healing stone, which the player can break first; a stone carrying a silver plate on the
  east arm at (22, 18, 572), from which Carnwyn calls his animals. Nothing else fixed: the gods plant and grow the rest.
- `HoldGround(-22, 34, 544, 600)`. Phase 2's camera pull-back (Proposal, `WORLD.md`) is the bowl's own framing.

**The solstice passage** (Decided: the ride out; Proposal in `WORLD.md`: the first ray down the passage of a barrow at the
meeting ground's edge, as at Newgrange). When both choices have been made, the sun clears the horizon for the first time.
Its first ray runs down the bowl along the spiral's seam, into the passage mouth, 19 m down the passage, across the
chamber and onto the triple spiral in the south recess, which takes the light and glows gold. The mist burns off; every
shadow in the bowl shortens; the land lifts a step in colour (`LOOKS.md`). The prompt: "Walk into the light". The player
walks back down the passage, south toward the spiral; the camera holds from inside the chamber, looking north up the
passage at the player coming down it with the sun behind them, and at the spiral the light fills the frame. The way in
becomes the way out, and the dark carving passed on the way to the fight is where the land ends. It answers Fire's land:
there the sun set for the first time in ages, here it rises.

---

## 7. The optional trial

**Decided**: one optional trial per land, invented for it, guarding a Memory or a Body piece; the owner gave no steer for
this land. Three options (question 5), the first proposed.

### 7.1 The Whispering Knights (proposed)

**The nod.** At Rollright the Whispering Knights stand apart from the King's Men: four leaning stones of a burial
chamber, said to be knights who drew aside to plot against their king and were turned to stone mid-whisper.

**Where.** A round platform 20 m across on the east shoulder of the King's Men's hill, (-56, 16, 276), reached through the
hollow way's east bank gap at (-72, 15, 276). The **portal dolmen** at its north side, (-56, 16, 284): four uprights
leaning together, the capstone slid off onto the platform. The south edge a kerb 1.2 m. It stands between the two sides,
looking east over the Hollow Hills toward the wildwood.

**The two knights.** A duo, the land's own: it is a small, fair preview of the gods' fight, where each undoes the other.
- **The Grey Knight**, captain of the King's Men: taller, a great stone sword and a stone shield; he moves only while
  unwatched (1.7), and his frozen blade completes when the player turns away.
- **The Green Knight**, a champion of the Green Men, with a holly club (the Green Knight of the Gawain poem rode in with a
  holly bough): he roots and heals twice, his root lane is long (9 m), and a limb cut from him grows back after 10 s.
- **They whisper.** Only one attacks at a time (the same rule the owner set for the gods: they take turns); while the
  player faces the Grey Knight, the Green Knight attacks, and while the player faces the Green Knight, the Grey advances.
  Two red attacks never land within 1.2 s of each other.
- **Each undoes the other.** The Grey Knight's blade completing on the Green Knight cuts all his roots at once and
  staggers him (stone cuts wood). The Green Knight's root lane, dodged so that it runs under the Grey Knight, binds his
  feet for 3 s (wood holds stone): the one time the player may turn their back on him safely. Bait each into the other,
  or stand so that both are inside one fan and fight the Green Knight with the Grey frozen behind him.
- **The prize:** a Body piece (or a Memory) on the fallen capstone. Beaten, the knights stay down for good.
- *Minutes:* 5.

### 7.2 The Turning Barrow

A round chamber 16 m across off the owl's chamber's east recess, behind a door that opens once both seats are empty. Inside,
its own year turns every 25 s (I, II, III, IV, and round), whatever the land's year: saplings (many small Green Men),
thicket (briar walls cutting the floor into lanes), high wood (root bridges over a pit that opens in the floor), windfall
(trunks falling across the floor on marked lines). Waves of Green Men, wicker and a stag. It is the land's own verb made a
fight, as Dishonored 2's *A Crack in the Slab* made its two times one place to fight in. Weaker because it turns the years
from a slow puzzle into a fast hazard, and every year's arena must be tuned against every wave.

### 7.3 Count the Stones

On the cairn field, a ring of twelve cairns, and on the King Stone a number of strokes. Countless rise from the ring; each
broken without its ochre stone stands again and wakes another. The player must strike down the ring until the number left
standing matches the King Stone's strokes, three times running (the Rollright legend: whoever counts the stones three
times and finds the same number wins their heart's desire). Very much the land's; a counting puzzle under pressure reads
poorly from above when twelve cairns look alike, and it repeats the countless's one mechanic.

---

## 8. Side paths: see it, then find the way, and the Oldest Animals

**Decided**: "see it, then find the way" is the main kind of discovery, holding the plain weapons and lore; the Oldest
Animals side path gives the land's plain weapons and a way to make the god fight easier. **Proposal** (`WORLD.md`): the
blackbird in the bog starts it; the stag in the wildwood gives the plain scythe; the owl is in the barrows; the eagle on
the Giant's Dance hill gives the plain greatsword (a weathered slab from a fallen stone); the salmon in a pool beside the
meeting ground remembers the gods as friends and leads the player to the oath stone, where the ivy ring is taken.

In *Culhwch and Olwen* each animal says it is not old and sends the seeker to an older one, and each gives a sign of its
age. The lines below keep that shape (Claude's drafts, to be set in the voice of `GAME.md`); three of the signs are the
land's own years.

| Animal | Where | Seen | Found | Says (draft) | Gives |
|---|---|---|---|---|---|
| The blackbird | The sunk post by F1 | On waking | Walk to it | "I have worn a smith's anvil to a nut with my beak. I am not old. Ask the stag." | The thread |
| The stag | The stag's hollow, the ravine floor (78, 0, 290) | From the root bridge in III (V4) | In IV, down the fallen alder | "I saw that oak an acorn. I saw it stand a hundred years. I saw it fall. I am not old. Ask the owl." | **The plain Scythe** |
| The owl | The owl's chamber, the barrow road (0, 4.5, 218) | Its eyes in the west recess, passing | Step into the recess | "This hill was a wood three times, and three times cut. I am not old. Ask the eagle." | The way on |
| The eagle | The south lintel of the Dance (-100, 27, 324) | From inside the ring, near its south side (a lintel 5 m up is in frame from 2 to 6 m north of it) | After the Bargain: a fallen upright leans from the ring's floor at (-115, 22, 342) up to the lintels (15 degrees); walk the lintel ring (1.2 m wide) round to the south | "I pecked the stars from the top of a stone. Now it is a hand high. I am not old. Ask the salmon." | **The plain Massive Sword** |
| The salmon | The pool under nine hazels (18, 18, 496) | From the rise | The shelf path from (8, 18, 494) | "I remember them. Two, at the stone above the water. Follow." | Leads to the oath stone |

- **The plain weapons** (question 7). The plain Scythe in this land's make: an iron blade on an ash snath bound with
  bronze, hung in the hawthorn over the stag. The plain Massive Sword: a slab of a fallen stone knapped to an edge, its
  grip bound in hide, lying on the eagle's lintel. Each plays exactly as the plain weapon; the look changes, as the owner
  chose for the Chain Sickle, the Great Bow and the Twin Knives. Carnwyn's golden scythe and Brannoch's standing-stone
  sword stay the gods'.
- **The stag's hollow** (x 70 to 92, z 286 to 294, y 0): ferns and a spring at the ravine's west end; two Green Men in the
  ferns. Reached only in IV (the alder's trunk) and left the same way; a player can only be there in IV (1.5). 3 minutes.
- **The eagle's lintel walk**: rooks lift as the player passes; a fall from the lintels lands 5 m below inside or outside
  the ring for 10 damage, never out of the world. 2 minutes.
- **The oath stone** (-18, 18, 500): a holed stone 2 m high (the Odin Stone nod), a thin stone grown through its hole one
  way and an oak sapling the other. The salmon swims down a rill from its pool, under the rise in a stone channel, and
  surfaces in a basin at the oath stone's foot; only then does the ivy grow through the stone's hole as the player
  watches, a ring of it, which they take: "Take the ring". Carried, not used (Proposal in `WORLD.md`): in the fight,
  whenever one beast hurts the other, the one who did it freezes a few seconds in half-remembered remorse; at the end the
  two dying men clasp hands, as here. Without the thread, the stone is only a holed stone. 2 minutes with the salmon.
- **The crannog** (36, 1.5, 78), seen from the hurdle way in the first minutes; in III or IV the alder carr's roots reach it
  from (25, 0.4, 82). A ring of posts, a hearth, a wicker and a Green Man; on the hearthstone **L2**, a carving of two
  hands clasped round a stone and a sapling (the oath, before the player knows it), and a gold torc's worth of Shards.
  2 minutes.
- **The druids' stair and the well** (Year I): the stair at the terrace's east end, clear only in I, has a shelf halfway
  with **L8**, an oak carved with a cut bough and a sickle (Pliny's golden sickle and the mistletoe); the green's old well
  is open only in I, Shards on its floor 3 m down by a rope (`Ladder`). 2 minutes together.

---

## 9. Vista zones and held shots

**Vista zones** (`LandZones`, `TopDownCamera.SetFraming`; Fire's B13): about 20 m ahead while the player stands in them,
never while an enemy has noticed the player or, on the plain, during a horn's warning.

| | Where | Size | Shows |
|---|---|---|---|
| V1 | The Fork Stone (0, 3, 168) | 8 by 4 m | The causeway going west, the carr going east |
| V2 | Field 4's top (-110, 12, 246) | 8 by 3 | The King's Men's first ranks and their shadows |
| V3 | The Hollow Hill's east mouth (131, 4, 268), first exit; the same short framing on the first exit of each barrow | 4 by 3 | The year that passed |
| V4 | The root bridge's middle (124, 4, 290), in III | 3 by 3 | West down the ravine to the stag |
| V5 | The Hunt Stone's ring (0, 10, 368) | Its ring | Band A's stones |
| V6 | A3's ring (-4, 11, 398) | Its ring | Both routes through band B, the mere between |
| V7 | The Cove's ring (-4, 13, 456) | Its ring | Band C and the Last Ring |

**Held shots** (question 6). The owner chose three held shots for the Water land, one per district. The brawl alone needs
three here (Decided: seen from the two seats and on the last walk), so this plan proposes those three and the arrival.
Each is skippable, plays only with no enemy near, and can be played again.

| | Trigger | Camera | Shows |
|---|---|---|---|
| H0 | Waking for the first time at (0, 0.5, 0) | Starts 40 m behind and 20 m above, settles to the game camera over 6 s | The land in one image (`LOOKS.md` `el_plank_road`): the stone circle on its hill left, the great oak right, the rise hiding the meeting ground between, nothing of the brawl |
| H1 | "Sit in the seat" at the Dance's empty seat, after the Bargain | Rises behind the seated player to 30 m and turns north-north-east over the lintels, 8 s | The plain below, its stones' long shadows reaching toward the camera, the mere, the Last Ring, the rise; beyond, at the meeting ground, **the forest surging** under a plume of ochre dust, trees bursting up and toppling; round the ring, the flat stones that make the sleeping woman's profile. After the first seal: a crack runs across the gate mound and dust rings out from it. After the second: the gate stands open and the Hunt pours out and round onto the plain |
| H2 | "Sit in the seat" at the grove's seat of roots, after the Wicker Man | Rises through the fading crown to 30 m and turns north-north-west, 8 s | The plain; at the meeting ground **the stone side breaking the ground**, ochre heaving, standing stones bursting up through the canopy one after another; the same seal or the Hunt's release |
| H3 | Crossing the brow (0, 22, 510) going north, once per state of the seals | Lifts 20 m and pushes north, 8 s | The gate mound ahead with the sun on the horizon behind it; the henge bank; beyond it the brawl whole: half the far sky ochre dust, half heaving canopy, and for one beat the two shapes in it, the giant and the antlered lord |

The far set (1.1, rule 3) is built once and lit for these shots only.

---

## 10. Build list

New pieces are made in code by a `StoneGroveBuilder` (menu `Clover/Build/Stone Grove`) and a `MeetingGroundBuilder`,
as the Fire and Water builders are: static meshes with `MeshCollider`s, ink outlines where the toon material carries one.
Each piece names its parameters; sizes are the defaults the plan uses.

### 10.1 New geometry

| | Piece | What it is | Dimensions and parameters |
|---|---|---|---|
| G1 | **Plank road** | Oak planks laid across two runners on pegs over black water; sunk stretches with the planks 0.3 m under water (wading); a peat island joined by a plank | 3.2 m wide, y 0.5; sunk z 26 to 32; Corlea's planks 3 to 3.5 m long |
| G2 | **Bog** | Black pools (deep: throw-back), peat islands, bog cotton, cut faces of peat (1.25 m, layered), turf stacks (1.2 m) | Section 3.1 |
| G3 | **Stone causeway** | Slabs on a bank with body shapes worn into them; a cracked slab that breaks for good | 4 to 5 m wide |
| G4 | **Hurdle way and briar** | Woven hazel hurdles; briar as a **Grown** thing (cut: three blows a metre; grows back at every rest) | 3 m; briar patches 4 to 6 m |
| G5 | **Barrow** | A mound with a kerb ring, mouths with flat **entrance stones** (spiral sunwise or widdershins, four cups), corbelled passages, chambers; the mound's shell fades as the player enters and is never drawn above a walked passage; mound tops never walkable | `Barrow(kind, mouths, length)`: *horseshoe* (the Hollow Hill, 36 m), *long* (the Long Cairn, 30 m), *gallery* (the barrow road, 62 m, three flights, the owl's chamber), *descending* (the Dance barrow, 50 m, four flights; the grove barrow, 30 m), *cruciform* (the gate barrow: door passage 8 m, chamber 7 m, long passage 19 m) |
| G6 | **Lynchet fields** | Field terraces with south faces 2 to 2.5 m, cart-way ramps, dry-stone walls 1.2 m in breakable 4 m sections (progress key each) | Four fields, section 3.2 |
| G7 | **Hollow way** | A track sunk between banks 2.5 m, ruts and rollers, a wall across its foot that the rolling stone breaks for good | 8 m by 58 m, 8 degrees |
| G8 | **Standing stones** | King's Men (soldier-shaped stones, the men told by a sword's pale line), plain ranked stones, the King Stone, the Fork Stone, the Hunt's stones with gold spirals and **rings of untouched heather**, the Broken Avenue, the Cove, the Last Ring | Section 3.5's table |
| G9 | **The Giant's Dance** | Outer ring of 30 uprights with a continuous lintel ring (walkable, 1.2 m, 5 m up), the horseshoe of five trilithons opening NNE, the seat, the fallen stone, a fallen upright as a ramp to the lintels, flat stones round the ring in a woman's profile | Section 5.1 |
| G10 | **Roundhouse with a tree** | A roofless round wall (1.4 m south, 2 m elsewhere) with an oak, ash or hawthorn grown through it | Six on the green |
| G11 | **Year-variant living things** | One object with four forms for I to IV: the year hazel, the great alder (root bridge in III; fallen along the ravine in IV), the windfall oak (fallen ramp in IV), the druids' stair's roots, the crannog roots, the well's hazel, lane briar | Section 1.3's table |
| G12 | **Ravine** | An east-west cleft, sheer north wall, a 40-degree fern slope on the south, a stream floor; the clapper bridge with a slab that is levered back | x 70 to 160, z 284 to 296 |
| G13 | **The Feast Yard and the cauldron** | A roofless hall; a sunk bronze bowl 6 m across, rim level on the south, a root ramp in, three mended seams breakable from inside only, water that runs out | Section 3.3 |
| G14 | **Terrace face** | 6 m of roots over old stone facing south; a ledge for the two drops | z 310 |
| G15 | **The sacred grove** | The great oak (trunk 5 m, crown fading), three root ridges 0.5 m, six cages on chains (swing arcs), five lamps, the seat of roots, the reapers' stone | Section 5.2 |
| G16 | **The plain** | Heather moor (flattened grey-green, untouched rings pale), the mere, the briar field wall, the three pens and gates, the lane behind them with the slab, the south terrace | Section 3.5 |
| G17 | **The rise and the bowl** | The banked processional way; the salmon's pool with nine hazels and its rill; the oath stone; the gate barrow and its seals; the henge bank; the bowl with the double spiral, its seam on the sun's line; the bullaun stone; the plate stone | Sections 3.6 and 6 |
| G18 | **The far set** | The meeting ground and the gods' brawl in low detail 200 to 300 m off, plumes, a heaving canopy, stones bursting; its own layer, lit and drawn only in H1 to H3 | Section 9 |
| G19 | **The Whispering Knights' platform** | A round platform 20 m, a portal dolmen with its capstone fallen | Section 7.1 |
| G20 | **The crannog** | A timber island with a ring of posts and a hearth | (36, 1.5, 78) |

### 10.2 New behaviour the rooms rely on (for the code worker)

| | What | Where it is used |
|---|---|---|
| C1 | **Year clock**: the land's year I to IV, saved (`earthlife.year`), held through falls; broadcasts to every year-variant | Life's side, the bog's carr |
| C2 | **Barrow passage**: two mouths and a direction; changes the year on coming out (sleeps at the ends); lights the entrance stones' cups (now gold, next green); status line; first-exit framing | Every barrow |
| C3 | **Remember and forget**: breakables tagged *Stone* keep their broken state across rests (progress keys); things tagged *Grown* (briar, hurdles, wicker, thorn patches) reset at every rest | Everywhere |
| C4 | **The dead take root**: on Earth's side a killed enemy becomes a flat stone shape (kept, eight per room); on Life's side a thorn mound (until rest); respawns climb out of their own stone or mound | Everywhere |
| C5 | **King's Men**: the facing test (110 degrees, 15 m), freeze and move, the frozen blade, the floor fan | The hill; the Grey Knight |
| C6 | **Countless**: the scatter, the ochre stone among the scatter, the rebuild in 3 s, death by the ochre stone | Bog, causeway, cairn field |
| C7 | **Boar's litter**: the furrow charge, furrows kept (Earth), double speed along old furrows, wall stun and break; Grugyn's lead | The boar fields |
| C8 | **Haulers and the stone**: the gang on two ropes, the lasso, the creep, the roll down a lane, the wall burst, the respawn that hauls again | The hollow way |
| C9 | **Green Men**: rooting, healing, three cuttable roots, the root lane | Bog, Life's side, the plain's quiet bodies, the trial |
| C10 | **Wicker**: the cage, the captive, the strands' regrowth ring, the freed captive lighting a lamp | Life's side, the Wicker Man's adds |
| C11 | **Mute reborn**: no sound at all; the cauldron's 8 s return; the cauldron's three inner seams | Feast Yard; the plain |
| C12 | **Overgrown herd**: the tangling charge, shed antlers rooting a thorn patch; on the plain, running to the wave at the first horn | Lanes; plain |
| C13 | **The Hunt**: release on the second seal; the horn clock by band; the mist and dimming warning; the sweeping front; stone rings and the avenue's shelter; sweeping enemies away (counted as killed); carry and set down; damage that cannot kill; the clock's reset | The plain |
| C14 | **Brawl beats**: the clock by area, the four cues, the stereo bearing, quiet in arenas and during red attacks | `StoneGrove` |
| C15 | **Held shots and the far set**: "Sit in the seat"; the brow trigger; the arrival | Section 9 |
| C16 | **Seals and seat barrows**: a progress key per elite; the seals' states at the gate; the seat barrows opening | Sections 5 and 6 |
| C17 | **The Bargain's stones**: heavy misses plant stones, kept across a fall, seven at most | 5.1 |
| C18 | **The Wicker Man**: captives as health, regrowth slowed in lamplight, the swinging cages, the grab into a cage | 5.2 |
| C19 | **The Whispering Knights**: the shared turn, each undoing the other | 7.1 |
| C20 | **The oath stone**: the salmon's swim, the ivy ring grown and taken; the remorse freeze in the gods' fight | 8; the fight |
| C21 | **The solstice**: the ray along the seam and down the passage, the spiral lit, the walk into the light | 6 |

### 10.3 What is reused

| Piece or code | Where |
|---|---|
| `FallCatch` and the throw-back (`water_levels.md` 1.2) | Bog pools, the mere, the ravine's deep pools, under the whole scene |
| Water's wading | The plank road's sunk stretches |
| `Ladder` (Fire's B6) | The well's rope |
| `LandZones`, `TopDownCamera.SetFraming` | V1 to V7; the plain's held framing |
| `Fadeable`, `OcclusionFader` | Mound shells, trilithons and lintels south of the floor, canopies, roundhouse walls |
| `RestPoint` | F1 to F8, in stone rings (Earth) and root rings (Life) |
| `WeaponPickup` | The plain Scythe, the plain Massive Sword |
| `LandVoice` progress keys | S1 to S5, the seals, the cauldron, the walls, the trial's prize, the year |
| `Enemy.MakeDormant`, `Enemy.HoldGround` | Rooted Green Men, the Bargain on his stone; the Dance, the grove, the bowl |
| `DistrictMood` | The light by area (`LOOKS.md` table): flattest in the bog, raking on Earth's side, shafts on Life's, dim in the barrows, open on the plain |
| `SceneDoor` | The brow to `MeetingGround` |
| `ShieldWall`'s lock | Not used: the haulers' ropes are their own |
| Fire's `Plateau`, `Bank`, `Ramp`, `Stairs` | Every lynchet, bank, ramp and stair |

### 10.4 New kit (Blender, through the `blender-prop` skill)

Oak planks and runners; peat faces and turf stacks; hurdles and briar; barrow kerbstones with spirals and cup marks,
entrance stones, corbelled passage pieces, chambers; standing stones (plain, soldier-shaped, spiralled, holed), trilithons
and lintels, the slab seat; cairns; dry-stone wall sections and their rubble; rollers and ropes; roundhouse walls; hazel in
four years, the great alder, the windfall oak, the great oak of the grove and its crown; the bronze cauldron; wicker cages
and the Wicker Man; lamps on posts; the clapper bridge; the dolmen; the crannog; the bullaun stone; the far set. None of the
earlier lands' kit carries over except as grey-box stand-ins.

---

## 11. Pacing

Minutes for a new player on a clean pass, counted as `fire_land.md` 3.1 counts them (a weak enemy about 30 s, a strong one
45 s, a group a little less each), with each barrow walked and each wave waited out counted in. The order below is Earth
first; Life first gives the same total.

| Stretch | Minutes (new) | Experienced | Tension | What it teaches |
|---|---|---|---|---|
| The landing, F1, the arrival shot | 1.0 | 0.5 | Rest | The land in one image |
| The plank road | 2.0 | 0.9 | Rising | The countless's ochre stone; the Green Man's roots |
| The Black Pool, either way | 2.0 | 0.9 | Rising | Stone stays broken; briar grows back; the crannog seen |
| The cuttings | 1.5 | 0.7 | Peak (small) | Both sides together; the two deaths |
| The Fork, F2 | 0.5 | 0.3 | Rest | Three ways |
| **District 1** | **7.0** | **3.3** | | |
| The causeway west | 1.5 | 0.6 | Rising | Which cairn stands up |
| The boar fields, F3 | 3.5 | 1.5 | Peak | Furrows remember; a wall as a stun that costs |
| The King's Men's hill | 3.0 | 1.3 | Peak | Face what you fear; the frozen blade; a back to stone |
| The haulers' head | 1.5 | 0.6 | Peak (small) | Kill the hauling; the stone rolls (S2) |
| The cairn field, F4 | 1.5 | 0.6 | Rising | The ochre stone under pressure |
| **The Bargain** | 4.0 | 2.0 | Peak | The bargain; stones that stay; The Return Blow |
| The seat (H1), the Dance barrow | 1.0 | 0.6 | Vista | The brawl; the first seal |
| **District 2** | **16.0** | **7.2** | | |
| F7, the quiet plain, the barrow road home (the owl), the Fork | 2.0 | 1.0 | Quiet, dread | The plain in peace; S4; the year turned back |
| The carr and the Hazel Gate | 2.0 | 0.8 | Rising | Cut paths |
| The wildwood lanes | 2.0 | 0.9 | Rising | The wicker and its captive; the tangle |
| The green, F5 | 2.5 | 1.1 | Peak | Roots while cages come on; wood forgets (the pen) |
| The Hollow Hill | 1.0 | 0.4 | Wonder | **The years**; which way is younger |
| The north lane, the root bridge | 1.2 | 0.5 | Rising | A year opens a way; the stag seen |
| The Feast Yard | 3.0 | 1.3 | Peak | Silent foes; break it from inside |
| The Long Cairn, the windfall, F6 | 1.5 | 0.6 | Rising | A second year for a second door |
| **The Wicker Man** | 4.5 | 2.2 | Peak | Free, do not kill; regrowth; the cages |
| The seat (H2), the grove barrow, F7 | 1.0 | 0.6 | Vista | The brawl; the Hunt released |
| **District 3** | **18.7** | **8.4** | | |
| Band A | 1.2 | 0.5 | Dread | The horn; the ring; the first wave placed |
| Band B | 2.5 | 1.0 | Peak | **Choosing a route** |
| Band C, the Last Ring | 1.3 | 0.6 | Peak | The dead set down; the end of the reach |
| **District 4** | **5.0** | **2.1** | | |
| The rise, H3, the gate, F8 | 1.3 | 0.7 | Dread, vista | The brawl seen |
| The two gods (both phases, the solstice) | 14 | 9 | Climax | (`earth_life_land.md`, later) |
| **Clean pass** | **64.0** | **31.7** | | |
| Side paths: the trial 5, the stag 3, the eagle 2, the salmon and the oath stone 2, the crannog 2, the druids' stair and the well 2, the quiet plain to the gate 2, the owl 0.5 | + 18.5 | + 9 | | |

That sits inside `WORLD.md`'s 55 to 70 for a duo land, a little past the middle, and at 1.6 times Fire's 41. Levers if play
runs long, in order: the second boar in field 2 (0.4), the cuttings' second Green Man (0.4), the lanes' stag (0.6), one
reborn in the Feast Yard (0.5); with all four, 62.1.

**Fires apart (new player's minutes).** F1 to F2 about 7; F2 to F3 5; F3 to F4 6; F4 to F7 5 (the Bargain and the seat
between); F7 home to F2 2 and on to F5 6.5; F5 to F6 6.7; F6 to F7 5.5 (the Wicker Man between); F7 to F8 6.3. All within or
near the decided 6 to 8; the two short ones each hold an elite.

**Enemy count.**

| District | Main path | Count |
|---|---|---|
| D1 | 3 countless, 5 Green Men | 8 |
| D2 | 3 countless, 4 boars (Grugyn among them), 6 King's Men, 4 haulers | 17 |
| D3 | 5 Green Men, 3 wicker, 1 stag, 3 mute reborn | 12 |
| D4 | 2 mute reborn in the avenue, 2 stags, up to 2 dead set down | 6 |
| Elites | The Bargain; the Wicker Man | 2 |
| Side paths | The crannog 1 wicker, 1 Green Man; the stag's hollow 2 Green Men; the trial's two knights | 6 |
| **All** | | **43 on the main path, two elites, and the two gods** (Fire 40, Water 33, Ice 34) |

Fewer per minute than Fire's because the barrows, the years and the Hunt carry the land's time, as the tide and the
huntress carried theirs.

---

## 12. Questions for the owner

Each has two or three options, the recommended one first. The rest of the plan stays Proposal (Claude) until the owner
walks it.

1. **What is the plain between the two seals?**
   - **(a) Open and quiet after the first seal: the Hunt is shut in the meeting ground with the gods and rides out when
     the second seal breaks, seen from that seat.** Recommended. The goal is shown early and earned late, the shape the
     owner chose for Metal's sealed heart; Breath of the Wild keeps its castle in view and lets its Divine Beasts come in
     any order. A player may learn the stones in peace, and the Hunt stays one set piece, met once.
   - (b) Closed until both seals: the first seat's barrow leads home to the Fork, and the plain is first seen with the
     Hunt in it. A purer surprise; the first elite's barrow then goes nowhere new.
   - (c) The Hunt rides from the start, and the plain is the way between the branches. More of the Hunt; it is then met
     two or three times, against "one set piece".
2. **Where does the Hunt set down a player it catches?**
   - **(a) Carried west along its line and set down behind the plain's west wall at the same latitude: 25 damage, never
     the last point of health, a gate back onto the same band.** Recommended. In the Welsh lore people swept up by the
     Hunt were set down far away; Inside's shockwave field puts a fallen player back at the right point of its rhythm so
     the rhythm is never broken. The cost is health and position, not a repeat of the whole plain.
   - (b) Carried back to the plain's south edge, at F7. The literal reading of "back to the plain's edge"; a mistake in
     band C costs about a minute and the plain becomes a retry loop.
   - (c) Set down at the last great ring the player sheltered in. Gentle; it loses the image of being swept away.
3. **How many years, and do they wrap?**
   - **(a) Four (sapling, thicket, high wood, windfall), starting at the thicket; at either end a passage sleeps and the
     year does not move.** Recommended. Ocarina of Time's two ages and Dishonored 2's two times show how few states a
     player holds in mind; this plan needs two for the main path (III, IV) and the others for side paths and a second way
     up. Stopping ends keep "which way is younger" true everywhere.
   - (b) Four on a wheel: past the windfall the clearing seeds again (IV to I, and back). The Celtic wheel of the year, and
     no passage ever sleeps; but at the wrap "younger" leads to the oldest wood.
   - (c) Two (young wood and old wood). The clearest; it loses the second door the branch is built on.
4. **How does the player read the year, and which way is younger?**
   - **(a) In the world: a flat entrance stone at every mouth with a spiral turning sunwise (in here, older) or widdershins
     (in here, younger) and four cups (the present year gold, the year you will come out into green); a year hazel at
     every mouth on Life's side; a status line and a short framing on coming out.** Recommended. Dishonored 2's timepiece
     let the player look into the other time before shifting, and that preview made each shift a choice, not a gamble; the
     cups are that preview in the land's carving. The owner chose world marks over HUD marks for the Water land's tide.
   - (b) The same, plus a small year mark on screen while on Life's side. Certain; a HUD mark the other lands do not have.
   - (c) The year hazels only. Purest; the direction must then be learned by trying.
5. **What is the land's trial?**
   - **(a) The Whispering Knights: a stone knight who moves only unwatched and a green knight who roots and regrows, taking
     turns, each able to undo the other.** Recommended. It is the one trial only a duo land could have, a small, fair
     rehearsal of the gods' "each undoes the other" before the meeting ground; Dark Souls' Ornstein and Smough show how
     much a duo teaches when the pair's moves interlock. It keeps the owner's fairness rule for duos (they take turns).
   - (b) The Turning Barrow: waves of Life's people in a chamber whose own year turns every 25 s. The land's verb as a
     fight; heavy to tune, and it makes the years a fast hazard rather than a slow puzzle.
   - (c) Count the Stones: break a ring of countless until the number standing matches the King Stone's three times.
     Rollright's own legend; poor to read from above, and one enemy's mechanic repeated.
6. **How many held camera shots?**
   - **(a) Four: the arrival over the bog, the two seats, and the brow of the rise.** Recommended. The owner chose one
     held shot per district for the Water land; this land has four districts, and the decided brawl views already need
     three. The arrival is the only way the two seats can be the bog's landmarks, as `WORLD.md` proposes, since no game
     camera sees farther than 7 m.
   - (b) Three: the seats and the brow; the arrival is the ordinary game camera. Keeps the Water land's count; the
     landmarks are then known only by the Fork Stone's carvings.
   - (c) The seats only, with the brow as a vista zone. The brow's vista cannot reach the bowl, so the last walk's view
     (Decided) would be dust over the bank.
7. **The plain weapons: the plain Scythe from the stag, the plain Massive Sword from the eagle.**
   - **(a) In this land's make (an iron scythe on an ash snath bound with bronze; a slab of a fallen stone knapped to an
     edge), playing exactly as the plain weapons.** Recommended, as the owner chose for the Chain Sickle, the Great Bow
     and the Twin Knives.
   - (b) In the weapons' standard look. Less modelling; two odd objects in a land of stone and wood.
   - (c) Given by the two elites on the main path. Sure to be found; the Oldest Animals lose their gifts.
8. **Does the second branch change because the first is done?**
   - **(a) No: both branches are built to the same pressure; the second is easier only because the player is stronger,
     and the second elite acknowledges the first seal in a line.** Recommended. Mega Man made the order of its stages
     matter through what the player carries, not through the stages; Dark Souls II's four great souls are fixed wherever
     they are taken. Scaling makes the choice of order meaningless.
   - (b) The second branch's elite grows stronger (Breath of the Wild's Blights gain health with each beast freed). Keeps
     the second fight tense; it punishes nothing but the order.
   - (c) The war spreads: with one seal broken, a few of the other side's people appear in the second branch (King's Men
     in the wildwood, Green Men on the hill). Good lore; it doubles the encounter tuning for both orders.

---

## Sources

- Newgrange's passage (19 m) and the solstice sunrise down it, the roof-box, the triple spiral in the chamber:
  [Heritage Ireland](https://heritageireland.ie/visit/newgrange-winter-solstice/),
  [Wikipedia, Newgrange](https://en.wikipedia.org/wiki/Newgrange).
- The Rollright Stones (the King's Men who cannot be counted, the King Stone and the seven strides, the Whispering Knights):
  [The Rollright Stones, folklore and legends](https://www.rollrightstones.co.uk/articles/story/folklore-and-legends),
  [Wikipedia, Rollright Stones](https://en.wikipedia.org/wiki/Rollright_Stones).
- The Corlea trackway, an Iron Age oak plank road across a bog: [Wikipedia, Corlea Trackway](https://en.wikipedia.org/wiki/Corlea_Trackway).
- The Wild Hunt and Gwyn ap Nudd: [Wikipedia, Wild Hunt](https://en.wikipedia.org/wiki/Wild_Hunt),
  [Wikipedia, Gwyn ap Nudd](https://en.wikipedia.org/wiki/Gwyn_ap_Nudd).
- Dishonored 2's *A Crack in the Slab* and its timepiece (two times in one place, looking before shifting):
  [Kotaku](https://kotaku.com/what-made-dishonored-2s-time-travel-level-so-good-1819596566),
  [Dishonored wiki](https://dishonored-archive.fandom.com/wiki/A_Crack_in_the_Slab).
- Ocarina of Time's two ages working together in the Spirit Temple: [CBR](https://www.cbr.com/zelda-oot-spirit-temple-guide/).
- Inside's shockwave field (a fixed rhythm, cover to run between, the player put back at the right point of the cycle):
  [Inside the Loop, *Game Studies*](https://link.springer.com/article/10.1007/s40869-018-0071-x),
  [Audiokinetic, Wwise Tour 2016, Playdead](https://www.audiokinetic.com/en/learn/videos/tcsuvzujmlw/?course=wwise301&lesson=1).
- Branches in any order: Dark Souls II's four great souls ([Forbes](https://www.forbes.com/sites/erikkain/2014/03/31/dark-souls-ii-review-part-two-all-the-pretty-castles/),
  [Fextralife](https://darksouls2.wiki.fextralife.com/Game_Progress_Route)); Breath of the Wild's Divine Beasts and their
  scaling Blights ([Zelda Central](https://zeldacentral.com/games/breath-of-the-wild/divine-beasts/)); Mega Man's stage
  select and weapon weaknesses ([Wikipedia, Mega Man (1987)](https://en.wikipedia.org/wiki/Mega_Man_(1987_video_game))).
- From memory, not looked up this session: Stonehenge's horseshoe and axis toward the midsummer sunrise; Avebury's Cove and
  the Kennet Avenue; lynchets; cup-and-ring marks; *deiseal*, the sunwise turn; bullaun stones; the Odin Stone at
  Stenness; the Oldest Animals and their signs of age in *Culhwch and Olwen*; Efnisien bursting the Cauldron of Rebirth in
  *Branwen*; the salmon and the hazels of the Well of Segais; the Green Knight's holly bough; Pliny's golden sickle; Dark
  Souls' Ornstein and Smough.
