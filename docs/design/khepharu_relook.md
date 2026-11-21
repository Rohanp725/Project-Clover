# Khepharu: the relook (brief for the Unity and Blender workers)

Written 2026-11-20, offline, from the design docs, the code and the frames already in `review/shots/current/`. Nothing
here was run in the editor. Line numbers are as of commit `21319c7`.

**Why.** The owner played the fight and said, word for word: "there needs to be a high detail pass on the second boss
fight kepharu, there is a bunch of issues with it being not as detailed as it needs to be, with the arena, the
background, and parts of the sun clipping through the floor, and then when its a ball it rolls through parts of the
beetle. It needs a in depth close relook and update on all the visuals and the moves, so it looks and feels like a cool
fight and not just a fight with cool concepts but not epic and immersive".

**The bar this brief holds to** (owner's standing rules): no flat square areas, real level design; no reskinned
attacks, and boss attacks that show the event happening over time; fire looks like real flame, never white or gold
light, never pure white; scenery is walked, not just framed (no overhangs, no walk-through props, no fall-outs);
pieces worn on the body sit on the body.

**How to read it.** Section 1 says what is built and where. Section 2 lists the faults, with the cause in the code
where it can be seen. Section 3 says honestly why the fight is "cool concepts, not epic". Section 4 is the research.
Section 5 is the work list. Section 6 is what the Unity worker films and measures first, before anything changes.

---

## 1. The fight as built

### 1.1 Where things live

| What | Where |
|---|---|
| Design: the court, the man, the shadow, the form, the model pieces | `docs/design/fire_land.md` 3.6 (lines 367-400), 6.1-6.6 (701-1007) |
| What was built, and how it departs from the design | `fire_land.md` "As built: the court" (1414-1523) and "the fight reworked" (2172-2357) |
| The attack-visuals plan (rules for marks and effects) | `docs/design/fire_boss_visuals.md` 0-2 (27-200) |
| The approach stair, planned but **not built** | `docs/design/fire_levels.md` 3.5 "The Way of Noon" (634-660). No builder code mentions it |
| Owner decisions on Fire | `docs/design/WORLD.md` 120-176 (no face on the scarab; fire not light; sun parry stays) |
| The player's guide to the fight | `docs/GUIDE.md` 10 (1346-1415) |
| Sound | `docs/GAME.md` 336-342: put off on purpose by the owner on 2026-11-15. The game has **no audio at all**: `Audio/*` folders are empty, `Scripts/Runtime/Audio/` is empty, no `AudioSource` in runtime code |
| The scene builder | `Scripts/Editor/Builders/SunCourtBuilder.cs` (menu `Clover/Build/Sun Court`) |
| What lies beyond the walls | `Scripts/Editor/Builders/SunCityBeyond.cs` `Court` (91-147) |
| The form's builder | `Scripts/Editor/Builders/SunBearerBuilder.cs` |
| The court's director (line, words, shadow, camera, transformation, sunset) | `Scripts/Runtime/World/SunCourtDirector.cs` |
| The man | `Scripts/Runtime/Gameplay/EnemyKind.Fire.cs` 54-113; clips forged in `Scripts/Editor/Forge/SunKingAnimations.cs` |
| The form | `Scripts/Runtime/Gameplay/SunBearer.cs` (core), `.Body.cs` (keep-inside, the ball, shell fire, camping), `.Ground.cs` (dig, ridge, Burrow, fetch), `.Moves.cs` (Ricochet, Flares, Gust, Grab, Sunfall) |
| The form's choices | `SunBearerChooser.cs` (opening order 17, shares 24-25, Zenith gap 33) |
| The loose sun as a ball | `RollingSun.cs` |
| The form's effects that are not particles | `SunBearerLook.cs` |
| The shadow | `ShadowHand.cs`; turned in `SunCourtDirector.TurnCourt` (632-638) |
| Camera | `Scripts/Runtime/Player/TopDownCamera.cs` (pitch 55, distance 12 by default, lines 14-16); court framing in `SunCourtDirector.Frame` (606-629) |
| Light | `LookBuilder.Noon` (`Editor/Builders/LookBuilder.cs` 88-101) |
| Probes | `AgentScripts/SunCourtProbe.cs`, `SunFightProbe.cs` (parts listed in `AgentScripts/README.md` 401-438), `BossFxProbe.cs`, `profile_court.sh` |

### 1.2 Phase one: the man with the Sun-Spear

Khepharu as a man: `boss_sunking.fbx` at scale 1.25 (about 2.6 m), on the boss skeleton. 480 health, 170 balance,
preferred range 4.2 m. His clips are **forged poses** (keyframed in code, `SunKingAnimations.cs` 107-463), not authored
animation. The fight starts when the player crosses the gold line at z -20 (`SunCourtDirector.cs` 360-369): a wall of
heat closes and a click-through box shows his four lines. He is standing at (0, 0, 8) and waits.

Each move: name, tell (the time the mark shows), mark, damage, the answer, and the opening. Data in
`EnemyKind.Fire.cs` 92-113.

| Move | Trigger | Tell, then travel and after | Mark | Damage | Answer and opening | Effects now |
|---|---|---|---|---|---|---|
| Thrusts 1, 2, 3 (92-94) | Player 2 to 5.2 m | 0.50 / 0.34 / 0.55 s; strike 0.12; lunge 1.0 / 1.0 / 1.6 m; the third aims until 0.8 | Pale lanes 5.5 x 1.1 m | 15, 15, 22 | Guard all three, perfectly if possible. Open 0.8 s after the third | Gold streak and tongue (second pass); `sunking_thrust_*` forged |
| Drive, then drive-sweep (112-113) | Below half | 0.55 s, lunge 1.6; then 0.7 s sweep, no rest | Pale lane, then pale half circle | 22, 20 | Guard. Open 0.75 s after the sweep | As thrust and sweep |
| Reach (96) | Player 3.5 to 7 m, 3 s cooldown | 0.55 s, lunge 1.8 | Pale lane 7 x 1.0 | 20 | Sidestep in. Open 0.6 s | As thrust |
| Sweep (98) | Player inside 3 m | 0.70 s, strike 0.18 | Pale wedge 3.6 m, 180 degrees | 20 | Guard | Gold crescent |
| Butt (100) | His counter after two guarded blows | 0.30 s | Pale wedge 2.2 m, 70 degrees | 10 | Guard or step back | Tongue |
| Throw (103) | Player 6 to 16 m, 8 s cooldown | 0.90 s, aim locks at 0.7; the spear flies and stands | Red lane 14 x 1.2 | 26 | Step off | Spear flies with a fire trail and stands smoking (`SunCourtDirector.WatchSpear` 641) |
| Returning ray (104) | Always after the throw | 1.8 s bare-handed, hand raised; strike 0.15 | Red lane 14 x 2.2 from him along the throw | 30 | Do not stand on the line. **The biggest opening: 1.8 s, no guard** | A jet of fire from blade to hand (`SunCourtBuilder.RayLook` 311-349) |
| Plant (106) | At half health at once, then 18 s cooldown | 1.1 s, then 8 s of burning | Red circle 5 m round him (design wanted a ring 7 m out) | 22, then 10 every 0.5 s | Get out and stay out. Open as he pulls the spear free | A wall of 90 flame tongues round the edge (`SpearLook.ConfigurePlantFire`) |
| Noon shafts 1, 2, 3 (108-110) | Player 6 to 20 m, 9 s cooldown | 0.85 / 0.50 / 0.50 s | Red circles 2.1 m, on the player; never laid in the shadow | 18 each | Keep walking, or stand in the shadow | A shaft falls from the sky |

At half health he says "It is always noon.", plants at once and gains the drive. The camera for the duel is 1.3 times
the usual distance and 8 degrees steeper (`SunCourtDirector.cs` 115, 610).

### 1.3 The change (4.6 s)

`SunCourtDirector.AdvanceTransformation` (381-436) and `SunBearer.AdvanceEntering` (1079-1150). He gets up, lifts the
spear, a column of fire stands on its point (a camera-facing quad, `SunBearerBuilder.cs` 117-122), the sun comes down
it (from 1.1 s, lands at 2.1 s, hangs 4.6 m up), he burns and is hidden, the scarab grows out of the floor **by being
scaled up from nothing**, takes the sun between its forelegs, and roars at 3.5 s. Only then are its name and bar shown.
The player is held and cannot be hit. The camera holds both at 1.5 times distance.

### 1.4 Phase two: the Sun-Bearer

One creature, two shapes, one at a time: the scarab (6 m long, on the floor) and the falcon (14 m across, circling the
north rim 12 m up). 560 health, 160 balance. The sun is a 3 m ball and the shared piece: it is the blow in most attacks;
knocking it away (a perfect guard on it, or 30 damage to the body during one attack) leaves the shape exposed for 4 s.
The full rules are `fire_land.md` 6.4 (802-953). Fury below half: a quarter faster, rests 55%, changed patterns.

**The scarab's moves**

| Move | Code | Trigger | Timeline (tell / travel / impact / after) | Mark | Damage | Answer / opening |
|---|---|---|---|---|---|---|
| Roll | `AdvanceRoll` 1322 | Share 20 | Walks to the rim (0.3-1.4 s); lane follows 0.9, holds 0.4; sun rolls 12 m/s, scarab behind; stops hard at the far rim | Red lane 4 m wide | 28 | Step off; perfect guard knocks the sun; hit the flank as it passes. Lane burns 6 s |
| Furrows | same, `rollsInAll > 1` | Share 14 | Three rolls, 1.0 s apart; follow 0.6, hold 0.3; turns 180 degrees at each end, swinging the sun round itself (1413-1418) | Three red lanes | 24 each | Stand between lanes. Rests on the sun 2 s after the third |
| Ram | `BeginRam` 1457, `AdvanceRam` 1473 | Player within 9 m for 1.2 s | Leaves the sun where it lies; draws back 1.4 m; 0.8 follow, 0.3 hold; lunges in 0.2 s; horn stuck 1.8 s | Pale circle 3.4 m | 34 | Guard or walk out. **Head open 1.8 s** |
| Sparks | `AdvanceSparks` 1549 | Share 14 | Cases shaken; five circles 7 m out, each falls 1.0 s after drawn; 0.7 s later five more in the gaps | Red circles 2.2 m | 16 each | Step into the gaps |
| Ricochet | `Moves.cs` 48, 151 | Share 18 | Three red stretches of the bounce path drawn and filled over 1.5 s; sun shoved at 19 m/s, slowing; each stretch burns 3.5 s | Red lanes 3.4 m | 26 a stretch | Watch the bounce. Shell cool while the sun is away |
| Burrow | `Ground.cs` 191, 199 | Share 16 | Dig 0.7 s; ridge chases 2 s at 6.2 m/s; locks; red circle fills 0.75 s; bursts up; half out 1.8 s | Red circle 2.6 m | 32 and a 3 m shove | Move across its line. **Head open 1.8 s** |
| Flares | `Moves.cs` 260, 289 | Share 16 | Plants the sun 0.6 s (it swells to 1.2); three lines (five in fury, swept) fire 0.45 s apart, each with an arc that loops over and slams | Red lines 1.8 m wide, up to 15 m | 22 each | Stand in a gap or behind it. Shell cool |
| Shell flaring | `Body.cs` 326 | Player on the flank 2.5 s | Shell heats over 1.5 s; pale ring fills 0.55 s; fire thrown out | Pale ring 4.6 m | 16 and a 3 m shove | Guard or step off |
| Kick | `Body.cs` 368 | Player behind 2.5 s | Hind legs drawn up 0.45 s | Pale circle 2.3 m | 14 and a 4.5 m shove | Guard or step off |
| Rising (change to falcon) | `AdvanceChange` 1644 | Chooser | Rears 1.2 s with the sun overhead, throws it up, follows as the falcon (1.5 s) | In fury a red circle 6 m | 0; 20 in fury | Be 6 m off in fury |

**The falcon's moves**

| Move | Code | Trigger | Timeline | Mark | Damage | Answer / opening |
|---|---|---|---|---|---|---|
| Stoop | `BeginStoop` 1744, `AdvanceStoop` 1753 | Share 22 | Its shadow stops on the player and tightens; follows 1.0, holds 0.5; dives in about 0.24 s; stands on the sun 1.6 s | Red circle 5 m | 38 | Walk or roll out. **30 damage then knocks it off the sun** |
| Pass | `BeginPass` 1829, `AdvancePass` 1855 | Share 16 | Lane shown 1.2 s; comes down 6 m; flies it low in 1.0 s; climbs out. Fury: a second pass 0.9 s later | Red lane 5 m | 26 | Step off. Lane burns 6 s |
| Feathers | `AdvanceFeathers` 1940 | Share 12 | Centre, then ring of six 5 m out, then both turned; each falls 1.0 s after drawn | Red circles 1.8 m | 14 each | Step out, in; or stand in the shadow |
| Zenith | `BeginZenith` 1968, `AdvanceZenith` 1980 | Share 14, not within 25 s, opens the fury | Climbs over the obelisk; floor fills red 3.0 s; blaze 6 s (8 in fury); fury shadow stops 0.5 s and turns back | Whole floor red but the shadow | 12 every 0.5 s | Walk with the shadow. Then **Spent**: 2.5 s, head open (2084) |
| Ember Gust | `Moves.cs` 369, 389 | Share 16 | Comes round low (6.5 m); pale band 7 m follows 0.65 of 1.1 s; wall of fire crosses at 24 m/s | Pale band | 8 and a 4.5 m shove | Guard to hold ground. Leads to the Stoop |
| Talon Grab | `Moves.cs` 460, 467, carry 577 | Share 14 | Shadow tightens to a circle that follows 1.1, holds 0.45; dives 0.26 s; carries 8.5 m up 1.25 s; drops | Red circle 2.2 m | 6, then 30 | Roll out. **Missed: grounded 2.2 s, open** |
| Sunfall | `Moves.cs` 621, 628 | Share 18 | Hangs over the north tearing pieces off the sun; three circles 0.3 s apart, each falls 1.2 s after drawn; fury five in a closing ring | Red circles 1.7 m | 22 each | Move along the throw line. Leaves 1.3 m fires for 3 s |
| Setting (change to scarab) | `BeginSetting` 1733 | Chooser | Lands on the far rim and folds (1.5 s) | none | 0 | none |

**Shared states.** Exposed (2113): on its back or wings spread 4 s, then the scarab digs to the sun (or walks if within
7.5 m) and the falcon hops; then 12 s of poise. Staggered (2259): face down 5.5 s, head at 160%, a heavy blow is the
finisher. Parry (719-792): a perfect guard on the sun strikes it back into the form with a freeze and a slow. Death
(2283): 5.4 s; the sun gutters and goes out, the body is **squashed flat** (`squash` 2319) into ash, the dull sun rolls
into the shadow and cracks, and the man lies there.

**How it chooses.** A fixed opening of 20 picks shows all 18 moves within the first two turns of each shape
(`SunBearerChooser.cs` 17), then shares, nothing twice running, pairs (Ricochet into Ram, Gust into Stoop, Burrow into
Flares), a 2.4 s lull after two heavy attacks.

### 1.5 The arena and backdrop

- **Shape.** A flat round court, radius 20, at y 0. The floor is nine flat coloured discs stacked a few millimetres
  apart (`SunCourtBuilder.cs` 54-64, tops from 0.004 to 0.036), twelve gold bars, gold pips, and thin joints
  (66-97). No texture, no relief, no height anywhere on the fight floor. The real floor to stand on is an invisible
  120 m slab (49-50).
- **Wall.** 36 brick boxes, 3.9 x 3 x 1.4 m, round the north, east and west; 1.2 m on the south for the camera; open at
  the forecourt (100-113). A pale coping on top.
- **Props.** The obelisk at the centre (kit `obelisk`, 15 m, solid, thins when between camera and player, 158). The
  shade strip of the shadow (159-162). The seated colossus at the north, the kit's statue scaled 1.35, behind the wall
  at z 25.2, with two lotus columns (147-154): it is rarely in frame. A gilded gate leaf in the west wall for the
  sunset (117-144). The forecourt: house fronts, two braziers, jars, sand drifts (365-400).
- **Beyond the walls.** The court stands on a platform 18 m above a far town of plain boxes (houses, roofs, parapets),
  a west stair of 72 box steps, a road with braziers, a dry river strip and dunes (`SunCityBeyond.Court` 91-147). From
  the fight camera (63 degrees down) little of it is ever in frame.
- **Light.** `LookBuilder.Noon`: key light 1.5 at 78 degrees up (almost overhead), warm white; flat ambient; fog
  0.012; **a flat background colour** (0.78, 0.74, 0.66), no sky; saturation -10. The key light turns with the shadow
  (`SunCourtDirector.TurnCourt` 632-638). The sun ball carries an orange point light, range 9, no shadows
  (`SunBearerBuilder.cs` 157-166). The Zenith blends a warm volume (314-344); the sunset has its own (454-491).
- **Camera.** Pitch 55 + 8 = 63 degrees down. Duel at 1.3 x 12 = 15.6 m. Form fight at 2 x 12 = **24 m**, leaning up
  to 4.5 m toward the middle (`SunBearer.cs` 148). Entrance and death at 1.5 x (105). No cuts, only eases.

### 1.6 Models: what is Blender-made and what is primitives

| Thing | Source | Note |
|---|---|---|
| The man `boss_sunking.fbx` | Blender, 9,268 tris | `ASSETS.csv` row 17, **approval pending**; source `staging/blender/2026-11-17_fire/fire.blend` (not in `Art/Source/`, against the art rule) |
| The form `boss_sunform.fbx` | Blender, 17,608 tris, 20 rigid pieces, two 3-bone wings | Row 18, pending; same staging source; notes `staging/blender/2026-11-17_fire/notes.md` 125-167, 226-257. Legs are single rigid pieces "modelled bent". No skeleton for the body |
| The sun | Blender piece `sun` (sphere 3 m with twelve flame tongues reaching 2.7 m from the centre) and `sun_loose` | The builder swaps slot 0 to a molten material and hides the other slots (`SunBearerBuilder.cs` 67-74); the frames of 2026-11-19 (`suncourt_roll_2.png`) still show spikes round it |
| The corona round the sun | **A single camera-facing quad 5.8 m** (`SunBearerBuilder.cs` 217-220) | |
| Fire column, dive streaks, fallers, stun stars | Quads | |
| The burrow ridge | **A primitive sphere and six primitive cubes** (244-253) | |
| The falcon's shadow | A flattened cylinder disc (110-113) | |
| The court kit: obelisk, colossus, columns, gate kit | Blender `fire_kit.fbx` (row 19, pending) | |
| The court's floor, wall, gate pylons and leaf, forecourt walls, coping | **Primitives** (cylinders and cubes) | |
| The town below | **Primitive boxes** (`SunCityBeyond.cs` 185-200) | |
| The standing Sun-Spear | `WeaponBuilder` `sunspear` built of primitives | |
| Hot floor | Shader meshes (`clover_scorch`, `clover_flame_field`), one billboard tongue shape | |

---

## 2. Known faults

### 2.1 The owner's: the sun clips through the floor

What the code shows. Ranked by how likely each is to be what the owner saw.

1. **The corona quad goes below the floor.** The burning rim is one camera-facing quad, 5.8 m across times the sun's
   scale, hung 0.7 m behind the ball's middle away from the eye (`SunBearerLook.cs` 22, 504-505). The ball's middle is
   1.5 m up. At the fight camera (63 degrees down) the quad's lower edge sits about 0.4 m under the floor; at sun scale
   1.2 (Flares) and 1.5 (Zenith, in the air, harmless) it goes deeper. The floor cuts it along a hard straight line.
   This shows on every frame the burning sun is on the floor, and most when it rolls.
2. **The flame tongues spin into the floor.** The model's sun has twelve tongues round its middle reaching 2.7 m from
   the centre (notes 135). The sun turns as it rolls: `sun.SetPositionAndRotation(sunAt, Quaternion.Euler(sunSpin, yaw,
   0f))` (`SunBearer.cs` 2698). A tongue that turns to the bottom reaches 1.2 m under the floor. The builder means to
   hide them (67-74), but the 2026-11-19 frame shows spikes, so check which slot holds them.
3. **The spin is about the wrong axis.** The spin is tied to the boss's yaw, not to the way the ball moves. A loose sun
   rolled sideways, a Ricochet bounce and a parry flight all turn about an axis that does not match the travel, so the
   surface (and any tongue) slides and dips oddly.
4. **The sun sinks and rises through solid paving.** Digging in with the sun (`SunBearer.Ground.cs` 62: moves to 2 m
   under), coming up (169: starts 2.5 m under, 185: rises at 30 m/s), and the "Under" state. No hole is drawn; the
   paving is whole round it. `KeepInside` deliberately skips the floor check in the Burrow (`SunBearer.Body.cs` 131).
5. **The floor is not at y 0.** The court's top disc is at 0.036 (`SunCourtBuilder.cs` 56-64). The sun's lowest point
   sits at y 0, a few centimetres inside the floor. Small, but it makes a flat cut on the ball's foot at the close
   camera.
6. **The probe could not see any of this.** `SunFightProbe` part `Clip` measures the boxes of the body's renderers
   only. The builder gives the form `renderers` from the body alone (`SunBearerBuilder.cs` 77), and `KeepInside`
   treats the sun as a 1.5 m sphere (`Body.cs` 117). The corona, the tongues and the trail are never measured.

### 2.2 The owner's: the ball rolls through parts of the beetle

The sun and the body never test against each other except in one case (a hard roll meeting a thin capsule). Every
other path goes through.

1. **The parry sends the sun through the body on purpose.** After it strikes the face, "the sun bounces on past the
   form" to 6 m beyond it (`SunBearer.cs` 786-789, `ParryRebound` 78). Its straight path runs through the thorax,
   abdomen and hind legs. The parry is on by default (`SunParry`, 72).
2. **The Ram lunges through its own sun.** It leaves the sun where it was, 4.4 m ahead, then turns to the player and
   lunges up to about 11 m (`AdvanceRam` 1473-1499). When the player stands in front, as they usually do, the body
   passes straight through the ball.
3. **The Furrows' turn swings the sun through the legs.** At each end the scarab turns 180 degrees and the sun chases
   the held point 4.4 m in front at 40 m/s (1413-1418). The ball's inner edge is 2.9 m from the body's middle; the
   midlegs reach 4.2 m out (hip 1.6 + leg 2.6, notes 143). The same happens when it turns into a Roll (1344) or a
   Ricochet (`Moves.cs` 179).
4. **A soft-rolling loose sun ignores the body.** The body check runs only while the roll is hard
   (`SunBearer.Body.cs` 194; `RollingSun.cs` 19, 82: below 4 m/s it is soft). A light blow, a slowing heavy roll, and
   the 30% rebound after a shell strike (`Rebound` 45-49 sets soft at once) all pass through.
5. **The hard-roll check is too thin.** It treats the scarab as a capsule of girth 1.9 from 3 m behind to 2.2 m ahead
   (`Body.cs` 162-179). The legs reach 4.2 m out, so the ball sinks about 2 m into the legs before it "hits". The frame
   `sunfight_shellfire_1.png` shows the dull sun buried in the right foreleg.
6. **Taking the sun up pulls it straight through the body.** `TakeSunUp` (2243) sets the state to held; then
   `AdvanceSun` moves the ball at 40 m/s in a straight line to 4.4 m in front (2373-2374). After the 3 s walk timeout
   (2160) the sun can be anywhere round it, including behind.
7. **The Ricochet path is not checked against the scarab.** `PlanRicochet` (`Moves.cs` 87-109) checks the wall and the
   obelisk only. The second and third stretches can cross where the scarab stands at the rim.
8. **The falcon rolls the sun into its own talons.** `ReturnOnFoot` rolls it to 2.2 m from the falcon's middle
   (2215); the falcon's girth is 2.6 m.
9. **Nothing moves the sun aside when the body walks.** Walking to the sun (`SunSide` 2230-2240) aims for a spot 4.4 m
   off it, but the path there is not kept clear of the ball.

### 2.3 Earlier feedback on this fight

- 2026-11-17 (`WORLD.md` 145-160, 165-176): the first pass was too flat and square; the bosses' attacks needed better
  visuals; the scarab has no face; it read as light, not fire; the sun must be a burning ball.
- 2026-11-18 (`fire_land.md` 2174-2179): lanes and the ring looked like "just orange highlighted areas with flame";
  phase two "repetitive"; endless stagger; the beetle's vanishing "looks jank"; the loose sun was pointless; "parts of
  the bosses clipping in and out of the arena"; flame fields trapping; flank-camping trivial; "not ... super
  gimmicky ... fun to fight but also has cool set pieces". All were answered in code that night, and **none of it has
  been played since by the owner until today**.
- The workers' own "what is weak" lists, still open (`fire_land.md` 1514-1523, 2318-2357): bare floor and square-block
  wall; the colossus rarely in frame; rigid pieces with no lean or weight; the scarab on its back is the whole body
  rolled; scaled-from-nothing entrance; squashed death; flames are one comic teardrop shape, billboarded, giving no
  light; the ridge a squashed sphere; the shell fire a flat tint; the Grab's carry with no held pose; the Flares' arc a
  fat ribbon; the Gust a front of fire, not wind; the Sunfall's pieces thrown from above the frame; the poise has no
  tell; no sound.

---

## 3. Why it is "cool concepts, not epic and immersive"

An honest read. Each point names what the player sees.

1. **The boss is small on screen and often gone.** At 24 m and 63 degrees, a metre is about 35-40 px at 1080p
   (`fire_boss_visuals.md` 31). The scarab is about 230 px long; the man, at the duel's 15.6 m, about 160 px tall. The falcon, the showpiece,
   rides the north rim 12 m up (`fire_land.md` 1468), which at this camera is the top edge of the frame or above it
   (`aspect_court_pass_1280x720.png`: it is cut off). The biggest thing on screen is the obelisk, a grey slab that
   blocks the view.
2. **The court is not a place.** Nine flat discs in three creams. No paving, no wear, no height, no dressing on 1,250
   square metres of floor. A ring of brick boxes. The colossus, the court's one landmark, is above the frame. The town
   below exists, but the steep camera seldom sees it. There is no sky: a flat background colour. Light is overhead and
   desaturated, so nothing has form or shadow shape. It reads as a test arena with a clock painted on it.
3. **Nothing is at stake in the room.** The court never changes. Lanes burn and cool; the fury is faster numbers and a
   callout. No floor breaks, no wall falls, no light changes for good.
4. **The beats are flat.** The intro is a line on the floor and a text box; he is already standing there. The change
   (4.6 s) is the best beat, but the scarab scales up from nothing and the camera only eases. The fury has no beat of
   its own. The death squashes the body flat.
5. **The camera never moves with intent.** One distance, one pitch. No pull-out for the Zenith, no push-in on an open
   head or the finisher, no lift to show the falcon.
6. **Telegraphs out-shout the creature.** Every move is announced by the same flat red rectangle or circle. Up to six
   lanes, eighteen circles and a whole red floor. The marks are the spectacle; the scarab and the falcon do little
   with their bodies before a blow. That is the "concept" feel: rules on the floor, not a creature doing a thing.
7. **The moves have no weight.** The pieces turn on code with no anticipation, lean, overshoot or settle. Legs swing
   at the hip in two threes with no foot plant (`SunBearer.cs` 2656-2666). Shake, flash and hit-stop calls exist
   (about 50) but the shakes are much the same size; hit-stop is used only for the parry, the shell strike and the death. Hits leave little behind but
   scorch. And there is **no sound at all**, which alone takes away most of the weight.
8. **No music, no sound.** Owner's decision to wait (`GAME.md` 336-342). It is the biggest single gap in immersion.
9. **Too many moves, shown too fast.** 18 moves, all shown in the first 20 picks, shape changes every three or four.
   There is no build-up and no "it was holding back" turn. Many moves share one grammar (a red shape, then a hit), so
   the variety reads as more of the same.
10. **The fire looks drawn, not burning.** Hot floor is a hedge of identical billboard tongues that give no light; the
    shell on fire is a red tint; the ridge is a mud ball. The fire rule is met in colour, not yet in behaviour.

---

## 4. Research: how strong top-down fights feel epic

Principles only; nothing here copies another game's content. Gathered by a research pass on 2026-11-20.

1. **A beat structure, not a flat loop**: build-up, reveal, baseline, escalation, a midpoint turn, the full set, the
   kill, the reward. Mike Stout, "Boss Battle Design and Structure",
   https://www.gamedeveloper.com/design/boss-battle-design-and-structure
2. **An intro that shows who the boss is**, with a spawn of its own, not a fade. Blizzard on Diablo IV's world bosses,
   https://blog.playstation.com/2023/03/15/how-blizzard-crafts-diablo-ivs-menacing-world-bosses-beta-details/
3. **Pull the camera wide only for the big tells**, so they stand out against the usual frame. God of War (2018) Baldur
   fight, https://blog.playstation.com/2023/04/20/shaping-god-of-war-2018s-climactic-boss-encounter-with-baldur/
4. **Build the spectacle attacks first**, the small ones after. Diablo IV (as 2).
5. **Wind-ups of about 1 to 1.5 s, never past 2 s**; every attack looks and sounds different. "Boss Design: Tips From
   A Combat Designer", https://www.gamedeveloper.com/design/opinion-boss-design---tips-from-a-combat-designer
6. **After a big attack, a safe opening at least one full combo long**, counted after the player has closed in. (as 5)
7. **Freeze on impact, longer for bigger hits; a special stop on the killing blow.** Sakurai,
   https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/
8. **Shake scaled to the event**, layered with camera kick, particles and sound. Jan Willem Nijman, "The Art of
   Screenshake", https://www.youtube.com/watch?v=AJdEqssNZ-U ; "Juice it or lose it",
   https://www.richtaur.com/GameDevTreasure/post/juice-it-or-lose-it/
9. **Phase two shows the boss was holding back**, while what was learned still counts. Analysis of FromSoftware
   bosses, https://soulsdle.eu/guides/fromsoftware-boss-design-patterns/
10. **Change the arena at the midpoint** (floor gives way, hazard takes over), but never let the arena kill alone.
    Stout (as 1); Diablo IV's Beast in the Ice, https://diablobytes.com/diablo-iv/guides/beast-in-the-ice-boss/
11. **Lingering ground hazards stop the player hiding at range.** Diablo IV's Ashava (as 2). The scarab's burning
    tracks already do this; keep them.
12. **Design the boss and its room together**, and give the boss a "contradiction" besides menace (sadness, beauty).
    Miyazaki, https://news.xbox.com/en-us/2022/05/27/from-softwares-hidetaka-miyazaki-on-the-secrets-of-elden-rings-development/
    Khepharu's line "I promised them gold. I gave them fire." is that contradiction; the room should show it.
13. **Silhouettes that read from above**: weapon and limbs in a colour apart from the body. Eldest Souls devlog,
    https://www.indiedb.com/news/eldest-souls-the-struggle-of-design ; Diablo IV reworked Ashava's head for top-down.
14. **Music layered by phase**; a boss theme built on a motif heard earlier. Darren Korb on Hades,
    https://gameplay.co/hades-game-music-sound-design-darren-korb-supergiant-games/
15. **Stage in depth layers**: figures in the mid-ground, the threat rising behind; hide forced moves under a turn or
    a heavy shake rather than a cut. (as 3)
16. **A death with its own focused beat**; never an escape that feels like a cheat. (as 1, 9)

Not verified: no developer source was found for the boss cameras or backdrops of Hades, Death's Door, Tunic, Hyper
Light Drifter or Titan Souls. Principle 15 is the nearest sourced idea for what lies beyond the arena.

---

## 5. The relook: work items

Each item has a tag and a "done" line. **[Owner]** marks the few that wait on a choice (section 5.9). Everything else
is our call, held to the owner's bar. Verification for every item follows the `unity-check` skill: compile, console,
tests, a frame, and the profiler where it touches performance. Close frames as well as game-camera frames.

Order of work: 5.1 (clipping) first, since it is broken; then 5.2 and 5.3 (arena and boss model) in parallel, Unity
and Blender; then 5.4 to 5.8.

### 5.1 Clipping (the owner's two faults first)

1. **[Unity] One floor height.** Add a single `FloorTop` for the court (the top of the real paving once 5.2 lands;
   0.036 now) and use it for the sun's rest height, `KeepInside`, `RollingSun`, the marks and the scorch.
   *Done:* the sun's lowest point is exactly on the paving in a close side-on frame.
2. **[Unity] No sun part below the floor.** Replace the corona quad with a corona that is faded to nothing below
   `FloorTop + 0.05` by world height in its shader (or a mesh shell round the ball). Keep flame tongues only on the
   upper half, in world space, not spun with the ball. Remove or reslot any tongues left on the `sun` mesh.
   *Done:* a probe that reads the world bounds of every sun renderer (ball, corona, tongues, trail) in every state
   finds nothing under `FloorTop` except the Burrow's planned sinking; close frames of Roll, Ricochet, Flares, loose
   roll and parry show no hard cut line.
3. **[Unity] Spin the way it rolls.** Turn the ball about the axis at right angles to its actual travel
   (`Vector3.Cross(up, velocity)`), accumulated as a quaternion, in Roll, Furrows, Ricochet, loose roll, parry flight,
   pick-up and death. *Done:* in a 0.05 s frame sequence the surface pattern travels with the ground, no sliding.
4. **[Unity] The Burrow makes a real hole.** When the scarab digs with or without the sun, lay a pit decal and a ring
   of broken paving chunks (5.3 item 13) first, and clip the sun and body by world height inside the pit only. Same
   for coming up: the crater is drawn before anything rises through it. *Done:* no frame shows the ball or body
   passing through unbroken paving.
5. **[Unity] One rule: the sun never passes through the body.** A helper `SunClearOfBody` that measures the ball
   against each body piece's box (the same eight-corner boxes `KeepInside` uses, `Body.cs` 149-156), used by:
   - the **parry**: the sun strikes the face and bounces **back off it** toward the player's side and aside, never on
     through (replace 786-789);
   - the **Ram**: before the lunge the scarab shoves the sun aside with a foreleg (0.25 s, a sand puff), or the lunge
     path is bent so the body never crosses the ball;
   - the **turns** (Roll start, Furrows ends, Ricochet start): the scarab walks round the sun, keeping its forelegs on
     it, instead of swinging the ball through its legs; or the held point moves out to 6 m while turning;
   - the **loose sun**, hard or soft: it bounces off the body's boxes at any speed (only a hard roll sets the shell
     alight); and a widened body volume for the shell strike that includes the legs;
   - **taking the sun up**: the ball rolls into the forelegs along a path outside the leg span, at rolling speed;
   - the **Ricochet** plan: reject any path whose stretches pass within 4.5 m of the scarab;
   - the **falcon**: roll the sun to 3.5 m, not 2.2, and set it between the talons from above.
   *Done:* a new probe part measures the largest overlap between the ball and any body box every frame through every
   move and state; the worst is under 0.05 m except the planned contact between the forelegs.
