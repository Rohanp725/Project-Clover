# The eight remaining techniques: design for building

Written by Claude on 2026-11-16 from the user's request: "start on the rest of the weapons and elements, and build out
the framework for all of those ... be creative come up with super cool, flashy, and interesting moves", with the
standing emphasis on "getting good animations for each action" and the complaint about an early finisher: "the
animation is basically just a slash through the person, it isnt really cool". **Everything here is Claude's draft,
none of it is decided, and none of it has been played.** It is written so that a programmer-animator can build each
weapon without asking a question; where a choice was made for the user (which chain weapon, which scythe idea, which
bow idea) it is marked **Choice** and says why.

Covers: Massive Sword, Twin Knives, Chain Sickle, Scythe, Great Bow, Great Spear, Great Hammer, Full Body Armament.
Super Form is out of scope.

## 0. How to read this

**What was read.** `docs/GAME.md`; `Weapons.cs`, `Weapon.cs`, `WeaponCombo.cs`, `AttackStep.cs`, `PlayerCombat.cs`,
`PlayerAnimator.cs`, `Guard.cs`, `DodgeRoll.cs`, `PlayerDefence.cs` (fields), `Hazards.cs`, `EnemyKind.cs` (enemy
timings and damage); `Forge/ForgeRig.cs`, `ForgePose.cs`, `ForgeClip.cs`, `PlayerAnimations.cs`; `PlayerBuilder.cs`;
`staging/blender/2026-11-15_player/notes.md`; `docs/design/TREE_BRIEF.md`. Other workers were changing
`Runtime/Player` and `Runtime/UI` while this was written, so check field names against the code before building.

**The yardstick.** Everything is sized against the two weapons that exist:

| | light chain | time | damage | stagger | stamina | heavy (windup / total) |
|---|---|---|---|---|---|---|
| Fists | 4 blows | 1.62 s | 36 | 57 | 35 | 0.24 / 0.73 s, 18 damage, 34 stagger |
| Katana | 3 cuts | 1.50 s | 39 | 42 | 34 | 0.30 / 0.86 s, 26 damage, 34 stagger |

Stamina is 100 and comes back at 30 a second after 0.8 s. Enemy openings to fit a swing into: the soldier recovers
for 0.6 to 0.9 s, the acolyte 0.5 s, the captain 0.8 to 1.1 s, Narukami 0.6 to 1.0 s after a chain. So no wind-up
here is longer than 0.50 s except inside a finisher (where blows pass through the player) and no plain attack is
longer than 1.30 s; every recovery can still be cut by a dodge or a guard, as now.

**Tables.** Columns are the fields of `AttackStep` exactly: `Name`, `Sweep`, `Windup`, `Active`, `Recovery`,
`Damage`, `Stagger`, `StaminaCost`, `ManaGain`, `Lunge`, `Reach`, `ArcDegrees`, `HitStopSeconds`, `Shake`. `Heavy`
is true for every row from the heavy down. A last column, **new**, holds fields that do not exist yet; each is
defined once in section 1 and listed again in the appendix. Slots are those of `WeaponCombo` (`Steps`, `Heavy`,
`ChainHeavy`, `Skill`, `Finisher`, `SkillManaCost`, `ChargeTime`) plus the new slots of section 1.

**Poses.** Written in the Forge's own terms (`ForgePose`): character space, x to the right, y up, z ahead, metres;
hips, waist (spine and chest), head, hands and feet as places, `Blade` as the direction the weapon points from the
right hand, `TwoHanded` with `GripGap`. Every attack follows the house rule in `PlayerAnimations.cs`: a wind-up that
goes the other way first, a held beat, a blow that arrives fast, a follow-through past the target, a settle; hips
and chest lead, arms arrive after, the head stays on the enemy (`Looking`). Each attack is built with the existing
`Attack(step)` helper so the blow falls on `Windup / Total` and the body rings afterwards (`ForgeClip.Impact`).
"From above" says what the weapon's tip draws on the ground, since at 55 degrees and 12 m that is most of what
the player sees.

**Clip names.** One prefix per weapon: `gsword`, `knife`, `chain`, `scythe`, `bow`, `spear`, `hammer`, `arm`.
`idle_<p>`, `run_<p>`, `guard_<p>`, `guard_hit_<p>`, `guard_perfect_<p>`, `charge_<p>`, `attack_<p>_1..n`,
`attack_<p>_heavy`, `attack_<p>_chain`, `attack_<p>_charged`, `attack_<p>_skill`, `attack_<p>_finisher`, and
`roll_<p>` where the dodge is its own. Attack clips are picked up by name already (`PlayerBuilder.AnimatorController`
makes a state for every clip starting `attack_`, and `AttackStep.Name` picks the state).

**The rhythms, side by side** (no two alike; the test of this document):

| Weapon | Its beat | What it asks that nothing else does |
|---|---|---|
| Fists (built) | four quick, even blows | get inside and break balance |
| Katana (built) | two cuts and a step, from the sheath | the draw |
| Massive Sword | three wide sweeps, each wound up faster than the last: slow, quicker, quickest | commit, and trade blows on purpose |
| Twin Knives | five in a rattle, the third a double, the fifth carries you past | be behind them |
| Chain Sickle | near, far, near, all round: short, long, short, long | hold two distances at once |
| Scythe | long, short, long: a pendulum that swings out and back | keep them at the tip, never close |
| Great Bow | swat, stab, step off, loose | plant at range, cash in up close |
| Great Spear | a metronome of thrusts from behind the guard | own one line and attack without lowering the guard |
| Great Hammer | two blows, the second slower than the first, onto a spot on the ground | hit where they will be, and guard early |
| Full Body Armament | the fist chain, ended four different ways | choose the ender; stand in the blow and throw it back |

---

## 1. Engine work: the framework all eight stand on

Described once here. Each weapon lists which items it needs. E1 and E2 are needed by every weapon; the rest are
shared by two or more, except E14.

### E1. A weapon says how it is carried and defended (all weapons)

Today `Weapon.UsesSword` chooses between two looks, `PlayerAnimator` chooses locomotion and guard states from it, and
`Guard` and `DodgeRoll` have one set of numbers for every weapon. Replace with, on `Weapon`:

- `Carry` (string): which set of objects on the character belongs to this weapon (see E2). `UsesSword` becomes
  `Carry == "katana"`.
- `LocomotionState`, `GuardState`, `RollState`, `GuardHitState`, `PerfectGuardState` (strings; the Katana's are
  `locomotion`, `guard`, `roll`, and the two new ones of E13). `ChargeAnimation` exists already.
- `GuardProfile` (a `Guard`) and `DodgeProfile` (a `DodgeRoll`): on `PlayerCombat.Equip` their public numbers are
  copied onto `PlayerDefence.Guard` and `PlayerController.Dodge`. Null means the defaults in those classes.
- `MoveFactor` (default 1) and `GuardMoveFactor` (default 0.45, today on `PlayerController`): multiply `MoveSpeed`.
- `Stowed` (bool): whether the weapon leaves the hand when idle. True only for the Katana (its sheath). Great
  weapons stay in the hand always, which removes the sheathing logic for them (`SheatheDelay`).
- `Weapons.ByName` becomes a dictionary of name to builder, and `Weapons.All` yields all ten, so the existing check
  that every step has a clip covers the new weapons the day they are added.
- `PlayerAnimations.SwordIn(clip)` becomes "which carry does this clip belong to", read from the clip's prefix, so
  each contact sheet is drawn with the right weapon in the hand.

### E2. Weapon models, sockets, and a second aimed hand (all weapons)

- **Sockets.** The Katana is three objects inside `player.fbx` (`katana_drawn` on the right hand, `katana_hilt` and
  `katana_sheath` on the hips). New weapons are not put in the FBX. A new `WeaponBuilder` (Editor/Builders) makes
  each as a small prefab under `Generated/Weapons/<prefix>/`, from `BuildKit.Shape` primitives or from a Blender
  prop (the `blender-prop` skill) for curved blades, and `PlayerBuilder` parents them to four sockets it adds:
  `socket_hand_R` (under `hand.R`, placed and turned to match `katana_drawn`, so the weapon's long axis lies along
  `ForgeRig.RestBlade`), `socket_hand_L` (mirrored under `hand.L`), `socket_back` (under `chest`, 0.14 m behind it
  so it clears the straw hat) and `socket_hip` (under `hips`). `PlayerAnimator` shows the set named by `Carry`.
- **Materials.** Weapons are muted like the world: the player palette's `steel`, `iron`, `leather`, `wrap`, and a
  small tie of `accent` red. Every weapon has one named part, the **inlay** (`m_weapon_inlay`): dark iron until an
  element is held, then it takes the element's colour as an emissive (dim at rest, bright in the surge and for the
  weapon skill). This is how "elements are the only saturated colour" reaches the weapons, and it is the one thing
  to tint per element. Characters' ink outline applies. About the Katana's weight: 100 to 250 triangles each.
