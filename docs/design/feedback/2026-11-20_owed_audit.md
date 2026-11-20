# Owed-change audit of the current films (2026-11-20)

Every comment in `2026-11-19_effects_review.md` (114, the two later re-film comments and the brazier note) and
`2026-11-20_effects_review_2.md` (57) was checked against the newest film of that look. The question for each was
whether the change the owner asked for is visible in the strip. This is a read-only audit; nothing was changed.

**Films used:**

- Element afflictions and surges: `review/gifs/strips/elements/*.png` (04:13 to 05:05). These are newer than
  `strips/b/` (02:18 to 02:23).
- Reactions: `review/gifs/strips/reactions/react_*.png` (09:30, the films in `doc_main5`).
- Braziers: `review/gifs/strips/braziers/*.png`.
- Weapon swings: `review/gifs/swings/strips/` (2026-11-19, 21:18 to 23:08).
- Element spells, hits and charged swings have no strip. For those, eight-frame strips were cut from
  `review/gifs/elements/frames/<element>_<moment>/`. Those frames were filmed at 2026-11-19 17:38 to 17:46, after
  the first review was recorded (16:46) and with the 17:54 fix pass in them. Wind's hit was filmed again at
  2026-11-20 01:50.
- None of these moments has been filmed since the second review, and none of them is in the document's current
  GIF sets.

**How the verdicts were made:** "Pure white" means pixels of 250 or more in all three channels, counted per frame.
Where the strip says a reaction is "Split Rock" or "Windthrow", the owner's second review calls the same look Root
Snare and Seed Storm. Likewise "Burning Sap" is Sun-Sap.

## PARTLY and MISSED