6. **[Unity] Make the probe see it.** Extend `SunFightProbe` part `Clip` with the two checks above (sun renderers
   versus floor; ball versus body boxes) and a close, low camera frame of each case. *Done:* the part prints a
   worst-of-all line for each, and the before numbers are kept for comparison.
7. **[Unity] The Ram's forelegs stay out of the paving** (0.29 m now, `fire_land.md` 2264). Only the horn goes in.

### 5.2 The arena and the backdrop

8. **[Unity] The phase-two change to the room.** Decided (D1, the molten moat). The version chosen: at "Noon breaks" (fury) the
   Zenith's blaze cracks the court's outer ring (radius 16.4 to 20, outside the hour ring) and it drops 1.2 m into a
   moat of molten gold, crusting and bubbling, with the hour ring as its new lip. The playable floor shrinks to radius
   16; the gold below is the gold he promised. *Done:* the change plays as a 3 to 4 s beat, filmed; the shadow's rule,
   `CourtReach`, the lanes, the Ricochet's wall and every rim number are moved to the new radius and the clip probe
   passes there; the player can never be pushed in (a lip collider); the gold never kills on its own (it only bites
   like hot floor).
9. **[Unity + Blender] A real court floor.** Replace the nine discs with a paving mesh: radial courses of slabs, worn
   and cracked, a few chipped and sunk 2 to 3 cm (visual only, the walkable surface stays flat so the ball rolls true),
   the hour ring inlaid in gold with worn edges, the twelve hour marks carved, sand drifted in the joints toward the
   wall, char and old scorch near the rim, a dais of two low steps round the obelisk's foot (radius 3.4, which the
   sun already cannot enter). *Done:* at the game camera the floor has detail at three scales (slabs, cracks, sand and
   stains); a close frame shows real joints; the marks and scorch still read on it; draw calls stay low (one merged
   mesh, `SceneryMerge`).
