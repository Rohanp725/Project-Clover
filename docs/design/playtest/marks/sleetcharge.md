# Sleet Charge: detailed explanations

The long explanation the chart shows for every mark on the duo branch Ice and Lightning share, named for its reaction,
Sleet Charge (`docs/design/steel/pairs_d1_d2.md`, "Lightning and Ice"). The first of the new duo branches of Phase 4: Ice
and Lightning are not neighbours on the ring, so the branch is drawn in the heart of the chart, the ring's middle, while
both are carried (in the playtest, whenever you pick Ice beside Lightning or Lightning beside Ice).

## sleetcharge.shape
**What it does.** Opens Sleet Charge's two shapes. At a rest fire you choose which one your Sleet Charge wears, and from then on it runs instead of the plain one. Both keep the charging: the frost charges itself a Static every half second for 3 seconds. They change how it ends. **Thundersnow**: the bolt that ends it falls on every chilled enemy near. **Glass Spear**: the bolt freezes as it lands and the enemy is Frozen at once, whatever its Chill.

**When it happens.** Every time Sleet Charge fires (Static landing on a chilled enemy, or Chill on a charged one) while Ice and Lightning are both carried. Before this mark is bought, or with only one of the two carried, the plain Sleet Charge runs.

**The numbers.** Mix, 3 Shards. Both: a Static every 0.5 seconds for 3 seconds, then the Static goes off, 7 damage and 5 stagger a stack to everything within 4 metres. Thundersnow: then a bolt leaps on to every chilled enemy within 6 metres of the last, six at most, 7 damage, 5 stagger and a Static each. Glass Spear: then Chill to full on an ordinary enemy, which freezes it; an elite takes 3 Chill and a god 2. A heavy blow still sets the Static off early. Sleet Charge rests 4 seconds on each enemy.

**Works with.** Hoarfrost or Glass Storm to lay the Chill, Thunderclap or Static to lay the charge, Charged Hail (a Glass Spear's frozen enemy is ready to shatter), Nothing Resists.

**Example.** A frozen line of soldiers: wear Thundersnow, charge one, and three seconds later the bolt runs through every chilled one near it. Against one brute: wear Glass Spear, and when the charge breaks it stands frozen for your heavy blow.

**Not built yet.** Thundersnow's bolt leaps from one chilled enemy to the next rather than falling on each at once, and each is struck for 7 rather than discharging the Static it carries. Glass Spear freezes by filling the Chill, so it lasts the Chill's own time rather than 1.5 seconds, and the spear of clear ice is not drawn yet.

## sleetcharge.hail
**What it does.** Shattering an enemy that carried Static throws its charge into the shards: everything the shards strike takes a Static.

**When it happens.** Whenever a frozen enemy carrying Static shatters (a heavy blow or a finisher on a frozen enemy).

**The numbers.** Mix, 3 Shards. A Static on everything within 3 metres of the shattered enemy, at most five times a second.

**Works with.** Glass Spear (it freezes a charged enemy for you to shatter), Glass Storm, Nothing Resists, and Sleet Charge on whatever the shards chilled.

**Example.** Freeze a charged soldier and shatter it in a pack: the three round it are charged, and the chilled ones start their own Sleet Charge.

**Not built yet.** The charge is laid in a ring round the shattered enemy, not by each shard that strikes.

## sleetcharge.snap
**What it does.** A perfect guard against a chilled enemy makes its Sleet Charge strike at once, with full Static.

**When it happens.** A perfect guard (press the guard button just before a blow lands) against an enemy carrying Chill.

**The numbers.** Mix, 3 Shards. The enemy is charged to full Static (five) and the charge goes off at once: 7 damage and 5 stagger a stack, 35 and 25 at full, to everything within 4 metres. Once a second at most.

**Works with.** Hoarfrost, Glass Storm, Thundersnow, Nothing Resists, the perfect guard's own cooldown cut.

**Example.** A chilled brute swings: guard it perfectly and it takes 35 and its pack the same.

**Not built yet.** The Sleet Charge already running is not ended by it: the full charge goes off beside it, and a new Sleet Charge may begin from the Static laid.

## sleetcharge.resists
**What it does.** Cold carries lightning without loss: Static discharged from a chilled enemy runs on at full strength through the chilled enemies near.

**When it happens.** Whenever Static is set off on an enemy carrying Chill (a heavy blow, a Sleet Charge's end, Cold Snap).

**The numbers.** Mix, keystone, 5 Shards. An arc runs on from the enemy through up to four chilled enemies, each within 5 metres of the last: 7 damage, 5 stagger and a Static each, at most twice a second.

**Works with.** Thundersnow, Cold Snap, Charged Hail, Hoarfrost and Glass Storm to keep the pack chilled.

**Example.** Glass Storm has chilled a pack of five: one heavy blow on a charged one, and the arc runs through the other four.

**Not built yet.** It answers a discharge, not every arc leaving a chilled enemy, and each arc after the first is its own 7 rather than the full strike carried on.

## sleetcharge.glassstorm
**What it does.** Six shards of clear ice turn round you at 2 metres for 8 seconds, threads of lightning jumping between them. Each shard that passes through an enemy cuts it and lays a Chill, and half the passes a Static, so the ring sets up Sleet Charge on whatever presses in.

**When it happens.** From a spell slot, while Ice and Lightning are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Ice, the branch's first element.

**The numbers.** Mix, duo spell, opener, 5 Shards. No mana: it waits 14 seconds after a cast, less with weapon play. Six shards for 8 seconds; each that passes through an enemy strikes it at most every half second for 4 damage, 4 stagger and a Chill, and half the time (by chance) a Static as well.

**Works with.** Sleet Charge and its shapes, Charged Hail, Nothing Resists, Cold Snap, and any heavy blow on a chilled, charged enemy.

**Example.** Cast it as a pack closes: everyone the ring cuts is chilled, half of them charged, and Sleet Charge starts going off round you.

**Not built yet.** The Static comes by chance on half the passes rather than on every other one. Your weapon blows do not fling the shards yet (each landed blow should fling the nearest into the enemy struck, 8 with a Chill and a Static, and a heavy blow all that are left), and the shards are drawn as the toothed discs until their glassy look is made.
