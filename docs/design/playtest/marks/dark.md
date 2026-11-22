# Dark: detailed explanations

The long explanation the chart shows for each high-level mark of the Dark tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Dark): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Cursed" is Dark's affliction: a curse that makes the enemy
strike weaker, and whose death feeds you. "Risen" is the state after the Rise: stood up again from a death, your health
draining.

## dark.curse
**What it does.** Every weapon blow that lands puts a Curse on the enemy. A Cursed enemy hits you more weakly, and killing it heals you. Also, one time, a blow that would kill you doesn't. Instead you rise up again, Risen, with half your health, and that health slowly drains away.

**When it happens.** Always on once you learn it: every weapon blow that lands, light or heavy, while Dark is your first element. Weapon blows only add your first element's affliction. If Dark is your second element, you Curse enemies with Grasp, Dark's other spells, the charged blade and its strike.

**The numbers.** 2 Shards. A Curse is only one stack deep and lasts 8 seconds. Each blow restarts its timer. A Cursed enemy deals 15% less damage, and killing it heals you 6.

**Works with.** Grasp and every Dark spell (each one Curses), Dread, Devouring Maw (it uses up the Curse), Collected Twice, Nothing Stays Buried, Never Sated, Hunger (every blow Curses while it is lit), and the charged blade, Shadow Cut Loose.

**The Rise.** One time, while you carry Dark, a blow that would kill you doesn't. You stand up again with half your health, and it drains 4 every second, but it never takes your last point. If you are killed while Risen, you fall. After a fall, you can rise again. Only the god's gift, A Life for a Life, gives you the Rise back sooner: land a finisher on the enemy that killed you. Dead Already turns the Rise off.

**Example.** Hit the brute first. While it is Cursed, each of its swings takes 15% less of your health. When it dies, you get 6 health back.

## dark.dread
**What it does.** When a Cursed enemy dies, the other Cursed enemies near it lose their nerve. They lose their balance and drop their guard.

**When it happens.** Each time a Cursed enemy dies, no matter what killed it. At most once a second.

**The numbers.** 3 Shards. Every enemy within 8 metres of where it fell takes 30 stagger. Every Cursed one among them can't raise its guard for 3 seconds. It deals no damage.

**Works with.** The root mark, Debts Called and Grasp's The Pit (they gather Cursed enemies together), Devouring Maw and Gone to Ash, which come after it on the chart, Collected Twice, Nothing Stays Buried (the dead stand up too).

**Not built yet.** Only the Cursed enemies should lose their balance; for now, every enemy near where it fell takes the stagger. It should also cancel an attack the enemy was starting.

**Example.** Kill a Cursed soldier at the front of a pack. The four Cursed soldiers behind it are now 30 closer to being staggered, and open to your blows.

## dark.maw
**What it does.** Devouring Maw, the detonator of the Dark tree. A huge jaw, twice your height, rises out of your shadow and lunges down a path in front of you. On a Cursed enemy, it uses up the Curse to do more damage, and heals you.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 11 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 10 seconds, and each Cursed enemy bitten heals you 18.) It travels 5 metres along its path and deals 24 damage and 30 stagger. Each Cursed enemy it bites loses its Curse, takes 20 more damage, and heals you 12. The jaw itself doesn't add a Curse: Curse first, then bite.

**Works with.** The root mark, Grasp and Debts Called (Curse first), Dread, which it comes after on the chart, Never Sated, Hunger, and the charged blade.

**Example.** Curse a line of soldiers in a hallway with Grasp. Then send the jaw down it. Each one bitten takes 44 damage and gives you 12 health.

## dark.maw.shape
**What it does.** Unlocks two shapes for Devouring Maw. You pick one at any rest fire. **Rahu** (the head with no body that swallows the sun): a normal Cursed enemy is swallowed whole. A moment later, it rolls out of the open throat at your feet, staggered, because there is no body to hold it. A god or an elite is only bitten. **From Its Own Shadow**: the jaw rises out of the enemy's own shadow instead, up to 9 metres away, and bites it from below. There is no path; it reaches an enemy far away.

