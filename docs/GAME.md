# Project Clover: the game

This file is the source of truth for what the game is. Claude reads it before designing anything and writes to it when a
decision is made. Empty sections mean the decision has not been made yet; do not invent an answer, ask.

Each item carries a status: **Decided** (the user said so), **Leaning** (the user's current preference, not final),
**Draft** (a first idea the user expects to change), **Open** (not decided; see Open questions).

## Pitch

You are a mortal with no past, chosen by an outcast god of time to climb through the domains of the elemental gods
and kill them one by one. Combat is close, hard and readable from above: a dodge, a perfect guard, a weapon, and
magic you can only afford by landing hits. Each god is the master of one element and one weapon, and when it falls
you choose: take its element and become something new, or consume it and grow what you already are. When you die,
your patron turns time back and the gods remember nothing. You do. And the further you climb, the less sure you are
what your patron wants the elements for.

## Pillars

Approved by the user on 2026-11-15.

1. **Combat is difficult and immersive.** Fights reward learning the enemy (references: Elden Ring, Lies of P).
2. **Close combat comes first.** Elemental magic is a large part of a fight but never the main focus; every magic
   feature must make the player want to use their weapon more, not less.
3. **Elements and techniques mix.** The player's build is a combination of elements and weapon techniques, and
   combining them (an element empowering a weapon) is where the depth is.
4. **Every boss is a master and a choice.** Each boss shows one element and one technique at full strength, and
   beating them means choosing which of the two to take.

## Player fantasy and core loop

- **Decided**: the game is about fighting bosses; the player goes through all of them.
- **Decided**: after a major boss the player chooses to take **either** its element **or** its technique.
- **Decided**: the player is the only one who can hold several elements, apart from the final boss.
- **Decided**: run-based, and every run includes all the bosses. Run length is not yet known; the user's worry
  stands that a late death in a long, hard run is costly.
- **Decided**: between runs the player keeps unlocked starting weapons and starting elements.
- **Decided**: boss order varies inside tiers. The final boss is always last and the late bosses are
  always late, though their order among themselves can change. A reward not chosen is gone for that run.
- **Decided**: the very first run starts with fists and one chosen element. Later runs start with a choice among the
  unlocked weapons and elements.
- **Decided**: one weapon per fight. Some weapons have forms the player can change between. There is no weapon
  inventory: swapping leaves the old weapon on the ground where it was dropped.
- **Decided**: Full Body Armament is not a weapon pickup. It is an upgrade that appears in the element tree while no
  weapon is equipped.
- **Decided**: after a boss the player makes one choice. **Take**: the player's element and upgrade tree switch to
  the boss's, the upgrade points spent on the old tree are lost, and the player gets the boss's own special weapon
  and access to that boss's signature perk, a very strong upgrade available no other way. Each element has its own
  upgrades, and the later the boss, the stronger its element. **Consume**: the player keeps their element, gains
  upgrade points for their current tree (possibly unlocking mixed branches with the consumed element), and gets a
  plain version of the boss's weapon with nothing special about it.
- **Decided**: between bosses there are areas to explore with ordinary enemies. Those enemies use elemental
  combinations and weapons against the player, which is how the player discovers what is possible.
### Defence and resources

- **Leaning**: a dodge roll that costs stamina, somewhere between Elden Ring's roll and Hades's dash. There will be
  fewer projectiles to dodge than in Hades.
- **Decided**: a perfect guard in the manner of Lies of P, available to every build.
- **Decided**: magic is paid for with mana harvested by hitting enemies with the weapon.
- **Decided**: healing is a limited number of flasks, refilled at rest points.
- **Decided**: dying ends the run. Exceptions: the Dark element's revive, and possibly some later elemental upgrades.
- **Decided**: the game is designed for a controller, in the manner of Elden Ring, Lies of P and Black Myth: Wukong.
- **Decided**: the self buffs in the element table are only each element's base. The upgrade trees are where the
  inventive moves go, borrowing ideas freely from all sorts of games.

## Combat

### Combat system

Chosen by Claude on 2026-11-15 at the user's request ("research the good soulslike combat systems and take the good
pieces"), from Claude's knowledge of the games named. **Adopted as the working design; any line can be changed.**

| Piece | Taken from | How it works here |
|---|---|---|
| Committed attacks | Dark Souls, Elden Ring | Every attack has a wind-up, a moment it hits, and a recovery. Attacks cost stamina. |
| Input buffer and chains | Elden Ring, Nioh | A press during an attack is remembered and fires the next hit of the chain. Light, light, light; a heavy placed after one or two lights is a finisher. |
| Recovery cancel | Hades | The dodge can interrupt the recovery of an attack (never the hit itself), so play from above stays quick. |
| Soft aim | Hades, Death's Door | An attack turns toward the nearest enemy in the direction of the stick. A hard lock for bosses comes later if it is missed. |
| Guard and regain | Lies of P | Holding guard blocks a hit for stamina and a little "grey" health damage; grey health is won back by landing hits soon after. |
| Perfect guard | Lies of P, Sekiro | Guarding just before a hit lands costs nothing, hurts the attacker's stagger meter, gives mana, and is the trigger for element upgrades (Backdraft, Stored Charge, Burn and Detonate). |
| Stagger meter | Sekiro (posture), Lies of P (groggy), Elden Ring (stance) | Hits, heavies above all, and perfect guards fill an enemy's stagger meter. A full meter leaves it open for a few seconds; a heavy attack then is a finisher for large damage. The meter drains when the enemy is left alone. |
| Red attacks | Sekiro, Lies of P | An attack telegraphed in red cannot be blocked: dodge it or perfect-guard it. |
| Perfect dodge | Bayonetta, Black Myth: Wukong | A dodge at the last moment gives mana, and is the trigger for Afterimage. |
| Mana from fighting | Hollow Knight, Black Myth: Wukong | Mana comes only from landing hits, perfect guards and perfect dodges. It pays for element spells and the weapon's skill. |
| Weapon skill | Elden Ring (Ashes of War), Lies of P (Fable Arts) | Each weapon has one signature skill paid for with mana. The Katana's is the Iaido Slash; holding the heavy attack charges it. |
| Flasks that refill by fighting | Lies of P | With no flasks left, landing hits slowly earns one back. |
| Weight in the hit | every good action game | A few hundredths of a second of frozen time on a hit, a flash and a shove on the target, a camera shake on heavies. |
| Read from above | Hades | Large enemy attacks mark the ground they will hit before they land. |

Controller: R1 light attack, R2 heavy attack (hold to charge), L1 guard, Circle dodge, Square flask, Triangle weapon
skill, L2 and the face buttons for element spells (to be settled).

Katana, first version: the sword lives in its sheath and every chain starts with a draw. Light chain of three (a
drawing cut, a return cut, a stepping finisher). Heavy overhead. Held heavy sheathes and releases the Iaido Slash.

Build order: (1) light chain, soft aim, hit weight, stagger meter, mana, a training dummy; (2) heavy, finisher and
the Iaido Slash; (3) guard, regain and perfect guard, with a dummy that attacks; (4) flasks, perfect dodge, red attacks.
All four are built in the combat test arena as of 2026-11-15; the user has played all four and approved the feel.

How the pieces work as built (first values, all to be tuned by play):

- **Heavy button**: tapped, a heavy overhead cut. Pressed after one or two lights it comes out at once, quicker and
  harder, and ends the chain. Held about 0.4 s with 30 mana, releasing gives the Iaido Slash; without the mana the
  hold becomes a plain heavy. The Triangle button is not used yet: the weapon skill is on the held heavy only.
- **Finisher**: a staggered enemy pulses gold. Pressing heavy with one within 5 m gives the finisher instead of a
  swing: the player steps in to it and drives the blade through, with its own animation, a long stop, slowed time
  and a camera shake. It does three times its damage, ends the stagger, costs no stamina, gives 15 mana, and blows
  pass through the player while it lasts. Light hits on a staggered enemy do one and a half times.
- **Guard**: blocks for stamina (0.9 per point of damage); 35% of the damage comes through as grey health. A blow
  the stamina cannot pay for breaks the guard: full damage and a long flinch. The guard and the dodge can both cut
  an attack's recovery short. The player moves at 45% speed while guarding.
- **Decided 2026-11-15 (user)**: blocking is directional. The guard covers the front 200 degrees; a blow from behind
  lands. Raising the guard squares up to the nearest enemy at once and keeps turned to it; with no enemy near, the
  stick turns the player.
- **Perfect guard**: a guard pressed within 0.18 s before the blow. No cost, 15 mana, and the attacker's stagger meter
  takes a large hit. Pressing guard again within 0.4 s gives no perfect window, so it cannot be hammered.
- **Grey health**: won back at 0.6 per point of damage dealt; fades after 2.5 s without a block or a hit. An
  unguarded blow removes it. Guarding alone cannot kill.
- **Red attacks**: go through a held guard. A dodge or a perfect guard answers them.
- **Perfect dodge**: a blow that passes through a roll within its first 0.13 s. 12 mana and a third of a second of
  slowed time.
- **Flasks**: three, 45 health each, 0.8 s to drink with the heal at 0.4 s; a blow or a roll before the heal wastes
  only the time. With none left, 120 damage dealt earns one back.
- **Being hit**: a 0.35 s flinch that interrupts everything, with a short push back.
- **Open**: a hard lock-on for bosses, and with it a way to choose whom the guard faces when several enemies are near.

### Elements (Draft)

Eleven elements. Each has a self buff; these are the user's "super basic" starting ideas and each element is meant to
get much more, possibly a branching specialisation and combinations between elements.

| Element | Self buff (first idea) | Other notes |
|---|---|---|
| Fire | Damage up | |
| Water | Healing items heal more | |
| Light | Purification: removes all status effects | |
| Dark | One revive as a "zombie": health drains constantly, dealing damage heals, an empty bar is death | |
| Metal | Defence up: take less damage | |
| Poison | Heal over time, based on the damage the poison deals | Attacks add damage over time, in bursts |
| Life | Heal | |
| Lightning | Critical damage up | |
| Wind | Better dodge | |
| Ice | Temporary barrier that blocks the next hit | |
| Earth | Resistance to negative status effects | |

- **Decided**: elements can combine with weapons to empower them.
- **Open**: the shape of each element's upgrade tree and which mixes exist.

### Techniques (Draft)

A technique is a weapon style. Nine weapons and one special form.

| Technique | Character | Notes |
|---|---|---|
| Massive Sword | Slow and heavy, very large area and damage | |
| Full Body Armament | The body is coated in an element and fights as a bruiser | Best with some elements: lightning as a speed fighter, earth as a slow armoured bruiser, ice |
| Knife | Fast and agile | Suits wind, lightning, poison |
| Chain | A family of chained weapons: flail, chain-sickle, nunchaku | |
| Katana | Elemental draws; a single-draw iaijutsu with lightning or wind | |
| Scythe | To be designed | Ideas below |
| Great Bow | To be designed | Ideas below |
| Great Spear | Mid range | Many directions possible |
| Great Hammer | Like the Massive Sword but blunt | Suits heavier, slower elements |
| Super Form | Boosts all attributes and gives parts of every weapon mode | Takes all technique slots; the final boss shows it at full strength |

**Built so far** (2026-11-16): Katana, and as Claude's defaults the Massive Sword, the Scythe (idea (a) below, the
sweet spot), the Full Body Armament, the Great Spear, the Great Hammer and the Twin Knives; see "The newer weapons as
built", "The Great Spear and the Great Hammer as built" and "The Twin Knives as built" at the end of this document. The design for all eight remaining
techniques is `docs/design/weapons.md`. The Great Bow and the Chain Sickle were built last, also as Claude's defaults:
see "The Great Bow and the Chain Sickle as built" at the end. All eight are now in.

Ideas offered by Claude for the two undesigned weapons, not yet accepted:

- **Scythe**: (a) sweet-spot weapon, where the blade tip at the end of a wide arc hits hardest and the shaft hits
  weakly, so spacing is the skill; (b) hook and pull, dragging enemies toward the player or the player toward them;
  (c) reaping, where hits build a harvest that is spent on a large finishing sweep or on healing.
- **Great Bow**: (a) a close-range heavy bow, with slow point-blank shots that stagger and a melee bash with the bow
  itself; (b) planted arrows, shot into the ground or into an enemy and later detonated by an element (this is where
  Landmines fits); (c) a charged pinning shot that holds a target in place to open it up for melee.

### Skills (Draft list)

Tied to a weapon: Iaido Slash (Katana), Crescent Slash (Scythe), Entangle (Chain), Flying Spear (Great Spear), Fan of
Knives (Knife), Spinning Hammer Toss (Great Hammer), Energy Reflect (Full Body Armament), Elemental Spike Line (Massive
Sword), Landmines (Great Bow).

Not tied to a weapon: Self Buff, Blades From Above, Simple Blast, Giant Beam, Phalanx Blades (blades hover near the
player and fire on their own after a short time), Dragon Blast (a summoned dragon head attacks), Energy Explosion (a
burst around the caster), Tracking Explosion Orb (a slow orb that follows and then detonates), a very large charged
bomb, Elemental Spiked Floor, Elemental Parry, Element Drill, Element Minions, Many Bullet Spawn (many projectiles
placed around the target, then fired together), Reverse Blast.

## Bosses

