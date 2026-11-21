# Lightning: detailed explanations

The long explanation the chart shows for each high-level mark of the Lightning tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Lightning): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## lightning.static
**What it does.** Light blows build up Static on an enemy, one stack at a time, up to five. Static does nothing by itself. A heavy blow or a finisher sets it all off at once, into the enemy you hit and everyone standing near it.

**When it happens.** Always on once you learn it. While Lightning is your first element, every light blow that lands adds a stack. If Lightning is your second element, your weapon adds none, and Static comes from your Lightning spells, the charged blade and the R2 strike. Either way, a heavy blow or a finisher on an enemy with Static sets it off.

**The numbers.** 2 Shards. One stack per light blow, up to five, lasting 6 seconds. Setting it off deals 7 damage and 5 stagger for each stack to everything within 4 metres: 35 damage and 25 stagger at five.

**Works with.** Single Stroke, Waiting Thunder, Chain Lance, Seven Flashes, Overdrive's faster swings, and Ionised Air or Thunder Lift on the mixed branches.

**Example.** Four light blows on a soldier, then a heavy blow. 28 damage jumps out of it into the two soldiers next to it as well.

## lightning.lift
**What it does.** Strikes Twice. Every reaction with Static in it hits again half a second later where it happened, the way lightning comes back down the same path.

**When it happens.** Whenever Static reacts on an enemy with another affliction it has (Flashover with Burn, and the rest).

**The numbers.** 3 Shards. Half a second after the reaction, a bolt hits within 2 metres of the enemy, for 10 damage, 10 stagger and a stack of Static.

**Works with.** Carrying a second element: every one of Lightning's ten reactions. Chain Lance and Waiting Thunder, which come after it on the chart.

**Not built yet.** The reaction itself should happen again at half strength. For now, a bolt falls there.

**Example.** Burn and Static meet on a soldier. Flashover goes off, and a moment later the bolt follows it down.

## lightning.lance
**What it does.** You throw a bolt forward that jumps through up to five enemies, leaving Static on each one.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 7 seconds to cast it again (its Empower makes it 5.5, with six jumps). It reaches 12 metres to the first enemy, then jumps up to 5 metres from one enemy to the next. Each takes 10 damage, 15 stagger and two stacks of Static.

**Works with.** Strikes Twice, which it comes after on the chart; Static; Waiting Thunder's heavy blow, which sets off the Static it added.

**Example.** Start a fight from far away on a pack of five archers. Each one gets two Static before they can draw their bows.

## lightning.lance.shape
**What it does.** Unlocks two shapes for Chain Lance. You pick one at any rest fire. **Stepped Leader**: the lance isn't thrown. Instead, a bolt steps out along the ground one bright step at a time, adding Static to anything near it. Then the return stroke lights up the whole path at once. **Return Stroke**: after its last jump, it comes back along the same path, hitting each enemy again for half.

**When it happens.** Whenever you cast Chain Lance, using the shape you picked.

**The numbers.** 3 Shards. Stepped Leader: it goes up to 12 metres, taking a step every 0.15 seconds, with 4 damage and a Static on each step. 1.2 seconds after you cast it, the return stroke hits the whole path for 14 damage, 10 stagger and a Static. Return Stroke: 0.6 seconds later, each enemy it hit takes 5 damage and 5 stagger again.

