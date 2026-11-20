# Dark: detailed explanations

The long explanation the chart shows for each high-level mark of the Dark tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Dark): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Cursed" is Dark's affliction: a curse that makes the enemy
strike weaker, and whose death feeds you. "Risen" is the state after the Rise: stood up again from a death, your health
draining.

## dark.curse
**What it does.** Every blow of your weapon that lands curses the enemy. A Cursed enemy hits you more weakly, and killing it heals you. And once, the blow that would kill you does not: you rise, Risen, with half your health, and it drains away.

**When it happens.** Always on once learned: every weapon blow that lands, light or heavy, while Dark is your first element. Weapon blows lay only the first element's affliction; carried second, Dark curses through Grasp, its other spells, the charged blade and its strike.

**The numbers.** 2 Shards. Cursed is one deep and lasts 8 seconds; each blow sets its clock back. A Cursed enemy deals 15% less damage, and killing it heals you 6.

**Works with.** Grasp and every Dark spell (each one Curses), Dread, Devouring Maw (it spends the Curse), Collected Twice, Nothing Stays Buried, Never Sated, Hunger (every blow curses while it burns), and the charged blade, Shadow Cut Loose.

**The Rise.** Once, while Dark is carried, the blow that would kill you does not. You stand again with half your health, and it drains 4 a second, never the last point. A blow that kills you while Risen is a fall; after a fall you may rise again. Only the god's gift, A Life for a Life, buys the Rise back sooner, with a finisher on the one that killed you. Dead Already turns it off.

**Example.** Cut the brute first: while it is Cursed every swing of its takes 15% less of your health, and when it falls you get 6 back.

## dark.dread
**What it does.** When a Cursed enemy dies, the other Cursed near it lose heart: their balance buckles and their guard falls.

**When it happens.** Each time a Cursed enemy dies, whatever killed it; at most once a second.

**The numbers.** 3 Shards. 30 stagger to every enemy within 8 metres of where it fell, and every Cursed one among them cannot raise its guard for 3 seconds. No damage.

**Works with.** The root, Debts Called and Grasp's The Pit (a knot of Cursed enemies), Devouring Maw and Gone to Ash, which grow from it, Collected Twice, Nothing Stays Buried (the dead stand up too).

**Not built yet.** The buckling for the Cursed alone: for now every enemy near where it fell takes the stagger. An attack it was showing dropped.

**Example.** Kill a Cursed soldier at the front of a pack and the Cursed four behind it are 30 nearer a stagger and open to your blows.

## dark.maw
**What it does.** Devouring Maw, the detonator of the tree. A jaw twice your height rises out of your shadow and lunges down a lane before you. On a Cursed enemy it spends the Curse for more, and feeds you.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 11 seconds after a cast, less with weapon play (its Empower: 10). It runs 5 metres along its lane: 24 damage and 30 stagger. Each Cursed enemy it bites loses its Curse, takes 20 damage more and heals you 12. The jaw itself lays no Curse: curse first, then bite.

**Works with.** The root, Grasp and Debts Called (curse first), Dread, which it grows from, Never Sated, Hunger, and the charged blade.

**Not built yet.** The Empower's heal of 18: for now it only shortens the wait.

**Example.** Curse a row of soldiers in a corridor with Grasp, then loose the jaw down it: each one bitten takes 44 and gives you 12.

## dark.maw.shape
**What it does.** Opens two shapes of Devouring Maw, chosen at any rest fire. **Rahu**, the head with no body that swallows the sun: an ordinary Cursed enemy is swallowed whole, and a moment later rolls out of the open throat at your feet, staggered, because there is nothing to hold it. A god or an elite is only bitten. **From Its Own Shadow**: the jaw rises out of the enemy's own shadow instead, up to nine metres away, and closes on it from below. No lane: it reaches the one far off.

**When it happens.** Whenever you cast Devouring Maw, as the shape it wears. Before this is bought, the plain jaw is cast.

