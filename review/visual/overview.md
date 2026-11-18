# Visual overview: where the game looks unfinished, and the Fire land's world pass

Written 2026-11-18 from files only: the builders under `Assets/_Project/Scripts/Editor/Builders`, the shaders under
`Assets/_Project/Art/Shaders`, the design documents, and about 80 frames under `review/shots/current/` (about 42 of
them looked at directly for the Fire land, and about 38 of the hub, castle and summit through a frame review). The
editor was not used. `review/shots/golden/` is empty, so nothing here is a comparison against an accepted look.

The owner's words this answers: "Even though the enemy models are high quality, the world and area models are low
detail and make it seem unfinished, as well as all the effects being undone." Their examples were a phase 1 ring of
fire that is "just a ring of low quality fire" (where it should be a field of flames inside and an intense ring at the
edge) and burning lanes that are "just orange highlighted areas with flame".

Two other workers are busy tonight, and this report does not duplicate their work:

- **The boss-effects worker** is fixing the Fire god's fight effects and attacks. Those effects are judged in the
  effects section so the picture is complete, but the work plan leaves `SunBearer*`, `SpearLook`, `SunCourtDirector`
  and the god's attack code to that worker.
- **The Blender worker** is modelling the Fire land's building and prop kit. The work plan says which builder pieces
  each modelled piece replaces, and builds a place for each piece to slot in.

---

## Summary: the five things that most make the game look unfinished

Ranked by how much of the screen they cover and how often.

1. **The ground and walls are plain colour on Unity cubes.**
   - The Fire land uses untextured toon colours on purpose: the castle's stone textures "carry its blues and reds"
     (`SunCityKit.Plain`). Nothing replaced them.
   - From this camera about three quarters of every frame is floor or roof. Large parts of the Fire land frames are
     one flat tone: `suncity2_bank_1`, `suncity2_roofs_0`, `suncity2_yard_1`, `suncity2_bowl_0`,
     `suncourt_forecourt_1`.
   - Paving is one cube per flagstone, each 2.4 to 3.3 m wide. A flagstone is wider than the player is tall, so
     scenes read as a board of toy tiles.
   - Houses are single boxes (`Mass`). Ovens and natron heaps are spheres, the dead are capsules, and the kneeling gold
     statues are a cube with a sphere on it.
   - Next to enemies with modelled faces and wrappings, this is the gap the owner sees.
2. **Nothing in the world moves, and fire is almost absent from the city.**
   - `SunCityBuilder` and `SunCourtBuilder` create no particle systems of their own.
   - In 520 m of city there are 6 burning decorative braziers (two in the gallery, two on the avenue, two in the bowl),
     two wicks that start lit, and two lamp niches. Their flames cast no light.
   - The five rest braziers are dark iron until rested at (`suncity2_quay_0`, `suncity2_avenue_0`,
     `suncourt_forecourt_1`).
   - There is no dust, ash, smoke, cloth movement or heat haze outside the noon swell.
   - LOOKS.md rule 7 ("something always moves") and the Fire card's "fire actually present in the city" are both
     unmet.
3. **Effects are separate sprites, not layered substances.**
   - Fire effects are one layer of upright flame tongues: no scorched ground under them, no smoke column, no ember
     field, no light on the floor.
   - The plant's ring is 16 to 20 single tongues standing apart (`bossfx_plant_after`).
   - The hot lanes are orange see-through paint with clumps of flame on them (`bossfx_zenith_1`).
   - The ashen's burst and the gold splashes are bushes of identical tongues sprinkled with white squares
     (`fire2_ashen_burst_2`, `fire2_gold_many_2`).
   - Guard bands, glints and gold glints are still white light (`suncity2_champion_2`, `fire2_gold_many_2`).
4. **Shade has no form, and the values flatten.**
   - The noon key light leans north, so every wall face the camera sees is in shade. Each one is drawn as a single
     flat violet slab (`suncity2_alley_0`, `suncity2_lamp_1`, `suncity2_gate_0`).
   - Floors facing the sun are near white.
   - World outlines are a third of the character line or nothing: floors have outline 0.
   - The land's enemies are linen, dried skin and gold, so they sit at the same value as the floor and are held apart
     only by their ink (`suncity2_pans_0`, `suncity2_champion_2`).