- **Decided**: each major boss is the sole master of one element and uses a technique no other boss uses.
- **Decided**: each boss lives in a zone that represents its element. The zone is for atmosphere only and applies no
  status effect.
- **Decided**: every magic user other than the player and the final boss uses one element only.
- **Leaning**: later bosses are duos whose elements overlap: Lightning with Wind (weather) and Earth with Life (the
  ground), possibly under a "heaven and earth" motif. They must be fair as well as hard; the user disliked Elden
  Ring's duo fights.
- **Decided**: the final boss wields Light and Dark together with the Super Form, combines both elements in every
  attack, and wears armour that is half light and half dark. That is phase one.
- **Leaning**: final boss phase two is a monstrous form.

How the numbers fall out of the above (Claude's reading, to be confirmed): five solo bosses (Fire, Water, Metal,
Poison, Ice), two duo fights (four bosses), and the final boss. That is nine bosses before the final one, and there
are exactly nine weapon techniques, so each can carry one, with the Super Form left for the final boss.

## Story and setting

- **Decided**: gameplay comes first; the story should still be properly formed, with twists.
- **Decided**: fantasy. The bosses are the gods of the elements, each ruling a domain. The final boss, holding Light
  and Dark, stands above them.
- **Decided**: the gods were mortals once who took their elements. The player, taking elements from them, is doing
  what they did.
- **Decided**: the player is a mortal sponsored by a being with power over time who opposes the gods. The sponsor
  turns time back after each death, so the gods remember nothing of earlier runs.
- **Decided**: the sponsor speaks with the player and seems trustworthy. Later the player learns the sponsor may not
  be good-hearted.
- **Decided**: the player begins with no past. The twist about the sponsor may reveal one.
- **Decided**: the tone is grim and sparse, in the manner of Sekiro. This is the voice for all in-game text: few
  words, plain and weighty, no jokes.
- **Decided**: every god has a second, godly form: a huge monster, beast or figure. The later the boss, the more
  extravagant the form.
- **Decided**: between runs there is a hub, where the sponsor speaks and the player picks a starting weapon and
  element.
- **Decided**: the first time the game is played it opens on a hellish battlefield that serves as the tutorial. A
  mortal army's assault on the gods is being destroyed, and the player is one nameless soldier in it, with a starting
  element and a weapon. They fight through ordinary soldiers, then face the final boss, the god of Light and Dark,
  and fall. As they die the sponsor saves them, and the price is the player's past: they wake with no history and
  only their fists. The player can fight in that last duel, dodging and guarding for a while, so the loss does not
  feel cheap. This opening is not part of the vertical slice.
- **Leaning**: the gods are corrupted, and the corruption spreads. Ordinary enemies are infected with their god's
  element: it shows on their bodies, it is in their attacks, and it has driven them mad.
- **Decided for now**: the sponsor is only a voice. Its form and name wait until the story is more settled.
- **Open**: what the sponsor really wants, what the player's past is, the sponsor's name and form.

## Look

- Camera: **Decided for the vertical slice**: top-down in the manner of Hades. It is a trial: if the user dislikes
  it once the slice is playable, it changes. Worries to test: whether combat feels good from above, and the camera
  when the player is near or behind something.
- Camera distance: **Decided for now**: 12 m at a 40 degree field of view and 55 degree pitch, closer than the first
  try so the character reads. **Open**: how the camera frames much larger bosses (pulling back for a fight is one way).
- Characters: **Decided**: the player character is made for this project in Blender, through the Blender MCP
  connection, so it matches the look; no bought or downloaded character packs.
- Style: **Decided**: cel-shaded with outlines, like an anime. A dark, muted world (stone, ash, storm cloud) in which
  the elements are the only saturated colour, so every spell and elemental effect stands out. The user's condition:
  it must be done well or it will not fit, so it is judged on screenshots early. References: Hades, Cuphead and
  Splatoon for vividness; Elden Ring and Lies of P for the dark setting; Black Myth: Wukong, Elden Ring, Sekiro and
  Lies of P for flashy set-piece moments.
- **Decided (user, 2026-11-15): the main visual reference is Ravenswatch**: a dark fairy-tale, comic-book look with
  cel shading and bold ink outlines. Secondary: Death's Door and Tunic for clean, minimal flat-shaded 3D; Hades for
  the camera angle, silhouette clarity and effect readability. What to aim for:
  - *Shading*: stepped toon lighting, two or three hard bands, no smooth gradients; deep cool shadows; a subtle rim
    light that lifts characters off the background.
  - *Outlines*: thick, dark, ink-like on characters and key props; thinner or lighter on the environment so gameplay
    elements stand out.
  - *Colour*: dark, desaturated environments (deep blues, purples, murky greens) with saturated accents on
    characters, enemies, hazards and spells. Readability first: player, enemies and attacks must pop from the ground.
  - *Textures*: simple hand-painted or flat colour, minimal surface detail; shape and silhouette do the work.
  - *Effects*: bold, graphic, hard-edged shapes, high-contrast emissive colour, comic-style impact frames and flashes;
    magic must read clearly from above.
  - *Mood*: grim but colourful, an illustrated dark storybook, not gritty realism.
- As built toward that (first pass, not yet judged): three hard light bands with purple-blue shade; heavy ink on
  characters and a thinner, lighter line on the world; stone in deep indigo with flat per-stone tones and no grain;
  the player in saturated blue and orange, enemies in bronze, crimson and violet with cyan lightning; hard-edged
  slash arcs and sparks with a white core; a spiked impact burst and a one-frame screen wash on heavy blows,
  finishers, perfect guards and wounds.
- Palette and lighting: a first version is built (see "The first area as built" under Vertical slice); not yet
  judged by the user.
- Fidelity targets: not decided; they depend on the camera.
- Performance budget: default 1080p at 16.6 ms and 0 B GC per frame on the user's RTX 3080, until changed.

## Sound

