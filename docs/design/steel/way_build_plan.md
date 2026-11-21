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
