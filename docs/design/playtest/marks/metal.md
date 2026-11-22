# Metal: detailed explanations

The long explanation the chart shows for each high-level mark of the Metal tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Metal): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "A notch" is one stack of Sundered; "your blades" means
the steel you throw and plant: Flung Blade, the blades of Grave of Swords.

## metal.sunder
**What it does.** Heavy blows and perfect guards put notches in an enemy's armour, and a finisher puts in three at once. Each notch is a stack of Sundered, and each one lets your weapon cut deeper.

**When it happens.** Always on once you learn it: every heavy blow that lands, every perfect guard (on the enemy that hit you), and every finisher. Light blows don't notch. Like every root mark, your weapon only adds it while Metal is your first element (or your only one). The perfect guard's notch works either way. If Metal is your second element, Metal's notches come from your spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. One notch for each heavy blow or perfect guard, three for a finisher, up to three. Sundered lasts 8 seconds. Each notch adds 8% to your weapon's damage on that enemy, so 24% at three. It doesn't make spells stronger.

**Works with.** Counter Cut and critical hits (they multiply each other), Poured Bronze (it uses up the notches for bronze), Every Crack Rings, Temper (one more notch with every blow), Flung Blade, and Live Iron when Static meets the notches.

**Example.** Two heavy blows on a shield bearer give its armour two notches. Every cut after that does 16% more damage for the next 8 seconds.

## metal.countercut
**What it does.** A blow that lands while an enemy is in the middle of its own attack, winding up or swinging, is a critical hit.

**When it happens.** Always on once you learn it: any weapon blow that lands on an enemy in the middle of its attack, or while it is grabbing you.

**The numbers.** 3 Shards. A critical hit deals one and a half times its damage. The planned extra stagger isn't in yet.

**Works with.** Sunder (a critical hit and Sundered multiply each other), Poured Bronze, Grave of Swords and Master's Answer, which come after it on the chart, and Halo of Blades on the Halo branch (each critical hit makes a blade).

**Example.** A brute raises its hammer. Step in and land a heavy blow before it comes down, for one and a half times the damage.

## metal.bronze
**What it does.** Poured Bronze, the detonator of the Metal tree. You raise a hand, and over every notched enemy near you, a pot of melted bronze tips over in the air and pours into its cracks. The more notches it had, the harder it is hit. The notches are used up, and the bronze hardens: the enemy is held under a crust that cools from cherry red to black.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Bronze only pours into enemies that have Sundered.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 15 damage for each notch.) It reaches every enemy within 8 metres, 0.4 seconds after you cast it. Each notched enemy takes 10 damage, plus 12 damage and 15 stagger for each notch it had (46 damage and 45 stagger at three). Its notches are used up, and it is held for 0.4 seconds for each notch it had (1.2 seconds at three). A god or an elite is held for a quarter of that: 0.1 seconds for each notch.

**Works with.** Sunder, Open Line and Forged Answer (they notch on a perfect guard), Every Crack Rings, the charged blade (its charged blows notch), and Grave of Swords and Returning Edge, which notch a crowd for it to pour into.

**Not built yet.** The pot of bronze tipping over each enemy and the crust cooling on it aren't shown yet.

**Example.** Give three soldiers three notches each with heavy blows and Grave of Swords, then pour. Each takes 46 damage and is held while you choose which to finish.

## metal.bronze.shape
**What it does.** Unlocks two shapes for Poured Bronze. You pick one at any rest fire. **Set**: the crust stays. A heavy blow on a crusted enemy shatters it, throwing bronze splinters out behind it that notch whatever they hit. Pour, then break. **Red-Hot** (Talos, the bronze giant, heating himself up): the bronze stays melted in the cracks. There is no hold, but every weapon blow on that enemy spits sparks and deals more damage for each notch that was used up. Pour, then keep attacking.

**When it happens.** Whenever you cast Poured Bronze, using the shape you picked. If you have not bought this, you cast the plain pour.

**The numbers.** 3 Shards. Set: the crust stays for 4 seconds. The next heavy blow on it shatters it for 20 damage and 15 stagger within 1 metre, and sends a path of splinters 3 metres out behind it, away from you. Each enemy the splinters reach takes 8 damage, 8 stagger and a notch. Only the enemies the bronze poured into get the crust or the melted bronze. Red-Hot: there is no hold. For 4 seconds, every weapon blow on the enemy spits sparks for 6 more damage for each notch the pour used up (three notches: 18 more).

