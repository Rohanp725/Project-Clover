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
