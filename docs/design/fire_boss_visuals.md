# The Fire bosses' attacks: how they should look (plan)

Written 2026-11-17 by Claude as a combat-effects plan for the owner's note that the Fire land's bosses need better
"visuals of the bosses' attacks" (`fire_rework.md`, section 5). Nothing here is built. It was written from the code
(`SunBearer.cs`, `EnemyKind.Fire.cs`, `Enemy.cs`, `SunCourtDirector.cs`, `Vfx.cs`, `VfxBuilder.cs`, `GroundMark.cs`,
`StormDragon.cs`, the two court builders, the glow and spell shaders) and from the frames in
`review/shots/current/` (`suncourt_*`, `suncity_gate_*`, `dragon_beam_*`, `dragon_breath_*`). The editor was not
opened; no number here has been tried on screen.

Sizes are metres, times are seconds, colours are the land's palette (`fire_land.md` section 2 and `SunCityKit`) with
their hex in plain sRGB:

| Name | Hex | Where it comes from |
|---|---|---|
| gold | `#F2B32E` | `m_sun_gold`, `SunCityKit.Gold` |
| ember | `#FF6B14` (drawn at 1.6 to 3 times past white for the bloom) | `m_sun_ember`, `SunCityKit.Ember` |
| molten | `#FF9E29` | the kit's "molten" |
| sun white | `#FFEB9E` | the kit's "sun" |
| noon white | `#FFF2D6` | the key light of the `Noon` mood |
| char | `#171417` | `m_sun_char` |
| shade | `#4D4A5C` | `m_sun_shade` |
| stone, sand | `#A89E8C`, `#A8946E` | `m_sun_stone`, the kit's "sand" |
| mark pale, mark red, mark hot | `#FFEDBF`, `#FF140D`, `#FF730F` | `SunBearer.PlainMarker`, `RedMarker`, `HotMarker` |

---

## 0. The rules every effect here keeps

The ground marks are the warning and stay exactly as they are: pale can be guarded, red cannot, orange is floor that
is hot now. The effects are the attack itself. So that the two never fight at the court's camera (twice the usual
distance and 8 degrees steeper; one metre is roughly 35 to 40 pixels at 1080p in the frames):

1. **Marks own the floor, effects own the air.** Anything an effect lays flat on the floor sits *under* the marks, at
   0.02 m (marks are at 0.05 and 0.07, hot floor's outline at 0.08, the Zenith's shadow at 0.10). Anything standing
   up starts at 0.3 m or higher.
2. **No effect is red.** Red `#FF140D` belongs to unguardable marks alone, pale `#FFEDBF` to guardable marks and the
   shadow's outline. Effects are gold, ember, molten, sun white, char and smoke grey. White hotter than sun white is
   allowed only at the instant of a hit (0.15 s at most).
3. **During a tell the effect lives on the attacker, not on the mark.** The spear's blade heats, the sun spins, the
   falcon's shadow tightens, a shaft of light stands thin over its circle. Nothing additive lies over a red fill
   while it is filling (additive light over red turns it orange, and orange means "hot now"). The bright part of an
   effect arrives the frame the mark is hidden.
4. **What is left behind has the hazard's exact footprint.** A scorched furrow is the hot lane, not wider; a crater
   is the hot circle. Purely cosmetic leftovers (a scorch where a spark fell, the char after the hot floor cools) are
   char with no glow and no outline, so nothing that looks hot is safe and nothing that is hot looks cold.
5. **Thin things vanish.** Lines narrower than 0.2 m are lost at this camera; streaks are 0.25 m or wider, rings
   0.3 m thick or more.
6. **One instant of weight per heavy blow.** Every heavy hit has the same four beats the dragon's do: an impact star
   (`VfxKind.Impact`), a flash (`ScreenFlash`), a camera shake (`TopDownCamera.Shake`), and the player's own hit-stop
   when it lands on them (`HitStop`, already in `PlayerDefence`). The new part is the *thing* that hit and *what it
   left*.

---

## 1. What each move shows now, and what is wrong with it

### Shared faults

- **Off-palette colour in every exchange with the two men.** `Enemy.SlashColour` defaults to a magenta
  `(1.6, 0.45, 1.5)` and only the ashen override it, so every thrust, reach and sweep of the Champion and of
  Khepharu draws a pink-violet tongue or arc (`suncourt_sunking_thrust_1_2`).
- **The impact ring is cold blue-white.** `VfxKind.Ring` is built `(4, 5.5, 7)`; it is thrown at every sun impact,
  stoop, ram, rising, noon shaft and fire circle in the land. Fire wants a gold or ember ring (`TintRing` takes a
  colour already).
- **Dust is grey-blue** `(0.55, 0.55, 0.6)` on a bone-pale court; it should be thrown tinted sand.
- **Emission counted in frames.** The roll (`Time.frameCount % 3`), the pass (`% 2`) and the dragon's beam throw
  effects every few frames, so at the editor's 300 frames a second there are five times the puffs a 60 Hz player
  sees. Effects along a path should be laid per metre travelled or per fixed interval, as the hot floor already is
  (`untilFlame`).
- **Hot floor is drawn as a mark, not as burnt ground.** An orange translucent rectangle or disc with flame puffs
  (`suncourt_furrows_3`, `suncourt_perfect_guard_1`): it reads as "another warning" rather than "this was scorched".