**The numbers.** 3 Shards. Rahu: each ordinary enemy bitten is held still and cannot attack or guard for 1.5 seconds, then is dragged 5 metres toward you and takes 60 stagger. From Its Own Shadow: one jaw from below on the nearest enemy within 9 metres, 1.6 metres round, 24 damage and 30 stagger, a moment after the cast.

**Works with.** Rahu: a heavy weapon (the swallowed one comes out staggered at your feet for a finisher), Near the Grave. From Its Own Shadow: archers and casters at the back, Grasp's Tethered.

**Not built yet.** Rahu taking only the Cursed: for now every ordinary enemy bitten is swallowed, because the bite spends the Curse first. The enemy hidden in the throat: for now it stands held where it was bitten. From Its Own Shadow drawn rising out of the enemy's shadow, and aimed at the enemy you face: for now it rises under the nearest.

**Example.** Rahu on a Cursed soldier: it vanishes into the jaw and drops at your feet staggered, and your heavy blow finishes it.

## dark.maw.master
**What it does.** Bite an enemy in its wind-up, and its attack is swallowed and broken, and Devouring Maw is ready again.

**When it happens.** Each bite, with Devouring Maw in a slot.

**The numbers.** 3 Shards. For now every enemy bitten has what it had begun broken (as a flinch, 0.4 seconds), and a bite ends Devouring Maw's wait once in 16 seconds.

**Works with.** Reading the enemies' wind-ups, Gone to Ash (another answer to an attack), Dread.

**Not built yet.** The enemy's wind-up as the moment: for now every bite breaks what was begun, and the reset comes once in 16 seconds whatever it bit.

**Example.** A brute raises its club: the jaw takes it mid-swing, the blow never lands, and the jaw is ready to go again.

## dark.ash
**What it does.** Gone to Ash, Dark's counter. You stand with arms crossed as grey ash runs over you. The first blow to land scatters you as a cloud of ash, and you take nothing. A moment later you stand again behind the attacker. It is Cursed, and your first blow on it within a second strikes as a heavy and heals you.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Then let them strike you.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 9, and it holds 1.5 seconds). It holds 1.2 seconds. The blow turned does nothing to you; the answer Curses what is in front of you within 2.5 metres, you step 1.5 metres past the attacker, and your first weapon blow within a second strikes twice as hard and heals 10.

**Works with.** The root, Dread, which it grows from, Devouring Maw (bite what you came out behind), Near the Grave, and a heavy weapon.

**Not built yet.** The wait of 5 seconds when nothing struck. The scattering into ash and standing again a moment later: for now you step at once. The blow struck as a heavy: for now it strikes twice as hard. The Empower's heal of 15.

**Example.** A captain swings at you: you scatter, stand at its back, and your cut lands double and gives you 10.

## dark.ash.shape
**What it does.** Opens two shapes of Gone to Ash, chosen at any rest fire. **Ash on the Brow**, the ash on Shiva's brow: you stand again where you were, and the ash settles on the attacker's brow as a grey smear. For six seconds it is Cursed, the next blow it lands on you scatters you for nothing once more, and your first blow on it is a heavy. The ash marks the one who struck. **Ash Hangs**: you stand again where you were, and the ash stays as a sinking cloud. Enemies inside it are Cursed, and an attack they begin inside goes where they already faced.

**When it happens.** Whenever you cast Gone to Ash, as the shape it wears. Before this is bought, the plain counter is cast.

**The numbers.** 3 Shards. Ash on the Brow: the attacker's Curse does not fade for 6 seconds, and you gain one ward that turns the next blow that would land. Ash Hangs: a cloud 3 metres across on the attacker for 3 seconds, Cursing what stands in it each second.

**Works with.** Ash on the Brow: a duel with one strong enemy, Dread, Collected Twice. Ash Hangs: a pack, Dread, Debts Called's Collected.

