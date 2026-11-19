# Cross-land kit list

Written by Claude on 2026-11-19 from the eight level plans, so that builders reuse one piece where several lands need the
same thing. **Nothing here is decided.** It lists what the plans already ask for, what of it exists in the code today,
and a suggested single way to build each shared piece. Where two plans want different versions of the same piece, the
difference is a question for the owner in section 5, with a recommendation; it is not settled here.

**Read for this list.** The level plans and their build lists: `fire_levels.md` (second pass, 2026-11-17; built, no
questions section), `water_levels.md`, `metal_levels.md`, `ice_levels.md`, `poison_levels.md` (each with the owner's
answers, which win over the plan's body), `lightning_wind_levels.md`, `earth_life_levels.md`, `seat_levels.md` (their
questions are still open). The code: `Assets/_Project/Scripts/Editor/Builders/` (`BuildKit`, `SceneBuild`,
`SunCityGround`, `SunCityKit`, `SunCityBuilder`, `FireMechanicsKit`, `ProvingGroundBuilder` and the other builders) and the
runtime components the builders place (`Runtime/World`, `Runtime/Gameplay`).

**Land names used in the tables:** Fire, Water, Metal, Ice, Poison, L&W (Lightning and Wind), E&L (Earth and Life),
Seat (the seat under the eclipse).

---

## 0. In short

- **23 shared pieces** (section 1) and **23 shared behaviours** (section 3). Eight pieces are needed by every land.
- **What exists is Fire's kit**, and most of it is private. Public today: `SunCityGround.Plateau`, `Ramp`, `Ring`,
  `Oval`, `Rect`; `BuildKit` (materials, `Block`, `Group`); `SunCityKit` (`Brazier`, `RestFlames`, `Lamp`, `Scorch`,
  `Place`); `FireMechanicsKit` (`Jar`, `Mirror`, `WallOfShields`, `Heat`). Private to `SunCityBuilder`: `Stairs`,
  `Ladder`, `Barred`, `Rest`, `ShadeRoof`, `Carving`, `Floor`, `Box`, `Mass`, `Wall`, `Disc`, `Rubble`, `Scatter`,
  `Stain`, `Solid`, `Tilt`, `Drift`, `Trodden`, `Soft`. Several are already copied privately into other builders:
  `Disc` in five (`CastleBuilder`, `HubBuilder`, `SummitBuilder`, `SunCourtBuilder`, `SunCityBuilder`), a rest-fire maker
  in four (`CastleBuilder`, `SunCourtBuilder`, `SunCityBuilder`, `ProvingGroundBuilder.RestFire`), `Rubble` in three.
- The runtime side is in better shape: `Ladder`, `BarredDoor`, `BreakableWall`, `SealedWay`, `RestPoint`, `SceneDoor`,
  `WeaponPickup`, `LandZones`, `Fadeable`, `OcclusionFader`, `FallCatch`, `DistrictMood`, `CirclingBirds`,
  `SunlightZones`, `WaterJar`, `Notice`, `HeatSchedule`, `NoonHeat`, `GroundMark`, `ShieldWall`, `EnemyTurns`, `SunSense`,
  `Enemy.MakeDormant` and `Enemy.HoldGround` all exist and every later plan names them.
- **Fire's kit models do not carry over.** Every later plan says the earlier lands' models are grey-box stand-ins only
  (`fire_levels.md` 6.3 lists Fire's: `reed_boat`, `barque`, `market_awning`, `house_block`, `brazier`, `statue_king` and
  the rest). What is shared is code and shape: the piece, its parameters and its behaviour. Each land brings its own
  models and materials.

---

## 1. Shared pieces

### 1.1 Summary

| # | Piece | Lands | Exists today | Size to make shared |
|---|---|---|---|---|
| K1 | Plateau and built faces | all 8 | `SunCityGround.Plateau` (public) | small (face styles) |
| K2 | Ramp | all 8 | `SunCityGround.Ramp` (public) | small |
| K3 | Stairs and flights | all 8 | `SunCityBuilder.Stairs` (private) | small to medium |
| K4 | Edge rim, parapet, rail | all 8 | `SunCityBuilder.Wall`/`Edge` (private), `Dressing.FloorEdge` | small |
| K5 | Ladder and other fixed climbs | 7 (all but Water) | `Ladder` (runtime), `SunCityBuilder.Ladder` (private) | medium |
| K6 | Barred door or gate | 5 (Fire, Metal, Poison, L&W, Seat) | `BarredDoor`, `SunCityBuilder.Barred` (private) | small |
| K7 | Seal kept by an elite | 4 (Fire, Metal, L&W, E&L) | `SealedWay` (one keeper), `GateOfNoon` | small |
| K8 | One-way drop | 6 (Fire, Water, Ice, Poison, L&W, E&L) | no | small |
| K9 | Keyed opener from one side | all 8 | in part (`Ladder.DownKey`, `BarredDoor.OpenKey`) | medium |
| K10 | Walkable span (rope, plank, trunk, slab) | 7 (all but Fire) | no | medium |
| K11 | Wreck deck: a hull as a tipped floor | 5 (Fire, Water, Metal, Ice, L&W) | Fire's barque deck (in `SunCityBuilder`) | small |
| K12 | Cleft and ravine | 5 (Water, Ice, L&W, E&L, Seat) | no | medium |
| K13 | Vista zone and held shot | all 8 | `LandZones`, `TopDownCamera.SetFraming` | medium |
| K14 | Rest fire | all 8 | `RestPoint`, four private makers | small |
| K15 | Breakable wall or section | 5 (Fire, Metal, L&W, E&L, Seat) | `BreakableWall` | small |
| K16 | Lore carving | all 8 | `Notice`, `SunCityBuilder.Carving` (private) | small |
| K17 | Height band on faces, and gauge post | 2 (Water, Poison) | no | small |
| K18 | Struck sound prop (gong, bell, drum) | 3 (Metal, Poison, L&W) | no | small |
| K19 | Struck heavy prop (jar, vat, cradle, cart, laden tree) | 4 (Fire, Metal, Ice, Poison) | `WaterJar` only | medium |
| K20 | Floor trap (plate, snare, deadfall, net, tripwire) | 3 (Metal, Ice, Poison) | no | medium |
| K21 | Float, moving floor, ride on a line | 6 (Water, Metal, Ice, Poison, L&W, and every ride out) | no | large |
| K22 | Shelter marks (sills, rings, shade, herb ring) | 7 (all but Water) | Fire's shade (`SunlightZones`, `ShadeRoof`) | small (pieces); see S5 |
| K23 | Shadow flyers and life tells | 5 (Fire, Water, Metal, L&W, E&L) | `CirclingBirds` | small |

### 1.2 Each piece

**K1. Plateau and built faces.** A floor of any outline with faces falling to a lower level, faces turned south toward
the camera. Every plan builds its terraces, banks and shores on Fire's B1 and B2.

