# Stand-in clearance plan

The owner's rule (2026-11-20): every stand-in gets a real solution in a later pass, not left as the stand-in. This plan
counts every stand-in, groups them by the engine piece each one waits for, and orders the pieces into passes. Written
2026-11-21 from `main` at `c39168a`. Read-only analysis: no code changed.

Where the stand-ins are written down:

- **`// NEEDS:` comments** under `Assets/_Project/Scripts`: **511** (457 start a comment with `// NEEDS:`, 54 have
  `NEEDS:` partway through a comment). All but two are in `Runtime/Player/Trees/` (the others: `SpellBranches.cs:86`,
  `Forms/RushForm.cs:9`).
- **The ledger** `docs/design/steel/content_open.tsv`: **141 rows** (143 lines less the note and the header).

How it was counted: a script read each NEEDS comment with its follow-on comment lines and matched the ledger ids in it
(315 lines name an id). The other 196 were matched by their words (158) or by hand (38). A line that names more than
one piece is counted once, under the first piece it names ("first"), and also under every piece it touches
("touching"). It is cleared only when every piece it names is built.

## 1. Inventory

### 1.1 Groups by the engine piece they wait for

| # | Group (engine piece) | NEEDS, first | NEEDS, touching | Ledger rows | Ledger ids in it (NEEDS lines first) |
|---|---|---|---|---|---|
| A | Counts and clocks kept on each enemy | 54 | 63 | 16 | count-rules 12, count-from-depth 9, fades-never 8, per-enemy-cooldowns 7, missed-counter 3, rest-fire-counts 2, damage-tally 2, blister-swells 2, sundered-past-three 2, struck-twice, seal-counts-bearer, struck-count, parry-count, reaction-start-count, mark-blows-on, alternate-blows |
| B | Affliction depth moved, held and shared | 17 | 25 | 12 | depth-moved 4, frozen-for 3, poison-held 3, poison-heal, depth-passed, quickened-beat, depth-heal, bite-share, laid-as-condition, burn-bite-share, seal-thunder 0 |
| C | Who a blow or reaction reaches (target filters and tests) | 57 | 60 | 18 | condition-needs 12, aim-at-afflicted 6, reaction-stats 6, reaction-struck 5, shatter-made 5, afflicted-only 4, forced-alone 3, side-of-blow 3, burst-spares-source 3, blow-made-critical 2, soaked-only-web 2, arc-through-afflicted, reaction-numbers-when, spend-soaked, stagger-on-condition, guard-made-perfect, area-set-off, reach-only-afflicted 0 |
| D | Combat moments (new triggers marks can answer) | 69 | 79 | 18 | their-blow-moment 15, enemy-begins-attack 10, thrown-into 7, freeze-moment 5, attacker-of-turned-blow 4, finisher-only 4, cast-on-beat 4, death-in-condition 4, shove-moment 4, red-attack 3, grown-bursts 2, death-reaction 2, heart-touch, laying-answer, condition-ends-now, arc-leaves, jar-deaths, ward-as-guard 0 |
| E | Shape and Empower order (a shape's rules against its Learn) | 41 | 47 | 5 | shape-sets-aside 17, base-spell-marks 8, empower-after-shape 6, form-times 6, shape-before-learn 3, and 1 with no row (a base spell's element) |
| F | Presses and cast windows | 48 | 59 | 8 | spell-windows 17, held-press 9, second-press 8, while-spell-stands 7, cast-in-motion 2, gathered-rings 2, bag-of-winds 2, own-clock-share |
| G | Player rules, prices and the blade | 16 | 19 | 13 | blow-made-heavy 3, afflictions-on-player 2, unshaken 2, blade-carries 2, cast-on-cooldown, health-scaled, wounds-returned, stood-still, blade-five, price-in-flasks, health-share 0, until-dodge 0, guard-edge 0, and 1 already buildable (untouchable) |
| H | Ground that keeps acting on what stands in it | 54 | 69 | 16 | ground-states 25, in-own-ground 7, dodge-path 4, pull-to-point 3, pitch-coat 3, seams 3, plume-along-path 2, mending-ground 2, whirl-fed 2, closed-ring, harmless-clouds, lane-shove, purify 0, in-own-water 0, puddles-burst 0, ground-flare 0 |
| I | Enemy states (what an enemy may not do) | 15 | 18 | 9 | links-pass-blows 6, carried-along 2, enemy-balance-held 2, shield-pass, no-dodge-or-leap, cannot-turn, turn-to-face, swallowed 0, god-hold 0, and 1 with no row (the shadow cut loose) |
| J | The player's own things (blades, grown things, helpers) | 28 | 33 | 10 | at-grown 7, planted-blades 6, returning-blade 3, helpers-answer 3, grown-sources 3, heart-places 2, helpers-most, grows-back, two-suns, blades-home |
| K | Standing things and thrown pieces | 49 | 53 | 11 | standing-things 28, pickups 5, cone-from-enemy 4, thrown-from-enemy 4, shatter-shards 2, bending-discharges 2, separate-pieces 2, pieces-at-nearest, orbit-fling, plates 0, sweeping-line 0 |
| L | Spell forms (new shapes of spell) | 54 | 75 | 1 | form-pieces: about 40 effect forms, 26 forms that move the player (new animation), 9 creature forms (cobra, serpent, totem, swarm, jaw, horns, shades, sapling, copy) |
| M | Looks | 6 | 8 | 2 | looks (the row names about 22 looks), fight-clock-dial 0 |
| N | Surges | 2 | 2 | 2 | surges 0, surge-from-mark 0, and 2 with no row (a duo surge chosen in the surge slot) |
| O | Owner decision and later phase | 1 | 1 | 0 | none (the Way refund) |
| | **Total** | **511** | | **141** | |

