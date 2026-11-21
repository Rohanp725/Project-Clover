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
