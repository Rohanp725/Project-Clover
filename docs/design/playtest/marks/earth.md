# Earth: detailed explanations

The long explanation the chart shows for each high-level mark of the Earth tree (the root, the passives, every spell
branch's Learn, Shape and Master, the Devotion, the reaction lift and the god's gift). Reshaped into spell branches in
Phase 4 (`docs/design/steel/elements_b.md`, Earth): each spell is four marks, Learn, Empower, Shape and Master. An
Empower is a plain number (the chart's own words say it), so it has no entry here. Where a mark does less than its
design yet, its **Not built yet** line says what is missing. "Your stones" means anything of stone you raise: Stone
Rise's rocks, a pillar, a heap of rubble.

## earth.tremor
**What it does.** Your heavy blows shake the ground under an enemy and lay Tremor, and a finisher lays all of it at once. A shaken enemy is slowed, and its balance does not come back while the shaking lasts.

**When it happens.** Every heavy blow that lands adds a stack; every finisher fills it. Like every root, it lays with the weapon only while Earth is your first element; carried second, Earth's Tremor comes from your spells, the charged blade and the R2 strike.

**The numbers.** 2 Shards. One stack a heavy blow, three on a finisher, three at most. Tremor lasts 6 seconds; each stack slows the enemy by 10%, and while it lasts its stagger meter does not drain.

**Works with.** Heavy Hand and The Slow Hand (more heavy blows, or every blow shaking), Cairn (it spends Tremor for stones), Left in Rubble, Bedrock (one more stack with every blow), Buckled on the Forge branch, and The Ground Gives when Soaked meets it.

**Not built yet.** Earth's base buff from GAME.md, afflictions laid on you lasting half as long: nothing lays an affliction on the player yet.

**Example.** A heavy blow on a brute leaves its balance bar stuck where you left it: step back, guard its swing, and come in again with the bar still waiting.

## earth.heavyhand
**What it does.** Nothing stops your heavy blows. Struck while winding one up or swinging it, you still take the blow, but less of it, and yours still lands. Red attacks cannot stop it either.

**When it happens.** Always on once learned: any blow that lands on you during a heavy blow's wind-up or swing.

**The numbers.** 3 Shards. You take 35% less from a blow that lands then, and you do not flinch.

**Works with.** Tremor, The Slow Hand, Under the Mound and Cairn, which grow from it, Hand of the Mountain, Hammer and Anvil on the Forge branch.

**Example.** Two soldiers swing at you as you wind up: you take both, lightly, and your heavy blow lands on the first anyway.

## earth.mound
**What it does.** Under the Mound, the movement spell of the tree. You sink into the ground and travel as a hump of heaving turf along the stick, out of reach, and burst out in a ring of thrown sod and stones that shakes everything round you.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 8, and 10 metres). Up to 8 metres; where you come out, 12 damage, 25 stagger and one Tremor. As built it is a step through the enemy faced (8 metres ahead with none), and if it lands, your next heavy blow within 4 seconds is critical.

**Works with.** Heavy Hand, Tremor and Cairn (it shakes what it reaches), The Slow Hand, the charged blade: it charges your weapon with Earth when it lands.

**Not built yet.** Going under the ground along the stick, untouchable on the way, and the ring of sod 2.5 metres round where you come up: for now a step through the enemy faced that strikes as you pass.

**Example.** An archer behind a line of shields: go under them and come up at its back, shaken and open to your heavy blow.

## earth.mound.shape
**What it does.** Opens two shapes of Under the Mound, chosen at any rest fire. **Ditch and Bank**, the old earthworks: the turf you heave stays up behind you as a bank a metre high along your whole path, nothing walks through it, and what stands on the line as it rises is thrown off it, shaken. You draw a wall as you go. **Barrow Road**: hold to stay under and steer; come up under an enemy and it is thrown up as by Stone Rise, and you stand behind it.

**When it happens.** Whenever you cast Under the Mound, as the shape it wears. Before this is bought, the plain spell is cast.

**The numbers.** 3 Shards. Ditch and Bank: a line of six patches 8 metres long from where you went under, 0.6 metres round each, for 5 seconds, biting once a second for 4 damage, 10 stagger and one Tremor. Barrow Road: under for up to 1.5 seconds; what you come up under is thrown up for 0.6 seconds.

