# Windthrow: detailed explanations

The long explanation the chart shows for every mark on the duo branch Wind and Life share, named for its reaction,
Windthrow (`docs/design/steel/pairs_d3.md`, "Wind and Life"). Drawn in the heart of the chart while Wind and Life are
both carried.

## windthrow.shape
**What it does.** Unlocks two new shapes for Windthrow. At a rest fire, you pick which shape your Windthrow uses. From then on, it uses that shape instead of the plain one. **Blown Seed**: the shove doesn't tear the enemy out of the ground. Instead, its seeds blow away with the wind and root whatever they reach. **Deep Roots**: the enemy is held longer, your blows on it heal you, and tearing it out staggers it harder.

**When it happens.** Every time Windthrow goes off while you carry both Wind and Life. Windthrow goes off when Seeded lands on an Unsteady enemy, or when Unsteady lands on a seeded one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Windthrow.

**The numbers.** Mix, 3 Shards. Blown Seed: the enemy is rooted for 2 seconds. Any shove or throw (your heavy blow, a Gust, a charge, a lift) throws its seeds up to 6 metres past it, away from you, giving two Seeded to everything they reach. This is on purpose: it may root them too. Deep Roots: the enemy is rooted for 4 seconds, and each blow on it heals you 2. Any shove or throw tears it out for 14 damage and 60 stagger, throws it 6 metres, and leaves a torn-up pit. After Windthrow goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Gust and Briar Patch to set it up, Seed on the Wind, The Fairy Wind, Thistledown.

**Example.** Use Blown Seed on the front soldier of a line, and the line behind it gets rooted.

**Not built yet.** Deep Roots heals a flat 2 per blow, not twice the seed's own healing.

## windthrow.seedonwind
**What it does.** A dodge that passes within 2 metres of an enemy leaves it Seeded and Unsteady.

**When it happens.** At the end of each dodge, while you carry both Wind and Life.

**The numbers.** Mix, 3 Shards. One Seeded and one Unsteady on every enemy within 2 metres of the dodge's path, from where it started to where it stopped. Each enemy gets them once a dodge.

**Works with.** Windthrow (both at once root the enemy), Thistledown, Green Wood.

**Example.** Roll past a soldier, and it is rooted to the floor behind you.

## windthrow.tornup
**What it does.** A heavy blow on a rooted enemy counts as a shove: it tears the enemy out, whatever weapon you use.

**When it happens.** Every heavy blow or finisher that lands on an enemy that has both Seeded and Unsteady.

**The numbers.** Mix, 3 Shards. For now: the enemy is thrown 3 metres away from you, at most twice a second.

**Works with.** Windthrow, The Fairy Wind, any weapon.

**Example.** The Hammer's heavy blow throws a rooted soldier across the room.

**Not built yet.** For now, Windthrow already tears enemies out on any heavy blow, so this only adds a throw.

## windthrow.greenwood
**What it does.** Torn-up pits stay longer and add one more stack of Seeded.

**When it happens.** Each time an enemy is torn out.

**The numbers.** Mix, a minor mark, 1 Shard. The torn-up pit stays for 8 seconds instead of 5, and adds two Seeded instead of one. It's the same with Deep Roots.

**Works with.** Seed on the Wind, which it comes after on the chart, Windthrow.

**Example.** The pits left behind keep Seeding the fight.

## windthrow.fairywind
**What it does.** Where an enemy is torn out, a small whirlwind rises and wanders for 4 seconds, making whatever it passes Unsteady and Seeded. So tearing one enemy out roots the next.

**When it happens.** Each time Windthrow goes off, while you carry both Wind and Life.

**The numbers.** Mix, keystone, 5 Shards. For now: a patch 2.4 metres across appears where Windthrow went off and lasts 4 seconds. It adds an Unsteady every half second and a Seeded every second to whatever stands in it. This is on purpose: these may start Windthrow again.

**Works with.** Torn Up, Seed on the Wind, Thistledown.

**Example.** One Windthrow in a pack starts the next.

**Not built yet.** For now, the whirlwind stays still instead of wandering around.

## windthrow.thistledown
**What it does.** You break apart into a cloud of seed fluff and blow in the direction of the stick, passing through enemies. Each enemy you pass gets two stacks of Seeded and Unsteady, so it gets rooted as you go by.

**When it happens.** Cast it from a spell slot while you carry both Wind and Life. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Wind.

**The numbers.** Mix, duo spell, movement, 5 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. You travel 7 metres. Everything within 1.5 metres of your path takes 4 damage, two Unsteady and two Seeded.

**Works with.** Windthrow, Torn Up, The Fairy Wind.

**Example.** Drift through a pack, turn around, and find them rooted.

**Not built yet.** You can still be hit while you drift. No ring of flowers forms where you land yet. A light blow right after doesn't count as a shove yet.
