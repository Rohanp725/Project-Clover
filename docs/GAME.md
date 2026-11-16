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
summit, which will not open while any enemy stands. Eleven enemies.

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

**Narukami's godly form, "the Storm Itself"** (built later the same night; Claude's design, not yet played by the user):

- *What it is*: when the swordsman falls he does not stay fallen. After a breath (the player is given back a third of
  their health, no flasks), what he became rises out of the cloud beyond the summit's north edge: an oni's head with
  gold horns and a mane of white spikes, shoulders like a hill, and two clawed hands held to him by ropes of
  lightning. He is too large to stand on the floor. The pieces are rigid models moved by code (`Colossus.cs`), not a
  skinned character. 760 health, and the bar across the bottom takes his new name.
- *How he fights*: with his hands, which hang in the air either side of the player when idle.
  - **The slam**: a pale circle follows the player for 0.85 s, stops, and 0.35 s later a hand comes down on it for 28.
    It can be guarded, and a perfect guard throws 60 of the 170 his balance takes. The hand then lies where it fell
    for two seconds.
  - **Red: the sweep**: a red lane is drawn right across the floor through where the player stands, and after 1.25 s a
    hand crosses it edge-on in half a second for 30. Roll through it or leave the lane. The hand lies spent at the
    far edge for a second and a half.
  - **Red: the bolts**: six circles, one every half second, each landing 0.95 s after it is drawn, for 16.
  - Below half health he acts a quarter faster and slams twice, one hand after the other.
- *How he is hurt*: a hand lying on the floor can be cut (three quarters of a blow's damage reaches him). Blows on
  the hands and perfect guards wear down his balance; when it breaks, the head falls on its back on the floor for five
  and a half seconds and takes blows at 160%, and a heavy blow on it is a finisher that takes at least 22% of all his
  health and sends him back up. Hands hanging in the air cannot be reached.
- *Not built*: any attack from the head itself (a breath, a bite); arms; a second phase of his own beyond the faster
  pace; the afterimage and the upgrades were not tuned against him.

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
braziers to the east (Fire, Lightning), and the gate to the north under a great broken ring.

- *What a run can begin with.* Bare hands and Fire always. The Katana once it has been drawn from the stone in any
  run; Lightning once it has been taken from its god in any run. These unlocks are kept between sittings. A stand or
  brazier not yet won is empty or cold and says how to win it. What is chosen is carried through the gate, and a ring
  of light on the floor marks the one carried.
- *The sponsor* is called only "The Voice" for now: a spindle of pale gold light with splinters turning round it. It
  speaks unasked the first time the player ever wakes; afterwards it waits to be spoken to, and has different words
  after a fall (five short sets, each with one true piece of advice), after a god has fallen, and after coming back
  alive. All its words are in one file, `SponsorLines.cs`, and are click-through: each line stays until the use
  button is pressed.

**Not built**: any way to wipe the saved unlocks from inside the game; a title screen; a pause menu.

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
