# Poison: detailed explanations

The long explanation the chart shows for each high-level mark of the Poison tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Poison): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Poisoned" is Poison's affliction: doses that lie up to
eight deep and bite for all their depth at once on a slow beat. "The beat" is that bite, every 2 seconds on each enemy;
a blow or a cast "on the beat" comes within a fifth of a second of it.

## poison.venom
**What it does.** Your weapon poisons. Every blow that lands adds a dose, a heavy blow one more, a finisher more again. The poison does not tick quietly: on a slow beat it bites for all its doses at once. And a tenth of the damage your poison deals comes back to you as health, so every Poison player is fed by the poison.

**When it happens.** Always on once learned. The doses come with blows only while Poison is your first element: your weapon lays only your first element's affliction, so with Poison second its doses come from your spells, the charged blade and the R2 strike instead. The healing comes whoever laid the poison.

**The numbers.** 2 Shards. One dose a blow; a heavy blow lays two, a finisher four. Up to eight doses, for 6 seconds, and each new dose starts the time again. Every 2 seconds the poison bites for 2 a dose: 16 at eight. Set off, it deals 6 a dose at once: 48 at eight. You heal 0.2 a dose at each bite (1.6 at eight) and 0.6 a dose when it is set off (4.8 at eight).

**Works with.** Rupture and Volatile to set it off, Catching to spread it, In Time on its beat, every Poison spell, Fever, and Rot Roots when a Seeded enemy is poisoned.

**Not built yet.** The heal as a tenth of what the poison really dealt: for now it is counted from the plain 2 a bite and 6 a burst, so stronger or ripened poison feeds no more, and it comes at each bite rather than spread over time.

**Example.** Four light blows and a heavy leave a soldier six doses deep; roll away and the next bite takes 12 from it and gives you a little over 1.

## poison.rupture
**What it does.** A heavy blow on an enemy poisoned to its full depth bursts the poison. All of it lands at once, on it and everyone close to it, and the doses are spent.

**When it happens.** A heavy blow or finisher that lands on an enemy carrying a full dose of poison.

**The numbers.** 3 Shards. Set off for 6 damage a dose, 48 at eight, with 4 stagger a dose, to everything within 1 metre of the enemy struck. At most about three times a second. Full is eight doses, twenty with Slow Death.

**Works with.** Venom (fill it with blows, end with a heavy), Blister and One by One, which grow from it, Volatile, Coiled Strike (four doses toward full), the root's healing (each burst feeds you).

**Example.** Eight doses on a soldier, then a heavy blow: 48 lands at once with the blow, and the soldier beside it takes the same.

## poison.blister
**What it does.** Blister, the detonator of the tree. You raise a blister on the enemy you face. It swells each time its poison bites, through three sizes, and a heavy blow breaks it in a burst that poisons everything round it. Left too long, it withers.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). With no enemy faced, nothing is raised and the slot waits all the same.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 9, and 50 damage). An enemy within 8 metres; one blister at a time; it waits 12 seconds. The burst is 3 metres in radius round the enemy: 40 damage, 30 stagger and four doses.

**Works with.** The root, Rupture, which it grows from (four doses toward full), Coiled Strike (step in and break it from behind), In Time, and the charged blade.

**Not built yet.** The swelling through three sizes, 20, 40 and 70 in two, three and four metres with 2, 4 and 6 doses: for now it bursts at the middle size. Broken by a heavy blow alone: for now any weapon blow on it breaks it.

**Example.** Blister the brute in the middle of a group, cut it once, and everyone round it is four doses deep.

## poison.blister.shape
**What it does.** Opens two shapes of Blister, chosen at any rest fire. **Weeping Sore**: it never bursts by itself or by a heavy blow. Each time its poison bites it weeps a bubbling puddle where the enemy stands, so a walking enemy lays your poison; your heavy blow bursts every puddle at once. Slow and insidious, not a swell and a pop. **The Vessel**, the court's elixir-drinkers: the whole enemy swells, and when its poison bites at full depth it bursts by itself into a cloud that doses everything in it. You set it and leave it.