- **The sun lights nothing.** It is the brightest thing in the court and throws no light on the floor or the
  scarab; the floor under a rolling sun is the same bone colour as everywhere.
- **The Sun-Bearer gives no glint or red flare when it commits.** The men do (`Enemy` plays `Glint` or `Danger`).

### The Champion of the Gate (`suncity_gate_*`)

| Move | Shows now | Wrong |
|---|---|---|
| `champion_thrust_1`, `_2`, `_3` | Pale lane fills; at the strike a short magenta tongue from `SlashArcs`; sparks only if it lands | No gold streak; magenta; the third looks like the first |
| `champion_sweep`, `champion_reap` | Pale wedge; a magenta crescent | Magenta; a haft skimming the floor raises nothing |
| `champion_butt` | Pale wedge; a magenta tongue | Fine in shape, wrong colour |
| `champion_throw`, `_throw_twice` | A red lane; he charges down it with the spear (the captain's charge) | No thrown spear, no standing spear, no recall: the stand-in of the city's departure 7 |
| `champion_plant` | Red circle 4.2 m round him; burst, fire, cold ring; then fire puffs at random spots for 6 s | Not a ring; no standing fire; the circle's edge, which is what bites, is not drawn by the fire |

### Khepharu, the man (`suncourt_sunking_*`, `suncourt_spear_stands_*`)

| Move | Shows now | Wrong |
|---|---|---|
| Three thrusts, `sunking_drive` | Pale lane; magenta tongue | The god's own spear leaves no light |
| `sunking_reach` | Pale lane 7 m; magenta tongue | As above; nothing says it reaches further |
| `sunking_sweep`, `_drive_sweep` | Pale half-circle; magenta crescent | As above |
| `sunking_butt` | Pale wedge; tongue | Colour only |
| `sunking_throw` | Red lane 14 m; at the strike the spear simply appears standing at the lane's end, three primitives (`suncourt_sunking_throw_2`) | No spear in the air, no trail, no landing; nothing smokes; the stand-in model is weak |
| `sunking_ray` | Red lane 2.2 m; for the last 45% of the 1.8 s tell a flat additive line 0.1 to 0.9 m wide from spear to hand (`suncourt_spear_stands_2`); at the strike the line is gone and the spear is back in his hand | The best of the man's effects, but it is a flat bar, not light; the strike itself, the moment that hurts, shows nothing; the spear does not travel |
| `sunking_plant` | Red circle 5 m round him; burst, fire, cold ring; fire puffs at random for 8 s (`suncourt_sunking_plant_2`) | As the champion's |
| `sunking_noon_1` to `_3` | Red circle 2.1 m; at the strike a white burst, a flame and a cold ring on the floor (`suncourt_sunking_noon_1_2`) | Nothing comes from the sky: it reads as a mine going off, not sunlight falling |

### The Sun-Bearer (`suncourt_roll_*` ... `suncourt_zenith_*`)

| Move | Shows now | Wrong |
|---|---|---|
| Roll | Red lane 4 m; the sun rolls at 12 m/s with a flame puff at its foot every third frame; at the far end the whole lane turns orange at once | The furrow does not follow the sun: the ground behind it is untouched until the roll ends; no dust or bow wave; no light |
| Furrows | Three of the above | As above, three times; when three lanes are hot the court is three orange rectangles |
| Ram | Pale circle 3.4 m; at the hit an impact star, a cold ring, six grey puffs | No cracked paving, no chips; the horn stuck in the floor leaves nothing to see |
| Sparks | Embers thrown up over the scarab; five red circles; at each a flame and a white burst (`suncourt_sparks_2`) | Nothing flies from the scarab to the circles |
| Rising | Rear, sun lifted; at the throw a burst, a cold ring, a flash (in fury a red circle and fire) | The sun's flight upward is unmarked; no pillar, no flare on the floor |
| Stoop | Falcon over a red circle 5 m; it moves 8 m down in 0.24 s; impact star, cold ring, four flames, grey puffs, shake, flash (`suncourt_stoop_2`) | The dive has no streak, so it reads as a cut; no shock ring through the air; the crater left is an orange disc |
| Pass | Red lane 5 m; the falcon flies it in 1.0 s, a flame at the sun's foot every second frame; lane all hot at the end (`suncourt_pass_2`) | The wings cover the player and nothing shows the speed; the lane does not burn in a wave behind the sun |
| Feathers | Embers thrown down from the falcon; small red circles; a flame and a burst at each (`suncourt_feathers_2`) | No feathers fall |
| Zenith | Floor fills red over 3 s, shadow outlined pale; then the floor pulses orange for 6 s with flame puffs, one flash, the sun swells (`suncourt_zenith_2`) | The court turns orange, not white; nothing shimmers; the sun is off the top of the picture so nothing shows where the heat comes from |
| Setting | Grey dust and a small shake on landing | The fire of the wings just stops |
| Hot floor | Orange translucent shape with an orange outline and flame puffs, fading over its last 2 s | See the shared faults |

**The quality bar** (`dragon_beam_2`, `dragon_breath_2`): the dragon's beam is three `LineRenderer`s (a thick core and
two kinked strands) redrawn every 0.04 s with jitter, sparks thrown along it, a ghost lane sweeping ahead during the
tell, a ring and a flash as it begins, the camera shaking for its whole length. It works because the beam is a
*thing* that is drawn from the mouth to the floor, it moves, and it is brighter than everything else for exactly as
long as it hurts. Every move below is aimed at that.

