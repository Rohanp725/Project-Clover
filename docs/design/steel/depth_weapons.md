# Depth for the long game, weapon side: the Heresies and the pace of the Way

Written by Claude on 2026-11-18 for `docs/design/STEEL_AND_ELEMENT.md` sections 5 (the Hand) and 7 (the Way),
following `docs/design/steel/BRIEF.md` and the owner's answers of 2026-11-18 ("be creative and come up with cool
ideas and concepts that take the standard gameplay and break the rules a little to really let the player feel the
fantasy of the cool setup"). **Everything here is Proposal.** Numbers are first guesses for play to tune. The element
Heresies are written at the same time in `depth_elements.md`, which did not exist yet when this was written, so the
two have not been checked against each other.

## Index

**1. What a Heresy is** — one rule broken outright, a real cost on another axis, worn in the Hand's Heresy slot.

**2. What rule-breaking items teach** — nine lessons from Balatro, Slay the Spire, Risk of Rain 2, Path of Exile,
Hades, Noita, Sekiro and Dark Souls.

**3. Weapon Heresies (20)**
- Katana: **Only the Draw** (every press is a draw, and the guard is a cut); **Single Combat** (your steel harms
  only your chosen opponent, twice over).
- Massive Sword: **Dragged Iron** (the walk loads the swing); **On Their Knees** (nothing dies until you finish it).
- Scythe: **The Measure** (the tip takes a share of any life); **The Scythe Walks Alone** (it reaps by itself).
- Great Spear: **Run Through** (the dead stay on the shaft); **Set Against the Charge** (the point strikes by itself).
- Great Hammer: **Head On** (no dodge; swings smash blows aside); **Saved Blows** (lights store, the heavy pays).
- Twin Knives: **Through the Crowd** (walk through bodies); **Only the Back** (from behind kills, from the front
  nothing).
- Great Bow: **Called Home** (six arrows that fly back); **Over the Wall** (every shot falls from the sky).
- Chain Sickle: **None Go Far** (everything struck is chained to you); **The Line Carries You** (you move by the line).
- Fists: **The Body Is the Spell** (no spells; every blow is the element); **On All Fours** (the pack's way).
- War Fan: **Never Cut** (no harm but the shove); **Under My Signal** (the staggered fight for you).

**4. Cross-system Heresies (9)**
- **Three Bloods** (three elements), **Unwearied** (no stamina, no cancels), **Bought Back** (a Bond rank for a second
  chance), **One Word** (one great spell), **The Edge Drinks** (a blade that never cools and drinks you), **Dry
  Flask** (no flasks; skill heals), **The Hoard** (unspent Shards as strength), **Old Magic** (spells for mana, no
  cooldowns), **The Hard Road** (a harder world, a richer one).

**5. Builds a Heresy makes** — four examples.

**6. The long game on the Way** — Memories per land, what opens when, whether a Way runs dry.

**7. Engine needs.**

**8. Sources.**

---

## 1. What a Heresy is

- **One rule, broken outright.** A Heresy says "cannot", "every" or "never", in one sentence, about a rule the player
  has relied on for hours: the guard holds, the dodge saves, blows kill, arrows fly one way, a body carries two
  elements. It replaces the rule with another; it never adds a percentage on top.
- **A real cost, on another axis.** What is given and what is taken are different things, so the player chooses
  which weakness their build can hide.
- **The slot.** The Hand gains a **Heresy slot** after the third god (the end of tier 2), and a second after the
  seventh (the end of the first duo land). A Heresy is changed at a rest fire like the rest of the Hand, so there are
  no trap choices: once earned it is kept for the climb.
- **A weapon Heresy** works only while its weapon is carried, in every aspect and in the god's bough. Where it
  contradicts an aspect's mark, the Heresy wins and that mark sleeps. Two Heresies of the same weapon may be worn
  together (some of the best builds do it).
- **One rule, one Heresy.** Each Heresy names the rule it breaks. Two that break the same rule cannot be worn at once:
  The Body Is the Spell and One Word both rewrite the spell slots; One Word and Old Magic both rewrite the cooldown.
  (Two weapon Heresies of different weapons never meet, since one weapon is carried.)
- **Earned, never bought.** Each weapon's first Heresy is sealed at the end of its Way and opens when **30 Memories**
  have been spent on that Way. Its second is given by an elite or a god, found in the world, or given by Bond's top
  rank. The cross-system Heresies come from gods, the world and Bond.
- **Seen from above.** Every Heresy changes the player's look or the weapon's, so the build can be read at a glance.
- **In the story.** The rules of the fight are the patron's order: the time it turns back, the two elements a mortal
  may carry. A Heresy is a piece of the world that does not obey it. The patron never offers one; at most it remarks
  on one.
- **The word.** "Heresy" is also the name of Risk of Rain 2's set of four lunar items (Visions of Heresy and the
  rest). It is the owner's word and a common one, so it is kept here; say if it should change.

---

## 2. What rule-breaking items teach

1. **Say the rule absolutely.** Path of Exile's keystones are one or two flat lines: Chaos Inoculation makes maximum
   life 1 and the character immune to chaos; Resolute Technique never misses and never crits; Unwavering Stance
   cannot be stunned and, in the second game, cannot dodge roll. A player reads it once and their whole build turns.
