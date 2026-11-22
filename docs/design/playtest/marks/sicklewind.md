# Sickle Wind: detailed explanations

The long explanation the chart shows for every mark on the duo branch Wind and Metal share, named for its reaction,
Sickle Wind (`docs/design/steel/pairs_d5.md`, "Wind and Metal"). Drawn in the heart of the chart while Wind and Metal
are both carried.

## sicklewind.shape
**What it does.** Loose Plates unlocks two new shapes for Sickle Wind. At a rest fire, you pick which shape your Sickle Wind uses. From then on, it uses that shape instead of the plain one. **Sent Downwind**: the whirlwind of torn armour plates leaves its enemy and drifts along with the last wind, cutting and notching everything it passes. Aim it with Gust and your shoves. **Cut Unfelt**: at first, the shards don't draw blood. When the whirlwind ends, every cut opens at once for 50% more.

**When it happens.** Every time Sickle Wind goes off while you carry both Wind and Metal. Sickle Wind goes off when Unsteady lands on a notched enemy, or when a notch lands on an Unsteady one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Sickle Wind.

**The numbers.** Mix, 3 Shards. Sent Downwind: three patches of whirling grit land in a line past the enemy, 2 metres apart and 1 metre across, for 3 seconds. They cut for 3 damage and 4 stagger every 0.4 seconds. Half a second in, each enemy there loses a notch of armour. Cut Unfelt: nothing happens for 3 seconds. Then it deals 32 damage and 28 stagger within 1.5 metres, plus 6 more for each weapon blow you landed on the enemy during those 3 seconds. After Sickle Wind goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Bellows, Grit, Wind Sickle, Three Weasels.

**Example.** A line of soldiers: use Sent Downwind, and shove the first one into the rest. One armoured brute: use Cut Unfelt, and pile on blows before the bill comes due.

**Not built yet.** Sent Downwind's whirlwind doesn't drift yet at 2 metres a second in the direction the enemy was last shoved. For now, it lands all at once in a line past the enemy. Cut Unfelt's final hit is a set amount plus your blows, instead of the cuts the shards made, added up and paid back at 50% more.

## sicklewind.bellows
**What it does.** A dodge that ends within 3 metres of a whirlwind feeds it: it gets one more shard, and its next cut adds Unsteady.

**When it happens.** Each dodge that ends, while you carry both Wind and Metal.

**The numbers.** Mix, 3 Shards. When your dodge ends within 3 metres of the edge of a Sickle Wind whirlwind, the whirlwind gets one more shard: each cut from then on deals 3 more damage and 4 more stagger. Its next cut also adds one Unsteady to everything it reaches. If no whirlwind is that close, nothing happens.

**Works with.** Sickle Wind (the whirlwind it feeds), Grit, Wind Sickle.

**Example.** A whirlwind is cutting a notched brute. Dodge in beside it: the whirlwind grows a shard, and its next cut leaves the brute and its friends Unsteady.

## sicklewind.grit
**What it does.** Sickle Wind's whirlwind has one more shard and lasts 1 second longer.

**When it happens.** Each time Sickle Wind goes off.

**The numbers.** Mix, a minor mark, 1 Shard. The whirlwind cuts for 6 damage each time instead of 3, and lasts about 4 seconds instead of 3. With Sent Downwind, it adds one more patch, and the patches last 4 seconds. With Cut Unfelt, you have 4 seconds to add blows to the final hit.

**Works with.** Sickle Wind and its shapes, Bellows.

**Example.** Even a fresh enemy throws off a full whirlwind.

## sicklewind.windsickle
**What it does.** The last blow of a chain of light blows throws a sickle of wind 5 metres in the direction of the swing. It deals 6 damage and an Unsteady, and a notch torn from a Sundered enemy becomes a shard that joins the nearest whirlwind.

**When it happens.** The last blow of each chain of light blows that lands, while you carry both Wind and Metal.

**The numbers.** Mix, keystone, 5 Shards. A cone 5 metres long and 30 degrees wide. It deals 6 damage, 6 stagger and one Unsteady to everything in it. When the chain's last blow lands on a notched (Sundered) enemy and a Sickle Wind whirlwind is within 10 metres of it, one notch is torn off that enemy and joins the nearest whirlwind as a shard: each of its cuts deals 3 more damage and 4 more stagger. With no whirlwind that close, the notch stays.

**Works with.** Sickle Wind (a notched enemy it reaches starts its whirlwind, and later notches feed it), Bellows, Three Weasels.

**Example.** End each chain facing the pack, and the sickles keep them off balance.

## sicklewind.weasels
**What it does.** Three presses within 4 seconds, like the three sickle weasels working together. First, a rush of wind to the enemy you face that throws it. Second, a pass straight through it that notches its armour. Third, a step back out of reach that patches up your wounds.

**When it happens.** Cast it from a spell slot while you carry both Wind and Metal. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Wind. Press the same button again within 4 seconds for the second weasel, and again for the third.

**The numbers.** Mix, duo spell, movement, 5 Shards. It costs no mana. After the first press, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. First press: a step of up to 5 metres to the enemy, dealing 6 damage, 20 stagger and two Unsteady, and throwing it 2 metres. Your next heavy blow within 2 seconds is a critical hit. Second press: you pass straight through the enemy you face to 3 metres past it, and nothing can hit you for 0.4 seconds. It deals 12 damage, 10 stagger and a notch. Third press: you step 4 metres back from the enemy and heal 2 for each weapon blow you landed since the first press, up to 16.

**Works with.** Sickle Wind (it adds both afflictions at once), Bellows, Wind Sickle.

**Example.** Rush the archer and send its armour plates whirling.