**When it happens.** Whenever you cast Blister, as the shape it wears. Before this is bought, the plain blister is cast.

**The numbers.** 3 Shards. Weeping Sore: for 12 seconds, each bite of the enemy's poison leaves a puddle 1.5 metres across for 3 seconds that doses what stands in it each second; a heavy blow on the sore bursts 2 metres round it for 12 damage and two doses, at most twice a second. The Vessel: for 12 seconds, the first bite at full depth lays three doses on everything within 2 metres and leaves a cloud 4 metres across for 3 seconds that doses each second, once a cast.

**Works with.** Weeping Sore: an enemy that walks, Catching, Low Mist. The Vessel: Venom's blows to fill it, Catching, Slow Death (full is twenty, so it waits longer). Volatile stills the beat, and neither shape weeps or bursts without bites.

**Not built yet.** Both shapes still raise the plain blister beside them, which any weapon blow breaks: a shape cannot yet set aside its spell's own work. Weeping Sore's heavy blow bursts round the sore, not each puddle where it lies.

**Example.** Weeping Sore on a charging brute: it lays a trail of puddles as it comes, and the soldiers following it walk through them.

## poison.blister.master
**What it does.** Break it with a heavy blow as its poison bites, within a fifth of a second, and the burst doubles.

**When it happens.** The first heavy blow on the blistered enemy, with Blister in a slot.

**The numbers.** 3 Shards. For 12 seconds after the cast, the first heavy blow on that enemy, if it lands within a fifth of a second of its bite, strikes round it with a second burst as large: 40 damage, 30 stagger and four doses in 3 metres.

**Works with.** In Time (the same moment), Coiled Strike's heavy blow made critical, a heavy weapon.

**Not built yet.** The blister's own burst doubled: for now a second burst lands with it, and only if the heavy blow is the one that breaks it (a light blow breaks the plain blister first).

**Example.** Watch the bubbles on the blistered captain; as they burst, bring the heavy down: two bursts, 80 to all round it.

## poison.onebyone
**What it does.** One by One, Poison's larger spell, paid in mana. Raise a hand: a green line runs along the ground from poisoned enemy to poisoned enemy, then you snap your fingers and each one's poison bursts in turn, each passing its doses to the next before it goes. The last burst is the largest.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, 50 mana (its Empower: 45). Up to six enemies within 10 metres of you; half a second's warning, then a column under each in turn, every 0.07 seconds, 1 metre round. The column itself does no damage; each poisoned enemy it strikes has its poison set off: 6 damage and 4 stagger a dose to everything within 1 metre, 48 and 32 at eight. What a column strikes is thrown up for 0.8 seconds.

**Works with.** Rupture, which it grows from, Venom, Blister, Catching and Miasma to lay the poison first, the root's healing (every burst feeds you), and the charged blade.

**Not built yet.** The line from poisoned enemy to poisoned enemy alone: for now the columns come up under the six nearest, poisoned or not. Each passing its doses to the next, the last the largest: for now each bursts with what it carries. The Empower's 7 a dose.

**Example.** Dose a pack with Miasma and a rush of Share Alike, then snap: six bursts in a breath, each one feeding you.

## poison.onebyone.shape
**What it does.** Opens two shapes of One by One, chosen at any rest fire. **Eaten in Turn**, the gu jar, where the order of killing matters: the line is drawn but nothing bursts. For a while, when an enemy on it dies, all its doses pass to the next on the line, doubled; one pushed past eight bursts at once and passes on. The blade chooses the order. **All Into One**, the Metal and Poison reaction's quicksilver: no line. The poison runs out of every poisoned enemy near as beads of mercury that join in the enemy you face, its doses added to its own with no limit. Then it alone bursts. For the one that matters.

