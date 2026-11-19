# Steel and Element: how to build it

The plan for building everything in `docs/design/STEEL_AND_ELEMENT.md` and this folder, written for the workers who
build Phases 1 to 6 of the combat playtest (`docs/GAME.md`, "The evolving combat playtest"), so that each builds into
one architecture instead of inventing one. Written by Claude on 2026-11-18 against the code at `f790325` plus the
Phase 0 work then in progress (uncommitted under `Scripts/`). It names classes, files and numbers; every number is
the design's first guess, kept in one place so play can tune it.

**How sure this is.** What exists was read in the code: the mark catalogue (`Upgrade`, `UpgradeTree`, `MarkRules`,
`PlayerUpgrades.Refresh`, the effect runner's triggers, actions and conditions), `PlayerElement`, `ElementKit`,
`ManaPool`, `AfflictionSet`, `CombatEvents`, `Hit`, the `TakeHit` paths of every afflictable thing, `PlayerCombat`'s
blow path, `WeaponCombo`, `Weapon`, `AttackStep`, `ControlBindings`, `Controls.inputactions`, `Progress`, `RunState`,
`PlaytestMode`, `RestPoint`, `GodReward`, `Gifts`, the UI classes' headers. What was **not** read closely, and should be
checked by the task that touches it: `TapForm` and the forms' internals, `Weapons.cs` move by move, `ControlsPage`,
`PlayerInteraction`, `EnemyBrain`'s awareness, the animator and the Forge, `HubBuilder` and `ProvingGroundBuilder`, the
directors of the god fights, and the pair files and `elements_b.md` beyond their shared rules and tables. Nothing here
has been compiled or played.

Contents: 0 the architecture in one page; 1 what exists and what changes; 2 the data model; 3 the runtime systems;
4 the UI; 5 saves; 6 testing; 7 the build plan; 8 risks and open technical questions.

---

## 0. The architecture in one page

1. **One catalogue of marks.** Every bought, found or given thing (element marks, spell branches, duo branches,
   reactions, the Way, the Body, Heresies, Evolutions, Bond marks) is an `Upgrade` in `Upgrade.All`, written as C#
   object initialisers as the trees are now, gathered by `UpgradeTrees.AddAll`, and run by the one effect runner in
   `PlayerUpgrades`. New fields say which tree and currency a mark belongs to and when it is in force. There is no
   second system for weapons or reactions.
2. **One loadout decides what is in force.** A plain `Loadout` (the Hand: attunements, four slots, the surge, shapes,
   Heresies; plus the weapon kind and its active aspect) is the only input to `MarkRules.InForce(mark, loadout)`.
   `PlayerUpgrades.Refresh` asks that instead of "belongs to the element held".
3. **Rules are plain classes, MonoBehaviours adapt** (`.claude/skills/feature-loop`). Each new rule (cooldowns, the
   charged blade, the reaction engine, Bond, the death rule) is a class with no scene in it and its own EditMode tests.
   New behaviour goes into new adapters (`PlayerSpells`, `PlayerReactions`, `BondTracker`, `ShardKeeper`), not into
   `PlayerUpgrades`, which is already 2,100 lines; it gains only small public hooks.
4. **Rule switches live in one `RuleBook`.** Heresies, Devotions and keystones that turn a standard rule off or swap it
   (cooldowns, the guard, flasks, stamina, the mana bar) write to one per-player `RuleBook`; the systems ask it at the
   moment the rule applies. No Heresy check is scattered through the guard or the flask code.
5. **One save.** A versioned JSON file (`SaveData`) replaces `RunState`'s in-memory run and `Progress`'s PlayerPrefs
   flags; `Progress.Has/Unlock` stay as a facade so every door, ladder and wall keeps working unchanged.
6. **New screens build themselves** at runtime under the HUD, as `GuidePage` already does, so no scene must be rebuilt
   to add the Hand screen, the rest menu, the spell diamond or the Way chart.
7. **Words are data.** The playtest's details and guide texts stay Markdown in `docs/design/playtest/`, read into the
   generated `PlaytestTexts` asset by `Clover > Build > Playtest Texts`; a test ties every high-level mark id to an
   entry.

```
 Controls.inputactions --> PlayerController (layered buttons) --+--> PlayerCombat (blows, infused strike, chord)
                                                                +--> PlayerSpells (4 slots, cooldowns, surge) --> forms
                                                                +--> PlayerDefence (guard, dodge, flask) <-- RuleBook
 Loadout (Hand, weapon, aspect) --> MarkRules.InForce --> PlayerUpgrades.Refresh --> stats, effects, switches
 AfflictionSet.Lay --> ReactionEngine --> CombatEvents.Reacted --> PlayerReactions --> effect runner
 Blows, kills, gods --> BondTracker --> BondLedger (save)        Deaths --> ShardKeeper --> SaveData
```

---

## 1. What exists and what changes

| Today (as built) | Becomes | How |
|---|---|---|
| `Upgrade`, `UpgradeKind`, `UpgradeEffect`, triggers, actions, `StatMod`, `StatWhen`, `TallySpec` | The same, extended | New fields (2.1), kinds `Devotion`, `Reaction`, `Heresy`, `Evolution`, `Aspect`, `Temper`, `Technique`, `Bond`; new triggers, actions and stats listed where used |
| The effect runner in `PlayerUpgrades` (`Trigger`, `Flush`, 64 effects a frame) | Reused as is | Reactions, infused strikes, masters and Way marks all fire through it; one public entry `PlayerUpgrades.Fire(trigger, target, at)` |
| `PlayerUpgrades.InForce`: "belongs to the element held" and "weapon name contains" | Replaced | `MarkRules.InForce(mark, loadout)` (3.6) |
| `MarkRules.Open/CanBuy` (Phase 0) | Extended | Currency, gates and the playtest's free chart in one place |
| `UpgradeTree` (ring geometry, stick steps) | Kept; layouts added | `IChartLayout`: `RingLayout` (today's), `HeartLayout` (duo branches), `WayLayout` (4.2) |
| The eleven trees, 428 marks, written to the old shape | Rewritten tree by tree (Phase 4) | Spell branches, passives, Devotion, lift; old ids kept where the design keeps the mark, cut ids retired |
| The eleven ring mixes (`PlasmaTree` ...) | Become 11 of the 55 duo branches | Reshaped by `pairs_d1_d2.md`; ids kept |
| `ElementKit` base spell (tap) and surge | Base spell becomes a slot spell; surge stays | Each base spell is a `Learn` mark of its branch (Phase 4); in Phase 1 a hidden given mark `<element>.base` |
| The worn spell (hold Triangle), `SpecialHoldSeconds`, `Worn`, `Lent`, `RunState.WornSpell` | **Retired** | Four slots in the `Loadout`; `Lent` (the yard's rack) becomes "lend to a slot" |
| Mana: starts at 0, pays for every spell | Changed | Basic spells on cooldowns; mana for the surge, larger spells, deep strikes, the weapon skill; settles toward half out of a fight (3.2) |
| `PlayerCombat.Empower` (the surge's coat) | Kept for the surge | The charged blade is a separate rule (3.3) |
| `NextBlowDamage` (Stored Charge) | Kept | Unrelated to the charged blade; both multiply |
| One element: `PlayerElement.Kit/Info`, 113 reads in 13 files | Two attunements | `PlayerElement` holds two kits; each read becomes "the primary" or "the spell's or mark's own element" (task 2.2) |
| `AfflictionSet` (all eleven kinds can already lie at once) | Kept; gains a present mask, a per-pair rest and a `Lay` entry | Reactions hook there (3.7) |
| `Hit` carries one affliction plus a burn | Gains a second affliction | Both carried roots lay on every blow (`pairs_d1_d2.md`: "both afflictions are now laid by carrying both") |
| Five copies of "ignite, apply, report" in `Enemy`, `TrainingDummy`, `Colossus`, `StormDragon`, `SunBearer` `TakeHit` | One helper | `AfflictionSet.TakeFrom(in Hit, Component who)` |
| `CombatEvents` | Extended | `Reacted`, `EnemyBeginsAttack` (Uraeus and others) |
| `Weapons` factories, `Weapon`, `WeaponCombo`, `AttackStep` | Kept as the base of every Way | Aspects patch a built `Weapon`; a god's bough swaps the factory (`Weapons.StormKatana`) |
| Weapon skill on a held heavy | Moves to Triangle / Y / Q | `WeaponSkill` action; held heavy stays a charged heavy |
| `Controls.inputactions` (one Player map, 10 actions), `ControlBindings`, `ControlsPage` | Extended | A `Spells` map and a layer action (3.5); rebinding keeps working per map |
| `Progress` (PlayerPrefs flags), `RunState` (memory only) | **Retired as storage** | `SaveData` and `SaveStore` (5); `Progress` becomes a facade over `SaveData.Flags` |
| `RunEnder` (a fall ends the run, wake in the hub) | **Retired** | Linear game: a fall returns to the last rest fire (`AreaDirector` already does this in the lands) |
| `EmberDebt` (lost embers spread over the killed) | **Retired** | `ShardKeeper`: the killer carries them (3.13) |
| `ReturnRule`, `LandReturn`, `FallCatch` | Kept | `FallCatch`'s last firm ground is the safe ground for the death rule |
| `GodReward` (Take replaces the element and forgets its tree) | Changed | Take: own the element for good, the god's weapon and its bough, the god's Heresy found; Consume: an offer screen (3.12) |
| `Gifts` (Signature by rule) | Kept | The god's gift stays the tree's Signature |
| `LoadoutChoice` (hub stands, run start) | Becomes the armoury | `ArmouryStand` (3.14) |
| `RestPoint.Use` opens the chart | Opens a rest menu | `RestMenu`: Hand, Chart, Way, Hub, Leave |
| `UpgradeMenu` (uGUI, ring chart) | Kept, layouts injected | Spell branches, shapes, the heart view for pairs, the Way |
| `PlayerHud` (built by `PlayerBuilder`) | Kept | The spell diamond and blade sign are a separate self-building `SpellDiamondHud` |
| `GuidePage`, `MarkDetails`, `PlaytestTexts` (Phase 0) | Kept, fed more | Reactions, cooldowns, the blade, aspects, Heresies, Evolutions, the codex |
| `PlaytestMode` | Kept | `RestMemories` is read at last; every new rule asks `PlaytestMode.On` where the playtest differs |

---

## 2. The data model

### 2.1 The mark, extended

`Upgrade` gains these fields. Each defaults to today's meaning, so the 428 marks as written stay valid until their
tree is rewritten.

| Field | Type | Meaning |
|---|---|---|
| `Tree` | `MarkTree` enum: `Element`, `Duo`, `Way`, `Body`, `Hidden` | Which chart it is drawn on. `Hidden` marks (reactions, base spells in Phase 1, undiscovered Evolutions) are never drawn and never sold |
| `Currency` | `Currency` enum: `Shards`, `Memories`, `None` | What it is bought with. `None`: given (gifts, Bond marks, the god's Heresy) |
| `Branch` | string | For a spell branch's marks: the id of its `Learn` mark. Empty for passives |
| `Part` | `BranchPart` enum: `None`, `Learn`, `Empower`, `Shape`, `Master` | Its place in the branch. Frames and prices follow (Learn as a Spell, Empower as a Minor, Shape and Master as Majors) |
| `Shapes` | `ShapeDef[2]` | On a `Shape` mark (a spell's or a duo branch's): the two versions, each with `Name`, `Description`, and any of `Stats`, `Effects`, `Form`, `Spell` that replace or add to the base |
| `Role` | `SpellRole` enum: `None`, `Opener`, `Breaker`, `Detonator`, `Counter`, `Movement` | On a `Learn` mark and a duo spell: the role icon in the Hand |
| `Lifts` | string (element) | On a reaction-lift mark: the element whose reactions it lifts (its effects ride the `Reacted` trigger, 2.6) |
| `Weapon` | string (exists) | Now the weapon **kind** (`Weapons.KatanaName`); the god's weapon counts as its kind |
| `Aspect` | string | On a Way mark: the aspect id it belongs to (`katana.sheath`); empty for the trunk |
| `Gate` | string | A progress flag that must be set before it can be bought: an elite beaten (techniques), a land reached (the Way's stages, `depth_weapons.md` 6), a god felled (slots). Sealed marks say what opens them |
| `Breaks` | `RuleTag` | On a Heresy: the rule it breaks; two with the same tag cannot be worn together |
| `Recipe` | `RecipeDef` | On an Evolution: the base `Learn` id and the partner (2.8) |
| `Seen` | string | One line for the body sign or look (Heresies), read by the look code |

`SpellSpec` gains `Cooldown` (seconds; 0 means a mana spell), `HoldForm` (the form a held press casts, for Held
Thunder and its kind), `SecondPress` (a form or step a second press within the window casts), and keeps `Cost` (mana)
and `HealthCost`.

New kinds and prices (`Upgrade.UsualCost`; the shared prices of `README.md`):

| Kind | Price | Notes |
|---|---|---|
| `Spell` (a `Learn`) | 4 Shards | Learn 4, Empower 1 (`Minor`), Shape 3 (`Major`), Master 3 (`Major`) |
| `Root`, `Major`, `Keystone` | 2, 3, 6 Shards | As now |
| `Devotion` | 6 Shards | In force only all in (3.8) |
| `Major` with `Lifts` | 3 Shards | The reaction lift |
| `Mix` (a duo branch mark) | written: Shape 3, Minor 1, Major 3, Keystone 5, duo spell 5, duo surge 5 | As now, `Element2` set; any pair, not only neighbours |
| `Signature` | given | The god's gift |
| `Reaction` | never sold | One per pair, `Tree = Hidden` |
| `Heresy` | given, or 8 Shards (an element's deep Heresy), or opened by 30 Memories spent on its Way | 3.10 |
| `Evolution` | 5 Shards, once found | 3.11 |
| `Aspect` | 2 Memories (a god's bough: free after Take) | Opens the bough and its form |
| `Temper` | 1, 1, 2, 2, 3 Memories | +6% weapon damage, +5% stagger each |
| `Technique` | 2 Memories, `Gate` an elite | On the R1 and R2 chord |
| Way `Minor`/`Major`/`Keystone` | number 1, move 2, rule 2, keystone 4 Memories | `weapons.md` 0.2 |
| Body marks | 2 Memories each | Health 4 ranks, stamina 3, flask count 2, flask strength 2, poise 1 |
| `Bond` | given at Bond V | 3.9 |

### 2.2 Element trees

A tree file keeps its form (`docs/UPGRADE_SCHEMA.md` 13): `public static class FireTree` with `Add(List<Upgrade>)`,
one line in `UpgradeTrees.AddAll`. A spell branch is four marks; a helper keeps it to one call so a writer cannot
get the wiring wrong:

```csharp
// Trees/SpellBranch.cs: builds Learn, Empower, Shape, Master with Branch, Part, Requires and prices set.
SpellBranch.Add(to, E, id: "fire.wave", name: "Flame Wave", role: SpellRole.Opener, slot: Slots.LeftLow,
    learn: new SpellSpec { Cooldown = 7f, Damage = 20f, Stagger = 24f, Reach = 5.5f, Arc = 70f, Stacks = 1, Seconds = 3f },
    form: SpellForms.Wave, learnWords: "...",
    empower: new Empower { Cooldown = 5.5f, Seconds = 5f, Words = "..." },
    shapeA: new ShapeDef { Name = "Cinder Fan", Spell = new SpellSpec { Reach = 3f, Arc = 150f, Stacks = 2 }, Effects = ... },
    shapeB: new ShapeDef { Name = "Running Fire", Form = SpellForms.Spikes, Spell = ... },
    master: new UpgradeEffect { On = UpgradeTrigger.ChargedBlowThird, Do = UpgradeAction.CastOwn, ... });
```

- **Empower** is "stronger, shorter cooldown": an `Empower` record of the numbers it changes in the spell's spec, not
  a stat (stats apply to every spell).
- **Shape** is one mark holding both `ShapeDef`s. Buying it opens both; the `Loadout` holds which one each spell uses
  (`ShapeChoice`), changed at any rest fire. Before it is bought the base spell is cast.
- **Master** is effects with `OwnBranch = true` (a new condition: fires only for this branch's spell) and the new
  actions in 3.1 (`ResetCooldown`, `InstantNext`, `CutCooldowns`).
- **Passives**: Root (lays the element's affliction on every blow), majors, one or two keystones, the lift, Devotion,
  the god's gift. Written exactly as marks are written now.
- **The larger spell** (one per element, 35 to 50 mana; Dark's paid in health) is a branch whose `SpellSpec.Cooldown`
  is 0 and `Cost` set.
- **Per element, outside the tree**, in a new `Trees/ElementSteel.cs` table: the charged blade's third-blow extra, the
  infused strike, the deep strike and the starter Hand. These are not bought; they come with carrying the element.

```csharp
public sealed class ElementSteel   // one per element, in ElementSteel.All
{
    public string Element;
    public UpgradeEffect[] BladeThirdBlow;     // Fire: the ground under the target burns 3 s
    public string StrikeName;                  // "Burning Edge"
    public UpgradeEffect[] Strike;             // acts on the carrier's Footprint (3.4)
    public string DeepName;                    // "Burnt Offering" (all in, L2 with R2, 20 mana)
    public UpgradeEffect[] Deep;
    public string[] StarterHand;               // four Learn ids and a surge
}
```

Layout on the chart: about six branches of four plus about ten passives is 34 marks, the size trees are now, but the
shape differs. Task 4.1 writes the new worked layout (three boughs, each with two branches of four as short stalks
off the bough, passives on the boughs and trunk) as a pattern proved by a test, as `ThePatternTreeOfTheSchema...` proves
today's. Writers then use the slots without arithmetic.

### 2.3 Duo branches, duo spells, duo surges

- A duo branch is `Kind = Mix` marks with `Element` and `Element2` set, for **any** pair. Storage order does not matter:
  every pair lookup goes through `AfflictionPairs.Index(a, b)` (unordered, 0 to 54). Ids: the 11 built branches keep
  theirs (`plasma.*` for Fire and Lightning, whose reaction is Flashover); the 44 new ones take their reaction's name
  (`steam.*`, `capsize.*`, `sleetcharge.*`).
- Open while both elements are carried (`MarkRules.Open`; in the playtest, always).
- Its `Shape` mark carries the reaction's two shapes (2.4). Its duo spell is a `Learn` with `Element2` set: it can sit
  in a slot only while both are carried, and costs 5 Shards. A duo surge is a mark with `UpgradeAction.Form` on
  `SurgeStart`, as the built crowns are, offered in the surge slot while both are carried.
- Files: one file per pair under `Trees/Duo/` (namespace `Clover.Player.Trees.Duo`), named by reaction
  (`SteamDuo.cs`), registered in `UpgradeTrees.AddAll` after the ring's trees. 55 small files are easier for parallel
  writers than five large ones.

### 2.4 Reactions

A reaction is one hidden mark per pair, so it reuses effect slots, cooldowns, `Owner` and the runner:

```csharp
// Trees/Reactions/Reactions.cs: all 55 plain reactions, in the order of README.md's table.
to.Add(new Upgrade { Id = "react.flashover", Name = "Flashover", Kind = UpgradeKind.Reaction, Tree = MarkTree.Hidden,
    Element = Elements.Fire, Element2 = Elements.Lightning,
    Reaction = new ReactionDef { Rest = 4f, OrderMatters = false, Spends = true },
    Effects = new[] {
        new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Burn, Share = 0.5f, Radius = 3f, Stagger = 20f },
        new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Share = 0.5f, Radius = 3f },
        new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Flinch, Seconds = 0.4f } } });
```

- **The plain reaction** is always in force: it fires from the first time, whatever is carried (a Soaked enemy from the
  world's water reacts with the player's Burn, which is a good thing).
- **Shapes**: the duo branch's `Shape` mark has two `ShapeDef`s whose `Effects` ride `Reacted` with a new condition
  `Reaction = "react.flashover"`. When the `Loadout` sets a shape for that pair, the runner fires the shape's effects
  **instead of** the plain mark's (`ReactionTable.EffectsFor(pair, loadout)`).
- **Lifts**: a mark with `Lifts = Fire` holds effects on `Reacted` with `Needs = Burn` (true for every reaction Fire is
  in, since the target carries Burn when it reacts) and stats on the new `PlayerStat.ReactionDamage` and
  `ReactionStagger`, which every `Reacted` effect is multiplied by.
- **Keystones and marks that deepen one reaction** are ordinary duo marks with effects on `Reacted` and the
  `Reaction` condition.
- **Deliberate cascades** (Sea Ice, Breakers, Hot Shards, Blown Seed, The Fairy Wind, Sandblast, The Debt Passes) set
  `Cascades = true` on the effect: what it lays may react. **Forced through the rest** (Lit Fuse, Plasma Lance, Black
  Sun, The Sea Boils): the action `ForceReaction` with the pair, which ignores the rest once.

### 2.5 The Way

Each weapon's Way is one file under `Trees/Ways/` (`KatanaWay.cs`), marks with `Tree = Way`, `Currency = Memories`,
`Weapon` the kind, `Aspect` the bough's id (empty for the trunk). The Body branch is `Trees/Ways/BodyWay.cs`
(`Tree = Body`, no weapon). What a bough **does to the moves** is code, because aspects change chains, add moves and
change rules:

```csharp
// Runtime/Player/Way/WeaponAspect.cs
public abstract class WeaponAspect
{
    public abstract string Id { get; }            // "katana.sheath"
    public abstract string Weapon { get; }        // Weapons.KatanaName
    public virtual bool GodBough => false;        // true: the god's weapon itself
    public virtual Weapon Build() => null;        // a god's bough builds its own weapon (Weapons.StormKatana())
    public abstract void Patch(Weapon weapon, IMarkQuery marks);   // retime, add, replace steps; set flags
    public virtual void Tick(ref AspectRun run, in AspectContext context) { }  // counters, windows (Heat, sheaves, the Answer)
}
```

- `Armoury.Resolve(kind, aspectId, marks)` builds the weapon: a god's bough calls its `Build()`, any other bough takes
  `Weapons.ByName(kind)` and its `Patch`. It runs on choosing the aspect at a rest fire, on entering a scene, and when
  an aspect mark is bought, then `PlayerCombat.Equip` takes the result. One aspect class per bough (30 plain, 9 god
  boughs), in `Runtime/Player/Way/Aspects/`. Stateless like forms; state lives in `AspectRun` on the player.
- **Temper** marks are trunk marks with stats `WeaponDamage +0.06`, `StaggerDealt +0.05` (III and V add a line of the
  weapon's own, written as effects or a patch flag).
- **Infusion family marks** (Ground, Line, Weight, Draw) are trunk marks that set a flag the infused strike reads (3.4);
  the Katana's element infusion marks (Burning Crescent and the rest) are trunk marks with `Element` set, in force
  while that element is carried.
- **Gated techniques**: `Kind = Technique`, `Gate = "elite.<id>"`, a `SpellSpec` or an `AttackStep` factory for the
  move; on the chord (3.5). The Fists, taught twice, keep a `TechniqueChoice` in the `Loadout`.
- **Body**: stats `Health`, new `StaminaMost`, `FlaskCount`, `FlaskHeal`, new `Poise`. Found Body pieces are given
  marks of the same stats (`Currency = None`, `Tree = Body`, drawn as found).

### 2.6 Heresies and Evolutions

- **Heresy**: `Kind = Heresy`, `Breaks = RuleTag.X`, `Element` or `Weapon` set (works only while that is carried) or
  neither (the nine cross-system ones). What it does is a small class implementing `IHeresy` in
  `Runtime/Player/Heresies/` (`Apply(RuleBook)`, optional `Tick`), named by the mark's id, plus any effects it can say
  as data. `RuleTag` holds every rule either depth file names: `Cooldowns`, `Mana`, `FourSlots`, `Stagger`,
  `PerfectGuard`, `Guard`, `Dodge`, `PerfectDodge`, `Stamina`, `Healing`, `Health`, `Moving`, `ChargedBlade`,
  `AfflictionFade`, `SpellOrigin`, `Reach`, `Death`, `FightGrowth`, `PlayerAfflictions`, `Harm`, `Collision`, `Arrows`,
  `Enemies`, `Elements`, `Currency`, `WorldHardness`.
- **Evolution**: `Kind = Evolution`, `Tree = Hidden` until found, `Recipe = new RecipeDef { Base = "fire.uraeus",
  Partner = PartnerKind.Aspect, PartnerId = "spear.sunspear" }`. Partner kinds: `Aspect`, `Devotion` (bought, element
  carried), `SpellInHand`, `ReactionShape`, `BondRank`. The evolved spell is a full `SpellSpec` and form, no shapes.

### 2.7 Bond and Pair Bond

`Trees/BondTable.cs`: per weapon kind and per element, the five ranks as `StatMod[]` (weapon: recoveries 4% shorter,
finisher +10%, recoveries 8%, finisher +20%; element: shorter cooldowns and deeper afflictions, numbers to be set),
the points for each rank, and the Bond mark id at V (a `Kind = Bond` mark: the active aspect's keystone in its true
form, or the element's second Devotion-strength passive). Pair Bond: three ranks, +5% infused strike at I and II, the
pair's own strike at III, built by rule (`PairStrike`, 3.9) or from one of the twelve standouts (`Trees/PairStandouts.cs`).

### 2.8 How content becomes data, and how the words tie in

- **C# initialisers, not ScriptableObjects or tables.** The trees are already C#, the schema page and
  `UpgradeChartTests` prove them, and effects as code are checked by the compiler. A parallel table format would need
  an importer, a second schema and its own tests. Writers turn a design file into a tree file by hand, a tree per
  session.
- **What cannot yet be said as data** is written the way the schema already allows: the nearest working version, a
  `// NEEDS: <engine piece>` comment, and a line in a new ledger `docs/design/steel/content_open.tsv` (id, what is
  approximated, the engine need from README's list). The ledger is the progress report; `DumpMarks` gains an entry
  that writes `content_open.md` from it.
- **Details texts**: `docs/design/playtest/marks/<file>.md` keyed by mark id, as Phase 0 has them, one file per tree,
  plus `duo/<reaction>.md`, `way/<weapon>.md`, `heresies.md`, `evolutions.md`. The guide to effects
  (`effects.md`) gains the groups Reactions (all 55), Spells and cooldowns, The blade, The Way, Heresies. New EditMode
  test `EveryHighLevelMarkHasDetailsAndEveryDetailNamesAMark` (the auto account in `MarkDetails` stays the fallback, so
  the test lists missing ones as a report, and fails only on details that name no mark).
- **Retired ids**: cut marks are deleted from their tree file and listed in `Trees/RetiredMarks.cs` (id and refund),
  so a save that holds one refunds its price (5.2). Kept marks keep their ids (`fire.cinder`).

---

## 3. The runtime systems

Numbers below are the shared rules' (`README.md`, `STEEL_AND_ELEMENT.md`). Each rules class exposes them as public
fields so `script eval` and `component set_field` can tune them live; the adapter copies its serialized values in.

### 3.1 Spell cooldowns, shortened by weapon play

- **Rules**: `Runtime/Player/Magic/SpellCooldowns.cs`. Fixed arrays for six timers: four slots and the two infused
  strike buttons. `Start(i, seconds)`, `Tick(dt, rate)`, `Cut(seconds)` (every running timer), `Reset(i)`,
  `Ready(i)`, `Fraction(i)`, `InstantNext(i)` (the next cast of slot i has no wind-up and no cooldown), `Frozen` (no
  cooldowns at all, for In One Breath). No allocation.
- **Cuts**, in `Magic/CooldownCuts.cs`: a weapon blow that lands 0.3 to 0.6 s by move (new `AttackStep.CooldownCut`;
  left 0, it is 0.3 for a light step and 0.6 for a heavy one), a perfect guard 2 s, a perfect dodge 1.5 s, a finisher
  3 s; an infused strike counts as a weapon blow. Scaled by new stats `CooldownCut` (share) and divided by
  `SpellCooldown` (share, from Empower-like marks and Bond).
- **Adapter**: `PlayerSpells` (new MonoBehaviour on the player). `PlayerController` calls `PlayerSpells.Advance(dt,
  free)` right after `PlayerElement.Advance` in its one order. It reads the slot buttons (3.5), casts through the forms
  exactly as `PlayerUpgrades.TrySpecial` does now (refactored into `PlayerUpgrades.Cast(Upgrade mark, SpellSpec spec,
  int slot)` so the base spell, slot spells, duo spells and Evolutions share one path), starts the slot's cooldown, and
  listens to `PlayerCombat.BlowLanded`, `PlayerDefence.PerfectGuarded`, `PerfectDodged` and `Finished` (new C# events
  raised where `upgrades.OnBlow` and its siblings are called today) to cut.
- **Events**: `PlayerSpells.Cast(slot, mark)`, `Landed(slot, target)` (first enemy struck by that cast; the charged
  blade listens), `Ready(slot)` (the HUD flashes the slot).
- **Data actions** (for Masters and marks such as Glass Bell, Strike While Hot, Need-Fire): `UpgradeAction.CutCooldowns`
  (`Amount` seconds), `ResetCooldown` (own branch's slot, or `Spell` id), `InstantNext`.
- **Rule switches** read from the `RuleBook`: `CooldownMode` = `Normal`, `None` (Given Fire), `ManaPriced` (Old Magic:
  15 mana a basic spell, no cooldowns, weapon play no longer cuts).

### 3.2 Mana for the big moves

- **Rules**: `ManaPool` gains `Settle(dt, inFight)`: out of a fight, mana below half rises at `SettleRate` (5 a second,
  a guess) to half; above half it stays. `Maximum` 100. Earning is unchanged (blows, perfect guards 15, perfect dodges
  12, finishers). The pool starts each scene at half.
- **What it pays for**: the surge (as now), each element's larger spell (35 to 50), duo spells that are their pair's
  larger spell (40 to 60), the deep strike (20), the weapon skill (as now, now on its own button). Basic spells set
  `SpellSpec.Cost = 0`.
- **In a fight**: `Magic/FightClock.cs` (rules) says a fight has ended when no enemy has been aware of the player within
  `FightRange` (12 m) for 8 s (`depth_elements.md`'s definition, which The Last One in the Jar also needs).
  The adapter feeds it from `AreaDirector`'s enemies (nearest aware enemy, sampled four times a second, not every
  frame). Not verified: which `EnemyBrain` state means "aware"; the task reads `EnemyBrain` first.
- `ManaPool.Instead` (Heartwood) stays and becomes the `RuleBook`'s `ManaMode.Sealed`; Drawn From the Cloud adds
  `ManaMode.FromStatic`.

### 3.3 The charged blade

- **Rules**: `Magic/ChargedBlade.cs`: `Charge(element, blows = 3)` (a fresh charge replaces the old), `OnBlowLanded()`
  returns `(element, damageShare 1.2, isThird)` and counts down; `Clear()`. Infused strikes, reaction damage and
  effect damage never charge it. The Edge Kept Bright and The Edge Drinks are `RuleBook.BladeMode` values (`Normal`,
  `LitByCritical`, `NeverFades`).
- **What charges it**: `PlayerSpells.Landed` for a slot spell (basic, larger, duo, evolved). A duo spell charges with
  its first element (an open question below).
- **Adapter**: in `PlayerCombat`'s blow path (after `Stats` and before `TakeHit`): the charged element's affliction
  (one stack, its usual time) goes into the `Hit`'s second affliction when it differs from the root's, or adds a stack
  when it is the same; damage times 1.2; on the third, `ElementSteel.BladeThirdBlow` fires through the runner with the
  struck enemy as target (new trigger `ChargedBlowThird`; `ChargedBlow` for every charged blow).
- **The sign**: `BladeLook` (in `Runtime/Player/`), placed every frame on the weapon's hand socket from `WeaponRack`,
  drawn with `ElementSubstance`/`FormFx` pieces so Fire is flame and smoke, Lightning crackle, Ice frost
  (`element-effects-read-as-element` applies). Three pips on the HUD (4.3).
- **Events**: `ChargedBlade.Charged(element)`, `Spent()`.

### 3.4 Infused strikes

- **The carrier**: `Weapon.Carrier` (an `AttackStep` reference) set in each `Weapons` factory from `weapons.md` (the
  Katana's drawing cut, the Hammer's drop ...). A god's bough may set its own (the Jawbone's cast).
- **The footprint**: `Player/Footprint.cs`, a struct read from the carrier step and recorded when it strikes: `Lane`
  (`LineWidth`), `Disc` (`Radius`), `Path` (`Travel` or a shot), `Ring`, else `Arc` (`Reach`, `ArcDegrees`); and the
  enemies struck. New `HazardShape.Footprint` and `EffectAt.Footprint` let a data effect lay ground along it or strike
  along it, so each element's strike (`ElementSteel.Strike`) is data: Fire's Burning Edge is a `Cone` over the arc plus
  3 m and a `Hazard` with `Shape = Footprint`.
- **Input**: L2 with R1 (first element), L2 with R2 (second element; all in, the deep strike, which also costs 20
  mana). Started through `WeaponCombo.Follow(carrierClone, stamina)`, the existing way to start a given step outside
  the chain; the clone carries a flag `Infused` and the element.
- **Rules**: each button has its own 8 s timer in `SpellCooldowns` (slots 4 and 5), cut by weapon play like a spell's;
  the strike counts as a weapon blow (cuts cooldowns, gives mana) and never charges the blade.
- **Infusion family marks** bend the clone before it is started (Line: the draw becomes a 5 m dash; Weight: slower,
  unstoppable, +60% stagger), as small functions in `Way/Infusion.cs` keyed by family. All in, the family mark's
  length, time or pull is doubled.
- **Pair Bond III** replaces the generic strike with the pair's (3.9).

### 3.5 The L2 layer as input actions

`Controls.inputactions` (JSON, edited directly as earlier commits did, not a denied file type) becomes:

| Map | Action | Pad | Keyboard and mouse |
|---|---|---|---|
| Player | `Dodge`, `LightAttack`, `HeavyAttack`, `Guard`, `Flask`, `Interact`, `Pause`, `Move` | as now | as now |
| Player | `WeaponSkill` (the old `Spell` action **renamed**, so its id and any saved override survive) | Triangle / Y | Q |
| Player | `Surge` | removed from the pad (L2's tap is read from `SpellLayer`) | F |
| Spells | `SpellLayer` | Left trigger | Left Ctrl |
| Spells | `SpellWest`, `SpellNorth`, `SpellEast`, `SpellSouth` | Square, Triangle, Circle, Cross (X, Y, B, A) | 1, 2, 3, 4 |

- **Rules**: `Magic/SpellLayerRules.cs` (no Input System types, so it is testable): fed each frame with the layer
  held, the four slot presses (and whether each came from the keyboard), light, heavy and the face buttons of the
  Player map. It answers: which slot was cast; whether light or heavy is an infused strike; which Player-map presses
  to swallow (on a pad, while the layer is held, Square, Triangle, Circle, Cross, R1 and R2 belong to the layer); and
  whether the surge is lit. **The surge on a pad is a tap of L2**: lit on release when it was held under
  `SurgeTapSeconds` (0.25) and nothing on the layer was pressed meanwhile. Keyboard spells 1 to 4 need no layer.
- **Adapter**: `PlayerController` owns a `LayeredButtons` object built from both maps; `PlayerCombat`, `PlayerDefence`,
  `PlayerInteraction` and `PlayerSpells` ask it (`Pressed(PlayerButton.Flask)`) instead of calling
  `WasPressedThisFrame` on their own actions, so swallowing happens in one place. Five readers change (`PlayerController`
  dodge, `PlayerCombat` light and heavy, `PlayerDefence` guard and flask, `PlayerInteraction` use, `PlayerElement`'s
  spell and surge, which move to `PlayerSpells`).
- **The technique chord** (R1 and R2 together, both mouse buttons): `Magic/ChordRules.cs`. Light starts at once as now;
  a heavy press within `ChordSeconds` (0.08) of a light press, while that light step is still in its first 0.08 s of
  wind-up, cancels it into the technique (a new `WeaponCombo.Replace(step)` that is allowed in early wind-up). No
  latency is added to the light attack.
- **Rebinding**: clash checks in `ControlBindings.FindClash` are per map, so the slot actions may share face buttons
  with Flask and Dodge (they live in another map); within the Spells map they cannot clash with each other.
  `ControlsPage` gains a "Spells" group of rows; `ControlNames` names them for prompts; the HUD diamond reads the
  effective bindings, so a rebound slot shows its new button.
- **Tests to update**: `ControlsAssetTests.ControlsHaveThePlayerMap...` (the action list), `ControlBindingsTests`.

### 3.6 The Hand and the loadout

- **Data**: `Runtime/Player/Build/Loadout.cs` (plain, serializable):

```csharp
[Serializable] public class Loadout
{
    public string[] Attunements = new string[2];   // "" for an empty attunement; the same twice is all in
    public string[] Slots = new string[4];          // Learn or duo-spell or Evolution ids, by button: West, North, East, South
    public string Surge = "";                       // a surge id: "<element>.surge" or a duo surge mark id
    public string[] Heresies = new string[2];
    public List<IdChoice> Shapes = new List<IdChoice>();   // spell or reaction id -> 0 or 1 (A or B)
    public List<IdChoice> UseEvolved = new List<IdChoice>();// Learn id -> 1 when the evolved spell is chosen
    public string Weapon = "";                      // weapon kind
    public List<IdText> Aspects = new List<IdText>();      // weapon kind -> active aspect id
    public string Technique = "";                   // the Fists' choice of two
    public bool AllIn => Attunements[0] != "" && Attunements[0] == Attunements[1];
}
```

  Up to three saved Hands are `Loadout` copies in the save (only their Hand fields are swapped in, never the weapon).
  Three Bloods (a third element) makes `Attunements` length 3; nothing else assumes two, and every loop runs over
  `Attunements`.
- **Rules**: `Build/HandRules.cs`: `ChoicesFor(slot, loadout, owned, learned)` (only spells of carried elements, duo
  spells with both carried, grouped by element, role attached; at most about 13), `Validate(loadout)` (drops what is
  no longer allowed: a duo spell whose pair broke, an Evolution whose partner went, which then falls back to its base),
  `SlotsOpen(progress)` (`Build/SlotSchedule.cs`: one element, two slots and the surge at the start; the third slot at
  the Champion of the Gate (`elite.champion_gate`), the fourth at the Fire god (`god.fire`), the second attunement after
  any god; all open in the playtest), `CanChangeHere(atRestFire)`.
- **What is in force**: `MarkRules.InForce(mark, loadout, learned)`:
  - element marks: their element is attuned; a `Devotion` only all in; Empower, Shape and Master only while their
    `Learn` is in a slot; a shape's effects only for the chosen shape;
  - duo marks: both elements attuned; the duo spell only in a slot;
  - reactions: always (the plain one or the chosen shape, 2.4);
  - Way marks: the weapon kind carried; aspect marks only for the active aspect; marks of a sleeping bough are kept and
    sleep (`weapons.md` 0.1); Body marks always;
  - Heresies: in a Heresy slot, and their element or weapon carried;
  - Bond and Evolution marks: by their owner being carried.
  `PlayerUpgrades.Refresh` replaces its `InForce` with this call; the effect lists and stats are rebuilt exactly as now.
- **Adapter**: `PlayerBuild` (new MonoBehaviour on the player) holds the current `Loadout`, applies it on scene start
  and at a rest fire (`PlayerElement.Attune`, `Armoury.Resolve` and `PlayerCombat.Equip`, `PlayerUpgrades.Refresh`),
  raises `Changed`. The Hand changes only through `PlayerBuild.Apply(loadout, atRestFire: true)`.
- **Effects that name no affliction** now lay the mark's own element's (its `Owner.Element`), not "the element held".
  Spells cast in their own element (`PlayerElement.NewCast` takes an `ElementInfo`).

### 3.7 Afflictions two at a time, and the reaction engine

- **Already there**: `AfflictionSet` keeps stacks for all eleven kinds at once. What is new:
  - `PresentMask` (a `ushort`, kept as stacks rise and fall) so "does it carry the partner" is one AND;
  - `ReactionRest` (`float[55]` per set, the time until each pair may react again: 220 bytes per enemy);
  - `Lay(kind, stacks, seconds, power, LaySource source)`: the one entry every laying goes through. `Apply` stays as
    its body; **`Burn.Ignite` is no longer called from outside**: the five `TakeHit` methods call the new
    `AfflictionSet.TakeFrom(in Hit, Component who)` instead of their own three copied lines, and the dummies and
    `PlayerCombat.AfflictArea` call `Lay`.
  - `LaySource`: `Weapon`, `Spell`, `Effect`, `Reaction`, `World`. Afflictions laid with `Reaction` start nothing,
    unless the effect that laid them had `Cascades = true` (then the source is `Effect`).
- **Rules**: `Runtime/Gameplay/Reactions/ReactionEngine.cs` (plain): on each `Lay` that brought a stack, for each
  partner kind present (`PresentMask` without the kind laid), if `ReactionRest[pair] <= 0`: set the rest
  (`ReactionDef.Rest`, 4 s by default, 6 s for distance-3 pairs), and report `(who, pair, laidKind)`. Two partners can
  react at once (different pairs). A per-frame budget `MostPerFrame` (8) defers the rest to the next frame. Order is
  laid kind then partner, for the entries where order matters. `AfflictionPairs` (the 0 to 54 index, distance on the
  ring, the pair's reaction id) lives beside it. In `Clover.Gameplay`, keyed by `AfflictionKind` so it needs nothing
  from `Clover.Player`.
- **Event**: `CombatEvents.Reacted(Component who, int pair, AfflictionKind laid)`. Listeners note it and act on their
  next step, as the existing events require.
- **Adapter**: `PlayerReactions` (on the player) queues each into the effect runner as trigger `Reacted` with the
  enemy as target, the pair's effects chosen by `ReactionTable.EffectsFor(pair, loadout)` (plain or shape, plus every
  in-force lift and duo mark with that `Reaction` condition), with the runner's `LaySource` set to `Reaction` while
  they run. Ground a reaction lays carries the flag in `HazardSpec.FromReaction` so its later bites start nothing.
  It also shows the reaction: its name as a short status word over the enemy (status text, allowed to be timed) and
  the codex notes it as seen.
- **Enemy conditions** (README's most-needed engine piece: "a timed state on one enemy that runs an effect on a beat",
  over twenty reactions): `Runtime/Gameplay/Reactions/EnemyConditions.cs`, a fixed array of six `ConditionRun` slots
  per `AfflictionSet` (id, seconds left, beat, beat clock, a count, the mark that set it, flags). Flags cover the
  enemy states the files ask for: `Rooted`, `BeginsNoAttack`, `CannotGuard`, `WindupSlowed`, `DamageTakenShare`,
  `DamageDealtShare`, `SightShort`, `Linked` (to another enemy, for Black Chain and Rot Roots). New action
  `UpgradeAction.Condition` sets one; its `OnBeat`, `OnStruck` and `OnEnd` effects run through the runner (new
  triggers `ConditionBeat`, `ConditionStruck`, `ConditionEnded`). `Enemy` and the gods read the flags where they choose
  attacks, guard and move (one read each in `EnemyBrain` and the `GodlyForm` choosers). **Knocked down** needs its own
  clip and a brain state: a separate task (3.7) because it is animation work.
- **Performance**: `Lay` is called on every blow; the check is a mask test and at most ten array reads. The real costs
  are what reactions lay: hazards (`Hazards.Capacity` 48), forms (`SpellWorks` 24 runs) and the runner's 64 effects a
  frame. Task 3.6 profiles a crowd and raises the capacities only if the profiler says so.

### 3.8 Devotion

- In force only all in (`MarkRules.InForce`). Most Devotions are data (Fire Without End: `AfflictionKeeps` and a
  deepening `EverySeconds` effect). Those that change a standard rule write to the `RuleBook`: In One Breath sets
  `SpellCooldowns.Frozen` for 2.5 s after a perfect guard or dodge; Deep Winter sets `ChillNeverFades`; The Guard Holds
  sets `GuardMode.Unbreakable`.
- All in, L2 with R2 is the deep strike (`ElementSteel.Deep`, 20 mana, its own 8 s timer).

### 3.9 Bond and Pair Bond

- **Rules**: `Runtime/Player/Growth/BondLedger.cs`: points per weapon kind, per element and per pair (weapon by
  element, 110), and `Rank(points)` from `BondTable`. `Add(kind, id, points)`, `RankOf`, `LoseRank` (Bought Back is
  the one thing that lowers Bond). Plain, saved whole.
- **Counting** (`BondTracker`, on the player): a weapon blow landed adds 1 to the weapon and to each carried element;
  a kill adds a little; an elite 50 and a god 200 to everything carried. Pair Bond: a blow while the element is carried
  1, a blow carrying the element (charged blade, infused strike) 2, an elite 50, a god 200. Points are added in memory
  and written to the save at rest fires and scene changes, not per blow.
- **Ranks in force**: `PlayerUpgrades.Refresh` adds the `BondTable` stats of the carried weapon's and elements' ranks
  as learned stats; the Bond mark is given (`Currency = None`) when V is reached and is in force like any mark.
- **Pair strike by rule**: `Growth/PairStrike.cs` builds the pair's strike from the carrier's `Footprint` and the
  element's verb (`spreads`, `jumps`, `carries`, `holds`, `returns`, `grows`, `rises`, `forges`, `mirrors`, `swallows`,
  `seeps`), each verb a short function over existing actions (`Hazard` along the footprint, `Arc`, `Step` and `Push`,
  `Echo`, `Pull` then `Burst`, `Falling` with `Count`) at 1.3 times. One new action: a mirrored `Echo` at a point with
  the facing turned (Light). Standouts replace the rule for their pair.
- **The look** (the weapon wears in, the element grows richer) is art: a rank value handed to `WeaponRack`'s material
  and to `ElementLook`'s brightness, left to the art pass.

### 3.10 Heresies, and how a rule is switched off in one place

- **`RuleBook`** (`Runtime/Player/Growth/RuleBook.cs`, one instance per player, owned by `PlayerUpgrades`, rebuilt in
  `Refresh`): a set of plain fields with their standard values: `CooldownMode`, `ManaMode`, `GuardMode` (`Normal`,
  `Off`, `Draw`, `AllRound`, `Unbreakable`), `DodgeMode` (`Normal`, `Off`, `Step`), `PerfectGuardOpen`,
  `PerfectDodgeOpen`, `StaminaUsed`, `FlasksAllowed`, `HealthMode` (`Bar`, `Stones`), `StaggerFrom` (`Blows`, `Burn`),
  `SlotCount` (4, or 1 for One Word), `SlotsDeck` (What the Bag Holds), `AutoCast` (They Move by Themselves),
  `BladeMode`, `DeathIntercept` (a list of `IDeathIntercept`: the Rise, Bought Back, Death Passed On),
  `WorldShare` (The Hard Road's multipliers). Each system asks it at the moment its rule applies: `PlayerDefence` before
  raising the guard and before drinking, `DodgeRoll` before rolling, `PlayerSpells` before starting a cooldown,
  `ManaPool` for its mode, `PlayerDefence.Die` for intercepts.
- The existing scattered switches (`Afflictions.PoisonBeatOff`, `PlayerCombat.HeavyUnbroken`, `ManaPool.Instead`,
  `DodgeRoll.PayInstead` ...) stay where they are; new ones go only into the `RuleBook`, and an old one moves there when
  a task touches it anyway.
- **Heresy slots**: the `Loadout`'s two; `HandRules.HeresyChoices` filters by carried element or weapon and by
  `Breaks` (two with the same tag are refused, and the Hand says which). Slots open at the third god and the seventh
  (`SlotSchedule`); all open in the playtest.
- **Earning**: the god's Heresy is given where the god fell (a `GiftShrine`-like pickup), sealed until the slot is open
  and the element owned; an element's deep Heresy is bought (8 Shards) and grows from its Devotion or a keystone; a
  weapon's first Heresy opens at 30 Memories spent on its Way (`WayRules.Spent(weapon)`); others come from elites,
  places and Bond (`Gate` flags).
- **The body sign**: `HeresyLook` places each worn Heresy's constant sign from its `IHeresy.Look`.

### 3.11 Evolutions

- **Rules**: `Growth/EvolutionRules.cs`: `Check(learned, loadout, bond, out found)` at each rest fire: for each
  Evolution not yet found, its base's Master bought and its partner in force. Newly found ones are moved from hidden to
  the chart (`SaveData.Found`), a line is shown ("Uraeus has changed."), and the codex records the recipe.
- Buying it (5 Shards) lets the slot offer base or evolved (`Loadout.UseEvolved`). If the partner goes,
  `HandRules.Validate` falls back to the base.
- The codex is a group of entries in `GuidePage` built from `SaveData.Found` and the reactions seen.

### 3.12 Currencies, Take and Consume

- **`Wallet`** (`Growth/Wallet.cs`, plain, saved): `Shards`, `Memories`, `ShardsBanked` (`depth_elements.md`'s
  recommendation: Shards laid in a rest fire once every carried tree is full, safe from death). `Earn`, `Spend`,
  `CanAfford(mark)`. `RunState.Points` and `PlayerUpgrades.Earn/Refund` become calls into it; "Embers" in code and
  words becomes "Shards" as the task touches them.
- **Income** (`Growth/Income.cs`): Shards per kill by tier (enemies 1, first kill a visit; elites 3, gods 5; tier 3 on:
  2, 5, 8); Memories for the first defeat of each kind of enemy (1, by `EnemyKind` id kept in the save), elites 3,
  named fighters 2, gods 6, the last champion 8. The playtest tops both up at a rest (`PlaytestMode.RestShards`,
  `RestMemories`).
- **Take**: the element is owned for good (`SaveData.OwnedElements`), the god's weapon owned and its bough opened,
  the god's Heresy left to pick up, the Signature given. Nothing is forgotten (the old `Forget` and refund go).
- **Consume**: `ConsumeOffer` rules list the offers for each owned element (a free Devotion mark, a free branch mark,
  the deep Heresy) and pay about 12 Shards and some Memories; a small reading-box choice in `GodReward`. Consume also
  owns the god's plain weapon kind where the god's weapon is a kind no other weapon is (the War Fan).

### 3.13 The death rule

- **Rules**: `Growth/ShardKeeper.cs` (plain): `OnDeath(carried, killerKey, safeGround, cause)` returns the new
  `LostShards { Amount, Scene, KillerKey, Position }` and forfeits any earlier one (dying again before reclaiming loses
  it for good). `KillerKey` is the scene name plus the enemy's index in `AreaDirector.Enemies` (as `EmberDebt` keys them
  now). A fall, a hazard, or a killer that has no key (a summoned add) leaves them at `safeGround`, from `FallCatch`'s
  last firm ground. `Reclaim()` on killing the keyed enemy or walking over the spot.
- **Adapter**: `ShardDrop` (on the `AreaDirector`) re-marks the killer with a glow each time the scene or a rest
  revives it, and lays the pickup at the safe ground. `LandReturn` keeps its elite and god rules and drops its embers
  rule; `EmberDebt` is retired. The Hoard's Heresy sets `ShardKeeper.Carried = false` (lost for good).
- **The playtest**: `PlaytestMode.On` skips it (a death loses nothing).

### 3.14 The hub armoury

- `ArmouryStand` replaces `LoadoutChoice` in the hub: a stand for each owned weapon kind (with its aspects and Bond
  rank shown) and a brazier for each owned element. Choosing writes `Loadout.Weapon` (and, if the owner wants elements
  chosen here as well as in the Hand, the attunements: an open question below).
- **The route**: `RestMenu`'s "To the hub" saves `LastRest { Scene, RestId }` and loads the hub through `SceneRoutes`
  (Phase 0); the hub's way out returns to that rest fire. `RestPoint` gains a stable `RestId` set by its builder.
- The Proving Ground's rack and altars (Phase 0) are the playtest's armoury and stay as they are; in the playtest
  every weapon and element counts as owned.

### 3.15 One frame, in order

`PlayerController` already settles the player in one order. After this work: input (`LayeredButtons`) ->
`PlayerDefence.Advance` (asks the `RuleBook`) -> `PlayerElement.Advance` (the surge, two attunements) ->
`PlayerSpells.Advance` (slots, strikes, cooldowns, chord) -> `PlayerCombat.Advance` (blows; the charged blade) ->
`PlayerUpgrades.Update` (runner, `Flush`). Reactions raised during blows are acted on in `PlayerReactions.LateUpdate`
or the next frame's flush, never inside the blow that caused them.

---

## 4. The UI

The project's UI is uGUI with legacy `Text`, built by code (`PlayerBuilder` for the HUD; `GuidePage` builds itself the
first time it is wanted). New screens follow `GuidePage`: made by code at runtime under the HUD, read at the player's
own pace, the stick, d-pad, keys, mouse and wheel all work, the dodge button backs out, the world stops.

### 4.1 The rest menu and the Hand

- **`RestMenu`** (opened by `RestPoint.Use` instead of the chart): Set the Hand, The chart (Shards), The Way
  (Memories, from Phase 5), To the hub (Phase L), Leave. A plain `RestChoices` class like `PauseChoices`, for tests.
- **`HandMenu`**: the four slots in a diamond laid out like the buttons (reading their effective bindings), the surge
  slot in the middle, the two attunements above, the Heresy slots below (from Phase 6). Choosing a slot lists only
  what can go there, grouped by element, each with its role icon (five new pictures in `UpgradeIcons`) and one plain
  line; the details toggle shows numbers and the `MarkDetails` text; the Shape of a chosen spell is switched here.
  Saved Hands one to three on the shoulder buttons; "Suggested" fills `ElementSteel.StarterHand`. Unopened slots are
  drawn shut with what opens them.
- **Phase 1's stopgap** (before the Hand exists): on the chart, pressing use on a learned spell puts it in the next
  slot (West, North, East, South, round again) and the HUD shows it; the base spell starts in West.

### 4.2 The chart

- **Layouts**: `UpgradeMenu` takes its places from an `IChartLayout` instead of calling `UpgradeTree.OnChart` directly:
  `RingLayout` (today's ring, the trees with spell branches), `HeartLayout`, `WayLayout`. The stick's `UpgradeTree.Step`
  works on whatever places a layout gives.
- **Spell branches** are drawn as short stalks; the Shape mark's frame is split in two and shows the chosen half lit;
  the role icon sits on the Learn mark.
- **Duo branches in the heart**: the ring's middle (inside the emblems at 9 steps) is empty today. The duo branch of
  the two carried elements is drawn there, upright, in today's mix-branch slots (x1 to xs); the playtest's free chart
  adds a pair picker (shoulder buttons cycle the partner) so all 55 can be walked. The 11 ring gaps go empty, so all 55
  branches look alike. Reactions are not drawn; the heart shows the pair's reaction name over its branch.
- **The Way** is its own chart in the same screen (`WayLayout`): one weapon at a time, the trunk up the middle
  (temper, infusion, the technique sealed until its elite falls, the Bond mark at the top), three boughs fanning out
  (four with a god's bough), the Body branch at the foot. The shoulder buttons change weapon in the playtest. The
  active aspect is chosen here at a rest fire.
- `MarkDetails` (Phase 0) shows every new kind; the guide button opens the entry for the trigger or affliction named.

### 4.3 The HUD

- **`SpellDiamondHud`** (self-building, beside `PlayerHud`): the four slots in the button diamond, each with its spell's
  picture, a cooldown ring (an `Image` with radial fill, set from `SpellCooldowns.Fraction`), a flash on `Ready`, and a
  mana price where it has one. Shown large while the layer is held (the design) and small at rest in a corner (a
  proposal: a player should see cooldowns without holding L2; easy to turn off). The two strike buttons sit under it
  with their rings; the surge in the middle.
- **The charged-blade sign**: three pips in the element's colour by the diamond, spent one a blow, and the weapon's
  own `BladeLook`.
- **Enemy pips**: `EnemyBars` already draws affliction diamonds; it draws every kind present (two or more) side by
  side. Reactions show their name briefly over the enemy.
- **Prompts** name buttons through `ControlNames`, so a rebound slot reads right.

### 4.4 The guide and the details

`effects.md` gains groups and entries; `whats_new.md` a section each phase; `marks/*.md` per tree as rewritten; the
codex is a `GuidePage` group fed at runtime. Every text change is followed by `Clover > Build > Playtest Texts`.

---

## 5. Saves

### 5.1 The new schema

`Runtime/Core/Save/SaveData.cs` (plain, `[Serializable]`, written with `JsonUtility`, so lists of pairs instead of
dictionaries), stored by `SaveStore` at `Application.persistentDataPath/clover_save.json`, written to a temporary file
and swapped in, at rest fires, scene changes, god rewards and quitting.

```csharp
[Serializable] public class SaveData
{
    public int Version = 1;
    public List<string> Flags;            // what Progress.Has/Unlock kept in PlayerPrefs: doors, ladders, walls, met.sponsor, elites, gods
    public List<string> OwnedElements, OwnedWeapons;
    public List<string> Learned;          // mark ids bought or given
    public List<string> Found;            // Evolutions discovered, Body pieces picked up, Heresies picked up
    public List<string> SeenReactions, KindsDefeated;
    public Wallet Wallet;                 // Shards, Memories, ShardsBanked
    public LostShards Lost;               // the death rule's one pile, or none
    public Loadout Current;  public Loadout[] SavedHands = new Loadout[3];
    public List<IdPoints> Bond;           // "weapon:katana", "element:Fire", "pair:katana/Fire"
    public List<IdCount> Tallies;         // marks' kept counts (RunState.Tallies today)
    public RestRef LastRest;              // scene and rest id
    public int GodsFelled;
}
```

- `SaveStore` honours `Progress.Sandboxed` (probes play in memory and never touch the real file: the 2026-11-16
  rule that probes must not wipe saved progress) and `PlaytestMode` (the playtest keeps its own file,
  `clover_playtest.json`, so a tester's game proper is never mixed with the free chart).
- `Progress` keeps its API and stores flags in `SaveData.Flags`; `RunState` is retired, field by field, as the task
  that owns each field moves it (`Points` to the wallet, `Unlocked` to `Learned`, `ElementsHeld` and `ElementConsumed`
  gone, `WornSpell` to `Loadout.Slots`).
- Bindings and the outfit stay in PlayerPrefs as now (`ControlBindings`, `Outfits`): they are settings, not progress.

### 5.2 Existing players

What exists on a tester's machine was checked in code: **there is no run save**. `RunState` is memory only, so the
old playtest's runs, marks and embers end with the game. What is on disk is PlayerPrefs: the progress flags
(`clover.progress.weapon.<name>`, `element.<name>`, `met.sponsor`, and the world keys of doors, ladders, walls and the
land's waking line), the bindings JSON and the outfit. `SaveMigration.FromPrefs()` runs once when no save file exists:
it copies every `clover.progress.*` flag into `Flags`, turns `weapon.*` and `element.*` into owned weapons and
elements, sets `Version = 1`, writes the file, and leaves the PlayerPrefs keys alone (so an old build still runs).
Later versions chain `Migrate(n)`; a learned id found in `RetiredMarks` is removed and its price refunded.

The bindings JSON survives the input change because overrides are keyed by action id: renaming `Spell` to
`WeaponSkill` keeps its id. A saved override that points at an action that no longer exists is dropped by
`ControlBindings.Load`'s existing catch.

---

## 6. Testing

### 6.1 EditMode tests (written before the code, per `feature-loop`)

| Suite (in `Tests/EditMode/Player` or `Gameplay`) | What it proves |
|---|---|
| `SpellCooldownTests` | Start, tick, cut every running one, reset, instant next, frozen; cuts by move, guard, dodge, finisher; Old Magic and Given Fire modes |
| `ManaSettleTests` | Rises to half out of a fight, never above it, keeps what is above; fight clock ends after 8 s unaware |
| `ChargedBladeTests` | Three blows, 1.2 times, third reported; a new charge replaces; strikes and effects never charge; Edge modes |
| `SpellLayerRulesTests` | Slot from pad only with the layer, from keyboard without; face presses swallowed while held; surge on a short tap only; strikes on R1 and R2 with the layer |
| `ChordRulesTests` | Heavy within 0.08 s of light in early wind-up is the chord; later it is not |
| `HandRulesTests` | Choices per slot (carried only, duo with both, at most 13 in a full case), validate drops and falls back, slots open on the schedule, all open in the playtest, Heresy exclusion by tag |
| `InForceTests` | Each rule of 3.6, table-driven |
| `AfflictionPairsTests` | 55 unique pairs, symmetric index, distance on the ring |
| `ReactionEngineTests` | Fires when a partner is present; rests 4 s per enemy and pair; two partners give two; reaction-laid starts nothing unless cascading; budget defers; Burn through `TakeFrom` reacts |
| `EnemyConditionTests` | Slots, beats, end, flags read, refresh rules |
| `ReactionCatalogueTests` | All 55 pairs have exactly one reaction mark; every shape names a real reaction; every lift names an element |
| `UpgradeChartTests` (extended) | New kinds' prices; spell branch wiring (Learn, Empower, Shape, Master share `Branch`, grow in order); the new pattern layout; heart layout; Way layout; hidden marks never drawn; currency per tree |
| `WayRulesTests` | Aspect marks sleep off their aspect; gates; spent per Way; temper sums |
| `ArmouryTests` | `Resolve` returns the god's weapon for a god bough and a patched plain weapon otherwise; every aspect patch keeps every step's animation name known |
| `BondLedgerTests`, `PairStrikeTests` | Points to ranks; losing a rank; footprint from each carrier; each verb's actions |
| `EvolutionRulesTests`, `RuleBookTests` | Found only with Master and partner; fall back; each Heresy's switches |
| `WalletTests`, `ShardKeeperTests`, `SaveDataTests` | Income; death, second death, reclaim, fall to safe ground, playtest exempt; round trip, migration from prefs flags, retired ids refunded |
| `ControlsAssetTests`, `ControlBindingsTests` (updated) | The two maps and their actions; per-map clashes |
| `PlaytestTextsTests` | Every high-level mark has details (a report), every details entry names a mark (a failure) |

### 6.2 Play-mode probes (AgentScripts, run through the `playmode-probe` loop, sandboxed)

| Phase | Probe | What it shows |
|---|---|---|
| 1 | `LayerProbe` | Hold the layer and press each face button: the slot casts, nothing else does (no flask drunk, no dodge); a short tap lights the surge; a long hold does not |
| 1 | `CooldownProbe` | Cast, watch the ring, land three blows and a perfect guard, the cooldown shortens by the right amount; mana unchanged by a basic cast |
| 1 | `BladeProbe` | A landed spell charges, three blows lay the affliction and hit 1.2 times, the sign shows (screenshot per element) |
| 1 | `StrikeProbe` | L2 with R1 on each weapon with Fire: the carrier plays, the footprint burns, the 8 s ring runs |
| 2 | `HandProbe` | Rest, open the Hand, set two elements and four slots with the pad and the keyboard, leave, cast all four; saved Hands swap |
| 3 | `ReactionProbe` | Fire and Water on a dummy: Steam fires once, not again within 4 s; a reaction's burn does not react |
| 3 | `CrowdProbe` (performance) | Twelve enemies, two elements, every slot cast on cooldown for 30 s; `diagnostics profiler_record` and `profiler_summary`: frame under 16.6 ms, steady GC zero |
| 4 | `BranchProbe` | Buy Learn to Master on a Fire branch, swap shapes at the fire, the Master's timing works |
| 5 | `WayProbe` | Buy the Katana's three aspects, switch at the fire, each chain plays; Storm Katana bough swaps model and moves |
| 6 | `HeresyProbe`, `EvolutionProbe`, `BondProbe` | A Heresy's rule is off and its sign shows; a recipe found at rest; Bond rank rises after a scripted fight |
| L | `DeathProbe` | Die to a soldier carrying 10 Shards: it glows, killing it returns them; die again first: gone; fall: at the last firm ground. Must not touch the real save |

### 6.3 Playtest mode

Every probe runs once with `PlaytestMode.Set(true)` and once without where the rule differs (rest top-ups, slots all
open, free chart, death costs nothing, Heresy and Evolution lists). Each phase ends with the playtest build smoke test
(`playtest-build` skill on `Clover > Build > Combat Playtest`), a "What's new" entry, and screenshots of the new
screens added as `ShotStation`s where the screen is stable enough to have a golden frame.

---

## 7. The build plan

Each task is one worker session. **P** marks a task whose code can be written without the editor (rules classes and
their tests, data files, docs): several can be written at once in worktrees, but each still needs the editor to
compile and run its tests, so their verification runs one at a time. **E** marks a task that needs the editor for
most of its work (scene or builder changes, play probes, screenshots, animation); only one runs at a time while
another agent holds the editor.

**Before Phase 1**: Phase 0's uncommitted changes touch `PlayerUpgrades`, `UpgradeMenu`, `PauseMenu`, `PauseChoices`,
`RestPoint`, `GodReward`, `RunEnder`, `LandReturn`, `ReturnRule`, `SceneDoor` and the builders. Tasks that edit those
files wait for Phase 0's commit; tasks marked "after Phase 0" below.

### Phase 1: spells on cooldowns, mana for big moves, the charged blade, infused strikes, the L2 layer

| # | Task | Files | Acceptance | Needs | Kind |
|---|---|---|---|---|---|
| 1.1 | Magic rules | New `Player/Magic/SpellCooldowns.cs`, `CooldownCuts.cs`, `FightClock.cs`, `ChargedBlade.cs`, `SpellLayerRules.cs`, `ChordRules.cs`; `ManaPool.Settle`; tests `SpellCooldownTests`, `ManaSettleTests`, `ChargedBladeTests`, `SpellLayerRulesTests`, `ChordRulesTests` | All new tests pass; no `UnityEngine.InputSystem` in the rules; statics none | nothing | P |
| 1.2 | The layer in the controls | `Controls.inputactions` (Spells map, rename `Spell` to `WeaponSkill`, pad `Surge` binding removed); new `Player/LayeredButtons.cs`; `PlayerController`, `PlayerCombat`, `PlayerDefence`, `PlayerInteraction` read through it; `ControlsPage` Spells rows; `ControlNames`; `ControlsAssetTests`, `ControlBindingsTests` | Tests pass; `LayerProbe`; rebinding a slot on pad and keyboard is kept after a restart; weapon skill on Triangle / Y / Q | 1.1 | E |
| 1.3 | Four slots and cooldowns in play | New `PlayerSpells.cs`; `PlayerUpgrades.Cast` (from `TrySpecial`), new events on `PlayerCombat`/`PlayerDefence`; `SpellSpec.Cooldown`, `SpellRole`; `Trees/SpellTimings.cs` (cooldown or mana for every built spell id from `elements_a/b.md` rosters, and the base spells); hidden `<element>.base` marks (`Tree = Hidden`); `PlayerElement` loses the tap/hold split; `UpgradeMenu` stopgap "put in next slot"; `SpellTimingsTests` (every spell has a cooldown or a price, not both) | `CooldownProbe`; the yard's lent spells still cast | 1.1, 1.2, after Phase 0 | E |
| 1.4 | Mana for big moves | `ManaPool`, `PlayerCombat` (start at half), `PlayerSpells` (settle with `FightClock` fed from `AreaDirector`); surge, larger spells, skill keep their prices | `ManaSettleTests`; probe: after a fight 10 rises to 50 and stops, 80 stays | 1.3 | E |
| 1.5 | The charged blade | `PlayerCombat` blow path; `Hit` second affliction and `AfflictionSet.TakeFrom` (the five `TakeHit`s); `Trees/ElementSteel.cs` (the eleven third-blow extras); new `BladeLook.cs`; triggers `ChargedBlow`, `ChargedBlowThird` | `ChargedBladeTests`; `BladeProbe` screenshots for Fire, Lightning, Ice read as the element | 1.3 | E |
| 1.6 | Infused strikes | `Weapon.Carrier` in every `Weapons` factory; `Footprint.cs`; `HazardShape.Footprint`, `EffectAt.Footprint`; `ElementSteel` strikes and deep strikes for all eleven; `PlayerSpells` strike timers; `WeaponCombo` start through `Follow` | `StrikeProbe` on all ten weapons with two elements; strike never charges the blade; deep strike costs 20 | 1.3, 1.5 | E |
| 1.7 | The HUD | New `UI/SpellDiamondHud.cs`, five role pictures in `UpgradeIcons`, `UiArt` if sprites are generated | Screenshot with the layer held and released, pad and keyboard prompts; zero allocation per frame (profiler) | 1.3 (1.5, 1.6 for the pips and strike rings) | E |
| 1.8 | Words, guide and release | `effects.md` (cooldowns, cuts, mana, the blade, strikes, deep strike, the layer), `whats_new.md`, `docs/GUIDE.md` controls, `docs/GAME.md` record; Playtest Texts rebuilt; `Phase1Probe` running the whole loop | Playtest build smoke-tested; texts test passes | 1.2 to 1.7 | E |

1.1 can start now, in parallel with Phase 0. The words of 1.8 can be drafted (P) alongside 1.5 to 1.7.

### Phase 2: two elements and the Hand

| # | Task | Files | Acceptance | Needs | Kind |
|---|---|---|---|---|---|
| 2.1 | Loadout and Hand rules | New `Player/Build/Loadout.cs`, `HandRules.cs`, `SlotSchedule.cs`; `MarkRules.InForce`; tests `HandRulesTests`, `InForceTests` | Tests pass | 1.3 | P |
| 2.2 | Two attunements | `PlayerElement` (two kits, `Attune`, surge slot's element), `NewCast(spec, name, source, element)`; audit of the 113 reads of `Info`/`Kit`/`ElementName`; the runner's default affliction from the mark's element; only the first element's root lays on weapon blows (owner's decision; the second comes from spells, the charged blade and infused strikes); `PlayerUpgrades.Refresh` on `InForce`; new `PlayerBuild.cs` | Probe with Fire and Lightning: each spell lays its own, colours right, blows lay only the first element; all existing EditMode suites pass | 2.1, 1.5 | E |
| 2.3 | The rest menu and the Hand screen | New `UI/RestMenu.cs`, `RestChoices.cs`, `UI/HandMenu.cs`; `RestPoint.Use`; saved Hands; starter Hands from `ElementSteel` | `HandProbe` on pad and keyboard; screenshots; `RestChoices` tests | 2.1, 2.2, after Phase 0 | E |
| 2.4 | The Proving Ground and words | Proving Ground altars set either attunement; slot schedule wired to the Champion of the Gate and the Fire god flags; `effects.md`, `whats_new.md`, `GUIDE.md` | Playtest build smoke; probe that slots open on the flags outside the playtest | 2.3 | E |

### Phase 3: reactions

| # | Task | Files | Acceptance | Needs | Kind |
|---|---|---|---|---|---|
| 3.1 | The engine | `AfflictionSet` (`PresentMask`, `ReactionRest`, `Lay`, `LaySource`); new `Gameplay/Reactions/AfflictionPairs.cs`, `ReactionEngine.cs`; `CombatEvents.Reacted`; `HazardSpec.FromReaction`; the five `TakeHit`s already on `TakeFrom` | `AfflictionPairsTests`, `ReactionEngineTests`; existing `AfflictionTests` pass | 1.5 | P |
| 3.2 | Reactions as marks, and the first six | `UpgradeKind.Reaction`, `MarkTree.Hidden`, trigger `Reacted`, conditions `Reaction`, `Cascades`, action `ForceReaction`, `Flinch`; `PlayerUpgrades.Fire`; new `PlayerReactions.cs`, `ReactionTable.cs`; `Trees/Reactions/Reactions.cs` with Flashover, Steam, Carried Embers, The Ground Gives, Live Iron, Rot Roots (the design's six, each at its simplest version with `// NEEDS` where an engine piece is missing); status word over the enemy | `ReactionProbe`; `ReactionCatalogueTests` for what exists | 3.1, 2.2 | E |
| 3.3 | Enemy conditions | New `Gameplay/Reactions/EnemyConditions.cs`; `UpgradeAction.Condition`; triggers `ConditionBeat`, `ConditionStruck`, `ConditionEnded`; reads in `EnemyBrain` and the gods' choosers | `EnemyConditionTests`; probe: a rooted soldier attacks but does not move | 3.1 | E |
| 3.4a-d | The other 49 plain reactions | `Reactions.cs` (or split by distance), the ledger `content_open.tsv`; one session per pair file: d1-d2 (minus the six), d3, d4, d5 | `ReactionCatalogueTests`: all 55 | 3.2, 3.3 | P (writing), verification serial |
| 3.5 | Readability and words | `EnemyBars` (every kind present), reaction name and codex seen; `effects.md` "Reactions" (55 entries) | Screenshots of three reactions; texts test | 3.2 | E |
| 3.6 | Performance | `CrowdProbe`; capacities (`Hazards.Capacity`, runner queue, `ReactionEngine.MostPerFrame`) set from the profile | Frame under 16.6 ms on the crowd; zero steady GC | 3.4 | E |
| 3.7 | Knocked down | Enemy brain state and clip (Forge), `Condition` flag `Down` | Probe: Capsize knocks a soldier flat and it stands | 3.3 | E |

No release after Phase 3: the owner moved the next playtest build to after Phase 6 (2026-11-19).

### Phase 4: trees reshaped into spell branches (Fire and Lightning first, Devotion, the first new duo branch)

| # | Task | Files | Acceptance | Needs | Kind |
|---|---|---|---|---|---|
| 4.1 | Schema and layout | `Upgrade` fields (2.1), kinds and prices, `ShapeDef`, `Empower`, `Trees/SpellBranch.cs`, conditions `OwnBranch`, actions `CutCooldowns`, `ResetCooldown`, `InstantNext`; `IChartLayout` and the spell-branch pattern layout; `UPGRADE_SCHEMA.md` section 20; `RetiredMarks.cs` | `UpgradeChartTests` extended, pattern test for the new layout | 2.1 | P |
| 4.2 | The Fire tree | `FireTree.cs` rewritten from `elements_a.md`, `ElementSteel` Fire checked, `marks/fire.md`, ledger | Chart tests; `BranchProbe` on Fire | 4.1 | P, then E for the probe |
| 4.3 | The Lightning tree | `LightningTree.cs`, `marks/lightning.md` | As 4.2 | 4.1 | P, then E |
| 4.4 | Devotion | `UpgradeKind.Devotion`, `InForce` all in, `RuleBook` (first fields), Fire Without End, In One Breath, deep strikes on R2 all in | Probe all in on Fire and on Lightning | 4.2, 4.3 | E |
| 4.5 | Duo branches and the heart | `HeartLayout`, `MarkRules.Open` for any pair, duo spells in the Hand, reaction shapes in the `Loadout` and `ReactionTable`; Plasma reshaped (`pairs_d1_d2.md`) and one new branch (Ice and Lightning, Glass Storm) | Chart tests for the heart; probe: shape swap changes the reaction | 4.1, 3.2 | E |
| 4.6-4.14 | The nine other trees | One tree per session from `elements_a/b.md` | As 4.2 | 4.1 | P, then E |
| 4.15-4.19 | The other duo branches | By pair file, about eleven a session | Chart tests | 4.5 | P, then E |

### Phase 5: the Way, Memories and the Body branch

| # | Task | Files | Acceptance | Needs | Kind |
|---|---|---|---|---|---|
| 5.1 | Way rules and the wallet | `Currency`, `Aspect`, `Gate` in `MarkRules`; new `Growth/Wallet.cs`, `Growth/Income.cs`, `Way/WayRules.cs`; `PlaytestMode.RestMemories` read | `WayRulesTests`, `WalletTests` | 4.1 | P |
| 5.2 | Aspects and the armoury resolve | New `Way/WeaponAspect.cs`, `Way/Armoury.cs`, `AspectRun`; Katana's Sheath, Stream, Answer and the Storm Katana bough; `Trees/Ways/KatanaWay.cs` | `ArmouryTests`; `WayProbe` on the Katana | 5.1 | E (moves need clips; first pass retimes existing clips) |
| 5.3 | Temper, infusion, the technique chord | Trunk marks; `Way/Infusion.cs`; `WeaponCombo.Replace`; Hold the Gate (Great Spear) gated on `elite.champion_gate`; the Champion uses it first (already his move per the design: check) | Probe: chord plays the technique only after the gate | 5.2, 1.6 | E |
| 5.4 | The Body branch | `Trees/Ways/BodyWay.cs`; stats `StaminaMost`, `Poise`; found pieces as pickups | Probe: health and flask rise | 5.1 | E |
| 5.5 | The Way chart | `WayLayout`; `RestMenu` entry; `marks/way/katana.md` | Screenshots; stick reaches every mark | 5.2 | E |
| 5.6 | Memories income | First defeat per kind, elites, gods, named | Tests; probe | 5.1 | P, then E |
| 5.7+ | The other nine Ways | One Way per session (`weapons.md`), aspects that need new clips flagged and ledgered | `ArmouryTests` | 5.2 | E |

### Phase 6: Bond, Heresies and Evolutions

| # | Task | Files | Acceptance | Needs | Kind |
|---|---|---|---|---|---|
| 6.1 | Bond | New `Growth/BondLedger.cs`, `BondTracker.cs`, `Trees/BondTable.cs`; Bond marks; armoury shows ranks | `BondLedgerTests`; `BondProbe` | 5.1 | P, then E |
| 6.2 | Pair Bond and pair strikes | New `Growth/PairStrike.cs` (eleven verbs), mirrored `Echo`; two standouts first (Katana and Lightning, Great Spear and Fire) | `PairStrikeTests`; probe | 6.1, 1.6 | E |
| 6.3 | The RuleBook and Heresy slots | `RuleBook` complete; `IHeresy`; Hand's Heresy slots and exclusion; `HeresyLook`; the switch Heresies first: Old Magic, Dry Flask, Unwearied, Given Fire, One Word, The Edge Drinks | `RuleBookTests`; `HeresyProbe` | 2.3, 4.4 | E |
| 6.4 | Evolutions and the codex | New `Growth/EvolutionRules.cs`; rest-time check and reveal; codex group in `GuidePage`; The Winged Sun and Plasma Lance first | `EvolutionRulesTests`; `EvolutionProbe` | 4.2, 4.3, 5.2 | E |
| 6.5+ | The other Heresies and Evolutions | A handful a session; the hardest last (8.4) | Ledger shrinks | 6.3, 6.4 | E |

### Track L: the linear game (outside the playtest, which keeps "a death costs nothing")

| # | Task | Files | Acceptance | Needs | Kind |
|---|---|---|---|---|---|
| L.1 | The save | New `Core/Save/SaveData.cs`, `SaveStore.cs`, `SaveMigration.cs`; `Progress` as a facade; `RunState` fields moved as their owners move | `SaveDataTests` (round trip, migration, sandbox) | nothing | P (can start alongside Phase 1) |
| L.2 | Take and Consume | `GodReward` (own for good, god's weapon and bough, Heresy pickup), `ConsumeOffer` | Probe on the summit and the sun court | L.1, 2.2 | E |
| L.3 | The death rule | New `Growth/ShardKeeper.cs`, `World/ShardDrop.cs`; `LandReturn`; `EmberDebt` and `RunEnder` retired | `ShardKeeperTests`; `DeathProbe` | L.1 | E |
| L.4 | The hub armoury and the route | `ArmouryStand` (from `LoadoutChoice`), `RestMenu` "To the hub", `RestPoint.RestId`, `HubBuilder` | Probe: rest, hub, change weapon, back at the same fire | L.1, 2.3, 5.2 | E |

---

## 8. Risks and open technical questions

### 8.1 Performance

- **Reactions are cheap to find and dear to show.** Detection is a mask test. What they lay is the cost: ground,
  forms and effects, all with fixed capacities (48 hazards, 24 form runs, 64 effects a frame). With two roots laying
  on every blow, every enemy in a crowd reacts once each 4 s. **Simplest version**: the per-pair rest, the no-cascade
  rule and an engine budget (8 a frame) are all in from task 3.1; a reaction's look is one burst or one patch, never a
  lasting form, unless its entry needs one. Task 3.6 measures before anything is raised.
- **Sun City has hundreds of enemies** (`LandReturn` stops animating far ones). The reaction rest is 220 bytes an
  enemy; conditions six slots an enemy. Both fixed, allocated with the enemy.
- **The HUD** must not allocate per frame: cooldown rings set fill amounts only; texts change only on events.

### 8.2 Readability

Four cooldowns, two afflictions on each enemy, reactions, a charged blade and a surge is a lot to read from above.
The design's answers are the role icons, the diamond and the guide; the plan adds the reaction's name as a short word
over the enemy and the always-visible small diamond (both easy to turn off). Each reaction's look must read as its
elements (`element-effects-read-as-element`), which is 55 looks: the simplest version reuses each element's
`ElementLook` pieces in the two colours and saves bespoke looks for the reactions played most.

### 8.3 Size of the content

About 1,250 entries: eleven trees of about 34 (374), 55 duo branches (about 330), 55 reactions with 110 shapes, ten
Ways (about 300) and the Body, 51 Heresies, 22 Evolutions, twelve pair standouts, eleven techniques. Many need engine
pieces README lists (about twenty enemy states, downwind, side of a blow, second and held presses, standing things
that block, links between enemies). **Simplest version**: write every entry at once as data at its nearest working
version, ledger what is approximated (`content_open.tsv`), and build engine pieces in the order of how many ledger
lines each would close (README's "most needed": enemy conditions, knocked down, downwind, the second press). The
writing (P) parallelises well; each piece of engine does not.

### 8.4 Hard as written

| Piece | Why it is hard | Simplest version that keeps the intent |
|---|---|---|
| Aspects that add moves (the Stream's five cuts, Half-Sword, the Pole's vault) | New animation clips from the Forge, animator rebuilds, editor-bound | First pass retimes and reorders existing clips; new clips are a ledgered art task per Way |
| God boughs that swap the weapon | Only the Storm Katana, the Sun-Spear and the Armament exist; the others are "Not built yet" | `GodBough` aspects that point at missing factories show the Phase 0 "Not built yet" slot |
| The L2 tap as surge | The surge waits for the release (up to 0.25 s later) | Accept the delay; or light the surge on press and cancel it if a layer button follows within 0.15 s (worse to read). Decide in the 1.2 probe |
| The R1 and R2 chord | Must not delay light attacks | Cancel the light's first 0.08 s into the technique (3.5) |
| Both roots on every blow | Every enemy would react each 4 s from melee alone | Resolved by the owner (2026-11-18): only the first element's root lays on blows; reactions come from weaving weapon and spell |
| Count the Stones, The Endless Slope, The Hour Freezes, The Eye Above, Ridden | Each replaces a whole system (health, movement, time, camera, enemy allegiance) | Build last; each is its own task; the `RuleBook` gives them a place but not their code |
| Three Bloods | Breaks the two-attunement model | `Loadout.Attunements` sized by the `RuleBook` from the start |
| Afflictions on the player (Given Fire, The Last One in the Jar, All Turns to Venom) | The player carries no `AfflictionSet` today (`UPGRADE_SCHEMA.md` 14) | Give the player an `AfflictionSet` read by `PlayerDefence`, only when the first such Heresy is built |
| Knocked down, Hooked, Disarmed | Brain states and clips for every enemy kind | `Condition` flags with a shared stagger-like clip first |
| The killer carrying Shards when it is a god or a pen enemy | Gods' fights reset; Proving Ground pens respawn | Gods and unkeyed killers leave them at the safe ground instead |
| 55 duo branches on the ring | There is no gap for non-neighbours | The heart view (4.2) |

### 8.5 Open questions for the owner (the plan's default in brackets)

1. **Answered (owner, 2026-11-18)**: the carried elements and the weapon are picked in the hub; the Hand at a rest fire
   sets the four spells and the surge from those two. The Hand no longer changes attunements.
2. Which element a duo spell charges the blade with. (Its first element.)
3. With one element in Phase 1, what L2 with R2 does. (The deep strike, so testers try it before Devotion exists.)
4. The diamond always shown small, or only while L2 is held. (Small always, large while held; used as the default.)
5. Bond rank thresholds and the element ranks' numbers: not in the design. (A first guess in `BondTable`, ledgered;
   used as the default, tuned in playtesting.)
6. **Answered**: the spare ring-mix spells are Evolution results, as `depth_elements.md` has them (approved with the
   rest).
7. **Answered (owner, 2026-11-18)**: weapon blows lay only the FIRST element's affliction; the second comes from
   spells, the charged blade and infused strikes, so reactions come from weaving. Build it this way from the start.

### 8.6 Process risks

- **`PlayerUpgrades` contention.** Phase 0 is editing it now and most phases touch it. Keep every phase's edits there
  to small hooks; put new behaviour in new classes; commit Phase 0 first.
- **Statics and domain reload.** New rules are instance state on the player; any static (the `RuleBook` is not one)
  gets its `SubsystemRegistration` reset, and `ConventionsTests` is extended to look for new statics without one where
  a scan can.
- **Ids.** A mark id is never changed once a save holds it; cut ones go to `RetiredMarks`. Rewriting a tree keeps every
  surviving mark's id.
- **Enemies are tuned for a player with little magic** (`STEEL_AND_ELEMENT.md` 12). The plan builds no retuning; after
  Phase 3's playtest, a retuning pass per land is its own work.
