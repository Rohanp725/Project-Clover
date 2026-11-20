# Water: detailed explanations

The long explanation the chart shows for each high-level mark of the Water tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Water): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## water.soak
**What it does.** Every blow of your weapon that lands leaves the enemy Soaked. A Soaked enemy takes more from your spells, and Static or Chill laid on it is laid on everyone standing near it as well.

**When it happens.** Always on once learned: every weapon blow that lands, while Water is your first element (or all in). With Water second, Soaked comes from your spells, the charged blade and the R2 strike instead.

**The numbers.** 2 Shards. Soaked is on or off and lasts 8 seconds; each blow starts it again. Spells deal a Soaked enemy 25% more damage. Static or Chill laid on it is also laid on every other enemy within 4 metres, whether they are Soaked or not.

**Works with.** Riptide, Wellspring, Heavy Water, Deep Crack (which spends it), Drawn From the Drowned, Spreading Water, Lightning's Static and Ice's Chill as your second element, and the reactions Steam (with Fire) and The Ground Gives (with Earth).

**Example.** Carrying Water and Lightning, cut three soldiers in a huddle, then Thunderclap one: the Static runs to all three.

## water.ebbflood
**What it does.** A perfect guard brings the Flood: a splash soaks the attacker and those beside it, and for a while your blows strike harder. A heavy blow that lands brings the Ebb: for a while your guard and dodge cost half the stamina and the moment of your perfect guard is longer.

**When it happens.** The Flood with every perfect guard (press the guard button just before a blow lands). The Ebb with every heavy blow or finisher that lands.

**The numbers.** 3 Shards. Flood: a splash 2.5 metres round the attacker, 6 damage, 8 stagger and Soaked; then 15% more weapon damage for 5 seconds. Ebb: dodges and blocked blows cost half the stamina for 4 seconds, and the perfect guard's moment is 22% longer for as long.

**Works with.** Blowhole and Deep Crack, which grow from it; Spreading Water above them; Heavy Water; Undertow.

**Not built yet.** The Flood's longer reach and a splash on each blow: for now it is more damage and one splash on the attacker.

**Example.** Land a heavy blow, then guard the answer perfectly with the Ebb's wider moment: the Flood soaks it and your next cuts land 15% harder.

## water.crack
**What it does.** A fist of water snaps shut on the enemy you face. A bubble as big as a body forms on it and collapses with a crack. On a Soaked enemy the water boils out of it: its Soaked is spent, and every other Soaked enemy near cracks with it. The detonator of the tree.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). It closes on the nearest enemy within reach.

**The numbers.** 4 Shards, no mana: it waits 11 seconds after a cast, less with weapon play (its Empower: 9, and 22 damage). An enemy within 7 metres; the bubble collapses 0.4 seconds after the cast, 1 metre round: 18 damage and 40 stagger. On a Soaked enemy its Soaked is spent and the crack runs on to up to six other Soaked enemies, each within 5 metres of the last: 18 damage and 30 stagger each.

**Works with.** Soaked, Riptide and Whirlpool to soak a crowd first, Ebb and Flood, which it grows from, Spreading Water.

**Not built yet.** Each of the others cracking where it stands and having its own Soaked spent: for now the crack runs to them as an arc of water, and they keep their Soaked.

**Example.** Riptide three soldiers to your feet, Soaked, and snap the bubble on the middle one: all three take the crack.

## water.crack.shape
**What it does.** Opens two shapes of Deep Crack, chosen at any rest fire. **Ink**: the bubble is black water from the deep and bursts into a pool of ink; enemies in it lose track of you beyond three metres and stay Soaked, and you stay in plain sight. **String of Bubbles**: three bubbles in a row along the line you face, each cracking a moment after the last; aim it down a corridor.

**When it happens.** Whenever you cast Deep Crack, as the shape it wears. Before this is bought, the single bubble is cast.

**The numbers.** 3 Shards. Ink: a pool 1.5 metres round for 5 seconds, biting once a second for 1 damage and Soaked; what it reaches as it bursts sees no further than 3 metres for 5 seconds. String of Bubbles: a line of water 7 metres long, 2 metres wide, run out at 13 metres a second: 18 damage and 40 stagger on all it crosses, and the crack runs on from each Soaked enemy as above.

**Works with.** Blowhole and Whirlpool to hold them in the ink; Riptide's Breaking Swell to line them up for the string.

**Not built yet.** Ink's blindness only while an enemy stands in the ink (for now what the ink first reaches keeps it for 5 seconds), and String of Bubbles as three bubbles 2 metres apart (for now one running line).

