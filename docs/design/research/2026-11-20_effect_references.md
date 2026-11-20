# Effect references: Wind, Thunder Gust and Carried Embers (2026-11-20)

Research for the fix plan (`docs/design/feedback/2026-11-20_fix_plan.md`, "Research first"). It answers three of the
owner's comments in `docs/design/feedback/2026-11-20_effects_review_2.md`:

- **Wind, Unsteady** (d4839de5): "Wind is a line through the enemy, it doesnt look good at all, and there is no sense of
  what it is. It needs a real sense of identity. An idea is a wind blowing around the enemy. Additonaly a lot of
  reactions with wind in it have random white shapes pop up, which dont make any sense, and look terrible ... please
  research all the wind effects and whatnot in other games". Seed Storm is "the first one that actually feels like wind".
- **Thunder Gust** (636509df): "it needs to be more like a lightning tornado, search up lightning tornados from fiction,
  I am also still getting a bunch of strange shapes and lines, instead of actual wind shapes". Keep the shock chain.
- **Carried Embers** (378806ae): "the wind part of it still doesn't look good at all", with the photo
  https://npr-brightspot.s3.amazonaws.com/a5/2f/7005e345478dbba425abf239a9d9/fire-tornado.jpg as the model.

Each section lists techniques (what it looks like, why it reads from our camera, how to build it, source), then the
recommended designs, then what to avoid. Where no written breakdown exists and a note comes from what a game or show is
known to look like, it says so.

## What our camera does to these looks

- The camera is 12 m away, looking 55 degrees down. A vertical thing shows at about 0.57 of its height
  (cos 55); a thing lying on the floor keeps about 0.82 of its depth (sin 55). **The floor is the strongest canvas**, and
  a column needs a wide top so its rim reads as a ring from above.
- An enemy is small on screen. One or two strong shapes, a clear value step against the dark muted world, and motion
  along a readable path read; fine texture and many small parts turn to noise.
- The world is dark and muted and the elements are the only strong colour, so a wind look that is pale and nothing
  else reads as "white shapes". Wind needs something to carry and a darker edge.
- `LOOKS.md` (Lightning and Wind land) already sets wind as "pale streaks of dust, grass and leaves", "a wall of dust
  rolling across the floor", "thin pale crescents trailing dust", floors "combed by the wind", and the two elements
  together as "lightning carried inside wind-driven cloud, sparks streaming along the gust". The research agrees.

## Principles shared by every source

