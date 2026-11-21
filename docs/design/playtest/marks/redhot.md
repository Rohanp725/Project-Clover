# Red Hot: detailed explanations

The long explanation the chart shows for every mark on the duo branch Metal and Fire share, named for its reaction,
Red Hot (`docs/design/steel/pairs_d4.md`, "Metal and Fire"). Drawn in the heart of the chart while Metal and Fire are
both carried.

## redhot.shape
**What it does.** Slag or Shirt unlocks two new shapes for Red Hot. At a rest fire, you pick which shape your Red Hot uses. From then on, it uses that shape instead of the plain one. **Slag**: the enemy's armour melts, dripping hot liquid metal that burns where it falls, so drive the enemy through its friends. **The Burning Shirt**: no sparks. Instead, the hot plates of armour cling to the enemy and burn it, and it can't raise its guard.

**When it happens.** Every time Red Hot goes off while you carry both Metal and Fire. Red Hot goes off when a Burn lands on a notched enemy, or when a notch lands on a burning one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Red Hot.

**The numbers.** Mix, 3 Shards. Slag: for 5 seconds, the enemy drips a pool of melted metal each second. Each pool is 0.8 metres across and burns for 3 seconds, dealing 3 damage and a Burn each second to everything in it except the enemy dripping it. The Burning Shirt: for 5 seconds, the enemy can't guard, and every 1.5 seconds it gets a Burn. After Red Hot goes off on an enemy, it can't go off on that enemy again for 5 seconds.

**Works with.** Heated Edge to set it up, Strike While Hot, Red-Short, Tipped Crucible, Heated Bronze.

**Example.** A brute in a crowd: use Slag, and drive it among them. A shield bearer: use The Burning Shirt, and its guard is gone.

**Not built yet.** Slag drips each second where the enemy stands, instead of once for each metre it moves. The Burning Shirt doesn't yet answer each attack the enemy starts (with two more Burns and a flinch). For now, it burns on a timer, and the enemy can't guard for the whole 5 seconds, not only while it has five Burns. Its Burn doesn't yet bite twice as hard, like the plain Red Hot's does.

## redhot.heatededge
**What it does.** Your heavy blows add a Burn and a notch at the same time.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Metal and Fire.

**The numbers.** Mix, 3 Shards. One Burn and one notch.

**Works with.** Red Hot (one heavy blow on a fresh enemy adds both), Strike While Hot.

**Example.** Land a heavy blow, and the armour you hit glows.

## redhot.strikewhilehot
**What it does.** Each heavy blow on a red-hot enemy gives you 4 mana and takes 0.5 seconds off the wait on every spell that is waiting.

**When it happens.** During the 5 seconds after each Red Hot, on each heavy blow on that enemy, while you carry both Metal and Fire.

**The numbers.** Mix, 3 Shards. 4 mana and 0.5 seconds off every spell's wait, for each heavy blow.

**Works with.** Red Hot, Heated Edge, Tipped Crucible.

**Example.** Hammer the glowing brute, and your spells come back sooner.

**Not built yet.** For now, it keeps its own 5-second timer from each Red Hot, instead of checking whether the enemy is glowing.

## redhot.redshort
**What it does.** A finisher on a red-hot enemy crumbles its armour. Every notch is used up for 15 damage each, and for 10 seconds it takes 30% more damage from your weapon.

**When it happens.** The first heavy blow on an enemy during the 5 seconds after its Red Hot, while you carry both Metal and Fire.

**The numbers.** Mix, keystone, 5 Shards. 15 damage for each notch used up, and 30% more damage from your weapon for 10 seconds. Once for each Red Hot.

**Works with.** Heated Edge, Red Hot, Strike While Hot.

**Example.** Give it lots of notches, make it glow, then crack it open.

**Not built yet.** For now, any heavy blow crumbles it, not only a finisher, and it keeps its own 5-second timer from each Red Hot.

## redhot.crucible
**What it does.** A pot of melted bronze hangs at your shoulder for 8 seconds, with four pours in it. Each pour that lands deals 14 damage, 25 stagger, a Burn and a notch, and the ground under it burns while the bronze is red. A heavy blow tips out all the pours that are left.

**When it happens.** Cast it from a spell slot while you carry both Metal and Fire. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Metal.

**The numbers.** Mix, duo spell, breaker, 5 Shards, 35 mana: the larger spell of this pair. Four pours, each dealing 14 damage, 25 stagger, a notch and a Burn, so Red Hot starts. The ground under each enemy it hits burns for 3 seconds, dealing 3 damage and a Burn twice a second.

**Works with.** Red Hot and its shapes, Heated Edge, Strike While Hot, Heated Bronze.

**Example.** Raise the pot, close in, and land a heavy blow. The bronze pours over the pack.

**Not built yet.** The pours aren't yet tipped out one by one along each heavy swing, as streams 4 metres long that cool from white-gold to black. For now, they fly at the nearest enemy on their own, or all at once on a heavy blow.

## redhot.heatedbronze
**What it does.** A duo surge. While it is lit, you glow red all the way through, like the bronze guardian. Every blow that lands on you or your guard burns and notches the enemy that hit you. Your guard costs 30% less stamina. A perfect guard grabs the attacker, gives it three Burns, and throws it 2 metres.

**When it happens.** At a rest fire, pick it for your surge slot in the Hand while you carry both Metal and Fire. Light it with the surge button.

**The numbers.** Mix, duo surge, 5 Shards. 40 mana, lasts 10 seconds. For now: a Burn and a notch on whoever hits you or your guard; your guard costs 30% less stamina; a perfect guard adds three Burns and throws the attacker 2 metres.

**Works with.** Red Hot, Tipped Crucible, Heated Edge.

**Example.** Light your surge and let the pack break itself on your guard.

**Not built yet.** For now, the perfect guard throws the attacker right away, instead of holding it for 1 second first.
