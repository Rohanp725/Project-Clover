# Metal: detailed explanations

The long explanation the chart shows for each high-level mark of the Metal tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Metal): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "A notch" is one stack of Sundered; "your blades" means
the steel you throw and plant: Flung Blade, the blades of Grave of Swords.

## metal.sunder
**What it does.** Heavy blows and perfect guards notch an enemy's armour, and a finisher notches it three deep at once. Each notch is a stack of Sundered, and each lets your weapon bite deeper.

**When it happens.** Always on once learned: every heavy blow that lands, every perfect guard (on the one who struck), every finisher. Light blows notch nothing. Like every root, it lays with the weapon only while Metal is your first element (or your only one); the perfect guard's notch comes either way, and carried second, Metal's notches come from your spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. One notch a heavy blow or perfect guard, three for a finisher, three at most. Sundered lasts 8 seconds; each notch adds 8% to your weapon's damage on that enemy, 24% at three. Spells are not made stronger by it.

**Works with.** Counter Cut and critical blows (they multiply each other), Poured Bronze (it spends the notches for bronze), Every Crack Rings, Temper (one more notch every blow), Flung Blade, and Live Iron when Static meets the notches.

**Example.** Two heavy blows on a shield-bearer and its armour is two deep: every cut after lands 16% harder for the next 8 seconds.

## metal.countercut
**What it does.** A blow that lands while an enemy is showing its own attack, winding up or swinging, is critical.

**When it happens.** Always on once learned: any weapon blow that lands on an enemy in the middle of its attack, or holding you.

**The numbers.** 3 Shards. A critical blow deals one and a half times its damage. The design's extra stagger is not in it yet.

**Works with.** Sunder (a critical blow and Sundered multiply each other), Poured Bronze, Grave of Swords and Master's Answer, which grow from it, Halo of Blades on the Halo branch (each critical blow forges a blade).

**Example.** A brute raises its hammer: step in and land a heavy blow before it falls, for one and a half times the damage.

## metal.bronze
**What it does.** Poured Bronze, the detonator of the tree. You raise a hand, and over every notched enemy near you a lip of molten bronze tips in the air and pours into its cracks. The more notches it carried, the harder it is struck. The notches are spent, and the bronze sets: the enemy is held under a crust that cools from cherry red to black.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Only enemies carrying Sundered are poured into.

**The numbers.** 4 Shards, no mana: it waits 12 seconds after a cast, less with weapon play (its Empower: 11, and 15 a notch). Every enemy within 8 metres is reached 0.4 seconds after the cast. On each notched one: 10 damage, and 12 damage and 15 stagger for each notch it carried (46 damage and 45 stagger at three). Its notches are spent, and it is held 0.8 seconds (a god or an elite 0.4 of that).

**Works with.** Sunder, Open Line and Forged Answer (they notch on the perfect guard), Every Crack Rings, the charged blade (its charged blows notch), Grave of Swords and Returning Edge, which notch a crowd for it to pour into.

**Not built yet.** The hold set by the notches, 0.4 seconds a notch (a quarter of that on a god or an elite): for now 0.8 seconds whatever the depth. The lip of bronze tipping over each enemy and the crust cooling on it. The Empower's 15 a notch: for now it only shortens the wait.

**Example.** Notch three soldiers to three with heavy blows and Grave of Swords, then pour: each takes 46 and stands held while you choose which to finish.

## metal.bronze.shape
**What it does.** Opens two shapes of Poured Bronze, chosen at any rest fire. **Set**: the crust stays, and a heavy blow on a crusted enemy shatters it, throwing bronze splinters out behind it that notch what they hit. Pour, then break. **Red-Hot**, Talos heating himself: the bronze stays molten in the cracks. There is no hold, but every weapon blow on that enemy spits sparks and deals more for each notch that was spent. Pour, then press.

**When it happens.** Whenever you cast Poured Bronze, as the shape it wears. Before this is bought, the plain pour is cast.

