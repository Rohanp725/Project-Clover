# The Way: build plan and the Katana's move-by-move spec

Written by Claude on 2026-11-21 for the owner, as step 3b of the agreed order in `docs/design/STATUS.md` (finish the
Way: Phase 5 tasks 5.3 to 5.7 and Memories paying out in play). It turns the decided design (`way_plan.md`, every
fork option 1 and every god's bough option (a), owner 2026-11-19; `phase5_questions.md`, every default kept,
2026-11-21) into a spec precise enough to build right the first time. **Nothing here changes a decision.** Where the
design is silent, or where it breaks one of the owner's standing rules, this doc asks (section 6) instead of filling
the gap. Numbers this doc adds where the design gave none are marked **(P)** and covered by question Q3.

**Progress** (sections are committed one at a time; a section not ticked is not written yet)

- [x] 1. What exists and what's left
- [ ] 2. The systems every Way uses
- [ ] 3. The Katana's Way, move by move
- [ ] 4. The other nine Ways
- [ ] 5. Build order and checks
- [ ] 6. Owner questions

**The owner's standing rules, baked into every line below.** Effects are animated timelines (the cause arriving,
the impact, the aftermath), never a still that pops in and out. Film on real enemies, in groups of 3 to 5 where a move
reaches several. No pure white and no ring effects. Effect pieces sit on the body, never floating off it. Nothing is
drawn on the blade itself; only slash trails and the element-coloured highlight on the weapon. Each element reads as
itself: fire is real flickering flame; Wind is curls, never a line through the body; Water is a big drop or splash;
Metal is only a glint or a small forged blade; Poison is miasma clouds. No reskinned attacks: every move is its own
mechanic with a body-first telegraph (wind-up, travel, impact, aftermath). Player text reads at an 8th-grade level in
short plain sentences. Every stand-in is ledgered in `content_open.tsv` and none is reported as done. The Katana's
Way is built and reviewed first; the other nine follow its checklist.

---

## 1. What exists and what's left

Paths are under `Assets/_Project/Scripts/` unless they start with `docs/` or `AgentScripts/`. Line numbers are as of
commit `a2c88f7`.

### 1.1 Built and tested (data and rules)

| Piece | Where | State |
|---|---|---|
| The Way's rules | `Runtime/Player/Way/WayRules.cs`: `KindOf` 40, `ActiveAspect` 61, `InForce` 83, `Open` 111, `SettleBought` 122, `ChooseAspect` 142, `ChooseKeystone` 152, `Spent` 160, `TemperRank` 177 | Done. `WayRulesTests` (17 tests). A trunk mark works in every aspect; a bough's marks only while it is active; only the set keystone of a bough is in force; an element infusion mark needs its element carried; the first aspect or keystone bought switches on at once |
| The wallet | `Runtime/Player/Growth/Wallet.cs`: `Memories` 23, `Pay` 60, `Rested` 79; saved at `Runtime/Core/Save/SaveData.cs:36` | Done. `WalletTests` (6). The playtest tops Memories up to 9999 at a rest (`Runtime/Core/PlaytestMode.cs:28`). The run's Shards still live in `RunState.Points` (5.1 Q1, kept) |
| Memories income, the rules | `Runtime/Player/Growth/Income.cs`: prices 41 to 49 (kind 1, named 2, elite 4, god 8, Teacher 4, last champion 12, found 3 or 5), `Defeat` 75, `GodFelled` 90, `Found` 94, `TierOf` 123; paid keys in `SaveData.KindsDefeated` (`SaveData.cs:35`) | Rules done, `IncomeTests` (8). **Nothing in play calls them** (1.2) |
| Buying a Way mark | `Runtime/Player/PlayerUpgrades.cs:514` to 519 (can buy, in Memories), 550 to 555 (`TryBuy` pays from the wallet); `Runtime/Player/MarkRules.cs:59` to 85 | Done, and proved once in play by `AgentScripts/WayProbe.cs` (buys Katana Temper I and the Sheath in the Proving Ground) |
| The Katana's Way as data | `Runtime/Player/Trees/Ways/KatanaWay.cs` (406 lines), registered at `Runtime/Player/Trees/UpgradeTrees.cs:79` | 62 marks: the trunk 20 (Temper I to V 134 to 152, four infusion families 157 to 165, five basics 170 to 183, Before the Thunder 187 to 189, five element infusion marks 193 to 222), the Sheath 10 (229 to 265), the Stream 10 (272 to 308), the Answer 10 (316 to 352), the Storm Katana 12 (359 to 403). Every mark has its words, price and gate; X and Y are zero (no chart places them) |
| Boughs as code | `Runtime/Player/Way/WeaponAspect.cs`, `Armoury.cs` (tables 18 to 29, `Resolve` 64 to 89), `WayTrunk.cs`, `AspectRun.cs`, `AspectContext.cs`; `Way/Aspects/KatanaTrunk.cs`, `SheathAspect.cs`, `StreamAspect.cs`, `AnswerAspect.cs`, `StormKatanaBough.cs` | Done as patches on a freshly built weapon, `ArmouryTests` (15). First pass only: each plays an existing clip, retimed (5.2 Q7) |
| The technique chord, engine | `Runtime/Player/Magic/ChordRules.cs`; `PlayerCombat.cs:146` to 152 (`TechniqueStep`, null), 475 to 476 (the chord read), 1536 to 1541 (`TakeChord`); `WeaponCombo.Replace` at `Runtime/Player/WeaponCombo.cs:197` to 205 | Built in Phase 1 and tested; **nothing ever sets `TechniqueStep`**, so the chord is never taken |
| The rest fire's Way entry | `Runtime/UI/RestChoices.cs:23` to 28 (`RestEntry.Way`), label 61, note 76 | Written, but `Runtime/UI/RestMenu.cs:28` and 139 build `new RestChoices()`, so the entry never shows |
| Mark details | `Runtime/UI/MarkDetails.cs:47`, 58 to 85 | Done: names the Way, the bough, the kind (Keystone, Bearing, Technique) and the price in Memories |
| The ledger | `docs/design/steel/content_open.tsv` lines 136 to 149 (`way-refund`, `way-clips`, `way-answer-window`, `way-chain-rules`, `way-held-light`, `way-sheathe`, `way-counters`, `way-infusion-families`, `way-roll-break`, `way-thrown-knife`, `way-god-moves`, `way-technique`, `way-finisher-hold`), plus `turn-to-face` (118) and `red-attack` (62) | Every Katana stand-in is on it |

