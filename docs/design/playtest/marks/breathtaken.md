# Breath Taken: detailed explanations

The long explanation the chart shows for every mark on the duo branch Dark and Wind share, named for its reaction,
Breath Taken (`docs/design/steel/pairs_d4.md`, "Dark and Wind"). Drawn in the heart of the chart while Dark and Wind are
both carried.

## breathtaken.shape
**What it does.** Storm or Breath unlocks two new shapes for Breath Taken. At a rest fire, you pick which shape your Breath Taken uses. From then on, it uses that shape instead of the plain one. Both shapes still leave the enemy winded, like before. **Black Storm**: the stolen breath doesn't scatter. It keeps going as a streak of black dust and winds the next enemies too. **Second Wind**: the breath blows over to you, and you breathe it in. You get stamina, mana and free dodges.

**When it happens.** Every time Breath Taken goes off while you carry both Dark and Wind. Breath Taken goes off when a Curse lands on an Unsteady enemy, or when Unsteady lands on a cursed one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Breath Taken.

**The numbers.** Mix, 3 Shards. Both shapes, same as the plain one: the enemy is winded for 1.5 seconds (0.6 for an elite). While winded, it can't start an attack, and it takes 30% more stagger. Black Storm: the two nearest other enemies within 6 metres are also winded for 1.5 seconds, and each gets a Curse and an Unsteady. Second Wind: you get 25 stamina and 8 mana, and your next dodge within 3 seconds costs no stamina. After Breath Taken goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Ill Omen to set it up, Short of Breath, The Last Breath, Blown Through, Wind From the Grave.

**Example.** A pack is about to swing. Use Black Storm, and three of them double over. A long fight on your own: use Second Wind, and get a free dodge each time it goes off.

**Not built yet.** Black Storm's streak doesn't yet travel with the wind from one enemy to the next. For now, the two nearest enemies are winded at the same time.

## breathtaken.illomen
**What it does.** A perfect dodge Curses the enemy you dodged and gives it two stacks of Unsteady.

**When it happens.** Each perfect dodge, while you carry both Dark and Wind.

**The numbers.** Mix, 3 Shards. A Curse and two stacks of Unsteady on the enemy you dodged, and on no one else.

**Works with.** Breath Taken (one perfect dodge adds both afflictions), Short of Breath, Blown Through.

**Example.** Slip past the swing at the last moment, and the enemy's breath is taken.

## breathtaken.short
**What it does.** Your heavy blows on a winded enemy hit 50% harder. Each one also takes 1 second off the wait on every spell that is waiting to be cast again.

**When it happens.** While an enemy is winded by Breath Taken (1.5 seconds; 0.6 for an elite), while you carry both Dark and Wind.

**The numbers.** Mix, 3 Shards. Your heavy blows do 1.5 times their damage on the winded enemy. Light blows hit as usual. Each heavy blow takes 1 second off every spell's wait, at most once every 0.3 seconds. It ends the moment the enemy gets its breath back.

**Works with.** Breath Taken and its shapes, Ill Omen.

**Example.** Wind the enemy, then hit it with a heavy blow while it gasps for air.

## breathtaken.lastbreath
**What it does.** When a winded enemy dies, its last breath bursts out as a black gust. Every enemy within 4 metres is pushed 3 metres and Cursed.

**When it happens.** When an enemy dies while it is still winded by Breath Taken, in any shape (1.5 seconds; 0.6 for an elite), while you carry both Dark and Wind.

**The numbers.** Mix, keystone, 5 Shards. A 3-metre push and a Curse on every enemy within 4 metres.

**Works with.** Ill Omen, Short of Breath, Breath Taken.

**Example.** Kill one enemy while it gasps. Its pack is thrown back and Cursed, ready to be winded next.

## breathtaken.blownthrough
**What it does.** You stand inside a black gust for 1 second. The first blow that comes at you hits nothing. The wind carries you through the attacker and sets you down 2 metres behind it, and its breath is taken. If you hit its back within a second, that first blow is a critical hit and gives back half the wait.

**When it happens.** Cast it from a spell slot while you carry both Dark and Wind. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Dark.

**The numbers.** Mix, duo spell, counter, 5 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. When a blow comes, the attacker takes 6 damage, 15 stagger, a Curse and an Unsteady, so Breath Taken goes off. You step to 2 metres behind it. Your first weapon blow on it within 1 second is a critical hit and gives back half of this spell's wait.

**Works with.** Breath Taken and its shapes, Short of Breath, Ill Omen.

**Example.** Wait for the brute's swing, pass through it, and cut its back.

**Not built yet.** The wind doesn't yet carry you through untouchable and set you down facing the enemy; for now, you just step past it. If no blow comes, the gust doesn't yet end in a small push.

## breathtaken.grave
**What it does.** A duo surge. While it is lit, a black wind blows out from you in the direction you face. It is 9 metres long and 100 degrees wide. Enemies in it are pushed away half a metre each second and made Unsteady each second. A Curse put on any enemy in the wind is carried along with the wind to the enemies behind it.

**When it happens.** At a rest fire, pick it for your surge slot in the Hand while you carry both Dark and Wind. Light it with the surge button.

**The numbers.** Mix, duo surge, 5 Shards. 35 mana, lasts 10 seconds. For now: once a second, a cone 9 metres long and 100 degrees wide adds an Unsteady to every enemy in it, and every enemy within 6 metres of you is pushed half a metre away.

**Works with.** Breath Taken, Ill Omen, Blown Through.

**Example.** Light your surge, face the pack, and Curse one of them. The wind does the rest.

**Not built yet.** There is no flowing wind that turns with you yet. The push goes all around you, and a Curse is not yet carried along by the wind.
