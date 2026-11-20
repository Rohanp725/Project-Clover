# Capsize: detailed explanations

The long explanation the chart shows for every mark on the duo branch Wind and Water share, named for its reaction,
Capsize (`docs/design/steel/pairs_d1_d2.md`, "Wind and Water"). Drawn in the heart of the chart while Wind and Water are
both carried.

## capsize.shape
**What it does.** Opens Capsize's two shapes. At a rest fire you choose which one your Capsize wears, and from then on it runs instead of the plain one. **Waterspout**: it is lifted in a column of spray instead of going down, in the air where your blows hold it up. **Breakers**: it goes down as before, and as it falls it throws a sheet of spray behind it, so a line of wet, unsteady enemies goes over like dominoes.

**When it happens.** Every time Capsize fires (Unsteady landing on a soaked enemy, or Soaked on an unsteady one) while Wind and Water are both carried. Before this mark is bought, or with only one of the two carried, the plain Capsize runs (down for 1.2 seconds; the great only stagger).

**The numbers.** Mix, 3 Shards. Waterspout: lifted for 1.2 seconds; what is too heavy to lift reels instead. Breakers: down for 1.2 seconds (an elite takes 25 stagger, a god 15), and two bursts of spray beyond it on your line, 0.9 to 4 metres past it, Soak and give 2 Unsteady to what stands there; on purpose, this can capsize them in turn. Capsize rests 4 seconds on each enemy.

**Works with.** Riptide and Gust to bring the two, Rolled In, Swamped, Ride the Squall, Updraft's juggles (Waterspout).

**Example.** A queue of soldiers in a corridor, all soaked: Breakers on the first and they go over one after another.

## capsize.wake
**What it does.** A perfect dodge leaves a line of spray behind it for 3 seconds that Soaks and unsteadies whatever crosses it.

**When it happens.** Each perfect dodge (roll just as a blow would land).

**The numbers.** Mix, 3 Shards. Four patches 0.8 metres across in a line 3 metres long, for 3 seconds: a Soaked every half second and an Unsteady every second to what stands in them.

**Works with.** Capsize (crossing it twice is enough), Rolled In, Ride the Squall.

**Example.** Dodge a charge and the charger runs through your wake and goes down.

**Not built yet.** The line lies ahead of where the dodge ends, not along the dodge's own path.

## capsize.rolledin
**What it does.** An enemy that capsizes is dragged 2 metres toward you as it falls, into reach.

**When it happens.** Each time Capsize fires on an enemy, plain or shaped.

**The numbers.** Mix, 3 Shards. Drawn 2 metres toward you.

**Works with.** Swamped (it falls where you can strike it), Capsize's Breakers.

**Example.** An archer capsizes at the edge of the fight and slides to your feet.

## capsize.swamped
**What it does.** Blows on a capsized enemy fill its stagger meter twice as fast.

**When it happens.** Always on once learned, while Wind and Water are both carried.

**The numbers.** Mix, keystone, 5 Shards. As built: each blow that lands on an enemy both Soaked and Unsteady adds 10 stagger, at most five times a second.

**Works with.** Capsize, Rolled In, the finisher.

**Example.** Put a brute down and two blows open it for a finisher.

**Not built yet.** It answers blows on any soaked, unsteady enemy with a flat 10 stagger rather than doubling the stagger of blows on one that is down.

## capsize.squall
**What it does.** You are carried 7 metres along the stick on a low racing sheet of spray. Every enemy near the line takes 6, is Soaked and made 2 deeper Unsteady, and so capsizes if it was already unsteady.

**When it happens.** From a spell slot, while Wind and Water are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Its first landing charges your blade with Wind.

**The numbers.** Mix, duo spell, movement, 5 Shards. No mana: it waits 9 seconds after a cast, less with weapon play. 7 metres; everything within 1.5 metres of the line takes 6 damage, 6 stagger, 2 Unsteady and a Soaked.

**Works with.** Capsize, Wake, Open the Gourd.

**Example.** Ride through a soaked pack and leave it on its back behind you.

**Not built yet.** You are not yet untouchable on the ride, and casting it out of a perfect dodge does not yet skip its cooldown or make the next blow critical.

## capsize.gourd
**What it does.** A duo surge. While your surge lasts a gourd of winds hangs at your hip: each dodge lets one out, a gust down the dodge's line that Soaks, unsteadies twice and shoves everything near, and each capsize gives you mana.

**When it happens.** Each dodge while your surge lasts, while Wind and Water are both carried.

**The numbers.** Mix, duo surge, 5 Shards. A lane 6 metres long and 2 metres wide: 2 Unsteady and a Soaked. Everything within 3 metres of you is shoved 2 metres. 6 mana for each capsize.

**Works with.** Capsize, Ride the Squall, Wake.

**Example.** Light your surge and roll through the fight: every roll knocks a line of them over.

**Not built yet.** It is not yet a surge of its own in the Hand's surge slot (its own 40 mana and 10 seconds, a second more for each capsize up to 15): for now it rides whichever surge you light while both are carried.
