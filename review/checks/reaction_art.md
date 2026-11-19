# Reaction art

24 designed looks from Assets/_Project/Art/VFX/Combos/combos_recipe.json, 94 parts, 28 lamps, 28 floor masks. A look over 8000 triangles is marked OVER. Lamps: intensity = W / (4 pi^2), range sqrt(2 I) held to 1.5-5 m, at most 2 a look.

| Reaction (recipe) | Game name | Pieces | Parts (solid, glow, marks) | Triangles | Lamps kept of | Notes |
|---|---|---|---|---|---|---|
| Thermal Shock (fire_ice) | Thermal Shock | 80 (3 flat) | 5 (4, 0, 1) | 5610 | 2 of 3 | lamps left out: c2_fire_ice_cold 200 W |
| Flashover (fire_lightning) | Flashover | 56 (3 flat) | 4 (3, 0, 1) | 5876 | 2 of 2 |  |
| Fire Medicine (poison_fire) | Fire Medicine | 84 (7 flat) | 7 (4, 1, 2) | 7266 | 2 of 3 | lamps left out: c2_poison_fire_heart 150 W |
| Sleet Charge (lightning_ice) | Sleet Charge | 88 (2 flat) | 4 (3, 0, 1) | 2818 | 2 of 2 |  |
| Diamond Ring (light_dark) | Diamond Ring | 11 (7 flat) | 6 (3, 1, 2) | 416 | 1 of 1 |  |
| The Grudge (dark_lightning) | The Grudge | 8 (2 flat) | 3 (2, 0, 1) | 664 | 1 of 1 |  |
| Capsize (wind_water) | Capsize | 39 (4 flat) | 3 (2, 0, 1) | 2679 | 1 of 1 | laid along the blow |
| Rime (wind_ice) | Rime | 85 (3 flat) | 3 (1, 1, 1) | 3002 | 1 of 1 | laid along the blow |
| Caught Light (water_light) | Caught Light | 22 (5 flat) | 4 (2, 1, 1) | 2168 | 1 of 1 |  |
| Burnt Mound (earth_fire) | Burnt Mound | 59 (8 flat) | 4 (3, 0, 1) | 3176 | 2 of 5 | lamps left out: c2_earth_fire_stone0 180 W, c2_earth_fire_stone1 180 W, c2_earth_fire_stone2 180 W |
| Fish Poison (poison_water) | Fish Poison | 35 (10 flat) | 4 (2, 0, 2) | 2710 | 1 of 1 |  |
| The Sealed Jar (dark_poison) | Shut in the Dark | 98 (2 flat) | 4 (3, 0, 1) | 5788 | 1 of 1 |  |
| Nerve Fire (poison_lightning) | Nerve Fire | 10 (4 flat) | 3 (2, 0, 1) | 498 | 1 of 1 |  |
| The Ground Gives (water_earth) | The Ground Gives | 35 (5 flat) | 6 (4, 0, 2) | 2312 | 0 of 0 |  |
| Logan Stone (wind_earth) | Logan Stone | 33 (2 flat) | 4 (3, 0, 1) | 1966 | 0 of 0 | laid along the blow |
| Anvil Stone (earth_metal) | Anvil Stone | 27 (3 flat) | 3 (2, 0, 1) | 3889 | 1 of 1 |  |
| Iron Thorns (life_metal) | Iron Thorns | 46 (2 flat) | 3 (2, 0, 1) | 2008 | 1 of 1 |  |
| Girdled (life_dark) | Girdled | 31 (4 flat) | 3 (2, 0, 1) | 5766 | 0 of 0 |  |
| Steam (fire_water) | Steam | 53 (3 flat) | 3 (2, 0, 1) | 7421 | 2 of 2 |  |
| Glint (metal_light) | Glint | 13 (4 flat) | 4 (2, 1, 1) | 749 | 1 of 1 |  |
| Firebloom (fire_life) | Firebloom | 48 (7 flat) | 4 (3, 0, 1) | 3140 | 2 of 4 | lamps left out: c2_fire_life_lily1 150 W, c2_fire_life_lily2 150 W |
| Earthed (lightning_earth) | Earthed | 35 (2 flat) | 5 (4, 0, 1) | 4746 | 2 of 2 |  |
| Stolen Warmth (dark_ice) | Stolen Warmth | 22 (5 flat) | 3 (2, 0, 1) | 1136 | 0 of 0 | laid along the blow |
| The Bolt Runs True (light_lightning) | The Bolt Runs True | 6 (4 flat) | 2 (1, 0, 1) | 156 | 1 of 1 |  |

## Composed (31)

| Reaction | First parent's piece | Second parent's piece | Ground mark |
|---|---|---|---|
| Thunder Gust | lightning_crawl_affliction | wind_debris_hit | - |
| Supercooled | ice_tiers_affliction | water_aftersplash_hit | - |
| Quickening | water_aftersplash_hit | life_sprout_affliction | - |
| Split Rock | life_sprout_affliction | earth_cubes_hit | - |
| Carried Embers | fire_globs_hit | wind_debris_hit | fire_sootring_hit |
| Winter Seed | ice_tiers_affliction | life_sprout_affliction | - |
| Earthlight | earth_cubes_hit | light_prabhavali_hit | light_lotus_hit |
| Black Chain | metal_clang_hit | dark_streaksair_hit | dark_drain_hit |
| Burning Sap | light_prabhavali_hit | poison_wound_hit | poison_stain_hit |
| Smoulder | dark_streaksair_hit | fire_globs_hit | fire_sootring_hit |
| Seized | lightning_crawl_affliction | water_aftersplash_hit | - |
| Windthrow | wind_debris_hit | life_sprout_affliction | - |
| Frost Heave | ice_tiers_affliction | earth_cubes_hit | - |
| Rust | water_aftersplash_hit | metal_clang_hit | metal_filings_affliction |
| Shining Herb | life_sprout_affliction | light_prabhavali_hit | light_lotus_hit |
| Turned to Stone | earth_cubes_hit | dark_streaksair_hit | dark_drain_hit |
| Quicksilver | metal_clang_hit | poison_wound_hit | poison_stain_hit |
| Kindling Point | light_prabhavali_hit | fire_globs_hit | fire_sootring_hit |
| Ill Wind | poison_wound_hit | wind_debris_hit | poison_stain_hit |
| Lightning Flower | lightning_crawl_affliction | life_sprout_affliction | - |
| Cold Short | ice_tiers_affliction | metal_clang_hit | metal_filings_affliction |
| Chokedamp | earth_cubes_hit | poison_wound_hit | poison_stain_hit |
| Red Hot | metal_clang_hit | fire_globs_hit | fire_sootring_hit |
| Breath Taken | dark_streaksair_hit | wind_debris_hit | dark_drain_hit |
| Venom Rime | poison_wound_hit | ice_tiers_affliction | poison_stain_hit |
| Sickle Wind | wind_debris_hit | metal_clang_hit | metal_filings_affliction |
| Mock Suns | ice_tiers_affliction | light_prabhavali_hit | light_lotus_hit |
| Drowned Weight | water_aftersplash_hit | dark_streaksair_hit | dark_drain_hit |
| Rot Roots | life_sprout_affliction | poison_wound_hit | poison_stain_hit |
| Live Iron | metal_clang_hit | lightning_crawl_affliction | metal_filings_affliction |
| Golden Feathers | light_prabhavali_hit | wind_debris_hit | light_lotus_hit |
