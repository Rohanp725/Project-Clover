# Fire: detailed explanations

The long explanation the chart shows for each high-level mark of the Fire tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Fire): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## fire.cinder
**What it does.** Every weapon blow that lands sets the enemy on fire and adds one stack of Burn. Each blow after that adds another stack, up to five. A burning enemy takes damage every second, and more for each stack.

**When it happens.** Always on once you learn it, while Fire is your first element: every weapon blow that lands, light or heavy. If Fire is your second element, your weapon adds nothing. Burn then comes from your Fire spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. One stack per blow, up to five. A burn deals 5 damage a second, and each stack adds 30%, so it deals 12.5 a second at five stacks. Each blow resets its timer to 4 seconds. When the timer runs out, all the stacks go away. When another mark sets off a burn, it deals 10 damage plus 10 more for each stack: 60 at five stacks.

**Works with.** Heat, Breath of the Lioness (its cone sets off a burn), Catching Fire, Meteor Leap's Master mark, and Kindle.

**Example.** Five quick light blows on a soldier give it five stacks. It burns for 12.5 a second while you step back and raise your guard.

## fire.heat
**What it does.** Landing blows without getting hit builds up Heat, one level at a time, up to five. Each level makes your weapon hit harder. A blow that lands on you puts it out, and staying out of the fight too long cools it down one level at a time.

**When it happens.** Always on once you learn it. Every weapon blow that lands counts. Lighting Kindle with the surge button fills it right away.

**The numbers.** 3 Shards. Three blows for each level, up to five levels. Each level adds 6% weapon damage, so 30% at five. Going six seconds without landing a blow loses a level. A blow that lands on you drops it to zero.

**Works with.** White Heat, Wildfire Rush (a level for each enemy you pass through), Kindle, and the two spells that come after it on the chart, Uraeus and Meteor Leap.

**Example.** Fifteen blows in, every cut does 30% more damage. One careless moment and a brute's swing puts you back to zero.

## fire.base
**What it does.** Flame Wave, Fire's own spell. It comes with the element; you never buy it. Fire runs along the ground in a cone in front of you, hitting everything in it and setting it on fire. Three patches of fire keep burning where it ran.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Fire, you can always put it in a slot.

**The numbers.** Comes with Fire. It costs no mana. After you cast it, you must wait 7 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 5.5 seconds and the patches burn for 5 seconds. The cone is 5.5 metres long and 70 degrees wide. It deals 20 damage, 24 stagger and a stack of Burn. Three patches keep burning for 3 seconds.

**Works with.** Its Empower, its two shapes and its Master mark; Cinder, to add more Burn to what it set on fire; and the charged blade, which it charges with Fire when it lands.

**Example.** Start a fight by casting it across a line of soldiers. Every one of them is burning before your first blow.

## fire.base.shape
**What it does.** Unlocks two shapes for Flame Wave. You pick one at any rest fire, in the Hand. **Cinder Fan**: short and wide, the shape for when you are surrounded. Whatever it touches gets two stacks of Burn and is thrown back. **Running Fire**: one narrow tongue of flame runs far along the ground, and leaves a low burning line behind it. Anything that crosses the line gets two Burn.

**When it happens.** Whenever you cast Flame Wave, using the shape you picked. If you have not bought this, you cast the plain wave.

**The numbers.** 3 Shards. Cinder Fan: 3 metres long and 150 degrees wide. It deals 22 damage, 20 stagger and two Burn, and throws enemies back 1.5 metres. Running Fire: it runs 12 metres in under a second. Whatever the flame touches takes 12 damage, 10 stagger and two Burn. The line burns for 4 seconds, dealing 3 damage and two Burn every half second.

**Works with.** Catching Fire and Black Pitch in a crowd (Cinder Fan). Breath of the Lioness and Kiln Stance against a pack that closes in. Fuel to It on wet ground (Running Fire across a river makes steam).

**Example.** Surrounded by four soldiers, Cinder Fan throws them all back with two stacks of Burn. Facing an archer behind a wall of shields, Running Fire can reach it.

## fire.base.master
**What it does.** Cast Flame Wave at the same moment the charged blade's third blow lands, and its wait doesn't start.

**When it happens.** When the third blow of a charged blade lands, your next Flame Wave within half a second has no wait. Flame Wave must be in a spell slot.

**The numbers.** 3 Shards. You have 0.5 seconds after the third charged blow. Then the wave lands, charges the blade again, and the cycle keeps going.

