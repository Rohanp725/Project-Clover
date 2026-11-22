# The owner's comments on the weapon swing films (2026-11-21)

The owner went through the companion document "Project Clover: Weapon swings on film"
(https://claude.ai/artifact/1DsBYw3SitkixsNv9jzHQN) on 2026-11-21 and left 12 open comment threads, all their own, all on
the Massive Sword tab. Every other tab (Overview, Katana, Scythe, Great Spear, Great Hammer, Twin Knives, Great Bow, Chain
Sickle, Fists, Full Body Armament, War Fan) has no thread. No thread has a reply. The owner said the comments apply to
every weapon: the swings themselves are good, and the problem is the end effect of the charged (heavy) attack for some
elements. They are kept here word for word, grouped by element, because the threads were taken off the page (resolved)
after this file was written.

Every comment is anchored on the **Trail alone** picture of its element's row (the film
`review/gifs/swings/alone/swing_gsword_<element>.gif`); none is on an Into enemies picture. Times are local (the
document's UTC times minus six hours); the comments were written between 14:17 and 14:29.

**How each comment is split.** No comment contains the word "edit". Two comments were rewritten in place by the owner
after they were first posted (Fire and Earth); for those both the first text and the current text are kept, and the
current text is the one that stands.

## Summary

| Element | Verdict | What needs work |
|---|---|---|
| Fire | Rework part | Keep the first kindle-like flames; the later flames burn off nothing and last too long |
| Lightning | Remove part | Keep the dash and the lightning; take out the effect left on the ground |
| Wind | Good | |
| Ice | Good | |
| Water | Good | |
| Earth | Good, with a question | Does it make sense for the heavy to cast the element, given the spells? |
| Metal | Rework | The final effect on the heavy charge, fully |
| Life | Good | |
| Light | Scrap and rework | The half circle that spawns on the final swing |
| Dark | Scrap and rework | The charged effect after the swing |
| Poison | Rework | The charged effect after the swing is the old poison look |

## Fire

- Thread `ba2604b4-ca0f`, 14:17 (edited 14:22), Massive Sword tab, Fire row, Trail alone picture.
  - First text (14:17): "The final heavy slash applies burn too wide. Not sure if this is the actual charged attack or a spell"
  - Current text (14:22): "For the final heavy swing, the initial flames that come up are good, the ones that are similar to the kindle effect. the later fire effects are a little unclear because its burning off of nothing, and burns for too long, so it looks like flames coming out of nothing"

## Lightning

- Thread `1d28ffd8-00e3`, 14:24, Massive Sword tab, Lightning row, Trail alone picture.
  - "The dash effect and the lightning effects are good on the charged attack, but the effect that remains on the ground is not good, just take it out for this"

## Wind

- Thread `f46ebc4f-78bc`, 14:24, Massive Sword tab, Wind row, Trail alone picture.
  - "Good"

## Ice

- Thread `d5de2611-b5ce`, 14:25, Massive Sword tab, Ice row, Trail alone picture.
  - "Good"

## Water

- Thread `1cadf90f-9cfe`, 14:25, Massive Sword tab, Water row, Trail alone picture.
  - "Good"

## Earth

- Thread `bcfb61dd-6692`, 14:25 (edited 14:26), Massive Sword tab, Earth row, Trail alone picture.
  - First text (14:25): "Good"
  - Current text (14:26): "Good but not sure if this coincides too much with the elemental spells and whatnot. Does it make sense for the heavy to have the element cast?"

## Metal

- Thread `e768109c-8126`, 14:27, Massive Sword tab, Metal row, Trail alone picture.
  - "The final effect on the heavy charge needs to be fully reworked but the swings are good"

## Life

- Thread `a59f13fd-3bc5`, 14:27, Massive Sword tab, Life row, Trail alone picture.
  - "Good"

## Light

- Thread `7a49a9d7-6993`, 14:28, Massive Sword tab, Light row, Trail alone picture.
  - "good, but on the final swing, the half circle that spawns is terrible needs a full scrap and then rework"

## Dark

- Thread `32fbf337-2499`, 14:28, Massive Sword tab, Dark row, Trail alone picture.
  - "Good but the charged effect after the swing needs to be scrapped and reworked"

## Poison

- Thread `14603175-efef`, 14:29, Massive Sword tab, Poison row, Trail alone picture.
  - "Good but the charged effect after the swing is old poison visuals and needs to be completely reworked"

## Owner's answers and the fix plan (2026-11-21)

The final big swing in every film is the **infused strike** (L2 + RB), not the charged heavy; the owner has been told. The strike art is shared, so every fix reaches all 11 weapons.

**The rule for infused strikes (the owner, on Earth's question):** keep an element effect, but it must grow out of the blade's cut (along the slash line and where it hits), so it reads as a sword move and never like a spell being cast. Wind, Ice, Water and Life were judged "Good" and stay as they are.

| Element | Fix | Who |
|---|---|---|
| Fire | Keep the first Kindle-like tongues. The ground burn lays a scorch with embers for its flames to burn on, lasts about 1 s (not 3 s), and stays tight to the cut (`ElementSteel.cs:200`, `:436-450`; `Hazards.cs:297-299`) | Unity |
| Lightning | Delete the floor scorch star (`StrikeArt.cs:74`); keep the dash and the bolts | Unity |
| Earth | The stones rise along the arc of the cut, not as a separate Broken Ground line ahead | Blender, then Unity |
| Metal | Scrap the disc and its dashed rings (`StrikeArt.cs:121-127`). The cut leaves a bright steel glint along its edge, and a spray of small steel shards rings off whatever it hits | Blender, then Unity |
| Light | Scrap the sundial (`StrikeArt.cs:132-133`). A warm beam of light sweeps down along the cut's line and flares on what it hits, then fades, with no floor shapes | Blender, then Unity |
| Dark | Rework Taken. Shadow hands rise only from enemies the cut actually hits, grip them and drag them a step; nothing is drawn on a miss; near-black | Blender, then Unity |
| Poison | Replace the old elements 2 look (`StrikeArt.cs:153-164`; `ElementMoves` Strikes[10] is null). Rolling green miasma bursts out along the cut and from what it hits, then drifts and thins over about 2 s | Blender, then Unity |

**Checks before the owner sees anything:**
- Film only these seven elements' strikes, on every weapon, into enemies and with the trail alone.
- Check each film against this table and against the standing rules: animated, no white, no rings, on the body, nothing drawn on blades.
- Have an independent review agent look for faults, then swap the films into the doc in place.

## Owner's verdicts on the Blender strips (2026-11-21)

- **Approved:** Earth and Metal, as they are.
- **Light:** "light from the cut". No sky beam: the cut leaves a line of warm glare that flares on each struck enemy and breaks into motes.
- **Dark:** "Do the many small hands, but dont actually move the enemy into the floor because that would read badly in my opinion". Many small, real hands tug downward at the legs from the shadow; the enemy stays on the floor.
- **Poison:** "dont really like the clouds coming off the swing". It is now veins plus spatter: dark green veins spread over the struck body from the wound, and venom spatters to the floor in sizzling droplets. No clouds off the swing.
- **Done in Unity (be75cac, 6c2b00c, 073fff5, df35a8d):** Fire and Lightning. Fire's ground burn is now 4 small patches on char with ember cracks, lasting about 1 s; Lightning's floor star is gone.
- **Open from the films:**
  - Lightning's lunge carries the player so far that the crop splits player from enemies.
  - A white-blue bolt from the Storm Caller enemy appears in the Fire films.
- **Done in Unity (2eaddfc, 20490f4, 2c4b056, 63bd109, bb3bd1a, 0c246f1):** Earth, Metal, Light, Dark and Poison play the Blender worker's final strikes (staging/blender/2026-11-21_strikes), on all 11 weapons. Pieces made once per enemy the cut hits are drawn only on those enemies, so Dark and Poison draw nothing on a miss. Earth's stones lift what they reach 0.55 m and set it down. Dark's hands have a violet rim and never lift or sink the enemy. The arcs are sized to the weapon's reach: as drawn for the Katana, at most 1.3 times larger and at least 0.85 times smaller. The old disc, rings, sundial, Taken and Venom Flick looks are no longer played. Films: review/gifs/swings/ and review/gifs/swings/alone/; stills on one soldier: review/gifs/swings/stills/.
- **Fixed after the coordinator's review of those films (5ceeafc, d69a463):**
  - Swing direction: the strike look is now mirrored about the strike's line when the blade cuts left to right. SwingTrails measures how far the fastest blade tip turned round the player. The Katana, Twin Knives, Chain Sickle, Fists, Armament, War Fan and Great Bow are mirrored; the Massive Sword's rising cut, the Scythe, the Great Hammer and the Great Spear turn the recipe's way. The glare, glint, stones and per-enemy pieces now run with the blade.
  - Gold-white starbursts: these were the critical blow's star and spark spray (`PlayerCombat`; a Marked enemy takes critical blows). They were thrown untinted, and the glow shader gave every effect with a hot core a white middle whatever its tint. The star and sparks are now gold, held low. A tinted effect's core now takes its tint's hue, dimmer when the tint is low; plain effects keep their white. On every weapon's Light strike, pure-white pixels are now at or near the background level (the Katana's peak fell from about 46,000).
  - The film setup stands the player and the enemies back on their spots before the strike.
- **Traced, not changed:**
  - The yellow dashed ring with an X is the finish mark (`StaggerMark`, built by `WorldBars.FinishMark`) over a staggered enemy. In the films it was over the Knife Acolyte, which the chain had left 0.9 to 1.1 m in front of the player, so it drew over the player. The target is right; it is a decided cue, though it is a ring.
  - The Massive Sword's strike still misses with Earth, Light and Poison, even with everyone back on their spots. Its rising cut flings the enemies (`Fling = 6`) and the player turns about 35 degrees before the element's lane lands, so the lane passes them. Dark and Metal land on one enemy. The Great Bow's Metal and Poison strikes also land on no one. These are combat rules, not the film setup.
  - Other pure white still in the films, not from the strikes: the Great Hammer's slam (a white glow ring and shock behind the target), pale steel streaks on Metal's Massive Sword carrier blow, and the lime Armament in Poison.
- **For the weekend round:** Earth's charged-hit cracks read as brown thorns at the enemies' feet.
- **Still open:** Poisoned's lime clouds and Marked's sky beams and floor discs cover the struck enemies. Poison's vein web is faint on the real models; the Blender worker suggests a vein mask in the body's shader.

## Strikes per weapon motion (the owner, 2026-11-21)

The owner noticed that the infused strike plays the same arc look on every weapon: "the hammer is slamming down, and i still am getting a slash of earth spikes pop up". They said the effects "are now pretty good, they just need to follow the path of the weapon, and for things like a hammer smash down, it needs to be instead of a bunch of earth in a slash, it needs to be a area that is hammered down has a big spike come out, and so on for each type of weapon".

**Decided:**
- **Strike shapes by weapon group.** Every element gets a look for each shape:
  - ARC: Katana, Massive Sword, Scythe, War Fan. These use the current approved looks, mirrored to follow the blade.
  - THRUST, a line ahead: Great Spear, Twin Knives.
  - SLAM, an area where it lands: Great Hammer, Fists, Armament.
  - SHOT, at the arrow's impact: Great Bow.
  - LASH, along the chain's curve: Chain Sickle.

  Confirm each weapon's real motion from its clip before building.
- **Slam:** a hammered-down patch of cracked ground where it lands, with ONE big hero piece bursting from the centre:
  - Earth: a great spike.
  - Ice: a frozen spike.
  - Fire: a flame pillar.
  - Water: a geyser.
  - Lightning: a bolt down onto it.
  - Metal: a forged blade.
  - Life: a thorn.
  - Dark: grasping hands.
  - Poison: a venom burst.
  - Light: a sunburst flare.
  - Wind: a gust burst of curls.
- **Order:** slam first, then thrust, then shot and lash. The owner sees each shape's strips before Unity builds it.
- **Twin Knives grip, fix now.** The knives sit at wrong angles and the hands don't close on the handles. Re-angle them into a proper grip, close the fingers, and check close-ups through the whole moveset.

## The hammer slam, the owner's verdicts (2026-11-21, evening)

- **Approved:** Earth (a stone spike), Ice (a frozen spike) and Life (a curved thorn).
- **Fire:** "one big flame that erupts then dissapears".
- **Water:** "a big splash".
- **Metal:** a bigger, sharper forged blade with a steel glint.
- **Dark, an implosion:** the shadows lurch inward, a black pit forms for a beat, then a smoky ripple of darkness bursts out and knocks the enemies back.
- **Wind, a downdraft burst:** curls race out low along the ground, with dust and grit, and shove the enemies back.
- **Lightning, the hammer as conductor:** one huge bolt hits the hammer head; on landing it forks through the cracks into the struck enemies.
- **Light, light from the cracks:** gold light bleeds out of the cracks, then a short column bursts up from the ground and fades into motes.
- **Poison, a venom geyser:** a liquid eruption of dark green venom, raining round drops that pool and bubble.
- **Unity note:** the slam lands 2.5 m ahead (`PlayerCombat.cs:1475`). Remove the hammer's white glow ring (`PlayerCombat.cs:1476-1477`).

- **Slam round 2 (the owner):** "All of them are good in concept, but the visuals in unity need to be well implemented." Polish five before the build: Water gets a crown splash wall, Metal a blade with a hilt and guard, Poison a brighter sickly rim, Light smooth beams with no flame-like spikes, and Wind fewer, fatter curls. Unity must then match the strips closely; check the frames against them before showing the owner.
- **Metal slam (the owner, round 3):** "the sword is too big out of the ground now, I dont think the hilt should come all the way out of the ground". Only the blade (at most the guard) rises; the grip and pommel stay buried; less height overall.
- **Slam built in Unity (2026-11-21, evening run):** all 11 slams play the Blender worker's round-5b looks (`ElementLookBuilder.Slams.cs`, keys `e3_slam_<element>` and `e3_slam_<element>@b`) for the Great Hammer's infused strike only, about the spot 2.5 m ahead where the head lands; Lightning's look starts 0.13 s before the head lands so the bolt strikes the raised head. Pieces made per enemy struck (Dark's smoke burst, Poison's bruise and veins, Lightning's jumps) go only on enemies the slam reaches. The hammer's white glow ring is gone from all its slams; the infused slam also drops the old dust and the swing trail's ground burst, since the new look replaces them. Checked with one still per element at the strip's peak beat beside the strip (`review/checks/slam_compare/`); fixed from those: the cracks lay under the patch (raised), the Lightning flash, forks and sparks and the Metal glint bloomed white (darkened), the Metal blade and Life thorn read too pale (darkened), the Metal glint was a star wider than a soldier (shrunk). **Gameplay change for the owner to confirm:** with Lightning the hammer now slams in place (still 1.5 times the blow) instead of Flash Cut's 5 m lunge, which would carry the player through the spot the bolt strikes. **What still differs from the strips:** the game's own layers the stand-ins lack sit on top: Fire's Burn sets the struck bodies alight at once, Ice's Chill frosts them, and each element's swing trail draws its sweep (a pale wedge; Lightning's a white-blue spray that hides part of its crack star). The full motion is filmed at night (`scratchpad/run/NIGHT_FILMS.md`).

- **Scythe SPIN (2026-11-22):** the owner approved all 11 as they are (`staging/blender/2026-11-21_strikes/spin/`). Next: THRUST (Great Spear).
- **New standing rule (the owner, 2026-11-22):** filming and GIF runs happen only at night, unless the owner approves otherwise. Unity keeps a night film list in `scratchpad/run/NIGHT_FILMS.md`.
