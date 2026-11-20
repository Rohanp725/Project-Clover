# Windthrow: detailed explanations

The long explanation the chart shows for every mark on the duo branch Wind and Life share, named for its reaction,
Windthrow (`docs/design/steel/pairs_d3.md`, "Wind and Life"). Drawn in the heart of the chart while Wind and Life are
both carried.

## windthrow.shape
**What it does.** Opens Windthrow's two shapes. At a rest fire you choose which one your Windthrow wears, and from then on it runs instead of the plain one. **Blown Seed**: the shove does not tear it out; its seeds blow off downwind and root what they reach. **Deep Roots**: it holds longer, your blows on it heal you, and tearing it out staggers harder.

**When it happens.** Every time Windthrow fires (Seeded landing on an unsteady enemy, or Unsteady on a seeded one) while Wind and Life are both carried. Before this mark is bought, or with only one of the two carried, the plain Windthrow runs.

**The numbers.** Mix, 3 Shards. Blown Seed: rooted 2 seconds; your heavy blow throws its seeds up to 6 metres beyond it, away from you, two Seeded on all they reach, which on purpose may root them. Deep Roots: rooted 4 seconds; each blow on it heals you 2; the heavy blow tears it out for 14 damage and 60 stagger, throws it 6 metres and leaves a torn pit. Windthrow rests 6 seconds on each enemy.

**Works with.** Gust and Briar Patch to bring the two, Seed on the Wind, The Fairy Wind, Thistledown.

**Example.** Blown Seed on the front soldier of a line: the line roots behind it.

**Not built yet.** Only a heavy blow tears it out or blows its seeds, not any shove, and Deep Roots heals a flat 2 a blow.

## windthrow.seedonwind
**What it does.** A dodge that passes within 2 metres of an enemy leaves it Seeded and Unsteady.

**When it happens.** At the end of each dodge, while Wind and Life are both carried.

**The numbers.** Mix, 3 Shards. One Seeded and one Unsteady on everything within 2 metres of where you stop.

**Works with.** Windthrow (both at once root it), Thistledown, Green Wood.

**Example.** Roll past a soldier and it is rooted to the floor behind you.

**Not built yet.** It reaches those near where the dodge ends, not those it passed on the way.

## windthrow.tornup
**What it does.** A heavy blow on a rooted enemy counts as a shove: it tears it out, whatever the weapon.

**When it happens.** Every heavy blow or finisher that lands on an enemy carrying both Seeded and Unsteady.

**The numbers.** Mix, 3 Shards. As built: it is thrown 3 metres away from you, at most twice a second.

**Works with.** Windthrow, The Fairy Wind, any weapon.

**Example.** The Hammer's heavy blow throws a rooted soldier across the room.

**Not built yet.** As built, Windthrow already tears out on any heavy blow; this adds a throw.

## windthrow.greenwood
**What it does.** Torn pits stand longer and seed one deeper.

**When it happens.** Each time an enemy is torn out.

**The numbers.** Mix, a minor, 1 Shard.

**Works with.** Seed on the Wind, which it grows from, Windthrow.

**Example.** The pits left behind keep seeding the fight.

**Not built yet.** It does nothing yet: a mark that changes the reaction's own numbers is still to come.

## windthrow.fairywind
**What it does.** Where an enemy is torn out, a small whirlwind rises and wanders for 4 seconds, making what it passes Unsteady and Seeded, so tearing one out roots the next.

**When it happens.** Each time Windthrow fires, while Wind and Life are both carried.

**The numbers.** Mix, keystone, 5 Shards. As built: a patch 2.4 metres across for 4 seconds where it reacts, an Unsteady every half second and a Seeded every second to what stands in it; on purpose these may start Windthrow again.

**Works with.** Torn Up, Seed on the Wind, Thistledown.

**Example.** One windthrow in a pack starts the next.

**Not built yet.** The whirlwind stands still rather than wandering.

## windthrow.thistledown
**What it does.** You come apart into a drift of seed-down and blow along the stick, passing through enemies. Each one passed is Seeded and Unsteady two deep, so it roots as you go by.

**When it happens.** From a spell slot, while Wind and Life are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Wind.

**The numbers.** Mix, duo spell, movement, 5 Shards. No mana: it waits 9 seconds after a cast, less with weapon play. 7 metres; everything within 1.5 metres of the line takes 4 damage, two Unsteady and two Seeded.

**Works with.** Windthrow, Torn Up, The Fairy Wind.

**Example.** Drift through a pack and turn to find them rooted.

**Not built yet.** You are not yet untouchable in the drift, no ring of flowers forms where you land, and a light blow just after does not yet count as a shove.
