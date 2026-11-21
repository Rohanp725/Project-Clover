# Forge: detailed explanations

The long explanation the chart shows for every mark on Forge, the duo branch Earth and Metal share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Earth and Metal") and drawn in the heart of the chart while Earth and Metal are both
carried. Its reaction is Anvil Stone; its Shape mark lets you choose what Anvil Stone does.

## forge.shape
**What it does.** Unlocks two new shapes for Anvil Stone. At a rest fire, you pick which shape your Anvil Stone uses. From then on, it uses that shape instead of the plain one. **Ringing Iron**: no stone, and no waiting for a heavy blow. The shaken armour rings like struck iron right away. **Hammer Scale**: no ring. Instead, flakes of glowing metal fly off the enemy into the enemies nearby.

**When it happens.** Every time Anvil Stone goes off while you carry both Earth and Metal. Anvil Stone goes off when Sundered lands on a shaken enemy, or when Tremor lands on a notched one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Anvil Stone. The plain one puts a stone behind the enemy for 5 seconds, and your next heavy blow smashes the enemy into it.

**The numbers.** Mix, 3 Shards. Ringing Iron: the enemy's Sundered fills all the way right away, and a ring of sparks 3 metres around it deals 10 damage, 15 stagger and a notch to each enemy. Hammer Scale: up to four enemies, each within 6 metres of the last one, take 8 damage, a notch and a Tremor. After Anvil Stone goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Buckled (shaken, notched enemies lose their balance sooner), Hammer and Anvil, and Stone Rise and Flung Blade to set it up.

**Example.** No time for a heavy blow: Ringing Iron breaks the enemy's armour the moment the two afflictions meet. A spread-out pack: Hammer Scale notches four at once.

**Not built yet.** Hammer Scale's flakes should jump from one enemy to the next in an arc, instead of flying at each one.

## forge.hammeranvil
**What it does.** A heavy blow on a shaken enemy hits twice: once from your weapon, and once more a moment later as the stone answers. Heavy blows also add Tremor and Sundered together, no matter which of the two elements is your first. For now, the second hit needs Tremor on the enemy, not a stone behind it, and it bursts around you instead of behind the enemy.

**When it happens.** Every heavy blow that lands. The second hit needs the enemy to already have Tremor.

**The numbers.** Mix, 3 Shards. The second hit lands 0.15 seconds later, in a 2.3-metre circle around you. It deals 60% of the heavy blow's damage and 20 stagger, at most twice a second. Each heavy blow also adds a stack of Tremor and a notch of Sundered. Your first element's affliction isn't added twice if its root mark already adds it with heavy blows.

**Works with.** Tremor, Heavy Hand, Buckled, Mountain Stance and Quake Step (Tremor to start it), Metal's root mark.

**Example.** Shake a brute with Quake Step, then land two heavy blows. Each one hits again for 60% as the stone answers.

## forge.struckore
**What it does.** A perfect guard makes a spike of ore shoot up from the ground under the enemy that hit you, shaking it. Three blades also fly from you toward it, notching what they pass.

**When it happens.** On a perfect guard: press the guard button just before a blow lands.

**The numbers.** Mix, 3 Shards. The spike comes 0.2 seconds later, with a 1.2-metre radius. It deals 10 damage, 20 stagger and one stack of Tremor. The three blades fly in a narrow fan at 10 metres a second for 1.2 seconds. Each one hits the first enemy it meets for 9 damage, 4 stagger and one notch of Sundered. For now, the spike hits and disappears. A pillar that stays standing until it is broken is not built yet.

**Works with.** Buckled (both afflictions from one guard), Mountain Stance, Hammer and Anvil, Anvil Stone's shapes.

**Example.** Perfect-guard a soldier's swing. It is shaken and notched at once, and the soldier behind it gets hit by a blade too.

## forge.buckled
**What it does.** An enemy that is both shaken and notched loses its balance much sooner, from everything that hits it.

**When it happens.** Always on once you learn it, on any enemy that has Tremor and Sundered at the same time.

**The numbers.** Mix, 3 Shards. 40% more stagger from everything while it has both. Tremor also stops its stagger meter from going down, so the stagger you add stays.

**Works with.** Hammer and Anvil, Struck Ore, World Anvil (adds both), Mountain Stance, Quake Step, the root marks of Earth and Metal, and the charged blade of whichever element you carry second.

**Example.** Notch a brute with Metal blows, then shake it with Quake Step. Its balance bar fills almost 50% faster.

## forge.drawnfromstone
**What it does.** You pull a sword of stone and iron out of the ground and make one huge swing through a half circle in front of you. For now, it is that one swing. Holding the great sword for three swings is not built yet.

**When it happens.** Cast it from a spell slot. Press that slot's button (hold L2 and a face button, or press 1 to 4).

**The numbers.** Mix, 5 Shards, 50 mana. A half circle 4 metres deep. It deals 40 damage, 60 stagger and one stack of Tremor to everything in it.

**Works with.** Hammer and Anvil or Struck Ore (it comes after either on the chart), Buckled, Standing Stones. When it lands, it charges your blade with Earth.

**Example.** Soldiers are lined up in front of you. One swing shakes them all and puts each one well on its way to a stagger.

## forge.foldedsteel
**What it does.** Every finisher you land folds your blade one more time. Each fold makes every weapon blow a little stronger, and a little heavier to swing. The count shows on the screen as Folds. The folds belong to this stretch of fighting and last until your next rest fire. They don't improve the weapon itself (the Way is what improves a weapon). For now, they last while this mark is working and are lost when it stops, and a rest fire doesn't clear them yet.

**When it happens.** Each finisher that lands (a heavy blow on a staggered enemy) adds a fold.

**The numbers.** Mix, 5 Shards. Up to ten folds. Each fold adds 3% weapon damage and 1% stamina cost for each blow. At ten folds: 30% more damage and 10% more stamina.

**Works with.** Buckled and Tremor (more staggers means more finishers), World Anvil, Hammer and Anvil.

**Example.** After ten finishers in a land, every cut you make does 30% more damage than when you started.

## forge.worldanvil
**What it does.** Your finisher strikes the enemy on an anvil that rises out of the ground. Its ringing sound spreads out and hits every other enemy nearby, shaking it and breaking its armour. A blade is made for each enemy it reaches. The blades hang at your shoulders and fly out with your next heavy blow, each one at a nearby enemy.

**When it happens.** Every finisher that lands. The blades wait for your next heavy swing.

**The numbers.** 5 Shards. You can buy it once you carry both elements; no god gives it. The ring reaches 6 metres from the finished enemy. Every other enemy in it takes 15 damage, 30 stagger, three stacks of Tremor and three notches of Sundered. You get one blade for each enemy it reaches, up to six, and keep them for up to 14 seconds. Each blade flies at an enemy within 10 metres for 10 damage, 12 stagger and one notch of Sundered.

**Works with.** Buckled (every enemy it reaches is left shaken and notched), Folded Steel, Hammer and Anvil.

**Example.** Finish a soldier in a pack of four. The other three are shaken and get three notches each, and your next heavy blow throws three blades.