**Example.** Ink in a doorway: the archers behind it lose you, and the soldiers in it stay Soaked for the next crack.

## water.crack.master
**What it does.** Snap it as your heavy blow lands on the same enemy, and every Soaked enemy in the fight cracks.

**When it happens.** For now, a heavy blow (a finisher counts) on the enemy within 0.3 seconds after the bubble cracks on it. Deep Crack must be in a slot.

**The numbers.** 3 Shards. The crack runs on to up to twelve Soaked enemies, each within 20 metres of the last: 18 damage and 30 stagger each.

**Works with.** Soaked and every mark that soaks a crowd; Heavy Water; Spreading Water.

**Not built yet.** The snap timed to the heavy blow's landing rather than the crack's, and each enemy cracking where it stands.

**Example.** Soak the room with Riptide and Whirlpool, then cast on the brute in front of you and bring the heavy down as it cracks: every soaked body in the room cracks with it.

## water.blowhole
**What it does.** You stand on a vent in black rock. Each blow that comes is turned and drives the sea down the vent, and its spray rises a step. Let go, and the sea comes up as a spout under the attacker, harder for every step, and throws it up open to your blade.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4), then any blow in the next 3 seconds. If none comes, the cast is spent.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 8, and 18 a step). Ready for 3 seconds. As built the first blow is turned and answered at once as a spout of three steps: a cone 3 metres long and 140 degrees wide, 42 damage (54 with the Empower), 66 stagger and Soaked. What it strikes begins nothing and cannot guard for 0.8 seconds.

