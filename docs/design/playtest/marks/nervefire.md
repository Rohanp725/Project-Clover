# Nerve Fire: detailed explanations

The long explanation the chart shows for every mark on the duo branch Poison and Lightning share, named for its
reaction, Nerve Fire (`docs/design/steel/pairs_d1_d2.md`, "Poison and Lightning"). Drawn in the heart of the chart while
Poison and Lightning are both carried.

## nervefire.shape
**What it does.** Struck Nerve unlocks two new shapes for Nerve Fire. At a rest fire, you pick which shape your Nerve Fire uses. From then on, it uses that shape instead of the plain one. Both shapes keep the shocks. **Ozone**: each shock lets off a puff of sharp blue mist that poisons and charges nearby enemies. **Lockjaw**: the third shock locks the enemy in place.

**When it happens.** Every time Nerve Fire goes off while you carry both Poison and Lightning. Nerve Fire goes off when Static lands on a poisoned enemy, or when poison lands on a charged one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Nerve Fire.

**The numbers.** Mix, 3 Shards. Both shapes: while both afflictions last, each bite of poison uses up a Static and shocks the enemy, for 7 damage, 5 stagger and a 0.3-second flinch (an elite only flinches once every 4 seconds, and each elite keeps its own wait; a god takes 11 stagger and doesn't flinch). Ozone: each shock also adds a dose and a Static to everything within 1 metre. Lockjaw: the third shock holds the enemy for 1.2 seconds. After Nerve Fire goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Miasma and Thunderclap to set it up, Strike on the Beat, Five Thunders, Hidden Crimes.

**Example.** A tightly packed line: Ozone spreads the shocks down it. A brute winding up its big blow: use Lockjaw.

**Not built yet.** For now, the shapes' shocks only show as the burst, not the small shocks across the body that the plain Nerve Fire shows.

## nervefire.beat
**What it does.** A heavy blow that lands in time with the beat of the enemy's poison sets off all its Static and adds just as many doses.

**When it happens.** A heavy blow or finisher that lands on an enemy that has both Static and poison, at most twice a second.

**The numbers.** Mix, 3 Shards. Its Static goes off, dealing 7 damage and 5 stagger per stack within 4 metres, and it gets a dose for each stack.

**Works with.** Nerve Fire, Hidden Crimes, Static.

**Example.** A poisoned soldier has five Static. One heavy blow turns them into five more doses.

**Not built yet.** For now, it works on any heavy blow on a poisoned, charged enemy, not only one within 0.3 seconds of a bite.

## nervefire.fivethunders
**What it does.** Every fifth bite of poison on an enemy with Static calls a bolt down on it, which jumps to two more enemies.

**When it happens.** When poison bites an enemy that has Static.

**The numbers.** Mix, 3 Shards. Exactly every fifth bite. Each enemy keeps its own count. It deals 14 damage and a Static to the enemy and to up to two more within 5 metres.

**Works with.** Nerve Fire, Miasma, Static.

**Example.** A poisoned, charged pack crackles with bolts as the poison works.

## nervefire.hiddencrimes
**What it does.** Poison counts as charge. When Static goes off on a poisoned enemy, it deals 7 more damage for every two doses the enemy has, and uses them up.

**When it happens.** Each time Static is set off on a poisoned enemy (by a heavy blow, Strike on the Beat, or Thunderclap).

**The numbers.** Mix, keystone, 5 Shards. Every dose is used up, dealing 3.5 damage each to everything within 4 metres, on top of the Static burst. At most three times a second.

**Works with.** Strike on the Beat, Static, Miasma, One by One.

**Example.** Eight doses and three Static: the heavy blow bursts for 21, plus 28 more.

## nervefire.seal
**What it does.** You slap a seal of yellow paper on the forehead of the enemy you face, and it is held in place. For 6 seconds, each weapon blow on it burns the seal down by a fifth. On the fifth blow, thunder falls on it.

**When it happens.** Cast it from a spell slot while you carry both Poison and Lightning. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Poison.

**The numbers.** Mix, duo spell, detonator, 5 Shards. It costs no mana. After you cast it, you must wait 13 seconds to cast it again. Fighting with your weapon makes the wait shorter. You step up to 3 metres to the enemy you face and slap the seal on. A normal enemy is held for 1.2 seconds, an elite for half a second, and a god not at all. Your blows only burn the seal down. On the fifth, thunder falls: 30 damage plus 4 for each dose of poison the enemy has, all its Static set off into the same blow (7 damage and 5 stagger a charge), and 50 stagger and a dose to everything within 3.5 metres. Each dose the enemy has also passes one dose to every other enemy within 3 metres. If the 6 seconds run out first, the thunder still falls, at half strength.

**Works with.** Nerve Fire, Hidden Crimes, Strike on the Beat.

**Example.** Seal a captain with six doses and land five blows. Thunder falls on it for 54 damage, and its guards each take six doses.

**Not built yet.** The paper fluttering and charring at each blow and the red brush strokes aren't drawn yet; the seal shows as a sign over the enemy that shrinks with each blow.
