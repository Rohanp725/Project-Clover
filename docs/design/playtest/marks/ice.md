# Ice: detailed explanations

The long explanation the chart shows for each high-level mark of the Ice tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Ice): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## ice.chill
**What it does.** Every blow of your weapon that lands chills the enemy, a stack at a time, up to five. Each stack slows it. At five it is Frozen: held still in ice until it thaws, or until a heavy blow shatters it for a burst of damage.

**When it happens.** Always on once learned: every weapon blow that lands, light or heavy, while Ice is your first element (or all in). With Ice second, your weapon lays the first element's affliction and Chill comes from your Ice spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. One stack a blow, five at most, lasting 6 seconds. Each stack slows the enemy by 7%. At five it freezes for 2.5 seconds; elites and gods thaw after 1 second, and some great forms cannot be frozen. A heavy blow on a frozen enemy shatters it for 45 more damage, and the Chill ends. Nothing chills a frozen enemy further.

**Works with.** Glaze (one more Chill a blow), Hoarfrost, Cold Read, Flying Shards, Brittle, All at Once, Cold Settles, Deep Winter, and Soaked, which carries Chill to everyone within 4 metres.

**Example.** Four quick light blows slow a soldier to a crawl; the fifth freezes it, and a heavy blow breaks it for 45 on top of the cut.

## ice.shards
**What it does.** A shattered enemy throws its ice wide. Everyone the shards reach is struck and chilled two deep, the one that shattered among them.

**When it happens.** Each time a frozen enemy is shattered, by your heavy blow or anything else that shatters.

**The numbers.** 3 Shards. A burst 4.5 metres round the shattered enemy: 14 damage, 10 stagger and two stacks of Chill to each.

**Works with.** Chill, Brittle, All at Once, Eight Points and Hanging Ice, which grow from it, Cold Settles, Icebreaker on the Glacier branch, and The World Holds Still, whose chain of shatters throws the shards again at each link.

**Example.** Shatter one frozen soldier in a pack of four: the other three are two deep at once, three light blows from freezing.

## ice.eight
**What it does.** Eight shards stand on a ring round the enemy you face for a breath, then fly in together. Each lays a stack of Chill, and as they close they spend the Chill it carries for more damage: the detonator of the tree.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). With no enemy faced the shards close on a spot 6 metres ahead, and the slot still waits.

**The numbers.** 4 Shards, no mana: it waits 11 seconds after a cast, less with weapon play (its Empower: 9, and the shards close after 0.9 seconds). An enemy within 6 metres. The ring is 3.5 metres round it and closes after 1.2 seconds. Each shard deals 5 damage, 4 stagger and one stack of Chill; each that lands on a chilled enemy then spends its Chill in a small burst, 5 damage and 2 stagger a stack.

**Works with.** Flying Shards, which it grows from; Chill and Cold Read to lay the stacks first; Hanging Ice; All at Once; Cold Settles when a second element's affliction is on the enemy too.

**Not built yet.** A Frozen enemy shattered by the shards: a spell's blow does not shatter, so a frozen enemy's Chill is spent like any other's.

**Example.** Chill a brute four deep with your blade, then close the ring on it: the first shard spends all four for 25 on top of its own.

## ice.eight.shape
**What it does.** Opens two shapes of Eight Points, chosen at any rest fire. **Wolf's Jaws**: two rows of four close from in front and behind in half a second, too fast to walk out of; one target only. **Eight on Eight**: one shard comes down on each of up to eight enemies within 8 metres, each spending that enemy's Chill; for crowds.

**When it happens.** Whenever you cast Eight Points, as the shape it wears.

**The numbers.** 3 Shards. Wolf's Jaws: the ring 2 metres round the enemy, closing after 0.5 seconds. Eight on Eight: up to eight enemies within 8 metres, each struck 0.5 seconds after the cast for 8 damage, 10 stagger and a stack of Chill, then its Chill spent as above.

