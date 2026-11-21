# The Bolt Runs True: detailed explanations

The long explanation the chart shows for every mark on the duo branch Light and Lightning share, named for its
reaction, The Bolt Runs True (`docs/design/steel/pairs_d4.md`, "Light and Lightning"). Drawn in the heart of the chart
while Light and Lightning are both carried.

## boltrunstrue.shape
**What it does.** Flat or Again unlocks two new shapes for The Bolt Runs True. At a rest fire, you pick which shape your The Bolt Runs True uses. From then on, it uses that shape instead of the plain one. **Laid Flat**: the thread of light lies along the ground to the next Marked enemy, and the bolt runs along that line. **Struck Again**: no big bolt. Instead, the thread stays on the enemy while its Mark lasts, and each critical hit calls a smaller bolt down it.

**When it happens.** Every time The Bolt Runs True goes off while you carry both Light and Lightning. The Bolt Runs True goes off when Static lands on a Marked enemy, or when a Mark lands on a charged one. If you have not bought this mark, or you carry only one of the two elements, you get the plain The Bolt Runs True.

**The numbers.** Mix, 3 Shards. Laid Flat: 0.5 seconds after it goes off, it deals 18 damage, 20 stagger and two Static to the enemy and to the nearest other Marked enemy within 10 metres. Then the enemy's Static goes off, dealing 7 damage per stack within 4 metres. Struck Again: while the enemy's Mark lasts, each weapon blow on it (blows on a Marked enemy are your critical hits) calls down a bolt for 9 damage, 15 stagger and a Static. After The Bolt Runs True goes off on an enemy, it can't go off on that enemy again for 5 seconds.

**Works with.** Charged Light to set it up, Diamond Guard, The Diamond Falls, Diamond Thunder, The Net of Jewels.

**Example.** Two Marked captains far apart: use Laid Flat, and the bolt runs between them. One Marked brute: use Struck Again, and every critical hit calls down the sky.

**Not built yet.** For now, Laid Flat's bolt jumps to the enemy at the far end, instead of hitting everything on the line in between, and it only looks for Marked enemies, not charged ones.

## boltrunstrue.chargedlight
**What it does.** Your light blows add Static to a Marked enemy, and a burst of Static Marks every enemy it reaches.

**When it happens.** Every light blow on a Marked enemy, and every time Static goes off, while you carry both Light and Lightning.

**The numbers.** Mix, 3 Shards. One Static per light blow. A Mark on every enemy within 4 metres of a Static burst, at most once every 0.3 seconds.

**Works with.** The Bolt Runs True (a light blow on a Marked enemy adds both), Diamond Guard, The Net of Jewels.

**Example.** Mark an enemy, cut it with a light blow, and the bolt comes down.

## boltrunstrue.diamondguard
**What it does.** A perfect guard against a Marked enemy calls the bolt down on it right away, no matter what else it has. Once every 5 seconds.

**When it happens.** A perfect guard against a Marked enemy, while you carry both Light and Lightning, at most once every 5 seconds.

**The numbers.** Mix, 3 Shards. It adds a Static, and The Bolt Runs True goes off right away, even if it would normally have to wait.

**Works with.** The Bolt Runs True and its shapes, Charged Light.

**Example.** Block the Marked brute's swing perfectly, and the bolt answers.

## boltrunstrue.diamondfalls
**What it does.** The Bolt Runs True no longer needs Static. The third critical hit on a Marked enemy calls it down, once every 5 seconds.

**When it happens.** Critical hits on a Marked enemy, while you carry both Light and Lightning.

**The numbers.** Mix, keystone, 5 Shards. Every third critical hit on a Marked enemy adds a Static to it, so the bolt comes down. Each enemy keeps its own count. At most once every 5 seconds.

**Works with.** Charged Light, The Bolt Runs True, Diamond Thunder.

**Example.** A Light player with Lightning as a second element calls down the bolt with the blade alone.

## boltrunstrue.diamondthunder
**What it does.** A straight line of light lies on the floor from your hand, 14 metres long, and lightning runs the whole line in one strike. Every enemy on it takes 22 damage and 40 stagger and is Marked, and every Static it meets goes off. Your first blow on each one within 2 seconds is a critical hit.

**When it happens.** Cast it from a spell slot while you carry both Light and Lightning. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Light.

**The numbers.** Mix, duo spell, detonator, 5 Shards. It costs no mana. After you cast it, you must wait 11 seconds to cast it again. Fighting with your weapon makes the wait shorter. It deals 22 damage, 40 stagger and a Mark to each enemy on the line. Their Static goes off, dealing 7 damage per stack within 4 metres. For 2 seconds, your first weapon blow on each one is a critical hit.

**Works with.** The Bolt Runs True (a charged enemy it hits gets both afflictions), Charged Light, The Net of Jewels.

**Example.** Charge up a line of soldiers, then run the strike down them.

**Not built yet.** You can't hold the line on the button yet for up to 1.5 seconds, turning with you, and release it when you let go. A perfect guard while you hold it doesn't yet release it right away for double damage.

## boltrunstrue.net
**What it does.** A duo surge. While it is lit, every Marked enemy within 12 metres is linked to every other one by threads of light. When Static goes off on one, it runs along the threads to all the others for half. Each critical hit on one gives every linked enemy a Static.

**When it happens.** At a rest fire, pick it for your surge slot in the Hand while you carry both Light and Lightning. Light it with the surge button.

**The numbers.** Mix, duo surge, 5 Shards. 45 mana, lasts 10 seconds. For now: when Static goes off on a Marked enemy, it arcs through up to five Marked enemies within 12 metres, for 3.5 damage per stack each, at most once every half second. A critical hit on a Marked enemy adds a Static to every enemy within 12 metres, at most once every 0.3 seconds.

**Works with.** Charged Light, Diamond Thunder, The Bolt Runs True.

**Example.** Mark a pack, light your surge, and one burst of Static runs through all of them.

**Not built yet.** There are no threads yet, so the Static arcs from one enemy to the next, and a critical hit charges every enemy nearby, Marked or not.
