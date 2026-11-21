# Plasma: detailed explanations

The long explanation the chart shows for every mark on Plasma, the duo branch Fire and Lightning share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Fire and Lightning") and drawn in the heart of the chart, the ring's middle, while
Fire and Lightning are both carried (in the playtest, whenever you pick Lightning beside Fire or Fire beside Lightning). Its
reaction is Flashover; its Shape mark lets you choose what Flashover does.

## plasma.shape
**What it does.** Unlocks two new shapes for Flashover. At a rest fire, you pick which shape your Flashover uses. From then on, it uses that shape instead of the plain Flashover. **Corona**: nothing is used up. Violet flame crawls over the enemy and throws small arcs of lightning at the enemies next to it. **Fulgurite**: the crack goes down instead of out. The ground under the enemy melts into a scorched, glassy patch that burns and shocks anyone standing in it.

**When it happens.** Every time Flashover goes off while you carry both Fire and Lightning. Flashover goes off when Static lands on a burning enemy, or when Burn lands on a charged one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Flashover.

**The numbers.** Mix, 3 Shards. Corona: lasts 4 seconds. Every 0.7 seconds, an arc jumps from the enemy to the nearest other enemy within 5 metres for 4 damage, a Burn and a Static (about five arcs in total). Fulgurite: a patch 2.5 metres across that lasts 4 seconds. Once a second, it deals 4 damage, a Burn and a Static. What either shape adds doesn't start a reaction of its own. After Flashover goes off on an enemy, it still can't go off on that enemy again for 4 seconds.

**Works with.** Ionised Air (an arc jumps on through three more enemies, whatever the shape), Twin Break, Meltdown (Corona and Fulgurite both keep Burn and Static on everyone nearby), Plasma.

**Example.** A pack pushing in: use Corona, and one burning soldier you charge keeps arcing into the two next to it for four seconds. Fighting one brute in a doorway: use Fulgurite, and fight on the patch it leaves.

**Not built yet.** Corona's arcs jump out from the enemy, instead of pulling other bursts of Static into its flame. Fulgurite's patch looks like burning, crackling ground until the glassy look is made.

## plasma.twin
**What it does.** A perfect guard sets off Burn and Static together on the enemy that hit you. Set off together, they hit 50% harder, and the blow counts as a heavy blow, so it stops the enemy where it stands. If it only has one of the two, that one is set off alone.

**When it happens.** A perfect guard (press the guard button just before a blow lands) against an enemy that has Burn, Static, or both.

**The numbers.** Mix, 3 Shards. Together: 10 damage per Burn stack and 7 per Static stack, times 1.5, plus 5 stagger per stack. At five of each, that's 127.5 damage and 50 stagger. Alone, within 3 metres: Burn deals 10 plus 15 per stack; Static deals 10.5 per stack; each adds 5 stagger per stack.

**Works with.** Kiln Stance, Stored Charge, Meltdown, Ball Lightning (it comes after this or Ionised Air on the chart). Breath of the Lioness uses up the burn first, so the two get in each other's way.

**Example.** A burning, charged brute swings. Guard it perfectly, and it takes 127 damage and stops dead.

## plasma.ionised
**What it does.** Whenever Flashover goes off, an arc jumps from the burning enemy through three more, hitting and charging each one.

**When it happens.** Each time Flashover goes off on an enemy, plain or in either shape, while you carry both Fire and Lightning.

**The numbers.** Mix, 3 Shards. 10 damage, 8 stagger and a Static to each of up to three more enemies, jumping up to 5 metres from one to the next. Flashover itself still can't go off on the same enemy again for 4 seconds.

**Works with.** Flashover's Shape mark (Corona or Fulgurite), Chain Reaction, which comes after it on the chart, and Static or Thunderclap to add the charge.

**Example.** Set a soldier on fire in a crowd and hit it with Thunderclap. Flashover goes off, and the arc runs on into three of its friends, leaving each one charged for the next.

