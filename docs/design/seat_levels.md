# The last land's levels: the seat under the eclipse (first pass)

Written by Claude on 2026-11-19 as the last champion's land's counterpart to `fire_levels.md` and the plans that
followed it (`water_levels.md`, `metal_levels.md`, `ice_levels.md`, `poison_levels.md`), before any of the land is
built. **Everything here is Proposal (Claude)** unless it is marked **Decided** with its source. The land's myth, the
champion and his fight are `WORLD.md`, "The last champion and his land (Indian)"; its look is `LOOKS.md` section 10; the
rules every land follows are `WORLD.md`, "Level design rules for every land (decided 2026-11-18, evening)". The fight
itself is not designed here: this plan gives the walk, the council as a room, and how the room plays with the moves
`WORLD.md` already lists, as `fire_levels.md` section 5 did for the Champion of the Gate.

Axes as in Fire: x east, z north, y up; north is up the screen. Coordinates are metres. "Floor y 8" is the walking
height. The land is one scene (name proposed: `StoppedWar`, built by `StoppedWarBuilder`, menu `Clover/Build/Stopped
War`): the path and the council must share a scene, because the council's first sight is taken from the path and its
shadow reaches out onto the path. The player wakes at (0, 0, 0).

---

## 0. What the owner has decided, and how this plan reads it

| Decided | Source | How this plan uses it |
|---|---|---|
| A single path through a massive battlefield of a great war frozen in time; enemies from each land unfreeze close to the path and attack | `WORLD.md`, "The land: the seat under the eclipse" | One path, about 450 m to the council, never a corridor: it braids, climbs and offers a choice of risk in every section (section 2). Its edges are where the war wakes (1.2) |
| At the end, a council; each seat themed to one element; Light's and Dark's in front, vacant or destroyed by the breaking | same | Section 5. Light's and Dark's stand either side of the ring's south opening, the side the player comes from |
| The final fight is in the area between the seats; the godly fight turns the arena into a separate domain; phase 2 is a split arena, half blazing day and half night | same; `LOOKS.md` 10 | Section 5.4 lays the floor so the line can run along the gnomon's shadow (Proposal in `WORLD.md`) |
| The champion causes the eclipse: a last crescent of sun burns over the frozen land until his chakram covers it at the end of phase 1 | `WORLD.md`, 2026-11-18; `LOOKS.md` question 13 | The crescent is the land's only direct light, and its place in the sky is chosen for the fight (1.1, item 4) |
| The owner is wary of imagery too close to Mount Olympus | `WORLD.md`; `LOOKS.md` 10 "Avoid" | No colonnades, no cloud floors, no thrones in a row. Seats are small stepped shrine-towers; the gnomon is an Indian stair-sundial (5.2) |
| The frozen world is grey and the bubble round the player brings colour back; the council's seats each in their element's colour | `LOOKS.md` 10 and question 5 (Decided, 2026-11-17 evening) | The thaw is the land's readable shape (1.2); the council is the first place with colour of its own |
| Rest fires in Fire's rhythm: one every 6 to 8 minutes of a new player's walk, always one at the god's door, a shortcut loop per district back to its fire | `WORLD.md`, level design rules | Four fires and three shortcuts for a land of about 18 minutes (section 2). The usual 5 to 7 is for lands twice this long |
| Two kinds of optional discovery: "see it, then find the way" holding lore, and one optional trial invented per land | same | The champion's chariot, the Stutter and the player's empty place in a frozen rank (section 4; the empty place Decided, owner, 2026-11-19, answer 5); the trial is the Chakravyuha (4.3; Decided, owner, 2026-11-19, answer 4) |
| Up to three walkable levels per district, joined by ramps, stairs and ladders seen from above; nothing walkable under a roof | same | Each district names its three levels. Nothing hanging in the frozen air is walkable or roofs a floor (1.1) |
| The hub and the council are two places; the hub is the sponsor's shore | same | The council belongs only to this land. Rest fires still reach the hub, as everywhere |
| No ride out of this land | `WORLD.md`, the rides table | The land ends with the fight and the champion's last words |
| Clean pass for a new player: 15 to 20 minutes before the last champion; the final area a bit shorter, the weight on the last fight | `WORLD.md`, "How a climb is structured" (the 15 to 20 is a Proposal there, used as the target) | Section 8: 18.3 minutes to the champion |

The owner's answers to earlier plans carry over where they are rules of the game and not of one land: the world's own
marks over HUD marks (`water_levels.md` 9), at most one held camera shot per district, rides out not counted
(`kit_list.md` Q1, Decided 2026-11-19), noise drawn as rings (`ice_levels.md` 9), and the clock's state kept honest
after a fall. The other cross-land answers of `kit_list.md` section 5 (Decided 2026-11-19) change this plan in four
places: the walk's arrows are pale with the standard perfect window (Q3, 1.3); the frozen Hunt uses Earth and Life's
rules (Q4, 3B); the frozen bolts are the shared strike piece, a ring here (Q5, 3A); and the storm host's people are the
Lightning and Wind plan's own (Q9, 1.6 and 3A).

---

## 1. Principles

### 1.1 What the camera sees: what changes for this land

The camera is Fire's (`fire_levels.md` 1.1): pitch 55 degrees, 12 m, vertical field 40 degrees. The picture reaches
7.2 m north and 4.3 m south of the player and is 15.5 m wide at the player; nothing beyond about 7 m north is on screen;
something H m above the floor is in frame only within (5.0 - H) / 0.7 m north; landmarks are rooms the player arrives in,
or vista zones; the land rises north and its drops face south; the south-side rule; roofs fade; enemies walk straight
at the player with no navmesh; ranged attackers stay in frame. What this land changes:

1. **The camera eases back to 1.15 times for the whole land** (`TopDownCamera.SetFraming(1.15, (0, 0, 0), 0)` set by
   the land, not by a zone). The picture then runs from 4.9 m south to **8.2 m north** and is 17.9 m wide at the player.
   This is for the thaw: a 5 m circle round the player then almost fits the frame, with 3 m of grey world beyond it
   to the north and nearly 4 m to each side, which is the band where the player reads what is about to wake (1.2).
   LOOKS asks that "a game-camera frame always shows frozen grey world all round it"; at Fire's 1.0 the north margin
   is only 2.2 m. (Decided, owner, 2026-11-19, answer 1: 5 m, and 1.15 times for the whole land.)
