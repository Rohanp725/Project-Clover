# Stolen Warmth: detailed explanations

The long explanation the chart shows for every mark on the duo branch Dark and Ice share, named for its reaction,
Stolen Warmth (`docs/design/steel/pairs_d5.md`, "Dark and Ice"). Drawn in the heart of the chart while Dark and Ice
are both carried.

## stolenwarmth.shape
**What it does.** Let the Cold In unlocks two new shapes for Stolen Warmth. At a rest fire, you pick which shape your Stolen Warmth uses. From then on, it uses that shape instead of the plain one. **Cold Shade**: the enemy's warmth is drawn into you like before, and a shattered enemy leaves its shadow behind as a figure of black ice that chills everything nearby. **The Long Night**: you don't draw any health. Instead, the warmth goes into the dark, and while the enemy is frozen, the cold spreads to everything near it.

**When it happens.** Every time Stolen Warmth goes off while you carry both Dark and Ice. Stolen Warmth goes off when a Curse lands on a chilled enemy, or when Chill lands on a Cursed one. If you have not bought this mark, or you carry only one of the two elements, you get the plain Stolen Warmth.

**The numbers.** Mix, 3 Shards. Both shapes: while the enemy has both afflictions, its Chill doesn't fade, and each of your weapon blows on it adds a Chill. Cold Shade: each blow also heals you 2 (1 against a god), at most four times a second. When the enemy dies, a patch 5 metres across stays for 4 seconds where it fell, chilling whatever stands in it once a second. The Long Night: no healing. While the enemy is frozen, everything within 3 metres gets a Chill once a second. After Stolen Warmth goes off on an enemy, it can't go off on that enemy again for 6 seconds.

**Works with.** Hel's Half, Deep Veins, No Summer, Ice in the Blood, Three Winters.

**Example.** Fighting alone and hurt: use Cold Shade, and heal as you hit. A pack around a big enemy: use The Long Night, freeze the big one, and let the cold take the rest.

**Not built yet.** For now, Cold Shade's shadow is left when the enemy dies instead of when it is shattered, and it is just a chilling patch. You can't hit it with a heavy blow to shatter it a second time yet.

## stolenwarmth.helshalf
**What it does.** Killing an enemy whose warmth is being stolen Curses every enemy within 4 metres and gives each one two Chill.

**When it happens.** Each kill, while you carry both Dark and Ice.

**The numbers.** Mix, 3 Shards. For now: any enemy that dies while both Cursed and chilled puts a Curse and two Chill on everything within 4 metres, at most once every half second. Since this mark adds both at once, it starts Stolen Warmth on each of them.

**Works with.** Stolen Warmth (it spreads from body to body), Three Winters, No Summer.

**Example.** Kill one enemy in the pack, and the cold spreads into the rest.

**Not built yet.** For now, it works on any enemy that dies Cursed and chilled, whether its warmth was being stolen or not.

## stolenwarmth.deepveins
**What it does.** Each blow on an enemy whose warmth is being stolen draws 3 health instead of 2, up to 12 a second.

**When it happens.** Each weapon blow on an enemy like that.

**The numbers.** Mix, a minor mark, 1 Shard. Each blow draws 3 health instead of 2 (1.5 from a god), at most four times a second: 12 a second. It's the same with Cold Shade.

**Works with.** Stolen Warmth and Cold Shade, Hel's Half.

**Example.** A long fight with one brute keeps you at full health.

## stolenwarmth.nosummer
**What it does.** Hunger costs no health while any enemy within 8 metres is Frozen.

**When it happens.** While you have Dark's Hunger as your surge, and carry both Dark and Ice.

**The numbers.** Mix, keystone, 5 Shards. For now: for 4 seconds after each shatter, costs paid with health are skipped.

**Works with.** Stolen Warmth (its Chill doesn't fade, so freezing comes easily), The Long Night, Three Winters, Dark's Hunger.

**Example.** Keep one enemy frozen near you, and feed Hunger for free.

**Not built yet.** It doesn't check yet whether an enemy near you is Frozen. For now, the cost is skipped for 4 seconds after each shatter, and for every cost paid with health, not only Hunger's.

## stolenwarmth.iceinblood
**What it does.** A sign of black frost is put on the enemy you face for 6 seconds. Each of your next four weapon blows on it freezes one of its veins, for 8 damage and two Chill. The fourth breaks it from the inside: 35 damage, 50 stagger, and a Curse and two Chill on everything within 3 metres.

**When it happens.** Cast it from a spell slot while you carry both Dark and Ice. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Dark.

**The numbers.** Mix, duo spell, detonator, 5 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. It reaches up to 8 metres. Your weapon blows set it off for 35 damage, 50 stagger, a Curse and two Chill to everything within 3 metres.

**Works with.** Stolen Warmth (everything it hits gets both afflictions), Hel's Half, Three Winters.

**Example.** Put the sign on the captain, land four blows, and its guard breaks in frost.

**Not built yet.** The veins aren't frozen one at a time yet. For now, your blows set off the sign directly, and it doesn't wait for the fourth blow.

## stolenwarmth.threewinters
**What it does.** A duo surge. While it is lit, frost creeps over the floor around you, and every enemy that dies freezes where it falls as a statue of black ice for 5 seconds. Each statue adds a stack of Chill every second within 2.5 metres. A heavy blow on a statue shatters it, dealing 25 damage, two Chill and a Curse within 3 metres.

**When it happens.** At a rest fire, pick it for your surge slot in the Hand while you carry both Dark and Ice. Light it with the surge button.

**The numbers.** Mix, duo surge, 5 Shards. 40 mana, lasts 10 seconds. For now: each enemy that dies leaves a patch 5 metres across for 5 seconds, chilling whatever stands in it once a second.

**Works with.** Stolen Warmth, Hel's Half, No Summer, Ice in the Blood.

**Example.** Light your surge in a crowd, and each death freezes the floor around it.

**Not built yet.** The frost on the floor isn't drawn yet, and no statue appears for you to shatter. For now, each death leaves a chilling patch.