**Not built yet.** Both shapes still step you behind the attacker: a shape cannot yet set aside its spell's step. Ash on the Brow's ward turns any blow, not only that attacker's, and its grey stripe is not drawn. Ash Hangs' attacks that cannot turn: for now they are only Cursed.

**Example.** Ash Hangs in a ring of soldiers: the one that struck and the two beside it come away Cursed.

## dark.ash.master
**What it does.** Scatter a red attack, and the attacker is staggered and Gone to Ash is ready again.

**When it happens.** Each blow Gone to Ash scatters, with it in a slot.

**The numbers.** 3 Shards. For now any blow scattered gives the attacker 60 stagger and ends Gone to Ash's wait, once in 20 seconds.

**Works with.** Gods and elites (their red attacks), Devouring Maw and Pawned Heart (the staggered one for a great blow).

**Not built yet.** A red attack as the blow scattered: for now any blow does it, once in 20 seconds.

**Example.** A god's red overhead comes down on you: you scatter, it reels, and the counter is ready for its next one.

## dark.lift
**What it does.** Collected Twice, Dark's reaction lift. Every reaction the Curse takes part in leaves a black thumbprint on the brow of what it struck. The next weapon blow on it repeats the reaction's damage. The debt is collected by the blade.

**When it happens.** Whenever a reaction with Cursed in it happens, on any enemy; the print lasts 4 seconds or until your next weapon blow on it.

**The numbers.** 3 Shards. A weapon blow on a printed enemy within 4 seconds strikes it for 15 more.

**Works with.** Every pair with Cursed in it (Dark's reactions with its neighbours and the rest), the root, Dread, which it grows from, Never Sated and Nothing Stays Buried, which grow from it.

**Not built yet.** The reaction's own damage repeated: for now 15, whatever the reaction did. The print on everything the reaction struck: for now only on the enemy it happened on. The black print drawn on the head.

**Example.** A Curse reaction fires on a soldier: your next cut on it lands with 15 more.

## dark.neversated
**What it does.** You carry no flasks at all. Instead every blow you land feeds you a little, every Cursed kill a great deal, and every finisher more.

**When it happens.** Always on once learned: with every weapon blow that lands, every Cursed enemy killed and every finisher.

**The numbers.** 6 Shards. One flask is left (the game keeps one at least), and none are earned back. Each blow heals 6% of its damage. A Cursed kill heals 12, on top of Cursed's own 6. A finisher heals 20.

**Works with.** The root, Devouring Maw (it feeds as well), Hunger (its share and this one together), Collected Twice, which it grows from, Dead Already.

**Example.** Against a long pack, never stop: each kill and finisher keeps you standing where a flask would have.

## dark.devotion
**What it does.** Nothing Stays Buried, Dark's Devotion: all in on Dark only. Every enemy that dies Cursed stands up again as your shade, ash-grey with violet eyes, and fights for you for a while. While you are Risen, every enemy that dies stands up, Cursed or not. The vetala riding the dead; The Dead Stand, once a spell of its own, lives on here.

**When it happens.** Only while Dark is carried twice (all in). Each Cursed death, and each death while you are Risen.

**The numbers.** 6 Shards. Each shade stands where its body fell for 6 seconds and strikes what is within 2.5 metres of it every 0.8 seconds: 12 damage, 12 stagger and a Curse each time. Buying this also gives you the deep strike, Yama's Noose. Use it with L2 and R2 while you carry this element alone or twice.

**Works with.** The root and every Dark spell (Curse them before they die), Dread, Collected Twice, which it grows from, Dead Already (Risen for good, every death stands up), A Life for a Life.

**Not built yet.** Three shades at most: for now as many as die. Each shade walking to its own enemy: for now it fights where it fell. A Cursed death while you are Risen raises two shades, not one.

**Example.** Kill three Cursed soldiers in a knot: three grey shades stand up among the rest and keep cutting while you turn to the captain.

## dark.bloodprice
**What it does.** When a spell asks more mana than you hold, the rest is paid in health. That health is not lost at once: it turns grey, and dealing damage wins it back like any grey health.

**When it happens.** Each time you cast a spell paid in mana with too little mana for it: another element's larger spell or a duo spell. Dark's own spells cost no mana: they wait on cooldowns, and Pawned Heart is paid in health.

**The numbers.** 3 Shards. Each point of mana you lack costs half a point of health, taken as grey health. It never takes your last point: if the health is not there, the spell is not cast.

**Works with.** Near the Grave and Debts Called, which grow from it (they reward low health), Pawned Heart, duo spells.

**Not built yet.** Blood Price re-aimed at cooldowns, as its design asks: a spell still on its cooldown cast anyway, paid in health, 1 for each second left, as grey health owed, never the last point. For now it pays the mana a spell lacks.

**Example.** At 10 mana, cast a duo spell for 40: it goes off, 15 health turns grey, and the blows that follow win it back.

## dark.debtscalled
**What it does.** Debts Called, the breaker of the tree. Everything near is dragged in to a point just in front of you, and when it gets there a burst of shadow strikes it all at once. The nearer you are to death, the harder it strikes.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 9). The point is 1 metre ahead of you. For half a second everything within 6 metres of it is drawn in; then it all takes 18 damage, 35 stagger and a Curse. Cast below half your health, spells strike half again as hard for the next second.

