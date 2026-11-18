# Fire's land: the sun-king's city (build specification)

Written by Claude on 2026-11-17 while the owner slept, from `docs/design/WORLD.md`, `docs/GAME.md` and the code as it
stands. It is for the programmer who builds the land tonight and for the owner who reads it in the morning.

**How to read the statuses.** Section 1 holds the owner's words, quoted from `docs/design/WORLD.md` with the status
that file gives them. **Everything after section 1 is Proposal (Claude)** unless it sits in quotation marks with a
status beside it. No number here has been played. Where a rule needs code that does not exist yet, it says **New code**.

Contents: 1 the owner's decisions; 2 the land's feel; 3 the map; 4 the ordinary enemies; 5 the elite; 6 the god;
7 lore; 8 the gift; 9 build order for tonight; 10 where this departs from the code; 11 what the owner should confirm.

---

## 1. The owner's decisions

Quoted from `docs/design/WORLD.md` (the think session of 2026-11-16 and 17). The status word is that file's.

| # | Status and date | The words |
|---|---|---|
| 1 | Decided (user, 2026-11-17) | "the first land is Fire's, the sun-king's city. The player starts there with fists and no element, and takes the first element from the Fire god. This replaces the earlier decision in `docs/GAME.md` that the first run starts with fists and one chosen element." |
| 2 | Liked (user, 2026-11-17) | "four kinds of corruption by fire, of the six Claude offered. *The wick*: they burn and are not consumed; the wrapped dead smoulder like candle wicks and the living are charred with ember light in the cracks; they grab and hold to share the burning. *The gilded*: the sun-king promised his people the golden flesh of gods and it came true as molten gold, masks fused to faces; the more gold, the higher the rank. *Sun-blind*: they looked at their god until their eyes burned out, and hunt by heat and by shadow. *The thirst*: fire as drought; dried husks that crave anything wet and may go for the player's flasks." |
| 3 | Liked (user) | "each land is about three districts, one elite and the god, with side paths that hold the lore and the land's plain weapon." |
| 4 | Proposal (Claude, 2026-11-17), in the file under the owner's "Liked as first drafts" | "1. *The dry river*: where the player wakes. A dead riverbed, cracked mud, reed boats lying where the water left them, a harbour with no water. The thirsting. Fists only; the first rest fire. Landmark: a great barque stranded on the mud, which is the ride out at the end. Side path: a sealed well shrine. 2. *The city of wicks*: mud-brick streets and the embalmers' quarter. The wicks. Narrow alleys, grabbing hands; the plain spear is found at a gatehouse. Side path: painted tombs that show the star falling in the desert and a mortal carrying it home, and under the paint an older god of the hearth and the dawn. 3. *The golden way*: the avenue of seated statues, their faces recut from the old god's to the king's, and the hall of columns before the court. The gilded, and the sun-blind in the columns' light and shade. *Elite*: one of the wholly gilded, a champion cast in gold who holds the last gate with a spear and shows the god's patterns before the god does. *The god's room*: an open sun-court with an obelisk whose shadow turns like a clock hand. First a spear duel with the sun-king as a man. Then the form: the scarab that rolls the sun across the court and the falcon that carries it up, one creature in two shapes, as the sun was a beetle at dawn and a falcon at noon. The sun is the shared piece of the fight. When the whole court burns, the obelisk's moving shadow is the only safe ground. The player has no choice here but to take Fire. Then the barque." |
| 5 | Leaning (user, 2026-11-17) | "the first land takes a brand-new player about 30 to 40 minutes and an experienced player about 20. Claude's reading: that is the time of a clean pass, the walk and the fights won, with deaths and retries on top; side paths are extra and optional." |
| 6 | Leaning (user, 2026-11-17) | "Death returns the player to the last rest fire in the land they are in, in the manner of Elden Ring and Sekiro: the sponsor turns time back a little, not all the way. Gods already beaten stay beaten and the build is kept for the whole climb. From the per-land-run option it keeps a cost for dying: the land's enemies come back and (Claude's proposal) the unspent embers being carried are lost." |
| 7 | Leaning (user, 2026-11-17) | "lands need larger first areas to explore and to carry the land's lore, even if sparse in the manner of Elden Ring." |
| 8 | From the gods' table | Fire, Egypt: "A nod to the sun god: a sun-king"; weapon "Great Spear as a sun-spear (proposal)"; godly form "Falcon and scarab in one"; ride out "The solar barque (proposal)"; status "Egypt, the sun-god nod and the two-in-one form are the user's idea". |
| 9 | Decided (user) | "every godly second form is a mythological or godly creature." |
| 10 | Decided (user) | "each god carries a personal, finer version of its weapon, in the way the best weapons in Elden Ring are boss weapons; the player's ordinary version is plainer." |
| 11 | Decided (user) | "each god's name is an original name with the sound and feel of its source, a nod and never the deity's own name." |
| 12 | Proposal in the file, not yet the owner's | "because the player has nothing to grow yet, the first god is simply taken from; the take-or-consume choice first appears at the second god." and "It is always noon there." and "the sponsor tells one story aloud; the carvings and remains in each land quietly tell another, about the god murdered there before the usurper came." |
| 13 | Open in the file | "which choice after a god gives the god's own weapon. `docs/GAME.md` has Take giving it and Consume giving a plain copy; the user's note of 2026-11-17 said 'only when you consume their stuff do you get to keep their weapon'. To be confirmed." |

