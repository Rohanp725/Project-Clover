# The Way, bough by bough: options to choose from

Written by Claude on 2026-11-19 for the owner, from the owner's structural answers of the same day (recorded as
Decided in `weapons.md` section 0). **Decided (owner, 2026-11-19): all 56 questions in section 12 are answered, the
recommended option every time except 3.5 (the Swath's keystone renamed The Long Stroke); so each bough's
"Recommended" fork option below is the decided one. Numbers stay first guesses.** The starting point is the draft of
2026-11-18 in `weapons.md`; where a bough's identity there is strong it is kept, and where a more exciting real
tradition fits, a replacement is offered and said so. Numbers are first guesses in the scale of the moves as built
(`docs/GUIDE.md` section 3) and the yardstick in `docs/design/weapons.md` section 0: stamina 100, light blows 5 to 34,
heavies 18 to 62, stagger 20 to 100, no plain wind-up over 0.50 s.

**How to read a bough.** The opening (the aspect: what changes at once), the stem (three marks every player of the
form buys), then two or three **fork options**. Each fork option is a pair of paths, A and B, each two marks and a
keystone. One option is marked **Recommended**, with the reason in one line. Each keystone has a one-line pitch and
the real tradition or myth it comes from. `[clip: new]` marks a move that needs a new animation clip; `[clip: reuse
X]` one that does not; `[fx]` a rule that needs a new effect but no clip. Each bough ends with its clip count. The
questions to put to the owner are numbered at the end (section 12), recommended option first.

**Every mark is checked against** the Heresies (`depth_weapons.md`), the element trees as built (`docs/GUIDE.md`
section 7), the reactions, the gated techniques and the twelve Pair Bond standouts (`weapons.md` 15.4). Where a mark
comes close to one of them it says so. No mark is only "+X% damage" except Temper and each stem's one Number mark.
Everything is read from above: what the floor shows is said where it matters, and every new enemy-facing threat has
a tell.

## Index

- **0. The shape, the trunk, the Memories and the Body**: one bough drawn; the trunk's five weapon basics; the new
  prices and income; the Body branch in full.
- **1. Katana**: the Sheath, the Stream, the Answer; Narukami's Storm Katana.
- **2. Massive Sword**: the Doppelsöldner (offered in place of the Wall), Half-Sword, the Trade; Brannoch's Standing
  Stone.
- **3. Scythe**: the Swath, the Hook, the Harvest; Carnwyn's Golden Scythe.
- **4. Great Spear**: the Long Point, the Pole, the Cross; Khepharu's Sun-Spear.
- **5. Great Hammer**: the Quake, the Swing, the Anvil; the Smith's Hammer.
- **6. Twin Knives**: the Shadow, the Rattle, the Reverse Grip; the Alchemist's Hooks.
- **7. Great Bow**: the Stave (as the bow-spear), the Snare, the Long Draw; the Huntress's Bow.
- **8. Chain Sickle**: the Weight, the Bind, the Sickle; the Fisher's Jawbone.
- **9. Fists**: Wrapped Hands, the Hold, the Armament (no god carries the Fists).
- **10. War Fan**: the Iron Fan, the Commander's Fan; Kazehaya's Fan.
- **11. Build cost in clips**.
- **12. Questions for the owner**: general questions, then four to seven per weapon.
- **13. Sources**.

---

## 0. The shape, the trunk, the Memories and the Body

### 0.1 One bough (Decided, owner, 2026-11-19, as to shape; the counts are Proposal)

```text
                                           +-- path A: mark -- mark -- KEYSTONE A --+
                                           |                                       |
  ASPECT -- stem: Number -- mark -- mark --+                                       +-- one keystone active,
  (the form)                               |                                       |   set at a rest fire
                                           +-- path B: mark -- mark -- KEYSTONE B --+
```

- **A plain bough is ten marks**: the aspect, a stem of three (the one Number and two Moves or Rules), and two paths
  of two marks and a keystone. That is the top of the owner's "about 8 to 10", chosen so each path has room to build
  toward its keystone. Cost: aspect 2, stem 5, each path 8: **23 Memories**.
- **A god's bough is twelve marks** (plain size plus two): the aspect (free after Take), a stem of five (one Number and
  four Moves or Rules), and two paths of two marks and a keystone. Each keystone is a **god move**, with its own
  animation and effect in the god's element. Cost: stem 9, each path 8: **25 Memories**.
- Marks are bought in order along the bough: the aspect, the stem, then either path. Both keystones can be bought
  over time; **one is active**, chosen at a rest fire like the aspect (Decided). **Recommended**: a path's two marks
  work whenever their bough is active, whichever keystone is set; only the keystones exclude each other (question G1).
- **Inspiration** (Decided): each bough starts from a real fighting tradition, and its keystones go over the top into
  myth. Grounded in the stem and paths, flashy at the peaks.
- The Bond mark (`weapons.md` 0.6) gives the true form of whichever keystone is active. True forms are written once
  the keystones are chosen.

### 0.2 The trunk (Decided, owner, 2026-11-19: temper, infusion, technique and Bond kept, five weapon basics added)

| Part | Marks | Memories |
|---|---|---|
| Temper I to V (`weapons.md` 0.3) | 5 | 1, 1, 2, 2, 3: 9 |
| Infusion, one per element family (0.4) | 4 | 2 each: 8 |
| The gated technique (0.5) | 1 (the Fists 2) | 2 (the Fists 4) |
| **The weapon basics** (new) | 5 | 9 |
| The Bond mark (0.6) | 1 | given at Bond V |
| **The trunk** | 16 | **28** (the Fists 30) |

**The weapon basics** are five marks every form of the weapon keeps, bought in any order once the weapon is owned.
They share one frame on every weapon, so the chart reads as one system and the inputs are learned once:

| Slot | Kind | Input | What every weapon's version does |
|---|---|---|---|
| 1. Out of the roll | Move 2 | R1 in the last 40% of a roll or within 0.15 s of its end (engine E10, `DashAttack`) | A weapon blow out of the dodge that counts as the chain's first step |
| 2. The break | Move 2 | R2 in the same window | A kick, shove or hilt: about 45 stagger, and a raised guard or shield is broken for 1 s |
| 3. Quicker hands | Number 1 | none | One recovery, 10 to 15% shorter |
| 4. The weapon's own | Move 2 | Guard held, then R1 (engine E10, `GuardAttack`), unless the weapon says otherwise | A throw, a pommel or a trick only this weapon has |
| 5. Footing | Rule 2 | none | A small rule about the guard, the dodge or the space round you |

Where a bough already uses one of these inputs, the bough's move wins while that bough is active, and the bough says
so. Slots 1 and 2 put the owner's "better dodge-attack" and "guard-break kick or shove" on one place for every
weapon: roll, then R1 to cut, R2 to break.

### 0.3 Memories (Decided, owner, 2026-11-19: prices kept, income raised)

**Prices, kept**: aspect 2; Number 1; Move 2; Rule 2; keystone 4; Temper 1, 1, 2, 2, 3; infusion 2; technique 2; Body
marks 2, the Body's two bearings 4.

**What things cost now**

| Piece | Memories |
|---|---|
| A plain bough (10 marks) | 23 |
| A god's bough (12 marks) | 25 |
| The trunk | 28 (the Fists 30) |
| **A plain Way** (trunk and three plain boughs) | **97** |
| **A Way with its god's bough** | **122** |
| The War Fan (trunk, two plain boughs, the god's bough) | 99 |
| The Fists (trunk with two techniques, three plain boughs) | 99 |
| The Katana with its five element infusion marks | 132 |
| The Body branch | 28 |
| Every Way and the Body | about 1,210 |

The other element infusion marks written in `docs/design/weapons.md` (Brand, Glacier Edge and the rest) are outside
these sums, at 2 each, as before.

**Income, raised** (Decided: elite about 4, god about 8, each new enemy kind 1 the first time, Memories found in the
world; the rest Proposal): a named fighter on a side path 2 (kept from `depth_weapons.md` 6), found Memories 3 in each
solo land and 5 in each duo land (on the "see it, then find the way" side paths of the level design rules), the last
champion 12 and the Teacher 4. Memories still cannot be farmed.

| Land (one climb order) | God | Elites | New kinds | Named | Found | Land | By its god |
|---|---|---|---|---|---|---|---|
| Fire | 8 | 4 | 4 | 2 | 3 | 21 | 21 |
| Water | 8 | 8 (the brothers) | 4 | 2 | 3 | 25 | 46 |
| Metal | 8 | 4 | 3 | 2 | 3 | 20 | 66 |
| Ice | 8 | 4 | 3 | 2 | 3 | 20 | 86 |
| Poison | 8 | 4 | 3 | 2 | 3 | 20 | 106 |
| Lightning and Wind | 16 | 8 | 9 | 2 | 5 | 40 | 146 |
| Earth and Life | 16 | 8 | 8 | 2 | 5 | 39 | 185 |
| The champion's land | 12 | 4 (the Teacher) | 0 | 0 | 2 | 18 | 203 |

**The pace this gives** (the owner's aim: one main bough by mid-game, a full Way near the end):

- **One main bough by mid-game.** By the third god (66) a one-weapon player has the five basics (9), Temper I to III
  (4), two infusions (4), one whole bough (23) and the Body's stem (8), 48 in all, with about 18 toward the god's
  bough's stem. A bough alone is done by the second god.
- **A full Way near the end.** A Way with its god's bough and the whole Body is 150, reached in the second duo land
  (between 146 and 185), about three-quarters up the climb. The stage gates of `depth_weapons.md` 6 (Temper V and the
  second Heresy slot at the seventh god, the tier-4 gods' boughs in tier 4) are kept, so no Way fills before tier 4.
- **What is left over**, about 50, buys a second weapon's basics and one bough, which the champion's Super Form then
  draws on. A new climb paying 1.5 times (kept from `depth_weapons.md`) brings about 300, so about four more climbs
  fill every Way.

### 0.4 The Body branch, in full (Decided, owner, 2026-11-19, as to shape; the numbers are Proposal)

Twelve marks, 28 Memories. Steady numbers first, then a fork between the enduring body and the light body, each ending
in a **bearing**. Both sides can be bought and their numbers all count; the two bearings exclude each other like a
bough's keystones and are set at a rest fire (question G5). Found pieces in the world add on top.

```text
                                                       +-- the enduring body: Full Flask -- Steady -- Hard Frame -- BEARING
                                                       |
  Health I -- Stamina I -- Fourth Flask -- Health II --+
                                                       |
                                                       +-- the light body: Deep Lungs -- Quick Breath -- Long Step -- BEARING
```

| # | Mark | Kind | Memories | What it does |
|---|---|---|---|---|
| 1 | Health I | Number | 2 | Health 100 to 110 |
| 2 | Stamina I | Number | 2 | Stamina 100 to 110 |
| 3 | Fourth Flask | Number | 2 | Flasks 3 to 4 |
| 4 | Health II | Number | 2 | Health 110 to 120 |
| 5 | Full Flask | Number | 2 | *Enduring.* A flask heals 45 to 55 |
| 6 | Steady | Rule | 2 | *Enduring.* +6 poise in every swing of every weapon; outside a swing, a blow of 6 or less does not flinch you |
| 7 | Hard Frame | Number | 2 | *Enduring.* Health 120 to 135 |
| 8 | **The Enduring Body** | Bearing | 4 | *Enduring.* An unguarded blow flinches you 0.2 s, not 0.35, and pushes you half as far; grey health starts to fade after 4 s, not 2.5 |
| 9 | Deep Lungs | Number | 2 | *Light.* Stamina 110 to 125 |
| 10 | Quick Breath | Number | 2 | *Light.* Stamina starts to return after 0.6 s, not 0.8 |
| 11 | Long Step | Number | 2 | *Light.* Every weapon's dodge 0.6 m longer at the same cost (stacks with Wind's Long Stride; kept because the owner named dodge distance) |
| 12 | **The Light Body** | Bearing | 4 | *Light.* A dodge pressed in the last 0.1 s of a dodge flows into a second at half cost (never a third), and the recovery after a dodge is 0.1 s shorter |

Checked against Wind's tree: Empty Air (a perfect dodge gives back its stamina) and Thin Air (the dodge is safe
longer) are not repeated; the Light Body's bearing is a double step instead.

**Found and given pieces (Proposal)**: about one in each land and one from each god, as Sekiro's prayer beads are:
a vessel (+5 health), a breath stone (+5 stamina) or a flask shard (three make a flask). They add to the branch and
are never needed. Gods' gifts of this kind come with either choice over the body.

---

## 1. Katana

Built: drawing cut, return cut, stepping cut (10, 11, 18); overhead (26, 34); the quicker chain overhead (30, 40); Iaido
Slash (48, 50); a sheathed finisher (30, tripled). Default guard and dodge. **Carrier**: the drawing cut. **God's
bough**: Narukami's Storm Katana. **Technique**: Before the Thunder, taught by the Last Retainer (now named in
`WORLD.md`; the move is unchanged).

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Rising Draw | Move 2 | Roll, then R1: a low rising cut out of the roll (12, 150 degrees, 2.4 m), the chain's first cut. [clip: new `katana_roll_cut`] |
| Tsuka-ate | Move 2 | Roll, then R2: the hilt driven straight into the face. 6 damage, 45 stagger; a raised guard is broken 1 s. The pommel strikes of the old sword schools. [clip: new] |
| Settled Hands | Number 1 | The three lights recover 10% sooner |
| Kozuka | Move 2 | Guard held, R1: the small knife from the scabbard's pocket flicked at the faced enemy within 8 m. 4 damage, and a light enemy flinches. Once every 6 s. [clip: new, a flick from the hip; the knife flies as a shot, E8] |
| Zanshin | Rule 2 | For 1 s after a kill you turn to the nearest other enemy, and a guard raised in that second has a perfect window 0.05 s wider. Zanshin: the mind that stays on guard after the cut. [fx: a thin white glance line] |

### The Sheath (iai): kept

The four parts of a drawn cut in Musō Jikiden Eishin-ryū: the draw, the finishing cut, shaking off the blood, the
return to the sheath. **Opening** (as drafted): the sword goes home after every chain or 0.6 s idle; the first blow of
every chain, the draw, deals 1.5 times with +10 stagger; the chain is the draw, the return cut, the sheathe.

| Stem | Kind | What it does |
|---|---|---|
| Sharp Draw | Number 1 | The draw's 1.5 becomes 1.7 |
| Quick Sheath | Move 2 | Guard pressed in a cut's recovery sheathes in 0.15 s; the next R1 is a draw. [clip: new, a fast noto] |
| Chiburi | Move 2 | R1 just after a kill: one flick of the blade that clears the recovery and sheathes; the next draw +20 stagger. [clip: new] |

**Fork option 1 (Recommended: one keystone is a single perfect draw, the other a flurry of draws, the owner's own
example of two ways to play one form)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Held Draw | Move 2 | Hold R1 0.4 s: you sink and creep at a quarter speed. Let go: a draw down a lane 5 m by 1 m that carries you 3 m. 28 damage, 36 stagger. A thin white line on the floor shows the lane while you hold. [clip: new `katana_held_draw`, crouch and release] |
| A | Saya-biki | Rule 2 | Held 1 s or more, the held draw's lane is 7 m and it cuts every enemy on it, not only the first. The scabbard hand pulls back to free the blade's full length |
| A | **The Cut Lands at the Click** | Keystone 4 | The held draw can be held to 2 s; the white line brightens as it fills. Released full: 3 times the draw, through any guard, 100 stagger, and you are already sheathing. Every enemy on the lane stands still 0.4 s, and the wounds open only when the guard clicks home: the damage, the stagger and one hard white seam down the lane arrive together. Struck while holding, the draw is lost. [clip: new, the slow noto with a held pose] |
| B | Nukitsuke Aside | Move 2 | R1 with the stick held to one side: the draw steps 1.5 m that way as it cuts (10, 160 degrees), off the line of the enemy's blow, as the iai forms step aside. [clip: new, one clip mirrored] |
| B | Light Scabbard | Rule 2 | A draw that lands sheathes at once when its recovery ends (Quick Sheath for nothing), and draws within 1 s of each other cost 3 less stamina |
| B | **A Hundred Days at the Shrine** | Keystone 4 | R1 is always a draw: each cut is drawn and sheathed in one stroke (0.42 s a draw), without end, the draw's bonus at 1.3 times. Every fourth draw in a row with no blow taken leaves its white crescent hanging in the air 0.6 s, cutting again for 40% whatever walks into it. The overhead stays on R2. [clip: new, a looping draw-sheathe] |

- **A, The Cut Lands at the Click**: the one draw of the screen duel, where the enemy falls a beat after the sword is
  home. *From*: nukitsuke and noto, the draw and the return; the standoff of the samurai film (from memory).
- **B, A Hundred Days at the Shrine**: a sword that never seems to be out, a crescent on every press. *From*:
  Hayashizaki Jinsuke, iai's founder, said to have been given the draw after a hundred days' prayer at a shrine
  (legend, from memory).