**When it happens.** Whenever you cast One by One, as the shape it wears. Before this is bought, the plain line is cast.

**The numbers.** 3 Shards. Eaten in Turn: for 8 seconds, each enemy the spell struck that dies poisoned passes twice its doses to the nearest other enemy within 10 metres. All Into One: one column only, under the nearest enemy.

**Works with.** Eaten in Turn: a pack of weak enemies to kill in order, Slow Death (twenty deep to pass on), Rupture and Volatile on the one fed. All Into One: a god or an elite, Catching and Miasma to dose the crowd first.

**Not built yet.** Eaten in Turn still bursts as the plain spell does, so what dies must be dosed again first; the doses go to the nearest, not the next on the line; one pushed past eight does not burst. All Into One gathers nothing yet: the one enemy bursts with what it carries.

**Example.** Eaten in Turn on five soldiers: re-dose them as you cut, kill the front one six deep, and the next is twelve.

## poison.onebyone.master
**What it does.** Snap your fingers within a fifth of a second of the poison's beat, and every burst is one and a half times as strong.

**When it happens.** Each poisoned enemy a column strikes, with One by One in a slot.

**The numbers.** 3 Shards. For now each poisoned enemy struck within a fifth of a second of its own bite takes a second burst: 24 damage and 16 stagger within 1 metre, half again an eight-deep burst.

**Works with.** In Time (the same beat), Catching (the beat runs across the pack), Venom.

**Not built yet.** The snap itself timed against the beat: for now each enemy is tested as its column comes. The bursts raised by half: for now the same extra for any depth.

**Example.** Watch the pack's poison bite, raise the hand on it, and the columns that land in the bite strike half again.

## poison.volatile
**What it does.** Your poison no longer bites on its beat. It builds until it is full, then bursts by itself, spends itself and begins again.

**When it happens.** Always on once learned. It bursts the moment any affliction fills on a poisoned enemy: usually the poison reaching full depth, but another element's filling bursts it too, at whatever depth it has.

**The numbers.** 6 Shards. The burst deals 7 a dose, 56 at eight, with 4 stagger a dose, to everything within 2.5 metres of the enemy. At most twice a second. Full is eight doses, twenty with Slow Death (140 then).

**Works with.** Venom, Rupture, which it grows from, Slow Death, One by One, the root's healing (each burst feeds you). Catching, In Time, Weeping Sore, The Vessel and the beat Masters wait on bites, and there are none.

**Example.** Eight light blows on a soldier and it goes up on the eighth, taking the two beside it with it.

## poison.intime
**What it does.** Strike as the poison bites and your blow doses twice. For a moment after that timed blow, every blow you land is critical.

**When it happens.** A weapon blow that lands on a poisoned enemy within a quarter of a second of its poison biting, just before or just after. The extra dose comes only while Poison is your first element; the critical moment comes either way.

**The numbers.** 3 Shards. One dose more on the timed blow. For 1.2 seconds after it, your blows are critical (one and a half times damage). As built the timed blow itself is not made critical, only the ones after it.

**Works with.** Venom, Five Needles and Coiled Strike, which grow from it, every beat Master (the same moment), Blister's Master, a heavy blow or finisher in the critical moment.

**Example.** Watch the bubbles: as they burst, land a light blow, then a heavy in the next breath, and the heavy lands critical.

## poison.needles
**What it does.** Five Needles, the breaker of the tree. Five long needles, venom beading on them, are thrown in a narrow fan at the enemy you face. An enemy struck by all five is pinned at its points: held a moment, and its balance badly shaken.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 8). A fan 8 metres long and 12 degrees wide: 15 damage, 50 stagger and five doses, the five needles' worth, to everything in it. The first enemy it strikes is pinned: held 0.8 seconds (0.32 for a god or an elite) and 40 stagger more.

**Works with.** In Time, which it grows from, Rupture (five doses toward full), Coiled Strike (strike through the pinned one), and the charged blade.

