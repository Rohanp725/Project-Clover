# The pairs at distance 1 and 2: reactions, shapes and duo branches

Content for `docs/design/STEEL_AND_ELEMENT.md` sections 5 and 6, written 2026-11-18 to `docs/design/steel/BRIEF.md`.
Everything here is **Proposal**. Every number is a first guess. The weapon's own tree is the Way.

## Index

**How a reaction fires**: the shared rules this file assumes.

Distance 1 (built branches; image, reaction, shapes, and what to keep):
1. **Fire and Lightning, Plasma**: *Flashover*, the flame goes live and every discharge near bends into it.
2. **Lightning and Wind, Tempest**: *Thunder Gust*, the enemy is thrown and its charge goes off where it lands.
3. **Wind and Ice, Blizzard**: *Rime*, frost grows on the side facing you and your heavy blow blows it out the back.
4. **Ice and Water, Glacier**: *Supercooled*, the next blow freezes it outright.
5. **Water and Life, Bloom**: *Quickening*, the soaked seed splits and flowers at once.
6. **Life and Earth, Overgrowth**: *Split Rock*, roots wedge into the shaken ground and hold it, and a heavy splits the floor.
7. **Earth and Metal, Forge**: *Anvil Stone*, an anvil of stone heaves up at its back, and your heavy drives it onto it.
8. **Metal and Light, Halo**: *Glint*, its broken armour throws the light at the next enemy.
9. **Light and Dark, Eclipse**: *Diamond Ring*, dark closes over it and a bead of light breaks at the rim.
10. **Dark and Poison, Blight**: *Shut in the Dark*, a black ring seals it as a jar, and the poisoned that die inside feed it.
11. **Poison and Fire, Pyre**: *Fire Medicine*, the poison catches like powder and bangs.

Distance 2 (full branches):
12. **Fire and Wind**: *Carried Embers* blow downwind and light what they land by; duo spell *The Fire Breathes In*.
13. **Lightning and Ice**: *Sleet Charge*, the frost on it charges itself until the sky strikes; duo spell *Glass Storm*.
14. **Wind and Water**: *Capsize*, wet and off balance it goes down; duo spell *Ride the Squall*; duo surge *Open the Gourd*.
15. **Ice and Life**: *Winter Seed* sleeps under the frost and wakes as thorns at the thaw; duo spell *Green Under the Ice*.
16. **Water and Earth**: *The Ground Gives*, shaken wet ground turns to sand and swallows its legs; duo spell *The Island Breathes*.
17. **Life and Metal**: *Iron Thorns* grow out through its broken armour and fly off with each blow; duo spell *The Sown Men*.
18. **Earth and Light**: *Earthlight* breaks up through the cracks under it; duo spell *The Stone Keeps the Hour*.
19. **Metal and Dark**: *Black Chain* binds it to the next enemy and shares its wounds; duo spell *The Smith's Net*.
20. **Light and Poison**: *Burning Sap*, the sun wakes the sap, the poison holds back while criticals raise weals, then it all lands; duo spell *Bared to the Sun*.
21. **Dark and Fire**: *Smoulder*, the fire goes out of sight and deepens until it flares; duo spell *The Burning Lake*.
22. **Poison and Lightning**: *Nerve Fire*, each bite jolts it and breaks its wind-up; duo spell *Paper Seal*.

Then: **Engine needs**, one line each, and **Sources**.

---

## How a reaction fires (now the shared rule for all four pair files; see `README.md`)

- A reaction fires when an affliction lands on an enemy that already carries the other one of the pair. Order does
  not matter unless the entry says so.
- One enemy can show the same reaction once every 4 s (the rest is kept per enemy and pair; an entry may set its own,
  3 to 8 s). Afflictions that a reaction lays start no reaction of their own, so cascades stay readable; a shape or a
  keystone may break this on purpose and says so.
- A reaction spends nothing unless it says so.
- Holds, lifts and flinches on elites and gods follow the schema (0.4 of the time for a hold, 0.6 in the air; the
  colossus reels) unless the entry gives its own numbers.
- On each duo branch one mark is the **Shape** mark: it opens both shapes, chosen freely at any rest fire. Before it
  is bought the plain reaction works.
- Duo branch prices (shared): Shape 3, Minor 1, Major 3, Keystone 5 Shards; the duo spell 5; a duo surge 5.
- Where a built branch's spare spell "moves to" an element's tree below, that is an offer, not a placement: the
  rosters in `elements_a.md` and `elements_b.md` are full at six, so each such spell is cut unless the owner swaps it
  into a roster.

---

# Distance 1: the built branches

### Fire and Lightning (Burn + Static), distance 1

**The image.** A bolt heats the air it passes through to about 30,000 degrees, hotter than the face of the sun, and
turns it to plasma: the sun's fire and the storm's are one thing at that heat. The Egyptian sun-disc meets the drums of
the Japanese thunder god.

