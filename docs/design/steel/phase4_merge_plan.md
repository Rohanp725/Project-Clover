# Phase 4: merging the phase4 branch

Everything on `phase4` after the Devotion merge (7c5869b) was written offline: every commit compiles clean in all four
assemblies with the offline compiler and passes the scratch ring check, but none of it has been run in the editor, so
no test result and no probe line below has been seen yet. The branch is one straight line on 7c5869b, so each step
below is a merge of `phase4` at the step's last commit. Nothing can go in before the owner allows the Devotion merge.

## Before every step

1. Stop Play Mode before merging; never recompile during play (the 48 GB hang).
2. `unity --json recompile --timeout 120`, then `unity command console get -- --tail 30 --level error`.
3. **Clover > Build > Playtest Texts** (`editor invoke_menu -- --path "Clover/Build/Playtest Texts"`). Every step changes
   `docs/design/playtest/effects.md` and `marks/*.md`, and `Generated/Playtest/Resources/PlaytestTexts.asset` on the
   branch is stale: until it is rebuilt the chart shows old or missing details, and PlaytestModeTests reads it.
4. Commit what Unity writes: the `.meta` of each new script (none were written by hand: `HeartLayout.cs`,
   `Trees/HeartPattern.cs`, the folder `Trees/Duo/` and its 22 files, `Tests/EditMode/Player/DuoBranchTests.cs`), and
   the rebuilt `PlaytestTexts.asset`.
5. The whole EditMode suite: `unity command test run -- --mode editor`. The fixtures named below are the ones that step
   touches; run them first with `--filter` to fail fast.
6. Probes through `bash AgentScripts/runprobe.sh <Scene> <Probe> <seconds>`, which keeps saved progress sandboxed.

## The steps

| Step | Commits (oldest to tip) | What goes in |
|---|---|---|
| 0 | 7c5869b | Devotion and the RuleBook (task 4.4). The owner's call. |
| 1 | a6268ce, 1574885, 0ff477d, a26bd58, 57a8096, 794a22a | Task 4.5: the heart (HeartPattern, HeartLayout), MarkRules opening any pair, reaction shapes in Loadout and ReactionTable, Plasma reshaped, Sleet Charge, the heart drawn in the menu, DuoProbe. |
| 2 | 4cddf0a, 50e75b6, 06512d0, 1aad589, 7039106, e762f14, 81d0153, d46890b, 427e3b1, e714b07 | Task 4.15, distance 1: the ten built mixes moved into the heart (Tempest to Pyre), with their RetiredMarks. |
| 3 | a20202a, 6291de1, 00bb462, f969dfa, bfee09c, 61cd576, 7ef5b3f, 01d23ff, 70fc887, 2e2ebe3, fbec371, ac27505 | Task 4.15, distance 2: ten new duo branches, the 4.15 questions, two wording fixes. Data only. |
| 4 | 78b7a16, 8ab98e5, 208ec95, 1b3fb62, c8d2c7e, 5b916e9, 0bcd4d7, fc5918c, 407f777, 165d445, 0b1254e, 0de1fa4, and this file | Task 4.16, distance 3: eleven new duo branches, the 4.16 questions. Data only. |

Steps 3 and 4 add only data (`Trees/Duo/*.cs`, one line each in `UpgradeTrees.cs` and `SpellTimings.cs`, docs), so if
step 2 is sound they may go in together.

## Fixtures and probes at each step

- **Step 0.** The whole suite, with KeystoneRuleTests, PlaytestModeTests and SaveMigrationTests first. DevotionProbe.
- **Step 1.** DuoBranchTests (new: any pair opens, the heart draws only duo branches, a shape is worn instead of the
  plain reaction, each shape answers only its own conditions, Flashover and Sleet Charge change with the shape),
  UpgradeChartTests (the heart's pattern is spaced and walked from every tree), HandRulesTests (duo spells and the
  surge in the Hand), SpellSlotsTests, InForceTests, ReactionCatalogueTests, MarksBuiltTests. Probes:
  `runprobe.sh ProvingGround DuoProbe 120` (Flashover plain, Corona, Fulgurite, the swap at a rest fire, Sleet Charge's
  shapes, Glass Storm from a slot, the heart framed in `review/shots/current/duo_*.png`: look at every frame);
  `runprobe.sh ProvingGround ReactionProbe2 160`, which must still pass all 49 plain reactions; UpgradeProbe in
  CombatTest; ChartScaleProbe for the chart at each size.
- **Step 2.** UpgradeChartTests (no mix is left on the ring, prices in range), InForceTests, SaveMigrationTests and
  MarksBuiltTests (the RetiredMarks ids keep old saves loading and refunded), PoisonBeatTests (Blight, Pyre), DuoBranchTests.
  Probes: ReactionProbe2, UpgradeProbe (its expectations were changed in 06512d0 and e714b07), MarksProbe, DuoProbe again.
  Load a save from before the merge and check its Shards and marks.
- **Step 3 and 4.** DuoBranchTests and UpgradeChartTests carry every new branch through their sweeps (each shape's
  conditions its own, each branch on the heart's places); HandRulesTests, SpellSlotsTests (each duo spell has a
  timing). DuoProbe covers only Flashover and Sleet Charge: before playtesters see a distance 2 or 3 branch, give each a
  line in DuoProbe (plain, then each shape), most of all the patterns below that no built mark used before. A profiler
  pass (`diagnostics profiler_record` in a crowd fight with the playtest's unlock-all and several heart branches learned)
  checks that the extra answers to every blow allocate nothing per frame.

## The commits to watch

1. **a6268ce**: every reaction now asks `ReactionTable.WornShape` and `EffectsFor` before it runs, in PlayerReactions,
   PlayerUpgrades, SpellBranches and MarkRules. A fault here silences or doubles every reaction. ReactionProbe2 is the test.
2. **a26bd58**: the heart drawn in UpgradeMenu, upright in its own layer, with the knot and the shoulder buttons. Never
   seen. Look at it with one element, with each pair carried, and in the playtest with a pair picked.
3. **57a8096**: ConditionCaster answering the conditions a shape sets (AnswersOf). If it misses one, a shape's condition
   is laid and never answers.
4. **06512d0**: Glacier, with a change to PlayerUpgrades (Flash Freeze) and to UpgradeProbe.
5. **e762f14**: Forge's Folded Steel folds no longer kept for the run (`Kept = false`); a save that had folds loses them.
6. **427e3b1**: Wither made a minor, and UpgradeChartTests' price range widened to allow it.
7. **Patterns no built mark used before** (steps 3 and 4): a condition set from inside another condition's answer
   (Kindling Point's lens, Ill Wind's coat), Falling multiplied by the blows a condition took (Ash Plume), a plain mark's
   Reacted starting a beat (No Still Air), and marks answering every set-off of Static on the Cursed (Given Bones, The
   Exile Returns) beside Ash and Thunder, which sets Static off itself. Each is bounded by a cooldown or a reaction's
   rest, but a runaway cascade is the thing to look for.

What is open for the owner is in `phase4_questions.md` (tasks 4.5, 4.15, 4.16); the engine pieces the `// NEEDS:` lines
wait for are in `content_open.tsv`.