"0" after an id: the ledger row has no NEEDS line that names it (see 1.3).

### 1.2 Examples

- **A Counts and clocks.** `Trees/Duo/NerveFireDuo.cs:71` a count of bites kept on each enemy (now one bite in five
  by chance); `Trees/Reactions/Reactions.Near.cs:276` the elites' 4 s is one clock shared by every elite;
  `Trees/EarthTree.cs:105` the slow as 15% a stone (count-from-depth); `Trees/Duo/BlackChainDuo.cs:72` a notch kept
  while the curse holds (fades-never); `Trees/ForgeTree.cs:96` a count that empties at a rest fire.
- **B Depth.** `Trees/PoisonTree.cs:307` the sickest one's depth carried on the blade for five blows;
  `Trees/Reactions/Reactions.Near.cs:242` poison held back 3 s then landing doubled; `Trees/GlacierTree.cs:32` frozen
  for a set time whatever its Chill.
- **C Who it reaches.** `Trees/BloomTree.cs:88` a form aimed at the nearest Soaked enemy; `Trees/IceTree.cs:114` every
  reaction with Chill in it slowing all it struck (reaction-struck); `Trees/FireTree.cs:102` the stat ReactionDamage;
  `Trees/Reactions/Reactions.Five.cs:54` only a blow from the side is critical (side-of-blow);
  `Trees/Duo/ThermalShockDuo.cs:74` an answer to an enemy that is Cracked (condition-needs).
- **D Moments.** `Trees/DarkTree.cs:102` an enemy's wind-up as the moment; `Trees/FireTree.cs:184` and six more: "the
  shared movement rule's timing (started as a blow would land)"; `Trees/WindTree.cs:227` a thrown enemy meeting a wall
  or another enemy; `Trees/Reactions/Reactions.Near.cs:175` "at once if it freezes"; `Trees/Duo/IllWindDuo.cs:105`
  any shove setting off the shed.
- **E Shape and Empower.** `Trees/DarkTree.cs:128` standing again where you were, the step behind set aside
  (shape-sets-aside); `Trees/IceTree.cs:151` the Empower's wider fog not reaching past this shape's own;
  `Trees/FireTree.cs:64` "The base spell is cast by its kit, which takes only the Empower's wait" (five lines like it);
  `SpellBranches.cs:86` the element of a base spell cast by its kit.
- **F Presses and windows.** `Trees/LightningTree.cs:66` only when the lance is thrown within 0.5 s after the finisher;
  `Trees/LightningTree.cs:114` the held press, the clap growing with the hold; `Trees/LightningTree.cs:167` a second
  press while the rod stands; `Trees/LifeTree.cs:105` an effect in force only while its spell's thing stands.
- **G Player and blade.** `Trees/LightningTree.cs:61` the blade charged for five blows, not three; `Trees/DarkTree.cs:196`
  a spell cast while its slot still waits, paid in grey health; `Trees/WaterTree.cs:344` the flask as the price;
  `Trees/EarthTree.cs:60` afflictions laid on the player.
