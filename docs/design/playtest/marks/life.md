# Life: detailed explanations

The long explanation the chart shows for each high-level mark of the Life tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Life): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Grown" means anything of yours that grows from the
ground: Briar Patch's bed, a thornbush, a sapling, a thicket, a tuft of briar.

## life.seed
**What it does.** Every blow of your weapon that lands plants a seed in the enemy: one stack of Seeded, up to three. Each further weapon blow on a Seeded enemy mends you a little, more for each stack. When a Seeded enemy dies, it bursts into thorns that strike everyone near it.

**When it happens.** Always on once learned: every weapon blow that lands, light or heavy, while Life is your first element (or all in). With Life second, Seeded comes from your spells, the charged blade and the R2 strike instead.

**The numbers.** 2 Shards. One stack a blow, three at most, lasting 8 seconds. Each weapon blow on a Seeded enemy mends 1 health for each stack, 3 at full depth. Its death throws 12 damage a stack at everyone within 3 metres, 36 at three.

**Works with.** Ripening, Fallen Seeds, Reap What Is Sown (it bursts every seed it reaches), Root and Branch, Regrowth (one more stack a blow), Briar Patch, and Rot Roots when Poison meets the seed.

**Example.** With the Twin Knives, three quick cuts seed a soldier three deep; every cut after that gives back 3 health while you keep pressing it.

## life.fallen
**What it does.** Where a Seeded enemy dies, a thornbush grows and stands for a while. It lashes and seeds whatever stands in it. The bush is something grown.

**When it happens.** Whenever an enemy carrying Seeded dies, whatever killed it.

**The numbers.** 3 Shards. A bush 2 metres in radius for 8 seconds. It bites every half second for 4 damage and one stack of Seeded. Four stand at once at most; a fifth takes the place of the oldest.

**Works with.** Seeded (its death burst comes too), Reap What Is Sown and Elder Tree, which grow from it, Root and Branch above them, Drawn From the Wood (the bushes are its price), Root Walk (a place to come up), Wildwood.

**Example.** Kill a seeded soldier in a doorway: the bush where it fell seeds everyone who follows it through.

## life.reap
**What it does.** Reap What Is Sown, the detonator of the tree. You call in the harvest: everything you have grown bursts where it stands, the seeds in the enemies near you burst now and are spent, and you mend for each burst.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 12 seconds after a cast, less with weapon play (its Empower: 10). Each grown thing, wherever it stands, bursts 3 metres round for 16 damage and 25 stagger and is gone, and mends you 5. A call 6 metres round you lands 0.2 seconds after the cast: 16 damage and 25 stagger; every Seeded enemy it strikes has its seed burst at once, 11 damage and 5 stagger a stack to everyone within 3 metres, and mends you 5.

**Works with.** Fallen Seeds and Sapling (the things it bursts), Briar Patch, Seeded and Ripening, Wood Grows Back (the garden comes again), Wildwood (more to burst), and the charged blade: it charges your weapon with Life when it lands.

**Not built yet.** Every Seeded enemy in the fight bursting, not only those within the call's 6 metres; the Empower's 7 mending a burst (for now 5).

**Example.** Three saplings stand round you and a pack closes in: reap, and four bursts go off at once while you mend 20.

## life.reap.shape
**What it does.** Opens two shapes of Reap What Is Sown, chosen at any rest fire. **Golden Sickle**, the druids' sickle: only the Seeded within eight metres are reaped, each cut once by one sweep of gold, and the garden is kept standing. **Second Sowing**: everything bursts as before, and each burst sows a new seed in all it reaches, so the loop keeps going.

**When it happens.** Whenever you cast Reap What Is Sown, as the shape it wears. Before this is bought, the plain call is cast.

**The numbers.** 3 Shards. Golden Sickle: one sweep 8 metres all round you, 16 damage and 25 stagger, and each Seeded enemy it cuts has its seed burst as the Learn's. Second Sowing: each seed set off (by Reap, Ripening or anything else) lays one Seeded on every enemy within 3 metres of it.

