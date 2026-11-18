# Steel and Element: the content, reconciled (2026-11-18)

This folder fills in `docs/design/STEEL_AND_ELEMENT.md`: every element's spells, all 55 pairs, and every weapon's
Way. Seven writers worked in parallel from `BRIEF.md`; this page is where their drafts were made to agree. Everything
here is **Proposal** until you say otherwise. Every number is a first guess for play to tune.

Start here. The drafts are the detail behind each line.

---

## What is here

| File | What it holds |
|---|---|
| `BRIEF.md` | The brief all seven writers shared: formats, rules, research to do |
| `elements_a.md` | Fire, Lightning, Wind, Ice, Water, Life: six spells each, charged blade, infused strike, Devotion, passives, cuts |
| `elements_b.md` | Earth, Metal, Light, Dark, Poison: the same, plus each element's base buff moved onto its root |
| `pairs_d1_d2.md` | The 11 built ring mixes (what to keep of each branch) and the 11 pairs two apart, with the shared reaction rules |
| `pairs_d3.md` | The 11 pairs three apart, with two duo surges |
| `pairs_d4.md` | The 11 pairs four apart, with four duo surges |
| `pairs_d5.md` | The 11 pairs opposite each other on the ring, with two duo surges |
| `weapons.md` | Every weapon's Way (three aspects, a fourth for a god's weapon), temper, infusion, gated techniques, Pair Bond |
| `depth_elements.md` | The element Heresies and Evolutions |
| `review_elements.md`, `review_pairs.md` | The two feel reviews of 2026-11-18; their recommended fixes are now in the files above (changelog at the end) |
| `README.md` | This page |

(`grep.exe.stackdump` in this folder is a crash file left by a tool, not content. It can be deleted.)

---

## The shared rules

Where the drafts assumed differently, one rule was chosen and all seven files now follow it.

### Spells and mana

- **Basic spells never cost mana.** They run on cooldowns of 6 to 12 s, which weapon play shortens.
- **Each element has exactly one larger spell paid in mana** (35 to 50). Dark's is paid in health instead (Pawned
  Heart). Ice and Earth had none; Winter Wall (35) and the new Cairn (40) now take that place.
- A spell may also be paid in health where an element's own rule says so (Life's Feed the Ground, Dark's Blood Price).
- **Duo spells** run on cooldowns (9 to 16 s). A duo spell may instead be its pair's larger spell, paid in mana (The
  Sown Men, Tipped Crucible, and the 11 built ring-mix spells at 40 to 60).

### The infused strike

- L2 with R1 (and L2 with R2 for the second element) is the weapon's carrier move with the element on it.
- **Each strike button has its own 8 s cooldown**, shortened by weapon play like a spell's.
- It **counts as a weapon blow** (it cuts cooldowns and gives mana) and **never charges the blade**, so it cannot feed
  itself.
- **All in on one element, L2 with R2 is the deep strike**: it keeps its 8 s cooldown and also costs **20 mana**.
- Note for play: the deep strikes in `elements_a.md` are lighter than the held, charged ones in `elements_b.md`. Tune
  them to one bar once both are in hand.

### Mark prices

| Tree | Prices in Shards |
|---|---|
| A spell branch | Learn 4, Empower 1, Shape 3, Master 3 (11 a spell) |
| An element's passives | Root 2, Major 3, Keystone 6, Devotion 6, Reaction lift 3; the god's gift is given, never sold |
| A duo branch | Shape 3, Minor 1, Major 3, Keystone 5; the duo spell 5; a duo surge 5 |

An element tree is 34 marks and about 95 Shards. A duo branch is about 17 to 20 Shards.

### How a reaction fires

- A reaction fires when an affliction lands on an enemy that already carries the other one of the pair. Order does
  not matter unless an entry says so.
- **Each reaction rests per enemy and per pair: 4 s by default.** An entry may set its own rest (3 to 8 s). The
  distance-3 pairs all rest 6 s, because most of them hold an enemy or set up a heavy blow.
- **Afflictions laid by a reaction start no reaction of their own.** A shape or keystone may break this on purpose,
  and says so where it does (Sea Ice, Breakers, Hot Shards, Blown Seed, The Fairy Wind, Sandblast, The Debt Passes).
- The plain reaction works from the first time it happens; the duo branch's Shape mark opens its two shapes.

### Weapons and the Way

- **The Way is the only thing that improves a weapon.** Folded Steel (Forge) now lasts only until the next rest fire:
  a fight's growth, not a weapon's.
- **Katana-only tree marks leave the element trees** and become element infusion marks on the Katana's Way (2
  Memories each, working while their element is carried): Burning Crescent, Delayed Cuts, Drawing Wind, Winter Draw,
  and Halo's Answering Draw.
- **Full Body Armament is the Fists' third aspect, The Armament.** Its mark leaves every element tree.
- **Gated techniques are R1 and R2 pressed together** (both mouse buttons on keyboard). Nothing else uses the chord.
- "Forge" means only the Earth and Metal mix. The weapon tree is the Way.

### Names and moved spells

- **Names of marks the drafts cut are free to reuse.** All the plain minors go, so names such as Quickening, Long
  Embers, Banked Coals, Deep Roots, Green Wood, Heavy Curse, Held Fast, Light Feet, Quick Hands and Still Air, and the
  cut major Feast, now name new pieces. Every other built name is kept for what it names.
- **Where a built ring-mix branch's spare spell "moves to" an element's tree** (Plasma Lance, Storm Overhead,
  Whiteout, Creeping Ice, Lotus Step, Thorn Wall, Trip Hammer, Sun Wheels, Black Sun, Grave Soil, Fire Eater, Lit
  Fuse), that is an offer only. The element rosters are full at six, so each is cut unless you swap it in.

---

## The highlight reel

Fifteen pieces worth getting excited about.

1. **Uraeus** (Fire). A cobra of flame rises on your shoulder and spits burning venom at whatever begins an attack
   near you. It reads the room for you.