**When it happens.** Whenever you cast Devouring Maw, using the shape you picked. If you have not bought this, you cast the plain jaw.

**The numbers.** 3 Shards. Rahu: each normal Cursed enemy bitten is swallowed for 1.5 seconds. In that time it is gone from the fight: you can't see it, hit it or bump into it, and it can't do anything. Then it rolls out in front of your feet and takes 60 stagger. An enemy with no Curse is only bitten. From Its Own Shadow: one jaw bites from below on the nearest enemy within 9 metres, a moment after you cast it. It covers a circle 1.6 metres across and deals 24 damage and 30 stagger.

**Works with.** Rahu: a heavy weapon (the swallowed enemy comes out staggered at your feet, ready for a finisher), Near the Grave. From Its Own Shadow: archers and spellcasters at the back, Grasp's Tethered shape.

**Not built yet.** The jaw doesn't yet show the enemy going down its throat and rolling back out; for now, the enemy just vanishes and comes back. From Its Own Shadow should look like it rises out of the enemy's shadow, and aim at the enemy you face; for now, it rises under the nearest enemy.

**Example.** Use Rahu on a Cursed soldier. It disappears into the jaw and drops at your feet, staggered, and your heavy blow finishes it.

## dark.maw.master
**What it does.** Bite an enemy while it is winding up an attack, and its attack is swallowed and broken. Devouring Maw is ready to cast again.

**When it happens.** Each bite, while Devouring Maw is in a spell slot.

**The numbers.** 3 Shards. Only a bite on an enemy in its wind-up counts. Its attack is broken off (as a flinch, 0.4 seconds), and Devouring Maw's wait ends.

**Works with.** Watching for the enemies' wind-ups, Gone to Ash (another way to answer an attack), Dread.

**Example.** A brute raises its club. The jaw takes it in the middle of its swing, the blow never lands, and the jaw is ready to go again.

## dark.ash
**What it does.** Gone to Ash, Dark's counter. You stand with your arms crossed as grey ash runs over you. When the first blow lands, you scatter into a cloud of ash and take no damage. A moment later, you stand again behind the attacker. It is Cursed, and if you hit it within a second, that first blow hits like a heavy blow and heals you.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then let the enemies hit you.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 9 seconds, and it lasts 1.5 seconds.) It lasts 1.2 seconds. The blow you counter does nothing to you. Then every enemy in front of you within 2.5 metres is Cursed, you step 1.5 metres past the attacker, and your first weapon blow within a second lands as a heavy blow and heals you 10. If nothing hits you, the wait is half as long: 5 seconds.

**Works with.** The root mark, Dread, which it comes after on the chart, Devouring Maw (bite the enemy you came out behind), Near the Grave, and a heavy weapon.

**Not built yet.** Scattering into ash and standing up again a moment later; for now, you step right away.

**Example.** A captain swings at you. You scatter, stand up behind it, and your cut lands for double damage and gives you 10 health.