**The numbers.** 3 Shards. Set: the crust stays 4 seconds; the next heavy blow on it shatters it, 20 damage and 15 stagger within 1 metre, and a lane of splinters 3 metres on behind it, away from you, 8 damage, 8 stagger and a notch to each it reaches. Red-Hot: for 4 seconds every weapon blow on it spits sparks for 12 more damage (two notches' worth).

**Works with.** Set: heavy weapons, Counter Cut, a pack standing in a line behind the one poured into. Red-Hot: quick weapons, the charged blade, Open Line (a held enemy to press).

**Not built yet.** Both shapes take every enemy the pour reached, not only the ones poured into: the shape answers after the pour has spent the notches it would test. Red-Hot's 6 more for each notch spent (for now 12 whatever the depth), and its having no hold (for now the hold still comes).

**Example.** Set on a notched brute with two soldiers at its back: one heavy blow shatters the crust, and the splinters notch the two behind for the next pour.

## metal.bronze.master
**What it does.** Cast it within a second after a perfect guard, and the attacker's pour is doubled and Poured Bronze is ready again.

**When it happens.** For now, a cast of Poured Bronze within 1 second after a perfect guard, while it is ready, starts no wait. Poured Bronze must be in a slot.

**The numbers.** 3 Shards. The cast in that second starts no wait, so you may pour again at once.

**Works with.** Sunder, Open Line and Forged Answer (the same perfect guard notches, holds and forges), Edge Guard (the longer perfect moment), Master's Answer.

**Not built yet.** The attacker's pour doubled: for now only the wait is saved.

**Example.** Perfect-guard a duellist's thrust and pour at once: it is held under the bronze, and the pour is ready again for the next one.

## metal.graveofswords
**What it does.** Grave of Swords, Metal's larger spell. Twelve blades fall in a ring round you and stand there. Each light blow you land calls the nearest one through that enemy, and a heavy blow calls all that remain. As built the blades hang behind your shoulders and fly by themselves, one after another, at the nearest enemy, and a heavy blow that lands sends all that remain at the one it struck.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). It is Metal's larger spell, paid in mana.

**The numbers.** 4 Shards, 50 mana (its Empower: 45 mana, and sixteen blades). Twelve blades for 12 seconds. As built the first flies 0.6 seconds after the cast, then one every 0.55 seconds at the nearest enemy within 6 metres. Each strikes for 9 damage, 6 stagger and one notch of Sundered. Those not flown when the time is up go out.

**Works with.** Sunder, Forged Answer on the heavy blow that sends the rest, Poured Bronze (a crowd notched for it), To the Blade (its blades are blades to fly to), Every Crack Rings.

**Not built yet.** The twelve falling in a ring 4.5 metres round you, 8 damage under each, and standing there; a light blow calling the nearest one through its enemy, and a notch only every three blades (for now every blade notches).

**Example.** Cast it in a pack and fight on: the blades keep three notches on whoever is nearest, and a heavy blow on the brute sends the last five into it.

## metal.graveofswords.shape
**What it does.** Opens two shapes of Grave of Swords, chosen at any rest fire. **Clashing Bronze**, the Kouretes clashing bronze round the infant Zeus: the blades do not fly, they ring. Each light blow you land makes the nearest standing blade ring, a sharp ripple on the floor that notches all it reaches; a heavy blow rings them all at once, and they fall. Every blow rings. **The Tripods**, the smith's automata: three bronze tripods roll out, four blades on each, and go to notched enemies first, cutting; a heavy blow you land calls each home, throwing its blades through whatever it passes.

**When it happens.** Whenever you cast Grave of Swords, as the shape it wears. Before this is bought, the plain blades are cast.

**The numbers.** 3 Shards. Clashing Bronze: for 12 seconds, every weapon blow on an enemy that was within 8 metres when it was cast rings 3 metres round it, 8 damage, 15 stagger and a notch, at most five times a second; a heavy blow on one rings 4.5 metres round you as well, 16 damage, 30 stagger and a notch, once a second. The Tripods: three helpers for 6 seconds, each cutting the nearest enemy every half second for 9 damage, 6 stagger and a notch.

**Works with.** Clashing Bronze: quick weapons, Counter Cut, Poured Bronze on everything it rang. The Tripods: Poured Bronze, Open Line (the held enemy is cut for you).

**Not built yet.** Clashing Bronze's blades standing in their ring and ringing in place of flying (for now they still fly, and the ringing answers your blows on the enemies near when it was cast). The Tripods going to notched enemies first, a notch only every third cut (for now every cut), and the heavy blow calling each home through what it passes.

**Example.** Clashing Bronze in a crowd: every light blow throws a ripple through the three round the one you struck.

## metal.graveofswords.master
**What it does.** A finisher looses every blade left, and 20 mana comes back to you.

**When it happens.** For now, the first heavy blow (a finisher counts) that lands on an enemy that was within 8 metres when Grave of Swords was cast, within its 12 seconds; once a cast. Grave of Swords must be in a slot.

**The numbers.** 3 Shards. 20 mana back. The heavy blow already sends every blade that remains.

**Works with.** Sunder (the finisher notches three deep), Counter Cut, Forged Answer, Temper (the mana for it).

**Not built yet.** The finisher alone as the moment: for now any heavy blow on an enemy that was near when it was cast.

**Example.** Cast it, stagger the brute and finish it: the last blades go in, and 20 mana comes back.

## metal.mastersanswer
**What it does.** For two seconds after a perfect guard your weapon's skill costs no mana and comes at once, with no time to charge. At any other time it costs half again as much.

**When it happens.** Always on once learned. A perfect guard opens the two seconds; press the skill then.

**The numbers.** 6 Shards. Two seconds of a free skill that needs no charging; outside them the skill costs 1.5 times its mana.

**Works with.** Counter Cut, which it grows from, Open Line (the attacker is still held when the skill lands), Edge Guard, Forged Answer, Poured Bronze's Master, and on the Katana the Iaido Slash and Answering Draw.

**Example.** With the Katana, perfect-guard a thrust and the Iaido Slash leaves the sheath at once, free.

## metal.openline
**What it does.** A perfect guard throws the attacker's weapon wide. It is held still for a moment, and for a breath after the guard your weapon strikes harder.

**When it happens.** Every perfect guard: press the guard button just before a blow lands.

**The numbers.** 3 Shards. The attacker is held for 1.2 seconds (elites for less; what cannot be frozen, not at all). Your weapon deals 15% more damage for 1.5 seconds after the guard, on any enemy. As built the hold is the whole of it: it can still raise its guard, and blows on the held enemy do not count as from behind.

**Works with.** Sunder (a notch with the same guard), Forged Answer, Counter Cut, Master's Answer (the free skill comes in the same breath), Returning Edge and Circle of Steel, which grow from it.

**Example.** Perfect-guard a brute's swing and it stands frozen with its arm thrown wide: a heavy blow lands before it can move.

## metal.returningedge
**What it does.** Returning Edge, a breaker. A turning blade a metre and a half across flies out along your facing and back to your hand, drawing its lane on the ground, and cuts every enemy it passes each way.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 8, and 16 damage and 14 stagger). It runs 8 metres at 16 metres a second, 0.75 metres to each side of its line. Going: 12 damage, 10 stagger and a notch to each enemy it passes; a second later, coming home: 12 damage and 10 stagger again to each it struck going.