| Look | Owner's words (review date) | What the current strip shows instead | Strip | Verdict |
|---|---|---|---|---|
| Flashover | "The Lightning doesnt look like its coming from the sky" (2026-11-19) | All the flame is now violet, with no orange. The lightning is short white zigzags around the body at t 0.00; no bolt comes down from above. | `review/gifs/strips/reactions/react_flashover.png` | PARTLY |
| Thunder Gust | "a small wind tornado or cyclone almost around the enemy and with bolts of lightning striking through it" (2026-11-19) | A big violet-and-white streaked tornado at 0.85 to 1.25 s, with leaves. No bolt strikes through it; the only bolt, at 1.70 s, is outside the cyclone after it has shrunk. | `review/gifs/strips/reactions/react_thunder_gust.png` | PARTLY |
| Thunder Gust | "This is still a bunch of static lines going through the guy and not a wind mixed in with electricity. The shock chaining between everyone is good though" (2026-11-19, later) | It is wind now, but no electricity is mixed into the wind. The chain shock between enemies, which the owner liked, cannot be seen in any frame. | same | PARTLY |
| Thunder Gust | "it needs to be more like a lightning tornado ... I am also still getting a bunch of strange shapes and lines, instead of actual wind shapes" (2026-11-20) | The tornado is built from many thin pale streak lines, not lightning. | same | PARTLY |
| Rime | "not sure what the concentric circles coming off you are doing here, they are super random" (2026-11-19) | The cone along the hit is done (1.25 to 1.70 s). Grey dashed arcs and a green wind arc still circle the enemy's feet at 2.10 to 2.95 s. | `review/gifs/strips/reactions/react_rime.png` | PARTLY |
| Rime | "the rime instantly all appears instead of slowly building" (2026-11-20) | At 0.40 s ice covers the legs; by 0.85 s the whole body is white ice. It does not build. The pooled ice look (`react_look_ice_rime.png`) turns the body pure white by 0.85 s. | `react_rime.png`, `react_look_ice_rime.png` | MISSED |
| Quickening | "this growth can also give off a green/brown pulse, when grown kind of visualizing the heal and the seeding" (2026-11-19) | A drop and a splash, then blue sprouts. No green or brown pulse. By 2.55 s the plants are replaced by green hexagon chunks and twigs. | `review/gifs/strips/reactions/react_quickening.png` | PARTLY |
| Quickening | "they should just stay grown, also they should be much bluer to really get the water feel and when they burst should have a splash effect" (2026-11-20) | The plants are blue from 0.85 to 1.70 s and the splash is there. They do not stay: by 2.55 to 2.95 s only green hexagons and sprigs remain. | same | PARTLY |
| Split Rock (Root Snare) | "There is no real root effect I can see, its too small and at the feet" (2026-11-19) | Roots are still small brown scribbles at the feet (0.40 s and 1.70 to 2.95 s). A tan slab bursts at 0.85 s. | `review/gifs/strips/reactions/react_split_rock.png` | PARTLY |
| Split Rock (Root Snare) | "brown roots should grow out of the ground and snare the legs of the opponents. Afterwards the heavy attack leaves a large crack on the ground that spores come out of" (2026-11-20) | No root visibly grips the legs. There is no large crack and no spores, only small cracks and green hexagons. | same | MISSED |
| Anvil Stone | "it is better to have it spring up from the ground, and have the anvil in the center of the effect not one side" (2026-11-19) | A grey block does rise out of cracks (0.40 to 1.25 s). It sits at the enemy's right flank, not under its centre. | `review/gifs/strips/reactions/react_anvil_stone.png` | PARTLY |
| Earthlight | "It doesnt really give the feel of light shining upward" (2026-11-19) | The slabs now shove up and tip, which is done. The light comes down as rays from the sky, then the Marked ring covers the floor. Nothing shines upward from the cracks. | `review/gifs/strips/reactions/react_earthlight.png` | PARTLY |
| Black Chain | "it should be a chain in pure black binding two enemies together" (2026-11-19) | A chain does link the two enemies. It is dark with a bright violet glow and reads purple, not pure black. | `review/gifs/strips/reactions/react_black_chain.png` | PARTLY |
| Smoulder | "an idea is flames turning black and then burning kind of like Amaterasu" (2026-11-19) | The flame on the body is violet with an orange back-glow, then a violet outline. None of it is black. | `review/gifs/strips/reactions/react_smoulder.png` | PARTLY |
| Smoulder | "the hooded flame and whatnot is not good" (2026-11-20) | The leap to other enemies is gone. The body flame is still one hood-shaped violet cowl (0.40 to 0.85 s), and a spiky violet splash follows at 1.25 s. | same | PARTLY |
| Windthrow (Seed Storm) | "the visuals have nothing in them just random polygons chucked out" (2026-11-19) | The wind ribbons carrying seeds read better. Green hexagon chunks still pop out around the uproot patch. | `review/gifs/strips/reactions/react_windthrow.png` | PARTLY |
| Windthrow (Seed Storm) | "its hard to really see the roots trapping the enemy, and the wind throwing the enemy has a bunch of random stuff pop up at the uproot location" (2026-11-20) | No root grips the enemy in any frame. A brown disc ringed with green hexagon chunks appears at the uproot (0.85 to 1.70 s), and more hexagons appear at 2.55 to 2.95 s. | same | MISSED |
| Rust | "when the water effect goes, it turns rusty" (2026-11-19) | A small blade is through the enemy and a splash lands (0.85 s). The blade stays plain grey steel at 1.70 to 2.95 s. Only a tan puddle appears. | `review/gifs/strips/reactions/react_rust.png`, `react_look_metal_rust.png` | PARTLY |
| Rust | "The rust is not a rusty color at all" (2026-11-20) | The only "rust" is a pale tan puddle, sampled at about RGB 167, 117, 67. It is not red-brown rust, and the blade does not change colour. | same | MISSED |
| Quicksilver | "a poisonous looking thing that clearly when sticking through them is poisoning them" (2026-11-19) | A small grey blade with green puffs around the body. The blade itself does not look poisonous, and no silver mercury is visible. | `review/gifs/strips/reactions/react_quicksilver.png` | PARTLY |
| Girdled | "it just kinda looks like there is a wind effect around the black roots" (2026-11-19) | Purple coils still wrap the body in rings that read as a swirl (0.85 to 1.25 s). A beige compass ring also sits under the enemy at t 0. | `review/gifs/strips/reactions/react_girdled.png` | PARTLY |
| Rot Roots | "there needs to be roots that actually grow up from the ground that look like they are rotting and giving off poison" (2026-11-19) | Olive vine lines run along the ground to the others and green clouds come later. The roots barely rise and are not rotten. | `review/gifs/strips/reactions/react_rot_roots.png` | PARTLY |
| Rot Roots | "the roots don't give enough of the rot idea" (2026-11-20) | Same vines: olive with tan nodules and teal beads. No decay, blackening or ooze. There is a pure-white flash at 2.10 s. | same | MISSED |
| Live Iron | "There should be a sword through the person and jolts of lightning chaining from it to their body, then those bolts chain to others" (2026-11-19) | The blade is through the enemy and small jolts show on the body. The chain to others is a faint, thin, straight pink line (0.85 to 1.70 s). | `review/gifs/strips/reactions/react_live_iron.png` | PARTLY |
| Live Iron | "The arc bouncing between the enemies doesn't look good" (2026-11-20) | Unchanged: the faint thin straight line, not an arc of lightning. | same | MISSED |
| Golden Feathers | "there is no feathers, and there is no wind effect" (2026-11-19) | There is now a wind ribbon. The "feathers" are flat yellow jagged strips (0.40 to 0.85 s). | `review/gifs/strips/reactions/react_golden_feathers.png` | PARTLY |
| Golden Feathers | "The feathers dont really look like feathers they look like wheat" (2026-11-20) | Same flat yellow jagged strips, with no quill and no vane. The Marked ring then fills the floor. | same | MISSED |
| Winter Seed | "the blooming flower should be a ice flower with ice thorns and not a pink flower overshadowed by massive ice thorns" (2026-11-20) | The flower is no longer pink. It is a small pale-blue flower at the feet, still dwarfed by the large ice shards (1.25 to 1.70 s). Green sprigs and hexagons take over at 2.55 s. | `review/gifs/strips/reactions/react_winter_seed.png` | PARTLY |
| Lightning Flower | "Make them lightning looking flowers, like a blue purple flower not pink" (2026-11-20) | Not pink. The flowers are very pale lavender-white, almost white, with blue bolts. They do not read as blue-purple. | `review/gifs/strips/reactions/react_lightning_flower.png` | PARTLY |
| Red Hot | "The cone should be like a blast of fire through the enemy; look to the flame wave spell as inspiration" (2026-11-20) | A fan of cream and yellow cone spikes behind the enemy (0.85 to 1.25 s). It reads as spikes or sparks, not a flame blast like Flame Wave. | `review/gifs/strips/reactions/react_red_hot.png` | PARTLY |
| Drowned Weight | "the water droplets should be one mega droplet dropping on top of the persons head" (2026-11-20), with "the water should be super dark, pretty much black" (2026-11-19) | The mega drop is there, at 1.70 s, but it is bright cyan. The pool is dark; the drop is not. | `review/gifs/strips/reactions/react_drowned_weight.png` | PARTLY |
| The Ground Gives | "the ground frames really need like a splash outward or something to give more of a feel of water" (2026-11-19) | The outward splash is brown mud with tan pebbles (0.85 to 1.25 s). No water splash goes outward; there is only a cyan drop later on the body. | `review/gifs/strips/reactions/react_the_ground_gives.png` | MISSED |
| Shining Herb | "The herb shouldnt be on the enemy it should be next to it, and make sure the plant is almost pure light type, so like the green roots to the flower should be recolored" (2026-11-19) | Gold leaves burst on the enemy's body (0.40 to 1.70 s). No herb stands beside it, and green sprigs and hexagons sit on the body at 2.55 to 2.95 s. The owner praised the redrawn Blender timeline on 2026-11-20, not this film. | `review/gifs/strips/reactions/react_shining_herb.png` | MISSED |
| The Grudge | "it looks like some lightning is frozen in place even before the black bolt of lightning hits, and it also looks like the black bolt doesnt connect to the cloud" (2026-11-19) | The cloud floats from 0.40 to 1.70 s, then fades. The bolts appear on the body at 2.10 to 2.95 s, after the cloud is gone. They are white-violet, and no black bolt joins the cloud. | `review/gifs/strips/reactions/react_the_grudge.png` | MISSED |
| Breath Taken | "it should look like the black wind is going outwards from the enemies" (2026-11-19) and "it reads not as black wind coming out, it needs to have more wind identity" (2026-11-20) | Pale puffs, purple tendrils, a black hole underfoot and cream arc rings (2.10 to 2.95 s). No black wind bursts outward. | `review/gifs/strips/reactions/react_breath_taken.png` | MISSED (both comments) |
| Fire: Burn | "The burn is always a overlayed staircase of flames upward ... the purple ones that flicker and look like real flame should be how the flames look" (2026-11-20) | The flames now stand upright (the 2026-11-19 ask is done). On every burning body they are still columns of small orange tongues stacked up the sides, which is the staircase. Seen the same way in Steam, Thermal Shock, Red Hot and Kindling Point. | `review/gifs/strips/elements/fire_affliction.png` | PARTLY |
| Wind: Unsteady (the wind part of the comment) | "a lot of reactions with wind in it have random white shapes pop up, which dont make any sense" (2026-11-20) | Unsteady itself is fixed: curved arcs and leaves around each enemy. White shapes remain elsewhere: white paper squares in wind's hit, a pure-white crescent in Cutting Wind at t 0.30 (1,911 pure-white pixels), and white slash streaks in Tailwind. | `review/gifs/strips/elements/wind_infused.png`, `wind_surge.png`, `review/gifs/elements/frames/wind_hit/` | PARTLY |
| Wind: charged blade's blows landing | "the end cuts off randomly, it needs to be a smooth thing" (2026-11-19) | In the 01:50 re-film the pieces scatter off and vanish by frame 41 of 58, so the cut is softer. The blow is still straight white streaks through the enemy, with white paper squares popping (frames 16 to 33). | `review/gifs/elements/frames/wind_hit/` (`elem_wind_hit.gif`) | PARTLY |
| Ice: charged blade's blows landing | "The white spiderweb thing is really throwing off everything" (2026-11-19) | The crack-web is gone. In its place is a white snowflake decal on the floor and a white ice-chunk cloud over the enemy's legs (frames 17 to 42). It still covers the body. | `review/gifs/elements/frames/ice_hit/` | PARTLY |
| Ice: charged blade swung | "it should be kinda like a chainsaw type of ice" (2026-11-19) | The great sword shows serrated teeth on the blade at frame 42, then a smooth, near-white full ring. The element film shows almost nothing on the spear. | `review/gifs/swings/strips/alone_gsword.png`, `review/gifs/elements/frames/ice_charged/` | PARTLY |
| Ice: Frost Bite | "again that white spiderweb thing is really covering everything up" (2026-11-19) | A pure-white flash at frame 15, then a white snowflake decal and a white block over the enemy. It still covers everything. | `review/gifs/elements/frames/ice_infused/` | PARTLY |
| Water: Riptide | "The wave is really good, other than the random triangle polygons on top" (2026-11-19) | Pale triangle shards are still on the crest (frame 23). White bubble rings follow (frames 23 to 35), then a white dashed ring around the player (frame 47). | `review/gifs/elements/frames/water_spell/` | PARTLY |
| Water: Wellspring | "it should be water flowing off you though almost similar to water, or water pooling towards you" (2026-11-19) and "This is still floating water ... almost like a pool of water under the player" (2026-11-20) | There is a pool under the player now. Cyan fins and plumes are still flung up around a bright rim ring from 0.65 to 3.85 s, and nothing flows toward the player. | `review/gifs/strips/elements/water_surge.png` | PARTLY (both comments) |
| Metal: charged blade's blows landing | "This one shouldnt have anything grown on the weapon, it just adds a glint to the weapon" (2026-11-19) | A large pure-white flash disc, then concentric broken white rings round the enemy (frames 12 to 35) and radiating dashes on the floor (frames 46 to 58). Far more than a glint. | `review/gifs/elements/frames/metal_hit/` | MISSED |
| Metal: charged blade swung | "Nothing should be added to the weapon other than a glint" (2026-11-19) | A grey stepped streak with gold sparks (element film, frame 32). The swing films end in a burst of dashed concentric rings around the player (great sword frames 99 to 113, armament frames 59 to 75). | `review/gifs/swings/strips/alone_gsword.png`, `alone_armament.png` | PARTLY |
| Life: Seeded | "Instead of plants growing for the seeded, it should be like spores" (2026-11-19) and "the seeded affliction should have a concept of seeds/spores on the person, that then bloom" (2026-11-20) | Olive hexagon seeds and tan ovals are on the bodies. Green sprouts and vines still grow across every enemy, and there are no spores or spore puffs. | `review/gifs/strips/elements/life_affliction.png` | PARTLY (both comments) |
| Dark: Cursed | "the curse coming off the enemy doesn't look great" (2026-11-19) and "It should be like dark energy being sucked down into the black hole at the bottom" (2026-11-20) | The small black hole under each enemy is there, and the shadow is gone. Purple curling wisps stand up and off the body; nothing is drawn down into the hole. | `review/gifs/strips/elements/dark_affliction.png` | PARTLY (both comments) |
| Dark: charged blade swung | "I Don't like the random black crystals on the sword and the falling curse symbols" (2026-11-19) | The crystals are gone. Purple trident-shaped curse glyphs still float and fall around the player (frames 17 to 42). | `review/gifs/elements/frames/dark_charged/` | PARTLY |
| Dark: Taken | "I have no clue what this is doing" (2026-11-19) | A purple ring arc, a white mask- or fang-shaped piece and a shadow pool (frames 17 to 33). It is still unreadable. | `review/gifs/elements/frames/dark_infused/` | MISSED |
| Poison: Miasma | "it needs to be like a cloud ... it just needs to be a better color for poison" (2026-11-19) | Low mint or seafoam wave shapes, then a flat teal square pool (frames 111 to 134). It is not a cloud, and its colour does not match the newer yellow-green poison clouds. | `review/gifs/elements/frames/poison_spell/` | PARTLY |
| Brazier: Lightning | "Lightning: a crackling caged storm" (2026-11-19, in chat) | A blue orb with bolts crackling to the rim. There is no cage. | `review/gifs/strips/braziers/brazier_lightning.png` | PARTLY |