**Works with.** Ripening (its bursts sow too, while Second Sowing is worn); Fallen Seeds and Sapling for the garden Golden Sickle should keep.

**Not built yet.** Golden Sickle keeping the garden standing: for now the grown things still burst. Second Sowing's grown things sowing as they burst: for now only the seeds set off sow.

**Example.** Second Sowing in a crowd: reap once to seed the whole pack again, cut, reap again.

## life.reap.master
**What it does.** Cast it within half a second of a Seeded enemy dying, and every burst counts twice.

**When it happens.** For now, for half a second after any Seeded enemy dies. Reap What Is Sown must be in a slot.

**The numbers.** 3 Shards. For half a second every seed set off bursts twice as hard (bursts of seeds 100% stronger).

**Works with.** Seeded's own death burst (kill one, then reap the rest); Ripening's heavy blow in the same half second.

**Not built yet.** The garden's bursts and the mending counted twice too, and the window tied to the cast: for now any seed set off in that half second is doubled.

**Example.** Finish the seeded soldier and reap as it falls: the seeds in its friends burst for twice their thorns.

## life.tree
**What it does.** Elder Tree, the larger spell of the tree. A tree grows where you stand, shoot, sapling and oak, in three seconds. Grown, for fifteen seconds a ring lies under its crown: enemies inside stay Seeded, you mend, and every few seconds a root comes up under the enemies inside.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, 40 mana (its Empower: 32, and it grows in 2 seconds). The ring is 3 metres in radius and lies 18 seconds from the cast, biting once a second for 1 damage and Seeded. The roots first strike 3 seconds after planting, then every 3 seconds for 15 seconds: 16 damage, 45 stagger and one Seeded. The cast holds 15 seconds of shade: each second in a fight spends one and mends you 3. The tree is not one of your grown things: Reap What Is Sown and Drawn From the Wood leave it be.

**Works with.** Seeded, Ripening (heavy blows on what it seeded), Fallen Seeds, which it grows from, Sapling's Master (a small tree of the same kind), and the charged blade: its first strike charges your weapon with Life.

**Not built yet.** The tree's 40 health while it grows and the enemies turning on it; each root under one enemy, a staggered one first (for now the roots strike everything in the ring); the mending only inside the ring while the tree stands (for now 3 a second for 15 seconds of fighting, anywhere).

**Example.** Plant it where a pack must come to you and fight at its foot: every three seconds the roots stagger and seed them while you mend.

## life.tree.shape
**What it does.** Opens two shapes of Elder Tree, chosen at any rest fire. **Walking Tree**: the tree pulls up its roots and follows you at walking pace, with a smaller ring; the stag grown through with forest. **Grown at Once**: no growing; it comes up full in an instant and stands only six seconds, so setup becomes burst.

**When it happens.** Whenever you cast Elder Tree, as the shape it wears. Before this is bought, the growing tree is cast.

**The numbers.** 3 Shards. Walking Tree: the roots strike 2 metres round it. Grown at Once: the roots strike at once and then every 3 seconds for 6 seconds, 3 metres round: 16 damage, 45 stagger and Seeded.

**Works with.** Root Walk (go where the tree is); The Garden Holds You and Wildwood near the ring.

**Not built yet.** Walking Tree following you: for now it stands where it was planted, with the smaller ring. With either shape the shade ring and the mending keep the Learn's times.

**Example.** Grown at Once on a staggered brute: the first root lands before it stands up.

## life.tree.master
**What it does.** Plant it on something you grew, and it grows at once.

**When it happens.** For now, when you cast it within 4 metres of something you grew. Elder Tree must be in a slot.

**The numbers.** 3 Shards. For now a root bursts up at once 3 metres round you, 16 damage, 45 stagger and Seeded, before the tree's own first root.

**Works with.** Fallen Seeds and Sapling for something to plant on; Briar Patch's bed; Wildwood.

**Not built yet.** The whole tree grown at once on the grown thing: for now the first root comes at once and the tree still grows.