**Works with.** Open Line, which it grows from, Sunder, Poured Bronze on the line it notched, the charged blade (it charges your weapon with Metal).

**Not built yet.** The turning blade itself, seen flying out and back to your hand: for now the lane breaks open along the ground as it runs, and what it struck is cut again a second later where it stands. The Empower's numbers on the way home (for now the return keeps 12 and 10).

**Example.** Four soldiers come down a corridor: the blade runs through them and back, and all four are notched for the pour.

## metal.returningedge.shape
**What it does.** Opens two shapes of Returning Edge, chosen at any rest fire. **Bat It Home**: guard inside the perfect window as the blade arrives, and it goes out again, faster and harder, three times at most. **Through the Line**: it does not come back by itself. It stops out at the end of its run and spins in place, and your next dodge calls it home along the line, cutting everything between. Put them between you and the blade.

**When it happens.** Whenever you cast Returning Edge, as the shape it wears. Before this is bought, the plain blade is cast.

**The numbers.** 3 Shards. Bat It Home: for now, a perfect guard throws a blade out and back at 22 metres a second, 18 damage and 15 stagger to each enemy it passes each way, at most once in 2 seconds. Through the Line: the blade spins 8 metres out for 3 seconds, 0.9 metres round it, biting every 0.3 seconds for 8 damage; for now, when the 3 seconds are up, each enemy it struck going is cut again for 20 damage, 30 stagger and a notch.

