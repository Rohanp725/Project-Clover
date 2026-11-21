# Blizzard: detailed explanations

The long explanation the chart shows for every mark on Blizzard, the duo branch Wind and Ice share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Wind and Ice") and drawn in the heart of the chart while Wind and Ice are both
carried. Its reaction is Rime; its Shape mark lets you choose what Rime does.

## blizzard.shape
**What it does.** Unlocks two new shapes for Rime. At a rest fire, you pick which shape your Rime uses. From then on, it uses that shape instead of the plain one. **Rime Tree**: the ice crust never breaks, and every blow on the enemy adds Chill, so it ends up frozen inside its crust. **Spindrift**: the wind tears the crust off right away, and the ice blows out of the enemy's far side the moment Rime goes off. You don't need a heavy blow.

**When it happens.** Every time Rime goes off while you carry both Wind and Ice. Rime goes off when Chill lands on an Unsteady enemy, or when Unsteady lands on a chilled one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Rime.

**The numbers.** Mix, 3 Shards. Rime Tree: for 5 seconds, every blow that lands on the enemy, from a weapon or a spell, adds one Chill. Spindrift: two bursts land in a line behind the enemy, from 0.7 to 4.5 metres past it. Each burst deals 5 damage, 2 Chill and 1 Unsteady to everything in it. After Rime goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Windchill, Sliding Ice, Diamond Dust (Rime Tree freezes enemies so you can shatter them), Cold Front, Gust and Hoarfrost.

**Example.** A brute you need to freeze: use Rime Tree, land three quick blows, and it stands frozen. A pack behind one soldier: Spindrift sprays the ice into them the moment the two afflictions meet.

**Not built yet.** Rime Tree's freeze does not yet last a second longer, and it is not yet harder to shatter.

## blizzard.windchill
**What it does.** If the enemy you are facing is both Unsteady and chilled, it gets colder on its own, one stack at a time, until it freezes. For now, only the enemy you face gets colder.

**When it happens.** Every 2 seconds during a fight (when an enemy is within 12 metres), on the enemy you face, if it has both Unsteady and Chill.

**The numbers.** Mix, 3 Shards. It adds one stack of Chill every 2 seconds. At five stacks, the enemy is Frozen for 2.5 seconds (1 second for elites and gods).

**Works with.** Rime's shapes, Cold Front, Sliding Ice, Hoarfrost and Gust, Diamond Dust (gives you a frozen enemy to shatter).

**Example.** Hit a soldier with Gust and then Hoarfrost. Keep facing it while you guard. Eight seconds later it freezes, without you landing a blow.

## blizzard.front
**What it does.** A perfect dodge leaves a blast of ice behind you. It hits the attacker in a cone and adds two stacks of Chill. At the same time, every enemy around you gets two stacks of Unsteady and is thrown back.

**When it happens.** On a perfect dodge, which is when a blow passes through you at the very start of your roll. The cone aims at whoever hit at you. If the blow came from far away, it aims at the enemy you are facing.

**The numbers.** Mix, 3 Shards. The cone is 4 metres long and 90 degrees wide. It deals 10 damage, 20 stagger and two stacks of Chill. Every enemy within 4 metres of you gets two stacks of Unsteady and is pushed 3 metres away.

**Works with.** Windchill (this adds both afflictions at once), Rime's Spindrift shape, Gust, Tailwind.

**Example.** Roll through a brute's swing at the last moment. It now has two stacks of both afflictions, and it is pushed away from you before it can recover.

## blizzard.sliding
**What it does.** A light blow on a chilled enemy throws back every enemy close to you. Half a second later, ice bursts where the enemy you hit was standing. For now, it doesn't yet send a Frozen enemy sliding into a wall to shatter; the push and the burst take its place.

**When it happens.** When a light blow lands on an enemy that has Chill. At most once every 1.5 seconds.

**The numbers.** Mix, 3 Shards. Every enemy within 3.5 metres of you is pushed 5 metres away. Half a second later, the burst hits a circle with a 2.5-metre radius for 20 damage and 20 stagger. It adds no affliction.

**Works with.** Windchill, which it comes after on the chart, Hoarfrost, Avalanche.

**Example.** You are surrounded. Hit a chilled soldier, and the whole ring is thrown 5 metres back. Now you have room to raise your guard.

## blizzard.avalanche
**What it does.** A huge wave of snow shaped like a dragon rushes down a path in front of you. It hits everything it passes and carries it along. Where it stops, its jaws close and bite hard. For now, the enemies it carries are not buried and Frozen where it stops.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** Mix, 5 Shards, 50 mana. The path is 9 metres long and 2 metres wide on each side. The wave takes about 1.3 seconds to reach the end. Each enemy it passes takes 12.5 damage, 15 stagger and four stacks of Unsteady, once each, and is shoved along. The bite at the end hits a circle with a 3-metre radius for 25 damage, 30 stagger, a heavy blow and four stacks of Unsteady.

**Works with.** Windchill or Cold Front (it comes after either on the chart), Rising Cut (the bite leaves enemies with five stacks), Sliding Ice, the charged blade (Wind's shoves).

**Example.** A line of soldiers stands on a bridge. Send it down the bridge, and the whole line is piled up at the end and bitten.

## blizzard.dust
**What it does.** When you shatter an enemy, its ice shards don't fall. They float, spinning around you. Your next dodge throws all of them along your path.

**When it happens.** Each time you shatter a frozen enemy, four shards join the ring around you. Your next dodge throws them.

**The numbers.** 5 Shards. You can buy it once you carry both elements; no god gives it. You get four shards for each shatter, up to twelve. They stay for up to 40 seconds after your last shatter. The dodge throws them along a path 9 metres long and 1.1 metres wide on each side, four at a time. Each group of four deals 24 damage, 16 stagger and one stack of Chill to everything in the path.

**Works with.** Windchill, Cold Front, Rime Tree, a heavy blow on a frozen enemy to shatter it, Ice's charged blade (its blows shatter), Long Stride.

**Example.** Shatter three frozen soldiers, then roll toward the archers. Twelve shards go with you, and each archer is hit three times.