10. **[Unity + Blender] A real wall and rim.** Replace the 36 brick boxes with the kit's `wall_section` and
    `wall_broken`, topped with a cornice; on the north arc, a colonnade of lotus columns with the recut king statues
    between them, facing in; braziers at four quarters; two tall flag poles with burning pennants; heat shimmer over
    the top. On the south, a low balustrade so the camera sees over it to the city below. No overhangs a player can
    stand under; every prop solid where it looks solid. *Done:* the rim walked all the way round (section 6, F) with no
    gaps, no walk-through, no fall-out; the colossus is in frame for at least the north third of the court.
11. **[Unity] Bring the colossus into the fight.** Move it in and scale it so its knees and hands frame the north rim
    and its recut face is in the frame when the player is in the south half (about 18 to 22 m tall). It is the
    stage's backdrop and the falcon's perch for the Sunfall. *Done:* visible in at least half the fight's frames.
12. **[Unity + Blender] What lies beyond.** The town below is plain boxes. Rebuild it from `fire_city_kit` pieces near
    the platform's foot (the part the camera can see), with hearths, smoke columns, awnings, and people-scale detail
    within 30 m of the court; boxes only beyond 60 m. Add a sky: a gradient dome with the white sun high and a haze
    band, seen in the pulled-back camera beats (5.6). *Done:* frames from the south rim and from the intro camera
    show a city below and a sky, not a flat colour.