2. **In One Breath** (Lightning's Devotion). After a perfect guard or dodge your spells have no cooldown for 2.5 s:
   wait for the one moment, then spend the whole hand in it.
3. **Bag of Winds** (Wind). Fujin's bag at your hip catches blows and arrows; open it and every caught arrow flies
   back at its archer.
4. **Deep Crack** (Water). A fist of water snaps shut like the snapping shrimp's claw; the bubble collapses with a
   crack and boils the Soaked out of every enemy near.
5. **Rime** (Wind and Ice). Frost grows only on the side facing you. Strike from that side and the ice blows out of
   its back as a cone: where you stand is the whole reaction.
6. **Capsize** (Wind and Water). Wet and off balance, the enemy goes flat on its back; with Breakers, a line of them
   goes over like dominoes.
7. **Fire-Setting** (Fire and Ice). Bank a fire against the enemy, then throw rime on it and it splits, as Egypt's
   miners split rock.
8. **The Sown Men** (Life and Metal). Throw the dragon's teeth; bronze warriors with roots trailing rise from the
   floor and strike whenever you land a heavy blow.
9. **The Stone Keeps the Hour** (Earth and Light). A standing stone throws a line of sunlight round like a dial's
   shadow; a blow that lands as the line crosses is critical.
10. **The Grudge** (Dark and Lightning). A sooty cloud gathers over the enemy's head and waits for the blow it lands
    on you. Read it, guard it, and the cloud comes down on it.
11. **Paper Seal** (Poison and Lightning). Slap a yellow seal on its brow; each blow chars it down, and the fifth
    brings the thunder.
12. **Cold Chisel** (Ice and Metal). Light blows tap a chisel of black ice in, note by rising note; the heavy blow
    drives it home and splits the armour.
13. **Under the Black Water** (Water and Dark). Black water opens under you and the enemy at once, and you come up in
    each other's places: drag the archer into the melee.
14. **The Trade** (the Massive Sword's aspect). Take small blows on purpose; every blow traded mid-swing makes that
    swing land harder.
15. **Pair Bond by rule** (`weapons.md` 15.2). Each weapon's carrier leaves a footprint and each element has a verb.
    Scythe and Water's Backwash Turn drags enemies inside the scythe's sweet spot, so the pair fights the weapon's
    own habit and teaches a step back.

---

## Every element

| Element | Devotion (all in) | Strongest spell |
|---|---|---|
| Fire | **Fire Without End**: burns never go out, and a burn left alone deepens | **Uraeus**: a cobra of flame that spits at every wind-up |
| Lightning | **In One Breath**: after a perfect guard or dodge, no cooldowns for 2.5 s | **Ring of Drums**: three drums of cloud, each beaten by a perfect guard or dodge |
| Wind | **On the Wind**: spells have no wind-up and are cast from inside a dodge | **Bag of Winds**: catch blows and arrows, then open the bag |
| Ice | **Deep Winter**: Chill never fades, and Glaze freezes all near as it lights | **Rime Guard**: a freezing fog in which a perfect guard freezes the attacker |
| Water | **The Sea Comes In**: every spell leaves standing water you fight best in | **Deep Crack**: a collapsing bubble that boils out the Soaked |
| Life | **Wood Grows Back**: what you grow grows back, and every spell sows | **Drawn From the Wood**: the garden withers into you; every blow critical and mending |
| Earth | **Stone Remembers**: nothing you raise falls by itself | **Fissure** with Basalt: a row of columns your heavy blow topples like dominoes |
| Metal | **The Guard Holds**: the guard never breaks, and you take less | **Poured Bronze**: molten bronze poured into every notch, setting as a hold |
| Light | **Full Daylight**: a mark is not spent by blows, it burns out | **Reckoning**: a clock-faced seal that pays back half of what you dealt |
| Dark | **Nothing Stays Buried**: the Cursed dead stand up and fight for you | **Devouring Maw** with Rahu: the enemy is swallowed and rolls out of the throat |
| Poison | **Tasted Every Herb**: no poison harms you, and clouds heal you | **One by One**: every poisoned enemy bursts down a line, the last the largest |

---

## All 55 pairs

Duo spell roles: opener, breaker, detonator, counter, movement. "Built" marks the 11 ring-mix spells that already
exist (paid in mana). Duo surges are listed after the table.

| # | Pair | Reaction: what it does | Duo spell (role) |
|---|---|---|---|
| 1 | Fire and Lightning | **Flashover**: its flame goes live, and every discharge near bends into it and bursts again | Ball Lightning (built; detonator) |
| 2 | Lightning and Wind | **Thunder Gust**: thrown away, its charge goes off where it lands | Eye of the Storm (built; breaker) |
| 3 | Wind and Ice | **Rime**: frost on the side facing you, blown out the back by your heavy | Avalanche (built; breaker) |
| 4 | Ice and Water | **Supercooled**: the next blow freezes it outright | The Wave Stands Still (built; breaker) |
| 5 | Water and Life | **Quickening**: the soaked seed flowers; a heavy blow bursts it to heal | Strangling Vine (built, re-aimed at the flower; breaker) |
| 6 | Life and Earth | **Split Rock**: roots wedge into the shaken ground and hold it; a heavy splits the floor | Stone Oak (built; opener) |
| 7 | Earth and Metal | **Anvil Stone**: an anvil heaves up at its back; the heavy drives it on, the armour breaks to full and throws sparks | Drawn From Stone (built; breaker) |
| 8 | Metal and Light | **Glint**: broken armour throws a ray to the next enemy | Six Mirrors (built; detonator) |
| 9 | Light and Dark | **Diamond Ring**: dark closes over it, a bead of light breaks critical | Dusk and Dawn (built, re-aimed; movement) |
| 10 | Dark and Poison | **Shut in the Dark**: a black ring seals it as a jar; the poisoned that die inside feed it, then it breaks | Wasting Grip (built, re-aimed to seal the jar; opener) |
| 11 | Poison and Fire | **Fire Medicine**: the poison catches like powder and bangs | Light the Pyres (built; detonator) |
| 12 | Fire and Wind | **Carried Embers**: embers fly downwind and light what they land by | The Fire Breathes In (opener) |
| 13 | Lightning and Ice | **Sleet Charge**: the frost charges itself until a bolt falls | Glass Storm (opener) |
| 14 | Wind and Water | **Capsize**: wet and unsteady, it goes flat on its back | Ride the Squall (movement) |
| 15 | Ice and Life | **Winter Seed**: the seed sleeps under frost and wakes as thorns at the thaw | Green Under the Ice (detonator) |
| 16 | Water and Earth | **The Ground Gives**: shaken wet ground turns to sand and swallows its legs | The Island Breathes (breaker) |
| 17 | Life and Metal | **Iron Thorns**: thorns grow through the armour and fly off with each blow | The Sown Men (opener, mana) |
| 18 | Earth and Light | **Earthlight**: light breaks up through cracks under it | The Stone Keeps the Hour (opener) |
| 19 | Metal and Dark | **Black Chain**: chained to the next enemy, sharing its wounds | The Smith's Net (opener) |
| 20 | Light and Poison | **Burning Sap**: the sun wakes the sap; the poison holds back while criticals raise weals, then lands doubled | Bared to the Sun (opener) |
| 21 | Dark and Fire | **Smoulder**: the fire sinks in, deepens unseen, then flares | The Burning Lake (opener; a zone) |
| 22 | Poison and Lightning | **Nerve Fire**: each bite jolts it and breaks its wind-up | Paper Seal (detonator) |
| 23 | Fire and Ice | **Thermal Shock**: the body cracks, and the next heavy splits it | Fire-Setting (detonator) |
| 24 | Lightning and Water | **Seized**: the muscles lock and the floor goes live | The Flashing (opener) |
| 25 | Wind and Life | **Windthrow**: rooted, so the next shove tears it out | Thistledown (movement) |
| 26 | Ice and Earth | **Frost Heave**: the frozen ground heaves it up on a mound | The Hag's Staff (breaker) |
| 27 | Water and Metal | **Rust**: the notches stay and spread, and at five a plate falls | The Sea Takes It (breaker) |
| 28 | Life and Light | **Shining Herb**: a herb springs up at its feet to pick | Turned to the Sun (opener) |
| 29 | Earth and Dark | **Turned to Stone**: stone climbs it for a heavy blow to break | Stone Underfoot (movement) |
| 30 | Metal and Poison | **Quicksilver**: every weapon blow makes its poison bite | Ichor (counter) |
| 31 | Light and Fire | **Kindling Point**: the brackets close to a burning point; criticals deepen the burn | Hands of the Sun (opener) |
| 32 | Dark and Lightning | **The Grudge**: a cloud that answers the next blow it lands on you | The Exile's Grudge (counter) |
| 33 | Poison and Wind | **Ill Wind**: thrown, it sheds all its poison in a plume along its path | The Yellow Gale (detonator) |
| 34 | Fire and Water | **Steam**: water flashes off the burning body in a scalding puff | The Sisters' Quarrel (breaker) |
| 35 | Lightning and Life | **Lightning Flower**: a red fern on its skin, and your garden throws bolts into it | Sowing Thunder (detonator) |
| 36 | Wind and Earth | **Logan Stone**: it rocks like a balanced stone, and your third blow throws it down hard | Walking Dune (breaker) |
| 37 | Ice and Metal | **Cold Short**: the armour goes brittle; the next heavy cracks its plates off | Cold Chisel (detonator) |
| 38 | Water and Light | **Caught Light**: a net of light on the wet floor marks and slows | The Gods' Road (movement) |
| 39 | Life and Dark | **Girdled**: fig roots coil tighter with each blow and girdle it; dead, it leaves a hollow trunk | Death Turned Back (counter) |
| 40 | Earth and Poison | **Chokedamp**: the cracked ground breathes out a heavy poison that runs to the other shaken | The Clay Rank (opener) |
| 41 | Metal and Fire | **Red Hot**: notched armour heats red and burns its wearer | Tipped Crucible (breaker, mana) |
| 42 | Light and Lightning | **The Bolt Runs True**: a straight bolt comes down a thread of light | Diamond Thunder (detonator) |
| 43 | Dark and Wind | **Breath Taken**: winded, it begins no attack | Blown Through (counter) |
| 44 | Poison and Ice | **Venom Rime**: the poison will not fade, and each bite chills it toward a freeze | Rime Over Rime (opener) |
| 45 | Fire and Life | **Firebloom**: burning seeds fly out and grow into flame lilies | Need-Fire (opener) |
| 46 | Lightning and Earth | **Earthed**: the charge runs to ground as a burnt fern that strikes all on it | The Capstone (breaker) |
| 47 | Wind and Metal | **Sickle Wind**: its loose plates whirl round it as blades | Three Weasels (movement) |
| 48 | Ice and Light | **Mock Suns**: two mock suns hang either side of it; a blow from the flank is critical | Sunstone (counter) |
| 49 | Water and Dark | **Drowned Weight**: it swells with black water, and bursts when it falls | Under the Black Water (movement) |
| 50 | Life and Poison | **Rot Roots**: roots join it to its neighbours, and every bite runs down them | The Stalks Rise (detonator) |
| 51 | Earth and Fire | **Burnt Mound**: three stones thrown up and heated red, for your weapon to bat across the room | Vitrified Wall (breaker) |
| 52 | Metal and Lightning | **Live Iron**: every blow on it arcs from iron to iron, to the next Sundered enemy | Lightning Seeks Iron (movement) |
| 53 | Light and Wind | **Golden Feathers**: its mark breaks into feathers that ride the wind | Sweep of Wings (opener) |
| 54 | Dark and Ice | **Stolen Warmth**: the curse takes its warmth; each blow draws it into you and leaves a Chill | Ice in the Blood (detonator) |
| 55 | Poison and Water | **Fish Poison**: it fouls the water, and the heavily dosed float up stunned for the blade | Poison the Lagoon (opener) |

**Duo surges** (eleven): Open the Gourd (Wind and Water), Great Lotus (Water and Life, built), The Ground Wakes (Life
and Earth, built), Totality (Light and Dark, built), The Yawning Gap (Fire and Ice), Ash and Thunder (Dark and
Lightning), Heated Bronze (Metal and Fire), The Net of Jewels (Light and Lightning), Wind From the Grave (Dark and
Wind), When the Watch Sleeps (Lightning and Earth), Three Winters (Dark and Ice).

**Role count of the 44 new duo spells**: openers 15, detonators 9, breakers 8, movement 7, counters 5 (the built
11 add 5 breakers, 3 detonators, 2 openers, 1 movement). Counters are the thin role if more are wanted.

---

## Every weapon

Each Way has a trunk (five temper marks, four infusion marks by element family, the gated technique, the Bond mark)
and three aspects, one active at a time. A god's weapon adds a fourth.

| Weapon | Aspect one | Aspect two | Aspect three | God's bough |
|---|---|---|---|---|
| Katana | **The Sheath**: every chain is a draw; at the keystone draws strike as heavies | **The Stream**: five cuts, dodges keep the chain, a loop with no last cut | **The Answer**: a perfect guard opens a riposte; at the keystone it answers by itself | Narukami's Storm Katana (Lightning) |
| Massive Sword | **The Wall**: the planted blade as a stance; barge and uproot from behind it | **Half-Sword**: a second grip for thrusts and the murder stroke | **The Trade**: take small blows on purpose; each trade hardens the swing | Brannoch's Standing Stone (Earth) |
| Scythe | **The Swath**: the mower's walk, rewarding tip after tip | **The Hook**: hamstring from the tip; hold enemies at the curve | **The Harvest**: tip blows bind sheaves; the last sheaf stands as a straw figure | Carnwyn's Golden Scythe (Life) |
| Great Spear | **The Long Point**: block, seize, thrust; nobody stands inside the point | **The Pole**: vault over enemies and strike down | **The Cross**: the cross spear cuts going in and coming back | Khepharu's Sun-Spear (Fire) |
| Great Hammer | **The Quake**: place the blow; three on one spot break the floor | **The Swing**: the thrower's turns, faster each time round | **The Anvil**: a late perfect guard rings the head; strike while it rings | The Smith's Hammer (Metal) |
| Twin Knives | **The Shadow**: a perfect dodge puts you behind | **The Rattle**: hold the chain open for a hundred nicks | **The Reverse Grip**: the guard becomes a parry that slips past and cuts | none |
| Great Bow | **The Stave**: the bow as a close weapon; point-blank shots shove | **The Snare**: ground arrows joined by tripwires | **The Long Draw**: a draw held as long as you dare, that pins | The Huntress's Bow (Ice) |
| Chain Sickle | **The Weight**: the meteor's whirl, a ring that grows overhead | **The Bind**: swing and hurl what is bound | **The Sickle**: the chain coiled short; hook the guard and cut | none |
| Fists | **Wrapped Hands**: the boxer's slip, short hooks, standing in | **The Hold**: grabs and throws | **The Armament**: the element coat, one element on each arm | none |
| War Fan (new) | the plain fan: closed strikes, an open sweep, a thrown fan | **The Iron Fan**: a baton that turns blades aside | **The Commander's Fan**: a signal that drives every push toward one enemy | The Wind God's Fan (Wind) |
| Bone Hook (new) | the plain hook on a line: cast, reel, heave | **The Lure**: a missed cast lies glinting and draws them in | **The Line**: anchors to swing round, tripwires | The Fisher's Jawbone (Water) |
| Hook Swords (new) | the plain hook swords: back-edge cuts, leg hook, linked whip, needles | **Linked**: the pair joined, near and far in one swing | **The Catch**: hooks that catch weapons and legs | The Alchemist's Hooks (Poison) |

### Gated techniques (R1 and R2 together)

| Land | Elite | Weapon | Technique | What it does |
|---|---|---|---|---|
| Fire | The Champion of the Gate (Decided) | Great Spear | **Hold the Gate** | Plant the spear, kick round it, rip it free into a sweep and a thrust |
| Water | The blade brother | Twin Knives | **Fillet** | A carving pass of six cuts along a held enemy |
| Water | The hook brother | Bone Hook | **Hook and Hold** | Pin a foot to the floor and leave the line staked |
| Metal | The first golden handmaiden | Great Hammer | **The Smith's Tap** | Tap a spot; the next heavy turns to it and cannot miss or be guarded |
| Ice | The pack's alpha | Chain Sickle | **Bite and Shake** | Catch, shake three times, throw aside |
| Poison | The Undying Emperor | Fists | **The Scorpion's Tail** | A heel over the shoulder at whoever stands behind |
| Earth | The Bargain | Massive Sword | **The Return Blow** | Offer the neck, take the blow, return it double |
| Life | The Wicker Man | Scythe | **Over the Shoulder** | Hook an enemy up and over to land at your tip |
| Lightning | The Last Retainer (new) | Katana | **Before the Thunder** | The quickest draw; it breaks an attack in its wind-up |
| Wind | The Hermit of the Pass (new) | War Fan | **Turn the Spear** | Step along a thrust and strike its bearer |
| The champion's land | The Teacher (new) | Great Bow | **Only the Eye** | A closing ring on one point; let go as it closes |
| none | | Hook Swords | none | No elite is left (see decisions) |

---

## Decisions the owner needs to make

**The owner's answers (2026-11-18):**
- **1. Merge:** yes. The bone hook becomes the Chain Sickle's god's bough. The Poison god's hook swords also merge, into the Twin Knives as a form (their god's bough). Ten weapon kinds remain.
- **2. Elites:** the Teacher is the champion's land's elite, with the bow as his main weapon and moves that draw other weapons, as the last champion does. The Lightning and Wind elites wait for that land's overview.
- **4. Hook Swords:** they are now the Twin Knives, which the blade brother teaches.
- **5. Pair count:** 110 pairs.
- **6. Cuts:** yes, cut the dead marks. Depth is to be added where the trees thin out, with marks that bend the standard rules.
- **8. Spare ring-mix spells:** asked about, not yet decided.
- **Later the same day:** the Lightning and Wind elites are decided: the Last Retainer (Katana) and the Hermit of the Pass (War Fan). See `docs/design/WORLD.md`, Lightning and Wind. Their techniques are still drafts.

1. **Merge the Bone Hook into the Chain Sickle?** `weapons.md` recommends it: a thrown, catching, pulling line is
   what the chain already is. The fisher's Jawbone would become the Chain Sickle's god's bough, and the Lure and Line
   aspects would join the chain. You decided the hook is its own kind, so it is written whole; this is the one place
   two Ways risk feeling alike. If merged, the hook brother and the pack's alpha would both teach the chain, so one of
   them must teach something else.
2. **Three new elites**: the Last Retainer (Lightning, Katana), the Hermit of the Pass (Wind, War Fan), and the Teacher
   (the champion's land, Great Bow). Each exists to give a weapon its technique.
3. **The demo's Gate Captain.** The Last Retainer would replace him when the castle is reworked, because his spear
   repeats the Champion of the Gate's. Until then he stays as built and teaches nothing.
4. **Hook Swords have no gated technique.** Eleven elites, twelve weapon kinds. The offer: the Undying Emperor
   teaches *Hook the Dead* (both hooks behind the knees, the enemy hops stiff-legged onto your crescents) and the
   Fists lose The Scorpion's Tail. Or accept that one weapon has none, or add a twelfth elite.
5. **Pair Bond is 132 pairs, not "about 110"**: 12 weapon kinds by 11 elements. The design's figure counted ten
   weapons. 121 if the Bone Hook merges. Each pair's strike is built by rule, with twelve hand-made standouts.
6. **The element writers' cuts** (summarised):
   - Every plain minor (about 14 a tree) goes; Empower marks and Bond ranks carry the numbers.
   - Every "costs less mana" mark on a basic spell is dead (Short Breath, Quick Clap, Easy Breath, Thin Ice, Low
     Water, Ready Soil, Near to Hand, Ready Steel, Ready Light, Ready Poison).
   - Broken by cooldown spells: Spring Tide (Water), Paid in Blood and Heartwood (Life), Returning Light (Light),
     Live Wire as built (Lightning; fires nearly always now).
   - Repeats: Red Harvest's built clone (Poured Blade has the one water copy), Kindle's "every blow sets a burn"
     (repeats Cinder), Earth's Rampart, Metal's Hone, Poison's Rotten Fruit.
   - Folded in as shapes: Cinder Fan, Lodestone, River Serpent, Standing Rain, Hollow Statue, Thorn Ring, Black Ice,
     Swinging Censer, Toppling, Stone Skin, Sentence, What It Owes, Dazzle; The Dead Stand into Dark's Devotion.
     (Since the review fixes, Standing Rain, Hollow Statue and Thorn Ring are cut outright, and Sunspot and Moon on
     Water with them; see the changelog.)
   - Gone: Landslide, Ricochet, Splinter, Shatter Point, Unpaid, Feast, Glut, Borrowed Strength, Left to Ripen, One
     Sickness, Lance the Wound, and keystones Open Furnace, Scorched Earth, Overclock, Storm Front, Second Skin, River
     Without End, Shared Roots.
   - Moved: Mountain Cleaver to Earth's all-in strike; the Rise from Dark's gift to its root; Poison's self-heal to
     its root; the Katana-bound marks to the Katana's Way; Full Body Armament to the Fists.
   - Rebuild: Returning Edge to its trees_b design (the built one is a reskin of Devouring Maw).
7. **Ice's and Earth's larger spells.** Neither roster had a mana spell, against the rule. Winter Wall now costs 35
   mana and the new Cairn 40. Say if you would rather a different spell carry it.
8. **The built ring-mix spells** stay mana-paid larger duo spells (40 to 60), and their spare spells are cut unless
   you swap one into an element roster (list under "Names and moved spells" above).
9. **Fire's Heat and the Smith's Hammer's heat** are two meters with one name. A Fire player with the Hammer would see
   both. Rename one (the Hammer's could be its glow), or let Fire's Heat drive the Hammer's.
10. **Earth's Devotion and Brannoch's stones overlap a little**: both hurt an enemy driven into a standing stone
    (and so does Earth's Standing Stones major). Fine as "stone remembers" twice, or trim one.
11. **Still close, left as they are, for your eye**: several decoys that enemies turn to strike (Coiled Strike's
    Shed Skin, Seven Flashes' Left Behind, the Scythe's Last Sheaf, Root Walk's Changeling; Ski Run's Left Standing
    is now Thin Ice); Rot Roots passes poison bites to others (Fouled Water, which did it by pool, is now Fish
    Poison); Poured
    Bronze, Tipped Crucible and the Hammer's Quench all pour molten bronze (on notches, on a lane, in a disc).
12. **Two names in the old `docs/design/weapons.md`** (Water's Undertow for the spear, Lightning's Live Wire for the
    chain) repeat built tree marks. They need new names when built.
13. **Twin Knives and Hook Swords** are both two short blades. `weapons.md` keeps them apart by what each asks; if one
    must go after play, it is the plain Hook Swords' Catch aspect.

---

## Engine needs

Gathered from all seven files and de-duplicated. **Most needed** marks the pieces that unlock the most content.

### Foundations (most needed: everything stands on these)

- **Two afflictions on one enemy at once**, and a **reaction trigger** when the second lands, with a rest kept per
  enemy and pair and a flag on what a reaction lays so it starts nothing. All 55 pairs.
- **The cooldown model**: per-spell cooldowns, weapon play cutting every running cooldown, and "take N seconds off
  running cooldowns" as an action (Glass Bell, Strike While Hot, Short of Breath, Blown Through, Need-Fire,
  Wellspring's The Flask Breaks, Tailwind and the Masters).
- **The infused strike** on L2 with R1 and R2, its own cooldown, and the deep strike's mana.
- **The R1 and R2 chord** as an input action (every gated technique).

### Enemy states

- **A timed state on one enemy that runs an effect on a beat** (most needed: the schema's missing per-enemy debuff).
  Corona, Sleet Charge, Rime, Supercooled, Iron Thorns, Smoulder, Cracked, Seized, the Grudge's cloud, Kindling Point,
  Cold Short, Red Hot, Breath Taken, Live Iron, Stolen Warmth, Venom Rime, Drowned Weight, Paper Seal, Ichor, Girdled,
  Burning Sap's wait, Flashover's live flame and more: well over twenty pieces.
- **Knocked down** (most needed): a timed fall with its own clip. Capsize, Logan Stone (after rocking), Drifted Over,
  Shed Plate's slip, Thrown on the Reef, Walking Dune, Fire Front, the Hamstring, the Hip Throw, tripwires and snares,
  Hooked Twice, Low Turn.
- **Thrown into something** (most needed for Wind and Water): a thrown body that meets a wall, a stone or another
  enemy (Against the Wall, Gale Edge, Thrown on the Reef, Sent Sliding).
- **A coat that waits for fire** and lights on any Burn (Black Pitch).
- **Rooted**: cannot move or be shoved, can still attack (Windthrow; Hold stands in now).
- **Begins no attack** without stopping movement (Breath Taken).
- **Cannot guard** for a time (Red Paper, The Burning Shirt; Open Line is close).
- **Wind-up slowed or delayed** on one enemy (Rime Guard, Uraeus's Coiled, Catch the Arm, Seize, Ground Shiver, Rap
  the Knuckles, Cut the Wrist, Grey Stone).
- **Damage dealt or taken changed** on one enemy (Snapped Blade, Red-Short, Drowned Weight).
- **Sight cut short** (Blind Steam, Water's Ink).
- **Sinking**: a pool that slows, forbids dodges and leaps, and draws the body lower (The Ground Gives).
- **Disarmed**, **Hooked** (moves, but the line is yours), **pinned** (held by a staked point).
- **Affliction rules per enemy**: Sundered past three (Rust); Burn that sinks in unseen (Smoulder); an affliction that
  stops fading while a condition holds (Venom Rime, Neither Wins, Permafrost, Stolen Warmth); a mark not spent but not
  renewable (Full Daylight); a quickened poison beat (Ichor).

### Triggers and memory

- **Downwind** (most needed): the direction from the player through the enemy. Carried Embers, The Wind Carries It,
  Hot Wind, Bolt from the Blue, Breakers, Down the Passage, Fire Lance, Breath Taken, Golden Feathers, Sandblast.
- **The direction an enemy was last shoved** (Sent Downwind, Golden Feathers).
- **An enemy beginning an attack** (Uraeus, Gust's and Whirlpool's Masters, Devouring Maw's Master, Before the
  Thunder).
- **Side of a blow** (Rime's facing side, Mock Suns' flank).
- **A thaw**: a freeze or Chill ending (Winter Seed).
- **The player walking over a thing** (Shining Herb, Firebloom's lilies, Seed Under Snow, Carnwyn's blooms).
- **A tally per enemy** (The Exile's Grudge, Cut Unfelt, Reckoning, A Hundred Cuts) and a cast that remembers its
  targets (The Flashing).
- **Stacks drawn off enemies into a store** (The Yellow Gale, All Into One, Gathered In).
- **Second press, triple press and hold of a slot spell** (most needed among inputs): Fire-Setting, The Tide Turns,
  To the Blade's There and Back, Aruna's Road, Three Weasels; held: The Yellow Gale, Diamond Thunder, Held Thunder,
  Held Breath, Shut Door.
- **Paid in health**: a cast on cooldown paid in health (Blood Price); a cooldown on a surge (Hunger).

### Forms

- **Standing things that block and can be struck** (thunderstone, cold shade, black ice statues, Girdled's hollow
  trunk, Clay Rank soldiers, the turf bank of Ditch and Bank, Left in Rubble's heaps).
- **Things on the floor a weapon swing knocks along** (Burnt Mound's stones).
- **Links between enemies**: a leash that passes blows (Black Chain, The Smith's Net), roots that pass bites (Rot
  Roots), threads that pass discharges (The Net of Jewels).
- **Hazards that follow an enemy** (Sickle Wind's whirl, Mock Suns) and **ground
  laid behind a moving enemy** (Slag, Ill Wind).
- **Dash through enemies** (Ride the Squall, Thistledown, Lightning Seeks Iron, Three Weasels; near `drill`).
- **The player under the ground** (Under the Mound, Root Walk) and **swapping places** with an enemy (Under the Black
  Water) or a grown thing (Changeling).
- **Moving ridges** (Walking Dune) and **a sweeping line round a planted stone** (The Stone Keeps the Hour).
- **Helpers**: warriors that answer the heavy blow (The Sown Men), a rank that marches and pushes (The Clay Rank), a
  rime creature (The Drops Quicken).
- **A weapon blow that flings an orbiting piece** (Glass Storm).
- **Catching thrown attacks and sending them back** (Bag of Winds; the War Fan's Still Air is close).
- **Wind fields** that push and carry curses downwind (Wind From the Grave).
- **Pickups** (Shining Herb, Firebloom).

### Weapons and the player

- A grip or stance flag that swaps a combo (Half-Sword, the War Fan shut and open, Linked).
- Weapon counters: Heat, sheaves, nicks, weights, drums, layers of the coat.
- An answer window after a perfect guard (the Katana's Answer, the Hammer's ring).
- Anchors in the scenery for the Bone Hook (a tag on colliders).
- Pair Bond: the strike records its footprint and its struck; a mirrored `Echo`.
- The Rise on Dark's root; a heal over time from poison damage (Poison's root); clouds that heal the player (Poison's
  Devotion); stones that never fade (Earth's Devotion).

---

## Changelog of the reconciliation

### Name clashes (42: 41 fixed here, 1 already fixed by its writer)

| Name | Where it clashed | What changed, and in which file |
|---|---|---|
| The Ground Gives | Earth's detonator and the Water and Earth reaction, also the same sinking idea | `elements_b`: the spell is now **Cairn**, stones heaped on the shaken, and it is Earth's mana spell. The reaction keeps the name (liquefaction is the pair's image) |
| The Smith's Net | Metal and Dark duo spell; Water and Metal duo spell | `pairs_d3`: Water and Metal's is **The Weighted Net**. Metal and Dark keeps the invisible net of the myth |
| The Net | Metal's Circle of Steel shape, a third net | `elements_b`: **The Golden Chair** |
| Bag of Winds | Wind's counter; Poison and Wind duo spell | `pairs_d3`: **The Yellow Gale**, and it no longer pulls enemies (that was Indrawn Breath) |
| Ill Wind | Poison and Wind reaction; Dark and Wind duo surge | `pairs_d4`: the surge is **Wind From the Grave** |
| Briar Patch | Life's tap spell; an Overgrowth mark | `pairs_d1_d2` already renamed the mark **Thorns Take Root**; confirmed |
| Sun on the Blade | Fire's deep strike; Light and Fire major; Light's all-in strike | Light keeps it. `elements_a`: **Burnt Offering**. `pairs_d3`: **The Sun Points** |
| Burning Glass | Metal and Light shape; Light and Fire shape; Ice and Light shape | Light and Fire keeps it. `pairs_d1_d2`: **Burning Mirror** (Syracuse). `pairs_d5`: **Lens of Ice** |
| Golden Sickle | Life's Reap What Is Sown shape; Life and Metal shape | Life keeps it. `pairs_d1_d2`: **Pruning Hook** |
| Mistletoe | Life's Sapling shape; Life and Light duo spell; Life and Poison shape; Scythe and Life standout | The Scythe's standout keeps it (Carnwyn's blade). `elements_a`: **Dodder**. `pairs_d3`: **Life-Herb**. `pairs_d5`: **Gathered In**. `pairs_d4`: The Favoured Oak's sprig is now an oak leaf |
| Quicksilver | Metal and Poison reaction; One by One shape; Poison and Water shape | The reaction keeps it. `elements_b`: **All Into One**. `pairs_d5`: **Silver Pool** |
| Cinnabar | Light and Poison minor; Metal and Poison minor | Metal and Poison keeps it. `pairs_d1_d2`: **Bright Venom** |
| Deep Crack | Water's detonator; Fire and Ice shape | Water keeps it. `pairs_d3`: **Crazing** |
| Quench | Fire and Ice shape; Hammer and Metal standout | The Hammer keeps it. `pairs_d3`: **Hot Shards** |
| Fire Whirl | Wildfire Rush shape; Fire and Wind shape, both a pulling column | Fire and Wind keeps it. `elements_a`: **Fire Front**, a wall of flame that carries a crowd |
| Thunderhead | Waiting Thunder shape; Lightning and Wind shape | Lightning and Wind keeps it. `elements_a`: **Hanging Cloud** |
| Thunder Draw | Lightning's deep strike; the Storm Katana's built skill | `elements_a`: **Down the Channel** |
| Earthing | Chain Lance shape, beside the Lightning and Earth reaction Earthed | `elements_a`: **Planted Bolt** |
| Lightning Rod | Lightning's built spell; Metal and Lightning shape | `pairs_d5`: **Draws the Bolt** |
| Undertow | Water's built passive; Wind and Water major | `pairs_d1_d2`: **Rolled In** |
| Bellows | Fire and Wind major; Wind and Metal major | Wind and Metal keeps it (the smith's bellows). `pairs_d1_d2`: **Fan the Flame** |
| Grit in the Wind | Poison and Wind major; Wind and Earth major | Wind and Earth keeps it. `pairs_d3`: **Spring Dust** |
| Downwind | Poison and Wind major; the engine's direction word | `pairs_d3`: **Gust at the Heel** |
| Yellow Wind | Poison and Wind shape, beside the new Yellow Gale | `pairs_d3`: **Dust Bank** |
| Dust Devil | Lifting Wind shape; Wind and Metal shape, both wandering whirls | Wind keeps it. `pairs_d5`: **Sent Downwind** |
| Hot Iron | Metal and Fire major; the Smith's Hammer keystone | `pairs_d4`: **Heated Edge** |
| The King's Men | Under the Mound shape; Earth and Dark shape | Earth and Dark keeps it (the stones that cannot be counted). `elements_b`: **Ditch and Bank** |
| Root and Branch | Life's reaction lift; Wind and Life major | `pairs_d3`: **Torn Up** |
| Seen Through | Light and Poison major; Ice and Light shape mark | `pairs_d5`: **Through the Ice** (Clear Ice is a built Ice minor) |
| Sunstone | Earth and Light shape mark; Ice and Light duo spell | Ice and Light keeps it. `pairs_d1_d2`: **Lit Stone** |
| Seven Strides | Quake Step shape; Brannoch's move, the same walking quake | Brannoch keeps it. `elements_b`: **Struck From Afar**, a stamp on a far enemy |
| Standing Stone | Stone Rise shape; Life and Earth shape; beside Earth's built Standing Stones | `elements_b`: **Raised Stone**. `pairs_d1_d2`: **Split Rock** (new twist, below) |
| Stone Remembers | Earth's Devotion; Brannoch's number mark | The Devotion keeps it. `weapons`: **Long Standing** |
| Rivets | Flung Blade shape; the Smith's Hammer's skill | The Hammer keeps it. `elements_b`: **Cherry Red** |
| Hot Rivets | Metal and Lightning minor, beside both of the above | `pairs_d5`: **Bare Wire** |
| Long Glide | Light and Wind minor; the Huntress's Bow number | `pairs_d5`: **Riding High** |
| Long Gust | Gust's shape; the War Fan's number | `weapons`: **Strong Gust** |
| Open the Bag | War Fan move, beside Open the Gourd and The Bag Opened | `weapons`: **Held Gale** |
| Sickle Wind | Wind and Metal reaction; Katana and Wind standout | The reaction keeps it. `weapons`: **The Air Closes** (new idea, below) |
| Thorn Wall | Overgrowth's built spell; Carnwyn's move | `weapons`: **Thicket Line** |
| Ringing Steel | Metal's reaction lift, beside the Forge reaction Ringing Iron | `elements_b`: **Every Crack Rings** |
| The Noose | Dark's Grasp shape; Water and Dark reaction; beside Yama's Noose | `elements_b`: **Tethered**. `pairs_d5`: the reaction is reworked (below) |

Two new names were changed again before they settled, because they hit existing ones: Backwash (a Pair Bond name)
became Rolled In, and Clear Ice (a built minor) became Through the Ice.

### Idea clashes (24 rows: 19 reworked, 4 settled by a rename above, 1 checked and clean)

| Clash | What changed, and why |
|---|---|
| **Underground travel, four times** | Earth's **Under the Mound** keeps travel under the ground. Life's **Root Walk** stays: it goes to your own garden, not along the stick. Earth and Dark's **Under the Hill** is now **Stone Underfoot** (`pairs_d3`), a stride that turns the enemy to stone at once. Water and Dark's **Under the Black Water** now swaps you and the enemy through black water (`pairs_d5`) |
| **A leash between two enemies, twice** | Water and Dark's reaction The Noose was Black Chain to the number (6 m, 5 s, 4 m apart, 30% shared). Black Chain keeps it. `pairs_d5`: the reaction is now **Drowned Weight**, from Varuna swelling the guilty with water: it swells, slows, takes more, and bursts in black water; its shapes and marks were rebuilt to match |
| **A mark that spreads** | Ice and Light's **Mock Suns** threw marking rays at the nearest enemy, as Golden Feathers and Halo's Glint already do. `pairs_d5`: the mock suns now flank the enemy, and a blow struck from beside one is critical and keeps the mark |
| **Caustic light** | Light's own look in `elements_b` included the nets sunlight throws through water. Taken out: they belong to Water and Light's **Caught Light** alone |
| **Shatter chains** | Checked every shatter across the files. None repeats The World Holds Still (a shatter breaking the next frozen one); Sleet Charge had already stepped round it |
| **A fern of lightning over the floor, twice** | Lightning and Life's **Lightning Flower** and Lightning and Earth's **Earthed** both spread a burnt figure 4 m over the floor that struck all on it. Earthed keeps it (the charge goes to ground). `pairs_d4`: the fern is now on the body, and every grown thing of yours throws a bolt into it |
| **A fire trail behind a moving enemy, twice** | Metal and Fire's **Slag** and Dark and Fire's **Coal Seam** both left burning patches as it walked. Slag keeps it. `pairs_d1_d2`: Coal Seam is now one smouldering patch to lure enemies onto |
| **A trail behind a thrown body, twice** | Fire and Wind's keystone **The Wind Carries It** laid fire along a thrown enemy's path, which is Ill Wind's reaction in fire. `pairs_d1_d2`: the keystone now lets your heavy blows aim the embers downwind |
| **A cast that sets and is broken, twice** | Metal and Fire's **Cast in Gold** repeated Metal's Poured Bronze (Set). `pairs_d4`: now **The Burning Shirt**: the hot plates sear it each time it attacks, and at five Burn it cannot guard |
| **Stone at its back for another branch** | Life and Earth's **Standing Stone** shape fed the Forge's Hammer and Anvil, which a Life and Earth player cannot carry, and repeated the Forge's Anvil Stone. `pairs_d1_d2`: now **Split Rock**, the roots hold longer and your heavy splits the floor |
| **The three-stroke whirlwind, twice** | The Katana and Wind standout repeated Wind and Metal's **Three Weasels** (trip, cut, close the wound). `weapons`: now **The Air Closes**, a draw that cuts the air and the air rushing back |
| **A hook and line, twice** | Water's **Hook and Line** spell (cast, reel, swing round a point, snare a foot) repeated the Water god's Bone Hook and its technique. `elements_a`: now **The Tide Turns**, the ebb drawing you out and the flood bringing you in |
| **A walking quake, twice** | Quake Step's Seven Strides was Brannoch's Seven Strides. `elements_b`: **Struck From Afar** |
| **The beheading game, twice** | Mountain Stance's **The Bargain** shape (take the blow, return it threefold) was the Massive Sword's technique **The Return Blow**, taught by the elite named the Bargain. `elements_b`: now **Grey Stone**, the Cailleach gone to stone |
| **Needle grids, three times** | The Poison god's weapon throws needles that stand in grids (decided in `WORLD.md`). `pairs_d3`: Metal and Poison's **Hidden Needles** is now **Ichor** (a counter that pulls Talos's nail). `elements_b`: Five Needles' **Threads** shape is now **Red Paper** (it cannot guard) |
| **Step through, then a critical heavy, twice** | The built Eclipse spell **Dusk and Dawn** is Light's built Step Into Light again. `pairs_d1_d2`: re-aimed to step into the enemy's shadow and come out of the rim, setting off Diamond Ring |
| **Walk over a thing to heal, four times** | Shining Herb keeps it. `pairs_d4`: **Corpse Flowers** now make grey health real as you fight beside them. `pairs_d1_d2`: **All-Heal** (a mistletoe sprig to walk over) is now **Sap of Iron**, a quarter flask at once |
| **A pulling column of flame, twice** | Wildfire Rush's Fire Whirl and Fire and Wind's Fire Whirl both pulled enemies in. Handled with the name (Fire Front, above) |
| **Draw everything in, then blow it out, three times** | Wind's Indrawn Breath keeps it. `weapons`: the War Fan standout **The Bag Opened** now carries you behind the gale. `pairs_d3`: **The Yellow Gale** drinks only the poison |
| **The strike that comes again, three times** | Light and Lightning's keystone **The Flash, Then the Sound** repeated Lightning's lift Strikes Twice. `pairs_d4`: now **The Diamond Falls**: criticals call the bolt with no Static |
| **A sprig on the enemy struck for a reward** | Life's Sapling shape and Life and Light's spell were the same plant. Handled with the names (Dodder, Life-Herb) and their looks |
| **A decoy left behind, too often** | Under the Mound's stone figure and Share Alike's gecko tail were two more decoys among many. `elements_b`: Ditch and Bank (above), and **Shed Tail** now lashes what follows you |
| **A metal net as a single hold** | Handled with the name (The Golden Chair) so the net belongs to Metal and Dark |
| **Earth's pits** | Handled with Cairn (above) |

### Rules conformed

| Rule | Files changed |
|---|---|
| Infused strike: 8 s on each button, deep strike 20 mana | `elements_a` (was 6 s), `elements_b`, `weapons` |
| Element mark prices | `elements_b` (now states them; it had none) |
| One larger spell per element in mana | `elements_a` (Winter Wall, 35 mana), `elements_b` (Cairn, 40 mana) |
| Duo branch prices | `pairs_d1_d2` (eight Majors at 4 to 3, one Minor at 2 to 1, two duo spells at 4 to 5), `pairs_d3` (nine duo spells at 4 to 5), `pairs_d4` (44 "Mix" marks given their kinds and prices, and the duo spell price stated), `pairs_d5` (eleven Shape marks named, eleven Minors at 2 to 1, the duo spell price 4 to 5) |
| Reaction rest and no-cascade | `pairs_d1_d2` (the rule, now shared), `pairs_d3` (6 s stated as its own), `pairs_d4` and `pairs_d5` (the rule stated); seven deliberate cascades flagged |
| Folded Steel | `pairs_d1_d2` (the folds last until the next rest fire) |
| Katana-only marks and Full Body Armament | `elements_a`, `elements_b` (cut lists), `weapons` (the Katana's element infusion marks) |
| The R1 and R2 chord | `weapons` (Bare-Handed King no longer reads as a chord) |
| "The Way", not "the weapon tree" | `pairs_d1_d2` |
| Moved mix spells are offers | `pairs_d1_d2` (the rule, and Lotus Step is not a movement spell) |

### Review fixes applied (2026-11-18)

The owner said "apply the recommended fixes" of `review_elements.md` and `review_pairs.md`. Each Weak piece took the
review's recommended option, each empty Shape was filled, the Masters moved to their element's own timing, and the OK
pieces with a clear recommendation were changed. The standouts the pair review named were not touched.

**`elements_a.md`** (Fire, Lightning, Wind, Ice, Water, Life)

| Was | Now |
|---|---|
| Sunspot (Fire spell) | **Black Pitch**: blows smear pitch over a crowd, the first fire takes all of it. Shapes **Pitch Pool** and **The Torch**; Master kept from Sunspot (lit by a finisher) |
| Kindle's new half (cooldown cuts) | **Fed Flame**: Kindle lasts while blows feed it |
| Kiln Stance's Learn (the pillar parry) | **Shut Door** made the Learn; the built pillar is Shape A, **The Kiln**; Master: a burning attacker's Burn goes off |
| Uraeus's Master (a free spit on a perfect dodge) | A spit on a burning enemy lays double Burn; the spits are drawn as burning venom (pitch is Black Pitch's) |
| Backdraft (passive) | Renamed **Breath of the Lioness**, so the backdraft is Shut Door's alone |
| Falling Sun (look) | Drawn as a burning ball, never a white orb |
| Chain Lance's Planted Bolt | **Stepped Leader**: you draw the bolt's path, the heavy calls the return stroke |
| Lightning Rod (look) | Drawn as a sword planted point-up (Takemikazuchi); rule unchanged |
| Lifting Wind's Downdraft | **Turned About**: the column spins it to face away |
| Wind's charged blade (1 m more reach) | **Gale Edge**: blows shove, the third throws into what is behind |
| Against the Wall (a small burst) | As designed: thrown bodies that meet a wall, stone or enemy |
| Indrawn Breath's Master (after a finisher) | Cast mid-dodge, the draw rides with you |
| Ski Run's Left Standing | **Thin Ice**: the lane is lake ice that breaks under followers; Master: slide through a shatter |
| Unmoved (keystone) | **Sent Sliding**: the heavy sends a frozen body down the ice |
| Moon on Water (Water spell) | **Blowhole**: Still Water kept; Undertow Cut became **Blown Out** |
| Whirlpool's Standing Rain | **Rip Current**: a lane that carries them away |
| Reeled In (deep strike) | **Thrown on the Reef** |
| Catch the Rain | **Drawn From the Drowned** |
| Wellspring's new half | **The Flask Breaks**: each flask bursts out as a wave |
| Riptide's Master (after a perfect guard) | A second press within 1 s sends the wave out again, carrying them past you |
| Briar Patch's Thorn Ring | **Burrs**; Master: cast on something grown, it becomes a thicket |
| Red Harvest (Learn, paid in health) | The spell is now **Drawn From the Wood**: the garden withers into you. Feed the Ground keeps the one health price |
| Life's charged blade (mends 3) | **Sprout**: blows plant saplings |
| Thorn Set (infused strike) | **Grown Into It**: the enemy becomes part of the garden (Briar Root grows from the cutting) |
| Thin Blood | **The Garden Holds You** |
| Regrowth's new half (mend 6 a spell) | **The Garden Grows** |
| Elder Tree's Master (after a finisher) | Planted on something grown, it grows at once |

**`elements_b.md`** (Earth, Metal, Light, Dark, Poison)

| Was | Now |
|---|---|
| The Hill's Weight (Earth lift) | **Left in Rubble**: reactions leave heaps that stand as your stones |
| Mountain Stance's Learn (the fist parry) | **Grey Stone** made the Learn; the built fist is Shape B, **Fist of Rock** |
| Masters of Stone Rise, Fissure, Cairn | Standing your ground; a heavy on the crack's line; standing still 1 s |
| Under the Mound's Master (an unguardable heavy) | A heavy with Tremor 2 that breaks more balance (passing guards stays Metal's and Light's) |
| Grave of Swords' Grave (empty) | **Clashing Bronze** |
| Ring of Spears' Together, Standing Light's Lamp, Reckoning's The Hand Turns, Daybreak's Dawn (all empty) | **The Drawn Line**, **Row of Lamps**, **Witnessed**, **Red Before the Sun** |
| Masters of Step Into Light, Ring of Spears | Cast through two in a line; a critical blow while the spears hang |
| Debts Called's Called In, Gone to Ash's Behind It, Second Shadow's Changed Places, Pawned Heart's Pawned (all empty) | **The Black Buffalo**, **Ash on the Brow**, **Return to the Shadow**, **Buried Heart** |
| Dark's charged blade (10% as health) | **Shadow Cut Loose** |
| Every Debt Collected (Dark lift) | **Collected Twice** |
| Masters of Grasp, Debts Called, Second Shadow | Within 1 s of being struck; below a third of your health; within 1 s of being struck |
| Miasma's Creeping, Coiled Strike's Snake, Blister's Lanced, Share Alike's Share Alike, One by One's Snap (all empty) | **Low Mist**, **Centipede's Run**, **Weeping Sore**, **Carried on the Blade**, **Eaten in Turn** |
| Bitter Mix (Poison lift) | **The Delayed Touch** |
| Masters of Miasma, Five Needles, Share Alike | On the poison's beat |

**`pairs_d1_d2.md`**

| Was | Now |
|---|---|
| Flashover (two set-offs at half) | Re-aimed on flame that conducts: the flame goes live and draws every discharge near; Ball Lightning drawn violet-cored |
| Strangling Vine (no water in it) | Re-aimed: it drinks the Soaked and opens Quickening |
| Taking Root (reaction) | **Split Rock** is the reaction; **Taking Root** is the short-hold shape |
| Ringing Iron (reaction) | **Anvil Stone** is the reaction; **Ringing Iron** is a shape |
| Brood (reaction) | **Shut in the Dark** (the review's The Sealed Jar, renamed apart from The Last One in the Jar); Swarm moves the jar; Wasting Grip re-aimed to seal it |
| Burning Sap (criticals make it bite) | Given the hogweed's delay: held 3 s, weals raised by criticals, then all at once; Blistered and Bright Venom follow |
| Bitter Lantern (duo spell) | **Bared to the Sun** |

**`pairs_d3.md`**

| Was | Now |
|---|---|
| Rust (a quiet creep) | At five a plate falls (The Plate Falls folded in); the freed Major is **Salt Guard** |
| The Weighted Net (opener) | **The Sea Takes It** (breaker) |
| Life-Herb (duo spell) | **Turned to the Sun** |
| Ill Wind's trail (one dose) | Sheds all its doses in a plume |
| Turned Back (shape) | **Dust on the Blade** |

**`pairs_d4.md`**

| Was | Now |
|---|---|
| Topple (reaction) | **Logan Stone** is the reaction; its old shape slot is **Drifted Over**; New Face, The Prophecy Broken and Walking Dune's wording follow |
| Cold Short's crack (damage) | Its plates crack off: notches stay, 25% more from weapons |
| Spall (shape) | **Shed Plate** |
| Lightning Flower with nothing grown (a small crack) | The fern itself throws a bolt to the nearest Seeded |
| Grave Roots (reaction) | **Girdled**; Hollow Trunk is now its end, and its slot is **Fig in the Crown**; Held Down and What the Yew Keeps follow |
| Blackdamp (reaction) | **Chokedamp** is the reaction; **Blackdamp** is a shape |

**`pairs_d5.md`**

| Was | Now |
|---|---|
| Prism (reaction) | **Mock Suns** is the reaction; **Prism** is a shape; Wide Fan follows |
| Baked Earth (reaction), Fired Clay, Running Seam | **Burnt Mound**, **Heaped Stones**, **Fire-Cracked**; Hot Seams is **Hot Stones**; Vitrified Wall kept |
| Live Iron (arcs to any enemy) | Arcs only from iron to iron |
| Draws the Bolt (shape) | **Forged Bolt** |
| Frostbite (reaction), Swallowed Warmth | **Stolen Warmth**, **The Long Night**; Deep Veins and Hel's Half follow |
| Fouled Water (reaction), Silver Pool, Fish Poison (shapes) | **Fish Poison** is the reaction; shapes **Rock Pool** and **The Ebb Takes It** |
| The Hundred Rivers (duo spell) | **Poison the Lagoon** |

**`depth_elements.md`** (dependent pieces): Lit Fuse's base is Black Pitch; Lotus Step's base is Blowhole and it opens
its lotuses on the spray; Creeping Ice loses its hold by the feet (Thin Ice holds now); The Winged Sun spits venom;
The Hour Freezes is drawn as rime racing over the world; Water Takes the Blow speaks as the reef that takes the
swell; Grown Through Them no longer names Thin Blood; Grave Soil runs Shut in the Dark's rule; Trip Hammer rings as
Anvil Stone.

**This page**: Uraeus's highlight spits venom; Life's best spell is Drawn From the Wood; seventeen pair rows; the role
count (Weighted Net was an opener, The Sea Takes It is a breaker); the engine needs.

**Names changed for clashes**: The Sealed Jar became **Shut in the Dark**; Black Pitch's shape "The Wick" became
**The Torch** (The Wick is a Fire-land enemy); Backdraft became **Breath of the Lioness**.

## What could not be settled here

- The decisions above, which are yours.
- Balance: the deep strikes in the two element files differ in weight, and every number is a first guess.
- The distance-1 built ring-mix spells' roles were read from their built text, not designed; they may want re-aiming
  at their reactions the way Dusk and Dawn was.
