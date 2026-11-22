# Water: detailed explanations

The long explanation the chart shows for each high-level mark of the Water tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Water): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## water.soak
**What it does.** Every weapon blow that lands leaves the enemy Soaked. A Soaked enemy takes more damage from your spells. Also, any Static or Chill put on it is put on everyone standing near it too.

**When it happens.** Always on once you learn it: every weapon blow that lands, while Water is your first element (or you are all in on Water). If Water is your second element, Soaked comes from your spells, the charged blade and the R2 strike instead.

**The numbers.** 2 Shards. Soaked is either on or off, and lasts 8 seconds. Each blow restarts it. Spells deal 25% more damage to a Soaked enemy. Static or Chill put on it is also put on every other enemy within 4 metres, whether they are Soaked or not.

**Works with.** Riptide, Wellspring, Heavy Water, Deep Crack (which uses it up), Drawn From the Drowned, Spreading Water, Lightning's Static and Ice's Chill as your second element, and the reactions Steam (with Fire) and The Ground Gives (with Earth).

**Example.** Carrying Water and Lightning, cut three soldiers standing close together, then hit one with Thunderclap. The Static spreads to all three.

## water.ebbflood
**What it does.** A perfect guard brings the Flood: a splash soaks the attacker and the enemies next to it, and for a while your blows hit harder. A heavy blow that lands brings the Ebb: for a while, your guard and dodge cost half the stamina, and the timing window for a perfect guard is longer.

**When it happens.** The Flood comes with every perfect guard (press the guard button just before a blow lands). The Ebb comes with every heavy blow or finisher that lands.

**The numbers.** 3 Shards. Flood: a splash 2.5 metres around the attacker deals 6 damage, 8 stagger and Soaked. Then your weapon deals 15% more damage for 5 seconds. Ebb: dodges and blocked blows cost half the stamina for 4 seconds, and the perfect guard window is 22% longer for that time.

**Works with.** Blowhole and Deep Crack, which come after it on the chart; Spreading Water above them; Heavy Water; Undertow.

**Not built yet.** The Flood should reach farther and splash with each blow. For now, it gives more damage and one splash on the attacker.

**Example.** Land a heavy blow, then perfectly guard the enemy's answer, using the Ebb's wider window. The Flood soaks it, and your next cuts hit 15% harder.

## water.crack
**What it does.** A fist of water snaps shut on the enemy you face. A bubble as big as a body forms on it, then collapses with a crack. On a Soaked enemy, the water boils out of it: its Soaked is used up, and every other Soaked enemy nearby cracks with it. This is the detonator of the Water tree.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). It closes on the nearest enemy in reach.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 11 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 9 seconds and the damage 22.) It targets an enemy within 7 metres. The bubble collapses 0.4 seconds after you cast it, within 1 metre, dealing 18 damage and 40 stagger. On a Soaked enemy, its Soaked is used up, and every other Soaked enemy within 5 metres cracks where it stands: 18 damage and 30 stagger, and its own Soaked is used up too.

**Works with.** Soaked, Riptide and Whirlpool to soak a crowd first, Ebb and Flood, which it comes after on the chart, Spreading Water.

**Example.** Pull three soldiers to your feet with Riptide, Soaked, and snap the bubble on the middle one. All three take the crack.

## water.crack.shape
**What it does.** Unlocks two shapes for Deep Crack. You pick one at any rest fire. **Ink**: the bubble is black water from the deep sea, and it bursts into a pool of ink. Enemies in it lose track of you if you are more than three metres away, and they stay Soaked, while you can still see clearly. **String of Bubbles**: three bubbles in a row along the direction you face, each cracking a moment after the last. Aim it down a hallway.

**When it happens.** Whenever you cast Deep Crack, using the shape you picked. If you have not bought this, you cast the single bubble.

**The numbers.** 3 Shards. Ink: a pool with a 1.5-metre radius that lasts 5 seconds. Once a second, it deals 1 damage and adds Soaked. Whatever it reaches when it bursts can't see farther than 3 metres for 5 seconds. String of Bubbles: a line of water 7 metres long and 2 metres wide, moving at 13 metres a second. It deals 18 damage and 40 stagger to everything it crosses, and the crack spreads from each Soaked enemy, like above.

**Works with.** Blowhole and Whirlpool, to hold enemies in the ink; Riptide's Breaking Swell shape, to line them up for the string.