- **Left hand.** `ForgePose` aims only the right hand's weapon (`AimBlade`, `Blade`, `BladeRoll`). Add `AimBladeL`,
  `BladeL`, `BladeLRoll` for the left knife, the chain hand and the bow (which is held in the left). `GripGap`
  already places the left hand along the weapon; a negative `GripGap` puts it toward the head (the spear's slide).
- **Props that leave the hand.** A thrown spear, a chain's weight, a flying hammer, an arrow. Add one or two
  transforms under the model root, `prop_a` and `prop_b`, and on `ForgePose` the fields `PropA`, `PropAAim`,
  `PropAHeld` (bool: when true the prop sits in its hand socket and the other two are ignored), same for B.
  `ForgeClip.Bake` writes their curves like a bone's. In play a prop that has become a shot (E8) is hidden on the
  character and drawn by the shot.

### E3. Hit shapes and distance bands (Scythe, Spear, Hammer, Chain, Bow, Armament)

`PlayerCombat.Strike` knows one shape: a wedge of `Reach` and `ArcDegrees`. Enemy attacks already have lanes and
circles (`EnemyAttack.LineWidth`, `Radius`); give `AttackStep` the same names.

- `LineWidth` (metres, default 0): when above zero the blow is a lane `Reach` long and this wide, straight ahead;
  `ArcDegrees` is ignored.
- `Radius` (metres, default 0): when above zero the blow is a disc of this radius centred `Reach` metres ahead (a
  hammer head landing). An enemy standing closer than `Reach - Radius` is missed. `PlayerCombat.StrikeArea` already
  does the work.
- `InnerReach` and `InnerDamage`: a target nearer than `InnerReach` takes damage and stagger times `InnerDamage`
  (the haft of a pole weapon; 0.6 where used).
- `SweetFrom`, `SweetDamage`, `SweetStagger`: a target at or beyond `SweetFrom` takes those multiples (the scythe's
  tip). Distance is measured to the nearest point of the target's collider, as `Strike` does now.
- `SlashArcs.Show` needs a lane and a disc to draw, and for a sweet band a bright outer strip with a dim inner fan,
  so the player can see where the edge is.

### E4. One step, several blows (Knives, Chain, Hammer)

`Repeats` (float array, seconds after the first hit; default empty) and `RepeatScale` (default 1). `WeaponCombo`
raises `ComboEvents.Hit` again at each; `Strike` runs again with damage and stagger scaled, `struck` cleared, and
`ManaGain` paid again. `Active` must be long enough to hold them.

### E5. Two charges on the heavy button, walking while charging, and a counter (Sword, Hammer, Bow, Spear, Armament)

Today a held heavy becomes the skill at `ChargeTime` if there is mana. Add to `WeaponCombo`:

- `ChargedHeavy` (an `AttackStep`, default null) and `SkillChargeTime` (default equal to `ChargeTime`). With a
  `ChargedHeavy`: released before `ChargeTime` gives `Heavy`; from `ChargeTime` gives `ChargedHeavy` (no mana);
  held to `SkillChargeTime` with the mana and released gives `Skill`. `ComboEvents.Charged` fires at each tier; the
  first flash is white, the second the element's colour, and the "Ready" callout is spoken on the second only.
  Without the mana the hold stops at the first tier and waits. Without a `ChargedHeavy` nothing changes.
- `ChargeMoveFactor` (default 0): the share of walking speed kept while charging, and when above zero the stick
  turns the player during the charge (today `Aim()` runs only when an attack starts).
- `SkillIsCounter` (bool) and `CounterHoldMax` (seconds): for a counter skill. A blow that would land on the player
  while `IsCharged` is turned as a perfect guard is, `SkillManaCost` is spent, and `Skill` starts at once facing the
  attacker. Held past `CounterHoldMax` the charge releases itself as `ChargedHeavy`.

### E6. Poise in a swing (Sword, Hammer, Spear charge, Armament)

`Poise` on `AttackStep` (default 0): while that step is in `Windup` or `Active`, a blow whose damage is at or under
`Poise` still hurts but causes no flinch and does not interrupt (`PlayerDefence.Land` asks `PlayerCombat`). Red
attacks ignore poise. For scale: the acolyte's cuts are 10 and 12, the captain's jab 12, the soldier's thrust 20 and
sweep 24, Narukami's cuts 16.

### E7. Moving inside an attack (Scythe, Knives, Spear, Bow, Armament)

- `Lunge` may be negative (a step back during the wind-up). `PlayerCombat.Advance` multiplies it straight into
  `Step`, so it should already work; confirm with a test.
- `LungeSeconds` (default 0 meaning the whole `Windup`): the step-in is finished by this time, for long wind-ups.
- `Travel` (metres, default 0): distance covered during `Active`, with the blow struck every frame and each target
  hit once (`struck` is not cleared). `PassThrough` (bool): the player ignores enemy colliders while travelling,
  as Narukami's draw does (`EnemyAttack.PassThrough`).
- `Recoil` (metres): the player is pushed back this far over the first 0.12 s of `Recovery`.

### E8. Shots (Knives, Scythe, Bow, Spear, Hammer; the element trees want the same thing)

`Hazards.Lay` can already move a biting circle (`velocity`, `once`), which is how Burning Crescent flies. A shot is
that, plus four things it lacks: something to see riding it, stopping at the first thing hit, telling the weapon
what it hit, and coming back. Add `Shot` on `AttackStep` (a small class, null by default), fired at the step's hit
moment from 1 m ahead of the player at chest height:

`Speed`, `Range`, `Radius`, `Damage`, `Stagger`, `Count` (default 1), `SpreadDegrees`, `Pierce` (bool; false stops
at the first enemy), `Sticks` (bool; raises an event with the target, or the ground point at `Range`), `Returns`
(bool; flies back to the player at `ReturnSpeed`, striking again at `ReturnScale`), `HangSeconds` with `HangBite`
and `HangEvery` (stays at `Range` as a biting zone), `Heavy` (bool), `Prop` (which prefab is drawn riding it).
`TREE_BRIEF.md` lists "a projectile (straight, homing, or returning)" among the things the trees assume exist, so
this is shared work.

### E9. Dragging and shoving (Chain, Hammer, Spear, Armament)

`Enemy` has a private `shove` and a `Shove` distance. Add `Pull` on `AttackStep` (metres; positive toward the
player, negative away), carried on `Hit`, and applied through a new `IMovable.Drag(Vector3 metres, float seconds)`.
`EnemyKind` gets `Weight` (0 light, 1 heavy, 2 fixed): light is dragged the full distance, heavy half, fixed (gods,
the colossus's hands, an elite on its ground) not at all. `PullSelf` (bool on the step): against a fixed or heavy
target the player is drawn to it instead, stopping at `FinisherStandOff`.

### E10. More ways into an attack (Armament, Knives, Spear)

On `WeaponCombo`: `ChainHeavies` (a list indexed by how many lights have been thrown, used before `ChainHeavy`);
`DashAttack` (a step started by a light press in the last 40% of a roll or within 0.15 s of its end);
`GuardAttack` (a step started by a light press while the guard is held: the guard stays raised through it, no new
perfect window is earned, and it cannot chain).

### E11. Blows from behind (Knives)

`BackMultiplier` on `Weapon` (default 1): damage and stagger are multiplied when the player stands outside the
target's front 180 degrees. `Enemy.TakeHit` already works out whether a blow comes from behind for its guard; the
same test. Shown by a different cut mark (two short parallel strokes).

### E12. Staged finishers (all eight; each has a first pass that needs none of it)

The finisher today is one step: step in, one blow at the end of `Windup`, then the stop, the slowed time and the
camera's lean. To give a finisher a **moment**, add to the finisher step:

- `Beats`: a list of `(time, HitStopSeconds, Shake, VfxKind, TargetOffset, TargetTurn)`. A beat before the blow
  deals nothing; it only stops time for an instant, shakes, sparks, and may move the target.
- **The target is held**: from the finisher's start to its end the target's stagger cannot run out, it does not
  act, and between beats it is moved (eased) to each beat's `TargetOffset`, a place in the player's space. That is
  what lets an enemy be lifted, whirled, launched or thrown. Fixed targets (E9) ignore offsets: a god's finisher is
  the same body animation played against a target that stays put, and every finisher below is written to still
  read that way.
- `LungeSeconds` (E7) so the step-in is over early in a long wind-up.
- `HoldAt` and `HoldSeconds`: the camera's lean (`TopDownCamera.CloseIn`) and the slowed time may begin at a beat
  before the blow, for the held pose, and not only after it.
- Enemies need one clip each, `held`: limp, hanging from the chest, so a lifted body does not stand upright in the
  air.

The real damage still falls once, at the end of `Windup`, so `WeaponCombo` does not change. **First pass for every
finisher**: play the body animation only, with no target movement; it is still better than a slash through.

### E13. Guard reactions, and a guard that is slow to rise (all; Hammer needs the second part)

- There is one guard clip per weapon today and nothing plays when a blow is blocked or turned. Add two one-shot
  upper-body states per weapon, fired from `PlayerDefence` on `GuardResult.Blocked` and `GuardResult.Perfect`:
  `guard_hit_<p>` (0.22 s) and `guard_perfect_<p>` (0.30 s). Retrofit the Katana and Fists with the same.
- `Guard.PerfectFrom` (seconds, default 0): `IsPerfect` becomes `sincePress >= PerfectFrom && sincePress <=
  PerfectWindow`.
- `PerfectGuardRider` on `Weapon`: one optional extra thing a perfect guard does with this weapon (a pull, a
  planted arrow). Each is named under its weapon.

### E14. Things only one weapon needs

- **Chain Sickle**: a drawn chain (`ChainLine`) and the **Bound** state on an enemy.
- **Great Bow**: planted arrows on enemies and in the ground (`PlantedArrows`).
- **Full Body Armament**: the element coat (`ArmamentBuilder`) and the rule that swaps Fists for it.

Each is specified under its weapon.

---

## 2. Massive Sword

**Idea.** A slab of iron as long as its bearer is tall: every swing clears half the screen, and each swing of the
chain borrows the last one's weight, so the chain speeds up as it goes. It asks for **commitment**: you choose your
moment, you cannot take it back, and in exchange the swing has poise, so you can decide to take a small blow to
land a great one. No other weapon lets you trade on purpose.

**How it changes play.** Position to catch two or three enemies in one arc. Start the chain early, as the enemy's
own attack is ending, not after. Use the guard as a wall (it is cheap) and the charged cut as the punish.

### Numbers

`Name = "Massive Sword"`, `Carry = "gsword"`, `SkillName = "Elemental Spike Line"`, `ChargeAnimation =
"charge_gsword"`, `SlashColour = (1.9, 1.8, 1.6, 0.9)`. `SkillManaCost = 35`, `ChargeTime = 0.45`,
`SkillChargeTime = 0.95`, `ChargeMoveFactor = 0.25`.

| slot | Name | Sweep | Windup | Active | Recovery | Damage | Stagger | StaminaCost | ManaGain | Lunge | Reach | ArcDegrees | HitStop | Shake | new |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Steps[0] | attack_gsword_1 | -1 | 0.34 | 0.14 | 0.52 | 24 | 26 | 20 | 9 | 0.8 | 3.4 | 190 | 0.08 | 0.10 | Poise 12 |
| Steps[1] | attack_gsword_2 | +1 | 0.26 | 0.14 | 0.50 | 26 | 28 | 20 | 9 | 0.8 | 3.4 | 190 | 0.08 | 0.10 | Poise 12 |
| Steps[2] | attack_gsword_3 | -1 | 0.20 | 0.18 | 0.62 | 34 | 40 | 24 | 12 | 0.6 | 3.6 | 360 | 0.10 | 0.16 | Poise 12 |
| Heavy | attack_gsword_heavy | 0 | 0.50 | 0.12 | 0.66 | 44 | 56 | 28 | 14 | 1.0 | 3.6 | 50 | 0.13 | 0.28 | Poise 22 |
| ChainHeavy | attack_gsword_chain | +1 | 0.28 | 0.12 | 0.60 | 46 | 60 | 26 | 14 | 1.2 | 3.6 | 120 | 0.13 | 0.30 | Poise 22 |
| ChargedHeavy | attack_gsword_charged | 0 | 0.12 | 0.14 | 0.74 | 62 | 80 | 34 | 16 | 1.4 | 3.8 | 60 | 0.16 | 0.42 | Poise 30 |
| Skill | attack_gsword_skill | 0 | 0.22 | 0.10 | 0.64 | 20 | 30 | 20 | 0 | 0.4 | 2.2 | 120 | 0.12 | 0.30 | see below |
| Finisher | attack_gsword_finisher | 0 | 0.62 | 0.14 | 0.74 | 44 | 0 | 0 | 15 | 0 | 3.6 | 120 | 0.24 | 0.60 | Beats |

The chain: 84 damage and 94 stagger in 2.90 s for 64 stamina. Per second it is the Katana's damage, but on
everything in a 190 degree arc, and it empties two thirds of the stamina bar: one chain, then you must guard or
walk.

**Elemental Spike Line.** The sword is turned point down and driven into the ground (the plunge is the step in the
table and hits what stands on it). From the blade a line of eight spikes breaks out of the ground, travelling 10 m
at 14 m/s in a lane 1.6 m wide; each enemy is struck once for 34 damage and 45 stagger as a heavy blow, and the
spikes stand for 1.0 s (seen only; they do not block). In the held element's colour and carrying its affliction;
with no element they are plain stone. **Engine**: no shot is needed. It is eight `Hazards.Lay` calls along the
lane, 1.25 m apart, each delayed 0.09 s after the last, `once` true, sharing one list of who has been bitten
(the call takes a per-zone list today; give it an optional shared one), plus a new `HazardKind.Spike` look.

**Engine work**: E1, E2, E5 (two charge tiers), E6 (poise), E12, E13, and the shared bitten list above.

### Guard and dodge

- Guard (`GuardProfile`): `PerfectWindow 0.15`, `StaminaPerDamage 0.6`, `ChipFraction 0.25`, `ArcDegrees 240`,
  `GuardMoveFactor 0.3`. The blade is planted like a door: the cheapest, widest guard in the game and the
  tightest perfect guard. A wall you stand behind, not a parry you fish for.
- Dodge (`DodgeProfile`): `Distance 3.9`, `Duration 0.40`, `StaminaCost 30`, `InvulnerableFrom 0.05`,
  `InvulnerableUntil 0.25`, `Recovery 0.16`. A heavy shoulder dive, not a tumble. `MoveFactor 0.92`.

### Model

Total length 1.95 m. **Blade** 1.45 x 0.22 x 0.035 m, single-edged, straight back, the point clipped at 45
degrees so that from above it is a long bar with a slanted end, not a needle: `iron` for the body, a 0.04 m bevel
of `steel` along the edge and round the clipped point. A fuller 1.0 x 0.04 m down the flat on both sides is the
**inlay**. **Guard**: a blunt block 0.30 x 0.06 x 0.07, iron. **Grip** 0.42 x 0.045, `wrap` cloth in six bands,
with two `accent` ribbons 0.25 m long tied under the guard (two-bone tails for `ForgeTails`, optional). **Pommel**:
an iron ring 0.08 across. About 180 triangles. **Hands**: right directly under the guard, left 0.26 m below
(`GripGap 0.26`). **Carry**: always in hand (`Stowed` false). Idle and run carry it on the right shoulder, flat
down, edge out, point behind and above. `socket_back` holds it slanted from the right shoulder to behind the left
heel, point 0.15 m off the ground, for rest fires and the hub stand.

### Animation

New for this weapon: every clip. The Katana's set is the wrong weight everywhere: its cuts come from the wrists
and this one comes from the legs and back. What can be borrowed is structure only: `Overhead()` is the skeleton for
the heavy, and `SwordReady()`'s feet for the stance.

| clip | seconds | what it is |
|---|---|---|
| idle_gsword | 3.0 loop | Feet wide (0.5 m), left foot ahead, hips sunk 0.12. Sword on the right shoulder, right hand at the collarbone, left hand hanging on the pommel. Breathing lifts the blade's point 4 cm; every loop the weight shifts once from leg to leg. From above: a long bar lying across the right shoulder pointing back-right. |
| run_gsword | 0.62 loop | Slower cadence than `run`. Body leaned 20 degrees, sword still on the shoulder, both hands on the grip in front of the chest; the point bounces behind with each stride. The shoulders barely counter-turn (the load stops them): the bar stays steady while the legs work. |
| roll_gsword | 0.40 | Not a somersault. A low dive onto the left shoulder with the sword hugged along the right forearm, point trailing; one turn about the body's long axis, up onto one knee, sword already back on the shoulder. |
| guard_gsword | 0.16 | The blade is swung down point first into the ground in front, flat toward the enemy, both hands on the grip at chest height, left shoulder pressed to the flat, head just over the guard block. Hips sunk 0.3, back foot far back. From above: a bar across the front of the body, like a closed gate. |
| guard_hit_gsword | 0.22 | The whole wall is driven back 6 cm and returns; the head ducks behind the guard block; the back heel skids. |
| guard_perfect_gsword | 0.30 | The flat is punched forward a hand's breadth into the blow with the shoulder (a barge), then the point is torn out of the ground and the sword comes up to the shoulder: ready to swing. |
| charge_gsword | to ChargeTime, then held | Sword lifted from the shoulder to vertical over the head, both arms straight, point at the sky; hips sink, front knee bends. At the first tier the point tips back behind the head (the blade horizontal behind the shoulders, chest thrown open). Held: a slow tremble in the arms (a `Shape`). |

**attack_gsword_1 (a sweep, left to right).** *Anticipation*: from the shoulder carry the hips turn left 40
degrees and sink; the sword drops off the shoulder behind the left hip, point low and back, the chest wound left
60 degrees over a planted right foot; held for the last seventh of the wind-up. *Contact*: the hips fire first,
then the chest, and the arms are dragged round after, almost straight; the blade is level at hip height, edge
leading, as it crosses the front. The left foot steps through (the lunge). *Follow-through*: the blade carries on
round to point back-right, pulling the chest 70 degrees right and the left heel off the ground; the body leans
away from the blade to balance it. It does **not** return to a guard: it stays wound right, which is the wind-up
of the next. *Weight*: back leg to front leg. *From above*: the tip draws a 190 degree fan, left to right, at
3.4 m; the widest plain arc of any light attack.

**attack_gsword_2 (the return, right to left).** *Anticipation*: already wound right; only a further sink and a
roll of the wrists to turn the edge (hence the shorter wind-up). *Contact*: the same swing mirrored, at chest
height, the right foot stepping through. *Follow-through*: the blade carries round behind the left shoulder and
keeps going: the body lets it pull it into a turn. *From above*: the same fan drawn back the other way; with the
first, a figure of eight laid on its side.

**attack_gsword_3 (the full turn).** *Anticipation*: there is hardly any: the body is already turning with the
blade from the second cut. One quick drop of the hips with the back to the enemy and the head snapped round over
the left shoulder to keep the eyes on it. *Contact*: a full turn on the left foot, the sword at arm's length, level
at waist height, the right leg swung out as a counterweight; the blow counts for the whole circle. *Follow-through*:
the blade's weight pulls the bearer round another quarter turn and down; the point bites the ground at the right
side and drags a short scar; both knees bent, breathing; the long recovery is the price. *From above*: the tip
draws a complete ring round the player, 7.2 m across. Its `After` turns the model root through 360 degrees
about y, as `Roll()` turns it about x.

**attack_gsword_heavy (the overhead).** Built on `Overhead()`. *Anticipation*: the sword goes up off the shoulder
and back until it hangs down the spine, elbows by the ears, the chest arched back 18 degrees, weight on the back
foot, front foot light; a long held beat (this is the half second the enemy can read). *Contact*: a stride
forward and the whole back folds: the blade comes over the top and down the centre line, landing flat-on along
the ground straight ahead. *Follow-through*: the blade buries itself; the hips drop to 0.45 below rest, the head
goes down past the hands; then it has to be heaved out, a visible wrench back up to the shoulder. *From above*:
one straight line 3.6 m long struck down the facing, with a dust burst along it.

**attack_gsword_chain (the rising cut).** *Anticipation*: from wherever the chain left the blade, the point is
let fall to the ground behind the right heel. *Contact*: the sword is dragged forward along the ground, throwing
sparks (a `Vfx` line), and ripped upward right to left across the target from hip to shoulder as the legs
straighten: the bearer leaves the ground a hand's breadth. *Follow-through*: the blade finishes high over the left
shoulder, body stretched tall, then settles to the shoulder carry. *From above*: a spark line on the ground toward
the enemy, then a 120 degree arc.

**attack_gsword_charged.** Released from `charge_gsword`: the wind-up is already done, so the blow comes in
0.12 s. The overhead again, but with a jump: both feet leave the ground, knees tucked, and the bearer lands as the
blade does. The ground mark is twice as wide; three cracks spread from the point (seen only).

**attack_gsword_skill (Elemental Spike Line).** *Anticipation*: the sword is spun once in the right hand at the
side so that it hangs point down (reverse grip), then raised with both hands on the grip above the head, point at
the ground; the inlay lights from guard to point. *Contact*: driven straight down between the feet to a third of
its length; the body kneels behind it. *Follow-through*: held kneeling, head down, as the spikes run out ahead;
the sword is drawn out only in the last third of the recovery. *From above*: a point, then a line of spikes
running away from it up the screen.

**attack_gsword_finisher ("The Headsman").** The moment is a **held blade and a late crack**.

1. 0.00 to 0.14: step in; the pommel is driven into the kneeling enemy's chest, which rocks it back upright on its
   knees (beat: hit stop 0.04, small shake).
2. 0.14 to 0.44: the bearer walks one slow half circle to the enemy's side with the point trailing on the ground;
   it scores a ring of sparks round them both (from above: a bright half circle being drawn round the two
   figures). The blade then rises to vertical over the head. Both arms straight.
3. 0.44 to 0.62: **the hold**. Nothing moves but the scarf. The inlay fills with light from the guard to the
   point. The camera leans in (`HoldAt 0.44`, `HoldSeconds 0.18`).
4. 0.62: the cut, straight down. The real blow. Time stops for 0.24 s, then runs slow.
5. In the slow time a thin line of light runs out along the ground 6 m past the enemy, and 0.35 s after the blow
   the ground along that line **splits** upward in the element's colour (seen only). The bearer is already
   standing, back to it, lifting the sword to the shoulder.

*Shared or new*: all new. *First pass without E12*: steps 2 to 5 play as body animation with the enemy kneeling
where it is; nothing is lost but the pommel shove.

### Elements

- **Fire**: the sweeps drag a flame along the tip's path, and the ground under the arc of the third cut burns for
  2 s, so the full turn leaves a ring of fire the bearer stands inside. Spike Line is a row of flame pillars that
  leave their lane burning. Upgrade, **Brand** (Fire, needs the Massive Sword): while the heavy is charging the
  blade heats; a fully charged cut sets its whole line alight and detonates every burn it touches.
- **Ice**: the slow weapon and the element that holds things still were made for each other. Every blow lays two
  stacks of Chill instead of one, and the charged cut **shatters** a Frozen enemy for double. Spike Line raises a
  wall of ice that stands for 4 s and Chills what touches it. Upgrade, **Glacier Edge** (Ice, needs the Massive
  Sword): a perfect guard with the planted blade freezes the ground in front of it; what stands there takes three
  stacks of Chill.
- **Wind**: the element that lightens it. Wind makes the three sweeps one continuous spin: no recovery between
  them while stamina lasts, and each pushes enemies outward to the tip (Unsteady). Spike Line becomes a travelling
  whirl that carries small enemies along with it. Upgrade, **Cyclone** (Wind, needs the Massive Sword): holding the
  light button after the third cut keeps the spin going, a turn every 0.5 s for 10 stamina a turn, moving at half
  speed.

---

## 3. Twin Knives

**Idea.** Two short blades, the quickest hands in the game and the least reach: each cut is nothing, five of them
in a second and a half from behind is a great deal. It asks you to **be behind the enemy**: blows from the back
count for half as much again, the chain's last cut and its heavy both carry you through to the far side, and the
dodge is a cheap sidestep made to be spent on position.

**How it changes play.** You do not stand and trade (the guard is poor and the stagger is low). You slip a blow,
come out behind it, and unload. Mana comes fastest of any weapon (five small gains a chain), so this is the
spell-user's weapon: GAME.md's "suits wind, lightning, poison".

### Numbers

`Name = "Twin Knives"`, `Carry = "knife"`, `SkillName = "Fan of Knives"`, `ChargeAnimation = "charge_knife"`,
`SlashColour = (1.5, 1.9, 1.7, 0.85)`, `BackMultiplier = 1.5` (E11). `SkillManaCost = 25`, `ChargeTime = 0.30`.
No `ChargedHeavy`.

| slot | Name | Sweep | Windup | Active | Recovery | Damage | Stagger | StaminaCost | ManaGain | Lunge | Reach | ArcDegrees | HitStop | Shake | new |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Steps[0] | attack_knife_1 | -1 | 0.06 | 0.05 | 0.13 | 5 | 4 | 5 | 3 | 0.5 | 1.7 | 100 | 0.025 | 0 | |
| Steps[1] | attack_knife_2 | +1 | 0.06 | 0.05 | 0.13 | 5 | 4 | 5 | 3 | 0.5 | 1.7 | 100 | 0.025 | 0 | |
| Steps[2] | attack_knife_3 | 0 | 0.08 | 0.10 | 0.16 | 5 | 5 | 7 | 3 | 0.6 | 1.8 | 120 | 0.03 | 0 | Repeats {0.06} |
| Steps[3] | attack_knife_4 | -1 | 0.07 | 0.05 | 0.14 | 7 | 5 | 6 | 4 | 0.5 | 1.8 | 100 | 0.03 | 0 | |
| Steps[4] | attack_knife_5 | +1 | 0.12 | 0.08 | 0.30 | 12 | 12 | 9 | 6 | 0.4 | 2.0 | 200 | 0.06 | 0.06 | Travel 2.4, PassThrough |
| Heavy | attack_knife_heavy | 0 | 0.18 | 0.08 | 0.34 | 16 | 18 | 14 | 8 | 2.2 | 2.0 | 60 | 0.07 | 0.08 | |
| ChainHeavy | attack_knife_chain | -1 | 0.10 | 0.10 | 0.30 | 18 | 20 | 14 | 9 | 0.4 | 1.8 | 140 | 0.07 | 0.10 | Travel 3.0, PassThrough |
| DashAttack | attack_knife_dash | +1 | 0.04 | 0.08 | 0.20 | 9 | 8 | 6 | 5 | 1.0 | 2.0 | 160 | 0.04 | 0 | |
| Skill | attack_knife_skill | 0 | 0.10 | 0.06 | 0.36 | 0 | 0 | 12 | 0 | -2.6 | 0 | 0 | 0.05 | 0.10 | Shot |
| Finisher | attack_knife_finisher | 0 | 0.70 | 0.10 | 0.50 | 22 | 0 | 0 | 15 | 0 | 3.0 | 360 | 0.20 | 0.45 | Beats |

The chain: 39 damage, 35 stagger, 22 mana in 1.58 s for 32 stamina (six blows). From in front that is the Katana's
damage with less stagger and no reach. From behind it is 58 and 52. The fifth cut and the chain heavy both end
on the far side of the target, so a chain started in front ends behind.

**Fan of Knives.** A spring straight back (2.6 m, during the wind-up) and at its top five knives are thrown in a
50 degree fan. `Shot`: `Count 5`, `SpreadDegrees 50`, `Speed 22`, `Range 8`, `Radius 0.35`, `Damage 9`,
`Stagger 8`, `Pierce` false, no `Sticks`. Thrown from one step away all five hit one enemy (45 damage, 40
stagger); from range it thins out. It is the knives' way **out**, and it earns nothing (`ManaGain 0`).
The knives thrown are of light in the held element's colour (steel-white with none); the two real ones never
leave the hands.

**Engine work**: E1, E2 (left-hand blade), E4 (the double), E7 (`Travel`, `PassThrough`, negative `Lunge`), E8
(the fan), E10 (`DashAttack`), E11 (the back), E12, E13.

### Guard and dodge

- Guard: `PerfectWindow 0.21`, `StaminaPerDamage 1.25`, `ChipFraction 0.45`, `ArcDegrees 160`,
  `GuardMoveFactor 0.7`. Crossed knives stop little, and you can be walked round. But the perfect window is the
  widest in the game and you keep most of your speed: this guard is a parry and nothing else.
- Dodge: `Distance 3.4`, `Duration 0.26`, `StaminaCost 16`, `InvulnerableFrom 0.02`, `InvulnerableUntil 0.19`,
  `Recovery 0.06`. A low spinning sidestep (`roll_knife`), shorter and much cheaper: three for the price of two.
  A light press near its end gives the `DashAttack`. `MoveFactor 1.08`.

### Model

Two knives, each 0.42 m. **Blade** 0.27 x 0.045 x 0.012, single-edged with a slight forward belly and a clipped
point; `steel`, with the back in `iron`. The cutting edge, a strip 0.006 wide, is the **inlay**. A disc guard 0.05
across, a grip 0.11 in `leather`, and a ring pommel 0.05 in iron (it is what the finger spins the knife on). About
60 triangles each. **Hands**: the right knife is held point up (forward grip); the left is held point down
(reverse grip), so the two hands never make the same shape. **Carry** (`Stowed` false in play): at rest fires
and on the hub stand they sit crossed in two short sheaths on `socket_hip`, at the small of the back under the
rope belt, hilts out to either side.

### Animation

Borrowed as a first pass: the feet and hips of the fists' `Jab` and `Cross` for cuts 1 and 2 (the same rhythm,
within 0.01 s), with the hands re-aimed. Everything else new: the fists turn the shoulders like a door, the knives
turn the whole body like a top.

| clip | seconds | what it is |
|---|---|---|
| idle_knife | 1.1 loop | Low and narrow, weight on the toes, side on with the left shoulder leading. Right knife at the hip pointing at the enemy, left knife reversed along the forearm, raised in front of the chest. Never still: a small bounce, and once a loop the right knife is spun a full turn on its ring. |
| run_knife | 0.46 loop | The fastest run: bent far forward, both arms trailing straight back and low with the blades pointing behind (a ninja's run). From above: an arrowhead, two bright lines behind the body. |
| roll_knife | 0.26 | A stepping pirouette, not a roll: one full turn about y on the lead foot, body low, arms folded in then flung out at the end. The root is turned by `After`. |
| guard_knife | 0.10 | Both knives crossed in front of the face, the left reversed and the right forward, elbows down, chin tucked, hips barely sunk. |
| guard_hit_knife | 0.22 | The cross is driven back to the brow; both feet slide back together, a foot's length. |
| guard_perfect_knife | 0.30 | The cross opens like scissors, throwing the blow out to the left, and the body slips right past it in the same motion, finishing side-on beside the attacker with the right knife already drawn back. |
| charge_knife | to ChargeTime, then held | A crouch with both knives brought together at the left hip, weight sunk on the back leg like a set spring. |

**attack_knife_1.** *Anticipation*: the right hip draws back two frames. *Contact*: a flat backhand flick of the
right knife, left to right, at throat height, the hips turning with it. *Follow-through*: the hand carries only to
the right shoulder line; the left knife is already coming. *From above*: a short bright tick, 100 degrees.

**attack_knife_2.** The left, reversed, cutting right to left as the left hip comes through; the right hand
recoils to the ribs. A mirror tick. Together they are one breath.

**attack_knife_3 (the cross, two blows).** *Anticipation*: both hands thrown wide, chest open, a quick hop.
*Contact*: both knives cut inward at once and cross in front of the chest (first blow), then cut outward again
(second, 0.06 s later) as the arms are flung open. *Follow-through*: arms wide, chest forward. *From above*: an X
that closes and opens: the one shape in this chain that is seen from across the screen.

**attack_knife_4.** *Anticipation*: the body drops under its own arms. *Contact*: the right knife, turned to a
reverse grip in the drop, rips upward from the left hip to over the right shoulder as the legs drive up.
*Follow-through*: the hand finishes high and the body is already leaning into the turn of the fifth.

**attack_knife_5 (the passing spin).** *Anticipation*: the high right hand pulls the body into a turn, and the
left foot crosses over. *Contact*: a full turn with both knives out at arm's length, one high and one low, while
the body travels 2.4 m **through** the target and out the far side. *Follow-through*: a skid on the left foot,
turned to face the enemy's back, low, both knives trailing. *From above*: a tight double ring that slides through
the enemy. This is the move that puts the player behind.

**attack_knife_heavy (the lunge).** *Anticipation*: both knives drawn back to the right hip, body coiled low on
the back leg, held a beat. *Contact*: a flat leap of 2.2 m, body near horizontal, both points driven in together
at the end. *Follow-through*: the body's weight carries into the target; the knives are torn out sideways and the
feet land wide. *From above*: a dart.

**attack_knife_chain (the crossing step).** *Anticipation*: a drop of the lead shoulder. *Contact*: a low dash
past the target's left side with the left knife dragged across its middle, travelling 3.0 m. *Follow-through*: a
half turn on arrival, so the player ends behind the target and facing its back, right knife up. *From above*: a
straight line through the enemy with one cut mark left on it.

**attack_knife_dash.** Out of the sidestep's turn: the turn is simply allowed to finish, with the right arm
flung out; one flat cut, 160 degrees.

**attack_knife_skill (Fan of Knives).** *Anticipation*: both arms cross over the chest as the legs load.
*Contact*: a back handspring; at the top of it, body inverted and facing the enemy, both arms whip open and the
fan leaves the hands. *Follow-through*: lands in a low three-point crouch, one hand on the ground, 2.6 m back.
*From above*: the body flips away down the screen while five lines spread up it.

**attack_knife_finisher ("Three Crossings").** The moment is a **delayed payoff**.

1. Three passes, at 0.10, 0.28 and 0.46 s. Each is the crossing step: a dash through the kneeling enemy to a
   point 2.2 m beyond, each leaving one white line through the enemy. The three end points are the corners of a
   triangle with the enemy at its middle. Each pass is a beat (hit stop 0.03, no damage). From above the player
   draws a three-pointed star through the enemy in under half a second.
2. 0.46 to 0.70: the player arrives at the third corner with their **back to the enemy**, stands up slowly out of
   the crouch, and spins both knives once on their rings. The camera leans in on the player, not the enemy. The
   three lines still hang in the air.
3. 0.70: the knives stop dead, points down. The real blow: all three lines burst at once on the enemy behind.
   Stop, slow time.

*Shared or new*: all new; the pass reuses `attack_knife_chain`'s poses. *Different from the Katana with Delayed
Cuts*: that is one line and a sheath; this is a drawn figure and a turned back. *First pass without E12*: the
passes move only the player (`Travel` beats); the enemy never needed to move.

### Elements

- **Wind**: the dodge becomes a gust that carries 1.5 m further and leaves the next blow within 1 s counted as
  from behind wherever it lands. Fan of Knives is thrown as a full ring when cast in the middle of a crowd.
  Upgrade, **Slipstream** (Wind, needs the Twin Knives): each crossing (the fifth cut, the chain heavy, the dash
  attack) gives back 8 stamina, so a player who keeps passing through never has to stop.
- **Poison**: the natural pairing: many small blows, each a stack. Poisoned lands its bursts every 2 s, and with
  the knives a chain is five or six stacks. Fan of Knives lays two stacks a knife. Upgrade, **Open Vein** (Poison,
  needs the Twin Knives): a blow from behind on a Poisoned enemy sets off its next burst at once.
- **Metal**: the opposite temperament, and it mends the knives' weakness. Metal's Sundered makes weapon blows hit
  harder, and each of five blows profits; the surge hardens the guard to the Katana's numbers for its length.
  Upgrade, **Needlework** (Metal, needs the Twin Knives): the third cut's two blows ignore an enemy's guard and
  leave it Sundered.

---

## 4. Chain Sickle

**Choice: the chain-sickle, of the chain family, for now.** A flail is a hammer on a rope and would crowd the
Great Hammer; nunchaku are fast fists and would crowd the Knives and the Armament. The chain-sickle is the only
one of the three that is **two weapons at two distances in one**: a blade in the right hand for close work and a
weight on 3.6 m of chain in the left for far work. Its skill in GAME.md's list is Entangle, which is exactly what
a thrown chain does. And from a high camera a chain is a line drawn on the ground, the most readable shape there
is. The flail and the nunchaku can come later as other weapons of this family built on the same chain drawing.

**Idea.** A sickle for the enemy in front of you and a chain for the one four metres away: the light chain
alternates between them, so the weapon is always working at two ranges. It asks you to **manage distance in both
directions at once**, and it is the only weapon that moves the enemy: it drags the light ones to you and drags
you to the heavy ones.

**How it changes play.** The caster who backs away is pulled in. The soldier behind his guard is bound and walked
round. The fight's shape is yours to arrange. The price: the far blows are slow to come back, and up close with the
chain out you have only the sickle.

### Numbers

`Name = "Chain Sickle"`, `Carry = "chain"`, `SkillName = "Entangle"`, `ChargeAnimation = "charge_chain"`,
`SlashColour = (1.6, 1.7, 1.9, 0.85)`. `SkillManaCost = 30`, `ChargeTime = 0.40`. No `ChargedHeavy`.

| slot | Name | Sweep | Windup | Active | Recovery | Damage | Stagger | StaminaCost | ManaGain | Lunge | Reach | ArcDegrees | HitStop | Shake | new |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Steps[0] | attack_chain_1 | +1 | 0.10 | 0.07 | 0.22 | 9 | 8 | 9 | 5 | 0.6 | 2.2 | 130 | 0.04 | 0 | |
| Steps[1] | attack_chain_2 | 0 | 0.18 | 0.08 | 0.34 | 11 | 20 | 11 | 6 | 0.2 | 5.2 | - | 0.06 | 0.06 | LineWidth 0.9 |
| Steps[2] | attack_chain_3 | -1 | 0.10 | 0.07 | 0.24 | 10 | 8 | 9 | 5 | 0.7 | 2.3 | 130 | 0.04 | 0 | Pull 0.6 |
| Steps[3] | attack_chain_4 | +1 | 0.24 | 0.20 | 0.42 | 8 | 12 | 14 | 4 | 0 | 4.0 | 360 | 0.05 | 0.08 | Repeats {0.10} |
| Heavy | attack_chain_heavy | +1 | 0.30 | 0.14 | 0.50 | 22 | 26 | 20 | 10 | 0.4 | 4.6 | 220 | 0.09 | 0.14 | |
| ChainHeavy | attack_chain_chain | 0 | 0.16 | 0.08 | 0.44 | 14 | 30 | 16 | 9 | 0 | 5.5 | - | 0.08 | 0.12 | LineWidth 1.0, Pull 3.0, PullSelf |
| Skill | attack_chain_skill | 0 | 0.16 | 0.10 | 0.40 | 12 | 35 | 14 | 0 | 0 | 7.0 | - | 0.10 | 0.20 | LineWidth 1.2, Bound |
| Finisher | attack_chain_finisher | 0 | 0.66 | 0.12 | 0.60 | 28 | 0 | 0 | 15 | 0 | 3.2 | 120 | 0.22 | 0.50 | Beats |

The chain: 46 damage and 60 stagger in 2.26 s for 43 stamina (five blows: the whirl strikes twice). Less damage a
second than the Katana, more stagger, and half of it lands beyond any other light weapon's reach.

**Entangle.** The weight is thrown down a lane 7 m long. The first enemy it meets is **Bound** for 2.5 s: it
cannot walk or turn, an attack it was showing is broken (a red one is not), and it takes a quarter more stagger.
A drawn chain joins it to the player's left hand for as long as it lasts, and the player may not go further than
7 m from it. While it is Bound the heavy button gives the `ChainHeavy` whatever the chain's state: a light enemy
is hauled 3 m to the player's feet; with a heavy or fixed one the player flies to it along the chain and arrives
with the sickle. A god is Bound for 1.0 s and only its turning is stopped. The bind ends early if the player
rolls. **Engine (E14)**: `Bound` on `Enemy` (a timed state in `EnemyBrain` beside `Staggered`, with a per-kind
length multiplier).

**The chain itself (E14, `ChainLine`).** It cannot be a skinned mesh: it changes length. It is a ribbon of 24
points drawn every frame between two transforms, the left hand's socket and `prop_a` (the weight, or the
sickle when the sickle is thrown), with a sag that goes to zero when taut and a link pattern on it (a 16 px
texture of alternating links, drawn by code like the others), in `iron` with the ink outline's colour at its
edges. The animation keys only where the weight is (`PropA`); the ribbon follows. While an enemy is Bound the far
end is the enemy's chest. At rest the ribbon is hidden and a coiled mesh is shown at the hip.

**Engine work**: E1, E2 (props, left hand), E3 (`LineWidth`), E4, E9 (`Pull`, `PullSelf`, weights), E12, E13,
and E14 (`ChainLine`, `Bound`).

### Guard and dodge

- Guard: `PerfectWindow 0.18`, `StaminaPerDamage 1.1`, `ChipFraction 0.4`, `ArcDegrees 200`. The chain pulled
  taut between the two hands: poor against weight. `PerfectGuardRider`: the chain is whipped round the attacking
  limb and the attacker is yanked 1.2 m toward the player and takes 15 more stagger than a perfect guard already
  gives. It arrives inside the sickle's reach, off balance.
- Dodge: the defaults.

### Model

**Sickle**: a haft 0.45 x 0.035 m in `leather`-dark wood with two iron bands; a crescent blade of 0.30 m chord
and 0.06 m width set square to the haft at its top, `steel` with an `iron` spine; its inner edge is the **inlay**.
An iron ring at the butt takes the chain. About 70 triangles. **Weight**: an iron teardrop 0.11 x 0.09 m with a
flat band round its belly that is also **inlay**; 24 triangles. **Chain**: 3.6 m, drawn (above); at rest a
coil 0.22 m across, a torus of 12 x 4 with the link texture, hung on the left of `socket_hip`. **Hands**: right
on the haft just under the blade, blade forward and edge down; left holding the chain 0.5 m from the weight.
**Carry** (`Stowed` false): at rest the sickle is thrust through the back of the rope belt, blade to the right.

### Animation

All new. Nothing in the Katana or fist sets has a thing on a rope in it. The body work of sickle cuts 1 and 3
can be started from `DrawCut` and `ReturnCut` (one-handed cuts across the body) with the left arm re-posed.

| clip | seconds | what it is |
|---|---|---|
| idle_chain | 1.6 loop | Side on, right foot forward, sickle held low and ahead with the blade's point at the enemy. The left hand, back by the hip, keeps the weight turning in a slow upright circle 0.5 m across beside the left leg, one turn a loop. From above: a bright dot going round at the player's left side, always. It is what tells an onlooker which weapon this is. |
| run_chain | 0.54 loop | Sickle carried reversed along the right forearm; the left hand holds the gathered chain at the chest and the weight trails 1 m behind on a short length, swinging with the stride. |
| guard_chain | 0.12 | Both hands thrown up and apart, the chain snapped taut between them slantwise across the face and chest, sickle blade hooked over the top of it. |
| guard_hit_chain | 0.22 | The taut line bows in toward the chest and springs back; the hands are driven a hand's breadth together. |
| guard_perfect_chain | 0.30 | The left hand throws a loop over the incoming blow and both hands haul back and down past the left hip: the body turns side on with the pull. The weight end whips once round `prop_a`'s target point. |
| charge_chain | to ChargeTime, then held | The weight is let out to 1.5 m and swung overhead in a flat circle, faster as the charge fills (one turn, then two a second). Body upright under it, sickle held back. From above: a ring spinning over the player. |

**attack_chain_1 (sickle).** *Anticipation*: the sickle is drawn back over the right shoulder, the left hand's
weight keeps turning. *Contact*: a short hooking chop, right to left and downward, the right hip driving in.
*Follow-through*: the blade finishes by the left hip. *From above*: a small arc; and at the left side the turning
dot does not stop.

**attack_chain_2 (the weight snap).** *Anticipation*: the turning weight is let go on the upswing behind, the
left arm fully back, the chest opened left; the sickle hand crosses to the left hip to pay out chain. *Contact*:
the left arm whips forward underhand and the weight flies flat down the lane to 5.2 m, the chain running out
straight behind it. *Follow-through*: at full stretch the left hand snaps back, the chain cracks into a wave, and
the weight is flicked back to the hand over 0.34 s. *From above*: a line shot out up the screen and whipped back.

**attack_chain_3 (the hook).** *Anticipation*: the returning weight is caught; the sickle goes out wide to the
left, blade turned inward. *Contact*: the sickle is hooked behind the target and drawn back left to right toward
the player's own hip: a pull, not a push; the target comes 0.6 m nearer. *Follow-through*: the right elbow ends
behind the ribs, the body leaning back.

**attack_chain_4 (the whirl, two blows).** *Anticipation*: the left hand lets the chain run to 4 m and swings it
up overhead; the knees bend. *Contact*: two flat turns of the weight at full length round the player, the body
turning under it, the sickle arm held in to the chest. *Follow-through*: the weight is brought down and round the
body, wrapping once round the waist to kill its speed, then unwinds into the hand. *From above*: two full rings
at 4 m, the widest thing a light chain draws in this game.

**attack_chain_heavy (the thrown sickle).** *Anticipation*: the sickle is spun once on a metre of chain at the
right side, low. *Contact*: it is cast out right-hand and swept flat across the front, right to left, at 4.6 m:
the blade itself on the end of the chain. *Follow-through*: the left hand hauls and the sickle comes back spinning,
caught by the haft overhand. *From above*: a 220 degree sickle-moon of steel swept across the top of the screen.
`PropA` carries the sickle here and the weight stays in the left fist.

**attack_chain_chain (the yank).** *Anticipation*: a short underhand toss. *Contact*: the weight flies the lane
and wraps. *Follow-through*: both hands on the chain, the whole body thrown backward onto the heels: the target
comes. Against something that will not come, the same throw, and then the body is pulled off its feet and flies
forward along the chain, the sickle raised, landing with a chop.

**attack_chain_skill (Entangle).** *Anticipation*: three fast turns overhead (carried over from `charge_chain`).
*Contact*: released overhand; the weight flies 7 m and on meeting a body goes round it three times (the far end
of `ChainLine` circles the target). *Follow-through*: the left hand takes a turn of chain round the wrist and
sets; the stance drops low and wide, holding the line. *From above*: a line from player to enemy that stays there.

**attack_chain_finisher ("Unwinding").** The moment is a **body spun in on a line**.

1. 0.00 to 0.16: the weight is thrown and goes round the kneeling enemy three times: arms bound to its sides
   (beat at 0.16).
2. 0.16 to 0.50: the player turns their back, puts the chain over the right shoulder, and **walks** it, three
   heavy steps, hauling. The enemy is dragged upright and comes in toward the player **spinning**, as the chain
   unwinds off it: three full turns over 2.5 m (`TargetOffset` and `TargetTurn` beats). From above: a taut line
   and a body turning like a top along it.
3. 0.50 to 0.66: **the hold**. The player has stopped, back still turned, sickle raised level at the right
   shoulder, blade pointing back. The enemy is still coming, still turning. The camera leans in.
4. 0.66: without looking, one backward hooking cut as the enemy arrives at the player's shoulder. The real blow.
   Stop, slow time. The enemy carries on past and falls in front; the weight drops out of the air after it.

*Shared or new*: all new. *First pass without E12*: the enemy stays where it kneels; the player throws, hauls
themself to it along the chain (`PullSelf`), and makes the same blind backward cut. *Fixed targets*: that same
first pass is the permanent form.

### Elements

- **Lightning**: a chain is a wire. Static laid on one end of a taut chain is on both: while an enemy is Bound,
  each blow the player lands on **anything** adds a stack of Static to the Bound one, and a discharge on it runs
  back down the chain as a visible bolt. Upgrade, **Live Wire** (Lightning, needs the Chain Sickle): Thunderclap
  cast while an enemy is Bound strikes it without fail and leaps along the chain to everything the chain crosses.
- **Ice**: the chain freezes what it holds. Each second Bound lays a stack of Chill, so a full bind ends just
  short of Frozen and one weight blow completes it; the whirl's two rings chill a whole crowd. Upgrade, **Frost
  Shackle** (Ice, needs the Chain Sickle): a Frozen enemy pulled by the yank is not dragged but **shattered** in
  place, and its shards strike what stands near.
- **Dark**: the chain as a leash on something dying. While an enemy is Bound and Cursed, a share of the damage
  dealt to it comes back down the chain as health. Upgrade, **Tithe** (Dark, needs the Chain Sickle): killing a
  Bound enemy keeps the chain's far end where it fell for 4 s; the next Entangle within that time is free.

---

## 5. Scythe

**Choice: the sweet spot (idea (a) in GAME.md), bettered with a blade that can be left hanging in the air.** Why
not the others: hook and pull (b) is what the Chain Sickle now does, and two weapons that drag enemies about would
blur each other. Reaping (c) is a meter that pays out healing or a big sweep; healing from blows belongs to Dark
and Life and the trees already hold it, and a meter does not change where the player stands. The sweet spot does:
it is a rule about **distance**, which a top-down camera shows better than any other.

**Idea.** The longest blade on the longest haft, swung in great slow arcs, and it only truly cuts with its last
metre: the tip does half as much again, the haft does little. It asks you to **keep the enemy at exactly the right
distance, and never let it close**: the only weapon whose heavy attack steps backward.

**How it changes play.** The player fights at 3 m and treats 1.5 m as danger. Enemies that rush are a problem to
be solved with the back-stepping heavy and with the Crescent, which is left spinning in the air between you and
them like a gate. Crowds at the right range are mown.

### Numbers

`Name = "Scythe"`, `Carry = "scythe"`, `SkillName = "Crescent Slash"`, `ChargeAnimation = "charge_scythe"`,
`SlashColour = (1.4, 1.5, 2.1, 0.9)`. `SkillManaCost = 30`, `ChargeTime = 0.40`. No `ChargedHeavy`.

Every step but the skill's shot carries the same bands (E3): `InnerReach 1.5`, `InnerDamage 0.6`, `SweetFrom 2.5`,
`SweetDamage 1.5`, `SweetStagger 1.6`. The table gives the plain (middle) values.

| slot | Name | Sweep | Windup | Active | Recovery | Damage | Stagger | StaminaCost | ManaGain | Lunge | Reach | ArcDegrees | HitStop | Shake | new |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Steps[0] | attack_scythe_1 | +1 | 0.20 | 0.14 | 0.36 | 13 | 12 | 13 | 6 | 0.3 | 3.5 | 200 | 0.05 | 0 | bands |
| Steps[1] | attack_scythe_2 | -1 | 0.14 | 0.12 | 0.30 | 13 | 12 | 12 | 6 | 0.3 | 3.5 | 200 | 0.05 | 0 | bands |
| Steps[2] | attack_scythe_3 | +1 | 0.30 | 0.16 | 0.50 | 20 | 22 | 16 | 9 | 0.5 | 3.7 | 360 | 0.08 | 0.08 | bands |
| Heavy | attack_scythe_heavy | -1 | 0.28 | 0.12 | 0.46 | 24 | 28 | 18 | 10 | -1.4 | 3.8 | 150 | 0.09 | 0.14 | bands |
| ChainHeavy | attack_scythe_chain | +1 | 0.20 | 0.12 | 0.48 | 28 | 34 | 18 | 12 | 0.8 | 3.6 | 120 | 0.10 | 0.18 | bands |
| Skill | attack_scythe_skill | +1 | 0.14 | 0.12 | 0.50 | 16 | 18 | 15 | 0 | 0 | 3.6 | 180 | 0.10 | 0.20 | bands, Shot |
| Finisher | attack_scythe_finisher | +1 | 0.78 | 0.14 | 0.56 | 30 | 0 | 0 | 15 | 0 | 3.8 | 360 | 0.22 | 0.50 | Beats |

The chain, 2.22 s and 41 stamina: at the tip 69 damage and 74 stagger (the best light chain in the game); in the
middle band 46 and 46; on the haft 28 and 28 (the worst). The long, short, long beat is 0.70, 0.56, 0.96 s: the
second cut is the return of the first and uses its follow-through as its own wind-up.

**Crescent Slash.** The cut in the table, and off the blade a crescent of light 3.2 m wide: `Shot` with
`Speed 12`, `Range 6`, `Radius 1.6`, `Damage 20`, `Stagger 22`, `Pierce` true, then `HangSeconds 1.6`,
`HangBite 6` damage and 6 stagger, `HangEvery 0.4`. It flies 6 m in half a second, striking each enemy once on the
way, and then **stays**, turning on the spot for 1.6 s and biting what stands in it. It is a gate: thrown past an
enemy it cuts off the retreat; thrown between you and a rusher it makes the rusher choose. *Different from Fire's
Burning Crescent* on the Katana (a flame that flies on and is gone). **Engine**: E8 with the hang; a first pass
is two `Hazards.Lay` calls (one moving and `once`, one still and delayed 0.5 s) and a turning quad.

**Engine work**: E1, E2, E3 (bands), E7 (negative `Lunge`), E8, E12, E13.

### Guard and dodge

- Guard: `PerfectWindow 0.18`, `StaminaPerDamage 1.0`, `ChipFraction 0.35`, `ArcDegrees 220`. The haft held
  crosswise in both hands covers wide. Ordinary otherwise.
- Dodge: `Distance 4.8`, the rest default: a long gliding step, because distance is this weapon's whole business.
  `MoveFactor 1.0`.

### Model

**Haft** 1.9 m, 0.04 thick, in three straight segments with a slight S to it (bent 6 degrees at each joint), dark
wood (`leather`), with a peg handle 0.12 m long at 0.75 m from the butt, standing out to the right, and an iron
spike 0.1 m at the butt. **Blade**: a crescent of 0.95 m chord, 0.14 m wide at the root and tapering to a point,
set at 80 degrees to the haft at its top, curving back toward the bearer; `iron` spine, `steel` face, and the
inner (cutting) edge, a strip 0.02 wide, is the **inlay**. An iron collar where they meet, with a strip of
`accent` cloth 0.4 m long tied on (a two-bone tail). About 160 triangles. **Hands**: right at 1.15 m from the butt,
left on the peg (`GripGap 0.4`), sliding to the butt for the widest cuts (`GripGap 1.0`). **Carry** (`Stowed`
false): over the right shoulder, haft slanting down across the back to the left hip, blade above and behind the
head pointing left. From above the crescent sits behind the player's head like a dark halo. `socket_back` for
rest.

### Animation

All new. This weapon's whole character is the pendulum: no cut stops, each one is caught and turned back.

| clip | seconds | what it is |
|---|---|---|
| idle_scythe | 3.2 loop | Upright, almost at ease, the haft over the right shoulder with the right forearm resting along it, left hand loose on the peg behind. The slowest idle: one long breath a loop; the blade's point dips and rises 5 cm. |
| run_scythe | 0.58 loop | Haft carried level at the right hip in both hands, blade trailing low behind and to the right, edge up, its point a hand above the ground. From above: a long line behind the player ending in a hook. |
| guard_scythe | 0.14 | The haft swung up to stand slantwise across the body, blade end high to the left with the edge turned away, butt low to the right; hands wide apart; body side-on behind it. |
| guard_hit_scythe | 0.22 | The haft bows toward the body; the back foot slides; the blade end nods. |
| guard_perfect_scythe | 0.30 | The haft is rolled about its middle like a staff: the butt end knocks the blow aside to the right and the blade end comes over the top in the same turn, finishing drawn back over the right shoulder. |
| charge_scythe | to ChargeTime, then held | The scythe drawn far back to the right, level, at arm's length, the body wound 80 degrees right and sunk on the right leg, blade behind the body with its point touching the ground. |

**attack_scythe_1 (out, right to left).** *Anticipation*: from the shoulder the blade is let fall back and right;
the hips wind right 45 degrees, the left hand slides to the butt. *Contact*: a level mowing cut at knee to hip
height, both arms long, the hips unwinding left; the body **leans back away** from the blade's pull, so the feet
stay where they are. *Follow-through*: the blade carries round to the far left and a little up. *From above*: a 200
degree fan whose bright outer metre is the part that matters (the arc's outer band is drawn brighter, E3).

**attack_scythe_2 (back, left to right).** *Anticipation*: none of its own: the wrists roll at the top of the
first cut's follow-through and the edge turns over. *Contact*: the same arc mown back at chest height.
*Follow-through*: the blade swings up behind the right shoulder and its weight starts the body turning.

**attack_scythe_3 (the full moon).** *Anticipation*: a full wind: the back turned to the enemy, the blade
trailing low on the right, head over the left shoulder; a long beat. *Contact*: one whole turn on the left foot
with the scythe at full stretch, the blade rising from knee height to shoulder height as it goes round.
*Follow-through*: the haft is let slide through the hands and caught at the collar, the butt swung down and
planted; the bearer ends standing straight beside it. *From above*: a complete ring at 3.7 m, tilted: it reads as
a rising spiral.

**attack_scythe_heavy (the back reap).** *Anticipation*: the blade is reached out long **past** the enemy, at full
stretch forward and to the left, edge turned back toward the bearer. *Contact*: both feet leave the ground in a
flat hop backward of 1.4 m while the arms haul the blade back and across, left to right: the cut is made by the
retreat. *Follow-through*: lands low, haft across the knees, blade out to the right. *From above*: the player
slides down the screen and an arc is torn across where they stood. An enemy that was too close is now at the tip.

**attack_scythe_chain (the rising moon).** *Anticipation*: the point is dropped to the ground at the left.
*Contact*: a skip forward and an upward diagonal from the left foot to above the right shoulder.
*Follow-through*: the haft spins once over the head (hand over hand) before it settles on the shoulder.

**attack_scythe_skill (Crescent Slash).** *Anticipation*: from the charge pose, the inlay lit along the edge.
*Contact*: a level cut at chest height, right to left, snapped to a dead stop when the blade points straight
ahead: the light goes on without it. *Follow-through*: the blade held out pointing after the crescent for the
first third of the recovery, then swung up to the shoulder. *From above*: the arc of the cut, then a second,
brighter crescent that leaves it and stops, turning.

**attack_scythe_finisher ("The Wheel").** The moment is **a body carried round a full circle, and a wait under
it**.

1. 0.00 to 0.16: step in; the blade is hooked under the kneeling enemy from the right (beat).
2. 0.16 to 0.52: the bearer turns once on the spot, slowly, hauling: the enemy is lifted off the ground on the
   crook of the blade and carried **all the way round the player** at 3 m, rising as it goes (`TargetOffset`
   beats at the four quarters). From above: a body swept round a great circle, the haft as the spoke.
3. 0.52: at the top of the turn the blade is dropped out from under it. The enemy hangs in the air, 1.5 m up, in
   front.
4. 0.52 to 0.78: **the hold**. The bearer sinks, the scythe drawn far back and low on the right, perfectly still,
   looking up. The camera leans in. The enemy starts to fall.
5. 0.78: one rising cut that meets it on the way down. The real blow. The cut's arc is drawn as a full, closed
   crescent that stays on the screen through the slowed time.

*Shared or new*: all new. *First pass without E12*: steps 4 and 5 only, against the kneeling enemy: draw back,
hold, one great rising cut. *Fixed targets*: the hook slides off (a spark), the turn is made empty, and the hold
and the cut are as written.

### Elements

- **Dark**: the reaper's element. A kill made with the tip heals, as Cursed kills do, and the hanging Crescent
  Curses what it bites. Upgrade, **Harvest** (Dark, needs the Scythe): each enemy cut by the tip in one swing adds
  to a count, up to five; the next heavy spends the count, growing its arc and its damage for each.
- **Life**: tip blows lay Seeded, and a Seeded enemy killed by the scythe bursts into thorns in a ring at the
  tip's distance, not round the body, so the burst falls where the next enemy should be standing. Upgrade, **Reap
  and Sow** (Life, needs the Scythe): where the Crescent hangs, a briar grows when it fades and holds the ground
  for 4 s, slowing and Seeding.
- **Wind**: Unsteady enemies are pushed further by every blow, and the scythe pushes them outward along its arc:
  haft blows shove an enemy back **into** the sweet band. The Crescent flies 10 m and draws enemies a little
  toward itself while it hangs. Upgrade, **Gale Arc** (Wind, needs the Scythe): each cut throws a thin copy of
  itself one metre further than the blade reaches.

---

## 6. Great Bow

**Choice: planted arrows (idea (b) in GAME.md, where Landmines fits), bettered by making the bow's own melee the
detonator.** Why not the others: a pure point-blank bow (a) is a slow gun; a pinning shot (c) is one move, not a
weapon. Planted arrows alone would break the second pillar (close combat comes first): the player would plant from
range and set them off from range. So here they are set off by **hitting the enemy with the bow**. The shot is the
set-up, the blow is the payoff, and the weapon obeys the rule every magic feature must: it makes the player want to
close in.

**Idea.** A war bow taller than its bearer, with horn-shod tips for striking: arrows are planted in enemies and in
the ground, and every blow of the stave on an enemy snaps the arrows standing in it. It asks you to **work two
ranges in sequence**: step off and plant, step in and cash.

**How it changes play.** Each fight has a breath: out, in, out. The kick shot throws the player back to shooting
distance at the end of a melee chain, so the weapon does its own footwork. Arrows that miss stand in the ground as
traps, so a player who is driven back retreats through their own minefield.

### Numbers

`Name = "Great Bow"`, `Carry = "bow"`, `SkillName = "Landmines"`, `ChargeAnimation = "charge_bow"`,
`SlashColour = (1.8, 1.7, 1.3, 0.85)`. `SkillManaCost = 35`, `ChargeTime = 0.60`, `SkillChargeTime = 1.00`,
`ChargeMoveFactor = 0.35` (the player walks and turns while drawing).

| slot | Name | Sweep | Windup | Active | Recovery | Damage | Stagger | StaminaCost | ManaGain | Lunge | Reach | ArcDegrees | HitStop | Shake | new |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Steps[0] | attack_bow_1 | -1 | 0.13 | 0.07 | 0.30 | 9 | 14 | 9 | 5 | 0.6 | 2.6 | 130 | 0.045 | 0 | snaps arrows |
| Steps[1] | attack_bow_2 | 0 | 0.20 | 0.08 | 0.38 | 12 | 10 | 11 | 6 | 0.9 | 2.2 | 50 | 0.06 | 0 | snaps, then plants 1 |
| Heavy | attack_bow_heavy | 0 | 0.32 | 0.06 | 0.40 | 0 | 0 | 16 | 8 | 0 | 0 | 0 | 0.07 | 0.08 | Shot A, Recoil 0.3 |
| ChainHeavy | attack_bow_chain | 0 | 0.16 | 0.08 | 0.44 | 6 | 12 | 16 | 8 | 0.3 | 1.8 | 80 | 0.08 | 0.12 | Shot B, Recoil 3.0 |
| ChargedHeavy | attack_bow_charged | 0 | 0.06 | 0.08 | 0.50 | 0 | 0 | 24 | 12 | 0 | 0 | 0 | 0.11 | 0.22 | Shot C, Recoil 0.8 |
| Skill | attack_bow_skill | 0 | 0.26 | 0.08 | 0.46 | 0 | 0 | 15 | 0 | 0 | 0 | 0 | 0.08 | 0.16 | see below |
| Finisher | attack_bow_finisher | 0 | 0.80 | 0.10 | 0.60 | 30 | 0 | 0 | 15 | 0 | 2.6 | 120 | 0.24 | 0.55 | Beats |

Where a row's `Damage` is 0 the blow is the shot's; the shot pays the row's `ManaGain` when it strikes.

| Shot | Speed | Range | Radius | Damage | Stagger | Pierce | Sticks | Heavy |
|---|---|---|---|---|---|---|---|---|
| A (snap shot) | 30 | 9 | 0.4 | 18 | 22 | no | yes | yes |
| B (kick shot) | 30 | 7 | 0.4 | 20 | 24 | no | yes | yes |
| C (full draw) | 40 | 14 | 0.5 | 34 | 44 | yes | yes, in each | yes |

**Planted arrows (E14, `PlantedArrows`).**

- An arrow that strikes an enemy stays in it: up to three in one enemy (a fourth replaces the oldest), for 12 s
  each, drawn standing out of its body with the fletching in the element's colour.
- A melee blow of the bow (either light, and the kick of the kick shot) that lands on an enemy **snaps** every
  arrow in it: 10 damage and 14 stagger an arrow, on top of the blow, each with its own small flash 0.05 s apart.
  Three arrows and the swat is 39 damage and 56 stagger in one blow: more than the Katana's heavy, earned by three
  shots.
- The second light is a stab with an arrow held in the fist, and leaves that arrow in the enemy (after snapping
  what was there). So swat, stab, swat is a small loop that needs no shooting at all: the bow works in a corridor.
- An arrow that strikes no enemy stands in the ground where its `Range` ended, for 8 s, up to six at once. An
  enemy that comes within 1.1 m of one sets it off: 10 damage and 14 stagger in 1.6 m. The player does not.
- With an element held, an arrow that snaps or goes off also lays one stack of that element's affliction.

Balance without arrows: the two lights are 21 damage and 24 stagger in 1.16 s, the weakest melee in the game on
purpose. The snap shot alone is 18 damage in 0.78 s from nine metres, weak and safe. The weapon is only good when
the two are woven.

**Landmines.** The bow is tipped back and a handful of arrows loosed almost straight up. Half a second later five
of them fall in a ring 2.5 m across round the enemy the player faces (within 9 m; else 5 m ahead), each striking
8 damage and 8 stagger in 0.8 m where it lands and then standing as a ground arrow for 10 s. An enemy inside the
ring cannot leave it without setting one off; a player with their back to a wall has made a wall. **Engine**: five
`Hazards.Lay` with `delay 0.5` and `burst`, then five ground arrows.

**Engine work**: E1, E2 (left-hand aim, props), E5 (two tiers, walking while charging), E7 (`Recoil`), E8
(`Sticks`), E12, E13, and E14 (`PlantedArrows`): the second most of any weapon.

### Guard and dodge

- Guard: `PerfectWindow 0.18`, `StaminaPerDamage 1.15`, `ChipFraction 0.4`, `ArcDegrees 180`. A stave is not a
  shield. `PerfectGuardRider`: the arrow in the right fist is driven into the attacker as the blow is turned: one
  planted arrow. A perfect guard is always worth a snap.
- Dodge: the defaults.

### Model

**Bow** 1.9 m, asymmetric like a war bow gripped a third of the way up (upper limb 1.25 m, lower 0.65 m), each
limb four straight segments curving away from the bearer and recurving at the tip; 0.05 x 0.03 m in section,
tapering; black lacquer (`iron`) with `wrap` bindings at five places and a `leather` grip 0.14 m long. Each tip is
shod with a horn cap 0.10 m long (`straw`), the striking ends. A strip down the belly of both limbs is the
**inlay**. The **string** is a two-point line drawn by code from tip to tip, through the right hand's socket when
`PropAHeld` is true and the bow is drawn. About 140 triangles. **Arrows** 1.1 m, 0.02 thick, a leaf head 0.10 m in
`steel`, three vanes in `accent` red (the element's colour when one is held); 20 triangles; one in the right hand
whenever the bow is in hand (it is the stabbing arrow and the next one to shoot), five more in a **quiver** on the
right of `socket_hip`, slanting back (not on the back: the hat is there). **Hands**: the bow in the **left** hand
(`AimBladeL`: `BladeL` is the direction of the upper limb), arrow in the right. **Carry** (`Stowed` false): at
rest the bow is slung on `socket_back`, string across the chest.

### Animation

All new, and the only set whose weapon is in the left hand. The stave blows can take their feet from the Katana's
`DrawCut` (a one-handed cut across), mirrored.

| clip | seconds | what it is |
|---|---|---|
| idle_bow | 2.4 loop | Standing tall and side-on, left side to the enemy, bow upright in the left hand with its lower tip a hand off the ground, arrow held low across the thighs in the right. The head turns slowly, scanning. From above: a long thin upright line at the player's left. |
| run_bow | 0.56 loop | Bow carried level in the left hand at the hip, pointing ahead like a lance; arrow hand pumping. |
| guard_bow | 0.14 | The bow swung to stand slantwise across the front in both hands (the right hand comes onto the upper limb), belly to the enemy, body low behind it. |
| guard_hit_bow | 0.22 | The stave flexes visibly (the tips nod back) and the bearer gives a step. |
| guard_perfect_bow | 0.30 | The lower tip hooks the blow down and aside to the left; the right hand, with the arrow held like a dagger, punches in over the top and comes away empty. |
| charge_bow | to ChargeTime, then held | The draw. The bow is raised high above the head, arrow nocked, and brought down as the hands separate, until the right hand is behind the ear and the left arm straight at the enemy: the classical high draw. Hips square, feet wide. At full draw (first tier) the arms tremble; held to the second tier, the bow is tipped up 50 degrees to the sky. The legs keep walking under it (upper body over locomotion would be ideal; a first pass slides a fixed stance). |

**attack_bow_1 (the swat).** *Anticipation*: the bow is drawn back across the body to the right hip, upper limb
trailing. *Contact*: a backhand sweep with the upper limb, left to right, at head height, the left arm straightening
and the hips opening left; the horn cap leads. *Follow-through*: the stave carries out to the left and the right
hand turns the arrow point-down in the fist. *From above*: a long thin arc, 2.6 m: longer and finer than a sword's.

**attack_bow_2 (the arrow stab).** *Anticipation*: the right fist goes up by the ear with the arrow as a dagger;
the bow is held out to the left, out of the way. *Contact*: a stride and an overhand stab down into the chest.
*Follow-through*: the hand comes away open, the arrow left standing; the body bounces back off the blow and the
right hand goes straight to the quiver at the hip. *From above*: the body closes, parts, and a bright mark is left
on the enemy.

**attack_bow_heavy (the snap shot).** *Anticipation*: nock and half draw at chest height, no ceremony, the body
turned side-on; a short held beat at the anchor. *Contact*: loose. The right hand flies back, the bow rocks
forward in the left and its upper tip tips toward the target (the bow turning in the hand after the release).
*Follow-through*: the body sways back 0.3 m with the recoil; the right hand sweeps down to the quiver. *From
above*: a hard line up the screen.

**attack_bow_chain (the kick shot).** *Anticipation*: a skip in and the right knee lifted. *Contact*: a flat
stamping kick to the chest (it snaps the arrows); the kicking leg **pushes off** and the body goes backward and up,
laid out level in the air, and in the air the bow is drawn across the chest and loosed down the line of the body.
*Follow-through*: lands 3 m back on the left foot and right knee, bow level, already reaching for an arrow. *From
above*: the two figures spring apart and a line joins them again at once.

**attack_bow_charged (the full draw).** From `charge_bow`. *Contact*: the string hand opens; the shot is the
loudest thing the weapon does: the bow kicks, the bearer's whole body is rocked back 0.8 m on the heels, the scarf
snaps forward. *Follow-through*: the bow arm stays out, pointing, for a third of a second. *From above*: a line
that crosses the whole screen and everything on it.

**attack_bow_skill (Landmines).** *Anticipation*: from the second tier, the bow tipped up, three arrows held
between the fingers of the draw hand. *Contact*: loosed upward; the body arches back under the bow.
*Follow-through*: the bearer straightens and lowers the bow without looking up, as the arrows come down. *From
above*: nothing for half a second (five thin shadows gather on the ground), then five strikes in a ring.

**attack_bow_finisher ("Nailed").** The moment is **a full draw held at arm's length from the target, and what
follows it**.

1. 0.00 to 0.18: step in; the lower limb sweeps the kneeling enemy's legs and the upper limb, coming over,
   knocks it flat on its back (two beats).
2. 0.18 to 0.36: the bearer steps over it and sets the left foot on its chest. The bow comes up overhead.
3. 0.36 to 0.80: **the hold**. The high draw, made slowly, straight down, until the arrow's head is a hand from
   the enemy's chest and the bow is bent to its limit. The camera leans in from 0.36 and stays. The arms tremble;
   the inlay brightens along both limbs to the tips.
4. 0.80: loose. The real blow: the arrow goes through and into the ground; a ring of dust and one hard frame.
5. In the slowed time, every arrow that was standing in the enemy goes off in turn, 0.08 s apart, each at the
   snapping value (the payoff grows with how well the fight was played). The bearer steps off and walks away
   without looking back, drawing the next arrow.

*Shared or new*: all new. *First pass without E12*: the enemy is not thrown down; the bearer sets the bow's lower
tip against its chest to hold it upright and makes the same slow draw at point-blank. *Fixed targets*: the same.

### Elements

- **Fire**: arrows are fuses. A planted arrow burns its host for as long as it stands, and when snapped it bursts
  in fire (the burst sets off Burn and Detonate if that is learned). Landmines leaves five burning patches.
  Upgrade, **Powder Trail** (Fire, needs the Great Bow): arrows standing in the ground within 4 m of each other
  are joined by a line of fire, and setting one off sets off all that are joined.
- **Poison**: arrows are needles. Each planted arrow lays a stack of Poisoned every 2 s, in time with the bursts,
  so three arrows left alone do slow work and three arrows snapped do fast work: the player chooses. Upgrade,
  **Barbed Heads** (Poison, needs the Great Bow): snapping arrows no longer removes them, once; the second snap
  does.
- **Light**: arrows are marks. A planted arrow makes its host Marked (blows are critical and give more mana), so
  the bow becomes the mana weapon for a spell-heavy build, and the full draw passes through walls of enemies
  marking each. Upgrade, **Constellation** (Light, needs the Great Bow): three or more enemies carrying arrows are
  joined by lines of light; a snap on one snaps all.

---

## 7. Great Spear

**Idea.** A 2.9 m spear: the longest straight reach in the game, down one narrow line, from behind a guard that
never has to come down. It asks you to **own a line**: point the spear down the lane the enemy must come up, thrust
on a steady count without lowering the guard, and when the line is lost, throw the spear and go after it.

**How it changes play.** It is the stand-your-ground weapon the user asked the castle's fights to become ("the
smarter, even exchange of Sekiro"): the player holds the guard and thrusts from it, which no other weapon can do.
Its weakness is the flank: lanes miss what circles, the haft is weak up close, and its guard is the narrowest. Its
one great leap, the Flying Spear, moves the player 11 m in a quarter of a second.

### Numbers

`Name = "Great Spear"`, `Carry = "spear"`, `SkillName = "Flying Spear"`, `ChargeAnimation = "charge_spear"`,
`SlashColour = (1.4, 1.8, 2.0, 0.85)`. `SkillManaCost = 30`, `ChargeTime = 0.50`, `SkillChargeTime = 0.90`,
`ChargeMoveFactor = 0.3`.

Thrusts and the heavy carry `InnerReach 1.4`, `InnerDamage 0.6` (too close, and it is the haft).

| slot | Name | Sweep | Windup | Active | Recovery | Damage | Stagger | StaminaCost | ManaGain | Lunge | Reach | ArcDegrees | HitStop | Shake | new |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Steps[0] | attack_spear_1 | 0 | 0.14 | 0.07 | 0.27 | 10 | 10 | 10 | 5 | 0.5 | 4.0 | - | 0.045 | 0 | LineWidth 0.9, Inner |
| Steps[1] | attack_spear_2 | 0 | 0.14 | 0.07 | 0.27 | 11 | 10 | 10 | 5 | 0.5 | 4.0 | - | 0.045 | 0 | LineWidth 0.9, Inner |
| Steps[2] | attack_spear_3 | +1 | 0.20 | 0.10 | 0.36 | 15 | 20 | 14 | 8 | 0.6 | 3.6 | 160 | 0.07 | 0.06 | |
| GuardAttack | attack_spear_guard | 0 | 0.16 | 0.07 | 0.30 | 8 | 8 | 12 | 4 | 0.2 | 3.8 | - | 0.04 | 0 | LineWidth 0.8, Inner |
| Heavy | attack_spear_heavy | 0 | 0.30 | 0.10 | 0.48 | 24 | 32 | 20 | 10 | 0.8 | 4.4 | - | 0.10 | 0.16 | LineWidth 1.1, Inner |
| ChainHeavy | attack_spear_chain | 0 | 0.22 | 0.12 | 0.46 | 20 | 44 | 18 | 10 | 2.4 | 2.4 | 80 | 0.10 | 0.20 | Pull -2.0 |
| ChargedHeavy | attack_spear_charged | 0 | 0.08 | 0.34 | 0.50 | 30 | 36 | 26 | 12 | 0 | 3.2 | - | 0.10 | 0.24 | LineWidth 1.3, Travel 5.0, Pull -1.5, Poise 16 |
| Skill | attack_spear_skill | 0 | 0.20 | 0.06 | 0.20 | 0 | 0 | 15 | 0 | 0.6 | 0 | 0 | 0.08 | 0.14 | Shot, follow |
| (follow) | attack_spear_recall | 0 | 0.22 | 0.10 | 0.36 | 18 | 26 | 0 | 0 | 0 | 2.6 | 360 | 0.12 | 0.30 | |
| Finisher | attack_spear_finisher | 0 | 0.72 | 0.12 | 0.66 | 30 | 0 | 0 | 15 | 0 | 3.6 | 120 | 0.24 | 0.60 | Beats |

The chain: 36 damage and 40 stagger in 1.62 s for 34 stamina. A touch under the Katana in every number, at one
and a half times its reach, down a lane 0.9 m wide. The count is even: thrust, thrust (0.48 s each), then the
sweep that clears what has come round the side (0.66 s). The guard thrust is slower and weaker and costs more: it
is for holding, not for killing.

**Flying Spear.** The spear is thrown: `Shot` with `Speed 34`, `Range 11`, `Radius 0.5`, `Damage 26`,
`Stagger 30`, `Pierce` false, `Sticks` true, `Heavy` true. It stops in the first enemy it meets, or stands in the
ground at 11 m. Then, 0.15 s later, the player **goes to it**: a 0.22 s dash along the spear's path during which
blows pass through (the same rule as the roll's safe window), arriving with `attack_spear_recall`: the spear is
ripped out in a turning cut that strikes all round. Holding the stick **away** from the throw at that moment
refuses the dash: the spear tears itself loose and flies back to the hand over 0.5 s, and for that half second
the player has no attacks. So one button is three things: a 26 damage shot from range, a gap-closer onto a caster,
and an escape across the arena onto empty ground. **Engine**: E8 with `Sticks`, and a new **follow** on the skill
(`FollowStep`, `FollowDelay 0.15`, `FollowSeconds 0.22`): the dash reuses `PlayerController`'s leap movement
(Meteor Leap) laid flat.

**Engine work**: E1, E2 (props, negative `GripGap`), E3 (`LineWidth`, inner band), E5, E6, E7 (`Travel`), E8, E9
(shove), E10 (`GuardAttack`), E12, E13, and the follow above.

### Guard and dodge

- Guard: `PerfectWindow 0.18`, `StaminaPerDamage 0.8`, `ChipFraction 0.3`, `ArcDegrees 160`, `GuardMoveFactor
  0.5`. Strong and cheap along the line, and the narrowest in the game: a spearman who is flanked is hit. A light
  press while guarding thrusts without lowering it (`GuardAttack`).
- Dodge: `Distance 4.2`, the rest default.

### Model

2.9 m in all (the temple soldier's is 2.8 m; the player's should be no shorter). **Shaft** 2.3 m, 0.04 thick,
dark wood (`leather`), with `wrap` bindings at the two grips and three iron rings below the head. **Head** 0.5 m:
a straight leaf blade 0.38 x 0.09 x 0.02 in `steel` with a raised midrib that is the **inlay**, on a socket 0.12
long in `iron`, with a short crossbar 0.16 wide at the socket's base (it stops the spear going too deep, and from
above it makes the head a cross and not a splinter). A tuft of `accent` red cord 0.2 m under the crossbar (a
two-bone tail). **Butt**: an iron cap and spike 0.1 m. About 120 triangles. **Hands**: right at 1.0 m from the
butt, left 0.6 m ahead of it toward the head (`GripGap -0.6`); for the long thrusts the left hand lets the shaft
slide and the right drives from the hip. **Carry** (`Stowed` false): upright in the right hand at rest, butt on
the ground. `socket_back` holds it slanted, head above the left shoulder.

### Animation

New, but with the most to borrow of any weapon: the temple soldier and the Gate Captain already have thrust,
sweep, couch-and-charge and guard poses written for the same skeleton in `EnemyAnimations.cs` and
`CaptainAnimations.cs`. Take their feet and hips as a first pass and re-pose the upper body for a longer spear;
the player's versions must be quicker and finer than a soldier's, and must not look like one.

| clip | seconds | what it is |
|---|---|---|
| idle_spear | 2.8 loop | Standing side-on, left foot ahead, the spear upright in the right hand with its butt on the ground by the right foot and its head a metre above the bearer's own; left hand resting on the belt. Still and patient: the red cord moves more than the body. From above: a point beside the player with a cross on top. In a fight (after any attack, for 2 s) the stance is the **line**: spear levelled at the hip, pointing dead ahead, left hand forward on the shaft. |
| run_spear | 0.56 loop | The spear trailed in the right hand at the balance point, level, head forward and low, butt behind and high; left arm pumping. |
| guard_spear | 0.14 | Sunk low behind the levelled spear: left hand far forward on the shaft, palm down, right hand at the right hip, the shaft slanting up from hip to head so the point is at an enemy's throat. Left shoulder leads, chin behind it. From above: one long line pointing straight at the enemy. That is the guard's whole meaning, and the blue guard band on the ground is narrow to match. |
| guard_hit_spear | 0.22 | The shaft is knocked off the line to one side and brought straight back; the back foot digs. |
| guard_perfect_spear | 0.30 | A tight circle of the point (the left hand as the pivot) that winds round the incoming weapon and flings it out to the right, ending with the point back on the line and a hand's breadth further forward. |
| charge_spear | to ChargeTime, then held | The spear lifted and couched under the right arm, locked against the ribs, left hand out along the shaft, the body dropped into a sprinter's lean with the back heel raised. At the second tier the spear is lifted to above the right shoulder, level, for the throw, left hand pointing at the target. |

**attack_spear_1 (thrust).** *Anticipation*: the right hand draws the shaft back through the left until the fist
is behind the hip; the hips turn right a little, the weight goes to the back foot. *Contact*: the right hand
drives forward to meet the left; the hips snap square; the front foot slides forward half a step; the point goes
out to 4 m, flat, at chest height. *Follow-through*: the point dips a hand and is drawn straight back along the
same line (a spear is pulled back as fast as it is pushed). *From above*: the line simply gets longer and shorter.
To make that read, the body **leans** into it (the chest goes over the front knee) and the thrust is drawn with a
long thin flash down the lane.

**attack_spear_2 (thrust, high).** The same from the other foot: the back foot steps up past the front (a
passing step) and the point goes in at head height, the right hand finishing above the shoulder, palm up. Seen
from above the difference is the step and the lifted elbow.

**attack_spear_3 (the sweep).** *Anticipation*: the point is taken out wide to the right and low, the left hand
sliding back to join the right near the butt, so the whole length is free. *Contact*: a flat cut with the blade's
edge, right to left, at 3.6 m, the body turning under it. *Follow-through*: the shaft carries round behind the
back, passes from the right hand to the left there, and comes out on the other side to the line again: a flourish
that is also the fastest way back to guard. *From above*: a wide thin arc after two straight lines.

**attack_spear_guard.** From `guard_spear`: only the arms move. The right hand shoots the shaft forward through
the left, and back. The body, the feet and the guard band on the ground do not change. It should look almost
mean.

**attack_spear_heavy (the long chop).** *Anticipation*: both hands slide to the butt and the spear is raised
upright, then let fall backward over the right shoulder. *Contact*: brought over and down like a flail, full
length, the head landing on the ground 4.4 m ahead. *Follow-through*: the shaft whips and bounces; the bearer is
bent double, hands at the knees. *From above*: a line slapped down the lane with dust at its far end.

**attack_spear_chain (the vault).** *Anticipation*: the head is stabbed into the ground a stride ahead.
*Contact*: a run of two steps and the bearer swings up on the shaft, body level, and drives both feet into the
target 2.4 m on. *Follow-through*: lands beyond the planted spear and pulls it out of the ground behind without
looking. *From above*: the body swings round a fixed point like a door on a hinge and the target is knocked back
2 m.

**attack_spear_charged (the couched charge).** From `charge_spear`. *Contact*: a 5 m run in a third of a second,
low, spear locked under the arm; whatever the point meets is carried along and pushed off the end. *Follow-through*:
a long skid on both feet, turning side-on to stop. *From above*: a lane driven up the screen with the player in
it. It should be recognisably what the Gate Captain did to the player, turned round.

**attack_spear_skill (Flying Spear) and attack_spear_recall.** *Anticipation*: from the throwing pose, a
crow-hop: the back foot crosses behind, the spear arm drawn far back, the left hand pointing. *Contact*: thrown;
the body follows the arm over the front leg until the right hand nearly touches the ground. *Follow-through and
the dash*: the bearer pushes off that hand and runs the spear's own path, low and blurred (the afterimage shell
in steel-white). *Recall*: arriving, both hands take the shaft and the body's speed is turned into a full spin
that rips the spear out sideways. *From above*: a thrown line, then the player becomes a second line along it,
then a ring.

**attack_spear_finisher ("The Standard").** The moment is **a body raised overhead on the spear and held
there**.

1. 0.00 to 0.14: step in; a low thrust takes the kneeling enemy under the ribs (beat: hit stop 0.05).
2. 0.14 to 0.40: the bearer drops the butt to the ground, sets a foot against it, and **levers**: hand over hand
   up the shaft, the enemy is raised off the ground on the head of the spear until the shaft stands upright with
   the body at its top, three metres up (`TargetOffset` beats; the `held` clip on the enemy).
3. 0.40 to 0.72: **the hold**. The spear upright, the bearer beneath it with both hands on the shaft and head
   bowed, the body above. The camera leans in and, being overhead, the lifted body comes **toward** it and grows
   on the screen. The element runs up the shaft from the hands to the head.
4. 0.72: the bearer steps back and brings the whole length over and down, forward, like the heavy. The real blow:
   the enemy is driven into the ground 3 m ahead with a ring of dust. The spear is left standing in it.
5. In the recovery the bearer walks to it and draws it out with one hand.

*Shared or new*: all new; step 4 reuses `attack_spear_heavy`'s poses. *First pass without E12*: thrust, plant the
butt, a held push with the enemy kneeling on the point, then wrench the spear out sideways through it. *Fixed
targets*: that first pass.

### Elements

- **Lightning**: the spear is the rod. The Flying Spear flies as a bolt (it arrives at once), and where it stands
  it calls one real bolt down on itself before the player follows. Thrusts lay Static; the sweep, being a heavy
  arc in all but name, can be made to discharge it. Upgrade, **Thunder Road** (Lightning, needs the Great Spear):
  the dash to a thrown spear shocks everything along its path, as Blink does.
- **Water**: the element of reach and flow. Each thrust throws a jet a metre and a half past the point (Soaked,
  half damage), so the lane is 5.5 m long; the couched charge rides a wave that carries small enemies with it.
  Upgrade, **Undertow** (Water, needs the Great Spear): the withdrawal of a thrust pulls a Soaked enemy half a
  metre toward the player, back onto the point for the next one.
- **Metal**: the phalanx. The surge makes the spear's guard as wide as the Katana's and every guard thrust lays
  Sundered. Upgrade, **Hedge of Spears** (Metal, needs the Great Spear): a perfect guard plants three spears of
  bright metal in the ground in front of the player, points out, for 4 s; what walks or charges onto them is hurt
  and stopped.

---

## 8. Great Hammer

**Idea.** A block of iron on a pole that does not cut, it lands: every blow falls on a spot on the ground a
stride ahead and what matters is the weight that arrives there. It asks you to **hit where the enemy will be**,
half a second ahead, and to **guard early**: the hammer is too slow to raise at the last instant, so its perfect
guard has to be read, not reacted.

**How it is not the Massive Sword** (GAME.md: "like the Massive Sword but blunt"). The sword sweeps wide and
speeds up; the hammer strikes one place and slows down. The sword's arcs start at the body, so standing close is
safe for it; the hammer's discs are out at the head, so an enemy hugging the player is **missed**. The sword is
damage across a crowd; the hammer is stagger on one target: two lights do 82 stagger, more than half of the Gate
Captain's balance. The sword's guard is a wall with a tight perfect window; the hammer's perfect window is as long
as any but begins late.

### Numbers

`Name = "Great Hammer"`, `Carry = "hammer"`, `SkillName = "Spinning Hammer Toss"`, `ChargeAnimation =
"charge_hammer"`, `SlashColour = (2.0, 1.6, 1.2, 0.9)`. `SkillManaCost = 35`, `ChargeTime = 0.55`,
`SkillChargeTime = 1.00`, `ChargeMoveFactor = 0.3`.

| slot | Name | Sweep | Windup | Active | Recovery | Damage | Stagger | StaminaCost | ManaGain | Lunge | Reach | ArcDegrees | HitStop | Shake | new |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Steps[0] | attack_hammer_1 | -1 | 0.30 | 0.12 | 0.50 | 20 | 34 | 20 | 8 | 0.7 | 3.0 | 150 | 0.09 | 0.12 | Poise 12 |
| Steps[1] | attack_hammer_2 | 0 | 0.40 | 0.14 | 0.62 | 26 | 48 | 24 | 10 | 0.6 | 2.5 | - | 0.12 | 0.22 | Radius 1.5, Poise 16 |
| Heavy | attack_hammer_heavy | 0 | 0.46 | 0.12 | 0.70 | 36 | 70 | 30 | 14 | 0.8 | 2.6 | - | 0.14 | 0.34 | Radius 2.0, Poise 22 |
| ChainHeavy | attack_hammer_chain | +1 | 0.26 | 0.12 | 0.64 | 34 | 64 | 26 | 14 | 0.6 | 3.0 | 110 | 0.14 | 0.32 | Pull -2.5, Poise 22 |
| ChargedHeavy | attack_hammer_charged | 0 | 0.14 | 0.14 | 0.80 | 52 | 100 | 36 | 16 | 0.6 | 2.6 | - | 0.18 | 0.50 | Radius 3.0, Poise 30 |
| Skill | attack_hammer_skill | +1 | 0.30 | 0.30 | 0.50 | 14 | 24 | 20 | 0 | 0 | 3.0 | 360 | 0.08 | 0.20 | Repeats {0.15}, Shot |
| Finisher | attack_hammer_finisher | +1 | 0.76 | 0.12 | 0.62 | 36 | 0 | 0 | 15 | 0 | 3.2 | 180 | 0.26 | 0.70 | Beats |

The chain is two blows: 46 damage and 82 stagger in 2.08 s for 44 stamina. The first comes in 0.30 s, the second
in 0.40 s: it gets slower, which is the opposite of everything else, and the player has to learn to wait for the
second. The overhead's disc covers 1.0 to 4.0 m ahead: inside one metre it misses.

**Spinning Hammer Toss.** The bearer spins twice with the hammer at arm's length (two blows all round, the step
in the table and its repeat), walking at 30% speed and able to steer, and lets go: `Shot` with `Speed 16`,
`Range 8`, `Radius 1.2`, `Damage 28`, `Stagger 50`, `Pierce` true, `Heavy` true, `Returns` true, `ReturnSpeed 20`,
`ReturnScale 0.5`. The hammer flies out turning end over end, flattens what it passes, hangs for an instant at
8 m, and comes back to the hand, striking again at half. Out and back takes 0.9 s, and for that time the bearer is
unarmed (no attacks; the guard is the fists' guard; the roll is free). Catching it is its own small beat: the
hand goes up, the arm is driven back by it. An enemy between the player and the far end is hit from behind on the
return.

**Engine work**: E1, E2 (props), E3 (`Radius`), E4, E5, E6, E8 (`Returns`), E9 (shove), E12, E13
(`PerfectFrom`).

### Guard and dodge

- Guard: `PerfectFrom 0.08`, `PerfectWindow 0.24`, `StaminaPerDamage 0.55`, `ChipFraction 0.2`,
  `ArcDegrees 180`, `GuardMoveFactor 0.3`. The window is 0.16 s wide, about the Katana's, but it **starts** 0.08 s
  after the press: a press in the last eighth of a second before the blow is only a block. The player must commit
  to the guard earlier than with any other weapon. The plain block is the best in the game (the head is a
  shield).
- Dodge: `Distance 3.6`, `Duration 0.42`, `StaminaCost 32`, `InvulnerableFrom 0.06`, `InvulnerableUntil 0.25`,
  `Recovery 0.18`. The worst roll in the game. `MoveFactor 0.9`. The hammer's answer to danger is poise and the
  block, not the roll.

### Model

1.65 m in all. **Haft** 1.5 m x 0.05, dark wood (`leather`) with four `iron` bands and a `wrap` grip at the lower
third; an iron cap at the butt. **Head** 0.42 m long, 0.26 x 0.26 in section, `iron`, its long edges bevelled
0.03; one face flat with a raised round boss 0.16 across (the striking face), the other drawn out to a blunt
wedge. A band 0.05 wide round the middle of the head and the boss itself are the **inlay**. A collar under the
head, with rope (`straw`) wound below it and a knot of `accent` red. About 150 triangles. From above the head is a
fat dark rectangle, unmistakable beside the sword's long bar. **Hands**: right 0.35 m under the head for carrying
and 0.9 m under it for swinging (the hand slides down in each wind-up), left at the butt (`GripGap` 0.5 to 0.7).
**Carry** (`Stowed` false): on the right shoulder, head behind. `socket_back` for rest, head up.

### Animation

All new. The Massive Sword's clips must not be reused, whatever the temptation: that the two feel different is the
whole design. The difference in the body: the sword is swung from the hips with long arms; the hammer is **lifted
with the legs and dropped**, and after each blow it stays where it fell for a moment while the body recovers round
it.

| clip | seconds | what it is |
|---|---|---|
| idle_hammer | 3.0 loop | The head resting on the ground in front of the right foot, the haft upright-ish, both hands folded on the butt at belt height, feet wide. A workman leaning on his tool. Breathing only. From above: a dark block on the ground just ahead of the player. |
| run_hammer | 0.64 loop | The slowest run. Hammer on the right shoulder, right hand under the head, left hand on the butt in front of the chest; the body leans left to carry it; the head of the hammer rocks with each stride. |
| roll_hammer | 0.42 | The hammer is thrown ahead head-first along the ground and the bearer dives after it over the left shoulder, coming up with a hand already on the haft. |
| guard_hammer | 0.20 | The haft is swung up to stand before the body, head uppermost at face height with its flat face to the enemy, both forearms braced behind the haft, knees deep. Slow to arrive: this clip is 0.20 s where the Katana's is 0.14 s, and the perfect window opens as it ends. |
| guard_hit_hammer | 0.22 | Almost nothing: the head rings (a 2 cm shudder), the bearer does not move. That stillness is the reward for the block. |
| guard_perfect_hammer | 0.30 | The head is shoved forward into the blow, a short flat punch with the whole haft, and the attacker's weapon is knocked upward; the hammer finishes drawn back to the right hip, low, ready for the side swing. |
| charge_hammer | to ChargeTime, then held | Hands slide to the butt; the hammer is dragged back along the ground behind the right heel, then heaved up and over until it hangs behind the back, haft down the spine, the body arched and up on the toes. At the second tier the head is let swing down and out to the right at arm's length and the first slow turn begins. |

**attack_hammer_1 (the side swing).** *Anticipation*: the right hand slides down the haft as the head is swung
back to the right at knee height; the body turns right 50 degrees and the left foot comes off the ground; held.
*Contact*: the left foot stamps down and the hammer comes round level across the front, arms long; the head
arrives at 3.0 m and **stops** against what it hits. (Set `Sweep` to match the side the finished clip swings
from; the table's -1 assumes the wind-up ends on the bearer's left, so mirror the wind-up above or flip the sign.) *Follow-through*: unlike a blade it does not pass through: the head rebounds a
hand's breadth and drops to the ground on the left; the bearer's body carries on a quarter turn past it and is
pulled up short by its weight. *From above*: a thick, short arc, 150 degrees, that ends in a burst and not a
tail.

**attack_hammer_2 (the drop).** *Anticipation*: from where the head lies, both arms and the back heave it
straight up overhead; the bearer goes up on the toes, the body a straight line from heels to hammer; a long held
beat (this is the 0.40 s). *Contact*: the knees are simply let go: the bearer drops and the head comes down with
the whole body's weight onto a spot 2.5 m ahead. *Follow-through*: the bearer ends in a deep squat with the arms
straight down the haft; the head sits in a cracked circle on the ground; dust; then the slow stand. *From above*:
a disc 3 m across stamped on the ground ahead, a ring of dust running out of it.

**attack_hammer_heavy ("Quake").** The drop again, bigger. *Anticipation*: a step back first, hammer dragged; then
a run-up of one stride as it is swung up and over behind (the wind-up goes away from the enemy before it goes
toward it). *Contact*: a small jump, both feet off the ground, and the landing and the blow are the same instant.
*Follow-through*: the bearer is thrown forward over the haft by the stop and hangs on it. *From above*: a 4 m
disc, a double ring.

**attack_hammer_chain (the lift).** *Anticipation*: the head, lying on the ground, is slid back between the
feet. *Contact*: an upward swing from the ground through the target and on over the left shoulder: a golfer's
drive; the target is thrown 2.5 m back and briefly off its feet. *Follow-through*: the hammer goes all the way
round behind the head and its weight turns the bearer half round.

**attack_hammer_charged ("Earthshaker").** From `charge_hammer`, with the hammer hanging behind the back.
*Contact*: the body snaps from arched back to folded forward and the head comes over from behind in a full half
circle: it lands 0.14 s after release. *Follow-through*: the ground under a 6 m disc jumps (a second, pale ring
runs out past the first, seen only); the bearer is on one knee with the forehead on the hands. The longest
recovery of any attack in this document.

**attack_hammer_skill (Spinning Hammer Toss).** *Anticipation*: the turn that `charge_hammer` began speeds up:
the bearer leans back against the pull, arms straight, heels dug in, pivoting on the left foot; the scarf stands
straight out. Two turns. *Contact*: release at the top of the second turn, both hands flung open toward the
target; the body staggers two steps the other way. *Follow-through*: the bearer catches their balance, turns,
plants the feet and raises the right hand; the hammer arrives into it and drives the arm back to the shoulder.
*From above*: two rings round the player, then a spinning block that leaves, stops, and returns.

**attack_hammer_finisher ("The Bell").** The moment is **a turned back and a long wait**.

1. 0.00 to 0.16: step in; the lift (the chain heavy's swing), under the kneeling enemy's chin. Beat: hit stop
   0.06, a hard shake. The enemy goes **up**: 4 m, toward the camera, growing on the screen, turning slowly.
2. 0.16 to 0.56: **the hold**. The bearer does not watch. They turn their back on where it will land, let the
   hammer's head down onto the ground, roll the right shoulder once, and wind up for the side swing, slowly: the
   hammer drawn back level on the right, the body coiled, perfectly still from 0.40. The camera leans in on the
   bearer. A shadow on the ground beside them grows.
3. 0.76: as the enemy falls back to chest height the bearer turns into it with the side swing and meets it in the
   air. The real blow. The longest stop in the game (0.26 s), then the enemy leaves **sideways**, flat and fast,
   8 m, and skids (`TargetOffset` after the blow; if a wall is in the way it stops there with a second burst).
4. In the recovery the hammer's head drops to the ground and the bearer leans on it.

*Shared or new*: all new; reuses the chain heavy's and the first light's poses. *First pass without E12*: the
enemy stays kneeling: the same slow wind-up with the back half-turned, the same hold, the side swing into it, and
the existing finisher shove (`Enemy.Shove` times 2.2) sends it back. *Fixed targets*: that first pass.

### Elements

- **Earth**: the hammer's own. Every disc leaves Tremor in the ground it struck for 2 s (enemies there are slowed
  and their balance does not come back), and the charged blow throws up a ring of stone teeth at its edge that
  stands for a moment. Upgrade, **Aftershock** (Earth, needs the Great Hammer): each drop is followed half a
  second later by a second ring one and a half times as wide, for half the stagger.
- **Ice**: the breaker. A Frozen enemy shatters under any heavy blow, and the hammer has nothing but heavy blows:
  even its lights shatter. The toss lays Chill out and back. Upgrade, **Icebreaker** (Ice, needs the Great
  Hammer): a shattered enemy's shards fly out along the swing's own direction as a cone and Chill what they
  strike.
- **Lightning**: the contrast, a slow weapon given the quick element. Overdrive takes the hammer's wind-ups from
  0.30 and 0.40 s to 0.23 and 0.31 s, which is where it stops being a weapon you wait for. Static builds slowly
  (two lights) but the hammer's heavy discharges it over the whole disc. Upgrade, **Thunderhead** (Lightning,
  needs the Great Hammer): while the hammer is out of the hand on its toss a bolt joins it to the bearer, and what
  the bolt crosses is struck.

---

## 9. Full Body Armament

**What it is** (decided in GAME.md): "not a weapon pickup. It is an upgrade that appears in the element tree while
no weapon is equipped." So it is what **Fists become**: the held element coats the forearms, fists, shins and
feet, and bare hands turn from the starting weapon into a bruiser's whole style.

**Idea.** The fist chain, but each place you stop it has its own ending, and a stance that takes a blow and gives
it back. It asks you to **choose the ender for the moment** (shove, launch, slam or blast) and to **stand in the
enemy's attack on purpose**: Energy Reflect is the only skill in the game that needs the enemy to hit you.

**How it changes play.** Fists today are one chain and one skill. The Armament has the same four quick blows and
then a decision every time: one light and a ram to make room, two and the rising blow to break a guard, three and
the slam for the crowd, all four and the burst to spend the element. It has no reach at all, and the player lives
inside the enemy's arms, which is where Energy Reflect is strongest.

### How it is learned and worn

- Every element's tree carries one mark, **Armament** (a major mark, cost 3, on the root's first ring so it is
  reachable early). It is drawn in the tree only while `RunState.Weapon` is `Fists` or the Armament itself; with a
  weapon in hand it is not shown and cannot be bought.
