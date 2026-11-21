# Light: detailed explanations

The long explanation the chart shows for each high-level mark of the Light tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Light): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Marked" is Light's affliction: a mark of light that makes
the next blows on that enemy critical.

## light.mark
**What it does.** The last blow of a chain, and every perfect guard, leave a mark of light on the enemy: it is Marked. Your next three weapon blows on a Marked enemy are critical and each gives more mana; then the mark is spent.

**When it happens.** Always on once learned. The last light blow of a chain, or the heavy blow that ends one, marks the enemy it lands on; a perfect guard marks the one who struck. Like every root, it lays with the weapon only while Light is your first element (or your only one); the perfect guard's mark comes either way, and carried second, Light's marks come from your spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. A mark lasts 8 seconds or three weapon blows, whichever ends first. Each of those blows is critical, one and a half times its damage, and gives 2 more mana. Marking again refills the three blows.

**Works with.** Glare and every Light spell (each one Marks), Weighed (marks three deep), Just Cut and Unblemished (more critical blows), Condemned, Full Daylight (a mark not spent by blows), Clarity, and the charged blade, whose third Light blow throws a ray that Marks.

**Example.** Three light blows on a soldier mark it with the third; a heavy blow, then two more light blows, all land critical.

## light.weighed
**What it does.** Marks deepen, up to three. When an affliction on an enemy reaches its full depth, a mark at three among them, your critical blows strike harder for a while.

**When it happens.** Always on once learned. The deeper marks need Light to be your first element; with Light second, a mark stays one deep. The harder critical comes each time any affliction you lay fills on an enemy.

**The numbers.** 3 Shards. Marked up to three deep, each mark laid adding one. A full affliction makes critical blows deal 1.8 times their damage, where they deal 1.5, for 6 seconds.

**Works with.** The root, Reckoning's Spoken Sentence (12 more for each depth, once built), Ring of Spears and Brimming (marks laid fast), Condemned, which grows from it, Unblemished and Just Cut (more critical blows to spend it on).

**Not built yet.** Each depth making critical blows harder by itself: for now the harder critical comes when a mark fills.

**Example.** End three chains on a brute: it is three deep, and for six seconds every critical blow hits for 1.8 times.

## light.reckoning
**What it does.** Reckoning, the detonator of the tree. You set a seal on the enemy you face: a ring like a clock face on the ground under it, one hand sweeping it in six seconds. On the last tick it breaks upward in a column of light, and what your weapon did to that enemy in the six seconds is done to it again in part, with a great blow to its balance.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). With no enemy faced nothing is sealed, and the slot still waits. Then fight that enemy for six seconds.

**The numbers.** 4 Shards, no mana: it waits 12 seconds after a cast, less with weapon play. An enemy within 10 metres. The sign flashes, striking nothing, at each blow you land on it. When the six seconds end: 8 damage for each weapon blow that landed on it in that time, and 40 stagger. Its Empower makes it 10 for each blow, up to 180.

**Works with.** Quick weapons (more blows counted), the root and Just Cut (critical blows), Weighed, Condemned (the column's stagger with a full mark's), Ring of Spears (held still while you cut), the charged blade.

**Not built yet.** Half of the damage your weapon did in the six seconds, 150 at most: for now 8 a blow, whatever the blow. The clock face drawn on the ground.

**Example.** Seal a captain behind its guard, then cut it eight times in six seconds: the column takes 64 more and 40 stagger as the seal breaks.

## light.reckoning.shape
**What it does.** Opens two shapes of Reckoning, chosen at any rest fire. **Witnessed**, Surya the eye that sees every deed: the seal counts the bearer's deeds, not yours. Every blow it lands on you or your guard is scratched in gold on the clock's rim, and on the last tick a column falls on it. Guard well and the sun judges for you. **Spoken Sentence**: no clock. The seal waits for your next heavy blow on its bearer, then a blade of beaten gold falls on every Marked enemy near. Many enemies, at the moment you choose.

**When it happens.** Whenever you cast Reckoning, as the shape it wears. Before this is bought, the plain seal is cast.