**Works with.** Ditch and Bank: Quake Step and Cairn on what stands on the bank. Barrow Road: Under the Mound's own Master, your heavy blows on what hangs in the air.

**Not built yet.** The bank blocking bodies, and enemies thrown off it as it rises: for now it is ground along your path that bites and shakes. Barrow Road's hold and steer: for now the spell goes where it goes, and only the throw is built.

**Example.** Ditch and Bank across a doorway: the pack that follows you through it comes out shaken.

## earth.mound.master
**What it does.** Go under in the instant a blow would land on you, as a perfect dodge, and you come up at once behind the attacker. Your next blow lands as a heavy blow, with two Tremor, and breaks half again as much balance.

**When it happens.** For now, every cast of Under the Mound. It must be in a slot.

**The numbers.** 3 Shards. Each cast gives what a perfect dodge gives: 12 mana and 1.5 seconds off every running spell and strike clock. For 3 seconds, until your next blow lands, your blows break 50% more balance and each lays one stack more.

**Works with.** Tremor (the next blow's extra stack), Heavy Hand and The Slow Hand, Cairn (the mana it gives back).

**Not built yet.** The shared movement rule's timing, started as a blow would land, and coming up behind the attacker; the next blow landing as a heavy blow: for now every cast gives the dodge's reward and the stronger next blow.

**Example.** A brute's overhead is falling: go under it, come up behind, and the next blow nearly breaks it.

## earth.cairn
**What it does.** Cairn, Earth's larger spell and its detonator. You stamp the weapon's butt and the Cailleach's stones fall from her apron onto every shaken enemy near you, one stone for each depth of Tremor it carries. The Tremor is spent, and the stones stay heaped on it, slowing it.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). It costs mana, not a wait.

**The numbers.** 4 Shards, 40 mana (its Empower: 35). Every enemy within 10 metres carrying Tremor, 0.3 seconds after the cast: a stone falls for each depth, 12 damage and 15 stagger each (three stones on a full Tremor, 36 and 45), and 10 more damage for the heap. The heap stays 4 seconds and slows the enemy by 30%. An enemy with no Tremor is not touched.

**Works with.** Tremor and everything that lays it: heavy blows, finishers, Quake Step, The Slow Hand. Unbroken Cairn and your heavy blows, Rooted for its Master.

**Not built yet.** The slow as 15% a stone (for now 30% whatever the depth), and the Empower's 14 a stone (for now 12).

**Example.** Three soldiers each shaken three deep: one stamp drops nine stones and leaves all three slowed and with their shaking spent.

## earth.cairn.shape
**What it does.** Opens two shapes of Cairn, chosen at any rest fire. **Unbroken Cairn**: the stones land soft and wait, and your next heavy blow on a heaped enemy breaks its cairn outward into everything near it. The burst waits for your blow. **Thrown Stones**: the stones do not stay; each enemy's heap is flung at once at the nearest other enemy, so one shaken enemy shakes the next.

**When it happens.** Whenever you cast Cairn, as the shape it wears. Before this is bought, the plain stones fall.

**The numbers.** 3 Shards. Unbroken Cairn: a heavy blow on a heaped enemy within 4 seconds bursts 2.5 metres round it, 36 damage and 20 stagger. Thrown Stones: from each heaped enemy a stone flies to the nearest other enemy within 5 metres, 24 damage, 10 stagger and two Tremor.

**Works with.** Unbroken Cairn: Heavy Hand and The Slow Hand. Thrown Stones: Cairn cast again on what the stones shook.

**Not built yet.** Unbroken Cairn's stones landing soft (for now they still fall at the cast) and its burst as 18 a stone (for now 36 whatever the depth). Thrown Stones' heaps flung in place of staying (for now they still fall and heap) and a stone for each depth (for now two stones' worth).

**Example.** Unbroken Cairn on a pack round a brute: one heavy blow on the brute breaks its heap through all of them.

## earth.cairn.master
**What it does.** Cast it after standing still for a second, and every stone strikes twice and half the mana comes back.

**When it happens.** For now, when you cast it while Rooted (no dodge begun for 3 seconds). Cairn must be in a slot.

**The numbers.** 3 Shards. 20 mana back. A second fall 0.8 seconds after the cast on each shaken enemy, 24 damage and 30 stagger.

**Works with.** Rooted, Immovable, Heavy Hand; Mountain Stance, which keeps you standing.

**Not built yet.** Standing still for a second as the moment (for now Rooted, three seconds without a dodge), and every stone striking twice (for now a second fall of two stones' worth).

**Example.** Hold your ground through a soldier's chain, then stamp: twice the stones, and half the price back for the next.

## earth.slowhand
**What it does.** The Slow Hand, a keystone. Every swing is slower and costs more stamina, but your light blows hit like heavy ones, and every blow shakes its enemy.

**When it happens.** Always on once learned. The Tremor on every blow comes only while Earth is your first element.

**The numbers.** 6 Shards. Swings 22% slower; blows cost 20% more stamina. Light blows deal 35% more damage and 60% more stagger. Every weapon blow lays one stack of Tremor.

**Works with.** Tremor, Heavy Hand, Cairn (full Tremor everywhere to spend), Under the Mound's Master, Bedrock.

**Not built yet.** Light blows counting as heavy: for now they pass no guard, force no flinch, and set off nothing that waits on a heavy blow.

**Example.** Three light blows on a soldier leave it three deep in Tremor and close to staggered, without a heavy blow thrown.

## earth.rooted
**What it does.** Fight for a few breaths without rolling and you are Rooted. Rooted, you take less from every blow and holding your guard costs less stamina. Starting a roll ends it, and the count begins again.

**When it happens.** Always on once learned. You are Rooted once no dodge has begun for 3 seconds.

**The numbers.** 3 Shards. Rooted: 8% less damage taken, and a blocked blow costs 30% less stamina.

**Works with.** Quake Step and Mountain Stance, which grow from it, Immovable, Heavy Hand, the Masters of Stone Rise and Cairn (both ask you to stand your ground), and Bedrock (20% less damage again while it burns).

**Not built yet.** Nothing yet stops you being pushed back while Rooted.

**Example.** Plant your feet in front of a duellist and block its chain: each blocked blow costs less stamina and the grey it leaves is smaller.

## earth.quakestep
**What it does.** Quake Step, a breaker. You raise a foot and bring it down, and the ground heaves all round you and shakes everything it strikes. What it strikes flinches: the attack it had begun is broken, unless it is steadfast.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 9 seconds after a cast, less with weapon play (its Empower: 8, and 6 metres). It lands 0.45 seconds after the cast, 5 metres round you: 16 damage, 30 stagger and two stacks of Tremor on each enemy, and each flinches for 0.4 seconds.

**Works with.** Tremor and Cairn (two stacks on everything round you), Rooted, Standing Stones (a pillar on what it strikes), Buckled on the Forge branch. Landing it charges the blade with Earth.

**Not built yet.** The double stagger on an enemy showing an attack, and its attack broken only then: for now everything it strikes flinches, and the stagger is the same for all.

**Example.** Four soldiers close round you: stamp, and all four are two deep in Tremor with their balance bars stuck.

## earth.quakestep.shape
**What it does.** Opens two shapes of Quake Step, chosen at any rest fire. **Struck From Afar**, the Cailleach striking the ground with her staff: the stamp lands not round you but on the shadow of a far enemy, breaking its attack where it stands and holding it a moment. You break the archer's draw without crossing the room. **The Second Wave**: one stamp sends out two rings, the quick thin wave of an earthquake and then the slow heavy one; what both catch is thrown off its feet.

**When it happens.** Whenever you cast Quake Step, as the shape it wears. Before this is bought, the plain ring is cast.

**The numbers.** 3 Shards. Struck From Afar: a ring 2.5 metres round an enemy up to 8 metres off, 0.45 seconds after the cast, 16 damage, 30 stagger and two Tremor, and it is held 0.6 seconds. The Second Wave: a ring to 5 metres, 8 damage and one Tremor, then 0.7 seconds later a ring to 7 metres round where you stamped, 18 damage and 40 stagger; what the first ring strikes is thrown down for 1 second.

**Works with.** Struck From Afar: Cairn on the one it shook. The Second Wave: Tremor, Unbroken Cairn and your heavy blows on what lies on the ground.

**Not built yet.** Struck From Afar placed on the enemy faced (for now the nearest within 8 metres). The Second Wave throwing down only what the second ring catches too, at that moment (for now what the first ring catches is thrown down at once).

**Example.** The Second Wave in a crowd: step back after the first ring so the second catches them all as they come.

## earth.quakestep.master
**What it does.** Break an enemy out of its wind-up, and Quake Step is ready again at once.

**When it happens.** For now, whenever Quake Step strikes an enemy, at most once in 18 seconds. Quake Step must be in a slot.

**The numbers.** 3 Shards. Its own wait ends.

**Works with.** The Second Wave (two stamps in a row), Cairn after two stamps.

**Not built yet.** An enemy broken out of its wind-up as the moment: nothing yet tells when an enemy begins an attack, so any stamp that lands counts, once in 18 seconds.

**Example.** Stamp a soldier out of its swing, then stamp again as its friend closes.

## earth.stance
**What it does.** Mountain Stance, a counter (Grey Stone, the Cailleach turning to stone each summer). You go wholly to stone, unable to move. A blow that strikes the stone is turned, red ones too, and it jars its attacker: it is shaken and its next attack comes late. The blows the stone turned load your next heavy blow.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4), then any blow while you stand.

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 9, and 3 seconds). As built it is the stance of stone: up to 2.5 seconds, and the first blow to land is turned for nothing and answered by a jar of stone 3 metres long and 120 degrees wide toward the one who struck: 6 damage, 10 stagger and two Tremor. Its wind-ups come at 0.7 of their pace for 3 seconds, and for 3.5 seconds your next blow that lands breaks 50% more balance.

**Works with.** Rooted (stand, and it counts), Immovable, which grows from it, Heavy Hand, Cairn's Master, Buckled on the Forge branch.

**Not built yet.** Every blow turned for as long as you stand, not only the first; the load counted as 20 stagger for each blow turned (25 with the Empower) and given to the next heavy blow within a second of letting go; the wait of 5 seconds when nothing struck the stone. For now the first blow is turned and the next blow of any kind is loaded.

**Example.** A duellist starts its chain: go to stone, take the first cut on the stone, and its next swing comes slow enough to guard.

## earth.stance.shape
**What it does.** Opens two shapes of Mountain Stance, chosen at any rest fire. **Stone Skin**: the blow is turned but not answered; two plates of stone grow on your shoulders instead, each taking most of one blow and stopping its flinch, then breaking in a burst round you. **Fist of Rock**, the stance as it was first built: stand as stone a short while, and the first blow to land is turned for nothing and a fist of rock rises under the one who struck. One blow, answered at once.

**When it happens.** Whenever you cast Mountain Stance, as the shape it wears. Before this is bought, Grey Stone is cast.

**The numbers.** 3 Shards. Stone Skin: when the stance turns a blow, two wards, three held at most; each turns one blow and breaks in a burst 3 metres round you, 14 damage, 24 stagger and one Tremor. Fist of Rock: up to 1.5 seconds; the answer is a cone 4 metres long and 90 degrees wide, 30 damage, 60 stagger and two Tremor.

**Works with.** Stone Skin: Glaze's ward if you carry Ice, Heavy Hand, Immovable. Fist of Rock: Tremor and Cairn, Unbroken Cairn's heavy blow after it.

**Not built yet.** Stone Skin's plates taking 60% of one blow (for now a ward takes the whole of it), and the stance's jar set aside while it is worn. Fist of Rock's answer rising under the one who struck: for now it is a cone thrown at it.

**Example.** Stone Skin before a pack closes: the first two blows that reach you each throw stone back at the whole ring.

## earth.stance.master
**What it does.** Turn a red attack, and its attacker is stunned for a second and a half, less for a god or an elite, and Mountain Stance is ready again.

**When it happens.** For now, whenever the stance turns any blow. Mountain Stance must be in a slot.

**The numbers.** 3 Shards. The attacker is held 1.5 seconds (a god or an elite 0.6, the colossus not at all). The stance's wait ends, at most once in 20 seconds.

**Works with.** Fist of Rock (a full answer on a held enemy), Cairn and your heavy blows on the one held.

**Not built yet.** The red attack as the moment: nothing yet tells the blow turned was red, so any blow counts, and the stance comes back once in 20 seconds at most.

**Example.** A brute's red overhead lands on the stone: it stands stunned while you break its balance.

## earth.immovable
**What it does.** Immovable, a keystone. You stop rolling away. Your roll hardly moves you, but every blow takes less of you, guarding and dodging cost less stamina, and the moment of a perfect guard is longer.

**When it happens.** Always on once learned.

**The numbers.** 6 Shards. The roll goes a tenth as far (about half a metre). 16% less damage taken. Dodges and blocked blows cost 30% less stamina. The perfect guard's moment is 30% longer.

**Works with.** Mountain Stance, which it grows from, Rooted (do not roll, and both count), Heavy Hand, Cairn's and Stone Rise's Masters (you are always standing your ground), Bedrock.

**Not built yet.** The dodge as a brace that moves nowhere, whose first moment turns any blow, red ones too, and Rooted that never ends: for now the roll is still a roll, it lets blows pass through it, and it still ends Rooted.

**Example.** Fight a duellist toe to toe: guard everything, perfect-guard what you can, and roll in place through the one red thrust.

## earth.standingstones
**What it does.** When a spell of yours strikes an enemy, a pillar of stone comes down on it a moment later and shakes it.

**When it happens.** Whenever a spell of yours strikes an enemy, Stone Rise or any spell in your slots. At most once every second and a half.

**The numbers.** 3 Shards. The pillar falls on a warning ring 0.5 seconds later, 1.5 metres in radius: 20 damage, 35 stagger and one stack of Tremor.

**Works with.** Stone Rise, which grows beside it, Left in Rubble and Fissure, Quake Step, Tremor and Cairn.

**Not built yet.** The design re-aims it: two of your stones may stand at once, and an enemy driven into one is hurt and breaks it. Nothing yet stands on the field to be driven into, so the falling pillar stands in.

**Example.** Stone Rise under an archer, and half a second later a pillar lands on it as well.

## earth.base
**What it does.** Stone Rise, Earth's own spell, comes with the element and is never bought. A quarter second after the cast the ground breaks under the enemy faced and throws it up for a moment, and three rocks stand behind it that nothing walks through.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4). Carrying Earth, it is always yours to put in a slot.

**The numbers.** Given with Earth. No mana: it waits 7 seconds after a cast, less with weapon play; its Empower makes that 6. An enemy within 5 metres, 1.3 metres round it: 14 damage, 30 stagger and one Tremor. The rocks stand 4.5 seconds.

**Works with.** Its Empower, its two shapes and its Master; Tremor and Cairn; Standing Stones (a pillar on what it strikes); the charged blade, which its landing charges with Earth.

**Not built yet.** The Empower's 18 damage and 36 stagger: the base spell is cast by Earth's kit, which takes only the Empower's shorter wait.

**Example.** Rise the stone under the archer at the back: it is thrown up, shaken, and walled off from the fight behind it.

## earth.base.shape
**What it does.** Opens two shapes of Stone Rise, chosen at any rest fire. **Raised Stone**: no rocks behind; the pillar that lifted the enemy stays beside it, blocking bodies and thrown attacks, and a heavy blow of yours on it shatters it into a cone of shards away from you. Raise it, fight round it, break it through them. **Dropped From the Apron**: a boulder falls from above onto the spot, its shadow growing, and lies there; a heavy blow on it rolls it along the blow, crushing a lane, until it breaks on a wall or a large enemy.

**When it happens.** Whenever you cast Stone Rise, as the shape it wears. Before this is bought, the plain stone rises.

**The numbers.** 3 Shards. Raised Stone: for 6 seconds, your next heavy blow on the enemy it lifted throws a cone of shards from you through it, 5 metres and 70 degrees, 18 damage, 20 stagger and one Tremor. Dropped From the Apron: the nearest enemy within 5 metres, 0.6 seconds after the cast, 1.5 metres round it: 24 damage, 45 stagger and two Tremor; for 6 seconds a heavy blow on it crushes a lane 8 metres on from it, away from you, 16 damage, 30 stagger and one Tremor to each it strikes.

**Works with.** Raised Stone: Heavy Hand, The Slow Hand. Dropped From the Apron: a pack in a corridor, Cairn on the lane it shook.

**Not built yet.** Raised Stone's pillar left standing beside the enemy, blocking and struck itself (for now the kit's rocks stand behind it, and the heavy blow lands on the enemy). Dropped From the Apron's boulder falling from above and lying as a thing to strike, breaking on a wall or a large enemy (for now stone rises under the nearest enemy, and the heavy blow on it sends the lane on).