**Works with.** Set: heavy weapons, Counter Cut, a pack standing in a line behind the enemy you poured into. Red-Hot: fast weapons, the charged blade, Open Line (a held enemy to keep attacking).

**Example.** Use Set on a notched brute with two soldiers behind it. One heavy blow shatters the crust, and the splinters notch the two behind it, ready for the next pour.

## metal.bronze.master
**What it does.** Cast it within a second after a perfect guard, and the pour on the attacker is doubled, and Poured Bronze is ready again.

**When it happens.** When you cast Poured Bronze within 1 second after a perfect guard. Poured Bronze must be in a spell slot.

**The numbers.** 3 Shards. The cast doesn't start a wait, so you can pour again right away. The enemy whose blow you guarded takes its pour twice: 12 more damage and 15 more stagger for each notch.

**Works with.** Sunder, Open Line and Forged Answer (the same perfect guard notches, holds and makes a blade), Edge Guard (the longer perfect timing window), Master's Answer.

**Example.** Perfect-guard a duellist's thrust and pour right away. It is held under the bronze, and the pour is ready again for the next one.

## metal.graveofswords
**What it does.** Grave of Swords, Metal's larger spell. Twelve blades fall in a ring around you and stand there. Each light blow you land sends the nearest one through that enemy, and a heavy blow sends all that are left. For now, the blades hang behind your shoulders and fly on their own, one after another, at the nearest enemy, and a heavy blow that lands sends all that are left at the enemy it hit.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). It is Metal's larger spell, paid for with mana.

**The numbers.** 4 Shards, 50 mana (its Empower makes it 45 mana and sixteen blades). Twelve blades for 12 seconds. For now, the first flies 0.6 seconds after you cast it, then one every 0.55 seconds at the nearest enemy within 6 metres. Each hits for 9 damage, 6 stagger and one notch of Sundered. Any not sent when the time is up disappear.

**Works with.** Sunder, Forged Answer on the heavy blow that sends the rest, Poured Bronze (a notched crowd for it), To the Blade (its blades are blades to fly to), Every Crack Rings.

**Not built yet.** The twelve should fall in a ring 4.5 metres around you, dealing 8 damage under each, and stand there. A light blow should send the nearest one through its enemy, and only every third blade should notch (for now, every blade notches).

**Example.** Cast it in a pack and keep fighting. The blades keep three notches on whoever is nearest, and a heavy blow on the brute sends the last five into it.

## metal.graveofswords.shape
**What it does.** Unlocks two shapes for Grave of Swords. You pick one at any rest fire. **Clashing Bronze** (the Kouretes clashing bronze around the baby Zeus): the blades don't fly; they ring. Each light blow you land makes the nearest standing blade ring, sending a sharp ripple along the floor that notches everything it reaches. A heavy blow rings them all at once, and they fall. Every blow rings. **The Tripods** (the smith's moving machines): three bronze tripods roll out, with four blades on each. They go to notched enemies first and cut them. A heavy blow you land calls each one home, throwing its blades through whatever it passes.

**When it happens.** Whenever you cast Grave of Swords, using the shape you picked. If you have not bought this, you cast the plain blades.

**The numbers.** 3 Shards. Clashing Bronze: for 12 seconds, every weapon blow on an enemy that was within 8 metres when you cast it rings out 3 metres around it, for 8 damage, 15 stagger and a notch, at most five times a second. A heavy blow on one of them also rings 4.5 metres around you, for 16 damage, 30 stagger and a notch, once a second. The Tripods: three helpers for 6 seconds. Each one cuts the nearest enemy every half second for 9 damage, 6 stagger and a notch.

**Works with.** Clashing Bronze: fast weapons, Counter Cut, Poured Bronze on everything it rang. The Tripods: Poured Bronze, Open Line (the held enemy is cut for you).

**Not built yet.** Clashing Bronze's blades should stand in their ring and ring instead of flying (for now, they still fly, and the ringing comes from your blows on enemies that were near when you cast it). The Tripods should go to notched enemies first, notch only every third cut (for now, every cut), and be called home by a heavy blow through what they pass.

**Example.** Use Clashing Bronze in a crowd. Every light blow sends a ripple through the three enemies around the one you hit.

