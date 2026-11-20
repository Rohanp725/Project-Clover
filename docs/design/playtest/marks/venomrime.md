# Venom Rime: detailed explanations

The long explanation the chart shows for every mark on the duo branch Poison and Ice share, named for its reaction,
Venom Rime (`docs/design/steel/pairs_d4.md`, "Poison and Ice"). Drawn in the heart of the chart while Poison and Ice
are both carried.

## venomrime.shape
**What it does.** Floor or Bone: opens Venom Rime's two shapes. At a rest fire you choose which one your Venom Rime wears, and from then on it runs instead of the plain one. Both keep its poison from fading, each bite laying a Chill, as before. **Frost Over Frost**: a rimed enemy that freezes spreads its rime over the floor round it. **Cold Damage**: the cold drives the poison inward; while it is frozen its poison bites harder, and when it is shattered every dose goes off at once.

**When it happens.** Every time Venom Rime fires (a dose landing on a chilled enemy, or Chill on a poisoned one) while Poison and Ice are both carried. Before this mark is bought, or with only one of the two carried, the plain Venom Rime runs.

**The numbers.** Mix, 3 Shards. Both, as plain: while it carries both, its poison does not fade and each bite lays a Chill. Frost Over Frost: at the first bite after it freezes, rime 3 metres round it for 5 seconds lays a Chill and a dose each second on what stands there; at most once in 5 seconds. Cold Damage: each bite while it is frozen deals 2 more a dose; a heavy blow on it while frozen sets every dose off, 6 a dose, on top of the shatter. Venom Rime rests 4 seconds on each enemy.

**Works with.** Bitter Frost to bring the two, Six Levels Down, The Drops Quicken, Rime Over Rime.

**Example.** A crowd round one freezing brute: Frost Over Frost. One deep-dosed captain: Cold Damage, and shatter it.

**Not built yet.** Freezing is not yet a moment the rime can answer, so Frost Over Frost spreads at the first bite after the freeze. Cold Damage's bites are 2 a dose harder rather than twice as hard, and its doses go off on your heavy blow on the frozen enemy rather than on the shatter itself.

## venomrime.bitterfrost
**What it does.** Your heavy blows lay a Chill and a dose together.

**When it happens.** Every heavy blow or finisher that lands, while Poison and Ice are both carried.

**The numbers.** Mix, 3 Shards. One Chill and one dose.

**Works with.** Venom Rime (one heavy blow on a fresh enemy brings both), Six Levels Down.

**Example.** Heavy, and the rime begins to creep.

## venomrime.sixlevels
**What it does.** Each bite on a frozen enemy keeps it frozen 0.3 s longer.

**When it happens.** Each bite of poison on a frozen enemy, while Poison and Ice are both carried.

**The numbers.** Mix, 3 Shards. As built: a fresh Chill laid on it at each bite, at most every 0.3 seconds.

**Works with.** Venom Rime and its shapes, Bitter Frost, The Drops Quicken.

**Example.** A frozen, poisoned soldier stays frozen while the poison works.

**Not built yet.** A freeze cannot yet be held a set time longer: each bite lays a Chill on the frozen enemy instead.

## venomrime.drops
**What it does.** A rimed enemy that dies leaves a drop of venom rime that rises as a small rime creature and strikes the nearest enemy three times, 6 each, a dose and a Chill, then melts.

**When it happens.** Each death of an enemy carrying both poison and Chill, while Poison and Ice are both carried.

**The numbers.** Mix, keystone, 5 Shards. As built: three strikes 0.6 seconds apart, 6 damage, 6 stagger and a dose each; the rime it rises from lays a Chill on everything within 3 metres. At most every half second.

**Works with.** Bitter Frost, Six Levels Down, Venom Rime.

**Example.** Kill one in the rime and its drop goes after its neighbour.

**Not built yet.** The creature's strikes lay only the dose; the Chill comes from the rime round where it fell.

## venomrime.rimeoverrime
**What it does.** A drop of venom falls on the ground where you aim, within 8 metres, and rime grows from it frost over frost, out to 4.5 metres. Each time it grows it gives every enemy it reaches a dose and a Chill, three times over.

**When it happens.** From a spell slot, while Poison and Ice are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Poison.

**The numbers.** Mix, duo spell, opener, 5 Shards. No mana: it waits 10 seconds after a cast, less with weapon play. Three bites 0.6 seconds apart over 4.5 metres, each 3 damage, a dose and a Chill, so Venom Rime starts on all in it.

**Works with.** Venom Rime and its shapes, Bitter Frost, Six Levels Down.

**Example.** Drop it in the middle of a pack and the rime takes them all.

**Not built yet.** The rime does not yet grow in rings outward, 1.5, 3 and 4.5 metres, nor stand 4 seconds after; a heavy blow struck on it does not yet shatter every ring at once (14, a Chill and two doses to each standing on it).
