# The Poison land's levels (first pass)

Written by Claude on 2026-11-19 as the Poison land's counterpart to `fire_levels.md` and `water_levels.md`, before any
of the land is built. **Everything here is Proposal (Claude)** unless it is marked **Decided** with its source. The
land's myth, people, elite and fight are `WORLD.md`, "Poison's land: the court alchemist"; its look is `LOOKS.md`
section 7; the rules every land follows are `WORLD.md`, "Level design rules for every land (decided 2026-11-18,
evening)". The enemies' full numbers belong in a later `poison_land.md`, as `fire_land.md` section 4 holds Fire's. This
plan gives only the numbers the rooms and the clock depend on, marked as first guesses.

Axes as in Fire: x east, z north, y up; north is up the screen. Coordinates are metres. "Floor y 6" is the walking
height. The land is three scenes (names proposed): `LanternCity` (the river town and the inner court), `UndyingTomb`
(the tomb under the palace and the emperor), and `ImmortalTower` (the approach, the laboratory, the gu jar and the
crane). The tomb must be its own scene because it lies under the court: in one scene the court's floor would be a roof
over the tomb's walkable floor, which the decided rules forbid. The player wakes at (10, 0, 0) in `LanternCity`.

---

## 0. What the owner has decided, and how this plan reads it

| Decided | Source | How this plan uses it |
|---|---|---|
| The map is a city with a ground deck and a rooftop deck for the miasma hours | `WORLD.md`, level design rules, 2026-11-18 evening | Every district has a ground deck and a roof deck, and in the court a high deck above that (1.1). The roof deck is a chain of islands the player climbs onto when the gong rings, joined across three streets by narrow plank bridges (1.1 rule 3) |
| Poison's verb is *climb*: the one true clock; gongs ring the hours and the player climbs above the miasma | same | The hour is the land's clock (1.2). It runs on time, not places, so its warning has to be heard and seen from anywhere |
| The player climbs at marked points: fixed ladders, stairs, carts and drainpipes, each marked by a red lantern so it reads from above; enough of them that one is always 15 to 20 seconds away when the first gong warns; no new climbing system | same | Section 1.3. The plan keeps every point on the main path within 12 s of a climb point, to leave room to break off a fight. Ladders, drainpipes and carts use the existing `Ladder` climb |
| The rule of the land: gongs mark the hours and the poisonous mist rises and falls with them, sending the player up to rooftops and high walls | `WORLD.md`, "Poison's land" | The mist rises from drains and water at the first gong, holds, and sinks at the drum |
| Rest fires in Fire's rhythm: one every 6 to 8 minutes of a new player's walk, always one at the god's door, one shortcut loop per district back to its fire, 5 to 7 fires | level design rules | Six fires and four shortcuts, one of them a ferry (sections 2 and 4) |
| Two kinds of optional discovery: "see it, then find the way" (the main kind, holding the plain weapon and lore), and one optional trial invented for this land | same | The lantern barge, the old god's herb garden (the plain weapon), the craftsmen's gallery; the trial is the Third Watch (3.2) |
| Up to three walkable levels per district, joined by ramps, stairs and ladders seen from above; nothing walkable under a roof; roofs and overhangs only fade | same | Section 1.1. Roofs the player walks on are floors; nothing walkable lies under one. The roof deck crosses a street only by a narrow plank bridge drawn shadow-only from below, the owner's chosen exception for this land (Decided, owner, 2026-11-19, answer 3; 1.1 rule 3) |
| The enemies: the vessels (bloated drinkers of elixir who burst into a toxic cloud when the player gets close), the gu (when one dies, the nearest absorbs it and grows, so the order of killing matters), the terracotta soldiers (not poison-based) | `WORLD.md`, "Poison's land" | Section 1.6, each with its own mechanic. The clay army's immunity and formations are WORLD.md's **Proposal**, used here |
| The elite is the Undying Emperor, the puppet emperor before the alchemist | same | Section 5. His technique is the Scorpion's Tail (`steel/weapons.md` 14, Proposal) |
| The god's second phase is five creatures in a gu jar; at each gong, gas fills the jar's bottom and the player climbs onto shelves (the gong part is a Proposal) | `WORLD.md`, "The fight" | Section 6 gives the room and the approach, not the fight |
| The ride out is a crane | `WORLD.md`, rides out | Section 6. The hub keeps "the crane on its post" (Decided), so here the crane waits on a post |
| Night, a sick moon in haze; black lacquer, grey tile, dark water; the miasma blue-green, bile-yellow at the deadliest hour; cinnabar small, flat and a step browner than the robe; lanterns amber paper | `LOOKS.md` section 7 and its question 11 (Decided, 2026-11-18) | The two colours are the two kinds of hour (1.2). The climb points' red lanterns are the only red lanterns in the land, smaller and browner than the robe; every other lantern is amber (Decided, owner, 2026-11-19, answer 4) |
| The Poison god's hook swords are the Twin Knives' god's bough; Consume gives the plain knives if the player has not found them | `steel/weapons.md` 0.1, 2026-11-18 | So the land's plain weapon is the plain Twin Knives, as bronze herb knives in the old god's herb garden (3.2; Decided, owner, 2026-11-19, answer 7) |
| Poison is in tier 3, one of the last solo lands because it is the most chaotic; a clean pass for a new player about 45 minutes (Proposal in `WORLD.md`) | `WORLD.md`, "The order of the climb" and "How a climb is structured" | Section 8: 44 minutes |

The owner's answers to Water (`water_levels.md` 9) carry over where they are rules of the game, not of the lagoon: no
swimming (mercury and deep water throw the player back), the world's own marks over HUD marks, the clock's state kept
honest after a fall, and three held camera shots per land.

---

## 1. Principles

### 1.1 What the camera sees: two decks from 12 m up

The camera is Fire's (`fire_levels.md` 1.1): the picture reaches 7.2 m north of the player and 4.3 m south, 15.5 m wide
at the player. Something H metres above the player's floor is in frame only within (5.0 - H) / 0.7 m north. Landmarks
are rooms the player arrives in, or vista zones (`LandZones`). Enemies walk straight at the player with no navmesh, and
ranged attackers stay in frame. What changes here is that the land is two decks of a city.

