# How each land looks and feels

A look sheet for every land, written before any of them is built, so that a worker starting a land knows what the
screen should show. Started 2026-11-17 (evening) to be worked through with the user. Statuses as in `WORLD.md`:
**Decided** (the user said so), **Leaning**, **Proposal** (Claude's, not yet accepted), **Open**. Unless marked,
everything here is a **Proposal**.

What each land *is* (its myth, enemies, rule and fight) is in `WORLD.md`. This file is only about what it looks like,
sounds like later, and how it feels to walk through.

---

## 1. Rules for every land

These come from looking hard at the Fire land's frames (`review/shots/current/suncity2_*`, 2026-11-17) and at what
the camera actually sees.

**The camera sees floor and roofs, not walls.** At 12 m, a 40 degree field of view and 55 degrees of pitch, about
three quarters of every frame is ground or the tops of things. The Fire city's frames are mostly one flat tone of
floor and blank rectangles of roof. So, in every land:

1. **The floor is the main canvas.** Every floor carries its land's story from above: cracks, drifts, wet lines,
   patterns, stains, inlays, debris, tracks. No stretch of floor wider than about 4 m is one plain tone.
2. **Roofs and wall tops have life.** Whatever the land's people kept up there (jars, mats, nets, snow, moss,
   drying cloth, broken tiles) is the most-seen surface after the floor.
3. **Shade has form.** Walls in shadow are not one flat purple slab: they keep the cel bands, a rim of light at the
   top edge, and a line of bounced colour from the floor at their foot.
4. **Each district has its own floor and its own light**, so a screenshot tells you where you are without the HUD.
5. **The element is the only strong colour, and it is a substance.** Fire is flame, smoke and char; water is spray,
   foam and ink; metal is sparks and molten pour; ice is frost, shards and breath; poison is mist, drip and bubbles.
   White is kept for the hottest core or the brightest spark and is used sparingly. (Decided by the user for Fire,
   2026-11-17: "reads too much like light and not fire". Taken here as the rule for every land.)
6. **One rare accent per land.** A single colour outside the palette used only a few times, each on something the
   lore wants seen (Fire's lapis blue on the well's lid and the tomb tears).
7. **Something always moves.** A still frame of an empty room should still show motion: dust, foam, sparks, snow,
   mist, flags, birds, embers. Nothing in a land is dead still except where stillness is the point (the champion's
   frozen war).
8. **A landmark is always in view**, far off at the top of the screen or as a shadow falling into frame, so the
   player knows where the god is.
9. **Silhouette language differs by land** (square, curved, vertical, spiral), so even a black-and-white frame tells
   the lands apart.
10. **Hard shapes for effects, from above.** Attacks read from the camera angle: lanes and rings on the floor are made
    of the element (a line of foam, a crack of frost, a creeping mist edge), not of red paint, wherever that can stay
    readable. Red marks stay for what cannot be guarded.

**The test for every frame**: cover the HUD. Can someone name the land, the district and the element?

---

## 2. The climb at a glance

**Decided (user, 2026-11-18)**: the mood frames of Ice, Poison, Lightning and Wind, Earth and Life and the champion's
seat (`staging/blender/2026-11-18_moods/`) are the right moods; small details come when each land is built.

The lands are set apart by time of day, sky, shape and the one strong colour. Read across a row for one land; read
down a column to check that no two lands blur together.

