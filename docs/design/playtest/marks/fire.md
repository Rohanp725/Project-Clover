# Fire: detailed explanations

The long explanation the chart shows for each high-level mark of the Fire tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Fire): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## fire.cinder
**What it does.** Every blow of your weapon that lands sets the enemy burning and lays one stack of Burn. Each further blow adds another stack, up to five. A burning enemy takes damage every second, more for each stack.

**When it happens.** Always on once learned, while Fire is your first element: every weapon blow that lands, light or heavy. Carried as the second element, it lays nothing with the weapon; Burn then comes from your Fire spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. One stack a blow, five at most. A burn deals 5 damage a second and each stack adds 30%, so 12.5 a second at five. Each blow sets its clock back to 4 seconds; when it runs out the stacks go with it. A burn set off by another mark deals 10 damage and 10 more for each stack: 60 at five.

**Works with.** Heat, Breath of the Lioness (its cone sets a burn off), Catching Fire, Meteor Leap's Master, and Kindle.

**Example.** Five quick light blows on a soldier leave it five deep, burning for 12.5 a second while you step back and raise your guard.

## fire.heat
**What it does.** Landing blows without being struck builds Heat, a level at a time, up to five. Each level makes your weapon hit harder. A blow that lands on you puts it out, and standing off too long cools it a level at a time.

**When it happens.** Always on once learned. Every weapon blow that lands counts; lighting Kindle with the surge button fills it at once.

**The numbers.** 3 Shards. Three blows a level, five levels. Each level adds 6% weapon damage, 30% at five. Six seconds without landing a blow loses a level. A blow that lands on you drops it to nothing.

**Works with.** White Heat, Wildfire Rush (a level for each enemy passed), Kindle, and the two spells that grow from it, Uraeus and Meteor Leap.

**Example.** Fifteen blows in, every cut is 30% heavier. One careless swing from a brute and you are back to nothing.

## fire.base
**What it does.** Flame Wave, Fire's own spell, comes with the element and is never bought. Fire runs out along the ground in a cone ahead of you, striking everything in it and setting it burning, and three patches of fire burn on where it ran.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Fire, it is always yours to put in a slot.

**The numbers.** Given with Fire. No mana: it waits 7 seconds after a cast, less with weapon play; its Empower makes that 5.5. The cone is 5.5 metres long and 70 degrees wide: 20 damage, 24 stagger and a stack of Burn. Three patches burn on for 3 seconds.

**Works with.** Its Empower, its two shapes and its Master; Cinder to deepen what it lit; the charged blade, which its landing charges with Fire.

**Not built yet.** The Empower's longer patches (5 seconds): the base spell is cast by Fire's kit, which takes only the Empower's shorter wait.

**Example.** Open a fight with it across a line of soldiers: every one of them burning before your first blow.

## fire.base.shape
**What it does.** Opens two shapes of Flame Wave. Choose which it wears at any rest fire, in the Hand. **Cinder Fan**: close and wide, the shape for being surrounded; what it touches burns two deep and is thrown back. **Running Fire**: one narrow tongue of flame runs far along the ground, and a low burning line stays behind it; whatever crosses it takes two Burn.

**When it happens.** Whenever you cast Flame Wave, as the shape it wears. Before this is bought, the plain wave is cast.

**The numbers.** 3 Shards. Cinder Fan: 3 metres and 150 degrees, 22 damage, 20 stagger, two Burn, and a throw of 1.5 metres. Running Fire: 12 metres in under a second, 12 damage and 10 stagger with two Burn on what the tongue touches, and the line burns 4 seconds, biting 3 every half second with two Burn.

**Works with.** Catching Fire and Black Pitch in a crowd (Cinder Fan); Breath of the Lioness and Kiln Stance against a pack that closes in; Fuel to It on wet ground (Running Fire across a river makes steam).

**Example.** Surrounded by four soldiers, Cinder Fan throws them all back burning two deep. Facing an archer behind a wall of shields, Running Fire reaches it.

## fire.base.master
**What it does.** Cast Flame Wave in the same instant as the charged blade's third blow lands, and its cooldown does not start.

**When it happens.** The third blow of a charged blade lands: for half a second after it, your next Flame Wave starts no wait. Flame Wave must be in a slot.

**The numbers.** 3 Shards. A window of 0.5 seconds after the third charged blow. Then the wave lands, charges the blade again, and the dance goes on.

**Works with.** The charged blade above all; Cinder and Heat, which pay for staying in.

**Not built yet.** The wave leaving from the blade along the swing; for now it leaves from you as always.

