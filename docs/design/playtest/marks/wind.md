# Wind: detailed explanations

The long explanation the chart shows for each high-level mark of the Wind tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Wind): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## wind.unsteady
**What it does.** Every weapon blow that lands makes the enemy Unsteady, one stack at a time, up to five. An Unsteady enemy loses its balance sooner, is thrown farther, and turns more slowly, so you can circle behind it.

**When it happens.** Always on once you learn it: every weapon blow that lands, light or heavy, while Wind is your first element. If Wind is your second element, your weapon adds your first element's affliction, and Unsteady comes from Gust, the charged blade and your R2 strike.

**The numbers.** 2 Shards. One stack per blow, up to five, lasting 6 seconds. Each stack means the enemy takes 6% more stagger, is shoved 15% farther, and turns 8% slower: 30%, 75% and 40% at five. During Tailwind, every blow adds one more stack.

**Works with.** Rising Cut (five stacks throws it into the air), Indrawn Breath (its burst uses up the stacks), Against the Wall, Nothing Beneath, Carried Far, and the Carried Embers reaction when a burning enemy is made Unsteady.

**Example.** Four light blows and a heavy blow on a soldier. With five stacks, it takes 30% more stagger from the heavy blow, which often breaks its balance.

## wind.rising
**What it does.** A heavy blow on an enemy with five stacks of Unsteady throws it into the air. It hangs there helpless, and each blow you land on it up there keeps it up a little longer, up to three blows. The throw uses up the Unsteady.

**When it happens.** A heavy blow or finisher that lands on an enemy with five Unsteady, at most twice a second.

**The numbers.** 3 Shards. It stays in the air for 1 second (elites and gods for 0.6 of that). Each of the first three blows on it up there adds 0.45 seconds. An enemy too heavy to lift staggers for a second instead and takes 40 stagger.

**Works with.** Unsteady, Lifting Wind and Sky Dance, which come after it on the chart, Carried Far, Nothing Beneath, the deep strike Whirl Cut, and Thunder Lift and The Sky Finds Them on the Tempest branch.

**Example.** A soldier with five stacks takes your heavy blow and goes up. Two quick light blows keep it there while you get ready for the third.

## wind.lifting
**What it does.** A circle appears under the enemy you face and follows it for a moment. Then a column of wind bursts up from it, hitting as a heavy blow and throwing the enemy into the air, no matter how steady it is. An enemy too heavy to lift staggers instead.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). If no enemy is in reach, the column comes up 5 metres in front of you, and you still have to wait.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 7 seconds.) It targets the nearest enemy within 5 metres. The column rises 0.35 seconds after you cast it, with a 2-metre radius. It deals 8 damage, 40 stagger, counts as a heavy blow, adds one stack of Unsteady to everything in it, and keeps them in the air for 1 second (1.3 seconds with its Empower).

**Works with.** Rising Cut, which it comes after on the chart; Nothing Beneath; Sky Dance; Against the Wall (it counts as a spell hitting); and the charged blade: when it lands, your next three blows carry Wind.

**Example.** An archer stands behind a shield. Lift the archer, step in, and keep it up with your blows.

## wind.lifting.shape
**What it does.** Unlocks two shapes for Lifting Wind. You pick one at any rest fire. **Dust Devil**: the column wanders after enemies for a while, lifting the first one it reaches, then the next. **Turned About**: the column spins its enemy around to face away from you, and your first blow on its back is a critical hit. It's the shape for enemies that can't be lifted.

**When it happens.** Whenever you cast Lifting Wind, using the shape you picked.

**The numbers.** 3 Shards. Dust Devil: two columns rise under the two nearest enemies, 0.6 seconds after you cast it. Turned About: the column deals 8 damage and 40 stagger but lifts nothing. It spins the enemy around to face away from you for 1.2 seconds (half a second for an elite). In that time it can't start an attack or walk, and your first weapon blow on it ends the spin. If that blow lands on its back, it is a critical hit.

