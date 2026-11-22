# Life: detailed explanations

The long explanation the chart shows for each high-level mark of the Life tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Life): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Grown" means anything of yours that grows from the
ground: Briar Patch's bed, a thornbush, a sapling, a thicket, a tuft of briar.

## life.seed
**What it does.** Every weapon blow that lands plants a seed in the enemy: one stack of Seeded, up to three. Each weapon blow after that on a Seeded enemy heals you a little, and more for each stack. When a Seeded enemy dies, it bursts into thorns that hit everyone near it.

**When it happens.** Always on once you learn it: every weapon blow that lands, light or heavy, while Life is your first element (or you are all in on Life). If Life is your second element, Seeded comes from your spells, the charged blade and the R2 strike instead.

**The numbers.** 2 Shards. One stack per blow, up to three, lasting 8 seconds. Each weapon blow on a Seeded enemy heals you 1 health for each stack, so 3 at full. When it dies, it deals 12 damage per stack to everyone within 3 metres, so 36 at three stacks.

**Works with.** Ripening, Fallen Seeds, Reap What Is Sown (it bursts every seed it reaches), Root and Branch, Regrowth (one more stack per blow), Briar Patch, and Rot Roots when Poison meets the seed.

**Example.** With the Twin Knives, three quick cuts give a soldier three stacks. Every cut after that gives you back 3 health while you keep attacking it.

## life.fallen
**What it does.** Where a Seeded enemy dies, a thornbush grows and stays for a while. It lashes and Seeds whatever stands in it. The bush counts as something you grew.

**When it happens.** Whenever an enemy with Seeded dies, no matter what killed it.

**The numbers.** 3 Shards. A bush with a 2-metre radius that lasts 8 seconds. Every half second, it deals 4 damage and one stack of Seeded. Up to four can stand at once; a fifth replaces the oldest.

**Works with.** Seeded (its death burst happens too), Reap What Is Sown and Elder Tree, which come after it on the chart, Root and Branch above them, Drawn From the Wood (it uses up the bushes), Root Walk (a place to come up), Wildwood.

**Example.** Kill a seeded soldier in a doorway. The bush where it fell Seeds everyone who follows it through.

## life.reap
**What it does.** Reap What Is Sown, the detonator of the Life tree. You call in the harvest. Everything you've grown bursts where it stands, the seeds in nearby enemies burst right away and are used up, and you heal for each burst.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 10 seconds.) Each thing you grew, wherever it stands, bursts in a 3-metre circle for 16 damage and 25 stagger, then disappears, and heals you 5. A call reaches 6 metres around you 0.2 seconds after you cast it, dealing 16 damage and 25 stagger. Every Seeded enemy it hits has its seed burst right away, for 11 damage and 5 stagger per stack to everyone within 3 metres, and heals you 5.

**Works with.** Fallen Seeds and Sapling (the things it bursts), Briar Patch, Seeded and Ripening, Wood Grows Back (the garden comes back), Wildwood (more to burst), and the charged blade: it charges your weapon with Life when it lands.

**Not built yet.** Every Seeded enemy in the fight should burst, not only those within the call's 6 metres.

**Example.** Three saplings stand around you and a pack closes in. Reap, and four bursts go off at once while you heal 20.