- **H Ground.** `Trees/Reactions/Reactions.Four.cs:115` the pools as ground, dosing whatever stands in them;
  `Trees/IceTree.cs:236` enemies on the sheet thrown half again as far; `Trees/Duo/CapsizeDuo.cs:58` ground laid along
  the dodge's own path; `Trees/DarkTree.cs:295` everything in the pool drawn slowly to its middle.
- **I Enemy states.** `Trees/Reactions/Reactions.Near.cs:231` links that pass 30% of weapon damage and the 4 m leash;
  `Trees/FireTree.cs:210` balance not recovering in the ash; `Trees/Duo/RustDuo.cs:102` joints seizing, turning slower.
- **J Own things.** `Trees/MetalTree.cs:186` a turning blade drawn flying out and back to the hand;
  `Trees/LifeTree.cs:221` going under the ground to a grown thing of yours within 12 m; `Trees/DarkTree.cs:182` three
  shades at most.
- **K Standing things.** `Trees/Reactions/Reactions.Near.cs:106` the iron-faced stone, also "stone at its back";
  `Trees/IceTree.cs:191` the wall's 60 health; `Trees/Reactions/Reactions.Five.cs:110` a fan of three feathers, each
  stopping at the first it touches; `Trees/Duo/ShiningHerbDuo.cs:89` things on the floor the player picks up.
- **L Forms.** `Trees/FireTree.cs:81` the cobra spitting on a wind-up; `Trees/LightningTree.cs:202` all seven cuts on
  one line through the enemy faced; `Trees/PoisonTree.cs:312` the rush backward, away from the enemies;
  `Trees/WaterTree.cs:304` the serpent going out and coming back.
- **M Looks.** `Forms/RushForm.cs:9` a gust that runs ahead and a sickle cut in front of the enemy;
  `Trees/Reactions/Reactions.Near.cs:255` the hidden flame, a dark red glow in cracks; `Trees/WaterTree.cs:330` the
  line of foam running over the floor to you.
- **N Surges.** `Trees/EclipseTree.cs:109` and `Trees/OvergrowthTree.cs:92` a duo surge chosen in the Hand's surge slot.
- **O Owner.** `Trees/RetiredMarks.cs:192` whether a Shards-bought mark that moves to the Way is refunded then.

### 1.3 Mismatches between the code and the ledger

**Ledger rows with no NEEDS line (10).** Two are named in words inside lines tagged with another piece (sweeping-line
in `Duo/EarthlightDuo.cs:103`, ground-flare in `Duo/SmoulderDuo.cs:26`). The other eight have no NEEDS line at all:
`god-hold`, `surges`, `surge-from-mark`, `ward-as-guard`, `guard-edge`, `reach-only-afflicted`, `fight-clock-dial`,
`seal-thunder`. Their stand-ins live only in the marks' "Not built yet" details or in the ledger. Each needs a NEEDS
line at its code site so a pass finds it. 34 rows are never named by id; 24 of those are named only in words.

**NEEDS lines with no ledger id (196).**
- 151 name a ledger piece in words only (mostly `Reactions/*.cs` and `Duo/*.cs`, for example "per-enemy cooldowns",
  "standing things that block", "a shove moment"). Add the id in brackets when the line is next touched.
- 38 were matched by hand, mostly follow-on lines under a tagged line: form details such as `WindTree.cs:242` and
  `IceTree.cs:78`, held stances such as `FireTree.cs:141`. Most belong under form-pieces, whose row does not list them
  one by one.
- **7 have no ledger row at all:**
  - `SpellBranches.cs:86`: a base spell's element.
  - `EclipseTree.cs:109` and `OvergrowthTree.cs:92`: a duo surge chosen in the surge slot.
  - `ElementSteel.cs:135`: the shadow cut loose, an enemy state.
  - `ElementSteel.cs:121`: the wound-up blow that cannot be stopped (counted under unshaken).
  - `RetiredMarks.cs:192`: the Way refund.
  - `ElementSteel.cs:169`: untouchable through the lunge. **Buildable now**: `UpgradeAction.Untouchable` was built
    2026-11-20.

