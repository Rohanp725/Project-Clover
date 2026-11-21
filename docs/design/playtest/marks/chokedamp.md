# Chokedamp: detailed explanations

The long explanation the chart shows for every mark on the duo branch Earth and Poison share, named for its reaction,
Chokedamp (`docs/design/steel/pairs_d4.md`, "Earth and Poison"). Drawn in the heart of the chart while Earth and Poison
are both carried.

## chokedamp.shape
**What it does.** Damp or Ore unlocks two new shapes for Chokedamp. At a rest fire, you pick which shape your Chokedamp uses. From then on, it uses that shape instead of the plain one. **Blackdamp**: the poison gas doesn't spread to the other enemies. It sits still and deep under the one enemy, making a spot you can hold. **Red Ore**: there is no mist. The poison hardens in the cracks as veins of red ore, and your next heavy blow on the enemy bursts them.

**When it happens.** Every time Chokedamp goes off while you carry both Earth and Poison. Chokedamp goes off when a dose of poison lands on a shaken enemy, or when Tremor lands on a poisoned one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Chokedamp.

**The numbers.** Mix, 3 Shards. Blackdamp: a pool 3.5 metres wide that lasts 7 seconds. Once a second, it gives a dose of poison to everyone standing in it. Enemies in it when it forms move a fifth slower for the 7 seconds. Red Ore: for 4 seconds, your next heavy blow on the enemy bursts the ore within 3 metres. Each enemy it reaches takes 14 damage plus 3 more for each dose it has, 30 stagger, and three doses. After Chokedamp goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Foul Ground to set it up, The Lamp Goes Out, Seam of Ore, The Clay Rank.

**Example.** A doorway the enemies must come through: use Blackdamp. A brute with lots of doses in a crowd: use Red Ore, and hit it with a heavy blow.

**Not built yet.** Blackdamp's pool poisons anyone standing in it, but it only slows enemies who were in it when it formed.

## chokedamp.foulground
**What it does.** Your heavy blows add a Tremor and a dose of poison at the same time.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Earth and Poison.

**The numbers.** Mix, 3 Shards. One Tremor and one dose.

**Works with.** Chokedamp (one heavy blow on a fresh enemy adds both), The Lamp Goes Out.

**Example.** Hit into the pack with a heavy blow, and poison gas breathes out of the ground under them.

## chokedamp.lamp
**What it does.** Your weapon breaks the balance of enemies standing in the poison gas a quarter faster.

**When it happens.** For 5 seconds after each Chokedamp, on the enemy it went off on and on shaken enemies near it, while you carry both Earth and Poison.

**The numbers.** Mix, 3 Shards. A quarter more stagger on the enemy Chokedamp went off on, and on up to three shaken enemies within 7 metres, for 5 seconds.

**Works with.** Chokedamp, Foul Ground, Seam of Ore.

**Example.** Fill a pack with poison gas, then break their balance one after another.

**Not built yet.** For now, it keeps its own 5-second timer from each Chokedamp, instead of checking who is standing in the gas. It also raises all the stagger those enemies take, even from spells, not only from your weapon.

## chokedamp.seam
**What it does.** Each time poison bites a shaken enemy, the ground shakes under it. Every enemy within 2 metres gets a Tremor.

**When it happens.** Each bite of poison on an enemy that has Tremor, while you carry both Earth and Poison.

**The numbers.** Mix, keystone, 5 Shards. One Tremor on everything within 2 metres of the enemy, at most four times a second. Poisoned enemies it shakes may start Chokedamp.

**Works with.** Foul Ground, Chokedamp, The Lamp Goes Out.

**Example.** One shaken, poisoned soldier shakes the ground for all the enemies around it with every bite.

## chokedamp.clayrank
**What it does.** Three clay soldiers rise in a line in front of you. They are grey men with cracked faces leaking poison gas. They attack the enemy for 6 seconds. Each hit shakes and poisons the enemy it hits.

**When it happens.** Cast it from a spell slot while you carry both Earth and Poison. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Earth.

**The numbers.** Mix, duo spell, opener, 5 Shards. It costs no mana. After you cast it, you must wait 14 seconds to cast it again. Fighting with your weapon makes the wait shorter. Three soldiers fight for 6 seconds. Each one hits about every 1.2 seconds for 6 damage, 10 stagger, a Tremor and a dose, so Chokedamp starts under the enemies they hit.

**Works with.** Chokedamp and its shapes, Seam of Ore, Foul Ground.

**Example.** Send the clay soldiers into the pack, and follow them in.

**Not built yet.** For now, the soldiers just run at the enemies and hit them. They don't yet march 7 metres in a line, push enemies, block the enemies' blows and break after two hits. They don't yet burst into gas and clay when they break (12 damage, a Tremor and three doses within 2.5 metres, or twice as hard if your heavy blow breaks them).