**Works with.** Ebb and Flood (the turned blow is a guard's work), Deep Crack on the Soaked attacker, Heavy Water, Undertow.

**Not built yet.** The vent itself: standing 3 seconds and turning every blow, red ones too (15 stamina each), each raising the spray a step to five, and the spout loosed when you let go, 14 a step, throwing up for 0.8 seconds at three steps and 1.2 at five. For now the first blow is turned and answered as three steps, and what it strikes is left open rather than thrown up.

**Example.** Cast it as a brute winds up: the blow is turned, the spout answers, and the brute stands open for your next cut.

## water.blowhole.shape
**What it does.** Opens two shapes of Blowhole, chosen at any rest fire. **Still Water**: the first blow only, turned and answered at once with a thrust of water six metres straight through the attacker; a quick parry, not a store. **Blown Out**: the spout comes up under you instead and throws everything within four metres out three metres; room, not a target.

**When it happens.** Whenever you cast Blowhole, as the shape it wears. Before this is bought, the vent is cast.

**The numbers.** 3 Shards. Still Water: the answer is a thrust 6 metres long and 25 degrees wide, with Blowhole's damage and stagger. Blown Out: the answer is a ring 4 metres round you with Blowhole's damage and stagger, and everything within 4 metres is thrown 3 metres out.

**Works with.** Deep Crack's String of Bubbles down the line Still Water opens; Undertow to bring back what Blown Out threw.

**Not built yet.** Blown Out's store before the spout (as for the Learn): for now the first blow turned brings it up at once.

**Example.** Surrounded, wear Blown Out: the first blow turned throws the whole ring of them back.

## water.blowhole.master
**What it does.** Let go in the same instant as a blow lands on the vent, and the spout is doubled and the vent stays for a second round.

**When it happens.** For now, every answer, since as built the answer comes in the instant the blow lands. Blowhole must be in a slot.

**The numbers.** 3 Shards. The answer strikes again where it lands: 42 damage, 66 stagger and Soaked within 1.5 metres. And Blowhole's cooldown ends at once, once in 20 seconds, so it can be cast again for a second round.

**Works with.** Ebb and Flood; Deep Crack; Heavy Water on the attacker it soaks.

**Not built yet.** The timing (letting go within 0.2 seconds of a blow landing on the vent): for now every answer is doubled.

**Example.** Turn a soldier's blow, and cast again at once as the next one swings.

## water.lift
**What it does.** Spreading Water, Water's reaction lift: every reaction with Soaked in it runs on at half strength to every other Soaked enemy near.

**When it happens.** Whenever Soaked reacts on an enemy with the other affliction it carries (Steam with Burn, The Ground Gives with Tremor, and the rest).

**The numbers.** 3 Shards. For now an arc of water runs from where the reaction happened to up to four other Soaked enemies, each within 4 metres of the last: 10 damage and 15 stagger each.

**Works with.** Carrying a second element: every reaction of Water's ten; Soaked, to soak a crowd; Ebb and Flood, which it grows from; The Sea Comes In above it.

**Not built yet.** The reaction itself running on at half strength: for now a plain arc of water.

**Example.** Water and Fire on a huddle of soaked soldiers: Steam goes off on one, and the water carries on to the rest.

## water.devotion
**What it does.** The Sea Comes In, Water's Devotion: all in on Water only. Every spell leaves standing water where it lands. Enemies in water are always Soaked and slower, and in water your dodge slides further and your cooldowns run faster. Draw the fight onto your own water and stay there, as the tide takes the beach.

**When it happens.** Only while Water is carried twice (all in). A pool is left each time a spell strikes.

**The numbers.** 6 Shards. A pool 4 metres across where a spell strikes, at most one every 2.5 seconds, for 10 seconds, biting once a second for 1 damage and Soaked. For now everywhere, not only in water: the dodge goes half again as far, and every spell clock runs 30% faster. All in, L2 with R2 is the deep strike, Thrown on the Reef: the lash wraps the enemy and a wave throws it 5 metres away from you.

**Works with.** Every spell of the tree; Deep Crack and Spreading Water on the soaked crowd; Undertow to draw them onto the water.

**Not built yet.** Four pools at most, enemies in them 15% slower, and the dodge and the clocks quicker only while you stand in your own water. Thrown on the Reef's 30 damage, 50 stagger and 0.8 seconds lying down when the enemy meets a wall, a rock or another enemy.

**Example.** Riptide and Whirlpool lay two pools in the middle of the room; stand between them and cast again and again, a third faster.

## water.undertow
**What it does.** A heavy blow that lands drags every enemy near you a step toward you.

**When it happens.** Every heavy blow or finisher that lands, at most twice a second. As built it pulls everyone near, Soaked or not.

**The numbers.** 3 Shards. Every enemy within 6 metres of you is pulled 1.5 metres in. At most once every half second.

**Works with.** Heavy weapons that strike wide, The Tide Turns, which grows from it, Heavy Water, Whirlpool, Blowhole's Blown Out (which throws them back out).

**Example.** Two archers hang back while you fight a brute: each heavy blow on the brute draws them a step closer to your blade.

## water.tide
**What it does.** The tide in your hand. Press, and the ebb draws you straight back out of reach, leaving a line of wet sand. Press again within three seconds, and the flood brings you back along the line and on past where you began as a breaking wave, soaking all on the line and carrying it ahead of you.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 8 seconds after a cast, less with weapon play (its Empower: 6, and the flood runs 10 metres). As built one press is the flood alone: you drive 8 metres ahead at 20 metres a second, nothing can touch you as you go, and everything on the line, 2 metres wide, takes 10 damage, 10 stagger and Soaked and is carried 2 metres on ahead of you.

**Works with.** Undertow, which it grows from; Riptide to bring them in before the flood; Deep Crack on the line it soaks; Heavy Water.

**Not built yet.** The ebb (drawn 5 metres straight back in 0.3 seconds, untouchable, the wet sand left behind) and the flood as the second press back along that line: for now one press is the flood, run ahead.

**Example.** A pack closes on you: flood through it and out the far side, every one of them Soaked and stumbling.

## water.tide.shape
**What it does.** Opens two shapes of The Tide Turns, chosen at any rest fire. **King Tide**: the flood comes in four metres wide and throws what it carries down for a moment, but it stops where you began; the shape for a crowd. **Out to Sea**: there is no flood; the ebb takes the enemy faced with you, drawn the five metres out, Soaked, and dropped at your feet, away from its friends. A pull, not a return.

**When it happens.** Whenever you cast The Tide Turns, as the shape it wears. Before this is bought, the plain tide is cast.

**The numbers.** 3 Shards. King Tide: the flood 4 metres wide and 5 metres long; what it strikes lies down for 0.6 seconds (elites half as long, gods not at all). Out to Sea: the enemy faced is drawn 5 metres toward you and Soaked.

**Works with.** Deep Crack and Heavy Water on the crowd King Tide throws down; Blowhole and the blade on the one Out to Sea brings.

**Not built yet.** King Tide keeping its own 5 metres when the Empower is bought (for now the Empower's 10 metres win), and Out to Sea drawing you back with the enemy.

**Example.** An archer behind three shields: Out to Sea brings it to your feet, alone.

## water.tide.master
**What it does.** Started in the instant a blow would land on you, The Tide Turns counts as a perfect dodge, and a flood pressed in the instant a blow would land starts no cooldown.

**When it happens.** For now, every cast of The Tide Turns, and a cast within a second after a perfect dodge. The Tide Turns must be in a slot.

**The numbers.** 3 Shards. What a perfect dodge gives: 12 mana and 1.5 seconds off every running cooldown. A cast within 1 second after a perfect dodge, when the spell is ready, starts no clock.

**Works with.** The dodge; Ebb and Flood; every spell whose clock the cut shortens.

**Not built yet.** The timing (started as a blow would land), and the flood's own press: for now every cast gives the dodge's mana and cuts, and a perfect dodge makes the next cast free of its clock.

**Example.** Dodge a brute's swing perfectly and flood through it at once: the spell is ready again before it turns round.

## water.heavy
**What it does.** Striking a Soaked enemy makes your weapon break balance faster for a few seconds.

**When it happens.** Each time a weapon blow or a spell of yours strikes an enemy that is already Soaked. Each new strike starts the time again.

**The numbers.** 3 Shards. Your weapon's blows deal 20% more stagger for 3 seconds. It can be set going again at most twice a second.

**Works with.** Soaked, Whirlpool, which grows from it, Ebb and Flood, Blowhole, heavy blows and finishers, Riptide to soak first.

**Not built yet.** The design's form, Soaked enemies slow to recover after they strike: an enemy's recovery cannot be changed yet, so the weapon breaks their balance sooner instead.

**Example.** Soak a brute, then work it with light blows: its balance bar fills a fifth faster and the finisher comes sooner.

## water.whirlpool
**What it does.** You open a whirlpool under the enemy you face. Enemies in it are kept Soaked and bitten, and your blows on them break balance harder: the breaker of the tree.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). With no enemy faced it opens 6 metres ahead of you.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 8, and it lasts 7 seconds). On an enemy faced within 6 metres. The whirlpool is 4 metres in radius and lasts 5 seconds. It bites every half second for 3 damage, with no stagger, and lays Soaked with each bite. Each bite leaves its enemy taking 30% more stagger for 0.7 seconds, so while it stands in the water every blow breaks it harder.

