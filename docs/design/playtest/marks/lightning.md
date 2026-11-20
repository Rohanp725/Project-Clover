# Lightning: detailed explanations

The long explanation the chart shows for each high-level mark of the Lightning tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Lightning): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## lightning.static
**What it does.** Light blows build Static on an enemy, a stack at a time, up to five. Static does nothing by itself: a heavy blow or a finisher discharges it all at once, into the one struck and everyone standing near it.

**When it happens.** Always on once learned. While Lightning is your first element, every light blow that lands adds a stack. Carried as the second element, the weapon lays none; Static then comes from your Lightning spells, the charged blade and the R2 strike. Either way, a heavy blow or a finisher on an enemy carrying Static discharges it.

**The numbers.** 2 Shards. One stack a light blow, five at most, lasting 6 seconds. A discharge deals 7 damage and 5 stagger for each stack to everything within 4 metres: 35 damage and 25 stagger at five.

**Works with.** Single Stroke, Waiting Thunder, Chain Lance, Seven Flashes, Overdrive's quicker swings, and Arc Fire or Thunder Lift on the mixed branches.

**Example.** Four light blows on a soldier, then a heavy: 28 damage leaps out of it into the two beside it as well.

## lightning.lift
**What it does.** Strikes Twice: every reaction with Static in it strikes again half a second later where it happened, as lightning comes back down its own channel.

**When it happens.** Whenever Static reacts on an enemy with the other affliction it carries (Flashover with Burn and the rest).

**The numbers.** 3 Shards. Half a second after the reaction, a bolt in 2 metres round the enemy: 10 damage, 10 stagger and a stack of Static.

**Works with.** Carrying a second element: every reaction of Lightning's ten. Chain Lance and Waiting Thunder, which grow from it.

**Not built yet.** The reaction itself striking again at half strength: for now a bolt falls there.

**Example.** Burn and Static meet on a soldier: Flashover goes, and a breath later the bolt follows it down.

## lightning.lance
**What it does.** You throw a bolt ahead that leaps through up to five enemies, leaving Static on each.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 7 seconds after a cast (its Empower: 5.5, and six leaps). Reaches 12 metres to the first enemy, then leaps up to 5 metres from one to the next. 10 damage, 15 stagger and two stacks of Static to each.

**Works with.** Strikes Twice, which it grows from; Static; Waiting Thunder's heavy that discharges what it laid.

**Example.** Open on a pack of five archers from range: each takes two Static before they draw.

## lightning.lance.shape
**What it does.** Opens two shapes of Chain Lance, chosen at any rest fire. **Stepped Leader**: the lance is not thrown; a bolt steps out along the ground one bright step at a time, laying Static on what is near, then the return stroke lights the whole path at once. **Return Stroke**: after its last leap it comes back along the same path, striking each enemy again for half.

**When it happens.** Whenever you cast Chain Lance, as the shape it wears.

**The numbers.** 3 Shards. Stepped Leader: up to 12 metres, a step every 0.15 seconds, 4 damage and a Static on each step; 1.2 seconds after the cast the return stroke strikes the whole path, 14 damage, 10 stagger and a Static. Return Stroke: 5 damage and 5 stagger again on each enemy struck, 0.6 seconds later.