**Not built yet.** Ink's blindness should only last while an enemy stands in the ink (for now, enemies the ink reaches first stay blinded for 5 seconds). String of Bubbles should be three bubbles 2 metres apart (for now, it is one moving line).

**Example.** Use Ink in a doorway. The archers behind it lose track of you, and the soldiers in it stay Soaked for the next crack.

## water.crack.master
**What it does.** Snap it as your heavy blow lands on the same enemy, and every Soaked enemy in the fight cracks.

**When it happens.** For now, a heavy blow (a finisher counts) on the enemy within 0.3 seconds after the bubble cracks on it. Deep Crack must be in a spell slot.

**The numbers.** 3 Shards. Every Soaked enemy within 20 metres cracks where it stands, taking 18 damage and 30 stagger.

**Works with.** Soaked, and every mark that soaks a crowd; Heavy Water; Spreading Water.

**Not built yet.** The snap should be timed to your heavy blow landing, not to the crack.

**Example.** Soak the room with Riptide and Whirlpool. Then cast it on the brute in front of you, and bring the heavy blow down as it cracks. Every soaked enemy in the room cracks with it.

## water.blowhole
**What it does.** You stand on a hole in black rock that the sea blows up through. Each blow that comes is blocked and drives the sea down the hole, and its spray rises one step. Let go, and the sea shoots up as a spout under the attacker, harder for every step, and throws it up, open to your blade.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then it works on any blow in the next 3 seconds. If no blow comes, the cast is used up.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 8 seconds and 18 damage per step.) It is ready for 3 seconds. For now, the first blow is blocked and answered right away by a spout worth three steps: a cone 3 metres long and 140 degrees wide, dealing 42 damage (54 with the Empower), 66 stagger and Soaked. Whatever it hits can't start an attack or guard for 0.8 seconds.

**Works with.** Ebb and Flood (the blocked blow counts as guard work), Deep Crack on the Soaked attacker, Heavy Water, Undertow.

**Not built yet.** The blowhole itself: standing on it for 3 seconds and blocking every blow, red ones too (15 stamina each), with each blow raising the spray one step, up to five. Letting go should release the spout, at 14 damage per step, throwing enemies up for 0.8 seconds at three steps and 1.2 at five. For now, the first blow is blocked and answered as three steps, and the enemy it hits is left open instead of thrown up.

**Example.** Cast it as a brute winds up. The blow is blocked, the spout answers, and the brute stands open for your next cut.

## water.blowhole.shape
**What it does.** Unlocks two shapes for Blowhole. You pick one at any rest fire. **Still Water**: only the first blow is blocked, and you answer right away with a thrust of water six metres straight through the attacker. It's a quick parry, not a build-up. **Blown Out**: the spout comes up under you instead and throws everything within four metres out three metres. It's for making room, not hitting one target.

**When it happens.** Whenever you cast Blowhole, using the shape you picked. If you have not bought this, you cast the plain blowhole.

**The numbers.** 3 Shards. Still Water: the answer is a thrust 6 metres long and 25 degrees wide, with Blowhole's damage and stagger. Blown Out: the answer is a ring 4 metres around you with Blowhole's damage and stagger, and everything within 4 metres is thrown 3 metres out.

**Works with.** Deep Crack's String of Bubbles shape down the line Still Water opens; Undertow, to pull back what Blown Out threw.

**Not built yet.** Blown Out should build up before the spout (like the Learn mark should). For now, the first blocked blow brings it up right away.

**Example.** Surrounded, use Blown Out. The first blocked blow throws the whole ring of enemies back.

## water.blowhole.master
**What it does.** Let go at the same moment a blow lands on the blowhole, and the spout is doubled, and the blowhole stays for a second round.

**When it happens.** For now, every answer, since for now the answer always comes the moment the blow lands. Blowhole must be in a spell slot.

**The numbers.** 3 Shards. The answer hits again where it lands, for 42 damage, 66 stagger and Soaked within 1.5 metres. Blowhole's wait also ends right away, once every 20 seconds, so you can cast it again for a second round.

**Works with.** Ebb and Flood; Deep Crack; Heavy Water on the attacker it soaks.

**Not built yet.** The timing (letting go within 0.2 seconds of a blow landing on the blowhole). For now, every answer is doubled.

**Example.** Block a soldier's blow, and cast it again right away as the next one swings.

