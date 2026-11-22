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

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 15 damage for each notch.) It reaches every enemy within 8 metres, 0.4 seconds after you cast it. Each notched enemy takes 10 damage, plus 12 damage and 15 stagger for each notch it had (46 damage and 45 stagger at three). Its notches are used up, and it is held for 0.4 seconds for each notch it had (1.2 seconds at three; a god or an elite for 0.4 of that).

**Works with.** Sunder, Open Line and Forged Answer (they notch on a perfect guard), Every Crack Rings, the charged blade (its charged blows notch), and Grave of Swords and Returning Edge, which notch a crowd for it to pour into.

**Not built yet.** A god or an elite should be held for a quarter of the time. For now, it is held for 0.4 of it. The pot of bronze tipping over each enemy and the crust cooling on it aren't shown yet.

**Example.** Give three soldiers three notches each with heavy blows and Grave of Swords, then pour. Each takes 46 damage and is held while you choose which to finish.

## metal.bronze.shape
**What it does.** Unlocks two shapes for Poured Bronze. You pick one at any rest fire. **Set**: the crust stays. A heavy blow on a crusted enemy shatters it, throwing bronze splinters out behind it that notch whatever they hit. Pour, then break. **Red-Hot** (Talos, the bronze giant, heating himself up): the bronze stays melted in the cracks. There is no hold, but every weapon blow on that enemy spits sparks and deals more damage for each notch that was used up. Pour, then keep attacking.

**When it happens.** Whenever you cast Poured Bronze, using the shape you picked. If you have not bought this, you cast the plain pour.

**The numbers.** 3 Shards. Set: the crust stays for 4 seconds. The next heavy blow on it shatters it for 20 damage and 15 stagger within 1 metre, and sends a path of splinters 3 metres out behind it, away from you. Each enemy the splinters reach takes 8 damage, 8 stagger and a notch. Red-Hot: for 4 seconds, every weapon blow on the enemy spits sparks for 12 more damage (worth two notches).

**Works with.** Set: heavy weapons, Counter Cut, a pack standing in a line behind the enemy you poured into. Red-Hot: fast weapons, the charged blade, Open Line (a held enemy to keep attacking).

**Not built yet.** Both shapes work on every enemy the pour reached, not only the ones it poured into, because the shape checks after the pour has already used up the notches. Red-Hot should do 6 more for each notch used up (for now, it is 12 no matter how many), and it shouldn't hold the enemy (for now, the hold still happens).

**Example.** Use Set on a notched brute with two soldiers behind it. One heavy blow shatters the crust, and the splinters notch the two behind it, ready for the next pour.

## metal.bronze.master
**What it does.** Cast it within a second after a perfect guard, and the pour on the attacker is doubled, and Poured Bronze is ready again.

**When it happens.** For now, if you cast Poured Bronze within 1 second after a perfect guard, while it is ready, it doesn't start its wait. Poured Bronze must be in a spell slot.

**The numbers.** 3 Shards. A cast in that second doesn't start a wait, so you can pour again right away.

**Works with.** Sunder, Open Line and Forged Answer (the same perfect guard notches, holds and makes a blade), Edge Guard (the longer perfect timing window), Master's Answer.

**Not built yet.** Doubling the pour on the attacker. For now, it only skips the wait.

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

**When it happens.** Your first finisher that lands on an enemy that was within 8 metres when you cast Grave of Swords, during its 12 seconds; once per cast. Other heavy blows don't count. Grave of Swords must be in a spell slot.

**The numbers.** 3 Shards. 20 mana back. The heavy blow already sends every blade that's left.

**Works with.** Sunder (the finisher adds three notches), Counter Cut, Forged Answer, Temper (the mana for it).

**Not built yet.** It should work only while the blades still stand. For now, it works for the spell's full 12 seconds.

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
**What it does.** Returning Edge, a breaker. A spinning blade a metre and a half across flies out in the direction you face and back to your hand, marking its path on the ground, and cuts every enemy it passes, both ways.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 16 damage and 14 stagger, going out and coming back.) It travels 8 metres at 16 metres a second, reaching 0.75 metres to each side of its line. Going out, it deals 12 damage, 10 stagger and a notch to each enemy it passes. A second later, coming back, it deals 12 damage and 10 stagger again to each one it hit going out.