13. **[Unity] The approach.** Build the Way of Noon (`fire_levels.md` 3.5): the three flights rising 6 m to the
    forecourt between walls of recut sphinxes, the shadow of the obelisk reaching the gold line at some hours, the
    camera easing back at the top to reveal the court. *Done:* walked from the arrival door to the line with frames
    at each landing; no fall-outs.

### 5.3 The boss's models and motion

14. **[Blender] The scarab as one rigged body.** Keep the approved look (black-bronze and gold, no face) but rebuild
    it as one skinned mesh on a simple rig: thorax, abdomen, head with horn, wing cases, and six legs of three
    segments each with clawed feet (coxa, femur, tibia plus tarsus). Detail at the game camera: ridged wing cases with
    a burning crack along their seams (emission in the molten slot), gold edging, a segmented pale underside for when
    it lies on its back, hairs or spines on the legs, a worn and chipped shell. Up to about 40,000 triangles with
    LODs. *Done:* a staging render at the game angle and close, beside the player for scale, approved by the owner.
15. **[Blender] The falcon on the same rig family.** Wings with shoulder, elbow, wrist and a primaries fan that
    spreads and closes; a tail fan; talons that open and close round the sun; a head that turns. Burning between the
    primaries as now. *Done:* as 14.
16. **[Blender] The sun as a hero asset.** A molten ball with a flowing surface (a scrolling noise in the shader,
    darker crust plates drifting on brighter seams), no equator spikes; prominences as separate looping meshes that
    rise from the top half; a loose look that is a cooling crust with glowing cracks and smoke. *Done:* close and game
    frames; nothing white; reads as burning matter.
