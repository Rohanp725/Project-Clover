# Phase 4 progress

## 2026-11-20: tasks 4.1 to 4.3 merged into main and proven in the editor

**Merged:** branch `phase4` up to `d408f6c`, the newest commit that finished a whole tree. The worktree was clean when the merge
was made. This takes in 4.1 (`7fd52dd`: the spell-branch schema, `IChartLayout`, `RetiredMarks` and the save migration),
4.2 Fire (`926f257`), 4.3 Lightning (`47467d9`), the questions list (`3a5dbd2`), and the Wind, Ice, Water, Life and Earth
trees (`9df63cf`, `2f38e28`, `3283116`, `a40e468`, `d408f6c`). Merge commit: `46ff0c9`, made with `--no-ff`.

**Conflicts:** none in the text. Only `PlayerUpgrades.cs` was changed on both sides, and the changes touched different places.
Main's changes were the reaction-ring guards in `Do`, `CombatEvents.Report(..., laying)`, `OnAfflictionFull` and `OnCarried`.
Phase 4's were the `SpellBranches` hooks in `Owns`, `Refresh`, `Watch`, `Cast` and `PriceOf`, and the clock actions. Both
sets are in the merged file. Unity made the `.meta` files for the eleven new scripts, and they were committed with the merge.

**Compile:** clean, with 0 errors and 0 warnings.

