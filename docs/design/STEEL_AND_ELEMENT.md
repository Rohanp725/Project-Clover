# Steel and Element: how the player fights and grows

The design for making the element as strong a part of the game as close combat, and for a weapon tree that
specialises and upgrades the weapon. Worked out between the owner and Claude on 2026-11-18.

**How to read the status marks.** **Decided** means the owner said so. **Proposal** means Claude's design, waiting
for the owner's word or for play. A number is always a first guess to be tuned by play. The record of what was
decided and when is the section "Steel and element: the rework of 2026-11-18" in `docs/GAME.md`. This document is
the same design, written to be read in one go.

Contents: 1 the idea in one page; 2 what other games taught us; 3 the fight; 4 the elements you own and carry;
5 the Hand; 6 mixing; 7 the Way; 8 Bond; 9 currencies and death; 10 the hub and the story; 11 three builds, start
to finish; 12 what changes in the game as built; 13 the order to build it; 14 open questions.

---

## 1. The idea in one page

**The pillar (Decided).** Steel and element are equal partners. Every fight uses both, and each makes the other
stronger: weapon play speeds the magic, and magic that lands empowers the weapon. This replaces "close combat comes
first" from 2026-11-15.

**What the player does in a fight.** They swing, guard and dodge as now. Between blows they cast from four spells
held on the L2 layer, each on a short cooldown that their weapon play shortens. A spell that lands charges the blade,
so the next blows carry the element. Two elements on one enemy react. Mana is kept for the big moves: the surge,
the larger tree spells, the charged versions.

**What the player builds.** At a rest fire they set their **Hand**: one or two elements to carry, four spells, a
surge. In the hub they choose which of their collected weapons and elements to carry. On **the Way** each weapon
has three forms (aspects), one active at a time, and a shared Body branch for health, stamina and flasks.

**How they grow.** The game is linear now: everything bought is kept through death. **Shards** from every kill buy
the element trees; **Memories**, from beating people, buy the Way. **Bond** grows on whatever is carried, so
sticking with a build pays, and changing it costs nothing already earned.

**What keeps it from overwhelming.** Slots open one at a time through the first land. One build screen, at most
about thirteen choices for a slot. Nothing permanent can be chosen wrongly: the only real risk is carrying unspent
Shards into a death.

---

## 2. What other games taught us

| Game | What it does | What we take |
|---|---|---|
| Hades II | Every fight starts with full Magick; holding attack, special or cast makes a charged Omega move that costs it | Magic from the first second, and on the same buttons as the weapon |
| Final Fantasy XVI | Abilities on cooldowns of 12 s to 2 min, melee in the gaps; a well-timed block cuts some cooldowns by half; mastered abilities move to any loadout; free refunds | Cooldowns make magic constant instead of hoarded; skill with the weapon speeds the magic; no trap choices |
| Nine Sols | Parries deal no damage; they load Qi, which powers the talismans that do most of the damage | Defence charges the magic |
| Kingdom Hearts 2 | Empty MP refills over time, and attacking meanwhile fills the Drive gauge | Never locked out of magic, and running dry pushes the player into melee, not away from the fight |
| Black Myth: Wukong | Light attacks build Focus for heavies; spells on cooldowns and mana | Two rhythms in one fight |
| Diablo IV | A skill, then its enhancement, then one of two upgrades | Depth per spell, one small question at a time |
| Genshin Impact, Divinity: Original Sin 2 | Elements react when they meet: on an enemy, on the ground | Every pair of elements does something without a menu |
| God of War (2018) | Each weapon has a tree that buys moves | A weapon tree buys moves, not only numbers |
| Hades | Each weapon has aspects that change how it plays; keepsakes rank up the longer they are carried | The Way's boughs; Bond |
| Sekiro | Prayer beads found in the world raise health and posture | Body upgrades as things found |
| Bloodborne | The enemy that killed you can hold your blood echoes | What happens to lost Shards |
| Elden Ring, Lies of P | Affinities make a weapon fire or lightning; Lies of P builds a weapon from blade and handle | Where weapon and element meet is the interesting part |
| Avatar: The Last Airbender | Each bending style is a real martial art | The element is a way of fighting, not a spell list |
| Demon Slayer | Elemental breathing styles fought through the sword as numbered forms | Named weapon-and-element moves |

---

## 3. The fight

### 3.1 Spells on cooldowns (Decided), shortened by weapon play (Decided)

Each of the four spells has its own cooldown. Weapon play shortens every running cooldown.

