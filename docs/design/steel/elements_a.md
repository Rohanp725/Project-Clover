# Steel and element, the element slice A: Fire, Lightning, Wind, Ice, Water, Life

Written to `docs/design/steel/BRIEF.md` on 2026-11-18 as content for `docs/design/STEEL_AND_ELEMENT.md` sections 3, 5
and 6. Everything here is a **Proposal**. Every number is a first guess. The weapon tree is called **the Way** (the
owner's name of 2026-11-18). The Earth and Metal mix keeps the name Forge.

## Index

**Rules used across the slice**
- Branch costs: a spell branch is four marks and costs 11 Shards.
- Movement rule: a movement spell started as a blow would land counts as a perfect dodge.
- Infused strike rule: an 8 s cooldown on each of the two strike buttons, and it counts as a weapon blow; the deep
  strike (all in, L2 with R2) also costs 20 mana (the shared rule in `README.md`).
- Charged blade rule: the third charged blow of each element does one extra thing.

**Fire** (Burn)
- Flame Wave (built, tap): opener; fire runs along the ground in a cone.
- Uraeus (new): opener; a cobra of flame on the shoulder spits burning pitch at whatever starts an attack.
- Wildfire Rush (built): movement; drive through the enemies ahead in a line of fire.
- Meteor Leap (built): breaker, mana; come down on the enemy faced as a falling stone.
- Kiln Stance (built): counter; stand in flame and answer the first blow with a pillar of fire.
- Sunspot (built): detonator; a brand fed by blows and set off by a heavy.
- Charged blade: the third blow leaves the ground burning under the enemy.
- Infused strike, Burning Edge: the swing throws a sheet of flame along its arc.
- Devotion, Fire Without End: burns never go out, and a burn left alone deepens.
- Reaction lift, Fuel to It: every reaction with Burn in it is hotter and leaves fire on the ground.
- Passives: Cinder, Heat, Backdraft, Flame Trail, Firebomb Flask, Catching Fire, White Heat, Falling Sun.
- Surge: Kindle keeps its slot but trades its burn for fuel to the cooldowns.

**Lightning** (Static)
- Chain Lance (built): opener; a bolt that steps along the ground and leaps through five.
- Thunderclap (built, tap): breaker; Rolling Thunder or Held Thunder.
- Waiting Thunder (built): breaker; bolts behind the shoulders that fall with your blows.
- Seven Flashes (built): detonator, mana; seven cuts in one breath.
- Lightning Rod (built): movement; plant a rod, then step to it in a flash.
- Ring of Drums (new): counter; three drums of cloud, each beaten by a perfect guard or dodge.
- Charged blade: each charged blow leaps on to a second enemy.
- Infused strike, Flash Cut: a lunge as a line of lightning.
- Devotion, In One Breath: after a perfect guard or dodge, spells have no cooldown for a breath.
- Reaction lift, Strikes Twice: every reaction with Static in it strikes again where it happened.
- Passives: Static, Blink, Afterimage, Stored Charge, Single Stroke, Never There, Storm Step.
- Surge: Overdrive unchanged, with the critical chain-ender added.

**Wind** (Unsteady)
- Gust (built, tap): opener; throws everything in a cone down the wind.
- Lifting Wind (built): breaker; a dust devil under the enemy faced.
- Indrawn Breath (built): detonator; draws everything in, then spends its Unsteady in one burst.
- Closing Circle (built): movement; run a ring round the enemy and the wind inside rises.
- Bag of Winds (new): counter; catch blows and thrown things in a bag, then open it.
- Sky Dance (built): breaker, mana; go up with the enemy and cut in time.
- Charged blade: each charged blow reaches a metre further as a cut of wind.
- Infused strike, Cutting Wind: the swing throws a crescent of wind.
- Devotion, On the Wind: spells have no wind-up and are cast from inside a dodge.
- Reaction lift, Carried Far: every reaction with Unsteady in it reaches further and throws.
- Passives: Unsteady, Rising Cut, Passing Cut, Turned Aside, Against the Wall, Leaf on the Wind, Nothing Beneath, The Wind Remembers.
- Surge: Tailwind gains doubled cooldown cuts from perfect dodges.

**Ice** (Chill and Frozen)
- Hoarfrost (built, tap): opener; a fan of crystals that a blow shatters.
- Eight Points (built): detonator; eight shards close, spending Chill, shattering the Frozen.
- Hanging Ice (built): breaker; an icicle falls on the enemy faced after a wait.
- Winter Wall (built): breaker, mana; a wall that a heavy blow bursts forward.
- Rime Guard (new): counter; a freezing fog in which a perfect guard freezes the attacker.
- Ski Run (new): movement; a lane of clear ice you slide down at once.
- Charged blade: a charged blow shatters a Frozen enemy, light or heavy.
- Infused strike, Frost Bite: the blow plants a spike of ice through the enemy.
- Devotion, Deep Winter: Chill never fades, and Glaze freezes what is near as it is lit.
- Reaction lift, Cold Settles: every reaction with Chill in it leaves frozen ground and a slow.
- Passives: Chill, Cold Read, Flying Shards, Brittle, Splinter Ward, Unmoved, All at Once, The World Holds Still.
- Surge: Glaze unchanged in kind; a turned blow counts as a perfect guard.

**Water** (Soaked)
- Riptide (built, tap): opener; a wave rolls back and carries what it takes to your feet.
- The Tide Turns (new): movement; the ebb draws you back out of reach, the flood brings you in again as a wave.
- Moon on Water (built): counter; stand open, turn every blow, give it back in one cut.
- Whirlpool (built): breaker; a whirlpool in which balance does not recover.
- Deep Crack (new): detonator; a bubble collapses with a crack and boils out the Soaked.
- Poured Blade (built): breaker, mana; a copy of you in water repeats your blows.
- Charged blade: each charged blow splashes the enemies beside its target.
- Infused strike, Lash: the swing trails a whip of water that pulls.
- Devotion, The Sea Comes In: every spell leaves standing water, and you fight best in it.
- Reaction lift, Spreading Water: every reaction with Soaked in it runs on to the other Soaked.
- Passives: Soaked, Ebb and Flood, Undertow, Heavy Water, Broken Gourd, Catch the Rain, Deep Well, The Sea Rises.
- Surge: Wellspring, the weakest surge, gains a flask that buys back cooldowns.

**Life** (Seeded)
- Briar Patch (built, tap): opener; a bed of thorned vines.
- Sapling (built major, now a spell): counter; perfect guards and dodges plant lashing saplings.
- Root Walk (new): movement; go under the ground and come up at anything you have grown.
- Reap What Is Sown (built): detonator; everything grown and every seed bursts at once.
- Elder Tree (built): breaker, mana; a tree that grows and strikes the staggered.
- Red Harvest (built): breaker, paid in health; every blow critical and mending.
- Charged blade: each charged blow mends you.
- Infused strike, Thorn Set: the blow leaves a thorn that mends you while you strike it.
- Devotion, Wood Grows Back: whatever you grow grows back, and every spell sows.
- Reaction lift, Root and Branch: every reaction with Seeded in it mends you and sows.
- Passives: Seeded, Ripening, Fallen Seeds, Bitter Rind, Briar Coat, Thin Blood, Wildwood, First Spring.
- Surge: Regrowth gains mending for each spell cast in it.

**At the end**
- What to cut from the built content.
- Sources.

---

## Rules used across this slice

- **Marks and prices.** A spell branch is four marks: Learn (Spell, 4 Shards), Empower (Minor, 1), Shape (Major, 3;
  buying it opens both shapes, swapped at any rest fire), Master (Major, 3). That is 11 Shards a spell and 24 marks
  for six spells. The ten passives are a Root (2), majors (3), one or two keystones (6), Devotion (6), the Reaction
  lift (3) and the god's gift (given, never sold), so a tree is 34 marks and about 95 Shards.
- **Empower** is always "stronger, shorter cooldown". Each row gives the numbers.
- **Cooldowns.** Openers and movement take 6 to 8 s, breakers and counters 9 to 10 s, detonators 11 to 12 s. One
  spell per element is a larger spell paid in mana (35 to 50), as section 3.2 has it.
- **Masters** are tied to timing, and no two in an element use the same timing. **One shared rule**: a movement
  spell started in the instant a blow would land counts as a perfect dodge, with the mana and cooldown cuts a
  perfect dodge gives. Each movement spell's Master adds its own reward on top.
- **The infused strike** (L2 with R1) is the weapon's own carrier move (`weapons.md` 0.4), carried by the element.
  It has its own 8 s cooldown, which weapon play shortens like a spell's, counts as a weapon blow (it takes time off
  the spells' cooldowns and gives mana), and does not charge the blade, so it cannot feed itself. Each weapon's Way
  changes how it looks and works. All in on one element, L2 with R2 is the **deep strike** given under Devotion: it
  keeps that button's own 8 s cooldown and also costs 20 mana. (Settled across all seven files; see `README.md`.)
- **The charged blade** (3.3) is the same for every element: three blows, each laying its affliction and striking
  20% harder. Each element adds one thing to all three, said below.
- **Kinds of target.** Every hold, freeze, lift and pull has the shorter values for elites and gods set in
  `trees_a.md`. The colossus is never held or moved; it takes stagger instead.
- **Engine.** Forms named are from `FORMS.md`. A move that needs more is marked **New code**, in one line.

---

## Fire

**The image.** The sun-king's land at noon. The Eye of Ra was the sun's fire turned outward: as the uraeus cobra on
the brow it spat flame like venom at the enemies of the sun's boat, and Sekhmet's breath was the hot desert wind.
Fire here is flame, smoke, embers and char. It never shows as white or gold light.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Flame Wave** (built, tap) | Opener | 7 s | As built: fire runs out along the ground in a cone 5.5 m and 70 degrees: 20 damage, 24 stagger, Burn. Three patches burn on for 3 s. | 5.5 s; the patches burn 5 s. | **Cinder Fan** (built spell, folded in): close and wide, 3 m and 150 degrees. Burn two deep, and everything in it is thrown back 1.5 m. The shape for being surrounded. | **Running Fire** (new): one narrow tongue of flame runs 12 m along the ground in 0.8 s and stays as a low burning line for 4 s. Whatever crosses it takes 2 Burn. Lay it across a path, or reach an archer. | Cast in the same instant as the charged blade's third blow lands, and the wave leaves from the blade along the swing and its cooldown does not start. |
| **Uraeus** (new) | Opener | 10 s | A cobra of flame rises from your shoulder and stands 8 s. Each time an enemy within 8 m begins an attack, it spits a gob of burning pitch at it: 8 damage and 2 Burn, and the pitch burns at its feet for 2 s. Once every 1.2 s, six spits at most. Form: a phalanx blade that fires on an enemy's wind-up (**New code**: a trigger on an enemy beginning an attack). | 8 s; eight spits. | **Coiled**: it stays coiled close and spits only at what attacks you from within 3 m. Each spit lays 3 Burn and holds the enemy's wind-up for 0.3 s, so you have longer to read it. | **Set Down**: it is planted where you cast it, 2 m tall, and guards a circle of 9 m round itself whether you are there or not. Fight beside it, or leave it on a door. | Each perfect dodge while it stands gives it one more spit and 2 s more. |
| **Wildfire Rush** (built) | Movement | 8 s | As built: you drive 7 m through the enemies ahead as a line of fire, untouchable, and end behind them facing back. Each one passed takes 14 damage, 20 stagger and 2 Burn, and you gain 1 Heat. The line burns 2.6 s. | 6 s; the line burns 4 s. | **Fire Front** (new): you go 5 m as the front of a grass fire, a wall of flame 4 m wide, slower (0.7 s). Every enemy it meets is carried along before you, burning two deep, and thrown down at the end for 0.6 s. The drive that pushes a crowd instead of passing through it. | **Smoke Road**: you go 6 m back out of the fight in a roll of black smoke, and the line of fire is left between you and them. The escape. | Shared movement rule, and Heat goes to five. |
| **Meteor Leap** (built) | Breaker | 45 mana | As built: a 0.6 s leap, untouchable, up to 9 m, coming down on the enemy faced: 46 damage and 55 stagger in 3.2 m. The crater burns 4 s. | 35 mana; the crater burns 6 s. | **Falling Star** (the star in the tomb paintings): you do not leap. A burning stone falls on the aim point after 0.8 s with its shadow growing under it, and you go on fighting. 60 stagger. | **Ash Fall**: the landing throws hot ash over 6 m. For 5 s enemies in the ash gain 1 Burn a second and their balance does not recover. | Land it on an enemy at five Burn and its burn goes off with the blow (10 a stack). Half the mana comes back. |
| **Kiln Stance** (built) | Counter | 9 s | As built: you stand ready in flame for up to 1.2 s. The first blow that reaches you, red or not, is turned, and a pillar of fire rises under the attacker: 30 damage, 50 stagger, 3 Burn. It counts as a perfect guard. If nothing comes you are open for 0.4 s. | 7 s; the pillar lays 4 Burn. | **Shut Door** (a backdraft: smoke starved of air goes off when the door opens): the stance holds as long as the button is held, up to 3 s. Every blow turned is banked as smoke in a hood over you, three at most. Let go and the door opens: a cone of flame 6 m, 20 damage and 15 more for each blow banked. For combos, not single blows. | **Firewalker**: you can walk at half speed in the stance, but it catches only blows in its first 0.6 s. Walk into the swing. | Catch the blow in the stance's first 0.2 s and the pillar is twice as tall: 60 damage, and the attacker is thrown down. |
| **Sunspot** (built) | Detonator | 11 s | As built: a brand on the enemy faced (6 m, 7 s). Each light blow feeds it, six at most. A heavy blow sets it off: 12 damage and 9 a blow fed, 10 stagger and 8 a blow fed, in 3 m, and the target's Burn is spread to all it reaches. | 9 s; the target keeps 2 Burn after the burst. | **Smouldering Brand**: the brand is fed by Burn, not blows. It sets itself off when its target reaches five Burn, and spends it. For a player casting more than striking. | **Second Sun**: when it is set off, the brand jumps to the nearest burning enemy with half of what was fed. Work through a room. | Set off by a finisher, it bursts on every burning enemy within 10 m at half. |

**Seen from above.**
- Flame Wave: tongues of flame running out row by row over the floor, with smoke curling off the patches.
- Uraeus: a hooded cobra of layered flame at the shoulder, its hood edged with smoke. Each spit is an arcing black
  gob trailing fire that splashes into a burning smear.
- Wildfire Rush: a lane of real ground fire with char left behind it. Fire Front is a low wall of flame with black
  smoke rolling ahead of it and bodies tumbling before it.
- Meteor Leap: a stone wrapped in fire with a long smoke tail. The crater is cracked, glowing and smoking.
- Kiln Stance: flame standing up round the player like the walls of a kiln. Shut Door is a hood of brown smoke
  that thickens with each blow and then flashes over orange.
- Sunspot: a small burning sun over the enemy's head that swells with each blow (as built).

**Charged blade.** Flame runs along the edge and smoke streams off it, shedding embers as it swings. The third
charged blow leaves the ground under its target burning for 3 s (a 1.5 m patch, 1 Burn a second).

**Infused strike: Burning Edge.** The opening blow is drawn through fire. The swing throws a sheet of flame along its
arc and 3 m beyond, laying 2 Burn on all it reaches, and the arc's ground burns for 3 s.

**Devotion: Fire Without End.** Burns never go out by themselves; only death or water ends them. A burn left alone
deepens by one every 4 s, up to five. This changes play: light everything once, then fight the one in front of you
while the rest of the room cooks.
- *Deep strike (L2 with R2)*, **Burnt Offering**: the burning edge ends in a pillar of flame on the enemy struck.
  30 damage, and its burn goes off.

**Reaction lift: Fuel to It.** Every reaction with Burn in it strikes 25% harder and leaves the ground where it
happened burning for 3 s.

**Passives** (with Devotion and the lift, ten marks):

| Mark | Kind | What it does | Built or new |
|---|---|---|---|
| Cinder | Root | Every blow sets a burn, and each blow deepens it, up to five. | Built |
| Heat | Major | Blows landed without being struck build Heat, up to five, each level adding damage. A blow taken puts it out. | Built |
| Backdraft | Major | A perfect guard throws a cone of fire back at the one who struck. Against a burning enemy, its burn goes off too. | Built; Burn and Detonate folded in |
| Flame Trail | Major | The dodge leaves a line of fire behind it. | Built |
| Firebomb Flask | Major | Hold the flask button to throw it as a firebomb. | Built |
| Catching Fire | Major | A burning enemy that dies throws its burn onto everyone near it. | Built |
| White Heat | Keystone | Heat rises to eight, and above five your blows come faster. Struck above five, you burn. | Built |
| Falling Sun | Gift | Light Kindle, and the sun takes its road. | Built |

**Surge.** Kindle stays as built, 40 mana, in the surge slot. Its "every blow sets a burn" repeats Cinder, so trade
that half for something new: while it lasts, blows take twice as much off running cooldowns, fire feeding on fuel.

---

## Lightning

**The image.** Narukami's dry storm. Raijin beats a ring of drums to make the thunder. His beast Raiju leaps through
trees and leaves claw marks where the lightning struck. Real lightning comes down in bright steps, meets a streamer
rising from the ground, and then the return stroke lights the whole channel at once. Lightning is jagged bolts,
crackle and scorch. It is there and gone, and leaves only a burn on the floor.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Chain Lance** (built) | Opener | 7 s | A bolt is thrown ahead and leaps through up to five enemies: 10 damage and 2 Static on each. Re-aimed: it no longer competes with Thunderclap for stagger, and it lays the Static. | 5.5 s; six leaps. | **Planted Bolt**: it does not leap. It stands in the first enemy it strikes for 4 s like a spear in the ground, and arcs from it to every other enemy within 4 m once a second (1 Static). Pin the middle of a group. | **Return Stroke**: after its last leap it comes back along the same path to your blade, striking each enemy again for half, and the blade is charged for five blows, not three. Throw, then fight. | Thrown within 0.5 s after a finisher, it leaps to every enemy within 12 m. |
| **Thunderclap** (built, tap) | Breaker | 9 s | As built: a bolt on the enemy faced up to 10 m: 16 damage, 45 stagger, forking to two more. | 7 s; 55 stagger. | **Rolling Thunder** (from the design): three claps march out in a line, 3 m apart, 0.25 s each. | **Held Thunder** (from the design): hold for up to 1 s, then one huge clap with no forks: 90 stagger at the full hold. | A perfect guard makes the next Thunderclap instant and resets its cooldown (from the design). |
| **Waiting Thunder** (built) | Breaker | 10 s | As built: five bolts hang behind your shoulders for 10 s. Each light blow sends one down with it (8 damage, 1 Static), and a heavy blow sends all that are left (8 damage and 6 stagger each) before it discharges. | 8 s; seven bolts. | **Hanging Cloud**: the bolts hang in a small black cloud over the enemy faced, not over you. They fall on it whenever you strike anyone, so you can cut the soldier and drop the bolts on the archer. | **Hammer Fall**: light blows send nothing. The heavy blow brings all of them as one bolt in a 3 m circle: five times a bolt's damage and 60 stagger. Save it for the heavy. | Land the heavy blow within 1 s of the last bolt falling with a light blow, and all five come back for a second round. |
| **Seven Flashes** (built) | Detonator | 45 mana | As built: you vanish, seven lines are cut from enemy to enemy within 8 m, you stand still on the starting spot with your back turned, and the cuts land together. Any enemy at full Static discharges. | 35 mana; 11 a cut. | **One Flash**: against the enemy faced only. All seven cuts lie on one line through it and land as one blow of seven, and its Static discharges twice. | **Left Behind**: you do not return. You end behind the last enemy cut, and a shape of light takes the still pose on the starting spot and is what can be struck. No opening, but you end in the crowd. | Cast within 0.3 s of a perfect dodge and you do not stand still afterwards: the cuts land at once. |
| **Lightning Rod** (built) | Movement | 10 s | As built: a rod is planted 2 m ahead and stands 8 s. Every 0.9 s a bolt falls on an enemy within 3.5 m of it (12 damage, 1 Static). Press the button again while it stands and you step to it in a flash, landing with a 2.5 m shock (14 damage, 20 stagger), and the rod is spent. | 8 s; the rod strikes within 4.5 m. | **Lodestone** (built spell, folded in): the rod draws every enemy within 7 m toward it, further for each Static they carry, then discharges all of them in 3.5 m. There is no step. | **Thrown Rod**: the rod is thrown up to 10 m, into a far enemy or the ground, so the step goes that far. | Shared movement rule, and the rod is not spent by the step. |
| **Ring of Drums** (new) | Counter | 10 s | Three drums of cloud and lightning turn behind your shoulders for 8 s. Each perfect guard or perfect dodge beats one, and a bolt falls on the attacker: 18 damage, 35 stagger, 2 Static. The third beat brings a great clap in 4 m (30 damage, 60 stagger) that discharges everything in it. Form: phalanx blades fired by the perfect guard trigger. | 8 s; the ring stands 10 s. | **Drum Roll**: plain guards beat them too, for a small clap (8 damage, 15 stagger), and a perfect guard beats two at once. For a player who guards rather than dodges. | **Silent Drum**: nothing falls on a beat; each beat is banked, and your next heavy blow carries all of them (30% more damage and 20 stagger each). | Beat all three within 3 s and the ring stays for another round. |

**Seen from above.**
- Chain Lance: the bolt crosses the floor in visible bright steps, each a metre and a half, with faint forks.
  Then the whole path flashes at once.
- Thunderclap: a jagged bolt drawn from the sky, redrawn many times a second for a moment, leaving a scorch star.
- Waiting Thunder: bolts behind the shoulders jumping to new shapes (as built). Hanging Cloud is a small low black
  cloud lit from inside.
- Seven Flashes: thin cyan-violet cut lines on the floor that flare all at once (as built).
- Lightning Rod: an iron rod whose tip crawls with a corona of crackle before each strike, as a ship's mast does
  before the storm breaks.
- Ring of Drums: three round drum faces of dark cloud with crackle running round their rims. Each beat is a flat
  ring of dust thrown off the floor and a bolt.

**Charged blade.** Crackle crawls along the edge and jumps to the guard in short arcs. Each charged blow leaps on
to one more enemy within 4 m for half its damage, laying Static there too.

**Infused strike: Flash Cut.** The opening blow becomes a lunge. You cross up to 5 m as a line of lightning,
untouchable, and cut what is at the end for one and a half times the blow, with 2 Static.

**Devotion: In One Breath.** After a perfect guard or a perfect dodge, your spells have no cooldown for 2.5 s.
Cast everything you hold in the breath; then the cooldowns start from full. This changes play: lightning becomes
waiting for the one right moment, then spending the whole hand in it.
- *Deep strike (L2 with R2)*, **Down the Channel**: the lunge passes through. 0.4 s later a bolt comes down along the
  whole line and discharges every enemy on it.

**Reaction lift: Strikes Twice.** Every reaction with Static in it strikes again 0.5 s later at half strength
where it happened, as lightning comes back down its own channel.

**Passives** (with Devotion and the lift, ten marks):

| Mark | Kind | What it does | Built or new |
|---|---|---|---|
| Static | Root | Blows build Static, up to five. A heavy blow or a finisher discharges it. | Built |
| Blink | Major | Each dodge sends a line of lightning down its path. | Built |
| Afterimage | Major | A perfect dodge leaves a copy of you that strikes where you stood. | Built |
| Stored Charge | Major | Perfect guards bank charges, up to three; each blow spends one for double damage and stagger. | Built; Deep Reserve folded in |
| Single Stroke | Keystone | Static builds to ten, and a discharge lands all of it on the one struck, twice as hard. | Built |
| Never There | Keystone | Every dodge carries further and a perfect dodge costs nothing. Your guard holds half as well. | Built |
| Storm Step | Gift | While Overdrive lasts, every dodge costs nothing and shocks what it passes through. | Built |

**Surge.** Overdrive stays as built, 40 mana, in the surge slot. Faster swings already take more off the cooldowns,
so it needs no new part beyond the design's own: the last blow of every chain is critical.

---

## Wind

**The image.** Fujin carries the winds in a bag over his shoulders and lets them out by opening its neck. In the
reworked castle the storm is dry: dust, dry grass and torn paper streaming one way. Real wind comes down in a
downburst that spreads as a ring of dust, or stands up as a dust devil over hot ground. Wind is pale streaks of
dust, dry grass, leaves and paper, never a coloured glow.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Gust** (built, tap) | Opener | 6 s | As built: a rush of air 5 m ahead: 8 damage, 20 stagger, Unsteady two deep, and everything in it is thrown 3 m straight down the wind. | 4.5 s; Unsteady three deep. | **Downburst**: the wind comes down from above on a point up to 6 m away and spreads as a ring. Everything within 3 m is thrown 2.5 m outward from that point. Aim it at your own feet to clear a ring round you. | **Long Gust**: a narrow lane 2 m wide and 10 m long that throws what it takes 5 m. For throwing one enemy into a wall, a pit or its friends. | Cast in the instant an enemy's attack mark shows, and the gust turns its blow aside into empty air. Elites' blows come 0.4 s late instead; gods are not turned. |
| **Lifting Wind** (built) | Breaker | 9 s | As built: a 2 m circle is drawn under the enemy faced (within 5 m) for 0.35 s, then a column of wind: 8 damage and into the air for 1.0 s, whatever its footing. Breaker: 40 stagger as it rises. | 7 s; it hangs 1.3 s. | **Dust Devil**: the column wanders after enemies for 3 s, lifting the first it reaches and then the next. | **Downdraft**: the wind comes down instead. No lift: 60 stagger and it is laid flat for 0.6 s. For what cannot be lifted. | Cast on the enemy a perfect dodge has just taken you past, and it hangs twice as long. |
| **Indrawn Breath** (built) | Detonator | 11 s | As built: everything within 7 m is drawn over 0.5 s to a point 2 m ahead, then bursts apart (10 damage, 25 stagger, a 2 m push). Re-aimed as the detonator: the burst spends each enemy's Unsteady, 6 damage and 8 stagger a stack. | 9 s; it draws from 8.5 m. | **Held Breath**: the draw lasts as long as the button is held, up to 2 s, and the point walks with you. Let go to burst. | **All One Way**: the burst does not scatter them. All of them are thrown together 5 m the way you face. Gather a crowd, then throw it into a wall. | Cast within 0.5 s after a finisher, and the burst spends no Unsteady: every stack stays for the next blow. |
| **Closing Circle** (built) | Movement | 9 s | As built: with an enemy within 5 m you run one full ring round it in 0.7 s, untouchable, drawing a line behind you. The line snaps inward, a column stands in it, and everything inside takes 5 Unsteady and goes into the air for 1.2 s. You end where you began. | 7 s; enemies hang 1.5 s. | **Half Circle**: you run only half the ring and end on its far side, behind it. Nothing is lifted, and your first blow on its back is critical. | **Wide Circle**: you run round a group up to 6 m across. The column takes everyone inside but lifts each for only 0.6 s. | Shared movement rule, and the ring is run round the attacker wherever it stands within 8 m. |
| **Bag of Winds** (new) | Counter | 10 s | You untie a bag of wind at your hip for 8 s. Each blow you perfect-guard or perfect-dodge is caught in it: the attacker is thrown back 2 m and its wind goes into the bag, three at most. Thrown things that come within 2 m of you (arrows, stones, flame) are caught too. Press again, or when the time is up, and it opens: a cone 7 m, 12 damage and 25 stagger for each wind held, and every thrown thing flies back at its thrower. **New code**: catching thrown attacks and sending them back. | 8 s; four winds. | **Kept Wind**: the bag does not open by itself. It keeps its winds for 20 s, to be opened when you choose, such as on a staggered enemy. | **Loose Neck**: nothing is kept. Each wind leaves at once as a free Gust toward the enemy you next face. | Catch three winds within 3 s and the bag bursts by itself as a whirlwind: everything within 6 m goes into the air. |
| **Sky Dance** (built) | Breaker | 45 mana | As built: the enemy faced is thrown up and you go up beside it, untouchable. Press light attack as each of five rings closes, then the stick chooses where it is thrown down, bursting harder for each cut in time. | 35 mana; six rings. | **Carried Off**: there is no throw. You carry it down the lane with you, up to 8 m, and drop it on another enemy, and both take the burst. | **Two Rise**: the enemy faced and the nearest other both go up, and the rings take turns between them. | All five cuts in time, and you land with Tailwind lit for 4 s at no cost. |

**Seen from above.**
- Gust: streaks of dust and leaves driven one way (as built). Downburst is a flat ring of dust thrown out from a
  point, the way a microburst shows on dry ground.
- Lifting Wind: a leaning dust devil of grit and dry grass with a calm middle. Downdraft is a round print pressed
  into the dust.
- Indrawn Breath: curved lines of dust and leaves streaming inward, then thrown out as a ring.
- Closing Circle: a pale line of combed grass behind the runner that snaps inward (as built).
- Bag of Winds: a swelling hide bag at the hip with dust leaking from its neck. Caught arrows turn round the player
  in a ring of dust, and the opening throws a long fan of grit and leaves.
- Sky Dance: as built, with dust thrown off the floor where the enemy lands.

**Charged blade.** Dust and dry grass stream off the edge like a pennant. Each charged blow reaches 1 m further as a
cut of wind.

**Infused strike: Cutting Wind.** The swing throws a crescent of wind 7 m along its arc: 60% of the blow, 2 Unsteady
and a 1 m push.

**Devotion: On the Wind.** Your spells have no wind-up, and can be cast in the middle of a dodge or a swing without
ending it. A spell cast in a dodge carries you 2 m further along it. This changes play: a Wind player casts on the
move, and nearly all of the fight is spent in motion.
- *Deep strike (L2 with R2)*, **Whirl Cut**: the crescent becomes a small whirlwind that holds the first enemy it
  reaches in the air for 1 s.

**Reaction lift: Carried Far.** Every reaction with Unsteady in it reaches half again as far and throws what it
strikes 1.5 m out from where it happened.

**Passives** (with Devotion and the lift, ten marks):

| Mark | Kind | What it does | Built or new |
|---|---|---|---|
| Unsteady | Root | Blows knock an enemy Unsteady, up to five. | Built |
| Rising Cut | Major | A heavy blow on an enemy five deep in Unsteady throws it into the air. | Built; it is the launcher the Updraft play stands on |
| Passing Cut | Major | Where a dodge ends, a cut of wind strikes everything close and leaves it Unsteady. | Built |
| Turned Aside | Major | A perfect guard throws everyone near back, the attacker three deeper in Unsteady. | Built |
| Against the Wall | Major | Each enemy your spells strike is hit by a small burst of wind that shakes it and anything beside it. | Built; the design's wall collision is still wanted (**New code**) |
| Leaf on the Wind | Keystone | The dodge costs breaths of wind, not stamina. | Built |
| Nothing Beneath | Keystone | An enemy in the air stays there as long as you keep striking it. | Built |
| The Wind Remembers | Gift | While Tailwind lasts, every blow hangs where it fell, and all fall again when it ends. | Built |

**Surge.** Tailwind stays as built, 35 mana, in the surge slot. One change fits the new loop: while it lasts, a
perfect dodge takes 3 s off running cooldowns, not 1.5.

---

## Ice

**The image.** The huntress's mountain. In the Norse beginning the ice rivers ran out of misty Niflheim, and their
rime filled the gap until the first frost giant dripped out of it. She travels on skis with a bow and hunts. Real
rime grows as feathers of ice on the side that faces the wind, where freezing fog meets anything colder. Ice is
frost creeping in branching lines, shards, breath fog and clear ice with dark things inside it.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Hoarfrost** (built, tap) | Opener | 7 s | As built: thirteen crystals grow out over a fan 4.5 m and 110 degrees: 10 damage, 15 stagger, Chill two deep. They stand 4 s, and a weapon blow among them shatters them all for 5 damage and one more Chill. | 5.5 s; Chill three deep. | **Black Ice** (built major, folded in): no crystals. A sheet of clear ice covers the fan's ground for 5 s. Enemies on it gain 1 Chill a second and are thrown half again as far, and a dodge on it slides twice as far. | **Frost Line**: one branching line of frost creeps 10 m along the floor in 0.6 s, laying 2 Chill on all it crosses. It reaches the archer behind the shield. | Cast within 0.5 s after a shatter, and the crystals grow out from the shattered spot with Chill four deep. |
| **Eight Points** (built) | Detonator | 11 s | As built: eight shards stand on a 3.5 m ring round the enemy faced for 1.2 s, then fly in, 5 damage and 1 Chill each. Re-aimed as the detonator: as they close they spend its Chill, 5 more damage a stack, and on a Frozen enemy they shatter it. | 9 s; they close after 0.9 s. | **Wolf's Jaws**: two rows of four close from in front and behind in 0.5 s, too fast to walk out of. One target only. | **Eight on Eight**: one shard falls from above on each of up to eight enemies within 8 m, 8 damage each, each spending that enemy's Chill. For crowds. | Land a heavy blow on the enemy as the shards close (within 0.3 s), and every shard counts as a shatter. |
| **Hanging Ice** (built) | Breaker | 10 s | As built: a great icicle hangs over the enemy faced (within 7 m) and follows it, its shadow growing for 3 s, then falls in 2 m: 30 damage, 60 stagger, 3 Chill. On a Frozen target it shatters it for 90. | 8 s; it falls after 2.4 s. | **Icicles**: three smaller icicles fall 1 s apart along where it moves, each 12 damage and 25 stagger. It punishes running. | **Held by a Thread**: it does not fall by itself. It hangs up to 8 s and falls with your next heavy blow on its target. Weave it into the heavy. | If it lands within 0.3 s of the enemy becoming Frozen, it shatters it four times over and its cooldown comes back. |
| **Winter Wall** (built) | Breaker | 35 mana (Ice's larger spell) | As built: a wall of ice 5 m wide rises 2 m ahead for 6 s (60 health). It stops blows, and what touches it gains 1 Chill a second. Your heavy blow bursts it forward as a cone 6 m: 30 damage, 50 stagger, 3 Chill. | 30 mana; the burst does 36. | **Shield Wall**: a shorter wall, 3 m, that moves 2 m ahead of you at walking pace for 4 s. Advance behind it. | **Cracked Wall**: it bursts by itself 1.5 s after an enemy first strikes it. A trap that answers the enemy, not you. | Burst it with a heavy blow within 0.3 s of an enemy striking it, and the cone freezes everything it reaches. |
| **Rime Guard** (new) | Counter | 9 s | You breathe out a low freezing fog round you, 3 m, for 6 s. It is stippled and thin, so you stay seen. An enemy that strikes at you inside it gains 2 Chill, rime grows on its weapon arm, and its next attack winds up half again as slowly. A perfect guard inside it freezes the attacker at once. **New code**: slowing one enemy's next wind-up. | 7 s; the fog reaches 4 m. | **Freezing Fog**: the fog stays where it was breathed, 5 m wide, and everything in it gains 1 Chill a second. Hold a door. | **Breath on the Steel**: no fog. The rime goes on your guard for 6 s: every blow guarded lays 2 Chill on the attacker, and every perfect guard freezes it. | A perfect guard in the fog's first second freezes everything in the fog, not only the attacker. |
| **Ski Run** (new) | Movement | 8 s | A lane of clear ice runs 9 m ahead and you slide down it at once in 0.5 s, untouchable for the first 0.2 s, throwing snow. Enemies on the lane take 2 Chill. The lane stays 5 s, and a dodge along it carries twice as far. Form: a step with a ground hazard left. | 6 s; the lane is 12 m. | **Left Standing** (Hollow Statue, folded in): you slide 4 m back and leave a figure of ice where you stood, and enemies near it attack it. Struck by them, it bursts for 15 damage and 3 Chill in 3.5 m. Struck by your heavy blow, it bursts forward as a cone 7 m, 22 damage and 5 Chill. | **Shooting Run**: while you slide you loose three shards of ice at the nearest enemy, 8 damage and 1 Chill each, as she shoots from her skis. | Shared movement rule, and the attacker's feet freeze to the floor for 1 s. |

**Seen from above.**
- Hoarfrost: crystals growing out over the fan (as built). Frost Line branches like a crack of white over the floor.
- Eight Points: eight long shards on a ring, breath-fog curling off them. Wolf's Jaws closes like teeth.
- Hanging Ice: a spear-long icicle, dripping, its shadow swelling on the snow.
- Winter Wall: clear blue ice with dark shapes caught inside it and rime feathers on one face.
- Rime Guard: a low white fog rolling over the floor. Rime grows as white feathers on the side of the attacker that
  faced the player.
- Ski Run: a long clean ski line with snow sprayed out at its edges.

**Charged blade.** Rime feathers stand on the edge and breath-fog trails it. A charged blow on a Frozen enemy
shatters it, light blow or heavy.

**Infused strike: Frost Bite.** The blow plants ice. A spike of ice stands up through the enemy struck: 2 Chill, and
it is held to the spot for 0.8 s. Your next blow on it breaks the spike for 15 damage in 2 m.

**Devotion: Deep Winter** (the name the design gives). Chill never fades. Lighting Glaze freezes every enemy within
4 m at once. This changes play: no Chill is ever wasted, so an Ice player can spread it over a whole room and come
back for each in turn.
- *Deep strike (L2 with R2)*, **Ice Through**: the spike freezes the enemy at once if it carries three Chill or more.

**Reaction lift: Cold Settles.** Every reaction with Chill in it leaves frozen ground where it happened (3 m, 4 s,
1 Chill a second), and what it struck is 40% slower for 2 s.

**Passives** (with Devotion and the lift, ten marks):

| Mark | Kind | What it does | Built or new |
|---|---|---|---|
| Chill | Root | Blows chill an enemy, up to five; at five it is Frozen, and a heavy blow shatters it. | Built |
| Cold Read | Major | A perfect guard chills the one who struck, two deep. | Built |
| Flying Shards | Major | A shattered enemy throws its ice wide, chilling all it reaches two deep. | Built |
| Brittle | Major | An enemy that thaws or is shattered takes more from blows for a while. | Built |
| Splinter Ward | Major | When Glaze breaks it bursts round you and gives what a perfect guard gives. | Built |
| Unmoved | Keystone | Nothing breaks your heavy blows. Your dodge is short and costs more. | Built |
| All at Once | Keystone | Shattering strikes three times as hard. Chill no longer slows. | Built |
| The World Holds Still | Gift | With Glaze standing, press the surge button again to break it outward and freeze all it crosses. | Built |

**Surge.** Glaze stays as built, 40 mana, in the surge slot. One change fits the loop: a blow turned by the ward
counts as a perfect guard, so it takes 2 s off every cooldown.

---

## Water

**The image.** The fisher of islands. He fished land up from the sea on a hook, snared the sun with ropes of flax
to slow it, and swung across the beach on his line. The sea he stands on hides the octopus, and its ink is black
water from the deep. Real water cracks: the snapping shrimp shuts its claw so fast that the water tears into a
bubble, and the bubble collapses with a crack. Water is spray, foam, swell and ink.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Riptide** (built, tap) | Opener | 6 s | As built: a wave rises 5 m ahead and rolls back toward you. Everything in its path takes 8 damage and 18 stagger, is Soaked, and is carried to your feet, within reach. | 4.5 s; it rises 6.5 m away. | **River Serpent** (built spell, folded in): a serpent of water goes 8 m out along a lane and back, and brings only the first enemy it takes, dropping it 1.5 m in front of you with its guard down for 1 s. One target, from further. | **Breaking Swell**: the wave does not come back. It rolls 6 m away from you and breaks, throwing everything in it down 3 m off. The shape that makes room. | Cast within 0.5 s after a perfect guard, and the wave carries the attacker past you and drops it behind you, back turned. |
| **The Tide Turns** (new; it replaces a hook-and-line spell that repeated the Water god's Bone Hook) | Movement | 8 s | Press: the ebb draws you 5 m straight back in 0.3 s, untouchable, out of reach, and leaves a line of wet sand where you went. Press again within 3 s: the flood brings you back along the line and 3 m past where you began as a breaking wave. Everything on the line takes 10 damage, is Soaked and is carried 2 m ahead of you. Pressed once, it is the ebb alone. Form: a step, then a second press (the land's rule of the tide, in the hand). | 6 s; the flood runs 5 m past. | **King Tide**: the flood comes in 4 m wide and throws what it carries down for 0.6 s, but it stops where you began. The shape for a crowd. | **Out to Sea**: there is no flood. The ebb takes the enemy faced with you: it is drawn the 5 m out with you, Soaked, and dropped at your feet, away from its friends. It changes what the spell is for: a pull, not a return. | Shared movement rule, and a flood pressed in the instant a blow would land starts no cooldown. |
| **Moon on Water** (built) | Counter | 10 s | As designed: you stand open for up to 3 s on a still pool 3 m across. Every blow that comes, red or not, is turned and adds a ring to the pool, five at most, and each costs 15 stamina in place of the old mana. Let go: one cut in a 4 m arc of 200 degrees, 14 damage and 22 stagger a ring, Soaked. | 8 s; 18 damage a ring. | **Still Water**: the first blow only. It is turned and answered at once with a thrust of water 6 m straight through the attacker. A quick parry, not a store. | **Undertow Cut**: on release everything within 6 m is first dragged to you, then the cut lands. | Let go in the same instant as a blow lands on the pool (within 0.2 s), and the cut is doubled and the pool stays for a second round. |
| **Whirlpool** (built) | Breaker | 10 s | As built: a whirlpool opens under the enemy faced (within 6 m), 4 m across, for 5 s. Enemies in it are drawn to the middle at 1.2 m a second, kept Soaked, and bitten for 3 twice a second. Breaker: their balance does not recover inside it, and your blows there break balance 30% harder. | 8 s; it lasts 7 s. | **Maelstrom**: smaller (2.5 m) and pulling hard (3 m a second). At its end it throws everything inside up in a spout and down again for 50 stagger. | **Standing Rain** (built spell, folded in): no pull. Rain falls over 7 m where it was cast for 10 s; everything in it stays Soaked, and your blows there take twice as much off running cooldowns. | Open it under an enemy in the instant its attack mark shows, and the attack is lost as it goes in to the knees. Its cooldown is halved. |
| **Deep Crack** (new) | Detonator | 11 s | A fist of water snaps shut on the enemy faced (within 7 m). A bubble as big as a body forms on it and collapses 0.4 s later with a crack: 18 damage and 40 stagger. On a Soaked enemy the water in it boils out: its Soaked is spent, and every other Soaked enemy within 5 m cracks with it (18 damage and 30 stagger each). Form: a mark that detonates, set off by the affliction (**New code**: spending Soaked). | 9 s; 22 damage. | **Ink**: the bubble is black water from the deep. It bursts into a pool of ink 3 m across that stays 5 s. Enemies in it lose track of you beyond 3 m and stay Soaked. You stay in plain sight. | **String of Bubbles**: three bubbles in a row, 2 m apart along the line you face, each cracking 0.15 s after the last. Aim down a corridor. | Snap it as your heavy blow lands on the same enemy (within 0.3 s), and every Soaked enemy in the fight cracks. |
| **Poured Blade** (built) | Breaker | 30 mana | As built: a copy of you in water stands at your side for 12 s and repeats each blow a beat late, for 30% of its damage and all of its stagger, Soaked. | 25 mana; 35% for 15 s. | **Poured on the Edge** (its first design): no copy. A flask is poured along the weapon, and for 12 s its reach is 40% longer, every blow mends 2 and soaks, and the last blow of a chain throws a wave 5 m. It costs a flask, not mana. | **Brother at Your Back** (the fisher's brothers): the copy stands behind you facing the other way and repeats your blows on whatever is behind you. For being surrounded. | Make a perfect dodge while it stands, and the copy steps into the blow you left and answers the attacker at full strength. |

**Seen from above.**
- Riptide: a translucent curling wave with a white lip and foam left on the floor (the "Not yet" in GUIDE.md asks
  for this).
- The Tide Turns: water drawing back off wet sand with a hiss of foam, then a white-lipped wave running in over it.
- Moon on Water: a dark still pool reflecting the player, a white ring added for each blow turned.
- Whirlpool: spiralling lines of foam turning round green-black water at the eye.
- Deep Crack: a ring of white bubbles that pulls in to a point, then spray thrown flat across the floor. Ink is a
  spreading stain of black water with an oily edge.
- Poured Blade: a figure of clear running water with foam at its edges, which splashes apart when it ends.

**Charged blade.** A running film of water covers the blade and throws drops off the point. Each charged blow
splashes everything within 1.5 m of its target for 20% of the blow and soaks it.

**Infused strike: Lash.** The swing trails a whip of water twice the weapon's reach: 80% of the blow, Soaked, and the
enemy struck is pulled 1.5 m in.

**Devotion: The Sea Comes In.** Every spell leaves standing water where it lands, a pool 4 m across, for 10 s (four
at most). Enemies in water are always Soaked and 15% slower. In water your dodge slides half again as far and your
cooldowns run 30% faster. This changes play: a Water player draws the fight onto their own water and stays there,
as the tide takes the beach. It echoes the land's rule of the tide.
- *Deep strike (L2 with R2)*, **Reeled In**: the lash wraps round the enemy, reels it to your feet, and its guard is
  down for 1 s.

**Reaction lift: Spreading Water.** Every reaction with Soaked in it runs on at half strength to every other Soaked
enemy within 4 m.

**Passives** (with Devotion and the lift, ten marks):

| Mark | Kind | What it does | Built or new |
|---|---|---|---|
| Soaked | Root | Blows leave an enemy Soaked; spells strike it harder, and Static or Chill laid on it spread. | Built |
| Ebb and Flood | Major | A perfect guard brings the Flood, a heavy blow the Ebb. | Built |
| Undertow | Major | A heavy blow drags every enemy near a step toward you. | Built |
| Heavy Water | Major | Soaked enemies are slow to recover after they strike. | Built (designed form; built form breaks balance sooner) |
| Broken Gourd | Major | Hold the flask button to throw it; it leaves a pool that keeps all in it Soaked. | Built |
| Catch the Rain | Major | A perfect guard fills a little of an empty gourd. | Built |
| Deep Well | Keystone | One more flask, but a flask fills a well that mends you only while you land blows. | Built |
| The Sea Rises | Gift | While Wellspring lasts, flood the ground, then break the sea where you face. | Built |

**Surge.** Wellspring stays in the surge slot at 30 mana, but "flasks heal half again" is the weakest surge in the
game and asks to be held for an emergency. Add: a flask drunk while it lasts takes 4 s off every running cooldown,
so drinking is a way back into the fight.

---

## Life

**The image.** The antlered lord's grove, where wood forgets and grows back. The Dagda's club killed with one end and
raised the dead with the other. The druids cut mistletoe from the oak with a golden sickle. Real plants fight: the
squirting cucumber bursts and throws its seeds, the strangler fig climbs its host, and mistletoe drinks from the tree
it grows on. Life is vines, roots, thorns, leaves, spores and flowers that open and close.

**Roster.**

| Spell | Role | Cooldown or mana | Learn | Empower | Shape A | Shape B | Master |
|---|---|---|---|---|---|---|---|
| **Briar Patch** (built, tap) | Opener | 7 s | As built: a bed of thorned vines 2.5 m across grows 3 m ahead and stands 6 s. It lashes twice a second for 4, Seeds what stands in it, and slows it by a fifth. It counts as grown. | 5.5 s; 3.3 m across. | **Thorn Ring** (built spell, folded in): a ring of thorns 9 m across round you and the enemy faced for 7 s. Anything touching it is cut for 8, Seeded and thrown back. A duel on demand. | **Seed Pods**: no bed. Three pods are thrown in a fan to 6 m; they swell, and 1 s later each bursts in a spray of seeds over 3 m (12 damage, 2 Seeded) and leaves a small bush, as the squirting cucumber throws its seeds. | Cast within 0.5 s after a perfect guard, and it grows under the attacker and roots it there for 1 s. |
| **Sapling** (built major, now a spell) | Counter | 9 s | For 8 s, each perfect guard or perfect dodge plants a sapling where the attacker stands, three at most. Each stands 12 s and every 1.5 s lashes the nearest enemy within 3.5 m for 6 damage and 8 stagger, Seeding it. They count as grown. | 7 s; four saplings. | **Strangler**: each one climbs the attacker instead, as a strangler fig climbs its host. It is rooted 1 s and Seeded two deep, and nothing stands after. | **Dodder**: the seed grows on the attacker itself, pale threads of dodder that drink from it for 6 s. Each of your blows on it mends you 3 more, and its balance breaks 20% sooner. | Plant all three within 3 s and the third comes up as a full Elder Tree for 6 s. |
| **Root Walk** (new) | Movement | 8 s | You sink into the ground in a swirl of leaves and come up 0.5 s later at a grown thing of yours within 12 m (the one nearest where you aim), or 6 m ahead if there is none. Where you went down a sapling sprouts for 6 s. Arriving, roots burst round you: 10 damage, 15 stagger, Seeded, in 2.5 m. Untouchable while under. Form: a step. | 6 s; the burst lays 2 Seeded. | **Up Through Them**: go to a Seeded enemy instead, and come up under it, lifting it on roots for 0.6 s. | **Changeling**: you change places with the grown thing, so it stands where you stood, in the path of the blow aimed at you. | Shared movement rule, and the sapling left behind is a full thornbush. |
| **Reap What Is Sown** (built) | Detonator | 12 s | As built: everything you have grown bursts (3 m, 16 damage, 25 stagger), every Seeded enemy's thorns burst now and its seed is spent, and you mend 5 for each burst. | 10 s; you mend 7 a burst. | **Golden Sickle** (the druids' sickle): only the Seeded within 8 m are reaped, each cut once by a sweep of gold, and the garden is kept standing. | **Second Sowing**: everything bursts as the Learn says, and each burst sows a new seed in all it reaches. The loop keeps going. | Cast within 0.5 s of a Seeded enemy dying, and every burst counts twice. |
| **Elder Tree** (built) | Breaker | 40 mana | As designed: a tree takes 3 s to grow (40 health while it grows, and enemies turn on it). Grown, for 15 s a ring 6 m across lies under its crown. Enemies inside stay Seeded, you mend 3 a second inside, and every 3 s a root comes up under one enemy inside, a staggered one first: 16 damage, 45 stagger. | 32 mana; it grows in 2 s. | **Walking Tree**: the tree pulls up its roots and follows you at walking pace, with a ring 4 m across. The stag grown through with forest. | **Grown at Once**: no growing. It comes up full in an instant and stands only 6 s. Setup becomes burst. | Plant it within 0.5 s after a finisher, and it grows at once. |
| **Red Harvest** (built) | Breaker | 12 s, paid in health | As designed: kneel 0.5 s and give a quarter of your present health. A ring 5 m across follows you for 8 s. Every weapon blow is critical and mends 20% of its damage. If you win back more than you gave, thorns burst at the end: 5 m, 30 damage, 50 stagger, Seeded. | 10 s; the thorns do 40. | **Blood in the Furrow**: the ring stays where you knelt, 8 m across, and everything in it is Seeded each second. Fight in a place, not round yourself. | **Feed the Ground**: the health goes to your garden. For 8 s everything you have grown within 12 m strikes twice as hard and twice as often. | Cast within 1 s of being struck, and it is paid from the grey health that blow left, not from real health. |

**Seen from above.**
- Briar Patch: thorned vines and flowers (as built). Seed Pods are green pods that swell and split with a spray of
  seeds and sap.
- Sapling: a whip-thin sapling that lashes. Strangler is roots climbing the enemy's legs. Dodder is a tangle of pale
  orange threads wound over the enemy's shoulders.
- Root Walk: the floor heaves along a line of roots between where the player went down and where they come up,
  with leaves thrown up at both ends.
- Reap What Is Sown: every grown thing flowers in one instant and bursts into thorns and petals. Golden Sickle is
  one thin gold sweep with cut leaves falling.
- Elder Tree: shoot, sapling and oak in three steps, with roots heaving the floor and leaves falling under the
  crown.
- Red Harvest: red sap running into the ground at the knees, and red flowers opening round the ring.

**Charged blade.** A green vine winds round the blade with a flower at the point, and a petal falls with each blow.
Each charged blow mends you 3.

**Infused strike: Thorn Set.** The blow leaves a thorn in the enemy: 2 Seeded. For 6 s each of your blows on that
enemy mends you 2.

**Devotion: Wood Grows Back** (the land's rule "wood forgets"). Whatever you grow grows back where it stood 6 s
after it is spent, burst or cut down. Every spell you cast sows a sapling where it lands. This changes play: the
garden never runs out, so Reap What Is Sown and Root Walk can be spent freely, and every fight becomes a forest.
- *Deep strike (L2 with R2)*, **Briar Root**: the thorn grows into a briar round the enemy's feet. It is rooted for
  1 s, and the briar stays as something grown.

**Reaction lift: Root and Branch.** Every reaction with Seeded in it mends you 4 and sows a seed in everything it
reaches.

**Passives** (with Devotion and the lift, ten marks):

| Mark | Kind | What it does | Built or new |
|---|---|---|---|
| Seeded | Root | Blows plant a seed that mends you as you strike, and it bursts into thorns at death. | Built |
| Ripening | Major | A heavy blow on a Seeded enemy bursts its seed at once and mends you. | Built |
| Fallen Seeds | Major | Where a Seeded enemy dies, a thornbush grows. | Built |
| Bitter Rind | Major | A plain guard seeds the attacker; a perfect guard roots it for a moment. | Built |
| Briar Coat | Major | An enemy that strikes you, or whose blow you guard, is cut by thorns and Seeded. | Built |
| Thin Blood | Major | Below a third of your health, blows strike harder and mend more. | Built |
| Wildwood | Keystone | What you grow stands three times as long, more of it. Far from your garden, blows are weaker. | Built |
| First Spring | Gift | While Regrowth lasts, give half your health, and a forest holds every Seeded enemy. | Built |

**Surge.** Regrowth stays as built, 40 mana, in the surge slot. One change fits the loop: each spell cast while it
lasts mends 6, so it rewards casting, not hiding.

---

## What to cut from the built content

1. **Every "costs less mana" minor on a basic spell** (Short Breath, Quick Clap, Easy Breath, Thin Ice, Low Water,
   Ready Soil). They mean nothing once basic spells are on cooldowns. Empower takes their place.
2. **Spring Tide** (Water keystone: every spell spends all your mana). It is broken by cooldown spells. Cut it, or
   aim it only at the mana spells and the surge.
3. **Paid in Blood and Heartwood** (Life). Both rest on spells costing mana. Red Harvest now carries "health as the
   price". Cut both, or aim Paid in Blood at the surge alone.
4. **The clone in Red Harvest as built.** It duplicates Poured Blade's water copy. Build Red Harvest's own design
   (a health price, critical blows and mending) and keep one clone in the game, the water one.
5. **Kindle's "every blow sets a burn".** It repeats Cinder (above, under Fire's surge).
6. **Live Wire as built** (any spell cast lays a line, once every 6 s). It goes off nearly all the time once spells
   are cast every few seconds. Cut it, or bring back its design (a wire from the rod) as part of Lightning Rod.
7. **Built spells folded in, not kept on their own**: Cinder Fan, Lodestone, River Serpent, Standing Rain, Hollow
   Statue, Thorn Ring and Black Ice become shapes; Sapling becomes a spell; Deep Reserve and Burn and Detonate merge
   into Stored Charge and Backdraft.
8. **Weapon-bound marks** (Burning Crescent, Delayed Cuts, Drawing Wind, Winter Draw, and Halo's Answering Draw)
   leave the element trees. They belong on the Katana's Way as element infusion marks (`weapons.md`, Katana,
   Infusion). **Full Body Armament** leaves every tree too: it is now the Fists' third aspect, The Armament.
9. **The minors** (about fourteen a tree) go. Empower marks and Bond ranks carry the plain number growth.
10. **Keystones not carried over**: Open Furnace, Scorched Earth (its "spells cost more" means nothing now),
    Overclock, Storm Front, Second Skin, River Without End, Shared Roots. Each tree keeps two keystones beside
    Devotion, so the all-in passive is the one that stands out.

---

## Sources

- Eye of Ra and the uraeus spitting fire; Sekhmet's breath as the desert wind; Apep and the lakes of fire:
  [Sekhmet, Wikipedia](https://en.wikipedia.org/wiki/Sekhmet),
  [Apophis, Wikipedia](https://en.wikipedia.org/wiki/Apophis),
  [Lecture: the Eye of Ra](https://egyptianocculthistory.blogspot.com/2017/11/lecture-eye-of-ra.html).
- Raijin's ring of drums, Fujin's bag of winds and the thunder beast Raiju:
  [Raijin, Wikipedia](https://en.wikipedia.org/wiki/Raijin),
  [Raijū, Wikipedia](https://en.wikipedia.org/wiki/Raij%C5%AB),
  [Fūjin and Raijin, Japonista](https://japonista.com/fujin-raijin).
- Māui, Mahuika's fire and the snaring of the sun:
  [Māui, Wikipedia](https://en.wikipedia.org/wiki/M%C4%81ui),
  [How Māui Slowed the Sun](https://en.wikipedia.org/wiki/How_M%C4%81ui_Slowed_the_Sun),
  [Mahuika](https://en.wikipedia.org/wiki/Mahuika).
- Skadi on skis with the bow; Élivágar, Niflheim and the rime; Fimbulwinter:
  [Skadi, Norse Mythology for Smart People](https://norse-mythology.org/gods-and-creatures/giants/skadi/),
  [Élivágar](https://en.wikipedia.org/wiki/%C3%89liv%C3%A1gar),
  [Niflheim](https://en.wikipedia.org/wiki/Niflheim),
  [Fimbulwinter](https://en.wikipedia.org/wiki/Fimbulwinter).
- The Dagda's club and cauldron; Cernunnos; the Green Man:
  [The Dagda, Wikipedia](https://en.wikipedia.org/wiki/The_Dagda),
  [Cernunnos, History Cooperative](https://historycooperative.org/cernunnos/).
- Backdraft and smoke explosion (Kiln Stance's Shut Door):
  [Flashover, Wikipedia](https://en.wikipedia.org/wiki/Flashover),
  [Backdraft, Karel Lambert's review](http://cfbt-be.com/images/artikelen/LAMBERT_backdraft.pdf).
- Fire whirls (Wildfire Rush's Fire Whirl):
  [Fire whirl, Wikipedia](https://en.wikipedia.org/wiki/Fire_whirl),
  [Review of vortices in wildland fire, USFS](https://www.fs.usda.gov/rm/pubs_other/rmrs_2011_forthofer_j001.pdf).
- The stepped leader, return stroke and St Elmo's fire (Chain Lance, Lightning Rod, Strikes Twice):
  [NOAA JetStream, the lightning process](https://www.noaa.gov/jetstream/lightning/how-lightning-is-created/jetstream-max-lightning-process-keeping-in-step),
  [HyperPhysics, lightning sequence](http://hyperphysics.phy-astr.gsu.edu/hbase/electric/ligseq.html).
- Downbursts as rings of dust, and dust devils (Gust's Downburst, Lifting Wind):
  [Microburst Handbook](http://www.cimms.ou.edu/~doswell/microbursts/Handbook.html),
  [Gustnado, Wikipedia](https://en.wikipedia.org/wiki/Gustnado).
- Rime, hoarfrost, frost flowers and frost heave (Rime Guard, Frost Line):
  [Rime ice, Wikipedia](https://en.wikipedia.org/wiki/Rime_ice),
  [Met Office, frost and ice](https://weather.metoffice.gov.uk/learn-about/weather/types-of-weather/frost-and-ice),
  [Frost heaving, Wikipedia](https://en.wikipedia.org/wiki/Frost_heaving).
- Snapping shrimp cavitation (Deep Crack):
  [Claw motion and cavitation bubbles of snapping shrimp](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC7525291/),
  [Vortex formation with a snapping shrimp claw](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC3828329/).
- The squirting cucumber and serotiny (Seed Pods):
  [Oxford, the squirting cucumber](https://www.ox.ac.uk/news/2024-11-26-new-study-reveals-explosive-secret-squirting-cucumber),
  [PNAS, mechanics of the squirting cucumber](https://www.pnas.org/doi/10.1073/pnas.2410420121),
  [Serotiny, Wikipedia](https://en.wikipedia.org/wiki/Serotiny).
- Cooldowns cut by timing (Masters, In One Breath) and mana-paid charged moves:
  [Final Fantasy XVI Eikon guide](https://finalfaninsider.com/game-guides/final-fantasy-xvi-eikon-guide/),
  [Hades II combat](https://hades2.wiki.fextralife.com/Combat),
  [Hades II boons](https://hades.fandom.com/wiki/Boons/Hades_II).
- In the project: `docs/GUIDE.md` sections 4 to 7, `docs/design/trees_a.md`, `docs/design/FORMS.md`,
  `docs/design/WORLD.md`, `docs/design/LOOKS.md`, `docs/UPGRADE_SCHEMA.md`, `docs/design/weapons.md`.