**Works with.** The charged blade most of all, plus Cinder and Heat, which reward you for staying in the fight.

**Not built yet.** The wave should come out of the blade along the swing. For now, it comes out of you, like normal.

**Example.** Wave, three blows, and wave again right away: a burning crowd that never gets back on its feet.

## fire.uraeus
**What it does.** A cobra made of flame rises from your shoulder and stays for a while, spitting burning venom at enemies near you.

**When it happens.** Cast it from a spell slot. Press that slot's button. It spits on its own while it is there.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again (its Empower makes it 8). It stays for 8 seconds and spits at enemies within 8 metres, up to six times (its Empower makes it eight). Each spit deals 8 damage, 6 stagger and two Burn.

**Works with.** Heat, which it comes after on the chart; Cinder; and its Master mark on enemies that are already burning.

**Not built yet.** The spit should answer an enemy that starts an attack (the game needs a way to notice an enemy's wind-up), and the venom should burn at the enemy's feet. For now, the cobra spits on its own, like a guard made of fire.

**Example.** Cast it before a pack closes in. Every enemy that comes near gets hit with venom while you fight the first one.

## fire.uraeus.shape
**What it does.** Unlocks two shapes for Uraeus. You pick one at any rest fire. **Coiled**: it stays close and only spits at enemies within 3 metres, and each spit adds three Burn. **Set Down**: it is planted where you cast it and guards a 9-metre circle around itself, whether you stay or not. Leave it at a door.

**When it happens.** Whenever you cast Uraeus, using the shape you picked.

**The numbers.** 3 Shards. Coiled: it reaches 3 metres, and adds three Burn per spit. Set Down: it spits every 1.2 seconds at an enemy within 9 metres of the cobra, for 8 damage and two Burn, for 8 seconds.

**Works with.** Kiln Stance and Breath of the Lioness (Coiled, for fighting up close). Black Pitch and Catching Fire (Set Down next to a crowd).

**Not built yet.** Coiled's spit should freeze the enemy's wind-up for a moment.

**Example.** Set it down at a gate, and fight the brute on the far side of the yard while the cobra keeps the soldiers burning.

## fire.uraeus.master
**What it does.** Each spit that lands on an enemy that is already burning adds double the Burn. The fire is fed.

**When it happens.** When your Uraeus's spit hits an enemy that is already burning. Uraeus must be in a spell slot.

**The numbers.** 3 Shards. Two more stacks of Burn on top of the spit's own.

**Works with.** Cinder and Flame Wave, which set enemies on fire first.

**Not built yet.** The cobra should stay a second longer for each spit that feeds the fire.

**Example.** Set a pack on fire with Flame Wave, then cast Uraeus. Every spit brings them a step closer to five stacks.

## fire.meteor
**What it does.** You leap out of the fight and come down next to the enemy you face like a falling stone. The landing is a heavy blow that leaves a burning crater. Nothing can hit you while you are in the air.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards, 45 mana (its Empower makes it 35). It finds the enemy you face within 9 metres and lands you 1 metre in front of it. If there is none, you land 5 metres ahead. The leap takes 0.62 seconds. The landing hits everything within 3.2 metres for 46 damage and 55 stagger. The crater burns for 4 seconds (6 with its Empower) and deals 5 damage every half second.

**Works with.** Heat, which it comes after on the chart; Catching Fire; and its Master mark on an enemy with five Burn.

**Example.** An archer stands behind two shield bearers. Leap over them and land on it.

## fire.meteor.shape
**What it does.** Unlocks two shapes for Meteor Leap. You pick one at any rest fire. **Falling Star**: you don't leap. Instead, a burning stone falls where you aim a moment later, and you keep fighting. **Ash Fall**: the landing throws hot ash around you, and enemies in it keep catching fire.

**When it happens.** Whenever you cast Meteor Leap, using the shape you picked.

**The numbers.** 3 Shards. Falling Star: it falls 0.8 seconds after you cast it, within 9 metres. It deals 46 damage, 60 stagger and a stack of Burn within 3.2 metres, and leaves a burning patch for 4 seconds. Ash Fall: ash covers 3 metres around where you land for 5 seconds. Every second, it deals a Burn and 2 damage to everything in it. Enemies the landing hits can't get their balance back for those 5 seconds.

**Works with.** Cinder and Catching Fire (Ash Fall in a crowd). Kiln Stance and Breath of the Lioness (Falling Star while you hold your ground).

**Not built yet.** Falling Star's own stone and growing shadow (for now, it uses the thrown ball). An enemy that walks into Ash Fall's ash later doesn't yet lose its balance recovery.

**Example.** Hold the line with your guard, and call the stone down on the brute behind it.

## fire.meteor.master
**What it does.** Land Meteor Leap on an enemy with five Burn, and its burn goes off with the blow. You also get half the mana back.

**When it happens.** When the leap's landing hits an enemy with full Burn. Meteor Leap must be in a spell slot.

**The numbers.** 3 Shards. The burn goes off for 10 damage per stack (50 at five). You get 22 mana back, once per landing.

**Works with.** Cinder and Uraeus, to bring an enemy up to five; Heat, for extra damage behind it.

**Example.** Five quick blows on the brute, step back, and leap. Its burn goes off under the stone, and your mana is half full again.

## fire.lift
**What it does.** Fuel to It. Every reaction with Burn in it hits 25% harder and leaves the ground where it happened on fire.

**When it happens.** Whenever Burn reacts on an enemy with another affliction it has (Steam with Soaked, Flashover with Static, and the rest).

**The numbers.** 3 Shards. The reaction's own damage is 25% higher. A burning patch with a 1.5-metre radius that lasts 3 seconds. Every half second, it deals 3 damage and a stack of Burn.

**Works with.** Carrying a second element: every one of Fire's ten reactions.

**Example.** Fire and Water on one enemy: Steam bursts, and the ground under it keeps burning.

## fire.whiteheat
**What it does.** Heat can go up to eight. Every level above five also makes you swing faster. But if you get hit while above five, you catch fire yourself for a while.

**When it happens.** Always on once you learn it. The extra speed comes with each level above five. The burn comes when a blow lands on you while you are above five.

**The numbers.** 6 Shards. Heat goes up to eight, which gives 48% more weapon damage at the top. Each level above five adds 5% swing speed, so 15% at eight. If you are hit above five, you burn for 4 seconds, taking 4 damage a second, but it never takes your last point. The blow still puts your Heat out, like always.

**Works with.** Heat, Wildfire Rush, and Kindle, which now fills Heat up to eight right away.

**Example.** Light Kindle at the start of a fight. You are at eight Heat right away, swinging 15% faster for 48% more damage, as long as nothing hits you.

## fire.devotion
**What it does.** Fire Without End, Fire's Devotion, for going all in on Fire only. Burns never go out on their own; only death or water ends them. A burn you leave alone grows stronger by itself. Set everything on fire once, then fight the enemy in front of you while the rest of the room cooks.

**When it happens.** Only while you carry Fire twice (all in), once you buy it. Then always, on every burning enemy.

**The numbers.** 6 Shards. A burn's timer no longer runs down. Every burning enemy that hasn't had Burn added for 4 seconds gains a stack, and another stack 4 seconds after that, up to five. A burn that is set off still goes away, and Steam (Soaked meeting Burn) still takes half of it. Buying this also gives you the deep strike, Burnt Offering. Use it with L2 and R2 while you carry Fire alone or twice. It costs 20 mana and has its own 8-second wait.

**Works with.** Cinder, Flame Wave, Catching Fire, Breath of the Lioness (which sets off a burn), and the deep strike, whose pillar sets off the burn on the enemy it lands on.

**Example.** Cast Flame Wave across a room, then fight one soldier alone. When you turn around, the rest are at five stacks.

## fire.backdraft
**What it does.** Breath of the Lioness (it used to be called Backdraft). A perfect guard throws a cone of fire back at the enemy that hit you, the way the goddess Sekhmet's breath was the desert wind. If the enemy is burning, its burn goes off too.

**When it happens.** On a perfect guard: press the guard button just before a blow lands.

**The numbers.** 3 Shards. A cone 4.5 metres long and 100 degrees wide. It deals 16 damage and 18 stagger to everything in it, and each one burns for 3 seconds. The attacker gets two stacks of Burn. A burning attacker's burn goes off for 10 damage plus 10 more for each stack.

**Works with.** Cinder, to build up the burn it sets off; Kiln Stance (the blows it blocks count as perfect guards); Flame Trail.

**Example.** A soldier thrusts at you. Guard just before it lands, and the fire knocks it off balance. At five Burn, its burn goes off at the same moment.

## fire.kiln
**What it does.** Shut Door. Hold the button, and you stand ready, wrapped in flame. Every blow that reaches you while you hold it, from any side, red attack or not, does nothing to you. Each one counts as a perfect guard, so it gives you that mana and sets off every mark that needs a perfect guard. Each is stored as smoke under a hood. Let go, and the door opens: a cone of flame that grows with every blow you stored. If no blow came, nothing comes out.

**When it happens.** Cast it from a spell slot and keep holding that slot's button. It lasts while you hold it, up to 3 seconds. A quick tap still holds for a quarter of a second.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again (its Empower makes it 7 and adds a fourth Burn). It stores up to three blows. When you let go, the door is a cone 6 metres long and 90 degrees wide. It deals 20 damage plus 15 for each stored blow (65 with three), 50 stagger and three Burn, as a heavy blow.

**Works with.** Breath of the Lioness, which it comes after on the chart, and its Master mark on a burning attacker.

**Example.** A duellist starts a string of three cuts. Hold the stance through all three, then let go: the door opens on it for 65 damage.

## fire.kiln.shape
**What it does.** Unlocks two shapes for Kiln Stance. You pick one at any rest fire. **The Kiln**: the stance as it was first built. A pillar of fire answers the first blow right away; it's best for single blows. **Firewalker**: a stance you can walk in, for a shorter time. Walk into the enemy's swing.

**When it happens.** Whenever you cast Kiln Stance, using the shape you picked.

**The numbers.** 3 Shards. The Kiln: a tap, not a hold. It is ready for 1.2 seconds. The first blow is answered at once by a pillar of fire that deals 30 damage, 50 stagger and three Burn. Firewalker: the held stance, but it lasts only 0.6 seconds before the door opens, and you can walk at half speed while it lasts.

**Works with.** Breath of the Lioness; Uraeus's Coiled shape.

**Example.** Against a quick double cut, The Kiln answers the first one. Against a slow brute, Firewalker meets the swing.

## fire.kiln.master
**What it does.** Block a blow from an attacker that is already burning, and a pillar of fire rises under it as your answer. Its burn goes off, and it is knocked down.

**When it happens.** When Kiln Stance's answer hits a burning enemy. Kiln Stance must be in a spell slot.

**The numbers.** 3 Shards. Its burn goes off for 10 damage per stack, and it is knocked down for 1.2 seconds (less for elites and gods).

**Works with.** Cinder and Flame Wave, to set the attacker on fire first.

**Example.** Set the brute on fire, wait for its overhead swing, and answer it. It goes up in flames and down at the same time.

## fire.trail
**What it does.** Your dodge leaves a line of fire on the ground behind you that burns anything walking through it.

**When it happens.** Every dodge.

**The numbers.** 3 Shards. Four patches along the dodge's path, each with a 0.95-metre radius, left as you roll. Each burns for 2.6 seconds and deals 4 damage every half second, setting whatever it touches on fire for 2 seconds.

**Works with.** Wildfire Rush, which comes after it on the chart, for a second line; Breath of the Lioness.

**Example.** Roll back from a charging brute. It runs through all four patches to reach you, and arrives burning.

## fire.rush
**What it does.** You turn into a point of fire and charge straight through every enemy in your path, coming out the far side facing back. Each enemy you pass is hit, set on fire, and raises your Heat by one level. Then a second cut lands on everyone on the line, and the line burns. Nothing can hit you while you charge. A wall, or an enemy too big to push past, stops you.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 8 seconds to cast it again (its Empower makes it 6, and makes the line burn for 4 seconds). It goes up to 7 metres, along a path 2 metres wide. As you pass, each enemy takes 14 damage, 20 stagger and two stacks of Burn. The cut after deals 14 damage, 20 stagger and two more stacks. The line burns for 2.6 seconds, dealing 4 damage every half second. You get one Heat level for each enemy you pass.

**Works with.** Flame Trail, which it comes after on the chart; Heat and White Heat; Cinder; Catching Fire.

**Example.** Charge through three soldiers in a row. Your Heat goes from zero to three, and each soldier has four stacks of Burn.

## fire.rush.shape
**What it does.** Unlocks two shapes for Wildfire Rush. You pick one at any rest fire. **Fire Front**: you move as the front of a grass fire, a wall of flame. It is slower, and whatever it meets is knocked down at the end. It pushes a crowd instead of passing through it. **Smoke Road**: the escape. It leaves a line of fire between you and them.

**When it happens.** Whenever you cast Wildfire Rush, using the shape you picked.

**The numbers.** 3 Shards. Fire Front: it goes 5 metres, is 4 metres wide, and moves at a third of the speed. Every enemy it hits is pushed along in front of you, a step ahead, and thrown down at the end for 0.8 seconds (an elite for less; gods and bosses are too big to carry). Smoke Road: it goes 6 metres, and the cut as you pass is weaker (4 damage).

**Works with.** Catching Fire and Black Pitch (Fire Front into a crowd); Flame Trail (Smoke Road out of one).

**Not built yet.** Smoke Road should take you back out of the fight. For now, it charges forward.

**Example.** Use Fire Front through a doorway packed with soldiers. They fall down burning in a pile on the far side.

## fire.rush.master
**What it does.** If you start Wildfire Rush right as a blow would land on you, it counts as a perfect dodge, and your Heat goes to five.

**When it happens.** When you cast Wildfire Rush. Wildfire Rush must be in a spell slot.

**The numbers.** 3 Shards. It only counts if you start it in the last 0.3 seconds before an enemy's blow (one within 6 metres) lands on you. Then it is a perfect dodge: you get 12 mana, the game slows for a moment, the blow misses, and every mark that answers a perfect dodge answers this one. Your Heat also rises five levels, to five if it was empty.

**Works with.** Heat and White Heat.

**Example.** Charge out of a brute's swing and through its friends, and every other spell is that much closer to ready.

## fire.firebomb
**What it does.** Holding the flask button throws the flask instead of drinking it. It doesn't heal. It bursts into fire where it lands, leaves flames on the ground, and puts three stacks of Burn on everything nearby. A short press still drinks it.

**When it happens.** Hold the flask button. Each throw uses up a flask.

**The numbers.** 3 Shards. Hold for 0.3 seconds to throw. It flies at the enemy in front of you within 9 metres, or 5 metres ahead, and lands 0.4 seconds later. The burst has a 2.6-metre radius and deals 36 damage, 30 stagger, three stacks of Burn and 4 seconds of burning. The flames left behind have a 2.2-metre radius and deal 5 damage every half second for 3.5 seconds.

**Works with.** Catching Fire, Black Pitch, Flame Wave (all on the Ruin bough).

**Example.** Three soldiers bunch up at a gate. One flask gives all three three stacks and leaves them standing in fire.

## fire.catch
**What it does.** When a burning enemy dies, its burn jumps onto every enemy standing near it, with as many stacks as it had.

**When it happens.** The moment a burning enemy dies, no matter what killed it.

**The numbers.** 3 Shards. Every enemy within 4 metres of the one that died gets as many stacks of Burn as it had, up to five. This can happen at most five times a second.

**Works with.** Cinder, to build up the stacks first; Black Pitch (which comes after it on the chart); Firebomb Flask; and Breath of the Lioness, to set off the new burns.

**Example.** Kill a soldier with five Burn in the middle of a pack. The two next to it get five stacks right away, without you hitting them.

## fire.pitch
**What it does.** You throw a jar of embalmer's pitch (a thick, sticky tar) at the enemy you face. Your next blows on it splash fire over everyone near it.

**When it happens.** Cast it from a spell slot. Press that slot's button. Then it works on your next three weapon blows on the coated enemy, within 8 seconds.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 11 seconds to cast it again (its Empower makes it 9, and makes the coat last 10 seconds). You can throw it up to 7 metres. Each of the three blows on the coated enemy sets off a burst 2 metres around it, for 16 damage, 35 stagger and three Burn.

**Works with.** Catching Fire, which it comes after on the chart; Cinder; Firebomb Flask.

**Not built yet.** The planned pitch: your blows spread the coat from enemy to enemy without lighting it. Then the first fire that touches any coated enemy (a Burn, a burning patch, or a reaction) sets all of them on fire at once, for 16 damage plus 8 more for each blow that spread it, up to 70, and the floor under each one burns. For now, the coat is a mark that your next blows set off around the coated enemy.

**Example.** Coat the soldier in the middle of a pack and cut it three times. The pack burns with three stacks.

## fire.pitch.shape
**What it does.** Unlocks two shapes for Black Pitch. You pick one at any rest fire. **Pitch Pool**: it is thrown on the floor as a pool of burning pitch. **The Torch**: one enemy is coated thickly, and nothing splashes. It burns, and every second it sets fire to everyone near it as it walks.

**When it happens.** Whenever you cast Black Pitch, using the shape you picked.

**The numbers.** 3 Shards. Pitch Pool: 3 metres across, lasts 8 seconds, and deals 3 damage and a stack of Burn every half second. The Torch: for 8 seconds, every second, everyone within 2 metres of the torch gets a stack of Burn. Your blows on it set off a burst 0.8 metres across.

**Works with.** Catching Fire (send The Torch into a crowd); Flame Wave's Running Fire across the pool.

**Not built yet.** The pool should coat anyone who walks through it, and The Torch should only light up when fire touches it (for now, it lights up right away).

**Example.** Turn the brute into The Torch and let it walk into its friends.

## fire.pitch.master
**What it does.** Light it with a finisher, and every other burning enemy nearby goes up in flames along with the coated one, at half strength.

**When it happens.** When a finisher lands on a burning enemy. Black Pitch must be in a spell slot.

**The numbers.** 3 Shards. The finished enemy's burn goes off for 5 damage per stack within 4 metres.

**Works with.** Cinder, Catching Fire.

**Not built yet.** Every other burning enemy within 10 metres should go up at half strength, and only when the finisher lights the coat.

**Example.** End a string of blows on a coated soldier with a finisher, and the fire spreads to the next one.

## fire.sun
**What it does.** Lighting Kindle starts the sun on its path across the sky. At dawn, it rolls toward the enemy you face, and the ground it crosses burns. At noon, it hangs over that enemy, and burns near you grow stronger on their own. At dusk, it sets on the enemy below it, hitting harder for every burn it finds. If you get hit, the sun goes down, and then it rises again.

**When it happens.** Light Kindle with the surge button. It sets when Kindle ends, or when you press the surge button again.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. Dawn: it rolls up to 9 metres, and the first enemy it meets takes 14 damage, 20 stagger and two Burn. Noon: every second, everything within 6 metres of you gets a stack of Burn. Dusk: 60 damage and 80 stagger within 4 metres, plus 8% more for each Burn stack it finds, up to double. If you call it while it is still rolling, it does half. If you get hit, it goes down and rises again 1.5 seconds later, if Kindle has 3 seconds left.

**Works with.** Cinder, Heat.

**Example.** Press the surge button again over a crowd with five Burn each.

## fire.intothefire
**What it does.** Fire never backs off. When you dodge toward an enemy, you pass right through it without being hit, and everything you pass catches fire. Dodging away costs more stamina and can never be a perfect dodge.

**When it happens.** Every dodge, while you carry Fire. A dodge counts as toward the enemy you face when it goes within 60 degrees of the direct line to it. It counts as away when it goes within 60 degrees of the line directly away from it. A dodge to the side is a normal dodge.

**The numbers.** Keystone, 6 Shards. Toward: you can't be hit for the whole dodge, and you pass through enemies (walls still stop you). Every enemy along your path takes two Burn. A perfect dodge toward the enemy also raises your Heat by one level. Away: the dodge costs twice the stamina, and it is never a perfect dodge, so it gives no mana and nothing that needs a perfect dodge happens.

**Works with.** Flame Trail, which it comes after on the chart. Heat, which the perfect dodge builds up. Wildfire Rush. Big attacks and red attacks, which you now meet by going right through them.

**Example.** A brute winds up a huge swing. Roll into it. You come out behind it, it is burning, and you are already swinging.

## fire.drinks
**What it does.** Your weapon stops setting enemies on fire. Instead, it drinks the fire that's already there. Each blow on a burning enemy takes Burn off it and hits harder for each Burn it takes. Burns no longer hurt enemies on their own.

**When it happens.** Every weapon blow on a burning enemy, while you carry Fire.

**The numbers.** Keystone, 6 Shards. A light blow takes 1 Burn, a heavy blow takes 3, and a finisher takes all of them. Each Burn taken adds 6 damage to that blow. Cinder no longer adds Burn to your blows. A burn left on an enemy does no damage over time, but it still runs out at its normal speed.

**Works with.** Catching Fire, which it comes after on the chart. Everything that sets fire without your weapon: Flame Wave, Uraeus, the Firebomb Flask, Fire's infused strike, and reactions. Fire Without End, which keeps the fuel from running out.

**Example.** Throw a Flame Wave over a crowd for two Burn each, then finish the enemy in front of you. The finisher takes both Burn and hits for 12 more damage.