2. **Pay on a different axis.** Slay the Spire's boss relics nearly all give one more energy a turn, and each takes
   something unrelated: Ectoplasm, all gold; Runic Dome, the enemies' intents; Coffee Dripper, resting. Which one is
   good depends on the deck, so the same relic is a gift in one run and a trap in the next.
3. **Change what counts, and let the build multiply it.** Balatro's Four Fingers (flushes and straights of four),
   Shortcut (straights with gaps), Pareidolia (every card a face card) and Splash (every played card scores) add no
   score at all. They change definitions, and every other joker then scores more. A Heresy that changes what counts
   as a heavy, a guard or a kill feeds every mark the player already owns.
4. **A cost you can build round becomes a style.** Risk of Rain 2's Shaped Glass doubles damage and halves health,
   which quietly removes one-shot protection; Noita's Glass Cannon gives five times the damage and caps health at 50
   for good. Players build round the cost, and the cost is what they remember.
5. **Replace, do not add.** Risk of Rain 2's Visions of Heresy replaces the primary skill outright; Hades's
   Daedalus hammers rewrite one attack (Double-Edge makes the dash-strike hit twice; Charged Flight makes the shield's
   throw a held charge, and cannot be taken with Dashing Flight). A replaced move is learned fresh, which is depth.
6. **Hand over control on purpose.** Risk of Rain 2's Gesture of the Drowned halves the equipment's cooldown and fires
   it the instant it is ready. Losing the choice of when is the cost and also the fun (The Scythe Walks Alone, Set
   Against the Charge).
7. **Hardship as a reward.** Sekiro's Demon Bell makes enemies hit harder and resist more, for better drops, and can
   be undone; Dark Souls' Calamity Ring simply doubles damage taken. Some players want the harder road and want the
   game to notice (The Hard Road).
8. **Cheat death, at a price.** Noita's Saving Grace leaves the player at 1 health instead of dying when above it.
   Ours trades Bond, the one thing that otherwise never goes down (Bought Back).
9. **Commitment lives in the fight, not the menu.** Dark Souls' Ring of Favor and Protection breaks if taken off, and
   Path of Exile lets dangerous mixes stand (Chaos Inoculation with Eldritch Battery). We keep the danger and drop the
   menu trap: Heresies change freely at rest fires, and only two that break the same rule are kept apart.

---

## 3. Weapon Heresies

| # | Weapon | Heresy | Rule broken | Earned |
|---|---|---|---|---|
| 1 | Katana | Only the Draw | The guard holds a blow | The Way (30 Memories) |
| 2 | Katana | Single Combat | A blow harms what it touches | Katana Bond V |
| 3 | Massive Sword | Dragged Iron | A swing is as strong as its move | The Way |
| 4 | Massive Sword | On Their Knees | Blows kill | The Bargain |
| 5 | Scythe | The Measure | Damage is the weapon's number | The Way |
| 6 | Scythe | The Scythe Walks Alone | You hold your weapon | The Wicker Man's grove |
| 7 | Great Spear | Run Through | The struck stay where they fall | The Way |
| 8 | Great Spear | Set Against the Charge | You strike only when you press | Found: the frozen war |
| 9 | Great Hammer | Head On | You dodge | The golden handmaiden |
| 10 | Great Hammer | Saved Blows | A blow's damage lands when it strikes | The Way |
| 11 | Twin Knives | Through the Crowd | Bodies block you | The Way |
| 12 | Twin Knives | Only the Back | A blow's harm is its number | Found: the Poison court |
| 13 | Great Bow | Called Home | Arrows fly one way, and there are always more | The Way |
| 14 | Great Bow | Over the Wall | Arrows fly straight | The Teacher |
| 15 | Chain Sickle | None Go Far | Enemies go where they like | The Way |
| 16 | Chain Sickle | The Line Carries You | You walk to the fight | The Water god |
| 17 | Fists | The Body Is the Spell | The element rides a charged blade; the Hand holds spells | The Way |
| 18 | Fists | On All Fours | You guard | The pack's alpha |
| 19 | War Fan | Never Cut | Blows hurt | The Way |
| 20 | War Fan | Under My Signal | Enemies are always against you | Placeholder: the Lightning and Wind land |

### 1. Only the Draw (Katana)