17. **[Blender] Breakable paving kit.** Six to eight paving slab chunks, a pit with a broken rim, crack decals in
    three sizes, and rubble. Used by the burrow ridge (replacing the sphere and cubes), the Ram's horn, the Stoop's
    crater, the Burrow's eruption and the phase-two collapse. *Done:* staging render; then used in 5.1 item 4.
18. **[Unity] Motion with weight.** Drive the new rig from the fight code: foot planting with two-bone IK so legs
    step and grip the paving; a body that leans into turns, dips on landing, braces before a push; anticipation (a
    pull back) before every strike and a settle after; secondary motion on the wing cases and the falcon's feathers.
    The scarab on its back is a pose of the rig (legs kicking), not the body rolled. *Done:* side-by-side frame
    sequences before and after for walk, turn, Roll, Ram, exposed and right itself.
19. **[Blender] The man's detail and the Sun-Spear.** Check `boss_sunking` at 15.6 m and close; add what is missing
    for the read from above (a wider crown silhouette, the gold on the skin bright against the cloak). Model the
    Sun-Spear (3.2 m, gold shaft, leaf blade, winged disc) to replace the primitives in `WeaponBuilder`. Move
    `fire.blend` to `Art/Source/` as the art rule says. *Done:* approved render; `ASSETS.csv` rows updated.
20. **[Unity] The man's clips.** His forged poses become authored clips with anticipation and follow-through, at least
    for the thrusts, the throw, the ray catch, the plant and the noon lift. *Done:* contact sheets in
    `review/anim/sunking_*` before and after.

### 5.4 Every move reworked for weight and readability

**D4 is decided: all the moves stay and unlock in stages** (the scarab alone, then the falcon joins, then everything
in the fury). The chooser's opening of 20 picks (`SunBearerChooser.cs` 17) becomes three stages: the scarab's Roll, Ram,
Sparks and Ricochet; then the falcon with Stoop, Pass, Feathers and Zenith; then the rest at the fury. Each stage should
feel like the boss holding less back.
The common rule for every move: the body tells first, the mark second. Each has four beats: **wind-up on the body**
(0.4 to 1.5 s, never past 2), **travel you can see**, **impact** (hit-stop, shake sized to the move, debris, light),
**aftermath that lingers** (char, cracks, smoke, the creature recovering). Shake sizes: light 0.15, medium 0.3, heavy
0.5, set piece 0.8 (metres, as `TopDownCamera.Shake` takes). Hit-stop on the player when struck: 0.05 light, 0.08
heavy, 0.12 set piece. Hit-stop when the player lands a heavy blow on an open head: 0.06.

21. **[Unity] The man.**
    - Thrusts: a step-in on each; the third a deep lunge with dust kicked back. Drive: a run-up of two steps.
    - Throw: he plants his feet, draws back over 0.9 s, the blade heats; the spear flies a visible arc with a fire
      trail and drives in at an angle with cracks, a sand burst and a 0.3 shake.
    - Ray: he steps aside as the design asks (`fire_land.md` 755, not built: `GUIDE.md` 1415) so the line is a choice
      to read, not his own lane again.
    - Plant: the ring of fire 7 m out that can be stood inside (design 756; built as a filled circle). Inside, a duel
      ring; it reads as an arena within the arena.
    - Noon: shafts fall from the sky with the column visible from its top in the frame (5.6 lifts the camera a little
      for it). *Done for the man:* every move filmed at four beats, game and close.
22. **[Unity] Roll and Furrows.** Wind-up: the scarab backs up, rears on its hind legs and spins the sun in place
    (sparks and embers thrown back over it, 0.6 s). Travel: the sun drives a bow wave of sand and flame, the furrow
    burns behind it, the scarab gallops after with legs planting. Impact at the far rim: the ball hits the wall,
    chips fly from the wall, shake 0.5. Furrows: the scarab plants and pivots round the ball (5.1 item 5). *Done:*
    readable lane in 0.2 s at the game camera; no clipping; a frame sequence shows the four beats.
