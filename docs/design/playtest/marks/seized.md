# Seized: detailed explanations

The long explanation the chart shows for every mark on the duo branch Lightning and Water share, named for its
reaction, Seized (`docs/design/steel/pairs_d3.md`, "Lightning and Water"). Drawn in the heart of the chart while
Lightning and Water are both carried.

## seized.shape
**What it does.** Opens Seized's two shapes. At a rest fire you choose which one your Seized wears, and from then on it runs instead of the plain one. **Can't Let Go**: no film; your blade keeps the one enemy locked. **Running Water**: no hold; the live water runs outward and seizes what it reaches.

**When it happens.** Every time Seized fires (a Soaked enemy carrying two Static or more) while Lightning and Water are both carried. Before this mark is bought, or with only one of the two carried, the plain Seized runs.

**The numbers.** Mix, 3 Shards. Can't Let Go: held 1 second, and for 3 seconds each of your weapon blows on it holds it 0.3 seconds longer. Running Water: everything within 7 metres is held 0.5 seconds and given a Static. Seized rests 6 seconds on each enemy.

**Works with.** Riptide and Thunderclap to bring the two, Wet Thread, Grounded, The Flashing.

**Example.** One brute: Can't Let Go and keep cutting. A wet pack: Running Water locks them all.

**Not built yet.** Running Water reaches all within 7 metres at once rather than running out as a ring; Can't Let Go has no 3-second cap on the hold yet.

## seized.wetthread
**What it does.** Light blows on a Soaked enemy lay one Static.

**When it happens.** Every light blow that lands on a Soaked enemy, while Lightning and Water are both carried.

**The numbers.** Mix, 3 Shards. One Static.

**Works with.** Riptide, Seized (two Static on the wet seize it), Salt Water.

**Example.** Two light blows on a soaked soldier and it locks.

## seized.grounded
**What it does.** A heavy blow on a seized enemy discharges its Static into the floor: everything standing in live water takes the discharge in full.

**When it happens.** Every heavy blow or finisher that lands on an enemy carrying both Soaked and Static.

**The numbers.** Mix, 3 Shards. Its Static goes off, 7 damage and 5 stagger a stack, to everything within 4 metres.

**Works with.** Seized, Wet Thread, The Sea Finds Land.

**Example.** A soaked pack round a seized one: one heavy blow and the floor discharges.

**Not built yet.** It reaches everything within 4 metres rather than everything standing in live water.

## seized.saltwater
**What it does.** The film of live water stays 2 seconds longer.

**When it happens.** Each time Seized fires.

**The numbers.** Mix, a minor, 1 Shard. The film of live water lasts 5 seconds, not 3.

**Works with.** Wet Thread, which it grows from, Seized.

**Example.** The film keeps charging what wades in.

## seized.seafindsland
**What it does.** Static on a Soaked enemy needs no heavy blow: at full depth it discharges by itself, through the water, to every Soaked enemy within 8 metres.

**When it happens.** Whenever Static fills on a Soaked enemy, while Lightning and Water are both carried.

**The numbers.** Mix, keystone, 5 Shards. 7 damage and 5 stagger a stack within 8 metres, at most once a second.

**Works with.** Wet Thread, The Flashing, Riptide.

**Example.** Fill a wet soldier with Static and the whole wet floor goes off.

**Not built yet.** It reaches every enemy within 8 metres, not only the soaked ones.

## seized.flashing
**What it does.** You slap the floor and a flash runs across the ground to an enemy, then darts on to the next, up to five. Each it reaches is struck, Soaked and charged twice, so it is seized at once.

**When it happens.** From a spell slot, while Lightning and Water are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Lightning.

**The numbers.** Mix, duo spell, opener, 5 Shards. No mana: it waits 10 seconds after a cast, less with weapon play. Up to 10 metres to the first, up to 6 metres between leaps: 12 damage, 20 stagger, two Static and a Soaked each.

**Works with.** Seized and its shapes, Grounded, The Sea Finds Land.

**Example.** Open on a pack: five locked at once.

**Not built yet.** It does not yet seek the nearest soaked enemy or run along the floor, the path does not stay wet, and the next blow on the last one reached does not yet discharge all it touched.
