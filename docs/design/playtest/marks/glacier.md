# Glacier: detailed explanations

The long explanation the chart shows for every mark on Glacier, the duo branch Ice and Water share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Ice and Water") and drawn in the heart of the chart while Ice and Water are both
carried. Its reaction is Supercooled (which does what Flash Freeze did, and more); its Shape mark lets you choose what
Supercooled does.

## glacier.shape
**What it does.** Unlocks two new shapes for Supercooled. At a rest fire, you pick which shape your Supercooled uses. From then on, it uses that shape instead of the plain one. **Sea Ice**: your next weapon blow freezes the enemy solid, like before. Then the cold spreads through the floor in a web of frost that chills everything nearby, so a wet enemy next to it becomes Supercooled too. **Hard Water**: only a heavy blow freezes it.

**When it happens.** Every time Supercooled goes off while you carry both Ice and Water. Supercooled goes off when Chill lands on a soaked enemy, or when Soaked lands on a chilled one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Supercooled.

**The numbers.** Mix, 3 Shards. Both shapes: the water on the enemy stays still for 6 seconds. The blow that ends it freezes a normal enemy solid (Chill filled all the way), gives an elite 3 Chill and a god 2, and uses up the Soaked. Sea Ice: that blow also puts 2 Chill on everything within 4 metres. This is on purpose: it may start another Supercooled. Hard Water: only a heavy blow ends it. After Supercooled goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Icebreaker (mana and a sip of a flask for each shatter), Riptide and Hoarfrost to add the water and the cold, The Wave Stands Still.

**Example.** Riptide soaks a pack, and Hoarfrost touches one of them. With Sea Ice, one blow freezes it, and the frost spreads through the wet enemies next to it.

**Not built yet.** Sea Ice's frost reaches every enemy within 4 metres, not only the soaked ones. Hard Water's freeze doesn't last longer yet, and it isn't harder to shatter yet.

## glacier.breaker
**What it does.** Shattering an enemy gives you mana and refills part of a flask you have used.

**When it happens.** Each time you shatter a frozen enemy.

**The numbers.** Mix, 3 Shards. 12 mana and a third of a flask for each shatter, as long as you have fewer flasks than your maximum. Three shatters leave it just short; the fourth fills it.

**Works with.** Supercooled's shapes, Flying Shards, All at Once, The Wave Stands Still (one heavy blow shatters everything it holds), The World Holds Still.

**Example.** Shatter four frozen soldiers in one fight, and a used flask is back on your belt.

## glacier.bones
**What it does.** Hitting a Soaked enemy makes perfect guards easier for a few seconds. The timing window for them is a quarter longer.

**When it happens.** Each time a weapon blow or one of your spells hits a Soaked enemy, at most twice a second.

**The numbers.** Mix, 3 Shards. The perfect guard window is 25% longer for 4 seconds: about 0.23 seconds instead of 0.18. For now, it makes your own timing window longer. It doesn't slow down the enemy's wind-up.

**Works with.** Riptide (it soaks), Cold Read, Weight of Ice.

**Example.** Cut a Soaked duellist twice, then meet its next attack with a perfect guard you would have missed.

## glacier.wave
**What it does.** You throw a huge wave that rolls forward while you hold the button, soaking and carrying whatever it meets. Let go, and it freezes in place, with everything in it Frozen. One heavy blow next to the wall of ice shatters it and everything inside.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4), and keep holding it. Letting go, or the wave reaching its end, freezes it.

**The numbers.** Mix, 5 Shards, 50 mana. The wave starts 1 metre ahead of you and rolls at 6 metres a second. It goes at least 2.5 metres and at most 9, and is 6 metres wide. Whatever it meets takes 10 damage, is Soaked, and is pushed along. Once frozen, it is a wall nothing can walk through, and everything within about 3 metres behind it gets its Chill filled all the way. It stands for 4 seconds. A heavy blow swung next to it shatters it, dealing 45 damage and 60 stagger to everything in it. This counts as a heavy blow, so each frozen enemy also shatters for 45.

**Works with.** Icebreaker (mana and flask for every enemy shattered), Flying Shards and Clean Break on the Ice tree, All at Once (each shatter three times as hard), Brittle.

**Example.** Roll the wave over a pack and let go as it reaches them. Then walk up and break it, and every one of them shatters at once.

## glacier.weight
**What it does.** Each blow you block with your guard puts a plate of ice on you, up to five. A perfect guard puts two. Each plate blocks a little of every wound. Your next heavy blow uses them all up in one huge blade of ice along its line, longer and harder for every plate.

**When it happens.** You get plates with each blocked blow and each perfect guard. Your next heavy swing uses them up, whether it lands or not. Otherwise they fall off 30 seconds after you got the last one.

**The numbers.** 5 Shards. You can buy it once you carry both elements; no god gives it. Each plate cuts the damage you take by 6%, so 30% at five. The ice blade reaches 2.5 metres plus 1.2 more for each plate (8.5 at five), and is 1.3 metres wide on each side. For each plate, it deals 12 damage, 12 stagger and one stack of Chill, and it counts as a heavy blow. With five plates, that's 60 damage, 60 stagger and five Chill, enough to freeze.

**Works with.** Cold Read, Cold in the Bones, Chill.

**Example.** Guard a brute's three-blow string, the last one perfectly, for four plates. Your heavy blow throws an ice blade 7 metres long through the brute and the soldier behind it.