- **Breaks**: the guard holds a blow.
- **Instead**: the sword never leaves the sheath but to cut. R1, R2 and the guard button are all draws (R2 the held
  one, the guard a draw toward the attacker). A draw whose first 0.1 s meets an incoming blow cuts it out of the air,
  red ones included: it counts as a perfect guard (cooldowns, mana, the Answer's window), and the attacker takes the
  draw's damage and 30 stagger.
- **Cost**: there is no held guard. A blow that meets the draw any later lands in full. Every draw sheathes (0.3 s)
  before the next: there are no chains.
- **The fantasy**: the swordsman who parries nothing because he cuts first. The blow is the guard.
- **From above**: the sword stays at the hip; each press is one white crescent; a blow cut from the air bursts in
  sparks and its owner reels back.
- **Earned**: the Katana's Way. Best with the Sheath (every draw a heavy) and the Answer (a riposte off every cut).

### 2. Single Combat (Katana)

- **Breaks**: a blow harms whatever it touches.
- **Instead**: the first enemy you strike in a fight is your opponent, joined to your guard by a thin white thread.
  Your weapon deals it double. Your weapon deals nothing to anyone else: other bodies are only shoved 1 m. When the
  opponent dies the thread passes to the next you strike; holding guard 1 s, unstruck, names a new one.
- **Cost**: steel cannot thin a crowd. The crowd is the spells' work: spells, surges and reactions still strike
  everyone, so the Heresy hands the crowd to the element and the duel to the blade.
- **The fantasy**: the duellist who will not be distracted, killing the one he chose while the rest wait for the
  spell.
- **From above**: the white thread from your guard to one body, taut whatever is between.
- **Earned**: the Katana's Bond V (its true-form mark and this together).

### 3. Dragged Iron (Massive Sword)

- **Breaks**: a swing is as strong as its move.
- **Instead**: walking drags the blade's tip on the floor and loads the next swing: +6% damage and +4 stagger for each
  metre walked since your last swing, up to 15 m (+90%, +60). Any swing spends it all; standing still 1 s empties it.
- **Cost**: swings from a standstill deal 30% less, and you walk 10% slower.
- **The fantasy**: the giant's blade dragged across a field, sparks off every stone, and the swing that comes out of
  the walk.
- **From above**: a groove scored behind you, sparks at the tip; its length is the stored weight. The swing throws
  the groove's dust up along its arc.
- **Earned**: the Massive Sword's Way. Best with the Trade (walk into blows and trade them) and with On Their Knees.

### 4. On Their Knees (Massive Sword)

- **Breaks**: blows kill.
- **Instead**: your blows, spells and afflictions never take a common enemy below 1 health. An enemy brought to 1
  kneels with its head bowed for 3 s, and any heavy on a kneeler is the finisher (The Headsman). A finisher on a
  kneeler goes straight on to the next kneeler within 6 m with no recovery: you walk the line.
- **Cost**: nothing dies until you finish it. A kneeler left 3 s rises with a quarter of its health. Elites and gods
  are unchanged (they still need the stagger).
- **The fantasy**: the headsman; a field of the kneeling, and the one who walks it.
- **From above**: kneeling bodies, a red line across each neck; the finisher chain jumps from one to the next.
- **Earned**: the Bargain gives it when he falls, if the player took his bargain (stood for his blow). If they rolled
  away he gives nothing, and the sword's Bond V gives it instead.

### 5. The Measure (Scythe)

- **Breaks**: damage is a number from the weapon.
- **Instead**: a tip blow takes 5% of the enemy's whole health, whatever its armour or guard; 3% from an elite, 1.5%
  from a god. Afflictions are still laid.
- **Cost**: haft and middle blows deal nothing, and nothing raises the tip: temper, criticals and the charged blade's
  +20% do not count on it.
- **The fantasy**: death's blade cuts the length of a life, not its flesh. The giant goes as surely as the rat: twenty
  tip blows for anything common.
- **From above**: each tip blow pulls a grey wisp out of the body and up the blade, and the health bar takes a cut
  notch.
- **Earned**: the Scythe's Way. Best with the Swath (tip after tip) and the Hook (hold them at the tip).

### 6. The Scythe Walks Alone (Scythe)

- **Breaks**: you hold your weapon.
- **Instead**: the scythe hangs in the air at your shoulder and reaps by itself: every 1.2 s it strikes the nearest
  enemy within 5 m with a tip blow. R1 sends it to the faced enemy (it flies there and reaps twice); R2 calls it back
  in a full turn round you; the infused strike is that full turn.
- **Cost**: you fight bare-handed meanwhile (the plainest jab and cross, with no Fists Way) and cannot guard. A red
  blow on the scythe knocks it aside for 1 s.
- **The fantasy**: the reaper's scythe that hunts on its own, while its bearer only points.
- **From above**: an upright scythe drifting beside you, turning its blade to its next target.
- **Earned**: found standing upright in the cut field of the grove when the Wicker Man falls.

### 7. Run Through (Great Spear)

- **Breaks**: a struck enemy stays where it falls.
- **Instead**: a thrust that kills a light enemy, or strikes one staggered or knocked down, carries it on the shaft,
  three at most. Each body on the shaft adds 0.5 m to the lane and +15% to every thrust. R2 flings them all along
  your facing (20 each, to them and to what they land on).
- **Cost**: each body slows your walk 15% and every wind-up 0.05 s; with any on the shaft you cannot guard or vault.
- **The fantasy**: the warrior who walks out of the gate with three on his spear.
- **From above**: bodies stacked down the shaft; the spear grows longer and heavier with each.
- **Earned**: the Great Spear's Way. Best with the Long Point (the lane) and the Cross (the return cut drags).

### 8. Set Against the Charge (Great Spear)

- **Breaks**: you strike only when you press.
- **Instead**: hold the guard 0.5 s to set the spear level. While it is set, the first enemy to enter the point's lane
  (4.6 m) is thrust at once, and again every 0.6 s while anything stands in it. A charging enemy struck while set is
  stopped dead and takes double damage and stagger.
- **Cost**: you cannot walk while set, and the set spear does not block: blows from the front land in full. A dodge
  breaks the set.
- **The fantasy**: the pike set against horse; the gate held by a point no one can pass.
- **From above**: the spear held level with a pale wedge of its reach on the floor; each thrust snaps out on its own.
- **Earned**: found in the champion's land, in the hands of a frozen spearman set against a chariot frozen
  mid-charge, taken as the player passes.

### 9. Head On (Great Hammer)

- **Breaks**: you dodge.
- **Instead**: no swing of yours can be interrupted, and a swing that meets an incoming blow or shot breaks it: the
  blow is smashed aside and does nothing, and its owner stumbles 0.5 s. A red blow met this way is halved instead.
- **Cost**: the dodge is gone; the button is a short step of 1 m with no safe frames. What you do not swing into lands
  in full.
- **The fantasy**: the juggernaut who answers every blow with a bigger one.
- **From above**: a blow meeting the hammer's head bursts in sparks and splinters; the attacker reels.
- **Earned**: the first golden handmaiden, who never dodges and meets each blow, gives it when she falls.

### 10. Saved Blows (Great Hammer)

- **Breaks**: a blow's damage lands when it strikes.
- **Instead**: light blows deal no damage when they land. Each stores its damage and stagger on the head as a band of
  iron, ten at most. The next heavy that lands pays them all at once, +50%. Lights still count as blows (they cut
  cooldowns and give mana).
- **Cost**: struck while holding bands, you lose half of them. Light blows neither stagger nor kill.
- **The fantasy**: the smith's patient taps, then the blow that splits the anvil.
- **From above**: iron bands ringing the head, one per stored blow; the heavy bursts them outward with the blow.
- **Earned**: the Great Hammer's Way. Best with the Quake (place the heavy) and the Anvil (Strike While It Rings
  cannot be stopped).

