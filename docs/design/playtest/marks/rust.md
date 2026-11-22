# Rust: detailed explanations

The long explanation the chart shows for every mark on the duo branch Water and Metal share, named for its reaction,
Rust (`docs/design/steel/pairs_d3.md`, "Water and Metal"). Drawn in the heart of the chart while Water and Metal are both
carried.

## rust.shape
**What it does.** Unlocks two new shapes for Rust. At a rest fire, you pick which shape your Rust uses. From then on, it uses that shape instead of the plain one. Both shapes still rust the enemy like the plain one. **Seized Joints**: while the enemy rusts, its joints get stiff, so it walks and turns more slowly. Go around it. **Flaking**: each heavy blow knocks the rust off in a shower of flakes that notch and soak nearby enemies.

**When it happens.** Every time Rust goes off while you carry both Water and Metal. Rust goes off when Soaked lands on a notched enemy, or when a notch lands on a soaked one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Rust.

**The numbers.** Mix, 3 Shards. Both shapes: while the enemy is Soaked, its notches stay and spread, one more every 2 seconds. While it rusts, notches can go past three, up to five. Once it is rusted all the way through (five notches), a plate of armour falls off, and weapon blows hit it 16% harder for 6 seconds. Seized Joints: each time the rust spreads, the enemy walks 6% slower and turns 10% slower for each notch it has (with five notches, 30% slower walking and half-speed turning), so it can't turn to face you in time. Flaking: a heavy blow uses up its notches, dealing 6 damage for each one to everything within 3 metres, plus a notch and a Soaked. After Rust goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Riptide and Flung Blade to set it up, Salt Edge, Salt Guard, The Sea Takes It.

**Example.** An armoured brute: use Seized Joints, and walk around to its back. A pack: use Flaking, and land heavy blows on the rusted one.

## rust.saltedge
**What it does.** Heavy blows add one notch and Soak the enemy.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Water and Metal.

**The numbers.** Mix, 3 Shards. One notch and one Soaked. For your first element's affliction, it adds nothing beyond what that element's root mark already adds.

**Works with.** Rust (one heavy blow starts it), Brine, The Wreck.

**Example.** Each heavy blow on a fresh enemy starts it rusting.

## rust.saltguard
**What it does.** A perfect guard adds one notch and Soaks the attacker.

**When it happens.** Each perfect guard (press the guard button just before a blow lands).

**The numbers.** Mix, 3 Shards. One notch and one Soaked on the attacker.

**Works with.** Rust, Salt Edge, Counter Cut.

**Example.** Guard a brute perfectly, and it starts to rust.

## rust.brine
**What it does.** Soaked lasts twice as long on a notched enemy.

**When it happens.** Always on once you learn it, while you carry both Water and Metal.

**The numbers.** Mix, a minor mark, 1 Shard. While the enemy has a notch, its Soaked fades at half the usual speed.

**Works with.** Salt Edge, which it comes after on the chart, Rust.

**Example.** A notched soldier stays wet, so it keeps rusting.

## rust.wreck
**What it does.** A rusted enemy that dies leaves its armour on the floor as a wreck that Soaks and notches anything standing in it. Your heavy blows from inside it can't be interrupted by blows you take.

**When it happens.** Each time an enemy with both Soaked and Sundered dies, while you carry both Water and Metal.

**The numbers.** Mix, keystone, 5 Shards. A wreck 1.5 metres across that lasts 8 seconds. It adds a Soaked every second and a notch every 2 seconds to whatever stands in it. While you stand in it, nothing interrupts your heavy blows, even blows that hit you while you wind up or swing.

**Works with.** Rust, Salt Edge, The Sea Takes It.

**Example.** Fight from inside the wreck of the last soldier you killed.

## rust.seatakesit
**What it does.** A wave of salt water rolls ahead of you. Everything it crosses is Soaked, and every notched enemy it crosses rusts all the way through right away.

**When it happens.** Cast it from a spell slot while you carry both Water and Metal. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Water.

**The numbers.** Mix, duo spell, breaker, 5 Shards. It costs no mana. After you cast it, you must wait 13 seconds to cast it again. Fighting with your weapon makes the wait shorter. It reaches 7 metres ahead and is 50 degrees wide. It deals 8 damage, 10 stagger and a Soaked. Notched enemies get their notches filled all the way, and their joints go stiff: for 4 seconds they turn 40% slower. A heavy blow on a stiff enemy knocks a plate of armour off, so your weapon blows deal 16% more to it for 6 seconds.

**Works with.** Rust and its shapes, Salt Edge, The Wreck.

**Example.** Notch a pack with Flung Blade, then roll the sea through it.
