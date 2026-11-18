# The pairs at distance 3 (2026-11-18)

Eleven duo branches for the pairs three steps apart on the ring. Everything here is a **Proposal**: Claude's
design, waiting for the owner's word or for play. Every number is a first guess.

Rules that hold for every entry below:

- A reaction happens once per enemy every 6 s unless the entry says otherwise. The shared default is 4 s
  (`README.md`); these eleven rest longer because most of them hold an enemy or set up a heavy blow. The order the two
  afflictions arrive in does not matter unless the entry says so.
- Afflictions that a reaction lays start no reaction of their own (the shared rule); an entry that breaks it on
  purpose says so.
- Mark kinds and prices follow the shared chart: Minor 1 Shard, Major 3, Shape 3 (opens both shapes, swapped at rest
  fires), Keystone 5, duo spell 5, duo surge 5.
- Cooldowns are the new spell cooldowns (section 3.1 of the design), which weapon play shortens. A duo surge costs
  mana like any surge.

## Index

1. **Fire and Ice**: *Thermal Shock* cracks the body; *Fire-Setting* lays a fire against an enemy, then quenches it.
   Duo surge *The Yawning Gap*.
2. **Lightning and Water**: *Seized* locks the muscles and wets the floor; *The Flashing* runs along the floor from body
   to body.
3. **Wind and Life**: *Windthrow* roots the enemy so the next shove tears it out; *Thistledown* blows you through
   a crowd as seed.
4. **Ice and Earth**: *Frost Heave* lifts the enemy on a frozen mound; *The Hag's Staff* hammers frost into the ground.
5. **Water and Metal**: *Rust* keeps the notches and adds more; *The Weighted Net* drops a wet chain net.
6. **Life and Light**: *Shining Herb* grows a herb to pick up; *Life-Herb* roots in one enemy and ripens on critical
   blows.
7. **Earth and Dark**: *Turned to Stone* stiffens it for a heavy blow to break; *Stone Underfoot* strides to an enemy
   and turns it to stone at once.
8. **Metal and Poison**: *Quicksilver* makes every blow bite; *Ichor* turns a blow and pulls the nail, so the
   attacker bleeds silver.
9. **Light and Fire**: *Kindling Point* draws the brackets to a burning point; *Hands of the Sun* grip four enemies in
   flame.
10. **Dark and Lightning**: *The Grudge* hangs a cloud that answers the enemy's blow; *The Exile's Grudge* pays back
    every blow taken. Duo surge *Ash and Thunder*.
11. **Poison and Wind**: *Ill Wind* trails poison from every enemy thrown; *The Yellow Gale* drinks the poison in,
    then blows it out.

Roles of the eleven duo spells: openers 4 (The Flashing, The Weighted Net, Life-Herb, Hands of the Sun), breaker 1
(The Hag's Staff), detonators 2 (Fire-Setting, The Yellow Gale), counters 2 (Ichor, The Exile's Grudge),
movement 2 (Thistledown, Stone Underfoot).

---

### Fire and Ice (Burn + Chill), distance 3

**The image.** The Egyptian sun meets the Norse winter. In Snorri's telling, the heat out of Muspelheim met the rime
of Niflheim in Ginnungagap, and the drops it melted quickened into Ymir, the first giant. Egypt's gold miners in Nubia
cracked rock the same way: they built a fire against the rock face, then threw cold water on it, and it split. The two
do not cancel each other. Together they break things open.

**Reaction: Thermal Shock.** When a burning enemy is chilled, or a chilled enemy is set burning. The body cracks: 14
damage, and it is **Cracked** for 5 s. The next heavy blow on a Cracked enemy splits it: 30 damage and 40 stagger,
and the crack is spent. Both afflictions stay on it. What the player sees: frost goes white over the body and splits
along seams that glow like coals, spitting embers and flakes of rime.

**Shape A: Hot Shards.** The split throws shards of hot stone and ice 3 m round: 8 damage, one Burn and one Chill on
everything they hit, so a crowd starts to crack too (this breaks the no-cascade rule on purpose). **Shape B:
Crazing.** The crack does not wait for a heavy blow. While the enemy carries both afflictions it splits again by itself every 1.5 s (10 damage, 12 stagger), so the
play is keeping both alive on one enemy.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Spark and Rime | Major | 3 | Heavy blows lay one Burn and one Chill. |
| Hot Shards or Crazing | Shape | 3 | Opens the two shapes of Thermal Shock. |
| Hot Stone, Cold Water | Major | 3 | A perfect guard against a Cracked enemy splits it at once. |
| Neither Wins | Minor | 1 | On an enemy that carries both, neither the Burn nor the Chill fades. |
| Fire Under the Ice | Keystone | 5 | A Frozen enemy that is burning shatters on any blow, not only a heavy, and its shatter sets everything within 3 m burning two deep. |

