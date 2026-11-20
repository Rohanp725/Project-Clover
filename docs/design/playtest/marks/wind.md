# Wind: detailed explanations

The long explanation the chart shows for each high-level mark of the Wind tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Wind): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## wind.unsteady
**What it does.** Every blow of your weapon that lands knocks the enemy Unsteady, one stack at a time, up to five. An Unsteady enemy loses its balance sooner, is thrown further and turns more slowly, so you can circle behind it.

**When it happens.** Always on once learned: every weapon blow that lands, light or heavy, while Wind is your first element. With Wind as your second element the weapon lays the first element's affliction, and Unsteady comes from Gust, the charged blade and your R2 strike.

**The numbers.** 2 Shards. One stack a blow, five at most, lasting 6 seconds. Each stack means 6% more stagger taken, a 15% longer shove and 8% slower turning: 30%, 75% and 40% at five. During Tailwind every blow lays one more.

**Works with.** Rising Cut (five deep throws it into the air), Indrawn Breath (its burst spends the stacks), Against the Wall, Nothing Beneath, Carried Far, and the Carried Embers reaction when a burning enemy is knocked Unsteady.

**Example.** Four light blows and a heavy on a soldier: five deep, it takes 30% more stagger from the heavy and is often broken by it.

## wind.rising
**What it does.** A heavy blow on an enemy five deep in Unsteady throws it into the air. It hangs there helpless, and each blow you land on it up there holds it up a little longer, up to three. The throw spends the Unsteady.

**When it happens.** A heavy blow or finisher that lands on an enemy carrying five Unsteady, at most twice a second.

**The numbers.** 3 Shards. In the air for 1 second (elites and gods for 0.6 of it). Each of the first three blows on it up there adds 0.45 seconds. What is too heavy to lift reels for a second instead and takes 40 stagger.

**Works with.** Unsteady, Lifting Wind and Sky Dance, which grow from it, Carried Far, Nothing Beneath, the deep strike Whirl Cut, and Thunder Lift and The Sky Finds Them on the Tempest branch.

**Example.** Five deep, a soldier takes your heavy blow and goes up: two quick light blows keep it there while you set up the third.

## wind.lifting
**What it does.** A circle is drawn under the enemy you face and follows it for a breath; then a column of wind bursts up from it, striking as a heavy blow and throwing it into the air whatever its footing. What is too heavy to lift reels instead.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). With no enemy within reach the column comes up 5 metres ahead of you, and the slot still waits.

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 7). The nearest enemy within 5 metres. The column rises 0.35 seconds after the cast, 2 metres in radius: 8 damage, 40 stagger, a heavy blow and one stack of Unsteady to all in it, and 0.8 seconds in the air.

**Works with.** Rising Cut, which it grows from; Nothing Beneath; Sky Dance; Against the Wall (it is a spell striking); the charged blade: it lands, so your next three blows carry Wind.

**Not built yet.** The hang of a full second (1.3 seconds with its Empower): the column's lift is the form's own 0.8 seconds.

**Example.** An archer behind a shield: lift the archer, step in, and keep it up with your blows.

## wind.lifting.shape
**What it does.** Opens two shapes of Lifting Wind, chosen at any rest fire. **Dust Devil**: the column wanders after enemies for a while, lifting the first it reaches and then the next. **Turned About**: the column spins its enemy round to face away from you, and your first blow on its back is critical; the shape for what cannot be lifted.

**When it happens.** Whenever you cast Lifting Wind, as the shape it wears.

**The numbers.** 3 Shards. Dust Devil: two columns, under the two nearest enemies, 0.6 seconds after the cast. Turned About: for 1.2 seconds the enemy begins no attack and the first weapon blow on it is critical, which ends it.

**Works with.** Rising Cut and Nothing Beneath (Dust Devil in a pack); the heavy weapons and the colossus fights (Turned About).

**Not built yet.** Dust Devil as one column wandering for 3 seconds (for now two rise at once), and Turned About not lifting, the enemy swung round in the column, half the time for an elite and a god's next attack swung wide (for now it is lifted too).

**Example.** A brute too heavy to lift: Turned About, and your next cut lands critical on its back.