**Works with.** Rising Cut and Nothing Beneath (Dust Devil in a pack); the heavy weapons and the colossus fights (Turned About).

**Not built yet.** Dust Devil should be one column wandering for 3 seconds (for now, two rise at once). A god isn't spun around; its next attack should swing wide instead, and it doesn't yet.

**Example.** A brute too heavy to lift: use Turned About, and your next cut lands as a critical hit on its back.

## wind.lifting.master
**What it does.** Cast Lifting Wind on the enemy a perfect dodge just took you past, and it hangs in the air twice as long.

**When it happens.** A perfect dodge, then Lifting Wind within a second. Lifting Wind must be in a spell slot.

**The numbers.** 3 Shards. Within a second of a perfect dodge, casting Lifting Wind while it is ready doesn't start its wait. Each enemy the column lifts is held up for 1.6 seconds.

**Works with.** Passing Cut and Leaf on the Wind, which keep you dodging; Nothing Beneath.

**Not built yet.** Only the enemy you dodged past should hang twice as long. For now, every enemy the column lifts hangs longer, whether you dodged or not.

**Example.** Roll through a duellist's lunge and cast it as you come out behind it. It goes up for twice as long, and the next cast is free.

## wind.skydance
**What it does.** You throw the enemy you face into the air and leap up next to it. Five rings close in on it, one after another. Press light attack as each ring closes for a perfectly timed cut that hits 50% harder. Then a path follows the stick (or the cursor) for a moment, and you throw the enemy down along it. It bursts where it lands, harder for each perfectly timed cut. Nothing can hit you from the leap until you land.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). It needs an enemy you face within 6 metres that isn't still staggering from the last dance. Otherwise, it isn't cast and no mana is spent. Pressing heavy attack throws the enemy right away.

**The numbers.** 4 Shards, 45 mana (its Empower makes it 35, with six rings). A ring closes every 0.34 seconds. A press in the last part of its closing counts as perfectly timed. Each cut deals 10 damage, or 15 if perfectly timed. The first cut adds a stack of Unsteady. The throw goes 6 metres. The landing deals 20 damage, plus 6 more for each perfectly timed cut (50 with all five), and 60 stagger as a heavy blow, to the enemy you threw and everything within 3 metres. Those enemies are pushed 3 metres away. An enemy that can't be lifted is danced with on the ground.

**Works with.** Rising Cut, which it comes after on the chart; Nothing Beneath; Against the Wall (each cut counts as a spell hitting); Unsteady.

**Example.** Dance a brute up into the air, land all five cuts perfectly, and throw it down into the soldiers behind it, for 50 damage to each.

## wind.skydance.shape
**What it does.** Unlocks two shapes for Sky Dance. You pick one at any rest fire. **Carried Off**: there's no throw. Instead, you carry the enemy down the path and drop it on another enemy, and both take the burst. **Two Rise**: the enemy you face and the nearest other enemy both go up, and the rings take turns between them.

**When it happens.** Whenever you cast Sky Dance, using the shape you picked.

**The numbers.** 3 Shards. Carried Off: the path is 8 metres long. Two Rise: the first cut throws everything within 3 metres of the enemy into the air for 1.5 seconds.

**Works with.** Against the Wall and Indrawn Breath (Carried Off into a crowd); Nothing Beneath (Two Rise keeps two enemies up).

**Not built yet.** Carried Off should carry the enemy and drop it on another one (for now, it is thrown farther). Two Rise's rings should take turns between the two (for now, the second enemy is only lifted).

**Example.** Two soldiers stand side by side. Two Rise takes both of them up, and the landing bursts on both.

## wind.skydance.master
**What it does.** Land all five cuts perfectly, and you come down with Tailwind lit for four seconds, for free.

**When it happens.** When the last ring of Sky Dance closes and every cut was in time (all five, or all six with the Empower). Sky Dance must be in a spell slot.