1. **Wind is seen through what it moves.** Ghibli shows invisible wind through grass, hair, cloth and windmills, and
   spent many cels on moving plants in Nausicaa so the audience feels its strength and direction
   ([note.com Nausicaa analysis](https://note.com/moomoo/n/n7d1d235bfcc5?hl=en),
   [3 Quarks Daily](https://3quarksdaily.com/3quarksdaily/2016/07/current-genres-of-fate-wind-in-miyazaki.html)).
   The live-action Avatar team avoided a coloured "magic wind" and displaced the particles already in the scene
   ([Yahoo, how Avatar made air bend](https://www.yahoo.com/entertainment/tv/articles/avatar-last-airbender-made-water-120000446.html)).
   Seed Storm worked for the owner for the same reason: the gust carried seeds.
2. **Carried things ride a path.** To make leaves read as wind and not as falling leaves, draw them along streams of
   wind ([AnimeOutline, drawing anime weather](https://www.animeoutline.com/how-to-draw-anime-weather-tutorial/)).
3. **Lines are drawn on and wiped off; they never pop.** Wind Waker's wind lines are ribbons along a curve whose beauty
   is in the animation ([gordonnl, Wind Waker graphics analysis: Wind](https://medium.com/@gordonnl/wind-f4fc7a3b366a)).
   An anime crescent blade wipes in fast and its tail erodes toward the head
   ([EasyVFX slash design notes](https://github.com/VuxSussy/EasyVFX/pull/4)).
4. **Primary and secondary.** Riot's VFX style guide: the primary part has a focal point, high value and a clear
   silhouette; the secondary part is lower value, simpler, lower opacity, subtler; effects have anticipation, main and
   dissipation stages; linger is kept short to cut noise; never 100% or 0% value; analogous colours, and when two
   complementary colours meet, one becomes secondary
   ([League of Legends VFX style guide, summary](https://gdad.wiki/wiki/sources/source-lol-vfx-style-guide)).
5. **Layers at different speeds.** Anime compositors build wind from three layers: near large and fast, middle,
   far small and slow; dust crossing the front adds air
   ([Junpei Takatsu, anime compositing: wind](https://note.com/taka2composite/n/nbdf6a411a4bf?hl=en)). Tornado
   breakdowns spin inner layers faster than outer ones, and note that spin speed sets the felt size: too fast looks small,
   too slow loses threat ([80.lv, Creating a Tornado in Unity](https://80.lv/articles/creating-a-tornado-in-unity)).
6. **Stepped values give the hand-drawn look.** The same compositor gets the "hand-drawn texture" from a hard Levels
   step on soft noise, then smooths the jagged edge (Takatsu, above). That is our toon ramp: two or three flat values with
   an ink edge, not a soft gradient.

## 1. Wind

### Techniques

**W1. Wrapping curl ribbons (Wind Waker).**
- Looks like: a thin ribbon draws itself along a curve that loops round, ending in a small curl or hook, then its tail
  follows and erases it. Several at once, staggered, at different heights.
- Reads from above: a loop round a body is a closed shape, so it reads as "round", not "through". The draw-on and
  erase give direction without any pop.
- Build: a ribbon mesh along a Blender curve (bevel 0, a few cm wide, tapered at both ends). In Unity a shader reveals
  it along U (head) and erases it from U = 0 (tail), with a mask for the curl end. Motion: **orbit** round the body
  axis while the reveal runs. Toon-shaded pale body with a darker ink edge.
- Sources: [gordonnl, Wind](https://medium.com/@gordonnl/wind-f4fc7a3b366a);
  [Cartoon Wind Effect (Wind Waker) in Unity](https://www.youtube.com/watch?v=Jj8UHGe5Aps).

**W2. A wrapped airflow band with stepped noise (anime compositing).**
- Looks like: a short open band of streaky air around the body, its streaks running round it, hard-edged like a drawn
  cel, fading at top and bottom.
- Reads from above: from 55 degrees the band's near side shows as an arc in front of the body and the far side as an arc
  behind it, so the body is plainly inside the wind.
- Build: an open cylinder (or a cone, wider at the top) round the enemy at 0.5 to 0.7 m radius, about 0.4 m tall.
  A stretched noise texture pans along U (round the body), stepped by a threshold into two or three values, with vertex
  alpha black at the band's top and bottom. Wave the band's radius a little (a vertex offset) so it is not a perfect
  ring. Motion: **spin** (UV pan), different speeds per layer.
- Source: [Takatsu, wind compositing](https://note.com/taka2composite/n/nbdf6a411a4bf?hl=en) (CC Cylinder wrap and
  rotation, strong Levels, three Wave Warps, layered by distance).

**W3. Carried debris on the stream.**
- Looks like: two to five leaves, dust tufts or scraps of paper caught in the flow, tumbling as they ride the loop.
- Reads from above: these are solid, outlined, toon-shaded objects with real silhouettes. They read at a small size
  where faint air does not, and they say "wind" without any coloured effect.
- Build: small meshes (a leaf, a curled paper strip, a grit clump) with the outline shader, ochre and grey-gold per
  `LOOKS.md`. Motion: **orbit** on the ribbon's path plus **spin** about their own axis. They persist while the wind
  lasts, joining and leaving smoothly, so nothing pops.
- Sources: [AnimeOutline](https://www.animeoutline.com/how-to-draw-anime-weather-tutorial/); Seed Storm in our own
  combos pass (`staging/blender/2026-11-19_combos3/notes.md`, item 12), which the owner singled out.

**W4. The body answers the wind.**
- Looks like: the enemy leans a few degrees into or with the wind; its cloth, tassels or hair stream to one side.
- Reads from above: the silhouette of the enemy is the biggest thing in the picture; any change in it reads.
- Build: a lean on the enemy's root (a code offset, as Tremor already shakes the body); where an enemy has cloth bones or
  hanging parts, push them downwind. Motion: **sway**, slow, 1 to 2 cycles a second.
- Sources: Ghibli (note.com and 3 Quarks Daily above); the Animation Mentor wind tutorial on Avatar, where clothes
  and characters react to the gust so the force is seen
  ([Animation Mentor](https://www.animationmentor.com/blog/tutorial-animate-characters-in-wind/)).

**W5. The combed floor.**
- Looks like: grass laid over and dust drawn into thin ripples, swept round the feet (a swirl) or along a lane (a gust),
  with the pale undersides of the grass showing.
- Reads from above: the floor keeps most of its size from our camera, and it is not hidden by the body. This is the
  strongest single read of wind direction we have.
- Build: a decal or floor quad with a streak texture whose U runs along the flow; **spin** it under an Unsteady enemy,
  **sweep** it along a gust. Stepped values, darker than the wind ribbons, so it sits on the floor.
- Sources: `docs/design/LOOKS.md` ("combed by the wind", "a lane is a strip of flattened grass and dust"); Ghost of
  Tsushima drives grass, leaves and birds from one wind field so all of them show the same wind
  ([PlayStation Blog, VFX of Ghost of Tsushima](https://blog.playstation.com/2021/01/12/how-stunning-visual-effects-bring-ghost-of-tsushima-to-life/);
  [Real Time VFX thread](https://realtimevfx.com/t/vfx-of-ghost-of-tsushima-a-recent-blog/15815)).

**W6. Crescent air blades.**
- Looks like: one clean outer arc, width growing inward to a peak near the head and ending in needle tips, with a long
  thin tail. Three tones: a deep shade on the inner edge, the main colour, a light outer edge. Demon Slayer's wind
  breathing draws its cuts as jagged pale green-and-white slashes, "claws" of four parallel strokes and whirlwinds of dust
  that seem to tear the picture.
- Reads from above: a crescent lying near the floor plane is one bold shape with a direction.
- Build: a ring-segment mesh with a mask texture; **sweep** (wipe-in along U in 0.06 to 0.1 s), then erode from the
  tail. Shade with the three-tone toon ramp.
- Sources: [EasyVFX crescent blade notes](https://github.com/VuxSussy/EasyVFX/pull/4);
  [Venom Slash breakdown, ring mesh plus scrolling noise](https://realtimevfx.com/t/venom-slash-breakdown-of-the-effect-included/18903);
  [Demon Slayer wind breathing forms](https://boutique-demonslayer.com/en/blogs/demon-slayer/wind-breathing-sanemi-s-nine-forms)
  (fan description).

**W7. Spiral vortex in nested shells.**
- Looks like: a twisted funnel of two or three shells, the inner spinning faster than the outer, fading at the top and
  bottom.
- Reads from above: the top rim is a ring with a spiral inside it; the twisted bands on the near side show the spin.
- Build: one twisted, UV-mapped funnel mesh, duplicated and rotated 90, 180 and 270 degrees; a texture pans along U so
  it follows the twist; vertex alpha black at the ends; a slow vertex wobble. Motion: **spin**, per shell.
- Sources: [80.lv, Stolen Knowledge (spirit tornado)](https://80.lv/articles/stolen-knowledge-texturing-and-vfx-in-a-stylized-environment);
  [80.lv, Creating a Tornado in Unity](https://80.lv/articles/creating-a-tornado-in-unity);
  [1MaFX, stylized tornado breakdown](https://www.1mafx.com/blog/stylized-tornado-vfx-breakdown) (masked swirls for
  crisp detail, Fresnel opacity for the silhouette);
  [80.lv, Gustavo Rosa's cone-layer tornado](https://80.lv/articles/neat-stylized-tornado-vfx-set-up-in-unreal-engine-s-niagara).

**W8. Puffs that swirl as they dissolve (Breath of the Wild).**
- Looks like: a dust or cloud puff curls and twists while it eats away, so it is one continuous motion, never a sprite
  switching off.
- Reads from above: the end of a look stays readable and calm instead of flashing pieces.
- Build: our `Clover/Flipbook` dissolve, with the texture first distorted by a twirl flow map, then dissolved. Hand-
  painted two-layer flipbook in Blender, or a flow-map twirl in the shader.
- Sources: [80.lv, stylized combat VFX in the Zelda style (Soenke Seidel)](https://80.lv/articles/creating-stylized-combat-vfx-in-the-zelda-style);
  [80.lv, Making Zelda-like VFX with Unity (twirl flow map)](https://80.lv/articles/making-zelda-like-vfx-with-unity).

**W9. Streaks pan along the flow, not across it.**
- Looks like: lines that slide along their own length, with a second layer of straighter lines.
- Reads from above: diagonal noise reads as a texture crawling; streaks moving along themselves read as air moving.
- Build: pan U only, with a little noise; pack a straight-line layer and a curl layer in two channels of one texture.
- Source: [Real Time VFX, wind effect around a moving body](https://realtimevfx.com/t/wind-effect-breaking-sonic-barrier-need-help-with-the-texture/27462).

**W10. Distortion, as support only.**
- Looks like: a shimmer where the air is pushed, like heat haze.
- Reads from above: weakly at our size and on a cel-shaded picture, so it can back a strong gust (Gust's cone, a heavy
  blow) but cannot carry wind's identity alone.
- Build: a URP refraction pass would be a new Renderer Feature; a cheaper stand-in is a scrolling, faint, darker ring in
  the floor. Leave it for last.
- Source: [Yahoo, how Avatar made air bend](https://www.yahoo.com/entertainment/tv/articles/avatar-last-airbender-made-water-120000446.html).

Study material, not for import as is: KanaWorks SAKUGA VFX has hand-drawn wind flipbooks (vortex, impact, aura,
slash) to study frame by frame. Its licence is free under USD 1M revenue with a required credit, not CC0, so any use
would need an `ASSETS.csv` row and the credit ([GitHub](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx)).

## 2. Thunder Gust (lightning and wind)

### Lightning tornadoes in fiction and in nature

- **Naruto, Raiga's Lightning Dragon Tornado** (Rairyu no Tatsumaki): he holds lightning in and around himself and
  spins into a vortex of wind and electricity; even if it misses, lightning jumps from the vortex to a nearby target.
  This is the closest match to Thunder Gust, shock chain included
  ([Naruto wiki](https://naruto.fandom.com/wiki/Lightning_Dragon_Tornado)).
- **The Seven Deadly Sins, Castle of the Thunder Dragon**: Gilthunder's lightning wraps the target while Howzer raises a
  tornado; together they make a black tornado
  ([Nanatsu no Taizai wiki, Tempest](https://nanatsu-no-taizai.fandom.com/wiki/Tempest)).
- **Monster Hunter, Amatsu**: tornadoes and lightning together; purple circles on the ground show where bolts will land
  ([Monster Hunter Wiki, Amatsu](https://monsterhunterwiki.org/wiki/Amatsu_(MHGU));
  [Fextralife](https://monsterhunterrise.wiki.fextralife.com/Amatsu)).
- **Pokemon Legends: Arceus, Wildbolt Storm**: "summons a thunderous tempest and savagely attacks with lightning and
  wind" ([Dittobase](https://www.dittobase.com/legends-arceus/moves/wildbolt-storm)).
- **Genshin Impact, elemental absorption**: an Anemo tornado that touches Electro takes on Electro's colour while
  keeping its wind shape ([KQM](https://keqingmains.com/anemo-traveler/)). This is the rule for every wind reaction: the
  wind keeps its shape and the other element lives inside it.
- **Volcanic lightning, the real thing**: lightning inside a dark, churning column of ash; small "vent discharges"
  near the base, longer bolts higher in the plume, tearing through it
  ([Wikipedia](https://en.wikipedia.org/wiki/Volcanic_lightning);
  [Science Notes](https://sciencenotes.org/how-volcanic-lightning-works/)).

### Techniques

**T1. A funnel with mass.** A dark storm-grey funnel, wide at the top and narrow at the foot, three nested shells
spinning faster inside. Reads from above as a ring with a spiral inside, a big dark shape the bolts can light. Build as
W7, but the shells are cloud, not air: slate indigo, stepped toon bands, pale dust on the outer edge. Sources: W7's,
and the 80.lv Unity tornado's dark outer cloud over a glowing core.

**T2. Lightning lights the funnel from inside.** When a bolt fires, the band of the funnel round it flashes violet for a
frame or two, as an ash plume lights up. Reads from above even when the bolt is thin, because a whole band of the funnel
changes value. Build: an emissive band mask on the shells, pulsed by the bolt's cue (**flicker**, 1 to 2 strobes). The
80.lv tornado fakes lightning by lighting the clouds with an animated mask. Sources: volcanic lightning above;
[80.lv, Creating a Tornado in Unity](https://80.lv/articles/creating-a-tornado-in-unity).

**T3. Arcs ride the spiral.** The jagged arcs climb along the funnel's bands as helices, not straight lines through the
enemy. Reads as electricity carried by the wind, which is the owner's "wind mixed in with electricity". Build: a jagged
ribbon laid on the helix (a curve following the shell, with random kinks), redrawn every few frames (**flicker**),
moving up (**spiral**: orbit plus rise). Source: Raiga's tornado (lightning held in and around the vortex).

**T4. Arcs leap out to neighbours.** From the side of the funnel, arcs jump to the nearby enemies: the shock chain the
owner liked, now born from the tornado. Build: the existing chain bolts, starting on the funnel's surface instead of the
landing spot. Source: Raiga's tornado.

**T5. A cloud cap.** A flat disc of dark cloud spins on top, flickering with light inside. From 55 degrees down the top
is the most visible part of a column, so the cap is where "storm" reads. Build: a toon-shaded cloud disc mesh,
**spin** slowly, emissive mask flicker. Sources: Amatsu's storm; the wide-topped tornado profile in the 80.lv Unity
article.

**T6. Ground telegraph and scorch.** A spiral of combed dust on the floor before it rises, and a scorch fern wound into
a spiral after it. Build: floor decal (**spread**, **spin**). Sources: Amatsu's purple ground circles; `LOOKS.md`
(scorch figures that crackle cyan-violet at their tips for a moment).

**T7. Debris and sparks stream up.** Leaves, paper scraps and a few cyan sparks climb the funnel. Reads as wind (carried
solids) and lightning (sparks) together. Build: W3's debris on a rising helix; sparks as short-lived bright specks.
Source: `LOOKS.md` ("sparks streaming along the gust").

**T8. A dark tornado makes the bolts pop.** Keep the funnel dark and the bolts the only bright thing, white only at a
bolt's core. Sources: Seven Deadly Sins' black tornado; Riot's value rules.

**T9. Flicker discipline.** Few strobes, short. The owner asked for Seized to flicker less so it does not hurt players'
eyes; the same goes here. Two or three bright moments in the whole look, not a continuous strobe.

## 3. Carried Embers (fire and wind)

### What makes the owner's photo read

Looked at directly (the photo, downloaded and viewed for this note):

- **A twisting column**, many times taller than it is wide, slightly S-curved. Its surface is covered in diagonal flame
  streaks that wrap round it like a barber's pole, so the spin is visible in a still frame.
- **A narrow, pinched base**, the hottest and brightest part: almost white-yellow where it meets the ground, as if
  poured from a small hole.
- **Colour bands from the inside out and from the bottom up**: white-yellow core, then yellow, then orange, then a
  red-orange ragged edge. Lower is hotter and paler; higher the column keeps a yellow core but its edges redden.
- **Torn flags of flame** peel off the sides at a few points and trail sideways, so the edge is ragged, not a clean
  cylinder.
- **The smoke** is a much wider, soft, dim column behind and to one side, lit orange-red from the fire and fading to dark
  above. The flame column is sharp; the smoke is soft and slow. That contrast is most of the picture.
- Dark ground and a black sky: the fire is the only colour, which is exactly our look.

From the sources:

- Most fire whirls are 10 to 50 m tall but only a few metres wide; they are yellow from soot; smoke usually reveals the
  rotation; strong ones lift burning bark, and winds aloft carry those embers away from the fire
  ([Wikipedia, Fire whirl](https://en.wikipedia.org/wiki/Fire_whirl)).
- The core turns almost as a solid body and turns faster higher up; a line-fire study found the flame only weakly
  swirling near the surface, with strong inflow toward the base, and the bottom boundary layer drives the burning rate
  and flame height
  ([MDPI review, formation and precession of fire whirls](https://www.mdpi.com/2571-6255/4/3/43);
  [IAFSS, fire whirl from a line fire in cross wind](https://publications.iafss.org/publications/fss/11/1420/view/fss_11-1420.pdf);
  [ScienceDirect, bottom boundary layer of fire whirls](https://www.sciencedirect.com/science/article/abs/pii/S1540748918300099)).
- Whirls can be conical or nearly cylindrical with an open flame tip
  ([ScienceDirect, conical to cylindrical fire whirl](https://www.sciencedirect.com/science/article/abs/pii/S1540748920300018)).
- They form where the fire's updraft tilts wind shear upright, for example on the lee side of a plume or where wind
  crosses a line of fire ([Forthofer, review of vortices in wildland fire](https://onlinelibrary.wiley.com/doi/10.1155/2011/984363);
  [IAFSS, fire whirl from a line fire in cross wind](https://publications.iafss.org/publications/fss/11/1420/view/fss_11-1420.pdf);
  [NPR, scientists learning about fire tornadoes](https://www.npr.org/2021/08/23/1029638870/fire-tornadoes-scientists-funnels-flames)).
  So a gust over a burning enemy making a whirl is physically right.

### Techniques

**F1. Twisted column with wrapped flame streaks.** A twisted cylinder mesh whose flame texture pans up along the twist,
so the streaks wrap it diagonally. Reads from above because the diagonal bands show the spin on the near side and the
column throws a bright vertical stroke. Build: Blender twisted cylinder (slight S), flame texture with U along the
helix; **spin** the UVs upward; toon ramp in three or four flame values. Sources: the photo;
[80.lv, Creating a Tornado in Unity](https://80.lv/articles/creating-a-tornado-in-unity) (texture follows the spiral
because the mesh is twisted); [Gabriel Aguiar, Unity Fire Tornado](https://www.artstation.com/artwork/2qlzZv).

**F2. A pinched, white-hot foot.** The column necks down to about a third of its width at the ground, brightest there.
Reads as "rising out of this spot" and puts the focal point on the enemy's feet. Build: mesh profile plus a vertical
colour ramp (hot at the bottom). Sources: the photo; the bottom boundary-layer papers above.

**F3. Colour bands.** White-yellow core, yellow, orange, red-orange rim; paler low, redder high. Build: a ramp on the
Fresnel or view angle (rim redder) times a ramp on height. Riot's analogous-palette rule fits: all the bands are fire
colours. Sources: the photo; Wikipedia (yellow from soot).

**F4. Torn flame flags.** Three or four tongues peel off the column's edge and trail downwind, then go. They break the
silhouette so it reads as flame, not a glowing pipe. Build: Smoulder's flickering flame (the one the owner praised),
coloured as real fire, as flipbook cards stuck to the column's edge; **lick**, **flicker**. Sources: the photo;
the owner's Burn comment (037da842).

**F5. The smoke column.** A wide, soft, dim column behind and downwind of the flame, lit orange at its base and dark
brown-grey above, spinning slowly. It is the secondary element: low value, soft, slow. Build: a larger, cruder twisted
shell with a soft stepped cloud texture, slower **spin**, leaning downwind; dissolves upward. Sources: the photo;
Wikipedia ("smoke usually reveals the rotation"); Riot's primary and secondary.

**F6. Embers thrown downwind.** Bright specks and a few larger burning scraps leave the column's top and arc away
downwind to the other enemies. Reads as the "carried" in the reaction's name and as the area it covers. Build: small
emissive meshes or particles on **drift** paths with gravity. Source: Wikipedia (embers carried away by winds aloft).

**F7. The column leans and walks.** A whirl leans with the wind and travels. Leaning it downwind, away from the player,
shows the direction the fire is carried. Build: rotate the column's root a few degrees and **sweep** it along the lane.
Sources: Forthofer and the IAFSS cross-wind paper above.

**F8. Inflow on the floor.** Dust and grass streaks pulled in toward the foot (a spiral pointing inward), then a char ring
where it stood. From above this ground swirl is the clearest sign of a whirl. Build: floor decal with streaks, U toward
the centre, **spin** inward; then a char decal. Source: the MDPI review (strong inflow near the surface).

**F9. No hard edges at the top or bottom.** Mask the top and the bottom of the column with vertex alpha so it never ends at
a line; that was the main criticism of a fire tornado on the Real Time VFX forum, together with "lacking characteristics
... someone might expect" of a tornado ([Real Time VFX, Fire Tornado Effect](https://realtimevfx.com/t/fire-tornado-effect/25604)).

**F10. The wind keeps its shape; the fire fills it.** The gust that arrives is wind (W1 ribbons, pale); as it wraps the
burning enemy its ribbons catch from the tail forward and turn to flame. Source: Genshin's absorption
([KQM](https://keqingmains.com/anemo-traveler/)); the owner's 2026-11-19 comment 691903e2 ("the wind picking up the
fire, and turning into the wind effect but colored the fire colors").

## Recommended designs

Colours used below:
- Wind body: pale dust `#D1D6C7` (the code's `WindPale`, 0.82, 0.84, 0.78). Wind edge: the element's mint
  `#9EF2CC` (0.62, 0.95, 0.80) as a thin rim, with a dark teal ink line `#2F5E55`. Highest value about 85%; never pure
  white.
- Carried bits: dry leaf ochre `#8C7A4A`, grey-gold grass `#9A8F6A`, paper `#C9C2AE` with ink outline.
- Floor comb: two steps darker than the floor's own colour plus a pale stripe `#B8B6A6` at 50%.
- Storm funnel: slate indigo `#2B2F45` to `#4A5170`, pale dust edge `#B8BDB0`.
- Lightning: cyan `#73C7FF` (the element colour) with violet fringe `#8A6BFF`; white only at a bolt's core.
- Fire: core `#FFF2B0`, yellow `#FFC93C`, orange `#FF7A1E`, rim `#D8401A`; smoke lit `#6A2E1E`, smoke dark `#2E2622`.

### A. Unsteady (the wind affliction on an enemy)

Idea: **the enemy stands in a small whirl of wind that has caught a few leaves and combs the ground round its feet.**
Nothing passes through the body; everything goes round it.

Pieces:
- **Wrap ribbons** (W1): two thin tapered curl ribbons, 4 to 6 cm wide, at hip height (0.6 m) and chest height (1.2 m),
  radius 0.65 m, tilted 15 degrees in opposite directions. Pale body, mint rim, teal ink edge.
- **Caught leaves** (W3): one leaf or paper scrap per stack (up to five), riding the ribbons' orbits and tumbling.
- **Floor swirl** (W5): a 1 to 1.1 m radius ring of combed dust and laid grass under the feet, spiralling in the
  wind's turning direction.
- **Lean** (W4): the body leans 3 degrees downwind and sways at 1.5 Hz; cloth and hanging parts stream.

Loop (1.2 s, repeats while it lasts):
- 0.00 s: ribbon 1 starts drawing on behind the body (head first), orbiting at 300 degrees a second.
- 0.30 s: ribbon 1 fully drawn, about 120 degrees of arc, its head curling into a hook.
- 0.55 to 0.75 s: ribbon 1's tail erases toward the hook; the hook unrolls and fades as it leaves.
- 0.60 s: ribbon 2 starts on the other side and does the same, so one ribbon is always visible.
- Throughout: leaves orbit at 200 degrees a second and spin about their own axis; the floor swirl pans at 90 degrees a
  second; the lean sways.

Stacks: 1 stack, one ribbon and one leaf. 3, both ribbons and three leaves. 5, a third ribbon at the shoulder, orbit
speed 1.5 times, the floor swirl a step paler.

Arrival (when Unsteady is first laid, about 0.5 s): cause, 0 to 0.12 s, two streaks come in from the blow's direction
and **bend round** the body on both sides instead of going through it; impact, 0.12 to 0.3 s, they close into the orbit,
and leaves are snatched off the floor into it; aftermath, the loop takes over.
End (0.5 s): the ribbons unwind outward into straight streaks and drift off downwind; the leaves drop, tumble and settle
on the floor, then fade with the floor swirl. Nothing switches off.

### B. Wind's strike and swing look

Idea: **the swing is a pale crescent of air that does not stop at the end of the arc but blows on and curls away, and
on a hit it parts round the body.**

Pieces:
- **Crescent ribbon** (W6) on the swing arc: a clean outer edge, needle tips, widest a third back from the head; inner
  edge teal, body pale dust, outer rim mint.
- **Curl-off** (W1): at the arc's end the ribbon's head breaks into two or three curl hooks that travel on 1 to 1.5 m
  along the swing's tangent and unroll. This answers the earlier comment that the end "cuts off randomly".
- **Combed lane** (W5): five to seven thin streaks on the floor along the swept arc, laid grass and dust.
- **Kicked grit** (W3): three to five grit clumps or leaves thrown along the tangent, landing and settling.
- On a hit, a **parting**: the crescent splits round the struck enemy, the two halves bending past both sides and
  rejoining behind it as one curl (wind round an obstacle), with a dust puff behind that swirls as it dissolves (W8).

Timeline (about 1.0 s):
- 0.00 to 0.08 s (cause): the charged sheath already on the blade brightens; dust lifts at the feet.
- 0.08 to 0.20 s (impact): the crescent wipes in with the blade; on a hit at about 0.15 s it parts round the enemy; the
  enemy is shoved and leans away; leaves are kicked up.
- 0.20 to 0.55 s (aftermath): the tail erodes toward the head; the head curls off and travels on; the lane streaks
  fade from the swing's start to its end.
- 0.55 to 1.00 s: the curls unroll and go; grit and leaves settle; the dust puff behind the enemy twists away.

### C. Thunder Gust (1.5 s)

Idea: **a dark little storm funnel rises round the enemy with lightning living inside it, climbing its spiral and lighting
it from within, then it throws the enemy and spits arcs to its neighbours.** One straight bolt only, down the middle.

Pieces:
- **Funnel**: three nested twisted shells (T1), 3 m tall, 1.4 m radius at the top, 0.35 m at the foot; inner 540,
  middle 360, outer 200 degrees a second, all the same way. Slate indigo with stepped bands, pale dust edge.
- **Cloud cap** (T5): a 1.8 m dark disc spinning slowly on top.
- **Helix arcs** (T3): three or four jagged cyan-violet ribbons climbing the shells.
- **Inner lighting** (T2): violet emissive band on the shells where an arc is.
- **Debris** (T7): six leaves and paper scraps and a handful of cyan sparks climbing the funnel.
- **Floor**: combed dust spiralling inward (T6), then a spiral scorch fern.
- **Chain arcs** (T4): the kept shock chain, now leaping from the funnel's side.

Timeline:
- 0.00 to 0.35 s (cause): wind ribbons (as Unsteady's, but four and faster) draw on round the enemy and climb as they
  orbit (**spiral**); the floor combs inward; leaves are lifted; two tiny cyan sparks at the foot (the vent
  discharges).
- 0.35 to 0.95 s (impact): the funnel shells rise out of the ribbons from the foot up (0.35 to 0.55 s); the cap forms
  on top. Arcs climb the spiral in turn at 0.5, 0.62 and 0.74 s, each visible for about 0.08 s with two strobes, each
  lighting its band of the funnel violet. At 0.80 s the one main bolt drops from the cap down the axis into the enemy,
  white core, and the enemy judders, lifted 0.3 m and turning with the funnel.
- 0.95 to 1.5 s (aftermath): the funnel throws the enemy its 3 m and unwinds: the shells widen, slow and fade from the
  top down; the cap thins. Three arcs leap from the funnel's side to the three neighbours (Static). Leaves and sparks
  fling out along the tangent and fall. The spiral scorch fern on the floor crackles at its tips once and stays dark.

Keep it apart from Sickle Wind's column (pale bands with steel shards): Thunder Gust is dark, wide-topped, capped and
lit from within.

### D. Carried Embers (1.5 s, an area effect)

Idea: **the gust wraps the burning enemy, catches its flame and stands up as a real fire whirl with a pinched white-hot
foot, wrapped flame streaks, torn flags and a wide lit smoke column; the whirl leans and walks downwind and pours embers
onto the enemies behind.** Show the whole lane and the three enemies it reaches.

Pieces:
- **Wind ribbons** (W1) that catch fire from the tail forward (F10).
- **Fire column** (F1 to F4): a twisted, slightly S-shaped column 3.5 m tall, 0.9 m wide in the body, necking to 0.3 m
  at the foot; flame streaks panning up the twist; three or four torn flags; colours as above, hottest at the foot.
- **Smoke column** (F5): 2.5 to 3 m wide, behind and downwind, lit `#6A2E1E` low and `#2E2622` high, spinning slowly.
- **Embers** (F6): about twenty specks and five larger burning scraps.
- **Floor**: inflow streaks spiralling in to the foot (F8), then a char lane combed downwind, about 7 by 1.5 m, with
  embers glowing in it (the existing ground mark).

Timeline:
- 0.00 to 0.30 s (cause): two or three pale wind ribbons arrive from the player's side and wrap the burning enemy. Its
  flames lean over and are drawn up into the ribbons, which catch from the tail forward and turn orange. The floor
  combs inward to its feet.
- 0.30 to 0.90 s (impact): the ribbons close into the fire column, which rises from the foot (0.30 to 0.50 s), white-
  hot at the bottom, streaks wrapping up it. Flags peel off the sides and trail downwind (**lick**, **flicker**). The
  smoke column swells behind it. From 0.60 s the column leans 10 degrees away from the player and walks 2.5 m down the
  lane (**sweep**), the enemy staggering out of it.
- 0.90 to 1.50 s (aftermath): the column's top tears off downwind as a burning ribbon of wind and a spray of embers
  that arc onto the three enemies 4, 5 and 6 m behind, each catching a small flame leaning with the wind. The column
  unravels from the top into smoke; the foot shrinks to a spot and goes. The char lane stays, combed downwind, embers
  dimming in it.

Keep it apart from Thunder Gust: bright, narrow, pinched at the foot, smoke behind, travelling; Thunder Gust is dark,
wide-topped, capped and standing.

## What to avoid

- **The single line through the enemy.** Nothing in a wind look passes through the body. Wind goes round it, parts
  round it, or blows past it.
- **Random white shapes.** No piece in a wind look is pure white or appears and vanishes in one frame. Places to look
  first: older wind pieces lightened toward white (`ElementSubstance.Pale` in `AfflictionLook`'s Unsteady, and
  `TapForm`'s fallback that uses `Color.white` on every third cast when the element art is not loaded) and the gust sheet
  that breaks into "hard flecks" at its end (`FormFx.BlowSheet`); the white landing frame (`ReactionMotion`'s flash) is
  for cold, hard things and must not be on wind. Use pale dust with a mint rim and a teal ink edge, carried solids, and
  ends that drift, unroll or settle.
- **Uncarried flecks at a landing or an uproot** (Seed Storm's "random stuff pop[s] up at the uproot location"). If
  something appears there, it was carried there, or it is dust thrown up by the impact that falls back.
- **Straight static lines.** Wind lines curve, move along their own length and are drawn on and wiped off. Diagonal
  noise crawling across a mesh reads as texture, not air.
- **Overlaid copies of each element.** Each reaction is its own look; in wind reactions the wind keeps its shape and the
  other element fills it (absorption), as in Ill Wind, which the owner praised.
- **Hard ends.** No ribbon, column or sheet ends at a cut line: taper, vertex-alpha masks, curls.
- **Strobe noise.** Two or three bright moments per look; few strobes, as the owner asked for Seized.
- **Thin bolts as the only lightning.** Light the funnel from inside so a band of it changes value.
- **A glowing pipe for the fire whirl.** It needs the pinch at the foot, wrapped streaks, ragged flags and the soft
  smoke; and no hard top edge.
- **Too many small parts.** Per look: one primary shape, two or three secondary layers, carried solids in single digits.

## Other sources looked at

- [GDC 2017, Art Directing VFX for Stylized Games](https://www.gdcvault.com/play/1024715/Art-Directing-VFX-for-Stylized)
  (free on GDC's YouTube channel; only the summary was read).
- [Real Time VFX, stylized trails in Unity (Gabriel Aguiar)](https://realtimevfx.com/t/heres-how-some-stylized-trails-are-done-in-unity/18709).
- [Real Time VFX, stylized topics](https://realtimevfx.com/tag/stylized).
- No written breakdown was found for Breath of the Wild's or Genshin Impact's own wind lines, Okami's Galestorm or the
  original Avatar series' airbending; the notes on those come from the summaries cited and from what the games and show
  are widely known to look like.
