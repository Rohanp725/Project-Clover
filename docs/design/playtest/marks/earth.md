# Earth: detailed explanations

The long explanation the chart shows for each high-level mark of the Earth tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Earth): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Your stones" means anything of stone you raise: Stone
Rise's rocks, a pillar, a heap of rubble.

## earth.tremor
**What it does.** Your heavy blows shake the ground under an enemy and add Tremor. A finisher fills it all the way at once. A shaken enemy is slowed, and its balance doesn't come back while the shaking lasts. Also, afflictions that elites and gods put on you last half as long.

**When it happens.** Every heavy blow that lands adds a stack. Every finisher fills it. Like every root mark, your weapon only adds it while Earth is your first element. If Earth is your second element, Tremor comes from your spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. One stack for each heavy blow, three on a finisher, up to three stacks. Tremor lasts 6 seconds. Each stack slows the enemy by 10%, and while it lasts, the enemy's stagger meter doesn't go down. An affliction put on you (an elite or a god lays its land's when its blow lands) lasts half its usual time.

**Works with.** Heavy Hand and The Slow Hand (more heavy blows, or every blow shaking), Cairn (it uses up Tremor to drop stones), Left in Rubble, Bedrock (one more stack with every blow), Buckled on the Forge branch, and The Ground Gives when Soaked meets Tremor.

**Not built yet.** Earth's basic bonus from the game plan, which makes afflictions put on you last half as long. Nothing puts an affliction on the player yet.

**Example.** A heavy blow on a brute leaves its balance bar stuck where you left it. Step back, guard its swing, and come in again with the bar still waiting.

## earth.heavyhand
**What it does.** Nothing stops your heavy blows. If you are hit while winding one up or swinging it, you still take the blow, but less of it, and your blow still lands. Red attacks can't stop it either.

**When it happens.** Always on once you learn it: any blow that lands on you while you wind up or swing a heavy blow.

**The numbers.** 3 Shards. You take 35% less damage from a blow that lands then, and you don't flinch.

**Works with.** Tremor, The Slow Hand, Under the Mound and Cairn, which come after it on the chart, Hand of the Mountain, Hammer and Anvil on the Forge branch.

**Example.** Two soldiers swing at you as you wind up. You take both hits, lightly, and your heavy blow lands on the first one anyway.

## earth.mound
**What it does.** Under the Mound, the movement spell of the Earth tree. You sink into the ground and travel in the direction of the stick as a bump of rolling turf, where nothing can reach you. Then you burst out in a ring of flying dirt and stones that shakes everything around you.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 8 seconds and the reach 10 metres.) It reaches up to 8 metres. Where you come out, enemies take 12 damage, 25 stagger and one Tremor. For now, it is a step through the enemy you face (or 8 metres ahead if there is none). If it lands, your next heavy blow within 4 seconds is a critical hit.

**Works with.** Heavy Hand, Tremor and Cairn (it shakes what it reaches), The Slow Hand, and the charged blade: it charges your weapon with Earth when it lands.

**Not built yet.** Going under the ground in the direction of the stick, being untouchable on the way, and the ring of dirt 2.5 metres around where you come up. For now, it is a step through the enemy you face that hits it as you pass.

**Example.** An archer stands behind a line of shields. Go under them and come up behind it. It is shaken and open to your heavy blow.

## earth.mound.shape
**What it does.** Unlocks two shapes for Under the Mound. You pick one at any rest fire. **Ditch and Bank** (the old earth walls): the turf you push up stays behind you as a bank a metre high along your whole path. Nothing can walk through it, and enemies standing on the line as it rises are thrown off it, shaken. You draw a wall as you go. **Barrow Road**: hold the button to stay underground and steer. Come up under an enemy, and it is thrown into the air, as if by Stone Rise, and you stand behind it.

**When it happens.** Whenever you cast Under the Mound, using the shape you picked. If you have not bought this, you cast the plain spell.

**The numbers.** 3 Shards. Ditch and Bank: a line of six patches, 8 metres long, starting from where you went under. Each patch is 0.6 metres across and lasts 5 seconds. Once a second, each deals 4 damage, 10 stagger and one Tremor. Barrow Road: you stay under for up to 1.5 seconds. The enemy you come up under is thrown into the air for 0.6 seconds.

**Works with.** Ditch and Bank: Quake Step and Cairn on enemies standing on the bank. Barrow Road: Under the Mound's own Master mark, and your heavy blows on enemies in the air.