| Land | Variant |
|---|---|
| Fire | Mud-brick river banks leaning back 12 degrees with strata and a lip of loose blocks; clean ashlar terrace faces with a cornice |
| Water | Scale terraces (W9, overlapping scales, wet sheen, weed on the lip); benched cuts (W7, benches 0.7 by 0.8 m, sheer red north face); wet black sand, mud and reef shores (W20) |
| Metal | Quarry benches with tool-marked bands (M5); the rampart's 8 m faces |
| Ice | Rib faces of bone and blue shade (I14); trench walls with drifts piled on the north side (I5); snow and ice tops |
| Poison | Pale stone terraces with balustrades (P10) |
| L&W | 1.5 m terraces by band: cracked paddy mud, stubble, maple litter, silver grass (M6) |
| E&L | Lynchets 2 to 2.5 m (G6); 6 m of roots over old stone (G14); heather and peat |
| Seat | Sunken-road banks; the council plateau's 6 m faces |

Today: `SunCityGround.Plateau(name, parent, outline, top, bottom, topMaterial, sideMaterial, bandMaterial, jitter, lean,
band, segment)` already does outlines, jitter, lean and strata. Suggested: keep it, and add a **face style** argument
(strata, benches, scales, roots, tool bands, drift) instead of a new face builder per land.

**K2. Ramp.** A sloped walk with side faces down to the ground beside it. Fire's levy ramp and haul ramp; Metal's Slag
Road (four 18 m ramps at 11 degrees with kerbs and drop gaps); Ice's drift ramps onto humps and ribs; Poison's back ramp
outside the wall; L&W's horse ramps; E&L's cart ways (4 m, 14 degrees); Seat's levy ramp and processional. Today:
`SunCityGround.Ramp` (public). Suggested: add optional kerbs and a runner strip (Fire's sledge runners, Metal's rails
and ruts, E&L's hollow-way ruts) as arguments.

**K3. Stairs and flights.** Every land.

| Land | Variant |
|---|---|
| Fire | Stone steps, the Spearman's Stair, the bowl's three steps 30 m wide |
| Water | Hacked steps 0.22 m in red rock; the Gash's stairs; the cleft's two flights |
| Metal | The East, Slag and Yard stairs off the Round; the sally stair down the rampart's outer face |
| Ice | The ice stair beside the frozen falls, with a rope rail |
| Poison | Horse stairs up wall faces (P9); humpback bridge flights (P6); the tier stairs; the stair of long life |
| L&W | Sixteen racecourse legs: flights of 1.5 m rise over 2.6 m with landings, gate sockets every 8 m (M1) |
| E&L | Barrow flights (the barrow road has three, the Dance barrow four); the druids' stair |
| Seat | The last rise; the processional; the gnomon's stair |

Today: `SunCityBuilder.Stairs` (private): a walk ramp under step blocks, 3 to 12 steps. Suggested: move it to a shared
kit, let it make landings and flights longer than 12 steps (an L&W leg rises 8 to 9 m), and take a style (stone, hacked
rock, ice, timber).

**K4. Edge rim, parapet and rail.** Every plan bounds raised edges on the south side with a rim of at most 0.6 m, and
keeps the frame clear of anything taller within 4 m south. Variants: Fire's low south walls and roof parapets; Water's
deck rails, stilt fences and rock lips; Metal's Round parapets; Ice's 0.6 m rail on perches and the heaved ice rim along
open water; Poison's 0.3 m roof lip and 0.6 m south parapet, the plank bridges' rope rails; L&W's leg parapets with one
gap at each platform; E&L's barrow kerbs; Seat's walls of frozen bodies (simple hulls on the first two ranks, its Z2). Today:
`SunCityBuilder.Wall` with shadow-only thin south faces and `Edge` (private); `Dressing.FloorEdge` (public, dressing
only). Suggested: one `Rim(path, height, style, gaps)` that leaves named gaps for drops (K8) and stairs.

**K5. Ladder and other fixed climbs.** Water needs none on its main path (floats and cast bridges take their place).

| Land | Variant |
|---|---|
| Fire | Timber ladder 3 to 4 m; S1 lies on the top, kicked down |
| Metal | Ladder out of Pandora's cell; a grate ladder in the undercroft opened from below; ladders in the moulds |
| Ice | The Raven's shrouds; the hunting stand's ladder; the climbers' rope |
| Poison | Bamboo ladders (1.2 s), clay drainpipes (1.6 s up, 1.0 s down), carts against low roofs (1.0 s); S3 kicked down. **Each with a red lantern at its foot (2.4 m up, a floor pool 2.5 m across) and a small one at its head**, flaring at the first gong (Decided for Poison) |
| L&W | Chains (3 s, kicked down from above); the grandfather cedar's vine rope (2 s, cut loose from above) |
| E&L | The well's rope |
| Seat | A siege ladder let fall (K9); rope ladders at both side paths |

Today: the runtime `Ladder` (climb up or down, one `ClimbSeconds`, `DownKey` for a ladder lying on the top until kicked
down, all its words configurable) and the private `SunCityBuilder.Ladder` builder (timber rails and rungs). Needed:
separate up and down times (Poison's drainpipe), a model per variant, an "opened from below" form (Metal's grate), an
optional marker (Poison's lanterns; whether any other land marks its climbs is not asked by any plan), and a
**narrow footing** tag so the land's sweeping threats skip it (S6).

**K6. Barred door or gate.** Barred on one side; "Lift the bar" opens it for good. Fire S2, S3, S4; Metal S3 (the
Casting Floor's west gate); Poison S2 and the herb garden's gate; L&W S3 (the temple doors) and the war doors S4c and S4m;
Seat S2 (the rampart postern). Water lists it as available for dressing. Variants: mud-brick lane door (Fire), bronze
gate (Metal), palace gate in a wall (Poison), burned temple doors (L&W), postern in a rampart (Seat). Today: runtime
`BarredDoor` and the private `SunCityBuilder.Barred`. Suggested: move the builder into the shared kit with a model per
land; L&W's war doors (barred, but opened when the elite falls) are K7's condition put on a K6 door.

**K7. Seal kept by an elite.** A way that opens when named keepers fall, and stays open. Fire's Gate of Noon after the
champion; Metal's heart reached only after the handmaiden (by the sluice); L&W's two seals on the screen room (a drum
and a knotted cord, one per elite, either order; its question 4 is open) and its war doors; E&L's gate barrow with two
seals and the seat barrows that open as each elite falls (C16). Today: `SealedWay` (one keeper, or all enemies dead;
stays broken after a rest) and `GateOfNoon`. Suggested: let `SealedWay` take several keepers, any order, a progress key
each, and show each broken seal in the world.

**K8. One-way drop.** A gap in the rim over a south-facing face, a landing below, one way. Fire (the barque's prow, the
roofs' north edge); Water (Cut 2's ledge, 1.4 m); Ice (the west ice at the pressure wall, 2 m); Poison (roof drops 2 to 4
m, landing within 10 m of a climb point); L&W (every upper platform into the diamond, 2 to 2.5 m, landing within 30 m
of a stair up); E&L (the grove terrace's two 3 m drops; every drop 2 to 4 m and within 30 m of a way back). Today:
plain geometry, no piece. Suggested: a `Drop(edge, height, landing)` that cuts the rim's gap and **records the drop and
its landing**, so an editor check can test each land's never-stranded rule (the rules differ by land; see question
Q7).

**K9. Keyed opener from one side.** The shortcut family: a thing used once from one side that changes the world and
keeps it changed (a progress key). Every land has several.

| Land | Openers |
|---|---|
| Fire | S1 ladder kicked down; mirror M1 opening the sun door (S4) |
| Water | S1 "Push the plank"; S2 "Cast the net bridge"; S3 "Cast the bridge"; "Lift the sluice" (at low tide only) |
| Metal | S1 counterweight lift sent down; S2 the Thread pin driven out; the Rocks plank kicked across; the grate ladder opened from below; the E and S cradles tipped (until a rest) |
| Ice | S1 frozen sledge freed by a heavy blow, breaking the drift plug; S2 "Cut the rope" on the log boom; S3 the frozen climber knocked off his shelf by a heavy blow from the north |
| Poison | S1 the punt's first trip; S3 ladder kicked down; S4 "Turn the wheel" of the sluice gate, from the north only |
| L&W | S1 "Push the beam across"; S2 the cedar's rope cut loose; "Make fast the bridge"; chains kicked down |
| E&L | S1 "Topple the stone" across the drain; S3 "Lever the slab"; S4 "Roll the kerbstone"; S5 "Push the slab" |
| Seat | S1 and S3 "let it fall": a frozen siege ladder and banyan whose prop is struck in the thaw |

Today: only the special cases (`Ladder.DownKey`, `BarredDoor.OpenKey`, `BreakableWall.BrokenKey`). Suggested: one
`KeyedChange` component: a prompt or a struck trigger (Ice and Seat open with a blow), the side it may be used from, an
optional condition on the land's state (Water's sluice wants low tide), a before pose and an after pose played over a few
seconds, colliders swapped at the end, and a progress key. Ladders, barred doors, spans (K10) and cradles then become
models on one behaviour.

**K10. Walkable span.** A bridge laid between two edges.

| Land | Spans |
|---|---|
| Water | Cast bridges of net (16 m, falling 3 m) and rope (16 m, level), sagging, rope rails (W11); the push plank (W12); the jaw bridge (a rock arch, W14) |
| Metal | The plank to the Rocks |
| Ice | The deadfall bridge: a fallen spruce as a ramp 1.6 m wide (I13) |
| Poison | Three plank bridges over streets, 1.5 m wide, rope rails 0.6 m both sides, **drawn only as their shadow while the player's floor is below them** (P22; the owner's exception for Poison) |
| L&W | Rope bridges over ravines: the broken one and its repaired form, the racecourse rope bridge in diamond 4 |
| E&L | The plank road (G1); the clapper bridge with its levered slab (G12); the root bridge, in the third year only (G11) |
| Seat | The banyan fallen across the west ravine |

Today: none. Suggested: one `Span(from, to, width, sag, rails, style)` mesh and collider, opened by K9 where it is a
shortcut, with the shadow-only-from-below mode kept as an option that only Poison sets (question Q2).

**K11. Wreck deck.** A hull lying heeled, its deck a tipped floor, a rail only on the side away from the camera. Fire's
barque (deck heeled 8 degrees, gangplank, prow drop); Water's wrecked double canoe on the reef (W16, the mast fallen as a
ramp; holds the plain Chain Sickle); Metal's Greek ship at the Rocks; Ice's heeled longships and the jarl's ship (I1,
holds the plain Great Bow); L&W's warship on its side (M20, deck at 18 degrees; holds the plain Katana). Three of the
five hold a land's plain weapon. Today: Fire's `Barque Deck` and `Deck Rail` blocks in `SunCityBuilder`. Suggested:
`HullDeck(hull model, yaw, heel, length, railSide, mastRamp)`.