**Example.** Cast Briar Patch, step into the bed and plant the tree on it: the roots answer at once.

## life.lift
**What it does.** Root and Branch, Life's reaction lift: every reaction with Seeded in it mends you and sows a seed in everything it reaches.

**When it happens.** Whenever Seeded reacts on an enemy with the other affliction it carries (Rot Roots with Poison, and the rest).

**The numbers.** 3 Shards. Each such reaction mends you 4 and lays one Seeded on every enemy within 3 metres of where it happened.

**Works with.** Carrying a second element: every reaction of Life's ten; Seeded; Fallen Seeds, which it grows from; Wood Grows Back above it.

**Not built yet.** The seed sown in exactly what the reaction struck: for now everything within 3 metres.

**Example.** Life and Poison on a huddle: Rot Roots goes off on one, you mend 4, and its neighbours are seeded for the next.

## life.devotion
**What it does.** Wood Grows Back, Life's Devotion (the land's rule, "wood forgets"): all in on Life only. Whatever you grow grows back where it stood after it is spent, burst or cut down, and every spell you cast sows a sapling where it lands. The garden never runs out, so Reap What Is Sown and Root Walk can be spent freely, and every fight becomes a forest.

**When it happens.** Only while Life is carried twice (all in). A sapling is sown each time a spell strikes.

**The numbers.** 6 Shards. A sapling where a spell strikes, at most one every 2 seconds: it reaches 3.5 metres, stands 12 seconds, and every 1.5 seconds strikes for 6 damage, 8 stagger and Seeded; four stand at once at most. Buying this also gives you the deep strike, Briar Root. Use it with L2 and R2 while you carry this element alone or twice: the cutting grows into a briar round the enemy's feet and roots it for a second.

**Works with.** Every spell of the tree; Reap What Is Sown and Drawn From the Wood, which spend the garden; Root Walk, which goes to it.

**Not built yet.** Whatever is spent growing back where it stood 6 seconds later: for now only the saplings each spell sows. Briar Root's briar staying as something grown.

**Example.** Briar Patch, Reap What Is Sown, Briar Patch again: every cast leaves a sapling, so the reaping always has something to burst.

## life.ripen
**What it does.** A heavy blow on a Seeded enemy bursts its seed at once, without waiting for it to die. The thorns strike everyone near it, and you mend.

**When it happens.** Every heavy blow or finisher that lands on a Seeded enemy, at most once a second.

**The numbers.** 3 Shards. The burst is 3 metres in radius: 11 damage and 5 stagger for each stack, 33 and 15 at three. The seed is spent, so it will not burst again at death. You mend 10.

**Works with.** Seeded, Drawn From the Wood and The Garden Holds You, which grow from it, Bitter Rind (a rooted enemy is an easy heavy blow), Elder Tree (it seeds for you), Reap What Is Sown's Second Sowing.

**Example.** Two light blows and a heavy on a soldier in a crowd: the heavy bursts its seed through the pack and gives you 10.

## life.harvest
**What it does.** Drawn From the Wood. You kneel, and every grown thing of yours withers into you, bursting in thorns as it goes. In return you get a window of green blood: every weapon blow is critical and mends a fifth of its damage. The garden you grew is the price, not your health.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). It wants something grown near you.

**The numbers.** 4 Shards, no mana: it waits 12 seconds after a cast, less with weapon play (its Empower: 10). Each grown thing, wherever it stands, bursts 2 metres round for 12 damage and Seeded and is gone. For now, cast within 4 metres of something you grew, the window is 6 seconds: every weapon blow critical, and 20% of its damage back as health.

**Works with.** Fallen Seeds, Sapling, Briar Patch and Root Walk for the garden it spends; Wood Grows Back to have it again; Ripening, which it grows from; Seeded mending on top.

**Not built yet.** The window as a second and a half for each grown thing withered, ten seconds at most, and only those within 12 metres; the half-second kneel; the Empower's 16-damage thorns (for now 12).

**Example.** Three bushes stand round a brute: draw them in, and cut it with six seconds of critical, mending blows.

