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