**Works with.** Open Line, which it comes after on the chart, Sunder, Poured Bronze on the line it notched, and the charged blade (it charges your weapon with Metal).

**Not built yet.** The spinning blade itself, flying out and back to your hand. For now, the path breaks open along the ground as it goes, and whatever it hit is cut again a second later where it stands.

**Example.** Four soldiers come down a hallway. The blade runs through them and back, and all four are notched, ready for the pour.

## metal.returningedge.shape
**What it does.** Unlocks two shapes for Returning Edge. You pick one at any rest fire. **Bat It Home**: guard during the perfect timing window as the blade comes back, and it goes out again, faster and harder, up to three times. **Through the Line**: it doesn't come back on its own. It stops at the end of its run and spins in place, and your next dodge calls it home along the line, cutting everything in between. Get the enemies between you and the blade.

**When it happens.** Whenever you cast Returning Edge, using the shape you picked. If you have not bought this, you cast the plain blade.

**The numbers.** 3 Shards. Bat It Home: for now, a perfect guard throws a blade out and back at 22 metres a second, dealing 18 damage and 15 stagger to each enemy it passes, both ways, at most once every 2 seconds. Through the Line: the blade spins 8 metres out for 3 seconds, reaching 0.9 metres around it, and deals 8 damage every 0.3 seconds. For now, when the 3 seconds are up, each enemy it hit going out is cut again for 20 damage, 30 stagger and a notch.

**Works with.** Bat It Home: Edge Guard and Open Line (the perfect guard), Forged Answer. Through the Line: a fight with lots of dodging, Poured Bronze on the line, Counter Cut.

**Not built yet.** Bat It Home should work when your guard meets the returning blade, up to three times (for now, any perfect guard throws one, once every 2 seconds). Through the Line's blade should be called home by your dodge through everything in between, and shouldn't come back on its own (for now, the normal return still happens, and the final cut lands on the enemies it hit going out when its time is up).

**Example.** Use Through the Line down a hallway. Let the soldiers walk into the spinning blade, then the blade comes home through them.

## metal.returningedge.master
**What it does.** Catch it with a perfect guard as it comes home, and Returning Edge is ready again right away.

**When it happens.** For now, any perfect guard while its wait is counting down, once every 9 seconds. Returning Edge must be in a spell slot.

**The numbers.** 3 Shards. The wait ends right away.

**Works with.** Edge Guard and Open Line, Bat It Home, Forged Answer (the same guard makes a blade).

**Not built yet.** It should need you to actually catch the blade. For now, any perfect guard works, once every 9 seconds.

**Example.** Throw it, perfect-guard the brute's swing, and throw it again right away.

## metal.circle
**What it does.** Circle of Steel, the counter. You raise your blade for a moment and can't move. The first blow that reaches you, from any side, red attack or not, does nothing to you, and you answer with a cut all around you that notches everything nearby.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then it works on any blow in the next 1.2 seconds.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower blocks two blows and makes it 16 damage.) The answer hits everything within 3 metres, all the way around, for 14 damage, 20 stagger and one notch, as a heavy blow. For now, the blocked blow doesn't count as a perfect guard for your marks: it gives no mana and doesn't set off anything that needs one. If nothing hits you, the wait is half as long: 5 seconds.

**Works with.** Open Line, which it comes after on the chart, Edge Guard, which comes after it, Sunder, Poured Bronze on the ring of enemies it notched, and the charged blade (the answer is a spell, so it charges your weapon with Metal).

**Not built yet.** The Empower's second blocked blow.

**Example.** Three soldiers surround you. Cast it as the first one lunges. The cut blocks its blow and notches all three.