| What the player does | What it takes off every running cooldown (Proposal) |
|---|---|
| A weapon blow that lands | 0.3 to 0.6 s, by move (heavier blows more) |
| A perfect guard | 2 s |
| A perfect dodge | 1.5 s |
| A finisher | 3 s |

**Proposal**: basic spells on cooldowns of 6 to 12 s. Some spells' master marks (5.2) tie a reset to timing: a
perfect guard makes the next cast of that spell instant.

### 3.2 Mana for the big moves (Decided)

Mana no longer pays for the four basic spells. It pays for the surge, the larger tree spells, charged versions of
spells and the weapon skill.

- Earned as now: weapon blows that land, perfect guards, perfect dodges, finishers.
- **Proposal**: 100 at most. Out of a fight, mana below half rises slowly to half and mana above half stays. So the
  player never starts a fight empty, and what they earned in the last fight is kept.

### 3.3 The charged blade: magic feeds the weapon (Proposal)

A spell that lands charges the blade. The next three weapon blows carry the element: each lays its affliction and
hits 20% harder. A charged blade shows on the weapon (flame along the edge, crackle, frost).

So the loop runs both ways: weapon play speeds the magic (3.1), and magic powers the weapon. Weaving weapon, spell,
weapon is the strongest way to fight; spamming either alone is weaker.

### 3.4 Infused strikes (Proposal)

L2 with R1 or R2 is an infused strike: a weapon move carried by an element. L2 with R1 uses the first element in the
Hand, L2 with R2 the second. All in on one element (5.6), L2 with R2 is a deeper version of the first. Each weapon's
Way can change how its infused strike looks and works.

### 3.5 Controls

| Action | PlayStation | Xbox | Keyboard and mouse (Proposal) |
|---|---|---|---|
| Move | Left stick | Left stick | W A S D |
| Light attack | R1 | RB | Left mouse button |
| Heavy attack (hold to charge) | R2 | RT | Right mouse button |
| Guard | L1 | LB | Left Shift |
| Dodge | Circle | B | Space |
| Flask | Square | X | R |
| Weapon skill (Proposal) | Triangle | Y | Q |
| Use | Cross | A | E |
| Surge | L2, tap | LT, tap | F |
| Four spells (Decided: an L2 layer) | Hold L2, then Square, Triangle, Circle or Cross | Hold LT, then X, Y, B or A | 1, 2, 3, 4 |
| Infused strikes | Hold L2, then R1 or R2 | Hold LT, then RB or RT | Hold Left Ctrl, then a mouse button |
| Pause | Options | Menu | Escape |

