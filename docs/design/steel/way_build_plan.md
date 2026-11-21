# The Way: build plan and the Katana's move-by-move spec

Written by Claude on 2026-11-21 for the owner, as step 3b of the agreed order in `docs/design/STATUS.md` (finish the
Way: Phase 5 tasks 5.3 to 5.7 and Memories paying out in play). It turns the decided design (`way_plan.md`, every
fork option 1 and every god's bough option (a), owner 2026-11-19; `phase5_questions.md`, every default kept,
2026-11-21) into a spec precise enough to build right the first time. **Nothing here changes a decision.** Where the
design is silent, or where it breaks one of the owner's standing rules, this doc asks (section 6) instead of filling
the gap. Numbers this doc adds where the design gave none are marked **(P)** and covered by question Q3.

**Progress** (sections are committed one at a time; a section not ticked is not written yet)

- [x] 1. What exists and what's left
- [x] 2. The systems every Way uses
- [x] 3. The Katana's Way, move by move
- [x] 4. The other nine Ways
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

## 2. The systems every Way uses

Each system below has: what it does, the numbers, how it looks and reads, what "done" means (tests, a probe, film),
and the questions it raises (numbered in section 6). The Katana is the first user of each; every other Way plugs
into the same code.

### 2.0 Wiring the Way into play (the editor half of task 5.2; first, because nothing else shows without it)

- **What**: one call, `PlayerBuild.Rearm()` (new), builds the carried weapon with `Armoury.Resolve(loadout, marks)`
  and hands it to `PlayerCombat.Equip`. Every equip point in 1.2 item 1 calls it instead of `Weapons.ByName`. It also
  runs after a Way mark is bought (`PlayerUpgrades.TryBuy`), after an aspect or keystone is chosen at a rest fire, on
  entering a scene, and when the first element changes (the Armament's case, `PlayerElement.cs:321`). It never runs
  mid-swing: while the player is attacking it waits for the swing to end, as `PlayerElement` already does.
- **The aspect's state**: the player holds one `AspectRun` per carried weapon (in `PlayerCombat`), cleared on a rest
  and on a change of aspect, and calls the active aspect's `Tick` once a frame with an `AspectContext` built from
  events already raised (`Landed` from the blow path, `PerfectGuard` and `RedTurned` from `PlayerDefence`, `Killed`
  from the kill event, `InFight` from the `FightClock`). No allocation per frame.
- **The playtest's rack** keeps handing over "Storm Katana" by name; `WayRules.ActiveAspect` already treats that as
  the god's bough (5.2 Q13, kept).
- **Done means**: an EditMode test that `Rearm` equips exactly what `Resolve` builds for each of the Katana's five
  states (plain, Sheath, Stream, Answer, Storm), and that a mark bought mid-swing waits for the swing's end;
  `WayProbe` extended: buy each aspect in the Proving Ground, choose it, swing the light chain, and log each step's
  clip name and damage against this spec's numbers; one Game View capture of each chain at its third step.

### 2.1 Temper ranks (task 5.3)

- **What** (decided, `weapons.md` 0.3): five ranks on the trunk, bought in order for 1, 1, 2, 2, 3 Memories; each
  +6% weapon damage and +5% stagger (+30% and +25% at V). Gates (5.1 Q6, kept): I and II open, III at the third god,
  IV the fifth, V the seventh (`gods.N`); the playtest opens all (5.1 Q2).
- **Built**: the stats (`KatanaWay.cs:151`), Temper III's +0.2 m on the drawing cut and the carrier
  (`KatanaTrunk.cs:36` to 40) and V's +15% finisher (41). The stats work in play now; the patches wait on 2.0.
- **Left**: Temper V's "finisher's held pose 0.2 s longer". The Katana's finisher (`attack_finisher`, the
  run-through) has no held pose to lengthen (ledger `way-finisher-hold`). Question Q5.
- **How it reads**: the design gives Temper no look of its own. This spec draws nothing on the weapon (the rule
  allows only slash trails and the element highlight there); the chart shows the rank. Question Q4.
- **Player text** (one shared line with the weapon's name): "Katana Temper II. Every Katana hit does a little more
  damage and breaks balance a little sooner." Temper III adds "The drawing cut reaches a little further." Temper V
  adds "The finishing blow hits harder."
- **Done means**: `TemperTests` (rank counted, gates honoured outside the playtest, stats summed to +30% and +25% at
  V); the probe logs the drawing cut's damage at ranks 0, 3 and 5 (10, 11.8 and 13 on the plain Katana) and its reach
  at rank 3 (2.6 m).

### 2.2 Infusion (task 5.3)

- **What** (decided, `weapons.md` 0.4): the infused strike (L2 with R1 or R2) plays the weapon's carrier with an
  element on it. Four family marks on each trunk, 2 Memories each, gated `gods.1` (5.2 Q4, kept), bend the carrier for
  every element of one family. All in on one element, the deep strike doubles the family's length, time or pull.
- **Built**: the carrier (`Weapons.cs:660` to 668), the footprint, each element's own strike (`ElementSteel.cs`),
  and the Katana's four family marks as words only (`KatanaWay.cs:157` to 165).
- **Left**: `Way/Infusion.cs`, small functions keyed by family that bend the carrier's clone before
  `WeaponCombo.Follow` starts it (`IMPLEMENTATION.md` 3.4). A family mark is in force while its weapon is carried and
  the strike's element is of its family.
- **The Katana's four**, on the drawing cut (numbers decided in `weapons.md` 1, "Infusion"):

| Family (elements) | Mark | Timeline | Numbers | Look at the game camera, per element |
|---|---|---|---|---|
| Ground (Fire, Poison, Life) | Lying Draw | The carrier as built (0.18 s wind-up); at the blow the 170 degree arc settles onto the floor and lies 3 s (6 s all in), then dies back from its ends | The element's own ground rule along the arc | Fire: real flickering flame along the arc, licking up and guttering out from the ends. Poison: low miasma clouds rolling off the arc and thinning. Life: that element's built growth along the arc. Never a drawn outline: the arc is made of the substance |
| Line (Lightning, Wind, Light) | Running Draw | Wind-up 0.18 s; the draw becomes a dash of 5 m (10 m all in) through every enemy on the lane, cutting each as you pass; recovery as the carrier | The carrier's 22 and 20 to each | Lightning: a jagged bolt runs along the lane a beat ahead and you arrive on its end. Wind: curls of air peel off your body as you go and the struck spin in small eddies, never a straight streak. Light: that element's built look |
| Weight (Earth, Metal, Ice) | Settling Draw | Wind-up slows to 0.32 s and cannot be broken; the cut stops in the first enemy | +60% stagger (32); the element set in that enemy | Earth: stone crust climbs the struck enemy's legs. Metal: one small forged blade left standing in the enemy, with a glint. Ice: frost locks the enemy where the blade bit |
| Draw (Water, Dark) | Dragging Draw | The draw lands, then the return cut follows by itself 0.15 s (P) later and drags all the draw struck 1.5 m (3 m all in) onto the point | The return cut's 11 and 10 | Water: a big splash thrown up where the drag starts, the struck pulled through it. Dark: that element's built look |

- **A real gap**: Fire's own infused strike already leaves fire lying on the footprint for 3 s
  (`ElementSteel.cs:435` to 449), so Lying Draw with Fire repeats what Fire does anyway. Question Q6.
- **Player text**: Lying Draw: "Your infused draw with Fire, Poison or Life leaves its cut burning on the floor for a
  few seconds." Running Draw: "Your infused draw with Lightning, Wind or Light becomes a dash. You cut everyone in a
  line." Settling Draw: "Your infused draw with Earth, Metal or Ice is slower, but nothing can stop it. It sticks in
  the first enemy and staggers it hard." Dragging Draw: "Your infused draw with Water or Dark is followed by a second
  cut on its own. It pulls back everything the draw hit."
- **Done means**: `InfusionTests` (each family bends the clone and not the chain; all in doubles; the wrong family
  does nothing); `StrikeProbe` on the Katana with one element of each family against a pen of 3 to 5 soldiers,
  filmed at the game camera and up close (section 5).

### 2.3 The technique chord (task 5.3)

- **What** (decided, `weapons.md` 0.5): R1 and R2 together (both mouse buttons). One technique a weapon, sealed until
  its elite falls, then 2 Memories; works in every aspect. The elite uses the move on the player first, with the same
  tell. The Fists, taught twice, set one at a rest fire.
- **Built**: the chord (1.1). **Left**: each technique as an `AttackStep` factory on its Way; `PlayerBuild.Rearm`
  sets `PlayerCombat.TechniqueStep` from the technique in force (null when sealed or unbought, so the chord stays a
  heavy); the gate read from the mark.
- **The Katana's, Before the Thunder** (draft, `weapons.md` 1): the quickest draw there is, landing 0.12 s after the
  press. If it strikes an enemy in its wind-up (`EnemyState.Telegraph`, `Gameplay/Enemy.cs:120`), red attacks
  included, that attack is broken and the enemy takes 50 stagger. If it strikes nothing in a wind-up it is a plain draw
  with a long 0.6 s recovery. Its damage and price are not written: question Q7. Its teacher waits for the Lightning
  and Wind land's overview; nothing sets `elite.last_retainer` yet, so outside the playtest it stays sealed (the
  playtest opens every gate). Full block in section 3.1.