## water.lift
**What it does.** Spreading Water, Water's reaction lift. Every reaction with Soaked in it spreads at half strength to every other Soaked enemy nearby.

**When it happens.** Whenever Soaked reacts on an enemy with another affliction it has (Steam with Burn, The Ground Gives with Tremor, and the rest).

**The numbers.** 3 Shards. The reaction hits again on every other Soaked enemy within 4 metres of where it happened, at half strength: every blow it deals comes again there for half the damage. What it adds or uses up isn't done again.

**Works with.** Carrying a second element: every one of Water's ten reactions; Soaked, to soak a crowd; Ebb and Flood, which it comes after on the chart; The Sea Comes In above it.

**Example.** Water and Fire on a group of soaked soldiers. Steam goes off on one, and the water carries it on to the rest.

## water.devotion
**What it does.** The Sea Comes In, Water's Devotion, for going all in on Water only. Every spell leaves standing water where it lands. Enemies in the water are always Soaked and slower, and in the water, your dodge slides farther and your spell waits count down faster. Pull the fight onto your own water and stay there, the way the tide takes over the beach.

**When it happens.** Only while you carry Water twice (all in). A pool is left each time a spell hits.

**The numbers.** 6 Shards. A pool 4 metres across forms where a spell hits, at most one every 2.5 seconds, and lasts 10 seconds. Once a second, it deals 1 damage and adds Soaked. For now, these work everywhere, not only in water: your dodge goes 50% farther, and every spell's wait counts down 30% faster. Buying this also gives you the deep strike, Thrown on the Reef. Use it with L2 and R2 while you carry Water alone or twice. The lash wraps around the enemy, and a wave throws it 5 metres away from you.

**Works with.** Every spell in the Water tree; Deep Crack and Spreading Water on the soaked crowd; Undertow, to pull enemies onto the water.

**Not built yet.** Four pools at most, enemies in them moving 15% slower, and the faster dodge and spell waits only while you stand in your own water. Thrown on the Reef should deal 30 damage, 50 stagger, and knock the enemy down for 0.8 seconds when it hits a wall, a rock or another enemy.

**Example.** Riptide and Whirlpool leave two pools in the middle of the room. Stand between them and cast again and again, a third faster.

## water.undertow
**What it does.** A heavy blow that lands pulls every enemy near you a step closer.

**When it happens.** Every heavy blow or finisher that lands, at most twice a second. For now, it pulls everyone nearby, Soaked or not.

**The numbers.** 3 Shards. Every enemy within 6 metres of you is pulled 1.5 metres in. At most once every half second.

**Works with.** Heavy weapons with wide swings, The Tide Turns, which comes after it on the chart, Heavy Water, Whirlpool, Blowhole's Blown Out shape (which throws them back out).

**Example.** Two archers hang back while you fight a brute. Each heavy blow on the brute pulls them a step closer to your blade.

## water.tide
**What it does.** The tide in your hand. Press once, and the tide going out pulls you straight back out of reach, leaving a line of wet sand. Press again within three seconds, and the tide coming in brings you back along the line and past where you started, as a breaking wave that soaks everything on the line and carries it ahead of you.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 8 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 6 seconds and the wave 10 metres long.) For now, one press is just the incoming wave: you rush 8 metres ahead at 20 metres a second, and nothing can touch you on the way. Everything on the line, 2 metres wide, takes 10 damage, 10 stagger and Soaked, and is carried 2 metres ahead of you.

**Works with.** Undertow, which it comes after on the chart; Riptide, to pull enemies in before the wave; Deep Crack on the line it soaks; Heavy Water.

**Not built yet.** The tide going out (pulling you 5 metres straight back in 0.3 seconds, untouchable, leaving wet sand behind) and the incoming wave as the second press back along that line. For now, one press is the wave, rushing ahead.

**Example.** A pack closes in on you. Rush through it as a wave and out the far side. Every one of them is Soaked and stumbling.

## water.tide.shape
**What it does.** Unlocks two shapes for The Tide Turns. You pick one at any rest fire. **King Tide**: the wave comes in four metres wide and knocks down what it carries for a moment, but it stops where you started. It's the shape for a crowd. **Out to Sea**: there's no incoming wave. The outgoing tide takes the enemy you face along with you, pulling it 5 metres, Soaked, and dropping it at your feet, away from its friends. It's a pull, not a return.

**When it happens.** Whenever you cast The Tide Turns, using the shape you picked. If you have not bought this, you cast the plain tide.