## The general rules, across every strip

- **Nothing pure white:**
  - Pure-white crescent: Cutting Wind at t 0.30 (`elements/wind_infused.png`).
  - Bodies turned fully pure white: the Rime look (`react_look_ice_rime.png`) and the Rime reaction at 0.85 to
    1.70 s.
  - Pure-white highlights on the Chill ice, about 570 pixels a frame (`elements/ice_affliction.png`).
  - Pure-white flashes:
    - Rot Roots at 2.10 s, the most pure white of any reaction strip;
    - the hits of metal, light, water and poison;
    - Frost Bite, Broken Ground, Grown Into It and Briar Patch;
    - Flame Wave's frame 65.
  - Other white shapes:
    - Sleet Charge's floor snowflake;
    - the great sword's ice swing ring;
    - wind's white paper squares;
    - Nerve Fire's pale-white body flashes.
- **Rings and circles:**
  - Concentric broken rings: metal's hit and metal's swing burst.
  - Riptide's bubble rings and its dashed ring around the player.
  - Rime's dashed arcs at the enemy's feet.
  - Wellspring's bright rim ring.
  - Shut in the Dark's large pink loops at 0.40 to 0.85 s.
  - Beige compass rings at t 0 in Girdled and Smoulder.
  - Marked's cream mandala ring fills the floor in Glint, Kindling Point, Shining Herb, Golden Feathers, The Bolt Runs
    True, Mock Suns, Earthlight, Caught Light and Burning Sap. The owner praised Marked itself, but in these reactions
    it is the largest shape on screen.