### 11. Through the Crowd (Twin Knives)

- **Breaks**: bodies block you.
- **Instead**: you walk and dodge through enemies as if they were not there. Every enemy you pass through is cut (6)
  and loses you for 0.5 s, turning the wrong way.
- **Cost**: the knives cannot guard at all (the guard button does nothing), and you take 15% more from every blow.
- **The fantasy**: the ghost in the crowd. You pass through a line of them and it bleeds.
- **From above**: you slip through bodies, which flinch with thin red ticks and turn their backs to you.
- **Earned**: the Twin Knives' Way. Best with the Shadow (every pass is a crossing) and Only the Back.

### 12. Only the Back (Twin Knives)

- **Breaks**: a blow's harm is its number.
- **Instead**: a blow from behind kills a light enemy outright and deals three times to anything else. A blow from the
  front deals nothing and turns the enemy to face you.
- **Cost**: nothing from the front, the infused strike included. Spells are untouched.
- **The fantasy**: the court assassin, who has never fought a fair fight.
- **From above**: stand behind an enemy and a red seam opens across its back; a blow there closes it.
- **Earned**: found on the inner court's rooftops in the Poison land, beside a court assassin's body.

### 13. Called Home (Great Bow)

- **Breaks**: arrows fly one way, and there are always more.
- **Instead**: you carry six arrows. Every arrow stays where it stands, in bodies and in the floor. Hold R2 0.3 s with
  none nocked and they all fly home at once on straight lines, striking everything between (12 each, through all),
  each snapping as it tears out of a body (the bow's snap, as built).
- **Cost**: six arrows and no more. An empty quiver is empty until you call, and the call roots you 0.3 s.
- **The fantasy**: arrows as a flock that comes home. Every shot is two.
- **From above**: arrows lifting from bodies and floor and streaking back to you in a closing star.
- **Earned**: the Great Bow's Way. Best with the Stave (call them through the enemy at your chest) and the Snare.

### 14. Over the Wall (Great Bow)

- **Breaks**: arrows fly straight.
- **Instead**: every shot goes up and comes down where you aim, 6 to 18 m off, 0.7 s later, on a disc of 1.5 m. It
  ignores cover, guards, shields and walls. The full draw comes down as five in a 3 m disc.
- **Cost**: no direct shot. The fall is slow enough for a moving enemy to walk out of, and nothing nearer than 6 m can
  be shot.
- **The fantasy**: the Teacher's canopy; the archer who shoots over walls and shield-walls (the terracotta ranks).
- **From above**: arrows rising out of sight, a ring of shadow closing on the floor, then the fall.
- **Earned**: the Teacher, left with his laid-down bow (`weapons.md` 13.1).

### 15. None Go Far (Chain Sickle)

- **Breaks**: enemies go where they like.
- **Instead**: every enemy the weight strikes is chained to you for 10 s, four at most. None can go more than 6 m from
  you, and when you dodge or are thrown you drag them all along. A chain that goes taut strikes its enemy for 4.
- **Cost**: you cannot go more than 6 m from them either. A heavy enemy holds you in place; a god drags you.
- **The fantasy**: the chain-master at the middle of a web, a crowd held round him like dogs on leashes.
- **From above**: chains from your wrist to every bound body, snapping taut and falling slack.
- **Earned**: the Chain Sickle's Way. Best with the Weight (the whirl strikes the whole leashed ring) and the Bind.

### 16. The Line Carries You (Chain Sickle)

- **Breaks**: you walk to the fight.
- **Instead**: R2 at an enemy or anchor within 9 m yanks you to it in 0.25 s, arriving with a heavy blow on an enemy or
  swinging round an anchor. The dodge is a short yank to the nearest anchor or enemy within 5 m, untouchable on the
  way; with none near, a hop of 1 m.
- **Cost**: you walk at half speed and do not roll. Open ground with nothing to catch is slow and dangerous.
- **The fantasy**: the fisher who swung himself about the beach on hooked islets.
- **From above**: you fly along a taut line; the line flashes before each pull.
- **Earned**: the Water god: his line, left tangled round him when he falls, whichever choice is made.

### 17. The Body Is the Spell (Fists)

- **Breaks**: the element rides only a charged blade, and the Hand holds four spells.
- **Instead**: the four spell slots are sealed. In their place every blow lays its element's affliction (the left arm
  and leg the first element, the right the second, as the Armament does), your reach grows by 1 m as the substance is
  thrown off the knuckle, and enemies' stagger meters fill twice as fast from your blows. The surge stays.
