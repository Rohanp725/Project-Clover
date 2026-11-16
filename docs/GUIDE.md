# Project Clover: what does what

Every button, weapon, element, spell form, god's gift and chart mark in the game as it is built now, and how to trigger each.
Numbers are first values from the code and have not been tuned by play. Where the code and a design document disagree, this follows the code.
"Not yet" notes say what is first-pass or missing. Nothing in the game has sound yet.

## Contents

1. [Controls](#1-controls)
2. [How a fight works](#2-how-a-fight-works)
3. [Weapons](#3-weapons)
4. [Elements](#4-elements)
5. [Gods' gifts and mix crowns](#5-gods-gifts-and-mix-crowns)
6. [Spell forms](#6-spell-forms)
7. [The upgrade chart](#7-the-upgrade-chart)
8. [Where to try things](#8-where-to-try-things)

---

## 1. Controls

Pad names are PlayStation's. The game is designed for a pad; the keyboard does everything the pad does.

### In a fight

| Action | Pad | Keyboard | Notes |
|---|---|---|---|
| Move | Left stick | W A S D | Attacks turn toward the nearest enemy in the stick's direction. There is no lock-on. |
| Light attack | R1 | J | Press again during an attack to chain. The press is remembered. |
| Heavy attack | R2 (tap) | K (tap) | After one or more lights it is the weapon's chain heavy: quicker, harder, ends the chain. |
| Charged heavy | R2 (hold, let go at the white flash) | K (hold) | Massive Sword, Great Spear, Great Hammer and Armament only. Costs no mana. |
| Weapon skill | R2 (hold to full, let go) | K (hold) | Costs mana (25 to 35 by weapon). Hold time is in each weapon's table. Without the mana the hold gives the heavy or charged heavy. |
| Finisher | R2 with a staggered enemy within 5 m | K | A staggered enemy pulses gold and carries a gold broken ring. Replaces the heavy. |
| Guard | L1 (hold) | L (hold) | Blocks from the front. Turns you to the nearest enemy. You walk slowly while guarding. |
| Perfect guard | L1 just before the blow lands | L | Usually within 0.18 s. Pressing guard again within 0.4 s gives no perfect window. |
| Dodge | Circle | Space | Costs stamina. Can cut short an attack's recovery, never its wind-up or swing. |
| Flask | Square (tap) | R (tap) | Heals. With Fire's Firebomb Flask learned, holding 0.3 s throws it instead. |
| Element spell | Triangle (tap) | U (tap) | The held element's own spell. |
| Worn spell | Triangle (hold 0.3 s) | U (hold) | The spell learned on the chart and worn. With no spell worn, any press is the tap spell. |
| Surge | L2 | I | The element's self buff. One press; it is not held. Pressed while it runs, it starts over at full length and is paid for again. |
| Surge again | L2 while the surge runs | I | Only with a gift that listens for it (Falling Sun, The World Holds Still, The Sea Rises, First Spring). The second press triggers the gift and does not relight the surge. |
| Use | Cross | E | Rest fires, weapon stands, braziers, stones, doors, the next line of speech. |
| Pause | Start (Options) | Escape | Resume, controls, give up the run (castle and summit), forget everything saved (hub), quit. |

### On the upgrade chart (opened by using a rest fire)

| Action | Pad | Keyboard |
|---|---|---|
| Move from mark to mark | Left stick | W A S D |
| Learn the chosen mark; on a spell already learned, wear it | Cross | E |
| Whole-ring view, and back to your tree | R2 | K |
| Leave | Circle | Space |

**Not yet**

- Triangle is the element spell only; the weapon skill lives on the held heavy.
- No hard lock-on. No way to choose whom the guard faces when several enemies are near.
- The controls line on screen says "weapon skill, 30 mana" for every weapon; the real cost is per weapon (see section 3).

---

## 2. How a fight works

- **Health** is 100. A blow taken unguarded makes you flinch for 0.35 s and pushes you back. In a run, falling ends the run and wakes you in the hub with bare hands, Fire, no embers and nothing learned.
- **Stamina** is 100. Attacks, dodges and blocked blows spend it. It refills at 30 a second after 0.8 s of spending nothing.
- **Mana** is 100 at most and comes only from fighting: each weapon blow that lands (4 to 16 by move), a perfect guard (15), a perfect dodge (12), a finisher (15). It pays for the tap spell, the worn spell, the surge and the weapon skill.
- **Guard** blocks the front arc (200 degrees for most weapons). A block costs stamina (0.9 per point of damage for most weapons), and 35% of the damage comes through as grey health. A blow the stamina cannot pay for breaks the guard: full damage and a long flinch.
- **Grey health** is health you can win back: 0.6 per point of damage you deal. It starts fading 2.5 s after the last block or hit. An unguarded blow removes it. Guarding alone cannot kill you.
- **Perfect guard** costs nothing, gives 15 mana, and throws a large amount of stagger back on the attacker. **Red attacks** go through a held guard: dodge them or perfect-guard them.
- **Perfect dodge**: a blow that passes through the first 0.13 s of a roll gives 12 mana and a third of a second of slowed time.
- **Stagger**: blows (heavies most of all) and perfect guards fill an enemy's stagger meter. At 70% it flickers amber. Full, it kneels for about 3 s and pulses gold. Light blows on it then do one and a half times damage. Heavy within 5 m is the **finisher**: three times the finisher's damage, no stamina, 15 mana, and blows pass through you while it lasts.
- **Embers** buy marks on the chart. Each enemy gives 1 the first time it is killed on a visit, the Gate Captain 3, a god 5.
- **Resting** at a rest fire refills health and flasks, brings the area's enemies back (not their embers) and opens the chart. **Flasks**: three, 45 health each, 0.8 s to drink with the heal at 0.4 s. With none left, 120 damage dealt earns one back.

**Not yet**

- Enemies cannot lay afflictions on you.
- Dark's Rise (section 5) is the only way to survive a killing blow.

---

## 3. Weapons

One weapon is carried at a time. Seven can be taken up: the six below Fists plus Fists itself. "After N lights" means the heavy button pressed after that many light attacks in a chain. Damage is before upgrades and surges.

Guard and dodge when a weapon does not say otherwise: guard covers 200 degrees, perfect window 0.18 s, 0.9 stamina per point blocked, 35% comes through as grey health; the dodge goes 4.5 m for 25 stamina.

**Not yet, for every weapon**

- No element changes how a weapon behaves, beyond its colour and the affliction it lays.
- Finishers lift or shove the enemy but do not carry it; enemies have no "held" pose.
- The Massive Sword and Scythe show their own models, which are in the project marked pending approval. The Great Spear and Great Hammer are drawn with stand-in boxes until their models are approved. The Katana is part of the character's own model.
- All new weapons' stands are open from the start because there is nowhere yet to win them. The Katana must still be drawn from the stone in the castle once.

### Fists

The first run's weapon. The quickest chain, short reach, light damage, a great deal of stagger for its damage. Guard is crossed forearms (default numbers); default dodge.

| Input | Move | What it does |
|---|---|---|
| R1 x4 | Jab, cross, hook, kick | 6, 7, 9, 14 damage. Reach 1.8 to 2.2 m. The fastest lights in the game. |
| R2 | Lunging heavy | 18 damage, 34 stagger. Carries you 1.4 m forward. |
| R2 after any light | Rising blow | 20 damage, 42 stagger. Ends the chain. |
| Hold R2 0.4 s | **Skill: Breaker** (30 mana) | A charged straight that lunges 2.4 m: 30 damage, 75 stagger. Fills most of a stagger meter in one blow. |
| R2 on a staggered enemy | Finisher | One opening blow. 24 damage, tripled. |

**Not yet**: one charge tier only (no free charged heavy). The finisher has no name and no staged beats.

### Katana

Found in the stone on the castle's shrine terrace. The sword lives in its sheath and every chain starts with a draw. Default guard and dodge.

| Input | Move | What it does |
|---|---|---|
| R1 x3 | Drawing cut, return cut, stepping cut | 10, 11, 18 damage. Arcs of 140, 140 and 100 degrees, reach 2.4 to 2.7 m. The third steps 1.3 m in. |
| R2 | Overhead | 26 damage, 34 stagger. 90 degree arc, reach 2.8 m. |
| R2 after any light | Overhead, quicker | 30 damage, 40 stagger. Winds up in 0.20 s, not 0.30 s. Ends the chain. |
| Hold R2 0.4 s | **Skill: Iaido Slash** (30 mana) | Sheathe, hold, release: a 170 degree drawing cut that carries you 2.6 m. 48 damage, 50 stagger. |
| R2 on a staggered enemy | Finisher | A sheathed draw-cut through the enemy, a held pose in slowed time, then the return to the sheath. 30 damage, tripled. |

The **Storm Katana** is given with Lightning when it is taken from its god. Same moves, 15% more damage and stagger, and the skill (named Thunder Draw) carries 3.6 m with 3.8 m reach.

**Not yet**: one charge tier only. No perfect-guard or blocked-guard reaction animation.

### Massive Sword

Slow, wide and hard to interrupt. Its guard is the widest (240 degrees) and cheapest per point after the hammer's (0.6 stamina per point, 25% grey), with the tightest perfect window (0.15 s). The dodge is a short heavy dive: 3.9 m for 30 stamina. You walk at 92% speed and creep at a quarter speed while charging.

**Poise**: during a swing, a small blow still hurts but does not stop the swing. Red attacks ignore poise.

| Input | Move | What it does |
|---|---|---|
| R1 x3 | Three sweeps | 24, 26, 34 damage. 190 degree fans at 3.4 m, then a full circle at 3.6 m. Each winds up faster than the last (0.34, 0.26, 0.20 s). Poise 12. |
| R2 | Overhead | Buries itself: 44 damage, 56 stagger. Narrow (50 degrees), reach 3.6 m. Poise 22. |
| R2 after any light | Rising cut | Dragged up off the ground: 46 damage, 60 stagger, 120 degree arc. Poise 22. |
| Hold R2 0.45 s (white flash) | Charged heavy: leaping overhead | 62 damage, 80 stagger. No mana. Poise 30. |
| Hold R2 0.95 s (element-coloured flash, "Ready") | **Skill: Elemental Spike Line** (35 mana) | The sword is driven into the ground (20 damage up close). Eight spikes of the held element run 10 m up the lane, one every 0.09 s. Each enemy is struck once for 34 and takes the element's affliction. |
| R2 on a staggered enemy | **Finisher: The Headsman** | Pommel into the chest, a walk round to the side with the point scoring the ground, the blade held up, the cut. The ground splits along the cut a moment after. 44 damage, tripled. |

**Not yet**: the pommel does not rock the enemy upright. No element changes it (no burning ring under the turn).

### Scythe

Spacing is the skill. Every cut does 0.6 of its damage nearer than 1.5 m (the haft), plain damage in the middle, and **1.5 damage and 1.6 stagger from 2.5 m out** (the tip). The arc is drawn with its outer part bright. Guard: the haft held across, 220 degrees, 1.0 stamina per point. Dodge: a longer step, 4.8 m.

| Input | Move | What it does |
|---|---|---|
| R1 x3 | Out, back, full turn | 13, 13, 20 damage before the bands. 200 degree arcs at 3.5 m, then a full circle at 3.7 m. |
| R2 | Back reap | A hop 1.4 m backward with the cut made by the retreat, so an enemy that was too close is at the tip. 24 damage, 28 stagger, 150 degree arc, reach 3.8 m. |
| R2 after any light | Chain heavy | 28 damage, 34 stagger. Steps 0.8 m in, 120 degree arc. |
| Hold R2 0.4 s | **Skill: Crescent Slash** (30 mana) | A 180 degree cut (16 damage), and a crescent of light that flies 6 m and strikes what it passes for 20. It then hangs there for 1.6 s, biting for 6 every 0.4 s and laying one stack of the held element's affliction with each bite. |
| R2 on a staggered enemy | **Finisher: The Wheel** | The blade hooks under the enemy, one hauling turn, a heave that throws it into the air, a hold underneath, then one rising cut that meets it. 30 damage, tripled. Ignores the distance bands. |

**Not yet**: one charge tier only. The Wheel does not carry the enemy round you. The crescent hangs near the top edge of the view.

### Full Body Armament

What bare hands become in the held element: a coat on the forearms, fists, shoulders and shins in the element's colour, named for it on screen ("Fire Armament"). **Every blow lays one stack of the held element's affliction.** It re-makes itself when you change element. Guard: 200 degrees, 0.8 stamina per point, 20% grey. Default dodge.

| Input | Move | What it does |
|---|---|---|
| R1 x4 | Jab, cross, hook, kick | 8, 9, 11, 16 damage. The fists' timing, a little harder and 0.3 m further. |
| R2 | Lunging heavy | 22 damage, 36 stagger. Carries you 1.8 m. Poise 16. |
| R2 after 1 light | Ram | A shoulder charge that shoves the target 2.5 m. 14 damage, 30 stagger. Poise 22. |
| R2 after 2 lights | Rising blow | 22 damage, 46 stagger: the most stagger for its time. |
| R2 after 3 lights | Slam | Both fists on the ground: a disc of 2.2 m. 20 damage, 34 stagger. |
| R2 after 4 lights | Burst | Both palms: a cone 4.2 m long. 28 damage, 40 stagger, two stacks of the affliction. |
| Hold R2 0.3 s, let go | Charged heavy: Meteor Fist | A leaping punch that carries you 2.6 m. 28 damage, 52 stagger. No mana. Poise 26. |
| Keep holding R2 past 0.3 s (up to 1.2 s more) | **Skill: Energy Reflect** (25 mana, paid only when struck) | You stand braced. A blow that lands on the stance, from any side, guardable or red, does nothing and is answered at once: a cone 5 m long for 20 damage plus the whole of the blow taken, and heavy stagger. With less than 25 mana the blow simply lands. Held to the end unstruck, it lets go as Meteor Fist. |
| R2 on a staggered enemy | **Finisher: One Inch** | Six fast blows that deal nothing and drive the enemy back, a hold with the open hand on its chest, then the arm straightens. The element goes through as a cone and is laid on what stands behind. 26 damage, tripled. |

**Not yet**

- It is taken from a stand like any weapon. The design makes it a mark in every element's tree that replaces Fists while no weapon is carried; that mark is not built.
- The coat is the same eight boxes in every element, coloured. The clips are the fists', set lower and wider.
- The ram stops against its target; it does not pass through a crowd.

### Great Spear

Owns a line. Thrusts are a lane 4 m long and 0.9 m wide; nearer than 1.4 m it is the haft and does six tenths. Its guard is the narrowest (160 degrees; 0.8 stamina per point, 30% grey) but you walk at half speed behind it, faster than behind other guards. Dodge: 4.2 m.

| Input | Move | What it does |
|---|---|---|
| R1 x3 | Thrust, thrust, sweep | 10, 11 down the lane, then a 160 degree sweep at 3.6 m for 15 and 20 stagger. |
| R1 while guarding (L1 held) | Guard thrust | A thrust from behind the guard without lowering it: 8 damage for 12 stamina, lane 3.8 m. Repeats as long as stamina lasts. A blow that arrives during it is still blocked. Does not start or continue the chain. |
| R2 | Slap | The whole length brought down the lane: 24 damage, 32 stagger, 4.4 m long, 1.1 m wide. |
| R2 after any light | Vault | A vault on the planted spear that kicks the target 2 m back. 20 damage, 44 stagger. |
| Hold R2 0.5 s (white flash) | Charged heavy: couched charge | You travel 5 m in a third of a second, shoving what you meet 1.5 m. 30 damage, 36 stagger. No mana. Poise 16. |
| Hold R2 0.9 s (element-coloured flash) | **Skill: Flying Spear** (30 mana) | The spear itself is thrown up to 11 m. It stops in the first enemy for 26, or stands in the ground. A moment later you **run to it**, untouchable for the run, and arrive with a full turning cut that rips it out and strikes all round for 18. **Hold the stick away from the throw** (more than 120 degrees off) to refuse the run: the spear flies back to your hand in half a second. Until it is back you cannot attack. Left alone for 3 s a stuck spear comes home by itself. |
| R2 on a staggered enemy | **Finisher: The Standard** | A low thrust, the enemy levered up on the point, a hold with the spear upright, then the whole length brought over and down, shoving it 1.8 m on. 30 damage, tripled. |

**Not yet**

- The run to the spear stops at an enemy standing in the way; it does not pass through bodies.
- The couched charge pushes against its target; it does not pass through.
- No levelled "line" stance after an attack; the idle is always the upright spear.

### Great Hammer

Stagger on one target, and a guard that has to be read. Every swing has poise. Its block is the cheapest in the game (0.55 stamina per point, 20% grey, 180 degrees) and its **perfect guard opens late**: the press must come between 0.08 s and 0.24 s before the blow. Later than that is only a block, so raise it early. Its dodge is the worst: 3.6 m for 32 stamina. You walk at 90% speed.

| Input | Move | What it does |
|---|---|---|
| R1 x2 | Side swing, then drop | Two lights only. A 150 degree side swing (20 damage, 34 stagger), then a slower drop onto a disc of 1.5 m a stride ahead (26 damage, 48 stagger). Poise 12 and 16. |
| R2 | Quake | A bigger drop with a jump in it: a disc of 2 m, 36 damage, 70 stagger. Poise 22. |
| R2 after any light | Lift | A golfer's drive that throws the target 2.5 m. 34 damage, 64 stagger. Poise 22. |
| Hold R2 0.55 s (white flash) | Charged heavy: Earthshaker | A disc 6 m across: 52 damage, 100 stagger. No mana. Poise 30. |
| Hold R2 1.0 s (element-coloured flash) | **Skill: Spinning Hammer Toss** (35 mana) | Two turns that each strike all round for 14, then the hammer is let go. It flies 8 m end over end through everything for 28, hangs an instant, and comes back to your hand through everything again at half strength. About a second in all, during which you cannot attack. |
| R2 on a staggered enemy | **Finisher: The Bell** | A lift under the chin throws the enemy into the air. You turn your back, let the head to the ground, coil and wait, then turn into it and meet it as it comes down. It leaves sideways, flung 6 m. 36 damage, tripled. |

**Not yet**

- You cannot walk or steer during the spin.
- With the hammer thrown the guard is still the hammer's guard.
- A drop does not reliably miss an enemy standing inside one metre.

### Not built

| Weapon | State |
|---|---|
| Twin Knives | Designed, not started. Needs a second aimed hand and blows that count from behind. |
| Great Bow | Designed, not built. |
| Chain Sickle | Designed, not built. |

---

## 4. Elements

One element is held at a time. Each gives a **tap spell** (Triangle / U), a **surge** (L2 / I), and an **affliction** laid on enemies. Each affliction on an enemy shows as a small diamond in its colour under the enemy's bars, larger the deeper it lies.

### The ring

In ring order, clockwise. Each element shares a mixed branch with the one before it and the one after it.

| # | Element | Affliction | Tap spell | Surge | Mixes (with) |
|---|---|---|---|---|---|
| 0 | Fire | Burn | **Flame Wave**, 25 mana | **Kindle**, 40 mana, 10 s | Pyre (Poison), Plasma (Lightning) |
| 1 | Lightning | Static | **Thunderclap**, 25 mana | **Overdrive**, 40 mana, 10 s | Plasma (Fire), Tempest (Wind) |
| 2 | Wind | Unsteady | **Gust**, 20 mana | **Tailwind**, 35 mana, 10 s | Tempest (Lightning), Blizzard (Ice) |
| 3 | Ice | Chill | **Hoarfrost**, 25 mana | **Glaze**, 40 mana, 12 s | Blizzard (Wind), Glacier (Water) |
| 4 | Water | Soaked | **Riptide**, 20 mana | **Wellspring**, 30 mana, 12 s | Glacier (Ice), Bloom (Life) |
| 5 | Life | Seeded | **Briar Patch**, 20 mana | **Regrowth**, 40 mana, 6 s | Bloom (Water), Overgrowth (Earth) |
| 6 | Earth | Tremor | **Stone Rise**, 25 mana | **Bedrock**, 40 mana, 10 s | Overgrowth (Life), Forge (Metal) |
| 7 | Metal | Sundered | **Flung Blade**, 25 mana | **Temper**, 40 mana, 10 s | Forge (Earth), Halo (Light) |
| 8 | Light | Marked | **Glare**, 25 mana | **Clarity**, 40 mana, 8 s | Halo (Metal), Eclipse (Dark) |
| 9 | Dark | Cursed | **Grasp**, 25 mana | **Hunger**, no mana, 10 s | Eclipse (Light), Blight (Poison) |
| 10 | Poison | Poisoned | **Miasma**, 25 mana | **Fever**, 40 mana, 10 s | Blight (Dark), Pyre (Fire) |

### What each tap spell does

A tap spell is a short committed cast. A dodge or a blow taken breaks it; the mana is spent either way.

| Element | Spell | What it does |
|---|---|---|
| Fire | Flame Wave | A cone of fire 5.5 m ahead, 70 degrees wide: 20 damage, 24 stagger, and what it touches burns for 5 s. |
| Lightning | Thunderclap | A bolt on the enemy faced, up to 10 m off: 16 damage, 45 stagger. It leaps to two more enemies. |
| Wind | Gust | A cone 5 m ahead: 8 damage, 20 stagger, Unsteady two deep. Everything within reach is thrown 3 m away. |
| Ice | Hoarfrost | A wide fan (110 degrees) 4.5 m ahead: 10 damage, 15 stagger, Chill two deep. |
| Water | Riptide | A wave 5 m ahead, 90 degrees wide: 8 damage, 18 stagger, Soaked. Everything within reach is dragged 2 m toward you. |
| Life | Briar Patch | A bed of thorns 3 m ahead that stands 6 s, bites twice a second and seeds what stands in it. It counts as something grown. |
| Earth | Stone Rise | A pillar bursts up under the enemy faced (within 5 m) after a quarter second: 14 damage, 30 stagger, Tremor, and it lifts what it strikes. |
| Metal | Flung Blade | A blade is left standing in the enemy faced (within 9 m) for 6 s. Your next weapon blow on it drives it in for 12 and a notch of Sundered. |
| Light | Glare | A flash 4 m ahead, 90 degrees wide: 8 damage, 22 stagger, Marked. |
| Dark | Grasp | A hand of shadow to the enemy faced within 6 m: 10 damage, 12 stagger, Cursed, and it is dragged to your feet. |
| Poison | Miasma | A cloud 4 m ahead that hangs 4 s and gives what stands in it one dose of poison a second. |

### What each surge does

For every element but Fire and Lightning, **every weapon blow during the surge lays one stack of the element's affliction.**

| Element | Surge | What it does while it lasts |
|---|---|---|
| Fire | Kindle | Weapon damage +30%. Every blow sets its target burning for 3 s. |
| Lightning | Overdrive | Every swing 30% faster, +8 stagger a blow, weapon damage +10%. |
| Wind | Tailwind | The dodge costs half, goes 30% further and is safe 25% longer. You are 15% quicker. |
| Ice | Glaze | One ward: it takes the next blow that would have landed. |
| Water | Wellspring | Flasks heal half again as much. |
| Life | Regrowth | Grey health is made real at once, then 4 health a second. |
| Earth | Bedrock | 20% less damage taken. +6 stagger a blow. |
| Metal | Temper | 30% less damage taken. |
| Light | Clarity | Grey health is made real at once. Perfect guard and dodge windows are half again as long. |
| Dark | Hunger | Costs health, not mana: 3 health a second drain away (never the last of it). Weapon damage +30%, and 30% of each blow's damage comes back as health. |
| Poison | Fever | 1 health a second, and 6% of each blow's damage comes back as health. |

### What each affliction does

| Affliction | Depth and length | What it does to the enemy |
|---|---|---|
| Burn | Up to 5 stacks, 4 s, topped up | 5 damage a second; each stack adds 30%. Some marks set it off: 10 damage a stack. |
| Static | Up to 5, 6 s | Waits to be set off: 7 damage and 5 stagger a stack to everyone within 4 m. A heavy blow or a finisher discharges it (unverified whether this needs Lightning's Static mark learned). |
| Unsteady | Up to 5, 6 s | Each stack: 6% more stagger taken, shoved 15% further, turns 8% slower. |
| Chill | Up to 5, 6 s | Each stack slows it 7%. At five it is **Frozen**: held still for 2.5 s (elites and gods thaw sooner). A heavy blow shatters a frozen enemy for 45. |
| Soaked | On or off, 8 s | Takes 25% more from spells. Static and Chill laid on it pass to everyone within 4 m. |
| Seeded | Up to 3, 8 s | Each of your blows on it heals you 1 a stack. When it dies it bursts into thorns: 12 a stack within 3 m. |
| Tremor | Up to 3, 6 s | Each stack slows it 10%. Its balance (stagger meter) does not recover. |
| Sundered | Up to 3 notches, 8 s | Takes 8% more weapon damage a notch. |
| Marked | One deep, 8 s or three blows | Every weapon blow on it is critical (one and a half times damage) and gives 2 more mana. Spent after three blows. |
| Cursed | One deep, 8 s | Deals 15% less. Killing it heals you 6. |
| Poisoned | Up to 8 doses, 6 s | 2 damage a dose, landing in a burst every 2 s. Some marks set it off: 6 a dose. |

Critical blows: none by chance to begin with; marks add a chance. A critical blow shows a gold star where it lands.

### Taking and consuming

When a god falls it leaves two things on the floor; walk up and use one. **Take**: your element and tree switch to the god's, the old tree is forgotten and the embers spent on it are given back, you receive the god's own weapon and the **god's gift** at the crown of the new tree. **Consume**: you keep your element and gain 4 embers; if the god's element is your neighbour on the ring you also receive the **crown** of the branch the two share, and a plain version of the god's weapon. Either choice restores you. A gift that is a spell is worn at once.

A **mixed branch** opens when you hold one of its two elements and have held or consumed the other in this run. Sealed, it is still drawn on the chart, and the words panel says which element opens it.

**Not yet**

- Only one god exists: Narukami, of Lightning. In play the choice is therefore Take (Lightning, the Storm Katana, Storm Step) or Consume (stay as you are, 4 embers, a plain Katana; for a Fire holder also Plasma and a stronger Flame Wave and Kindle).
- The other nine elements have no gods. Their braziers in the hub are lit from the start as a stopgap. Lightning's stays cold until it has been taken once.
- Gust and Riptide move everything within reach, not only what the cone touched. Stone Rise's pillar does not stand to be broken. Flung Blade deals nothing as it lands. Grasp does not drag you to what cannot be dragged.
- Glaze, Wellspring, Bedrock, Temper, Clarity and Fever have only the plain halves listed above.
- Dark's "one revive as a zombie" is not the surge; it exists only as the god's gift A Life for a Life (section 5).

---

## 5. Gods' gifts and mix crowns

A gift is never bought. It sits at the crown of its tree, drawn as a crowned sun. Most cost nothing to use and are begun by something you already do. In the practice yard two debug shrines hand them out (section 8).

### Gods' gifts: one per element

Given by **taking** the element from its god.

| Element | Gift | How you trigger it | What it does |
|---|---|---|---|
| Fire | **Falling Sun** | Light Kindle (L2). Press L2 again to drop it early. | A sun grows over the enemy you face and follows it. Every second, burns within 6 m of you deepen by one. Pressed again, or when Kindle ends, it falls: 60 damage in 4 m, +8% for each burn stack under it (up to double), burning ground after, half a second of slowed time. A blow taken puts it out. |
| Lightning | **Storm Step** | Light Overdrive (L2), then dodge. | While Overdrive lasts every dodge costs no stamina, shocks what it passes through, and leaves a shape of light where you stood. |
| Wind | **The Wind Remembers** | Light Tailwind (L2), then land weapon blows. | Each blow leaves a pale cut hanging where it fell, up to sixteen. When Tailwind ends time slows and they all fall, each for half the blow it remembers. |
| Ice | **The World Holds Still** | Light Glaze (L2). Press L2 again to break it. | A shell of six fangs of ice stands round you. The second press breaks it outward: a ring to 9 m that strikes each enemy once (10 damage) and freezes it, with enemies slowed to a fifth for 1.5 s. Also, always: shattering a frozen enemy sends a line of breaking ice to the nearest frozen one within 7 m and shatters it too, and on. |
| Water | **The Sea Rises** | Light Wellspring (L2). Press L2 again. Costs one flask. | Floods a disc of 9 m for 6 s. Everything in it is Soaked, and each weapon blow runs out from the one struck as a wave carrying 40% of the blow. Then everything is dragged 4 m to you and a cone (9 m, 120 degrees) breaks where you face for 55 damage and 90 stagger. |
| Life | **First Spring** | Light Regrowth (L2). Press L2 again. | You give half your present health (left as grey). Trunks and thorns come up through every Seeded enemy within 12 m (eight at most), hold it for 4 s and strike it for 30. Blows on the held return half their damage as health, up to what was given. With no one Seeded, nothing is taken. |
| Earth | **Hand of the Mountain** | Light Bedrock (L2), then land heavy blows. | A hand of stone hangs over your sword shoulder. Each heavy blow that lands, it goes over that enemy and half a second later comes down: 70% of the blow in 3 m, heavy, with Tremor. |
| Metal | **Hanging Swords** | Light Temper (L2), then hold guard (L1). | While Temper lasts every blow held on your guard is a perfect guard, and a sword is forged above the attacker, eight at most. When Temper ends they fall in turn. |
| Light | **The Scales** | Light Clarity (L2), then deal more than you take. | Two pans of light hang over you: gold for damage your weapon deals, red for health you lose. When Clarity ends: if you dealt more, a sword of light falls on every Marked enemy within 12 m for 60% of the difference (120 at most). If not, your mana is emptied. |
| Dark | **A Life for a Life** | Take a blow that would kill you. | Once, you stand again at half health, and it drains 4 a second down to a sliver. Brackets and a sign mark the one that killed you. Land a finisher on it (on anyone, if it is gone) to stop the drain, return to half health, and earn the right to rise once more. |
| Poison | **Clean Blade** | Light Fever (L2), then land weapon blows. | While Fever lasts poison on every enemy neither bites nor fades, and every blow doses twice. When Fever ends time slows, you flick the blade, and each poisoned enemy within 14 m takes every bite it was owed at once. |

### Mix crowns: one per mixed branch

Given by **consuming** a god whose element is your neighbour on the ring.

| Branch | Crown | How you trigger it | What it does |
|---|---|---|---|
| Plasma (Fire + Lightning) | **Plasma** | Land a heavy blow or a finisher on a burning enemy. | It bursts for 18 damage and 16 stagger in 3.2 m and sets alight everyone it reaches. |
| Tempest (Lightning + Wind) | **Falling Thunder** | Land any finisher. | The enemy is thrown up, you vanish in a line of light, and 0.34 s later you land on it as a bolt: 22 damage in 4.5 m with Static and Unsteady three deep, everything pushed 3 m. You are untouchable throughout. |
| Blizzard (Wind + Ice) | **Diamond Dust** | Shatter a frozen enemy, then dodge. | Four shards from each shatter join a ring turning round you, twelve at most. Your next dodge throws them down its path: a lane 9 m long, 6 damage a shard, in volleys of four. |
| Glacier (Ice + Water) | **Weight of Ice** | Guard blows (L1), then swing a heavy. | Each blow guarded lays a plate of ice on you; a perfect guard lays two; five at most. Each plate turns 6% of every wound. Your next heavy swing spends them all as a lane of ice fangs, longer and harder for each plate. |
| Bloom (Water + Life) | **Great Lotus** | Drink a flask while your surge is running. | Five petals open on a disc of 3 m for 10 s. Enemies inside are Soaked and Seeded each second; you mend 2 health a second inside. Each perfect guard made inside closes a petal over the attacker and holds it 3 s. At the last petal, or when time is up, it bursts: 30 damage, +30% for each petal closed. |
| Overgrowth (Life + Earth) | **The Ground Wakes** | Light your surge (L2). | Sixteen stones and roots rise in a ring of 7 m. Everything inside is held for 2 s, given Tremor three deep, and seeded. Then it flowers: 15 damage inside, and you heal 5 for each enemy held. |
| Forge (Earth + Metal) | **World Anvil** | Land any finisher, then swing a heavy. | An anvil rises under the finished enemy. Its ring strikes every other enemy within 6 m (15 damage), shakes it and breaks its armour. A blade is forged for each one reached, six at most, and carried at your shoulders. They fly at nearby enemies with your next heavy swing: 10 damage and a notch each. |
| Halo (Metal + Light) | **Six Mirrors** | Hold Triangle / U (worn spell, 60 mana) facing an enemy within 8 m. | Six mirrors stand in a ring round the enemy. You vanish; a line crosses from each mirror in turn; you are back where you began, and a moment later all cuts land: 14 each on the enemy, half on anything else a line crossed, and a Mark and a notch for each cut. Untouchable throughout. |
| Eclipse (Light + Dark) | **Totality** | Light your surge (L2), then fight. | A black sun hangs over you and the ground goes dark. Each second every enemy within 14 m is Marked and Cursed. Every blow you land is critical and heals you 1. When the surge ends a column of light strikes each enemy for half of what your weapon took from it in the dark, 80 at most. |
| Blight (Dark + Poison) | **The Reaping** | Land a finisher on an enemy that is both Cursed and poisoned. | Time slows and a great scythe sweeps once round the enemy. All the poison in every enemy within 9 m goes off at once (plus 3 a dose), and half the total heals you. |
| Pyre (Poison + Fire) | **Light the Pyres** | Hold Triangle / U (worn spell, 60 mana) and stand still. | You kneel. A line of fire runs to each burning or poisoned enemy within 9 m (six at most) and follows it. Three rings mark three breaths over 2.2 s. Then a pillar rises under each: 40 damage in 2 m, its burn detonated and its poison set off. A dodge or a blow taken breaks it. With no one alight the mana comes back. |

### Four other spells with moves of their own

These are bought on the chart like any spell, then worn and cast with hold Triangle / U.

| Spell | Where | Cost | What you do and what it does |
|---|---|---|---|
| **Seven Flashes** | Lightning tree (grows from Afterimage) | 4 embers, 45 mana | You vanish. Seven lines are drawn on the ground from enemy to enemy within 8 m (one enemy: through it from seven sides). You stand again on the starting spot, back to the fight, still for half a second. Then the cuts land together (9 each), and any enemy at full Static discharges. Untouchable while gone, open while still: struck then, the cuts land at half and nothing discharges. |
| **The Wave Stands Still** | Glacier branch | 5 embers, 50 mana | **Keep the button held**: a wave 6 m wide rolls out, up to 9 m, soaking what it meets and carrying it along. **Let go**: it turns to a wall of ice for 4 s. Nothing walks through it, and everything within 3 m behind its face is Frozen. A heavy swing beside the wall shatters it and all it holds (45 damage, 60 stagger each). |
| **Ball Lightning** | Plasma branch | 4 embers, 40 mana | A ball of white fire appears 1.5 m ahead and drifts slowly after the nearest enemy for 8 s. **Hit it with a weapon swing** to send it along the swing and grow it a step, three at most. It bursts on the first enemy it meets, bigger and harder for each strike, laying Burn and Static. Left alone it bursts small when its time is up. |
| **Pawned Heart** | Dark tree | 4 embers, no mana | Your health drops to 1. The rest is set down behind you as a beating heart in a ring that closes over 10 s. While it beats your weapon deals 60% more and swings cost the least stamina they can. **Walk at least 2.6 m away, then back onto it** to get back all that was pawned plus 30% of the damage dealt meanwhile. If the ring closes first: health stays at 1 and half of what was pawned comes back grey. |

**Not yet**

- The design asked for "hold the surge button" on four gifts; as built it is "press the surge button again".
- The Scales: the design's "no mana for five seconds" is "mana is emptied".
- A Life for a Life is the only thing that grants the Rise. Being Risen for good (Dark's Dead Already) is not built.
- The Wave Stands Still soaks and freezes training dummies but does not carry them along.
- Hanging Swords works by making the perfect-guard window very long during Temper.
- None of the gifts has been played or tuned.

---

## 6. Spell forms

A form is the shape a spell takes. Any element can wear any form in its own colour with its own affliction; the marks of kind Spell on the chart each name one.

**Wearing and casting**: the first spell you learn is worn at once. To wear another, choose a spell you already own on the chart and press learn (Cross / E). The HUD names the worn spell beside the weapon. **Hold Triangle / U for 0.3 s** to cast the worn spell; a tap still casts the element's own.

| id | What the player sees and does |
|---|---|
| `wave` | A cone thrown ahead that strikes everything in it. |
| `bolt` | A bolt on the enemy faced that leaps on to others near it. |
| `leap` | A leap out of the fight, untouchable in the air, down beside the enemy faced, leaving a crater. |
| `rod` | A rod planted a little ahead; the element strikes round it for a while. |
| `blades` | Blades appear high over the enemy faced (or the aim point), hang, then fall one after another, each striking a small circle. |
| `sword` | One sword as tall as the screen falls on the aim point after a wait: a wide circle, a held frame on impact, and a crack left on the ground that bites for a while. |
| `beam` | A braced stance, then a thick beam held for some seconds, biting at intervals. You can turn slowly while it lasts. You are rooted; the dodge lets you go. |
| `phalanx` | Blades form in a fan behind your shoulders and hover. Each fires on its own at the nearest enemy in reach, or all at once when you land a heavy blow. |
| `dragon` | A great head of the element forms at your shoulder and lunges forward along a line, jaws closing at the end for a burst. Everything on the line is carried along a little. |
| `nova` | A burst round you after a short wait, with a ring that sweeps outward. |
| `orb` | A slow orb leaves the hand and follows the nearest enemy, then detonates. Strike it with a weapon blow and it detonates at once, larger. |
| `bomb` | You raise a growing sphere overhead (rooted), then throw it forward: a great burst and a burning patch after. |
| `spikes` | Spikes run out along the ground in a line from you, one after another, each erupting a moment after its mark shows. Some have several rows side by side. |
| `field` | A patch of ground at the aim point grows spikes, flame or frost for a while, biting at intervals. |
| `drill` | You become a spinning point of the element and bore forward through everything on the line, striking each enemy several times, and come out the far side facing back. Untouchable while it lasts. |
| `minions` | Small helpers of the element rise beside you. Each runs at the nearest enemy, strikes it a few times, then goes. |
| `bullets` | Bullets appear in a ring round the enemy faced, hang, then all fly inward together. |
| `reverse` | A point ahead pulls every enemy near it inward for a moment, then bursts. |
| `parry` | A guard stance held for a while. The first blow that lands on it is turned for nothing and answered with a cone of the element. If nothing strikes it, the mana is lost. |
| `wall` | A wall rises ahead after a wait and blocks movement and shots for a while. It bursts when it ends, or when you strike it with a heavy blow. |
| `discs` | Discs orbit you for a while, striking what they pass. |
| `brand` | A sign is set on the enemy faced. Each of your next few weapon blows on it sets off a burst. Needs an enemy faced, or the mana is lost. |
| `rain` | For a while, strikes fall at random inside a circle round the aim point, each with its own small mark on the ground first. |
| `clone` | A copy of you in the element's light stands beside you and repeats every weapon blow a beat late for a share of its damage. |
| `flash` | You vanish and lines of light cross the enemy faced, one after another. You reappear behind it, still, back turned. After a wait all the cuts land at once. Untouchable while it lasts. |
| `ring` | A ring closes round you and the enemy faced for a while. Nothing crosses it; enemies that touch it are bitten. |
| `pillars` | Pillars burst from the ground under the nearest enemies in reach, each lifting what it strikes for a moment. |
| `step` | You step through the enemy faced in a line of the element and cut it in passing. If it lands, your next heavy blow within a short time is critical. |

Every form shows itself coming on the ground before it lands (never less than 0.22 s), lands with a white star and a ring, and leaves a mark behind.

**Not yet**

- Enemies press against a wall or a ring and do not walk round it. The ring holds you in as well.
- The gods' gifts and crowns have forms of their own (section 5) and are not worn by other elements.

---

## 7. The upgrade chart

**What it is.** One night sky holding every tree: 428 marks. Eleven element trees (30 to 36 marks each) stand round a ring in the order of section 4, each growing outward from its element's emblem. In the gap between each pair of neighbours grows the mixed branch they share (7 to 10 marks). The chart opens when you use a rest fire, and the world stops while it is open.

**How a tree is built.** One root at the foot. Three boughs grow from it, each a spine of majors with minors hanging off, each ending in a keystone. Four or five spells. The god's gift at the crown. A mixed branch ends in its own crown.

### Mark kinds and costs

| Kind | Cost in embers | Frame | What it is |
|---|---|---|---|
| Root | 2 | The tree's foot | The foot of a tree. Turns the element's affliction on for weapon blows. |
| Minor | 1 | Small and round | A plain number: more damage, cheaper dodge, and so on. |
| Major | 3 | Square | A new rule: something happens on a trigger. |
| Spell | 4 | Diamond | A move you perform with the spell button held (section 6). |
| Keystone | 6 | Large, eight-sided | The end of a bough. Changes how the element plays. |
| Mix | 3 to 5, written on the mark | Six-sided | A mark on a branch shared by two neighbouring elements. Some are spells. |
| Signature (a god's gift) | Never sold | The largest, a crowned sun | Given only by a god's fall (section 5). |

A run through the castle and its god pays about 19 embers. No tree can be bought whole in a run, and two keystones are beyond one run, so a path must be chosen.

### What can be learned

A mark can be learned when all of these hold:

- **It is yours**: it is on the tree of the element you hold, or on one of the two branches that element shares.
- **It is open**: a mix mark needs its branch open (you hold one element and have held or consumed the other this run). Otherwise it reads "Sealed" and names the element that opens it.
- **It is reachable**: it grows from the foot of its tree, or **any one** of the marks it grows from is already learned. Otherwise the panel says "Grows from ..." and names them.
- **You carry enough embers.**

How a mark is tinted tells you where it stands: the element's colour when learned; pale white when it can be bought now; grey-blue when reachable but too dear; dark when out of reach; dull red when sealed; dull gold for a god's gift. A gold ring sits round the worn spell. Other elements' trees are drawn faint.

Taking a new element forgets the old tree and gives back the embers spent on it. Falling in a run loses everything learned.

### Moving around and zoom

- The chart is turned so your element's tree stands upright. The view stays close on that tree and leans toward the chosen mark.
- **Left stick / W A S D** moves the choice to the nearest mark in that direction. Hold to keep moving. The choice moves only among your tree and its two shared branches.
- **Cross / E** learns the chosen mark. On a spell you already own, it wears that spell.
- **R2 / K** pulls the view back to show the whole ring with every tree; the words panel is hidden while pulled back. Press again to return. There are only these two zoom levels.
- **Circle / Space** leaves.
- The words panel on the right gives the mark's name, kind and cost, how it stands with you (learned, worn, what it grows from, what opens it) and its description. The title shows your element and embers.

**Not yet**

- The chart has been looked at only at the editor's 1920 by 936 view.
- Some marks do less than their words; none of the numbers has been played for balance.
- Counts that marks bank show as words on the HUD, not pips.

### Every mark

All 428 marks, written out from the game's own data: the eleven trees in ring order, then the eleven mixed branches.
"Embers" is the price; a gift is never sold. "How it happens" is read from the mark's first effect, so for a mark with
several effects the description is the fuller account. About half of these do everything their words say today; the
rest are carried by the nearest thing the engine can do and are waiting for their own move.

#### Fire (34)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Cinder** | Root | 2 | Every blow of the weapon sets a burn, and each blow deepens it, up to five. | Always on once it is yours. |
| **Firebomb Flask** | Major | 3 | Hold the flask button to throw it. No healing: a great burst of fire where it lands. | Happens when you throw a flask (hold the flask button). |
| **Backdraft** | Major | 3 | A perfect guard throws a cone of fire back at the one who struck. | Happens when you guard at the last moment (a perfect guard). |
| **Wildfire** | Major | 3 | Where an enemy dies the ground burns for a while. | Happens when an enemy dies. |
| **Heat** | Major | 3 | Blows landed without being struck build Heat, up to five. Each level adds damage. A blow taken puts it out. Kindle fills it at once. | Always on once it is yours. |
| **Struck Flint** | Minor | 1 | A perfect guard gives more mana. | Always on once it is yours. |
| **Kindling** | Minor | 1 | The moment in which a guard is perfect lasts a little longer. | Always on once it is yours. |
| **Banked Coals** | Minor | 1 | Heat cools more slowly between blows. | Always on once it is yours. |
| **Short Breath** | Minor | 1 | Flame Wave costs less mana. | Always on once it is yours. |
| **Furnace Breath** | Minor | 1 | Backdraft breaks balance harder. | Always on once it is yours. |
| **Broad Wave** | Minor | 1 | Flame Wave reaches further. | Always on once it is yours. |
| **Stoked** | Minor | 1 | Heat rises on fewer blows. | Always on once it is yours. |
| **Burn and Detonate** | Major | 3 | A perfect guard against a burning enemy sets all of its burn off at once. | Always on once it is yours. |
| **Burning Crescent** | Major | 3 | The Iaido Slash throws a slow arc of flame that travels on after the cut. | Always on once it is yours. |
| **Flame Trail** | Major | 3 | The dodge leaves a line of fire behind it that burns what follows. | Always on once it is yours. |
| **Meteor Leap** | Spell | 4 | Hold the spell button to leap out of the fight and come down on the enemy faced, leaving a burning crater. 50 mana. | Hold Triangle (U) to cast. 50 mana. |
| **Long Embers** | Minor | 1 | Flame Trail burns longer on the ground. | Always on once it is yours. |
| **Cinder Fan** | Spell | 4 | Hold the spell button to throw fire in a wide fan, close in. What it touches burns two deep. 30 mana. | Hold Triangle (U) to cast. 30 mana. |
| **Kiln Stance** | Spell | 4 | Hold the spell button to stand ready in flame. The first blow that reaches you, red or not, is turned and answered with a pillar of fire. 30 mana. | Hold Triangle (U) to cast. 30 mana. |
| **Wide Hearth** | Minor | 1 | Backdraft and the detonation reach further. | Always on once it is yours. |
| **Long Kindle** | Minor | 1 | Kindle lasts longer. | Always on once it is yours. |
| **Hot Coals** | Minor | 1 | Spells strike burning enemies harder. | Always on once it is yours. |
| **Catching Fire** | Major | 3 | A burning enemy that dies throws its burn onto everyone near it. | Happens when an enemy dies. |
| **Flashpoint** | Major | 3 | After a perfect dodge, your next blow sets a burn three deep at once. | Happens when you dodge at the last moment (a perfect dodge). |
| **Hot Footing** | Minor | 1 | Flame Trail bites harder. | Always on once it is yours. |
| **Searing Edge** | Minor | 1 | Burns bite harder. | Always on once it is yours. |
| **Sunspot** | Spell | 4 | Hold the spell button to brand the enemy faced. Each blow you land feeds the brand, up to six. A heavy blow sets it off. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Open Furnace** | Keystone | 6 | A perfect guard sets off every burning enemy near you, not only the one who struck, and leaves each still alight. A plain guard lets more of the blow through. | Happens when you guard at the last moment (a perfect guard). |
| **Slow Fuse** | Minor | 1 | Burns last longer. | Always on once it is yours. |
| **Dry Tinder** | Minor | 1 | Your weapon cuts deeper into a burning enemy. | Always on once it is yours. |
| **Wildfire Rush** | Spell | 4 | Hold the spell button to drive through the enemies ahead in a line of fire. Each one passed is set burning and raises your Heat. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Scorched Earth** | Keystone | 6 | Fire you leave on the ground burns twice as long. While you stand in it Kindle does not run down and your blows give more mana. Your spells cost more. | Always on once it is yours. |
| **White Heat** | Keystone | 6 | Heat rises to eight, and above five your blows come faster. Struck above five, you burn. | Always on once it is yours. |
| **Falling Sun** | Signature | gift | When Kindle is lit a sun rises over the fight. While it hangs, every burn near you deepens by itself. When Kindle ends, or when you press the surge button again, it falls on the enemy you face, heavier for every burn it finds. A blow taken puts it out. | Happens when you light the surge (press L2 (I)). |

#### Lightning (36)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Static** | Root | 2 | Blows build Static on an enemy, up to five. A heavy blow or a finisher discharges it, and it arcs to everyone near. | Always on once it is yours. |
| **Stored Charge** | Major | 3 | A perfect guard banks a charge. The next blow spends it for double damage and stagger. | Always on once it is yours. |
| **Long Arc** | Minor | 1 | A discharge arcs further. | Always on once it is yours. |
| **Heavy Air** | Minor | 1 | Static stays longer on an enemy. | Always on once it is yours. |
| **Conductor** | Major | 3 | Thunderclap leaves Static two deep on all it strikes, and sets off an enemy already at five. | Happens when a spell of yours strikes an enemy. |
| **Third Leap** | Minor | 1 | Thunderclap leaps to one more enemy. | Always on once it is yours. |
| **Deep Reserve** | Major | 3 | Perfect guards bank up to three charges. Each blow spends one. | Happens when you guard at the last moment (a perfect guard). |
| **Blink** | Major | 3 | The dodge becomes a short step through lightning that shocks whatever it passes through. | Always on once it is yours. |
| **Open Seam** | Minor | 1 | Blows on an enemy carrying Static are critical more often. | Always on once it is yours. |
| **Forked Step** | Minor | 1 | Blink shocks a wider path. | Always on once it is yours. |
| **Quick Clap** | Minor | 1 | Thunderclap costs less mana. | Always on once it is yours. |
| **Delayed Cuts** | Major | 3 | The Iaido Slash passes through. Its cuts land a moment later, all at once, when the blade is home. | Always on once it is yours. |
| **Biting Step** | Minor | 1 | Blink shocks harder. | Always on once it is yours. |
| **Keen Edge** | Minor | 1 | Critical blows cut deeper. | Always on once it is yours. |
| **Hard Charge** | Minor | 1 | A stored charge breaks balance harder. | Always on once it is yours. |
| **Quick Return** | Minor | 1 | A dodge that breaks off an attack costs less stamina. | Always on once it is yours. |
| **Grounding** | Major | 3 | A perfect guard sends an arc through the one who struck and on to two more. | Happens when you guard at the last moment (a perfect guard). |
| **Quick Hands** | Minor | 1 | The weapon is swung a little faster. | Always on once it is yours. |
| **Full Discharge** | Major | 3 | A discharge of five Static holds its enemy still for a moment and gives mana. | Happens when you set off an affliction. |
| **Lightning Rod** | Spell | 4 | Hold the spell button to plant a rod. Bolts fall around it for a while. Hold again to step to it in a flash. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Waiting Thunder** | Spell | 4 | Hold the spell button to hang five bolts at your shoulder. Each blow you land sends one down with it. A heavy blow sends all that are left. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Afterimage** | Major | 3 | A dodge at the last moment leaves a copy of you that strikes where you stood. | Always on once it is yours. |
| **Bright Echo** | Minor | 1 | The copy left by Afterimage strikes harder. | Always on once it is yours. |
| **Live Wire** | Major | 3 | While a rod stands, a wire of lightning runs from it to you. Enemies it crosses are shocked. | Happens when you cast a spell (Triangle (U)). |
| **Running Current** | Minor | 1 | Your blows give more mana. | Always on once it is yours. |
| **Capacitor** | Minor | 1 | A perfect guard gives more mana. | Always on once it is yours. |
| **Charged Step** | Minor | 1 | A perfect dodge gives more mana. | Always on once it is yours. |
| **Single Stroke** | Keystone | 6 | Static builds to ten. A discharge no longer arcs: all of it lands on the one struck, twice as hard. | Happens when a heavy blow lands. |
| **Seven Flashes** | Spell | 4 | Hold the spell button to cut through up to seven enemies in one breath and return to where you stood. The cuts land together when you are still. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Long Overdrive** | Minor | 1 | Overdrive lasts longer. | Always on once it is yours. |
| **Slipstream** | Major | 3 | A dodge at the last moment quickens your feet and your weapon for a while. | Happens when you dodge at the last moment (a perfect dodge). |
| **Lodestone** | Spell | 4 | Hold the spell button to drag every enemy near toward you, further for each Static on it, then set all of it off at once. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Never There** | Keystone | 6 | Every dodge carries further and a perfect dodge costs nothing. Your guard holds half as well. | Happens when you dodge at the last moment (a perfect dodge). |
| **Overclock** | Keystone | 6 | Overdrive opens with a breath in which everything but you is slowed. Each perfect guard or perfect dodge while it lasts opens another. Overdrive costs more mana. | Happens when you guard at the last moment (a perfect guard), while the surge lasts (press L2 (I)). |
| **Chain Lance** | Spell | 4 | Hold the spell button to throw a bolt that leaps through four more and leaves Static on each. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Storm Step** | Signature | gift | While Overdrive lasts, every dodge costs nothing and shocks what it passes through. | Always on once it is yours. |

#### Wind (31)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Unsteady** | Root | 2 | Blows knock an enemy Unsteady, up to five. It is pushed further, turns slower and loses its balance sooner. | Always on once it is yours. |
| **Rushing Wake** | Major | 3 | The first blow after any dodge strikes harder and leaves its enemy more Unsteady. | Happens when a dodge ends. |
| **Following Draft** | Minor | 1 | Rushing Wake waits longer for its blow. | Always on once it is yours. |
| **Rising Cut** | Major | 3 | A heavy blow on an enemy five deep in Unsteady throws it into the air. It hangs there helpless, and each blow holds it up a little longer, up to three. | Happens when a heavy blow lands. |
| **Long Stride** | Minor | 1 | The dodge carries further. | Always on once it is yours. |
| **Against the Wall** | Major | 3 | An enemy pushed into a wall or into another enemy is hurt and shaken by it. So is the one it hits. | Happens when a spell of yours strikes an enemy. |
| **Thin Air** | Minor | 1 | The dodge is safe a moment longer. | Always on once it is yours. |
| **Off Balance** | Minor | 1 | Unsteady lasts longer. | Always on once it is yours. |
| **Loose Footing** | Minor | 1 | Heavy blows leave an enemy two deeper in Unsteady. | Happens when a heavy blow lands. |
| **Easy Breath** | Minor | 1 | Gust costs less mana. | Always on once it is yours. |
| **Passing Cut** | Major | 3 | Dodge through an enemy to cut it as you pass. It is left facing where you were. | Happens when a dodge ends. |
| **Light Feet** | Minor | 1 | The dodge costs less stamina. | Always on once it is yours. |
| **Wide Gust** | Minor | 1 | Gust spreads wider and reaches further. | Always on once it is yours. |
| **Hammer Down** | Major | 3 | A heavy blow on an enemy in the air drives it into the ground. The landing bursts. | Happens when a heavy blow lands. |
| **Clean Pass** | Minor | 1 | Passing Cut bites deeper. | Always on once it is yours. |
| **Hang Time** | Minor | 1 | Enemies thrown into the air hang longer. | Always on once it is yours. |
| **Turned Aside** | Major | 3 | A perfect guard throws the attacker back and leaves it three deeper in Unsteady. A thrown attack is sent back to its thrower. | Happens when you guard at the last moment (a perfect guard). |
| **Empty Air** | Major | 3 | A perfect dodge gives back its stamina and leaves the one who struck three deeper in Unsteady. | Happens when you dodge at the last moment (a perfect dodge). |
| **Open Air** | Minor | 1 | Blows strike an enemy in the air harder. | Always on once it is yours. |
| **Fair Wind** | Minor | 1 | Tailwind costs less mana. | Always on once it is yours. |
| **Strong Arm** | Minor | 1 | Your pushes throw enemies further. | Always on once it is yours. |
| **Lifting Wind** | Spell | 4 | Hold the spell button to raise a column of wind under the enemy faced. It goes up whatever its footing. 30 mana. | Hold Triangle (U) to cast. 30 mana. |
| **Running Start** | Minor | 1 | You move faster while Tailwind lasts. | Always on once it is yours. |
| **Drawing Wind** | Major | 3 | The Iaido Slash draws breath first. Everything before you is pulled onto the cut. | Happens when the weapon skill lands. |
| **Closing Circle** | Spell | 4 | Hold the spell button to run a ring round the enemy faced. When the ring closes, the wind inside it rises and takes everything off the ground. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Indrawn Breath** | Spell | 4 | Hold the spell button to drag everything near into one place before you, then burst it apart. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Sky Dance** | Spell | 4 | With an enemy in the air, hold the spell button to go up after it. Strike in time, up to five, then send it down where you choose. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Leaf on the Wind** | Keystone | 6 | The dodge costs no stamina. It spends a breath of wind instead: you hold three, and blows win them back. You are easier to wound. | Always on once it is yours. |
| **Storm Front** | Keystone | 6 | Every heavy blow throws a Gust with it at no cost. Your blows give less mana. | Happens when a heavy blow lands. |
| **Nothing Beneath** | Keystone | 6 | An enemy in the air stays there for as long as you keep striking it, and each blow up there gives double mana. Your blows on an enemy that is not Unsteady are weaker. | Always on once it is yours. |
| **The Wind Remembers** | Signature | gift | While Tailwind lasts, every blow you land hangs in the air where it fell, up to sixteen. When Tailwind ends, all of them fall again at once, each half as hard as the blow it remembers. | Happens when you light the surge (press L2 (I)). |

#### Ice (31)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Chill** | Root | 2 | Blows chill an enemy, up to five. Each slows it. At five it is Frozen, and a heavy blow shatters it. | Always on once it is yours. |
| **Black Ice** | Major | 3 | Hoarfrost leaves a sheet of ice on the ground. Enemies standing on it grow colder and are pushed further. | Happens when you cast a spell (Triangle (U)). |
| **Thin Ice** | Minor | 1 | Hoarfrost costs less mana. | Always on once it is yours. |
| **Long Frost** | Minor | 1 | Hoarfrost reaches further. | Always on once it is yours. |
| **Cold Read** | Major | 3 | A perfect guard chills the one who struck, two deep. | Happens when you guard at the last moment (a perfect guard). |
| **Splinter Ward** | Major | 3 | When Glaze breaks, it bursts on the one who broke it. The blow counts as a perfect guard. | Happens when a ward turns a blow. |
| **Patient Guard** | Minor | 1 | The moment for a perfect guard is a little longer. | Always on once it is yours. |
| **Bitter Cold** | Minor | 1 | Spells strike chilled enemies harder. | Always on once it is yours. |
| **Braced** | Minor | 1 | Guarding costs less stamina. | Always on once it is yours. |
| **Thick Glaze** | Minor | 1 | Glaze lasts longer. | Always on once it is yours. |
| **Flying Shards** | Major | 3 | A shattered enemy throws its ice wide. Everyone it reaches is chilled two deep. | Happens when a frozen enemy is shattered. |
| **Clear Ice** | Minor | 1 | Glaze costs less mana. | Always on once it is yours. |
| **Winter Wall** | Spell | 4 | Hold the spell button to raise a wall of ice before you. It stops blows and chills what touches it. Your heavy blow bursts it forward. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Gathering Frost** | Major | 3 | Hold your ground without striking and frost gathers at your feet, up to three rings. Your next blow spends them, harder and colder for each. | Happens when you block a blow. |
| **Cold Iron** | Minor | 1 | Heavy blows chill two deep. | Happens when a heavy blow lands. |
| **Long Winter** | Minor | 1 | Chill lasts longer. | Always on once it is yours. |
| **Winter Draw** | Major | 3 | The Iaido Slash shatters a Frozen enemy and carries on through everyone behind it. | Happens when the weapon skill lands. |
| **Deep Cold** | Minor | 1 | Chill slows more. | Always on once it is yours. |
| **Slow Pulse** | Minor | 1 | You take less harm while your guard is up. | Always on once it is yours. |
| **Heavy Snow** | Minor | 1 | Heavy blows break balance harder. | Always on once it is yours. |
| **Second Skin** | Keystone | 6 | Glaze returns by itself a while after it breaks, at no cost. The surge button breaks it by choice. You have less health. | Happens when an enemy is near, every few seconds. |
| **Brittle** | Major | 3 | An enemy that thaws or is shattered stays brittle for a while. Blows on it cut deeper and break its balance sooner. | Happens when a frozen enemy is shattered. |
| **Hollow Statue** | Spell | 4 | Hold the spell button to step back and leave a shape of ice where you stood. Struck, it bursts and chills. Strike it yourself with a heavy blow and it bursts forward, freezing. 30 mana. | Hold Triangle (U) to cast. 30 mana. |
| **Clean Break** | Major | 3 | Shatter an enemy with little life left and nothing is left of it. | Happens when a heavy blow lands. |
| **Hanging Ice** | Spell | 4 | Hold the spell button to hang a great blade of ice over the enemy faced. It falls in a few moments. If the enemy is Frozen when it lands, it is shattered three times over. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Held Fast** | Minor | 1 | Frozen enemies stay Frozen longer. | Always on once it is yours. |
| **Sharp Break** | Minor | 1 | Shattering strikes harder. | Always on once it is yours. |
| **Unmoved** | Keystone | 6 | Nothing breaks your heavy blows, and a blow that lands on you while you swing does half. Your dodge is short and costs more. | Always on once it is yours. |
| **Eight Points** | Spell | 4 | Hold the spell button to set eight shards in a ring round the enemy faced. They close on it together. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **All at Once** | Keystone | 6 | Shattering strikes three times as hard. Chill no longer slows anyone. | Happens when a heavy blow lands. |
| **The World Holds Still** | Signature | gift | With Glaze standing, press the surge button again to break it outward. Everything it crosses is Frozen where it stands. Shatter one and its ice shatters the next. | Happens when you light the surge (press L2 (I)). |

#### Water (31)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Soaked** | Root | 2 | Blows leave an enemy Soaked. Spells strike it harder, and Static or Chill laid on it spreads to others who are Soaked. | Always on once it is yours. |
| **Spilled Water** | Major | 3 | Drinking a flask throws a ring of water that pushes back and soaks everyone near. | Happens when you drink a flask. |
| **Earned Water** | Minor | 1 | An empty gourd is won back with fewer blows. | Always on once it is yours. |
| **Clear Water** | Minor | 1 | Flasks mend more. | Always on once it is yours. |
| **Undertow** | Major | 3 | A heavy blow drags every Soaked enemy near a step toward you. | Happens when a heavy blow lands. |
| **River Serpent** | Spell | 4 | Hold the spell button to send a serpent of water out and back. It brings the first enemy it takes to your feet, Soaked and open. 30 mana. | Hold Triangle (U) to cast. 30 mana. |
| **Ebb and Flood** | Major | 3 | A perfect guard brings the Flood: for a while your blows reach further and splash. A heavy blow that lands brings the Ebb: for a while your guard costs little and its moment is longer. | Happens when you guard at the last moment (a perfect guard). |
| **Broken Gourd** | Major | 3 | Hold the flask button to throw it. Where it breaks it leaves a pool, and you mend while you stand in it. | Happens when you throw a flask (hold the flask button). |
| **Low Water** | Minor | 1 | Riptide costs less mana. | Always on once it is yours. |
| **Still Pool** | Minor | 1 | The pool from a broken gourd lasts longer. | Always on once it is yours. |
| **Long Riptide** | Minor | 1 | Riptide reaches further. | Always on once it is yours. |
| **Wading** | Minor | 1 | You move faster with your guard up. | Always on once it is yours. |
| **Easy Water** | Minor | 1 | Your blows cost less stamina. | Always on once it is yours. |
| **Heavy Water** | Major | 3 | Soaked enemies are slow to recover after they strike. | Happens when you strike an enemy that already carries an affliction. |
| **Catch the Rain** | Major | 3 | A perfect guard fills a little of an empty gourd. | Happens when you guard at the last moment (a perfect guard). |
| **Weight of Water** | Minor | 1 | Spells strike Soaked enemies harder still. | Always on once it is yours. |
| **Flowing Guard** | Major | 3 | After a perfect guard, your next light blow is the last of its chain. | Happens when you guard at the last moment (a perfect guard). |
| **Long Draught** | Minor | 1 | Wellspring lasts longer. | Always on once it is yours. |
| **Long Flood** | Minor | 1 | The Flood and the Ebb last longer. | Always on once it is yours. |
| **Soaked Through** | Minor | 1 | Soaked lasts longer. | Always on once it is yours. |
| **Second Gourd** | Minor | 1 | You carry one more flask. | Always on once it is yours. |
| **Whirlpool** | Spell | 4 | Hold the spell button to open a whirlpool where the enemy faced stands. It drags everything near to its middle and holds them Soaked. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Grey Tide** | Minor | 1 | Blows win back grey health faster. | Always on once it is yours. |
| **Poured Blade** | Spell | 4 | Hold the spell button to pour a flask along your weapon. For a while it reaches further, every blow mends you a little, and the last blow of a chain throws a wave. One flask and 20 mana. | Hold Triangle (U) to cast. 20 mana. |
| **Unbroken Chain** | Major | 3 | A dodge or a guard no longer ends a chain. The next blow carries on from where it left off. | Happens when a dodge ends. |
| **Standing Rain** | Spell | 4 | Hold the spell button to call rain over the ground you stand on. All in it stay Soaked, and your blows there give more mana. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Spring Tide** | Keystone | 6 | Every spell spends all the mana you hold, and is as great as what it spends. | Always on once it is yours. |
| **Deep Well** | Keystone | 6 | You carry one more flask. A flask no longer mends you at once: it fills a well that mends you only while you are landing blows. | Happens when any weapon blow lands. |
| **Moon on Water** | Spell | 4 | Hold the spell button to stand open and turn every blow that comes, paying mana for each. Let go to give back all you turned in one cut. 10 mana to begin. | Hold Triangle (U) to cast. 10 mana. |
| **River Without End** | Keystone | 6 | The light chain never ends: its last blow runs into its first, quicker each time round. Each blow strikes a little softer. | Always on once it is yours. |
| **The Sea Rises** | Signature | gift | While Wellspring lasts, press the surge button again to flood the ground. One flask. For six seconds everything in the flood is Soaked and your blows run out across the water as waves. Then the sea draws back to you and breaks where you face. | Happens when you light the surge (press L2 (I)). |

#### Life (31)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Seeded** | Root | 2 | Blows plant a seed in an enemy. Each blow on a Seeded enemy mends you a little. When it dies, it bursts into thorns. | Always on once it is yours. |
| **Paid in Blood** | Major | 3 | A spell you cannot pay for in mana is paid for in health. | Always on once it is yours. |
| **Deep Roots** | Minor | 1 | You have more health. | Always on once it is yours. |
| **Ripening** | Major | 3 | Each blow ripens the seed in an enemy, up to five. A heavy blow on a ripe one bursts it without waiting for death. | Happens when a heavy blow lands. |
| **Wide Burst** | Minor | 1 | Thorns burst wider. | Always on once it is yours. |
| **Sweet Sap** | Minor | 1 | Blows on Seeded enemies mend more. | Always on once it is yours. |
| **Fallen Seeds** | Major | 3 | Where a Seeded enemy dies, a thornbush grows and stands for a while. | Happens when an enemy dies. |
| **Deep Seed** | Minor | 1 | Seeded lasts longer. | Always on once it is yours. |
| **Thin Blood** | Major | 3 | Below a third of your health, your blows strike harder and mend twice as much. | Always on once it is yours. |
| **Sharp Thorns** | Minor | 1 | What you grow strikes harder. | Always on once it is yours. |
| **Stubborn** | Minor | 1 | Below a third of your health, wounds take less. | Always on once it is yours. |
| **Long Season** | Minor | 1 | What you grow stands longer. | Always on once it is yours. |
| **Briar Coat** | Major | 3 | An enemy that strikes you by hand is cut by thorns and Seeded. | Happens when you are struck. |
| **Quickening** | Minor | 1 | Regrowth costs less mana. | Always on once it is yours. |
| **Bark** | Minor | 1 | Wounds take less while Regrowth lasts. | Always on once it is yours. |
| **Red Harvest** | Spell | 4 | Hold the spell button to give a quarter of your health to the ground. For a while every blow is critical and mends you. Win back more than you gave and the ground bursts into thorns. No mana. | Hold Triangle (U) to cast. |
| **Long Regrowth** | Minor | 1 | Regrowth lasts longer. | Always on once it is yours. |
| **Heartwood** | Keystone | 6 | You have no mana. Spells and skills are paid for in health, and everything that gave mana mends you instead. | Happens when any weapon blow lands. |
| **Green Wood** | Minor | 1 | Grey health waits longer before it fades. | Always on once it is yours. |
| **Sapling** | Major | 3 | A perfect guard plants a sapling at your feet, up to three. Each lashes the nearest enemy and seeds it. | Happens when you guard at the last moment (a perfect guard). |
| **Bitter Rind** | Major | 3 | A plain guard seeds the attacker. A perfect guard roots it where it stands for a moment. | Happens when you block a blow. |
| **Ready Soil** | Minor | 1 | Briar Patch costs less mana. | Always on once it is yours. |
| **Crowded Bed** | Minor | 1 | You may keep one more sapling. | Always on once it is yours. |
| **Shade** | Major | 3 | Standing near anything you have grown, you mend slowly and your stamina returns faster. | Happens when an enemy is near, every few seconds. |
| **Spreading Briar** | Minor | 1 | Briar Patch covers more ground. | Always on once it is yours. |
| **Thorn Ring** | Spell | 4 | Hold the spell button to raise a ring of thorns round you and the enemy faced. None leave and none enter until it falls. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Reap What Is Sown** | Spell | 4 | Hold the spell button to call in the harvest. Everything you have grown bursts at once, every seed in every enemy with it, and each burst mends you. 30 mana. | Hold Triangle (U) to cast. 30 mana. |
| **Elder Tree** | Spell | 4 | Hold the spell button to plant a tree. It takes a few moments to grow and can be cut down while it does. Grown, it seeds all beneath it, mends you in its shade and strikes the staggered with its roots. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Shared Roots** | Keystone | 6 | A third of every wound you take is carried by the Seeded enemies near you instead. Blows on Seeded enemies no longer mend you. | Happens when you are struck. |
| **Wildwood** | Keystone | 6 | What you grow does not wither while the fight lasts, and you may keep twice as many. Far from your garden, your blows are weaker. | Always on once it is yours. |
| **First Spring** | Signature | gift | While Regrowth lasts, press the surge button again and give half your health to the ground. A forest comes up through everything Seeded and holds it. Strike what it holds and the health returns. | Happens when you light the surge (press L2 (I)). |

#### Earth (31)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Tremor** | Root | 2 | Heavy blows and finishers shake an enemy's footing, up to three deep. A shaken enemy is slow, and its balance does not come back. | Happens when a heavy blow lands. |
| **Rooted** | Major | 3 | Fight without rolling for a few breaths and you are Rooted. Rooted, nothing pushes you back and your guard costs less. A roll ends it. | Always on once it is yours. |
| **Standing Stones** | Major | 3 | Two pillars may stand at once. An enemy driven into a pillar breaks it and is hurt. | Happens when a spell of yours strikes an enemy. |
| **Heavy Hand** | Major | 3 | Your heavy blows are not stopped by being struck. You take less from that blow, and yours still lands. | Happens when a heavy blow lands. |
| **Near to Hand** | Minor | 1 | Stone Rise costs less mana. | Always on once it is yours. |
| **Thick Hide** | Minor | 1 | Rooted, you take less from every blow. | Always on once it is yours. |
| **Deep Footing** | Minor | 1 | You become Rooted sooner. | Always on once it is yours. |
| **Hard Stone** | Minor | 1 | Pillars and their shards strike harder. | Always on once it is yours. |
| **Stone Skin** | Major | 3 | A perfect guard grows a plate of stone on you, up to three. A plate takes most of one blow, and breaks. | Happens when you guard at the last moment (a perfect guard). |
| **Toppling** | Major | 3 | A perfect guard beside one of your pillars topples it onto the one who struck. | Happens when you guard at the last moment (a perfect guard). |
| **Landslide** | Major | 3 | Each blow in a chain lands harder than the one before. The heavy blow that ends a chain shakes the ground round its target. | Happens when a light blow lands. |
| **Breaking Weight** | Minor | 1 | Heavy blows break balance sooner. | Always on once it is yours. |
| **Full Swing** | Minor | 1 | Heavy blows and finishers cut deeper. | Always on once it is yours. |
| **Far Roots** | Minor | 1 | Stone rises further from you, and its shards fly further. | Always on once it is yours. |
| **Old Granite** | Minor | 1 | Grey health lingers longer before it fades. | Always on once it is yours. |
| **Long Tremor** | Minor | 1 | Tremor lasts longer. | Always on once it is yours. |
| **Shaken Ground** | Minor | 1 | Shaken enemies take more from your weapon. | Always on once it is yours. |
| **Aftershock** | Major | 3 | A plate that breaks bursts outward and shakes everyone near. | Happens when a ward turns a blow. |
| **Fissure** | Spell | 4 | Hold the spell button to drive your weapon into the ground. A crack runs ahead, waits a breath, and breaks open in a row of spikes that hold what they catch. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Steady Arm** | Minor | 1 | Holding your guard costs less stamina. | Always on once it is yours. |
| **Quake Step** | Spell | 4 | Hold the spell button to raise a foot and bring it down. The ground heaves all round you, and an enemy caught in the middle of its own attack loses its footing. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Deep Bedrock** | Minor | 1 | Bedrock lasts longer. | Always on once it is yours. |
| **Old Stones** | Minor | 1 | Pillars, spikes and walls stand longer. | Always on once it is yours. |
| **Easy Burden** | Minor | 1 | Heavy blows cost less stamina. | Always on once it is yours. |
| **Mountain Stance** | Spell | 4 | Hold the spell button to stand as stone. The first blow to land on you is turned, and a fist of rock rises under the one who struck. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Rampart** | Spell | 4 | Hold the spell button to raise a wall before you. Strike it with a heavy blow and it falls forward on whatever stands behind it. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Mountain Cleaver** | Spell | 4 | Hold the spell button and stone gathers on your weapon, in three stages. Let go to bring it down. While you hold you can be hurt but not stopped. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **The Closed Ring** | Keystone | 6 | Bedrock raises a ring of stone round you and the enemy you face. Nothing enters and nothing leaves while it stands, you least of all. | Happens when you light the surge (press L2 (I)). |
| **Immovable** | Keystone | 6 | You no longer roll. The dodge button braces you where you stand, and a brace timed well turns any blow, the red ones too. You are always Rooted. | Always on once it is yours. |
| **The Slow Hand** | Keystone | 6 | Every swing is slower and costs more stamina. Every swing lands as a heavy blow. | Always on once it is yours. |
| **Hand of the Mountain** | Signature | gift | While Bedrock lasts, a hand of stone as tall as a tower follows your arm. Each heavy blow you land, it lands again a moment after. | Happens when you light the surge (press L2 (I)). |

#### Metal (30)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Sunder** | Root | 2 | Heavy blows and perfect guards notch an enemy's armour, up to three. Each notch lets your weapon bite deeper. At three, its guard turns nothing. | Happens when a heavy blow lands. |
| **Forged Answer** | Major | 3 | A perfect guard forges a blade from the blow it turned. Up to three hang at your shoulder. Your next heavy blow sends them all. | Happens when you guard at the last moment (a perfect guard). |
| **Buried Blades** | Major | 3 | Up to three of your thrown blades may stand in one enemy. A heavy blow drives in every one at once. | Happens when a heavy blow lands. |
| **Counter Cut** | Major | 3 | A blow that lands while an enemy is showing its own attack is critical. | Always on once it is yours. |
| **Ready Steel** | Minor | 1 | Flung Blade costs less mana. | Always on once it is yours. |
| **Bright Sparks** | Minor | 1 | Perfect guards give more mana. | Always on once it is yours. |
| **Practised Wrist** | Minor | 1 | The moment for a perfect guard is a little longer. | Always on once it is yours. |
| **True Flight** | Minor | 1 | Conjured blades strike harder. | Always on once it is yours. |
| **Open Line** | Major | 3 | A perfect guard throws the attacker's weapon wide. For a moment it cannot guard, and cannot turn to face you. | Happens when you guard at the last moment (a perfect guard). |
| **Ricochet** | Major | 3 | A blade that strikes, or is driven in, springs on to the next enemy near, and then the next. | Happens when a spell of yours strikes an enemy. |
| **Splinter** | Major | 3 | A blow on armour notched three deep throws its splinters into whoever stands behind. | Happens when any weapon blow lands. |
| **Finishing Edge** | Minor | 1 | The last blow of a chain cuts deeper. | Always on once it is yours. |
| **Deep Bite** | Minor | 1 | Each notch lets your weapon bite deeper still. | Always on once it is yours. |
| **Braced Frame** | Minor | 1 | A blow held on your guard lets less through. | Always on once it is yours. |
| **Long Throw** | Minor | 1 | Blades fly further and spring further. | Always on once it is yours. |
| **Hard Return** | Minor | 1 | A perfect guard breaks more of the attacker's balance. | Always on once it is yours. |
| **Clean Edge** | Minor | 1 | Critical blows cut deeper. | Always on once it is yours. |
| **Circle of Steel** | Spell | 4 | Hold the spell button and your blade turns all round you. For a breath, every blow from any side is met as a perfect guard. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Returning Edge** | Spell | 4 | Hold the spell button to throw a great turning blade out and back. Guard at the instant it returns and it goes out again, faster and harder, up to three times. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Shatter Point** | Major | 3 | A finisher on notched armour breaks it for good. The notches never mend, and the finisher strikes harder for each. | Happens when a finisher lands (heavy on a staggered enemy). |
| **Paid in Blows** | Minor | 1 | Blows on notched armour give more mana. | Always on once it is yours. |
| **Long Temper** | Minor | 1 | Temper lasts longer. | Always on once it is yours. |
| **Slow Mending** | Minor | 1 | Notched armour takes longer to mend. | Always on once it is yours. |
| **Edge Guard** | Keystone | 6 | Your guard no longer holds a blow: only the perfect guard turns one. Its moment is longer, it may be pressed again at once, and each one cuts the attacker as your weapon would. | Happens when you guard at the last moment (a perfect guard). |
| **Grave of Swords** | Spell | 4 | Hold the spell button and twelve blades fall in a ring round you and stand in the ground. Each blow you land calls one through its target. A heavy blow calls all that remain. 50 mana. | Hold Triangle (U) to cast. 50 mana. |
| **Hone** | Spell | 4 | Hold the spell button to draw a stone down the blade, in the open. Your next five blows cannot be turned, and each one notches. A blow taken while honing wastes it. 30 mana. | Hold Triangle (U) to cast. 30 mana. |
| **All Blades Home** | Keystone | 6 | Blades you throw or plant no longer fade. Beginning Temper calls every one of them back to you, through whatever stands between. Temper costs more. | Happens when you light the surge (press L2 (I)). |
| **Master's Answer** | Keystone | 6 | After a perfect guard, your weapon's skill is free and comes at once, with no time to charge. At any other time it costs more. | Happens when you guard at the last moment (a perfect guard). |
| **Balanced Blade** | Minor | 1 | Swings cost less stamina. | Always on once it is yours. |
| **Hanging Swords** | Signature | gift | While Temper lasts, every blow you hold on your guard is a perfect guard, and a sword is forged above the one who struck, up to eight. When Temper ends, they fall. | Happens when you light the surge (press L2 (I)). |

#### Light (31)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Marked** | Root | 2 | The last blow of a chain and every perfect guard leave a mark of light. The next three blows on a Marked enemy are critical and give more mana. | Happens when the last blow of a chain lands. |
| **Brimming** | Major | 3 | While your mana is full, every heavy blow throws a wave of light ahead of it. | Happens when a heavy blow lands. |
| **Just Cut** | Major | 3 | Press the next blow of a chain at the instant the last one lands and it flashes. A flashing blow is critical and costs no stamina. | Happens when any weapon blow lands. |
| **Measured Guard** | Minor | 1 | The moment for a perfect guard is a little longer. | Always on once it is yours. |
| **Weighed** | Major | 3 | Marks deepen, up to three. Each one makes your critical blows strike harder. | Happens when an affliction on an enemy reaches its full depth. |
| **Hard Light** | Minor | 1 | Spells of light strike harder. | Always on once it is yours. |
| **Lasting Mark** | Minor | 1 | A mark gives one more critical blow before it is spent. | Always on once it is yours. |
| **Gleaning** | Minor | 1 | Every blow of the weapon gives a little more mana. | Always on once it is yours. |
| **Sharp Light** | Minor | 1 | Critical blows strike harder. | Always on once it is yours. |
| **Plain Guilt** | Minor | 1 | Marked enemies take more from your weapon. | Always on once it is yours. |
| **Returning Light** | Major | 3 | A spell that strikes a Marked enemy gives back part of what it cost. | Happens when a spell of yours strikes an enemy. |
| **Dazzle** | Major | 3 | A perfect dodge flashes in the attacker's eyes. Blinded, it cannot turn to follow you, and it is Marked. | Happens when you dodge at the last moment (a perfect dodge). |
| **Sentence** | Major | 3 | A heavy blow on an enemy marked three deep passes sentence. The marks are spent, and a blade of light falls on it a moment later. | Happens when a heavy blow lands. |
| **Bright Evasion** | Minor | 1 | Perfect dodges give more mana. | Always on once it is yours. |
| **Bowed Head** | Minor | 1 | Critical blows break balance sooner. | Always on once it is yours. |
| **Long Rays** | Minor | 1 | Waves, rays and lanterns reach further. | Always on once it is yours. |
| **Narrow Escape** | Minor | 1 | The moment for a perfect dodge is a little longer. | Always on once it is yours. |
| **Standing Light** | Spell | 4 | Hold the spell button to plant a lantern of light. Enemies in its glow are Marked again and again, and a perfect guard inside it flashes and staggers all near you. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Rich Mark** | Minor | 1 | Blows on Marked enemies give still more mana. | Always on once it is yours. |
| **Step Into Light** | Spell | 4 | Hold the spell button to vanish and stand behind the enemy you face. Do it as its blow is about to land, and your next blow strikes as a finisher does. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Ring of Spears** | Spell | 4 | Hold the spell button. Six spears of light stand in the air round the enemy you face, wait a breath, and drive in together. It is held where it stands. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Ready Light** | Minor | 1 | Glare costs less mana. | Always on once it is yours. |
| **Long Clarity** | Minor | 1 | Clarity lasts longer. | Always on once it is yours. |
| **Daybreak** | Spell | 4 | Hold the spell button and the blade begins to gather light. Every blow you land adds to it, up to eight. Hold again to loose it all down one line. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Unblemished** | Keystone | 6 | Fight a while without a wound and every blow you land is critical. A wound ends it, and until it returns you take more from every blow. | Always on once it is yours. |
| **What It Owes** | Major | 3 | Light keeps count of every wound an enemy gives you. Sentence pays them back. | Happens when you are struck. |
| **Two Suns** | Keystone | 6 | When you cast a spell, a figure of light casts it again from the far side of your enemy. Spells cost more. | Happens when a spell of yours strikes an enemy. |
| **Reckoning** | Spell | 4 | Hold the spell button to set a seal on the enemy you face. A hand turns once round it. When it comes round, half of all your weapon did to it in that time is done again at once. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Heavy Verdict** | Minor | 1 | Blades and seals of judgement strike harder. | Always on once it is yours. |
| **Condemned** | Keystone | 6 | An enemy marked three deep and near death is Condemned: it may be finished whatever its balance. Your weapon deals less to enemies that bear no mark. | Happens when an affliction on an enemy reaches its full depth. |
| **The Scales** | Signature | gift | While Clarity lasts, the wounds you deal and the wounds you take are weighed. If yours are the heavier when it ends, a sword of light falls on every Marked enemy. If not, the light leaves you, and your mana with it. | Happens when you light the surge (press L2 (I)). |

#### Dark (31)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Curse** | Root | 2 | Blows of the weapon curse an enemy. A Cursed enemy deals less, and killing it heals you. | Always on once it is yours. |
| **Blood Price** | Major | 3 | With too little mana for a spell, the rest is paid in health. Health paid this way is only owed: blows win it back. | Always on once it is yours. |
| **Near the Grave** | Major | 3 | Below a third of your health you strike harder, and each kill wraps you in a shade that takes the next blow. | Happens when an enemy dies. |
| **Feast** | Major | 3 | A finisher on a Cursed enemy feeds you. It heals, and Hunger lasts a while longer. | Happens when a finisher lands (heavy on a staggered enemy). |
| **Hard to Bury** | Minor | 1 | Below a third of your health you take less from every blow. | Always on once it is yours. |
| **Small Cuts** | Minor | 1 | Prices paid in health are smaller. | Always on once it is yours. |
| **Slow Collector** | Minor | 1 | Health that is owed lingers longer before it is lost. | Always on once it is yours. |
| **Long Curse** | Minor | 1 | Curse lasts longer. | Always on once it is yours. |
| **Borrowed Strength** | Major | 3 | The more of your health is owed, the harder you strike. | Always on once it is yours. |
| **Hollow Strength** | Major | 3 | Risen, you strike harder and no blow makes you flinch. Each kill stills the draining for a few breaths. | Happens when an enemy dies. |
| **Glut** | Major | 3 | Healing beyond a full bar is kept as a second, dark measure of health that slowly drains away. | Happens when you drink a flask. |
| **Heavy Curse** | Minor | 1 | Cursed enemies deal less still. | Always on once it is yours. |
| **Grave Appetite** | Minor | 1 | Risen, your blows feed you more. | Happens when any weapon blow lands. |
| **Reckless Arm** | Minor | 1 | While any health is owed, swings cost less stamina. | Always on once it is yours. |
| **Fat of the Kill** | Minor | 1 | Killing a Cursed enemy heals more. | Happens when an enemy dies. |
| **Slow Rot** | Minor | 1 | Risen, your health drains more slowly. | Always on once it is yours. |
| **More to Spend** | Minor | 1 | You have more health to spend. | Always on once it is yours. |
| **Debts Called** | Spell | 4 | Hold the spell button. Everything near is dragged to you, and then thrown back by a burst that is greater the less health you have. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Second Shadow** | Spell | 4 | Hold the spell button to leave your shadow where you stand. It strikes when you strike, at whatever is near it. Hold again to change places with it. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Dread** | Major | 3 | When a Cursed enemy dies, the other Cursed lose heart. Their balance buckles and their guard falls for a moment. | Happens when an enemy dies. |
| **Long Shadows** | Minor | 1 | Shades and shadows strike harder. | Always on once it is yours. |
| **Bitter Harvest** | Minor | 1 | Below half your health, blows give more mana. | Always on once it is yours. |
| **Easy Prey** | Minor | 1 | Cursed enemies take more from your weapon. | Always on once it is yours. |
| **Pawned Heart** | Spell | 4 | Hold the spell button to tear out your heart and set it down. While it beats there you stand one blow from death, and strike as you never have. Come back for it in time and it returns with what your blows have earned. No mana. | Hold Triangle (U) to cast. |
| **The Dead Stand** | Spell | 4 | Hold the spell button and those you have killed in this fight stand up where they fell, up to three. Each strikes a few times for you and is gone. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Devouring Maw** | Spell | 4 | Hold the spell button and a great jaw rises out of your shadow and bites what stands before you. If the bite kills, or its prey is staggered, it swallows, and you are fed. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Unpaid** | Keystone | 6 | Every wound is only owed. All of it can be won back with blows before it is collected. Flasks heal half as much. | Happens when any weapon blow lands. |
| **Long Hunger** | Minor | 1 | Hunger lasts longer and takes no more for it. | Always on once it is yours. |
| **Dead Already** | Keystone | 6 | You do not wait to die. You are Risen from now on: in every fight your health drains, and dealing damage feeds you. You strike far harder. There is no second rising. | Always on once it is yours. |
| **Never Sated** | Keystone | 6 | You carry no flasks. Every blow you land feeds you a little, and every kill and finisher a great deal. | Happens when an enemy dies. |
| **A Life for a Life** | Signature | gift | The blow that would kill you does not, once. You rise with half your health, and it drains away. A sigil marks the one that killed you: land a finisher on it and you live again, and may rise once more. | Always on once it is yours. |

#### Poison (31)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Venom** | Root | 2 | Blows of the weapon poison, up to eight deep. On a slow, steady beat the poison bites for all its depth at once. | Happens when a heavy blow lands. |
| **In Time** | Major | 3 | A blow that lands as the poison bites is critical and doses twice. | Happens when any weapon blow lands. |
| **Catching** | Major | 3 | Each time poison bites, one dose leaps to the nearest enemy within reach. | Happens when you strike an enemy that already carries an affliction. |
| **Rupture** | Major | 3 | A heavy blow on an enemy poisoned to its full depth bursts the poison. All of it lands at once, and the depth is spent. | Happens when a heavy blow lands. |
| **Wide Cloud** | Minor | 1 | Clouds of poison are larger. | Always on once it is yours. |
| **Bitter Dose** | Minor | 1 | Each dose bites harder. | Always on once it is yours. |
| **Still Air** | Minor | 1 | Clouds of poison hang longer. | Always on once it is yours. |
| **Deeper Dose** | Minor | 1 | Poison goes two doses deeper. | Always on once it is yours. |
| **Left to Ripen** | Major | 3 | Poison you leave alone ripens. Each bite that comes without a fresh blow from you is stronger than the last. | Happens when an enemy is near, every few seconds. |
| **One Sickness** | Major | 3 | The poison in every enemy bites on the same beat. Each poisoned enemy beyond the first makes every bite stronger. | Happens when an affliction on an enemy reaches its full depth. |
| **Rotten Fruit** | Major | 3 | A poisoned enemy that dies bursts. Those the burst reaches take its depth. | Happens when an enemy dies. |
| **Ripe** | Minor | 1 | Bursting poison deals more. | Always on once it is yours. |
| **Wider Rupture** | Minor | 1 | Bursts of poison reach further. | Always on once it is yours. |
| **Long Leap** | Minor | 1 | Poison leaps further between enemies. | Always on once it is yours. |
| **Lingering** | Minor | 1 | Poison stays longer before it fades. | Always on once it is yours. |
| **Buckling** | Minor | 1 | Bursting poison breaks balance sooner. | Always on once it is yours. |
| **Lance the Wound** | Major | 3 | A perfect guard makes the attacker's poison bite at once, and with it the poison in everyone near. | Happens when you guard at the last moment (a perfect guard). |
| **Swinging Censer** | Spell | 4 | Hold the spell button to set a smoking censer swinging round you on its chain. It doses whatever it passes and leaves its smoke behind. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Blister** | Spell | 4 | Hold the spell button to raise a blister on the enemy you face. It swells each time its poison bites, through three sizes. Break it with a heavy blow. Wait too long and it withers. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Ready Poison** | Minor | 1 | Miasma costs less mana. | Always on once it is yours. |
| **Calm Pulse** | Minor | 1 | Holding your guard costs less stamina. | Always on once it is yours. |
| **Waiting Game** | Minor | 1 | Perfect guards give more mana. | Always on once it is yours. |
| **Spent Venom** | Minor | 1 | Blows on poisoned enemies give more mana. | Always on once it is yours. |
| **Coiled Strike** | Spell | 4 | Hold the spell button to coil, for as long as you dare. Let go to strike through the enemy you face, deeper for every breath you waited. Let go as a blow is about to land and it passes through you. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Share Alike** | Spell | 4 | Hold the spell button to rush through everything before you. Every enemy you pass is poisoned as deeply as the sickest among them. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **One by One** | Spell | 4 | Hold the spell button and raise your hand. At the snap of your fingers every poisoned enemy bursts in turn, nearest first, each passing its depth on to the next. 50 mana. | Hold Triangle (U) to cast. 50 mana. |
| **Sweet Fever** | Minor | 1 | Fever returns more of what the poison takes. | Always on once it is yours. |
| **Slow Death** | Keystone | 6 | Your weapon wounds far less. Your poison has no limit to its depth, and it does not fade while its bearer lives. | Always on once it is yours. |
| **Plague Bearer** | Keystone | 6 | You carry the sickness. Enemies near you are poisoned without a blow, deeper the longer they stay. Your flasks heal less. | Happens when an enemy is near, every few seconds. |
| **Volatile** | Keystone | 6 | Your poison no longer bites on its beat. It builds until it is full, and then bursts by itself and begins again. | Happens when an affliction on an enemy reaches its full depth. |
| **Clean Blade** | Signature | gift | While Fever lasts the poison waits, and every blow doses twice. When it ends you flick the blade clean, and every bite that was owed lands at once. | Happens when you light the surge (press L2 (I)). |

#### Plasma (Fire + Lightning) (10)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Charged Embers** | Mix | 3 | Your blows lay Burn and Static together. | Happens when any weapon blow lands. |
| **Arc Fire** | Mix | 3 | A discharge of Static carries Burn to everyone it reaches. A burn set off leaves Static on everyone it reaches. | Happens when you set off an affliction. |
| **Twin Break** | Mix | 4 | A perfect guard sets off Burn and Static together on the one who struck. Set off together, they strike half again as hard and stop it where it stands. | Happens when you guard at the last moment (a perfect guard). |
| **Ball Lightning** | Mix | 4 | Hold the spell button to loose a slow ball of white fire. Strike it to send it where the blow points, larger each time, up to three. It bursts on the first enemy it meets. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Chain Reaction** | Mix | 4 | An enemy that dies carrying both Burn and Static bursts, and passes both on at full depth. | Happens when an enemy dies. |
| **Ionised Air** | Mix | 3 | When an affliction on an enemy reaches full depth, an arc leaps from it through three more. | Happens when an affliction on an enemy reaches its full depth. |
| **Meltdown** | Mix | 5 | An enemy at five Burn and five Static begins to shake with light. A few moments later it bursts, far harder than either alone. Nothing stops it. | Happens when an affliction on an enemy reaches its full depth. |
| **Fused Core** | Mix | 4 | Spells hit harder, and leave both a burn and Static on what they strike. | Happens when a spell of yours strikes an enemy. |
| **Plasma Lance** | Mix | 5 | Hold the spell button to throw a bolt of plasma that leaps through three more. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Plasma** | Signature | gift | A heavy blow or a finisher on a burning enemy bursts, and sets alight everyone near. | Always on once it is yours. |

#### Tempest (Lightning + Wind) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Charged Air** | Mix | 3 | Your blows lay Static and Unsteady together. | Happens when any weapon blow lands. |
| **Thunder Lift** | Mix | 4 | A discharge of five Static throws its enemy into the air. | Happens when you set off an affliction. |
| **Slip Current** | Mix | 3 | Each enemy you pass in a dodge is left one deeper in Static and Unsteady, and pays half the stamina back. | Happens when a dodge ends. |
| **The Sky Finds Them** | Mix | 3 | An enemy in the air is struck by a bolt at the top of its rise. | Happens when you throw an enemy into the air. |
| **Eye of the Storm** | Mix | 5 | Hold the spell button to become the storm: a turning column of wind and lightning that drags enemies in and carries the light ones round. Let go and it throws them out, and a bolt meets each where it lands. 50 mana. | Hold Triangle (U) to cast. 50 mana. |
| **Storm Overhead** | Mix | 4 | Hold the spell button to call a low cloud that follows you. Each dodge beneath it calls a bolt down where you stood. A perfect dodge calls three. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Falling Thunder** | Signature | gift | Your finisher changes. You throw the enemy skyward, meet it there, and ride it down as a bolt. Where you land, everything near is shocked and thrown back. | Happens when a finisher lands (heavy on a staggered enemy). |

#### Blizzard (Wind + Ice) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Biting Wind** | Mix | 3 | Your blows lay Unsteady and Chill together. | Happens when any weapon blow lands. |
| **Windchill** | Mix | 3 | An enemy both Unsteady and chilled grows colder by itself. | Happens when an enemy is near, every few seconds. |
| **Cold Front** | Mix | 3 | A perfect dodge leaves a gust of ice behind you. It throws the attacker back, two deeper in Chill and Unsteady. | Happens when you dodge at the last moment (a perfect dodge). |
| **Sliding Ice** | Mix | 4 | A light blow sends a Frozen enemy sliding until it meets something. It shatters there, and what it met is hurt as much. | Happens when a light blow lands. |
| **Whiteout** | Mix | 4 | Hold the spell button to call down driving snow around you. Enemies in it lose sight of you beyond arm's reach and turn slowly. A blow from behind is critical there. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Avalanche** | Mix | 5 | Hold the spell button to send a wall of snow rolling out before you. It carries enemies along and buries them where it stops, Frozen. 50 mana. | Hold Triangle (U) to cast. 50 mana. |
| **Diamond Dust** | Signature | gift | Shards from a shattered enemy do not fall. They hang round you, four from each, up to twelve, and your next dodge throws them down its path. | Happens when a frozen enemy is shattered. |

#### Glacier (Ice + Water) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Cold Water** | Mix | 3 | Your blows lay Chill and Soaked together. | Happens when any weapon blow lands. |
| **Flash Freeze** | Mix | 3 | A Soaked enemy freezes at three Chill, not five. | Happens when you strike an enemy that already carries an affliction. |
| **Creeping Ice** | Mix | 4 | Hold the spell button to send a line of ice along the ground. Those it reaches are held to the floor by their feet: they can strike but cannot turn or step. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Cold in the Bones** | Mix | 4 | An enemy both Soaked and chilled is slow to wind up its blows. The moment to guard them is easier to find. | Happens when you strike an enemy that already carries an affliction. |
| **Icebreaker** | Mix | 3 | Shattering an enemy gives mana and fills a little of an empty gourd. | Happens when a frozen enemy is shattered. |
| **The Wave Stands Still** | Mix | 5 | Hold the spell button to throw a great wave. Let go, and it freezes where it stands with everything in it. One heavy blow shatters the wall and all it holds. 50 mana. | Hold Triangle (U) to cast. 50 mana. |
| **Weight of Ice** | Signature | gift | Each blow you guard lays a plate of ice on you, up to five. A perfect guard lays two. Plates turn a little of every wound. Your next heavy blow spends them all in one great edge of ice. | Happens when you block a blow. |

#### Bloom (Water + Life) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Wet Seed** | Mix | 3 | Your blows lay Soaked and Seeded together. | Happens when any weapon blow lands. |
| **Flowering** | Mix | 3 | The seed in a Soaked enemy flowers after a few moments. A heavy blow on the flower bursts it: you are mended and everyone near is Seeded and Soaked. Left alone, it withers. | Happens when a heavy blow lands. |
| **Drowned Roots** | Mix | 3 | A perfect guard against an enemy both Soaked and Seeded binds it in roots for a moment. | Happens when you guard at the last moment (a perfect guard). |
| **Morning Dew** | Mix | 4 | Mending beyond full health is kept as Dew. While you hold Dew, each blow throws a blade of water with it. Dew dries slowly, and a wound takes it first. | Happens when you drink a flask. |
| **Strangling Vine** | Mix | 5 | Hold the spell button to raise a great vine under the enemy faced. It lifts and holds it, and while it holds, each blow mends you three times over. Then it throws the enemy down. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Lotus Step** | Mix | 4 | Hold the spell button, and for a while each dodge leaves a lotus where it ends, up to four. Step on one to be mended. An enemy that steps on one is cut, Soaked and Seeded. 30 mana. | Hold Triangle (U) to cast. 30 mana. |
| **Great Lotus** | Signature | gift | A flask drunk while your surge lasts opens a great lotus under you. Inside it you mend and enemies are Soaked and Seeded. Each perfect guard inside closes a petal on the attacker and holds it, up to five. When the last petal closes, or its ten seconds are done, the lotus bursts. | Happens when you drink a flask, while the surge lasts (press L2 (I)). |

#### Overgrowth (Life + Earth) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Root Grip** | Mix | 3 | Heavy blows lay both Tremor and Seed. A heavy blow on an enemy that bears both sends roots up its legs and holds it where it stands. | Happens when a heavy blow lands. |
| **Moss on Stone** | Mix | 3 | While you fight without rolling you heal slowly, and thorns grow on you that cut whoever strikes you. | Happens when an enemy is near, every few seconds. |
| **Briar Patch** | Mix | 3 | The thorns of a Seeded enemy's death take root where they fall. The patch slows what walks in it and seeds it. | Happens when an enemy dies. |
| **Thorn Wall** | Mix | 4 | Hold the spell button to raise a hedge of stone and briar before you. Enemies that touch it are cut and Seeded. A heavy blow breaks it into thorns. 35 mana. | Hold Triangle (U) to cast. 35 mana. |
| **Living Rampart** | Mix | 5 | Your guard is alive. Every blow held on it heals you a little and seeds the one who struck. A perfect guard roots the attacker where it stands. Guarding costs more stamina. | Happens when you block a blow. |
| **Stone Oak** | Mix | 5 | Hold the spell button to plant a sapling. It grows as you land blows near it, through three ages. Full grown, its roots strike what comes near and its shade heals you. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **The Ground Wakes** | Signature | gift | When your surge begins, the ground rises round you in a ring of root and stone and holds every enemy inside it. Then it flowers, and you are healed for each one held. | Happens when you light the surge (press L2 (I)). |

#### Forge (Earth + Metal) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Hammer and Anvil** | Mix | 3 | A heavy blow on an enemy with stone at its back strikes twice: once from your weapon, once from the stone. | Happens when a heavy blow lands. |
| **Struck Ore** | Mix | 3 | A perfect guard strikes a spike of ore from the ground under the attacker. It stands. A heavy blow breaks it into blades. | Happens when you guard at the last moment (a perfect guard). |
| **Buckled** | Mix | 3 | An enemy both shaken and notched loses its balance far sooner. | Happens when you strike an enemy that already carries an affliction. |
| **Trip Hammer** | Mix | 4 | Hold the spell button and a great hammer forms above the enemy you face, and falls. Land a heavy blow of your own as it lands and both strike twice as hard. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Folded Steel** | Mix | 5 | Every finisher folds your blade once more, up to ten. Each fold adds to every blow for the rest of the run. Each fold makes it heavier in the hand. | Happens when a finisher lands (heavy on a staggered enemy). |
| **Drawn From Stone** | Mix | 5 | Hold the spell button to thrust your hand into the ground and draw out a sword of stone and iron far too large for you. It gives three great swings. The third breaks it. 50 mana. | Hold Triangle (U) to cast. 50 mana. |
| **World Anvil** | Signature | gift | Your finisher is struck on an anvil that rises out of the ground. Its ring breaks the armour of everyone near and shakes them, and a blade is forged for each one it reaches, up to six. They fly with your next heavy blow. | Happens when a finisher lands (heavy on a staggered enemy). |

#### Halo (Metal + Light) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Bright Steel** | Mix | 3 | A perfect guard flashes off the blade. The attacker is Marked, and its armour is notched. | Happens when you guard at the last moment (a perfect guard). |
| **Halo of Blades** | Mix | 4 | Each critical blow forges a blade of light in a ring behind your shoulders, up to six. A perfect guard looses them all at the one who struck. | Happens when a critical blow lands. |
| **Answering Draw** | Mix | 4 | Needs the Katana. Release the Iaido Slash at the instant a blow would land. The blow is turned, and the draw cuts as light: critical, and through any guard. | Happens when the weapon skill lands. |
| **Mirror Edge** | Mix | 3 | A perfect guard against anything thrown sends it back as light. | Happens when you guard at the last moment (a perfect guard). |
| **Sun Wheels** | Mix | 4 | Hold the spell button and two wheels of bright steel turn round you. A perfect guard sends both out through the attacker and back. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Blinding Guard** | Mix | 5 | A perfect guard blinds everyone before you. They are Marked, and for a breath none of them can begin an attack. A blow held on a plain guard costs more stamina. | Happens when you guard at the last moment (a perfect guard). |
| **Six Mirrors** | Signature | gift | Hold the spell button and six mirrors of steel stand in a ring round the enemy you face. You pass through it from each one in turn, faster than can be followed. The cuts land when you are back where you began. 60 mana. | Hold Triangle (U) to cast. 60 mana. |

#### Eclipse (Light + Dark) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Twilight** | Mix | 3 | The blow that ends a chain both Marks and Curses. Critical blows on an enemy that bears both feed you. | Happens when the last blow of a chain lands. |
| **Day and Night** | Mix | 4 | A perfect guard is of the day and gives mana. A perfect dodge is of the night and gives health. Do one after the other, and the second gives double. | Happens when you dodge at the last moment (a perfect dodge). |
| **Dusk and Dawn** | Mix | 4 | Hold the spell button to cut through the enemy you face with a blade of dark. It wounds nothing yet. Land a heavy blow before the line fades and the cut of light answers along it, and both land together. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Low Sun** | Mix | 3 | The less health you have, the more mana every blow gives. | Happens when your health falls below a third. |
| **Black Sun** | Mix | 4 | Hold the spell button to hang a black sun over the enemy you face. It draws everything beneath it inward, and it grows with each blow you land in its shadow. Then it sets, and its rim breaks outward as light. 50 mana. | Hold Triangle (U) to cast. 50 mana. |
| **The Dividing Line** | Mix | 5 | Above half your health your blows are of the day: critical on the Marked, and rich in mana. Below half they are of the night: they Curse and they feed you. Each time you cross the line, it bursts. | Happens when any weapon blow lands. |
| **Totality** | Signature | gift | When your surge begins, the sun is covered. While the dark lasts every enemy is Marked and Cursed and every blow you land is critical and feeds you. When the light returns, it strikes all you wounded. | Happens when you light the surge (press L2 (I)). |

#### Blight (Dark + Poison) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Wither** | Mix | 3 | Heavy blows both Curse and poison. Poison bites harder on the Cursed, and does not fade from them. | Happens when a heavy blow lands. |
| **Feeding Rot** | Mix | 3 | Each time poison bites a Cursed enemy, a little of it comes back to you as health. | Happens when you strike an enemy that already carries an affliction. |
| **Wasting Grip** | Mix | 4 | Hold the spell button to seize the enemy before you and keep hold. While you hold, its poison bites again and again and every bite feeds you. Let go to throw it at the others. A blow from anyone breaks your grip. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Carrion Flies** | Mix | 3 | An enemy that dies Cursed and poisoned lets go a swarm. It finds the nearest living enemy and gives it everything the dead one carried. | Happens when an enemy dies. |
| **Grave Soil** | Mix | 4 | Hold the spell button to foul the ground round you. On it every enemy is Cursed and poison bites twice as often. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **One Death Feeds** | Mix | 5 | When a Cursed enemy dies, the poison in every other enemy bites at once, and you are fed by all of it. | Happens when an enemy dies. |
| **The Reaping** | Signature | gift | Your finisher on a Cursed, poisoned enemy reaps the field. All the poison in it and in every enemy near lands at once, and half of it is yours. | Happens when a finisher lands (heavy on a staggered enemy). |

#### Pyre (Poison + Fire) (7)

| Mark | Kind | Embers | What it does | How it happens |
|---|---|---|---|---|
| **Fuel** | Mix | 3 | Heavy blows both poison and burn. | Happens when a heavy blow lands. |
| **Goes Up** | Mix | 4 | When a burning enemy's poison reaches its full depth, it goes up. The poison explodes and sets alight everyone near. | Happens when an affliction on an enemy reaches its full depth. |
| **Fire Eater** | Mix | 4 | Hold the spell button to take a mouthful and breathe it out as green fire, for as long as you hold. It poisons and burns all it sweeps across. 40 mana. | Hold Triangle (U) to cast. 40 mana. |
| **Funeral Fire** | Mix | 3 | An enemy that dies poisoned and burning explodes, and leaves the ground burning where it fell. | Happens when an enemy dies. |
| **Lit Fuse** | Mix | 5 | Hold the spell button to set a fuse on the enemy you face. It burns down while you work. Every dose you lay makes the blast greater, and a perfect guard against that enemy sets it off at once, and harder. 45 mana. | Hold Triangle (U) to cast. 45 mana. |
| **Slow Burn** | Mix | 5 | Burn and poison keep one time. Both bite together on the beat, and each bites harder for the depth of the other. | Always on once it is yours. |
| **Light the Pyres** | Signature | gift | Hold the spell button and drive your weapon into the ground. A line of fire runs to every poisoned or burning enemy near. Stand through three breaths, and a pillar of flame rises under each. 60 mana. | Hold Triangle (U) to cast. 60 mana. |

---

## 8. Where to try things

### The base game (hub, castle, summit)

1. In the editor open `Assets/_Project/Scenes/Hub.unity` and press Play. The hub's door leads to the Castle, the
   Castle's sealed door (open once the Gate Captain has fallen) leads to the Summit and the Lightning god, and the
   Summit's door leads back to the hub.
2. It plays with your real saved progress: the Katana if you have drawn it, Lightning if you have taken it.
3. To play it from the very beginning without losing that progress, tick **Clover > Play > As A New Player (saves
   nothing)** before pressing Play. Untick it to get your progress back.
4. The hub also holds the new work as a stopgap: stands for every new weapon along the western rim and nine extra
   braziers along the eastern rim, all open. For the base game as designed, take only Fists or the Katana and only
   Fire or Lightning.
5. For a standalone build: **Clover > Build > Windows Playtest**. The build on disk is older than today's work until
   that is run again.

### The test yard

Open `Assets/_Project/Scenes/CombatTest.unity` and press Play. Three training dummies stand in the middle and one
that hits back stands to the west, for practising the guard and the dodge.

| Where | What | How to use it |
|---|---|---|
| North wall | A rack of all seven weapons, each named | Walk up and press use (E / Cross) to take one up. |
| South wall | 24 stones, one for each spell form that has a sample, each named | Stand by one and a card shows what it does and how to cast it. Use it to wear the spell, then hold Triangle (U). Mana comes back by itself while a stone's spell is worn. |
| In front of the stones, middle | ELEMENT stone | Use it to hold the next element round the ring. Its card says what you hold and what comes next. |
| Left of the element stone | GOD GIFT shrine | Its card names the gift of the element you hold, what it does and what sets it going. Use it to be given that gift. |
| Right of the element stone | CONSUME NEXT shrine | Its card names the crown of the branch your element shares with the next one. Use it to open that branch and be given the crown. |

To try a particular god's gift: use the ELEMENT stone until you hold its element, read the GOD GIFT card, use the
shrine, then do what the card says (most begin with the surge, L2 or I).

### The chart

Rest at a rest fire in the Castle to open the chart. It stays on the tree of the element you hold; R2 or K shows the
whole ring.