**The numbers.** 3 Shards. King Tide: the wave is 4 metres wide and 5 metres long. Whatever it hits is knocked down for 0.6 seconds (half as long for elites; gods not at all). Out to Sea: the enemy you face is pulled 5 metres toward you and Soaked.

**Works with.** Deep Crack and Heavy Water on the crowd King Tide knocks down; Blowhole and your blade on the one Out to Sea brings to you.

**Not built yet.** King Tide should keep its own 5 metres when you buy the Empower (for now, the Empower's 10 metres wins). Out to Sea should pull you back along with the enemy.

**Example.** An archer stands behind three shields. Out to Sea brings it to your feet, alone.

## water.tide.master
**What it does.** If you start The Tide Turns right as a blow would land on you, it counts as a perfect dodge. A wave started right as a blow would land doesn't start a wait.

**When it happens.** For now, every time you cast The Tide Turns, and when you cast it within a second after a perfect dodge. The Tide Turns must be in a spell slot.

**The numbers.** 3 Shards. It only counts if you start it in the last 0.3 seconds before an enemy's blow (one within 6 metres) lands on you. Then it is a perfect dodge: you get 12 mana, the game slows for a moment, the blow misses, and every mark that answers a perfect dodge answers this one. If you cast it within 1 second after a perfect dodge, while the spell is ready, it doesn't start a wait.

**Works with.** The dodge; Ebb and Flood; every spell whose wait this shortens.

**Not built yet.** The wave's own press. For now, a perfect dodge makes your next cast skip its wait.

**Example.** Dodge a brute's swing perfectly, and rush through it right away. The spell is ready again before the brute turns around.

## water.heavy
**What it does.** Hitting a Soaked enemy makes your weapon break balance faster for a few seconds.

**When it happens.** Each time a weapon blow or one of your spells hits an enemy that is already Soaked. Each new hit restarts the timer.

**The numbers.** 3 Shards. Your weapon's blows deal 20% more stagger for 3 seconds. It can restart at most twice a second.

**Works with.** Soaked, Whirlpool, which comes after it on the chart, Ebb and Flood, Blowhole, heavy blows and finishers, and Riptide to soak first.

**Not built yet.** The planned version, where Soaked enemies are slow to recover after they attack. The game can't change how fast an enemy recovers yet, so your weapon breaks their balance faster instead.

**Example.** Soak a brute, then hit it with light blows. Its balance bar fills a fifth faster, and the finisher comes sooner.

## water.whirlpool
**What it does.** You open a whirlpool under the enemy you face. Enemies in it stay Soaked and get bitten, and your blows on them break their balance harder. This is the breaker of the Water tree.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). If you aren't facing an enemy, it opens 6 metres in front of you.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 8 seconds, and makes it last 7 seconds.) It opens on the enemy you face within 6 metres. The whirlpool has a 4-metre radius and lasts 5 seconds. It bites every half second for 3 damage, with no stagger, and adds Soaked with each bite. Each bite makes its enemy take 30% more stagger for 0.7 seconds, so while it stands in the water, every blow breaks its balance harder.

**Works with.** Soaked, Heavy Water, which it comes after on the chart, Undertow, Deep Crack, a second element's Static or Chill, and Steam if you carry Fire.

**Not built yet.** Enemies should be pulled to the middle at 1.2 metres a second, and their balance shouldn't recover inside it.

**Example.** Open it under a pack. For five seconds, every one of them stays Soaked and breaks a third faster under your blade.

## water.whirlpool.shape
**What it does.** Unlocks two shapes for Whirlpool. You pick one at any rest fire. **Maelstrom**: smaller, with a strong pull. At the end, it throws everything inside up in a spout and down again. **Rip Current**: no pull to the middle. Instead, a path of churning foam runs from the whirlpool, away from you. Enemies in it are carried down it and can't dodge against it. Cut them as the current drags them past.

**When it happens.** Whenever you cast Whirlpool, using the shape you picked. If you have not bought this, you cast the plain whirlpool.

**The numbers.** 3 Shards. Maelstrom: a 2.5-metre radius. When the bites on an enemy stop (the whirlpool ends, or the enemy leaves the water), it is thrown up for 0.8 seconds and takes 50 stagger. Rip Current: each bite carries its enemy 1.25 metres away from you, two and a half metres a second, and the bites still churn.

