# The pairs at distance 4

Content for `docs/design/STEEL_AND_ELEMENT.md` sections 6.1 to 6.3: the eleven pairs that stand four apart on the
ring. Everything here is a **Proposal**; every number is a first guess. Written to the brief in `docs/design/steel/BRIEF.md`.

## Index

1. **Fire and Water: Steam.** Water flashes off a burning body. Shapes: *Blind Steam* (a cloud enemies cannot see
   through) or *Scald* (a hard burst). Duo spell *The Sisters' Quarrel*: flame and wave close like pincers on one enemy.
2. **Lightning and Life: Lightning Flower.** A red fern of lightning flowers on its skin, and everything you have
   grown throws a bolt into it.
   Shapes: *The Rice Wakes* (a field that mends you) or *Struck Oak* (the body bursts like a struck tree). Duo spell
   *Sowing Thunder*: seeds flung out, and a heavy blow calls a bolt onto every one.
3. **Wind and Earth: Topple.** A shaken, unsteady enemy goes down flat. Shapes: *Sandblast* (the dust of the fall is
   thrown on as stinging grit) or *Logan Stone* (it rocks under three blows, then falls hard). Duo spell *Walking
   Dune*: a ridge of sand the wind drives over them.
4. **Ice and Metal: Cold Short.** Frost in the notches turns armour to glass. Shapes: *Spall* (shrapnel thrown out
   behind) or *Snapped Blade* (its own blow snaps on your guard). Duo spell *Cold Chisel*: tap it in, then drive it.
5. **Water and Light: Caught Light.** The light caught in wet ground makes a net that marks all who stand in it.
   Shapes: *Thrown Wide* (the net hauls them in) or *The Fish's Eye* (one point, a triple critical). Duo spell
   *The Gods' Road*: run a rainbow of spray to an enemy and come down on it.
6. **Life and Dark: Grave Roots.** Black roots hold the enemy down, and blows on it feed you. Shapes: *Corpse
   Flowers* (pale flowers that mend) or *Hollow Trunk* (its death leaves a trunk of roots to fight round). Duo
   spell *Death Turned Back*: the roots take a blow meant for you.
7. **Earth and Poison: Blackdamp.** The cracked ground breathes out a heavy poison that sits on the floor. Shapes:
   *Chokedamp* (it runs to other shaken enemies) or *Red Ore* (arsenic veins your heavy blow bursts). Duo spell
   *The Clay Rank*: three clay soldiers march out and burst in poison.
8. **Metal and Fire: Red Hot.** Notched armour heats red and burns its wearer. Shapes: *Slag* (it drips fire where
   it walks) or *The Burning Shirt* (the armour clings and sears it each time it attacks). Duo spell *Tipped Crucible*: each heavy blow pours
   molten bronze. Duo surge *Heated Bronze*.
9. **Light and Lightning: The Bolt Runs True.** The mark lays a path and a straight bolt comes down it. Shapes:
   *Laid Flat* (the bolt runs to the next enemy) or *Struck Again* (each critical calls a bolt). Duo spell
   *Diamond Thunder*: a held line of light, then one stroke down it. Duo surge *The Net of Jewels*.
10. **Dark and Wind: Breath Taken.** A black wind draws the breath out of the enemy and leaves it winded.
    Shapes: *Black Storm* (the wind runs on to the next) or *Second Wind* (the breath comes to you). Duo spell
    *Blown Through*: a counter that carries you through the attacker on the wind. Duo surge *Wind From the Grave*.
11. **Poison and Ice: Venom Rime.** Poison that will not fade, and each bite freezes it deeper. Shapes: *Frost Over
    Frost* (rime spreads on the floor) or *Cold Damage* (the shatter lets every dose go). Duo spell *Rime Over Rime*:
    rings of venom frost growing outward from one point.

Then: the engine needs flagged, and the sources.

## How these entries read

- A reaction happens when an affliction is laid on an enemy that already carries the other. Order does not matter
  unless the entry says so. Each reaction has a rest per enemy (written as "once every N s on one enemy") so it is an
  event the player sees, not a hum.
- A reaction never touches the player, and nothing here hides the player: clouds and mist are drawn stippled and low,
  and the player is drawn through them.
- Each branch: one mark that lays both afflictions, one that opens the two shapes (swapped at any rest fire), one or
  two that tie the reaction to the weapon, then the duo spell. Kinds and prices follow the shared chart in
  `README.md`: Shape 3, Minor 1, Major 3, Keystone 5 Shards; the duo spell 5; a duo surge 5.
- Afflictions that a reaction lays start no reaction of their own (the shared rule); an entry that breaks it on
  purpose says so.
- Damage and stagger are in weapon points (a soldier: 60 health, 100 balance; a light blow of the fists: 8 and 10).

---

### Fire and Water (Burn + Soaked), distance 4

**The image.** Where lava runs into the sea the water boils to a white plume of steam, acid and glass, and black
land is made at the shore: in the Hawaiian telling it is Pele and her elder sister Nāmaka, sea and fire, still at war
on the beach. The Egyptian sun rose out of the waters of Nun; here the two meet again and neither wins.