Set aside on purpose until the story is clearer (the user's decision, 2026-11-15).

- Mix character: not decided.
- Music: not decided.
- Voice: not decided.

## Scope

- **Decided**: one developer (the user, working with Claude).
- **Decided**: the goal for the two to three weeks from 2026-11-15 is a **vertical slice**; the full game comes
  after. The slice holds: movement, dodge with stamina, perfect guard, mana harvested from hits, one or two spells;
  fists and the Katana; Fire and Lightning, each with a short upgrade tree; one small area with two or three ordinary
  enemy types; one boss; and the take-or-consume choice after it. Which boss, and where the Katana is found, are open.
- **Decided**: Windows PC only for now, single player. Session and run length: not decided.
- "Clover" is a temporary name; there is no real title yet.
- What is in the first playable, what is in the first playtest build, what is out: not decided.

## Vertical slice

The first thing to be built. **Decided** unless marked.

- **Start**: fists and Fire.
- **Area**: a castle under constant lightning strikes, with two or three ordinary enemy types. A plain Katana is found
  partway through.
- **Boss**: the Lightning master, who fights with a Katana and a lightning iaijutsu draw. The player climbs to the top
  of the castle, which stands in the storm clouds, and walks out onto the clouds; the fight happens there.
- **Reward**: take Lightning (Fire points lost; the boss's special Katana and signature perk gained) or consume it
  (Fire points and a plain Katana gained).
- **Lightning strikes** are scenery. **Leaning**: the castle's enemies attract them, and an enemy struck by a bolt
  is buffed for a short time.
- **Flasks**: the player starts with three.
- **Player look**: a young adventurer, as a placeholder until the story is decided.
- **Ordinary enemies** (Decided), each infected with lightning: a temple soldier with a spear, who teaches guarding;
  a fast acolyte with knives, who teaches dodging; a caster who throws lightning from range, who teaches closing the
  distance.
- **Hub**: a small hub where the player returns after dying, hears the sponsor's voice and starts again.
- **Build order for the boss**: the duellist phase first; the godly form only if time allows.
- **Boss look**: the god of Lightning, a human-sized duellist with a Katana in the first phase. The stage is in
  effect the heavens, so the fight must feel that large; the user's reference is the fight against the dragon in the
  clouds in Sekiro. **Leaning**: a second phase in a huge godly form. One idea from Claude, not accepted: a vast
  storm serpent that coils through the clouds behind the duel and enters the fight in phase two.
- **Katana moves**: a light combo, a heavy attack, and a charged draw (sheathe, hold, release for the Iaido Slash).

### The first area as built (2026-11-15)

Built by Claude in one pass at the user's instruction to work through the area without stopping; every choice below is
**Claude's default, open to change**, and none of it has been played by the user yet. The scene is
`Assets/_Project/Scenes/Castle.unity`, built by `Clover > Build > Castle`.

**Layout** (north is up the screen and up the hill): a causeway over the drop with the first rest fire; the ruined
gate; the outer court (two temple soldiers and an acolyte); a stair up to the shrine terrace, where the Katana stands
in a stone inside a ring of standing stones (two acolytes and a storm caller, and the second rest fire); a second
stair to the upper ward under the keep's wall (two soldiers, an acolyte, two storm callers); and the sealed door to the
summit, which will not open while any enemy stands. Eleven enemies. (2026-11-16: twelve, with the Gate Captain
before the door; the ward's five now stand in its south half. See "The Gate Captain as built".)

**Fists** (the starting weapon): the quickest chain in the game, short reach, light damage, and a great deal of
stagger for that damage, so fists open an enemy for the finisher. Four lights (jab, cross, hook, front kick), a
lunging heavy, a rising heavy that ends a chain, and the skill **Breaker**: a charged straight that fills most of a
stagger meter in one blow. The guard with fists is crossed forearms.

**Fire's base kit** (mana only comes from fighting, as before):

- **Flame Wave** (Triangle, 25 mana): a short committed cast that throws a cone of fire 5.5 m ahead. Modest damage,
  good stagger, and it leaves what it touches **burning** (5 damage a second for 5 s; burning does not stack, it is
  topped up). A dodge can break the cast; the mana is spent either way.
- **Kindle** (L2, 40 mana): Fire's self buff. For 10 s the weapon is wrapped in flame: 30% more damage, and every blow
  sets its target burning for 3 s.

**The three enemies**, each with lightning in it (glowing marks and eyes). Every attack is drawn on the ground before
it lands: pale for one that can be guarded, red for one that cannot.

| Enemy | Health | What it does | What it teaches |
|---|---|---|---|
| Temple Soldier | 95 | A long narrow thrust; a wide sweep up close; a red charging lunge from range. Slow to turn. | Guarding, and the perfect guard: its blows are slow and plainly shown. |
| Knife Acolyte | 55 | Two quick cuts with no rest between them; a red leap from range. Fast. | Dodging: a guard held through both cuts costs too much stamina. |
| Storm Caller | 45 | Calls a bolt down on where the player stands (red); backs away; bursts in a ring when cornered. | Closing the distance. |

Enemies flinch when hit, except that an attack they are already showing is broken only by a heavy blow. A full stagger
meter drops them to one knee for three seconds, open to the finisher. A perfect guard against a blow struck by hand
throws stagger back on the attacker.

**The storm** (the user's leaning, built as a first version): bolts strike the towers as scenery. While a fight is on,
every eight to fifteen seconds the storm marks one of the enemies in it with a blue ring that follows it, and a moment
later a bolt lands on it. The enemy is **charged** for nine seconds (30% faster and harder-hitting, pulsing blue) and
the player is hurt if standing inside the ring. So a marked enemy is one to step away from, or to kill first.

**Resting**: lighting a rest fire refills health and flasks, brings every enemy in the area back, and becomes the
place the player wakes after falling. Death does not yet end the run in this area: the hub and the run structure are
not built, so falling returns the player to the last fire lit.

**Look as built**: night under the storm, cool grey stone, rain, fog that swallows the keep, warm pools of light from
braziers; cel-shaded with outlines, light falling in hard bands, a cool rim on characters; bloom on anything brighter
than white, which is how fire, lightning, slashes and the enemies' infection glow while the world stays muted. A wall
or tower standing between the camera and the player thins to a screen of dots.

### After the user's first play of the castle (2026-11-15)

The user's verdict: the base gameplay "is starting to look like what I have envisioned". Two problems, and what was
changed for each (Claude's first answers, not yet played):

- **Several small enemies ganging up was annoying.** Enemies now take turns: only one may be showing or striking a
  hand-to-hand attack at a time (and one a thrown attack), with at least 1.1 s between one enemy starting and the
  next. The others hold a ring about 5.5 m out and circle until it is their turn.
- **Fights leaned toward run in, spam, run out, where the user wants the smarter, even exchange of Sekiro and Elden
  Ring.** Three changes push toward standing and trading:
  - *Enemy guard.* The Temple Soldier turns light blows from the front on its spear while it stands ready: it takes a
    quarter of the damage but all of the stagger. Its guard is beaten by filling its stagger meter, by a heavy blow,
    by fire, or by getting behind it. After turning three blows it strikes back at once.
  - *Poise.* Light blows no longer make every enemy flinch every time: a soldier shrugs off two and flinches on the
    third, an acolyte flinches on the second. Heavy blows always do.
  - *The perfect guard is worth more*: it throws 55 stagger back on the attacker (was 40 to 45), so two perfect guards
    nearly open a soldier for the finisher.
- **Open**: whether this is enough. Further levers not yet pulled: stamina costs that punish spamming, enemies that
  chase a retreating player harder, and enemy combos that reward holding ground.

### Reading the fight: guard and attack indicators (2026-11-15)

The user found the guard visuals "fairly basic" and asked for indicators drawn from games of this kind. What was
taken, from Claude's knowledge of them, and how it is shown here (first pass, not yet played):

| Indicator | Taken from | Here |
|---|---|---|
| The guard as a shape on the ground | Hades (shielded enemies), top-down action games generally | While a guard is up, a bright curved band lies on the ground in front of its holder, as wide as what it covers and brightest dead ahead: blue for the player (200 degrees), gold for a guarding enemy (150 degrees). |
| The opening | Sekiro, Elden Ring | An enemy's band goes out the moment it attacks, flinches or staggers. No band means it can be hit. |
| Steel on steel | Sekiro's deflect | A blow turned on any guard throws a cold spiked burst and sparks at the point of contact, a small camera jolt, and the band jumps outward white. |
| The perfect guard | Sekiro, Lies of P | The band flares gold and jumps half again as wide, with a ring, a burst, a short stop and a one-frame wash of the screen. |
| The guard break | Lies of P, Elden Ring | The band bursts red and a red flare marks the spot. |
| The commit glint | Elden Ring, Lies of P | The instant an enemy commits to an attack, a white glint flashes over its head; for an attack that cannot be guarded, a larger red flare. This comes before the ground marker has filled. |
| Balance about to break | Sekiro's posture bar | An enemy at 70% or more of its stagger meter flickers amber: one or two more blows will open it. Staggered, it pulses gold as before. |

- **Decided (user)**: for the lightning level the heavy blue is right; it should feel stormy. Rain is now heavy,
  driven sideways by wind, with splash rings on the ground around the player, and the sky flashes more often.
- **Open**: sound for all of these (deferred with the rest of sound); a guard stance animation for the soldier, which
  still stands as it did.

### The overnight run of 2026-11-15 to 16: what was built and decided

Built by Claude alone, on the user's instruction to work through the night without input. Everything here is
**Claude's default, open to change**, and none of it has been played by the user.

**How animation is made now.** Animations are no longer keyed in Blender. Each one is written as code
(`Assets/_Project/Scripts/Editor/Forge/`): a list of poses in the character's own space, with feet and hands given as
places to be and the knees and elbows worked out from them, eased timing between poses, and the scarf, hair and
cloak simulated frame by frame so they trail and settle. `Clover > Forge > Bake Player Animations` and `Bake Enemy
Animations` write the clips to `Assets/_Project/Generated/Animation/` and draw a contact sheet of every clip to
`review/anim/`, which is how they are checked. The rule every action follows: a wind-up that goes the other way
first, a held beat, a blow that arrives fast, a follow-through past the target, a settle. Attacks read their timing
from the game's own numbers, so the blow in the animation always falls on the moment the game deals it.

**Models.** The player, the three castle enemies and the god were all rebuilt as shaped, smooth-shaded bodies with
weights blended at the joints, on one shared skeleton. The player keeps the ronin design the user approved, with far
more in it (bandaged chest under an open jacket, rope belt and flask, plated left arm, torn right sleeve, wide torn
trousers, scar and headband, heavy scarf, the hat on his back). The soldier has layered plate, a wide iron hat and a
banner on his back; the acolyte a hood, crossed wraps and a long scarf; the storm caller a robe, mantle and tall hat.

**Narukami, the god of Lightning** (first form; name and details are Claude's):

- *Who*: once a swordsman, now the storm. Storm-grey skin, white hair, gold horns, a living lightning scar across the
  chest, the great rope and paper streamers of a shrine at his waist, and behind his shoulders the ring of seven
  thunder drums. Human-sized (a fifth larger than the player), with a long sword.
- *Where*: the summit above the storm, reached through the castle's sealed door once every enemy below is dead. A
  round stone floor inlaid with rings, standing in a sea of cloud, with a ring of red gate-posts hung with drums
  round its far side. No rain: the storm is below.
- *How he fights*: as a duel. He guards and answers like the soldiers (light blows from the front are turned; after
  two he strikes back), and his poise is high. 620 health, and a finisher takes a fixed fifth of it, so breaking
  his balance is the real way to kill him: each perfect guard throws 42 of the 190 it takes.
  - Three cuts in a row: across, back, and down. All can be guarded.
  - A quick thrust down a lane at middle range.
  - **Red**: the draw. He sheathes, sinks, holds, and goes through the player along a ten-metre lane to stand behind.
  - He steps through lightning to the player's flank and cuts as he arrives.
  - Three bolts called down in a row on where the player stands.
  - A burst all round him when crowded.
  - **Second phase** at half health: it opens with a roar that cannot be guarded, he acts a sixth faster, the bolts
    around the summit fall three times as often, and he gains the draw twice in a row.
- *Not built*: his voice, and any sound.

**Narukami's godly form, "the Storm Itself"** (built later the same night and deepened the night after; Claude's design from the unattended overnight run, not yet played by the user):

- *What it is*: when the swordsman falls he does not stay fallen. After a breath (the player is given back a third of
  their health, no flasks), what he became climbs out of the cloud beyond the summit's north edge: an oni's head with
  gold horns and a mane of white spikes, shoulders like a hill, and two clawed hands held to him by ropes of
  lightning. He is too large to stand on the floor. The pieces are rigid models moved by code (`Colossus.cs`), not a
  skinned character. 760 health, and the bar across the bottom takes his new name. Every number below is a named
  field on `Colossus` or on its `ColossusChooser`.
- *The entrance* (4.2 s; nothing in it hurts the player and nothing the player does counts): bolts fall round the rim
  every 0.3 s and the floor shakes; at 1.0 s his left hand comes up over the rim and down on the floor, at 1.7 s the
  right (each draws the pale circle of a slam first, so the first thing the fight shows is what a mark means; dust
  and a shake, no damage); he hauls himself up on them, crown first; at 2.75 s the head stands up over the rim and at
  3.2 s he roars: a pale ring crosses the whole floor in 0.8 s (seen only), the camera shakes, one hard frame of
  white, the eyes flare, and the callout names him. Only at 4.2 s does the bar appear and the first attack come.
  The camera looks further north for it (and for his death) than in the fight.
- *How he fights*: with his hands, which hang in the air either side of the player when idle, and with his head.
  - **The slam**: a pale circle (3 m) follows the player for 0.85 s, stops, and 0.35 s later a hand comes down on it
    for 28. It can be guarded, and a perfect guard throws 60 of the 170 his balance takes. The hand then lies where it
    fell for two seconds.
  - **Red: the sweep**: a red lane 4.6 m wide is drawn right across the floor through where the player stands, and
    after 1.25 s a hand crosses it edge-on in half a second for 30. Roll through it or leave the lane. The hand lies
    spent at the far edge for a second and a half.
  - **Red: the bolts**: six circles (2.3 m), one every half second, each landing 0.95 s after it is drawn, for 16.
  - **Red: the breath** (the head): he rears up off the rim, his eyes flare to five times their light, and a red lane
    5 m wide is drawn from the north rim through the player and on across the floor. It follows the player for 0.9 s,
    holds for 0.4 s, and then for 0.8 s a beam of lightning stands in it, burning whoever is in it for 14 on contact
    and again every 0.4 s. Step out of the lane or roll through it; a perfect guard turns one burn and throws 30 on
    his balance. He hangs his head for 0.9 s afterwards (out of reach: a rest, not an opening).
  - **The bite** (the head; only against a player who has stood within 7 m of the middle of the north rim for
    1.2 s): the head comes in over the rim and stands up behind a pale circle (3.8 m) whose middle lies 3 m rimward
    of the player; the circle follows for 0.8 s, holds 0.3 s, and the head falls flat on its face in it for 40. It
    can be guarded (heavy: 36 stamina), and a perfect guard throws 60. Its crown lands at the player's feet, and it
    is stuck there for 1.6 s: the head can be struck at plain damage (100%, not the broken head's 160%), with no
    finisher.
  - **The clap**: both hands draw 9 m apart either side of the player, level and edge-on, and two pale lanes (4.4 m
    wide) grow from them toward each other along the east-west line through the player. They follow for 1.0 s, hold
    0.4 s while the hands draw back a little further, and close in 0.16 s for 36. It can be guarded whichever way the
    player faces, and a perfect guard counts as two (120 of his 170 balance). Leave the line north or south, or roll.
    Both hands then lie stunned where they met for 2.6 s: the longest opening on the hands.
- *How he chooses* (`ColossusChooser`, plain C#, with tests): the fight opens with a set pattern, slam, sweep,
  breath, clap. After it: a player who has stood under the head long enough is bitten (never twice running); a player
  south of the line 5 m below the floor's middle is breathed on six times in ten (never twice running, and never at
  a player near the rim); otherwise by shares (slam 32, sweep 20, clap 18, bolts 18, breath 12 in a hundred). Nothing
  comes three times running and only the slam twice (in his fury, where each slam is already two, not even that). Sweep, breath, bite and clap are heavy: after two of them the
  next rest is a lull of 2.4 s instead of 0.9 s.
- *Fury*, below half health: one flash, a shake and the callout mark it. He acts a quarter faster, slams twice (one
  hand after the other), and rests 55% as long between attacks (the lull is not cut). The eyes burn 2.6 times and the
  scar 1.35 times as bright, pulsing; bolts fall round the rim about every 0.9 s (scenery, off the floor); and each
  bolt of the bolts attack leaves a red crackling pool (1.9 m) for 2 s that burns for 8 on contact and every 0.5 s.
- *The ropes of lightning*: three thin strands from each shoulder to the wrist (the brightest 7.5 cm wide), kinked
  at hard corners, redrawn twenty times a second and never all there at once; a hand that is attacking has all three
  and 1.7 times the width.
- *How he is hurt* (unchanged): a hand lying on the floor can be cut (three quarters of a blow's damage reaches
  him). Blows on the hands and perfect guards wear down his balance; when it breaks, the head falls to the floor for
  five and a half seconds and takes blows at 160%, and a heavy blow on it is a finisher that takes at least 22% of
  all his health and sends him back up. Hands hanging in the air cannot be reached.
- *The death* (5 s, then the rewards as before): everything stops on the last blow (0.35 s) and then runs slow; the
  bar goes; the head stands up over the rim thrown back to the sky, cold light breaks out on one piece and then
  another, and from 0.9 s the sky strikes him every 0.22 s; at 1.5 s the left rope parts and that hand falls away
  into the cloud, at 2.0 s the right; at 2.6 s the head comes forward and at 3.0 s lands flat on its face across the
  rim; at 3.6 s it slides back off the edge. Then the director's flash and bolt, "A god falls", the storm rods stop
  and the sky's own flashes are set to one in 40 to 90 s.
- *Not built*: his voice and any sound; arms; cracks drawn on the pieces in his death (it is their whole light that
  flickers); a title card for his name (the scenes' card sits where his head is, so the callout and the bar name
  him); the breath's own tell cannot be seen from the far south, where the head is out of the picture and the lane
  is the whole warning; the afterimage and the upgrades were not tuned against him.

**Take or consume** (built as the two things he leaves on the floor, chosen by walking up and using one):

- *Take the storm*: Fire is lost. The player gains Lightning's kit and his sword, the Storm Katana (the Katana with
  15% more damage and stagger and a draw that carries further).
- *Consume the storm*: Fire is kept and strengthened (Flame Wave 20 to 28 damage, burning 5 to 7 s and reaching
  further; Kindle 30% to 45% damage and 10 to 13 s), with a plain Katana.
- Either restores the player, and then the way back down to the castle opens. Weapon and element carry between scenes.

**Lightning's base kit** (for a player who takes it): *Thunderclap* (25 mana) drops a bolt on the enemy faced, up to
ten metres off, light on damage and very heavy on stagger, and leaps to two more. *Overdrive* (40 mana, 10 s) makes
every swing 30% faster and adds stagger to each blow. Fire's kit is damage and burning; Lightning's is speed and
broken guards.

### Upgrades in the slice (Decided: all of these are in)

Fire, built around pressure and momentum:

- **Burn and Detonate**: weapon hits stack Burn; a perfect guard against a burning enemy sets all the stacks off at once.
- **Heat**: landing hits without being hit raises a Heat level that adds damage; taking a hit drops it. The self buff
  fills it instantly.
- **Flame Trail**: the dodge leaves a line of fire behind it.
- **Burning Crescent** (Katana): the Iaido Slash throws a slow travelling arc of flame.
- **Firebomb Flask**: a flask can be thrown instead of drunk, trading a heal for a large explosion.
- **Backdraft**: a perfect guard releases a cone of fire back at the attacker.
- **Meteor Leap**: jump out of the fight and land on a marked spot, leaving a burning crater.

Lightning, built around speed and precision:

- **Static**: hits build Static on an enemy; a critical hit discharges it and arcs to everyone nearby.
- **Blink**: the dodge becomes a short teleport that damages anything passed through.
- **Afterimage**: a dodge at the last moment leaves a copy that repeats the player's last attack.
- **Lightning Rod**: plant a rod in the ground; bolts strike around it at intervals, and the player can warp to it.
- **Delayed Cuts** (Katana): the Iaido Slash passes through the enemy, and the cuts land only when the blade clicks
  back into the sheath.
- **Stored Charge**: a perfect guard banks a charge that makes the next attack a guaranteed critical hit.
- **Overclock**: a higher tier of the self buff briefly slows everything except the player.

Signature perk of the Lightning boss (only by taking the element): **Storm Step**, where every dodge is a
free Blink while the self buff is active. Mixed Fire and Lightning branch (only by consuming): **Plasma**,
where a critical hit on a burning enemy causes a chained explosion.

### The upgrades as built (2026-11-16, overnight run; Claude's defaults, not yet played by the user)

**Embers and the tree.** Each enemy gives one ember the first time it is killed on a visit (a rest brings the enemies
back but not their embers); a god gives five. Embers are spent at any rest fire: resting opens the element's tree, the
world stops while it is open, the stick chooses, the use button learns and the dodge button leaves. Costs run from 1
to 4 and all seven of a tree cost 17, so a first run through the castle (11 enemies) and its god buys most of one
tree and not all of it. There is no order to the tree yet: anything affordable can be learned. Taking a new element
forgets the old tree and gives back the embers spent on it; consuming gives 4 embers toward the tree already held.
The HUD shows the embers held (top right) and, once Heat is learned, its five levels under the flasks.

**Fire** (all seven built):

| upgrade | cost | what it does, in numbers |
|---|---|---|
| Heat | 1 | every 3 blows landed raise a level, up to 5; each level is +6% weapon damage; a level cools off after 6 s without a blow; a blow taken puts it all out; Kindle fills it |
| Burn and Detonate | 2 | every weapon blow sets a 4 s burn and deepens it a stack, up to 5; each stack adds 30% to the burn's damage; a perfect guard sets it off for 10 damage plus 10 a stack |
| Backdraft | 2 | a perfect guard throws a 4.5 m, 100 degree cone of fire back: 16 damage, 18 stagger, 3 s burn (and it carries the detonation if that is learned) |
| Flame Trail | 2 | the dodge leaves four patches of fire along its path for 2.6 s; each bites twice a second for 4 and sets a 2 s burn |
| Burning Crescent | 3 | the Iaido Slash throws an arc of flame that travels 8 m/s for 1.1 s and hits each enemy once for 18 and a 3 s burn |
| Firebomb Flask | 3 | hold the flask button (0.3 s) to throw at the enemy faced, up to 9 m: 36 damage, 30 stagger, 4 s burn in a 2.6 m burst, then the ground burns for 3.5 s. A tap still drinks |
| Meteor Leap | 4 | hold the spell button (0.3 s), 50 mana: a 0.6 s leap, untouchable in the air, down beside the enemy faced up to 9 m off: 46 damage and 55 stagger in 3.2 m as a heavy blow, and a crater that burns for 4 s. A tap still casts |

**Lightning** (all seven built, some simpler than first written):

| upgrade | cost | what it does, in numbers |
|---|---|---|
| Static | 1 | light blows build Static on an enemy, up to 5; a heavy blow or a finisher discharges it for 7 damage and 5 stagger a stack to everyone within 4 m. (Written as "a critical hit"; there are no critical hits, so the heavy and the finisher stand in) |
| Stored Charge | 2 | a perfect guard banks a charge; the next blow that lands deals double damage and stagger |
| Blink | 2 | the dodge shocks whatever it passes through for 10 damage and 14 stagger. (It is still the roll to look at, not yet a teleport) |
| Afterimage | 3 | a perfect dodge leaves a strike where the player stood, 0.3 s later: 26 damage, 28 stagger in 2.3 m. (It does not yet show a copy of the player or repeat the last attack) |
| Delayed Cuts | 3 | the Iaido Slash lands again 0.55 s later for 60% of its damage. (It does not yet withhold the first cut) |
| Lightning Rod | 3 | hold the spell button, 40 mana: for 6 s bolts fall in a 3.5 m circle ahead of the player, biting every 0.9 s for 12. (No rod object and no warp yet) |
| Overclock | 4 | Overdrive opens with 2.5 s in which the enemies' own time runs at 35% |

**What a god leaves** (built later the same night): *Storm Step*, given with Lightning when it is taken: while
Overdrive lasts every dodge costs no stamina, shocks what it passes through, and leaves a shape of light where the
player stood. *Plasma*, given when the storm is consumed into Fire: a heavy blow or a finisher on a burning enemy
bursts for 18 damage and 16 stagger in 3.2 m and sets alight everyone it reaches. Neither is for sale and neither is
listed in the tree. A thrown flask is now seen in the air, and Lightning's dodge and the Afterimage leave a copy of
the player's body made of light.

### The hub and the run's loop as built (2026-11-16, overnight run; Claude's defaults, not yet played by the user)

**The loop.** The game now opens in the hub. The gate there leads to the castle; the castle's sealed door to the
summit; the summit's way out, once the god has fallen and the choice is made, back to the hub. **A fall anywhere in a
run ends it**: the fallen player is looked at for a moment, the screen goes dark with the words "The hours turn back",
and they wake in the hub with bare hands, Fire, no embers and nothing learned. Rest fires therefore no longer bring
the player back from death; they heal, refill the flasks, bring the area's enemies back and open the element's tree.
The practice arena keeps its old rule of standing up again.

**The hub** (working name: the still place). An island of slate laid out as a stopped clock, in black water, with
rain hanging in the air where it was when the hours were turned back. From where the player wakes everything is in
view: the sponsor's light over the clock's centre, the weapon stands to the west (bare hands, the Katana), the element
braziers to the east (Fire, Lightning), and the gate to the north under a great broken ring. (Later on 2026-11-16 three more
weapon stands were added along the western rim, all open for now: see "The newer weapons as built".)

- *What a run can begin with.* Bare hands and Fire always. The Katana once it has been drawn from the stone in any
  run; Lightning once it has been taken from its god in any run. These unlocks are kept between sittings. A stand or
  brazier not yet won is empty or cold and says how to win it. What is chosen is carried through the gate, and a ring
  of light on the floor marks the one carried.
- *The sponsor* is called only "The Voice" for now: a spindle of pale gold light with splinters turning round it. It
  speaks unasked the first time the player ever wakes; afterwards it waits to be spoken to, and has different words
  after a fall (five short sets, each with one true piece of advice), after a god has fallen, and after coming back
  alive. All its words are in one file, `SponsorLines.cs`, and are click-through: each line stays until the use
  button is pressed.

**Not built**: a title screen. (A pause menu, with a way to forget the saved unlocks, was added later the same night.)

### The art pass of the overnight run (2026-11-16; Claude's defaults, not yet seen by the user)

Made against the Ravenswatch brief, and then against a second pair of eyes that was shown the screenshots and asked
only for faults.

- **Painted ground.** Stone, wall, wood, earth and roof textures are drawn by code at 1024 px as a hand-painted floor
  is: rows of stones of uneven width, each its own flat tone leaning a little blue, violet, green or warm, a lit edge
  at the top and left and a shaded edge at the bottom and right, worn corners, a crack across one stone in four, moss
  in the joints. `Clover > Build > Repaint Textures` repaints them.
- **Dressing** (`Dressing.cs`, for every builder): puddles drawn flat (a dark sheet, a line of wet stone round it,
  broken strokes of sky in it) that take lamp light; tufts of grass along walls and round pillars; fallen leaves in
  dull red and gold; iron lantern posts; barrels, sacks, a broken cart; spears left standing with a torn pennant. A
  rectangle of earth laid on paving gets a ragged edge of spills and grass. The castle has a paler path of smaller
  stones up its middle, with lantern posts along it.
- **Warm against cold.** A lamp's pool on the ground is now sized by the lamp's strength, falls in two hard steps,
  and keeps the lamp's own colour, so every court has pools of warm light on the cold stone.
- **Fire** is drawn as tongues of flame (round below, pointed above, the edge licking) in two flat tones with a hard
  line between, laid over one another instead of added, so a big fire stays a drawn shape and does not burn out white.
- **Ink.** Character outlines are a third thicker. Training dummies carry the same ink as characters.
- **HUD.** Slanted bars in inked frames; a pale piece that lingers where health was just lost, then drains; gourds for
  flasks; a diamond for embers; framed panels; a stroke of dry brush behind prompts and under the chosen row of a
  menu; the mana bar takes the element's colour; the world dims behind the upgrade tree. The controls hint is shown
  only in the hub and the practice arena.
- **The font** is Spectral SemiBold (SIL Open Font License), brought in during the run without the user's sight of it;
  its row in `ASSETS.csv` says so. Cormorant Garamond was tried first and dropped because its old-style numerals read
  as letters at HUD size.
- **Title cards.** The castle's name and the god's are shown on a brush stroke for three seconds on arrival.
- **Movement seen from above.** Both runs were re-posed for the top-down camera, where legs hardly show: more lean,
  the shoulders turning against the hips, the sword arm swinging.
- **Animation, criticised and redone.** Every contact sheet was shown to a second pair of eyes asked only for faults.
  From that: every attack now rings when it lands (the body drives on past its pose and comes back) so no blow is a
  frozen pose; idles and walks move enough to be seen from above; the heavy cut goes from over the right shoulder
  down to the left and the third light cut rises from the left hip to the right shoulder, so both cross the body in
  the top view; the fist attacks turn the hips and shoulders into the blow, the rising blow leaves the ground and the
  fourth is now a round kick; the soldier's thrust and lunge are no longer alike; the caster throws its bolt with the
  free hand and slams its staff for the burst; the acolyte really leaps; deaths are staged falls that end lying long
  on the ground; the guards are braced; the god's roar has a motion of its own.
- **The castle's three courts are now three places.** The causeway is storm-broken (gaps in the parapet, a standing
  and a toppled guardian, piers and crags in the drop). The outer court is a garrison yard (a well with a lamp, weapon
  racks, striking posts, a torn lean-to). The terrace is a shrine (stone lanterns, sacred ropes between the standing
  stones, offering tables, two old trees). The upper ward is an execution ground (lightning rods arcing along the
  keep wall, stakes on scorch marks, a broken ballista). Walls have footings, buttresses, arrow slits and ivy, and
  the line where floor meets wall is broken with worn stones and weeds. Far ridges and towers were built beyond the
  walls but the camera, which looks down, never shows them from inside the courts.
- **The summit and the hub were dressed too.** The summit has a storm crest inlaid in its floor, scorch scars, a
  stepped plinth and broken balustrade at its rim, and the keep's roofs showing through nearer cloud. The hub's clock
  has minute ticks, numerals and two small dials, shallows at its rim, and things stopped in time round it (gears, a
  bell, a pendulum, a crane, lanterns).
- **Feedback in the fight.** A dead enemy burns away: the lightning leaves it, it goes to an ink shape and breaks up,
  and its ember flies to the player. A staggered enemy carries a gold broken ring round a white cross: finish it. Thin
  brackets on the ground show which enemy the next attack will turn to. Enemy bars have ink frames and a pale piece
  that lingers where health was lost. Under 30% health a dark red edge breathes round the screen. A blow that lands
  leaves a white cut line across the enemy, longer for a heavy blow and crossed for a finisher.
- **A second critic, and the polish that followed.** Attack marks on the ground are now a bold edge with a faint
  inside, and the growing part is a brighter copy of the shape, so they no longer paint over the fighters. The impact
  frame is spikes from the screen's edges with the middle left clear, shorter and fainter. Lightning bolts are thin
  and kinked and no longer burn out white. Trim stone has its own dressed-stone texture and sits below the characters
  in value. While the storm itself is in the fight the camera pulls back by half, looks further north and tips up, so
  his hands and face are in frame from the northern part of the floor. Title cards sit above the player. Enemy bars
  and the captain's name are drawn over everything and step clear of one another. Shadows use a 4096 map with four
  cascades and soft edges.
- **The finisher, redone.** The Katana's finisher is a sheathed draw-cut: a deep coil with the hand on the hilt, the
  draw through the enemy, a long held pose with the blade out to the side while time is slowed and the camera leans
  in, then a slow flick and the return to the sheath. The fist finisher opens wide straight after its blow. The
  god's first-form cuts carry the sword clear of his drums.
- **A pause menu** on Start or Escape in every scene: resume, the controls and how a fight is won, give up the run
  (castle and summit), forget everything saved (hub), quit. Each of the last three asks once more before it acts.
  The hub shows one quiet line saying so in place of the old controls text.

### The Gate Captain as built (2026-11-16, overnight run; Claude's design, not yet played by the user)

An elite: the castle's twelfth enemy and its last fight, alone before the sealed door. He is there to push the castle
further toward the duel the user asked for, and to teach what the god will ask: guard, perfect guard, punish the
opening, step out of the red. Every number is a first value. Probed by `AgentScripts/CaptainProbe.cs`.

**Who he is.** The soldier's body at 1.45 (about 1.3 times a soldier), in black iron with a bone-white banner at his
back, dull gold cloth, and the lightning in him burning brighter than in any of his men. He does not level his spear
at whoever comes: he carries it slantwise across his chest and waits. While he is in the fight his name stands over
his own bars; the bar across the bottom of the screen stays the god's.

**His ground.** A course of pale stone crosses the upper ward at z 113, between the second pair of pillars. North of
it is his; south of it belongs to the ward's five, who were moved south to make room. An enemy that keeps a ground
notices only someone standing on it, never walks off it, gives a target that leaves three seconds and then goes back
to its post. A blow struck at it from off its ground brings it out for eight seconds, so the line is not a place to
hit from in safety. The storm does not pick him to charge. In the probe, walking his ground woke none of the five, and
five in pursuit stopped short of the line and had all let the player go within seven seconds.

| | |
|---|---|
| Health | 260. A finisher takes at least 34% (88), so three finishers fell him; two and some honest blows do too |
| Balance | 150. A perfect guard throws back 50 (three break him); it is open for 3 s |
| Guard | Turns light blows from the front for 15% of their damage and half their stagger, and heavy blows for 40% of their damage and all of their stagger. After four blows turned he answers at once with the jab |
| Poise | Shrugs off four light blows and flinches on the fifth; a heavy blow that gets past his guard flinches him as he recovers but never breaks an attack he is showing |
| Awareness | Notices at 8.5 m on his own ground; rests 0.7 to 1.4 s between attacks, a third of that when the player hangs back beyond 4.5 m |
| Second phase | At half health: struck by the storm, he opens with the leap, acts 15% faster, and gains the two chains |
| Reward | Three embers, once a run. The sealed door counts him: twelve must fall |

| Attack | Shown for | Shape | Damage | Notes |
|---|---|---|---|---|
| Two thrusts | 0.6 s, then 0.55 s | lanes 4.4 m and 4.8 m long, 1.1 and 1.2 m wide | 16, then 22 | Both can be guarded. The second lands 0.67 s after the first, which is longer than a roll, and turns to follow for longer. Open for 0.9 s after |
| Wide cut | 0.8 s | 200 degree wedge, 3.8 m | 26 | Punishes circling. Open for 0.8 s |
| Short stab | 0.34 s | 70 degree wedge, 2.4 m | 12 | Only when the player is within 2.6 m; also his answer after four blows turned |
| Leap | 1.0 s | circle of 2.6 m where the player stood | 30 | He lands at its edge. Walk out, or guard it at a price. From 3.6 to 8 m. Open for 1.0 s |
| Charge (red) | 1.1 s | lane 9 m long, 1.5 m wide | 34 | He raises the spear upright over his head first, then couches it and goes the length of the lane and out the far end. From 5 to 11 m. Open for 1.1 s |
| Cut into thrust (second phase) | 0.7 s, then 0.6 s | the wide cut, then the long lane | 26, then 22 | No rest between |
| Charge twice (second phase, red) | 0.95 s, then 0.85 s | the charge, and back again | 30 and 30 | The second from wherever the first left him |

**How he is beaten.** Not by chipping: blows on his guard do little, and he does not flinch out of what he has begun.
Guard his blows (all but the charge), perfectly when it can be done, hit him in the opening after each attack, and
when his balance breaks finish him. Twice or three times.

**Made for him, usable by any enemy:** a leaping attack (a circle the attacker jumps to), a guard that meets heavy
blows, a named counter, impatience with a player who hangs back, a ground an enemy keeps, embers by kind, and a mark
on the ground that is lifted over a step between attacker and target so the step cannot hide it.

**Not built:** sound; a model of his own (he is the soldier, recoloured and larger); any words or scene when the
fight begins; a guard stance clip (he stands in his ready pose while guarding); a wall or gate on his line (the
pillars on it still block sight and movement, and a marked lane passes through them as every enemy's does); return
of his health if he is left. Not tuned by play: every number above.

### The great chart: the framework under the element trees (2026-11-16, day run; Claude's defaults, not yet played by the user)

Built from the user's request for "a massive upgrade tree, with 30-40 possible options for each element including the
mixed elements as well as the super special boss one" on "a branching tree outwards in many directions ... when you
choose an element it will stay zoomed to that element's upgrade tree". This is the machinery, not the content: the
plan is in `docs/design/TREE_BRIEF.md`, and how a tree is written as data is in `docs/UPGRADE_SCHEMA.md`. Everything
below is Claude's default unless it quotes the user.

**The ring.** Eleven elements stand round one middle in a fixed order (Fire, Lightning, Wind, Ice, Water, Life, Earth,
Metal, Light, Dark, Poison), each with one colour, one affliction it lays on enemies, and one mixed branch shared with
the next element round (Plasma, Tempest, Blizzard, Glacier, Bloom, Overgrowth, Forge, Halo, Eclipse, Blight, Pyre).
One file, `Elements.cs`, knows all of it.

**All eleven elements can be held.** Fire and Lightning are as they were. The other nine had stand-in kits made from
what already existed (the table below is as it was that morning; **the kits now carry the designs' names and numbers:
see "The great chart filled" further down**):

| Element | Spell (tap) | Surge (from the user's first table of self buffs) |
|---|---|---|
| Wind | Gale: a long narrow wave, Unsteady two deep | Tailwind: the dodge goes 30% further, is safe 40% longer and costs 40% less; 15% quicker |
| Ice | Frost Fan: a short wide wave, Chill two deep | Rime Ward: one ward that turns aside the next blow that would land |
| Water | Tide: a broad wave that soaks | Wellspring: flasks heal half again as much |
| Life | Thorn Lash: a bolt that seeds one enemy and the one beside it | Renewal: 3 health a second |
| Earth | Stone Shove: a short heavy wave, Tremor | Bedrock: 20% less damage taken, more stagger on every blow |
| Metal | Shrapnel: a narrow hard wave, Sundered | Tempered: 35% less damage taken |
| Light | Lance of Day: a bolt on one enemy far off, Marked | Purify: grey health comes back at once, everyone within 7 m is Marked, critical blows hit harder |
| Dark | Hex: a weak bolt that leaps through four, Cursed | Hunger: health drains 2 a second (as grey, never the last of it) and 12% of each blow comes back. The user's "one revive as a zombie" is not built; this stands in for it |
| Poison | Spit: a wave, Poison two deep | Antivenom: 1 health a second and 6% of each blow back |

During any of the nine surges every blow of the weapon lays one stack of the element's affliction.

**Afflictions.** Burn and Static are no longer special cases; every enemy (and the training dummies, and the colossus
in a lesser way) carries a set of afflictions with the same rules for all eleven: stacks, seconds, and what each does.
The numbers are one table in `Afflictions.cs`, repeated in the schema document. In short: Burn burns; Static waits to
be set off; Unsteady staggers sooner and is shoved further; Chill slows and at five stacks **freezes** (held still for
3 s, a heavy blow shatters it for 45; elites and gods thaw in 1.2 s); Soaked takes more from spells and passes Static
and Chill to everyone within 4 m; Seeded heals the player on each blow and bursts into thorns when it dies; Tremor
slows and stops balance from recovering; Sundered takes more from weapons; Marked makes every blow on it critical and
gives more mana; Cursed deals less and heals the player when it dies; Poison lands in a burst every 2 s. Each
affliction an enemy carries shows as a small diamond in its colour under the enemy's bars, larger the deeper it lies;
a frozen enemy glows a steady pale blue. The colossus is too great to be frozen, slowed or moved, and what burns or
poisons it cannot take its last point of health.

**Critical blows** now exist: a weapon blow is critical by chance (none to begin with; upgrades add it) or always on a
Marked enemy, deals half again as much, and shows a gold star where it lands. Stored Charge keeps its own numbers (the
next blow doubled) and shows the same star.

**The player's numbers.** Twenty-nine numbers (weapon damage, stagger, mana gains, stamina costs, dodge distance and
safe time, the perfect guard's window, health, flask healing and count, critical chance and damage, spell damage,
cost and reach, surge length and cost, damage taken, grey health kept, embers earned, walking and swing speed, and the
depth, length and strength of afflictions laid) are summed from learned upgrades, the surge and timed boons, and the
weapon, guard, dodge, flasks and spells read them. Wards are new: each turns aside one blow that would have landed;
three are held at most; the HUD's status line says how many.

**Upgrades are data.** A mark has a kind (root, minor, major, spell, keystone, mix, signature), a cost set by its kind
(**minor 1, root 2, major 3, spell 4, keystone 6, mix 3 to 5**; the existing marks were re-priced to this: Fire's seven
now cost 21 and Lightning's 24, still more than the 19 a run pays), any one of several marks it may grow from, a place
in its tree, a picture, changes to the player's numbers, and effects: a trigger (23 of them, from "a light blow lands"
to "mana full") and an action (19, from "lay stacks" to "a helper that strikes a few times") with numbers. One runner
does them all. The first sixteen marks of Fire and Lightning behave exactly as before and are still done by hand
under their ids. Thirteen marks made only of data were added to prove it: Kindling, Stoked, Dry Tinder, Searing Guard,
Wildfire and the spell Cinder Fan for Fire; Keen Edge, Quick Hands, Capacitor, Grounding, Slipstream and the spell
Chain Lance for Lightning; and Ionised Air, Fused Core and the spell Plasma Lance on the Plasma branch. Their numbers
are in the tree files and are first guesses.

**Spells are worn.** A learned spell is worn in the hold-spell slot: hold the spell button for the worn spell, tap it
for the element's own. The first spell learned is worn at once; on the chart, pressing learn on a spell already owned
wears that one. The HUD names the worn spell beside the weapon. Meteor Leap and Lightning Rod are spells of this kind
now. A spell is thrown by a "form" (wave, bolt, leap, rod so far) that knows nothing of any one element, so any
element can wear any form in its own colour with its own affliction; the library of forms is the next piece of work.

**Mixed branches.** A mix mark is open to a player who holds one of its two elements and has **held or consumed** the
other in this run (the run now remembers both). Sealed, the branch is still drawn, and the words panel says which
element opens it. Plasma, the gift of consuming the storm into Fire, sits at the crown of the Plasma branch.

**The chart.** The rest-fire menu is now a night sky of stars with the whole ring on it. It is turned so the tree of
the element held stands upright, and the view stays close on that tree and leans toward the chosen mark; **the heavy
attack button (R2 or K) pulls the view back to show the whole ring, and returns**. Marks are framed pictures, not
letters: forty small white pictograms painted by code, tinted by the mark's state; the frame says the kind (minor
small and round, major square, spell a diamond, keystone large and eight-sided, mix six-sided, a god's gift a crowned
sun). Other elements' trees are faint; the branches shared with the two neighbours can be walked to and read. The
words panel gives the name, the kind and cost, how the mark stands with the player (learned, worn, what it grows from,
what opens it), and the description. The chart was tried filled to 381 marks (every tree and branch at the size
planned) and drawn that way it allocates nothing per frame.

**The hub's braziers.** There are eleven now: Fire and Lightning where they stood, and the other nine in an arc along
the eastern rim in the order of the ring, each burning in its own colour. **Stopgap**: Lightning still stays cold
until it is taken from its god, but the nine new elements have no gods to be won from yet, so their braziers are lit
from the first waking. When their gods exist each should be sealed the way Lightning is.

**Not built, or left rough**: the trees themselves beyond the thirteen proof marks (other workers are writing them
against the schema); the library of spell forms beyond the first four; Dark's revive; any way for enemies to lay
afflictions on the player; a sound for anything new; balance of every new number. Pull and push move ordinary enemies
and hardly move elites and gods. The chart was looked at only at the editor's 1920 by 936 game view.

### The great chart filled: the trees, the gifts and the engine under them (2026-11-16, day run, afternoon; Claude's defaults, not yet played by the user)

Built while the user was away, from their words: "a massive upgrade tree, with 30-40 possible options for each element
including the mixed elements as well as the super special boss one", "super cool, flashy, and interesting moves", "a
branching tree outwards in many directions", and of the economy "the goal is to make it as if you have to choose a
path in the tree". Nothing below was decided by the user; all of it is Claude's default, to be kept or changed.

**The trees are in.** Twenty-two files under `Runtime/Player/Trees/`: eleven elements of 30 to 36 marks each and
eleven mixed branches of seven to ten, 428 marks in all, written from `docs/design/trees_a.md` and `trees_b.md`. Each
element has a root, three boughs ending in three keystones, four or five spells and a god's gift; each branch has a
crown. Wind's "Slipstream" is called **Rushing Wake** (Lightning already had a Slipstream; no two marks share a name).

**The economy holds, by test.** No element's whole tree can be bought with a run's nineteen embers (every tree costs
more than three runs), the cheapest road from a root to one keystone costs at most twenty-two, and two keystones are
beyond one run. The designed costs were kept as they are.

**The kits carry the designs' names and, as far as the forms allow, their behaviour.**

| Element | Base spell (tap) | Surge |
|---|---|---|
| Wind | **Gust**: a short cone, 8 damage, Unsteady two deep, everything within reach thrown 3 m | **Tailwind**, 35 mana: the dodge costs half, goes 30% further, is safe longer; 15% quicker |
| Ice | **Hoarfrost**: a fan 4.5 m ahead, 10 damage, Chill two deep | **Glaze**, 12 s: one ward that takes the next blow |
| Water | **Riptide**, 20 mana: a wave 5 m ahead that soaks and drags what stands within reach 2 m closer | **Wellspring**, 30 mana, 12 s: flasks heal half again as much |
| Life | **Briar Patch**, 20 mana: a bed of thorns 3 m ahead for 6 s, biting twice a second and seeding (the `field` form; it counts as something grown) | **Regrowth**, 6 s: grey health made real at once, then 4 health a second |
| Earth | **Stone Rise**: a pillar under the enemy faced, 14 damage, 30 stagger, Tremor (the `pillars` form: it lifts) | **Bedrock**: 20% less damage taken |
| Metal | **Flung Blade**: a blade left in the enemy faced for 6 s; the next weapon blow drives it in for 12 and a notch (the `brand` form) | **Temper**: 30% less damage taken |
| Light | **Glare**: a flash 4 m ahead, 8 damage, 22 stagger, Marked | **Clarity**, 8 s: grey health made whole, perfect guard and dodge windows half again as long |
| Dark | **Grasp**: a hand to the enemy faced within 6 m that drags it to the feet, Cursed | **Hunger**, no mana: 3 health a second drained, the weapon deals 30% more and 30% of it comes back |
| Poison | **Miasma**: a cloud 4 m ahead for 4 s, one dose a second (the `field` form) | **Fever**: 1 health a second and 6% of each blow back |

Short of the designs: Gust and Riptide move everything within reach, not only what the cone touched; Stone Rise's
pillar does not stand to be broken; Flung Blade deals nothing as it lands; Grasp does not drag the player to what
cannot be dragged; Glaze, Wellspring, Bedrock, Temper, Clarity and Fever have their plain halves only (no "the swing
goes on", no instant flask, no shed afflictions, no guard that throws stagger back, no doubled beat).

**Afflictions follow the designs' numbers now**: Unsteady five deep at 6% stagger, 15% shove and 8% turning a stack;
Chill 7% a stack and Frozen 2.5 s; Static 6 s; Soaked on or off at 25% more from spells; Seeded 8 s with its burst in
3 m; Sundered 8% a notch; Marked one deep (three with Weighed) for 8 s, carrying three critical blows and then spent;
Cursed one depth at 15%; Poison eight deep at 2 a dose for 6 s, set off for 6 a dose. Not moved: the shatter is still
a flat 45, Seeded still heals by depth, and a guard is not opened by three notches.

**Gifts are by rule.** Taking an element from its god gives the Signature at the crown of that element's tree;
consuming a god gives the crown of the branch its element shares with the one held, when the two are neighbours on
the ring, and the embers as before. A gift that is a spell is worn at once. The summit's choice is unchanged in play
(Storm Step; Plasma for a Fire holder), but nothing names those ids any more. A mix's first mark lays only the
affliction of the element not held on top of what the held element's root already lays.

**Two debug shrines in the practice yard** (the Combat Test scene only), beside the element stone: **GOD GIFT** gives
the Signature of the element held; **CONSUME NEXT** marks the next element round the ring as consumed, which opens
the branch between them and gives its crown. With the element stone that is every gift in the game, tried by hand.

**The engine grew to say what the marks say.** Stats that hold only in a state (guarding, the surge, low or high
health, unhurt, planted, owing health, full mana, near something grown), on a kind of blow (heavy, light, critical,
the last of a chain, on an enemy in the air) or against a named affliction; stamina split into attacks, guard and
dodge; base-spell-only cost, reach, damage, arc and count; a deeper greatest depth; burst reach and damage; leech;
kills that know what the dead carried; triggers for a set-off with its depth, a shatter, a critical blow, the end of
a chain, a ward spent, a lift and a landing; **Hold**; **counts** a mark banks and spends, named on the HUD beside the
weapon; **health as a price** (a spell's own, and mana paid in blood) and grey health read; **the air** (an enemy
thrown up hangs helpless and lands; blows hold it up, three times; gods hang a little; the colossus reels); the flask
filled by parts; **grown things** counted, limited, stood near and burst; and the numbers of the hand-written Fire and
Lightning moves and of Heat (White Heat is whole: Heat to eight, quicker above five, and struck up there you burn).
`docs/UPGRADE_SCHEMA.md` section 15 is the exact list.

**Layout.** Minors were moved beside what they grow from by script; Fire and Lightning, which carry extra marks from
the first slice, were settled whole. Plasma stands in two columns, and its kept mark Ionised Air grows from Twin
Break now.

**Not built, or left rough** (the full list of marks that do less than their words is in the day run's report): the
bespoke spells, keystones and signatures; Dark's Rise; poison's beat; anything that changes an enemy's own attacks,
guard or sight; things that stand and can be struck; the lift has no shadow or landing ring; counts show as words,
not pips; no sound for anything new; none of the new numbers has been played for balance. Light's Unblemished and
Metal's Master's Answer are the loudest untried numbers.

## Open questions

1. Camera: top-down is on trial in the slice; confirm or change once it is playable.
2. The sponsor's true aim, name and form; the player's past.
3. What the corruption of the gods is and where it came from.
4. Full Body Armament is no longer a boss's weapon, so nine bosses share eight weapons: what does the ninth carry?
5. How the upgrades are arranged into each tree: order, cost, what requires what.
6. How long a full run should take, and how large each area is.
7. Element depth: the upgrade trees, the mixes, the status effects each element inflicts.
8. The game's real name. Sound and music, once the story is clearer.

## Decisions log

| date | decision | why |
|---|---|---|
| 2026-11-14 | Workflow set up before the game is designed; the Unity CLI is the only editor bridge | one bridge is safer and covers everything the project needs |
| 2026-11-15 | The game is a difficult, melee-first action game built on elements plus weapon techniques | the user's core idea; references Elden Ring and Lies of P |
| 2026-11-15 | Magic supports close combat and must not become the main focus of fights | the user wants the player empowered to use their weapons |
| 2026-11-15 | Each major boss masters one element and one unique technique; beating it gives a choice of one of the two | makes every boss a build decision |
| 2026-11-15 | Boss zones are themed to the element for atmosphere only, with no status effect | atmosphere without extra rules |
| 2026-11-16 | Animations are written as code and baked in Unity, not keyed in Blender | it lets every action be placed, timed and checked frame by frame, and the user asked for special emphasis on animation |
| 2026-11-16 | Narukami's godly form is a colossus beside the summit who fights with his hands; they can be cut only where they lie after a blow, and breaking his balance brings his head down | Claude's design during the unattended overnight run; the decided design called for a godly second form |
| 2026-11-16 | Game text is set in Spectral SemiBold (OFL); surfaces are painted by code in a hand-painted manner; fire is drawn as layered tongues | Claude's defaults during the unattended overnight run, toward the Ravenswatch brief |
| 2026-11-16 | The hub is a stopped clock outside time; a fall ends the run and wakes the player there; the Katana and Lightning become starting choices once won in a run | Claude's defaults during the unattended overnight run, following the decided run-based structure |
| 2026-11-16 | The element trees become one great chart of eleven elements round a ring with a mixed branch between neighbours; upgrades are data (kinds, costs by kind, triggers and actions); afflictions, critical blows, wards and worn spells exist; the nine new elements have stand-in kits and lit braziers | Claude's defaults during the day run, from the user's request for a massive branching tree with mixed elements; see "The great chart" |
| 2026-11-16 | The twenty-two tree files land (428 marks); kits renamed and rebuilt to the designs; afflictions moved to the designs' numbers; gifts given by rule; two debug shrines in the practice yard; the engine gains conditional stats, counts, Hold, the air, health as a price, grown things and tunable hand-written moves | Claude's defaults during the day run of 2026-11-16, from the user's request; see "The great chart filled" |
| 2026-11-16 | Upgrades are bought with embers (one per enemy, five per god) at rest fires, in any order; held buttons give the flask and the spell their upgraded moves | Claude's defaults during the unattended overnight run |
| 2026-11-16 | The god of Lightning is Narukami, a duellist with a ring of thunder drums, fought above the clouds; take or consume is two things he leaves on the floor | Claude's defaults during the unattended overnight run |
| 2026-11-15 | Blocking is directional; the finisher is its own move on a staggered enemy | the user's feedback after playing the combat arena |
| 2026-11-15 | The first area, its three enemies, fists and Fire's base kit were built in one pass on Claude's defaults | the user asked for long stretches of work with few check-ins and a higher bar for visuals |
| 2026-11-15 | Only the player and the final boss use more than one element; the final boss has Light, Dark and the Super Form | makes the final boss the player's mirror |
| 2026-11-15 | Mana for magic is harvested by landing weapon hits | forces melee, which serves the weapons-first pillar |
| 2026-11-15 | Areas with ordinary enemies lead to each boss | exploration, and enemies teach combinations by using them on the player |
| 2026-11-15 | Run-based; every run has all the bosses; unlocked starting weapons and elements carry over | the user's preference; unlocks make a failed run still count |
| 2026-11-15 | One weapon per fight, no weapon inventory; a swapped weapon stays on the ground | keeps the weapon a commitment |
| 2026-11-15 | Boss reward is take (new element and tree, old points lost, special weapon, signature perk) or consume (points for the current tree, plain weapon) | a real trade between a new build and the one already growing |
| 2026-11-15 | Perfect guard is available to every build | bosses can then be designed around it |
| 2026-11-15 | The first goal is a vertical slice: Katana, Fire, Lightning, one area, one boss, top-down camera on trial | the full design does not fit one person in two to three weeks; the slice tests the combat feel |
| 2026-11-15 | Slice layout: start with fists and Fire, a lightning-struck castle, the Lightning Katana master fought on the clouds above it | shows every decided system once |
| 2026-11-15 | All fourteen offered Fire and Lightning upgrades, Storm Step and Plasma are in the slice | the user liked them all |
| 2026-11-15 | The bosses are elemental gods who were once mortals; the player is a mortal with no past, sponsored by a time-turning being who proves untrustworthy | fits the heavens stage, explains the runs and the rewards, and carries the twist |
| 2026-11-15 | Tone and in-game voice are grim and sparse, like Sekiro | the user's preference |
| 2026-11-15 | Pitch approved | the user accepted Claude's draft |
| 2026-11-15 | Every god has a godly second form, more extravagant the later the boss | the user's choice |
| 2026-11-15 | Look: cel-shaded with outlines; muted world, vivid elements | joins the grim tone to the bright anime look; to be proven on screenshots |
| 2026-11-15 | A hub between runs, where the sponsor speaks and the starting weapon and element are picked | the user's choice |
| 2026-11-15 | The game opens on a battlefield tutorial that ends in a loss to the final boss; the sponsor saves the player at the price of their past | sets the goal, explains the empty past and the fists, plants doubt about the sponsor |
| 2026-11-15 | The slice has a small hub and builds the Lightning god's duellist phase before its godly form | keeps the slice inside the time available |
| 2026-11-15 | Windows PC only for now; sound and the game's title are put off | they wait on the story |
| 2026-11-15 | The four pillars are approved | the user accepted them |
| 2026-11-15 | Castle enemies: spear soldier, knife acolyte, lightning caster, each infected with the element | each teaches one core skill; the infection ties them to their god |
| 2026-11-15 | The player character is modelled, rigged and animated in Blender for this project | the user wants visuals tailored to the game |
| 2026-11-15 | Designed for a controller | matches the reference games |
| 2026-11-15 | Limited flasks refilled at rest points; death ends the run except through Dark's revive and some later upgrades | the user's choice |
| 2026-11-15 | Fists are the first run's weapon; Full Body Armament is an element-tree upgrade for the unarmed | gives fighting unarmed its own path |
| 2026-11-16 | An elite, the Gate Captain, guards the sealed door alone on his own ground: guard, poise, a red charge, a second phase at half, three embers (Claude's design in the unattended overnight run; not yet played) | pushes the castle's last fight toward the even duel the user asked for and teaches what the god asks |

## Spell forms (Claude's defaults from the day run of 2026-11-16)

Nothing here was decided by the user; it is what was built while they were away, to be kept or changed.

- **There are twenty-eight forms a spell can take.** The twenty-four new ones are blades from above, a great sword, a
  held beam, blades at the shoulders, a dragon's head, a nova, a slow orb, a great bomb, spikes along the ground, a
  bristling field, a drill, small helpers, a ring of bullets, a burst that runs backwards, a parry, a wall, orbiting
  discs, a brand, a rain, a copy of the player, a flash of cuts, a closed ring, pillars, and a step through an enemy.
  What each reads and does exactly is in `docs/UPGRADE_SCHEMA.md`, section 10.
- **How a spell is seen.** Every form has the same four beats: it shows itself coming (a sign turning on the ground, a
  rimmed disc the size of what will strike, a ring closing in, a lane of arrowheads), it lands (one white star for a
  frame, a hard ring thrown outward, a stop of time and a shake by its weight), and it leaves something (cracks that
  glow for a second or two, a dark stain, a blade stuck in the ground). Nothing waits less than 0.22 s to be seen coming.
- **What spells are made of.** Solid shapes of the element drawn from a few flat wedges each, shaded in three flat
  tones (a pale face toward their own light, the element's colour, a deep shade of it) with the characters' ink line,
  and flat marks of light. No soft spheres, no smoke. A thing that appears is white for its first frames and snaps to
  size; a thing that goes shrinks or sinks, it does not fade.
- **The body does something for every cast**: the cast pose for things thrown, the charge crouch for things gathered,
  the heavy chop for things called out of the ground or down from the sky, the iaido draw for things that are a cut.
  The beam, the bomb and the parry root the player and the dodge lets them go; the drill, the flash and the step make
  the player untouchable until they are over.
- **Brightness.** The marks are drawn at half the strength first tried (`FormKit.Brightness`), because the bloom
  turned every element white. If the look is to be louder or quieter, that is the one number.
- **Things short of the design, by choice or by limit**: pillars do not lift (no enemy can leave the ground yet; the
  blow is heavy instead); enemies press against a wall or a ring and do not walk round; the ring holds the player in
  as well; the brand needs an enemy faced or the mana is lost; the forms have no sounds.
- **Trying them.** The practice yard (the Combat Test scene, and only it) has a rack of twenty-four stones along its
  south side, one for each form. Using a stone lends its sample spell in the element that suits it and keeps the mana
  coming back; hold the spell button to cast. The tall stone in front of the rack gives the next element round the ring.

## The newer weapons as built (Claude's defaults from the day run of 2026-11-16, later afternoon)

Nothing here was decided by the user; it is what was built while they were away, from `docs/design/weapons.md`, to be
kept or changed. Three of the eight designed techniques are in: the **Massive Sword**, the **Scythe** and the **Full
Body Armament**. The other five (Great Spear, Great Hammer, Twin Knives, Great Bow, Chain Sickle) are designed and not
built; the framework below was made so that each is added without changing it.

**The models are not in the game yet.** Seven weapon models were made in Blender earlier today and wait in
`staging/blender/2026-11-16_weapons/` (see `lineup.png` there). The rule is that nothing enters `Art/` before the
user has looked at it, so the sword and the scythe are drawn in play by **stand-ins made of boxes** to the models' own
sizes, axes and slot names. When the models are approved, `Clover > Weapons > Intake Staged Models` copies them in,
writes their manifest rows and rebuilds the prefabs from them; then the scenes are rebuilt. That menu has never been
run, so how the files sit after Unity imports them (the axis conversion) is still unchecked.

**What a weapon is now.** A weapon says for itself how it is carried (which model, in which hand, where it rests on
the body), which clips it stands, runs, guards, rolls and charges with, the numbers of its own guard and dodge, how
fast its bearer walks, and what the screen calls it. Great weapons never leave the hand except to drink or to cast,
when they hang on the back. Each model has two parts the held element colours in play: the **accent** (a ribbon, a
cloth) is dyed, and the **inlay** (the sword's fuller, the scythe's cutting edge, the whole of the Armament's coat)
glows with it: dim at rest, brighter in a surge, brightest for a charge, the skill and the finisher.

**New rules the weapons brought** (all in `docs/design/weapons.md`, section 1; each is used below):

- *Two tiers on a held heavy.* Let go early: the heavy. Held to the first flash (white): a charged heavy, for no
  mana. Held on to the second (the element's colour, and the word "Ready"): the skill, for mana. Without the mana the
  hold waits at the first tier. The Katana and the fists keep their one tier.
- *Poise.* Inside the wind-up and the swing of some attacks a small blow still hurts but does not stop the swing. Red
  attacks ignore it.
- *Distance bands.* A blow can do less near the hand and more at the tip; the arc is drawn with its last metre bright.
- *Enders.* A weapon may have a different chain heavy for each place the light chain is stopped.
- *A counter.* A skill may be a stance instead of a release: a blow taken on it, from any side, is thrown back.
- *Staged finishers.* A finisher may have beats before its blow (time stops for an instant, a spark, a shove or a
  lift of the target) and a **hold**: time runs at two fifths and the camera leans in before the blow, not only after.
- *Shapes.* A blow may be a disc on the ground ahead or a lane, a blow may be carried over ground, may shove what it
  hits, and may throw something that flies on and hangs where it stops.

**Massive Sword.** Three wide sweeps, each wound up faster than the last (0.34, 0.26, 0.20 s), 84 damage in a 190
degree fan and then a full circle, for 64 of the 100 stamina. Poise 12 on the sweeps (an acolyte's cut does not stop
them; a soldier's thrust does), 22 on the heavies, 30 on the charged cut. Heavy: an overhead that buries itself, 44.
Chain heavy: a rising cut dragged up off the ground, 46. Held 0.45 s: a leaping overhead, 62 damage and 80 stagger.
Held 0.95 s with 35 mana: **Elemental Spike Line**, the sword driven into the ground and eight spikes of the held
element running 10 m up the lane, 34 damage to each enemy once, laying the element's affliction. Finisher, **The
Headsman**: the pommel into the chest, a walk round to the enemy's side with the point scoring the ground, the blade
stood up over the head and held, the cut, and the ground splitting along its line a moment after. Guard: the blade
planted like a door: the widest (240 degrees) and cheapest guard in the game and the tightest perfect guard (0.15 s).
Dodge: a short heavy dive (3.9 m, 30 stamina). Walks at 92% speed, and creeps at a quarter speed while charging.

**Scythe.** Long, short, long: out, back, and a full turn. Every cut does 0.6 of its damage nearer than 1.5 m, plain
in the middle, and **1.5 of its damage and 1.6 of its stagger from 2.5 m out** (measured to the near side of the
enemy), so the chain is the best in the game at the tip (69 damage) and the worst on the haft (28). The heavy is the
**back reap**: a hop 1.4 m backward with the cut made by the retreat, so an enemy that was too close is at the tip.
Skill, **Crescent Slash** (hold 0.4 s, 30 mana): a cut, and a crescent of light that flies 6 m striking what it
passes for 20, then hangs there turning for 1.6 s and bites what stands in it for 6 every 0.4 s, laying the held
element. Finisher, **The Wheel**: the blade hooked under the enemy, one hauling turn on the spot, a heave that throws
it into the air, the hold underneath with the scythe drawn back and the eyes up, and one rising cut that meets it.
Guard: the haft held across, 220 degrees. Dodge: a longer step (4.8 m).

**Full Body Armament.** What bare hands become in the held element: a coat on the forearms, fists, shoulders and
shins in the element's colour, named for it ("Fire Armament"). The fists' four blows to the hundredth of a second,
hitting a little harder and 0.3 m further, and **every blow lays one stack of the element's affliction** by itself.
Then a choice by where the chain is stopped: after one light the **ram** (a shoulder charge that shoves the target
2.5 m, poise 22), after two the **rising blow** (the most stagger for its time), after three the **slam** (both fists
on the ground: a disc of 2.2 m), after four the **burst** (both palms: a cone 4.2 m long laying two stacks). Held
0.3 s: **Meteor Fist**, a leaping punch, free. And held past that the bearer stands **braced**: a blow that lands on
the stance in the next 1.2 s, from any side, guardable or red, costs 25 mana, does nothing, and is answered at once
with **Energy Reflect**: a cone 5 m long for 20 damage plus the whole of the blow taken and 95 stagger. With less
than 25 mana the stance is only a stance and the blow lands. Finisher, **One Inch**: six fast blows that deal nothing
and drive the enemy back, a hold with the open hand laid on its chest, and then the arm straightening by the width of
the palm; the element goes through and is laid on what stands behind.

**Stopgaps and things short of the design** (each is a choice to revisit):

- **Every new weapon's stand in the hub is open.** They stand in an arc along the western rim, as the nine newer
  braziers do along the eastern, and like those they are open because there is nowhere yet to win them. The Katana
  still has to be drawn from the stone first.
- **The Armament is taken from a stand, not learned in the tree.** The design makes it a mark in every element's tree
  that replaces Fists while no weapon is carried. That mark, and the rule that puts it to sleep under a weapon, are
  not built; for now it is a weapon like the others, and it re-makes itself when the element in hand changes.
- **Finishers move the target only a little.** The Wheel throws the enemy up (anything that can be lifted) but does
  not carry it round the bearer; the Headsman's pommel does not rock it upright; a god stays where it stands. Enemies
  have no limp "held" pose.
- **The Armament's coat is the same eight boxes in every element**, coloured; the design gives each element its own
  shapes. Its first six clips are the fists' own, set lower and wider.
- **The perfect-guard and blocked-guard reactions** exist for the three new weapons and for the fists, not yet for
  the Katana.
- No element changes a weapon yet (the design's "Elements" paragraphs: Fire's burning ring under the sword's turn,
  Dark's healing tip, Lightning's quick Armament), beyond colour and affliction. No weapon has sounds.

**Trying them.** The practice yard (`Clover > Build > Combat Test Arena`) has a rack of five stands along its north
wall, one per weapon, all open; the hub has the stands. Contact sheets of every clip are in `review/anim/`
(`player_*_gsword.png`, `*_scythe.png`, `*_arm*.png`); the top row of each is the game's camera.

### The gods' gifts and the crowns as built (2026-11-16, evening; Claude's defaults, not yet played by the user)

Every element's Signature and every mixed branch's crown is now a move of its own (`docs/design/FORMS.md`, "Bespoke";
Storm Step and Plasma were already hand-built). Choices made where the design was silent or could not be met in the
time, all **Claude's defaults, open to change**:

- **"Hold the surge button" became "press the surge button again"** for Falling Sun, The World Holds Still, The Sea
  Rises and First Spring. While one of these waits, a second press does not relight the surge.
- **Falling Sun** rises by itself when Kindle is lit (no Heat-at-five gate: Kindle fills Heat anyway) and falls at
  Kindle's end if it was not called down sooner; 60 damage, up to double by burn stacks under it.
- **The Wind Remembers** keeps sixteen cuts and each falls for half the blow it remembers.
- **The Sea Rises** floods for six seconds whatever is left of Wellspring; a wave carries 40% of the blow.
- **The Scales**: the sword deals 60% of the difference, 120 at most, one enemy each; found wanting, mana is emptied
  (the design's "no mana for five seconds" needed a stat that is not there).
- **The Rise** is given by A Life for a Life itself: half health, draining 4 a second to a sliver, one rise until a
  finisher on the killer buys it back (to half health again) or until a real death. Nothing else grants it yet.
- **Hanging Swords**: the held guard is perfect because the perfect window is made far longer than Temper; eight swords.
- **Totality**: the returning light gives each enemy half of what it lost in the dark, 80 at most.
- **Great Lotus** also bursts when its ten seconds are done; +30% for each petal closed.
- **World Anvil**'s blades fly at the next heavy swing, ten damage and a notch each.
- **Light the Pyres**: pyres that overlap strike an enemy once; with no one alight the mana is returned.
- **Six Mirrors** lays a mark and a notch for each cut; its cuts are not critical.
- **Seven Flashes** and **The Wave Stands Still** are built as their own forms (`flashes`, `frozenwave`). The wave rolls at least 2.5 m; training dummies are soaked and frozen but not carried along.
- **Ball Lightning** and **Pawned Heart** are built as their own forms (`ball`, `heart`). The heart is set down behind the player and must be left by 2.6 m before walking back onto it counts; it asks no health price besides itself.

### The Great Spear and the Great Hammer as built (2026-11-16, evening; Claude's defaults, not yet played by the user)

Two more of the eight designed techniques, built to the numbers of `docs/design/weapons.md` sections 7 and 8 on
stand-in models (the Blender models wait in staging for the user's word, as before). Everything here is Claude's
default until the user has played it; where it departs from the design is listed in that document's Appendix C.

**Great Spear.** The weapon that owns a line. Thrust, thrust (a lane 4 m long and 0.9 m wide, 0.48 s each), then a
sweep for what has come round the side. Nearer than 1.4 m it is the haft and does six tenths. Its guard is the
narrowest there is (160 degrees) and **a light press behind the guard thrusts without lowering it**: 8 damage for 12
stamina, as often as the stamina lasts, and a blow that arrives during the thrust is still blocked. Heavy: the whole
length slapped down the lane, 4.4 m. After a light: a vault on the planted spear that kicks the target 2 m back.
Held half a second: the couched charge, 5 m in a third of a second, shoving what it meets. Held 0.9 s with 30 mana:
the **Flying Spear**. The spear itself is thrown (34 m a second, 11 m); it stops in the first enemy for 26, or
stands in the ground. A moment later the bearer **runs to it**, untouchable for the fifth of a second the run takes,
and arrives with a full turning cut that rips it out and strikes all round for 18. Holding the stick away from the
throw refuses the run: the spear tears loose and flies back to the hand in half a second. Until it is back, no
attack can be made. Finisher, **The Standard**: a low thrust, the enemy levered up on the point, a hold with the
spear upright and the head bowed beneath it, and the whole length brought over and down.

**Great Hammer.** Stagger on one target, and a guard that has to be read. Two lights only, and the second is the
slower: a side swing (0.30 s) and a drop onto a disc a stride ahead (0.40 s), 82 stagger between them. Quake, a
bigger drop with a jump in it; after a light, the lift, a golfer's drive that throws the target 2.5 m; held 0.55 s,
Earthshaker, a disc 6 m across for 100 stagger. Every swing shrugs off small blows. Its block is the cheapest in the
game and its **perfect guard opens late**: a press less than 0.08 s before the blow is only a block, so the guard
must be raised early. Its roll is the worst there is. Held a second with 35 mana: the **Spinning Hammer Toss**. Two
turns that strike all round, and the hammer is let go: it flies 8 m end over end through everything, hangs, and
comes back to the hand through everything again at half strength, about a second in all, during which the bearer
has no attacks. Finisher, **The Bell**: a lift under the chin throws the enemy into the air; the bearer turns their
back, lets the hammer's head to the ground, coils and waits; then turns into it and meets it as it comes down, and
it leaves sideways.

**New rules these brought, which other things can use.** An attack from behind the guard. A thrown thing that
stays where it lands and can be followed or called back. A thrown thing that returns. A weapon that is out of the
hand (shown as empty hands that wait, and a catch). A finisher's blow that throws the finished enemy sideways.

**Stopgaps and things short of the design** (each is a choice to revisit):

- Both stands are open in the hub, like the other new weapons', because there is nowhere yet to win them.
- The run to the spear stops at an enemy standing in the way; it does not pass through bodies.
- With the hammer thrown the guard is still the hammer's guard (the design gives the fists' guard), and the bearer
  cannot walk during the spin.
- The spear's idle is always the upright spear; the levelled "line" stance after an attack is not built.
- The hands never slide along a haft: the right hand is fixed where the model's grip is.
- Finishers still do not carry the enemy: The Standard lifts it by the same half-second lift the Wheel uses.
- No element changes either weapon yet, and neither has sounds.
- The Twin Knives were built later the same night: see "The Twin Knives as built" below; the Great Bow and the Chain
  Sickle after them: see "The Great Bow and the Chain Sickle as built".

**A fault found and mended on the way.** A weapon's own guard and dodge numbers (the sword's wide guard, the
scythe's long roll) were quietly put back to the starting weapon's whenever a mark changed a sum. They now stay the
weapon's own, with the marks multiplying them.

**Dark's Risen marks now mean Risen** (a small thing done in the same run). Hollow Strength (a quarter more weapon
damage, and 8 health for a kill), Slow Rot (the drain runs at seven tenths) and Grave Appetite (a point of health
for a blow) act while the player is Risen, not while Hunger lasts. Still short of their words: Hollow Strength does
not stop a flinch and a kill gives health in place of stilling the drain. Dead Already is unchanged (being Risen
for good is not built), and Long Hunger was always about Hunger.

**Trying them.** The practice yard's rack has seven stands now and the hub has seven (the Katana's in the south, the
rest along the western rim). Contact sheets:
`review/anim/player_*_spear*.png` and `player_*_hammer*.png`.

### Marks finished on the night of 2026-11-16 (Claude's defaults, not yet played by the user)

All numbers and wordings here are **Claude's defaults, open to change**. The list of marks still unfinished is
`docs/design/marks_open.md` (284 of 428 counted fully true).

- **Sky Dance (Wind, worn spell, 45 mana)** is its own move: the enemy faced is thrown up and the player goes up
  beside it; five rings close on it in turn (0.34 s each) and a light attack pressed in the last 45% of a ring is a
  cut in time (15 for 10, a star); then for 0.45 s the stick aims a lane and the enemy is thrown down it, bursting
  where it comes to rest for 20 + 6 for each cut in time, 60 stagger, 3 m. The player cannot be struck throughout.
  What cannot be lifted is danced on the ground. With no enemy faced the mana is lost.
- **Poison's beat can be played with.** In Time: a blow within a quarter second of the bite doses again and makes
  blows critical for 1.2 s. Left to Ripen: each bite with no fresh dose is a quarter stronger than the last. Lance the
  Wound: a perfect guard makes every poisoned enemy within 5 m of the attacker bite at once. Catching: each bite sends
  one dose to another enemy within 4 m. One Sickness: one beat for all, 15% stronger for each further poisoned enemy.
  Volatile: no beat at all. Slow Death: twelve doses deeper (not without limit) and no fading. Wither: a quarter harder
  on the Cursed. Feeding Rot: half a point of health a dose from each bite on a Cursed enemy. One Death Feeds: every
  poisoned enemy within 12 m of a Cursed death bites, four health for each.
- **Dead Already** is the Rise for good: every "Risen" mark works always, two health a second drain while an enemy is
  within 12 m (never the last point), three tenths of weapon damage returns, and there is no second rising. It no
  longer shortens the health bar. **Hollow Strength**: Risen, blows do not make the player flinch, and a kill stills
  the drain for three seconds.
- Words changed to say what the mark does (the designs they gave up are noted in the tree files): Slow Burn, Still
  Air, Wide Cloud, Sweet Fever, Devouring Maw, Black Ice, Slow Pulse, Still Pool, Slow Death, In Time, Left to Ripen,
  Wither, One Death Feeds, Dead Already, Falling Thunder, Sky Dance, Undertow, Grey Tide, Long Flood, Dread, Feast,
  Carrion Flies, Sapling, Rooted, Stone Skin, Easy Burden, Landslide, Windchill, Turned Aside, Clean Pass, Splinter
  Ward, Rushing Wake, Following Draft, Flashpoint, Never There.
- **A boon may be for one blow.** Rushing Wake (Wind) and Flashpoint (Fire) end with the first swing that lands.
  Following Draft lengthens every timed gift of the marks by two thirds. A perfect dodge now acts on the attacker it
  dodged (Empty Air, Dazzle), not on the enemy faced. Never There makes a guarded blow cost twice the stamina in place
  of a shorter perfect guard.

### The Twin Knives as built (2026-11-16, night; Claude's defaults, not yet played by the user)

Built from `docs/design/weapons.md` section 3 with its numbers; nothing here was decided by the user. The model is
a box stand-in to the staged knife's size (the Blender knife still waits for approval), one model in both hands.

**Twin Knives.** Be behind them. Five quick cuts (the third is two blows), and every blow on an enemy's back counts
for half as much again, damage and stagger. The fifth cut and the heavy that ends a chain both carry you through
the enemy and turn you to face its back. The dodge is a short cheap spinning sidestep (three for the price of two
rolls), and a light pressed in its second half is a flat cut out of the turn. The guard has the widest perfect
window in the game and the dearest block: a parry and nothing else. Held heavy, 25 mana: **Fan of Knives**, a back
handspring 2.6 m away with five knives of light thrown in a fan (all five on an enemy a step away, one from range;
it earns no mana). Finisher: **Three Crossings**, three dashes through the kneeling enemy to the corners of a
triangle, each leaving a line of light; the back turned, both knives spun once and stopped dead, and the lines
burst on the enemy behind.

Claude's defaults to keep or change:

- What counts as "behind": roughly the rear 145 degrees of the enemy (`PlayerCombat.BackDot` -0.3). A spark in the
  knives' colour marks a blow that counted. The finisher and the fan do not get the bonus.
- The fan is thrown from where you stood when the spring began, so point-blank it is 45 damage as designed; the
  body is already in the air behind the knives as they leave.
- After a passing cut the player is turned by the game to face what was passed; with nothing struck, to face back
  the way they came.
- The cut out of the sidestep needs the press during the second half of the dodge, not after it.

Not yet: no element changes the knives, no sounds, no true reverse grip on the left hand, no sheaths at rest, the
finisher does not move or hold the enemy. The Great Bow and the Chain Sickle are below.

### The Great Bow and the Chain Sickle as built (2026-11-16, late night; Claude's defaults, not yet played by the user)

Nothing here was decided by the user. Both are from sections 6 and 4 of `docs/design/weapons.md`, on box stand-in
models (the Blender models `bow.fbx`, `arrow.fbx` and `chain_sickle.fbx` wait in staging for approval). Each stands
on the hub's weapon stands and the test yard's rack. What departs from the design is in that file's Appendix C.

**Great Bow.** Plant with the shots, cash with the stave. The bow is in the left hand and an arrow in the right.
Every arrow that strikes an enemy stays standing in it (three at most, twelve seconds each; a fourth takes the
oldest one's place), and every blow of the stave (the swat, the stab, the kick, the finisher) snaps every arrow in
what it lands on: 10 damage and 14 stagger each, one after another, with "Snap x2" or "Snap x3" called out. Light,
light: a long backhand swat with the upper limb, then a stab with the arrow in the fist, which leaves that arrow
behind. Heavy: the snap shot, 18 from nine metres. Heavy after a light: a kick that throws the bearer three metres
back and looses a shot from the air on the way. Heavy held 0.6 s: the full draw, 34, through every enemy on its
line and fourteen metres long, walking and turning while it is drawn. Held 1.0 s, 35 mana: **Landmines**, five
arrows loosed upward that come down half a second later in a ring round the enemy faced and stand there. An arrow
that hits no one stands in the ground (six at most, eight seconds; Landmines' stand ten) and goes off under the
first enemy to come within 1.1 m. A perfect guard leaves the arrow from the fist in the attacker. Finisher,
**Nailed**: the stave takes the legs and comes over the top, a foot is set on the enemy, and the high draw is made
slowly straight down with the bow lying across the whole body; loose, and every arrow already in the enemy goes off.
With an element held, a snapped or tripped arrow lays one stack of its affliction.

Claude's defaults inside it, to keep or change:
- The bow is held canted (laid out to the left like a horse archer's), never upright: upright it is a dot from the
  game's camera.
- An attack of the bow turns to an enemy up to twelve metres off, but only one within thirty degrees of where the
  stick (or the body) points; other weapons take the nearest within seventy.
- The swat travels right to left (the design's table said the other way; its text describes this).
- An arrow's fletching takes the held element's colour as it lands (red in the hand and in flight).

Not yet: the second tier of the hold has no pose of its own (the bow is tipped up in the skill's wind-up), the
legs do not walk under the draw (the stance slides), no quiver on the hip, the right hand never looks empty, the
finisher does not throw the enemy flat, no element changes the bow (no fuses, needles or marks), no sounds.

**Chain Sickle.** Two ranges at once, and it moves what it catches. Sickle in the right hand, a weight on a chain
in the left that never stops turning. Light chain: sickle cut, the weight snapped 5.2 m down a lane, a hook that
pulls its target 0.6 m nearer, then the whirl, two rings at 4 m. Heavy: the sickle itself thrown out on the chain
and swept across 220 degrees at 4.6 m. Heavy after a light: the yank, a 5.5 m lane that drags an ordinary enemy
3 m to the bearer's feet; an elite, a god or anything fixed is not dragged, and the bearer flies to it instead.
Heavy held 0.4 s, 30 mana: **Entangle**, the weight thrown 7 m; the nearest enemy it strikes is **Bound** for
2.5 s (held still, a quarter more stagger from every blow, the chain drawn to it), and while it is bound the heavy
button is the yank from any state. A roll, or eight metres of distance, lets go. A perfect guard yanks the attacker
1.2 m in and adds 15 stagger. Finisher, **Unwinding**: the weight wraps the kneeling enemy, the bearer turns the
back and walks the chain out over the shoulder as two heaves drag the enemy in behind, stops with the sickle raised,
and cuts backward without looking.

Claude's defaults inside it:
- Its clips and its model are named `sickle`, not `chain`: "chain" is already every weapon's word for the heavy
  that ends a chain (`attack_gsword_chain`).
- The chain is a plain pale line with the weight on its end, drawn by code; no link pattern, no coil at the hip.
- Entangle strikes everything in its lane and binds the nearest (the design: it stops at the first).
- Something that cannot be held (a god) is bound for one second and is not stopped at all.
- The bearer is not kept within seven metres of what is bound: at eight the chain lets go.
- Unwinding's blow reaches all round, since the enemy is behind the bearer when it is made.

Not yet: the bound enemy's attack is not broken by the bind itself (it is held as ice holds), the enemy does not
spin in on the chain in the finisher, the flight to a heavy enemy has no landing chop of its own, the weight and the
link of `chain_sickle.fbx` have no intake rule (the file holds three objects), no element changes it (no live wire,
frost shackle or leash), no sounds.

### Keystones and marks finished late on the night of 2026-11-16, and the marks reworded (Claude's defaults, not yet played by the user)

Built to their designs, with these **defaults of Claude's, open to change**:

- **Closing Circle** (Wind): the ring is as wide as the player stood from the enemy (2 to 3 m); 6 damage, 5 Unsteady,
  1.2 s in the air. The player cannot be touched for the whole run (the design says the first half only).
- **Eye of the Storm** (Tempest): 3.5 m ring, up to 2.5 s and never less than half a second; each bite lays the
  affliction of the element held, not one of each; the bolt at the end is a heavy blow of 12.
- **Open Furnace** (Fire): the single detonation of a perfect guard is replaced, not added to; each enemy keeps 2 Burn.
- **Single Stroke** (Lightning): the discharge is 14 damage and 10 stagger a stack on the one struck.
- **All at Once** (Ice): the shatter is three times the plain shatter (135 where it was 45), which is more than the
  design's "two to three"; the words said three times and now it is so.
- **Clean Break** (Ice): under a fifth of its health; gods are spared, elites are not. The words now say so.
- **Unmoved** (Ice) and **Heavy Hand** (Earth): no interruption in the wind-up and swing of a heavy blow, red blows
  included; damage taken then is halved (Unmoved) or cut by 35% (Heavy Hand). The recovery after the swing is not covered.
- **Heartwood** (Life): 1 health for 2 mana, left grey; 1 health mended for 3 mana gained. The mana bar stays on the
  screen, empty. A weapon skill asked for when the health cannot pay still goes out.
- **Leaf on the Wind** (Wind): one breath back every 2 s and one for every 3 blows landed (each enemy a swing
  strikes counts); with none there is no dodge. The words now say that.
- **Brittle** (Ice, 5 s, a fifth more), **Buckled** (Forge, 40% more stagger), **Flash Freeze** (Glacier, at three,
  spends Soaked), **Counter Cut** (Metal, critical on an enemy in its wind-up or strike; the design's 25% more
  stagger is not in it).

- **Spring Tide** (Water): damage grows to two and a half times with four times the cost held or more; reach by the
  square root. A form that lasts takes its numbers when it is cast.
- **Master's Answer** (Metal): the window is 2 s; in it any press of heavy is the skill, at once and free (a plain
  heavy blow cannot be made in the window). Outside it the skill costs half again as much.
- **Deep Well** (Water): 70 a flask into a well of 140, paid at 6 a second while a blow has landed in the last 3 s;
  nothing is paid at full health. The well is shown with the other counts (`Well 70/140`), not beside the health bar.

**124 other marks were reworded** to say only what they do today, because they were not built that night. Their
old words and their designs are in `docs/design/marks_reworded.md`; the wording of each new description is Claude's
and open to change, and the designed versions remain the intent.