23. **[Unity] Ram.** The scarab paws twice (sand puffs), lowers its horn, its wing cases flare. The lunge carries
    dust and a short streak along its flanks. The horn smashes paving chunks up (kit, item 17); stuck, it shudders and
    rips free with a burst of chips; the cracks stay 6 s. *Done:* reads as a charge; the sun is never crossed.
24. **[Unity] Sparks.** Real sparks: five burning bits flung from under the wing cases on arcs that land exactly as
    each circle ends, each leaving a small char. *Done:* the thing that lands is seen leaving the beetle.
25. **[Unity] Ricochet.** The scarab winds up like a ram and shoves with its horn; each bounce knocks a chunk off the
    wall's face, with dust and a 0.3 shake; the path never crosses the scarab (item 5). *Done:* bounces seen and felt.
26. **[Unity] Burrow.** It digs a real hole (item 4); the ridge heaves real slabs (item 17) that drop back cracked; the
    eruption throws slabs and fire up in a column, then the scarab hauls itself out of a crater, half-out pose on the
    rig. *Done:* no sphere and cubes anywhere; the eruption is the biggest ground event in the scarab's set.
27. **[Unity] Flares.** The planted sun swells and pulses; prominences (item 16) loop off it and slam along their
    lines, splitting the paving into burning seams. *Done:* each arc is a solid loop of flame, not a ribbon.
28. **[Unity] Shell flaring and the kick.** The shell's heat shows on the rig's plates, not as a whole-body tint; the
    flare throws a ring of fire and embers; the kick is a fast, ugly back-leg strike with sand. *Done:* readable from
    any side.
29. **[Unity] Rising and Setting.** The scarab rears, the sun is hurled up on a trail of fire, the wing cases split,
    wings unfold in stages and beat it off the ground with sand blown outward in a ring (the change itself is a short
    beat with a 0.3 shake). Setting: it drops, wings flare to brake, sand ring, folds, catches the sun. *Done:* frame
    sequences; no pop.
30. **[Unity] Stoop.** The shadow tightens; the falcon is seen high in frame (5.6) folding its wings; the dive is a
    streak of flame; impact cracks a crater (kit), a shock ring of air and dust, 0.5 shake, 0.08 hit-stop on a hit;
    it stands on the sun, wings mantled. *Done:* the falcon is on screen for the whole move.
31. **[Unity] Pass.** The falcon comes in low and large across the frame, wing tips trailing fire, sand torn up in a
    wake on both sides, the lane catching in a wave behind the sun. *Done:* the player stays findable (finder ring),
    and the speed reads.
32. **[Unity] Feathers.** Burning feathers seen falling in a sway from the beating wings, landing as their circles
    end, standing briefly as quills. *Done:* the falling feathers are seen.
33. **[Unity] Ember Gust.** A wall of wind: sand and dust lifted in a front, the hot floor's flames bending away
    from it, embers streaming; the flames of any burning lane lean with it as it passes. *Done:* reads as wind first,
    fire second.
34. **[Unity] Talon Grab.** The player in a held pose in the talons, struggling; the camera stays with them; the drop
    is a hard landing with dust and 0.5 shake. *Done:* no standing in the talons.
35. **[Unity] Sunfall.** The falcon perches on the colossus's head or shoulder (item 11) in the frame and tears pieces
    off the sun; each piece arcs into frame early with a smoke trail and lands steep. *Done:* the throw is seen.
36. **[Unity] Zenith, the set piece.** The camera pulls back and tilts (5.6) so the falcon and the sun are seen
    climbing over the obelisk; the sky darkens round the sun; the court fills; the blaze is light, shimmer and real
    flame bursts on the lit floor, with the shadow cool, still and calm. At the end the falcon falls spent with a
    0.8 shake. In the fury this is where the room changes (item 8). *Done:* the best-looking 10 s of the fight, filmed.
37. **[Unity] Hot floor.** More than one tongue shape (three or four), varied heights, tongues that stand up from the
    floor rather than turn to the camera, and a little light from burning lanes (a few pooled point lights or an
    emissive floor glow). *Done:* a lane running north-south reads as thick as one running east-west.
38. **[Unity] The poise tell.** A gold edge on the balance bar and a faint gold sheen on the shell while it cannot be
    broken (`fire_land.md` 2343). *Done:* the player can see why it will not break.

### 5.5 The cinematic beats

39. **[Unity] The intro.** Decided (D3): he sits in the colossus's lap, rises as the player enters, and steps down
    into the court, the camera pulling back to show the colossus (item 11 brings it into frame). As well: the camera starts wide and low (a pulled-back frame showing the
    colossus and the sky), the man is doing something when the player arrives, his name and title come up as a title
    card held 1.5 s, and the words are click-through. *Done:* a frame sequence of the intro, 6 to 10 s, skippable on
    a second attempt.
40. **[Unity] The change to the Sun-Bearer.** Keep the spear and the fire column. Then: the sky's sun dims to a white
    ring (design 6.4, not built) and the court's light drops; the sun comes down and **strikes the floor**, cracking a
    crater (kit); the scarab claws its way out of the crater (rig pose, not a scale-up), gets the sun between its
    forelegs, rears, roars, and the camera pushes in for the name. *Done:* 5 to 6 s; frame sequence; no scaling up.
41. **[Unity] The fury ("Noon breaks").** A beat of its own: the falcon climbs, the sky goes to a deep orange eclipse,
    the court's outer ring breaks (D1), the music layer changes (if D2 allows). *Done:* 3 to 4 s, filmed.
42. **[Unity] The finisher.** On the broken balance, the heavy blow on the head gets a push-in, 0.12 s hit-stop and
    0.5 s of slow time. *Done:* filmed.
43. **[Unity] The death.** No squash. The sun gutters and goes out; the gold runs off the shell in streams; the wing
    cases drop, the legs fold, the body breaks into its rig's pieces and ashes from the edges in; the man kneels where
    it was, small and without gold (design 6.4, 949-953); the white ring in the sky fills again a hand's breadth west.
    The camera holds it, then the click-through line. *Done:* frame sequence; the kneeling man is seen.

### 5.6 The camera

44. **[Unity] Frame the fight, not the floor.** In the form fight, frame both the player and the boss (the
    `FrameBoth` the director already uses for the entrance), between 1.5 and 2.1 times the usual distance, weighted to
    the player, with the lean toward the boss. *Done:* the boss is in frame in 95% of fight frames (measured, section
    6 G).
45. **[Unity] Keep the falcon on screen.** Let it fly the whole rim, but lower over the south (8 m) so it never
    covers the player, and lift the camera's aim when it is high. *Done:* measured as in 44.
46. **[Unity] Camera beats.** Pull back and tilt about 10 degrees flatter for the Zenith, the Rising and the intro;
    push in 15% on the open head after the Ram and the Burrow; push in on the finisher. Eases of 0.4 to 0.8 s, never a
    cut mid-fight. *Done:* each beat filmed; no motion sickness from rapid changes (no more than one change in 2 s).
47. **[Unity] The obelisk in the way.** Fade it harder (to about 20%) and earlier when between the camera and the
    player or boss. *Done:* never hides the player or the scarab's head.

### 5.7 The light

48. **[Unity] Light by phase.** Duel: noon, but with a lower key (about 65 degrees) so bodies have form, warmer bounce
    and saturation back to 0. Form: the sky's sun dims (item 40) and the sun ball becomes the main warm light, its
    pool moving with it. Zenith: hot orange over the lit floor, the shadow strip cool and blue-grey. Fury: an eclipse
    sky, deep orange-red, with the molten moat glowing from below. *Done:* one golden frame per phase in
    `review/shots/golden/` after the owner accepts it.
49. **[Unity] The sun ball's light casts shadows** when on the floor (one point light with shadows, or a cheap blob
    shadow for the scarab and player thrown away from the ball). *Done:* the scarab and the player cast a shadow from
    the rolling sun; frame time within budget.

### 5.8 Sound

50. **[Unity] Sound hooks only (D2: stay silent for now).** No audio is added. Each cue below becomes a named hook
    the fight calls at the right moment (one static `SoundCue.Play(name, at)` or an event that nothing listens to yet),
    so sound can be added later without touching the fight. No `AudioSource`, mixer or clip comes in with this work.
    *Done:* every move calls its hooks; a test lists the hook names; the game is still silent.
