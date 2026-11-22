# Ice: detailed explanations

The long explanation the chart shows for each high-level mark of the Ice tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_a.md`, Ice): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing.

## ice.chill
**What it does.** Every weapon blow that lands chills the enemy, one stack at a time, up to five. Each stack slows it down. At five stacks it is Frozen: stuck in ice until it thaws, or until a heavy blow shatters it for a burst of damage.

**When it happens.** Always on once you learn it: every weapon blow that lands, light or heavy, while Ice is your first element (or you are all in on Ice). If Ice is your second element, your weapon adds your first element's affliction, and Chill comes from your Ice spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. One stack per blow, up to five, lasting 6 seconds. Each stack slows the enemy by 7%. At five stacks, it freezes for 2.5 seconds. Elites and gods thaw after 1 second, and some huge enemies can't be frozen. A heavy blow on a frozen enemy shatters it for 45 more damage, and the Chill ends. A frozen enemy can't be chilled any more.

**Works with.** Glaze (one more Chill per blow), Hoarfrost, Cold Read, Flying Shards, Brittle, All at Once, Cold Settles, Deep Winter, and Soaked, which spreads Chill to everyone within 4 metres.

**Example.** Four quick light blows slow a soldier to a crawl. The fifth freezes it, and a heavy blow breaks it for 45 more damage on top of the cut.

## ice.shards
**What it does.** A shattered enemy throws its ice all around. Everyone the shards reach is hit and gets two stacks of Chill, including the one that shattered.

**When it happens.** Each time a frozen enemy is shattered, by your heavy blow or anything else that shatters.

**The numbers.** 3 Shards. A burst with a 4.5-metre radius around the shattered enemy. It deals 14 damage, 10 stagger and two stacks of Chill to each one.

**Works with.** Chill, Brittle, All at Once, Eight Points and Hanging Ice, which come after it on the chart, Cold Settles, Icebreaker on the Glacier branch, and The World Holds Still, whose chain of shatters throws the shards again at each step.

**Example.** Shatter one frozen soldier in a pack of four. The other three get two stacks right away, three light blows away from freezing.

## ice.eight
**What it does.** Eight ice shards stand in a ring around the enemy you face for a moment, then fly in together. Each adds a stack of Chill, and as they close in they use up the enemy's Chill for more damage. This is the detonator of the Ice tree.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). If you aren't facing an enemy, the shards close in on a spot 6 metres ahead, and you still have to wait.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 11 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 9 seconds, and the shards close in after 0.9 seconds.) It targets an enemy within 6 metres. The ring has a 3.5-metre radius and closes after 1.2 seconds. Each shard deals 5 damage, 4 stagger and one stack of Chill. Each shard that lands on a chilled enemy then uses up its Chill in a small burst, for 5 damage and 2 stagger per stack.

**Works with.** Flying Shards, which it comes after on the chart; Chill and Cold Read, to add the stacks first; Hanging Ice; All at Once; Cold Settles, when a second element's affliction is on the enemy too.

**Not built yet.** The shards shattering a Frozen enemy. A spell's hit can't shatter yet, so a frozen enemy's Chill is used up like any other's.

**Example.** Give a brute four stacks of Chill with your blade, then close the ring on it. The first shard uses up all four for 25 damage on top of its own.

## ice.eight.shape
**What it does.** Unlocks two shapes for Eight Points. You pick one at any rest fire. **Wolf's Jaws**: two rows of four shards close in from the front and back in half a second, too fast to walk out of. It only hits one target. **Eight on Eight**: one shard comes down on each of up to eight enemies within 8 metres, each using up that enemy's Chill. It's for crowds.

**When it happens.** Whenever you cast Eight Points, using the shape you picked.

**The numbers.** 3 Shards. Wolf's Jaws: the ring has a 2-metre radius around the enemy and closes after 0.5 seconds. Eight on Eight: up to eight enemies within 8 metres are each hit 0.5 seconds after you cast it, for 8 damage, 10 stagger and a stack of Chill. Then their Chill is used up, like above.

**Works with.** Hanging Ice (Wolf's Jaws on its enemy while the icicle falls); Flying Shards and Cold Settles in a crowd (Eight on Eight).

**Not built yet.** Wolf's Jaws should be two rows closing from the front and back (for now, it is a tighter, faster ring). Eight on Eight's shards should fall from above (for now, they rise under each enemy as pillars).

**Example.** A duellist that can step out of the plain ring can't step out of Wolf's Jaws. Six soldiers with two stacks each: Eight on Eight uses up all of them at once.

## ice.eight.master
**What it does.** Land a heavy blow on the enemy as the shards close in, and every shard counts as a shatter.

**When it happens.** A heavy blow (a finisher counts) on an enemy within 0.3 seconds of an Eight Points shard landing on it. Eight Points must be in a spell slot.

**The numbers.** 3 Shards. The heavy blow hits once more, like a shatter does: 45 damage, 30 stagger and a stack of Chill within a metre.

**Works with.** Hanging Ice and Flying Shards; All at Once; the heavy weapons, whose heavy blows are hard to time because they're slow.

**Not built yet.** Every shard should count as a shatter, so you'd get eight shatters, each with Flying Shards. For now, the timed heavy blow hits once more, as one shatter.

**Example.** Start the heavy blow as the ring tightens. It lands with the shards, and the enemy takes a second break on top.

## ice.hanging
**What it does.** A huge icicle hangs over the enemy you face and follows it while its shadow grows. Then it falls as a heavy blow that chills what it hits, and leaves cracked ground that hurts for a moment. On a Frozen enemy, it shatters it.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). If you aren't facing an enemy, the spell is still used up and you have to wait.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 8 seconds, and makes it fall after 2.4 seconds.) It targets an enemy within 7 metres, or 7 metres ahead if there is none. It falls after 3 seconds, following its enemy until a moment before. The fall hits everything within 2 metres for 30 damage, 60 stagger and three stacks of Chill, as a heavy blow. The cracked ground has a 1.5-metre radius. Every half second for 2 seconds, it deals 2 damage and a stack of Chill.

**Works with.** Chill (freeze the enemy while you wait, and the icicle shatters it), Flying Shards, which it comes after on the chart, Eight Points, Brittle, All at Once.

**Not built yet.** Shattering a Frozen enemy for 90. For now, the fall shatters it like any heavy blow does, for 45.

**Example.** Hang it over a brute, and chill it to five with your blade while the icicle waits. The icicle comes down on a frozen body and breaks it.

## ice.hanging.shape
**What it does.** Unlocks two shapes for Hanging Ice. You pick one at any rest fire. **Icicles**: three smaller icicles fall a second apart along the enemy's path. It punishes enemies that run. **Held by a Thread**: it doesn't fall on its own. It hangs for up to eight seconds and falls with your next heavy blow on its enemy, landing together with the blow.

**When it happens.** Whenever you cast Hanging Ice, using the shape you picked.

**The numbers.** 3 Shards. Icicles: three falls, a second apart, each with a 1.5-metre radius. Each deals 12 damage, 25 stagger and a stack of Chill. Held by a Thread: a sign appears on an enemy within 7 metres for 8 seconds. Your next weapon blow on it brings down the full icicle, for 30 damage, 60 stagger and three Chill within 2 metres.

**Works with.** Chill and the heavy blow (with Held by a Thread, the icicle and the heavy blow land together); Flying Shards.

**Not built yet.** Only a heavy blow should bring Held by a Thread down, and you should see the icicle hanging. For now, any weapon blow on its enemy sets it off, and you only see a sign.

**Example.** An archer keeps running from you. Use Icicles, and every step it takes is under falling ice.

## ice.hanging.master
**What it does.** Let Hanging Ice land at the very moment its enemy freezes, and it shatters the enemy four times over, and you get its wait back.

**When it happens.** For now, when the fall itself freezes what it hits. Hanging Ice must be in a spell slot.

**The numbers.** 3 Shards. The spell's wait ends right away.

**Works with.** Chill and Cold Read, to get the enemy to two stacks before it lands; Eight Points; Rime Guard.

**Not built yet.** The timing (within 0.3 seconds of the enemy becoming Frozen, from anything), and shattering four times over. For now, a fall that brings its enemy to five stacks freezes it and gives the wait back.

**Example.** Give a brute two Chill, then the icicle adds its three. It freezes under the fall, and a second icicle is ready to hang over it.

## ice.lift
**What it does.** Cold Settles, Ice's reaction lift. Every reaction with Chill in it leaves frozen ground where it happened, and the enemy it hit is slowed.

**When it happens.** Whenever Chill reacts on an enemy with another affliction it has (Steam with Burn, Supercooled with Static, and the rest).

**The numbers.** 3 Shards. A patch of frozen ground with a 1.5-metre radius around the reaction, lasting 4 seconds. Once a second, it deals 1 damage and a stack of Chill. Everything the reaction hit moves at 60% of its speed for 2 seconds: the enemy where it happened and every enemy its blows reached (elites are slowed half as much; gods aren't slowed).

**Works with.** Carrying a second element: every one of Ice's ten reactions; Flying Shards, which it comes after on the chart; All at Once and Deep Winter, above it.

**Example.** Ice and Water on a soldier in a crowd. The reaction goes off, the floor freezes around it, and soldiers walking in get Chill.

## ice.allatonce
**What it does.** Shattering hits three times as hard. The cost: Chill no longer slows anyone. It still freezes at five stacks.

**When it happens.** Always on once you learn it.

**The numbers.** 6 Shards. A shatter deals 135 instead of 45. Chill no longer slows enemies at any number of stacks.

**Works with.** Eight Points, Hanging Ice, Flying Shards, Brittle, Cold Settles, Flash Freeze on the Glacier branch.

**Example.** Freeze an elite and shatter it with one heavy blow: 135 damage from the ice alone, on top of the blow.

## ice.devotion
**What it does.** Deep Winter, Ice's Devotion, for going all in on Ice only. Chill never fades, and lighting Glaze freezes every enemy within four metres at once. No Chill is ever wasted, so you can spread it over a whole room and come back for each enemy one at a time.

**When it happens.** Only while you carry Ice twice (all in), once you buy it. The freeze comes each time you light Glaze.

**The numbers.** 6 Shards. Chill no longer runs out on any enemy. It stays until a freeze thaws it away or a shatter breaks it. Lighting Glaze fills Chill all the way on everything within 4 metres of you, which freezes everything that can be frozen. Buying this also gives you the deep strike, Ice Through. Use it with L2 and R2 while you carry Ice alone or twice. The spike freezes the enemy right away if it has three or more Chill.

**Works with.** Every spell in the Ice tree; Splinter Ward and The World Holds Still, which also work with Glaze; Flying Shards, for a whole frozen room.

**Example.** Give four soldiers two stacks of Chill each, step into the middle, and light Glaze. All four are Frozen, and every heavy blow breaks one.

## ice.coldread
**What it does.** A perfect guard chills the enemy that hit you, two stacks at once.

**When it happens.** On a perfect guard: press the guard button just before a blow lands.

**The numbers.** 3 Shards. Two stacks of Chill on the attacker.

**Works with.** Chill (three blows after the guard freeze it), Rime Guard and Splinter Ward, which come after it on the chart, Flash Freeze on the Glacier branch.

**Example.** A soldier that already has three stacks swings at you. Guard it perfectly, and it freezes on the spot, ready for a heavy blow.

## ice.rime
**What it does.** You breathe out a low, thin, freezing fog around you. An enemy that attacks you gets two stacks of Chill, frost grows on its weapon arm, and its next wind-up is 50% slower. A perfect guard freezes the attacker right away.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). The answers come from the next blows aimed at you.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 9 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 7 seconds, and the fog's radius 4 metres.) The fog has a 3-metre radius around where you breathed it and lasts 6 seconds. It doesn't hurt anything itself. Each cast gives three answers. A blow that lands on you or on your guard gives its attacker two Chill. A perfect guard gives five, which freezes it. Every enemy standing in the fog winds up its attacks 50% slower while it stays there.

**Works with.** Cold Read, which it comes after on the chart (a perfect guard in the fog gives Cold Read's two and the freeze); Hanging Ice; Splinter Ward.

**Not built yet.** The answers should only work during the fog's six seconds (for now, the three answers wait for the next blows, fog or not, and those blows use them up). Frost on the attacker's weapon arm. Only the attacker's next wind-up should be slowed (for now, everything standing in the fog winds up slowly).

**Example.** Breathe the fog at a duellist and let it come. Its first thrust gives it two Chill, its next attack comes slowly enough to guard perfectly, and it freezes.

## ice.rime.shape
**What it does.** Unlocks two shapes for Rime Guard. You pick one at any rest fire. **Freezing Fog**: the fog stays where you breathed it, five metres wide, and everything in it gets a Chill every second. Use it to hold a doorway. **Breath on the Steel**: no fog. Instead, frost goes on your guard. Every blow you guard gives the attacker two Chill, and every perfect guard freezes it.

**When it happens.** Whenever you cast Rime Guard, using the shape you picked. Breath on the Steel's guard answers work while you have that shape picked.

**The numbers.** 3 Shards. Freezing Fog: a 2.5-metre radius, adding a stack of Chill to everything in it each second for 6 seconds. Breath on the Steel: two Chill for each blow you guard, and five (a freeze) for each perfect guard.

**Works with.** Cold Read and Splinter Ward (Breath on the Steel); Winter Wall and Hanging Ice, to keep enemies in the fog (Freezing Fog).

**Not built yet.** Breath on the Steel should only work for six seconds after you cast it. For now, every guard chills and every perfect guard freezes while you have the shape picked, whether you cast it or not.

**Example.** A hallway and a pack: put Freezing Fog in the doorway, and they reach you with three or four stacks.

## ice.rime.master
**What it does.** A perfect guard in the fog's first second freezes everything in the fog, not only the attacker.

**When it happens.** For now, any perfect guard while Rime Guard is in a spell slot, once every nine seconds.

**The numbers.** 3 Shards. Chill filled all the way on everything within 3 metres of you, which freezes everything that can be frozen.

**Works with.** Cold Read, Flying Shards (freeze a crowd, then break them one by one), All at Once.

**Not built yet.** It should only work in the fog's first second, and only on enemies standing in the fog. For now, it works on any perfect guard, fog or not, once every nine seconds.

**Example.** Three soldiers swing at once into the fresh fog. Guard the first one perfectly, and all three stand frozen around you.

## ice.splinter
**What it does.** When a ward blocks a blow, it bursts around you, hitting and chilling everything nearby. It also gives you what a perfect guard gives: its mana, and time off the wait on every spell.

**When it happens.** Each time a ward is used up: Glaze's ward, or any other.

**The numbers.** 3 Shards. A burst with a 3.5-metre radius around you. It deals 12 damage, 30 stagger and three stacks of Chill. You get 15 mana, and 2 seconds come off the wait on every spell that is waiting.

**Works with.** Glaze (one ward when you light it), Cold Read, which it comes after on the chart, Winter Wall and Sent Sliding above it, Deep Winter, Chill.

The blocked blow also counts as a perfect guard for every other mark that answers one (Cold Read, Rime Guard).

**Example.** Light Glaze and let a brute's red attack land on the ward. Everything around you gets three stacks and is staggered, and your spells are two seconds closer to ready.

## ice.wall
**What it does.** You raise a wall of ice in front of you. Nothing can walk through it, and blows from the other side can't reach you. When its time ends, or when you hit it with a heavy blow, it bursts and chills everything near it.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). A heavy blow swung close enough to reach it bursts it early.

**The numbers.** 4 Shards, 35 mana (its Empower makes it 30, and the burst 36). The wall rises 2 metres ahead of you, is 5 metres wide, and stands for 6 seconds. It has 60 health: enemies' blows on it wear it down, and if they break it, it bursts at once. Once a second, anything standing against either side of it (within 0.9 metres) gets a Chill. The burst reaches 4 metres around it and deals 30 damage, 50 stagger and three stacks of Chill. If your heavy blow bursts it, it counts as a heavy blow. Only one wall of ice can stand at a time.

**Works with.** Splinter Ward, which it comes after on the chart, Sent Sliding, Rime Guard's Freezing Fog shape, Chill.

**Not built yet.** The burst should be thrown forward as a 6-metre cone. For now, it bursts all around where the wall stands.

**Example.** Archers shoot at you across a yard. Raise the wall, wait behind it for the soldiers to come around, then cut it as they gather.

## ice.wall.shape
**What it does.** Unlocks two shapes for Winter Wall. You pick one at any rest fire. **Shield Wall**: a shorter wall that moves ahead of you at walking speed. Move forward behind it. **Cracked Wall**: it bursts on its own a moment after an enemy first hits it. It's a trap that answers the enemy, not you.

**When it happens.** Whenever you cast Winter Wall, using the shape you picked.

**The numbers.** 3 Shards. Shield Wall: 3 metres wide, standing for 4 seconds. Cracked Wall: the wall stands its full 6 seconds, but once an enemy first hits it, it bursts by itself 1.5 seconds later, with the Learn mark's burst.

**Works with.** Rime Guard and Hanging Ice behind a Shield Wall; Flying Shards at a Cracked Wall in a doorway.

**Not built yet.** Shield Wall should move with you (for now, it stands where it rose, just shorter).

**Example.** A god charges across the arena. Put a Cracked Wall in its path, and it runs into the burst.

## ice.wall.master
**What it does.** Burst Winter Wall with a heavy blow right after an enemy hits it, and the burst freezes everything it reaches.

**When it happens.** For now, every time the wall bursts. Winter Wall must be in a spell slot.

**The numbers.** 3 Shards. Chill filled all the way on everything the burst hits, which freezes everything that can be frozen.

**Works with.** Flying Shards and All at Once (a frozen crowd to break), Sent Sliding.

**Not built yet.** The timing (a heavy blow within 0.3 seconds of an enemy hitting the wall). For now, every burst freezes what it reaches.

**Example.** Soldiers are hacking at the wall. Cut it right as one of them hits it, and all of them stand frozen in its shards.

## ice.sliding
**What it does.** Your heavy blow no longer shatters a Frozen enemy where it stands. Instead, it sends it sliding in the direction of the blow as a block of ice, hitting everything it meets. It shatters at the end, or when it hits a wall. Freeze one, then bowl it into the rest.

**When it happens.** Always on once you learn it: each time a frozen enemy is shattered.

**The numbers.** 6 Shards. A path of ice 10 metres long and 2 metres wide runs on from the shattered enemy in the direction of the blow. Everything on it takes 30 damage, 30 stagger and two stacks of Chill. At most twice a second.

**Works with.** Flying Shards and Chill (each enemy hit is two stacks closer to freezing), Winter Wall, All at Once, The World Holds Still.

**Not built yet.** The heavy blow sending the frozen body sliding as a block, knocking aside what it meets, and shattering at the end or on a wall. For now, it shatters where it stands like before, and the path of ice runs on from it.

**Example.** Freeze the soldier at the front of a line and break it toward the rest. The ice runs through all of them.

## ice.brittle
**What it does.** When an enemy's ice is gone, from thawing or shattering, it stays brittle for a while. Your weapon cuts deeper into it, and its balance breaks sooner.

**When it happens.** Always on once you learn it, on every enemy that was Frozen and isn't anymore.

**The numbers.** 3 Shards. For 5 seconds after its ice is gone, the enemy takes 20% more damage from weapon blows and 20% more stagger from everything. Spells don't do extra damage.

**Works with.** Chill, Flying Shards, All at Once, Clean Break, which comes after it on the chart (and Ski Run after that), and the finisher: a shatter and a few brittle blows often stagger it.

**Example.** Shatter a brute, then keep cutting. For five seconds, every blow hits a fifth harder, and its balance bar fills fast.

## ice.base
**What it does.** Hoarfrost, Ice's own spell. It comes with the element; you never buy it. Ice crystals grow out along the ground in a fan, and the cold hits as they come. They stay for a while, and one blow of your weapon among them shatters them all.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Ice, you can always put it in a slot.

**The numbers.** Comes with Ice. It costs no mana. After you cast it, you must wait 7 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 5.5 seconds and the Chill three stacks. Thirteen crystals grow over a fan 4.5 metres long and 110 degrees wide. They deal 10 damage, 15 stagger and two stacks of Chill. They stay for 4 seconds. A weapon blow among them shatters them all, dealing 5 damage, 8 stagger and one more Chill to everything nearby.

**Works with.** Its Empower, its two shapes and its Master mark; Chill; Brittle; and the charged blade, which it charges with Ice when it lands.

**Example.** Cast it at three soldiers, then cut into the crystals. Every one of them has three stacks before your second blow.

## ice.base.shape
**What it does.** Unlocks two shapes for Hoarfrost. You pick one at any rest fire. **Black Ice**: no crystals. Instead, a sheet of clear ice covers the ground ahead and chills whatever stands on it. **Frost Line**: one branching line of frost creeps far along the floor, chilling everything it crosses. It can reach the archer behind the shield.

**When it happens.** Whenever you cast Hoarfrost, using the shape you picked. If you have not bought this, you cast the crystals.

**The numbers.** 3 Shards. Black Ice: a sheet with a 2.3-metre radius, 2.5 metres ahead of you, lasting 5 seconds. Each second, it deals 1 damage and a stack of Chill to whatever stands on it. Frost Line: a line 10 metres long and 1.4 metres wide that spreads out in a moment. It deals 8 damage, 10 stagger and two stacks of Chill.

**Works with.** Winter Wall and Rime Guard, to keep enemies on the ice (Black Ice); Eight Points and Hanging Ice on the archer it reaches (Frost Line).

**Not built yet.** Black Ice should make enemies on it get thrown 50% farther, and make a dodge on it slide twice as far.

**Example.** Put Black Ice down a hallway. The soldiers who walk in reach you with two or three stacks before you have swung.

## ice.base.master
**What it does.** Cast Hoarfrost within half a second after a shatter, and the crystals grow out from the shattered spot with four stacks of Chill.

**When it happens.** When you cast Hoarfrost within half a second after a shatter. Hoarfrost must be in a spell slot.

**The numbers.** 3 Shards. Everything that cast hits gets two more stacks of Chill: four in all.

**Works with.** Flying Shards (its burst is the hit that uses up the mark); Chill; All at Once.

**Not built yet.** The crystals should grow out from the shattered spot; for now, they come from you as usual.

**Example.** Shatter one soldier and cast right away into the rest. They get the crystals' Chill plus two more, one blow away from freezing.

## ice.ski
**What it does.** A path of clear ice shoots out in front of you, and you slide down it right away, throwing up snow, through whatever stands on it. Enemies on the path get two stacks of Chill, and the path stays.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). A wall, or anything too big to push, stops the slide.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 8 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 6 seconds, and the path 12 metres.) You slide 9 metres. Each enemy you pass takes 6 damage, 10 stagger and two stacks of Chill. The path stays for 5 seconds and deals 1 damage every half second to whatever stands on it. Nothing can hit you while you slide.

**Works with.** Clean Break, which it comes after on the chart; Brittle; Chill; Hanging Ice and Eight Points on the enemies you leave behind.

**Not built yet.** You should only be untouchable for the first 0.2 seconds (for now, it's the whole slide). A dodge along the path should carry you twice as far.

**Example.** You're surrounded. Slide out through the thinnest side. Everyone you pass gets two stacks, and the ice lies between you and them.

## ice.ski.shape
**What it does.** Unlocks two shapes for Ski Run. You pick one at any rest fire. **Thin Ice**: the path is thin lake ice over black water. Anyone who steps on it breaks through up to the knees and is stuck, and a heavy blow on them then freezes the water around them. **Shooting Run**: as you slide, you shoot three ice shards at the nearest enemy, the way the huntress shoots from her skis.

**When it happens.** Whenever you cast Ski Run, using the shape you picked.

**The numbers.** 3 Shards. Thin Ice: the path stays as thin ice for 5 seconds. Each enemy on it, one the slide passes or one that steps on it later, cracks through once: it is held for 1.2 seconds (less for elites) and gets a stack of Chill. A god is slowed by 30% for those 1.2 seconds instead of held. A heavy blow on a cracked-through enemy within those 1.2 seconds fills its Chill all the way, and it is Frozen. Shooting Run: three shards that chase their target, each dealing 8 damage, 6 stagger and a stack of Chill.

**Works with.** Brittle and Flying Shards (Thin Ice: a held enemy frozen and broken); Hanging Ice (Shooting Run builds up an archer's Chill).

**Not built yet.** Shooting Run's shards should be shot all through the slide (for now, all three go when it starts).

**Example.** Slide past two soldiers on Thin Ice and turn around. Both are stuck to the knees, and one heavy blow freezes the first.

## ice.ski.master
**What it does.** If you start Ski Run right as a blow would land on you, it counts as a perfect dodge. Also, slide through an enemy at the moment it shatters, and the path freezes everything standing on it.

**When it happens.** For now, every time you cast Ski Run, and every time you slide through a Frozen enemy. Ski Run must be in a spell slot.

**The numbers.** 3 Shards. It only counts if you start it in the last 0.3 seconds before an enemy's blow (one within 6 metres) lands on you. Then it is a perfect dodge: you get 12 mana, the game slows for a moment, the blow misses, and every mark that answers a perfect dodge answers this one. Sliding through a Frozen enemy sends a path 9 metres long and 2 metres wide on from it, filling Chill all the way on everything on it, at most once a second.

**Works with.** Rime Guard's and Cold Read's freezes, to slide through; Flying Shards; All at Once.

**Not built yet.** The moment of a shatter. For now, sliding through any Frozen enemy freezes the path past it.

**Example.** Freeze the front soldier, then slide through it toward the line behind. The line stands frozen behind you.

## ice.stillworld
**What it does.** Lighting Glaze raises a shell of ice around you. Press the surge button again while Glaze lasts, and the shell breaks outward as a ring that freezes everything it crosses, while the world slows down. Also, whenever you shatter a frozen enemy, a line of breaking ice runs to the nearest enemy that is still Frozen and shatters it too, which sends the line on again.

**When it happens.** The shell comes when you light the surge and stays while it lasts. The break comes when you press the surge button again, once each time you light it. For now, it works with whichever surge you light. The chain of shatters comes with every shatter, with or without Glaze.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. The ring spreads out to 9 metres in half a second. It deals 10 damage, 30 stagger and fills Chill all the way, so it freezes everything that can be frozen. Enemies slow to a fifth of their speed for 1.5 seconds. Glaze and its ward stay. The chain looks for a Frozen enemy within 7 metres of the one that shattered and hits it 0.2 seconds later for 20 damage and 30 stagger as a heavy blow, which shatters it for 45 more.

**Works with.** Glaze, Flying Shards (the shards fly at every step of the chain), All at Once, Brittle, Sent Sliding, Deep Winter.

**Example.** Light Glaze in a crowd, break the shell outward, and shatter the nearest enemy. The ice runs from one to the next until every frozen body has broken.

## ice.cleanbreak
**What it does.** Shatter an enemy that has less than a fifth of its health left, and nothing is left of it. Gods are safe from this; elites are not.

**When it happens.** Each time you shatter a frozen enemy, while you carry Ice.

**The numbers.** Major, 3 Shards. An enemy below 20% of its health when it is shattered dies a moment later. It never works on a god.

**Works with.** Brittle, which it comes after on the chart, and Ski Run, which comes after it. Every way to freeze and shatter: Hoarfrost, Hanging Ice, heavy blows on a frozen enemy, All at Once.

**Example.** A knight is frozen and down to a sliver of health. One heavy blow shatters it, and it's gone.