## life.reap.shape
**What it does.** Unlocks two shapes for Reap What Is Sown. You pick one at any rest fire. **Golden Sickle** (the druids' sickle): only the Seeded enemies within eight metres are reaped, each cut once by one sweep of gold, and the things you grew stay standing. **Second Sowing**: everything bursts like before, and each burst plants a new seed in everything it reaches, so the cycle keeps going.

**When it happens.** Whenever you cast Reap What Is Sown, using the shape you picked. If you have not bought this, you cast the plain call.

**The numbers.** 3 Shards. Golden Sickle: one sweep 8 metres all around you, for 16 damage and 25 stagger. Each Seeded enemy it cuts has its seed burst, like the Learn mark's, and the things you grew are kept standing. Second Sowing: each seed that goes off (from Reap, Ripening or anything else) puts one Seeded on every enemy within 3 metres of it.

**Works with.** Ripening (its bursts plant seeds too, while you use Second Sowing); Fallen Seeds and Sapling, for the garden Golden Sickle should keep.

**Not built yet.** In Second Sowing, the things you grew should plant seeds as they burst; for now, only seeds that go off plant new ones.

**Example.** Use Second Sowing in a crowd. Reap once to Seed the whole pack again, cut, then reap again.

## life.reap.master
**What it does.** Cast it within half a second of a Seeded enemy dying, and every burst counts twice.

**When it happens.** For now, for half a second after any Seeded enemy dies. Reap What Is Sown must be in a spell slot.

**The numbers.** 3 Shards. For half a second, every seed that goes off bursts twice as hard (seed bursts are 100% stronger).

**Works with.** Seeded's own death burst (kill one, then reap the rest); Ripening's heavy blow in the same half second.

**Not built yet.** The garden's bursts and the healing should count twice too, and the half second should be tied to casting this spell. For now, any seed that goes off in that half second is doubled.

**Example.** Finish the seeded soldier and reap as it falls. The seeds in its friends burst for twice their thorns.

## life.tree
**What it does.** Elder Tree, the larger spell of the Life tree. A tree grows where you stand, from shoot to sapling to oak, in three seconds. Once grown, a ring lies under its branches for fifteen seconds. Enemies inside stay Seeded, you heal, and every few seconds a root bursts up under the enemies inside.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards, 40 mana (its Empower makes it 32, and makes it grow in 2 seconds). The ring has a 3-metre radius and lasts 18 seconds from when you cast it. Once a second, it deals 1 damage and adds Seeded. The roots first hit 3 seconds after planting, then every 3 seconds for 15 seconds, for 16 damage, 45 stagger and one Seeded. Each cast holds 15 seconds of shade: each second you spend in a fight uses one up and heals you 3. The tree doesn't count as something you grew, so Reap What Is Sown and Drawn From the Wood leave it alone.

**Works with.** Seeded, Ripening (heavy blows on what it Seeded), Fallen Seeds, which it comes after on the chart, Sapling's Master mark (a small tree of the same kind), and the charged blade: its first hit charges your weapon with Life.

**Not built yet.** The tree should have 40 health while it grows, and enemies should turn and attack it. Each root should come up under one enemy, staggered ones first (for now, the roots hit everything in the ring). The healing should only happen inside the ring while the tree stands (for now, it is 3 a second for 15 seconds of fighting, anywhere).

**Example.** Plant it where a pack has to come to you, and fight at its foot. Every three seconds, the roots stagger and Seed them while you heal.

## life.tree.shape
**What it does.** Unlocks two shapes for Elder Tree. You pick one at any rest fire. **Walking Tree**: the tree pulls up its roots and follows you at walking speed, with a smaller ring, like the stag with a forest growing through it. **Grown at Once**: no growing. It comes up fully grown in an instant but only stays for six seconds, so a slow setup becomes a quick burst.

**When it happens.** Whenever you cast Elder Tree, using the shape you picked. If you have not bought this, you cast the growing tree.

**The numbers.** 3 Shards. Walking Tree: the roots hit within 2 metres of it. Grown at Once: the roots hit right away and then every 3 seconds for 6 seconds, within 3 metres, for 16 damage, 45 stagger and Seeded.

**Works with.** Root Walk (go to where the tree is); The Garden Holds You and Wildwood near the ring.

**Not built yet.** Walking Tree following you; for now, it stays where it was planted, with the smaller ring. With either shape, the shade ring and the healing keep the Learn mark's times.

**Example.** Use Grown at Once on a staggered brute. The first root lands before it gets back up.

## life.tree.master
**What it does.** Plant it on something you grew, and it grows right away.

**When it happens.** For now, when you cast it within 4 metres of something you grew. Elder Tree must be in a spell slot.

**The numbers.** 3 Shards. For now, a root bursts up right away within 3 metres of you, for 16 damage, 45 stagger and Seeded, before the tree's own first root.

**Works with.** Fallen Seeds and Sapling, for something to plant it on; Briar Patch's bed; Wildwood.

**Not built yet.** The whole tree growing at once on the thing you grew. For now, the first root comes right away, and the tree still grows.

**Example.** Cast Briar Patch, step into the bed, and plant the tree on it. The roots answer right away.

## life.lift
**What it does.** Root and Branch, Life's reaction lift. Every reaction with Seeded in it heals you and plants a seed in everything it reaches.

**When it happens.** Whenever Seeded reacts on an enemy with another affliction it has (Rot Roots with Poison, and the rest).

**The numbers.** 3 Shards. Each of these reactions heals you 4 and puts one Seeded on everything it hit: the enemy where it happened and every enemy its blows reached.

**Works with.** Carrying a second element: every one of Life's ten reactions; Seeded; Fallen Seeds, which it comes after on the chart; Wood Grows Back above it.

**Example.** Life and Poison on a tight group. Rot Roots goes off on one, you heal 4, and the enemies next to it are Seeded for the next one.

## life.devotion
**What it does.** Wood Grows Back, Life's Devotion, for going all in on Life only. It is the land's rule, "wood forgets". Whatever you grow grows back where it stood after it is used up, burst or cut down, and every spell you cast plants a sapling where it lands. The garden never runs out, so you can use Reap What Is Sown and Root Walk as much as you like, and every fight becomes a forest.

**When it happens.** Only while you carry Life twice (all in). A sapling is planted each time a spell hits.

**The numbers.** 6 Shards. A sapling grows where a spell hits, at most one every 2 seconds. It reaches 3.5 metres and stays for 12 seconds. Every 1.5 seconds, it hits for 6 damage, 8 stagger and Seeded. Up to four can stand at once. Buying this also gives you the deep strike, Briar Root. Use it with L2 and R2 while you carry Life alone or twice. The cutting grows into a briar around the enemy's feet and roots it in place for a second.

**Works with.** Every spell in the Life tree; Reap What Is Sown and Drawn From the Wood, which use up the garden; Root Walk, which travels to it.

**Not built yet.** Whatever is used up should grow back where it stood 6 seconds later; for now, there are only the saplings each spell plants. Briar Root's briar should stay as something you grew.

**Example.** Briar Patch, Reap What Is Sown, then Briar Patch again. Every cast leaves a sapling, so the reaping always has something to burst.

## life.ripen
**What it does.** A heavy blow on a Seeded enemy bursts its seed right away, without waiting for it to die. The thorns hit everyone near it, and you heal.

**When it happens.** Every heavy blow or finisher that lands on a Seeded enemy, at most once a second.

**The numbers.** 3 Shards. The burst has a 3-metre radius and deals 11 damage and 5 stagger for each stack (33 damage and 15 stagger at three). The seed is used up, so it won't burst again when the enemy dies. You heal 10.

**Works with.** Seeded, Drawn From the Wood and The Garden Holds You, which come after it on the chart, Bitter Rind (a rooted enemy is easy to hit with a heavy blow), Elder Tree (it Seeds for you), Reap What Is Sown's Second Sowing shape.

**Example.** Two light blows and a heavy blow on a soldier in a crowd. The heavy blow bursts its seed through the pack and heals you 10.

## life.harvest
**What it does.** Drawn From the Wood. You kneel, and everything you've grown withers into you, bursting into thorns as it goes. In return, you get a short window of green blood: every weapon blow is a critical hit and heals you a fifth of its damage. You pay with the garden you grew, not your health.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). It needs something you grew nearby.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 10 seconds.) Each thing you grew, wherever it stands, bursts in a 2-metre circle for 12 damage and Seeded, then disappears. For now, if you cast it within 4 metres of something you grew, the window lasts 6 seconds: every weapon blow is a critical hit, and you get 20% of its damage back as health.

**Works with.** Fallen Seeds, Sapling, Briar Patch and Root Walk, for the garden it uses up; Wood Grows Back, to have the garden again; Ripening, which it comes after on the chart; and Seeded's healing on top.

**Not built yet.** The window should last a second and a half for each thing that withered, up to ten seconds, and only count things within 12 metres. The half-second kneel.

**Example.** Three bushes stand around a brute. Draw them in, and cut the brute with six seconds of critical, healing blows.

## life.harvest.shape
**What it does.** Unlocks two shapes for Drawn From the Wood. You pick one at any rest fire. **Blood in the Furrow**: the green blood soaks into the ground where you knelt, making a ring that Seeds everything in it each second. You fight in one place, instead of carrying the power around with you. **Feed the Ground**: the one spell where Life costs you health. Nothing withers. Instead, you give a quarter of your health to your garden, and for a while everything you've grown hits twice as hard and twice as often.

**When it happens.** Whenever you cast Drawn From the Wood, using the shape you picked. If you have not bought this, you cast the plain withering.

**The numbers.** 3 Shards. Blood in the Furrow: a ring 8 metres across where you knelt, lasting 6 seconds. Once a second, it deals 1 damage and adds Seeded. Feed the Ground: it costs 25 health, which turns grey so you can win it back with blows. You can't cast it with 25 health or less. Nothing withers. For 8 seconds, the things you grow hit twice as hard.

**Works with.** Blood in the Furrow: Bitter Rind and Briar Coat, which keep the fight in one place. Feed the Ground: Fallen Seeds, Sapling and Wildwood, for a garden worth feeding; and the Master mark, to pay with grey health.

**Not built yet.** Blood in the Furrow's window should only work while you stand in the ring. Feed the Ground should take a quarter of your current health (for now, it takes 25), and double the power and speed of what is already standing (for now, only what you grow during the 8 seconds hits harder).

**Example.** Use Blood in the Furrow at a narrow spot. Wait in the ring and let them come to you, getting Seeded with every step.

## life.harvest.master
**What it does.** Cast it within a second of getting hit, and the cost comes out of the grey health that blow left behind. Nothing withers, each thing you grew still gives its time, and Feed the Ground takes none of your real health.

**When it happens.** For now, for one second after any blow lands on you. Drawn From the Wood must be in a spell slot.

**The numbers.** 3 Shards. During that second, costs paid in health cost nothing: Feed the Ground's 25 isn't taken.

**Works with.** Feed the Ground; Briar Coat (the blow that lands cuts the attacker too); Seeded's healing, to win the grey health back.

**Not built yet.** Keeping the garden on that cast while each thing you grew still gives its time, and taking the cost from that blow's grey health. For now, only the health cost is skipped.

**Example.** Take a soldier's blow, then cast Feed the Ground right away. The garden hits twice as hard, and you pay nothing.

## life.garden
**What it does.** The Garden Holds You. Near anything you grew, you can't be thrown, pulled or knocked down, and normal blows don't make you flinch. Roots run from the nearest plant to your heels. Fight in the garden.

**When it happens.** Always on once you learn it, while something you grew stands within 4 metres of you.

**The numbers.** 3 Shards. For now, you take 20% less damage while something you grew stands within 4 metres.

**Works with.** Briar Patch and Fallen Seeds, for a garden to stand in; Root Walk and Wildwood, which come after it on the chart; Ripening.

**Not built yet.** The footing itself (never thrown, pulled or knocked down, and no flinching from normal blows), and the roots running to your heels. For now, it only cuts the damage you take in the garden.

**Example.** Stand in your own bed of briars against a brute. Its blows land for a fifth less while you trade hits.

## life.walk
**What it does.** Root Walk, the movement spell of the Life tree. You sink into the ground in a swirl of leaves and come up at something you grew, or ahead of you if there is nothing. Nothing can touch you under the ground. A sapling sprouts where you went down, and roots burst out around you as you come up.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 8 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 6 seconds and adds two Seeded.) For now, it is a step: up to 8 metres toward and past the enemy you face, or 6 metres ahead if there is none, and you can't be hit during it. Whatever it passes takes 10 damage, 15 stagger and Seeded, and your next heavy blow within half a second is a critical hit. The sapling where you went down reaches 3.5 metres and stays for 6 seconds. Every 1.5 seconds, it hits for 6 damage, 8 stagger and Seeded.