**Rows marked built that are still open.**
- `their-blow-moment` and `second-press` say "Built 2026-11-20", but 21 NEEDS lines still wait on their open parts:
  - The moment a blow is about to land (the shared movement rule), 7 lines, for example `WaterTree.cs:221` and
    `WindTree.cs:164`; and a short window on that blow, 3 lines (`IceTree.cs:209`, `LightTree.cs:334`,
    `PoisonTree.cs:230`).
  - A condition that drifts to the nearest enemy when its carrier dies, 2 lines: `Duo/TheGrudgeDuo.cs:32` and
    `Reactions/Reactions.Three.cs:134`.
  - Presses on a thing the spell left standing, 8 lines, for example `LightningTree.cs:167` and `MetalTree.cs:339`.
- Split these into their own rows (`blow-about-to-land`, `drift-on-death`, `press-on-standing-thing`) so the built
  parts can be struck off.

**Later phases.** No NEEDS line or ledger row waits on Bond, Heresies or Evolutions (Phase 6). One waits on the Way
(Phase 5): `RetiredMarks.cs:192`. One leans on Track L's rest fires: rest-fire-counts (2 lines). It can be built now
against the playtest's rest point (`Core/Save/RestRef.cs`) and checked again when L.4 moves the rest fire to the hub.

## 2. Passes

Each pass builds one engine piece and strikes off every NEEDS line and ledger row that waited only on it and on
earlier passes. "Clears" counts NEEDS lines fully finished by that pass, given the passes before it; a line that
names two pieces clears at the later one. Sizes: small is about a session, medium two or three, large a week of
sessions or more. Every pass ends with the `unity-check` skill (compile, console, tests). A pass marked "play" also
needs a `playmode-probe` of what it touches, filmed only where the change is (targeted testing).

| Pass | Builds | Clears (running total) | Ledger rows struck | Needs first | Size | Editor | Blender | New animation | Owner |
|---|---|---|---|---|---|---|---|---|---|
| 0 | Ledger tidy-up and the one buildable now | 1 (1) | 0, 3 rows split | nothing | small | compile only | no | no | no |
| 1 | A: counts and clocks kept on each enemy | 44 (45) | 16 | nothing | medium | tests | no | no | no |
| 2 | B: affliction depth moved, held and shared | 16 (61) | 12 | 1 | small | tests | no | no | no |
| 3 | C: who a blow or reaction reaches | 49 (110) | 18 | 1 | medium | tests, one play check | no | no | no |
| 4 | D: combat moments | 65 (175) | 18 | 1 | large (4a, 4b) | play | no | no | no |
| 5 | E: shape and Empower order | 32 (207) | 5 | 4 for 6 lines | medium | tests | no | no | no |
| 6 | F: presses and cast windows | 51 (258) | 8 | 4, 5 | medium | play, input | no | one held stance | no |
| 7 | G: player rules, prices and the blade | 14 (272) | 13 | 1 | small | play, HUD | no | no | Q1 |
| 8 | H: ground that keeps acting | 56 (328) | 16 | 1, 3, 4 | large | play, effects | no | no | no |
| 9 | I: enemy states | 16 (344) | 9 | 4 | medium | play | no | knocked-down clip (in staging) | no |
| 10 | J: the player's own things | 29 (373) | 10 | 4, 8 | medium | play | no | no | no |
| 11 | K: standing things and thrown pieces | 52 (425) | 11 | 4, 8 | large | play, colliders | 5 props | no | Q2 |
| 12 | L: spell forms (12a effects, 12b movement, 12c creatures) | 75 (500) | 1 | most of the above | large (three passes) | play, effects | 12c | 12b | Q3 |
| 13 | N: duo surges in the surge slot | 2 (502) | 2 | 6 | small | play, HUD | no | no | Q4 |
| 14 | M: looks | 8 (510) | 2 | the effects pipeline | large, spread out | effects | some | no | no |
| 15 | O: the Way refund | 1 (511) | 0 | Phase 5 (the Way) | small | no | no | no | Q5 |

The order puts the cheap, code-only pieces first: passes 1 to 3 clear 110 lines with no Blender, no animation and no
owner decision. Then come the triggers most other passes stand on (pass 4), and the art-heavy pieces come last.
Passes 2, 3 and 7 do not depend on each other and can run in any order after pass 1.

### Pass 0: tidy the ledger, clear the one that is ready (small)