**Works with.** Hanging Ice (Wolf's Jaws on its enemy while it falls); Flying Shards and Cold Settles in a crowd (Eight on Eight).

**Not built yet.** Wolf's Jaws as two rows closing from front and back (for now a tighter, faster ring), and Eight on Eight's shards falling from above (for now they rise under each enemy as pillars).

**Example.** A duellist that steps out of the plain ring cannot step out of Wolf's Jaws. Six soldiers two deep: Eight on Eight spends all of them at once.

## ice.eight.master
**What it does.** Land a heavy blow on the enemy as the shards close, and every shard counts as a shatter.

**When it happens.** A heavy blow (a finisher counts) on an enemy within 0.3 seconds of an Eight Points shard landing on it. Eight Points must be in a slot.

**The numbers.** 3 Shards. The heavy blow strikes once more as a shatter does: 45 damage, 30 stagger and a stack of Chill within a metre.

**Works with.** Hanging Ice and Flying Shards; All at Once; the heavy weapons, whose heavy is slow to time.

**Not built yet.** Every shard counting as a shatter, eight shatters with Flying Shards from each: for now the timed heavy strikes once more, as one shatter.

**Example.** Start the heavy as the ring tightens: it lands with the shards, and the enemy takes a second break on top.

## ice.hanging
**What it does.** A great icicle hangs over the enemy you face and follows it while its shadow grows. Then it falls as a heavy blow that chills what it strikes, and leaves cracked ground that bites for a moment. On a Frozen enemy it shatters it.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). With no enemy faced the slot is still spent and waits.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 8, and it falls after 2.4 seconds). An enemy within 7 metres; with none, 7 metres ahead. It falls after 3 seconds, following its enemy until a moment before. The fall strikes everything within 2 metres for 30 damage, 60 stagger and three stacks of Chill, as a heavy blow. The crack, 1.5 metres round, bites every half second for 2 seconds for 2 damage and a stack of Chill.

**Works with.** Chill (freeze it during the wait and the icicle shatters it), Flying Shards, which it grows from, Eight Points, Brittle, All at Once.

**Not built yet.** The shatter of a Frozen enemy for 90: for now the fall shatters it as any heavy blow does, for 45.

**Example.** Hang it over a brute and chill it to five with your blade as it falls: the icicle comes down on a frozen body and breaks it.

## ice.hanging.shape
**What it does.** Opens two shapes of Hanging Ice, chosen at any rest fire. **Icicles**: three smaller icicles fall a second apart along where the enemy moves; it punishes running. **Held by a Thread**: it does not fall by itself, but hangs up to eight seconds and falls with your next heavy blow on its enemy, woven into the heavy.

**When it happens.** Whenever you cast Hanging Ice, as the shape it wears.

**The numbers.** 3 Shards. Icicles: three falls a second apart, each 1.5 metres round, 12 damage, 25 stagger and a stack of Chill. Held by a Thread: a sign on the enemy within 7 metres for 8 seconds; the next weapon blow on it brings down the full icicle, 30 damage, 60 stagger and three Chill in 2 metres.

**Works with.** Chill and the heavy blow (Held by a Thread: the icicle and the heavy land together); Flying Shards.

**Not built yet.** Only a heavy blow bringing Held by a Thread down, and the icicle seen hanging (for now any weapon blow on its enemy sets it off, as a sign).

**Example.** An archer that keeps running from you: Icicles, and every step it takes is under ice.

## ice.hanging.master
**What it does.** Let Hanging Ice land in the moment its enemy freezes, and it shatters it four times over and its cooldown comes back.

**When it happens.** For now, when the fall itself freezes what it strikes. Hanging Ice must be in a slot.

**The numbers.** 3 Shards. The slot's wait is ended at once.

**Works with.** Chill and Cold Read to bring the enemy to two before it lands; Eight Points; Rime Guard.

