# Drowned Weight: detailed explanations

The long explanation the chart shows for every mark on the duo branch Water and Dark share, named for its reaction,
Drowned Weight (`docs/design/steel/pairs_d5.md`, "Water and Dark"). Drawn in the heart of the chart while Water and
Dark are both carried.

## drownedweight.shape
**What it does.** Bound in Debt unlocks two new shapes for Drowned Weight. At a rest fire, you pick which shape your Drowned Weight uses. From then on, it uses that shape instead of the plain one. **Drawn Under**: as the enemy swells, it sinks knee-deep into a pool of black water and is held there, and the other enemies are pulled in toward it. **The Pull of Night**: there is no burst. Instead, while it is swollen, each of your blows on it pulls the wet enemies nearby toward it, the way the sea at night pulls everything back.

**When it happens.** Every time Drowned Weight goes off while you carry both Water and Dark. Drowned Weight goes off when a Curse lands on a Soaked enemy, or when water lands on a Cursed one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Drowned Weight.

**The numbers.** Mix, 3 Shards. Both shapes: the enemy swells for 5 seconds. It moves a fifth slower and takes 15% more damage from weapon blows. Drawn Under: the enemy is held for 1.5 seconds, and everything within 3 metres is pulled 2 metres toward it. It bursts like the plain one when it dies or when a heavy blow lands: 14 damage, 20 stagger, Soaked and Cursed to everything within 4 metres. The Pull of Night: there is no burst. Each weapon blow on the enemy pulls every other Soaked enemy within 5 metres 1 metre toward it, at most once every 0.3 seconds. Enemies that aren't Soaked stay where they are. After Drowned Weight goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Held Under, Deep Swell, The Debt Passes, Under the Black Water.

**Example.** Archers hanging back: use The Pull of Night, and every blow on the swollen enemy drags them in. A tight pack: use Drawn Under, then a heavy blow for the burst.

**Not built yet.** Drawn Under's pool isn't drawn yet, and it pulls the others 2 metres all at once instead of 1.5 metres a second.

## drownedweight.heldunder
**What it does.** A perfect guard against a swollen enemy pushes it down knee-deep. It is held for 1 second, and the attack it was making is lost.

**When it happens.** Each perfect guard against a swollen enemy, while you carry both Water and Dark.

**The numbers.** Mix, 3 Shards. The enemy flinches (whatever it had started is lost) and is held for 1 second (0.4 for elites), at most once a second.

**Works with.** Drowned Weight, The Debt Passes, Under the Black Water.

**Example.** Block the swollen brute's swing perfectly, and it stands sunk in place for your heavy blow.

**Not built yet.** For now, it works on any enemy that is both Soaked and Cursed, whether it is swollen or not.

## drownedweight.deepswell
**What it does.** Drowned Weight keeps the enemy swollen 2 seconds longer, and its burst reaches 5 metres.

**When it happens.** Each time Drowned Weight goes off.

**The numbers.** Mix, a minor mark, 1 Shard. The enemy stays swollen and slow for 7 seconds instead of 5, and its burst reaches 5 metres instead of 4. It's the same in both shapes.

**Works with.** Drowned Weight and its shapes, Held Under.

**Example.** The burst catches the second row of enemies too.

## drownedweight.debtpasses
**What it does.** When a swollen enemy bursts, the nearest enemy the burst reaches swells right away. Killing a swollen enemy heals you 6 more.

**When it happens.** Each time a swollen enemy dies, while you carry both Water and Dark.

**The numbers.** Mix, keystone, 5 Shards. For now: when an enemy that is both Soaked and Cursed dies, every enemy within 4 metres is Cursed, at most once a second, and you heal 6. This is on purpose: it may make those enemies swell too.

**Works with.** Drowned Weight (its burst soaks them all first), Held Under, Under the Black Water.

**Example.** Kill the first swollen soldier, and the debt passes down the line.

**Not built yet.** For now, it works when any Soaked, Cursed enemy dies, and the Curse passes to every enemy within 4 metres, not only the nearest.

## drownedweight.blackwater
**What it does.** Black water opens under you and under the enemy you face. You both sink, and each of you comes up out of the other's water: you where it stood, and it where you stood. It comes up Soaked, Cursed, and held knee-deep for 1 second. The water you came up from stays for 3 seconds as a pool that soaks.

**When it happens.** Cast it from a spell slot while you carry both Water and Dark. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Water.

**The numbers.** Mix, duo spell, movement, 5 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. It reaches up to 9 metres. The enemy takes 8 damage, 15 stagger, Soaked and Cursed (so it swells), and is held for 1 second (0.4 for elites). You and the enemy swap places: it ends up where you stood when you cast, and you end up where it stood. An elite or a god is too heavy to move, so you come up 1.5 metres behind it instead. The pool where you come up is 2.5 metres across and lasts 3 seconds. It Soaks and Curses once a second. Your next weapon blow within 2 seconds hits 50% harder, and your next heavy blow within 2 seconds is a critical hit.

**Works with.** Drowned Weight, Held Under, The Debt Passes.

**Example.** Drop through the water to the archer and come up swinging.