## dark.ash.shape
**What it does.** Unlocks two shapes for Gone to Ash. You pick one at any rest fire. **Ash on the Brow** (the ash on the god Shiva's forehead): you stand again where you were, and the ash settles on the attacker's forehead as a grey smear. For 6 seconds it is Cursed, the next blow it lands on you scatters you for no damage again, and your first blow on it counts as a heavy blow. The ash marks the enemy that hit you. **Ash Hangs**: you stand again where you were, and the ash stays as a cloud that slowly sinks. Enemies inside it are Cursed, and an attack they start inside it goes the way they were already facing.

**When it happens.** Whenever you cast Gone to Ash, using the shape you picked. If you have not bought this, you cast the plain counter.

**The numbers.** 3 Shards. Ash on the Brow: the attacker's Curse doesn't fade for 6 seconds, and you get one ward that blocks the next blow that would land. Ash Hangs: a cloud 3 metres across sits on the attacker for 3 seconds, Cursing whatever stands in it each second. With either shape, you stand up again where you were, not behind the attacker.

**Works with.** Ash on the Brow: a one-on-one fight with a strong enemy, Dread, Collected Twice. Ash Hangs: a pack, Dread, Debts Called's Collected shape.

**Not built yet.** Ash on the Brow's ward blocks any blow, not only the attacker's, and its grey smear isn't drawn yet. Ash Hangs should stop enemies inside it from turning their attacks; for now, they are only Cursed.

**Example.** Use Ash Hangs in a ring of soldiers. The one that hit you and the two next to it come away Cursed.

## dark.ash.master
**What it does.** Scatter a red attack, and the attacker is staggered and Gone to Ash is ready to cast again.

**When it happens.** Each blow Gone to Ash scatters, while it is in a spell slot.

**The numbers.** 3 Shards. Only a red attack counts. Scattering it gives the attacker 60 stagger and ends Gone to Ash's wait.

**Works with.** Gods and elites (their red attacks), Devouring Maw and Pawned Heart (a big hit on the staggered enemy).

**Example.** A god's red overhead swing comes down on you. You scatter, it staggers back, and the counter is ready for its next swing.

## dark.lift
**What it does.** Collected Twice, Dark's reaction lift. Every reaction the Curse is part of leaves a black thumbprint on the forehead of the enemy it hit. Your next weapon blow on that enemy repeats the reaction's damage. Your blade collects the debt.

**When it happens.** Whenever a reaction with Cursed in it happens, on any enemy. The print goes on everything the reaction hit: the enemy where it happened and every enemy its blows reached. It lasts 4 seconds, or until your next weapon blow on that enemy.

**The numbers.** 3 Shards. The print keeps the damage the reaction's blow dealt to that enemy. Your next weapon blow on it within 4 seconds hits it again for that much.

**Works with.** Every element pair with Cursed in it (Dark's reactions with its neighbours and the rest), the root mark, Dread, which it comes after on the chart, and Never Sated and Nothing Stays Buried, which come after it.

**Not built yet.** The black print isn't drawn on the head yet.

**Example.** A Curse reaction hits a soldier for 24. Your next cut on it does 24 more damage.

## dark.neversated
**What it does.** You carry no flasks at all. Instead, every blow you land heals you a little, every Cursed kill heals you a lot, and every finisher heals you even more.

**When it happens.** Always on once you learn it: with every weapon blow that lands, every Cursed enemy you kill, and every finisher.

**The numbers.** 6 Shards. You are left with one flask (the game always keeps at least one), and you don't earn any back. Each blow heals you 6% of the damage it deals. A Cursed kill heals 12, on top of the 6 the Curse already gives. A finisher heals 20.

**Works with.** The root mark, Devouring Maw (it heals too), Hunger (its healing and this one stack together), Collected Twice, which it comes after on the chart, Dead Already.

**Example.** Against a long line of enemies, never stop fighting. Each kill and finisher keeps you on your feet, the way a flask would have.

## dark.devotion
**What it does.** Nothing Stays Buried, Dark's Devotion, for going all in on Dark only. Every enemy that dies Cursed stands back up as your shade, ash-grey with violet eyes, and fights for you for a while. While you are Risen, every enemy that dies stands back up, Cursed or not. This is the vetala, a spirit that rides the dead. The Dead Stand, which used to be its own spell, lives on here.

**When it happens.** Only while you carry Dark twice (all in). Each Cursed death, and each death while you are Risen.

**The numbers.** 6 Shards. Each shade stands where its body fell for 6 seconds. Every 0.8 seconds, it hits whatever is within 2.5 metres of it for 12 damage, 12 stagger and a Curse. Three shades can stand at once; a new one takes the place of the one closest to fading. Each death raises one shade, even a Cursed death while you are Risen. Buying this also gives you the deep strike, Yama's Noose. Use it with L2 and R2 while you carry Dark alone or twice.

**Works with.** The root mark and every Dark spell (Curse enemies before they die), Dread, Collected Twice, which it comes after on the chart, Dead Already (you are always Risen, so every death stands up), A Life for a Life.

**Not built yet.** Each shade should walk to its own enemy; for now, it fights where it fell.

**Example.** Kill three Cursed soldiers in a group. Three grey shades stand up among the rest and keep cutting while you turn to the captain.

## dark.bloodprice
**What it does.** You can cast a spell that is still waiting, and pay for the wait with health. That health isn't lost right away. It turns grey, and dealing damage wins it back, like any grey health.

**When it happens.** Each time you cast a spell that costs mana without enough mana for it: another element's larger spell, or a duo spell. Dark's own spells cost no mana. They have waits instead, and Pawned Heart is paid for with health.

**The numbers.** 3 Shards. Each second left on the spell's wait costs 1 health, taken as grey health. Casting it starts its wait again. It never takes your last point. If you don't have the health, the spell isn't cast.

**Works with.** Near the Grave and Debts Called, which come after it on the chart (they reward low health), Pawned Heart, duo spells.

**Example.** You have 10 mana and cast a duo spell that costs 40. It goes off, 15 of your health turns grey, and the blows you land next win it back.

## dark.debtscalled
**What it does.** Debts Called, the breaker of the Dark tree. Every enemy nearby is dragged in to a spot just in front of you. When they get there, a burst of shadow hits them all at once. The closer you are to death, the harder it hits.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 9 seconds.) The spot is 1 metre in front of you. For half a second, everything within 6 metres of it is pulled in. Then they all take 18 damage, 35 stagger and a Curse. The damage rises smoothly as your health drops: 18 at full health, 27 at half, 36 at one point.

**Works with.** The root mark, Blood Price, which it comes after on the chart, Near the Grave and Pawned Heart (low health), Dread, Devouring Maw, and the charged blade.

**Not built yet.** The burst should also throw back the enemies it hits.

**Example.** Four soldiers are spread around you. One cast pulls them into a tight group at the tip of your sword, all Cursed, ready for a wide swing.

## dark.debtscalled.shape
**What it does.** Unlocks two shapes for Debts Called. You pick one at any rest fire. **The Black Buffalo** (the buffalo the god Yama rides): there is no pull. A buffalo made of sinking black smoke charges through the spot in front of you. It carries every Cursed enemy on its horns to the far end and throws them down. Follow its path and cut down the enemies it threw. This is Dark's only push. **Collected**: only Cursed enemies are called, from farther away, and each one that reaches you gives you health. There is no burst.

**When it happens.** Whenever you cast Debts Called, using the shape you picked. If you have not bought this, you cast the plain call.

**The numbers.** 3 Shards. The Black Buffalo: it charges 8 metres down a path about 2.4 metres wide, dealing 18 damage (rising to 36 as your health drops, like the plain spell), 35 stagger and a Curse to everything it runs through. Every enemy that was already Cursed rides on its horns to the far end and is thrown down there for 0.8 seconds (gods and bosses are too big to carry). Collected: only Cursed enemies within 10 metres are pulled in. Each takes 20 stagger and no damage, and heals you 4. Enemies without a Curse aren't moved or hit.

**Works with.** The Black Buffalo: a heavy weapon to follow its path, Dread, Devouring Maw's path. Collected: the root mark and Grasp (Curse them first), Never Sated, Near the Grave.

**Not built yet.** The buffalo should look like a buffalo made of smoke; for now, it lunges down the path in the shape of the jaw.

**Example.** Use Collected on a pack of five Cursed soldiers. They come to your feet staggering, and you get 20 health.

## dark.debtscalled.master
**What it does.** Cast it while below a third of your health, and the burst, or the buffalo, hits twice as hard.

**When it happens.** Each cast while below a third of your health, while Debts Called is in a spell slot.

**The numbers.** 3 Shards. For one second after you cast it, your spells deal double damage, on top of the rise from your low health.

**Works with.** Near the Grave, Pawned Heart (you stay at one point of health the whole time), Blood Price, Dead Already.

**Not built yet.** Only the burst or the buffalo should get stronger; for now, every spell that lands in that second does.

**Example.** Use Pawned Heart, then cast Debts Called. At one point of health, the burst hits four soldiers for 45 each.

## dark.nearthegrave
**What it does.** While you are below a third of your health, you hit harder, and each kill wraps you in a shade that blocks the next blow.

**When it happens.** Always on once you learn it, while your health is below a third. You get a shade with each kill you make while that low.

**The numbers.** 3 Shards. Below a third of your health, your weapon deals 20% more damage. Each kill below a third gives you one ward, up to three.

**Works with.** Blood Price, which it comes after on the chart, Pawned Heart and Dead Already, which come after it, Debts Called's Master mark, Hollow Strength, A Life for a Life.

**Example.** You are at 25 health in a pack. Cut harder, and each soldier that falls leaves you a shade to block the next one's blow.

## dark.pawnedheart
**What it does.** Pawned Heart, Dark's larger spell. You pay for it with health, not mana. Your health drops to one point, and the rest is set down behind you as a beating heart inside a ring that slowly closes. While the heart beats, your weapon hits much harder and swings cost little stamina. Walk away from the heart and then back onto it, and you get back all the health you gave up, plus a share of the damage you dealt in the meantime. If the ring closes first, the heart stops: you keep one point of health, and only half of what you gave up comes back, as grey health.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). You can't cast it at one point of health.

**The numbers.** 4 Shards. It costs no mana; you pay with health. After you cast it, you must wait 40 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower gives 70% more damage and makes the ring last 12 seconds.) The heart sits 1.8 metres behind you and beats for 10 seconds. While it beats, your weapon deals 60% more damage. You must step 2.6 metres away from it, then come back within 1 metre of it. You then get back all the health you gave up, plus 30% of the damage you dealt. A blow that lands on you while it beats can kill you.

