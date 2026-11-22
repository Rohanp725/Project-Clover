# Ill Wind: detailed explanations

The long explanation the chart shows for every mark on the duo branch Poison and Wind share, named for its reaction,
Ill Wind (`docs/design/steel/pairs_d3.md`, "Poison and Wind"). Drawn in the heart of the chart while Poison and Wind
are both carried.

## illwind.shape
**What it does.** Unlocks two new shapes for Ill Wind. At a rest fire, you pick which shape your Ill Wind uses. From then on, it uses that shape instead of the plain one. Both shapes still loosen the enemy's poison, like before. **Dust on the Blade**: no trail. Instead, the poisoned grit coats your blade, and your next blows each add a dose and an Unsteady. **Dust Bank**: the trail doesn't settle. It keeps going in the direction the enemy was thrown, as a low bank of dust that makes anything it passes Unsteady.

**When it happens.** Every time Ill Wind goes off while you carry both Poison and Wind. Ill Wind goes off when poison lands on an Unsteady enemy, or when Unsteady lands on a poisoned one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Ill Wind.

**The numbers.** Mix, 3 Shards. Both shapes: the poison stays loose while the enemy has both afflictions, and your heavy blow on it knocks all of it loose. Dust on the Blade: no cloud. The grit coats your blade, and your next three light blows, on any enemy, each add a dose and an Unsteady. Heavy blows don't use it up. Dust Bank: a cloud like the plain one (3 seconds, a dose every second), plus a line of dust 8 metres long and 1.2 metres wide, blowing with the wind. It lasts 4 seconds and makes whatever stands in it Unsteady once a second. After Ill Wind goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Spring Dust to set it up, Gust at the Heel, No Still Air, The Yellow Gale.

**Example.** One brute: use Dust on the Blade, and keep hitting it. A line of soldiers: use Dust Bank, and knock the first one down the line with a heavy blow.

**Not built yet.** Only a heavy blow knocks the poison loose, not every shove and throw. Dust Bank's trail appears all at once instead of drifting 2 metres a second.

## illwind.springdust
**What it does.** Heavy blows poison whatever they shove, and make it Unsteady.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Poison and Wind.

**The numbers.** Mix, 3 Shards. One dose and one Unsteady. For your first element's affliction, it adds nothing beyond what that element's root mark already adds.

**Works with.** Ill Wind (one heavy blow adds both), Fine Dust, No Still Air.

**Example.** Each heavy blow on a fresh enemy loosens its poison.

## illwind.gustheel
**What it does.** A perfect dodge leaves a gust behind you that throws the attacker 3 metres. A poisoned attacker leaves a trail of poison right away.

**When it happens.** Each perfect dodge, while you carry both Poison and Wind.

**The numbers.** Mix, 3 Shards. Everything within 2.5 metres of you is thrown 3 metres, at most once every half second.

**Works with.** Ill Wind, No Still Air, Spring Dust.

**Example.** Dodge a poisoned brute's swing at the last moment, and it is blown away from you.

**Not built yet.** For now, it throws everything near you, not only the attacker, and the throw doesn't knock the dust loose yet.

## illwind.finedust
**What it does.** Ill Wind's trails last 2 seconds longer.

**When it happens.** Each time Ill Wind knocks poison loose, while you carry both Poison and Wind.

**The numbers.** Mix, a minor mark, 1 Shard. The trail of poison lasts 5 seconds instead of 3. With Dust Bank, both trails last 2 seconds longer.

**Works with.** Spring Dust, which it comes after on the chart, Ill Wind and its shapes.

**Example.** The cloud is still there when the next soldier steps in.

## illwind.nostillair
**What it does.** A poisoned, Unsteady enemy can't stand still. Every 2 seconds, the wind shoves it 1 metre away from you, and it leaves a trail of poison each time.

**When it happens.** After each Ill Wind, for as long as that enemy has both afflictions, while you carry both Poison and Wind.

**The numbers.** Mix, keystone, 5 Shards. Every 2 seconds: a 1-metre shove straight away from you. All its poison is knocked loose into a cloud behind it. The cloud lasts 2 seconds and gives a dose once a second to everyone standing in it.

**Works with.** Ill Wind, Spring Dust (which keeps it Unsteady and poisoned), The Yellow Gale.

**Example.** A poisoned, Unsteady brute is blown back step by step, spreading poison dust over its own pack.

## illwind.yellowgale
**What it does.** The wind swirls around you and blows out as a storm of yellow grit, in a cone 9 metres long and 60 degrees wide. Everything in it is thrown 4 metres, gets three stacks of Unsteady and doses of poison, and its poison bites right away.

**When it happens.** Cast it from a spell slot while you carry both Poison and Wind. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Poison.

**The numbers.** Mix, duo spell, detonator, 5 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. It deals 8 damage, 20 stagger, two doses and three Unsteady, throws enemies 4 metres, and makes their poison bite.

**Works with.** Ill Wind (everything it hits gets both afflictions), No Still Air, Spring Dust.

**Example.** Gather a pack in front of you and blow it away in yellow dust.

**Not built yet.** You can't hold the button yet to draw everything in for 1.5 seconds. For now, it doesn't pull in clouds, trails, and half the doses of each poisoned enemy within 6 metres to share them out. Weapon blows before you cast it don't add doses yet.