## life.harvest.shape
**What it does.** Opens two shapes of Drawn From the Wood, chosen at any rest fire. **Blood in the Furrow**: the green blood soaks into the ground where you knelt, a ring that seeds everything in it each second; fight in a place, not round yourself. **Feed the Ground**: the one place Life pays in blood; nothing withers, you give a quarter of your health to your garden, and for a while everything you have grown strikes twice as hard and twice as often.

**When it happens.** Whenever you cast Drawn From the Wood, as the shape it wears. Before this is bought, the withering is cast.

**The numbers.** 3 Shards. Blood in the Furrow: a ring 8 metres across where you knelt for 6 seconds, biting once a second for 1 damage and Seeded. Feed the Ground: it costs 25 health, left grey to be won back by blows; with 25 health or less it cannot be cast. For 8 seconds what you grow strikes twice as hard.

**Works with.** Blood in the Furrow: Bitter Rind and Briar Coat, which hold the fight in place. Feed the Ground: Fallen Seeds, Sapling and Wildwood for a garden worth feeding; the Master for a price paid in grey.

**Not built yet.** Blood in the Furrow's window held only while you stand in the ring. Feed the Ground taking a quarter of your present health (for now 25), sparing the garden (for now it still withers as the Learn's does) and doubling what already stands and how often it strikes (for now only what you grow in the 8 seconds strikes harder).

**Example.** Blood in the Furrow at a choke point: wait in the ring and let them come to you, seeded on every step.

## life.harvest.master
**What it does.** Cast it within a second of being struck, and the price comes from the grey health that blow left: nothing withers, each grown thing still gives its time, and Feed the Ground takes no real health.

**When it happens.** For now, for one second after any blow lands on you. Drawn From the Wood must be in a slot.

**The numbers.** 3 Shards. For that second, prices in health cost nothing: Feed the Ground's 25 is not taken.

**Works with.** Feed the Ground; Briar Coat (the blow that lands cuts the attacker too); Seeded's mending to win the grey back.

**Not built yet.** The garden kept on such a cast while each grown thing still gives its time, and the price drawn from that blow's grey: for now only the health price is waived.

**Example.** Take a soldier's blow, then Feed the Ground at once: the garden strikes twice as hard and you pay nothing.

## life.garden
**What it does.** The Garden Holds You. Near anything you grew, you cannot be thrown, pulled or knocked down, and plain blows do not make you flinch. Roots run from the nearest plant to your heels. Fight in the garden.

**When it happens.** Always on once learned, while something you grew stands within 4 metres of you.

**The numbers.** 3 Shards. For now, wounds take 20% less while something you grew stands within 4 metres.

**Works with.** Briar Patch and Fallen Seeds for a garden to stand in, Root Walk and Wildwood, which grow from it, Ripening.

**Not built yet.** The footing itself (never thrown, pulled or knocked down, no flinch from plain blows) and the roots drawn to your heels: for now it is less damage taken in the garden.

**Example.** Stand in your own bed of briars against a brute: its blows land for a fifth less while you trade.

## life.walk
**What it does.** Root Walk, the movement spell of the tree. You sink into the ground in a swirl of leaves and come up at a grown thing of yours, or ahead if there is none. Nothing can touch you under the ground. A sapling sprouts where you went down, and roots burst round you as you come up.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 8 seconds after a cast, less with weapon play (its Empower: 6, and two Seeded). For now it is a step: up to 8 metres toward and past the enemy you face, 6 metres ahead with none, untouchable for it; what it passes takes 10 damage, 15 stagger and Seeded, and your next heavy blow within half a second is critical. The sapling where you went down reaches 3.5 metres, stands 6 seconds and every 1.5 seconds strikes for 6 damage, 8 stagger and Seeded.

**Works with.** Fallen Seeds, Sapling and Briar Patch for places to come up; The Garden Holds You, which it grows from; Wood Grows Back (spend the garden, it returns).

**Not built yet.** Going under the ground to a grown thing within 12 metres, the one nearest where you aim, and coming up half a second later; the roots bursting round you as you come up rather than along the way.

