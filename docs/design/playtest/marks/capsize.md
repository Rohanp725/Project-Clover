# Capsize: detailed explanations

The long explanation the chart shows for every mark on the duo branch Wind and Water share, named for its reaction,
Capsize (`docs/design/steel/pairs_d1_d2.md`, "Wind and Water"). Drawn in the heart of the chart while Wind and Water are
both carried.

## capsize.shape
**What it does.** Unlocks two new shapes for Capsize. At a rest fire, you pick which shape your Capsize uses. From then on, it uses that shape instead of the plain one. **Waterspout**: instead of falling down, the enemy is lifted up in a column of spray. It stays in the air while your blows keep it up. **Breakers**: the enemy falls down like before. As it falls, it throws a sheet of spray behind it, so a line of wet, Unsteady enemies falls over like dominoes.

**When it happens.** Every time Capsize goes off while you carry both Wind and Water. Capsize goes off when Unsteady lands on a soaked enemy, or when Soaked lands on an Unsteady one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Capsize. The plain one knocks the enemy down for 1.2 seconds. Big enemies are only staggered.

**The numbers.** Mix, 3 Shards. Waterspout: the enemy is lifted for 1.2 seconds. An enemy too heavy to lift staggers back instead. Breakers: the enemy is knocked down for 1.2 seconds (an elite takes 25 stagger instead, a god 15). Two bursts of spray land in a line behind it, from 0.9 to 4 metres past it. They Soak whatever stands there and give it 2 Unsteady. This is on purpose: it can make those enemies capsize too. After Capsize goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Riptide and Gust to set it up, Rolled In, Swamped, Ride the Squall, and Updraft's juggles (with Waterspout).

**Example.** A line of soaked soldiers stands in a hallway. Use Breakers on the first, and they fall over one after another.

## capsize.wake
**What it does.** A perfect dodge leaves a line of spray behind you for 3 seconds. Anything that crosses it is Soaked and made Unsteady.

**When it happens.** Each perfect dodge (roll right as a blow would land).

**The numbers.** Mix, 3 Shards. Four patches, each 0.8 metres across, in a line 3 metres long, for 3 seconds. An enemy standing in them gets Soaked every half second and Unsteady every second.

**Works with.** Capsize (crossing it twice is enough to set it off), Rolled In, Ride the Squall.

**Example.** Dodge a charge. The charging enemy runs through your spray and falls down.

**Not built yet.** The line lands in front of where your dodge ends, not along the path of the dodge.

## capsize.rolledin
**What it does.** An enemy that capsizes is dragged 2 metres toward you as it falls, so it's in reach.

**When it happens.** Each time Capsize goes off on an enemy, plain or with a shape.

**The numbers.** Mix, 3 Shards. Pulled 2 metres toward you.

**Works with.** Swamped (the enemy falls where you can hit it), Capsize's Breakers shape.

**Example.** An archer capsizes at the edge of the fight and slides right to your feet.

## capsize.swamped
**What it does.** Blows on a capsized enemy fill its stagger meter twice as fast.

**When it happens.** Always on once you learn it, while you carry both Wind and Water.

**The numbers.** Mix, keystone, 5 Shards. For now: each blow that lands on an enemy that is both Soaked and Unsteady adds 10 stagger, at most five times a second.

**Works with.** Capsize, Rolled In, the finisher.

**Example.** Knock a brute down, and two blows leave it open for a finisher.

**Not built yet.** For now, it adds a flat 10 stagger to blows on any Soaked, Unsteady enemy, instead of doubling the stagger of blows on an enemy that is down.

## capsize.squall
**What it does.** You ride 7 metres in the direction of the stick on a low, fast sheet of spray. Every enemy near your path takes 6 damage, is Soaked, and gets 2 more stacks of Unsteady. So if it was already Unsteady, it capsizes.

**When it happens.** Cast it from a spell slot while you carry both Wind and Water. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Wind.

**The numbers.** Mix, duo spell, movement, 5 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. You travel 7 metres. Every enemy within 1.5 metres of your path takes 6 damage, 6 stagger, 2 Unsteady and a Soaked.

**Works with.** Capsize, Wake, Open the Gourd.

**Example.** Ride through a soaked pack, and leave them on their backs behind you.

**Not built yet.** You can still be hit during the ride. Casting it right after a perfect dodge doesn't yet skip its wait or make your next blow a critical hit.

## capsize.gourd
**What it does.** A duo surge. While it is lit, a gourd full of wind hangs at your hip. Each dodge lets out one gust along the path of the dodge. The gust Soaks enemies, gives them two stacks of Unsteady, and shoves everything nearby. Each time an enemy capsizes, you get mana.

**When it happens.** At a rest fire, pick it for your surge slot in the Hand while you carry both Wind and Water. Light it with the surge button. Each dodge while it is lit lets out a gust.

**The numbers.** Mix, duo surge, 5 Shards. 40 mana, lasts 10 seconds. Each capsize while it is lit adds 1 second, up to 15 seconds. The gust covers a path 6 metres long and 2 metres wide and gives 2 Unsteady and a Soaked. Every enemy within 3 metres of you is shoved 2 metres. You get 6 mana for each capsize.

**Works with.** Capsize, Ride the Squall, Wake.

**Example.** Light your surge and roll through the fight. Every roll knocks over a line of enemies.
