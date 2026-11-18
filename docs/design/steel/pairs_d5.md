# The pairs at distance 5: the furthest mixes on the ring

Eleven pairs whose elements sit opposite each other on the ring, so the least likely to be tried. Each is meant to be
found by accident and remembered: two substances that should not meet, and something new where they do. Written
2026-11-18 to `docs/design/steel/BRIEF.md`. Everything here is a **Proposal**; every number is a first guess.

## Index

| Pair | Reaction | Duo spell (role) | In one clause |
|---|---|---|---|
| Fire and Life | Firebloom | Need-Fire (opener) | Fire opens the seed: burning seeds fly out and grow into flame lilies |
| Lightning and Earth | Earthed | The Capstone (breaker) | The charge goes to ground as a burnt fern across the floor that strikes everyone standing on it |
| Wind and Metal | Sickle Wind | Three Weasels (movement) | The wind tears off the broken plates and whirls them round the enemy as blades |
| Ice and Light | Mock Suns | Sunstone (counter) | Two mock suns hang in plates of ice either side of it, and a blow from beside one is critical |
| Water and Dark | Drowned Weight | Under the Black Water (movement) | The curse turns the water on it black and heavy: it swells, slows, takes more, and bursts in black water when it falls |
| Life and Poison | Rot Roots | The Stalks Rise (detonator) | Pale roots join the host to its neighbours, and every bite runs down them |
| Earth and Fire | Burnt Mound | Vitrified Wall (breaker) | The shaken floor throws up three stones, the fire heats them red, and your weapon bats them across the room |
| Metal and Lightning | Live Iron | Lightning Seeks Iron (movement) | Broken armour conducts: every blow arcs from iron to iron and breaks the armour further |
| Light and Wind | Golden Feathers | Sweep of Wings (opener) | The brackets of light break into feathers that ride the wind and mark whoever they reach |
| Dark and Ice | Stolen Warmth | Ice in the Blood (detonator) | The curse takes its warmth and the cold is what is left: each blow draws warmth out of it into you |
| Poison and Water | Fish Poison | Poison the Lagoon (opener) | The water runs off it fouled, and the heavily dosed float up stunned for the blade |

Duo surges: **When the Watch Sleeps** (Lightning and Earth), **Three Winters** (Dark and Ice).

