# Steam: detailed explanations

The long explanation the chart shows for every mark on the duo branch Fire and Water share, named for its reaction,
Steam (`docs/design/steel/pairs_d4.md`, "Fire and Water"). Drawn in the heart of the chart while Fire and Water are
both carried.

## steam.shape
**What it does.** Two Sisters unlocks two new shapes for Steam. At a rest fire, you pick which shape your Steam uses. From then on, it uses that shape instead of the plain one. **Blind Steam**: the cloud of steam doesn't rise. It stays low around the enemy, a white cloud that enemies inside can't see out of. **Scald**: no cloud. Instead, the water flashes into steam in a harder burst that throws everything nearby, and the Burn isn't used up but keeps boiling.

**When it happens.** Every time Steam goes off while you carry both Fire and Water. Steam goes off when Fire lands on a Soaked enemy, or when Water lands on a burning one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Steam.

**The numbers.** Mix, 3 Shards. Blind Steam: like the plain one, the Soaked is used up, along with half the Burn. The cloud covers 3 metres around the enemy for 4 seconds and stings for 3 damage a second. Enemies in it when it forms can only see 2 metres for those 4 seconds, and their swings come a fifth slower. Scald: the Soaked is used up, but the Burn stays. A burst within 2.5 metres deals 14 damage, plus 6 more for each Burn the enemy has, and 30 stagger, and throws everything in it 1.5 metres. After Steam goes off on an enemy, it can't go off on that enemy again for 3 seconds.

**Works with.** Hissing Blade to set it up, Strike Through Steam, Kettle, The Sisters' Quarrel.

**Example.** A crowd closing in on you: use Blind Steam, and they lose you in the cloud. One brute with lots of Burn: use Scald, again and again, since its Burn is never used up.

**Not built yet.** An enemy that walks into Blind Steam's cloud after it forms gets stung, but not blinded.

## steam.hissingblade
**What it does.** Your heavy blows add Burn and Soaked at the same time.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Fire and Water.

**The numbers.** Mix, 3 Shards. One Burn and one Soaked.

**Works with.** Steam (one heavy blow on a fresh enemy adds both), Strike Through Steam.

**Example.** Land a heavy blow, and the enemy you hit steams right away.

## steam.strikethrough
**What it does.** A weapon blow on an enemy within a second of its Steam is a scald: it does 50% more damage and stagger.

**When it happens.** For one second after Steam goes off on an enemy, while you carry both Fire and Water. It works on your first weapon blow on that enemy in that second.

**The numbers.** Mix, 3 Shards. The blow deals 1.5 times its damage and stagger. During that second, every bit of stagger the enemy takes is raised the same way, even from spells.

**Works with.** Hissing Blade (the heavy blow that makes it steam, then a quick light blow), Steam.

**Example.** Land a heavy blow to make it steam, then cut through the cloud right away.

## steam.kettle
**What it does.** A Soaked enemy that dies while burning goes up as steam at full strength, with no wait.

**When it happens.** Each time a Soaked, burning enemy dies, while you carry both Fire and Water.

**The numbers.** Mix, keystone, 5 Shards. A burst deals 8 damage, plus 4 more for each Burn the enemy had, and 15 stagger to everything within 2.5 metres.

**Works with.** Hissing Blade, Steam, The Sisters' Quarrel.

**Example.** Kill a soaked, burning soldier in the middle of its pack, and the pack is scalded.

**Not built yet.** For now, it bursts like the plain Steam, whatever shape you picked, instead of in your chosen shape.

## steam.quarrel
**What it does.** Flame from one hand and a wave from the other close in like pincers on the enemy you face, up to 7 metres away. Where they meet, steam bursts, dealing 30 damage and 55 stagger within 3.5 metres, and Steam goes off at full strength. For 3 seconds after, your weapon blows on the enemies caught in it hit harder and burn.

**When it happens.** Cast it from a spell slot while you carry both Fire and Water. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Fire.

**The numbers.** Mix, duo spell, breaker, 5 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. The two hits close in over 0.45 seconds, each dealing 8 damage and 10 stagger, with one adding Burn and the other Soaked. The blast deals 30 damage and 55 stagger within 3.5 metres. Steam goes off on the enemy even if it would normally have to wait. For 3 seconds, your weapon blows on the enemy deal a quarter more damage, and each one adds a Burn.

**Works with.** Steam and its shapes, Strike Through Steam, Kettle.

**Example.** Trap the captain between flame and wave, then cut it while it is scalded.

**Not built yet.** The flame and the wave don't run along two curved paths yet, hitting what they cross. For now, two hits close in on the enemy from a ring around it.