**Not built yet.** The timing (within 0.3 seconds of the enemy becoming Frozen, by any hand) and the shatter four times over; for now a fall that brings its enemy to five freezes it and gives the cooldown back.

**Example.** Two Chill on a brute, then the icicle's three: it freezes under the fall, and a second icicle is ready to hang over it.

## ice.lift
**What it does.** Cold Settles, Ice's reaction lift: every reaction with Chill in it leaves frozen ground where it happened, and what it struck is slowed.

**When it happens.** Whenever Chill reacts on an enemy with the other affliction it carries (Steam with Burn, Supercooled with Static, and the rest).

**The numbers.** 3 Shards. A patch of frozen ground 1.5 metres round the reaction for 4 seconds, biting once a second for 1 damage and a stack of Chill. The enemy it happened on keeps 60% of its speed for 2 seconds (elites half the slowing, gods none).

**Works with.** Carrying a second element: every reaction of Ice's ten; Flying Shards, which it grows from; All at Once and Deep Winter above it.

**Not built yet.** The slow on everything the reaction struck: for now only the enemy it happened on.

**Example.** Ice and Water on a soldier in a crowd: the reaction goes off, the floor freezes round it, and the soldiers walking in take Chill.

## ice.allatonce
**What it does.** Shattering strikes three times as hard. The price: Chill no longer slows anyone. It still freezes at five.

**When it happens.** Always on once learned.

**The numbers.** 6 Shards. A shatter deals 135 where it dealt 45. Chill's slowing is gone, at every depth.

**Works with.** Eight Points, Hanging Ice, Flying Shards, Brittle, Cold Settles, Flash Freeze on the Glacier branch.

**Example.** Freeze an elite and shatter it with one heavy blow: 135 from the ice alone, on top of the blow.

## ice.devotion
**What it does.** Deep Winter, Ice's Devotion: all in on Ice only. Chill never fades, and lighting Glaze freezes every enemy within four metres at once. No Chill is ever wasted, so you can spread it over a whole room and come back for each in turn.

**When it happens.** Only while Ice is carried twice (all in), once bought. The freeze comes each time Glaze is lit.

**The numbers.** 6 Shards. Chill no longer runs down on any enemy: it stays until a freeze thaws it away or a shatter breaks it. Lighting Glaze lays Chill to full depth on everything within 4 metres of you, which freezes what can be frozen. Buying this also gives you the deep strike, Ice Through. Use it with L2 and R2 while you carry this element alone or twice: the spike freezes the enemy at once if it carries three Chill or more.

**Works with.** Every spell of the tree; Splinter Ward and The World Holds Still, which answer Glaze too; Flying Shards, for the room frozen at once.

**Example.** Chill four soldiers two deep each, step into the middle and light Glaze: all four are Frozen, and every heavy blow breaks one.

## ice.coldread
**What it does.** A perfect guard chills the one who struck, two stacks at once.

**When it happens.** A perfect guard: press the guard button just before a blow lands.

**The numbers.** 3 Shards. Two stacks of Chill on the attacker.

**Works with.** Chill (three blows after the guard freeze it), Rime Guard and Splinter Ward, which grow from it, Flash Freeze on the Glacier branch.

**Example.** A soldier already three deep swings at you: guard it perfectly and it freezes on the spot, ready for a heavy blow.

## ice.rime
**What it does.** You breathe out a low, thin freezing fog round you. An enemy that strikes at you is chilled two deep, rime grows on its weapon arm, and its next wind-up comes half again as slowly. A perfect guard freezes the attacker at once.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). The answers come from the next blows that come at you.

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 7, and the fog 4 metres round). The fog is 3 metres round where you breathed it and stands 6 seconds; it does no harm itself. Each cast holds three answers: a blow that lands on you or on your guard lays two Chill on its attacker, and a perfect guard lays five, which freezes it. Every enemy standing in the fog winds up its attacks half again as slowly while it stays there.