## metal.graveofswords.master
**What it does.** A finisher sends every blade that's left, and you get 20 mana back.

**When it happens.** Your first finisher that lands on an enemy that was within 8 metres when you cast Grave of Swords, while its blades still stand; once per cast. Other heavy blows don't count. Grave of Swords must be in a spell slot.

**The numbers.** 3 Shards. 20 mana back. The heavy blow already sends every blade that's left.

**Works with.** Sunder (the finisher adds three notches), Counter Cut, Forged Answer, Temper (the mana for it).

**Example.** Cast it, stagger the brute, and finish it. The last blades go in, and you get 20 mana back.

## metal.mastersanswer
**What it does.** For two seconds after a perfect guard, your weapon's skill costs no mana and comes out right away, with no time to charge. At any other time, it costs 50% more.

**When it happens.** Always on once you learn it. A perfect guard starts the two seconds; press the skill button during them.

**The numbers.** 6 Shards. You get two seconds of a free skill that doesn't need charging. Outside of those, the skill costs 1.5 times its mana.

**Works with.** Counter Cut, which it comes after on the chart, Open Line (the attacker is still held when the skill lands), Edge Guard, Forged Answer, Poured Bronze's Master mark, and, on the Katana, the Iaido Slash and Answering Draw.

**Example.** With the Katana, perfect-guard a thrust, and the Iaido Slash leaves the sheath right away, for free.

## metal.openline
**What it does.** A perfect guard knocks the attacker's weapon wide. It is held still for a moment, and for a short time after the guard, your weapon hits harder.

**When it happens.** Every perfect guard: press the guard button just before a blow lands.

**The numbers.** 3 Shards. The attacker is held for 1.2 seconds (less for elites; enemies that can't be frozen aren't held at all). Your weapon deals 15% more damage for 1.5 seconds after the guard, on any enemy. For now, the hold is all it does: the enemy can still raise its guard, and blows on the held enemy don't count as hitting from behind.

**Works with.** Sunder (a notch from the same guard), Forged Answer, Counter Cut, Master's Answer (the free skill comes in the same moment), and Returning Edge and Circle of Steel, which come after it on the chart.

**Example.** Perfect-guard a brute's swing, and it stands frozen with its arm thrown wide. A heavy blow lands before it can move.

## metal.returningedge
**What it does.** Returning Edge, a breaker. A spinning blade a metre and a half across flies out in the direction you face and comes back to your hand, wherever you are by then, cutting every enemy it passes, both ways.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 16 damage and 14 stagger, going out and coming back.) It travels 8 metres at 16 metres a second, reaching 0.75 metres to each side of its line. Going out, it deals 12 damage, 10 stagger and a notch to each enemy it passes. Coming back, it deals 12 damage and 10 stagger to each enemy it passes on the way home.

**Works with.** Open Line, which it comes after on the chart, Sunder, Poured Bronze on the line it notched, and the charged blade (it charges your weapon with Metal).

**Not built yet.** Its own look: for now, the turning blade is two plain blades crossed and spinning flat.

**Example.** Four soldiers come down a hallway. The blade runs through them and back, and all four are notched, ready for the pour.

## metal.returningedge.shape
**What it does.** Unlocks two shapes for Returning Edge. You pick one at any rest fire. **Bat It Home**: guard during the perfect timing window as the blade comes back, and it goes out again, faster and harder, up to three times. **Through the Line**: it doesn't come back on its own. It stops at the end of its run and spins in place, and your next dodge calls it home along the line, cutting everything in between. Get the enemies between you and the blade.

**When it happens.** Whenever you cast Returning Edge, using the shape you picked. If you have not bought this, you cast the plain blade.

**The numbers.** 3 Shards. Bat It Home: when the blade reaches your hand, a guard inside the perfect timing window sends it out again the way you now face, at 1.4 times the speed (22.4 metres a second) and 1.5 times the damage (18 damage, 15 stagger), up to three times. Through the Line: the blade stops 8 metres out and spins for 3 seconds, dealing 8 damage every 0.3 seconds to anything within 0.9 metres of it. Your next dodge calls it home, cutting every enemy on the line between it and you for 20 damage, 30 stagger and a notch. If you don't dodge in those 3 seconds, it falls and is gone.

