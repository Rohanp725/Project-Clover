# Light: detailed explanations

The long explanation the chart shows for each high-level mark of the Light tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Light): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Marked" is Light's affliction: a mark of light that makes
the next blows on that enemy critical.

## light.mark
**What it does.** The last blow of a chain of blows, and every perfect guard, leave a mark of light on the enemy: it is Marked. Your next three weapon blows on a Marked enemy are critical hits, and each gives more mana. Then the Mark is used up.

**When it happens.** Always on once you learn it. The last light blow of a chain, or the heavy blow that ends one, Marks the enemy it lands on. A perfect guard Marks the enemy that hit you. Like every root mark, your weapon only adds it while Light is your first element (or your only one). The perfect guard's Mark works either way. If Light is your second element, Light's Marks come from your spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. A Mark lasts 8 seconds or three weapon blows, whichever ends first. Each of those blows is a critical hit, dealing one and a half times its damage, and gives 2 more mana. Marking the enemy again gives you three fresh blows.

**Works with.** Glare and every Light spell (each one Marks), Weighed (Marks stack up to three), Just Cut and Unblemished (more critical hits), Condemned, Full Daylight (a Mark that blows don't use up), Clarity, and the charged blade, whose third Light blow throws a ray that Marks.

**Example.** Three light blows on a soldier: the third Marks it. A heavy blow and two more light blows after that all land as critical hits.

## light.weighed
**What it does.** Marks stack up, to three. When an affliction on an enemy fills all the way up (including a Mark at three), your critical hits hit harder for a while.

**When it happens.** Always on once you learn it. Stacking Marks needs Light as your first element; if Light is your second, a Mark stays at one. The stronger critical hits come each time any affliction you add fills up on an enemy.

**The numbers.** 3 Shards. Marks stack up to three, one more for each Mark added. When an affliction fills up, critical hits deal 1.8 times their damage instead of 1.5, for 6 seconds.

**Works with.** The root mark, Reckoning's Spoken Sentence shape (12 more for each stack, once built), Ring of Spears and Brimming (they add Marks fast), Condemned, which comes after it on the chart, Unblemished and Just Cut (more critical hits to use it on).

**Not built yet.** Each stack should make critical hits stronger on its own. For now, the stronger critical hits come when a Mark fills up.

**Example.** End three chains on a brute. It has three Marks, and for six seconds every critical hit does 1.8 times damage.

## light.reckoning
**What it does.** Reckoning, the detonator of the Light tree. You put a seal on the enemy you face: a ring like a clock face on the ground under it, with one hand that sweeps around it in six seconds. On the last tick, it breaks upward in a column of light. It deals 8 damage for each weapon blow you landed on that enemy in those six seconds, along with a big hit to its balance.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). If you aren't facing an enemy, nothing is sealed, and you still have to wait. Then fight that enemy for six seconds.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. It targets an enemy within 10 metres. The seal flashes, without hitting anything, each time you land a blow on the enemy. When the six seconds end, it deals 8 damage for each weapon blow that landed on the enemy during that time, plus 40 stagger. Its Empower makes it 10 for each blow, up to 180.

**Works with.** Fast weapons (more blows counted), the root mark and Just Cut (critical hits), Weighed, Condemned (the column's stagger plus a full Mark's), Ring of Spears (holds the enemy still while you cut), the charged blade.

**Not built yet.** The clock face isn't drawn on the ground yet.

**Example.** Seal a captain behind its guard, then cut it eight times in six seconds. As the seal breaks, the column deals 64 more damage and 40 stagger.

## light.reckoning.shape
**What it does.** Unlocks two shapes for Reckoning. You pick one at any rest fire. **Witnessed** (the sun god Surya, the eye that sees every deed): the seal counts the enemy's attacks, not yours. Every blow it lands on you or your guard is scratched in gold on the clock's edge, and on the last tick a column falls on it. Guard well, and the sun judges for you. **Spoken Sentence**: no clock. The seal waits for your next heavy blow on the sealed enemy. Then a blade of gold falls on every Marked enemy nearby. It hits many enemies, at the moment you choose.

**When it happens.** Whenever you cast Reckoning, using the shape you picked. If you have not bought this, you cast the plain seal.