- Learning it replaces `Weapons.Fists()` with `Weapons.Armament(element)` at once. Picking a weapon up later puts
  the Armament to sleep (the mark stays learned and greyed); dropping the weapon wakes it. Taking a new element
  from a god forgets the old tree as now, so the Armament is lost with it until the new tree's own mark is bought;
  consuming keeps it.
- Each element changes it (below), so there are in effect eleven Armaments on one frame. **Open for the user**:
  whether a god's "special weapon" for a bare-handed player who takes the element should be that element's
  Armament already learned (it would answer what a fist build gets from Take).

### Numbers

`Name = "Armament"` (shown as the element's name and "Armament": "Fire Armament"), `Carry = "arm"`,
`SkillName = "Energy Reflect"`, `ChargeAnimation = "charge_arm"`, `SlashColour` is the element's colour at 2x.
`SkillManaCost = 25`, `ChargeTime = 0.30`, `SkillIsCounter = true`, `CounterHoldMax = 1.2`.

| slot | Name | Sweep | Windup | Active | Recovery | Damage | Stagger | StaminaCost | ManaGain | Lunge | Reach | ArcDegrees | HitStop | Shake | new |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Steps[0] | attack_arm_1 | 0 | 0.07 | 0.06 | 0.17 | 8 | 10 | 7 | 4 | 0.5 | 2.1 | 90 | 0.035 | 0 | |
| Steps[1] | attack_arm_2 | 0 | 0.08 | 0.06 | 0.18 | 9 | 11 | 7 | 4 | 0.5 | 2.1 | 90 | 0.035 | 0 | |
| Steps[2] | attack_arm_3 | +1 | 0.11 | 0.07 | 0.22 | 11 | 15 | 9 | 5 | 0.6 | 2.2 | 150 | 0.05 | 0 | |
| Steps[3] | attack_arm_4 | -1 | 0.15 | 0.09 | 0.36 | 16 | 26 | 12 | 8 | 1.1 | 2.5 | 150 | 0.08 | 0.10 | |
| Heavy | attack_arm_heavy | 0 | 0.24 | 0.09 | 0.40 | 22 | 36 | 16 | 9 | 1.8 | 2.3 | 80 | 0.09 | 0.16 | Poise 16 |
| ChainHeavies[1] | attack_arm_ram | 0 | 0.12 | 0.14 | 0.36 | 14 | 30 | 14 | 8 | 0 | 1.6 | 120 | 0.08 | 0.16 | Travel 2.2, Pull -2.5, Poise 22 |
| ChainHeavies[2] | attack_arm_rising | 0 | 0.15 | 0.10 | 0.44 | 22 | 46 | 16 | 11 | 0.8 | 2.2 | 100 | 0.10 | 0.18 | |
| ChainHeavies[3] | attack_arm_slam | 0 | 0.20 | 0.10 | 0.46 | 20 | 34 | 16 | 10 | 0.3 | 0.8 | - | 0.10 | 0.24 | Radius 2.2 |
| ChainHeavies[4] | attack_arm_burst | 0 | 0.18 | 0.12 | 0.52 | 28 | 40 | 18 | 12 | 0.2 | 4.2 | 70 | 0.12 | 0.28 | lays 2 stacks |
| ChargedHeavy | attack_arm_charged | 0 | 0.10 | 0.12 | 0.50 | 28 | 52 | 18 | 10 | 2.6 | 2.5 | 90 | 0.13 | 0.28 | Poise 26 |
| Skill | attack_arm_skill | 0 | 0.04 | 0.12 | 0.44 | 20 | 40 | 0 | 0 | 0 | 5.0 | 90 | 0.16 | 0.40 | counter; see below |
| Finisher | attack_arm_finisher | 0 | 0.72 | 0.12 | 0.56 | 26 | 0 | 0 | 15 | 0 | 2.8 | 120 | 0.22 | 0.55 | Beats |

The four lights keep the fists' timing to the hundredth (1.62 s, 35 stamina), with 44 damage and 62 stagger where
fists have 36 and 57, 0.3 m more reach (the coat stands off the fist), and one stack of the element's affliction a
blow. The Armament is the fists' rhythm on purpose: it replaces them, and the hands the player has learned must
still work. What is new is everything after the lights.

**The four enders** (`ChainHeavies`, E10; index is how many lights came before):

1. **Ram** (after one light): a shoulder charge of 2.2 m through a blow of 22 or less, throwing the target 2.5 m
   back. Makes room, breaks a ring of enemies, puts a soldier against a wall.
2. **Rising blow** (after two): the fists' rising heavy with more in it. The most stagger for its time in the
   kit: the guard-breaker.
3. **Slam** (after three): both fists down on the ground: a disc of 2.2 m round a point just ahead of the feet.
   The crowd ender.
4. **Burst** (after all four): both palms thrown forward and the coat **leaves the arms** as a cone 4.2 m long,
   laying two stacks of the element's affliction on all it touches. For half a second afterwards the arms are bare
   and dark, then the coat creeps back from the elbows. The one ranged thing the Armament has, and it must be
   earned with a full chain.

**Meteor Fist** (`ChargedHeavy`): hold the heavy 0.3 s and let go. The fists' Breaker made free and a little
lighter (28 damage and 52 stagger against Breaker's 30 and 75 for 30 mana), with poise.

**Energy Reflect** (the skill, a counter; E5). While the heavy is held past `ChargeTime` the bearer stands
**braced** (`charge_arm`), the coat blazing. A blow that lands in that state, from any side, guardable or red,
thrown or struck by hand, is taken on the coat: no damage, 25 mana spent, and the Skill step fires at once toward
whoever struck: a cone 5 m long and 90 degrees wide in the element's colour for 20 damage plus the whole damage of
the blow that was taken, and 40 stagger plus what a perfect guard would have thrown back (55 on the castle's
enemies). A bolt or other thrown attack is answered along the line it came from. Braced costs nothing to hold but
lasts 1.2 s at most, then becomes Meteor Fist by itself; with less than 25 mana the stance still braces but the
blow lands as on an unguarded player, so the mana is the whole wager. It is a perfect guard that is easier to time
(a 0.9 s window in place of 0.18 s), costs mana where that gives mana, cannot be held for ever, and hits back
harder the harder the enemy hits.

**Engine work**: E1, E3 (`Radius`), E5 (the counter), E6, E7 (`Travel`), E9 (shove), E10 (`ChainHeavies`), E12,
E13, and E14: the coat and the swap rule above. No shots, no props, no new weapon model: the cheapest of the
eight to make playable.

### Guard and dodge

- Guard: `PerfectWindow 0.18`, `StaminaPerDamage 0.8`, `ChipFraction 0.2`, `ArcDegrees 200`. The fists' crossed
  forearms, but the coat takes the edge off: less chip and less stamina than bare arms.
- Dodge: the defaults. (Elements change it; see below.)

### Model

No weapon object. The **coat** (`ArmamentBuilder`, E14) is a set of rigid shells parented to bones and shown only
while the Armament is worn: `forearm.L/R` (a sleeve 0.30 m long standing 0.02 m off the arm), `hand.L/R` (a
gauntlet 1.3 times the hand, so the fists read larger from above), `shin.L/R` and `foot.L/R` (greaves, smaller),
and two low flares on `shoulder.L/R`. One material, `m_armament_<element>`: the element's colour as an emissive in
two flat tones with a hard line between them (the way fire is drawn here), with the characters' ink outline. The
**shape** of the shells is the element's, from three or four primitives each, so that every Armament has its own
silhouette: Fire, tongues of flame licking back from the knuckles toward the elbow; Lightning, thin jagged
spines that jump between two positions every few frames; Earth, slabs of stone, the gauntlet twice the size of the
hand; Ice, long clear crystals with three claws over the fingers; Metal, plain riveted plates; Wind, loose rings
turning round the forearm; and so on, one sheet of shapes per element as each tree is built. About 200 triangles
for a full coat. First pass: the same four boxes for every element, tinted.

### Animation

The **most shared** set of the eight. First pass: the four lights, the heavy, the rising ender, the guard and the
locomotion are the fists' own clips under new names (`Jab`, `Cross`, `Hook`, `FrontKick`, `Overhand`,
`Uppercut`, `FistGuard`, `IdleFist`, `RunFist` baked a second time with the `arm` names, so the two sets can part
later). New and needed for it to feel like itself: the ram, the slam, the burst, the braced stance, the reflect,
Meteor Fist's leap, and the finisher. Second pass: re-pose the lights heavier (lower hips, wider feet, each blow
driven 10 cm further through) so the Armament does not look like the starting weapon with paint on.