- **Cost**: no spells at all: no cooldown play, no duo spells. Afflictions you lay last half as long.
- **The fantasy**: the element as a martial art. The bender, not the caster.
- **From above**: each blow throws a short tongue of the substance off the fist; the spell diamond is gone from the
  HUD.
- **Earned**: the Fists' Way. Best with the Armament (two elements, one each arm) and Three Bloods.

### 18. On All Fours (Fists)

- **Breaks**: you guard.
- **Instead**: you fight as the pack does. You run 25% faster; the dodge is a pounce of up to 5 m onto the faced
  enemy (a light one is knocked down 0.8 s) or, with none faced, a bound of 4 m; a pounce through an attack counts as
  a perfect dodge. Blows are claws: reach +0.4 m, and every third blow on one enemy is critical.
- **Cost**: no guard and no perfect guard. Flasks take twice as long to drink (so it is little cost with Dry Flask,
  and the two may be worn together all the same).
- **The fantasy**: the man-wolf; the alpha's way of fighting, learned from the alpha.
- **From above**: you run low on hands and feet; each dodge is a short arcing leap.
- **Earned**: the pack's alpha, beside his technique Bear Them Down.

### 19. Never Cut (War Fan)

- **Breaks**: blows hurt.
- **Instead**: the fan deals no damage. Every blow shoves three times as far, and the harm is what the shove does: an
  enemy driven into a wall, a pillar, a hazard or another enemy takes 24 plus 8 for each metre of shove left, and so
  does what it strikes. Shoved off an edge, it falls.
- **Cost**: in an open field the fan harms nothing by itself. The charged blade's afflictions still ride the shove.
- **The fantasy**: the iron-fan master who never draws blood. The room does the killing.
- **From above**: bodies flung across the floor, bursting where they strike stone.
- **Earned**: the War Fan's Way. Best with the Commander's Fan (every shove aimed at one body).

### 20. Under My Signal (War Fan)

- **Breaks**: enemies are always against you.
- **Instead**: an enemy you stagger takes your red cloth and fights for you for 5 s: it turns on the nearest enemy,
  and enemies attack it. Elites and gods are not turned; they lose their next attack instead.
- **Cost**: your blows deal 25% less, and a turned enemy cannot be harmed by you while it is turned.
- **The fantasy**: the general whose signal commands even his enemies. (Unlike Dark's Devotion, which raises the dead,
  this turns the living for a moment.)
- **From above**: a red cloth tied on the turned enemy's arm; it turns its back to you.
- **Earned**: placeholder, from the Lightning and Wind land's elite or god when that land is designed.

---

## 4. Cross-system Heresies

| # | Heresy | Rule broken | Earned |
|---|---|---|---|
| 21 | Three Bloods | A mortal body carries two elements | Taking both gods of a duo land |
| 22 | Unwearied | Actions cost stamina | Found: the Metal land's forge |
| 23 | Bought Back | Death returns you to the rest fire | The man on the bed of arrows |
| 24 | One Word | The Hand holds four spells | The first element at Bond V |
| 25 | The Edge Drinks | The charged blade fades after three blows | The first Pair Bond at rank III |
| 26 | Dry Flask | You heal by flask | Found: the Fire land's sealed well |
| 27 | The Hoard | Unspent Shards are only money | Found: the hub, from tier 3 |
| 28 | Old Magic | Basic spells never cost mana | Found: the Ice land's giant's bones |
| 29 | The Hard Road | The world is as hard as it is | The Poison court's great gong |

### 21. Three Bloods