## wind.lifting.master
**What it does.** Cast Lifting Wind on the enemy a perfect dodge has just taken you past, and it hangs twice as long.

**When it happens.** A perfect dodge, then Lifting Wind within a second. Lifting Wind must be in a slot.

**The numbers.** 3 Shards. Within a second of a perfect dodge, a Lifting Wind that is ready starts no wait. Each enemy the column takes is held up for 1.6 seconds.

**Works with.** Passing Cut and Leaf on the Wind, which keep you dodging; Nothing Beneath.

**Not built yet.** Only the enemy you dodged past hanging twice as long: for now every enemy the column takes hangs longer, after a dodge or not.

**Example.** Roll through a duellist's lunge, cast as you come out behind it: it goes up for twice the time and the next cast is free.

## wind.skydance
**What it does.** You throw the enemy you face into the air and go up beside it. Five rings close on it, one after another: press the light attack as each closes for a cut in time, half again as hard. Then a lane follows the stick (or the cursor) for a moment, and you throw the enemy down along it. It bursts where it lands, harder for each cut in time. Nothing can strike you from the leap to the landing.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). It needs an enemy faced within 6 metres that is not still reeling from the last dance; otherwise it is not cast and no mana is spent. The heavy attack throws at once.

**The numbers.** 4 Shards, 45 mana (its Empower: 35, and six rings). A ring every 0.34 seconds; a press in the last part of its closing is in time. Each cut deals 10 damage, 15 in time; the first lays a stack of Unsteady. The throw goes 6 metres. The landing deals 20 damage, and 6 more for each cut in time (50 at five), with 60 stagger as a heavy blow, to the one thrown and to everything within 3 metres, which is pushed 3 metres away. What cannot be lifted is danced with on the ground.

**Works with.** Rising Cut, which it grows from; Nothing Beneath; Against the Wall (each cut is a spell striking); Unsteady.

**Example.** Dance a brute up, land all five cuts in time, and throw it down into the soldiers behind it: 50 to each.

## wind.skydance.shape
**What it does.** Opens two shapes of Sky Dance, chosen at any rest fire. **Carried Off**: there is no throw; you carry the enemy down the lane and drop it on another, and both take the burst. **Two Rise**: the enemy faced and the nearest other both go up, and the rings take turns between them.

**When it happens.** Whenever you cast Sky Dance, as the shape it wears.

**The numbers.** 3 Shards. Carried Off: the lane is 8 metres. Two Rise: the first cut throws up everything within 3 metres of the enemy for 1.5 seconds.

**Works with.** Against the Wall and Indrawn Breath (Carried Off into a crowd); Nothing Beneath (Two Rise keeps two up).

**Not built yet.** Carried Off carrying the enemy and dropping it on another (for now it is thrown further), and Two Rise's rings taking turns between two (for now the second is only lifted).

**Example.** Two soldiers side by side: Two Rise takes both up, and the landing bursts on both.

## wind.skydance.master
**What it does.** Land all five cuts in time, and you come down with Tailwind lit for four seconds at no cost.

**When it happens.** Sky Dance strikes. Sky Dance must be in a slot.

**The numbers.** 3 Shards. For 4 seconds your dodge carries 30% further and is safe 25% longer, and you move 15% faster: Tailwind's own gifts, once a dance.

**Works with.** Passing Cut, Leaf on the Wind, Closing Circle: the fight goes on in motion after the landing.

**Not built yet.** Only when all five cuts were in time, and Tailwind itself lit (its own hanging blows and its Unsteady): for now any dance that strikes gives the dodge and the speed.

**Example.** Five in time, thrown into a pack: you land and roll through them at once, further and safer.

## wind.lift
**What it does.** Carried Far: every reaction with Unsteady in it throws what it strikes out from where it happened.

**When it happens.** Whenever Unsteady reacts on an enemy with the other affliction it carries (Carried Embers with Burn, Windthrow and the rest).

**The numbers.** 3 Shards. Every enemy within 2.5 metres of the reaction is thrown 1.5 metres away from it.

**Works with.** Carrying a second element: every reaction of Wind's ten; Against the Wall, for the throw into something.

