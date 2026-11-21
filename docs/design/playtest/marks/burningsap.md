# Burning Sap: detailed explanations

The long explanation the chart shows for every mark on the duo branch Light and Poison share, named for its reaction,
Burning Sap (`docs/design/steel/pairs_d1_d2.md`, "Light and Poison"). Drawn in the heart of the chart while Light and
Poison are both carried.

## burningsap.shape
**What it does.** Lit Veins unlocks two new shapes for Burning Sap. At a rest fire, you pick which shape your Burning Sap uses. From then on, it uses that shape instead of the plain one. **The Light Finds Them**: when it goes off, enemies all across the room are Marked too, and their veins light up. **Blistered**: the blisters don't wait until the end. Each one bursts on the blow that raises it.

**When it happens.** Every time Burning Sap goes off while you carry both Light and Poison. Burning Sap goes off when poison lands on a Marked enemy, or when a Mark lands on a poisoned one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Burning Sap.

**The numbers.** Mix, 3 Shards. The Light Finds Them: the sap wakes up for 3 seconds, like the plain one. Every enemy within 8 metres is Marked. At the end, the poison bites, and each blister bursts, putting two doses on everything within 2.5 metres. Blistered: for 3 seconds, each weapon blow you land on the enemy splashes two doses on everything within 2.5 metres. At the end, the poison bites. After Burning Sap goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Glare and Miasma to set it up, Seen Through, Nectar After Poison, Bared to the Sun.

**Example.** A poisoned pack: The Light Finds Them Marks them all at once, ready for your critical hits. A one-on-one fight: use Blistered, and three quick blows splash poison onto any enemy that comes to help.

**Not built yet.** The Light Finds Them Marks every enemy within 8 metres, not only the poisoned ones. Blistered's blisters come with every weapon blow, not only critical hits.

## burningsap.nectar
**What it does.** An enemy that dies with both a Mark and poison on it gives you health and mana.

**When it happens.** Each time an enemy with both dies, while you carry both Light and Poison.

**The numbers.** Mix, 3 Shards. You heal 6 and gain 8 mana.

**Works with.** Burning Sap, Bared to the Sun, and Daybreak and One by One to spend the mana.

**Example.** Clear out a Marked, poisoned pack, and walk away with more health and mana.

## burningsap.seenthrough
**What it does.** A perfect guard against a poisoned enemy Marks it and makes all its poison bite at once.

**When it happens.** Each perfect guard (press the guard button just before a blow lands) against a poisoned enemy.

**The numbers.** Mix, 3 Shards. It adds a Mark, and every dose the enemy has bites right away.

**Works with.** Burning Sap (a Mark on a poisoned enemy starts it), Bright Venom, Five Needles.

**Example.** A poisoned brute swings. Guard it perfectly, and it is Marked, bitten, and its sap starts to wake up.

## burningsap.brightvenom
**What it does.** The saved-up poison bites hit 15% harder when they land.

**When it happens.** Each time Burning Sap goes off, while you carry both Light and Poison.

**The numbers.** Mix, a minor mark, 1 Shard. For now: one extra dose of poison when Burning Sap goes off.

**Works with.** Seen Through, which it comes after on the chart, Burning Sap.

**Example.** Each time the sap wakes up, it carries one more dose into its bite.

**Not built yet.** The saved-up bites don't hit harder yet. One extra dose takes its place for now.

## burningsap.bared
**What it does.** You tear the shade away from a spot. A sharp beam of sunlight shines down where you aim for 5 seconds. Every enemy in it is Marked. Poisoned enemies start Burning Sap right away, even if Burning Sap would normally have to wait.

**When it happens.** Cast it from a spell slot while you carry both Light and Poison. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Light.

**The numbers.** Mix, duo spell, opener, 5 Shards. It costs no mana. After you cast it, you must wait 13 seconds to cast it again. Fighting with your weapon makes the wait shorter. It lands up to 8 metres ahead of you, covers a circle with a 2.5-metre radius, and lasts 5 seconds. Once a second, it deals 2 damage and adds a Mark to every enemy in it. A poisoned enemy starts Burning Sap once in each cast. Each enemy keeps its own wait, so every poisoned enemy in the beam gets its turn.

**Works with.** Burning Sap and its shapes, Miasma (poison them first), Nectar After Poison.

**Example.** Poison a pack, then shine the sun on it.

**Not built yet.** Your weapon blows inside the beam don't raise more blisters yet.