**Works with.** Static and Single Stroke (the return stroke's Static, then a heavy blow).

**Not built yet.** Steering Stepped Leader with the stick while you fight, having your heavy blow call the return stroke, and Return Stroke charging the blade for five blows.

**Example.** Send Stepped Leader down a hallway of soldiers. They all have Static when the return stroke lights up.

## lightning.lance.master
**What it does.** Throw Chain Lance within half a second after a finisher, and it jumps to every enemy within twelve metres.

**When it happens.** When a finisher lands. Chain Lance must be in a spell slot.

**The numbers.** 3 Shards. For now, the finisher itself throws the bolt: 10 damage, 10 stagger and two Static, jumping to up to eight enemies within 12 metres, once every 7 seconds.

**Works with.** Static, Waiting Thunder.

**Not built yet.** It should only work when you throw the lance within half a second of the finisher.

**Example.** End a chain of blows on the captain, and every soldier in the yard has Static.

## lightning.waiting
**What it does.** Five bolts hang at your shoulder, crackling. Each light blow you land sends one down onto the enemy you hit. A heavy blow sends all the ones that are left. Any not sent when the time runs out fade away.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then your blows send the bolts.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again (its Empower makes it 8, with seven bolts). Five bolts, for 10 seconds. Each deals 8 damage, 6 stagger and one stack of Static.

**Works with.** Strikes Twice, which it comes after on the chart; Static (the bolts build it up as you cut); Single Stroke.

**Example.** Cast it, land two light blows, then a heavy blow. The last three bolts fall with it, and the heavy blow sets off the Static they added.

## lightning.waiting.shape
**What it does.** Unlocks two shapes for Waiting Thunder. You pick one at any rest fire. **Hanging Cloud**: the bolts hang in a small black cloud over the enemy you face and fall on it while you fight. **Hammer Fall**: light blows send nothing. Your heavy blow brings all of it down as one bolt.

**When it happens.** Whenever you cast Waiting Thunder, using the shape you picked.

**The numbers.** 3 Shards. Hanging Cloud: it hangs over the enemy you face, within 8 metres. A bolt falls every 0.9 seconds within 1.5 metres for 10 seconds, each dealing 8 damage, 6 stagger and a Static. Hammer Fall: the enemy you face is marked for 10 seconds, and your next blow on it brings down one bolt within 3 metres, for 40 damage, 60 stagger and two Static.

**Works with.** Static and Single Stroke (Hammer Fall into a heavy blow that sets off the Static).

**Not built yet.** Hanging Cloud's bolts should fall as you hit anyone (for now, they fall on their own). Hammer Fall should only wait for a heavy blow.

**Example.** Hang the cloud over the archer, and fight the soldier next to you.

## lightning.waiting.master
**What it does.** Land the heavy blow within a second of the last bolt falling from a light blow, and all five come back for a second round.

**When it happens.** When a heavy blow lands. Waiting Thunder must be in a spell slot.

**The numbers.** 3 Shards. For now, every heavy blow brings five bolts down around the enemy you hit, each dealing 8 damage, 6 stagger and a Static, once every 10 seconds.

**Works with.** Static, Single Stroke.

**Not built yet.** It should only work within a second of the last bolt falling from a light blow.

**Example.** Light, light, light, light, light, heavy: and the sky goes around again.

## lightning.devotion
**What it does.** In One Breath, Lightning's Devotion, for going all in on Lightning only. After a perfect guard or a perfect dodge, your spells have no wait for a moment. Cast everything you have during that moment. Then all their waits start from the beginning.

**When it happens.** Only while you carry Lightning twice (all in), once you buy it. Every perfect guard and perfect dodge.

**The numbers.** 6 Shards. For 2.5 seconds after the perfect guard or dodge, all four spell slots are ready, no matter how long their waits have left. A spell cast during that moment doesn't start its wait yet. When the moment ends, its whole wait starts. The waits of spells you didn't cast keep counting down underneath. The L2 strikes keep their own timers, and spells that cost mana still cost their mana. A perfect guard or dodge during the moment makes it last 2.5 seconds from then. Buying this also gives you the deep strike, Down the Channel. Use it with L2 and R2 while you carry Lightning alone or twice. It costs 20 mana and has its own 8-second wait.

**Works with.** Ring of Drums and Thunderclap's Master mark, which also work on a perfect guard.

**Example.** Wait for the brute's swing, guard it perfectly, and throw your whole Hand of spells at it.

## lightning.charge
**What it does.** Perfect guards store up charges, up to three. Each blow you land uses one up for double damage and double stagger.

**When it happens.** A perfect guard (press the guard button just before a blow lands) stores one. Your next weapon blow that lands uses it up, and the next charge is ready for the blow after that.

**The numbers.** 3 Shards. Up to three charges, shown on the screen. A charged blow deals twice its damage and twice its stagger.

**Works with.** Ring of Drums and Thunderclap's Master mark on the same perfect guard; a heavy blow that also sets off Static; Single Stroke.

**Example.** Perfect-guard three blows of a duellist's string of attacks, then answer with three double-strength blows.

## lightning.base
**What it does.** Thunderclap, Lightning's own spell. It comes with the element; you never buy it. A bolt falls on the enemy you face and splits off to two more.

**When it happens.** Cast it from a spell slot. Press that slot's button. While you carry Lightning, you can always put it in a slot.

**The numbers.** Comes with Lightning. It costs no mana. After you cast it, you must wait 9 seconds to cast it again (its Empower makes it 7). It reaches up to 10 metres and deals 16 damage and 45 stagger, splitting off to two more enemies. It doesn't add any Static of its own.

**Works with.** Static (a heavy blow right after sets off whatever Static is there); its Master mark on a perfect guard; Stored Charge.

**Not built yet.** The Empower's stronger clap (55 stagger). For now, the spell only gets the Empower's shorter wait.

**Example.** A shield bearer won't open up. The clap breaks its balance, and your next blow lands.

## lightning.base.shape
**What it does.** Unlocks two shapes for Thunderclap. You pick one at any rest fire. **Rolling Thunder**: three claps march out in a line. **Held Thunder**: one huge clap with no splitting.

**When it happens.** Whenever you cast Thunderclap, using the shape you picked.

**The numbers.** 3 Shards. Rolling Thunder: three claps, 3 metres apart, over 9 metres. Each deals 16 damage, 30 stagger and a Static. Held Thunder: one bolt on the enemy you face within 10 metres, for 20 damage and 90 stagger, with no splitting.

**Works with.** Static and Chain Lance (Rolling Thunder down a line); Stored Charge (Held Thunder, then a double-strength blow).

**Not built yet.** Held Thunder should be held on its button and grow stronger the longer you hold. For now, it is always at full strength.

**Example.** Held Thunder on the captain staggers it right away.

## lightning.base.master
**What it does.** A perfect guard makes your next Thunderclap instant and ends its wait.

**When it happens.** On a perfect guard. Thunderclap must be in a spell slot.

**The numbers.** 3 Shards. Its wait ends right away, and your next cast doesn't start a new one.

**Works with.** Stored Charge and Ring of Drums on the same guard; In One Breath.

**Not built yet.** The cast should also skip its wind-up.

**Example.** Guard the thrust perfectly and clap right away: guard, clap, guard, clap.

## lightning.drums
**What it does.** Three drums made of cloud and lightning spin behind your shoulders. Each perfect guard or perfect dodge beats one, and a bolt falls on the attacker.

**When it happens.** Cast it from a spell slot. Press that slot's button. Then it works on each perfect guard or perfect dodge until all three drums are beaten.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again (its Empower makes it 8). There are three beats. Each is a bolt within 1.2 metres of the enemy, for 18 damage, 35 stagger and two Static. The screen shows how many drums are left.

**Works with.** Stored Charge, which it comes after on the chart; Thunderclap's Master mark; In One Breath.

**Not built yet.** The ring should last 8 seconds (for now, the drums wait until they are beaten). The third beat should be a huge clap within 4 metres (30 damage, 60 stagger) that sets off all the Static in it.

**Example.** Cast it before the duellist's string of attacks. Three perfect guards drop three bolts on it.

## lightning.drums.shape
**What it does.** Unlocks two shapes for Ring of Drums. You pick one at any rest fire. **Drum Roll**: normal guards beat them too, for a small clap. **Silent Drum**: the beats are saved up, and your next heavy blow carries them.

**When it happens.** While Ring of Drums is in a spell slot: Drum Roll on every normal guard, Silent Drum on a heavy blow.

**The numbers.** 3 Shards. Drum Roll: a clap within 1.5 metres of the attacker, for 8 damage, 15 stagger and a Static, at most once every half second. Silent Drum: a heavy blow hits within 1.2 metres of its target for 18 more damage, 40 stagger and two Static, once every 3 seconds.

**Works with.** Stored Charge (Silent Drum on the double-strength heavy blow).

**Not built yet.** In Drum Roll, a perfect guard should beat two drums at once. In Silent Drum, nothing should fall on a beat, and the heavy blow should carry exactly the beats you saved.

**Example.** Drum Roll is for a player who guards more than they dodge.

## lightning.drums.master
**What it does.** Beat all three drums within three seconds, and the ring stays for another round.

**When it happens.** For now: a perfect dodge beats one extra drum. Ring of Drums must be in a spell slot.

**The numbers.** 3 Shards. One more bolt, for 18 damage, 35 stagger and two Static, at most once every 9 seconds.

**Not built yet.** Beating all three drums within 3 seconds to keep the ring.

**Example.** Dodge, guard, guard: the ring goes around again.

## lightning.stroke
**What it does.** Static builds up to ten. Setting it off no longer jumps to anyone else. All of it lands on the enemy you hit, twice as hard.

**When it happens.** Always on once you learn it, while you carry Lightning.

**The numbers.** 6 Shards. Static goes up to ten stacks. Setting it off deals 14 damage and 10 stagger for each stack, to the enemy you hit only: 140 damage and 100 stagger at ten.

**Works with.** Static, Waiting Thunder, Ring of Drums (which it comes after on the chart), Stored Charge (a charged heavy blow that also sets off Static).

**Example.** Against the Gate Captain, keep cutting until it has ten Static, then land one heavy blow for 140.

## lightning.blink
**What it does.** Each dodge sends a line of lightning along its path that shocks whatever it crosses.

**When it happens.** Every dodge.

**The numbers.** 3 Shards. The shock has a 1.1-metre radius and travels with you for the length of the dodge. It hits each enemy once for 10 damage and 14 stagger.

**Works with.** Afterimage, Lightning Rod, Never There, Storm Step.

**Example.** Roll through a gap between two soldiers, and both are shocked as you pass.

## lightning.rod
**What it does.** You plant a sword point-up a little in front of you. For eight seconds, bolts fall again and again inside a circle around it, hitting whatever stands there and adding Static.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again (its Empower makes it 8, and the circle's radius 4.5 metres). It is planted 2 metres in front of you; the circle has a 3.5-metre radius. It hits 0.2 seconds after you plant it, then every 0.9 seconds for 8 seconds, for 12 damage, 12 stagger and one stack of Static each time.

**Works with.** Blink, which it comes after on the chart; Static (heavy blows on enemies inside set it off).

**Not built yet.** Pressing again while the rod stands should flash you to it, landing with a shock.

**Example.** Plant it in a doorway and fight at its edge. Everything that comes through arrives with Static already on it.

## lightning.rod.shape
**What it does.** Unlocks two shapes for Lightning Rod. You pick one at any rest fire. **Lodestone**: the rod pulls in every enemy nearby, farther for each Static they have, then sets all of them off. **Thrown Rod**: the rod is thrown far, into an enemy far away or into the ground.

**When it happens.** Whenever you cast Lightning Rod, using the shape you picked.

**The numbers.** 3 Shards. Lodestone: everything within 7 metres is pulled in, then a burst deals 14 damage and 30 stagger and sets off all their Static. Thrown Rod: it is planted up to 10 metres away.

**Works with.** Chain Lance and Waiting Thunder, to add the Static first (Lodestone).

**Not built yet.** Lodestone should pull toward the rod (for now, it pulls toward you). The flash to the rod, which is what makes Thrown Rod's reach worth it.

**Example.** Lance a pack, then use Lodestone. They come together and go off all at once.

## lightning.rod.master
**What it does.** Cast it right as a blow would land on you, and Lightning Rod counts as a perfect dodge, and flashing to the rod doesn't use it up.

**When it happens.** When you cast Lightning Rod. Lightning Rod must be in a spell slot.

**The numbers.** 3 Shards. For now, every cast gives you what a perfect dodge gives: 12 mana, and 1.5 seconds off the wait on every spell that is waiting.

**Not built yet.** The timing (casting it right as a blow would land), and the flash to the rod that would use it up.

**Example.** Plant the rod in the brute's face as it swings.

## lightning.afterimage
**What it does.** A dodge at the last moment leaves a copy of you behind that strikes where you stood a moment later.

**When it happens.** On a perfect dodge, which is when a blow passes through you at the very start of your roll.

**The numbers.** 3 Shards. The strike lands 0.3 seconds later where your dodge started. It has a 2.3-metre radius and deals 26 damage and 28 stagger.

**Works with.** Blink, and Seven Flashes and Never There, which come after it on the chart.

**Example.** A brute swings overhead. Roll through it at the last moment, and the copy left behind hits it as the blow lands.

## lightning.flashes
**What it does.** You vanish and cut from enemy to enemy in one moment, then stand again where you started, with your back to the fight. The cuts all land together once you are standing still, and any enemy left with full Static has it set off. Nothing can hit you while you are gone. If you are hit while standing still afterwards, the cuts land at half strength and no Static is set off.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards, 45 mana (its Empower makes it 35, with 11 damage per cut). Seven cuts among enemies within 8 metres (against one enemy, seven cuts from all sides), 0.1 seconds apart, then half a second standing still. Each cut deals 9 damage, 8 stagger and one stack of Static. An enemy with full Static has it set off for 7 damage and 5 stagger per stack within 4 metres.

**Works with.** Afterimage, which it comes after on the chart; Static; Never There.

**Example.** Against one duellist with three Static: seven cuts fill its Static, and it goes off as you turn around.

## lightning.flashes.shape
**What it does.** Unlocks two shapes for Seven Flashes. You pick one at any rest fire. **One Flash**: it only hits the enemy you face, setting off its Static again and again. **Left Behind**: you don't stand still for as long, and you end up in the middle of the crowd.

**When it happens.** Whenever you cast Seven Flashes, using the shape you picked.

**The numbers.** 3 Shards. One Flash: the cuts only reach 3 metres, and each one that lands on Static sets it off for 4 damage per stack, at most once every half second. Left Behind: you only stand still for a tenth of a second afterwards.

**Works with.** Static, Single Stroke (One Flash on the enemy with ten Static).

**Not built yet.** One Flash's seven cuts should be along one line as one blow. Left Behind should end behind the last enemy and leave a shape of light behind to be hit.

**Example.** Use One Flash on the duellist with five Static.

## lightning.flashes.master
**What it does.** Cast it within a third of a second of a perfect dodge, and you don't have to stand still afterwards: the cuts land right away.

**When it happens.** Not yet: see below.

**The numbers.** 3 Shards.

**Not built yet.** All of it. It needs a timing window for casting after a perfect dodge, and a version of Seven Flashes without the standing still. Buying it now does nothing.

## lightning.neverthere
**What it does.** Every dodge carries you farther, and a perfect dodge costs no stamina. The cost: blocking a blow with your guard costs twice the stamina.

**When it happens.** Always on once you learn it. The free dodge comes with each perfect dodge. The cost comes with each blow you block.

**The numbers.** 6 Shards. Dodges go 30% farther. A perfect dodge gives back 25 stamina, which is what a dodge normally costs. A blocked blow costs twice the stamina for each point of damage blocked.

**Works with.** Blink, Afterimage (which it comes after on the chart), Seven Flashes, Lightning Rod.

**Example.** Fight a fast duellist by dodging every swing at the last moment and never raising your guard.

## lightning.stormstep
**What it does.** While Overdrive lasts, every dodge costs no stamina and shocks whatever it passes through, whether you learned Blink or not. A shape of light is left behind where you stood.

**When it happens.** Every dodge while Overdrive is running. Light it with the surge button.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. Each dodge gives back all the stamina it cost. The shock is the same as Blink's: 10 damage and 14 stagger, with a 1.1-metre radius along the dodge.

**Works with.** Blink, Afterimage, Never There.

**Example.** Light Overdrive in a crowd and roll back and forth through them. You spend no stamina, and every pass shocks them.