**The numbers.** 3 Shards. Witnessed: a column on the sealed enemy as six seconds end, 30 damage, 40 stagger and Marked, 1.2 metres round. Spoken Sentence: for 8 seconds your next heavy blow on the sealed enemy drops a blade on every Marked enemy within 8 metres a moment later, 32 damage and 40 stagger 1.2 metres round each, and their marks are spent.

**Works with.** Witnessed: a counter build, perfect guards, Standing Light, the root (perfect guards mark the attacker). Spoken Sentence: Weighed (deep marks), Ring of Spears and Glare's Seven Horses (many Marked at once), Brimming, heavy weapons.

**Not built yet.** Witnessed counting the bearer's blows on you, 30 a blow and 15 more for each turned with a perfect guard: for now the column is one blow's worth. Spoken Sentence's 12 more for each depth of mark: for now 32 whatever the depth. Both shapes keep the plain seal's count beside their own: a shape cannot yet set its spell's own rule aside.

**Example.** Spoken Sentence on a captain with three Marked soldiers round it: one heavy blow, and four blades fall.

## light.reckoning.master
**What it does.** A finisher on the sealed enemy pays the count twice.

**When it happens.** While the seal is on its enemy, the six seconds after a cast, with Reckoning in a slot.

**The numbers.** 3 Shards. For now the first heavy blow on the sealed enemy strikes it again for 40 damage and 40 stagger, once a seal.

**Works with.** Heavy weapons, Condemned (the full mark's stagger brings the finisher on), Ring of Spears (held still to be finished).

**Not built yet.** A finisher as the blow, apart from any heavy blow, and the count paid a second time: for now any heavy blow, for a fixed 40.

**Example.** Stagger the sealed brute with a full mark and finish it: the seal pays out at once, and again when it breaks.

## light.spears
**What it does.** Ring of Spears, the breaker. Six spears of beaten gold stand in the air in a ring round the enemy you face, wait a breath, and drive in together, Marking it and holding it still.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). With no enemy faced they close on a spot ahead of you, and the slot still waits.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 40 stagger in all). It finds an enemy within 10 metres. The ring is 3 metres in radius and closes after 0.6 seconds. Each spear deals 6 damage and 5 stagger and Marks: 36 damage and 30 stagger in all. What it strikes is held still 1.5 seconds (a god or an elite 0.4 of that).

**Works with.** Weighed (each spear lays a mark), Reckoning (held still while the seal counts), Condemned, Spoken Sentence, and the charged blade, which it charges with Light.

**Example.** Cast on a brute, land your three critical blows while it is held, then a heavy blow while it is still Marked.

## light.spears.shape
**What it does.** Opens two shapes of Ring of Spears, chosen at any rest fire. **The Drawn Line**, the line drawn round Sita's door that no one meaning harm could cross: the six spears drive into the floor in a row across your front and stand. An enemy that crosses it is held and Marked, and your blows across the line are critical. Fight across it. **In Turn**: the spears drive in one after another, and a spear that lands with one of your weapon blows strikes double and is critical. Six beats to meet.

**When it happens.** Whenever you cast Ring of Spears, as the shape it wears. Before this is bought, the plain ring is cast.

**The numbers.** 3 Shards. The Drawn Line: for now the spears still drive in, then stand 5 seconds in the floor in their ring 3 metres round the enemy, six spears each 0.7 metres round, Marking and jarring for 12 stagger once a second whatever stands at them. In Turn: six blades fall on the enemy one after another, 6 damage, 5 stagger and Marked each; for a second and a half after, each weapon blow on it strikes a spear's worth more, 6 damage and 5 stagger, at most four times a second.

**Works with.** The Drawn Line: a doorway or a stair, ranged weapons and spears that strike across it, the root (Marked enemies take critical blows). In Turn: quick weapons, Just Cut, the charged blade.

**Not built yet.** The Drawn Line as a straight line six metres across your front that holds for a second what crosses it, with the spears not driving into the enemy. In Turn's quarter second between spears (for now they fall quickly one after another), and a spear landing within a tenth of a second of a blow as the moment.

**Example.** The Drawn Line across a corridor: the pack comes on, each one Marked as it reaches the spears, and every cut you give across them lands critical.