- Clear `ElementSteel.cs:169` (untouchable through the lunge) with `UpgradeAction.Untouchable`, built 2026-11-20.
- Add ledger rows for the pieces with none: `base-spell-element`, `duo-surge-slot`, `shadow-cut-loose`,
  `unstoppable-blow` (or fold it into `unshaken`) and `way-refund`.
- Split the open parts out of `their-blow-moment` and `second-press` into `blow-about-to-land`, `drift-on-death` and
  `press-on-standing-thing`.
- Add NEEDS lines at the code sites of the 8 rows with none (1.3).
- Put the ledger id in brackets on the 151 lines that name their piece only in words. A script can do most of it,
  keeping each file's own line endings. Do this before pass 1 so each pass finds its lines with one Grep.

### Pass 1: counts and clocks kept on each enemy (medium; clears 44)

- **Builds** one fixed-size record per enemy, allocated with the enemy (IMPLEMENTATION 8.1 budgets 220 bytes an enemy
  for the reaction rest). It holds:
  - cooldown clocks for each effect, so an elite's flinch waits 4 s on each elite, not on all of them;
  - counters for each mark (every fifth bite, every other pass, three beats in 3 s, a perfect guard beating two);
  - a damage tally with a cap;
  - a count set from an affliction's depth;
  - a "lasts the fight" flag;
  - Sundered allowed up to five;
  - a count emptied at a rest fire.
- **Clears** count-rules, count-from-depth, fades-never, per-enemy-cooldowns, missed-counter, damage-tally,
  blister-swells, sundered-past-three, and the one-line rows in group A. The stand-ins that work "by chance" (one bite
  in five by chance) become exact.
- **Tests**: EditMode tests on the record, no scene. It only needs the editor to compile and run them.
- **Rest fire**: rest-fire-counts is built against the playtest's rest point now and checked again when Track L task
  L.4 moves the rest fire to the hub.

### Pass 2: affliction depth moved, held and shared (small; clears 16)

- **Builds**, on top of pass 1's record:
  - poison held back, then landed at once and doubled;
  - a quickened beat;
  - a bite share while a condition holds, for poison and for Burn;
  - depth moved between enemies as one number, passed on at death and carried on the blade;
  - a heal by depth, or from the damage poison really dealt;
  - a freeze laid as a state for a set time;
  - an affliction laid as if its condition were met;
  - the brand's thunder (seal-thunder).
- **Code and tests only.**

### Pass 3: who a blow or reaction reaches (medium; clears 49)

- **Builds** target tests in the effect data:
  - only enemies carrying an affliction;
  - aimed at the nearest enemy carrying one;
  - an arc that leaps on only from those that carry one;
  - the reaction's struck set as the targets a mark answers;
  - a condition another mark set (Cracked, swollen, warmth stolen);
  - the side the blow comes from;
  - a burst that spares its source;
  - a reaction forced when only one of its pair is carried;
  - a shatter made by an effect;
  - a guard made perfect by a condition;
  - the stats ReactionDamage and ReactionStagger.
- **Checks**: EditMode tests, and one play check for side-of-blow, which needs the facing at the moment of reacting.

### Pass 4: combat moments (large, as two passes; clears 65)

- **4a, moments the game already sees in its own code (medium):**
  - freeze, thaw and shatter;
  - any shove (Gust, ram, Riptide, a lift), not only a heavy blow;
  - an enemy that dies in a condition, or is killed by a reaction's own burst;
  - a finisher apart from other heavy blows;
  - an affliction being laid;
  - an arc leaving an enemy;
  - a condition ended on demand;
  - a poison bite (cast-on-beat);
  - a grown thing's burst;
  - the heart touched in a perfect dodge;
  - deaths inside the jar's ring;
  - a condition drifting to the nearest enemy when its carrier dies.
- **4b, moments that need the enemy's side (medium, play):**
  - an enemy beginning its wind-up, and the wind-up held 0.3 s (Animator speed, not a new clip);
  - a blow about to land (the shared movement rule, 7 lines);
  - a red attack turned, guarded or dodged (`Enemy.cs` and `Colossus.cs` already know red attacks);
  - the attacker of a turned blow, apart from what the answer strikes;
  - a ward's turn counted as a perfect guard;
  - a thrown body meeting a wall, a pillar or another enemy (a sweep along its throw).
- **Checks**: each moment gets a probe on the dummy and one real enemy.

### Pass 5: shape and Empower order (medium; clears 32)