Every action can be rebound, for pad and keyboard, from the controls menu (owner's request of 2026-11-18). With the
mouse, attacks aim toward the cursor (Proposal). While L2 is held, the HUD shows the four spells in a diamond laid
out like the buttons, each with its cooldown ring.

---

## 4. The elements you own and carry

### 4.1 Owning an element

- **Decided**: **Take** makes the god's element the player's for good, so it can be carried, and gives the god's
  weapon (with its own signature aspect on the Way, 7.1).
- **Decided**: **Consume** does not give the element. It pays a large sum of Shards and Memories and strengthens one
  element the player already owns: for example a free Devotion mark or a free mark on one of its branches.
- An element's tree is kept through death once its marks are bought.

### 4.2 Carrying elements

- **Decided**: the player carries one or two elements at once. Two are a mix (section 6); the same one twice is all
  in (5.6).
- **Decided**: the build changes only at rest fires.

### 4.3 Slots open gradually (Decided)

So that each piece is used on its own before the next arrives (the way Hades and Kingdom Hearts teach):

| When (Proposal) | What opens |
|---|---|
| The start | One element, two spell slots, the surge |
| Partway through the Fire land (the Champion of the Gate) | The third spell slot |
| The Fire god | The fourth spell slot |
| After the first god | The second element |

---

## 5. The Hand: the build screen at a rest fire (Proposal, built on Decided pieces)

### 5.1 What the Hand holds

| Part | What it is |
|---|---|
| Two elements | Which owned elements are carried. The same one twice is all in |
| Four spell slots | Each holds any learned spell of a carried element, or a duo spell while both its elements are carried |
| One surge slot | Either element's surge, or a duo surge once learned |
| Two infused strikes | The first and second element on the weapon (3.4) |

That is the whole build: four spells and a surge, from one element or two.

### 5.2 Each spell is a short branch on its element's tree (Decided: spells come from branches on the trees)

| Mark | What it does |
|---|---|
| Learn | The spell |
| Empower | Stronger, shorter cooldown |
| Shape | One of two versions. Swappable freely at any rest fire, so there are no trap choices |
| Master | A reward tied to timing |

Example, Thunderclap: Learn (the clap); Empower; Shape: *Rolling Thunder* (three claps marching in a line) or *Held
Thunder* (hold, then one huge clap that staggers); Master: a perfect guard makes the next Thunderclap instant and
resets its cooldown.

About six spells per element give about 24 marks of spell branches. With about ten passive marks (the affliction, the
surge, reactions, Devotion), a tree is about 34 marks, the size the trees already are. The shape changes, not the
size. The spells already built become the branches.

### 5.3 Roles

Each spell carries a small icon for its role:

| Role | What it does |
|---|---|
| Opener | Reaches out and lays the affliction |
| Breaker | Fills the stagger meter |
| Detonator | Spends afflictions for a burst |
| Counter | Powered by a perfect guard or dodge |
| Movement | Repositions the player |

Building four slots then reads like building a hand of cards: "two openers and no detonator". The gaps in a build show
without reading paragraphs.

### 5.4 The screen

- The four slots sit in a diamond laid out like the buttons, the surge slot in the middle.
- Choosing a slot lists only the spells that can go there, grouped by element, each with its role icon and one plain
  line. At most about thirteen choices.
- The numbers sit behind a "details" toggle.
- Up to three saved Hands, switched at a rest fire.
- A suggested starter Hand for each element, which can be ignored.
- The training dummy in the hub, for trying a Hand before taking it out.

### 5.5 Nothing goes permanently wrong

Bought marks are kept forever. Shapes, slots and Hands change freely at rest fires. The only real commitment is
spending Shards before a death takes them.

### 5.6 Devotion: all in on one element

Carrying the same element twice turns on its **Devotion**, a strong passive no mix can have, and deepens the second
infused strike. Examples: Ice, *Deep Winter*: chill never fades and the surge freezes at once. Fire: burns never go
out by themselves.

The choice is real either way: a mix brings reactions and a duo spell; all in brings Devotion and depth.

---

## 6. Mixing

### 6.1 Every pair reacts (Decided)

When two afflictions meet on one enemy they react. Eleven elements make 55 pairs, and every one does something from
the first time it happens. Players discover mixes by playing, without a menu. Each reaction is one small effect,
written as data.

| Pair | Afflictions | First idea for the reaction |
|---|---|---|
| Fire and Water | Burn and Soaked | Steam: a cloud that blinds what is inside |
| Ice and Lightning | Chill and Static | A frozen enemy struck shatters into lightning that jumps to the next frozen one |
| Fire and Wind | Burn and Unsteady | The fire spreads to every enemy near |
| Earth and Water | Tremor and Soaked | Mire: the ground takes the enemy to the knees and slows it |
| Metal and Lightning | Sundered and Static | The broken armour conducts: every blow arcs to the next Sundered enemy |
| Life and Poison | Seeded and Poisoned | The seed grows rot: poison bites spread through the roots to neighbours |

Afflictions must be able to sit on an enemy two at a time.

### 6.2 Upgrades strengthen reactions and change them (Decided)

On two levels (Proposal):

- **On an element's own tree**, a mark can lift every reaction that element is part of. A Fire mark makes all ten of
  Fire's reactions hotter.
- **On the pair's duo branch**, marks deepen that one reaction and offer a choice of two shapes that change what it
  does. Burn and Soaked: a blinding steam cloud, or a scalding burst. Shapes swap at rest fires.

### 6.3 Duo branches and duo spells (Decided: a branch for every pair)

- A duo branch opens while both its elements are carried, and is bought with Shards.
- **Proposal**: each is small: three to five marks deepening the reaction, crowned by a **duo spell** (Decided: a
  combination spell) and, on some branches, a duo surge.
- A duo spell takes one of the four slots and works only while both elements are carried. Example, Ice and Lightning:
  *Glass Storm*, charged shards of ice that turn round the player and arc between frozen enemies.
- The 11 ring mixes already built (Plasma, Tempest, Blizzard, Glacier, Bloom, Overgrowth, Forge, Halo, Eclipse,
  Blight, Pyre) become 11 of the 55. 44 are new.

---

## 7. The Way: the weapon tree

### 7.1 Aspects (Decided)

Each weapon has three boughs, each a different form of the weapon, like Hades's weapon aspects. One is active at a
time, chosen at a rest fire. Marks can be bought on all three over time, so a player who keeps everything still has
to choose what their weapon is today.

Each bough buys moves as well as numbers (new chain enders, launchers, stances) and ends in a keystone that changes
how the weapon plays. Examples (Proposal):

| Weapon | Aspects |
|---|---|
| Katana | **Iai**: decisive single draws. **Flow**: long chains and cancels. **Counter**: follow-ups after a perfect guard |
| Great Spear | **Reach**: long thrusts and spacing. **Vault**: the spear as a pole to leap and strike down. **Phalanx**: guarding thrusts and holding ground |
| Great Hammer | **Quake**: blows through the floor. **Momentum**: spins that build speed. **Anvil**: heavy counters off the guard |

**Decided (owner, 2026-11-18)**: taking a god's weapon unlocks it as a bough on that weapon's Way. Choosing that
bough gives the god's weapon itself, its look and its moves; choosing any other bough gives the plain weapon with
that bough's form. A player who changes off it can always change back. Where a god's weapon is a kind no other
weapon is (the war fan, the bone hook, the hook swords), its Way is new and the god's bough sits beside two plain
ones; Consume gives the plain weapon and those two.

### 7.2 Temper marks (Decided)

The Way is the only thing that improves a weapon. Raw damage comes from temper marks on it; there is no separate upgrade
level and no smithing materials.

### 7.3 Infusion marks (Proposal)

Where the weapon meets the carried element: marks that change how this weapon's infused strike (3.4) works. Katana
with Fire leaves a burning trail on the draw; Katana with Lightning turns the draw into a dash.

### 7.4 Gated techniques (Decided)

Some marks on the Way open only by beating a particular miniboss. Each miniboss teaches one technique for **its own weapon
only**. The fight is weapon-first: the miniboss uses that very move against the player, so they see it before they
learn it.

The technique unlocks even if the player is using another weapon, and waits for them. That is meant to make players
try other weapons and find what they really like, not stay with what they started with.

With about one elite per solo land and two in each duo land, there are about ten minibosses, roughly one per weapon.
Example: the Champion of the Gate (Fire) teaches a Great Spear technique.

### 7.5 The Body branch (Decided: bought and found)

Health, stamina, flask count, flask strength and poise.

- Bought with Memories on a shared Body branch of the Way.
- Also found in the world and given by gods, like Sekiro's prayer beads. Exploring is rewarded, never required.

---

## 8. Bond: a reason to stay, with no penalty for changing (Decided)

### 8.1 Bond

Every owned weapon and element has a Bond that grows only while it is carried, and never decays.

- **It grows from**: blows landed, enemies killed, elites and gods beaten while carrying it. A god counts most, so
  what the player beat a land's god with feels like theirs.
- **About five ranks**, each a small, steady bonus, kept modest so changing never feels like a punishment. A weapon:
  faster recovery, a stronger chain finisher. An element: shorter cooldowns, deeper afflictions.
- **At the top rank, a Bond mark** available no other way: a weapon's aspect in its true form (the Katana's Iai draws
  twice); an element's second Devotion-strength passive.