**Works with.** Bat It Home: Edge Guard and Open Line (the perfect guard), Forged Answer. Through the Line: a dodge-heavy fight, Poured Bronze on the line, Counter Cut.

**Not built yet.** The blade's arrival as the moment the guard meets, and three times at most (for now any perfect guard throws one, once in 2 seconds). Through the Line's blade called home by your dodge through everything between, and its not coming back by itself (for now the plain return still comes, and the home cut lands on the enemies it struck going, when its time is up).

**Example.** Through the Line down a corridor: let the soldiers walk into the spinning blade, then the line comes home through them.

## metal.returningedge.master
**What it does.** Catch it on a perfect guard as it comes home, and Returning Edge is ready again at once.

**When it happens.** For now, any perfect guard while its wait is running, once in 9 seconds. Returning Edge must be in a slot.

**The numbers.** 3 Shards. The wait ends at once.

**Works with.** Edge Guard and Open Line, Bat It Home, Forged Answer (the same guard forges a blade).

**Not built yet.** The catch itself as the moment: for now any perfect guard, once in 9 seconds.

**Example.** Throw it, perfect-guard the brute's swing, and throw it again at once.

## metal.circle
**What it does.** Circle of Steel, the counter. You raise the blade for a breath, unable to move. The first blow that reaches you, from any side, red attack or not, is turned for nothing and answered with a cut all round you that notches everything near.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4), then any blow in the next 1.2 seconds.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 9, two blows turned, and 16 damage). The answer strikes everything within 3 metres, all the way round, for 14 damage, 20 stagger and one notch, a heavy blow. As built the caught blow is not a perfect guard for your marks: it gives no mana and sets off nothing that waits on one.

**Works with.** Open Line, which it grows from, Edge Guard, which grows from it, Sunder, Poured Bronze on the ring it notched, the charged blade (the answer is a spell, so it charges your weapon with Metal).

**Not built yet.** The shorter wait, 5 seconds, when nothing struck; the Empower's second blow turned.

**Example.** Ringed by three soldiers, cast it as the first lunges: the cut turns its blow and notches all three.

## metal.circle.shape
**What it does.** Opens two shapes of Circle of Steel, chosen at any rest fire. **The Golden Chair**, the smith's chair that bound the queen of the gods: a blow turned closes bands of gold round the attacker, a faint shimmer that turns bright as they lock. It is held, notched twice, and your blows on it pass any guard. **Ringing Plate**: you stand as a struck plate. Every blow turned rings out in a sharp ripple on the floor, each ring wider than the last. The more of them strike you, the better.

**When it happens.** Whenever you cast Circle of Steel, as the shape it wears. Before this is bought, the plain circle is cast.

**The numbers.** 3 Shards. The Golden Chair: the answer is a cut 3 metres and 90 degrees at the one who struck, 14 damage, 20 stagger and a notch; what it strikes is held 2 seconds (a god or an elite 0.4 of that), notched once more, and cannot raise its guard for 2 seconds. Ringing Plate: ready for 1.5 seconds; the blow turned rings 4 metres all round, 8 damage, 20 stagger and a notch.

**Works with.** The Golden Chair: Open Line, Poured Bronze (two notches and a held enemy), Counter Cut. Ringing Plate: being surrounded, Every Crack Rings.

**Not built yet.** The Golden Chair's bands on the attacker alone, with the cut all round kept (for now the answer is a cone at it, and all the cone strikes is bound). Ringing Plate's every blow turned in its 1.5 seconds, four at most, each ring wider (for now the first blow turned rings once).