**Works with.** Soaked, Heavy Water, which it grows from, Undertow, Deep Crack, a second element's Static or Chill, Steam if you carry Fire.

**Not built yet.** Enemies drawn to the middle at 1.2 metres a second, and their balance not recovering inside it.

**Example.** Open it under a pack: for five seconds every one of them stays Soaked and breaks a third sooner under your blade.

## water.whirlpool.shape
**What it does.** Opens two shapes of Whirlpool, chosen at any rest fire. **Maelstrom**: smaller and pulling hard, and at its end it throws everything inside up in a spout and down again. **Rip Current**: no pull to the middle; a lane of churning foam runs from the whirlpool's point away from you, and enemies in it are carried down it and cannot dodge against it. Cut them as the current drags them past.

**When it happens.** Whenever you cast Whirlpool, as the shape it wears. Before this is bought, the plain whirlpool is cast.

**The numbers.** 3 Shards. Maelstrom: 2.5 metres in radius; when the bites on an enemy stop (the whirlpool ends, or it leaves the water), it is thrown up for 0.8 seconds and takes 50 stagger. Rip Current: each bite carries its enemy 1.25 metres away from you, two and a half metres a second, and the bites keep their churn.

**Works with.** Deep Crack on the bodies Maelstrom throws up; Undertow and your blade on the ones Rip Current carries past.

**Not built yet.** Maelstrom's hard pull (3 metres a second), and Rip Current's lane of foam 2 metres wide and 9 long for 6 seconds and its rule against dodging: for now the whirlpool itself carries what it bites away from you.

**Example.** Maelstrom on a pair of soldiers: they churn for five seconds, then go up together for your heavy blow.

## water.whirlpool.master
**What it does.** Open it under an enemy in the instant its attack mark shows, and the attack is lost as it goes in to the knees. Its cooldown is halved.

**When it happens.** For now, the first enemy it bites on each cast. Whirlpool must be in a slot.

**The numbers.** 3 Shards. The first enemy bitten flinches out of what it had begun, once in 5 seconds.

**Works with.** Blowhole and Ebb and Flood, for the moment an attack begins; Heavy Water.

**Not built yet.** The timing (the instant its attack mark shows) and the halved cooldown: for now its first bite breaks whatever its enemy had begun.

**Example.** A brute raises its club: open the whirlpool under it and the swing never comes.