- **The look shows it**: the weapon wears in, the element's effects grow richer.

### 8.2 Pair Bond

A weapon and an element used together grow a shared Bond. At the top it gives an **infused strike unique to that
pair**: a Katana-and-Lightning draw no other pairing has. Splitting the pair loses the strike, not each one's own
Bond.

The result: the player can always change, and a fresh pick is fully usable; but a build they have lived with is
noticeably stronger and holds things no one else's does.

---

## 9. Currencies and death

| | Shards | Memories |
|---|---|---|
| Come from | Every kill | The first defeat of each kind of enemy; elites and named fighters; gods (Proposal, so they cannot be farmed) |
| Buy | The element trees and the duo branches | The Way: aspects, temper marks, gated techniques, the Body branch |
| On death | Unspent ones are lost (below) | Kept (Decided) |

Memories were the owner's idea: a player with no past fills the gap with other people's memories and the techniques
in them.

**Death (Decided).** The game is linear: everything bought is kept. Unspent Shards follow the Bloodborne rule:

- The enemy that killed the player glows and carries them. Killing it gives them back.
- Dying again before that loses them for good.
- **Proposal**: after a fall or a hazard, they lie where the player last stood on safe ground.

---

## 10. The hub and the story

- **Decided**: at a rest fire the player can go to the hub, choose other weapons and elements from those collected,
  and go back.
- **Decided**: the reason. What the player takes from the gods does not come back into time with them: the patron
  keeps it. A mortal body carries one weapon and two elements into the world at once. A rest fire is where time is
  thin enough to step out and choose.
- It feeds the story's doubt: the patron is quietly gathering everything taken from the gods.

---

## 11. Three builds, start to finish

**Fire and Lightning, Katana.**
- Hand: Flame Wave (opener, lays Burn), Thunderclap with *Held Thunder* (breaker), Seven Flashes (detonator), Ball
  Lightning (the Plasma duo spell). Surge slot: Overdrive.