**K12. Cleft and ravine.** An east-west cut whose north wall is sheer (it faces the camera and reads as the next level
up) and whose south side is a slope of at most 40 to 45 degrees (so it never hides the floor). Water's gill cleft (W8)
and benched cuts (W7), and the Gash, which runs north-south; Ice's ravine (I12); L&W's ravine with its waterfall (M14,
its floor white water, not walkable); E&L's Alder Ravine (G12); Seat's west ravine under the banyan. Metal's Pour Way
(M12) is a channel of the same shape. Today: none (`Plateau` and `Ring` can stand in). Suggested: `Cleft(path, width,
northWall, southSlope, floor)` where the floor is walkable, water (S9) or a fall catch.

**K13. Vista zone and held shot.** Every land: Fire V1 to V6, Water V0 to V6, Metal V1 to V6, Ice V1 to V7, Poison V1 to
V7, L&W V1 to V10, E&L V1 to V7 and H0 to H3, Seat V1 to V5. Today: `LandZones` (rectangles with height bands, one
distance, pitch and look-ahead for the whole land, never while the player is noticed) and
`TopDownCamera.SetFraming`/`ClearFraming`. What the plans add:
- a framing and a direction **per zone** (Water's W19, Metal's table in its section 13, Ice, Poison P21, L&W M23, Seat);
- extra conditions: L&W only between races, Ice only while the huntress is unaware or fallen back, E&L never during a
  horn's warning;
