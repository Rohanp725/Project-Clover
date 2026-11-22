# Poison: detailed explanations

The long explanation the chart shows for each high-level mark of the Poison tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Poison): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Poisoned" is Poison's affliction: doses that lie up to
eight deep and bite for all their depth at once on a slow beat. "The beat" is that bite, every 2 seconds on each enemy;
a blow or a cast "on the beat" comes within a fifth of a second of it.

## poison.venom
**What it does.** Your weapon poisons. Every blow that lands adds a dose of poison. A heavy blow adds one more, and a finisher adds even more. The poison doesn't hurt a little at a time. Instead, on a slow beat, it bites for all its doses at once. You also get a tenth of the damage your poison deals back as health, so poison always heals a Poison player.

**When it happens.** Always on once you learn it. Your blows only add doses while Poison is your first element, because your weapon only adds your first element's affliction. If Poison is your second element, its doses come from your spells, the charged blade and the R2 strike instead. The healing works no matter who added the poison.

**The numbers.** 2 Shards. One dose per blow; a heavy blow adds two, a finisher four. Up to eight doses, lasting 6 seconds, and each new dose restarts the timer. Every 2 seconds, the poison bites for 2 damage per dose: 16 at eight. When it is set off, it deals 6 damage per dose all at once: 48 at eight. You get back a tenth of the damage the poison really deals, at each bite and each time it is set off (1.6 for a bite of 16, 4.8 for a burst of 48). Stronger or ripened poison heals you more. The health comes back over about 2 seconds, not all at once.

**Works with.** Rupture and Volatile to set it off, Catching to spread it, In Time with its beat, every Poison spell, Fever, and Rot Roots when a Seeded enemy is poisoned.

**Example.** Four light blows and a heavy blow give a soldier six doses. Roll away, and the next bite takes 12 health from it and gives you a little over 1.

## poison.rupture
**What it does.** A heavy blow on an enemy whose poison is full bursts the poison. All of it lands at once, on that enemy and everyone close to it, and the doses are used up.

**When it happens.** A heavy blow or finisher that lands on an enemy with full poison.

**The numbers.** 3 Shards. It is set off for 6 damage per dose (48 at eight) and 4 stagger per dose, to everything within 1 metre of the enemy you hit. At most about three times a second. Full is eight doses, or twenty with Slow Death.

**Works with.** Venom (fill it up with blows, finish with a heavy blow), Blister and One by One, which come after it on the chart, Volatile, Coiled Strike (four doses toward full), and the root mark's healing (each burst heals you).

**Example.** A soldier has eight doses. Land a heavy blow: 48 damage lands at once with the blow, and the soldier next to it takes the same.

## poison.blister
**What it does.** Blister, the detonator of the Poison tree. You raise a blister on the enemy you face. It swells each time its poison bites, through three sizes, and a heavy blow pops it in a burst that poisons everything around it. If you leave it too long, it shrivels up.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). If you aren't facing an enemy, no blister is raised, and you still have to wait.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 50 damage.) It targets an enemy within 8 metres. Only one blister can be up at a time, and it lasts 12 seconds. The burst has a 3-metre radius around the enemy and deals 40 damage, 30 stagger and four doses.

**Works with.** The root mark, Rupture, which it comes after on the chart (four doses toward full), Coiled Strike (step in and pop it from behind), In Time, and the charged blade.

**Not built yet.** Swelling through three sizes: 20, 40 and 70 damage, within 2, 3 and 4 metres, with 2, 4 and 6 doses. For now, it bursts at the middle size. Only a heavy blow should pop it; for now, any weapon blow on it pops it.

**Example.** Put a blister on the brute in the middle of a group and cut it once. Everyone around it gets four doses.