**The numbers.** 3 Shards. Tailwind is lit for 4 seconds, with all it does, and costs no mana. If Tailwind is already lit, it lasts at least 4 more seconds. It works when Tailwind is your surge.

**Works with.** Passing Cut, Leaf on the Wind, Closing Circle: the fight keeps moving after you land.

**Not built yet.** What it gives when another surge is in your surge slot is still being decided. For now, it gives nothing then.

**Example.** Land five perfect cuts and throw the enemy into a pack. You land and roll right through them, farther and safer.

## wind.lift
**What it does.** Carried Far. Every reaction with Unsteady in it reaches half again as far, and throws whatever it hits away from where it happened.

**When it happens.** Whenever Unsteady reacts on an enemy with another affliction it has (Carried Embers with Burn, Windthrow, and the rest).

**The numbers.** 3 Shards. The reaction's bursts, ground, arcs and pulls reach 50% farther. Every enemy within 2.5 metres of the reaction is thrown 1.5 metres away from it.

**Works with.** Carrying a second element: every one of Wind's ten reactions; Against the Wall, for throwing enemies into things.

**Example.** Wind and Fire on a soldier in a crowd. Carried Embers goes off, and the soldiers around it are thrown back.

## wind.nothing
**What it does.** An enemy in the air stays up as long as you keep hitting it, each blow up there gives double mana, and your blows don't stop holding it up after three. The cost: your weapon is weaker against an enemy that isn't Unsteady.

**When it happens.** Always on once you learn it. The longer hang time and the mana come with each blow that lands on an enemy in the air.

**The numbers.** 6 Shards. Up to 23 blows can hold an enemy in the air, instead of three. The first adds 0.45 seconds, and each one after adds a little less, about 3 seconds more in total. Blows on an enemy in the air give twice their mana. Your weapon deals 15% less damage, but full damage against an Unsteady enemy.

**Works with.** Rising Cut, Closing Circle, Lifting Wind, Sky Dance, Unsteady.

**Example.** Lift a soldier and keep cutting. It hangs in the air for four seconds while your mana bar fills twice as fast.

## wind.devotion
**What it does.** On the Wind, Wind's Devotion, for going all in on Wind only. Your spells have no wind-up, and you can cast them in the middle of a dodge or a swing without stopping it. A spell cast during a dodge carries you farther along it. Almost the whole fight happens on the move.

**When it happens.** Only while you carry Wind twice (all in). It is always on then.

**The numbers.** 6 Shards. A spell cast during a dodge carries you 2 metres farther. Buying this also gives you the deep strike, Whirl Cut. Use it with L2 and R2 while you carry Wind alone or twice. The crescent becomes a small whirlwind that holds the first enemy it reaches in the air for a second.

**Works with.** Every spell in the Wind tree; Passing Cut and Leaf on the Wind; the deep strike.

**Not built yet.** Spells with no wind-up, cast during a dodge or a swing without stopping it, and the 2 metres added to a dodge you cast in. For now, every dodge carries you 30% farther.

**Example.** Roll at a pack, cast Indrawn Breath during the roll, and come out of it with the pack piled up in front of you.

## wind.passing
**What it does.** Where a dodge ends, a cut of wind hits everything close to you and leaves it Unsteady.

**When it happens.** At the end of every dodge, at most twice a second.

**The numbers.** 3 Shards. Everything within 2.2 metres of where you stop takes 10 damage, 8 stagger and one stack of Unsteady.

**Works with.** Closing Circle and Turned Aside, which come after it on the chart; Leaf on the Wind; Tailwind's longer dodge.

**Not built yet.** The cut should follow the path you passed through, and each enemy you passed shouldn't be able to turn for a moment. For now, the cut comes where you stop.

**Example.** Roll into the middle of three soldiers. All three are cut and Unsteady before you swing.