| clip | seconds | first pass |
|---|---|---|
| idle_arm, run_arm | 1.3 loop, 0.52 loop | `IdleFist`, `RunFist`. Second pass: the idle lower and stiller (a bruiser does not bounce), hands open and held wider. |
| guard_arm | 0.12 | `FistGuard`. |
| guard_hit_arm | 0.22 | New (and give it to Fists): the crossed forearms are driven back to the brow and the body slides half a foot. |
| guard_perfect_arm | 0.30 | New (and give it to Fists): the cross is thrown open outward, both arms flung wide and down, chest out, a stamp of the lead foot: the blow is burst apart, not turned. |
| charge_arm | to ChargeTime, then held | New: **braced**. Feet wide and square to the enemy, knees bent, both fists drawn down to the hips with the elbows back, chest lifted, chin down: an open, unguarded front, daring the blow. Held: the shoulders rise and fall with one slow breath. |
| attack_arm_1 to 4, attack_arm_heavy, attack_arm_rising | as the table | The fists' clips. |

**attack_arm_ram.** *Anticipation*: the lead (left) shoulder dips and turns in, the arm folded across the chest
with the fist under the chin; the back leg loads. *Contact*: a flat drive of 2.2 m, shoulder and coated forearm
leading, body at 45 degrees, the back leg trailing straight. *Follow-through*: the folded arm is thrown open
outward at the end (that is the shove), and the bearer stops square, arms low. *From above*: a short lane with a
burst at its end and the target sliding away.

