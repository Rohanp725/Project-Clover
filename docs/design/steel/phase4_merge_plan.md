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
   `Trees/HeartPattern.cs`, the folder `Trees/Duo/` and its 44 files (22 in steps 3 and 4, 22 in steps 5 and 6), `Tests/EditMode/Player/DuoBranchTests.cs`), and
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
| 4 | 78b7a16, 8ab98e5, 208ec95, 1b3fb62, c8d2c7e, 5b916e9, 0bcd4d7, fc5918c, 407f777, 165d445, 0b1254e, 0de1fa4, 1da3626 | Task 4.16, distance 3: eleven new duo branches, the 4.16 questions, this plan. Data only. |
| 5 | 1345984, 94ea497, 7347be4, affae6f, 8649487, 360ba2b, 7b9f962, e5b8b1c, 514dce2, 794e804, 3804f02 | Task 4.17, distance 4 (pairs_d4.md): eleven new duo branches, Steam to Venom Rime, with three duo surges (Heated Bronze, The Net of Jewels, Wind From the Grave). Data, and a comment in DuoBranchTests. |
| 6 | a84fab1, 6eac8de, bf083d4, ab7f73f, 35812bf, e867b7f, 98238c2, 79fa97c, f74e890, 7cc8953, bebfde2, f73b7da, and this file | Task 4.17, distance 5 (pairs_d5.md): eleven new duo branches, Firebloom to Fish Poison, with two duo surges (When the Watch Sleeps, Three Winters); DuoProbeAll; the 4.17 questions. Data only, and a probe. |

Steps 3 to 6 add only data (`Trees/Duo/*.cs`, one line each in `UpgradeTrees.cs` and `SpellTimings.cs`, docs), so if
step 2 is sound they may go in together; after step 6 every one of the 55 pairs has its branch in the heart.

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
  timing). DuoProbe covers only Flashover and Sleet Charge in depth; DuoProbeAll (step 6) gives every branch a line, plain
  and then each shape, so run it once all the steps are in (or after step 4 for distances 1 to 3: a pair whose branch is
  not in yet is judged WRONG on its first line and skipped). A profiler
  pass (`diagnostics profiler_record` in a crowd fight with the playtest's unlock-all and several heart branches learned)
  checks that the extra answers to every blow allocate nothing per frame.
- **Step 5 and 6.** The same sweeps (DuoBranchTests, UpgradeChartTests, HandRulesTests, SpellSlotsTests: Tipped
  Crucible is the one duo spell of these paid in mana, 35, inside the 30 to 60 range), and DuoBranchTests'
  `AReactionWearsItsChosenShape...`, whose test Shape mark for Steam is now gathered after the game's own Steam branch
  (ReactionTable keeps the last Shape mark of a pair, so it still stands in; the comment says so since 1345984).
  ReactionCatalogueTests and ReactionProbe2 again, since a Shape mark exists now for every pair. Probes:
  `runprobe.sh ProvingGround DuoProbeAll 200` (about eight minutes; it writes about 390 lines and runprobe.sh prints
  the last 150, so read the rest with `unity command console get -- --tail 450` and grep for WRONG), then DuoProbe. The
  profiler pass of steps 3 and 4 with a d4 and a d5 branch learned too (Fast Water, below).

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
8. **Patterns no built mark used before** (steps 5 and 6), each bounded by a cooldown or a reaction's rest:
   - Cold Damage (Venom Rime) sets the doses off on a heavy blow on a frozen enemy; if the shatter clears the Chill
     before the check, it never fires. Six Levels Down lays a Chill on a frozen enemy at each bite: whether that
     lengthens the freeze is untested.
   - Fast Water (Fish Poison) answers a poison bite with another bite; only its 0.5 s cooldown stops it feeding itself.
   - The Sisters' Quarrel, Cold Chisel, Held Down and Diamond Guard lay an affliction and then force the reaction:
     look for a reaction shown twice.
   - A duo spell answering a condition it sets itself (The Capstone's stone, Sweep of Wings' swept enemies, The
     Sisters' Quarrel's raw scald), and two plain marks answering one reaction (Strike While Hot and Red-Short).
   - When the Watch Sleeps lays a Tremor and forces Earthed in the same heavy blow; Wingbeat pushes everything round
     the enemy it starts from; Gathered In's roots beat on each bite and pass a dose on.
   - Cascades on purpose: Sandblast's grit, Fig in the Crown, Wings Behind, Hel's Half and The Debt Passes lay both of
     a pair's afflictions on others, so reactions may chain.
   - No Summer is a boon of -1 on HealthPrices after a shatter: if the stat is clamped above zero, prices only drop.
9. **The owner's decisions of 2026-11-20 on branches written before them**: Halo's Six Mirrors and Pyre's Light the
   Pyres are still gods' gifts in step 2, the twelve spare ring-mix spells are still retired with refunds there, and
   the branch files of steps 1 to 4 call their duo spell "the crown". Steps 5 and 6 follow the decisions (every duo
   spell bought with 5 Shards); the earlier ones are question 12 of task 4.17.

What is open for the owner is in `phase4_questions.md` (tasks 4.5, 4.15, 4.16, 4.17); the engine pieces the `// NEEDS:` lines
wait for are in `content_open.tsv`.
