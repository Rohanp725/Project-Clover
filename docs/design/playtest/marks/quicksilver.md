# Quicksilver: detailed explanations

The long explanation the chart shows for every mark on the duo branch Metal and Poison share, named for its reaction,
Quicksilver (`docs/design/steel/pairs_d3.md`, "Metal and Poison"). Drawn in the heart of the chart while Metal and Poison
are both carried.

## quicksilver.shape
**What it does.** Opens Quicksilver's two shapes. At a rest fire you choose which one your Quicksilver wears, and from then on it runs instead of the plain one. Both keep the beads: while it carries both, each of your weapon blows makes its poison bite at once. **Heavy Silver**: the mercury pools under it, slowing it and dosing what steps in. **Running Silver**: when it dies, its mercury runs along the floor to the nearest enemy and gives it doses and notches.

**When it happens.** Every time Quicksilver fires (poison landing on a notched enemy, or a notch on a poisoned one) while Metal and Poison are both carried. Before this mark is bought, or with only one of the two carried, the plain Quicksilver runs.

**The numbers.** Mix, 3 Shards. Both: a bite at once on each weapon blow, at most every half second, nothing spent. Heavy Silver: it moves 30% slower while it carries both, and a puddle 1.5 metres across stands 6 seconds where it reacted, a dose every second and a notch every 2 seconds to what stands in it. Running Silver: four doses and two notches pass to the nearest enemy within 6 metres when it dies. Quicksilver rests 6 seconds on each enemy.

**Works with.** Miasma and Flung Blade to bring the two, Envenomed Edge, The Nail Pulled, Ichor.

**Example.** A brute that runs: Heavy Silver. A pack: Running Silver walks the poison from one to the next.

**Not built yet.** The puddle stays where it reacted rather than following the enemy, and Running Silver passes a set four doses and two notches rather than all it carried.

## quicksilver.edge
**What it does.** Heavy blows lay one notch and one dose.

**When it happens.** Every heavy blow or finisher that lands, while Metal and Poison are both carried.

**The numbers.** Mix, 3 Shards. One notch and one dose; of your first element's, none beyond its root's.

**Works with.** Quicksilver (one heavy blow starts it), Cinnabar, The Nail Pulled.

**Example.** Each heavy blow on a fresh enemy sets the mercury running.

## quicksilver.nail
**What it does.** A finisher on a Quicksilver enemy pulls the nail: all its poison bites at once, twice.

**When it happens.** Each finisher that lands on an enemy carrying both Sundered and poison.

**The numbers.** Mix, 3 Shards. Two bites at once, 0.3 seconds apart.

**Works with.** Quicksilver, Envenomed Edge, Miasma.

**Example.** Eight doses and a finisher: the poison lands twice over.

## quicksilver.cinnabar
**What it does.** Notches last as long as the poison does.

**When it happens.** Always on once learned, while Metal and Poison are both carried.

**The numbers.** Mix, a minor, 1 Shard.

**Works with.** Envenomed Edge, which it grows from, Quicksilver.

**Example.** A poisoned soldier stays notched.

**Not built yet.** It does nothing yet: an affliction kept as long as another is still to come.

## quicksilver.drink
**What it does.** Your flasks are cut with quicksilver: drinking one throws a ring of silver drops round you that poison three deep and notch everything, but each flask heals a fifth less.

**When it happens.** Each time you drink a flask, while Metal and Poison are both carried.

**The numbers.** Mix, keystone, 5 Shards. Three doses and a notch on everything within 4 metres. Flasks heal 20% less.

**Works with.** Quicksilver (everything near now carries both), The Nail Pulled, Wellspring.

**Example.** Drink in the middle of a pack and the pack starts biting itself.

## quicksilver.ichor
**What it does.** You raise the guard. The first blow that lands on it is turned, and your answer pulls the nail: the attacker is notched to full, poisoned three deep, and its poison bites.

**When it happens.** From a spell slot, while Metal and Poison are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Metal.

**The numbers.** Mix, duo spell, counter, 5 Shards. No mana: it waits 11 seconds after a cast, less with weapon play. The guard stands 1.2 seconds; the answer: 10 damage and 20 stagger in a cone 3 metres long, notched to full, three doses, and a bite.

**Works with.** Quicksilver, The Nail Pulled, Envenomed Edge.

**Example.** Turn a brute's swing and it bleeds silver.

**Not built yet.** It does not yet bleed for 4 seconds with its poison quickened, your blows in that time do not add doses, and half the cooldown does not come back when nothing strikes.