**Works with.** Bat It Home: Edge Guard and Open Line (the perfect guard), Forged Answer. Through the Line: a fight with lots of dodging, Poured Bronze on the line, Counter Cut.

**Example.** Use Through the Line down a hallway. Let the soldiers walk into the spinning blade, then the blade comes home through them.

## metal.returningedge.master
**What it does.** Catch it with a perfect guard as it comes home, and Returning Edge is ready again right away.

**When it happens.** When the blade reaches your hand and your guard is inside its perfect timing window. Returning Edge must be in a spell slot.

**The numbers.** 3 Shards. The wait ends right away.

**Works with.** Edge Guard and Open Line, Bat It Home, Forged Answer (the same guard makes a blade).

**Example.** Throw it, perfect-guard the brute's swing, and throw it again right away.

## metal.circle
**What it does.** Circle of Steel, the counter. You raise your blade for a moment and can't move. The first blow that reaches you, from any side, red attack or not, does nothing to you, and you answer with a cut all around you that notches everything nearby.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then it works on any blow in the next 1.2 seconds.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 16 damage, and after the first blow you stay ready: a second blow in the same 1.2 seconds is blocked and answered too.) The answer hits everything within 3 metres, all the way around, for 14 damage, 20 stagger and one notch, as a heavy blow. For now, the blocked blow doesn't count as a perfect guard for your marks: it gives no mana and doesn't set off anything that needs one. If nothing hits you, the wait is half as long: 5 seconds.

**Works with.** Open Line, which it comes after on the chart, Edge Guard, which comes after it, Sunder, Poured Bronze on the ring of enemies it notched, and the charged blade (the answer is a spell, so it charges your weapon with Metal).

**Example.** Three soldiers surround you. Cast it as the first one lunges. The cut blocks its blow and notches all three.

