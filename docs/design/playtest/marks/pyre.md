# Pyre: detailed explanations

The long explanation the chart shows for every mark on Pyre, the duo branch Poison and Fire share. Reshaped in Phase 4
(`docs/design/steel/pairs_d1_d2.md`, "Poison and Fire") and drawn in the heart of the chart while Poison and Fire are both
carried. Its reaction is Fire Medicine; its Shape mark lets you choose what Fire Medicine does.

## pyre.shape
**What it does.** Opens Fire Medicine's two shapes. At a rest fire you choose which one your Fire Medicine wears, and from then on it runs instead of the plain one. **Fire Lance**: the bang is a jet of fire out of the enemy's far side, away from you. **Flying Fire**: the bang throws five burning pellets that land round it as small fires.

**When it happens.** Every time Fire Medicine fires (Burn landing on a poisoned enemy, or poison on a burning one) while Poison and Fire are both carried. Before this mark is bought, or with only one of the two carried, the plain Fire Medicine runs (a bang round it, 2.5 metres).

**The numbers.** Mix, 3 Shards. Fire Lance: after the 0.6-second fuse, a lane 5 metres long and 2 metres wide beyond it takes 4 damage a dose and 6 a Burn stack, 30 stagger, a dose and a Burn. Flying Fire: five patches 1 metre across in a ring 3 metres out, for 3 seconds; each burns 3 every half second with a Burn and poisons once a second. Both spend half its doses. Fire Medicine rests 4 seconds on each enemy.

**Works with.** Goes Up (at full depth the bang is twice as hard and spends nothing), Funeral Fire, Miasma and Flame Wave to bring the two, Slow Burn.

**Example.** A line of soldiers behind a poisoned one: stand at its front and Fire Lance burns down the line. Holding a doorway: Flying Fire rings the poisoned one with fire.

**Not built yet.** Fire Lance's jet is a lane rather than a cone 40 degrees wide.

## pyre.flashpoint
**What it does.** When Fire Medicine goes off on an enemy whose poison is at full depth, it strikes twice as hard and spends nothing: a second bang of the same size follows the first, and the doses it spent are laid back.

**When it happens.** Each time Fire Medicine fires on an enemy at full depth of poison (eight doses), while Poison and Fire are both carried.

**The numbers.** Mix, 3 Shards. A second bang 2.5 metres round it, 0.65 seconds after the reaction: 32 damage, 30 stagger and a Burn on all it reaches. Four doses laid back on it.

**Works with.** Miasma and Venom (eight doses fast), Fire Medicine's shapes, Funeral Fire, Slow Burn.

**Example.** Fill a soldier with eight doses, then burn it: two bangs, and it is still eight deep.

**Not built yet.** The reaction itself is not doubled: a second bang stands in, and the doses are laid back after half are spent.

## pyre.funeralfire
**What it does.** An enemy that dies poisoned and burning explodes, and leaves the ground burning where it fell.

**When it happens.** The moment an enemy dies carrying both poison and Burn.

**The numbers.** Mix, 3 Shards. A burst 3.5 metres in radius: 20 damage, 20 stagger and two doses of poison, and one stack of Burn on all within it. The ground burns 1.5 metres round for 4 seconds, biting for 5 every half second with a stack of Burn. At most twice a second.

**Works with.** Goes Up, Catching Fire, Fire Medicine.

**Example.** Kill one poisoned, burning soldier in a crowd and the rest catch both.

## pyre.slowburn
**What it does.** Every affliction you lay works harder, Burn and poison among them: each bite, and each set-off.

**When it happens.** Always on once learned.

**The numbers.** Mix, 5 Shards. 30% stronger afflictions.

**Works with.** Goes Up or Funeral Fire (it grows from either), Fire Medicine, Light the Pyres.

**Example.** A five-deep burn that dealt 12.5 a second now deals over 16.

## pyre.lightthepyres
**What it does.** You kneel and drive your weapon into the ground. A line of fire runs to every poisoned or burning enemy near and follows it. Stand through three breaths, and a pillar of flame rises under each, setting off its burn and its poison. A dodge or a blow taken breaks it. With no one alight, the mana comes back.

**When it happens.** Cast it from its spell slot (hold L2 and a face button, or 1 to 4) and stay still.

**The numbers.** 5 Shards, bought once both elements are carried; no god gives it. 60 mana. Up to six enemies within 9 metres. The three breaths take 2.2 seconds. Each pillar is 2 metres in radius: 40 damage and 40 stagger, a heavy blow, two doses of poison, plus 6 a dose of poison and 10 and 10 a stack of Burn set off.

**Works with.** Goes Up, Fire Medicine, Cinder, Slow Burn.

**Example.** Fight until five enemies are burning, roll clear, and light the pyres.