**Duo spell: Fire-Setting** (the crown, 5 Shards). Detonator. Cooldown 12 s. **Press once**: a fire is laid against
the enemy faced within 6 m. Flames bank up against its body for up to 4 s, 4 damage a second, and its Burn deepens
by one each second. **Press again** (or when the 4 s are up): you throw a dash of rime and it breaks over the heated
body: 20 damage plus 8 for each Burn, 60 stagger, Chill three deep, and all its Burn is spent. Enemies within 2.5 m
take half. What it looks like: a fire banked against a body as miners banked it against a rock face, then a fan of
white rime thrown from the hand, and the body splitting along glowing seams while chunks of frost and slag fall. How
it rewards steel: each weapon blow that lands on the heated enemy before the quench makes the quench 10% harder (to
50%), and the quench counts as a Thermal Shock split.

**Duo surge: The Yawning Gap** (40 mana, 10 s). Your weapon blows take turns: the first burns, the next chills, and so
on, so every second blow can set off Thermal Shock, once per enemy every 2 s while the surge lasts. Each split lets
fall drops of melt round the enemy, and they quicken: each drop heals you 2 (6 a second at most). On you: one arm
rimed white, the other burning, sparks and snowflakes circling the opposite ways.

Engine needs: a second press of a slot spell (16.2 covers only the surge); blows that alternate affliction.

---

### Lightning and Water (Static + Soaked), distance 3

**The image.** Lightning that hits the sea does not go down. It races out across the skin of the water, which is why
only fish near the surface die. Polynesian navigators of the Santa Cruz islands steered by *te lapa*, "the flashing":
streaks of light darting along the sea toward land. The Japanese thunder meets the Polynesian sea, and the bolt
travels along the surface.

**Reaction: Seized.** When a Soaked enemy carries two Static or more. This is beyond what Soaked already does when it
passes laid Static to its neighbours. The current locks its muscles: it cannot let go, and is held for 1 s, twitching
(elites and gods for 0.4 s). The water on it runs off into a film 2.5 m round its feet that stays live for 3 s: any
other enemy standing in the film gains one Static every half second. Its own Static is not spent. What the player
sees: the body goes rigid with its arms locked, water sheets off it onto the floor, and thin white threads skitter
across the film.

**Shape A: Can't Let Go.** Each weapon blow on a seized enemy holds it 0.3 s longer, up to 3 s in all; no film is
made. One enemy, kept locked by the blade. **Shape B: Running Water.** No hold. The film runs outward at 6 m a second
to 7 m, and every enemy its edge crosses is seized for 0.5 s and given one Static.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Wet Thread | Major | 3 | Light blows on a Soaked enemy lay one Static. |
| Can't Let Go or Running Water | Shape | 3 | Opens the two shapes of Seized. |
| Grounded | Major | 3 | A heavy blow on a seized enemy discharges its Static into the floor: everything standing in live water takes the discharge in full. |
| Salt Water | Minor | 1 | The film of live water stays 2 s longer. |
| The Sea Finds Land | Keystone | 5 | Static on a Soaked enemy needs no heavy blow. At full depth it discharges by itself, through the water, to every Soaked enemy within 8 m. |

**Duo spell: The Flashing** (the crown, 5 Shards). Opener. Cooldown 10 s. You slap the floor with an open hand. A
flash runs across the ground to the nearest Soaked enemy within 10 m (the enemy faced if none is Soaked), then darts
on to the next nearest within 6 m, up to five, one every 0.12 s. Each one it reaches takes 12 damage and 20 stagger,
is Soaked and is given two Static, so it is seized at once. The path stays wet for 2 s. What it looks like: a low
bright seam darting over the floor under a skin of running water, spray kicked up at every turn, never a bolt from the
sky. How it rewards steel: the next weapon blow on the last enemy reached, within 2 s, discharges every enemy the flash
touched.

Engine needs: Seized is Hold (exists); the film is a Hazard that gives Static. "Discharge everything this flash
touched" needs the cast to remember its targets.

---

### Wind and Life (Unsteady + Seeded), distance 3

**The image.** Windthrow: a gale tears a tree out by its roots and stands the root plate on edge. The Celtic grove
meets the Japanese wind god and the bag he carries: roots hold on, wind pulls, and something gives. Irish country
people said a whirlwind on a still day, the *sí gaoithe*, was the fairy host passing. They blessed themselves or threw
dust after it.

**Reaction: Windthrow.** When an Unsteady enemy is Seeded, or a Seeded enemy made Unsteady. The seed roots it. Pale
roots crawl 1.5 m out over the floor from its feet, and for 2 s it cannot move or be shoved, though it can still turn
and swing. The next shove it takes while rooted (a Gust, a throwing heavy, a ram, Riptide) tears it out. It is
thrown twice as far, takes 14 damage and 35 stagger as the roots rip, and leaves a torn pit of earth and bramble
1.5 m across. The pit stands 5 s and seeds what walks into it; it counts as something grown. What the player sees:
roots spreading like cracks, then a disc of earth heaved up on edge and flopping over, with leaves blown off.