- **The Great Spear's, Hold the Gate** (decided, `weapons.md` 4): the spear driven upright, two kicks all round at
  2.4 m (10 each, 20 stagger), ripped free in a sweep at 3.8 m and 200 degrees (24) and a thrust down a 5.4 m lane
  (20). Gated `elite.champion_gate` (`SlotSchedule.ChampionFlag`, already set at `GateOfNoon.cs:63`). The task asks to
  check that the Champion shows it first as `champion_plant` into `champion_reap`; that check is a step in section 5.
- **Done means**: `TechniqueTests` (sealed: the chord is a heavy; open and bought: the chord takes the step; the Fists
  take only the chosen one); a probe that presses R1 and R2 within 0.08 s before and after the gate; a film of Before
  the Thunder breaking a soldier's wind-up in a pen of three.

### 2.4 The Body branch (task 5.4)

- **What** (decided as to shape, numbers Proposal, `way_plan.md` 0.4): twelve marks, 2 Memories each and 4 for each
  bearing, 28 in all, always in force whatever is carried. A stem of four, then two sides each ending in a **bearing**;
  both sides can be bought and all their numbers count, but only one bearing is set, at a rest fire (G5). The first
  bearing bought sets itself (5.1 Q3).

| # | Mark | Kind | Number | Needs |
|---|---|---|---|---|
| 1 | Health I | Number | Health 100 to 110 | `Health` stat, built |
| 2 | Stamina I | Number | Stamina 100 to 110 | New stat `StaminaMost` |
| 3 | Fourth Flask | Number | Flasks 3 to 4 | `FlaskCount`, built |
| 4 | Health II | Number | 110 to 120 | |
| 5 | Full Flask | Number | A flask heals 45 to 55 (`FlaskBelt.cs:13`) | `FlaskHeal`, built |
| 6 | Steady | Rule | +6 poise in every swing of every weapon; outside a swing a blow of 6 or less does not flinch you | New stat `Poise` |
| 7 | Hard Frame | Number | Health 120 to 135 | |
| 8 | The Enduring Body | Bearing | An unguarded blow flinches you 0.2 s, not 0.35, and pushes you half as far; grey health starts to fade after 4 s, not 2.5 | Flinch and push shares on `PlayerDefence` |
| 9 | Deep Lungs | Number | Stamina 110 to 125 | |
| 10 | Quick Breath | Number | Stamina returns after 0.6 s, not 0.8 | A stamina-delay stat |
| 11 | Long Step | Number | Every weapon's dodge 0.6 m longer, same cost (stacks with Wind's Long Stride) | Dodge distance |
| 12 | The Light Body | Bearing | A dodge pressed in a dodge's last 0.1 s flows into a second at half cost (never a third); the recovery after a dodge 0.1 s shorter | `DodgeRoll` chaining |

- **Found pieces** (Proposal): about one a land and one from each god, as Sekiro's prayer beads: a vessel (+5
  health), a breath stone (+5 stamina), a flask shard (three make a flask). Given marks of the same stats
  (`Currency = None`, `Tree = Body`), picked up as world objects. Where the Fire land's go is not written: question Q8.
- **How it reads**: the numbers need no effect; the HUD's bars grow longer, which is the proof. The Light Body's
  second step is the roll clip again from the first roll's end; the Enduring Body's shorter flinch is the hurt clip
  cut short.
- **Player text**: Health I: "More health." Stamina I: "More stamina." Fourth Flask: "Carry one more flask." Full
  Flask: "Each flask heals more." Steady: "Small hits don't make you flinch, and your swings are harder to stop." Hard
  Frame: "Much more health." The Enduring Body: "Hits knock you back less, and you recover faster. Lost health stays
  longer before it fades, so you have more time to win it back." Deep Lungs: "Much more stamina." Quick Breath:
  "Stamina starts coming back sooner." Long Step: "Your dodge goes further." The Light Body: "Dodge again just as a
  dodge ends to flow into a second one for half the stamina."