The Katana's moves as built (`Runtime/Player/Weapons.cs:636` to 657; the Storm Katana at 615 to 629 is the same
moves at 1.15 times with a longer Thunder Draw). These are the clips every first-pass aspect borrows:

| Slot | Clip | Wind-up / swing / recovery (s) | Damage, stagger | Stamina | Reach, arc |
|---|---|---|---|---|---|
| Light 1, the drawing cut | `attack_light_1` | 0.10 / 0.08 / 0.24 (0.42) | 10, 10 | 10 | 2.4 m, 140 deg |
| Light 2, the return cut | `attack_light_2` | 0.11 / 0.08 / 0.25 (0.44) | 11, 10 | 10 | 2.4 m, 140 deg |
| Light 3, the stepping cut | `attack_light_3` | 0.16 / 0.10 / 0.38 (0.64) | 18, 22 | 14 | 2.7 m, 100 deg |
| Heavy, the overhead | `attack_heavy` | 0.30 / 0.10 / 0.46 (0.86) | 26, 34 (chain heavy 0.20 wind-up, 30, 40) | 20 | 2.8 m, 90 deg |
| Skill, the Iaido Slash | `attack_iaido` | 0.08 / 0.12 / 0.50 (0.70) | 48, 50 | 15, and 30 mana | 3.3 m, 170 deg, carries 2.6 m |
| Finisher | `attack_finisher` | 0.30 / 0.14 / 0.56 | 30 | 0 | 3.2 m |
| Carrier (infused strike) | the drawing cut, retuned (`Weapons.cs:660` to 668) | 0.18 wind-up | 22, 20 | | 2.6 m, 170 deg, carries 1.6 m |

The clips are made in code by the Forge (`Editor/Forge/PlayerAnimations.cs:119` to 127). Which sword object shows
(drawn, hilt in the sheath, none) is chosen per clip name (`PlayerAnimations.cs:90` to 105). The Katana has no
`guard_hit` or `guard_perfect` clip yet (the other weapons have them, `docs/design/weapons.md` E13).

### 1.2 What does not work in play yet (found while reading the code)

These matter more than any single mark, because they make most of the Katana's Way invisible in play today.

