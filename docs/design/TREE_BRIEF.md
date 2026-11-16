# The great chart: brief for the element trees

Written by Claude on 2026-11-16 at the start of the day run, from the user's request: "make a massive upgrade tree,
with 30-40 possible options for each element including the mixed elements as well as the super special boss one, also
be creative come up with super cool, flashy, and interesting moves", and "a branching tree outwards in many directions
... with some interconnected stuff for other elements; when you choose an element it will stay zoomed to that
element's upgrade tree". Also from play: "it seems too easy to collect embers ... the goal is to make it as if you
have to choose a path in the tree". Everything below that the user did not say is Claude's default and is marked so
in `docs/GAME.md` when it lands.

## The ring

Eleven elements stand round one middle, each tree growing outward. Neighbours on the ring share a **mixed branch**
that grows in the gap between their two trees. Order, clockwise from straight up, 360/11 degrees apart:

| # | Element | Colour (linear-ish RGB) | Affliction it lays on enemies | Mix with the next one round |
|---|---|---|---|---|
| 0 | Fire | 1.00 0.56 0.20 | **Burn**: damage over time, up to 5 deep | **Plasma** (Fire + Lightning) |
| 1 | Lightning | 0.45 0.78 1.00 | **Static**: up to 5; a heavy blow or finisher discharges it and it arcs | **Tempest** (Lightning + Wind) |
| 2 | Wind | 0.62 0.95 0.80 | **Unsteady**: pushed further, turns slower, staggers sooner | **Blizzard** (Wind + Ice) |
| 3 | Ice | 0.70 0.92 1.00 | **Chill**: slowed, up to 5; at 5 it is **Frozen** (held still; a heavy blow shatters it for big damage) | **Glacier** (Ice + Water) |
| 4 | Water | 0.25 0.50 1.00 | **Soaked**: takes more from spells; carries Static and Chill to those near | **Bloom** (Water + Life) |
| 5 | Life | 0.45 0.90 0.35 | **Seeded**: blows on it heal the player a little; it bursts into thorns when it dies | **Overgrowth** (Life + Earth) |
| 6 | Earth | 0.72 0.52 0.30 | **Tremor**: slowed and its balance does not recover | **Forge** (Earth + Metal) |
| 7 | Metal | 0.78 0.80 0.88 | **Sundered**: armour broken, takes more damage from weapons | **Halo** (Metal + Light) |
| 8 | Light | 1.00 0.93 0.60 | **Marked**: blows on it are critical and give more mana | **Eclipse** (Light + Dark) |
| 9 | Dark | 0.62 0.30 0.85 | **Cursed**: deals less; killing it heals the player | **Blight** (Dark + Poison) |
| 10 | Poison | 0.60 0.85 0.15 | **Poisoned**: stacking damage that lands in bursts every 2 s | **Pyre** (Poison + Fire) |