**Works with.** Fallen Seeds, Sapling and Briar Patch, for places to come up; The Garden Holds You, which it comes after on the chart; Wood Grows Back (use up the garden, and it comes back).

**Not built yet.** Going under the ground to something you grew within 12 metres, the one nearest to where you aim, and coming up half a second later. The roots should burst out around you as you come up, not along the way.

**Example.** You're surrounded. Root Walk out through the soldier in front of you. The sapling you leave keeps the rest busy.

## life.walk.shape
**What it does.** Unlocks two shapes for Root Walk. You pick one at any rest fire. **Up Through Them**: you go to a Seeded enemy instead, and come up underneath it, lifting it on roots. **Changeling**: you swap places with something you grew, so it stands where you stood, in the way of the blow aimed at you.

**When it happens.** Whenever you cast Root Walk, using the shape you picked. If you have not bought this, you cast the plain walk.

**The numbers.** 3 Shards. Up Through Them: a Seeded enemy the walk hits is thrown into the air for 0.6 seconds (less for elites and gods). Changeling: a thornbush with a 2-metre radius rises where you stood and lasts 8 seconds. Every half second, it deals 4 damage and Seeded. Enemies within 2.5 metres lose track of you for 1.5 seconds.

**Works with.** Up Through Them: Seeded, the charged blade, and every mark that works on a lifted enemy. Changeling: Fallen Seeds' bushes, and Reap What Is Sown to burst what you left behind.