| Land | Time and sky | The muted world | The element's strong colour | Rare accent | Shapes | The floor from above |
|---|---|---|---|---|---|---|
| Fire (Egypt) | Always noon; white-hot sky | Bone mud brick, sand, pale stone | Flame orange and poured gold | Lapis blue | Squares, slopes, pyramids, flat roofs | Cracked mud, dust drifts, worn stone with gold seams |
| Water (Polynesia) | Tropical sun with a storm coming in from the sea (Decided); the storm nearer the deeper you go | Black volcanic sand, grey basalt, green peaks and palms, bleached driftwood, dry thatch | Sea turquoise in the shallows, ink in the deep, white foam | Coral red | Curves, hooks, spirals, canoe prows | Wet black sand with foam lines, tide pools, reef that comes and goes |
| Metal (Greece) | Volcanic dusk; ash sky lit from below | Soot-grey marble, slag, verdigris bronze | Molten bronze: white-gold pour cooling through cherry red to black, sparks | Aegean deep blue | Straight lines, right angles, Greek key, columns | Meander flagstones, rails, slag, grates with forge glow under them |
| Ice (Norse) | Overcast mountain daylight, softened by blowing snow (Decided) | Blue-grey snow, black rock, dark pine, grey timber | Ice cyan-white, frost, breath | Blood red (the hunt) | Long verticals: pines, masts, ribs; knotwork | Wind-rippled snow, clear ice with things frozen under it |
| Poison (China) | Night; a sick moon in haze; paper lanterns (proposed amber, not red, so they do not compete with the player's red robe) | Black lacquer (the Qin court's colour), grey roof tiles, dark water, grey clay | The miasma in two strengths: blue-green "green-grass" mist, and bile-yellow "yellow-grass" mist at the deadliest hour | Cinnabar red (kept small and flat, below the player's vermilion robe) and mercury silver; the alchemist's own purple and gold | Upturned eaves, round moon gates, lattices, walls inside walls | Dark wet stone in courtyard grids; the mist line swallowing the floor |
| Lightning and Wind (Japan) | Dry storm night, no rain, cloud lit from inside; the climb goes into the storm and out of its top, so the light steps from far and dim on the shore to strobing on the summit to lit from below on the clouds | Indigo stone, dry grass bleaching toward silver as the climb rises, ash, char-black and wind-stripped grey gates, blue-black sea | Lightning cyan-violet; wind as pale streaming dust and grass | Shrine vermilion: the one whole torii in the surf, paint left on ruined gates, the crossing shrines, the war fan | Torii, zigzag stairs crossing like a braid; the castle half angular (sloped walls, stacked tile roofs), the monastery half curved (swept bark roofs, bells, fans) | Stone and dry grass combed flat by the wind; branching lightning scorch; raked gravel the wind keeps raking |
| Earth and Life (Celtic) | Grey dawn mist on a moor; a low sun that never clears the horizon until both gods fall, always in the same quarter | Grey-green moor, peat, lichen stone, black bog water, weathered oak | Earth: ochre stone dust and cracks. Life: living green that overgrows, the brightest colour in the land | Gold as worked metal: torcs, brooches, the scythe, lit spirals | Circles, spirals, standing stones, barrows; upright and straight on Earth's side, tangled and round on Life's; no knotwork (Ice's) | Heather, peat, the plank road over black pools, stone circles, ochre cracks, roots breaking through |
| The champion's seat (India) | A great war frozen under an eclipse; corona in the sky (decided 2026-11-18: frozen just before totality, a last crescent of sun, totality at the fight's phase change) | Everything outside the thaw drained to grey, motionless (Decided) | Colour returns only around the player (Decided); the council's seats each in their element's colour (Decided); in the fight, Light gold-white and Dark violet-black | Saffron and vermilion (marigolds, banners), seen only inside the thaw | Mandalas, stepped temple towers, chariots; in the grey, each land's soldiers told apart by silhouette | A battlefield of frozen arrows, dust hanging in the air, shadows of things that will not fall; the thaw's circle of colour |

**Open**: Fire's flame orange and Metal's molten bronze sit close. The proposal keeps them apart by substance and
setting: Fire is soft flame and smoke in bleached daylight; Metal is hard liquid metal, sparks and glowing seams in a
dark dusk with blue sea around it. See Metal's questions.

---

## 3. Fire: the sun-king's city (Egypt)

Built and in its second pass; this card is the reference the others follow. The full specification is
`fire_land.md` section 2. **Decided**: always noon; the city bone-pale and still; only flame orange and poured gold
are strong. What the frames show still needs (for a later pass, after the current workers): floors that tell a story
from above, roofs with life, fire actually present in the city (braziers that burn, wicks that smoulder, soot over
doors, embers in the air), and effects that look like flame rather than light.

**As built (2026-11-18, the world pass; awaiting the owner's review, nothing here is decided).** The detail is in
`review/visual/overview.md`, "As built". In short:

- **Surfaces.**
  - Walls, floors and faces carry the land's own patterns.
  - House fronts and roofs are the Blender city kit.
  - Paving is courses of slabs under a metre and a half with quiet joints and a broad wash of tone, worn through in
    patches.
- **Fire.** Braziers, smoulders, lamps and hearth glows burn in the streets, and fire bowls with smoke plumes stand on the
  tall things.
- **Movement.**
  - Palms, reeds and hanging linen sway.
  - Dust, ash, sparks, smoke and glare motes drift by district.
  - Kites' shadows cross the ground.
- **Light.** Each district has its own grade and fog: warm ochre by the river, sooty in the city of wicks, glaring on the
  king's ground, cool in the hall.
- **What lies beyond.**
  - The city stands on a mound over a lower town and dunes.
  - The court stands on a platform over its own lower town, with a stair going down west from the sunset gate.

---

## 4. Water: the fisher of islands (Polynesia)

**Decided (user, 2026-11-17, night)**: tropical with a storm coming, after seeing the two mood frames
(`staging/blender/2026-11-17_moods/water_tropical_*.png` and `water_storm_*.png`). The frames below were rewritten to
that.

**One image.** Black sand and green peaks under a hot tropical sun, palms leaning in the wind, turquoise shallows,
and out at sea a dark wall of storm that is a little nearer every time you look.

**Light and weather.** Tropical daylight, warm and clear overhead, the sun high and slightly west. Out to sea a storm
wall stands on the horizon: anvil cloud, rain curtains, lightning without sound. The storm is the land's clock of
story, not a timer: the deeper the player goes, the nearer it is and the more of the sky it takes, so the light goes
from bright and warm in the first district, to half sun and half shadow at the storm's edge, to the full storm on
the fight's beach. Wind rises with it. When the tide is high the light dims and greens a little, as if seen through
shallow water; at low tide it is drier and paler.

**Palette.** The world is black sand, wet dark basalt, green volcanic peaks and palms, bleached grey driftwood,
sun-dried thatch and pandanus mats. The greens are deep and a little muted so the water stays the strongest thing:
turquoise shallows, white foam, ink-black deep water. The storm is slate and bruise-purple. **Accent**: coral red, used
only on the reef-grown's armour, the fisher's hook and the gash where the brothers cut.

**Shapes.** Curves and hooks: canoe prows, fishhook carvings, spirals, outrigger frames, carved poles with stacked
faces kept vague and respectful, nothing copied from real carving traditions. Thatch roofs are steep and long.

**The floor from above.** Wet black sand with the curved lines of the last wave, foam that slides in and draws back,
tide pools that reflect the sky, reef heads that appear at low tide and vanish at high tide. The tide line is the
land's big readable shape: the player always sees where the water is now and where it will be.

**What moves.** Foam lines, the tide itself, nets shifting in the wind, frigate birds overhead, the hooked jerking
on their lines (the lines run off-screen toward something unseen), the ground of the fished-up island rising and
falling slowly like a sleeping chest.

**The element's effects.** Water as a substance: heavy curling swells, spray thrown up in sheets, foam that lingers
where water hit, black ink water from the deep. Arms of the octopus are seawater with solid suckers and tips
(Decided). A lane is a line of churning foam; a ring is a ring of rising water.

**District by district.**
1. *The drowned village* (in full sun, the storm far off): stilt houses half under the water, thatch roofs at water
   level, canoes hanging in trees where the tide left them. At low tide a muddy road appears between the houses; at
   high tide only the roofs. The drowned stand still under the surface, seen as dark shapes; the sunny calm makes
   them worse.
2. *The fished-up island* (at the storm's edge, half sun and half cloud shadow): raw land that is still a fish. The ground is ridged like scales and shines wet; caves are
   gill slits that breathe out warm mist in rhythm; tufts of seaweed instead of grass. No buildings, only the
   brothers' cuts, with red wet rock showing, deepening toward their room.
3. *The navigators' cliffs*: high dark headlands, wind, carved hook-shaped stones marking star paths, canoe sheds,
   and from the top the long beach where the fight starts.

**The god's room.** A long beach of black sand between the cliffs and the sea, islets just offshore, under the full
storm: the sun gone, rain driving across, the sea white-capped. In phase two the eye of the whirlpool: bare seabed,
wrecks, walls of spinning green-black water rising all round, light falling in from the hole of sky high above.

**The ride out.** The voyaging canoe under a crab-claw sail as the storm breaks: the cloud tears open and the
tropical sun comes back over a calm sea.

**Avoid.** A postcard with no threat in it: the storm is always somewhere on the horizon. Anything that looks like
*Moana* (Decided caution in `WORLD.md`).

---

## 5. Metal: the lame smith (Greece)

**Decided (user, 2026-11-17, night)**: the separation from Fire is enough, and the game-camera mood frame
(`staging/blender/2026-11-17_moods/metal_game.png`) is what the user had in mind, "just obviously way less detailed".
The frames below stand.

**One image.** A white marble island gone grey with soot under a dusk sky lit from below by forges, the deep blue
sea all round it far below, and a bronze giant walking the coast on its round.

**Light and weather.** A long dusk: the sky ash-grey turning violet, lit orange-red from below where the volcano
forge glows. The key light is low and cool from the sea side; a second, warm light comes up from forge vents and
grates in the floor, so things are lit from two sides. Ash falls slowly like grey snow. When the giant passes, its
shadow slides across whole districts.

**Palette.** Soot-grey marble, dark slag, green verdigris on old bronze, ochre dust in the quarries. The strong
colour is molten bronze: white-gold where it pours, cooling through cherry red to black crust. **Accent**: the deep
Aegean blue of the sea seen from the walls and on a few old painted pots.

**Shapes.** Straight lines and right angles: the labyrinth is all corridors and corners. Greek key borders on
everything the smith made; fluted columns (many broken); tripods; scaffolds and cranes in the quarries. The
smith's work is rivets, plates and hinges; the older Cyclopes' work under it is huge, rough and single-eyed (round
windows, round furnace mouths).

**The floor from above.** Flagstones laid in a meander pattern, rails for ore carts, slag heaps, iron grates with
forge glow under them, cracked plates that the traps hide in (a careful eye sees which plates are newer).

**What moves.** Sparks drifting up from vents, the giant on its round in the distance or its shadow, ore carts,
chains, automata rolling on their legs, steam venting, ash falling.

**The element's effects.** Metal as a substance: sparks with hard bright heads and short tails, liquid bronze that
pours and sets, glowing seams, red-hot metal cooling to black, the ring of a struck plate shown as a sharp ripple.
Not fire: there is little flame and smoke, the heat is in the metal itself.

**District by district.**
1. *The quarries*: terraced white-grey pits cut into the hill, the chained working the faces, ore carts on rails,
   cranes, the giant first seen on the skyline.
2. *The labyrinth*: high walls and corridors (the camera sees down into them, so the wall tops carry Greek key and
   the floors carry the routes), traps in the plates, walls that slide. The giant's round passes overhead and its
   shadow and boulders fall into the corridors.
3. *The volcano forge*: the brightest place in the land; channels of molten bronze in the floor, great bellows
   breathing, anvils, the bronze-veined working. Lit from below, deep red.

**The god's room.** A forge room at the labyrinth's heart, low and round, lit only by the anvil's glow and a
furnace mouth like a single eye. When the giant crashes through the roof, dusk light and ash pour in, and phase two
spills out into the broken labyrinth.

**The ride out.** The bronze chariot and its bronze horses, cooling from red as they run, out across a causeway
over the sea.

**Avoid.** Too close to the Fire land: no flame-orange washes, no bleached daylight. No gears or steampunk; this is
bronze-age making.

---

## 6. Ice: the huntress of the snows (Norse)

**Decided (user, 2026-11-17, evening)**: a snowy mountainscape, in the vein of the opening snowy mountains of
*Horizon Zero Dawn* and the ice area of *Prince of Persia: The Lost Crown*, with a layer of snow blowing around, so it
is not very bright but not very dark either. This replaces Claude's first proposal of endless blue twilight.

**One image.** High snowy mountains under a pale overcast sky, snow streaming off the ridges and blowing low across
the ground, black pines, a frozen fjord full of ships far below, and a red-fletched arrow standing in the snow just
ahead of where the player was about to step.

**Light and weather.** Overcast mountain daylight. The sun is a soft bright patch behind high cloud, so shadows are
present but soft-edged and blue. A layer of snow is always blowing: low drifting streams across the floor, which
show the wind's direction, and thin veils in the air that take the glare off the snow and hide the far distance.
The snow lights things from below, so the shade is blue-grey, never black. Breath shows. The harbour is calmest. The
forest has the thickest snowfall, dimmer under the pines. The giant's bones are the windiest, with the most blowing
snow and the brightest sky. **Proposal**: the skull lake at the top is where the cloud breaks, and the light there is
clearest, until the fight's blizzard closes it in again. (The aurora of the first proposal is dropped unless the user
wants it back for the lake.)

**Palette.** Blue-grey snow, black rock, dark green-black pine, grey weathered timber, iron. The strong colour is ice:
cyan-white, frost, the glow inside thick ice. **Accent**: blood red, used for the hunt only (the huntress's fletching
and cloak, the alpha's markings, wounds in the snow).

**Shapes.** Long verticals and points: pine trunks, masts, spears, ribs, horns. Knotwork and runes cut in wood and
stone. The longships' curved prows are the one soft shape.

**The floor from above.** Snow rippled by wind, with drifts against everything; clear lake ice with dark water and
frozen things under it (fish, ships, the giant's teeth); paths trodden into the snow; ski runs as long clean lines.
Tracks show only in the wolf phase (Decided rule).

**What moves.** Snow blowing off ridges in sheets, the buried moving as lines of heaving snow, ravens, the pack
crossing the far edge of the screen, the huntress glimpsed on a ridge and gone.

**The element's effects.** Ice as a substance: frost that creeps across the floor in branching lines, shards that
burst outward, freezing breath as a rolling white fog, ice that grows in spikes, snow thrown up in sprays. Brittle
enemies shatter into real fragments.

**District by district.**
1. *The ice-locked harbour*: longships frozen in the fjord at angles, their masts as the huntress's perches, a
   harbour town of turf-roofed longhouses buried to the eaves.
2. *The hunting forest*: dense black pines, ski runs, snares visible to a careful eye, the pack.
3. *The giant's bones*: wind-cleared ridges that are ribs and a spine; climbing from rib to rib; at the top the horns
   of the skull.

**Light by district** (added 2026-11-18; **Proposal**, drawn out of the light paragraph above):
1. *The harbour*: the calmest air and the flattest light. The drifting snow is a low veil, the fjord ice is pale and
   the frozen ships stand dark against it. The sun's bright patch hangs low over the fjord mouth. Each district's
   pattern of being hunted is in `WORLD.md`; here it is the warning shot, so the light must leave the path ahead clear
   enough to see an arrow land.
2. *The hunting forest*: dimmer and bluer under the pines, with the thickest snowfall and grey shafts of light between
   the trunks. The floor's snow is the brightest thing, so the pack reads as dark shapes on it and the snares as dark
   lines a careful eye can find.
3. *The giant's bones*: the brightest sky and the hardest wind, snow torn off the ribs in sheets. The ribs' shadows
   lie across the climb in long soft blue bars, which are the cover the player runs between under her arrows.

**The alpha's ground** (**Proposal**; `WORLD.md` does not place it). A clearing at the top of the forest, before the
first rib, where the pack beds down: snow trampled to grey, bones and torn hides, the pines at their thickest round it.
It is the darkest place in the land, so the alpha's blood-red markings are the strongest thing in it.

**The first god under the land.** Since 2026-11-18 every land's murdered god is that element's first god (Decided,
`WORLD.md`). Here that is the giant whose bones, blood and skull the land is made of (the Ymir nod, Proposal in
`WORLD.md`), so nothing more needs adding to show him. What the frames must do is make the ridges read as ribs and the
crater as a skull, so that on the way up the whole land is plainly his body.

**The god's room.** The frozen lake in the bowl of the skull, a crater rim all round, the clouds above (the
"thoughts" of the myth) broken open over it. Phase two closes in to a blizzard: white, with only the wolf's shadow,
breath and eyes.

**The god's room, light** (added 2026-11-18; **Proposal**):
- **Phase 1**: under the broken cloud, the clearest and coldest light in the land. A real sun gives hard blue shadows
  on the lake for the first time, and the ice is dark and glassy, with the frozen things under it showing best here.
  Her skate lines are long pale curves on the dark ice.
- **Phase 2**: the blizzard takes the light down to a flat white-grey with no shadow but the wolf's. As heavy hits thin
  it, the shadows come back one by one.
- **The cracks**: when the lake cracks, the holes of black water are the darkest things in the frame.

**The ride out.** The burial longship rising through the broken lake, frosted, ancient, the player alone on it as
the snow settles and the wind drops for the first time in the land.

**The ride out, light** (added 2026-11-18; **Proposal**): the blizzard gone and the cloud edge lifting, a low sun
under it laying long white-gold light along the lake and the frosted hull. It is the only warm light in the land.

**Avoid.** A white screen where nothing reads; the blizzard thins with each heavy hit (Proposal in `WORLD.md`).
Also (added 2026-11-18; **Proposal**):
- horned helmets and the other Viking cliches;
- an ice blue that drifts toward Lightning's cyan-violet: Ice is cyan-white in daylight, Lightning cyan-violet at
  night;
- the hunt's red spreading to anything that is not the hunt;
- snow as one flat white plane without drifts, ripples or paths (rule 1);
- the aurora, dropped with the first proposal, unless the user asks for it back.

**The player's outfit** (approved 2026-11-18, `docs/GAME.md`): a deep blue wool tunic with rust tablet-woven bands, a
dark brown fur mantle, grey-brown leg wraps cross-gartered in rust thongs, and a thick rust wool scarf. From above it
is a dark figure on pale snow, a strong read, and the rust is its warm mark. The one
risk is the hunt's blood red. Keep the hunt's red a deep crimson, cooler and darker than the scarf's rust, so the
player's scarf is never read as one of her arrows or a wound in the snow.

**Mood frames to render** (added 2026-11-18). Game camera: 12 m from the player, pitched 55 degrees down, 40 degree
vertical field of view, the player at the centre, 1280 by 720. The player wears the Ice outfit
(`staging/blender/2026-11-18_outfits/outfit_ice.fbx`), mid-stride unless stated. Render to
`staging/blender/<date>_moods/` under the names given.

1. **`ice_warning_shot`**: the land's first district.
   - *Camera*: game camera, on the harbour path.
   - *In frame*:
     - a trodden path through the harbour town between longhouses buried to the eaves, their turf roofs under snow,
       smoke from one;
     - the stern and mast of a frozen longship standing at an angle at the top of the frame;
     - the edge of the fjord ice at the bottom of the frame;
     - a red-fletched arrow standing in the path 2 m ahead of the player;
     - drifts against every wall, wind ripples on the open snow, low streams of blowing snow crossing the frame.
   - *Light*: overcast daylight, the sun a soft bright patch behind cloud at the upper left; soft blue shadows; the
     snow's bounce lifting the shade; a thin veil of snow in the air.
   - *Must prove*: the frame is neither very bright nor very dark (Decided); the snow floor is never one flat tone;
     the fletching is the only red besides the player's scarf, and the two do not read as the same red; the dark
     player reads on the pale ground.
2. **`ice_skull_lake`**: the god's room.
   - *Camera*: wide, from the crater rim behind the skull's horns, about 60 m back and 30 m up, looking down onto the
     lake.
   - *In frame*:
     - the two horns in the foreground framing the bowl;
     - the frozen lake filling the crater: clear dark ice with ship hulls, fish and huge teeth frozen under it;
     - the crater rim with icefalls;
     - above, the cloud torn open, with a ring of cloud round the hole;
     - the huntress's skate lines as long pale curves on the ice;
     - the player a small figure at the lake's edge.
   - *Light*: phase 1, the clearest in the land; a real sun through the hole, hard blue shadows, the ice glassy.
   - *Must prove*: the lake reads as a stage with an edge; the things under the ice read from above without the ice
     looking like water; the bowl and the horns read as a skull.
3. **`ice_blizzard`**: phase 2.
   - *Camera*: game camera, on the lake.
   - *In frame*:
     - lake ice under a blizzard;
     - at the upper right edge, the wolf as a shadow with two clouds of breath and its eyes glowing pale cyan;
     - a line of fresh, bright, short-lived tracks crossing near the player;
     - one crack starting in the ice.
   - *Light*: flat white-grey with no shadow but the wolf's; snow streaming hard across the frame.
   - *Must prove*: a blizzard frame is not a white screen; the player, the tracks, the shadow and the eyes all read;
     ice cyan stays in frost and cracks and does not tint the whole frame.

---

## 7. Poison: the court alchemist (China)

**What the sources give** (researched 2026-11-17, evening, at the user's request: "research what this might look like
with inspiration from the actual myths"):
- *The miasma of the south* (zhang, zhangqi). Chinese writers from the Tang onward feared the poison air of the far
  south, which rose from twisted, overgrown mountains and stagnant water: "a great amount of mist and fog that
  cause miasmas", described as "neither smoke nor fog nor clouds". It was named by season and grass. The
  *green-grass* miasma came in spring and summer. The *yellow-floss-grass* miasma came in autumn and was held to be
  the deadliest. So the myths give the poison mist two colours: a blue-green, and a yellow that is worse.
- *The alchemist's furnace* (waidan). The elixir work used cinnabar (red mercury ore), mercury, lead, realgar
  (orange-red arsenic), orpiment (gold-yellow arsenic) and malachite (green). The word for elixir, *dan*, also means
  cinnabar and vermilion. One recipe heats mercury and lead until they give off their "Essence and Flower", which is
  **purple or gold**, and cinnabar roasts into mercury and back into cinnabar in a cycle. Several emperors died of
  these elixirs.
- *The first emperor's tomb*. Sima Qian wrote that it held a hundred rivers of flowing mercury, kept moving by
  machinery, under a ceiling of the heavens, with crossbows set to fire on intruders. The clay army was not grey
  when made: it was painted in lacquer with cinnabar red, malachite green, azurite blue, pink skin and the man-made
  *Han purple*, which flaked away after burial. The Qin court's own colour was black.
- *The five poisons* (wudu: centipede, snake, scorpion, toad, gecko) are pictured on red paper and pierced with five
  needles to kill them; children have the character for king, the tiger, written on their brow in realgar wine. The
  *golden silkworm* gu, the most potent gu, gleams gold all over, and the house that keeps one is spotless.
- *The jiangshi*: stiff corpses in dark official robes, pale skin with a greenish mould, held still by a yellow paper
  talisman written in red.

**One image.** A city of black lacquer and grey tile at night, red lanterns on dark water, and a blue-green mist
rising up the streets with the gong, turning bile-yellow at the deadliest hour, while the player climbs onto the
roofs.

**Light and weather.** Night, with a sick, haloed moon in haze. The warm lights are the lanterns; the cold light is
the moon. The miasma is lit from inside, so when it rises the streets glow from below and the roofs above it stay
dark and safe. It is *neither smoke nor fog nor cloud*: thicker than fog, slow, layered, with a hard top edge the
player can read. Most hours it is the blue-green of the green-grass miasma. At the deadliest gong it is the
yellow-grass miasma, sick yellow, higher and faster, so the colour itself warns. After the gong the mist falls and
the streets are wet and shining.

**Palette.** Black lacquer (the Qin court's colour), grey roof tiles, grey clay, dark water, faded silk. The strong
colour is the miasma and venom, in its two strengths, blue-green and bile-yellow. **Accents**: cinnabar red (the
court's lacquer, seals, talismans, the five-poison papers) and mercury silver (the tomb's rivers). Both are the
story: cinnabar is mercury ore, the elixir that killed the emperor. **The alchemist's own colours**, seen only on him,
his vials and his fight, are the purple and gold of the elixir's "Essence and Flower". The last gu, if it is the
golden one, gleams gold.

**The clay army.** Grey clay with flakes of its first paint still on it (cinnabar, malachite, azurite, Han purple),
the only place those colours appear: what the emperor's world looked like before it was buried.

**Shapes.** Upturned eaves, round moon gates, lattice screens, walls inside walls, stairs up to roofs. The terracotta
army is rows, grids and ranks, the one place where the land is orderly.

**The floor from above.** Dark stone courtyards in grids, wet and reflecting lanterns; roofs of grey tile in strong
diagonal rows (the player spends time up there, so the roofs are floors too); the mist line, a soft edge creeping
over the floor and up the steps; mercury pooling silver in the tomb.

**What moves.** Lanterns swaying, the mist breathing, paper talismans fluttering on the dead, venom dripping, the
gong's ripple across the water, the clay army in lockstep.

**The element's effects.** Poison as a substance: mist that rolls and pools, bubbling puddles that leave stains,
venom spat in strings, clouds that burst and hang. Bright only in its blue-green or bile-yellow; never pure white.
Mercury is the land's second substance: silver beads that run together, roll downhill and reflect, in the tomb and
in the alchemist's fight.

**District by district.**
1. *The river town*: paper lanterns on black water, boats, bridges, the vessels drinking on the quays.
2. *The inner court*: walled courtyards, gates and rooftops with real height; the route goes up when the gong
   sounds.
3. *The tomb under the palace*: rivers of mercury, the clay army in ranks, the emperor's seat.

**Light by district** (added 2026-11-18; **Proposal**):
1. *The river town*: the warmest. Lanterns are doubled in the black water, and the moon hangs behind haze. The mist
   lies on the water first, so the quays go under before the streets do.
2. *The inner court*: colder and higher. Moonlight lies on the roof tiles, and the courtyards below become wells of
   mist lit from inside when the gong sounds. The roofs are the dark, safe level.
3. *The tomb*: no moon and no lanterns. The light is the miasma seeping down from the court, lying glowing in the low
   places, and the mercury rivers mirror it. The ceiling of painted heavens over the tomb (Sima Qian's) is seen mostly
   as a reflection in the mercury. The flakes of old paint on the clay army are the only warm colour.

**The Undying Emperor's seat** (**Proposal**): the emperor's seat at the head of the tomb, up a flight of steps over
the mercury, lit from below by its reflected glow. He is the one figure there in the court's black, with the yellow
talisman written in red on his brow.

**The first god under the land.** Since 2026-11-18 every land's murdered god is that element's first god (Decided,
`WORLD.md`); here a nod to Shennong, the divine farmer who tasted plants to tell medicine from poison (Proposal in
`WORLD.md`). **Proposal** for how it shows: a walled herb garden on a side path of the inner court, its terraces gone
over to poison plants, and a worn statue of a man with a leaf at his lips, whose chest was carved open to show the
plants working inside him (the old story gives him a transparent body). It is the one green-leafed place in the land.

**The god's room.** His laboratory: shelves of jars, the furnace, the five vials. When he splits, the lid slams and
the camera pulls up: the arena is inside a giant gu jar, its curved walls glazed and cracked, the five creatures on
and around them. The jar's inner wall is pasted with the red five-poison papers, each creature pierced with five
needles, as people did at the Double Fifth to kill the poisons, and the alchemist's own needles are the same ones.
The floor of the jar is the colour of whichever miasma the gong last called.

**The god's room, light** (added 2026-11-18; **Proposal**):
- **The laboratory**: lit by the furnace's fumes, which burn in the elixir's purple and gold, the alchemist's own
  colours, so his room is the only place they light. The five vials glow faintly on their shelf.
- **The jar**: when the lid slams the light from above is gone, and the arena is lit from below by the miasma on its
  floor. The glazed wall catches the glow up its curve, with its cracks dark and the red papers dark red against it.
  At each gong the light changes colour with the mist.

**The ride out.** The crane, white against the night, lifting out of the mist into clean air and the moon.

**The ride out, light** (added 2026-11-18; **Proposal**): as the crane climbs, the haze thins and the moon loses its
sick halo, sharp and clean for the first time in the land; below, the city is a sea of glowing mist with roofs standing
out of it.

**Avoid.** Generic neon-green slime. The mist is old jade and bile, not a highlighter. The jiangshi's look from
1980s Hong Kong film (Qing robes) is later than the first emperor; the undying emperor wears the black of his own
court, with the yellow talisman.
Also (added 2026-11-18; **Proposal**):
- a red that covers walls: cinnabar is small and flat (see the outfit below);
- many red lanterns reading as many accents;
- mercury that looks like water or ice: it is heavy, bright and beaded, and it rolls;
- the clay army's old paint brighter than flakes;
- a generic "Chinatown" of neon and later dynasties' gold dragons.

**The player's outfit** (approved 2026-11-18 as revision 6, `docs/GAME.md`): a vermilion robe with a gold collar,
cuffs, chest band and hem key, a black sash and trousers, a golden straw hat, blue piping, and the scarf in a deeper
rust so it separates from the robe by value. Against black lacquer, grey tile and the blue-green mist it stands out
strongly, since red is the green mist's complement; the yellow hour is its hardest ground.
- **It does compete with the cinnabar accent.** Both are in the same red family, and rule 6 wants cinnabar seen only a
  few times, on things the lore wants seen.
- **Proposal** to keep them apart:
  - cinnabar stays small and flat (seals, talismans, the five-poison papers, one lacquered post), never a large red
    surface beside the player's path, and a step darker and browner than the robe;
  - the "red lanterns" of the river town glow as warm amber-orange paper, with only their frames and tassels in
    cinnabar, so the many lanterns are not many reds.
  - Then the player is the largest and brightest red on screen, and the one that moves.
- **A second watch point**: the player's gold against the alchemist's purple and gold. His mass is purple and the
  player's red, with gold as trim on both, so they read apart in his fight.

**Mood frames to render** (added 2026-11-18). Game camera: 12 m from the player, pitched 55 degrees down, 40 degree
vertical field of view, the player at the centre, 1280 by 720. The player wears the Poison outfit
(`staging/blender/2026-11-18_outfits/outfit_poison.fbx`, revision 6), mid-stride unless stated. Render to
`staging/blender/<date>_moods/` under the names given.

1. **`poison_gong_green`**: the land's rule, at an ordinary hour.
   - *Camera*: game camera, on the inner court's roofs.
   - *In frame*:
     - the player on a grey-tile roof slope, the tiles in strong diagonal rows, a ridge and an upturned eave corner;
     - below, two walled courtyards filling with blue-green mist, its hard top edge partway up the walls and a moon
       gate half under it;
     - a cinnabar talisman on a door;
     - amber paper lanterns swaying on a gallery;
     - a neighbouring roof at the top of the frame, with the palace roofs beyond.
   - *Light*: night; a haloed moon at the upper right, cold; the lanterns warm; the mist lit from inside, so the
     courtyards glow from below and the roofs above them stay dark.
   - *Must prove*: the roofs read as a floor; the mist's top edge can be read; the red robe outranks every cinnabar
     mark and lantern on screen; the mist is old jade and not neon.
2. **`poison_gong_yellow`**: the deadliest hour.
   - *Camera*: the same camera and set as `poison_gong_green`.
   - *In frame*: the same, with the mist bile-yellow, higher (up to the eaves of the lower roofs) and streaming
     faster; the gong's ripple on a water tank in one courtyard.
   - *Light*: the yellow mist is now the main light, from below; the moon's halo has yellowed.
   - *Must prove*: the colour change alone warns, so that side by side with the green frame the hour can be told
     without the HUD; the red robe still reads against yellow, its hardest case.
3. **`poison_mercury_tomb`**: the tomb.
   - *Camera*: game camera; a second render wide, at 25 m, to show the ranks.
   - *In frame*:
     - a channel of flowing mercury crossing the frame, silver beads pooling at its edges;
     - clay soldiers in ranks on both banks, grey with flakes of cinnabar, malachite, azurite and Han purple;
     - a crossbow mechanism set in a wall;
     - the steps up to the emperor's seat at the top of the frame;
     - low miasma lying glowing in the hollows.
   - *Light*: no moon or lantern; the seeping miasma glows low and the mercury mirrors it, with the painted heavens of
     the ceiling seen in the mercury.
   - *Must prove*: mercury reads as liquid metal, not water or ice; the ranks read as the only orderly place; the
     flaked paint reads as the old world, faint but there.

---

## 8. Lightning and Wind (Japan): the rivals' mountain

*Rewritten 2026-11-18 for mood frames.*

**Decided (user)**: the castle and summit are reworked with wind, perhaps as a dry storm with no rain (2026-11-17).
The land itself was worked out with the user on 2026-11-18 (`WORLD.md`, "The land worked out"): the drowned shrine
shore with a great torii in the surf; two stairs of torii crossing and recrossing up the mountain; the rivals
streaking past on the stairs; the split summit, Narukami's castle and Kazehaya's monastery as two equal halves of one
hall, with a drum tower and an armour hall on one side and a court of wind bells and a hall of fans on the other; the
screen room where the two halves meet; phase 1 on the roof; the summit's top breaking away and phase 2 on clouds with
the dragon and the white tiger; the storm cloud as the ride out. The base look (dry storm night with no rain, cloud
lit from inside, indigo stone, lightning cyan-violet, wind as pale streaming dust and grass, shrine vermilion as the
accent, floors combed by the wind) is the 2026-11-17 proposal, kept as the base of this sheet. **Decided (user,
2026-11-18)**: the dry storm night with no rain; the first rain falls behind the storm cloud on the ride out. The
cloud lit from inside is decided too: lightning flickers inside the clouds and is the land's main light, so bolts
that reach the ground are rare and mean something. Everything else here is a **Proposal**.

**Decided (user, 2026-11-18)**: the mood of the frames is right (details to come when the land is built), and the
mountain must not be barren: it is full of life and places, as in Sekiro, with the trees showing the wind (see
`WORLD.md`, "the mountain is not barren").

**One image.** A dry storm on a holy mountain at night: two stairs of weathered gates zigzagging up through dry grass
that the wind combs flat, crossing and recrossing, a cyan-violet bolt and a pale streak of dust racing each other up
them, and at the top, inside a cloud lit from within, one great hall split down the middle.

**Light and weather.** Night, and dry: no rain falls anywhere in the land until the player leaves it. The storm sits
on the mountain and the light is the cloud's: lightning flickers inside it and the whole cloud glows violet-grey from
within, so the land is lit from above by a lamp that pulses. Bolts that reach the ground are rare and hard,
cyan-violet with a white core only at the very centre, and they light the world in two hard bands for a frame. The
wind never stops and always blows down the mountain toward the sea, so the climb is into it and every streak on the
floor points back the way the player came. The climb is a walk into the storm, as Water's is a walk toward one: the
light steps from far and dim on the shore, to the underside of the cloud on the upper stairs, to the inside of the
cloud on the summit, and out of the top of it into clear night for the last fight.

**Light and palette by area.**

| Area | Where the storm is | Key light | Palette step |
|---|---|---|---|
| The drowned shore | Far above, sitting on the summit | The cloud's glow from high up the mountain; the surf catches it; flashes light the torii | The darkest and coldest: blue-black sea, grey wind-rippled sand, wet indigo paving, the one whole vermilion torii |
| The lower stairs | The cloud base overhead, still high | Even violet-grey from above, pulsing; strikes on the scorched stair now and then | Grass grey-gold, char black, stripped silver-grey wood, vermilion only in scraps |
| The upper stairs | The cloud base comes down onto the stair | Light from all round, not from above; wisps of cloud stream across the floor; strikes more often | Grass bleached pale silver by the wind; stone bluer; the vermilion scraps fewer |
| The castle half | Inside the cloud, striking | Stroboscopic: dark, then a hard cyan-violet flash; no steady light | Indigo stone, black lacquer, char; cyan in the cracks |
| The monastery half | Inside the cloud, never striking | Steady and even: pale violet-grey glow from every side, no flashes | Bleached cypress, pale grey stone, faded paper; dust and grass streaks everywhere |
| The screen room | Shut out | Low and warm: one lamp and the storm's glow through high paper windows on old gold | Dull dark gold, ink, the screens' storm; the only warm room |
| The roof | Overhead, close enough to touch | The hardest strikes in the land; arcs between the roof's finials; the wind at its strongest | Indigo tile against bleached bark; the most cyan-violet of any area |
| The heavens | Below the player | From below for the first time: the cloud floor glows in pulses from the lightning inside it; cool moonlight from above | Cloud floor in mid violet-grey (never white); deep indigo night; the white tiger the lightest thing |
| The ride out | Behind, breaking | A grey dawn on the sea ahead | The palette lifts to grey-blue; the first rain falls behind |

**Palette.** Indigo stone (the built castle's), dry grass in grey-gold that bleaches toward silver the higher the
climb, ash, char black, wind-stripped grey wood, a blue-black sea. The strong colours are the two elements: lightning
cyan-violet; wind pale, the colour of the dust, grass and paper it carries, so it reads as moving air and never as a
colour of its own. **Accent**: shrine vermilion, used only on gates and on Kazehaya's war fan. The great torii in the
surf is the one whole vermilion thing in the land. On the stairs the vermilion survives only as paint left on ruined
gates: deep in the cracks of the scorched gates, in streaks on the lee side of the stripped ones. The small gates of the
crossing shrines are whole and freshly painted, because they are the rest places. The dragon and tiger emblem is carved
into stone and wood, never painted. The screen room's gold is dull, darkened gold leaf kept to that one room, so it
stays a warm surprise and not a second accent (see question 9).

**Shapes.** The rivals have two mirrored shape languages, and the land is built from both. Narukami's is angles and
zigzags: battered castle walls that slope out at the foot, stacked tile roofs, the drum tower, plates of armour, the
forks of a strike. Kazehaya's is long curves: swept cypress-bark roofs, streamers, bell frames, fans opening into arcs.
They share the torii, which is the land's mark, and the emblem of the dragon and the tiger. The two stairs zigzag up
the mountain and cross like a braid, so from high up the mountainside reads as a chain of X shapes.

**The floor from above.**
- **Combed by the wind** everywhere, so its direction is on the floor:
  - grass laid over downhill, with paler undersides showing where the last gust passed;
  - dust in long ripples behind every post and step;
  - torn paper talismans and dead leaves pinned against the windward side of things.
- **Written by lightning**: branching black scorch figures, the fern pattern a strike leaves, spread over stone and
  grass. Fresh ones crackle cyan-violet at the tips for a moment. They are scorch, never embers or flame.
- **The two stairs' treads** are worn hollow in the middle. On the scorched stair they are cracked and char-black; on
  the wind-split stair they are scoured pale and sanded smooth.
- **The monastery half** has the raked gravel of a dry garden, which the wind keeps raking into new lines.
- **The roofs** are floors too: grey tile in strong rows on the castle side, bark shingle combed into grooves on the
  monastery side.

**What moves.** Grass in waves, dust in streams, torn talismans and streamers, the shimenawa ropes on the gates, wind
bells, clouds racing low, the pulse of lightning inside the cloud, the surf round the torii's legs, Heike crabs
scuttling, drum skins shuddering in the castle half, and every so often the rivals themselves streaking past.

**The element's effects.** Two elements, and two racers whose paths must read on the floor.
- **Lightning**: jagged forks with a violet fringe, crackling arcs that run along metal and wet edges, scorch figures
  left behind. White only at the core of a bolt. Narukami's race path is a jagged cyan line across the floor (Decided).
- **Wind**: pale streaks of dust, grass and leaves. A gust front is a wall of dust rolling across the floor with the
  grass flattening just ahead of it. Wind blades are thin pale crescents trailing dust. Kazehaya's race path is a pale
  streak (Decided), and her fan's gust opens as an arc.
- **A lane** is a strip of flattened grass and dust (wind) or a branching scorch line (lightning). **A ring** is a ring
  of thrown-up dust or a ring of forks.
- **When their paths cross** and the slower crashes into the faster (Decided), the crash is a burst of cyan-violet
  sparks inside a puff of dust.
- **Both together** (the ride out, and the land's emblem): lightning carried inside wind-driven cloud, sparks streaming
  along the gust.

**Area by area.**
1. *The drowned shore.* The drowned shrine of the two murdered first gods (Decided).
   - The great torii stands in the surf, a nod to Itsukushima (Decided), with pale foam round its legs. It is the
     first gate of the stair.
   - Behind it the wind-scoured shrine town is falling into the sea (Decided): roofs tilted into the water, stone
     lanterns toppled in the surf, a covered walkway on piles half under the waves.
   - The beach is grey sand rippled by the wind, with wet paving running out into the sea. It is not Water's wet black
     sand, and there is no turquoise.
   - The Heike crabs' tracks run across the sand from the ruined shrine; the ship-ghosts rise pale in the surf.
   - The wreck on the side path that holds the plain Katana (Proposal in `WORLD.md`) lies on its side at the tide
     line, its mast pointing up the mountain.
2. *The two stairs* (Decided: two stairs of torii crossing up the mountain; the scorched and wind-split gates are the
   proposal in `WORLD.md`).
   - **The scorched stair (Narukami's)**: gates charred black and split by strikes, cyan crackle still running in the
     cracks of the newest, lanterns burst, treads cracked, scorch figures across the steps.
   - **The wind-split stair (Kazehaya's)**: gates leaning downwind, crossbeams snapped and hanging, paint scoured off to
     silver-grey wood, ropes and paper streamers streaming uphill-to-downhill.
   - **The crossings.** Each crossing has a small shrine where the player rests or changes stairs (Decided). It is
     the one place on the mountain where the wind drops: the grass stands upright, the dust settles, the small gate
     is whole and red, and a lantern burns still. The calm itself says "rest".
   - **The rivals streaking past** (Decided). Warning: the cloud above brightens and a rising roar comes down the
     stair. Then a cyan line races up the scorched stair and a pale streak up the other, swapping the lead at each
     crossing. They leave a fresh scorch lane and a lane of flattened grass. Shelter under a gate reads because each
     gate stands on a pale stone sill, and the strip of floor under its beam stays untouched.
   - The messengers race each other up the gates (the raiju a crackling beast, the crow-tengu black runners). The
     fox-masked shrine guards stand as statues by the gates, their white masks the palest things on the stair, until
     they wake.
   - The hermit's cave on the side path with the plain War Fan (Proposal in `WORLD.md`) is a calm hollow in the rock,
     out of the wind: its floor is the only uncombed ground on the mountain.
3. *The split summit.* One great hall in two mirrored halves, equal, so neither seems the lesser (Decided). Same
   footprint, same height, same weight on screen; the seam runs straight down the middle of the hall.
   - **The castle half (Narukami's)**, with its drum tower and armour hall (Decided): indigo stone courtyards inside
     sloped walls; the drum tower's great drums with their skins scorched; the armour hall's suits of black lacquered
     armour on stands, struck and split, crackle in their joints. Lightning falls here often. The Last Retainer, the
     castle half's elite (Decided), waits in the armour hall, the one figure among the empty suits that moves.
   - **The monastery half (Kazehaya's)**, with its court of wind bells and hall of fans (Decided): swept cypress-bark
     roofs; the court's frames hung with dark iron bells and paper strips, ringing all the time, over a raked gravel
     garden; the hall's folding fans pinned open across its walls and screens, all flapping. No strike ever falls
     here. The Hermit of the Pass, the monastery half's elite (Decided), waits in the court of bells.
4. *The screen room* (Decided: the two first gods painted as one storm on the screens, the last quiet room before the
   roof). The thunder and the wind face each other across a field of old gold, a nod to the composition of the famous
   folding screens of the wind and thunder gods, painted here in the land's own colours (the thunder in cyan-violet
   ink, the wind in the pale of dust) and not copied. The doors are shut, so the room is still: no streak on the floor,
   nothing moving but the lamp flame. Because the camera sees floors, the screens stand as a low ring of folding
   screens opened in a zigzag (echoing the stairs) round the centre of the room, angled so their panels face up toward
   the camera, and the polished dark floor reflects their gold. The dragon and the tiger are on the two end panels.

**The god's room.**
- **The roof, phase 1** (Decided). A broad flat top walk along the ridge of the split hall, where the ridge would be.
  The two slopes fall away on either side into cloud: grey tile on the castle side, bark shingle on the monastery
  side, the seam running along the walk. The cloud ceiling is right overhead, strikes are the hardest in the land,
  arcs jump between the roof's finials, and the wind is strongest. Both race paths (Decided) must read across the tile
  rows, so the walk's own floor is plain stone flags in large slabs.
- **The break** (Decided). When the beasts come the summit's top breaks away between the two transformations; the
  walk tears off with the player on it and rises through the cloud.
- **The heavens, phase 2** (Decided: the player walks on clouds; the dragon coils through the cloud layers and
  strikes through holes it bursts in the floor; the tiger bounds from cloud to cloud and her roar shoves whole clouds
  sideways).
  - Above the storm the night is clear, with a cool moon.
  - The cloud tops are dense and sculpted, cel-shaded lobes with ink outlines and darker rims, so they read as ground
    with edges. The gaps between them show the lit storm deep below and the sea under that.
  - The floor glows from below in pulses from the lightning inside it, the first light from below in the land.
  - Holes the dragon bursts show the storm through them, ringed with torn cloud.
  - Pieces of the broken summit roof drift among the clouds.
  - The dragon is a dark serpent with cyan-violet lightning running along its body.
  - The white tiger is the lightest thing on screen: white with black ink stripes, pale dust streaming off her as she
    bounds. The cloud floor stays mid violet-grey in value, never white, so she never sinks into it. She is the one
    place in the land where a large white mass is allowed, because she is the white tiger (Decided).

**The ride out.** The storm cloud, a mix of both (Decided). The player stands on a small dense cloud the wind carries
off the mountain, lightning running inside it, sparks streaming along the gust. Behind, as the cloud carries the
player out over the sea, the storm over the mountain breaks into rain: the first rain in the land. The dry storm ends
when the rivals' race does, as Water's storm breaks to sun and Fire's noon sets (see question 8). Ahead, a grey dawn
on the sea.

**The player's outfit** (the ronin, approved 2026-11-18, `docs/GAME.md`): a deep blue jacket over a bandaged chest,
dark violet trousers, oatmeal wraps, the straw hat on his back and a bright rust scarf. The jacket's blue is close in
hue to the indigo stone, so the ronin carries on value instead. The pale wraps and straw hat stand out against dark
stone, and the scarf is the warmest thing on screen bar the vermilion gates. The scarf and the gates share a red
family, but the gates are rare, big and still, while the scarf is small and always moving. Lightning gives him a cyan
rim. Check in frames 2 and 3 that the jacket does not vanish into the stone between flashes.

**Avoid.**
- An endless tunnel of red gates (Fushimi Inari as a postcard): the gates are ruined and the vermilion rare.
- Rain, puddles and wet shine anywhere but the surf: this storm is dry.
- Lightning that reads as light and not as electricity (the Fire land's lesson, rule 5): every bolt forks, crackles and
  leaves a scorch.
- Wind drawn as cartoon swoosh lines with nothing in them: wind always carries dust, grass, leaves or paper.
- One half of the summit looking lesser than the other (Decided).
- A copy of the real wind and thunder god screens: the nod is the composition, not the painting.
- Water's land on the shore: no black sand, no turquoise, no palms.
- Cherry blossom, a red sun and other postcard Japan.
- The white tiger lost against white cloud.

**Mood frames to render.** Game camera: 12 m from the player, pitched 55 degrees down, 40 degree vertical field of
view, the player at the centre, 1280 by 720. The player wears the ronin
(`staging/blender/2026-11-18_outfits/outfit_ronin.fbx`), mid-stride unless stated. Render to
`staging/blender/<date>_moods/` under the names given.

1. **`lw_torii_surf`**: the land in one image.
   - *Camera*: wide, from the sea about 80 m out and 15 m up, looking inland and up the mountain.
   - *In frame*:
     - in the middle ground, the great vermilion torii standing in the surf, foam round its legs;
     - behind it, the drowned shrine town: roofs tilted into the water, toppled stone lanterns, a half-drowned walkway
       on piles, grey wind-rippled sand;
     - behind that, the mountain rising into the cloud, with the two stairs of gates zigzagging and crossing (three
       crossings visible, each with its small shrine), one stair char-dark and the other pale;
     - at the top, the split hall's silhouette just under the cloud base;
     - the player a small figure on the sand.
   - *Light*: night with no moon. The key light is the cloud's violet glow from high up the mountain. One bolt is
     striking the summit and the torii is flash-lit by it. The sea is blue-black.
   - *Must prove*: the landmark is at the top; the two stairs read as two; vermilion appears once, whole; the frame is
     dry, with no wet shine but the surf; it could never be mistaken for Water's shore.
2. **`lw_rivals_pass`**: the race felt on the stairs.
   - *Camera*: game camera, on the scorched stair just below a crossing.
   - *In frame*:
     - the player sheltering under a charred torii on its pale stone sill;
     - the stair's treads climbing diagonally across the frame, cracked and char-black;
     - the wind-split stair crossing it at the top of the frame at the crossing shrine: a small roofed shrine, a small
       whole vermilion gate, a still lantern, the grass standing upright round it;
     - dry grass on both sides combed flat downhill;
     - beside the player, a jagged cyan-violet path just struck up the steps, with fresh branching scorch on the treads;
     - on the other stair, a pale streak of dust and torn grass racing up;
     - torn paper streamers, and a fox-masked guard statue by a gate.
   - *Light*: under the cloud base, violet-grey from above. Render it twice: in the strike's flash, and one second
     after in the pulsing dark.
   - *Must prove*: the two stairs read apart from above by their gates and treads alone; both racers' paths read on
     the floor; the wind's direction is on the floor; the shelter under the gate reads; the crossing reads as calm;
     the ronin reads against indigo stone in both renders.
3. **`lw_split_summit`**: two equal halves.
   - *Camera*: game camera, standing on the seam in the hall's central court, with the screen room's doors at the top
     of the frame.
   - *In frame*:
     - the seam running top to bottom through the centre of the frame;
     - on the left, the castle half: an indigo stone courtyard with scorch figures, the foot of the drum tower, one
       struck suit of armour on its stand;
     - on the right, the monastery half: a raked gravel court with its lines being re-raked by the wind, a bell frame
       hung with dark iron wind bells and streaming paper strips, a cypress-bark eave;
     - at the top, the screen room's open doors, the dull gold of the zigzag screens and its still floor.
   - *Light*: on the left, the dark between flashes with the last strike's rim on the edges; on the right, a steady
     pale violet-grey glow; in the screen room, warm and dim.
   - *Must prove*: the halves carry equal weight and are told apart by light, floor and shape; the screen room reads
     as the one warm, still place; the gold stays dull and does not become a second accent.
4. **`lw_heavens`**: the fight above the storm.
   - *Camera*: game camera pulled back to 18 m for the beasts (an assumption: how the camera frames large bosses is
     Open in `docs/GAME.md`), plus one render at the standard 12 m.
   - *In frame*:
     - the player on a broad cloud island with an inked rim, and a gap to the next island showing the lit storm deep
       below;
     - near the player, a hole burst in the floor with the dragon's coil arching up out of it, a dark serpent with
       cyan-violet lightning along it;
     - the white tiger mid-bound on the next cloud, dust streaming off her;
     - the two race paths converging on the player, a jagged cyan line and a pale streak;
     - a piece of the broken summit roof drifting at the edge of the frame.
   - *Light*: from below, the floor glowing in pulses from the lightning inside it; a cool moon key from above.
   - *Must prove*: the cloud reads as ground with edges; the tiger is the lightest mass and does not vanish into the
     cloud; the dragon reads against it; light from below makes this place unlike every other area; both paths and
     the player read at 18 m.

---

## 9. Earth and Life (Celtic): the stones and the grove

*Rewritten 2026-11-18 for mood frames.*

**Decided (user, 2026-11-18)**: the mood of the frames is right. To keep it apart from Fire's
sandstone and the champion's dust, the land is a cooler grey-green; ochre is kept to the cracks and Earth's
worked stone, not the ground.

**Decided (user, 2026-11-17, night; in `WORLD.md`)**:
- the areas: the border bog crossed by an oak plank road; Earth's branch with the King's Men's hill and the Giant's
  Dance, Brannoch's seat; Life's branch with the wildwood village and the sacred grove, Carnwyn's seat; the barrows;
  the Wild Hunt's plain; the meeting ground;
- the gods' brawl at the meeting ground is seen only from the arenas of the two seats and on the walk to the meeting
  ground;
- the Hunt cannot pass stone;
- the ride out is the solstice passage.

The look itself the user deferred until the review of the two duo lands (section 11, question 6), so every line of
look here is a **Proposal**. Its base is the sketch of 2026-11-17, kept: grey dawn mist on a moor, a low sun, earth
ochre and living green, gold torcs and spirals.

**One image.** A grey-green moor under dawn mist and a sun that never quite rises; an oak plank road across a black
bog; a ring of standing stones on a hill to one side and a great oak over a forest to the other.

**Light and weather.** A dawn that never finishes, and the story says why: the sun clears the horizon only when both
gods fall. The sun is a low gold disc behind mist, never more than a hand's width above the horizon. It stands in the
same quarter everywhere in the land, the quarter the meeting ground's barrow faces, so the shadows are always long and
always point the same way, a compass the player learns without being told. The mist lies in layers: thick in hollows,
thin on hilltops, crisp near the player and grey in the distance. Dew on everything. Earth's side is drier and
windswept, its mist pushed down into the valleys, its light raking and clear. Life's side is still and damp, under a
canopy, with the mist caught in shafts of light.

**Light and palette by area.**

| Area | Key light | Mist | Palette step |
|---|---|---|---|
| The border bog | The sun a pale disc straight ahead, flat and low | The thickest in the land, close round the player | The lowest saturation: grey-green, black water, pale wet oak; both elements only hinted |
| Earth's branch | The clearest in the land: low sun raking across the hills, long hard shadows | Pushed down into the valleys; the hilltops clear | Ochre, rust and lichen grey, the green drained out of the grass |
| Life's branch | Dim under the canopy, with gold shafts falling through it | Hanging in the shafts with spores and seeds | Living green, the brightest colour in the land; moss on everything; the grey drained out of the stone |
| The barrows | Dark; light only down the passage behind and at the rest place | None inside | Grey stone and black; the gold of the carved spirals where light catches them |
| The Wild Hunt's plain | Open and windy; when the horn sounds the light dims and the mist runs toward the player | Lifting, streaming in the wind | Grey-green heather, the standing stones' gold spirals the only warm marks |
| The meeting ground | The sun right on the horizon behind the barrow mound: a gold rim on everything that faces it | Burnt thin by the fight | Half ochre stone, half green growth, meeting along a double spiral |
| The ride out | The first full sunrise: warm direct light, shadows shortening | Burning off in gold | The whole land lifts a step: greener green, warmer stone |

**Palette.** The world is grey-green moor, brown peat, lichen-spotted grey stone, black bog water and weathered oak.
It is a duo land, so it has two element colours. Earth is ochre and rust: stone dust, cracks, freshly broken rock.
Life is a living green brighter than anything else on screen: moss, leaves and shoots that overgrow. **Accent**: gold,
always as worked metal and never as paint on a surface: torcs, brooches, Carnwyn's golden scythe, the druids' sickle,
and the spirals of the safe stones and the barrows where light catches their grooves. The low sun is gold too, so its
light is kept pale and the mist grey, and worked gold stays the richest yellow in the frame.

**Shapes.** Circles and spirals: stone rings, barrows, triskeles, round houses, the double spiral of the meeting
ground. On Earth's side the shapes are upright and straight: standing stones, trilithons, cairns, furrows ploughed
straight across the ground. On Life's side they are tangled and round: roots, round houses with trees grown through
them, wicker. "Stone remembers, wood forgets" (Decided as the land's core rule) is also its look: on Earth's side every
break stays broken and every scar stays, and on Life's side cut paths and broken things are grown over again at each
rest. The land uses spirals and circles, not knotwork, which belongs to Ice (rule 9).

**The floor from above.**
- **The bog**: black pools, peat cuttings, bog cotton, the oak plank road on its pegs, sunk in places.
- **The moor**: heather and peat.
- **Earth's side**:
  - ochre cracks spreading out from every stone;
  - the gouged tracks of hauled stones;
  - the boar's litter's rooted-up furrows;
  - the dead set into stone: grey shapes of bodies in the floor (the dead take root, Decided).
- **Life's side**:
  - roots breaking through paving and floors;
  - moss carpets and leaf litter;
  - flowers in hoofprints;
  - the dead sprouting thorns in low thorny mounds.
- **The stone circles** themselves, read from above as rings.

**What moves.** Mist in layers, bog cotton and heather in the wind, rooks lifting off the stones, the haulers' ropes
and rollers, the forest visibly growing (vines creeping, leaves unfolding, a shoot pushing up), spores and seeds
drifting in the grove's shafts. The King's Men move only when unwatched, so in a still frame they are just stones. On
the walk to the meeting ground and from the two seats, the far brawl: dust plumes and a heaving canopy on the skyline.

**The element's effects.**
- **Earth**: stone that cracks, heaves and bursts, ochre dust clouds, rubble that stays where it fell. A lane is a
  crack line opening across the floor; a ring is a ring of heaved stones.
- **Life**: vines and roots growing in fast, flowers that open and close, spores. A lane is a line of thorns
  sprouting; a ring is a ring of flowers turning to thorns.
- The planted stones of Brannoch's sword and the blooms and thickets of Carnwyn's scythe (Decided as the weapons)
  are the same substances, so the player's own weapons draw on the floor in the land's two languages.

**Area by area.**
1. *The border bog* (Decided), where the player arrives.
   - The oak plank road (Decided) runs on pegs across black pools, sunk in places, its wet planks the brightest line
     in the frame and the path the eye follows.
   - Bones lie in the peat.
   - From here the two seats are the landmarks (Proposal in `WORLD.md`): the stone circle on its hill to one side,
     the grove's great oak to the other.
   - The meeting ground is hidden behind a rise; nothing of the brawl shows from the bog (Decided).
   - The blackbird that starts the Oldest Animals thread sits on a sunk post.
2. *Earth's branch* (Decided).
   - **The King's Men's hill**: an army of stone soldiers frozen mid-march in ranks across the hillside, a nod to the
     Rollright Stones. They are lichen-spotted, their long shadows laid parallel across the slope. The countless stand
     as cairns, the haulers drag standing stones on rollers, and the boar's litter roots the ground into furrows.
   - **The Giant's Dance**, Brannoch's seat (Decided; a nod to Stonehenge): a great ring of trilithons on the
     hilltop. The camera sees the lintels' tops, which carry lichen, rook perches and the giant's tool marks. The empty
     seat is a great stone slab chair facing the meeting ground. The Bargain sits on his fallen stone with his axe
     across his knees. Older stones lie flat in the heather round the ring, worn by weather; from the hill they make
     the profile of a sleeping woman, the Cailleach whose stones the giant stood on end. The eagle of the Oldest
     Animals perches on a lintel.
   - From the Dance the player sees, far off, the forest surging at the meeting ground (Decided).
3. *Life's branch* (Decided).
   - **The wildwood village**: round houses with trees grown up through their roofs and walls, paths that close
     behind the player at each rest, the stag of the Oldest Animals, Green Men rooted as saplings. In the mute
     reborn's district stands their cauldron, a great bronze bowl furred with moss (the Dagda nod), the one dark metal
     shape in a green place.
   - **The sacred grove**, Carnwyn's seat (Decided): the great oak with mistletoe in its crown. The empty seat is grown
     from its roots, with a crown of antler-like branches above it. Wicker cages hang in the grove. Lamps stand dark
     until freed captives light them (Proposal in `WORLD.md`), so the grove grows warmer as the Wicker Man's people
     are freed.
   - From the grove the player sees the stone side breaking the ground at the meeting ground (Decided).
4. *The barrows* (Decided: under the hills, linking the land; each trip under ages Life's side by a year).
   - Corbelled stone passages and round chambers, spirals carved in the kerb and the walls (the Newgrange nod), the
     owl of the Oldest Animals in a niche.
   - Coming out frames the year that passed: the sapling by the mouth is now a young tree, a path is shut by a
     thicket, roots bridge a gap that was open.
5. *The Wild Hunt's plain* (Decided). Open moor between the branches and the meeting ground, scattered with standing
   stones.
   - When the horn sounds the light dims and the mist streams across the plain toward the player. The Hunt comes as a
     wave: Carnwyn's herd of stags with thicket antlers and the grey dead riding among them, the heather flattened and
     torn green growth whipped along under them.
   - The stones it cannot pass (Decided) must read from above. Each carries a gold-lit spiral and stands in a ring of
     untouched heather that the Hunt's wave parts round.
6. *The walk to the meeting ground* (Decided: the brawl is seen on it). The last stretch from the plain climbs a rise.
   From the top, the meeting ground lies ahead: half the far sky thick with ochre dust, the other half with heaving
   canopy, the ground shuddering underfoot. Its barrow gate has two seals, a stone and a thorn (Proposal in
   `WORLD.md`), and both are broken.

**The god's room.** The meeting ground (Decided). A broad shallow bowl, its floor carved with a great double spiral
(the motif proposed in `WORLD.md`). Through phase 1 one half is heaved stone and bare furrows and the other tangled
growth, and the line between them shifts as the gods fight. The salmon's pool and the oath stone lie beside it. At the
bowl's edge is the barrow mound whose passage faces the sun. The sun sits right on the horizon behind that mound, so
everything facing it has a gold rim and long shadows run across the bowl toward the player. In phase 2 the camera
pulls back (Proposal in `WORLD.md`). The boar ploughs stone furrows and knocks over the planted trees; the stag sprouts
trees and flowers where it passes. So the floor is redrawn the whole time in ochre and green. The end state is the
arena left half forest and half stone, with the two men lying side by side.

**The ride out.** The solstice passage (Decided). When both gods fall the sun clears the horizon at last, and its
first ray runs straight across the bowl and down the passage of the barrow at its edge, as at Newgrange. The mist burns
off in gold for the first time, the shadows shorten, and the whole land lifts one step in colour. The player walks into
the lit passage toward the triple spiral at its end. It answers Fire's land: there the sun set for the first time in
ages, here it rises.

**The player's outfit** (approved 2026-11-18, `docs/GAME.md`): a green check tunic with ochre lines, plain blue trews,
a long rust brat cloak to the calves pinned with a gold ring brooch, and a gold torc. The green sits close to the moor
and close to Life's green, which is meant to be the brightest thing in the land. From above, the long rust cloak
carries the figure, as the outfit notes found on a moor-green ground. Keep the check's green a muted olive, darker than
the moor and far duller than Life's living green, so the player reads as a dark shape on Life's side and a green one on
Earth's ochre. The torc is the land's own gold on the player, small enough not to spend the accent. Check in frames 2
and 3.

**Avoid.**
- Postcard and new-age Celtic: a Stonehenge sunset, glowing blue runes, shamrocks, clan tartan (the outfit's check is
  Iron Age check, not tartan).
- Knotwork, which belongs to Ice: here it is spirals and circles.
- Green on green in the grove. Life's green must be the only saturated green, with the moor and the outfit duller.
- Mist flattening every frame into one grey: the near ground stays crisp and the layers step back.
- Ice's land: no snow, no pines, no blue-grey; the moor is wet green-grey.
- A spectral ghost-blue Wild Hunt: the Hunt is grey dead and green-thicket stags.
- The brawl visible from anywhere but the seats and the walk (Decided).

**Mood frames to render.** Game camera: 12 m from the player, pitched 55 degrees down, 40 degree vertical field of
view, the player at the centre, 1280 by 720. The player wears the Earth and Life outfit
(`staging/blender/2026-11-18_outfits/outfit_celtic.fbx`), mid-stride unless stated. Render to
`staging/blender/<date>_moods/` under the names given.

1. **`el_plank_road`**: the land in one image.
   - *Camera*: wide, from 40 m behind and 20 m above the player on the road, looking across the bog toward the low sun.
   - *In frame*:
     - the oak plank road on its pegs winding away across black pools, sunk in two places;
     - bog cotton, heather, bones half out of the peat;
     - the player small on the road;
     - on the left, the stone circle on its hill; on the right, the forest under the great oak;
     - between them, a rise hiding the meeting ground, with nothing of the brawl showing.
   - *Light*: the sun a pale gold disc just above the horizon ahead, behind mist. The wet planks catch it. The stones
     and the oak stand as soft silhouettes, and the mist is in at least three layers.
   - *Must prove*: both landmarks read; the land is muted grey-green with ochre and green only hinted; the mist is
     layered, not a grey wash; it reads as neither Ice nor Water.
2. **`el_giants_dance`**: Earth's side.
   - *Camera*: game camera, inside the stone circle near its edge.
   - *In frame*:
     - the feet of two trilithons and the tops of their lintels, with lichen, a rook and tool marks;
     - the empty stone seat;
     - the Bargain's fallen stone with his axe lying across it;
     - ochre cracks spreading out from a short stone newly planted by a blow;
     - wind-pressed heather;
     - through a gap at the top of the frame, far below, the meeting ground's forest surging under a plume of ochre
       dust.
   - *Light*: the clearest in the land; low raking sun from one side, the stones' long shadows laid across the floor
     in bars; mist lying in the valley below.
   - *Must prove*: Earth's side reads ochre and grey with the green drained out; the shadow bars give form without
     hiding the player; the circle reads as a circle from above; the far brawl reads at that distance; the green check
     and rust cloak read on ochre.
3. **`el_sacred_grove`**: Life's side.
   - *Camera*: game camera, under the edge of the great oak's crown.
   - *In frame*:
     - the oak's roots breaking up through old paving;
     - Carnwyn's empty seat grown from the roots;
     - two hanging wicker cages, one with a figure inside;
     - one lamp just lit, one still dark;
     - moss, ferns, flowers open in a row of hoofprints;
     - a Green Man rooted as a sapling;
     - the edge of the crown at the top of the frame with a hanging ball of mistletoe.
   - *Light*: dim under the canopy; two gold shafts of light with mist and spores in them; the lit lamp warm.
   - *Must prove*: Life's green outranks everything on screen; the frame is dim but readable; the player's muted green
     does not vanish (the rust cloak reads); round shapes, no knotwork.
4. **`el_first_sunrise`**: the god's room and the ride out.
   - *Camera*: game camera pulled back to 18 m (the phase 2 framing; the boss camera is Open in `docs/GAME.md`).
   - *In frame*:
     - the meeting ground after the fight: the bowl half heaved stone and furrows, half young forest;
     - the double spiral carved across the floor and over the seam;
     - the two men lying side by side;
     - the barrow mound at the top edge of the frame with its passage mouth;
     - the player standing between the men and the mound.
   - *Light*: render it twice. First, the moment before: the sun on the horizon behind the mound, the dawn that never
     finishes. Then the moment after: the sun just clear of the horizon, its first ray running in a gold line across
     the bowl and into the passage, the mist burning off, shadows shorter.
   - *Must prove*: the change of light is felt between the two renders; it reads as the opposite of Fire's sunset;
     both halves of the arena read from above.

---

## 10. The champion's seat under the eclipse (India)

*Rewritten 2026-11-18 for mood frames.*

**Decided (user, 2026-11-17, evening)**: the frozen world is grey and uncoloured, and the bubble round the player
brings colour back. The final area is themed with each seat in its own element's colour, and then the final fight.

**Decided (user, in `WORLD.md`)**:
- a single path through a massive battlefield of a great war frozen in time, where enemies from each land unfreeze
  close to the path and attack;
- at the end, a council where each seat is themed to one element, and the seats of Light and Dark in front, vacant or
  destroyed by the breaking;
- the final fight in the space between the seats, where the godly fight turns the arena into a separate domain: a
  split arena, half the floor in blazing day and half in night;
- the user is wary of imagery too close to Mount Olympus.

Everything else here is a **Proposal**.

**One image.** A vast battlefield stopped in time, every soldier and arrow grey and motionless, and a small circle of
colour and motion around the player, in which the war comes back to life.

**Light and weather.** An eclipse: the sun a black disc with a white corona, the light dim and strange, shadows sharp
but faint. Nothing moves in the world, not even dust; the dust hangs.

**The eclipse and the fight** (found 2026-11-18; see question 13). The land is under an eclipse, and `WORLD.md` also
has the champion make the eclipse at the end of phase 1 by hurling the chakram at the sun. **Decided (user,
2026-11-18)**: the champion causes the eclipse; the sun was almost eclipsed, but time stopped before it could be.
As written: the war froze in the last moment before totality. Over the path and the council the sky holds the black disc with
a thin hooked crescent of sun still burning on one side and the corona faint round the rest. The crescent gives the
only direct light, which is why shadows are sharp but faint. Under anything with gaps in it (a banner's fringe, a
hanging volley of arrows) the light falls in small crescents on the floor, as it does in a real partial eclipse. At the
phase change the chakram covers the last crescent. Totality comes with the beasts: the full corona, and the horizon
glowing pale all round as it does at totality. When the player cracks the chakram in phase 3, the sun comes back.

**Palette.** Outside the thaw everything is drained to a warm grey, like a faded photograph. Inside the thaw colour
returns: saffron and vermilion banners, marigold garlands, brass, painted chariots. In the fight, Light is gold-white
and Dark is violet-black, the only place in the game where the two meet.

**Shapes.** Mandalas and circles, stepped temple towers, chariot wheels, the council's ring of twelve seats. In the
grey, colour cannot say which land a frozen soldier came from, so the silhouettes do it (rule 9): the square shields
of the wicks and the gilded, the storm soldiers' wide iron hats, the pack running on all fours, the clay army's
ranks.

**The floor from above.** Trampled earth with frozen arrows hanging above it, each casting a shadow; dropped weapons
from every land; the council floor as a mandala, with the gnomon's shadow sweeping it as the obelisk's did in Fire's
court.

**What moves.** Only what the thaw touches. That stillness is the land's identity, and the one exception to rule 7.

**The thaw.** The thaw bubble (Proposal in `WORLD.md`) is the land's main readable shape, as the tide line is Water's.
- **Size**: a circle about 5 m in radius round the player, so a game-camera frame always shows frozen grey world all
  round it.
- **Edge**: a band about a metre wide, not a line. Colour rises out of the grey across it and motion stutters in it:
  dust half-falling, a banner's edge twitching, an arrow trembling in the air.
- **Inside**: full colour and motion. The hanging dust falls, the frozen arrows start flying again (the hazard lanes
  of `WORLD.md`), and soldiers caught in it come back to life.
- **Readable thaws** (Proposal in `WORLD.md`): a figure about to wake shows it at the edge first, a twitching hand or
  dust starting to fall off its shoulders.
- **The ground behind** the player re-freezes as the bubble passes: arrows stop mid-flight again and the colour drains
  back out.

**Area by area.**
1. *The battlefield path* (Decided: a single path through the frozen war). The path runs through sections of the
   battlefield themed to each land, a recap of the climb in miniature (Proposal in `WORLD.md`): wicks and gilded from
   Fire, storm soldiers, the reef-grown, the brittle and the pack, the clay army in its ranks. When the thaw passes
   over a section, that land's element comes back on it for a moment, the flame on a wick, frost on a brittle, the
   mist round a vessel, so the path's edges flash each land's colour in turn and drain again.
2. *The bed of arrows.* Beside the path, the lore figure on a bed of arrows, a nod to Bhishma (Proposal in
   `WORLD.md`): the only thing in the land fully awake. He stays grey like the frozen world, because colour comes
   only with the elements and he carries none, but he moves: his chest rises, his eyes follow the player, his beard
   stirs in a wind no one else feels. So he is grey and moving, the one figure that is both. When the player's thaw
   reaches him his colour returns: white cloth and a single marigold garland. The arrows under him are the war's own,
   and they stay frozen even in the thaw, because he holds them there. It is the land's one quiet stop.
3. *The council* (Decided: each seat in its element's colour; Light's and Dark's in front, broken).
   - **The ring**: a mandala of twelve seats in a circle, each facing outward to its land like a temple's ring of
     planetary shrines (Proposal in `WORLD.md`). Each seat is a small stepped shrine-tower, not a throne or a column,
     to stay clear of Olympus.
   - **The floor**: a mandala inlaid in stone. Time's seat is the gnomon at the centre (Proposal in `WORLD.md`), and
     its shadow, cast by the crescent sun, lies across the floor like a clock hand.
   - **The champion** sits between the two ruined seats of Light and Dark, waiting (Proposal in `WORLD.md`).

**The god's room.** The council of twelve seats, and the first place in the land with colour of its own, outside the
thaw: each seat in its element's colour and substance, the same colour the player has seen as that element all climb
(fire's seat smoulders orange, water's runs turquoise, ice's frosts cyan-white, poison's breathes its mist, and so on).
The seats of Light and Dark in front are the exception, broken and burnt to grey by the breaking. **Proposal**: the
seats of elements the player has taken glow brighter, and those they consumed are dull. In phase two, the split
day-and-night domain.
- **Phase 1** under the crescent: the seats' own light round the ring, the dim eclipse light on the floor, and the
  gnomon's shadow.
- **Phase 2, the split domain** (Decided: half the floor in blazing day and half in night). The line between them
  sweeps round like a clock hand (Proposal in `WORLD.md`), along the gnomon's shadow, so the first god's room and the
  last rhyme.
  - **The day half**: a hard gold-white sun overhead, short black shadows, the mandala floor warm sandstone with
    saffron inlay. The eagle's light attacks read gold-white in it, and the eagle's shadow sweeps it on fly-overs.
  - **The night half**: violet-black under stars, the mandala's inlay faintly lit, the bull's dark pools blacker than
    the floor round them.
  - **The line**: a hard edge on the floor, not a gradient, so the cross rule's shelter is always readable.
  - **The ring of seats** stays visible past both halves, each seat still in its colour.
- **Phase 3**: the chakram held in front of the sun by the eagle, a dark ring with a burning rim. When it cracks and
  falls, the sun returns as full daylight over the council for the first time. The frozen war beyond the council
  stays grey: the freeze is the sponsor's, and only beating the sponsor could lift it (Proposal in `WORLD.md`).

**The ride out.** None (Decided). The land ends with the fight and the champion's last words.

**The player's outfit** (approved 2026-11-18, `docs/GAME.md`): a cream dhoti with wide indigo borders, brass
armlets and belt, and a rust angavastram over the shoulders. The player is always inside the thaw, so always in
colour. Cream and rust read clearly against the warm grey of the frozen war, as the outfit notes found. Inside the
thaw the rust sits near the saffron and vermilion banners and the marigolds; the player stays apart by being the
largest moving mass, with the banners kept at the bubble's edge. In the split arena the cream is the brightest thing
in the night half, but in the day half it could wash out under gold-white light: keep the day floor a mid warm tone so
the cream stays lighter than it. Check in frame 3.

**Avoid.**
- Olympus (Decided wariness): no white marble colonnades, no cloud floors, no thrones in a row.
- Copied idols or temple iconography of worshipped gods: the eagle and the bull are nods, not statues of Garuda and
  Nandi. No charioteer look for the sponsor (Proposal in `WORLD.md`).
- Black-and-white noir: the drained world is warm grey, a faded photograph, never pure greyscale.
- Colour leaking outside the thaw anywhere but the council, and the figure on the bed of arrows once thawed.
- A sky so full of frozen arrows that the hazard lanes cannot be read: the hanging arrows leave clear floor.
- Twelve seats reading as a rainbow: each seat's colour is its substance (rule 5), small and on its seat.

**Mood frames to render.** Game camera: 12 m from the player, pitched 55 degrees down, 40 degree vertical field of
view, the player at the centre, 1280 by 720. The player wears the champion's land outfit
(`staging/blender/2026-11-18_outfits/outfit_champion.fbx`), mid-stride unless stated. Render to
`staging/blender/<date>_moods/` under the names given.

1. **`ch_thaw`**: the bubble.
   - *Camera*: game camera, on the battlefield path.
   - *In frame*:
     - grey soldiers frozen mid-charge on both sides of the path, from two lands (the gilded with square shields and
       the storm soldiers in wide iron hats);
     - arrows hanging in the air with their shadows on the floor;
     - dust hanging;
     - round the player, the 5 m thaw: a marigold garland and a saffron banner in colour and moving, the dust falling,
       two arrows inside flying again, a gilded soldier at the edge with its hand twitching and its gold coming back;
     - at the top of the frame, beside the path, the figure on the bed of arrows, grey with his head turned toward
       the player.
   - *Light*: the crescent sun, dim and strange; sharp, faint shadows; small crescents of light under a banner's
     fringe.
   - *Must prove*: the bubble reads from above as a circle with a soft edge; grey against colour is the whole story;
     the stillness reads as frozen time and not as a paused game; the man on the arrows reads as grey but alive.
2. **`ch_council`**: the twelve seats.
   - *Camera*: wide, from above the ring's outer edge, about 40 m up, looking across and down.
   - *In frame*:
     - the mandala floor;
     - the ring of twelve small stepped shrine-towers facing outward, each in its element's substance;
     - the gnomon at the centre with its long shadow across the floor;
     - the broken grey seats of Light and Dark in front;
     - the champion sitting between them;
     - the frozen grey war beyond the ring on every side;
     - the player small, entering the ring.
   - *Light*: the crescent sun with the faint corona; each seat lit by its own element.
   - *Must prove*: twelve colours in one frame read as a ring of substances, not a rainbow; it reads as an Indian
     temple ring and not as Olympus; the broken seats read as broken.
3. **`ch_day_night`**: the split domain.
   - *Camera*: game camera pulled back to 18 m (the boss camera is Open in `docs/GAME.md`).
   - *In frame*:
     - the dividing line crossing the frame on a diagonal, with the player standing just on the day side;
     - in the day half, the eagle's shadow on the floor and a gold-white feather of light striking;
     - in the night half, a black pool of the bull's dark, and the bull at the edge of the frame turned toward where
       the eagle will land;
     - the mandala inlay running across both halves;
     - two seats of the ring at the top of the frame.
   - *Light*: two key lights, a hard gold-white sun over the day half and a cool violet starlight over the night;
     totality's pale horizon glow.
   - *Must prove*: both halves read and the line between them is hard; the player reads in each half (the cream not
     washed out in the day); Light and Dark are the only strong colours on the floor.

---

## 11. Questions for the user

Answers of 2026-11-17, evening:

1. **Water's sky.** **Decided** after the mood frames: tropical with a storm coming (section 4).
2. **Metal against Fire.** **Decided**: the separation is enough; the game-camera frame is right (section 5).
3. **Ice's light.** **Decided**: a snowy mountainscape with blowing snow, neither very bright nor very dark (see
   section 6).
4. **Poison's colour.** Researched (section 7). **Proposal**: the two miasmas of the old texts, blue-green and the
   deadlier bile-yellow, with cinnabar, mercury, and the alchemist's purple and gold.
5. **The champion's frozen world.** **Decided**, as proposed, with the seats in their elements' colours (section 10).
6. **Earth and Life.** Deferred by the user until the review of the Lightning and Wind and the Earth and Life lands.
   The sheet is now written in full for that review (section 9, 2026-11-18).

Questions of 2026-11-18, raised while writing the sheets for the mood frames (all **Open**):

7. **Lightning and Wind's base.** **Decided (user, 2026-11-18)**: the dry storm night with no rain. Also decided:
   the cloud lit from inside (lightning flickering within the cloud as the land's main light; ground bolts rare).
8. **Rain on the ride out.** **Decided (user, 2026-11-18)**: yes. The dry storm breaks into the land's first rain behind the storm cloud as
   the player leaves, the way Water's storm breaks to sun and Fire's noon sets. Yes, or does it stay dry to the end?
9. **The screen room's gold.** **Proposal**: dull, darkened gold leaf kept to that one room, the land's only warm
   surface. Is a second warm colour welcome there, or should the screens be ink on dark paper?
10. **The vermilion on the stairs.** **Proposal**: the gates are ruined, so vermilion survives only as scraps of paint
    on them, whole only on the great torii and the crossing shrines. Is that rare enough, or too little red for a
    stair of gates?
11. **Poison's red.** The approved red and gold robe shares the cinnabar accent's hue. **Decided (user, 2026-11-18)**,
    which also gives the land a more corrupted, poisonous look: cinnabar stays
    small, flat and a step browner than the robe, and the river town's lanterns become amber paper instead of red.
12. **Earth and Life's player.** The green check tunic sits near the moor and Life's green. **Proposal**: keep it a
    muted olive and let the rust cloak carry the figure. To be judged in `el_giants_dance` and `el_sacred_grove`.
13. **The eclipse's timing.** The land is under an eclipse, and the champion also makes one at the end of phase 1.
    **Decided (user, 2026-11-18)**: the champion causes the eclipse; the sun was almost eclipsed when time stopped,
    so a last crescent burns until his chakram covers it at the phase change (section 10).
14. **The man on the bed of arrows.** **Proposal**: grey like the frozen world but moving, coloured only when the
    player's thaw reaches him, so the council stays the first place with colour of its own. Yes?
15. **Ice's two reds and the alpha.** **Proposal**: the hunt's red a cool deep crimson, apart from the scarf's rust;
    the alpha fought in a pack's clearing at the top of the forest. Yes?
16. **The boss camera in the frames.** How the camera frames large bosses is Open in `docs/GAME.md`. The frames of
    the duo beasts and the split domain assume 18 m as well as 12 m. Is 18 m the right trial distance?
17. **Which frames come first.** Seventeen frames are listed (Ice 3, Poison 3, Lightning and Wind 4, Earth and Life 4,
    the champion's seat 3, with some rendered twice). **Proposal**: render Lightning and Wind and Earth and Life first,
    since the user's look review of the duo lands is waiting on them.