## plasma.chain
**What it does.** An enemy that dies with both Burn and Static bursts, and passes both on to everyone nearby, with as many stacks as it had.

**When it happens.** The moment an enemy with both dies.

**The numbers.** Mix, 3 Shards. The burst has a 4-metre radius and deals 18 damage plus as many Burn stacks as the dead enemy had. Everyone within 4 metres also gets as many Static stacks as it had. At most a few times a second.

**Works with.** Ionised Air, which it comes after on the chart, Catching Fire, Meltdown.

**Example.** Kill one soldier with five of each, and the three around it start with five of each too, one blow away from Meltdown.

## plasma.meltdown
**What it does.** An enemy near you with five Burn and five Static starts to shake with light. A few moments later, it bursts, much harder than either one alone, and the burst uses up both. Nothing stops it, and it doesn't hurt you.

**When it happens.** Always on once you learn it, while you carry both Fire and Lightning. A few times a second, it checks for an enemy within 14 metres that has both afflictions full. Only one enemy melts down at a time.

**The numbers.** Mix, keystone, 5 Shards. The enemy shakes for 3 seconds, then bursts. It takes 45 damage and 60 stagger as a heavy blow, and everything else within 5 metres takes half of each. The same enemy can't melt down again for 10 seconds.

**Works with.** Twin Break or Chain Reaction (it comes after either on the chart), Ball Lightning, Corona.

**Example.** Fill a brute with both and roll away. Three seconds later it goes up, staggering the pack around it.

## plasma.ball
**What it does.** You release a slow ball of plasma, violet in the middle with real flame on its surface, that floats after the nearest enemy. Hit it with a swing to send it flying fast in the direction of the swing, and it grows one step bigger, up to three. It bursts on the first enemy it meets, adding Burn and Static, and it is stronger for each time you hit it. If you leave it alone, it bursts small when its time is up.

**When it happens.** Cast it from a spell slot while you carry both Fire and Lightning. Press that slot's button (hold L2 and a face button, or press 1 to 4), then swing at the ball. The first time it lands, it charges your blade with Fire, this branch's first element.

**The numbers.** Mix, duo spell, 5 Shards, 40 mana. It appears 1.5 metres in front of you and floats at 2 metres a second for 8 seconds. When you hit it, it flies at 9 metres a second. The burst has a 2.5-metre radius and deals 16 damage, 20 stagger, and two stacks each of Burn and Static. Each hit adds 0.7 metres, 12 damage, 10 stagger and one stack of each. At three hits: 52 damage, 50 stagger, and it counts as a heavy blow.

**Works with.** Twin Break or Ionised Air (it comes after either on the chart), Meltdown, and Flashover on whatever it leaves burning and charged.

**Example.** Hit it three times into an archer. It deals 52 damage, and everything nearby has five of both.

**Not built yet.** The ball still has its first colours. The violet middle and the flame on its surface will come with its new look.

## fire.plasma
**What it does.** This branch's top mark. A heavy blow or a finisher on a burning enemy makes it burst. The burst sets off the burn on everyone it reaches, then sets each of them on fire again. If the enemy has Static as well, Flashover goes off on it right away, even if Flashover would normally have to wait, in the shape you picked.

**When it happens.** Every heavy blow or finisher that lands on a burning enemy, while you carry both Fire and Lightning.

**The numbers.** 5 Shards. You can buy it once you carry both elements; no god gives it. The burst has a 3.2-metre radius and deals 18 damage and 16 stagger. Each enemy in it that was burning also has its burn set off, for 10 damage plus 10 more per stack, and every enemy in it burns for 4 seconds afterward. None of the first enemy's Burn is used up.

**Works with.** Cinder, Heat, Catching Fire, Flashover's shapes, Ionised Air.

**Example.** Three soldiers each have five Burn. A heavy blow on one bursts over all three for 78 each, and leaves them burning. If that one was charged too, Flashover goes off on it right away.

**Not built yet.** On an enemy with no Static, the burst is still its own thing. The plan is to make it a Flashover that doesn't use anything up.