**The numbers.** 3 Shards. Witnessed: when the six seconds end, a column falls on the sealed enemy within 1.2 metres, for 30 damage, 40 stagger and a Mark. Spoken Sentence: for 8 seconds, your next heavy blow on the sealed enemy makes a blade fall a moment later on every Marked enemy within 8 metres. Each blade deals 20 damage, plus 12 more for each stack of Mark, and 40 stagger within 1.2 metres, and uses up their Marks. With either shape, the plain seal's own count doesn't pay out.

**Works with.** Witnessed: a build that counters attacks, perfect guards, Standing Light, the root mark (perfect guards Mark the attacker). Spoken Sentence: Weighed (stacked Marks), Ring of Spears and Glare's Seven Horses shape (lots of Marked enemies at once), Brimming, heavy weapons.

**Not built yet.** Witnessed should count the enemy's blows on you, at 30 per blow, plus 15 more for each one you perfectly guarded. For now, the column is worth one blow.

**Example.** Use Spoken Sentence on a captain with three Marked soldiers around it. One heavy blow, and four blades fall.

## light.reckoning.master
**What it does.** A finisher on the sealed enemy pays out the count twice.

**When it happens.** While the seal is on its enemy, during the six seconds after you cast it, while Reckoning is in a spell slot.

**The numbers.** 3 Shards. For now, your finisher on the sealed enemy hits it again for 40 damage and 40 stagger, once per seal. Other heavy blows do nothing.

**Works with.** Heavy weapons, Condemned (the full Mark's stagger sets up the finisher), Ring of Spears (holds it still to be finished).

**Not built yet.** It should pay out the count a second time. For now, it is a flat 40.

**Example.** Stagger the sealed brute with a full Mark and finish it. The seal pays out right away, and again when it breaks.

## light.spears
**What it does.** Ring of Spears, the breaker. Six spears of gold hang in the air in a ring around the enemy you face. They wait a moment, then drive in together, Marking the enemy and holding it still.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). If you aren't facing an enemy, they close on a spot in front of you, and you still have to wait.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes it 40 stagger in total.) It finds an enemy within 10 metres. The ring has a 3-metre radius and closes after 0.6 seconds. Each spear deals 6 damage, 5 stagger and a Mark: 36 damage and 30 stagger in total. The enemy it hits is held still for 1.5 seconds (a god or an elite for 0.4 of that).

**Works with.** Weighed (each spear adds a Mark), Reckoning (holds the enemy still while the seal counts), Condemned, Spoken Sentence, and the charged blade, which it charges with Light.

**Example.** Cast it on a brute. Land your three critical hits while it is held, then a heavy blow while it is still Marked.