**Fork option 2 (the draft's keystone kept)**: path A is Held Draw, Saya-biki and the draft's **The Sword Stays Home**
(every blow from the sheath counts as a heavy: it discharges Static, shatters the Frozen, no guard turns it; every
chain is one draw and a sheathe, 0.7 s). Path B as in option 1. The draft's keystone plays mostly through afflictions
on the enemy, so it shows less from above than the click.

Clips: 6 new (option 1).

### The Stream (flow): kept

Musashi's Water Book: the spirit takes the shape of its vessel. **Opening** (as drafted): five cuts in place of three
(the built three, a turning cut 10 at 200 degrees, a rising cut 14); a dodge in any cut's recovery no longer ends the
chain.

| Stem | Kind | What it does |
|---|---|---|
| Light Wrist | Number 1 | From the third cut on, cuts are 8% quicker |
| River Bend | Move 2 | R2 in the middle of the chain is a quick overhead (26) that does not end it. [clip: new] |
| Stepping Water | Move 2 | R1 anywhere in a dodge: a cut out of the roll (12, 140 degrees) that counts as the chain's next step. Takes the Rising Draw's input while the Stream is active. [clip: reuse `katana_roll_cut`] |

**Fork option 1 (Recommended: one keystone stands and never stops, the other never stands still)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Second Breath | Rule 2 | Each cut from the fourth on that lands gives back 4 stamina |
| A | Unbroken | Rule 2 | A blow of 12 or less taken in the chain does not end it: you flinch in place 0.15 s and the next R1 carries on |
| A | **No Last Cut** | Keystone 4 | After the fifth cut the first follows with no pause; each loop +8%, three loops at most. The chain ends only when a blow over 12 lands, you guard, or you stop pressing. Light blows give half the mana. At the end of the third loop all five arcs are cut at once as one ring round you (16, 30 stagger). [clip: new, the ring cut] |
| B | Ebb Step | Rule 2 | A dodge inside the chain is a short slide of 2.4 m for 15 stamina |
| B | Eddy | Rule 2 | A Stepping Water cut made out of a perfect dodge is critical |
| B | **The Water Has No Shape** | Keystone 4 | Every dodge in the chain cuts along its own line as you slide (10, 30 stagger to all you pass beside, never through bodies) and counts as the chain's next step: a chain can be all slides. Each slide leaves a pale wake on the floor for 1 s. [clip: new, a sliding cut] |

- **A, No Last Cut**: the river that does not stop; hold the ground and keep cutting. *From*: the flowing water of the
  sword schools; the ring of five arcs is ours.
- **B, The Water Has No Shape**: cut while moving, never where they aim. *From*: Musashi, *The Book of Five Rings*,
  the Water scroll. Checked against Through the Crowd: the slide passes beside bodies, not through them.

**Fork option 2**: path A as in option 1; path B becomes **Tsubame-gaeshi** (marks: Return Wrist, Rule 2, the return
cut 0.06 s quicker; Drying Pole, Rule 2, the chain's arcs 0.4 m longer. Keystone: a cut that misses or is guarded is
reversed at once along the same arc, 0.1 s later, for the same damage, drawn as a swallow's forked tail of two
crescents). *From*: Sasaki Kojirō's swallow cut, said to cut a swallow in flight (legend, from memory). Flashier, but
it plays as a safety net rather than a second way to move.

Clips: 3 new (option 1).

### The Answer (counter): kept

**Opening** (as drafted): a perfect guard opens a 0.6 s answer, shown as a white flick down the blade. R1 in it,
**Riposte** (24, critical); R2, **Through the Gap**, a step to the attacker's back (30, 40 stagger). Light blows deal
10% less.

| Stem | Kind | What it does |
|---|---|---|
| Turned Edge | Number 1 | Perfect guards throw 25% more stagger |
| Open Window | Rule 2 | The answer lasts 0.8 s, not 0.6 |
| Patient Blade | Rule 2 | Each 0.5 s the guard is held unstruck, the next answer +10 stagger, to +40 |

**Fork option 1 (Recommended: one keystone answers everything as it comes, the other saves itself for the red blow,
which is where bosses are beaten)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Guard Cut | Move 2 | Guard held, R1: a short cut (8) with the guard still raised; it does not chain. Takes Kozuka's input while the Answer is active. [clip: new] |
| A | Second Answer | Rule 2 | The answer stays open for a second perfect guard; a second answer in one window +30 stagger |
| A | **Kiri-otoshi** | Keystone 4 | The answer comes by itself: every perfect guard is followed at once by the Riposte, or by Through the Gap if R2 is pressed within 0.3 s. The perfect window is 0.03 s wider; the light chain is two cuts. The Riposte's line runs on 4 m past the attacker as a white seam, striking whoever stands behind it for half. [clip: reuse the Riposte] |
| B | Catch the Red | Rule 2 | After a perfect guard on a red attack the answer lasts 1.0 s |
| B | Read the Red | Rule 2 | Your perfect window against red attacks is 0.04 s wider |
| B | **The First Sword** | Keystone 4 | A perfect guard on a red attack stops time for 0.3 s (the camera leans in), and the answer's R1 becomes one cut through the attacker: 3 times the Riposte, 120 stagger, gods included, ending behind it. Perfect guards on other blows answer as the opening only. [clip: new, a long passing cut] |

- **A, Kiri-otoshi**: every blow they throw is the blow that cuts them. *From*: Ittō-ryū's kiri-otoshi, the one cut
  that meets the enemy's cut on its line, turns it and strikes (from memory). The seam is ours. Checked against Only
  the Draw: that Heresy removes the guard; this needs it.
- **B, The First Sword**: the duel's one decisive answer, kept for the blow that matters. *From*: ichi no tachi, the
  first and deciding cut of the old schools (from memory); the stopped moment is ours.

**Fork option 2**: path A as in option 1; path B becomes **Every Blow Answered** (marks: Bank the Answer, Rule 2, each
perfect guard stores its answer as a flick on the blade, three at most; Turn and Answer, Rule 2, a perfect guard
against a blow from behind turns you to it. Keystone: R1 spends every stored answer in one turning cut, a ring of 3 m
that strikes each stored attacker as a Riposte, the flicks flying from the blade to them). *From*: Musashi alone
against the Yoshioka school at the Ichijō-ji pine (legend, from memory). It is a crowd answer, which the Stream and
the god's thunder already cover.

Clips: 2 new (option 1).

### Narukami's Storm Katana (Lightning; god's bough)

**Opening** (as drafted): his own sword, longer by 0.3 m, storm-grey with a crawling scar of lightning down the
fuller and the shrine's paper streamers at the hilt; +15% damage and stagger and the Thunder Draw skill, as built. The
chain is his: across, back, and down (the third a downward cut, 22, in a 3 m lane). Everything here looks like a
storm: jagged bolts with a white core and a cyan rim, the thunderclap after the flash, fern figures scorched into the
floor.

| Stem | Kind | What it does |
|---|---|---|
| Long Blade | Number 1 | Reach +0.2 m more |
| Flank Step | Move 2 | Dodge, then R1 within 0.2 s: you step through a line of lightning to the faced enemy's side (up to 6 m) and cut as you arrive (18). [clip: new] |
| Seven Drums | Rule 2 | Each cut that lands beats one of seven drums drawn round the guard; at the seventh the next draw +40 stagger. A nod to the ring of drums the thunder god carries |
| The Red Draw | Move 2 | Hold R2 (the Katana has no charged heavy): sheathe, sink, hold 0.6 s, then go through along a 10 m lane to stand behind. 40 damage, 50 stagger, untouchable on the lane, 40 mana. His red draw, given. [clip: new] |
| The Falling Cut | Rule 2 | His third cut's lane +1 m, and a light enemy it strikes is driven to one knee for 0.6 s |

**Fork option 1 (Recommended: two god moves with their own clips, one aimed down a lane, one all round you)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Thunder Follows | Rule 2 | (The draft's keystone, now a path mark.) Every draw leaves its lane on the floor; 0.5 s later thunder runs down it and strikes all on it again for half the draw and 30 stagger: the flash, then the sound |
| A | Long Thunder | Rule 2 | Raikiri's lane +4 m, and it bends up to 20 degrees toward the faced enemy |
| A | **Raikiri, the Cut Through Lightning** | Keystone 4 | Hold Triangle past the Thunder Draw (1.2 s, 50 mana): you raise the sword overhead and a bolt comes down onto the blade (0.6 s; a column of cyan light on you is the tell). Then you cut down a 12 m lane with the bolt carried on the edge: 60, 80 stagger, the lane scorched for 2 s. [clip: new `katana_raikiri`, the raise and the down cut] |
| B | Drumbeat | Rule 2 | Perfect guards and finishers each beat two drums |
| B | Rolling Drums | Rule 2 | Beaten drums no longer fade out of a fight; seven beaten wait until spent |
| B | **The Drum Ring** | Keystone 4 | With seven drums beaten, the Thunder Draw becomes the ring: seven drums stand in a ring 4 m across round you and a bolt strikes each in turn round the ring over 0.7 s (14 to all within 1.5 m of a drum), the last bolt coming down on you and out down the draw's lane (40). Each drum flashes before its bolt, so the ring reads as a clock. [clip: new, a drummer's stance between the beats] |

- **A, Raikiri**: call the bolt down and cut with it. *How it looks*: the streamers stand up in the updraught, the
  bolt lands on the raised blade with a white flash and a thunderclap ring on the floor, and the cut throws a branching
  bolt down the lane. *From*: Tachibana Dōsetsu's sword Raikiri, said to have cut a bolt of lightning (legend, from
  memory).
- **B, The Drum Ring**: the thunder god's drums round you, beaten by the sky. *How it looks*: seven taiko-shaped rings
  of cloud round you, each burst by a jagged bolt in turn, a drumbeat for each. *From*: Raijin's ring of drums, as
  the folding screens show him (the land's first thunder god, `WORLD.md`).

Checked against Stored in the Sheath (the Katana and Lightning standout): that is the infused draw down one lane with
charge; Raikiri is the skill held, from overhead, and works with any element carried.

**Fork option 2**: path B as in option 1; path A keeps the draft: **Thunder Follows** as the keystone (the thunder
runs down every draw's lane), with Long Thunder replaced by Quick Thunder (Rule 2: the thunder comes 0.3 s after the
draw, not 0.5). One god move and one passive: less to build, less to see.

Clips: 4 new (option 1).

---

## 2. Massive Sword

Built: three sweeps with poise (24, 26, 34), the buried overhead (44, 56), the rising cut (46, 60), the leaping charged
overhead (62, 80), Elemental Spike Line, The Headsman. Guard 240 degrees, perfect window 0.15 s; dodge 3.9 m for 30.
**Carrier**: the rising cut. **God's bough**: Brannoch's Standing Stone. **Technique**: The Return Blow, taught by the
Bargain.

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Roll and Rise | Move 2 | Roll, then R1: the blade dragged up out of the roll in a 160 degree rising sweep (22, poise 12). [clip: new `gsword_roll_cut`] |
| Shoulder Into Them | Move 2 | Roll, then R2: the shoulder and the flat driven into the enemy. 8 damage, 45 stagger, shoves 1.5 m; a raised guard or shield is broken 1 s. [clip: new] |
| Carried Weight | Number 1 | The three sweeps recover 10% sooner |
| Parierhaken | Move 2 | Guard held, R1: the side-hooks above the grip catch the faced enemy's weapon within 3 m: its next attack is held back 0.5 s (4 damage). The parrying hooks of the two-handed sword. [clip: new] |
| Brace | Rule 2 | A perfect guard plants the blade: the next swing within 1 s has poise 30 |

### The Doppelsöldner (offered in place of the Wall)

**Why replace the Wall**: the Wall's keystone (Stand Like Stone: no blow breaks the planted guard) is the Anvil's
keystone again (The Anvil Does Not Move), and the Massive Sword has no form for its built strength, crowds. The
**Doppelsöldner** was the Landsknecht paid double to fight in the front rank with the two-handed sword, said to have
cut a way into the pike block (the pike-breaking is debated; from memory). The Wall stays available inside option 2.

**Opening**: every sweep carries you 0.6 m forward, and a sweep that strikes a shield or a raised guard breaks it for
0.6 s.

| Stem | Kind | What it does |
|---|---|---|
| Long Grip | Number 1 | Sweeps reach 0.3 m further |
| Through the Shafts | Rule 2 | A sweep that strikes an enemy carrying a spear, pole or shield knocks its weapon aside: its next attack is 0.4 s late |
| Full Turn Forward | Move 2 | Hold R1 at the third sweep: the full circle turns twice, carrying you 2 m forward (18 each, poise 16). [clip: new] |

**Fork option 1 (Recommended: one keystone carves a road through a crowd, the other is one blow that splits the
floor; the crowd and the giant)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Shoulder the Gap | Rule 2 | Enemies a sweep strikes are pushed 1.2 m to the side it ends, out of your path |
| A | Double Pay | Rule 2 | Each enemy struck by one sweep after the first gives back 4 stamina |
| A | **The Forlorn Hope** | Keystone 4 | While sweeps follow each other (each begun within 0.3 s of the last's recovery), each carries you 1.2 m, no blow under 20 flinches you, and blows from the front are halved. After the sixth in a row the last is a two-turn whirl that throws everything within 4 m out 3 m. [clip: new, the whirl] |
| B | Hold the Ricasso | Rule 2 | While charging the heavy you walk at 60%, not 25%, a hand on the blade above the grip |
| B | Shatter | Rule 2 | A charged heavy on a guarding or shielded enemy breaks its guard for 2 s, +40 stagger |
| B | **Durendal Cleaves the Mountain** | Keystone 4 | The charged heavy has a second tier (hold 1.2 s): the blade is swung twice overhead and brought down a 7 m lane that splits the floor (90, 120 stagger, through guards); everything within 1.5 m either side is thrown aside 2 m. The cleft stays 4 s as a trench nothing walks across. [clip: new `gsword_cleave`, the double wind-up] |

- **A, The Forlorn Hope**: walk into a crowd and come out the other side. *From*: the Landsknechts' *verlorene
  Haufen*, the "lost heap" that went first into the breach; the whirl is ours.
- **B, Durendal Cleaves the Mountain**: one blow, a canyon. *From*: Roland's sword Durendal, said to have cut the
  Brèche de Roland in the Pyrenees as he tried to break it (legend, from memory). Checked against Dragged Iron: the
  charge here is time held, not distance walked.

**Fork option 2 (keeps the Wall as a path)**: path A becomes the Wall: **Planted Guard** (Move 2: hold the guard 0.6 s
to plant; you cannot walk, blocked blows cost half, it covers 260 degrees), **Uproot** (Move 2: R2 while planted, the
blade torn out in a rising cut, 34, 50 stagger), and the draft's **Stand Like Stone**. Path B as in option 1. It
repeats the Anvil's keystone (question 2.1).

Clips: 3 new (option 1).

### Half-Sword: kept

From the German longsword's half-swording (the left hand on the blade to thrust) and the murder stroke (the sword
reversed, the cross struck like a hammer). **Opening** (as drafted): R2 while guarding shifts grip (0.3 s). In the
short grip the lights are three thrusts down a lane 2.6 m by 0.8 m (0.18 s wind-ups, 16 each) and the guard is a
narrow parry (160 degrees, window 0.19 s). The long grip is the sword as built.

| Stem | Kind | What it does |
|---|---|---|
| Quick Shift | Number 1 | Grip change 0.3 s to 0.15 s |
| Murder Stroke | Move 2 | Short grip, R2: the sword reversed, held by the blade, the cross brought down like a hammer. 30, 70 stagger, through any guard. [clip: new] |
| Between the Plates | Rule 2 | Short thrusts on a guarding enemy go through at half |

**Fork option 1 (Recommended: one keystone weaves the two grips into one chain, the other commits to the reversed
sword as a hammer)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Pommel Third | Move 2 | The third short thrust is a pommel blow that knocks the enemy back 1 m (12, 30 stagger). [clip: new] |
| A | Find the Gap | Rule 2 | Short thrusts on a staggered or knocked-down enemy are critical |
| A | **Two Swords in One** | Keystone 4 | (Draft.) A grip change in the middle of a chain no longer ends it: the next blow is the other grip's next step, and each change in one chain adds 10% to the blows after it, three at most. Each change flashes the hand sliding along the blade |
| B | Mordschlag Swing | Move 2 | Long grip, R2 after a light: the sword flipped mid-chain into the murder stroke without a grip change (0.1 s). [clip: new] |
| B | Ring the Helm | Rule 2 | A murder stroke from the front leaves the enemy dazed 1 s: it walks at half and starts no attack |
| B | **The Cross Comes Down** | Keystone 4 | Short grip, hold R2 0.5 s: the reversed sword swung overhead three times like a war hammer, each a disc of 2 m (24, 50 stagger, through guards), the third breaking the floor into a crater of 2.5 m that slows for 3 s. [clip: new, three overhead blows] |

- **A, Two Swords in One**: one weapon, two answers, changed mid-swing. *From*: Liechtenauer's armoured fighting,
  where the half-sword and the long grip trade places in one exchange (from memory).
- **B, The Cross Comes Down**: the murder stroke made a hammer's dance. *From*: the *Mordschlag* of the German fencing
  books; the three-blow peak is ours.

**Fork option 2**: path B as in option 1; path A becomes **The Gaps in the Plate** (marks: Pommel Third; Find the Gap.
Keystone: each short thrust that lands marks one of four gaps on the enemy, armpit, visor, groin and hand, drawn as
four white ticks; at four the next thrust goes through all of them at once, 60, critical, 80 stagger). *From*: thrusts
to the gaps of the harness in the armoured fighting of Ringeck and Lew (from memory). It is a counter like the
Rattle's nicks, so option 1 keeps the Half-Sword's own idea, the grip change, at the top.

Clips: 4 new (option 1).

### The Trade: kept, rooted in the berserkir and Cú Chulainn

**Opening** (as drafted): +10 poise on every swing. A blow taken under poise in a swing's wind-up or swing is a
**trade**: that swing lands +30%, and a red spark runs down the blade to say so.

| Stem | Kind | What it does |
|---|---|---|
| Blow for Blow | Number 1 | A traded swing +30% to +45% |
| Hard Head | Rule 2 | The grey health from a traded blow is not wiped by it |
| Into the Teeth | Rule 2 | The charged heavy no longer stops when you are struck (poise 30 through the whole charge) |

**Fork option 1 (Recommended: one keystone trades all the time, the other fights hardest at the edge of death)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Headsman's Due | Rule 2 | A finisher makes real the grey health taken in the last 4 s |
| A | Scar Tissue | Rule 2 | Each trade in the last 6 s gives +3 poise, five at most |
| A | **Bear-Shirt** | Keystone 4 | Poise 40 in every swing; damage taken while swinging is halved; every trade fills the striker's stagger by 25; the dodge costs 40 stamina (the draft's Let Them Strike). At five trades in 6 s you roar (0.4 s, a red shout ring 4 m): light enemies in it flinch 0.5 s. [clip: new, the roar] |
| B | Wound Remembered | Rule 2 | The swing after a trade winds up 0.1 s quicker |
| B | Grit | Rule 2 | A trade gives back 10 stamina |
| B | **Tied to the Stone** | Keystone 4 | Below a quarter health no blow but a red one flinches you or knocks you down, and each kill gives back 8 health. You cannot drink a flask while below a quarter (the hand is tied). At the threshold you drive the blade into the floor and lean on it for 0.5 s, the tell that the stand has begun. [clip: new, the lean] |

- **A, Bear-Shirt**: walk into every blow and answer it harder. *From*: the berserkir of the sagas, said to feel no
  wounds (from memory).
- **B, Tied to the Stone**: the last stand, on your feet to the end. *From*: Cú Chulainn, who tied himself to a
  standing stone to die standing (the Ulster Cycle; from memory). It fits the Earth and Life land. Checked against
  Bought Back: nothing here refuses a killing blow.