## wind.circle
**What it does.** You run a full circle around the enemy you face. When the circle closes, the wind inside it rises. Everything inside is hit, given five stacks of Unsteady, and thrown into the air. You end where you started, facing in, and nothing can hit you while you run.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). It needs an enemy you face within 5 metres. If there is none, it isn't cast and nothing is spent.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 7 seconds and the time in the air 1.5 seconds.) The circle is as wide as the gap between you and the enemy, with a radius of 2 to 3 metres. The run takes 0.7 seconds, and the circle snaps shut 0.2 seconds later. Everything inside takes 6 damage, 10 stagger and five stacks of Unsteady, and hangs in the air for 1.2 seconds. Enemies that can't be lifted stagger instead.

**Works with.** Passing Cut, which it comes after on the chart; Rising Cut and Nothing Beneath; Indrawn Breath, whose burst uses up the stacks it added.

**Not built yet.** For now, nothing can hit you for the whole run. The plan is for that to last only the first half.

**Example.** Two soldiers stand on either side of a brute. Run the circle around the brute, and all three go up with five stacks.

## wind.circle.shape
**What it does.** Unlocks two shapes for Closing Circle. You pick one at any rest fire. **Half Circle**: you end on the far side of the enemy, behind it. Nothing is lifted, and your first blow on its back is a critical hit. **Wide Circle**: you run around a group, and the column takes everyone inside, but only lifts each one for a moment.

**When it happens.** Whenever you cast Closing Circle, using the shape you picked.

**The numbers.** 3 Shards. Half Circle: a step through an enemy within 5 metres, dealing 6 damage and 10 stagger, and for 1.5 seconds your next blow is a critical hit. Wide Circle: it targets an enemy you face within 7 metres, the circle has a radius of up to 4 metres, and enemies stay in the air for 0.6 seconds.

**Works with.** Turned About (another way to get behind an enemy); Indrawn Breath and Against the Wall (Wide Circle around a crowd).

**Not built yet.** Half Circle should run half the circle (for now, it steps through the enemy). With the Empower, Wide Circle's short hang grows by the same 0.3 seconds the plain circle's does (0.9 seconds in all).

**Example.** A shield bearer is facing you. Use Half Circle, and the first cut lands as a critical hit on its unguarded back.

## wind.circle.master
**What it does.** If you start Closing Circle right as a blow would land on you, it counts as a perfect dodge, and you run the circle around the attacker, wherever it stands within eight metres.

**When it happens.** For now, every time you cast Closing Circle. It must be in a spell slot.

**The numbers.** 3 Shards. It only counts if you start it in the last 0.3 seconds before an enemy's blow (one within 6 metres) lands on you. Then it is a perfect dodge: you get 12 mana, the game slows for a moment, the blow misses, and every mark that answers a perfect dodge answers this one.

**Works with.** Lifting Wind's Master mark and the other marks a perfect dodge sets off; Leaf on the Wind.

**Not built yet.** Running the circle around the attacker within 8 metres.

**Example.** A spearman thrusts at you. Start the circle into it, and you run around it as the spear passes, with your mana and spell waits paid back.

## wind.turned
**What it does.** A perfect guard throws back everyone near you and gives the attacker three more stacks of Unsteady.

**When it happens.** On a perfect guard: press the guard button just before a blow lands.

**The numbers.** 3 Shards. Every enemy within 4 metres is pushed 3 metres away. The attacker gets three stacks of Unsteady.

**Works with.** Bag of Winds and Leaf on the Wind, which come after it on the chart; Rising Cut; Against the Wall.

**Not built yet.** Sending a thrown attack back at whoever threw it.

**Example.** Four soldiers surround you. Perfect-guard the first swing, and the ring is thrown back, and the one who swung has three stacks.