**Not built yet.** Five needles thrown, each its own strike: for now the fan strikes as one, and the pin falls on the first enemy struck, not the one struck by all five. The Empower's 14 stagger a needle and 50 for the pin.

**Example.** A shield-bearer turns to face you: the needles pin it, and you have most of a second to walk round its shield.

## poison.needles.shape
**What it does.** Opens two shapes of Five Needles, chosen at any rest fire. **Spread**: a wide fan, so each needle finds a different enemy; each enemy pierced takes two doses, and its poison bites at once. **Red Paper**, the five poisons pinned to red paper (the needle grids in the floor are the Poison god's own weapon): all five needles pin one sheet of red paper to the enemy you face. For six seconds it cannot guard or turn a blow, and your blows on it pass any shield. The breaker for what hides behind a guard.

**When it happens.** Whenever you cast Five Needles, as the shape it wears. Before this is bought, the plain fan is cast.

**The numbers.** 3 Shards. Spread: a fan of 60 degrees, 3 damage, 10 stagger and two doses to each enemy in it, and each one's poison bites at once. Red Paper: the first enemy struck cannot raise its guard for 6 seconds.

**Works with.** Spread: a crowd, Catching (every bite throws a dose on), In Time. Red Paper: guarding enemies and captains, a heavy weapon, Blister.

**Not built yet.** Spread is one fan, not five needles, and still pins the first enemy it strikes. Red Paper's blow that cannot be turned and the blows that pass a shield.

**Example.** Red Paper on a captain who guards everything: for six seconds your chain lands in full.

## poison.needles.master
**What it does.** Throw them on a poisoned enemy's beat, within a fifth of a second of its bite, and every needle that strikes it makes its poison bite once more.

**When it happens.** Each poisoned enemy the needles strike, with Five Needles in a slot.

**The numbers.** 3 Shards. For now an enemy struck within a fifth of a second of its bite bites once more at once, for all its depth: 16 at eight.

**Works with.** In Time (the same beat), Catching (every bite throws a dose on), the root's healing (every bite feeds you), Spread.

**Not built yet.** A bite for every needle: for now one for the fan.

**Example.** A soldier eight deep: throw on its bite and it is bitten twice in a breath, 32.

## poison.coiled
**What it does.** Coiled Strike, Poison's counter. You coil, then strike through the enemy you face in an instant and come out on its far side, facing back, cutting everything on the line and poisoning it deeply. If the cut lands, your next heavy blow soon after is critical. Nothing can strike you in the step; a wall, or something too big to shove, stops you short.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 9, and 30 damage). It goes up to 6 metres, ending 1.8 metres past the enemy faced (with none, 4.5 metres ahead). The cut is 1.1 metres to each side of the line: 24 damage, 25 stagger and four doses, the strength of two fangs. A heavy blow within 3 seconds is critical.

**Works with.** In Time, which it grows from, Slow Death, which grows from it, Rupture (four doses toward full), Blister, and the charged blade: if it lands it charges your weapon with Poison.

**Not built yet.** The coil held up to 2.4 seconds while three fangs light (14, 24 or 38 damage and 2, 4 or 8 doses by the fangs): for now it strikes at once as two fangs. Let go as a blow is about to land, passing through you as three fangs.

**Example.** Step through a shield-bearer to its back, turn, and the heavy blow you owe it lands critical.

## poison.coiled.shape
**What it does.** Opens two shapes of Coiled Strike, chosen at any rest fire. **Centipede's Run**, the centipede, one of the five poisons: let go and you strike through up to three enemies in a zigzag of short rushes, a bite on each, the fangs' doses shared between them; you end behind the last. The snake strikes one, the centipede many. **Shed Skin**, the snake that sheds its skin: let go inside the perfect window and you leave your skin standing where you were, pale and papery. The enemy goes on striking it while your strike lands from behind.

**When it happens.** Whenever you cast Coiled Strike, as the shape it wears. Before this is bought, the plain strike is cast.

**The numbers.** 3 Shards. Centipede's Run: a straight rush 12 metres through everything on a line 2 metres wide, 14 damage, 15 stagger and two doses to each (30 damage each once the Empower is bought, which sets its number on the rush too). Shed Skin: every enemy within 3 metres of where you stood loses sight of you for 1.5 seconds.

**Works with.** Centipede's Run: a row of enemies, Catching, Rupture. Shed Skin: a pack that crowds you, Blister (break it from behind), a heavy weapon.

**Not built yet.** Centipede's Run's zigzag of three short rushes, ending behind the last: for now it is one straight run. Shed Skin only inside the perfect window, and a skin drawn standing: for now every cast blinds those near where you stood.

**Example.** Centipede's Run down a line of archers: each takes two doses and you stand past the last.

## poison.coiled.master
**What it does.** Let go inside the perfect window, and Coiled Strike is ready again.

**When it happens.** With Coiled Strike in a slot.

**The numbers.** 3 Shards. For now a perfect dodge ends Coiled Strike's wait, once in 10 seconds.

**Works with.** Reading the enemies' wind-ups, Shed Skin, In Time.

**Not built yet.** The let-go inside the perfect window as the moment: for now any perfect dodge does it.

**Example.** Dodge a captain's swing perfectly and Coiled Strike is ready to put you at its back.

## poison.slowdeath
**What it does.** Your weapon wounds far less. Your poison goes far deeper, and it never fades while the one carrying it lives.

**When it happens.** Always on once learned. The greater depth holds while Poison is your first element; the poison stops fading either way.

**The numbers.** 6 Shards. Weapon damage 30% less. Poison holds twelve more doses, up to twenty: at twenty a bite deals 40 every 2 seconds, and set off it deals 120. Poison you lay does not run out.

**Works with.** Venom and its healing (a deep bite feeds you more), Coiled Strike, which it grows from, Catching, Volatile (it fills at twenty), Rupture (full is twenty now), One by One's Eaten in Turn, Clean Blade.

**Example.** Dose a Gate Captain twenty deep and spend the rest of the fight guarding: 40 every two seconds, and it never wears off.

## poison.catching
**What it does.** Each time poison bites, one dose leaps from that enemy to the nearest other enemy within reach.

**When it happens.** Every bite of poison on any enemy, however it was laid.

**The numbers.** 3 Shards. One dose to the nearest enemy within 4 metres.

**Works with.** Venom, Miasma and the lift The Delayed Touch, which grow from it, Five Needles' Spread and Master (more bites, more leaps), Share Alike, The Vessel.

**Example.** Poison one soldier deep and keep it near its friends: every two seconds one of them gains a dose without a blow from you.

## poison.base
**What it does.** Miasma, Poison's own spell, comes with the element and is never bought. A low cloud rolls out ahead of you, hangs, creeps after the nearest enemy and doses whatever stands in it once a second; bubbles rise through it and it leaves a stain. You stand in it unharmed.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Poison, it is always yours to put in a slot.

**The numbers.** Given with Poison. No mana: it waits 7 seconds after a cast, less with weapon play; its Empower makes that 6. The cloud lies 4 metres ahead, 2.5 metres across, for 4 seconds, and doses what stands in it once a second.

**Works with.** Its Empower, its two shapes and its Master; the root, Catching, which it grows from, Rupture and One by One (doses for them to burst), and the charged blade, which it charges with Poison.

**Not built yet.** The Empower's cloud 3 metres across: the base spell is cast by Poison's kit, which takes only the Empower's shorter wait.

**Example.** Cloud the doorway a pack must come through, then guard: each one through it comes out dosed.

## poison.base.shape
**What it does.** Opens two shapes of Miasma, chosen at any rest fire. **Low Mist**, the land's hours of the miasma, mist that pools low and drives people up: the cloud is heavier than air and pours out from your feet along the floor the way you face, a low sheet with a hard top edge, stopping at walls. Enemies in it are dosed once a second, and a dodge or roll in it costs them a dose. A lane where the plain cloud is a disc. **Censer**, Swinging Censer folded in: the cloud is carried. A censer swings round you on its chain, and each pass deals damage and a dose and leaves smoke hanging. The fight comes with you.

**When it happens.** Whenever you cast Miasma, as the shape it wears. Before this is bought, the plain cloud is cast.

**The numbers.** 3 Shards. Low Mist: a row of mist 8 metres long and 2.5 metres wide from your feet, lying 5 seconds, a dose a second to what stands in it. Censer: one censer 3 metres out, for 8 seconds, going round once every 1.2 seconds; it strikes whatever is within 1 metre of it for 4 damage, 4 stagger and a dose.

**Works with.** Low Mist: a corridor, Weeping Sore, Tasted Every Herb. Censer: a pack that crowds you, Catching, a guard held while it swings.

**Not built yet.** Low Mist still casts the plain cloud beside the sheet, and does not stop at walls or take a dose from an enemy's dodge. Censer's smoke hanging 2 seconds after each pass.

**Example.** Censer, and stand your ground in a doorway: whatever crowds you is dosed every swing as you guard.

## poison.base.master
**What it does.** Cast it on the poison's beat, within a fifth of a second of a bite on any enemy near, and the cloud doses twice a second.

**When it happens.** Each enemy the cloud doses, with Miasma in a slot.

**The numbers.** 3 Shards. For now an enemy the cloud doses within a fifth of a second of its own bite is dosed once more each second for 4 seconds.

**Works with.** In Time (the same beat), Catching, every other beat Master.

**Not built yet.** The cast itself timed against a bite on any enemy within 8 metres: for now each enemy is tested as the cloud doses it.

**Example.** Cast as a poisoned soldier bites, and the cloud lays two doses a second on it while it stands there.

## poison.lift
**What it does.** The Delayed Touch, Poison's reaction lift, from the legend of the delayed death touch. Every reaction poison takes part in is held in the body: a thin ring of mist closes on it, and the reaction lands later at double strength, dosing all it reaches. A heavy blow on that enemy brings it on at once. Poison is paid later, and the heavy chooses the moment.

**When it happens.** Whenever a reaction with Poisoned in it happens, on any enemy. Two and a half seconds later, or at your first heavy blow on that enemy before then.

**The numbers.** 3 Shards. A burst 2.5 metres round the enemy: 20 damage, 20 stagger and two doses to all it reaches.

**Works with.** Every reaction with Poisoned in it (Fire Medicine, Nerve Fire, Quicksilver, Venom Rime, Rot Roots and the rest), Catching, which it grows from, Share Alike and Tasted Every Herb, which grow from it, a heavy weapon.

**Not built yet.** The reaction itself held and landed again at double strength: for now a burst of its own lands when the hold ends. Burning Sap, which already waits, taking only the double strength. The closing ring of mist.

**Example.** Quicksilver goes off on a soldier: two and a half seconds later, or as your heavy lands, it bursts again and doses the two beside it.

## poison.sharealike
**What it does.** Share Alike, the movement spell. You rush straight through everything before you, untouchable, and come out the far side facing back. Every enemy you pass is struck and poisoned, and then every one of them is raised to the depth of the sickest.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 8, and nine metres). Up to 7 metres along a lane 2 metres wide; a wall or something too big to shove stops you. Passing: 10 damage, 10 stagger and two doses.