## light.spears.master
**What it does.** Land a critical blow on the enemy while the spears hang, and they all drive in at once, critical, and Ring of Spears is ready again.

**When it happens.** In the 0.6 seconds the spears hang over the enemy faced, with Ring of Spears in a slot.

**The numbers.** 3 Shards. For now the first weapon blow on that enemy in those 0.6 seconds strikes 1.5 metres round it for 18 damage, 15 stagger and Marked, and the wait ends at once.

**Works with.** A Marked enemy (its blows are critical), Just Cut, Unblemished, quick weapons that can land a blow in the moment.

**Not built yet.** Only a critical blow counted, and the spears themselves driven in at once: for now any weapon blow, and a burst of half again their worth.

**Example.** Mark a duellist with a chain, cast, and cut it before the spears fall: they strike at once and the ring is ready for the next one.

## light.condemned
**What it does.** When an affliction on an enemy reaches its full depth, its balance takes a great blow. The price: your weapon deals less to an enemy that carries nothing, though blows and spells on any afflicted enemy strike a little harder.

**When it happens.** Always on once learned. The blow to balance comes each time any affliction you lay fills on an enemy, a third mark among them.

**The numbers.** 6 Shards. A burst 1.2 metres round the enemy: no damage, 60 stagger, at most once every 4 seconds. Weapon damage 15% less; damage on an afflicted enemy 15% more, so against those the two nearly cancel.

**Works with.** Weighed, which it grows from, Ring of Spears, Glare and Brimming to fill marks, Reckoning (the column's stagger with it), and a second element's affliction, which fills on its own.

**Not built yet.** An enemy marked three deep and under a quarter of its health (a tenth for a god or an elite) finished whatever its balance: for now the full mark breaks much of its balance.

**Example.** Bring a brute's mark to three: the burst all but staggers it, and a heavy blow finishes the work.

## light.justcut
**What it does.** A blow that lands may flash. For a moment after a flash every blow you land is critical. A blow that lands may also give you a little stamina back.

**When it happens.** Any weapon blow that lands, one time in four, at most once every 1.5 seconds. The flash and the stamina are each their own roll.

**The numbers.** 3 Shards. One chance in four of a flash: every blow critical for 0.8 seconds. One chance in four of 6 stamina back. Each can come at most once every 1.5 seconds.

**Works with.** Weighed (critical blows harder), Step Into Light and Standing Light, which grow from it, Ring of Spears' Master, and quick weapons such as the Twin Knives, which fit more blows into the moment.

**Not built yet.** The flash for the next blow of a chain pressed at the very moment the last one lands, critical and free of stamina: for now it comes by chance.

**Example.** In a flurry on a duellist, one cut flashes and the two after it land critical without a mark.

## light.step
**What it does.** Step Into Light, the movement spell. You step through the enemy you face on a line of gold dust and come out on its far side, turned back toward it, cutting and Marking everything on the line. If the cut lands, your next heavy blow in the moment after is critical. Nothing can strike you in the step; a wall, or something too big to shove, stops you short.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). With no enemy faced you step ahead anyway, and the slot still waits.

**The numbers.** 4 Shards, no mana: it waits 8 seconds after a cast, less with weapon play (its Empower: 7, and 9 metres). Up to 7 metres: to 1.8 metres past the enemy faced, or about 5 metres ahead with none. The cut is 10 damage, 10 stagger and Marked on all on the line. The heavy blow after it is critical if it lands within 1.5 seconds.

**Works with.** The root (the mark it lays gives the heavy blow and two more their critical), Just Cut, Weighed, and the charged blade: the cut is a spell, so it charges your weapon with Light for the next three moves.

**Example.** Step through an archer behind its shield-bearer, turn, and land a critical heavy blow in its back.

## light.step.shape
**What it does.** Opens two shapes of Step Into Light, chosen at any rest fire. **Aruna's Road**, after the charioteer of dawn: the line you stepped stays a while as a road of gold dust that Marks whatever crosses it, and pressing again steps you back along it to where you began. Out and back. **Behind the Eye**: you step to behind the enemy, landing at its back, and your first blow after deals a finisher's damage.

