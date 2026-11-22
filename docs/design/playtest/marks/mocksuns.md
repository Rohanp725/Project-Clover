# Mock Suns: detailed explanations

The long explanation the chart shows for every mark on the duo branch Ice and Light share, named for its reaction,
Mock Suns (`docs/design/steel/pairs_d5.md`, "Ice and Light"). Drawn in the heart of the chart while Ice and Light are
both carried.

## mocksuns.shape
**What it does.** Through the Ice unlocks two new shapes for Mock Suns. At a rest fire, you pick which shape your Mock Suns uses. From then on, it uses that shape instead of the plain one. **Prism**: no mock suns. Instead, the frost on the enemy clears to glass, your next weapon blow is a critical hit, and its light fans out of the enemy's far side as three rays. **Lens of Ice**: the light gathers inside the one enemy. While it is frozen, it keeps its Mark, and when it shatters, the Mark is thrown onto everything nearby.

**When it happens.** Every time Mock Suns goes off while you carry both Ice and Light. Mock Suns goes off when a Mark lands on a chilled enemy, or when Chill lands on a Marked one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Mock Suns.

**The numbers.** Mix, 3 Shards. Prism: for 4 seconds, the enemy's Mark isn't used up. Your next weapon blow on it, a critical hit thanks to the Mark, sends its light out of the enemy's far side as a fan of rays 6 metres long and 30 degrees wide. Every other enemy in it takes 8 damage, a Mark and a Chill. Lens of Ice: while the enemy is Frozen, blows don't use up its Mark. Shattering it while it is Marked and Frozen deals 60% more and puts a Mark on every enemy within 4 metres. After Mock Suns goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Bent Light, Wide Fan, White Night (freeze a Marked enemy with a perfect guard), Sunstone.

**Example.** A crowd behind one soldier: use Prism, and hit the soldier so the rays run through the rest. A pack around a big enemy: use Lens of Ice, then freeze and shatter the big one to Mark them all.

**Not built yet.** Prism's rays are struck at once; each thin ray isn't drawn yet.

## mocksuns.bentlight
**What it does.** A ray that meets ice you made (Hoarfrost's crystals, a wall of ice, a frozen enemy) bends toward the nearest enemy and keeps going 5 metres, dealing 8 damage and a Mark.

**When it happens.** Each spell that hits a frozen enemy, while you carry both Ice and Light.

**The numbers.** Mix, 3 Shards. An arc from the frozen enemy to the nearest other enemy within 5 metres, dealing 8 damage, 5 stagger and a Mark, at most once every half second.

**Works with.** Mock Suns, Sunstone, White Night (freeze the enemy first).

**Example.** Freeze one enemy, then cast through it. The light finds the next one.

**Not built yet.** For now, only a frozen enemy bends the light, and any spell's hit counts, not only rays. Hoarfrost's crystals and walls of ice don't bend it yet.

## mocksuns.widefan
**What it does.** The mock suns hang in the sky 2 seconds longer. With Prism, the light past the enemy reaches farther and has five hits instead of three.

**When it happens.** Each time Mock Suns goes off.

**The numbers.** Mix, a minor mark, 1 Shard. The suns hang for 6 seconds instead of 4. With Prism, the fan of rays reaches 8 metres and is 50 degrees wide, wide enough for five rays.

**Works with.** Mock Suns and its shapes, Bent Light.

**Example.** The suns are still hanging there when you've circled back around.

## mocksuns.whitenight
**What it does.** A perfect guard against a Marked enemy that has three or more Chill freezes it right away.

**When it happens.** Each perfect guard against an enemy like that, while you carry both Ice and Light.

**The numbers.** Mix, keystone, 5 Shards. Its Chill is filled all the way, which freezes it for as long as Chill normally freezes, at most once every half second.

**Works with.** Mock Suns, Lens of Ice (freeze, then shatter), Bent Light, Sunstone.

**Example.** Mark a brute, give it three stacks of Chill, and turn its next swing into ice.

## mocksuns.sunstone
**What it does.** You hold up a block of clear ice crystal for 1.2 seconds. The first blow that lands on it does nothing to you and is split: five rays fan out 8 metres toward the attacker, each dealing 12 damage, 20 stagger, a Mark and two Chill.

**When it happens.** Cast it from a spell slot while you carry both Ice and Light. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Ice.

**The numbers.** Mix, duo spell, counter, 5 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. The caught blow splits into five thin rays fanned 80 degrees wide toward the attacker, each 8 metres long. Each ray deals 12 damage, 20 stagger, two Chill and a Mark to everything it crosses. If nothing hits it, the wait is half as long: 5 seconds.

**Works with.** Mock Suns (everything it hits gets both afflictions), White Night, Bent Light.

**Example.** Raise it as the brute swings, and the pack behind it is Marked and chilled.

**Not built yet.** A perfect guard in its last 0.3 seconds doesn't freeze the attacker solid yet.