---

## 2. Every move, as it should look

Each row: **tell** (while the mark shows), **flight** (the strike's travel), **hit** (the instant), **left** (what
stays, and for how long). Mark changes: none unless said.

### 2.1 The Champion of the Gate

The champion shows the god's patterns before the god does, so his effects are the god's in a duller gold: no white
core, molten `#FF9E29` where the god's are sun white.

| Move | Tell | Flight | Hit | Left |
|---|---|---|---|---|
| **Thrusts 1 to 3** | The spear head glints gold (`Glint` tinted gold) at 60% of the tell; the gold at his joints brightens (emission 0 to 0.6 on the cast tone) | A streak of gold light along the lane: a flat quad 0.3 m wide (0.45 m on the third) at spear height 1.1 m, from 0.6 m in front of him to the lane's end, drawn tip-first in 0.06 s | Gold sparks (`HitSpark` tinted gold) on the player if it lands; a gold `TintRing` 1.2 m at the tip if it does not | The streak fades tail-first over 0.2 s; three gold motes (`Embers` tinted gold) drift at the tip for 0.4 s. The third adds a sand puff at his lead foot (the 1.4 m lunge) |
| **Sweep, reap** | The haft lowers; dust trickles from its butt | `SlashArcs` crescent in gold, not magenta | As the thrust | Five sand puffs along the wedge's rim at 0.25 m, where the haft skimmed the floor |
| **Butt** | none beyond the pale wedge | short gold tongue | `Clang` star at the butt's end | none |
| **Throw** | He draws back 0.4 m; the head heats gold to sun white over the 1.0 s; heat shimmer over the blade (section 4, item 16) or, until that exists, three molten motes rising from it | A real spear flies the lane at 40 m/s (12 m in 0.3 s), turned along its path, with a trail 0.25 m wide, 0.25 s long, molten to ember to clear, and a flame puff every 1.0 m | It drives in at 14 degrees: `Impact` star, gold ring 1.5 m, four sand puffs, cracks 1.6 m across at 0.02 m (glow shape 10, char) | The spear stands; its blade cools from ember to gold over 1.2 s; a thread of smoke rises from the blade (`Ash` tinted grey, one every 0.2 s) for as long as it stands |
| **Recall** | A gold thread 0.2 m wide from the standing spear to his open hand for the whole bare-handed 1.2 s, flickering; the spear trembles (2 degrees at 20 Hz) in the last 0.4 s | The spear rips out (sand burst, the cracks stay) and flies back down the thread at 30 m/s with the same trail; the thread widens to the lane's 1.2 m as it passes, as a pale-gold beam (section 2.2, ray) | `Clang` at his hand; a gold ring 1 m | The cracks for 4 s, char |
| **Plant** (as a ring 5.5 m out when the ring is built; until then the filled 4.2 m circle) | The spear lifted point-down; its point white; embers drawn *inward* from the circle's edge toward the point (`Embers` thrown at 16 points on the rim, direction to the centre) | none: it is struck down | `Impact`, ember flash `(1, 0.6, 0.25, 0.3)` for 0.08 s, shake 0.5 m for 0.3 s; sixteen flame tongues (`FormMesh.Flame`, spell shader, ember) rise out of the floor round the rim in 0.15 s, 1.2 to 1.8 m tall | The tongues stand for the burn's 6 s, each swaying and pulsing in height (plus or minus 15%), on a char band exactly the ring's 1.2 m width. As the ring dies they sink in 0.4 s each, one after another round the ring, leaving a char ring for 3 s |
| **The gold runs** (second phase) | Molten drips fall from his joints (`Embers` tinted molten, thrown down, one every 0.3 s) and leave gold spots 0.2 m on the floor for 2 s | | | |

### 2.2 Khepharu, the man with the Sun-Spear

His spear is white-hot along the blade's edges (`fire_land.md` 6.1), so every effect of his has a **sun white core
inside gold**. The god is cleaner than the champion: fewer, brighter, sharper effects.