## light.spears.shape
**What it does.** Unlocks two shapes for Ring of Spears. You pick one at any rest fire. **The Drawn Line** (the line drawn around Sita's door that no one meaning harm could cross): the six spears drive into the floor in a row across the space in front of you and stay standing. An enemy that crosses it is held and Marked, and your blows across the line are critical hits. Fight across it. **In Turn**: the spears drive in one after another. A spear that lands at the same moment as one of your weapon blows hits for double and is a critical hit. Six beats to match.

**When it happens.** Whenever you cast Ring of Spears, using the shape you picked. If you have not bought this, you cast the plain ring.

**The numbers.** 3 Shards. The Drawn Line: for now, the spears still drive in, then stand in the floor for 5 seconds in their ring 3 metres around the enemy. There are six spears, each 0.7 metres across. Once a second, they Mark and jolt whatever stands at them for 12 stagger. In Turn: six blades fall on the enemy one after another, a quarter second apart, each for 6 damage, 5 stagger and a Mark. For a second and a half after that, each weapon blow on it hits for one more spear's worth, 6 damage and 5 stagger, at most four times a second.

**Works with.** The Drawn Line: a doorway or a staircase, ranged weapons and spears that reach across it, the root mark (Marked enemies take critical hits). In Turn: fast weapons, Just Cut, the charged blade.

**Not built yet.** The Drawn Line should be a straight line six metres across in front of you that holds anything crossing it for a second, without the spears driving into the enemy. In Turn should check for a spear landing within a tenth of a second of your blow.

**Example.** Put The Drawn Line across a hallway. The pack comes on, each one is Marked as it reaches the spears, and every cut you make across them is a critical hit.

## light.spears.master
**What it does.** Land a critical hit on the enemy while the spears hang over it, and they all drive in at once as critical hits, and Ring of Spears is ready again.

**When it happens.** During the 0.6 seconds the spears hang over the enemy you face, while Ring of Spears is in a spell slot.

**The numbers.** 3 Shards. For now, the first weapon blow on that enemy in those 0.6 seconds hits within 1.5 metres of it for 18 damage, 15 stagger and a Mark, and the wait ends right away.

**Works with.** A Marked enemy (its blows are critical hits), Just Cut, Unblemished, and fast weapons that can land a blow in that moment.

**Not built yet.** Only critical hits should count, and the spears themselves should drive in at once. For now, any weapon blow counts, and it makes a burst worth 50% more than the spears.

**Example.** Mark a duellist with a chain, cast, and cut it before the spears fall. They hit right away, and the ring is ready for the next one.

## light.condemned
**What it does.** When an affliction on an enemy fills all the way up, its balance takes a big hit. The cost: your weapon does less damage to an enemy with no afflictions, though blows and spells on any afflicted enemy hit a little harder.

**When it happens.** Always on once you learn it. The hit to balance comes each time any affliction you add fills up on an enemy, including a third Mark.

**The numbers.** 6 Shards. A burst 1.2 metres around the enemy deals no damage and 60 stagger, at most once every 4 seconds. Your weapon deals 15% less damage, but damage on an afflicted enemy is 15% higher, so against those the two almost cancel out.

**Works with.** Weighed, which it comes after on the chart, Ring of Spears, Glare and Brimming to fill up Marks, Reckoning (the column's stagger together with it), and a second element's affliction, which fills up on its own.

**Not built yet.** An enemy with three Marks and under a quarter of its health (a tenth for a god or an elite) should be finished off no matter how much balance it has. For now, the full Mark breaks much of its balance.

**Example.** Bring a brute to three Marks. The burst nearly staggers it, and a heavy blow finishes the job.

## light.justcut
**What it does.** A blow that lands may flash. For a moment after a flash, every blow you land is a critical hit. A blow that lands may also give you a little stamina back.

**When it happens.** Any weapon blow that lands, one time in four, at most once every 1.5 seconds. The flash and the stamina each have their own separate chance.

**The numbers.** 3 Shards. One chance in four of a flash: every blow is a critical hit for 0.8 seconds. One chance in four of getting 6 stamina back. Each can happen at most once every 1.5 seconds.

**Works with.** Weighed (stronger critical hits), Step Into Light and Standing Light, which come after it on the chart, Ring of Spears' Master mark, and fast weapons like the Twin Knives, which fit more blows into the moment.

**Not built yet.** The flash should come when you press the next blow of a chain at the exact moment the last one lands, making it a critical hit that costs no stamina. For now, it comes by chance.

**Example.** In a flurry of blows on a duellist, one cut flashes, and the two after it are critical hits without any Mark.

## light.step
**What it does.** Step Into Light, the movement spell. You step through the enemy you face on a line of gold dust and come out on its far side, turned back toward it, cutting and Marking everything on the line. If the cut lands, your next heavy blow right after is a critical hit. Nothing can hit you during the step. A wall, or an enemy too big to push past, stops you short.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). If you aren't facing an enemy, you step forward anyway, and you still have to wait.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 8 seconds to cast it again. Fighting with your weapon makes the wait shorter. (Its Empower makes the wait 7 seconds and the reach 9 metres.) It goes up to 7 metres: to 1.8 metres past the enemy you face, or about 5 metres ahead if there is none. The cut deals 10 damage, 10 stagger and a Mark to everything on the line. The heavy blow after it is a critical hit if it lands within 1.5 seconds.

**Works with.** The root mark (the Mark it adds makes the heavy blow and two more critical hits), Just Cut, Weighed, and the charged blade: the cut is a spell, so it charges your weapon with Light for the next three moves.

**Example.** Step through an archer behind its shield bearer, turn around, and land a critical heavy blow on its back.

## light.step.shape
**What it does.** Unlocks two shapes for Step Into Light. You pick one at any rest fire. **Aruna's Road** (named for the driver of the sun's chariot at dawn): the line you stepped along stays for a while as a road of gold dust that Marks whatever crosses it. Pressing again steps you back along it to where you started. Out and back. **Behind the Eye**: you step to behind the enemy, landing at its back, and your first blow after that deals a finisher's damage.

**When it happens.** Whenever you cast Step Into Light, using the shape you picked. If you have not bought this, you cast the plain step.