**Not built yet.** The bank should block enemies and throw them off as it rises; for now, it is ground along your path that deals damage and shakes. Barrow Road's hold and steer; for now, the spell goes where it goes, and only the throw is built.

**Example.** Use Ditch and Bank across a doorway. The pack that follows you through it comes out shaken.

## earth.mound.master
**What it does.** Go under the ground right as a blow would land on you, like a perfect dodge, and you come up behind the attacker right away. Your next blow counts as a heavy blow, adds two Tremor, and breaks 50% more balance.

**When it happens.** For now, every time you cast Under the Mound. It must be in a spell slot.

**The numbers.** 3 Shards. It only counts if you start it in the last 0.3 seconds before an enemy's blow (one within 6 metres) lands on you. Then it is a perfect dodge: you get 12 mana, the game slows for a moment, the blow misses, and every mark that answers a perfect dodge answers this one. For 3 seconds after that, or until your next blow lands, your blows break 50% more balance and each adds one more stack.

**Works with.** Tremor (the extra stack on the next blow), Heavy Hand and The Slow Hand, Cairn (the mana it gives back).

**Not built yet.** It doesn't yet bring you up behind the attacker, and the next blow doesn't yet count as a heavy blow.

**Example.** A brute's overhead swing is coming down. Go under it, come up behind it, and your next blow nearly breaks its balance.

## earth.cairn
**What it does.** Cairn, Earth's larger spell and its detonator. You stamp the end of your weapon on the ground, and the Cailleach's stones fall from her apron onto every shaken enemy near you. Each enemy gets one stone for each stack of Tremor it has. The Tremor is used up, and the stones stay piled on the enemy, slowing it.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). It costs mana instead of having a wait.

**The numbers.** 4 Shards, 40 mana (its Empower makes it 35). It hits every enemy within 10 metres that has Tremor, 0.3 seconds after you cast it. A stone falls for each stack, dealing 12 damage and 15 stagger each (three stones on full Tremor: 36 damage and 45 stagger), plus 10 more damage for the pile. The pile stays for 4 seconds and slows the enemy by 15% for each stone (45% with three stones). An enemy with no Tremor isn't touched.

**Works with.** Tremor and everything that adds it: heavy blows, finishers, Quake Step, The Slow Hand. Unbroken Cairn and your heavy blows, and Rooted for its Master mark.

**Example.** Three soldiers each have three stacks of Tremor. One stamp drops nine stones, and all three are slowed with their Tremor used up.

## earth.cairn.shape
**What it does.** Unlocks two shapes for Cairn. You pick one at any rest fire. **Unbroken Cairn**: the stones land softly and wait. Your next heavy blow on an enemy under a pile breaks the pile outward into everything near it. The burst waits for your blow. **Thrown Stones**: the stones don't stay. Each enemy's pile is thrown right away at the nearest other enemy, so one shaken enemy shakes the next.

**When it happens.** Whenever you cast Cairn, using the shape you picked. If you have not bought this, the plain stones fall.

**The numbers.** 3 Shards. Unbroken Cairn: a heavy blow on an enemy under a pile within 4 seconds bursts in a 2.5-metre circle around it, for 36 damage and 20 stagger. Thrown Stones: a stone flies from each enemy under a pile to the nearest other enemy within 5 metres, for 24 damage, 10 stagger and two Tremor.

**Works with.** Unbroken Cairn: Heavy Hand and The Slow Hand. Thrown Stones: cast Cairn again on the enemies the stones shook.