**Reaction: Steam.** Either order. The water on the body flashes off: Soaked is spent and half the Burn (rounded up)
goes with it. A puff of scalding steam in 2.5 m: 8 damage and 4 more for each Burn spent, 15 stagger, to every enemy
in it. Once every 3 s on one enemy. Seen: a white plume torn upward off the body with a hiss, black flecks of glass
thrown out of it, gone in 0.6 s.

**Shape A: Blind Steam.** The plume does not lift. It lies 3 m wide for 4 s, low, white and stippled. Enemies inside
see only 2 m: one that loses sight of the player stops and turns about, and their swings inside are 20% slower. It
stings: 3 damage a second. **Shape B: Scald.** No cloud. The burst is harder and throws: 14 damage and 6 for each
Burn stack, 30 stagger, everything pushed 1.5 m. The Burn is not spent; it boils on.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Hissing Blade | Major | 3 | Your heavy blows lay Burn and Soaked together. |
| Two Sisters | Shape | 3 | Opens the shapes of Steam: Blind Steam or Scald. |
| Strike Through Steam | Major | 3 | A weapon blow on an enemy within a second of its steam is a scald: half again the damage and stagger. |
| Kettle | Keystone | 5 | A Soaked enemy that dies burning goes up as steam in your chosen shape, at full strength, with no rest. |

**Duo spell: The Sisters' Quarrel** (breaker). 12 s cooldown. Flame runs from your left hand and a wave from your
right, along two curved lanes that close on the enemy faced, up to 7 m off, in 0.45 s. Each lane strikes what it
crosses for 8 (the flame lays a Burn, the wave Soaks). Where they meet, a steam blast of 3.5 m: 30 damage, 55
stagger, and Steam at full in its shape. Seen: a run of flame tongues and a foam-topped wave racing in a pincer, and
a white eruption with black glass where they meet. **Steel**: for 3 s the enemies caught are scalded raw; your weapon
blows on them strike 25% harder and each lays a Burn.

---

### Lightning and Life (Static + Seeded), distance 4

**The image.** The Japanese word for the lightning flash, *inazuma*, means "the rice's spouse": storms came with the
ripening, and the lightning was thought to make the rice bear. In Welsh *dar* is both oak and thunder, and the druids
cut mistletoe from the oaks heaven favoured. Lightning that strikes the living leaves a red fern on the skin, the
"lightning flower". A spark sown into a seed, and the field answering.

**Reaction: Lightning Flower.** Either order. The Static finds the seed's roots, and the roots find yours: a red fern
of lightning flowers over the enemy's skin, and every grown thing of yours within 8 m (briars, saplings, shoots,
lilies) throws a bolt into it, four at most: 8 damage and 10 stagger each, and one Static. With nothing grown near,
the seed itself cracks for 10 damage. The enemy keeps both afflictions. Once every 4 s on one enemy. Seen: a branching
red-violet fern spreading over the body, and thin bolts leaping from the player's plants to it. (A fern spread over
the floor is the Lightning and Earth reaction, Earthed: there the charge goes to ground, here it comes up out of
your garden.)

**Shape A: The Rice Wakes.** The ground round it, 3 m, sprouts a field of green shoots for 6 s (a grown thing, so it
throws its own bolt at the next Lightning Flower). Standing on it
you mend 2 a second; an enemy that crosses it takes a Static each second. **Shape B: Struck Oak.** No fern. The
current boils the seed inside the body and it bursts like a struck oak, bark and splinters thrown 3 m: 16 damage, and
5 more for each Static and each Seed it carried; its Static is discharged and its Seeds spent.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Storm Season | Major | 3 | Your heavy blows lay Static and a Seed together. |
| Field or Oak | Shape | 3 | Opens the shapes of Lightning Flower: The Rice Wakes or Struck Oak. |
| The Favoured Oak | Major | 3 | A perfect guard against a Seeded enemy hangs a scorched oak leaf on your blade. Your next heavy blow discharges at twice the strength. |
| Rain on the Field | Keystone | 5 | Each Lightning Flower mends you 3 for every bolt thrown into it, and every grown thing that throws one stands 3 s longer. |

**Duo spell: Sowing Thunder** (detonator). 10 s cooldown. You fling seven seeds in a fan 6 m ahead, 70 degrees
wide, like a farmer sowing; each that strikes an enemy Seeds it, the rest sink into the floor. For 6 s your next
heavy blow, or any discharge of Static, calls lightning onto every seed at once: 12 damage and 20 stagger in 1.5 m
round each, and every seed in or under an enemy is a Lightning Flower. Seen: grains glinting as they fly, a row of
small lights in the ground, then seven bolts in one beat and ferns spreading from each. **Steel**: the heavy blow is
the trigger; if it is a finisher, every bolt strikes twice.

---

### Wind and Earth (Unsteady + Tremor), distance 4

