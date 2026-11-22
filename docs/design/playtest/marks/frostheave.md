# Frost Heave: detailed explanations

The long explanation the chart shows for every mark on the duo branch Ice and Earth share, named for its reaction,
Frost Heave (`docs/design/steel/pairs_d3.md`, "Ice and Earth"). Drawn in the heart of the chart while Ice and Earth are
both carried.

## frostheave.shape
**What it does.** Unlocks two new shapes for Frost Heave. At a rest fire, you pick which shape your Frost Heave uses. From then on, it uses that shape instead of the plain one. **Sorted Stones**: the heave pushes every other enemy away, leaving the heaved enemy alone with you. **Pingo**: the mound has a core of ice that bursts.

**When it happens.** Every time Frost Heave goes off while you carry both Ice and Earth. Frost Heave goes off when Tremor lands on a chilled enemy, or when Chill lands on a shaken one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Frost Heave.

**The numbers.** Mix, 3 Shards. Both shapes heave the enemy up 0.4 seconds later. It takes 12 damage and 25 stagger and is thrown into the air for 1 second, and the frozen mound stays behind it as with the plain Frost Heave. Sorted Stones: every other enemy within 3 metres is pushed 3 metres away. Pingo: 2 seconds later, the mound bursts apart for 24 damage and two Chill within 2.5 metres. After Frost Heave goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Hoarfrost and Stone Rise to set it up, Hard Frost, The Hag's Staff.

**Example.** A one-on-one fight in a crowd: Sorted Stones clears space around you. A pack chasing you: use Pingo, and lead them over it.

**Not built yet.** Sorted Stones' ring of stones doesn't appear on the floor yet. For now, it just shoves the other enemies away.

## frostheave.hardfrost
**What it does.** Heavy blows on a chilled enemy shake it. Heavy blows on a shaken enemy chill it.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Ice and Earth.

**The numbers.** Mix, 3 Shards. One Tremor on a chilled enemy, or one Chill on a shaken enemy.

**Works with.** Frost Heave (one heavy blow starts it), Permafrost, Winter in the Bones.

**Example.** Chill a soldier with Hoarfrost, and one heavy blow heaves it.

## frostheave.footing
**What it does.** A perfect guard while you stand within 2 metres of a mound heaves the ground under the attacker.

**When it happens.** Each perfect guard (press the guard button just before a blow lands) while you stand within 2 metres of a frozen mound.

**The numbers.** Mix, 3 Shards. Frost Heave goes off on the attacker, even if it would normally have to wait.

**Works with.** Frost Heave, Hard Frost, Mountain Stance.

**Example.** A chilled, shaken brute swings. Guard it perfectly, and it goes flying up.

## frostheave.permafrost
**What it does.** Tremor doesn't fade from a chilled enemy.

**When it happens.** Always on once you learn it, while you carry both Ice and Earth.

**The numbers.** Mix, a minor mark, 1 Shard. Once the Chill is gone, the Tremor starts to fade as usual.

**Works with.** Hard Frost, which it comes after on the chart, Frost Heave.

**Example.** A chilled soldier stays shaken.

## frostheave.bones
**What it does.** A shaken enemy that is Frozen stays frozen twice as long. Shattering it heaves the ground under every enemy within 4 metres.

**When it happens.** Each time a shaken enemy is Frozen or shattered, while you carry both Ice and Earth.

**The numbers.** Mix, keystone, 5 Shards. A shaken enemy's freeze lasts twice as long as usual. When it is shattered, everything within 4 metres is thrown into the air for 1 second and takes 12 damage, 25 stagger and a Tremor, at most three times a second.

**Works with.** Hard Frost, Hoarfrost, Frost Heave.

**Example.** Shatter a frozen, shaken soldier in a pack, and the whole pack goes flying up.

## frostheave.hagsstaff
**What it does.** You strike the ground and hammer frost into it. A circle around you turns hard and white. Each second, every enemy on it is chilled and shaken, so it heaves when the two afflictions meet.

**When it happens.** Cast it from a spell slot while you carry both Ice and Earth. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Ice.

**The numbers.** Mix, duo spell, breaker, 5 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. It covers 6 metres around you for 6 seconds. Each second, it deals 2 damage, a Chill and a Tremor.

**Works with.** Frost Heave and its shapes, Hard Frost, Winter in the Bones.

**Example.** Strike it in the middle of a pack, and fight among the enemies flying up.

**Not built yet.** Your heavy blows on the frozen ground don't push frost under the enemy you hit yet.