**Works with.** The Delayed Touch, which it grows from, Catching, One by One (a line of poisoned enemies), Rupture, and the charged blade: if it lands it charges your weapon with Poison.

**Not built yet.** Every one passed raised to the depth of the sickest: for now each gets its two doses.

**Example.** Rush through a line of three archers: each is dosed and you stand behind them.

## poison.sharealike.shape
**What it does.** Opens two shapes of Share Alike, chosen at any rest fire. **Carried on the Blade**: you rush through as learned, but take the sickest one's depth onto your blade, and your next five weapon blows each set what they strike to that depth. Venom drips heavy from the edge, five beads to count down. The rush loads the weapon. **Shed Tail**, the gecko: you rush backward, away, and a thrashing tail is left where you stood, lashing everything near it. Away from them, and what follows you pays.

**When it happens.** Whenever you cast Share Alike, as the shape it wears. Before this is bought, the plain rush is cast.

**The numbers.** 3 Shards. Carried on the Blade: for 5 seconds after the rush, every blow lays two doses more. Shed Tail: where you stood, for 2 seconds, everything within 2 metres takes three doses every half second.

**Works with.** Carried on the Blade: a fast weapon, Rupture, In Time. Shed Tail: enemies that chase you, Catching, Low Mist.

**Not built yet.** Carried on the Blade carrying the sickest one's depth for five blows: for now two doses more a blow for 5 seconds. Shed Tail's rush backward: for now you rush forward as learned, and the tail is left where you began.