## metal.circle.shape
**What it does.** Unlocks two shapes for Circle of Steel. You pick one at any rest fire. **The Golden Chair** (the smith's chair that trapped the queen of the gods): a blocked blow closes bands of gold around the attacker, a faint shimmer that turns bright as they lock. It is held, gets two notches, and your blows on it go through any guard. **Ringing Plate**: you stand like a struck metal plate. Every blow you block rings out in a sharp ripple on the floor, each ring wider than the last. The more enemies hit you, the better.

**When it happens.** Whenever you cast Circle of Steel, using the shape you picked. If you have not bought this, you cast the plain circle.

**The numbers.** 3 Shards. The Golden Chair: the answer is a cut 3 metres long and 90 degrees wide at the enemy that hit you, for 14 damage, 20 stagger and a notch. Whatever it hits is held for 2 seconds (a god or an elite for 0.4 of that), gets one more notch, and can't raise its guard for 2 seconds. Ringing Plate: ready for 1.5 seconds. The blocked blow rings out 4 metres all around, for 8 damage, 20 stagger and a notch.

**Works with.** The Golden Chair: Open Line, Poured Bronze (two notches and a held enemy), Counter Cut. Ringing Plate: being surrounded, Every Crack Rings.

**Not built yet.** The Golden Chair's bands should go on the attacker alone, while keeping the cut all around (for now, the answer is a cone at the attacker, and everything the cone hits is bound). Ringing Plate should ring for every blow it blocks in its 1.5 seconds, up to four, each ring wider (for now, the first blocked blow rings once).

**Example.** Use The Golden Chair on a shield bearer's charge. It stands trapped in gold with its shield down while you cut.

## metal.circle.master
**What it does.** Block a red attack, and you get 15 mana back and Circle of Steel is ready again right away.

**When it happens.** For now, any blow the circle blocks, once every 20 seconds. Circle of Steel must be in a spell slot.

**The numbers.** 3 Shards. The wait ends right away, and you get 15 mana.

**Works with.** Edge Guard, Temper (the mana for it), Ringing Plate.

**Not built yet.** It should only work on red attacks. For now, any blocked blow works, once every 20 seconds.

**Example.** A god's red sweep comes. Raise the circle, block it, and raise it again for the next one.

## metal.edgeguard
**What it does.** The perfect guard timing window is 50% longer, and every perfect guard cuts the attacker and notches its armour. The cost: dodging and every blow blocked with a normal guard cost 50% more stamina.

**When it happens.** Always on once you learn it. The cut comes with every perfect guard. The cost comes with every dodge and every blocked blow.

**The numbers.** 6 Shards. The perfect timing window grows by 55%, from 0.18 seconds to about 0.28. Each perfect guard hits everything within 1.2 metres of the attacker for 12 damage, 10 stagger and one notch of Sundered. A dodge costs 37.5 stamina instead of 25. A blocked blow costs 1.35 stamina per point of damage instead of 0.9. For now, holding your guard still blocks like normal.

**Works with.** Circle of Steel, which it comes after on the chart, Sunder (two notches from one guard), Forged Answer, Open Line, Master's Answer, every Master mark that works on a perfect guard, Hanging Swords, and Bright Steel and Halo of Blades on the Halo branch.

**Not built yet.** Only the perfect guard should block a blow (for now, holding your guard still blocks), and you should have to press guard again after 0.2 seconds.

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

**The numbers.** Comes with Metal. It costs no mana. After you cast it, you must wait 6 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the drive hit for 16. It targets an enemy within 9 metres. It deals 4 damage when it lands, and stays stuck for 6 seconds. When your next weapon blow drives it in, it deals 12 damage, 15 stagger, a notch, and a burst of sparks 1.5 metres around, plus a third as much to anything on its path home.

**Works with.** Its Empower, its two shapes and its Master mark; Sunder, Poured Bronze, To the Blade (it is a blade to fly to), and the charged blade, which it charges with Metal when it lands.

**Not built yet.** The Empower's 16 damage when driven, and up to three blades stuck in one enemy with a heavy blow driving them all in (it used to be called Buried Blades). The spell doesn't read the Empower's numbers yet, so for now the Empower does nothing.

**Example.** Throw the blade into the archer at the back, then close in and cut it. The blade bursts out of it and flies home through the soldier in between.

## metal.base.shape
**What it does.** Unlocks two shapes for Flung Blade. You pick one at any rest fire. **Cherry Red**: three red-hot blades fly in a fan, each sticking in a different enemy. They glow cherry red and cool to black, and one you drive in while it is still red hits double. You throw wide, then hurry. **The Nail** (the bronze nail in the giant Talos's ankle): one long nail is driven low into the enemy, and it doesn't come back. Each heavy blow on that enemy drives it deeper, and on the third, the enemy drops to one knee while golden blood runs out around it. Three heavy blows on one enemy.

**When it happens.** Whenever you cast Flung Blade, using the shape you picked. If you have not bought this, you throw the plain blade.

**The numbers.** 3 Shards. Cherry Red: for now, two more blades fly in a fan next to the normal one, dealing 4 damage and a notch to each enemy they pass. The normal blade, if driven in within 2 seconds, hits again for 12 damage and 15 stagger. The Nail: for 10 seconds, every heavy blow on the nailed enemy drives it in for 16 damage, 15 stagger and a notch. If its third weapon blow is a heavy blow, it is knocked down for 1.5 seconds (an elite for half that, a god not at all), and a pool of golden blood 1.5 metres around it stays for 4 seconds, dealing 3 damage and a notch once a second.

**Works with.** Cherry Red: fast weapons, a pack spread out wide, Poured Bronze. The Nail: heavy weapons, Counter Cut, Open Line (a held enemy to hammer).

**Not built yet.** Cherry Red's three blades should each stick in a different enemy and cool over 6 seconds (for now, the normal blade sticks, and the fan cuts and keeps flying). The Nail should stay in its enemy and be driven deeper, for 12, then 16, then 24 (for now, the normal blade still flies home, and each heavy blow drives in for 16). The third hit should count only heavy blows (for now, it is the third weapon blow, if that one is heavy).

**Example.** Put The Nail in a brute. Three heavy blows in a row, and it is down on one knee in a pool of gold.

## metal.base.master
**What it does.** A perfect guard drives in every blade stuck in the attacker at once, and Flung Blade is ready again.

**When it happens.** Every perfect guard while Flung Blade is in a spell slot.

**The numbers.** 3 Shards. The wait ends right away. For now, a notched attacker takes one blade's drive: 12 damage, 15 stagger and a notch within 0.8 metres of it.

**Works with.** Edge Guard and Open Line (the perfect guard), Forged Answer, Sunder.

**Not built yet.** Only the blades stuck in the attacker should be driven in. For now, a notched attacker takes one blade's worth, no matter what is stuck in it.

**Example.** Throw the blade into a duellist and perfect-guard its lunge. The blade drives in, and the next one is ready in your hand.

## metal.lift
**What it does.** Every Crack Rings, Metal's reaction lift. Every reaction Sundered is part of rings out like struck metal. It notches everything around it, and hits harder on notched enemies.

**When it happens.** Whenever a reaction with Sundered in it happens, on any enemy.

**The numbers.** 3 Shards. The reaction's own damage is 25% higher. One notch on every enemy within 3 metres of where it happened.

**Works with.** Every element pair with Sundered in it (Live Iron with Static, Rust with Soaked, and the rest), Poured Bronze (the notches it spreads), and To the Blade and The Guard Holds, which come after it on the chart.

**Not built yet.** For now, the 25% counts on every enemy the reaction hits, not only the notched ones. The notch should go on what the reaction reached, not on everything within 3 metres.

**Example.** Static meets the notches in one soldier. Live Iron goes off, and the two soldiers next to it come away notched, ready for the pour.

## metal.toblade
**What it does.** To the Blade, the movement spell of the Metal tree. You fly along a line of sparks to one of your blades, untouchable on the way, cutting whatever is in between, and arrive driving that blade in. If you have no blade out, you throw one ahead into the ground and go to it.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 8 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 7 seconds and the reach 14 metres.) For now, it always works like you have no blade out: you rush 6 metres ahead at 24 metres a second, in a quarter of a second. Each enemy on the line, within 1 metre on each side, is hit once for 10 damage, 15 stagger and a notch.

**Works with.** Flung Blade and Grave of Swords (blades to fly to), Every Crack Rings, which it comes after on the chart, Sunder, Poured Bronze. When it lands, it charges your blade with Metal.

**Not built yet.** Flying to one of your blades within 12 metres (a Flung Blade stuck in an enemy or the ground, or a blade from Grave of Swords), being untouchable on the way, and driving that blade in when you arrive. The Empower's 14 metres. For now, it always throws a blade ahead.

**Example.** Throw Flung Blade into the archer across the room, then fly to it through the two soldiers in between.

## metal.toblade.shape
**What it does.** Unlocks two shapes for To the Blade. You pick one at any rest fire. **Chain**: a bronze chain stays stretched along the line you flew. An enemy that crosses it is notched and tripped. Your path becomes a tripwire. **There and Back**: within two seconds of arriving, press again to fly back along the same line, cutting it a second time. In and out.

**When it happens.** Whenever you cast To the Blade, using the shape you picked. If you have not bought this, you cast the plain flight.

**The numbers.** 3 Shards. Chain: a line of five patches, 6 metres long from where you started, each 0.6 metres across, lasting 3 seconds. Once a second, it deals 20 stagger and a notch. There and Back: for now, each enemy the flight cut is cut again a second later for 10 damage, 15 stagger and a notch.

**Works with.** Chain: a doorway, Poured Bronze on enemies that crossed it, Circle of Steel. There and Back: Sunder, Poured Bronze (two notches from one flight).

**Not built yet.** Chain's trip should knock enemies down (for now, it is 20 stagger each time). There and Back's flight back should come from a second press within 2 seconds (for now, the second cut happens on its own where the enemies stand).

**Example.** Put Chain across the entrance to a hallway. The soldiers that follow you through it arrive notched and off balance.

## metal.toblade.master
**What it does.** Cast it at the moment of a perfect dodge, and you are untouchable from the moment you press, and To the Blade is ready again.

**When it happens.** For now, if you cast To the Blade within 1 second after a perfect dodge, while it is ready, it doesn't start its wait. To the Blade must be in a spell slot.

**The numbers.** 3 Shards. A cast in that second doesn't start a wait.

**Works with.** The perfect dodge, Flung Blade and Grave of Swords (a blade to fly to), Counter Cut on the enemy you arrive at.

**Not built yet.** The timing shared by the movement spells: cast it right as a blow would land, and it counts as a perfect dodge and you are untouchable from the moment you press. For now, casting it after a perfect dodge skips its wait.

**Example.** Perfect-dodge a brute's swing and fly right away to the blade in the archer behind it, then on to the next one.

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
**What it does.** All Blades Home, a keystone. Lighting Temper throws five blades out around you in a fan, and they come back to you, cutting and notching everything they pass, both ways. Temper costs more and lasts a little longer.

**When it happens.** When you light Temper, the surge. Always on once you learn it, while you carry Metal.

**The numbers.** 6 Shards. Five blades fly at 14 metres a second, out and back over 1.2 seconds. Each deals 12 damage, 8 stagger and one notch of Sundered to whatever it passes. Temper costs 37.5% more mana (55 instead of 40) and lasts 20% longer.

**Works with.** Grave of Swords, which it comes after on the chart (from its Master mark), To the Blade, Forged Answer, Every Crack Rings, Hanging Swords (Temper's god's gift).

**Not built yet.** The blades you summon should never fade (up to sixteen), and lighting Temper should call every one of them home in a straight line through whatever stands in between, each adding a moment to Temper. For now, the surge throws its own five blades instead.

**Example.** Cast Grave of Swords, fight among the blades, then light Temper. Five more fly out and back through the crowd, and every enemy they cross is notched.
