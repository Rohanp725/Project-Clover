# Fish Poison: detailed explanations

The long explanation the chart shows for every mark on the duo branch Poison and Water share, named for its reaction,
Fish Poison (`docs/design/steel/pairs_d5.md`, "Poison and Water"). Drawn in the heart of the chart while Poison and
Water are both carried.

## fishpoison.shape
**What it does.** Bad Water unlocks two new shapes for Fish Poison. At a rest fire, you pick which shape your Fish Poison uses. From then on, it uses that shape instead of the plain one. **Rock Pool**: a small slick of poisoned water that floats its enemy up with only two doses. It works on one enemy, often. **The Ebb Takes It**: the poisoned water flows away from you as a stream, soaking everything along it, and floats up any enemy with four doses.

**When it happens.** Every time Fish Poison goes off while you carry both Poison and Water. Fish Poison goes off when poison lands on a Soaked enemy, or when water lands on a poisoned one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Fish Poison.

**The numbers.** Mix, 3 Shards. Rock Pool: a slick 1.5 metres across that lasts 5 seconds and soaks anything standing in it. An enemy with two or more doses floats up, stunned. The Ebb Takes It: four patches of slick run 6 metres past the enemy, away from you. Each is 1.5 metres across and lasts 5 seconds. An enemy with four doses floats up. The float works like the plain one: the enemy is lifted and held for 1.5 seconds (0.5 for elites; gods take 30 stagger instead). Your next weapon blow on it is a critical hit and uses up two doses for 6 damage each. Any other enemy in the slick or the stream that reaches the doses floats up the same way, once for each slick. After Fish Poison goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** The Tide Brings It, Wide Slick, Fast Water, Poison the Lagoon.

**Example.** A one-on-one fight with a brute: use Rock Pool, and float it up again and again. A line of soldiers: use The Ebb Takes It, and soak the whole line.

## fishpoison.tide
**What it does.** When Riptide's wave crosses a poisoned slick, it carries the poison with it. Everything the wave carries takes two doses.

**When it happens.** Each time you cast Riptide, while you carry both Poison and Water.

**The numbers.** Mix, 3 Shards. Every fouled slick lying in the wave's path, up to 7 metres ahead of you, rides along with the wave for 0.8 seconds, about 6 metres, and stays where it stops. It still soaks and floats what stands in it on the way. Each enemy the wave hits that is Soaked and poisoned takes two doses, at most once every half second.

**Works with.** Fish Poison (more doses means more floats), Fast Water, Water's Riptide.

**Example.** Wash a poisoned pack toward you, and each one arrives sicker.

## fishpoison.wideslick
**What it does.** Fish Poison's slick is 4 metres across and lasts 2 seconds longer.

**When it happens.** Each time Fish Poison goes off.

**The numbers.** Mix, a minor mark, 1 Shard. The slick is 4 metres across instead of 3, and lasts 7 seconds instead of 5. With Rock Pool, the pool is 2 metres across and lasts 7 seconds. With The Ebb Takes It, the stream lasts 7 seconds.

**Works with.** Fish Poison and its shapes, The Tide Brings It.

**Example.** The slick reaches the soldier standing next to the enemy.

## fishpoison.fastwater
**What it does.** Poison on a Soaked enemy bites twice as often and fades twice as fast.

**When it happens.** Always on once you learn it, while you carry both Poison and Water.

**The numbers.** Mix, keystone, 5 Shards. Poison normally bites every 2 seconds and lasts 6 seconds. On a Soaked enemy, it bites every second and lasts 3 seconds. Each enemy keeps its own timing, so one enemy getting wet doesn't change the others.

**Works with.** Fish Poison, The Tide Brings It, Poison the Lagoon.

**Example.** Soak a poisoned pack, and the bites come fast.

## fishpoison.lagoon
**What it does.** You crush a fish-poison seed on the floor in front of you. A slick spreads out in rings until it is 6 metres across, and stays for 6 seconds, soaking everything in it. Each weapon blow on an enemy in it stirs the water and gives a dose to every other enemy in it. Any enemy that reaches four doses floats up, just like Fish Poison.

**When it happens.** Cast it from a spell slot while you carry both Poison and Water. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Poison.

**The numbers.** Mix, duo spell, opener, 5 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. It reaches up to 7 metres away and lasts 6 seconds. Once a second, it deals 1 damage, a dose and a Soaked to every enemy in it. An enemy with four doses has Fish Poison set off on it right away, even if it would normally have to wait, at most once a second.

**Works with.** Fish Poison (everything in it gets Fish Poison), Fast Water, The Tide Brings It.

**Example.** Drop it under a pack, then work the slick with your blade.

**Not built yet.** Your weapon blows don't stir the water to poison the other enemies in it yet. The forced Fish Poison comes at most once a second for all the enemies together, not once a second for each one.