1. **No aspect reaches the sword.** `Armoury.Resolve` is never called. Every equip goes through
   `Weapons.ByName` (`PlayerCombat.cs:297`, 310; `World/LoadoutChoice.cs:100`; `World/GodReward.cs:75`, 96;
   `World/WeaponPickup.cs:36`; `Player/PlayerElement.cs:321`). So the Sheath's draw, the Stream's five cuts, the
   Answer's lighter blows, the Storm Katana's falling cut, Settled Hands, the Rising Draw and Temper III and V change
   only the weapon built in tests. This is the "editor half of 5.2" that `Armoury.cs` names and that was never done.
   What does work: marks that are plain stats or effects (Temper's +6% and +5%, Zanshin's wider window, Chiburi,
   Second Breath, Eddy, Kiri-otoshi's window, Thunder Follows, the five element infusion marks), through
   `PlayerUpgrades`.
2. **No aspect keeps a count or a window.** No aspect overrides `WeaponAspect.Tick`, and nothing on the player
   holds an `AspectRun`. The Answer's window, the drums, a Hundred Days' count and Patient Blade all wait on this.
3. **No chart shows the Way.** `RingLayout.Draws` leaves Way marks out (`Runtime/Player/IChartLayout.cs:33`), no
   `WayLayout` exists, and the rest fire's Way entry is hidden (1.1). A player cannot see or buy a Way mark; only the
   probe can.
4. **The chord plays nothing** (`TechniqueStep` is never set), and `Way/Infusion.cs` does not exist, so the four
   family marks do nothing.
5. **No Body.** No `Trees/Ways/BodyWay.cs`. `PlayerStat` has `Health` (`Runtime/Player/PlayerStat.cs:47`),
   `FlaskHeal` (49) and `FlaskCount` (51) but no `StaminaMost` and no `Poise`. No found pieces.
6. **No Memories are paid.** `AreaDirector.OnEnemyDied` (`Runtime/World/AreaDirector.cs:102` to 112) pays Shards
   only. The Fire god's fall sets `god.fire` at `Runtime/World/SunCourtDirector.cs:317`; the Champion's flag is set at
   `Runtime/World/GateOfNoon.cs:63`. Neither pays a Memory. No HUD line shows Memories.
7. **The roll's cut window is narrower than the design.** `PlayerCombat.cs:468` asks for the dodge's cut only in the
   roll's second half (`Dodge.Progress >= 0.5`). The Rising Draw wants the last 40% of a roll or 0.15 s after it;
   Stepping Water wants any part of it. Neither is a step of the chain yet.
8. **Retiming has a limit.** A clip is scrubbed by the step's progress (`Runtime/Player/PlayerAnimator.cs:219`), so
   a step retimed by one even share plays its clip correctly. But the blow's frame is fixed where the clip was built,
   at the base step's `Windup / Total` (`PlayerAnimations.cs:155`). A patch that changes the wind-up alone (Before the
   Thunder's 0.12 s on a clip built for 0.10 of 0.42, or the Red Draw's long lane on the Thunder Draw's 0.70 s clip)
   puts the blade's arrival on the wrong frame. **Rule for every Way**: a first-pass reuse may only scale a step
   evenly; any move whose wind-up, travel or blow moment differs from its borrowed clip gets its own clip.

### 1.3 What's left, task by task

| Task | Left to do | Needs |
|---|---|---|
| 5.2, the editor half | Equip what `Armoury.Resolve` gives at every equip point; re-resolve on buying a Way mark, on choosing an aspect or keystone at a rest fire, and on entering a scene; an `AspectRun` on the player ticked each frame; `WayProbe` extended so each bough's chain plays | Editor (probe) |
| 5.3 | Temper V's held pose; `Way/Infusion.cs` (the four families on the carrier); the technique's step on the chord, with its gate; Hold the Gate for the Great Spear and a check that the Champion already shows it | Editor; new clips for Before the Thunder and Hold the Gate |
| 5.4 | `BodyWay.cs` (12 marks), two new stats, the bearings' rules, found pieces as pickups | Editor; a pickup model each (Blender or Forge) |
| 5.5 | `WayLayout`, the Way chart screen, the rest entry, choosing aspect and keystones at the fire, `marks/way/katana.md` | Editor; screenshots |
| 5.6 in play | Hooks for first defeats, elites, gods, found Memories; what the player sees when paid | Editor; a pickup model; one effect |
| Katana engine pieces | The eleven `way-*` ledger lines above, in the order of section 5 | Editor |
| Katana clips | 18 new clips (`way_plan.md` 11), plus `guard_hit` and `guard_perfect` for the Katana | Forge (code, editor) |
| Katana looks | Every move's effect, filmed (section 5) | Editor; possibly Blender for meshes |
| 5.7+ | The other nine Ways, one a session, on the Katana's checklist | As the Katana |

---
