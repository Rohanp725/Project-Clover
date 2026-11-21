# The Grudge: detailed explanations

The long explanation the chart shows for every mark on the duo branch Dark and Lightning share, named for its reaction,
The Grudge (`docs/design/steel/pairs_d3.md`, "Dark and Lightning"). Drawn in the heart of the chart while Dark and
Lightning are both carried.

## thegrudge.shape
**What it does.** Unlocks two new shapes for The Grudge. At a rest fire, you pick which shape your Grudge uses. From then on, it uses that shape instead of the plain one. **First Strike**: the storm cloud doesn't wait for the enemy to attack you. Your first heavy blow on it calls the cloud down. It's for players who would rather not get hit. **Ash Plume**: the cloud grows with each of your blows and comes down as a storm of lightning bolts.

**When it happens.** Every time The Grudge goes off while you carry both Dark and Lightning. The Grudge goes off when a second Static lands on a Cursed enemy, or when a Curse lands on an enemy with two Static. If you have not bought this mark, or you carry only one of the two elements, you get the plain Grudge.

**The numbers.** Mix, 3 Shards. First Strike: the cloud waits for 8 seconds. Your first heavy blow on the enemy sets off all its Static at double strength, for 14 damage each within 4 metres, and holds it still for 0.6 seconds. Ash Plume: the cloud waits for up to 6 seconds. When the enemy next attacks you (whether the blow lands, is guarded, or is blocked by a ward), or when the time runs out, one bolt falls around it for each weapon blow you landed on it, up to five bolts, each dealing 10 damage and a Static. Then all its Static goes off within 4 metres. After The Grudge goes off on an enemy, it can't go off on that enemy again for 8 seconds.

**Works with.** Grievance to add the Static, The Exile Returns, Given Bones, The Exile's Grudge.

**Example.** A lone brute: use First Strike, and end the cloud with a heavy blow. A pack around one Cursed captain: use Ash Plume, and keep hitting the captain for six seconds.

**Not built yet.** The cloud doesn't move to the next enemy yet when the enemy under it dies.

## thegrudge.grievance
**What it does.** Blocking a blow from a Cursed enemy puts two Static on it.

**When it happens.** Each blow you block from a Cursed enemy.

**The numbers.** Mix, 3 Shards. Two Static, at most once every 0.3 seconds.

**Works with.** The Grudge (two Static on a Cursed enemy is all it needs), Long Memory, Given Bones.

**Example.** Curse a brute, then block its first swing. The cloud gathers over its head.

## thegrudge.exilereturns
**What it does.** The Grudge's black bolt heals you 1 for each Static it sets off, and Curses everything within 3 metres.

**When it happens.** Each time the Grudge's black bolt falls, in any of its shapes: when the enemy under the cloud attacks you, when your heavy blow calls it down (First Strike), or when Ash Plume's storm falls. Other ways of setting off Static don't count. You must carry both Dark and Lightning.

**The numbers.** Mix, 3 Shards. 1 health for each Static the bolt sets off, and one Curse on everything within 3 metres. At most once every 0.3 seconds.

**Works with.** The Grudge and its shapes, Given Bones, Ash and Thunder.

**Example.** Bring the cloud down on a charged captain. You heal, and the soldiers next to it are Cursed, ready for the next cloud.

## thegrudge.longmemory
**What it does.** A Curse on a charged enemy lasts as long as its Static does.

**When it happens.** Always on once you learn it, while you carry both Dark and Lightning.

**The numbers.** Mix, a minor mark, 1 Shard.

**Works with.** Grievance, which it comes after on the chart, The Grudge.

**Example.** The Curse doesn't run out before the cloud gathers.

**Not built yet.** It does nothing yet. The game can't yet make one affliction last as long as another one does.

## thegrudge.givenbones
**What it does.** The Grudge's black bolt costs you 2 health and hits twice as hard. The health you pay turns grey, and you win it back by landing blows.

**When it happens.** Each time the Grudge's bolt falls, in any of its shapes. Other ways of setting off Static don't count. You must carry both Dark and Lightning.

**The numbers.** Mix, keystone, 5 Shards. 2 health turns grey (it never takes your last point). The bolt hits a second time for 14 damage for each Static it sets off, within 4 metres. At most once every 0.3 seconds.

**Works with.** The Grudge, The Exile Returns (which heals back what it costs), Ash and Thunder.

**Example.** A Cursed brute has five Static. The bolt lands, then lands again.

## thegrudge.exilesgrudge
**What it does.** A black cloud hangs over you for 3 seconds. Whoever hits you or your guard during that time gets answered when the cloud breaks: a bolt that Curses it and gives it three Static.

**When it happens.** Cast it from a spell slot while you carry both Dark and Lightning. Press that slot's button (hold L2 and a face button, or press 1 to 4). The first time it lands, it charges your blade with Dark.

**The numbers.** Mix, duo spell, counter, 5 Shards. It costs no mana. After you cast it, you must wait 12 seconds to cast it again. Fighting with your weapon makes the wait shorter. The cloud stays for 3 seconds. The answer is a cone 4 metres long that deals 12 damage, 15 stagger, a Curse and three Static.

**Works with.** The Grudge (the answer leaves every attacker ready for it), Grievance, Given Bones.

**Example.** You're surrounded. Cast it, guard whatever comes, and every attacker is Cursed and charged.

**Not built yet.** Each attacker isn't tied to you by a thread yet and paid back twice the damage it did to you (up to 60). A blow on your guard doesn't count double yet.

## thegrudge.ashthunder
**What it does.** A duo surge. While it is lit, a cloud of ash streams up off your shoulders. Every blow you land adds a Static, every third one calls a bolt down on a Cursed enemy, and each Cursed enemy you kill heals you 6 more.

**When it happens.** At a rest fire, pick it for your surge slot in the Hand while you carry both Dark and Lightning. Light it with the surge button.

**The numbers.** Mix, duo surge, 5 Shards. 40 mana, lasts 10 seconds. Each kill while it is lit adds 1 second. For now: each blow adds one Static; one blow in three on a Cursed enemy sets off its Static; each Cursed enemy you kill heals you 6.

**Works with.** The Grudge, The Exile Returns, Given Bones.

**Example.** Curse a pack, light your surge, and the ash cloud keeps the storm clouds gathering.

**Not built yet.** For now, the bolt falls on one blow in three, by chance, on the enemy you hit, instead of on every third blow at the nearest Cursed enemy within 8 metres.