- **Builds**:
  - a shape that sets aside an effect of its Learn while worn (17 lines);
  - a shape's rule answered before its Learn spends what it tests;
  - an Empower that adds to the worn shape's numbers rather than setting its own over them;
  - a base spell cast through its Learn mark with all its numbers (the "cast by its kit" lines);
  - a base spell that knows its own element when two are carried;
  - a form's lasting things timed by the spec (crater, ring, cobra).
- **Checks**: data and rules only, with the chart and mark tests.

### Pass 6: presses and cast windows (medium; clears 51)

- **Builds**:
  - a cast within a moment's window as a Master's condition (0.5 s after a finisher, 0.3 s after a perfect dodge,
    1 s after the last bolt);
  - an instant cast with no wind-up;
  - the held press (`SpellSpec.HoldForm`), a stance kept while held;
  - presses a shape or Master adds, and presses on a thing the spell left standing (rod, shadow, bag);
  - an effect in force only while its spell's thing stands;
  - spells cast in motion;
  - rings gathered by blows;
  - a shared, halved wait;
  - the bag of winds.
- **Input**: needs a hold interaction in `Controls.inputactions`, made through the editor, and a play probe for each
  press.
- **Animation**: one stance, walking at half speed while held (`FireTree.cs:141`). Use a blend on the existing walk if
  it reads well, and a new clip only if not.

### Pass 7: player rules, prices and the blade (small; clears 14)

- **Builds**:
  - the next light blow struck as a heavy;
  - something hung on the blade for the next blow of a kind;
  - the blade charged for five blows;
  - unshaken, and the wound-up blow that cannot be stopped;
  - standing still for a set time;
  - a boon that ends at the next dodge;
  - the guard edge;
  - prices paid in health, in flasks, or as a share of health, and grey health owed;
  - wounds given back as grey health;
  - damage rising as health falls.
- **Afflictions on the player** (question 1, decided): elites and gods lay their land's affliction when they hit.

### Pass 8: ground that keeps acting (large; clears 56)

- **Builds** ground that tests what stands in it each tick, not only what stood there when it formed:
  - pools, nets, slicks and sheets that slow, dose, float or knock down whoever wades in later;
  - the player's own ground other than fire, and their own water, as states;
  - ground laid along a dodge's path or a thrown enemy's path;
  - pulls to a point;
  - the Closed Ring as a wall;
  - lanes that shove;
  - star-shaped seams;
  - ground that mends the player;
  - ground that flares at its end;
  - pitch that spreads and lights all at once;
  - enemy clouds burned away, and clouds that cannot harm the player;
  - puddles burst by a heavy blow.
- **Budget**: stay inside the fixed 48-hazard budget, and read the profiler before raising it.
- **Checks**: walk every ground in play (the scenery-must-be-played rule), and give each its element's look (fire must
  read as flame).

### Pass 9: enemy states (medium; clears 16)

- **Builds** what an enemy may not do:
  - no dodge, leap or step back while sunk, with the body drawn lower by a root offset;
  - cannot turn, or turns slower;
  - balance not recovering;
  - turned to face a point;
  - blows that pass a shield;
  - a hold that reaches gods at 0.4 of its time;
  - swallowed;
  - links that pass a share of blows, and the 4 m leash;
  - enemies carried before a moving spell;
  - the shadow cut loose.
- **Animation**: "tripped, knocked down" uses the knocked-down clip already in staging (overnight run 2026-11-19),
  brought in through asset-intake.

### Pass 10: the player's own things (medium; clears 29)

- **Builds** one registry of the player's things in the world, which marks can count, test, drive and fly to:
  - planted blades;
  - the returning blade, drawn going and coming;
  - blades that never fade;
  - grown things as sources, grown things that grow back, and casts tested against grown things;
  - helpers with a cap, and helpers that answer the heavy blow;
  - the figure of light that casts again;
  - Pawned Heart's other places.
- **Needs** pass 4's grown-burst moment and pass 8's own ground.

### Pass 11: standing things and thrown pieces (large; clears 52)

- **Builds standing things** that block movement and shots and can be struck or broken, each with health, a collider
  and NavMesh carving:
  - the iron-faced anvil stone, which also counts as "stone at its back";
  - the rimed mound;
  - the hollow trunk;
  - the ice wall with 60 health;
  - the stone pillar or spike;
  - the Grave Marker's stone;
  - the sundial stone;
  - plates that each take a share of a blow.
