# Tempest: detailed explanations

The long explanation the chart shows for every mark on Tempest, the duo branch Lightning and Wind share. Reshaped in
Phase 4 (`docs/design/steel/pairs_d1_d2.md`, "Lightning and Wind") and drawn in the heart of the chart while Lightning
and Wind are both carried. Its reaction is Thunder Gust; its Shape mark lets you choose what Thunder Gust does.

## tempest.shape
**What it does.** Unlocks two new shapes for Thunder Gust. At a rest fire, you pick which shape your Thunder Gust uses. From then on, it uses that shape instead of the plain one. **Thunderhead**: the enemy is thrown up, not away, and a bolt meets it at the top and sets off its Static over the heads of the rest. **Bolt from the Blue**: nothing is thrown. Instead, its Static goes off where it stands and keeps going as an arc through the enemies behind it.

**When it happens.** Every time Thunder Gust goes off while you carry both Lightning and Wind. Thunder Gust goes off when Unsteady lands on a charged enemy, or when Static lands on an Unsteady one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Thunder Gust.

**The numbers.** Mix, 3 Shards. Thunderhead: the enemy is thrown up for 1 second. Half a second in, its Static goes off, dealing 7 damage and 5 stagger per stack to everything within 4 metres. Bolt from the Blue: its Static goes off within 1.5 metres, then an arc runs through up to three enemies, each up to 5 metres from the last, dealing 14 damage, 10 stagger and a Static to each. After Thunder Gust goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** The Sky Finds Them (Thunderhead's throw calls its bolt), Slip Current, Static, Gust.

**Example.** A pack around one charged soldier: Thunderhead lifts it out of the middle, and the bolt bursts over all of them. A line of archers: Bolt from the Blue sends the charge down the line.

**Not built yet.** For now, Bolt from the Blue's arc jumps to the nearest enemies, instead of along the line behind the first one, and each takes 14 damage instead of the whole burst it would have had.

## tempest.slip
**What it does.** Where a dodge ends, every enemy close to you gets one more stack of Static and one more of Unsteady.

**When it happens.** At the end of every dodge.

**The numbers.** Mix, 3 Shards. One stack of Static and one of Unsteady on every enemy within 2.2 metres of where you stop.

**Works with.** Thunder Gust's shapes, Blink, Storm Step, Tailwind.

**Example.** Roll into the middle of three soldiers. All three get a stack of each before you've even swung.

## tempest.sky
**What it does.** An enemy you throw into the air is hit by a bolt, which jumps on to two more enemies near it and leaves Static on each.

**When it happens.** Each time one of your marks throws an enemy into the air, like Thunder Gust's Thunderhead. For now, the bolt falls as soon as the enemy is thrown up, not when it reaches the top.

**The numbers.** Mix, 3 Shards. 14 damage and two stacks of Static to the enemy you threw, then the same to up to two more, each within 4 metres of the last.

**Works with.** Thunder Gust's Thunderhead shape, Static, Updraft.

**Example.** Thunderhead throws a soldier up. The bolt hits it and the two soldiers next to it, leaving each with two stacks of Static.

## tempest.eye
**What it does.** You become the storm: a spinning column of wind and lightning around you that drags enemies in and bites them, and carries the light ones around. While it spins, you walk more slowly, take half damage, and can't be made to flinch. Let go, and it throws them all out, and a bolt meets each one where it lands.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4), and keep holding it. Letting go ends it.

**The numbers.** Mix, 5 Shards, 50 mana. It lasts up to 2.5 seconds, and at least half a second. The column has a 3.5-metre radius and can hold up to eight enemies. It bites every 0.4 seconds for 6 damage and one stack of Static (this branch's spells are made of Lightning, whichever element is first). Enemies are thrown out up to 5 metres, and each bolt deals 12 damage and 20 stagger.

**Works with.** Slip Current or The Sky Finds Them (it comes after either on the chart), Thunder Gust's shapes, Static.

**Example.** Swarmed by small enemies: hold it for the full time, then let go, and the bolts fall on every enemy thrown.

## tempest.falling
**What it does.** After your finisher lands, you throw the enemy into the sky, vanish after it in a streak of light, and come down on it as a bolt. Where you land, everything nearby is shocked, knocked off balance, and thrown back. Nothing can hit you from the throw until you land.

**When it happens.** Every finisher that lands (a heavy blow on a staggered enemy).

**The numbers.** 5 Shards. You can buy it once you carry both elements; no god gives it. You land 0.34 seconds after the throw. Everything within 4.5 metres takes 22 damage, 40 stagger, three stacks of Static and three of Unsteady, and is pushed 3 metres.

**Works with.** Static, Thunder Gust's Thunderhead shape, Slip Current, Stored Charge.

**Example.** Finish a staggered brute in a crowd, and land among its friends, leaving each with three stacks of both.
