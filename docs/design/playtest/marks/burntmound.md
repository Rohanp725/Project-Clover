# Burnt Mound: detailed explanations

The long explanation the chart shows for every mark on the duo branch Earth and Fire share, named for its reaction,
Burnt Mound (`docs/design/steel/pairs_d5.md`, "Earth and Fire"). Drawn in the heart of the chart while Earth and Fire
are both carried.

## burntmound.shape
**What it does.** Fired Hard unlocks two new shapes for Burnt Mound. At a rest fire, you pick which shape your Burnt Mound uses. From then on, it uses that shape instead of the plain one. **Heaped Stones**: the three hot stones melt together into one boulder. Your heavy blow rolls it through a line of enemies. **Fire-Cracked**: you don't hit the stones. Instead, each stone cracks on its own when an enemy steps near it, so you can drive enemies across a field of them.

**When it happens.** Every time Burnt Mound goes off while you carry both Earth and Fire. Burnt Mound goes off when a Burn lands on a shaken enemy, or when Tremor lands on a burning one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Burnt Mound.

**The numbers.** Mix, 3 Shards. Heaped Stones: one red-hot boulder appears 1.5 metres from the enemy and stays for 6 seconds. It burns anything that touches it (3 damage and a Burn every half second). Your first heavy blow on the enemy sends the boulder rolling 8 metres away from you. Each enemy it rolls over takes 20 damage, 20 stagger, two Burns and two Tremors. Fire-Cracked: three stones appear 1.5 metres from the enemy and stay for 6 seconds. Each one cracks once, the first time an enemy steps within 1 metre of it: 12 damage and a Burn to everything within 2 metres, and the stone is gone. After Burnt Mound goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Timber-Laced, Hot Stones, Brick and Fire (which keeps both afflictions coming), Vitrified Wall.

**Example.** A line of soldiers: use Heaped Stones, and hit the first one toward the rest with a heavy blow. A crowd rushing at you: use Fire-Cracked, and fight from behind the stones.

**Not built yet.** Heaped Stones' boulder doesn't yet roll from where it lies. For now, your heavy blow on the enemy sends the roll out from the enemy itself.

## burntmound.timberlaced
**What it does.** Rock you raise catches fire from a burning enemy within 2 metres. It burns anything that touches it for 4 damage and a Burn every half second. When it ends, it bursts into shards of glass that deal 12 damage within 2.5 metres.

**When it happens.** For now, each time one of your spells hits a burning enemy, while you carry both Earth and Fire.

**The numbers.** Mix, 3 Shards. The ground under the enemy burns in a patch 2 metres across for 3 seconds (4 damage and a Burn every half second). Then it bursts into glass shards for 12 damage and 10 stagger within 2.5 metres. At most once a second.

**Works with.** Burnt Mound, Earth's raised stones, Brick and Fire, Vitrified Wall.

**Example.** Raise stone under a burning brute, and the ground catches fire.

**Not built yet.** Your raised rock doesn't catch fire itself yet. For now, any spell that hits a burning enemy puts the burning ground and the glass shards under it.

## burntmound.hotstones
**What it does.** Burnt Mound's stones stay 2 seconds longer and roll 2 metres farther.

**When it happens.** Each time Burnt Mound goes off.

**The numbers.** Mix, a minor mark, 1 Shard. The three stones stay for 8 seconds instead of 6. With Heaped Stones, the boulder stays for 8 seconds and rolls 10 metres instead of 8. With Fire-Cracked, the stones stay for 8 seconds.

**Works with.** Burnt Mound and its shapes, Timber-Laced.

**Example.** The stones are still glowing when the pack comes back around.

## burntmound.brickfire
**What it does.** Heavy blows on a burning enemy add Tremor. Light blows on a shaken enemy add Burn. Mixing light and heavy blows keeps both coming.

**When it happens.** Each heavy blow on a burning enemy, and each light blow on a shaken one, while you carry both Earth and Fire.

**The numbers.** Mix, keystone, 5 Shards. One Tremor or one Burn per blow. For your first element's affliction, it adds nothing beyond what that element's root mark already adds.

**Works with.** Burnt Mound (each affliction brings on the other, so the stones keep coming), Timber-Laced, Vitrified Wall.

**Example.** Hit one enemy light, light, heavy, and it shakes and burns, back and forth.

## burntmound.wall
**What it does.** A wall of stone woven with burning wood rises 4 metres ahead of you. It is 6 metres wide and blocks for 5 seconds. A heavy blow on it, or the end of its time, melts it: the stone turns to black glass and bursts outward. The burst deals 30 damage, 60 stagger, two Burns and two Tremors within 3 metres.

**When it happens.** Cast it from a spell slot while you carry both Earth and Fire. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Earth.

**The numbers.** Mix, duo spell, breaker, 5 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. It rises after 0.4 seconds and stands for 5 seconds. The burst deals 30 damage, 60 stagger, two Tremors and two Burns to everything within 3 metres.

**Works with.** Burnt Mound (everything in the burst gets both afflictions), Brick and Fire, Timber-Laced.

**Example.** Block the pack with the wall, keep them pinned against it, and let it burst in their faces.

**Not built yet.** Enemies standing where it rises are not thrown aside yet. The wall doesn't burn anything that touches it yet. A heavy blow on it doesn't make it burst early yet; for now, it bursts when its time ends.