**Works with.** Cold Read, which it grows from (a perfect guard in the fog: Cold Read's two and the freeze); Hanging Ice; Splinter Ward.

**Not built yet.** The fog's six seconds as the window for the answers (for now the three answers wait for the next blows, fog or not, and are spent by them), the rime on the attacker's weapon arm, and only the attacker's next wind-up slowed (for now everything standing in the fog winds up slowly).

**Example.** Breathe the fog at a duellist and let it come: its first thrust lands you two Chill on it, its next comes slow enough to guard perfectly, and it freezes.

## ice.rime.shape
**What it does.** Opens two shapes of Rime Guard, chosen at any rest fire. **Freezing Fog**: the fog stays where it was breathed, five metres wide, and everything in it gains a Chill a second; hold a door with it. **Breath on the Steel**: no fog; the rime goes on your guard, and every blow guarded lays two Chill on the attacker and every perfect guard freezes it.

**When it happens.** Whenever you cast Rime Guard, as the shape it wears. Breath on the Steel's guard answers come while the shape is worn.

**The numbers.** 3 Shards. Freezing Fog: 2.5 metres round, a stack of Chill on all in it each second for 6 seconds. Breath on the Steel: two Chill for each blow guarded, five (a freeze) for each perfect guard.

**Works with.** Cold Read and Splinter Ward (Breath on the Steel); Winter Wall and Hanging Ice to keep them in the fog (Freezing Fog).

**Not built yet.** Breath on the Steel's six seconds after a cast: for now every guard chills and every perfect guard freezes while it is worn, cast or not. Freezing Fog empowered reaches 4 metres round, past its own 2.5 (an Empower's numbers are set after the shape's).

**Example.** A corridor and a pack: Freezing Fog in the doorway, and they arrive at you three or four deep.

## ice.rime.master
**What it does.** A perfect guard in the fog's first second freezes everything in the fog, not only the attacker.

**When it happens.** For now, any perfect guard while Rime Guard is in a slot, once in nine seconds.

**The numbers.** 3 Shards. Chill to full depth on everything within 3 metres of you, which freezes what can be frozen.

**Works with.** Cold Read, Flying Shards (a crowd frozen, then broken one by one), All at Once.

**Not built yet.** Only in the fog's first second, and only what stands in the fog; for now any perfect guard, fog or not, once in nine seconds.

**Example.** Three soldiers swing at once into the fresh fog: guard the first perfectly and all three stand frozen round you.

## ice.splinter
**What it does.** When a ward turns a blow, it bursts round you, striking and chilling everything near, and gives you what a perfect guard gives: its mana and its cut to every cooldown.

**When it happens.** Each time a ward is spent: Glaze's ward, or any other.

**The numbers.** 3 Shards. A burst 3.5 metres round you: 12 damage, 30 stagger and three stacks of Chill. 15 mana, and 2 seconds off every running cooldown.

**Works with.** Glaze (one ward when lit), Cold Read, which it grows from, Winter Wall and Sent Sliding above it, Deep Winter, Chill.

**Not built yet.** The turned blow counting as a perfect guard for other marks (Cold Read, Rime Guard): for now it gives a perfect guard's mana and cut alone.

**Example.** Light Glaze and let a brute's red attack land on the ward: everything round you is three deep and staggered, and your spells are two seconds nearer.

## ice.wall
**What it does.** You raise a wall of ice before you. Nothing walks through it and blows from its far side do not reach you. When its time ends, or when you strike it with a heavy blow, it bursts and chills everything near it.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). A heavy blow swung within reach of it bursts it early.

**The numbers.** 4 Shards, 35 mana (its Empower: 30, and a burst of 36). The wall rises 2 metres ahead, 5 metres wide, and stands 6 seconds. The burst reaches 4 metres round it: 30 damage, 50 stagger and three stacks of Chill; burst by your heavy blow, it lands as a heavy blow. One wall of ice stands at a time.