| Move | Tell | Flight | Hit | Left |
|---|---|---|---|---|
| **Three thrusts, drive** | The blade's edge brightens as the lane fills (spear emission 0.4 to 1.2); a gold glint at the tip at 60% | A streak 0.35 m wide, 5.5 m long, sun white core 0.12 m, gold edges, at 1.1 m, tip-first in 0.05 s. The third and the drive: 0.5 m wide, plus two afterimages of the streak 0.1 m to either side fading in 0.15 s | Gold sparks; a sun-white `Impact` on a landed third thrust | Streak fades tail-first over 0.18 s. The third leaves a line of heat on the floor under the streak (a faint ember line 0.2 m wide at 0.02 m) for 0.6 s, purely cosmetic and below the marks |
| **Reach** | The spear drawn back further; dust under his back foot | As the thrust, 7 m long, 0.4 m, and the streak's tip carries a bright star for 0.1 s | As the thrust | A sand puff at the far end where the point stopped |
| **Sweep** | Haft low; a gold glow along the shaft | `SlashArcs` in gold with a white inner edge, at 0.5 m; ember sparks thrown out from the arc's leading edge, five over the 0.18 s strike | Gold sparks | Sand puffs along the rim |
| **Butt** | | gold tongue | `Clang` | |
| **Throw** | The Sun-Spear raised; its blade goes sun white over the 0.9 s; the disc below the blade flares (a gold star behind it, 1 m, `Critical` tinted gold, at 70%) | The spear flies the 14 m lane in 0.3 s (47 m/s) with a fire trail 0.35 m wide and 0.3 s long, sun white at the spear to ember to clear, and a small ember point light (range 4 m) riding with it | Driven in at 14 degrees: `Impact`, a gold ring 2 m, sand burst, cracks 2 m (char, 0.02 m), shake 0.3 m for 0.2 s | **It stands smoking**: blade sun white cooling to molten over 1.8 s (the bare-handed time), a smoke thread from the blade, embers falling from the disc every 0.25 s, the cracks under it glowing ember at their roots |
| **Ray** | Section 2.4 below: the beam of real light | | | |
| **Plant** | As the champion's, larger: point-down, white point, embers drawn in from the 5 m edge | | `Impact`, ember flash, shake 0.6 m. Twenty tongues of flame round the edge (24 when it is a 7 m ring), 1.4 to 2.0 m, and, while it is still a filled circle, eight more scattered inside | Tongues for the burn's 8 s on a char disc (or band); the edge tongues pulse together on a 0.5 s beat, which is the damage's beat |
| **Noon shafts 1 to 3** | **A shaft of light falls from the sky onto the circle.** From the moment the circle is drawn, a vertical column stands over it from the floor to 16 m: two crossed quads with the glow shader's beam shape, sun white at 25% strength, 0.3 m wide at first, widening to 1.2 m as the circle fills; dust motes (`Heal`-style slow risers tinted sun white, or `Embers` tinted pale) drift down inside it. The column never touches the red fill except its foot, 1.2 m across at most | At the strike the column slams to the circle's full width, 4.2 m, for 0.1 s: sun white core 1.4 m | Sun-white `Impact`, gold ring 2.1 m, flash `(1, 0.97, 0.85, 0.3)` 0.06 s, shake 0.25 m | The column collapses to a 0.2 m line and is gone in 0.25 s. The floor: a bleached disc (sun white, flat, additive) that fades in 0.5 s to a char scorch 1.6 m across with an ember rim, smoking, gone in 2.5 s (cosmetic: it cannot bite) |

### 2.3 The Sun-Bearer