- **Breaks**: a mortal body carries one weapon and two elements (the story's rule, decided).
- **Instead**: the Hand holds three elements. The four slots take spells from any of the three; all three pairs react
  and their duo branches are open. L2 with R1 and R2 carry the first two; the third rides the charged blade only.
- **Cost**: every spell's cooldown is 40% longer, the surge costs 20 more mana, and no Devotion can be had.
- **The fantasy**: the body that holds more than a mortal should: three gods' blood at once.
- **From above**: the charged blade turns through all three substances in turn (smoke, then frost, then sparks).
- **Earned**: taking both gods of one duo land.

### 22. Unwearied

- **Breaks**: actions cost stamina.
- **Instead**: there is no stamina. Nothing costs it and nothing runs out: guard, dodge and swing as often as you
  like.
- **Cost**: nothing can be cancelled. Every action runs its full recovery (a dodge 0.35 s; a blow's whole recovery,
  with no dodge out of it; a guarded blow pushes you back 0.3 s).
- **The fantasy**: the form that never tires, every motion finished; the smith's bronze body that would not limp.
- **From above**: the stamina bar is gone; each recovery shows as a short white tick at your feet.
- **Earned**: found in the Metal land's volcano forge, a spare bronze heart on the smith's bench.

### 23. Bought Back

- **Breaks**: death returns you to the rest fire.
- **Instead**: once a rest, a killing blow leaves you on your feet at half health, untouchable 2 s. The weapon you
  carry loses one Bond rank, to be earned again; if it has none, the first element pays; if neither has one, it does
  not save you.
- **Cost**: Bond, the one thing that otherwise never goes down.
- **The fantasy**: choosing your hour; refusing death and paying for it with what you have lived through.
- **From above**: as you rise, the weapon's worn look (its Bond look, 8.1) flakes away from it.
- **Earned**: given by the man on the bed of arrows in the champion's land (the Bhishma nod: he chose the hour of his
  own death).

### 24. One Word

- **Breaks**: the Hand holds four spells.
- **Instead**: one slot. The spell in it deals double damage and stagger and covers half again the area. Its cooldown
  is two and a half times as long, but weapon play takes twice as much off it.
- **Cost**: one spell. No second role; no duo spell unless it is the one.
- **The fantasy**: the one great word, spoken seldom and heard by all.
- **From above**: the spell's sign on the ground is half again as wide; the HUD's diamond is one great slot.
- **Earned**: the first element to reach Bond V.

### 25. The Edge Drinks

- **Breaks**: the charged blade fades after three blows.
- **Instead**: the blade is charged with the first element and never fades: every blow lays its affliction and hits
  20% harder.
- **Cost**: every weapon blow that lands costs 1 health from the real bar. A kill gives back 3.
- **The fantasy**: the blade that drinks its bearer to stay lit.
- **From above**: the element always on the edge, and a thin dark trickle running from your hand down the grip.
- **Earned**: the first Pair Bond to reach rank III.

### 26. Dry Flask

- **Breaks**: you heal by flask.
- **Instead**: no flasks. Every perfect guard and perfect dodge heals 6; a finisher heals 15.
- **Cost**: no flask at all. A mistake is not undone by a drink.
- **The fantasy**: healed by skill alone; the first land's thirst turned to strength.
- **From above**: the flask gone from your belt; a heal draws a short breath of mist up from the floor at your feet.
- **Earned**: found in the Fire land's sealed well shrine. It is the first Heresy most players see, long before they
  can wear one.

### 27. The Hoard

- **Breaks**: unspent Shards are only money.
- **Instead**: each 50 unspent Shards you carry gives +1% damage and +1% to afflictions laid, up to +30% (1,500
  Shards).
- **Cost**: on a death they are gone for good. The killer does not carry them, and nothing gives them back.
- **The fantasy**: greed made strength; walking heavy with what you should have spent.
- **From above**: Shards glinting in a ring at your belt, more as the purse grows.
- **Earned**: found in the hub from the start of tier 3, in a room that was shut before: the patron's. It feeds the
  story's doubt about what the patron gathers.

### 28. Old Magic

- **Breaks**: basic spells never cost mana.
- **Instead**: spells have no cooldowns. Each basic spell costs 15 mana; the larger spells cost as now. Weapon blows
  give twice the mana.
- **Cost**: mana does not rise out of a fight, and weapon play no longer cuts cooldowns. An empty bar is an empty hand.
- **The fantasy**: casting as the old world did, as fast as you can pay. It is also the game's magic as first built,
  kept for those who liked it.
- **From above**: the spell diamond shows no cooldown rings, only the mana bar beside it.
- **Earned**: found in the Ice land on the climb up the giant's bones (the first world's body).

### 29. The Hard Road

- **Breaks**: the world is as hard as it is.
- **Instead**: enemies strike 30% harder, have 25% more poise, and wait a quarter less between attacks.
- **Cost**: that. The reward: Shards from kills are doubled, and every elite and god gives 1 Memory more.
- **The fantasy**: the road chosen hard, and the game noticing.
- **From above**: every enemy carries a thin ring of ash at its feet.
- **Earned**: strike the great gong of the Poison land's inner court; its note stays with you.

---

## 5. Builds a Heresy makes

- **The headsman's walk.** Massive Sword with Dragged Iron and On Their Knees: cut a crowd down to kneeling, then walk
  the line, each step loading the next finisher.
- **The ghost.** Twin Knives with Through the Crowd and Only the Back: passing through a body leaves you behind it,
  where every blow kills.