**What each deck sees of the other.**
- *From the ground deck*, the roof deck 4 m up is in frame only within 1.4 m north: the player sees its **south faces**,
  never its top. A roof deck reads from below as a band of face with an eave line, drawn in the shade colour with an
  ink edge (Fire's rule 2). So climb points that are ladders or drainpipes stand on **south faces** (1.3), and every
  red lantern hangs at the climb point's **foot**, 2.4 m up, throwing a pool of red-amber light 2.5 m across on the
  ground. The pool is what the player sees from 7 m away, because the floor is the main canvas (`LOOKS.md` rule 1).
- *From the roof deck*, the ground 4 m below is in frame to 12.9 m north (5 + 4 over 0.7) and over a wider strip: the
  roofs are where the player sees farthest. They are the scout's deck. Every "see it" moment in this land is seen from
  a higher deck.

**Rules for the roof deck** (Proposal, each from the decided "nothing walkable under a roof; roofs and overhangs only
fade" and Fire's rules 3 and 4):
1. **Walkable roofs are floors.** Their tiles are pitched at most 8 degrees where fights happen (a broad ridge walk 1.5
   m wide on top) and at most 15 degrees on strips only walked. The tall, steep roofs of halls, towers and gates are
   never walkable: they are landmarks and they fade.
2. **No overhang over a walkable floor.** A walkable roof's eaves stand out at most 0.8 m, and the 0.8 m strip under
   them is a raised stone kerb 0.6 m high carrying planters, water jars and stalls: not a floor (the step offset is 0.3
   m). Nothing walkable lies under any roof in the land (a plank bridge is not drawn as a roof from below: rule 3).
3. **The roof deck crosses a street only by a plank bridge.** **Decided** (owner, 2026-11-19, answer 3): the roof deck
   may cross streets by narrow plank bridges, drawn shadow-only from below so the street stays visible; this is the
   owner's chosen exception to the roof rule, for this land only. Otherwise roofs join only across water (a canal, a
   moat), across closed light-wells too narrow and cluttered to walk, and along the tops of thick walls. The roof deck
   is still a set of **islands**, joined by planks in three places where a plank makes a better route (PB1 to PB3, below);
   streets and courtyards are otherwise crossed in the calm. **Proposal (Claude)** for the planks:
   - *Where:* **PB1** over the alley between blocks EA and EB (x 15, z 30 to 33, y 4, 3 m long); **PB2** over the lane
     between EB and EC (x 18, z 50 to 60, y 4, 10 m long), so the east row is one roof walk from L1 to EC; **PB3** from
     store hall SH2's east edge to the Outer Court's east wall walk (z 202, x 6 to 10, y 10, 4 m long), so SH2 has a way
     off besides its stair. Each joins a stair island to a ladder island or a wall walk: the plank is the way off a stair
     head that is being lost (1C, 2B).
   - *Size:* 1.5 m wide, at most 10 m long (a crossing under 2 s at a walk), level with the roofs it joins, a rope rail
     0.6 m high along both sides (the parapet height of rule 5), so no roll and no push goes off it.
   - *From above* (the player on a roof deck): bare grey boards with the grain across the walk, the rope rails as two
     dark lines, iron cleats where they sit on the roof lips. No lantern (planks are not climb points). A character on a
     plank casts its contact shadow on the boards, so the boards name the deck (cue 1 below).
   - *From below* (the player's floor lower than the plank): the plank is not drawn; only its shadow lies across the
     street, a soft moon-shadow band 1.5 m wide with the rails' two thin lines in it. The street under it, and anyone on
     it, is seen whole, which is also how a plank keeps the south-side rule (rule 4). The shadow is the cue that a way
     across runs overhead.
   - *Enemies:* the clay never step onto a plank (too heavy, and no rank fits on it): they stop at its end in the
     cut-off pose (cue 4 below), as at a ladder. Vessels do not wander onto planks; a vessel's cloud that reaches a plank
     falls through it to the street below (it flows off edges, 1.6). A gu crawler crosses a plank when its straight line
     runs along it; otherwise it drops to the street and crawls on.
   - *Under fire:* a plank gives no cover. Crossbow bolts reach a player on it from either side, though the boards stop
     bolts from straight below like any floor; a bolt's 0.5 m push is not applied on a plank. A crossing is timed
     between volleys.
   - *The mist:* the D1 planks (y 4) are above every hour; PB3 (y 10) is above the green and under the yellow, like the
     wall walks.
4. **The south-side rule, measured from the floor the player stands on.** On the ground: nothing more than 1.4 m
   above the street within 4 m south unless it fades, so the house fronts on an east-west street's south side fade
   (`Fadeable`). On a roof: nothing more than 1.4 m above that roof within 4 m south unless it fades, so a steep hall
   roof south of a roof walk fades too.
5. **Edges.** Every roof edge has a lip of 0.3 m; south edges have a parapet of at most 0.6 m (Fire's rule 4). Drops
   from a roof face south, are 2 to 4 m, and land on ground that has a climb point within 10 m.
6. **Cover on roofs.** Crossbow bolts from below (1.6) are stopped by anything on the roof at least 1.4 m tall:
   chimney stacks, stacked tiles, drying racks hung with mats, bronze water vats. Roof furniture is placed as cover, in
   groups 4 to 6 m apart.

**Which deck an enemy is on.** The player always has four cues, and the plan never asks for a fifth:
1. *Floors differ by deck*: the ground is dark wet stone in a grid; the roof deck grey tile in strong diagonal rows
   (`LOOKS.md`); wall walks brick with a paved centre line; plank bridges bare boards; the high deck pale stone. Every character casts a contact
   shadow on the floor it stands on, so the pattern under its shadow names its deck.
2. *The mist separates the decks*: when it is up, everything in it is lit from below and loses its legs in the glow,
   and the clay army reads as dark silhouettes against it; everything above it is rimmed by the cold moon.
3. *One deck per melee fight*: no melee enemy stands on another deck within 4 m of where the player will fight. An
   enemy on another deck is a crossbowman (ranged), a soldier on a stair (climbing toward the player, in frame), or a
   vessel's cloud falling off a roof.
4. *The cut-off pose*: an enemy that cannot reach the player's deck stops at the edge and looks up (clay ground their
   halberds), so it never jitters against a wall.

### 1.2 The hours

**The clock** (Decided: the gongs ring the hours and the mist rises with them; the details are Proposal). One clock per
scene. The length of the hour is set by the district the player stands in, changed only during the calm.

| Phase | What happens | Green hour | Yellow hour |
|---|---|---|---|
| Calm | The streets are open; the people are out | D1 110 s, D2 70 s | (D2 only) 70 s |
| **First gong** (one stroke) | The warning. Mist wells from every drain grate, canal and well; the climb lanterns flare and swing; the watchman's wooden clapper starts, every 2 s, then faster | Mist at the knee at **18 s**, full at 25 s | A deeper stroke, gong and great bell together; knee at **16 s**, full at 20 s |
| **Second gong** (two strokes) | The mist at its full height; it bites wherever it is | Held 35 s (D1), 45 s (D2) | Held 50 s |
| **The drum** (three beats) | The mist sinks over 10 s; the streets are wet and shining | 10 s | 10 s |
| The hour (Decided, owner, 2026-11-19, answer 1) | | D1 180 s; D2 150 s | D2 150 s, every third hour |

**What is safe.** The mist bites a player whose floor is 0.5 m or more below the mist's top (it is over the knees):
green 3 every 0.8 s, yellow 5 every 0.8 s (first guesses, Fire's `NoonHeat` numbers for green; a green hour fully in
the mist kills, being 4 s late costs 15). Each district's heights:

| District | Ground deck | Green top | Yellow top | Safe in green | Safe in yellow |
|---|---|---|---|---|---|
| D1 the river town | boats y -0.8, streets and quays y 0 | y 2.5 | none | bridge crowns (y 3), roofs (y 4), the street above its second landing (y 4) | |
| D2 the inner court | courtyards y 6 | y 8.5 | y 11.5 | wall walks and flat roofs (y 10), throne tier 2 (y 10.5) | the high deck only: gate pier tops, the drum platform, the watch terrace, throne tier 3 (all y 13) |
| D3 the tomb | see 1.7 | | | | |
| The tower approach | rises a tier at every gong and never falls (6.1) | | | | |

Every face the mist reaches carries two **stain lines**, a green one at the green top and a yellow one at the yellow
top, laid down by years of hours: anywhere in the land the player can see how high each hour comes, as the Water land's
weed band shows the high tide (`water_levels.md` 1.2, the owner's answer 2).

**The warnings, together.**
- *Sound*: the gong is struck in the drum tower (D2's landmark, 3.2) and heard everywhere, muffled underground. The
  clapper is the countdown: Chinese night watchmen walked in pairs with a gong and a wooden clapper (sources). The drum
  ends the hour, as drums marked the watches.
- *Light*: at the first gong every climb lantern brightens and swings; the drain grates glow from below a second
  before the mist comes out of them; the moon's halo takes the hour's colour (yellow at a yellow hour).
- *The mist's edge*: a hard, bright top edge that climbs every south face as it rises, toward the stain line.
- *Words*: once, at the land's first hour, a status line: "The gong. The mist is rising." (Timed text is for status
  lines; `CLAUDE.md`.)

**Placed hours.** As Fire placed its first swell, the first hour of D1 is forced by a trigger (1C), the first hour after
F2 is placed (1E), and D2's first yellow hour is forced at the drum tower (2C). No hour is yellow before that one; the
count of three starts from it. Otherwise the clock runs.

**After a rest or a fall**, the clock starts at the beginning of a calm, so the next first gong comes no sooner than 60 s
after waking. The clock is not saved across sittings; everything else the player changed is.

**The people and the hours.** The clock changes who is on the streets, so the calm and the mist are two different
fights:

| | Calm | First gong | Mist held | Drum |
|---|---|---|---|---|
| Vessels | Wander and drink | Walk to the nearest door within 15 m and go in; those on roofs stay | Inside. One caught in the mist swells and bursts after 4 s | Come out of the same door |
| Gu | As placed | Go down their home drain grate (each has one within 10 m) | Below | Come up the same grate |
| Clay (town, Outer and Middle courts) | **Statues** at their posts. Struck, they crack and break, but do not wake | Wake, form up | **March the watch**: their route, through the mist, which does not touch them | Walk back to their posts and set |
| Clay (Inner Court and tomb) | Awake: near the emperor they never sleep | As the mist | March | Guard |

This is how the miasma hours become a fight and not a wait (Proposal, built on WORLD.md's proposal that the clay are
immune and march through the mist):
1. **The roofs are the artillery deck.** In the mist the clay march below, in frame from 4 m up, and the player's
   spells land on them (pillar 2: steel and element are equal partners). A formation's officer, safe behind shields on
   the ground, can be struck from above.
2. **The army shoots back.** Crossbow ranks volley at anyone in frame on any deck (1.6), so the player moves from cover
   to cover on the roofs.
3. **Stairs break formations.** Clay climb stairs but never ladders, drainpipes or carts. A formation climbing a stair
   becomes a file, one soldier at a time, and the player fights each alone at the stair head. Whether an island has a
   stair is a choice the player makes when they climb: a ladder island is safe from the clay's feet, a stair island
   can be held, and where a plank joins the two (1.1 rule 3) the player can hold the stair head and still have a way
   off.
4. **Tip the vats.** Bronze fire vats stand on some roof and terrace edges (the palace's real fire vats stood in its
   courts). A heavy blow tips one: it falls 2 m out from the edge, cracks whatever it lands on and breaks a formation's
   lock for 3 s.
5. **The calm prepares for the mist.** In the calm, the clay are statues. A player who spends calm time breaking the
   crossbowmen of a formation whose route crosses their way meets it in the next hour without its volleys. The time
   spent is the cost; broken statues stay broken until a rest.

### 1.3 Climbing points

**Decided**: fixed ladders, stairs, carts and drainpipes, each marked by a red lantern; no new climbing system. Proposal
for each:

| Kind | Climb | Clay follow? | Where it stands | Built from |
|---|---|---|---|---|
| Ladder (bamboo, fixed) | 1.2 s up or down | No | South faces; east or west faces only where its floor pool lies on the walk's line; never north faces | `Ladder` as built |
| Drainpipe (a clay downpipe in iron brackets) | 1.6 s up, 1.0 s down | No | As ladders | `Ladder` with `ClimbSeconds` 1.6 |
| Cart (a handcart or ox-cart against a low roof) | 1.0 s onto the low roof, at most 3 m up | No | Anywhere it can be seen from above | `Ladder` (foot at the cart's tail, head on the roof) |
| Stair (an outside stair, a horse ramp up a wall, a terrace stair, a bridge) | Walked | **Yes** | Anywhere: stairs read from above | `Stairs` (Fire's builder helper) |

- The lantern hangs at the foot of every climb point at 2.4 m and throws its floor pool. A second, smaller lantern
  hangs at the head, on the roof's lip, so from the roof the way down reads too.
- **Density.** No point of the main path's ground deck is more than **12 s of walking (72 m of path)** from a climb point
  that leads above the next hour's mist. That leaves 3 to 8 s inside the decided 15 to 20 s for breaking off a fight.
  Every room below names its climb points and the farthest walk to one.
- In D2, every climb point onto a wall walk (y 10) is within 10 s of a stair up to the high deck (y 13), so a yellow
  first gong can be answered from anywhere. The Ministry's two store halls are green-only refuges (SH2 joined by the
  plank PB3 to the east wall walk, itself green-only): from their roofs the yellow stain line is in view on the Middle
  Wall's face, above the player's head.
- One-way drops are not climb points and carry no lantern.

### 1.4 Never stranded

1. The mist bites only below its top, and every district has floors above its highest hour within 12 s of every point
   on its ground deck (1.3).
2. Every roof island has at least one two-way climb point; no island is reached only by a drop.
3. **Every rest fire burns the old god's herbs** (mugwort and calamus, which households hung at the Double Fifth to keep
   off the five poisons; `LOOKS.md` 7). The smoke keeps a ring 4 m round the fire clear of mist at every hour. Fires
   also stand above their district's mist where the room allows; the ring is for F1, which cannot, and for the player
   who is late. The clay walk into the ring; only the mist cannot.
4. Waking is always in the calm (1.2).
5. A player knocked off a roof or a wall into the mist lands on ground with a climb point within 10 m (rule 5 of 1.1).
6. Deep water and deep mercury are never entered: the player meets the throw-back of `water_levels.md` 1.2 (returned to
   `FallCatch.LastFirm` after 1.2 s with 10 damage; 12 in mercury), as the owner chose for Water (no swimming).

### 1.5 Measures

| What | Measure | Why |
|---|---|---|
| Walk, roll | 6 m/s; roll 4.5 m (fists) | As Fire |
| Deck heights | Roof deck 4 m above the ground deck; high deck 3 m above the roof deck (D2) | 4 m reads as "up" and puts a whole hour's mist between the decks; more and the roof's face would fill the frame |
| A street | 6 m wide (quays 6 m, lanes 4 to 6 m) | Room to sidestep a volley comb (lanes 1.6 m apart); not room to circle a formation |
| A roof island | at least 6 by 10 m where a fight is held | A stair head plus one soldier, and cover |
| A wall walk | 4 m wide, parapets 0.6 m | One-on-one; a fight here is a duel |
| A plank bridge (**Proposal (Claude)**) | 1.5 m wide, at most 10 m long, rope rails 0.6 m both sides; three in the land | Narrow (the owner's answer 3); crossed in under 2 s, so between volleys; too narrow to fight on, and the clay will not take it |
| A formation's front | 2 or 3 shields abreast, 1.5 m apart | Fills a 6 m street; a gap is 1.5 m |
| A volley comb | lanes 0.5 m wide every 1.6 m, 16 m long; the second rank's comb 1.2 s after, offset 0.8 m | Stand between lanes, then step half a lane |
| Climb point spacing | at most 12 s of walking on the main path | 1.3 |
| The calm | D1 110 s, D2 70 s | Each ground leg between climb points is crossed in the calm with time to fight |
| A rise that reads; a drop; a step; stairs; ramps | 1.5 m; 2 m; 0.3 m; 27 to 32 degrees; 8 degrees fought on, 15 walked | As Fire |

### 1.6 The land's people, as the rooms use them

All three corruptions are **Decided**; how each one fights is **Proposal**, each its own mechanic (the owner's bar: no
reskins) and each tied to the hours.

- **The vessels** (bloated drinkers of the court's elixir). They wander, and drink at bronze vats. A player within 2.5 m
  for 0.6 s makes one swell (the belly glows green) and burst; so does killing it. The burst is the owner's own example
  made poison, so what makes it this land's is the cloud:
  - **The cloud is heavy.** 3 m round, lasting 6 s, it doses what is in it (2 every 0.5 s) and **flows downhill**: down
    stairs, and off roof edges into the street below, where it pools for 10 s. A vessel burst on a roof poisons the
    street, not the roof.
  - **They drink.** Each drink at a vat (one every 6 s) widens its cloud by 1 m, up to three. A vessel left at a vat
    becomes a bomb.
  - **They chain.** A vessel in another's cloud swells and bursts 1.5 s later.
  - A heavy blow knocks a vessel 2 m before it bursts: aim it at the others. A spell bursts it from range.
  - Clay are immune to the cloud; gu are not.
- **The gu** (people with a gu creature in them). On death the creature crawls out (a pale grub the length of a forearm,
  2.5 m/s, a glowing trail) **straight toward the nearest living gu**. Reaching it, that host grows one stage, heals,
  and gains a move. One blow kills a crawler; a crawler that meets water or mercury sinks; one with no host within 12 m
  dries out in 4 s. Three stages:
  - *Hollow*: a lunging bite (1.2 m).
  - *Grown* (one absorbed): adds a venom string spat down a 6 m lane, leaving a puddle for 4 s.
  - *Gu King* (two or more): adds a burrow, down any drain grate and up under the player (a ring, 0.9 s).
  So the order matters (Decided): kill the one farthest from the others first and catch its crawler; never kill one
  beside a Grown one. The crawl is the first sight of what the god's creatures do with their dead (`WORLD.md`, eating
  the dead), in a small form, as the hook brother shows the fisher's moves.
- **The terracotta soldiers** (the clay; not poison-based, Decided). Grey clay with flakes of their first paint
  (`LOOKS.md`).
  - **They keep the hours** (1.2): breakable statues between hours in the town and the outer courts; awake always near
    the emperor (Decided, owner, 2026-11-19, answer 2).
  - **They do not flinch.** Light blows chip them (damage, no stagger). A heavy blow cracks one (a dark seam); a cracked
    soldier shatters at the next blow of any kind.
  - **They march routes, not at the player.** A formation follows its watch route (a closed line of points set by the
    builder) and only turns on the player when the player is within 8 m on its deck, or within 8 m of a stair foot it
    can take. It then wheels as one block, at 30 degrees a second: getting round it is a race against its wheel. To
    reach a player on a roof island it walks to that island's nearest stair foot, then up, then straight at the player
    (two straight legs: no navmesh).
  - **Ranks.** A *ji-bearer* front rank (shield and halberd: the shields lock, and a locked front takes no damage from
    the front except a heavy's crack; the halberd's hook sweeps a 3 m arc). Behind it, *crossbowmen* in two ranks, one
    kneeling and one standing, firing in turn: the arrangement of the real crossbow unit in the emperor's second pit,
    where one line shot while the other reloaded (sources). Their **volley** is a comb of lanes (1.5), the standing
    rank's then the kneeling rank's into its gaps; bolts are guardable from the front (pale lanes) and push the player
    back 0.5 m; a volley is fired only by a shooter that is on screen.
  - The **officer** (a taller figure with a drum on his back). While he stands, the formation closes any gap within
    4 s (the soldier behind steps up). Break him and the formation dissolves into single soldiers who walk straight at
    the player.
  - Breaking one shield-bearer opens a gap 1.5 m wide (WORLD.md's proposal): through it, the crossbowmen club with their
    stocks and are slow.
  - **How this is not Fire's shield wall** (`ShieldWall`, levies locked and advancing in step): the clay march a route
    whatever the player does, are fought from another deck as often as on the ground, never flinch, crack and then
    shatter, volley in alternating combs, never climb a ladder, and walk through the land's hazard instead of fleeing
    it. The lock may reuse `ShieldWall`'s code; nothing else does.

### 1.7 Under the palace: how the rule changes in the tomb

The tomb has no sky, a vault that is never drawn (seen only as the painted heavens reflected in the mercury, as
`LOOKS.md` 7 has it) and rivers of mercury (Sima Qian, sources). The rule changes in five exact ways:

1. **The hours still come, from above.** The tomb lies under the court, so its clock is the court's (D2's hour and its
   yellow every third), heard as a muffled gong through the stone with a ripple running across the mercury.
2. **The mist falls instead of rising.** At the first gong it pours down **breathing shafts** in the vault as glowing
   cascades, each landing on a known spot marked by a cinnabar ring on the floor. From there it **flows along the floor
   at 2 m/s** and fills the low places first, like water filling a basin. So the warning is a place as well as a time:
   the player sees where it comes from and which way it is running. In a room 30 m from a shaft the mist arrives about
   25 s after the gong.
3. **It fills to a level, then stops.** Each hall has a brim: green 2.4 m above its floor, yellow 3.0 m (the yellow
   reaches the tops of the pit walls but does not bite there; it hides everything below it). In the Hundred Rivers the
   brim is 1.0 and 1.6 m.
4. **It lies on the mercury always.** Between hours, the channels of mercury keep a low glowing mist on them; at the
   drum the floor's mist does not rise back up the shafts but **sinks through the floor grates into the mercury**,
   which takes it ("the mercury drinks it"). So the mercury rivers glow at every hour and are never crossed except by
   bridges and causeways.
5. **The red lanterns are the tomb's everlasting lamps.** There are no lanterns in the tomb (`LOOKS.md`), except the
   climb marks: lamps of "man-fish" oil meant to burn forever (Sima Qian), in red-lacquered bronze shades. They are the
   only warm light besides the clay army's flaking paint.

The climbing points underground are the pit walls' stairs, the beams that fell across the pits, the bridges over the
mercury, and the model hills on the map floor.

---

## 2. The whole land

From the river landing up the lantern canal, across the floating market, through the elixir house and up the stepped
street to the court's south gate; through three courts, walls inside walls, past the drum tower that rings the hours, to
the throne terrace; down the spirit ramp into the tomb, across the clay pits and the hundred rivers of mercury to the
emperor's seat; up the way of smoke to the alchemist's tower, its stair, his laboratory and the jar. The main path is
about 640 m of walking.

```
 LanternCity
 z
392 ............... [ tomb door (-3,13,392) ] -> UndyingTomb
       THRONE TERRACE tier 3 y13 F4*(10,13,389)  tier 2 y10.5  tier 1 y8
336    INNER COURT y6   (honour guard, awake)          walls y10 both sides
310 ===== Inner Wall y10 == Inner Gate (-6..0) ==========================
       [HERB GARDEN y2..9.5]  |  DRUM TOWER COURT y6           [WATCHMEN'S
       (side, W of x-64)      |  drum platform y13 (-28..-12,262..278) YARD y6]
       S3 ladder (-60,10,292) |  honour round (clay keep the hours)   (trial, E)
238 ===== Middle Wall y10 == Middle Gate (-24..-18), F3*(-27,13,236) =====
       OUTER COURT: MINISTRY OF ELIXIRS y6  store halls y10   S2 gate (-70,6,170)
166 ===== Outer Wall y10 === South Gate (-46..-40) ======================
152    SOUTH GATE TERRACE y6  (gate formation)
128    LADDER STREET y0 -> 2 -> 4 -> 6 (x -46..-38)
104    ELIXIR HOUSE courtyard y0, tea terrace y4 F2*(-10,4,114)
 66    FLOATING MARKET boats y-0.8, quays y0          |  canal x-3..3
 54    HUMPBACK BRIDGE crown y3 (first hour)          |  east quay x3..9, roofs y4
  8    LANTERN CANAL                                   |
  0    LANDING y0, wake (10,0,0), F1*(17,0,3)    river to the south; lantern barge
      x: -110      -60        -30        0         30        60
```

```
 UndyingTomb (own coordinates)          ImmortalTower (own coordinates)
196 way of smoke -> ImmortalTower       70  THE JAR'S MOUTH, the laboratory y24
170 EMPEROR'S SEAT y4.5                 40  TOP TERRACE y24 F6*(-12,24,32), the crane on its post
140 EMPEROR'S ISLAND y1.5 F5*(3,1.5,136)   17  stair of long life: four flights 0->24
 76 THE HUNDRED RIVERS y0, mercury     -40  GARDEN OF LONG LIFE y0, arrival (0,0,-60)
 66 RIVER TERRACE y3
  6 THE PITS: trenches y0, walls y3
 -6 BRONZE DOOR COURT y0
-52 SPIRIT RAMP y12 -> 0, arrival
```

**Heights.** `LanternCity`: river and canal water y -1.5; boats -0.8; quays, streets and the market y 0; D1 roofs, the
plank bridges PB1 and PB2, and the tea terrace 4; bridge crowns 3; Ladder Street's landings 2 and 4; the court's courtyards 6; wall walks, store hall
roofs and the plank PB3 10; throne tiers 8, 10.5, 13; gate pier tops, the drum platform and the watch terrace 13; the herb garden 2 to
9.5. `UndyingTomb`: trench floors and halls 0, pit walls and the river terrace 3, mercury surface -0.4, bridge crowns
1.5, the island 1.5, the seat 4.5. `ImmortalTower`: garden 0, landings 6, 12, 18, top terrace and lid floor 24, jar
floor 12, jar shelves 14.5.

**Rest fires (six).** Each is a bronze censer of the old god's herbs (1.4).

| Fire | Where | Foot | Why there |
|---|---|---|---|
| F1 | The landing | (17, 0, 3) | In sight of waking; the herb ring is its refuge |
| F2 | The Elixir House's tea terrace | (-10, 4, 114) | After the market; reached from the market's cart, before the courtyard; S1 and S2 come back toward it |
| F3 | The Middle Gate's west pier top | (-27, 13, 236) | After the Ministry; above every hour; S3 comes back to it |
| F4 | Throne tier 3, behind the hall, at the tomb's door | (10, 13, 389) | After the honour guard; the tomb's fire; S4 comes back toward it |
| F5 | The causeway's north landing, before the emperor's island | (3, 1.5, 136) (`UndyingTomb`) | The elite's door |
| F6 | The tower's top terrace | (-12, 24, 32) (`ImmortalTower`) | The god's door (Decided: one always at it) |

**The route.** Wake at the landing (F1); up the canal's east quay; the first hour at the humpback bridge; across into the
floating market; the cart onto the tea terrace (F2); the elixir courtyard; up Ladder Street to the South Gate; through
the Ministry of Elixirs to the Middle Gate (F3); the drum tower court and the first yellow hour; the Inner Gate, the
honour guard, the throne terrace (F4); down the spirit ramp; across the pits on their walls and beams; the river
terrace; across the hundred rivers (F5); the emperor; up the way of smoke; the garden of long life; the stair of long
life (F6); the laboratory; the jar; the crane.

**Branches and loops.**
- The lantern barge on the river (see it at waking, reached by the S1 ferry): lore and Shards.
- The herb garden of the old god (see it from the drum platform, reached by the west wall way from the Inner Court):
  the plain Twin Knives and the land's deepest lore.
- The Watchmen's Yard (the trial), off the drum tower court.
- The craftsmen's gallery in the tomb (it is S4): their names.

**Vista zones** (`LandZones`; the look-ahead may point any way, Water's W19).

| | Where | Box | Looks | Shows |
|---|---|---|---|---|
| V1 | The landing, (6, 0, 6) | 8 by 3 m | North | The canal running north, lanterns doubled in the black water, the first red pool of light; once, the crane's reflection crossing the water |
| V2 | The South Gate's gap, (-43, 6, 160) | 6 by 3 m | North | The Ministry's furnace yard, the clay standing as statues, the store halls' roofs |
| V3 | The drum platform's west side, (-27, 13, 270) | 4 by 6 m | West | Over the west wall: the sunk herb garden, the statue, the glint on its offering stone |
| V4 | The Inner Gate, (-3, 6, 312) | 6 by 3 m | North | The honour guard on the spirit way, the throne terrace's first tier |
| V5 | The pits' entry, (0, 0, 4) (`UndyingTomb`) | 10 by 3 m | North | **Held shot 2** (below) |
| V6 | The river terrace, (0, 3, 72) | 10 by 3 m | North | The hundred rivers flowing east, the painted heavens in the mercury, the island and the seat |
| V7 | The top terrace's south edge, (0, 24, 30) (`ImmortalTower`) | 12 by 3 m | South | The city below as a sea of glowing mist with roofs standing out of it |

**Three held shots** (as the owner chose for Water, one per district):
1. *The first gong* (1C): the camera eases back 1.6 times for 6 s and the mist wells out of every drain and the canal at
   once, the lanterns flaring down the quay.
2. *The army* (V5): the camera pulls back to 25 m for 5 s over the pits, the clay standing in ranks in their trenches
   (the wide frame of `LOOKS.md`'s `poison_mercury_tomb`).
3. *The crane* (6.4): the ride out.

---

## 3. The districts

Each district: the idea, its three levels, a plan, then each space in walking order. Encounters are numbered E1.1 and so
on, as in Fire. Climb points are L (ladder), DP (drainpipe), C (cart), ST and HR (a stair or a horse stair; bridges and terrace
stairs too: clay can take them). S1 to S4 are the shortcuts.

### 3.1 District 1: the river town (z -30 to 152)

**The idea.** A river town of paper lanterns that served the court: a canal of black water running north from the river,
shophouses along its quays, a floating market of lashed boats, and the elixir house where the court's elixir is sold to
everyone, at the hour. The warmest district (`LOOKS.md`): the mist lies on the water first, so the boats and quays go
under before anything else. **Levels**: the boats (y -0.8), the quays and streets (y 0), the roofs (y 4). Green hours
only, 180 s long.

```
 z     x: -60   -46  -40       -14  -9  -3  3   9         21
152    ====== SOUTH GATE TERRACE y6 (x -60..-10) ======
148     |flight 4->6|
132  DP1|LADDER ST. | landings y2, y4         (gate formation marches down at the hour)
128  ...|  x-46..-38|.......... hall (closed) ..........
120     |side lane  |  ELIXIR COURTYARD y0  vats o o o  | TEA TERRACE y4 F2*
104     |           |                     ST2 stair ->  | (-14..-6)
104  ---+-----------+-- shed y3 [C1 cart] --------------+
 94     | N quay  vat(v)            |                    | canal ends z100
 80  L3 |  FLOATING MARKET boats y-0.8   (g)            |W quay| EC roof y4
 66     |  (g)            (g)       |                    |      | L2(14,0,60)
 60  ===+== south quay ============ +==== BRIDGE crown y3 (z54..58) ===
 50      west row WA roof y4 | W quay | canal | E quay | EB roof y4, ST1 stair, PB2 N to EC
 31      clay post (c)(c)    |  x-9..-3 x-3..3  x3..9  | (v)(v)   alley z30..33, PB1 over
  8                          |        |       |        | EA roof y4  L1(15,0,8)
  0                          river mouth    LANDING y0, wake (10,0,0), F1*(17,0,3)
```
`(v)` vessel; `(g)` gu host; `(c)` clay statue.

#### 1A. The landing (x 3 to 24, z -6 to 8, y 0)
- *Shape:* a stone landing stage on the river's north bank, east of the canal's mouth, steps down into the black water,
  lantern boats moored, the river stretching off the bottom of the frame. North, the first house of the east row with
  a ladder on its south face.
- *Sees on waking:* F1's censer 7 m to the right, its smoke lying in a ring on the stone; a red pool of light at the foot
  of a ladder ahead. **V1** at (6, 0, 6).
- *Enemies:* none. *Lore:* the sponsor speaks on waking (its words are the owner's; a proposal: "When the gong sounds,
  climb.").
- *Climb:* **L1**, ladder on the east row's block EA, (15, 0, 8), to its roof (y 4, x 9 to 21, z 8 to 30). Not needed;
  there to be tried. From EA's roof the plank PB1 leads on north (1B): a player who tries the ladder finds the first
  plank, and the roofs as a way along the canal.
- *See it:* 14 m out on the river at (2, -0.8, -20), a great lantern barge moored, dark, one lantern lit on its stern:
  out of reach (the side path below).
- *Kit:* `censer` (F1), `lantern_boat` x3, `lantern_string` across the canal mouth, `shophouse` blocks.

#### 1B. The lantern canal (east quay x 3 to 9, z 8 to 50, y 0)
- *Shape:* a quay 6 m wide between the canal (x -3 to 3, water y -1.5, lantern boats moored) and the east row of
  shophouses (x 9 to 21, roofs y 4): block EA (z 8 to 30), an alley east at z 30 to 33, block EB (z 33 to 50). Over the
  alley, **PB1**, a plank bridge 3 m long at x 14.25 to 15.75, joins EA's roof to EB's (**Proposal (Claude)**, 1.1 rule
  3); from the quay the player sees only its shadow lying across the alley's mouth. Across the canal, 12 m away and in frame, the west quay and the west row. Strings of amber lanterns cross
  the canal every 8 m. The eave strip along the house fronts is a kerb of planters and jars (1.1 rule 2).
- *Sees on entering:* a bloated figure at a jar on the quay's edge, drinking, its back turned.
- *Enemies:*
  - E1.1 one **vessel** at (6, 0, 22), drinking from a jar. The burst taught alone: back off when the belly glows, or
    knock it into the canal with a heavy (its cloud lies on the water, harmless to the quay).
  - E1.2 two **vessels** at (5, 0, 40) and (7.5, 0, 44), 4.7 m apart: burst one and the other chains. The lesson of
    the cloud: it runs down off the quay's edge onto the water.
- *Climb:* L1; **ST1** an outside stair along EB's west face, x 9 to 10.5, z 36 to 43 (y 0 to 4); farthest walk 14 m.
  With PB1, L1 and ST1 reach one roof walk (EA and EB).
- *Across the canal:* two **clay statues** at a watch hut on the west quay, (-6, 0, 28) and (-6, 0, 31), facing south;
  a gong rack beside the hut. Seen, not reached: the player walks past them on the other side.
- *Lore:* P1 on EA's corner, the court's edict board: "By the Emperor's grace the Elixir of Long Life is given to
  every house. Drink at the hour." (Claude's, new.)
- *Kit:* `shophouse` x6 with low-pitch roofs and ridge walks, `kerb_strip`, jars, `lantern_string`, `plank_bridge`
  (PB1).

#### 1C. The humpback bridge and the first hour (z 48 to 62)
- *Shape:* the quay meets a humpback bridge crossing the canal west at z 54 to 58: 4 m wide, flights 5 m long from each
  quay (feet at x 7 and x -7) to a flat crown 4 m long (x -2 to 2) at **y 3**. North of the bridge's foot, block EC
  (x 9 to 21, z 60 to 98, roof y 4) with **L2**, a ladder on its south face at (14, 0, 60). A lane east at z 50 to 60
  between EB and EC, crossed overhead by **PB2**, a plank bridge 10 m long at x 17.25 to 18.75 from EB's north edge to
  EC's south edge (**Proposal (Claude)**, 1.1 rule 3). L2 stands 3 m west of the plank's shadow, clear of it, so the red
  pool and the shadow band are read together from the quay: up here, and across there.
- *The first hour* (placed): crossing z 48 on the quay rings the land's first gong, whatever the clock. Held shot 1. The
  status line. Three refuges are in frame: ST1 (8 m back), the bridge crown (S, 6 m ahead), and L2 (14 m ahead); and
  PB2's shadow across the lane ahead shows that EB's roof and EC's are one walk.
- *Enemies:* E1.3 the two **clay** across the canal wake at the gong and march their watch: north along the west quay,
  over the bridge, south down the east quay to z 12, and back the same way. Walking through the mist, dark against its
  glow. The lesson, by consequence:
  - On the **bridge crown** the player is above the mist but on the clay's route: two soldiers who do not flinch climb
    onto a 4 m crown.
  - On **EB's roof** (by ST1) the player is safe from the mist, but the clay passing below within 8 m of ST1's foot come
    up the stair, one at a time. The player can hold the stair head, or cross PB2 to EC when it goes badly: the clay
    stop at the plank's end and ground their halberds, and must go back down ST1 to walk their watch.
  - On **EC's roof** (by L2, or over PB2) the player watches them pass below and turn back. Ladders and planks keep the
    clay's feet off.- *After the drum:* the clay walk back to the watch hut and set. A player may break them now (three heavies each, or a
  heavy and a blow) and the next hour's watch here is empty. The hut holds a broken statue lying beside its stand: one
  was broken before.
- *Mist:* green top 2.5; the crown is safe by 0.5 m.
- *Kit:* `humpback_bridge` (build list P6), `watch_hut`, `gong_rack`, `clay_soldier` x2, `plank_bridge` (PB2, P22).

#### 1D. The floating market (x -46 to -9, z 60 to 100)
- *Shape:* west of the bridge, a basin (water x -40 to -12, z 66 to 94) filled with boats lashed side by side into a
  floor of decks at **y -0.8**, with gangplanks up to the quays (y 0) round it: the south quay (z 60 to 66), the west quay
  (x -46 to -40), the north quay (z 94 to 100), and the canal's west quay on the east (x -9 to -3). Awnings on the boats,
  stalls of jars and dried herbs. West, a row of houses (x -60 to -46, roofs y 4); north, the Elixir House.
- *Sees on entering:* off the bridge's west foot, the boats ahead, a figure on a deck crouched over another, still.
- *Enemies:* E1.4 three **gu hosts** on the boats at (-30, -0.8, 74), (-22, -0.8, 80) and (-34, -0.8, 88). The first one
  met, (-22, 80), is 10 m from the first and 14.4 m from the third. Kill it and its crawler takes 4 s over the decks to
  the host at (-30, 74): time to catch it, if the player watches. A crawler that misses a deck and drops between boats
  sinks. E1.5 one **vessel** at the vat on the north quay, (-28, 0, 97), drinking: by the time the gu are done it may
  have drunk three times.
- *Why it suits them:* the boats are open and low, so the crawls are long and seen; the water between some boats is a
  second answer to a crawler.
- *Mist:* the boats go under first. At the first gong the gu go down into the boats' holds (their "drains"), the vessel
  into the warehouse door on the north quay.
- *Climb:* **L3**, a ladder on the west houses' south face at (-52, 0, 66); **C1**, a handcart against the shed on the
  north quay at (-14, 0, 99), up to the shed's roof (y 3); from the shed roof three wooden steps up to the tea terrace
  (y 4). Farthest walk from the market's middle to a climb point: 24 m.
- *Shortcut:* **S1-ferry** at the market's south-east, (-11, -0.8, 64): a lantern punt on a pole (below, section 4).
- *Kit:* `lantern_boat` lashed x14 (static decks), gangplanks, awnings, stalls, `bronze_vat`, `shed`, `handcart`.

#### 1E. The Elixir House (x -44 to -6, z 100 to 128)
- *Shape:* a great house where the elixir is poured: a courtyard at y 0 (x -40 to -14, z 104 to 120) with three bronze
  vats, the hall on its north (x -40 to -14, z 120 to 128: doors shut, the vessels' door at (-27, 0, 120); its steep roof
  is a landmark and fades), the east wing's flat **tea terrace** (x -14 to -6, z 104 to 126, y 4) above the courtyard's
  east side, with a stair down into it (**ST2**, x -16 to -14, z 108 to 115). A side lane west (x -46 to -40, z 104 to 128)
  leads to Ladder Street's foot.
- *Rest:* **F2** at (-10, 4, 114), reached first from the market by C1, so the player looks down into the courtyard from
  the fire before going in.
- *Sees from F2:* the courtyard 4 m below and 13 m into it: three vats, vessels at each, a gu host by the drain.
- *Enemies:* E1.6 three **vessels** at the vats (-34, 0, 110), (-26, 0, 114) and (-20, 0, 108), drinking (each a step
  closer to a 6 m cloud), and a **gu host** by the drain at (-30, 0, 104). The vats are 6 to 9 m apart: one vessel's
  cloud reaches the next only once it has drunk. The answers: burst them from the terrace with spells before they
  drink; or go down and knock one into the others; or let them drink and burst all three at once from outside 6 m, and
  then the gu is in the cloud too.
- *The placed hour:* 40 s after the player first steps into the courtyard, the first gong rings (if no hour is under
  way). The vessels go in at the hall's door. E1.7, the **gate formation** (two ji-bearers, a kneeling and a standing
  crossbowman; statues on the South Gate terrace until now) marches down Ladder Street, along the side lane, into the
  courtyard, round the vats and back. **The first volley**: a player on the tea terrace is in frame as they pass its
  west face (x -16) and is shot at; the terrace has cover (a drying rack of mats and two water jars on stands, 1.4 m).
  The player on the terrace can also strike them from above.
- *Climb:* ST2 (clay can take it, so the terrace can be held at its stair head); C1 by the shed.
- *Lore:* P3 the price board by the hall's door, with every price struck through and one word over them: "Free."
- *Kit:* `bronze_vat` x3, `elixir_hall` (closed), `tea_terrace`, `drying_rack`, `censer` (F2).

#### 1F. Ladder Street (x -46 to -38, z 128 to 152)
- *Shape:* a stepped street 8 m wide climbing north 6 m in three flights of 2 m over 4 m (27 degrees) with two landings
  6 m deep: flight 1 z 128 to 132, landing 1 (y 2) z 132 to 138, flight 2 z 138 to 142, landing 2 (y 4) z 142 to 148,
  flight 3 z 148 to 152, onto the South Gate terrace (y 6). Shophouses either side whose roofs step with the street
  (y 6 beside landing 1, y 8 beside landing 2).
- *The hour here:* the street climbs out of the mist (green top 2.5: landing 1 is bitten, landing 2 is safe). The stair
  is itself a climb point, and the gate formation's route.
- *Enemies:* E1.7 if not yet met (in the calm they stand on the terrace as statues; the player may break them in
  passing). In an hour, their volleys run down the street's length; the landings' side houses' doorsteps (kerb strips)
  give no cover, so the answer is the comb's gaps, or **DP1**, a drainpipe on the east house's south face at landing 1
  (-36, 2, 132), up to its roof (y 6).
- *Kit:* `stepped_street`, `shophouse` stepped x4, drainpipe.

#### Side path: the lantern barge (the river, z -30 to -12)
- *Seen first:* from the landing (V1), dark on the river with one lantern lit.
- *The way:* the S1 ferry. Once poled from the market down to the landing, the punt's pole points also to the barge:
  "Pole out to the barge" from the landing. 12 s each way.
- *The barge:* a long deck (x -6 to 10, z -24 to -16, y -0.8) under a frame of unlit paper lanterns (no roof: the frame
  is bare ribs), the court's elixir casks stacked on it. E1.8 two **gu hosts** asleep among the casks at (-2, -0.8, -20)
  and (6, -0.8, -18): 8.2 m apart, nothing else near. Shards in the master's chest.
- *Lore:* P2, the barge master's log, its last entries (Claude's, new): "Two thousand casks up the river for the
  court." / "The court sent them back down for the town. They said we had earned it." / "My wife drank first. She said it
  tasted of forever."
- *Mist:* the barge is under in every hour; the herb ring of F1 is 22 m away.
- *Minutes:* 2.

### 3.2 District 2: the inner court (z 152 to 392)

**The idea.** Walls inside walls (`LOOKS.md`): three courts, each behind a thicker wall, rising toward the throne. The
court stands on a terrace 6 m above the town, so its south face is the next level from the moment Ladder Street opens.
Colder and higher: moonlight on the tiles, and when the gong sounds the courtyards become wells of mist lit from inside.
The walls' walks are the roof deck; the gate piers, the drum tower's platform and the throne's top tier are the high
deck, the only floors above the yellow hour. **Levels**: the courtyards (y 6), the wall walks and flat roofs (y 10), the
high deck (y 13). Hours of 150 s; every third is yellow.

```
 z     x: -110  -64 -60  -30  -24 -18 -12  0    20 24       60
310   ===========|=Inner Wall y10 ======|Inner Gate|===================
306   HERB       |  N wall section (S3 ladder -60,10,292)    |  WATCHMEN'S
296   GARDEN     |  (broken parapet -64,10,298)              |  YARD y6
290   shrine y9.5|     route of the honour round             |  watch terrace
278   y7         |   +-- DRUM PLATFORM y13 --+  vat           |  y13 (36..48,
268   y4.5       |   |   [tower]  (v)        |               |   264..276)
262   floor y2   |   +--##grand stair##------+ DP3           |
248   (g)(g)(g)(g)|        (-23..-17)  first yellow           |
242   gate (barred)| HR3                         HR4          | door (22,6,270)
238   ===========|==Middle Wall y10 ===|Mid Gate|F3* pier y13========
226              |    formation post (-21,6,226)  DP2(-40,6,234)
214              |          [SH2 y10] vats -> =PB3= (E walk)    |
200     [SH1 y10]|   furnaces  (g)(g)   vats (v)(v)(v)          |
176   L4(-59,6,176)  ST3 stair on SH2's west face               |
170   S2 gate (-70,6,170)    OUTER COURT: MINISTRY OF ELIXIRS y6 |
166   ===========Outer Wall y10 ====|South Gate|============== HR1 ===
152                   SOUTH GATE TERRACE y6
```

#### 2A. The South Gate terrace and the gate (x -60 to -10, z 152 to 166)
- *Shape:* the terrace at Ladder Street's head (y 6, z 152 to 162), and the **Outer Wall** (z 162 to 166, x -80 to 14,
  4 m thick, its walk at y 10 with 0.6 m parapets). The **South Gate** is a gap 6 m wide (x -46 to -40) with its leaves
  open and nothing over it (1.1 rule 3); either side a gate pier (x -52 to -46 and -40 to -34), open-topped at y 13,
  reached by stairs from the wall walk. **HR1**, a horse stair up the wall's south face, x -30 to -22, rising west (4 m
  over 8 m).
- *Sees on entering:* the gate, the formation's empty places on the terrace (if E1.7 marched) or the statues. **V2** in
  the gap.
- *Climb:* HR1; the gate piers' stairs. Farthest walk 18 m.
- *Kit:* `city_wall` (P8), `gate_pier`, `horse_stair`.

#### 2B. The Outer Court: the Ministry of Elixirs (x -70 to 10, z 166 to 234, y 6)
- *Shape:* the court's workshop, 80 by 68 m but not open: a furnace yard in its middle (two furnaces at (-36, 6, 196) and
  (-24, 6, 206), three vats at (-30, 6, 186), (-40, 6, 210) and (-20, 6, 192), drain grates at (-32, 6, 200) and (-26, 6,
  214)), and two **store halls** with flat tiled roofs at y 10: SH1 west (x -66 to -52, z 176 to 200) and SH2 east (x -8
  to 6, z 190 to 214). Walled on all sides: walks at y 10 on the west (x -74 to -70), east (x 10 to 14) and north (the
  Middle Wall, z 234 to 238). **PB3**, a plank bridge 4 m long, crosses the 4 m strip of court floor between SH2's east
  edge and the east wall walk at z 201.25 to 202.75 (**Proposal (Claude)**, 1.1 rule 3); from the yard it is a shadow
  band across that strip.
- *Sees on entering:* through the gate, the furnaces' glow, officials drinking at the vats, the formation standing
  before the Middle Gate across the yard.
- *Enemies:*
  - E2.1 three **vessels** at the vats; three **gu hosts** at (-34, 6, 204), (-28, 6, 202) and (-22, 6, 218). The two
    western hosts are 6.3 m apart; the third is 17 m from either. Kill the far one first and its crawler has 7 s to
    cross; kill one of the pair and the other is Grown 2.5 s later. A vessel's cloud is the other answer: the pair stand
    6 m from the vat at (-30, 186) when its vessel has drunk twice.
  - E2.2 the **Ministry's watch**: two ji-bearers, a kneeling and a standing crossbowman, and **the first officer**,
    statues at the Middle Gate (-21, 6, 226), facing south. Their route, at the hour: a rectangle round the furnace yard,
    (-21, 226), (-46, 226), (-46, 180), (-11, 180), (-11, 226). It passes 3 m from SH2's west face.
- *The hour here:* the player's choices are SH1 (a ladder: safe from the watch, far from its route), SH2 (a stair: the
  watch comes up it, but the route runs below its west edge, where **two bronze vats** stand at (-7, 10, 198) and
  (-7, 10, 206) to be tipped onto the formation), or the wall walks. From SH2's roof the officer, 6 m below and 5 m out,
  is in reach of a spell; break him and the four who are left come at the stair one by one. SH2 is no longer a dead end:
  when the stair head is lost, PB3 takes the player off its east edge onto the east wall walk (the clay stop at the
  plank), and the walk runs north to the Middle Wall's walk and F3's pier. The plank lies on the hall's far side from
  the watch's route, so a crossing is out of their crossbows' frame unless they have come round to the east.