**The sun, in every state.** It is the shared piece of the fight and the thing that hurts; make it light the court.
A point light rides with it: molten `#FF9E29`, range 9 m; intensity 3 when held, 4.5 while rolling or carried in a
pass, 6 at the zenith, off when loose (a dull red sun lights nothing). The intensity flickers by plus or minus 8% on
two sine waves (7 Hz and 13 Hz) so the floor's warm pool breathes. A soft corona (a camera-facing quad, glow shape 0,
5 m, sun white at 15%) pulses with the light. Loose, it smokes: a smoke wisp from its top every 0.4 s and a ring of
cracks glowing ember on its shell (the model's dull look already has them).

| Move | Tell | Flight | Hit | Left |
|---|---|---|---|---|
| **Roll** | The scarab braces behind the sun (as built); the sun spins up in place, 1 to 3 turns a second, throwing embers backward over the scarab; the hind legs kick sand (a sand puff behind each hind foot every 0.25 s); the sun's light rises from 3 to 4.5 | **The furrow follows the sun.** The hot lane's length grows with the sun's place each frame, so the floor behind it is scorched the moment the sun has passed and untouched ahead. A bow wave at the sun's front foot: one flame and one sand puff thrown forward-and-outward per metre travelled; embers spun off its top every 0.5 m; the scarab's feet raise sand every 1 m | `Burst` and gold sparks on the player; at the far rim a sand ring (`TintRing` sand, 3 m) and the shake (as built) | **A smoking furrow** 4 m wide (the hot lane, section 2.5) for 10 s |
| **Furrows** | As the roll, quicker (0.6 s follow) | As the roll, three times | As the roll | Three smoking furrows; while it rests on the sun at the rim for 2 s, smoke vents from under its wing cases (`Ash` grey, 2 puffs a second) and the cases' gold edges dim: it is spent |
| **Ram** | The forelegs scrape: a sand puff behind each foreleg every 0.25 s; the horn's tip glints gold at 60% of the tell (it is pale: guardable) | Two speed streaks (glow shape 1, gold at 40%, 4 m long, 0.3 m wide) along its flanks for the 0.2 s lunge | `Impact`, stone chips (`HitSpark` tinted stone, scale 2), a sand ring 3.4 m, cracks 3 m across at the horn (char, 0.02 m), shake 0.7 m | **Horn in the paving**: sand trickles from round the horn every 0.3 s while it is stuck; as it tears free a burst of chips and a second sand puff; the cracks stay 6 s |
| **Sparks** | Wing cases shaken; embers up (as built) | **Each spark is a thing that flies.** At each wave, five burning sparks (small `FormMesh.Fireball`, 0.5 m, spell shader in ember) leave from under the cases and arc to the circles' middles, apex 5 m, landing exactly when each circle's 1.0 s warning ends; a short trail (0.2 m wide, 0.15 s) behind each | A flame, an ember ring 2.2 m, a few embers | A char scorch 1.1 m across, no glow, for 3 s (cosmetic) |
| **Rising** | Rear 1.2 s; the sun lifted overhead swells from 1 to 1.3 and its light from 3 to 6 | At the throw a pillar of sun white light from the floor to the sun (the builder's "Light Line", now drawn with the glow shader's beam, 1.2 m wide) for 0.3 s, then the sun shoots up with a trail 2 m wide, 0.4 s long | Flash, a gold ring 6 m. In the fury: fire stands up across the red circle (12 flame tongues at 1 to 6 m out) and a ring of scorch 6 m | Fire shed from the opening wings: embers thrown from the wing tips for 1 s; in the fury the circle is char for 3 s (cosmetic) |
| **Stoop** | Its shadow stops over the player (as built) and **tightens** over the 0.5 s hold: from 5 m to 3.6 m across, from 60% to 90% dark; the wings go high; the sun in the talons brightens | **The dive tears the air.** A vertical streak from where it was to the floor: two crossed quads 1.6 m wide, glow shape 1, gold with a sun white core, drawn the 0.24 s it falls and fading over 0.2 s after; three bands of air-pressure (thin white rings, 0.3 m thick) shrinking round the streak as it comes down | **A shock ring through the air**: a white ring 0.4 m thick at 1 m height that widens from 1 m to 7 m in 0.2 s; and a gold ring on the floor widening to 5 m in 0.3 s; ten flames thrown outward along the floor, eight sand puffs at 2.5 m, sun white `Impact` 1.5 times its size; flash and shake as built | **A crater** (the hot circle, 4 m across, section 2.5) with cracks radiating 1 m past its rim (char only past the rim, so the rim still reads); a column of smoke from under the falcon as it stands on the sun |
| **Pass** | The falcon banks into the lane (as built); its wing tips start to draw trails | **A wake.** A trail from each wing tip (0.4 m wide, 0.5 s, gold to clear) and from the sun (2 m wide, 0.6 s, sun white to ember to smoke grey); the lane catches fire **in a wave behind the sun** (as the roll's furrow grows); sand blown outward from both lane edges, one puff per metre; a light shake 0.2 m for the 1.0 s | Burst and sparks on the player | The burning lane 5 m wide for 10 s. The player finder ring stays drawn through the falcon (as built), and the wake trails are thin enough that it is never hidden |
| **Feathers** | The wings beat (as built) | **Burning feathers fall.** Each is a flat feather (`FormMesh.Leaf` scaled 0.4 by 1.2 m, spell shader, gold with ember heat) released from the falcon's height and brought down in a swaying spiral (0.6 m sway, two turns) so that it lands exactly when its circle's 1.0 s warning ends; it trails embers | A flame and an ember ring 1.8 m | The quill stands in the floor burning for 1.5 s (a tongue 0.4 m tall), then a char spot for 2 s |
| **Zenith** | The climb (as built). Over the 3 s fill the court **whitens**: the key light goes from 1.5 to 2.6 and toward noon white; a post-processing volume blends in from 0 to 1 (exposure plus 0.6, saturation minus 35, contrast minus 10, bloom threshold down to 0.8). Six shafts of light (as the noon shafts, 0.6 m, sun white at 20%) fall slowly round the court's edge. The red fill is the warning and stays as built; the shadow is darker (shade, and nothing additive over it) with its pale outline | | The flash as the blaze begins (as built) | **The blaze, 6 s (8 in fury)**: the volume stays at 1; the floor's hot fill is drawn at a third of its strength so the white light, not orange paint, is what the eye sees; **heat shimmer over all the lit floor, none in the shadow** (section 4, item 16); short flame tongues spring up at random places (never in the shadow), 24 pooled, each 0.6 s; the sun overhead is shown by its light and a ring of light on the floor round the obelisk, since it is above the top of the picture. **The shadow is the calm thing on the screen**: cool, still, clear, with three or four dust motes drifting in it. At the end the volume falls in 1.0 s and smoke rises from twenty places on the floor |
| **Setting** | | It comes down on the rim | A gold ring and a sand ring 4 m | Its wing fire goes out: embers from the wings for 0.5 s, then smoke from the folding cases |

### 2.4 The ray: a beam of real light

The man's `sunking_ray` (and the champion's recall, at a smaller size, and the burning ray below half health).

- **Tell (1.8 s, he stands bare-handed with one hand raised).** From the first moment, a thread of light joins the
  standing spear's blade to his hand: a `LineRenderer` 0.2 m wide, sun white at 40%, flickering (width plus or minus
  20% at 15 Hz). The spear's blade brightens from molten to sun white over the tell. Motes drift into the line from
  either side (`Embers` tinted sun white, one every 0.1 s, thrown toward the line). From 70% the thread widens to
  0.4 m and the blade sparks. The red lane, 2.2 m wide, is drawn as built; the thread runs above it at 1.6 to 2.4 m
  height, so it never lies on the red.
- **Strike (0.15 s).** The dragon's beam, in gold: three lines. A core 1.0 m wide, sun white `(6, 5.6, 4.2)` with
  the glow shader's beam shape (12) for its ragged edge and round nose; two outer strands 0.35 m wide, gold, kinking
  by up to 0.5 m round it; all redrawn every 0.04 s with jitter. The outer edge of the strands' kinks is kept inside
  the lane's 2.2 m. The spear itself flies down the beam to his hand in the 0.15 s (93 m/s) as a white streak.
  A point light (sun white, range 6 m, intensity 6) rides with the spear. Flash `(1, 0.95, 0.8, 0.3)` 0.06 s, shake
  0.3 m for 0.2 s.
- **Hit.** If it lands: sun white `Impact`, gold sparks along the beam's direction. At his hand: `Clang` and a gold
  ring 1 m: the catch.
