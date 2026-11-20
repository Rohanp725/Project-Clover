# Sickle Wind: detailed explanations

The long explanation the chart shows for every mark on the duo branch Wind and Metal share, named for its reaction,
Sickle Wind (`docs/design/steel/pairs_d5.md`, "Wind and Metal"). Drawn in the heart of the chart while Wind and Metal
are both carried.

## sicklewind.shape
**What it does.** Loose Plates: opens Sickle Wind's two shapes. At a rest fire you choose which one your Sickle Wind wears, and from then on it runs instead of the plain one. **Sent Downwind**: the whirl of torn plates leaves its enemy and wanders down the last wind, cutting and notching everything it passes; aim it with Gust and your shoves. **Cut Unfelt**: the shards draw nothing at first, and when the whirl ends every cut opens at once for half again.

**When it happens.** Every time Sickle Wind fires (Unsteady landing on a notched enemy, or a notch on an Unsteady one) while Wind and Metal are both carried. Before this mark is bought, or with only one of the two carried, the plain Sickle Wind runs.

**The numbers.** Mix, 3 Shards. Sent Downwind: three patches of whirling grit laid on your line beyond it, 2 metres apart and 1 metre round, for 3 seconds, cutting 3 damage and 4 stagger every 0.4 seconds; half a second in, each enemy there loses a notch of armour. Cut Unfelt: for 3 seconds nothing; then 32 damage and 28 stagger in 1.5 metres, and 6 more for each of your weapon blows on it in those 3 seconds. Sickle Wind rests 4 seconds on each enemy.

**Works with.** Bellows, Grit, Wind Sickle, Three Weasels.

**Example.** A line of soldiers: Sent Downwind, and shove the first into the rest. One armoured brute: Cut Unfelt, and pile blows on before the debt comes due.

**Not built yet.** Sent Downwind's whirl does not yet wander at 2 metres a second along the way the enemy was last shoved: it is laid at once on your line beyond it. Cut Unfelt's debt is a set sum and your blows, not the cuts the shards made, counted and paid at half again.

## sicklewind.bellows
**What it does.** A dodge that ends within 3 metres of a whirl feeds it: one more shard, and its next cut lays Unsteady.

**When it happens.** Each dodge that ends, while Wind and Metal are both carried.

**The numbers.** Mix, 3 Shards. As built: one Unsteady on everything within 3 metres of where you stop, at most once in 2 seconds.

**Works with.** Sickle Wind (an Unsteady laid on a notched enemy starts it), Grit, Wind Sickle.

**Example.** Dodge in beside a notched brute and the wind takes its plates.

**Not built yet.** It does not yet feed a standing whirl a shard: it unsteadies everything near where your dodge ends, whirl or no whirl.

## sicklewind.grit
**What it does.** Sickle Wind's whirl has one more shard and lasts 1 second longer.

**When it happens.** Each time Sickle Wind fires.

**The numbers.** Mix, a minor, 1 Shard.

**Works with.** Sickle Wind and its shapes, Bellows.

**Example.** Even a fresh enemy throws a full whirl.

**Not built yet.** It does nothing yet: a mark that changes the reaction's own numbers is still to come.

## sicklewind.windsickle
**What it does.** The last blow of a light chain throws a sickle of wind 5 metres along the swing: 6 damage and an Unsteady, and a notch torn from a Sundered enemy becomes a shard that joins the nearest whirl.

**When it happens.** Each light chain's last blow that lands, while Wind and Metal are both carried.

**The numbers.** Mix, keystone, 5 Shards. A cone 5 metres long and 30 degrees wide: 6 damage, 6 stagger and one Unsteady to all in it.

**Works with.** Sickle Wind (a notched enemy it reaches starts its whirl), Bellows, Three Weasels.

**Example.** End each chain facing the pack and the sickles keep it shaken.

**Not built yet.** The notch torn into a shard of the nearest whirl is still to come: as built it is only the sickle's cut.

## sicklewind.weasels
**What it does.** Three presses within 4 seconds, as the three sickle weasels work together: a rush of wind to the enemy you face that throws it, a pass straight through it that notches its armour, and a step back out of reach that binds your wounds.

**When it happens.** From a spell slot, while Wind and Metal are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Wind.

**The numbers.** Mix, duo spell, movement, 5 Shards. No mana: it waits 10 seconds after a cast, less with weapon play. As built, one press: a step of up to 5 metres to the enemy, 6 damage, 20 stagger, two Unsteady, a notch and a throw of 2 metres; your next heavy blow within 2 seconds is critical.

**Works with.** Sickle Wind (it lays both at once), Bellows, Wind Sickle.

**Example.** Rush the archer and set its plates whirling.

**Not built yet.** Only the first press is built. The second (a pass straight through it, untouchable, for 12 damage and a notch) and the third (a step back that binds 2 health for each weapon blow you landed since the first press, eight at most) wait for a slot's second press.