Self buffs (the element's **surge**, held on L2) start from the user's table in `docs/GAME.md` ("Elements (Draft)"):
Fire damage up; Water flasks heal more; Light purification; Dark one revive as a draining "zombie"; Metal defence up;
Poison heal over time from poison dealt; Life heal; Lightning critical damage up; Wind better dodge; Ice a barrier that
blocks the next hit; Earth resistance to stagger and status. Trees are where these grow.

## What a tree holds (per element, 30 to 40 marks counting its share of mixes and the god's gift)

- **One root** (cost 2): turns the element's affliction on for weapon blows, in its simplest form.
- **Three boughs** from the root, each with its own idea of how to fight (for example Fire: *Pressure* (heat, momentum,
  never stop hitting), *Answer* (perfect guards and dodges turned into fire), *Ruin* (big spells, craters, the flask)).
  A bough is a spine of 3 or 4 **major** marks with **minor** marks hanging off them in twos and threes, and it ends in
  a **keystone**.
- **Minor** (cost 1): a plain number that suits the bough. One line. About half the tree.
- **Major** (cost 3): a new rule: something happens on a trigger. One or two sentences.
- **Spell** (cost 4): a move the player performs. Learned spells are worn in the **hold-spell** slot (hold Triangle);
  the tap stays the element's base spell. Each element has 4 or 5. These are the flashy ones.
- **Keystone** (cost 6): the end of a bough; changes how the element plays. Three per element, and the cost of all
  three is far more than a run pays, so a path must be chosen.
- **Mix** (cost 3 to 5): six to eight marks in the branch shared with each neighbour, at least one a spell. Open only
  to a player who holds one of the two elements and has **consumed or held** the other in this run.
- **Signature** (never sold): the god's gift at the crown. Given by taking the element from its god (the Take reward)
  or, for the mix signature, by consuming. One per element: the "super special boss one". It should be the single
  most spectacular thing in the tree.

Own marks per element: 1 root + about 14 minor + about 7 major + 4 or 5 spells + 3 keystones + 1 signature = about 31.
With half of two mixed branches that is 38 or so.

A run through the castle and its god pays 19 embers; later areas will pay more. Do not design to that number; design
so that three boughs cannot all be finished.

## What the engine can do without new code (design to these; flag anything else as BESPOKE)

**Triggers**: a light blow lands; a heavy blow lands; a finisher lands; the weapon skill lands; a blow kills; a
perfect guard; a plain guard; a perfect dodge; any dodge begins / ends; the player is struck; a spell is cast; a spell
hits; the surge begins / ends; a flask is drunk / thrown; an afflicted enemy is hit; an affliction reaches full depth;
every N seconds in a fight; health below a third; mana full.

**Actions**: lay N stacks of an affliction on the target / on everything within R; a burst of damage and stagger
round the target or the player; a cone thrown forward; a line or a ring of ground hazard that lasts; an arc that leaps
between enemies; a projectile (straight, homing, or returning); a blade or pillar that falls on the target after a
delay; heal; give mana; give stamina; a timed boon on the player (more damage, speed, guard window, damage taken down,
cost down); a ward that blocks the next hit; a short step (teleport) to or through the target; a pull or a push; slow
time for everything but the player; a copy of the player that repeats the last blow; a summoned helper that strikes a
few times and goes; set off an affliction (spend its stacks for a burst).

**Numbers a minor can change**: weapon damage; stagger dealt; damage to afflicted enemies; affliction depth, length
or strength; spell damage, cost, reach; surge length, cost; mana from blows, guards, dodges; stamina costs; dodge
distance and its safe time; perfect guard window; health; flask healing and count; critical chance and damage; speed
while the surge lasts; damage taken; grey health kept; embers earned.

**Spell forms** (each can be worn by any element in its colour with its affliction): wave; chain bolt; blades from
above; giant beam; phalanx blades that hover then fire; dragon head that lunges; nova round the caster; slow tracking
orb that detonates; charged great bomb; spiked floor (line or field); drill dash through enemies; minions; ring of
many bullets set round the target then fired together; reverse blast (pulls in, then bursts); leap and crater; planted
totem that keeps striking; elemental parry (a guard stance that answers); wall or pillar that blocks and bursts; twin
spinning discs that orbit; a mark that detonates when struck; rain over an area; a clone that fights beside you.

BESPOKE moves beyond these are welcome where they are the point of the element (at most five per element, the
signature among them). Describe exactly what the player presses, what is seen, and what it does.

## Rules of the house

- Close combat comes first: every spell either sets up a blow or is paid for by blows (mana only comes from landing
  hits, perfect guards and perfect dodges). Nothing that wins a fight from across the room.
- Timing skill is the heart: perfect guard and perfect dodge should be the trigger of each element's best things.
- Readable from a high camera: shapes on the ground, bold silhouettes, one saturated colour per element.
- Names are two or three plain words with weight ("Stored Charge", "Burn and Detonate", "Meteor Leap"). Descriptions
  are one or two short sentences in plain words, saying exactly what happens, in the present tense, no numbers unless
  they are a count the player sees ("up to five"), no exclamation marks. Read the existing fourteen in
  `Assets/_Project/Scripts/Runtime/Player/Upgrade.cs` for the voice.
- Fire and Lightning keep their existing marks (same ids and names) and grow round them.