**Shape A: Blown Seed.** The shove does not tear it out. Its seeds blow off downwind instead: every enemy within 6 m
in the direction it was shoved is Seeded two deep, and can be rooted in turn (this breaks the no-cascade rule on
purpose). **Shape B: Deep Roots.** It holds for
4 s, and each of your blows on it heals you twice the usual for the seed. Tearing it out then deals 60 stagger.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Seed on the Wind | Major | 3 | A dodge that passes within 2 m of an enemy leaves it Seeded and Unsteady. |
| Blown Seed or Deep Roots | Shape | 3 | Opens the two shapes of Windthrow. |
| Torn Up | Major | 3 | A heavy blow on a rooted enemy counts as a shove: it tears it out, whatever the weapon. |
| Green Wood | Minor | 1 | Torn pits stand longer and seed one deeper. |
| The Fairy Wind | Keystone | 5 | Where an enemy is torn out, a small whirlwind rises and wanders for 4 s, making what it passes Unsteady and Seeded, so tearing one out roots the next (on purpose past the no-cascade rule). |

**Duo spell: Thistledown** (the crown, 5 Shards). Movement. Cooldown 9 s. You come apart into a drift of seed-down
and blow 7 m along the stick in 0.4 s, untouchable, passing through enemies. Each one passed is Seeded two deep and
Unsteady two deep, so it roots as you go by. You form again with a ring of flowers 2 m across at your feet. The ring
stands 4 s, counts as something grown, and heals you 2 a second while you stand in it. What it looks like: the body
streams away as hundreds of white tufts and a few leaves, then gathers. How it rewards steel: a light blow within 1 s
of forming again counts as a shove, so it can tear out what you just rooted.

Engine needs: a **rooted** state (cannot move or be shoved, can still attack): Hold stops attacks too (16.6). A dash
that passes through enemies (15.9; the drill form does it).

---

### Ice and Earth (Chill + Tremor), distance 3

**The image.** Frost heave: water freezing in the soil lifts the ground, and over many winters it sorts stones into
rings, as on Svalbard's patterned ground. The Celtic Cailleach is winter and stone in one body: her staff freezes the
ground, mountains fell from her creel, and each summer she turns to grey stone. The Norse mountains are the bones of
Ymir, the first frost giant. In both lands winter lives in the stone.

**Reaction: Frost Heave.** When a shaken enemy is chilled, or a chilled enemy shaken. The cold gets into the ground
under it. After 0.4 s the floor heaves up beneath it: it is thrown up for 1 s, takes 12 damage and 25 stagger, and the
mound stays for 6 s as a low hump of rimed earth 1.5 m across that nothing walks through. What the player sees: the
floor bulges, cracks run outward with white frost in them, and clods and ice needles stand up.

**Shape A: Sorted Stones.** The heave pushes everything else within 3 m out to a ring of stones that stands 4 s.
Nothing crosses the ring, so the heaved enemy is left alone with you in a cleared circle. **Shape B: Pingo.** The mound
has a core of ice. 2 s later it bursts: 24 damage and two Chill to everything within 2.5 m. Lure them onto it.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Hard Frost | Major | 3 | Heavy blows on a chilled enemy shake it; heavy blows on a shaken enemy chill it. |
| Sorted Stones or Pingo | Shape | 3 | Opens the two shapes of Frost Heave. |
| Frozen Footing | Major | 3 | A perfect guard while you stand within 2 m of a mound heaves the ground under the attacker. |
| Permafrost | Minor | 1 | Tremor does not fade from a chilled enemy. |
| Winter in the Bones | Keystone | 5 | A Frozen enemy that is shaken stays frozen twice as long, and shattering it heaves the ground under every enemy within 4 m. |