- **The bender.** Fists with The Body Is the Spell and Three Bloods, the Armament aspect: three afflictions laid by
  bare hands, reacting with each other, and no spell cast all fight.
- **The duel and the storm.** Katana with Single Combat, and a Hand of crowd spells (Flame Wave, Thunderclap, Ball
  Lightning): the blade for the one, the element for the many.

---

## 6. The long game on the Way

### Memory income (Proposal)

From `weapons.md` 0.2, kept: **a god 6, an elite 3, the first defeat of each kind of enemy 1**. Added: **a named
fighter on a side path 2** (about one a land), and **the last champion 8** (three phases, and the last of the climb).
Memories cannot be farmed.

| Land (in climb order) | God | Elites | New kinds | Named | Memories | Total by its god |
|---|---|---|---|---|---|---|
| Fire | 6 | 3 | 4 | 2 | 15 | 15 |
| Water (tier 2) | 6 | 6 (the brothers) | 4 | 2 | 18 | 29 to 33 |
| Metal (tier 2) | 6 | 3 | 3 | 2 | 14 | 47 |
| Ice (tier 3) | 6 | 3 | 3 | 2 | 14 | 61 |
| Poison (tier 3) | 6 | 3 | 3 | 2 | 14 | 75 |
| Lightning and Wind (tier 4) | 12 | 6 | about 6 | 2 | about 26 | 101 to 103 |
| Earth and Life (tier 4) | 12 | 6 | 8 | 2 | 28 | 129 |
| The champion's land | 8 | 3 (the Teacher) | 0 (its enemies come from every land) | 0 | 11 | 140 |

### What the Memories buy

| Piece | Memories |
|---|---|
| A plain Way (trunk 19: temper 9, infusion 8, technique 2; three boughs of about 13) | 58 |
| A god's bough | 13 (the Alchemist's Hooks 16) |
| A Way with its god's bough | about 71; the Katana 81 with its five element infusion marks; the Fists 60 (no god, two techniques) |
| **The Body branch** (Proposal: health four ranks, stamina three, flask count two, flask strength two, poise one, 2 each) | 24 |
| Every Way and the Body | about 740 |

### What opens on the Way, land by land (Proposal)

Memories in hand are not enough on their own: each stage opens with the climb, so a player who spends on one weapon
cannot outgrow the land they are in.

| When | What opens on the Way |
|---|---|
| The Fire land | The Way itself (Fists first); one bough on each weapon; Temper I and II; Body ranks I; Hold the Gate |
| The Fire god | A second bough on each weapon; the infusion marks (an element is now owned); the Sun-Spear's bough |
| Tier 2 | Fillet, Hook and Hold, the Smith's Tap; the Jawbone's and the Smith's Hammer's boughs |
| The third god | The third bough on each weapon; Temper III; Body ranks II; **the first Heresy slot** |
| Tier 3 | Bear Them Down, the Scorpion's Tail; the Huntress's Bow and the Alchemist's Hooks; the first Bond V marks |
| The fifth god | Temper IV; Body ranks III |
| Tier 4 | The Return Blow, Over the Shoulder, the two placeholder techniques; Brannoch's, Carnwyn's, Narukami's and the Wind god's boughs |
| The seventh god | Temper V; **the second Heresy slot** |
| The champion's land | Only the Eye; Over the Wall, Set Against the Charge, Bought Back; the Super Form reads every Way |

### Does a Way run dry? Is one too deep?