**Works with.** Splinter Ward, which it grows from, Sent Sliding, Rime Guard's Freezing Fog, Chill.

**Not built yet.** Its 60 health, the touch that chills, and the burst thrown forward as a cone of 6 metres: for now it chills only when it bursts, round where it stands.

**Example.** Archers loose at you across a yard: raise the wall, wait behind it for the soldiers to come round, then cut it as they gather.

## ice.wall.shape
**What it does.** Opens two shapes of Winter Wall, chosen at any rest fire. **Shield Wall**: a shorter wall that moves ahead of you at walking pace; advance behind it. **Cracked Wall**: it bursts by itself a moment after an enemy first strikes it, a trap that answers the enemy, not you.

**When it happens.** Whenever you cast Winter Wall, as the shape it wears.

**The numbers.** 3 Shards. Shield Wall: 3 metres wide, standing 4 seconds. Cracked Wall: it bursts by itself 3 seconds after it rises, with the Learn's burst.

**Works with.** Rime Guard and Hanging Ice behind a Shield Wall; Flying Shards at a Cracked Wall in a doorway.

**Not built yet.** Shield Wall moving with you (for now it stands where it rose, shorter), and Cracked Wall bursting 1.5 seconds after an enemy first strikes it (for now on a clock).

**Example.** A god's charge across the arena: a Cracked Wall in its road, and it runs into the burst.

## ice.wall.master
**What it does.** Burst Winter Wall with a heavy blow in the instant after an enemy strikes it, and the cone freezes everything it reaches.

**When it happens.** For now, every burst of the wall. Winter Wall must be in a slot.

**The numbers.** 3 Shards. Chill to full depth on everything the burst strikes, which freezes what can be frozen.

**Works with.** Flying Shards and All at Once (a frozen crowd to break), Sent Sliding.

**Not built yet.** The timing (a heavy blow within 0.3 seconds of an enemy striking the wall): for now every burst freezes what it reaches.

**Example.** Soldiers hacking at the wall: cut it as one strikes, and all of them stand frozen in its shards.

## ice.sliding
**What it does.** Your heavy blow no longer shatters a Frozen enemy where it stands. It sends it sliding along the blow as a block of ice, striking all it meets, and it shatters at the end or on a wall. Freeze one, then bowl it into the rest.

**When it happens.** Always on once learned: each time a frozen enemy is shattered.

**The numbers.** 6 Shards. A lane of ice 10 metres long and 2 wide runs on from the shattered enemy along the blow: everything on it takes 30 damage, 30 stagger and two stacks of Chill. At most twice a second.

**Works with.** Flying Shards and Chill (each one struck is two nearer its own freeze), Winter Wall, All at Once, The World Holds Still.

**Not built yet.** The heavy blow sending the frozen body sliding as a block, throwing aside what it meets and shattering at its end or on a wall: for now it shatters where it stands as before, and the lane of ice runs on from it.

**Example.** Freeze the soldier at the head of a line and break it toward the rest: the ice runs through all of them.

## ice.brittle
**What it does.** An enemy whose ice is gone, by a thaw or a shatter, stays brittle for a while. Your weapon cuts deeper into it and its balance breaks sooner.

**When it happens.** Always on once learned, on every enemy that has been Frozen and is no longer.

**The numbers.** 3 Shards. For 5 seconds after its ice is gone the enemy takes 20% more damage from weapon blows and 20% more stagger from everything. Spells do not cut deeper.

**Works with.** Chill, Flying Shards, All at Once, Ski Run, which grows from it, and the finisher: a shatter and a few brittle blows often stagger it.

**Example.** Shatter a brute, then keep cutting: for five seconds every blow bites a fifth harder and its balance bar fills fast.