**Fork option 2**: path B as in option 1; path A becomes **The Unbroken Swing** (keystone: each trade adds 0.3 m reach
and 10 degrees of arc to the chain until it ends, five at most; the blade's red spark grows with each). A plainer
version of path A without the roar.

Clips: 2 new (option 1).

### Brannoch's Standing Stone (Earth; god's bough)

**Opening** (as drafted): the giant's sword, a grey slab of standing stone, lichened, its edge knapped sharp, a grip
bound in hide; +15%. The heavy and the charged heavy plant a stone where the blade lands (1.2 m tall, 8 s, three at
most). Stones are cover and a wall: an enemy driven into one takes +40% and 30 stagger. Everything looks like earth:
grey stone, grit, dust rings, cracks, lichen; nothing glows.

| Stem | Kind | What it does |
|---|---|---|
| Long Standing | Number 1 | Stones stand 12 s, four at most |
| Slam Against | Rule 2 | Light blows shove 1 m toward the nearest stone |
| Seven Strides | Move 2 | The charged heavy becomes up to seven strides, each a quake disc of 2 m (14, 20 stagger); let go to end on the overhead. [clip: new] |
| The Living Wall | Move 2 | The skill lays the blade flat as a wall 4 m across for 3 s that stops enemies and shots; R2 heaves it up in a cut (44). [clip: new] |
| Stone Remembers | Rule 2 | A stone that takes a blow or a shot meant for you cracks; a cracked stone shoved into an enemy bursts (20 to all within 1.5 m) |

**Fork option 1 (Recommended: one god move moves every stone at once, the other throws one)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Raise the Ring | Move 2 | The charged heavy plants three stones in a triangle round where it lands, 3 m out. [fx] |
| A | Dancers' Ground | Rule 2 | Stones moved by the Dance leave furrows that slow enemies by 30% for 3 s |
| A | **The Giant's Dance** | Keystone 4 | Hold R2 past the charged heavy (1.4 s): you drive the sword into the earth and turn its hilt like a capstan. Every standing stone, and three new ones raised round you, turns once round you like dancers, each at its own distance (7 m at most), striking all they pass for 30; they stop where the turn ends. [clip: new `gsword_dance`, the heave on the hilt] |
| B | Toppling | Rule 2 | A heavy struck into a stone topples it onto the line beyond (40, a 3 m lane) |
| B | The Dead Take Root | Rule 2 | An enemy killed by a stone leaves a small stone standing where it fell for 6 s (a stone like any other) |
| B | **The Giant's Throw** | Keystone 4 | R2 within 1.5 m of a stone: the blade slides under it, the stone is heaved up on the flat and flung 12 m in an arc. Its shadow on the floor tells where it falls 0.6 s later: 50, 70 stagger on a disc of 3 m, and it stands again where it lands. [clip: new `gsword_heave`, the scoop and the throw over the shoulder] |

- **A, The Giant's Dance**: the stone circle wakes and turns. *How it looks*: the stones grind round on their bases
  throwing up dust and turf, cracks spiralling out from the sword. *From*: Geoffrey of Monmouth's Giant's Dance,
  Stonehenge carried from Ireland; the land's seat (`WORLD.md`).
- **B, The Giant's Throw**: a standing stone as a thrown weapon. *How it looks*: lichened stone tumbling in the air,
  shedding grit, landing with a ring of dust and a crack in the floor. *From*: the giants of British folklore who
  hurled stones that still stand, and the Cailleach dropping hers (from memory).

Checked against The Uncounted (the Massive Sword and Earth standout, seven stones round an enemy): neither keystone
rings an enemy in.

**Fork option 2**: path B as in option 1; path A makes **Seven Strides** the keystone (each stride plants a stone),
with the stem's Seven Strides replaced by Raise the Ring. Less to build; the Dance is the land's own legend, so
option 1 keeps it.

Clips: 4 new (option 1).

---

## 3. Scythe

Built: haft, middle and tip bands (0.6 near, 1.5 times and 1.6 stagger from 2.5 m); out, back, full turn (13, 13, 20);
the back reap; Crescent Slash; The Wheel. Guard 220 degrees; dodge 4.8 m. **Carrier**: the full turn. **God's bough**:
Carnwyn's Golden Scythe. **Technique**: Over the Shoulder, taught by the Wicker Man.

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Mow Out of the Roll | Move 2 | Roll, then R1: a flat low sweep of 180 degrees out of the roll (14 before the bands). [clip: new `scythe_roll_cut`] |
| Snath Butt | Move 2 | Roll, then R2: the butt of the snath (the long handle) driven into the enemy. 6 damage, 45 stagger, shoves 2 m, out to your tip band; a raised guard is broken 1 s. [clip: new] |
| Long Arms | Number 1 | The back reap recovers 15% sooner |
| Haft Spin | Move 2 | Guard held, R1: the scythe spun in the hands before you for 0.6 s: shots that meet it fall, and all within 1.5 m are struck three times (4 each). [clip: new] |
| Step to the Tip | Rule 2 | A perfect guard steps you back 1 m, so the attacker stands at your tip |

### The Swath: kept, rooted in hand mowing and the kosynierzy

The mower's walk: long, even strokes, a step with each. **Opening** (as drafted): every light steps 0.6 m and the
chain loops out and back while R1 is pressed; the full turn moves to R2 after lights; the tip band is 0.3 m wider.

| Stem | Kind | What it does |
|---|---|---|
| Long Pull | Number 1 | Light arcs 200 to 230 degrees |
| Even Stroke | Rule 2 | Every fifth tip blow in a row is critical; a haft blow or a miss ends the row (was +6% a blow) |
| Windrow | Rule 2 | Enemies struck at the tip are pushed 0.8 m to the side the swing ends, so they lie in a row for the next |

**Fork option 1 (Recommended: one keystone mows a crowd stroke after stroke, the other is a single stroke the width of
the room)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Whetstone | Move 2 | Guard tapped twice: strop the edge 0.5 s; the next five tip blows are critical. [clip: new] |
| A | The Mower's Beat | Rule 2 | A soft tick sounds at each stroke's end; R1 pressed on the tick costs no stamina |
| A | **Mow the Field** | Keystone 4 | (Draft, grown.) While you walk the chain, blows of 15 or less do not flinch you, and every fifth tip blow in a row cuts the whole tip ring round you, laying the grass flat in a circle and leaving a windrow of cut stalks. [clip: new, the ring stroke] |
| B | Wide Stance | Rule 2 | The full turn's tip band reaches 0.5 m further out |
| B | Draw Back | Move 2 | Hold R1 at the full turn: the blade is drawn back over the shoulder 0.3 s for +1 m of reach. [clip: new] |
| B | **The Long Stroke** | Keystone 4 | Hold R2 after lights 0.6 s (the blade drawn far back over the shoulder is the tell): one full turn at 6 m, its tip band from 4.8 m out, 40 at the tip, 70 stagger; nothing nearer than 4.8 m is struck. Every enemy cut stands still 0.3 s as the stroke passes. [clip: new `scythe_great_turn`] |

- **A, Mow the Field**: walk into the crowd as into standing grass. *From*: mowing by hand; the ring stroke is ours.
- **B, The Long Stroke** (renamed from "The Sickle of Kronos" by the owner, 2026-11-19, answer 3.5, so no weapon mark touches the patron of time's story before it is settled): one stroke that crosses the room. *From*: the adamant sickle Kronos was given to cut
  the sky from the earth (Hesiod's *Theogony*). A quiet nod: Kronos is often confused with time, and the player's
  patron is a being of time (question 3.5).

**Fork option 2**: path A as in option 1; path B becomes **the Scythemen** (marks: Straighten the Blade, Move 2, hold R2
0.3 s to turn the blade straight on the snath, a thrust down a 4.4 m lane for 20; Forward, Rule 2, straightened thrusts
step 0.8 m. Keystone **Racławice**: with the blade straight, hold R2 1 s for a running charge 8 m through the first
enemy, the blade turned back to the curve in a full turn at the end, 30 at the tip). *From*: the kosynierzy, the
Polish scythemen who took the Russian guns at Racławice in 1794 (from memory). It is near the Spear's couched charge.

Clips: 4 new (option 1).

### The Hook: kept

From Mair's sickle and scythe plays: reach to the forward leg and pull back. **Opening** (as drafted): R2 after a
light is the **Hamstring** (the blade behind the leg at the tip and pulled; a light enemy is knocked down 1.2 s, an
elite stumbles, a god is turned; 16, 30 stagger). The back reap stays on the plain R2.

| Stem | Kind | What it does |
|---|---|---|
| Long Curve | Number 1 | Hamstring reach +0.6 m |
| Over the Haft | Rule 2 | Every blow on a fallen enemy is a tip blow |
| Reel In | Move 2 | Hold R1: hook a light enemy up to 4 m off and drag it to exactly the tip band. [clip: new] |

**Fork option 1 (Recommended: one keystone holds a crowd at the curve, the other brings down the one enemy too big to
hold)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Catch the Arm | Rule 2 | A perfect guard hooks the attacker's arm: its next attack is 0.5 s late |
| A | Lay Them Down | Rule 2 | A hamstrung light enemy stays down 0.4 s longer for each blow on it, to 2.4 s |
| A | **Nothing Leaves the Curve** | Keystone 4 | (Draft.) An enemy in the tip band that moves away from you is hooked back 1 m, once every 2 s each. Each hook-back leaves a short grey drag mark on the floor. [clip: none; the enemy's stumble clip] |
| B | Big Game | Rule 2 | A Hamstring on an elite adds 40 stagger |
| B | Low Line | Rule 2 | The Hamstring's wind-up is 0.06 s quicker on an enemy that is attacking |
| B | **Fell the Giant** | Keystone 4 | Three Hamstrings on one elite within 6 s bring it down: knocked down 1.5 s, the finisher's window open. On a god the third Hamstring is a stumble of 1 s and its next attack is lost. [clip: new, the two-handed haul on the third] |

- **A, Nothing Leaves the Curve**: the curve as a cage. *From*: the Ankou of Breton folklore, from whose scythe none
  turn away (from memory).
- **B, Fell the Giant**: the curve brings down what stands taller than you. *From*: Mair's leg plays, and over the
  top the hamstringing of Völund the smith in the *Völundarkviða* (from memory). Checked against Over the Shoulder:
  nothing here throws.

**Fork option 2**: path A as in option 1; path B becomes **Reap the Crowd** (keystone: R2 after the full turn is a ring
Hamstring, the hook going round behind every leg in the tip band; light enemies in it fall 1 s). A crowd keystone
where path A already is one.

Clips: 2 new (option 1).

### The Harvest: kept

**Opening** (as drafted): each tip blow binds a sheaf, drawn as stalks tied on the haft, five at most. The heavy spends
them: +12% damage and +10 degrees of arc a sheaf.

| Stem | Kind | What it does |
|---|---|---|
| Good Year | Number 1 | Six sheaves at most |
| Seed Corn | Rule 2 | One sheaf is always kept back after spending |
| Thresh | Move 2 | R2 after the full turn: the haft beaten on the floor and the sheaves thrown out as chaff, a disc of 3 m, 6 damage and 8 stagger a sheaf. [clip: new] |

**Fork option 1 (Recommended: one keystone makes a decoy of the dead, the other a threshing floor of the living)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Corn Dolly | Rule 2 | The straw figure stands 1 s longer for each enemy within 6 m of it, to 5 s |
| A | Harvest Home | Rule 2 | A kill binds a sheaf, as a tip blow does |
| A | **The Last Sheaf** | Keystone 4 | (Draft.) With five sheaves the next kill is the last sheaf: the body is bound standing as a straw figure for 3 s, and every enemy within 6 m turns to strike it. Then it falls apart: 20 a sheaf to all within 3 m. [clip: none for the player; the figure is a prop that is struck] |
| B | Granary | Rule 2 | Eight sheaves at most |
| B | Glean | Move 2 | Hold the flask button 0.4 s with sheaves: spend them for 4 health each (grey made real first). No flask is used. [clip: new, a quick bow over the haft] |
| B | **The Threshing Floor** | Keystone 4 | Thresh becomes three beats of the haft: a ring of chaff runs out from 1 to 4 m over 0.6 s and strikes each enemy once, 6 a sheaf and 8 stagger a sheaf (48 and 64 at eight), and the chaff hangs 2 s over the floor, slowing all in it by half. [clip: new, three beats] |

- **A, The Last Sheaf**: the dead stand up as straw to draw the blows. *From*: the last-sheaf custom of Scotland and
  Ireland, where the last sheaf cut was dressed as the Cailleach, the old woman (from memory). The Cailleach is the
  murdered god under Earth's side of the Celtic land.
- **B, The Threshing Floor**: hoard the harvest, then beat it out over the whole floor. *From*: threshing with the
  flail on a round floor; the goddess's threshing floor of Greek tradition (from memory).

**Fork option 2**: path A as in option 1; path B becomes **Glean the Field** (keystone: Glean takes no time and can be
used while walking; each sheaf spent heals 6). A healer's keystone; less to see from above.

Clips: 3 new (option 1).

### Carnwyn's Golden Scythe (Life; god's bough)

**Opening** (as drafted): a gold blade grown long from the druids' gold sickle, on an oak haft with mistletoe tied
under the blade; +15%. A kill sprouts a healing bloom (8 health, walked over, stands 6 s); the full turn grows a ring of
thicket at the tip that stands 1.5 s and slows what crosses it by half. Everything looks like growth: leaves, thorns,
bark, petals, gold flowers; never light.

| Stem | Kind | What it does |
|---|---|---|
| Deep Root | Number 1 | Blooms heal 12 |
| Harvest Sweep | Move 2 | Hold R2 0.5 s: a 300 degree sweep at 4.2 m that stops at nothing (30 at the tip). [clip: new] |
| Thicket Line | Move 2 | R2 while guarding: thorns rise in a line 4 m across ahead for 3 s; nothing walks through. [clip: new] |
| Cauldron Call | Move 2 | The skill: two antlered beasts of the cauldron's plate run out through the faced enemy and back (three strikes of 10). [clip: new, the call] |
| Green Guard | Rule 2 | A perfect guard sprouts a bloom at your feet |

**Fork option 1 (Recommended: one god move rides out down a lane, the other wakes the ground you have grown)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Hounds of Annwn | Rule 2 | Cauldron Call's beasts are the white hounds with red ears; they hold the faced enemy 1 s when they strike |
| A | The Hunt Feeds | Rule 2 | Every kill during the Wild Hunt sprouts a bloom |
| A | **The Wild Hunt** | Keystone 4 | Hold Triangle 1.2 s (50 mana): you raise a horn and blow it (0.5 s, the tell). The Hunt rides through down a lane 14 m long and 3 m wide where you face for 2 s: every enemy on it is struck three times (12 each) and light enemies are carried to the lane's end. [clip: new `scythe_horn`] |
| B | Every Cut Grows Back | Rule 2 | (The draft's keystone, now a path mark.) A bloom left unpicked for 4 s grows into a thorn stand 2 m across for 4 s that slows by half all in it |
| B | Deep Thorns | Rule 2 | Thorn stands are 3 m across and stand 6 s |
| B | **The Grove Answers** | Keystone 4 | With three or more thorn stands within 8 m, hold R2 1 s: you lift the scythe and sweep it down, and every stand bursts into a great gold flower: each heals you 6 and throws a spray of thorns at the nearest enemy (12 each). [clip: new, the blessing sweep] |

- **A, The Wild Hunt**: the lord's herd and the dead ride out at your horn. *How it looks*: stags of thicket and
  hounds of white bark with red-leaf ears, leaves streaming behind them, hoofprints that flower. *From*: the Wild
  Hunt led by Gwyn ap Nudd in Welsh lore, and the hounds of Annwn (from memory); Carnwyn's name echoes Gwyn's.
- **B, The Grove Answers**: the ground you have grown turns on them. *How it looks*: brown thorn stands split open
  into gold flowers, petals and thorns thrown out together. *From*: the druids' grove (Pliny), and the land's rule
  that wood grows back.

Checked against Mistletoe (the Scythe and Life standout, a sprig on each struck enemy): neither keystone marks enemies.
Checked against Life's tree: the heals come from the bough's own blooms, not from Life's marks.

**Fork option 2**: path A as in option 1; path B keeps the draft: **Every Cut Grows Back** as the keystone, with Deep
Thorns and a new Thorn Snare (Rule 2: an enemy in a thorn stand when it grows is held 1 s). A passive in place of a god
move.

Clips: 5 new (option 1).

---

## 4. Great Spear

Built: two thrusts and a sweep (10, 11, 15); the guard thrust (guard held, R1); the slap; the vault; the couched
charge; Flying Spear; The Standard. Guard 160 degrees; dodge 4.2 m. **Carrier**: the first thrust. **God's bough**:
Khepharu's Sun-Spear. **Technique**: Hold the Gate, taught by the Champion of the Gate.

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Rising Point | Move 2 | Roll, then R1: a thrust out of the roll down a 4 m lane (12). [clip: new `spear_roll_thrust`] |
| Sauroter | Move 2 | Roll, then R2: the butt spike driven into the enemy. 6 damage, 45 stagger; a raised guard or shield is broken 1 s. The Greek spear's "lizard-killer". [clip: new] |
| Quick Withdraw | Number 1 | Thrusts recover 10% sooner |
| Spin the Shaft | Move 2 | Hold R1 at the sweep: the spear spun before you for 1 s; shots that meet it fall, and all within 1.8 m are struck four times (4 each). The guard thrust keeps guard and R1. [clip: new] |
| Out to the Point | Rule 2 | A perfect guard shoves the attacker 1.5 m out, to the point |

### The Long Point: kept

The Chinese spear's three methods: block, seize, thrust (lán, ná, zhā). **Opening** (as drafted): R1 while guarding is
the circle: the point turns, a blow landing in the turn is turned aside as by a perfect guard for half the stagger,
and the thrust follows by itself. The lane +0.6 m.

| Stem | Kind | What it does |
|---|---|---|
| Long Arm | Number 1 | The lane +0.4 m |
| Seize | Move 2 | R2 while guarding: the point presses the enemy's weapon down; its next attack is 0.4 s late and your next thrust within 1 s is critical. [clip: new] |
| King of Weapons | Rule 2 | A thrust that strikes with only its last metre is critical |

**Fork option 1 (Recommended: one keystone keeps everyone at the point, the other blooms into a storm of points)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Keep Them There | Rule 2 | Thrusts push 0.5 m: they hold an enemy at the point |
| A | Give Ground | Move 2 | R1 with the stick held away from the faced enemy: a thrust with a 1 m step back. [clip: new] |
| A | **Nobody Inside** | Keystone 4 | (Draft.) An enemy within 1.4 m when a thrust starts is shoved out to 2 m first, then struck at the point. There is no haft blow left. A pale ring at 1.4 m round you shows the ground nobody keeps. [clip: new, the shove-and-thrust] |
| B | Spear Flowers | Move 2 | Hold R1 after the second thrust: quick thrusts, six a second, 5 each and 4 stamina each, the point drawing circles. [clip: new, a looped thrust] |
| B | Drive | Rule 2 | Each quick thrust that lands pushes 0.2 m |
| B | **The Hundred Birds** | Keystone 4 | After 1 s of Spear Flowers the point's after-images show as a fan 30 degrees wide and every thrust strikes the whole fan; let go and the last is one long thrust down 7 m (30, through all). [clip: reuse Spear Flowers; fx: the fan of points] |

- **A, Nobody Inside**: the spear that holds a whole field at its length. *From*: the qiang's spacing, and over the
  top Zhao Yun riding alone through Cao Cao's army at Changban (*Romance of the Three Kingdoms*; from memory).
- **B, The Hundred Birds**: a storm of points from one spear. *From*: "a hundred birds facing the phoenix", a spear
  form named in Chinese spear lore (from memory; check before any in-game use).

**Fork option 2**: path B as in option 1; path A becomes **Changban** (keystone: the couched charge can be steered and
made three times running without a pause, each through the first enemy it meets). Near the built charge, so less new.

Clips: 4 new (option 1).

### The Pole: kept, rooted in pole jumping

The spear as a vaulting pole, as the Frisian canal jumpers use one (fierljeppen). **Opening** (as drafted): a dodge
toward an enemy within 4 m is a vault, over it to land behind (3.5 m, untouchable in the air); R1 in the air is a
plunging thrust (20, 40 stagger). The draft's Pole Spin is dropped: swinging round the planted spear with kicks is
Hold the Gate, the technique.

| Stem | Kind | What it does |
|---|---|---|
| Light Vault | Number 1 | The vault 20 to 15 stamina (renamed from Light Feet, which is a Wind mark) |
| High Slap | Move 2 | R2 in the air: the slap from above, 30, on a disc of 2 m where you land. [clip: new] |
| From Above | Rule 2 | Blows from the air go through guards |

**Fork option 1 (Recommended: one keystone never lands, the other reaches anywhere)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Kick Off | Move 2 | R1 at the top of a vault: a kick down into the enemy below, shoving it 2 m (8). [clip: new] |
| A | Land Soft | Rule 2 | Landing from a vault, you are untouchable 0.1 s more |
| A | **Never Touch the Ground** | Keystone 4 | (Draft, grown.) A plunging thrust that lands lets you vault again at once off the enemy's back for no stamina, three times running, each +15%; the third lands as a quake of 3 m (24). [clip: reuse the vault and plunge] |
| B | Long Vault | Rule 2 | The vault reaches enemies up to 6 m off and carries 5 m |
| B | Pole Sweep | Move 2 | R1 on landing: the pole swept round at 3 m, 200 degrees (14). [clip: new] |
| B | **As Long as You Like** | Keystone 4 | Hold High Slap 0.3 s in the air: the shaft grows to 9 m as it falls, and the slap comes down a lane 9 m long (50, 70 stagger); after it the spear shrinks back. Out of the air, the lights' lane is 1 m longer. [clip: new; the shaft is scaled, no new model] |

- **A, Never Touch the Ground**: from back to back, never on the floor. *From*: the bull-leapers of the Knossos
  fresco, over the beast and down (from memory).
- **B, As Long as You Like**: a pole that grows to the length you need. *From*: Sun Wukong's staff Ruyi Jingu Bang,
  which grows and shrinks at a word (*Journey to the West*).

**Fork option 2**: path B as in option 1; path A becomes **Bull-Leaper** (keystone: a vault over an enemy that is
charging or attacking counts as a perfect dodge, and elites can be vaulted). A defensive peak where option 1's is an
attacking one.

Clips: 4 new (option 1).

### The Cross: kept

Hōzōin-ryū's cross spear: thrust as a spear, cleave as a halberd, slash as a sickle. **Opening** (as drafted): every
thrust cuts going in and coming back (the crossbar hooks, 40%, pulling 0.5 m); the sweep becomes a cleave of 200
degrees.

| Stem | Kind | What it does |
|---|---|---|
| Wide Bar | Number 1 | Lane width 0.9 to 1.2 m |
| Hook Back | Rule 2 | The return cut on a guarding enemy drags its guard aside: broken 0.8 s |
| Crescent Moon | Move 2 | R2 after the cleave: a rising cut with the crossbar (36) that throws light enemies up. [clip: new] |

**Fork option 1 (Recommended: one keystone works one enemy with all three uses, the other sweeps a crowd in on the
crossbar)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Catch the Blade | Rule 2 | A perfect guard catches the attacker's weapon in the crossbar, held 0.6 s; the next thrust is critical |
| A | Turn the Cross | Rule 2 | The return hook pulls 1 m, not 0.5 |
| A | **Thrust, Cleave, Slash** | Keystone 4 | (Draft, grown.) The lights are always thrust, cleave, slash. A chain that strikes one enemy with all three ends with a free fourth, the cross turned in the wound: 24, 60 stagger, it is held 0.5 s, and a cross-shaped seam opens on the floor under it. [clip: new, the turn] |
| B | Two Hooks | Rule 2 | The return hook catches up to two enemies on the lane |
| B | Reel the Lane | Rule 2 | Hooked enemies are pulled to the middle of the lane, in a line |
| B | **The Moon in the Pond** | Keystone 4 | After three return hooks in one chain, the cleave becomes a full turn with the crossbar leading: everything within 4 m is dragged to 2.5 m and cut (24), and a pale crescent ripples out across the floor from the turn. [clip: new, the dragging turn] |

- **A, Thrust, Cleave, Slash**: the three uses on one body, then the turn. *From*: Hōzōin-ryū's teaching of the
  cross spear's three uses.
- **B, The Moon in the Pond**: the crescent that gathers the field. *From*: the legend that Hōzōin In'ei found the
  cross spear when he thrust at the crescent moon's reflection in a pond (from memory).

**Fork option 2**: path A as in option 1; path B becomes **Catch Every Blade** (keystone: with the guard held, every
blow on it is caught in the crossbar 0.4 s). Near the Twin Knives' Take Their Blade.

Clips: 3 new (option 1).

### Khepharu's Sun-Spear (Fire; god's bough)

**Opening** (as drafted): his spear, 3.2 m, a gold shaft, a winged gold disc below a leaf blade that burns along both
edges and trails smoke; +15%; the skill is the Sun Throw as built (thrown, it stands, then comes home as a burning ray
2.2 m wide, 26). Everything looks like fire: flame along the edges, embers, smoke, heat shimmer, gold; never white
light.

| Stem | Kind | What it does |
|---|---|---|
| Hot Leaf | Number 1 | The ray 2.2 to 2.6 m wide |
| The King's Reach | Move 2 | R2 at an enemy 3.5 to 7 m off: one long lane of 7 m with a 1.8 m lunge (24). [clip: new] |
| The King's Thrusts | Rule 2 | The first two lights are his long thrusts: lanes 5.5 m |
| Plant the Noon | Move 2 | Hold R2 past the charged heavy (1.2 s): the spear is planted and a ring of fire 4 m round it rises (18, then 6 every 0.5 s on contact, 5 s). R2 pulls it free in a sweep. [clip: new] |
| Falcon's Stoop | Move 2 | R2 in the air after the vault: a dive down onto a disc of 2 m (26), wings of flame flaring from the disc on the shaft; the falcon of his godly form. [clip: new] |

**Fork option 1 (Recommended: one god move fights bare-handed while the spear is away, the other sends the spear round
the enemy like the sun's disc)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Long Throw | Rule 2 | The thrown spear stands 2 s longer before it comes home |
| A | Hands of Noon | Rule 2 | While the spear is out, a blow blocked with the bare forearms lets no chip through |
| A | **Bare-Handed King** | Keystone 4 | (Draft, given its own clips.) While the thrown spear stands, R1 and R2 are the king's fists: a jab, a cross and a burning backhand, +40%, flame wreathing the forearms. Every enemy struck by them takes the ray's full damage again when the spear comes home. Hold R2 to call it home early. [clip: new `spear_king_backhand`; the jab and cross reuse the fists'] |
| B | Ring of the Disc | Rule 2 | The disc's loop leaves its ring burning on the floor 2 s |
| B | Wide Wings | Rule 2 | The loop is 5.5 m across, not 4 |
| B | **The Winged Disc** | Keystone 4 | The Sun Throw flies out 11 m, circles the faced enemy once in a ring of fire 4 m across (18 to all it passes), and comes home as the ray. [clip: new, a throw with a spin on it] |

- **A, Bare-Handed King**: the king does not need his spear to burn you. *How it looks*: the forearms in flame to the
  elbow, embers off every blow, the spear standing smoking where it fell. *From*: the sun-king's pride (Fire's land).
- **B, The Winged Disc**: the spear as the sun's disc, circling. *How it looks*: the spear spinning flat, the gold
  disc's wings flaring with flame, a ring of fire drawn round the enemy as it passes, then the ray home. *From*: the
  winged sun disc of Egyptian temples, Horus of Behdet (from memory).

Checked against The Obelisk (the Spear and Fire standout: a planted spear and a line of fire turning round it): the
disc circles the enemy, thrown, and lays no line from a planted spear.

**Fork option 2**: path A as in option 1; path B becomes **The Ring of Noon** (keystone: Plant the Noon's ring grows
0.5 m a second while it stands, to 7 m). It sits close to The Obelisk, so option 1 is safer.

Clips: 5 new (option 1).

---

## 5. Great Hammer

Built: side swing and drop (20, 26); the quake (36, 70); the lift (34, 64); Earthshaker (52, 100, a disc 6 m across);
Spinning Hammer Toss; The Bell; the late perfect guard (0.08 to 0.24 s). Dodge 3.6 m for 32. **Carrier**: the drop.
**God's bough**: the Smith's Hammer. **Technique**: The Smith's Tap, taught by the golden handmaiden.

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Roll and Drop | Move 2 | Roll, then R1: a short drop out of the roll onto a disc of 1.2 m (18). [clip: new `hammer_roll_drop`] |
| Haft Shove | Move 2 | Roll, then R2: the haft driven across the chest. 6 damage, 45 stagger, shoves 1.5 m; a raised guard or shield is broken 1 s. [clip: new] |
| Lighter in the Hand | Number 1 | The side swing recovers 12% sooner |
| Butt Spike | Move 2 | Guard held, R1: the haft's end jabbed over the guard (6, 30 stagger), the guard still up. Replaces the Anvil's draft Hammer and Tongs. [clip: new] |
| Raised Early | Rule 2 | A blocked blow costs 20% less stamina if the guard was raised 0.3 s or more before it: the hammer rewards guarding early |

### The Quake: kept, rooted in pile driving

Three blows of the great wooden beetle on one stake. **Opening** (as drafted): drops land where you aim; the stick puts
the disc up to 3 m off.

| Stem | Kind | What it does |
|---|---|---|
| Wide Head | Number 1 | Discs +0.3 m |
| Ground Shiver | Rule 2 | Enemies within 4 m of a disc but not in it stumble: their next attack is 0.2 s late |
| Stake Driver | Rule 2 | Each drop within 1 m of the last +15 stagger, three at most (was +15% damage) |

**Fork option 1 (Recommended: one keystone digs one spot into a pit, the other walks a quake across the room)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Ground Up | Move 2 | R2 after a drop: the hammer heaved up out of the floor under the enemy struck, throwing it 2 m into the air. [clip: new] |
| A | Fault Line | Rule 2 | Two drops within 2 s and 3 m are joined by a crack on the floor for 3 s; a light enemy that crosses it trips (0.6 s) |
| A | **The Floor Gives Way** | Keystone 4 | (Draft.) The third drop on one spot breaks the floor: a crater of 4 m for 6 s. What is in it at the break is knocked down and walks at half speed while inside. You do not. [fx: the crater] |
| B | Shockwave | Rule 2 | Earthshaker's outer metre knocks light enemies down 0.8 s |
| B | Brace Through | Rule 2 | Earthshaker's poise 30 to 45 |
| B | **The Earth-Shaker Walks** | Keystone 4 | Hold R2 after Earthshaker lands: three more blows stamping forward, each a disc of 3 m set 3 m on from the last, 0.45 s apart (30, 50 stagger). Each disc shows a ring of dust 0.2 s before the head lands. [clip: new `hammer_walking_quake`] |

- **A, The Floor Gives Way**: three blows on one stake, and the ground goes. *From*: pile driving with the beetle;
  over the top, Thor's blows on the giants' halls (the Eddas; from memory).
- **B, The Earth-Shaker Walks**: the quake that walks toward them. *From*: Poseidon's Homeric name *Ennosigaios*, the
  earth-shaker.

**Fork option 2**: path A as in option 1; path B becomes **Strike Where They Will Be** (keystone: drops may be aimed up
to 5 m off, and an enemy that walks into a disc in its last 0.2 s is struck as a critical). Less to see than the walk.

Clips: 2 new (option 1).

### The Swing: kept

The Highland hammer thrower's turns. **Opening** (as drafted): hold R1 after the side swing and you turn with the
hammer at arm's length, a ring of 2.6 m, each turn quicker than the last (0.6, 0.45, 0.35 s, then held), 12 plus 4 for
each turn before it, 8 stamina a turn, steering at 40%. Let go to stop.

| Stem | Kind | What it does |
|---|---|---|
| Wide Shoulders | Number 1 | The ring 2.6 to 3.0 m |
| Carry the Weight | Rule 2 | Poise 22 while turning |
| Low Turn | Rule 2 | From the third turn, the turns knock light enemies down |

**Fork option 1 (Recommended: one keystone never stops turning, the other lets go and calls the hammer back)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Steer | Rule 2 | You walk at 70% while turning, not 40% |
| A | Hammer's Wake | Rule 2 | From the fourth turn the ring leaves a trail of grit that slows enemies by 30% for 1 s |
| A | **Never Let It Stop** | Keystone 4 | (Draft, grown.) After the third turn the turns cost no stamina and you walk at 80%; it ends only when R1 is let go or a red blow lands. You cannot guard while turning. At the sixth turn the ring grows to 4 m and the grit rises round you as a dust devil. [clip: reuse the turn; fx] |
| B | Let It Fly | Move 2 | R2 while turning: let go. The hammer flies 8 m and comes back, as the toss at 60%, for no mana. [clip: new] |
| B | Catch It Coming Home | Rule 2 | R1 pressed within 0.2 s of the hammer's return catches it at once, and the next swing +40 stagger |
| B | **It Always Comes Back** | Keystone 4 | A thrown hammer (Let It Fly or the skill) that strikes flies on to the next nearest enemy within 6 m, up to three, before it comes home: 24 and 40 stagger each. While it is away you cannot attack, and you guard with bare hands. [clip: new, the catch] |

- **A, Never Let It Stop**: the turn that becomes a storm of grit. *From*: the hammer throw of the Highland games
  (from memory); the dust devil is ours.
- **B, It Always Comes Back**: the hammer that seeks and returns. *From*: Mjölnir, made by dwarf smiths, which comes
  back to Thor's hand (the Prose Edda). Checked against Called Home (a Bow Heresy): that recalls every arrow at once;
  this is one thrown weapon finding three.

**Fork option 2**: path A as in option 1; path B becomes **Weight for Distance** (keystone: Let It Fly goes 14 m in an
arc and lands as a disc of 3 m, 40; you walk to it and pick it up). One long throw instead of three bounces.

Clips: 2 new (option 1).

### The Anvil: kept

**Opening** (as drafted): the late perfect guard rings the hammer's head like an anvil; all within 2.5 m stumble
0.3 s, and R2 within 0.5 s is **Strike While It Rings** (44, 80 stagger, nothing interrupts it).

| Stem | Kind | What it does |
|---|---|---|
| Ring True | Number 1 | The ring 2.5 to 3.5 m |
| Counterweight | Rule 2 | Each blow blocked stores a weight, three at most; the next heavy +30 stagger a weight (was +25% damage and +30 stagger) |
| Hard Face | Rule 2 | Chip through the guard 20% to 10% |

**Fork option 1 (Recommended: one keystone is the anvil nothing moves, the other the anvil that calls the giant)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Long Ring | Rule 2 | Strike While It Rings may be pressed up to 0.8 s after the ring |
| A | Heavy Weights | Rule 2 | Counterweight stores five |
| A | **The Anvil Does Not Move** | Keystone 4 | (Draft.) The guard cannot be broken: at no stamina it holds and costs health instead, 1 for every 4 blocked. Every perfect guard also stores a weight. Each ring shows as a visible ripple of sound on the floor. [fx] |
| B | Ring Out | Rule 2 | The ring's stumble 0.3 to 0.6 s |
| B | Ring Twice | Rule 2 | Strike While It Rings rings again where it lands (2.5 m) |
| B | **Three Strokes on the Anvil** | Keystone 4 | With three weights stored, hold R2 0.5 s: you strike the floor as an anvil three times in 0.9 s. Each stroke is a ring running out to 3, 4.5 and 6 m (20 each, each ring striking an enemy once); the third knocks light enemies down and every shot in the air within it falls. [clip: new `hammer_three_strokes`] |

- **A, The Anvil Does Not Move**: stand, take everything, ring. *From*: the anvils of the Cyclopes, the first smiths
  (the Metal land's murdered god, `WORLD.md`).
- **B, Three Strokes on the Anvil**: the ringing that carries across the island. *From*: the smith's three strokes on
  his anvil that call the bronze giant (the Metal god's fight, `WORLD.md`).

**Fork option 2**: path A as in option 1; path B makes the draft's true form the keystone: **Strike the Whole Ring**
(Strike While It Rings strikes everyone in the ring). One new keystone fewer to animate.

Clips: 1 new (option 1).

### The Smith's Hammer (Metal; god's bough)

**Opening** (as drafted): the lame smith's hammer, a short haft, a head too big for it, forge-black with a molten seam
that drips; +15%. **Heat** 0 to 100 (+10 a blow that lands, +20 a heavy; it falls 5 a second after 3 s without a blow).
At 100 it is **hot**: blows +60% that cannot be guarded, the head pouring sparks and molten drops, and the first heavy
struck hot spends the heat. The skill is **Rivets** (seven red-hot rivets 8 m out, 8 each, that pop 4 s later for 6).
Everything looks like the forge: molten bronze, orange sparks, black scale, gold ichor; heat shimmer, never light.

| Stem | Kind | What it does |
|---|---|---|
| Slow to Cool | Number 1 | Heat falls 3 a second, not 5 |
| Crutch Step | Move 2 | The dodge is a swing of your weight round the planted hammer, 3.6 m in an arc, ending with it raised. [clip: new] |
| Forged in the Fight | Move 2 | With 50 heat or more, R2 while guarding: strike the floor as an anvil; a blade forged there flies at the faced enemy (20) and costs 30 heat. [clip: new] |
| Golden Plate | Move 2 | The finisher leaves a gold plate 2 m across on the floor for 6 s: an enemy that steps on it is held 1 s. [fx] |
| Bellows | Rule 2 | A perfect guard while hot keeps the heat from falling for 3 s |

**Fork option 1 (Recommended: one god move forges a new weapon in the fight, the other drives home the nail)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Hot Iron | Rule 2 | (The draft's keystone, now a path mark.) A heavy no longer spends the heat: it stays hot until you are struck, and then falls to 50 |
| A | Bronze Breaks | Rule 2 | When the forged blade's time ends it bursts into molten shards: 20 to all within 2.5 m |
| A | **The Smith's Gift** | Keystone 4 | At 100 heat, hold R2 1 s: three strokes on the floor as on an anvil, and you raise a blade forged there, glowing orange and cooling to bronze. For 8 s your lights are the Massive Sword's sweeps with the forged blade (3.4 m), every blow +10 heat and, while hot, unguardable; then it breaks. [clip: new `hammer_forge`; the sweeps reuse the Massive Sword's] |
| B | More Rivets | Rule 2 | Ten rivets, popping for 8 |
| B | Red Rivet | Rule 2 | Each blow on an enemy with a rivet in it adds 10 heat |
| B | **Drive the Nail** | Keystone 4 | R2 at an enemy with a rivet in it, within 2.5 m: one tap and one drive (two beats in 0.4 s), and the rivet is driven home: 60, gold ichor spraying from the wound. A light or middle enemy is staggered at once; an elite takes 80 stagger, a god 40. [clip: new `hammer_drive`] |

- **A, The Smith's Gift**: he forges a new weapon mid-fight and uses it until it breaks. *How it looks*: sparks fly
  from three ringing strokes, a blade lifted white-orange out of the floor, cooling as it swings, bursting into molten
  drops at the end. *From*: the Metal god's fight (`WORLD.md`), and Hephaestus the maker of the gods' arms.
- **B, Drive the Nail**: find the nail and strike it home. *How it looks*: the rivet glows white-hot as you face it,
  the tap rings, the drive bursts gold ichor. *From*: Talos, whose one vein was closed with a bronze nail at the ankle
  (Apollonius' *Argonautica*; from memory).

Checked against The Smith's Tap (the technique: tap a spot and the heavy follows) and Quench (the Hammer and Metal
standout): Drive the Nail is also two beats; it needs a rivet laid first and spends it, so it plays as a payoff, not
an aim (question 5.4).

**Fork option 2**: path A as in option 1; path B becomes **Talos's Embrace** (keystone: while hot, R2 on a light or
middle enemy within 2 m seizes it against the glowing haft and crushes it, 40, then throws it 3 m; an elite is burned
and shoved). Near the Fists' Hold.

Clips: 4 new (option 1).

---

## 6. Twin Knives

Built: five cuts in a rattle (5, 5, 5 and 5, 7, 12), half again from behind; the passing spin; the lunge; the crossing
step; the cut out of the sidestep; Fan of Knives; Three Crossings. Parry guard (window 0.21 s); dodge a sidestep of
3.4 m for 16. **Carrier**: the crossing step. **God's bough**: the Alchemist's Hooks. **Technique**: Fillet, taught by
the blade brother.

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Out of the Spin | Move 2 | The built cut out of the sidestep now counts as the rattle's second cut and deals 12, not 9. [clip: reuse] |
| Knee and Elbow | Move 2 | Sidestep, then R2: a knee to the body and an elbow down (4 and 6), 45 stagger; a raised guard is broken 1 s. [clip: new] |
| Light Recovery | Number 1 | The five cuts recover 10% sooner |
| Throw a Knife | Move 2 | Guard held, R1: the right knife thrown at the faced enemy within 9 m (8; 1.5 times from behind). For 1 s the left fights alone (lights 0.7 times) while a spare is drawn from the boot. [clip: new] |
| Parry Breath | Rule 2 | A perfect guard gives back 16 stamina (Wind's Empty Air does this for the perfect dodge, not the guard) |

### The Shadow: kept, rooted in Baguazhang's circle walking

The deer-horn knives of Baguazhang, fought while walking a circle round the opponent. **Opening** (as drafted): a
perfect dodge through a blow puts you behind the attacker (up to 2 m), and the next blow within 1 s counts as from
behind wherever it lands.

| Stem | Kind | What it does |
|---|---|---|
| Low Shadow | Number 1 | The sidestep 16 to 12 stamina |
| Lost Sight | Rule 2 | After a crossing (the fifth cut, the crossing step) the enemy turns the wrong way for 0.5 s |
| Behind the Knee | Move 2 | R2 from behind: a low cut behind the knee (14, 30 stagger) that drops a light enemy to one knee. [clip: new] |

**Fork option 1 (Recommended: one keystone stays behind one enemy, the other runs from back to back through a crowd)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Circle Walk | Move 2 | Hold the guard and move round a faced enemy within 3 m: you walk the circle at full speed, the guard up, always facing it. [clip: new, a sideways walk] |
| A | Wear Them Down | Rule 2 | Blows from behind fill the stagger meter 20% more |
| A | **They Never See You** | Keystone 4 | (Draft, grown.) Every crossing leaves the enemy unable to turn to you for 1 s, and a red seam shows across its back for that second; blows from the front deal 30% less. [fx] |
| B | Back to Back | Rule 2 | A crossing that ends within 3 m of another enemy turns you to face that one's back |
| B | Next One | Move 2 | R2 within 0.4 s of a kill: a crossing step to the nearest enemy within 5 m, ending behind it. [clip: reuse the crossing step] |
| B | **Eight Changes** | Keystone 4 | R2 again within 0.4 s of a crossing's end crosses the next nearest enemy within 5 m: eight crossings at most in a row, each 0.1 s quicker. Your path draws an eight-sided figure on the floor that bursts when the run ends (6 to all inside it). [fx: the figure] |

- **A, They Never See You**: one enemy, and you are always behind it. *From*: Baguazhang's circle walking (from
  memory); the seam is ours.
- **B, Eight Changes**: a crowd crossed one back at a time. *From*: the eight trigrams that give Baguazhang its name
  and its "eight palm changes". Checked against Through the Crowd: each crossing is one step through one enemy, as the
  built crossing step; you do not walk through bodies.

**Fork option 2**: path B as in option 1; path A becomes **The Shadow Stays** (keystone: each crossing leaves a dark
after-image where it began for 0.8 s, and the enemy turns to it). A decoy, but close to Dark's shadow work, so option 1
avoids it.

Clips: 2 new (option 1).

### The Rattle: kept, rooted in Filipino kali

Kali's *sinawali* ("woven") double-weapon drills and its "defanging the snake" (cutting the attacking hand), and the
centipede of the five venoms, whose hands come too fast to count. **Opening** (as drafted): hold R1 after the fifth cut
and the rattle goes on, 8 cuts a second (4 damage, 3 stagger, 3 stamina each). Each cut on one enemy leaves a nick, a
white tick; at 12 the nicks open: 30 damage, and they clear.

| Stem | Kind | What it does |
|---|---|---|
| Quicker Hands | Number 1 | 8 to 10 cuts a second |
| Open Wound | Rule 2 | The nicks open at 10 |
| Light Breath | Rule 2 | Rattle cuts on a staggered enemy cost no stamina |

**Fork option 1 (Recommended: one keystone builds one enemy up to a single great opening, the other weaves through a
crowd)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Last Cut | Move 2 | Let go after six or more rattle cuts: a closing X with both knives, 16 and 2 a nick. [clip: new] |
| A | Defang the Snake | Rule 2 | Rattle cuts on an enemy winding up an attack add 0.05 s to its wind-up each, to 0.3 s |
| A | **The Hundred Cuts** | Keystone 4 | The nicks no longer open by themselves: they count to 100 on one enemy, drawn as a white lattice over it. R2 opens them all at once: 2 a nick and 1 stagger a nick, critical. [clip: reuse Last Cut] |
| B | Sinawali | Rule 2 | You can walk at 60% while rattling, and each cut strikes every enemy within 1.6 m |
| B | Keep the Weave | Rule 2 | Rattle cuts cost 1 stamina while two or more enemies are within reach |
| B | **The Woven Storm** | Keystone 4 | Hold R1 2 s into the rattle: it becomes a walking whirl of cuts 2.2 m round you, twelve a second shared among all in it (3 each, 2 stamina each). Each enemy's nicks open at 10. The blades' trails draw a lattice in the air round you. [clip: new, the whirl] |

- **A, The Hundred Cuts**: a slow build on one body, opened at a word. *From*: the centipede of the five venoms (the
  film *Five Deadly Venoms*, and the five poisons of the Poison land).
- **B, The Woven Storm**: the weave turned outward. *From*: kali's sinawali.

**Fork option 2**: path B as in option 1; path A keeps the draft keystone, **A Hundred Cuts** as a passive (each nick
+1% to every blow on that enemy). It is close to a plain damage mark, so option 1 makes it an opening instead.

Clips: 2 new (option 1).

### The Reverse Grip: kept, rooted in Wing Chun and the pakal grip

Wing Chun's butterfly knives, where the guard and the strike are one movement, and the reverse (*pakal*) grip of the
Filipino arts. **Opening** (as drafted): both knives reversed; blocking costs double, the perfect window is 0.26 s,
and every perfect guard slips you to the attacker's side with a cut (8).

| Stem | Kind | What it does |
|---|---|---|
| Fine Edge | Number 1 | The perfect window +0.02 s |
| Cut the Wrist | Rule 2 | A perfect guard makes the attacker's next attack 0.6 s late |
| No Wall | Rule 2 | You walk at full speed while guarding |

**Fork option 1 (Recommended: one keystone takes their weapon, the other turns the guard into a walking counter)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Two Answers | Move 2 | R1 within 0.4 s of a perfect guard: the X from the side, two blows of 8, as from behind. [clip: new] |
| A | Wrist Lock | Rule 2 | A perfect guard on the last blow of an enemy's string holds it 0.5 s |
| A | **Take Their Blade** | Keystone 4 | (Draft, grown.) Three perfect guards on one enemy within 6 s disarm it for 3 s: it can only shove and grab, and its weapon stands in the floor beside you. R2 within those 3 s throws the weapon back at it (30). A god's next attack is broken instead. [clip: new, the throw back] |
| B | Block and Cut | Rule 2 | Every blocked blow, not only a perfect one, cuts back once (5) if the attacker is within 1.6 m |
| B | Sticky Hands | Rule 2 | After a perfect guard you stay at the attacker's side, moving with it, for 1 s |
| B | **Guard and Strike as One** | Keystone 4 | Hold the guard: you walk at full speed with the knives crossed, and every blow that meets the guard is turned and answered at once with a cut (8, as from behind), as many as come. A red blow still breaks it. [clip: new, the crossed-knife walk] |

- **A, Take Their Blade**: leave them empty-handed. *From*: Yagyū Shinkage-ryū's *mutō-dori*, taking the sword with
  empty hands (from memory).
- **B, Guard and Strike as One**: never a guard without a cut. *From*: Wing Chun's simultaneous defence and attack
  (from memory). Near the Katana's Kiri-otoshi, which answers perfect guards only; this answers every block for a
  small cut (question 6.3).

**Fork option 2**: path A as in option 1; path B becomes **Chi Sao** (keystone: Sticky Hands lasts 3 s, and while you
stick, the enemy's attacks are all 0.2 s late). A quieter version.

Clips: 3 new (option 1).

### The Alchemist's Hooks (Poison; god's bough)

**Opening** (as drafted): the knives become his hook swords, blued steel with green lacquer, crescent guards and
pommel points, a needle case on the left forearm; +15%; the hook swords' moveset (four lights to 2.3 m, the leg hook,
the linked whip at 4.2 m, Needles as the skill, Both Hooks as the finisher); a needle thrown every fourth light;
pressure points on the third blow in 3 s. Everything looks like poison and the court: sickly green vapour, black
lacquer, beads of mercury; never light.

| Stem | Kind | What it does |
|---|---|---|
| Long Needles | Number 1 | Needles stand 12 s, not 8 |
| Needle Grid | Rule 2 | An enemy passing between two needles less than 3 m apart is pricked (4) and slowed by 30% for 1 s |
| Sun and Moon | Move 2 | Hold R2 after the linked whip: the pair stays linked until the chain ends, and every swing strikes near and far at once (1.5 m and 4.2 m). [clip: new] |
| Hooked Twice | Rule 2 | An enemy caught by both hooks within a second is thrown flat 2 s, and blows on it land as on the staggered |
| Gather the Needles | Move 2 | R2 while guarding: every standing needle flies to the faced enemy (2 each). [clip: new] |

**Fork option 1 (Recommended: one god move drinks all five poisons at once, the other whirls the joined hooks as one
great weapon)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Five Vials | Move 2 | (Draft.) Hold the flask button 0.6 s: drink a vial (three a rest, not flasks) and take the next stance in turn for 12 s: centipede (whips +1 m), snake (lights become thrusts), toad (blows taken -25%), scorpion (heavies leave a needle that pricks each second), gecko (dodge +30%, needles never prick you). [clip: new, the drink] |
| A | Strong Stomach | Rule 2 | Five vials a rest |
| A | **The Last Elixir** | Keystone 4 | Hold the flask button 1.2 s with two or more vials left: drink them all. For 8 s you fight with all five stances at once. Your skin takes a green-black sheen and the five creatures flicker in the vapour off you: centipede legs along the arms, a scorpion's tail of mist. [clip: new, the long drink and a shudder] |
| B | Long Link | Rule 2 | The linked whip reaches 0.5 m further |
| B | Mercury Drops | Rule 2 | Linked strikes shed beads of mercury where they pass, which slow enemies by 30% for 2 s |
| B | **Heaven and Earth** | Keystone 4 | Hold R2 after the linked whip: the joined pair is whirled overhead in a great circle for 1.5 s, three turns at 4.2 m (12 each) that drag what they strike in onto the crescents at 1.5 m (8 each), then throw everything out 4 m. [clip: new `knife_hooks_whirl`] |

- **A, The Last Elixir**: all five poisons in one body, as he drank them. *How it looks*: green vapour boiling off the
  skin, the five creatures' shapes in it, the empty vials dropped ringing on the floor. *From*: the alchemist's five
  vials and the gu jar (the Poison god's fight, `WORLD.md`).
- **B, Heaven and Earth**: the hook swords joined into one long weapon and swung as the sky turns. *How it looks*:
  arcs of green-tinged steel, beads of mercury flung off the edge. *From*: the hook sword's old name, "Heaven and
  Earth, Sun and Moon sword" (from memory).

Checked against The Last One Alive (the Knives and Poison standout, a sealed jar of mist): neither keystone seals a
ring.

**Fork option 2**: path B as in option 1; path A keeps the draft keystone **Forever** (stances no longer end; each vial
adds one, five at most; a blow taken loses the oldest). A passive in place of a god move.

Clips: 5 new (option 1).

---

## 7. Great Bow

Built: arrows planted in bodies and the ground; the swat and the arrow stab that snap them; the snap shot (18 to 9 m);
the kick shot; the full draw (34, 44, a 14 m line); Landmines; Nailed. **Carrier**: the snap shot. **God's bough**:
the Huntress's Bow. **Technique**: Only the Eye, taught by the Teacher.

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Roll and Loose | Move 2 | Roll, then R1: a snap shot from the crouch at the roll's end, no draw (14). [clip: new `bow_roll_shot`] |
| Stave Shove | Move 2 | Roll, then R2: the lower limb thrust into the chest. 4 damage, 45 stagger, shoves 1.5 m, snaps every arrow in it; a raised guard is broken 1 s. [clip: new] |
| Quick Nock | Number 1 | The snap shot recovers 12% sooner |
| Hand-Thrown Arrow | Move 2 | Guard held, R1: an arrow thrown by hand 6 m (8); it stands where it lands like any arrow. [clip: new] |
| Kyūdō Breath | Rule 2 | Standing still 0.5 s before a full draw shortens the draw 0.6 to 0.45 s. From the eight stages of the Japanese draw |

### The Stave: kept, made the bow-spear

**Why change it**: the Stave was the bow fought close with no tradition behind it. The **yumi-yari** was a blade
lashed to the bow's upper tip so an archer could fight as a spearman when the enemy closed (from memory). It gives the
Stave's close fight a real weapon. **Opening**: the draft's three stave lights (the swat, the arrow stab, a stave
thrust that snaps), with the thrust made by the lashed blade (14, a 2.6 m lane); a shot loosed within 3 m deals 1.5
times and shoves 1.5 m.

| Stem | Kind | What it does |
|---|---|---|
| Heavy Stave | Number 1 | Stave blows +20% stagger |
| Bow and Stave | Rule 2 | Each stave blow that lands nocks an arrow: the next shot within 2 s needs no draw (0.08 s) |
| Limb Hook | Move 2 | R2 after the thrust: the upper tip hooks a leg; a light enemy is knocked down 1 s. [clip: new] |

**Fork option 1 (Recommended: one keystone shoots point-blank, the other fights as a spear that shoots)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Through the Guard | Rule 2 | Point-blank shots go through a guard at 60% |
| A | Muzzle to Chest | Rule 2 | A point-blank shot on a staggered enemy snaps every arrow in it |
| A | **No Range** | Keystone 4 | (Draft, grown.) Shots beyond 6 m deal half. Within 3 m every shot is a heavy blow (it discharges, it shatters), staggers twice as hard, and throws the enemy 3 m into whatever stands behind (12 to that). [clip: new, the braced point-blank loose] |
| B | Bow-Spear Lunge | Move 2 | R2 after the swat: a lunge with the bladed tip down a 3.4 m lane (18). [clip: new] |
| B | Long Limb | Rule 2 | Stave blows reach 0.4 m further |
| B | **The Spear That Shoots** | Keystone 4 | Every stave thrust that lands looses an arrow through the enemy at the same instant, the string drawn by the thrust itself: a line 9 m on behind it (14, through all), the arrow standing where it ends. [clip: reuse the thrust; fx] |

- **A, No Range**: an archer who wants you close. *From*: the archer's close fight; the throw is ours.
- **B, The Spear That Shoots**: blade and bow in one stroke. *From*: the yumi-yari; the loosing thrust is ours.

**Fork option 2**: path A as in option 1; path B becomes **Bow and Blade** (keystone: R1 and R2 alternate stave and
shot with no recovery between them while each lands). A faster version with nothing new to see.

Clips: 3 new (option 1).

### The Snare: kept

Snares in the hunting forest of the Ice land, laid on the player there first. **Opening** (as drafted): two arrows
standing within 5 m of each other are joined by a bowstring drawn on the floor; an enemy that crosses it is tripped (a
light enemy knocked down 1 s, a heavy one stumbles), and that line is gone.

| Stem | Kind | What it does |
|---|---|---|
| Spread the Snares | Number 1 | Ground arrows 6 to 9 |
| Peg Shot | Move 2 | Hold R2 0.3 s aimed at the floor: two arrows at once, 4 m apart across the facing: a snare in one shot. [clip: new] |
| Trip and Snap | Rule 2 | A tripped enemy's arrows all snap at once |

**Fork option 1 (Recommended: one keystone binds what the web catches, the other makes every line a loaded bow)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Drive Them In | Rule 2 | Stave blows shove 1.5 m toward the nearest line |
| A | Taut Line | Rule 2 | Lines reach 7 m |
| A | **Gleipnir** | Keystone 4 | Lines no longer break; each enemy can be tripped once every 4 s; an enemy tripped twice within 4 s is bound by the lines for 1.5 s, the strings wrapping round it. [fx: the wrap] |
| B | Spring Pole | Rule 2 | A tripped light enemy is flung 3 m along the line instead of knocked down |
| B | Rearm | Rule 2 | A broken line forms again after 3 s while both its arrows stand |
| B | **The Trapper's Ground** | Keystone 4 | A crossed line snaps like a bowstring and looses both its arrows into whoever crossed it (14 each); they stand again where they were 3 s later. [fx] |

- **A, Gleipnir**: a web that holds the wolf. *From*: the fetter Gleipnir, made by the dwarves from impossible things,
  that bound the wolf Fenrir (the Prose Edda). The Ice land is Norse and its god becomes a wolf.
- **B, The Trapper's Ground**: every line a trap that shoots. *From*: the spring-pole snare and the set bow of
  trappers (from memory).

**Fork option 2**: path B as in option 1; path A keeps the draft keystone **The Hunting Ground** (lines no longer
break; each enemy tripped once every 4 s) without the bind.

Clips: 1 new (option 1).

### The Long Draw: kept

From Drona's test of the princes: only Arjuna saw nothing but the bird's eye. **Opening** (as drafted): the full draw
can be held as long as you like, walking at 40%; each second past the charge +20% (to +80%), and the arrow pins the
first enemy it strikes (held 1.2 s).

| Stem | Kind | What it does |
|---|---|---|
| Steady Hands | Number 1 | Walking while drawing 40% to 60% |
| Loose and Step | Move 2 | A dodge within 0.2 s of loosing comes up with an arrow nocked: the next full draw takes 0.3 s. [clip: new] |
| Pinned to the Wall | Rule 2 | A pinned enemy within 1 m of a wall or a stone is held 3 s |

**Fork option 1 (Recommended: one keystone strings a crowd on one arrow, the other reaches across the field)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Through Them All | Rule 2 | The full draw pins the last enemy on its line, not the first |
| A | Hold Your Breath | Rule 2 | While drawing past the charge, a blow of 10 or less does not flinch you |
| A | **Through Twelve Axes** | Keystone 4 | The snap shot is gone; only the full draw is left (the draft's One Arrow). Held to 3 s, the arrow goes through everything on a 16 m line and every enemy on it is pinned where it stands for 1.5 s, strung on the arrow's line. [clip: new, the held peak of the draw] |
| B | Far Sight | Rule 2 | While you draw, the faced enemy beyond 10 m is outlined |
| B | Wind Reading | Rule 2 | The full draw bends up to 5 degrees toward the faced enemy |
| B | **Yoichi's Fan** | Keystone 4 | The full draw's line reaches 18 m, and an arrow that strikes beyond 10 m is critical and staggers twice as hard. A faint ring on the floor at 10 m shows where far begins. [fx: the ring] |

- **A, Through Twelve Axes**: one arrow through a line of them all. *From*: Odysseus' shot through the twelve axe heads
  (*Odyssey*, book 21).
- **B, Yoichi's Fan**: the far shot that cannot miss. *From*: Nasu no Yoichi, who shot the fan from the mast of a
  rocking boat at Yashima (*The Tale of the Heike*). Near Only the Eye (the technique: a critical aimed point); that
  needs the closing ring and a held aim, this only distance.

**Fork option 2**: path B as in option 1; path A keeps the draft keystone **One Arrow** (only the full draw; it deals
twice and snaps every arrow on its line).

Clips: 2 new (option 1).

### The Huntress's Bow (Ice; god's bough)

**Opening** (as drafted): her bow of yew and antler, frost on the limbs and a hunter's tally cut into the grip; +15%;
you move 12% faster, and the dodge is a **skate**: a 6 m glide that leaves a trail of frost for 3 s (gliding along a
trail is half again as quick), during which R2 looses a snap shot without stopping. Everything looks like hard winter:
rime, frost, blue-white ice, breath clouds; never light.

| Stem | Kind | What it does |
|---|---|---|
| Long Glide | Number 1 | The glide 6 to 8 m |
| Shoot Running | Rule 2 | Snap shots no longer stop you |
| Break the Edge | Move 2 | The charged heavy aimed at the floor 6 to 12 m off brings down a fall of ice there 0.6 s later: a disc of 2.5 m, 30. [clip: new] |
| Volley | Move 2 | The skill is a fan of five arrows; one is fletched gold and strikes three times as hard. Her readable volley, turned round. [clip: new] |
| Skate the Loop | Rule 2 | A glide begun on your own frost trail costs no stamina |

**Fork option 1 (Recommended: one god move circles one quarry, the other never stops skating)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | The Quarry | Rule 2 | (The draft's keystone, now a path mark.) The last enemy struck by an arrow is the quarry: glides toward it go half again as far, shots bend up to 10 degrees toward it, and other enemies take 20% less from you while it lives |
| A | Tally | Rule 2 | Each arrow in the quarry cuts a notch on the grip; at five, the next shot pins it 1 s |
| A | **Skaði's Circle** | Keystone 4 | Hold R2 at the quarry within 8 m: you skate a full circle round it 4 m out in 1 s, loosing five arrows inward as you go (16 each, the fifth 40 and 60 stagger), and end where you began. [clip: new `bow_skate_circle`] |
| B | Drive-By | Rule 2 | A glide that passes within 2 m of an enemy looses an arrow at it by itself, once a glide for each enemy |
| B | Wide Fan | Rule 2 | Running snap shots loose three arrows 12 degrees apart |
| B | **Ski and String** | Keystone 4 | Hold the dodge to skate on for up to 3 s at 1.5 times speed, steered by the stick; R2 looses a fan of three without slowing, and each arrow that lands gives back 0.5 s of skate. [clip: new, the skating draw] |

- **A, Skaði's Circle**: the huntress closes the ring on her quarry. *How it looks*: a ring of frost scored on the
  floor at full speed, rime-tipped arrows streaking inward, the fifth trailing ice. *From*: Skaði, the giantess who
  hunts with the bow on skis (the Prose Edda).
- **B, Ski and String**: a hunt that never stops moving. *How it looks*: long frost trails curving across the floor,
  arrows loosed sideways from the glide, breath clouds. *From*: the same, and the Ice god's skating fight
  (`WORLD.md`).

Checked against The Lake Sings (the Bow and Ice standout, a skipping shot) and Ice's Chill: the frost trails carry
you only; nothing here chills an enemy.

**Fork option 2**: path B as in option 1; path A keeps the draft keystone **The Hunt Never Ends** (the quarry, with the
next nearest taking its place when it dies) as a passive.

Clips: 4 new (option 1).

---

## 8. Chain Sickle

Built: the sickle cut, the weight snap (a 5.2 m lane), the hook, the whirl, the thrown sickle, the yank, Entangle (the
Bound state), Unwinding. **Carrier**: the weight snap. **God's bough**: the Fisher's Jawbone. **Technique**: Hook and
Hold, taught by the hook brother.

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Roll and Snap | Move 2 | Roll, then R1: the weight snapped out of the roll down a 5 m lane (10). [clip: new `chain_roll_snap`] |
| Chain Bar | Move 2 | Roll, then R2: the chain held taut between the hands and driven into the chest. 4 damage, 45 stagger, shoves 1.5 m; a raised guard is broken 1 s. [clip: new] |
| Quick Coil | Number 1 | The weight snap recovers 12% sooner |
| Wrap the Weapon | Move 2 | Guard held, R1: the chain flicked round the faced enemy's weapon within 3 m; its next attack is 0.4 s late (4). [clip: new] |
| Taut Guard | Rule 2 | A perfect guard also wraps the attacker's arm: Bound 0.5 s |

### The Weight: kept

The meteor hammer's play: spinning, shooting, retrieving. **Opening** (as drafted): hold R1 and the weight whirls
overhead, a ring that grows from 2 to 5 m over 1.5 s (8 each turn, 0.4 s a turn), walking at half speed. Let go to
throw it at the faced enemy, +4 for each turn, 30 at most.

| Stem | Kind | What it does |
|---|---|---|
| Long Chain | Number 1 | The whirl's ring grows to 5.5 m |
| Figure of Eight | Rule 2 | While whirling you may guard: the turning chain is the guard, 200 degrees, half cost |
| Round the Post | Move 2 | (Draft, from the Bone Hook.) The weight thrown at an anchor within 6 m wraps it, and R2 swings you round it on the chain, a ring of 4 m, 14 to all you pass, untouchable while you swing. Near The Line Carries You (the Water god's Heresy), which yanks you to anchors from 9 m. [clip: new] |

**Fork option 1 (Recommended: one keystone is a ring nobody walks into, the other a meteor that falls where you
point)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Wrap the Arm | Move 2 | R2 while whirling: the chain wraps the forearm and the weight is a fist: three close blows (10, 10, 18). [clip: new] |
| A | Low Whirl | Rule 2 | Hold the guard while whirling and the weight drops to ankle height: light enemies it strikes trip (0.6 s) |
| A | **Always Turning** | Keystone 4 | (Draft, grown.) When you are not attacking, the weight circles by itself at 2.2 m (6 every 0.5 s on touch); after 3 s a second ring joins it at 4 m. The light chain is the sickle only. [clip: new, the idle whirl] |
| B | Release Point | Rule 2 | A throw out of the whirl goes 9 m |
| B | Heavy Head | Rule 2 | A throw after three or more turns knocks a light enemy down 1 s |
| B | **The Shooting Star** | Keystone 4 | Whirl 1.5 s, then let go with the stick at an enemy: the weight goes up out of sight and falls on it 0.6 s later as a meteor; its shadow on the floor is the tell. A disc of 3 m, 40, and what it lands on is knocked down. [clip: new, the overhead release] |

- **A, Always Turning**: a guard that is also a weapon, turning round you. *From*: the meteor hammer's spinning
  play; the second ring is ours.
- **B, The Shooting Star**: the meteor hammer as its name. *From*: the meteor hammer's Chinese name, *liuxing chui*,
  "shooting star hammer". Checked against Over the Wall (the Bow's arcing Heresy): one thrown move, not every shot.

**Fork option 2**: path A as in option 1; path B makes **Round the Post** the keystone (any anchor within 9 m can be
swung round, and you may go round twice). Closer to The Line Carries You.

Clips: 4 new (option 1).

### The Bind: kept, rooted in hojōjutsu

Japanese hojōjutsu, the art of binding a man with cord, and its quick rope (*hayanawa*). **Opening** (as drafted):
Entangle costs 30 to 20, and a Bound light enemy is a weapon: R1 swings it round you on the chain (14 to all within
4 m, 10 to it); R2 hurls it 6 m where you face (20 to it and to what it lands on).

| Stem | Kind | What it does |
|---|---|---|
| Quick Knots | Number 1 | Entangle 20 to 15 mana |
| Held Fast | Rule 2 | Your roll no longer frees what is bound |
| Lead It | Move 2 | Walk while a heavy enemy is bound: it turns with you, its back to you after 1 s. [clip: new, the hauling walk] |

**Fork option 1 (Recommended: one keystone makes one bound body a flail, the other binds everything the weight
touches)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Two on One Chain | Rule 2 | Entangle binds the first two enemies on its lane together and drags them into each other (20 each, 30 stagger) |
| A | Long Hurl | Rule 2 | The hurl goes 9 m, and the body bowls over everything it passes (10) |
| A | **Whirled and Flung** | Keystone 4 | Hold R1 with a bound light or middle enemy: it is swung overhead without end, a ring of 4 m (12 to all each turn, 8 to it), you walking at half; let go to fling it 9 m. [clip: new, the overhead swing] |
| B | Staked Chain | Move 2 | (Draft, from the Bone Hook.) Hold R2 0.3 s at the floor: the weight driven in up to 6 m off, the chain taut from it to your hand for 6 s; walk round the stake and the chain sweeps the floor like a clock hand, tripping what it crosses. R2 tears it free in a snap (12). [clip: new] |
| B | Knot Twice | Rule 2 | Bound enemies take 40% more stagger, not 25% |
| B | **Nothing Gets Away** | Keystone 4 | (Draft.) Every weight blow that lands binds for 0.8 s, for no mana; Entangle itself is gone. Each bind shows the chain knotting itself round the body. [fx] |

- **A, Whirled and Flung**: one enemy as the weapon against the rest. *From*: hojōjutsu's binding, and over the top
  Herakles whirling Lichas round his head and flinging him into the sea (Ovid, *Metamorphoses*; from memory).
- **B, Nothing Gets Away**: everything the chain touches is tied. *From*: the hayanawa, the quick rope. Checked
  against None Go Far: that leashes enemies to you for 10 s; this holds them still for less than a second.

**Fork option 2**: path B as in option 1; path A becomes **Lead the Bound** (keystone: a bound heavy enemy can be led
for 3 s and walked into other enemies, each taking 16). Near None Go Far, so option 1 is safer.

Clips: 3 new (option 1).

### The Sickle: kept, rooted in Isshin-ryū

Isshin-ryū kusarigama: the sickle close, the weight far (from memory); and Mair's sickle plays (turn the blow aside,
take the arm, cut). **Opening** (as drafted): the chain coiled short; the lights are the sickle only, four hooking cuts
(9, 9, 10, 14) within 2.4 m.

| Stem | Kind | What it does |
|---|---|---|
| Sharp Bill | Number 1 | Sickle blows +10% |
| Hook the Guard | Move 2 | R2: the sickle hooks the guard or weapon and pulls: the guard is broken 1 s (16). [clip: new] |
| Free Hand | Rule 2 | A perfect guard turns the blow on the sickle and takes the arm: held 0.6 s |

**Fork option 1 (Recommended: one keystone duels one enemy to the neck, the other fights two distances at once)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Short Chain | Rule 2 | The fourth light is a weight snap of 3 m |
| A | Pull Round | Rule 2 | A guard the sickle breaks turns the enemy half round, its side to you |
| A | **Reap the Neck** | Keystone 4 | (Draft.) Every guard the sickle breaks is followed at once by a cut to the neck (28, critical), drawn as a crescent. This form loses Entangle. [clip: new, the neck cut] |
| B | Weight Hand | Rule 2 | R2 after a sickle light throws the weight 5 m at the next nearest enemy (10) while the sickle keeps the near one |
| B | Long Hand | Rule 2 | Weight throws from the sickle form reach 6 m |
| B | **Near and Far** | Keystone 4 | Every sickle light also snaps the weight at the nearest other enemy within 5 m (6, 10 stagger), the chain paying out and back in the same beat. [clip: new, the two-handed light] |

- **A, Reap the Neck**: break the guard, take the head. *From*: Mair's sickle; over the top Perseus' sickle-sword, the
  harpē, that took the Gorgon's head (from memory).
- **B, Near and Far**: the sickle for the one in front, the weight for the one behind. *From*: the kusarigama's two
  ends.

**Fork option 2**: path A as in option 1; path B becomes **Spin the Short Chain** (keystone: hold R1 to spin the weight
on a 1.2 m chain before you, a shield that strikes 4 a turn and knocks shots down). A defensive version.

Clips: 3 new (option 1).

### The Fisher's Jawbone (Water; god's bough)

**Opening** (as drafted): his hook in place of the sickle, old yellow bone as long as a forearm with the teeth still
set in its curve, lashed with plaited flax to a line that drips; the weight a stone sinker; +15%. **Hooked**: a hooked
enemy can still move, but the line is yours to reel or heave for 3 s. The hook's moveset (three lights, the cast down
12 m, reel, heave, fish up the ground, Spin the Line as the skill, Landed as the finisher). Everything looks like the
sea: seawater, spray, foam, deep blue-green, dripping line; never light.

| Stem | Kind | What it does |
|---|---|---|
| Long Line | Number 1 | The cast 12 to 14 m |
| The Lure | Rule 2 | (Draft.) A cast that strikes nobody leaves the hook lying, its shell flashing, for 4 s: light enemies within 5 m walk to it, and your blows on them count as from behind (1.4 times) |
| Set the Hook | Rule 2 | Enemies stay Hooked 6 s, and a heavy one is held still 1 s as the hook sets |
| The Surge | Move 2 | R2 at a fished-up slab: it is yanked along the line toward you, striking all it passes (24). [clip: new] |
| The Water Mace | Move 2 | The skill: the hook spun overhead while seawater gathers round it in a ball (1.2 s), slammed down 4 m ahead: a disc of 3 m, 40, 60 stagger. [clip: new] |

**Fork option 1 (Recommended: one god move hauls up the land, the other nooses a crowd)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Many Islets | Rule 2 | Fished-up slabs, three at most, become five |
| A | The Swell | Rule 2 | A fished-up slab comes in on a wave that drives enemies before it 2 m |
| A | **Fish Up the Island** | Keystone 4 | Hold R2 at the floor 1 s: you hook the ground and haul over your shoulder like a catch. A swell of seawater runs 6 m ahead with an islet 5 m across on its crest (40, shoves 3 m); when it drains the islet stands 10 s as cover and an anchor. [clip: new `chain_haul`] |
| B | Second Line | Rule 2 | Two lines can be out at once |
| B | Haul Together | Rule 2 | Enemies reeled into each other crash (16 each) |
| B | **Noose the Sun** | Keystone 4 | Hold Triangle 1.2 s (50 mana): five lines cast at once in a fan. Every enemy struck is Hooked 6 s and walks at half speed while hooked, the lines straining; R1 reels them all to your feet, where they crash (16 each, 30 stagger). [clip: new, the fan cast and the haul] |

- **A, Fish Up the Island**: the fisher's own deed, in the fight. *How it looks*: the line goes taut into the floor,
  the sea heaves up in a swell, the islet rides in on its crest streaming water and drops into place. *From*: Maui
  fishing up the North Island with his grandmother's jawbone (the Water god's lore, `WORLD.md`; treated with the
  respect that section asks).
- **B, Noose the Sun**: many lines, everything slowed. *How it looks*: dripping lines fanned out and straining, the
  hooked dragging their feet through foam. *From*: Maui and his brothers noosing the sun with ropes to slow it (from
  memory).

Checked against Taken From Below (the Chain and Water standout, a hook rising under the enemy): Fish Up the Island
raises land ahead of you and hooks nothing.

**Fork option 2**: path A as in option 1; path B keeps the draft keystone **The Hook Remembers** (three lines at once,
the reel drawing all three to one point).

Clips: 4 new (option 1).

---

## 9. Fists

Built: jab, cross, hook, kick (6, 7, 9, 14); the lunging heavy; the rising blow; Breaker; the finisher. **Carrier**: the
kick. **No god carries the Fists** (checked in `WORLD.md`: the gods' table gives every god another weapon, and the last
champion carries his own of every kind with no bough). The Fists' third bough is **the Armament** (Decided in
`weapons.md` 9), which takes the god's bough's place: it is opened once any element is owned and grows with each
element taken. So, as the owner's answer 8 has it, the Fists have three plain boughs and no god's bough; whether the
Armament should be the size of a god's bough is question 9.4. **Techniques**: The Scorpion's Tail (the Undying Emperor)
and Bear Them Down (the pack's alpha), one on the chord at a time.

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Roll Into the Body | Move 2 | Roll, then R1: a hook to the body out of the roll (10, 30 stagger). [clip: new `fist_roll_hook`] |
| Push Kick | Move 2 | Roll, then R2: a push kick to the middle, the teep of Muay Thai. 4 damage, 45 stagger, shoves 2 m; a raised guard is broken 1 s. [clip: new] |
| Snap Back | Number 1 | The four lights recover 10% sooner |
| Elbow | Move 2 | Guard held, R1: a short elbow over the guard (8, 30 stagger), the guard staying up. [clip: new] |
| Roll With It | Rule 2 | An unguarded blow taken while throwing a light flinches you 0.2 s, not 0.35, and pushes you half as far |

### Wrapped Hands: kept

The Greek boxer's leather-wrapped hands and short hooking blows. **Opening** (as drafted): the **slip**, the dodge
tapped toward an incoming blow, a 0.18 s lean of 1.2 m for 10 stamina, safe for 0.12 s, that does not end the chain; a
light after a slip is critical.

| Stem | Kind | What it does |
|---|---|---|
| Hard Wraps | Number 1 | Lights +10% |
| Short Hooks | Rule 2 | Hold R1 at the hook: it repeats, up to four hooks (9 each) |
| Body Blow | Move 2 | R2 after a slip: a blow under the ribs, 22, 60 stagger. [clip: new] |

**Fork option 1 (Recommended: one keystone slips everything and counters, the other never stops punching)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Inside | Rule 2 | Within 1.5 m of the faced enemy, lights cost no stamina |
| A | Counter Cross | Rule 2 | The cross thrown after a slip also adds 40 stagger |
| A | **Stand In** | Keystone 4 | (Draft, grown.) The roll is gone; the slip is the dodge. A slip on the last instant is a perfect dodge and costs nothing. Two perfect slips within a second make the next blow strike twice as hard and throw the enemy 3 m. [clip: reuse the slip and the cross] |
| B | Double Up | Rule 2 | Jab and cross loop while R1 is pressed quickly; the hook and kick come only after a 0.2 s pause |
| B | Pressure | Rule 2 | Each light that lands within 1.5 m pushes the enemy 0.3 m, and you step after it |
| B | **No Rounds** | Keystone 4 | While you land a blow at least every 1.2 s, each light gives back 2 stamina and the kick's recovery is skipped. After twenty blows in a row the next hook is a haymaker that throws the enemy 4 m and adds 60 stagger. [clip: new, the haymaker] |

- **A, Stand In**: never be where the blow lands, and answer it. *From*: Greek boxing; over the top Polydeuces, the
  Argonauts' boxer, beating the giant Amycus by skill against strength (Apollonius' *Argonautica*; from memory).
- **B, No Rounds**: the bout that does not stop. *From*: ancient Greek boxing, fought with no rounds until one man gave
  in (from memory); the haymaker is ours.

**Fork option 2**: path A as in option 1; path B becomes **Body Work** (keystone: blows to the body, the hook and Body
Blow, leave stagger that does not fade for 6 s). Quieter, nothing new to see.

Clips: 2 new (option 1).

### The Hold: kept

Pankration: holds and throws. **Opening** (as drafted): R2 after two lights is a grab (light and middle enemies; a
heavy one is only turned). While holding: R1, knees (three of 6); R2, a throw where the stick points, 4 m. Thrown into
another enemy, both take 20; into a wall, +50%.

| Stem | Kind | What it does |
|---|---|---|
| Strong Grip | Number 1 | A hold lasts 2 to 3 s |
| Hip Throw | Move 2 | R2 at once on the grab: over the hip to land at your feet, 26 and knocked down 1.5 s. [clip: new] |
| On the Ground | Rule 2 | An enemy thrown down takes blows as if staggered (1.5 times) for 1 s |

**Fork option 1 (Recommended: one keystone wrestles the one enemy too big to hold, the other slams bodies into a
crowd)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Neck Hold | Move 2 | Hold R2 while holding: the enemy held before you as a shield for 2 s; blows from the front land on it. [clip: new] |
| A | Into the Wall | Rule 2 | Thrown into a wall or pillar, +100%, not +50%, and knocked down 1.5 s |
| A | **Herakles and Antaeus** | Keystone 4 | Elites can be grabbed and thrown 2 m. Hold R2 while holding an elite: you lift it off the ground for 1 s and crush it (40, 80 stagger). A god can be held 0.5 s and turned, back to you (the draft's No Weight Too Great). [clip: new, the lift and crush] |
| B | Walk With It | Rule 2 | You walk at 80% while holding |
| B | Throw Far | Rule 2 | Throws carry 6 m, and the body strikes everything it crosses (12) |
| B | **Milo's Bull** | Keystone 4 | Hold R2 while holding a light or middle enemy: you lift it overhead (0.4 s) and slam it down, a disc of 3 m (30 to all in it, 30 to it), knocked down 2 s. [clip: new, the overhead slam] |

- **A, Herakles and Antaeus**: lift the giant off the earth and it has no strength. *From*: Herakles holding Antaeus,
  who drew his strength from the ground, up until he was crushed (from memory).
- **B, Milo's Bull**: a body as a hammer. *From*: Milo of Croton, the wrestler said to have carried a grown bull on his
  shoulders (legend, from memory). Checked against the Bind's opening (a bound body swung on the chain): this is a
  lift and one slam.

**Fork option 2**: path A as in option 1; path B becomes **The Throw That Clears the Room** (keystone: Throw Far's body
bowls 9 m and knocks down everything it strikes). One new clip fewer.

Clips: 4 new (option 1).

### The Armament: kept

**Opening** (as drafted): the coat as built (every blow lays one stack; the four enders; Meteor Fist; Energy Reflect;
One Inch). Opened once any element is owned. With two elements carried, the left arm and leg wear the first and the
right the second, so one chain lays both afflictions (the Decided exception for the Armament).

| Stem | Kind | What it does |
|---|---|---|
| Thick Coat | Number 1 | Blocking 0.8 to 0.6 stamina a point |
| Meteor | Move 2 | Meteor Fist from a dodge: R2 within 0.2 s of a roll's end, no charge. Takes the break's input (roll, then R2) while the Armament is active. [clip: reuse Meteor Fist] |
| Reflect Twice | Rule 2 | Energy Reflect holds for two blows; the second is answered at half |

**Fork option 1 (Recommended: one keystone grows the coat by attacking, the other stores every blow taken and gives
it back)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Charged Coat | Rule 2 | A charged blade lasts six blows on the Armament, not three |
| A | Layered | Rule 2 | Only a blow of over 12 strips a layer |
| A | **Golden Bell** | Keystone 4 | (Draft's The Coat Grows.) Each full chain of four thickens the coat a layer, three at most: each layer one more stack on every blow and +8 poise. A blow taken strips a layer. At three layers the substance covers the whole body. [fx: the layers] |
| B | Hold the Stance | Rule 2 | Energy Reflect can be held 2 s longer |
| B | Bank the Blow | Rule 2 | Each blow Energy Reflect answers also stores its damage, five at most |
| B | **Every Blow Returned** | Keystone 4 | Let go of the Reflect stance with blows stored: a straight from no distance at all, down a lane 6 m: 20 plus everything stored, the element thrown off the knuckle as the built One Inch cone. [clip: new, the braced straight] |

- **A, Golden Bell**: the element as a skin that thickens with every chain. *From*: the "golden bell cover" and "iron
  shirt" body-hardening of Chinese martial arts (from memory).
- **B, Every Blow Returned**: take it in, give it all back from an inch away. *From*: the one-inch punch of Wing Chun
  and Jeet Kune Do (from memory). Checked against The Body Is the Spell (the Fists' Heresy): spells stay.

**Fork option 2**: path A as in option 1; path B becomes **Two Coats** (keystone: with two elements carried, every
fourth blow lays both afflictions from both arms at once). Close to the mixed branches' first marks, which already
let blows lay both.

Clips: 1 new (option 1).

---

## 10. War Fan

The plain war fan (the draft's moveset: four lights from rib-end jab to a snap-open cut, the open sweep, close and
strike, Thrown Fan as the skill, The Fan Closes as the finisher) is not built yet; it needs its own set of about a
dozen clips before any bough. **Carrier**: the open sweep. **God's bough**: Kazehaya's Fan. **Technique**: Turn the
Spear, taught by the Hermit of the Pass (now named in `WORLD.md`; the move is unchanged). Two plain boughs and the
god's bough, as drafted (Decided, owner's answer 8).

### Trunk basics

| Mark | Kind | What it does |
|---|---|---|
| Open Out of the Roll | Move 2 | Roll, then R1: the fan snapped open in a 160 degree cut as you come up (12). [clip: new `fan_roll_cut`] |
| Rib Strike | Move 2 | Roll, then R2: the shut fan's rib ends driven into the throat. 4 damage, 45 stagger; a raised guard is broken 1 s. [clip: new] |
| Quick Wrist | Number 1 | The four lights recover 10% sooner |
| Fan the Eyes | Move 2 | Guard held, R1: the fan flicked open before the faced enemy's eyes within 2 m; no damage, and its next attack is 0.3 s late. [clip: new] |
| Read the Wind | Rule 2 | Shots that strike your guard drop at your feet and let no chip through |

### The Iron Fan: kept

The Japanese iron war fan (tessen) with which Yoshitsune is said to have turned Benkei's spear on Gojō Bridge.
**Opening** (as drafted): the fan stays shut, a baton that strikes the hands: four blows on one enemy within 3 s make
its next attack 0.4 s late.

| Stem | Kind | What it does |
|---|---|---|
| Iron Ribs | Number 1 | Blocking 1.1 to 0.8 a point |
| Turn the Blow | Rule 2 | Perfect guards make the attacker stumble past you, its back to you 0.8 s |
| Rap the Knuckles | Move 2 | R1 within 0.3 s of a blocked blow: a rap on the striking hand (6); its next attack 0.3 s late. [clip: new] |

**Fork option 1 (Recommended: one keystone answers every turned blow, the other saves the delays for one great
blow)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Open at Need | Move 2 | R2 snaps the fan open in a 200 degree cut at 2.4 m (16) and shut again. [clip: new] |
| A | Over the Rail | Move 2 | A dodge within 0.3 s of a perfect guard is a leap over the attacker to land at its back. [clip: new] |
| A | **Ushiwaka on the Bridge** | Keystone 4 | (Draft's A Fan Against a Sword.) Every blow your perfect guard turns also strikes the attacker (12) and knocks its weapon aside: for 0.8 s it cannot guard. Each turned blow rings with a clang and a ring of dust at your feet. [fx] |
| B | Three Blows | Rule 2 | The opening's four blows in 3 s become three |
| B | Bruised | Rule 2 | An enemy delayed twice within 4 s walks at 70% for 2 s |
| B | **The Thousandth Sword** | Keystone 4 | Each attack the fan delays cuts a notch on its ribs; at ten the next shut-fan blow is struck like a sword: 60, 80 stagger, and the enemy cannot guard for 1.5 s. [clip: new, the full shut-fan blow] |

- **A, Ushiwaka on the Bridge**: the boy with a fan who beat the giant monk. *From*: the legend of Yoshitsune, as the
  boy Ushiwakamaru, against Benkei on Gojō Bridge (from memory).
- **B, The Thousandth Sword**: Benkei took 999 swords and wanted one more. *From*: the same legend; the notches are
  ours.

**Fork option 2**: path A as in option 1; path B makes the draft's true form the keystone: **Lose the Turn** (the turned
enemy also loses its next attack). Quieter.

Clips: 4 new (option 1).

### The Commander's Fan: kept

The flat signal fan (gunbai) generals carried; Takeda Shingen is said to have stopped Uesugi Kenshin's sword with his
at Kawanakajima. **Opening** (as drafted): a solid leaf; the skill is replaced by the **signal**: R2 held 0.3 s marks
the faced enemy with a red cloth for 6 s, and every enemy your sweeps shove is shoved toward it; driven into it, each
takes 12 and 20 stagger, and so does it.

| Stem | Kind | What it does |
|---|---|---|
| Wide Leaf | Number 1 | The sweep 4 to 5 m |
| Flat Leaf | Rule 2 | The guard covers 220 degrees |
| Signal Again | Move 2 | The signal on an enemy already marked moves the mark to the next, and the first is struck by all pushed past it. [clip: reuse the signal] |

**Fork option 1 (Recommended: one keystone aims every shove, the other commands your own body by the four words of
the banner)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Close Order | Rule 2 | A marked enemy struck by three or more shoved bodies is knocked down 1 s |
| A | Drive Home | Rule 2 | Shoves toward the mark go 1 m further |
| A | **The Field Is Mine** | Keystone 4 | (Draft.) While a mark stands, every enemy you shove lands where the stick points (within 5 m), not straight along the push. Each shoved body trails a red streak of dust to where it lands. [fx] |
| B | Kawanakajima | Rule 2 | A perfect guard on a red blow shoves the attacker 3 m toward the mark |
| B | Long Signal | Rule 2 | The mark lasts 9 s |
| B | **Fūrinkazan** | Keystone 4 | Each signal gives you the next of four stances for 6 s, in turn, the banner's character shown on your back: Wind (you move 30% faster), Forest (enemies more than 6 m off start no attack on you), Fire (shoves harm twice), Mountain (poise 30; nothing under 20 flinches you). [fx: the banner] |

- **A, The Field Is Mine**: the general places every body on the field. *From*: the gunbai's use to signal troops.
- **B, Fūrinkazan**: swift as wind, quiet as forest, fierce as fire, unmoving as the mountain. *From*: Takeda
  Shingen's banner, quoting Sun Tzu (from memory). Near the Alchemist's Five Vials, which also takes stances in turn
  (question 10.2).

**Fork option 2**: path A as in option 1; path B makes the draft's true form the keystone: **Two Marks** (two marks
stand at once and shoves go to the nearer).

Clips: 0 new beyond the fan's base set (option 1).

### Kazehaya's Fan (Wind; god's bough)

**Opening** (as drafted): her war fan, iron ribs and a pale leaf that strains like a sail, the bag's cord knotted at
the pivot; +15%. Every open stroke lets out a gust that shoves 1 m and lays Unsteady once. Hold the dodge: ride a gust,
a glide of 6 m. Everything looks like wind: streaming dust, leaves and cloud wisps, flattened grass, a pale streak
(her racing line in the land); never light.

| Stem | Kind | What it does |
|---|---|---|
| Strong Gust | Number 1 | Gusts shove 1.5 m |
| Held Gale | Move 2 | The skill: the fan held open at the enemy, a gale 7 m by 3 m for 2 s that drives enemies back and knocks shots down. [clip: new] |
| Ride the Wind | Move 2 | R1 during the glide: an open cutting stroke (14) without stopping. [clip: new] |
| Still Air | Rule 2 | While the fan is held open, shots within 2.5 m of you fall |
| Tiger's Stride | Rule 2 | The glide can be turned once on the way, up to 60 degrees, by the stick |

**Fork option 1 (Recommended: one god move races down a line, the other fills the air with cutting leaves)**

| Path | Mark | Kind | What it does |
|---|---|---|---|
| A | Racing Line | Rule 2 | Glides along a wind road are twice as quick |
| A | First to Arrive | Rule 2 | You are untouchable for the first 0.2 s of the Gale Road |
| A | **The Gale Road** | Keystone 4 | Hold R2 at a glide's end: the fan opened behind you as a sail, you ride a straight gale 14 m in 0.5 s; everything within 1 m of your line is thrown aside 3 m (20, 30 stagger), and the line stays 3 s as a wind road of streaming dust. [clip: new `fan_sail`, leaning into the wind] |
| B | Every Stroke a Wind | Rule 2 | (The draft's keystone, now a path mark.) Every stroke sends a thin cut of wind 3 m past its reach (40%) |
| B | Leaf Edge | Rule 2 | The open sweep's shove +1 m |
| B | **Storm of Leaves** | Keystone 4 | Hold Triangle 1.2 s (50 mana): the fan swept open overhead and down, and a storm of cut leaves flung in a cone 8 m long for 1.5 s: thirty small cuts shared among all in it (2 each), every enemy in it pushed back 2 m, and shots in it knocked down. [clip: new, the overhead sweep] |

- **A, The Gale Road**: the race, given. *How it looks*: a pale streak down the floor, dust and leaves pouring along it,
  bodies tumbling off its edges, her leaf straining like a sail. *From*: the land's race rule, where her path is a
  pale streak that arrives first (`WORLD.md`).
- **B, Storm of Leaves**: the wind made visible by what it carries. *How it looks*: a cone of tumbling red and gold
  leaves, each a small cut, dust low along the floor. *From*: the mountain where "the trees are where the wind shows"
  (`WORLD.md`), and the wind god of the folding screens.

Checked against The Bag Opened (the War Fan and Wind standout: the bag untied, a cone that carries you), Wind's Bag
of Winds, Indrawn Breath and Closing Circle: nothing here opens the bag, draws in, or rings an enemy; the Gale Road
carries you down a line with nothing ahead of you. Storm of Leaves is a cone like The Bag Opened's but leaves you
standing; question 10.3 offers a third keystone if the owner wants the cone gone.

**Fork option 2**: path A as in option 1; path B keeps the draft keystone **Every Stroke a Wind** as a passive (with
True Wind, Rule 2, the cuts of wind go 5 m).

Clips: 4 new (option 1).

---

## 11. Build cost in clips

New clips for the recommended fork options (the trunk basics, every bough's stem and both its paths). The War Fan's
base set is extra, as it is not built at all. Reused clips and rule-only marks are not counted.

| Weapon | Basics | Bough 1 | Bough 2 | Bough 3 | God's bough | All |
|---|---|---|---|---|---|---|
| Katana | 3 | Sheath 6 | Stream 3 | Answer 2 | Storm Katana 4 | 18 |
| Massive Sword | 3 | Doppelsöldner 3 | Half-Sword 4 | Trade 2 | Standing Stone 4 | 16 |
| Scythe | 3 | Swath 4 | Hook 2 | Harvest 3 | Golden Scythe 5 | 17 |
| Great Spear | 3 | Long Point 4 | Pole 4 | Cross 3 | Sun-Spear 5 | 19 |
| Great Hammer | 3 | Quake 2 | Swing 2 | Anvil 1 | Smith's Hammer 4 | 12 |
| Twin Knives | 2 | Shadow 2 | Rattle 2 | Reverse Grip 3 | Alchemist's Hooks 5 | 14 |
| Great Bow | 3 | Stave 3 | Snare 1 | Long Draw 2 | Huntress's Bow 4 | 13 |
| Chain Sickle | 3 | Weight 4 | Bind 3 | Sickle 3 | Fisher's Jawbone 4 | 17 |
| Fists | 3 | Wrapped Hands 2 | Hold 4 | Armament 1 | none | 10 |
| War Fan | 3 | Iron Fan 4 | Commander's Fan 0 | | Kazehaya's Fan 4 | 11, plus about 12 for the base set |
| **All** | **29** | | | | | **147, plus the fan's base set** |

About 15 new clips a weapon. The Forge (`ForgePose`, `ForgeClip`) makes clips from poses in code, so a clip is a pose
list, not a hand-keyed animation; the god moves are the ones worth an extra pass (a held pose, slowed time, the
camera's lean, E12). The enemy side needs two shared clips for the marks here: **knocked down** (already listed in
`weapons.md` 17) and **held** (E12).

---

## 12. Questions for the owner

**Answered (owner, 2026-11-19), all 56.** The owner took the recommended option (a) on every question but one:
**3.5 is (b)**: the Swath's keystone is renamed **The Long Stroke**, so no weapon mark touches the patron of time's
story before it is settled. Everything this plan recommends is therefore Decided: the general shape (G1 to G7), every
bough's fork option 1, every god's bough's pair of god moves (a), the trunk basics (a), the Doppelsöldner in place
of the Wall, the yumi-yari Stave, one tradition per knives form, Pole Spin dropped, Round the Post kept short, the
Armament at plain size, and the plain war fan built before Kazehaya's. Numbers stay first guesses, to be tuned in play.

Numbered for asking in batches of four. Each gives its options one to a line, the recommended one first. "Option 1"
and "option 2" are the fork options under each bough above.

### General

- **G1. A path's marks under the other keystone.**
  - (a, recommended) A path's two marks work whenever their bough is active, whichever keystone is set; only the
    keystones exclude each other.
  - (b) A path's marks work only while its own keystone is set.
- **G2. The plain bough's size.**
  - (a, recommended) Ten marks: a stem of three, two paths of two marks and a keystone (23 Memories).
  - (b) Nine marks: a stem of four, two paths of one mark and a keystone (21).
  - (c) Eleven marks: a stem of four, two paths of two marks and a keystone (25).
- **G3. The god's bough's size.**
  - (a, recommended) Twelve marks: a stem of five, two paths of two marks and a god move (25).
  - (b) Thirteen marks: a stem of four, two paths of three marks and a god move (27).
- **G4. The trunk basics' inputs.**
  - (a, recommended) One frame on every weapon: roll then R1 to cut, roll then R2 to break, guard held and R1 for the
    weapon's own.
  - (b) Each weapon chooses its own inputs for its basics.
- **G5. The Body's two bearings.**
  - (a, recommended) Both can be bought; one is active, set at a rest fire, like a bough's keystones.
  - (b) Both work at once once bought.
  - (c) Only one can ever be bought.
- **G6. Memory income.**
  - (a, recommended) God 8, elite 4, each new kind 1, named fighter 2, found 3 a solo land and 5 a duo land, the
    champion 12: about 200 a climb; a full Way and the Body in the second duo land.
  - (b) The same with found Memories 6 and 8: about 225; the Way fills about a land sooner.
  - (c) No found Memories: about 175; a full Way only at the champion.
- **G7. The light body's Long Step, which stacks with Wind's Long Stride.**
  - (a, recommended) Keep it: the owner named dodge distance for the light body.
  - (b) Replace it with Sure Footing (a dodge on rough or sliding ground is as long as on flat).

### 1. Katana

- **1.1 The Sheath's fork.**
  - (a, recommended) Option 1: The Cut Lands at the Click (one perfect held draw) and A Hundred Days at the Shrine
    (endless draws).
  - (b) Option 2: the draft's The Sword Stays Home (draws count as heavies) and A Hundred Days at the Shrine.
- **1.2 The Stream's fork.**
  - (a, recommended) Option 1: No Last Cut (stand and loop) and The Water Has No Shape (cut while sliding).
  - (b) Option 2: No Last Cut and Tsubame-gaeshi (a missed cut reverses at once).
  - (c) The Water Has No Shape and Tsubame-gaeshi, dropping No Last Cut.
- **1.3 The Answer's fork.**
  - (a, recommended) Option 1: Kiri-otoshi (every perfect guard answers by itself) and The First Sword (the great
    answer to a red blow).
  - (b) Option 2: Kiri-otoshi and Every Blow Answered (stored answers spent in one ring).
- **1.4 Narukami's Storm Katana: the two god moves.**
  - (a, recommended) Raikiri (the bolt called onto the raised blade and cut down a lane) and The Drum Ring (seven
    drums struck in turn round you).
  - (b) The draft's Thunder Follows (passive) and The Drum Ring.
  - (c) Raikiri and Thunder Follows as the second keystone.
- **1.5 The Katana's fourth basic.**
  - (a, recommended) Kozuka: the small knife from the scabbard flicked at an enemy 8 m off.
  - (b) Saya-ate: the scabbard's end swung up into the enemy's hands; its next attack 0.4 s late.
- **1.6 The Stream's Stepping Water and the roll's R1.**
  - (a, recommended) In the Stream, Stepping Water (a cut from any part of a dodge) replaces the Rising Draw.
  - (b) Stepping Water works only in a dodge's first 60%, and the Rising Draw keeps the end.

### 2. Massive Sword

- **2.1 The third bough.**
  - (a, recommended) The Doppelsöldner in place of the Wall: a form for crowds, which the sword lacked.
  - (b) Keep the Wall, with Stand Like Stone rewritten so it no longer repeats the Anvil's keystone.
  - (c) Liechtenauer's Master Cuts (the five named cuts of the German longsword, each beating one kind of attack); a
    second German form beside Half-Sword.
- **2.2 The Doppelsöldner's fork** (if 2.1 is a).
  - (a, recommended) Option 1: The Forlorn Hope (a road through a crowd) and Durendal Cleaves the Mountain (one blow
    that splits the floor).
  - (b) Option 2: the Wall's path, ending in Stand Like Stone, and Durendal.
- **2.3 Half-Sword's fork.**
  - (a, recommended) Option 1: Two Swords in One (the draft: grips woven in one chain) and The Cross Comes Down (the
    reversed sword as a hammer, three blows).
  - (b) Option 2: The Gaps in the Plate (four gaps marked, then one thrust through them all) and The Cross Comes Down.
- **2.4 The Trade's fork.**
  - (a, recommended) Option 1: Bear-Shirt (trade all the time, with the roar) and Tied to the Stone (the last stand
    below a quarter health).
  - (b) Option 2: The Unbroken Swing (each trade lengthens the chain's reach) and Tied to the Stone.
- **2.5 Brannoch's Standing Stone: the two god moves.**
  - (a, recommended) The Giant's Dance (every stone turns round you) and The Giant's Throw (a stone heaved 12 m).
  - (b) Seven Strides as a keystone and The Giant's Throw.
- **2.6 The Massive Sword's fourth basic.**
  - (a, recommended) Parierhaken: the side-hooks catch the enemy's weapon; its next attack held back 0.5 s.
  - (b) A pommel blow over the guard (6, 30 stagger).

### 3. Scythe

- **3.1 The Swath's fork.**
  - (a, recommended) Option 1: Mow the Field (stroke after stroke through a crowd) and The Long Stroke (one
    stroke 6 m round).
  - (b) Option 2: Mow the Field and Racławice (the blade straightened for the scythemen's charge).
- **3.2 The Hook's fork.**
  - (a, recommended) Option 1: Nothing Leaves the Curve (the draft: hold a crowd at the tip) and Fell the Giant
    (three Hamstrings bring an elite down).
  - (b) Option 2: Nothing Leaves the Curve and Reap the Crowd (a ring Hamstring after the full turn).
- **3.3 The Harvest's fork.**
  - (a, recommended) Option 1: The Last Sheaf (the draft's straw decoy) and The Threshing Floor (eight sheaves beaten
    out in a ring of chaff).
  - (b) Option 2: The Last Sheaf and Glean the Field (sheaves as instant heals).
- **3.4 Carnwyn's Golden Scythe: the two god moves.**
  - (a, recommended) The Wild Hunt (a horn, and the herd rides down a lane) and The Grove Answers (every thorn stand
    bursts into gold flowers).
  - (b) The Wild Hunt and the draft's Every Cut Grows Back (passive).
- **3.5 The nod to Kronos in the Swath.**
  - (a, recommended) Keep it: a quiet nod to the patron of time that feeds the doubt about it.
  - (b) Rename it The Long Stroke, so no weapon mark touches the patron's story before it is settled.

### 4. Great Spear

- **4.1 The Long Point's fork.**
  - (a, recommended) Option 1: Nobody Inside (the draft: no one stands within the point) and The Hundred Birds (a
    fan of points).
  - (b) Option 2: Changban (the couched charge three times running) and The Hundred Birds.
- **4.2 The Pole's fork.**
  - (a, recommended) Option 1: Never Touch the Ground (vault from back to back) and As Long as You Like (the shaft
    grows to 9 m for one slap).
  - (b) Option 2: Bull-Leaper (a vault through an attack is a perfect dodge) and As Long as You Like.
- **4.3 The draft's Pole Spin.**
  - (a, recommended) Drop it: swinging round the planted spear with kicks is Hold the Gate, the technique.
  - (b) Keep it and change Hold the Gate.
- **4.4 The Cross's fork.**
  - (a, recommended) Option 1: Thrust, Cleave, Slash (the draft's three uses on one enemy) and The Moon in the Pond
    (the crossbar drags a crowd in).
  - (b) Option 2: Thrust, Cleave, Slash and Catch Every Blade (the held guard catches every blow).
- **4.5 Khepharu's Sun-Spear: the two god moves.**
  - (a, recommended) Bare-Handed King (the draft, with its own clips: burning fists while the spear stands) and The
    Winged Disc (the thrown spear circles the enemy in fire).
  - (b) Bare-Handed King and The Ring of Noon (the planted ring grows); close to The Obelisk.

### 5. Great Hammer

- **5.1 The Quake's fork.**
  - (a, recommended) Option 1: The Floor Gives Way (the draft: three on one spot) and The Earth-Shaker Walks (a
    quake that strides forward).
  - (b) Option 2: The Floor Gives Way and Strike Where They Will Be (aimed drops, critical on arrival).
- **5.2 The Swing's fork.**
  - (a, recommended) Option 1: Never Let It Stop (the draft's endless turn, with the dust devil) and It Always Comes
    Back (the thrown hammer finds three).
  - (b) Option 2: Never Let It Stop and Weight for Distance (one long thrown arc).
- **5.3 The Anvil's fork.**
  - (a, recommended) Option 1: The Anvil Does Not Move (the draft) and Three Strokes on the Anvil (three rings that
    call the giant).
  - (b) Option 2: The Anvil Does Not Move and Strike the Whole Ring.
- **5.4 The Smith's Hammer: the two god moves.**
  - (a, recommended) The Smith's Gift (a blade forged in the fight) and Drive the Nail (a rivet struck home).
  - (b) The Smith's Gift and Talos's Embrace (seize and crush while hot); near the Fists' Hold.
  - (c) The draft's Hot Iron (passive) and Drive the Nail.

### 6. Twin Knives

- **6.1 The Shadow's fork.**
  - (a, recommended) Option 1: They Never See You (the draft, with the red seam) and Eight Changes (cross a crowd one
    back at a time).
  - (b) Option 2: The Shadow Stays (an after-image decoy) and Eight Changes; near Dark's shadow work.
- **6.2 The Rattle's fork.**
  - (a, recommended) Option 1: The Hundred Cuts (nicks counted to 100 and opened at once) and The Woven Storm (a
    walking whirl of cuts).
  - (b) Option 2: the draft's A Hundred Cuts (each nick +1%, passive) and The Woven Storm.
- **6.3 The Reverse Grip's fork.**
  - (a, recommended) Option 1: Take Their Blade (the draft, with the thrown-back weapon) and Guard and Strike as One
    (every block answered); near the Katana's Kiri-otoshi.
  - (b) Option 2: Take Their Blade and Chi Sao (stick to the attacker, its attacks late).
- **6.4 The Alchemist's Hooks: the two god moves.**
  - (a, recommended) The Last Elixir (all five stances at once) and Heaven and Earth (the joined hooks whirled
    overhead).
  - (b) The draft's Forever (passive) and Heaven and Earth.
- **6.5 The knives' traditions.**
  - (a, recommended) One for each form: Baguazhang for the Shadow, kali for the Rattle, Wing Chun for the Reverse
    Grip.
  - (b) Kali for all three: it has double knives, weaving drills and the reverse grip.

### 7. Great Bow

- **7.1 The Stave.**
  - (a, recommended) Make it the bow-spear, the yumi-yari, a blade lashed to the upper tip.
  - (b) Keep the plain stave of the draft.
- **7.2 The Stave's fork.**
  - (a, recommended) Option 1: No Range (the draft's point-blank shots, now throwing the enemy) and The Spear That
    Shoots (every thrust looses an arrow).
  - (b) Option 2: No Range and Bow and Blade (stave and shot alternate with no recovery).
- **7.3 The Snare's fork.**
  - (a, recommended) Option 1: Gleipnir (lines that bind) and The Trapper's Ground (lines that shoot).
  - (b) Option 2: the draft's The Hunting Ground and The Trapper's Ground.
- **7.4 The Long Draw's fork.**
  - (a, recommended) Option 1: Through Twelve Axes (one arrow pins a line of them) and Yoichi's Fan (the far shot).
  - (b) Option 2: the draft's One Arrow and Yoichi's Fan.
- **7.5 The Huntress's Bow: the two god moves.**
  - (a, recommended) Skaði's Circle (skate round the quarry loosing five) and Ski and String (skate on, loosing
    fans).
  - (b) The draft's The Hunt Never Ends (passive) and Ski and String.

### 8. Chain Sickle

- **8.1 The Weight's fork.**
  - (a, recommended) Option 1: Always Turning (the draft, with a second ring) and The Shooting Star (the weight falls
    as a meteor).
  - (b) Option 2: Always Turning and Round the Post as a keystone.
- **8.2 The Bind's fork.**
  - (a, recommended) Option 1: Whirled and Flung (a bound body as a flail) and Nothing Gets Away (the draft: every
    weight blow binds).
  - (b) Option 2: Lead the Bound and Nothing Gets Away; near None Go Far.
- **8.3 The Sickle's fork.**
  - (a, recommended) Option 1: Reap the Neck (the draft) and Near and Far (every sickle light also snaps the weight).
  - (b) Option 2: Reap the Neck and Spin the Short Chain (a turning shield).
- **8.4 The Fisher's Jawbone: the two god moves.**
  - (a, recommended) Fish Up the Island (an islet hauled up on a swell) and Noose the Sun (five lines, all slowed).
  - (b) Fish Up the Island and the draft's The Hook Remembers.
- **8.5 Round the Post, near the Water god's Heresy The Line Carries You.**
  - (a, recommended) Keep it in the Weight's stem: one anchor within 6 m, a swing, not a yank.
  - (b) Cut it, leaving travel by anchor to the Heresy.

### 9. Fists

- **9.1 Wrapped Hands' fork.**
  - (a, recommended) Option 1: Stand In (the draft: the slip is the dodge) and No Rounds (punch without stopping, the
    haymaker at twenty).
  - (b) Option 2: Stand In and Body Work (body blows' stagger does not fade).
- **9.2 The Hold's fork.**
  - (a, recommended) Option 1: Herakles and Antaeus (elites lifted and crushed) and Milo's Bull (a body lifted and
    slammed into a crowd).
  - (b) Option 2: Herakles and Antaeus and The Throw That Clears the Room.
- **9.3 The Armament's fork.**
  - (a, recommended) Option 1: Golden Bell (the draft's coat that grows) and Every Blow Returned (blows stored in the
    Reflect stance and given back).
  - (b) Option 2: Golden Bell and Two Coats; close to the mixed branches' first marks.
- **9.4 The Armament's size, as the bough in the god's bough's place.**
  - (a, recommended) Plain size, ten marks: the Fists already carry two techniques and two Heresies.
  - (b) A god's bough's size, twelve marks, with its two keystones made god-move size.
  - (c) Plain size, and each element's coat bend (`docs/design/weapons.md` section 9) as an extra mark for that
    element, 2 each, like the element infusion marks.

### 10. War Fan

- **10.1 The Iron Fan's fork.**
  - (a, recommended) Option 1: Ushiwaka on the Bridge (the draft: every turned blow strikes back) and The Thousandth
    Sword (ten delays, then one sword-blow).
  - (b) Option 2: Ushiwaka on the Bridge and Lose the Turn (the draft's true form).
- **10.2 The Commander's Fan's fork.**
  - (a, recommended) Option 1: The Field Is Mine (the draft) and Fūrinkazan (four stances in turn from the signal);
    near the Alchemist's Five Vials.
  - (b) Option 2: The Field Is Mine and Two Marks.
- **10.3 Kazehaya's Fan: the two god moves.**
  - (a, recommended) The Gale Road (ride a gale 14 m down her racing line) and Storm of Leaves (a cone of cutting
    leaves).
  - (b) The Gale Road and the draft's Every Stroke a Wind (passive).
  - (c) The Gale Road and The Tiger's Roar (a ring 5 m round you that shoves everything out 3 m; "wind follows the
    tiger"), if the cone sits too near The Bag Opened.
- **10.4 What is built first.**
  - (a, recommended) The plain war fan's own moveset first (about a dozen clips), then the boughs.
  - (b) Kazehaya's Fan first, since most players will hold the god's weapon, with the plain fan after.

Fifty-six questions in all: seven general and forty-nine on the weapons (Katana 6, Massive Sword 6, Scythe 5, Great
Spear 5, Great Hammer 4, Twin Knives 5, Great Bow 5, Chain Sickle 5, Fists 4, War Fan 4).

---

## 13. Sources

Nothing was searched on the web this session. Every real tradition and myth named above is from memory unless it is
one of those already searched for `weapons.md` (18) on 2026-11-18, linked here. **Check every item marked "from
memory" before any of it goes into in-game text.**

**Searched on 2026-11-18 (links from `weapons.md` 18)**

- Iai and Musō Jikiden Eishin-ryū: [Itten Dojo](https://www.ittendojo.org/articles/mjer).
- The Chinese spear's three methods: [Qiang](https://en.wikipedia.org/wiki/Qiang_(spear)),
  [Wudang Academy](https://wudang.academy/en/page/weapons-qiang); the cross spear:
  [Hōzōin-ryū](https://en.wikipedia.org/wiki/H%C5%8Dz%C5%8Din-ry%C5%AB).
- [Kusarigama](https://en.wikipedia.org/wiki/Kusarigama), [Isshin-ryū (Ryūpedia)](https://ryupedia.org/article/isshin-ryu-kusarigamajutsu),
  [Meteor hammer](https://en.wikipedia.org/wiki/Meteor_hammer).
- [Hook sword](https://en.wikipedia.org/wiki/Hook_sword); [Five Deadly Venoms](https://en.wikipedia.org/wiki/Five_Deadly_Venoms).
- [Tessenjutsu](https://en.wikipedia.org/wiki/Tessenjutsu); [Kurama-tengu](https://en.wikipedia.org/wiki/Kurama-tengu).
- [Paulus Hector Mair](https://wiktenauer.com/wiki/Paulus_Hector_Mair); [War scythe](https://en.wikipedia.org/wiki/War_scythe).
- [Skaði](https://en.wikipedia.org/wiki/Ska%C3%B0i); [Drona](https://en.wikipedia.org/wiki/Drona).
- [Hei matau](https://en.wikipedia.org/wiki/Hei_matau); [Golden maidens (Theoi)](https://www.theoi.com/Ther/AutomotonesKourai.html);
  [Pankration](https://en.wikipedia.org/wiki/Pankration);
  [Greek boxing (Working Classicists)](https://www.workingclassicists.com/zine/ancient-athletics-part-six-kicking-and-punching).

**From memory, not searched**

- *Japanese sword*: Hayashizaki Jinsuke and the hundred days at the shrine; nukitsuke and noto; Musashi's *Book of
  Five Rings* (the Water scroll) and the fight at the Ichijō-ji pine; Sasaki Kojirō's swallow cut and his long sword;
  Ittō-ryū's kiri-otoshi; ichi no tachi; tsuka-ate; the kozuka and the saya; zanshin; Tachibana Dōsetsu's Raikiri;
  Raijin's ring of drums on Sōtatsu's Wind God and Thunder God screens; Yagyū Shinkage-ryū's mutō-dori.
- *European sword*: the Landsknechts' Doppelsöldner and *verlorene Haufen*; the two-handed sword's Parierhaken;
  Liechtenauer's armoured fighting, Ringeck and Lew, the *Mordschlag*; Durendal and the Brèche de Roland.
- *Norse and Celtic*: the berserkir; Cú Chulainn tied to the standing stone; Geoffrey of Monmouth's Giant's Dance;
  the Cailleach and the last sheaf; Gwyn ap Nudd, the Wild Hunt and the hounds of Annwn; Pliny's druids; the Ankou;
  Völund hamstrung (*Völundarkviða*); Mjölnir and its makers; Gleipnir and Fenrir; Skaði on skis.
- *Greek*: Kronos's adamant sickle (Hesiod); Poseidon *Ennosigaios*; the Cyclopes' forge; Talos and his nail
  (Apollonius); Hephaestus; the sauroter; Odysseus and the twelve axes; Perseus' harpē; Herakles and Antaeus, and
  Herakles and Lichas (Ovid); Polydeuces and Amycus; Milo of Croton; boxing with no rounds; the Knossos bull-leapers.
- *Chinese*: Zhao Yun at Changban; "a hundred birds facing the phoenix"; Sun Wukong's Ruyi Jingu Bang;
  Baguazhang's deer-horn knives and circle walking; Wing Chun's butterfly knives, simultaneous defence and attack,
  chi sao; the golden bell cover and iron shirt; the meteor hammer's name.
- *Elsewhere*: the kosynierzy at Racławice (1794); Filipino kali's sinawali, "defanging the snake" and the pakal grip;
  Muay Thai's teep; the one-inch punch; fierljeppen; the Highland hammer throw; pile driving with a beetle; trappers'
  spring poles and set bows; hojōjutsu and the hayanawa; the yumi-yari; kyūdō's eight stages; Nasu no Yoichi at
  Yashima (*The Tale of the Heike*); Yoshitsune and Benkei on Gojō Bridge; Takeda Shingen's gunbai at Kawanakajima and
  Fūrinkazan; Horus of Behdet's winged disc; Maui noosing the sun.

**The project**: `CLAUDE.md`; `docs/GAME.md` (the pillars, the voice, "Steel and element: the rework of 2026-11-18");
`docs/design/STEEL_AND_ELEMENT.md` sections 7 and 8; `docs/design/steel/weapons.md` (the draft, every section);
`docs/design/steel/depth_weapons.md` (the Heresies, the Memory income and gates); `docs/design/weapons.md` (sections 0
and 1, the yardstick and E1 to E14); `docs/GUIDE.md` sections 1 to 3 and 7 (Wind's marks); `docs/design/WORLD.md` (the
gods, the elites, each god's fight); `docs/design/lightning_wind_levels.md` section 6 (the Last Retainer and the Hermit
of the Pass).
