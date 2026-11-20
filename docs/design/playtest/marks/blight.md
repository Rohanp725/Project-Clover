# Blight: detailed explanations

The long explanation the chart shows for every mark on Blight, the duo branch Dark and Poison share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Dark and Poison") and drawn in the heart of the chart while Dark and Poison are both
carried. Its reaction is Shut in the Dark, the jar; its Shape mark lets you choose what the jar does.

## blight.shape
**What it does.** Opens Shut in the Dark's two shapes. At a rest fire you choose which one your jar wears, and from then on it runs instead of the plain one. **Swarm**: when the jar breaks, its swarm crawls to the nearest enemy and seals a new jar there. **Rot to the Bone**: while it is sealed, the rot eats the enemy's balance.

**When it happens.** Every time Shut in the Dark fires (Poison landing on a cursed enemy, or a Curse on a poisoned one) while Dark and Poison are both carried. Before this mark is bought, or with only one of the two carried, the plain jar runs.

**The numbers.** Mix, 3 Shards. Both: sealed 8 seconds with its doses kept from fading; when the time ends it breaks for 5 damage a dose in 3 metres and half its doses on all it reaches, and the same as it dies. Swarm: as it breaks, 2 doses and a Curse leap to the nearest enemy within 8 metres, which may seal a new jar there. Rot to the Bone: 9 stagger every second while it is sealed. Shut in the Dark rests 8 seconds on each enemy.

**Works with.** Miasma and Grasp to bring the two, Wither, Feeding Rot, One Death Feeds, The Reaping, Wasting Grip.

**Example.** A pack: Swarm walks the jar from one to the next. A brute: Rot to the Bone, and a finisher when its balance breaks.

**Not built yet.** The swarm leaps as a stroke of poison and curse rather than crawling, and Rot to the Bone's stagger comes on a beat rather than with each bite's doses, and the meter still recovers.

## blight.wither
**What it does.** Poison bites a quarter harder on a Cursed enemy, and does not fade from it while the curse lasts.

**When it happens.** Always on once learned, while you carry both Dark and Poison. Whenever poison bites a Cursed enemy.

**The numbers.** Mix, a minor, 1 Shard. Poison on a Cursed enemy bites 25% harder and keeps its doses for as long as the curse holds. Re-aimed in Phase 4: its heavy blows no longer lay a Curse and poison (the jar does that work). Of your first element's affliction, once its root is learned, it adds only what goes beyond the root's own stack: with Dark first the Curse, one deep at most, adds nothing; with Poison first the poison adds one dose. A mixed branch's marks are the exception to the rule that weapon blows lay only your first element's affliction.

**Works with.** Curse or Venom, Feeding Rot, Shut in the Dark, The Reaping, Grasp and Miasma, and the infused strikes (R2 lays the second element).

**Example.** Poison first: lay eight doses with blade and heavy blows; the heavy blows Curse the soldier as they go. While it stays Cursed the poison never runs out and bites for 20 at a time.

## blight.feedingrot
**What it does.** Each time poison bites a Cursed enemy, a little of it comes back to you as health.

**When it happens.** Every bite of poison, on its slow beat, on an enemy that is Cursed.

**The numbers.** Mix, 3 Shards. Half a point of health for each dose that bit: 4 at eight doses, every 2 seconds.

**Works with.** Wither (the poison does not fade while the curse holds), Curse, Dread, One Death Feeds, Wasting Grip.

**Example.** Three Cursed soldiers at eight doses each: every two seconds you heal 12 while you guard.

## blight.wastinggrip
**What it does.** You brace and pour a short stream of rot at what stands before you, biting again and again and laying Curse. While you hold this mark, any spell of yours that strikes a poisoned enemy also feeds you.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). While it pours, the stick or the cursor turns it slowly. A dodge lets it go, and a blow that lands on you breaks it. The feeding comes whenever a spell of yours strikes a poisoned enemy.

**The numbers.** Mix, duo spell, 5 Shards, 40 mana. On an enemy already Cursed and poisoned its stream seals the jar at once, whatever its rest (once each 2.5 seconds at most). A bar 2.5 metres long and 1.2 metres wide, after a 0.3-second brace, for 2.5 seconds. It bites every half second for 8 damage, 6 stagger and a Curse: five bites on what stays in it. A spell striking a poisoned enemy heals 5, at most once a second. As built it does not yet take hold of an enemy, drink its poison or throw it.

**Works with.** Wither, Feeding Rot, Blood Price (pay in health when short of mana), Miasma and Grasp (both feed it when they strike the poisoned), and the charged blade (it charges with Dark).

**Example.** A poisoned brute in front of you: pour the rot over it and it is Cursed through the bites while every spell you land on it heals 5.

## blight.onedeathfeeds
**What it does.** When a Cursed enemy dies, the poison in every other enemy near it bites at once, out of turn, and you are fed for each one bitten.

**When it happens.** The moment a Cursed enemy dies, whatever killed it; at most twice a second.

**The numbers.** Mix, 5 Shards. Every poisoned enemy within 12 metres of where it fell bites at once for its doses, and you heal 4 for each one bitten. Each poison keeps its own beat afterward.

**Works with.** Wither, Feeding Rot (each extra bite on a Cursed one feeds again), Shut in the Dark's Swarm, Curse.

**Example.** Five soldiers at eight doses: kill one Cursed, and the other four bite for 16 each at once while you heal 16.

## blight.reaping
**What it does.** Your finisher on an enemy both Cursed and poisoned reaps the field. A blade as long as the field sweeps once round it, time slows, and all the poison in every enemy near lands at once. Half of all it deals comes back to you as health.

**When it happens.** Every finisher (a heavy blow on a staggered enemy) that lands on an enemy carrying both Curse and poison.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. The sweep takes 0.42 seconds and reaches 9 metres round the finished enemy. Each poisoned enemy in it has its poison set off and spent: 9 damage a dose (72 at eight) and 4 stagger a dose. Half the total heals you.

**Works with.** Wither, Shut in the Dark (spread the doses first), Devouring Maw, Miasma, Grasp, Never Sated.

**Example.** Four soldiers at eight doses round a Cursed brute: finish the brute and each takes 72 while you heal half of it.