**attack_arm_slam.** *Anticipation*: both fists go up together over the head, the back arched, up on the toes.
*Contact*: both come down side by side onto the ground just ahead of the feet as the knees fold.
*Follow-through*: crouched over the fists, head down, a ring of the element running out along the ground to
2.2 m; the fists are pulled out of two small craters. *From above*: a ring round the player.

**attack_arm_burst.** *Anticipation*: both hands are drawn back to the right hip, wrists together, one palm up
and one down, the body turned side-on and sunk; the coat runs down the arms and gathers between the palms,
brightening (the sleeves thin as it goes). *Contact*: a stride and both palms are thrust out, heels of the hands
together, fingers spread. *Follow-through*: the arms stay out, bare and dark, while the cone travels; the fingers
close slowly and the coat grows back from the elbows to the knuckles over the recovery. *From above*: a 70 degree
cone of pure colour up the screen from the hands.

**attack_arm_charged (Meteor Fist).** From `charge_arm`. *Contact*: a leap of 2.6 m with the right fist drawn
back past the ear, the left hand out in front pointing at the target, landing with the punch driven **down**
through it at 45 degrees. *Follow-through*: one knee and the fist on the ground, the left arm out for balance.

**attack_arm_skill (Energy Reflect).** There is no anticipation: the braced stance is it, and the enemy's blow is
the cue. *Contact* (0.04 s after the blow): the chest takes it visibly: the body bows back a hand's breadth round
the point of impact while the coat flares white there; and in the same motion both fists come up from the hips
and are punched out and **open**, arms wide, chest thrown forward, head back: a roar with the whole body.
*Follow-through*: the arms are held wide for a third of the recovery as the cone goes out, then drop. *From
above*: a white flash on the player, then a cone thrown back along the line the blow came in on.