- a framing for **a whole land** (Seat: 1.15 times, its question 1) or **a whole area held** (E&L's plain: 1.3 times);
- **held shots**: a scripted camera move, skippable, replayable, played once per trigger or per state (three in Water,
  Poison, L&W and Seat, four proposed in E&L, scripted glimpses in Ice and Metal; question Q1).
Suggested: extend `LandZones` with a per-zone framing record and a condition hook, and add one held-shot player beside it.
The camera side is S17.

**K14. Rest fire.** Every land, with its own dressing and, in most, a quiet radius the land's threat respects.

| Land | Fires | Dressing | Quiet around it |
|---|---|---|---|
| Fire | 6 | Brazier | (shade rules) |
| Water | 6 | Fire ring | Every fire on both-tide ground, reaching a tide stone at either tide |
| Metal | 6 | Tripod brazier | No throws within 4 m |
| Ice | 6 | Stone ring under a windbreak | No perch sees within 12 m |
| Poison | 6 | Censer burning mugwort and calamus | A ring 4 m round kept clear of mist |
| L&W | 9 | Stone lantern at a crossing shrine | The whole crossing is calm (C17) |
| E&L | 8 | Stone ring (Earth's side), root ring (Life's side) | (none stated) |
| Seat | 4 | A pot on three stones | A ring of thaw 3 m round it |

Today: `RestPoint` (lighting is resting; refills, brings enemies back, sets the return point), `SunCityKit.Brazier` and
`RestFlames`, and four private makers. Suggested: one `Rest(kind, foot, quietRadius)` with a dressing table per land,
the quiet radius written into the scene's shelter list (S5) so each land's threat reads it.

**K15. Breakable wall or section.** Fire's hearth wall (B8, one heavy blow or a burst beside it); Metal's rubble plug
broken by a cart, the rampart's breach and the forge room's dome (both scripted in the fight); E&L's field walls in 4 m
sections broken for good by a ploughing boar, the hollow way's wall burst by the rolling stone, the cracked causeway
slab; L&W's armour suits cut in half by the Retainer's draw; Seat's frozen things that finish their fall (K9). Today:
`BreakableWall` (`BreaksAt`, `BrokenKey`), placed by `SunCityBuilder`. Needed: break on a named event (an enemy's charge,
a rolling stone, a cart) as well as a blow, and a reset policy (for good, or until a rest; S1).

**K16. Lore carving.** Every land has its numbered lore (C1 to C11 in Fire and Metal, and the like in the others), read
at the player's own pace. Variants: obelisk feet and hearth niches (Fire), carved posts (Water), plaque rims and stelae
(Metal), runestones (Ice), seals and steles (Poison), votive tablets at the crossings (L&W), kerbstones and the Fork Stone
(E&L), the figure's words (Seat). Today: `Notice` (click-through), `WakingNotice`, and the private
`SunCityBuilder.Carving` and `Painting`. Suggested: move `Carving` to the shared kit with a model per land.

**K17. Height band on faces, and gauge post.** A band painted on every face up to a height, so the player can read
from anywhere how high a land's rising level comes. Water's weed and barnacle band to y 2.0 with a wet line at y 0, and a
carved tide post at the mouth of every one-tide route (W3; the owner's answer 2); Poison's two stain lines at the green
and yellow tops of the mist (P2), "as the Water land's weed band shows the high tide". Today: none (`Stain` is a floor
stain). Suggested: one decal pass that takes a list of heights and looks and bands every face that crosses them, fed
from the same numbers as the level volume (S9), so the band and the water or mist cannot disagree.

**K18. Struck sound prop.** A gong, bell or drum that a blow (or the land) rings, with a cooldown and an event heard to a
radius. Metal's five gongs in the bronze birds' trial (the owner's choice: ringing one grounds the nearest birds and draws
the next volley) and the anvil's rings; Poison's hour gong in the drum tower (rung by the clock) and the watch gong the
player strikes to start the Third Watch; L&W's temple bell (C16, stretching races), the trial's start and finish bells,
the drummers' slit drums and the great drum. Today: none. Suggested: `StruckProp(sound, cooldown, radius)` raising an
event the land's systems listen to, shared with K19.

**K19. Struck heavy prop.** A prop a heavy blow tips, drops or sets rolling, with a marked landing and an effect.
Fire's water jars (built) and oil jars and knocked braziers (not built); Metal's ore carts with a brake that roll down
slopes and tangle, and the cradles' tip lever; Ice's laden spruce whose load drops on a blow (soft cover 4 m for 4 s, a
noise of 10 m, stuns a runner under it); Poison's bronze vats (tip 2 m out from an edge, crack what they land on, break a
formation's lock for 3 s, stay tipped until a rest) and the emperor's five basins. Seat's frozen falls (its Z4) are the
same shape played by the thaw. Today: `WaterJar` and `FireMechanicsKit.Jar` only. Suggested: one component: struck
from a side, a short fall along a path, a floor mark while it falls (S4), an effect on landing, a reset policy (S1).

**K20. Floor trap.** A trigger on the floor, a tell, an effect, a reset. Metal's jaw plate, pit plate, net, biting jar and
binding chest, re-armed by menders (its section 9); Ice's snares (hold 1.5 s; hold an enemy 3 s) and deadfalls (a 1.5 m
strip after 0.8 s), which catch enemies whose straight line crosses them; Poison's trap crossbows on the spirit ramp (a
tripwire drawn red, a lane 0.4 s after crossing). Today: none. Suggested: `Trap(trigger shape, delay, effect, tell,
reset, catchesEnemies)`, with "re-armed by" as an optional hook for Metal's menders.

**K21. Float, moving floor, ride on a line.** Something the player stands on that moves and carries them.
- Water: tethered floats that rise and settle with the tide (raft, pontoon, hull, canoe-deck raft; W4, C5).
- Metal: the counterweight lift (M6); ore carts on rails.
- Poison: the lantern punt (S1, "Pole the boat", 12 s, a moving float on a fixed line, C10).
- Rides out, all on a fixed line with no control: Water's canoe, Metal's chariot, Ice's longship down the fjord (C14,
  "Water's moving floors"), Poison's crane, L&W's storm cloud.