**Works with.** The root, Blood Price, which it grows from, Near the Grave and Pawned Heart (low health), Dread, Devouring Maw, and the charged blade.

**Not built yet.** The damage rising smoothly as your health falls, 36 at one point: for now a step at half your health. The burst throwing back what it struck. The Empower's eight metres: for now it only shortens the wait.

**Example.** Four soldiers spread round you: one cast draws them into a knot at your sword's point, all Cursed, ready for a wide swing.

## dark.debtscalled.shape
**What it does.** Opens two shapes of Debts Called, chosen at any rest fire. **The Black Buffalo**, the buffalo Yama rides: no pull. A buffalo of sinking black smoke charges through the point before you, carrying everything Cursed on its horns to the far end and throwing it down. Follow the lane and cut what it threw: Dark's one push. **Collected**: only the Cursed are called, from further, and each one that reaches you gives back health. There is no burst.

**When it happens.** Whenever you cast Debts Called, as the shape it wears. Before this is bought, the plain call is cast.

**The numbers.** 3 Shards. The Black Buffalo: a charge 8 metres down a lane about 2.4 metres wide, 18 damage, 35 stagger and a Curse to all it runs through. Collected: everything within 10 metres is drawn in and takes 20 stagger and no damage; each Cursed one heals you 4.

**Works with.** The Black Buffalo: a heavy weapon to follow the lane, Dread, Devouring Maw's lane. Collected: the root and Grasp (Curse them first), Never Sated, Near the Grave.

**Not built yet.** The buffalo carrying only the Cursed and throwing them down at the far end, and drawn as a buffalo of smoke: for now a lunge down the lane in the jaw's shape. Collected calling only the Cursed: for now it draws everything in, and only the Cursed heal you.

**Example.** Collected in a pack of five Cursed soldiers: they come to your feet reeling, and you are 20 health better.

## dark.debtscalled.master
**What it does.** Cast it below a third of your health, and the burst, or the buffalo, strikes twice as hard.

**When it happens.** Each cast below a third of your health, with Debts Called in a slot.