- **A player who keeps one weapon all climb** (the common case: the Sun-Spear from the first god) has its Way with the
  god's bough and the whole Body (95) by the end of the first duo land, at about 100 Memories, and no sooner, because
  Temper V opens there. **Their Way runs dry at about four-fifths of the climb.** That is the right place: the last
  fifth is where the earned depth arrives (Bond V, Pair Bond III, the second Heresy slot, the Teacher's Heresy), and
  the 40 or so Memories left over buy a second weapon, which the champion's Super Form then draws on. A Way should
  not fill before tier 4; the gates above see to that.
- **A player who spreads over two weapons** fills one Way and about two-thirds of the second (140 less the Body's 24).
- **The weapon Heresy arrives in time**: 30 Memories spent on one Way is reached around the third god by a player
  who favours one weapon, which is when the first slot opens.
- **No single Way is too deep for one climb** (71 against 140). **All of them are**, by design: about 740 in all is
  five first climbs. Recommend that **a new climb pays Memories again at 1.5 times** (the Ways bought stay bought), so
  four climbs fill every Way.
- **The thinnest Ways**: the Fists (no god's bough) carry two techniques and two Heresies instead, and the Armament
  grows with every element taken, so they hold up. The War Fan's depth waits on its land like its technique.

**Recommended numbers**: about 140 Memories in a first climb (15, 18, 14, 14, 14, 26, 28, 11 by land); a plain Way 58,
with a god's bough about 71; the Body 24; the first Heresy slot at the third god and the second at the seventh; a
weapon's first Heresy at 30 Memories spent on its Way; later climbs at 1.5 times.

---

## 7. Engine needs

- **The Hand's Heresy slots** (two, opened by progress), each Heresy tagged with the rule it breaks so two with the
  same tag cannot be worn together.
- **Overrides of the standard rules**: the guard input turned into a draw or a set (Only the Draw, Set Against the
  Charge) or removed (Through the Crowd, On All Fours); the dodge removed or replaced by a step, a pounce or a yank
  (Head On, On All Fours, The Line Carries You); stamina off with uncancellable recoveries (Unwearied); spell slots
  sealed or merged into one (The Body Is the Spell, One Word); flasks off and heals on perfects (Dry Flask); spells
  paid in mana with no cooldowns (Old Magic); a third carried element (Three Bloods).
- **The damage model**: a floor at 1 health and a kneeling state with chained finishers (On Their Knees); damage as a
  share of maximum health that ignores every bonus (The Measure); damage stored by lights and paid by the heavy (Saved
  Blows); damage by side of the body, zero from the front (Only the Back); harm to one chosen target only (Single
  Combat); harm from impacts after a shove (Never Cut); distance walked as a charge (Dragged Iron).
- **Bodies and links**: no collision with enemies (Through the Crowd); bodies carried on a weapon and flung (Run
  Through); leashes from the player to enemies that drag both ways (None Go Far); a grapple yank to enemies and
  anchors (The Line Carries You); a weapon that fights as a helper (The Scythe Walks Alone); a zone that attacks by
  itself (Set Against the Charge); a swing that cancels an incoming blow or shot (Head On).
- **Arrows**: a counted quiver and a recall (Called Home); shots that arc down on a point (Over the Wall).
- **Enemies**: a turned enemy for a time (Under My Signal); global tuning multipliers (The Hard Road).
- **Death and progress**: a death intercept once a rest (Bought Back), Bond ranks that can be lost, unspent Shards as a
  buff with the Bloodborne return switched off (The Hoard).

---

## 8. Sources

- Slay the Spire boss relics: [dood.gg relic guide](https://dood.gg/en/slay-the-spire/guides/relic-guide/),
  [Defect relic tier list](https://maybelatergames.co.uk/tools/slaythespire/tierlist/relics/defect/),
  [Avid Achievers boss relic tier list](https://avidachievers.com/tier-list/slay-the-spire-tier-list/).
- Risk of Rain 2 lunar items (Shaped Glass, Gesture of the Drowned, Visions of Heresy and the Heresy set):
  [Risk of Rain 2 Wiki](https://riskofrain2.wiki.gg/wiki/Items), [Pocket Tactics](https://www.pockettactics.com/risk-of-rain-2/lunar-items),
  [DualShockers](https://www.dualshockers.com/ror-2-lunar-items-guide/).
- Path of Exile keystones: [PoEDB Keystone](https://poedb.tw/Keystone), [senseos.gg PoE2 keystones](https://senseos.gg/guides/path-of-exile-2/poe2-keystones/),
  [Fextralife PoE2 passive skills](https://pathofexile2.wiki.fextralife.com/Passive_Skills).
- Hades: [Daedalus Hammer (Hades Wiki)](https://hades.fandom.com/wiki/Daedalus_Hammer),
  [Shield of Chaos hammers](https://www.soloplayguide.com/games/hades/weapons/shield-of-chaos),
  [Hades II Daedalus Hammer](https://hades2.wiki.fextralife.com/Daedalus_Hammer),
  [Hades II boons and Chaos](https://vandal.elespanol.com/en/hades-2/hades-2-all-god-boons-effects-and-benefits.html).
- Balatro: [Four Fingers (Balatro Wiki)](https://balatrowiki.org/w/Four_Fingers),
  [BalatroHQ on hand-rule jokers](https://balatrohq.com/guides/blueprint-brainstorm-combo/),
  [dood.gg poker hands](https://dood.gg/en/balatro/guides/poker-hands/), [Balatro (Wikipedia)](https://en.wikipedia.org/wiki/Balatro).
- Noita perks: [Noita Wiki perks](https://noita.wiki.gg/wiki/Perks), [dood.gg perks guide](https://dood.gg/en/noita/guides/perks-guide/).
- Sekiro and Dark Souls: [Demon Bell](https://sekiroshadowsdietwice.wiki.fextralife.com/Demon_Bell),
  [Kuro's Charm discussion](https://gamefaqs.gamespot.com/boards/241004-sekiro-shadows-die-twice/77593965?page=2),
  [Ring of Favor and Protection](https://darksouls.wiki.fextralife.com/Ring_of_Favor_and_Protection),
  [Dark Souls rings](https://darksouls.fandom.com/wiki/Rings_(Dark_Souls)).
- The Teacher's and the champion's land's lore: [Ekalavya](https://en.wikipedia.org/wiki/Ekalavya),
  [Drona Parva](https://en.wikipedia.org/wiki/Drona_Parva),
  [Mahabharata Online on Drona](https://www.mahabharataonline.com/stories/mahabharata_character.php?id=76).
- From memory, not searched this session: pike set against cavalry; the headsman's block; the reaper's scythe as an
  image of death. Check before any is put into in-game text.
- The project: `docs/design/STEEL_AND_ELEMENT.md`, `docs/design/steel/BRIEF.md`, `docs/design/steel/README.md`,
  `docs/design/steel/weapons.md`, `docs/design/WORLD.md`, `docs/GAME.md`.