- **No line through the body for wind:**
  - Wind's charged hit is straight streaks through the enemy.
  - Logan Stone at 0.85 s has long straight ribbons across the body.
  - Unsteady, Ill Wind and Carried Embers now go round the body.
- **No stacked staircase for fire:**
  - Burn still shows stacked tongues up the body in `fire_affliction.png`, Steam (1.70 to 2.95 s), Thermal Shock, Red
    Hot (2.10 to 2.95 s) and Kindling Point (1.70 s and later).
  - The fire braziers are a crown of flames, not a staircase.

## Count

| Source | DONE | PARTLY | MISSED | CAN'T TELL | N/A | Total |
|---|---|---|---|---|---|---|
| 2026-11-19, 114 picture comments | 53 | 27 | 6 | 4 | 24 | 114 |
| 2026-11-19, 2 later re-film comments (also repeated in the second review) | 1 (Flashover) | 1 (Thunder Gust) | 0 | 0 | 0 | 2 |
| 2026-11-19, brazier note (11 braziers) | 10 | 1 | 0 | 0 | 0 | 11 |
| 2026-11-20, 57 comments less the 2 repeats | 17 | 12 | 8 | 0 | 18 | 55 |
| **All** | **81** | **41** | **14** | **4** | **42** | **182** |

The table above has 51 rows for these 55 PARTLY and MISSED verdicts, because a row that covers both reviews counts
once per review.

- **CAN'T TELL (4):**
  - Shut in the Dark (speed; the strip runs 3.1 s, but pace can't be judged from still frames).
  - The Bolt Runs True (whether the straight bolt passes all the way through).
  - Fire's charged blade landing (the blade's own look is hidden by the burst).
  - Wind's charged blade swung (whether it still reads backward).
- **One DONE that depends on motion:** Seized's circles are gone, but its flicker can't be judged from stills.