- *Climb:* **L4**, SH1's south face (-59, 6, 176); **ST3**, a stair up SH2's west face (x -10 to -8, z 194 to 201);
  **DP2**, a drainpipe on the Middle Wall's south face (-40, 6, 234); **HR2**, a horse stair up the Middle Wall's south
  face (x -12 to -4, rising east). Farthest walk from the yard's middle: 38 m.
- *Lore:* P4, the furnace recipe cut in the larger furnace's brick: "Cinnabar roasted gives mercury. Mercury and sulphur
  give cinnabar. Round and round: so the elixir does not end." (`LOOKS.md`: the cycle of cinnabar.)
- *Shortcut:* **S2**, a barred gate in the west wall at (-70, 6, 170) (section 4).
- *Kit:* `furnace` x2, `bronze_vat` x5, `store_hall` x2, `drain_grate`, `city_wall`, `plank_bridge` (PB3).

#### 2C. The Middle Gate (F3) and the drum tower court (x -60 to 20, z 238 to 306, y 6)
- *The gate:* the Middle Gate is a gap 6 m wide at x -24 to -18 with open-topped piers either side (x -30 to -24 and
  -18 to -12). **F3** on the west pier's top at (-27, 13, 236), above every hour, by the pier's stair from the wall walk
  or by HR2 and the walk.