**The image.** Wind cannot cut stone, but the grit it carries can: ventifacts are rocks faceted and polished by
sandblast, turning a new face to the wind each time one is worn flat. Cornwall's logan stones, eighty tons balanced so
a child can rock them, were said to be beyond any human power to overturn, until sixty sailors did it. And the wind
walks the earth itself: a barchan dune creeps downwind, tens of metres a year, over whatever stands in its way. The
wind god's bag against the standing stones: what can be rocked can be toppled, and what is toppled is buried.

**Reaction: Topple.** Either order. The shaken, unsteady enemy goes down flat for 1.2 s (elites 0.5 s; gods and the
colossus reel for 40 stagger instead). Its fall shakes the ground: 10 damage, 15 stagger and a Tremor in 2.5 m. Once
every 6 s on one enemy. Seen: the body tips over and hits the floor, and a ring of dust bursts out from under it.

**Shape A: Sandblast.** The dust of the fall is not let settle. The wind throws it on, away from you, as a cone of
stinging grit 5 m long and 60 degrees wide: 6 damage, 15 stagger and an Unsteady to each enemy in it, so one that is
already shaken may topple in turn (this breaks the no-cascade rule on purpose). Fell them in a row. **Shape B: Logan
Stone.** It does not fall at once: it rocks. Each of your next three blows within 3 s rocks it further (each breaks
balance a quarter harder than the last, dust puffing from alternate sides), and the third throws it down hard: flat for
1.8 s, its fall 20 damage and 30 stagger in 3 m.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Grit in the Wind | Major | 3 | Your light blows on a shaken enemy leave it Unsteady; your heavy blows on an Unsteady enemy shake it. |
| Grit or Stone | Shape | 3 | Opens the shapes of Topple: Sandblast or Logan Stone. |
| New Face | Major | 3 | Your first blow on a toppled enemy is critical and gives 6 mana. |
| The Prophecy Broken | Keystone | 5 | Elites and gods can be toppled: after three Topples have only made one reel in a fight, the fourth throws it flat for 1 s. |

**Duo spell: Walking Dune** (breaker). 12 s cooldown. A ridge of sand and grit 5 m wide heaps up before you and the
wind drives it forward 7 m in 1.2 s, its crest smoking with blown sand. Each enemy it rolls over takes 16 damage and
35 stagger, two Unsteady and a Tremor, so it topples as the dune passes. Where it stops it lies 4 s as a low dune:
enemies in it wade at 60% speed. Seen: a ridge with a sharp crest and a long back, ripples combed across it, sand
streaming off the top the way it goes. **Steel**: a heavy blow struck into the standing dune drives it on again, 5 m
more along the blow, once.

---

### Ice and Metal (Chill + Sundered), distance 4

**The image.** Cold makes steel glass. In the winters of the Second World War welded Liberty ships cracked, and some
split in two at the dock; smiths have long called iron that breaks in the cold "cold-short". The Norse winter takes
hold of the Greek bronze, and what the smith made to bend, snaps.

**Reaction: Cold Short.** Either order. Frost fills the notches and the armour goes brittle for 5 s: blue-white rime
in every cut, a glassy sheen on the plates. While brittle its notches do not fade. The next heavy blow on it cracks
the armour across: 10 damage and 10 more for each notch, 25 stagger for each notch. The notches stay. Once every 5 s
on one enemy. Seen: a white crack runs the body's length with a high ring of steel, and splinters fly.

**Shape A: Spall.** The crack throws the armour off the far side of the body, away from your blow: a cone of frozen
splinters 4 m long and 60 degrees wide, 8 damage, a Chill and a notch to each enemy it reaches. Line them up.
**Shape B: Snapped Blade.** The armour does not crack under your blow; its own weapon snaps on yours. While brittle,
any blow of its that you guard counts as a perfect guard and breaks its weapon: 50 stagger, held 1 s, and its blows
deal half for 5 s.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Rimed Edge | Major | 3 | Your heavy blows lay a Chill and a notch together. |
| Spall or Snap | Shape | 3 | Opens the shapes of Cold Short: Spall or Snapped Blade. |
| Glass Bell | Major | 3 | Each blow on a brittle enemy rings like glass: 3 mana, and 0.3 s off every running spell cooldown. |
| Brittle to the Core | Keystone | 5 | Cold Short on a frozen enemy shatters it as well, the shatter and the crack landing together. |

**Duo spell: Cold Chisel** (detonator). 10 s cooldown. A chisel of black ice sets point-first in the enemy faced,
within 8 m. Each light blow you land on that enemy taps it in: a notch and a Chill, three at most, each with a higher
ringing note. Your first heavy blow on it within 6 s drives it home and splits the armour: 24 damage and 12 more for
each notch, 70 stagger, and Cold Short at once. If the enemy is frozen it shatters too. **Steel**: the taps load it;
the heavy blow spends it.

---

### Water and Light (Soaked + Marked), distance 4