**Not built yet.** Up Through Them should go to the nearest Seeded enemy, not the one you face. Changeling should swap places with something you grew; for now, a bush rises where you stood, and enemies near it attack it.

**Example.** Use Changeling to escape two soldiers closing in from both sides. Both of them swing at the bush you leave behind.

## life.walk.master
**What it does.** If you start it right as a blow would land on you, it counts as a perfect dodge, and the sapling you leave behind is a full thornbush.

**When it happens.** For now, every time you cast it. Root Walk must be in a spell slot.

**The numbers.** 3 Shards. It only counts if you start it in the last 0.3 seconds before an enemy's blow (one within 6 metres) lands on you. Then it is a perfect dodge: you get 12 mana, the game slows for a moment, the blow misses, and every mark that answers a perfect dodge answers this one. A thornbush with a 2-metre radius then grows where you went down and lasts 8 seconds. Every half second, it deals 4 damage and Seeded.

**Works with.** Every mark that uses mana; Reap What Is Sown (one more thing to burst); The Garden Holds You.

**Not built yet.** The thornbush doesn't replace the sapling yet; for now, it grows next to it.

**Example.** Root Walk out of a brute's swing, then walk again sooner. Each walk leaves a bush and a sapling behind.

## life.wildwood
**What it does.** The things you grow last three times as long, and you can have three more of each kind. The cost: far from your garden, your weapon is a fifth weaker.