- *Shape:* the court of the city's clock. In its middle the **drum platform** (x -28 to -12, z 262 to 278), 7 m up, its
  top at **y 13**, the drum tower itself (x -24 to -16, z 266 to 274) standing on it: closed, its swept roof the
  district's landmark, fading when south of the player. The platform's walkable ring round the tower is 4 m wide. Its
  **grand stair** climbs the south face, x -23 to -17, z 248 to 262 (7 m over 14 m). Its south face is a band of pale
  stone 7 m high: from the Middle Gate the player sees the stair going up into it. The court's walls have walks at y 10
  (west x -64 to -60, east x 20 to 24, the Inner Wall at z 306 to 310 with the Inner Gate gap at x -6 to 0). The west
  wall's walk is broken by a fallen section at z 286 to 290 (rubble below it, not walkable).
- *Sees on entering:* off the pier's stair, the grand stair ahead, five clay standing round the platform's foot.
- *Enemies:* E2.3 the **honour round**: two ji-bearers, two crossbowmen, an officer, statues at the platform's foot.
  Their route: a square 6 m out from the platform, (-34, 256), (-34, 284), (-6, 284), (-6, 256). And one **vessel** on
  the platform's ring at (-14, 13, 270), a court drinker who climbed to the gong.
- ***The first yellow hour*** (placed): reaching the grand stair's foot rings it, if no hour is under way; otherwise the
  next hour is yellow. The great bell with the gong; the clapper fast. The wall walks go under (yellow top 11.5); only
  the platform, the gate piers and the watch terrace are above. The player who climbs the grand stair holds the
  platform: the honour round marches the square below, its crossbows volley up at the ring (cover: two bronze vats at
  the platform's corners, (-27, 13, 263) and (-13, 13, 277), and the tower's own walls), and when the player is within
  8 m of the stair foot the soldiers turn and climb in file. The stair head is the fight. The vessel on the ring is the
  chaos: burst it near the stair head and its cloud runs down the steps, harmless to the clay and deadly to a player who
  follows it down.
