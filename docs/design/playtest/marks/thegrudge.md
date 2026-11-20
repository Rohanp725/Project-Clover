# The Grudge: detailed explanations

The long explanation the chart shows for every mark on the duo branch Dark and Lightning share, named for its reaction,
The Grudge (`docs/design/steel/pairs_d3.md`, "Dark and Lightning"). Drawn in the heart of the chart while Dark and
Lightning are both carried.

## thegrudge.shape
**What it does.** Opens The Grudge's two shapes. At a rest fire you choose which one your Grudge wears, and from then on it runs instead of the plain one. **First Strike**: the cloud does not wait for the enemy's blow; your first heavy blow on it calls the cloud down, for those who would rather not be struck. **Ash Plume**: the cloud grows with each of your blows and comes down as a storm of bolts.

**When it happens.** Every time The Grudge fires (a second Static on a Cursed enemy, or a Curse on one carrying two) while Dark and Lightning are both carried. Before this mark is bought, or with only one of the two carried, the plain Grudge runs.

**The numbers.** Mix, 3 Shards. First Strike: the cloud waits 8 seconds; your first heavy blow on it discharges all its Static at double, 14 for each in 4 metres, and holds it still 0.6 seconds. Ash Plume: the cloud waits up to 6 seconds, and when the enemy next strikes at you (landed, guarded or warded), or the time runs out, one bolt falls round it for each weapon blow you landed on it, 10 damage and a Static each, and all its Static is discharged in 4 metres. The Grudge rests 8 seconds on each enemy.

**Works with.** Grievance to bring the Static, The Exile Returns, Given Bones, The Exile's Grudge.

**Example.** A lone brute: First Strike, and end the cloud with a heavy. A pack round one cursed captain: Ash Plume, and keep hitting it for six seconds.

**Not built yet.** Ash Plume's flashes are not yet capped at five. The cloud does not yet drift to the next enemy when its host dies.

## thegrudge.grievance
**What it does.** A blow held on your guard against a Cursed enemy lays two Static on it.

**When it happens.** Each blow you block on your guard from a Cursed enemy.

**The numbers.** Mix, 3 Shards. Two Static, at most every 0.3 seconds.

**Works with.** The Grudge (two Static on a Cursed enemy is all it needs), Long Memory, Given Bones.

**Example.** Curse a brute, then guard its first swing: the cloud gathers over its head.

## thegrudge.exilereturns
**What it does.** The Grudge's bolt heals you 1 for each Static it discharged and Curses everything within 3 metres.

**When it happens.** Each discharge of Static on a Cursed enemy, while Dark and Lightning are both carried.

**The numbers.** Mix, 3 Shards. 1 health for each Static spent; one Curse on everything within 3 metres. At most every 0.3 seconds.

**Works with.** The Grudge and its shapes, Given Bones, Ash and Thunder.

**Example.** Bring the cloud down on a charged captain: you mend, and the soldiers beside it are cursed for the next cloud.

**Not built yet.** It answers any discharge on a Cursed enemy, not only the Grudge's own bolt.

## thegrudge.longmemory
**What it does.** A Curse on a charged enemy lasts as long as its Static does.

**When it happens.** Always on once learned, while Dark and Lightning are both carried.

**The numbers.** Mix, a minor, 1 Shard.

**Works with.** Grievance, which it grows from, The Grudge.

**Example.** The curse does not run out before the cloud gathers.

**Not built yet.** It does nothing yet: an affliction kept as long as another is still to come.

## thegrudge.givenbones
**What it does.** A discharge on a Cursed enemy costs you 2 health, left grey to be won back by blows, and lands twice as hard.

**When it happens.** Each discharge of Static on a Cursed enemy, while Dark and Lightning are both carried.

**The numbers.** Mix, keystone, 5 Shards. 2 health turned grey (never the last point), and a second burst of 7 for each Static spent, in 4 metres. At most every 0.3 seconds.

**Works with.** The Grudge, The Exile Returns (which heals back what it costs), Ash and Thunder.

**Example.** Five Static on a cursed brute: the bolt lands, then lands again.

## thegrudge.exilesgrudge
**What it does.** A black cloud stands over you for 3 seconds. Whoever strikes you or your guard in that time is answered when the cloud breaks: a bolt that Curses it and charges it three deep.

**When it happens.** From a spell slot, while Dark and Lightning are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Dark.

**The numbers.** Mix, duo spell, counter, 5 Shards. No mana: it waits 12 seconds after a cast, less with weapon play. The cloud stands 3 seconds; the answer: 12 damage and 15 stagger in a cone 4 metres long, a Curse and three Static.

**Works with.** The Grudge (the answer leaves every attacker ready for it), Grievance, Given Bones.

**Example.** Surrounded: cast it, guard what comes, and every attacker is cursed and charged.

**Not built yet.** Each attacker is not yet tied by a thread and paid back twice what it dealt you (60 at most), and a blow on the guard does not yet count double.

## thegrudge.ashthunder
**What it does.** A duo surge. While your surge lasts a plume of ash streams up off your shoulders: every blow you land lays a Static, every third calls a bolt down on a Cursed enemy, and each Cursed enemy killed heals you 6 more.

**When it happens.** While your surge lasts, with Dark and Lightning both carried.

**The numbers.** Mix, duo surge, 5 Shards. As built: each blow lays one Static; one blow in three on a Cursed enemy discharges its Static; each Cursed enemy killed heals you 6.

**Works with.** The Grudge, The Exile Returns, Given Bones.

**Example.** Curse a pack, light your surge, and the plume keeps the clouds gathering.

**Not built yet.** It is not yet a surge of its own (40 mana, 10 seconds) in the Hand's surge slot; the bolt falls on one blow in three by chance on the enemy struck, not on every third blow at the nearest Cursed enemy within 8 metres, and a kill does not yet add a second.