**When it happens.** Always on once you learn it. The cost applies whenever nothing you grew stands within 7 metres of you.

**The numbers.** 6 Shards. The things you grow last three times as long (thornbushes 24 seconds, saplings 36). Three more of each kind can stand at once. Your weapon deals 20% less damage when nothing you grew is within 7 metres.

**Works with.** Fallen Seeds, Sapling, Briar Patch, The Garden Holds You, which it comes after on the chart, and Reap What Is Sown and Drawn From the Wood (more to use up).

**Not built yet.** The things you grow should last forever; for now, they last three times as long.

**Example.** Plant a ring of saplings at the top of a staircase and hold it. The garden lasts the whole fight, and you never have to leave it.

## life.rind
**What it does.** A normal guard Seeds the attacker. A perfect guard roots it in place for a moment.

**When it happens.** Each blow you block, and each perfect guard.

**The numbers.** 3 Shards. One stack of Seeded on the attacker for each blocked blow. A perfect guard holds it still for 1 second (less for elites and gods; a few huge enemies can't be held).

**Works with.** Briar Coat, which comes after it on the chart, Sapling (perfect guards plant saplings), Seeded, Ripening (a held enemy is easy to hit with a heavy blow).

**Example.** Perfect-guard a brute's swing. It stands rooted for a second, long enough for a heavy blow that bursts its seed.

## life.base
**What it does.** Briar Patch, Life's own spell. It comes with the element; you never buy it. A bed of thorny vines grows in front of you and stays, lashing, Seeding and slowing whatever stands in it. The bed counts as something you grew.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Life, you can always put it in a slot.

**The numbers.** Comes with Life. It costs no mana. After you cast it, you must wait 7 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 5.5 seconds and the bed 3.3 metres across. The bed is 2.5 metres across, lands 3 metres ahead of you, and lasts 6 seconds. Twice a second, it lashes for 4 damage and Seeded, and it slows whatever stands in it by a fifth.

**Works with.** Its Empower, its two shapes and its Master mark; Seeded; Reap What Is Sown and Drawn From the Wood, which use up the bed; The Garden Holds You and Root Walk; and the charged blade, which it charges with Life when it lands.

**Example.** Put the bed in a doorway and fight at its edge. Everything that comes through is Seeded and slowed.

## life.base.shape
**What it does.** Unlocks two shapes for Briar Patch. You pick one at any rest fire. **Burrs**: no bed. Instead, a spray of hooked burrs covers a fan in front of you. Every enemy hit carries them and drops seeds as it moves, which sprout into tufts of briar. Your blows on an enemy with burrs shake burrs onto the enemies next to it. The enemy plants your garden for you, the way burdock burrs ride on animals. **Seed Pods**: no bed. Three pods are thrown, swell up, and burst a second later in a spray of seeds, each leaving a small bush.

**When it happens.** Whenever you cast Briar Patch, using the shape you picked. If you have not bought this, you cast the bed.

**The numbers.** 3 Shards. Burrs: a cone 4 metres long and 90 degrees wide, dealing 4 damage, 5 stagger and Seeded. An enemy with burrs carries them for 6 seconds and drops a tuft every 1.5 seconds. Each tuft is 0.8 metres across, lasts 4 seconds, and deals 4 damage and Seeded every half second. Tufts count as things you grew, up to six. Your weapon blows on an enemy with burrs give Seeded and burrs to every enemy within 2 metres, at most twice a second. Seed Pods: three pods land under the nearest enemies within 6 metres and burst 1 second later in a 1.5-metre circle, for 12 damage, 10 stagger and two Seeded. Each one that hits leaves a bush 1 metre across for 6 seconds, which deals 4 damage and Seeded every half second. Bushes count as things you grew, up to three.

**Works with.** Burrs: Root Walk (places to come up wherever the enemies went), and Reap What Is Sown on a room full of tufts. Seed Pods: Fallen Seeds' bushes, Drawn From the Wood.

**Not built yet.** Burrs should drop a seed for every 1.5 metres the enemy moves (for now, every 1.5 seconds, moving or not). Seed Pods should be thrown in a fan, each leaving its bush where it bursts, whether it hits or not (for now, they land under the nearest enemies, with a bush under each one they hit).

**Example.** Cover a pack in burrs and back away. They bring your garden to you.

## life.base.master
**What it does.** Cast it on something you grew, within a metre of it, and the bed grows into a thicket around it, bigger and longer-lasting. Whatever stands in it as it grows is rooted in place.

**When it happens.** For now, when you cast it within 4 metres of something you grew. Briar Patch must be in a spell slot.

**The numbers.** 3 Shards. A thicket 4 metres across grows where the bed lands and lasts 10 seconds. Every half second, it deals 4 damage and Seeded. It counts as something you grew, up to two. Every enemy within 2 metres of it as it grows is rooted for 1 second: it can't walk, but it can still hit whatever comes within reach.

**Works with.** Fallen Seeds, Sapling and the bed itself, for something to cast it on; Ripening on what the thicket roots; Wildwood.

**Not built yet.** The thicket should grow around the thing you cast it on; for now, it grows where the bed lands, as long as you stand near something you grew.

**Example.** Two beds in a row: the second one grows into a thicket and roots the soldiers in it.

## life.coat
**What it does.** Thorns grow on you. An enemy that hits you, or whose blow you block, is cut and Seeded.

**When it happens.** Each blow that lands on you, and each blow you block with a normal guard, at most twice a second for each.

**The numbers.** 3 Shards. A burst within 1.2 metres of the attacker deals 8 damage and one stack of Seeded. For now, any attacker is cut, even one hitting you from far away: the thorns land where the blow came from.

**Works with.** Seeded (your next blows on the enemy heal you), Bitter Rind, which it comes after on the chart, Sapling, which comes after it, Drawn From the Wood's Master mark (a blow you take).

**Example.** Block a soldier's string of three blows. It is cut and Seeded as it comes in, and your answer heals you.

## life.sapling
**What it does.** Sapling, the counter of the Life tree. For a while after you cast it, each perfect guard or perfect dodge plants a sapling where the attacker stands. Each sapling lashes nearby enemies again and again, Seeding them. They count as things you grew.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then guard or dodge perfectly: press the guard button just before a blow lands, or dodge just before it.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 7 seconds.) Each cast gives three saplings, used up by your next perfect guards and perfect dodges. Each sapling reaches 3.5 metres and stays for 12 seconds. Every 1.5 seconds, it hits everything in reach for 6 damage, 8 stagger and one stack of Seeded. Up to three can stand at once from guards, and three from dodges; one more replaces the oldest. On a dodge, it is planted at the enemy you face.