**Works with.** Deep Crack on the bodies Maelstrom throws up; Undertow and your blade on the enemies Rip Current carries past.

**Not built yet.** Maelstrom's strong pull (3 metres a second). Rip Current's path of foam, 2 metres wide and 9 long, lasting 6 seconds, and its rule against dodging. For now, the whirlpool itself carries the enemies it bites away from you.

**Example.** Use Maelstrom on two soldiers. They churn around for five seconds, then go up together for your heavy blow.

## water.whirlpool.master
**What it does.** Open it under an enemy right as its attack warning shows, and the attack is lost as it sinks in up to its knees. Its wait is cut in half.

**When it happens.** For now, the first enemy it bites on each cast. Whirlpool must be in a spell slot.

**The numbers.** 3 Shards. An enemy it bites in the first 0.3 seconds of its attack warning flinches and loses the attack.

**Works with.** Blowhole and Ebb and Flood, for the moment an attack starts; Heavy Water.

**Not built yet.** The wait isn't cut in half yet.

**Example.** A brute raises its club. Open the whirlpool under it, and the swing never comes.

## water.gourd
**What it does.** Holding the flask button throws the flask instead of drinking it. Where it breaks, it hits and soaks everyone nearby and leaves a pool that keeps whatever stands in it Soaked. The throw also heals you a little right away. A short press still drinks it.

**When it happens.** Hold the flask button. Each throw uses up a flask.

**The numbers.** 3 Shards. Hold for 0.3 seconds to throw. It flies at the enemy in front of you within 9 metres, or 5 metres ahead. The burst has a 2.6-metre radius and deals 18 damage, 20 stagger and Soaked. The pool has a 1.5-metre radius and lasts 6 seconds. Once a second, it deals 1 damage and adds Soaked. The throw heals you 20.

**Works with.** Drawn From the Drowned, which comes after it on the chart, to win the flask back; Deep Well; Heavy Water; Deep Crack on the soaked crowd; Steam if you carry Fire.

**Not built yet.** The pool should heal you while you stand in it. For now, the throw heals you part of that right away.

**Example.** A pack gathers at a door. Break a gourd on the first one, and every one of them is Soaked before you reach them.

## water.base
**What it does.** Riptide, Water's own spell. It comes with the element; you never buy it. A wave rises in front of you and rolls back toward you, soaking everything in its path and carrying it to your feet, within reach of your blade.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Water, you can always put it in a slot.

**The numbers.** Comes with Water. It costs no mana. After you cast it, you must wait 6 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 4.5 seconds. The wave rises 5 metres in front of you and is 90 degrees wide. It deals 8 damage, 18 stagger and Soaked, and whatever it hits is carried to your feet.

**Works with.** Its Empower, its two shapes and its Master mark; Soaked; Deep Crack and Heavy Water on what it brings; and the charged blade, which it charges with Water when it lands.

**Not built yet.** The Empower's wave rising 6.5 metres away. For now, the spell only gets the Empower's shorter wait.

**Example.** Three archers stand far away. Riptide brings all three to your feet, Soaked, for one wide swing.

## water.base.shape
**What it does.** Unlocks two shapes for Riptide. You pick one at any rest fire. **River Serpent**: a serpent made of water goes out along a path and back, and only brings back the first enemy it grabs, dropping it in front of you with its guard down. It hits one target, from farther away. **Breaking Swell**: the wave doesn't come back. It rolls away from you and breaks, knocking down everything in it. It's the shape for making room.

**When it happens.** Whenever you cast Riptide, using the shape you picked. If you have not bought this, you cast the plain wave.

**The numbers.** 3 Shards. River Serpent: a path 8 metres long and 2.4 metres wide, moving at 16 metres a second. It deals 12 damage, 30 stagger and Soaked. The first enemy it hits is pulled 4 metres back toward you and can't guard for 1 second. Breaking Swell: a wave 6 metres long and 90 degrees wide. It deals 8 damage, 18 stagger and Soaked. Whatever it hits is thrown 3 metres away and knocked down for 0.6 seconds (half as long for elites; gods not at all).

**Works with.** Blowhole and your blade on the enemy River Serpent brings; Deep Crack's String of Bubbles shape down the line Breaking Swell clears.

**Not built yet.** River Serpent should go out and come back, and drop its enemy 1.5 metres in front of you. For now, the serpent runs its path, and its first enemy is pulled back 4 metres.

