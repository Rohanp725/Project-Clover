# Tempest: detailed explanations

The long explanation the chart shows for every mark on Tempest, the duo branch Lightning and Wind share. Reshaped in
Phase 4 (`docs/design/steel/pairs_d1_d2.md`, "Lightning and Wind") and drawn in the heart of the chart while Lightning
and Wind are both carried. Its reaction is Thunder Gust; its Shape mark lets you choose what Thunder Gust does.

## tempest.shape
**What it does.** Opens Thunder Gust's two shapes. At a rest fire you choose which one your Thunder Gust wears, and from then on it runs instead of the plain one. **Thunderhead**: the enemy is thrown up, not away, and a bolt meets it at the top of its rise and discharges it over the heads of the rest. **Bolt from the Blue**: nothing is thrown; its Static goes off where it stands and runs on as an arc through the enemies behind it.

**When it happens.** Every time Thunder Gust fires (Unsteady landing on a charged enemy, or Static on an unsteady one) while Lightning and Wind are both carried. Before this mark is bought, or with only one of the two carried, the plain Thunder Gust runs.

**The numbers.** Mix, 3 Shards. Thunderhead: thrown up for 1 second; half a second in, its Static is discharged, 7 damage and 5 stagger a stack to everything within 4 metres. Bolt from the Blue: its Static goes off in 1.5 metres, then an arc runs through up to three enemies, each up to 5 metres from the last, for 14 damage, 10 stagger and a Static each. Thunder Gust rests 4 seconds on each enemy.

**Works with.** The Sky Finds Them (Thunderhead's throw calls its bolt), Slip Current, Static, Gust.

**Example.** A pack round one charged soldier: Thunderhead lifts it out of their middle and the bolt bursts over all of them. A line of archers: Bolt from the Blue runs the charge down the line.

**Not built yet.** Bolt from the Blue's arc leaps to the nearest enemies rather than along the line downwind of the first, and each takes 14 rather than the whole discharge it would have had.

## tempest.slip
**What it does.** Where a dodge ends, every enemy close to you is left one deeper in Static and in Unsteady.

**When it happens.** At the end of every dodge.

**The numbers.** Mix, 3 Shards. One stack of Static and one of Unsteady on every enemy within 2.2 metres of where you stop.

**Works with.** Thunder Gust's shapes, Blink, Storm Step, Tailwind.

**Example.** Roll into the middle of three soldiers: all three gain a stack of each before you have swung.

## tempest.sky
**What it does.** An enemy you throw into the air is struck by a bolt, which leaps on to two more near it and leaves Static on each.

**When it happens.** Each time one of your marks throws an enemy into the air, such as Thunder Gust's Thunderhead. As built the bolt falls as soon as it is thrown up, not at the top of its rise.

**The numbers.** Mix, 3 Shards. 14 damage and two stacks of Static to the one thrown, then the same to up to two more within 4 metres of each other.

**Works with.** Thunder Gust's Thunderhead, Static, Updraft.

**Example.** Thunderhead throws a soldier up; the bolt strikes it and its two neighbours and leaves each two deep in Static.

## tempest.eye
**What it does.** You become the storm: a turning column of wind and lightning round you that drags enemies in and bites them, and carries the light ones round. While it turns you walk slower, take half damage and cannot be made to flinch. Let go and it throws them all out, and a bolt meets each where it lands.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4), and keep holding it. Letting go ends it.

**The numbers.** Mix, 5 Shards, 50 mana. Up to 2.5 seconds, at least half a second. The column is 3.5 metres in radius and holds up to eight. A bite every 0.4 seconds: 6 damage and one stack of Static (the branch's spells are made of Lightning, whichever element is first). Thrown out up to 5 metres; each bolt deals 12 damage and 20 stagger.

**Works with.** Slip Current or The Sky Finds Them (it grows from either), Thunder Gust's shapes, Static.

**Example.** Swarmed by small enemies: hold it the full time, then let go and the bolts fall on every one thrown.

## tempest.falling
**What it does.** After your finisher lands, you throw the enemy skyward, vanish after it in a line of light, and come down on it as a bolt. Where you land, everything near is shocked, unsteadied and thrown back. Nothing can strike you from the throw to the landing.

**When it happens.** Every finisher that lands (a heavy blow on a staggered enemy).

**The numbers.** A god's gift, never sold; in the playtest it can be bought. You land 0.34 seconds after the throw. Everything within 4.5 metres takes 22 damage, 40 stagger, three stacks of Static and three of Unsteady, and is pushed 3 metres.

**Works with.** Static, Thunder Gust's Thunderhead, Slip Current, Stored Charge.

**Example.** Finish a staggered brute in a crowd and land among its friends with each three deep in both.
