# Black Chain: detailed explanations

The long explanation the chart shows for every mark on the duo branch Metal and Dark share, named for its reaction, Black
Chain (`docs/design/steel/pairs_d1_d2.md`, "Metal and Dark"). Drawn in the heart of the chart while Metal and Dark are
both carried.

## blackchain.shape
**What it does.** Hidden Chain unlocks two new shapes for Black Chain. At a rest fire, you pick which shape to use. From then on, your Black Chain uses that shape instead of the plain one. **Drawn Tight**: the chain pulls tight right away. The two enemies are dragged together and smash into each other. Then the chain falls off. **Shared Wound**: there is no leash. Instead, the chain lasts longer, and half of every blow you land on one enemy also hits the other.

**When it happens.** Every time Black Chain goes off while you carry both Metal and Dark. Black Chain goes off when a Curse lands on a notched enemy, or when Sundered lands on a cursed one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Black Chain. The plain one chains the enemy to the nearest other enemy within 6 metres for 5 seconds.

**The numbers.** Mix, 3 Shards. Drawn Tight: the enemy takes 12 damage and 25 stagger, and so does the nearest other enemy within 6 metres. Shared Wound: the enemy is chained to the nearest other enemy within 6 metres for 8 seconds, and half of every blow is shared. After Black Chain goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Grasp and Flung Blade to set it up, Dead Weight, One Noose for Two, The Smith's Net.

**Example.** Two shield bearers stand side by side. Drawn Tight smashes them together. A big brute stands next to a small enemy. Use Shared Wound, then cut down the small one, and half of each blow goes to the brute.

**Not built yet.** Drawn Tight hits both enemies where they stand instead of dragging them together. Shared Wound's chain does not yet pass a Curse from one enemy to the other.

## blackchain.deadweight
**What it does.** When one of two chained enemies dies, the chain drags the other one to where it fell. Then it holds that enemy in place and Curses it again.

**When it happens.** Each time an enemy dies while it has both a Curse and Sundered on it, while you carry both Metal and Dark.

**The numbers.** Mix, 3 Shards. For now: every enemy within 6 metres of where it fell is pulled 3 metres toward that spot. Any enemy within 2 metres of the spot is held for 1 second and Cursed. This can happen at most twice a second.

**Works with.** Black Chain, One Noose for Two, Never Sated.

**Example.** Kill a chained soldier. Its partner is dragged to the body, held, and Cursed, ready for your next blow.

**Not built yet.** For now it goes off on any death of an enemy that is both Cursed and notched, and it pulls every enemy nearby, not only the chained partner.

## blackchain.rust
**What it does.** Notches on a Cursed enemy do not fade for as long as the Curse lasts.

**When it happens.** Always on once you learn it, while you carry both Metal and Dark.

**The numbers.** Mix, 3 Shards.

**Works with.** Flung Blade and Sunder (for notches), Grasp (for Curses), Black Chain.

**Example.** Curse a brute, and its broken armour stays broken.

**Not built yet.** It does nothing yet. The game can't yet make one affliction last as long as another one does.

## blackchain.noose
**What it does.** When you land a finisher on a chained enemy, it also hits the enemy it is chained to. That second hit does half the damage and half the stagger.

**When it happens.** Every time a finisher lands on an enemy that has both a Curse and Sundered on it.

**The numbers.** Mix, keystone, 5 Shards. For now: half the finisher's damage, plus 30 stagger, jumps to the nearest other enemy within 4 metres.

**Works with.** Black Chain, Dead Weight, The Smith's Net, Sunder.

**Example.** Finish off a chained captain, and its guard staggers from the same blow.

**Not built yet.** It jumps to the nearest other enemy, not to the chained partner. Its stagger is always 30, not half of the finisher's.

## blackchain.net
**What it does.** You throw a net of thin black chain. A moment later it opens and falls. You only see a shimmer and its shadow growing on the floor. Every enemy under it is Cursed, notched, and held in place, so Black Chain binds them.

**When it happens.** Cast it from a spell slot while you carry both Metal and Dark. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Metal.

**The numbers.** Mix, duo spell, opener, 5 Shards. It costs no mana. After you cast it, you must wait 16 seconds to cast it again. Fighting with your weapon makes the wait shorter. It lands up to 7 metres ahead of you. It falls 0.4 seconds after you cast it and covers a circle with a 2-metre radius. Each enemy under it takes 6 damage and 10 stagger, gets a notch and a Curse, and is held for 1.5 seconds.

**Works with.** Black Chain and its shapes, One Noose for Two, Dead Weight.

**Example.** Throw the net over a pack, then cut through them while they stand chained.

**Not built yet.** It does not yet chain every enemy under it to every other one for 6 seconds. It does not yet keep them inside the circle for 3 seconds. It does not yet pass a quarter of every blow to the rest.
