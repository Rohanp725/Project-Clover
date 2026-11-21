# Carried Embers: detailed explanations

The long explanation the chart shows for every mark on the duo branch Fire and Wind share, named for its reaction,
Carried Embers (`docs/design/steel/pairs_d1_d2.md`, "Fire and Wind"). Drawn in the heart of the chart while Fire and Wind
are both carried.

## carriedembers.shape
**What it does.** Unlocks two new shapes for Carried Embers. At a rest fire, you pick which shape your Carried Embers uses. From then on, it uses that shape instead of the plain one. **Fire Whirl**: the flame rises up on the enemy as a spinning column. It pulls nearby enemies into it and sets them on fire. **Hot Wind**: instead of embers, one long gust blows away from you.

**When it happens.** Every time Carried Embers goes off while you carry both Fire and Wind. Carried Embers goes off when Unsteady lands on a burning enemy, or when Burn lands on an Unsteady one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Carried Embers. The plain one sends a column of fire 5 metres with the wind, setting fire to what it reaches.

**The numbers.** Mix, 3 Shards. Fire Whirl: lasts 3 seconds. Once a second, every enemy within 3 metres is pulled 1 metre toward it, and every enemy within 2 metres gets a Burn. Hot Wind: a path 8 metres long and 2 metres wide past the enemy puts a Burn on everything in it, and the enemy is shoved 2 metres. After Carried Embers goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Flame Wave and Gust to set it up, Fan the Flame, The Fire Breathes In.

**Example.** A ring of soldiers: Fire Whirl pulls them in around the burning one. A hallway: stand at one end, and Hot Wind burns all the way down it.

**Not built yet.** The whirl stays on the enemy, instead of moving around and pulling as a danger of its own. Hot Wind only shoves the enemy, not everything in the path.

## carriedembers.fan
**What it does.** A perfect dodge fans the flames on the attacker. It gets one more stack of Burn. If it is also Unsteady, Carried Embers goes off again right away, even if it would normally have to wait.

**When it happens.** Each perfect dodge (roll right as a blow would land) against a burning enemy.

**The numbers.** Mix, 3 Shards. One Burn on the enemy you face. Carried Embers goes off without waiting its usual 4 seconds.

**Works with.** Gust and Updraft (they add Unsteady), Long Embers, The Wind Carries It.

**Example.** A burning, Unsteady brute swings. Dodge it perfectly, and its embers fly again into the pack behind it.

## carriedembers.long
**What it does.** When Carried Embers goes off, one extra ember flies 2 metres farther than the rest.

**When it happens.** Each time Carried Embers goes off, plain or with a shape, while you carry both Fire and Wind.

**The numbers.** Mix, a minor mark, 1 Shard. Four embers fly instead of three, and they land 2 metres farther with the wind. The farthest lands about 8.5 metres away.

**Works with.** Carried Embers, The Wind Carries It.

**Example.** Embers land on the archers at the back, as well as the soldiers in front.

## carriedembers.carries
**What it does.** Your heavy blows on a burning enemy throw its embers with the wind, as if Carried Embers had gone off. You don't need Unsteady, and there's no wait. Each throw uses up one of its Burn stacks. Where you stand when you hit decides where the fire goes, so with every heavy blow you choose where it lands.

**When it happens.** Every heavy blow or finisher that lands on a burning enemy, while you carry both Fire and Wind.

**The numbers.** Mix, keystone, 5 Shards. Three small fires land 5 metres past the enemy, 1 metre apart. Each is 1 metre across and lasts 3 seconds, dealing 3 damage and a Burn every half second. One of the enemy's Burn stacks is used up.

**Works with.** Cinder (gives you Burns to use up), Flame Wave, Long Embers, Fan the Flame.

**Example.** A burning soldier stands between you and a pack. Three heavy blows later, the pack is standing in fire.

## carriedembers.breathesin
**What it does.** A ring of flame appears where you aim. The fire sucks in its own wind. Every nearby enemy is pulled toward it and made Unsteady as it comes, and then the fire bursts over them.

**When it happens.** Cast it from a spell slot while you carry both Fire and Wind. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Fire.

**The numbers.** Mix, duo spell, opener, 5 Shards. It costs no mana. After you cast it, you must wait 16 seconds to cast it again. Fighting with your weapon makes the wait shorter. It lands up to 6 metres ahead of you. For 3 seconds, it pulls in everything within 6 metres. Then it bursts for 20 damage, 20 stagger, two Burns and an Unsteady.

**Works with.** Carried Embers (enemies come out burning and Unsteady), The Wind Carries It, Fan the Flame.

**Example.** Cast it into a spread-out fight, then fight the bunch of enemies it pulls together.

**Not built yet.** The ring doesn't yet stay for 6 seconds burning what's inside. The pull doesn't yet reach 8 metres or make enemies Unsteady every 2 seconds. A heavy blow doesn't yet throw an enemy toward it.