**Duo spell: The Hag's Staff** (the crown, 5 Shards). Breaker. Cooldown 12 s. You strike the ground and hammer the
frost into it. A disc 6 m round you goes hard and white for 6 s. Each second every enemy on it is given one Chill and
one Tremor, so it heaves when the two meet (once each, by the reaction's rule). What it looks like: one hammer blow,
frost ferns racing out to a crisp ring of rime, the floor's cracks filling white. How it rewards steel: while it lasts,
each heavy blow you land on the frozen ground drives frost under the one struck: 25 more stagger and a small heave
that throws it up for 0.5 s.

Engine needs: mounds and stone rings are the barriers Stone Rise and the ring form already make. Little new.

---

### Water and Metal (Soaked + Sundered), distance 3

**The image.** Salt water in broken armour rusts it. In the Iliad, Hephaestus, thrown out of heaven, fell into the sea,
and two sea goddesses kept him nine years in a cave with the Ocean flowing round it, where he forged. The smith who
caught the swift war god in a net of chains as fine as spider's web meets the fisher who pulls up islands. Their
meeting is a net.

**Reaction: Rust.** When a notched enemy is Soaked, or a Soaked enemy notched. The wet cuts rust. While it is Soaked
its notches do not fade, and the rust spreads by itself: one more notch every 2 s, up to five (two past Sundered's
usual three). What the player sees: an orange-brown bloom creeping out from each notch over its armour, flakes falling,
and a brown drip line on the floor where it walks.

**Shape A: Seized Joints.** Each notch of rust slows its walk 6% and its turning 10%, so a rusted enemy cannot turn to
face you in time: go round it. **Shape B: Flaking.** A heavy blow knocks the rust off in a shower. It spends the
notches, and each one throws flakes at enemies within 3 m: 6 damage, one notch and Soaked.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Salt Edge | Major | 3 | Heavy blows lay one notch and Soak. |
| Seized Joints or Flaking | Shape | 3 | Opens the two shapes of Rust. |
| The Plate Falls | Major | 3 | At five notches of rust, the next heavy blow knocks a plate off: 30 damage, and for 6 s the enemy takes weapon blows as if notched five deep, wet or dry. |
| Brine | Minor | 1 | Soaked lasts longer on a notched enemy. |
| The Wreck | Keystone | 5 | A rusted enemy that dies leaves its armour on the floor as a wreck 1.5 m across for 8 s. It Soaks and notches whatever stands in it, and your heavy blows struck from inside it are not broken by blows taken. |

**Duo spell: The Weighted Net** (the crown, 5 Shards; the invisible Smith's Net is the Metal and Dark duo spell, so
this is the fisher's half of the meeting). Opener. Cooldown 12 s. You throw a net of fine wet chain up to 8 m. It spreads over a disc 3.5 m across and falls. Everything under it is Soaked, notched once and held for 1.2 s
(elites and gods less). The net then lies on them for 5 s, and they move 40% slower under it. What it looks like: a
glinting mesh of fine chain beaded with water, its edge weighted with shell sinkers, water running from every knot as
it settles. How it rewards steel: a heavy blow on any netted enemy hauls the net, and every other enemy under it is
dragged 2 m toward the one struck and notched again.

Engine needs: Rust's notches past three (an `AfflictionDepth` for Sundered alone). A net that follows the enemies it
holds.

---

### Life and Light (Seeded + Marked), distance 3

**The image.** Green things turn toward the light. Pliny wrote that the druids cut mistletoe from the oak with a
golden sickle on the sixth day of the moon and held it an antidote to every poison. In later tellings of the Ramayana,
Hanuman found the herbs of Dronagiri shining in the dark, and could not tell which one would save a life, so he
carried the whole mountain. The Celtic grove meets the Indian light in a herb that shines.

**Reaction: Shining Herb.** When a Seeded enemy is Marked, or a Marked enemy seeded. The seed turns to the light and
sprouts outside it: a shining herb springs up on the floor at its feet, on the side facing you, and stands 6 s (three
at most at once). **Walk over it** to pick it: you heal 8 and your next weapon blow is critical. Left unpicked, it
flowers at the end and Marks and Seeds every enemy within 3 m. So each one asks a small question: step in and take
it, or leave it to spread. What the player sees: a sprig of leaves whose veins glow like lit sap, a thin ray standing
from it, leaves streaming into you when it is picked.

**Shape A: Bitter Leaf.** The herb grows on the enemy, not on the floor, and your next critical blow on it picks it:
the heal without the walk, for those who never leave a target. **Shape B: Seed of Light.** No herb. The seed bursts on
the next critical blow as a flare that Marks and Seeds every enemy within 4 m.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Sixth Moon | Major | 3 | The blow that ends a chain on a Seeded enemy Marks it. |
| Bitter Leaf or Seed of Light | Shape | 3 | Opens the two shapes of Shining Herb. |
| Open to the Sun | Major | 3 | A perfect guard against a Marked enemy seeds it three deep. |
| Long Day | Minor | 1 | Herbs stand 3 s longer before they flower. |
| The Whole Mountain | Keystone | 5 | Picking a herb draws every other standing herb to you across the floor, and each one picked after the first heals twice. |

**Duo spell: Life-Herb** (the crown, 5 Shards; the herb of Dronagiri that gives back life. Mistletoe is the Scythe and
Life Pair Bond strike). Opener. Cooldown 11 s. You throw a shining herb into the enemy faced within 8 m. It roots
there for 8 s and seeds the enemy three deep. While it grows, every weapon blow on that enemy is critical, and each
critical blow opens a lit leaf on the herb. At six leaves, or when the host dies, or when the time ends, the leaves
fall: each heals you 3 and Marks and Seeds every enemy within 2.5 m of the host. What it looks like: a small bush
with veins glowing like lit sap standing out of the enemy's shoulder, leaves opening one by one with a glint, then
dropping in a little rain of rays. How it rewards steel: it is six critical blows held in a herb, and only the blade
picks them.

Engine needs: **pickups** the player walks over (new). Life-Herb is the `brand` form, with the leaves as its count.

---

### Earth and Dark (Tremor + Cursed), distance 3

**The image.** Both lands have curses that turn people to stone. In Oxfordshire a king and his men were turned to
standing stones that cannot be counted. In later Indian tellings Ahalya, cursed by her husband, lay as a stone until
Rama's foot brushed it. Under both lands there is a dark below the ground: the Celtic Otherworld inside the hills, and
the "black boar" of the Taittiriya Aranyaka that raised the earth out of the deep waters.

**Reaction: Turned to Stone.** When a Cursed enemy is shaken, or a shaken enemy Cursed. Stone climbs it from the feet
up. Over 2 s it slows (30%, then 60%) to a stop. Then it stands as stone for 1.5 s: held, taking double stagger from
weapon blows and half damage from spells. A heavy blow on the stone breaks it out at once for 50 stagger, often
enough to make it kneel. Where Thermal Shock is about damage, this is about balance and the finisher. What the player
sees: grey, lichened stone growing up the legs with black seams in it and dust falling, then slabs breaking away.

**Shape A: The King's Men.** When the stone breaks, the curse moves on: the nearest enemy within 5 m begins to turn,
so a crowd falls one at a time. **Shape B: Grave Marker.** A turned enemy that dies leaves its stone standing for 10 s.
It blocks the way, and every enemy within 3 m of it is Cursed every 2 s.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Grave Dirt | Major | 3 | Heavy blows on a Cursed enemy shake it; the blow that ends a chain on a shaken enemy Curses it. |
| The King's Men or Grave Marker | Shape | 3 | Opens the two shapes of Turned to Stone. |
| Broken Idol | Major | 3 | A finisher on an enemy just broken out of stone heals you 10 and Curses every enemy within 5 m. |
| Heavy Curse | Minor | 1 | Stone climbs half a second faster. |
| Stone Does Not Forgive | Keystone | 5 | An enemy turned to stone once keeps its Tremor and its Curse until it dies. |

**Duo spell: Stone Underfoot** (the crown, 5 Shards; Ahalya lay as stone until a foot touched her, and here the foot
does the reverse. Travel under the ground is Earth's own Under the Mound). Movement. Cooldown 10 s. You stride up to
8 m to the enemy faced in 0.3 s, untouchable, leaving grey dust footprints, and set your foot on its shadow. It is
Cursed and shaken two deep, and turned to stone at once, without the slow climb (gods are only slowed 60% for
1.5 s). Every other enemy you passed within 1.5 m is slowed 30% for 2 s as grey stone flecks its legs. What it looks
like: a line of grey footprints, then stone bursting up the body from where the foot came down. How it rewards steel:
the heavy blow that breaks it out within 1.5 s strikes for double stagger, which is most often a kneel.

Engine needs: a dash that ends at an enemy (the `step` form); Turned to Stone set at once, past its climb.

---

### Metal and Poison (Sundered + Poisoned), distance 3

**The image.** Quicksilver is a metal and a poison in one. China's First Emperor sought long life in elixirs of
mercury and cinnabar, and Sima Qian says his tomb holds rivers of quicksilver. The Greeks dipped arrows in the Hydra's
blood, and Talos's one vein was closed with a bronze nail. The Greek smith meets the Chinese alchemist where the armour
is broken: poison goes in through the cracks.

**Reaction: Quicksilver.** When a notched enemy is poisoned, or a poisoned enemy notched. Beads of mercury run into the
cracks. While it carries both, each weapon blow on it makes its poison bite at once, at its present depth, besides the
usual beat (no more than once every half second). The doses are not spent. What the player sees: silver beads in its
cracks jumping and spattering with each blow, droplets bouncing on the floor, a grey-green fume.

**Shape A: Heavy Silver.** The mercury pools under it as a silver puddle 1.5 m across. It moves 30% slower while it
stands in the puddle, and any enemy that steps in is dosed and notched. **Shape B: Running Silver.** When it dies, its
mercury runs along the floor to the nearest enemy within 6 m, a visible silver rivulet, and gives it every dose and
notch it carried.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Envenomed Edge | Major | 3 | Heavy blows lay one notch and one dose. |
| Heavy Silver or Running Silver | Shape | 3 | Opens the two shapes of Quicksilver. |
| The Nail Pulled | Major | 3 | A finisher on a Quicksilver enemy pulls the nail: all its poison bites at once, twice. |
| Cinnabar | Minor | 1 | Notches last as long as the poison does. |
| The Undying Drink | Keystone | 5 | Your flasks are cut with quicksilver: drinking one throws a ring of silver drops 4 m round you that poison three deep and notch everything, but each flask heals a fifth less. |

**Duo spell: Ichor** (the crown, 5 Shards; Talos's one vein, closed by a bronze nail, and the ichor that ran out like
molten lead when it was pulled. Needle grids in the floor are the Poison god's own weapon and attack, so they are left
to the Hook Swords). Counter. Cooldown 11 s. You raise the guard for 1.2 s. The first blow that lands on it is turned,
and your answer pulls the nail: the attacker is notched to full, poisoned three deep, and bleeds silver for 4 s, its
poison biting every 0.5 s instead of on its beat. When the bleeding stops, every dose it carries bites at once. If
nothing strikes, half the cooldown comes back. What it looks like: a bronze nail popping from the attacker's ankle
with a glint, and silver ichor running down its leg and beading on the floor. How it rewards steel: each weapon blow on
the bleeding enemy adds a dose, so the 4 s are for pressing, and the last bite is as big as the pressing was.

Engine needs: Quicksilver is data (`AnyHit`, `Needs` and `Needs2`, `Do = Bite`, a cooldown). Running Silver is `Pass`
with every stack. Ichor is the `parry` form with a timed state that quickens one enemy's poison beat.

---

### Light and Fire (Marked + Burn), distance 3

**The image.** A burning glass: sunlight drawn to one point sets things alight. In the Mahabharata Agni, the starving
fire god, asked Arjuna to guard his feast while he ate the Khandava forest, and gave him the bow Gandiva to do it with.
The Egyptian sun's rays end in hands. The light aims and the fire eats, and the fire stays flame, never light.

**Reaction: Kindling Point.** When a burning enemy is Marked, or a Marked enemy set burning. The brackets on it draw
in to one point of gathered sun on its body, and smoke curls up from it. Each critical blow on it deepens its Burn by
one. The third, the last blow the Mark allows, sets it alight at the point: all its Burn bursts out as a tongue of
flame 2 m tall, 10 damage for each Burn, and it goes on burning two deep. What the player sees: four brackets closing
to a small star, a ring of char spreading round it, then real flame standing up out of the body.

**Shape A: Burning Glass.** After the Mark is spent the point stays on the enemy for 4 s and deepens its Burn by one
each second, a lens held still. **Shape B: The Fire Is Hungry.** The tongue of flame does not stay on the enemy. It
leaps to the nearest other enemy within 6 m and Marks it and burns it two deep, so the point moves on through a crowd.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| The Sun Points | Major | 3 | The blow that ends a chain on a burning enemy Marks it. |
| Burning Glass or The Fire Is Hungry | Shape | 3 | Opens the two shapes of Kindling Point. |
| Noon Guard | Major | 3 | A perfect guard against a Marked, burning enemy sets it alight at the point at once. |
| Clear Sky | Minor | 1 | A Mark lasts one blow longer on a burning enemy. |
| The Feast | Keystone | 5 | When a Kindling Point burst kills, every burn within 8 m deepens by one and you gain 10 mana. |

**Duo spell: Hands of the Sun** (the crown, 5 Shards). Opener. Cooldown 12 s. A burning sun rises over you for a
breath, and up to four long rays reach down to the four nearest enemies within 9 m. Each ray ends in a hand of flame
that grips: the enemy is held for 1 s, burned two deep and Marked, so every one of them carries a Kindling Point. What
it looks like: a burning ball with flame licking off its rim (not a white orb), rays as long streaks of heat-shimmer
and flame, and open hands of fire closing on the bodies, after the sun-disc of Amarna. How it rewards steel: the hand
lets go when your first blow lands on its enemy, and that blow is critical and deepens the Burn by two.

Engine needs: nothing beyond the reaction itself: `Hold`, `Afflict` and a form for the rays and hands.

---

### Dark and Lightning (Cursed + Static), distance 3

**The image.** In 930 lightning struck the Seiryōden of the palace at Kyoto and killed the courtiers who had driven
Sugawara no Michizane into exile. The court decided his grudge had become thunder, and made him a god to quiet it. In
India the thunderbolt was made from the bones of the sage Dadhichi, who gave up his life for it. Volcanoes make
lightning inside black plumes of ash. The Japanese storm meets the Indian dark in a grudge that comes down as
lightning.

**Reaction: The Grudge.** When a Cursed enemy carries two Static or more, or a charged enemy is Cursed. A small black
cloud of ash gathers over its head, lightning crawling inside it. The next blow it lands on you, or on your guard,
calls the cloud down on it: a black bolt that discharges all its Static at double and stops it where it stands for
0.6 s. If it dies first, the cloud drifts to the nearest enemy within 6 m and waits there. Once per enemy every 8 s.
What the player sees: a sooty knot of cloud over one head, flickering. Read it and guard the blow you know is coming.

**Shape A: First Strike.** The cloud does not wait for its blow. Your first heavy blow on the enemy calls it down, for
those who would rather not be struck. **Shape B: Ash Plume.** The cloud grows: each blow you land on the enemy adds a
flash inside it, up to five. When it comes down it falls as a storm: one bolt for each flash, shared among the enemy
and everything within 4 m.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Grievance | Major | 3 | A blow held on your guard against a Cursed enemy lays two Static on it. |
| First Strike or Ash Plume | Shape | 3 | Opens the two shapes of The Grudge. |
| The Exile Returns | Major | 3 | The Grudge's bolt heals you 1 for each Static it discharged and Curses everything within 3 m. |
| Long Memory | Minor | 1 | A Curse on a charged enemy lasts as long as its Static does. |
| Given Bones | Keystone | 5 | A discharge on a Cursed enemy costs you 2 health (left grey) and lands twice as hard. |

**Duo spell: The Exile's Grudge** (the crown, 5 Shards). Counter. Cooldown 12 s. A black cloud stands over you for
3 s. Every enemy whose blow lands on you or on your guard in that time is tied to it by a thread of black lightning.
Then the cloud breaks: a bolt falls on each enemy tied to it, for 12 damage plus twice what that enemy dealt you (60
at most), three Static and a Curse. Blows taken inside still hurt, so guard them. What it looks like: a storm of ash
over your head, thin forks reaching out to each attacker's head, then every bolt falling at once in black smoke. How
it rewards steel: a blow held on the guard counts double toward what is paid back, so the guard is how the grudge
is fed.

**Duo surge: Ash and Thunder** (40 mana, 10 s). A plume of ash streams up off your shoulders, see-through and never
hiding you. Every blow you land lays one Static. Every third blow, a bolt from the plume strikes the nearest Cursed
enemy within 8 m for 12 and discharges it. Each Cursed enemy killed while it lasts heals you 6 more than the Curse
already does, and adds 1 s to the surge.

Engine needs: The Grudge is mostly data (`Struck` and `Guard` name the attacker, `Needs` and `Needs2`, `SetOff`); the
cloud that moves on at death is `Pass`. The Exile's Grudge needs a tally kept per enemy (15.9).

---

### Poison and Wind (Poisoned + Unsteady), distance 3

**The image.** Each spring the westerlies carry yellow dust from the Gobi and the Loess Plateau across the sea to
Japan, and pick up what the cities give it on the way. Twice a fleet that sailed from China was broken by the wind
before Japan's shore. What the Chinese alchemist brews, the Japanese wind god carries, and turns back on whoever sent
it.

**Reaction: Ill Wind.** When a poisoned enemy is made Unsteady, or an Unsteady enemy poisoned. Its poison goes loose
like dust. Whenever it is shoved or thrown (a Gust, a heavy that shoves, a lift), it sheds one dose into a trail of
yellow-green grit along its path, 1.2 m wide, lasting 3 s, which doses each enemy in it once a second. What the player
sees: the thrown body pours a tail of dust like smoke behind it, and the grit settles slowly on the floor. Fire and
Wind (Carried Embers) throws fire downwind off the body; this is different: it lays a line along the path the enemy
travels.

**Shape A: Turned Back.** No trail. The dust goes with the body, and where it lands it bursts: every poisoned enemy
within 3 m bites at once. Throw the bearer into the crowd. **Shape B: Dust Bank.** The trail does not settle. It
drifts on the way the enemy was thrown, 2 m a second for 4 s, as a low bank of dust that makes Unsteady what it
passes.

**Duo branch**:

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Spring Dust | Major | 3 | Heavy blows dose what they shove, and make it Unsteady. |
| Turned Back or Dust Bank | Shape | 3 | Opens the two shapes of Ill Wind. |
| Gust at the Heel | Major | 3 | A perfect dodge leaves a gust behind you that throws the attacker 3 m, so a poisoned attacker trails at once. |
| Fine Dust | Minor | 1 | Trails last 2 s longer. |
| No Still Air | Keystone | 5 | A poisoned, Unsteady enemy cannot stand still: every 2 s the wind shoves it 1 m away from you, and it sheds a trail each time. |

**Duo spell: The Yellow Gale** (the crown, 5 Shards; the wind bag is Wind's own counter, Bag of Winds, and drawing
the enemies themselves in is Wind's Indrawn Breath, so this one drinks only the poison). Detonator. Cooldown 12 s.
**Hold the button**: the wind turns round you and drinks for up to 1.5 s. Every poison cloud and trail within 6 m is
drawn into it, and every poisoned enemy within 6 m gives up half its doses as a stream of green dust that runs to
you; the enemies themselves stay where they are. **Let go**: the drunk dust goes out as a gale, a cone 9 m long and
60 degrees wide. Everything in it is thrown 4 m and made Unsteady three deep, and the doses the wind drank are shared
among them and bite at once. What it looks like: streams of yellow-green dust spiralling in round the player, then a
gale of streaked grit, as the spring dust crosses the sea. How it rewards steel: each weapon blow you land in the 4 s
before casting adds two more doses to the gale.

Engine needs: drawing stacks off enemies into a store (new; `Pass` only gives). The held button is 16.7.

---

## Engine needs, together

- **For every pair: a reaction trigger.** It fires when an enemy comes to carry both of a pair's afflictions, with a
  timer kept per enemy (once every 6 s). Per-enemy state is listed as not yet possible (15.9). Several states above
  (Cracked, rooted, the Grudge's cloud, the Kindling Point) are a timed flag on one enemy.
- A **second press** of a slot spell: Fire-Setting (16.2 covers only the surge).
- A **rooted** state: cannot move or be shoved, can still attack (Windthrow). 16.6 says Hold has to stand in for it.
- **Pickups** the player walks over (Shining Herb).
- **Taking stacks off** an enemy into a store (The Yellow Gale); a **tally per enemy** (The Exile's Grudge).
- **Sundered past three** (Rust).
- **A quickened poison beat** on one enemy for a time (Ichor).
- A dash through enemies (Thistledown), near `drill`.

## Sources

- Snorri Sturluson, *Gylfaginning* 5 to 8 (Muspelheim, Niflheim, Ginnungagap, Ymir): Wikipedia, "Muspelheim" and
  "Niflheim"; vikingr.org, "Ginnungagap".
- Fire-setting and its use in Egypt's Nubian gold mines (Diodorus Siculus): Wikipedia, "Fire-setting" and "Mining
  industry of Egypt"; showcaves.com, "Fire-setting".
- Genshin Impact's Melt, where order sets the multiplier (a principle noted and not copied): Genshin Impact wiki,
  "Melt".
- Lightning on the sea spreading across the surface: Royal Meteorological Society, "What happens when lightning
  strikes the ocean?"; NOAA, "Lightning and fish".
- Te lapa: Wikipedia, "Te lapa"; David Lewis, *We, the Navigators* (1972); Marianne George, "Polynesian Navigation and
  Te Lapa", *Time and Mind* 5.2 (2012).
- The let-go threshold and muscle tetany under current: Florida State University emergency medicine review,
  "Electrical Injuries in the Emergency Department".
- Ryūjin and the tide jewels, Hoori's lost fishhook: Wikipedia, "Tide jewels" and "Ryūjin".
- The fairy wind (*sí gaoithe*): Oxford Reference, *A Dictionary of Celtic Mythology*, "fairy wind"; Ireland's
  Mythology, "Sí Gaoithe".
- Fūjin and his wind bag, Sōtatsu's screens: Wikipedia, "Fūjin".
- Sorted stone circles and frost heave: Kessler and Werner, "A model for sorted circles as self-organized patterns",
  *JGR* (2001); University of Washington news, "How ice needles weave patterns of stones" (2021).
- The Cailleach's staff and creel: Wikipedia, "Cailleach"; Historic Environment Scotland blog, "Who is the Cailleach".
- Hephaestus in the sea cave: Homer, *Iliad* 18.395 to 405 (text at web.viu.ca); theoi.com, "Hephaistos". His net:
  *Odyssey* 8 (Kline translation, poetryintranslation.com).
- Pliny, *Natural History* 16.95 (the druids and mistletoe): Wikipedia, "Ritual of oak and mistletoe"; Philip
  Harland's translation.
- Sanjeevani and Dronagiri: Wikipedia, "Dunagiri (mountain)"; Mythlok, "Sanjeevani" (the shining herbs are in later
  retellings; not confirmed in Valmiki here).
- Ahalya turned to stone (Puranic and later tellings): Wikipedia, "Ahalya". The Rollright Stones and the Celtic lore:
  `docs/design/WORLD.md`.
- Varaha and the black boar of the Taittiriya Aranyaka: Wikipedia, "Varaha".
- The First Emperor's mercury and elixirs: Wikipedia, "Chinese alchemical elixir poisoning"; Archaeology Magazine
  (2025), on the source of the tomb's mercury.
- The Aten's rays ending in hands: Wikipedia, "Aten" and "Amarna art"; the Metropolitan Museum, "Akhenaten Sacrificing
  a Duck".
- The burning of the Khandava forest: Wikipedia, "Khandava Forest" and "Gandiva".
- Sugawara no Michizane and the strike on the Seiryōden: Wikipedia, "Sugawara no Michizane", "Tenjin (kami)" and
  "Kuwabara kuwabara".
- Dadhichi and the vajra: Wikipedia, "Dadhichi" and "Vajra".
- Volcanic lightning: Aplin, Houghton and Nicoll, "Electrical charging of ash in Icelandic volcanic plumes" (arXiv
  1404.6905); WorldAtlas, "What causes volcanic lightning?".
- Asian dust (kosa): Wikipedia, "Asian Dust"; JAXA Earth View, "Asian dust (KOSA) blows up from the Continent". The
  1281 typhoon: Wikipedia, "Kamikaze (typhoon)"; NOAA AOML hurricane blog.
- Hades duo boons (two gods' powers made one): Twinfinite, "Hades: All Duo Boons List".