**The numbers.** 3 Shards. For the second after such a cast your spells deal twice their damage (two and a half times with the Learn's own step below half health).

**Works with.** Near the Grave, Pawned Heart (one point of health the whole time), Blood Price, Dead Already.

**Not built yet.** Only the burst or the buffalo strengthened: for now every spell landing in that second is.

**Example.** Pawn the heart, then call: at one point of health the burst strikes four soldiers for 45 each.

## dark.nearthegrave
**What it does.** Below a third of your health you strike harder, and each kill wraps you in a shade that takes the next blow.

**When it happens.** Always on once learned, while your health is below a third. The shade comes with each kill made down there.

**The numbers.** 3 Shards. Below a third: 20% more weapon damage. Each kill below a third gives one ward, three at most.

**Works with.** Blood Price, which it grows from, Pawned Heart and Dead Already, which grow from it, Debts Called's Master, Hollow Strength, A Life for a Life.

**Example.** At 25 health in a pack: cut harder, and each soldier that falls leaves you a shade against the next one's blow.

## dark.pawnedheart
**What it does.** Pawned Heart, Dark's larger spell, paid in health, not mana. Your health drops to one and the rest is set down behind you as a beating heart in a ring that closes. While it beats your weapon strikes far harder and swings cost little stamina. Walk away from the heart and then back onto it, and all the health you pawned comes back with a share of the damage you dealt meanwhile. If its ring closes first, the heart stops: you keep one point, and half of what you pawned comes back only as grey health.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). It cannot be cast at one point of health.

**The numbers.** 4 Shards, no mana: the price is health, and it waits 40 seconds after a cast, less with weapon play (its Empower: 36, 70% more and a ring of 12 seconds). The heart lies 1.8 metres behind you and beats for 10 seconds. While it beats: 60% more weapon damage. You must step 2.6 metres from it, then come within 1 metre of it again: back come all the health pawned and 30% of the damage you dealt. A blow that lands on you while it beats can kill you.

**Works with.** Near the Grave, which it grows from (you are below a third the whole time), Debts Called's Master, Dead Already, Hunger, and the wards of Near the Grave and Gone to Ash.

**Example.** Pawn the heart beside a staggered brute, cut it down in five strong swings, then step back onto the heart and stand up with more health than you pawned.

## dark.pawnedheart.shape
**What it does.** Opens two shapes of Pawned Heart, chosen at any rest fire. **Buried Heart**: the heart sinks into the floor where you stand and beats under it, a dark pulse in a ring of ash. Your weapon is strong only while you stand near it; when the ring closes it rises whole. Stand your ground and cut. **Hung From the Tree**, the vetala's tree: the heart hangs in the air over the enemy you face instead, and follows it. You must fight your way in under it to touch it. You go in, not out.

**When it happens.** Whenever you cast Pawned Heart, as the shape it wears. Before this is bought, the plain heart is set behind you.

**The numbers.** 3 Shards. For now both shapes still set the heart behind you as learned. Buried Heart: a ring of ash 3 metres round where you cast lies for 10 seconds, Cursing what stands in it each second. Hung From the Tree: the enemy you face keeps its Curse for the 10 seconds.

**Works with.** Buried Heart: a stand against a pack that comes to you, Gone to Ash. Hung From the Tree: a duel with one strong enemy, Grasp's Tethered.

**Not built yet.** The heart under your feet, its strength only within 3 metres, and rising whole with 30% of the damage dealt when the ring closes. The heart hung over the enemy and following it. Both wait for the heart form to set its heart elsewhere.

**Example.** Buried Heart in a doorway: the soldiers that come through it stand in your ring of ash and come away Cursed.

## dark.pawnedheart.master
**What it does.** Touch the heart during a perfect dodge, and it returns 60% of the damage dealt, not 30%.

**When it happens.** For now each perfect dodge, with Pawned Heart in a slot.

**The numbers.** 3 Shards. For now a perfect dodge heals you 10, once in 3 seconds.

**Works with.** Clean dodging, Near the Grave (low health a heal matters at), Gone to Ash.

**Not built yet.** The touch of the heart during a perfect dodge as the moment, and the 60%: for now any perfect dodge heals 10, heart or no heart.

**Example.** At one point of health with the heart beating, roll through a soldier's thrust at the last moment: 10 health back while you work your way home.

## dark.deadalready
**What it does.** You do not wait to die: you are Risen from now on. In every fight your health drains toward its last point, and your blows feed you. You strike far harder. There is no second rising.

**When it happens.** Always on once learned. The drain runs while an enemy is within 12 metres of you.

**The numbers.** 6 Shards. 25% more weapon damage, and 30% of each blow's damage comes back as health. The drain takes 2 health a second, never the last point. Every mark that asks for you to be Risen is in force at all times. A Life for a Life can no longer stand you up.

**Works with.** Near the Grave, which it grows from, Hollow Strength (another 25%, no flinch, and kills still the drain), Nothing Stays Buried (every death stands up), Hunger (another 30% back), Never Sated.

**Example.** Fight without pause: every cut gives back far more than the drain takes, and the moment you stop to wait, it bleeds you down.

## dark.hollowstrength
**What it does.** While you are Risen, you strike harder and no blow makes you flinch. Each kill stills the draining of your health for a few breaths.

**When it happens.** While Risen: after A Life for a Life stands you up, or for good with Dead Already.

**The numbers.** 3 Shards. Risen: 25% more weapon damage, and blows that land do not stop you. Each kill stops the drain for 3 seconds.

**Works with.** A Life for a Life, Dead Already, Second Shadow, which grows from it, Nothing Stays Buried (Risen, every death stands up), Near the Grave.

**Example.** Risen with a sliver of health: swing straight through the soldiers' blows, and each one you kill gives you three seconds without the drain.

## dark.base
**What it does.** Grasp, Dark's own spell, comes with the element and is never bought. A black ball crawls along the ground to the enemy you face, drawing in what it passes. Where it arrives it strikes and Curses, and what it has hold of is dragged to your feet.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Dark, it is always yours to put in a slot.

**The numbers.** Given with Dark. No mana: it waits 7 seconds after a cast, less with weapon play; its Empower makes that 6. It reaches the enemy you face within 6 metres: 10 damage, 12 stagger and Cursed to all within 1.7 metres of where it arrives.

**Works with.** Its Empower, its two shapes and its Master; the root, Dread, Devouring Maw and Debts Called's Collected (Cursed enemies for them), the charged blade, which it charges with Dark.

**Not built yet.** The Empower's 14 damage and eight metres: the base spell is cast by Dark's kit, which takes only the Empower's shorter wait.

**Example.** Grasp the archer at the back: it is dragged to your feet Cursed, and the jaw finishes it.

## dark.base.shape
**What it does.** Opens two shapes of Grasp, chosen at any rest fire. **Tethered**, Yama's noose: a rope of black is thrown instead and binds the enemy you face to you. It cannot go far from you, and your first heavy blow on it yanks it to your feet. You keep it close. **The Pit**: the ball stops at the point you aim and spreads into a pool of ink, dragging everything in it slowly to the middle and Cursing it. You gather them at a spot.

**When it happens.** Whenever you cast Grasp, as the shape it wears. Before this is bought, the plain ball is cast.

**The numbers.** 3 Shards. Tethered: for 4 seconds after Grasp strikes an enemy, your first heavy blow on it drags it 4 metres toward you and gives it 20 stagger. The Pit: a pool 3 metres across, 6 metres ahead, for 4 seconds, biting for 2 and Cursing what stands in it each second.

**Works with.** Tethered: a duel, Devouring Maw's From Its Own Shadow, a heavy weapon. The Pit: Dread, Debts Called, Ash Hangs, a pack.

**Not built yet.** Tethered's rope thrown in place of the ball, and the enemy kept within 5 metres of you: for now the ball is cast and only the yank of the heavy blow is built. The Pit drawing everything to its middle: for now it only bites and Curses. With the Empower bought, The Pit takes its numbers too, eight metres ahead.

**Example.** The Pit under a pack in a doorway: all of it is Cursed in four seconds, and Dread is ready for the first death.

## dark.base.master
**What it does.** Cast it within a second of being struck, and the one that struck you is dragged to your feet and Cursed twice. Dark answers when it is hurt.

**When it happens.** A blow landing on you, with Grasp in a slot.

**The numbers.** 3 Shards. For a second after a blow lands on you, the first blow of yours on the one that struck you, Grasp or any other, drags it 4 metres toward you and lays two Curses.

**Works with.** Pawned Heart and Near the Grave (being hurt is the plan), Gone to Ash, Dread.

**Not built yet.** Grasp's own cast as the answer: for now any blow of yours on the attacker within the second does it.

**Example.** A spearman pokes you from its reach: answer at once, and it is at your feet and Cursed.

## dark.secondshadow
**What it does.** Second Shadow, Dark's movement spell. Leave your shadow where you stand: your shape in flat black with violet eyes, joined to you by a thin line on the ground. For a while it repeats every blow you land, a beat late, at half the damage, and Curses.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Then every weapon blow that lands is echoed.

**The numbers.** 4 Shards, no mana: it waits 12 seconds after a cast, less with weapon play (its Empower: 10). It stands for 8 seconds. Each echo lands a moment after your blow, for 50% of its damage, and lays a Curse. One shadow at a time.

**Works with.** The root, Hollow Strength, which it grows from, Pawned Heart (half of a much bigger blow), Near the Grave, Hunger.

**Not built yet.** Pressing again to change places with the shadow, once. The Empower's 60%: for now it only shortens the wait.

**Example.** Cast it as a brute kneels staggered: every cut of the finisher's opening lands twice.

## dark.secondshadow.shape
**What it does.** Opens two shapes of Second Shadow, chosen at any rest fire. **Return to the Shadow**: the shadow fights as learned, but the second press does not swap. Within its time, press again to sink into it and rise where it lies, and every wound taken since you left it comes back as grey health to win back. A rewind paid in position. **Long Shadow**: your shadow is thrown out along the floor ahead, long as an evening shadow. You sink into its foot and rise at its head a moment later, and everything it lay across is Cursed. No copy: just the road.

**When it happens.** Whenever you cast Second Shadow, as the shape it wears. Before this is bought, the plain shadow is cast.

**The numbers.** 3 Shards. Return to the Shadow: for the shadow's 8 seconds you take a quarter less from every blow. Long Shadow: you travel 8 metres ahead in about a third of a second, and everything within a metre of the road takes 2 damage and a Curse.

**Works with.** Return to the Shadow: Pawned Heart, Near the Grave, a fight you mean to stand in. Long Shadow: Debts Called and Grasp (a road into the pack, then the call), Dread.

**Not built yet.** Return to the Shadow's second press, and the wounds given back as grey health: for now the quarter less stands in. Long Shadow sunk into the floor and unseen along the road: for now you are seen going.

**Example.** Long Shadow through a line of three soldiers: you rise behind them with all three Cursed.

## dark.secondshadow.master
**What it does.** Change places within a second of being struck, and the blow passes to the shadow, and the health it took comes back to you.

**When it happens.** For now each cast, with Second Shadow in a slot.

**The numbers.** 3 Shards. For now each cast gives one ward that turns the next blow that would land on you, three at most.

**Works with.** Near the Grave (wards too), Pawned Heart (one point of health, one blow turned), Gone to Ash.

**Not built yet.** Changing places within a second of being struck as the moment, the blow passed to the shadow and its health given back: for now the ward on each cast stands in.

**Example.** Cast the shadow before a brute's chain: its first blow breaks on nothing.

## dark.lifeforalife
**What it does.** When you are Risen (Dark's root stood you up from a blow that would have killed you), a sigil hangs over the enemy that killed you: land a finisher on it and you live again, health returns, and you may rise once more.

**When it happens.** While Risen. The finisher on the killer, or on anyone if the killer is already gone, ends the Rise.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. The finisher gives back half your full health and readies the rising again. Without the gift you stay Risen, draining to one point, and a blow that kills you then is a fall; after a fall you may rise again.

**Works with.** Hollow Strength, Nothing Stays Buried (Risen, every death stands up), Near the Grave, Never Sated, Hunger.

**Example.** A brute's red overhead kills you: the root stands you up, Risen. Break its balance, finish it, and stand with half your health and your rising ready again.
