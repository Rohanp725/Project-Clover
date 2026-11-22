# Pyre: detailed explanations

The long explanation the chart shows for every mark on Pyre, the duo branch Poison and Fire share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Poison and Fire") and drawn in the heart of the chart while Poison and Fire are both
carried. Its reaction is Fire Medicine; its Shape mark lets you choose what Fire Medicine does.

## pyre.shape
**What it does.** Unlocks two new shapes for Fire Medicine. At a rest fire, you pick which shape your Fire Medicine uses. From then on, it uses that shape instead of the plain one. **Fire Lance**: the bang is a jet of fire out of the enemy's far side, away from you. **Flying Fire**: the bang throws five burning pellets that land around the enemy as small fires.

**When it happens.** Every time Fire Medicine goes off while you carry both Poison and Fire. Fire Medicine goes off when Burn lands on a poisoned enemy, or when poison lands on a burning one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Fire Medicine. The plain one makes a bang 2.5 metres around the enemy.

**The numbers.** Mix, 3 Shards. Fire Lance: after a 0.6-second fuse, a path 5 metres long and 2 metres wide past the enemy takes 4 damage per dose and 6 per Burn stack, plus 30 stagger, a dose and a Burn. Flying Fire: five patches, each 1 metre across, land in a ring 3 metres out and last 3 seconds. Each one burns for 3 damage and a Burn every half second, and poisons once a second. Both shapes use up half of the enemy's doses. After Fire Medicine goes off on an enemy, it can't go off on that enemy again for 4 seconds.

**Works with.** Goes Up (with full poison, the bang is twice as hard and uses nothing up), Funeral Fire, Miasma and Flame Wave to set it up, Slow Burn.

**Example.** A line of soldiers stands behind a poisoned one. Stand in front, and Fire Lance burns all the way down the line. Holding a doorway: Flying Fire surrounds the poisoned enemy with fire.

**Not built yet.** For now, Fire Lance's jet is a straight path instead of a cone 40 degrees wide.

## pyre.flashpoint
**What it does.** When Fire Medicine goes off on an enemy whose poison is full, it hits twice as hard and uses nothing up.

**When it happens.** Each time Fire Medicine goes off on an enemy with full poison (eight doses), while you carry both Poison and Fire.

**The numbers.** Mix, 3 Shards. The game checks the enemy's poison as the reaction goes off. At full depth, every blow of Fire Medicine deals double damage, and none of its doses are used up.

**Works with.** Miasma and Venom (eight doses fast), Fire Medicine's shapes, Funeral Fire, Slow Burn.

**Example.** Give a soldier eight doses, then set it on fire. Fire Medicine hits twice as hard, and it still has eight doses.

## pyre.funeralfire
**What it does.** An enemy that dies while poisoned and burning explodes, and leaves the ground burning where it fell.

**When it happens.** The moment an enemy dies with both poison and Burn on it.

**The numbers.** Mix, 3 Shards. A burst with a 3.5-metre radius deals 20 damage, 20 stagger, two doses of poison and one stack of Burn to everything in it. The ground burns 1.5 metres around for 4 seconds, dealing 5 damage and a stack of Burn every half second. At most twice a second.

**Works with.** Goes Up, Catching Fire, Fire Medicine.

**Example.** Kill one poisoned, burning soldier in a crowd, and the rest catch both.

## pyre.slowburn
**What it does.** Every affliction you add works harder, including Burn and poison: every bite, and every time one is set off.

**When it happens.** Always on once you learn it.

**The numbers.** Mix, 5 Shards. Afflictions are 30% stronger.

**Works with.** Goes Up or Funeral Fire (it comes after either on the chart), Fire Medicine, Light the Pyres.

**Example.** A five-stack burn that dealt 12.5 damage a second now deals over 16.

## pyre.lightthepyres
**What it does.** You kneel and drive your weapon into the ground. A line of fire runs to every poisoned or burning enemy nearby and follows it. Stay still through three breaths, and a pillar of flame rises under each one, setting off its burn and its poison. A dodge or getting hit breaks it. If no enemy is poisoned or burning, you get the mana back.

**When it happens.** Cast it from its spell slot (hold L2 and a face button, or press 1 to 4) and stay still.

**The numbers.** 5 Shards. You can buy it once you carry both elements; no god gives it. 60 mana. It reaches up to six enemies within 9 metres. The three breaths take 2.2 seconds. Each pillar has a 2-metre radius and deals 40 damage and 40 stagger, counts as a heavy blow, and adds two doses of poison. On top of that, the poison is set off for 6 damage per dose, and the Burn for 10 damage plus 10 per stack.

**Works with.** Goes Up, Fire Medicine, Cinder, Slow Burn.

**Example.** Fight until five enemies are burning, roll away from them, and light the pyres.
