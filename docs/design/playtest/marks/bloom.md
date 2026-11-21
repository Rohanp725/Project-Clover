# Bloom: detailed explanations

The long explanation the chart shows for every mark on Bloom, the duo branch Water and Life share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Water and Life") and drawn in the heart of the chart while Water and Life are both
carried. Its reaction is Quickening (which does what Flowering did, with no wait); its Shape mark lets you choose what
Quickening does.

## bloom.shape
**What it does.** Unlocks two new shapes for Quickening. At a rest fire, you pick which shape your Quickening uses. From then on, it uses that shape instead of the plain one. **Floating Flower**: the flower drops to the ground under the enemy as a lily. The lily heals you and soaks enemies standing on it. **Rootlets**: as the flower opens, small roots spread through the water and fill every nearby enemy with Seeded. A heavy blow still bursts the flower to heal you.

**When it happens.** Every time Quickening goes off while you carry both Water and Life. Quickening goes off when Seeded lands on a soaked enemy, or when Soaked lands on a seeded one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Quickening.

**The numbers.** Mix, 3 Shards. Both shapes fill the enemy's Seeded all the way up. Floating Flower: you heal 8 as it opens. The lily is 2 metres across and lasts 5 seconds. It soaks anything standing on it once a second. Rootlets: every enemy within 5 metres gets Seeded all the way up. A heavy blow on the flowering enemy within 4 seconds heals you 10. After Quickening goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Riptide and Briar Patch to add the water and the seed, Drowned Roots, Strangling Vine, and the burst of thorns Seeded makes when a seeded enemy dies.

**Example.** A wet pack has one seeded soldier. Rootlets fills them all with Seeded, and each one that dies bursts into thorns.

**Not built yet.** The lily heals you once as it opens, instead of 3 every second while you stand on it. The roots reach every enemy within 5 metres, not only the soaked ones.

## bloom.roots
**What it does.** A perfect guard against an enemy that is both Soaked and Seeded wraps it in roots. It is held in place for a moment.

**When it happens.** On a perfect guard (press the guard button just before a blow lands) against an enemy that has both Soaked and Seeded.

**The numbers.** Mix, 3 Shards. The enemy is held still for 1.2 seconds.

**Works with.** Riptide and Briar Patch (each adds one of the afflictions), Quickening's shapes, Great Lotus, and every Water and Life mark that works on a perfect guard.

**Example.** Soak and seed a duellist. Then guard its next thrust perfectly. It stands stuck in roots while you land two cuts.

## bloom.dew
**What it does.** Drinking a flask gives you a ward that blocks one blow. Also, your blows sometimes throw a short blade of water toward the enemy you hit, soaking whatever it passes. For now, the dew doesn't yet gather in your health bar.

**When it happens.** You get a ward each time you drink a flask. The blade of water comes when a weapon blow lands, at most once every 0.8 seconds.

**The numbers.** Mix, 3 Shards. One ward for each flask; you can hold up to three. The water blade flies 3.5 metres at 14 metres a second and is 0.8 metres wide. It deals 4 damage, no stagger, and adds Soaked.

**Works with.** Wellspring, Deep Well, Drowned Roots.

**Example.** Drink right before the brute's red overhead swing. The ward takes the hit, and your cuts back soak the brute.

## bloom.vine
**What it does.** A huge vine bursts out of the ground under the enemy nearest to you. It hits hard and throws that enemy, and anything next to it, into the air for a moment. If the enemy it lifts is soaked and seeded, the vine drinks up the water (the Soaked is used up) and the seed blooms right away. That sets off Quickening, even if Quickening would normally have to wait. For now, the vine comes up under the nearest enemy, not the nearest soaked one. The double heal from a heavy blow on a lifted enemy is not built yet.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). It marks the spot under the enemy and follows the enemy until the vine comes up.

**The numbers.** Mix, 5 Shards, 45 mana. It targets the nearest enemy within 5 metres. If there is none, it lands 5 metres ahead of you. It comes up 0.5 seconds after you cast it and hits a circle with a 3-metre radius. It deals 20 damage and 50 stagger, counts as a heavy blow, and adds Soaked. Anything it hits is thrown into the air for 0.8 seconds. If it lands, it charges your blade with Water.

**Works with.** Quickening (a soaked and seeded enemy it lifts blooms right away, even if Quickening would normally have to wait), Drowned Roots or Morning Dew (it comes after either on the chart), Riptide.

**Example.** A brute and two soldiers close in. The vine throws all three into the air, and you cut the brute as it falls.

## bloom.great
**What it does.** If you drink a flask while your surge is lit, a huge lotus opens under you. Inside it you heal, and enemies are Soaked and Seeded. Each perfect guard you make inside it closes a petal around the attacker and holds it in place. When the last petal closes, or time runs out, the lotus bursts. It hits harder for every petal that closed.

**When it happens.** At a rest fire, pick it for your surge slot in the Hand while you carry both Water and Life. Light it with the surge button. Drink a flask while it is lit to open the lotus.

**The numbers.** 40 mana, lasts 10 seconds. 5 Shards. You can buy it once you carry both elements; no god gives it. The lotus has a 3-metre radius and lasts 10 seconds. Every second, each enemy in it gets Soaked and a stack of Seeded, and you heal 2 while you stand in it. There are five petals; each holds its enemy for 3 seconds. The burst has a radius of about 5 metres. It deals 30 damage, plus 30% more for each petal closed (75 with all five), 40 stagger, and a stack of Seeded.

**Works with.** Morning Dew (the flask gives a ward), Drowned Roots.

**Example.** Light the Great Lotus, drink a flask in the middle of a pack, then perfect-guard five blows inside the lotus. It bursts for 75 on every enemy it holds.