51. **[Unity] Cue hooks (named now, heard later).** The sun: a rolling rumble whose pitch follows its speed, a crackle of burning, a thud and hiss on
    each bounce. The scarab: leg clatter, wing-case clacks, a grinding dig, an eruption boom, a horn strike and stone
    crack. The falcon: a scream for the roar, wingbeats, a dive whistle, an impact boom. Fire: lanes catching (a
    whoosh), burning loops, cooling hiss. The Zenith: a rising drone and heat hiss, silence in the shadow. The man:
    spear whoosh, the ray's roar, the noon shafts. The death: the sun going out, ash falling, quiet. *Done:* every move
    has a sound; the Loudness Report is clean.
52. **[Later] Music** (not now: D2 keeps the game silent; the layer switches below are hooks only). One theme in layers: the duel (percussion, low strings), the form
    (full), the fury (added choir or brass), the Zenith (drop to a drone). *Done:* layers switch on the beats in 5.5.

### 5.9 Decisions for the owner

Four questions. Each had a recommendation. **All four were decided by the owner on 2026-11-20 (night)**; the answers are
under each, and the work items above that hung on them are changed to match.

- **D1 decided: (a), the molten moat.** At "Noon breaks" the outer ring cracks and drops into molten gold, and the floor
  shrinks to radius 16. Item 8 is built as written.
- **D2 decided: stay silent for now.** No audio is added. Every move calls a named sound hook that does nothing yet, so
  sound can be added later without touching the fight. Items 50 to 52 become hooks only.
- **D3 decided: (a), he steps down from the colossus.** He sits in the colossus's lap, rises as the player enters, and
  steps down into the court; the camera pulls back to show the colossus. Item 39 is built that way.
- **D4 decided: (c), unlock in stages.** The scarab alone first, then the falcon joins, then everything in the fury.
  The rows of 5.4 are built for that order.

The questions as they were put:

- **D1. What changes in the court at phase two?**
  - (a) **The outer ring breaks into molten gold.** At the fury the court's outer ring cracks and sinks into a moat
    of liquid gold; the floor shrinks to radius 16. It shows his line: he promised gold and gave fire. *Recommended.*
  - (b) **The colossus breaks.** The colossus's recut head falls into the north of the court, leaving rubble and a
    second, fixed shadow.
  - (c) **Light only.** The room does not change; the sky goes to an eclipse and the light turns red.
- **D2. Sound for this fight now?** (Sound was put off on 2026-11-15.)
  - (a) Hooks only: every move calls a named sound, silent until sound is decided.
  - (b) **Sound effects now, from free CC0 sources or made here; music waits.** *Recommended*: weight needs sound.
  - (c) Sound effects and a layered boss theme now.
- **D3. How does the fight open?**
  - (a) **He sits in the colossus's lap**, steps down the statue as the camera pulls back, and lands at the obelisk.
    *Recommended*: it puts the colossus in the fight and gives him an entrance.
  - (b) He kneels at the obelisk turning the shadow with his spear, and rises when the line is crossed.
  - (c) He comes down from the sky with the sun, landing in fire.
- **D4. How do the 18 moves come?**
  - (a) Keep all 18 shown in the first two minutes, as now.
  - (b) Cut to the 12 strongest and make each deeper.
  - (c) **Keep them but unlock in stages**: the scarab alone at first (Roll, Ram, Sparks, Ricochet), then the falcon
    joins (Stoop, Pass, Feathers, Zenith), then the rest at the fury. Each stage feels like the boss holding less
    back. *Recommended.*

---

## 6. The Unity worker's audit checklist (do this first, before any change)

Film and measure the fight **as it stands** so every later change has a before. No code changes to the game during
the audit; write one probe, `AgentScripts/KhepharuAudit.cs`, run with `script run`. Follow the `playmode-probe` skill.
Rules that bit before: no recompile during play (stop first); bound every loop and pin statics; **the probe must not
wipe saved progress**; set the Input System to ignore focus (see the `SunCourtProbe` notes in `AgentScripts/README.md`).
Game view 1920 x 1080. Frames to `review/shots/relook/before/`. Write findings to
`review/checks/relook/khepharu_audit.md`, one line per check, OK or WRONG with the number.

Two cameras for every move:
- **Game**: the fight camera as it is.
- **Close**: the camera at 0.5 times distance and 35 degrees down (a flatter side view), aimed at the sun or the
  body part in question. Set through `TopDownCamera.SetFraming`; put it back after.

For each move capture **four beats**: the start of the tell, the end of the tell, the instant of impact, and 0.5 s
after. For moving moves also a sequence of frames every 0.1 s (for a GIF), as the earlier GIF reviews did.

**A. Setup**
1. `unity status`; build nothing; open `SunCourt`. Note the commit.
2. `SunBearer.OnlyWhenAsked = true` and `Queued` to force each move alone; fury forced by setting health below half.

**B. The man (game and close, four beats each)**
3. Thrusts 1 to 3; drive and drive-sweep; reach; sweep; butt; throw (the spear in flight and standing); ray; plant
   (rise, standing, dying); noon 1 to 3. Note the man's height in pixels at the game camera.

**C. The change and the beats**
4. The intro from the forecourt to the end of the text box (frames every 0.5 s).
5. The change: 14 frames over its 4.6 s, game and close.
6. The fury's start; the stagger and the finisher; the death, 14 frames over 5.4 s; the coal; the sunset; the way out.

**D. The Sun-Bearer (game and close, four beats, plus 0.1 s sequences for the starred)**
7. Roll*, Furrows* (all three, and both turns), Ram* (with the player **in front**, so the lunge crosses the sun),
   Sparks, Ricochet* (with the player placed so a bounce comes back toward the scarab), Burrow* (dig, chase, lock,
   erupt, half out), Flares* (planted and swelling), Shell flaring, Kick, Rising*, Stoop*, Pass*, Feathers, Zenith*
   (fill, blaze, spent; and the fury version with the turn-back), Ember Gust*, Talon Grab* (caught, carried, dropped;
   missed), Sunfall*, Setting*.
8. Exposed scarab and exposed falcon; the dig back to the sun; the walk back (sun within 7.5 m); the falcon hopping.
9. The parry: a perfect guard on a Roll (SunParry on), sequence of the sun's flight back and the rebound.

**E. The clipping cases (measured every frame, plus close sequences)**
10. **Sun against the floor.** Every frame, the lowest world point of each sun renderer: the ball's bounds; the
    corona quad's four corners (from its transform and scale); each flame tongue's vertices if the tongues are drawn.
    Report the worst below `0.036` per state: held, rolling (Roll, Furrows), Ricochet, loose rolling, parry flight,
    Flares planted (size 1.2), dig with the sun, coming up, Rising throw, Setting set-down, death roll.
11. **Which slot holds the tongues.** List the `sun` mesh's submeshes and materials as built, and say which are drawn.
12. **Sun against the body.** Every frame, the ball (centre, radius 1.5 x size) against each body renderer's box (the
    eight-corner boxes as in `Body.cs` 149-156): report the deepest overlap per case. Cases: the parry rebound; the
    Ram with the player in front; the Furrows' turns; the Roll's and Ricochet's starts with the player behind the
    scarab; a light blow rolling the loose sun into the scarab; a heavy roll into the scarab (the shell strike); a
    heavy roll into the exposed scarab on its back; taking the sun up with the sun behind it; the walk to the sun
    timing out (3 s); a Ricochet whose bounce passes the scarab; the falcon's hop and roll to its talons.
13. Body against floor, wall and obelisk: rerun `SunFightProbe` part `Clip` and keep its numbers as the baseline.

**F. The arena's edges (walk them, do not just frame them)**
14. Walk the player round the rim at radius 19.3 and push outward at every 15 degrees: can the player leave the court,
    clip into the wall, or stand on the low south wall? Frame each 30 degrees at the game camera.
15. The south low wall and the forecourt mouth after the wall of heat closes: push through at five points.
16. The west gate leaf (closed and after it sinks), the sunset mark, the stair beyond: any fall-out?
17. The obelisk: walk round it pushing in; does the visual match the collider? Any spot where the player hides from
    a mark?
18. From four rim points (N, E, S, W), take a game frame and a frame at 2.5 times distance looking outward: what is
    beyond the wall at each? Any empty background colour, holes, floating boxes, seams in the platform?

**G. The camera**
19. Over one full falcon cycle (Rising to Setting, with Stoop, Pass, Sunfall and Zenith), count the frames in which
    the falcon's body centre is outside the viewport (`Camera.WorldToViewportPoint`) and the frames in which the
    obelisk covers the player or the boss.
20. The boss's size on screen: the scarab's length and the falcon's span in pixels at the game camera; the man's
    height in the duel.