## water.gourd
**What it does.** Holding the flask button throws the flask instead of drinking it. Where it breaks it strikes and soaks everyone near and leaves a pool that keeps what stands in it Soaked. The throw also mends you a little at once. A short press still drinks.

**When it happens.** Hold the flask button. Each throw uses a flask.

**The numbers.** 3 Shards. Hold 0.3 seconds to throw. It flies at the enemy ahead within 9 metres, or 5 metres ahead. The burst is 2.6 metres in radius: 18 damage, 20 stagger and Soaked. The pool is 1.5 metres in radius and lasts 6 seconds, biting once a second for 1 damage and Soaked. The throw mends you 20.

**Works with.** Drawn From the Drowned, which grows from it, to win the flask back; Deep Well; Heavy Water; Deep Crack on the soaked crowd; Steam if you carry Fire.

**Not built yet.** The pool mending you while you stand in it: for now the throw mends a part at once.

**Example.** A pack gathers at a door: break a gourd on the first one and every one of them is Soaked before you reach them.

## water.base
**What it does.** Riptide, Water's own spell, comes with the element and is never bought. A wave rises ahead of you and rolls back toward you, soaking everything in its path and carrying it to your feet, within reach of your blade.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Water, it is always yours to put in a slot.

**The numbers.** Given with Water. No mana: it waits 6 seconds after a cast, less with weapon play; its Empower makes that 4.5. The wave rises 5 metres ahead, 90 degrees wide: 8 damage, 18 stagger and Soaked, and what it strikes is carried in to your feet.

**Works with.** Its Empower, its two shapes and its Master; Soaked; Deep Crack and Heavy Water on what it brings; the charged blade, which its landing charges with Water.

**Not built yet.** The Empower's wave rising 6.5 metres away: the base spell is cast by Water's kit, which takes only the Empower's shorter wait.

**Example.** Three archers at range: Riptide brings all three to your feet, Soaked, for one wide swing.

## water.base.shape
**What it does.** Opens two shapes of Riptide, chosen at any rest fire. **River Serpent**: a serpent of water goes out along a lane and back, and brings only the first enemy it takes, dropping it in front of you with its guard down; one target, from further. **Breaking Swell**: the wave does not come back; it rolls away from you and breaks, throwing everything in it down; the shape that makes room.

**When it happens.** Whenever you cast Riptide, as the shape it wears. Before this is bought, the plain wave is cast.

**The numbers.** 3 Shards. River Serpent: a lane 8 metres long and 2.4 metres wide, run at 16 metres a second: 12 damage, 30 stagger and Soaked; the first enemy it strikes is drawn 4 metres back toward you and cannot guard for 1 second. Breaking Swell: a wave 6 metres long and 90 degrees wide: 8 damage, 18 stagger and Soaked; what it strikes is thrown 3 metres away and lies down for 0.6 seconds (elites half as long, gods not at all).

**Works with.** Blowhole and the blade on the one River Serpent brings; Deep Crack's String of Bubbles down the line Breaking Swell clears.

**Not built yet.** River Serpent going out and coming back, and dropping its enemy 1.5 metres in front of you: for now the serpent runs its lane and its first enemy is drawn back 4 metres.

**Example.** A shaman behind its guards: River Serpent takes it alone, guard down, to your blade.

## water.base.master
**What it does.** Press again within a second of the wave reaching you, and it goes out again the way it came, carrying what it brought past you and on four metres, backs turned. The tide's return, in the hand.

**When it happens.** For now, on everything the wave strikes. Riptide must be in a slot.

**The numbers.** 3 Shards. For now what the wave strikes stands with its back to you for 1.2 seconds: it begins nothing, and your first weapon blow on it is critical and ends it.

**Works with.** The charged blade; Heavy Water; every mark that answers a critical blow.

**Not built yet.** The second press, sending the wave out again with what it brought: for now what it brings is left turned away.

**Example.** Riptide two soldiers in and cut the nearer at once: the first blow lands on its back, critical.

## water.drowned
**What it does.** When a Soaked enemy dies its water runs over the floor to you as a line of foam and fills part of an empty flask. Soak, then kill.

**When it happens.** Each time an enemy dies while Soaked, by any blow or spell, while you carry fewer flasks than your most.

**The numbers.** 3 Shards. Each Soaked death fills a fifth of a flask: five fill one.

**Works with.** Soaked, Broken Gourd, which it grows from, Poured Blade and Deep Well above it, and every spell that soaks a crowd.

**Not built yet.** The line of foam seen running to you.

**Example.** Whirlpool a pack and cut it down: by the last body a flask is back on your belt.

