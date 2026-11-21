# Phase 4b merge plan: the owner's answers of 2026-11-20

Branch `phase4b`, made from `main` at `7bc85df`. It was built offline, with no editor and no `unity` CLI. All four
assemblies were compiled offline after each commit (`offline_compile.py`), and the EditMode fixtures that run outside the
editor were run with the offline harness.

**NEEDS lines.** Before this work there were **493** `// NEEDS` lines in `Assets/_Project/Scripts/Runtime` (562 counting
every "NEEDS" mention). After it there are **455** (523). In the ledger `content_open.tsv`, rows went from 139 to 141. Three
rows were closed: `reaction-numbers`, `duo-surge-slot` and `empower-effects`. Five were opened, each naming exactly what is
still missing: `reaction-numbers-when`, `reaction-start-count`, `mark-blows-on`, `whirl-fed` and `alternate-blows`. The
named marks and pieces in the third column went from about 928 to about 880.

## The commits, in order

| Commit | Task | What it does |
|---|---|---|
| `1e07c7f` | 7 | The deep strike needs the element's Devotion bought (`StrikeRules.IsDeep(second, mixed, devoted)`). Without it, L2 with R2 alone or all in is the plain infused strike, which costs no mana. Also updates the HUD, the guide and the Devotions' texts. |
| `552fdf4` | 8 | The heart's picker steps through all ten partners. Fast Water quickens each Soaked enemy's own poison clock (`Afflictions.PoisonQuickWhenSoaked`). The Exile Returns and Given Bones answer only the Grudge's bolt, through `UpgradeEffect.AnyMark` and `PlayerUpgrades.FireCondition`. Ash Plume's storm is capped at five bolts (`PerMost`). |
| `3581206` | 5 | Undoes the "a second less" Empower defaults. Builds Empowers that change their Learn's own effects (twins in the same slots) and Empowers laid on the Learn's spell alone (`LearnOnly`). |
| `1bbd191` | 1 | Adds the six keystones and the engine pieces they stand on. |
| `48171c0` | 6 | Keeps Hammer Down, Clean Break, Spilled Water and Shade as marks. Adds a 37th pattern place, `T.3`. |
| `234bb48` | 2 | Adds `reaction-numbers` (`ReactionNumbers`, `Upgrade.ReactionChanges`) and the 18 minors finished with it. |
| `4ec7497` | 3 | Adds the duo-surge slot (`DuoSurge`, `Upgrade.Surge`, `PlayerElement.WearDuoSurge`, `HandRules.CanWear`) and the nine duo surges. |
| `2b52de6` | 4 | Adds the form pieces: second presses (`Upgrade.Presses`), `SwapPlaces`, `StepBack`, `Untouchable` and `ClearCount`. Finishes Three Weasels and Under the Black Water. |
| `83dde33` | review | Fixes from a code review, and adds `AgentScripts/Phase4bProbe.cs`. |
| `b8e7c17` | docs | This plan, and the pass recorded in `phase4_progress.md`. |
| `a38e6db` | 6 | Indrawn Breath and Ski Run grow from Hammer Down and Clean Break, so the ring check passes (RING OK). |

## Before merging

1. Stop Play Mode. Merge `phase4b` into `main` with `--no-ff`.
2. Recompile: `unity --json recompile --timeout 120`. Expect 0 errors and 0 warnings.
3. Commit the `.meta` files Unity writes for the four new scripts:
   - `Runtime/Player/DodgeWay.cs`
   - `Runtime/Player/ReactionNumbers.cs`
   - `Runtime/Player/DuoSurge.cs`
   - `AgentScripts/Phase4bProbe.cs` (outside Assets, so no meta)
4. Rebuild the playtest texts: Clover > Build > Playtest Texts. Many `docs/design/playtest/marks/*.md` entries changed, and
   ten new entries were added: six keystones and four kept marks. Commit the new `PlaytestTexts.asset`.

## EditMode fixtures to run (the whole suite, and these by name)

Offline, these all passed. The harness reported 708 passed in all. The 114 that failed offline are the same as on `main`:
they need the editor, for example scenes, JSON and the input asset.