**H. Frame time**
21. `bash AgentScripts/profile_court.sh` (1200 frames of the fury's busiest stretch). Also record the change, the
    Zenith and the death. Report main thread and GPU median, 95th percentile and worst, frames over 16.6 ms, and
    garbage a frame. Note the editor's profiler overhead as the earlier run did (`fire_land.md` 2307-2316).

**I. Console**
22. `console clear` at the start; at the end, every error and warning with its count.

**J. Hand-back**
23. Put the worst frames for each fault (the two owner faults first) at the top of the audit file, each with the
    measured number and the code line it points to (section 2 of this brief).

---

## Audit results (2026-11-20) — IN PROGRESS

Section 6's audit was started on the fight as it stands at commit `cabb87d` and paused part-way at the owner's request.
Nothing in the game was changed. The probe is `AgentScripts/KhepharuAudit.cs` (with `KhepharuMeshDump.cs`, which must be
run once in edit mode first, since the model's meshes cannot be read in Play Mode); frames go to
`review/khepharu/audit/<game|close>/` (one folder per move or case: `seq/*.jpg` every 0.1 s and `b1`..`b4` PNG beats;
`worst/*.png` taken the frame a new worst is measured; `frames.csv`; `summary.txt`). The game view was 1920 x 1080.

**Done so far:** the whole game-camera run: the intro, every move of the man, the change, every move of the
Sun-Bearer (18, four beats and a 0.1 s sequence each), the twelve sun-against-body cases (sixteen runs), the arena's
edges, the fury's Zenith, the Flares and Furrows in fury, the stagger and finisher, and the death
(`review/khepharu/audit/game/summary.txt`; the first part, the man's moves, kept in `game_part1/`). **Still to do:** the
close-camera run, `profile_court.sh`, `SunFightProbe` part `Clip` as the body's baseline, the GIFs, reading the frames,
the full write-up below and `worst.png`.

A first run of the form's moves was thrown away: the king's greeting was left open (the editor lost focus, so the probe's
keys were dropped), which holds the player for the rest of the fight, and its sun-inside-the-body test read a held sun as
3 m deep. Both are fixed in the probe (input ignores focus; a watchdog reads out any box; inside is now two-ray parity).

### Headline numbers from the game-camera run (to be written up in full)

- **The sun goes through the body: confirmed.** Deepest overlap of the ball (radius 1.5) with the body's triangles:
  the Ram with the player in front **2.32 m** into the thorax (`game/worst/body_case_ram_front_Ram_Left_2p11.png` shows
  the sun buried in the head); the parry **2.04 m** into the upturned body; a Ricochet whose bounce passes the scarab
  **2.09 m** (abdomen), 1.95 and 1.87 m in the other two tries; taking the sun up from behind **1.90 m**; the walk round
  to a sun on the far side **1.53 m** through the left foreleg; a light roll into the upturned scarab **1.40 m**
  (midleg); the Ram's own turn **1.43 m** (midleg); a heavy roll (shell strike) **0.84 m** (upturned) and **0.76 m**
  (righted). The falcon: its head is **2.35 m** into the sun through the whole Zenith, its talons 1.28 m in on the Stoop
  and 1.46 m on the Rising.
- **The corona below the floor: confirmed at every frame the burning sun is on the floor**: 1.03 m held or rolling
  (Roll 641 of 641 frames over 0.05 m, Furrows 1608 of 1608, Ricochet 829 of 829), **1.29 m** with the Flares' swollen
  sun. The Burrow carries the drawn, held sun **4.60 m** under unbroken paving (164 of 164 frames).
- **The boss is off the screen 29.9% of the form fight's frames; the falcon 82.6% of its frames** (Feathers, Gust,
  Sunfall, Zenith and Setting: every frame; Pass 429 of 530; Stoop 331 of 681). The scarab is off the screen for nearly
  all of a Ricochet (1228 of 1230 frames).
- **Frame time in the editor:** the form fight median 6.7 ms, p95 12.0, 502 of 50985 frames over 16.6 ms, 14.7 KB of
  garbage a frame; the fury's Zenith median **12.3 ms, p95 22.5** (39 of 164 frames over 16.6); the death median 10.7,
  p95 17.0, worst 87.4 ms.

### The brief's claims checked so far

| Claim | Verdict | Evidence |
|---|---|---|
| 1.5 / 3.1: the fight camera looks 63 degrees down (55 + 8) | **Wrong** | `DuelPitch` and `ViewPitch` are **-8**: the view is 8 degrees *flatter*. Measured 47 degrees down in every filmed clip (duel and form). This changes every number below that was worked out at 63. |
| 1.5: duel at 15.6 m, form fight at 24 m | Confirmed | Camera to player 15.4-15.6 m in all eight man clips; 22-25 m in the form fight (first run). |
| 2.1.1: the corona quad goes below the floor, about 0.4 m | **Confirmed, and deeper** | At 47 degrees the quad's lower edge is **1.03 m** under the paving's top with the sun held at idle (286 of 975 frames over 0.05 m in the change; smoke run), **0.85 m** in this run's change at 3.34 s (32 of 502 frames over 0.05 m): `game/worst/floor_change_Entering_Left_0p85.png`. Still to check in close frames: whether the cut *shows* (at the game camera the flame ring reads whole, so the floor may only be cutting the quad's empty corners). |
| 2.1.2: the twelve flame tongues spin into the floor | **Wrong** | The tongues are submesh 1 of `sun` (344 vertices, reaching 2.77 m) with `m_fire_hidden`: not drawn. The spikes in the frames (and in `suncourt_roll_2.png`) are the **corona quad's** flame ring, which always faces the camera. |
| 2.1.5: the floor's top is at 0.036 | **Partly** | The drawn rings' tops are 0.004 to 0.028 m (`Paving Inner` 0.028 the highest); what is walked on (`Court Platform`) is at 0.000. The ball's lowest point is at about y 0 (0.04 below 0.036), so it sits up to 3 cm inside the drawn paving. |
| 2.1.6: `SunFightProbe` part `Clip` cannot see the sun's parts | Confirmed | It measures the `Body/` pieces' boxes and three points of a 1.5 m sphere; the corona, the trail and the dull sun are never measured. |
| 3.1: the man is about 160 px tall in the duel | **Wrong** | 206-232 px at 1920 x 1080 in the duel (eight clips); 163 px only at the intro's camera, before the duel framing. |
| 1.5: the court's edge holds | Confirmed | Pushed outward for 1 s from r 19.3 at every 15 degrees, the player stops at r 19.92-20.37, never lower than y 0.08: no way out, no fall-out. The heat wall at the forecourt's mouth holds at z -19.72 at five points. |

### New faults found so far

1. **The obelisk's collider is wider than the stone.** Pushed into from eight sides the player stops 2.08 m from its
   middle (1.94 on the diagonals); its drawn foot is 1.2 m half-across, so the player meets an invisible wall short of
   the stone (player radius not yet logged).
2. **The obelisk stands between the camera and the boss** for the whole change (502 of 502 frames): the occlusion
   fader thins it to a dithered column that runs up through the frame to the sun.
3. **The sun's corona is a flat spiky sticker.** At the game camera the burning sun reads as a ball on a flat,
   camera-facing star of flame (see the frame above); it does not read as a burning mass from any side.

### Frame time so far (editor, 1920 x 1080, frames stalled by a capture left out)

| Part | Frame ms median / p95 / worst | Over 16.6 ms | Garbage a frame |
|---|---|---|---|
| Intro | 5.6 / 10.5 / 16.4 | 0 of 938 | 9.1 KB |
| The man's moves | 5.8 / 10.1 / 26.1 | 12 of 5275 | 9.0 KB |
| The change | 6.1 / 10.8 / 15.5 | 0 of 659 | 11.3 KB |

### Running notes

- 2026-11-21: extra (a) done: Drawn Back to Break's throw sized by `Enemy.ShoveTravel`; `DrawnBackThrowProbe` slid the
  soldier 2.53-2.58 m (2.1-2.2 m beyond a plain blow's 0.35-0.45 m) in three runs, 1.26 m in one where the soldier was
  mid-attack; `KeystoneRuleTests` 18 passed. Next: extra (b), then the close-camera run.

### To resume (in this order)

1. In edit mode: `unity command script run -- --file AgentScripts/KhepharuMeshDump.cs --entry KhepharuMeshDump.Run`
   (only if `review/khepharu/audit/meshes.bin` is gone). Set the game view: `1920x1080` in
   `AgentScripts/gameview_size.txt`, then `script run --file AgentScripts/GameViewSize.cs --entry GameViewSize.Run`.
2. (Done: the game-camera run.)
3. The close camera: `echo "Change Form Cases Close" > AgentScripts/khepharu_only.txt` and the same run.
4. `bash AgentScripts/profile_court.sh`; then `SunFightProbe` part `Clip` (its guide handling is fixed) for the body's
   floor, rim and obelisk baseline.
5. `python AgentScripts/khepharu_audit_sheets.py gifs </dev/null`, read the frames, write the rest of this section and
   `review/khepharu/audit/worst.png` (`khepharu_audit_sheets.py sheet path=caption ...`).
6. Game view back to `free`; auto-tick and run-in-background off.
