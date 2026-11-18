# Elements, part B: Earth, Metal, Light, Dark, Poison

The element format of `docs/design/steel/BRIEF.md` for five elements, written to `docs/design/STEEL_AND_ELEMENT.md`
sections 3, 5 and 6. Everything here is a **Proposal**. Every number is a first guess.

## Index

**Earth** (Tremor): build stone and break it over the enemy.
- Stone Rise (built): opener; the ground breaks under the enemy and stone is left standing.
- Fissure (built): breaker; a crack runs ahead and erupts.
- Quake Step (built): breaker; a stamp that breaks an enemy's attack.
- Mountain Stance (built, re-aimed): counter; go wholly to stone, and every blow it turns loads your heavy.
- Under the Mound (new): movement; travel under the ground and burst out.
- Cairn (new): detonator, mana (Earth's larger spell); stones heap on every shaken enemy and spend the shaking.
- Charged blade: grit on the edge; the blows cannot be stopped.
- Infused strike: Broken Ground, slabs heaved along the swing; all in, the Mountain Cleaver.
- Devotion, Stone Remembers: nothing you raise falls by itself.
- Reaction lift, Left in Rubble: Tremor's reactions hit harder and leave rubble that stands as your stone.
- Passives: ten marks, the root, three majors, three keystones, the gift.
- Surge, Bedrock: unchanged, and it takes Earth's base buff (afflictions shed).

**Metal** (Sundered): meet the blow exactly, and drive steel in.
- Flung Blade (built): opener; a blade stands in the enemy until a blow drives it.
- Circle of Steel (built): counter; a blade raised all round.
- Returning Edge (built): breaker; a turning blade out and back.
- Poured Bronze (new): detonator; molten bronze poured into the notches, and it sets.
- To the Blade (new): movement; fly to your own planted steel along a line of sparks.
- Grave of Swords (built): mana spell; twelve blades called through what you strike.
- Charged blade: cherry-red seam cooling through the temper colours; blows pass any guard.
- Infused strike: Struck Plate, a ringing ripple; all in, Red Iron.
- Devotion, The Guard Holds: the guard never breaks, and you take less.
- Reaction lift, Every Crack Rings: Sundered's reactions notch all they reach.
- Passives: ten marks.
- Surge, Temper: unchanged; it is the defence the user asked of Metal.

**Light** (Marked): mark the enemy, then make the few blows that matter count.
- Glare (built): opener; a narrow ray that marks.
- Step Into Light (built): movement; step through the enemy on a line of gold dust.
- Ring of Spears (built): breaker; six spears of beaten gold drive in.
- Standing Light (built): counter; a lamp whose glow rewards the perfect guard and purifies the ground.
- Reckoning (built): detonator; a seal that pays back what you did.
- Daybreak (built): mana spell; light gathered on the blade by blows, loosed as a beam.
- Charged blade: gilt along the edge; the third blow throws a ray.
- Infused strike: Ray Along the Edge; all in, The Sun on the Blade.
- Devotion, Full Daylight: a mark is not spent by blows, it burns out.
- Reaction lift, Seen Plainly: Marked's reactions are critical.
- Passives: ten marks.
- Surge, Clarity: unchanged; it is Light's purification.

**Dark** (Cursed): pay in health, take it back out of the enemy, rise once.
- Grasp (built): opener; a black ball that drags.
- Debts Called (built): breaker; pull in, burst out, harder the nearer death.
- Devouring Maw (built): detonator; a jaw that spends the Curse and feeds you.
- Gone to Ash (new): counter; the blow scatters you as ash.
- Second Shadow (built): movement; your shadow fights, and you change places.
- Pawned Heart (built): the price spell; paid in health, not mana.
- Charged blade, Shadow Cut Loose: violet-black smoke sinking off the edge; the third blow cuts the enemy's shadow loose.
- Infused strike: Taken, a cut that drags; all in, Yama's Noose.
- Devotion, Nothing Stays Buried: the Cursed dead stand up and fight for you.
- Reaction lift, Collected Twice: the next blow on what a Curse reaction struck repeats the reaction's damage.
- Passives: ten marks; the Rise moves to the root.
- Surge, Hunger: free of mana; give it a cooldown.

**Poison** (Poisoned): lay doses, wait, burst; the poison feeds you.
- Miasma (built): opener; a cloud to hold a fight in.
- Five Needles (new): breaker; needles thrown into the pressure points.
- Coiled Strike (built): counter; coil and strike through.
- Blister (built): detonator; a blister that swells with each bite.
- Share Alike (built): movement; rush through and share the sickness.
- One by One (built): mana spell; every poisoned enemy bursts down a line.
- Charged blade: venom in strings off the edge; the third blow makes the poison bite.
- Infused strike: Venom Flick; all in, Nine Times Refined.
- Devotion, Tasted Every Herb: no poison harms you, and clouds heal you.
- Reaction lift, The Delayed Touch: poison's reactions are held in the body and land later, doubled.
- Passives: ten marks; the root heals from poison damage.
- Surge, Fever: unchanged in cost; it raises the healing the root already gives.

What to cut from the built content is at the end, before the sources.

---

## Assumptions shared by all five

- **Cooldowns** are the times before weapon play shortens them (section 3.1). Basic spells sit at 6 to 12 s and
  never cost mana; the one larger spell in each roster is paid in mana (35 to 50), as section 3.2 asks. Dark's is
  paid in health (Pawned Heart).
- **Empower** takes the place of the old plain minors: it is the one numbers mark on each spell branch. The minors
  that only lowered a tap spell's mana cost (Near to Hand, Ready Steel, Ready Light, Ready Poison) have nothing left
  to do, because tap spells no longer cost mana.
- **Masters** are tied to timing, as section 5.2 asks, and each element leans on the timing that is its own substance
  (the feel review of 2026-11-18): Earth standing still or as a heavy blow lands; Metal meeting the blow exactly (the
  perfect guard, the land's lesson); Light a critical blow or a straight line; Dark when you are hurt or have paid;
  Poison on the poison's beat. The Masters the review judged sound keep their perfect guard, dodge or finisher.
- **The weapon tree** is called the Way. "Forge" in this file means only the Earth and Metal mix.
- **Infused strikes** (L2 with R1, and L2 with R2) each run on their own 8 s cooldown, shortened by weapon play. The
  all-in version on L2 with R2 keeps that cooldown and also costs 20 mana. This is now the shared rule for all seven
  files (`README.md`).
- **Marks cost** Learn 4, Empower 1, Shape 3, Master 3 Shards (11 a spell); Root 2, Major 3, Keystone 6, Devotion 6,
  Reaction lift 3; the god's gift is given, never sold. The same prices as `elements_a.md`.
- **Ten passives per tree** includes the root, the Devotion, the reaction lift and the god's gift. With six spells of
  four marks each, every tree is 34 marks.
- Numbers are kept in the range the built spells use: tap spells 8 to 20 damage and 15 to 45 stagger; a counter's
  answer up to 30 damage; a mana spell up to about 70.

---

## Earth

**The substance.** Stone that cracks, heaves and bursts: ochre dust, rust-brown grit, grey lichened stone, sod thrown
up with the stones in it, and the hexagonal tops of basalt columns, which read at once from above. Nothing glows.
**The myth.** The Cailleach dropping stones from her apron to make the hills, and striking the ground with her staff;
the beheading game of *Bricriu's Feast*; the King's Men who move only while unwatched; the barrows as roads; "stone
remembers". **The phenomena.** Columnar basalt from slow-cooling lava, the two waves of an earthquake (the quick thin
one, then the slow heavy one), sinkholes.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Stone Rise** (built, the tap spell) | Opener | 7 s | After a quarter second the ground breaks under the enemy faced (within 5 m): 14 damage, 30 stagger, Tremor 1, and it is thrown up for a moment. Three rocks stand behind it for 4.5 s; nothing walks through them. | 18 damage, 36 stagger; 6 s. | **Raised Stone**: no rocks behind. The pillar that lifted the enemy stays beside it for 6 s and blocks bodies and thrown attacks. A heavy blow of yours on it shatters it into a cone of shards away from you (5 m, 70 degrees: 18 damage, 20 stagger, Tremor 1). Raise it, fight round it, break it through them. | **Dropped From the Apron**: a boulder falls from above onto the spot, its shadow growing for 0.6 s (24 damage, 45 stagger, Tremor 2) and lies there 6 s. A heavy blow on it rolls it 8 m along the blow, crushing a lane (16 damage, 30 stagger, Tremor 1 to each), until it breaks on a wall or a large enemy. | Cast while you stand your ground (Rooted, or still for 1 s): the stone rises under the enemy's own feet and throws it 3 m away from you, Tremor 2, before it stands. |
| **Fissure** (built) | Breaker | 10 s | Drive the weapon into the ground. A crack runs 8 m ahead in 0.4 s, glows for 0.9 s, then breaks open in spikes: 20 damage, 35 stagger, Tremor 1; ordinary enemies are held 1.2 s. | 26 damage; 9 s. | **Basalt**: the spikes come up as a row of six-sided columns that stand 4 s as a wall. A heavy blow on the row topples it away from you one column after another, like dominoes: 20 damage and 30 stagger from each column that falls on an enemy. | **Fault Line**: the crack does not break open by itself. It stays open for 6 s, and any heavy blow you land within 2 m of it erupts the whole line (24 damage, 40 stagger, Tremor 2). It can be erupted twice. You lay the line, then fight beside it. | Land a heavy blow on the crack's line while it glows, and it erupts at once at one and a half times. |
| **Quake Step** (built) | Breaker | 9 s | Raise a foot (0.45 s) and bring it down: a ring to 5 m, 16 damage, 30 stagger, Tremor 2. An enemy showing an attack takes double stagger and its attack is broken (a god or elite takes only the stagger). | Ring to 6 m; 8 s. | **Struck From Afar** (the Cailleach striking the ground with her staff; the walking quake is Brannoch's Seven Strides on the Massive Sword's Way): the stamp lands not round you but on the shadow of the enemy faced, up to 8 m off, as a ring of 2.5 m there. Its attack breaks wherever it stands (double stagger, Tremor 2), and it is held 0.6 s. You break the archer's draw without crossing the room. | **The Second Wave**: one stamp sends out two rings: a fast thin one of dust to 5 m (8 damage, Tremor 1), then 0.7 s later a slow heavy one of heaved slabs to 7 m (18 damage, 40 stagger). An enemy caught by both is thrown off its feet for 1 s. It rewards standing where the second ring will catch them. | An enemy broken out of its wind-up resets the cooldown. |
| **Mountain Stance** (built, re-aimed: its Grey Stone shape is now the Learn, and the built fist of rock is a shape) | Counter | 10 s (5 s if nothing struck) | **Grey Stone** (the Cailleach turning to stone each summer; the beheading game is the Massive Sword's technique, The Return Blow): you go wholly to stone, up to 2.5 s, unable to move or answer. Every blow is turned, red ones too, and each one that strikes the stone jars its attacker: Tremor 2, and its next attack 0.3 s late. Let go, and your next heavy blow within 1 s carries 20 more stagger for each blow the stone turned. | 3 s; 25 stagger a blow turned. | **Stone Skin**: the blow is turned but not answered. Two plates of stone grow on your shoulders instead. Each takes 60% of one blow and stops its flinch, then breaks in a burst of 3 m (14 damage, 24 stagger, Tremor 1). | **Fist of Rock** (the stance as built): stand as stone, up to 1.5 s, unable to move. The first blow to land, red ones too, is turned for nothing, and a fist of rock rises under the one who struck: 30 damage, 60 stagger, Tremor 2. One blow, answered at once. | Turning a red attack stuns the attacker for 1.5 s (0.6 s on a god or elite) and resets the cooldown. |
| **Under the Mound** (new) | Movement | 9 s | You sink into the ground in 0.2 s and travel as a hump of heaving turf up to 8 m along the stick, untouchable. You burst out in a ring of thrown sod and stones: 2.5 m, 12 damage, 25 stagger, Tremor 1. | 10 m; 8 s. | **Ditch and Bank** (the old earthworks): the turf you heaved stays up behind you as a bank 1 m high along your whole path for 5 s. Nothing walks through it, and enemies standing on the line as it rises are thrown off it with Tremor 1. You draw a wall as you go. | **Barrow Road**: hold to stay under for up to 1.5 s and steer. Come up under an enemy and it is thrown up as by Stone Rise, and you stand behind it. | Go under within 0.2 s of a blow landing (as a perfect dodge) and you come up at once behind the attacker; your next blow is a heavy that lands with Tremor 2 and breaks half again as much balance (passing any guard stays Metal's and Light's). |
| **Cairn** (new; Earth's larger spell. An earlier draft opened pits under the shaken, which is the Water and Earth reaction The Ground Gives) | Detonator | 40 mana | Stamp the weapon's butt. The Cailleach's stones fall from her apron onto every enemy within 10 m that carries Tremor, one stone for each depth, each landing for 12 damage and 15 stagger plus 10 more. The Tremor is spent, and the stones stay heaped on it for 4 s: 15% slower a stone. | 35 mana; 14 damage a stone. | **Unbroken Cairn**: the stones land soft and wait. Your next heavy blow on a heaped enemy breaks its cairn outward: 18 damage a stone to everything within 2.5 m. The burst waits for your blow. | **Thrown Stones**: the stones do not stay. Each enemy's heap is flung at once at the nearest other enemy within 5 m, 12 damage and Tremor 1 a stone. One shaken enemy shakes the next. | Cast after standing still for 1 s: every stone strikes twice, and half the mana comes back. |

**Charged blade.** Grit and dust crust the edge and flake off as rock chips with every blow. Each charged blow lays
Tremor, strikes 20% harder and throws a short spray of gravel (a 2 m cone, 4 damage). While a charged blow is wound
up you cannot be made to flinch and take 25% less. The charged blade is for trading blows.

**Infused strike: Broken Ground** (L2 with R1). The weapon is brought down and the ground heaves along the swing: four
slabs in a 4 m line, 14 damage, 25 stagger, Tremor 1, and the slabs stand 2 s as knee-high cover. **All in, L2 with
R2: Mountain Cleaver** (the built spell, moved here). Hold, and stone climbs the weapon in three stages (0.5, 1.2 and
2.0 s; lanes 3.5, 5 and 6.5 m drawn ahead); let go to bring it down: 30 / 55 / 90 damage, 40 / 70 / 120 stagger,
Tremor 1 / 2 / 3. While holding you can be hurt but not stopped.

**Devotion: Stone Remembers.** Nothing you raise falls by itself: stones, boulders, columns and slabs stand until
something breaks them (eight at most; the oldest crumbles). An enemy driven into one by a heavy blow, a finisher or a
quake takes 20 damage, 35 stagger and Tremor 1, and the stone holds. The longer the fight, the more of the floor is
yours. (It is the rule of the Celtic land, "stone remembers", given to the player.)

**Reaction lift: Left in Rubble.** Every reaction Tremor takes part in strikes 20% harder and leaves a knee-high heap of
rubble where it happened, which stands as one of your stones: Stone Remembers keeps it, The Circle Speaks casts from
it, and an enemy driven into it takes the Devotion's blow. Seen: a heap of broken stone and a puff of ochre dust at
each reaction. Heavy blows drive enemies into it. (It replaces The Hill's Weight, whose "cannot be pushed" stopped the
Earth pairs' own throws.)

**Passives.**
| Mark | Kind | What it does |
|---|---|---|
| Tremor | Root (built) | Heavy blows and finishers shake an enemy, up to three deep; its balance does not come back. Also Earth's base buff, from GAME.md: afflictions on you last half as long. |
| Rooted | Major (built) | Fight a few breaths without rolling and you are Rooted: less harm, cheaper guard, no push. A roll ends it. |
| Heavy Hand | Major (built) | Your heavy blows are not stopped by being struck. |
| Standing Stones | Major (built, re-aimed) | Two of your stones may stand at once, and an enemy driven into one is hurt and breaks it. |
| Immovable | Keystone (built) | The dodge becomes a brace where you stand, and a brace timed well turns any blow. |
| The Slow Hand | Keystone (built) | Every swing is slower and costs more, and every swing lands as a heavy blow. |
| The Closed Ring | Keystone (built) | Bedrock raises a ring of stones round you and the enemy faced; nothing crosses it while it stands. |
| Stone Remembers | Devotion | Above. |
| Left in Rubble | Reaction lift | Above. |
| Hand of the Mountain | Gift (built) | While Bedrock lasts a hand of stone comes down again on each heavy blow. |

**Surge: Bedrock** (40 mana, the surge slot). It fits unchanged: it is the button for trading, and nothing about it
leaned on spells costing mana. Give it Earth's base buff from GAME.md as trees_b designed: every affliction on you is
shed while it lasts. No cooldown is needed; its mana is its limit.

**New to the engine:** stones heaped on an enemy that slow it and wait for a blow (Cairn); a rolling boulder struck
along a blow; stones that never fade (Devotion); a bank of turf raised along a path (Ditch and Bank); a stamp placed
on a far enemy (Struck From Afar); a heap of rubble left where a reaction happened (Left in Rubble).

---

## Metal

**The substance.** Sparks with hard bright heads and short tails; liquid bronze that pours and sets, cooling from
white-gold through cherry red to a black crust; glowing seams in cracked plate; the ring of struck metal shown as a
sharp ripple on the floor. No flame and little smoke: the heat is in the metal. **The myth.** Hephaestus thrown down
and lame, who does not chase but makes the room come to him; the invisible golden net; Talos walking his round, heating
red-hot, his one vein closed by a bronze nail at the ankle, the ichor running out like molten lead; the self-moving
tripods. **The phenomena.** Quenching and tempering, where the steel's colour runs pale straw, bronze, purple, blue as
it cools; casting; the ring of a struck plate.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Flung Blade** (built, the tap spell) | Opener | 6 s | A blade flies into the enemy faced within 9 m (4 damage) and stands in it 6 s. Your next weapon blow on that enemy drives it in: 12 damage, 15 stagger, a notch, a burst of sparks of 1.5 m, and it flies back to your hand, cutting the lane for a third as much. | 16 when driven; up to three blades may stand in one enemy, and a heavy blow drives them all. | **Cherry Red** (rivets are the Smith's Hammer's skill): three red-hot blades in a fan, each standing in a different enemy. They glow cherry red and cool to black over 6 s; one driven while still red (the first 2 s) strikes double. You throw wide and then hurry. | **The Nail** (Talos's ankle): one long bronze nail driven low into the enemy. It does not come back. Each heavy blow on that enemy drives it deeper (12, then 16, then 24). At the third it kneels for 1.5 s (0.5 s for a god or elite) and golden ichor runs out round it: a pool of 3 m for 4 s, 3 damage a bite and a notch. Three heavies on one enemy. | A perfect guard drives every blade standing in the attacker at once, and resets the cooldown. |
| **Circle of Steel** (built) | Counter | 10 s (5 s if nothing struck) | Raise the blade for a breath (1.2 s), unable to move. The first blow from any side is turned for nothing and answered with a cut all round you (3 m, 14 damage, 20 stagger) that notches. | Two blows may be turned; 16 damage. | **The Golden Chair** (the smith's chair that bound the queen of the gods; his net is the Metal and Dark duo spell): a blow turned closes bands of gold round the attacker, a faint shimmer that turns bright as they lock. It is held 2 s (0.6 s for a god or elite), notched twice, and your blows on it pass any guard. | **Ringing Plate**: you stand as a struck plate. Every blow turned in 1.5 s (four at most) rings: a sharp ripple to 4 m, 8 damage, 20 stagger and a notch on all, each ring wider than the last. The more of them strike you, the better. | Turning a red attack resets the cooldown and gives 15 mana. |
| **Returning Edge** (built, as designed in trees_b) | Breaker | 9 s | A turning blade a metre and a half across flies 8 m along your facing and back to your hand, drawing its lane on the ground: 12 damage and 10 stagger to each enemy it passes each way, a notch on the first pass. | 16 damage, 14 stagger. | **Bat It Home**: guard inside the perfect window as it arrives and it goes out again, faster and harder (1.4 times the speed, 1.5 times the damage), three times at most. | **Through the Line**: it does not come back by itself. It stops 8 m out and spins in place for 3 s (8 damage to what touches it, every 0.3 s). Your next dodge calls it home along the line, cutting everything between: 20 damage, 30 stagger, a notch. Put them between you and the blade. | Catching it on a perfect guard resets the cooldown. |
| **Poured Bronze** (new) | Detonator | 12 s | Raise the hand. Over each notched enemy within 8 m a lip of molten bronze tips in the air (0.4 s), then pours into its cracks: 10 damage, plus 12 damage and 15 stagger for each notch. The notches are spent. The bronze sets: the enemy is held 0.4 s a notch (a quarter of that on a god or elite) under a crust cooling from cherry red to black. | 15 damage a notch. | **Set**: the crust stays 4 s. A heavy blow on a crusted enemy shatters it for 20 more and throws bronze splinters 3 m behind it, notching what they hit. Pour, then break. | **Red-Hot** (Talos heating himself): the bronze stays molten in the cracks for 4 s. No hold, but every weapon blow on that enemy spits sparks and deals 6 more for each notch that was spent. Pour, then press. | Cast within 1 s after a perfect guard: the attacker's pour is doubled, and the cooldown resets. |
| **To the Blade** (new) | Movement | 8 s | You fly along a line of sparks to a blade of yours within 12 m (a Flung Blade standing in an enemy or the ground, a blade of Grave of Swords), untouchable for the 0.25 s of the flight, cutting what lies between (10 damage, 15 stagger, a notch), and arrive driving that blade. With no blade out, you throw one 6 m ahead into the ground and go to it. | 14 m; 7 s. | **Chain**: a bronze chain stays stretched along the line you flew for 3 s. An enemy that crosses it is notched and tripped (20 stagger). Your path becomes a tripwire. | **There and Back**: within 2 s of arriving, press again to fly back along the same line, cutting it a second time. In and out. | Cast within the perfect dodge window: you are untouchable from the press, and the cooldown resets. |
| **Grave of Swords** (built) | Breaker (mana) | 50 mana | Twelve blades fall in a ring 4.5 m round you (8 damage under each) and stand 12 s. Each light blow you land calls the nearest one through that enemy (9 damage, 6 stagger); a heavy blow calls all that remain; every three add a notch. | Sixteen blades. | **Clashing Bronze** (the Kouretes clashing bronze round the infant Zeus): the twelve blades do not fly; they ring. Each light blow you land makes the nearest standing blade ring: a sharp ripple 3 m from it, 8 damage, 15 stagger and a notch to all it reaches. A heavy blow rings them all at once, and they fall. Seen: upright blades round you throwing out hard ripples on the floor. Every blow rings. | **The Tripods** (the smith's automata): three bronze tripods roll out, four blades on each, and go to notched enemies first, cutting twice a second (9 damage, a notch every third cut). A heavy blow you land calls each one back to you, and it throws its remaining blades through whatever it passes on the way. | A finisher looses every blade left and gives back 20 mana. |

**Charged blade.** A cherry-red seam opens along the edge and throws sparks with hard heads at every swing. As each of
the three charged blows is spent the seam cools through the temper colours, straw, purple, blue, so the count can be
read on the blade itself. Charged blows lay a notch, strike 20% harder and pass any guard: a shield or a raised guard
takes them in full.

**Infused strike: Struck Plate** (L2 with R1). The blow lands like a hammer on an anvil. A sharp ripple runs 3 m from
the one struck: 10 damage and a notch to all it reaches, and those already at three notches take 25 stagger. **All
in, L2 with R2: Red Iron.** Hold for up to 1.5 s while the blade heats from cherry to white with a rising ring; let go
for a blow that cannot be guarded, lays two notches and deals one and a half times at full heat. The blade then stays
hot for 5 s (weapon damage up 15%), as the smith's own hammer does when he has forged with it.

**Devotion: The Guard Holds.** Your guard never breaks. A blow your stamina cannot pay for is held anyway, and what it
cannot pay comes through as grey health. Every blow held on your guard notches the attacker and throws sparks into it
(4 damage). You take 15% less from everything. Red attacks still need a dodge or a perfect guard. (GAME.md's base buff
for Metal is defence; this is it at full strength.)

**Reaction lift: Every Crack Rings.** Every reaction Sundered takes part in rings out: it notches everything it reaches and
strikes 25% harder on the notched.

**Passives.**
| Mark | Kind | What it does |
|---|---|---|
| Sunder | Root (built) | Heavy blows and perfect guards notch an enemy's armour, up to three; a finisher notches it three deep. |
| Forged Answer | Major (built) | A perfect guard forges a blade at your shoulder, three at most; your next heavy blow sends them all. |
| Counter Cut | Major (built) | A blow that lands while an enemy is showing its own attack is critical. |
| Open Line | Major (built) | A perfect guard throws the attacker's weapon wide; for a moment it cannot guard or turn. |
| Edge Guard | Keystone (built) | Only the perfect guard turns a blow, but its moment is longer and each one cuts. |
| Master's Answer | Keystone (built) | After a perfect guard the weapon's skill is free and instant; otherwise it costs more. |
| All Blades Home | Keystone (built) | Your blades never fade; lighting Temper calls them all home through whatever stands between. |
| The Guard Holds | Devotion | Above. |
| Every Crack Rings | Reaction lift | Above. |
| Hanging Swords | Gift (built) | While Temper lasts every blow held on the guard is perfect, and a sword hangs over the attacker. |

**Surge: Temper** (40 mana, the surge slot). It fits unchanged: 30% less damage taken is Metal's raised defence, and
Hanging Swords rides on it. Cooldown spells change nothing here; if anything, Temper is now the first thing a Metal
player spends mana on, so 40 is right.

**New to the engine:** a pour that holds an enemy for a time set by its notches; a planted blade the player can fly to;
bands that hold and open the guard (The Golden Chair).

---

## Light

**The substance.** Light here is never a plain glow. It is gold made solid (beaten-gold spears, gold leaf, feathers of
gold), and rays made visible by the dust they cross. White is kept for the hottest point. The bright nets that
sunlight throws through water belong to the Water and Light pair (Caught Light), not to Light alone. Mirrors belong to Halo and stay there. **The myth.** Surya's chariot of
seven horses and its charioteer Aruna, the red of dawn, who rides in front to shield the world from the heat; Garuda's
gold; Shibi's scales; the lamp circled before a god; Clarity as purification. **The phenomena.** Shafts of sun made
visible by dust; a lens drawing light to a point.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Glare** (built, the tap spell) | Opener | 6 s | A narrow ray 5.5 m ahead, dust lit along it: 8 damage, 22 stagger, Marked. It strikes only what you face; what it marks wears brackets of gold. | 12 damage; 7 m. | **Gathered**: hold for up to 1 s and the ray narrows, the dust in it drawn to a point on the enemy; let go and the stagger rises from 22 to 60 with the hold. A lens, for breaking one enemy. | **Seven Horses** (Surya's seven): the ray splits into seven thin rays in a 70 degree fan, 6 m: 5 damage each, and every enemy touched is Marked. Marking a crowd. | Cast within the perfect dodge window: the attacker is blinded and cannot turn for 2 s (1 s for a god or elite). |
| **Step Into Light** (built) | Movement | 8 s | Step through the enemy faced, up to 7 m, on a line of gold dust, cutting and Marking it. If the cut lands, your next heavy blow within 1.5 s is critical. | 9 m; 7 s. | **Aruna's Road** (the charioteer of dawn): the line you stepped stays 3 s as a road of gold dust. It Marks whatever crosses it, and pressing again within the 3 s steps you back along it to where you began. Out and back. | **Behind the Eye**: you step to behind the enemy rather than through, landing at its back. Your first blow within 1 s deals a finisher's damage (three times) without ending any stagger. | Cast through two enemies standing in one line: both are cut and Marked, and the cooldown resets. Light is straight. |
| **Ring of Spears** (built) | Breaker | 10 s | Six spears of beaten gold stand in the air in a ring 3 m round the enemy faced, wait 0.6 s, and drive in together: 6 damage each, 30 stagger in all, Marked, and it is held still 1.5 s (0.5 s on a god or elite). | 40 stagger in all. | **The Drawn Line** (the line drawn round Sita's door that no one meaning harm could cross): the six spears drive into the floor in a straight line 6 m across your front and stand 5 s. An enemy that crosses it is held 1 s and Marked, and your blows across the line are critical. Seen: a row of gold spears in the floor, dust lit along it. Fight across it. | **In Turn**: the spears drive in one after another, a quarter second apart. A spear that lands within 0.1 s of one of your weapon blows strikes double and is critical. Six beats to meet. | Land a critical blow on the enemy while the spears hang: they all drive in at once, critical, and the cooldown resets. |
| **Standing Light** (built) | Counter | 12 s | Plant a lamp of brass. A shaft of light stands over it, dust turning in it, and a glow of 5 m lies round it for 10 s. Enemies in the glow are Marked every 2.5 s, and a perfect guard made inside it bursts round you (3.5 m, 10 damage, 25 stagger). No enemy cloud, flame or frost can lie inside the glow: it burns them away (Light purifies). | Inside the glow your perfect guard and dodge windows are half again as long. | **Row of Lamps** (the rows of lamps at Diwali): three small lamps are set in a line 6 m long. Every enemy between two lamps is Marked each second and can carry no cloud, flame or frost; a perfect guard made in line with them throws a ray down the row (12 damage, 25 stagger). Seen: three brass lamps with dust-lit shafts and a thin gold line between them. The guard fires the row. | **Circled Lamp** (the lamp waved before the god): a smaller lamp circles you at 2 m, once a second, for 8 s. It Marks what it passes and burns away any hazard you walk into. It goes where you go. | The first perfect guard inside the glow resets every Light cooldown, once a lamp. |
| **Reckoning** (built) | Detonator | 12 s | Set a seal on the enemy faced: a ring like a clock face on the ground under it, one hand sweeping it in 6 s. On the last tick it breaks upward in a column: half of all your weapon did to that enemy in the 6 s is done again at once (150 at most), with 40 stagger. | 60% of the count, 180 at most. | **Witnessed** (Surya, the eye that sees every deed): the seal counts the bearer's deeds, not yours. For 6 s every blow it lands on you or your guard is scratched in gold on the clock's rim. On the last tick a column falls on it: 30 a blow written and 15 more for each you turned with a perfect guard (150 at most), 40 stagger. Seen: the clock face under it gaining a gold mark at each blow it lands. Guard well and the sun judges for you. | **Spoken Sentence**: no clock. The seal waits up to 8 s for your next heavy blow on its bearer, then a blade of beaten gold falls on every Marked enemy within 8 m: 20 damage plus 12 for each depth of mark, 40 stagger, and the marks are spent. Many enemies, at the moment you choose. | A finisher on the sealed enemy pays the count twice. |
| **Daybreak** (built) | Detonator (mana) | 45 mana | Begin, and every blow you land adds a ring of light up the blade, eight at most. Hold again to loose it all as a beam 12 m long and 1.6 m wide: 8 damage and 6 stagger a ring, and it Marks. | 10 damage a ring. | **Red Before the Sun** (Aruna rides before Surya and shields the world from his heat): the rings are not loosed as a beam; they stand before you as a curved shield of red dawn 3 m wide for 4 s that stops thrown attacks and blows from the front. Each blow it stops adds a ring; press again to loose what is left as the beam. Seen: a red-gold arc of lit dust in front of the player. Light's one shield, and still a beam at the end. | **High Noon**: the light comes straight down on you as a column 5 m across. It burns away every enemy cloud, flame, frost and hazard inside, clears every affliction on you, and strikes everything inside for 6 damage a ring, Marking it. The purifying version. | Loose it within 0.2 s of a blow landing on you: the blow is turned and the beam is critical. |

**Charged blade.** Gold dust runs along the edge like gilt, and each cut leaves a thin gold line hanging in the air
for half a second. Each charged blow Marks and strikes 20% harder; the third throws a ray 5 m along the swing that
Marks everything on it.

**Infused strike: Ray Along the Edge** (L2 with R1). A tenth of a second after the swing a ray follows its path 6 m
out: 10 damage, 15 stagger, Marks. **All in, L2 with R2: The Sun on the Blade.** Hold for up to 1.2 s while a shaft of
light comes down through the dust onto the raised weapon; let go for a cut that is critical and passes any guard, and
the same cut opens at half strength on every Marked enemy within 8 m.

**Devotion: Full Daylight.** A mark is no longer spent by blows. Every weapon blow on a Marked enemy is critical until
the mark fades (6 s), but a mark cannot be laid on that enemy again for 4 s after it fades. The fight becomes a rhythm:
mark, spend the window in full, wait out the light.

**Reaction lift: Seen Plainly.** Every reaction Marked takes part in is critical and Marks everything it reaches.

**Passives.**
| Mark | Kind | What it does |
|---|---|---|
| Marked | Root (built) | The last blow of a chain and every perfect guard Mark; the next three blows on the Marked are critical. |
| Weighed | Major (built) | Marks deepen to three, and each depth makes your critical blows strike harder. |
| Just Cut | Major (built) | The next blow of a chain pressed at the instant the last one lands flashes: critical and free of stamina. |
| Brimming | Major (built) | While your mana is full, every heavy blow throws a wave of light. Mana kept for the big spells now has a use while it waits. |
| Unblemished | Keystone (built) | Fight a while without a wound and every blow is critical; a wound ends it. |
| Two Suns | Keystone (built) | A figure of light casts each of your spells again from the far side of the enemy. |
| Condemned | Keystone (built) | An enemy marked three deep and near death may be finished whatever its balance. |
| Full Daylight | Devotion | Above. |
| Seen Plainly | Reaction lift | Above. |
| The Scales | Gift (built) | While Clarity lasts the wounds given and taken are weighed, and a sword falls on the Marked if you gave more. |

**Surge: Clarity** (40 mana, the surge slot). It fits unchanged and carries Light's purification (afflictions burned
away, grey health made whole). It does gain from the new design: its longer perfect windows now also mean more
cooldown taken off by perfect guards and dodges (2 s and 1.5 s each). If that runs hot in play, lower it to 35 mana
before touching its windows.

**New to the engine:** a seal that waits for a heavy blow (Spoken Sentence); a glow that removes enemy hazards; a mark
that is not spent but cannot be renewed for a time (Devotion); a line on the floor that holds what crosses it (The
Drawn Line); a shield standing before the player (Red Before the Sun); a seal that counts the bearer's blows (Witnessed).

---

## Dark

**The substance.** Violet-black smoke that sinks rather than rises; ink pooling on the floor; grey ash, as on a
cremation ground; a rope of black; the player's own shadow come loose. **The myth.** Yama's noose, which drew out
Satyavan's soul, a thing the size of a thumb; Rahu, a head with no body that swallows the sun, which rolls out of his
throat again; the vetala, a spirit that rides the corpses of a cremation ground and hangs from its tree; the ash on
Shiva's brow. **The constraint.** Dark's base self buff, from GAME.md, is one revive as a zombie. As built that revive
exists only as the god's gift. It belongs on the root (below), so every Dark player has it.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Grasp** (built, the tap spell) | Opener | 7 s | A black ball crawls along the ground to the enemy faced within 6 m, drawing in what it passes. Where it arrives: 10 damage, 12 stagger, Cursed, and up to five bodies are dragged to your feet. What cannot be dragged drags you to it. | 8 m; 14 damage. | **Tethered** (Yama's noose itself is the all-in strike below): a rope of black is thrown instead and binds the enemy faced to you for 4 s. It cannot go more than 5 m from you, and your first heavy blow on it yanks it to your feet (20 stagger). You keep it close. | **The Pit**: the ball stops at the aimed point and spreads into a pool of ink 3 m across for 4 s, dragging everything in it slowly to the middle and Cursing it. You gather them at a spot. | Cast within 1 s of being struck: the one that struck you is dragged to your feet and Cursed twice. Dark answers when it is hurt. |
| **Debts Called** (built) | Breaker | 10 s | Everything within 6 m is dragged in to a point before you over 0.5 s, then thrown back by a burst of 3.5 m: 18 damage, rising to 36 at one point of health; 35 stagger; Cursed. | 8 m. | **The Black Buffalo** (Yama rides a buffalo): no pull. A buffalo of sinking black smoke charges 8 m through the point before you, carrying everything Cursed on its horns to the far end and throwing it down: 18 damage, rising to 36 at one point of your health, 35 stagger. Seen: a horned shape of smoke running a lane, bodies tossed at its end. Follow the lane and cut what it threw: Dark's one push. | **Collected**: only the Cursed are called, from up to 10 m, and each one that reaches you gives back 4 health. There is no burst; the pull does 20 stagger. | Cast below a third of your health: the burst, or the buffalo, strikes twice as hard. |
| **Devouring Maw** (built) | Detonator | 11 s | A jaw twice your height rises out of your shadow and lunges 5 m along a lane: 24 damage, 30 stagger. On a Cursed enemy it spends the Curse: 20 damage more, and you heal 12. | Heal 18. | **Rahu** (the head with no body): an ordinary Cursed enemy is swallowed whole, and 1.5 s later rolls out of the open throat at your feet, staggered, because there is nothing to hold it. A god or elite is only bitten. | **From Its Own Shadow**: the jaw rises out of the enemy's shadow instead, up to 9 m away, and closes from below. No lane: it reaches the one far off. | Bite an enemy in its wind-up: its attack is swallowed and broken, and the cooldown resets. |
| **Gone to Ash** (new) | Counter | 10 s (5 s if nothing struck) | Stand with arms crossed as grey ash runs over you, up to 1.2 s. The first blow to land scatters you as a cloud of ash, and you take nothing. 0.4 s later you stand again behind the attacker. It is Cursed, and your first blow within 1 s is a heavy blow that heals you 10. | 1.5 s; heal 15. | **Ash on the Brow** (the ash on Shiva's brow): you stand again where you were. The ash settles on the attacker's brow as a grey smear: for 6 s it is Cursed, the next blow it lands on you scatters you for nothing once more, and your first blow on it is a heavy. Seen: a grey stripe across its head. The ash marks the one who struck. | **Ash Hangs**: you stand again where you were. The ash stays as a sinking cloud of 3 m for 3 s; enemies inside it are Cursed, and an attack they begin inside goes where they already faced. | Scattering a red attack staggers the attacker and resets the cooldown. |
| **Second Shadow** (built) | Movement | 12 s | Leave your shadow where you stand: your shape in flat black with violet eyes, joined to you by a thin line on the ground. For 8 s it repeats every blow you land a tenth of a second late on the nearest enemy within its reach, at 50%, Cursing. Hold again to change places with it, once. | 10 s; 60%. | **Return to the Shadow**: the shadow fights as learned, but the second press does not swap. Within its 8 s, press again to sink into it and rise where it lies, and every wound taken since you left it comes back as grey health to win back. Seen: your flat black shape on the floor and a thin line to it, then you sinking into it. A rewind paid in position. | **Long Shadow**: your shadow is thrown out along the floor 8 m ahead, long as an evening shadow. You sink into its foot and rise at its head 0.3 s later, and everything it lay across is Cursed. No copy: just the road. | Change places within 1 s of being struck: the blow passes to the shadow, and the health it took comes back to you. |
| **Pawned Heart** (built) | Breaker (a price) | No mana: health; 40 s | Your health drops to 1 and the rest is set down behind you as a beating heart in a ring that closes over 10 s. While it beats your weapon deals 60% more and swings cost the least stamina they can. Walk at least 2.6 m away, then back onto it, for all that was pawned plus 30% of the damage dealt meanwhile. If the ring closes first, health stays at 1 and half of what was pawned comes back grey. | 70% more; the ring closes over 12 s. | **Buried Heart**: the heart sinks into the floor where you stand and beats under it. For 10 s your weapon deals 60% more only while you stand within 3 m of it; when the ring closes it rises whole, with 30% of the damage dealt. Seen: a dark pulse under the floor in a ring of ash. Stand your ground and cut: out (as learned), in (Hung From the Tree), or here. | **Hung From the Tree** (the vetala's tree): the heart hangs in the air over the enemy faced instead, and follows it. You must fight your way in under it to touch it. You go in, not out. | Touch the heart during a perfect dodge: it returns 60% of the damage dealt, not 30%. |

**Charged blade: Shadow Cut Loose.** Violet-black smoke pours off the edge and sinks to the floor behind each swing.
Each charged blow Curses and strikes 20% harder; the third cuts the enemy's shadow from its feet. For 4 s the shadow
lies 2 m away on the floor; the enemy takes 25% more, and a blow on the loose shadow strikes the body as well. Seen:
an enemy standing with no shadow beside a shadow with no body, the clearest thing a top-down camera can show. Two
targets for one enemy: Dark takes something you can see.

**Infused strike: Taken** (L2 with R1). The cut leaves a black line across the enemy; a moment after, the line drags
it 2 m toward you: 12 damage, 20 stagger, Cursed. **All in, L2 with R2: Yama's Noose.** A black rope is thrown 8 m to
the enemy faced and drawn tight; what is drawn out is the size of a thumb, a small pale figure that hangs between you
for 2 s. Strike it and the enemy takes 40 damage and 50 stagger through any guard; if the enemy dies while it hangs,
you heal 25.

**Devotion: Nothing Stays Buried.** Every enemy that dies Cursed stands up again as your shade, ash-grey with violet
eyes, and fights for you for 6 s (a blow every 0.8 s: 12 damage, 12 stagger, Curse), three at most. While you are
Risen, every enemy that dies stands up, Cursed or not. (The vetala riding the dead. The Dead Stand folds into this.)

**Reaction lift: Collected Twice.** Every reaction the Curse takes part in leaves a black thumbprint on the brow of
what it struck for 4 s; the next weapon blow on it repeats the reaction's damage. Seen: a black print on each struck
head. The debt is collected by the blade. (It replaces Every Debt Collected, a third lifesteal.)

**Passives.**
| Mark | Kind | What it does |
|---|---|---|
| Curse and the Rise | Root (built, moved) | Blows Curse; a Cursed enemy deals less and killing it heals. And once, the blow that would kill you does not: you rise Risen, health draining, blows feeding you, an empty bar death. Resting ends it and gives it back. |
| Blood Price | Major (built, re-aimed) | A spell still on its cooldown can be cast anyway, paid in health: 1 for each second left, as grey health owed, never the last point. |
| Near the Grave | Major (built) | Below a third of your health you strike harder, and each kill wraps you in a shade that takes the next blow. |
| Hollow Strength | Major (built) | Risen, you strike harder and do not flinch; each kill stills the drain for a few breaths. |
| Dread | Major (built) | When a Cursed enemy dies, the other Cursed lose heart: their balance buckles and their guard falls. |
| Dead Already | Keystone (built) | You are Risen from now on, in every fight; there is no second rising. |
| Never Sated | Keystone (built) | You carry no flasks; every blow feeds you a little and every kill a great deal. |
| Nothing Stays Buried | Devotion | Above. |
| Collected Twice | Reaction lift | Above. |
| A Life for a Life | Gift (built, re-aimed) | Risen, land a finisher on the one that killed you: you live again at half health, and the Rise is yours once more. |

**Surge: Hunger** (no mana, health; the surge slot). It fits the slot as it is: the slot holds a surge, and Dark's
price is health. What changes is that mana used to keep it rare. It now needs a limit of its own: a cooldown of 25 s
from the moment it ends, so it is lit two or three times in a long fight, not held on. It also gives the Dark player
a different use for mana: with the surge and Pawned Heart both paid in health, a Dark Hand's mana goes to the
weapon's skill and the all-in infused strike. That is the right flavour: Dark buys its magic with its body and keeps
its mana for steel.

**New to the engine:** a cast paid in health while on cooldown (Blood Price); a swallowed enemy that rolls out again;
the Rise on the root; a cooldown on a surge; a shadow cut loose from its body that can be struck (Shadow Cut Loose);
wounds remembered and returned as grey health (Return to the Shadow).

---

## Poison

**The substance.** Mist that rolls and pools with a hard top edge; bubbling puddles that leave stains; venom spat and
dripped in strings; clouds that burst and hang. Blue-green most of the time, bile-yellow at its worst, never white or
highlighter green. Mercury is the second substance: silver beads that run together and roll downhill. **The myth.**
The gu jar, where the last creature alive becomes the poison; the five poisons pinned on red paper with five needles;
Shennong, who tasted every herb to tell medicine from poison; the cinnabar refined nine times; the snake that sheds its
skin, the gecko its tail. **The constraint.** Poison heals over time from the damage it deals (GAME.md). As built
only Fever heals, and weakly. The root should carry it, so every Poison player has it.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Miasma** (built, the tap spell) | Opener | 7 s | Eight clouds over a disc 4 m ahead hang for 4 s, creep after the nearest enemy at 0.8 m a second, and give what stands in them a dose a second. A stain is left for 3 s. You stand in it unharmed. | 6 s; 3 m across. | **Low Mist** (the land's hours of the miasma, mist that pools low and drives people up): the cloud is heavier than air. It pours out from your feet along the floor the way you face, a low sheet with a hard top edge 2.5 m wide and 8 m long, over 1 s, and lies 5 s, stopping at walls. Enemies in it are dosed once a second, and a dodge or roll in it costs them a dose. Seen: a flat blue-green sheet with a sharp edge creeping out like spilled water. A lane where the Learn is a disc. | **Censer** (Swinging Censer folded in): the cloud is carried. A censer swings round you on its chain at 3 m, once every 1.2 s, for 8 s. Each pass deals 4 damage and a dose and leaves smoke hanging for 2 s. The fight comes with you. | Cast on the poison's beat (within 0.2 s of a bite on any enemy within 8 m): the cloud doses twice a second. |
| **Five Needles** (new) | Breaker | 9 s | Five long needles, venom beading on them, thrown in a narrow fan 8 m at the enemy faced: 3 damage, 10 stagger and a dose each. An enemy struck by all five is pinned at its points: held 0.8 s (0.3 s for a god or elite) and 40 stagger more. | 14 stagger each; 50 more for all five. | **Spread**: a wide fan of 60 degrees, so each needle finds a different enemy. Each enemy pierced takes two doses and its poison bites at once. | **Red Paper** (the five poisons pinned to red paper; the needle grids in the floor are the Poison god's own weapon): all five needles pin one sheet of red paper to the enemy faced. For 6 s it cannot guard or turn a blow, and your blows on it pass any shield. The breaker for what hides behind a guard. | Thrown on a poisoned enemy's beat (within 0.2 s of its bite): every needle that strikes it makes its poison bite once more. |
| **Coiled Strike** (built) | Counter | 10 s | Coil for as long as you dare, up to 2.4 s, three fangs lighting over your head. Let go to strike through the enemy faced and stand behind it: 14 / 24 / 38 damage and 2 / 4 / 8 doses by the fangs lit. Let go as a blow is about to land and it passes through you, and the strike counts as three fangs. | 18 / 30 / 46. | **Centipede's Run** (the centipede, one of the five poisons): let go and you strike through up to three enemies in a zigzag of short rushes, 4 m each, a bite on each, the fangs' doses shared between them; you end behind the last. Seen: a jointed line of strikes over the floor like a centipede's track. The snake strikes one, the centipede many. | **Shed Skin**: let go inside the perfect window and you leave your skin standing where you were, pale and papery, for 1.5 s. The enemy goes on striking it while your strike lands from behind. | Letting go inside the perfect window resets the cooldown. |
| **Blister** (built) | Detonator | 10 s | Raise a blister on the enemy faced. It swells each time its poison bites, through three sizes. A heavy blow breaks it: 20 / 40 / 70 damage in 2 / 3 / 4 m, and 2 / 4 / 6 doses on everyone else it reaches. Left too long it withers. | 25 / 50 / 85. | **Weeping Sore**: it never bursts by itself or by a heavy. Each time its poison bites it weeps a bubbling puddle 1.5 m across where the enemy stands (3 s, a dose a second), so a walking enemy lays your poison; your heavy blow bursts every puddle at once (12 damage and 2 doses to all in each). Seen: a trail of stained, bubbling puddles behind it. Slow and insidious, not a swell and a pop. | **The Vessel** (the court's elixir-drinkers): the whole enemy swells. When its poison bites at full depth it bursts by itself into a cloud of 4 m that doses everything in it three deep and hangs 3 s. You set it and leave it. | Break it with a heavy blow as its poison bites (within 0.2 s): the burst doubles. |
| **Share Alike** (built) | Movement | 9 s | Rush 7 m through everything before you, untouchable: 10 damage and 10 stagger to each enemy passed, and then every one of them is raised to the depth of the sickest (two at least). | 9 m. | **Carried on the Blade**: rush through as learned, but take the sickest one's depth onto your blade: your next five weapon blows each set what they strike to that depth. Seen: venom dripping heavy from the edge, five beads to count down. The rush loads the weapon. | **Shed Tail** (the gecko): rush 6 m backward, away. A thrashing tail is left where you stood for 2 s, lashing everything within 2 m every 0.5 s for 3 doses. Away from them, and what follows you pays. | Rush on the poison's beat (within 0.2 s of a bite on any enemy you pass): the doses double. |
| **One by One** (built) | Detonator (mana) | 50 mana | Raise a hand. A green line runs along the ground from poisoned enemy to poisoned enemy, nearest first (0.5 s), then you snap your fingers and each one's poison bursts in turn (6 damage and 4 stagger a dose), each passing its doses to the next before it goes. The last burst is the largest. | 7 damage a dose. | **Eaten in Turn** (the gu, where the order of killing matters): the line is drawn but nothing bursts. For 8 s, when an enemy on it dies, all its doses pass to the next on the line, doubled; one pushed past eight bursts at once (6 a dose) and passes on. Seen: the green line glowing brighter at each link fed. The blade chooses the order. | **All Into One** (Quicksilver is the Metal and Poison reaction): no line. The poison runs out of every poisoned enemy within 10 m as beads of mercury that roll across the floor and join in the enemy faced, its doses added to its own with no limit. Then it alone bursts. For the one that matters. | Snap within 0.2 s of the poison's beat: every burst is one and a half times. |

**Charged blade.** Venom runs down the edge in strings and beads at the tip, dripping as the weapon moves. Each charged
blow doses twice and strikes 20% harder; the third makes the enemy's poison bite at once, out of its beat.

**Infused strike: Venom Flick** (L2 with R1). The swing flicks a string of venom 5 m ahead. Where it lands a bubbling
puddle of 2 m lies for 3 s, dosing what stands in it once a second. **All in, L2 with R2: Nine Times Refined** (the
cinnabar refined nine times). Hold for up to 1.8 s while nine rings pulse down the blade, cinnabar red turning to
mercury silver; let go for a cut that lays one dose for each ring, and at nine the poison in that enemy turns to
quicksilver: its bites deal double until it fades.

**Devotion: Tasted Every Herb.** No poison, cloud or mist harms you, yours or the enemy's (the court's miasma among
them). Standing in any cloud heals you 2 a second, and your blows dose twice while you stand in one. Where everyone
else leaves the mist, you fight in it. (Shennong, who tasted every poison; realgar wine, poison against poison.)

**Reaction lift: The Delayed Touch.** Every reaction poison takes part in is held in the body: a thin ring of mist
closes on it, and the reaction lands 2.5 s later at double strength, dosing all it reaches two deep; a heavy blow on
that enemy brings it on at once. (Burning Sap, which already waits, takes only the double strength.) Seen: a closing
green ring round the body. Poison is paid later, and the heavy chooses the moment. (It replaces Bitter Mix, a heal;
the legend of the delayed death touch.)

**Passives.**
| Mark | Kind | What it does |
|---|---|---|
| Venom | Root (built, widened) | Blows poison, up to eight deep, biting on a slow beat. And a tenth of all the damage your poison deals comes back to you as health over time (GAME.md's base buff). |
| In Time | Major (built) | A blow that lands as the poison bites is critical and doses twice. |
| Catching | Major (built) | Each bite, one dose leaps to the nearest enemy. |
| Rupture | Major (built) | A heavy blow on an enemy poisoned to full depth bursts it all at once. |
| Slow Death | Keystone (built) | Your weapon wounds far less; your poison has no limit and does not fade. |
| Volatile | Keystone (built) | Your poison no longer bites on its beat; it builds to full and bursts by itself. |
| Plague Bearer | Keystone (built) | Enemies near you are poisoned without a blow; your flasks heal less. |
| Tasted Every Herb | Devotion | Above. |
| The Delayed Touch | Reaction lift | Above. |
| Clean Blade | Gift (built) | While Fever lasts the poison waits; when it ends every owed bite lands at once. |

**Surge: Fever** (40 mana, the surge slot). It fits unchanged in cost. With the root now healing a tenth of poison
damage, Fever's job is clear: the beat comes twice as fast and the healing rises to 40% (as trees_b designed; the
built "1 health a second and 6% of blows" should give way to it). It is still pressed when the work is already done,
which suits a mana surge.

**New to the engine:** a heal over time from poison damage (on the root); a timed state in which an enemy cannot
guard (Red Paper); doses gathered from many enemies into one (All Into One); clouds that heal the player (Devotion);
a reaction held in the body and released later (The Delayed Touch); a cloud that pours along the floor and stops at
walls (Low Mist).

---

## What to cut from the built content

- **Every plain minor** (14 a tree) goes; Empower marks carry the numbers. The four that only lowered a tap spell's
  mana (Near to Hand, Ready Steel, Ready Light, Ready Poison) are dead outright.
- **Full Body Armament** leaves all five trees: it is now the Fists' third aspect on their Way (`weapons.md`
  section 9). Halo's Answering Draw (Katana only) moves to the Katana's Way as a Light infusion mark.
- **Earth: Rampart.** It repeats Stone Rise's raised stone and Overgrowth's Thorn Wall. Toppling, Stone Skin and
  Aftershock fold into Stone Rise and Mountain Stance; Mountain Cleaver moves to the all-in infused strike; Landslide
  goes.
- **Metal: Hone.** The charged blade now does what it did (blows that pass guards and notch). Buried Blades folds into
  Flung Blade's Empower; Ricochet, Splinter and Shatter Point go.
- **Light: Returning Light** (gives back spell mana, which spells no longer cost). Sentence and What It Owes fold into
  Reckoning's Spoken Sentence; Dazzle into Glare's Master.
- **Dark: The Dead Stand** folds into the Devotion. Unpaid, Feast, Glut and Borrowed Strength go. Blood Price is
  re-aimed at cooldowns, and the Rise moves from the gift to the root.
- **Poison: Swinging Censer** folds into Miasma's Censer shape. Left to Ripen, One Sickness, Rotten Fruit (it repeats
  Blight's Carrion Flies) and Lance the Wound go.
- **Returning Edge as built** (a head of steel lunging down a line, the dragon form) should be rebuilt to its trees_b
  design, the turning blade out and back. The built version is a reskin of Devouring Maw.

---

## Sources

- Project: `docs/GAME.md`, `docs/GUIDE.md` sections 4 to 7, `docs/design/WORLD.md`, `docs/design/LOOKS.md`,
  `docs/design/trees_b.md`, `docs/design/FORMS.md`, `docs/UPGRADE_SCHEMA.md`, `docs/design/STEEL_AND_ELEMENT.md`.
- Yama's noose and Satyavan's soul: [Mackenzie, *Indian Myth and Legend*, ch. III](https://sacred-texts.com/hin/iml/iml08.htm); [Savitri and Satyavan](https://en.wikipedia.org/wiki/Savitri_and_Satyavan).
- Rahu, the head that swallows the sun: [Rahu](https://en.wikipedia.org/wiki/Rahu); [Svarbhanu](https://en.wikipedia.org/wiki/Svarbh%C4%81nu).
- The vetala of the cremation ground: [Vetala](https://en.wikipedia.org/wiki/Vetala); [Vetala Panchavimshati](https://en.wikipedia.org/wiki/Vetala_Panchavimshati).
- Surya's seven horses, Aruna and Garuda: [Surya](https://en.wikipedia.org/wiki/Surya); [Aruna](https://en.wikipedia.org/wiki/Aruna_(Hinduism)).
- Hephaestus's net and Talos's vein: [The affair of Ares and Aphrodite](https://www.greekmyths-greekmythology.com/secret-affair-ares-aphrodite/); [Ichor](https://en.wikipedia.org/wiki/Ichor).
- Quenching and temper colours: [Tempering colours of steel](https://metallurgyzone.com/tempering-colors-of-steel/); [Metallurgy of quenching](https://www.orchardblacksmith.com/blog/understanding-the-metallurgy-of-quenching-steel).
- The Cailleach's stones, hammer and staff: [Historic Environment Scotland](https://blog.historicenvironment.scot/2025/12/cailleach-scotland-ice-queen/); [Mythbeasts](https://mythbeasts.com/beast/cailleach_bheur/).
- Columnar basalt: [British Geological Survey, the Giant's Causeway](https://www.bgs.ac.uk/discovering-geology/maps-and-resources/office-geology/the-giants-causeway-and-causeway-coast/); [Columnar jointing](https://en.wikipedia.org/wiki/Columnar_jointing).
- Shennong and the broken-heart grass: [Fabled China](https://fabledchina.com/shennong-tasting-the-hundred-herbs/); [Jade Almanac](http://jadealmanac.com/chinese-mythology/shennong).
- Cinnabar refined nine times: [Pregadio, Waidan](https://www.goldenelixir.com/jindan/jindan_intro.html); [Chinese alchemy](https://en.wikipedia.org/wiki/Chinese_alchemy).
- Gu, the five poisons and realgar wine: [Gu (poison)](https://en.wikipedia.org/wiki/Gu_(poison)); [Five Poisons](https://en.wikipedia.org/wiki/Five_Poisons).
- Shafts of light and caustics: [Tyndall effect](https://en.wikipedia.org/wiki/Tyndall_effect); [Crepuscular rays](https://usefulbs.com/blog/why-do-sunbeams-radiate-from-clouds-like-spokes-on-a-wheel).
- Bending as a martial art (earth as rooted stance): [Martial Journal](https://www.martialjournal.com/the-martial-arts-in-avatar-the-last-airbender/).
- The pieces added by the feel review of 2026-11-18 (Clashing Bronze, The Drawn Line, Witnessed, The Black Buffalo,
  The Delayed Touch and the rest) carry their sources in `review_elements.md`: the Kouretes, the Lakshmana rekha,
  Surya and Aruna, Yama's buffalo, the Touch of Death legend.
- Games, for principle only: Final Fantasy XVI's cooldowns and timing resets; Diablo IV's skill, enhancement and
  choice of two; Genshin Impact's shields from stone; Hades's aspects.