## wind.bag
**What it does.** You untie a bag of wind at your hip. Each blow you perfectly guard or perfectly dodge is caught in it, up to three, and thrown things that come near are caught too. Press again, and it opens: a cone that hits harder for each wind inside, and every thrown thing flies back at whoever threw it.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then it works on each perfect guard or perfect dodge while the bag holds a wind.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again (its Empower makes it 8). The bag holds three winds, shown on the screen as Winds. Each perfect guard or perfect dodge lets one out as a cone 7 metres long and 60 degrees wide, dealing 12 damage, 25 stagger and a stack of Unsteady.

**Works with.** Turned Aside, which it comes after on the chart; the Master marks of Lifting Wind and Closing Circle (more perfect dodges); Indrawn Breath.

**Not built yet.** The planned bag: it stays open for 8 seconds, each caught blow throws its attacker back 2 metres and is kept, thrown things are caught, and a second press (or the end of the time) opens it as one cone dealing 12 damage and 25 stagger for each wind inside, with the thrown things flying back. For now, casting it fills the bag, and each perfect guard or dodge lets a wind out right away. The Empower's fourth wind.

**Example.** Cast it as a brute starts its string of blows. Guard three blows perfectly, and three cones throw it back down the hall.

## wind.bag.shape
**What it does.** Unlocks two shapes for Bag of Winds. You pick one at any rest fire. **Kept Wind**: the bag doesn't open on its own. It keeps its winds for a long time, so you can open it when you choose, like on a staggered enemy. **Loose Neck**: nothing is kept. Each wind leaves right away as a free Gust toward the enemy you face next.

**When it happens.** Whenever you cast Bag of Winds, using the shape you picked. Loose Neck works on every perfect guard and perfect dodge while it is in a slot.

**The numbers.** 3 Shards. Kept Wind: 20 seconds. Loose Neck: each perfect guard or perfect dodge throws a Gust, a cone 5 metres long and 70 degrees wide, dealing 8 damage, 20 stagger and two Unsteady. An attacker you guarded against is also thrown 3 metres.

