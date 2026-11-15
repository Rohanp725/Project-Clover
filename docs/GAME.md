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
- Style: **Decided**: cel-shaded with outlines, like an anime. A dark, muted world (stone, ash, storm cloud) in which
  the elements are the only saturated colour, so every spell and elemental effect stands out. The user's condition:
  it must be done well or it will not fit, so it is judged on screenshots early. References: Hades, Cuphead and
  Splatoon for vividness; Elden Ring and Lies of P for the dark setting; Black Myth: Wukong, Elden Ring, Sekiro and
  Lies of P for flashy set-piece moments.
- Style references, what to take from each: not yet pinned down.
- Palette and lighting: not decided.
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
| 2026-11-15 | Designed for a controller | matches the reference games |
| 2026-11-15 | Limited flasks refilled at rest points; death ends the run except through Dark's revive and some later upgrades | the user's choice |
| 2026-11-15 | Fists are the first run's weapon; Full Body Armament is an element-tree upgrade for the unarmed | gives fighting unarmed its own path |
