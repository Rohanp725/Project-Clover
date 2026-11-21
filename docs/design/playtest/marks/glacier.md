# Glacier: detailed explanations

The long explanation the chart shows for every mark on Glacier, the duo branch Ice and Water share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Ice and Water") and drawn in the heart of the chart while Ice and Water are both
carried. Its reaction is Supercooled (which does what Flash Freeze did, and more); its Shape mark lets you choose what
Supercooled does.

## glacier.shape
**What it does.** Opens Supercooled's two shapes. At a rest fire you choose which one your Supercooled wears, and from then on it runs instead of the plain one. **Sea Ice**: the next weapon blow freezes it outright as before, and the cold runs out through the floor in a web of frost that chills everything near, so a wet neighbour becomes Supercooled too. **Hard Water**: only a heavy blow freezes it.

**When it happens.** Every time Supercooled fires (Chill landing on a soaked enemy, or Soaked on a chilled one) while Ice and Water are both carried. Before this mark is bought, or with only one of the two carried, the plain Supercooled runs.

**The numbers.** Mix, 3 Shards. Both: the water stays still for 6 seconds; the blow that ends it freezes an ordinary enemy outright (Chill to full), gives an elite 3 Chill and a god 2, and spends the Soaked. Sea Ice: that blow also lays 2 Chill on everything within 4 metres, and on purpose this may start another Supercooled. Hard Water: only a heavy blow ends it. Supercooled rests 4 seconds on each enemy.

**Works with.** Icebreaker (mana and a flask's sip for each shatter), Riptide and Hoarfrost to bring the water and the cold, The Wave Stands Still.

**Example.** Riptide soaks a pack and Hoarfrost touches one: with Sea Ice, one blow freezes it and the frost runs through the wet ones beside it.

**Not built yet.** Sea Ice's frost reaches every enemy within 4 metres, not only the soaked ones, and Hard Water's freeze is not yet held longer nor its shatter made harder.

## glacier.breaker
**What it does.** Shattering an enemy gives you mana and fills part of a flask you have used.

**When it happens.** Each time a frozen enemy is shattered.

**The numbers.** Mix, 3 Shards. 12 mana and a third of a flask a shatter, while you carry fewer flasks than your most. Three shatters leave it a hair short; the fourth fills it.

**Works with.** Supercooled's shapes, Flying Shards, All at Once, The Wave Stands Still (one heavy blow shatters all it holds), The World Holds Still.

**Example.** Shatter four frozen soldiers in one fight and a spent flask is back on your belt.

## glacier.bones
**What it does.** Striking a Soaked enemy makes your perfect guard easier for a few seconds: the moment for it is a quarter longer.

**When it happens.** Each time a weapon blow or a spell of yours strikes a Soaked enemy, at most twice a second.

**The numbers.** Mix, 3 Shards. The perfect-guard moment is 25% longer for 4 seconds: about 0.23 seconds where it is 0.18. As built it lengthens your own moment to guard; it does not slow the enemy's wind-up.

**Works with.** Riptide (it soaks), Cold Read, Weight of Ice.

**Example.** Cut a Soaked duellist twice, then meet its answer with a perfect guard you would have missed.

## glacier.wave
**What it does.** You throw a great wave that rolls ahead while you hold the button, soaking and carrying what it meets. Let go, and it freezes where it stands with everything in it Frozen. One heavy blow beside the wall shatters it and all it holds.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4), and keep holding it. Letting go, or the wave reaching its end, freezes it.

**The numbers.** Mix, 5 Shards, 50 mana. The wave rolls from 1 metre ahead at 6 metres a second, at least 2.5 metres and at most 9, 6 metres wide. What it meets takes 10 damage and is Soaked, and is pushed along. Frozen, it is a wall nothing walks through, and everything within about 3 metres behind it is chilled to full depth. It stands 4 seconds. A heavy blow swung beside it shatters it: 45 damage and 60 stagger to everything in it, as a heavy blow, so each frozen enemy also shatters for 45.

**Works with.** Icebreaker (mana and flask for every enemy shattered), Flying Shards and Clean Break on the Ice tree, All at Once (each shatter three times as hard), Brittle.

**Example.** Roll the wave over a pack, let go as it reaches them, then walk up and break it: every one of them shatters at once.

## glacier.weight
**What it does.** Each blow you catch on your guard lays a plate of ice on you, up to five; a perfect guard lays two. Each plate turns a little of every wound. Your next heavy blow spends them all in one great edge of ice along its line, longer and harder for every plate.

**When it happens.** Plates come with each blocked blow and each perfect guard. They are spent by your next heavy swing, whether or not it lands, or fall away 30 seconds after the last was laid.

**The numbers.** 5 Shards, bought once both elements are carried; no god gives it. Each plate takes 6% off the damage you take, 30% at five. The edge reaches 2.5 metres and 1.2 more a plate (8.5 at five), 1.3 metres to each side: 12 damage, 12 stagger and one stack of Chill a plate, as a heavy blow. Five plates: 60 damage, 60 stagger and five Chill, enough to freeze.

**Works with.** Cold Read, Cold in the Bones, Chill.

**Example.** Guard a brute's three-blow chain, the last perfectly: four plates. Your heavy blow throws an edge 7 metres long through it and the soldier behind.