**Not built yet.** The reactions reaching half again as far (it needs a stat for a reaction's reach).

**Example.** Wind and Fire on a soldier in a crowd: Carried Embers goes off and the soldiers round it are thrown back.

## wind.nothing
**What it does.** An enemy in the air stays up as long as you keep striking it, each blow there gives double mana, and your blows no longer drop it after three. The price: your weapon is weaker against an enemy that is not Unsteady.

**When it happens.** Always on once learned. The longer hang and the mana come with each blow that lands on an enemy in the air.

**The numbers.** 6 Shards. Up to 23 blows hold an enemy in the air, not three; the first adds 0.45 seconds and each after a little less, about 3 seconds more in all. Blows on an enemy in the air give twice their mana. Weapon damage is 15% lower, back to full against an Unsteady enemy.

**Works with.** Rising Cut, Closing Circle, Lifting Wind, Sky Dance, Unsteady.

**Example.** Lift a soldier and keep cutting: it hangs for four seconds while your mana bar fills twice as fast.

## wind.devotion
**What it does.** On the Wind, Wind's Devotion: all in on Wind only. Your spells have no wind-up and can be cast in the middle of a dodge or a swing without ending it, and a spell cast in a dodge carries you further along it. Nearly all of the fight is spent in motion.

**When it happens.** Only while Wind is carried twice (all in). Then always.

**The numbers.** 6 Shards. A spell cast in a dodge carries you 2 metres further. All in, L2 with R2 is the deep strike, Whirl Cut: the crescent becomes a small whirlwind that holds the first enemy it reaches in the air for a second.

**Works with.** Every spell of the tree; Passing Cut and Leaf on the Wind; the deep strike.

**Not built yet.** Spells with no wind-up, cast in a dodge or a swing without ending it, and the 2 metres added to a dodge cast in. For now every dodge carries 30% further.

**Example.** Roll at a pack, cast Indrawn Breath in the roll, and come out of it with the pack heaped in front of you.

## wind.passing
**What it does.** Where a dodge ends, a cut of wind strikes everything close to you and leaves it Unsteady.

**When it happens.** At the end of every dodge, at most twice a second.

**The numbers.** 3 Shards. Everything within 2.2 metres of where you stop takes 10 damage, 8 stagger and one stack of Unsteady.

**Works with.** Closing Circle and Turned Aside, which grow from it; Leaf on the Wind; Tailwind's longer dodge.

**Not built yet.** The cut along the path you passed through, each enemy passed unable to turn for a moment: for now the cut comes where you stop.

**Example.** Roll into the middle of three soldiers: all three are cut and Unsteady before you swing.

## wind.circle
**What it does.** You run a full ring round the enemy you face. When the ring closes, the wind inside it rises: everything within is struck, knocked five deep in Unsteady and thrown into the air. You end where you began, facing in, and nothing can strike you while you run.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). It needs an enemy faced within 5 metres; with none, it is not cast and nothing is spent.

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 7, and 1.5 seconds in the air). The ring is as wide as the gap to the enemy, 2 to 3 metres in radius. The run takes 0.7 seconds and the ring snaps shut 0.2 seconds later. All inside take 6 damage, 10 stagger and five stacks of Unsteady, and hang in the air for 1.2 seconds. What cannot be lifted reels instead.

**Works with.** Passing Cut, which it grows from; Rising Cut and Nothing Beneath; Indrawn Breath, whose burst spends what it laid.

**Not built yet.** Nothing can strike you for the whole run; the design asks for the first half only.

**Example.** Two soldiers flank a brute: circle the brute and all three go up five deep.

## wind.circle.shape
**What it does.** Opens two shapes of Closing Circle, chosen at any rest fire. **Half Circle**: you end on the enemy's far side, behind it; nothing is lifted, and your first blow on its back is critical. **Wide Circle**: you run round a group, and the column takes everyone inside, but lifts each for only a moment.

**When it happens.** Whenever you cast Closing Circle, as the shape it wears.

**The numbers.** 3 Shards. Half Circle: a step through the enemy within 5 metres, 6 damage and 10 stagger, and for 1.5 seconds your next blow is critical. Wide Circle: an enemy faced within 7 metres, a ring up to 4 metres in radius, and 0.6 seconds in the air.