- **Builds thrown pieces**, each its own striker:
  - a fan thrown from an enemy, each piece stopping at the first it touches;
  - pieces thrown at the nearest others;
  - separate blades;
  - a shatter's own shards;
  - a cone from an enemy's far side;
  - discharges that bend toward one enemy;
  - an orbiting piece flung by a blow.
- **Pickups** (question 2, decided): real pickups you walk over, gone after 8 s (`World/WeaponPickup.cs` is a
  starting point).
- **Blender**: about 5 props through the blender-prop skill (anvil stone, rimed mound, hollow trunk, ice wall, stone
  pillar), each scanned and walked.

### Pass 12: spell forms (large; clears 75, the most of any pass)

Split three ways, and run after the passes whose rules the forms use.

- **12a, effect forms** (about 40 lines; editor and effects):
  - cuts on one line;
  - an end behind the last enemy, with a shape of light left behind;
  - a cloud that drops bolts as you strike;
  - a wall walking ahead;
  - shards falling from above;
  - bubbles cracking in turn;
  - drums of cloud;
  - a wind coming down on a point;
  - a whirl that wanders.

  Every one is animated (the effects-must-be-animated rule).
- **12b, forms that move the player** (about 26 lines; new animation): the rush backward, a step through the enemy, a
  drive back out of the fight, going under the ground to a grown thing, the slide and the lunge. Each needs a clip or
  a root-motion curve on the existing dodge.
- **12c, creature forms** (9 lines; Blender and owner question 3): the cobra that spits, the serpent going out and
  back, the totem, the swarm, the jaw from the shadow, the horns, the shades, the sapling and the copy behind you.

### Pass 13: duo surges in the surge slot (small; clears 2 lines and 2 rows)

- **Builds** a duo surge chosen in the Hand's surge slot, and the open surge rows: Kindle's Fed Flame and the others
  in `surges`, and a surge lit by a mark at no cost.
- **Question 4, decided**: the duo surge is picked in the surge slot at a rest, in place of a single element's surge.

### Pass 14: looks (large, spread over the effects passes)

- **Builds** the ledger's `looks` row and the fight-clock dial. The row names about 22 looks: the hidden flame, the
  roots on the floor, the jar's ring, the scorched fern, the mock suns, the glassy fulgurite, and the rest.
- **When**: few NEEDS lines wait on these, but every stand-in puff they replace is on screen. Fold each look into its
  land's effects pass rather than running it alone, with the review-shots check and the element rule.

### Pass 15: later phase (Phase 5)

- `RetiredMarks.cs:192`, the Way refund, cannot be cleared until the Way exists. It is owner question 5 and also
  question 2 in `phase4_questions.md`.
- Nothing waits on Bond, Heresies, Evolutions or Track L beyond the rest-fire check in pass 1.
- Those phases will add stand-ins of their own. Each gets a NEEDS line and a ledger row when written, and joins the
  pass whose piece it waits for.

## 3. Owner questions

Only the stand-ins that need a design choice. Every other stand-in is engine work with its design already written.
The owner answered questions 1 to 4 on 2026-11-21 (also in the decisions log in `docs/GAME.md`); question 5 keeps its
default.

1. **Afflictions laid on the player** (pass 7; Tremor, Bedrock and Daybreak's High Noon answer them, 3 marks). Who
   lays them?
   - a) Elites and gods lay their land's affliction on a hit (Burn in Sun City, Chill on the Ice land).
   - b) Every enemy lays its land's affliction.
   - c) No one: the three marks are reworded to answer something that exists now, such as stagger or a knock-down.

   **Recommended: a.** It gives the marks something to answer without making every fight about cleansing.

   **Decided 2026-11-21: a.** Elites and gods lay their land's affliction on the player when they hit.
2. **Pickups on the floor** (pass 11; Shining Herb, Firebloom, Seed Under Snow, The Whole Mountain, 5 lines). How do
   they work?
   - a) Real pickups: a small herb or seed you walk over to take, gone after 8 s.
   - b) No pickup: the gift goes to you at once if you are within 3 m when it appears.
   - c) They fly to you by themselves after 1 s.

   **Recommended: a.** It is what the marks say, and it makes you move through the fight.

   **Decided 2026-11-21: a.** Real pickups: you walk over a herb or seed to take it, and it is gone after 8 s.
