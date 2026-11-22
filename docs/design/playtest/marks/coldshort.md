# Cold Short: detailed explanations

The long explanation the chart shows for every mark on the duo branch Ice and Metal share, named for its reaction,
Cold Short (`docs/design/steel/pairs_d4.md`, "Ice and Metal"). Drawn in the heart of the chart while Ice and Metal
are both carried.

## coldshort.shape
**What it does.** Plate or Snap unlocks two new shapes for Cold Short. At a rest fire, you pick which shape your Cold Short uses. From then on, it uses that shape instead of the plain one. **Shed Plate**: the enemy's armour turns brittle, and your heavy blow cracks the plates off, like before. The cracked plate falls as a sheet of black ice that knocks down enemies standing on it. **Snapped Blade**: the armour doesn't crack. Instead, the enemy's own weapon turns brittle and snaps, so its blows deal half damage.

**When it happens.** Every time Cold Short goes off while you carry both Ice and Metal. Cold Short goes off when Chill lands on a notched enemy, or when a notch lands on a chilled one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Cold Short.

**The numbers.** Mix, 3 Shards. Shed Plate: the enemy is brittle for 5 seconds, and its notches stay. Your next heavy blow cracks the plates: your weapon deals 25% more to it for 6 seconds, and its notches stay for the rest of the fight. The black ice is 2 metres across and lasts 4 seconds. Every enemy within 1.2 metres slips and falls flat for 0.6 seconds. Snapped Blade: for 5 seconds, the enemy's notches don't fade and its blows deal half damage. Any guard you raise against it counts as a perfect guard, costing no stamina, and each one breaks its weapon: 50 stagger and a 1-second hold. After Cold Short goes off on an enemy, it can't go off on that enemy again for 5 seconds.

**Works with.** Rimed Edge to set it up, Glass Bell, Brittle to the Core, Cold Chisel.

**Example.** A wall of shields: use Shed Plate, and the ice under the first enemy knocks down the ones next to it. An enemy that hits hard: use Snapped Blade, and block its blows with your guard.

**Not built yet.** Shed Plate's ice knocks down enemies nearby when it falls, but not enemies who step on it later.

## coldshort.rimededge
**What it does.** Your heavy blows add a Chill and a notch at the same time.

**When it happens.** Every heavy blow or finisher that lands, while you carry both Ice and Metal.

**The numbers.** Mix, 3 Shards. One Chill and one notch.

**Works with.** Cold Short (one heavy blow on a fresh enemy adds both), Glass Bell.

**Example.** Land a heavy blow, and the armour you hit frosts over.

## coldshort.glassbell
**What it does.** Each blow on a brittle enemy rings like glass. You get 3 mana, and 0.3 seconds come off the wait on every spell that is waiting.

**When it happens.** Each weapon blow on a brittle enemy, while you carry both Ice and Metal. The enemy is brittle for 5 seconds after its Cold Short, in any shape, until a heavy blow cracks it. That heavy blow rings too.

**The numbers.** Mix, 3 Shards. 3 mana and 0.3 seconds off every spell's wait for each blow, at most ten times a second.

**Works with.** Cold Short, Rimed Edge, Cold Chisel.

**Example.** Ring a brittle brute with light blows, then crack it with a heavy blow.

## coldshort.core
**What it does.** Cold Short on a frozen enemy also shatters it. The shatter and the crack land at the same time.

**When it happens.** Each Cold Short on a frozen enemy, while you carry both Ice and Metal.

**The numbers.** Mix, keystone, 5 Shards. The ice breaks just as it would under your heavy blow, for the same damage, plus 40 stagger. Anything that answers a shatter answers this one too.

**Works with.** Rimed Edge, Cold Short, Cold Chisel.

**Example.** Freeze a soldier and notch it, and it breaks right away.

## coldshort.chisel
**What it does.** A chisel of black ice sticks point-first into the enemy you face, if it is within 8 metres. Each of your next blows on that enemy taps the chisel in, adding a notch and a Chill with a higher ringing sound each time. The tap that finds three notches drives it all the way in and splits the armour: 36 damage, 70 stagger, and Cold Short goes off right away.

**When it happens.** Cast it from a spell slot while you carry both Ice and Metal. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Ice.

**The numbers.** Mix, duo spell, detonator, 5 Shards. It costs no mana. After you cast it, you must wait 10 seconds to cast it again. Fighting with your weapon makes the wait shorter. The chisel stays for 6 seconds, for three taps. Each tap deals 6 damage, 8 stagger, a Chill and a notch. The tap that finds three notches drives it in: 36 damage, 70 stagger, and Cold Short goes off even if it would normally have to wait.

**Works with.** Cold Short and its shapes, Rimed Edge, Glass Bell, Brittle to the Core.

**Example.** Stick the chisel in the captain. Tap, tap, and the third blow splits its armour.

**Not built yet.** Any blow taps it, not only light blows. It is driven in by the tap that finds three notches, instead of by your first heavy blow. The final hit doesn't yet grow by 12 for each notch.