**Not built yet.** Unbroken Cairn's stones should land softly (for now they still fall when you cast), and its burst should be 18 for each stone (for now it is 36 no matter how many stacks). Thrown Stones' piles should be thrown instead of staying (for now they still fall and pile up), with a stone for each stack (for now it is two stones' worth).

**Example.** Use Unbroken Cairn on a pack around a brute. One heavy blow on the brute breaks its pile through all of them.

## earth.cairn.master
**What it does.** Cast it after standing still for a second, and every stone hits twice and you get half the mana back.

**When it happens.** For now, when you cast it while Rooted (you haven't started a dodge for 3 seconds). Cairn must be in a spell slot.

**The numbers.** 3 Shards. You get 20 mana back. A second fall hits each shaken enemy 0.8 seconds after you cast it, for 24 damage and 30 stagger.

**Works with.** Rooted, Immovable, Heavy Hand, and Mountain Stance, which keeps you standing still.

**Not built yet.** It should work after standing still for one second (for now it needs Rooted, which is three seconds without a dodge), and every stone should hit twice (for now there is a second fall worth two stones).

**Example.** Hold your ground through a soldier's string of blows, then stamp. You get twice the stones, and half the cost back for the next one.

## earth.slowhand
**What it does.** The Slow Hand, a keystone. Every swing is slower and costs more stamina, but your light blows hit like heavy ones, and every blow shakes the enemy.

**When it happens.** Always on once you learn it. The Tremor on every blow only comes while Earth is your first element.

**The numbers.** 6 Shards. Swings are 22% slower, and blows cost 20% more stamina. Light blows deal 35% more damage and 60% more stagger. Every weapon blow adds one stack of Tremor.

**Works with.** Tremor, Heavy Hand, Cairn (full Tremor on everything, ready to use up), Under the Mound's Master mark, Bedrock.

**Not built yet.** Light blows should count as heavy blows. For now, they don't break through guards, don't make enemies flinch, and don't set off anything that needs a heavy blow.

**Example.** Three light blows on a soldier leave it with three stacks of Tremor and close to staggered, without a single heavy blow.

## earth.rooted
**What it does.** Fight for a few seconds without rolling, and you become Rooted. While Rooted, you take less damage from every blow, and holding your guard costs less stamina. Starting a roll ends it, and the count starts over.

**When it happens.** Always on once you learn it. You become Rooted once you haven't started a dodge for 3 seconds.

**The numbers.** 3 Shards. While Rooted, you take 8% less damage, and a blocked blow costs 30% less stamina.

**Works with.** Quake Step and Mountain Stance, which come after it on the chart, Immovable, Heavy Hand, the Master marks of Stone Rise and Cairn (both ask you to hold your ground), and Bedrock (20% less damage on top of this while it is lit).

**Not built yet.** Nothing stops you from being pushed back while Rooted yet.

**Example.** Plant your feet in front of a duellist and block its string of blows. Each blocked blow costs less stamina, and the grey health it leaves is smaller.

## earth.quakestep
**What it does.** Quake Step, a breaker. You lift a foot and stamp it down. The ground heaves all around you and shakes everything it hits. Enemies it hits flinch, which breaks the attack they started, unless they are steadfast.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 8 seconds and the reach 6 metres.) It lands 0.45 seconds after you cast it and reaches 5 metres around you. Each enemy takes 16 damage, 30 stagger and two stacks of Tremor. An enemy that is starting an attack (its wind-up is showing) takes 30 more stagger, double in all, and its attack is broken off (a 0.4-second flinch; a god or an elite only takes the stagger).

**Works with.** Tremor and Cairn (two stacks on everything around you), Rooted, Standing Stones (a pillar on what it hits), Buckled on the Forge branch. When it lands, it charges your blade with Earth.

**Example.** Four soldiers close in around you. Stamp, and all four have two stacks of Tremor, with their balance bars stuck.

## earth.quakestep.shape
**What it does.** Unlocks two shapes for Quake Step. You pick one at any rest fire. **Struck From Afar** (the Cailleach striking the ground with her staff): the stamp doesn't land around you. Instead it lands on the shadow of an enemy far away, breaking its attack where it stands and holding it for a moment. You can stop an archer drawing its bow without crossing the room. **The Second Wave**: one stamp sends out two rings, first the quick, thin wave of an earthquake and then the slow, heavy one. Enemies caught by both are knocked off their feet.

**When it happens.** Whenever you cast Quake Step, using the shape you picked. If you have not bought this, you cast the plain ring.

**The numbers.** 3 Shards. Struck From Afar: a ring with a 2.5-metre radius around an enemy up to 8 metres away, 0.45 seconds after you cast it. It deals 16 damage, 30 stagger and two Tremor, and holds the enemy for 0.6 seconds. The Second Wave: first a ring out to 5 metres for 8 damage and one Tremor. Then, 0.7 seconds later, a ring out to 7 metres around where you stamped, for 18 damage and 40 stagger. Enemies the first ring hits are knocked down for 1 second.

**Works with.** Struck From Afar: Cairn on the enemy it shook. The Second Wave: Tremor, Unbroken Cairn, and your heavy blows on enemies lying on the ground.

**Not built yet.** Struck From Afar should land on the enemy you face (for now, the nearest within 8 metres). The Second Wave should only knock down enemies the second ring also catches, at that moment (for now, enemies the first ring catches are knocked down right away).

**Example.** Use The Second Wave in a crowd. Step back after the first ring, so the second ring catches them all as they come at you.

## earth.quakestep.master
**What it does.** Break an enemy out of its wind-up, and Quake Step is ready to cast again right away.

**When it happens.** For now, whenever Quake Step hits an enemy, at most once every 18 seconds. Quake Step must be in a spell slot.

**The numbers.** 3 Shards. When the stamp breaks an ordinary enemy out of its wind-up, Quake Step's wait ends.

**Works with.** The Second Wave (two stamps in a row), Cairn after two stamps.

**Example.** Stamp a soldier out of its swing, then stamp again as its friend comes in.

## earth.stance
**What it does.** Mountain Stance, a counter (Grey Stone: the Cailleach turning to stone each summer). You turn completely to stone and can't move. A blow that hits the stone is blocked, red ones too, and it jolts the attacker: it is shaken, and its next attack comes late. The blows the stone blocks power up your next heavy blow.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then it works on any blow while you stand as stone.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower lets you stand for up to 3 seconds.) For now, it works like this: you stand as stone for up to 2.5 seconds. The first blow to land does nothing to you. You answer with a jolt of stone toward the attacker, 3 metres long and 120 degrees wide, for 6 damage, 10 stagger and two Tremor. For 3 seconds, the attacker's wind-ups go at 0.7 times their normal speed, and for 3.5 seconds your next blow that lands breaks 50% more balance.

**Works with.** Rooted (standing still counts), Immovable, which comes after it on the chart, Heavy Hand, Cairn's Master mark, Buckled on the Forge branch.

**Not built yet.** Every blow should be blocked for as long as you stand, not only the first. Each blocked blow should add 20 stagger (25 with the Empower) to your next heavy blow within a second of letting go. The wait should be only 5 seconds if nothing hit the stone. For now, the first blow is blocked, and your next blow of any kind is powered up.

**Example.** A duellist starts its string of blows. Turn to stone and take the first cut on the stone. Its next swing comes slowly enough for you to guard.

## earth.stance.shape
**What it does.** Unlocks two shapes for Mountain Stance. You pick one at any rest fire. **Stone Skin**: the blow is blocked but not answered. Instead, two plates of stone grow on your shoulders. Each one takes most of one blow and stops you from flinching, then breaks in a burst around you. **Fist of Rock** (the stance as it was first built): you stand as stone for a short while. The first blow to land does nothing to you, and a fist of rock rises under the enemy that hit you. One blow, answered right away.

**When it happens.** Whenever you cast Mountain Stance, using the shape you picked. If you have not bought this, you cast Grey Stone.

**The numbers.** 3 Shards. Stone Skin: when the stance blocks a blow, you get two wards, up to three. Each one blocks one blow and breaks in a burst 3 metres around you, for 14 damage, 24 stagger and one Tremor. Fist of Rock: you stand for up to 1.5 seconds. The answer is a cone 4 metres long and 90 degrees wide, for 30 damage, 60 stagger and two Tremor.

**Works with.** Stone Skin: Glaze's ward if you carry Ice, Heavy Hand, Immovable. Fist of Rock: Tremor and Cairn, and Unbroken Cairn's heavy blow after it.

**Not built yet.** Stone Skin's plates should take 60% of one blow (for now, a ward takes all of it), and the stance's jolt should be turned off while you use this shape. Fist of Rock's answer should rise under the enemy that hit you; for now, it is a cone thrown at it.

**Example.** Use Stone Skin before a pack closes in. The first two blows that reach you each throw stone back at the whole ring.

## earth.stance.master
**What it does.** Block a red attack, and the attacker is stunned for a second and a half (less for a god or an elite), and Mountain Stance is ready to cast again.

**When it happens.** For now, whenever the stance blocks any blow. Mountain Stance must be in a spell slot.

**The numbers.** 3 Shards. Only a red attack counts. The attacker is held for 1.5 seconds (0.6 for a god or an elite; the colossus not at all), and the stance's wait ends.

**Works with.** Fist of Rock (a full answer on a held enemy), Cairn, and your heavy blows on the held enemy.

**Example.** A brute's red overhead swing lands on the stone. It stands stunned while you break its balance.

## earth.immovable
**What it does.** Immovable, a keystone. You stop rolling away. Your roll barely moves you, but every blow does less damage to you, guarding and dodging cost less stamina, and the timing window for a perfect guard is longer.

**When it happens.** Always on once you learn it.

**The numbers.** 6 Shards. Your roll goes a tenth as far (about half a metre). You take 16% less damage. Dodges and blocked blows cost 30% less stamina. The perfect guard window is 30% longer.

**Works with.** Mountain Stance, which it comes after on the chart, Rooted (don't roll, and both work), Heavy Hand, the Master marks of Cairn and Stone Rise (you are always holding your ground), Bedrock.

**Not built yet.** The dodge should become a brace that doesn't move you, whose first moment blocks any blow, red ones too, and Rooted should never end. For now, the roll is still a roll, it lets blows pass through it, and it still ends Rooted.

**Example.** Fight a duellist up close. Guard everything, perfect-guard what you can, and roll in place through its one red thrust.

## earth.standingstones
**What it does.** When one of your spells hits an enemy, a pillar of stone comes down on it a moment later and shakes it.

**When it happens.** Whenever one of your spells hits an enemy, Stone Rise or any spell in your slots. At most once every second and a half.

**The numbers.** 3 Shards. The pillar falls on a warning ring 0.5 seconds later. It has a 1.5-metre radius and deals 20 damage, 35 stagger and one stack of Tremor.

**Works with.** Stone Rise, which sits next to it on the chart, Left in Rubble and Fissure, Quake Step, Tremor and Cairn.

**Not built yet.** The plan changes how it works: two of your stones can stand at once, and an enemy knocked into one is hurt and breaks it. Nothing stands on the field to be knocked into yet, so the falling pillar takes its place.

**Example.** Use Stone Rise under an archer, and half a second later a pillar lands on it too.

## earth.base
**What it does.** Stone Rise, Earth's own spell. It comes with the element; you never buy it. A quarter second after you cast it, big stones burst up under the enemy you face and knock it into the air for a moment. Then they sink back into the ground. No wall is left behind.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Earth, you can always put it in a slot.

**The numbers.** Comes with Earth. It costs no mana. After you cast it, you must wait 7 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 6 seconds, the damage 18 and the stagger 36. It hits an enemy within 5 metres, in a 1.3-metre circle around it, for 14 damage, 30 stagger and one Tremor.

**Works with.** Its Empower, its two shapes and its Master mark; Tremor and Cairn; Standing Stones (a pillar on what it hits); the charged blade, which it charges with Earth when it lands.

**Example.** Raise the stone under the archer at the back. It is knocked into the air and shaken.

## earth.base.shape
**What it does.** Unlocks two shapes for Stone Rise. You pick one at any rest fire. **Raised Stone**: the pillar that lifted the enemy stays next to it, blocking enemies and thrown attacks. A heavy blow of yours on it shatters it into a cone of shards that flies away from you. Raise it, fight around it, then break it through them. **Dropped From the Apron**: a boulder falls from above onto the spot, its shadow growing, and stays there. A heavy blow on it rolls it in the direction of the blow, crushing a path, until it breaks on a wall or a large enemy.

**When it happens.** Whenever you cast Stone Rise, using the shape you picked. If you have not bought this, the plain stone rises.

**The numbers.** 3 Shards. Raised Stone: for 6 seconds, your next heavy blow on the enemy it lifted throws a cone of shards from you through it. The cone is 5 metres long and 70 degrees wide, and deals 18 damage, 20 stagger and one Tremor. Dropped From the Apron: it lands on the nearest enemy within 5 metres, 0.6 seconds after you cast it, in a 1.5-metre circle around it, for 24 damage, 45 stagger and two Tremor. For 6 seconds, a heavy blow on that enemy crushes a path 8 metres long past it, away from you, dealing 16 damage, 30 stagger and one Tremor to each enemy it hits.

**Works with.** Raised Stone: Heavy Hand, The Slow Hand. Dropped From the Apron: a pack in a hallway, Cairn on the path it shook.

**Not built yet.** Raised Stone's pillar should stay standing next to the enemy, block things, and be hit itself (for now, the stones sink back like normal, and the heavy blow lands on the enemy). Dropped From the Apron's boulder should fall from above and lie there as something to hit, breaking on a wall or a large enemy (for now, stone rises under the nearest enemy, and the heavy blow on that enemy sends the path on).

**Example.** Use Dropped From the Apron on the first of three soldiers in a hallway. One heavy blow on it crushes the two behind.

## earth.base.master
**What it does.** Cast it while you hold your ground, Rooted or standing still for a second, and the stone rises under the enemy's own feet. It throws the enemy three metres away from you with two stacks of Tremor, before the enemy can stand.

**When it happens.** For now, when you cast it while Rooted (you haven't started a dodge for 3 seconds). Stone Rise must be in a spell slot.

**The numbers.** 3 Shards. The enemy it hits is thrown 3 metres straight away from you and takes one more stack of Tremor (two, with the stone's own).

**Works with.** Rooted, Immovable, Mountain Stance, and Cairn on the two stacks it leaves.

**Not built yet.** It should also work after standing still for one second (for now, only Rooted counts), and the throw should happen before the stones sink back.

**Example.** Hold your ground, and raise the stone under a brute that got too close. It is thrown off you with two stacks.

## earth.lift
**What it does.** Left in Rubble, Earth's reaction lift. Every reaction with Tremor in it hits 20% harder and leaves a knee-high pile of rubble where it happened, with a puff of yellow-brown dust and broken stone. The pile shakes whatever stands in it.

**When it happens.** Whenever a reaction with Tremor in it happens, on any enemy.

**The numbers.** 3 Shards. The reaction's own damage is 20% higher. A pile 1 metre across that lasts 6 seconds. Once a second, it deals 3 damage, 8 stagger and one Tremor.

**Works with.** Every element pair with Tremor in it (The Ground Gives with Soaked, Chokedamp with Poison, and the rest), Stone Remembers (it lasts three times as long), and Fissure and Stone Remembers, which come after it on the chart.

**Not built yet.** The rubble should count as one of your stones, which heavy blows can knock enemies into and which The Circle Speaks can cast from.

**Example.** Soak and shake a soldier. The ground gives way under it, and the rubble left behind keeps shaking the next enemy that walks through.

## earth.fissure
**What it does.** Fissure, a breaker. You drive your weapon into the ground. A crack runs ahead of you and breaks open into a row of spikes. The spikes hit and shake whatever they catch, and hold normal enemies where they stand.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 9 seconds and the damage 26.) The crack runs 8 metres at 20 metres a second. There is a spike every 1.5 metres, starting 1.3 metres out, each with a 1-metre radius. Each enemy is hit once, for 20 damage, 35 stagger and one Tremor, and is held for 1.2 seconds (0.5 for a god or an elite).

**Works with.** Left in Rubble, which it comes after on the chart, Standing Stones, Tremor and Cairn, and your heavy blows on the enemies it holds. When it lands, it charges your blade with Earth.

**Not built yet.** The crack should glow for 0.9 seconds before it breaks open; for now, it breaks open as it runs.

**Example.** Three spearmen stand in a line down a staircase. One crack catches all three and holds them for your blade.

## earth.fissure.shape
**What it does.** Unlocks two shapes for Fissure. You pick one at any rest fire. **Basalt**: the spikes come up as a row of six-sided stone columns, like the slow-cooled stone of the giant's causeway, and stand as a wall. A heavy blow on the row knocks it over away from you, column after column, like dominoes. **Fault Line**: the crack doesn't break open on its own. It stays open, and a heavy blow you land next to it makes the whole line erupt. You lay down the line, then fight next to it.

**When it happens.** Whenever you cast Fissure, using the shape you picked. If you have not bought this, you cast the plain crack.

**The numbers.** 3 Shards. Basalt: for 4 seconds, a heavy blow on an enemy the spikes hit knocks over a path 6 metres long past it, away from you, dealing 20 damage and 30 stagger to each enemy. Fault Line: the crack itself does no damage. For 6 seconds, a heavy blow on an enemy it crossed erupts in a 2-metre circle around that enemy, for 24 damage, 40 stagger and two Tremor, at most twice a second.

**Works with.** Heavy Hand and The Slow Hand, and Tremor and Cairn on the enemies the eruption shook.

**Not built yet.** Basalt's columns should stand as a wall that blocks, and fall over one after another (for now, the heavy blow on an enemy the spikes hit sends a path on). Fault Line's open crack should show on the ground and erupt along its whole length when you land a heavy blow within 2 metres of it, at most twice (for now, a heavy blow on an enemy the crack crossed erupts under that enemy, up to twice a second).

**Example.** Use Fault Line under a pack, then fight among them. Every heavy blow erupts under the enemy it lands on.

## earth.fissure.master
**What it does.** Land a heavy blow on the crack's line while it glows, and it erupts right away, 50% harder.

**When it happens.** For now, a heavy blow on an enemy within 0.9 seconds after the spikes hit it. Fissure must be in a spell slot.

**The numbers.** 3 Shards. A second eruption in a 1-metre circle around the enemy, for 10 damage, 18 stagger and one Tremor.

**Works with.** Heavy Hand (nothing can stop the heavy blow as it lands), The Slow Hand, Tremor.

**Not built yet.** The crack's glow before it breaks open, and landing the heavy blow on its line during that glow. For now, the timing window opens when the spikes hit, and it adds a second eruption on that enemy.

**Example.** Cast the crack at a brute, and step in with a heavy blow as it breaks open. It erupts again under the brute.

## earth.devotion
**What it does.** Stone Remembers, Earth's Devotion, for going all in on Earth only. It is the rule of the Celtic land, "stone remembers", given to you. Nothing you raise falls on its own: stones, boulders, columns and slabs stay standing until something breaks them. An enemy knocked into one takes a heavy blow of stone, and the stone stays standing. The longer the fight, the more of the floor is yours.

**When it happens.** Only while you carry Earth twice (all in). It is always on then.

**The numbers.** 6 Shards. For now: whatever you leave on the ground (a spell's ground, a danger zone from your marks, Left in Rubble's piles) lasts three times as long. A heavy blow that lands hits its enemy as if it were knocked into stone, in a 1-metre circle around it, for 20 damage, 35 stagger and one Tremor, once every 3 seconds. Buying this also gives you the deep strike, Mountain Cleaver. Use it with L2 and R2 while you carry Earth alone or twice.

**Works with.** Left in Rubble, which it comes after on the chart, Fissure, Stone Rise, Standing Stones, Heavy Hand.

**Not built yet.** Stones that never fall on their own, up to eight, with the oldest crumbling away, and enemies knocked into them by a heavy blow, a finisher or a quake. Nothing stands on the field to be knocked into yet, so every heavy blow takes its place, once every 3 seconds.

**Example.** A long fight in a narrow hall. The rubble from every reaction stays, and every heavy blow lands as if into stone.

## earth.hand
**What it does.** While Bedrock is lit, a hand of stone as tall as a tower hangs over your shoulder. Each time you land a heavy blow or a finisher, the hand rises over that enemy and comes down on it a moment later, hitting everything around it.

**When it happens.** Light Bedrock with the surge button. The hand stays until Bedrock ends.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. The hand falls 0.5 seconds after the blow, in a circle with a 3-metre radius. It deals 70% of the blow's damage again, 40 stagger, counts as a heavy blow, and adds one stack of Tremor. It answers one blow at a time: a heavy blow you land while it is still falling isn't answered.

**Works with.** Bedrock, Heavy Hand, The Slow Hand, Tremor, finishers.

**Example.** Light Bedrock and finish a staggered soldier. The hand comes down on that spot and staggers the two soldiers behind it.

## earth.closedring
**What it does.** The Closed Ring, a keystone. Lighting Bedrock raises eight stones in a ring around you. For ten seconds, they hit and shake whatever stands among them, and you take a little less damage. Bedrock costs a quarter more.

**When it happens.** When you light Bedrock, the surge. Always on once you learn it, while you carry Earth.

**The numbers.** 6 Shards. Eight stones in a ring 12 metres across, each a metre wide, for 10 seconds. Once a second, each stone hits whatever stands on it for 8 damage, 20 stagger and one stack of Tremor. You take 8% less damage for the same 10 seconds. Bedrock costs 25% more mana (50 instead of 40).

**Works with.** Cairn, which it comes after on the chart (from its Master mark), Standing Stones and Left in Rubble (more stone around you), Rooted and Immovable (stand inside the ring and let them come), Hand of the Mountain.

**Not built yet.** The ring should act as a wall: nothing can walk, roll or be thrown across it while it stands, an enemy knocked into a stone is hurt and staggered, and you are Rooted inside it. For now, the stones only hit what stands on them, and anything can pass between them.

**Example.** Light Bedrock with two soldiers on you. The ring rises around all three of you, and every second they stand in it they are shaken closer to a stagger.