**Example.** The Golden Chair on a shield-bearer's charge: it stands bound in gold with its shield down while you cut.

## metal.circle.master
**What it does.** Turn a red attack, and 15 mana comes back to you and Circle of Steel is ready again at once.

**When it happens.** For now, any blow the circle turns, once in 20 seconds. Circle of Steel must be in a slot.

**The numbers.** 3 Shards. The wait ends at once, and 15 mana.

**Works with.** Edge Guard, Temper (the mana for it), Ringing Plate.

**Not built yet.** A red attack as the blow turned: for now any blow turned, once in 20 seconds.

**Example.** A god's red sweep comes: raise the circle, turn it, and raise it again for the next.

## metal.edgeguard
**What it does.** The perfect guard's moment is half again as long, and every perfect guard cuts the attacker and notches its armour. The price: the dodge and every blow held on a plain guard cost half again as much stamina.

**When it happens.** Always on once learned: the cut comes with every perfect guard, the price with every dodge and every blocked blow.

**The numbers.** 6 Shards. The perfect moment grows by 55%, from 0.18 seconds to about 0.28. Each perfect guard strikes everything within 1.2 metres of the attacker for 12 damage and 10 stagger, with one notch of Sundered. A dodge costs 37.5 stamina where it cost 25; a blocked blow costs 1.35 stamina a point of damage where it cost 0.9. As built a held guard still blocks as usual.

**Works with.** Circle of Steel, which it grows from, Sunder (two notches from one guard), Forged Answer, Open Line, Master's Answer, every Master that answers a perfect guard, Hanging Swords, Bright Steel and Halo of Blades on the Halo branch.

**Not built yet.** Only the perfect guard turning a blow (for now a held guard still blocks), and the guard pressed again after 0.2 seconds.

**Example.** Fight a fast duellist with the guard button alone: each of its blows met at the moment is turned, cut and notched.

## metal.forgedanswer
**What it does.** Each perfect guard forges a blade from the blow you turned, and it hangs at your shoulder. Up to three wait there. Your next heavy blow that lands sends every one of them at the enemy it struck.

**When it happens.** A perfect guard forges one; the next heavy blow that lands looses all that hang. The count shows on the screen as Blades.

**The numbers.** 3 Shards. Three blades at most. Each flies at 16 metres a second, turning after the enemy, and strikes for 10 damage and 12 stagger. As built the blades lay no notch of their own.

**Works with.** Sunder (the heavy blow that sends them notches as well), Edge Guard (easier perfect guards), Open Line, Every Crack Rings, which grows from it, Flung Blade's Master.

**Example.** Perfect-guard a duellist's three-blow flurry, then answer with one heavy blow: three blades follow it in for 30 more damage and 36 stagger.

## metal.base
**What it does.** Flung Blade, Metal's own spell, comes with the element and is never bought. A blade flies into the enemy faced and stands in it. Your next weapon blow on that enemy drives it in with a burst of sparks, and it tears free and flies back to your hand, cutting the lane.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Metal, it is always yours to put in a slot.

**The numbers.** Given with Metal. No mana: it waits 6 seconds after a cast, less with weapon play; its Empower makes that 5. An enemy within 9 metres: 4 damage as it lands, and it stands 6 seconds. Driven by your next weapon blow: 12 damage, 15 stagger, a notch and a burst of sparks 1.5 metres round, and a third as much to what stands in the lane home.

**Works with.** Its Empower, its two shapes and its Master; Sunder, Poured Bronze, To the Blade (it is a blade to fly to), the charged blade, which its landing charges with Metal.

**Not built yet.** The Empower's 16 when driven, and up to three blades standing in one enemy with a heavy blow driving them all (once Buried Blades): the base spell is cast by Metal's kit, which takes only the Empower's shorter wait.

**Example.** Throw the blade into the archer at the back, then close and cut it: the blade bursts out of it and flies home through the soldier between.