- **Left.** A line of heat on the floor along the beam, 0.6 m wide, ember fading to char over 1.5 s (cosmetic).
  Below half health the burning ray instead lays the hot lane (2.2 m wide, 4 s) with the hot floor's look.

### 2.5 Hot floor: scorched, not painted

What is left by the roll, the furrows, the pass, the stoop, the man's burning ray, and (with its own tongues) the
plant.

- **Under the marks (0.02 m):** a char wash `#171417` at 65% alpha (alpha blended, not additive) over the exact
  footprint, its edge broken by the glow shader's noise (`_Noise` 0.35) so it looks burnt, not cut.
- **Ember cracks:** on top of the char, the glow shader's cracks shape (10) tiled along the lane every 3 m (or one
  per crater), ember at 2.5 times past white, breathing on the 0.5 s bite rhythm (strength 0.7 to 1.0).
- **The edge:** the existing `HotMarker` outline only, no fill, at 0.08 m, so the boundary of what bites is a clean
  orange line. (Today's orange fill goes.)
- **Smoke and flame:** smoke wisps (`Ash` tinted `(0.5, 0.48, 0.46)`, rising, one per 2 m of lane every 0.3 s) and
  the existing flame puffs at a third of today's rate. Heat shimmer over it when that exists.
- **Cooling** (the last 2 s): cracks fade from ember to dull red to nothing, the outline fades, smoke thickens for
  a second and stops. Then the char alone stays 4 s more and fades: by then it has no outline and no glow, so it
  reads as safe, which it is.
- **Under the obelisk's shadow** the part of the lane in the shade loses its cracks' glow and its smoke (the rule
  says it is cold there): the char stays, the ember cracks are drawn dark. This shows the shadow's rule on the floor
  itself, for free, every time.

---

## 3. The parry made for the sun (proposal for the owner to judge)

`fire_rework.md` section 5: a perfect guard against the sun strikes it back like a ball into the form, which is
stunned. As built a perfect guard throws the sun 8 m sideways and the form lies exposed for 4 s. The proposal keeps
the rule "the further the sun goes, the longer the opening" by having the sun rebound past the form after it strikes.
Gameplay numbers here are suggestions, marked as such.

| Beat | Real time | What is seen |
|---|---|---|
| **1. Contact** | 0 | The player's guard meets the sun. Hit-stop 0.18 s (longer than the ordinary 0.12 s perfect guard: this is the land's big one). Flash `(1, 0.95, 0.8, 0.55)` for 0.08 s. The `PerfectGuard` burst and a gold `Critical` star 1.5 times its size at the point of contact. The sun is drawn white-hot for the freeze (`_EmissionColor` up, its light to 8) |
| **2. The strike back** | 0.18 to 0.53 | Time runs at 35% for 0.35 s (`HitStop.Slow`). The sun leaves the player's guard along the line to the form's face (the scarab's thorax; the falcon's breast; for the stoop, straight up into it) on a shallow arc, apex 2 m. Behind it a trail 2.4 m wide and 0.4 s long, sun white to gold to ember, and embers thrown off it every 0.5 m. The camera eases 15% closer on the midpoint of player and form for this beat (`TopDownCamera.CloseIn(0.85, ..., 0.12)`) |
| **3. It strikes the form** | about 0.55 | Freeze 0.08 s. A sun white `Impact` three times its size and a `Burst` at the face; a gold ring 4 m on the floor under it; flash ember `(1, 0.6, 0.25, 0.35)` 0.06 s; shake 0.6 m for 0.35 s; the form flashes white (its `Paint` flash, already there). *Suggested:* 30 damage at 100% and 60 balance, more than a sideways knock (40) because it took a reading of the tell to earn it |
| **4. The form goes over** | 0.6 to 1.2 | The scarab rolls onto its back, the falcon tumbles out of the air and lies with its wings spread (both as built). Its fire goes out *visibly*: a puff of smoke from every glowing edge (`Ash` grey, five at once), the gold edges dim to dark in 0.3 s. Five gold stars circle above its head at 1.2 m radius, one turn a second (comic stun stars: five small quads, glow shape 5), for the whole stun |
| **5. The sun rebounds** | 0.55 to 1.4 | It bounces off the form and rolls 6 m beyond it, slowing, going from white to dull red over the 0.85 s, smoking. The form must walk round itself to reach it: about 4 s on its back plus 2 to 3 s of walking, the same opening a long sideways knock gives today |

Why it reads at the top-down camera: the trail draws the line from player to form across the floor in one stroke,
and the slow-down holds it there long enough to see. Nothing in it is red.

---

## 4. How to build it, in this project, by gain per hour

### What exists to reuse

- **`Vfx.Play(kind, at, direction, scale, tint)`**: one pooled emitter per kind, world-space, no allocation. Any
  kind can be tinted (the tint is the particle's start colour, multiplied with the material's). `Fire`, `Embers`,
  `Ash`, `Dust`, `Impact`, `Critical`, `Glint`, `Clang`, `Burst`, `TintRing`, `TintSpark` cover every puff, spark,
  star and ring above. `ElementLook.Puff`, `Sparks`, `Ring`, `Strike` give Fire's set in one call.
- **The glow shader** (`clover_glow.shader`): shapes 0 soft disc, 1 streak, 2 slash, 3 flat, 4 ring, 5 star, 6 flame,
  10 cracks, 12 beam, 13 rimmed disc; `_Noise` and `_NoiseSpeed` for breakup; `_Hard` and `_Core` for the printed
  edge and white core; additive or alpha blending from `BuildKit.Glow(..., additive)`. No textures needed.