**EditMode suite:** 735 run and 735 passed, after Clover > Build > Playtest Texts (89 effects and 341 marks' details). The
named fixtures all passed:

| Fixture | Passed |
|---|---|
| SpellBranchTests | 9 |
| InForceTests | 5 |
| UpgradeChartTests | 28 |
| SaveMigrationTests | 11 |
| MarksBuiltTests | 9 |
| KeystoneRuleTests | 7 |
| PoisonBeatTests | 10 |
| SpellCooldownTests | 14 |
| SpellSlotsTests | 10 |
| PlaytestModeTests | 9 |
| HandRulesTests | 15 |
| SignatureMoveTests | 14 |
| ReactionCatalogueTests | 6 |

**Probes** (all in the sandbox, through `runprobe.sh`):

- `BranchProbe`, new, in ProvingGround: **42 OK, 0 WRONG.** It plays Fire's Flame Wave and Lightning's Thunderclap.
  - Each Learn comes with its element and is never sold.
  - The wait is the table's at first (7 s and 9 s), then the Empower's (5.5 s and 7 s).
  - Neither a Shape nor a Master is sold before the mark it grows from.
  - Each first shape is worn by default, cast through its mark in its own form (`wave` and `spikes`), and keeps the Empower's wait.
  - `ChooseShape` is refused away from a rest fire. At the fire it sets the second shape, which casts in its own form
    (`spikes` and `bolt`) with the same wait.
  - Flame Wave's Master: cast within 0.5 s of the charged blade's third blow and no clock starts. After 0.8 s the promise has
    lapsed and the 5.5 s clock starts. While the clock runs, the third blow does not end it.
  - Thunderclap's Master: a perfect guard takes the wait from 7.00 s to 0, the next cast starts no clock, and the one after
    waits 7 s again.
  - Empower, Shape and Master are out of force without the spell slotted.
- `P1aProbe`: **44 OK, 0 WRONG.** It was moved off the retired Cinder Fan and Sunspot. The chart's "next slot" now uses
  Black Pitch and Uraeus, slot North casts Uraeus, and the slot-fill check accepts any three learned Fire spells after the base.
- `UpgradeProbe` in CombatTest: runs to `done` with 0 errors and 0 warnings.
  - The retired marks were taken out or replaced by their successors: Breath of the Lioness for Burn and Detonate, Uraeus for
    Cinder Fan, Drawn From the Drowned for Catch the Rain (the five deaths are now spaced 0.25 s apart, and it fills a flask:
    2 to 3), and The Garden Holds You for Shade.
  - Lines whose mark is gone now say what to expect.
- `MarksProbe` in CombatTest: runs to `done` with errors logged 0.
  - The moves for the retired Flashpoint, Open Furnace, Clean Break, Heartwood, Spring Tide, Scorched Earth and Storm Front
    are removed.
  - `sunspot` became `pitch` (Black Pitch), `reserve` plays Stored Charge (it banks three and spends three), and `lodestone`
    plays Lightning Rod wearing its Lodestone shape (cast as `lodestone`, shoves 51 each).
- `ReactionProbe2`: **336 OK, 0 WRONG** (heard 54, shown 54, dropped 0).
- `CrowdProbe`: **6 OK, 0 WRONG.**

**The chart** (`review/shots/current/branch_chart_fire.png`, `branch_chart_lightning.png`, `branch_chart_wide.png`): the
branches read. Each spell is a stalk of four: the Learn's diamond at its foot, then the plus (Empower), the spiral (Shape) and
the star (Master). Six stalks rise from the three boughs, and the learned path is lit from the root to the Master. In the whole
ring the seven reshaped trees stand as narrow stalk groups beside the four trees still in the old fan.

## For the owner

1. **No screen chooses a shape yet.** `PlayerBuild.ChooseShape(branch, choice, atRestFire)` works and is kept for the run, but
   nothing in the rest menu or the chart calls it. A tester always gets the first shape. The rest-fire screen for it is still to
   be built (the Hand's screen, or a "shape" line in the chart's details at a fire).
2. **Long Shape names overflow the details panel.** "Flame Wave: Cinder Fan or Running Fire" and "Thunderclap: Rolling
   Thunder or Held Thunder" run past the panel's right edge. The careful default is to wrap the title or shorten the Shape mark's
   name to "Flame Wave: Shape". This was left as it is, for the owner's choice.
3. **Lines cross stalks.** On the Lightning tree, some lines from a passive to the Learn it opens cross the neighbouring stalk.
   They still read, but a little busily.
4. **Charged blade timing (Flame Wave Master).** As built, the 0.5 s promise does not end a running clock: the spell has to be
   ready, as the text "its cooldown does not start" implies. If the owner meant the blow to end the wait as well, it is a
   one-line change (`InstantNext` with seconds 0).
5. **Two base spells, one Master (as noted in the code).** With two elements carried, a base spell's Master answers either
   element's base spell. That is the `NEEDS` in `SpellBranches.OwnBranch`.
6. The questions in `phase4_questions.md` are still open, with their defaults in force.

## 2026-11-20: the Metal, Light, Dark and Poison trees merged into main and proven in the editor

**Merged:** `17f73a0` exactly (Metal `b28625c`, Light `8a9727f`, Dark `5a11057`, Poison `17f73a0`), not the branch tip, with
`--no-ff`. Merge commit: `04b6f7e`. The worktree was clean.

**Conflicts:** none in the text. `AfflictionSet.cs` and `PlayerUpgrades.cs` were changed on both sides since the last merge,
in different places, and both sides' changes are in the merged files. The merge added no new scripts, so no `.meta` files.

**Compile:** clean, with 0 errors.

**Playtest Texts** rebuilt first: 89 effects and 383 marks' details (`9b65a37`).

**EditMode suite:** 743 run and 743 passed after the merge; 746 and 746 with the HitStop tests below.

**PoisonBeatTests, the stats no mark gives** (checked in the editor):
- Across the two merges the "every stat is given by some mark" list lost six stats: `PushDistance`, `BoonSeconds` and
  `FreezeSeconds` (Strong Arm, Following Draft and Held Fast, cut with Wind and Ice), then `GuardGrey`, `BurstStagger` and
  `LeapReach` (Braced Frame, Buckling and Long Leap, cut with Metal and Poison).
- No mark gives any of the six now. Each is still read by the game as a multiplier, and with nothing laid on it each
  reads x1, so the game runs at its base numbers.
- The three newly cut marks are retired (`metal.bracedframe`, `poison.buckling`, `poison.longleap`), each with its refund
  of 1.
- So the change is sound. The test still checks the two stats a mark still gives (`ShatterDamage`, `GroundSeconds`).

**Probes** (all in the sandbox, through `runprobe.sh`):
- **`BranchProbe`, in ProvingGround: 110 OK, 0 WRONG.**
  - Fire and Lightning as before.
  - It now also learns Metal's Flung Blade, Light's Glare, Dark's Grasp and Poison's Miasma to their Masters, through
    `PlayerBuild.Attune`. For each it reads the table's wait and the Empower's (6 to 5, 6 to 5, 7 to 6, 7 to 6), wears the
    first shape, chooses the second at a rest fire, and casts the Master.
  - A shape with no form of its own (Cherry Red, The Nail, Gathered, Tethered, Low Mist) is judged by being worn, its
    effects riding the kit's cast. A shape with a form (Seven Horses, The Pit, Censer) casts through its mark in that form.
- **A game bug found by it, and fixed** (`05c1555`, `HitStopTests`):
  - The rest menu opened straight after a blow saved the hit stop's nearly frozen time scale (0.03) and put it back when it
    closed. The hit stop had already let go, so the world stayed at three per cent speed, and every cast then held the
    player thirty times as long.
  - The hit stop now remembers what it wrote when it lets go. When time comes back to that scale, it puts back the scale
    from before the stop.
- **`MarksProbe`, in CombatTest:** runs to done, with errors +0 in every move.
  - The retired Lance the Wound became Five Needles' Spread. It was cast as `wave`, heard 3 bites (one on each dummy hit)
    and laid Poison x6.
  - Great Lotus, Eye of the Storm, Twin Break and Meltdown report "in force False", because the probe holds one element.
    Those mixes behaved this way before this merge too.
- **`UpgradeProbe`, in CombatTest:** runs to done with 0 errors and 0 warnings.
  - No retired id, constant or name is left in it after this merge (checked against `RetiredMarks.All` and the names those
    marks had), so it needed no change.
  - Its V lines buy each tree's first marks. Each worn spell logs "cast +0", which is the probe's own count and is worth a
    look.
- **`ReactionProbe2`: 336 OK, 0 WRONG** (heard 54, shown 54, dropped 0).
- **`CrowdProbe`: 6 OK, 0 WRONG** (frame p95 6.5 ms, mean 4.7 ms).

**The chart, the whole ring** (`review/shots/current/branch_chart_wide.png`, 460 marks): it still reads.
- Eleven coloured fans of stalks stand round the hub, every tree's name is clear, and each element is its own colour.
- It is denser than before. Single marks are dots at this zoom and are read tree by tree.
- Light's and Metal's stalks crowd each other at the lower left.
- The lowest marks and crowns of Metal and Earth run under the bottom bar.

**Not merged: task 4.4 (Devotion, `7c5869b`).** The merge was refused by the session's permission rules, so it was not
made. `DevotionProbe` has not been run, and the kept burn's flames and the HUD during the breath have not been looked at.
It waits for the owner's permission.

## For the owner (this merge)

1. **The whole ring is cut at the bottom**: Metal's and Earth's lowest marks sit under the bottom bar in the wide view. The
   careful fix is to zoom the wide view out a little or start it a little higher. It was left for a look.
2. **Six stats are given by nothing** until the Empowers and Bonds that will give them again are built: `PushDistance`,
   `BoonSeconds`, `FreezeSeconds`, `GuardGrey`, `BurstStagger`, `LeapReach`. They sit at x1.
3. **Shapes without a form** (Cherry Red, The Nail, Gathered, Tethered, Low Mist) change the spell by their effects alone.
   A tester sees the same cast with something added, not a new shape on the floor. That is as built, from the design.
4. The questions for Metal, Light, Dark and Poison in `phase4_questions.md` are still open, with their defaults in force.

## 2026-11-20 (afternoon): Devotion and every phase4 step merged into main and proven in the editor

The owner approved the whole merge. It was made in the order of `phase4_merge_plan.md`, each step `--no-ff` and proven before
the next. Before every step Play Mode was stopped, the editor recompiled (0 errors, 0 warnings), Clover > Build > Playtest
Texts was rebuilt, and the `.meta` files Unity wrote and the new `PlaytestTexts.asset` were committed. There were no
conflicts in the text at any step. Main's bespoke reaction looks survive: `PlayerReactions.Act` still plays
`ReactionLook.Play(pair, at, who, way, transform)` for every reaction, and a shaped reaction plays its plain reaction's look
before its shape's effects.

| Step | Merge commit | EditMode | Probes |
|---|---|---|---|
| 0: Devotion and the RuleBook (`7c5869b`) | `47f6fe0` | 783 run, 1 failed, fixed; then all pass | DevotionProbe 23 OK, 0 WRONG |
| 1: the heart, reaction shapes, Plasma, Sleet Charge (`794a22a`) | `558fe63` | 792 passed | DuoProbe 41 OK, 0 WRONG; ReactionProbe2 336 OK, 0 WRONG (54 heard, 54 shown, 0 unbuilt, 0 dropped); UpgradeProbe and ChartScaleProbe ran clean |
| 2: the ten mixes moved into the heart (`e714b07`) | `0a94abe` | 792 passed | ReactionProbe2 336/0; DuoProbe 41/0; UpgradeProbe (T: the duo mark chosen in the heart and bought); MarksProbe (no errors); the old-save check below |
| 3 and 4: twenty-one new duo branches (`1da3626`) | `aa822fc` | 792 passed | DuoProbe 42/0; ReactionProbe2 336/0; the profiler pass below |

**What was fixed on the way.**

1. `RuleBookTests.ASecondBreathLastsTheLongerAndARestEndsIt` gave a second breath of 1 s over 0.5 s left and expected 0.5:
   the code keeps the longer, as its summary says. The test now gives a shorter second breath (0.25 s) and expects the
   0.5 s kept.
2. `DevotionProbe` took Lightning while all in on Fire and expected a mix; an altar used all in starts again with its
   element alone (`AltarChoice`). It now carries Fire alone first.
3. `MarksProbe` carried one element for Great Lotus, Eye of the Storm, Twin Break and Meltdown, which are duo marks now and
   in force only with both carried. It now carries both elements of a duo mark (all four then worked: Great Lotus's burst
   75 on each, the Eye's 14 bites and 3 bolts, a twin break of 66, a Meltdown).

**A save from before the merge.** No save file exists on this machine (the probes are sandboxed), so `OldSaveCheck` builds
one: every mark id main's trees had before the merge (322 read from git, 258 of them marks) learned, 7 Shards, Plasma Lance
in the Hand and Fire Eater in a kept Hand, written as JSON and read back through the real `SaveStore`. Shards 7 -> 160 (the
153 owed for the 45 retired marks), no retired id left, no live mark lost, both retired spells taken out of the Hands. The
64 old ids that are neither marks nor retired are all shape and condition names (`dark.ash.ashen`, `fire.torch` and the
like), never learned. Forge's Folded Steel folds are a fight's growth now (`e762f14`); a save that had folds keeps none.

**The profiler pass.** `HeartCrowdProbe` (`bash AgentScripts/profile_heart.sh`): the Proving Ground's twelve penned enemies
round the player, two afflictions laid every half second, every duo branch's mark learned (188) and every reaction shape
chosen (33), the carried pair turned every 6 s through The Grudge, Ill Wind, Kindling Point and Plasma. The p95 frame is
6.0 ms without the profiler (8.6 with it) and the reaction queue peaked at 15 of 64, so no runaway cascade. Garbage is 8194 B
a frame, the editor included, against 8051 B for this morning's `CrowdProbe` without the heart. In the 600-frame capture
(`review/checks/heart_crowd.data`) every Clover marker allocates nothing except `SpellDiamondHud.LateUpdate` (169 B a frame on
average, 2708 B at most), `PlayerReactions.LateUpdate` (4 B), `Hazards.Update` (3 B) and `LoadoutChoice.Update` (3 B).
`SpellDiamondHud` was not changed by the merge; whether its share is the pair swaps rewriting the diamond or something every
frame has not been looked at yet.

**The heart on the chart** (`review/shots/current/duo_heart_sleet.png`, `duo_heart_plasma.png`) reads: a small upright
diamond of five marks in the middle of the ring, the pair and its reaction named over it ("Lightning and Ice: Sleet
Charge"), clear of every tree's spokes. With one element carried it is empty (`chart_fire_ring.png`).

## 2026-11-20: the owner's decisions, begun

**Decision 1, three keystones: done.** The Closed Ring, All Blades Home, Two Suns and Plague Bearer are keystones again under
their old ids, taken out of `RetiredMarks`, so a save that had one keeps it and is refunded nothing. Each stands at the top
of its trunk (`T.2`), grown from the Master of the middle bough's second spell (Cairn, Grave of Swords, Ring of Spears, One
by One): the one empty place whose lines pass 0.55 steps or more from every mark, and none of them grows from another
keystone. They are built as their old stand-ins were, with "Not built yet" in the details for the ring as a wall, the
blades that never fade, and the figure of light. Two Suns' price is now "your spells wait a fifth longer" in place of the
mana it used to cost (phase4_questions.md, Light 1). The chart test allows one to three keystones in a reshaped tree;
Earth, Metal, Light and Poison have ten passives each. EditMode 792 passed; Playtest Texts 495 marks' details;
`chart_earth_crown.png` shows The Closed Ring beside the Devotion, its line down to Cairn's Master.

**Decisions 2 to 6 have not been started.** The coordinator stopped the work after decision 1 so that a pass on the
reaction looks (`docs/design/feedback/2026-11-20_owed_audit.md`) can go first.

## For the owner (this merge)

1. **DuoProbe plays only Flashover and Sleet Charge.** The twenty-one distance 2 and 3 branches pass the chart and shape
   tests and ran in the crowd without a runaway, but none has been played line by line, as the plan asks before
   playtesters see them.
2. **A shaped reaction plays its plain reaction's look.** Corona, Fulgurite, Thundersnow and Glass Spear draw their own
   pieces on top, but no shape has a look of its own from the combos 4 pass.
3. **Where the three new keystones stand** (top of the trunk, grown from a spell's Master) is the careful default. The
   other empty place, `L.mid`, would sit on the line from the left foot to the left bough's second spell.
4. Decisions 2 to 6 (the spare mixes as hidden marks, the Rise on Dark's root, no duo crowns from gods, the shape screen,
   the enemy's-blow moment) wait for the reaction pass.

## 2026-11-20 (evening): steps 5 and 6 merged, and the owner's decisions 2 to 6 built

**Merged:** `9c0eaa8` (task 4.17: pairs_d4 and pairs_d5, twenty-two duo branches, DuoProbeAll, the 4.17 questions) with
`--no-ff`. Merge commit: `b1b2a45`. There was one conflict, in `content_open.tsv`: both sides had added rows after
`turn-to-face`. Both sides' rows were kept: phase4's longer `depth-passed` and `quickened-beat` rows, main's three keystone
rows and phase4's seven new rows. Compile clean. Playtest Texts rebuilt (89 effects, 610 marks' details). The twenty-two
`.meta` files are committed (`97a1647`). Every one of the 55 pairs now has its branch in the heart.

**EditMode:** 793 run and 1 failed after the merge. New Face said "gives 6 mana." and the chart test read that as a price.
It now says "gives you 6 mana back", as every mark that gives mana does (`9e54217`). After all the work below: **802 run,
802 passed.**

**DuoProbeAll** (ProvingGround, about eight minutes, read whole with `console get -- --tail 700`): **333 OK, 1 WRONG**
after the merge.
- The one WRONG was Red Hot in Slag, which read shape `''`. Slag's own condition was held on the host, so the shape had
  run. Then Chokedamp's leftover poison ground made Fire Medicine react on the same host in the same frame, and the probe
  read `LastShape` after that second reaction.
- The probe now notes such a second reaction and reads its own pair's shape from the table. The PlayerPrefs key
  `clover.probe.duoall.only` runs only the pairs named.
- Re-run on Fire Medicine, Chokedamp, Red Hot and The Bolt Runs True: 28 OK, 0 WRONG.
- Two pairs reacting on one body from one laying is real play, not a fault.
- **After all six commits below, DuoProbeAll ran whole again: 334 OK, 0 WRONG.** Four lines noted a second pair shown on
  the same host in the same frame, each from ground an earlier pair left.

**ReactionProbe2: 336 OK, 0 WRONG** (heard 54, shown 54, dropped 0), after the merge and again after the Grudge change.
**CrowdProbe: 6 OK, 0 WRONG** (p95 5.7 ms, mean 4.4 ms, 346 reactions shown, no runaway).

**The watch items (merge plan item 8).** `WatchProbe` ends at 16 OK, 0 WRONG. Fixes are in `14ba73c`.
- **Cold Damage never fired**, as the plan feared. The heavy blow's `Shatter()` ends the Chill inside `TakeHit`, and the
  answer runs later, asking for `Needs = Chill, NeedsFull`.
  - Fix: a blow that breaks the ice is now told as `Shattered` (in `ConditionNews`, `EnemyConditions.Struck` and
    `AfflictionSet.TakeFrom`), and Cold Damage answers `StruckBy.Shatter`.
  - In play: four doses were set off on the shatter (55 for the shatter and the blow, 79 in all). A heavy blow on the
    enemy unfrozen set none off.
- **Six Levels Down did nothing.** A Chill laid on a frozen enemy is refused, because the freeze is already full.
  - Fix: a new action, `KeepFrozen` (`AfflictionSet.KeepFrozen`), keeps the freeze 0.3 s longer for each bite.
  - In play: the freeze lasted 2.50 s without the mark and 2.80 s with it.
- **A reaction laid and then forced in one frame was shown twice.** This is the pattern of The Sisters' Quarrel, Cold
  Chisel, Held Down and Diamond Guard: `ReactionEngine.Force` went through the rest.
  - Fix: the engine now remembers what it found this frame (in fixed arrays, so nothing is allocated). A force on the
    same enemy and pair in that frame is the same single reaction. In a later frame a force goes through the rest as before.
  - Tests: ReactionEngineTests, and ReactionHookupTests, whose same-frame force was moved a frame on.
- **Fast Water** is bounded: its bite does not feed itself.
  - Its 0.5 s cooldown is shared by every enemy. In the yard, where other dummies are poisoned and Soaked, it answered
    the host's first bite but not its second.
  - That is the ledger's stand-in ("once in half a second for all").
- **Runaway cascades:** none seen in CrowdProbe or DuoProbeAll.

## The owner's decisions 2 to 6

| Decision | Commit | Proof |
|---|---|---|
| 2. Spare spells kept hidden for the Evolutions | `c2ee406` | SaveMigrationTests: a save keeps Plasma Lance and Fire Eater, refunds nothing and takes them out of its Hands; each of the twelve is hidden, never sold, never open, never slotted, never in force and never drawn. An InForceTests case. |
| 3. Dark's Rise on the root | `9af1ed2` | RiseProbe (CombatTest): 9 OK, 0 WRONG |
| 4. Gods give no duo crowns | `6c3b874` | UpgradeChartTests `EveryElementHasItsTreeAndItsGiftAndNoGodGivesADuoCrown` |
| 5. The shape screen | `5667873` | ShapeChoicesTests, RestChoicesTests, ShapeProbe 15 OK, 0 WRONG; frames `review/shots/current/shape_screen.png` and `shape_screen_pad.png` |
| 6. The enemy's-blow moment | `3f51389` | ConditionPiecesTests, GrudgeProbe 13 OK, 0 WRONG; ReactionProbe2 336 OK, 0 WRONG |

### 2. Spare spells

- The twelve are hidden marks in `Trees/SpareSpells.cs`, as they stood before the reshaping, with their timings: Plasma
  Lance, Storm Overhead, Whiteout, Creeping Ice, Lotus Step, Thorn Wall, Trip Hammer, Sun Wheels, Black Sun, Grave Soil,
  Fire Eater and Lit Fuse.
- No d2 to d5 branch cut a spare spell; phase4_questions.md says so for each task.
- `MarkRules.InForce` and `Slottable` refuse hidden marks.
- The twelve are out of `RetiredMarks`, so a save keeps them with no refund. `SaveMigration.ClearHidden` takes them out
  of the Hands, since they can sit in no slot.

### 3. The Rise

- `PlayerUpgrades.Rise` asks for Dark's root, Curse.
- The Rise's cast names the root, so forgetting the root ends the Rise.
- RiseForm's finisher buys the Rise back only with A Life for a Life. A Rise that is not bought back stays spent until a
  real death.
- Curse's words, the gift's words, dark.md, the guide and the ledger say so. The ledger row `rise-on-root` is taken out.

### 4. No duo crowns

- The eleven old crowns are `Mix` marks that cost 5 Shards and open once both elements are carried.
  - Each grows from its branch's duo spell.
  - Six Mirrors and Light the Pyres move to the duo spell's place and grow from their branch's first two marks. They still
    cost 60 mana to cast.
- `Gifts.CrownOf` is gone. A consumed god (`GodReward`) gives only the 4 Shards.
- The CONSUME NEXT shrine gives the 4 Shards and no crown. Its prompt is set when it wakes, because the scene still holds
  the old words.
- "The crown" is gone from the duo branch files.
- Texts updated:
  - the details texts;
  - GUIDE.md section 5, now "Gods' gifts and duo branch tops";
  - a dated note under GAME.md's "Gifts are by rule".
- "An element's base spell comes free with its element" holds. The same test now checks it: each base Learn is given and
  never sold.

### 5. The shape screen

The screen is `UI/ShapeMenu.cs`, with `ShapeChoices.cs` and `ShapeRow.cs`. At the rest fire it is the **Shapes** entry,
after Set the Hand.
- **The list** shows every learned spell's Shape mark, then every learned reaction's, each with the shape worn.
  - A line whose shape is kept but not in force says "(not in a slot)" or "(both elements not carried)".
- **The right side** shows the chosen line's two shapes in full, with the worn one lit.
- **The controls:**
  - W S, the wheel, the stick or the d-pad choose a line.
  - A D, left and right, use or a click wear the other shape, through `ChooseShape(key, i, atRestFire: true)`.
  - Dodge, Esc or Start go back to the fire.
- **ShapeProbe** drove the screen with the keys and with a pad (d-pad up to choose, south to wear, east to back out).
  After leaving the fire, Steam ran as Scald.
- **The frames** read well, and the hint names the pad's own buttons (A, B) when a pad is in hand.

### 6. The enemy's-blow moment

- There are two new moments:
  - `UpgradeTrigger.TheirBlow`, for marks. Its target is the attacker.
  - The condition moment `Attacked` (`UpgradeTrigger.ConditionAttacked`), with `ConditionBlow.TheirBlow` to spend a
    condition on it.
- Both are told whenever an enemy's blow lands, is guarded, is perfectly guarded or is turned by a ward.
- **The plain Grudge's cloud** now waits for that blow, then strikes. The enemy's Static is discharged at double (with
  3 Static it lost 42), the enemy is held 0.6 s, and the cloud is spent. A guarded blow and a warded blow do the same.
- **Ash Plume's storm** falls on that blow, or when its 6 s run out.
- **Left in the ledger row** `their-blow-moment`: the other marks there need the moment a blow is *about to* land, which
  is a different piece. They are the shared movement rule, Coiled Strike and Daybreak's Master.

## For the owner (this pass)

1. **Fast Water's half-second is shared by all enemies.** In a crowd of poisoned, Soaked enemies it adds about two bites a
   second in all, not "twice as often" on each. A cooldown kept for each enemy would match the words. It is left as the
   ledger's stand-in.
2. **Ash Plume and the cloud.** Ash Plume's storm is not capped at five flashes (it makes one per weapon blow on the
   cloud). The cloud does not drift to the next enemy when its host dies. Both stay in the ledger.
3. **The Exile Returns** still answers any discharge on a Cursed enemy, not only the Grudge's own bolt.
4. **The rest menu has five rows.** Once the Way and the hub are built it will have six entries (the Hand, Shapes, the
   chart, the Way, the hub and leave). `RestMenu.MostRows` and the note's place will need a sixth row then.
5. **The CombatTest scene still holds the shrine's old prompt.** It is replaced when the shrine wakes. Rebuilding the scene
   with its builder would store the new words; the scene was not rebuilt here.
6. **Halo and Pyre in the heart** now have their duo spell at the spell's place and nothing at the crown. This has not been
   looked at on the chart.
7. **The Grudge's cloud has no look of its own yet** (effects.md: "Not yet: the cloud itself").

## 2026-11-20 (night): the owner's answers built offline on `phase4b` (not merged yet)

Branch `phase4b`, from `main` at `7bc85df`. It was built with no editor, and every commit compiles clean in all four
assemblies. The EditMode fixtures that run outside the editor pass: 708 in all, with the same 114 editor-only failures as
`main`. The merge steps, fixtures and probes are in `phase4b_merge_plan.md`. `// NEEDS` lines went from 493 to 455.

| Task | Commit | What was built |
|---|---|---|
| 1. Six keystones | `1bbd191` | See "The six keystones" below. |
| 2. `reaction-numbers` | `234bb48` | A duo mark changes its own reaction's numbers. `ReactionNumbers` keeps copies of the reaction's effects and of its shapes' effects, one copy for each set of changing marks in force, in the original slots. 18 minors are finished. See below for the 7 that need other pieces. |
| 3. `duo-surge-slot` | `4ec7497` | Each of the nine duo surges is its own surge in the Hand's surge slot. It is chosen at a rest fire while both elements are carried. It has its own kit for its own mana and time, and is in force only while worn. Open the Gourd lasts a second longer for each capsize, to 15 s. Ash and Thunder lasts a second longer for each kill. |
| 4. `form-pieces` | `2b52de6` | See "Form pieces" below. |
| 5. Empowers | `3581206` | See "Empowers" below. |
| 6. Four marks kept | `48171c0` | See "Four marks kept" below. |
| 7. The deep strike | `1e07c7f` | It needs the element's Devotion bought (`StrikeRules.IsDeep`). Without it, R2 alone or all in is the plain infused strike and costs no mana. The HUD, the guide and the Devotions' texts say so. |
| 8. Smaller changes | `552fdf4` | See "Smaller changes" below. |
| Review | `83dde33` | Nine fixes from a code review, and `Phase4bProbe`. |

### The six keystones

- **Into the Fire.** A dodge toward the enemy you face (within 60 degrees) is safe for its whole length and goes through
  bodies. It sets everything on its path burning two deep. A perfect dodge toward the enemy raises Heat a level. A dodge
  away costs twice the stamina and is never perfect.
- **The Blade Drinks Fire.** Fire's root lays nothing with the weapon. Burns do not bite. Each blow takes Burn off a
  burning enemy and hits 6 harder for each: one for a light blow, three for a heavy blow, all for a finisher.
- **High Water, Low Water.** A tide clock turns every 6 s, and a perfect guard or dodge turns it at once. High water: spells
  x1.33 and the weapon x0.8. Low water: the other way round.
- **Drawn Back to Break.** Each dodge away banks a wave, three at most. The next blow breaks them all as a 4 m cone that
  soaks and throws. With no wave held, the weapon is 20% softer (a count's empty state).
- **Shared Roots (`life.sharedroots`).** A third of each wound goes to the Seeded enemies within 8 m, split between them.
  Blows on Seeded enemies no longer heal you.
- **In Its Season.** A seed left alone for 6 s ripens and stops fading. A ripe burst is three times as strong and grows a
  thornbush (`RipeBurst`). An unripe burst is half as strong. A burst means set off, its bearer's death, or a reaction.

### Smaller changes (task 8)

- The heart's picker steps through all ten partners.
- Fast Water quickens each Soaked enemy's own poison clock: it bites twice as often and fades twice as fast.
- The Exile Returns and Given Bones answer only the Grudge's bolt, in any of its three shapes (`AnyMark`). Given Bones adds
  14 a Static.
- Ash Plume's storm is capped at five bolts.

### Empowers (task 5)

- The "a second less" defaults are undone, and the chart test lets an Empower strengthen or shorten.
- Empowers can now change their Learn's own effects (`empower-effects`) or lay their numbers on the Learn's spell alone
  (`LearnOnly`).
- This finishes Poured Bronze, Reckoning, One by One, Daybreak, Five Needles, Returning Edge, Cairn, Reap What Is Sown,
  Drawn From the Wood, Devouring Maw, Gone to Ash, Debts Called and Second Shadow.

### Four marks kept (task 6)

- Hammer Down and Clean Break are in the empty middle place of Wind's and Ice's right boughs. Indrawn Breath and Ski Run now
  grow from them, as the pattern grows those stalks: a line from the foot would run over the kept mark, and the ring check
  refuses that. So those two spells are 3 Shards dearer to reach. Hammer Down's heavy blow on an enemy in the air also hits
  half again as hard.
- Spilled Water and Shade are at a new place, the top of the trunk (`T.3`). Water and Life had no empty place left after
  their keystones.

### Form pieces (task 4)

- A spell's later presses: `Upgrade.Presses`, done by the slot's own button within its window.
- A change of places (`SwapPlaces`), a step straight back (`StepBack`), and a moment untouchable (`Untouchable`).
- Three Weasels now has all three presses.
- Under the Black Water now changes places. Elites and gods stay put and you rise behind them. Its pool curses, and your
  next blow hits half again as hard.

### What is left (all in the ledger)

- **Reaction-numbers marks that need other pieces:**
  - Goes Up (`reaction-numbers-when`)
  - Held Down and What the Yew Keeps (`reaction-start-count`)
  - Clear Sky (`mark-blows-on`)
  - Bellows and Wind Sickle (`whirl-fed`)
  - Bright Venom (`poison-held`)
- **Empowers:**
  - Standing Light's does nothing until `in-own-ground`.
  - Flung Blade's now does nothing until `base-spell-marks`, since its shorter wait was the only part that worked.
  - The `empower-after-shape` row is left for the rest.
- **Second presses:** those a Shape or a Master adds, those that act on a thing the spell left standing, and Fire-Setting.
  Fire-Setting needs a brand that lays the fire without a blow setting it off.
- **The other duo spells' form pieces.** They stay in `form-pieces`, each with what it needs.
- **The Yawning Gap:** alternating blows (`alternate-blows`).
- **Looks**, in the `looks` row:
  - the bud and flower of a ripe seed
  - the tide ring and HUD sign
  - the blade drinking fire
  - Shared Roots' roots
  - the waves behind the shoulders
  - Into the Fire's lunge
  - duo surges have no look of their own and wear their first element's

## For the owner (this pass)

1. **The top of the trunk.** Spilled Water and Shade are at a new place, `T.3`, at (0, 9.75). It grows from the middle
   bough's crown (Spreading Water, Root and Branch), because their trees had no empty place left after the new keystones.
   Alternatives: make one of them a minor somewhere else, or cut it again.
2. **Indrawn Breath and Ski Run are dearer.** They now grow from Hammer Down and Clean Break, so each is 3 Shards dearer
   to reach. Growing them from the foot as well would draw a line over the kept mark, which the ring check refuses. The
   other choice is to put Hammer Down and Clean Break somewhere else.
3. **Readings of the rosters**, as built:
   - Grit: 6 a cut, not 3, and the whirl about 4 s.
   - Green Wood: torn pits stand 8 s, not 5.
   - Riding High: the farthest feather lands 9 m beyond the enemy.
   - Wide Fan: Prism's five rays reach 7 m beyond the enemy.
   - Reckoning's Empower: 10 a blow, 180 at most (60% of the approved 8 a blow).
   - Daybreak's Empower: 10 a ring on the beam only.
4. **Prices undone.** Daybreak's and One by One's price defaults were undone too, as the roster gave only stronger numbers
   for them.
5. **Duo surges start with no affliction of their own on the blows.** Their marks do everything.
6. **Three Weasels' clock** starts at the first press. A press refused (nothing to do) ends the run of presses.
7. **What "toward" means for Into the Fire and Drawn Back to Break.** A dodge counts when it is within 60 degrees of the line
   to the enemy faced: the one ahead, or else the nearest, within 12 m. A dodge to the side is neither.

## 2026-11-20 (late night): `phase4b` merged into main and partly proven in the editor

**Merged:** `phase4b` (12 commits, `1e07c7f` to `9ff2272`) with `--no-ff`. Merge commit: `f4ecf80`. There were no conflicts in
the text. Both sides touched `PlayerCombat.cs`, `PlayerUpgrades.cs`, `effects.md` and `carriedembers.md`, and git merged them
cleanly. Main's effects fixes (`1d990fc`, `b863f3c`) survive, such as the tinted heavy-blow star, no affliction-full ring
under the new art, and Carried Embers' column sparing its target. Reaction-numbers copies are made with `MemberwiseClone`,
so they carry main's `SparesTarget` too.

**Compile:** 0 errors, 0 warnings. **Playtest Texts** rebuilt, with ten new entries: Fire +2, Ice +1, Life +3, Water +3 and
Wind +1. The three `.meta` files and the new `PlaytestTexts.asset` are committed (`2f96a1b`).

**EditMode.** The merge plan's fixtures all passed:

| Fixture | Passed |
|---|---|
| KeystoneRuleTests | 15 |
| UpgradeChartTests | 31 |
| SpellBranchTests | 12 |
| DuoBranchTests | 10 |
| HandRulesTests | 18 |
| InForceTests | 53 |
| PoisonBeatTests | 11 |
| SaveMigrationTests | 12 |
| RuleBookTests | 19 |
| MarksBuiltTests | 9 |
| SpellCooldownTests | 14 |
| SpellSlotsTests | 10 |
| ReactionCatalogueTests | 6 |

The full suite: **822 run, 822 passed.**

**Probes.** Each probe below had at least one WRONG on its first run. All of them were faults in the probe; the code was
right.

- **Phase4bProbe: 32 OK, 0 WRONG.** It first ran with 30 OK and 2 WRONG.
  - **Drawn Back to Break: "waves 1".** A roll turns the player the way it goes. After the first dodge south, the player
    faced the afflicted dummies 4 m away at z -5, so the second dodge counted as toward them. The probe now turns the player
    back to the host before the second dodge.
  - **Three Weasels: "comes out beyond the enemy".** The first press is the step form, which already steps through the
    enemy (z 2 to 7, the host at 6). The second press then passes back through it, to z 3. The probe now checks that the
    player ends on the other side of the enemy from where the rush left them.
- **GrudgeProbe: 16 OK, 0 WRONG.** It first ran with 15 OK and 1 WRONG.
  - **"Seven blows call down five bolts".** The probe read the host's health. But `Falling` drops its first bolt on the
    enemy and the rest round it, so the health shows one bolt whatever the count (31 lost with three blows and with seven).
    The probe now counts the bolts as they are laid: **5 laid from seven blows**.

**Put off by the owner's change of priority** (the effects build needs the editor), and still to run: P1bProbe, P2Probe,
DuoProbe, MarksProbe, and the end-of-job regression of ReactionProbe2, DuoProbeAll and CrowdProbe. DevotionProbe and the
`HeartCrowdProbe` profiler pass from the merge plan have not been run either.

## For the owner (this merge)

1. **Three Weasels' first weasel steps through the enemy.** It is the plain step form, so the "rush to the enemy that throws
   it" ends 1 m beyond the enemy. The second weasel's "pass straight through" then runs back the way you came. To make the
   first press stop in front of the enemy, it would need a rush of its own.
   **Answered 2026-11-20 (evening): give it a rush of its own that stops in front; the second press goes through.** Built
   on `phase4c` (below).
2. **"The enemy you face" after a roll away.** A roll turns you the way it goes. A second dodge away is measured against
   whatever is ahead of you then, and only against the first enemy if nothing is ahead within 12 m. In a crowd, two dodges
   back from one enemy can count as one away and one toward another. Drawn Back to Break and Into the Fire both read it.
   **Answered 2026-11-20 (evening): remember the target until it dies or you get far from it.** Built on `phase4c`, with
   16 m as "far" (below). Then also: **let it go after 3 s without a dodge.**
3. **Drawn Back to Break's throw** was not seen in the probe: the still dummy cannot be shoved, so "moved 0.0 m". The cone
   did soak it and spend the waves.

## 2026-11-20 (evening): `phase4c`, the owner's two answers

Built offline in the worktree `Clover-phase4c` (branch `phase4c`), while the editor was busy with other work. Not yet run
in the editor.

**Three Weasels' first press stops in front.** A new bespoke form, `rush` (`RushForm`, a `StepForm` that stops short and
keeps facing the enemy). It stops `FormRules.RushShort` (1.2 m) in front of the enemy faced, within the spell's 5 m reach,
and cuts on into it. It still throws the enemy 2 m and lays two Unsteady. The second press is unchanged: straight through
to 3 m beyond (`FormRules.MarkStep`, which `UpgradeAction.Step` now uses). In the probe's layout (you at z 2, the enemy at
z 6) the first press now ends at z 4.8, and the second at z 9. The rush still promises a critical heavy blow when it lands,
as the step did. Its look is the step's streak and cut, stopped short: a stand-in, in the ledger's `looks` row.

**A run of dodges remembers its enemy.** `DodgeHold<T>` and `DodgeRules.StillHeld` (in `DodgeWay.cs`). The first dodge
holds the enemy it was measured against (the one ahead, else the nearest, within 12 m). Every later dodge is measured
against that enemy while it lives and stands within `PlayerUpgrades.DodgeHoldRange`, 16 m (`DodgeRules.HoldRange`: the
fight's 12 m and most of a 4.5 m roll). When it dies or the player gets farther, the next dodge takes the enemy faced then.
The one helper (`PlayerUpgrades.WayOf`) feeds both readers of a dodge's way: `OnDodge` (Drawn Back to Break's waves, Into
the Fire's lunge and its perfect-dodge Heat) and `DodgeTermsFor` (Into the Fire's cost and safety). No other mark reads it.
The owner then added a clock: the hold also ends 3 s after the last dodge (`DodgeRules.HoldSeconds`, exposed as
`PlayerUpgrades.DodgeHoldSeconds`, read on `Time.time`; every dodge starts the 3 s again). After a longer pause the next
dodge takes whoever is faced then.

**Tests.** `KeystoneRuleTests` gains `TheEnemyFirstDodgedFromStillCountsInACrowd`,
`TheHeldEnemyIsLetGoWhenItDiesOrThePlayerGetsFarFromIt` and `TheHeldEnemyIsLetGoAfterThreeSecondsWithoutADodge` (held
2.9 s after a dodge, let go 3.1 s after); `DuoBranchTests` gains
`ThreeWeaselsFirstPressStopsInFrontAndTheSecondPassesThrough`; `SpellFormTests` knows the new form.

**Probe.** `Phase4bProbe` drops the teleport back to the host before Drawn Back to Break's second dodge, and judges that
the host is held and the second roll south counts as away. Three Weasels now judges that the first press stops just in
front (0.6 to 2 m short, on the near side) and the second comes out beyond.

**Still to do in the editor:** compile, the three fixtures, `Phase4bProbe`, and commit the `.meta` Unity writes for
`RushForm.cs`.