## metal.base.shape
**What it does.** Opens two shapes of Flung Blade, chosen at any rest fire. **Cherry Red**: three red-hot blades fly in a fan, each standing in a different enemy. They glow cherry red and cool to black, and one driven while still red strikes double. You throw wide and then hurry. **The Nail**, the bronze nail at Talos's ankle: one long nail driven low into the enemy, and it does not come back. Each heavy blow on that enemy drives it deeper, and at the third it kneels while golden ichor runs out round it. Three heavies on one enemy.

**When it happens.** Whenever you cast Flung Blade, as the shape it wears. Before this is bought, the plain blade is thrown.

**The numbers.** 3 Shards. Cherry Red: for now two more blades fly in a fan beside the kit's, 4 damage and a notch to each enemy they pass; the kit's blade driven within 2 seconds strikes again for 12 damage and 15 stagger. The Nail: for 10 seconds every heavy blow on the nailed enemy drives for 16 damage, 15 stagger and a notch; if its third weapon blow is heavy, it is knocked down for 1.5 seconds (an elite for half that, a god not at all), and a pool of ichor 1.5 metres round it lies 4 seconds, biting once a second for 3 damage and a notch.

**Works with.** Cherry Red: quick weapons, a pack spread wide, Poured Bronze. The Nail: heavy weapons, Counter Cut, Open Line (a held enemy to hammer).

**Not built yet.** Cherry Red's three blades each standing in a different enemy, cooling over 6 seconds (for now the kit's one blade stands, and the fan cuts and flies on). The Nail staying in its enemy and driven deeper, 12, then 16, then 24 (for now the kit's blade still flies home, and each heavy blow drives for 16); the third counted among heavy blows alone (for now the third weapon blow, if it is heavy).

**Example.** The Nail in a brute: three heavy blows in a row, and it is down on one knee in a pool of gold.

## metal.base.master
**What it does.** A perfect guard drives every blade standing in the attacker at once, and Flung Blade is ready again.

**When it happens.** Every perfect guard while Flung Blade is in a slot.

**The numbers.** 3 Shards. The wait ends at once. On a notched attacker, for now, one blade's drive: 12 damage, 15 stagger and a notch within 0.8 metres of it.

**Works with.** Edge Guard and Open Line (the perfect guard), Forged Answer, Sunder.

**Not built yet.** Only the blades standing in the attacker driven: for now a notched attacker takes one blade's worth, whatever stands in it.

**Example.** Throw the blade into a duellist and perfect-guard its lunge: the blade drives in, and the next is ready in your hand.

## metal.lift
**What it does.** Every Crack Rings, Metal's reaction lift. Every reaction Sundered takes part in rings out like struck plate: it notches everything round it, and strikes harder on the notched.

**When it happens.** Whenever a reaction with Sundered in it happens, on any enemy.

**The numbers.** 3 Shards. One notch on every enemy within 3 metres of where it happened.

**Works with.** Every pair with Sundered in it (Live Iron with Static, Rust with Soaked and the rest), Poured Bronze (the notches it spreads), To the Blade and The Guard Holds, which grow from it.

**Not built yet.** Every reaction with Sundered in it striking 25% harder on the notched; the notch laid on what the reaction reached, not on everything 3 metres round it.

**Example.** Static meets the notches in one soldier: Live Iron fires, and the two beside it come away notched for the pour.

## metal.toblade
**What it does.** To the Blade, the movement spell of the tree. You fly along a line of sparks to a blade of yours, untouchable for the flight, cutting what lies between, and arrive driving that blade. With no blade out, you throw one ahead into the ground and go to it.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 8 seconds after a cast, less with weapon play (its Empower: 7, and 14 metres). As built it is the throw with no blade out: you drive 6 metres ahead at 24 metres a second, a quarter of a second, and each enemy on the line, 1 metre to each side, is struck once for 10 damage, 15 stagger and a notch.

**Works with.** Flung Blade and Grave of Swords (the blades to fly to), Every Crack Rings, which it grows from, Sunder, Poured Bronze. Landing it charges the blade with Metal.

**Not built yet.** Flying to a blade of yours within 12 metres (a Flung Blade standing in an enemy or the ground, a blade of Grave of Swords), untouchable for the flight, and driving that blade as you arrive; the Empower's 14 metres. For now it is always the throw ahead.

**Example.** Throw Flung Blade into the archer across the room, then fly to it through the two soldiers between.

## metal.toblade.shape
**What it does.** Opens two shapes of To the Blade, chosen at any rest fire. **Chain**: a bronze chain stays stretched along the line you flew, and an enemy that crosses it is notched and tripped. Your path becomes a tripwire. **There and Back**: within two seconds of arriving, press again to fly back along the same line, cutting it a second time. In and out.

**When it happens.** Whenever you cast To the Blade, as the shape it wears. Before this is bought, the plain flight is cast.

**The numbers.** 3 Shards. Chain: a line of five patches 6 metres long from where you set off, 0.6 metres round each, for 3 seconds, biting once a second for 20 stagger and a notch. There and Back: for now each enemy the flight cut is cut again a second later, 10 damage, 15 stagger and a notch.

**Works with.** Chain: a doorway, Poured Bronze on what crossed it, Circle of Steel. There and Back: Sunder, Poured Bronze (two notches from one flight).

**Not built yet.** Chain's trip as a fall (for now 20 stagger a bite). There and Back's flight back as a second press within 2 seconds (for now the second cut comes by itself where they stand).

**Example.** Chain across a corridor mouth: the soldiers that follow you through it arrive notched and off balance.

## metal.toblade.master
**What it does.** Cast it in the perfect dodge's moment, and you are untouchable from the press and To the Blade is ready again.

**When it happens.** For now, a cast of To the Blade within 1 second after a perfect dodge, while it is ready, starts no wait. To the Blade must be in a slot.

**The numbers.** 3 Shards. The cast in that second starts no wait.

**Works with.** The perfect dodge, Flung Blade and Grave of Swords (a blade to fly to), Counter Cut on the enemy you arrive at.

**Not built yet.** The shared movement rule's timing: cast in the instant a blow would land, it counts as a perfect dodge and you are untouchable from the press. For now the cast after a perfect dodge saves its wait.

**Example.** Perfect-dodge a brute's swing and fly at once to the blade in the archer behind it, and again to the next.

## metal.devotion
**What it does.** The Guard Holds, Metal's Devotion (GAME.md's base buff for Metal, defence, at full strength): all in on Metal only. Your guard never breaks: a blow your stamina cannot pay for is held anyway, and what it cannot pay comes through as grey health. Every blow held on your guard notches the attacker and throws sparks into it. You take less from everything. Red attacks still need a dodge or a perfect guard.

