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