- **The spell shader** (`clover_spell.shader`): three flat tones, ink outline, `_Hot` 0 to 1 for white heat, `_Thin`
  for smoke; colour and heat by property block. With **`FormMeshes`**: `Flame` (tongues), `Fireball` (sparks),
  `Leaf` (feathers), `Ray` (a needle of light), `Gust` (a streak of air), `Shard`.
- **`LineRenderer`** beams as `DragonBuilder.Bolt` makes them and `StormDragon.DrawBeam` draws them (fixed point
  array, redraw every 0.04 s). `SunCourtDirector` already owns the ray's line.
- **`TrailRenderer`** as `ThrownThing` and `EmberMotes` use them.
- **`GroundMark`** and `MarkerShapes` for any new floor shape; the hot floor's pools (`hotLanes`, `hotCircles`)
  already exist and only change material and length.
- **`HitStop.Freeze` and `Slow`**, **`ScreenFlash.Flash`**, **`TopDownCamera.Shake` and `CloseIn`**.
- **Lights**: the toon shader draws other lights as hard-edged pools, so a point light under the sun will print a
  warm disc on the floor in the game's own style. `LightningBolts` and `StormLight` show the pooled flash pattern.
- **A post-processing volume blended by weight**: `NoonHeat.swellLook` (being built now, untracked) is exactly the
  "whiten the look" the Zenith needs; share its profile or make a second.
- **The PC render pipeline asset has the opaque texture on** (`m_RequireOpaqueTexture: 1`), so a heat shimmer can
  sample `_CameraOpaqueTexture` without changing settings.

### Where the code goes

- **The Sun-Bearer**: its effects live in `SunBearer` already; add a small `SunBearerLook` (Clover.Gameplay, plain
  MonoBehaviour) holding the pools below, configured by `SunBearerBuilder`, with methods the fight calls
  (`StreakDown(from, to)`, `ShockRing(at)`, `Feather(i, from, to, seconds)`, `Spark(i, from, to, seconds)`,
  `SunLight(state)`). Keeps the 2,000-line `SunBearer` from growing much.
- **The two men** are driven by `Enemy`, which another worker has open right now (`git status`). Do not add to
  `Enemy.cs` beyond two read-only getters (`StrikePoint` for ranged circles, `LungeFrom` for passing lanes). Put
  their effects in a `SpearLook` component on the boss that listens to `Enemy.AttackStruck` and reads
  `Brain.Current`, `Brain.State` and `Brain.TelegraphProgress` (the way `SunCourtDirector.WatchSpear` already does),
  configured by `SunCourtBuilder` and `SunCityBuilder`. Set `SlashColour` from the builders.
- **Pools, made by the builders, never at play time**: 6 streak quads, 2 spear trails, 1 beam (3 lines), 3 shaft
  pairs, 24 flame tongues, 16 feathers, 10 sparks, 6 scorch meshes, 2 shock rings, 5 stun stars, 2 point lights
  (sun, impact). Each is enabled, placed and tinted through one shared `MaterialPropertyBlock`; nothing calls
  `new`, `Instantiate` or `GetComponent` per frame; every array is sized at build time. Any static gets its
  `SubsystemRegistration` reset.

### The list, ordered by visual gain per hour

**Bold rows are the 90-minute session**, about 85 minutes of work plus checks.

