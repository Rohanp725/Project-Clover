# Forge: detailed explanations

The long explanation the chart shows for every mark on Forge, the duo branch Earth and Metal share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Earth and Metal") and drawn in the heart of the chart while Earth and Metal are both
carried. Its reaction is Anvil Stone; its Shape mark lets you choose what Anvil Stone does.

## forge.shape
**What it does.** Opens Anvil Stone's two shapes. At a rest fire you choose which one your Anvil Stone wears, and from then on it runs instead of the plain one. **Ringing Iron**: no stone and no waiting for a heavy blow; the shaken armour rings like struck iron at once. **Hammer Scale**: no ring; flakes of glowing scale fly from it into the enemies near.

**When it happens.** Every time Anvil Stone fires (Sundered landing on a shaken enemy, or Tremor on a notched one) while Earth and Metal are both carried. Before this mark is bought, or with only one of the two carried, the plain Anvil Stone runs (a stone at its back for 5 seconds; your next heavy blow drives it onto it).

**The numbers.** Mix, 3 Shards. Ringing Iron: its Sundered to full at once, and a ring of sparks 3 metres round it, 10 damage, 15 stagger and a notch on each. Hammer Scale: up to four enemies, each within 6 metres of the last, take 8 damage, a notch and a Tremor. Anvil Stone rests 4 seconds on each enemy.

**Works with.** Buckled (the shaken and notched lose their balance sooner), Hammer and Anvil, Stone Rise and Flung Blade to bring the two.

**Example.** No time for a heavy blow: Ringing Iron breaks its armour the moment the two meet. A loose pack: Hammer Scale notches four at once.

**Not built yet.** Hammer Scale's flakes leap from one enemy to the next as an arc rather than flying at each.

## forge.hammeranvil
**What it does.** A heavy blow on a shaken enemy strikes twice: once from your weapon, and once more a moment later as the stone answers. Heavy blows also lay Tremor and Sundered together, whichever of the two you carry first. As built, the second strike asks for Tremor on the enemy, not stone at its back, and it bursts round you rather than behind the enemy.

**When it happens.** Every heavy blow that lands. The second strike needs the enemy to carry Tremor already.

**The numbers.** Mix, 3 Shards. The second strike lands 0.15 seconds later, 2.3 metres round you: 60% of the heavy blow's damage and 20 stagger, at most twice a second. Each heavy blow also adds a stack of Tremor and a notch of Sundered; your first element's own is not laid twice when its root already lays it with heavy blows.

**Works with.** Tremor, Heavy Hand, Buckled, Mountain Stance and Quake Step (Tremor to start it), Metal's root.

**Example.** Shake a brute with Quake Step, then land two heavy blows: each one lands again for 60% as the stone answers.

## forge.struckore
**What it does.** A perfect guard strikes a spike of ore up from the ground under the one who struck and shakes it, and three blades fly from you toward it, notching what they pass.

**When it happens.** A perfect guard: press the guard button just before a blow lands.

**The numbers.** Mix, 3 Shards. The spike falls 0.2 seconds later, 1.2 metres in radius: 10 damage, 20 stagger and one stack of Tremor. The three blades fly in a narrow fan at 10 metres a second for 1.2 seconds; each strikes the first enemy it meets for 9 damage, 4 stagger and one notch of Sundered. As built the spike strikes and is gone; a pillar that stands to be broken is to come.

**Works with.** Buckled (both afflictions from one guard), Mountain Stance, Hammer and Anvil, Anvil Stone's shapes.

**Example.** Perfect-guard a soldier's swing: it is shaken and notched at once, and the soldier behind it takes a blade too.

## forge.buckled
**What it does.** An enemy that is both shaken and notched loses its balance far sooner, from everything that strikes it.

**When it happens.** Always on once learned, on any enemy carrying Tremor and Sundered together.

**The numbers.** Mix, 3 Shards. 40% more stagger from every source while it carries both. Tremor also stops its balance draining, so what you put in stays.

**Works with.** Hammer and Anvil, Struck Ore, World Anvil (lays both), Mountain Stance, Quake Step, Earth's and Metal's roots, the charged blade of whichever element you carry second.

**Example.** Notch a brute with Metal blows, shake it with Quake Step, and its balance bar fills almost half again as fast.

## forge.drawnfromstone
**What it does.** You draw a sword of stone and iron from the ground and give one great swing through a half circle before you. As built it is that one swing; holding the great sword for three swings is to come.

**When it happens.** From a spell slot: cast it with that slot's button (hold L2 and a face button, or 1 to 4).

**The numbers.** Mix, 5 Shards, 50 mana. A half circle 4 metres deep: 40 damage, 60 stagger and one stack of Tremor to everything in it.

**Works with.** Hammer and Anvil or Struck Ore (it grows from either), Buckled, Standing Stones. Landing it charges the blade with Earth.

**Example.** Ringed by soldiers in front of you: one swing shakes them all and leaves each well on its way to staggered.

## forge.foldedsteel
**What it does.** Every finisher you land folds your blade once more. Each fold makes every blow of the weapon a little harder, and a little heavier to swing. The count shows on the screen as Folds; the folds are a fight's growth, kept until your next rest fire, not the weapon's (the Way is what improves a weapon). As built they are kept while the mark is in force and lost with it, and a rest fire does not yet empty them.

**When it happens.** Each finisher that lands (a heavy blow on a staggered enemy) adds a fold.

**The numbers.** Mix, 5 Shards. Ten folds at most. Each fold: 3% more weapon damage and 1% more stamina for each blow. At ten: 30% more damage, 10% more stamina.

**Works with.** Buckled and Tremor (more staggers, more finishers), World Anvil, Hammer and Anvil.

**Example.** Ten finishers into a land, every cut you make is 30% heavier than when you began.

## forge.worldanvil
**What it does.** Your finisher is struck on an anvil that rises out of the ground. Its ring runs out and strikes every other enemy near, shaking it and breaking its armour, and a blade is forged for each one it reaches. The blades hang at your shoulders and fly with your next heavy blow, each at an enemy near.

**When it happens.** Every finisher that lands. The blades wait for your next heavy swing.

**The numbers.** 5 Shards, bought once both elements are carried; no god gives it. The ring reaches 6 metres from the finished enemy: 15 damage, 30 stagger, three stacks of Tremor and three notches of Sundered to each other enemy in it. One blade for each enemy reached, six at most, kept up to 14 seconds. Each flies at an enemy within 10 metres for 10 damage, 12 stagger and one notch of Sundered.

**Works with.** Buckled (every enemy it reaches is left shaken and notched), Folded Steel, Hammer and Anvil.

**Example.** Finish a soldier in a pack of four: the other three are shaken and notched three deep, and your next heavy blow throws three blades.