**Example.** A shaman stands behind its guards. River Serpent grabs it alone, guard down, and brings it to your blade.

## water.base.master
**What it does.** Press again within a second of the wave reaching you, and it goes back out the way it came, carrying what it brought past you and on another four metres, with their backs turned. It's the tide going back out, in your hand.

**When it happens.** For now, on everything the wave hits. Riptide must be in a spell slot.

**The numbers.** 3 Shards. For now, whatever the wave hits stands with its back to you for 1.2 seconds. It can't start anything, and your first weapon blow on it is a critical hit and ends the effect.

**Works with.** The charged blade; Heavy Water; every mark that works on a critical hit.

**Not built yet.** The second press, sending the wave back out with what it brought. For now, what it brings is left turned away.

**Example.** Pull in two soldiers with Riptide and cut the nearer one right away. The first blow lands on its back as a critical hit.

## water.drowned
**What it does.** When a Soaked enemy dies, its water runs across the floor to you as a line of foam and refills part of an empty flask. Soak, then kill.

**When it happens.** Each time an enemy dies while Soaked, from any blow or spell, as long as you have fewer flasks than your maximum.

**The numbers.** 3 Shards. Each Soaked death fills a fifth of a flask. Five fill one flask.

**Works with.** Soaked, Broken Gourd, which it comes after on the chart, Poured Blade and Deep Well above it, and every spell that soaks a crowd.

**Not built yet.** The line of foam running to you isn't shown yet.

**Example.** Use Whirlpool on a pack and cut it down. By the last body, a flask is back on your belt.

## water.poured
**What it does.** You call up a copy of yourself made of water to stand next to you. It repeats each of your weapon blows a moment later, with part of the strength, and soaks whatever it hits.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards, 30 mana: Water's larger spell. (Its Empower makes it 25 mana, 35% damage, and 15 seconds.) The copy lasts 12 seconds. Each weapon blow that lands is repeated 0.2 seconds later for 30% of its damage, and adds Soaked. You can only have one copy at a time.

**Works with.** Soaked, Heavy Water, Wellspring, Drawn From the Drowned, which it comes after on the chart.

**Not built yet.** The copy's blows should carry all of the blow's stagger. For now, they carry none.

**Example.** Cast it while a brute is staggered. The finisher lands, then lands again for almost a third.

## water.poured.shape
**What it does.** Unlocks two shapes for Poured Blade. You pick one at any rest fire. **Poured on the Edge** (the spell's first design): no copy. Instead, a flask is poured along your weapon. For a while, it reaches farther, every blow heals you and soaks the enemy, and the last blow of a chain throws a wave. It costs a flask instead of mana. **Brother at Your Back** (the fisher's brothers): the copy stands behind you, facing the other way, and repeats your blows on whatever is behind you. It's for when you're surrounded.

**When it happens.** Whenever you cast Poured Blade, using the shape you picked. If you have not bought this, you cast the copy at your side.

**The numbers.** 3 Shards. Poured on the Edge: for 12 seconds, you get a tenth of your weapon damage back as health. While you have this shape picked, the last blow of every chain throws a wave 5 metres long and 60 degrees wide, dealing 10 damage, 15 stagger and Soaked. Brother at Your Back: the copy works like the plain one.

**Works with.** Heavy Water and every mark that works on the end of a chain (Poured on the Edge); Undertow and Blowhole's Blown Out shape when you're surrounded (Brother at Your Back).

**Not built yet.** Poured on the Edge should cost a flask instead of mana, make your weapon reach 40% farther, and only throw its wave during its 12 seconds. Brother at Your Back's copy should stand behind you, facing the other way.

**Example.** Four soldiers surround you. Use Brother at Your Back, and whatever comes at your back gets cut as you cut what's in front.

## water.poured.master
**What it does.** Make a perfect dodge while the copy is there, and the copy steps into the blow you avoided and answers the attacker at full strength.

**When it happens.** For now, every perfect dodge while Poured Blade is in a spell slot, once every 2 seconds.

**The numbers.** 3 Shards. A copy left where you stood hits everything within 2.3 metres of it 0.3 seconds later, for all of your last blow's damage plus 30 stagger.

**Works with.** The dodge; The Tide Turns' Master mark; Heavy Water.

**Not built yet.** It should only work while the copy is there, and the answer should aim at the attacker. For now, any perfect dodge leaves the answering copy.