**Works with.** Turned About (another way behind it); Indrawn Breath and Against the Wall (Wide Circle round a crowd).

**Not built yet.** Half Circle running half the ring (for now it steps through the enemy), and the Empower's longer hang reaching Wide Circle's short one (an Empower's numbers are set after the shape's, so empowered it hangs 1.5 seconds).

**Example.** A shield-bearer facing you: Half Circle, and the first cut lands critical on its unguarded back.

## wind.circle.master
**What it does.** Started in the instant a blow would land on you, Closing Circle counts as a perfect dodge, and the ring is run round the attacker wherever it stands within eight metres.

**When it happens.** For now, every cast of Closing Circle. It must be in a slot.

**The numbers.** 3 Shards. What a perfect dodge gives: 12 mana and 1.5 seconds off every running cooldown.

**Works with.** Lifting Wind's Master and the other marks a perfect dodge sets off; Leaf on the Wind.

**Not built yet.** The timing (started as a blow would land) and the ring run round the attacker within 8 metres; for now every cast gives the dodge's mana and cuts.

**Example.** A spearman thrusts: start the circle into it, and you run round it as the spear passes, mana and cooldowns paid.

## wind.turned
**What it does.** A perfect guard throws everyone near you back and leaves the attacker three deeper in Unsteady.

**When it happens.** A perfect guard: press the guard button just before a blow lands.

**The numbers.** 3 Shards. Every enemy within 4 metres is pushed 3 metres away. Three stacks of Unsteady on the attacker.

**Works with.** Bag of Winds and Leaf on the Wind, which grow from it; Rising Cut; Against the Wall.

**Not built yet.** A thrown attack sent back to its thrower.

**Example.** Ringed by four soldiers, perfect-guard the first swing: the ring is thrown back and the one who swung is three deep.

## wind.bag
**What it does.** You untie a bag of wind at your hip. Each blow you perfect-guard or perfect-dodge is caught in it, three at most, and thrown things that come near are caught too. Press again, and it opens: a cone that strikes harder for each wind held, and every thrown thing flies back at its thrower.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Then each perfect guard or perfect dodge while the bag holds a wind.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast (its Empower: 8). The bag holds three winds, shown on the screen as Winds. Each perfect guard or perfect dodge lets one out as a cone of 7 metres and 60 degrees: 12 damage, 25 stagger and a stack of Unsteady.

**Works with.** Turned Aside, which it grows from; Lifting Wind's and Closing Circle's Masters (more perfect dodges); Indrawn Breath.