**Example.** Surrounded, walk out through the soldier in front: the sapling you leave keeps the rest busy.

## life.walk.shape
**What it does.** Opens two shapes of Root Walk, chosen at any rest fire. **Up Through Them**: you go to a Seeded enemy instead, and come up under it, lifting it on roots. **Changeling**: you change places with the grown thing, so it stands where you stood, in the path of the blow aimed at you.

**When it happens.** Whenever you cast Root Walk, as the shape it wears. Before this is bought, the plain walk is cast.

**The numbers.** 3 Shards. Up Through Them: a Seeded enemy the walk strikes is thrown up for 0.6 seconds (elites and gods for less). Changeling: a thornbush 2 metres round rises where you stood for 8 seconds, biting every half second for 4 and Seeded, and the enemies within 2.5 metres lose track of you for 1.5 seconds.

**Works with.** Up Through Them: Seeded, the charged blade and every mark that answers a lifted enemy. Changeling: Fallen Seeds' bushes, Reap What Is Sown to burst what you left behind.

**Not built yet.** Up Through Them going to the nearest Seeded enemy rather than the one faced. Changeling changing places with a grown thing: for now a bush rises where you stood and those near it strike at it.

**Example.** Changeling out of a pincer: both soldiers swing at the bush you leave.

## life.walk.master
**What it does.** Started in the instant a blow would land on you, it counts as a perfect dodge, and the sapling left behind is a full thornbush.

**When it happens.** For now, every cast. Root Walk must be in a slot.

**The numbers.** 3 Shards. Each cast gives a perfect dodge's mana (12) and takes 1.5 seconds off every running spell and strike clock. A thornbush 2 metres round grows where you went down for 8 seconds, biting every half second for 4 and Seeded.

**Works with.** Every mark that answers mana; Reap What Is Sown (one more thing to burst); The Garden Holds You.

**Not built yet.** The timing (started as a blow would land), and the thornbush in place of the sapling: for now it grows beside it.

**Example.** Walk out of a brute's swing, then walk again sooner: each walk leaves a bush and a sapling behind.

## life.wildwood
**What it does.** What you grow stands three times as long, and you may keep three more of each kind. The price: far from your garden, your weapon is a fifth weaker.

**When it happens.** Always on once learned. The price holds whenever nothing you have grown stands within 7 metres of you.

**The numbers.** 6 Shards. Grown things stand three times as long (thornbushes 24 seconds, saplings 36). Three more of each kind may stand at once. Weapon damage down 20% with nothing grown within 7 metres.

**Works with.** Fallen Seeds, Sapling, Briar Patch, The Garden Holds You, which it grows from, Reap What Is Sown and Drawn From the Wood (more to spend).

**Not built yet.** Grown things standing for ever: for now three times as long.

**Example.** Plant a ring of saplings at the top of a stair and hold it: the garden lasts the whole fight, and you never leave it.

## life.rind
**What it does.** A plain guard seeds the attacker. A perfect guard roots it where it stands for a moment.

**When it happens.** Each blow you block, and each perfect guard.

**The numbers.** 3 Shards. One stack of Seeded on the attacker for each blocked blow. A perfect guard holds it still for 1 second (elites and gods for less; a few great forms cannot be held).

**Works with.** Briar Coat, which grows from it, Sapling (perfect guards plant), Seeded, Ripening (a held enemy is an easy heavy blow).

**Example.** Perfect-guard a brute's swing: it stands rooted for a second, long enough for a heavy blow that bursts its seed.

## life.base
**What it does.** Briar Patch, Life's own spell, comes with the element and is never bought. A bed of thorned vines grows ahead of you and stands, lashing, seeding and slowing whatever stands in it. The bed counts as grown.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Life, it is always yours to put in a slot.

**The numbers.** Given with Life. No mana: it waits 7 seconds after a cast, less with weapon play; its Empower makes that 5.5. The bed is 2.5 metres across, 3 metres ahead, and stands 6 seconds: it lashes twice a second for 4 damage and Seeded, and slows what stands in it by a fifth.