**Example.** Wave, three blows, wave again at once: a burning crowd that never gets its feet.

## fire.uraeus
**What it does.** A cobra of flame rises from your shoulder and stands for a while, spitting burning venom at enemies near you.

**When it happens.** From a spell slot: cast it with that slot's button. It spits by itself while it stands.

**The numbers.** 4 Shards, no mana: it waits 10 seconds (its Empower: 8). It stands 8 seconds and spits at enemies within 8 metres, six spits at most (its Empower: eight), each 8 damage, 6 stagger and two Burn.

**Works with.** Heat, which it grows from; Cinder; its Master on what already burns.

**Not built yet.** The spit answering an enemy that begins an attack (it needs a trigger on an enemy's wind-up), and the venom burning at the target's feet. For now the cobra spits on its own, like a phalanx of fire.

**Example.** Cast it before a pack closes in, and every one that comes near takes venom while you fight the first.

## fire.uraeus.shape
**What it does.** Opens two shapes of Uraeus, chosen at any rest fire. **Coiled**: it stays close and spits only at what is within 3 metres, each spit laying three Burn. **Set Down**: it is planted where you cast it and guards a circle of 9 metres round itself whether you stay or not: leave it on a door.

**When it happens.** Whenever you cast Uraeus, as the shape it wears.

**The numbers.** 3 Shards. Coiled: reach 3 metres, three Burn a spit. Set Down: a spit every 1.2 seconds at an enemy within 9 metres of the cobra, 8 damage and two Burn, for 8 seconds.

**Works with.** Kiln Stance and Breath of the Lioness (Coiled, for fighting close); Black Pitch and Catching Fire (Set Down by a crowd).

**Not built yet.** Coiled's spit holding the enemy's wind-up for a moment.

**Example.** Set it down at a gate and fight the brute on the far side of the yard while the cobra keeps the soldiers burning.

## fire.uraeus.master
**What it does.** Each spit that lands on an enemy already burning lays twice the Burn: the fire is fed.

**When it happens.** A spit of your Uraeus strikes an enemy that is already burning. Uraeus must be in a slot.

**The numbers.** 3 Shards. Two more stacks of Burn on top of the spit's own.

**Works with.** Cinder and Flame Wave, which light them first.

**Not built yet.** The cobra standing a second longer for each spit fed.

**Example.** Light a pack with Flame Wave, then cast Uraeus: every spit takes them a step nearer to five.

## fire.meteor
**What it does.** You leap out of the fight and come down beside the enemy you face as a falling stone, a heavy blow that leaves a burning crater. Nothing can strike you in the air.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, 45 mana (its Empower: 35). It finds an enemy faced within 9 metres and lands you 1 metre short of it; with none, 5 metres ahead. The leap takes 0.62 seconds. The landing strikes everything within 3.2 metres for 46 damage and 55 stagger. The crater burns for 4 seconds and bites for 5 every half second.

**Works with.** Heat, which it grows from; Catching Fire; its Master on an enemy at five Burn.

**Not built yet.** The Empower's longer crater (6 seconds).

**Example.** An archer stands behind two shield-bearers: leap over them and land on it.

## fire.meteor.shape
**What it does.** Opens two shapes of Meteor Leap, chosen at any rest fire. **Falling Star**: you do not leap; a burning stone falls on the aim point after a breath and you go on fighting. **Ash Fall**: the landing throws hot ash round you, and enemies in it keep catching fire.

**When it happens.** Whenever you cast Meteor Leap, as the shape it wears.

**The numbers.** 3 Shards. Falling Star: it falls 0.8 seconds after the cast, within 9 metres, 46 damage and 60 stagger in 3.2 metres, a stack of Burn, and a burning patch for 4 seconds. Ash Fall: ash 3 metres round where you land for 5 seconds, a Burn and 2 damage a second to all in it.

**Works with.** Cinder and Catching Fire (Ash Fall in a crowd); Kiln Stance and Breath of the Lioness (Falling Star while you hold your ground).

**Not built yet.** Falling Star's own stone and growing shadow (it uses the thrown sphere for now), and balance not recovering in Ash Fall.

**Example.** Hold the line with your guard and call the stone down on the brute behind it.

## fire.meteor.master
**What it does.** Land Meteor Leap on an enemy at five Burn and its burn goes off with the blow, and half the mana comes back.

**When it happens.** The leap's landing strikes an enemy at full Burn. Meteor Leap must be in a slot.

**The numbers.** 3 Shards. The burn is set off for 10 damage a stack (50 at five). 22 mana comes back, once a landing.

**Works with.** Cinder and Uraeus, to bring an enemy to five; Heat for the weight behind it.

**Example.** Five quick blows on the brute, step back, leap: its burn goes off under the stone and your mana is half full again.

## fire.lift
**What it does.** Fuel to It: every reaction with Burn in it leaves the ground where it happened burning.

**When it happens.** Whenever Burn reacts on an enemy with the other affliction it carries (Steam with Soaked, Flashover with Static and the rest).

**The numbers.** 3 Shards. A burning patch 1.5 metres in radius for 3 seconds, biting 3 every half second with a stack of Burn.

**Works with.** Carrying a second element: every reaction of Fire's ten.

**Not built yet.** The reactions themselves striking 25% harder (it needs a stat for reaction damage).

**Example.** Fire and Water on one enemy: Steam bursts, and the ground under it keeps burning.

## fire.whiteheat
**What it does.** Heat can rise to eight. Every level above five also quickens your swing. But struck while above five, you catch fire yourself for a while.

**When it happens.** Always on once learned. The speed comes with each level above five; the burn comes when a blow lands on you up there.

**The numbers.** 6 Shards. Heat to eight, 48% weapon damage at the top. Each level above five adds 5% swing speed, 15% at eight. Struck above five, you burn for 4 seconds at 4 damage a second; it never takes your last point. The blow still puts your Heat out, as always.

**Works with.** Heat, Wildfire Rush, and Kindle, which now fills Heat to eight at once.

**Example.** Light Kindle at the start of a fight: you are at eight Heat straight away, swinging 15% faster for 48% more, as long as nothing touches you.

## fire.devotion
**What it does.** Fire Without End, Fire's Devotion: all in on Fire only. Burns never go out by themselves; only death or water ends them. A burn left alone deepens by itself. Light everything once, then fight the one in front of you while the rest of the room cooks.

**When it happens.** Only while Fire is carried twice (all in), once bought. Then always, on every burning enemy.

**The numbers.** 6 Shards. A burn's time no longer runs down. Every burning enemy that has had no Burn laid on it for 4 seconds gains a stack, and another 4 seconds later another, up to five. A burn set off still goes, and Steam (Soaked meeting Burn) still takes half of it. All in, L2 with R2 is the deep strike, Burnt Offering: 20 mana on its own 8-second clock.

**Works with.** Cinder, Flame Wave, Catching Fire, Breath of the Lioness, which sets a burn off, and the deep strike, whose pillar sets off the burn it lands on.

**Example.** Flame Wave across a room, then a duel with one soldier: the rest are at five when you turn round.

## fire.backdraft
**What it does.** Breath of the Lioness (once Backdraft): a perfect guard throws a cone of fire back at the one who struck, as Sekhmet's breath was the desert wind. Against a burning enemy, its burn goes off too.

**When it happens.** A perfect guard: press the guard button just before a blow lands.

**The numbers.** 3 Shards. A cone 4.5 metres long and 100 degrees wide. 16 damage and 18 stagger to all in it, and each burns for 3 seconds. Two stacks of Burn on the attacker. A burning attacker's burn goes off for 10 damage and 10 more for each stack.

**Works with.** Cinder to build the burn it sets off, Kiln Stance (whose turned blows count as perfect guards), Flame Trail.

**Example.** Against a soldier's thrust, guard just before it lands: the fire throws it off balance; at five Burn, its burn goes off in the same breath.

## fire.kiln
**What it does.** You stand ready in flame. The first blow that reaches you, from any side, red attack or not, is turned for nothing and answered with fire. The caught blow counts as a perfect guard, so it gives that mana and sets off every mark that waits on one. If nothing comes, the stance goes out and the cast is spent.

**When it happens.** From a spell slot: cast it with that slot's button, then any blow in the next 1.2 seconds.

**The numbers.** 4 Shards, no mana: it waits 9 seconds (its Empower: 7, and four Burn). Ready for 1.2 seconds. The answer is a pillar of fire 1.7 metres in radius under the attacker: 30 damage, 50 stagger, a heavy blow, three stacks of Burn.

**Works with.** Breath of the Lioness, which it grows from; its Master on a burning attacker.

**Not built yet.** Shut Door as designed: held up to 3 seconds, every blow turned banked as smoke under a hood (three at most), and the door opened in a cone of flame when you let go, 20 damage and 15 more for each blow banked. It needs a spell held on its button. For now the Learn is the stance as it was built, The Kiln.

**Example.** A brute winds up a red overhead you cannot guard: cast just before it falls, and the pillar answers it.

## fire.kiln.shape
**What it does.** Opens two shapes of Kiln Stance, chosen at any rest fire. **The Kiln**: the stance as it was, the first blow answered at once by a pillar of fire, for single blows. **Firewalker**: a stance you walk in, for a shorter time: walk into the swing.

**When it happens.** Whenever you cast Kiln Stance, as the shape it wears.

**The numbers.** 3 Shards. The Kiln: ready 1.2 seconds, the pillar 30 damage, 50 stagger, three Burn. Firewalker: ready 0.6 seconds.

**Works with.** Breath of the Lioness; Uraeus Coiled.

**Not built yet.** Walking at half speed in Firewalker (for now it is the stance held a shorter time).

**Example.** Against a quick double cut, The Kiln answers the first; against a slow brute, Firewalker meets the swing.

## fire.kiln.master
**What it does.** Turn a blow from an attacker already burning, and a pillar of fire rises under it with the answer: its burn goes off and it is thrown down.

**When it happens.** Kiln Stance's answer strikes a burning enemy. Kiln Stance must be in a slot.

**The numbers.** 3 Shards. Its burn goes off for 10 damage a stack, and it lies knocked down for 1.2 seconds (elites and gods less).

**Works with.** Cinder and Flame Wave to light the attacker first.

**Example.** Light the brute, wait for its overhead, answer it: it goes up and down at once.

## fire.trail
**What it does.** Your dodge leaves a line of fire on the ground behind it that burns whatever walks through.

**When it happens.** Every dodge.

**The numbers.** 3 Shards. Four patches along the dodge's path, each 0.95 metres in radius, laid as you roll. Each burns for 2.6 seconds and bites for 4 damage every half second, setting what it touches burning for 2 seconds.

**Works with.** Wildfire Rush, which grows from it, for a second line; Breath of the Lioness.

**Example.** Roll back from a charging brute: it runs through all four patches to reach you and arrives burning.

## fire.rush
**What it does.** You become a point of fire and drive straight through every enemy in your path, coming out the far side facing back. Each enemy passed is struck, set burning and raises your Heat a level. Then a second cut lands on everyone on the line, and the line burns. Nothing can strike you while you drive; a wall, or something too big to shove, stops you.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 8 seconds after a cast (its Empower: 6, and the line burns 4 seconds). Up to 7 metres, along a lane 2 metres wide. Passing: 14 damage, 20 stagger and two stacks of Burn. The cut after: 14 damage, 20 stagger and two stacks more. The line burns 2.6 seconds, biting for 4 every half second. One Heat level for each enemy passed.

**Works with.** Flame Trail, which it grows from; Heat and White Heat; Cinder; Catching Fire.

**Example.** Drive through three soldiers in a row: Heat goes from none to three, and each is four deep in Burn.

## fire.rush.shape
**What it does.** Opens two shapes of Wildfire Rush, chosen at any rest fire. **Fire Front**: you go as the front of a grass fire, a wall of flame, slower, and what it meets is thrown down at the end; the drive that pushes a crowd instead of passing through it. **Smoke Road**: the escape, the line of fire left between you and them.

**When it happens.** Whenever you cast Wildfire Rush, as the shape it wears.

**The numbers.** 3 Shards. Fire Front: 5 metres, 4 metres wide, a third of the speed; each enemy struck is knocked down for 0.6 seconds. Smoke Road: 6 metres, the passing cut lighter (4 damage).

**Works with.** Catching Fire and Black Pitch (Fire Front into a crowd); Flame Trail (Smoke Road out of one).

**Not built yet.** Fire Front carrying the enemies before you, and Smoke Road going back out of the fight: for now both drive forward.

**Example.** Fire Front through a doorway packed with soldiers: they go down burning in a heap on the far side.

## fire.rush.master
**What it does.** Started in the instant a blow would land on you, Wildfire Rush counts as a perfect dodge, and your Heat goes to five.

**When it happens.** You cast Wildfire Rush. Wildfire Rush must be in a slot.

**The numbers.** 3 Shards. For now every cast gives what a perfect dodge gives: 12 mana and 1.5 seconds off every running cooldown.

**Works with.** Heat and White Heat.

**Not built yet.** The timing (started as a blow would land) and Heat going to five: both need the enemy's blow as a moment.

**Example.** Drive out of a brute's swing and through its friends, and every other spell is that much nearer.

## fire.firebomb
**What it does.** Holding the flask button throws the flask instead of drinking it. It heals nothing: it bursts in fire where it lands, leaves flames on the ground there, and lays three stacks of Burn on everything near. A short press still drinks.

**When it happens.** Hold the flask button. Each throw uses a flask.

**The numbers.** 3 Shards. Hold 0.3 seconds to throw. It flies at the enemy ahead within 9 metres, or 5 metres ahead, and lands 0.4 seconds later. The burst is 2.6 metres in radius: 36 damage, 30 stagger, three stacks of Burn and 4 seconds of burning. The flames left are 2.2 metres in radius and bite for 5 every half second for 3.5 seconds.

**Works with.** Catching Fire, Black Pitch, Flame Wave (all on the Ruin bough).

**Example.** Three soldiers bunch at a gate: one flask leaves all three three deep and standing in fire.

## fire.catch
**What it does.** When a burning enemy dies, its burn is thrown onto everyone standing near it, as deep as it was.

**When it happens.** The moment a burning enemy dies, whatever killed it.

**The numbers.** 3 Shards. Every enemy within 4 metres of the one that died takes as many stacks of Burn as it carried, up to five. It can happen at most five times a second.

**Works with.** Cinder to build the depth first, Black Pitch (which grows from it), Firebomb Flask, and Breath of the Lioness to set the new burns off.

**Example.** Kill a soldier at five Burn in the middle of a pack, and the two beside it are five deep at once without a blow from you.

## fire.pitch
**What it does.** You throw a jar of embalmer's pitch at the enemy faced. Your next blows on it splash fire over everyone near it.

**When it happens.** From a spell slot: cast it with that slot's button. Then the next three blows of your weapon on the coated enemy, within 8 seconds.

**The numbers.** 4 Shards, no mana: it waits 11 seconds (its Empower: 9, and the coat lasts 10 seconds). Thrown up to 7 metres. Each of three blows on it sets off a burst 2 metres round it: 16 damage, 35 stagger and three Burn.

**Works with.** Catching Fire, which it grows from; Cinder; Firebomb Flask.

**Not built yet.** The pitch as designed: blows spread a coat from enemy to enemy without fire, and the first fire that touches any coated enemy (a Burn laid, a burning patch, a reaction) takes all of them at once, 16 damage and 8 more for each blow that spread it, up to 70, and the floor under each burns. For now the coat is a brand that the next blows set off round the one coated.

**Example.** Coat the soldier in the middle of a pack and cut it three times: the pack burns three deep.

## fire.pitch.shape
**What it does.** Opens two shapes of Black Pitch, chosen at any rest fire. **Pitch Pool**: thrown on the floor, a pool of burning pitch. **The Torch**: one enemy coated deep, and nothing splashes; it burns and sets alight everyone near it each second as it walks.

**When it happens.** Whenever you cast Black Pitch, as the shape it wears.

**The numbers.** 3 Shards. Pitch Pool: 3 metres across, 8 seconds, biting 3 every half second with a stack of Burn. The Torch: for 8 seconds, every second, a stack of Burn on everyone within 2 metres of the torch; its blows set off a burst of 0.8 metres.

**Works with.** Catching Fire (The Torch into a crowd); Flame Wave's Running Fire across the pool.

**Not built yet.** The pool coating whoever walks through it, and The Torch lit only when fire touches it (for now it is lit at once).

**Example.** Make the brute The Torch and let it walk into its friends.

## fire.pitch.master
**What it does.** Light it with a finisher, and every other burning enemy near goes up with the coated, at half.

**When it happens.** A finisher lands on a burning enemy. Black Pitch must be in a slot.

**The numbers.** 3 Shards. The finished enemy's burn is set off for 5 damage a stack in 4 metres.

**Works with.** Cinder, Catching Fire.

**Not built yet.** Every other burning enemy within 10 metres going up at half, and only when the finisher lights the coat.

**Example.** End a coated soldier's chain with a finisher, and the fire runs on to the next.

## fire.sun
**What it does.** Lighting Kindle starts the sun on its road. At dawn it is rolled to the enemy you face, and the ground it crosses burns. At noon it hangs over that enemy, and burns near you deepen by themselves. At dusk it sets on the enemy below, heavier for every burn it finds. A blow on you sends it under, to rise again.

**When it happens.** Light Kindle with the surge button. It sets when Kindle ends, or when you press the surge button again.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. Dawn: up to 9 metres; the first enemy met takes 14 damage, 20 stagger, two Burn. Noon: each second, a stack of Burn on all within 6 metres of you. Dusk: 60 damage, 80 stagger in 4 metres, 8% more a Burn stack found, up to double; half if called while rolling. Sent under, it rises in 1.5 seconds if Kindle has 3 left.

**Works with.** Cinder, Heat.

**Example.** Press again over a crowd at five Burn.