**The image.** Light falling through moving water is caught in a net of bright lines on the floor below (the
optician's word, *caustic*, means "burning"); seen through spray it breaks into the rainbow, which in Hawaii and Tahiti
was the road the gods walked down to the earth. In the Indian telling, Arjuna won his bride by shooting a turning
fish's eye while looking only at its reflection in water.

**Reaction: Caught Light.** Either order. The water on the body catches the mark's light and throws it down: a net
of shifting bright lines lies on the wet floor 3 m round the enemy for 4 s. Every enemy that stands on it is Marked
and moves 25% slower, caught. The mark that made it is not spent. Once every 6 s on one enemy. Seen: pool-floor light,
gold lines crawling over a dark wet sheen, beads of spray thrown up where an enemy steps.

**Shape A: Thrown Wide.** The net is cast 5 m wide and hauled: when its time ends everything on it is dragged 2 m
toward the enemy that bore it. **Shape B: The Fish's Eye.** No net. The water on the body gathers the light into one
white eye, as Arjuna's target was found in the water below it. Your next weapon blow on it is a critical at three
times the damage (not one and a half) and passes through to strike one enemy behind it within 3 m. The mark is spent.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Bright Water | Major | 3 | The blow that ends a chain lays Soaked and a Mark together. |
| Net or Eye | Shape | 3 | Opens the shapes of Caught Light: Thrown Wide or The Fish's Eye. |
| Sun on the Spray | Major | 3 | A perfect dodge Soaks and Marks every enemy within 3 m of where it ends. |
| Seen in the Water | Keystone | 5 | While an enemy stands on the net, your blows on any other enemy on it are critical as if it were Marked, without spending a mark. |

**Duo spell: The Gods' Road** (movement). 9 s cooldown. A rainbow of falling spray arches from you to a Soaked or
Marked enemy within 10 m (or the enemy faced). You run it, untouchable, in 0.4 s and come down on the enemy with a
falling cut: 18 damage, 30 stagger, Soaked and Marked. The road stands 3 s as a band of spray and light on the ground
under its arch; what stands in it is Soaked and Marked. Seen: an arch of droplets lit in bands, not a painted stripe,
with spray falling from it the whole length. **Steel**: press light attack as you land and the falling cut is
critical and leaves Caught Light under the enemy.

---

### Life and Dark (Seeded + Cursed), distance 4

**The image.** The churchyard yew, the druids' tree of death and return, was said to root through the eyes of the
dead and hold them in their graves; the ghost pipe, the "corpse plant", grows white in deep shade with no light of its
own, fed through the roots of others. Under a banyan, whose kin the strangler fig leaves a hollow trunk where its host
stood, Savitri followed Death with his noose and talked him into giving her husband back. Growth that feeds in the
dark, on the dying.

**Reaction: Grave Roots.** Either order. Black roots grow from the seed down the enemy's legs into the floor and hold
it for 1.2 s (elites 0.5 s). While it is held each of your blows on it mends you 2. Once every 6 s on one enemy.
Seen: roots like a fig's, black and glossy, winding up to the knee in 0.2 s, pale waxy flowers opening along them.

**Shape A: Corpse Flowers.** When the roots let go, three pale flowers stand where it stood for 8 s (grown things).
Each weapon blow you land on an enemy within 3 m of a flower makes 3 of your grey health real: fight beside them, do
not walk over them (walk-over pickups are Shining Herb's). **Shape B: Hollow Trunk.** The roots do not let go: they
stand round the enemy as a cage for 6 s, and if it dies inside, the cage stays where it stood for 8 s as a hollow
trunk of black roots. Nothing walks or shoots through it, and an enemy that touches it is Seeded and Cursed. Kill
them where you want a wall.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Bitter Seed | Major | 3 | Your finishers lay a Seed and a Curse on every enemy within 3 m. |
| Flowers or Trunk | Shape | 3 | Opens the shapes of Grave Roots: Corpse Flowers or Hollow Trunk. |
| Held Down | Major | 3 | A perfect guard against a Cursed enemy roots it as Grave Roots does, whatever else it carries. Once every 6 s. |
| What the Yew Keeps | Keystone | 5 | A Seeded, Cursed enemy that dies does not burst into thorns: its seed and its curse pass whole to the nearest enemy within 6 m, which is rooted at once. |

**Duo spell: Death Turned Back** (counter). 12 s cooldown. You go down on one knee and roots spread round you, 1.5 m,
for 1.2 s. The first blow that would land on you is taken by the roots: they climb the attacker and hold it 2 s
(elites 0.8 s), Seed it three deep and Curse it, and you mend what the blow would have dealt, 30 at most. If nothing
comes, half the cooldown comes back. Seen: hanging roots dropping round the attacker like a banyan's and closing to
the waist, a cord of black root drawn tight round its neck like Death's noose. **Steel**: each of your blows on the held enemy mends you 3.

---

### Earth and Poison (Tremor + Poisoned), distance 4

**The image.** Shake the ground and it breathes out what it held: miners called the gases damps, and blackdamp put
their lamps out; at Lake Nyos a cloud heavier than air rolled silently down the valleys. Realgar, the red arsenic
stone, was drunk in wine in China to drive off the five poisons. The Celts' barrows and cairns and the First
Emperor's buried clay army: the earth keeps its dead, and its poisons with them.

**Reaction: Blackdamp.** Either order. The ground cracks under the enemy and breathes out a heavy mist that hugs the
floor: a pool 2.5 m wide for 5 s. What stands in it takes a dose each second, moves 20% slower, and does not regain
balance. Once every 6 s on one enemy. Seen: cracks open in a star, grey-green mist pours out and settles in them,
never above the knee, with the cracks showing through.

**Shape A: Chokedamp.** The damp is heavy and runs: a creeping tongue of it flows along the floor to each other
shaken enemy within 7 m, three at most, and pools under each. **Shape B: Red Ore.** No mist. The poison sets in the
cracks as veins of orange-red realgar under the enemy. Your next heavy blow on it within 4 s breaks the ore: a burst
of red dust and stone chips in 3 m, 14 damage and 3 more for each dose it carries, three doses and 30 stagger to each
enemy reached.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Foul Ground | Major | 3 | Your heavy blows lay a Tremor and a dose together. |
| Damp or Ore | Shape | 3 | Opens the shapes of Blackdamp: Chokedamp or Red Ore. |
| The Lamp Goes Out | Major | 3 | Your weapon breaks the balance of enemies standing in the damp a quarter harder. |
| Seam of Ore | Keystone | 5 | Each time poison bites a shaken enemy, the ground shakes under it: a Tremor to every enemy within 2 m. |

**Duo spell: The Clay Rank** (opener). 14 s cooldown. Three clay soldiers rise in a rank before you and march 7 m in
lockstep, shields first, over 6 s. They push what stands before them at half a metre a second and take enemy blows
from the front; each breaks after two. A soldier that breaks, by an enemy's blow, at the end of its march or by your
heavy blow, bursts in a puff of damp and clay shards: 12 damage, a Tremor and three doses in 2.5 m. Seen: grey clay
men with cracked faces, grey-green damp leaking from their joints, cairn stones tumbling from them as they walk. **Steel**: one broken
by your own heavy blow bursts twice as hard.

---

### Metal and Fire (Sundered + Burn), distance 4

**The image.** The sun-king promised his people the gods' own flesh, which in Egypt was gold over silver bones;
Talos, the bronze guardian of Crete, heated himself red-hot and crushed men in his embrace. Iron heated with sulphur in
it goes "red-short" and crumbles under the hammer. Burning armour turns on whoever wears it.

**Reaction: Red Hot.** Either order. The notched armour heats cherry red for 5 s and smoke pours off it. Burns on it
bite twice as hard, and each heavy blow on it throws a spray of forge sparks out of its far side, a cone 3 m long: 6
damage and a Burn to each enemy in it. Once every 5 s on one enemy. Seen: the plates glowing from the cuts outward,
heat squiggles, sparks spraying like struck iron.

**Shape A: Slag.** The armour runs. For the 5 s, every metre it moves leaves a drip of molten slag that burns 3 s
where it falls (0.8 m, a bite and a Burn). Walk it through its friends. **Shape B: The Burning Shirt** (the
poisoned shirt that clung to the Greek hero and burned him; a cast that sets and is broken is Metal's own Poured
Bronze). No sparks: the hot plates cling. For the 5 s, each attack it begins deepens its Burn by two and makes it
flinch 0.3 s as the metal sears it, and while it burns five deep it cannot guard. Let it swing, then punish.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Heated Edge | Major | 3 | Your heavy blows lay a Burn and a notch together. |
| Slag or Shirt | Shape | 3 | Opens the shapes of Red Hot: Slag or The Burning Shirt. |
| Strike While Hot | Major | 3 | Each heavy blow on a red-hot enemy gives 4 mana and takes 0.5 s off every running spell cooldown. |
| Red-Short | Keystone | 5 | A finisher on a red-hot enemy crumbles its armour: every notch is spent for 15 damage each, and for 10 s it takes 30% more from your weapon. |

**Duo spell: Tipped Crucible** (breaker). 35 mana. A crucible of molten bronze hangs at your shoulder for 8 s. Each
heavy blow you land tips it along the swing: a tongue of pour 4 m long and 1.2 m wide, white-gold at the lip cooling
through cherry red to black over 3 s. What it crosses takes 14 damage and 25 stagger, a Burn and a notch, and the
ground burns while the pour is red. Four pours at most. **Steel**: only heavy blows pour, so the spell is spent by the
weapon.

**Duo surge: Heated Bronze.** 40 mana, 10 s. You glow red to the bone like the bronze guardian. Every enemy blow that
lands on you or on your guard burns and notches the one who struck; your guard costs 30% less stamina; and a perfect
guard is the embrace: the attacker is gripped and held 1 s, burned three deep, then thrown 2 m. Seen: plates of
glowing bronze over your shoulders and arms, smoke rising off you, the floor scorched where you stand still.

---

### Light and Lightning (Marked + Static), distance 4

**The image.** Lightning takes the easiest road, and on the Säntis in 2021 a laser opened one: the bolt ran down the
beam. In India the thunderbolt and the diamond are one word, *vajra*, Indra's weapon; it came to Japan as the
*kongōsho* in the hands of the temple guardians. Light lays the road; lightning runs it straight.

**Reaction: The Bolt Runs True.** Either order. A thread of light stands from the enemy into the sky; 0.5 s later a
straight, unforked bolt comes down it: 20 damage, 35 stagger, and the enemy's Static discharges at full (7 damage and
5 stagger a stack in 4 m). The mark is not spent. Once every 5 s on one enemy. Seen: from above, a bright point on the
enemy with a ring spreading round it on the floor, then the bolt, thick and dead straight with no branches, unlike any
other lightning in the game.

**Shape A: Laid Flat.** The thread lies along the ground to the nearest other Marked or Static enemy within 10 m, and
the bolt runs the line: 18 damage to everything on it and two Static to each. **Shape B: Struck Again.** No great
bolt. The thread stays while the mark lasts, and each critical blow on the enemy (the mark's three) calls a smaller
bolt down it: 9 damage, 15 stagger, a Static.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Charged Light | Major | 3 | Your light blows lay Static on a Marked enemy, and a discharge Marks every enemy it reaches. |
| Flat or Again | Shape | 3 | Opens the shapes of The Bolt Runs True: Laid Flat or Struck Again. |
| Diamond Guard | Major | 3 | A perfect guard against a Marked enemy calls the bolt onto it at once, whatever it carries. Once every 5 s. |
| The Diamond Falls | Keystone | 5 | The Bolt Runs True no longer needs Static: the third critical blow on a Marked enemy calls it, once every 5 s. A Light player with a Lightning second hand calls the bolt with the blade alone. (A strike that comes again a moment later is Lightning's own lift, Strikes Twice.) |

**Duo spell: Diamond Thunder** (detonator). 11 s cooldown. Hold the button: a straight line of light 14 m long lies
on the floor from your hand and turns with you, for 1.5 s at most. Let go: lightning runs the whole line in one
stroke: 22 damage and 40 stagger to every enemy on it, and each is Marked; every Static it meets discharges. Seen:
a thin gold line on the ground, then a white-cored bolt along it, leaving a glassy scar for 2 s. **Steel**: a perfect
guard while the line is held looses it at once for twice the damage, and your first blow on each enemy struck within 2
s is critical.

**Duo surge: The Net of Jewels.** 45 mana, 10 s. Every Marked enemy within 12 m is linked to every other by threads of
light, each knot a bright bead (Indra's net, where each jewel holds every other; it came to Japan with Kegon
teaching). A discharge or a bolt on one runs the threads to all the others for half. Each critical blow on any one
gives every linked enemy a Static. Six enemies at most are linked, so the threads stay readable.

---

### Dark and Wind (Cursed + Unsteady), distance 4

**The image.** Japan's wind was born in the land of the dead: in the old telling the thunder and wind gods grew on
Izanami's rotting body in Yomi and slipped out through a crack in the stone that sealed it. In India the breath is
the wind inside the body, the life the wind god gives, and dying is that breath going out; and in the north the
*kali andhi*, the black storm, rolls a wall of dust over the sun before the monsoon. A wind out of the dark that takes
the breath with it.

**Reaction: Breath Taken.** Either order. A black gust wraps the enemy and draws its breath out: a grey wisp torn
from it and blown away downwind, away from you. It is winded for 1.5 s (elites 0.6 s): it begins no attack (one
already swung is finished) and takes 30% more stagger. Once every 6 s on one enemy. Seen: the body doubles over and a
long grey-black thread streams off it on the wind.

**Shape A: Black Storm.** The taken breath does not scatter. It runs on downwind as a streak of black dust to the next
enemy within 6 m and winds it too, and on, three bodies at most, leaving each Cursed and Unsteady. **Shape B: Second
Wind.** The breath is blown to you instead and you draw it in: 25 stamina, 8 mana, and your next dodge within 3 s
costs nothing.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Ill Omen | Major | 3 | A perfect dodge Curses the one dodged and leaves it two deeper in Unsteady. |
| Storm or Breath | Shape | 3 | Opens the shapes of Breath Taken: Black Storm or Second Wind. |
| Short of Breath | Major | 3 | Your heavy blows on a winded enemy strike half again as hard, and each takes 1 s off every running spell cooldown. |
| The Last Breath | Keystone | 5 | A winded enemy that dies lets its last breath out as a black gust: everything within 4 m is pushed 3 m and Cursed. |

**Duo spell: Blown Through** (counter). 9 s cooldown. You stand in a black gust for 1 s. The first blow that comes
finds no one: the wind carries you through the attacker, untouchable, and sets you down 2 m behind it, facing it,
with its breath taken (the reaction at full, in its shape). If nothing comes, the gust breaks in a small push round
you. Seen: you go to a streak of dark dust and leaves and come out of it at the enemy's back. **Steel**: your first
blow from its back within a second is critical and gives back half the cooldown.

**Duo surge: Wind From the Grave** (the wind born in Yomi; Ill Wind is the Poison and Wind reaction). 35 mana, 10 s. A black wind blows from you the way you face when you light it: a streaming
field 9 m long and 100 degrees wide that turns slowly with you, drawn as low streaks of dark dust and blown leaves.
Enemies in it are pushed away at half a metre a second and take an Unsteady each second. A Curse laid on any enemy
in the wind is carried to every enemy downwind of it.

---

### Poison and Ice (Poisoned + Chill), distance 4

**The image.** The Norse world began with the Élivágar, rivers of venom that froze, "rime over rime", and out of that
venomous rime the first giant was made: Ymir, whose corpse the Ice land is built on. China's great book of medicine,
the *Treatise on Cold Damage*, follows a cold that enters through the skin and goes down six levels toward the bone.
Cold that keeps the poison; poison that feeds the cold.

**Reaction: Venom Rime.** Either order. While both lie on the enemy its poison does not fade, and each time the poison
bites it lays a Chill. A crust of green-white rime grows on it, thicker with each bite, until it freezes. Seen: frost
creeping over the body in a sickly tint, a small puff of rime at each bite, then the freeze in green-white.

**Shape A: Frost Over Frost.** A rimed enemy that freezes spreads its rime over the floor 3 m round it for 5 s: what
stands there takes a Chill and a dose each second. **Shape B: Cold Damage.** The cold drives the poison inward. While
the rimed enemy is frozen its poison bites twice as hard, and when it is shattered every dose it carries goes off at
once, 6 damage a dose, on top of the shatter.

**Duo branch**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Bitter Frost | Major | 3 | Your heavy blows lay a Chill and a dose together. |
| Floor or Bone | Shape | 3 | Opens the shapes of Venom Rime: Frost Over Frost or Cold Damage. |
| Six Levels Down | Major | 3 | Each bite on a frozen enemy keeps it frozen 0.3 s longer. |
| The Drops Quicken | Keystone | 5 | A rimed enemy that dies leaves a drop of venom rime that rises as a small rime creature and strikes the nearest enemy three times (6 each, a dose and a Chill), then melts. |

**Duo spell: Rime Over Rime** (opener). 10 s cooldown. A drop of venom falls on the ground at the aim point, within
8 m, and rime grows from it "frost over frost": a ring every 0.6 s, out to 1.5 m, 3 m and 4.5 m. Each ring as it
grows gives every enemy it reaches a dose and a Chill, so one that stands in the middle takes all three. The rimed
ground stands 4 s, green-white, crusted in rings. Seen: a dark drop, then rings of frost feathers spreading outward
like growth rings, each with a sickly sheen. **Steel**: a heavy blow struck on the rimed ground shatters every ring at
once: 14 damage, a Chill and two doses to each enemy standing on it.

---

## Engine needs flagged

Beyond the reactions themselves (two afflictions on one enemy, a reaction table, a rest per enemy), these are not in
`docs/UPGRADE_SCHEMA.md` yet:

1. **Enemy sight cut short** inside a cloud (Blind Steam). Section 15.9 lists "changing an enemy's sight" as not yet
   possible.
2. **A knocked-flat state** (Topple, Logan Stone, Walking Dune). `Hold` with a fall animation can stand in.
3. **"Begins no attack" without stopping movement** (Breath Taken's winding). Section 16.6 says `Hold` stands in.
4. **A timed change to one enemy's damage dealt or taken, or to whether it can guard** (Snapped Blade, Red-Short,
   The Burning Shirt). Section 15.9 says no per-enemy
   debuff exists as data.
5. **Ground laid behind a moving enemy** (Slag), and **a moving ridge that rolls enemies over and then stands as
   slowing ground** (Walking Dune; the `wave` and `wall` forms are the nearest).
6. **Summons that walk in a rank, push and can be struck** (The Clay Rank). Section 14 lists "summon something that
   walks" as bespoke.
7. **Taking seconds off running spell cooldowns** as an action (Glass Bell, Strike While Hot, Short of Breath,
   Blown Through). It belongs to the cooldown model of section 3.1, which the schema does not have yet.
8. **Poison that does not fade on one enemy** while a condition holds (Venom Rime). `PoisonKeeps` is a switch for all.
9. **Links between Marked enemies that carry discharges** (The Net of Jewels), and **a wind field that carries
   curses downwind** (Wind From the Grave). Both are bespoke surge forms.
10. **A guard against an afflicted enemy counted as perfect** (Snapped Blade). Hanging Swords already stretches the
    window by hand, so it may be close.
11. **A cage of roots that stands, blocks and can outlast its enemy** (Hollow Trunk). Section 15.9 lists "things that
    stand and block and can be struck" beyond `wall` and `ring` as not yet possible.

Everything else uses actions and forms that exist: `Hazard` with `Grown` for the rice field and the flowers, `Pull`
for the haul, `Step` for Blown Through, `Helper` for the rime creature, `brand` for the Cold Chisel, `parry` for
Death Turned Back, `field` for Rime Over Rime, and the held button of `frozenwave` for Diamond Thunder.

## Sources

Phenomena:
- USGS Hawaiian Volcano Observatory, "Lava entering ocean", and Volcano Watch on laze and littoral explosions:
  https://usgs.gov/observatories/hvo/science/lava-entering-ocean
- Lichtenberg figures, PubMed 17944174 and PMC10329080: https://pubmed.ncbi.nlm.nih.gov/17944174/
- Exploding trees and lightning-struck trees, Alabama Cooperative Extension and Wikipedia:
  https://www.aces.edu/blog/topics/forestry/caring-for-lightning-struck-trees/
- Ventifacts and yardangs: https://en.wikipedia.org/wiki/Ventifact ; https://www.nzgeo.com/stories/ventifacts-sandblast-from-the-past/
- Rocking stones and the Logan Rock of Treen: https://en.wikipedia.org/wiki/Logan_Rock ;
  https://www.countrylife.co.uk/out-and-about/curious-questions-who-dislodged-britains-most-famous-balancing-rock-258035
- The ductile to brittle transition and the Liberty ships: https://metallurgyzone.com/ductile-to-brittle-transition-temperature-dbtt/ ;
  https://en.wikipedia.org/wiki/Embrittlement
- Red-short steel: https://en.wikipedia.org/wiki/Red-short_carbon_steel
- Caustics on a pool floor: https://www.onlandscape.co.uk/2019/01/physics-of-caustic-light-in-water/
- Laser-guided lightning on the Säntis (Houard et al., Nature Photonics 2023): https://www.ncbi.nlm.nih.gov/pmc/articles/PMC9988683/
- Strangler figs: https://en.wikipedia.org/wiki/Strangler_fig
- Ghost pipe, the corpse plant: https://www.fs.usda.gov/wildflowers/plant-of-the-week/monotropa_uniflora.shtml
- Mine damps: https://en.wikipedia.org/wiki/Blackdamp ; https://www.britannica.com/technology/mine-gas
- The Lake Nyos disaster: https://en.wikipedia.org/wiki/Lake_Nyos_disaster
- Realgar wine and the five poisons: https://en.wikipedia.org/wiki/Dragon_Boat_Festival ;
  https://baike.baidu.com/en/item/realgar%20wine/31867
- Barchan dunes and how fast they walk: https://en.wikipedia.org/wiki/Barchan ; https://www.britannica.com/science/barchan
- Kali andhi: https://www.weatheronline.co.uk/reports/wind/The-Andhi.htm

Myth:
- Pele and Nāmaka: https://en.wikipedia.org/wiki/Pele_(deity)
- Nun and the benben: https://en.wikipedia.org/wiki/Nun_(mythology) ; https://en.wikipedia.org/wiki/Benben
- Ra's flesh of gold, bones of silver, hair of lapis: https://the-past.com/feature/hilary-wilson-on-lapis-lazuli/
- Inazuma and shide: https://www.behindthename.com/name/inazuma/submitted ; https://omikuji-sando.com/en/word/%E7%B4%99%E5%9E%82/
- The oak, mistletoe and thunder (Pliny; *dar*): https://www.sacred-texts.com/neu/celt/rac/rac16.htm ;
  https://lornasmithers.co.uk/2019/02/16/daronwy-the-prophetic-oak/
- Fūjin and Raijin, born in Yomi: https://en.wikipedia.org/wiki/F%C5%ABjin ; https://en.wikipedia.org/wiki/Raijin
- The vajra and the kongōsho: https://butuzou.com/blogs/blog/the-vajra-in-japanese-esoteric-buddhism-kongosho-symbolism-in-mandalas-and-statues
- Indra's net: https://en.wikipedia.org/wiki/Indra's_net
- Draupadi's svayamvara and the fish's eye: https://en.wikipedia.org/wiki/Drupada
- The rainbow as the gods' road in Hawaii and Tahiti: https://en.wikipedia.org/wiki/Rainbows_in_mythology ;
  https://en.wikipedia.org/wiki/%CA%BBOro
- Savitri, Yama and the banyan: https://en.wikipedia.org/wiki/Savitri_and_Satyavan ; https://en.wikipedia.org/wiki/Vat_Purnima
- The churchyard yew: https://www.icysedgwick.com/yew/
- The Élivágar, eitr and Ymir (*Gylfaginning* 5): https://en.wikipedia.org/wiki/%C3%89liv%C3%A1gar ; https://en.wikipedia.org/wiki/Ymir
- The *Shanghan Lun*: https://en.wikipedia.org/wiki/Shanghan_Lun
- The project's own myth notes: `docs/design/WORLD.md` (Talos, the Qin tomb and terracotta army, Ymir's fjord, the
  Celtic barrows and cairns, Arjuna).

Games, for principles only:
- Hades duo boons (two gods' signature effects made into one rule): https://www.thegamer.com/hades-duo-boons-list-how-to-get/
- Genshin Impact reactions (amplifying versus self-damaging reactions; reactions that leave something behind):
  https://gamerant.com/genshin-impact-elemental-reactions-guide/
- Divinity: Original Sin 2's steam clouds and surfaces, Breath of the Wild's chemistry and Noita's lava and water,
  from knowledge: the lesson taken is that a reaction should leave a thing on the floor the player can use, not only
  a number.
- Checked against the other slices as they stood: `pairs_d5.md` already holds the sickle weasels, a dust devil, a
  noose, burning glass and channels of quicksilver, so those were left to it. In the reconciliation the caustic nets
  were taken out of Light's own look in `elements_b.md`, so they belong to Caught Light alone.