- *Climb:* the grand stair (S); **DP3**, a drainpipe on the platform's south face at (-13, 6, 262) (clay cannot follow);
  **HR3** and **HR4**, horse stairs up the west and east walls' inner faces (x -60 to -56 and 16 to 20, z 252 to 260).
  Farthest walk to the high deck: 30 m.
- *Vista:* **V3** on the platform's west ring, looking west into the herb garden.
- *Lore:* P5, the hour board beside the drum: a wheel of the night's watches with a peg in it, and the yellow watch
  painted over in fresh lacquer. "The third watch is the Emperor's. Do not be below."
- *Kit:* `drum_platform`, `drum_tower` (closed), `grand_stair`, `bronze_vat` x2.

#### Side path: the herb garden of the old god (x -110 to -64, z 240 to 300)
- *Seen first:* from V3, over the west wall: a sunk garden 4 m below the courts, its terraces gone over to poison plants
  (the land's one green-leafed place, `LOOKS.md`), a worn statue at its north, and something bronze glinting on the
  stone before it. From the west wall's walk (by HR3) the player looks straight down into it; the walk ends at the
  fallen section.
- *Shape:* a bowl: its floor y 2 (z 240 to 262), terraces rising north at y 4.5 (z 262 to 274), y 7 (z 274 to 286) and
  the shrine terrace y 9.5 (z 286 to 300), stairs between them at x -90 to -86. Its gate, in its east wall at (-64, 6,
  246), is barred from inside, with a stair down from it to the floor. A poison well on the floor at (-88, 2, 258).
- *The way:* from the Inner Court (2D), along its west wall walk south, over the Inner Wall, onto the north section of
  the Middle Court's west wall (z 290 to 306), where the garden side's parapet has crumbled at (-64, 10, 298): a step
  down onto the shrine terrace. (The same walk holds S3's ladder.) So the garden is found on the way back from the
  throne, or by a player who climbs to the Inner Court's walls.
- *The mist:* a bowl fills first and drains last (2 s after the rest). Green tops at 8.5: the floor and the two lower
  terraces are under, the shrine terrace is safe; yellow covers it, and a drainpipe on the garden's north wall (-80,
  9.5, 300) climbs to the wall's top (y 13).
- *Enemies:* E2.5 four **gu hosts** in the lowest beds, two pairs: (-100, 2, 248) and (-94, 2, 252); (-78, 2, 250) and
  (-72, 2, 256). The pairs are 22 m apart: kill one of a pair and the other grows; kill both of one pair quickly
  (catching the crawler) and the other pair is still Hollow. One **vessel** drinks at the poison well.
- *The plain weapon:* on the offering stone before the statue, (-88, 9.5, 292), **the plain Twin Knives** in the land's
  make: a pair of short bronze herb knives with green patina, their grips wound with cord, playing exactly as the plain
  Twin Knives (Decided, owner, 2026-11-19, answer 7). They were the old god's gardener's knives, for cutting medicine.
- *Lore:* P6, the statue of the old god (a nod to Shennong; a placeholder name, "the Taster", is the owner's to change,
  and must not be a real deity's name): a man with a leaf at his lips, his chest carved open to show the plants working
  inside him (`LOOKS.md`). Words cut in the base (Claude's, new): "He tasted every leaf, so that no one else would die of
  one." / "The last leaf killed him. We grew it here, to remember which." / "The new one tasted it too, and asked for
  more."
- *Out:* lift the bar of the east gate: back into the drum court by the stair, 20 m from the grand stair.
- *Minutes:* 4.

#### The trial: the Third Watch (the Watchmen's Yard, x 24 to 60, z 250 to 290)

**Decided** (owner, 2026-11-19, answer 5): the trial is the Third Watch; the yard and its waves below are **Proposal
(Claude)**. A **survival trial in which the player rings the hours**, which no other
land's trial can be (`WORLD.md`: "a survival wave in another"). The god keeps the clock in his jar; here the player
learns to fight through it first, as Water's trial gave the player the tide before the fisher took it.

- *The place:* the yard where the watchmen were trained, behind a door in the drum court's east wall at (22, 6, 270)
  (seen from the platform's east ring). A square yard at y 6, walled with walks at y 10: horse stairs at its south-west
  and north-east corners (clay can climb), ladders at the south-east and north-west. In its middle the **watch terrace**
  (x 36 to 48, z 264 to 276, y 13), a ladder on its south face (42, 6, 264) and a stair down its north side to the yard
  (x 40 to 44, z 276 to 290). The watch gong stands on the terrace.
- *The rule:* "Strike the watch" rings three hours one after another, each a short one (15 s of warning, 40 s held, 10 s
  falling, 15 s of calm), the third yellow. Each hour brings the watch in through the yard's gates:
  - *First watch:* a file of four ji-bearers and an officer, who climb the corner horse stairs onto the walks.
  - *Second watch:* a formation with four crossbowmen in two ranks, volleying at the walks and the terrace, and two
    vessels released onto the walks in the calm before it.
  - *Third watch (yellow):* the walks go under; only the terrace is above. Two formations march round it, one each way,
    and the last officer climbs the north stair with his file. The trial ends when he breaks.
- *Reward:* a Memory or a Body piece (`WORLD.md`). *Lore:* the watchmen's roll on the gong's frame, every name struck out
  but the last.
- *Minutes:* 4.

#### 2D. The Inner Court and the throne terrace (x -54 to 48, z 310 to 392)
- *Shape:* the court of audience: a paved floor at y 6 (z 310 to 336, 102 m wide) with a spirit way of carved slabs up
  its middle, and the **throne terrace** rising north of it in three tiers of pale stone: tier 1 (y 8, z 336 to 392),
  tier 2 (y 10.5, z 346 to 392), both the court's whole width; tier 3 (y 13, x -20 to 14, z 356 to 392) carrying the
  **throne hall** (x -16 to 10, z 364 to 386: closed, its double roof the land's highest landmark, fading). Stairs up the
  middle of every tier face (x -8 to 2, around a carved slab that is not walked); side stairs at tier 1's ends. The side
  walls' walks (y 10) meet tier 2 at their north ends with a 0.5 m step: the high way round. Four bronze vats on tier
  2's south edge at x -40, -20, 14 and 34.
- *Sees on entering:* through the Inner Gate, the guard standing across the spirit way, shields locked. **V4**.
- *Enemies:* E2.6 the **honour guard**, awake (near the emperor the clay never sleep): two ji-bearers, two crossbowmen,
  an officer, at (-3, 6, 326) facing south. When the player passes the Inner Gate they advance on the player in
  formation; at the hour they march the spirit way up and down. E2.7 a **file of two ji-bearers** at the central stair's
  head on tier 1, (-3, 8, 338).
  - *Three answers* (the room shows all three from V4): break a gap in the front and go through to the crossbowmen and
    the officer; get round the formation's flank faster than it wheels, across the wide court, and up a side stair
    (x -54 to -50 or 44 to 48); or climb the horse stair by the Inner Gate (**HR5**, x -54 to -50, z 312 to 320, onto
    the west wall walk) and go round on the walls to tier 2, then tip the vats onto the guard below.
- *The hour here:* the throne terrace is a stair of refuges, each tier higher: green covers tier 1 (by 0.5) and leaves
  tier 2 safe; yellow leaves only tier 3. So the closer to the throne, the higher.
- *Rest:* **F4** on tier 3 behind the hall, at (10, 13, 389), beside the tomb's door (-3, 13, 392): the head of the
  spirit ramp, which descends north into the dark (a `SceneDoor` to `UndyingTomb`).
- *Lore:* P7, the emperor's stele by the tier 3 stair (Claude's, new): "I have drunk the elixir. I will not die. Neither
  will my army. Neither will you."
- *Kit:* `throne_terrace` (three tiers, P10), `throne_hall` (closed), `bronze_vat` x4, `horse_stair`.

### 3.3 District 3: the tomb under the palace (`UndyingTomb`, own coordinates, z -52 to 196)

**The idea.** The emperor's underworld: a ramp down from the throne, the bronze door, the pits where his clay army stands
in trenches under the fallen beams of their roof, a hall where a hundred rivers of mercury run across a map of the world
he ruled, and his seat at their head. No moon, no lanterns: the light is the mist that seeps down from the court and the
mercury mirroring it (`LOOKS.md`). The rule is 1.7. **Levels**: the floors (y 0), the pit walls and the river terrace
(y 3), the island and the seat (y 1.5 and 4.5).

```
 z     x: -24 -19      -5 -2  2  5          16 19   24     40
196               way of smoke (up north) -> ImmortalTower
170               EMPEROR'S SEAT y4.5  basin
140          EMPEROR'S ISLAND y1.5   basins at the corners  (mercury lake round)
136               F5* causeway landing y1.5
124   ~~~~~~~~~~~ R3 mercury ~~~~~~~ bridge ~~~~ (g) ~~~~~ sea (x30..40)
110   ~~~~~~~~~~~ R2 ~~~~~ (g)(g) ~~~ bridge ~~~ (g) (x)(x)
 92   ~~~~~~~~~~~ R1 ~~~~~~~~~~~~~~ bridge ~~~~~~~~~~~
 76   RIVER TERRACE y3 (V6)         stair down
 66   |gallery|W0| T1 |W1| T2 |W2| T3 |W3| T4 |W4| T5 |W5|   shafts at z62
 44   | S4    |  |(x)(x)|  |(j)|  |(o)(j)| |(j)(j)| =B5=|(x)|
 30   | bars  |=B2=|  |  |=B4=|  |=B3=|  |  |  |(x)|  |    beams on wall tops y3,
 18   |       |  |  |(j)|  |=B1=|  |  |  |  |  |  |    rubble heaps under them
  6   |       |s |  |  |  |s |  |  |  |  |  |  |s |    wall stairs (s) at W0, W2, W5
 -6   ========= BRONZE DOOR COURT y0, shaft S0 at (0,0,2) =========
-52              SPIRIT RAMP y12 -> 0 (traps at z -42, -32, -20)
```
`(x)` crossbowman; `(j)` ji-bearer; `(o)` officer; `(g)` gu host.

#### 3A. The spirit ramp and the bronze door (z -52 to 6)
- *Shape:* from the door (0, 12, -52) a ramp 8 m wide descends north 12 m over 46 m (15 degrees) to the **bronze door
  court** (x -12 to 12, z -6 to 6, y 0). Its walls are 5 m (they fade) and lined with clay civil officials, unarmed, who
  turn their heads to follow the player and do nothing else.
- *The traps* (Sima Qian: crossbows primed to shoot whoever enters): three **tripwires** of cinnabar thread across the
  ramp at z -42, -32 and -20, 0.1 m high, drawn as thin red lines on the floor from above. Crossing one fires a lane
  across the ramp from a mechanism in the east wall 0.4 s later (red: roll on or back). The tomb's lesson: read the
  floor. They reset at a rest.
- *The first shaft:* in the court, at the first muffled gong, a cascade of mist falls from the dark onto a cinnabar ring
  at (0, 0, 2) and runs out along the floor: the underground rule shown in one place before it matters.
- *Enemies:* none but the traps.
- *Kit:* `spirit_ramp`, `clay_official` x12, `trap_crossbow` x3, `shaft_cascade`, `bronze_door` (open).

#### 3B. The pits (x -19 to 19, z 6 to 66)
- *Shape:* five trenches running north, each 4 m wide (floor y 0), between six partition walls of rammed earth 3 m wide
  whose tops are at **y 3** (the emperor's first pit was built so: corridors between earth walls under a timber roof,
  sources). x: W0 -19 to -16, T1 -16 to -12, W1 -12 to -9, T2 -9 to -5, W2 -5 to -2, T3 -2 to 2, W3 2 to 5, T4 5 to 9,
  W4 9 to 12, T5 12 to 16, W5 16 to 19. The roof is gone; its **beams** fell across the trenches in bundles 1.5 m wide,
  lying on the wall tops: B1 over T3 at z 18, B2 over T1 at z 28, B3 over T4 at z 30, B4 over T2 at z 42, B5 over T5 at
  z 44, B6 over T3 at z 54. Under every beam the trench is choked by a heap of earth and broken clay to y 1.5: not
  walkable, and it cuts each trench into segments. So nothing walkable lies under a beam, and every rank has a segment
  for its march. Stairs up the south ends of W0, W2 and W5 (z 6 to 11, 0 to 3), each with a red lamp. The walls' north
  ends meet the river terrace (y 3) at z 66.
- *Sees on entering:* **V5**, held shot 2: the ranks in their trenches, facing south, the walls' tops running north
  into the dark like roads.
- *Enemies:* E3.1 the **pit ranks** (awake, never sleeping):
  - T1, segment z 28 to 42: two crossbowmen. T5, segment z 6 to 44: two crossbowmen. The outer trenches are the
    volleys: they shoot up at a player on any wall in frame.
  - T2, segment z 6 to 42 (open onto the court, so its rank can walk out and up W2's stair): two ji-bearers.
  - T3, segment z 18 to 54: **the officer** and one ji-bearer. While the officer stands, his drum keeps every rank in
    the pits in step: the two outer trenches volley together, two combs at once from both sides. Break him (a spell
    from a wall top, or a drop of 3 m into his segment, facing south) and the volleys fall out of step, one side at a
    time.
  - T4, segment z 30 to 66: two ji-bearers, whose segment meets the river terrace's stair: the last fight before it.
  Each rank marches its segment end to end when the player is within 12 m.
- *The way across:* along the wall tops and over the beams: W2's stair, north to B1, over to W3, north to B3, over to
  W4, north to B5, over to W5, north to the terrace. Or the west way, W0 by B2 to W1 and B4 to W2 and north. A wall top
  is 3 m wide with no parapet: a roll the wrong way is a 3 m fall into a trench segment with its rank (and a stair or
  the court within 10 m: never stranded).
- *The hour here* (1.7): five shafts over the trenches' north ends at z 62 pour at the first gong; the mist runs south
  along every trench at 2 m/s, filling the segments north to south and spilling over the heaps once it is above 1.5 m,
  to its brim at y 2.4 (green) or y 3.0 (yellow). The player sees it coming down the trenches as a glowing front. On the
  wall tops it never bites. **In a yellow hour it laps level with the wall tops and hides everything below**: the ranks
  are lost in the glow and the volleys come from shooters the player cannot see, only their lanes. Crossing in a yellow
  hour is the hardest thing in the tomb; waiting it out on a wall is allowed.
- *Lore:* P8, the makers' names stamped on the soldiers' backs and on the bricks, as the real foremen stamped theirs
  (sources); the player reads them on any broken soldier: "Made by the workshop of the left. Made by Gong. Made by Gong."
- *Kit:* `pit_trench` (P13), `beam_bundle` x6, `rubble_heap`, `clay_soldier` ranks, `shaft_cascade` x5, `everlasting_lamp`.

#### Side path and shortcut: the craftsmen's gallery (x -24 to -20, z -6 to 72)
- A corridor along the pits' west side, behind a wall with bronze bars in it: seen from W0 all the way along, out of
  reach. Its north end rises by a stair to the river terrace's west end, where a **bronze sluice gate** at (-22, 0, 60)
  closes it, its wheel on the north side. From the terrace: "Turn the wheel" (**S4**, section 4). Inside, the names of the
  craftsmen who built the tomb and were sealed in it, cut by their own hands along the wall (Claude's, new): "We built the
  rivers. We built the stars. Then they closed the door." Shards in a toolbox.

#### 3C. The hundred rivers (x -40 to 40, z 66 to 136)
- *Shape:* the **river terrace** (y 3, z 66 to 76) looks north over a hall whose floor (y 0) is a map of the world the
  emperor ruled. Three rivers of **mercury** run across it from the west wall to a mercury sea in the east (x 30 to 40):
  R1 (z 88 to 92), R2 (z 104 to 110), R3 (z 120 to 124), their surface at y -0.4, flowing east in slow silver beads. The
  floor between them is three provinces (P0 south of R1 to P3 north of R3), with model hills (mounds 2 m high, ramps at
  15 degrees) at (-20, 0, 98), (16, 0, 114) and (-14, 0, 128), and bronze cranes, geese and swans standing on the
  banks (the emperor's afterlife garden held 46 bronze waterfowl, sources). The **causeway**, the emperor's road (x -2 to
  2), runs north from the terrace's stair (x -2 to 2, z 76 to 81) over a bridge on each river (crowns y 1.5) to the
  island. Side bridges: x -24 over R1; x 18 over R2 and over R3. Shallow tributaries of mercury (0.1 m) wander across the
  provinces: walking in one slows the player to 70% and a roll in one slides 1 m further; the beads roll east, downhill.
- *Sees on entering:* **V6**: the rivers, and in them, mirrored, the painted heavens of the vault (`LOOKS.md`); the island
  and its seat at the head of the causeway.
- *Enemies:* E3.2 four **gu hosts** (the sealed craftsmen, the alchemist's gu put in them) on the provinces: A (-28, 0,
  98) and B (-14, 0, 100), together on P1; C (12, 0, 116) on P2; D (24, 0, 130) on P3. **The mercury starves the gu**: a
  crawler goes straight, and a crawler that meets mercury sinks. C's nearest host is D, 18 m away across R3: kill C and
  its crawler dies in the river. A and B share a province: kill one and the other grows. The map is the puzzle. E3.3 two
  **crossbowmen** on P1's east side at (14, 0, 96) and (14, 0, 100), facing west, volleying across the province at the
  causeway.
- *The hour here* (1.7): three shafts over the rivers at (-20, z 90), (10, z 107) and (-6, z 122). The mist falls onto
  the mercury and spreads over the provinces to its brim, y 1.0 (green) or 1.6 (yellow): the floors are bitten, the
  bridge crowns (1.5) and the model hills (2) are not. Between hours the rivers keep their low glow (rule 4).
- *Climb points:* the bridges and the hills, each with a red lamp at its foot. Farthest walk on the causeway's line: 14 m.
- *Rest:* **F5** at the causeway's north landing (x -4 to 4, z 132 to 140, y 1.5), at (3, 1.5, 136).
- *Kit:* `mercury_river` (P14), `model_hill` x3, `bronze_waterfowl` x9, `causeway`, `arched_bridge` x6.

### 3.4 The way of smoke (`UndyingTomb`, z 172 to 196)
Behind the seat (section 5), a stair 4 m wide climbs north 12 m over 22 m (28 degrees) in a cut, from (0, 4.5, 172) to
(0, 16.5, 194), lit from its head by the first moonlight since the court: the alchemist's furnace flue opens there. A
`SceneDoor` at (0, 16.5, 196) to `ImmortalTower`. No enemies. Walked in 6 s.

---

## 4. Shortcuts and loops

| | Opens from | To | How | Saves |
|---|---|---|---|---|
| S1 (the ferry) | The floating market's south-east, (-11, -0.8, 64) | The landing, (8, -0.8, -4), beside F1 | A lantern punt moored at the market: "Pole the boat" carries the player down the canal in 12 s; after the first trip it waits at either end and also poles out to the lantern barge. It does not run while the mist is up | F1 to the market without the canal, the bridge or the watch post |
| S2 | The Outer Court's west wall, (-70, 6, 170) | The Elixir House's side lane, (-44, 0, 120); F2 by ST2 | A barred gate, barred on the court side ("Lift the bar"); then a back ramp outside the wall (x -78 to -74, z 170 down to 146, 6 m over 24 m) and a lane east at z 120 to 124 | F2 to the Ministry without Ladder Street, the gate formation or the South Gate |
| S3 | The north section of the Middle Court's west wall, (-60, 10, 292) | The drum court's floor, (-59, 6, 292); F3 by the Middle Gate's pier | A ladder lying on the walk, kicked down (`Ladder` with a `DownKey`); the walk is reached from tier 2 of the throne terrace by the Inner Court's west wall | F3 to the throne terrace without the Inner Gate or the honour guard |
| S4 | The river terrace's west end, (-22, 3, 70) | The craftsmen's gallery and the bronze door court, (-22, 0, -4); F4 at the ramp's head | "Turn the wheel" of the bronze sluice gate, from the north side only | F4 to the hundred rivers without the pits |
| Drop | The tea terrace's south edge, (-10, 4, 104) | The shed roof (y 3), then C1 | A 1 m step, both ways | The cart is the terrace's second way down |
| Loop | The west wall way from tier 2 | The herb garden, its gate, the drum court | Walking | S3 and the side path share the walk |

S1 to S4 stay open after a fall and across sittings (progress keys, as Fire's). Every shortcut can be walked in an hour:
S2's ramp and lane are ground (in the town the lane is under at an hour, so it waits for the calm, like every street);
S3's ladder is a climb point; S4's gallery is under in an hour and its stair is a climb point.

**Fires apart (new player's minutes).** F1 to F2 about 6.4; F2 to F3 about 6.5; F3 to F4 about 7.0; F4 to F5 about 7.4;
F5 to F6 about 5.5 with the emperor. Within the decided 6 to 8, the last a little short because the god's door wants a
fire of its own.

---

## 5. The Undying Emperor's seat (the elite's arena)

**Decided**: the elite is the Undying Emperor, the puppet before the alchemist. **Proposal** (`WORLD.md`, `LOOKS.md`,
`steel/weapons.md` 14): the emperor who drank the elixir, the court's greatest undying corpse (the jiangshi nod), in his
own court's black with the yellow talisman written in red on his brow; he fights bare-handed in the five venoms'
styles, showing the god's five creatures first; he is a stiff corpse who cannot turn, and kicks back over his shoulder
at anyone behind him (the Scorpion's Tail, which he teaches); his seat is at the head of the tomb, up a flight of steps
over the mercury, lit from below.

**Shape.** An island in the mercury sea at the head of the causeway, its floor at **y 1.5**, 28 m east-west by 20 m
(x -14 to 14, z 140 to 160). North, the steps (x -6 to 6, z 160 to 166, 1.5 to 4.5) to **the seat** (x -8 to 8, z 166 to
172, y 4.5), a throne of black lacquer. The mercury lies all round, deep (the throw-back). The island's south edge is
open to the causeway landing (F5). On the island's four corners and on the seat stand **five bronze basins of mercury**,
each cast with one creature: centipede (-12, 1.5, 143), snake (12, 1.5, 143), scorpion (-12, 1.5, 157), toad (12, 1.5,
157), gecko (0, 4.5, 170). From each basin a groove 0.1 m deep is cut across the floor to the island's edge.

```
 z    x: -14   -8    0    8   14
172        [  SEAT y4.5  ] gecko basin
166         ## steps ##
157   scorp.                   toad
150            (E) at (0,1.5,152)
143   centi.                   snake
140   ==== causeway landing, F5*(3,1.5,136) ====
```

**How it plays with his moves** (the fight's numbers are for `poison_land.md`):
- *He hops; he cannot turn.* He moves in hops of 1.5 m (0.5 s each) and turns only between hops, 45 degrees at a time.
  The island is open, with nothing on its floor but the basins at its corners, because getting behind him is the
  answer the room must allow. **The Scorpion's Tail** is his answer to it: within 2.4 m and the 120 degrees behind him,
  a back kick over his shoulder (red). So the player learns where behind is safe: off to the side of his back, or out of
  reach, then in after the kick.
- *The five basins are his vials.* Every 25 s he hops to a basin and drinks; for 20 s he fights in that creature's
  style: the centipede's long chain of palm strikes down one hop line; the snake's coiled lunge, 6 m; the scorpion's
  two-handed pincer grab; the toad's crouch, leap and slam with a cloud spat on landing; the gecko's leap up onto the
  seat or a basin's rim and drop. These are the alchemist's vial stances and the jar's creatures, shown once at a
  man's size (`WORLD.md`, phase 1 proposal).
- *Tip a basin and deny the style.* A heavy blow tips a basin: its mercury pours down the groove to the sea as a line of
  silver beads (slow, and a roll on it slides), and that style is gone from the fight. It is the god's "destroy the
  corpse" in a small form (`WORLD.md`). The grooves cross the island, so every tipped basin adds a line of bad footing:
  the arena changes with the player's choices. The gecko's basin on the seat can only be tipped from the steps.
- *The talisman.* At half health a perfect guard against any of his strikes knocks the talisman askew and he stands
  frozen for 2 s (the jiangshi held by its talisman), once per phase.
- *No hour here.* The island stands above the mist's brim (1.0 and 1.6); at each gong the mist pours round it onto the
  sea and lights the fight from below. The gong is heard, and is only a light.
- *His ground:* `HoldGround(-14, 14, 140, 172)`. Beaten, he sits back down on the seat as if he had never left it, and
  the way of smoke behind it is open.

---

## 6. The approach to the gu jar, the jar and the crane (`ImmortalTower`, its own coordinates)

The fight belongs to a later `poison_land.md`; this section gives its room and approach, as `fire_levels.md` 3.5 and
`water_levels.md` 6 do.

### 6.1 The garden of long life and the stair (walked once)
- *The garden:* the way of smoke comes out at (0, 0, -60) in a walled garden behind the palace (x -16 to 16, z -64 to
  -40, y 0): dead peach trees (the peaches of immortality), the mist lying knee-deep and never going down. North, the
  alchemist's tower: a high terrace of four tiers, each with a stair up its south face.
- *The stair of long life:* four flights, each 6 m up over 11 m (29 degrees), 6 m wide, with landings 8 m deep: flight 1
  z -40 to -29 (0 to 6), landing 1 (y 6) to z -21, flight 2 to z -10 (12), landing 2 to z -2, flight 3 to z 9 (18),
  landing 3 to z 17, flight 4 to z 28 (24), the top terrace (x -20 to 20, z 28 to 40, y 24).
- *The last hour:* here the mist does not fall. The first gong rings as the player steps onto flight 1, and a gong every
  15 s after; at each the mist rises one tier (to y 4 by 12 s, y 10 by 27 s, y 16 by 42 s, y 22 by 57 s) and stays. All
  yellow. Walking straight up takes about 15 s; a player who stops on landing 2 to read the carving there (P9: two
  cranes carved flying up out of a cloud, and a third with its wings bound) is caught by 27 s.
- *No enemies.* Dread, the vista, the lore, the fire.

### 6.2 The top terrace (the god's door)
- **F6** at (-12, 24, 32). **V7** at its south edge, looking south: the city as a sea of glowing mist with roofs standing
  out (`LOOKS.md`, the ride out's light, seen first here).
- **The crane on its post.** At the terrace's east edge, (14, 24, 34), a bronze immortal holds a dew plate over his head
  (a nod to the bronze figures emperors set on towers to catch the dew for their elixirs). A white crane stands on the
  plate, alive, shifting its feet: the crane that should have carried the alchemist to the immortals (`WORLD.md`). It does
  not move while the alchemist lives. The hub keeps it later on its post (Decided).
- After a fall the player wakes here.

### 6.3 The laboratory and the jar
- *The laboratory* is the jar's mouth: north of the terrace, through a moon gate in a lattice screen at (0, 24, 41), a
  round floor 28 m across centred (0, 24, 56), its rim a lip 1.2 m high; shelves of jars along the rim (the south ones
  fade), the furnace in the middle burning purple and gold (the alchemist's own colours, `LOOKS.md`), the five vials on
  their shelf. The floor is the jar's wooden lid. Open to the night: nothing walkable under a roof.
- *The split* (`WORLD.md`, Proposal): he drinks his last elixir and splits into the five creatures; the lid floor gives
  way and the player falls with the furnace into the jar; overhead a bronze lid swings shut on its chain from the bronze
  immortal's arm. The lid is never drawn; the light from above is gone (`LOOKS.md`).
- *The jar:* floor at y 12, 28 m across, glazed curved walls rising to the mouth at y 24, pasted with the red five-poison
  papers, each creature pierced with five needles. The south arc of the wall fades (the south-side rule; as Water's
  beach used a slope). **Three shelves** in the wall at y 14.5 (west, north, east arcs, each 10 m long and 2.5 m deep),
  each with a ladder and a red paper lantern at its foot: the jar's climb points. The hour goes on in the jar (Proposal
  in `WORLD.md`): every 60 s the gong, and gas fills the floor to y 13 (bitten) for 20 s; the shelves and the scorpion's
  back are above it. The floor's colour is the last hour's mist.
- *His ground:* the jar.

### 6.4 The crane (the ride out)
When the last creature falls, the remains crawl back together into the man (`WORLD.md`), he says his line ("So this is
what forever tastes like."), and the choice is made over his body. Then the chain draws the lid up, the moon comes in,
and the crane drops from the plate onto the jar's rim. The prompt is "Ride the crane". **Held shot 3**: the crane lifts
the player out of the jar and out of the mist into clean air; the haze thins and the moon loses its sick halo, sharp
for the first time in the land; below, the city is a sea of mist with roofs standing out of it, and as they climb the
mist starts to sink for the last time (`LOOKS.md`, the ride out's light).

---

## 7. Build list

New pieces are made in code by a `LanternCityBuilder`, an `UndyingTombBuilder` and an `ImmortalTowerBuilder` (menu
`Clover/Build/...`), as Fire's and Water's are: static meshes with `MeshCollider`s, ink outlines where the toon material
carries one. Sizes are the plan's defaults.

### 7.1 New geometry

| | Piece | What it is | Dimensions and parameters |
|---|---|---|---|
| P1 | **Miasma** | One mist volume for the scene: a top height per district and per hour colour, moving with the clock's curve (knee at 18 s or 16 s, full at 25 s or 20 s, sinking over 10 s); hard bright top edge, translucent body lit from inside, figures in it as silhouettes; two colours, blue-green and bile-yellow (`LOOKS.md`) | `Miasma(district tops)`: D1 green 2.5; D2 green 8.5, yellow 11.5. Separate held bodies: the herb garden (fills first, drains 2 s late) |
| P2 | **Stain lines** | Two decal lines on every face the mist reaches, at the green and yellow tops | Every south face in D1 and D2 |
| P3 | **Climb point** | A `Ladder` (or drainpipe, or cart) or a `Stairs`, with its red lantern at the foot (2.4 m, a floor pool 2.5 m across) and a small one at the head; the lanterns flare at the first gong | L1 to L4, DP1 to DP3, C1, ST1 to ST3, HR1 to HR5, the grand stair, the tier stairs, the jar's three ladders. Underground, P3 with an `everlasting_lamp` |
| P4 | **Walkable roof** | A shophouse or store hall with a low-pitch tiled roof (at most 8 degrees, a 1.5 m ridge walk) or a flat tiled roof, diagonal tile rows, an eave lip of 0.3 m, south parapet at most 0.6 m, cover props in groups | D1 blocks EA, EB, EC, WA, the west houses, the tea terrace; D2 SH1 and SH2 |
| P5 | **Kerb strip** | A raised stone kerb 0.6 m high and 0.8 m deep along every house front under its eaves, carrying planters, jars and stalls | Every walkable roof's eave line over a street |
| P6 | **Humpback bridge** | An arched stone bridge: two flights and a flat crown | 4 m wide, flights 5 m, crown 4 m at y 3 (D1); the tomb's arched bridges, crowns y 1.5 |
| P7 | **Lashed boats** | A floor of boat decks at y -0.8 with gaps of water, gangplanks up to the quays, awnings | The floating market (14), the lantern barge |
| P8 | **City wall** | A wall 4 m thick with a walk on top at y 10, parapets 0.6 m, gate gaps with open-topped piers (y 13) reached by stairs from the walk | Outer, Middle, Inner walls and the side walls; the Watchmen's Yard's walls |
| P9 | **Horse stair** | A stair up a wall's face, 4 m over 8 m | HR1 to HR5, the yard's two |
| P10 | **Terraces** | `Plateau` (Fire's B1) faces of pale stone with balustrades: the drum platform (7 m, y 13), the throne terrace (three tiers), the watch terrace, the herb garden's terraces, the tower's tiers | As placed |
| P11 | **Bronze vat** | A fire vat 1.4 m tall: cover; a heavy blow from the open side tips it off an edge, landing 2 m out, cracking what it lands on and breaking a formation's lock for 3 s; stays tipped until a rest | SH2 (2), the drum platform (2), throne tier 2 (4); untippable vats in the courtyards and the Ministry |
| P12 | **Drain grate** | A grate that glows from below a second before the mist comes; a gu's home | The Ministry, the Elixir courtyard, the herb garden, the town's streets |
| P13 | **Pit trench** | Earth partition walls 3 m wide and 3 m tall with trenches 4 m wide between; fallen beam bundles on the wall tops with rubble heaps under them; stairs in chosen walls' south ends | `UndyingTomb` 3B |
| P14 | **Mercury** | A heavy liquid-metal surface that beads, rolls downhill and mirrors (`LOOKS.md`: not water, not ice). Deep: the throw-back (12 damage). Shallow tributaries 0.1 m: slow to 70%, rolls slide 1 m further. Keeps a low glowing mist on it | R1 to R3, the sea, the island's grooves, the tipped basins' lines |
| P15 | **Shaft cascade** | A column of mist falling from the unseen vault onto a cinnabar ring, at each gong, feeding P16 | S0 in the door court, five over the pits, three over the rivers |
| P16 | **Pour** | The tomb's mist: fills from a shaft's ring along the floor at 2 m/s to a brim per hall (pits 2.4 and 3.0; rivers 1.0 and 1.6), spilling over heaps, sinking into grates at the drum | 1.7 |
| P17 | **Trap crossbow** | A tripwire 0.1 m high drawn red, firing a lane across the ramp 0.4 s after it is crossed; resets at a rest | The spirit ramp (3) |
| P18 | **The emperor's island** | The island, the steps, the seat, five basins (tippable, P11's rule), the grooves | Section 5 |
| P19 | **Stair of long life and the tower top** | Section 6.1 and 6.2; the bronze immortal with the dew plate and the crane on it | `ImmortalTower` |
| P20 | **The jar** | The lid floor that gives way, the jar's interior with three shelves and ladders, the lid on its chain, the south arc fading | Section 6.3 |
| P21 | **Vistas with a direction** | Water's W19 | V1 to V7 |
| P22 | **Plank bridge** (**Proposal (Claude)**; the exception is Decided, owner, 2026-11-19) | Bare boards across a street between two roofs or a roof and a wall walk, rope rails both sides, iron cleats on the lips; drawn whole while the player's floor is level with it or above, and only as its cast shadow (rails included) while the player's floor is below it; a floor for the player, an edge for the clay | 1.5 m wide, rails 0.6 m; PB1 3 m and PB2 10 m at y 4 (D1), PB3 4 m at y 10 (D2) |

### 7.2 New behaviour the rooms rely on (for the code worker)

| | What | Where it is used |
|---|---|---|
| C1 | **GongClock**: the hour's phases (calm, first gong, rising, second gong, held, drum, falling), its length by district, yellow every third in D2, placed hours by trigger, a restart into the calm at a rest or a fall; events for each phase. Build it on `HeatSchedule` (which already has calm, warning and swell) by adding the falling phase and the yellow count | The whole land |
| C2 | **Mist bite**: the knee rule (the top 0.5 m or more above the floor under the player), green and yellow doses, the herb ring at every `RestPoint`; built on `NoonHeat`'s burn | Everywhere |
| C3 | **Roof islands**: islands and their stair feet as data from the builder; an enemy's two-leg way to a player on an island (to the stair foot, up, at the player); the edge stop and the cut-off pose; clay never take ladders, carts, drainpipes or plank bridges. **Plank bridges** (**Proposal (Claude)**): islands joined by a plank are one for the player and stay two for the clay (they head for the stair foot of the island the player stands on, and stop at a plank's end); vessels keep off planks; clouds fall through them; a crawler crosses only along one; no bolt push on a plank | D1, D2, the trial |
| C4 | **The vessel**: wander, drink (cloud +1 m per drink, three), the swell and burst, the heavy cloud flowing down slopes and off edges, the chain, going indoors at the first gong, bursting in the mist after 4 s. Check `AshenBurst` for the swell; the cloud is new | D1, D2, the trial |
| C5 | **The gu**: three stages and their moves, the crawler (straight, 2.5 m/s, killed by one blow, sinking in water and mercury, drying with no host in 12 m), drains at the gong | Everywhere |
| C6 | **The clay**: statues between hours (`Enemy.MakeDormant` that takes damage and cracks without waking), awake near the emperor; crack and shatter; formations on routes, the lock (from `ShieldWall`), the wheel at 30 degrees a second, the gap closed by the officer, the dissolve when he breaks; the volley combs, fired only from on screen; the officer's drum keeping ranks in step | Everywhere |
| C7 | **Vat tip** (P11) | D2 |
| C8 | **Pour** (P15, P16) | The tomb |
| C9 | **Mercury** (P14): the throw-back, the slow and the slide, beads rolling downhill along a groove | The tomb |
| C10 | **Ferry**: a moving float on a fixed line (Water's C5, moving floors) | S1 |
| C11 | **The emperor**: hops and turns, the Scorpion's Tail, the basins as stances, the tipping, the talisman | Section 5 |
| C12 | **The Third Watch**: the trial's three short hours and their waves | The Watchmen's Yard |
| C13 | **The last hour**: the tower stair's rising tiers that never fall | 6.1 |

### 7.3 What is reused

| Piece or code | Where |
|---|---|
| `Ladder` | Every ladder, drainpipe and cart; S3 (with `DownKey`) |
| `BarredDoor` | S2, the herb garden's gate |
| `HeatSchedule`, `NoonHeat`, `SunlightZones` (calm areas) | The clock and the bite (C1, C2) |
| `ShieldWall` | The formation's lock only (C6) |
| `AshenBurst`, `BuriedMound` | The vessel's swell (check before writing C4) |
| `LandZones`, `TopDownCamera.SetFraming` | V1 to V7 and the held shots |
| `Fadeable`, `OcclusionFader` | Every steep roof, house front and hall, the jar's south arc; the plank bridges' shadow-only draw from below (by the player's floor height, not by occlusion: check whether the fader can switch on that before writing new code) |
| `RestPoint` | F1 to F6, as censers |
| `WeaponPickup` | The plain Twin Knives |
| `LandVoice` progress keys | S1 to S4, the garden's gate, broken statues until a rest |
| `Enemy.MakeDormant`, `Enemy.HoldGround` | The clay statues; the emperor's ground |
| `FallCatch` | Under every scene; the throw-back's last firm ground |
| `SceneDoor` | The tomb's door, the way of smoke |
| `DistrictMood` | The moon and the lanterns by district |
| Fire's B1 `Plateau`, B3 `Ramp`, `Stairs` | Terraces, ramps, every stair |

### 7.4 New kit (Blender, through the `blender-prop` skill)

Shophouse blocks with low-pitch roofs and ridge walks; steep hall, gate and tower roofs with upturned eaves; the drum
tower; the throne hall; moon gates and lattice screens; city wall sections and gate piers; the humpback bridge; lantern
boats, the lantern barge and the punt; amber paper lanterns (frames and tassels in cinnabar) and the red climb lanterns;
the censer; plank bridges with rope rails and cleats; bronze vats and elixir vats; furnaces; drain grates; handcarts; clay downpipes; the clay soldiers (ji-bearer,
crossbowmen kneeling and standing, officer with drum, civil official) with flakes of paint; the vessel, the gu host
in three stages and the crawler; the emperor and his seat; the five basins; bronze waterfowl; the old god's statue;
the bronze immortal with the dew plate; the crane; the jar and its lid. No Fire or Water kit carries over except as
grey-box stand-ins.

---

## 8. Pacing

Minutes for a new player on a clean pass, counted as `fire_land.md` 3.1 counts them (a weak enemy about 30 s, a strong
one 45 s, a group a little less each), with the held mist of each hour met counted where it is spent fighting, and
waits counted in full.

| Stretch | Minutes (new) | Experienced | Tension | What it teaches |
|---|---|---|---|---|
| The landing, F1, the first ladder | 1.0 | 0.5 | Rest | Climb points and their lanterns |
| The lantern canal | 1.2 | 0.5 | Rising | The vessel's burst; the heavy cloud; the chain |
| The bridge and the first hour | 1.7 | 1.0 | Dread, then wonder | **The hour**; clay march through the mist; ladders keep their feet off; statues in the calm |
| The floating market | 2.2 | 1.0 | Rising | The gu: the crawl, the order of killing |
| The cart, F2 | 0.3 | 0.2 | Rest | |
| The Elixir courtyard and its hour | 2.0 | 0.9 | Peak | Vessels that drink; the first volley; the roof as artillery |
| Ladder Street | 1.0 | 0.4 | Rising | A stair that climbs out of the mist |
| **District 1** | **9.4** | **4.5** | | |
| The South Gate, the Ministry, F3 | 3.5 | 1.5 | Peak | The officer; tipping vats; choosing an island by its climb point |
| The drum tower court and the first yellow hour | 3.0 | 1.3 | Peak | The yellow hour; the high deck; holding a stair head |
| The Inner Gate | 0.5 | 0.2 | Rising | |
| The honour guard | 3.0 | 1.3 | Peak | Clay that never sleep; three ways round a formation |
| The throne terrace, F4 | 0.5 | 0.3 | Rest | A stair of refuges |
| **District 2** | **10.5** | **4.6** | | |
| The spirit ramp and the door | 1.2 | 0.5 | Dread | The traps; the mist falls here |
| The pits | 3.5 | 1.5 | Peak | Walls and beams; the pour; the officer's step; blind volleys in yellow |
| The hundred rivers, F5 | 2.7 | 1.2 | Rising | Mercury; the map starves the gu |
| The Undying Emperor | 4.0 | 2.0 | Peak | Behind him and the Scorpion's Tail; the five styles; denying a style |
| The way of smoke | 0.5 | 0.2 | Relief | The first moon |
| **District 3** | **11.9** | **5.4** | | |
| The garden and the stair of long life, F6 | 1.0 | 0.5 | Dread | The last hour |
| The alchemist (both phases) | 11 | 8 | Climax | (`poison_land.md`, later) |
| **Clean pass** | **43.8** | **23.0** | | |
| Side paths: the lantern barge 2, the herb garden 4, the craftsmen's gallery 0.5, the Third Watch 4 | + 10.5 | + 5 | | |

That sits a minute under `WORLD.md`'s proposal of about 45 for the middle solo lands. Fire's plan came to 41 and Water's
to 40; this one is longer by the clock. Levers if play runs long, in order: E2.7's file on tier 1 (0.5), the Ministry's
third gu (0.4), the second pair of crossbowmen in the hundred rivers (0.5).

**Enemy count.**

| District | Main path | Count |
|---|---|---|
| D1 | 7 vessels, 4 gu, 6 clay (2 at the watch post, 4 in the gate formation) | 17 |
| D2 | 4 vessels, 3 gu, 17 clay (5 in the Ministry's watch, 5 in the honour round, 5 in the honour guard, 2 on tier 1) | 24 |
| D3 | 4 gu, 12 clay (10 in the pits, 2 in the rivers) | 16 |
| Elite | The Undying Emperor | 1 |
| Side paths | Barge 2 gu; herb garden 4 gu, 1 vessel; the Third Watch's waves | 7 and the trial |
| **All** | | **57 on the main path, the emperor, and the alchemist** (Fire 40, Water 33) |

More than Fire's because 35 of them are clay, who between hours in the town and outer courts are statues the player
walks past or breaks unopposed, and who in the hours are often fought from another deck or avoided by the choice of
island. A new player is in close combat with about 35. It is the most chaotic land, as decided.

---

## 9. Questions for the owner

**Answered (owner, 2026-11-19).**

1. **The hours** last 180 s in the town and 150 s in the court, and every third hour in the court is yellow.
2. **The clay soldiers** stand as breakable statues between hours, and are always awake near the emperor.
3. **The roof deck** may cross streets by **narrow plank bridges**, drawn shadow-only from below so the street stays
   visible. This is the owner's chosen exception to the roof rule, for this land.
4. **The climbing lanterns** are the only red lanterns in the land, smaller and browner than the robe; every other
   lantern is amber.
5. **The trial** is the Third Watch.
6. **In the tomb** the mist pours down visible shafts, rises to a set level and sinks into the mercury at the drum.
7. **The plain Twin Knives** are bronze herb knives in the old god's herb garden.

Where the plan below differs from these answers (the plank bridges especially), the answers win; the plan is to be
brought into line.

The body of the plan has been brought into line with these answers (2026-11-19).



Each has two or three options, the recommended one first.

1. **How long is the hour, and how often is it yellow?**
   - **(a) D1 180 s, green only; D2 150 s with every third hour yellow; the tomb hears the court's hours; the tower's
     last hour never falls.** Recommended. A long calm and a short dark is the rhythm that worked for *Dying Light*,
     whose night was about 7 minutes against a 64-minute day, with a warning sound three minutes before it fell: rare
     enough to dread, long enough to matter. Here the mist is about a quarter of D1's time and a third of D2's.
   - (b) One clock for the whole city, 240 s, yellow every other hour. Simpler to learn; the first district loses its
     gentle start and the court its rising pressure.
   - (c) Shorter hours everywhere (90 s), so the player climbs about once a minute. Climbing becomes the walk itself; it
     risks the "wait for the cycle" the owner ruled out.
2. **What do the clay do between hours?**
   - **(a) In the town and the outer courts they are statues between hours that can be broken but do not wake; near the
     emperor (the Inner Court and the tomb) they never sleep.** Recommended. The calm and the mist become two different
     fights, the calm can be spent preparing for the mist, and the escalation is the story. *Dying Light* works the same
     way: the day's infected and the night's Volatiles are different threats, and the night is the one that hunts.
   - (b) Always awake, marching their routes. More pressure everywhere; the hours stop changing who is on the streets.
   - (c) They rise out of the mist only, and sink back into it. Eerie, but it makes them a poison thing, which the owner
     decided they are not.
3. **May the roof deck cross a street?**
   - **(a) No: the decided "nothing walkable under a roof" is kept strictly; roofs join only over water, closed gaps and
     wall tops, and streets are crossed in the calm.** Recommended. It keeps the lesson of Sun City's playtest whole, and
     the islands make the mist hours a choice (which island, which climb point) rather than a free second map. Height in
     top-down and isometric games is hard to read once floors overlap: developers' own advice is to keep high things
     behind low ones and give everything a shadow on the floor it stands on.
   - (b) Narrow plank bridges (1.5 m) over streets, drawn shadow-only from below, so the roof deck is one network. A
     freer rooftop city, closer to the parkour of *Dying Light*; one exception to a decided rule, and a hidden player
     under every plank.
4. **The red lanterns of the climb points beside the amber lanterns of `LOOKS.md`.**
   - **(a) The climb lanterns are the only red lanterns in the land: smaller, a step browner than the robe, with a red
     floor pool, flaring at the first gong; every other lantern is amber paper.** Recommended. Red keeps one meaning,
     "climb here", and the robe still outranks it on screen.
   - (b) Amber like the rest, with a cinnabar tassel and a red floor pool. Calmer; harder to find in a hurry.
   - (c) Red paper talismans on the climb point instead of lanterns. Fits the court's seals; poor at night from above.
5. **What is the land's trial?**
   - **(a) The Third Watch: the player strikes the watch gong and survives three short hours on a yard of walls and one
     high terrace while the clay watch comes in, the third hour yellow.** Recommended. It is the one trial no other land
     could have, and it teaches the god's own clock before the jar. *Hollow Knight*'s Colosseum of Fools changes its arena
     between waves; this changes it with the mist.
   - (b) The Gu Pit: a sealed yard of eight gu where the last one alive becomes a king with everything it ate. Strong,
     but it spends the god's "last one alive" before the god.
   - (c) A duel with a lone clay general on a floor where mercury runs downhill. A good fight; Water's trial is already a
     duel.
6. **How does the miasma rule change underground?**
   - **(a) The court's gongs are heard; the mist pours down shafts at known spots, runs along the floor and fills the low
     places to a brim, lies on the mercury always, and sinks into the mercury at the drum.** Recommended. The verb stays
     *climb*, but the warning becomes a place you can see coming, which is new. Poison swamps in FromSoftware's games
     (Blighttown, the Lake of Rot) are remembered as slogs where the poison is simply always there; a mist that comes and
     goes, from a source the player can see, keeps the tomb a puzzle and not a toll.
   - (b) No clock underground: the mist lies fixed in the low places and the danger is only the mercury. Calmer; the
     tomb stops being part of the land's rule.
   - (c) The tomb fills a level at every hour and only empties at a rest. Real pressure; it punishes the careful player.
7. **The land's plain weapon: the plain Twin Knives, in the old god's herb garden.**
   - **(a) There, in this land's make (a pair of bronze herb knives, green with age, playing exactly as the plain Twin
     Knives).** Recommended: Consume over the alchemist gives the plain knives only if they were not found
     (`steel/weapons.md` 0.1), so the land should offer them, and the old god's garden ties the weapon to the land's
     deepest lore. As with Water's Chain Sickle, the look changes, not the weapon.
   - (b) There, in the plain knives' standard look. Less modelling; one odd object in the court.
   - (c) On the lantern barge instead, found earlier and more easily. Sure to be found; the garden keeps only lore.

---

## Sources

- *Dying Light*'s night (about 7 minutes against a 64-minute day, the warning before it, rooftops and safe zones):
  [Prima Games](https://primagames.com/tips/dying-light-survive-night-get-more-xp),
  [Dying Light wiki, Dark Area](https://dyinglight.fandom.com/wiki/Dark_Area),
  [GameFAQs thread on the cycle's length](https://gamefaqs.gamespot.com/boards/716561-dying-light/71173623).
- The five night watches, watchmen with gong and clapper, drum and bell towers:
  [Story and Hour, the five watches](https://storyandhour.com/journal/night-watchman-five-watches/),
  [Baidu Baike, bell tower and drum tower](https://baike.baidu.com/en/item/bell%20tower%20and%20drum%20tower/1525576),
  [Baidu Baike, night watchman](https://baike.baidu.com/en/item/Night%20Watchman/1513284).
- The terracotta army's formations (the first pit's vanguard and corridors, the second pit's kneeling and standing
  crossbowmen firing in turn, the makers' stamps): [Travel China Guide](https://www.travelchinaguide.com/attraction/shaanxi/xian/terra_cotta_army/strategy_1.htm),
  [Live Science](https://www.livescience.com/25510-terracotta-warriors.html),
  [Wikipedia, Terracotta Army](https://en.wikipedia.org/wiki/Terracotta_Army),
  [Antiquity, Qin structural timbers](https://www.cambridge.org/core/journals/antiquity/article/qin-structural-timbers-and-the-first-emperors-mausoleum-xian-china/06C0DD897F1729B17730EE55F58EDACE).
- The tomb in Sima Qian (mercury rivers, crossbows set against intruders, everlasting lamps) and the bronze waterfowl of
  pit K0007: [Smarthistory](https://smarthistory.org/tomb-first-emperor-qin/),
  [Wikipedia, Mausoleum of Qin Shi Huang](https://en.wikipedia.org/wiki/Mausoleum_of_Qin_Shi_Huang),
  [Discover Magazine on the traps](https://www.discovermagazine.com/the-booby-traps-of-qin-shi-huangs-tomb-fact-fiction-or-something-even-better-44580),
  [Travel China Guide, the accessory pits](https://www.travelchinaguide.com/cityguides/xian/terracotta/pits.htm).
- Poison areas in soulslikes: [Wikipedia, Blighttown](https://en.wikipedia.org/wiki/Blighttown),
  [a defence of the Lake of Rot as level design of mood](https://doshmanziari.substack.com/p/the-best-of-elden-rings-level-design).
- Reading height from above (shadows on the floor each thing stands on, high things behind low ones):
  [GameDev.net, isometric depth problems](https://gamedev.net/forums/topic/551977-isometric-depth-problems/4550476/),
  [TIGSource, verticality in isometric games](https://forums.tigsource.com/index.php?topic=60909.0).
- Optional arenas that change between waves: [Hollow Knight's Colosseum of Fools](https://hollowknight.fandom.com/wiki/Colosseum_of_Fools).
- Soulslike loops and shortcuts opened from the far side: [The Level Design Book, Undead Burg](https://book.leveldesignbook.com/studies/sp/undead-burg).