**Works with.** Turned Aside and Rising Cut (Loose Neck's Unsteady); a staggered enemy (Kept Wind).

**Not built yet.** Kept Wind does nothing beyond the bag yet. It is waiting for a bag that keeps its winds. Loose Neck's gust should aim at the enemy you face next (for now, it aims at the one that was caught).

**Example.** Use Loose Neck against a pack of duellists. Every perfect guard throws a gust into them.

## wind.bag.master
**What it does.** Catch three winds within three seconds, and the bag bursts on its own as a whirlwind. Everything within six metres goes into the air.

**When it happens.** For now, on a perfect dodge, at most once every 10 seconds. Bag of Winds must be in a spell slot.

**The numbers.** 3 Shards. Everything within 6 metres of you goes into the air for a second.

**Works with.** Rising Cut and Nothing Beneath, for the enemies in the air; Lifting Wind.

**Not built yet.** It should need three winds caught within 3 seconds to burst the bag. For now, a perfect dodge raises the whirlwind.

**Example.** Dodge a brute's swing perfectly in a crowd. Everything around you goes up, ready for your cuts.

## wind.leaf
**What it does.** Dodging costs no stamina. Instead, it uses a breath of wind. You can hold three, and you get them back over time and by landing blows. With no breath left, you can't dodge. The cost: you take more damage.

**When it happens.** Always on once you learn it. Every dodge uses a breath. The breaths come back as you fight.

**The numbers.** 6 Shards. Up to three breaths, shown on the screen as Breath. You get one back every 2 seconds while you have fewer than three, and one for every three blows you land. You take 20% more damage from every blow.

**Works with.** Passing Cut, Closing Circle, Bag of Winds, Tailwind's longer, safer dodge.

**Example.** Against a fast duellist, dodge three times in a row, then land three blows. A breath is back before the next swing comes.

## wind.wall
**What it does.** Any enemy you throw that hits a wall, a pillar or another enemy is hit hard, harder for each Unsteady it has. When two enemies crash together, both are hit. This is the payoff every throw in the Wind tree is waiting for.

**When it happens.** For now, every time one of your spells hits an enemy: Gust, any spell in your slots, and the spells your marks throw.

**The numbers.** 3 Shards. The plan: 15 damage, plus 6 more for each Unsteady, and 30 stagger. For now: a burst with a 1.6-metre radius on each enemy a spell hits, dealing 6 damage, 15 stagger and one stack of Unsteady.

**Works with.** Gust and its Long Gust shape, Indrawn Breath's All One Way shape, Sky Dance, Turned Aside, and the charged blade's Gale Edge.

**Not built yet.** The thrown enemy hitting something (a wall, a pillar, another enemy). For now, the burst comes wherever a spell hits.

**Example.** Gust three soldiers in a row. Each one takes the gust and a burst, and they shake each other with their bursts.

## wind.base
**What it does.** Gust, Wind's own spell. It comes with the element; you never buy it. A rush of air shoots out in front of you, hitting everything in it, making it Unsteady and throwing it straight back with the wind.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Wind, you can always put it in a slot.

**The numbers.** Comes with Wind. It costs no mana. After you cast it, you must wait 6 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 4.5 seconds and the Unsteady three stacks. It is a cone 5 metres long and 70 degrees wide. It deals 8 damage, 20 stagger and two stacks of Unsteady, and everything in it is thrown 3 metres.

**Works with.** Its Empower, its two shapes and its Master mark; Against the Wall; and the charged blade, which it charges with Wind when it lands.

**Example.** A soldier stands at the edge of a drop. Gust it, and it's gone.

## wind.base.shape
**What it does.** Unlocks two shapes for Gust. You pick one at any rest fire. **Downburst**: the wind comes down from above and spreads out in a ring, throwing everything nearby outward. Aim it at your feet to clear a ring around you. **Long Gust**: a narrow path that throws what it hits far away, for throwing one enemy into a wall, a pit or its friends.

**When it happens.** Whenever you cast Gust, using the shape you picked. If you have not bought this, you cast the plain gust.

**The numbers.** 3 Shards. Downburst: everything within 3 metres of you takes 8 damage, 20 stagger and two Unsteady, and is thrown 2.5 metres outward. Long Gust: a path 2 metres wide and 10 metres long that deals 8 damage, 20 stagger and two Unsteady, and throws enemies 5 metres.

**Works with.** Against the Wall (Long Gust into a wall); Passing Cut and Turned Aside when you're surrounded (Downburst).

**Not built yet.** Downburst should be aimed at a spot up to 6 metres away. For now, it always comes down on you.

**Example.** Four soldiers surround you. Downburst throws them all back with two stacks. An archer stands behind a line of soldiers: Long Gust knocks it off its ledge.

## wind.base.master
**What it does.** Cast Gust right as an enemy's attack warning shows, and the gust pushes its blow aside into empty air.

**When it happens.** For now, every enemy Gust hits. Gust must be in a spell slot.

**The numbers.** 3 Shards. Each enemy it hits flinches, which breaks whatever it had started, unless it is steadfast. The plan is for elites' blows to come 0.4 seconds late instead, and for gods not to be pushed aside.

**Works with.** Every shape of Gust; Against the Wall.

**Not built yet.** It should only work right as the attack warning shows (the game needs a way to notice an enemy's wind-up first), and elites' blows should come late. For now, any gust that lands makes its enemies flinch.

**Example.** A brute starts its red overhead swing. Gust it, and the blow falls on nothing.

## wind.indrawn
**What it does.** You breathe in, and everything nearby is dragged into one spot in front of you. Then the breath bursts out of that spot, hitting all of them as a heavy blow and using up each enemy's Unsteady, harder for every stack.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 11 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 9 seconds and pulls from 8.5 metres.) The spot is 2 metres in front of you, or on an enemy that close. For 0.5 seconds, everything within 7 metres of it is dragged most of the way in. Then the burst deals 10 damage, 25 stagger and two stacks of Unsteady to each, as a heavy blow, and each enemy's Unsteady goes off for 6 damage and 8 stagger per stack.

**Works with.** Hammer Down, which it comes after on the chart; Against the Wall; Closing Circle (five stacks on everything inside the circle); Rising Cut; and Catching Fire if you carry Fire, since it bunches up a crowd.

**Example.** Archers are spread around a room, each with a few stacks. Pull them all to one spot, and every stack they have bursts at once.

## wind.indrawn.shape
**What it does.** Unlocks two shapes for Indrawn Breath. You pick one at any rest fire. **Held Breath**: the pull lasts as long as you hold the button, and the spot moves with you. Let go to burst. **All One Way**: the burst doesn't scatter them. All of them are thrown together in the direction you face. Gather a crowd, then throw it into a wall.

**When it happens.** Whenever you cast Indrawn Breath, using the shape you picked.

**The numbers.** 3 Shards. Held Breath: up to 2 seconds. All One Way: each enemy the burst hits is thrown 5 metres straight away from you.

**Works with.** Against the Wall (All One Way), Sky Dance, Bag of Winds.

**Not built yet.** Held Breath should be held on its button, with the spot moving with you (for now, the pull always lasts the full 2 seconds). All One Way's burst shouldn't scatter them first.

**Example.** Pull a crowd into a pile, then All One Way throws the pile into the wall behind it.

## wind.indrawn.master
**What it does.** Cast Indrawn Breath in the middle of a dodge. The spot travels with you to where the dodge ends, and the burst doesn't use up any Unsteady, so every stack stays for the next blow.

**When it happens.** For now, every enemy the burst hits. Indrawn Breath must be in a spell slot.

**The numbers.** 3 Shards. Each enemy it hits gets two stacks of Unsteady back after the burst.

**Works with.** Rising Cut (back toward five stacks), Closing Circle.

**Not built yet.** It should need you to cast it in the middle of a dodge, with the spot traveling with you and the burst using nothing up. For now, each enemy gets two stacks back.

**Example.** Roll through a pack and pull it in behind you. The burst lands, and the pack stays Unsteady for your next heavy blow.

## wind.remembers
**What it does.** While your surge lasts, every weapon blow you land leaves its cut hanging in the air where it landed, up to sixteen. When the surge ends, time slows down, and all of them fall again, one right after another, each at half the strength of the blow it remembers.

**When it happens.** Light the surge with the surge button (tap L2, or press F). The cuts fall the moment it ends.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. Up to sixteen cuts. After that, each new blow is added to the weakest cut hanging there. Each falls within 1.4 metres of where it landed for half the blow's damage, 10 stagger and one stack of Unsteady. Tailwind lasts 10 seconds and costs 35 mana.

**Works with.** Tailwind, Against the Wall (each falling cut counts as a spell hitting), Nothing Beneath, Rising Cut.

**Example.** Light Tailwind and cut a pack sixteen times. As it ends, sixteen cuts fall on them again at half strength.

## wind.hammer
**What it does.** A heavy blow on an enemy in the air slams it into the ground. The blow hits 50% harder, and where the enemy lands, the ground bursts and throws back everyone nearby.

**When it happens.** Each heavy blow (finishers too) that lands on an enemy in the air or staggering, while you carry Wind.

**The numbers.** Major, 3 Shards. The heavy blow deals 50% more damage. The enemy drops right away. The burst deals 14 damage and 30 stagger to everything within 3 metres and throws them 2 metres back.

**Works with.** Against the Wall, which it comes after on the chart, and the enemies it throws. Indrawn Breath, which comes after it. Lifting Wind, Sky Dance, Rising Cut, and anything that throws enemies up.

**Example.** Throw a soldier up with Lifting Wind, then bring a heavy blow down on it. It slams into the ground and knocks its friends back.