## water.poured
**What it does.** You call a copy of yourself in water to stand at your side. It repeats each of your weapon blows a beat late, for a share of its strength, and soaks what it strikes.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, 30 mana: Water's larger spell (its Empower: 25 mana, 35% for 15 seconds). The copy lasts 12 seconds. Each weapon blow that lands is struck again 0.2 seconds later for 30% of its damage and lays Soaked. One copy at a time.

**Works with.** Soaked, Heavy Water, Wellspring, Drawn From the Drowned, which it grows from.

**Not built yet.** The copy's blows carrying all of the blow's stagger: for now they carry none.

**Example.** Cast it as a brute is staggered: the finisher lands, then lands again for nearly a third.

## water.poured.shape
**What it does.** Opens two shapes of Poured Blade, chosen at any rest fire. **Poured on the Edge**, its first design: no copy; a flask is poured along the weapon, and for a while its reach is longer, every blow mends and soaks, and the last blow of a chain throws a wave. It costs a flask, not mana. **Brother at Your Back**, the fisher's brothers: the copy stands behind you facing the other way and repeats your blows on whatever is behind you; for being surrounded.

**When it happens.** Whenever you cast Poured Blade, as the shape it wears. Before this is bought, the copy at your side is cast.

**The numbers.** 3 Shards. Poured on the Edge: for 12 seconds a tenth of your weapon damage comes back as health; while this shape is worn the last blow of every chain throws a wave 5 metres long and 60 degrees wide, 10 damage, 15 stagger and Soaked. Brother at Your Back: the copy as above.

**Works with.** Heavy Water and every mark that answers the end of a chain (Poured on the Edge); Undertow and Blowhole's Blown Out when surrounded (Brother at Your Back).

**Not built yet.** Poured on the Edge's price in flasks rather than mana, the weapon's reach 40% longer, and its wave only within its 12 seconds; Brother at Your Back's copy standing behind you and facing the other way.

**Example.** Ringed by four soldiers, wear Brother at Your Back: what comes at your back is cut as you cut what is in front.

## water.poured.master
**What it does.** Make a perfect dodge while it stands, and the copy steps into the blow you left and answers the attacker at full strength.

**When it happens.** For now, every perfect dodge while Poured Blade is in a slot, once in 2 seconds.

**The numbers.** 3 Shards. A copy left where you stood strikes everything within 2.3 metres of it 0.3 seconds later for all of your last blow's damage and 30 stagger.

**Works with.** The dodge; The Tide Turns' Master; Heavy Water.

**Not built yet.** Only while the copy stands, and the answer aimed at the attacker: for now any perfect dodge leaves the answering copy.

**Example.** A brute swings at you and your copy: you roll clear and the copy answers it with your heavy blow.

## water.well
**What it does.** You carry one more flask, but a flask no longer mends you at once. It fills a well instead, and the well mends you only while you are landing blows.

**When it happens.** Drinking fills the well; it pays out while a weapon blow of yours has landed in the last 3 seconds and you are not at full health. The well shows on the screen as Well.

**The numbers.** 6 Shards. Four flasks. Each flask adds 70 to the well (more with Wellspring), and the well holds 140 at most. It pays 6 health a second.

**Works with.** Wellspring (flasks hold half again as much), Drawn From the Drowned, which it grows from, Broken Gourd (the throw still mends 20 at once).

**Example.** Drink early in a fight, then keep cutting: the health comes back as you fight, but stop and it waits.

## water.sea
**What it does.** While Wellspring burns, press the surge button again to flood the ground round you, spending a flask. For six seconds everything in the flood is kept Soaked, and every weapon blow you land runs out across the water as a wave. Then the sea draws back to you, dragging everything with it, and breaks where you face.

**When it happens.** Light Wellspring with the surge button, then press the surge button again before it ends. With no flask left, it says so and waits.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. The flood is 9 metres in radius where you stood. Each wave strikes everything within 3.6 metres of the enemy struck for 40% of the blow and 8 stagger, with Soaked. At the end, everything within 9 metres of you is dragged 4 metres in; 0.55 seconds later the sea breaks in a cone 9 metres long and 120 degrees wide: 55 damage and 90 stagger, a heavy blow.

**Works with.** Wellspring, Soaked, Heavy Water, Drawn From the Drowned (for the flask), Deep Crack after the sea breaks, a second element's Static or Chill.

**Example.** Flood a crowd, cut into the middle of it for six seconds, then roll clear and face them as the sea breaks.