## poison.blister.shape
**What it does.** Unlocks two shapes for Blister. You pick one at any rest fire. **Weeping Sore**: it never bursts on its own or from a heavy blow. Each time its poison bites, it leaks a bubbling puddle where the enemy stands, so a walking enemy spreads your poison. Your heavy blow bursts every puddle at once. It is slow and sneaky, not a swell and a pop. **The Vessel** (like the court's elixir-drinkers): the whole enemy swells up, and when its poison bites while full, it bursts on its own into a cloud that poisons everything in it. You set it and leave it.

**When it happens.** Whenever you cast Blister, using the shape you picked. If you have not bought this, you cast the plain blister.

**The numbers.** 3 Shards. Weeping Sore: for 12 seconds, each bite of the enemy's poison leaves a puddle 1.5 metres across for 3 seconds that gives a dose each second to whatever stands in it. A heavy blow on the sore bursts 2 metres around it for 12 damage and two doses, at most twice a second. The Vessel: for 12 seconds, the first bite while its poison is full puts three doses on everything within 2 metres, and leaves a cloud 4 metres across for 3 seconds that gives a dose each second. Once per cast. With either shape, no blister is raised, so no blow can pop one.

**Works with.** Weeping Sore: an enemy that walks around, Catching, Low Mist. The Vessel: Venom's blows to fill it up, Catching, Slow Death (full is twenty, so it waits longer). Volatile stops the beat, and neither shape leaks or bursts without bites.

**Not built yet.** Weeping Sore's heavy blow bursts around the sore, not at each puddle where it lies.

**Example.** Use Weeping Sore on a charging brute. It leaves a trail of puddles as it comes, and the soldiers following it walk through them.

## poison.blister.master
**What it does.** Pop it with a heavy blow right as its poison bites, within a fifth of a second, and the burst doubles.

**When it happens.** The first heavy blow on the blistered enemy, while Blister is in a spell slot.

**The numbers.** 3 Shards. For 12 seconds after you cast it, the first heavy blow on that enemy, if it lands within a fifth of a second of its bite, sets off a second burst just as big around it: 40 damage, 30 stagger and four doses within 3 metres.

**Works with.** In Time (the same moment), Coiled Strike's heavy blow made into a critical hit, a heavy weapon.

**Not built yet.** The blister's own burst should double. For now, a second burst lands with it, and only if the heavy blow is the one that pops it (a light blow pops the plain blister first).

**Example.** Watch the bubbles on the blistered captain. As they pop, bring the heavy blow down: two bursts, 80 damage to everything around it.

## poison.onebyone
**What it does.** One by One, Poison's larger spell, paid for with mana. You raise a hand, and a green line runs along the ground from poisoned enemy to poisoned enemy. Then you snap your fingers, and each one's poison bursts in turn, each one passing its doses to the next before it goes. The last burst is the biggest.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards, 50 mana (its Empower makes it 7 damage per dose instead of 6). It reaches up to six poisoned enemies within 10 metres of you. The line starts at the poisoned enemy you face (or the nearest one) and runs to the poisoned enemy nearest that one, and so on. Enemies with no poison are skipped and never hit. After half a second's warning, a column rises under each one in turn, every 0.07 seconds, 1 metre across. The column itself does no damage. Each enemy's poison is set off: 6 damage and 4 stagger per dose to everything within 1 metre (48 damage and 32 stagger at eight). Each burst also carries every dose of the bursts before it, so three enemies with 2, 3 and 4 doses burst with 2, 5 and 9. Whatever a column hits is thrown into the air for 0.8 seconds.

**Works with.** Rupture, which it comes after on the chart, Venom, Blister, Catching and Miasma to add the poison first, the root mark's healing (every burst heals you), and the charged blade.

**Example.** Poison a pack with Miasma and a rush of Share Alike, then snap your fingers. Six bursts go off in a moment, and each one heals you.

## poison.onebyone.shape
**What it does.** Unlocks two shapes for One by One. You pick one at any rest fire. **Eaten in Turn** (the gu jar, where the order of killing matters): the line is drawn, but nothing bursts. For a while, when an enemy on the line dies, all its doses pass to the next one on the line, doubled. One pushed past eight doses bursts right away and passes it on. Your blade chooses the order. **All Into One** (like the quicksilver of the Metal and Poison reaction): no line. The poison runs out of every poisoned enemy nearby as beads of mercury that join together in the enemy you face. Their doses are added to its own, with no limit. Then that one enemy bursts alone. It's for the one enemy that matters.

**When it happens.** Whenever you cast One by One, using the shape you picked. If you have not bought this, you cast the plain line.

**The numbers.** 3 Shards. Eaten in Turn: the line is drawn, but the snap bursts nothing. For 8 seconds, each enemy on the line that dies poisoned passes twice its doses to the nearest other enemy still on the line (or, if none is left, the nearest enemy within 10 metres). If that pushes it past eight doses, it bursts at once, 6 damage a dose, and the extra doses pass on to the next one. This can happen up to four times in a row. All Into One: when you cast it, every poisoned enemy within 10 metres gives up all its doses. One column rises under the poisoned enemy you face (or the nearest one), and its burst carries every dose gathered, with no limit: four enemies with eight doses each make one burst of 32 doses.

**Works with.** Eaten in Turn: a pack of weak enemies to kill in order, Slow Death (twenty doses to pass on), Rupture and Volatile on the enemy that gets fed. All Into One: a god or an elite, Catching and Miasma to poison the crowd first.

**Not built yet.** In Eaten in Turn, the doses go to the nearest enemy still on the line, not the next one in the line's own order. All Into One's beads of mercury rolling across the floor aren't drawn yet.

**Example.** Use Eaten in Turn on five soldiers. Poison them again as you cut, kill the front one with six doses, and the next one has twelve.

## poison.onebyone.master
**What it does.** Snap your fingers within a fifth of a second of the poison's beat, and every burst is one and a half times as strong.

**When it happens.** When you snap your fingers within a fifth of a second of a poison bite on any enemy within 8 metres, while One by One is in a spell slot.

**The numbers.** 3 Shards. For the next 1.5 seconds, while the columns rise, One by One's own bursts deal 50% more damage. Other bursts at the same time, like a reaction's or Volatile's, don't get stronger.

**Works with.** In Time (the same beat), Catching (the beat spreads across the pack), Venom.

**Example.** Watch the pack's poison bite and raise your hand right then. The columns that land on the bite hit 50% harder.

## poison.volatile
**What it does.** Your poison no longer bites on its beat. It builds up until it is full, then bursts on its own, uses itself up, and starts again.

**When it happens.** Always on once you learn it. It bursts the moment any affliction fills up on a poisoned enemy. Usually that is the poison reaching full, but another element's affliction filling up bursts it too, at however many doses it has.

**The numbers.** 6 Shards. The burst deals 7 damage per dose (56 at eight) and 4 stagger per dose to everything within 2.5 metres of the enemy. At most twice a second. Full is eight doses, or twenty with Slow Death (140 damage then).

**Works with.** Venom, Rupture, which it comes after on the chart, Slow Death, One by One, and the root mark's healing (each burst heals you). Catching, In Time, Weeping Sore, The Vessel and the beat Master marks all need bites, and there aren't any.

**Example.** Eight light blows on a soldier, and it goes up on the eighth, taking the two next to it with it.

## poison.intime
**What it does.** Hit right as the poison bites, and your blow adds two doses instead of one. For a moment after that timed blow, every blow you land is a critical hit.

**When it happens.** A weapon blow that lands on a poisoned enemy within a quarter of a second of its poison biting, just before or just after. The extra dose only comes while Poison is your first element; the critical moment comes either way.

**The numbers.** 3 Shards. One more dose on the timed blow. For 1.2 seconds after it, your blows are critical hits (one and a half times the damage). For now, the timed blow itself isn't a critical hit, only the ones after it.

**Works with.** Venom, Five Needles and Coiled Strike, which come after it on the chart, every beat Master mark (the same moment), Blister's Master mark, and a heavy blow or finisher during the critical moment.

**Example.** Watch the bubbles. As they pop, land a light blow, then a heavy blow right after, and the heavy blow is a critical hit.

## poison.needles
**What it does.** Five Needles, the breaker of the Poison tree. Five long needles, dripping with venom, are thrown in a narrow fan at the enemy you face. An enemy hit by all five is pinned in place: held for a moment, with its balance badly shaken.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 14 stagger per needle, 70 in total, and 50 for the pin.) It is a fan 8 metres long and 12 degrees wide. It deals 15 damage, 50 stagger and five doses (all five needles' worth) to everything in it. The first enemy it hits is pinned: held for 0.8 seconds (0.32 for a god or an elite) and given 40 more stagger.

**Works with.** In Time, which it comes after on the chart, Rupture (five doses toward full), Coiled Strike (strike through the pinned enemy), and the charged blade.

**Not built yet.** Five separate needles, each hitting on its own. For now, the fan hits as one, and the pin goes on the first enemy hit, not the one hit by all five.

**Example.** A shield bearer turns to face you. The needles pin it, and you have most of a second to walk around its shield.

## poison.needles.shape
**What it does.** Unlocks two shapes for Five Needles. You pick one at any rest fire. **Spread**: a wide fan, so each needle finds a different enemy. Each enemy hit gets two doses, and its poison bites right away. **Red Paper** (the five poisons pinned to red paper; the needle grids in the floor are the Poison god's own weapon): all five needles pin one sheet of red paper to the enemy you face. For six seconds, it can't guard or block a blow, and your blows on it go through any shield. It's the breaker for enemies that hide behind their guard.

**When it happens.** Whenever you cast Five Needles, using the shape you picked. If you have not bought this, you cast the plain fan.

**The numbers.** 3 Shards. Spread: a fan 60 degrees wide. It deals 3 damage, 10 stagger and two doses to each enemy in it, and each one's poison bites right away. Nothing is pinned. Red Paper: the first enemy hit can't raise its guard for 6 seconds.

**Works with.** Spread: a crowd, Catching (every bite throws a dose onward), In Time. Red Paper: enemies that guard and captains, a heavy weapon, Blister.

**Not built yet.** Spread is one fan, not five needles. Red Paper should stop the enemy from blocking blows and let your blows go through shields.

**Example.** Use Red Paper on a captain who guards everything. For six seconds, your whole chain of blows lands.

## poison.needles.master
**What it does.** Throw them on a poisoned enemy's beat, within a fifth of a second of its bite, and every needle that hits it makes its poison bite one more time.

**When it happens.** Each poisoned enemy the needles hit, while Five Needles is in a spell slot.

**The numbers.** 3 Shards. For now, an enemy hit within a fifth of a second of its bite bites once more right away, for all its doses: 16 damage at eight.

**Works with.** In Time (the same beat), Catching (every bite throws a dose onward), the root mark's healing (every bite heals you), Spread.

**Not built yet.** One extra bite for every needle. For now, there is one extra bite for the whole fan.

**Example.** A soldier has eight doses. Throw on its bite, and it is bitten twice in a moment, for 32 damage.

## poison.coiled
**What it does.** Coiled Strike, Poison's counter. You coil up, then strike through the enemy you face in an instant and come out on its far side, facing back, cutting everything on the line and poisoning it heavily. If the cut lands, your next heavy blow soon after is a critical hit. Nothing can hit you during the strike. A wall, or an enemy too big to push past, stops you short.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 30 damage.) It goes up to 6 metres, ending 1.8 metres past the enemy you face (or 4.5 metres ahead if there is none). The cut reaches 1.1 metres to each side of the line. It deals 24 damage, 25 stagger and four doses, the strength of two fangs. A heavy blow within 3 seconds is a critical hit.

**Works with.** In Time, which it comes after on the chart, Slow Death, which comes after it, Rupture (four doses toward full), Blister, and the charged blade: if it lands, it charges your weapon with Poison.

**Not built yet.** Holding the coil for up to 2.4 seconds while three fangs light up (14, 24 or 38 damage and 2, 4 or 8 doses, depending on the fangs). For now, it strikes right away as two fangs. Letting go right as a blow is about to land should make the blow pass through you, for three fangs.

**Example.** Strike through a shield bearer to its back, turn around, and the heavy blow you owe it is a critical hit.

## poison.coiled.shape
**What it does.** Unlocks two shapes for Coiled Strike. You pick one at any rest fire. **Centipede's Run** (the centipede, one of the five poisons): let go, and you strike through up to three enemies in a zigzag of short rushes, biting each one, with the fangs' doses shared between them. You end up behind the last one. The snake strikes one; the centipede strikes many. **Shed Skin** (the snake that sheds its skin): let go during the perfect timing window, and you leave your skin standing where you were, pale and papery. The enemy keeps attacking it while your strike lands from behind.

**When it happens.** Whenever you cast Coiled Strike, using the shape you picked. If you have not bought this, you cast the plain strike.

**The numbers.** 3 Shards. Centipede's Run: a straight rush 12 metres long through everything on a line 2 metres wide, dealing 14 damage, 15 stagger and two doses to each (30 damage each once you buy the Empower, which sets its number on the rush too). Shed Skin: every enemy within 3 metres of where you stood loses sight of you for 1.5 seconds.

**Works with.** Centipede's Run: a row of enemies, Catching, Rupture. Shed Skin: a pack that crowds you, Blister (pop it from behind), a heavy weapon.

**Not built yet.** Centipede's Run's zigzag of three short rushes, ending behind the last enemy; for now, it is one straight run. Shed Skin should only work during the perfect timing window, and should show a skin standing there; for now, every cast blinds the enemies near where you stood.

**Example.** Use Centipede's Run down a line of archers. Each one gets two doses, and you end up past the last.

## poison.coiled.master
**What it does.** Let go during the perfect timing window, and Coiled Strike is ready again.

**When it happens.** While Coiled Strike is in a spell slot.

**The numbers.** 3 Shards. For now, a perfect dodge ends Coiled Strike's wait, once every 10 seconds.

**Works with.** Watching for the enemies' wind-ups, Shed Skin, In Time.

**Not built yet.** It should need you to let go during the perfect timing window. For now, any perfect dodge does it.

**Example.** Dodge a captain's swing perfectly, and Coiled Strike is ready to put you at its back.

## poison.slowdeath
**What it does.** Your weapon does much less damage. But your poison goes much deeper, and it never fades while the enemy carrying it is alive.

**When it happens.** Always on once you learn it. The extra doses only work while Poison is your first element; the poison stops fading either way.

**The numbers.** 6 Shards. Your weapon deals 30% less damage. Poison can hold twelve more doses, up to twenty. At twenty, a bite deals 40 damage every 2 seconds, and setting it off deals 120. Poison you add doesn't run out.

**Works with.** Venom and its healing (a deeper bite heals you more), Coiled Strike, which it comes after on the chart, Catching, Volatile (it fills up at twenty), Rupture (full is now twenty), One by One's Eaten in Turn shape, Clean Blade.

**Example.** Give a Gate Captain twenty doses and spend the rest of the fight guarding. It takes 40 damage every two seconds, and the poison never wears off.

## poison.catching
**What it does.** Each time poison bites, one dose jumps from that enemy to the nearest other enemy in reach.

**When it happens.** Every bite of poison on any enemy, no matter how it was added.

**The numbers.** 3 Shards. One dose to the nearest enemy within 4 metres.

**Works with.** Venom, and Miasma and the reaction lift The Delayed Touch, which come after it on the chart, Five Needles' Spread shape and Master mark (more bites, more jumps), Share Alike, The Vessel.

**Example.** Poison one soldier heavily and keep it near its friends. Every two seconds, one of them gets a dose without you hitting it.

## poison.base
**What it does.** Miasma, Poison's own spell. It comes with the element; you never buy it. A low cloud rolls out in front of you, hangs there, creeps after the nearest enemy, and gives a dose of poison to whatever stands in it once a second. Bubbles rise through it, and it leaves a stain. You can stand in it without getting hurt.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Poison, you can always put it in a slot.

**The numbers.** Comes with Poison. It costs no mana. After you cast it, you must wait 7 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 6 seconds. The cloud lands 4 metres ahead of you, is 2.5 metres across, lasts 4 seconds, and gives a dose once a second to whatever stands in it.

**Works with.** Its Empower, its two shapes and its Master mark; the root mark, Catching, which it comes after on the chart, Rupture and One by One (doses for them to burst), and the charged blade, which it charges with Poison.

**Not built yet.** The Empower's bigger cloud, 3 metres across. For now, the spell only gets the Empower's shorter wait.

**Example.** Fill a doorway with the cloud where a pack must come through, then guard. Each one that comes through is poisoned.

## poison.base.shape
**What it does.** Unlocks two shapes for Miasma. You pick one at any rest fire. **Low Mist** (from this land's hours of the miasma, when mist pools low and drives people uphill): the cloud is heavier than air and pours out from your feet along the floor in the direction you face. It is a low sheet with a hard top edge, and it stops at walls. Enemies in it get a dose once a second, and a dodge or roll in it costs them a dose. It is a long path, where the plain cloud is a circle. **Censer** (it used to be Swinging Censer): you carry the cloud. A censer swings around you on its chain, and each pass deals damage and a dose and leaves smoke hanging in the air. The fight comes with you.

**When it happens.** Whenever you cast Miasma, using the shape you picked. If you have not bought this, you cast the plain cloud.

**The numbers.** 3 Shards. Low Mist: a strip of mist 8 metres long and 2.5 metres wide from your feet, lasting 5 seconds, giving a dose each second to whatever stands in it. The plain cloud is not cast. Censer: one censer swings around you 3 metres away for 8 seconds, going around once every 1.2 seconds. It hits whatever is within 1 metre of it for 4 damage, 4 stagger and a dose.

**Works with.** Low Mist: a hallway, Weeping Sore, Tasted Every Herb. Censer: a pack that crowds you, Catching, holding your guard while it swings.

**Not built yet.** Low Mist doesn't stop at walls or take a dose when an enemy dodges. Censer's smoke should hang in the air for 2 seconds after each pass.

**Example.** Use Censer and hold your ground in a doorway. Anything that crowds you gets poisoned with every swing while you guard.

## poison.base.master
**What it does.** Cast it on the poison's beat, within a fifth of a second of a bite on any nearby enemy, and the cloud gives two doses a second.

**When it happens.** When you cast Miasma within a fifth of a second of a poison bite on any enemy within 8 metres, while Miasma is in a spell slot.

**The numbers.** 3 Shards. Every enemy that cast's cloud poisons gets one more dose each second for 4 seconds.

**Works with.** In Time (the same beat), Catching, every other beat Master mark.

**Not built yet.** The cloud itself doesn't dose twice a second yet; for now, each enemy it poisons carries the second dose with it.

**Example.** Cast it as a poisoned soldier's poison bites, and the cloud gives it two doses a second while it stands there.

## poison.lift
**What it does.** The Delayed Touch, Poison's reaction lift, from the legend of the touch that kills later. Every reaction that poison is part of is held inside the body. A thin ring of mist closes in on it, and the reaction lands again later at double strength, poisoning everything it reaches. A heavy blow on that enemy makes it happen right away. Poison makes them pay later, and your heavy blow chooses the moment.

**When it happens.** Whenever a reaction with Poisoned in it happens, on any enemy. It lands two and a half seconds later, or at your first heavy blow on that enemy before then.

**The numbers.** 3 Shards. A burst with a 2.5-metre radius around the enemy. It deals 20 damage, 20 stagger and two doses to everything it reaches.

**Works with.** Every reaction with Poisoned in it (Fire Medicine, Nerve Fire, Quicksilver, Venom Rime, Rot Roots, and the rest), Catching, which it comes after on the chart, Share Alike and Tasted Every Herb, which come after it, and a heavy weapon.

**Not built yet.** The reaction itself should be held and land again at double strength. For now, a burst of its own lands when the hold ends. Burning Sap, which already waits, should only get the double strength. The closing ring of mist.

**Example.** Quicksilver goes off on a soldier. Two and a half seconds later, or when your heavy blow lands, it bursts again and poisons the two soldiers next to it.

## poison.sharealike
**What it does.** Share Alike, the movement spell. You rush straight through everything in front of you, untouchable, and come out the far side facing back. Every enemy you pass is hit and poisoned, and then every one of them is raised to the same number of doses as the sickest one.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it reach 9 metres.) It goes up to 7 metres along a path 2 metres wide. A wall, or an enemy too big to push past, stops you. Each enemy you pass takes 10 damage, 10 stagger and two doses.

**Works with.** The Delayed Touch, which it comes after on the chart, Catching, One by One (a line of poisoned enemies), Rupture, and the charged blade: if it lands, it charges your weapon with Poison.

**Example.** Rush through a line of three archers. Each one is poisoned, and you end up behind them.

## poison.sharealike.shape
**What it does.** Unlocks two shapes for Share Alike. You pick one at any rest fire. **Carried on the Blade**: you rush through like the plain spell, but you carry the sickest enemy's number of doses on your blade, and your next five weapon blows each set whatever they hit to that number. Venom drips heavily from the edge, five drops to count down. The rush loads your weapon. **Shed Tail** (like a gecko): you rush backward, away, and a thrashing tail is left where you stood, lashing everything near it. You get away, and whatever follows you pays.

**When it happens.** Whenever you cast Share Alike, using the shape you picked. If you have not bought this, you cast the plain rush.

**The numbers.** 3 Shards. Carried on the Blade: your blade takes the sickest number of doses among the enemies you rushed through. Your next five weapon blows, on any enemy, each raise what they hit to that number. A blow on an enemy that already has that many still counts as one of the five. Shed Tail: where you stood, for 2 seconds, everything within 2 metres gets three doses every half second.

**Works with.** Carried on the Blade: a fast weapon, Rupture, In Time. Shed Tail: enemies that chase you, Catching, Low Mist.

**Not built yet.** Shed Tail should rush you backward; for now, you rush forward like the plain spell, and the tail is left where you started. The heavy venom dripping from the edge isn't drawn yet.

**Example.** Use Carried on the Blade through a pack where one soldier has seven doses, then turn on the captain. Each of your next five blows brings what it hits up to seven.

## poison.sharealike.master
**What it does.** Rush on the poison's beat, within a fifth of a second of a bite on any enemy you pass, and the doses double.

**When it happens.** When you rush within a fifth of a second of a poison bite on any enemy within 8 metres, while Share Alike is in a spell slot.

**The numbers.** 3 Shards. Each enemy that rush passes gets two more doses.

**Works with.** In Time and every beat Master mark, Catching (the beat spreads across the pack), One by One afterward.

**Example.** Rush through a poisoned pack as their poison bites, and each one comes out with four more doses.

## poison.devotion
**What it does.** Tasted Every Herb, Poison's Devotion, for going all in on Poison only. No poison, cloud or mist can hurt you, whether it is yours or the enemy's, including the court's miasma. Standing in any cloud heals you, and your blows add two doses while you stand in one. Where everyone else leaves the mist, you fight in it. It is named for Shennong, who tasted every poison, and for realgar wine, poison used against poison.

**When it happens.** Only while you carry Poison twice (all in).

**The numbers.** 6 Shards. You heal 2 a second while an enemy is nearby, and every time you add doses, you add one more. Buying this also gives you the deep strike, Nine Times Refined. Use it with L2 and R2 while you carry Poison alone or twice.

**Works with.** Miasma and its Low Mist shape, Venom, The Delayed Touch, which it comes after on the chart, Fever, Clean Blade.

**Not built yet.** Poison, clouds and mist should not hurt you; for now, the enemy's clouds hurt you like before. The healing and the extra dose should only work while you stand in a cloud; for now, they work in every fight.

**Example.** A god fills the floor with its own mist. You stay in it, healing, and every blow you land there adds two doses.

## poison.cleanblade
**What it does.** While your surge lasts, the poison waits: it doesn't bite and it doesn't fade, and every bite it would have made is saved up. Your blows also add one more dose. When the surge ends, time slows down, you flick your blade clean, and every saved-up bite on every poisoned enemy nearby lands at once.

**When it happens.** Light the surge (tap L2, or press F); it starts with whichever surge you have picked. The flick comes when the surge ends. The extra dose only works while Poison is your first element.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. One more dose per blow, on top of Fever's. When the surge ends, every enemy within 14 metres takes one bite for every full 2 seconds it waited, at however many doses it has then, plus 20 stagger. Fever's 10 seconds save up five bites: 80 damage on an enemy with eight doses.

**Works with.** Venom, Fever, Slow Death (deeper bites), and Share Alike and Miasma to poison a pack while it waits. In Time, Catching and the beat Master marks need bites, and there aren't any while the poison waits.

**Example.** Light Fever, give a whole pack eight doses while the poison waits, and let the surge run out. Each enemy takes 80 at once.

## poison.plaguebearer
**What it does.** Plague Bearer, a keystone. You carry the sickness. Every enemy near you is poisoned without you hitting it, and more the longer it stays. Your flasks heal less.

**When it happens.** Always on once you learn it, while you carry Poison, every second and a half.

**The numbers.** 6 Shards. One dose of poison every 1.5 seconds on every enemy within 4 metres of you. Flasks heal 30% less.

**Works with.** One by One, which it comes after on the chart (from its Master mark), Venom, Catching (each bite jumps onward), Volatile and Slow Death (the doses never stop coming), Rupture, Tasted Every Herb.

**Example.** Stand in the middle of three soldiers and guard. Without landing a blow, each one has five doses within eight seconds.