**Works with.** Its Empower, its two shapes and its Master; Seeded; Reap What Is Sown and Drawn From the Wood, which spend the bed; The Garden Holds You and Root Walk; the charged blade, which its landing charges with Life.

**Not built yet.** The Empower's bed 3.3 metres across: the base spell is cast by Life's kit, which takes only the Empower's shorter wait.

**Example.** Lay the bed in a doorway and fight on its edge: everything that comes through is seeded and slow.

## life.base.shape
**What it does.** Opens two shapes of Briar Patch, chosen at any rest fire. **Burrs**: no bed; a spray of hooked burrs over a fan ahead, and every enemy hit carries them, dropping seeds that sprout tufts of briar as it goes; your blows on a burred enemy shake burrs onto its neighbours. The enemy plants your garden for you, as burdock rides on animals. **Seed Pods**: no bed; three pods are thrown, swell, and burst a second later in a spray of seeds, each leaving a small bush.

**When it happens.** Whenever you cast Briar Patch, as the shape it wears. Before this is bought, the bed is cast.

**The numbers.** 3 Shards. Burrs: a cone 4 metres long and 90 degrees wide, 4 damage, 5 stagger and Seeded. A burred enemy carries them 6 seconds and drops a tuft every 1.5 seconds: 0.8 metres round, 4 seconds, biting every half second for 4 and Seeded, grown, six at most. Your weapon blows on it seed and burr every enemy within 2 metres, at most twice a second. Seed Pods: three pods under the nearest enemies within 6 metres, bursting 1 second later 1.5 metres round: 12 damage, 10 stagger and two Seeded; each that strikes leaves a bush 1 metre round for 6 seconds, biting every half second for 4 and Seeded, grown, three at most.

**Works with.** Burrs: Root Walk (a place to come up wherever they went), Reap What Is Sown on a room full of tufts. Seed Pods: Fallen Seeds' bushes, Drawn From the Wood.

**Not built yet.** Burrs dropping a seed for every 1.5 metres the enemy moves (for now every 1.5 seconds, moving or not). Seed Pods thrown in a fan, each leaving its bush where it bursts whether it strikes or not (for now under the nearest enemies, a bush under each it strikes).

**Example.** Burr a pack and back away: they bring your garden to you.

## life.base.master
**What it does.** Cast it on something you grew, within a metre of it, and the bed grows into a thicket round it, larger and longer, and whatever stands in it as it grows is rooted.

**When it happens.** For now, when you cast it within 4 metres of something you grew. Briar Patch must be in a slot.

**The numbers.** 3 Shards. A thicket 4 metres across where the bed lands, for 10 seconds, biting every half second for 4 and Seeded; it is grown, two at most. Every enemy within 2 metres of it as it grows is rooted for 1 second: it cannot walk, but it still strikes what comes into reach.

**Works with.** Fallen Seeds, Sapling and the bed itself for something to cast on; Ripening on what the thicket roots; Wildwood.

**Not built yet.** The thicket grown round the grown thing it was cast on: for now it grows where the bed lands while you stand near something grown.

**Example.** Two beds in a row: the second grows into a thicket and roots the soldiers in it.

## life.coat
**What it does.** Thorns grow on you. An enemy that strikes you, or whose blow you block, is cut and seeded.

**When it happens.** Each blow that lands on you, and each blow you block with a plain guard, at most twice a second for each.

**The numbers.** 3 Shards. A burst 1.2 metres round the attacker: 8 damage and one stack of Seeded. As built any attacker is cut, even one striking from afar: the thorns land where the blow came from.

**Works with.** Seeded (your next blows on it mend you), Bitter Rind, which it grows from, Sapling, which grows from it, Drawn From the Wood's Master (a blow taken).

**Example.** Block a soldier's chain of three: it is cut and seeded on the way in, and your answer mends you.