**The numbers.** 3 Shards. Aruna's Road: five patches along 7 metres in front of where you cast it, each 0.7 metres across, Mark whatever stands in them once a second for 3 seconds. Behind the Eye: if the cut lands, your next weapon blow within a second deals three times its damage.

**Works with.** Aruna's Road: a pack that follows you, the root mark, Weighed, Spoken Sentence. Behind the Eye: heavy weapons, the Learn mark's critical heavy blow (the two multiply together), Unblemished.

**Not built yet.** Aruna's Road's step back along the road, with a second press within the three seconds.

**Example.** Use Behind the Eye on a captain. Step, turn, and one heavy blow lands as a critical hit, three times as hard.

## light.step.master
**What it does.** Cast it through two enemies standing in a line. Both are cut and Marked, and Step Into Light is ready again. Light goes straight.

**When it happens.** As the step cuts, while Step Into Light is in a spell slot.

**The numbers.** 3 Shards. For now, a step that cuts anything ends its own wait, once every 16 seconds.

**Works with.** Hallways and staircases, a line of spearmen, Aruna's Road (a second road right away).

**Not built yet.** It should need one step to cut two enemies. For now, any step that cuts works, at most once every 16 seconds.

**Example.** Two soldiers come up a staircase one behind the other. Step through both, then step again right away.

## light.lantern
**What it does.** Standing Light, the counter. You plant a brass lamp a little in front of you. A beam of light shines over it, with dust swirling in it, and a glow surrounds it while it stands. Enemies in the glow are hit lightly and Marked again and again, and a perfect guard bursts around you.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). Then guard.

**The numbers.** 4 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. It is planted 2.5 metres in front of you. The glow has a 5-metre radius and lasts 10 seconds. It hits 0.2 seconds after you plant it, then every 2.5 seconds, for 2 damage and a Mark each time. Each lamp gives three perfect-guard bursts, shown next to the weapon as Lamp. Each burst deals 10 damage and 25 stagger within 3.5 metres of you.

**Works with.** The root mark (a fresh Mark, and three fresh critical hits, every 2.5 seconds), Unblemished, which comes after it on the chart, Clarity's longer perfect timing windows, Reckoning's Witnessed shape, Condemned.

**Not built yet.** The burst should only come from a perfect guard made inside the glow. For now, your next three perfect guards after each lamp burst, wherever you stand. The glow doesn't clear away enemy clouds, flames or frost yet. The Empower's longer perfect guard and dodge windows inside the glow; for now, the Empower does nothing.

**Example.** Plant it in a doorway and fight at its edge. Everything that comes through arrives Marked, and each perfect guard throws them back.

## light.lantern.shape
**What it does.** Unlocks two shapes for Standing Light. You pick one at any rest fire. **Row of Lamps** (the rows of lamps at Diwali): three small lamps are set in a line. Every enemy between two of them is Marked and kept clear of clouds, flames and frost. A perfect guard made in line with them shoots a ray down the row. Your guard fires the row. **Circled Lamp** (the lamp waved in a circle before the god): a smaller lamp circles you, Marking what it passes and burning away any danger zone you walk into. It goes where you go.

**When it happens.** Whenever you cast Standing Light, using the shape you picked. If you have not bought this, you plant the plain lamp.

**The numbers.** 3 Shards. Row of Lamps: the lamp still stands, and three patches, each a metre across, along 6 metres in front of you Mark whatever stands in them once a second for 10 seconds. Each perfect guard shoots a ray 6 metres past the attacker, for 12 damage, 25 stagger and a Mark, at most once a second. Circled Lamp: one lamp circles you 2 metres away for 8 seconds, hitting whatever it passes for 2 damage and Marking it.

**Works with.** Row of Lamps: a build that counters attacks, Unblemished, perfect guards. Circled Lamp: a fight on the move, fast weapons, Brimming.

**Not built yet.** Row of Lamps should only affect enemies between two lamps, only fire on a guard made in line with them, and show the three lamps, with nothing placed between them. Circled Lamp should burn away danger zones, and look like a lamp (for now, it is a disc of light).

**Example.** Use Row of Lamps across a hall. Stand at one end and perfect-guard the brute that comes down it, and the ray runs back through the pack behind it.

## light.lantern.master
**What it does.** The first perfect guard inside the glow makes every Light spell ready again, once per lamp.

