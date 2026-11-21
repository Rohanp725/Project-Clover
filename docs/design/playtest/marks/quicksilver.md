# Quicksilver: detailed explanations

The long explanation the chart shows for every mark on the duo branch Metal and Poison share, named for its reaction,
Quicksilver (`docs/design/steel/pairs_d3.md`, "Metal and Poison"). Drawn in the heart of the chart while Metal and Poison
are both carried.

## quicksilver.shape
**What it does.** Unlocks two new shapes for Quicksilver. At a rest fire, you pick which shape your Quicksilver uses. From then on, it uses that shape instead of the plain one. Both shapes keep the beads of mercury: while the enemy has both afflictions, each of your weapon blows makes its poison bite right away. **Heavy Silver**: the mercury pools under the enemy, slowing it and poisoning anything that steps in. **Running Silver**: when the enemy dies, its mercury runs along the floor to the nearest enemy and gives it doses and notches.

**When it happens.** Every time Quicksilver goes off while you carry both Metal and Poison. Quicksilver goes off when poison lands on a notched enemy, or when a notch lands on a poisoned one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Quicksilver.

**The numbers.** Mix, 3 Shards. Both shapes: each weapon blow makes the poison bite right away, at most once every half second on each enemy (each enemy keeps its own wait), and nothing is used up. Heavy Silver: the enemy moves 30% slower while it has both afflictions, and a puddle 1.5 metres across stays for 6 seconds where Quicksilver went off. It gives a dose every second and a notch every 2 seconds to whatever stands in it. Running Silver: when the enemy dies, every dose and every notch it had passes to the nearest enemy within 6 metres. After Quicksilver goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Miasma and Flung Blade to set it up, Envenomed Edge, The Nail Pulled, Ichor.

**Example.** A brute that runs: use Heavy Silver. A pack: Running Silver moves the poison from one enemy to the next.

**Not built yet.** The puddle stays where Quicksilver went off, instead of following the enemy, and the enemy is slowed everywhere, not only while it stands in the puddle.

## quicksilver.edge
**What it does.** Heavy blows add one notch and one dose of poison.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Metal and Poison.

**The numbers.** Mix, 3 Shards. One notch and one dose. For your first element's affliction, it adds nothing beyond what that element's root mark already adds.

**Works with.** Quicksilver (one heavy blow starts it), Cinnabar, The Nail Pulled.

**Example.** Each heavy blow on a fresh enemy sets the mercury running.

## quicksilver.nail
**What it does.** A finisher on an enemy with Quicksilver pulls out the nail: all its poison bites at once, twice.

**When it happens.** Each finisher that lands on an enemy that has both Sundered and poison.

**The numbers.** Mix, 3 Shards. Two bites at once, 0.3 seconds apart.

**Works with.** Quicksilver, Envenomed Edge, Miasma.

**Example.** Eight doses and a finisher: the poison lands twice.

## quicksilver.cinnabar
**What it does.** Notches last as long as the poison does. While an enemy is poisoned, its notches don't fade.

**When it happens.** Always on once you learn it, while you carry both Metal and Poison.

**The numbers.** Mix, a minor mark, 1 Shard. Once the poison is gone, the notches start to fade as usual.

**Works with.** Envenomed Edge, which it comes after on the chart, Quicksilver.

**Example.** A poisoned soldier stays notched.

## quicksilver.drink
**What it does.** Your flasks are mixed with quicksilver. Drinking one throws a ring of silver drops around you that gives three doses of poison and a notch to everything nearby. But each flask heals a fifth less.

**When it happens.** Each time you drink a flask, while you carry both Metal and Poison.

**The numbers.** Mix, keystone, 5 Shards. Three doses and a notch on everything within 4 metres. Flasks heal 20% less.

**Works with.** Quicksilver (everything nearby now has both afflictions), The Nail Pulled, Wellspring.

**Example.** Drink in the middle of a pack, and the pack starts getting eaten by its own poison.

## quicksilver.ichor
**What it does.** You raise your guard. The first blow that lands on it is blocked, and your answer pulls out the nail: the attacker's notches are filled all the way, it gets three doses of poison, and it bleeds for 4 seconds. While it bleeds, its poison bites every half second, and each of your weapon blows adds a dose. At the end, every dose goes off at once.

**When it happens.** Cast it from a spell slot while you carry both Metal and Poison. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Metal.

**The numbers.** Mix, duo spell, counter, 5 Shards. It costs no mana. After you cast it, you must wait 11 seconds to cast it again. Fighting with your weapon makes the wait shorter. The guard stays up for 1.2 seconds. The answer is a cone 3 metres long that deals 10 damage and 20 stagger, fills the notches all the way, and adds three doses. The bleeding lasts 4 seconds. If nothing hits you, the wait is half as long: 5.5 seconds.

**Works with.** Quicksilver, The Nail Pulled, Envenomed Edge.

**Example.** Block a brute's swing, and it bleeds silver.