**When it happens.** Whenever you cast Step Into Light, as the shape it wears. Before this is bought, the plain step is cast.

**The numbers.** 3 Shards. Aruna's Road: five patches along 7 metres ahead of where you cast, each 0.7 metres round, Mark whatever stands in them once a second for 3 seconds. Behind the Eye: if the cut lands, your next weapon blow within a second deals three times its damage.

**Works with.** Aruna's Road: a pack that follows you, the root, Weighed, Spoken Sentence. Behind the Eye: heavy weapons, the Learn's critical heavy blow (the two multiply), Unblemished.

**Not built yet.** Aruna's Road's step back along the road, a second press within the three seconds.

**Example.** Behind the Eye on a captain: step, turn, and one heavy blow lands critical and three times as hard.

## light.step.master
**What it does.** Cast it through two enemies standing in one line: both are cut and Marked, and Step Into Light is ready again. Light is straight.

**When it happens.** As the step cuts, with Step Into Light in a slot.

**The numbers.** 3 Shards. For now a step that cuts anything ends its own wait, once in 16 seconds.

**Works with.** Corridors and stairs, a line of spearmen, Aruna's Road (a second road at once).

**Not built yet.** Two enemies cut by one step as the moment: for now any step that cuts, at most once in 16 seconds.

**Example.** Two soldiers come up a stair in single file: step through both, and step again at once.

## light.lantern
**What it does.** Standing Light, the counter. You plant a lamp of brass a little ahead of you. A shaft of light stands over it, dust turning in it, and a glow lies round it while it stands. Enemies in the glow are struck lightly and Marked again and again, and a perfect guard bursts round you.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Then guard.

**The numbers.** 4 Shards, no mana: it waits 12 seconds after a cast, less with weapon play. Planted 2.5 metres ahead; the glow is 5 metres in radius and lasts 10 seconds. It strikes 0.2 seconds after planting and then every 2.5 seconds: 2 damage and Marked each time. Each lamp gives three perfect-guard bursts, shown beside the weapon as Lamp: each is 10 damage and 25 stagger within 3.5 metres of you.

**Works with.** The root (a fresh mark, and three fresh critical blows, every 2.5 seconds), Unblemished, which grows from it, Clarity's longer perfect moments, Reckoning's Witnessed, Condemned.

**Not built yet.** The burst only for a perfect guard made inside the glow: for now the next three perfect guards after each lamp burst, wherever you stand. No enemy cloud, flame or frost lying inside the glow. The Empower's longer perfect guard and dodge inside the glow: for now the Empower does nothing.

**Example.** Plant it in a doorway and fight on its edge: everything that comes through arrives Marked, and each perfect guard throws them back.

## light.lantern.shape
**What it does.** Opens two shapes of Standing Light, chosen at any rest fire. **Row of Lamps**, the rows of lamps at Diwali: three small lamps set in a line, every enemy between two of them Marked and kept clean of cloud, flame and frost, and a perfect guard made in line with them throws a ray down the row. The guard fires the row. **Circled Lamp**, the lamp waved before the god: a smaller lamp circles you, Marking what it passes and burning away any hazard you walk into. It goes where you go.

**When it happens.** Whenever you cast Standing Light, as the shape it wears. Before this is bought, the plain lamp is planted.

**The numbers.** 3 Shards. Row of Lamps: the lamp still stands, and three patches a metre round along 6 metres ahead of you Mark what stands in them once a second for 10 seconds; each perfect guard throws a ray 6 metres on from the attacker, 12 damage, 25 stagger and Marked, at most once a second. Circled Lamp: one lamp 2 metres out circles you for 8 seconds, striking what it passes for 2 damage and Marking it.

**Works with.** Row of Lamps: a counter build, Unblemished, perfect guards. Circled Lamp: a fight on the move, quick weapons, Brimming.

**Not built yet.** Row of Lamps: only the enemies between two lamps, only a guard made in line with them, the three lamps drawn, and nothing laid between them. Circled Lamp: hazards burned away, and the lamp drawn as a lamp (for now a disc of light).