**When it happens.** A perfect guard, while Standing Light is in a spell slot.

**The numbers.** 3 Shards. The waits on Glare, Step Into Light, Ring of Spears, Standing Light and Reckoning end right away, for any that are in a slot. Once every 12 seconds. Daybreak doesn't have a wait, so nothing changes for it.

**Works with.** A Hand full of Light spells, Unblemished, Just Cut, Clarity's longer perfect timing windows.

**Not built yet.** It should only work on a perfect guard made inside the glow. For now, any perfect guard works, once every 12 seconds.

**Example.** Cast everything, plant the lamp, perfect-guard one blow, and cast everything again.

## light.unblemished
**What it does.** Fight for a while without getting hurt, and every weapon blow you land is a critical hit and gives more mana. A blow that lands on you ends it, and until you go a while without getting hurt again, you take more damage from every blow.

**When it happens.** Always on once you learn it. You count as unhurt once no blow has landed on you for 8 seconds. The bonus lasts until the next blow lands.

**The numbers.** 6 Shards. While unhurt: every weapon blow is a critical hit, and blows give 25% more mana. After you get hurt, for 8 seconds or longer, you take 15% more damage. A blocked blow, a perfect guard or a perfect dodge doesn't count as getting hurt.

**Works with.** Standing Light, which it comes after on the chart, Weighed (critical hits 30% harder), Clarity's longer perfect timing windows, Just Cut. Every blow you block keeps you unhurt.

**Example.** Guard and dodge a duellist's whole string of blows without a scratch, and every answer you give it lands as a critical hit.

## light.brimming
**What it does.** While your mana is full, every heavy blow sends a wave of light ahead of it that hits and Marks everything in front of you. Mana you save for the big spells is useful while it waits.

**When it happens.** Each heavy blow or finisher that lands while your mana bar is full.

**The numbers.** 3 Shards. A cone 5 metres long and 70 degrees wide. It deals 12 damage, 12 stagger and a Mark.

**Works with.** Daybreak (the mana it saves up for), Seen Plainly, which comes after it on the chart, Weighed, Condemned, Spoken Sentence, and perfect guards to fill the bar. Spending mana on Daybreak, the surge or a deep strike stops it until the bar is full again.

**Example.** You have full mana in front of a pack. Every heavy blow Marks the three enemies in front of you.

## light.base
**What it does.** Glare, Light's own spell. It comes with the element; you never buy it. A narrow ray shoots from your hand in the direction you face, lighting up the dust along it, and hits and Marks whatever it crosses. The enemies it Marks show gold brackets.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4). While you carry Light, you can always put it in a slot.

**The numbers.** Comes with Light. It costs no mana. After you cast it, you must wait 6 seconds to cast it again. Fighting with your weapon makes the wait shorter. Its Empower makes the wait 5 seconds, the reach 7 metres and the damage 12. The ray is 5.5 metres long and about 1.4 metres wide. It deals 8 damage, 22 stagger and a Mark to each enemy on it.

**Works with.** Its Empower, its two shapes and its Master mark; the root mark, Weighed, Condemned, Spoken Sentence (Marked enemies for the blades), and the charged blade, which its ray charges with Light.

**Example.** Glare the soldier in front of the captain. It is Marked and staggering, and your next three cuts on it are critical hits.