**Example.** A brute swings at you and your copy. You roll away, and the copy answers it with your heavy blow.

## water.well
**What it does.** You carry one more flask, but a flask no longer heals you right away. Instead, it fills a well, and the well only heals you while you are landing blows.

**When it happens.** Drinking fills the well. The well heals you while one of your weapon blows has landed in the last 3 seconds and you are not at full health. The well shows on the screen as Well.

**The numbers.** 6 Shards. Four flasks. Each flask adds 70 to the well (more with Wellspring), and the well holds up to 140. It heals you 6 health a second.

**Works with.** Wellspring (flasks hold 50% more), Drawn From the Drowned, which it comes after on the chart, Broken Gourd (the throw still heals you 20 right away).

**Example.** Drink early in a fight, then keep cutting. The health comes back as you fight, but if you stop, it waits.

## water.sea
**What it does.** While Wellspring is lit, press the surge button again to flood the ground around you, using up a flask. For six seconds, everything in the flood stays Soaked, and every weapon blow you land sends a wave out across the water. Then the sea pulls back to you, dragging everything with it, and breaks in the direction you face.

**When it happens.** Light Wellspring with the surge button, then press the surge button again before it ends. If you have no flask left, it tells you so and waits.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. The flood has a 9-metre radius around where you stood. Each wave hits everything within 3.6 metres of the enemy you hit, for 40% of the blow's damage, 8 stagger and Soaked. At the end, everything within 9 metres of you is pulled 4 metres in. 0.55 seconds later, the sea breaks in a cone 9 metres long and 120 degrees wide, dealing 55 damage and 90 stagger as a heavy blow.

**Works with.** Wellspring, Soaked, Heavy Water, Drawn From the Drowned (for the flask), Deep Crack after the sea breaks, a second element's Static or Chill.

**Example.** Flood a crowd, cut into the middle of it for six seconds, then roll away and face them as the sea breaks.

## water.highlow
**What it does.** Your own tide goes back and forth. At high water, your spells hit harder and your weapon hits softer. At low water, it's the other way around. A perfect guard or a perfect dodge turns the tide right away.

**When it happens.** Always on once you learn it, while you carry Water. The tide turns on its own every six seconds. "High water" or "Low water" shows on the screen when it turns.

**The numbers.** Keystone, 6 Shards. High water: spells hit 33% harder, and your weapon hits 20% softer. Low water: your weapon hits 33% harder, and spells hit 20% softer. The tide starts high. A perfect guard or a perfect dodge turns it and restarts the six seconds.

**Works with.** Spreading Water, which it comes after on the chart. Ebb and Flood. Every spell in your Hand, and a weapon you like to swing.

**Example.** Cast your spells at high water. When it turns low, close in and use the blade. Need spells right now? Make a perfect dodge to turn the tide back.

## water.drawnback
**What it does.** Each time you dodge away from the enemy you face, the sea comes back with you as a wave. Your next weapon blow that lands breaks all your waves at once in front of you. If you have no waves, your blows are weaker.

**When it happens.** You get a wave with each dodge away from the enemy you face, while you carry Water. The waves break on your next weapon blow that lands. "Waves" shows how many you have, next to the weapon.

**The numbers.** Keystone, 6 Shards. Up to three waves. For each wave, the break deals 12 damage and 20 stagger in front of you, out to 4 metres. Everything it hits is Soaked and thrown 2 metres back. With no waves, your weapon hits 20% softer.

**Works with.** Heavy Water, which it comes after on the chart. Undertow and The Tide Turns, which pull enemies back in after you. Anything that uses Soaked.

**Example.** Dodge back three times from a pack, then step in with one blow. Three waves break for 36 damage and 60 stagger, and they are all thrown back and soaked.

## water.spilled
**What it does.** When you drink a flask, a ring of water bursts out from you. It soaks everyone nearby and pushes them back, so you get room to breathe.

**When it happens.** Each time you drink a flask, while you carry Water.

**The numbers.** Major, 3 Shards. The ring reaches 3.5 metres. Everything in it takes 1 damage and 15 stagger, is Soaked, and is pushed 2.5 metres away.

**Works with.** Spreading Water, which it comes after on the chart. Everything that uses Soaked. Deep Well and Broken Gourd, the other flask marks.

**Example.** You're surrounded and hurt, so you drink. The ring throws the pack back and soaks them, ready for your next reaction.