**Example.** Dropped From the Apron on the first of three soldiers in a corridor: one heavy blow on it crushes the two behind.

## earth.base.master
**What it does.** Cast it while you stand your ground, Rooted or still for a second, and the stone rises under the enemy's own feet and throws it three metres away from you, two deep in Tremor, before it stands.

**When it happens.** For now, when you cast it while Rooted (no dodge begun for 3 seconds). Stone Rise must be in a slot.

**The numbers.** 3 Shards. What it strikes is thrown 3 metres straight away from you and takes one more stack of Tremor (two with the stone's own).

**Works with.** Rooted, Immovable, Mountain Stance; Cairn on the two stacks it leaves.

**Not built yet.** Still for a second as the moment (for now Rooted only), and the throw before the rocks stand.

**Example.** Stand your ground and rise the stone under a brute that has come too close: it is thrown off you, two deep.

## earth.lift
**What it does.** Left in Rubble, Earth's reaction lift. Every reaction with Tremor in it leaves a knee-high heap of rubble where it happened, a puff of ochre dust and broken stone, which shakes whatever stands in it.

**When it happens.** Whenever a reaction with Tremor in it happens, on any enemy.

**The numbers.** 3 Shards. A heap 1 metre round for 6 seconds, biting once a second for 3 damage, 8 stagger and one Tremor.

**Works with.** Every pair with Tremor in it (The Ground Gives with Soaked, Chokedamp with Poison and the rest), Stone Remembers (it stands three times as long), Fissure and Stone Remembers, which grow from it.

**Not built yet.** Every reaction with Tremor in it striking 20% harder; the rubble standing as one of your stones, that heavy blows drive enemies into and that The Circle Speaks casts from.

**Example.** Soak and shake a soldier: the ground gives under it, and the rubble left keeps shaking the next one through.

## earth.fissure
**What it does.** Fissure, a breaker. You drive your weapon into the ground. A crack runs ahead and breaks open in a row of spikes that strike and shake what they catch, and holds ordinary enemies where they stand.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** 4 Shards, no mana: it waits 10 seconds after a cast, less with weapon play (its Empower: 9, and 26 damage). The crack runs 8 metres at 20 metres a second, a spike every 1.5 metres from 1.3 metres out, each 1 metre in radius. Each enemy is struck once: 20 damage, 35 stagger and one Tremor, and held 1.2 seconds (a god or an elite 0.5).

**Works with.** Left in Rubble, which it grows from, Standing Stones, Tremor and Cairn, your heavy blows on what it holds. Landing it charges the blade with Earth.

**Not built yet.** The crack glowing for 0.9 seconds before it breaks open: for now it breaks open as it runs.

**Example.** Three spearmen in a line down a stair: one crack catches all three and holds them for your blade.

## earth.fissure.shape
**What it does.** Opens two shapes of Fissure, chosen at any rest fire. **Basalt**: the spikes come up as a row of six-sided columns, the slow-cooled stone of the causeway, that stand as a wall; a heavy blow on the row topples it away from you column after column, like dominoes. **Fault Line**: the crack does not break open by itself. It stays open, and a heavy blow you land beside it erupts the whole line. You lay the line, then fight beside it.

**When it happens.** Whenever you cast Fissure, as the shape it wears. Before this is bought, the plain crack is cast.

**The numbers.** 3 Shards. Basalt: for 4 seconds, a heavy blow on an enemy the spikes struck topples a lane 6 metres on from it, away from you, 20 damage and 30 stagger to each. Fault Line: the crack strikes for nothing; for 6 seconds a heavy blow on an enemy it crossed erupts 2 metres round that enemy, 24 damage, 40 stagger and two Tremor, at most twice a second.

**Works with.** Heavy Hand and The Slow Hand, Tremor and Cairn on what the eruption shook.

**Not built yet.** Basalt's columns standing as a wall that blocks, and toppling one after another (for now the heavy blow on a struck enemy sends a lane on). Fault Line's open crack shown on the ground, erupting along its whole length on a heavy blow within 2 metres of it, twice at most (for now a heavy blow on an enemy the crack crossed erupts under that enemy, as often as twice a second).

**Example.** Fault Line under a pack, then fight among them: every heavy blow erupts under the one it lands on.

## earth.fissure.master
**What it does.** Land a heavy blow on the crack's line while it glows, and it erupts at once, half as hard again.

**When it happens.** For now, a heavy blow on an enemy within 0.9 seconds of the spikes striking it. Fissure must be in a slot.

**The numbers.** 3 Shards. A second eruption 1 metre round it, 10 damage, 18 stagger and one Tremor.

**Works with.** Heavy Hand (the heavy blow cannot be stopped as it lands), The Slow Hand, Tremor.

**Not built yet.** The crack's glow before it breaks open, and the heavy blow landed on its line then: for now the window opens as the spikes strike, and it adds a second eruption on that enemy.

**Example.** Cast the crack at a brute and step in with a heavy blow as it breaks: it erupts again under the brute.

## earth.devotion
**What it does.** Stone Remembers, Earth's Devotion (the rule of the Celtic land, "stone remembers", given to you): all in on Earth only. Nothing you raise falls by itself: stones, boulders, columns and slabs stand until something breaks them. An enemy driven into one takes a heavy blow of stone, and the stone holds. The longer the fight, the more of the floor is yours.

**When it happens.** Only while Earth is carried twice (all in). Always on then.

**The numbers.** 6 Shards. As built: what you leave on the ground (a spell's ground, a hazard from your marks, Left in Rubble's heaps) stands three times as long, and a heavy blow that lands strikes its enemy as if driven into stone, 1 metre round it, 20 damage, 35 stagger and one Tremor, once in 3 seconds. Buying this also gives you the deep strike, Mountain Cleaver. Use it with L2 and R2 while you carry this element alone or twice.

**Works with.** Left in Rubble, which it grows from, Fissure, Stone Rise, Standing Stones, Heavy Hand.

**Not built yet.** Stones that never fall by themselves, eight at most with the oldest crumbling, and an enemy driven into one by a heavy blow, a finisher or a quake: nothing yet stands on the field to be driven into, so every heavy blow stands in, once in 3 seconds.

**Example.** A long fight in a narrow hall: the rubble of every reaction stays, and every heavy blow lands as if into stone.

## earth.hand
**What it does.** While Bedrock burns, a hand of stone as tall as a tower hangs over your shoulder. Each heavy blow or finisher you land, it rises over that enemy and comes down on it a moment later, striking everything round it.

**When it happens.** Light Bedrock with the surge button. The hand stays until Bedrock ends.

**The numbers.** A god's gift, never sold; in the playtest it can be bought. The hand falls 0.5 seconds after the blow, 3 metres in radius: 70% of the blow's damage again, 40 stagger, a heavy blow, one stack of Tremor. It answers one blow at a time: a heavy blow landed while it is still falling is not answered.

**Works with.** Bedrock, Heavy Hand, The Slow Hand, Tremor, finishers.

**Example.** Light Bedrock and finish a staggered soldier: the hand comes down on the finisher's spot and staggers the two behind it.

## earth.closedring
**What it does.** The Closed Ring, a keystone. Lighting Bedrock raises eight stones in a ring round you. For ten seconds they strike and shake whatever stands among them, and you take a little less harm. Bedrock costs a quarter more.

**When it happens.** When you light Bedrock, the surge. Always on once learned while Earth is carried.

**The numbers.** 6 Shards. Eight stones in a ring 12 metres across, each a metre wide, for 10 seconds; once a second each strikes what stands on it for 8 damage, 20 stagger and one stack of Tremor. 8% less damage taken for the same 10 seconds. Bedrock costs 25% more mana (50 in place of 40).

**Works with.** Cairn, whose Master it grows from, Standing Stones and Left in Rubble (more stone round you), Rooted and Immovable (stand inside the ring and let them come), Hand of the Mountain.

**Not built yet.** The ring as a wall: nothing walking, rolling or being thrown across it while it stands, an enemy driven into a stone hurt and staggered by it, and you Rooted inside it. For now the stones only strike what stands on them, and anything can pass between them.

**Example.** Light Bedrock with two soldiers on you: the ring comes up round all three of you, and every second they stand in it they are shaken further toward a stagger.