**Reaction: Flashover** (re-aimed by the pair review: flame is ionised and carries current, which is why wildfires trip
power lines; the engineers' word for it is flashover). Static meets Burn. The fire on it stands up violet at the root
and goes live for 4 s. Its own Static discharges at once through the flame (7 a stack in 3 m, 20 stagger), and none
of the Burn is spent. For the 4 s every discharge or arc within 6 m bends into its flame and bursts there again (5 a
Burn stack in 2.5 m, once every 0.8 s). Seen: real flame on the body, violet at its roots, and arcs bending sideways
across the floor into it. Steel: heavy blows on other charged enemies throw their charge into the flame, so the
burning one becomes the room's conductor.

**Shape A: Corona.** Nothing is spent. Violet flame crawls over it for 4 s (the fire on ships' masts in a storm), and
every 0.7 s an arc leaps from it to the nearest enemy within 5 m: 4 damage, 1 Burn, 1 Static. **Shape B: Fulgurite.**
The crack goes down instead of out: the ground under it fuses to a scorched glassy patch 2.5 m across for 4 s that
bites 4 a second with 1 Burn and 1 Static. A place to fight on rather than a burst.

**The built branch.** Cut Charged Embers (both afflictions are now laid by carrying both; the Shape mark takes its
place) and Fused Core (a plain number). Arc Fire folds into Corona; cut it. Keep Twin Break, Chain Reaction and
Meltdown (Meltdown as the branch's keystone). Re-aim Ionised Air: when Flashover fires, an arc leaps on through three
more. Ball Lightning is the duo spell (as section 11 has it), drawn as violet-cored plasma with real flame on its
skin, never "white fire"; Plasma Lance moves to Lightning's tree or is cut. The
crown Plasma (a heavy blow on a burning enemy bursts) now overlaps Flashover: make it the branch's top mark, its burst
a Flashover that spends nothing.

### Lightning and Wind (Static + Unsteady), distance 1

**The image.** Raijin and Fujin, thunder and wind, painted side by side on the same gold screen: the one pair here that
already belongs together. A dry storm's downburst throws dust and debris ahead of the bolt that follows it.

**Reaction: Thunder Gust.** Static meets Unsteady. A crack of thunder and wind throws it 3 m straight away from you;
where it stops, its Static discharges as if struck by a heavy blow (7 damage and 5 stagger a stack in 4 m), and
anything it hits on the way takes the discharge first. Seen: a white streak of dust and grass behind the body, a bolt
where it lands.

**Shape A: Thunderhead.** It is thrown up, not away (1 s in the air), and a bolt meets it at the top of its rise and
discharges it there, over the heads of the rest. **Shape B: Bolt from the Blue.** Nothing is thrown; the discharge runs
sideways downwind as an arc through up to three enemies on the line behind it, each taking the full discharge.

**The built branch.** Cut Charged Air (the Shape mark replaces it). Thunder Lift is now Thunderhead; cut it. Keep The
Sky Finds Them (it pays off Thunderhead) and Slip Current. Eye of the Storm is the duo spell; Storm Overhead moves to
Lightning's tree. Falling Thunder, begun by a finisher, becomes the branch's top mark.

### Wind and Ice (Unsteady + Chill), distance 1

**The image.** Rime grows into the wind: supercooled fog freezes on whatever it strikes, building feathers on the
windward side only. On Mount Zao in Japan whole firs become white giants that lean into the gale; the Japanese wind
meets the Norse winter.

**Reaction: Rime.** Chill meets Unsteady. A crust of white feathers grows on the side of it that faces you, for 5 s.
Your next heavy blow from that side (within 90 degrees) breaks the crust and the ice blows out of its far side: a cone
4 m long and 60 degrees wide, 10 damage, 2 Chill and 1 Unsteady to everything in it. A blow from behind it wastes the
rime. Seen: the crust from above is a bright crescent on one side of the body; the cone is a fan of white spray and
chips on the floor.

**Shape A: Rime Tree.** The crust never breaks. Every blow on it adds 1 Chill, and when it freezes it freezes standing
in its rime: 1 s longer than a freeze and its shatter strikes 25% harder. **Shape B: Spindrift.** The wind tears the
crust away at once by itself: the cone goes the moment the reaction fires, at half damage, no heavy blow needed.

**The built branch.** Cut Biting Wind (Shape mark). Keep Windchill, Cold Front, Sliding Ice. Avalanche is the duo spell;
Whiteout moves to Ice's or Wind's tree. Diamond Dust, begun by a shatter, becomes the top mark.

### Ice and Water (Chill + Soaked), distance 1

**The image.** Water can be cooled below freezing and stay liquid until a jolt or a speck of ice makes it freeze in a
rush. Under the sea ice, brine sinks in a hollow finger of ice that reaches the seabed and freezes the slow things
there. Ymir's blood became the sea; the Polynesian ocean meets the Norse frost.

**Reaction: Supercooled.** Chill meets Soaked. The water on it goes still and glassy below freezing, for 6 s. The next
weapon blow that lands freezes it outright, whatever its Chill (an elite takes 3 Chill instead, a god 2), and spends the
Soaked. Seen: the body's wet sheen turns grey-blue and stops dripping; the blow sends frost running over it from the
point struck.

**Shape A: Sea Ice.** When it freezes, the cold runs down into the floor and out in a web of frost: every Soaked enemy
within 4 m takes 2 Chill (this breaks the no-cascade rule on purpose: a Soaked one reached becomes Supercooled).
**Shape B: Hard Water.** Only a heavy blow freezes it, but that freeze lasts half again as long and its shatter strikes
30% harder.

**The built branch.** Cut Cold Water (Shape mark). Flash Freeze is now the reaction; cut it. Keep Icebreaker and Cold in
the Bones. The Wave Stands Still is the duo spell; Creeping Ice moves to Ice's tree. Weight of Ice, begun by the guard,
becomes the top mark.

### Water and Life (Soaked + Seeded), distance 1

**The image.** A seed that drinks swells until its coat splits. The Hawaiian Kumulipo begins in the slime of the deep,
"out from the slime come rootlets", and the coral polyp is the first life born; the Celtic holy wells feed the grove.

**Reaction: Quickening.** Soaked meets Seeded. The seed splits its coat at once: Seeded goes to full depth and a pale
flower opens on the enemy for 4 s. A heavy blow on it bursts the flower: you heal 10, and everything within 3 m is
Soaked and Seeded. Left alone it withers. Seen: a white flower on its back that the camera can find.

**Shape A: Floating Flower.** The flower drops to the ground under it as a lily 2 m across for 5 s; standing on it mends
you 3 a second. This is the Scythe build of section 11: stand in the flower and strike with the blade's tip. **Shape B:
Rootlets.** As it opens, white rootlets run through the wet to every Soaked enemy within 5 m and seed each to full.

**The built branch.** Cut Wet Seed (Shape mark). Flowering is now the reaction without its wait; cut it. Keep Drowned
Roots and Morning Dew. Strangling Vine is the duo spell, re-aimed at the flower (as built it had no water in it): it
rises under the nearest Soaked enemy and drinks, its Soaked is spent and Quickening opens on it at once, past the
rest, and a heavy blow on it while it is lifted bursts the flower for twice the heal. Lotus Step (a standing lotus
that cuts, not a movement) moves to Water's tree. Great
Lotus, begun by a flask in the surge, becomes the duo surge.

### Life and Earth (Seeded + Tremor), distance 1

**The image.** A seed in a crack becomes a tree that splits the rock: roots wedge stone apart. The giant raised the
stones and the antlered lord grew the grove; they were allies before they forgot.

**Reaction: Split Rock** (made the plain reaction by the pair review: the split is the image, and it was buried in a
shape). Seeded meets Tremor. Roots go down through its feet and wedge into the shaken ground: it is held 2 s, and a
heavy blow on it while it is held splits the floor round it, throwing up shards in 3 m: 10 damage, Tremor 1 and
Seeded on each. Seen: roots driving into a star of cracked flags round its feet, then the floor heaving apart. Steel:
the heavy blow is the wedge.

**Shape A: Taking Root** (the old plain reaction, now the short hold). Held only 1 s, and no split: the floor cracks
round it at once, Tremor 1 on every other enemy within 2.5 m. **Shape B: Root Web.** The roots run on underground to
every Seeded enemy within 6 m, a line of lifting soil to each: each is held 0.5 s and given Tremor 1.

**The built branch.** Root Grip is now the reaction; cut it and put the Shape mark in its place. Keep Moss on Stone and
Living Rampart (keystone). Its mark Briar Patch shares a name with Life's tap spell: rename it Thorns Take Root. Stone
Oak is the duo spell; Thorn Wall moves to Earth's tree. The Ground Wakes, begun by the surge, becomes the duo surge.

### Earth and Metal (Tremor + Sundered), distance 1

**The image.** Ore lies in the stone, and iron was first drawn out of bogs; the Celtic giant's stones are the anvil and
the Greek smith's hammer falls on them. Struck hot iron rings and throws off flakes of scale.

**Reaction: Anvil Stone** (made the plain reaction by the pair review: hammer on anvil is the forge). Tremor meets
Sundered. An iron-faced stone heaves up at its back, away from you, and stands 5 s, "stone at its back" for Hammer and
Anvil. Your next heavy blow on it drives it onto the anvil: its Sundered goes to full depth, and a ring of sparks runs
out 3 m: 10 damage, 15 stagger and a notch on each enemy it reaches. Seen: a grey stone with an iron face rising
behind the body, then one bright orange ring on the floor and a shower of sparks, never a plain flash. Steel: the ring
is the heavy blow's payoff.

**Shape A: Ringing Iron** (the old plain reaction). No stone: the shaken armour rings like struck iron at once,
Sundered to full and the ring of sparks as above. **Shape B: Hammer Scale.** No ring: four flakes of glowing scale fly
from it at the nearest enemies within 6 m, 8 damage, a notch and Tremor 1 each.

**The built branch.** The name Forge stays (the weapon tree is now called the Way). Keep Hammer and Anvil (the
reaction's stone feeds it), Struck Ore and Buckled (Buckled is the standing rule while both are on; the reaction is the event).
Re-aim Folded Steel: its folds add weapon damage for the rest of the run, which clashes with the rule that the Way is
the only thing that improves a weapon. Settled (`README.md`): the folds last until the next rest fire, so they are a
fight's growth and not a weapon's.
Drawn From Stone is the duo spell; Trip Hammer moves to Earth's or Metal's tree. World Anvil, begun by a finisher,
becomes the top mark.

### Metal and Light (Sundered + Marked), distance 1

**The image.** Polished bronze throws the sun: the shields of Syracuse turned into burning mirrors in the old story.
In India, the divine smith shaved an eighth of the sun's brightness away on his lathe and forged the gods' weapons,
the discus among them, from the shavings. Greek bronze and Indian sun.

**Reaction: Glint.** Marked meets Sundered. The light catches in the broken armour and is thrown: a straight ray from
it to the nearest other enemy within 6 m: 10 damage, 15 stagger, Marked and a notch. Seen: a hard white line between
two bodies, a spark of light on the armour where it left.

**Shape A: Mirror to Mirror.** The ray goes on from every enemy it strikes that carries Sundered, up to four in all,
drawing a zigzag across the room. **Shape B: Burning Mirror** (the bronze mirrors of Syracuse). No ray: the light gathers into the cracks to one
white-hot point, and the next weapon blow on it is critical and lays two notches.

**The built branch.** Keep Bright Steel, Halo of Blades, Mirror Edge and Blinding Guard (keystone). Answering Draw needs
the Katana: move it to the Katana's infusion marks on the Way. Six Mirrors is the duo spell; Sun Wheels moves
to Metal's tree. One of the first marks becomes the Shape mark.

### Light and Dark (Marked + Cursed), distance 1

**The image.** At totality, ripples of shadow run over the ground, beads of sunlight break through the moon's valleys,
and the last one flares as the diamond ring. In the Indian telling, the severed head Rahu swallows the sun, and it
slips out of his cut throat.

**Reaction: Diamond Ring.** Marked meets Cursed. A disc of dark closes over it in 0.5 s and a bead of light breaks at
its rim: 16 damage as a critical, 30 stagger; and for 4 s its Mark is not spent by blows. Seen: a black disc on the
floor under it with one burning point on the edge.

**Shape A: Shadow Bands.** As the dark closes, bands of shadow ripple out across the floor to 5 m and Curse every enemy
they cross. **Shape B: Through the Throat.** The light slips out of its far side as a line 6 m long: 12 damage and
Marked on everything on the line.

**The built branch.** Cut Twilight (it only lays both; Shape mark). Keep Day and Night, Low Sun and The Dividing Line
(keystone). Dusk and Dawn is the duo spell, because it pays the heavy blow, but as built it is Light's Step Into
Light again (a step through, then a critical heavy). Re-aim it at this pair: you step into the enemy's shadow (dusk),
gone for 0.4 s, and come out of the far rim as the bead of light (dawn), cutting; the cut sets off Diamond Ring at
once, whatever the rest, and the heavy blow after it is critical. Black Sun moves to Dark's tree. Totality,
begun by the surge, becomes the duo surge.

### Dark and Poison (Cursed + Poisoned), distance 1

**The image.** The gu: venomous creatures sealed in a dark jar until the last alive is the poison. The halahala that
rose first from the churned ocean, which Shiva held in his throat until it turned blue. Poison kept in the dark grows.

**Reaction: Shut in the Dark** (the gu, made the reaction by the pair review; named apart from Poison's Heresy, The
Last One in the Jar). Cursed meets Poisoned; rest 8 s. A ring of black lacquer 4 m across closes on the floor round it
for 8 s: the jar's mouth, seen from above. Its doses stop fading. Each other poisoned enemy that dies inside the ring
crawls to it as a black swarm and gives it every dose (to twelve, past the usual eight), and each kill you make inside
feeds you 4. When it dies, or the 8 s end, the jar breaks: 5 a dose in 3 m, and half its doses laid on all it reaches.
Seen: a black ring, swarms crawling across the floor to one body, green-black bubbles swelling on it. Steel: kill order
is the play: finish the small ones beside the jar, then break the jar with a finisher.

**Shape A: Swarm.** The jar moves on: when it breaks, its swarm crawls to the nearest enemy within 8 m and seals a new
jar there. **Shape B: Rot to the Bone.** The jar eats its balance: each bite fills its stagger meter by 3 a dose, and
the meter does not recover while it is sealed.

**The built branch.** Wither re-aims to a minor (the curse's half of it overlaps the jar). Carrion Flies is now Swarm;
cut it. Keep Feeding Rot and One Death Feeds (keystone). Wasting Grip is the duo spell, re-aimed at the jar: the stream
seals a jar on the first enemy it bites, past the rest, and each second of the stream adds a dose to whatever is
inside the ring. Grave Soil moves to Dark's or Poison's tree. The Reaping, begun by a finisher, becomes the top mark.

### Poison and Fire (Poisoned + Burn), distance 1

**The image.** Chinese alchemists seeking the elixir of life heated saltpetre, sulphur and charcoal and found *huoyao*,
"fire medicine": gunpowder. Egypt's embalming resins burn. A medicine that turns out to be fire.

**Reaction: Fire Medicine.** Burn meets Poisoned. The poison catches like powder: a fuse of sparks runs over it for 0.6
s, then it bangs: 4 a dose and 6 a Burn stack in 2.5 m, 30 stagger, 1 Burn on all reached; half its doses are spent.
Seen: hissing sparks, a puff of white smoke, a flash of orange with green at its heart.

**Shape A: Fire Lance.** The bang is a jet out of its far side (the first gunpowder weapon was a tube spitting flame): a
cone 5 m long and 40 degrees wide, same damage, a dose and a Burn on all in it. **Shape B: Flying Fire.** The bang
throws five burning pellets that land in a ring 3 m out as fire patches (1 m, 3 s) that burn and poison what walks in.

**The built branch.** Cut Fuel (Shape mark). Re-aim Goes Up: at full depth of poison, Fire Medicine spends nothing and
strikes twice as hard. Keep Funeral Fire and Slow Burn (keystone). Light the Pyres is the duo spell; Fire Eater moves to
Poison's tree and Lit Fuse to Fire's.

---

# Distance 2: the new branches

### Fire and Wind (Burn + Unsteady), distance 2

**The image.** Embers on the wind: a fire that leaves its body and lands ahead of itself. Spotting carries embers far
past the fire's front, and a fire draws its own wind into it until it whirls. In 1657 a dry northwest wind carried the
long-sleeves fire across two thirds of Edo; Egypt's hot desert wind blows for fifty days.

**Reaction: Carried Embers.** Burn meets Unsteady. The wind strips three embers off it and they fly 5 m downwind (away
from you, along your line to it) and land 1 m apart as small fires (1 m, 3 s, 3 a bite, 1 Burn); any enemy within 1.5 m
of a landing is set alight. Order does not matter, but the direction is the player's: they aim the fire by where they
stand. Seen: a short stream of orange sparks and smoke carried in one direction, flames springing up where they fall.
(The document's first idea, "spreads to every enemy near", is kept but given a direction, so it is a choice.)

**Shape A: Fire Whirl.** The flame stands up on the enemy as a turning column 1.5 m across for 3 s: it pulls enemies
within 3 m toward it at 1 m a second and sets everything within 2 m burning once a second. **Shape B: Hot Wind.** One
long gust instead of embers: a lane 8 m long and 2 m wide downwind that lays 1 Burn and shoves everything in it 2 m.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Fire on the Wind | Shape | 3 | Opens Fire Whirl and Hot Wind. |
| Fan the Flame | Major | 3 | A perfect dodge fans the flame on the attacker: one Burn deeper, and Carried Embers fires again at once. |
| Long Embers | Minor | 1 | Embers fly 2 m further, and one more flies. |
| The Wind Carries It | Keystone | 5 | Your heavy blows on a burning enemy throw its embers downwind as if it had reacted, with no Unsteady needed and no rest; each throw costs it one Burn. Where you stand when you strike aims the fire, so every heavy blow is a choice of where it lands. (A trail of fire behind a thrown body is too near the Poison and Wind reaction, Ill Wind.) |

**Duo spell: The Fire Breathes In** (the crown). Opener; gathers. Cooldown 16 s; learned for 5 Shards. A ring of flame
2.5 m across stands at the aim point (up to 6 m ahead) for 6 s, and the fire draws its own wind: every enemy within 8 m
is pulled toward it at 1.2 m a second and made 1 deeper Unsteady every 2 s; inside the ring, 4 damage and 1 Burn every
0.5 s. Seen: a tall ring of real flame with streaks of pale dust and sparks drawn in along the floor from every side.
Steel: while it stands, a heavy blow on any enemy within 8 m throws it toward the fire rather than away; an enemy
thrown in fires Carried Embers at once, takes 15, and the ring grows 0.5 m (to 4 m). Bosses are pulled at a fifth.

### Lightning and Ice (Static + Chill), distance 2

**The image.** Lightning is born of ice. In a thundercloud, soft hail and ice crystals collide and split the charge
between them; a storm that makes little ice makes no lightning. Thundersnow is the rare storm where you hear it. The
Japanese thunder meets the Norse winter, as Thor's hammer met the frost giants.

**Reaction: Sleet Charge.** Static meets Chill. The frost on it begins to charge itself: for 3 s it gains one Static
every 0.5 s (to full). At the end, or at once if it freezes, a bolt falls on it from above and discharges all its
Static (7 damage and 5 stagger a stack in 4 m), and everything the discharge reaches takes 1 Chill. A heavy blow
discharges it early as usual: wait for the full charge, or take it now. Seen: fine white pellets turning round it and
crackling, brighter each half second, then a bolt from the top of the frame. (The first idea, a shatter that jumps to
the next frozen enemy, is already Ice's gift The World Holds Still, so it is not repeated.)

**Shape A: Thundersnow.** The cloud is wide: the discharge lands as one bolt on every chilled enemy within 6 m, each
discharging whatever Static it carries. **Shape B: Glass Spear.** The bolt freezes as it lands: a spear of clear ice
stands where it struck and the enemy is Frozen at once for 1.5 s, whatever its Chill, ready for a heavy blow.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Storm Ice | Shape | 3 | Opens Thundersnow and Glass Spear. |
| Charged Hail | Major | 3 | Shattering an enemy that carried Static throws its charge into the shards: each shard that strikes lays 1 Static. |
| Cold Snap | Major | 3 | A perfect guard against a chilled enemy makes its Sleet Charge strike at once, with full Static. |
| Nothing Resists | Keystone | 5 | Cold carries lightning without loss: an arc that leaves a chilled enemy strikes the next at full damage and goes on to every chilled enemy within 5 m of the last. |

**Duo spell: Glass Storm** (the crown). Opener; pressure. Cooldown 14 s; learned for 5 Shards. Six shards of clear ice
turn round you at 2 m for 8 s, with threads of lightning jumping between them; each shard that passes through an enemy
cuts it for 4 and lays 1 Chill, every other pass 1 Static. Steel: each weapon blow that lands flings the nearest shard
into the enemy struck (8 damage, 1 Chill, 1 Static), and a heavy blow flings all that are left. The ring is
ammunition the blade spends. Seen: hard glassy splinters, never a glowing ring.

### Wind and Water (Unsteady + Soaked), distance 2

**The image.** Wet footing in a gale. Fujin carries the winds in a bag; in Hawaii, La'amaomao's gourd held every wind
of the islands, each called out by name, and when the boy Ku-a-Paka'a opened it the squall swamped the king's fleet. Two
cultures each keep the winds in a vessel, and both tell of what happens to boats when it is opened.

**Reaction: Capsize.** Soaked meets Unsteady. It slips and goes down on its back: knocked over for 1.2 s, a ring of
splash round it. An elite only staggers (25 stagger); a god takes 15 stagger. Seen: the body tipped flat, spray thrown
out on the side the wind came from. It changes play at once: free blows, and a reason to get the order right.

**Shape A: Waterspout.** It is lifted in a column of spray for 1.2 s instead (in the air, so air blows hold it up).
**Shape B: Breakers.** As it falls it throws a sheet of spray 4 m behind it: whatever stands there is Soaked and made
2 deeper Unsteady, and one already Unsteady capsizes in turn (this breaks the no-cascade rule on purpose: a line of
enemies goes over like dominoes).

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Heavy Weather | Shape | 3 | Opens Waterspout and Breakers. |
| Wake | Major | 3 | A perfect dodge leaves a line of spray along its path for 3 s that Soaks and unsteadies what crosses it. |
| Rolled In | Major | 3 | An enemy that capsizes is dragged 2 m toward you as it falls, into reach. |
| Swamped | Keystone | 5 | Blows on a capsized enemy fill its stagger meter twice as fast. |

**Duo spell: Ride the Squall** (the crown). Movement. Cooldown 9 s; learned for 5 Shards. You are carried 7 m along the
stick on a low racing sheet of spray, untouchable for the 0.45 s ride; every enemy within 1.5 m of the line takes 6, is
Soaked and made 2 deeper Unsteady, and so capsizes if it was already Unsteady. Seen: a white streak of foam and a
spray fan, the player crouched low in it. Steel and timing: cast out of a perfect dodge, it starts no cooldown and the
first weapon blow after it lands is critical.

**Duo surge: Open the Gourd.** Learned for 5 Shards; 40 mana, 10 s. A gourd hangs at your hip. Each dodge lets out one of its winds: a gust
6 m long and 2 m wide down the dodge's line that Soaks, unsteadies twice and shoves 2 m. Each capsize while it lasts
gives 6 mana and 1 s more of the surge, to 15 s. Strong and distinct: the dodge becomes the attack.

### Ice and Life (Chill + Seeded), distance 2

**The image.** Many seeds will not wake until they have been through a winter. The wood frog freezes solid, its heart
stopped, and thaws in spring and hops away. In the Norse end of the world, Líf and Lífþrasir hide in the wood of
Hoddmímir through the great winter, live on the morning dew, and the new world grows from them. The Celtic hag of winter
gives way to spring each year.

**Reaction: Winter Seed.** Chill meets Seeded. The seed sleeps under the frost: its Seeded stops fading while the Chill
lasts. When the Chill ends, or when it was Frozen and the freeze ends by thaw or shatter, the seed wakes: thorns burst
from it, 12 a Seeded stack within 2.5 m (36 at most), seeding what they reach, and you heal 3 a stack. Seen: a
frost-white bud on its back; at the waking, green thorns crack out through the ice. A shatter is the quickest spring.

**Shape A: Hard Spring.** The seed wakes only on a shatter, but twice as hard, and it seeds all it reaches to full.
**Shape B: Heart Under Ice.** The waking mends rather than wounds: half the damage, 5 health a stack, and one ward.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Thaw | Shape | 3 | Opens Hard Spring and Heart Under Ice. |
| Seed Under Snow | Major | 3 | An enemy that dies chilled and Seeded leaves a frozen seed on the floor for 8 s; step on it to crack it: heal 8, and everything within 2.5 m is chilled and seeded. |
| Late Frost | Major | 3 | A perfect guard against a Seeded enemy roots it in frost: 2 Chill, held 0.8 s. |
| Hidden in the Tree | Keystone | 5 | Things you grow are rimed: they chill what they bite, and while you stand within 4 m of one, the first blow that would land on you every 6 s is turned by its frozen bark. |

**Duo spell: Green Under the Ice** (the crown). Detonator. Cooldown 14 s; learned for 5 Shards. You strike the floor and
lines of green run through the frost to every enemy within 9 m that carries both (eight at most): each one's Winter
Seed wakes at once, and any of them Frozen is shattered as by a heavy blow. With no one to reach, half the cooldown
comes back. Steel: an enemy your weapon struck in the last 2 s wakes at one and a half times. Seen: cracks of frost
glowing green, thorned shoots breaking shells of ice, chips and petals thrown up together.

### Water and Earth (Soaked + Tremor), distance 2

**The image.** Shake wet sand and the water between its grains takes the weight: the ground turns to liquid, buildings
sink, sand boils up through cracks. The fisher pulled the land up out of the sea, and in his land the island still
breathes; the Celtic bogs swallowed offerings and kept bodies for two thousand years. Land that is still sea.

**Reaction: The Ground Gives.** Tremor meets Soaked. The floor under it turns to wet sand in a pool 2 m across for 4 s.
It sinks to the knees: slowed 50%, and it cannot dodge, leap or step back; other enemies that wade in are slowed 30%. A
heavy blow on a sunk enemy holds it 0.5 s. Elites are slowed 25%; gods only take the Tremor. Seen: a dark wet ring with
bubbling at the edge, the body drawn lower in it. (The first idea, the mire to the knees, kept and given its cause.)

**Shape A: Quicksand.** The pool is 3 m and drags what stands in it to its middle at 1 m a second, so enemies end up
together. **Shape B: Sand Boil.** The pool lasts only 1.2 s, then erupts in a fountain of mud and water: it is lifted for
1 s and everything within 3 m takes 12, Soaked and Tremor 1.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Wet Ground | Shape | 3 | Opens Quicksand and Sand Boil. |
| Given to the Bog | Major | 3 | An enemy that dies sunk is swallowed and the bog gives back: a quarter of a flask. |
| Driven Down | Major | 3 | Heavy blows on a sunk enemy pass its guard and hold it 0.5 s longer. |
| The Land Was Sea | Keystone | 5 | Your dodge through wet sand is a slide: it costs no stamina and leaves 3 m of wet sand behind it for 3 s. |

**Duo spell: The Island Breathes** (the crown). Breaker. Cooldown 15 s; learned for 5 Shards. The ground in a 5 m circle
at the aim point (up to 8 m) breathes. In (0.9 s, a ring tightening on the floor): it heaves up like a back, and every
enemy on it is lifted for 1 s and given Tremor 2. Out: it slumps into wet sand for 4 s, Soaking everything on it, and
each one there is sunk as The Ground Gives. Seen: the floor bulging, then foam opening like gills along the rim as it
breathes out. Steel: blows on the lifted hold them up; a heavy blow on a sunk enemy in the out-breath strikes as if
from above, for double stagger.

### Life and Metal (Seeded + Sundered), distance 2

**The image.** Cadmus sowed the dragon's teeth and a crop of armed men rose from the furrows, spearheads first, then
helmets. The druids cut mistletoe, the all-heal, with a golden sickle. Some real plants draw nickel out of the soil
until their sap runs blue-green with metal. A seed finds a crack and grows through it.

**Reaction: Iron Thorns.** Seeded meets Sundered. The seed roots in the cracks of its armour and iron thorns grow out
of it, one for each notch (three at most), for 6 s. Each weapon blow on it snaps one off and the thorn flies at the
nearest other enemy within 6 m: 6 damage, Seeded and a notch. Seen: dark barbed spikes standing out of the body, each
one leaving with a blow.

**Shape A: Pruning Hook.** Your blows prune the thorns rather than throw them: each one cut heals you 4. (The druids'
golden sickle is Life's own, on Reap What Is Sown.) **Shape B: Iron
Harvest.** Five thorns grow, and they all burst out together when it dies or the 6 s end: a ring of blades to 3 m, 8
damage and a notch each.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Graft | Shape | 3 | Opens Pruning Hook and Iron Harvest. |
| Sap of Iron | Major | 3 | An enemy that dies with thorns in it gives its iron to the ground and the ground to you: a quarter of a flask fills at once, as plants draw metal up through the root. |
| Ore in the Root | Major | 3 | Things you grow draw iron from the ground: each grown thing lays a notch of Sundered when it bites. |
| Grafted Edge | Minor | 1 | A notch laid on a Seeded enemy lays two. |

**Duo spell: The Sown Men** (the crown). Opener; pressure. 45 mana (a larger spell); learned for 5 Shards. You throw five
teeth in a fan 4 m ahead; they sink, and 0.8 s later five warriors of bronze and root rise from the soil. Each goes to
the nearest enemy and strikes it three times (8 damage; the third strike Seeds and notches), and after 6 s they crumble
to rust and leaves. Steel: each heavy blow you land, every sown man still standing strikes that same enemy at once. Seen:
spearheads, then crested helmets, coming up out of the floor with soil and roots falling from them.

### Earth and Light (Tremor + Marked), distance 2

**The image.** Stressed rock gives off light: glows and flashes are seen over faults before and during earthquakes, and
quartz flashes when it is crushed. At Newgrange the solstice sunrise runs straight down the stone passage; at Konark the
sun's chariot is carved in stone and its wheels are sundials. Stone that keeps the sun's hours.

**Reaction: Earthlight.** Tremor meets Marked. The floor under it cracks in a star of glowing seams 3 m across, and 0.5
s later light breaks up through them: 14 damage, 25 stagger, and Marked on every enemy standing on the seams. The one
that reacted has its Mark renewed (three more blows). Seen: gold seams cracking outward through the stone, then the
burst rising out of them; the shape is the crack, not a disc of light.

**Shape A: Down the Passage.** The light runs on through the ground in a straight line from it away from you, 8 m long
and 1 m wide: 12 damage and Marked on all on it. **Shape B: Lit Seam.** The seams stay lit for 5 s, and each heavy blow
you land on an enemy standing on them flashes them again: 10 damage and 15 stagger to all on the seams.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Lit Stone | Shape | 3 | Opens Down the Passage and Lit Seam. |
| Keeping the Hour | Major | 3 | Every 6 s in a fight, the next heavy blow is on the hour: critical, with Tremor 2. A small dial at your feet fills to show it. |
| Dawn Through the Door | Major | 3 | A perfect guard made on glowing seams blinds the attacker: Marked, Tremor to full depth. |
| Quartz Vein | Minor | 1 | Earthlight's star is 0.5 m wider. |

**Duo spell: The Stone Keeps the Hour** (the crown). Opener; holds ground. Cooldown 16 s; learned for 5 Shards. A
standing stone rises 3 m ahead for 9 s, and a line of sunlight 6 m long sweeps round it like a dial's shadow, one turn
every 3 s; every enemy it crosses takes 10, 15 stagger, Marked and Tremor 1. The stone blocks the way and counts as
stone at an enemy's back. Seen from above: a turning hand on the floor, the rhyme of Khepharu's obelisk and the
council's gnomon. Steel: a weapon blow that lands on an enemy in the moment the line crosses it is critical and
staggers double. The line never harms the player.

### Metal and Dark (Sundered + Cursed), distance 2

**The image.** The lame smith made a net of chains too fine to see and too strong to break, and caught the lovers in
the dark of his own bed. In India, death comes with a noose: Yama throws it and draws the soul out, and only the one who
keeps walking behind him wins it back. Chains no one sees.

**Reaction: Black Chain.** Cursed meets Sundered. A black chain runs from the cracks in its armour to the nearest other
enemy within 6 m (into the floor if there is none) for 5 s. The two cannot move more than 4 m apart (one chained to the
floor cannot leave 2 m of the spot), and 30% of weapon damage dealt to either is dealt to the other. Gods are never
leashed; the share still holds. Seen: one thin dark line on the floor between two bodies, glinting as it pulls.

**Shape A: Drawn Tight.** The chain snaps taut at once: the two are dragged together and collide, 12 damage and 25
stagger each, and the chain falls away. **Shape B: Shared Wound.** No leash, but half of every blow is shared, for 8 s,
and a curse laid on one runs along the chain to the other.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Hidden Chain | Shape | 3 | Opens Drawn Tight and Shared Wound. |
| Dead Weight | Major | 3 | When one of a chained pair dies, the chain drags the other to where it fell, holds it 1 s and Curses it again. |
| Rust in the Curse | Major | 3 | Notches on a Cursed enemy do not fade while the curse lasts. |
| One Noose for Two | Keystone | 5 | A finisher on a chained enemy lands on its chain-mate too, at half its damage and stagger. |

**Duo spell: The Smith's Net** (the crown). Opener; controls a crowd. Cooldown 16 s; learned for 5 Shards. You throw a
net of fine black chain up to 7 m; it opens to 4 m across and falls 0.4 s later, seen only as a shimmer and its shadow
growing on the floor (the shadow is the warning). Everything under it is Cursed, notched once and chained to every
other enemy under it for 6 s, and none can leave the circle for 3 s (gods are not held in). Steel: while they are
netted, a quarter of every weapon blow on one lands on all the others.

### Light and Poison (Marked + Poisoned), distance 2

**The image.** Hogweed sap does nothing in the shade; in sunlight it burns the skin. Scorpions, one of the five poisons
of the gu, glow blue-green under ultraviolet light. Shennong, who tasted a hundred herbs, had a body you could see
through, and watched each poison work inside him. The sun shows the poison and wakes it.

**Reaction: Burning Sap** (given the hogweed's delay by the pair review). Marked meets Poisoned; rest 6 s. The light
wakes the sap, but the burn waits. For 3 s its poison stops biting and its veins light gold-green under the skin,
brighter each second. Each critical blow in those 3 s raises a weal (a swollen yellow bead on the body, three at most:
the Mark's three). At the end every bite it held back lands at once, doubled, and each weal bursts, splashing two
doses on every enemy within 2.5 m. Seen: veins brightening, beads swelling, then one wet yellow burst. Steel: the three
criticals load the weals; stay for the burst or move on. Held, then released, not bitten blow by blow (that is
Quicksilver).

**Shape A: The Light Finds Them.** When it reacts, every poisoned enemy within 8 m is Marked as well; their veins light
up across the room. **Shape B: Blistered.** The weals do not wait for the end: each bursts at the critical that
raises it, splashing its two doses on every enemy within 2.5 m.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Lit Veins | Shape | 3 | Opens The Light Finds Them and Blistered. |
| Nectar After Poison | Major | 3 | An enemy that dies carrying both gives up 6 health and 8 mana (the nectar came after the poison in the churning). |
| Seen Through | Major | 3 | A perfect guard against a poisoned enemy Marks it and makes all its poison bite at once. |
| Bright Venom | Minor | 1 | The held bites strike 15% harder when they land. |

**Duo spell: Bared to the Sun** (the crown). Opener. Cooldown 13 s; learned for 5 Shards. You tear the shade off a
place: a hard-edged shaft of sun 5 m across falls on the aim point (within 8 m) for 5 s, drawn as dust motes in the
air and a bright floor with the enemies' shadows thrown sharp across it. Every poisoned enemy in it is Marked and
starts Burning Sap at once (past its rest, once each). Steel: each weapon blow you land inside raises one more weal on
the one struck, so every weal is a blow. In the Fire land, where shade is a rule, it reads twice. (It replaces Bitter
Lantern, which laid a mark and a dose side by side.)

### Dark and Fire (Cursed + Burn), distance 2

**The image.** Fire without flame: peat and coal seams smoulder underground for years, and one in Australia has burned
for six thousand. Each night the Egyptian sun goes down into the underworld, through twelve hours of dark and a lake of
fire, and is reborn as the scarab at dawn; Shiva's third eye burned desire to ash. The sun at midnight.

**Reaction: Smoulder.** Cursed meets Burn. The flames on it go out and the fire sinks in: its Burn shows as a dark red
glow in cracks with black smoke seeping, goes on biting, stops fading, and deepens by one every 2 s (to full) while the
curse lasts. When the curse ends or it dies, it flares: every Burn stack is set off at once (10 a stack within 3 m),
and what is reached is set alight. Gods deepen every 3 s. Seen: a body that looks almost out, then bursts.

**Shape A: Night Sun.** While it smoulders, each of your blows on it heals you 1 a Burn stack. **Shape B: Coal Seam.** The
fire goes down into the floor where it reacted: a patch of smouldering ground 2.5 m across for 8 s, almost dark, a red
glow in its cracks. Any enemy that stands on it smoulders as though Cursed, and when the patch flares at its end it
sets off every smoulder on it. A place to lure them to, not a trail (the trail of molten drips is the Metal and Fire
pair's Slag).

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Banked Coals | Shape | 3 | Opens Night Sun and Coal Seam. |
| Third Eye | Major | 3 | A perfect guard against a smouldering enemy opens a line of black-edged fire from you through it, 6 m: 20 damage, 2 Burn and Cursed to all on it. |
| Deep Coal | Major | 3 | Burn on a smouldering enemy can lie two deeper. |
| The Sun Goes Under | Keystone | 5 | While your health is below half, every Burn you lay smoulders, curse or no curse. |

**Duo spell: The Burning Lake** (the crown). Zone; rewards the kill. Cooldown 16 s; learned for 5 Shards. A lake 4 m
across opens at the aim point (up to 8 m) for 7 s: a still black surface, flame licking at its rim, embers rising. Every
enemy on it is Cursed, takes 1 Burn a second, and is slowed 20%, so all of them smoulder. Steel: an enemy killed or
finished on it is drawn under, and the lake grows 0.5 m and lasts 1 s longer; a finisher on it flares every smoulder on
the lake at once.

### Poison and Lightning (Poisoned + Static), distance 2

**The image.** The venoms of the centipede and the scorpion work on the nerves, the body's own lightning. A storm leaves
ozone, sharp and harmful, in the air. In China the thunder god punishes hidden crimes, and the Daoist thunder rites
cast out the spirits of sickness with paper talismans; the hopping dead are stopped by a paper seal on the brow, and
Japan's shrines hand out paper charms of their own.

**Reaction: Nerve Fire.** Poisoned meets Static. While both last, each bite of poison spends one Static and jolts it: a
spark runs over it and it convulses, 7 damage, 5 stagger and a 0.3 s flinch that breaks an attack being wound up.
Elites flinch once every 4 s; gods never flinch and take 6 more stagger instead. Seen: a jerk of the whole body with
each green bite, a blue crackle along the limbs.

**Shape A: Ozone.** Each jolt lets off a puff of sharp blue mist 2 m across that lays a dose and a Static on enemies
near. **Shape B: Lockjaw.** The third jolt locks it: held 1.2 s.

**Duo branch** (four marks):

| Mark | Kind | Shards | What it does |
|---|---|---|---|
| Struck Nerve | Shape | 3 | Opens Ozone and Lockjaw. |
| Strike on the Beat | Major | 3 | A heavy blow that lands within 0.3 s of its bite discharges all its Static and lays as many doses. |
| Five Thunders | Major | 3 | Every fifth bite on an enemy carrying Static calls a bolt down on it: 14 damage, 1 Static, leaping to two more. |
| Hidden Crimes | Keystone | 5 | Poison counts as charge: a discharge on a poisoned enemy deals 7 more for every two doses it carries, and spends them. |

**Duo spell: Paper Seal** (the crown). Detonator; breaker. Cooldown 13 s; learned for 5 Shards. You step 3 m to the enemy
faced and slap a seal of yellow paper on its brow: it is held 1.2 s (elites 0.5 s, gods not at all). For 6 s each
weapon blow on it burns the seal down a fifth; at the fifth blow thunder falls on it: 30 damage plus 4 a dose, all its
Static discharged, 50 stagger in 3.5 m, and each dose it carries passes one to every enemy within 3 m. If the 6 s run out
first, the thunder falls at half. Seen: the paper fluttering and charring at each blow, red brush strokes, then the bolt.

---

## Engine needs

- **Reactions themselves**: a trigger when an affliction lands on an enemy carrying a given second one, a 4 s lock per
  enemy and pair, and a flag on what a reaction lays so it starts nothing (the rule above).
- **A timed state on one enemy** that runs an effect on a beat (Corona, Sleet Charge, Rime, Supercooled, Iron Thorns,
  Smoulder): the schema's "per-enemy debuff or tally" gap, wanted by six reactions.
- **Downwind**: the direction from the player through the enemy, for Carried Embers, Hot Wind, Bolt from the Blue,
  Breakers, Down the Passage, Fire Lance.
- **Side of a blow**: whether a heavy blow came from the side the rime faces (Rime).
- **Knocked down**: a lying state for Capsize; Hold stands in for now.
- **Sinking**: a pool that slows, forbids an enemy's dodges and leaps, and draws its body lower (The Ground Gives).
- **Linked enemies**: shared damage and a leash between two enemies (Black Chain, The Smith's Net).
- **Burn that sinks in**: Burn that does not fade, deepens on its own and is drawn without flame (Smoulder).
- **A flinch action**: Twin Break's flinch as data, on a poison bite (Nerve Fire).
- **Thaw trigger**: a freeze or a Chill ending, for Winter Seed.
- **Heavy blow throws toward a point**: The Fire Breathes In; a moving whirl hazard that pulls (Fire Whirl).
- **A weapon blow flings an orbiting piece**: Glass Storm (discs plus a blow hook).
- **A dash through enemies**: Ride the Squall (the drill form's pass, low and wet).
- **Helpers that answer the heavy blow**: The Sown Men.
- **A sweeping line form** round a planted stone, with a timing window on blows (The Stone Keeps the Hour).
- **A shaft of sun on a place** that starts a reaction in those inside: Bared to the Sun (a field form).
- **Discharges that bend toward one enemy**: Flashover's live flame (arcs within 6 m redirected into it).
- **Poison held back and released**: Burning Sap's 3 s wait, with weals counted by critical blows.
- **A ring on the floor that keeps count of deaths inside it**: Shut in the Dark (doses passed past eight, to twelve).
- Data only, no new code: Glint (Arc), Diamond Ring (delayed Burst and a Boon), Strike on the Beat (OnBeat), Paper
  Seal (brand form and Hold).

## Sources

Phenomena:
- [NOAA: how lightning is created](https://www.noaa.gov/jetstream/lightning/how-lightning-is-created) and [Arizona: lightning and thunder](http://www.atmo.arizona.edu/students/courselinks/spring17/atmo336/lectures/sec2/lightning.html) (ice and graupel make the charge)
- [Soil liquefaction, Britannica](https://www.britannica.com/science/soil-liquefaction), [Utah Geological Survey](https://geology.utah.gov/hazards/earthquakes/liquefaction/)
- [Earthquake light](https://en.wikipedia.org/wiki/Earthquake_light), [National Geographic](https://www.nationalgeographic.com/science/article/140106-earthquake-lights-earthquake-prediction-geology-science)
- [Ember attack](https://en.wikipedia.org/wiki/Ember_attack), [Smithsonian on embers and fire whirls](https://www.smithsonianmag.com/science-nature/untangling-physics-firenadoes-drifting-embers-other-wildfire-phenomena-180971735/), [Firestorm](https://en.wikipedia.org/wiki/Firestorm)
- [Great Fire of Meireki](https://en.wikipedia.org/wiki/Great_Fire_of_Meireki)
- [Smouldering](https://en.wikipedia.org/wiki/Smouldering), [Coal-seam fire](https://en.wikipedia.org/wiki/Coal-seam_fire)
- [Brinicle](https://en.wikipedia.org/wiki/Brinicle), [Supercooling](https://en.wikipedia.org/wiki/Supercooling)
- [Zao's rime trees (Highlighting Japan)](https://www.gov-online.go.jp/eng/publicity/book/hlj/html/202401/202401_02_en.html), [Smithsonian on the snow monsters](https://www.smithsonianmag.com/travel/arboreal-snow-monsters-overrun-northern-japan-every-winter-180971323/)
- [Wood frog freezing (National Geographic)](https://www.nationalgeographic.com/animals/article/animals-freeze-then-thaw-explained), [PeerJ/PMC paper](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC5682099/)
- [Giant hogweed, NYSDEC](https://dec.ny.gov/nature/animals-fish-plants/plants/harmful-plants/giant-hogweed) (phototoxic sap)
- [Scorpion fluorescence, ASU Ask a Biologist](https://askabiologist.asu.edu/ultraviolet-scorpion)
- The review fixes of 2026-11-18 (Flashover on conducting flame, Burning Sap's delay, the gu jar of Shut in the Dark)
  carry their sources in `review_pairs.md`: wildfire flashover on power lines, phytophotodermatitis, the gu.
- [Gunpowder and Chinese alchemy: elixir poisoning](https://en.wikipedia.org/wiki/Chinese_alchemical_elixir_poisoning)
- [Archimedes' heat ray](https://en.wikipedia.org/wiki/Archimedes'_heat_ray)

Myth:
- [La'amaomao and the wind gourd](https://en.wikipedia.org/wiki/La%CA%BBa_Maomao), [Paka'a](https://en.wikipedia.org/wiki/Paka%CA%BBa)
- [Kumulipo](https://en.wikipedia.org/wiki/Kumulipo), [Kamehameha Schools text](https://blogs.ksbe.edu/adakina/files/2008/02/kumulipo-text.pdf)
- [Líf and Lífþrasir](https://en.wikipedia.org/wiki/L%C3%ADf_and_L%C3%AD%C3%BErasir), [Hoddmímis holt](https://en.wikipedia.org/wiki/Hoddm%C3%ADmis_holt)
- [Spartoi (Theoi)](https://www.theoi.com/Gigante/Spartoi.html), [Dragon's teeth](https://en.wikipedia.org/wiki/Dragon%27s_teeth_(mythology))
- [Odyssey 8, the net (LitCharts)](https://www.litcharts.com/lit/the-odyssey/book-8)
- [Savitri and Satyavan](https://en.wikipedia.org/wiki/Savitri_and_Satyavan), [Mackenzie, Yama](https://www.sacred-texts.com/hin/iml/iml08.htm)
- [Rahu](https://en.wikipedia.org/wiki/Rahu), [Halahala](https://en.wikipedia.org/wiki/Halahala)
- [Sudarshana Chakra](https://en.wikipedia.org/wiki/Sudarshana_Chakra) (forged from the sun's shavings)
- [Konark Sun Temple](https://en.wikipedia.org/wiki/Konark_Sun_Temple), [World History Encyclopedia on its wheels](https://www.worldhistory.org/image/5270/wheel-konarak-sun-temple/)
- [Amduat](https://en.wikipedia.org/wiki/Amduat), [Ra](https://en.wikipedia.org/wiki/Ra)
- [Lei Gong (Britannica)](https://www.britannica.com/topic/Lei-Gong), [Five Thunders rites (Reiter)](https://www.airitilibrary.com/Article/Detail/P20170630002-201612-201807050024-201807050024-7-31)
- [Shennong (Hubei government)](http://en.hubei.gov.cn/culture/intangible/201508/t20150821_1412127.shtml)
- The project's own `docs/design/WORLD.md` for each god's land and myth.

Games, for the principle only:
- [Hades duo boons](https://hades.wiki.gg/wiki/Hades:Duo_Boon) and [an essay on why they work](https://venbright.medium.com/the-harmonious-secrets-of-hades-hermes-3282fc9eaa3e): a duo ties two gods' habits into one rule.
- Genshin Impact's reactions and Divinity: Original Sin 2's surfaces (known from play): order and place matter, so
  reactions give direction (downwind, behind, under) rather than only damage.