## ice.base
**What it does.** Hoarfrost, Ice's own spell, comes with the element and is never bought. Crystals grow out along the ground in a fan and the cold strikes as they come. They stand a while, and a blow of your weapon among them shatters them all.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Ice, it is always yours to put in a slot.

**The numbers.** Given with Ice. No mana: it waits 7 seconds after a cast, less with weapon play; its Empower makes that 5.5. Thirteen crystals over a fan 4.5 metres long and 110 degrees wide: 10 damage, 15 stagger and two stacks of Chill. They stand 4 seconds; a weapon blow among them shatters them all, 5 damage, 8 stagger and one more Chill to everything near.

**Works with.** Its Empower, its two shapes and its Master; Chill; Brittle; the charged blade, which its landing charges with Ice.

**Not built yet.** The Empower's third stack of Chill: the base spell is cast by Ice's kit, which takes only the Empower's shorter wait.

**Example.** Cast it at three soldiers, then cut into the crystals: every one of them is three deep before your second blow.

## ice.base.shape
**What it does.** Opens two shapes of Hoarfrost, chosen at any rest fire. **Black Ice**: no crystals; a sheet of clear ice covers the ground ahead and chills what stands on it. **Frost Line**: one branching line of frost creeps far along the floor, chilling all it crosses; it reaches the archer behind the shield.

**When it happens.** Whenever you cast Hoarfrost, as the shape it wears. Before this is bought, the crystals are cast.

**The numbers.** 3 Shards. Black Ice: a sheet 2.3 metres round, 2.5 metres ahead, for 5 seconds; each second it bites what stands on it for 1 damage and a stack of Chill. Frost Line: a line 10 metres long, 1.4 metres wide, run out in a moment: 8 damage, 10 stagger and two stacks of Chill.

**Works with.** Winter Wall and Rime Guard to keep enemies on the ice (Black Ice); Eight Points and Hanging Ice on the archer it reaches (Frost Line).

**Not built yet.** Black Ice throwing enemies on it half again as far and a dodge on it sliding twice as far.

**Example.** Black Ice down a corridor: the soldiers who walk in arrive two or three deep before you have swung.

## ice.base.master
**What it does.** Cast Hoarfrost within half a second after a shatter, and the crystals grow out from the shattered spot, Chill four deep.

**When it happens.** For now, in the moment after any shatter. Hoarfrost must be in a slot.

**The numbers.** 3 Shards. For 0.8 seconds after a shatter, every enemy within 6 metres of it is marked; the next blow or spell on each lays two more stacks of Chill.

**Works with.** Flying Shards (its burst is the blow that spends the mark); Chill; All at Once.

**Not built yet.** Only Hoarfrost cast in the half second after a shatter, its crystals grown from the shattered spot: for now anything that strikes them in that moment lays the two more.

**Example.** Shatter one soldier and cast at once into the rest: they take the crystals' Chill and two more, one blow from freezing.

## ice.ski
**What it does.** A lane of clear ice runs out ahead and you slide down it at once, throwing snow, through whatever stands on it. Enemies on the lane are chilled two deep, and the lane stays.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). A wall, or anything too big to be shoved, stops the slide.

**The numbers.** 4 Shards, no mana: it waits 8 seconds after a cast, less with weapon play (its Empower: 6, and a lane of 12 metres). You go 9 metres; each enemy passed takes 6 damage, 10 stagger and two stacks of Chill. The lane stays 5 seconds and bites what stands on it for 1 damage every half second. Nothing can strike you while you slide.

**Works with.** Brittle, which it grows from; Chill; Hanging Ice and Eight Points on those you leave behind.

**Not built yet.** The untouchable time only for the first 0.2 seconds (for now the whole slide), and a dodge along the lane carrying twice as far.

**Example.** Ringed in, slide out through the thinnest side: everyone you pass is two deep and the ice lies between you.

