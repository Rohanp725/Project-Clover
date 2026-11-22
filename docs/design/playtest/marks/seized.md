# Seized: detailed explanations

The long explanation the chart shows for every mark on the duo branch Lightning and Water share, named for its
reaction, Seized (`docs/design/steel/pairs_d3.md`, "Lightning and Water"). Drawn in the heart of the chart while
Lightning and Water are both carried.

## seized.shape
**What it does.** Unlocks two new shapes for Seized. At a rest fire, you pick which shape your Seized uses. From then on, it uses that shape instead of the plain one. **Can't Let Go**: no film of water. Instead, your blade keeps the one enemy locked in place. **Running Water**: no hold. Instead, the electrified water runs outward and locks up whatever it reaches.

**When it happens.** Every time Seized goes off while you carry both Lightning and Water. Seized goes off when a Soaked enemy has two or more Static. If you have not bought this mark, or you carry only one of the two elements, you get the plain Seized.

**The numbers.** Mix, 3 Shards. Can't Let Go: the enemy is held for 1 second, and for 3 seconds, each of your weapon blows on it holds it 0.3 seconds longer. Running Water: everything within 7 metres is held for 0.5 seconds and gets a Static. After Seized goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Riptide and Thunderclap to set it up, Wet Thread, Grounded, The Flashing.

**Example.** One brute: use Can't Let Go, and keep cutting. A wet pack: Running Water locks them all up.

**Not built yet.** Running Water reaches everything within 7 metres at once, instead of spreading out as a ring. Can't Let Go doesn't have its 3-second limit on the hold yet.

## seized.wetthread
**What it does.** Light blows on a Soaked enemy add one Static.

**When it happens.** Every light blow that lands on a Soaked enemy, while you carry both Lightning and Water.

**The numbers.** Mix, 3 Shards. One Static.

**Works with.** Riptide, Seized (two Static on a wet enemy lock it up), Salt Water.

**Example.** Two light blows on a soaked soldier, and it locks up.

## seized.grounded
**What it does.** A heavy blow on a locked-up enemy sends its Static into the floor. Everything standing in electrified water takes the full burst.

**When it happens.** Every heavy blow or finisher that lands on an enemy that has both Soaked and Static.

**The numbers.** Mix, 3 Shards. Its Static goes off, dealing 7 damage and 5 stagger per stack to everything within 4 metres.

**Works with.** Seized, Wet Thread, The Sea Finds Land.

**Example.** A soaked pack surrounds a locked-up enemy. One heavy blow, and the floor bursts with lightning.

**Not built yet.** For now, it reaches everything within 4 metres, instead of everything standing in electrified water.

## seized.saltwater
**What it does.** The film of electrified water stays 2 seconds longer.

**When it happens.** Each time Seized goes off.

**The numbers.** Mix, a minor mark, 1 Shard. The film of electrified water lasts 5 seconds instead of 3.

**Works with.** Wet Thread, which it comes after on the chart, Seized.

**Example.** The film keeps charging enemies that wade into it.

## seized.seafindsland
**What it does.** Static on a Soaked enemy doesn't need a heavy blow. When it fills up, it goes off on its own, through the water, to every Soaked enemy within 8 metres.

**When it happens.** Whenever Static fills up on a Soaked enemy, while you carry both Lightning and Water.

**The numbers.** Mix, keystone, 5 Shards. 7 damage and 5 stagger per stack to every Soaked enemy within 8 metres, at most once a second. Dry enemies are not hit.

**Works with.** Wet Thread, The Flashing, Riptide.

**Example.** Fill a wet soldier with Static, and the whole wet floor goes off.

## seized.flashing
**What it does.** You slap the floor, and a flash runs across the ground to an enemy, then darts on to the next, up to five. Each one it reaches is hit, Soaked, and charged twice, so it locks up right away.

**When it happens.** Cast it from a spell slot while you carry both Lightning and Water. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Lightning.

**The numbers.** Mix, duo spell, opener, 5 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. It reaches up to 10 metres to the first enemy, and up to 6 metres between jumps. Each enemy takes 12 damage, 20 stagger, two Static and a Soaked.

**Works with.** Seized and its shapes, Grounded, The Sea Finds Land.

**Example.** Start a fight with it on a pack: five enemies lock up at once.

**Not built yet.** It goes first to the nearest soaked enemy in reach (or the one you face), but it doesn't run along the floor yet, and its path doesn't stay wet. Your next blow on the last enemy it reached doesn't set off everything it touched yet.