**Example.** Row of Lamps across a hall: stand at one end and perfect-guard the brute that comes down it, and the ray runs back through the pack behind it.

## light.lantern.master
**What it does.** The first perfect guard inside the glow makes every Light spell ready again, once a lamp.

**When it happens.** A perfect guard, with Standing Light in a slot.

**The numbers.** 3 Shards. The waits of Glare, Step Into Light, Ring of Spears, Standing Light and Reckoning end at once, those that are in a slot; once in 12 seconds. Daybreak waits on no clock, so nothing changes for it.

**Works with.** A Hand full of Light spells, Unblemished, Just Cut, Clarity's longer perfect moments.

**Not built yet.** Only a perfect guard made inside the glow: for now any perfect guard, once in 12 seconds.

**Example.** Cast everything, plant the lamp, perfect-guard one blow, and cast everything again.

## light.unblemished
**What it does.** Fight a while without a wound and every weapon blow you land is critical, and gives more mana. A blow that lands on you ends it, and until you have gone a while unwounded again you take more from every blow.

**When it happens.** Always on once learned. You are unwounded once no blow has landed on you for 8 seconds; the gift lasts until the next one does.

**The numbers.** 6 Shards. Unwounded: every weapon blow critical, and 25% more mana from blows. After a wound, for 8 seconds or longer, you take 15% more damage. A blocked blow, a perfect guard or a perfect dodge is not a wound.

**Works with.** Standing Light, which it grows from, Weighed (critical blows 30% harder), Clarity's longer perfect moments, Just Cut: every blow you turn keeps you clean.

**Example.** Guard and dodge a duellist's whole chain without a scratch, and every answer you give it lands critical.

## light.brimming
**What it does.** While your mana is full, every heavy blow throws a wave of light ahead of it that strikes and Marks everything in front of you. Mana kept for the big spells has a use while it waits.

**When it happens.** Each heavy blow or finisher that lands while your mana bar is full.

**The numbers.** 3 Shards. A cone 5 metres long and 70 degrees wide: 12 damage, 12 stagger and Marked.

**Works with.** Daybreak (the mana it waits for), Seen Plainly, which grows from it, Weighed, Condemned, Spoken Sentence, and perfect guards to fill the bar. Spending mana on Daybreak, the surge or a deep strike ends it until the bar is full again.

**Example.** Full of mana before a pack: every heavy blow marks the three in front of you.

## light.base
**What it does.** Glare, Light's own spell, comes with the element and is never bought. A narrow ray from your hand along your facing, the dust lit along it, striking and Marking what it crosses. What it marks wears brackets of gold.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Light, it is always yours to put in a slot.

**The numbers.** Given with Light. No mana: it waits 6 seconds after a cast, less with weapon play; its Empower makes that 5. A ray 5.5 metres long and about 1.4 metres wide: 8 damage, 22 stagger and Marked to each enemy on it.

**Works with.** Its Empower, its two shapes and its Master; the root, Weighed, Condemned, Spoken Sentence (Marked enemies for the blades), the charged blade, which its ray charges with Light.

**Not built yet.** The Empower's 12 damage and seven metres: the base spell is cast by Light's kit, which takes only the Empower's shorter wait.

**Example.** Glare the soldier in front of the captain: it is Marked and reeling, and your next three cuts on it land critical.

## light.base.shape
**What it does.** Opens two shapes of Glare, chosen at any rest fire. **Gathered**: hold, and the ray narrows, the dust in it drawn to a point on the enemy; let go for a ray that breaks balance far harder. A lens, for breaking one enemy. **Seven Horses**, Surya's seven: the ray splits into seven thin rays in a fan, each striking lightly, and every enemy touched is Marked. Marking a crowd.

**When it happens.** Whenever you cast Glare, as the shape it wears. Before this is bought, the plain ray is cast.

**The numbers.** 3 Shards. Gathered: for now every ray strikes as at a full hold, 38 more stagger on each enemy it crosses (60 in all). Seven Horses: a fan of 70 degrees, 6 metres long: 5 damage, 6 stagger and Marked to every enemy in it.