## ice.ski.shape
**What it does.** Opens two shapes of Ski Run, chosen at any rest fire. **Thin Ice**: the lane is lake ice over black water; whoever steps onto it cracks through to the knees and is held, and a heavy blow on it then freezes the water round it. **Shooting Run**: as you slide you loose three shards of ice at the nearest enemy, as she shoots from her skis.

**When it happens.** Whenever you cast Ski Run, as the shape it wears.

**The numbers.** 3 Shards. Thin Ice: each enemy the slide passes is held for 1.2 seconds (elites less) and is three deep in Chill; a heavy blow on it within those 1.2 seconds lays Chill to full depth, and it is Frozen. Shooting Run: three homing shards, each 8 damage, 6 stagger and a stack of Chill.

**Works with.** Brittle and Flying Shards (Thin Ice: a held enemy frozen and broken); Hanging Ice (Shooting Run brings an archer's Chill up).

**Not built yet.** Thin Ice taking whoever steps onto the lane within 5 seconds after (for now those the slide passes), and a god slowed by 30% rather than held; Shooting Run's shards loosed through the slide (for now all three as it starts).

**Example.** Slide past two soldiers on Thin Ice and turn: both are held to the knees, and one heavy blow freezes the first.

## ice.ski.master
**What it does.** Started in the instant a blow would land on you, Ski Run counts as a perfect dodge. And slide through an enemy in the moment it shatters, and the lane freezes everything standing on it.

**When it happens.** For now, every cast of Ski Run, and every slide through a Frozen enemy. Ski Run must be in a slot.

**The numbers.** 3 Shards. What a perfect dodge gives: 12 mana and 1.5 seconds off every running cooldown. Sliding through a Frozen enemy runs a lane 9 metres on from it, 2 metres wide, laying Chill to full depth on all on it, once a second at most.

**Works with.** Rime Guard's and Cold Read's freezes to slide through; Flying Shards; All at Once.

**Not built yet.** The timing (started as a blow would land), and the moment of a shatter: for now every cast gives the dodge's mana and cuts, and any Frozen enemy slid through freezes the lane on from it.

**Example.** Freeze the front soldier, slide through it at the line behind: the line stands frozen in your wake.

## ice.stillworld
**What it does.** Lighting Glaze raises a shell of ice round you. Press the surge button again while Glaze lasts and the shell breaks outward as a ring that freezes everything it crosses, while the world is slowed. And whenever you shatter a frozen enemy, a line of breaking ice runs to the nearest one still Frozen and shatters that one too, which sends the line on again.

**When it happens.** The shell comes when you light the surge, and stands while it lasts; the break comes when you press the surge button again, once each time you light it. As built it answers whichever surge you light. The chain of shatters comes with every shatter, Glaze or not.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. The ring runs out to 9 metres in half a second: 10 damage, 30 stagger and Chill to full depth, so it freezes what can be frozen. Enemies are slowed to a fifth of their speed for 1.5 seconds. Glaze and its ward stay. The chain looks for a Frozen enemy within 7 metres of the one that shattered and strikes it 0.2 seconds later for 20 damage and 30 stagger as a heavy blow, which shatters it for 45 more.

**Works with.** Glaze, Flying Shards (the shards fly at every link), All at Once, Brittle, Sent Sliding, Deep Winter.

**Example.** Light Glaze in a crowd, break it outward, and shatter the nearest: the ice runs from one to the next until every frozen body has broken.

## ice.cleanbreak
**What it does.** Shatter an enemy that has less than a fifth of its health left, and nothing is left of it. Gods are spared; elites are not.

**When it happens.** Each time you shatter a frozen enemy, while Ice is carried.

**The numbers.** Major, 3 Shards. An enemy below 20% of its health when it is shattered dies a moment later. It never works on a god.

**Works with.** Brittle, which it grows from. Every way to freeze and shatter: Hoarfrost, Hanging Ice, heavy blows on a frozen enemy, All at Once.

**Example.** A knight is down to a sliver of health and frozen. One heavy blow shatters it, and it is gone.