**Not built yet.** The bag as designed: open 8 seconds, each caught blow throwing its attacker back 2 metres and kept, thrown things caught, and a second press (or the time's end) opening it as one cone of 12 damage and 25 stagger for each wind held, the thrown things flying back. For now the cast fills the bag and each perfect guard or dodge lets a wind out at once. The Empower's fourth wind.

**Example.** Cast it as a brute begins its combo: guard three blows perfectly, and three cones throw it back down the hall.

## wind.bag.shape
**What it does.** Opens two shapes of Bag of Winds, chosen at any rest fire. **Kept Wind**: the bag does not open by itself; it keeps its winds for a long while, to be opened when you choose, such as on a staggered enemy. **Loose Neck**: nothing is kept; each wind leaves at once as a free Gust toward the enemy you next face.

**When it happens.** Whenever you cast Bag of Winds, as the shape it wears; Loose Neck on every perfect guard and perfect dodge while it is slotted.

**The numbers.** 3 Shards. Kept Wind: 20 seconds. Loose Neck: each perfect guard or perfect dodge throws a Gust, a cone of 5 metres and 70 degrees, 8 damage, 20 stagger and two Unsteady; a guarded attacker is also thrown 3 metres.

**Works with.** Turned Aside and Rising Cut (Loose Neck's Unsteady); a staggered enemy (Kept Wind).

**Not built yet.** Kept Wind does nothing beyond the bag yet: it waits for the bag that keeps its winds. Loose Neck's gust turned toward the enemy you next face (for now toward the one caught).

**Example.** Loose Neck against a pack of duellists: every perfect guard throws a gust into them.

## wind.bag.master
**What it does.** Catch three winds within three seconds, and the bag bursts by itself as a whirlwind: everything within six metres goes into the air.

**When it happens.** For now, a perfect dodge, at most once every 10 seconds. Bag of Winds must be in a slot.

**The numbers.** 3 Shards. Everything within 6 metres of you goes into the air for a second.

**Works with.** Rising Cut and Nothing Beneath, for the enemies in the air; Lifting Wind.

**Not built yet.** Three winds caught within 3 seconds as what bursts the bag; for now a perfect dodge raises the whirlwind.

**Example.** Dodge a brute's swing perfectly in a crowd: everything round you goes up, ready for the cuts.

## wind.leaf
**What it does.** The dodge costs no stamina. It spends a breath of wind instead: you hold three, and time and blows win them back. With no breath left you cannot dodge. The price: you take more damage.

**When it happens.** Always on once learned. Every dodge spends a breath; the breaths come back as you fight.

**The numbers.** 6 Shards. Three breaths at most, shown on the screen as Breath. One comes back every 2 seconds while you hold fewer than three, and one for every three blows you land. You take 20% more damage from every blow.

**Works with.** Passing Cut, Closing Circle, Bag of Winds, Tailwind's longer, safer dodge.

**Example.** Against a fast duellist, dodge three times in a row, then land three blows: a breath is back before the next swing comes.

## wind.wall
**What it does.** Any enemy you throw that meets a wall, a pillar or another enemy is struck hard, harder for each Unsteady it carries; when two bodies meet, both are. The payoff every throw in the tree waits for.

**When it happens.** For now, every time a spell of yours strikes an enemy: Gust, any spell from your slots, and the spells your marks throw.

**The numbers.** 3 Shards. As designed: 15 damage, 6 more for each Unsteady, and 30 stagger. For now a burst 1.6 metres in radius on each enemy a spell strikes: 6 damage, 15 stagger and one stack of Unsteady.

**Works with.** Gust and its Long Gust, Indrawn Breath's All One Way, Sky Dance, Turned Aside, the charged blade's Gale Edge.

**Not built yet.** The throw meeting something (a wall, a pillar, another enemy): for now the burst comes wherever a spell strikes.

**Example.** Gust three soldiers in a row: each takes the gust and a burst, and they shake each other with theirs.

## wind.base
**What it does.** Gust, Wind's own spell, comes with the element and is never bought. A rush of air runs out ahead of you, striking everything in it, knocking it Unsteady and throwing it straight down the wind.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Wind, it is always yours to put in a slot.

**The numbers.** Given with Wind. No mana: it waits 6 seconds after a cast, less with weapon play; its Empower makes that 4.5. A cone 5 metres long and 70 degrees wide: 8 damage, 20 stagger and two stacks of Unsteady, and everything in it is thrown 3 metres.

**Works with.** Its Empower, its two shapes and its Master; Against the Wall; the charged blade, which its landing charges with Wind.

**Not built yet.** The Empower's third stack of Unsteady: the base spell is cast by Wind's kit, which takes only the Empower's shorter wait.

**Example.** A soldier at the edge of a drop: Gust, and it is gone.

## wind.base.shape
**What it does.** Opens two shapes of Gust, chosen at any rest fire. **Downburst**: the wind comes down from above and spreads as a ring, throwing everything near outward; aim it at your feet to clear a ring round you. **Long Gust**: a narrow lane that throws what it takes far, for throwing one enemy into a wall, a pit or its friends.

**When it happens.** Whenever you cast Gust, as the shape it wears. Before this is bought, the plain gust is cast.

**The numbers.** 3 Shards. Downburst: everything within 3 metres of you, 8 damage, 20 stagger, two Unsteady, thrown 2.5 metres outward. Long Gust: a lane 2 metres wide and 10 long, 8 damage, 20 stagger, two Unsteady, thrown 5 metres.

**Works with.** Against the Wall (Long Gust into a wall); Passing Cut and Turned Aside when surrounded (Downburst).

**Not built yet.** Downburst aimed at a point up to 6 metres away; for now it always comes down on you.

**Example.** Ringed by four soldiers, Downburst throws them all back two deep. An archer behind a line: Long Gust carries it off its ledge.

## wind.base.master
**What it does.** Cast Gust in the instant an enemy's attack mark shows, and the gust turns its blow aside into empty air.

**When it happens.** For now, every enemy Gust strikes. Gust must be in a slot.

**The numbers.** 3 Shards. Each enemy struck flinches: what it had begun is broken, unless it is steadfast. As designed elites' blows come 0.4 seconds late instead, and gods are not turned.

**Works with.** Every shape of Gust; Against the Wall.

**Not built yet.** The instant of the attack mark as the timing (it needs a trigger on an enemy's wind-up), and the elites' late blow: for now any gust that lands makes its enemies flinch.

**Example.** A brute's red overhead begins: Gust, and the blow falls on nothing.

## wind.indrawn
**What it does.** You draw in your breath, and everything near is dragged into one place before you. Then the breath bursts out of that place, striking all of them as a heavy blow and spending each enemy's Unsteady, harder for every stack.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 11 seconds after a cast, less with weapon play (its Empower: 9, and it draws from 8.5 metres). The point is 2 metres ahead, or on an enemy that close. For 0.5 seconds everything within 7 metres of it is dragged most of the way in. Then the burst: 10 damage, 25 stagger, a heavy blow and two stacks of Unsteady to each, and each enemy's Unsteady goes off for 6 damage and 8 stagger a stack.

**Works with.** Against the Wall, which it grows from; Closing Circle (five deep inside the ring), Rising Cut; Catching Fire with Fire carried, since it bunches a crowd.

**Example.** Archers spread round a room, each a few deep: draw them all to one spot, and every stack they carry bursts at once.

## wind.indrawn.shape
**What it does.** Opens two shapes of Indrawn Breath, chosen at any rest fire. **Held Breath**: the draw lasts as long as the button is held, and the point walks with you; let go to burst. **All One Way**: the burst does not scatter them; all of them are thrown together the way you face. Gather a crowd, then throw it into a wall.

**When it happens.** Whenever you cast Indrawn Breath, as the shape it wears.

**The numbers.** 3 Shards. Held Breath: up to 2 seconds. All One Way: each enemy the burst strikes is thrown 5 metres straight away from you.

**Works with.** Against the Wall (All One Way), Sky Dance, Bag of Winds.

**Not built yet.** Held Breath held on its button, the point walking with you (for now the draw always lasts the full 2 seconds); All One Way's burst not scattering them first.

**Example.** Draw a crowd into a heap, then All One Way throws the heap into the wall behind it.

## wind.indrawn.master
**What it does.** Cast Indrawn Breath in the middle of a dodge: the draw's point rides with you to where the dodge ends, and the burst spends no Unsteady, so every stack stays for the next blow.

**When it happens.** For now, every enemy the burst strikes. Indrawn Breath must be in a slot.

**The numbers.** 3 Shards. Each enemy struck is given two stacks of Unsteady back after the burst.

**Works with.** Rising Cut (back toward five), Closing Circle.

**Not built yet.** A spell cast in the middle of a dodge as the timing, the point riding with you, and the burst spending nothing; for now each enemy gets two stacks back.

**Example.** Roll through a pack and draw it in behind you: the burst lands and the pack stays Unsteady for your next heavy.

## wind.remembers
**What it does.** While your surge lasts, every weapon blow you land leaves its cut hanging in the air where it fell, up to sixteen. When the surge ends, time slows and all of them fall again, one on the heels of the next, each half as hard as the blow it remembers.

**When it happens.** Light the surge with the surge button (a tap of L2, or F). The cuts fall the moment it ends.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. Sixteen cuts at most; past that, each new blow is added to the lightest cut hanging. Each falls 1.4 metres round where it was struck for half the blow's damage, 10 stagger and one stack of Unsteady. Tailwind lasts 10 seconds for 35 mana.

**Works with.** Tailwind, Against the Wall (each falling cut is a spell striking), Nothing Beneath, Rising Cut.

**Example.** Light Tailwind and cut a pack sixteen times: as it ends, sixteen cuts fall on them again at half strength.