5. **The edges of the playable space open onto nothing, and no landmark is ever in frame.**
   - Beyond the Sun City's walls is a 320 by 720 m sand-coloured cube (`SunCityBuilder.Beyond`).
   - Beyond the court is a 120 m mud slab and fog, so the sunset exit is "mostly red haze" (`fireend_exit_2`).
   - The hub floats in black (`tour_hub_4`). The summit's sky is flat cloud cut-outs (`tour_summit_4`).
   - The camera cannot see the horizon or anything tall more than about 7 m north of the player (see "What the camera
     can see"). The god's landmarks are therefore never on screen during play.

---

## What the camera can see (read this before planning any scenery)

The camera is 12 m from the player at a 55 degree pitch (`TopDownCamera.Distance`, `PitchDegrees`). GAME.md gives a
40 degree field of view. Worked out from those numbers, and matching the frames (about three 2.4 m flags show above the
player in `suncity2_yard_1`):

- **North (top of frame).** The top edge of the picture meets the ground about 7 m north of the player. A tall thing
  further north is out of frame however tall it is, because the top edge looks down at about 35 degrees. This is why
  the avenue's statue heads are "above the top of the picture". It is also why no obelisk, pylon or god on the far
  horizon can ever be seen.
- **South (bottom of frame).** The bottom edge meets the ground about 4 m south of the player.
- **Sides.** The view is about 18 m wide at the player.
- **Lower ground.** Ground lower than the player is seen further out. Ground 20 m below a ledge stays in frame to about
  40 m from the camera.

What this means for every land:

- **"Beyond the walls" is seen at the sides and below drops, never at the top.** It is the ground outside the wall
  tops, seen past low south walls and side walls. To make it rich, put lower ground there: a city below a terrace, a
  canyon, the sea.
- **A landmark has to arrive in the frame some other way:**
  - as a shadow or light falling into the frame;
  - as smoke or ash drifting overhead (anything between the camera and the floor, at 2 to 9 m up, is in frame);
  - as lines on the floor pointing to it;
  - in a vista framing (`LandZones.SetVistas`, `TopDownCamera.SetFraming`) that pulls the camera back.
- **The land climbs north toward the god.** That makes the next district higher ground, which is even harder to show.
  Vistas need more pull-back than the current `SetFraming(1.5, (0,0,6), -10)` to show anything of it.

---

## How the world is made, scene by scene

| Scene | Builder | What makes the world | Unique meshes | Floors | Walls and roofs | Beyond the edge |
|---|---|---|---|---|---|---|
| Hub | `HubBuilder` | Stacked `round_drum` discs, cube rings and drift stones, about 30 to 40 meshes merged with `Dressing.Mould` (clock numerals, gears, bell, lanterns, crane) | about 35 merged meshes plus primitives | `Flagstones` texture world-mapped, outline 0 | No walls or roofs; gate posts are `Block`s | Black water disc (r 140), fog, floating stones, near-black clear colour |
| Castle | `CastleBuilder` (+ `Dressing`, 176 calls) | Almost all `BuildKit.Block` cubes and Unity cylinders | Only Dressing's taper, star, ivy, edge, ridge, tuft and scatter meshes | `Flagstones`, `Ground` textures, puddle, moss and scorch decals | `WallBlocks` texture on boxes; roofs are stacked flat boxes with a `RoofTiles` texture | Crags 40 m down, a mist sheet, 3 flat `Ridge` silhouettes and 6 `FarTower`s in near-black |
| Summit | `SummitBuilder` | Drum discs, red `Block` gate posts, sphere clouds, about 10 inlay meshes and 14 rim materials from `Dressing.Mould` | about 25 merged meshes | `Flagstones` plus inlays (gold bolts, mitsudomoe, burn stars) | Keep roofs come up through the cloud; "roof courses are drawn as dark beam lines"; untextured roof material | A cloud-sea disc (r 160), 104 sphere clouds, 24 cloud banks |
| Fire Yard | `FireYardBuilder` | Kit `mud_tile`s, brick boxes, kit statues, the pylon and the obelisk | Kit pieces | `mud_tile` crazy paving | Plain brick boxes | Mud plain |
| Sun City | `SunCityBuilder`, `SunCityGround`, `SunCityKit` | Cubes (`Box` 112 calls, `Deco` primitives), `SunCityGround.Plateau`/`Ramp`/`Ring` procedural meshes for terraces and banks, 40 kit placements (19-piece `fire_kit.fbx`, 29k triangles, and 6 `tomb_panels`) | The 5 `PanMesh` stains, the procedural plateaus, the kit's 19 pieces | One cube per flag (`Paving`, 3 plain tones, some missing); `mud_tile` plates in the bed; `Soft` stains drawn unlit with the glow shader | `Box` walls in plain `m_sun_brick`; `Mass` houses with a parapet, a dust slab and 1 to 3 cube "roof walls"; `RoofTop` with seams and hatches | `Beyond()`: one 320 by 720 m sand cube at y -1.3, and the kit obelisk at 1.5 scale over the last gate |
| Sun Court | `SunCourtBuilder` | Drum discs for the floor rings, 3.9 m `Block`s set round the circle for the wall, kit obelisk, colossus (statue at 1.35), columns | Kit pieces | Discs of `stone`, `pale`, `dark` and floor gold, with 12 hour bars and pips | Brick blocks with a pale coping; low on the south | A 120 m `Mud` slab and Noon fog |

**Shaders.**

| Shader | What it does |
|---|---|
| `clover_toon` | Three bands (shade under 0.22, middle at 0.78 brightness, full above 0.62), a shade tint, rim, outline and a world-mapped texture option. No vertex colour. |
| `clover_glow` | 16 effect shapes: soft disc, streak, slash, flat, ring, star, flame tongue, bolt, brackets, cross, cracks, sigil, beam, landing disc, arrow lane and burning rim. Its `_Fire` mode is used by the Fire land. |
| `clover_molten` | The sun ball. |
| `clover_spell` | Spell solids. |
| `clover_world_text` | Text in the world. |

**Light and post** come from `LookBuilder.Apply`, one setup per mood:

- one shadowed key light, flat ambient light and exponential-squared fog;
- a solid clear colour (no skybox anywhere);
- one global volume: Bloom (threshold 1.05, intensity 1.1), Neutral tonemapping, contrast +20, the mood's exposure and
  saturation, a violet-shadow split tone, and a 0.3 vignette.

The Fire land adds two more volumes: the swell and the Zenith blaze.

---

## Per scene

### Hub (demo, untouched tonight)

**Reads well**

- The gold clock face and numeral ring inlaid in the round floor.
- A row of element braziers, each in its own colour.
- The vermilion and cyan gate arch makes a clear composition (`tour_hub_0`).

**Reads as unfinished**

- *Floors.* The castle's flagstone texture again, cracks drawn as dark scribbles. Nothing in the floor says "outside the
  hours" except the clock.
- *Walls and roofs.* None; one flat disc with no height, steps or routes.
- *Props.* Weapon stands and braziers are cylinders and boxes. The brazier flames are faceted crystal shapes (`tour_hub_0`).
- *Sky and beyond.* Near-black void with rain streaks frozen in it (`tour_hub_4`, `tour_hub_6`). The floating gear and
  lanterns are low-poly shapes nearly the colour of the void, so they read as stray polygons.
- *Edge.* A thin dark rim. Blue "water" shapes on the rim look like paper cut-outs (`tour_hub_4`).
- *Light.* One key light plus coloured point lights; the light shaft over the gate is a pale box with dashes
  (`hub_whole`, `hub_braziers`).
- *Fog.* Nothing at play range.

### Castle (demo, untouched tonight)

**Reads well**

- The most lived-in place in the game. The textured stone, puddles with streak highlights, moss, ivy, barrels, a well,
  weapon racks and a bonsai planter give it real surface interest (`tour_castle_0`, `tour_castle_12`,
  `tour_castle_15`).
- Hard cast shadows from the walls give shape (`tour_castle_3`, `tour_castle_18`).
- The palette holds: indigo stone, teal moss, vermilion accents, orange brazier pools.

**Reads as unfinished**

- *Floors.* One tile texture over 60 to 70% of the frame, the same small ring decals everywhere.
- *Walls.* Brick-textured boxes with plain tops.
- *Roofs.* The Japanese castle has no roofs, eaves or timber to see: towers are stacked flat boxes. From above it reads
  as a stone dungeon, not a castle.
- *Props.* Crates are boxes and barrels are cylinders.
- *Fire light.* Drawn as hard-edged translucent magenta discs on the floor (`tour_castle_15`), which read as decals. The
  brazier flame is the crystal flame.
- *Shadow edges.* Some show stair-stepping (`tour_castle_15`, the green moss strip along the wall top).
- *Beyond.* Never in frame in the tour, so it cannot be judged from the frames. The builder places ridges and far towers
  in near-black.
- *Frame dates.* Older frames (`castle_court`, `castle_shrine`, `castle_bolt`, 2026-11-15) are stale.

### Summit (demo, untouched tonight)

**Reads well**

- The best boss frames in the game. The vermilion torii posts with lantern discs, the shimenawa and paper streamers, gold
  lightning inlays in the floor, and the great blue dragon with cyan veins (`dragon_entrance_reared`,
  `dragon_death_rears`).
- The white jagged bolt with a cyan edge reads as lightning (`dragon_storm_2`, `storm2_middle_slam_strike`).

**Reads as unfinished**

- *Floor.* One disc of the same flagstones, with no cover or height.
- *Gate posts.* Close to the camera they are big plain red boxes with yellow cube lamps (`tour_summit_4`, `tour_summit_6`).
- *Blocks at the sides.* Flat blue slabs.
- *Beyond the rail.* A lavender swirl with flat cloud discs and cut-out rocks, with no depth layering and no cliff below
  the rim to give height.
- *Telegraphs.* Solid red fans and lanes that hide the floor (`dragon_breath_1`, `dragon_beam_2`).
- *Breath and beam.* Read as white light with crystal spikes, not electricity.

### Fire Yard (test yard; low priority)

The frames are older: `fire_yard_*` still show "Embers" in the HUD and a grey palette.

- The kit's `mud_tile` floor reads as crazy paving, not cracked riverbed (`fire_yard_2`).
- The second-pass probes (`fire2_*`) are where most land effects were judged; see the effects table.
- Nothing here needs a world pass. It is a bench.

### Sun City, district 1: the Dry River and the Fishers' Bank

**Reads well**

- The banks' leaning, banded faces with wandering edges and slumps are the best-shaped ground in the game
  (`suncity2_narrows_0`, `suncity2_bend_0`).
- The stranded barque is a strong landmark from above (`suncity2_overview_river`).

**Reads as unfinished**

- *Floors.*
  - The kit's `mud_tile` plates read as **flagstones or crazy paving**, not a cracked riverbed. The plates are flat
    with bevels and no curl, no dark crack depth, no silt and no debris (`suncity2_pans_0`, `suncity2_narrows_0`).
  - A straight seam runs down the bed's middle in every river frame (`suncity2_narrows_0`, `suncity2_swell_0`). The
    grid's tile edges fall at x = 0.
  - The bank tops are one plain tone. Their silt and sand stains are unlit cream blots that read as **fog or clouds
    lying on the ground**, not dust (`suncity2_bank_1`: the whole frame is one tan plane with two milky smudges).
- *Walls.* The outer walls are plain brick boxes (left edge of `suncity2_overview_river`).
- *Props.*
  - Reed bundles are cylinders and drying racks are boxes on posts.
  - The fishers' huts are the kit's `house_block`, the only detailed buildings here.
  - No nets on the ground, no fish bones, no boats' ribs, no dead reeds growing on the banks.
- *Foliage.* None in a land built on a dead river: no dead palms, no reed beds, no dry papyrus stalks.
- *Beyond.* The sand cube shows past the west wall (`suncity2_overview_river`); from the game's camera the banks fill
  the sides.
- *Movement.* None.
- *Fire.* The first rest fire at the awning is dark until rested.

### Sun City, district 2: the City of Wicks (Lamp Street and Square, burnt houses, Oil Alley, roofs, market, gate, yard, tombs)

**Reads well**

- The painted tomb panels are the richest surfaces in the land (`suncity2_tombs_1`).
- The burnt houses' black floor with charred beams reads as a story from above (`suncity2_overview_city`).
- A burning sun-blind reads as fire: layered orange tongues with a yellow heart (`suncity2_hall_1`, a district 3 frame
  showing the same effect).

**Reads as unfinished**

- *Roofs.* **The worst surface in the game.**
  - `Mass` and `RoofTop` give flat beige plains with a parapet and one or two cube "roof walls" or hatches.
  - `suncity2_roofs_0` is 90% one tone, and `suncity2_overview_city` shows the quarter as a few huge blank rectangles.
  - The roofs read as walls, not houses: no house-by-house steps, no clutter, no stair-heads, no soot.
- *Walls.*
  - Alley and house faces in shade are single violet slabs with no courses, doors, soot or bounce light
    (`suncity2_alley_0`, `suncity2_lamp_1`).
  - Lamp niches are a dark cube with a sphere lamp.
- *Floors.*
  - Plain earth between large flags.
  - In the yard, flags 2.4 m across in three tones with plain earth between them, and drains as black strips
    (`suncity2_yard_1`).
  - In the market, more cream blots that read as smoke (`suncity2_market_1`).
- *Props.*
  - The cold oven is one sphere (`suncity2_market_1`).
  - Oil jars are cylinders with a black disc lip; natron heaps are spheres.
  - The wrapped dead are capsules and the linen strips are flat cubes that never move.
  - The kit's `house_doorway`, `burial_niche` and `sarcophagus` are the only modelled pieces.
- *Fire.* For "the city of wicks" there is almost none:
  - two lamp niches;
  - the two burning wicks and the wicks' own head flames (`suncity2_lamp_1`);
  - two braziers in the gallery.
  - No hearths glow in doorways, no soot is over the doors, no smoke rises from roofs, and no embers drift.
- *Edges.*
  - The tombs' south edge is a jagged sand cut seen from above (`suncity2_tombs_1`).
  - The hearth room's rubble is boxes (`suncity2_hearth_1`).

### Sun City, district 3: the Golden Way, the Masons' Terrace, the Hall of Columns, the bowl and the gate

**Reads well**

- The avenue's gold lines and the seated statues give a strong axis in the overview (`suncity2_overview_golden`).
- The kit's lotus columns in the hall are the best architecture in the city (`suncity2_hall_1`).
- The gate of noon's stair and bronze doors read as a destination (`suncity2_gate_0`).

**Reads as unfinished**

- *Floors.*
  - Big pale flags in near-white, and the Levy Ramp is one plain pale band 7 m wide (`suncity2_vista_v3`,
    `suncity2_avenue_0`).
  - The bowl is a chessboard of 2.6 m flags with nothing on it (`suncity2_bowl_0`).
  - The gold lines are drawn in the lit `m_kit_gold` and glow like strips of light (`suncity2_avenue_0`). The court
    uses the dull `SunCityKit.FloorGold`, which is what the owner asked for.
- *The hall's floor* shows a fine stipple over the whole frame (`suncity2_hall_1`). The cause was not confirmed from the
  files; it may be the thinning dither or the shadow filtering. It reads as noise, not stone.
- *Statues.*
  - The heads are above the top of the frame on the road (known).
  - The "falcons" on the great plinths are the seated king at 0.45.
  - The kneeling predecessors in the bowl are a gold cube, a cube and a sphere.
- *Walls.* The avenue walls are 16 m plain stone boxes, and the pylon wings are boxes beside the kit's pylon.
- *Props.* Squared blocks and offcuts are cubes, and the column drums are cylinders.
- *Fire.*
  - The avenue's two braziers flank a rest fire that is dark until rested at (`suncity2_avenue_0`).
  - The bowl has two braziers, the hall has none, and the hall has no motes in its light bars.
- *Beyond.* Not seen from the game's camera in these frames; only the sides of the terraces show.

### Sun Court

**Reads well**

- The clock-face floor with the turning shadow is a clear arena idea (`suncourt_court_1`, `bossfx_roll_cooling`).
- The sunset court is the strongest picture in the land: long violet shadows, orange stone and the beam through the
  gate (`fireend_sunset_3`).

**Reads as unfinished**

- *Floor.* Plain wedges between thin lines. The floor carries no history: no scorch rings, no worn path round the
  hours, no cracks from the obelisk, no offerings.
- *Wall.* 3.9 m blocks set on a polygon, so its facets show (right side of `bossfx_roll_cooling`).
- *Colossus.* Rarely in frame.
- *Obelisk.* Covers a third of the frame as a dark diagonal band in some fight frames (`bossfx_plant_after`).
- *Forecourt.* One flat tone, two plain walls and a dark rest brazier (`suncourt_forecourt_1`).
- *Beyond.* Mud and fog. At the exit the left half of the frame is red haze with the pylon blocks floating in it
  (`fireend_exit_2`).

---

## Effects, judged against "the element's own substance"

### What fire needs to read as fire from this camera

Use this as a checklist for any Fire land effect. Every fire effect should have most of these six layers. Most effects
now have one or two.

| Layer | What it is | Scale at this camera | Motion and life |
|---|---|---|---|
| **Ground** | A char or scorch wash on the floor with an irregular, hard edge, and ember cracks glowing in it | The full area that burns | Appears in 0.1 s and outlives the fire by 3 to 10 s, cooling from orange cracks to black to grey ash |
| **Body** | Many small flame tongues packed edge to edge (a carpet), not a few large ones standing apart; sizes vary 1:3; dark-red fringe and yellow heart (the glow shader's `_Fire` bands already do this) | Tongues 0.3 to 0.9 m tall inside a field, 1.2 to 2.5 m at an intense edge | Each tongue lives 0.3 to 0.8 s and is replaced; the field flickers, never pulses in step |
| **Edge** | Where a fire has a boundary, a wall of taller, hotter, denser tongues leaning outward | 2 to 3 times the body's height | The densest emission |
| **Smoke** | Dark grey-brown puffs that rise, lean with a wind and spread (`BuildKit.Smoke`) | 1 to 3 m puffs rising 3 to 6 m, so they cross the frame between the camera and the floor | 2 to 4 s; thicker as the fire dies |
| **Embers** | Sparks rising and drifting, a few falling back | Small (0.05 to 0.1 m) but many (20 to 60 a second for a big fire), with bloom | 1 to 2.5 s, curling |
| **Light** | A point light on the floor and nearby walls, orange `(1, 0.46, 0.1)`, flickering | Range about 1.5 times the fire's radius | Flicker at 6 to 13 Hz, uneven |

The owner's two examples, written against this list, are for the boss-effects worker to check their work against:

- **The plant ring** should be ground char over the whole circle, a low carpet of small tongues inside it, and a tall
  dense wall of flame at the rim. Smoke should roll off the rim and embers should rise from the whole field.
- **A burning lane** should be char along the lane with orange cracks, a carpet of tongues, a taller edge on both sides,
  and a smoke trail leaning away. The orange see-through fill should go.

### The effects as they are

"Fire set" means the Fire land's own materials from `VfxBuilder.Build(fireLand: true)`. Other lands keep the common set.

#### Common one-shot effects (`VfxKind`, made in `VfxBuilder`)

| Effect | How it looks now | What is wrong | What it should be |
|---|---|---|---|
| Hit spark, heavy spark | Common: additive warm-white streaks 0.05 to 0.15 m. Fire set: fire-shaded streaks 0.06 to 0.17 m | Too small to read at 12 m. In the Fire land a hit on flesh or wrappings throws streaks of flame, which is odd for a fist on a body | Fire set: a spray of embers and a puff of ash or dust from what was struck (sand for the thirsting, ash for wicks, molten gold for the gilded), 0.15 to 0.3 m, with one larger impact flake |
| Guard spark, perfect guard | Common: cold white streaks. Fire set: fire streaks; the perfect guard has a white core | Readable; the perfect guard keeps a white heart, which the rules allow | Keep. Add a ring of sand or ash thrown along the floor for the perfect guard |
| Burst (death, finisher) | Common: 36 white-gold streaks. Fire set: fire streaks | A spray of lines with no mass | Fire set: a fire burst plus a smoke pall plus ash flakes falling, and a scorch left where the body was |
| Impact, critical, glint, clang (star shape 5) | An eight-pointed star; in the Fire set, ragged flame-coloured | Reads as a comic sign. That suits the style for impact; as fire it is "a star made of fire" | Keep the impact star as the style's graphic mark. Make the glint a small flare of flame, not a star |
| Danger flare | A red eight-petal flame star, drawn through walls (`suncity2_alley_0`, `suncity2_market_1`) | Large, bloomed, and its shape matches no fire. It is the land's one red warning, so its shape was kept on purpose | Leave the shape until the owner says otherwise. Shrink it about 25% so it does not cover the enemy's bar |
| Ring, tint ring | Fire set: a ring of fire bands (`m_vfx_fire_ring`) | A flat circle with no body | A ring of low tongues thrown outward with sand or ash under it |
| Fire (puffs) | Fire set: flame tongues (shape 6) | Good as a body layer alone. Clustered, they read as a bush (`fire2_gold_many_2`) | Use as the "body" layer only, alongside ground, smoke and embers |
| **Embers** | **Not replaced in the Fire set**: additive glowing dots `(6, 2, 0.3)` | Light, not embers. They bloom into yellow blobs | A fire-set ember: small, elongated by speed, cooling from yellow to red to dark |
| Smoke | New, soft grey-brown, only the Fire land throws it | Good material; thrown sparingly | Throw it from every fire effect, larger and longer-lived |
| Dust | Fire set: sand puffs | Fine | Add a low ring shape for landings |
| **Ash** | `m_vfx_ash`, glow shape 3 (a flat square) in pale grey `(2.6, 2.6, 2.6)` | **Draws as white squares** (`fire2_ashen_burst_2`, `fire2_gold_many_2`): they read as paper confetti or pixels | Dark grey flakes with ragged edges (glow shape 0 with `_Noise` breakup, or a new flake shape), mid-grey not white, tumbling and falling slowly |
| Wound, heal, shock, wisp, bolt, tint puff, tint spark | Common light materials; not in the Fire set | Fine for other lands. The heal's green and the shock's blue are not used in the Fire land | Nothing tonight |

#### Marks, bands and slashes

| Effect | Where it is made | How it looks now | What is wrong | What it should be |
|---|---|---|---|---|
| Slash arcs (player and enemies) | `VfxBuilder.SlashMaterial`; enemies get an "ember" tint (`EnemyBuilder`) | Additive light arcs; in the Fire land, orange crescents of light | The docs call this not done: "a sweep is an orange crescent of light rather than a sheet of flame" | Fire land enemies: the slash shape through `BuildKit.Fire` with smoke at the tail. The player's slashes stay steel (the player carries no fire at the start) |
| Guard arc (enemy guard band) | `GuardArcMaterial`, white additive | A white glowing arc on the floor (`suncity2_champion_2`, `suncity2_well_0`) | White light, the biggest non-fire bright shape in a fight | In the Fire land, a band of bronze or shield colour with a hard edge, drawn over the floor rather than added to it |
| Telegraphs | `GroundMark.Show`; `Enemy.RedMarker` (1, 0.08, 0.05) and `PlainMarker` (1, 0.93, 0.75) | An outline plus a filling copy; red for unguardable, cream for guardable (`suncity2_bend_0`, `suncity2_market_1`, `suncourt_furrows_2`) | Readable but paint. LOOKS rule 10 asks for marks made of the element where that stays readable | A guardable mark drawn as a line of scattered sand or soot with the same outline; a red mark kept red but drawn with flame licking along its outline as it fills. Keep red for unguardable |
| Burnt ground | `GroundMark.ShowBurnt` | A char wash inside an orange outline (boss hot floor) | Still reads as an orange outline round a dark wash (`bossfx_zenith_1`) | Ember cracks in the char (the plan's 2.5, not done), an irregular edge rather than the mark's clean shape, flames along the edge |

#### Fire in the world

| Effect | Where it is made | How it looks now | What is wrong | What it should be |
|---|---|---|---|---|
| Brazier fire | `SunCityKit.BrazierFire`: 16 tongues a second, 0.55 to 0.95 m, 0.45 to 0.85 s; smoke 3 a second | A small cluster of flame in the bowl with a thread of smoke (`suncity2_fire_1`) | Reads as fire but small. It casts no light unless it is a rest fire. There are 6 non-rest braziers in the whole city | Twice the rate, a light on every brazier, embers rising, a glow disc on the floor drawn as light (not paint), and many more braziers |
| Rest fire | `SunCityBuilder.Rest`, `RestPoint` | Unlit: a dark iron brazier on a gold ring and a mat (`suncity2_quay_0`, `suncity2_avenue_0`, `suncourt_forecourt_1`) | Reads as a dead prop. The player cannot tell it is a rest fire until close | Unlit: banked coals glowing in the bowl, a thin smoke thread, the odd ember. Lit: full fire plus light |
| Castle brazier flame (demo) | `CastleBuilder.Flames` and the kit flame | Faceted crystal flame and a magenta disc of "light" on the floor (`tour_castle_15`) | Crystal, not flame; the light is a decal | Later: the `BuildKit.Fire` flame and a real point light |
| Hazards (player's ground fire, the trail, the crater) | `World/Hazards`, puffs of `VfxKind.Fire` at intervals | Not seen in the Fire land frames | Cannot be judged from these frames | Same six-layer recipe at small scale |
| Wick flame | `WickFlame` | A small bright flame on the head with a glowing disc, and burning footprints (`suncity2_lamp_1`, `suncity2_alley_0`) | The head flame is close to a torch's white-hot dot. The footprints were not seen in the frames | More smoke (the doc's "no smoke thread yet"), embers dropping, the char showing on the wrappings |
| Ashen swell and burst | `AshenBurst`, `SunCityKit.Ashen` | A white-hot core disc in front of the body, then a wall of large tongues with white squares and a white crescent (`fire2_ashen_burst_2`) | The core is a disc, not cracks. The squares are the Ash bug above. The white crescent is light. No scorch is left | Glow through cracks in the mesh (needs the model), a dark ash ring on the floor that stays, grey ash flakes, a smoke pall |
| Molten gold | `MoltenFloor`, `GoldRuns` | Flat brown discs as it cools (`fire2_buried_0`). Hot, a field of flame tongues with white star glints and yellow glow dots (`fire2_gold_many_2`) | Cooling gold reads as mud puddles. The glints are white stars | Hot gold as a glossy, banded liquid (a molten-shader disc with an irregular edge); cooling to a dark bronze crust with a bright rim; small fire only at the hottest |
| Priest's beam | `SunMirror` | A soft orange glow lane with a white zigzag core and flame chevrons (`fire2_beam_1`) | Mostly glow: still reads as light | A jet of fire like the god's ray: a churning core, smoke off its sides, scorch along the floor |
| Noon swell | `NoonHeat`, `NoonSwellProfile` | The whole screen goes warm orange (`suncity2_swell_0`); the player smokes | It grades the image but adds nothing in the world | Heat shimmer that reads (thicker squiggles over lit ground), dust devils, cracks in the mud glowing faintly |
| Heat shimmer | `Core/HeatShimmer`, 32 line renderers | Faint warm squiggles, easy to miss (`bossfx_zenith_1`) | Too thin and too few | Doubled width and count, kept as drawn squiggles (decided) |

#### The god's fight (the boss-effects worker's job tonight; listed for completeness)

| Effect | How it looks now | What is wrong | What it should be |
|---|---|---|---|
| Sun ball | The molten shader ball, spiky rim quad, smoke (`bossfx_roll_2`) | Reads as a burning ball; the rim spikes are hard | (Owned) Fewer, bolder tongues |
| Plant ring of fire | 16 to 20 single tongues standing apart round the circle, a few puffs inside (`bossfx_plant_after`) | **The owner's first example** | (Owned) Field plus intense rim, per the recipe |
| Hot floor and furrows | Orange see-through bands with clumps of flame, smoke puffs, faint shimmer (`bossfx_zenith_1`, `bossfx_roll_2`) | **The owner's second example** | (Owned) Char, cracks, carpet, edge, smoke |
| Noon shafts | A tall rectangle of yellow-white flame light with a hard quad edge (`bossfx_noon_1`) | The rectangle shows; it reads as a lit slab | (Owned) A rounded column of churning fire with smoke at its foot |
| Thrust trail | A flame streak with a white heart and a flame star at the tip (`bossfx_thrust_3`) | Acceptable | (Owned) |
| Ray and call | Thin in the call (`bossfx_ray_2`) | Thin | (Owned) |
| Sunset beam | A flat glowing strip on the floor (`fireend_sunset_3`, `fireend_exit_2`) | Left as light on purpose | Light is right for a sunset. The fix is what it shines across (see the work plan, step 8) |

#### Demo effects (later)

| Effect | How it looks now | What is wrong | What it should be |
|---|---|---|---|
| Lightning bolts | A white jagged line with a cyan edge (`dragon_storm_2`) | Reads as lightning but thin | Later: forked branches, an after-image, a scorch star |
| Dragon breath and beam | White-cyan bloom with crystal spikes | Reads as light | Later: crawling arcs and ionised streaks |
| Rain | White streaks | Fine | Keep |

### Telegraphs, guard bands, rest fires: what to change first in the land

1. **Fix the white squares and white light first.**
   - The Ash material draws white squares.
   - Guard arcs, glints and gold glints are white light.
   - These are cheap material fixes in `VfxBuilder.Build(fireLand)` and `EnemyBuilder`.
2. **Rest fires that read as rest fires when unlit.** Banked coals and a smoke thread.
3. **Fire-set embers.** The one common kind the Fire set does not replace.

---

## Characters against the world

| What | Characters | World | Effect on the frame |
|---|---|---|---|
| Ink | 0.034 m in near-black `CharacterInk` | 0.7 times the given outline: 0.0084 m for most kit and paving, **0** for floors made with `Flat()` and for the hub's and summit's floors; navy `WorldInk` even in the warm Fire land | Characters look drawn and the world looks rendered. Ravenswatch's world has painted edges and a drawn line on its props; here the ground has none |
| Shading | Two to three bands plus rim on curved bodies | At noon every floor is in the full band and every wall face the camera sees is in shade. **Every surface has one band**, with no mid tone and no edge light | The world looks unshaded beside shaded characters |
| Colour value (Fire land) | Wrappings `(0.7, 0.67, 0.6)`, the thirsting's skin `(0.53, 0.44, 0.37)`, gilded gold | Paving `(0.61 to 0.69)`, new paving `(0.75 to 0.8)`, brick `(0.58, 0.52, 0.44)` | Enemies and floor share the same values. Only the ink separates them (`suncity2_pans_0`: the thirsting nearly vanishes into the mud; `suncity2_champion_2`: gold on cream) |
| Colour value (castle) | Saturated vermilion, violet and cyan enemies | Indigo stone | Better separation; the castle reads more finished for this reason alone |
| Contact | Real noon shadows (short) and a soft blob under the player | Props cast shadows; the `Soft` stains are unlit and cast none | Fine |
| Scale | The player is about 1.8 m tall | Flags 2.4 to 3.3 m, doors 3.6 m, walls 10 to 16 m of plain face | The flagstones are the biggest scale breaker: real ones are 0.6 to 1.2 m |
| Detail density | Modelled faces, wrappings, rank gold, thousands of triangles | Cubes; texture-free | This mismatch is what the owner named |

---

## Lighting and post-processing

- **Bands.**
  - The toon shader has three bands, but the Noon key at 78 degrees makes the land two-toned: floors full, south faces
    shade.
  - Banded point lights exist (`_LightBands` 2), but almost no fire in the city carries a light, so the land never shows
    them.
- **Shade colour.** Fire: `(0.56, 0.53, 0.66)` grey-violet, which looks right on stone. Everywhere else:
  `(0.3, 0.28, 0.58)`.
- **Shade form.** Nothing breaks up a shaded face: no bounce from the bright floor at its foot, no lit top edge, no
  courses.
  - LOOKS rule 3 asks for exactly these three.
  - The sun city's key shadow strength is lowered to 0.72 to keep the hall readable, which makes cast shadows weaker
    everywhere else as well.
- **Rim.**
  - Characters carry a rim.
  - World materials in the Fire land get a faint warm rim `(0.75, 0.7, 0.6)`, which cannot show on flat-on surfaces.
  - Gold carries a broad rim plus emission, so the city's gold lines glow.
- **Bloom.** Threshold 1.05, intensity 1.1. Anything emissive (the gold lines, the ember slots, white glints) blooms into
  light. This is part of why effects "read as light".
- **Fog and depth.**
  - Noon fog is exponential-squared at 0.012: about 6% at 20 m and about 40% at 60 m. It does nothing at play range and
    only greys the far plain.
  - There is no layered haze, no heat haze outside the swell, and no near-camera dust.
- **Exposure and grade.**
  - Exposure +0.15, saturation -10, contrast +20, violet split toning, and a vignette at 0.3.
  - The pale floors sit near white (`suncity2_bowl_0`, `suncity2_champion_2`), so most frames are low-contrast cream
    with violet holes.
- **Horizon.** Never seen during play: see "What the camera can see". The sky colour `(0.78, 0.74, 0.66)` is seen only
  past the edges of geometry, as at the sunset exit, where it is the red haze.
- **Districts.** Every district shares one mood. Only the swell's volume changes the look.

---

## Work plan: the Fire land's world pass (tonight)

### Ground rules

- **Stay out of the boss-effects worker's code.** Do not touch `SunBearer*`, `SunBearerLook`, `SpearLook`,
  `SpearRay`, `SunCourtDirector` or `Enemy`'s fiery circle.
- **Keep Sun Court changes small and separate.** `SunCourtBuilder` builds the ray and spear materials the boss worker
  may also be editing. Keep its changes to its floor, wall and beyond code in separate methods, and check
  `git status`/`git diff` on it before editing.
- **Sun City first.**
- **Do not import the Blender worker's pieces yourself.** Pieces enter `Assets/` only through the asset-intake skill
  once approved. Until then every step below keeps a primitive fallback behind one call, so a modelled piece replaces
  it by changing a single line.
- **Take baseline frames before changing anything.** Copy the current `suncity2_*`, `suncourt_forecourt_1`,
  `fireend_exit_2` and `fireend_sunset_*` frames to `review/shots/before/` (same names), so every step's check is a
  side-by-side.
- **After each step:** rebuild (`Clover/Build/Sun City`, and `Sun Court` when touched), then run `SunCityProbe`
  (baseline 91 `OK`) or `SunCourtProbe` (baseline 51 `OK`).
- **Keep the frame budget.** `PerfProbe` in the market gives a baseline of 3.7 ms mean, 731 draw calls and 7.9 KB of
  garbage a frame. Stay well inside 16.6 ms, and add no new per-frame allocation.

### Steps, in priority order

Each step lists what to change, where, how to check it with a frame, and the payoff.

#### Step 1. Floors and walls with surface: the land's own textures

- **What.** Add sun-land patterns to `ProceduralTextures`, all near-white multiplier patterns in warm greys only (no blue
  or red, the reason the castle's were dropped):
  - `SunMud`: cracked plates with dark, deep cracks, curled edges drawn as lighter rims, silt flecks;
  - `SunEarth`: packed earth, pebbles, tread scuffs, straw specks;
  - `SunBrick`: mud-brick courses, each brick ±6% in tone, eroded mortar, straw flecks;
  - `SunAshlar`: large blocks with chisel marks and chips;
  - `SunSand`: wind ripples.
- **Where.** `ProceduralTextures.cs` (new functions next to `Flagstones`), then `SunCityKit.Plain` and `Flat`, which
  pass `null` today: pass the texture and a tiles-per-metre value, using the world-mapping path in `clover_toon`.
  Map `m_sun_mud`, `m_sun_earth`/`earth_dark`, `m_sun_brick`/`brick_dark`, `m_sun_sand`, `m_sun_silt`,
  `m_sun_bank_face`, `m_sun_ashlar`, `m_sun_stone`.
  - Size paving with the texture: draw stones 0.8 to 1.2 m inside the texture rather than 2.4 m cubes, and keep the cube
    flags only for a few raised or missing stones.
  - `SunCourt` and `FireYard` use the same materials and gain the textures for free.
- **Check.** Recapture `suncity2_bank_1`, `suncity2_roofs_0`, `suncity2_yard_1`, `suncity2_bowl_0` and
  `suncourt_forecourt_1`. Pass if no stretch wider than 4 m is one tone.
- **Payoff.** The largest change for the time: every floor, wall and roof in three scenes at once, and the scale fix of
  smaller stones.

#### Step 2. Ground stains that read as ground, gold that reads as metal, no seam

- **What.**
  - `Soft` stains are drawn unlit in cream and read as fog or clouds (`suncity2_bank_1`, `suncity2_market_1`,
    `suncity2_well_0`). Make silt and sand darker and warmer, lower their alpha, and give `PanMesh` a harder printed
    edge (move the 0.7 ring out and fade only the last 10%). Better still, make them toon-lit decals if the shader
    allows a cut-out.
  - Drifts become ripple-textured low patches (`SunSand`) instead of two stacked blots.
  - `GoldLine` uses `SunCityKit.FloorGold`, not the glowing `m_kit_gold`.
  - Offset or jitter the `mud_tile` grid so its edges do not fall on one straight line at x = 0.
- **Where.** `SunCityBuilder.ReadySoft`, `PanMesh`, `Drift`, `GoldLine`, and the `mud_tile` loop in `DryRiver`.
- **Check.** `suncity2_bank_1`, `suncity2_market_1`, `suncity2_avenue_0`, `suncity2_narrows_0`, `suncity2_swell_0`.
- **Payoff.** Removes three things that read as bugs.

#### Step 3. Roofs with life, and houses that are houses

- **What.** Replace `Mass()` and `RoofTop()` with a `HouseRow` that splits a block into houses 4 to 8 m wide.
  - Each house gets its own height (±0.6 to 2 m), its own parapet (broken in places), and beam ends poking out of the
    south face under the parapet.
  - Each roof draws 2 to 5 items from a district's roof table: reed mats, drying linen on a line, jars, baskets, stacked
    bricks, a stair-head hut with a dark door, a wind-catcher (a sloped box hood), palm-frond shades, a sleeping mat,
    and in district 2 a soot-blackened hearth hole with a smoke thread (step 5) and charred patches.
  - Use `Dressing.Mould` (already used by the hub and summit) to merge each row's pieces into a few meshes.
- **Kit pieces this replaces.** The Blender kit's house modules and roof clutter replace the box body and the
  primitive clutter. Fallback: primitives.
- **Where.** `SunCityBuilder.Mass`, `RoofTop`, and the `Upper Room` loop in `CityOfWicks`.
- **Check.** `suncity2_roofs_0`, `suncity2_overview_city`, `suncity2_lamp_1`.
- **Payoff.** The second most-seen surface, and the one the owner's "low detail" fits best.

#### Step 4. Shade with form

- **What.**
  - **Geometry, which touches only this land.** `Box()` walls in the city gain:
    - a pale coping strip on the top edge, catching the sun as a lit line;
    - a darker foot band 0.4 m high with sand drifted against it;
    - doors and windows as dark insets on long faces every 5 to 8 m;
    - soot stains over doors in district 2.
  - **Shader, optional properties with defaults that change nothing.** In `clover_toon`, a `_BounceColor` and
    `_BounceHeight`: on faces whose normal is horizontal and that are in shade, add a warm floor-coloured bounce that
    fades out over 1.5 m above the floor (world y against a `_BounceFloor` height, or a height above the mesh's bottom).
    Set it only on the Fire land's materials.
- **Where.** `SunCityBuilder.Box`, or a new `Wall` helper used for the long walls; `clover_toon.shader`;
  `SunCityKit.Light`.
- **Check.** `suncity2_alley_0`, `suncity2_lamp_1`, `suncity2_gate_0`: shaded faces show a lit top line, a warm foot
  and openings.
- **Payoff.** Fixes the violet holes that make a third of district 2's frames.

#### Step 5. Fire present in the city

- **What.**
  - A new `SunCityKit.Smoulder(parent, at, size, light)`: a char decal (an irregular dark stain), 3 to 8 small tongues
    (`BuildKit.Fire` shape 6, 0.25 to 0.6 m), a smoke thread (`BuildKit.Smoke`), ember spits, and an optional
    unshadowed point light (range 3 to 4) with a small flicker component.
  - Give every brazier a point light (range 6), not only rest fires.
  - Unlit rest braziers keep a banked-coal glow and a smoke thread; `RestFlames` stops only the tall flames.
  - Soot stains on walls over doorways, lamp niches and hearth mouths.
- **Placement by district.**
  - *District 1:* sparse. Fishers' cook fires and smoking reed heaps.
  - *District 2:* dense.
    - Every lamp niche lit.
    - Embers glowing in doorways (a dark doorway with an orange floor glow).
    - Smouldering heaps in the burnt houses and on roofs.
    - A spilled oil jar burning in the alley.
    - Target: at least two fire sources in any 18 by 11 m frame.
  - *District 3:* ordered and royal.
    - A brazier at every vendor stall interval on the avenue, both sides (every 9.5 m).
    - Fire bowls on the pylon.
    - Lamps on the hall's columns.
- **Kit pieces.** A modelled brazier variant, a lamp-niche piece and an oil-jar piece replace the primitives.
- **Where.** `SunCityKit` (new), `SunCityBuilder.Rest`, `RestFlames`, the district methods. Check the URP asset's
  per-object light limit before scattering lights (the forward path's limit is 8 lights per object).
- **Check.**
  - Frames: `suncity2_alley_0`, `suncity2_market_1`, `suncity2_lamp_1`, `suncity2_avenue_0`, `suncity2_quay_0`.
  - Performance: `PerfProbe` in the market; record draw calls and ms against the baseline.
- **Payoff.** The Fire card's main missing item, and the reason the city of wicks does not read as one.

#### Step 6. Something always moves

- **What.**
  - **Ambient layer.** One emitter that follows the player in world space, in a 30 by 10 by 24 m box. Its preset is
    switched by `LandZones` as the player crosses districts (the district rectangles exist already):
    - *District 1:* dust motes and low sand streaks blowing east.
    - *District 2:* ash flakes falling, embers rising, smoke wisps drifting at 4 to 7 m (between the camera and the
      ground, so they cross the frame).
    - *District 3:* glare motes and a little sand.
    - *Hall:* dust only inside the `HallLight` bars.
  - **Cloth.** Linen strips, awning slats, the drying linen and the hanging nets sway. Use a `_Sway` vertex property in
    `clover_toon` (world-position phase, height-weighted; default 0) or a small runtime `Sway` component on the few
    hanging pieces.
  - **Drifting smoke shadows.** A scrolling smoke cookie on the key light, if the URP asset allows main-light cookies.
    Otherwise large, very faint dark `Soft` blots moved by a small runtime component.
- **Where.** `SunCityBuilder` (the emitter and presets), `World/LandZones.cs` (call the preset swap where it already sets
  the district's heat), `clover_toon.shader` (sway), `LookBuilder` or the scene's key light (cookie).
- **Check.** Two captures of the same idle frame 0.5 s apart (`suncity2_bank_1`, `suncity2_yard_1`, `suncity2_roofs_0`)
  must differ visibly.
- **Payoff.** Meets LOOKS rule 7; the city stops reading as a model on a table.

#### Step 7. Districts told apart

- **What.** Each district gets its own floor signature and its own light. Add a per-district global volume blended by
  weight, as `NoonSwellProfile` is, switched by `LandZones`.
  - *District 1:* warm ochre; mud plates, silt and reed debris; slightly hazier (fog 0.016).
  - *District 2:* sooty; exposure -0.1, contrast +6; dark packed earth with ash, cinders, oil stains and wick
    footprints; smoke haze.
  - *District 3:* glaring; exposure +0.1, a little more bloom; white new stone with gold seams and chisel dust.
  - *Hall:* cool deep shade with bright bars and motes.
- **Where.** `SunCityBuilder` (floor materials per district, the volumes), `World/LandZones.cs`.
- **Check.** One frame per district put side by side, HUD covered: can each be named? (LOOKS.md, "the test for every
  frame".)
- **Payoff.** Rule 4, and a sense of travel.

#### Step 8. What lies beyond the walls (and the sunset exit)

- **What.**
  - **Sun City.** Replace the 320 by 720 m sand cube in `Beyond()` with:
    - lower ground outside the outer walls (y -6 to -12 where the walls allow), filled with a far city of house blocks
      (no colliders, no shadows, GPU-instanced, static) with roof clutter, a few braziers glowing, and smoke columns;
    - beyond them, desert dunes in `SunSand`;
    - south of the river mouth, the dry bed running on with stranded boats.
  - **Sun Court.** Stand the court on a raised platform:
    - drop the outside ground 15 to 20 m;
    - put the city below to the south and west (the same far-city blocks), the dry river's line, and the desert to the
      western horizon where the sun sets;
    - beyond the west gate, a stair or ramp going down, so the exit frame looks down on a city turned orange by the
      sunset instead of into haze.
  - Lower ground is what this camera can see far (see "What the camera can see").
- **Kit pieces.** House modules at a lower level of detail (one mesh per house, no clutter) are ideal here; fallback:
  boxes with the `SunBrick` texture and a parapet.
- **Where.** `SunCityBuilder.Beyond`; `SunCourtBuilder` (the `Ground` slab and the gate's outside; keep it a separate
  method).
- **Check.** `fireend_exit_2`, `fireend_sunset_2`, `fireend_sunset_3`, `suncourt_forecourt_1`, the left edge of
  `suncity2_bank_1`.
- **Payoff.** Answers "mostly haze", and ends the floating-on-a-table look at every edge.

#### Step 9. Landmarks in view

- **What.**
  - Because the north is never in frame, bring the god into it by other means:
    - **A smoke pall from the court** drifting south over the city at 6 to 9 m, with its shadow (steps 5 and 6), thicker
      as the player nears.
    - **Floor lines pointing north:** the gold lines and sun-disc inlays of district 3, plus district 2's worn wick-tracks
      leading to the gate.
    - **Stronger vistas:** V1 to V5 at about twice the distance and a pitch 15 degrees lower, each placed where it sees
      the next tall thing. Give that thing a burning cap and a smoke plume so it reads at that framing.
  - Lower the avenue's statue plinths or move the statues so their heads come into frame from the road.
- **Where.** `SunCityBuilder.Zones` (vistas), `GoldenWay`, the ambient layer from step 6.
- **Check.** `suncity2_vista_v1` to `suncity2_vista_v5`: each shows the next goal; `suncity2_avenue_0` shows a statue's
  head.
- **Payoff.** LOOKS rule 8; orientation; the land feels ruled.

#### Step 10. Props from primitives to shapes (no Blender needed)

- **What.** Use `Dressing.Mould` shapes (`Lathe`, tapered `Box`, `Ball`, `Patch`):
  - oil, cargo and great jars as lathe jars with necks and lips;
  - natron heaps as lumpy mounds;
  - the wrapped dead as tapered bundles with binding bands;
  - the cold oven as a dome with a mouth and soot;
  - rubble as broken brick chunks;
  - the column drums with flutes.
  - The kneeling predecessors in the bowl should become the gilded model posed kneeling, or the kit's statue, not
    cube-and-sphere.
- **Where.** `SunCityBuilder` (`Cargo`, the oil jars, `Rubble`, the natron heaps, `Deco` capsules, the bowl's `Cast`
  pieces).
- **Check.** `suncity2_market_1`, `suncity2_hearth_1`, `suncity2_yard_1`, `suncity2_bowl_0`.
- **Payoff.** Removes the "programmer art" props that sit beside high-quality enemies.

#### Step 11. Edges of the playable space

- **What.**
  - Run `Cornice` (now only on the quay and gallery) along every terrace face.
  - Chip and crumble the tops of long walls (missing blocks, sand lips).
  - Put fallen blocks and sand at the foot of faces.
  - Grow dead reeds and a few dead palms (kit) along the banks.
- **Where.** `SunCityBuilder.GoldenWay`, `CityOfWicks`, `DryRiver`; `Cornice`.
- **Check.** `suncity2_vista_v3`, `suncity2_masons_0`, `suncity2_overview_terraces`.
- **Payoff.** Terraces stop reading as clean boxes (the fifth worker's own complaint).

#### Step 12. Wrap-up and record

- Rebuild Sun City, Sun Court and Fire Yard.
- Run:
  - `SunCityProbe` and `SunCourtProbe`;
  - `FireMechanicsProbe`;
  - the EditMode tests;
  - `PerfProbe` in the market and the court.
- Put a before and after sheet per district in `review/visual/`.
- Add `ShotStation`s for one frame per district so `Clover > Review > Capture Stations` covers the city in future. Do
  not accept goldens: that is the owner's word.
- Write an "As built" section in `fire_land.md`.

### Where the Blender kit replaces builder pieces

| Kit piece wanted | Replaces (builder, method) | Fallback until approved |
|---|---|---|
| Mud-brick house modules (wall run, corner, doorway, window, upper room, stair-head) | `SunCityBuilder.Mass`, `RoofTop`, the `Box` house walls in `CityOfWicks` ("Alley Houses", "Arm Houses", "Square South Houses", "Upper Room"), `FishersBank` huts beside `house_block` | `HouseRow` from primitives plus `SunBrick` |
| Roof clutter set (mats, linen line, baskets, jars, brick stack, wind-catcher, frond shade) | `Mass` "Roof Wall" cubes, `RoofTop` hatches and seams, the "Drying Linen" canopies | Mould shapes |
| Palms (dead and living), reed clumps | Nothing yet: there is no foliage in the land. Add along `DryRiver` banks, `FishersBank`, the well court, the avenue's sides | None |
| Columns (a shorter portico column, a broken column, fluted drums) | `Colonnade` (the lotus column at a third of its size), "Terrace Pillar", "Gallery Pillar", "Column Drum", "Fallen Column" | The kit's `column_lotus` |
| Pylons and gate pieces | "Pylon Wing" boxes, the embalmers' gate towers ("Gate Tower West/East", "Tower Cap"), `LastGate`'s gate wall and jambs | Boxes |
| Statues: falcon, kneeling gilded predecessor | Falcons as `statue_king` at 0.45 (`GoldenWay`); "Cast Legs/Body/Head" in `LastGate` | The kit's king |
| Props: jars (oil, water, great), crates, bales, oven, embalming table, natron heap, wrapped dead, sledge, scaffold | `Cargo`, the oil jars, "Great Jar", "Cold Oven", "Embalming Table", natron spheres, the "Wrapped Dead" and "Standing Dead" capsules, "Sledge", scaffold poles | Mould shapes |
| Ground dressing (rubble set, fallen cornice, brick scatter, reed debris, fish bones, boat ribs, pottery shards) | `Rubble`, `Slumps`' "Loose Brick", `Scatter`'s cube stones | Mould shapes |
| Brazier variants, lamp niche, wall lamp | `SunCityKit.Place("brazier")` (one model), "Lamp Niche" plus "Lamp" spheres | The kit's brazier |
| Far-city low-detail houses | `Beyond()` | Textured boxes |

### As built (2026-11-18)

Two workers in one night. The first half (commit `0a20bec`) did steps 1 to 5 with the Blender city kit. The second half
calmed the paving, paid back the cost, and did steps 6 to 11 in part. Nothing here is accepted by the owner yet.

**First half (steps 1 to 5).**

- The land's own textures (`ProceduralTextures.Sun*`) are on the city's walls, floors and faces.
- The kit's brick and stone fronts stand on the house boxes (`Wall`, `House`, `Roof`, `RoofClutter`).
- The ground is the kit's slabs and cracked mud, painted per district (`GroundPaint`, `Tone`).
- A warm bounce lights faces in shade (`clover_toon` `_BounceColor`).
- Fire is in the streets: braziers, smoulders, lamps and hearth glows, with flickering lights (`SunCityKit.Brazier`,
  `Smoulder`, `Lamp`, `FireFlicker`).

**Second half: fixes.**

- *Paving.* The kit's slab tiles (40% sand, octagon heaps, overlapping in places) are replaced. Each `Paving` call now lays
  one `PavedFloor`, built when the scene opens by `PavingLayout`:
  - courses of slabs 0.6 to 1.1 m across (0.9 to 1.45 m on the king's new stone), laid across the whole stretch with no
    tile repeat;
  - joints a shade under the stone; some slabs chipped, cracked or recut paler;
  - worn-through patches with uneven edges where the old "missing" squares were, with the same litter;
  - one layer only (the quay's strip is cut out of the main paving);
  - a 40 m wash of tone over it all (`ProceduralTextures.SunMottle`).

  The dice are drawn exactly as before, so nothing placed after a stretch moved. The forecourt is paved the same way.
- *Roofs.* There is one hatch tile per six at most, never touching another. Roofs run to the box's edge where no front
  stands. House boxes are mud plaster (`m_sun_plaster`), not brick courses.
- *Riverbed.* Its pale silt plates are toned near the mud (`MudPaint`). The yard's drains are dark earth, not char.

**Second half: cost.**

- *Merging.* `SceneryMerge.Prepare` (editor) writes every still, static, toon-drawn piece of the city down as mesh,
  materials and place, and takes the piece out of the scene. `MergedScenery` (runtime, `[ExecuteAlways]`, never saved)
  merges them when the scene opens: one object for each 40 m square and each fade group.
- *Carrier materials.* Materials that differ only in colour share one white carrier (`clover_toon` `_VertexColour`,
  default off), and each piece's colour rides in its points. The city's 131 materials draw as 23.
- *Fades.* Fronts still thin out with their box: they are merged per Fadeable, and the Fadeable is pointed at the merged
  renderer in play.
- *Shadows.* Fronts cast no shadow; the box behind them does, with a thin parapet caster for the roof behind a south
  front. Fadeables' shadows are merged per square.
- *Outlines.* Materials with no outline skip the outline pass.
- *Particles.* Far fires and roof fires go without sparks.
- *The kits* are read when the scene opens (`AssetImportRules`: Environment `fire_*` and `tomb_*` are readable).

**Second half: steps 6 to 11.**

- *Step 6.* Done.
  - Palms (kit `palm_a/b/c`), reeds and papyrus sway through `clover_toon` `_Sway` (default 0; height-weighted, gust and
    flutter). The yard's linen strips hang and swing. Roof drying lines sway.
  - `DistrictMood` (runtime) follows the player with each district's air: dust motes and blown sand over the river
    town; ash, sparks and smoke wisps over the city of wicks; glare motes, sand and the court's smoke pall on the king's
    ground; motes in the hall's bars of light.
  - Three kites wheel high over the player, drawn only as their shadows (`CirclingBirds`).
- *Step 7.* Done for the light. Each district has a global volume (exposure, contrast, saturation, colour filter,
  bloom) and its own fog density, eased by `DistrictMood` from `LandZones.District`. The floor signatures per district
  were not changed.
- *Step 8.* Done.
  - The city now stands on a mound (the tell, its faces dried mud in strata) 11 m over a lower town of flat-roofed
    houses, lanes, roof hearths and smoke columns, palms, and dunes beyond. Gardens and palm groves lie on the mound
    outside the west wall (`SunCityBeyond.City`).
  - The court stands on a platform 18 m over its own lower town (`SunCityBeyond.Court`, called from
    `SunCourtBuilder.Build` right after the ground slab; the slab is kept to stand on and no longer drawn).
  - A 40 m stair goes down west from the sunset gate, and a road runs on toward the dunes with fires along it. A dry
    river line runs south-east.
- *Step 9.* Partly.
  - Vistas pull back to 2.2 times the distance at 20 degrees less pitch, looking 10 m ahead.
  - Fire bowls with smoke plumes stand on the embalmers' gate towers, the pylon, the gate of noon's wall and the
    obelisk.
  - The court's smoke drifts south over district 3 (`DistrictMood` "Pall").
  - Not done: the statues' heads (impossible from this camera without moving them) and new floor lines.
- *Step 10.* Partly (`SunCityProps.Dress`).
  - Oil, cargo, vendors' and great jars are kit jars and amphorae.
  - Natron heaps are lumpy mounds and the wrapped dead are bound bundles (meshes made here).
  - Squared blocks and offcuts are kit fallen blocks; broken drums are kit broken columns.
  - The bowl's predecessors are the seated king cast in dull gold.
  - Rubble cubes are kit rubble and brick heaps.
  - Not done: the yard's baskets and linen bundles, the column drums' look.
- *Step 11.* Only reeds and papyrus at the banks' feet, palms, and gardens on the mound. No new cornices, wall-top
  crumbling or fallen blocks at faces.
- *Step 12.* This note; frames are `review/shots/current/world2_*.png`; numbers in `review/checks/world2/`.

**Cost, measured in the market's busy moment** (`profile_city.sh`, PerfProbe):

| | Draw calls | Set-pass calls | Triangles | Profiler main thread median | GPU median |
|---|---|---|---|---|---|
| Before the pass | ~796 | n/a | ~333k | 10.3 ms | 3.4 ms |
| After the first half | 1549 | 334 | 712k | 11.7 ms | 4.3 ms |
| After the second half | 736 | 232 | 707k | 11.9 ms | 4.5 ms |

The triangle count is back where the first half left it: the merged squares are culled whole, and the district air and the
fire on the tall things add to it. About 6 ms of every recorded main-thread frame is the profiler's own memory counters. The render loop's own time is back
to 1.56 ms (before 1.51, first half 1.86). The scene file is 30 MB, down from 36 MB.

---

## A later work plan for the castle, the summit and the hub

The demo stays untouched tonight. The owner has said the Lightning land's enemies and area will be reworked after the
demo. The order below follows what most makes them look unfinished.

1. **Castle: make it a Japanese castle from above.**
   - Curved tiled roofs with eaves over the walls and towers, timber galleries, white plaster with black boards, and
     sloped *ishigaki* stone bases.
   - These come from a Blender kit, as the Fire land's does.
   - Replace `Tower`'s stacked boxes and the wall-top caps (`CastleBuilder.Tower`, `Wall`, `Crenels`).
2. **Castle and summit: fire and lightning as substance.**
   - Replace the crystal brazier flame and the magenta floor discs with `BuildKit.Fire` flames and real point lights
     (`CastleBuilder.Brazier`, `Flames`).
   - Give bolts branches, an after-image and a scorch star where they strike (`LightningBolts`, `VfxBuilder` common set).
   - Turn telegraphs into crackling lines along their outlines.
3. **Floors with each place's own story.** Raked gravel and wind-combed grass in the castle's courts; puddles that ripple
   with the rain; the summit's arena worn by the dragon. Use new textures rather than the shared `Flagstones`, which
   makes the hub, castle and summit look the same.
4. **What lies beyond.**
   - *Castle:* a castle town of roofs far below the causeway, lit windows, storm cloud lit from inside.
   - *Summit:* layered moving cloud cards, a cliff face under the rim, and peaks.
   - *Hub:* a backdrop for the void, with silhouettes of the lands as stopped islands, a lip and stopped water at the
     disc's edge.
5. **Shade with form and world ink.** Apply the step 4 shader properties to their materials, with cool bounce instead of
   warm.
6. **Something always moves.** Leaves and paper streamers in the wind, rain splashes on puddles (rain already exists),
   cloud shadows on the summit.

---

## What could not be judged from the frames

- **Fire Yard frames are older than the current city.** They predate the latest effects pass (they show "Embers" in the
  HUD), so the yard's current look is unknown.
- **The castle's and summit's surroundings are rarely in frame** in the tour frames. The builders say what is there, but
  how it reads was not seen.
- **Some effects have no frame:**
  - the priest's beam at its strike (only its tell and travel);
  - the champion's fire arcs in the city;
  - the falcon's pass;
  - wick footprints;
  - the player's hazards in the Fire land.
- **Motion cannot be judged from stills:** flicker, sway, the swell's onset.
- **Performance of the proposals is estimated, not measured.**
- **The cause of the hall's stipple** (`suncity2_hall_1`) was not found in the files.