**Works with.** Static and Single Stroke (the return stroke's Static, then a heavy).

**Not built yet.** Steering Stepped Leader with the stick while you fight, the heavy blow calling the return stroke, and Return Stroke charging the blade for five blows.

**Example.** Stepped Leader down a corridor of soldiers: they all carry Static when the return stroke lights.

## lightning.lance.master
**What it does.** Thrown within half a second after a finisher, Chain Lance leaps to every enemy within twelve metres.

**When it happens.** A finisher lands. Chain Lance must be in a slot.

**The numbers.** 3 Shards. For now the finisher itself throws the arc: 10 damage, 10 stagger and two Static, leaping to eight enemies within 12 metres, once every 7 seconds.

**Works with.** Static, Waiting Thunder.

**Not built yet.** Only when the lance is thrown within half a second of the finisher.

**Example.** End a chain on the captain and every soldier in the yard carries Static.

## lightning.waiting
**What it does.** Five bolts hang at your shoulder, crackling. Each light blow you land sends one down onto the enemy struck. A heavy blow sends every one that is left. Those not sent when the time runs out go out.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Then your blows send the bolts.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast (its Empower: 8, and seven bolts). Five bolts, for 10 seconds. Each deals 8 damage and 6 stagger and lays one stack of Static.

**Works with.** Strikes Twice, which it grows from; Static (the bolts build it as you cut); Single Stroke.

**Example.** Cast, land two light blows, then a heavy: the last three bolts fall with it and the heavy discharges what they laid.

## lightning.waiting.shape
**What it does.** Opens two shapes of Waiting Thunder, chosen at any rest fire. **Hanging Cloud**: the bolts hang in a small black cloud over the enemy faced and fall on it while you fight. **Hammer Fall**: light blows send nothing; your heavy blow brings all of it as one bolt.

**When it happens.** Whenever you cast Waiting Thunder, as the shape it wears.

**The numbers.** 3 Shards. Hanging Cloud: over the enemy faced within 8 metres, a bolt every 0.9 seconds in 1.5 metres for 10 seconds, 8 damage, 6 stagger and a Static each. Hammer Fall: the enemy faced is marked for 10 seconds, and your next blow on it brings one bolt in 3 metres, 40 damage, 60 stagger and two Static.

**Works with.** Static and Single Stroke (Hammer Fall into a heavy that discharges).

**Not built yet.** Hanging Cloud's bolts falling as you strike anyone (for now they fall by themselves), and Hammer Fall waiting for a heavy blow only.

**Example.** Hang the cloud over the archer and fight the soldier beside you.

## lightning.waiting.master
**What it does.** Land the heavy blow within a second of the last bolt falling with a light blow, and all five come back for a second round.

**When it happens.** A heavy blow lands. Waiting Thunder must be in a slot.

**The numbers.** 3 Shards. For now every heavy blow brings five bolts down round the enemy struck, 8 damage, 6 stagger and a Static each, once every 10 seconds.

**Works with.** Static, Single Stroke.

**Not built yet.** Only within a second of the last bolt falling with a light blow.

**Example.** Light, light, light, light, light, heavy: and the sky goes round again.

## lightning.devotion
**What it does.** In One Breath, Lightning's Devotion: all in on Lightning only. After a perfect guard or a perfect dodge, your spells have no cooldown for a breath. Cast everything you hold in it; then the cooldowns start from full.

**When it happens.** Only while Lightning is carried twice (all in). Every perfect guard and perfect dodge.

**The numbers.** 6 Shards. For now a perfect guard or dodge ends every running cooldown at once. All in, L2 with R2 is the deep strike, Down the Channel.

**Works with.** Ring of Drums and Thunderclap's Master, which also live on the perfect guard.

**Not built yet.** No cooldown at all for 2.5 seconds after the perfect guard or dodge (the first rule switch, in the next task).

**Example.** Wait for the brute's swing, guard it perfectly, and spend the whole Hand on it.

## lightning.charge
**What it does.** Perfect guards bank charges, up to three. Each blow you land spends one for double damage and double stagger.

**When it happens.** A perfect guard (press the guard button just before a blow lands) banks one; your next weapon blow that lands spends it, and the next charge is ready for the blow after.

**The numbers.** 3 Shards. Three charges at most, shown on the HUD. A charged blow deals twice its damage and twice its stagger.

**Works with.** Ring of Drums and Thunderclap's Master on the same perfect guard; a heavy blow that also discharges Static; Single Stroke.

**Example.** Perfect-guard three blows of a duellist's chain, then answer with three doubled blows.

## lightning.base
**What it does.** Thunderclap, Lightning's own spell, comes with the element and is never bought. A bolt falls on the enemy you face and forks to two more.

**When it happens.** From a spell slot: cast it with that slot's button. Carrying Lightning, it is always yours to put in a slot.

**The numbers.** Given with Lightning. No mana: it waits 9 seconds after a cast (its Empower: 7). Up to 10 metres: 16 damage and 45 stagger, forking to two more. It lays no Static of its own.

**Works with.** Static (a heavy right after discharges what is there); its Master on a perfect guard; Stored Charge.

**Not built yet.** The Empower's harder clap (55 stagger): the base spell is cast by Lightning's kit, which takes only the Empower's shorter wait.

**Example.** A shield-bearer will not open: the clap breaks its balance, and your next blow lands.

## lightning.base.shape
**What it does.** Opens two shapes of Thunderclap, chosen at any rest fire. **Rolling Thunder**: three claps march out in a line. **Held Thunder**: one huge clap with no forks.

**When it happens.** Whenever you cast Thunderclap, as the shape it wears.

**The numbers.** 3 Shards. Rolling Thunder: three claps 3 metres apart over 9 metres, 16 damage, 30 stagger and a Static each. Held Thunder: one bolt on the enemy faced within 10 metres, 20 damage and 90 stagger, no forks.

**Works with.** Static and Chain Lance (Rolling Thunder down a line); Stored Charge (Held Thunder, then a doubled blow).

**Not built yet.** Held Thunder held on its button and growing with the hold: for now it is always the full hold.

**Example.** Held Thunder on the captain staggers it outright.

## lightning.base.master
**What it does.** A perfect guard makes the next Thunderclap instant and ends its wait.

**When it happens.** A perfect guard. Thunderclap must be in a slot.

**The numbers.** 3 Shards. Its cooldown ends at once, and the next cast starts none.

**Works with.** Stored Charge and Ring of Drums on the same guard; In One Breath.

**Not built yet.** The cast also losing its wind-up.

**Example.** Guard the thrust perfectly and clap at once: guard, clap, guard, clap.

## lightning.drums
**What it does.** Three drums of cloud and lightning turn behind your shoulders. Each perfect guard or perfect dodge beats one, and a bolt falls on the attacker.

**When it happens.** From a spell slot: cast it with that slot's button, then each perfect guard or perfect dodge until the three drums are beaten.

**The numbers.** 4 Shards, no mana: it waits 10 seconds (its Empower: 8). Three beats, each a bolt 1.2 metres round the enemy: 18 damage, 35 stagger and two Static. The drums are shown on the HUD as a count.

**Works with.** Stored Charge, which it grows from; Thunderclap's Master; In One Breath.

**Not built yet.** The ring standing 8 seconds (for now the drums wait until beaten), and the third beat's great clap in 4 metres (30 damage, 60 stagger) that discharges everything in it.

**Example.** Cast it before the duellist's chain: three perfect guards, three bolts on it.

## lightning.drums.shape
**What it does.** Opens two shapes of Ring of Drums, chosen at any rest fire. **Drum Roll**: plain guards beat them too, for a small clap. **Silent Drum**: the beats are saved, and your next heavy blow carries them.

**When it happens.** While Ring of Drums is in a slot: Drum Roll on every plain guard, Silent Drum on a heavy blow.

**The numbers.** 3 Shards. Drum Roll: a clap 1.5 metres round the attacker, 8 damage, 15 stagger and a Static, at most every half second. Silent Drum: a heavy blow strikes 1.2 metres round its target for 18 more damage, 40 stagger and two Static, once every 3 seconds.

**Works with.** Stored Charge (Silent Drum on the doubled heavy).

**Not built yet.** A perfect guard beating two drums at once in Drum Roll; in Silent Drum, nothing falling on a beat and the heavy carrying exactly the beats banked.

**Example.** Drum Roll for a player who guards rather than dodges.

## lightning.drums.master
**What it does.** Beat all three within three seconds and the ring stays for another round.

**When it happens.** For now: a perfect dodge beats one more drum. Ring of Drums must be in a slot.

**The numbers.** 3 Shards. One more bolt, 18 damage, 35 stagger and two Static, at most every 9 seconds.

**Not built yet.** The three beats within 3 seconds keeping the ring.

**Example.** Dodge, guard, guard: the ring goes round again.

## lightning.stroke
**What it does.** Static builds to ten. A discharge no longer arcs to anyone else: all of it lands on the one struck, twice as hard.

**When it happens.** Always on once learned, while you hold Lightning.

**The numbers.** 6 Shards. Static up to ten stacks. A discharge deals 14 damage and 10 stagger for each stack to the one struck alone: 140 damage and 100 stagger at ten.

**Works with.** Static, Waiting Thunder, Ring of Drums (which it grows from), Stored Charge (a charged heavy blow that also discharges).

**Example.** Against the Gate Captain, keep cutting until it carries ten Static, then land one heavy blow for 140.

## lightning.blink
**What it does.** Each dodge sends a line of lightning down its path that shocks whatever it crosses.

**When it happens.** Every dodge.

**The numbers.** 3 Shards. The shock is 1.1 metres in radius and travels with you for the length of the dodge, striking each enemy once for 10 damage and 14 stagger.

**Works with.** Afterimage, Lightning Rod, Never There, Storm Step.

**Example.** Roll through a gap between two soldiers and both are shocked as you pass.

## lightning.rod
**What it does.** You plant a sword point-up a little ahead of you. For eight seconds bolts fall inside a circle round it, again and again, striking whatever stands there and laying Static.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast (its Empower: 8, and the circle reaches 4.5 metres). Planted 2 metres ahead; the circle is 3.5 metres in radius. It strikes 0.2 seconds after planting and then every 0.9 seconds for 8 seconds: 12 damage, 12 stagger and one stack of Static each time.

**Works with.** Blink, which it grows from; Static (heavy blows on enemies inside discharge it).

**Not built yet.** A second press while the rod stands stepping you to it in a flash, landing with a shock.

**Example.** Plant it in a doorway and fight on its edge: everything that comes through it arrives already carrying Static.

## lightning.rod.shape
**What it does.** Opens two shapes of Lightning Rod, chosen at any rest fire. **Lodestone**: the rod draws every enemy near in, further for each Static they carry, then discharges all of them. **Thrown Rod**: the rod is thrown far, into a distant enemy or the ground.

**When it happens.** Whenever you cast Lightning Rod, as the shape it wears.

**The numbers.** 3 Shards. Lodestone: everything within 7 metres drawn in, then a burst of 14 damage and 30 stagger that sets off all their Static. Thrown Rod: planted up to 10 metres away.

**Works with.** Chain Lance and Waiting Thunder to lay the Static first (Lodestone).

**Not built yet.** Lodestone pulling toward the rod (for now toward you), and the step to the rod that makes Thrown Rod worth its reach.

**Example.** Lance a pack, then Lodestone: they come together and go off at once.

## lightning.rod.master
**What it does.** Cast in the instant a blow would land on you, Lightning Rod counts as a perfect dodge, and the rod is not spent by the step.

**When it happens.** You cast Lightning Rod. Lightning Rod must be in a slot.

**The numbers.** 3 Shards. For now every cast gives what a perfect dodge gives: 12 mana and 1.5 seconds off every running cooldown.

**Not built yet.** The timing (cast as a blow would land) and the step that would spend the rod.

**Example.** Plant the rod in the brute's face as it swings.

## lightning.afterimage
**What it does.** A dodge at the last moment leaves a copy of you that strikes where you stood a moment later.

**When it happens.** A perfect dodge: a blow passes through the very start of your roll.

**The numbers.** 3 Shards. The strike lands 0.3 seconds later where your dodge began, 2.3 metres in radius: 26 damage and 28 stagger.

**Works with.** Blink, Seven Flashes and Never There, which grow from it.

**Example.** A brute's overhead: roll through it at the last moment, and the copy left behind hits it as the blow lands.

## lightning.flashes
**What it does.** You vanish and cut from enemy to enemy in one breath, then stand again where you began with your back to the fight. The cuts land together once you are still, and any enemy left at full Static discharges. You cannot be struck while gone; if you are struck while standing still, the cuts land at half and nothing discharges.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, 45 mana (its Empower: 35, and 11 a cut). Seven cuts among enemies within 8 metres (against one enemy, seven from all sides), 0.1 seconds apart, then half a second still. Each cut deals 9 damage, 8 stagger and one stack of Static. A full enemy discharges for 7 damage and 5 stagger a stack within 4 metres.

**Works with.** Afterimage, which it grows from; Static; Never There.

**Example.** Against one duellist at three Static: seven cuts bring it to full and it discharges as you turn.

## lightning.flashes.shape
**What it does.** Opens two shapes of Seven Flashes, chosen at any rest fire. **One Flash**: against the enemy faced only, its Static discharged again. **Left Behind**: you do not stand still so long, ending in the crowd.

**When it happens.** Whenever you cast Seven Flashes, as the shape it wears.

**The numbers.** 3 Shards. One Flash: the cuts reach only 3 metres, and each that lands on Static sets it off for 4 damage a stack, at most every half second. Left Behind: the still moment after is a tenth of a second.

**Works with.** Static, Single Stroke (One Flash on the one carrying ten).

**Not built yet.** One Flash's seven cuts on one line as one blow, and Left Behind ending behind the last enemy with a shape of light left to be struck.

**Example.** One Flash on the duellist who carries five Static.

## lightning.flashes.master
**What it does.** Cast within a third of a second of a perfect dodge, and you do not stand still afterwards: the cuts land at once.

**When it happens.** Not yet: see below.

**The numbers.** 3 Shards.

**Not built yet.** All of it: it needs a cast's window after a perfect dodge and a Seven Flashes without its still moment. Buying it now does nothing.

## lightning.neverthere
**What it does.** Every dodge carries you further and a perfect dodge costs nothing. The price: a blow held on your guard costs twice the stamina.

**When it happens.** Always on once learned. The free dodge comes with each perfect dodge; the price with each blow you block.

**The numbers.** 6 Shards. Dodges go 30% further. A perfect dodge gives back 25 stamina, the dodge's usual cost. A blocked blow costs twice the stamina for each point caught.

**Works with.** Blink, Afterimage (which it grows from), Seven Flashes, Lightning Rod.

**Example.** Fight a fast duellist by dodging every swing at the last moment and never raising your guard.

## lightning.stormstep
**What it does.** While Overdrive lasts, every dodge costs nothing and shocks what it passes through, whether or not you learned Blink. A shape of light is left where you stood.

**When it happens.** Every dodge while Overdrive, lit with the surge button, is running.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. Each dodge gives back its whole stamina cost. The shock is Blink's: 10 damage and 14 stagger, 1.1 metres in radius along the dodge.

**Works with.** Blink, Afterimage, Never There.

**Example.** Light Overdrive in a crowd and roll back and forth through them: no stamina spent, and every pass shocks.
