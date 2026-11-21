# Iron Thorns: detailed explanations

The long explanation the chart shows for every mark on the duo branch Life and Metal share, named for its reaction, Iron
Thorns (`docs/design/steel/pairs_d1_d2.md`, "Life and Metal"). Drawn in the heart of the chart while Life and Metal are
both carried.

## ironthorns.shape
**What it does.** Graft unlocks two new shapes for Iron Thorns. At a rest fire, you pick which shape your Iron Thorns uses. From then on, it uses that shape instead of the plain one. **Pruning Hook**: your blows cut off the thorns instead of throwing them, and each one you cut heals you. **Iron Harvest**: no thorns are thrown. Instead, they all burst out together at the end.

**When it happens.** Every time Iron Thorns goes off while you carry both Life and Metal. Iron Thorns goes off when Sundered lands on a seeded enemy, or when Seeded lands on a notched one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Iron Thorns. The plain one grows three thorns, and each weapon blow throws one at the nearest other enemy.

**The numbers.** Mix, 3 Shards. Pruning Hook: the thorns last 6 seconds. Each of your first three weapon blows on the enemy heals you 4. Iron Harvest: when the enemy dies or the 6 seconds end, a ring of blades bursts out to 3 metres, dealing 8 damage, 8 stagger and a notch to each enemy it reaches. After Iron Thorns goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Briar Patch and Flung Blade to set it up, Sap of Iron, Grafted Edge, The Sown Men.

**Example.** Low on health against one brute: use Pruning Hook, and three blows heal you 12. A pack: use Iron Harvest on the one in the middle.

**Not built yet.** Iron Harvest's five thorns burst out as one ring instead of five blades.

## ironthorns.sap
**What it does.** An enemy that dies with thorns in it gives its iron to the ground, and the ground gives it to you. A quarter of a flask fills up right away, the way plants draw metal up through their roots.

**When it happens.** Each time an enemy dies with both Seeded and Sundered on it, while you carry both Life and Metal.

**The numbers.** Mix, 3 Shards. A quarter of a flask, at most twice a second.

**Works with.** Iron Thorns, Grafted Edge, The Sown Men.

**Example.** A pack of seeded, notched soldiers falls, and a flask comes back full.

**Not built yet.** For now, it works on any enemy that dies seeded and notched, not only one that still has thorns in it.

## ironthorns.ore
**What it does.** Things you grow draw iron out of the ground. Each thing you grew adds a notch of Sundered when it bites.

**When it happens.** Always on once you learn it, while you carry both Life and Metal.

**The numbers.** Mix, 3 Shards. For now: while you stand near something you grew, your blows add a notch, at most twice a second.

**Works with.** Briar Patch, Sapling, Elder Tree, Iron Thorns (a notch on a seeded enemy starts it).

**Example.** Fight next to your sapling, and every soldier you hit there is notched and ready for Iron Thorns.

**Not built yet.** The things you grow don't add the notch themselves yet. For now, your blows near them do.

## ironthorns.grafted
**What it does.** A notch added to a Seeded enemy adds two instead.

**When it happens.** Always on once you learn it, while you carry both Life and Metal.

**The numbers.** Mix, a minor mark, 1 Shard. For now: each blow on an enemy that has both Seeded and Sundered adds one more notch, at most twice a second.

**Works with.** Sap of Iron, which it comes after on the chart, Iron Thorns (more notches, more thorns), Flung Blade.

**Example.** Two blows on a seeded soldier, and its armour has three notches.

**Not built yet.** For now, it works on each blow, not each time a notch is added.

## ironthorns.sownmen
**What it does.** You throw five teeth ahead of you. They sink into the ground, and warriors made of bronze and roots rise from the soil. Each one goes to the nearest enemy and hits it again and again, adding Seeded, and every third hit adds a notch. After 6 seconds, they crumble into rust and leaves.

**When it happens.** Cast it from a spell slot while you carry both Life and Metal. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Life.

**The numbers.** Mix, duo spell, opener, 5 Shards, 45 mana: the larger spell of this pair. Five warriors fight for 6 seconds. Each one hits about every 0.7 seconds for 8 damage, 4 stagger and a Seeded. About one hit in three also adds a notch.

**Works with.** Iron Thorns (a seeded, notched enemy grows thorns), Sap of Iron, Grafted Edge.

**Example.** Sow them into a pack and fight next to them.

**Not built yet.** The teeth aren't thrown in a fan yet, and the warriors don't rise from the floor yet. The notch comes by chance instead of on every third hit. Your heavy blow doesn't yet call every warrior to hit the same enemy.