**Example.** Carried on the Blade through a pack, then turn on the captain: every blow of your chain lays three.

## poison.sharealike.master
**What it does.** Rush on the poison's beat, within a fifth of a second of a bite on any enemy you pass, and the doses double.

**When it happens.** Each enemy the rush passes, with Share Alike in a slot.

**The numbers.** 3 Shards. For now each enemy passed within a fifth of a second of its own bite takes two doses more.

**Works with.** In Time and every beat Master, Catching (the beat runs across the pack), One by One after.

**Not built yet.** A bite on any enemy passed as the moment: for now each enemy is tested as it is passed.

**Example.** Rush a poisoned pack as their poison bites, and each comes out four doses deeper.

## poison.devotion
**What it does.** Tasted Every Herb, Poison's Devotion: all in on Poison only. No poison, cloud or mist harms you, yours or the enemy's, the court's miasma among them. Standing in any cloud heals you, and your blows dose twice while you stand in one. Where everyone else leaves the mist, you fight in it. Shennong, who tasted every poison; realgar wine, poison against poison.

**When it happens.** Only while Poison is carried twice (all in).

**The numbers.** 6 Shards. You heal 2 a second while an enemy is near, and every lay of doses lays one more. All in, L2 with R2 is the deep strike, Nine Times Refined.