**Works with.** Near the Grave, which it comes after on the chart (you are below a third of your health the whole time), Debts Called's Master mark, Dead Already, Hunger, and the wards from Near the Grave and Gone to Ash.

**Example.** Use Pawned Heart next to a staggered brute. Cut it down with five strong swings, then step back onto the heart and stand up with more health than you gave up.

## dark.pawnedheart.shape
**What it does.** Unlocks two shapes for Pawned Heart. You pick one at any rest fire. **Buried Heart**: the heart sinks into the floor where you stand and beats under it, a dark pulse inside a ring of ash. Your weapon is only strong while you stand near it. When the ring closes, the heart rises whole. Stand your ground and fight. **Hung From the Tree** (the vetala's tree): instead, the heart hangs in the air above the enemy you face, and follows it. You must fight your way in under it to touch it. You go in toward the enemy, not away.

**When it happens.** Whenever you cast Pawned Heart, using the shape you picked. If you have not bought this, the plain heart is set down behind you.

**The numbers.** 3 Shards. For now, both shapes still set the heart behind you, like the plain one. Buried Heart: a ring of ash 3 metres across appears where you cast it and stays for 10 seconds, Cursing whatever stands in it each second. Hung From the Tree: the enemy you face keeps its Curse for the 10 seconds.

**Works with.** Buried Heart: holding your ground against a pack that comes to you, Gone to Ash. Hung From the Tree: a one-on-one fight with a strong enemy, Grasp's Tethered shape.

**Not built yet.** Buried Heart's heart under your feet, its power only working within 3 metres, and the heart rising whole with 30% of the damage dealt when the ring closes. Hung From the Tree's heart hanging over the enemy and following it. Both are waiting until the heart can be placed somewhere other than behind you.

**Example.** Use Buried Heart in a doorway. The soldiers that come through stand in your ring of ash and come away Cursed.

## dark.pawnedheart.master
**What it does.** Touch the heart during a perfect dodge, and you get back 60% of the damage you dealt instead of 30%.

**When it happens.** When you step onto the heart within 0.6 seconds of a perfect dodge (dodge onto it at the last moment), while Pawned Heart is in a spell slot.

**The numbers.** 3 Shards. The heart gives back everything you pawned, plus 60% of the damage you dealt while it beat, instead of 30%.

**Works with.** Clean dodging, Near the Grave (low health, when a heal matters most), Gone to Ash.

**Example.** You are at one point of health with the heart beating, and you dealt 200 damage. Roll through a soldier's thrust at the last moment and onto the heart: you get back all you pawned and 120 more.

## dark.deadalready
**What it does.** You don't wait to die: from now on, you are always Risen. In every fight, your health drains toward its last point, and your blows heal you. You hit much harder. You don't get a second Rise.

**When it happens.** Always on once you learn it. The drain only runs while an enemy is within 12 metres of you.

**The numbers.** 6 Shards. Your weapon deals 25% more damage, and you get 30% of each blow's damage back as health. The drain takes 2 health a second, but never your last point. Every mark that needs you to be Risen is always working. A Life for a Life can no longer stand you up.

**Works with.** Near the Grave, which it comes after on the chart, Hollow Strength (another 25%, no flinching, and kills pause the drain), Nothing Stays Buried (every death stands up), Hunger (another 30% back), Never Sated.

**Example.** Fight without stopping. Every cut gives back much more than the drain takes. The moment you stop and wait, it bleeds you down.

## dark.hollowstrength
**What it does.** While you are Risen, you hit harder and no blow makes you flinch. Each kill pauses the drain on your health for a few seconds.

**When it happens.** While you are Risen: after A Life for a Life stands you up, or all the time with Dead Already.

**The numbers.** 3 Shards. While Risen, your weapon deals 25% more damage, and blows that land on you don't stop you. Each kill stops the drain for 3 seconds.

**Works with.** A Life for a Life, Dead Already, Second Shadow, which comes after it on the chart, Nothing Stays Buried (while Risen, every death stands up), Near the Grave.

**Example.** You are Risen with only a sliver of health. Swing straight through the soldiers' blows. Each one you kill gives you three seconds without the drain.

## dark.base
**What it does.** Grasp, Dark's own spell. It comes with the element; you never buy it. A black ball crawls along the ground to the enemy you face, pulling in what it passes. Where it arrives, it hits and Curses, and the enemies it has hold of are dragged to your feet.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Dark, you can always put it in a slot.

**The numbers.** Comes with Dark. It costs no mana. After you cast it, you must wait 7 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 6 seconds, the reach 8 metres and the damage 14. It reaches the enemy you face if it is within 6 metres, and deals 10 damage, 12 stagger and a Curse to everything within 1.7 metres of where it arrives.

**Works with.** Its Empower, its two shapes and its Master mark; the root mark, Dread, Devouring Maw and Debts Called's Collected shape (they all want Cursed enemies), and the charged blade, which it charges with Dark.

**Example.** Grasp the archer at the back. It is dragged to your feet, Cursed, and the jaw finishes it.

## dark.base.shape
**What it does.** Unlocks two shapes for Grasp. You pick one at any rest fire. **Tethered** (the god Yama's noose): instead of the ball, you throw a black rope that ties the enemy you face to you. It can't get far from you, and your first heavy blow on it yanks it to your feet. You keep it close. **The Pit**: the ball stops where you aim and spreads into a pool of ink. It slowly drags everything in it to the middle and Curses it. You gather enemies in one spot.

**When it happens.** Whenever you cast Grasp, using the shape you picked. If you have not bought this, you cast the plain ball.

**The numbers.** 3 Shards. Tethered: for 4 seconds after Grasp hits an enemy, it can't go more than 5 metres from you; if it is farther, it is dragged back toward you. Your first heavy blow on it in that time drags it 4 metres toward you, gives it 20 stagger, and ends the tether. Gods can't be tied. The Pit: a pool 3 metres across, 6 metres ahead of you, that lasts 4 seconds. Each second, it deals 2 damage and a Curse to whatever stands in it.

**Works with.** Tethered: a one-on-one fight, Devouring Maw's From Its Own Shadow shape, a heavy weapon. The Pit: Dread, Debts Called, Ash Hangs, a pack.

**Not built yet.** Tethered should throw a rope instead of the ball; for now, the ball is cast. The Pit should drag everything to its middle; for now, it only deals damage and Curses. If you buy the Empower, The Pit uses its numbers too, and lands 8 metres ahead.

**Example.** Put The Pit under a pack in a doorway. In four seconds they are all Cursed, and Dread is ready for the first death.

## dark.base.master
**What it does.** Cast it within a second of being hit, and the enemy that hit you is dragged to your feet and Cursed twice. Dark strikes back when it is hurt.

**When it happens.** When you cast Grasp within one second after a blow lands on you.

**The numbers.** 3 Shards. The enemy whose blow landed on you is dragged 4 metres toward you and gets two Curses, wherever Grasp itself goes.

**Works with.** Pawned Heart and Near the Grave (getting hurt is the plan), Gone to Ash, Dread.

**Example.** A spearman pokes you from far away. Answer right away, and it is at your feet and Cursed.

## dark.secondshadow
**What it does.** Second Shadow, Dark's movement spell. You leave your shadow where you stand: a flat black copy of you with violet eyes, joined to you by a thin line on the ground. For a while, it repeats every blow you land, a moment later, for half the damage, and Curses the enemy.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). After that, every weapon blow that lands is repeated.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 10 seconds.) The shadow stays for 8 seconds. Each repeated blow lands a moment after yours, for 50% of its damage, and adds a Curse. You can only have one shadow at a time.

**Works with.** The root mark, Hollow Strength, which it comes after on the chart, Pawned Heart (half of a much bigger blow), Near the Grave, Hunger.

**Not built yet.** Pressing the button again to swap places with the shadow, once.

**Example.** Cast it while a brute kneels, staggered. Every cut of your finisher lands twice.

## dark.secondshadow.shape
**What it does.** Unlocks two shapes for Second Shadow. You pick one at any rest fire. **Return to the Shadow**: the shadow fights like the plain one, but pressing again doesn't swap places. Instead, while the shadow lasts, press again to sink into it and rise where it is. Every wound you took since you left it comes back as grey health you can win back. You rewind your health, but you go back to where you were. **Long Shadow**: your shadow stretches out along the floor in front of you, long like an evening shadow. You sink into its foot and rise at its head a moment later, and every enemy it lay across is Cursed. There is no copy, just the path.

**When it happens.** Whenever you cast Second Shadow, using the shape you picked. If you have not bought this, you cast the plain shadow.

**The numbers.** 3 Shards. Return to the Shadow: for the shadow's 8 seconds, you take a quarter less damage from every blow. Long Shadow: you travel 8 metres ahead in about a third of a second, and every enemy within a metre of the path takes 2 damage and a Curse.

**Works with.** Return to the Shadow: Pawned Heart, Near the Grave, a fight where you plan to hold your ground. Long Shadow: Debts Called and Grasp (a path into the pack, then the call), Dread.

**Not built yet.** Return to the Shadow's second press, and getting your wounds back as grey health; for now, taking a quarter less damage takes its place. Long Shadow should hide you as you travel under the floor; for now, enemies can see you go.

**Example.** Use Long Shadow through a line of three soldiers. You rise behind them, and all three are Cursed.

## dark.secondshadow.master
**What it does.** Swap places within a second of being hit, and the blow moves to the shadow, and you get back the health it took.

**When it happens.** For now, each time you cast it, while Second Shadow is in a spell slot.

**The numbers.** 3 Shards. For now, each cast gives you one ward that blocks the next blow that would land on you, up to three.

**Works with.** Near the Grave (it gives wards too), Pawned Heart (one point of health, one blow blocked), Gone to Ash.

**Not built yet.** Swapping places within a second of being hit, the blow moving to the shadow, and getting its health back; for now, the ward on each cast takes its place.

**Example.** Cast the shadow before a brute's string of blows. Its first blow breaks on nothing.

## dark.lifeforalife
**What it does.** When you are Risen (Dark's root mark stood you up from a blow that would have killed you), a symbol hangs over the enemy that killed you. Land a finisher on it, and you come back to life: your health returns, and you can rise once more.

**When it happens.** While you are Risen. A finisher on the enemy that killed you ends the Rise. If that enemy is already gone, a finisher on any enemy does.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. The finisher gives back half your full health and makes the Rise ready again. Without the gift, you stay Risen, draining down to one point. If you are killed then, you fall. After a fall, you can rise again.

**Works with.** Hollow Strength, Nothing Stays Buried (while Risen, every death stands up), Near the Grave, Never Sated, Hunger.

**Example.** A brute's red overhead swing kills you. The root mark stands you up, Risen. Break the brute's balance and finish it. You stand up with half your health, and your Rise is ready again.