How the eleven were kept apart: one spreads a mark (Light and Wind by the wind's direction) and one teaches the flank
(Ice and Light's mock suns), one ties enemies together (Life and Poison by roots; a leash between two enemies is the
Metal and Dark reaction, Black Chain, so Water and Dark swells one body instead), two make ground (Lightning and
Earth a burnt figure, Poison and Water a fouled slick), one leaves hot stones for the blade (Earth and Fire), one
makes a whirl, one an arc from iron to iron, one a garden, one a stolen warmth, one a swelling. No two share a look.
(Ice and Light, Earth and Fire, Metal and Lightning, Dark and Ice and Poison and Water were reworked by the pair
review of 2026-11-18.)

How the branches are built: each is four marks (the Shape mark that opens the two shapes, a minor, a major and a
keystone), then the duo spell as its crown. Prices follow the shared chart (`README.md`): Shape 3, minor 1, major 3,
keystone 5, the duo spell 5, a duo surge 5. Each reaction rests 4 s on one enemy (the shared default), and afflictions
a reaction lays start no reaction of their own unless an entry says so.

---

### Fire and Life (Burn + Seeded), distance 5

**The image.** Seeds that wait for fire: pine cones and banksias sealed shut with resin open only when a blaze passes,
and the burnt ground is the first to turn green. The Egyptians sowed grain in the wrapped body of Osiris so that the
dead god sprouted; the Gaels drove their herds between two Beltane fires to make the summer fruitful. Where the
sun-king's flame meets the antlered lord's grove, fire is what opens growth, not what ends it.

**Reaction: Firebloom.** Burn and Seeded on one enemy, in either order. The heat splits the seeds: one burning seed
for each Seeded stack (one to three) is thrown 2 to 3 m out and at once grows into a **flame lily**, a grown thing
that stands 5 s. A lily burns what stands within 1 m of it (3 damage and a stack of Burn every 0.5 s). The first time
the player passes within 1 m of a lily they pick its fire up: 3 health and one charged blow (Burn) on the blade. The
enemy's Seeded is spent; its Burn stays. Seen: seed husks bursting on the body with a puff of sparks, seeds trailing
smoke as they fly, lilies with a green stem and petals of real tongued flame, ash at their foot.

**Shape A: Between Two Fires.** The lilies grow in pairs, 3 m apart, facing each other across the enemy. Single
lilies give nothing; passing between the two of a pair heals 8 and charges the blade with two blows. The player is
drawn to fight in the gap. **Shape B: Ash Bed.** No lilies. The burn and the seed become one: while it is Seeded its
Burn does not fade, and when it dies the ash is sown, a bed of green shoots 3 m across for 6 s that heals the player 2
a second inside and seeds every enemy that steps in.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Seeds That Wait | Shape | 3 | Opens the two shapes of Firebloom. |
| Dry Pods | Minor | 1 | One more seed is thrown (four at most); lilies stand 2 s longer. |
| Kindled Guard | Major | 3 | A perfect guard made within 3 m of a lily makes it flare: 12 damage and two Burn in 2 m, and it stands 3 s longer. |
| The Burning Tree | Keystone | 5 | The first lily of each fight grows into a burning tree that stands 10 s and throws a burning seed at the nearest enemy every 2 s (Seeded and Burn). |

**Duo spell: Need-Fire** (opener). Cooldown 12 s. Two fires spring up 4 m apart on either side of the enemy faced
(within 7 m) and burn 6 s, joined by a line of sparks on the ground. Every enemy on the line between them takes a Burn
and a Seeded each second. While the player stands between them, each weapon blow that lands heals 2 and takes twice
its usual time off the running cooldowns. A blow that shoves an enemy across the line sets its Burn to full. Seen: two
fires of piled green branches, crackling and spitting, their smoke leaning toward each other over the gap. It rewards
steel by making the gap the place to stand and swing.

---

### Lightning and Earth (Static + Tremor), distance 5

**The image.** Lightning that goes to ground. Where a bolt strikes the earth its current runs through the soil as a
branching scorch like a fern and melts sand into tubes of glass; farmers in Britain and Ireland picked old stone axes
out of the furrows and called them thunderstones, fallen with the bolt. In Japan the thunder god of Kashima holds the
earthquake catfish down under a capstone, and the ground shakes only when he looks away. Here the bolt and the quake
are one stroke: the charge is driven into the ground and comes up under everyone.

**Reaction: Earthed.** Static and Tremor on one enemy, in either order. The Tremor grounds it: all its Static runs
into the floor at once and spreads as a branching figure of scorched lines 4 m round it. Every other enemy standing on
the figure takes 5 damage and 3 stagger for each stack spent, and one Tremor; the grounded enemy is rooted for 1 s.
The figure is drawn in 0.3 s and the scorch stays 3 s. Seen: a fern of burnt, glowing lines racing over the floor,
dust jumping off it, glass glinting in the lines.

**Shape A: Thunderstone.** The charge does not spread. It fuses into a knee-high stone of glassy rock where the enemy
stood, holding the stacks, for 6 s. A heavy blow on the stone, or any discharge within 4 m, throws the stored charge
as bolts at every enemy within 6 m (7 damage a stack, one Static each). The player chooses when the ground answers.
**Shape B: Step Current.** The figure spreads to 6 m and stays live for 3 s. Any enemy that moves on it takes 4 damage
and 6 stagger every half second; one that stands still is spared. Enemies who close on the player pay for every step.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Ground Strike | Shape | 3 | Opens the two shapes of Earthed. |
| Deep Earth | Minor | 1 | The figure is 1 m wider; the grounded enemy is rooted 0.5 s longer. |
| The Watch Kept | Major | 3 | A perfect guard against an enemy that carries Tremor lays two Static on it. |
| The Ground Answers | Keystone | 5 | Every rock the player raises holds charge. Static discharged within 4 m of one runs into it and out of every other rock they have standing: 7 damage a stack in 2.5 m round each. |

**Duo spell: The Capstone** (breaker). Cooldown 12 s. After 0.4 s a round-headed stone falls on the enemy faced
(within 7 m): 18 damage, 40 stagger, full Tremor, and it is pinned (held 2 s; elites and gods 0.8 s). The stone
stands 5 s. Each weapon blow on an enemy within 2 m of it calls a bolt down the stone into the ground: 6 damage,
12 stagger and one Static to everything within 2 m (once every 0.5 s). Seen: a weathered grey stone with a straw rope
round its waist, bolts crawling down it into the floor. It rewards steel: the pin is the window, the blows are the
bolts.

**Duo surge: When the Watch Sleeps.** 40 mana, 10 s. The god looks away and the ground is loose. Every heavy blow
that lands shakes the floor in 3 m round the enemy struck (one Tremor), and any enemy shaken while it carries Static is
Earthed at once. Heavy blows deal 8 more stagger. Seen: small bolts running down the player's legs into the floor,
the floor jumping at every heavy blow.

---

### Wind and Metal (Unsteady + Sundered), distance 5

**The image.** The wind that cuts. In the Japanese mountains a sudden whirlwind leaves clean cuts on travellers who
never felt them: the sickle weasel riding the gust with blades for claws; in western Japan it is plainly called the
wind sickle, and the sober explanation is grit and sharp debris carried round in the dust devil. On Lemnos the lame
smith worked his forge with twenty bellows that blew at his word. Here the wind takes the armour that the smith's
blows have broken and turns it into a storm of blades.

**Reaction: Sickle Wind.** Unsteady and Sundered on one enemy, in either order. The wind tears its loose plates off:
one shard for each Sundered notch, plus one (two to four), whirling round it in a ring of 1.5 m for 3 s. Each shard
cuts whatever it passes, the enemy itself included, for 3 damage and 4 stagger every 0.4 s. Seen: a tight dust devil
of grey grit with bent plates and rivets flashing in it, sparks where a shard touches flesh or steel.

**Shape A: Sent Downwind.** The whirl leaves its enemy and wanders 6 m down the last wind (the way the enemy was last
shoved; otherwise away from the player) at 2 m a second, cutting everything it passes and breaking a notch of armour
on each. The player aims it with Gust and shoves. **Shape B: Cut Unfelt.** The shards draw nothing at first. Each cut
leaves a thin pale line hanging on the body, and when the whirl ends they all open at once for half again what they
owed. The player has 3 s to add to the debt.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Loose Plates | Shape | 3 | Opens the two shapes of Sickle Wind. |
| Grit | Minor | 1 | One more shard; the whirl lasts 1 s longer. |
| Bellows | Major | 3 | A dodge that ends within 3 m of a whirl feeds it: one more shard, and its next cut lays Unsteady. |
| Wind Sickle | Keystone | 5 | The last blow of a light chain throws a sickle of wind 5 m along the swing: 6 damage, one Unsteady, and a notch torn from a Sundered enemy into a shard that joins the nearest whirl. |

**Duo spell: Three Weasels** (movement). Cooldown 10 s. Three presses within 4 s, after the tale of the three sickle
weasels that work together. **First**: a rush of wind 5 m to the enemy faced: 6 damage, 20 stagger, two Unsteady, and
it is thrown 2 m. **Second**: a pass straight through it to 3 m beyond, untouchable: 12 damage and a notch of
Sundered. **Third**: a step back out of reach that binds the wounds: 2 health for each weapon blow the player landed
since the first press (eight at most). Seen: three streaks of grey dust and a sickle glint at the end of each. It
rewards steel: the blows between the presses are what heal.

---

### Ice and Light (Chill + Marked), distance 5

**The image.** Sunlight through ice. Plate crystals drifting in freezing air bend the low sun into mock suns either
side of it, red on their inner edge; clear ice shaped by hand can focus the sun to light tinder. The sagas tell of a
sunstone that showed a navigator where the sun stood behind cloud, and a crystal of Iceland spar does split the light
to show it. Where the huntress's winter meets the champion's judging light, the ice becomes a lens: what is frozen is
seen through, and the light comes out the other side.

**Reaction: Mock Suns** (made the plain reaction by the pair review, which took the pair off the far-side cone that
Rime already owns). Chill (any depth) and Marked on one enemy, in either order. Two mock suns, small glints held in
flat ice plates, hang 2 m either side of the enemy for 4 s and turn with it. A weapon blow struck from beside one
(from the enemy's left or right, not its face) is critical, lays two Chill and does not spend the Mark. It teaches the
flank, not the spread. (Rays thrown at the nearest other enemy are Halo's Glint, and marks carried to others are
Light and Wind's feathers.) Seen: two glints beside the body, red on their inner edge, wheeling as it turns. Steel:
footwork round the enemy is the reaction.

**Shape A: Prism** (the old plain reaction). No mock suns. The ice in it goes clear: the next weapon blow on it is
critical and does not spend the Mark, and the light of that blow passes through the body and out the far side as a
fan of three rays, 6 m long and 30 degrees wide: 8 damage each, a Mark and a Chill on what they cross. Seen: the
frost on the body clears to glass; on the blow, three thin rays fan out behind it, red on the inside edge and blue on
the outside. **Shape B: Lens of Ice** (fire from ice). No rays: the light is gathered into the one body. While it is Frozen its Mark is
not spent by blows, and a shatter of a Marked, frozen enemy deals 60% more and throws its Mark onto every enemy within
4 m.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Through the Ice | Shape | 3 | Opens the two shapes of Mock Suns: Prism or Lens of Ice. |
| Wide Fan | Minor | 1 | The mock suns hang 2 s longer; with Prism, the fan reaches 8 m and holds five rays. |
| Bent Light | Major | 3 | A ray that meets ice the player made (Hoarfrost's crystals, a wall of ice, a frozen enemy) bends toward the nearest enemy and runs on 5 m. |
| White Night | Keystone | 5 | A perfect guard against a Marked enemy that carries three or more Chill freezes it at once. |

**Duo spell: Sunstone** (counter). Cooldown 10 s. The player holds up a block of clear ice spar for 1.2 s. The first
blow that lands on it is caught for nothing and split: five rays fan 8 m toward the attacker across 80 degrees,
12 damage and 20 stagger each, a Mark and two Chill. A perfect guard made through it (the blow landing in its last
0.3 s) freezes the attacker outright. If nothing strikes it, half the cooldown comes back. Seen: a palm-sized crystal
that doubles the world behind it; the struck blow breaking into banded rays. It rewards steel through the guard.

---

### Water and Dark (Soaked + Cursed), distance 5

**The image.** The black water that binds. Varuna, the Vedic lord of the waters and the night sky, holds a noose for
the guilty and swells them with water as punishment. In Polynesian telling the world comes out of Te Pō, the long
night, and the goddess of night waited at its edge for Maui. An octopus's ink, thickened with mucus, holds the
animal's own shape long enough to fool a hunter. Here water becomes the weight of debt: what is cursed and wet swells
with the dark sea, as Varuna's guilty swelled, and gives it back when it falls. (A rope tying two enemies together was
the first idea here; it is the Metal and Dark reaction, Black Chain, so this pair takes Varuna's other punishment.)

**Reaction: Drowned Weight.** Soaked and Cursed on one enemy, in either order. The curse turns the water on it black
and heavy, and for 5 s it swells with it: 20% slower, and it takes 15% more from weapon blows. When it dies or a
finisher lands on it while swollen, it bursts in a ring of black water to 4 m: 14 damage, 20 stagger, Soaked and
Cursed on everything reached. Seen: the wet sheen on the body turning to ink, the body swelling and dripping black,
then a ring of dark water thrown flat across the floor.

**Shape A: Drawn Under.** The black water pulls down: as it swells it sinks knee-deep into a pool of black water 3 m
across, held 1.5 s, and every other enemy in the pool is drawn toward it at 1.5 m a second. **Shape B: The Pull of
Night.** No burst. While it is swollen, each weapon blow on it draws every other Soaked enemy within 5 m 1 m toward
it, as the night sea draws all things back into itself.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Bound in Debt | Shape | 3 | Opens the two shapes of Drowned Weight. |
| Deep Swell | Minor | 1 | It stays swollen 2 s longer, and the burst reaches 5 m. |
| Held Under | Major | 3 | A perfect guard against a swollen enemy pushes it down knee-deep: held 1 s, and the attack it was making is lost. |
| The Debt Passes | Keystone | 5 | When a swollen enemy bursts, the nearest enemy the burst reaches swells at once (this breaks the no-cascade rule on purpose), and killing it heals 6 more. |

**Duo spell: Under the Black Water** (movement). Cooldown 10 s. Black water opens under the player and under the
enemy faced within 9 m at once. Both sink, the player untouchable for 0.5 s, and each rises out of the other's water:
the player where the enemy stood, the enemy where the player stood, Soaked, Cursed and held knee-deep for 1 s. Where
the player rose, the black water stays 3 s as a pool 2.5 m across that Soaks and Curses. Gods and elites are not
moved: the player rises behind them instead. The next weapon blow within 2 s deals half again. Seen: two holes of
dark water opening on the floor at once, two bodies going down and coming up in each other's places. It is the one
movement that moves the enemy too: drag the archer into the melee, or put yourself behind the shield. (Travel under
the ground is Earth's Under the Mound.)

---

### Life and Poison (Seeded + Poisoned), distance 5

**The image.** Rot that travels by root. Under a wood, fungus threads join every root to its neighbours, and what one
tree takes in can reach the next. On the high plateau a fungus fills a buried caterpillar and in summer a stalk grows
out of its head, so the Chinese call it "winter worm, summer grass". The druids cut mistletoe, the plant that lives
off another tree, with a golden sickle; the divine farmer Shennong tasted every herb to tell medicine from poison. Here
seed and venom are one thing: a growth that feeds on its host and reaches for the next.

**Reaction: Rot Roots.** Seeded and Poisoned on one enemy, in either order. Pale roots run out from its feet along
the ground to up to three other enemies within 5 m and hold 6 s. Each joined enemy is Seeded once as the roots arrive,
and every poison bite on the host also bites each joined enemy for half. Seen: grey-white threads crawling over the
floor, a yellow-green drip swelling along them with each bite.

**Shape A: Gathered In.** The roots drink instead of giving. Each bite on a joined enemy draws one dose out of it into
the host (to eight), so the poison of a crowd gathers in one body for the player's detonation. Drops run along the
roots and pale berries swell on the host. **Shape B: Summer Grass.** No roots. When the host dies a dark stalk grows
from its head and after 1.5 s bursts a puff of spores 4 m across: three doses and one Seeded on everything in it, and
6 health to the player if they stand inside.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Bad Ground | Shape | 3 | Opens the two shapes of Rot Roots. |
| Deep Threads | Minor | 1 | Roots reach 7 m and join one more enemy. |
| Tasting | Major | 3 | While the player stands on a root, each poison bite on a rooted enemy heals them 1. |
| The Host | Keystone | 5 | Whatever the player grows feeds on the poisoned: a poisoned enemy that dies within 3 m of something grown makes it stand 3 s longer and lay a dose of poison with every bite. |

**Duo spell: The Stalks Rise** (detonator). Cooldown 14 s. Every enemy within 9 m that is both Seeded and Poisoned
grows a dark stalk from its head; after 1.2 s each stalk bursts, spending both: 5 damage a dose and 8 a Seeded stack
in 2.5 m, the spores laying two doses on what is near. A weapon blow on an enemy while its stalk stands doubles that
stalk's spores. Seen: thin black stalks with swollen ochre heads, then brown spore clouds drifting low. It rewards
steel in the 1.2 s window.

---

### Earth and Fire (Tremor + Burn), distance 5

**The image.** Stone in the kiln. In Iron Age Scotland whole ramparts laced with timber burned so long and so hot that
their stones melted and fused into glass. The sun-king's city is mud brick, and the Egyptian potter god Khnum shaped
people from clay on his wheel. Here shaken earth meets fire and is fired: the stones the shaking throws up glow red,
and crack when they are struck, as the stones of the Celtic burnt mounds did.

**Reaction: Burnt Mound** (it replaces Baked Earth, whose star of glowing seams was Earthlight's in orange and whose
Fired Clay was Turned to Stone). In Bronze Age Ireland and Britain stones were heated in a fire and dropped in water,
and they cracked apart; nearly six thousand mounds of fire-cracked stone are left. Tremor and Burn on one enemy, in
either order; rest 6 s. The shaken floor throws up three stones round it, 1.5 m out, and its fire heats them red. They
lie 6 s, black with orange cracks and smoking, and burn what touches them (3 damage and a Burn each half second). Any
weapon swing that passes over a stone, or a heavy blow within 1.5 m of one, knocks it along the swing: it skips 7 m,
strikes the first enemy for 14 damage, 20 stagger, a Burn and a Tremor, and cracks into hot sherds (6 damage and a Burn
in 1.5 m). Seen: three glowing stones; a skipping stone leaving a line of sparks. Steel: the reaction is ammunition and
only the weapon fires it, so light blows bat each stone at a target.

**Shape A: Heaped Stones.** The three fuse into one boulder that a heavy blow rolls 8 m through a line: 20 damage, two
Burn and two Tremor to each it crosses. **Shape B: Fire-Cracked.** No batting: each stone cracks by itself when an
enemy steps within 1 m: 12 damage and a Burn in 2 m. A field to drive them across.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Fired Hard | Shape | 3 | Opens the two shapes of Burnt Mound: Heaped Stones or Fire-Cracked. |
| Hot Stones | Minor | 1 | The stones lie 2 s longer and skip 2 m further. |
| Timber-Laced | Major | 3 | Rock the player raises catches fire from a burning enemy within 2 m: it burns what touches it (4 damage and a Burn every half second), and when it ends it bursts in glass sherds (12 damage in 2.5 m). |
| Brick and Fire | Keystone | 5 | Heavy blows that land on a burning enemy lay Tremor; light blows that land on a shaken enemy lay Burn. The weave of light and heavy feeds both. |

**Duo spell: Vitrified Wall** (breaker). Cooldown 12 s. After 0.4 s a rampart of stone laced with burning timber
rises 4 m ahead, 6 m wide, throwing aside what stands there (12 damage, one Tremor). It blocks for 5 s and burns what
touches it. A heavy blow on it, or its end, fuses it: the timbers flare, the stone runs to black glass and bursts
outward from its face: 30 damage, 60 stagger, two Burn and two Tremor within 3 m. Seen: grey stones stacked between
charred beams with flame in the gaps, then the face slumping glossy and black before it breaks. It rewards steel: the
heavy blow picks the moment.

---

### Metal and Lightning (Sundered + Static), distance 5

**The image.** Bolts are forged. Hesiod's elder Cyclopes, named Thunder, Lightning and Bright, forged the thunderbolt
for the king of the gods; the smith's land remembers them as its murdered god. In nature, lightning is what makes a
lodestone: the bolt's field sets iron ore into a lasting magnet. Where the storm meets the forge, broken armour is a
conductor, and a bolt that finds iron goes on from iron to iron.

**Reaction: Live Iron.** Sundered and Static on one enemy, in either order. For 6 s every weapon blow on it arcs
only from iron to iron: to the nearest other Sundered enemy within 8 m, never to a clean one, for 35% of the blow,
and lays a notch where it lands. An arc that reaches an enemy already Sundered leaps once more (two leaps at most; one
arc every 0.3 s). The notches become a wire the player lays with heavy blows (the pair review moved it off
Lightning's own charged blade, which leaps to any second enemy). Seen: the cracked plates glowing blue-white
at their edges, arcs jumping plate to plate, molten sparks spitting where they land.

**Shape A: Drawn Iron.** The arcs magnetise: each enemy an arc reaches is drawn 1.5 m toward the one struck, plates
clattering, so a crowd folds in onto the blade. **Shape B: Forged Bolt** (the Cyclopes, named Thunder, Lightning and
Bright, forged the bolt). No arcs. Each weapon blow on it hammers one of its Static into a glowing iron spike standing
in it (three at most); a heavy blow tears them out as one thunderbolt thrown in a line 10 m: 12 damage a spike, 20
stagger, two Static to all on the line. (It replaces Draws the Bolt, which pulled every discharge into one body as
Flashover now does.)

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Open Plates | Shape | 3 | Opens the two shapes of Live Iron. |
| Bare Wire | Minor | 1 | Arcs carry 45% of the blow. |
| Three Smiths | Major | 3 | Every third weapon blow on a Live Iron enemy is answered from the sky: a bolt for 12 damage, 15 stagger and one Static. |
| Iron Calls the Storm | Keystone | 5 | Every blade the player throws (Flung Blade and the rest) turns in flight to the nearest enemy carrying Static and lands with two more. |

**Duo spell: Lightning Seeks Iron** (movement). Cooldown 10 s. The player becomes a bolt and in 0.25 s, untouchable,
goes to the farthest Sundered enemy within 10 m, cutting every enemy on the line for 10 damage, 15 stagger and two
Static; arriving: 16 damage and two Static. With no Sundered enemy it takes the enemy faced at half. A perfect guard in
the 2 s before the cast sends it on to a second Sundered enemy. Seen: a white-blue line scorched along the floor with
curls of metal shaving thrown up, sparks running off the weapon as the player stands again. It rewards steel: armour
is broken by heavy blows, and the guard doubles the road.

---

### Light and Wind (Marked + Unsteady), distance 5

**The image.** The golden-winged bird. Garuda, the eagle of the sun whose wingbeats sound like thunder and blow down
houses, went east with Buddhism and became Karura in Japan, the golden-winged bird whose flames stand as the halo
behind the Fudō statues. The champion's eagle and the summit's wind are one creature in two lands. Here judgement
takes feathers and goes where the wind takes it.

**Reaction: Golden Feathers.** Marked and Unsteady on one enemy, in either order. The brackets of light round it break
into three golden feathers that ride the wind 6 m downwind (the way it was last shoved; otherwise away from the
player), spread across 40 degrees. Each Marks the first enemy it touches and lays one Unsteady. The first enemy keeps
its Mark. Seen: three long feathers, gold at the edges of the vane, tumbling and gliding low over the floor and
leaving a small drift of dust.

**Shape A: Eagle's Sight.** No feathers fly. The eagle's sight stays on the enemy: the player's next dodge toward it carries
them all the way to it (up to 6 m) and ends in a cut, 10 damage and critical. **Shape B: Wingbeat.** The feathers
gather and beat once: a gust from the enemy outward to 3 m throws every other enemy away and Marks it.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Feathered | Shape | 3 | Opens the two shapes of Golden Feathers. |
| Riding High | Minor | 1 | Feathers fly 9 m, four of them. |
| Bane of Serpents | Major | 3 | A feather that reaches an enemy already Marked strikes it for 12 damage and 20 stagger. |
| Wings Behind | Keystone | 5 | Every perfect dodge raises a half ring of golden feathers behind the player that flies out 4 m, Marking and laying Unsteady on what it reaches. |

**Duo spell: Sweep of Wings** (opener). Cooldown 9 s. A great wing of layered gold feathers sweeps from the player's
side through 160 degrees, 6 m out: 10 damage, 25 stagger, a Mark and two Unsteady, and everything it strikes is thrown
3 m along the sweep, sideways rather than away. For 3 s each weapon blow on an enemy the wing threw lays another
Unsteady. Seen: real feathers, gold-edged, with a fan of dust raised under them. It rewards steel: it sets up the
critical blows of the Mark and the stagger of Unsteady together.

---

### Dark and Ice (Cursed + Chill), distance 5

**The image.** The cold when the sun is eaten. The Norse send wolves after the sun until they catch it at the world's
end, after three winters with no summer; in India the head of Rahu, who drank the nectar, swallows the sun again and
again, and it falls out of his cut throat. Both tell of what really happens in a total eclipse: the air drops several
degrees in a minute. Hel, who rules the cold dead, is half living and half dead in her body. Here the curse takes the
warmth out of the body, and the cold is what is left.

**Reaction: Stolen Warmth** (it replaces Frostbite, in which Dark did nothing of its own). Cursed and Chill on one
enemy, in either order; rest 6 s. The curse takes its warmth, and the cold is what is left. While both last its Chill
does not fade, and each of your weapon blows on it draws warmth out of it: one Chill on it, and 2 health to you (8 a
second at most). When it freezes, the last of its warmth is torn out at once: you heal 2 for every Chill it carried,
and it stays frozen 1 s longer. Gods give half. Seen: frost thickening on the body from the curse's sign while a pale
wisp of breath-smoke runs from it to you at each blow; at the freeze, one last cloud of breath torn out of it. Steel:
every blow is a draw, so the healing is paced by the weapon.

**Shape A: Cold Shade.** When an enemy whose warmth is stolen is shattered, its shadow stays behind as a figure of black ice for
4 s. Enemies within 2.5 m of it are chilled a stack a second, and a heavy blow on the shade shatters it a second time
(30 damage, two Chill in 3 m). **Shape B: The Long Night.** The warmth goes to the dark instead of to you: no health
is drawn, and every other enemy within 3 m of it gains a Chill each second while it is frozen. (It replaces
Swallowed Warmth, now the plain reaction's idea.)

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Let the Cold In | Shape | 3 | Opens the two shapes of Stolen Warmth: Cold Shade or The Long Night. |
| Deep Veins | Minor | 1 | Each blow draws 3 health, not 2 (12 a second at most). |
| Hel's Half | Major | 3 | Killing an enemy whose warmth is stolen Curses every enemy within 4 m and gives each two Chill. |
| No Summer | Keystone | 5 | Hunger costs no health while any enemy within 8 m is Frozen. |

**Duo spell: Ice in the Blood** (detonator). Cooldown 12 s. A sign of black frost is set on the enemy faced (within
8 m) for 6 s. Each of the next four weapon blows on it freezes a vein: 8 damage and two Chill. The fourth breaks it
from within, frozen or not: 35 damage, 50 stagger, and a Curse and two Chill on everything within 3 m. Seen: frost
branching from the sign across the body, one more limb going black with each blow. It rewards steel: four blows are
the fuse.

**Duo surge: Three Winters.** 40 mana, 10 s. Frost creeps over the floor round the player to 6 m and the light there
drops a step (the player and enemies stay plain to see). Every enemy that dies while it lasts freezes where it falls
as a statue of black ice for 5 s, chilling a stack a second within 2.5 m. A heavy blow on a statue shatters it:
25 damage, two Chill and a Curse in 3 m. Fights turn into a field of statues to be broken in the right order.

---

### Poison and Water (Poisoned + Soaked), distance 5

**The image.** Poison carried by water. On Pacific shores the seeds of the fish-poison tree are scraped on the rock
beside a tide pool and stirred into the shallows, and the stunned fish float up to be gathered by hand. The Chinese
alchemist's poison meets the fisher's sea, and the sea gives up what it holds. (The mercury rivers of the first
emperor's tomb belong to quicksilver, the Metal and Poison substance, and are left to that pair.)

**Reaction: Fish Poison** (made the plain reaction by the pair review; the old Fouled Water passed bites through a
pool, as Rot Roots does by root). Soaked and Poisoned on one enemy, in either order; rest 6 s. The water runs off it
fouled into a slick 3 m across for 5 s: dark water with a yellow-green sheen and rising bubbles. What stands in it is
Soaked. Any enemy in the slick that comes to carry four doses or more floats up stunned: lifted off its feet and held
1.5 s (elites 0.5 s; gods take 30 stagger instead), and the next weapon blow on it is critical and spends two doses
for 6 damage each. Seen: bodies lifting belly-up out of a sheen that ripples with every bite. Steel: the float is a
window for the blade, and the doses are what you built to reach it.

**Shape A: Rock Pool.** The slick is 1.5 m across and stuns at two doses: one enemy, often. **Shape B: The Ebb Takes
It.** The slick runs 6 m away from you as a stream 1.5 m wide over 1 s, stunning by the same rule along its length.

**Duo branch:**

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Bad Water | Shape | 3 | Opens the two shapes of Fish Poison: Rock Pool or The Ebb Takes It. |
| Wide Slick | Minor | 1 | The slick is 4 m across and lasts 2 s longer. |
| The Tide Brings It | Major | 3 | Riptide's wave, crossing a fouled slick, carries it: everything the wave carries takes two doses. |
| Fast Water | Keystone | 5 | Poison on a Soaked enemy bites twice as often and fades twice as fast. |

**Duo spell: Poison the Lagoon** (opener; it replaces The Hundred Rivers, whose quicksilver belongs to Metal and
Poison). Cooldown 12 s. You crush the fish-poison seed on the floor at the aim point (within 7 m): a slick spreads in
rings to 6 m across and stands 6 s, Soaking all in it. Each weapon blow you land on an enemy in the slick stirs the
water: every other enemy in it takes a dose. Any enemy in it that reaches four doses floats up as Fish Poison does
(past its rest, once each). Seen: grey husks scattered, the sheen spreading out in rings from where they fell. It
rewards steel: the blade stirs the poison through the crowd.

---

## Engine needs flagged

Beyond the reactions themselves (two afflictions on one enemy, and a hook when they meet):

- **A thing that stands and can be struck**: the thunderstone, the cold shade, the black ice statues (15.9 lists it
  as not yet possible beyond `wall` and `ring`).
- **A link between enemies** that passes poison bites: Rot Roots' roots (the leash that limits distance and passes
  blows is Black Chain's, in `pairs_d1_d2.md`).
- **A swollen state** on one enemy that slows it and raises damage taken, then bursts (Drowned Weight).
- **Swapping the player and an enemy** (Under the Black Water).
- **A hazard that follows an enemy**: the whirl of Sickle Wind, the mock suns.
- **Side of a blow** (Mock Suns' flank; Rime's facing side needs the same).
- **A cone or throw that starts at an enemy and faces away from the player**: Prism's fan, Golden Feathers, Wingbeat.
- **Things on the floor that a weapon swing knocks along**: Burnt Mound's stones (a skip along the swing).
- **An enemy lifted off its feet and held**: Fish Poison's float.
- **Spikes counted in one enemy and torn out as a line**: Forged Bolt.
- **Memory of the direction an enemy was last shoved**: Dust Devil and Golden Feathers.
- **A per-enemy count of damage owed**: Cut Unfelt (15.9's "per-enemy tally").
- **A move that passes through an enemy**: Three Weasels' second press and Lightning Seeks Iron (the `drill` and
  `step` forms are close).
- **A spell pressed three times in a row**: Three Weasels.
- **Chill that does not fade while a curse holds, and health drawn by each blow** (Stolen Warmth).
- **A trigger for the player walking through a grown thing**: Firebloom's lilies, Between Two Fires.
- **A hazard that bites only moving enemies**: Step Current.

---

## Sources

- Serotiny and pyriscence: https://en.wikipedia.org/wiki/Serotiny
- Corn mummies and Osiris beds: https://en.wikipedia.org/wiki/Corn_mummy ; Frazer, *The Golden Bough* ch. 39,
  https://sacred-texts.com/pag/frazer/gb03902.htm
- Beltane and driving cattle between two fires: https://www.newworldencyclopedia.org/entry/Beltane
- The Bennu of Heliopolis: https://en.wikipedia.org/wiki/Bennu
- Fulgurites: https://en.wikipedia.org/wiki/Fulgurite ; Lichtenberg figures on ground and skin:
  https://itotd.com/articles/4125/lichtenberg-figures/
- Takemikazuchi, the namazu and the kaname-ishi: https://en.wikipedia.org/wiki/Takemikazuchi ;
  https://en.wikipedia.org/wiki/Namazu ; https://en.wikipedia.org/wiki/Kaname-ishi
- The kamaitachi and the kazakama: https://en.wikipedia.org/wiki/Kamaitachi ; https://yokai.com/kamaitachi/
- Hephaestus's twenty bellows, Iliad 18: https://www.theoi.com/Text/HomerIliad18.html
- Sun dogs: https://www.atoptics.org.uk/halo/parhelia.htm ; https://en.wikipedia.org/wiki/Sun_dog
- Ice lenses and burning glasses: https://www.primitiveways.com/fire_from_ice.html ;
  https://en.wikipedia.org/wiki/Burning_glass
- The Viking sunstone: https://www.scientificamerican.com/article/did-vikings-navigate/ ;
  https://www.livescience.com/16831-viking-sunstone-crystal-compass.html
- Varuna and his noose: https://en.wikipedia.org/wiki/Varuna
- Hine-nui-te-pō and Te Pō: https://en.wikipedia.org/wiki/Hine-nui-te-p%C5%8D ;
  https://teara.govt.nz/en/photograph/28777/hine-nui-te-po-and-maui
- Cephalopod ink and pseudomorphs: https://en.wikipedia.org/wiki/Cephalopod_ink ;
  https://www.ncbi.nlm.nih.gov/pmc/articles/PMC4052311/
- Mycorrhizal networks (and their contested claims): https://en.wikipedia.org/wiki/Mycorrhizal_network ;
  https://www.pbs.org/wgbh/nova/nature/wood-wide-web.html
- Chinese caterpillar fungus: https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6059063/
- Vitrified forts: https://en.wikipedia.org/wiki/Vitrified_fort ;
  https://www.archaeology.org/news/6585-180501-hill-fort-vitrification
- The Cyclopes and the thunderbolt: https://en.wikipedia.org/wiki/Cyclopes ;
  https://www.britannica.com/topic/Cyclops-Greek-mythology
- Lodestones magnetised by lightning: https://en.wikipedia.org/wiki/Lodestone ;
  https://ntrs.nasa.gov/api/citations/19760021632/downloads/19760021632.pdf
- Garuda and Karura: https://en.wikipedia.org/wiki/Karura ; https://www.onmarkproductions.com/html/karura.shtml
- Rahu and the Norse sun-wolves: https://en.wikipedia.org/wiki/Rahu ;
  https://www.atlasobscura.com/articles/five-mythic-eclipse-monsters
- The temperature drop in a total eclipse: https://www.planetary.org/articles/eclipse-checklist
- The mercury rivers of the first emperor's tomb: https://en.wikipedia.org/wiki/Mausoleum_of_Qin_Shi_Huang
- The fish-poison tree: https://en.wikipedia.org/wiki/Barringtonia_asiatica
- Burnt mounds and fulachtaí fia (Burnt Mound): https://en.wikipedia.org/wiki/Burnt_mound ;
  https://en.wikipedia.org/wiki/Fulacht_fiadh
- Duo boons (the principle of two gods' powers made one): https://hades.fandom.com/wiki/Duo_Boons/Hades_II
- Project documents: `docs/design/STEEL_AND_ELEMENT.md`, `docs/GUIDE.md` sections 4 to 7, `docs/design/WORLD.md`,
  `docs/design/LOOKS.md`, `docs/UPGRADE_SCHEMA.md`, `docs/design/FORMS.md`.
