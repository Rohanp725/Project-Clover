# Rust: detailed explanations

The long explanation the chart shows for every mark on the duo branch Water and Metal share, named for its reaction,
Rust (`docs/design/steel/pairs_d3.md`, "Water and Metal"). Drawn in the heart of the chart while Water and Metal are both
carried.

## rust.shape
**What it does.** Opens Rust's two shapes. At a rest fire you choose which one your Rust wears, and from then on it runs instead of the plain one. Both rust as the plain one does. **Seized Joints**: while it rusts its joints seize, so it walks and turns slower; go round it. **Flaking**: each heavy blow knocks the rust off in a shower of flakes that notch and soak the enemies near.

**When it happens.** Every time Rust fires (Soaked landing on a notched enemy, or a notch on a soaked one) while Water and Metal are both carried. Before this mark is bought, or with only one of the two carried, the plain Rust runs.

**The numbers.** Mix, 3 Shards. Both: while it is Soaked its notches stay and spread, one every 2 seconds, and once it is rusted through a plate falls (weapon blows strike it 16% harder for 6 seconds). Seized Joints: it moves a fifth slower while it rusts. Flaking: a heavy blow spends its notches, 6 damage for each on everything within 3 metres, with a notch and a Soaked. Rust rests 6 seconds on each enemy.

**Works with.** Riptide and Flung Blade to bring the two, Salt Edge, Salt Guard, The Sea Takes It.

**Example.** An armoured brute: Seized Joints and walk round to its back. A pack: Flaking and heavy blows on the rusted one.

**Not built yet.** Notches do not yet go past three, and Seized Joints slows by a fifth rather than a little more for each notch, turning included.

## rust.saltedge
**What it does.** Heavy blows lay one notch and Soak.

**When it happens.** Every heavy blow or finisher that lands, while Water and Metal are both carried.

**The numbers.** Mix, 3 Shards. One notch and one Soaked; of your first element's, none beyond its root's.

**Works with.** Rust (one heavy blow starts it), Brine, The Wreck.

**Example.** Each heavy blow on a fresh enemy sets it rusting.

## rust.saltguard
**What it does.** A perfect guard lays one notch and Soaks the attacker.

**When it happens.** Each perfect guard (press the guard button just before a blow lands).

**The numbers.** Mix, 3 Shards. One notch and one Soaked on the attacker.

**Works with.** Rust, Salt Edge, Counter Cut.

**Example.** Guard a brute perfectly and it starts to rust.

## rust.brine
**What it does.** Soaked lasts longer on a notched enemy.

**When it happens.** Always on once learned, while Water and Metal are both carried.

**The numbers.** Mix, a minor, 1 Shard.

**Works with.** Salt Edge, which it grows from, Rust.

**Example.** A notched soldier stays wet, and so keeps rusting.

**Not built yet.** It does nothing yet: an affliction kept longer while another lies on the same enemy is still to come.

## rust.wreck
**What it does.** A rusted enemy that dies leaves its armour on the floor as a wreck that Soaks and notches whatever stands in it, and your heavy blows struck from inside it are not broken by blows taken.

**When it happens.** Each time an enemy carrying both Soaked and Sundered dies, while Water and Metal are both carried.

**The numbers.** Mix, keystone, 5 Shards. A wreck 1.5 metres across for 8 seconds: a Soaked every second and a notch every 2 seconds to what stands in it.

**Works with.** Rust, Salt Edge, The Sea Takes It.

**Example.** Fight from the wreck of the last soldier.

**Not built yet.** Your heavy blows from inside it are not yet kept from being broken.

## rust.seatakesit
**What it does.** A wave of brine rolls ahead. Everything it crosses is Soaked, and every notched enemy it crosses rusts through to full at once.

**When it happens.** From a spell slot, while Water and Metal are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Water.

**The numbers.** Mix, duo spell, breaker, 5 Shards. No mana: it waits 13 seconds after a cast, less with weapon play. 7 metres ahead, 50 degrees wide: 8 damage, 10 stagger and a Soaked; notched enemies are notched to full.

**Works with.** Rust and its shapes, Salt Edge, The Wreck.

**Example.** Notch a pack with Flung Blade, then roll the sea through it.

**Not built yet.** Their joints do not yet seize (turning 40% slower), and a heavy blow on a seized one does not yet knock a plate off.