Today: none; nothing yet carries the player's `CharacterController`. Suggested: one carrier that moves whatever stands
on its deck with it (Water's C5), and a line follower that drives it; floats add a height from the level volume (S9).

**K22. Shelter marks.** The pieces that make a safe place and must agree with the safety test (S5). Fire's shade under
awnings, colonnades and roofs; Metal's lee behind walls from the giant's light, and the stelae in the hub courts; Ice's
lee behind hard cover from each perch; Poison's herb ring at every fire and the islands above the mist; L&W's pale gate
sills 1.6 m deep, the great gates' 3 m sills and the stone-lantern alcoves; E&L's rings of untouched heather round the
Hunt's stones (r 1.5 to 6); Seat's rings of thaw at the fires. Today: `Sunlight` shade rectangles registered through
`SunlightZones` and `ShadeRoof` (private). Suggested: each shelter piece registers its own shape in the scene's shelter
list as it is built (Fire's B12 and L&W's M24 say exactly this).

**K23. Shadow flyers and life tells.** Living things drawn mostly by their shadows, and creatures that react to a
land's warning. Fire's kites and Water's frigate birds (`CirclingBirds`, built); Metal's twelve bronze birds in the trial,
seen as a ring of shadows; L&W's crows that lift off every gate top on the race's first far beat, deer and macaques that
bolt uphill; E&L's flocks crossing the frame north to south, shadows first, on a brawl beat. Today: `CirclingBirds`.
Suggested: give `CirclingBirds` an event to "lift" or "cross" on cue, so S3's clocks can drive it.

---

## 2. Per-land unique pieces

What only one land needs (each plan's build list has the full parameters).

| Land | Unique pieces |
|---|---|
| Fire | Mirror stands and gold discs (B10, built as `MirrorStand`, `MirrorTarget`); the heat swell by district (B14, built); the champion's bowl (B15); the arcaded quay face (B5); bridge-house and roofed passages (B16); the burnt houses (B17); oil jars (B9) |
| Water | Lagoon water at two tides (W1); tide stones (W2); fishpond wall and sluice (W5); stilt houses and decks with stilt screens (W6); gill breath emitter (W8); the Gash with its anchors and the eye under the window (W10); sea wall and raft slip (W13); jaw bridge (W14); canoe sheds and slipways (W15); hook stones, star stones, hook arch (W17); canoes in trees (W18); the Old Reef islet |
| Metal | The rampart and the Round with the Tread (M1); maze walls with the cutaway (M2); bronze leaves in grooves (M3); rails and carts (M4); the crane with its hanging block (M7); cradles (M8); hub courts and plaque tables (M9, M10); grate floors over lit trenches (M11); the Pour Way (M12); forge pieces: crucible, Runs, quench troughs, gantry and ladle, bellows, Cyclopean furnaces (M13); the Hall of the Handmaidens (M15); the forge room (M16); the Field with its gongs and chimney (M17); the Landing (M18); watchdogs; Ariadne's thread |
| Ice | Longships in three forms (I1); fjord ice and pressure ridges (I2); the open lead (I3); nausts (I4); buried longhouses and trenches (I5); the hall shell (I6); frozen falls (I7); spruce and laden spruce (I8); ski runs (I10); the hunting stand (I11); ribs, spine, vertebrae, neck drums and horns (I14 to I16); the alpha's ground (I17); the hollow (I18); the lake (I19); the thawed fjord for the ride out (I21); the edge glint and state ring |
| Poison | The miasma (P1); walkable roofs (P4); kerb strips under eaves (P5); humpback bridge (P6); lashed boats (P7); city walls with walks (P8); drain grates (P12); pit trenches (P13); mercury (P14); shaft cascades and the pour (P15, P16); the emperor's island (P18); the stair of long life and the tower top (P19); the jar (P20) |
| L&W | Racecourse legs with their lanes (M1); torii in five forms and the great torii (M2); platforms, turns and crossing precincts (M3 to M5); the shrine on piles (M7); surf and wash (M8); the drowned town (M9); the village (M10); cedars, maples, pines, bamboo bands (M11); the temple (M12); the lantern graveyard (M13); the pagoda (M15); crags (M16); the split summit (M17); the screen room (M18); `StormRoof` and the cloud islands (M19); the hermit's cave (M21); the Messengers' Course (M22) |
| E&L | The bog (G2); the stone causeway (G3); hurdle ways and briar (G4); barrows with entrance stones (G5); the hollow way (G7); standing stones, King's Men, the Hunt's stones (G8); the Giant's Dance (G9); roundhouses with trees (G10); year-variant living things (G11); the Feast Yard and cauldron (G13); the sacred grove (G15); the plain (G16); the rise and the bowl (G17); the far set (G18); the Whispering Knights' platform (G19); the crannog (G20) |
| Seat | The thaw look (Z1); frozen ranks, instanced (Z2); volleys as data (Z3); the wave (Z6); the bed of arrows (Z8); the Stutter (Z9); the council (Z10); the domain (Z11); the Chakravyuha if wanted (Z13) |

---

## 3. Shared behaviours (systems, not geometry)

### A. World state

| # | Behaviour | What it must do | Lands | Today |
|---|---|---|---|---|
| S1 | **Keys and reset policy** | Every changed thing (opened, broken, tipped, fallen, grown) keeps one of: for good (a progress key, across falls and sittings), until a rest, or reset at a fall. E&L names it outright: things tagged *Stone* keep their state, things tagged *Grown* reset at every rest | all 8. For good: every shortcut, E&L's Earth side. Until a rest: Poison's broken statues and tipped vats, Metal's tipped cradles and the layout, E&L's briar and wicker. Reset at a fall: Seat's bearers, volleys and falls | Per-component keys (`Ladder.DownKey`, `BarredDoor.OpenKey`, `BreakableWall.BrokenKey`), `Progress`; `AreaDirector.Rest` brings enemies back. No shared policy |
| S2 | **Land state and its variants** | One saved number per land that many objects show in different forms, broadcast on change | Water (the tide, `water.tide`, two states); E&L (the year, `earthlife.year`, four); Metal (the maze layout, three, not saved); Ice (the thawed fjord for the ride out) | None |

### B. Clocks, warnings and fairness

| # | Behaviour | What it must do | Lands | Today |
|---|---|---|---|---|
| S3 | **Land clock** | A rhythm with phases (calm, warning, event, falling), a rate per district, first events placed or forced by a trigger, a restart at a rest with a quiet start, and events others listen to | Fire (heat swell); Poison (the hours: calm, first gong, rising, held, drum, falling; yellow every third in D2); L&W (the race: interval, far and near beats, lead per diamond); E&L (the Hunt's horns by band; the brawl beats every 45 to 75 s); Ice (spindrift every 18 s); Metal (the giant's 150 s lap and the Tread; bellows on every second footfall; the ladle's pour); Water (the gill's breath every 8 s); L&W (the surf's 7 s wash) | `HeatSchedule` (plain numbers: first, every, warning, swell), `NoonHeat`, `LandZones` districts and forced rectangles. Poison says build its clock on `HeatSchedule`; L&W proposes its own `RivalsRace`. Suggested: one clock type for all |
| S4 | **Warned strike** | A shape on the floor (lane, ring, strip, comb, front) drawn before it lands, a warning time, the strike, a damage profile (damage, pin, stun, shove along a line, knockdown, carry), what blocks it (cover, shelter), whether it also hits enemies, and a guard rule per source (pale, red, perfect only) with its own perfect window and reward | Ice (her arrows: a fixed crimson line, 1.0 s); Metal (the giant's stone: a growing shadow 1.6 s; footprints 0.8 s; the birds' feathers 0.8 s); L&W (the two sweeps up a lane on the beat); E&L (the Hunt's front at 16 m/s); Seat (thawed arrows; frozen falls marked 1.6 s); Poison (volley combs; trap crossbows); Water (the harpoon's 12 m lane); Fire (the champion's spear lane) | `GroundMark` (an outline and a growing fill), `AreaDirector`'s storm strikes (warning, radius, damage), `SpearRay` and `SpearCast` lanes, `LightningBolts` (the look) |
| S5 | **Shelter and quiet areas** | One list per scene, filled by the pieces that make shelter (K22, K14), read by every threat: "is this point safe from this threat now" | Fire (shade); Metal (lee from the light; no throws near fires, in the elite's hall or during the Tread); Ice (lee from each perch; no perch sees near fires; never hunts a shortcut); Poison (above the mist; the herb ring); L&W (sills, alcoves, crossings, calm zones C17); E&L (stone rings; the brawl never in an arena); Seat (fire rings) | `Sunlight` and `SunlightZones` (Fire's shade) |
| S6 | **One red at a time, narrow footing, quiet waking** | A land's sweeping threat takes the same attack token the enemies use, so two unguardable things never land within about 1.2 s of each other; nothing sweeps a ladder, chain, rope bridge or rib top; a quiet span after waking | Ice (her arrow takes the pack's token; no arrow on a climb; unaware 10 s after a fall); L&W (the near count belongs to the race; no race on narrow footing; first race 20 s after a fire); Seat (arrows hold their tremble near a red attack; no volley over a ladder); E&L (no brawl beat during a red attack); Poison (first gong at least 60 s after waking) | `EnemyTurns` (how many may attack at once); hazards do not take its token |
| S7 | **Sound from a bearing** | Play a cue placed in stereo from a point or a bearing, even far off screen, at a volume that grows as it nears; muffle it underground | Metal (footfalls every 2.4 s, the anvil); Ice (her draw's creak from her side, each howl from its howler); L&W (the count from the gates below); E&L (brawl beats from the meeting ground; the horns); Poison (the gong everywhere, muffled in the tomb; the watchman's clapper) | None (`Runtime/Audio` is empty) |
| S8 | **Tremor, and a shake setting** | A small camera shake with dust shaken off tops, rings on puddles and loose stones falling, **off when the player turns shake off** | Metal (within 30 m of a footfall); E&L (brawl tremors, stronger nearer the meeting ground) | `TopDownCamera` shakes on heavy blows; there is no setting to turn shake off, which both plans assume |

### C. Ground, water and liquids

| # | Behaviour | What it must do | Lands | Today |
|---|---|---|---|---|
| S9 | **Level volume and depth** | A surface whose height moves between levels by region and by clock or state, with a bright edge where it meets the floor; a query "how deep is it here now" used by the player, enemies and floats; a bite or slow from the depth | Water (the lagoon at y 0 or y 2.0 over 6 s; the depth table); Poison (the mist's top by district and hour; the tomb's pour filling to a brim); E&L and L&W use the depth query on still water (the bog, the surf) | None |
| S10 | **Throw-back** | Stepping, rolling or being dragged into deep liquid puts the player back on the last firm ground after 1.2 s, with damage and no Shards lost; enemies that fall in are taken and count as killed, unless at home there | Water (10 damage; the drowned and the reef-grown are at home); Poison (deep mercury, 12 damage); L&W (the sea); E&L (bog pools and the mere, 10) | `FallCatch` (puts the player back on the last firm ground, no damage) |
| S11 | **Surface tags** | A tag on every floor setting walk speed, roll length, noise and footstep sound | Ice (deep snow 4.5 m/s, roll 3.5 m, loud; hard ground silent); Water (wading 4 m/s, roll 3 m); E&L (the plank road's sunk stretches, wading); Poison (shallow mercury: 70% speed, rolls slide 1 m further); L&W (the wash, 20% slower) | None |
| S12 | **Noise rings** | Noise events with radii, heard by listening enemies and drawn as faint rings on the floor | Ice (decided: the owner's answer 4); Seat (carried over: missed arrows are noise to the buried inside the thaw) | None |
| S13 | **Floor fluids** | Something poured or burst that spreads, runs downhill, fills low places and bites or slows | Fire (gold runs, molten floor); Metal (slag pours, the Runs, the wheeler's tip); Poison (the vessel's heavy cloud flowing downhill and off edges; the pour; mercury beads rolling in grooves); Seat (the crucible's pour) | `MoltenFloor`, `GoldRuns`, `ScorchedGround`, `FlameField` (Fire) |

### D. Movement

| # | Behaviour | What it must do | Lands | Today |
|---|---|---|---|---|
| S14 | **Carriers** | Moving floors carry characters standing on them; a line follower drives a carrier with or without control | Water (floats), Metal (lift, carts), Poison (punt, crane), Ice, L&W, Metal and Water rides out (K21) | None |
| S15 | **Shove and gust fields** | A region or lane that pushes characters a set distance in a set direction, with a warning | Water (the gill's breath, 2 m out and 1 m in); Metal (bellows gust lanes; the mender's puff); L&W (Kazehaya's gust shoves 4 m down the leg; conch wind; fan crescents); Ice (the alpha's squall) | `IShovable` |

### E. Drawing and the camera

| # | Behaviour | What it must do | Lands | Today |
|---|---|---|---|---|
| S16 | **Fade rules beyond the occlusion line** | Besides fading what stands between camera and player: fade by nearness (canopies within 6 m); cut away the band of a wall within a few metres south of the player, keeping its top as an ink line; hide a shell or roof while the player is inside it; draw a thing only as its shadow while the player's floor is below it; dither things hanging above a height | Metal (maze walls cut away above 1.2 m within 5 m south; the giant dithers above its shins; the culvert's deck); E&L (the barrow's mound shell, passage walls with inked tops); Ice and L&W (canopies within 6 m); L&W (gate beams within 4 m south or overhead; the screen room's roof, its question 6); Poison (the planks from below; the tomb's vault; the jar's south arc); Seat (hanging things above 2.5 m within 4 m south) | `Fadeable` and `OcclusionFader` (the camera-to-player line); `ShadowCastingMode.ShadowsOnly` used in `SunCityBuilder` |
| S17 | **Framing and held shots** | The camera side of K13: per-zone framing and direction, conditions, a land-wide or area-wide framing, scripted held shots | all 8 | `TopDownCamera.SetFraming`/`ClearFraming` |

### F. Enemies

| # | Behaviour | What it must do | Lands | Today |
|---|---|---|---|---|
| S18 | **Dormant figures and statue states** | Enemies that stand as statues or frozen figures: some take blows and crack without waking, some are hard cover until broken, some wake as the player passes, some show a tell before waking | Poison (clay statues between hours); Ice (dormant brittle as hard cover); L&W (fox statues waking through the gate); Seat (frozen bearers with the stir); E&L (respawns climbing out of their own stone or thorn mound); Metal (the bronze-veined set as a crust statue) | `Enemy.MakeDormant(wakeWithin, clip)`, `WakingNotice` |
| S19 | **Per-enemy local time** | Each enemy reads its own time scale (frozen, slow, full), not `Time.deltaTime` | Seat (the thaw: 0, 0.1 or 1 per object; its plan calls this the land's main engineering cost); L&W's Overclock already slows enemies | `WorldClock.EnemyScale`, one value for all enemies, applied in `Enemy` and the bosses. About 22 scripts in `Runtime/Gameplay` read `Time.deltaTime` directly, among them `AshenBurst`, `BuriedMound`, `ShieldWall`, `WickFlame`, `ThirstDrinker`, `SpearCast`, `GoldRuns`, `MoltenFloor` |
| S20 | **Route followers** | Enemies and threats that move on a route the builder lays down, with no navmesh | Metal (the rail graph for wheelers, menders and carts; the giant's round); Poison (the clay's watch routes and formations; the roof islands' stair feet); E&L (the haulers' track; the Hunt's front); L&W (the racers' lanes; the messengers' paths) | None (`Enemy` walks straight at the player) |
| S21 | **Formation lock** | Neighbours lock and move in step; an officer closes gaps | Fire (levies); Poison (the clay, wheeling 30 degrees a second); Seat (a formation woken in part) | `ShieldWall`, `FireMechanicsKit.WallOfShields` |
| S22 | **Watcher sight** | A watcher above the player sees along a line, blocked by cover, and finds the player only after unbroken sight for a moment; the lee behind cover can be checked at build time | Metal (the giant's gaze: lit for 0.5 s is seen); Ice (the huntress: 1.0 s unaware, 0.6 s searching; the build-time lee map C2); E&L (the King's Men read the player's facing instead, 110 degrees, 15 m) | `SunSense` (Fire's sun-blind priest finds someone lit without a break for a moment) |
| S23 | **Lines out and back** | A thrown thing that travels a lane and comes back, or a line staked between thrower and target that can be cut | Fire (the champion's spear and recall; spearmen); Water (the harpoon and reel; the hooked's yank along a line, cut the line; the hook brother); Metal (the chained's lash to the circle's edge); Seat (the hooked harpooner, reused) | `SpearCast`, `SpearRay` (enemy side); `ChainThrow`, `ChainLine` (player side) |

---

## 4. Build order

The order puts first what the most lands place, then the systems the hazards hang on, then the narrower pieces. Sizes
are rough: small (a component or a helper), medium (a component with a builder and a check), large (a system several
others depend on).

| Step | What | Size | Unblocks | Needs first |
|---|---|---|---|---|
| 1 | **One shared land kit**: move `SunCityBuilder`'s private helpers (K3 `Stairs`, K5 `Ladder`, K6 `Barred`, K14 `Rest`, K16 `Carving`, `ShadeRoof`, `Floor`, `Box`, `Mass`, `Wall`, `Edge`, `Disc`, `Rubble`, `Scatter`, `Stain`, `Solid`, `Tilt`) beside `SunCityGround` into one public kit, and point the other builders' private copies at it | medium | all 8 | |
| 2 | **Per-enemy time hook** (S19): one time scale per enemy, read by every enemy behaviour instead of `Time.deltaTime` | small now; large if left until Seat, as each new land adds enemy scripts | Seat; keeps L&W's Overclock | |
| 3 | **K1 to K4 extended**: plateau face styles, stairs with landings and long flights, the rim with named gaps; **K8** drops with recorded landings | small to medium | all 8 | 1 |
| 4 | **Keyed opener family** (K9 with S1's reset policy): `KeyedChange`; `Ladder` with up and down times and variants (K5); barred doors (K6); seals with several keepers (K7); breakables with event triggers (K15) | medium | all 8 | 1 |
| 5 | **Framing zones and held shots** (K13, S17) | medium | all 8 | |
| 6 | **Rest fires and the shelter list** (K14, K22, S5) | small to medium | all 8 | 1 |
| 7 | **Warned strike and the shared red token** (S4, S6), with the narrow-footing tag | large | Ice, Metal, L&W, E&L, Seat, Poison, Water, Fire | 6 |
| 8 | **Land clock** (S3) on `HeatSchedule`, with phases, district rates, forced firsts, rest restart and events | medium | Fire, Poison, L&W, E&L, Ice, Metal, Water | |
| 9 | **Sound from a bearing, tremor, the shake setting, life tells** (S7, S8, K23) | medium | Metal, Ice, L&W, E&L, Poison | 8 |
| 10 | **Surface tags and the throw-back** (S11, S10) | medium | Water, Ice, Poison, L&W, E&L | |
| 11 | **Spans, wreck decks, clefts** (K10, K11, K12) | medium each | Water, Metal, Ice, Poison, L&W, E&L, Seat, Fire | 4 |
| 12 | **Fade rules beyond occlusion** (S16) | medium | Metal, E&L, Ice, L&W, Poison, Seat | |
| 13 | **Level volume and height bands** (S9, K17) | large | Water, Poison (and the depth query for E&L, L&W) | 10 |
| 14 | **Carriers** (S14, K21) | large | Water, Poison, Metal, and every ride out | 13 for floats |
| 15 | **Struck props, traps, noise rings, floor fluids, shove fields** (K18, K19, K20, S12, S13, S15) | medium each | two to four lands each | 7 |
| 16 | **Enemy systems**: dormant statue states, route followers, formation lock in part, watcher sight, lines out and back, land state variants (S18, S20, S21, S22, S23, S2) | medium to large each | per land, as its enemies are built | 2 |

**The five to build first:**
1. The shared land kit (step 1): every land places stairs, ladders, doors, rest fires and carvings, and today they are
   private to one builder and copied into four others.
2. The keyed opener family (step 4): every land's shortcuts, all eight, on one behaviour with one reset policy.
3. Framing zones and held shots (step 5): every land has six to ten vistas and three or four held shots.
4. The warned strike with the shared red token and the shelter list (steps 6 and 7): the sweeping threat of six lands
   (her arrows, the giant's stones, the race, the Hunt, the thaw's arrows, the clay's volleys) is one shape drawn on the
   floor before it lands.
5. The land clock (step 8): seven lands run a rhythm with warnings, and Fire's `HeatSchedule` is already its core.

The per-enemy time hook (step 2) is cheap now and costly later; it is worth doing alongside the first five.

---

## 5. Where plans differ: questions for the owner

Each gives what differs, then a recommendation. None is decided here.

**Q1. How many held shots does a land get?** The owner chose three for Water (its answer 6, "one per district"). Poison,
L&W and Seat carry "three per land" as a rule of the game. E&L proposes four (its question 6: the arrival, both seats and
the brow), because its decided brawl views already need three and it has four districts. Two of the three in Poison and
L&W are rides out (the crane, the storm cloud), and Ice adds a 60 s held ride down the fjord on top of its glimpses.
- (a) One held shot per district, as the reason given for Water; rides out do not count. Water 3, E&L 4, the others as
  planned. **Recommended**: it keeps the owner's own reason and settles E&L without a special case.
- (b) Three per land at most, rides included. E&L drops one; Poison and L&W keep one shot inside the land.
- (c) Three per land, rides not counted. E&L drops one.

**Q2. Roofs over walkable floors: shadow-only or fading?** Fire's second pass (2026-11-17, built) draws a roof the player
walks under shadow-only: the hall, the colonnades (B11), the bridge-house over the Lamp Street with the roof walk above it
(B16), the gatehouse arch. The rule decided on 2026-11-18 evening says nothing walkable lies under a roof and roofs only
fade, and Poison asked for and got an exception for its plank bridges only (shadow-only from below). The shared
overhead piece cannot have one behaviour for both.
- (a) One overhead piece with two modes; fade is the default everywhere; shadow-only is kept only where the owner has
  allowed it (Poison's planks), and Fire's hall, colonnades and bridge-house are checked against the rule when Fire is
  next revised. **Recommended**: the later rule wins, and Fire's shade mechanic can still use fading roofs, since the
  shade test reads rectangles, not the drawing.
- (b) Fire's shadow-only overheads stay as an exception for Fire, as Poison's planks are for Poison.

**Q3. Warned arrows: one piece, three guard rules.** Ice's arrows are red: only a perfect guard stops one, its window
0.10 s against a blade's 0.18 s (the owner: not easy, several to fill her bar); 40 damage and a 1.0 s pin; the line laid
1.0 s ahead and fixed. Seat's thawed arrows are pale: guardable from the front, a perfect guard reflects, the window not
stated; 12 damage and a flinch. Metal's feathers are called "the huntress's warned arrows made small" but their line is
0.8 s ahead, they stick as slowing spikes, and their guard rule is not stated.
- (a) One warned-arrow piece with the guard rule and window per source: Ice as decided; Seat's walk arrows pale with
  the standard 0.18 s window (they are many and the walk is a rehearsal); Metal's feathers pale, since the trial's skill
  is the gongs, not the guard. **Recommended**.
- (b) Every arrow in the game uses Ice's rule (red, 0.10 s). One rule to learn; Seat's walk and Metal's trial become far
  harder.

**Q4. The Wild Hunt in two plans.** E&L (its section 3.5): a front 6 m deep sweeping at 16 m/s on a horn clock; shelter is
a ring of untouched heather round each stone (r 1.5 to 6); caught, 25 damage that cannot kill and set down behind the
plain's west wall (its question 2, open). Seat (3B, written before E&L's plan): single riders at 9 m/s thawed by the
bubble; shelter is the 2 m west of each stone; caught, 20 damage and carried 4 m south.
- (a) Seat's frozen Hunt uses E&L's pieces and rules (the rings of heather as shelter, its damage and its carry), scaled
  to the thaw: the riders still come a few at a time from the bubble's edge. **Recommended**: the recap reuses each land's
  people "with their own mechanics unchanged" (Seat 1.6).
- (b) Seat keeps its own lighter version, as a different moment of the same Hunt.

**Q5. A lightning strike, three ways.** L&W's bolt sweeps a lane up a leg: 28 damage and a 0.5 s stun. Seat's frozen
bolts strike a ring 2 m round a scorch: 35 damage, after a 0.5 s tremble. The storm strikes already built for the castle
and summit (`AreaDirector`) strike a ring 2.8 m round: 18 damage after a 1.3 s warning.
- (a) One strike piece (S4) with the shape and numbers per use, and one look (the cyan fork of `LOOKS.md` 8); Seat's ring
  stays a ring because it is not a race. **Recommended**.
- (b) Seat's bolts sweep a short lane, as L&W's do, so the echo of the race reads as the race.

**Q6. Slow water.** Water: under 0.3 m is as on land; 0.3 to 1.0 m is wading (4 m/s, roll 3 m). E&L's sunk plank
stretches are 0.3 m under water and called wading, exactly on Water's boundary. L&W's surf is "knee-deep, slow" with
its wash 20% slower (4.8 m/s). Poison's shallow mercury is 70% with a sliding roll. The throw-back costs 10 in Water and
E&L, 12 in mercury, and is not given for L&W.
- (a) One depth rule from Water for all water; E&L's sunk stretches set a little deeper so they are plainly wading; L&W's
  wash kept as its own named surface; mercury its own; the throw-back 10 for water, 12 for mercury. **Recommended**.
- (b) Each land keeps its own numbers.

**Q7. Drops: how high, and where they must land.** Every plan's measures repeat Fire's "a drop 2 m". Water's Cut 2 drop
is 1.4 m; Poison and E&L allow 2 to 4 m; L&W 2 to 2.5 m and at most 3. The landing rules differ: Poison wants a climb
point within 10 m; L&W and E&L a way back within 30 m; Water ground that reaches a tide stone at either tide; Fire states
none.
- (a) One drop piece taking its height (1.4 to 4 m) and each land's landing rule checked in the editor from the recorded
  landings. **Recommended**: the plans already agree on facing south and on never stranding; only the numbers differ.
- (b) One height (2 to 3 m) for every land, and one landing rule (a way back within 30 m).

**Q8. Watcher sight: a light or a ray.** Metal: the giant's sight is a real shadowed spot light, and what is lit is what
is seen (the owner's answer 3). Ice: her sight is a line test from her eye to the player's chest against cover, with a lee
map worked out at build time from each perch, and the snow's shadows laid to agree with it. They answer the same question
(cover from a watcher above) two ways.
- (a) Both use the same line test and the same build-time lee check; Metal's light is still real and is made to agree
  with the test (the light and the ray start at the same point, as Metal's plan already says). **Recommended**: one tested
  system, and Metal keeps the owner's light.
- (b) Each land keeps its own: Metal tests by the light's shadow, Ice by rays.

**Q9. Seat's people from lands planned after it.** Seat was written before the L&W and E&L plans and guessed their
people. Besides the Hunt (Q4): its storm samurai is "a fast draw and a charge", where L&W's struck samurai are armour
charged by a bolt with an arc between the charged; its fox guard is single and wakes at 8 m, where L&W's come as a pair
with a double and wake through a gate; its crow-tengu's gust shoves 3 m, where L&W's gust shoves 4 m.
- (a) Bring Seat's three L&W people and its E&L people into line with their own plans, as its own section 1.6 intends.
  **Recommended**.
- (b) Keep Seat's simpler versions for a short recap.

### 5.1 Open owner questions in the plans that change a shared piece

These are already asked in their plans; they are listed here because a shared piece waits on them.

| Plan, question | Shared piece it changes |
|---|---|
| L&W 1 (what a perfect dodge through the race gives) | S4's per-source reward |
| L&W 2 (the count and the half-beat lead) | S3, S7 |
| L&W 4 (both elites before the screen room) | K7 (several keepers) |
| L&W 6 (the screen room's roof) | S16 (hide a roof while inside) |
| E&L 2 (where the Hunt sets the player down) | S4's carry; Q4 |
| E&L 3 and 4 (how many years, how they read) | S2 (the number of states and whether they wrap) |
| E&L 6 (held shots) | K13; Q1 |
| Seat 1 (the land-wide framing) | K13, S17 |
| Seat 2 and 3 (what moves in the thaw; whether woken enemies freeze again) | S19 |
| Seat 4 (the Chakravyuha) | Seat's frozen ranks with hulls (unique) |

### 5.2 Where an owner's answer and a plan's body still disagree

Not questions: the answers win, and these lines are to be brought into line when the plans are next touched.
- **Metal**: the owner chose Ariadne's thread (answer 2), but the build list still says "G6 Ariadne's thread (if chosen)".
  The owner replaced the bronze bulls with the bronze birds (answer 1), but 15.3's new kit still lists `bronze_bull`,
  and the enemy count in section 16 still counts "2 bulls, 6 sown men".
