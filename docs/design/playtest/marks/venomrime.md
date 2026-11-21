# Venom Rime: detailed explanations

The long explanation the chart shows for every mark on the duo branch Poison and Ice share, named for its reaction,
Venom Rime (`docs/design/steel/pairs_d4.md`, "Poison and Ice"). Drawn in the heart of the chart while Poison and Ice
are both carried.

## venomrime.shape
**What it does.** Floor or Bone unlocks two new shapes for Venom Rime. At a rest fire, you pick which shape your Venom Rime uses. From then on, it uses that shape instead of the plain one. Both shapes still keep the enemy's poison from fading, with each bite adding a Chill, like before. **Frost Over Frost**: a frosted enemy that freezes spreads its poisoned frost over the floor around it. **Cold Damage**: the cold drives the poison deeper inside. While the enemy is frozen, its poison bites harder, and when it is shattered, every dose goes off at once.

**When it happens.** Every time Venom Rime goes off while you carry both Poison and Ice. Venom Rime goes off when a dose lands on a chilled enemy, or when Chill lands on a poisoned one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Venom Rime.

**The numbers.** Mix, 3 Shards. Both shapes work like the plain one: while the enemy has both afflictions, its poison doesn't fade, and each bite adds a Chill. Frost Over Frost: at the first bite after the enemy freezes, frost covers 3 metres around it for 5 seconds, adding a Chill and a dose each second to whatever stands there. At most once every 5 seconds. Cold Damage: each bite while the enemy is frozen deals 2 more damage per dose. A heavy blow on it while it is frozen sets off every dose, for 6 damage each, on top of the shatter. After Venom Rime goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Bitter Frost to set it up, Six Levels Down, The Drops Quicken, Rime Over Rime.

**Example.** A crowd around one freezing brute: use Frost Over Frost. One captain with lots of doses: use Cold Damage, and shatter it.

**Not built yet.** The game can't tell yet the exact moment an enemy freezes, so Frost Over Frost spreads at the first bite after the freeze. Cold Damage's bites deal 2 more per dose instead of twice as much, and its doses go off on your heavy blow on the frozen enemy, instead of on the shatter itself.

## venomrime.bitterfrost
**What it does.** Your heavy blows add a Chill and a dose of poison at the same time.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Poison and Ice.

**The numbers.** Mix, 3 Shards. One Chill and one dose.

**Works with.** Venom Rime (one heavy blow on a fresh enemy adds both), Six Levels Down.

**Example.** Land a heavy blow, and the poisoned frost starts to creep.

## venomrime.sixlevels
**What it does.** Each poison bite on a frozen enemy keeps it frozen 0.3 seconds longer.

**When it happens.** Each bite of poison on a frozen enemy, while you carry both Poison and Ice.

**The numbers.** Mix, 3 Shards. For now: each bite adds a fresh Chill to the enemy, at most once every 0.3 seconds.

**Works with.** Venom Rime and its shapes, Bitter Frost, The Drops Quicken.

**Example.** A frozen, poisoned soldier stays frozen while the poison works.

**Not built yet.** The game can't yet make a freeze last a set amount of time longer. For now, each bite adds a Chill to the frozen enemy instead.

## venomrime.drops
**What it does.** A frosted enemy that dies leaves behind a drop of poisoned frost. It rises as a small frost creature, hits the nearest enemy three times for 6 damage, a dose and a Chill each, then melts.

**When it happens.** Each time an enemy dies with both poison and Chill on it, while you carry both Poison and Ice.

**The numbers.** Mix, keystone, 5 Shards. For now: three hits 0.6 seconds apart, each dealing 6 damage, 6 stagger and a dose. The frost it rises from adds a Chill to everything within 3 metres. At most once every half second.

**Works with.** Bitter Frost, Six Levels Down, Venom Rime.

**Example.** Kill one enemy in the frost, and its drop goes after the enemy next to it.

**Not built yet.** For now, the creature's hits only add the dose. The Chill comes from the frost around where the enemy fell.

## venomrime.rimeoverrime
**What it does.** A drop of venom falls on the ground where you aim, within 8 metres, and frost grows out from it, layer over layer, out to 4.5 metres. Each time it grows, it gives every enemy it reaches a dose and a Chill, three times in all.

**When it happens.** Cast it from a spell slot while you carry both Poison and Ice. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Poison.

**The numbers.** Mix, duo spell, opener, 5 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. It bites three times, 0.6 seconds apart, over 4.5 metres. Each bite deals 3 damage, a dose and a Chill, so Venom Rime starts on everything in it.

**Works with.** Venom Rime and its shapes, Bitter Frost, Six Levels Down.

**Example.** Drop it in the middle of a pack, and the frost takes them all.

**Not built yet.** The frost doesn't grow outward in rings yet (1.5, 3 and 4.5 metres), and it doesn't stay for 4 seconds afterward. A heavy blow on it doesn't yet shatter every ring at once (for 14 damage, a Chill and two doses to each enemy standing on it).
