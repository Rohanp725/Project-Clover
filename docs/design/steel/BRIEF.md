# The shared brief for the Steel and Element content (2026-11-18)

Seven writers fill in the content of `docs/design/STEEL_AND_ELEMENT.md` at once, each a slice. This brief is what
all of them share, so the pieces fit together. Read it whole before writing, then read the design document itself.

## Read first (everyone)

- `docs/design/STEEL_AND_ELEMENT.md`: the design. Everything you write is content inside it.
- `docs/GAME.md`: pillars, the voice of in-game text (grim and sparse, in the manner of Sekiro: few words, plain
  and weighty, no jokes), and the section "Steel and element: the rework of 2026-11-18".
- `docs/GUIDE.md` section 4 (Elements): the ring, each element's affliction, tap spell and surge, how each
  affliction behaves, the mixed branches and the gods' gifts. Section 6 onward lists the marks and spells as built.
- `docs/design/WORLD.md`: each god's mythology (Fire Egyptian; Lightning and Wind Japanese; Water Polynesian; Ice
  Norse; Earth and Life Celtic; Metal Greek; Poison Chinese; Light and Dark Indian). Mine it.
- `docs/design/LOOKS.md` section 1 and each land's section: every element is drawn as its own substance (fire is
  flame, smoke and embers, never white or gold light; water is spray, foam and ink; metal is sparks and molten pour;
  ice is frost, shards and breath; poison is mist, drip and bubbles). The owner rejects effects that read as plain
  coloured light.
- `docs/UPGRADE_SCHEMA.md`: what the effect engine can already do (forms such as Nova and Rain, effects, triggers).
  Prefer what exists; when you need something new, say so plainly in one line.
- For your slice, also: `docs/design/trees_a.md`, `docs/design/trees_b.md`, `docs/design/TREE_BRIEF.md`,
  `docs/design/FORMS.md`, `docs/design/weapons.md` as they bear on it.

## The ring and the afflictions

| # | Element | Affliction | Mixes already built with the next one round |
|---|---|---|---|
| 0 | Fire | Burn | Plasma (Lightning) |
| 1 | Lightning | Static | Tempest (Wind) |
| 2 | Wind | Unsteady | Blizzard (Ice) |
| 3 | Ice | Chill (deepens to Frozen) | Glacier (Water) |
| 4 | Water | Soaked | Bloom (Life) |
| 5 | Life | Seeded | Overgrowth (Earth) |
| 6 | Earth | Tremor | Forge (Metal) |
| 7 | Metal | Sundered | Halo (Light) |
| 8 | Light | Marked | Eclipse (Dark) |
| 9 | Dark | Cursed | Blight (Poison) |
| 10 | Poison | Poisoned | Pyre (Fire) |

A pair's **distance** is how far apart they are round the ring (1 to 5). The 55 pairs fall into five groups of 11.

## Research (everyone)

Do real research, with web searches, before inventing. Look for:

- **Real phenomena** where the two substances meet: chemistry and physics (thermite, the Leidenfrost effect,
  galvanic corrosion, superconductivity, flash freezing, petrichor, phosphorescence, quenching, tempering, rust,
  dry lightning, fulgurite, eclipse shadow bands), weather, biology, the forge.
- **Myth and story** from the two gods' cultures, and above all where the two meet. Fire and Ice is Muspelheim and
  Niflheim meeting in Ginnungagap; the Egyptian sun crossing the Norse winter. Use the meeting of the two cultures to
  give each duo its own image.
- **Games and media** that combine elements well: Hades's duo boons (two gods' powers made one), Genshin Impact's
  reactions, Divinity: Original Sin 2's surfaces, Magicka's combining, Breath of the Wild's chemistry, Noita, Path of
  Exile, Wizard of Legend, Avatar: The Last Airbender's sub-bendings (metal, lava, blood, lightning), Demon Slayer's
  breathing styles, Jujutsu Kaisen, Final Fantasy XVI. Learn the principle; never copy a name or an effect whole.

End your file with a short "Sources" list of what you used.

## Rules for everything you design

1. **Readable from above.** The camera is 12 m away, pitched 55 degrees. Every effect must be told at a glance by
   shape and substance, not by a number or a tint.
2. **One idea each.** A reaction is one small effect; a mark is one sentence of rule. If it needs a paragraph, cut it.
3. **Both, not added.** A duo must feel like the two substances becoming something new, not two effects side by side.
4. **Changes play, not only numbers.** A shape or keystone changes what the player does. Plain number marks are
   allowed (minors) but are never the interesting part.
5. **Steel and element are equal.** Where it fits, a duo or spell should reward weaving weapon and spell.
6. **Fair and top-down.** No effect that hides the player, fills the screen or makes the fight unreadable.
7. **Names** are plain English, short and weighty, in the game's voice ("Seven Flashes", "The Wave Stands Still",
   "Pawned Heart"). No borrowed names from other games. Mythic nods are welcome when plain.
8. **Numbers** are first guesses, shown as plain values (damage, radius in m, seconds). Keep them in the ranges the
   built spells use (GUIDE.md).
9. **Do not repeat** an idea already built (GUIDE.md) or another writer's slice where you can see it. If two pairs in
   your slice want the same idea, give it to the one where it fits best.

## Formats

Every file starts with a one-line-per-item **index** (name and one clause), so it can be read quickly, then the
full entries.

### A pair (reaction writers)

```
### <Element A> and <Element B> (<Affliction A> + <Affliction B>), distance <n>

**The image.** One or two sentences: what these two substances become when they meet, and the myth or phenomenon it
comes from.

**Reaction: <Name>.** When it happens (both afflictions on one enemy; say if order matters). What it does, with
numbers. What the player sees.

**Shape A: <Name>.** How it changes the reaction. **Shape B: <Name>.** The other way.

**Duo branch** (three to five marks):
| Mark | Kind | Shards | What it does |

**Duo spell: <Name>** (the crown). Role (opener, breaker, detonator, counter, movement). Cooldown or mana. What it
does, with numbers. What it looks like. How it rewards steel.

**Duo surge: <Name>** (only where it is strong and distinct; optional).
```

For the 11 distance-1 pairs, whose branches already exist (Plasma and the rest), write the image, the reaction and
its two shapes, then a short note on what of the built branch to keep, cut or re-aim at the reaction. Do not rewrite
the built branch.

### An element (element writers)

```
## <Element>

**Roster.** About six spells covering at least four roles, built from the spells that exist (tap spell, worn
spells, signature moves; GUIDE.md) plus new ones to fill gaps. For each:
| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |

**Charged blade.** What three charged blows do with this element, and how the blade looks.
**Infused strike.** What L2 with R1 does with this element (generic to all weapons; weapons vary it on their Way).
**Devotion: <Name>.** The all-in passive. Strong, distinct, changes play.
**Reaction lift: <Name>.** The tree mark that lifts every reaction this element is part of.
**Passives.** The other marks of the tree (about ten in all with the above), one line each.
```

### A weapon (the weapon writer)

```
## <Weapon>

**Aspects.** Three boughs (a fourth for a god's weapon), each a form of the weapon: name, what changes, three to five
marks with moves as well as numbers, and a keystone.
**Temper.** How temper marks sit on it.
**Infusion.** How this weapon's infused strike varies the generic one (one line per element family, or a rule).
**Gated technique.** Which miniboss teaches it, and the move.
```

## Where to write

Your own file under `docs/design/steel/` (named in your task). Do not edit any other file. Do not commit.