**When it happens.** Only while Metal is carried twice (all in). Always on then.

**The numbers.** 6 Shards. You take 15% less from everything. Each blow held on your guard strikes the attacker for 4 damage and a notch, at most five times a second. As built a blow held on the guard costs half the stamina it did. All in, L2 with R2 is the deep strike, Red Iron.

**Works with.** Every Crack Rings, which it grows from, Sunder, Forged Answer, Hanging Swords (every held blow a perfect guard), Temper.

**Not built yet.** The guard that never breaks, what the stamina cannot pay coming through as grey health: for now the guard costs half as much stamina, so it breaks far later.

**Example.** Hold the guard up in the middle of a pack: every blow that lands on it notches its attacker, and the pour waits for all of them.

## metal.hangingswords
**What it does.** While Temper lasts, every blow you hold on your guard counts as a perfect guard, and a sword is forged over the one who struck, up to eight. Each sword hangs over its enemy and follows it. When Temper ends, they fall.

**When it happens.** Light Temper with the surge button, then guard. The swords fall when Temper ends.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. The perfect moment lasts the whole of Temper, as long as the guard is raised fresh. Each guard gives 5 mana more, at most about three times a second. Eight swords at most; they fall 0.3 seconds after Temper ends, 0.11 seconds apart, each striking 1.5 metres round for 16 damage, 20 stagger and a notch of Sundered.

**Works with.** Temper (30% less damage taken, a notch more every blow), Sunder, Forged Answer, Open Line, Edge Guard, The Guard Holds, every Master that answers a perfect guard.

**Example.** Light Temper and stand in the middle of a pack with your guard up: every blow turns, and when Temper ends eight swords come down on them.