- In a fight: Flame Wave lays Burn, which charges the blade; three charged draws; Thunderclap lays Static on the
  burning enemy and they react; a perfect guard takes 2 s off everything, and Seven Flashes is ready again.
- The Way: the Katana's Iai aspect. Pair Bond with Lightning is growing toward its unique draw.

**All in on Ice, Great Bow.**
- Hand: Hoarfrost (opener), Eight Points (detonator), an Ice counter spell, an Ice movement spell. Surge slot: Glaze
  with The World Holds Still. Devotion: *Deep Winter*.
- In a fight: frozen enemies stay frozen; shattering one breaks the next; the bow's infused shot plants ice.

**Water and Life, Scythe** (an unusual pair, made possible by a duo branch for every pair).
- Hand: Riptide (opener, Soaked), Briar Patch (Seeded), Bloom's duo spell, a Water movement spell. Surge slot:
  Regrowth.
- Their reaction, shaped on the Bloom branch, heals the player for each soaked, seeded enemy struck by the blade's tip.

---

## 12. What changes in the game as built

| Area | As built now | After the rework |
|---|---|---|
| Pillar 2 | Close combat comes first | Steel and element are equal partners |
| Structure | Run-based; a fall wakes the player in the hub with nothing learned | Linear; everything bought is kept; unspent Shards follow the Bloodborne rule |
| Mana | Starts at 0; pays for every spell, the surge and the weapon skill | Basic spells on cooldowns; mana for the surge, larger spells, charged versions, the weapon skill; rises to half out of a fight |
| Spells | One tap spell, one worn spell (held Triangle) | Four spells on the L2 layer, built in the Hand |
| Elements | One held; Take switches it and forgets the old tree | One or two carried; owned elements kept; Take gives an element for good, Consume strengthens one owned |
| Element trees | Root, three boughs, four or five spells, a gift (428 marks, 11 trees and 11 mixes) | Spell branches plus passives and Devotion; 55 duo branches |
| Afflictions | One kind on an enemy at a time in practice | Two at once, and every pair reacts |
| Weapons | One per fight; swapping leaves the old one on the ground; no tree | Collected and chosen in the hub; each has a Way with three aspects, temper and gated techniques |
| Health and stamina | Fixed except a few minor marks | The Body branch, bought with Memories and found in the world |
| Currencies | Shards only | Shards for the elements, Memories for the Way |
| Controls | Triangle is the tap spell; L2 the surge; weapon skill on held heavy; keyboard keys only; no Xbox; no rebinding | The table in 3.5; Xbox support; every action rebindable |
| HUD | Mana bar | Adds the spell diamond with cooldown rings and the charged-blade sign |
| Enemies and bosses | Tuned for a player with little magic | Retuned for a player casting every few seconds |

The weapon movesets, the spells, the afflictions and most of the tree content stay. They become content inside the
new structure.

---

## 13. The order to build it (Proposal)

1. **Controls first.** Xbox support, rebinding of every action, the L2 layer as input actions, the keyboard layout.
   The owner asked for the first two already.
2. **Cooldown spells, mana for big moves, the charged blade**, using today's spells, in the combat test arena.
3. **Two elements and the Hand** at the rest fires, with the HUD's spell diamond. Slots opening gradually.
4. **Reactions**: two afflictions on an enemy, the reaction table, the first six reactions.
5. **Trees reshaped** into spell branches, Fire and Lightning first, with Devotion and the first new duo branch.
6. **The Way**: Memories, the Katana's three aspects, temper marks, the Body branch.
7. **Linear progression**: kept upgrades, the Bloodborne rule for Shards, the hub's armoury of collected weapons and
   elements.
8. **Bond and Pair Bond.**
9. **Gated techniques**, starting with the Champion of the Gate.

Then filling in: the other trees, the 55 reactions and 44 new duo branches, each weapon's aspects, the Pair Bond
strikes. Each step is played before the next is built.

---

## 14. Open questions

- **Pair Bond's size: Decided (owner, 2026-11-18).** About 110 pairs. Each pair's strike is built by rule from the
  weapon's infused strike and the element's substance, with about a dozen hand-made standouts for the pairs the gods
  carry.
- **Content.** 55 reactions, 44 new duo branches, three aspects for each weapon: a lot. Which come first after Fire
  and Lightning?
- **The found Body pieces**: their name and how many there are per land.
- **Gods as Bond**: whether beating a god with a build gives more than Bond (a mark, a look).
- **The hard lock-on** for bosses, still open from 2026-11-15, matters more with spells aimed at one enemy.
- **Balance**: every number here.