3. **The creature forms** (pass 12c; the cobra, the serpent, the totem, the swarm, the jaw, the horns, the shades, the
   sapling, the copy, 9 lines). What are they made of?
   - a) Effects only (flame, water, shadow and particles shaped like the creature), no model.
   - b) A Blender model for each, with a simple rig, dressed in its element's effect.
   - c) Models from the ComfyUI and TRELLIS pilot once it is judged, Blender for the rest.

   **Recommended: b** for the cobra, serpent and totem, which stay on screen and must read; **a** for the brief ones
   (jaw, horns, swarm, copy).

   **Decided 2026-11-21: a mix.** Blender models with a simple rig for the cobra, the serpent and the totem;
   effect-only shapes for the jaw, the horns, the swarm, the copy, the shades and the sapling.
4. **Choosing a duo surge** (pass 13; Eclipse and Overgrowth, 2 lines and 2 rows). How does the player get it?
   - a) The duo surge is picked in the Hand's surge slot at a rest, in place of a single element's surge.
   - b) It fires by itself when any surge is lit while both elements are carried (the stand-in now).
   - c) It gets a slot of its own beside the surge.

   **Recommended: a.** One slot, one clear choice, and no surprise surges.

   **Decided 2026-11-21: a.** The duo surge is picked in the Hand's surge slot at a rest, in place of a single
   element's surge.
5. **The Way refund** (pass 15; `RetiredMarks.cs:192`). This is already question 2 in `phase4_questions.md`. The
   default stands until the owner answers there: no refund now, and the mark is kept under its id until the Way
   arrives.

   **Default kept (2026-11-21).**
6. **Plasma's Flashover on an enemy with no Static** (pass 3, forced-alone). Flashover's burst is a discharge of
   Static, so forced on a burning enemy that carries none it would strike nothing.

   **Decided 2026-11-22:** the forced Flashover lays one Static on the enemy just before, so the discharge always fires.
7. **What the afflictions laid on the player do** (pass 4, afflictions-on-player, after question 1).

   **Decided 2026-11-22:** their full effects plus a HUD mark. Each affliction on the player does its own thing,
   mirroring the enemies where it makes sense for a player: Chill slows, Burn burns, Soaked makes you slower to dodge,
   and so on. Each shows as a small icon with a timer by the health bar, with a light look on the body. The rules and the
   HUD component are built offline with tests; the body look stays a stand-in in the ledger's `looks` row until a look
   pass.
8. **How The Yellow Gale shares its store** (pass 2, depth-moved). The gale drinks clouds, trails and half of each
   poisoned enemy's doses near it into a store "shared among those the gale strikes".

   **Decided 2026-11-22:** the store is split evenly among the enemies the gale strikes, rounded up. One lone enemy gets
   it all; the more it strikes, the thinner it spreads.
9. **What One by One's Master makes stronger** (pass 4, cast-on-beat). Its snap on the poison's beat makes bursts half
   again as hard: every burst for 1.5 s, or only One by One's own?

   **Decided 2026-11-22:** only One by One's own bursts. Reactions' bursts and Volatile's in the same moment are not
   made stronger.
10. **Totality and The Ground Wakes as duo surges** (pass 13, duo-surge-slot): 40 mana and 10 s each, the house
    standard, as built.

    **Confirmed 2026-11-22.** Their row is closed.
11. **What The Tide Turns' line of wet sand does** (pass 8, ground-states). The ebb leaves a line of wet sand; nothing says
    what it does to what stands on it.
    - a) It Soaks what stands on it each second for 3 s and slows it 15%, like your other water.
    - b) It Soaks only.
    - c) It is only a look.

    **Recommended: a.** It reads as water, so it acts as your water does.
12. **Which way Sweep of Wings throws** (pass 12a). The wave form strikes all at once, so the sweep has no direction of its own.
    - a) Every enemy struck is thrown to your left, as if the wing swept from your right (built so).
    - b) Each is thrown outward from your line, to the side it stood on.
    - c) The way you turned as you cast.

    **Recommended: a.** One clear way reads as one sweep.
13. **Rip Current's "no dodging against the current"** (pass 8, no-dodge-or-leap).
    - a) While in the foam an enemy cannot leap, lunge or step back at all (the built Mired rule).
    - b) Only a dodge against the current is forbidden (a new rule).
    - c) Drop it.

    **Recommended: a.** The rule exists and the foam is only 6 s.