**Works with.** Bitter Rind and Briar Coat (they work on the same guard), Briar Coat, which it comes after on the chart, Reap What Is Sown and Drawn From the Wood (they use up the saplings), Root Walk, Wildwood, Seeded.

**Not built yet.** The window should last eight seconds after you cast it (for now, the three saplings wait until used). There should be three at most in total between guards and dodges. The Empower's fourth sapling. Each sapling should only lash the nearest enemy (for now, it hits everything within reach).

**Example.** Cast it as a pack closes in and perfect-guard three blows. Three saplings grow among them, Seeding everything that comes near.

## life.sapling.shape
**What it does.** Unlocks two shapes for Sapling. You pick one at any rest fire. **Strangler**: instead of a sapling, a plant climbs up the attacker, the way a strangler fig climbs a tree. The attacker is rooted and gets lots of Seeded, and nothing is left standing afterwards. **Dodder**: the seed grows on the attacker itself, as pale threads that drink from it. Each of your blows on it heals you more, and its balance breaks sooner.

**When it happens.** For now, on every perfect guard and perfect dodge while you have the shape picked, along with the saplings. If you have not bought this, only the saplings are planted.

**The numbers.** 3 Shards. Strangler: the attacker is rooted for 1 second (it can't walk, but it can still hit whatever comes within reach) and gets two stacks of Seeded. Dodder: for 6 seconds, each of your weapon blows on it heals you 3, and it takes a quarter more stagger.

**Works with.** Strangler: Ripening on the rooted enemy. Dodder: Seeded's own healing; Bitter Rind and Briar Coat on the same attacker.

**Not built yet.** Each should replace the sapling, and only work during the spell's window. For now, they work on every perfect guard and dodge while picked, and the saplings are still planted.

**Example.** Use Dodder on a brute whose swing you perfectly guarded. You get six seconds of cuts that each heal 3 more and break its balance a quarter sooner.

## life.sapling.master
**What it does.** Plant all three saplings within three seconds, and the third one comes up as a full Elder Tree for six seconds.

**When it happens.** For now, on a perfect guard, at most once every 15 seconds. Sapling must be in a spell slot.

**The numbers.** 3 Shards. A small Elder Tree rises at the attacker. Its roots hit within 3 metres 0.3 seconds later, then every 3 seconds for 6 seconds, dealing 16 damage, 45 stagger and Seeded each time.

**Works with.** Elder Tree; Bitter Rind on the same perfect guard; Ripening on what the roots Seed.

**Not built yet.** It should need all three saplings planted within three seconds. For now, any perfect guard works, once every fifteen seconds.

**Example.** Perfect-guard a soldier's swing. A tree rises under it, and the roots break up the crowd behind it.

## life.spring
**What it does.** While your surge is lit, press the surge button again to give half your health to the ground. A group of trees grows up through every Seeded enemy nearby, hitting it and holding it. Hit the enemies the forest holds, and you get back the health you gave.

**When it happens.** Light the surge (Regrowth, or the other element's surge if you have that one picked), then press the surge button again while it lasts. Once each time you light the surge. If nothing nearby is Seeded, it tells you so, and you can press again.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. It reaches up to eight Seeded enemies within 12 metres. Each takes 30 damage and 60 stagger and is held for 4 seconds (less for elites and gods). Half your current health is taken and turns grey. Each blow on a held enemy wins back half its damage as health, but never more than you gave.

**Works with.** Seeded, Regrowth (4 health a second while it lasts), Briar Patch's Burrs and Seed Pods shapes to Seed a crowd, Root and Branch.

**Example.** Seed a pack with Briar Patch, light Regrowth, and press again. The whole pack is held in place while you cut your health back out of them.

## life.sharedroots
**What it does.** Enemies with your seeds in them take part of your damage for you. Thin roots run from your heels to each Seeded enemy near you. But your seeds no longer heal you when you hit.

**When it happens.** Every blow that hurts you, while you carry Life, if a Seeded enemy is within 8 metres.

**The numbers.** Keystone, 6 Shards. A third of the damage from each blow is taken off you and dealt to the Seeded enemies within 8 metres instead, split evenly between them (up to eight of them). If no Seeded enemy is nearby, you take all the damage. Blows on Seeded enemies no longer heal you.

**Works with.** Briar Coat, which it comes after on the chart, and which Seeds the enemies that hit you. Briar Patch and Seeded, to plant seeds in a crowd.

**Example.** Seed a whole pack with Briar Patch, then wade in. Every blow you take is split between them.

## life.inseason
**What it does.** Your seeds ripen if you leave them alone. A ripe seed flowers and stops fading. When a ripe seed bursts, it hits three times as hard and leaves a thornbush behind. A seed that bursts before it is ripe only hits half as hard.

**When it happens.** A seed ripens once it has stayed in an enemy for six seconds with no new seed planted on it, while you carry Life. A seed bursts when you set it off (Ripening, Reap What Is Sown), when the enemy dies, or in a reaction.

**The numbers.** Keystone, 6 Shards. A seed is ripe after 6 seconds alone. A ripe seed doesn't fade until it bursts. A ripe burst deals three times the damage, and a thornbush grows there. The bush lasts 8 seconds and deals 4 damage and a seed every half second to whatever stands in it (up to four bushes). An unripe burst deals half the damage.

**Works with.** Root and Branch, which it comes after on the chart. Ripening, Reap What Is Sown, Fallen Seeds, and the reactions that use Seeded.

**Example.** Plant seeds in three enemies, then fight a fourth for six seconds. Now reap: three ripe bursts and three thornbushes.

## life.shade
**What it does.** While you stand near anything you've grown, you heal slowly and your stamina comes back faster.

**When it happens.** Every second, while something you grew stands within 4 metres of you and you carry Life.

**The numbers.** Major, 3 Shards. 2 health and 5 stamina every second.

**Works with.** Root and Branch, which it comes after on the chart. Briar Patch, Elder Tree, Sapling and Fallen Seeds, which grow things to stand next to.

**Example.** Fight next to your Elder Tree. Each second, you heal a little, and your stamina refills for the next dodge.