**Works with.** Miasma and its Low Mist, Venom, The Delayed Touch, which it grows from, Fever, Clean Blade.

**Not built yet.** No poison, cloud or mist harming you: for now the enemy's clouds harm you as before. The healing and the second dose only while you stand in a cloud: for now they hold in every fight.

**Example.** A god fills the floor with its own mist: you stay in it, mending, and every blow you land there doses twice.

## poison.cleanblade
**What it does.** While your surge lasts the poison waits: it neither bites nor fades, and every bite it would have bitten is owed. Your blows dose once more. When the surge ends time slows, you flick the blade clean, and every bite owed by every poisoned enemy near lands at once.

**When it happens.** Light the surge (a tap of L2, or F); it begins with whichever surge you wear. The flick comes when the surge ends. The extra dose holds while Poison is your first element.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. One more dose a blow, on top of Fever's. When the surge ends, every enemy within 14 metres takes one bite for each whole 2 seconds it waited, at its depth then, with 20 stagger. Fever's 10 seconds owe five bites: 80 on an enemy at eight doses.

**Works with.** Venom, Fever, Slow Death (a deeper bite), Share Alike and Miasma to dose a pack while it waits. In Time, Catching and the beat Masters wait on bites, and there are none while the poison waits.

**Example.** Light Fever, dose a whole pack to eight while it waits, and let the surge run out: each takes 80 at once.
