# The upgrade schema: how to write a tree file

Written by Claude on 2026-11-16 (day run), for whoever writes an element's tree as data. **Section 15 lists everything added
that afternoon** (conditions on stats, new stats, triggers, actions, counts, the air, grown things, gifts): read it with sections 5 to 7. Everything here matches the
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
| Static | 5 | 6 | nothing by itself; set off for 7 a stack. (Lightning's own root discharges it for 7 damage and 5 stagger a stack in 4 m on a heavy blow) |
| Unsteady | 5 | 6 | +6% stagger taken, shoved 15% further, turns 8% slower, per stack |
| Chill | 5 | 6 | 7% slower per stack. At 5: **Frozen** for 2.5 s (1 s for elites and bosses, never the colossus): held still; a heavy blow shatters it for 45 and ends the chill |
| Soaked | 1 | 8 | on or off: +25% damage from spells. Static or Chill laid on it is also laid on everyone within 4 m |
| Seeded | 3 | 8 | each weapon blow on it heals the player 1 per stack. When it dies: 12 damage per stack to everyone within 3 m |
| Tremor | 3 | 6 | 10% slower per stack, and its balance does not drain back |
| Sundered | 3 | 8 | +8% damage from weapons per stack |
| Marked | 1 (3 with Light's Weighed) | 8 | carries three critical blows: each weapon blow on it is critical (x1.5), gives 2 more mana per stack and spends one; the third ends the mark. Marking again refills the three |
| Cursed | 1 | 8 | deals 15% less. When it dies the player is healed 6 |
| Poison | 8 | 6 | nothing between bursts; every 2 s a burst of 2 per stack. Set off: 6 a stack |

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
| `PerfectDodge` | a perfect dodge | the attacker whose blow was dodged (none if it was a thrown bolt): see 17.6 |
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
| `Pillars` ("pillars") | a mark under each of the nearest enemies, then a column up under each in turn | `Count` (8 at most), `Reach`, `Radius`, `Damage`, `Stagger`, `Delay` | what a column strikes is thrown **into the air** for 0.8 s (section 15); the blow is heavy as well, so it finishes a staggered enemy and shatters a frozen one; with fewer enemies than pillars the rest stand round the first |
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
- The player carries no afflictions; nothing in this schema lays one on the player. (White Heat burns the player by hand.)
- `FlaskCount` can take the flasks to none; `Health` never below 1.

## 15. Added on the afternoon of 2026-11-16

Everything here is in the code and tested (`UpgradeEngineTests`, `AfflictionTests`, `UpgradeChartTests`) and played by
`UpgradeProbe` (sections V to X). It closes the gaps the first twenty-two tree files had to write round.

### 15.1 A stat may be conditional

`new StatMod(PlayerStat.X, amount, StatWhen.Y)` holds only in a state or on a kind of blow;
`new StatMod(PlayerStat.X, amount, AfflictionKind.Z)` holds only against a target that carries Z.

| `StatWhen.` | Holds | Works with |
|---|---|---|
| `Guarding` | while the guard is raised | any stat |
| `Surge` | while the surge lasts | any stat |
| `LowHealth` / `HalfHealth` / `HighHealth` | health below a third / below a half / at a half or more | any stat |
| `Unhurt` | no blow has landed on the player for 8 s | any stat |
| `Planted` | no dodge begun for 3 s (sooner with `PlantedSooner`) | any stat |
| `GreyHeld` | grey health is held | any stat |
| `ManaFull` | mana is full | any stat |
| `NearGrown` / `FarFromGrown` | something the player grew stands within 4 m / nothing within 7 m | any stat |
| `HeavyBlow` / `LightBlow` | on a heavy blow (finishers too) / a light blow | `WeaponDamage`, `StaggerDealt`, `CritChance` |
| `CriticalBlow` | on a critical blow | `WeaponDamage`, `StaggerDealt` |
| `ChainEndBlow` | on the last light blow of a chain, or the heavy blow thrown from a chain | `WeaponDamage`, `StaggerDealt` |
| `AirborneBlow` | on a blow that lands on an enemy in the air | `WeaponDamage`, `StaggerDealt`, `ManaFromBlows` |

Against an affliction works with `WeaponDamage`, `StaggerDealt`, `CritChance`, `ManaFromBlows` (weapon blows) and
`SpellDamage` (spells and effects). State conditions join the stat's one sum while they hold. Blow and affliction
conditions multiply the blow by `1 + their own sum`, apart from the standing sum. At most 48 stats against an
affliction are in force at once.

### 15.2 New stats

| `PlayerStat.` | Kind | What it changes |
|---|---|---|
| `AttackStamina` | share | stamina of the weapon's own blows |
| `GuardStamina` / `DodgeStamina` | share | stamina of a blocked blow alone / of the dodge alone (each on top of `StaminaCost`) |
| `BaseSpellReach` / `BaseSpellDamage` / `BaseSpellArc` | share | the base spell alone (on top of the stats for every spell) |
| `BaseSpellCooldown` | share | the wait of the base spell of the mark's own element alone (-0.2 is a fifth shorter; on top of `SpellCooldown`) |
| `BaseSpellCount` | flat, whole | one more leap, blade or pillar of the base spell |
| `AfflictionDepth` | flat, whole | how much deeper the held element's affliction can be laid ("full" then means the deeper depth) |
| `BurstReach` / `BurstDamage` | share | bursts afflictions make: `SetOff`, and the burst of a death |
| `Leech` | flat | share of weapon damage dealt that returns as health (0.06 is six in a hundred) |
| `FlaskEarn` | share | how quickly damage dealt earns an empty belt a flask back |
| `BloodPrice` | flat | health a point of missing mana costs when a spell is cast without enough (0.5); the health is left grey |
| `HealthPrices` | share | prices paid in health: `SpellSpec.HealthCost`, the blood price, `SpendHealth`, a surge's drain |
| `GreyStrength` | flat | weapon damage for each point of grey health held (0.0075), to 30% |
| `PlantedSooner` | flat, seconds | counts as planted sooner after a dodge |
| `AirSeconds` | flat, seconds | enemies the player throws into the air hang longer |
| `AirBlows` | flat, whole | more blows than three can hold an enemy up; past three each adds 15% less |
| `GrownSeconds` / `GrownDamage` | share | what the player grows stands longer / bites harder |
| `GrownLimit` | flat, whole | one more of each kind of grown thing |
| `BackdraftStagger`, `BackdraftReach`, `TrailSeconds`, `TrailDamage`, `BlinkPower`, `BlinkWidth`, `AfterimagePower`, `DischargeReach`, `ChargeStagger` | share | the numbers of the hand-written Fire and Lightning moves (`BackdraftReach` is also the detonation's) |
| `HeatBlows` | flat, whole | blows fewer for each level of Heat |
| `HeatCooling` | share | how long Heat lasts between blows |
| `HeatCap` | flat, whole | levels of Heat above five (each level above five also quickens the swing by 5%, by hand) |

### 15.3 New triggers and conditions

| `UpgradeTrigger.` | When | Target |
|---|---|---|
| `CritHit` | a critical weapon blow lands | the enemy struck |
| `ChainEnd` | the last light blow of a chain, or the heavy blow thrown from a chain, lands | the enemy struck |
| `SetOff` | the player sets an affliction off: the `SetOff` action, Lightning's discharge, a detonated burn, Plasma's burst | that enemy; carries the affliction and the stacks spent |
| `Shatter` | a frozen enemy is shattered | that enemy |
| `WardSpent` | a ward turns a blow | none |
| `Lifted` | the player throws an enemy into the air | that enemy |
| `Landed` | an enemy that was in the air comes down | that enemy |

`Kill` now knows what the dead enemy carried: `Needs` and `Needs2` are asked of the dead one's afflictions (the target
is still only the place it fell).

| Field | Meaning |
|---|---|
| `Needs2` | the target must carry this as well as `Needs` |
| `NeedsFull` | `Needs` must be at its full depth |
| `NeedsAirborne` | the target must be in the air (or reeling) |
| `While` | a `StatWhen` state of the player that must hold (`While = StatWhen.LowHealth`) |
| `HealthBelow` | only while the player's health is below this share |
| `Spent` | for `SetOff`: only when this affliction was the one spent |
| `SpentAtLeast` | for `SetOff`: only when at least this many stacks were spent |
| `OwnSpell` | for `SpellCast`: only when the spell cast is this mark's own |
| `PerStack` | damage, stagger and amounts (or, for `Projectile`, `Falling` and `Ward`, the count) are multiplied by what the trigger carried: stacks spent by a set-off, stacks the dead carried of `Needs`, or points of a count spent |
| `CarriesDepth` | the stacks it lays are as many as the trigger carried |
| `Spends` / `SpendsAll` | it happens only if its mark's count (below) holds that many / any, and spends them; `SpendsAll` carries the number for `PerStack` |

When a trigger names no enemy (`PerfectDodge`, `EverySeconds`, `SurgeStart` and the like), `Needs`, `Afflict` without
a `Radius`, `SetOff`, `Hold` and `Lift` now take the enemy the player faces within 12 m, as section 6 always said.

### 15.4 New actions

| `UpgradeAction.` | What it does | Fields it reads |
|---|---|---|
| `Hold` | holds the target still: it cannot move, turn or begin an attack. Elites and gods for 0.4 of the time, the colossus not at all, as with Frozen | `Seconds` (1.5), `Radius` (0: the target alone), `At` |
| `Lift` | throws the target into the air (15.6) | `Seconds` (1), `Radius` (0), `Stagger` (40) for what only reels, `At` |
| `Drop` | drives an enemy in the air into the ground at once | none |
| `Bank` | adds to the count its own mark keeps | `Count` (1) |
| `FlaskFill` | fills part of a flask; a whole one joins the belt if there is room | `Amount`: share of a flask (0.25) |
| `SpendHealth` | takes health and leaves it grey; never the last point | `Amount` |
| `Leech` | heals by a share of the last weapon blow's damage | `Amount` (0.1) |
| `BurstGrown` | bursts everything the player has grown, each where it stands, healing for each | `Damage`, `Stagger`, `Radius` (3), `Amount` health for each, `Affliction`, `Stacks` |

`Hazard` reads two more fields: `Grown = true` makes the patch something grown (15.7), and `Limit` is how many of this
effect's may stand at once.

### 15.5 A count a mark keeps

```csharp
Tally = new TallySpec { Name = "Blades", Cap = 3 },   // optional: Stat, PerPoint, Stat2, PerPoint2, Kept
Effects = new[]
{
    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Bank, Count = 1 },
    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, SpendsAll = true, PerStack = true, Do = UpgradeAction.Projectile, Flight = Flight.Homing, Count = 1, Damage = 10f },
}
```

One count to a mark. The HUD names it beside the weapon ("Blades 2/3") while the mark is in force. `Stat` and
`PerPoint` change a stat by each point held (folds of a blade); `Kept = true` keeps the count from scene to scene
until the run ends. A test fails on a mark that banks or spends with no `Tally`.

### 15.6 In the air

`Lift` (and the `pillars` form) throws an enemy 2 m up: it hangs helpless for its seconds and comes down, taking 20
stagger as it lands. Each weapon blow that reaches it holds it up 0.45 s longer, three times (`AirBlows` for more).
Elites and gods hang 0.6 of the time. What cannot be frozen cannot be lifted: the colossus **reels** for a second
instead (and takes the action's `Stagger`), which counts as in the air for `NeedsAirborne` and `AirborneBlow`.

### 15.7 What the player grows

A `Hazard` with `Grown = true`, and Life's base spell, are **grown things**: they are counted, a `Limit` keeps the
oldest of a kind from outstaying a new one, `GrownSeconds` and `GrownDamage` change them, `StatWhen.NearGrown` and
`FarFromGrown` ask whether the player stands by one, and `BurstGrown` sets them all off.

### 15.8 Spells, kits and gifts

- `SpellSpec.HealthCost`: health the cast takes and leaves grey. A spell with it may have `Cost = 0` ("No mana").
- A base spell may wear any form: `ElementKit.SpellForm` with `SpellRadius`, `SpellSeconds`, `SpellInterval`,
  `SpellLinger`, `SpellDelay`, `SpellCount`; `SpellPull` drags what stands in its reach toward the player (negative: away).
- **A mix's first mark does not double-lay.** An `Afflict` on a single target, on a mix mark, on a weapon-blow or
  perfect-guard trigger, that names the held element's own affliction, lays only what it lays beyond the held
  element's root (so with the root learned, one stack laid by both is laid once). The affliction not held is laid in full.
- **Gifts by rule** (`Gifts.cs`): Take gives the Signature of the element taken (`Kind = Signature`, no `Element2`);
  Consume gives the Signature that has both elements when they are neighbours. A gift with a `Form` is worn at once.
  A new tree needs no line anywhere but `UpgradeTrees.AddAll`.
- Roots need not set `WeaponStacks`: Earth and Metal lay theirs on heavy blows by effect, Light on `ChainEnd`.
- A mark may not share its name with another (tested).

### 15.9 Still not possible as data

An effect tied to one spell's hits; a per-enemy debuff or tally; "the target is showing an attack"; changing an
enemy's attacks, guard or sight; a dodge that passes through enemies; a report when a pushed enemy meets a wall; a
revive; the weapon's chain (jump to its end, loop it, remember it through a dodge); poison's beat (bite now, hold,
double); things that stand and block and can be struck (beyond the `wall` and `ring` forms).

## 16. Added for the gifts and crowns (2026-11-16, evening)

### 16.1 A mark may begin a form: `UpgradeAction.Form`

| `UpgradeAction.` | What it does | Fields it reads |
|---|---|---|
| `Form` | casts a spell form by name, free, when the trigger fires: at the trigger's own enemy when it has one, else as a spell cast would aim | `Form` (an id in `SpellForms`), `Spell` (a `SpellSpec`; its `Cost` is 0), `Affliction` (the form is coloured and lays as that affliction's element; left out, the element held), and every condition |

This is how a move that lasts, watches the weapon, or needs the body is hung on a trigger. The form is a
`RunningForm` like any other: it is told of every swing and every blow that lands (`Swung`, `Struck`), and may read
what the player's own parts already count: `PlayerDefence.Blocks`, `PerfectGuards`, `BlowsTaken`, `PlayerCombat.Finishers`,
and on `PlayerUpgrades`: `LastGuardFrom` and `LastGuardEnemy` (who the last guarded blow came from), `DodgeCount`,
`LastDodgeFrom` and `LastDodgeDirection`, and `SurgeAgain`.

```csharp
Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Sun,
    Spell = new SpellSpec { Cost = 0f, Damage = 60f, Stagger = 80f, Radius = 4f } } }
```

### 16.2 The surge button pressed again

A form that waits on a second press of the surge calls `cast.Upgrades.ListenForSurge()` every frame it waits and
watches `SurgeAgain` rise. While any form listens, the press is its own: the surge is not lit over itself and no
mana is spent. With no listener a second press lights the surge again, as before. Used by `sun`, `stillworld`, `sea`
and `spring`. A held surge button is not read: the designs that asked for a hold use this press.

### 16.3 One run that later casts add to

`SpellWorks.Find(form, out found)` hands back the run of a form already in progress, for forms the player has one of
at a time: `dust` (each shatter adds shards) and `plates` (each guard adds plates).

### 16.4 The Rise

`PlayerUpgrades.Rise(attacker)` is asked by `PlayerDefence` when a blow has taken the last health. With Dark's gift
(`dark.lifeforalife`), and once until it is bought back, the player stands again with `RiseHealth` (0.5) of their health
and `Risen` is true; the `rise` form drains them and watches for the finisher. `EndRise(lived, share)` ends it.
`StatWhen.Risen` holds while it lasts, so a stat or an effect may wait on it (`While = StatWhen.Risen`); the Dark marks
that speak of being Risen still read it as "while Hunger lasts" and are to be moved over. `RiseDrain` is the place a
stat for the drain would go.

### 16.5 Poison made to wait

`Afflictions.PoisonWaits` (set by the `cleanblade` form for the length of the surge) stops poison biting and fading on
everything; each `AfflictionSet` counts the seconds (`OwedSeconds`) and `TakeOwed()` pays one bite for each whole beat
at the depth it has then. This is the first piece of "poison's beat" (section 14's gap): a trigger when it bites is
still not there.

### 16.6 Still not possible as data

A held button other than the spell button; a finisher whose animation is replaced (Falling Thunder takes the body
after the finisher's blow lands); "cannot begin an attack" without also stopping movement (Hold stands in).

### 16.7 A spell that lasts while the button is held

`PlayerElement.SpellHeld` is true while the spell button is down. A worn spell is cast after the button has been held for 0.3 s; a form that reads `cast.Caster.SpellHeld` each frame can go on for as long as the hold continues (`frozenwave`).

## 17. Added on the night of 2026-11-16: poison's beat, nine stats, whose spell it was, the Risen, a boon for one blow, the dodge's attacker

Tested in `PoisonBeatTests` (EditMode) and played by `AgentScripts/MarksProbe.cs`.

### 17.1 Poison's beat

| What | How it is written | What it does |
|---|---|---|
| `UpgradeTrigger.PoisonBite` | `On = UpgradeTrigger.PoisonBite` | fires each time poison bites an enemy (on its beat or made to). Target: that enemy. Carries the depth it bit at, so `PerStack` multiplies by it. `Needs` is asked of the bitten enemy |
| `UpgradeAction.Bite` | `Do = UpgradeAction.Bite, Radius = 5f, Amount = 4f` | makes poison bite out of turn on its next step: the target's, or with `Radius` everyone's within it, each at its own depth. `Amount` heals the player that much for each one bitten. The enemy's own beat goes on as it was |
| `UpgradeAction.Pass` | `Do = UpgradeAction.Pass, Affliction = P, Stacks = 1, Reach = 4f` | lays stacks on the nearest other enemy within `Reach` of the target and nothing on the target: a dose that leaps |
| `UpgradeEffect.OnBeat` | `OnBeat = 0.25f` | a condition: the target's poison bit, or will bite, within that many seconds of now (a blow timed to the beat) |

Five rules of the beat are kept by `PlayerUpgrades` as statics on `Afflictions`, each switched by one mark while it is in
force (they are not data; a new mark that wants one needs a line in `PlayerUpgrades.Refresh`):

| Static | Mark | Rule |
|---|---|---|
| `PoisonBeatOff` | Volatile | poison never bites on its beat; it can only be set off |
| `PoisonKeeps` | Slow Death | poison does not fade while its bearer lives |
| `PoisonRipens` (0.25) | Left to Ripen | each bite with no fresh dose since the last is that share stronger than the one before; any fresh dose starts it over |
| `PoisonOnCursed` (0.25) | Wither (Blight) | poison bites that share harder on a Cursed enemy and does not fade from it while the curse lasts |
| `SharedBeat`, `SharedBeatShare` (0.15) | One Sickness | the poison in everything bites on one beat (`Afflictions.TickBeat`); each bite is that share stronger for every poisoned enemy beyond the first that bit on the beat before |

Still not there: burn biting on poison's beat (Slow Burn); a bite that grows with the depth of another affliction; an
affliction with no top at all (Slow Death goes twelve deeper by `AfflictionDepth`).

### 17.2 New stats

| `PlayerStat.` | Kind | What it changes |
|---|---|---|
| `PushDistance` | share | how far the `Push` action, `PlayerCombat.Displace` (outward) and `ShoveArea` throw. Not the shove of a weapon blow |
| `GuardGrey` | share | the part of a blow held on the guard that still comes through as grey health (-0.2 lets a fifth less through) |
| `FreezeSeconds` | share | how long a freeze lasts |
| `ShatterDamage` | share | how hard a shatter lands (`AfflictionPower` still scales it too) |
| `BurstStagger` | share | the stagger of `SetOff` and of the burst of a death |
| `GroundSeconds` / `GroundSize` | share | how long / how wide ground is that a `Hazard` effect lays or a form leaves through `cast.Ground`. Forms that build walls, pillars or spikes of their own do not read them. No mark gives `GroundSize` yet |
| `LeapReach` | share | how far an `Arc` effect or a `Pass` leaps |
| `BoonSeconds` | share | how long every timed `Boon` of the marks lasts |

### 17.3 Whose spell it was

`OwnSpell = true` now also works on `SpellHit`: the effect fires only when the blow is the mark's own spell's. It is
known for what the form strikes during its `Cast` and during every `Tick` of a form that lasts
(`PlayerUpgrades.Casting`). It is **not** known for ground the spell leaves behind (the `Hazards` bite later, by
themselves): those hits count as no one's. `BaseSpell = true` on a `SpellCast` effect holds only for the element's
base spell (the tap), never a worn spell.

### 17.4 The Risen

- `PlayerDefence.Unflinching`: a blow that lands hurts but does not break what the player is doing. Set by
  `PlayerUpgrades` while Risen with Hollow Strength.
- A kill while Risen with Hollow Strength stills the drain for `StilledSeconds` (3): `RiseDrain` reads 0.
- Dead Already: `PlayerUpgrades.Risen` is true for good (so `StatWhen.Risen` holds and every "Risen, ..." mark works
  at all times), health drains `DeadAlreadyDrain` (2) a second while an enemy is within 12 m, slower with Slow Rot,
  stilled by Hollow Strength's kills, never the last point; `Rise()` refuses. `RisenNow` is true only inside the Rise itself.

### 17.5 The account of what is unfinished

`docs/design/marks_open_notes.tsv` has one line for every mark that does not yet do all its words say (id, tab, what
is missing). `unity command script run -- --file AgentScripts/DumpMarks.cs --entry DumpMarks.Open` writes
`docs/design/marks_open.md` from it with the count. Delete a mark's line when it is finished, and run it again.

### 17.6 A boon for one blow, and who was dodged

- `UntilBlow = true` on a `Boon`: it ends with the next swing of the weapon that lands on anything, if that comes
  before its `Seconds` are up (every enemy that swing strikes has it). Eight such boons wait at once.
- `PerfectDodge` now names the attacker, as `PerfectGuard` does: `Afflict`, `Hold`, `SetOff`, `Burst` and the rest at
  `EffectAt.Target` fall on the one whose blow was dodged, and `Needs` is asked of it. A dodged bolt names no one and
  the effect takes the enemy faced, as before.

## 18. Added late on the night of 2026-11-16: keystones written by hand, a state for the heavy swing, no mana, breaths, three debuffs on one enemy

### 18.1 `StatWhen.HeavySwing`

Holds while a heavy blow of the weapon is being wound up or swung (`PlayerCombat.InHeavySwing`). A conditional stat
may stand on it: `new StatMod(PlayerStat.DamageTaken, -0.5f, StatWhen.HeavySwing)`. `PlayerStats.WhenCount` is the
last state plus one: move it when a state is added after this one.

### 18.2 Rules that are switches, kept by `PlayerUpgrades.Refresh`

None of these is data on the mark; each is turned on by owning the mark named, with the element held as always.

| Switch | Mark | What it does |
|---|---|---|
| `PlayerCombat.HeavyUnbroken` | ice.unmoved, earth.heavyhand | Nothing that lands on the player interrupts a heavy blow in its wind-up or swing, red blows included. |
| `PlayerCombat.CounterCut` | metal.countercut | A blow that lands on an `Enemy` whose brain is in Telegraph or Strike is critical. |
| `Afflictions.ChillSlowOff` | ice.allatonce | Chill slows no one; it still freezes at full depth. |
| `Afflictions.BrittleSeconds` (5), `BrittleShare` (0.2) | ice.brittle | When a freeze ends, by thaw or shatter, the enemy is brittle for the time: weapon damage and stagger taken are multiplied by 1 + share. `AfflictionSet.IsBrittle`. |
| `Afflictions.BuckledShare` (0.4) | forge.buckled | An enemy carrying Tremor and Sundered together takes that share more stagger from everything. |
| `Afflictions.FlashFreezeAt` (3) | glacier.flash | A Soaked enemy freezes at that depth of Chill, and the freeze spends the Soaked. |
| `ManaPool.Instead` | life.heartwood | There is no mana: every `TrySpend` is asked of `PlayerUpgrades.PayInHealth` (1 health for 2 mana, left grey, never the last point) and every `Gain` is set aside and mends 1 health for 3 mana on the next frame. `Current` reads as full so that every price may be asked; the bar reads `Fraction`, which is empty. |
| `DodgeRoll.PayInstead` | wind.leaf | The roll asks `PlayerUpgrades.SpendBreath` in place of stamina. Breaths are the mark's own count (`Tally`, cap 3, shown with the other counts); one comes back every `BreathSeconds` (2) and one for every `BreathBlows` (3) blows landed. With none there is no dodge. |

| `WeaponCombo.SkillCostScale`, `SkillAtOnce` | metal.mastersanswer | The weapon skill's mana price is `SkillPrice` (cost times scale). For `AnswerSeconds` (2) after a perfect guard the scale is 0 and a press of heavy releases the skill at once; at any other time the scale is `AnswerDearer` (1.5). |

### 18.3 Moves written by hand in `PlayerUpgrades`

- **Spring Tide** (`TideRises`, `TideSpent`; called where the base spell and a worn spell are paid for): a spell that
  can be paid for is made greater by mana held over its cost, to `TideMost` (2.5), as a boon to `SpellDamage` for
  `TideSeconds` (1.2 s) and to `SpellReach` by the square root; then all the mana is taken.
- **Deep Well** (`OnFlaskDrunk`, `Update`): the flask's own healing is 0; each flask adds `WellPerFlask` (70, times
  `FlaskHeal`) to a well of `WellMost` (140), shown as the mark's count; it pays `WellPerSecond` (6) while a blow has
  landed in the last `WellBlowSeconds` (3) and health is not full.

- **Open Furnace** (`OpenFurnace`, on a perfect guard): every burning enemy within `FurnaceRadius` (8 m) is set off
  where it stands by a detonating blow on itself alone, then given `FurnaceKeeps` (2) Burn back; the `SetOff` trigger
  fires for each. The single detonation of Burn and Detonate is not also made. Its other half is the stat
  `GuardGrey +0.71` (35% of a guarded blow through as grey becomes 60%).
- **Single Stroke** (in `OnSwing`): `AfflictionDepth +5` lets Static lie ten deep with Lightning held; the discharge
  strikes the one enemy (half a metre) at twice the damage and stagger a stack, and does not reach the rest.
- **Clean Break** (`OnShattered`, `FinishBroken`): a shattered `Enemy` under `CleanBreakShare` (a fifth) of its
  health, not a god, is killed on the next frame.
- `StandFirm(seconds)`: a form may ask that the player not flinch for a while (Eye of the Storm does).

### 18.4 The account of what is unfinished, now

`docs/design/marks_open.md` is empty: every mark's words are true. 124 marks were made true on 2026-11-16 by
rewriting their words to what they did; eleven of those have since been built (section 19) and 113 remain reworded; `docs/design/marks_reworded.md` keeps, for each, the old words, the new words and
what the design wanted. To build one to its design: build it, put the designed words back, delete its entry there.

## 19. Marks built to their designs on 2026-11-17 (night run)

Eleven of the 124 reworded marks now do what their designs asked, and Falling Sun was rebuilt as the sun's road.
`docs/design/marks_reworded.md` lists them under "Built since"; 113 remain reworded.

### 19.1 New pieces of the schema

| Piece | Where | What it is |
|---|---|---|
| `StatWhen.InOwnFire` | `StatWhen`, set in `PlayerUpgrades.States` | Holds while Scorched Earth is learned and `Hazards.FireUnder(position)` is true. `PlayerStats.WhenCount` now ends on it: a new state goes after it and moves that constant. |
| `Hazards.FireSecondsScale` | `Hazards.Lay`, set where the other scales are set in `PlayerUpgrades` | Fire patches that stay where they are laid and bite more than once last this many times as long (2 with Scorched Earth). Reset with the other statics. |
| `Hazards.FireUnder(at)` | `Hazards` | True when a standing fire patch, past its delay, covers the point (its radius plus 0.35 m). |
| `PlayerUpgrades.SurgeHeld` | read by `PlayerElement` | While true the surge's clock does not run (`ElementKit.TickSurge` is not called) and flames are thrown up round the player's feet every 0.3 s. |
| `Heat.Raise(levels)` | `Heat` | Whole levels at once, to the maximum: what Wildfire Rush gives for each enemy passed. |
| `PlayerElement.CastBaseFree()` | `PlayerElement` | Throws the base spell as the marks have made it, for no mana and with no wind-up; `SpellsCast` and `OnSpellCast` do not hear it; `FreeCasts` counts it. |
| `PlasmaRules` | `Runtime/Player/PlasmaRules.cs` | Twin Break's and Meltdown's sums. |
| `SunRules`, `SunspotRules`, `LodestoneRules`, `GustRules`, `TideRules.Owed`/`Knock` | `Runtime/Player/Forms` | The plain sums of those moves, each with EditMode tests. |

### 19.2 The marks

| Mark | How it is carried now |
|---|---|
| fire.sun (Falling Sun) | `SunForm`, four stations: Dawn (rolled 6 m/s to the enemy faced, up to 9 m or 2.5 s; a fire patch 0.8 m in radius for 4 s every metre, 4 a bite and one Burn; 14 damage, 20 stagger, two Burn on the first enemy met), Noon (carried up over 0.9 s to 3.8 m, grown to 1.35 m; one Burn a second on every burning enemy within 6 m of the player), Dusk (falls in 0.32 s: the spec's 60 damage and 80 stagger in 4 m, times `SunRules.Weight`), Under (a blow taken: no blow of its own; it rises again 1.5 s later from dawn if the surge had 3 s left). Called while still being rolled: half weight in 2.5 m. |
| fire.rush | `DrillForm` lays `cast.Ground` every 1.1 m when the spec has `LingerDamage` and `Seconds`, and calls `Heat.Raise` for each enemy first struck when the cast lays Burn. |
| fire.kiln | `ParryForm`, when the cast lays Burn: `OnPerfectGuard`, `PerfectGuarded` and the guard's mana, then `StrikeArea` 1.7 m under the attacker in place of the cone. Other elements' parries keep the cone. |
| fire.sunspot | Form `sunspot` (`SunspotForm`). Spec: `Count` 6 (most blows fed), `Seconds` 7, `Radius` 3, `Damage` 12, `Stagger` 10; plus 9 and 8 a blow fed (`SunspotRules`). |
| fire.scorched | Stats `ManaFromBlows +0.5` when `InOwnFire`, `SpellCost +0.2`; the rest by the switches in 19.1. |
| lightning.waiting | Form `waiting` (`WaitingThunderForm : PhalanxForm`, `FliesByItself` false, `Piece` Bolt). |
| lightning.lodestone | Form `lodestone` (`LodestoneForm`). Spec: `Radius` 7 (how far it reaches for enemies), `Delay` 0.3, `Damage` 14, `Stagger` 30, `Stacks` 0. The burst reaches 3.5 m round the player (`LodestoneRules.BurstRadius`). |
| plasma.twin | `PlayerUpgrades.TwinBreak`, called in `OnPerfectGuard` before the trigger; the two `SetOff` effects stay for an enemy carrying only one. `TwinBreaks` counts. |
| plasma.meltdown | `PlayerUpgrades.Melt`, looked for four times a second among twelve enemies within 14 m; no effects in its data. `Meltdowns` counts. |
| wind.front | No effects in its data; `PlayerUpgrades` owes a `CastBaseFree` to each heavy blow or finisher that lands, at most one every 0.4 s. |
| lightning.reserve | `Tally` "Charge", cap 3, no effects: `PlayerUpgrades` adds one with each perfect guard (where Stored Charge is armed) and, in `States`, takes one off and arms the next when the armed blow has been spent. |
| fire.firebomb | Unchanged: found to be at its design. |

### 19.3 Riptide and Gust

Riptide's carry is now a debt: when the wave's face takes a body it is owed `TideRules.Owed(along)` metres (the way
to where the wave breaks, less `Knock`, the 0.45 m the blow itself moves it) and is paid that as the wave comes in,
kept in the run's `Times`. Gust gathers what stands within reach and pushes it over the 0.45 s it blows
(`GustRules.Impulse` 4.5, which is 3 m of travel), so the blow's own flinch no longer swallows the throw.

## 20. Spell branches (Phase 4, 2026-11-20)

The trees are being reshaped into spell branches (`docs/design/steel/IMPLEMENTATION.md` 2.1, 2.2, task 4.1; the
designs are `docs/design/steel/elements_a.md` and `elements_b.md`). Fire, Lightning, Wind, Ice, Water and Life are written
this way; the other five keep the shape of sections 9 to 19 until their turn (tasks 4.6 to 4.14). Everything in sections 1 to 19 still
holds: a branch's marks are ordinary marks with a few more fields.

### 20.1 The shape of a tree

About 34 marks: **six spells of four marks each** (24) and **about ten passives**: the root, majors, one or two
keystones, the **Devotion**, the **reaction lift** and the god's gift. One spell per element is its larger spell, paid
in mana (`Magic/SpellTimings.cs`); the rest run on cooldowns. One of the six is the element's **base spell**, which
comes with the element.

| Part | Kind | Price | What it is |
|---|---|---|---|
| Learn | `Spell` | 4 Shards (the base spell's: given) | The spell, cast from a slot. Its id is the branch's id (`fire.rush`). |
| Empower | `Minor` | 1 | "Stronger, shorter cooldown": an `Empower` record of the numbers it changes. |
| Shape | `Major` | 3 | Opens two `ShapeDef`s; the loadout says which one the spell wears (`Loadout.Shapes`, keyed by the Learn id), changed at a rest fire. Before it is bought the base spell is cast. |
| Master | `Major` | 3 | A reward for timing the spell well: effects with `OwnBranch` and the clock actions. |
| Devotion | `Devotion` | 6 | In force only all in (the same element in both attunements). Drawn as a keystone. |
| Reaction lift | `Major` with `Lifts` set | 3 | Its effects ride `Reacted`, with `Needs` the element's affliction (true for every reaction it is in). |

### 20.2 New fields

| Field | Type | Meaning |
|---|---|---|
| `Upgrade.Currency` | `Currency`: `Shards` (default), `Memories`, `None` | What it is bought with. `None`: given, never sold, costs 0 (the base spell's Learn). |
| `Upgrade.Branch` | string | A branch mark's Learn id; empty for a passive. |
| `Upgrade.Part` | `BranchPart`: `None`, `Learn`, `Empower`, `Shape`, `Master` | Its place in the branch. |
| `Upgrade.Shapes` | `ShapeDef[]` | On a Shape mark, its two shapes. |
| `Upgrade.Empower` | `Empower` | On an Empower mark, the numbers it sets. |
| `Upgrade.Role` | `SpellRole` | On a Learn: the role icon in the Hand. |
| `Upgrade.Lifts` | string (element) | On a reaction lift: the element whose reactions it lifts. |
| `Upgrade.Aspect`, `Gate`, `Seen` | string | For the Way (phase 5) and Heresies (phase 6); read by nothing yet. |
| `UpgradeEffect.OwnBranch` | bool | For `SpellCast` and `SpellHit`: only when the spell is this branch's own. A base spell cast by its kit counts as every base spell's branch (the element of a kit's cast is not known yet: a `NEEDS`). |
| `UpgradeEffect.Branch` | string | For the clock actions: the branch whose slot is acted on; empty for its own mark's. |

`Upgrade.IsBaseSpell` is true for a Learn whose id is a base id (`fire.base`); `UpgradeTree.Reachable` counts a base id as
learned, so a base spell's Empower is open at once. New kinds and their usual prices: `Devotion` 6, `Evolution` 5,
`Heresy` 8, `Aspect` 2, `Technique` 2, `Temper` 1 (the dearer ranks write their own), `Bond` 0.

`ShapeDef`: `Name`, `Description`, `Form` (a form cast in place of the base spell's; empty keeps it), `Spell` (a whole
spec in place of the base spell's numbers, which keeps the Learn's cooldown and price), `Change` (a `SpellChange` of
numbers set on the base spell or on `Spell`), `Stats` and `Effects` (in force only while that shape is worn; each
effect's owner is the Shape mark, so a `Bank` or `Spends` there needs a count on the Shape mark, not the Learn).

`SpellChange` and `Empower`: `Cooldown`, `Mana`, `Damage`, `Stagger`, `Reach`, `Radius`, `Arc`, `Seconds`, `Interval`,
`Count`, `Stacks`, `Delay`, `Speed`, `LingerDamage`, `AfflictionSeconds`. A field left at zero leaves the spell's number
alone, so no number can be changed to zero this way: a shape that needs a zero gives a whole `Spell`.

### 20.3 New actions

| Action | Reads | What it does |
|---|---|---|
| `CutCooldowns` | `Amount` (seconds) | Takes seconds off every running spell and strike clock, as a landed blow does. |
| `ResetCooldown` | `Branch` | Ends the clock of the branch's slot (its own, or the one named). Nothing when that spell is in no slot. |
| `InstantNext` | `Branch`, `Seconds` | The next cast of the branch's slot starts no clock. With `Seconds`, only within that window, and a running clock is left running ("cast in the same instant as..." asks that it was ready); without, until it is cast, and the clock is ended now. |

They are done by `Magic/BranchCooldowns.cs` on the slots' own clocks (`PlayerSpells.Answer`).

### 20.4 Writing a branch, and the pattern

`Trees/SpellBranch.cs` writes the four marks in one call; `Trees/BranchPattern.cs` gives the places. A writer never
does arithmetic: name a stalk for a branch and a place for a passive.

```csharp
SpellBranch.Add(to, E, "fire.rush", "Wildfire Rush", SpellRole.Movement, BranchPattern.LeftB, Upgrade.FlameTrail, SpellForms.Drill,
    new SpellSpec { Reach = 7f, Damage = 14f, Stagger = 20f, Stacks = 2, Speed = 20f, Seconds = 2.6f, LingerDamage = 4f },
    "You drive seven metres through the enemies ahead...", UpgradeIcons.Arrow,
    new Empower { Cooldown = 6f, Seconds = 4f }, "Wildfire Rush waits 6 seconds, not 8...",
    new ShapeDef { Name = "Fire Front", Description = "...", Change = new SpellChange { Reach = 5f, Radius = 2f, Speed = 7f } },
    new ShapeDef { Name = "Smoke Road", Description = "...", Change = new SpellChange { Reach = 6f } },
    "Opens two shapes of Wildfire Rush...",
    new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.CutCooldowns, Amount = 1.5f } },
    "Started in the instant a blow would land on you, it counts as a perfect dodge...");
to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.FlameTrail, ... , Requires = new[] { Upgrade.Backdraft } }, BranchPattern.LeftMiddle));
```

The ids: the Learn keeps the spell's own id; the others are `<id>.empower`, `<id>.shape`, `<id>.master`. A base spell's
branch is written the same way with the base id (`fire.base`), an empty form and a null spec: its kit casts it, and a
shape with a form of its own is cast through the mark instead (`SpellBranches.CastsThroughMark`). Optional arguments:
`weapon`, `tally` and `learnEffects` (on the Learn: Ring of Drums keeps its drums there), `masterStats`.

The pattern, upright (X across, Y outward), 36 places for a tree's 34 marks, proved for all eleven elements beside every
mixed branch by `UpgradeChartTests.TheSpellBranchPatternFillsAllElevenElementsAndEveryMixWithoutACrowdAndTheStickReachesAll`:

| Place | X, Y | Grows from | Used for |
|---|---|---|---|
| `root` | 0, 1.2 | | the root |
| `M.foot` | 0, 2.5 | root | the middle bough's first passive |
| `M.a`, `M.b` | -0.48 and 0.48; 3.45, 4.4, 5.35, 6.3 | M.foot | two branches |
| `M.crown` | 0, 7.35 | M.foot | a passive up the trunk (the lift, a Devotion) |
| `T.1`, `T.2` | -0.62 and 0.62, 8.5 | M.crown | the top of the trunk (a keystone, a Devotion) |
| `L.foot`, `R.foot` | -1.55 and 1.55, 2.55 | root | each side bough's first passive |
| `L.a`, `R.a` | -1.38 to -1.6 (mirrored), 3.6 to 6.4 | its foot | a branch; also the base spell's, grown from the root (its line passes clear) |
| `L.mid`, `R.mid` | -2.45 and 2.45, 6.0 | its foot | the side bough's middle passive |
| `L.b`, `R.b` | -2.6 to -3.05 (mirrored), 7.0 to 9.85 | its middle | a branch; may grow from its foot when the middle is empty |
| `L.crown`, `R.crown` | -1.6 and 1.6, 8.6 | its middle | a keystone |
| `gift` | 0, 12 | | the god's gift |

A writer may grow a mark from another place than the table's as long as it lies further out than its parent and its
line misses every other mark by 0.4 steps (Lightning's Single Stroke grows from Ring of Drums' Learn). The chart takes its
places from an `IChartLayout` (`RingLayout`, the ring as it is; the heart and the Way come with tasks 4.5 and 5.5).

### 20.5 What is in force, and what is cast

`MarkRules.InForce` adds two rules: a branch's Empower, Shape and Master are in force only while its Learn sits in a
slot, and a `Devotion` only all in. A Shape mark adds the stats and effects of the shape worn and no other
(`SpellBranches.Worn`). As a spell is cast, `SpellBranches` (one instance on `PlayerUpgrades`, one spec kept per spell,
so a cast allocates nothing after the first) gives its form and numbers: the Learn's, then the worn shape's whole spec
or change, then the Empower's change; the slot's wait is that spec's cooldown. A base spell cast by its kit takes only
its Empower's wait (`SpellBranches.BaseCooldown`): its other numbers are the kit's, a `NEEDS` until base spells are cast
through their marks. A shape is chosen with `PlayerBuild.ChooseShape(branch, choice, atRestFire)` and kept for the run
(`RunState.Shapes`); the Hand screen has no control for it yet.

### 20.6 Retired marks

A mark the design cuts is deleted from its tree file and listed in `Trees/RetiredMarks.cs` with what it cost; a save that
holds one is refunded and the id leaves the Hand's slots, the kept Hands and the shape choices
(`SaveMigration.RemoveRetired`, every load). A retired id is never used again (`UpgradeChartTests.NoRetiredIdIsUsedAgain`).
The Katana-bound marks that leave the element trees for the Katana's Way (Burning Crescent, Delayed Cuts) are listed
apart, in `RetiredMarks.Moving`: a save keeps them and nothing is refunded, because the Way will take the same ids.
Kept marks keep their ids, even renamed (`fire.backdraft` is Breath of the Lioness).

### 20.7 What the data cannot say yet

Each such piece is written at its nearest working version with a `// NEEDS:` line beside it and a line in
`docs/design/steel/content_open.tsv`, as the reactions are.