- **Done means**: `BodyWayTests` (bearings exclusive, numbers summed, found pieces stack); a probe that buys the Body
  and shows health 135 and four flasks healing 55 (the task's "health and flask rise"); a filmed double step under
  the Light Body.

### 2.5 The Way chart at the rest fire (task 5.5)

- **Where**: the rest menu's "The Way" entry (shown by building `new RestChoices(true, false)` in `RestMenu`).
  Outside the playtest it lists only owned weapon kinds; the shoulder buttons change weapon (in the playtest, all
  ten). The world is stopped and the dodge button backs out, as on the other rest screens.
- **Layout** (`WayLayout`, an `IChartLayout`; the decided shape, `IMPLEMENTATION.md` 4.2): one weapon at a time. The
  trunk runs up the middle: the five basics in a row at its foot; the four infusion families and the element infusion
  marks in a second row; Temper I to V as one column up the centre; the technique above them (drawn sealed, with its
  elite's name, until the gate opens); the Bond mark at the top (drawn shut until Phase 6). The three plain boughs fan
  out left, up-left and right; the god's bough up-right in its god's colour, drawn only once Taken (the playtest draws
  it). Each bough: the aspect at its root, the stem (three marks, five on a god's bough), then the fork into two paths
  of two marks each ending in a keystone. The Body branch sits at the foot under the trunk, the same on every weapon's
  chart.
- **How it reads**: the active aspect's bough is lit; sleeping boughs are dimmed but readable. A set keystone wears a
  small frame; another bought keystone is drawn without it. A mark that cannot be bought yet (order, gate, Memories)
  is faint, and the reason is the first line of its details ("Opens at the third god." "Buy Sharp Draw first." "You
  need 4 Memories."). The Memories you hold show in the top corner; each mark shows its price.
- **Buying**: the same Learn press as the element chart (`PlayerUpgrades.TryBuy`). The mark fills in along its line,
  the Memories count ticks down, and the weapon is re-armed (2.0). A bough's first aspect or keystone switches on at
  once (5.1 Q3).
- **Choosing at the fire**: how the player sets the active aspect and keystone is not written: question Q9.
- **Written details**: `docs/marks/way/katana.md`, the long text each mark's details page shows, from section 3's
  player text.
- **Done means**: `WayLayoutTests` (every Katana and Body mark placed, none overlapping, every mark reachable by the
  stick from the trunk's foot); screenshots at 1280x720 with nothing bought, half bought and all bought, on pad and on
  keyboard prompts; a probe that walks the stick over all 74 marks.

### 2.6 Memories paying out in play (task 5.6)

- **The hooks** (rules built in `Income.cs`; each pays once, under its key, into `Progress.Data.Wallet`):

| Hook | Where | Pays | Key |
|---|---|---|---|
| First defeat of a kind | `AreaDirector.OnEnemyDied` (`AreaDirector.cs:102`), beside the Shards | 1 | `Income.KeyOf(kind)` in `KindsDefeated` |
| An elite | the same hook, by `Income.TierOf` | 4 | the kind's name |
| A god's fall | where the fall's flag is set: the Fire god at `SunCourtDirector.cs:317`; each later god the same | 8, once; its bodies pay 0 | `god.<element>` |
| Found Memories | a new pickup, `World/MemoryCache.cs`, with a save key | 3 in a land of one element, 5 in a land of two | the pickup's key in `SaveData.Found` |
| Named fighter, the Teacher, the last champion | when the first is built (a field on `EnemyKind`, ledgered) | 2, 4, 12 | |

- The Proving Ground's pens pay no Memories (5.6 Q3, kept); the playtest tops Memories up at every rest anyway.
- **The Gate Captain** is marked `IsElite` (`Gameplay/EnemyKind.cs:293`), but `weapons.md` 14 says he teaches nothing
  until the castle is reworked, and the Fire land's sum (21) counts one elite: question Q11.
- **How the player sees a payment** is not written: question Q10. The recommended answer: a small mote of warm amber
  light (not white) rises from the fallen body and drifts into the player's chest over 0.8 s, as the Shards' embers
  already fly (`Enemy.CarryEmbers`); a short status line "+4 Memories" under the Shards' count for 2 s; a god's 8 shown
  after the fall's own moment, never over it. The rest menu and the Way chart show the total.
- **Player text** (guide entry, "Memories"): "Memories come from the people you beat. You get them the first time you
  beat each kind of enemy, and from strong foes and gods. Some lie hidden in the world. Spend them on your weapon's Way
  at a rest fire. You never lose them."
- **Done means**: `IncomeHookTests` (a hook pays once, a pen pays nothing, a god's two bodies pay once at the fall);
  a probe in the Fire Yard that kills two soldiers of one kind and checks +1, then the Champion and checks +4, with
  saved progress sandboxed (`runprobe.sh`); a film of the mote.

### 2.7 Engine pieces the Katana brings in for every Way

Each is one ledger line now and serves later Ways. Built in the order of section 5, each with its test first.

| Piece (ledger) | What it is | Katana marks it finishes | Later Ways that use it |
|---|---|---|---|
| The answer window (`way-answer-window`) | A perfect guard opens a window (`AspectRun.Window`); light or heavy in it plays a given step | The Answer, Open Window, Patient Blade, Second Answer, Kiri-otoshi, Catch the Red, The First Sword | The Reverse Grip, the Iron Fan, the Anvil |
| Chain rules (`way-chain-rules`) | A dodge in a recovery keeps the chain; a heavy mid-chain that does not end it; a step that counts as the chain's next; a looping chain; a condition on the chain's step | The Stream and its marks, the Rising Draw | Every bough that changes a chain |
| A held light press (`way-held-light`) | Light held past a threshold becomes a held move with a release | Held Draw, Saya-biki, The Cut Lands at the Click | The Long Draw (bow) |
| The sheathe beat (`way-sheathe`) | A short sheathe step after a chain or on a press; the next light is a draw | The Sheath, Quick Sheath, Chiburi, Light Scabbard, Nukitsuke Aside | The Katana's own |
| Aspect counters (`way-counters`) | `AspectRun.Count`, fading out of a fight unless kept | Seven Drums, Drumbeat, Rolling Drums, The Drum Ring, A Hundred Days, Patient Blade | Heat (the Smith's Hammer), sheaves (the Harvest), nicks (the Rattle) |
| A blow out of the roll on heavy (`way-roll-break`) | Roll then R2, about 45 stagger; an enemy's raised guard or shield broken 1 s | Tsuka-ate | Every Way's second basic |
| A thrown shot from the guard (`way-thrown-knife`) | Guard held then R1 throws a small shot (E8) with its own cooldown | Kozuka | The Knives' Throw a Knife |
| A finisher's held pose (`way-finisher-hold`) | A beat inside the finisher where time slows and the pose holds | Temper V | Every Way's Temper V |
| A red attack as a trigger (`red-attack`) | The guard turning an unguardable attack raises its own event | Catch the Red, Read the Red, The First Sword | Many element marks already ledgered |
| Turn to face (`turn-to-face`) | The player turned toward a target over a short time | Zanshin | |
| Stand still on a lane | Every enemy on a lane held still a short time | The Cut Lands at the Click | Phase 3's enemy conditions, if one already holds an enemy in place; else a short new `Held` condition |

---

## 3. The Katana's Way, move by move

**How to read a block.** *Press*: what the player presses. *Timeline*: wind-up, travel, impact, aftermath, in
seconds. *Numbers*: damage, stagger, stamina, mana, reach. *Risk*: the counterplay, what punishes it or what an enemy
does about it. *Clip*: an existing clip retimed, or a new one. *Look*: what the game camera (55 degrees, 12 m) shows,
under the standing rules. *Text*: the player's words, 8th-grade, short sentences. *Now*: what plays today and what
replaces it. Rules and numbers with nothing to animate get a short block.

**Rules for every block.**

- **Clip names**: an attack clip must start with `attack_` or the animator makes no state for it
  (`PlayerBuilder.AnimatorController`); the ledger's `katana_roll_cut`, `katana_held_draw` and `katana_raikiri` are
  built as `attack_katana_roll_cut`, `attack_katana_held_draw` and `attack_storm_raikiri`. Each clip follows the house
  rule (`PlayerAnimations.cs`): a wind-up that goes the other way first, a held beat, a blow that arrives fast, a
  follow-through, a settle; hips and chest lead, arms arrive after, the head stays on the enemy.
- **Retimed reuse**: only by one even share (1.2 item 8). A move whose blow lands at a different moment gets its own
  clip.
- **Colour**: the Katana's slash colour is pale steel-blue (`Weapons.cs:641`); the Storm Katana's is cyan (620).
  Every "white" sign in the design is drawn in the weapon's slash colour, or the carried element's when the move
  carries one (question Q1). The pale-cyan core of a bolt is never pure white.
- **Damage numbers below are the plain Katana's at Temper 0.** The Storm Katana's built moves are 1.15 times; its
  bough's own marks are as written (5.2 Q9, kept). The Answer's light blows are 0.9 times.
- **(P)** marks a number this spec proposes where the design gave none (question Q3).

### 3.1 The trunk

**Rising Draw** (`katana.risingdraw`; Move, 2)
- *Press*: R1 in the last 40% of a roll, or within 0.15 s after it ends.
- *Timeline*: wind-up 0.08 s (P), from the roll's crouch: the body stays low, the sword hand already at the hilt.
  Travel: the cut rises from low-left to high-right as you come up, carrying 0.6 m (P). Impact at 0.08 s, hit stop
  0.045 s. Aftermath: recovery 0.26 s (P), and the next R1 is the chain's second cut.
- *Numbers*: 12 damage, 12 stagger, 150 degrees, 2.4 m, 10 stamina (P, as the drawing cut).
- *Risk*: it commits you at the roll's end; a roll that ends out of reach cuts air. A wide sweep catches you rising.
- *Clip*: new, `attack_katana_roll_cut` (0.42 s).
- *Look*: a crescent trail sweeping up out of the roll; the struck flinch upward. Nothing else.
- *Text*: "Press light attack as your dodge ends. You rise out of the roll with a cut. Your combo goes on from there."
- *Now*: the drawing cut's clip, only in the roll's second half, and not a step of the chain (`way-clips`,
  `way-chain-rules`).

**Tsuka-ate** (`katana.tsukaate`; Move, 2)
- *Press*: R2 in the same window as the Rising Draw.
- *Timeline*: wind-up 0.12 s (P): the back hand drives the hilt forward as the body rises behind it. Travel: a 0.8 m
  (P) step in. Impact: the pommel to the face, hit stop 0.08 s (P), the head snaps back. Aftermath: recovery 0.3 s (P).
- *Numbers*: 6 damage, 45 stagger; an enemy's raised guard or shield is broken for 1 s. Reach 1.4 m (P), a point,
  not an arc; 12 stamina (P).
- *Risk*: short and narrow; a miss leaves you close and open.
- *Clip*: new, `attack_katana_tsuka_ate`.
- *Look*: no effect of its own: the game's hit spark at the pommel, the enemy's head and guard knocked up and apart.
- *Text*: "Press heavy attack as your dodge ends. You slam the hilt into their face. It does little damage but breaks
  their balance. It also breaks a raised guard or shield for a moment."
- *Now*: nothing (`way-roll-break`).

**Settled Hands** (`katana.settledhands`; Number, 1). The three light cuts recover 10% sooner (0.24, 0.25 and 0.38 s
become 0.216, 0.225 and 0.342). No look. *Text*: "Your three light cuts recover a little faster." *Now*: built
(`KatanaTrunk.cs:24`), reaches play with 2.0.

**Kozuka** (`katana.kozuka`; Move, 2)
- *Press*: guard held, then R1. (While the Answer is active, Guard Cut takes this input; decided.)
- *Timeline*: wind-up 0.12 s (P): the guard stays up, the sword hand drops to the scabbard's pocket. Travel: the small
  knife flies at 30 m/s (P), at most 8 m, in about 0.27 s. Impact: it sticks in the faced enemy, which flinches 0.3 s
  (P) if light. Aftermath: recovery 0.2 s (P); the knife stays in the body 1 s, then is gone.
- *Numbers*: 4 damage; once every 6 s; 6 stamina (P).
- *Risk*: heavy enemies do not flinch; the 6 s wait.
- *Clip*: new, `attack_katana_kozuka`, upper body. *Prop*: the kozuka, a small knife mesh (Blender or Forge).
- *Look*: a small spinning knife, then the knife standing in the body and the flinch. No trail beyond a faint streak
  in the slash colour.
- *Text*: "While guarding, press light attack. You flick a small knife at the enemy you face. It barely hurts, but a
  light enemy flinches. You can throw it every few seconds."
- *Now*: nothing (`way-thrown-knife`).

**Zanshin** (`katana.zanshin`; Rule, 2)
- *What*: for 1 s after a kill you turn to the nearest other enemy within 8 m (P) over 0.15 s (P), and a guard raised
  in that second has a perfect window 0.05 s wider (0.18 to 0.23 s).
- *Look*: body first: the head snaps to the next enemy, then the hips follow. The design's "thin white glance line" is
  a white line: question Q19.
- *Text*: "After a kill, you turn to face the next enemy. For a moment, a perfect guard is easier."
- *Now*: only the wider window (`turn-to-face`).

**Temper I to V** and **the four infusion families**: section 2.1 and 2.2.

**Before the Thunder** (`katana.beforethethunder`; Technique, 2; gated `elite.last_retainer`)
- *Press*: R1 and R2 together.
- *Timeline*: wind-up 0.06 s (P, question Q7): no step back, the body drops 5 cm and the blade is out. Travel: a
  0.8 m (P) lunge. Impact: one flat cut. If it meets an enemy in its wind-up, that attack is cancelled, hit stop
  0.12 s. Aftermath: recovery 0.3 s (P) if it broke a wind-up; 0.6 s if it struck nothing that was winding up.
- *Numbers*: 22 damage (P, Q7), 50 stagger on a broken wind-up, 2.8 m (P), 120 degrees (P), 15 stamina (P, Q7).
- *Risk*: timing. A draw into an enemy that is not winding up is a plain cut with a long recovery.
- *Clip*: new, `attack_katana_before_thunder`.
- *Look*: one long flat trail. On a broken wind-up, a burst of warm sparks where the blades meet, and the enemy's
  wind-up pose snaps back into its stagger. Nothing floats.
- *Text*: "Press light and heavy attack together. You draw faster than anything. Hit an enemy as it winds up an
  attack, even a red one, and the attack is broken. Miss the timing and you are left open."
- *Now*: nothing, sealed (`way-technique`).

**The five element infusion marks** (built on the Iaido Slash, gated `gods.1`, each in force while its element is
carried; 2 each). They are not stand-ins; the Katana's review re-films each against the rules.

| Mark | Rule as built (`KatanaWay.cs`) | What the film must show | Text |
|---|---|---|---|
| Burning Crescent (Fire) | The Iaido Slash throws a slow arc of flame that travels on | Real flickering flame in the arc's shape, low over the floor, tongues licking up and guttering out, never a glowing crescent | "With Fire, your Iaido Slash throws a slow wave of flame ahead of it." |
| Delayed Cuts (Lightning) | The slash passes through; its cuts land a moment later, all at once, when the blade is home | The pass, a still beat, the sheathe, then bolts biting along the cut on each body at once | "With Lightning, your Iaido Slash passes through. The cuts land a moment later, all at once, when your sword is home." |
| Drawing Wind (Wind) | On a hit, all within 6 m pulled toward you and made Unsteady 2 | Curls of air spiralling in toward you from each enemy, never straight lines; the pulled stumble | "With Wind, your Iaido Slash pulls everyone nearby toward you and throws them off balance." |
| Winter Draw (Ice) | On a hit on a chilled enemy, a blade of ice flies on (18, 20 stagger, Chill 2) | The ice blade forms at the struck body and flies on, frost where it hits | "With Ice, your Iaido Slash on a chilled enemy sends a blade of ice flying on." |
| Answering Draw (Light) | The slash Marks and notches what it cuts; after a perfect guard, every blow lands critical for 1.5 s | Light's built Marked and Sundered looks on the bodies | "With Light, your Iaido Slash marks enemies and cracks their armour. After a perfect guard, your hits are critical for a moment." |

### 3.2 The Sheath (iai)

**The Sheath** (`katana.sheath`; Aspect, 2)
- *Press*: the light chain as usual.
- *Timeline*: the draw (the drawing cut, 0.10 / 0.08 / 0.24 s), the return cut (0.11 / 0.08 / 0.25 s), then the
  sheathe, 0.3 s (P). The sword also goes home after 0.6 s idle.
- *Numbers*: the draw 1.5 times with +10 stagger: 15 damage, 20 stagger. The return cut 11 and 10. The stepping cut
  is gone.
- *Risk*: the sheathe is a 0.3 s beat with no cut in it; a dodge cancels it, the guard does not (P) until Quick Sheath.
- *Clip*: the draw and the return cut as built; new `attack_katana_noto`, the fast sheathe, shared with Quick Sheath.
- *Look*: the draw's trail is longer than a cut's; the sheathe is the body's: the blade turned flat at the scabbard's
  mouth and slid home. No effect.
- *Text*: "Iai. Your sword goes back into its sheath after every combo. The first cut of each combo is a draw from the
  sheath, and it hits much harder. Each combo is the draw, a return cut, and then the sword goes home."
- *Now*: the draw's bonus and the two-cut chain; no sheathe beat (`way-sheathe`).

**Sharp Draw** (`katana.sheath.sharpdraw`; Number, 1). The draw's 1.5 becomes 1.7: 17 damage. *Text*: "Your draw from
the sheath hits even harder." *Now*: built.

**Quick Sheath** (`katana.sheath.quicksheath`; Move, 2)
- *Press*: guard in a cut's recovery.
- *Timeline*: the recovery is cut short; the sheathe plays in 0.15 s; the guard rises after it (P). The next R1 is a
  draw.
- *Numbers*: no cost (P).
- *Risk*: for 0.15 s you are neither cutting nor guarding.
- *Clip*: `attack_katana_noto` played at 0.15 s (an even share of the same motion).
- *Look*: the snap home. No effect.
- *Text*: "Press guard during a cut's recovery to sheathe at once. Your next light attack is a draw."
- *Now*: nothing (`way-sheathe`).

**Chiburi** (`katana.sheath.chiburi`; Move, 2)
- *Press*: R1 within 0.5 s (P) of a kill.
- *Timeline*: wind-up 0.06 s (P): the wrist turns. Travel: one sharp flick down and out. Impact: none, no damage.
  Aftermath: the sheathe, 0.2 s (P); 0.3 s (P) in all, and it clears the recovery of the cut that killed.
- *Numbers*: the next draw +20 stagger.
- *Risk*: none to speak of; it is a reward for a clean kill.
- *Clip*: new, `attack_katana_chiburi`.
- *Look*: the flick and the sheathe. Whether drops fly off the blade is question Q23.
- *Text*: "Right after a kill, press light attack. You flick your sword clean and sheathe it. Your next draw breaks
  balance harder."
- *Now*: the next blow after a kill staggers twice as hard, which is more than the design's +20; no flick
  (`way-sheathe`).

**Held Draw** (`katana.sheath.helddraw`; Move, 2; path A)
- *Press*: hold R1 0.4 s, then let go. How a hold is told from a press without slowing every light is question Q13.
- *Timeline*: wind-up: the hold, 0.4 s at least: you sink 15 cm, the scabbard hand pulls back, and you creep at a
  quarter speed. Travel: on release, you dash 3 m along a lane 5 m by 1 m, in 0.12 s (P). Impact: the first enemy on
  the lane, hit stop 0.1 s (P). Aftermath: recovery 0.35 s (P), ending sheathed.
- *Numbers*: 28 damage, 36 stagger; 18 stamina (P).
- *Risk*: a long, readable crouch; any blow taken while holding is a normal hit and loses the draw.
- *Clip*: new, `attack_katana_held_draw` (sink, a held loop, the release).
- *Look*: while you hold, a lane fills along the floor ahead from your feet outward, in dust lit in the slash colour
  (not white, Q1); on release, the trail runs the lane's length and the struck flinch sideways.
- *Text*: "Hold light attack to crouch and creep forward. Let go to dash and cut along the line in front of you."
- *Now*: nothing (`way-held-light`).

**Saya-biki** (`katana.sheath.sayabiki`; Rule, 2). Held 1 s or more, the lane is 7 m and cuts every enemy on it, not
only the first. *Look*: the lane on the floor grows to 7 m at the 1 s mark. *Text*: "Hold the draw for a second or
more. It goes further and cuts everyone in the line." *Now*: nothing (`way-held-light`).

**The Cut Lands at the Click** (`katana.sheath.click`; Keystone, 4)
- *Press*: hold R1 up to 2 s, then let go.
- *Timeline*: wind-up: the hold, filling to full at 2 s. Travel: the 7 m dash, 0.12 s (P). Impact: you arrive, and
  nothing happens yet: every enemy on the lane stands still 0.4 s. Aftermath: the slow sheathe (0.4 s); at the click,
  every wound opens at once: the damage, the stagger and the seam arrive together. Recovery ends with the click.
- *Numbers*: released full, 3 times the held draw (84, P reading of "3 times the draw"), through any guard, 100
  stagger.
- *Risk*: a 2 s tell; struck while holding, the draw is lost. Released early it is a Saya-biki draw.
- *Clip*: new, `attack_katana_click` (the release and the slow sheathe with a held pose).
- *Look*: the lane brightens from dull to full over 2 s (slash colour, never white); the dash; a held beat with the
  struck frozen mid-step; at the click, one thin cut opens across every body on the lane at once and along the floor
  under them, and dust lifts off the lane.
- *Text*: "Hold the draw even longer, up to two seconds. Let go when it is full. You cut through any guard, and
  everyone in the line freezes. Their wounds open when your sword clicks back into its sheath. If you are hit while
  holding, you lose the draw."
- *Now*: nothing (`way-held-light`, and "stand still on a lane" in 2.7).

**Nukitsuke Aside** (`katana.sheath.aside`; Move, 2; path B)
- *Press*: R1 with the stick held to one side (sideways part over half, P).
- *Timeline*: the draw's 0.10 / 0.08 / 0.24 s, and during the swing you step 1.5 m to that side.
- *Numbers*: the draw's 10 at 1.5 times (15, P reading: it is the chain's draw), 160 degrees.
- *Risk*: only to the stick's side; with the stick still it is the plain draw.
- *Clip*: new, `attack_katana_draw_aside`, mirrored for the other side.
- *Look*: the trail bends as you slide; dust from the planted foot. Nothing else.
- *Text*: "Hold the stick to one side and press light attack. You step that way as you draw, out of the path of
  their attack."
- *Now*: nothing (`way-sheathe`).

**Light Scabbard** (`katana.sheath.lightscabbard`; Rule, 2). A draw that lands sheathes at once when its recovery
ends (Quick Sheath for nothing), and draws within 1 s of each other cost 3 less stamina. *Text*: "When a draw hits,
your sword goes straight home. Draws close together cost less stamina." *Now*: every draw costs 3 less; no sheathe
(`way-sheathe`).

**A Hundred Days at the Shrine** (`katana.sheath.hundreddays`; Keystone, 4)
- *Press*: R1, again and again. The overhead stays on R2.
- *Timeline*: each draw is drawn and sheathed in one stroke: 0.10 / 0.08 / 0.24 s (0.42 s), with no end to the
  chain. The fourth draw in a row with no blow taken leaves its crescent hanging 0.6 s.
- *Numbers*: each draw 1.3 times (13 damage, 20 stagger), 10 stamina (7 with Light Scabbard). The hanging crescent
  cuts once for 40% (5.2, P: once to each enemy that walks into it).
- *Risk*: stamina runs out; a blow taken resets the count to zero.
- *Clip*: new, `attack_katana_draw_loop`.
- *Look*: each draw a short crescent trail. The fourth's trail stays at waist height where it was cut for 0.6 s and
  frays from its ends; whatever walks into it is cut. Whether a hanging trail is allowed is question Q18.
- *Text*: "Every light attack is a draw. You draw and sheathe in one quick stroke, again and again. If you are not
  hit, every fourth draw leaves its cut hanging in the air. Anyone who walks into it is cut again."
- *Now*: every light is a draw at 1.3 times on the drawing cut's clip; no crescent (`way-counters`, `way-clips`).

### 3.3 The Stream (flow)

**The Stream** (`katana.stream`; Aspect, 2)
- *Press*: the light chain; a dodge in any cut's recovery keeps the chain, and the next R1 carries on.
- *Timeline*: cuts 1 to 3 as built; the turning cut 0.12 / 0.10 / 0.26 s (P); the rising cut 0.12 / 0.08 / 0.30 s
  (P).
- *Numbers*: the turning cut 10 damage, 10 stagger, 200 degrees; the rising cut 14 and 14 (5.2 Q9, kept). Stamina
  10 and 12 (P). The whole chain is 56 stamina (P).
- *Risk*: a long chain drains stamina; a dodge to keep it costs more.
- *Clip*: new `attack_katana_turning_cut` (the body turns through 200 degrees, the blade low) and
  `attack_katana_rising_cut` (low to high); the design did not count these two (question Q20).
- *Look*: five trails, each its own shape: across, back, the stepping cut, a wide turning sweep, a rising stroke. No
  other effect.
- *Text*: "Flow. Your combo is five cuts instead of three. After the third come a turning cut and a rising cut. You
  can dodge in the middle of the combo and keep going."
- *Now*: five cuts, the fourth on the return cut's clip, the fifth on the drawing cut's; a dodge still ends the chain
  (`way-clips`, `way-chain-rules`).

**Light Wrist** (`katana.stream.lightwrist`; Number, 1). From the third cut on, cuts are 8% quicker. *Text*: "From
the third cut on, your cuts are faster." *Now*: built.

**River Bend** (`katana.stream.riverbend`; Move, 2)
- *Press*: R2 after cut 1, 2, 3 or 4.
- *Timeline*: wind-up 0.18 s (P), swing 0.10 s, recovery 0.30 s (P); the next R1 is the chain's next cut.
- *Numbers*: 26 damage, 30 stagger (P), 16 stamina (P).
- *Risk*: it takes the place of the chain heavy, which ends the chain, while the Stream is active.
- *Clip*: new, `attack_katana_river_bend`.
- *Look*: a short vertical trail; the struck driven down a step.
- *Text*: "Press heavy attack in the middle of your combo. You make a quick overhead cut, and the combo keeps going."
- *Now*: nothing (`way-chain-rules`, `way-clips`).

**Stepping Water** (`katana.stream.steppingwater`; Move, 2)
- *Press*: R1 at any point in a dodge. It takes the Rising Draw's input while the Stream is active (decided).
- *Timeline*: as the Rising Draw, from wherever the roll is.
- *Numbers*: 12 damage, 12 stagger, 140 degrees; counts as the chain's next step.
- *Clip*: the Rising Draw's `attack_katana_roll_cut` (decided); whether that makes it a reskin is question Q21.
- *Look*: as the Rising Draw.
- *Text*: "Press light attack at any point in a dodge. You cut out of the roll, and it counts as the next cut of your
  combo."
- *Now*: the dodge's cut in the roll's second half, outside the chain (`way-chain-rules`).

**Second Breath** (`katana.stream.secondbreath`; Rule, 2). Each cut from the fourth on that lands gives back 4
stamina. No look beyond the bar. *Text*: "From the fourth cut on, each cut that hits gives you back a little stamina."
*Now*: every landed light gives 4 (`way-chain-rules`).

**Unbroken** (`katana.stream.unbroken`; Rule, 2). A blow of 12 or less taken in the chain does not end it: you flinch
in place 0.15 s (the hurt clip cut short) and the next R1 carries on. *Text*: "A small hit during your combo does not
stop it. You flinch for a moment and keep cutting." *Now*: poise 12 in the wind-ups and swings only
(`way-chain-rules`).

**No Last Cut** (`katana.stream.nolastcut`; Keystone, 4)
- *Press*: keep pressing R1.
- *Timeline*: after the fifth cut the first follows with no pause; each loop +8%; three loops at most. At the end of
  the third loop: wind-up 0.2 s (P), the body coils; impact: all five arcs at once; recovery 0.45 s (P).
- *Numbers*: the finale 16 damage and 30 stagger to all it reaches (3 m, P). Light blows give half the mana while it
  runs. It ends on a blow over 12, the guard, or no press.
- *Risk*: stamina; one big hit ends it.
- *Clip*: new, `attack_katana_five_arcs`.
- *Look*: the design draws the finale as "one ring round you", which the no-ring rule forbids: question Q14.
- *Text*: "After the fifth cut, your combo starts again with no pause, and each time round it hits harder. It can go
  round three times. Only a big hit, your guard, or stopping ends it. At the end of the third round, you cut all
  around you at once."
- *Now*: nothing (`way-chain-rules`, `way-clips`).

**Ebb Step** (`katana.stream.ebbstep`; Rule, 2). A dodge inside the chain is a short slide: 2.4 m in 0.25 s (P) for 15
stamina. *Clip*: a slide is not a roll and not a cut; it needs its own, `roll_katana_slide` (question Q20). *Look*:
low dust from the feet. *Text*: "A dodge in the middle of your combo becomes a short, cheap slide." *Now*: nothing
(`way-chain-rules`).

**Eddy** (`katana.stream.eddy`; Rule, 2). A Stepping Water cut out of a perfect dodge is critical (1.5 times). *Look*:
the game's critical hit on the struck. *Text*: "A cut out of a perfect dodge is a critical hit." *Now*: the next blow
within 0.8 s of any perfect dodge is critical (`way-chain-rules`).

**The Water Has No Shape** (`katana.stream.noshape`; Keystone, 4)
- *Press*: dodge during the chain.
- *Timeline*: each dodge is a slide that cuts along its own line as you pass: travel 2.4 m in 0.25 s (P); impact on
  every enemy you pass beside (never through bodies); aftermath: recovery 0.15 s (P), and it counts as the chain's
  next step. A chain can be all slides.
- *Numbers*: 10 damage, 30 stagger to each passed; 15 stamina (Ebb Step's).
- *Risk*: you must pass close beside them; a slide into a body stops.
- *Clip*: new, `attack_katana_slide_cut`.
- *Look*: the trail runs along your path at knee height through those beside it; the design's "pale wake" on the floor
  is drawn as low dust in the floor's own colour, settling over 1 s (not pale and not water, so it does not read as
  the Water element; Q1).
- *Text*: "Every dodge during your combo cuts everyone you slide past, and counts as the next cut. Your whole combo
  can be slides."
- *Now*: nothing (`way-chain-rules`, `way-clips`).

### 3.4 The Answer (counter)

**The Answer** (`katana.answer`; Aspect, 2), with its two moves
- *Press*: guard perfectly (0.18 s window), then R1 for the Riposte or R2 for Through the Gap within 0.6 s.
- *Timeline, Riposte*: wind-up 0.06 s (P): the guard snaps straight into the cut. Travel: a 0.5 m lunge (P). Impact:
  hit stop 0.08 s (P). Aftermath: recovery 0.3 s (P).
- *Timeline, Through the Gap*: wind-up 0.08 s (P); travel 3 m through the attacker, untouchable while passing (P);
  impact as you pass; aftermath: you stand behind it, turned to face it, recovery 0.35 s (P).
- *Numbers*: the Riposte 24, critical (36), 20 stagger. Through the Gap 30 and 40 stagger. Light blows outside the
  answer 10% less.
- *Risk*: it needs a perfect guard; a late guard opens nothing.
- *Clip*: new `attack_katana_riposte` and `attack_katana_through_gap` (question Q20); now the stepping cut's and the
  Iaido Slash's.
- *Look*: how the open window shows is question Q12 (the design's "white flick down the blade" is white and is drawn
  on the blade, both against the rules). The Riposte: one straight trail. Through the Gap: the body passes the
  attacker's shoulder and the trail is cut behind it.
- *Text*: "Counter. A perfect guard gives you a moment to answer. Press light attack to riposte with a critical cut.
  Press heavy attack to slip past and cut them from behind. Your normal light cuts do a little less damage."
- *Now*: only the light blows a tenth weaker; the window does not exist (`way-answer-window`).

**Turned Edge** (`katana.answer.turnededge`; Number, 1). A perfect guard throws more stagger (built: +8 flat for the
design's 25%, 5.2 Q9 kept). *Text*: "Perfect guards break the attacker's balance faster." *Now*: built.

**Open Window** (`katana.answer.openwindow`; Rule, 2). The answer lasts 0.8 s. *Text*: "You have longer to answer."
*Now*: nothing (`way-answer-window`).

**Patient Blade** (`katana.answer.patientblade`; Rule, 2). Each 0.5 s the guard is held unstruck, the next answer +10
stagger, to +40. *Look*: the weapon's own highlight (the one thing allowed on it) steps up once each 0.5 s, four steps
(P). *Text*: "The longer you hold your guard without being hit, the harder your next answer breaks balance." *Now*:
nothing (`way-answer-window`, `way-counters`).

**Guard Cut** (`katana.answer.guardcut`; Move, 2; path A)
- *Press*: guard held, then R1 (takes Kozuka's input in the Answer).
- *Timeline*: wind-up 0.08 s (P), swing 0.06 s (P), recovery 0.2 s (P). The guard stays raised throughout.
- *Numbers*: 8 damage, 6 stagger (built), 1.8 m (P), 90 degrees (P), 8 stamina (P). It does not chain.
- *Risk*: small; it spends stamina the guard needs.
- *Clip*: new, `attack_katana_guard_cut`, upper body over the guard pose.
- *Look*: a short trail from behind the guard.
- *Text*: "While guarding, press light attack. You make a short cut and keep your guard up."
- *Now*: the guard's light attack on the drawing cut's clip.

**Second Answer** (`katana.answer.secondanswer`; Rule, 2). The answer stays open for a second perfect guard; a second
answer in one window +30 stagger. *Text*: "Your moment to answer stays open for a second perfect guard. A second
answer breaks balance harder." *Now*: nothing (`way-answer-window`).

**Kiri-otoshi** (`katana.answer.kiriotoshi`; Keystone, 4)
- *Press*: guard perfectly; the Riposte follows by itself; R2 within 0.3 s makes it Through the Gap.
- *Timeline*: as the Riposte, starting at once on the perfect guard.
- *Numbers*: the perfect window 0.03 s wider (built as +17%); the light chain is two cuts (built); the Riposte runs on
  4 m past the attacker and strikes whoever stands behind for half (12).
- *Risk*: the answer is spent at once; you cannot hold it for a better moment.
- *Clip*: the Riposte's.
- *Look*: the Riposte's trail carries on through the bodies behind, at body height (the design's "white seam" in the
  slash colour, Q1).
- *Text*: "Your answer comes on its own. Every perfect guard is followed by a riposte right away. Press heavy attack
  quickly to slip past instead. Perfect guards are a little easier, and your light combo is two cuts. The riposte
  cuts on past them and hits whoever is behind."
- *Now*: the wider window and the two-cut chain (`way-answer-window`).

**Catch the Red** (`katana.answer.catchthered`; Rule, 2; path B). After a perfect guard on a red attack (a perfect
guard turns even an unblockable blow, `Guard.cs:69`) the answer lasts 1.0 s. *Text*: "After a perfect guard on a red
attack, you have much longer to answer." *Now*: nothing (`red-attack`, `way-answer-window`).

**Read the Red** (`katana.answer.readthered`; Rule, 2). Against red attacks the perfect window is 0.04 s wider.
*Text*: "Red attacks are easier to guard perfectly." *Now*: nothing (`red-attack`).

**The First Sword** (`katana.answer.firstsword`; Keystone, 4)
- *Press*: guard a red attack perfectly, then R1 in the answer.
- *Timeline*: the turned red blow; time nearly stops for 0.3 s (the world at a twentieth of its speed, P) and the
  camera leans in (`TopDownCamera.CloseIn`). Wind-up 0.05 s (P). Travel: one cut through the attacker, 4 m (P),
  passing through. Impact as you pass. Aftermath: you stand behind it, time returns, the attacker reels; recovery
  0.4 s (P).
- *Numbers*: 3 times the Riposte (72), 120 stagger, gods included. Perfect guards on other blows answer as the Answer
  only.
- *Risk*: it waits for red attacks, which are rare and dangerous; a mistimed guard eats the red blow.
- *Clip*: new, `attack_katana_first_sword`.
- *Look*: the stopped moment (the world's motion stilled, sparks hanging where the blades met), then one long trail
  through the body and out.
- *Text*: "A perfect guard on a red attack stops time for a moment. Your riposte becomes one cut straight through
  them, even a god. Perfect guards on other attacks answer as normal."
- *Now*: nothing (`red-attack`, `way-answer-window`, `way-clips`).

### 3.5 Narukami's Storm Katana (Lightning; the god's bough)

**Narukami's Storm Katana** (`katana.storm`; Aspect, given by Take)
- *Press*: as the Katana; the skill is the Thunder Draw.
- *Timeline*: the chain is his: across (0.10 / 0.08 / 0.24 s), back (0.11 / 0.08 / 0.25 s), and down: the third cut
  falls straight down a lane, 0.16 / 0.10 / 0.38 s (P, the stepping cut's times).
- *Numbers*: +15% damage and stagger on every built move; the third cut 22 in a lane 3 m by 1.2 m; the Thunder Draw
  55 and 57.5, carrying 3.6 m.
- *Clip*: new `attack_storm_falling_cut` (question Q20); now the stepping cut's.
- *Look*: his sword, longer, storm-grey, with lightning crawling down the fuller (the model's own material, not an
  effect) and the shrine's paper streamers at the hilt. The design asks the sword be "longer by 0.3 m"; the built
  Storm Katana is longer only on its skill (`Weapons.cs:626` to 627), so the spec adds +0.3 m reach to its lights.
- *Text*: "Narukami's own sword. It is longer, and every hit is stronger. Its skill is the Thunder Draw. Its combo is
  his: across, back, then straight down along a line."
- *Now*: the falling third cut on the stepping cut's clip; no extra reach on the lights.

**Long Blade** (`katana.storm.longblade`; Number, 1). Reach +0.2 m on every move. *Text*: "Every cut reaches a little
further." *Now*: built.

**Flank Step** (`katana.storm.flankstep`; Move, 2)
- *Press*: dodge, then R1 within 0.2 s.
- *Timeline*: wind-up 0.06 s (P): a crouch. Travel: a bolt forms between your feet and the faced enemy's side (up to
  6 m) a beat before you move, and you cross it in 0.12 s (P). Impact: the cut as you land, at its side. Aftermath:
  recovery 0.3 s (P); the bolt's scorch fades in 1 s.
- *Numbers*: 18 damage, 20 stagger (P); 12 stamina (P).
- *Risk*: it needs a faced enemy within 6 m; you land beside it, in reach of its friends.
- *Clip*: new, `attack_storm_flank_step`.
- *Look*: cause, the bolt laid ahead; travel, you flicker along it with sparks crawling on your body; impact, the cut;
  aftermath, a thin branching scorch on the floor along the path.
- *Text*: "Right after a dodge, press light attack. You flash along a bolt of lightning to the enemy's side and cut
  as you land."
- *Now*: nothing (`way-god-moves`).

**Seven Drums** (`katana.storm.sevendrums`; Rule, 2)
- *What*: each cut that lands beats one of seven drums; with seven beaten, the next draw +40 stagger. Drums fade out
  of a fight. Which moves count as "a draw" on the Storm Katana is question Q22.
- *Look*: the design draws the drums "round the guard", a ring floating off the body: question Q15.
- *Text*: "Each cut that hits beats one of seven drums. When all seven are beaten, your next draw breaks balance much
  harder."
- *Now*: nothing (`way-counters`).

**The Red Draw** (`katana.storm.reddraw`; Move, 2)
- *Press*: hold R2.
- *Timeline*: wind-up: sheathe and sink, held 0.6 s. Travel: through 10 m along a lane 1.2 m wide in 0.15 s (P),
  untouchable. Impact: every enemy on the lane as you pass. Aftermath: you stand at the far end; recovery 0.4 s (P).
- *Numbers*: 40 damage, 50 stagger, 40 mana.
- *Risk*: the 0.6 s crouch is a tell; 40 mana; it can carry you into a crowd.
- *Clip*: new, `attack_storm_red_draw`, after the boss's own red draw.
- *Look*: as you hold, sparks crawl along the floor down the lane ahead (the tell, cyan); you go through as a streak
  of your own body's sparks; the struck jolt in turn behind you; a scorch line fades in 1 s.
- *Text*: "Hold heavy attack. You sheathe and crouch, then flash through everyone in a long line and stop behind
  them. Nothing can hurt you as you pass. It costs mana."
- *Now*: the held heavy on the Thunder Draw's clip, free (`way-god-moves`). The free first pass is the owner's 5.2 Q8
  default; the build charges the 40 mana once the engine can.

**The Falling Cut** (`katana.storm.fallingcut`; Rule, 2). His third cut's lane +1 m (4 m), and a light enemy it
strikes is driven to one knee for 0.6 s (knocked down, built in Phase 3 task 3.7). *Text*: "His downward cut reaches
further, and it knocks light enemies to one knee." *Now*: only the longer lane.

**Thunder Follows** (`katana.storm.thunderfollows`; Rule, 2; path A)
- *What*: every draw leaves its lane on the floor; 0.5 s later thunder runs down it from you and strikes all on it
  again for half the draw and 30 stagger: the flash, then the sound.
- *Look*: cause, a faint scorch under the draw's path; travel, 0.5 s later a bolt runs along it from your end to the
  far end; impact, sparks crawl over each body it passes; aftermath, branching scorches fade over 1.5 s.
- *Text*: "Each draw leaves a mark on the floor. Half a second later, thunder runs along it and hits everyone there
  again."
- *Now*: a 6 m lane from what the Thunder Draw struck, 0.5 s later, 27 and 30 (`way-god-moves`).

**Long Thunder** (`katana.storm.longthunder`; Rule, 2). Raikiri's lane +4 m (16 m), and it bends up to 20 degrees
toward the faced enemy. *Text*: "Raikiri reaches further and bends toward the enemy you face." *Now*: nothing.

**Raikiri, the Cut Through Lightning** (`katana.storm.raikiri`; Keystone, 4, a god move)
- *Press*: hold the skill button (Triangle, Y or Q) past the Thunder Draw, 1.2 s.
- *Timeline*: wind-up: the hold; for its last 0.6 s you raise the sword overhead and a bolt comes down onto you (a
  column of pale cyan light, the tell). Travel: the down cut throws a branching bolt down a 12 m lane in 0.15 s (P).
  Impact: all on the lane. Aftermath: the lane stays scorched 2 s (look only, P); recovery 0.5 s (P).
- *Numbers*: 60 damage, 80 stagger, 50 mana.
- *Risk*: 1.2 s rooted is the longest tell the Katana has; struck while raising, the move is lost and the mana kept
  (P).
- *Clip*: new, `attack_storm_raikiri` (the raise and the down cut).
- *Look*: the streamers stand up in the updraught (on the model); the column lands on you; then the bolt down the
  lane. The design puts the bolt "on the edge" and a "thunderclap ring" on the floor, against two rules: question Q17.
- *Text*: "Hold the skill button past the Thunder Draw. You raise your sword, and lightning strikes it. Then you cut
  down, and the lightning tears along a long line in front of you. It costs a lot of mana."
- *Now*: nothing (`way-god-moves`).

**Drumbeat** (`katana.storm.drumbeat`; Rule, 2; path B). Perfect guards and finishers each beat two drums. *Text*:
"Perfect guards and finishing blows each beat two drums." *Now*: nothing (`way-counters`).

**Rolling Drums** (`katana.storm.rollingdrums`; Rule, 2). Drums no longer fade between fights; seven beaten wait until
spent. *Text*: "Your drums do not fade after a fight. Once all seven are beaten, they wait until you use them."
*Now*: nothing (`way-counters`).

**The Drum Ring** (`katana.storm.drumring`; Keystone, 4, a god move)
- *Press*: the skill (the Thunder Draw) with seven drums beaten.
- *Timeline*: wind-up 0.2 s (P): a drummer's stance. Travel and impact: seven bolts over 0.7 s, one each 0.1 s,
  round you at 2 m (4 m across); each place shows its tell 0.1 s before its bolt. Then the last bolt comes down on you
  and out down the draw's lane. Aftermath: scorches fade over 1.5 s; recovery 0.4 s (P).
- *Numbers*: 14 to all within 1.5 m of each strike; the last 40 down a 6 m lane (P, as Thunder Follows' built lane);
  the Thunder Draw's 30 mana; the drums spent.
- *Risk*: it needs seven drums; enemies outside 3.5 m are untouched until the lane.
- *Clip*: new, `attack_storm_drum_stance`.
- *Look*: the design stands seven cloud drums in a ring 4 m across: question Q16.
- *Text*: "When all seven drums are beaten, your Thunder Draw calls lightning all around you. Bolts strike one after
  another around you. The last one hits you and flies out along your draw."
- *Now*: nothing (`way-counters`, `way-god-moves`).

### 3.6 The Katana's clip list and stand-in count

| # | Clip | For | In `way_plan.md` 11? |
|---|---|---|---|
| 1 | `attack_katana_roll_cut` | Rising Draw, Stepping Water | Yes (basics 3) |
| 2 | `attack_katana_tsuka_ate` | Tsuka-ate | Yes |
| 3 | `attack_katana_kozuka` | Kozuka (and the knife prop) | Yes |
| 4 | `attack_katana_noto` | the Sheath's sheathe, Quick Sheath | Yes (Sheath 6) |
| 5 | `attack_katana_chiburi` | Chiburi | Yes |
| 6 | `attack_katana_held_draw` | Held Draw, Saya-biki | Yes |
| 7 | `attack_katana_click` | The Cut Lands at the Click | Yes |
| 8 | `attack_katana_draw_aside` | Nukitsuke Aside (mirrored) | Yes |
| 9 | `attack_katana_draw_loop` | A Hundred Days | Yes |
| 10 | `attack_katana_river_bend` | River Bend | Yes (Stream 3) |
| 11 | `attack_katana_five_arcs` | No Last Cut's finale | Yes |
| 12 | `attack_katana_slide_cut` | The Water Has No Shape | Yes |
| 13 | `attack_katana_guard_cut` | Guard Cut | Yes (Answer 2) |
| 14 | `attack_katana_first_sword` | The First Sword | Yes |
| 15 | `attack_storm_flank_step` | Flank Step | Yes (Storm 4) |
| 16 | `attack_storm_red_draw` | The Red Draw | Yes |
| 17 | `attack_storm_raikiri` | Raikiri | Yes |
| 18 | `attack_storm_drum_stance` | The Drum Ring | Yes |
| 19 | `attack_katana_before_thunder` | Before the Thunder | No (techniques were not counted) |
| 20 | `guard_hit_katana`, `guard_perfect_katana` | every guard; the Answer's window starts on the perfect one | No (E13 retrofit) |
| 21 to 26 | `attack_katana_turning_cut`, `attack_katana_rising_cut`, `roll_katana_slide`, `attack_katana_riposte`, `attack_katana_through_gap`, `attack_storm_falling_cut` | the Stream's two new cuts, Ebb Step, the Answer's two moves, the Storm's third cut | No: question Q20 |

Up to 27 clips, all built in code by the Forge. Of the Katana's 62 marks, 15 are whole as written: the five element
marks and the temper ranks' stats work in play today, and Sharp Draw, Light Wrist, Long Blade, Turned Edge and Settled
Hands will once 2.0 wires the patches in (Temper III and V's own lines also wait on 2.0, and V's held pose on Q5). The
rest are ledgered stand-ins; none is reported done until it is filmed and checked (section 5).

---

## 4. The other nine Ways

Each follows the Katana's checklist (section 5) one Way a session (task 5.7+), after the Katana is reviewed. The
outlines are the decided design (`way_plan.md` sections 2 to 10, fork option 1 and god's option (a) everywhere, the
Swath's keystone renamed The Long Stroke). "WP" is `way_plan.md` and "W" is `weapons.md`, with line numbers. The gaps
below are the ones visible now, so their questions can be asked early (section 6, Q24 on).

**Across all nine.** About 13 marks draw a ring as their effect and about 9 call for white or "pale"; a few draw on
the weapon itself. Every weapon's second basic is the same roll-then-R2 shove (45 stagger, a guard broken 1 s), and
most fourth basics are "its next attack comes late"; the trunk frame was decided (G4), but under the no-reskin rule
each needs its own body and its own way of doing it. Several marks need enemy states nothing has yet: dazed,
disarmed, held or pinned (E12), hooked, and anchors in the scenery. Of the gods' weapons only the Storm Katana and the
Sun-Spear are built (`Runtime/Player/NotBuiltYet.cs` lists the rest); the War Fan and Kazehaya's Fan are not built at
all.

### 4.1 Massive Sword (16 new clips)

The trunk gives Roll and Rise, Shoulder Into Them, the Parierhaken (the side-hooks catch a weapon; its next attack
0.5 s late) and Brace. **The Doppelsöldner** (in place of the Wall) carves crowds: The Forlorn Hope (six chained
sweeps into a two-turn whirl) or Durendal Cleaves the Mountain (one blow splitting the floor down a 7 m lane).
**Half-Sword** swaps grips: Two Swords in One (the grips woven in one chain) or The Cross Comes Down (the reversed sword
as a hammer, three blows). **The Trade** wants to be hit: Bear-Shirt (trade all the time, with the roar) or Tied to
the Stone (the last stand below a quarter health). **Brannoch's Standing Stone** (Earth) plants stones: The Giant's
Dance (every stone turns round you) or The Giant's Throw (a stone heaved 12 m). Technique: The Return Blow, from the
Bargain (Earth).

- Rings: Bear-Shirt's "red shout ring 4 m" (WP 475), the stones' dust rings (WP 497, 511, 522).
- On the blade: the Trade's opening says "a red spark runs down the blade" on a trade (WP 460); Two Swords in One
  flashes the hand sliding along the blade (WP 440).
- No numbers for the Forlorn Hope's whirl (WP 402) or Bear-Shirt's roar (WP 475).
- Engine: dazed (Ring the Helm), a weapon knocked aside, a grip change, stones that block shots.
- Not built: Brannoch's Stone.

### 4.2 Scythe (17)

The trunk gives Mow Out of the Roll, the Snath Butt, Haft Spin and Step to the Tip. **The Swath**: Mow the Field
(stroke after stroke through a crowd) or The Long Stroke (one stroke 6 m round, 40 at the tip). **The Hook**: Nothing
Leaves the Curve (a crowd held at the tip) or Fell the Giant (three Hamstrings bring an elite down). **The Harvest**:
The Last Sheaf (a straw decoy) or The Threshing Floor (eight sheaves beaten out). **Carnwyn's Golden Scythe** (Life):
The Wild Hunt (a horn, and the herd rides down a lane) or The Grove Answers (every thorn stand bursts into gold
flowers). Technique: Over the Shoulder, from the Wicker Man (a Proposal).

- Rings: Mow the Field's ring stroke (WP 570), the Threshing Floor's ring of chaff (WP 644), Carnwyn's ring of thicket
  (WP 660). The Long Stroke is a full turn by its mechanic; its look must be a sweep, not a drawn ring.
- White: "the white hounds", "hounds of white bark" (WP 676, 684).
- On the weapon: sheaves "tied on the haft" (WP 626).
- Overlap: Haft Spin repeats the Spear's Spin the Shaft (WP 549, 714); Glean and Harvest Home heal beside Life's tree.
- Engine: Hooked; held (E12); knocked down exists (Phase 3).
- Not built: Carnwyn's Scythe.

### 4.3 Great Spear (19)

The trunk gives Rising Point, the Sauroter, Spin the Shaft and Out to the Point. **The Long Point**: Nobody Inside
(no one stands inside the point) or The Hundred Birds (a fan of quick thrusts, then one 7 m thrust). **The Pole**:
Never Touch the Ground (vault from back to back) or As Long as You Like (the shaft grows to 9 m for one slap).
**The Cross**: Thrust, Cleave, Slash (three uses on one enemy) or The Moon in the Pond (the crossbar drags a crowd in).
**Khepharu's Sun-Spear** (Fire, built as a weapon): Bare-Handed King (burning fists while the spear stands) or The
Winged Disc (the thrown spear circles the enemy in fire). Technique: Hold the Gate, from the Champion of the Gate
(decided, task 5.3).

- Rings: Nobody Inside's "pale ring at 1.4 m" (WP 735), Plant the Noon's ring of fire (WP 831), the Winged Disc and
  the Ring of the Disc (WP 842, 844), the Moon in the Pond's ripples (WP 807).
- Reskin: Bare-Handed King's jab and cross "reuse the fists'" (WP 841).
- Overlap the design itself flags: Plant the Noon beside The Obelisk and Hold the Gate.
- No numbers for Nobody Inside's thrust or Bare-Handed King's backhand.
- Check: the Champion already shows `champion_plant` into `champion_reap` (the task's "check").

### 4.4 Great Hammer (12)

The trunk gives Roll and Drop, Haft Shove, Butt Spike and Raised Early. **The Quake**: The Floor Gives Way (three drops
on one spot break the floor) or The Earth-Shaker Walks (a quake that strides forward). **The Swing**: Never Let It
Stop (the endless turn, with the dust devil) or It Always Comes Back (the thrown hammer finds three). **The Anvil**:
The Anvil Does Not Move or Three Strokes on the Anvil (three rings that call the giant). **The Smith's Hammer** (Metal;
Heat from 0 to 100): The Smith's Gift (a blade forged in the fight) or Drive the Nail (a rivet struck home).
Technique: The Smith's Tap, from the first golden handmaiden (a Proposal).

- Rings: the Earth-Shaker's ring of dust (WP 898), the Anvil's ripple of sound (WP 961), Three Strokes' rings (WP 964).
- White and Metal: the Smith's Gift's "white-orange" blade, Drive the Nail's "white-hot" rivet (WP 1004, 1006). Heat's
  molten drops and sparks read as fire; Metal should be only a glint or a small forged blade, and the Gift raises a
  3.4 m sword.
- Reskin: the Smith's Gift swings the Massive Sword's sweeps (WP 998); Drive the Nail is two beats like The Smith's Tap
  and Quench.
- No numbers for the Floor Gives Way's crater or Never Let It Stop's turns once stamina is free.
- Not built: the Smith's Hammer and Heat.

### 4.5 Twin Knives (14)

The trunk gives Out of the Spin, Knee and Elbow, Throw a Knife and Parry Breath. **The Shadow** (Baguazhang): They
Never See You (a crossing leaves the enemy unable to turn) or Eight Changes (cross a crowd one back at a time).
**The Rattle** (kali): The Hundred Cuts (nicks counted to 100 and opened at once) or The Woven Storm (a walking whirl
of cuts). **The Reverse Grip** (Wing Chun): Take Their Blade (disarm and throw it back) or Guard and Strike as One
(every block answered). **The Alchemist's Hooks** (Poison): The Last Elixir (all five stances at once) or Heaven and
Earth (the joined hooks whirled overhead). Technique: Fillet, from the blade brother (decided).

- White: the Rattle's "white tick" nicks and the Hundred Cuts' "white lattice" (WP 1079, 1094).
- Floating: the Woven Storm's lattice in the air round you (WP 1097); Eight Changes' figure on the floor (WP 1060).
- Poison's look: mercury beads (WP 1167, 1174) read as Metal, not miasma.
- Overlap the design flags: Guard and Strike as One beside the Katana's Kiri-otoshi (6.3); Five Vials beside
  Fūrinkazan.
- Engine: disarmed; a stance counter for five flags.
- Not built: the Alchemist's Hooks.

### 4.6 Great Bow (13)

The trunk gives Roll and Loose, Stave Shove, the Hand-Thrown Arrow and Kyūdō Breath. **The Stave** (the bow-spear,
yumi-yari): No Range (point-blank shots throw the enemy) or The Spear That Shoots (every thrust looses an arrow).
**The Snare**: Gleipnir (lines that bind) or The Trapper's Ground (lines that shoot). **The Long Draw**: Through
Twelve Axes (one arrow pins a line) or Yoichi's Fan (the far shot). **The Huntress's Bow** (Ice): Skaði's Circle
(skate round the quarry loosing five) or Ski and String (skate on, loosing fans). Technique: Only the Eye, from the
Teacher (decided; not built as an enemy).

- Code against design: the Stave's opening changes a third stave light, but the bow has two (`attack_bow_1` and
  `attack_bow_2`, `Weapons.cs:481` to 482).
- Rings: Yoichi's Fan's ring at 10 m (WP 1290), Skaði's ring of frost (WP 1329), Only the Eye's closing ring.
- White: "blue-white ice" (WP 1308).
- Engine: pinned or held (E12), the skate dodge, a held aim, a quarry mark.
- No numbers for Through Twelve Axes, Ski and String's arrows, Gleipnir's bind.
- Not built: the Huntress's Bow.

### 4.7 Chain Sickle (17)

The trunk gives Roll and Snap, the Chain Bar, Wrap the Weapon and the Taut Guard. **The Weight**: Always Turning (a
second ring of the weight) or The Shooting Star (the weight falls as a meteor). **The Bind** (hojōjutsu): Whirled and
Flung (a bound body as a flail) or Nothing Gets Away (every weight blow binds). **The Sickle** (Isshin-ryū): Reap the
Neck or Near and Far (every sickle light also snaps the weight). **The Fisher's Jawbone** (Water): Fish Up the Island
(an islet hauled up on a swell) or Noose the Sun (five lines, all slowed). Technique: Hook and Hold, from the hook
brother (decided).

- Rings: the weight's turning reach is a mechanic; Always Turning's "second ring" must read as the weight's path, not
  a drawn ring.
- Engine: Hooked and anchors in the scenery (W 17); Round the Post needs an anchor and sits near The Line Carries You.
- Overlap: Whirled and Flung beside the Fists' Milo's Bull.
- No numbers for Nothing Gets Away.
- Not built: the Jawbone.

### 4.8 Fists (10)

The trunk gives Roll Into the Body, the Push Kick, the Elbow and Roll With It, and two techniques (one set on the
chord at a fire). **Wrapped Hands**: Stand In (the slip is the dodge) or No Rounds (punch without stopping, the
haymaker at twenty). **The Hold**: Herakles and Antaeus (elites lifted and crushed) or Milo's Bull (a body lifted and
slammed into a crowd). **The Armament** (the third bough, plain size): Golden Bell (a coat that grows) or Every Blow
Returned (blows stored and given back). No god's bough. Techniques: Bear Them Down (the pack's alpha) and The
Scorpion's Tail (the Undying Emperor), both teachers Proposals.

- The cleanest Way against the look rules.
- Engine: a grab and a lift (the enemy held in the air, E12); the slip dodge; a leap onto an enemy.
- Which technique is on the chord before either is chosen (5.1 Q7: both, the chord takes the first) needs its order
  set.
- The Armament is its own weapon on the rack and counts as the Fists' kind (5.1 Q5).

### 4.9 War Fan (11, plus about 12 for the base set)

Not built at all; `docs/design/weapons.md` has no War Fan section, and the base set is a Proposal (W 1045). The trunk
gives Open Out of the Roll, the Rib Strike, Fan the Eyes and Read the Wind. **The Iron Fan**: Ushiwaka on the Bridge
(every turned blow strikes back) or The Thousandth Sword (ten delays, then one sword-blow). **The Commander's Fan**:
The Field Is Mine or Fūrinkazan (four stances in turn). **Kazehaya's Fan** (Wind): The Gale Road (ride a gale 14 m)
or Storm of Leaves (a cone of cutting leaves). Technique: Turn the Spear, from the Hermit of the Pass.

- The base moveset must be designed and built first (decided, 10.4), about a dozen clips.
- Input clash: the plain fan's Thrown Fan is R2 held 0.4 s (W 1049) and the Commander's signal is R2 held 0.3 s
  (WP 1688).
- Wind's look: "a pale streak" and "thin cut of wind" (WP 1741, 1742) are lines; Wind is curls.
- Rings: Ushiwaka's ring of dust (WP 1670). On the weapon: notches cut on its ribs (WP 1673).
- The Hermit is a placeholder in W 14 and W 1114 but decided in WP 1638 and `README.md`; one of them is stale.
- Engine: disarmed; cannot guard.

---