## metal.circle.shape
**What it does.** Unlocks two shapes for Circle of Steel. You pick one at any rest fire. **The Golden Chair** (the smith's chair that trapped the queen of the gods): a blocked blow closes bands of gold around the attacker, a faint shimmer that turns bright as they lock. It is held, gets two notches, and your blows on it go through any guard. **Ringing Plate**: you stand like a struck metal plate. Every blow you block rings out in a sharp ripple on the floor, each ring wider than the last. The more enemies hit you, the better.

**When it happens.** Whenever you cast Circle of Steel, using the shape you picked. If you have not bought this, you cast the plain circle.

**The numbers.** 3 Shards. The Golden Chair: the answer is a cut all around you, 3 metres, for 14 damage, 20 stagger and a notch to everything it hits. Only the enemy whose blow you blocked is bound: it is held for 2 seconds (a god or an elite for 0.4 of that), gets one more notch, and can't raise its guard for 2 seconds. Ringing Plate: ready for 1.5 seconds. Every blow you block in that time rings out, up to four. Each ring deals 8 damage, 20 stagger and a notch all around you. The first ring reaches 4 metres, and each one after it reaches 1 metre further: 5, 6, then 7.

**Works with.** The Golden Chair: Open Line, Poured Bronze (two notches and a held enemy), Counter Cut. Ringing Plate: being surrounded, Every Crack Rings.

**Example.** Use The Golden Chair on a shield bearer's charge. It stands trapped in gold with its shield down while you cut.

## metal.circle.master
**What it does.** Block a red attack, and you get 15 mana back and Circle of Steel is ready again right away.

**When it happens.** For now, any blow the circle blocks, once every 20 seconds. Circle of Steel must be in a spell slot.

**The numbers.** 3 Shards. Only a red attack counts. The wait ends right away, and you get 15 mana.

**Works with.** Edge Guard, Temper (the mana for it), Ringing Plate.

**Example.** A god's red sweep comes. Raise the circle, block it, and raise it again for the next one.

## metal.edgeguard
**What it does.** Your guard is all edge. Held, it blocks nothing: only a perfect guard turns a blow. In return, the perfect guard timing window is 50% longer, you can press guard again sooner, and every perfect guard cuts the attacker and notches its armour.

**When it happens.** Always on once you learn it. The cut comes with every perfect guard.

**The numbers.** 6 Shards. The perfect timing window grows by 55%, from 0.18 seconds to about 0.28. Each perfect guard hits everything within 1.2 metres of the attacker for 12 damage, 10 stagger and one notch of Sundered, and counts as a light blow landed: it gives you that blow's mana and takes that blow's time off your waits. A held guard lets every blow through (and costs no stamina). You can press guard again 0.2 seconds after the last press and still get a perfect window, instead of 0.4.

**Works with.** Circle of Steel, which it comes after on the chart, Sunder (two notches from one guard), Forged Answer, Open Line, Master's Answer, every Master mark that works on a perfect guard, Hanging Swords, and Bright Steel and Halo of Blades on the Halo branch.

**Example.** Fight a fast duellist with just the guard button. Each of its blows you meet at the right moment is blocked, cut and notched.

## metal.forgedanswer
**What it does.** Each perfect guard makes a blade out of the blow you blocked, and it hangs at your shoulder. Up to three can wait there. Your next heavy blow that lands sends all of them at the enemy it hit.

**When it happens.** A perfect guard makes one. The next heavy blow that lands sends all of them. The count shows on the screen as Blades.

**The numbers.** 3 Shards. Up to three blades. Each flies at 16 metres a second, turning to follow the enemy, and hits for 10 damage and 12 stagger. For now, the blades don't add a notch of their own.

**Works with.** Sunder (the heavy blow that sends them adds a notch too), Edge Guard (easier perfect guards), Open Line, Every Crack Rings, which comes after it on the chart, and Flung Blade's Master mark.

**Example.** Perfect-guard a duellist's flurry of three blows, then answer with one heavy blow. Three blades follow it in for 30 more damage and 36 stagger.

## metal.base
**What it does.** Flung Blade, Metal's own spell. It comes with the element; you never buy it. A blade flies into the enemy you face and sticks in it. Your next weapon blow on that enemy drives it in with a burst of sparks. Then it tears free and flies back to your hand, cutting along the way.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Metal, you can always put it in a slot.

**The numbers.** Comes with Metal. It costs no mana. After you cast it, you must wait 6 seconds to cast it again. Fighting with your weapon makes the wait shorter. It targets an enemy within 9 metres. It deals 3 damage when it lands, and stays stuck for 6 seconds. When your next weapon blow on that enemy drives it in, it deals 12 damage, 15 stagger, a notch, and a burst of sparks 1.5 metres around, plus a third as much to anything on its path home. Only one blade sticks in one enemy: throw another at it, and the first flies home. If it hits no one, it sticks in the ground for a moment and then flies home. Its Empower makes the drive hit for 16, lets up to three blades stick in one enemy, and makes a heavy blow drive all of them in at once (a light blow drives in the oldest).

**Works with.** Its Empower, its two shapes and its Master mark; Sunder, Poured Bronze, To the Blade (it is a blade to fly to), and the charged blade, which it charges with Metal when it lands.

**Example.** Throw the blade into the archer at the back, then close in and cut it. The blade bursts out of it and flies home through the soldier in between.

## metal.base.shape
**What it does.** Unlocks two shapes for Flung Blade. You pick one at any rest fire. **Cherry Red**: three red-hot blades fly in a fan, each sticking in a different enemy. They glow cherry red and cool to black, and one you drive in while it is still red hits double. You throw wide, then hurry. **The Nail** (the bronze nail in the giant Talos's ankle): one long nail is driven low into the enemy, and it doesn't come back. Each heavy blow on that enemy drives it deeper, and on the third, the enemy drops to one knee while golden blood runs out around it. Three heavy blows on one enemy.

**When it happens.** Whenever you cast Flung Blade, using the shape you picked. If you have not bought this, you throw the plain blade.

**The numbers.** 3 Shards. Cherry Red: your normal blade and two more, each flying into a different enemy within 9 metres (not the one you face, and not one that already has a blade in it). Each one deals 3 damage when it lands and stays 6 seconds, with a red ring of heat around it that fades to dark as it cools. Drive one in within its first 2 seconds and it hits double: 24 damage instead of 12. The Nail: your blade becomes the nail. It stays stuck for 10 seconds and never flies home. Only heavy blows drive it, and each one drives it deeper: 12 damage, then 16, then 24, each with 15 stagger and a notch, and the nail sinks lower each time. The third drive knocks the enemy down for 1.5 seconds (an elite for half that, a god not at all), and a pool of golden blood 1.5 metres around it stays for 4 seconds, dealing 3 damage and a notch once a second. Light blows in between don't count.

**Works with.** Cherry Red: fast weapons, a pack spread out wide, Poured Bronze. The Nail: heavy weapons, Counter Cut, Open Line (a held enemy to hammer).

**Example.** Put The Nail in a brute. Three heavy blows in a row, and it is down on one knee in a pool of gold.

## metal.base.master
**What it does.** A perfect guard drives in every blade stuck in the attacker at once, and Flung Blade is ready again.

**When it happens.** Every perfect guard while Flung Blade is in a spell slot.

**The numbers.** 3 Shards. The wait ends right away. Every blade stuck in the attacker is driven in at once, each for its own drive (12, or 16 with the Empower; a Cherry Red blade still red hits double; the Nail goes one step deeper). An attacker with no blade in it takes nothing extra, but the wait still ends.

**Works with.** Edge Guard and Open Line (the perfect guard), Forged Answer, Sunder.

**Example.** Throw the blade into a duellist and perfect-guard its lunge. The blade drives in, and the next one is ready in your hand.

## metal.lift
**What it does.** Every Crack Rings, Metal's reaction lift. Every reaction Sundered is part of rings out like struck metal. It notches everything around it, and hits harder on notched enemies.

**When it happens.** Whenever a reaction with Sundered in it happens, on any enemy.

**The numbers.** 3 Shards. Any reaction's own blows deal 25% more to an enemy that already has a notch, and nothing extra to one that doesn't. Every reaction with Sundered in it puts one notch on everything it hit: the enemy where it happened and every enemy its blows reached.

**Works with.** Every element pair with Sundered in it (Live Iron with Static, Rust with Soaked, and the rest), Poured Bronze (the notches it spreads), and To the Blade and The Guard Holds, which come after it on the chart.

**Example.** Static meets the notches in one soldier. Live Iron goes off, and the two soldiers next to it come away notched, ready for the pour.

## metal.toblade
**What it does.** To the Blade, the movement spell of the Metal tree. You fly along a line of sparks to one of your blades, untouchable on the way, cutting whatever is in between, and arrive driving that blade in. If you have no blade out, you throw one ahead into the ground and go to it.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 8 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 7 seconds and the reach 14 metres.) It flies to the nearest of your blades within 12 metres, picking one in front of you first, and stops just short of the enemy the blade is stuck in. You fly at 24 metres a second, and nothing can hit you on the way. Each enemy on the line, within 1 metre on each side, is hit once for 10 damage, 15 stagger and a notch. When you arrive, the blade is driven in, just like a weapon blow would drive it. With no blade within reach, you rush 6 metres straight ahead.

**Works with.** Flung Blade and Grave of Swords (blades to fly to), Every Crack Rings, which it comes after on the chart, Sunder, Poured Bronze. When it lands, it charges your blade with Metal.

**Not built yet.** Flying to a blade from Grave of Swords: those blades don't stay standing in their ring yet.

**Example.** Throw Flung Blade into the archer across the room, then fly to it through the two soldiers in between.

## metal.toblade.shape
**What it does.** Unlocks two shapes for To the Blade. You pick one at any rest fire. **Chain**: a bronze chain stays stretched along the line you flew. An enemy that crosses it is notched and tripped. Your path becomes a tripwire. **There and Back**: within two seconds of arriving, press again to fly back along the same line, cutting it a second time. In and out.

**When it happens.** Whenever you cast To the Blade, using the shape you picked. If you have not bought this, you cast the plain flight.

**The numbers.** 3 Shards. Chain: a line of five patches, 6 metres long from where you started, each 0.6 metres across, lasting 3 seconds. Twice a second, it deals 10 stagger and a notch to what stands on it, and each enemy that crosses it is tripped and knocked flat for 0.6 seconds (each one at most once every 1.5 seconds). There and Back: press the slot again within 2 seconds to fly straight back to where you cast it, cutting everything on the line for 10 damage, 15 stagger and a notch.

**Works with.** Chain: a doorway, Poured Bronze on enemies that crossed it, Circle of Steel. There and Back: Sunder, Poured Bronze (two notches from one flight).

**Example.** Put Chain across the entrance to a hallway. The soldiers that follow you through it arrive notched and off balance.

## metal.toblade.master
**What it does.** Cast it right as a blow would land on you, and it counts as a perfect dodge, you are untouchable from the moment you press, and To the Blade is ready again.

**When it happens.** Each time you cast To the Blade right as a blow would land on you. To the Blade must be in a spell slot.

**The numbers.** 3 Shards. It only counts if you start it in the last 0.3 seconds before an enemy's blow (one within 6 metres) lands on you. Then it is a perfect dodge: you get 12 mana, the game slows for a moment, the blow misses, and every mark that answers a perfect dodge answers this one. You are untouchable for 0.6 seconds from the press, and the spell's wait ends.

**Works with.** The perfect dodge, Flung Blade and Grave of Swords (a blade to fly to), Counter Cut on the enemy you arrive at.

**Example.** Cast it just as a brute's swing comes down. The swing misses, you fly to the blade in the archer behind it, and the spell is ready again for the next one.

## metal.devotion
**What it does.** The Guard Holds, Metal's Devotion, for going all in on Metal only. It is Metal's basic bonus, defence, at full strength. Your guard never breaks. A blow your stamina can't cover is still blocked, and whatever your stamina can't cover comes through as grey health. Every blow you block notches the attacker and throws sparks into it. You take less damage from everything. Red attacks still need a dodge or a perfect guard.

**When it happens.** Only while you carry Metal twice (all in), once you buy it. It is always on then.

**The numbers.** 6 Shards. You take 15% less damage from everything. A blow your stamina can't cover is still blocked: all the stamina you have is used up, and the part of the blow it couldn't cover comes through in full as grey health, on top of the usual chip damage. So with no stamina at all, the whole blow is grey. Each blow you block hits the attacker for 4 damage and a notch, at most five times a second. Buying this also gives you the deep strike, Red Iron. Use it with L2 and R2 while you carry Metal alone or twice.

**Works with.** Every Crack Rings, which it comes after on the chart, Sunder, Forged Answer, Hanging Swords (every blocked blow is a perfect guard), Temper.

**Example.** Keep your guard up in the middle of a pack. Every blow that lands on it notches its attacker, and the pour waits for all of them.

## metal.hangingswords
**What it does.** While Temper lasts, every blow you block counts as a perfect guard, and a sword is made over the enemy that hit you, up to eight. Each sword hangs over its enemy and follows it. When Temper ends, they fall.

**When it happens.** Light Temper with the surge button, then guard. The swords fall when Temper ends.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. The perfect timing window lasts all of Temper, as long as you raise your guard fresh. Each guard gives 5 more mana, at most about three times a second. Up to eight swords. They fall 0.3 seconds after Temper ends, 0.11 seconds apart, each hitting 1.5 metres around for 16 damage, 20 stagger and a notch of Sundered.

**Works with.** Temper (30% less damage taken, and one more notch with every blow), Sunder, Forged Answer, Open Line, Edge Guard, The Guard Holds, and every Master mark that works on a perfect guard.

**Example.** Light Temper and stand in the middle of a pack with your guard up. Every blow is blocked, and when Temper ends, eight swords come down on them.

## metal.allbladeshome
**What it does.** All Blades Home, a keystone. The blades you throw never fade. They stay stuck in enemies, or in the ground once their enemy falls, until you drive them in. Lighting Temper calls every one of them back to you in a straight line, cutting and notching every enemy in the way, and each blade that comes home makes Temper last a little longer. Temper costs more.

**When it happens.** When you light Temper, the surge. Always on once you learn it, while you carry Metal.

**The numbers.** 6 Shards. Up to sixteen blades can stand at once; a seventeenth sends the oldest home. When you light Temper, each blade flies straight back to you and deals 12 damage, 8 stagger and one notch of Sundered to every enemy on its line, within 0.7 metres on each side. Each blade that comes home adds 0.4 seconds to Temper. Temper costs 37.5% more mana (55 instead of 40) and lasts 20% longer. Blades called home are gone.

**Works with.** Grave of Swords, which it comes after on the chart (from its Master mark), To the Blade, Forged Answer, Every Crack Rings, Hanging Swords (Temper's god's gift).

**Not built yet.** The blades of Grave of Swords should be blades of yours too (they don't stand in their ring yet). The blades fly home at once; the short pause with their points turned toward you, and the wheel of blades folding into your weapon, aren't shown yet.

**Example.** Throw Flung Blade into three enemies over a fight and let the blades stand. Then light Temper. All three fly home through the crowd, and every enemy they cross is notched.
