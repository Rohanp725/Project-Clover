# The upgrade schema: how to write a tree file

Written by Claude on 2026-11-16 (day run), for whoever writes an element's tree as data. Everything here matches the
code under `Assets/_Project/Scripts/Runtime/Player/` as of that day. If the code and this page disagree, the code is
right and this page has a bug: fix the page.

A tree file is one C# file of object initialisers. It needs no new code as long as it keeps to the kinds, triggers,
actions, stats, pictures and spell forms listed here. The EditMode suite `UpgradeChartTests` checks every rule on this
page that a machine can check (ids, costs, what grows from what, where a mark may sit, that the stick reaches it, that
the file is registered), so a file written blind is proved or refuted by `unity command test run -- --mode editor
--filter UpgradeChartTests`.

## 1. Files and registration

| What | Where |
|---|---|
| An element's tree | `Assets/_Project/Scripts/Runtime/Player/Trees/<Element>Tree.cs`, class `<Element>Tree`, namespace `Clover.Player.Trees` |
| A mixed branch | `.../Trees/<MixName>Tree.cs`, class `<MixName>Tree` (`PlasmaTree`, `TempestTree`, ...) |
| The registry | `.../Trees/UpgradeTrees.cs`: add one line `XTree.Add(to);` in `AddAll`, in ring order, each mix after its first element |

Every tree class is `public static class` with `public static void Add(List<Upgrade> to)`. A file that is not listed
in `UpgradeTrees.AddAll` fails the test `EveryTreeFileIsGatheredIntoTheCatalogue`.

Usings a tree file needs: `System.Collections.Generic`, and `Clover.Gameplay` if it names an `AfflictionKind`.

## 2. The ring

Eleven elements, clockwise from straight up, 360/11 degrees apart. Use the constants in `Elements`
(`Elements.Fire`, `Elements.Lightning`, ...), never the bare strings.

| # | `Elements.` | Affliction (`AfflictionKind.`) | Mixes with the next: name |
|---|---|---|---|
| 0 | Fire | Burn | Lightning: Plasma |
| 1 | Lightning | Static | Wind: Tempest |
| 2 | Wind | Unsteady | Ice: Blizzard |
| 3 | Ice | Chill | Water: Glacier |
| 4 | Water | Soaked | Life: Bloom |
| 5 | Life | Seeded | Earth: Overgrowth |
| 6 | Earth | Tremor | Metal: Forge |
| 7 | Metal | Sundered | Light: Halo |
| 8 | Light | Marked | Dark: Eclipse |
| 9 | Dark | Cursed | Poison: Blight |
| 10 | Poison | Poison | Fire: Pyre |

### What each affliction does (numbers in `Gameplay/Afflictions.cs`)

All are laid in stacks, last some seconds, and start their time over when laid again. "Per stack" multiplies by depth.