| # | Item | Moves it lifts | Work | How |
|---|---|---|---|---|
| **1** | **Gold, not magenta and not blue** | All of the men's; every ring in the land | **15 min** | Builders set `SlashColour` on the champion `(2.0, 1.4, 0.4, 0.8)` and on Khepharu `(2.6, 2.1, 1.1, 0.9)` (gold with white heat). In `SunBearer` and `Enemy`'s fiery branch, `Ring` becomes `TintRing` in gold; `Dust` is thrown tinted sand. Five lines in `SunBearer`, one in `Enemy` (tell its other worker) |
| **2** | **Emission by distance, not by frame** | Roll, pass, the dragon's beam | **10 min** | Replace `Time.frameCount % n` with a metres-travelled or seconds counter, as `untilFlame` does |
| **3** | **The sun lights the court** | Every scarab move; the whole fight | **20 min** | One point light under the sun's holder in `SunBearerBuilder` (molten, range 9, no shadows); `SunBearer.AdvanceSun` sets intensity by state with the two-sine flicker. Check the frame cost with one extra light (the probe's profiler) |
| **4** | **The furrow follows the sun** | Roll, furrows, pass | **20 min** | `LayLane` at the moment the sun starts, length 0; each frame set that slot's `hotLength` to the sun's distance along. The perfect-guard path already lays a partial lane; it becomes the same slot |
| **5** | **The stoop tears the air** | Stoop (the falcon's main blow) | **25 min** | Two crossed quads (glow shape 1, additive, gold with core) stretched from the falcon to the floor during the 0.24 s drop and faded over 0.2 s; a second, white `Ring`-style mesh at 1 m height scaled 1 to 7 m in 0.2 s for the shock ring; the shadow blob scaled 5 to 3.6 m over the hold |
| 6 | The ray as a beam of light | The man's ray; the champion's recall later | 45 min | `SunCourtDirector`: the thread from the tell's start; at the strike three lines drawn as `StormDragon.DrawBeam` draws (copy the method into a small shared `BeamLines` class both can use); the ray's material changes to the glow shader's beam shape; a spear streak and a light along it. Needs the director to see the 0.15 s strike state, which it can (`Brain.State`) |
| 7 | Scorched hot floor | Every hot lane and crater | 45 min | A second material for the hot fills: char, alpha-blended, noise 0.35; a cracks quad (shape 10) per 3 m of lane from a pool of 6; drop the orange fill alpha to 0, keep the outline; smoke wisps on the existing `untilFlame` timer; dark cracks inside the shadow (`Sunlight.InHand` is already asked per point) |
| 8 | The thrust's streak | All thrusts, reach, drive | 30 min | `SpearLook`: on `AttackStruck` for a lane attack, place a streak quad (shape 1, gold, white core) from the spear bone's tip along the lane, scale its length up over 0.05 s and fade its tail over 0.2 s |
| 9 | Noon shafts from the sky | The man's noon; the Zenith's six shafts | 40 min | Three pairs of crossed vertical quads (shape 12 with u up the height), placed over each ranged circle from the moment it is drawn, width driven by `TelegraphProgress`; slam to full width at the strike; a bleached disc then a scorch from the scorch pool |
| 10 | The thrown spear flies and stands smoking | Both men's throws | 45 min | A copy of the boss's spear mesh (not three primitives; `SunCourtBuilder` can bake it from the `weapon.R` child) with a `TrailRenderer`; `SpearLook` flies it along the lane over the strike, then hands it to `WatchSpear` to stand; smoke and embers on timers; cracks from the scorch pool |
| 11 | Zenith whitening | Zenith | 40 min | Key light intensity and colour lerped by `SunBearer` during the fill (the director already turns the light); a `Volume` with its own profile whose weight follows the fill and the blaze; hot fill drawn at a third; flame tongues from the pool of 24 |
| 12 | The parry made for the sun | Roll, furrows, stoop, pass | 90 min | Gameplay in `KnockSun`: a new path when the guard faced the sun's way; a short flight state for the sun with a `TrailRenderer`; `HitStop.Freeze(0.18)` then `Slow(0.35, 0.35)`; impact on the body; rebound target 6 m past it; stun stars. Needs a test in `SunCourtTests` that the rebound respects `KeepSun` and the opening still lasts at least 4 s. **Owner's call first** |
| 13 | Feathers that fall, sparks that fly | Feathers, sparks | 60 min | Pools of 16 leaf meshes and 10 fireball meshes (spell shader); each laid with its fall's point and warning; positions computed from the warning left (`fallLeft`), so landing and damage are the same instant by construction |
| 14 | The plant's ring of tongues | Both men's plants | 45 min | `SpearLook` on the plant's strike: the 24 flame tongues of the pool placed round the edge, height pulsing on the burn's beat, sunk one by one at the end; a char band from the scorch pool. Replaces the random puffs (in `Enemy`, with its other worker) |
| 15 | The pass's wake | Pass | 30 min | `TrailRenderer`s on the two wing-tip bones and the sun, emitting only during the pass's flight; sand puffs per metre at the lane's edges |
| 16 | Heat shimmer | Zenith, hot floor, the throw's blade | 2 h | A new small transparent shader (`Art/Shaders/clover_heat.shader`) that offsets `_CameraOpaqueTexture` by two scrolling sine waves (amplitude 0.004 of the screen, 6 and 9 cycles a metre, rising 0.8 m/s), masked by a vertex alpha and, for the court, by the shadow strip passed in as a centre, direction and half-width (`ShadowHand` has all three), so the shadow is still. Drawn on the hot meshes and on one floor-sized quad during the blaze. Check the cost on the profiler; if it is more than 0.3 ms, fall back to stylised heat squiggles (glow shape 1 streaks rising and wavering) |
| 17 | The ram's cracks and chips; the sweep's dust; the rising's pillar; the setting's quench | Ram, sweeps, rising, setting | 40 min together | Pieces of the pools above, each a few `Vfx.Play` calls and one scorch |
| 18 | The champion's real throw, recall and ring | Champion | (gameplay: the city's departure 7) | Once the gameplay is built, his effects are item 6, 10 and 14 at his sizes and in molten, not sun white |

**Done means** (CLAUDE.md, the `unity-check` skill): it compiles; the console is clean; `SunCourtTests` and the
probe's checks pass; `SunCourtProbe` and `SunCityProbe` re-shoot their frames and the new ones are read beside the old
(`suncourt_roll_2`, `_stoop_2`, `_spear_stands_2`, `_sunking_thrust_1_2` are the before pictures for items 1 to 6);
the profiler over a Zenith with two hot lanes shows no new garbage a frame and the frame time is still near the
measured 3.0 ms mean. Every mark must still be readable in the new frames: look for red or pale fills that an effect
has washed out, and for any effect that is red.

---

## 5. Questions for the owner

1. The parry that strikes the sun back (section 3): build it as described, with the rebound so the opening stays
   about as long as today's, or let the struck form lie longer as the reward?
2. Heat shimmer: a real refraction (item 16, about two hours and a little frame cost) or drawn squiggles in the
   comic style of the impact stars (about forty minutes)?
3. The noon shafts and the Zenith's shafts make light come *from the sky* where the camera cannot see the sun. Is
   a visible beam from above the right call in a game whose sun is otherwise only a light direction?
