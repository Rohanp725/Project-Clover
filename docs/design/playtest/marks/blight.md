# Blight: detailed explanations

The long explanation the chart shows for every mark on Blight, the duo branch Dark and Poison share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Dark and Poison") and drawn in the heart of the chart while Dark and Poison are both
carried. Its reaction is Shut in the Dark, the jar; its Shape mark lets you choose what the jar does.

## blight.shape
**What it does.** Unlocks two new shapes for Shut in the Dark, the jar. At a rest fire, you pick which shape your jar uses. From then on, it uses that shape instead of the plain one. **Swarm**: when the jar breaks, a swarm crawls out to the nearest enemy and seals a new jar on it. **Rot to the Bone**: while the jar is sealed, the rot wears down the enemy's balance.

**When it happens.** Every time Shut in the Dark goes off while you carry both Dark and Poison. It goes off when Poison lands on a Cursed enemy, or when a Curse lands on a poisoned one. If you have not bought this mark, or you carry only one of the two elements, you get the plain jar.

**The numbers.** Mix, 3 Shards. Both shapes: the enemy is sealed for 8 seconds, and its poison doses don't fade during that time. When the 8 seconds end, or when the enemy dies, the jar breaks. It deals 5 damage for each dose to everything within 3 metres, and gives each of them half its doses. Swarm: as the jar breaks, 2 doses and a Curse jump to the nearest enemy within 8 metres, which may seal a new jar there. Rot to the Bone: 9 stagger every second while the jar is sealed. After Shut in the Dark goes off on an enemy, it can't go off on that enemy again for 8 seconds.

**Works with.** Miasma and Grasp to set it up, Wither, Feeding Rot, One Death Feeds, The Reaping, Wasting Grip.

**Example.** Against a pack, Swarm moves the jar from one enemy to the next. Against a brute, use Rot to the Bone, then land a finisher when its balance breaks.

**Not built yet.** The swarm jumps across as a streak of poison and Curse instead of crawling. Rot to the Bone's stagger comes on a steady beat, not with each bite of poison. The balance meter still refills.

## blight.wither
**What it does.** Poison hits a quarter harder on a Cursed enemy. It also doesn't fade from that enemy for as long as the Curse lasts.

**When it happens.** Always on once you learn it, while you carry both Dark and Poison. It works whenever poison bites a Cursed enemy.

**The numbers.** Mix, a minor mark, 1 Shard. Poison on a Cursed enemy bites 25% harder and keeps all its doses for as long as the Curse lasts. This mark was changed in Phase 4: your heavy blows no longer add a Curse and poison (the jar does that job now).

**Works with.** Curse or Venom, Feeding Rot, Shut in the Dark, The Reaping, Grasp and Miasma, and the infused strikes (R2 adds the second element).

**Example.** Poison first: put eight doses on a soldier with your blade, then Curse it with Grasp or your second element's strike. While it stays Cursed, the poison never runs out, and it bites for 20 each time.

## blight.feedingrot
**What it does.** Each time poison bites a Cursed enemy, you get a little of it back as health.

**When it happens.** Every bite of poison on a Cursed enemy. Poison bites on a slow, steady beat.

**The numbers.** Mix, 3 Shards. You heal half a point for each dose that bit. At eight doses, that is 4 health every 2 seconds.

**Works with.** Wither (the poison doesn't fade while the Curse lasts), Curse, Dread, One Death Feeds, Wasting Grip.

**Example.** Three Cursed soldiers each have eight doses. Every two seconds you heal 12, even while you guard.

## blight.wastinggrip
**What it does.** You brace yourself and pour a short stream of rot at whatever is in front of you. It bites again and again and adds a Curse. While you have this mark, any of your spells that hits a poisoned enemy also heals you.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While it pours, you can turn it slowly with the stick or the cursor. Dodging stops it, and getting hit breaks it. The healing happens whenever one of your spells hits a poisoned enemy.

**The numbers.** Mix, duo spell, 5 Shards, 40 mana. If the enemy is already Cursed and poisoned, the stream seals the jar on it right away, even if the jar normally would have to wait (at most once every 2.5 seconds). The stream is 2.5 metres long and 1.2 metres wide. You brace for 0.3 seconds, then it pours for 2.5 seconds. It bites every half second for 8 damage, 6 stagger and a Curse. An enemy that stays in it takes five bites. A spell that hits a poisoned enemy heals you 5, at most once a second. For now, it doesn't yet grab an enemy, drink its poison, or throw it.

**Works with.** Wither, Feeding Rot, Blood Price (pay with health when you run low on mana), Miasma and Grasp (both heal you when they hit a poisoned enemy), and the charged blade (it charges your blade with Dark).

**Example.** A poisoned brute stands in front of you. Pour the rot over it. The bites Curse it, and every spell you land on it heals you 5.

## blight.onedeathfeeds
**What it does.** When a Cursed enemy dies, the poison in every other enemy near it bites right away, without waiting for its next beat. You heal for each enemy bitten.

**When it happens.** The moment a Cursed enemy dies, no matter what killed it. At most twice a second.

**The numbers.** Mix, 5 Shards. Every poisoned enemy within 12 metres of where it fell gets bitten right away, for its doses. You heal 4 for each one bitten. After that, each poison goes back to its own beat.

**Works with.** Wither, Feeding Rot (each extra bite on a Cursed enemy heals you again), Shut in the Dark's Swarm shape, Curse.

**Example.** Five soldiers each have eight doses. Kill one that is Cursed, and the other four each take 16 at once while you heal 16.

## blight.reaping
**What it does.** Your finisher on an enemy that is both Cursed and poisoned sweeps the whole area. A huge blade swings once around it, and time slows down. All the poison in every enemy nearby hits at once. You get half of all that damage back as health.

**When it happens.** Every finisher (a heavy blow on a staggered enemy) that lands on an enemy that has both a Curse and poison.

**The numbers.** 5 Shards. You can buy it once you carry both elements; no god gives it. The sweep takes 0.42 seconds and reaches 9 metres around the finished enemy. Every poisoned enemy it reaches has all its poison set off and used up: 9 damage and 4 stagger for each dose (72 damage at eight doses). You heal half the total damage.

**Works with.** Wither, Shut in the Dark (spread the doses first), Devouring Maw, Miasma, Grasp, Never Sated.

**Example.** Four soldiers, each with eight doses, stand around a Cursed brute. Finish the brute, and each soldier takes 72 while you heal half of it.