| Affliction | Most stacks | Seconds | What it does |
|---|---|---|---|
| Burn | 5 | 4 (tops up, 8 at most) | 5 damage a second, +30% a stack. Set off: 10 + 10 a stack. A burn can be laid with seconds and no depth |
| Static | 5 | 8 | nothing by itself; set off for 7 a stack. (Lightning's own root discharges it for 7 damage and 5 stagger a stack in 4 m on a heavy blow) |
| Unsteady | 3 | 6 | +15% stagger taken, shoved 40% further, turns 15% slower, per stack |
| Chill | 5 | 6 | 12% slower per stack. At 5: **Frozen** for 3 s (1.2 s for elites and bosses, never the colossus): held still; a heavy blow shatters it for 45 and ends the chill |
| Soaked | 3 | 8 | +10% damage from spells per stack. Static or Chill laid on it is also laid on everyone within 4 m |
| Seeded | 3 | 10 | each weapon blow on it heals the player 1 per stack. When it dies: 12 damage per stack to everyone within 3.5 m |
| Tremor | 3 | 6 | 10% slower per stack, and its balance does not drain back |
| Sundered | 3 | 8 | +10% damage from weapons per stack |
| Marked | 3 | 6 | every weapon blow on it is critical (x1.5), and gives 2 more mana per stack |
| Cursed | 3 | 8 | deals 10% less per stack. When it dies the player is healed 6 per stack |
| Poison | 5 | 8 | nothing between bursts; every 2 s a burst of 5 per stack. Set off: 9 a stack |

## 3. The `Upgrade` fields

```csharp
to.Add(new Upgrade
{
    Id = "wind.gale",              // unique, lower case, "<element>.<word>" or "<mixname>.<word>"; never change one once shipped
    Name = "Rising Gale",          // two or three plain words
    Element = E,                   // the tree it is on; for a mix, the FIRST of the two (the earlier round the ring)
    Element2 = "",                 // only for a mix: the second element (the first one's neighbour); leave out otherwise
    Kind = UpgradeKind.Major,      // see section 4
    Cost = 0,                      // leave out: it takes its kind's cost. Only Mix marks write a cost (3, 4 or 5)
    Requires = new[] { "wind.root" },   // ids it grows from; ANY ONE learned opens it. Leave out only for the Root (and a mix's first mark, and Signatures)
    X = -1.21f, Y = 2.4f,          // place in its own tree, in steps: see section 9
    Icon = UpgradeIcons.Gust,      // see section 8
    Description = "...",           // one or two short plain sentences, present tense, no exclamation marks
    Weapon = "",                   // leave out, or Weapons.KatanaName / Weapons.FistsName: works only with that weapon in hand
    Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.05f) },   // section 5
    Effects = new[] { new UpgradeEffect { ... } },                   // sections 6 and 7
    WeaponStacks = 0,              // stacks of the element's affliction EVERY weapon blow lays while learned (a Root sets 1)
    Form = "",                     // a spell form id from SpellForms (section 10); with a form the mark can be worn
    Spell = null,                  // new SpellSpec { ... }: the numbers of that spell
});
```

A mark may have stats, effects and a form all at once. A mark with none of them (and `WeaponStacks` 0) is shown and can
be bought but does nothing: that is how a **BESPOKE** mark is written until its code exists. Put `// BESPOKE: what it
must do` on the line above it and add its id as a `public const string` in the tree class, so the code that will do it
by hand in `PlayerUpgrades` has a name to check.

## 4. Kinds

| `UpgradeKind.` | Cost | Frame on the chart | What it is |
|---|---|---|---|
| `Root` | 2 | round, double line | The foot of the tree. One per element, no `Requires`, at `X = 0, Y = 1.2`. Set `WeaponStacks = 1` |
| `Minor` | 1 | small, round | A plain number: `Stats` only. One line of description |
| `Major` | 3 | square, cut corners | A new rule: one or more `Effects` |
| `Spell` | 4 | diamond | Must have `Form` and `Spell`. Worn in the hold-spell slot |
| `Keystone` | 6 | large, eight-sided, double line | The end of a bough |
| `Mix` | written: 3 to 5 | six-sided | A mark on a mixed branch. Must have `Element2`. May have a `Form` (then it is a spell and can be worn) |
| `Signature` | 0, never sold | largest, a sun, with a crown | The god's gift. No `Requires`. Tree: `X = 0, Y = 12`. Given by code (`GodReward`), not bought |

The costs of everything sold on one element's own tree must sum to more than 19.

## 5. Stats (`PlayerStat`)

`new StatMod(PlayerStat.X, amount)`. For a **share**, 0.10 means ten percent more and -0.10 ten percent less; all
amounts for one stat (from every learned mark, the surge and timed boons) are added, then applied once as
`1 + sum`, never below 0.1. A **flat** stat is a plain amount added on.

| `PlayerStat.` | Kind | What it changes | A sensible minor |
|---|---|---|---|
| `WeaponDamage` | share | damage of every weapon blow | 0.05 |
| `StaggerDealt` | share | stagger of every weapon blow | 0.08 |
| `DamageToAfflicted` | share | weapon and spell damage on an enemy carrying any affliction | 0.08 |
| `AfflictionStacks` | flat, whole | added whenever the player lays stacks (weapon root, spells, effects) | 1 (strong: keystone-sized) |
| `AfflictionSeconds` | share | how long afflictions the player lays last | 0.25 |
| `AfflictionPower` | share | how strongly they work (damage, slow, what they let through) | 0.15 |
| `SpellDamage` | share | damage of the base spell and worn spells (not of `Effects`) | 0.10 |
| `SpellCost` | share | mana of the base spell and worn spells | -0.10 |
| `SpellReach` | share | reach of the base spell and worn spells | 0.15 |
| `SurgeSeconds` | share | how long the surge lasts | 0.20 |
| `SurgeCost` | share | mana the surge costs | -0.15 |
| `ManaFromBlows` | share | mana from weapon blows | 0.15 |
| `ManaFromGuards` | share | mana from a perfect guard (15) | 0.30 |
| `ManaFromDodges` | share | mana from a perfect dodge (12) | 0.30 |
| `StaminaCost` | share | stamina of the dodge and of a blocked blow | -0.10 |
| `DodgeDistance` | share | how far the dodge goes (4.5 m) | 0.10 |
| `DodgeSafeTime` | share | how long blows pass through the dodge, and its perfect window | 0.15 |
| `PerfectGuardWindow` | share | the perfect guard's window (0.18 s) | 0.15 |
| `Health` | flat, points | greatest health (100) | 10 |
| `FlaskHeal` | share | health a flask gives (45) | 0.15 |
| `FlaskCount` | flat, whole | flasks carried (3; the HUD draws up to 6) | 1 |
| `CritChance` | flat | chance a weapon blow is critical; 0.05 is five in a hundred | 0.05 |
| `CritDamage` | share, added to the x1.5 | what a critical blow multiplies by | 0.15 |
| `SurgeSpeed` | share | swing speed and walking speed while the surge lasts | 0.10 |
| `DamageTaken` | share | damage of blows that land on the player | -0.06 |
| `GreyKept` | share | how slowly grey health fades (1.0 = twice as long) | 0.30 |
| `EmbersEarned` | share | embers from the fallen (fractions carry over) | 0.10 |
| `MoveSpeed` | share | walking speed (6 m/s) | 0.05 |
| `SwingSpeed` | share | how fast the weapon is swung | 0.05 |

Critical blows are weapon blows only. A critical blow shows a gold star where it lands.

## 6. Effects: triggers (`UpgradeTrigger`)

An effect is `new UpgradeEffect { On = <trigger>, Do = <action>, ...numbers }`. "Target" is who the action is aimed at
when the effect's `At` is `EffectAt.Target` (the default).

| `UpgradeTrigger.` | When | Target |
|---|---|---|
| `LightHit` | a light weapon blow lands (not blocked) | the enemy struck |
| `HeavyHit` | a heavy blow lands, a finisher included | the enemy struck |
| `FinisherHit` | a finisher lands | the enemy finished |
| `SkillHit` | the weapon's skill lands | the enemy struck |
| `AnyHit` | any weapon blow lands | the enemy struck |
| `Kill` | an enemy dies, by a blow or by an affliction | where it fell (a point, not an enemy) |
| `PerfectGuard` | a perfect guard | the attacker (a point if it was a thrown bolt) |
| `Guard` | a blow blocked for stamina | the attacker |
| `PerfectDodge` | a perfect dodge | none |
| `DodgeStart` | any dodge begins | none |
| `DodgeEnd` | any dodge ends | none |
| `Struck` | a blow lands on the player | the attacker |
| `SpellCast` | the base spell or a worn spell is cast | none |
| `SpellHit` | the base spell or a worn spell strikes an enemy; once per enemy | that enemy |
| `SurgeStart` | the surge is lit | none |
| `SurgeEnd` | the surge runs out | none |
| `FlaskDrunk` | a flask heals | none |
| `FlaskThrown` | a flask is thrown. Learning any effect on this trigger is what lets the flask be thrown (hold the flask button; it heals nothing) | the enemy faced |
| `AfflictedHit` | a weapon blow or a spell strikes an enemy that already carried an affliction | that enemy |
| `AfflictionFull` | any affliction on an enemy reaches its full depth | that enemy |
| `EverySeconds` | every `Every` seconds while an enemy is within 12 m | none |
| `LowHealth` | the player's health falls below a third (once each time) | none |
| `ManaFull` | the player's mana becomes full (once each time) | none |

With no target, an effect at `EffectAt.Target` takes the enemy the player faces within 12 m; with none, a point 2.5 m
ahead. `At = EffectAt.Self` puts it on the player.

### Conditions (all optional; all that are set must hold)

| Field | Meaning | Default |
|---|---|---|
| `Chance` | chance it happens, 0.01 to 1 | 1 |
| `Cooldown` | seconds before it can happen again | 0 (`AfflictionFull` and `Kill` are never quicker than 0.2) |
| `Every` | seconds between, for `EverySeconds` | 5 |
| `Needs` | the target must already carry this `AfflictionKind` | none |
| `WhileSurge` | only while the surge lasts | false |

Damage dealt by an effect does **not** fire `SpellHit`, `LightHit` or `AfflictedHit` again, and is not multiplied by
`SpellDamage`. It is multiplied by `DamageToAfflicted`, and afflictions it lays do count toward `AfflictionFull`.

## 7. Effects: actions (`UpgradeAction`)

Units: metres, seconds, degrees; damage and stagger in weapon points (a light blow of the fists is 8 damage and 10
stagger; a soldier has 60 health and 100 balance). A field left at zero takes the value in brackets. `Affliction`
left out means the element the player holds; `Stacks = 0` on a damaging action lays nothing. An effect that names
another element's affliction is drawn in that element's colour.

| `UpgradeAction.` | What it does | Fields it reads |
|---|---|---|
| `Afflict` | lays stacks on the target, or with `Radius` on everything within it | `Affliction`, `Stacks` (1), `Radius` (0), `At` |
| `Burst` | a burst of damage and stagger in a circle | `Damage`, `Stagger`, `Radius` (2.5), `Delay`, `Affliction`, `Stacks`, `At` |
| `Cone` | a cone from the player, who turns to the target first | `Damage`, `Stagger`, `Reach` (4.5), `Arc` (90), `Affliction`, `Stacks` |
| `Hazard` | ground that bites what stands on it | `Shape` (`HazardShape.Patch`, `Line`, `Ring`), `Damage` each bite, `Stagger`, `Radius` (1) of a patch, `Seconds` (3), `Interval` (0.5), `Count` (4) patches for Line and Ring, `Reach` (4): the line's length from the player or the ring's radius, `Delay`, `Affliction`, `Stacks`, `At` |
| `Arc` | strikes the target and leaps on to others | `Damage`, `Stagger`, `Count` (2) leaps, `Reach` (5) of each leap, `Affliction`, `Stacks` |
| `Projectile` | thrown from the player; strikes each enemy it passes once | `Flight` (`Flight.Straight`, `Homing`, `Returning`), `Damage`, `Stagger`, `Radius` (0.9), `Speed` (10), `Seconds` (1.2), `Count` (1) in a fan 14 degrees apart, `Affliction`, `Stacks` |
| `Falling` | a blade or pillar that falls on a marked spot | `Damage`, `Stagger`, `Radius` (2), `Delay` (0.6), `Count` (1; the rest fall round the first), `Affliction`, `Stacks`, `At` |
| `Heal` | heals the player | `Amount` |
| `Mana` | gives mana (of 100) | `Amount` |
| `Stamina` | gives stamina (of 100) | `Amount` |
| `Boon` | a timed change to one stat; the same boon given again restarts and does not stack with itself | `Stat`, `Amount`, `Seconds` (5) |
| `Ward` | wards: each turns aside one blow that would have landed; three held at most | `Count` (1) |
| `Step` | a short step through the air to the target | `Reach` (8), `Amount`: metres beyond the target to land (0 stops in front of it) |
| `Pull` | drags enemies toward a point (bosses hardly move) | `Radius` (5), `Amount` (3) metres, `At` |
| `Push` | throws enemies away from a point | `Radius` (5), `Amount` (3) metres, `At` |
| `SlowTime` | slows everything but the player | `Seconds` (2), `Amount` (0.35): the share of its speed the world keeps |
| `Echo` | a copy of the player repeats the last blow where they stand | `Amount` (0.6) share of that blow's damage, `Delay` (0.3), `Radius` (2.3), `Stagger` |
| `Helper` | a helper strikes the target a few times and goes | `Damage`, `Stagger`, `Count` (3) strikes, `Interval` (0.6), `Radius` (2), `Affliction`, `Stacks` |
| `SetOff` | spends an affliction on the target for a burst round it | `Affliction` (the element's own), `Damage` added per stack spent (on top of the affliction's own set-off damage), `Stagger` per stack, `Radius` (3) |

One mark may hold several effects, on the same trigger or different ones.

## 8. Pictures (`UpgradeIcons`)

Use the constant: `Icon = UpgradeIcons.Shield`. The forty names:

`Flame Bolt Gust Snowflake Drop Leaf Rock Anvil Sun Moon Fang` (one per element, in ring order; each element's emblem
uses its own) `Blade Fist Boot Shield Flask Eye Heart Star Skull Hourglass Ring Arrow Wave Orb Beam Spike Wall Crown
Burst Comet Chain Spiral Plus Target Wing Tower Gem Claw Hammer`

Suggested use: Blade weapon skill; Fist weapon damage; Boot dodge; Shield guard; Flask flasks; Eye critical; Heart
health; Hourglass time and windows; Skull kills; Gem mana; Wing speed; Chain arcs; Target marks; Plus healing; Crown a
god's gift (a Signature with any other picture gets a small crown drawn over its frame as well).

## 9. Where a mark sits

Each tree has its own frame: **Y runs outward** from the element's emblem (which is at Y = 0), **X runs across**,
positive to the right when the tree stands upright (the chart is turned so the held tree always does). One step is the
unit. The chart places the tree on the ring for you.

Rules (all tested):

1. Tree marks: `1 <= Y <= 12` and `|X| <= 0.28 * (9 + Y) - 1.45`. That is 1.4 at Y 1.2, 1.9 at Y 3, 2.5 at Y 5, 3.0 at
   Y 7, 3.6 at Y 9, 4.2 at Y 11. A tree is a wedge: narrow at its foot.
2. Mix marks: `1 <= Y <= 9` and `|X| <= 0.6`.
3. No two marks anywhere on the chart nearer than **0.85** steps (centre to centre). Keep 1.0 or more between two
   marks that are not minors.
4. A mark never lies inward of what it grows from: its Y is at least its parent's.
5. The stick must reach every mark: from any mark, a push moves to the nearest mark within 62 degrees of the push,
   preferring ones straight ahead. A mark with a neighbour within about 1.9 steps up, down or diagonally is safe.
   Do not leave a mark alone more than 2.5 steps from every other.

### The worked layout: three boughs, thirty marks

This layout is proved by the test `ThePatternTreeOfTheSchema...` for all eleven elements at once with a full mix
between every pair (418 marks): nothing overlaps and the stick reaches everything. **Use these exact slots** unless
you have a reason not to; then no arithmetic is needed. Drop slots you do not need from the outer ends inward; to add
a 31st, put a minor at `(-0.75, 11.0)` or a spell at `(0.75, 11.1)`.

| Slot | X | Y | Meant for | Grows from |
|---|---|---|---|---|
| root | 0 | 1.2 | Root | (nothing) |
| **Left bough** | | | | |
| L1 | -1.21 | 2.4 | Major | root |
| la | -1.98 | 3.42 | Minor | L1 |
| L2 | -1.61 | 4.2 | Major | L1 |
| lc | -1.2 | 4.97 | Minor | L2 |
| lb | -2.4 | 5.23 | Minor | L2 |
| LS | -2.0 | 6.0 | Spell (or Major) | L2 |
| le | -1.6 | 6.77 | Minor | LS |
| ld | -2.8 | 7.03 | Minor | LS |
| LK | -2.4 | 7.8 | Keystone | LS |
| **Middle bough** | | | | |
| M1 | 0 | 2.9 | Major | root |
| m1 | 0.62 | 3.8 | Minor | M1 |
| M2 | 0 | 4.7 | Major | M1 |
| m2 | -0.62 | 5.75 | Minor | M2 |
| MS1 | 0 | 6.5 | Spell | M2 |
| m3 | 0.62 | 7.4 | Minor | MS1 |
| M3 | 0 | 8.3 | Major | MS1 |
| m4 | -0.62 | 9.2 | Minor | M3 |
| MS2 | 0.75 | 9.3 | Spell | M3 |
| MK | 0 | 10.1 | Keystone | M3 |
| **Right bough** (the left, mirrored) | | | | |
| R1 | 1.21 | 2.4 | Major | root |
| ra | 1.98 | 3.42 | Minor | R1 |
| R2 | 1.61 | 4.2 | Major | R1 |
| rc | 1.2 | 4.97 | Minor | R2 |
| rb | 2.4 | 5.23 | Minor | R2 |
| RS | 2.0 | 6.0 | Spell (or Major) | R2 |
| re | 1.6 | 6.77 | Minor | RS |
| rd | 2.8 | 7.03 | Minor | RS |
| RK | 2.4 | 7.8 | Keystone | RS |
| **Crown** | | | | |
| gift | 0 | 12 | Signature | (nothing) |

That is 1 root, 7 majors, 4 spells, 3 keystones, 14 minors and the gift. Any slot may hold another kind: the frames
differ in size (a minor is 0.5 steps wide, a major 0.68, a spell 0.78, a keystone 0.94, a gift 1.15), and every slot
has room for a keystone except the lower-case minor slots, which should stay minors or majors.

Fire and Lightning already use `root, L1, la, L2, LS, M1, m1, M2, MS1, R1, ra, R2, RS` (and Lightning the gift).

### A mixed branch: eight marks

The branch grows in the gap between its two trees, from a small knot of its own. Its frame is the same (Y outward, X
across), and it is narrow: zigzag up it.

| Slot | X | Y | Meant for | Grows from |
|---|---|---|---|---|
| x1 | 0 | 1.0 | Mix | (nothing: the branch's foot) |
| x2 | 0.45 | 2.0 | Mix | x1 |
| x3 | -0.45 | 3.0 | Mix | x2 |
| x4 | 0.45 | 4.0 | Mix | x3 |
| x5 | -0.45 | 5.0 | Mix | x4 |
| x6 | 0.45 | 6.0 | Mix | x5 |
| xs | 0 | 7.2 | Mix with a `Form` (the branch's spell) | x6 |
| xgift | 0 | 8.6 | Signature (with `Element2`) | (nothing) |

`PlasmaTree` uses x1, x2, x3 and has its gift at `(0, 4.6)` for now.

## 10. Spells and spell forms

A mark with a `Form` teaches a spell. The first spell learned is worn at once; on the chart, pressing learn on a spell
already owned wears that one instead. **Hold** the spell button (0.3 s) to cast the worn spell; a **tap** is always the
element's base spell. The HUD names the worn spell beside the weapon. Each element should have four or five.

`Spell = new SpellSpec { ... }`. Fields and their defaults; a form reads only the ones listed for it:

| Field | Default | Meaning |
|---|---|---|
| `Cost` | 30 | mana |
| `Damage` | 20 | of its main blow |
| `Stagger` | 20 | of its main blow |
| `Reach` | 6 | metres from the player |
| `Radius` | 2.5 | of whatever it strikes with |
| `Arc` | 70 | full width of a cone, degrees |
| `Seconds` | 4 | how long it, or what it leaves, lasts |
| `Interval` | 0.5 | seconds between bites of what it leaves |
| `Count` | 2 | how many: leaps, blades, bullets |
| `Stacks` | 1 | stacks of the element's affliction on what it strikes |
| `AfflictionSeconds` | 0 | seconds that affliction lasts; 0 for its usual time |
| `Delay` | 0 | seconds before it lands or fires |
| `Speed` | 10 | metres a second |
| `Height` | 3.4 | how high a leap rises |
| `LingerDamage` | 5 | each bite of what it leaves behind |

The forms that exist (`SpellForms.` constants; the id string in brackets):

| Form | What the player sees | Reads |
|---|---|---|
| `Wave` ("wave") | the element thrown ahead in a cone, striking all in it at once | `Damage`, `Stagger`, `Reach`, `Arc`, `Stacks` |
| `Bolt` ("bolt") | a strike on the enemy faced that leaps to others | `Damage`, `Stagger`, `Reach`, `Count` (leaps), `Radius` (how far each leap can be), `Stacks` |
| `Leap` ("leap") | a leap out of the fight, untouchable in the air, down beside the enemy faced as a heavy blow, leaving a crater of the element for 4 s | `Reach`, `Seconds` (in the air), `Height`, `Damage`, `Stagger`, `Radius`, `LingerDamage`, `Stacks` |
| `Rod` ("rod") | a rod planted `Reach` metres ahead; the element strikes inside a circle round it again and again | `Reach`, `Radius`, `Seconds`, `Interval`, `Damage`, `Stagger`, `Delay`, `Stacks` |

The twenty-four forms built on 2026-11-16 (the ids are fixed in `docs/design/FORMS.md`; one sample spell of each, with
ordinary numbers, is in `FormSamples.cs`). **Reads** lists every field the form uses; anything else in the spec is ignored.
`Damage`, `Stagger` and `Reach` are read after the player's spell stats; every blow lays `Stacks` of the caster's
affliction for `AfflictionSeconds`. Where a form departs from `FORMS.md`, the last column says how.

| Form | What the player sees | Reads | As built |
|---|---|---|---|
| `Blades` ("blades") | blades snap into being over the enemy faced, hang over a turning sign, fall one after another and stick in the ground | `Count` (16 at most), `Delay`, `Radius`, `Damage`, `Stagger`, `Reach` | they follow the enemy until the first falls; with more than one they are scattered over 0.9 x `Radius` round it |
| `Sword` ("sword") | one sword 13 m long hangs over the mark, falls, and stands in ground it has cracked | `Delay`, `Radius`, `Damage`, `Stagger`, `Seconds`, `LingerDamage`, `Reach` | the blow is heavy; the crack bites in 0.75 x `Radius` every half second; it follows the enemy until 0.2 s before it falls |
| `Beam` ("beam") | a brace, a lane on the ground, then a thick bar held | `Reach`, `Radius` (half width), `Seconds`, `Interval`, `Damage` (each bite), `Stagger` | 0.3 s of brace first; turns at 55 degrees a second with the stick; the dodge lets it go and a blow that lands breaks it |
| `Phalanx` ("phalanx") | a wing of blades behind the shoulders | `Count` (16 at most), `Seconds`, `Reach`, `Damage`, `Stagger`, `Speed` | one lets fly every 0.55 s at the nearest enemy within `Reach`; a heavy blow that lands sends all the rest at whoever it struck; they fly at 2.4 x `Speed`; one wing at a time |
| `Dragon` ("dragon") | a horned head forms at the shoulder and lunges down a lane, neck winding behind, jaws closing at the end | `Reach`, `Radius`, `Damage`, `Stagger`, `Speed` | what it passes takes half the damage and stagger once and is carried along with it; the bite at the end is the whole of both, heavy, in 1.5 x `Radius`; it travels at 1.5 x `Speed` |
| `Nova` ("nova") | the element drawn in to a sign underfoot, then a crown of shards and a ring to the edge | `Radius`, `Delay`, `Damage`, `Stagger` | as written |
| `Orb` ("orb") | a cut stone with two splinters circling it drifts after the nearest enemy and bursts | `Speed`, `Seconds`, `Radius`, `Damage`, `Stagger` | it looks for the nearest enemy within 16 m and stops 0.9 m short of it; it bursts only when its time is up, or at once, heavy and half as large again in everything, when a blow of the weapon reaches it |
| `Bomb` ("bomb") | a growing stone held overhead, thrown, a great burst | `Delay`, `Reach`, `Radius`, `Damage`, `Stagger`, `Seconds`, `LingerDamage` | it lands on the enemy faced, or `Reach` ahead; rolling away or being struck before the throw loses it; the patch it leaves is 0.8 x `Radius` |
| `Spikes` ("spikes") | rings run out along the ground in rows and a spike erupts in each, taller the further out | `Reach`, `Speed`, `Count` (rows, 5 at most), `Radius`, `Damage`, `Stagger` | each enemy on the line is struck once in a cast; spikes stand 1.5 x `Radius` apart |
| `Field` ("field") | a rimmed patch that bristles with small spikes | `Reach`, `Radius`, `Seconds`, `Interval`, `LingerDamage` | as written; no stagger |
| `Drill` ("drill") | the player is gone and a boring point runs the line, rings thrown off behind; they stand at the far end facing back and the cuts land | `Reach`, `Radius`, `Count`, `Damage` (each), `Stagger`, `Speed` | the first strike is given in passing; the other `Count` - 1 land afterwards, 0.07 s apart, on everyone still on the line; it runs at 1.8 x `Speed`; it stops at a wall and at anything that cannot be shoved (the colossus) |
| `Minions` ("minions") | small horned helpers rise beside the player, run at enemies and jab | `Count` (6 at most), `Seconds`, `Damage` (each jab), `Stagger`, `Interval` | each takes an enemy of its own while there are enough; they run at 7.5 m a second; one band at a time |
| `Bullets` ("bullets") | splinters in a ring round the enemy faced, points inward, then all in at once | `Count` (16 at most), `Radius`, `Delay`, `Damage` (each), `Stagger`, `Reach` | they strike the enemy faced and no other; with no enemy faced they strike a 1.2 m circle at the point aimed at |
| `Reverse` ("reverse") | rings and streaks falling inward to a point, then everything thrown out of it | `Reach`, `Radius`, `Delay`, `Damage`, `Stagger` | enemies are drawn 0.85 x `Radius` toward the point over the delay; the burst is heavy and as wide as `Radius` |
| `Parry` ("parry") | a sign held tipped in front of the chest; a blow turned; a fan of light thrown back | `Seconds`, `Reach`, `Arc`, `Damage`, `Stagger` | it turns a blow from any side; the turned blow counts as a perfect guard for the attacker (it reels) but gives no mana; the answer is heavy; the player is rooted and the dodge lets it go |
| `Wall` ("wall") | a line, then slabs breaking out of the ground from the middle outward | `Reach`, `Arc` (width, 2 to 14 m), `Delay` (0.15 s at least), `Seconds`, `Radius`, `Damage`, `Stagger` | a solid barrier: nothing walks through it, the player included; a blow whose attacker stands beyond it does not reach the player; the burst strikes `Radius` round its middle and along its whole line; one wall at a time. **Enemies do not path round it**: they press against it |
| `Discs` ("discs") | toothed wheels circling the player | `Count` (8 at most), `Radius`, `Seconds`, `Interval`, `Damage` (each bite), `Stagger` | each disc bites 1 m round itself; an enemy is bitten once an interval however many discs touch it; one halo at a time |
| `Brand` ("brand") | a sign under the enemy faced and a crossed mark over its head, smaller for every burst spent | `Reach`, `Seconds`, `Count`, `Radius`, `Damage`, `Stagger` | with no enemy faced nothing is branded and the mana is lost ("No one to brand"); the burst strikes the branded enemy too; one brand at a time |
| `Rain` ("rain") | rings closing on spots inside a circle and a shard down on each | `Reach`, `Radius`, `Seconds`, `Interval`, `Damage` (each), `Stagger` | each strike is 1.2 m round its spot, 0.34 s after its ring shows; every other one falls where an enemy inside the circle stands |
| `Clone` ("clone") | the player's shape in light at their right shoulder, doing what they did a fifth of a second ago | `Seconds`, `Damage` (percent of the blow) | the echo lands 0.2 s after each blow that lands, with no stagger; one copy at a time |
| `Flash` ("flash") | the player gone, lines crossing the enemy, the player beyond it with their back turned, every line landing at once | `Count` (12 at most), `Interval`, `Delay` (0.15 s at least), `Damage` (each cut), `Stagger`, `Reach` | they reappear 2 m beyond the enemy; with no enemy faced the cuts land on a 1.6 m circle at the point aimed at |
| `Ring` ("ring") | stakes come in from outside and stand in a circle | `Radius`, `Seconds`, `LingerDamage`, `Reach` | its middle is half-way between the player and the enemy faced and it is never too small to hold both; a solid twelve-sided barrier that holds the player in too; the band within a metre of the stakes bites every 0.4 s, without stagger; one ring at a time |
| `Pillars` ("pillars") | a mark under each of the nearest enemies, then a column up under each in turn | `Count` (8 at most), `Reach`, `Radius`, `Damage`, `Stagger`, `Delay` | **nothing is lifted** (no enemy can leave the ground yet): the blow is heavy instead, so it finishes a staggered enemy and shatters a frozen one; with fewer enemies than pillars the rest stand round the first |
| `Step` ("step") | the player on the far side of the enemy, a line of light behind and their shape left at the start | `Reach`, `Damage`, `Stagger`, `Seconds` | it strikes everything within 1.1 m of the line, ends 1.8 m beyond the enemy faced, turned to face it; it stops at a wall and at anything that cannot be shoved |

Every form that shows itself before it strikes waits at least 0.22 s (`FormRules.LeastWarning`) whatever its `Delay`.
On gods: every form strikes the swordsman, the colossus and its parts through the same blows as any enemy; the
colossus is never pulled, carried or passed through, and the swordsman is shoved a third as far as a soldier.
Every cast in progress ends when the player falls. Marks on the ground are drawn at the height the caster stood at.
The forms have no sounds yet.

A tree may name a form that does not exist yet **only** with a `// NEEDS FORM: <name>` comment and the form set to the
nearest existing one, because the test `EveryMarkIsWhole...` fails on an unknown form id.

### For whoever writes a form

```csharp
public interface ISpellForm { string Id { get; } int Cast(in SpellCast cast); }   // returns how many it struck at once
public interface ISpellLanding { void Landed(in SpellCast cast); }                 // only for forms that call cast.Player.Leap(...)
public abstract class RunningForm : ISpellForm { bool Tick(ref SpellRun run, float deltaTime); ... }  // a form that lasts
```

A form is a class under `Runtime/Player/Forms/` (namespace `Clover.Player.Forms`), stateless (a test fails on a form
with a field of its own), one instance for every cast, plus a constant and one line in the array in `SpellForms.cs`.
The mana is already paid. `SpellCast` hands it:
`Name`, `Source` (the upgrade, or null for a base spell), `Spec`, `Damage` / `Stagger` / `Reach` (with the player's
spell stats already applied), `Player`, `Combat`, `Defence`, `Caster` (the `PlayerElement`), `Upgrades`, `Element`
(`ElementInfo`), `Colour`, `Affliction`, `Target` (the enemy faced within reach, or null), `Origin`, `Forward`, `Aim`.
Helpers: `cast.Blow(damage, stagger, stacks = -1)` makes a `SpellBlow` in the element's colour carrying its
affliction; `cast.Linger(damage)` the light bite of ground left behind; `cast.Ground(at, radius, seconds, interval,
blow)` a `HazardSpec` in the element's look. Strike through `cast.Combat.StrikeCone(reach, arc, blow)`,
`StrikeArea(centre, radius, blow, once, innerRadius)`, `StrikeLane(from, direction, length, halfWidth, blow, once)`,
`StrikeOne(victim, blow, from)`, `StrikeChain(reach, blow, leaps, leapRange, cast.Element, first)`, `AfflictArea`,
`Displace`, `ShoveArea`, and find enemies with `EnemyNear(point, radius, except)`; lay ground and things in flight with
`Hazards.Lay(spec)` (`Velocity`, `Flight`, `Target`, `Follow`, `Bites`, `Burst`, `Warn`, `Delay`, `Once`); draw with
`ElementLook.Puff / Sparks / Ring / Strike` and `cast.Caster.ShowArc`; shake with `cast.Upgrades.Jolt(stop, shake,
seconds)`. Never create or destroy objects in a cast, and never allocate per frame.

A form that takes time extends `RunningForm`: in `Cast` it calls `SpellWorks.Begin(this, cast, out begun)` and fills in
the `SpellRun` that comes back (plain fields that mean what the form says: `Phase`, `Count`, `Done`, `T0..T3`, `A`, `B`,
`C`, `Flag`, `Victim`); `SpellWorks` then calls its `Tick` every frame on the game's clock until it returns false, and
its `Swung` / `Struck` when the weapon is swung or lands (note it there, act in `Tick`). Each run has lists of its own
(`SpellWorks.Solids / Marks / Points / Times / Things / Once(run)`); handles left in the first two are put away when
the run ends. `SpellWorks.TakeBody(ref run, untouchable, cancellable)` gives the run the player's body (rooted, moved
with `Player.FormStep` or `PassTo`, posed with `FormPose` and `FormPoseProgress`, hidden with `Hidden`);
`SpellWorks.Gesture(cast, pose, seconds)` is the short pose of a cast that does not hold the body;
`SpellWorks.Barrier` and `Screen` stand a wall; `SpellWorks.Ghost` leaves the player's shape in light. The works holds
24 runs.

What forms are drawn with is `FormFx`: 120 solid shapes (`FormMesh`: blade, sword, shard, spike, pillar, disc, orb,
slab, dragon skull, jaw and spine, drill, minion; drawn in code by `FormMeshes`, shaded by `Clover/Spell` in three flat
tones with an ink line) and 140 flat marks of light (`FormMark`: ring, disc, star, cross, crack, sigil, beam, lane,
streak, scorch, brackets; shapes 10 to 14 of `Clover/Glow` were added for them). Throw one that looks after itself
(`Erupt`, `Drop`, `Fly`, `Lay`, `Run`, `Face`, `Cut`) or hold one and place it every frame (`Hold` / `Place` / `Heat` /
`Release`, `HoldMark` / `PlaceMark` / `ReleaseMark`). `FormKit` has what the forms share: `Warn` (something will land
here), `Impact` (it has: star, ring, cracks, stain, stop and shake by weight), `Lit` (the colour as light; one
`Brightness` number turns all of it up or down), the poses. `WaveForm`, `BoltForm`, `LeapForm` and `RodForm` are the
short examples; `BladesForm` and `BeamForm` the examples of a form that lasts and of one that holds the body.

## 11. Mixes

A mix mark belongs to both elements. It is open to a player who **holds one of the two and has held or consumed the
other in this run**; sealed, it is still shown, and the words panel says which element opens it. Once bought it works
while the player holds either element. Write it with the earlier element first:

```csharp
Element = Elements.Wind, Element2 = Elements.Ice, Kind = UpgradeKind.Mix, Cost = 4,
```

`Requires` may name only marks on the same branch. An effect in a mix that should lay one particular affliction must
name it (`Affliction = AfflictionKind.Chill`), because the default is whichever element the player holds.
The branch's gift is `Kind = UpgradeKind.Signature` with both elements set.

## 12. The voice

Names: two or three plain words with weight. Descriptions: one or two short sentences saying exactly what happens, in
the present tense, in plain words; no numbers unless they are a count the player sees ("up to five") or a mana cost on
a spell ("35 mana."); no exclamation marks (tested); name buttons by what they do ("Hold the spell button").

## 13. A complete file

Wind's tree, cut to eleven marks to show every kind. A real tree fills the thirty slots.

```csharp
using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>Wind: never where the blow lands. Left the dodge, middle the gale, right the open guard.</summary>
    public static class WindTree
    {
        public const string Root = "wind.breath";
        public const string Sidestep = "wind.sidestep";
        public const string LightFeet = "wind.lightfeet";
        public const string Slipstream = "wind.slipstream";
        public const string Updraft = "wind.updraft";
        public const string LongBreath = "wind.longbreath";
        public const string Cyclone = "wind.cyclone";
        public const string EyeOfTheStorm = "wind.eye";
        public const string Buffet = "wind.buffet";
        public const string Windfall = "wind.windfall";
        public const string Gift = "wind.gift";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Wind;

            to.Add(new Upgrade { Id = Root, Name = "First Breath", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Gust,
                Description = "Every blow of the weapon leaves an enemy Unsteady: it staggers sooner and is pushed further.",
                WeaponStacks = 1 });

            // Left bough: the dodge.
            to.Add(new Upgrade { Id = Sidestep, Name = "Sidestep", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -1.21f, Y = 2.4f, Icon = UpgradeIcons.Boot,
                Description = "A dodge at the last moment throws a gust back along your path.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Cone, Damage = 14f, Stagger = 30f, Reach = 5f, Arc = 80f, Stacks = 2 } } });
            to.Add(new Upgrade { Id = LightFeet, Name = "Light Feet", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Sidestep }, X = -1.98f, Y = 3.42f, Icon = UpgradeIcons.Wing,
                Description = "The dodge costs less stamina.",
                Stats = new[] { new StatMod(PlayerStat.StaminaCost, -0.1f) } });
            to.Add(new Upgrade { Id = Slipstream, Name = "Slipstream", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Sidestep }, X = -1.61f, Y = 4.2f, Icon = UpgradeIcons.Spiral,
                Description = "Every dodge ends with a ring of wind that pushes enemies away and leaves you quicker for a while.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 3.5f, Amount = 3f, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Boon, Stat = PlayerStat.MoveSpeed, Amount = 0.2f, Seconds = 4f },
                } });

            // Middle bough: the gale.
            to.Add(new Upgrade { Id = Updraft, Name = "Updraft", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 0f, Y = 2.9f, Icon = UpgradeIcons.Arrow,
                Description = "A heavy blow on an Unsteady enemy spends the wind on it in a burst.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.SetOff, Needs = AfflictionKind.Unsteady, Damage = 4f, Stagger = 12f, Radius = 3f } } });
            to.Add(new Upgrade { Id = LongBreath, Name = "Long Breath", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Updraft }, X = 0.62f, Y = 3.8f, Icon = UpgradeIcons.Hourglass,
                Description = "Tailwind lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.2f) } });
            to.Add(new Upgrade { Id = Cyclone, Name = "Cyclone", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Updraft, LongBreath }, X = 0f, Y = 4.7f, Icon = UpgradeIcons.Spiral,
                Description = "Hold the spell button to plant a whirlwind ahead of you. It batters what stands in it for a while. 35 mana.",
                Form = SpellForms.Rod,
                Spell = new SpellSpec { Cost = 35f, Reach = 3f, Radius = 3f, Seconds = 5f, Interval = 0.6f, Damage = 8f, Stagger = 14f, Stacks = 1 } });
            // BESPOKE: while the surge lasts the player cannot be struck from behind. Needs code in PlayerDefence.
            to.Add(new Upgrade { Id = EyeOfTheStorm, Name = "Eye of the Storm", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Cyclone }, X = 0f, Y = 6.5f, Icon = UpgradeIcons.Eye,
                Description = "While Tailwind lasts, no blow from behind can land." });

            // Right bough: the open guard.
            to.Add(new Upgrade { Id = Buffet, Name = "Buffet", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 1.21f, Y = 2.4f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard drags everyone near toward you and leaves them Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Pull, At = EffectAt.Self, Radius = 6f, Amount = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 6f, Stacks = 2 },
                } });
            to.Add(new Upgrade { Id = Windfall, Name = "Windfall", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Buffet }, X = 1.98f, Y = 3.42f, Icon = UpgradeIcons.Gem,
                Description = "A perfect guard gives more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromGuards, 0.3f) } });

            to.Add(new Upgrade { Id = Gift, Name = "Heart of the Gale", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Tailwind lasts, every dodge is free and leaves a copy of you that strikes." });
        }
    }
}
```

(The costs of this cut-down example sum to exactly 2 + 3 + 1 + 6 + 3 + 1 + 4 + 6 + 3 + 1 = 30, above the 19 the test
asks for.) Then, in `UpgradeTrees.AddAll`, after `LightningTree.Add(to);`: `WindTree.Add(to);`.

## 14. Limits of the engine, so nothing is designed past them

- At most 64 effects are done in one frame; the rest of that frame's are dropped. Give `AnyHit` effects that strike a
  `Cooldown`.
- 48 patches of ground or things in flight are alive at once (`Hazards.Capacity`); a 49th takes the oldest's place.
- 16 boons run at once; three wards are held at most.
- An effect cannot yet: change an enemy's own attacks, revive the player, summon something that walks, or alter the
  weapon's moves. Those are BESPOKE.
- Enemies that take afflictions: every ordinary enemy, the Gate Captain, the swordsman god (frozen for 1.2 s only), the
  training dummies, and the colossus (which is worn down and lets more through but is never frozen, slowed or moved,
  and cannot be killed by damage over time).
- The player carries no afflictions; nothing in this schema lays one on the player.
