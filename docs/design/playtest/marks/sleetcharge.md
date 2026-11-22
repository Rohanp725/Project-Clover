# Sleet Charge: detailed explanations

The long explanation the chart shows for every mark on the duo branch Ice and Lightning share, named for its reaction,
Sleet Charge (`docs/design/steel/pairs_d1_d2.md`, "Lightning and Ice"). The first of the new duo branches of Phase 4: Ice
and Lightning are not neighbours on the ring, so the branch is drawn in the heart of the chart, the ring's middle, while
both are carried (in the playtest, whenever you pick Ice beside Lightning or Lightning beside Ice).

## sleetcharge.shape
**What it does.** Unlocks two new shapes for Sleet Charge. At a rest fire, you pick which shape your Sleet Charge uses. From then on, it uses that shape instead of the plain one. Both shapes keep the charging: the frost adds a Static to the enemy every half second for 3 seconds. They change how it ends. **Thundersnow**: the bolt that ends it falls on every chilled enemy nearby. **Glass Spear**: the bolt freezes as it lands, and the enemy is Frozen right away, no matter how much Chill it has.

**When it happens.** Every time Sleet Charge goes off while you carry both Ice and Lightning. Sleet Charge goes off when Static lands on a chilled enemy, or when Chill lands on a charged one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Sleet Charge.

**The numbers.** Mix, 3 Shards. Both shapes: a Static every 0.5 seconds for 3 seconds, then the Static goes off, dealing 7 damage and 5 stagger per stack to everything within 4 metres. Thundersnow: then a bolt falls at once on every chilled enemy within 6 metres, dealing 7 damage, 5 stagger and a Static to each, and each one's own Static goes off where it stands. Glass Spear: then a normal enemy is Frozen for 1.5 seconds, however much Chill it had. An elite gets 3 Chill and a god 2. A heavy blow still sets off the Static early. After Sleet Charge goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Hoarfrost or Glass Storm to add the Chill, Thunderclap or Static to add the charge, Charged Hail (Glass Spear's frozen enemy is ready to shatter), Nothing Resists.

**Example.** A frozen line of soldiers: use Thundersnow and charge one of them. Three seconds later, the bolt runs through every chilled enemy near it. Against one brute: use Glass Spear, and when the charge breaks, it stands frozen for your heavy blow.

**Not built yet.** The spear of clear ice isn't drawn yet.

## sleetcharge.hail
**What it does.** Shattering an enemy that had Static throws its charge into the shards. Everything the shards hit gets a Static.

**When it happens.** Whenever a frozen enemy with Static shatters (from a heavy blow or a finisher on a frozen enemy).

**The numbers.** Mix, 3 Shards. A Static on everything within 3 metres of the shattered enemy, at most five times a second.

**Works with.** Glass Spear (it freezes a charged enemy for you to shatter), Glass Storm, Nothing Resists, and Sleet Charge on whatever the shards chilled.

**Example.** Freeze a charged soldier and shatter it in a pack. The three around it are charged, and the chilled ones start their own Sleet Charge.

**Not built yet.** For now, the charge goes in a ring around the shattered enemy, instead of from each shard that hits.

## sleetcharge.snap
**What it does.** A perfect guard against an enemy with a Sleet Charge running makes the charge go off right away, with full Static.

**When it happens.** A perfect guard (press the guard button just before a blow lands) against an enemy that has Chill.

**The numbers.** Mix, 3 Shards. The enemy's Static is filled all the way (five), and its running Sleet Charge ends at once, so its own bolt falls now: 7 damage and 5 stagger per stack (35 damage and 25 stagger at full) to everything within 4 metres, and a Chill on all it reaches. At most once a second.

**Works with.** Hoarfrost, Glass Storm, Thundersnow, Nothing Resists, and the time a perfect guard takes off your spell waits.

**Example.** A chilled brute swings. Guard it perfectly, and it takes 35 damage, and its pack takes the same.

## sleetcharge.resists
**What it does.** Cold carries lightning without losing any of it. When Static goes off on a chilled enemy, it keeps going at full strength through the chilled enemies nearby.

**When it happens.** Whenever Static is set off on an enemy that has Chill (by a heavy blow, the end of a Sleet Charge, or Cold Snap).

**The numbers.** Mix, keystone, 5 Shards. An arc runs from the enemy through up to four chilled enemies, each within 5 metres of the last one, dealing 7 damage, 5 stagger and a Static to each, at most twice a second.

**Works with.** Thundersnow, Cold Snap, Charged Hail, and Hoarfrost and Glass Storm to keep the pack chilled.

**Example.** Glass Storm has chilled a pack of five. One heavy blow on a charged one, and the arc runs through the other four.

**Not built yet.** For now, it only works when Static goes off, not on every arc that leaves a chilled enemy. Each arc after the first deals its own 7 damage, instead of carrying the full hit along.

## sleetcharge.glassstorm
**What it does.** Six shards of clear ice spin around you, 2 metres away, for 8 seconds, with threads of lightning jumping between them. Each shard that passes through an enemy cuts it and adds a Chill, and every other pass adds a Static too, so the ring sets up Sleet Charge on any enemy that pushes in.

**When it happens.** Cast it from a spell slot while you carry both Ice and Lightning. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Ice, this branch's first element.

**The numbers.** Mix, duo spell, opener, 5 Shards. It costs no mana. After you cast it, you must wait 14 seconds to cast it again. Fighting with your weapon makes the wait shorter. Six shards spin for 8 seconds. Each one that passes through an enemy hits it at most once every half second, for 4 damage, 4 stagger and a Chill. Every other pass on an enemy adds a Static as well. Each enemy keeps its own count.

**Works with.** Sleet Charge and its shapes, Charged Hail, Nothing Resists, Cold Snap, and any heavy blow on a chilled, charged enemy.

**Example.** Cast it as a pack closes in. Every enemy the ring cuts is chilled, the ones it cuts twice are charged, and Sleet Charge starts going off all around you.

**Not built yet.** Your weapon blows don't throw the shards yet (each blow that lands should throw the nearest shard into the enemy you hit, for 8 damage, a Chill and a Static, and a heavy blow should throw all that are left). The shards look like toothed discs until their glassy look is made.