**Works with.** Gathered: Condemned, Reckoning, a heavy weapon to finish what it breaks. Seven Horses: Spoken Sentence, Weighed, Brimming, a pack.

**Not built yet.** Gathered's hold of up to a second, the ray narrowing and the stagger rising with it: for now the full stagger at once. Seven Horses' seven thin rays: for now one cone of the fan's width. With the Empower bought, Seven Horses takes its numbers too, 12 damage and 7 metres.

**Example.** Seven Horses into a pack of five: all five Marked at once, and a Spoken Sentence drops five blades.

## light.base.master
**What it does.** Cast it in the perfect dodge's moment, and the attacker is blinded and cannot turn for two seconds, one for a god or an elite. Dazzle, once a mark of its own, lives on here.

**When it happens.** A perfect dodge, with Glare in a slot.

**The numbers.** 3 Shards. For now a perfect dodge holds the enemy faced, and every enemy within 2 metres of it, still for 2 seconds (a god or an elite 0.4 of that) and Marks them, once in 6 seconds.

**Works with.** Clarity (perfect moments half again as long), Step Into Light, Unblemished, the root's three critical blows.

**Not built yet.** The cast in the perfect dodge's window as the moment, and a blinding that stops only turning: for now the dodge alone holds them wholly, whether or not Glare is cast.

**Example.** Roll through a brute's swing at the last moment: it stands still and Marked while you turn and land three critical blows.

## light.lift
**What it does.** Seen Plainly, Light's reaction lift. Every reaction Marked takes part in is critical, and Marks everything it reaches.

**When it happens.** Whenever a reaction with Marked in it happens, on any enemy.

**The numbers.** 3 Shards. One mark on every enemy within 3 metres of where it happened.