**attack_arm_finisher ("One Inch").** The moment is **six fast blows and then no blow at all**.

1. 0.08 to 0.54: six blows on the kneeling enemy, 0.09 s apart, each a beat (hit stop 0.03, a small spark in the
   element's colour, no damage): left to the ribs, right to the ribs, left hook to the head, right hook, a knee,
   a rising elbow. Each drives the enemy back a little and the bearer steps in after it (`TargetOffset` 0.15 m a
   blow). The rhythm is even and fast: it should sound, when there is sound, like a drum roll.
2. 0.54 to 0.72: **the hold**. Silence. The right hand is opened and laid flat and still against the enemy's
   chest, the arm bent, the left fist drawn back to the hip. The camera leans in. The coat drains out of the whole
   body, legs first, and gathers into that one hand until it is the only bright thing on the screen.
3. 0.72: the hand does not draw back. The hips turn and the arm straightens by the width of the palm. The real
   blow. On the stop, the element goes **through**: a cone of it bursts out of the enemy's back, 4 m long, and
   strikes what stands behind (seen, and it lays the element's affliction).
4. In the recovery the hand stays where it is as the enemy falls away from it, then closes.

*Shared or new*: new; the six blows are the existing fist poses run at double speed. *First pass without E12*: the
enemy is not driven back; everything else is as written. *Different from the fists' palm strike*, which lands and
opens wide at once: here the blow is the stillness before it.

### Elements

GAME.md names the three it suits best: "lightning as a speed fighter, earth as a slow armoured bruiser, ice".
Since the Armament is an element's own mark, each element should bend it; these three first.

- **Lightning, the speed fighter.** The four lights take 15% less time each and the dodge becomes a Blink-length
  step that can be taken out of any light's recovery for half stamina. The Ram is a step-through: the bearer
  appears behind the target and the shove goes the other way. Energy Reflect answers with a bolt that leaps to two
  more. Upgrade, **Hundred Hands** (Lightning, needs the Armament): holding the light button after the fourth blow
  keeps the fists going, six blows a second for 4 stamina each, each laying Static, until the button or the
  stamina gives out.
- **Earth, the slow armoured bruiser.** The opposite trade: every blow has poise (12 on the lights, 30 on the
  enders), the bearer takes a fifth less damage, and the price is that the roll becomes a short heavy step
  (the hammer's roll). The Slam's disc is 3.2 m and leaves Tremor. Braced can be held for 2 s. Upgrade,
  **Bedrock** (Earth, needs the Armament): while braced, blows from in front are plainly blocked for no stamina
  even when there is no mana for the Reflect.
- **Ice, the counter-fighter.** Every blow lays Chill, and the Armament throws more blows than anything but the
  knives, so an enemy is Frozen within a chain and a half; the Rising blow shatters. Energy Reflect freezes the
  attacker solid for 1.5 s where other elements only burn or shock it. The Burst leaves a fan of ice on the
  ground that Chills what crosses it. Upgrade, **Cold Hands** (Ice, needs the Armament): the hold of a perfect
  guard: a blow turned by a perfect guard with the Armament leaves the attacker's weapon arm frozen, and its next
  attack is 40% slower to come.

For the other eight, in a line each, to be designed with their trees: Fire, the Burst detonates every burn it
touches and the fists leave Heat faster; Wind, the enders push twice as far and the Ram carries the bearer 4 m;
Water, flasks drunk while the coat is on also refill it, and the Burst is a wave that soaks; Life, blows on Seeded
enemies heal a point each, a bruiser who mends in the pocket; Metal, the plain block costs half and the coat's
guard is as wide as the sword's; Light, the Reflect marks all it strikes; Dark, the coat feeds: Meteor Fist heals
for a share of its damage; Poison, each ender spends an enemy's poison stacks as a burst.

---

## 10. What to build, in what order

### Cost of each weapon in new engine work

| Weapon | Shared items | Its own | New clips | Weight |
|---|---|---|---|---|
| Full Body Armament | E1, E3 (disc), E5 (counter), E6, E7, E9, E10 | the coat; the swap rule | 7 new, 9 from Fists | **lightest** |
| Massive Sword | E1, E2, E5 (tiers), E6 | a shared bitten list for the spike line | 14 | light |
| Scythe | E1, E2, E3 (bands), E7 (check only), E8 (a moving and a hanging zone: nearly there in `Hazards`) | none | 13 | light |
| Great Hammer | E1, E2, E3 (disc), E4, E5, E6, E8 (returning), E9, E13 (`PerfectFrom`) | none | 15 | middling |
| Great Spear | E1, E2, E3 (lane), E5, E6, E7, E8 (sticking), E9, E10 (guard attack) | the follow dash | 15 | middling |
| Twin Knives | E1, E2 (left hand), E4, E7 (pass through), E8 (fan), E10 (dash attack), E11 | none | 15 | middling |
| Great Bow | E1, E2 (left hand, string, props), E5 (walk while charging), E7, E8 (sticking) | planted arrows on enemies and ground, snapping, the quiver | 13 | heavy |
| Chain Sickle | E1, E2 (props), E3 (lane), E4, E9 (drag, weights, pull self) | the drawn chain in every clip; Bound on every enemy kind | 14 | **heaviest** |

E12 (staged finishers) and E13 (guard reactions) are left out of the table: every weapon wants them, each
finisher has a first pass that needs neither, and they are best built once after the first three weapons exist to
test them on.

### The three to build first

Most new play for the least new engine work, in this order:

1. **Massive Sword.** Needs only the weapon profile (E1), sockets (E2), the second charge tier (E5) and poise
   (E6), and its skill is eight calls to a function that exists. It is the furthest thing from the Katana that can
   be made on the present code: slow against quick, trading against evading, a crowd against a duel. Building it
   first forces E1 and E2, which all the others stand on. Its fourteen clips are all new but all of one kind
   (a two-handed blade, which `ForgePose` already aims).
2. **Scythe.** One real rule, the distance bands (E3), which is a dozen lines in `Strike` and a change to how the
   arc is drawn, and it turns spacing into the game. The Crescent is two `Hazards.Lay` calls for a first pass. It
   proves E3, which the spear, the hammer and the chain all need.
3. **Full Body Armament.** The least art (no model, nine clips borrowed), it is the weapon that most directly
   serves the day's other work (the element trees: it **is** a tree mark), it makes the first run's bare hands
   worth staying with, and its engine pieces (`ChainHeavies`, the counter, the shove) are small and each is wanted
   again by other weapons. It could well go first; it is third only because E1 and E3 should exist before it.

Then, in order of what they add for what they cost: **Great Spear** (reuses E3's lane and E5; the guard thrust
and the follow dash are its only new machinery, and the enemies' spear poses give its animation a start),
**Great Hammer** (by then only the returning shot and `PerfectFrom` are new), **Twin Knives** (the left-hand aim,
the dash attack and the back rule; quick to animate because the clips are short), **Great Bow**, and last the
**Chain Sickle**.

### The most expensive, and why

**The Chain Sickle.** Three reasons, all of them the chain.

1. **It has to be drawn in every frame of every clip.** Every other weapon is a rigid thing in a hand, which
   `ForgePose` already aims. The chain is a line of changing length between a hand and a weight that flies, wraps,
   whips and returns; it needs its own renderer (`ChainLine`), a keyed path for the far end in all fourteen clips
   (the props of E2), and it will look wrong before it looks right. A stiff or floating chain ruins the weapon,
   and the user's bar for animation is the highest bar in the project.
2. **It moves enemies, and every enemy must agree.** Dragging (E9) touches `Enemy`'s movement, the ground an
   elite keeps, the turn-taking ring, the colossus's hands and the god, each of which needs its own answer to
   "what happens when this is pulled". Bound is a new state in every enemy's brain, and must be made fair against
   bosses without being useless against them.
3. **Its finisher is the one that most needs the target puppeted (E12)**; its first pass is the weakest of the
   eight.

The Great Bow is the second most expensive, for a different reason: not animation but bookkeeping (arrows that
live on enemies and in the ground, with counts, lifetimes and bursts), and the risk that it tempts the player
away from close combat if the snap is not tuned to be plainly the better half.

---

## Appendix A. New fields, in one place

Everything used in the tables above that is not in the code today.

| Where | Field | Type, default | Meaning | Item |
|---|---|---|---|---|
| `Weapon` | `Carry` | string, "" | which objects on the character are this weapon | E1 |
| `Weapon` | `LocomotionState`, `GuardState`, `RollState`, `GuardHitState`, `PerfectGuardState` | string | animation states | E1, E13 |
| `Weapon` | `GuardProfile`, `DodgeProfile` | `Guard`, `DodgeRoll`, null | numbers copied on equip | E1 |
| `Weapon` | `MoveFactor`, `GuardMoveFactor` | float, 1 and 0.45 | share of `MoveSpeed` | E1 |
| `Weapon` | `Stowed` | bool, false | leaves the hand when idle (Katana only) | E1 |
| `Weapon` | `BackMultiplier` | float, 1 | damage and stagger from behind | E11 |
| `Weapon` | `PerfectGuardRider` | small class, null | one extra effect of a perfect guard | E13 |
| `AttackStep` | `LineWidth` | float, 0 | above zero: a lane `Reach` long | E3 |
| `AttackStep` | `Radius` | float, 0 | above zero: a disc centred `Reach` ahead | E3 |
| `AttackStep` | `InnerReach`, `InnerDamage` | float, 0 and 1 | weak band near the body | E3 |
| `AttackStep` | `SweetFrom`, `SweetDamage`, `SweetStagger` | float, 0, 1, 1 | strong band at the tip | E3 |
| `AttackStep` | `Repeats`, `RepeatScale` | float[], empty; float, 1 | more blows in one step | E4 |
| `AttackStep` | `Poise` | float, 0 | blows at or under this do not interrupt | E6 |
| `AttackStep` | `LungeSeconds` | float, 0 | the step-in ends by then | E7 |
| `AttackStep` | `Travel`, `PassThrough` | float, 0; bool | moving during `Active` | E7 |
| `AttackStep` | `Recoil` | float, 0 | pushed back after the blow | E7 |
| `AttackStep` | `Shot` | class, null | `Speed`, `Range`, `Radius`, `Damage`, `Stagger`, `Count`, `SpreadDegrees`, `Pierce`, `Sticks`, `Returns`, `ReturnSpeed`, `ReturnScale`, `HangSeconds`, `HangBite`, `HangEvery`, `Heavy`, `Prop` | E8 |
| `AttackStep` | `Pull`, `PullSelf` | float, 0; bool | drag the target, or the player | E9 |
| `AttackStep` | `Beats`, `HoldAt`, `HoldSeconds` | list; float | finisher staging | E12 |
| `WeaponCombo` | `ChargedHeavy`, `SkillChargeTime` | `AttackStep`, null; float | second charge tier | E5 |
| `WeaponCombo` | `ChargeMoveFactor` | float, 0 | walk and turn while charging | E5 |
| `WeaponCombo` | `SkillIsCounter`, `CounterHoldMax` | bool; float | the counter skill | E5 |
| `WeaponCombo` | `ChainHeavies` | list of `AttackStep` | ender by lights thrown | E10 |
| `WeaponCombo` | `DashAttack`, `GuardAttack` | `AttackStep`, null | other ways in | E10 |
| `WeaponCombo` | `FollowStep`, `FollowDelay`, `FollowSeconds` | `AttackStep`; float | the dash to a thrown spear | Spear |
| `Guard` | `PerfectFrom` | float, 0 | the perfect window opens late | E13 |
| `ForgePose` | `AimBladeL`, `BladeL`, `BladeLRoll` | as the right hand's | aim the left hand's weapon | E2 |
| `ForgePose` | `PropA`, `PropAAim`, `PropAHeld` (and B) | Vector3, Vector3, bool | things that leave the hand | E2 |
| `EnemyKind` | `Weight` | int, 0 | light, heavy, fixed | E9 |
| `Enemy` | `Drag(...)` via `IMovable`; `Bound`; `held` clip | | pulled, bound, lifted | E9, E14, E12 |
| new | `WeaponBuilder`, `ArmamentBuilder`, `ChainLine`, `PlantedArrows`, `HazardKind.Spike` | | | E2, E14 |

Existing fields used with a new kind of value: `AttackStep.Lunge` negative (Scythe heavy, Fan of Knives);
`ForgePose.GripGap` negative (Spear).

## Appendix B. Checks made on this document

- **Field names**: every column of every table is a field of `AttackStep` as it stood on 2026-11-16 (`Name`,
  `Sweep`, `Windup`, `Active`, `Recovery`, `Damage`, `Stagger`, `StaminaCost`, `ManaGain`, `Lunge`, `Reach`,
  `ArcDegrees`, `HitStopSeconds`, `Shake`, `Heavy`); every slot is a member of `WeaponCombo` (`Steps`, `Heavy`,
  `ChainHeavy`, `Skill`, `SkillManaCost`, `Finisher`, `ChargeTime`) or is in Appendix A; guard and dodge numbers
  are members of `Guard` (`PerfectWindow`, `StaminaPerDamage`, `ChipFraction`, `ArcDegrees`) and `DodgeRoll`
  (`Distance`, `Duration`, `StaminaCost`, `InvulnerableFrom`, `InvulnerableUntil`, `Recovery`) or are in Appendix A.
- **Timings**: light wind-ups run from 0.06 s (knives; fists are 0.07) to 0.40 s (the hammer's second blow); tapped
  heavy wind-ups from 0.18 to 0.50 s (the Katana's is 0.30); nothing but a finisher winds up for longer than
  0.50 s, and the charged blows land 0.06 to 0.14 s after release like the Iaido Slash (0.08). The longest plain
  attack is 1.30 s (the sword's heavy; the Katana's is 0.86) and its 0.66 s of recovery can be rolled out of.
  Finisher wind-ups are 0.62 to 0.80 s against the Katana's 0.30, because the staging happens inside them; blows
  pass through the player for all of it, as now, and the target is held (E12). Without E12 the staggered enemy's
  three seconds still cover the longest of them.
- **Stamina**: no light chain costs more than 64 of 100 (the sword); no single attack more than 36.
- **Rhythms**: the table in section 0. The two pairs most at risk of feeling alike were looked at again: sword and
  hammer (three accelerating sweeps from the body against two decelerating drops onto a spot), scythe and spear
  (long, short, long arcs against an even count of lanes).
- **Not checked, because this was a paper task**: nothing here was compiled, baked or played. Whether a negative
  `Lunge` moves the player back as expected, whether `weapon.R` exists on the player's rig for `RestBlade`, and
  whether a 1.95 m sword clears the hat and scarf on the back are the first three things to look at in the editor.
