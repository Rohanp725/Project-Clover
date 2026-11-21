# Plasma: detailed explanations

The long explanation the chart shows for every mark on Plasma, the duo branch Fire and Lightning share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Fire and Lightning") and drawn in the heart of the chart, the ring's middle, while
Fire and Lightning are both carried (in the playtest, whenever you pick Lightning beside Fire or Fire beside Lightning). Its
reaction is Flashover; its Shape mark lets you choose what Flashover does.

## plasma.shape
**What it does.** Opens Flashover's two shapes. At a rest fire you choose which one your Flashover wears, and from then on it runs instead of the plain Flashover. **Corona**: nothing is spent; violet flame crawls over the enemy and throws small arcs at its neighbours. **Fulgurite**: the crack goes down instead of out; the ground under the enemy fuses into a scorched glassy patch that burns and shocks whoever stands in it.

**When it happens.** Every time Flashover fires (Static landing on a burning enemy, or Burn on a charged one) while Fire and Lightning are both carried. Before this mark is bought, or with only one of the two carried, the plain Flashover runs.

**The numbers.** Mix, 3 Shards. Corona: lasts 4 seconds; every 0.7 seconds an arc leaps from the enemy to the nearest other enemy within 5 metres for 4 damage, a Burn and a Static (about five arcs). Fulgurite: a patch 2.5 metres across for 4 seconds that bites once a second for 4 damage, a Burn and a Static. What either lays starts no reaction of its own. Flashover still rests 4 seconds on each enemy.

**Works with.** Ionised Air (an arc leaps on through three more whatever the shape), Twin Break, Meltdown (Corona and Fulgurite both keep Burn and Static on everyone near), Plasma.

**Example.** A pack pressing in: wear Corona, and one burning soldier you charge keeps arcing into the two beside it for four seconds. Fighting one brute in a doorway: wear Fulgurite and fight on the patch it leaves.

**Not built yet.** Corona's arcs leap from the enemy rather than drawing other discharges into its flame, and Fulgurite's patch is drawn as burning and crackling ground until the glassy look is made.

## plasma.twin
**What it does.** A perfect guard sets off Burn and Static together on the one who struck. Spent together they strike half again as hard, and the blow is heavy, so it stops the enemy where it stands. If it carries only one of the two, that one is set off alone.

**When it happens.** A perfect guard (press the guard button just before a blow lands) against an enemy carrying Burn, Static, or both.

**The numbers.** Mix, 3 Shards. Together: 10 damage a Burn stack and 7 a Static stack, times 1.5, with 5 stagger a stack: 127.5 damage and 50 stagger at five and five. Alone, in 3 metres: Burn 10 plus 15 a stack, Static 10.5 a stack, 5 stagger a stack.

**Works with.** Kiln Stance, Stored Charge, Meltdown, Ball Lightning (it grows from this or Ionised Air). Breath of the Lioness spends the burn first, so the two clash.

**Example.** A burning, charged brute swings: guard it perfectly and it takes 127 and stops dead.

## plasma.ionised
**What it does.** Whenever Flashover fires, an arc leaps on from the burning enemy through three more, each struck and charged.

**When it happens.** Each time Flashover fires on an enemy, plain or in either shape, while Fire and Lightning are both carried.

**The numbers.** Mix, 3 Shards. 10 damage, 8 stagger and a Static to each of up to three more enemies, leaping up to 5 metres from one to the next. Flashover itself rests 4 seconds on each enemy.

**Works with.** Flashover's Shape mark (Corona or Fulgurite), Chain Reaction, which grows from it, and Static or Thunderclap to bring the charge.

**Example.** Burn a soldier in a crowd and Thunderclap it: Flashover goes off, and the arc runs on into three of its friends, leaving each charged for the next one.

## plasma.chain
**What it does.** An enemy that dies carrying both Burn and Static bursts, and passes both on to everyone near at the depth it carried them.

**When it happens.** The moment an enemy carrying both dies.

**The numbers.** Mix, 3 Shards. The burst is 4 metres in radius: 18 damage, and as many Burn stacks as the dead one had. Everyone within 4 metres also takes as many Static stacks as it had. At most a few times a second.

**Works with.** Ionised Air, which it grows from, Catching Fire, Meltdown.

**Example.** Kill one soldier at five and five, and the three round it start at five and five too, one blow from Meltdown.

## plasma.meltdown
**What it does.** An enemy near you carrying five Burn and five Static begins to shake with light. A few moments later it bursts, far harder than either alone, and the burst spends both. Nothing stops it, and it does not harm you.

**When it happens.** Always on once learned, while Fire and Lightning are both carried. It looks a few times a second for an enemy within 14 metres at full depth in both; one enemy melts at a time.

**The numbers.** Mix, keystone, 5 Shards. It shakes for 3 seconds, then bursts: 45 damage and 60 stagger to it as a heavy blow, and half of each to everything else within 5 metres. The same enemy cannot melt again for 10 seconds.

**Works with.** Twin Break or Chain Reaction (it grows from either), Ball Lightning, Corona.

**Example.** Fill a brute with both and roll away: three seconds later it goes up and staggers the pack around it.

## plasma.ball
**What it does.** You loose a slow ball of plasma, violet at its heart with real flame on its skin, that drifts after the nearest enemy. Strike it with a swing to send it fast along the swing and grow it a step, up to three. It bursts on the first enemy it meets with Burn and Static, harder for each strike. Left alone, it bursts small when its time is up.

**When it happens.** From a spell slot, while Fire and Lightning are both carried: cast it with that slot's button (hold L2 and a face button, or 1 to 4), then swing at the ball. Its first landing charges your blade with Fire, the branch's first element.

**The numbers.** Mix, duo spell, 5 Shards, 40 mana. It appears 1.5 metres ahead and drifts at 2 metres a second for 8 seconds; batted, it flies at 9. Burst: 2.5 metres in radius, 16 damage and 20 stagger, two stacks of Burn and of Static; each strike adds 0.7 metres, 12 damage, 10 stagger and a stack of each. At three strikes: 52 damage, 50 stagger and a heavy blow.

**Works with.** Twin Break or Ionised Air (it grows from either), Meltdown, and Flashover on whatever it leaves burning and charged.

**Example.** Bat it three times into an archer: 52 damage, and all near are five deep in both.

**Not built yet.** The ball is still drawn in its first colours; the violet heart and the flame on its skin come with its look.

## fire.plasma
**What it does.** The branch's top mark. A heavy blow or a finisher on a burning enemy bursts. The burst sets off the burn on everyone it reaches and then sets each of them alight again. If the enemy carries Static as well, Flashover goes off on it at once, whatever its rest, in the shape you wear.

**When it happens.** Every heavy blow or finisher that lands on a burning enemy, while Fire and Lightning are both carried.

**The numbers.** 5 Shards, bought once both elements are carried; no god gives it. The burst is 3.2 metres in radius: 18 damage and 16 stagger. Each enemy in it that was burning takes its burn set off as well, 10 damage and 10 more a stack, and every enemy in it burns for 4 seconds afterward. Nothing of the first enemy's Burn is spent.

**Works with.** Cinder, Heat, Catching Fire, Flashover's shapes, Ionised Air.

**Example.** Three soldiers burning five deep: a heavy blow on one bursts over all three for 78 each, and leaves them burning; if it was charged too, Flashover goes off on it at once.

**Not built yet.** On an enemy with no Static the burst is still its own; the design makes it a Flashover that spends nothing.