| Fixture | What it covers here |
|---|---|
| `KeystoneRuleTests` (15) | The six keystones' places and pieces: dodge way, dodge terms, burns that do not bite, the tide's stats, the waves, ripening seeds (including a ripe seed fading once the mark leaves), and Shared Roots built by hand. |
| `UpgradeChartTests` (31) | Tree counts (Fire 35; Fire, Water and Life 12 or 13 passives), the 37-place pattern, the four kept marks, Empowers that strengthen or shorten, and press effects counted in the slots. |
| `SpellBranchTests` (12) | The Empowers that keep the table's wait, Empower twins, and `LearnOnly` (Daybreak's wall keeps 48). |
| `DuoBranchTests` (10) | The heart picker's ten partners, The Exile Returns and Given Bones answering only the bolt, Ash Plume capped at five, reaction-numbers copies, every reaction change naming a real effect, and Three Weasels' presses with Under the Black Water's swap. |
| `HandRulesTests` (18) | The deep strike needs the Devotion. A duo surge is offered and kept only while its pair is carried, is in force only while worn, has its own kit, and can be lengthened. |
| `InForceTests` | The deep strike needs the Devotion, and each element's Devotion is found. |
| `PoisonBeatTests` (11) | Fast Water per enemy. |
| `SaveMigrationTests` | Hammer Down, Clean Break, Spilled Water and Shade are no longer retired. Check that a save holding one keeps it and is refunded nothing. |
| `RuleBookTests`, `MarksBuiltTests`, `SpellCooldownTests`, `SpellSlotsTests`, `ReactionCatalogueTests` | Regression. |

## Probes to run (sandboxed, through `runprobe.sh`)

| Probe | Scene | Expect |
|---|---|---|
| **`Phase4bProbe`** (new) | ProvingGround | 0 WRONG. It checks the following. Into the Fire: a dodge through a dummy burns it two deep, and a dodge away costs double. The Blade Drinks Fire: the root lays nothing, burns do not bite, and a light blow drinks one Burn. High Water, Low Water: spells x1.33, then the weapon x1.33 after 6 s. Drawn Back to Break: two dodges away give two waves, and one blow breaks both, soaks and throws. Shared Roots: 20 of 30 taken, 10 by the Seeded dummy. In Its Season: ripe after 6 s and not fading. Deep Swell: swollen 7 s. Under the Black Water: places changed. Three Weasels: three presses. Ash and Thunder: worn and lit for 40. |
| `GrudgeProbe` | ProvingGround | 0 WRONG, with the new lines: seven blows call down five bolts, and The Exile Returns curses a neighbour at the bolt. |
| `P1bProbe` | its own scene | 0 WRONG. Step 4 now strikes once without Fire's Devotion (plain, no mana), then buys it and strikes deep. |
| `P2Probe` | its own scene | 0 WRONG. All in on Fire, R2 is plain until the Devotion is bought. |
| `DevotionProbe` | ProvingGround | 0 WRONG. It gives the Devotions before the deep strikes, so it should be unchanged. |
| `DuoProbe` | ProvingGround | 0 WRONG. The picker line now expects the next partner round the ring, built or not. |
| `DuoProbeAll` | ProvingGround | 334 OK, 0 WRONG as before. The plain and shaped reactions run through `PlayerUpgrades.ReactionEffects`, which returns the effects as written while no changer is learned. |
| `MarksProbe` | CombatTest | Runs to done. Great Lotus is now worn in the surge slot before it is tried. |
| `ReactionProbe2`, `CrowdProbe` | as before | 336 OK, 0 WRONG; 6 OK, 0 WRONG. |
| `HeartCrowdProbe` (`bash AgentScripts/profile_heart.sh`) | ProvingGround | Profiler: no new allocation per frame from `PlayerUpgrades`, `PlayerSpells` (the Devotion cache), `PlayerReactions` (the copies are made once) or `SpellDiamondHud`. |

## What to look at in play

1. **Into the Fire.** Does the roll through a body read as a lunge? It passes bodies through the same move the passing cut
   uses. Does the smoke on a dodge away read?
2. **The tide.** "High water" and "Low water" appear as callouts with a ring at the feet. There is no HUD sign yet (ledger
   `looks`).
3. **The chart.** Check Wind's and Ice's right boughs with Hammer Down and Clean Break in the middle place. Indrawn Breath and
   Ski Run now grow from them, as the pattern grows those stalks. Check Water and Life with the new top-of-the-trunk place
   `T.3`. The ring check (`ring_check.py`) prints RING OK: 383 marks drawn, least spacing 0.912.
4. **Under the Black Water** on a god or an elite: you should rise behind it and it should stay put.
5. **The Hand's surge box** with a duo surge worn: its name, its two elements and its price.