2. **Nothing in the frozen air is a floor or a roof.** Arrows, dust, debris, a breaking wave's crest and rearing
   elephants hang over walkable ground. None is walkable (the player's own thaw would set it moving under them) and
   none is dense enough to hide the floor: hanging things above 2.5 m within 4 m south of the player dither out, as
   roofs fade. The arrow canopy (3A) is a lattice with 1.5 m gaps, never a sheet.
3. **The frozen ranks are the walls.** The path is bounded by the war itself: soldiers, chariots, elephants and siege
   works standing shoulder to shoulder. Each figure has a simple solid hull, so the path's edge is a wall of bodies,
   never a fall. Behind the first two ranks the war is scenery only (instanced, no colliders, 8.3).
4. **The light is a low crescent in the north.** The sun stands due north at 30 degrees, a hooked crescent with the
   corona round the rest of the disc (`LOOKS.md` 10). Shadows are 1.7 times a thing's height, faint and sharp, and they
   all point **south, toward the camera**: every frozen soldier throws a long shadow down the frame. Three reasons:
   - *A compass.* The council is north. Walking against the shadows is walking toward the seat, with no HUD.
   - *The council's shadow reaches out.* The gnomon's frozen shadow runs out of the ring and down the steps toward
     the player, the first thing of the council the player meets on the floor, as the obelisk's shadow was the first
     thing of Khepharu's court (`fire_levels.md` 3.5). The first god's room and the last rhyme (Proposal in `WORLD.md`).
   - *The last reflect faces up the screen.* In phase 3 the player reflects arrows into the chakram held before the
     sun (Decided). With the sun north, that guard faces the top of the frame, the most readable direction there is.
   Under anything with gaps (a banner's fringe, the arrow canopy) the light falls in small crescents, as in a real
   partial eclipse (`LOOKS.md` 10).
5. **The floor carries the story.** The path is the track the champion cut through the frozen war: two chariot ruts in
   the trampled earth for its first half, and from the bronze road onward, where his chariot turned aside, the prints
   of a man walking (4.1). Beside the path, the war's fallen weapons from every land, landed arrows standing in the
   earth in strips (1.3), and dust that does not settle.

### 1.2 The thaw

**What it is** (Proposal in `WORLD.md`: time cannot fully hold the player, who carries elements, so a small radius
around them moves while the rest stays stopped; enemies thaw inside it, so the path's edge is dangerous). The rules,
all Proposal:

| | Inside the thaw (5 m round the player) | Outside |
|---|---|---|
| **The player**, and everything the player throws or casts | Full speed | Full speed. Where the player's blow, arrow or spell strikes, it carries a small thaw of its own: 1.5 m round, for 2 s |
| **The bearers**: the corrupted of every land, who carry their land's element in them | Wake (below), then full speed and hunt the player | Frozen. An awake bearer that leaves the thaw **clings** to it for 4 s, then freezes in whatever pose it is in |
| **The mortal war**: soldiers, horses, elephants, banners, dust, water, snow | Moves at **the slow**, about a tenth of real speed: the war resumes its last second, slowly, round the player. Its blows are aimed at each other, never at the player | Frozen |
| **Things in flight or falling**: arrows, stones, a wave's crest, a pour of bronze, a lightning bolt, a rearing elephant's feet, a toppling ladder | Tremble 0.5 s, then move at the slow along their line; falls land 1.6 s after they start to move. They **hurt whatever they hit**, the player and bearers alike | Frozen in mid-air |
| **The rest fires** | Lit by the thaw | Stay lit, each holding a ring of thaw 3 m round it: the only colour the player leaves behind |
| **The council** | | Moves on its own: its seats hold their elements (section 5) |
| **The figure on the bed of arrows** | Takes colour | Awake, grey, moving (2C) |

So there are three speeds and each looks different: the player and the bearers at full speed and full colour; the war
in slow motion and full colour; everything beyond, still and grey. Only full-speed things hunt. (Decided, owner,
2026-11-19, answer 2: these three speeds.)

**The bubble from above** (the decided shape of `LOOKS.md` 10, sized here):
- **Radius 5 m.** The colour and motion rise across an **edge band 1 m wide** (4.5 to 5.5 m), not a line: dust
  half-falling, a banner twitching, an arrow trembling. On the floor the band is where trampled grey earth warms to its
  ochre and the fallen marigolds turn orange: the ring reads on the floor first, because the floor is the canvas
  (`LOOKS.md` rule 1).
- **Behind the player** the colour drains and the motion stops again as the bubble passes.
- **Size and the frame.** At the land's camera (1.1, item 1) the whole circle is on screen except a sliver at the bottom;
  the grey world shows beyond it on three sides. A bigger bubble would push the grey to the frame's edges and lose the
  image; a smaller one gives a player in a fight no room to roll inside it (a roll is 4.5 m).

**How the player sees what is about to wake.** Thousands of grey figures stand round the path; a few dozen are bearers.
They must be told apart before they move:
1. **The stir (8 m).** A bearer within 8 m of the player stirs even while frozen: a thread of its own element at its
   edge, the only colour outside the thaw in the field. A wick's crack glows ember-orange, a gilded's gold catches, a
   brittle breathes frost, a clay soldier's paint flakes red, a samurai's armour flickers cyan. Mortal figures never
   stir. The rule the player learns: **in the grey, anything with a thread of colour will wake.** This is `WORLD.md`'s
   "readable thaws" (figures about to wake show cracks of motion), with the element doing the telling, so each land's
   people are known by their colour as well as their silhouette (`LOOKS.md` 10: silhouettes tell the lands apart).
2. **Silhouette.** Each land's people keep their silhouettes in grey: the gilded's square shields, the pack on all
   fours, the clay in ranks, the storm samurai's flared helmets, the King's Men in their marching file.
3. **Waking (1.2 s).** When the edge band reaches a bearer, colour floods it from its element outward, the grey flakes
   off in dust, it turns its head to the player, and only then does it move. It can be struck while waking: the first
   blow lands, and wakes it at once. One free blow, as the kneeling thirsting gave in Fire.
4. **Again (0.4 s).** A bearer that froze after clinging (above) wakes again in 0.4 s when the bubble touches it, in the
   pose it froze in. Its wounds stay.
5. **Struck from afar.** A spell or a reflected arrow that strikes a frozen bearer thaws it where it stands (its small
   thaw), so the player can **wake one at a time from range**: the pull of every soulslike, made the land's own.

**What this makes of a fight.** On a single path, where the player walks decides what wakes. A rank seen from its end
wakes one man at a time; seen from its middle, it wakes whole. A volley crossed slowly comes a few arrows at a time;
rolled through, it comes all at once. The thaw is how a one-way path still holds choices: every section offers a line
close to the war's edge and a line away from it, and each has its own price.

**Clinging, and why** (Decided, owner, 2026-11-19, answer 3). A bearer that is fighting the player stays inside the bubble, so it never freezes
mid-fight. Clinging matters only to the player who walks away: a crowd can be shed by outwalking it (most bearers are
slower than 6 m/s), and each one freezes 4 s later where it stood. It comes back to life when the player returns. That
is a tool and a debt, not an escape: the path is single, and what is left frozen behind is met again after a fall.

**After a fall** the sponsor turns time back (Decided, `WORLD.md`, "How a climb is structured"): every bearer stands
again in its first frozen pose, every hanging arrow and stone hangs again, and the land's enemies return as in every
land. What the player let fall to open a shortcut stays fallen (a progress key each).

### 1.3 The frozen arrows

**Decided as a proposal in `WORLD.md`**: frozen arrows in the air start flying again as the player passes, making hazard
lanes. The rules (Proposal):

- **Two kinds.** *Flat* arrows hang at body height (1.0 to 2.0 m), flying nearly level: they cross the path along a
  line. *Falling* arrows hang 3 to 6 m up, dropping steeply: they rain onto a strip.
- **How each is read from above.** An arrow is 0.9 m long, about 75 pixels across at this camera: its shaft and
  fletching show its heading. Behind each hangs a short frozen streak of disturbed dust, 1.5 m long, so it reads as an
  arrow in flight and not a stick (Quantum Break's frozen scenes used long-exposure streaks for the same job). Where a
  volley lands, the floor already bristles with the arrows that landed before it: **the stubble marks the strip**, a
  band of standing shafts the player sees before any arrow moves.
- **In the thaw** an arrow trembles 0.5 s, then flies at the slow, **5 m/s**, along its line; it sticks where it lands
  or freezes again where it leaves the bubble. An arrow coming at the player from the bubble's edge arrives 1.5 s after
  the edge touched it: time to read, step or guard.
- **The player sets the pace.** Arrows thaw only where the bubble goes. Standing still, the arrows already inside fly
  out or land in about 2 s and the bubble is quiet. Walking brings new ones in at the leading edge; rolling brings many
  at once. Walk slowly through a volley and it comes a few at a time. This is Superhot's "time moves only when you move"
  made spatial: the war moves only *where* the player goes.
- **A hit** is 12 damage and a flinch of 0.25 s. Arrows are guardable from the front (they read pale), with the
  standard perfect window of 0.18 s, not Ice's 0.10 s (`kit_list.md` Q3, Decided 2026-11-19: one warned-arrow piece,
  its guard rule per source).
- **The perfect guard reflects.** A perfect guard sends the arrow back up its line at full speed: it is the player's now,
  it does not freeze, and it strikes the first bearer in its way (40 damage) or wakes a frozen one (its small thaw).
  This is the walk's rehearsal for the champion's last phase, where perfect guards against his arrows crack the
  chakram (Decided). The reflect arc's lessons (`WORLD.md`, Proposal) become: the sun in Fire, her arrows in Ice, the
  war's arrows on the path, his arrows and the astra in the fight.
- **Never two red things together.** As Ice's fairness rules (`ice_levels.md` 1.3): no volley thaws over a ladder, and
  a bearer's red attack and a thawed arrow never land within 1.2 s of each other on the player (the arrows nearest the
  player hold their tremble).

The volleys, in walking order, escalate from a lesson to a gauntlet (each is placed in its section):

| | Where | Kind | What it teaches |
|---|---|---|---|
| A1 | 1B, z 28 | Three lone flat arrows across the sunken road | An arrow flies when the bubble reaches it, along its heading |
| A2 | 1C, z 84 to 96 | A thin flat volley over the east half of the path | The price of the east line beside the wave |
| A3 | 2A, z 176 to 196 | Falling arrows over a strip of deep snow | Walk slowly and it rains a little; missed arrows are noise to the buried |
| A4 | 2B, centre (4, 5, 246) | **The thousandfold arrow**, frozen mid-split | A preview of his first decided attack (below) |
| A5 | 3A, z 336 to 346 | **The arrow canopy**, a lattice overhead | A preview of his canopy, dropped in rows |
| A6 | 3C, z 380 to 404 | **His volley**, flat arrows down the last rise in five lanes | The reflect, under pressure, before the fight |

**The field teaches his moves.** The war's arrows are his: he shot them in the war. So A4 and A5 hang as his techniques
frozen half done, and the player walks through them once, slowly, before meeting them at full speed. The thousandfold
arrow (Decided: one arrow shot into the air multiplies into 10, then 100, then 1000) hangs as a tree of arrows, one high
out of frame, ten below it, a hundred in a cone 2 to 4 m over the floor, each with its landing ring drawn in hanging
dust on the floor, so the split and its gaps are shown before they are fought (his telegraph, Proposal in `WORLD.md`).
The canopy (Proposal in `WORLD.md`, the Khandava forest) hangs as a lattice whose shadow grid lies on the floor. This is
how the Champion of the Gate showed Khepharu's patterns before the god.

### 1.4 Let it fall: the land's way of opening things

Nothing in a frozen world can be moved but what the player thaws. So the land's shortcuts and side ways are opened by
**letting a frozen fall finish** (Proposal). Things caught mid-fall lean at angles no standing thing could hold, with
dust hanging round their base: a siege ladder, a falling tree, a hanging stone. Each is held by a **prop** (a frozen hand
on a rope, the last uncut wood, a jammed wedge) that stands on the far side. Struck in the thaw, the prop gives, the
thing finishes its fall at the slow (1.6 s) and stays where it lands, a ladder, a bridge, a broken gate (a progress
key). Quantum Break's designers used the same idea: objects caught in time loops became the platforms that let the
player on, or the hazards in the way.

### 1.5 Measures

| What | Measure | Why |
|---|---|---|
| Walk, roll | 6 m/s; roll 4.5 m | As Fire |
| The thaw | Radius 5 m, edge band 1 m | 1.2 |
| The stir | 8 m | Bearers within the frame's north edge (8.2 m) show colour before they wake |
| Waking; waking again; clinging | 1.2 s; 0.4 s; 4 s | 1.2 |
| The slow | About a tenth of real speed; arrows 5 m/s; falls land 1.6 s after they start (Metal's stone throw is 1.6 s from shadow to landing) | Slow enough to read, quick enough to matter |
| A thawed arrow | Tremble 0.5 s, 5 m/s, 12 damage, a 0.25 s flinch; guardable (pale), perfect window 0.18 s; the perfect guard reflects | 1.3 |
| A thawed fall (stone, crest, foot, bolt) | Shadow or cracked mark on the floor from the moment it trembles; lands 1.6 s later; 30 to 40 and a knockdown | Every fall is marked on the floor before it lands |
| The path | 10 to 18 m wide between the frozen ranks; narrowest 8 m | Wider than the bubble's diameter at its widest, so there is always a line away from one edge |
| A group fight | 12 to 16 m across, 10 to 12 m deep | As Fire |
| A rise that reads; a drop; a step; stairs; ramps | 1.5 m; 2 m; 0.3 m; 27 to 32 degrees; 8 degrees fought on, 15 walked | As Fire |

### 1.6 Each land's people, as the rooms use them

The recap reuses each land's people with **their own mechanics unchanged**, as their land's plan specifies them, plus
the thaw (Decided, owner, 2026-11-19, answer 6: four sections in the order of the tiers, each with its lands' own
people and one piece of their rule caught mid-event). Nothing is reskinned and no land is rebuilt: each section is that land's host, camped where it stood when the
war froze, with a few pieces of its kit and one piece of its land's rule caught mid-event. Two or three of each land's
people appear, chosen for how their mechanic meets the thaw:

| Land | Who (their own mechanic, from their plan) | What the thaw adds | Section |
|---|---|---|---|
| Fire | Wicks (the grab; flame spreads to the dormant and to oil jars), gilded levies (the shield wall advancing in step), the ashen (the burst) | Fire spreads only inside the thaw: a jar lit and left freezes mid-burst and goes off when the player comes back. An ashen frozen in its first instant of bursting is a fireball hanging over the road | 1B |
| Water | A hooked harpooner (the line, the yank; cut the line), a drowned (the grab from the foam) | The Water host came on a wave: a wall of sea frozen mid-break beside the path, that crashes only as far as the player walks beside it | 1C |
| Metal | A wheeler and a mender on a rail (break the repairer first), a chained (the reach circle round its cart) | A crucible frozen mid-pour; the bronze lands and pools only in the thaw | 1D |
| Ice | The pack (runners, the howler, the pincer), brittle (the shatter, the chain), a buried (the moving line, drawn to noise) | Snow hangs in the air; missed arrows are noise; brittle stand among frozen mortals and look like them | 2A |
| Poison | A clay formation (the lock, the officer, the volley comb), a vessel (the heavy cloud flowing downhill) | The formation wakes only where the thaw touches it: wake the front and the officer behind stays frozen and cannot close the gap | 2B |
| Lightning and Wind | A struck samurai (light blows at 25% on its armour; a bolt within 2 m charges it for 4 s, then the armour bursts and it is stunned 2 s), a fox pair (one real guard and a fox-fire double that casts no shadow; they wake when the player passes through their gate), a crow-tengu with a feather fan (the swoop, and the fan crescent that shoves 1 m) | Lightning frozen mid-strike stands along the road; the bolts strike as the thaw reaches them | 3A |
| Earth and Life | The King's Men (they move only while the player is not facing them), a Green Man (roots and heals) | The Wild Hunt frozen mid-ride across the path; riders thawed gallop on, cannot pass stone (Decided for their land), and keep their land's rules: the ring of untouched heather round each stone as shelter, 25 damage that cannot kill, the carry west | 3B |

Lightning and Wind's and Earth and Life's people follow their own plans, written after this one
(`lightning_wind_levels.md` 1.6, `earth_life_levels.md` 1.7 and 3.5; `kit_list.md` Q4 and Q9, Decided 2026-11-19). The gu (Poison) and the bait (Water) are left out:
the gu's crawl needs a living host within 12 m and the bait's lure needs water at low tide, and neither reads in a
moment in a land with no time to teach it again.

**Why every land's host is here** (Proposal, for the owner's story): when Light and Dark fell at the seat, every land's
god sent its host to the field below the council to take what was left, and this land's mortal army met them. Time
froze the war so that its champion could cross the field and take Light and Dark (Decided: "The sponsor froze a great
war in time so that the champion could consume Light and Dark"). The bearers are each land's corrupted; the grey
army round them is mortal. **Decided** (owner, 2026-11-19, answer 5): this is the war of the game's opening, which is
played on this field in its last minutes; on the path the player finds the one empty place in a frozen rank, where they
stood when the sponsor took them out of time (4.4).

---

## 2. The whole land

The path leaves the near ranks of the war, climbs through each land's host in the order of the climb, passes the old
commander on his bed of arrows, crosses the storm and the stones, and climbs the last rise into the champion's volley.
At the crest the council is seen whole; three flights lead up to the god's door, where Time's shadow lies across the
steps and the champion sits in it. The main path is about 450 m of walking.

```
 z
495  ............. RING OF TWELVE PLACES, centre (0,14,470), floor r 22 ..............
470        gnomon (x -2..2, z 462..478, 2 m -> 22 m high), hour channel r 12
445  ====== Time's bare place (0,14,445): THE CHAMPION; Light's seat E, Dark's seat W ======
440  ---- tip of Time's shadow (the line) ----
436  top landing y14        flight 3: 13 -> 14 (z 432..436)
428  F4*(6,13,428)  V5      flight 2: 12 -> 13 (z 420..424)
410  PROCESSIONAL x -6..6   flight 1: 11 -> 12 (z 410..414)
404  ===== CREST y11 ===== held shot 3: the council
380  3C THE LAST RISE y8 -> 11   A6 his volley (five lanes)     V4 at the foot
     3B THE STONES (Earth and Life) y8: standing stones, the frozen Hunt, King's Men
345  3A THE STORM'S FILE (Lightning and Wind) y8: frozen bolts, A5 the canopy   [S3 banyan W -> F3]
                                                     [gully W -> SP2 THE STUTTER (-60,4,300)]
300  2C BHISHMA'S HILL y8: the bed of arrows, F3*(-2,8,286), held shot 2, V3 west
258  2B THE CLAY SQUARE (Poison) y5: the formation, A4 the thousandfold arrow, vessels on the slope
212  2A WINTER'S EDGE (Ice) y3: hanging snow, deep snow and the track, the pack, A3   [S2 postern -> F2]
164  ======== the rampart (top y6), its gate y3; F2*(5,3,156) =====  [rampart top E -> SP1 THE CHARIOT (34,3,140)]
     1D THE BRONZE ROAD (Metal) y3: rails, the crucible, the elephants, the siege tower (top y9, V2)
112  1C THE TIDE LINE (Water) y3: the standing wave W, A2 E
 66  ===== ramp head =====
     1B THE GILDED RANK (Fire) y0: the sunken road, banks y3, A1, wicks, the hanging burst, the shield wall's ramp
 20                                                     [S1 ladder: 1A up the W bank -> the outer lane -> ramp head]
  0  1A THE WAKING y0: wake (0,0,0), F1*(-3,0,6), held shot 1
       x: -30      -15       0       15       30
```

**Heights, south to north.** The sunken road 0; its banks 3; the tide line, the bronze road and the rampart's gate 3;
the siege tower's top 9; the rampart's top 6; winter's edge 3; the clay square 5; Bhishma's hill, the storm's file and the
stones 8; the crest 11; the processional's landings 12 and 13; the council 14. **Three levels per district**: D1 the
road (0), the plateau (3) and the siege tower or rampart top (6 and 9); D2 the snow (3), the square (5), the hill (8);
D3 the Stutter's crater and the ravine (4), the moor (8), the crest and the processional (11 to 14).

**Rest fires (four).** Each is a cook-fire of the frozen host, a pot on three stones with its flame standing still,
which the player's thaw lights and which then holds a ring of thaw 3 m round it (1.2). No bearer stands within 12 m of
a fire.

| Fire | Where | Foot | Why there |
|---|---|---|---|
| F1 | The waking, among the camp's rear | (-3, 0, 6) | In sight of waking; S1 comes down 9 m from it |
| F2 | The rampart's gate, inside its south mouth | (5, 3, 156) | After three lands' hosts, about 7 minutes in; S2 comes out at the postern 25 m west; SP1 starts from the rampart top above it |
| F3 | Bhishma's hill, his attendants' fire | (-2, 8, 286) | The land's one quiet stop; S3 and the Stutter's way home end here |
| F4 | The processional's second landing | (6, 13, 428) | The god's door (Decided: one always there), 17 m from the champion, below the line |

From F1 to F2 is about 7 minutes of a new player's walk, F2 to F3 about 6, F3 to F4 about 5: the decided rhythm for a
land this short.

**The route.** Wake (F1); north up the sunken road through the Fire host, up the levy ramp; along the tide line beside
the standing wave; across the bronze road, past the crucible and under the rearing elephant, to the rampart's gate
(F2). Through winter's edge, by the snow or by the track; up the stair onto the clay square, through or round the
formation and under the thousandfold arrow; up the vessels' slope to Bhishma's hill (F3). Down the storm's file between
the bolts and under the canopy; across the moor through the Hunt and among the King's Men; up the last rise into his
volley; the crest; the processional (F4); the line; the champion.

**Branches and loops.**
- S1 (D1): a siege ladder let fall from the west bank's top, from 1A up to the outer lane, which runs north to the ramp
  head: F1 to the tide line without the Fire host.
- SP1, the champion's chariot (see it from the siege tower; reach it from the rampart's top): lore and Shards.
- S2 (D2): a postern in the rampart, barred on the south side, opened from winter's edge: F2 to the snow's west edge.
- SP2, the Stutter (see it from Bhishma's hill; reach it from the storm's file): the scar of the champion's rebellion;
  its rope ladder climbs back to F3.
- S3 (D3): a banyan let fall across the west ravine: the moor to F3 without the storm's file.
- SP3, the empty place in a frozen rank, beside winter's edge (4.4; Decided, owner, 2026-11-19, answer 5).
- The trial, the Chakravyuha, east of the clay square (Decided, owner, 2026-11-19, answer 4).

**Vista zones and held shots** (`LandZones`; the look may point any way, as in Water's W19). A vista opens only while no
bearer is awake within 12 m. Every vista toward the north shows, at its top edge, the council as a small ring of
coloured lights on the far ground, the only colour in the grey that the player did not bring: the landmark always in
view that `LOOKS.md` rule 8 asks for, held back from its full reveal until the crest.

| | Where | Box | Looks | Shows |
|---|---|---|---|---|
| V1 | The waking, (0, 0, 10) | 10 by 4 m | North | **Held shot 1** (below), then the sunken road and the first stirring wick |
| V2 | The siege tower's top, (-16, 9, 150) | 3 by 3 m | East | The ruts turning east through the elephant line to the chariot on its mound (SP1) |
| V3 | Bhishma's hill, west edge, (-12, 8, 284) | 4 by 6 m | West | The Stutter far off: a flicker of colour and motion in the grey where nothing should move (SP2) |
| V4 | The last rise's foot, (0, 8, 382) | 12 by 3 m | North | His volley hanging in five lanes up the rise, the tortoises of shields, the crest |
| V5 | F4, (0, 13, 428) | 12 by 3 m | North | The top flight, Time's shadow lying down the steps, the champion seated in it between the two broken seats, the coloured seats beyond |

**Three held shots** (one per district, as the owner chose for Water and then for every land, `kit_list.md` Q1,
2026-11-19):
1. *The stopped war* (V1, on waking): the camera rises to 30 m for 6 s and shows the war standing still to the edge of
   sight in every direction, arrows hanging in sheets, and one circle of colour in it with the player at the centre
   (`LOOKS.md` frame `ch_thaw`). Then it comes down to the land's camera.
2. *What the player left behind* (2C, at F3, once): the camera rises and turns south for 5 s. The whole road walked so
   far is grey, except two small rings of colour where F1 and F2 still burn.
3. *The council* (the crest, once): the camera rises to 40 m and looks north for 7 s: the mandala floor, the ring of
   seats each in its substance, the gnomon and its long shadow running out of the ring toward the player, the broken
   grey seats in front, the champion sitting between them, the frozen war round the ring on every side (`LOOKS.md`
   frame `ch_council`).

---

## 3. The districts

Each district: the idea, its three levels, then each space in walking order. Encounters are numbered E1.1 and so on as
in Fire. Every bearer is placed so that the straight line from it to where the player will be is open, and every
ranged bearer stands where it is in frame when it shoots.

### 3.1 District 1: the near ranks (z -4 to 164)

**The idea.** The rear of the war: camps, then the hosts of the first three lands. The district teaches the thaw in
the order a player can take it: colour, then a bearer waking, then an arrow, then a fall, then a choice of line.
**Levels**: the sunken road (y 0), the plateau and the bank tops (y 3), the siege tower's top and the rampart (y 9
and 6).

```
 z     x: -24   -14  -6    0    6   14       34
164   ======= RAMPART top y6 ===|gate y3|======== rampart top E -> rope ladder -> SP1 chariot mound (34,3,140)
156   [S2 postern -20]          F2*(5,3,156)
150   siege tower (-16) top y9, V2           elephants x10..16 (frozen line)
138   (c)chained cart rail      rearing elephant over x5..9
128   crucible (-10) pour -> (-6)    (wh)rail z132  (m)(6,128)
112   ===================================================
100   WAVE x-8..-4 | kerb -4..-2 | (d)    path to x12
 92                    (h)harpooner (2,3,92)   A2 x4..12
 66   ========= ramp head y3 ======= outer lane (W bank top) runs S
 63         levies (-2.4,0,2.4)
 44   ramp foot y0   [stair E bank (6..8, z40) up to E bank top y3]
 40    (w)(-2,38)  burst (4,40)  oil jars E foot z30..44   (w)(3,34)
 28    A1  three arrows across the road
 20   ---- sunken road x -6..6, banks y3 ----
 16   [S1 ladder (-8,0,15) up the W bank]
  6   F1*(-3,0,6)   wake (0,0,0)
```

#### 1A. The waking (x -8 to 8, z -4 to 20, y 0)
- *Shape:* the war's rear camp: a cleared patch among tents and wagons, the frozen host's cook-fires, mortal soldiers
  standing as they stood when it stopped (one mid-stride, one lifting a water skin, a boy holding a horse). Two chariot
  ruts run north out of it into the sunken road.
- *On waking:* the bubble. F1's pot 3 m to the left, its flame frozen like painted cloth until the thaw lights it. A
  banner beside the player swings slowly in the thaw; a mortal soldier lowers a cup at the slow. **Held shot 1.**
- *Teaching, no enemies:* walking north, the player watches the colour drain behind and rise ahead, and meets a grey
  wick standing at the road's mouth at (1, 0, 22) whose crack glows ember-orange from 8 m away (the stir). It is a
  **dummy**: a wick of the Fire host already slain in the war, standing dead; when the bubble reaches it, it does not
  wake but falls at the slow, its fire going out. The first stir is shown with nothing at stake. (The player meets a
  live one 12 m later.)
- *The sponsor speaks on waking* (its words are the owner's; a proposal: "Walk. It moves only near you.").
- *Kit:* new (the Indian host's camp: tents, wagons, pots on three stones, mortal soldiers, horses).
- *S1 comes down here:* the ladder at (-8, 0, 15), 9 m from F1.

#### 1B. The gilded rank: the Fire host (x -6 to 6, z 20 to 66, the road y 0, banks y 3)
- *Shape:* a dry watercourse the war fought along, 12 m wide, between banks 3 m high (Fire's Bank, B2 of
  `fire_levels.md`, leaning back 12 degrees). On both bank tops the Fire host stands in square ranks of gilded
  shields, grey, facing north (scenery; the first rank has hulls). At the north end the **levy ramp** climbs out of the
  road up the west half (x -3.5 to 3.5, from z 44 at y 0 to z 66 at y 3: 22 m, 7.8 degrees). A narrow cut stair climbs
  the east bank at z 40 (x 6 to 8, 3 m over 5 m) to the east bank top, which runs north to the ramp head.
- *Sees on entering:* A1 hanging across the road; beyond it a wick's ember glow.
- *A1:* three flat arrows at 1.4 m at (-3, 1.4, 28), (0, 1.4, 28.5), (3, 1.4, 28), flying west. Each trembles and flies
  as the bubble reaches it and sticks in the west bank. Shown in turn as the player walks; nothing else in the road
  moves yet.
- *Enemies:*
  - **E1.1 two wicks** at (3, 0, 34) and (-2, 0, 38), flames standing still. **Five oil jars** along the east bank's
    foot between z 30 and 44. Their own mechanic (`fire_levels.md` 2B): the grab, and fire spreading to jars. The
    thaw's addition: **fire spreads only inside the thaw**. A jar lit and left (the player walks on) freezes mid-burst,
    a ball of fire hanging over its stain, and goes off when the player comes back near it: fire you start waits for
    you.
  - **The hanging burst** at (4, 0, 40): an ashen of the Fire host frozen in the first instant of its burst, a fireball
    2 m across over a crouching body. It counts as a thing in flight (the table in 1.2): thawed, it trembles 0.5 s and
    bursts (Fire's ring, 3 m). A wick drawn into it burns. A player who keeps to the west foot (x -4) passes 8 m from
    it and never sets it off.
  - **E1.2 the shield wall**: three gilded levies locked at the ramp's head, (-2.4, 3, 63), (0, 3, 63.4),
    (2.4, 3, 63), frozen mid-step down it. Their own mechanic (`fire_levels.md` C5): locked, they advance in step; from
    the front only a heavy blow opens them. Three answers, as on Fire's Levy Ramp, of which the third is this land's:
    - break them from the front;
    - let them come down into the road (they follow straight at the player) and circle them there;
    - **freeze them on the ramp**: back away down the road faster than they advance (2.5 m/s in step); after 4 s
      outside the thaw they freeze mid-ramp, locked and facing south. Then climb the east bank's stair, walk the east
      bank top north, and come to the ramp head **behind** them: the bubble wakes them back to front, and the lock turns
      slowly (Fire's wheel). All three answers are visible from the road.
- *Shade, heat:* none. This land has no swell; its clock is the player's own walk.
- *Lore:* the levies' shields carry the sun disc scratched out and a crescent cut over it: the Fire host marched under
  the eclipse (Proposal, a small mark).
- *Kit reused:* Fire's `Bank` and `Ramp` (B2, B3), `ShieldWall`, the wick, the ashen's burst (`AshenBurst`), oil jars
  (B9), gilded shields.
- *Shortcut S1:* from the ramp head, the **outer lane** runs south along the west bank top (x -14 to -9, y 3), a strip
  cleared between the Fire host's rear ranks and the camp's palisade, with no bearers. At its south end, over 1A, a
  siege ladder stands frozen mid-fall at (-9, 3, 16), leaning out over the bank's face with its foot in the air. Its
  prop, a frozen soldier's hand on its rope, is on the bank top: struck in the thaw, the ladder finishes its swing and
  its hooks catch the lip, a ladder from 1A to the bank top (`Ladder`, B6). From below, its prop is 3 m up and out of
  reach.

#### 1C. The tide line: the Water host (x -2 to 12, z 66 to 112, y 3)
- *Shape:* a plateau 14 m wide, the war's line along the edge of the Water host. Along its west side, x -8 to -4 from
  z 72 to 108, stands **the wave**: a wall of seawater 5 m high frozen mid-break, its crest leaning 2 m east over a
  kerb of wet rubble (x -4 to -2, 0.6 m high, not walkable), grey-green with foam held in it. In it, frozen, the Water
  host's canoes tossed on the crest (Water's canoe kit), and drowned figures inside the water. East of the path, the
  mortal ranks the wave was breaking on.
- *Sees on entering:* off the ramp head, the wave's foot running north on the left, a harpooner's line running into it,
  arrows hanging over the right.
- *The wave in the thaw:* the part of the crest within 5 m of the player falls at the slow (1.6 s, its shadow and a
  line of foam marking the floor first) and crashes onto the path's west 3 m (x -2 to 1): 10 damage and 2 m of
  knockback east, and a strip of foam that lasts 4 s. Then that part of the wave stands frozen again as a broken
  splash. **The wave crashes only as far as the player walks beside it.** At the east line (x 9 or more) it never moves.
- *A2:* a thin flat volley over the east half, x 4 to 12, z 84 to 96: arrows at 1.2 to 1.8 m every 1.5 m, flying west
  (shot at the wave). Thawed, they cross the east half; those that reach the wave stick in its water.
- *The choice:* the **east line** is the arrows'; the **west line** is the wave's and the drowned's; the middle meets
  some of both. Each line shows its price before it is paid: the stubble along the east, the foam stain along the west.
- *Enemies:*
  - **E1.3 a harpooner** of the hooked at (2, 3, 92), its line taut west into the wave toward a puller that is frozen.
    Its own mechanic (`water_levels.md` 1.5): yanked along the line, a 12 m harpoon thrown and reeled back (dodge twice);
    cut the line within 2 m of its back and it kneels. The thaw's addition: its line runs to the frozen wave, so every
    yank drags it **west, toward the wave**, and a player who fights it near the wave sets the crest falling on both.
  - **E1.4 a drowned** frozen inside the wave at (-5, 3, 100). It comes out only when the crest falls within 5 m of it,
    riding the foam, and grabs from the foam strip (its ring of foam round the player's feet, 0.9 s, red) for the 4 s the
    foam lasts; out of the foam it crawls (Water's stranded drowned). The east line never meets it.
- *Kit reused:* Water's canoes, foam and wet-stone materials, the hooked, the drowned.

#### 1D. The bronze road: the Metal host (x -14 to 10, z 112 to 164, y 3)
- *Shape:* the Metal host laid rails across the field to bring its engines: three bronze rails cross the path on the
  diagonal and the floor is slag and ash between them (Metal's floor). On the west, a crucible on a wagon tips toward
  the path, frozen mid-pour: an arc of bronze in the air from (-10, 5, 124) to the ground at (-6, 3, 126). On the east,
  x 10 to 16, a **line of war elephants** of this land's mortal host stands frozen mid-charge, a wall the player cannot
  pass; one rears over the path at (8, 3, 138) with its forefeet 3 m up over x 5 to 9. On the west at (-16, 3, 150) a
  **siege tower** with a ladder on its south face and an open top at y 9 (a 3 by 3 m platform, rail 0.6 m). North, the
  **rampart**: an earthwork across the field at z 158 to 164, top y 6, its gate a passage at x -3 to 3 on the floor
  (y 3).
- *The ruts turn here:* the chariot ruts from 1A turn east at z 140 and run through a gap at the north end of the
  elephant line, which the elephants have closed again. From here on the floor carries **a man's footprints**.
- *The crucible in the thaw:* the pour finishes landing (1.6 s, a glow on the floor where it will land) and spreads a
  pool 3 m across that burns for 5 s (Metal's slag wedge, as the wheeler's *tip*); the wagon finishes tipping. Out of
  the thaw the pool freezes grey, and is hot again when the player returns to it.
- *The rearing elephant:* thawed, its forefeet come down at the slow onto the two ovals already cracked in the floor
  (Metal's footprint tell, `metal_levels.md` 1.4): 40 and a knockdown under them. The player walks under it to pass on
  the east line, and must be through in 1.6 s or step back.
- *Enemies:*
  - **E1.5 a wheeler and a mender** (`metal_levels.md` 11): the wheeler frozen mid-run, legs tucked, on the rail at
    z 132; the mender beside a wreck at (6, 3, 128). Their mechanics: the run down the rail (a red lane the rail's
    width), the mender rebuilding a wreck in 4 s, so break the repairer first. The thaw's addition: the wheeler's run
    ends where it leaves the thaw, frozen on the rail mid-roll, and finishes the run when the bubble comes back to it.
  - **E1.6 a chained** at its cart on the rail at (-8, 3, 146); its 6 m circle is scraped in the ash. Its mechanic:
    reach is the circle; it hauls the cart along the rail. The path passes the circle's east edge: on the east line
    (x 4) the chained is 12 m off and stays frozen, but the east line passes under the rearing elephant. On the west line
    the circle and the crucible.
- *V2* on the siege tower's top (y 9, by its ladder): the ruts through the elephant line, and beyond, on a low mound at
  (34, 3, 140), a chariot standing alone with its banner (SP1).
- *F2* at (5, 3, 156), inside the rampart gate's south mouth. *S2* comes out at the postern (-20, 3, 162).
- *Kit reused:* Metal's rails, wheeler, mender, chained and cart, crucible and slag materials, footprint ovals.

### 3.2 District 2: the middle of the field (z 164 to 300)

**The idea.** The heart of the war, where the hosts of the third tier met, and the hill where the old commander lies.
**Levels**: winter's edge (y 3), the clay square (y 5), Bhishma's hill (y 8).

```
 z     x: -24   -12    0     8    16   24
300   ====== hill's north edge; S3 lane comes in W (-18,8,296) ======
286   V3(-12) bed of arrows (-6,8,282)  F3*(-2,8,286)
270   ==== hill top y8 ====
258   ramp y5 -> 8 (x -2..6)   vessel (12,6,262), frozen cloud over the slope (10,6,260)
246   A4 the thousandfold arrow, centre (4,5,246)       [the trial E: the Chakravyuha, x 30..74]
230   formation marching E:  xbow(-9) officer(-6) shields(-2)
212   ==== terrace face 2 m, stair x -2..4 ====
196   DEEP SNOW x -16..-2   A3 rain     | track x 10..14 (hard)
186   (b)buried (-9)                    | pack: runners (2,184) (10,186), howler on a drift (8,4.5,194)
          brittle among the frozen spearmen knot (4..8, 186..190)
166   [S2 postern (-20,3,162), barred S side]  gate x -3..3
```

#### 2A. Winter's edge: the Ice host (x -16 to 14, z 164 to 212, y 3)
- *Shape:* north of the gate the Ice host brought its winter. Snow hangs in the air, every flake still; drifts lie
  across the field. The west two thirds, x -16 to 4, is **deep snow** (Ice's measures: 4.5 m/s, rolls 3.5 m, every
  step a noise); along the east a **trodden track** of hard ground, x 10 to 14, swings wide round a knot of frozen
  mortal spearmen at x 4 to 8, z 184 to 190, whom the pack was hunting when it stopped.
- *Snow in the thaw:* the hanging flakes fall at the slow round the player; outside, they hang. The thaw is never so
  plain as here: a moving cylinder of falling snow in a still storm.
- *A3:* falling arrows hanging 3 to 6 m up over the deep snow, x -16 to -2, z 176 to 196, their stubble a band across the
  snow. Thawed, they fall in the bubble (1.6 s); **a missed arrow is a noise** (`ice_levels.md` 1.4: an arrow strike is
  heard to 10 m, and the owner chose noise drawn as rings), and the buried go to it.
- *The choice:* the **snow** is slow and rained on, with the buried under it; the **track** is quick and hard but
  passes the pack's arc and the frozen knot with brittle in it.
- *Enemies:*
  - **E2.1 a buried** under the deep snow at (-9, 3, 186), its frozen line of disturbed snow pointing at the A3 strip.
    Its own mechanic: it moves toward the last noise, bulges red and grabs. The thaw's addition: noise wakes nothing
    outside the thaw; inside it, the buried go to every missed arrow. Walk slowly and few arrows fall.
  - **E2.2 the pack**: two runners frozen mid-run in an arc, (2, 3, 184) and (10, 3, 186), and a howler on a drift at
    (8, 4.5, 194). Their own mechanic (`ice_levels.md` 1.5): slots on a ring round the player, one attack at a time, the
    howl's pincer. The track at x 12 passes 2 m from the east runner and 4 m from the howler: on the track, the arc's
    **end** wakes first, then the howler. A player who wakes the east runner alone, from the track's south end, fights
    it before the howler stirs.
  - **E2.3 two brittle** standing among the frozen spearmen, at (5, 3, 186) and (7, 3, 189), looking like them in grey.
    Told apart only by the stir (frost breath at 8 m). Their mechanic: a heavy blow shatters one, shrapnel 3 m,
    breaking any brittle within 2 m. Shrapnel is a bearer's: full speed. A shatter beside the pack hurts the pack.
- *Kit reused:* Ice's deep snow, drifts, noise rings, the pack, brittle, buried.
- *Shortcut S2:* along the field's west edge (x -24 to -18) a lane of hard ground runs from the clay terrace's
  south-west stair (-22, 5, 212 down to y 3) south to the rampart's **postern** at (-20, 3, 162), a door barred on its
  south side: from the north, lift the bar (`BarredDoor`, B7); F2 is 25 m east. From F2, the clay square without
  winter's edge.

#### 2B. The clay square: the Poison host (x -24 to 16, z 212 to 258, y 5)
- *Shape:* a terrace 2 m above winter's edge (its face of packed earth faces south), climbed by a stair at x -2 to 4,
  z 208 to 212. On it, the Poison host's clay army in its ranks, most of it grey mortal scenery round the edges, and one
  **formation** frozen marching east across the middle. Over the north half hangs **A4, the thousandfold arrow**.
  The north edge climbs to Bhishma's hill by a ramp (x -2 to 6, z 258 to 280, y 5 to 8, 7.8 degrees) whose east side is
  a slope down which the vessels' clouds flow.
- *Sees on entering:* from the stair head, the formation's shields 16 m north and the officer's drum behind them; the
  ringed floor of A4 beyond.
- *Enemies:*
  - **E2.4 the formation**, marching east along z 230: two ji-bearers locked at the front, (-2, 5, 228) and
    (-2, 5, 230.5); the officer with the drum at (-6, 5, 229); two crossbowmen, kneeling and standing, at (-9, 5, 228)
    and (-9, 5, 231). Their own mechanic (`poison_levels.md` 1.6): the lock, the wheel at 30 degrees a second, the
    officer closing any gap within 4 s, the volley comb (lanes 0.5 m every 1.6 m), fired only from on screen; break the
    officer and they dissolve.
    The thaw's addition, and the lesson of this room: **a formation wakes only where the thaw touches it.** The
    formation is 7 m deep. Come at its front and the shields wake, while the officer and the crossbowmen behind stay
    frozen: break a shield-bearer and **the gap stays open**, because the officer who would close it is grey. Come at
    its rear (round the west, x -16) and the crossbowmen wake first, at their weakest range. Come at its middle and all
    of it wakes at once.
  - **A4 over its route.** The formation marches east under the thousandfold arrow's cone. Its arrows hurt anything
    they fall on: a formation lured under a thawed patch of the cone is struck through its shields from above. The
    falling arrows land on their dust rings (1.3), 10 damage each to the player.
  - **E2.5 a vessel** at a vat on the slope at (12, 6, 262), and beside it the cloud of a second, frozen mid-burst over
    the slope at (10, 6, 260). Their mechanic (`poison_levels.md` 1.6): within 2.5 m for 0.6 s a vessel swells and
    bursts; **the cloud is heavy and flows downhill**. The thaw's addition: the frozen cloud flows only while thawed,
    downhill across the ramp toward the square, 2 m a second, so a player climbing the ramp's east side brings it down on
    themselves. The west side of the ramp keeps 6 m from it.
- *Kit reused:* the clay soldiers and their formation code, the vessel, the cloud, the bronze vat, the clay army's
  painted-flake materials.

#### 2C. Bhishma's hill (x -14 to 10, z 266 to 300, y 8)
- *Shape:* a low knoll on the field, the frozen war thinning round it: here the fighting stopped before the freeze, as if
  the armies drew back from the old man. No bearers within 20 m. The bed of arrows at (-6, 8, 282): an old man lying
  above the ground on the shafts that went through him, his head held up on three arrows set as a pillow. His
  attendants' cook-fire, F3, at (-2, 8, 286).
- *The figure* (`LOOKS.md` 10, Proposal: grey but moving; colour returns when the thaw reaches him): his chest rises, his
  eyes follow the player from the moment he is in frame, his beard stirs in a wind no one else feels. When the bubble
  reaches him, white cloth and a single marigold garland come back. The arrows under him stay frozen even in the thaw,
  because he holds them there (`LOOKS.md`).
- *What he is* (Proposal, a nod to Bhishma, who lay on a bed of arrows and chose the hour of his death, waiting for the
  sun to turn north): the old commander of this land's mortal army, felled by the champion's own arrows before the
  freeze, who chose not to die until the sun moves again. He is the only thing in the land fully awake (Proposal in
  `WORLD.md`), and the witness of the champion's rebellion. He speaks in click-through lines (text the player reads at
  their own pace, `CLAUDE.md`). His name is the owner's; a nod, never Bhishma's own. Draft lines, in the land's grim and
  sparse voice (`docs/GAME.md`), for the owner to set:
  - "You move. Then it has found another."
  - "I chose to wait. Time did not ask me."
  - "He put these into me himself. Then he went up the hill to the seats."
  - "He came back down with both of them in him. He wept like a boy."
  - "Then he turned on the voice in his head. Out there, to the west. The ground still keeps the hour."
  - "I will die when the sun moves. Not before."
- *F3* at the attendants' fire. *Held shot 2* plays once, on lighting it. *V3* at the hill's west edge looks west to
  the Stutter (SP2).
- *Kit:* new (the bed of arrows and the figure).

### 3.3 District 3: the far ranks (z 300 to 404)

**The idea.** The last tier's hosts, the storm and the stones, and then the war's front line before the council,
where the champion's own volley hangs. **Levels**: the ravine and the Stutter's crater (y 4), the road and the moor
(y 8), the crest (y 11).

```
 z     x: -24   -14   -6    0    6    14
404   ======== CREST y11 ======== held shot 3
392   tortoises (-3,9,388) (3,10,394) (-2,10.5,400)   A6: five lanes x -4,-2,0,2,4, arrows flying S
380   ==== the last rise, y8 -> 11 ====   V4 (0,8,382)
376   (K)(-8) (K)(2,380) (K)(10,374)
370   stones . . . . . . (row 2)
366   (G) Green Man (4,8,366)            <- the frozen Hunt riding W, band z 352..378
360   stones . . . . . . (row 1)
346   ==== A5 the canopy z 336..346 at 4.5 m ====
338   (t)tengu (-3)          (f)fox pair (7)
322   bolt(-4,320)   (s)samurai (1,8,322)   bolt(6,326)
308   bolt(-4,308)   bolt(6,314)
300   [S3: ravine W, banyan at (-16,8,342); lane x -24..-20 S to (-18,8,296)]
      [gully W from (-6,8,312) -> SP2]
```

#### 3A. The storm's file: the Lightning and Wind host (x -6 to 8, z 300 to 346, y 8)
- *Shape:* a road 14 m wide between the storm host's ranks: samurai in lacquered armour, crow-tengu with fans, grey,
  and the two rivals' messengers frozen mid-race along the road's edges. Along the road stand **frozen bolts**:
  lightning caught mid-strike, a jagged column from the sky to a scorch ring on the floor, grey, each flickering cyan
  at 8 m (the stir). Five of them, staggered: (-4, 8, 308), (6, 8, 314), (-4, 8, 320), (6, 8, 326), (-4, 8, 332).
- *Bolts in the thaw:* tremble 0.5 s, then the strike: a ring 2 m round the scorch, 35 damage. Then the bolt is gone
  and its scorch ring stays. The bolt is the shared strike piece with its own shape and numbers, a ring because this
  is not a race, and the one look of lightning, the cyan fork of `LOOKS.md` 8 (`kit_list.md` Q5, Decided 2026-11-19). Walking the road's centre line (x 1), each bolt is 5 m off as the player passes it: the
  bolts strike **one after another in time with the walk**, a drumbeat on either side, and miss. Lightning and Wind's
  own verb is rhythm (Decided); this is its echo. They are dangerous only to a player pushed sideways.
- *Enemies:*
  - **E3.1 a samurai** frozen mid-charge south down the centre line at (1, 8, 322), between the bolts at z 320 and 326:
    a duel in a lane whose sides strike. Its mechanic is the struck samurai's own (`lightning_wind_levels.md` 1.6;
    `kit_list.md` Q9): the armour takes light blows at 25% and no stagger; a bolt landing within 2 m charges it for 4 s
    (a third faster, its guarded cuts cost double stamina), then the armour bursts and it is stunned 2 s. The frozen
    bolts stand 5 m off the centre line, so the way to open it is to bring it near one as the thaw sets it off, and
    outlast the four seconds.
  - **E3.2 a fox pair** at (7, 8, 336) on the road's edge: one real guard and its fox-fire double, which casts no
    shadow (`lightning_wind_levels.md` 1.6; `kit_list.md` Q9). They stand as statues flanking a gate and wake when the
    player passes through it or strikes one, their own rule over the thaw's, as the King's Men keep theirs; then the
    four-count bell and the echo blow as in their land, and the double goes out when the real one dies. No race runs
    here, so the real guard shuts no sill; the long crescent shadows tell the two apart. The gate (one of the storm
    host's, at the road's edge) is to be placed at build.
  - **E3.3 a crow-tengu** with a feather fan at (-3, 8, 338). Its own mechanic (`lightning_wind_levels.md` 1.6;
    `kit_list.md` Q9): the swoop, a pale lane 6 m long that lands it behind the player with a fan cut, and the fan
    crescent, a wind blade of 10 m, guardable, that shoves 1 m, toward a bolt's ring or under the canopy's falling rows.
    Whether a 1 m shove is enough to matter beside the bolts is to be checked at build. Kill it first, or keep it
    between the player and the bolts.
- *A5, the canopy:* over z 336 to 346, a lattice of arrows frozen at 4.5 m, rows 1.5 m apart, its shadow a grid on the
  floor with small crescents of light in its gaps. Thawed, the rows over the bubble drop one at a time (1.6 s each, the
  row's shadow line darkening first): a preview of his canopy, dropped in rows (Proposal in `WORLD.md`).
- *SP2's way:* a gully leaves the road's west side at (-6, 8, 312), turns south and drops 2 m (facing south) into the
  Stutter's ravine.
- *Kit:* the storm host's soldiers and fans (the Lightning and Wind land's kit when made); the frozen bolt is new.

#### 3B. The stones: the Earth and Life host (x -16 to 14, z 346 to 380, y 8)
- *Shape:* a moor (heather, peat, lichen stone from Earth and Life's floor) with **two rows of standing stones** across
  the path, at z 360 and z 370, stones 1.4 m wide every 6 m, the rows staggered. Across the whole moor, a band of
  **the Wild Hunt** frozen mid-ride from east to west (z 352 to 378): riders, hounds and Carnwyn's herd hanging in the
  middle of their leaps, a churned band of hoofprints under them.
- *The Hunt in the thaw:* riders within 5 m gallop west (bearers: full speed), across the path, and freeze again beyond
  the bubble's west edge. They keep Earth and Life's pieces and rules (`earth_life_levels.md` 3.5; `kit_list.md` Q4,
  Decided 2026-11-19), scaled to the thaw. **They cannot pass stone** (Decided for their land): a rider meeting a stone
  parts round it, and the shelter is the ring of untouched heather round each stone (r 2 to 3, as for that land's
  standing stones). Caught in the open: 25 damage that cannot take the last point of health, knocked down 1 s, and
  carried west along the riders' line, as in their land; where on the moor's west side the player is set down (the S3
  ravine lies there) is to be set at build, on firm ground with a way back onto the path. Walk slowly from stone to
  stone and the riders come a few at a time, from the east edge of the bubble, 1 s of warning each; their speed is to
  be set at build to keep that second (Earth and Life's front runs at 16 m/s, which from the bubble's edge would give
  about a third of a second).
- *Enemies:*
  - **E3.4 three King's Men** frozen mid-march among the stones at (-8, 8, 376), (2, 8, 380), (10, 8, 374). Their own
    rule (Decided): they move only while the player is not facing them. The thaw's addition: in the thaw they keep
    their rule, so a King's Man is frozen twice over. The Hunt comes from the east and the King's Men stand north and
    west: watching the riders is turning away from the men.
  - **E3.5 a Green Man** rooted against a stone at (4, 8, 366). Its own mechanic (Decided): hurt, it roots and heals;
    strike it while uprooted, or cut the roots. Rooted against a stone, it is in the stone's shelter from the Hunt.
- *Kit reused:* Earth and Life's standing stones, moor floor, the Hunt's riders and herd, the King's Men, the Green Man.
- *Shortcut S3:* a ravine (y 4, 6 m wide) runs along the moor's west side. On its north lip at (-16, 8, 342) a **banyan**
  stands frozen mid-fall, felled by the haulers of the Earth host, its last uncut wood the prop. Struck in the thaw, it
  falls south across the ravine (1.6 s) and lies as a log bridge; the lane on the far lip (x -24 to -20, y 8) runs south
  to the hill's north-west corner at (-18, 8, 296), 16 m from F3. From F3, the moor without the storm's file.

#### 3C. The last rise (x -6 to 6, z 380 to 404, y 8 to 11)
- *Shape:* the front line of the war, the last ground before the council: a slope 12 m wide climbing 3 m over 24 m
  (7 degrees), between the ranks that were charging up it. The ranks thin toward the crest: nobody reached it.
- *A6, his volley:* flat arrows frozen flying **south down the rise**, toward the player, in five lanes at x -4, -2, 0,
  2 and 4, an arrow every 2 m up each lane at 1.2 to 1.8 m, each with its streak and the stubble of the lanes in the
  slope. They are his, shot from the council at the war coming up the hill. Thawed, they come down the lanes at 5 m/s,
  1.5 s from the bubble's north edge to the player.
- *Shelter:* three **tortoises**, groups of mortal shield-bearers frozen with shields locked overhead and in front, at
  (-3, 9, 388), (3, 10, 394) and (-2, 10.5, 400): arrows stop in them. Climbing from tortoise to tortoise is the slow way.
- *The reflect:* every arrow can be perfect-guarded back up its lane (1.3). Nothing to strike here: the reflected arrows
  fly up and over the crest and are gone. The rise is the place to learn the timing with nothing else at stake; a
  player who guards perfectly can walk straight up the centre lane.
- *No enemies.* Dread and the lesson, as Fire's Way of Noon.
- *V4* at the foot, (0, 8, 382), shows the lanes before the climb.
- *The crest* (0, 11, 404): **held shot 3**, the council.

---

## 4. Side paths and the trial

### 4.1 SP1: the champion's chariot (see it, then find the way)

- *Seen:* from the siege tower's top (V2), across the elephant line, on a low mound at (34, 3, 140).
- *The way:* from F2, the ramp up to the rampart's top (y 6) and east along it to x 30; at (30, 6, 158) a rope ladder
  hangs frozen at an angle down the rampart's south face; in the thaw it drops straight and can be climbed both ways
  (`Ladder`). Down it, south through the elephant line's rear (the elephants face west; their backs are a wall on the
  right), to the mound.
- *What is there:* his chariot, standing where it stopped: one wheel jammed between an elephant's frozen feet, the ruts
  ending in it. The reins are tied to the front rail. The charioteer's bench is empty, worn smooth. One wheel is cut with
  hour lines, a sundial (a nod to the wheels of the sun temple at Konark, carved as sundials, from memory); its shadow has
  not moved. His banner on its staff, a plain device of the owner's choosing (no copied temple iconography, `LOOKS.md`).
  From the chariot, his footprints lead back to the path.
- *Lore* (draft, for the owner): "Reins knotted to the rail. Nobody held them." / "The bench is worn on one side.
  Someone sat beside him a long time." / "Hours cut in the wheel. The shadow has not moved." The empty bench is the
  nod to the charioteer (`WORLD.md`) without giving the sponsor a charioteer's look: the sponsor has no body on it, only
  the place where one sat.
- *Gift:* Shards (a cache in the chariot), no weapon: the champion's weapons are his own and fixed (Decided).
- *No enemies*: a quiet minute, between the near ranks and winter.
- *Minutes:* 2 for a new player.

### 4.2 SP2: the Stutter (see it, then find the way)

- *Seen:* from Bhishma's hill (V3), far to the west: a flicker of colour and motion in the grey, the only motion outside a
  thaw anywhere in the land, which the player has learned cannot be.
- *The way:* from the storm's file, the gully west at (-6, 8, 312), a drop of 2 m south into the ravine (y 4), and west
  along it to a crater at x -70 to -50, z 290 to 310, floor y 4.
- *What is there:* the scar of the champion's fight with the sponsor (Decided: as a last rebellion he fought the sponsor
  and drove it out). Time there is broken: a ring 12 m across **repeats the same 3 seconds**, over and over, in colour.
  In it, a grey figure of the champion looses an arrow straight up at nothing; a burst of gold-white and violet-black
  splits the ground; the earth heaves; then everything runs backwards and begins again (Quantum Break's looping
  stutters: things that repeat one action forever). The player's thaw does nothing here: this is not the freeze.
- *The burst* reaches 6 m every 3 s: harmless, a shove of 2 m. A player inside it sees, for a moment, the two halves of
  the split arena on the crater floor, a glimpse of phase 2.
- *Lore:* the arrow he looses is the only arrow in the land that flies up. Draft words on a broken shield at the
  crater's edge (for the owner): "Here he heard it plainly." The sponsor, which talks often elsewhere, is silent in the
  crater; leaving it, it says one line (the owner's; a proposal: "Do not linger there.").
- *The way home:* a rope ladder up the crater's east lip to the hill's west edge, beside V3: 8 m from F3.
- *Minutes:* 2.5 for a new player.

### 4.3 The trial: the Chakravyuha

**Decided** (owner, 2026-11-19, answer 4): the land has a short trial, the Chakravyuha. The room and its rules below
are **Proposal (Claude)**; the case against a trial is kept after it for the record.

**The Chakravyuha** (Proposal). In the Mahabharata the chakravyuha is a battle formation of rings like a wheel
or a spiral; the champion's son knew how to break into it and not how to get out, and was killed inside (Abhimanyu; a
carving at Halebidu shows it as a labyrinth). Here: east of the clay square, x 30 to 74, z 220 to 264, y 5, reached by a
gap in the square's east ranks.
- *Shape:* three rings of frozen soldiers from every land, shoulder to shoulder (walls 1.5 m deep with hulls), with a
  lane 4 m wide between them winding inward as a classical labyrinth, to a heart 10 m across. 44 m across in all. From
  above the floor reads as a spiral: the lane is trampled earth, the walls are ranks.
- *The rule:* the lane is narrower than the bubble, so walking it thaws both walls. Each ring holds bearers of two or
  three lands among its grey mortals, a harder mix than the path's (outer: Fire and Water; middle: Metal and Ice;
  inner: Poison, the storm and the stones). A woken bearer **steps out of its wall** and leaves a gap. So the way in is the
  lane, and **the way out is through the gaps the player's fights made**: the tale turned, the one who goes in can come
  out if they break the rings.
- *Clinging in a maze:* bearers woken through a wall from the next lane cannot reach the player (they walk straight; the
  wall stops them, and they hold the cut-off pose of `poison_levels.md` 1.1). When the player moves on, they freeze 4 s
  later where they stood: in the lane ahead.
- *The heart:* a Memory piece (the climb's reward for a trial, Decided), beside a boy's small bow, frozen grey. Whether the
  heart is the champion's son is the owner's (it is heavy, and it is the best answer to why the champion is as he is).
- *Minutes:* 4 for a new player.

**Against one** (not taken). The land is 18 minutes before the last fight and the weight is on that fight (Decided). A trial spends a
new player's flasks and attention on the way in, and a short land has less room to hide it: the Chakravyuha's gap in the
ranks would sit on the main path's edge. Dark Souls' Kiln of the First Flame is a short ash path, a few knights, and the
end; players remember it as right for an ending, though no developer has said why it is short.

### 4.4 SP3: the empty place (see it, then find the way)

**Decided** (owner, 2026-11-19, answer 5): this is the war of the game's opening (`docs/GAME.md`, "Story and setting":
the battlefield where a mortal army's assault on the gods is destroyed and the player, one nameless soldier in it,
falls). The opening is played on this field in its last minutes. On the path the player finds **the one empty place in a
frozen rank**, where they stood when the sponsor took them out of time, as a "see it, then find the way" beside
winter's edge (2A).
- *What it holds to:* it is a gap in a rank of the mortal army, so nothing in it wakes; it shows the player's past a
  place without saying what that past was (the past stays Open in `docs/GAME.md`).
- *To be set at build:* which rank, where on the main path it is seen from, the way to it, any lore or gift, and its
  minutes.

---

## 5. The council

### 5.1 First sight and the approach (z 404 to 445)

The council is seen in four steps, held back to the last:
1. **A ring of small lights** at the top of every north-facing vista (V1, V4): colour the player did not bring.
2. **The shadow.** From the crest onward, the gnomon's frozen shadow lies on the floor as a dark band 4 m wide running
   south out of the ring, down the processional, its tip at z 440: the first thing of the council the player walks on.
3. **Held shot 3** at the crest: the whole ring from 40 m.
4. **V5 at F4**: the champion seated in the shadow between the broken seats, 17 m away.

**The processional** (x -6 to 6, z 404 to 445): three stepped ramps of 1 m over 4 m (14 degrees, walked only, never
fought on: four broad steps of 0.25 m with treads of 1 m, shallower than Fire's stairs so the climb reads as a
procession, not a choke), at z 410 to 414 (y 11 to 12), z 420 to 424 (12 to 13) and z 432 to 436 (13 to 14), with landings 6 to 8 m
deep. The frozen war stops at the crest: the processional is bare stone, swept, a ring of fallen soldiers of every land
lying at its foot where his arrows met them. No enemies. F4 on the second landing at (6, 13, 428).

**The line** is the tip of Time's shadow at z 440. Crossing it is crossing into the champion's ground (as the gold line
at Fire's bowl): he rises.

### 5.2 The ring (centre (0, 14, 470))

```
 z     x: -25    -12.5     0     12.5    25
495                     12 Poison
492           11 Ice              1 Lightning
482   10 Metal          gnomon high end (22 m)    2 Wind
478                     |  |
470   9 Water        ( hour channel r12 )        3 Earth
462                     |  | low end (2 m)
457   8 Fire                                     4 Life
448        7 DARK (ruined)          5 LIGHT (ruined)
445                  6 Time's bare place: THE CHAMPION
440   ------------------ tip of the shadow ------------------
```

- **The floor:** a disc 44 m across (radius 22) at y 14, inlaid as a mandala in pale stone: concentric bands, the twelve
  hour lines from the centre to each place, lotus petals at the hour channel. A parapet 0.6 m high runs round the rim
  between the seats; the south opening, between Light's and Dark's seats, is the only way in.
- **The twelve places** (Proposal in `WORLD.md`: a mandala of twelve seats facing outward like a temple's ring of
  planetary shrines; a clock face with Time as the twelfth). Each place is 30 degrees round a circle of radius 25, just
  outside the floor's rim. **The twelfth place, Time's, is the south opening itself** (6 o'clock; Decided, owner,
  2026-11-19, answer 7): a bare stone 3 m
  square and 0.6 m high with nothing built on it, because Time ruled nothing and only watched (Proposal, "The story
  behind it"); it sat at the centre instead, where the gnomon stands. **Light and Dark**, Time's siblings (Decided), sit
  either side of its place, at 5 and 7 o'clock, in front (Decided), ruined and burnt grey by the breaking. The other
  nine go round in the order of the climb: Fire 8, Water 9, Metal 10, Ice 11, Poison 12, Lightning 1, Wind 2, Earth 3,
  Life 4 (so the two duo lands' seats stand side by side).
- **The seats:** small stepped shrine-towers, 5 m square at the base and 9 m tall, each facing outward toward its land
  (so from inside the ring the player sees their backs and the light of their element spilling round them). No two face
  each other, as the nine planetary deities are set in a temple so that no two meet each other's gaze. Each is in its
  element's colour and substance (Decided): Fire's smoulders, Water's runs, Metal's glows at its seams, Ice's frosts,
  Poison's breathes its mist, Lightning's flickers, Wind's streams dust, Earth's is ochre stone, Life's is overgrown.
  `LOOKS.md` proposes that the seats of elements taken glow brighter and those consumed are dull. The seats fade when
  within 4 m south of the player (the south-side rule): from the camera, those in the south half of the ring are seen
  through.
- **The gnomon, Time's seat** (Proposal in `WORLD.md`: the gnomon at the centre whose shadow lies across the floor like a
  clock hand). A stair-sundial in the manner of the Samrat Yantra at Jaipur, whose gnomon is a staircase 27 m high rising
  toward the pole: here a stepped wall 4 m wide from z 462 (2 m high) to z 478 (22 m high), its steps too steep to climb
  (51 degrees), its sides cut with hour scales. With the crescent at 30 degrees in the north, its high end throws a shadow
  38 m long: out of the ring by Time's place, over the stone where the champion sits, to the tip at z 440. Indian
  instrument, not a column: it keeps clear of Olympus.
- **The hour channel:** a ring of shallow water 1.2 m wide and 0.1 m deep at radius 12 round the gnomon, hour marks cut in
  its rim. Walkable. It is the pool his reflection shot needs (Proposal in `WORLD.md`).
- **Beyond the ring** the plateau drops 6 m on every side to the frozen war, which surrounds the council in grey to the
  edge of sight (`LOOKS.md` frame `ch_council`). The council is not frozen: its seats hold their elements, so the
  ring moves and has colour of its own, the first place in the land that does (`LOOKS.md` 10).

### 5.3 How the room plays with his phase 1 (the moves of `WORLD.md`)

- **The thousandfold arrow:** the floor's bands and hour lines give the player a grid to read the rings and gaps against;
  the player has walked through it frozen at A4.
- **The chakram** (Decided: thrown out, it multiplies, flies round the area and bounces): the ring of seats and the
  parapets between them are its bumpers. Eleven seats and the gnomon make its bounces readable: it strikes stone, sparks
  and turns.
- **The arrow canopy:** its shadow grid on the mandala, its small crescents in the gaps (A5's rehearsal).
- **The reflection shot:** he turns his back at the hour channel and aims at the player's reflection in it.
- **The hunter's double** appears at the seat opposite him across the gnomon, which blocks one line of the crossfire: the
  gnomon is cover from one shooter at a time.
- **The gnomon** is the room's pillar, as the barque shrine was the warden's in Fire: arrows stop on it, the chakram
  turns off it, and circling it is always a choice of which side to come out.
- **The phase change** (Decided): he throws the chakram at the crescent in the north; it covers it; totality. The
  gnomon's shadow goes out with the sun, the only time in the land it moves.

### 5.4 The domain, phase 2 and phase 3 (layout only)

- **Phase 2** (Decided: the split arena; Proposal: its line sweeps like a clock hand). The line starts along the
  gnomon's north-south axis: **day on the east** (Light's side, its seat at 5 o'clock) and **night on the west** (Dark's,
  at 7), and sweeps round the gnomon. The gnomon is the central pillar of the churning of the ocean (Proposal in
  `WORLD.md`: the beasts haul a serpent wrapped round a central pillar). The hour channel stays, lit in the night half,
  sun-struck in the day half. The ring of seats stays visible past both halves (`LOOKS.md`).
- **Phase 3** (Decided): the eagle holds the chakram in front of the sun, which is north at 30 degrees, so the
  player reflects his arrows toward the top of the screen (1.1, item 4). When the chakram cracks, the sun comes back as
  full daylight over the council for the first time, and the gnomon's shadow lies again. The war beyond stays grey: the
  freeze is the sponsor's (`LOOKS.md`).
- **The end** (Decided): he falls off the bull; the beasts die and leave; he speaks a few words. The choice is made over
  his body on the mandala, in the gnomon's shadow. There is no ride out.
- *His ground:* `HoldGround` on the disc; the line at z 440.

---

## 6. Shortcuts and loops

| | Opens from | To | How | Saves |
|---|---|---|---|---|
| S1 | The west bank's top over 1A, (-9, 3, 16), reached along the outer lane from the ramp head | 1A, (-8, 0, 15), 9 m from F1 | A siege ladder let fall: its prop struck from the bank top (1.4) | F1 to the tide line without the Fire host |
| S2 | Winter's edge's west lane, the rampart postern's north side, (-20, 3, 163) | The postern's south side, 25 m from F2 | A door barred on the south side (`BarredDoor`) | F2 to the clay square without winter's edge |
| S3 | The banyan on the west ravine's north lip, (-16, 8, 342) | The lane down the ravine's far lip to (-18, 8, 296), 16 m from F3 | A tree let fall across the ravine | F3 to the moor without the storm's file |
| Loop | Bhishma's hill, west | The Stutter's crater, home by its rope ladder | Walking | SP2 is a loop from F3 back to F3 |
| Loop | F2, the rampart's top | The chariot's mound, home by the same ladder | Walking | |

All three stay open after a fall and across sittings (a progress key each, as `LandVoice` keeps).

---

## 7. Build list

New pieces are made in code by `StoppedWarBuilder` as Fire's are: static meshes with `MeshCollider`s, ink outlines where
the toon material carries one. Sizes are the plan's defaults. Reuse comes first: most of each section is another land's
kit and another land's enemy code, placed frozen.

### 7.1 New geometry and effects

| | Piece | What it is | Parameters |
|---|---|---|---|
| Z1 | **The thaw look** | Colour and motion by distance from the player: every material in the scene blends to warm grey (a faded photograph, never pure greyscale, `LOOKS.md`) beyond the radius, across the 1 m band, through a global shader property (centre, radius, band) read by the land's toon materials. Per-material is cheaper and sharper than a full-screen pass, and stays inside URP. The three fires' rings and the Stutter are extra centres | Radius 5, band 1; up to 4 extra centres of 3 m; the council and the bed of arrows flagged as always coloured |
| Z2 | **Frozen ranks** | The mortal war as instanced static figures in posed meshes (no animators), in their land's silhouettes; the first two ranks along the path get simple hulls; inside the thaw the nearest swap to posed animated figures playing their last second at the slow | Every section's edges; the processional's foot |
| Z3 | **Volleys** | A volley is data: arrow positions, headings, kind (flat or falling), each arrow instanced with its dust streak; the stubble strip drawn on the floor from the same data | A1 to A6 (1.3) |
| Z4 | **Frozen falls** | One component for anything frozen mid-event: a pose, a trembling clip, a 1.6 s completion clip played at the slow when thawed, a floor mark from the tremble, a hurt shape at the landing, and whether it stays done (a progress key) or resets at a rest | The hanging burst, the wave's crest, the crucible's pour, the rearing elephant, the frozen bolts, the canopy's rows, the thousandfold arrow's patches, the vessel's frozen cloud |
| Z5 | **Let it fall** | Z4 with a prop to strike and a stays-done key | S1 the siege ladder, S3 the banyan |
| Z6 | **The wave** | A breaking wall of water 5 m high, frozen, its crest made of segments 2 m long that fall separately (Z4), a foam strip 4 s; the canoes and drowned in it | 1C |
| Z7 | **The rest fire** | A pot on three stones whose flame stands still until thawed; lit, it holds a 3 m centre of Z1 | F1 to F4 |
| Z8 | **The bed of arrows** | The figure (breathing, head and eyes tracking, beard in its own wind), the bed of shafts, the marigold garland | 2C |
| Z9 | **The Stutter** | A ring 12 m across that plays a 3 s clip forward and backward forever, coloured, outside the freeze | SP2 |
| Z10 | **The council** | The plateau and its 6 m faces; the mandala disc 44 m with its parapet; eleven stepped seats and Time's bare stone; the gnomon; the hour channel; the processional | Section 5 |
| Z11 | **The domain** | The phase 2 floor and light swap on the same disc: a hard line through the centre, day east and night west at start, turning | 5.4; `LOOKS.md` frame `ch_day_night` |
| Z12 | **Land framing and vistas** | The land-wide camera framing (1.15) and the vistas and held shots with a direction | 1.1 item 1; V1 to V5; three held shots |
| Z13 | **The Chakravyuha** (Decided, answer 4) | Three rings of frozen soldiers with hulls, the lane, the heart | 4.3 |

### 7.2 New behaviour the rooms rely on (for the code worker)

| | What | Where it is used |
|---|---|---|
| C1 | **ThawField**: the player's centre and radius; classes (player's things, bearer, mortal, flight or fall, fire); per object a local time scale of 0 (frozen), 0.1 (the slow) or 1; the bearer's wake (1.2 s), re-wake (0.4 s) and cling (4 s); the small thaw at a strike of the player's (1.5 m, 2 s). **Every enemy behaviour must read its own local time**, not `Time.deltaTime`: this touches every enemy script in the game and is the land's main engineering cost. Check `Enemy` for a time scale hook before writing it | Everywhere |
| C2 | **The stir**: bearers within 8 m show their element's thread (each land's own effect at low strength) | Everywhere |
| C3 | **Volley flight**: tremble, 5 m/s along the heading, stick or refreeze; 12 damage and a flinch; guardable; the perfect guard sends it back at full speed as the player's; held tremble so no arrow lands within 1.2 s of a red attack | A1 to A6 |
| C4 | **Frozen falls and let it fall** (Z4, Z5) | 1B, 1C, 1D, 2B, 3A, S1, S3 |
| C5 | **Each land's people, unchanged, started frozen**: wick, gilded levy and `ShieldWall`, `AshenBurst`; hooked harpooner, drowned; wheeler, mender, chained; pack runner and howler, brittle, buried; clay ji-bearer, crossbowman, officer, vessel; struck samurai, fox pair and its double, crow-tengu with a fan (as `lightning_wind_levels.md` 1.6); King's Man, Green Man; the Hunt's riders (a crossing band that stones block, with Earth and Life's heather rings, damage and carry). Each one's code from its land; this land adds only C1's time scale and C2 | Sections 3.1 to 3.3 |
| C6 | **The formation woken in part**: the officer's gap-closing and the lock work only among members awake; a frozen officer closes nothing | 2B |
| C7 | **Noise from arrows** (Ice's noise rings) heard by the buried only inside the thaw | 2A |
| C8 | **The fall resets**: on a fall, every bearer to its first pose and frozen, every volley and frozen fall reset, let-it-fall keys kept | Everywhere |
| C9 | **The figure and his words**: click-through lines, head tracking | 2C |
| C10 | **The champion's ground**: the line at z 440, `HoldGround` on the disc | Section 5 |

### 7.3 What is reused

| Piece or code | Where |
|---|---|
| Fire's `Plateau`, `Bank`, `Ramp`, `Stairs` (B1 to B3) | The sunken road and its banks, the levy ramp, the terraces, the last rise, the processional |
| `Ladder` | S1, the siege tower, SP1's and SP2's rope ladders |
| `BarredDoor` | S2 |
| `RestPoint` | F1 to F4, as cook-fires |
| `LandZones`, `TopDownCamera.SetFraming` | The land's framing, V1 to V5, the held shots |
| `Fadeable`, `OcclusionFader` | Siege tower, elephants, the wave, the seats, the gnomon, hanging things above 2.5 m |
| `LandVoice` progress keys | S1 to S3, SP1's ladder |
| `FallCatch` | Under the scene |
| `Enemy.HoldGround` | The champion |
| Each land's enemy code and kit | Table in C5 and 1.6: Fire's wicks, levies, oil jars and shields; Water's hooked, drowned and canoes; Metal's rails, automata, chained, cart, crucible and footprint ovals; Ice's deep snow, drifts, noise rings and people; Poison's clay, formation, vessel and vat; the storm's and the stones' people and kit when made |

### 7.4 New kit (Blender, through the `blender-prop` skill)

This land's own: the Indian mortal host (foot soldiers with round shields and bows, archers, standard-bearers),
chariots and their horses, war elephants (standing, charging, rearing), siege towers and ladders, tents, wagons, pots on
three stones, banners in saffron and vermilion and marigold garlands (seen in colour only in the thaw), the hanging arrow
with its streak and the landed-arrow stubble, the tortoise of shields, the frozen bolt, the banyan, the bed of arrows and
its figure, the champion's chariot, the eleven seats and Time's bare stone, the gnomon, the mandala floor, the hour
channel. The other lands' people and props are their own kit.

### 7.5 Performance

The land is the first with thousands of figures on screen. They must be static, instanced and without animators outside
the thaw (Z2); only the bubble's few dozen move. Profile the densest frame (2B, the formation under A4 with the clay
ranks round the square) with `diagnostics profiler_record` before the land is called done, as `CLAUDE.md` requires.

---

## 8. Pacing

Minutes for a new player on a clean pass, counted as `fire_land.md` 3.1 counts them (a weak enemy about 30 s, a strong
one 45 s, a group a little less each), with the volleys and frozen falls counted where they slow the walk.

| Stretch | Minutes (new) | Experienced | Tension | What it teaches |
|---|---|---|---|---|
| The waking, F1 | 1.0 | 0.4 | Wonder | **The thaw**: colour, the slow, the stir on a dead wick |
| The gilded rank | 2.0 | 0.8 | Rising | A bearer waking; A1; fire that waits; freezing a shield wall mid-ramp |
| The tide line | 1.8 | 0.7 | Rising | Choosing a line: the wave's or the arrows'; a fall marked on the floor |
| The bronze road, F2 | 2.2 | 0.9 | Peak | A frozen pour; a foot overhead; the chained left frozen by the line taken |
| **District 1** | **7.0** | **2.8** | | |
| Winter's edge | 2.2 | 0.9 | Peak | The snow or the track; arrows that make noise; the end of a pack's arc |
| The clay square | 2.3 | 1.0 | Peak | Waking part of a formation; the thousandfold arrow, frozen |
| Bhishma's hill, F3 | 1.5 | 0.5 | Rest | The story; what the player leaves behind |
| **District 2** | **6.0** | **2.4** | | |
| The storm's file | 1.8 | 0.7 | Rising | Bolts in rhythm with the walk; the canopy in rows; a gust into danger |
| The stones | 1.8 | 0.7 | Peak | The Hunt from stone to stone; the King's Men behind the player's back |
| The last rise | 1.0 | 0.5 | Dread | **The reflect**, with nothing else at stake |
| The crest, the processional, F4 | 0.7 | 0.3 | Awe, then dread | The council; Time's shadow; the line |
| **District 3** | **5.3** | **2.2** | | |
| **To the champion** | **18.3** | **7.4** | | |
| The last champion (three phases) | 14 | 9 | Climax | (his fight, later) |
| **Clean pass** | **32.3** | **16.4** | | |
| Side paths: the chariot 2, the Stutter 2.5, the trial (the Chakravyuha) 4; the empty place (minutes set at build) | + 8.5 | + 4 | | |

That is within `WORLD.md`'s 15 to 20 before the champion, with the weight on the fight (Decided: "the final area is a
bit shorter"). Levers if play runs long, in order: the chained in 1D (0.4), the Green Man in 3B (0.5), the drowned in 1C
(0.4). Levers if it runs short: a third ji-bearer in the formation, a second runner in the pack.

**Enemy count.**

| District | Main path | Count |
|---|---|---|
| D1 | 2 wicks, 3 gilded levies, 1 hooked harpooner, 1 drowned, 1 wheeler, 1 mender, 1 chained | 10 |
| D2 | 2 runners and a howler, 2 brittle, 1 buried, 5 clay (officer, 2 ji-bearers, 2 crossbowmen), 1 vessel | 12 |
| D3 | 1 samurai, 1 fox pair (counted as one, as in its land), 1 crow-tengu, 3 King's Men, 1 Green Man | 7 |
| Hazards, not counted | The hanging burst, the wave, the pour, the elephant, five bolts, the Hunt's riders, A1 to A6 | |
| Side paths | None on SP1 to SP3; the trial's rings about 12 | about 12 |
| **All** | | **29 on the main path, and the champion** (Fire 40 in 41 minutes) |

About one and a half a minute, more than Fire's one, because most can be met one at a time or left frozen by the line
the player walks: an experienced player who reads the thaw fights perhaps 18 of them.

---

## 9. Questions for the owner

**Answered (owner, 2026-11-19): the recommended option (a) in all seven.**
1. The thaw is 5 m, with the camera eased back to 1.15 times for the whole land.
2. Three speeds: the corrupted wake at full speed and hunt; the mortal war resumes at a tenth of speed and never aims
   at the player; arrows and falls move at the slow along their lines and hurt what they hit.
3. Woken enemies freeze again after clinging 4 s outside the thaw, and wake again in 0.4 s.
4. The land has a short trial, the Chakravyuha.
5. Yes: this is the war of the game's opening, and the player finds their own empty place in a frozen rank.
6. The climb is recapped in four sections in the order of the tiers.
7. Time's place is the ring's opening, a bare stone, with the gnomon at the centre where Time sat.

The body of the plan has been brought into line with these answers (2026-11-19), and with the cross-land answers of
`kit_list.md` section 5 (Q1, Q3, Q4, Q5, Q9). The rest of the plan stays Proposal (Claude) until the owner walks it.

Each has two or three options, the recommended one first.

1. **How big is the thaw, and does the camera change for it?**
   - **(a) 5 m, with the camera eased back to 1.15 times for the whole land, so the circle almost fits the frame with
     grey world beyond it on three sides.** Recommended. The image is the circle in the grey (`LOOKS.md`); Skyward
     Sword's timeshift fields are small and local for the same reason, so the past and the present are seen side by side
     and the robots that come alive are always close.
   - (b) 5 m with Fire's camera unchanged: at the top of the frame only 2.2 m of grey is left to read what will wake.
   - (c) 7 m: room to fight in the colour, but the grey is pushed off the frame's sides and the stir has nowhere to show.
2. **What moves in the thaw?**
   - **(a) Three speeds: the corrupted of every land wake at full speed and hunt; the mortal war resumes its last second
     at a tenth of speed and never aims at the player; arrows and falls move at that slow speed along their lines and
     hurt what they hit.** Recommended. Only one kind of thing hunts, so a frame is readable, and the arrows become
     dodgeable lanes. Superhot's makers call their rule "pure speed without the time pressure"; Quantum Break's frozen
     and looping scenes made stopped debris into hazards and platforms. Both show stopped time reads best when what moves
     is few and chosen.
   - (b) Everything in the thaw moves at full speed and the mortal soldiers attack too: a true battle round the player,
     but every frame is a crowd and the corrupted are lost in it.
   - (c) Only the corrupted move; the mortal war stays frozen even in the thaw. Simplest; the war never comes back to
     life (the image `LOOKS.md` asks for) and the arrows cannot fly.
3. **Do woken enemies freeze again?**
   - **(a) Yes, after clinging 4 s outside the thaw, in whatever pose they hold, and they wake again in 0.4 s when the
     thaw comes back.** Recommended. A fight never freezes mid-fight (the enemy is beside the player), but a crowd can be
     shed and a formation woken in part, which is where the land's choices come from. Quantum Break's designers found
     that players simply run round enemies in stopped time and were "always having to find ways to gate players"; the
     cling and the single path are this plan's gate.
   - (b) Once woken, awake until killed. Pure soulslike; the thaw becomes only a trigger and the land loses its tactics.
   - (c) Frozen the moment they leave the thaw. A strong tool that makes most fights a step backward.
4. **Does this land have a trial?**
   - **(a) Yes, a short one, the Chakravyuha (4.3): three rings of frozen soldiers from every land round a heart, a
     harder mix woken as the player walks the spiral, the way out made through the gaps the fights leave, a Memory at
     the heart.** Recommended. It is the one trial no other land could have, it tests the thaw harder than the path, and
     it is built from the path's own pieces. Its source is the land's own epic, a formation that was entered and not left.
   - (b) None: the land is short and the weight is on the last fight. Dark Souls' Kiln of the First Flame is remembered
     as a short ash path and then the end, and it works.
   - (c) A duel with the hunter's double (the shadow hunter of his phase 1, `WORLD.md`): a strong fight, but it spends
     one of the champion's surprises before he uses it.
5. **Is this the war of the game's opening?** `docs/GAME.md` decided (2026-11-15) that the game opens on a battlefield
   where a mortal army's assault on the gods is destroyed, the player falls to the god of Light and Dark, and the
   sponsor saves them at the price of their past. It has not been revisited since Fire became the first land.
   - **(a) Yes: the opening is played on this field in its last minutes, and on the path the player finds the one empty
     place in a frozen rank, where they stood when the sponsor took them out of time (a "see it, then find the way" beside
     2A).** Recommended. It makes the last land a return, and gives "what the player's past was" (Open) a place without
     saying it. Sekiro brings the player back to Ashina Castle at the end, under attack and changed, and has Kuro wait in
     the field where the player first met him.
   - (b) Yes, but the field shows no mark of the player: the link waits for the sponsor's fight.
   - (c) No: two different wars, and the opening is designed apart.
6. **How is the climb recapped?**
   - **(a) Four sections in the order of the tiers, each holding its lands' own people with their own mechanics and one
     piece of their land's rule caught mid-event (1.6).** Recommended. Celeste's last chapter climbs through one section
     for each earlier chapter, in order, each reusing that chapter's terrain and tools in new arrangements: the recap is
     felt as the whole journey, not as a boss rush.
   - (b) The same, but the two halves of each tier's section swap to match the order this player beat them in. More
     personal; each half must then work from either end, which costs a second layout of the joins.
   - (c) No sections by land: the hosts mixed along the whole path. Harder fights; the climb is no longer read in it.
7. **What is the twelfth place on the ring?**
   - **(a) Time's place is the ring's opening: a bare stone where nothing was built, Light and Dark either side of it, and
     the gnomon at the centre is where Time sat instead.** Recommended. It counts twelve (eleven seats and Time's place),
     puts Time's siblings beside it, and makes the champion sit in Time's own place and its shadow. Circular yogini
     temples are open to the sky and their circle is read as the cycle of time; the nine planets of a temple are set so
     that none faces another, as these seats face outward.
   - (b) Twelve seats all round the ring, Time's an empty one at 12 o'clock, opposite the opening. A clock face exactly;
     Time's emptiness is far from the player.
   - (c) Eleven seats in the ring and the gnomon as the twelfth. Simplest; the clock has an hour missing.

---

## Sources

- Celeste's last chapter, a section for each earlier chapter in order:
  [Celeste Wiki, The Summit](https://celeste.ink/wiki/The_Summit),
  [Neoseeker walkthrough](https://www.neoseeker.com/celeste/walkthrough/Chapter_7_-_The_Summit).
- Skyward Sword's timeshift stones and orb (a local field of the past; the robots alive only inside it):
  [Zelda Dungeon wiki](https://www.zeldadungeon.net/wiki/Timeshift_Stone),
  [Zelda Wiki](https://zelda.fandom.com/wiki/Timeshift_Stone),
  [Zelda Dungeon, Lanayru Mining Facility](https://www.zeldadungeon.net/skyward-sword-walkthrough/lanayru-mining-facility/).
- Superhot, "time moves only when you move", and its makers' "pure speed without the time pressure":
  [Game Developer, Road to the IGF](https://www.gamedeveloper.com/design/road-to-the-igf-superhot-team-s-i-superhot-i-),
  [Wikipedia](https://en.wikipedia.org/wiki/Superhot).
- Quantum Break's stutters (frozen and looping objects as hazards and platforms, long-exposure streaks, gating players
  who run round enemies): [GameSpot](https://www.gamespot.com/articles/how-quantum-breaks-developers-tackled-time-manipul/1100-6436820/),
  [Game Informer](https://gameinformer.com/b/features/archive/2015/11/06/new-details-and-screenshots-on-quantum-breaks-frozen-reality.aspx),
  [Shacknews](https://www.shacknews.com/article/93637/quantum-break-narrative-designer-discusses-junction-points-time-powers-and-episodic-gaming).
- Dark Souls' Kiln of the First Flame, a short final path (fan discussion; no developer statement found):
  [Giant Bomb](https://www.giantbomb.com/kiln-of-the-first-flame/3035-4807/),
  [ResetEra thread](https://www.resetera.com/threads/what-if-the-kiln-of-the-first-flame-in-dark-souls-was-another-full-fledged-area-spoilers-i-guess.203223/).
- Sekiro's Ashina Castle revisited and changed at the end:
  [Sekiro wiki](https://sekiro-shadows-die-twice.fandom.com/wiki/Ashina_Castle),
  [atomicbobomb, revisiting levels](https://atomicbobomb.home.blog/2019/06/15/sekiro-shadows-die-twice-revisiting-levels/),
  [GameFAQs, the final visit](https://gamefaqs.gamespot.com/ps4/241004-sekiro-shadows-die-twice/faqs/76974/ashina-castle-and-outskirts-the-final-visit).
- The Samrat Yantra at Jaipur, a staircase gnomon 27 m high: [Wikipedia, Jantar Mantar, Jaipur](https://en.wikipedia.org/wiki/Jantar_Mantar,_Jaipur).
- The nine planets set so that none faces another: [Wikipedia, Navagraha](https://en.wikipedia.org/wiki/Navagraha),
  [Dayton Hindu temple](https://daytontemple.com/navagraha/).
- Circular yogini temples, open to the sky, their circle read as the cycle of time:
  [Wikipedia, Chausath Yogini Temple, Mitaoli](https://en.wikipedia.org/wiki/Chausath_Yogini_Temple,_Mitaoli),
  [UNESCO tentative list](https://whc.unesco.org/en/tentativelists/6804/).
- Bhishma on the bed of arrows, choosing the hour of his death: [Wikipedia, Bhishma](https://en.wikipedia.org/wiki/Bhishma),
  [Wikipedia, Uttarayana](https://en.wikipedia.org/wiki/Uttarayana).
- The chakravyuha, the formation entered and not left, and the Halebidu carving:
  [Wikipedia, Chakravyuha](https://en.wikipedia.org/wiki/Chakravyuha), [Wikipedia, Abhimanyu](https://en.wikipedia.org/wiki/Abhimanyu),
  [Inditales](https://inditales.com/labyrinths-in-india-explore/).
- From memory, not looked up this session: the wheels of the Konark sun temple carved as sundials; the burning of the
  Khandava forest and the arrow canopy (already in `WORLD.md`).