## light.base.shape
**What it does.** Unlocks two shapes for Glare. You pick one at any rest fire. **Gathered**: hold the button, and the ray narrows, with the dust in it drawn to a point on the enemy. Let go for a ray that breaks balance much harder. It works like a magnifying glass, for breaking one enemy. **Seven Horses** (the sun god Surya's seven horses): the ray splits into seven thin rays in a fan. Each hits lightly, and every enemy touched is Marked. It's for Marking a crowd.

**When it happens.** Whenever you cast Glare, using the shape you picked. If you have not bought this, you cast the plain ray.

**The numbers.** 3 Shards. Gathered: hold the button for up to a second, then let go. The ray is 5.5 metres long and narrow. Its stagger rises from 22 to 60 the longer you hold; a quick tap gives 22. Seven Horses: a fan 70 degrees wide and 6 metres long. It deals 5 damage, 6 stagger and a Mark to every enemy in it.

**Works with.** Gathered: Condemned, Reckoning, and a heavy weapon to finish what it breaks. Seven Horses: Spoken Sentence, Weighed, Brimming, a pack.

**Not built yet.** Gathered's ray doesn't yet look like it narrows to a point while you hold. Seven Horses' seven thin rays; for now, it is one cone as wide as the fan. If you buy the Empower, Seven Horses uses its numbers too: 12 damage and 7 metres.

**Example.** Use Seven Horses on a pack of five. All five are Marked at once, and a Spoken Sentence drops five blades.

## light.base.master
**What it does.** Cast it at the moment of a perfect dodge, and the attacker is blinded and can't turn around for two seconds (one second for a god or an elite). Dazzle, which used to be its own mark, lives on here.

**When it happens.** A perfect dodge, while Glare is in a spell slot.

**The numbers.** 3 Shards. For now, a perfect dodge holds the enemy you face, and every enemy within 2 metres of it, still for 2 seconds (a god or an elite for 0.4 of that), and Marks them, once every 6 seconds.

**Works with.** Clarity (perfect timing windows 50% longer), Step Into Light, Unblemished, and the root mark's three critical hits.

**Not built yet.** It should need you to cast Glare during the perfect dodge's window, and the blinding should only stop the enemy from turning. For now, the dodge alone holds them completely still, whether you cast Glare or not.

**Example.** Roll through a brute's swing at the last moment. It stands still and Marked while you turn and land three critical hits.

## light.lift
**What it does.** Seen Plainly, Light's reaction lift. Every reaction that Marked is part of is a critical hit, and Marks everything it reaches.

**When it happens.** Whenever a reaction with Marked in it happens, on any enemy.

**The numbers.** 3 Shards. Every blow of the reaction is a critical hit, for 1.5 times the damage. One Mark on everything the reaction hit: the enemy where it happened and every enemy its blows reached.

**Works with.** Every element pair with Marked in it (Light's reactions with its neighbours and the rest), the root mark (each Mark gives three critical hits), Weighed, and Daybreak and Full Daylight, which come after it on the chart.

**Example.** A reaction goes off on one Marked soldier in a pack. The two next to it come away Marked, and your next cuts on all of them are critical hits.

## light.daybreak
**What it does.** Daybreak, Light's larger spell, paid for with mana. You brace yourself and fire a beam of light down one long line. It hits everything in it four times and Marks it. You can turn it slowly with the stick or the cursor while it burns.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** 4 Shards, 45 mana. You brace, then fire a beam 12 metres long and 1.6 metres wide for 0.4 seconds. It hits every 0.1 seconds, four times in all, for 12 damage, 9 stagger and a Mark each time. An enemy that stays in it takes 48 damage and 36 stagger. Its Empower makes each hit 15 damage instead of 12 (60 in total). The two shapes keep their own numbers.

**Works with.** Brimming (mana kept full for it), Seen Plainly, which it comes after on the chart, Weighed, Condemned, Spoken Sentence, and the charged blade, which it charges with Light.

**Not built yet.** The rings should build up on the blade from the blows you land after starting it, up to eight, each worth 8 damage and 6 stagger, and fire with a second press. For now, the beam fires right away, as strong as six rings would be (48 damage).

**Example.** A line of spearmen comes up a staircase. One beam down the stairs Marks them all and leaves them staggering.

## light.daybreak.shape
**What it does.** Unlocks two shapes for Daybreak. You pick one at any rest fire. **Red Before the Sun** (Aruna riding in front of the sun god Surya to shield the world from his heat): the rings stand in front of you as a curved shield of red dawn that stops blows from the front, and then fire as the beam. It is Light's only shield, and still a beam at the end. **High Noon**: the light comes straight down on you as a column, burning away every enemy danger zone inside it, clearing every affliction on you, and Marking everything it hits. This is the cleansing version.

**When it happens.** Whenever you cast Daybreak, using the shape you picked. If you have not bought this, you cast the plain beam.

**The numbers.** 3 Shards, and the same 45 mana. Red Before the Sun: a wall 3 metres wide stands 1.5 metres in front of you for 4 seconds. Nothing can walk through it or reach you from the other side. When its time is up, or when your heavy blow hits it, it bursts along its width for 48 damage, 36 stagger and a Mark. High Noon: a column 5 metres across lands on you a moment after you cast it, dealing 36 damage, 36 stagger and a Mark to everything inside.

**Works with.** Red Before the Sun: archers and charges coming at you, a build that counters attacks, Unblemished. High Noon: being surrounded, Weighed, Spoken Sentence.

**Not built yet.** Red Before the Sun should add rings for each blow it stops, and fire the beam with a second press; for now, it bursts where it stands. It should also stop thrown attacks. High Noon clears the afflictions on you as it comes down, but it doesn't yet burn away enemy clouds, flames, frost and danger zones.

**Example.** Use High Noon in the middle of a pack. Everything around you is Marked at once, and the cuts that follow are critical hits.

## light.daybreak.master
**What it does.** Fire it within a fifth of a second of a blow landing on you, and the blow is blocked and the beam is a critical hit.

**When it happens.** Each time you cast it, while Daybreak is in a spell slot.

**The numbers.** 3 Shards. For now, every cast gives you a ward that blocks the next blow that would land on you. A beam fired in the half second after a perfect guard hits 50% harder (spells are 50% stronger for that half second).

**Works with.** Standing Light and perfect guards, Unblemished (a blocked blow doesn't count as getting hurt), Brimming.

**Not built yet.** It should work when a blow lands on you right as you cast it. For now, the ward and the perfect guard bonus take its place.

**Example.** A brute's swing comes in as you fire the beam. It glances off, and the beam runs down the whole pack behind it.

## light.devotion
**What it does.** Full Daylight, Light's Devotion, for going all in on Light only. Blows no longer use up a Mark. Every weapon blow on a Marked enemy is a critical hit until the Mark fades, but after it fades, that enemy can't be Marked again for a while. The fight gets a rhythm: Mark, use the whole window, then wait for the light to come back.

**When it happens.** Only while you carry Light twice (all in), once you buy it. It is always on then.

**The numbers.** 6 Shards. Blows no longer use up a Mark. Every weapon blow on a Marked enemy is a critical hit until the Mark fades, which takes 6 seconds instead of 8. Once it has faded, that enemy can't be Marked again for 4 seconds. A Mark set off or used up by a spell doesn't have this wait. Buying this also gives you the deep strike, The Sun on the Blade. Use it with L2 and R2 while you carry Light alone or twice.

**Works with.** Seen Plainly, which it comes after on the chart, the root mark, Just Cut and Unblemished (critical hits even stronger with Weighed), fast weapons, The Scales.

**Example.** Glare a captain, then cut it for six seconds. Every one of a dozen blows is a critical hit.

## light.scales
**What it does.** While your surge lasts, the damage you deal with your weapon and the damage you take are weighed against each other. If you dealt more when it ends, a sword of light falls on every Marked enemy near you. If not, the light leaves you, and takes your mana with it.

**When it happens.** Light your surge with the surge button (Clarity, or the other element's surge if you have that one picked). The scales are checked when it ends.

**The numbers.** A god's gift; it is never sold, but in the playtest you can buy it. Each sword deals 60% of how much more damage you dealt than you took, up to 120, plus 80 stagger, as a heavy blow, to each Marked enemy within 12 metres. If you took more damage than you dealt, or the same amount, your mana empties.

**Works with.** Clarity (it Marks with every blow and makes perfect timing windows 50% longer, so you take less damage), Unblemished, the root mark, and Standing Light and Brimming, to have enemies Marked when it ends.

**Example.** Light Clarity, then guard and cut for eight seconds without getting hit. You dealt 200 and took none, so a sword falls for 120 on every Marked enemy.

## light.twosuns
**What it does.** Two Suns, a keystone. Every enemy your spell hits is hit again a moment later by a burst of light, and Marked. Your spells have to wait a fifth longer between casts.

**When it happens.** Whenever one of your spells hits an enemy, while you carry Light.

**The numbers.** 6 Shards. The second burst comes 0.4 seconds later and is 2.5 metres across. It deals 12 damage, 10 stagger and one Mark, at most once every 0.2 seconds. Every spell's wait is 1.2 times as long.

**Works with.** Ring of Spears, which it comes after on the chart (from its Master mark), Reckoning, Weighed and Condemned (more Marks to finish with), and Daybreak's long line (every enemy on it is hit twice).

**Not built yet.** A figure of light should cast the spell again from the far side of the enemy you face. For now, each enemy hit takes a second burst of light where it stands.

**Example.** Drive Ring of Spears into a soldier with two others next to it. A moment later the light bursts over it again, and all three are Marked for your next blows.