**Works with.** Every pair with Marked in it (Light's reactions with its neighbours and the rest), the root (each mark three critical blows), Weighed, Daybreak and Full Daylight, which grow from it.

**Not built yet.** The reaction itself critical: for now only the marks spread. The mark laid on what the reaction reached, not on everything 3 metres round it.

**Example.** A reaction fires on one Marked soldier in a pack: the two beside it come away Marked, and your next cuts on them all land critical.

## light.daybreak
**What it does.** Daybreak, Light's larger spell, paid in mana. You brace and loose a beam of light down one long line. It bites everything in it four times and Marks it. The stick, or the cursor, turns it slowly while it burns.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, 45 mana. A brace, then a beam 12 metres long and 1.6 metres wide for 0.4 seconds. It bites every 0.1 seconds: 12 damage, 9 stagger and Marked each time, 48 damage and 36 stagger on what stays in it, six rings' worth. Its Empower makes each ring 10 damage, not 8 (60 in all); the two shapes keep their own numbers.

**Works with.** Brimming (mana kept full for it), Seen Plainly, which it grows from, Weighed, Condemned, Spoken Sentence, and the charged blade, which it charges with Light.

**Not built yet.** The rings gathered on the blade by the blows you land after beginning, eight at most, 8 damage and 6 stagger each, and loosed by a second press: for now the beam is loosed at once at six rings' strength.

**Example.** A line of spearmen coming up a stair: one beam down the stair marks them all and leaves them reeling.

## light.daybreak.shape
**What it does.** Opens two shapes of Daybreak, chosen at any rest fire. **Red Before the Sun**, Aruna riding before Surya to shield the world from his heat: the rings stand before you as a curved shield of red dawn that stops blows from the front, and then go as the beam. Light's one shield, and still a beam at the end. **High Noon**: the light comes straight down on you as a column, burning away every enemy hazard inside, clearing every affliction on you and Marking all it strikes. The purifying version.

**When it happens.** Whenever you cast Daybreak, as the shape it wears. Before this is bought, the plain beam is cast.

**The numbers.** 3 Shards, and the same 45 mana. Red Before the Sun: a wall 3 metres wide stands 1.5 metres before you for 4 seconds, and nothing walks through it nor reaches you from its far side; when its time is up, or a heavy blow of yours strikes it, it bursts along its width, 48 damage, 36 stagger and Marked. High Noon: a column 5 metres across on you a moment after the cast, 36 damage, 36 stagger and Marked to everything inside.

**Works with.** Red Before the Sun: archers and a charge coming at you, a counter build, Unblemished. High Noon: being surrounded, Weighed, Spoken Sentence.

**Not built yet.** Red Before the Sun's rings added by each blow it stops and the beam loosed by a second press: for now a burst at its own place. Its stopping thrown attacks as well. High Noon's burning away of enemy clouds, flame, frost and hazards, and its clearing of the afflictions on you.

**Example.** High Noon in the middle of a pack: everything round you is Marked at once, and the cuts that follow are critical.

## light.daybreak.master
**What it does.** Loose it within a fifth of a second of a blow landing on you, and the blow is turned and the beam is critical.

**When it happens.** Each cast, with Daybreak in a slot.

**The numbers.** 3 Shards. For now every cast gives a ward that turns the next blow that would land on you, and a beam loosed in the half second after a perfect guard strikes half again as hard (spells 50% harder for that half second).

**Works with.** Standing Light and perfect guards, Unblemished (a turned blow is no wound), Brimming.

**Not built yet.** The blow landing on you as the moment of the cast: for now the ward and the perfect guard stand in.

**Example.** A brute's swing comes in as you loose the beam: it glances off, and the beam runs down the whole pack behind it.

## light.devotion
**What it does.** Full Daylight, Light's Devotion: all in on Light only. A mark is no longer spent by blows. Every weapon blow on a Marked enemy is critical until the mark fades, but no mark can be laid on that enemy again for a while after it fades. The fight becomes a rhythm: mark, spend the window in full, wait out the light.

**When it happens.** Only while Light is carried twice (all in), once bought. Always on then.

**The numbers.** 6 Shards. Blows no longer spend a mark: every weapon blow on a Marked enemy is critical until the mark fades, in 6 seconds where it lasted 8. Once it has faded, that enemy cannot be Marked again for 4 seconds; a mark set off or spent by a spell does not wait. Buying this also gives you the deep strike, The Sun on the Blade. Use it with L2 and R2 while you carry this element alone or twice.

**Works with.** Seen Plainly, which it grows from, the root, Just Cut and Unblemished (critical blows harder still with Weighed), quick weapons, The Scales.

**Example.** Glare a captain, then cut it for six seconds: every one of a dozen blows lands critical.

## light.scales
**What it does.** While your surge lasts, the wounds you deal with your weapon and the wounds you take are weighed. If yours are the heavier when it ends, a sword of light falls on every Marked enemy near you. If not, the light leaves you, and your mana with it.

**When it happens.** Light your surge with the surge button (Clarity, or the other element's surge if you wear that); the scales are read when it ends.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. Each sword deals 60% of how much more you dealt than you took, 120 at most, with 80 stagger, as a heavy blow, to each Marked enemy within 12 metres. Taken more than you dealt, or as much: your mana empties.

**Works with.** Clarity (it marks with every blow and makes the perfect moments half again as long, so you take less), Unblemished, the root, Standing Light and Brimming to have them Marked when it ends.

**Example.** Light Clarity, guard and cut for eight seconds without being touched: 200 dealt to none taken, and a sword falls for 120 on every Marked enemy.

## light.twosuns
**What it does.** Two Suns, a keystone. Every enemy your spell strikes is struck again a moment later by a burst of light, and Marked. Your spells wait a fifth longer between casts.

**When it happens.** Whenever a spell of yours strikes an enemy, while Light is carried.

**The numbers.** 6 Shards. The second burst comes 0.4 seconds later, 2.5 metres across: 12 damage, 10 stagger and one Mark. At most once every 0.2 seconds. Every spell's wait is 1.2 times as long.

**Works with.** Ring of Spears, whose Master it grows from, Reckoning, Weighed and Condemned (more Marks to finish with), Daybreak's long line (every enemy on it struck twice).

**Not built yet.** A figure of light casting the spell itself again from the far side of the enemy you face. For now each enemy struck takes a second burst of light where it stands.

**Example.** Drive Ring of Spears into a soldier with two beside it: a moment later the light bursts over it again and all three are Marked for your next blows.