From `docs/GAME.md`, still standing and used below: the voice ("grim and sparse, in the manner of Sekiro ... few
words, plain and weighty, no jokes"); the look ("A dark, muted world ... in which the elements are the only saturated
colour"); the zone "is for atmosphere only and applies no status effect"; three flasks; every attack drawn on the
ground before it lands, pale for what a held guard stops and red for what it does not.

---

## 2. The land's feel

**One sentence.** A city standing still under a sun that will not move: bone-pale mud brick, dust, hard short
shadows, and in all of it only two strong colours, the orange of what is burning and the gold of what has been poured.

**Light.** It is always noon (item 12). One key light, nearly straight down, leaning a little so that every shadow
falls north, the way the player walks.

| Mood `Noon` (a new entry beside `StormNight` in `LookBuilder`) | First value |
|---|---|
| KeyColour | (1.00, 0.95, 0.84) |
| KeyIntensity | 1.5 |
| KeyEuler | (78, 0, 0): light travels down and north; a 9 m roof throws its edge 1.9 m north |
| Ambient | (0.36, 0.33, 0.38): the shade is cool, a grey-violet, never black |
| Fog | (0.62, 0.56, 0.47), density 0.012: heat haze, pale, hides the far end of a street |
| Background | (0.78, 0.74, 0.66): a white-hot sky with no blue in it |
| Storm | false. No rain, no bolts. `AreaDirector` storm strikes are off (`DrawsStorm = false` on every kind here) |
| Exposure, Saturation | 0.15, -10: the world a little bleached |

**Palette.** The world is muted. Fire-orange and gold are the only strong colours.

| Material (folder `Generated/SunCity`) | Colour | Used for |
|---|---|---|
| `m_sun_mud` | (0.52, 0.47, 0.40) | cracked riverbed, with a crack texture |
| `m_sun_brick` | (0.58, 0.52, 0.44) | mud-brick walls and houses |
| `m_sun_brick_dark` | (0.40, 0.36, 0.33) | wall feet, retaining walls, alley floors |
| `m_sun_stone` | (0.66, 0.62, 0.55) | dressed stone: quay, avenue, hall, court |
| `m_sun_stone_new` | (0.76, 0.73, 0.66) | recut faces and anything the king had made: a shade too pale, so the eye catches it |
| `m_sun_shade` | (0.30, 0.29, 0.36) | painted shade on floors where the rule needs it read (hall, court) |
| `m_sun_reed` | (0.50, 0.46, 0.30) | boats, mats, awnings |
| `m_sun_wrap` | (0.70, 0.67, 0.60) | linen wrappings, natron heaps a little whiter |
| `m_sun_char` | (0.09, 0.08, 0.09) | charred flesh, burnt timber |
| `m_sun_gold` | (0.95, 0.70, 0.18), lit | **strong**: the gilded, the pyramidion, the god |
| `m_sun_ember` | (1.6, 0.5, 0.1) emissive | **strong**: cracks in wicks, hot floor, rest fires, the sun |
| `m_sun_blue` | (0.20, 0.33, 0.50) | used three times only: the well's lid, the first tomb wall, the tears on the third |

Nothing else is saturated. No red banners, no green. The player (blue and orange, as built) is the only other colour.

**The stage rule about shade.** Shade is the land's only shelter and its only cool colour, and wherever the land
makes a rule about light, **shade is the safe side**. Two places make such a rule, and they agree: in the hall of
columns the sun-blind cannot find someone in shade (section 4.4); in the sun-court the obelisk's shadow does not burn
(section 6.3). Everywhere else shade is only how the place looks. Shadows are hard-edged, short and fall north.
Only one shadow in the land moves, the obelisk's, and it moves because in his own court the king turns the sun
himself.

**Sound.** Not yet (sound is set aside in `docs/GAME.md`). Notes for later only: no wind; dry creaks; the wicks hiss
like damp wood; gold rings when struck; the hall is silent; the court has one low held note that rises with the blaze.

---

## 3. The map

### 3.1 Sizes, and how the minutes were worked out

From the code: the player runs at 6 m a second (`PlayerController.MoveSpeed`), rolls 4.5 m, has 100 health, 100
stamina and three flasks of 45. Fists land 36 damage and 57 stagger in a four-blow chain of about 1.4 s; the Great
Spear lands 36 and 40 in a three-blow chain of about 1.6 s, with a heavy of 24 damage and 32 stagger. The castle is
130 m long and holds twelve enemies. At 6 m a second walking is nothing: the whole land end to end is two minutes of
running. **The minutes are fights.** A new player takes about 30 s over a weak enemy and 45 s over a strong one,
counting the approach and the wait for an opening; an experienced one about 12 and 20.

| Stretch | Length north | Main-path enemies | New player | Experienced |
|---|---|---|---|---|
| 1. The dry river | 140 m | 11 thirsting | 7 min | 3 min |
| 2. The city of wicks | 150 m (path about 190 m) | 13 wicks, 1 thirsting | 9 to 10 min | 4.5 min |
| 3. The golden way and the hall | 180 m | 8 gilded, 4 sun-blind | 9 to 10 min | 5 min |
| The elite's gate | 30 m | 1 champion | 3 min | 1.5 min |
| The sun-court | radius 20 m | the god, two phases | 8 min | 6 min |
| **Clean pass** | about 520 m | 38 and the god | **36 to 38 min** | **20 min** |
| Side paths (optional) | | 2 thirsting, 2 wicks | + 4 to 5 min | + 2 min |

That is at the top of the owner's 30 to 40. The first lever if it plays long: take one gilded out of E3.2 and one wick
out of E2.4 (about 1.5 minutes).

**Scenes.** Two, as the castle and the summit are two: `SunCity.unity` (the three districts and the elite's gate,
built by a new `SunCityBuilder`, menu `Clover/Build/Sun City`) and `SunCourt.unity` (the god, built by
`SunCourtBuilder`, menu `Clover/Build/Sun Court`). The champion's `SealedWay` leads from one to the other.

**Axes**, as in `CastleBuilder`: x east, z north, y up; north is up the screen. The player wakes at (0, 0, 4).
Floors are `Slab`s, walls `Wall`s, columns and wells `Disc`s and cylinders, statues stacked `Block`s. **Camera rule
kept from the castle:** nothing taller than 1.6 m stands on the south (camera) side of where the player fights; tall
things stand to the north, east and west. Every wall along the south side of an east-west alley is a broken wall
1.4 m high.

**Rest fires.** Six: `Rest(group, director, foot, returnTo)` as in the castle. Death returns the player to the last
one lit (item 6): that is what `AreaDirector.OnRevived` already does in a scene with no `RunEnder`, so **these two
scenes do not call `PlayerBuilder.AddRunEnd`**. Before any fire is lit the player comes back to the start point.

| Fire | Where | Position |
|---|---|---|
| F1 | beside the first reed boat, in sight of where the player wakes | (4, 0, 12) |
| F2 | the harbour quay, at the head of the stair | (-8, 2, 126) |
| F3 | the Embalmers' Gate, beside the pinned soldier and the spear | (5, 2, 240) |
| F4 | the foot of the golden way | (-7, 2, 296) |
| F5 | the north porch of the hall, before the champion's line | (8, 4, 464) |
| F6 | the forecourt of the sun-court (in `SunCourt.unity`) | (0, 0, -26) in that scene |

### 3.2 District 1: the dry river

**Size:** 60 m wide (x -30 to 30), 140 m long (z 0 to 140). The riverbed is the floor at y 0, 28 m wide (x -14 to
14), cracked mud. Either side a bank 1.5 m high (retaining `Wall`s of `m_sun_brick_dark`, the bank tops `Slab`s of
dust at y 1.5) that the player cannot climb except at the west gully. North, the harbour: a basin (x -24 to 24,
z 100 to 120, y 0) and a stone quay across the whole width at y 2 (z 120 to 140) with one stair in the middle
(`Stair`, 8 m wide, run 6 m, rise 2 m, foot at z 114).

```
 z140 +===================== QUAY  y2 ======================+   -> street north (x 0)
      |   F2 *            [mooring stones]        C1         |
 z120 +--------------------###stair###----------------------+
      |  jars      (t)(t)(t)  dry cistern        crane       |   E1.5  harbour basin
 z100 |.........basin wall, gap 12 m in the middle..........|
      | W |                         ________________   | E  |
      | E |              (t) on deck/ THE BARQUE    \  | A  |   E1.4  under the hull
  z80 | S |<== gully        (t)(t)  \_______________/  | S  |
      | T |   to the well         in the hull's shade  | T  |
      |   |                                            |    |
  z62 | B |        (t)   [great jar, tipped]   (t)     | B  |   E1.3
      | A |                                            | A  |
  z44 | N |   reed boat ]>      (t)                    | N  |   E1.2
      | K |                                            | K  |
  z26 |   |          (t) kneeling at a dry pool        |    |   E1.1
      |   |   reed boat ]>   F1 *                      |    |
   z4 |   |              @ wake                        |    |
   z0 +---+--------------------------------------------+----+
      x-30 -14                  0                     14   30
```

**The path.** Straight up the riverbed, round the barque, through the gap in the basin wall, up the stair. The quay
and the pale gold tip of the far obelisk are in view from where the player wakes: the eye is led north.

**Landmarks.**
- *Reed boats* (four): a hull of three tapered `Block`s of `m_sun_reed`, 6 m long, lying tilted. Cover, and scale.
- *The great jar* (z 62): a cylinder 2 m across on its side, cracked, a dark dry stain under its mouth.
- *The barque* (centre (10, 0, 84), heading 20 degrees off north): 30 m long, 7 m in the beam, 4 m to the rail,
  heeled 12 degrees toward the east bank so its deck faces the camera. High curled prow and stern posts (8 m). A
  cabin of four posts and an awning. The hull throws the district's one big patch of shade. Stand-in: a long
  `Block` hull, two tall `Block` posts, a `Slab` deck. It is the ride out (section 6.6), so the deck must be
  reachable: a fallen gangplank (`Stair`, 3 m wide) from the mud at (4, 0, 78) up to the deck.
- *The harbour*: mooring stones along the quay edge (short `Pillar`s every 6 m), a dry cistern in the basin
  (cylinder 3 m across, 0.8 m rim), a timber crane on the quay (three `Block`s).

**Encounters.** Eleven thirsting on the main path. All stand with `HoldGround` unset; their aggro is 9 m.

| | Where | Who and how placed | What the fight teaches |
|---|---|---|---|
| E1.1 | (-4, 0, 26) | One thirsting, kneeling with its back to the player, scraping at a dry pool. It does not turn until struck or until the player is within 3 m | The chain. A free first blow. Its balance breaks fast: the first finisher |
| E1.2 | (3, 0, 44) | One, standing, facing south | Its two claws are slow and plain: hold guard, then try the perfect guard. One perfect guard breaks its balance |
| E1.3 | (-6, 0, 62) and (7, 0, 62) | Two, either side of the great jar | Enemies take turns (as built). The red pounce: step out of the lane |
| E1.4 | (6, 0, 80), (9, 0, 76) in the hull's shade; (10, 3, 86) on the deck | Three. The one on the deck drops when the player is within 6 m of the hull (a pounce from above, its ordinary red pounce started from the deck) | Look up. And this is where a hurt player first drinks: the snatch (section 4.1) |
| E1.5 | (-5, 0, 108), (0, 0, 110), (5, 0, 108) round the cistern; (-16, 0, 104) at the jars | Four, the land's first crowd, in open ground | Do not drink in a crowd. Pick one, break it, finish it: the finisher's untouchable moment is the breath |

**Side path: the sealed well shrine.** A gully cuts the west bank at z 78 to 84 (a ramp, `Stair`, 6 m wide, rise
1.5 m, heading west from x -14). It opens on a sunk court, x -52 to -30, z 68 to 94, floor at y 1.5, walls 3 m of
mud brick on the north, west and east. In the middle the well: a cylinder 4.4 m across and 1 m high of
`m_sun_stone`, closed with a lid of `m_sun_blue` stone, bound with four gold bands (`Block`s of `m_sun_gold`) that
meet in a seal. In front of it an offering table with a clay bowl.
- *Who is there:* two thirsting at (-43, 1.5, 82) and (-39, 1.5, 79), on their knees at the lid, scratching. They do
  not notice the player beyond 5 m.
- *What it holds:* in the bowl, **a full waterskin: a fourth flask** (`FlaskBelt.SetMaximum(4)`, which exists; the
  prompt is "Take the waterskin"; the callout is "A fourth flask"). And carving C3 (section 7.2). The well stays
  sealed; nothing opens it in this land.
- *Minutes:* 2 for a new player.

**The plain spear is not here.** District 1 is fists only (item 4).

### 3.3 District 2: the city of wicks

**Size:** 60 m wide (x -30 to 30), 150 m long (z 140 to 290), floor at y 2 throughout. Mud-brick blocks 4 m high
(`Block`s of `m_sun_brick`) with flat roofs; alleys 5 m wide. The path doubles back, so it is about 190 m of walking.

```
 z290 +----------------------###stair up###-----------------------+  -> golden way
      |  natron   [table](w)   [table](w)      racks of wrapped   |
      |  heaps                                  dead               |  E2.5  embalmers' yard
 z262 |<= stair down     (w)            (w)                        |        x -20..20, z 246..286
      |  to the tombs                                              |
 z246 +=================[ EMBALMERS' GATE ]========================+
      |                  F3 *  pinned soldier, THE SPEAR   C6      |  z 236..246
 z236 +-------+                              +---------------------+
      |#######|   (w)      cold oven   (w)   |#####################|
      |#######|        (w)        (t)        |#####################|  E2.4  market, x -12..20
 z214 |#######+---------------------------+  |#####################|        z 214..236
      |###################################|  |#####################|
      |###################################|(w) <- comes south      |  E2.3  east alley, x 20..25
      |###################################|  |#####################|        z 180..214
      |###################################|{w} dormant in bundles  |
 z185 |########+--------------------------+  |#####################|
      |########|  ->  (w)  (w)  Potters' Row    |##################|  E2.2  z 180..185, x 0..25
 z180 |########|  +--------------------------+--+##################|
      |########|  |################################################|
      |########|{w} doorway                                        |  E2.1  Street of Lamps
      |########|  |   C4 hearth niche                              |        x 0..5, z 140..180
      |########|(w)|################################################|
 z140 +--------+  +------------------------------------------------+
               ^ from the quay
```
`#` is a house block; `(w)` a wick standing; `{w}` a wick lying or hidden (dormant); `(t)` a thirsting.

**The path.** North up the Street of Lamps (5 m wide, 40 m); east along Potters' Row (25 m); north up the east alley
(34 m); west into the market (32 by 22 m, the district's one open space); through the Embalmers' Gate; across the
embalmers' yard (40 by 40 m) to the stair up.

**Landmarks.**
- *Lamps*: a clay lamp niche every 8 m along the first street, all cold but the ones a wick stands near.
- *Hearth niches*: every house has a small empty niche by its door, the name over it cut away (carving C4).
- *The cold oven* in the market: a dome (a half-sunk sphere 4 m across) with bread still on its shelf, grey.
- *The Embalmers' Gate*: two square towers 5 m wide and 7 m high either side of a passage 6 m wide and 10 m long,
  timber doors standing open. On the east door **a gilded soldier is pinned to the wood by his own spear**, a hand's
  width of gold on his mask and no more (carving C6 says why).
- *The embalmers' yard*: six stone tables (2.4 by 1 m `Block`s), heaps of natron (white cones), racks of the
  wrapped dead standing upright against the north wall in rows, facing the sun.

**Where the plain spear is found.** In the Embalmers' Gate, at (3, 2, 241): the spear through the pinned soldier.
A `WeaponPickup` with `WeaponName = Weapons.GreatSpearName`, prompt "Pull the spear free". The body slides down the
door when it is taken. It is the Great Spear as built. Rest fire F3 stands two strides from it, so the first thing a
player does with the spear is rest and walk into the yard with it.

**Encounters.** Thirteen wicks and one thirsting.

| | Where | Who and how placed | What the fight teaches |
|---|---|---|---|
| E2.1 | (2.5, 2, 152) standing; (0.5, 2, 168) dormant in a doorway on the west side | Two wicks. The second steps out when the player passes within 3.5 m | The swipe can be guarded. The grab is red: it cannot. Doorways hold hands |
| E2.2 | (12, 2, 182.5) and (18, 2, 182.5), walking west toward the player | Two wicks abreast in a 5 m alley | There is no room to circle. Back up and let the grab fall short, or break it with a heavy blow as it is shown |
| E2.3 | (22.5, 2, 192) dormant among wrapped bundles; (22.5, 2, 208) standing, walks south when the first rises | Two wicks, one before and one behind | Being held (section 4.2). Break the hold fast |
| E2.4 | (-4, 2, 226), (4, 2, 222), (12, 2, 228) round the oven; a thirsting at (8, 2, 218) | Three wicks and one thirsting, open ground | The two corruptions together: a held player cannot guard a flask. Kill the thirsting first |
| E2.5 | (-8, 2, 258) and (9, 2, 268) standing; (-10, 2, 276) and (6, 2, 280) lying wrapped on tables (dormant) | Four wicks. The two on the tables rise when the player is within 4 m or when either standing one is struck | The spear: its 4 m thrust outreaches the 3.4 m grab. Fight from the end of the lane |

**Side path: the painted tombs.** In the west wall of the embalmers' yard at z 262 a stair goes down (`Stair`, 5 m
wide, heading west from x -20, rise -3 m) into three chambers in a row, each 8 by 8 m, floor at y -1, joined by
doorways 3 m wide: x -30 to -22, -40 to -32, -50 to -42; z 258 to 266. They are open to the sky for the camera (no
roof is built); walls 3 m. The floors are `m_sun_shade`: it is cool down here.
- *What it holds:* five paintings and two captions (section 7.3), read at the walls with the use button. In the last
  chamber a grave dish with **three embers** ("Take what was left for the dead").
- *Who is there:* two wicks lying wrapped in niches in the middle chamber, at (-36, -1, 259) and (-36, -1, 265).
  They do not rise on the way in. They rise when the last painting has been read, and stand between the player and
  the stair. (A scare; timed text is allowed for it: "Something behind you is warm.")
- *Minutes:* 2 to 3 for a new player.

### 3.4 District 3: the golden way and the hall of columns

**Size:** 180 m long (z 290 to 470). The avenue: 24 m wide (x -12 to 12), z 290 to 392, floor at y 2 rising by two
`Stair`s (each rise 1 m, full width, at z 340 and z 384) to y 4. The pylon: z 392 to 400. The hall: 44 m wide
(x -22 to 22), 60 m long (z 400 to 460), floor at y 4. The north porch: z 460 to 470.

```
 z470 +- - - - - - - gold line: the champion's ground begins - - - -+
      |                 F5 *        porch                           |
 z460 +----------------------[ north door 8 m ]--------------------+
      | o    o   (G2)  |nave|    o    o   |   o = column, 2 m across,
 z452 |::::::(p)::::::::::::::::(p):::::::|       at x -15,-5,5,15
 z448 |::::::::::::LIGHT BAR 3::::::::::::|       and z 405,415,...455
      | o    o         |    |    o    o   |
      | o    o         |    |    o    o   |   : = sunlight on the floor
 z432 |::::::::::::LIGHT BAR 2::::::::::::|   blank = shade
 z428 |:::(p)-> patrols east and west:::::|
      | o    o         |    |    o    o   |   the nave (x -2..2) is lit
      | o    o         |    |    o    o   |   for its whole length
 z412 |::::::::::::LIGHT BAR 1::::::::::::|
 z408 |:::::::::::::::(p):::::::::::::::::|
      | o    o         |nave|    o    o   |
 z400 +----------------[ door 8 m ]-------+
 z392 |########  PYLON  ]      [  PYLON  ########|   two towers 16 m wide, 14 m high
      |   [S]         (G3)            [S]  |   E3.3   z 366..384
      |           (G1)     (G1)            |
 z384 |==============stair +1 m============|
      |   [S]                         [S]  |
      |   [S]  scaffold (G2)  (G1)    [S]  |   E3.2   z 336..350, the unfinished face
 z340 |==============stair +1 m============|
      |   [S]                         [S]  |
      |   [S]     (G1)        (G1)    [S]  |   E3.1   z 306..318
      |   F4 *                             |
 z290 +------------###stair from the yard###+
```
`[S]` is a seated statue; `(G1)`, `(G2)`, `(G3)` the gilded by rank; `(p)` a sun-blind priest.

**The path.** Straight. The avenue is the land's one long clear view, and at its end the pylon, and over the pylon
the gold cap of the obelisk.

**Landmarks.**
- *The seated statues*: five pairs, at x -17 and 17, z 302, 322, 346, 362, 380, facing the avenue. Each is 5 m
  wide, 6 m deep, 9 m high: a `Block` throne, a `Block` body, a `Block` head of `m_sun_stone_new`, paler than the
  body under it. The fourth on the west (z 346) is **unfinished**: timber scaffold, the head half cut, the old face
  still on its left half (stand-in: a head `Block` split down the middle, half pale and half weathered); carving C8.
- *The pylon*: two tapered towers with the door between, each face carved with the king spearing something that has
  been chiselled out.
- *The hall of columns*: 24 columns, cylinders 2 m across and 9 m high, at x -15, -5, 5, 15 and z 405, 415, 425,
  435, 445, 455. The roof is slabs at y 13 drawn as **shadows only** (`ShadowCastingMode.ShadowsOnly`), so the
  camera sees through it. The roof is missing over three strips right across the hall and along the nave; that is
  where the sun comes in. Section 4.4 gives the light's exact rectangles.

**Encounters.** Eight gilded and four sun-blind.

| | Where | Who and how placed | What the fight teaches |
|---|---|---|---|
| E3.1 | (-5, 2, 310) and (5, 2, 314) | Two of the first rank, standing guard, facing south | Their guard: light blows from the front are turned. Break the balance, strike heavy, or get behind |
| E3.2 | (-6, 3, 344) second rank by the scaffold; (4, 3, 348) first rank | Two | The second rank's red lunge and its two thrusts: the champion's and the god's first words |
| E3.3 | (0, 4, 376) third rank; (-5, 4, 370) and (5, 4, 370) first rank in front of it | Three, in rank order. The third rank holds back until one of the others falls (`WaitRange` 8) | The planted spear and its ring of fire, small. Rank is read from gold: kill up the ladder |
| E3.4 | priests at (0, 4, 410), (-10, 4, 430) patrolling to (10, 4, 430) and back, (-8, 4, 450), (8, 4, 450); a second-rank gilded at (6, 4, 457) in the shade beside the north door | Four sun-blind and one gilded who can see | Light and shade. The side aisles are shade; the bars are 4 m and a roll is 4.5 m. The gilded at the door is the check: he sees as anyone sees, and his fight in the nave's light brings the priests |

**Minutes.** Avenue 5 to 6, hall 4, for a new player.

### 3.5 The elite's gate

**Size:** 38 m wide (x -19 to 19), 30 m long (z 470 to 500), floor at y 4, open to the sky, walls 6 m east and west.
A line of gold inlay crosses the floor at z 470 (a `Slab` 0.44 m wide of `m_sun_gold`, as the captain's line of pale
stone). North, the last gate: a wall 14 m high with bronze doors 6 m wide and 8 m high at (0, 4, 500), and over the
wall the obelisk, close now.

```
 z500 +==========[ THE GATE OF NOON ]==========+   SealedWay at (0, 4, 498)
      |  brazier                      brazier  |
      |                                        |
      |                 (C)                    |   the champion, (0, 4, 490), facing south,
      |                                        |   spear upright, butt on the stone
      |   four low plinths, 1 m high,          |
      |   at (+-10, z 478) and (+-10, z 492):  |
      |   his fallen predecessors, cast gold,  |
      |   kneeling                             |
 z470 +- - - - - - - - gold line - - - - - - - +
                  porch, F5
```

The champion keeps this ground with `HoldGround(-19, 19, 470.5, 500)`, exactly as the Gate Captain keeps his.
Section 5 has the fight. Three minutes for a new player.

### 3.6 The sun-court (`SunCourt.unity`)

**Size:** a round court of radius 20 m, centre (0, 0, 0), open to the sky, floor of `m_sun_stone` laid in rings
(`Disc`, as the summit's floor, which is 17). A wall 3 m high all round on the north, east and west with a seated
colossus (12 m) against it at the north; on the south the wall is 1.2 m for the camera. South of it a forecourt
12 m wide and 16 m long (z -36 to -20) with rest fire F6 and the door the player came through. A line of gold at
z -20: the fight begins when it is crossed and a wall of heat (a `BoxCollider` with a glow) closes behind.

```
                 north: the seated colossus
              .  -  ~  ~  ~  -  .
          .                         .
        /        12 hours cut         \         radius 20 m
       |         into the floor        |
       |              /                |        [O] the obelisk, at the centre:
       |            /  shadow          |            2.4 m square at the foot,
       |         [O]   (turns)         |            14.4 m high, gold cap
       |                               |
       |          K                    |        K   the king, at (0, 0, 8) when
        \                             /             the player enters
          .                         .
              '  -  . ___ .  -  '
                    | gold line, z -20
                    |  F6 *   forecourt
                    +-- door from the gate of noon
```

**The obelisk** stands at the centre and is solid: it blocks movement and blows, and a lane drawn through it stops
at it. Twelve hour marks are cut in the floor at radius 17. Section 6.3 gives the shadow's rule.

**The camera** for phase two: 1.7 times further off and 15 degrees lower, between the colossus's values (1.55 and
15) and the dragon's (1.9 and 21), because the falcon is overhead and the whole court must be read.

**Minutes.** 8 for a new player on a winning attempt: about 3.5 for the man, 4.5 for the form.

---

## 4. The ordinary enemies

All four are `EnemyKind`s in the manner of the castle's three. Field names below are the code's: `Telegraph` is the
seconds an attack is shown, then `Strike` and `Recovery`; a wedge has `Reach` and `ArcDegrees`; a lane has `Reach`
and `LineWidth`; a circle has `Radius`. Marks are pale when a held guard stops the blow and red when it does not.
Every one has `DrawsStorm = false`.

**Stagger to the player.** `EnemyAttack` has no such number: as built, any blow that lands makes the player flinch
for 0.35 s with a short push, and a guard broken by a blow it cannot pay for is a long flinch. The tables keep to
that. The one exception is the wick's hold.

**Models.** `enemy_thirst`, `enemy_wick`, `enemy_gilded`, `enemy_sunblind`, on the standard enemy skeleton. Every
kind needs the five clips `EnemyBuilder` asks for (`<prefix>_idle_loop`, `_walk_loop`, `_hurt`, `_stagger`,
`_death`) and one clip per attack, named as the attack.

### 4.1 The thirsting

*"The thirst: fire as drought; dried husks that crave anything wet and may go for the player's flasks."* (Liked, user)

**What it is.** One of the river people, dried out standing up. Skin like cracked mud drawn tight over the bones,
grey-brown, split at the joints with nothing wet inside. No eyes to speak of; the mouth is open and stays open.
Long hands. A rag of a kilt. Stooped, quick, light: `Scale` 0.95. No gold and no ember light on it at all: it is the
only enemy in the land with no strong colour, which is how it is told apart at a glance.

**What it teaches.** The fists' chain, the perfect guard, the finisher, and to watch where one drinks.

| | |
|---|---|
| Name, prefix, model | "The Thirsting", `thirst`, `enemy_thirst` |
| Health | 40 (one fist chain and one blow) |
| Balance (`StaggerMaximum`) | 45. `PerfectGuardStagger` 45: **one perfect guard breaks it** |
| Poise | `PoiseHits` 1: every blow makes it flinch |
| Guards | no |
| Speed | walk 3.8, turn 380 |
| Brain | aggro 9, preferred range 1.9, wait range 5.5, rest 0.5 to 1.0, flinch 0.25 |
| Embers | 1 |

| Attack | Shown for | Strike, recovery | Shape | Damage | Mark | Range, cooldown | The answer |
|---|---|---|---|---|---|---|---|
| `thirst_claw_1` | 0.55 s | 0.12, 0.15, chains | wedge 2.2 m, 100 degrees, lunge 0.5 | 8 | pale | 0 to 2.4 | Guard it; a perfect guard ends the fight |
| `thirst_claw_2` | 0.40 s | 0.12, 0.60 | wedge 2.2 m, 100 degrees, lunge 0.5 | 10 | pale | chained | Guard it. Open for 0.6 s after |
| `thirst_pounce` | 0.90 s | 0.15, 0.80 | lane 5 m by 1.2 m, lunge 4, aim locks at 0.65 | 16 | **red** | 3.5 to 7, 5 s | Step off the lane. Open for 0.8 s after |
| `thirst_snatch` | 0.50 s | 0.12, 0.30 | lane 6.5 m by 1.4 m, lunge 5.5, aim locks at 0.7 | 6 | pale | see below | Guard it, or roll. A perfect guard breaks it |

**Its special rule: the snatch.** **New code** (`Enemy`, `FlaskBelt`, a few lines in `PlayerDefence`).

1. *When it comes.* (a) **A drink draws it.** The moment `FlaskBelt.TryStartDrink` succeeds, the nearest thirsting
   that is within 8 m, has noticed the player, and is not showing an attack, flinching, staggered or already
   drinking, starts `thirst_snatch` at once, out of turn (it does not wait for `EnemyTurns`). Only one answers a
   drink. (b) **Unprompted**, as an ordinary attack from 3 to 6.5 m with a cooldown of 14 s, and only while the
   player carries at least two flasks. A hand reaching for the belt, palm up, is the tell; the commit glint is the
   usual white one.
2. *What it does if it lands* (an unguarded hit, or a held guard broken): 6 damage, and **one full flask leaves the
   belt** (`Count` down by one; with none left it is only a claw). The drink that drew it still heals: the heal is
   at 0.4 s and the snatch lands at 0.5 s at the soonest.
3. *Then it drinks.* It stops where it is and plays `thirst_drink` for 1.6 s. While it drinks it is helpless and
   takes blows at 150%, as a staggered enemy does.
4. *How the player stops it.* Guard the snatch or roll out of its lane; or, having lost the flask, **kill it or
   break its balance before the 1.6 s is up**: the flask goes back on the belt at once (callout "Taken back"). 40
   health at 150% is one fist chain.
5. *If it finishes drinking:* the flask is gone until the next rest, and the thirsting is whole again (health to
   full). Callout "It drinks." It is not made stronger: the loss is the flask and the fight starting over.
6. *Limits.* One thirsting may hold a flask at a time. The first time a snatch begins in a sitting, a tip is shown
   after the fight (not during): "The thirsting come for the flask. Drink out of their reach, or take it back
   before they finish."

**Clips:** `thirst_idle_loop` (swaying, mouth open, hands half raised), `thirst_walk_loop` (a quick stooped
scuttle), `thirst_hurt`, `thirst_stagger`, `thirst_death` (folds and breaks like dry clay), `thirst_claw_1`,
`thirst_claw_2`, `thirst_pounce`, `thirst_snatch`, `thirst_drink`, and `thirst_kneel_loop` for E1.1 and the well.

### 4.2 The wick

*"The wick: they burn and are not consumed; the wrapped dead smoulder like candle wicks and the living are charred
with ember light in the cracks; they grab and hold to share the burning."* (Liked, user)

**What it is.** One kind, two dressings of one model (`Palette` "wick" and "wick_wrapped"). *The charred*: a
townsman burnt black, the skin split in a net of cracks with `m_sun_ember` light in every one, brightest at the
chest and the palms. *The wrapped*: a body in linen, the linen scorched brown at the edges and glowing along its
windings like a wick drawing flame; the head wholly wrapped. Both are slow, upright, arms a little out from the
body, palms forward. `Scale` 1.05. A thread of smoke rises from each (a small particle system, as the castle's
`WoundEmbers`).

**What it teaches.** That red means move; the heavy blow that breaks an attack being shown; how to fight in a
narrow place; and, with the spear, reach.

| | |
|---|---|
| Name, prefix, model | "The Wick", `wick`, `enemy_wick` |
| Health | 70 |
| Balance | 70. `PerfectGuardStagger` 55 |
| Poise | `PoiseHits` 3: shrugs off two light blows, flinches on the third. A heavy blow always flinches it and breaks an attack it is showing (as built, it is not `Steadfast`) |
| Guards | no |
| Speed | walk 2.2, turn 200. It is slow; the alley is what makes it dangerous |
| Brain | aggro 10, preferred range 2.2, wait range 4.5, rest 0.7 to 1.4 |
| Embers | 1 |

| Attack | Shown for | Strike, recovery | Shape | Damage | Mark | Range, cooldown | The answer |
|---|---|---|---|---|---|---|---|
| `wick_swipe` | 0.70 s | 0.15, 0.60 | wedge 2.4 m, 120 degrees, lunge 0.4 | 12 | pale | 0 to 2.6 | Guard it. Open for 0.6 s |
| `wick_slam` | 0.90 s | 0.18, 0.80 | wedge 2.6 m, 70 degrees: both arms brought down | 18 | pale | 0 to 2.6, 3 s | Guard it or step to its side. Open for 0.8 s |
| `wick_grab` | 0.90 s | 0.15, 1.00 | lane 3.4 m by 1.6 m, lunge 2.2, aim locks at 0.6 | 6, then the hold | **red** | 1.5 to 4, 6 s | Roll sideways out of the lane; or break it with a heavy blow while it is shown; or a perfect guard, which throws 55 on its balance. Missed, it stumbles: open for 1.0 s |

**Its special rule: the hold.** **New code** (a held state on `PlayerController` and `PlayerDefence`, and on `Enemy`).

1. *What it does.* A grab that lands takes hold. The wick plays `wick_hold_loop` with its arms round the player; the
   player cannot move, attack, guard, roll or drink. The hold lasts **2.5 s** and burns for **6 every 0.5 s**, the
   first bite at 0.5 s: 30 in all if nothing is done. Ember light runs from the wick's arms onto the player while it
   lasts.
2. *How it is broken.* **Every press of light attack or dodge takes 0.4 s off the hold.** Five presses end it after
   about a second and two bites (12). The prompt is shown once, the first time ever held: "Break free", with the two
   buttons.
3. *When it ends, either way,* the wick is thrown off 2 m and is open for 1.0 s (`wick_thrown_off`). If the player
   broke it by pressing, the wick's balance also takes 20.
4. *Others.* While one wick holds the player the turn stays taken: no other enemy starts a hand-to-hand attack. A
   thirsting's snatch is the exception (it is out of turn by its own rule), and a held player cannot guard it. That
   is E2.4's whole lesson.
5. *It does not kill.* A bite that would take the last health leaves 1 and ends the hold. Being held to death in
   the first land teaches nothing.

**Dormant wicks.** **New code** (one flag on `Enemy`): a dormant wick lies or stands still (`wick_lie_loop`, or its
idle in a doorway) and is not noticed by soft aim. It wakes when the player is within its wake distance (3.5 m in a
doorway, 4 m on a table) or when it is struck, takes 1.2 s to rise (`wick_rise`), and can be struck all the while.
A blow on one before it has risen does double damage.

**Clips:** `wick_idle_loop`, `wick_walk_loop` (a slow straight-legged walk, arms out), `wick_hurt`, `wick_stagger`,
`wick_death` (goes to its knees and burns down to a black stump; it is at last consumed), `wick_swipe`, `wick_slam`,
`wick_grab`, `wick_hold_loop`, `wick_thrown_off`, `wick_lie_loop`, `wick_rise`.

### 4.3 The gilded soldier

*"The gilded: the sun-king promised his people the golden flesh of gods and it came true as molten gold, masks fused
to faces; the more gold, the higher the rank."* (Liked, user)

**What it is.** A soldier of the golden way, with a spear and no shield. Gold was poured on him and took: it lies on
the body as a metal skin with a run edge, like wax, and where it ends the flesh under it is burnt. Every one has
the mask: a smooth gold face with the king's features, fused on, no eyeholes cut, only two slits. Below the gold he
is a thin man in a linen kilt. The gold is `m_sun_gold`; where it is thickest it still glows at the seams.

**Rank is how much gold**, and it is three `EnemyKind`s on one model with three palettes:

| Rank | Name | Gold on the body | `Scale` | Health | Balance | Damage | Poise | Guard | Attacks | Embers |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | "Gilded Levy" (`Palette` "gilded1") | the mask and the spear arm: about a quarter | 1.05 | 80 | 80 | as the table | 3 | turns light blows from the front for 25% damage and all the stagger | thrust, sweep | 1 |
| 2 | "Gilded Spearman" ("gilded2") | head, chest and both arms: about half | 1.12 | 110 | 100 | table times 1.15 | 4 | the same, and after 3 blows turned answers with the thrust | adds two thrusts and the red lunge | 2 |
| 3 | "Gilded Warden" ("gilded3") | all but the legs below the knee: about three quarters | 1.20 | 150 | 120 | table times 1.3 | 5, and `Steadfast`: nothing breaks an attack he has begun | turns light blows for 20% and heavy blows for 50% (`HeavyGuardedDamage` 0.5) | adds the planted spear | 3 |

All ranks: prefix `gilded`, model `enemy_gilded`, `Guards = true`, walk 2.7, turn 220, `PerfectGuardStagger` 55,
aggro 11, preferred range 3.4, rest 0.5 to 1.1. The gold is why they guard: a blow on gold rings and does little.
What is not gold is hurt as flesh is: from behind (outside the front 150 degrees) every blow lands in full.

**What it teaches.** The duel the castle's soldier teaches (guard, perfect guard, punish), and after it the spear's
own grammar: the lane, the second thrust that waits, the red lunge, the planted ring. Each rank adds one of the
god's words.

| Attack | Ranks | Shown for | Strike, recovery | Shape | Damage (rank 1) | Mark | Range, cooldown | The answer |
|---|---|---|---|---|---|---|---|---|
| `gilded_thrust` | all | 0.65 s | 0.15, 0.60 | lane 4.4 m by 1.0 m, lunge 0.9 | 16 | pale | 0 to 4.6 | Guard it, or sidestep and strike. Open for 0.6 s |
| `gilded_sweep` | all | 0.85 s | 0.20, 0.75 | wedge 3.4 m, 170 degrees | 20 | pale | 0 to 3.2, 2.5 s | Guard it. Punishes circling |
| `gilded_thrust_1`, then `gilded_thrust_2` | 2, 3 | 0.60 s, then 0.50 s | 0.12, 0.10; then 0.12, 0.85 | lanes 4.4 m and 4.8 m by 1.1 m; the second turns to follow until 0.8 | 14, then 18 | pale | 1.9 to 4.4, 3 s | Guard both. The second waits longer than a frightened roll lasts. Open for 0.85 s |
| `gilded_lunge` | 2, 3 | 1.00 s | 0.20, 0.90 | lane 5 m by 1.3 m, lunge 3.4 | 26 | **red** | 3.5 to 8, 6 s | Step off the lane. Open for 0.9 s |
| `gilded_plant` | 3 | 1.00 s | 0.20, 1.20 | circle of 3.2 m round himself; then the ground in it burns 3 s for 4 every 0.5 s | 20 | **red** | 0 to 3, 8 s | Get out of the circle, wait out the fire, or stay at spear's length. His spear is stuck: open for 1.2 s to anyone with reach |

**Its special rule** is the rank table: numbers, guard and attacks rise with the gold, and the gold can be read from
across the avenue. Nothing else is new code; every field exists on `EnemyKind`. (`gilded_plant`'s burning ground is
a `Hazards` patch that bites the player; hazards that hurt the player exist for the dragon's storm.)

**Clips:** `gilded_idle_loop` (spear grounded, upright, still as a statue), `gilded_walk_loop` (stiff where the gold
is, the bare leg leading), `gilded_hurt`, `gilded_stagger`, `gilded_death` (falls like a thing cast, without
bending), `gilded_thrust`, `gilded_thrust_1`, `gilded_thrust_2`, `gilded_sweep`, `gilded_lunge`, `gilded_plant`.
A `gilded_guard_loop` if time allows (the castle's soldier has none either).

### 4.4 The sun-blind priest

*"Sun-blind: they looked at their god until their eyes burned out, and hunt by heat and by shadow."* (Liked, user)

**What it is.** A priest of the hall. Shaven head, a long white robe gone grey, a leopard skin over one shoulder
burnt to a rag. Where the eyes were, two round burns with ember light deep in them, and a flat gold disc hung on
the brow that he keeps turned up toward the sun. He carries a long staff with a bronze mirror at its head. He holds
his head tilted, listening with his skin. `Scale` 1.0.

**What it teaches.** That shade is safe ground, which is the god's last lesson, and that an enemy who has not found
you can be struck first.

| | |
|---|---|
| Name, prefix, model | "Sun-Blind Priest", `sunblind`, `enemy_sunblind` |
| Health | 60 |
| Balance | 50. `PerfectGuardStagger` 40 |
| Poise | 2 |
| Guards | no |
| Speed | walk 2.0 feeling his way; 3.4 while he has a lit target; turn 300 |
| Brain | preferred range 7 with a lit target (he backs off under 4), 2.4 without; rest 0.8 to 1.5 |
| Embers | 1 |

| Attack | Shown for | Strike, recovery | Shape | Damage | Mark | Range, cooldown | The answer |
|---|---|---|---|---|---|---|---|
| `sunblind_ray` | 1.10 s | 0.20, 0.60 | circle of 2.0 m thrown on where the target stood (`Ranged`): he lifts the mirror and the sun comes down it | 20 | **red** | 3.5 to 18; **only at a lit target** | Keep moving; better, be in shade when he looks for the next one |
| `sunblind_brand` | 0.70 s | 0.15, 0.70 | wedge 2.8 m, 140 degrees: the staff swung low | 16 | pale | 0 to 3 | Guard it. Open for 0.7 s |
| `sunblind_flare` | 0.80 s | 0.20, 0.90 | circle of 3 m round himself; then that circle **counts as lit** for 4 s | 16 | pale | 0 to 3.2, 5 s | Guard it or step back. Then leave the lit ground, or every priest in the hall knows |

**Its special rule: light and shade.** **New code**: a small static class `Sunlight` (in `Clover.World`), and a
changed notice test in `Enemy` for this kind.

1. *What is lit.* The scene registers rectangles of floor. A position is **lit** when it is inside any of them, or
   inside a flare's circle while that lasts; otherwise it is **shaded**. That is the whole test:
   `Sunlight.IsLit(Vector3 position)`. It is plain C# and has EditMode tests. Outside the hall no rectangles are
   registered and the priest's rule is not used (no priest stands outside it).
2. *The hall's rectangles.* The roof is open over three strips and the nave. With the key light at 78 degrees and
   the roof at 9 m above the floor, each patch of light lies 1.9 m north of its hole. The builder makes the roof
   holes and the rectangles from one list so they cannot disagree, and lays a `Slab` of `m_sun_stone` (pale) under
   each lit rectangle on a floor of `m_sun_shade` (dark), so the rule is readable even if shadows are switched off:

   | Lit rectangle | x | z |
   |---|---|---|
   | Bar 1 | -22 to 22 | 408 to 412 |
   | Bar 2 | -22 to 22 | 428 to 432 |
   | Bar 3 | -22 to 22 | 448 to 452 |
   | The nave | -2 to 2 | 400 to 460 |

   A column's own shadow inside a bar is not shade for the rule; it is too thin to stand in. The tips of the
   priests' rays, and the player's blows, change nothing.
3. *What the priest knows.* He cannot see. **A player who has been lit for 0.4 s without a break is found**, by
   every priest within 18 m of them, through columns, whichever way the priest faces. (The 0.4 s is so that a roll,
   0.35 s and 4.5 m, crosses a 4 m bar unfound.) **A shaded player is found only** by a priest within 2.5 m (the
   heat of a body), or for 4 s after the player lands a blow or takes one within 8 m of that priest (the heat of
   a fight).
4. *What he does.* Found and lit: he keeps his distance and throws rays. The player steps into shade: the ray
   already shown still falls where it was thrown; he cannot throw another; he walks to where the player was last
   lit, searches there for 3 s (`sunblind_search_loop`, staff sweeping), and goes back to his post. Found at arm's
   length: the brand, and the flare when he has been struck.
5. *The reward for using it.* A blow on a priest who has not found the player does **double damage and double
   stagger**. A fist heavy from the shade is 36 of his 60 and most of his balance; a spear heavy is 48.
6. *How it is shown.* A priest who has found the player turns his brow disc toward them and it lights. One who is
   searching holds it level and dim. No bar, no icon.

**Clips:** `sunblind_idle_loop` (face lifted to the roof, turning slowly), `sunblind_walk_loop` (staff tapping
ahead), `sunblind_search_loop`, `sunblind_hurt`, `sunblind_stagger`, `sunblind_death` (kneels, face up, and the
ember in the eyes goes out), `sunblind_ray`, `sunblind_brand`, `sunblind_flare`.

---

## 5. The elite: the Champion of the Gate

*"one of the wholly gilded, a champion cast in gold who holds the last gate with a spear and shows the god's
patterns before the god does."* (item 4)

In the manner of the Gate Captain: alone, on his own ground, his name over his own bars, the sealed way counts him.

**Who he is.** Wholly gilded: there is no flesh left to see. A man cast in gold from the mask to the soles,
seamless, the features on the mask the king's, a soldier's kilt and collar modelled in the metal. He does not move
like the others; the gold has not stiffened on him, it flows a little at the joints when he moves and sets again
when he stops. He stands with his spear upright, butt on the stone, and does not level it until the gold line is
crossed. The model is `enemy_gilded` with `Palette` "cast", `Scale` 1.5; prefix `champion`; name "Champion of the
Gate"; `IsElite = true`.

**His ground.** Section 3.5. He notices only someone north of the gold line, never leaves it, gives up after 3 s,
and comes out for 8 s if struck from off it: all as the captain, with the same fields.

| | |
|---|---|
| Health | 240. A finisher takes at least 34% (82): three finishers fell him |
| Balance | 140. A perfect guard throws back 50; open for 3 s |
| Guard | Turns light blows from the front for 15% of their damage and half their stagger, heavy blows for 40% and all their stagger (the captain's numbers). After four blows turned he answers with the butt |
| Poise | 5; `Steadfast` |
| Awareness | 8.5 m on his ground; rests 0.7 to 1.4 s, a third of that when the player hangs back beyond 5 m (`ImpatientBeyond` 5) |
| Speed | walk 3.0, turn 260 |
| Second phase | At half health: **the gold runs**. He glows at every joint, opens with the planted spear, acts 12% faster (`SecondPhasePace` 1.12), and gains the last two rows |
| Reward | 3 embers. The gate counts him (`SealedWay.Configure(director, "Champion of the Gate")`) |

| Attack | Shown for | Shape | Damage | Mark | Notes, and what it shows of the god |
|---|---|---|---|---|---|
| `champion_thrust_1`, `_2`, `_3` | 0.60, 0.50, 0.60 s | lanes 5.0, 5.0 and 5.4 m long, 1.1 m wide; the third turns to follow until 0.8 | 14, 14, 20 | pale | **The god's three long thrusts**, a little slower. From 2 to 4.8 m. Open for 0.9 s after the third |
| `champion_sweep` | 0.80 s | wedge 3.8 m, 200 degrees | 24 | pale | Punishes circling. Cooldown 3 s. Open for 0.8 s |
| `champion_butt` | 0.34 s | wedge 2.4 m, 70 degrees | 12 | pale | Within 2.6 m only; his answer after four blows turned |
| `champion_throw`, then `champion_recall` | 1.00 s, then 1.20 s bare-handed, then 0.60 s | throw: lane 12 m by 1.2 m. Recall: a lane 1.2 m wide from where the spear stands back to his hand | 26, then 14 | **red**, then pale | **The god's thrown spear and its return.** From 6 to 12 m, cooldown 8 s. The spear stands in the floor where the lane ends. For 1.8 s he has no spear and **no guard**: this is his widest opening. The recall comes back along the line from spear to hand, so strike him from the side, not from between |
| `champion_plant` | 1.10 s | a ring of fire 5.5 m in radius round the spear, 1.2 m thick, standing 6 s: 18 to anyone on it as it rises, then 8 every 0.5 s to anyone touching it | 18 | **red** | **The god's ring.** Cooldown 16 s; it opens his second phase. He pulls the spear free 0.6 s after and fights inside the ring: there is no backing away from the lane for six seconds |
| `champion_reap` (second phase) | 0.70 s, then 0.60 s | the sweep, then the 5.4 m lane | 24, then 20 | pale | No rest between. Open for 0.95 s after |
| `champion_throw_twice` (second phase) | 0.90 s; recall 0.9 s after; then 0.80 s | the throw, the recall, and the throw again from where he stands | 24, 14, 24 | red, pale, red | The second spear is recalled as the first was. The opening is after the second throw |

**New code for him:** a thrown lane whose weapon stays where it lands and is called back (the player's Flying Spear
already does the first half: `WeaponShot` with `Sticks`), a bare-handed state with the guard down, and a ring-shaped
mark and hazard. `MarkerShapes` has lanes and sectors but no ring: until one is made, the ring is drawn as sixteen
circles of 1.2 m round the spear.

**How he is beaten.** As the captain is: guard, guard perfectly when it can be done, strike in the opening, break
the balance, finish, three times. What is new is that two of his attacks open him further than any guard does: stand
beside him when the spear leaves his hand.

**What he teaches about the god.** The lane is the spear's whole shape, so fight beside it, not at the end of it. A
thrown spear comes back. A planted spear closes the ground.

**What opening the gate needs.** The champion dead, once. The seal stays broken if a rest brings him back (as built).
Prompt: "Open the gate of noon". While he stands: "Sealed. The Champion of the Gate holds the way." (the code's
sentence). When it opens: "It is noon within." `LeadsTo = "SunCourt"`.

---

## 6. The god

### 6.1 His name and title

**Decided (user):** "each god's name is an original name with the sound and feel of its source, a nod and never the
deity's own name."

| Candidate | What it is made of | Note |
|---|---|---|
| **Khepharu** (KHEP-ha-roo) | the scarab's word (*kheper*, "to become") run into the falcon's (*heru*) | **Chosen.** The name is the form: beetle and falcon in one word. It says nothing a player needs to know and sounds like the place |
| Harakhem | "the falcon of the horizon", bent | Good sound; says only the falcon |
| Menharu | "the falcon endures" | Plainer; nearer to real throne names |

None of the three is, to Claude's knowledge, the name of any deity or king. That was checked from memory only, not
searched: the owner should search the chosen one before it is fixed.

**Titles.** Phase one, on the bar: "Khepharu" with the title "God of Fire" (as Narukami has "God of Lightning").
Among his people and on the walls: "the Sun-King". Phase two, on the bar: "Khepharu,  the Sun-Bearer" (the dragon's
bar has the same two spaces).

**His weapon** (item 10): **the Sun-Spear**. 3.2 m; a gold shaft; a leaf blade 0.7 m long that is white-hot along
its edges; below the blade a flat gold disc 0.35 m across with a wing either side of it. The player's is the plain
Great Spear found at the gate, 2.6 m, bronze and ash wood.

### 6.2 Phase one: the man with the sun-spear

**Who he is.** A tall thin old man, taller than the champion's model is drawn (`Scale` 1.25), on the boss skeleton
(`boss_sunking`). Bare-chested, a striped head cloth, a kilt, a broad collar. He is gilded as his people are, but on
him it is finished: the gold lies on his skin thin as leaf, with no burn at its edge. He wears no mask. His eyes
are the two round burns the priests have. He knows where the player is by the same rule they do (it is always light
in his court, so always).

`EnemyKind`: name "Khepharu", title "God of Fire", prefix `sunking`, model `boss_sunking`, `IsBoss = true`.

| | |
|---|---|
| Health | 480 (Narukami has 620, against a player with a katana and an element; this player has a plain spear and nothing else) |
| Balance | 170; staggered for 2.8 s |
| Finisher | at least 20% of his health (96) |
| Guard | turns light blows from the front for 20%; after 2 blows turned he answers with the butt |
| Perfect guard | throws back 42. His three thrusts guarded perfectly are 126 of 170 |
| Poise | 4 |
| Speed | walk 3.2, turn 400 |
| Brain | aggro 16, leash 80, **preferred range 4.2**: he fights at the far end of his spear, just past the end of the player's. Rest 0.5 to 1.1 |
| Below half health | He plants the spear at once (`SecondPhaseAttack = "sunking_plant"`), acts 15% faster, and gains the last two rows |
| Embers | none here; the five are paid when the form falls |

| Attack | Shown for | Shape | Damage | Mark | Range, cooldown | Answer, and opening |
|---|---|---|---|---|---|---|
| `sunking_thrust_1`, `_2`, `_3` | 0.50, 0.34, 0.55 s | lanes 5.5 m by 1.1 m, lunge 1.0, 1.0, 1.6; the third turns to follow until 0.8 | 15, 15, 22 | pale | 2 to 5.2 | Guard all three; perfectly if it can be done. **Open for 0.8 s** after the third |
| `sunking_reach` | 0.55 s | one lane 7 m by 1.0 m, lunge 1.8 | 20 | pale | 3.5 to 7, 3 s | For a player hanging back. Sidestep into him. Open for 0.6 s |
| `sunking_sweep` | 0.70 s | wedge 3.6 m, 180 degrees: the haft, low | 20 | pale | 0 to 3, 3 s | Guard it. It is what he does to someone who has got inside the lane |
| `sunking_butt` | 0.30 s | wedge 2.2 m, 70 degrees | 10 | pale | 0 to 2.4; his counter | Quick and light; it is there to end a chain on his guard |
| `sunking_throw` | 0.90 s | lane 14 m by 1.2 m: the spear thrown; it stands where the lane ends | 26 | **red** | 6 to 16, 8 s | Step off the lane. Then he is bare-handed: see the next row |
| `sunking_ray` | bare-handed 1.0 s, during which he takes two steps to one side; then 0.80 s | a lane 2.2 m wide from the standing spear to his raised hand, wherever he now is. The spear comes back down it as a ray of light | 30 | **red** | follows every throw | **Do not stand between the spear and the king.** He has no guard from the throw until the spear is in his hand: **open for 1.8 s**, and blows on him then throw half as much stagger again |
| `sunking_plant` | 1.10 s | a ring of fire 7 m in radius round the spear, 1.2 m thick, standing 8 s | 22 on the ring as it rises; 10 every 0.5 s on contact | **red** | any, 18 s | Be inside or outside, not on it. He pulls the spear free after 0.6 s (the opening) and the duel is inside the ring until it dies |
| `sunking_noon_1`, `_2`, `_3` *(Claude's addition)* | 0.85, 0.50, 0.50 s | he lifts the spear to the sky and three shafts of sun fall one after another on where the player stands: circles of 2.1 m | 18 each | **red** | 6 to 20, 9 s | Keep walking. **A shaft cannot be aimed at someone in the obelisk's shadow**: if the player is in it when a shaft is marked, the mark is laid at the shadow's edge nearest them. This is where the shadow is first learned, at no cost |
| `sunking_thrust_3` into `sunking_sweep` *(below half)* | as above, no rest between | | 22, then 20 | pale | | The thrusts no longer end in an opening every time: one time in two the sweep follows. Open for 0.75 s after the sweep |
| `sunking_ray`, burning *(below half)* | as above | the ray's lane stays alight for 4 s: 8 every 0.5 s | | red | | The court begins to be cut up, as it will be |

`sunking_noon` is Narukami's three bolts with new names, and `sunking_thrust` his three cuts made lanes: both are
data. The throw and the ring are the champion's code.

**Rhythm.** Slower and longer than Narukami: he does not come to the player, he keeps them at the point. Most of the
fight is thrusts and the reach, answered by guarding and stepping in; the throw comes when the player backs off; the
ring comes once before half health and at once below it. He never does the same thing twice running.

**Openings, in order of size.** Bare-handed after the throw (1.8 s, no guard, stagger at 150%); after the third
thrust (0.8 s); after the reach (0.6 s); as he pulls the planted spear free (0.6 s).

**What breaks his balance.** Perfect guards (42 each); heavy blows; any blow while he is bare-handed. Blows on his
guard do little. Two finishers and honest work in the openings fell him; a new player will need three.

**What is said.** Section 7.5. He speaks when the gold line is crossed, click-through, before he moves.

### 6.3 The obelisk's shadow: the exact rule

**New code**: `ShadowHand` (plain C#, in `Clover.Gameplay`, with EditMode tests) and a thin component that turns
the court's key light to match it.

- **Shape.** A straight strip on the floor that starts at the obelisk and points away from the sun. It runs from
  1.2 m to 18 m from the court's centre and is **3.6 m wide** (1.8 m either side of its middle line). It is drawn
  as a `Slab`-thin mesh of `m_sun_shade` with a hard edge, a hand's height off the floor, and the court's real
  shadow from the key light lies inside it.
- **The test.** `ShadowHand.Covers(position)`: take the position relative to the centre; `along` is its length
  along the hand's direction and `across` its distance from the middle line; covered when `1.2 <= along <= 18` and
  `|across| <= 1.8`.
- **How fast it turns.** Clockwise seen from above, at a steady rate: **4 degrees a second** while the man is
  fought (a full turn in 90 s); **6** against the form (60 s); **9** in the form's fury (40 s). At 10 m from the
  centre that is 0.7, 1.05 and 1.6 m a second: a slow walk. It starts pointing north. It never stops and never
  jumps, except once in the fury's blaze (section 6.4, the Zenith).
- **The light.** The court's key light keeps its 78 degrees of height and is turned about the upright axis so that
  it shines along the hand. In his court the king turns the sun; that is why noon here has a moving shadow and
  nowhere else does.
- **What standing in it does.** Three things and no more: (1) **the blaze does not burn there**; (2) **hot floor
  under it is cold while it is under it** (a lane or crater the shadow lies across does no damage inside the
  shadow, and burns again when the shadow has passed); (3) **no shaft of sun or falling feather is marked in it**.
  It does not heal, it does not hide the player from the god, and blows of the body (the spear, the ram, the stoop)
  land in it as anywhere.
- **The obelisk itself** blocks movement. The inner 1.2 m is not shadow, so nobody shelters by hugging the stone.

### 6.4 Phase two: Khepharu, the Sun-Bearer

*"the scarab that rolls the sun across the court and the falcon that carries it up, one creature in two shapes, as
the sun was a beetle at dawn and a falcon at noon. The sun is the shared piece of the fight. When the whole court
burns, the obelisk's moving shadow is the only safe ground."* (item 4)

**What was kept from the dragon** (`GodlyForm`, as built): the man falls; 35% of the player's health comes back;
2.4 s later the form rises in a 4.2 s entrance that hurts nobody and ends in a roar; every attack draws its mark
first, follows the player and locks a moment before the blow, and is followed by a rest of 0.9 s; a set opening
order that shows each attack; a lull of 2.4 s after two heavy attacks; a balance that breaks; a fallen head that a
heavy blow finishes; fury below half; a 5 s death. The new class is `SunBearer : GodlyForm`, with a
`SunBearerAttack` enum and a `SunBearerChooser` in the manner of `DragonAttack` and `DragonChooser`.

| | |
|---|---|
| Health | 560 (the dragon has 760) |
| Balance | 160. A perfect guard throws 60 (30 for the passes) |
| Balance broken | the shape falls on its face; the head lies on the floor for 5.5 s at 160%; a heavy blow is a finisher worth at least 22% (123) |
| Fury | below half health: a quarter faster, rests 55% as long, and the changes in the tables |
| Afflictions | wear at it; never freeze, hold or lift it (as `GodlyForm`) |

**One creature, two shapes.** Only one shape is in the court at a time.
- *The scarab* is on the floor: a beetle of black-bronze and gold 6 m long, with the king's gold face set in its
  thorax. It walks at 4 m a second and turns slowly. With the sun, its shell burns at the edges.
- *The falcon* is in the air: the same thorax and the same face, the beetle's wing cases lifted and wings of gold
  and fire unfolded from under them, 14 m across, a falcon's head and tail. It circles the rim 12 m up. **Its
  shadow crosses the floor under it**, a round dark blob 5 m across, and is the first tell of everything it does.
- *The change.* **The Rising**: the scarab rears, throws the sun straight up, and in the flare the wing cases open
  and it goes up after it as the falcon (1.5 s). **The Setting**: the falcon comes down on the rim, folds, and is
  the scarab, the sun set down in front of it (1.5 s). Neither hurts anyone outside the fury.

**The sun: the rule.** A burning ball 3 m across. It is always in one of four states.

| State | Where it is | What it does |
|---|---|---|
| Held by the scarab | on the floor in front of the scarab, between its forelegs | Touching it burns for 12. It is the blow in the scarab's lane attacks |
| Held by the falcon | in the talons, in the air | Out of reach. It is the blow in the stoop and the pass |
| At the zenith | high over the obelisk, the falcon under it | The blaze |
| **Loose** | lying on the floor, dull red, harmless | Nothing. It can be pushed: a heavy blow moves it 3 m, a light blow 1.5 m, away from the striker, until it meets the rim |

**How the sun is knocked away.** Two ways, the same for both shapes:
1. **A perfect guard against the sun itself** (the roll, the furrows, the stoop, the pass). The sun is thrown 8 m
   off, at right angles to the way it was going, and is loose.
2. **30 damage to the body of the shape holding it, dealt during one attack** (from the moment its mark is drawn to
   the end of its recovery). The scarab's flank as it passes; the falcon standing on the sun after a stoop. The
   count starts again with each attack. While it holds the sun its shell is hot and blows on the body do only 25%
   of their damage to its health, but they count in full toward the 30.

**What knocking it away does: the shape is exposed.** The scarab goes over on its back; the falcon tumbles and lies
with its wings spread. Its fire goes out: no glow, no burn. **For 4 s it lies there, and every blow on it lands at
100% and wears its balance.** Then it rights itself and goes to the sun at 3 m a second, still exposed, still
hittable, and takes it up (1.0 s). So **the further the sun was knocked, and the further it is pushed while loose,
the longer the opening**: 4 s at the least, 9 s at the most (after 9 s the sun rolls to it). A player may spend the
time on the body or on pushing the sun; the body is nearly always right, and the choice is there for those who
look. Then the fight goes on with the shape it was.

**The scarab's attacks.**

| Attack | Kind | Mark and timing | Damage | Answer | Opening |
|---|---|---|---|---|---|
| **The Roll** | red, heavy | The scarab goes to the rim. A red lane 4 m wide is drawn right across the court through the player; it follows for 0.9 s and holds 0.4 s. Then the sun is rolled down it at 12 m a second, the scarab behind it. **The lane is left hot for 10 s** (glowing orange; 6 every 0.5 s to whoever stands on it) | 28 | Step off the lane. A perfect guard facing the sun knocks it away. Or let the sun pass and put 30 into the scarab's flank as it follows | None, unless the sun is knocked. It turns at the far rim for 1.0 s |
| **The Furrows** | red, heavy | Three rolls one after another, 1.0 s apart, back and forth, each lane 5 m nearer the player's side than the last. Each lane follows for 0.6 s and holds 0.3 s. **Three hot lanes for 14 s** | 24 each | As the roll. Stand in the strip between two lanes, or beyond the last | After the third it rests on the sun at the rim for 2.0 s: the body can be struck (25% to health; counts toward the 30) |
| **The Ram** | pale | Only at a player within 9 m; always, once they have stood there 1.2 s, never twice running. It leaves the sun behind it and lunges horn first at a pale circle of 3.4 m that follows for 0.8 s and holds 0.3 s | 34 | Guard it; a perfect guard throws 60. Or walk out of the circle | **The horn is stuck in the paving for 1.8 s: the head is open at 100%.** The plain opening, as the dragon's bite is |
| **The Sparks** | red | It lifts its wing cases and shakes them. Five circles of 2.2 m in an arc 7 m out from it on the player's side, each falling 1.0 s after it is drawn; 0.7 s later five more in the gaps (the fury adds the first five again) | 16 each | Step into the gaps, then into where the first fell | None; it lowers the cases for 0.8 s |
| **The Rising** | the change; in the fury red, heavy | It rears with the sun over its head for 1.2 s and throws it up. In the fury a red circle of 6 m round it fills over that 1.2 s and the flare burns it | 0; 20 in the fury | Be more than 6 m off in the fury | None |

**The falcon's attacks.**

| Attack | Kind | Mark and timing | Damage | Answer | Opening |
|---|---|---|---|---|---|
| **The Stoop** | red, heavy | Its shadow stops over the player and becomes a red circle of 5 m that follows for 1.0 s and holds 0.5 s. It comes straight down, sun first. **The circle's inner 4 m is left hot for 10 s** | 38 | Walk or roll out. A perfect guard at the last instant knocks the sun out of its talons | **It stands on the sun for 1.6 s with its wings spread: 30 damage to it then knocks it off the sun.** The falcon's main opening |
| **The Pass** | red, heavy | A red lane 5 m wide from rim to rim through the player, shown for 1.2 s. It flies the lane low in 1.0 s with the sun hanging under it. **The lane is left hot for 10 s.** In the fury it turns and comes back down a lane 30 degrees round from the first, 0.9 s later | 26 | Step off. A perfect guard facing it knocks the sun away (and throws 30) | None, unless the sun is knocked |
| **The Feathers** | red | It beats its wings over the court. Burning feathers fall: a circle of 1.8 m where the player stands; 0.85 s later a ring of six 5 m out; 0.85 s later the middle again with the ring turned to its gaps. Each falls 1.0 s after it is drawn. None is marked in the obelisk's shadow | 14 each | Step out, step in, stand in the band between; or stand in the shadow | None |
| **The Zenith: the blaze** | red, heavy; the fight's set piece | It climbs with the sun to the top of the sky over the obelisk. **The whole floor fills red over 3.0 s, all but the obelisk's shadow**, which is outlined pale so it cannot be missed. Then the court blazes for **6 s** (8 in the fury): **12 every 0.5 s** to anyone not in the shadow. The shadow goes on turning at 6 degrees a second. In the fury it turns at 9 and, half-way through, **stops for 0.5 s and comes back the other way** (the stop is the warning) | 12 every 0.5 s | **Be in the shadow and walk with it.** Nothing else in the court attacks while the blaze lasts | When it ends the falcon comes down spent, sets (1.5 s) and lies on the sun as the scarab for **2.5 s with the head open at 100%**. Surviving the blaze is always paid |
| **The Setting** | the change | It lands on the rim furthest from the player and folds | 0 | | None |

**Hot floor, as a rule.** A hot lane or crater bites whoever stands on it for 6 every 0.5 s, the first bite 0.3 s
after stepping on. It is drawn as glowing orange paving with heat shimmer and cools from its edges. The chooser
does not lay a new lane while three are hot, so there is always ground to stand on. Under the obelisk's shadow it
is cold (section 6.3). It is the same `Hazards` patch as the dragon's storm.

**How it is hurt.** The scarab's head while the horn is stuck, and after the Zenith (100%); either shape while
exposed (100%); the body while it holds the sun (25%, but it counts toward knocking the sun away); the fallen head
when the balance is broken (160%, and a heavy blow finishes). Blows on any of these and perfect guards wear the
balance; knocking the sun away costs it 40.

**How it chooses** (`SunBearerChooser`; Claude's defaults). It opens with: Roll, Ram, Sparks, Furrows, Rising,
Stoop, Feathers, Pass, Setting, Roll, Rising, **Zenith**, which shows everything once. After that it stays in a
shape for three or four attacks (two or three as the falcon) and then changes. As the scarab: Roll 30, Furrows 20,
Sparks 25, the Ram when the player is in reach. As the falcon: Stoop 35, Pass 25, Feathers 25, Zenith 15. Nothing
twice running. The Zenith never comes within 25 s of the last one, and its fury opens with one. The Roll, the
Furrows, the Stoop, the Pass and the Zenith are heavy, and two heavy attacks earn the 2.4 s lull.

**Fury** (below half health). A quarter faster; rests 55% as long; the shadow turns at 9 degrees a second; it opens
with the Zenith, longer and with the shadow's turn-back; the Rising's flare burns; the Pass comes back; the Sparks
fall three times; the sun, loose, is taken up after 7 s at the most, not 9.

**Callouts** (timed, as the dragon's are): "The sun comes down" (as the form rises); "Khepharu, the Sun-Bearer" (at
the roar); "The sun is loose" (the first time it is knocked away, and never again); "The court blazes" (the first
Zenith); "Noon breaks" (fury).

**The entrance (4.2 s).** The king's body lies where it fell. His spear, standing in the floor, burns away to a line
of light going up. At 0.8 s the sun in the sky dims to a white ring: something has left it. At 1.4 s the sun, the
ball, comes down the line of light and lands where the spear stood. At 2.2 s the king's body is drawn into it. At
3.0 s the scarab's forelegs come round it from behind, and it lifts its face, his face, over the top. At 3.4 s it
roars, wing cases wide, and a pale ring crosses the floor.

**The stagger and the finisher.** When the balance breaks the shape drops the sun, which rolls a few metres and
goes dull, and falls on its face with the head on the floor: 5.5 s, 160%, and a heavy blow within 5 m is the
finisher (the weapon's own finisher, as built), worth at least 123. With the spear, the player drives it into the
gold face in the thorax.

**The death (5 s).** It reaches for the sun and cannot lift it. The gold runs off it and the black-bronze under the
gold goes to ash from the edges in; the wings burn back to the cases, the cases fall. At 3.0 s what is left on the
floor is the man, kneeling, small, with no gold on him. The sun rolls slowly to the foot of the obelisk and, in its
shadow, shrinks to a coal the size of a fist. The shadow stops turning. The white ring in the sky fills again and,
for the first time, it is a hand's breadth west of where it was. He speaks (section 7.5), click-through, and is ash.

### 6.5 What follows: there is no choice

*"The player has no choice here but to take Fire. Then the barque."* (item 4)

- One thing lies on the floor, not two: the coal, in the obelisk's shadow. It is a `GodReward` with `Takes = true`
  and `Element = Fire`; no second reward is built. Prompt: "Take the fire". Five embers are paid as the form dies.
- Taking it: the player's hand closes on it; Fire is held from that moment (base kit: Flame Wave and Kindle, as
  built) with the gift Falling Sun (section 8). The words "Fire is yours" stand for a moment.
- With it the player is given **the Sun-Spear** (section 8.4). See decision 3 in section 11: this follows
  `docs/GAME.md`'s rule for Take and may be the wrong way round.
- **Then the barque.** The screen goes to white and the player stands on the barque's deck in the dry river. The
  sky is no longer white: it is late afternoon, the first there has been, the light long and gold, every shadow in
  the land stretched east. On the deck is a rest fire, lit: **the first place the embers carried all this way can
  be spent**, and the first time the tree opens. The sponsor speaks there (section 7.4). A prompt at the steering
  oar, "Cast off", ends the land: the barque lifts off the mud on a river of low light and the game cuts to the
  next land. For tonight "Cast off" leads to the hub.

### 6.6 Model pieces the form needs

For the modeller's brief. Rigid pieces moved by code, as the dragon's are; sizes in metres. One file,
`boss_sunbearer.fbx`. The thorax, the face and the wing cases are shared by both shapes, which is what makes it read
as one creature.

| Piece | Size | Notes |
|---|---|---|
| `sun` | sphere 3.0 across | A burning ball with twelve short flame tongues round its middle. A second look for loose: dull red, cracked, no tongues |
| `thorax` | 2.4 long, 3.4 wide, 2.0 high | Black-bronze, gold-edged. **Shared** |
| `face` | 1.2 high, 0.9 wide | The king's gold face, set in the front of the thorax, looking forward and a little up so the camera sees it. **Shared**; what the finisher strikes |
| `wing_case_left`, `wing_case_right` | each 3.6 long, 2.0 wide, 0.5 thick | Hinged at the thorax; closed they are the scarab's back, lifted 70 degrees they are the falcon's shoulders. **Shared** |
| `scarab_head` | 1.6 long, 1.8 wide; horn 1.2 | A flat shovel head with one forward horn. The piece that sticks in the paving |
| `scarab_abdomen` | 3.0 long, 3.6 wide, 2.2 high | Under the wing cases. Its underside is pale and segmented: it is what shows when the scarab is on its back |
| `scarab_foreleg` (2) | 2.4 long, toothed | They hold the sun |
| `scarab_midleg` (2), `scarab_hindleg` (2) | 2.6 and 2.8 long | Three rigid sections each |
| `falcon_head` | 1.8 long; beak 0.9 | A falcon's head with a gold disc standing on it (0.8 across) |
| `falcon_wing_left`, `falcon_wing_right` | each 6.0 long in three rigid sections (2.4, 2.0, 1.6), 2.6 deep at the root | Gold primaries, fire between them. They unfold from under the wing cases. Span with the thorax: 14 |
| `falcon_tail` | 2.5 long, 2.2 wide at the fan | |
| `falcon_talon` (2) | 1.4 long | They hold the sun |

Whole scarab: 6.0 long, 4.4 wide, 2.6 high. Whole falcon: 5.0 beak to tail, 14 across.

Also to be modelled for the land: `boss_sunking` (the man, on the boss skeleton, with the clips below); the
**Sun-Spear** (3.2); the plain spear already exists; the **obelisk** (2.4 square at the foot tapering to 1.4 at
13.0, a gold cap 1.4 high; 14.4 in all); the **barque** (30 by 7); a seated statue (5 by 6 by 9) with two heads,
old face and new.

**The man's clips:** `sunking_idle_loop`, `sunking_walk_loop`, `sunking_hurt`, `sunking_stagger`, `sunking_death`,
`sunking_thrust_1`, `sunking_thrust_2`, `sunking_thrust_3`, `sunking_reach`, `sunking_sweep`, `sunking_butt`,
`sunking_throw`, `sunking_bare_loop` (empty hands, stepping aside), `sunking_ray` (one hand raised; the catch),
`sunking_plant`, `sunking_noon` (spear lifted to the sky), `sunking_noon_again`.

**The champion's clips:** `champion_idle_loop`, `champion_walk_loop`, `champion_hurt`, `champion_stagger`,
`champion_death`, `champion_thrust_1`, `_2`, `_3`, `champion_sweep`, `champion_butt`, `champion_throw`,
`champion_bare_loop`, `champion_recall`, `champion_plant`.

---

## 7. Lore: every word the player can read

All of it is **Proposal (Claude)**. The voice is `docs/GAME.md`'s: few words, plain and weighty, no jokes, no
exclamation marks. Carvings and paintings are in the present tense, as inscriptions are. **Everything here is
click-through** (`ReadingBox.Show`), read with the use button at the thing, one line at a time, except the callouts
marked timed, which are status lines or scares.

### 7.1 The two stories

*"the sponsor tells one story aloud; the carvings and remains in each land quietly tell another, about the god
murdered there before the usurper came."* (item 12)

- **What the sponsor says:** a man found fire lying in the desert, named himself the sun and burned his people to
  keep it. No god ruled here before him. He is a thief; take it from him.
- **What the walls say:** there was a god here, of the hearth and the dawn, who lit each morning from the people's
  own fires. One day the morning did not come. Something that is not painted stood over the god. Later a piece of
  the day fell in the desert, and a herdsman found it, and wept, and carried it home on his back for people sitting
  in the dark. The rest, the gold and the noon that does not end, came after, and the same hand cut the old name
  out of every wall.
- **Where they disagree,** and nobody remarks on it: whether there was a god before; whether the man stole or
  found; whether he took it for himself or carried it for others. The sponsor is never shown to lie about what the
  king became. It only leaves out the first wall.

### 7.2 Carvings

| | Where | The words |
|---|---|---|
| C1 | A mooring stone on the quay | "Here the boats of both banks tie up. High water or low, the river gives." / Scratched under it, later: "The river is given to the sun. Drink light." |
| C2 | The barque's prow | "The boat of the morning. It waits for one who rises." |
| C3 | The well's lid, on the gold bands | "Closed by the king's word. Water is the old debt. The sun pays every debt." / Under the rim, older, small: "Keeper of the hearth, opener of the day: the first cup is yours." |
| C4 | A hearth niche by a house door, Street of Lamps | "A fire in every house, and it is one fire. Bank it at night." / "In the morning" and then a name, cut away, "lights the day from it." |
| C5 | The lintel of the embalmers' yard | "The king says: you burn and are not consumed. Wrap them. Stand them in the sun. No one here is buried." |
| C6 | A tablet in the Embalmers' Gate, by the pinned soldier | "The guard of the golden way wears the god's flesh. Whoever refuses the pouring holds the lower gate, and holds it until he is relieved." / Nobody relieves him. |
| C7 | The base of the first statue | "Khepharu, who is the sun. Who is not born. Who does not set." / Under it, shallow, half ground away: "...who carries the day up from the river for us." |
| C8 | Mason's marks on the scaffold of the unfinished statue | "The old face still shows. Cut deeper." |
| C9 | The pylon, over the door of the hall | "Look on him and see." / Below, by another hand: "They look." |
| C10 | The gate of noon | "Noon is the king's hour. There is no other hour." |
| C11 | The obelisk's foot, one face at a time, in the forecourt's view and again in the court | East face: "He rises." South face: "He stands." North face: "He does not set." The west face is smooth. Nothing is written for evening. |

"Nobody relieves him" (C6) and the last two sentences of C11 are the reader's own eyes, shown as a line of plain
description after the carving, in the same box.

### 7.3 The painted tombs

Five paintings, each one line of description in the reading box, and two carved captions. First chamber: I and II.
Second: III and IV. Third: V.

| | The painting, as the player is told it |
|---|---|
| I | A river in flood, painted blue. People on both banks hold up small lamps. A figure with a lamp for a head walks ahead of the rising sun and lights it from theirs. |
| II | The lamp-headed figure lies on its side. Its lamp is out. Stars are falling. Beside it the plaster is left bare, in the shape of something standing. |
| | Caption: "The morning does not come. We bank our fires and wait. We do not know what we have done." |
| III | A star lies burning in the desert. A herdsman kneels to it with open hands. He is weeping; his tears are the only blue on this wall. |
| | Caption: "A piece of the day falls where a man can reach it." |
| IV | The herdsman carries the star home on his back, bent double, as a beetle carries its ball. People come out of dark houses. |
| V | Newer paint, and gold leaf. The same man, seated, the star for a crown, a falcon's wings behind him. Where the gold has flaked, the lamp-headed figure shows under it. It was never seated. It is kneeling, and holding out a light. |

After V (timed, a scare): "Something behind you is warm."

### 7.4 The sponsor

Speaker name "The Voice", as built (`SponsorLines`). It speaks unasked on waking; at a rest fire it speaks the
first time that fire is lit, before the fire's menu opens, and not again.

| When | Lines |
|---|---|
| Waking, in the dry river | "You are awake. Good." / "You do not know your name. I took it, with everything behind it. That was the price of pulling you out." / "This is the sun-king's country. There was a river here." / "A man found fire lying in the desert and named himself the sun. No god ruled here before him. He is the first, and he is a thief." / "I have nothing to lend you. Your hands will do. What you need, he has." / "If you fall I will turn the hours back. Not far. To the last fire you lit." |
| F1, the first fire | "Light it." / "Fire answers anyone who asks. That is how he began." / "I can reach you at a fire. Nowhere else, in this light." |
| F2, the quay | "They drank the river and are thirsty still." / "He told them the sun would be enough." |
| F3, the gate | "Take the spear. Its owner is done with it." / "He carries one like it. His is better. It will be yours." |
| F4, the golden way | "Every face on this road is his." / "A man who cannot bear to see another's." |
| F5, the porch | "Past the gate is his court. He will tell you he carried it home for them." / "Look at them. Then listen to him." |
| F6, the god's door | "My voice does not carry past this line. In his court it is his hour, not mine." / "Take what he took. All of it." |
| The first time back at a fire after a fall in this land | "You fell. I turned it back as far as the fire." / "They are standing again. So are you." |
| On the barque, after | "You have it." / "It burns the hand that holds it. You will learn where to hold it." / "The sun is moving. That is your doing. Do not stay to watch it set." / "The boat knows the way down the sky. Cast off." |

### 7.5 The god

| When | Lines |
|---|---|
| The gold line crossed (click-through; he does not move until it is read) | "Another, sent with empty hands." / "I know what sends you. It was here before the star fell." / "I carried the day home on my back. They were sitting in the dark. Look at them now. Warm." / "Come. It is noon." |
| At half health, as he plants the spear (timed) | "It is always noon." |
| Dying, as the man, kneeling (click-through) | "The light is low." / "I had forgotten evening." / "Ask it what became of the morning." / "Carry this better than I did." |

"It" in the third dying line is the sponsor. It is the land's one loose thread and is not picked up here.

### 7.6 Small words

Area title card (`PlayerBuilder.AddTitle`): "The Sun-King's City", "where a herdsman became the sun". Court title:
"The Court of Noon". Prompts: "Rest", "Take the waterskin", "Pull the spear free", "Read", "Take what was left for
the dead", "Open the gate of noon", "Take the fire", "Cast off". Callouts not given elsewhere: "A fourth flask";
"Taken back"; "It drinks."; "Sealed. The Champion of the Gate holds the way."; "It is noon within."; "Fire is yours."

---

## 8. The Fire god's gift: Falling Sun

### 8.1 As built

"When Kindle is lit a sun rises over the fight. While it hangs, every burn near you deepens by itself. When Kindle
ends, or when you press the surge button again, it falls on the enemy you face, heavier for every burn it finds. A
blow taken puts it out." In numbers (`SunForm`, `FireTree.FallingSun`): it hangs 3.8 m over whoever the player
faces; once a second every burn within 6 m of the player deepens by a stack; it falls in 0.32 s for 60 damage and
80 stagger in a 4 m circle, plus 8% for every burn stack under it up to double; the ground burns for 4 s.

### 8.2 Does it already suit a nod to the sun god

Half. It is a sun, and it rises and falls, and that is the right gift for this god. But it is anybody's sun: it
appears at noon and drops. Nothing in it is the thing the owner chose for this god, which is that the sun is
**carried**: a beetle at dawn, a falcon at noon. And the fight the player has just won taught three things with the
sun (it is rolled, it is carried up, it comes down) that the gift could hand back to them.

### 8.3 Recommended: the sun's road

The gift keeps its name, its damage and its place in the tree. What changes is that the sun now **makes its day's
journey across the fight**: rolled out at dawn by a scarab of embers, carried up at noon by a falcon of fire, and
set at dusk. One design, with numbers:

| Station | When | What happens |
|---|---|---|
| **Dawn** | Kindle is lit. Lasts until the sun reaches the enemy faced, or 2.5 s | A small sun (1.0 m) rises out of the ground at the player's feet with a scarab of embers behind it, and is **rolled along the ground toward the enemy the player faces** at 6 m a second, up to 9 m. It follows that enemy if it moves. **The ground it crosses burns**: a trail 1.6 m wide for 4 s, 4 every 0.5 s and a stack of burn. The sun itself strikes the first enemy it meets for 14 damage, 20 stagger and two stacks |
| **Noon** | From the end of dawn until Kindle ends | A falcon of fire stoops, takes the sun and **carries it up** to hang 3.8 m over the enemy faced, grown to 1.35 m. While it hangs, **once a second every burn within 6 m of the player deepens by a stack** (as built) |
| **Dusk** | When Kindle ends, or when the surge button is pressed again at any time after dawn | **It sets**: the falcon lets go and it falls in 0.32 s on the enemy under it. 60 damage, 80 stagger, 4 m, plus 8% for each burn stack it finds up to double; the ground burns 4 s (all as built) |
| **Called at dawn** | The surge button pressed again while it is still being rolled | It bursts where it is at half weight: 30 damage, 40 stagger, 2.5 m |
| **A blow taken** | At any station | **It goes under**: it sinks into the ground where it is, with no blow. If Kindle has 3 s or more left, it rises again at the player's feet 1.5 s later and begins again from dawn. (As built a blow puts it out for good; the sun that comes back is this god's sun, and a blow still costs the noon already earned) |

Kindle lasts 10 s as built, so an untroubled road is about 2 s of dawn, 8 s of noon, and the setting. The trail at
dawn is new damage and is small on purpose: it is there so that the first thing the gift does is put burn on the
ground between the player and the enemy, which is where the player is about to fight (pillar 2: it brings the
weapon in, it does not replace it).

**New code:** two more phases in `SunForm` (dawn before the hanging phase, and the going under), a `Hazards` trail
laid behind the rolling sun, and two small shapes of fire (scarab, falcon) from the form kit's meshes. The
`SpellSpec` keeps its numbers.

**Its description, in the game's voice** (replaces the `Description` on `FireTree.FallingSun`):

> "Light Kindle and the sun begins its road. At dawn it is rolled along the ground to the enemy you face, and the
> ground it crosses burns. At noon it is carried up, and every burn near you deepens by itself. When Kindle ends,
> or when you press the surge button again, it sets on the enemy below it, heavier for every burn it finds. A blow
> taken sends it under, to rise again."

### 8.4 The Sun-Spear

His own weapon (item 10), in the manner of `Weapons.StormKatana`: the Great Spear's moves with 15% more damage and
stagger on every step, a warm white-gold `SlashColour`, and its skill renamed **Returning Ray**: the spear is thrown
and sticks as the Flying Spear does, and then, in place of the bearer running to it, **it comes back to the hand as
a ray** along the line between them (a lane 1.2 m wide, 22 damage, 26 stagger, two stacks of burn on everything in
it). It is what he did to the player, handed over. Whether the player gets it here is decision 3 in section 11.

---

## 9. Build order for tonight (about three hours)

The aim: by the end the land is **playable from waking to taking Fire** at stand-in quality. Everything is blocks;
enemies wear the castle's bodies; special rules come last. After each step: `unity --json recompile --timeout 120`,
read the console, and for the scene steps build it and look at a capture (the `unity-check` skill).

**Stand-in bodies.** `EnemyBuilder` finds clips by `Prefix`. Until the new models exist, give each new kind an
existing prefix and model and point each attack at an existing clip through `EnemyAttack.Animation`, as
`boss_blink_cut` does:

| New kind | Stand-in model, prefix | Clips to borrow |
|---|---|---|
| The Thirsting | `enemy_acolyte`, `acolyte` | `acolyte_slash_1`, `acolyte_slash_2`, `acolyte_leap` (pounce and snatch) |
| The Wick | `enemy_caster`, `caster` | `caster_nova` (swipe, slam), `caster_bolt` (grab) |
| Gilded, all ranks | `enemy_soldier`, `soldier`, with a gold `Palette` | `soldier_thrust`, `soldier_sweep`, `soldier_lunge` |
| Sun-Blind Priest | `enemy_caster`, `caster` | `caster_bolt`, `caster_nova` |
| Champion of the Gate | `enemy_soldier`, `captain` | the captain's clips, one for one |
| Khepharu, the man | `boss_narukami`, `boss` | `boss_slash_1..3` (thrusts), `boss_thrust` (reach), `boss_iai` (throw), `boss_call` (noon), `boss_roar` (plant) |

| # | Minutes | Build | Done when |
|---|---|---|---|
| 1 | 0:00 to 0:25 | `SunCityBuilder`: copy `CastleBuilder`'s helpers (`Slab`, `Wall`, `Block`, `Pillar`, `Stair`, `Disc`, `Rest`); the `Noon` mood; the palette's materials; the floors, banks, quay, streets, avenue, hall floor and gate court of section 3 as bare slabs and walls; the start point; the five rest fires; **no** `AddRunEnd`; the title card | The player can run from (0, 0, 4) to (0, 4, 498) and rest at each fire |
| 2 | 0:25 to 0:35 | The player starts with fists and **no element** (section 10, point 1) | The spell and surge buttons do nothing; a rest fire heals and does not open a tree |
| 3 | 0:35 to 1:10 | The six kinds as **data only** in `EnemyKind` (`All` and `ByName` too): the thirsting without the snatch; the wick with the grab as a plain red lane of 18; the three gilded ranks in full (`gilded_plant` as a red circle without the burning ground); the priest as a caster with its ray and brand and ordinary sight; the champion with thrusts, sweep, butt, and `captain_charge`'s shape standing in for the throw. Palettes. EditMode test: every kind's attacks have names and a clip to borrow | The tests pass |
| 4 | 1:10 to 1:30 | Place every enemy of section 3 (`Spawn`), `HoldGround` for the champion, the `SealedWay` to `SunCourt`, the spear `WeaponPickup` at the gate, the waterskin at the well (court and well as blocks) | A play-mode probe: walk the land, fight one of each, the gate opens when the champion dies |
| 5 | 1:30 to 2:05 | `SunCourtBuilder`: the disc, the obelisk as a tapered block, the forecourt and F6, the gold line; `ShadowHand` with its tests, the strip drawn and turning, the key light turning with it; Khepharu the man as data (thrusts, reach, sweep, butt, noon shafts that respect the shadow; the plant as a red circle of 7 m; the throw as a red 14 m lane with no return) with the boss bar | He can be fought and killed; the shadow turns; a shaft is never marked in it |
| 6 | 2:05 to 2:45 | `SunBearer : GodlyForm`, **the scarab only**, from blocks and a sphere: the Roll with its hot lane, the Ram with the stuck head, the Zenith with the blaze and the shadow's test (the scarab throws the sun up and stands under it; no falcon), the sun knocked away by a perfect guard, the exposed state, balance, fallen head, finisher, death. Copy the skeleton of `StormDragon` (marks, rest, lull, fury flag); do not copy its body code | The form can be beaten, and standing outside the shadow in the blaze kills |
| 7 | 2:45 to 3:00 | The coal as a `GodReward` that only takes; taking it gives Fire and leads to the hub. Run all EditMode tests; one probe from the gate of noon to the coal; capture three frames (river, hall, court) for the owner | Waking to Fire, end to end |

**If step 6 will not fit:** end the land at the man's death and lay the coal there. The form is the piece most
likely to overrun (the dragon's class is 3,200 lines), and a land that ends cleanly is worth more in the morning
than half a form.

**What can wait, in the order it should come back:**
1. The special rules: the wick's hold; the thirsting's snatch; light and shade for the priests (`Sunlight`);
   dormant wicks; the ember rule of section 11, decision 2.
2. The champion's and the king's throw-and-return and the ring of fire as a real ring.
3. The falcon, the change of shapes, the Furrows, the Sparks, the Stoop, the Pass, the Feathers, pushing the loose
   sun, the fury's turn-back.
4. Every word of section 7 (carvings as `ReadingBox` text on small `Interactable`s; the sponsor's lines at fires).
5. The side paths' dressing, the tombs' paintings, the barque as a scene, the late-afternoon light.
6. The reworked Falling Sun and the Sun-Spear.
7. The look pass (statues, pylon, houses' doors and niches, reed boats, heat haze), shot stations and goldens.
8. The models and their clips. Sound, when sound is taken up.

---

## 10. Where this departs from the code, or could not find something in it

1. **No element.** `PlayerElement.StartingElement` is Fire, and an empty `RunState.Element` falls back to it. There
   is no state of holding nothing. The land needs one: no spells, no surge, no tree at a rest fire, mana still
   gathered (the Great Spear's skill spends it).
2. **Death.** As built a fall ends the run and wakes the player in the hub (`RunEnder`). The land follows item 6
   and leaves `RunEnder` out, which gives the older behaviour (`AreaDirector.OnRevived`: back to the last fire, the
   enemies restored). The last fire lit is not remembered across the two scenes; F6 in the court's own scene covers
   that.
3. **Embers with no tree.** Embers are spent in the element's tree and the player has no element until the end.
   About 44 are earned on the main path (the castle gives about 14), 51 with the side paths, and 5 from the god.
   This document has them carried to the barque. See decision 2.
4. **Stagger to the player.** `EnemyAttack` has no such field; the tables use the built flinch.
5. **Things enemies cannot yet do:** take a flask; hold the player; lie dormant; notice by a rule other than
   distance; throw a weapon that stays and comes back; drop their guard for a time; raise a ring. Each is marked
   **New code** where it is used.
6. **No ring mark.** `MarkerShapes` makes lanes and sectors (and the circles the code already draws). A ring is
   drawn as sixteen circles until one is made.
7. **"A third of the player's health comes back"** between a god's phases in `docs/GAME.md` is 35% in
   `SummitDirector`. This document says 35%.
8. **The roof of the hall** is drawn as shadows only so the camera can see in. That has not been tried in this
   project; if it reads badly, the painted light and shade on the floor carry the rule without it.
9. **"It is always noon"** is a proposal in `WORLD.md`, and an obelisk whose shadow turns needs a sun that moves.
   This document keeps both: noon everywhere, and in his own court the king turns the sun. If the owner would
   sooner have the shadow turn for no stated reason, nothing else changes.
10. **Forty-two enemies in one scene** is over three times the castle. Enemies beyond their aggro range do little, but
    nothing has been measured; if the frame suffers, `AreaDirector` should put to sleep those more than 45 m off.
11. **Not read:** `SummitBuilder`'s rim and cloud code beyond its sizes; `StormDragon.cs` beyond what
    `docs/GAME.md` records of it; `DragonChooser.cs` (its rules were taken from `docs/GAME.md`); the element
    trees' current ember costs.

## 11. What the owner should confirm

1. **Shade is the safe side, everywhere.** The sun-blind find a player who stands in light and lose one who stands
   in shade (not the other way about), so the hall teaches the obelisk's rule before the god asks it.
2. **Embers in a land with no tree.** Recommended: they are carried to the barque and spent there, and **a fall in
   this land does not take them** (the loss of unspent embers on death is Claude's proposal in `WORLD.md`, and here
   there is nowhere to spend them). The other way is a small tree of the body that needs no element.
3. **Whether Fire comes with the Sun-Spear.** There is no choice at this god. `docs/GAME.md` gives the god's weapon
   on Take; the owner's note of 2026-11-17 gives it only on Consume, which would mean the first god's weapon cannot
   be had at all on a first climb.
4. **The name Khepharu**, and that the form is one body seen as scarab on the floor and falcon in the air, never
   both at once, with the king's face in its chest in both.
5. **How the sun is knocked away:** a perfect guard against the sun itself, or 30 damage to whichever shape holds
   it during one attack; and that the opening lasts longer the further the sun lies. This is the heart of the
   second phase and has never been played.

**The owner's answers (2026-11-17, morning).** 1: leaning yes, and more: times of high heat across the land when the
player must find shade or burn. 2: embers are lost on a fall, but the enemies killed for them carry them again and
give them back when killed again. 3: Take gives the god's weapon, so Fire comes with the Sun-Spear. 4: the name
Khepharu stays; the form's shape is to be looked at in the afternoon. 5: undecided; a parry made for the sun is
welcome as a proposal. The sponsor's voice lines stay.

Also worth a word, if there is time: the fourth flask at the well (a permanent gain in the first hour); the wick's
hold never killing; Falling Sun rising again after a blow where it now goes out; and the land's length, which sits
at 36 to 38 minutes, the top of the 30 to 40 asked for.

---

## As built

### 2026-11-17, first worker: the models, the look, no element, the four ordinary enemies, the test yard

Built by Claude in one unattended night run. Nothing here has been played by the owner. Seen in `FireProbe` and its
frames (`review/shots/current/fire_*.png`) and in the contact sheets (`review/anim/thirst_*`, `wick_*`, `gilded_*`,
`sunblind_*`).

**Where things are.**

| What | Where |
|---|---|
| The kinds and their numbers (section 4) | `Runtime/Gameplay/EnemyKind.Fire.cs`: `EnemyKind.ThirstName`, `WickName`, `GildedLevyName`, `GildedSpearmanName`, `GildedWardenName` (or `GildedName(rank)`), `SunBlindName` |
| The new rules in the enemy | `Enemy.cs` (dormant, the snatch, the hold, hunting by light, burning ground), `EnemyBrain.Occupy` and `EnemyState.Busy`, `EnemyTurns.AnswersDrink` |
| The rules as plain classes with tests | `World/Sunlight.cs`, `Gameplay/SunSense.cs`, `Player/Grip.cs`, `Gameplay/FlaskSnatch.cs`; tests in `Tests/EditMode/Gameplay/FireLandTests.cs` |
| A scene's light and shade | `World/SunlightZones.cs` (a component the builder fills) |
| No element | `PlayerElement.StartsWithNone` and `HasElement`, `Elements.None`, `Elements.StartsEmpty`, `RestPoint.NothingToFeed` |
| The clips | `Editor/Forge/FireAnimations.cs`, baked by `Clover > Forge > Bake Enemy Animations` |
| Materials and kit pieces | `Editor/Builders/SunCityKit.cs` (`For`, `Dress`, `Place`, `Panel`, `FormPiece`, `Mud`, `Brick`, `BrickDark`, `Stone`, `Shade`); materials in `Generated/SunCity` |
| The mood | `LookBuilder.Noon`, `NoonSunDegrees`, `NoonShadowLength(height)` |
| The yard | `Editor/Builders/FireYardBuilder.cs`, `Scenes/FireYard.unity`, saved with `SceneBuild.FinishOutsideBuild` |

**Built to this document.** Every number in the four tables of section 4 (health, balance, poise, speeds, ranges,
embers, every attack row). The snatch, points 1 to 5. The hold, points 1 to 5. Dormant wicks (wake distance, 1.2 s to
rise, double damage until risen). The rank table of the gilded. The priest's rule, points 1 and 3 to 5. The mood's
table of values.

**Where it departs, and why.**

1. *The gilded carries a shield.* Section 4.3 says "a spear and no shield"; the model has a tall hide shield on the
   left forearm and the night's brief asked for spear and shield. He holds the spear in the gold hand alone.
2. *Gold by rank follows the model, not the fractions in the table.* Rank 1 is the mask and the runs on the neck and
   chest; rank 2 adds the whole right arm and the runs on it and up the spear; rank 3 adds the cuirass, kilt panel,
   shield rim, left forearm, shins and feet and the splashes and puddles. Slots ending `_r2` and `_r3` are gold from
   that rank up; below it the gold-only pieces are drawn with a material that draws nothing.
3. *One wick dressing.* `Palette` "wick" and "wick_wrapped" are not two looks: the one model is the wrapped dead with
   charred hands. No smoke thread yet.
4. *The priest's staff carries a sun, not a mirror* (the model's), and his brow disc does not light when he has found
   the player: nothing shows found from searching but what he does.
5. *The yard is open sky*, so the light rule runs the other way round from the hall: everything is lit except the
   registered shade (`Sunlight.SetOpenSky(true)` and `AddShade`). The hall will register `AddLight` rectangles under
   a closed sky; both are tested. A flare lights its circle either way.
6. *Burning ground* (the planted spear) is kept by the enemy that made it, since `Hazards` bites enemies only. It
   bites 4 every 0.5 s for 3 s as written, but each bite is an ordinary blow: the player flinches and is pushed a
   metre outward, so someone standing still takes one or two bites and is put out of the ring, not six. It stops if
   the warden dies. There is no mark on the ground for it beyond the flames.
7. *The fight a priest can feel* is any blow of his that lands within 8 m of him, or any blow he takes; a ray that
   lands farther off tells him nothing.
8. *The tip after a first snatch* (4.1, point 6) is not written. The hold's prompt is a status line, "Break free:
   strike or dodge", once a sitting.
9. *A dormant enemy can still be picked by soft aim.* 4.2 says it should not be.
10. *`thirst_kneel_loop`* is used through the same dormant flag (wake distance 3 m, 0.4 s to get up, no extra
    damage): `enemy.MakeDormant(3f, "thirst_kneel_loop")`. A wick on a table is
    `MakeDormant(4f, "wick_lie_loop")` and rises with `wick_rise_lying`; in a doorway `MakeDormant(3.5f)`.
11. *A rest fire with no element* says "Rested. Nothing to feed yet." (a status line) and does not open the chart.
12. *Noon's shade colour.* The toon shader's shade colour is set on every material of the land to (0.56, 0.53, 0.66),
    lighter than the castle's, so the south faces the camera sees (the sun leans north, so they are all in shade)
    are a grey-violet and not navy. The mud and brick are plain colours: the stone textures made for the castle carry
    its blues and reds.
13. *The roof drawn as shadow only* (section 10, point 8) was tried on the yard's porch and reads: the shade is a
    hard dark rectangle and the camera sees under it. The columns thin out when they stand between camera and player.

**For the next builder.** Place an enemy with `EnemyBuilder.Build(EnemyKind.WickName, position, yaw, kit, parent)`
exactly as the castle does, then `kit.Turns.Configure(all)` and `director.Configure(...)`. Do not call
`PlayerBuilder.AddRunEnd`. Set `PlayerElement.StartsWithNone = true` and `StartingWeapon = Weapons.FistsName` on the
player. Kit pieces have their fronts toward +Z: `SunCityKit.Place("column_lotus", parent, position)` turns them 180
to face the camera by default, with a box collider round the whole piece unless `solid` is false (give the pylon and
doorways their own colliders). `column_lotus` stands at y 0.5 on `column_base`; `statue_head_new` goes at
(0, 8.0, -1.1) from `statue_king` when both are turned 180; `barque` wants scale 0.73. `mud_tile` is laid at y -0.19
over a slab whose renderer is off, so its plates lie at the height of the ground. For the hall, add a
`SunlightZones` and `Configure(false, null, lightRectangles)` from the same list that cuts the roof; the shadow of a
roof at height h falls `LookBuilder.NoonShadowLength(h)` north of it.

### As built: the city (2026-11-17, second worker)

Built by Claude in one unattended stretch of about two hours. Nothing here has been played by the owner. Seen in
`SunCityProbe` and its frames (`review/shots/current/suncity_*.png`) and in the champion's contact sheets
(`review/anim/champion_*`). Sections 3.2 to 3.5, section 5 in part, and the carvings and paintings of section 7.

**Where things are.**

| What | Where |
|---|---|
| The scene and its builder | `Scenes/SunCity.unity`, `Editor/Builders/SunCityBuilder.cs`, menu `Clover/Build/Sun City`; saved with `SceneBuild.FinishOutsideBuild`, so it is not in the build list |
| Death returns to the last rest fire | `World/LandReturn.cs`, a component the builder puts on the Area Director's object. Flags: `LoseCarriedEmbers` (the cost proposed in WORLD.md; switch it off to keep embers), `ElitesStayFallen`, `SleepBeyond` (metres). The plain rule is `Gameplay/ReturnRule.cs`; `AreaDirector.ElitesStayFallen` is false unless a `LandReturn` sets it, so the castle's captain still returns as before. `AreaDirector.ReturnPoint` says where a fall goes now |
| The end door | `World/GateOfNoon.cs` on "Gate Of Noon" at (0, 4, 498.8). `LeadsTo = "SunCourt"`. It loads that scene only if `Application.CanStreamedLevelBeLoaded` says the game has it: build `SunCourt.unity` and put it in the build list (or it stays a status line, "It is noon within.") |
| The champion | `EnemyKind.Champion()` / `EnemyKind.ChampionName` in `EnemyKind.Fire.cs`; model `elite_gilded.fbx`; clips `champion_*` made in `FireAnimations.Champion` and baked with `EnemyAnimations.Bake("champion")` |
| A rest fire | `SunCityBuilder.Rest(parent, director, foot)`: the kit's brazier, a mat, a gold ring, a `RestPoint`; the return point is 1.6 m south of the foot. Add the call and rebuild |
| A carving | `SunCityBuilder.Carving(parent, foot, yaw, what, lines...)`: a pale stele with a gold cap carrying a `Notice` (click-through) |
| The waterskin and the grave dish | `World/LandGift.cs` (`FlasksAfter`, `Embers`, `Words`) |
| The last painting that wakes the tomb's wicks | `World/WakingNotice.cs` |
| The hall's light | `SunCityBuilder.HallLight`: one list makes the pale floor, the holes in the shadow-only roof and the `SunlightZones` rectangles |
| Ground helpers | `Paving` (flagstones in half bond, tones, missing stones), `Scatter` (patches and stones on bare earth), `Drift` (sand), `Trodden` (a walked path), in the builder; `SunCityKit.Flat` for a plain colour |

**Built to this document.** The three districts at the spec's sizes and heights, the five rest fires at (or within a
few metres of) their positions, all 42 enemies of the tables (14 thirsting, 15 wicks, 5 levies, 2 spearmen, 1 warden,
4 priests, the champion), both side paths, the gold line and the champion's ground (`HoldGround(-19, 19, 470.5, 500)`),
carvings C1 to C10, the five paintings and two captions, the scare, the waterskin (a fourth flask), the grave dish
(three embers), the spear at the Embalmers' Gate (`WeaponPickup`, the Great Spear as built), the title card.

**Where it departs, and why.**

1. *The barque is a landmark only.* There is no gangplank and nobody on the deck; E1.4's third thirsting stands on
   the mud beside the hull. The kit's barque at scale 0.73 is 24 m by 9.5 m with its oars; only the hull is solid, and it never thins out
   for the camera (a player close under its north side can be hidden by it); it stands at
   (5.5, 0, 84), west of the spec's place, so that the hull is in the camera's view from the path up the west side.
   The seated statues of the golden way likewise stand at x 14.2, not 17, so their feet and laps are in view from the road.
2. *The Embalmers' Gate passage is 12 m wide* (x -6 to 6), not 6, so the fire, the tablet and the pinned soldier fit
   in it. The fire (F3) is on the west side at (-4, 2, 240); the soldier and the spear on the east door at (5.4, 2, 243.4).
3. *The pinned soldier is the gilded model standing in its bind pose* against the door with a plain shaft through
   him. He does not slide down when the spear is taken. If the Great Spear was already drawn in an earlier run the
   pickup is not there (`WeaponPickup` as built hides a weapon the hub already offers).
4. *The painted tombs lie 6 m further west* than the spec (chambers x -35.5 to -26, -45.5 to -36.5, -56 to -46.5), so
   the stair down can be walked (3 m down over 6 m). The six kit panels carry the five paintings, matched to what
   each panel shows in the game: `tomb_1_hearth` (people round a seated figure and a fire) I; `tomb_2_star` (people
   pointing at a falling star) II and its caption; `tomb_3_bearer` (a man carrying the star to a city) III, its
   caption and IV; `tomb_4_king` (a seated king) the first sentence of V; `tomb_6_flaked` the rest of V, and reading
   it wakes the wicks. **The match is loose**: no panel shows the lamp-headed figure lying dead, nor the weeping
   herdsman, so I and II and III describe more than is painted. `tomb_5_recut` (masons on a scaffold recutting a
   seated statue) has no words in section 7.3; it carries one line that is **Claude's, not the spec's**: "Masons on a
   scaffold at a seated figure. They are cutting its face again."
5. *The avenue's levels follow its stairs*: the spec puts E3.3 at y 4 but that ground (z 342 to 386) is at y 3.
   E3.2 stands at z 348 and 351, clear of the scaffold.
6. *No patrol.* The second priest stands at (-10, 4, 430); enemies cannot yet walk a beat.
7. *The champion's throw is a stand-in.* `champion_throw` is a red lane 12 m long that he follows down with the
   spear (the captain's charge in shape); there is no spear left standing, no bare-handed opening and no recall.
   `champion_plant` is a red circle of 4.2 m round himself whose ground then burns for 6 s (the warden's rule, larger),
   not a ring 5.5 m out round the spear. `champion_throw_twice` is the lane twice. His clips are the gilded soldier's
   poses made again on his own body with his own timings, not new drawings; he has no idle with the spear grounded
   that differs from the soldier's.
8. *His bar* is the elite's: the name over his own bars above his head, as the Gate Captain has. There is no
   screen-wide bar for elites in the framework.
9. *The sponsor speaks through `World/LandVoice.cs`* (the "Land Voice" object; its words are set in the builder):
   the waking lines 2.5 s after the scene starts, once per saved game (progress key `suncity.woke`); each fire's
   lines the first time that fire is lit; the fall lines after the first fall of a sitting. All click-through. F6's
   lines and the barque's are not in (those places are not built). The hub's `Sponsor` is not used here.
10. *Rest fires look lit before they are lit*: the kit's brazier carries its flame in the model. Lighting adds the
    point light only.
11. *Shadows are a little softer here* (`shadowStrength` 0.72 on the key light, in the builder) and the hall's shaded
    floor is a pale stone, because at full strength the hall under its roof was near black.
12. *Enemies far off sleep*: beyond 55 m, one that has not noticed anyone stops animating
    (`AnimatorCullingMode.CullCompletely`) until the player comes near. Nothing else about it changes.
13. *Not built:* lamp niches that light near a wick are fixed (two are always lit); no heat haze; no smoke on the
    wicks; no shot stations or goldens for the city; `SunCourt.unity` does not exist.

### As built: the court (2026-11-17, third worker)

Built by Claude in one unattended stretch of about two and a half hours. Nothing here has been played by the owner.
Seen in `SunCourtProbe` and its frames (`review/shots/current/suncourt_*.png`), the aspect sheets
(`review/shots/current/sheets/aspect_court_pass.png`, `aspect_court_blaze.png`) and the king's contact sheets
(`review/anim/sunking_*`). Sections 3.6, 6.2 to 6.5 and the lines of 7.4 and 7.5.

**Where things are.**

| What | Where |
|---|---|
| The scene and its builder | `Scenes/SunCourt.unity`, `Editor/Builders/SunCourtBuilder.cs`, menu `Clover/Build/Sun Court`; saved with `SceneBuild.FinishOutsideBuild`, so it is not in the build list |
| The gate of noon leading here in the editor | `GateOfNoon.LoadOutsideBuild`, a loader the editor sets as play begins (`Editor/Builders/EditorSceneLoads.cs`, `EditorSceneManager.LoadSceneInPlayMode`). A built game never has it: the door stays a status line there until `SunCourt` is put in the build list |
| The obelisk's shadow | `Gameplay/ShadowHand.cs` (plain, tested); the court's director owns one and gives it to `Sunlight.Hand`, so `Sunlight.IsLit`, `InHand` and `OutOfHand` answer for it and the sun-blind's rule and the god's are one rule |
| The court's director | `World/SunCourtDirector.cs`: turns the shadow, the drawn shade and the key light; the gold line, the king's words and the wall of heat; the thrown spear standing in the floor and the ray; the man's fall and the form's rising; the form's fall, the coal, the sponsor's words and the way out; a fall mid-fight |
| The man | `EnemyKind.SunKing()` / `EnemyKind.SunKingName` in `EnemyKind.Fire.cs`; model `boss_sunking.fbx` at 1.25; clips `sunking_*` drawn in `Editor/Forge/SunKingAnimations.cs` and baked with `EnemyAnimations.Bake("sunking")` |
| The form | `Gameplay/SunBearer.cs` (a `GodlyForm`), `SunBearerAttack.cs`, `SunBearerChooser.cs` (plain, tested); built by `Editor/Builders/SunBearerBuilder.cs` from `boss_sunform.fbx` with the offsets of the modeller's notes |
| Taking Fire | `GodReward.Configure(SunCourtDirector)`: takes only; someone holding no element loses no tree; the weapon in the hand is kept; "Fire is yours." |
| Tests | `Tests/EditMode/Gameplay/SunCourtTests.cs` (12) |

**Built to this document.** The court's size, the obelisk at its centre, twelve hours on a gold ring at radius 17, the
seated colossus at the north, the forecourt with rest fire F6 outside the fight, the gold line at z -20 and the wall
of heat. The shadow's shape, its test, its three rates (4, 6, 9 degrees a second), the light turned along it, its
three effects, and the fury's stop and turn-back. Every number in the man's table but the rows listed below. The
form's health, balance, finisher, fury, the sun's two ways of being knocked away, the exposed state (4 s, then it
walks to the sun at 3 m a second, 9 s at most, 7 in fury), the loose sun pushed 3 m by a heavy blow and 1.5 m by a
light one, hot floor (6 every 0.5 s, 0.3 s after stepping on, cold under the shadow, no new lane while three are
hot), and all ten rows of the two attack tables with their timings and damage. The opening order, the shares, the
ram for someone who stands within 9 m for 1.2 s, the Zenith's 25 s, the 2.4 s lull. The five callouts. The king's
three sets of lines and the sponsor's four on the barque, word for word. Five embers when the form dies.

**Where it departs, and why.**

1. *Sizes of the form's marks are read as widths.* "A pale circle of 3.4 m", "a red circle of 5 m", "circles of 2.2 m"
   and "1.8 m" are drawn 3.4, 5, 2.2 and 1.8 m **across** (the sun is 3 m across and the falcon's shadow 5), where the
   man's table means radius. The Rising's flare in the fury is a radius of 6 m, as its answer says.
2. *The thrown spear and the ray are the same line.* `sunking_throw` is a red lane 14 m long; the spear then stands in
   the floor at its end (a stand-in spear of three primitives; his own is hidden by shrinking the `weapon.R` bone) and
   he stands empty-handed with one hand raised for 1.8 s; `sunking_ray` is a red lane 2.2 m wide **from him along the
   way he threw**, not to wherever he has stepped: he takes no steps aside. An enemy already has no guard while it
   makes an attack, so he has none; blows do **not** throw half as much stagger again.
3. *The planted spear is a filled red circle of 5 m round him* whose ground burns for 8 s (the warden's rule, larger),
   not a ring 7 m out. The burning ray below half is not built.
4. *The man's second half* adds `sunking_drive` and `sunking_drive_sweep` (the long thrust from standing with the sweep
   after it, no rest between) beside the plain three thrusts; it is not a coin toss on the third thrust.
5. *A shaft of noon aimed at someone in the shadow* is laid its own radius and half a metre clear of the shadow's
   nearer edge, so none of it falls in the shadow.
6. *The Furrows' later lanes* are aimed where the player has gone (5 m to their side of the last lane if they have
   stayed near it), from where the scarab ended; they are not parallel and evenly spaced. The scarab swings the sun
   round itself as it turns.
7. *The Pass's lane is laid across the line from the obelisk to the player*, at least 5.5 m from the centre, so
   neither the sun nor a wing passes through the stone; someone hugging the obelisk is beside it, not in it. The
   falcon banks away from the obelisk. The Stoop's circle is kept 4.6 m from the centre for the same reason, and the
   scarab walks round the obelisk, never nearer than 4.2 m.
8. *The falcon rides the north of the court only* (115 degrees either side of north, 12 m up, 15 m out), so it does
   not hang between the camera and a player in the south. When its body or the sun does cover the player (a stoop,
   a pass) the dragon's finder ring is drawn through it.
9. *The camera.* The spec's "1.7 times further off and 15 degrees lower" put the obelisk across the middle of the
   picture and the player under the boss bar. As built: twice as far and **8 degrees steeper**, leaning up to 4.5 m
   toward the middle of the court. The duel with the man is framed 1.3 times as far and 8 degrees steeper for the
   same reason. The obelisk thins out when it stands between the camera and the player.
10. *The shade is drawn over the floor's mark during the Zenith* (lifted to 10 cm), with a pale outline, so the one
    safe strip is the darkest thing in the court; at other times attack marks are drawn over it.
11. *The entrance* (reworked by the fourth worker, 4.6 s) begins 0.9 s after the man falls, while his body is still
    burning away (`SunCourtDirector.RisingDelay`). To 1.3 s an orange ring closes on the floor where he lies and a
    thread of light thickens out of him, with embers; 1.3 to 2.3 s the sun comes down the thread and hangs 6 m over
    him, landing with a flash, a ring and fire; 2.3 to 3.2 s the scarab grows out of the floor beside it with its
    cases thrown wide, and the sun sinks into its forelegs as they close; at 3.5 s it rears, roars, and only then are
    its name and its bar shown (`SunBearer.InFight` is true from the roar). The player is held still
    (`PlayerController.Suspended`) and cannot be struck from the rising to the end. The king's body is not drawn
    into the sun, the sky's sun does not dim, and the spear does not burn away.
12. *The death* (reworked by the fourth worker, 6 s): to 1.6 s it drags itself at the sun, shuddering, while the sun
    gutters faster and faster between burning and dull; at 1.6 s the sun goes out with a burst and a ring, the gold
    goes dark and the body is still; the dark sun rolls into the obelisk's shadow and shrinks to the size of a coal;
    3.0 to 4.6 s the cases fall open and the body sinks flat into ash and is gone; the last 1.4 s nothing happens,
    and then "A god falls" and his dying words in the box. The man is **not** seen kneeling, the gold does not
    run, the wings do not burn back, and the sky's sun does not move. The coal appears 4.6 m out along the shadow.
13. *After Fire.* No Sun-Spear: the player keeps what is in the hand. No barque: the sponsor's four lines are read
    in the court, then a gold mark by the gold line opens with the prompt "Cast off" and leads to the hub, ending
    the run exactly as the summit's way down does (`SceneDoor.EndsRun`). Fire is unlocked in saved progress as
    Lightning is by its god.
14. *A fall in the court* loses carried embers as in the city (`LandReturn`), wakes the player at F6, puts the form
    away (`GodlyForm.Retire`), restores the man whole at (0, 0, 8), points the shadow north at 4 degrees a second and
    opens the wall; the king does not speak a second time.
15. *Hot floor is the form's own*, not a `Hazards` patch (those bite enemies only): up to six patches, drawn with
    the ground-mark shapes in orange and with flames.
16. *The hours are counted in gold pips* inside the hour ring, one for the first hour to twelve at the north, so the
    floor reads as a clock face with the shadow for its hand. This is Claude's addition.
17. *While the form enters and while it dies, the camera leaves the player and looks at it*, at 1.5 times the usual
    distance (`SunBearer.ShowDistance`; the fight is at 2), starting from the moment the man falls. The obelisk thins
    out for whatever is being shown as it does for the player: `OcclusionFader.WatchAlso` with two more points
    (the form and the sun). So both are seen from any side; the price is that the player can be at the edge of the
    picture or out of it for those seconds (held and safe in the entrance, free in the death).

**Measured** (`SunCourtProbe`, editor, 1920 by 1080, no vsync). Over the Zenith with a pass's lane and a stoop's
crater still hot: frame 3.0 ms mean, 9.5 ms worst, main thread 2.9 ms, 8,231 bytes of garbage a frame. The same
scene standing in the forecourt with nothing risen (`PerfProbe`): 2.1 ms and 7,914 bytes a frame, so the fight adds
about 0.9 ms and about 300 bytes a frame, some of which is the probe's own. The 7.9 KB is there before the fight
begins and was not looked into.

**Stand-in, and what looks weak.** The standing spear is still three primitives. The coal is now a molten heart in a crust of
five dark shards with three crumbs on its ash, built in `SunCourtBuilder.Coal`: better than a ball, still blocks. In the entrance the scarab is still one body scaled up from nothing, the king's body takes no part beyond
burning away under the light, and the camera's move to it is an ease, not a cut. In the death the body is squashed
flat as a whole, not broken into its pieces, and a falcon that dies in the air drops to the floor first. A perfect
guard knocking the sun away is now tried by the probe against the Roll only, not the Stoop or the Pass; a Roll's
lane ends at the obelisk, so it cannot be tried from straight across the court. The court's floor is plain discs
with thin joints and gold pips and still reads bare beside the summit's. The wall is square blocks, the colossus is the kit's statue
scaled 1.35 and is rarely in frame. The form's motion is pieces turned by code: the scarab's legs are rigid pieces swung at the hip in two threes, the
falcon's wings beat about two joints, and there is no lean or weight in the walk. The scarab on its back is the
whole body rolled over. No sound anywhere in the fight.

**Changed in the city the same morning** (rebuilt, `SunCityProbe` run again):

- *The Gate of Noon leads on* in the editor's play mode (item 13 of the city's list said `SunCourt.unity` did not
  exist). `SunCityProbe` ends in the court.
- *The champion has four tones* where he had one: `SunCityKit.CastTones` copies his mesh to
  `Generated/SunCity/elite_gilded_cast.asset` with the gold slot split by the bone each part hangs from: mask and neck
  a paler gold, kilt (hips and thighs), hands and feet a deep red gold, the spear's shaft dark bronze, its head and the
  rest the gold they were. His throw is still the stand-in of point 7: no standing spear, no recall.
- *The riverbed's pans, the ground patches, the dry stains, the sand drifts and the trodden middles of the streets* are uneven patches
  whose colour thins to nothing at the edge (`SunCityBuilder.Soft`, five shapes from `PanMesh`), drawn with the glow
  shader, not discs with an outline. They are not lit: in the hall's shade they are a little brighter than the floor.
  A drift is two such patches, one inside the other; it is no longer a mound.
- *The Gate of Noon's doors* are a bronze with its own warm light (`m_sun_bronze`), so they are bronze in shade and
  not near black.
- *Not done:* the statue heads of the golden way are still above the top of the picture.

### As built: second pass, enemies (2026-11-17, fourth worker)

What `fire_rework.md` sections 1 to 3 asked for, built as parts a scene builder calls. Nothing here lays out a level:
SunCity is untouched and still plays as the first pass did (its priests have the beam and its warden the pour, since
those are attacks of the kinds; nothing else is added to it until its builder calls the kit). Everything was tried in
the yard (`Clover > Build > Fire Yard`, now with an east yard through a gap in its east wall) by
`AgentScripts/FireMechanicsProbe.cs`.

**For the level worker: the one file to call is `Editor/Builders/FireMechanicsKit.cs`.** In order, in a builder:

1. Build the enemies with `EnemyBuilder.Build(kind, position, yaw, kit, parent)` as before, collecting them in a list.
   New kind name: `EnemyKind.AshenName` ("The Ashen").
2. `FireMechanicsKit.Equip(enemies, kit)` once, after all are built. It gives every enemy its kind's trick: the ashen
   its core, lamp, burst ring and `AshenBurst`; every wick a `WickFlame` and a flame lamp; every gilded `GoldRuns`;
   every priest a `SunMirror` (its beam quads, and the scene's gilded to call); every thirsting a `ThirstDrinker`
   (told the scene's enemies, whose bodies it drinks).
3. Per enemy, as the level wants:
   - `FireMechanicsKit.Bury(thirsting, wakeWithin = 3.2f)`: a thirsting under the mud as a breathing mound. Call after
     `Equip`. It replaces `MakeDormant` for that one.
   - `FireMechanicsKit.Wick(wick, burning: true)`: a wick that starts alight (it lights any wick it touches; put one
     beside dormant ones for a chain). `Equip` already gives every wick an unlit flame; calling this again only makes
     it start burning (the same component and lamp).
   - `wick.MakeDormant(distance)` as before; fire from any source (a burst, a beam, a burning wick) wakes them.
   - `FireMechanicsKit.WallOfShields(name, parent, levy1, levy2, levy3...)`: levies that lock into a line once two or
     more of them are in the fight. Place them side by side about 1.3 m apart on a long ramp or a street.
4. Props, anywhere: `FireMechanicsKit.Jar(parent, position, director)` (a water jar: any blow breaks it, the spill is
   drunk by any thirsting within 8 m, 16 m in a swell; whole again after a rest), `FireMechanicsKit.Mirror(parent,
   position, facingDegrees)` (a mirror stand; it must also go into `PlayerInteraction.Configure(...)`'s list), and
   `FireMechanicsKit.Target(parent, position, facingDegrees, opens)` (a sun-disc; held 1 s in a mirror's beam it is
   lit for good and `opens.SetActive(false)`).
   **Wiring a mirror to a door:** pass the door's blocking object as `opens`. For anything else, subscribe to
   `MirrorTarget.Lit` (an `event Action<MirrorTarget>`) from a runtime component, or pass a `reveals` object through
   `MirrorTarget.Configure(opens, reveals, glow)` to show a hidden way. Beams stop at the first collider that is not a
   body or trigger, at the stand's height plus 1.4 m; aim them at the target with yaw steps of 45 degrees from the
   starting facing (`MirrorStand.TurnStep`).
5. Once per scene: `FireMechanicsKit.Floor(world, player, enemies)` (the pool of 40 pieces the gold, footprints and
   pour are drawn from; without it none of the three shows or does anything), `var hud = FireMechanicsKit.Hud()`,
   `FireMechanicsKit.Heat(director.gameObject, player, schedule, calmRectangles, enemies, hud)` and, on the scene's
   `LandReturn`, `land.ConfigureHud(hud)`.

**Setting the heat schedule.** `new HeatSchedule(firstAt, every, warning = 5, swell = 15)` in seconds of play since the
scene loaded; `HeatSchedule.Never` for none. A district's own rhythm: one `NoonHeat` per scene, so for districts call
`NoonHeat.Instance.SetSchedule(...)` from a trigger as the player crosses into a district (no trigger component was
written for that; it is a few lines). **A no-swell zone** is a calm rectangle `new Vector4(leastX, mostX, leastZ, mostZ)`
in `calmRectangles`: a player inside is never burnt and the look does not whiten there. The elite's arena wants one.
The shade the swell reads is the same `SunlightZones` rectangles the priests use, so every shade island the level
draws must also be given to `SunlightZones.Configure`, as the yard's porch and new east awning are
(`FireYardBuilder.AwningShade`: a roof drawn as its shadow only, its rectangle moved north by
`LookBuilder.NoonShadowLength(height)`).

**What each thing does (runtime, `Assets/_Project/Scripts/Runtime`):**

- *The ashen* (`EnemyKind.TheAshen()`, `Gameplay/AshenBurst.cs`): the wick's model in its own palette
  (`SunCityKit.Ashen`: grey ash shell, near-black char, the wick's glowing slots white-hot), 32 health, walks at 4.4
  (the wick 2.2), never flinches (`PoiseHits` 999). One attack, `ashen_swell`: 1.35 s telegraph, a red circle of 3.2 m
  (the burst's reach), unblockable. Through the telegraph the core (a camera-facing glow and a point lamp) brightens
  and grows, grey flakes and white sparks come off it, the body shakes. On the blow it bursts: white flash, ring of
  ash, flames round its edge, embers; 30 to the player, 45 (and a burn) to every other enemy in the ring; a wick in it
  is lit and takes only a quarter (they burn and are not consumed); the ashen is gone. Killed first, it bursts where it fell 0.7 s later, its red ring drawn meanwhile. Another
  ashen caught in a burst dies and bursts in turn. Clips: `ashen_idle_loop`, `ashen_walk_loop` (the stumbling run, an
  uneven stride), `ashen_swell` (arms dragged back, chest out, up on the toes, then flung open), `ashen_death`,
  `ashen_hurt`, `ashen_stagger`, `ashen_dormant_loop`, `ashen_rise`.
- *The wick's flame* (`Gameplay/WickFlame.cs`): lit, it wakes, burns from head and hands with its own lamp, moves 1.4
  times as fast, leaves a burning footprint every 0.35 s it walks (they bite for 3) and lights any wick within 1.6 m.
- *The heat of noon* (`World/NoonHeat.cs`, `Gameplay/HeatSchedule.cs`, `UI/LandHud.cs`): the warning whitens the look
  (a second global volume, `Generated/SunCity/NoonSwellProfile.asset`, its weight rising), hardens the sun's shadows,
  fills the sun mark at the top of the screen and says "The sun swells. Find shade." once. In the swell a player
  standing where `Sunlight.IsLit` is true burns 3 every 0.8 s (pale wisps and flame off the shoulders), never in shade
  nor a calm rectangle. Wicks and the ashen move and act 1.45 times as fast, the thirsting 1.3 and smell water from
  twice as far, the gilded drip gold on their own about once a second. `NoonHeat.Swelling` is static for any other
  part to read.
- *Embers after a fall* (`Gameplay/EmberDebt.cs`, in `World/LandReturn.cs`, on by `LandReturn.EmbersReturnWithTheDead`):
  the embers carried at a fall go, split evenly, to the enemies killed since the last rest (the first takes any
  remainder); each such carrier gives off ember motes over its heart; its next death pays its share (with motes
  flying to the player and "N embers reclaimed"); a second fall loses whatever is still waiting and spreads the new
  loss over those killed since. With nobody killed since the rest the embers are simply gone. The HUD line "Embers to
  reclaim: N" shows while any wait. `AreaDirector.Rested` is a new event (raised after a rest), used for this and the jars.
- *The running gold* (`Gameplay/GoldRuns.cs`, `Gameplay/MoltenFloor.cs`): each blow that lands or is turned on a gilded
  splashes molten gold beside him (a heavy, more); hot for 2.6 s (burns 4 every 0.5 s), then cooling for 5 s (a bright
  puddle that slows the player to 0.55 and any non-gilded enemy likewise), then gone. 40 pieces at most; when full the
  oldest goes. `PlayerController.GroundFactor` is new and only `MoltenFloor` sets it.
- *The warden's pour* (`gilded_pour` on rank 3): 0.95 s, a red wedge of 110 degrees and 3.6 m, cooldown 9 s, for a
  player 2.2 to 5.5 m off. When it falls he lays an arc of molten gold at the wedge's edge: blobs 1.1 m high with
  colliders, for 4.5 s, then they run flat. Clip borrowed from the plant at the pour's timing.
- *The shield wall* (`Gameplay/ShieldWall.cs`): two or more levies in the fight stand in a line 1.25 m apart across the
  way to the player, facing them, and the line steps forward at 1.5 m/s until 2.4 m off; they still thrust from it. A
  light blow from the front is turned as a levy always turned it; a heavy blow on a shield from the front breaks the
  line: a clang and a spray of gold, every levy's guard down for 2.5 s (`Enemy.GuardOpenFor`), and the line re-forms
  after 4 s.
- *The buried thirsting* (`Gameplay/BuriedMound.cs`): while dormant the body is hidden and a low mud mound with two
  dark cracks stands there, rising and falling, a puff of dust from a crack every 1.7 s. Woken, a spray of dust, clods
  and a ring, and its first act is the lunge for the belt (the snatch with two flasks or more, else the pounce).
- *The thirsting drink* (`Gameplay/ThirstDrinker.cs`, `Gameplay/Waters.cs`, `World/WaterJar.cs`): a fallen body
  (not a thirsting's) is water for 14 s, a broken jar's spill for 30 s. A thirsting not attacking runs to the nearest
  within reach, drinks for 1.4 s (helpless, the drink clip), and rises with 60% of its health back, its most raised
  by a quarter and its body 8% larger for each drink (three at most).
- *The priest's mirror* (`Gameplay/SunMirror.cs`): `sunblind_beam`, a 12 m lane 1.1 m wide, 1 s telegraph with glints
  at his breast, unblockable, 14 damage; the streak is drawn white-gold for 0.35 s and the lane stays pale and counts as
  sunlit for 3.5 s (`Sunlight.Beam`, new). It is used at someone found in shade as well as in sun; the ray stays
  sun-only. *The calling*: the moment he finds the player in the sun, and every 7 s while he keeps them, a ring of
  light at his raised disc, the player's ground lit for 2.5 s, and every gilded within 14 m woken and coming.
- *The mirror stand* (`World/MirrorStand.cs`, `World/MirrorTarget.cs`): use turns it 45 degrees in 0.4 s; while it
  stands in the sun it throws a beam until the first wall, burning every enemy in it for 8 every 0.5 s, lighting
  wicks, and lighting its lane.
- `Enemy` gained, all inert unless something calls them: the events `AttackStruck`, `Struck`, `Woke`, `Restored`;
  `Kill()`, `WakeNow()`, `Heal(amount, swell)`, `Sip(seconds)`, `Steer(point, speed, faceTarget)` and `ClearSteer()`;
  the paces `SwellPace`, `FlamePace`, `GroundPace`; `GuardOpenFor`. `Health.Heal` is new. `EnemyAttack` gained the
  flags `Bursts`, `Pours`, `Beams`. `PlayerDefence.Scorch(amount)` takes health without a flinch (heat), dealing a
  blow only when it would kill.

- *The spearman's throw* (`gilded_cast` on rank 2 only, `Gameplay/SpearCast.cs`, given by `Equip` to every enemy named
  `GildedSpearmanName`): 0.9 s, a red lane 10 m long, unblockable, 20 damage, for a player 5 to 10 m off, cooldown 10 s.
  The spear (a shaft with a molten head, its own lamp) stands leaning in the floor where the lane ends, where it met
  the player, or short of a wall, smoking; until he has walked to it (1.3 times his pace) and pulled it out his guard
  is down. Clip borrowed from the lunge. The spear in his model's hand stays drawn while he is bare-handed.

**Not built** (in the brief, left for later): the spearman's leap from a ledge (the leap needs ledges, which the level plan will give; `EnemyAttack.Leap`
already exists for an attack that jumps to a marked spot, so the cheap form is a `gilded_leap` attack with `Leap =
true` that a builder enables only for a spearman placed on a ledge; not written). A spilled flask as drink for the
thirsting (only bodies and jars count). "Fire from a knocked-over brazier" waking wicks: there is no knockable
brazier; `WickFlame.LightWithin(point, radius)` is the call for one. The district trigger for heat schedules. No
sounds for any of it.

**What looks weak** (from the frames, see the report): the ashen's burst is mostly the shared white flash and flame
sprites, so its ash ring reads less than its fire; the gold puddles are flat yellow lenses without a molten sheen;
the ashen's core glow sits in front of the body as a disc rather than showing through cracks (there are no cracks
in the mesh); the shield wall has no locked-shield effect between the levies, only their spacing and their guard arcs;
the buried mound seen from above is a flat grey disc with two dark strokes, more a lid than a heap of mud, and its burst
of dust comes out nearly white; the spearman keeps drawing the spear in his hand while the thrown one stands in the floor.
Measured in the probe but not shown: the spear throw's damage did not register on the still player (health 100 -> 100,
though the frame shows the lane and the flash on them), and in the first ashen test a levy placed 2.7 m from the burst
was not struck (a levy was struck in the early-kill test, 80 -> 34). No sounds for any of it.

### As built: second pass, the city (2026-11-17, fifth worker)

Built by Claude in one unattended stretch of about two and a half hours, to `fire_levels.md`. Nothing here has been
played by the owner. Seen in `SunCityProbe` and its frames (`review/shots/current/suncity2_*.png`) and in eight raised
overviews (`review/shots/current/suncity2_overview_<district>.png`, made in edit mode by `AgentScripts/SunCityOverview.cs`).
The first pass's frames (`suncity_*.png`) are stale.

**Where things are.**

| What | Where |
|---|---|
| The builder | `Editor/Builders/SunCityBuilder.cs` (menu `Clover/Build/Sun City`), rewritten district by district: `DryRiver`, `FishersBank`, `CityOfWicks`, `PaintedTombs`, `GoldenWay`, `HallOfColumns`, `LastGate`. Its helpers, lore, the sponsor's words, the rest fires, the return rule and the gate of noon are kept. Heights are named constants (`Bed`, `Bank`, `Quay`, `Square`, `Terrace`, `Gallery`, `Avenue`, `Masons`, `Upper`, `BowlFloor`, `Rim`) |
| The ground | `Editor/Builders/SunCityGround.cs`: `Plateau` (any outline, edges broken by a jitter, faces leaning back, strata in two materials, a mesh collider of exactly the mesh), `Ramp` (a top, two sides and two ends down to the lower ground), `Ring` (ground round an inner outline: the champion's bowl), `Oval`, `Rect` |
| Shade | One list in the builder (`SunCityBuilder.Shade`): every `Canopy` (a frame on posts with slats; its mat is drawn only as its shadow), `ShadeRoof` (a roof drawn only as its shadow) and `Colonnade` adds its rectangle, moved north by the noon lean, and the hall adds the rectangles between its light bars. One `SunlightZones.Configure(true, shade, null)` for the scene. `Sunlight.MostRectangles` is now 160 (was 48): the city registers 66 |
| Ladders | `World/Ladder.cs`: used from below it climbs, from above it climbs down, in the player's leap (arched 1.4 m so the body clears the lip). With a `DownKey` it lies on the top until kicked down from there (S1), and stays down |
| Barred doors | `World/BarredDoor.cs`: `BarSide` is the direction from the door to the side that can lift the bar; from the other side, "Barred from the other side." The `OpenKey` keeps it open for good |
| The hearth wall | `World/BreakableWall.cs`: a heavy blow, or any blow of 40 or more (an ashen's burst gives 45 to what is not the player), brings it down; `suncity.hearth` keeps it down |
| Vistas, districts' heat, forced swells | `World/LandZones.cs`, rectangles checked by position each frame (no triggers). Vista: `TopDownCamera.SetFraming(1.5, (0,0,6), -10)` while stood in, not while any enemy in the scene is aware. District: `NoonHeat.SetSchedule` as the player crosses into it, first swell one rhythm after entering. Forced: the warning starts at once, once a sitting |
| Progress keys | `suncity.s1.ladder`, `suncity.s2.door`, `suncity.s3.gate`, `suncity.s4.door`, `suncity.hearth` (constants on the builder) |

**Built to `fire_levels.md`.** The land climbs as planned, south to north: the riverbed at 0 between 3 m banks of
dried mud in strata that lean back 12 degrees, their edges wandering half a metre, with slumps at their feet; the bed's
outline is the plan's table (the Narrows, the Pans swinging west, the Jam, the Barque's Bend swinging east, the basin);
the quay at 4 with four boat-sheds cut into its face and the stair up its middle; the Lamp Street at 4, its stair to the
Lamp Square at 6, leg two, the Oil Alley and the market's lower floor at 6, the market's L-shaped terrace, the gate and
the yard at 8 with the gallery at 10; the avenue and lower court at 10, the Levy Ramp to the Masons' Terrace at 13, the
Spearman's Stair between its plinths (17.5) to the upper terrace, hall and porch at 16 with raised aisles at 17.2; the
champion's oval bowl sunk to 14.5 with three steps down the porch's width and a stair up to the gate on the rim at 16.
The side paths: the Fishers' Bank (huts, racks, the gully stair down into the sunk well court at 1), the barque's deck
(gangplank up, prow open to drop into the basin), the roofs at 10 (ladder up from leg two, upper rooms standing on them,
the drop onto the market's east arm), the hearth room behind the cracked wall, the painted tombs sunk to 4 below the
yard's west wall, and the west passage and chapel behind the sun door. Five rest fires at the plan's places (F6 is the
court's). The four shortcuts open from the far side only and stay open (progress keys). Every room's enemies as
tabled: 47 with the champion (15 thirsting, five of them buried; 12 wicks, two burning, five dormant; 4 ashen; 6 levies,
three in a shield wall at the head of the Levy Ramp; 3 spearmen; the warden; 5 sun-blind priests), equipped by
`FireMechanicsKit.Equip`; nine water jars; the mirrors M1 and M2, the sun-disc over the sun door. The noon: D1 every
100 s with its first swell forced crossing z 31 in the Pans, D2 every 70 s, D3 every 50 s with its first forced at the
Levy Ramp's foot, none in the hall, porch and bowl (a calm rectangle as well). Vistas V1 to V5. All the first pass's lore
at its new place (C1 on the quay's mooring stone, C2 at the prow on the deck, C3 at the well's lid, C4 on the square,
C5 and C6 at the gate, C7 at the first statue, C8 on the scaffold, C9 over the hall's door, C10 at the gate of noon),
the six paintings and the scare, the waterskin, the grave dish, the spear, plus two new texts of Claude's (the hearth
room's three lines with the placeholder name "Ahu", and the chapel's two).

**Where it departs, and why.**

1. *F1 is at (-2.8, 0, 9)*, under the fishers' awning, not (2.5, 0, 9): the plan's own shade rectangle for F1 is at x -5
   to -1. *The ladder S1's foot is 1.7 m out from the bank's face*, so the leaning face does not hold the climber up.
2. *The barque is not heeled*: its deck is a level slab at 3.35 to 3.5 so it can be walked and fought on. The hull's
   overhang over the east channel is not registered as shade (the noon shadow falls north of the hull, not east of it);
   the deck cabin and the sail over the west channel are.
3. *The boat-sheds' shade is inside the sheds* (z 116 to 119), where they are cut into the face; the plan's z 113 to 116
   lies in front of them in the sun.
4. *The great jar of the Jam lies at (-8.6, 0, 73.4)*, off the gap's mouth, which the plan's (-4, 0, 73) would block.
5. *Oil jars are props* (eleven in the alley, one on the roofs) with colliders: there is no oil-jar runtime (B9's oil
   half), so an ashen's burst does not set them alight. Only the water jars work.
6. *The market's east arm is x -4 to 4*, not -10 to 4, so the broad stair (x -11 to -5) climbs into the terrace beside it
   rather than into the arm.
7. *Colonnade columns are the kit's at a third of their size* to stand under a 3 m roof (the plan's 0.6 scale makes them
   5.4 m). The colonnade roofs at 13 and 16 are drawn and fade when between the camera and the player, and are walked on
   from above (the side stairs land on them).
8. *No 0.5 m kerb step across the avenue at z 296*: half a metre is a wall to the character controller (step offset 0.3).
   The statues stand at x 11.5, not 13, so their plinths are in frame from the road.
9. *The spearmen do not leap.* `gilded_leap` (C3) is not written; the spearman on the scaffold deck and the one on the
   east plinth walk off their edges when they notice the player and fall (enemies have gravity), with no mark first.
   The thirsting on the barque's deck does the same. The warden has no `WaitRange` (there is no such field); he holds as
   any gilded does. The caller has no mirror stand on his dais: his mirror is his own attack (`SunMirror`).
10. *The bowl is flat at 14.5*, not dished to 14.2.
11. *The hall's breach is not built*: the fallen column lies across the east aisle (where the column at (15, 443) stood),
    but there is no lit patch where the roof broke. The sun-disc that opens the sun door stands at the beam's height
    (the stand's height plus 1.4 m) on the door's north jamb, (-21.85, 17.4, 449.9), and M1 stands at (-8, 16, 449.6)
    on its line, not (-8, 16, 448): a disc in the doorway's middle blocked the way through once the door was open. The
    gold disc higher over the door is only ornament. M1 starts facing north and wants six turns of the use button.
12. *Shade is flat rectangles*, as the plan allowed: the bridge-house's shade also covers the roof walk over it.
13. *Awnings are drawn as their frame and slats; the mat itself is drawn only as its shadow* (Claude's choice after the
    first frames: a drawn mat over the rest fire at the quay hid the fire and read as a table top). Sails, linen and nets
    without posts are drawn.
14. *Vistas give way whenever any enemy in the scene has noticed the player*, not only enemies near: V4 at the Levy
    Ramp's head is usually suppressed by the shield wall standing over it.
15. *The forced first swells are once a sitting*: after a fall the district's ordinary rhythm runs on.
16. *The sun-court's approach (the Way of Noon) is not built*; `SunCourt` is untouched.
17. *New runtime code*: `World/Ladder.cs`, `World/BarredDoor.cs`, `World/BreakableWall.cs`, `World/LandZones.cs`;
    `Sunlight.MostRectangles` 48 to 160. Tests: `Tests/EditMode/Gameplay/SunCityGroundTests.cs` (4).

**Measured** (`SunCityProbe`, editor, Game view 1920 by 936, no vsync; last run after the last change): 91 lines `OK`,
none `WRONG`, console 0 errors and 0 warnings. Every stair and ramp walked to its height (19, the bowl's north stair with them); the four shortcuts
refuse from the near side and open from the far; the mirror opens the sun door in six presses; the swell burns twice in
2 s in the open in the Pans and on the avenue and not at all under the boat on trestles, in a boat-shed, under the
court's colonnade or in the champion's bowl; falls wake at the quay's fire, the avenue's and the porch's, with 19 embers
waiting on the two basin thirsting killed since the rest and paid back when they are killed again. `PerfProbe` in the
Market of the Cold Oven with twelve enemies awake and coming and a swell on: frame 3.7 ms mean, 4.9 ms at the 95th
percentile, 14.0 ms worst, main thread 3.7 ms, 731 draw calls, 7,905 bytes of garbage a frame (the same 7.9 KB the court
measured standing still before; not from the city's new parts, which allocate nothing a frame; not looked into further).
The city registers 78 shade rectangles. EditMode tests: 342 pass (338 before, and the 4 new).

**What looks weak** (from the frames; the overviews show the whole):

- *Big level floors are still big level floors.* The quay (76 by 18 m), the avenue and lower court (40 by 74 m), the
  Masons' Terrace and the embalmers' yard have their height at their edges (faces, stairs, ramps, colonnades) but are flat
  within, broken only by paving, props and canopies. From the game's camera the middle of the avenue or the yard is a
  frame of flagstones with one or two things in it (`suncity2_avenue_1`, `suncity2_yard_1`, `suncity2_vista_v3`).
- *The roofs are a flat beige plain* with upper rooms standing on them (`suncity2_roofs_0`); they read as walls, not as
  houses.
- *The terraces are clean boxes*: their faces are straight ashlar with course bands and no cornice, and their edges do
  not crumble; only the river's banks wander. The plinths of the Spearman's Stair and the shrine are plain blocks.
- *The Levy Ramp hardly reads as a slope* from above (6 degrees); its rising sides show only near its head.
- *Kit pieces are reused where new ones are wanted*: the falcons on the plinths are the seated king at 0.45, the barque
  shrine's boat is the reed boat painted gold, the colossus is the seated king at 1.6.
- *Side spaces are bare*: the west passage is a corridor of walls with lamp niches; the chapel has an altar and two
  columns.
- *The mud tiles' seam* (a straight line down the bed's middle) shows in every river frame, as it did in the first pass.

### As built: the bosses' attack visuals, the sun parry and the Sun-Spear (2026-11-17, sixth worker)

Built from `fire_boss_visuals.md` (the plan) and `fire_rework.md` section 5, in the plan's order: its 90-minute list,
the ray as a beam, the parry, the Sun-Spear; then from the extra list. Frames are `review/shots/current/bossfx_*.png`
(`BossFxProbe`, see `AgentScripts/README.md`) unless named otherwise.

**Where it lives.** The Sun-Bearer's effects that are not particles are in `Gameplay/SunBearerLook.cs` (the sun's light,
the stoop's streak and shock ring, the parry's trail of fire, the stun stars), built by `SunBearerBuilder.Look`;
`SunBearer` only says what happens. The two spearmen's effects are in `Gameplay/SpearLook.cs`, which listens to
`Enemy.AttackStruck` and reads `Brain.Current` and `Brain.State`; `EnemyBuilder` adds it (with three streak quads, and
for Khepharu four shaft quads) to the Champion and to Khepharu wherever they are built, and sets their `SlashColour`.
`Enemy.cs` gained three read-only getters (`StrikePoint`, `LungeFrom`, `LungeForward`) and its fiery circle's cold ring
became a gold one; nothing else in it changed. The ray is `Gameplay/SpearRay.cs`, owned by `SunCourtDirector`
(`ConfigureRay`). Heat shimmer is `Core/HeatShimmer.cs`, a pool of 32 line renderers made by `VfxBuilder` in every
scene. Every piece is made by a builder; nothing is created during play.

1. **The 90-minute list.**
   - *Gold, not magenta and not blue*: the Champion's arcs are `(2.0, 1.4, 0.4)`, Khepharu's `(2.6, 2.1, 1.1)`; every
     ring the Sun-Bearer throws is `TintRing` in gold; its dust is thrown in sand; a fiery circle of the men gives a gold
     ring. A lane blow of either man draws a streak of gold light down its lane at 1.1 m, tip-first in 0.06 s and eaten
     from the tail over 0.2 s (0.3 m wide, 0.45 for the third thrust, the reach and the drive; Khepharu's with a
     white core), with gold motes at its tip, a glint at the tip of the god's heavy ones and sand at his foot after a long
     lunge (`bossfx_thrust_1` to `_3`, `bossfx_reach_1`). A sweep raises sand along its rim; a circle throws sand at its
     edge.
   - *Effects timed by time, not frame*: every `Time.frameCount % n` in `SunBearer` is now a clock (`Every`): per metre
     travelled for the roll's and the pass's bow wave, per second for the entrance and the death.
   - *The sun lights the court*: a point light over the sun (molten, range 9, no shadows), 3 held, 4.5 rolling or carried
     in a pass, 6 at the Zenith, 8 while a parried sun flies, 0 loose, times `SunBearerLook.Strength` 4 (the toon
     shader's banded lamps needed it before the pool on the floor was wider than the sun), breathing on 7 and 13 Hz waves.
     It shows as a warm disc round the rolling sun (`bossfx_roll_2`).
   - *The furrow follows the sun*: the hot lane is laid as the roll or the pass begins and grows behind the sun each
     frame; the red lane is drawn only ahead of the sun, so the orange furrow is seen chasing it (`bossfx_roll_2`,
     `bossfx_pass_1`). A bow wave of flame and sand at the sun's front foot per metre, embers off its top. The furrow
     smokes (grey flakes on every other beat of its flame, only smoke in its last 2 s and under the shadow) and shimmers.
     At the far rim a sand ring.
   - *The stoop tears the air*: two crossed quads from where it left the sky to the floor of the circle, gold with a
     white core, through the 0.24 s dive, drawn in toward the floor over 0.2 s after (`bossfx_stoop_2`); a shock ring at
     1 m that widens from 1 to 7 m in 0.22 s (`bossfx_stoop_3`); a sun-white impact 1.5 times its size, ten flames
     thrown outward, eight sand puffs; the shadow tightens from 5 to 3.6 m over the hold.
2. **The ray as a beam.** Through the 1.8 s call a thread (the old line, now the glow's streak shape) joins the standing
   spear to his hand, flickering at 15 Hz, widening at 70%, with motes drifting in and sparks off the blade
   (`bossfx_ray_2`). At the strike the dragon's beam in gold: a 1.0 m white core in the glow's beam shape and two gold
   strands kinking round it, redrawn every 0.04 s; the spear flies down it to his hand in 0.15 s as a white streak
   carrying a light; a flash, a shake, a clang and a gold ring at his hand; a line of heat on the floor that fades in
   1.5 s (`bossfx_ray_3`; the core 0.8 m and the strands 0.5 m, so the gold reads beside the white under bloom). The
   standing spear is now the Sun-Spear's own model at his size, smoking where it stands and dropping embers from its
   disc. **His throw flies**: the spear crosses its lane point first in 0.2 s trailing fire (sun white to ember), and
   where it drives in a star, a gold ring, sand and a shake (`bossfx_throw_1`, `_2`).
3. **The sun parry** (a proposal for the owner: `SunBearer.SunParry`, one serialized bool, true now; false gives back
   today's sideways knock exactly). A perfect guard against the sun (roll, furrows, stoop or pass, the existing
   perfect-guard path) freezes 0.18 s, flashes, throws a gold critical star and the perfect-guard burst, then slows time
   to 35% for 0.35 s while the sun, still burning white, flies back on a 2 m arc into the form's face with a 2.4 m trail
   of fire (`bossfx_parry_1`, `_2`); it strikes (a 0.08 s freeze, a sun-white impact three times its size, a burst, a
   gold ring, an ember flash, a shake, the form flashing white, smoke from its edges), does 30 damage (`ParryDamage`,
   the plan's suggestion) and the knock's 40 balance as today, then rebounds 6 m beyond the form, dull. The form lies
   exposed for today's `ExposedSeconds` (4 s) under five circling gold stars (`bossfx_parry_4`) and must fetch the sun
   from beyond itself, so the opening is as long as today's. The camera's lean-in of the plan is not done: the fight's
   framing is always in force and `TopDownCamera.CloseIn` ignores calls while it is.
   **Heat shimmer** is drawn squiggles (the owner's answer), not refraction: thin wavy lines in a warm white that rise
   0.8 m a second, sway and thin away in 0.9 s; off the furrows and craters, all over the lit court through the blaze
   (none in the shadow: `bossfx_zenith_1`, `_2`), and round the player in the sun during a swell.
4. **The Sun-Spear.** `Weapons.SunSpear()`: the Great Spear (every move, number and pose the same, drawn in the god's
   gold) with its skill replaced by the **Sun Throw**: thrown 12 m at 40 m/s, it stands in the floor where it lands (or
   in the first enemy) for 0.9 s, then comes home as a ray 2.2 m wide in 0.18 s, striking everything on the line once at
   the throw's full 26 (`WeaponShot.RaysBack`, `RayRadius`, `RaySeconds`; walked in strides no wider than the ray, so
   nothing is passed over). Nobody runs to it (the Flying Spear's run is the one move given up). Taking Fire equips it
   (`Weapons.OfGod`, used by `GodReward` for both gods). Its model is built from primitives after the king's spear
   (`WeaponBuilder` spec `sunspear`, no FBX yet): a gold shaft, the sun's disc and a white-hot leaf, in the court's own
   `m_fire_gold` and `m_fire_sun`. EditMode `SunSpearTests` (3): Fire gives it and Lightning the storm katana; it fights
   as the Great Spear move for move; its throw sticks and rays back with weight, its run is gone. `BossFxProbe`
   `sunspear`: thrown at open floor, Khepharu held on the line home: rays +1, struck +1, he lost 26
   (`bossfx_sunspear_2`, `_3`).
5. **The extra list.**
   - *The heat swell's look*: its volume is now white-gold, not grey (exposure +0.55, saturation -8 instead of -35,
     contrast +4 instead of -12, a warm filter, bloom 2.2 tinted gold), shimmer squiggles rise off the lit ground round the
     player, and the player's burn is embers and a thread of smoke with the odd tongue of flame, not pale puffs
     (`fire2_noon_swell_1`).
   - *The ashen's burst* has its own look: an ember-red star, an ember ring inside its grey ash ring, a fountain of
     embers and ash flakes thrown out and up, not the shared white burst and cold ring (`fire2_ashen_burst_2`).
   - *Molten gold*: hot puddles carry a sheen (their glow swells and ebbs, each in its own time) and glints run across
     them; cooling they go from bright gold to a dark bronze crust and their glow dies.
   - *The spearman's held spear* is shrunk away (its `weapon.R` bone, as the court does for Khepharu) while his thrown one
     stands. Not confirmed by eye: the bone is found and is part of his skin, but in the one frame taken of him
     bare-handed (`fire2_spear_bare`) he still seems to carry a long shape; it may be weighted to another bone.
   - *The spear throw doing no damage*: **the throw was never broken; the probe was.** First the probe read the
     player's health a frame after turning its healing back on. Then (found afterwards, with a log inside
     `Enemy.Deal`) it waited for `SpearCast.Throws > 0`, but in some runs the spearman had already woken and thrown
     during an earlier section, so the wait ended at once and health was read before the forced throw landed. The log
     showed every forced throw `Landed` for 23. The probe now waits for a throw after the one it counted before
     forcing; three runs in a row read 100 to 77 `OK`.
   - *Shafts of noon* (plan item 9, done after the list): from the moment one of Khepharu's noon circles is drawn a
     thin shaft of sun-white light stands over it from 16 m up, widening to 1.2 m as it fills (`bossfx_noon_tell`); at
     the strike it slams to the circle's full width for 0.1 s and collapses in 0.25 s (`bossfx_noon_1`), with a
     sun-white star and sand at the edge.
   - *The plant's ring of fire* (plan item 14): at the strike of either man's planted spear, tongues of flame (the glow's
     flame shape, billboarded) rise out of the floor round the circle's edge in 0.15 s, sixteen for the Champion and
     twenty for Khepharu, pulse on a 0.5 s beat for the burn's 6 or 8 s and sink one after another round the ring at
     its end (`bossfx_plant_after`). The random flame puffs inside the circle, which `Enemy` throws, are still there.
   - *The Zenith whitens the court* (plan item 11): a second volume (`ZenithBlazeProfile`: exposure +0.45, saturation
     -12, a warm filter, bloom 2.4 from 0.8) follows the floor's fill to full and holds through the blaze, then eases
     off over a second (also when the Zenith is cut short by a knock or a break); the floor's hot fill is drawn at half
     its strength through the blaze (`bossfx_zenith_fill`, `bossfx_zenith_1`, `_2`).
   - *Sparks that fly, feathers that fall* (plan item 13): each falling mark has its own thing in the air, from a pool
     of 18 quads: from the scarab an ember streak thrown up from under its cases on a 5 m arc onto the mark, from the
     falcon a gold feather swaying down; each lands exactly as its warning ends, since its place is worked out from the
     warning left (`bossfx_sparks_1`, `bossfx_feathers_2`).
   - *The Rising and the Setting* (part of plan item 17): at the throw a pillar of light from the floor up past the sun
     with the ring of air, the sun's trail of fire for its climb, embers off the opening wings (`bossfx_rising_1`, `_2`);
     at the setting a sand ring, a last shower of embers and smoke from the folding cases.

**Measured** (editor, Game view 1920 by 936, no vsync; every run after the last change): EditMode tests 345 pass, 0
fail (342 before, and the 3 new `SunSpearTests`). `SunCourtProbe` 34 `OK`, 0 `WRONG` (the perfect guard on the rolling
sun now parries: perfect +1, knocks +1, loose, exposed, no blow taken; taking Fire gives the Sun-Spear);
`SunCityProbe` 91 `OK`, 0 `WRONG`; `FireMechanicsProbe` complete, 0 errors (its spear-throw line `OK` in 3 of 6 runs then; after the probe's fix above, 3 of 3); the hub's `RunProbe`
complete; `BossFxProbe` complete (the Sun-Spear's ray `OK`); console 0 errors and 0 warnings after each. `PerfProbe` in
the sun-court with the form risen and rolling: frame 3.0 ms mean, 2.8 median, 3.9 at the 95th percentile, 12.4 worst,
main thread 3.0 ms, 654 draw calls, 7,919 bytes of garbage a frame (the 7.9 KB measured before; the new effects
allocate nothing a frame). `SunCourtProbe`'s Zenith with hot floor: 3.2 ms mean, 11.3 worst, 8,221 bytes a frame.

**What looks weak** (from the frames):

- *Hot floor is still drawn as a mark* (plan item 7 not done): the furrows and craters are the orange translucent
  shape with flame puffs, now with smoke and shimmer over them, not a char wash with ember cracks. With the furrow
  growing behind the sun it reads better than before, but it is still "another warning" more than "scorched ground".
- *The shimmer squiggles are faint*: thin ember-tinted lines, easy to miss at the court's camera; at the height of the
  blaze the whitened floor swallows most of them (`bossfx_zenith_1`), and a squiggle rising 2 m from just south of the
  shadow is drawn over the shadow's strip by the camera's angle.
- *The ray's strike* is still mostly a white bar with gold kinks; the spear flying down it is barely seen
  (`bossfx_ray_3`).
- *The thrown spear in flight* is hidden behind the thrust streak most of its 0.2 s; the trail shows more than the spear
  (`bossfx_throw_1`). It stands blade up, as the court's first stand-in did, not driven in at 14 degrees point first.
- *The ashen's burst is still mostly flame puffs* with ash squares; its ember ring blooms to near white.
- *The Zenith's court is pink-salmon while it fills* (red fill under a whitening volume) and orange-cream through the
  blaze, not the plan's white; the sun overhead is still off the top of the picture.
- *Feathers and sparks are small* at this camera (glow streaks 0.65 to 0.75 m wide); the sparks read best while still
  near the scarab, the feathers only in the lower half of their fall.
- *The stoop's streak* is a narrow column next to a falcon fourteen metres across; the shock ring reads better.

**Not done**: the plan's items 7 (scorched hot floor with char and ember cracks), the rest of 10 (the cracks under the
standing spear, its cooling blade, the champion's throw and recall), 15 (the pass's wing-tip wake), most of 17 (the ram's cracks and chips, the
sweep's dust beyond what is listed); the camera lean-in of the parry (the court's framing blocks `CloseIn`); the
Sun-Spear's own model (built of primitives; a Blender model is wanted); the champion's real throw and recall (the
city's departure 7) and his plant as a ring.

**Changed outside the boss code**: `Core/HeatShimmer.cs` (new) and `VfxBuilder` (its pool, in every scene's Effects);
`Player/WeaponShot.cs` (`RaysBack`, `RayRadius`, `RaySeconds`), `Player/WeaponShots.cs` (the ray home, `Rays`,
`RayStruck`, `ConfigureRay`; a returning weapon that is not a ray strikes as before), `Player/PlayerCombat.cs` (a shot
that rays back takes the weapon out of the hand), `Player/Weapons.cs` (`SunSpearName`, `SunSpear()`, `OfGod`),
`World/GodReward.cs` (the god's weapon through `OfGod`), `PlayerBuilder` (the ray line on the player),
`WeaponBuilder` (the `sunspear` spec and stand-in); `World/NoonHeat.cs` and `FireMechanicsKit.SwellProfile` (the
swell's look); `Gameplay/AshenBurst.cs`, `Gameplay/MoltenFloor.cs`, `Gameplay/SpearCast.cs` with `FireMechanicsKit`;
`Enemy.cs` (three getters, the gold ring); probes `BossFxProbe` (new), `PerfProbe` (SunCourt branch),
`FireMechanicsProbe` (the spear's health read), `SunCourtProbe` (a safe spot is never beside the burning sun), and the
scripts `AgentScripts/rebuild_fire.sh` and `check_fire.sh`. Rebuilt: Sun City, Fire Yard, Sun Court, Hub, Castle,
Summit and the combat test arena (whose builder adds it to the build list: taken back out, the list is Hub, Castle,
Summit).

### As built: the god's fall and the sunset, and Shards (2026-11-17, seventh worker)

Built from the owner's decisions of the afternoon (WORLD.md, "Every god transforms", "The upgrade currency is Shards"
and "Fire's land: changes"), only for Fire: Narukami, the castle, the summit and the demo's flow are untouched.

**Shards.** Every player-facing word for the currency now says Shard or Shards: the HUD count (`Shards  12`), the
chart's title and costs (`1 Shard`, `3 Shards`), the land's "Shards to reclaim", the fall callouts ("3 Shards lost",
"The fallen carry your Shards", "2 Shards reclaimed"), the sponsor's line after a fall ("The Shards of the dead are
only worth something at a fire."), the pause page's "Shards are spent at rest fires.", and the city's two gifts
("Take the Shards they missed", "Five Shards", "Three Shards"). Code names and serialized fields keep `Embers`
(`PlayerUpgrades.Earn`, `Enemy.CarryEmbers`, `EmberMotes`, `LandGift.Embers`, `GodReward.ConsumeEmbers`, ...), and
fire embers as an effect (`VfxKind.Embers`, Long Embers, Charged Embers, Ember Rain, Dragon of Embers) are not the
currency and stay. `docs/GUIDE.md` says Shards throughout (39 places), `DumpMarks` writes a `Shards` column. The
pause page's text is baked into every scene by `PlayerBuilder`; in Hub, Castle, Summit, CombatTest and FireYard it was
changed in place through the editor (open, set the Text, save) instead of rebuilding them. The HUD icon (a small
upright diamond) already reads as a shard and was kept.

**He transforms in front of the player.** Beaten, Khepharu is no longer burnt away: the court sets
`Enemy.KeepsBody` on him (new: a kept body is not burnt or cleared, its Shards' motes fly at once, and it holds
whatever clip `Enemy.HoldPose` gives it; `Enemy.Shine` lights it from within). Two clips of his own were added in
`SunKingAnimations` and baked (`sunking_transform`: down on one knee, up, the spear lifted straight at the sky, then
arched back onto his toes with the free arm thrown wide; `sunking_fallen`: on his back, one knee up, the spear fallen
along the floor by his open hand, the left hand on his chest, the head lifted, breathing). The order, all on game time
so the pause stops it: he gets up and lifts the spear and turns to whoever beat him (`RisingDelay` 1.1 s, the camera
easing in at 0.35 s instead of its usual 0.9 and the obelisk thinned for him as well as for the player); the
Sun-Bearer's entrance begins on him: the gold ring closes on the floor round his feet, the thread of light comes down
to the point of his spear (`SunBearer.ThreadFoot` 3.8 m) instead of through him, the sun comes down the thread onto the
spear (`SunHangs` 4.6 m; it came to 6 m over the place he had burnt away), and he burns gold and then white from
inside as it comes (`PouredGlow`, with embers off him); in the flash as it arrives (`SunLandsAt`, now 2.1 s) his body is
no longer drawn and the scarab rises under the sun and takes it up as before. About five seconds from the killing
blow to the roar. The camera holds both him and the player the whole time (`TopDownCamera.FrameBoth` and `Hold`, new:
the look point and distance that keep two ground points, and the height of what stands at the second, in the
picture; the Sun-Bearer's entrance and death use it too, which is the fix for the player going off screen there).

**He falls back as a man.** When the form dies its body comes apart into ash as before; the dull sun now rolls out along
the obelisk's shadow (following it as it turns) to six metres, stays the size of a man, shudders with light from inside
and cracks open (`CrackAt` 4.3 s: flash, gold ring, sparks): Khepharu is lying there, on the shadow's middle line,
across it (his length square to the shadow, but never more than 35 degrees from lying east and west, since the camera
looks north and a man lying north and south reads as sitting up), feet toward the player. The shadow stops, and the sun
leans from noon for the first time (78 to 42 degrees) so the obelisk's own shadow reaches over him. A breath after the
form's death he says his one line through the reading box: "I promised them gold. I gave them fire." Only once it is
read does **Take his fire** appear, on his chest (the coal is gone; `HisFire` in the builder is a warm light and the
`GodReward`). Taking it gives Fire, Falling Sun and the Sun-Spear as before, and his own spear is gone from beside him.

**The sunset.** On the Take, `Sunset` (new, plain numbers, tested) runs for eight seconds while the sponsor speaks: the
shadow swings round to point east (the light from the west), the key light sinks from where it stood to 13 degrees and
goes orange, the air, the fog and the sky go warm and violet, a sunset grade is weighed in (`SunsetProfile`), the drawn
shadow stretches to 2.4 times its length, and in the last 40 percent the gilded leaf of the new west gate (two pylons in
the tall west wall) sinks into the floor. The camera stands back over the court for it. The low sun's beam is drawn
through the gate across the floor to the obelisk's foot and on out of the gate to the sun itself on the horizon (a soft
disc beyond the wall). When the sun has set and the sponsor is read, "West, after the sun" and the gate's gold mark
offers **Go west with the sun**, which ends the land and leads to the hub as Cast Off did. The way out is the beam and
not the long shadow: lit from the west the obelisk's shadow points east, and going west after the sun means walking
into the light (WORLD.md allows either). The sponsor's last line, the only one that named the boat, is now "The sun
knows the way down the sky. Follow it west."

**The barque in the city** stays as scenery. The prow's carving no longer promises a ride ("It waits for one who
rises" is now "The boat of the morning. It stopped when the river did."), and no text in the city or the court names
it as the way out.

**Checks.** EditMode 353 of 353 (eight new in `FireEndTests`: the sunset's rule, the gate, the shadow pointed outright,
lying across the shadow, his line, the camera holding both, the sponsor saying Shards). `SunCourtProbe` has a new `End`
part (`suncourt_only.txt`) and its last steps are now the god's end: the whole probe ran 51 lines `OK`, none `WRONG`,
console 0 errors and 0 warnings (the transformation with both on screen at each beat, the man gone into the sun as it
arrives, the disc cracking and him laid in the shadow, the line read once, the Take by the interact key over his body
with Fire, Falling Sun and the Sun-Spear, the sponsor's four lines with no boat in them, the sunset done with the
shadow pointing east and the sun at 13 degrees, the gate open, and the hub reached through it by the interact key).
`SunCityProbe` 91 `OK`, 0 `WRONG` (the prow's new carving read, the two gifts giving Shards); `FireMechanicsProbe`
and the hub's `RunProbe` 0 `WRONG`, 0 errors; the fall callouts read "The fallen carry your Shards", "5 Shards
reclaimed", "6 Shards gone. The fallen carry 5". Reports in `review/checks/fireend/`. A finding on the way: when the
editor is not the active application, the Input System drops the keys a probe queues (background behaviour
`ResetAndDisableNonBackgroundDevices`, keyboards respecting the Game view's focus), and every probe that walks or uses
things fails wholesale (a first run of `SunCityProbe` read 44 `WRONG` for that alone). Setting
`InputSystem.settings.backgroundBehavior` to `IgnoreFocus` and `editorInputBehaviorInPlayMode` to
`AllDeviceInputAlwaysGoesToGameView` through `script eval` for the run, and back after, fixed it; the settings are not
an asset here, so nothing on disk changes.

Frames worth opening (`review/shots/current/`): `fireend_transform_3` (gold-white under the thread, spear raised),
`fireend_transform_4` (white under the descending sun, the strongest), `fireend_crack_2` (the dull sun shuddering in the
shadow), `fireend_fallen_1` (lying on the shadow by the obelisk), `fireend_line_1` (the line), `fireend_take_1` (the
prompt over him), `fireend_sunset_2` (half-way: orange court, long shadow east, gate pylons), `fireend_sunset_3` (set:
the wall's shadow over the west half, the beam through the gate to the obelisk, his body in the long shadow),
`fireend_exit_1` and `_2` (the road out to the sun).

**What looks weak.** The camera has to hold the player and him together, so when they are far apart (the probe's case: the player
fourteen metres south-west, across the obelisk) he is small in the picture and the descending sun is cut by its top
edge for a moment. In the transformation clip the head is thrown back far enough that the tall crown juts out behind
him seen from the side. The fallen pose reads as a man lying down from the court's camera only because he is turned
toward east and west; at 35 degrees off it, with the head lifted, he can still look half sat up from further off
(`fireend_fallen_1`); close to (`fireend_take_1`) he reads as lying, with the obelisk's real shadow over him.
The sun's beam is a flat glowing strip on the floor, not light in the air, and the sun on the horizon is a soft disc;
outside the walls there is only fogged mud, so at the gate (`fireend_exit_2`) the picture is mostly red haze with the
pylons beside it (`fireend_exit_2`: the player on the gate's mark in the beam, readable but plain). The sunset court itself (`fireend_sunset_2`, `_3`) is the strongest picture.

**Not done.** No sound anywhere in it. He does not speak with his body (no gesture as he says the line). The sun's
light on the floor at the gate is only the drawn beam: the real low key light lights the court from the west but the
gate's opening is not a lit shaft in the air. Nothing outside the court was built for the sun to set over (the mud
ground and the fog). The demo's Narukami still dies as before (by the owner's word: Fire only).