## life.sapling
**What it does.** Sapling, the counter of the tree. For a while after the cast, each perfect guard or perfect dodge plants a sapling where the attacker stands. Each lashes the enemies near it again and again, seeding them. They count as grown.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Then guard or dodge perfectly: press the guard button just before a blow lands, or dodge just before it.

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 7). A cast holds three saplings, spent by the next perfect guards and perfect dodges. Each sapling reaches 3.5 metres and stands 12 seconds. Every 1.5 seconds it strikes everything in reach for 6 damage, 8 stagger and one stack of Seeded. Three stand at once from guards and three from dodges; one more takes the place of the oldest. On a dodge it is planted at the enemy you face.

**Works with.** Bitter Rind and Briar Coat (they answer the same guard), Briar Coat, which it grows from, Reap What Is Sown and Drawn From the Wood (they spend the saplings), Root Walk, Wildwood, Seeded.

**Not built yet.** The window as eight seconds after the cast (for now the three wait until spent); three at most between guards and dodges; the Empower's fourth sapling; each lashing only the nearest enemy (for now everything within its reach).

**Example.** Cast it as a pack closes in and perfect-guard three blows: three saplings stand among them, seeding everything that comes.

## life.sapling.shape
**What it does.** Opens two shapes of Sapling, chosen at any rest fire. **Strangler**: each one climbs the attacker instead, as a strangler fig climbs its host; it is rooted and seeded deep, and nothing stands after. **Dodder**: the seed grows on the attacker itself, pale threads that drink from it; each of your blows on it mends you more, and its balance breaks sooner.

**When it happens.** For now, on every perfect guard and perfect dodge while the shape is worn, beside the saplings. Before this is bought, the saplings alone are planted.

**The numbers.** 3 Shards. Strangler: the attacker is rooted for 1 second (it cannot walk, but still strikes what comes into reach) and Seeded two deep. Dodder: for 6 seconds each of your weapon blows on it mends you 3, and it takes a quarter more stagger.

**Works with.** Strangler: Ripening on the rooted enemy. Dodder: Seeded's own mending; Bitter Rind and Briar Coat on the same attacker.

**Not built yet.** Each in place of the sapling, and only within the cast's window: for now they answer every perfect guard and dodge while worn, and the saplings are still planted.

**Example.** Dodder on a brute's perfect-guarded swing: six seconds of cuts that mend 3 more each and break it a quarter sooner.

## life.sapling.master
**What it does.** Plant all three within three seconds, and the third comes up as a full Elder Tree for six seconds.

**When it happens.** For now, on a perfect guard, at most once every 15 seconds. Sapling must be in a slot.

**The numbers.** 3 Shards. A small Elder Tree rises at the attacker: its roots strike 3 metres round 0.3 seconds after, then every 3 seconds for 6 seconds, 16 damage, 45 stagger and Seeded each time.

**Works with.** Elder Tree; Bitter Rind on the same perfect guard; Ripening on what the roots seed.

**Not built yet.** The three planted within three seconds as its condition: for now any perfect guard, once in fifteen seconds.

**Example.** Perfect-guard a soldier's swing: a tree rises under it and the roots break the crowd behind.

## life.spring
**What it does.** While your surge burns, press the surge button again and give half your health to the ground. A stand of trees comes up through every Seeded enemy near, strikes it and holds it. Strike what the forest holds, and the health you gave comes back.

**When it happens.** Light the surge (Regrowth, or the other element's surge if you wear that), then press the surge button again while it lasts. Once each time the surge is lit. With nothing Seeded near, it says so and you may press again.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. Up to eight Seeded enemies within 12 metres. Each takes 30 damage and 60 stagger and is held for 4 seconds (elites and gods for less). Half your present health is taken and turns grey. Each blow on a held enemy wins back half its damage as health, never more than you gave.

**Works with.** Seeded, Regrowth (4 health a second while it lasts), Briar Patch's Burrs and Seed Pods to seed a crowd, Root and Branch.

**Example.** Seed a pack with Briar Patch, light Regrowth, press again: the whole pack stands held while you cut your health back out of them.
