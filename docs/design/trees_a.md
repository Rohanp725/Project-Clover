# The great chart, part A: Fire, Lightning, Wind, Ice, Water, Life

Written to `docs/design/TREE_BRIEF.md` on 2026-11-16. Covers six elements and the five mixed branches between them:
Plasma, Tempest, Blizzard, Glacier, Bloom. Earth, Metal, Light, Dark, Poison and their mixes are in `trees_b.md`.
Everything here is a proposal: nothing is built, every number is a first value, and nothing is Decided until the
user says so.

## How to read the tables

- **Kind and cost**: Root 2, Minor 1, Major 3, Spell 4, Keystone 6, Signature never sold. Mix marks carry their own
  cost (3 to 5) in a Cost column; a mix's Crown is never sold and is given by consuming the neighbour's god.
- **Grows from** is the id that must be learned first. A bough is learned from its foot outward.
- **Description** is the exact text the player reads. **Design** is what it does. A Design that starts with BESPOKE
  needs code beyond the brief's triggers, actions, numbers and spell forms; "small ask" marks a small addition to
  something the brief already lists.
- **Controls assumed**: tap Triangle is the element's base spell; hold Triangle is the one learned spell worn in the
  hold-spell slot; L2 is the surge; hold Square throws a flask where a mark says so; heavy on a staggered enemy is
  the finisher.
- **Words used in the text the player reads**: "stamina", "mana", "blow", "perfect guard", "perfect dodge", "mends"
  for healing, and the afflictions with a capital letter. Spell descriptions end with their mana cost, as Meteor
  Leap's does today.
- **Critical blows** do not exist in the build. Where a mark names one, it means a blow at one and a half times
  damage, drawn with the crossed cut line the finisher uses. Lightning's stored-charge blow and its discharges count
  as critical once criticals exist.
- **Elites and gods**: every hold, freeze, lift and pull has a shorter value for elites (the Gate Captain) and gods,
  given in the Design column. Nothing here takes a god out of a fight.

---

## Fire

**Idea.** Fire is the element of never letting up: every blow deepens a burn, and Heat pays the player who keeps
hitting and is not hit. A Fire player stands in the enemy's face, and when the enemy finally swings, the perfect
guard turns everything that has been built into one explosion.

**Base spell: Flame Wave** (tap, 25 mana). As built: a short committed cast, a cone of fire 5.5 m ahead, modest
damage, good stagger, sets a burn.

**Surge: Kindle** (L2, 40 mana, 10 s). As built: the weapon is wrapped in flame, 30% more damage, every blow sets a
burn, and Heat is filled at once. Made worth pressing: Kindle opens with a ring of flame round the player (2.5 m, 8
damage, 20 stagger, a short push), so it is also the way to make room and start an attack; and while it lasts no
burn on any enemy runs out.

**Affliction: Burn.** Up to five deep. One deep it deals 5 damage a second; each further stack adds 30%, so five
deep is 11 a second. It lasts 4 s and any blow or fire that sets it again tops it up. When it runs out all of it is
gone. A burning enemy cannot turn light blows on its guard (as built: fire beats the soldier's guard).

**Boughs.**

- **Pressure**: Heat and momentum. Keep hitting, never be hit, and the fire climbs.
- **Answer**: the perfect guard and the dodge turned into fire. The enemy's attack is the fuse.
- **Ruin**: big spells, burning ground and the flask. Choose where the fight stands and make that ground yours.

**What changes for the seven marks already built.** A new root, Cinder, carries the plain burn (the brief asks that
the root turn the affliction on), so Heat stops being the root and becomes the foot of Pressure at Major cost, and
Burn and Detonate keeps its id and name but its text loses its first sentence, which Cinder now says. Flame Trail
moves under Backdraft (the dodge belongs with the guard), Firebomb Flask grows straight from the root as the foot of
Ruin, Meteor Leap grows from the flask. **Plasma** (`fire.plasma`) moves to the Plasma mixed branch as its crown:
the brief says a mix's signature is given by consuming, which is exactly how Plasma is given today, and Fire had no
gift for a player who takes Fire from its god. Fire's new signature is **Falling Sun**.

| id | Name | Kind | Grows from | Bough | Description | Design |
|---|---|---|---|---|---|---|
| fire.cinder | Cinder | Root | none | Root | Every blow of the weapon sets a burn, and each blow deepens it, up to five. | Trigger: any weapon blow lands. Lay 1 Burn (4 s, topped up). New id; takes over the first half of Burn and Detonate as built. |
| fire.heat | Heat | Major | fire.cinder | Pressure | Blows landed without being struck build Heat, up to five. Each level adds damage. A blow taken puts it out. Kindle fills it at once. | As built: every 3 blows a level, +6% weapon damage a level, a level cools after 6 s without a blow. Was the root at cost 2. |
| fire.stoke | Stoked | Minor | fire.heat | Pressure | Heat rises on fewer blows. | A level every 2 blows, not 3. |
| fire.banked | Banked Coals | Minor | fire.heat | Pressure | Heat cools more slowly between blows. | Cooling 6 s to 9 s. |
| fire.crescent | Burning Crescent | Major | fire.heat | Pressure | The Iaido Slash throws a slow arc of flame that travels on after the cut. | Needs the Katana. As built: 8 m/s for 1.1 s, 18 damage and a 3 s burn, once per enemy. Added: each enemy it burns counts as a blow for Heat. |
| fire.catch | Catching Fire | Major | fire.heat | Pressure | A burning enemy that dies throws its burn onto everyone near it. | Trigger: a blow kills an afflicted enemy. Lay its Burn stacks on everything within 4 m. |
| fire.searing | Searing Edge | Minor | fire.catch | Pressure | Burns bite harder. | Burn strength +20%. |
| fire.fuse | Slow Fuse | Minor | fire.catch | Pressure | Burns last longer. | Burn length 4 s to 6 s. |
| fire.tinder | Dry Tinder | Minor | fire.catch | Pressure | Your weapon cuts deeper into a burning enemy. | Weapon damage to burning enemies +8%. |
| fire.rush | Wildfire Rush | Spell | fire.catch | Pressure | Hold the spell button to drive through the enemies ahead in a line of fire. Each one passed is set burning and raises your Heat. 35 mana. | Drill dash: 7 m in 0.35 s, untouchable while moving, ends behind them facing back. Each enemy passed: 14 damage, 20 stagger, 2 Burn, +1 Heat. The line burns 2.6 s as Flame Trail does. |
| fire.whiteheat | White Heat | Keystone | fire.rush | Pressure | Heat rises to eight, and above five your blows come faster. Struck above five, you burn. | Heat cap 8. Levels 6 to 8 each add +6% damage and +5% attack speed. A blow taken at 6 or more puts Heat out and burns the player for 4 damage a second for 4 s. |
| fire.backdraft | Backdraft | Major | fire.cinder | Answer | A perfect guard throws a cone of fire back at the one who struck. | As built: 4.5 m, 100 degrees, 16 damage, 18 stagger. Now lays 2 Burn. |
| fire.flint | Struck Flint | Minor | fire.backdraft | Answer | A perfect guard gives more mana. | 15 to 21. |
| fire.breath | Furnace Breath | Minor | fire.backdraft | Answer | Backdraft breaks balance harder. | Backdraft stagger 18 to 28. |
| fire.detonate | Burn and Detonate | Major | fire.backdraft | Answer | A perfect guard against a burning enemy sets all of its burn off at once. | As built: 10 damage plus 10 a stack, the stacks are spent. Text shortened: the stacking half now lives in Cinder. Cost falls from 5 to 3. |
| fire.hearth | Wide Hearth | Minor | fire.detonate | Answer | Backdraft and the detonation reach further. | Cone reach and burst radius +25%. |
| fire.trail | Flame Trail | Major | fire.backdraft | Answer | The dodge leaves a line of fire behind it that burns what follows. | As built: four patches for 2.6 s, 4 damage twice a second, sets Burn. Used to grow from Heat. |
| fire.embers | Long Embers | Minor | fire.trail | Answer | Flame Trail burns longer on the ground. | 2.6 s to 4 s. |
| fire.footing | Hot Footing | Minor | fire.trail | Answer | Flame Trail bites harder. | Patch damage 4 to 6. |
| fire.flash | Flashpoint | Major | fire.trail | Answer | After a perfect dodge, your next blow sets a burn three deep at once. | Trigger: perfect dodge. Boon for 3 s: the next weapon blow lays 3 Burn in place of 1. |
| fire.kiln | Kiln Stance | Spell | fire.detonate | Answer | Hold the spell button to stand ready in flame. The first blow that reaches you, red or not, is turned and answered with a pillar of fire. 30 mana. | Elemental parry. The stance holds while the button is held, up to 1.2 s. A blow caught does nothing to the player and counts as a perfect guard (mana, Backdraft, the detonation); a pillar under the attacker deals 30 damage, 50 stagger, 3 Burn. Nothing caught: the mana is gone and the player is open for 0.4 s. |
| fire.furnace | Open Furnace | Keystone | fire.kiln | Answer | A perfect guard sets off every burning enemy near you, not only the one who struck, and leaves each still alight. A plain guard lets more of the blow through. | The detonation reaches every burning enemy within 8 m, each bursting where it stands and keeping 2 Burn. Grey damage through a plain guard 35% to 60%. |
| fire.firebomb | Firebomb Flask | Major | fire.cinder | Ruin | Hold the flask button to throw it. No healing: a great burst of fire where it lands. | As built: up to 9 m, 36 damage, 30 stagger in 2.6 m, the ground burns 3.5 s. Now lays 3 Burn. Used to grow from Flame Trail at cost 5. |
| fire.broadwave | Broad Wave | Minor | fire.firebomb | Ruin | Flame Wave reaches further. | 5.5 m to 7 m. |
| fire.shortbreath | Short Breath | Minor | fire.firebomb | Ruin | Flame Wave costs less mana. | 25 to 20. |
| fire.meteor | Meteor Leap | Spell | fire.firebomb | Ruin | Hold the spell button to leap out of the fight and come down on the enemy faced, leaving a burning crater. 50 mana. | As built: 0.6 s leap, untouchable in the air, up to 9 m, 46 damage and 55 stagger in 3.2 m, crater burns 4 s. Cost falls from 6 embers to 4. |
| fire.coals | Hot Coals | Minor | fire.meteor | Ruin | Spells strike burning enemies harder. | Spell damage to burning enemies +20%. |
| fire.longkindle | Long Kindle | Minor | fire.meteor | Ruin | Kindle lasts longer. | 10 s to 13 s. |
| fire.sunspot | Sunspot | Spell | fire.meteor | Ruin | Hold the spell button to brand the enemy faced. Each blow you land feeds the brand, up to six. A heavy blow sets it off. 35 mana. | A mark that detonates when struck. Reach 6 m, lasts 7 s. A ring on the enemy grows a step with each light blow fed. Heavy blow: burst of 12 plus 9 a blow fed, stagger 10 plus 8 a blow fed, 3 m, and the target's Burn is spread to all it reaches. If the time runs out it bursts at half. |
| fire.scorched | Scorched Earth | Keystone | fire.sunspot | Ruin | Fire you leave on the ground burns twice as long. While you stand in it Kindle does not run down and your blows give more mana. Your spells cost more. | Trail, crater and flask fire last twice as long. Standing in own fire: Kindle's clock stops, mana from blows +50%. All spell costs +20%. |
| fire.sun | Falling Sun | Signature | none | Crown | With Heat at five and Kindle lit, hold the surge button to raise a sun over the fight. While it hangs, every burn near you deepens by itself. Press again to bring it down. A blow taken puts it out. | BESPOKE. Pressed: hold L2 0.5 s while Kindle lasts and Heat is five; no further mana. Seen: the player thrusts the weapon overhead and holds; a hard-edged disc of orange light 12 m across spreads over the ground and every cast shadow under it shrinks to nothing; a small sun with an ink ring hangs at the top of the frame. While it hangs (up to 10 s): Kindle and Heat do not run down, every enemy on the disc gains 1 Burn a second and no burn expires. L2 again, or the time ending: the disc closes in 0.6 s to a 4 m red circle on the enemy last struck and the sun lands there: 30 damage plus 8 for every Burn stack on every enemy that stood on the disc (capped at 150), 80 stagger, in 4 m; the crater burns 5 s. A blow taken while it hangs puts out the sun and Heat together and nothing falls. Either way Kindle ends and Heat is spent. |

Counts: 1 root, 14 minors, 8 majors, 4 spells, 3 keystones, 1 signature: 31. One BESPOKE. One Katana mark.

---

## Lightning

**Idea.** Lightning is speed and the single right moment: quick cuts load an enemy with Static and one heavy blow
spends it. A Lightning player is never where the blow lands, banks perfect guards as charges, and decides when the
whole fight goes off at once.

**Base spell: Thunderclap** (tap, 25 mana). As built: a bolt on the enemy faced, up to ten metres off, light on
damage and very heavy on stagger, leaping to two more. It opens a guard for the blade; it does not kill.

**Surge: Overdrive** (L2, 40 mana, 10 s). As built: every swing 30% faster with added stagger. Made worth pressing,
and to honour the user's "critical damage up": while it lasts the last blow of every chain is critical, and every
discharge of Static is critical too.

**Affliction: Static.** Light blows build it, up to five; it stays 6 s after the last blow. A heavy blow, a
finisher or the weapon skill discharges it: 7 damage and 5 stagger a stack to the one struck and to everyone within
4 m (as built).

**Boughs.**

- **Flicker**: the dodge. Blink, the copy left behind, and never being there.
- **Charge**: bank it and spend it. Perfect guards stored, Static held, one blow that counts.
- **Thunder**: the sky and the surge. Bolts, the rod, and Overdrive itself.

**What changes for the seven marks already built.** Static stays the root. Stored Charge, Blink, Afterimage and
Delayed Cuts are Majors (Afterimage and Delayed Cuts fall from cost 5 to 3). Lightning Rod is a Spell, grown to
what was first written: a real rod and a warp. It now grows from Conductor, a new Major at the foot of Thunder.
Overclock stays where it is, at the end of its bough at cost 6, and becomes the Thunder keystone with a second
sentence and a price. **Storm Step** stays the signature, unchanged.

| id | Name | Kind | Grows from | Bough | Description | Design |
|---|---|---|---|---|---|---|
| lightning.static | Static | Root | none | Root | Blows build Static on an enemy, up to five. A heavy blow or a finisher discharges it, and it arcs to everyone near. | As built. |
| lightning.blink | Blink | Major | lightning.static | Flicker | The dodge becomes a short step through lightning that shocks whatever it passes through. | As built (10 damage, 14 stagger), and now a true step: the player is gone for the first 0.1 s and passes through enemies. |
| lightning.bitingstep | Biting Step | Minor | lightning.blink | Flicker | Blink shocks harder. | Blink damage and stagger +40%. |
| lightning.forked | Forked Step | Minor | lightning.blink | Flicker | Blink shocks a wider path. | The shocked lane 1 m to 1.8 m wide. |
| lightning.return | Quick Return | Minor | lightning.blink | Flicker | A dodge that breaks off an attack costs less stamina. | Stamina cost of a dodge used to cancel a recovery -40%. |
| lightning.afterimage | Afterimage | Major | lightning.blink | Flicker | A dodge at the last moment leaves a copy of you that strikes where you stood. | As built (0.3 s later, 26 damage, 28 stagger in 2.3 m), and now the copy repeats the player's last blow, laying its Static. |
| lightning.echo | Bright Echo | Minor | lightning.afterimage | Flicker | The copy left by Afterimage strikes harder. | Copy damage and stagger +30%. |
| lightning.chargedstep | Charged Step | Minor | lightning.afterimage | Flicker | A perfect dodge gives more mana. | 12 to 18. |
| lightning.flashes | Seven Flashes | Spell | lightning.afterimage | Flicker | Hold the spell button to cut through up to seven enemies in one breath and return to where you stood. The cuts land together when you are still. 45 mana. | BESPOKE. Pressed: hold Triangle 0.3 s. Seen: the player is gone; over 0.7 s seven straight lines of blue light are drawn on the ground from enemy to enemy within 8 m (against one enemy, seven lines through it from seven sides, a star); the player reappears on the starting spot with their back to the fight and holds still for 0.5 s; then every line flares white and the cuts land. Each cut: 9 damage, 8 stagger, 1 Static; then any enemy at five discharges. Untouchable while gone; open during the held pose. Struck during the pose: the cuts land at half and nothing discharges. |
| lightning.neverthere | Never There | Keystone | lightning.flashes | Flicker | Every dodge carries further and a perfect dodge costs nothing. Your guard holds half as well. | Dodge distance +30%; a perfect dodge refunds its stamina. Guard stamina cost per point of damage doubles (0.9 to 1.8). |
| lightning.charge | Stored Charge | Major | lightning.static | Charge | A perfect guard banks a charge. The next blow spends it for double damage and stagger. | As built. The charged blow is critical. |
| lightning.heavyair | Heavy Air | Minor | lightning.charge | Charge | Static stays longer on an enemy. | 6 s to 10 s. |
| lightning.longarc | Long Arc | Minor | lightning.charge | Charge | A discharge arcs further. | 4 m to 5.5 m. |
| lightning.cuts | Delayed Cuts | Major | lightning.charge | Charge | The Iaido Slash passes through. Its cuts land a moment later, all at once, when the blade is home. | Needs the Katana. As first written: the slash deals nothing as it passes; 0.55 s later, on the click of the sheath, three cuts land for 160% of the slash in all and discharge Static. |
| lightning.reserve | Deep Reserve | Major | lightning.charge | Charge | Perfect guards bank up to three charges. Each blow spends one. | Three pips by the mana bar. Each charged blow is double damage and stagger, as Stored Charge. |
| lightning.hardcharge | Hard Charge | Minor | lightning.reserve | Charge | A stored charge breaks balance harder. | Stagger of a charged blow from double to two and a half times. |
| lightning.keen | Keen Edge | Minor | lightning.reserve | Charge | Critical blows cut deeper. | Critical damage +20%. |
| lightning.seam | Open Seam | Minor | lightning.reserve | Charge | Blows on an enemy carrying Static are critical more often. | Critical chance +8% against enemies with Static. |
| lightning.fulldischarge | Full Discharge | Major | lightning.reserve | Charge | A discharge of five Static holds its enemy still for a moment and gives mana. | Trigger: an affliction at full depth is set off. The target is held 0.8 s (elites 0.5 s; gods flinch only) and the player gains 10 mana. |
| lightning.waiting | Waiting Thunder | Spell | lightning.fulldischarge | Charge | Hold the spell button to hang five bolts at your shoulder. Each blow you land sends one down with it. A heavy blow sends all that are left. 35 mana. | Phalanx blades that fire on the player's blows. They hang 10 s. Light blow: one bolt on the target, 8 damage, 1 Static. Heavy blow: every bolt left, each 8 damage and 6 stagger, landing with the blow before it discharges. |
| lightning.stroke | Single Stroke | Keystone | lightning.waiting | Charge | Static builds to ten. A discharge no longer arcs: all of it lands on the one struck, twice as hard. | Static cap 10. Discharge: 14 damage and 10 stagger a stack to the target only. Full Discharge still triggers at five or more. |
| lightning.conductor | Conductor | Major | lightning.static | Thunder | Thunderclap leaves Static two deep on all it strikes, and sets off an enemy already at five. | Trigger: a spell hits. Lay 2 Static; a target at five is discharged by the bolt. |
| lightning.quickclap | Quick Clap | Minor | lightning.conductor | Thunder | Thunderclap costs less mana. | 25 to 20. |
| lightning.thirdleap | Third Leap | Minor | lightning.conductor | Thunder | Thunderclap leaps to one more enemy. | Leaps 2 to 3. |
| lightning.rod | Lightning Rod | Spell | lightning.conductor | Thunder | Hold the spell button to plant a rod. Bolts fall around it for a while. Hold again to step to it in a flash. 40 mana. | A planted totem with a step. The rod stands 2 m ahead for 8 s; every 0.9 s a bolt lands on an enemy within 3.5 m of it for 12 damage and 1 Static. Holding Triangle again while it stands costs nothing: the player steps to the rod, arriving with a 2.5 m shock (14 damage, 20 stagger), and the rod is spent. Text gains its third sentence. |
| lightning.wire | Live Wire | Major | lightning.rod | Thunder | While a rod stands, a wire of lightning runs from it to you. Enemies it crosses are shocked. | Small ask: a line hazard whose far end follows the player. Up to 10 m long; every 0.5 s whatever stands on it takes 5 damage and 1 Static. The fight becomes keeping the enemy between you and the rod. |
| lightning.longdrive | Long Overdrive | Minor | lightning.wire | Thunder | Overdrive lasts longer. | 10 s to 13 s. |
| lightning.current | Running Current | Minor | lightning.wire | Thunder | Your blows give more mana. | Mana from blows +15%. |
| lightning.lodestone | Lodestone | Spell | lightning.wire | Thunder | Hold the spell button to drag every enemy near toward you, further for each Static on it, then set all of it off at once. 45 mana. | Reverse blast. Radius 7 m: each enemy is pulled 1 m plus 0.8 m a stack; 0.3 s later a 3.5 m burst discharges every enemy in it (as the root) with 15 stagger more. Sets up one wide swing. |
| lightning.overclock | Overclock | Keystone | lightning.lodestone | Thunder | Overdrive opens with a breath in which everything but you is slowed. Each perfect guard or perfect dodge while it lasts opens another. Overdrive costs more mana. | As built: 2.5 s with enemies at 35% speed. Added: each perfect guard or perfect dodge during Overdrive gives 1 s more of it. Overdrive 40 to 55 mana. If the user wants Overclock left exactly as it is, drop the two added sentences and the price. |
| lightning.stormstep | Storm Step | Signature | none | Crown | While Overdrive lasts, every dodge costs nothing and shocks what it passes through. | As built: no stamina, the Blink shock, and a shape of light left where the player stood. |

Counts: 1 root, 14 minors, 8 majors, 4 spells, 3 keystones, 1 signature: 31. One BESPOKE (Seven Flashes) and one
small ask (Live Wire). One Katana mark.

---

## Wind

**Idea.** Wind fights in motion: the dodge is a weapon, the enemy is pushed, turned round and thrown into the air,
and nothing it does is aimed at where the player still is. A Wind player wins by where everyone is standing, or by
making sure the enemy is not standing at all.

**Base spell: Gust** (tap, 20 mana). A short cone of wind 5 m ahead: 8 damage, 2 Unsteady, and a 3 m push. It does
little harm by itself; it is for throwing an enemy into a wall, into its friends, or off its attack.

**Surge: Tailwind** (L2, 35 mana, 10 s). The user's "better dodge": the dodge costs half its stamina, carries 30%
further and is safe a little longer, and the player moves 15% faster. Made worth pressing: the first blow after
every dodge during Tailwind is a gale blow, 40% harder, that pushes its target 2 m. So the surge asks for a rhythm
of dodge, blow, dodge, blow.

**Affliction: Unsteady.** Up to five deep, 6 s, topped up by any blow. Each stack: pushed 15% further, turns 8%
slower, takes 6% more stagger. Nothing more at the root; the Updraft bough is where five deep becomes a launch.

**In the air.** Several marks throw an enemy **into the air**. This is one new state, described once in Rising Cut
and used wherever a Design says "into the air". Ordinary enemies and human-sized elites and gods can be lifted;
each lift of an elite or a god is 60% as long, and a second lift inside 10 s is shorter again. A colossal form
cannot be lifted: it **reels** instead (a 1 s flinch that does not break an attack already shown, and 40 stagger),
and a reeling enemy counts as "in the air" for every bonus.

**Boughs.**

- **Slipstream**: the dodge as a weapon. Pass through, cut, be behind.
- **Updraft**: launch and juggle. Put it in the air and keep it there.
- **Gale**: push and pull. Walls, crowds, and the perfect guard that throws.

| id | Name | Kind | Grows from | Bough | Description | Design |
|---|---|---|---|---|---|---|
| wind.unsteady | Unsteady | Root | none | Root | Blows knock an enemy Unsteady, up to five. It is pushed further, turns slower and loses its balance sooner. | Trigger: any weapon blow lands. Lay 1 Unsteady (6 s). Values as above. |
| wind.slip | Slipstream | Major | wind.unsteady | Slipstream | The first blow after any dodge strikes harder and leaves its enemy more Unsteady. | Trigger: a dodge ends. Boon for 1.2 s: the next weapon blow deals +30% and lays 2 Unsteady more. |
| wind.stride | Long Stride | Minor | wind.slip | Slipstream | The dodge carries further. | Dodge distance +20%. |
| wind.thinair | Thin Air | Minor | wind.slip | Slipstream | The dodge is safe a moment longer. | Safe time +0.04 s. |
| wind.draft | Following Draft | Minor | wind.slip | Slipstream | Slipstream waits longer for its blow. | 1.2 s to 2 s. |
| wind.passing | Passing Cut | Major | wind.slip | Slipstream | Dodge through an enemy to cut it as you pass. It is left facing where you were. | The dodge passes through enemies. Each one passed: 10 damage, 1 Unsteady, and it cannot turn for 0.8 s (elites and gods 0.4 s), so its back and its guard's blind side are open. |
| wind.cleanpass | Clean Pass | Minor | wind.passing | Slipstream | Passing Cut bites deeper. | Its damage +50%. |
| wind.lightfeet | Light Feet | Minor | wind.passing | Slipstream | The dodge costs less stamina. | -20%. |
| wind.emptyair | Empty Air | Major | wind.passing | Slipstream | A perfect dodge gives back its stamina and leaves the one who struck three deeper in Unsteady. | Trigger: perfect dodge. Give the dodge's stamina; lay 3 Unsteady on the attacker. |
| wind.circle | Closing Circle | Spell | wind.emptyair | Slipstream | Hold the spell button to run a ring round the enemy faced. When the ring closes, the wind inside it rises and takes everything off the ground. 35 mana. | BESPOKE. Pressed: hold Triangle 0.3 s with an enemy within 5 m. Seen: the player runs a full circle of 3 m radius round it in 0.7 s, drawing a pale green line on the ground behind them; when the line meets its own start it snaps inward in 0.2 s and a column of wind stands in it. Everything inside takes 5 Unsteady and goes into the air (1.2 s). The player ends where they began, facing in. Untouchable for the first half of the run; a blow taken in the second half breaks the line and the mana is lost. |
| wind.leaf | Leaf on the Wind | Keystone | wind.circle | Slipstream | The dodge costs no stamina. It spends a breath of wind instead: you hold three, and blows win them back. You are easier to wound. | Three pips. One returns every 2 s and one for every 3 blows landed. With none, no dodge. Damage taken +20%. |
| wind.rising | Rising Cut | Major | wind.unsteady | Updraft | A heavy blow on an enemy five deep in Unsteady throws it into the air. It hangs there helpless, and each blow holds it up a little longer, up to three. | BESPOKE: the state "in the air". Trigger: a heavy blow lands on an enemy at five Unsteady; the stacks are spent. Seen: the enemy rises about 2 m toward the camera, its shadow shrinks under it and a thin ring marks where it will come down; three pips over it count the blows. It hangs 1.0 s and cannot act; each blow adds 0.45 s, three times at most; the player's swings reach it from the ground. It lands lying for 0.6 s and takes 20 stagger. Limits for elites, gods and colossal forms as written above. |
| wind.loose | Loose Footing | Minor | wind.rising | Updraft | Heavy blows leave an enemy two deeper in Unsteady. | Heavy blows lay 2, not 1. |
| wind.offbalance | Off Balance | Minor | wind.rising | Updraft | Unsteady lasts longer. | 6 s to 9 s. |
| wind.hammer | Hammer Down | Major | wind.rising | Updraft | A heavy blow on an enemy in the air drives it into the ground. The landing bursts. | The heavy blow deals one and a half times and ends the lift at once. Burst where it lands: 14 damage, 30 stagger and a 2 m push in 3 m. |
| wind.hang | Hang Time | Minor | wind.hammer | Updraft | Enemies thrown into the air hang longer. | +0.4 s. |
| wind.openair | Open Air | Minor | wind.hammer | Updraft | Blows strike an enemy in the air harder. | Damage to enemies in the air +20%. |
| wind.lifting | Lifting Wind | Spell | wind.hammer | Updraft | Hold the spell button to raise a column of wind under the enemy faced. It goes up whatever its footing. 30 mana. | A pillar after a delay. Reach 5 m; a 2 m circle is drawn for 0.35 s, then the column: 8 damage and into the air, with no Unsteady needed. |
| wind.skydance | Sky Dance | Spell | wind.lifting | Updraft | With an enemy in the air, hold the spell button to go up after it. Strike in time, up to five, then send it down where you choose. 45 mana. | BESPOKE. Pressed: hold Triangle with an enemy in the air within 6 m; then the light button in time; then the heavy button with the stick. Seen: the player leaps and both rise toward the camera, drawn a quarter larger, their two shadows side by side on the ground; the world below dims a step. Five times, 0.25 s apart, a ring closes on the pair: a light press as it closes is a cut for 10 damage and 6 mana; a missed press ends the dance early. Then the stick moves a 3 m circle on the ground within 6 m and the heavy button throws the enemy into it: 20 damage plus 6 for each cut in time, 60 stagger, in 3 m, and everything else there is pushed 3 m. Untouchable while up (1.8 s at most); the player lands beside the circle, open for 0.3 s. On a reeling colossus: the player goes up onto the fallen head or hand, the same five cuts, and the last blow bursts where it lies. |
| wind.nothing | Nothing Beneath | Keystone | wind.skydance | Updraft | An enemy in the air stays there for as long as you keep striking it, and each blow up there gives double mana. Your blows on an enemy that is not Unsteady are weaker. | No limit of three: each blow adds hang time, 15% less each time. Mana from blows on enemies in the air doubled. Weapon damage to enemies with no Unsteady -15%. |
| wind.wall | Against the Wall | Major | wind.unsteady | Gale | An enemy pushed into a wall or into another enemy is hurt and shaken by it. So is the one it hits. | Small ask: a pushed enemy that meets a wall or another enemy stops there. 12 damage, 30 stagger and 2 Unsteady, to both if it met an enemy. |
| wind.widegust | Wide Gust | Minor | wind.wall | Gale | Gust spreads wider and reaches further. | Cone 70 to 100 degrees, 5 m to 6 m. |
| wind.easybreath | Easy Breath | Minor | wind.wall | Gale | Gust costs less mana. | 20 to 15. |
| wind.turned | Turned Aside | Major | wind.wall | Gale | A perfect guard throws the attacker back and leaves it three deeper in Unsteady. A thrown attack is sent back to its thrower. | Trigger: perfect guard. Push 3 m, 3 Unsteady. A projectile guarded perfectly returns at its own damage. |
| wind.strongarm | Strong Arm | Minor | wind.turned | Gale | Your pushes throw enemies further. | All push distances +25%. |
| wind.fairwind | Fair Wind | Minor | wind.turned | Gale | Tailwind costs less mana. | 35 to 28. |
| wind.runningstart | Running Start | Minor | wind.turned | Gale | You move faster while Tailwind lasts. | 15% to 25%. |
| wind.drawing | Drawing Wind | Major | wind.turned | Gale | The Iaido Slash draws breath first. Everything before you is pulled onto the cut. | Needs the Katana. As the draw is released, enemies in a 6 m, 90 degree cone ahead are pulled 2.5 m toward the line of the cut 0.15 s before it lands, and take 2 Unsteady. |
| wind.indrawn | Indrawn Breath | Spell | wind.turned | Gale | Hold the spell button to drag everything near into one place before you, then burst it apart. 35 mana. | Reverse blast. Everything within 7 m is drawn over 0.5 s to a point 2 m ahead of the player; then a burst: 10 damage, 25 stagger, 2 Unsteady and a 2 m push outward. Enemies thrown into each other set off Against the Wall. |
| wind.front | Storm Front | Keystone | wind.indrawn | Gale | Every heavy blow throws a Gust with it at no cost. Your blows give less mana. | A free Gust (with its own upgrades) on every heavy blow and finisher, aimed as the blow. Mana from blows -25%. |
| wind.remembers | The Wind Remembers | Signature | none | Crown | While Tailwind lasts, every blow you land hangs in the air where it fell. When Tailwind ends, all of them fall again at once. | BESPOKE. Pressed: nothing new; L2 again ends Tailwind early. Seen: every blow landed during Tailwind leaves a pale green crescent frozen in the air at the place and angle of the cut, up to twenty. When Tailwind ends the player lowers the weapon to the sheath or the side (0.4 s; a dodge may cut the pose short without losing the cuts) and the crescents flare white in the order they were made, over 0.6 s. Each repeats its blow at 35% damage and full stagger against whatever stands in it now; a crescent with nothing in it flies to the nearest enemy within 4 m and lands at 20%. A blow taken while they hang scatters the oldest half. No cost beyond Tailwind's. It rewards holding an enemy in one place, or pulling it back to where the cuts are waiting. |

Counts: 1 root, 14 minors, 8 majors, 4 spells, 3 keystones, 1 signature: 31. Four BESPOKE (in the air, Closing
Circle, Sky Dance, The Wind Remembers) and one small ask (Against the Wall). One Katana mark.

---

## Ice

**Idea.** Ice is patience: every blow slows the enemy until its attacks are easy to read, and when it stops moving
altogether one heavy blow breaks it. An Ice player gives up speed for certainty, stands still on purpose, and
spends a whole exchange on a single strike.

**Base spell: Hoarfrost** (tap, 25 mana). A fan of ice shards 4.5 m ahead: 10 damage, 15 stagger, 2 Chill.

**Surge: Glaze** (L2, 40 mana, 12 s). The user's "barrier that blocks the next hit": a shell of ice that takes one
blow of any kind in the player's place. Made worth pressing: while it stands, a blow does not break the player's
heavy attack (the shell takes the blow and the swing goes on), so Glaze is what lets a slow, committed strike be
thrown into an enemy's attack; and a shell that lasts its whole time without breaking gives 20 mana back.

**Affliction: Chill.** Up to five deep, 6 s, topped up by any blow. Each stack slows the enemy's movement and its
attacks by 7%. At five it is **Frozen**: held still for 2.5 s (elites 1.6 s; gods 1.2 s, and a god cannot be
chilled again for 8 s). A heavy blow, a finisher or the weapon skill on a Frozen enemy **shatters** it: that blow
deals double, the freeze ends, and shards hit everything within 3 m for 10. Light blows on a Frozen enemy land as
usual and do not free it. When a freeze ends its Chill is gone.

**Boughs.**

- **Stillness**: the guard and the wait. Stand, read, and make the one blow heavier.
- **Shatter**: freeze and break. Everything that makes the breaking worth the patience.
- **Winter**: ice on the ground and the shell. Shape the place and the fight slows to your pace.

| id | Name | Kind | Grows from | Bough | Description | Design |
|---|---|---|---|---|---|---|
| ice.chill | Chill | Root | none | Root | Blows chill an enemy, up to five. Each slows it. At five it is Frozen, and a heavy blow shatters it. | Trigger: any weapon blow lands. Lay 1 Chill. Freeze and shatter as above. |
| ice.coldread | Cold Read | Major | ice.chill | Stillness | A perfect guard chills the one who struck, two deep. | Trigger: perfect guard. Lay 2 Chill on the attacker. |
| ice.patient | Patient Guard | Minor | ice.coldread | Stillness | The moment for a perfect guard is a little longer. | 0.18 s to 0.21 s. |
| ice.braced | Braced | Minor | ice.coldread | Stillness | Guarding costs less stamina. | 0.9 to 0.75 a point of damage. |
| ice.gather | Gathering Frost | Major | ice.coldread | Stillness | Hold your ground without striking and frost gathers at your feet, up to three rings. Your next blow spends them, harder and colder for each. | BESPOKE (a new trigger: time in a fight with no attack and no dodge; walking and guarding are allowed). Seen: a white ring is drawn round the player's feet every 0.8 s, three at most, each wider than the last. The next weapon blow spends them: +20% damage and 1 Chill more for each ring. A dodge scatters them. |
| ice.snow | Heavy Snow | Minor | ice.gather | Stillness | Heavy blows break balance harder. | Stagger from heavy blows +15%. |
| ice.slowpulse | Slow Pulse | Minor | ice.gather | Stillness | You take less harm while your guard is up. | Grey damage through a plain guard 35% to 28%; a guard break deals 15% less. |
| ice.statue | Hollow Statue | Spell | ice.gather | Stillness | Hold the spell button to step back and leave a shape of ice where you stood. Struck, it bursts and chills. Strike it yourself with a heavy blow and it bursts forward, freezing. 30 mana. | A clone that is also a wall that bursts. The player steps 3 m back (0.2 s, safe); the statue stands 6 s and enemies nearer to it than to the player attack it. Struck by an enemy: 15 damage and 3 Chill in 3.5 m. Struck by the player's heavy blow: a 7 m, 60 degree cone away from the player, 22 damage and 5 Chill. |
| ice.unmoved | Unmoved | Keystone | ice.statue | Stillness | Nothing breaks your heavy blows, and a blow that lands on you while you swing does half. Your dodge is short and costs more. | No flinch from the start of a heavy attack or a held draw until it lands; damage taken in that time halved. Dodge distance -30%, dodge stamina +50%. |
| ice.shards | Flying Shards | Major | ice.chill | Shatter | A shattered enemy throws its ice wide. Everyone it reaches is chilled two deep. | Shard burst 3 m to 4.5 m, 10 to 14 damage, and 2 Chill, which can freeze the next. |
| ice.deepcold | Deep Cold | Minor | ice.shards | Shatter | Chill slows more. | 7% to 9% a stack. |
| ice.longwinter | Long Winter | Minor | ice.shards | Shatter | Chill lasts longer. | 6 s to 9 s. |
| ice.coldiron | Cold Iron | Minor | ice.shards | Shatter | Heavy blows chill two deep. | Heavy blows lay 2, not 1. |
| ice.winterdraw | Winter Draw | Major | ice.shards | Shatter | The Iaido Slash shatters a Frozen enemy and carries on through everyone behind it. | Needs the Katana. After a shatter the cut runs on as a straight lane 8 m long behind the target: 60% of the slash and 2 Chill to each enemy in it. |
| ice.brittle | Brittle | Major | ice.shards | Shatter | An enemy that thaws or is shattered stays brittle for a while. Blows on it cut deeper and break its balance sooner. | For 5 s after a freeze ends: weapon damage and stagger taken +20%. This is what a freeze is worth against a god that lives through the shatter. |
| ice.heldfast | Held Fast | Minor | ice.brittle | Shatter | Frozen enemies stay Frozen longer. | 2.5 s to 3.5 s (elites 1.6 to 2.2, gods 1.2 to 1.6). |
| ice.sharpbreak | Sharp Break | Minor | ice.brittle | Shatter | Shattering strikes harder. | Double to 2.4 times. |
| ice.cleanbreak | Clean Break | Major | ice.brittle | Shatter | Shatter an enemy with little life left and nothing is left of it. | A shatter on an enemy below a fifth of its health kills it. Not gods. |
| ice.hanging | Hanging Ice | Spell | ice.brittle | Shatter | Hold the spell button to hang a great blade of ice over the enemy faced. It falls in a few moments. If the enemy is Frozen when it lands, it is shattered three times over. 40 mana. | A blade from above after a delay. Reach 7 m. The blade follows its target; its shadow grows on the ground for 3 s. It lands in 2 m for 30 damage, 40 stagger and 3 Chill; on a Frozen target it shatters it for 90 in place of 30. The player has three seconds to make the freeze happen. |
| ice.eight | Eight Points | Spell | ice.hanging | Shatter | Hold the spell button to set eight shards in a ring round the enemy faced. They close on it together. 45 mana. | Ring of many bullets. Reach 6 m; eight shards stand on a 3.5 m ring for 1.2 s, then fly to its middle. Each: 5 damage, 1 Chill. All eight on one enemy freeze it; an enemy that walks out of the ring is missed; one standing in a shard's way takes that shard. |
| ice.allatonce | All at Once | Keystone | ice.eight | Shatter | Shattering strikes three times as hard. Chill no longer slows anyone. | Shatter multiplier 2 to 3 (Sharp Break adds 0.4). Chill's slowing is removed; freezing at five stays. |
| ice.blackice | Black Ice | Major | ice.chill | Winter | Hoarfrost leaves a sheet of ice on the ground. Enemies standing on it grow colder and are pushed further. | Trigger: a spell is cast. The cone's ground is a hazard for 4 s: 1 Chill a second, and pushes on enemies there carry 50% further. |
| ice.thinice | Thin Ice | Minor | ice.blackice | Winter | Hoarfrost costs less mana. | 25 to 20. |
| ice.longfrost | Long Frost | Minor | ice.blackice | Winter | Hoarfrost reaches further. | 4.5 m to 6 m. |
| ice.splinter | Splinter Ward | Major | ice.blackice | Winter | When Glaze breaks, it bursts on the one who broke it. The blow counts as a perfect guard. | Trigger: the ward is spent. Burst 3.5 m: 12 damage, 3 Chill. Then everything a perfect guard gives: mana, stagger thrown back, Cold Read. |
| ice.thick | Thick Glaze | Minor | ice.splinter | Winter | Glaze lasts longer. | 12 s to 18 s. |
| ice.clearice | Clear Ice | Minor | ice.splinter | Winter | Glaze costs less mana. | 40 to 32. |
| ice.bitter | Bitter Cold | Minor | ice.splinter | Winter | Spells strike chilled enemies harder. | Spell damage to chilled enemies +20%. |
| ice.wall | Winter Wall | Spell | ice.splinter | Winter | Hold the spell button to raise a wall of ice before you. It stops blows and chills what touches it. Your heavy blow bursts it forward. 35 mana. | A wall that blocks and bursts. 5 m wide, 2 m ahead, 6 s, 60 health. Enemies touching it gain 1 Chill a second. The player's heavy blow on it: a 6 m cone away from the player, 20 damage, 30 stagger, 3 Chill. |
| ice.skin | Second Skin | Keystone | ice.wall | Winter | Glaze returns by itself a while after it breaks, at no cost. The surge button breaks it by choice. You have less health. | Glaze regrows 12 s after it is spent and never runs out. L2 with Glaze up bursts it at once (Splinter Ward's burst, no perfect guard), free. Health -25%. |
| ice.stillworld | The World Holds Still | Signature | none | Crown | With Glaze standing, press the surge button again to break it outward. Everything it crosses is Frozen where it stands. Shatter one and its ice shatters the next. | BESPOKE. Pressed: L2 while Glaze stands; costs the Glaze and 40 mana. Seen: the shell breaks into a white ring that crosses 10 m in 0.3 s; behind the ring the picture loses every colour but ice blue, the rain stops in the air, and thrown attacks hang where they were and then fall as ice. Every enemy the ring crosses is Frozen whatever its Chill: 4 s (elites 2.5 s, gods 1.5 s, with no lock-out after). While it lasts, each shatter sends its shards to the nearest Frozen enemy within 6 m and shatters it 0.25 s later at 60%, and so on down the line. Colour returns as the last one thaws or breaks. With Second Skin, the keystone's free burst is replaced by this, still at 40 mana. |

Counts: 1 root, 14 minors, 8 majors, 4 spells, 3 keystones, 1 signature: 31. Two BESPOKE (Gathering Frost, The
World Holds Still). One Katana mark.

---

## Water

**Idea.** Water does not stop: guarding runs into striking and striking back into guarding, and the player who
keeps that rhythm is stronger in each half of it. Its flasks are more than healing: they are thrown, poured on the
blade and spilled across the floor.

**Base spell: Riptide** (tap, 20 mana). A low wave 5 m ahead that comes back: 8 damage, Soaked, and every enemy it
touches is pulled 2 m toward the player. Where Wind's Gust sends them away, Riptide brings them into reach.

**Surge: Wellspring** (L2, 30 mana, 12 s). The user's "healing items heal more": flasks heal half again as much.
Made worth pressing: while it lasts a flask is drunk in an instant and without stopping, even in the middle of a
chain, and it cannot be wasted by a blow. Healing stops being a retreat.

**Affliction: Soaked.** Not counted in stacks: an enemy is Soaked for 8 s or it is not. A Soaked enemy takes 25%
more from spells, and when Static or Chill is laid on it, one stack of the same is laid on every other Soaked enemy
within 4 m. For a player who holds only Water, the tree gives Soaked its own teeth (Undertow, Heavy Water).

**Boughs.**

- **Current**: the rhythm of guard and blow. Ebb, Flood, and a chain that does not end.
- **Tide**: pulling and placing. Whirlpools, rain, and enemies brought to the blade.
- **Vessel**: the flask. Thrown, shared with the ground, poured on the weapon.

| id | Name | Kind | Grows from | Bough | Description | Design |
|---|---|---|---|---|---|---|
| water.soak | Soaked | Root | none | Root | Blows leave an enemy Soaked. Spells strike it harder, and Static or Chill laid on it spreads to others who are Soaked. | Trigger: any weapon blow lands. Soaked for 8 s. Values as above. |
| water.ebbflood | Ebb and Flood | Major | water.soak | Current | A perfect guard brings the Flood: for a while your blows reach further and splash. A heavy blow that lands brings the Ebb: for a while your guard costs little and its moment is longer. | Two timed boons, one at a time, shown as a ring at the feet (running ripples for Flood, a flat still circle for Ebb). Flood, 5 s: weapon reach +25%; each blow splashes 20% of its damage in 1.5 m and soaks. Ebb, 4 s: guard stamina cost halved, perfect guard window +0.04 s. Each sets up the other. |
| water.easy | Easy Water | Minor | water.ebbflood | Current | Your blows cost less stamina. | Attack stamina -10%. |
| water.wading | Wading | Minor | water.ebbflood | Current | You move faster with your guard up. | 45% to 60% of walking speed. |
| water.flowing | Flowing Guard | Major | water.ebbflood | Current | After a perfect guard, your next light blow is the last of its chain. | Trigger: perfect guard. For 2 s the next light press gives the chain's final blow at once (the Katana's stepping finisher, the fists' kick). |
| water.greytide | Grey Tide | Minor | water.flowing | Current | Blows win back grey health faster. | 0.6 to 0.8 a point of damage dealt. |
| water.longflood | Long Flood | Minor | water.flowing | Current | The Flood and the Ebb last longer. | Each +2 s. |
| water.unbroken | Unbroken Chain | Major | water.flowing | Current | A dodge or a guard no longer ends a chain. The next blow carries on from where it left off. | BESPOKE, small: the chain's place is remembered for 1.5 s through a dodge, a guard or a perfect guard. |
| water.moon | Moon on Water | Spell | water.unbroken | Current | Hold the spell button to stand open and turn every blow that comes, paying mana for each. Let go to give back all you turned in one cut. 10 mana to begin. | Elemental parry with a store. Held up to 3 s; a still pool 3 m across lies at the player's feet. Every blow that arrives, red or not, is turned for 10 mana and adds a ring to the pool, five at most. Release: one cut in a 4 m, 200 degree arc for 14 damage and 22 stagger a ring, and Soaked. No rings: a small splash. Turned blows count as plain guards, so they give no mana back. If the mana runs out the stance breaks and the player flinches for 0.6 s. |
| water.river | River Without End | Keystone | water.moon | Current | The light chain never ends: its last blow runs into its first, quicker each time round. Each blow strikes a little softer. | BESPOKE. After the chain's final light blow, another light press starts the chain again with no recovery between. Each time round: +8% attack speed, three times at most; lost when the chain is dropped. All weapon damage -12%. A heavy blow still ends the chain. |
| water.undertow | Undertow | Major | water.soak | Tide | A heavy blow drags every Soaked enemy near a step toward you. | Trigger: a heavy blow lands. Every Soaked enemy within 6 m is pulled 1.5 m. |
| water.longpull | Long Riptide | Minor | water.undertow | Tide | Riptide reaches further. | 5 m to 6.5 m. |
| water.lowwater | Low Water | Minor | water.undertow | Tide | Riptide costs less mana. | 20 to 15. |
| water.serpent | River Serpent | Spell | water.undertow | Tide | Hold the spell button to send a serpent of water out and back. It brings the first enemy it takes to your feet, Soaked and open. 30 mana. | A dragon head that returns. 8 m out along a straight lane and back. The first enemy it meets takes 12 damage and is carried to 1.5 m in front of the player, with its guard down for 1 s. Elites are pulled 3 m; a god is pulled 1 m and flinches. This is the answer to the Storm Caller. |
| water.heavy | Heavy Water | Major | water.undertow | Tide | Soaked enemies are slow to recover after they strike. | The recovery after a Soaked enemy's attack lasts 25% longer: a longer opening. |
| water.soakedthrough | Soaked Through | Minor | water.heavy | Tide | Soaked lasts longer. | 8 s to 12 s. |
| water.weight | Weight of Water | Minor | water.heavy | Tide | Spells strike Soaked enemies harder still. | 25% to 35%. |
| water.whirlpool | Whirlpool | Spell | water.heavy | Tide | Hold the spell button to open a whirlpool where the enemy faced stands. It drags everything near to its middle and holds them Soaked. 35 mana. | A field that lasts. Reach 6 m, radius 4 m, 5 s: enemies in it are drawn to the middle at 1.2 m a second, Soaked, and take 3 damage twice a second. The player fights in the middle of it. |
| water.rain | Standing Rain | Spell | water.whirlpool | Tide | Hold the spell button to call rain over the ground you stand on. All in it stay Soaked, and your blows there give more mana. 40 mana. | Rain over an area. 7 m radius where it was cast, 10 s. Enemies in it are Soaked without pause. The player's blows on enemies in it give +30% mana. |
| water.spring | Spring Tide | Keystone | water.rain | Tide | Every spell spends all the mana you hold, and is as great as what it spends. | Any spell, tapped or held, needs its usual cost and takes everything. Damage scales with mana spent over cost, capped at two and a half times; reach and size scale with its square root. A 25 mana spell cast with 100 held deals two and a half times and reaches twice as far. One great spell, then back to the blade to earn the next. |
| water.spilled | Spilled Water | Major | water.soak | Vessel | Drinking a flask throws a ring of water that pushes back and soaks everyone near. | Trigger: a flask is drunk (at the moment it heals). Ring 3.5 m: a 2.5 m push, 15 stagger, Soaked. |
| water.clear | Clear Water | Minor | water.spilled | Vessel | Flasks mend more. | 45 to 54. |
| water.earned | Earned Water | Minor | water.spilled | Vessel | An empty gourd is won back with fewer blows. | 120 damage to 95. |
| water.gourd | Broken Gourd | Major | water.spilled | Vessel | Hold the flask button to throw it. Where it breaks it leaves a pool, and you mend while you stand in it. | A flask is thrown, up to 9 m: 18 damage, 20 stagger and Soaked in 2.6 m. The pool is 3 m across and lasts 6 s: the player heals 7 a second inside it (42 if all of it is used, against 45 drunk); enemies in it are Soaked. Healing that must be stood for. |
| water.stillpool | Still Pool | Minor | water.gourd | Vessel | The pool from a broken gourd lasts longer. | 6 s to 8 s. |
| water.catch | Catch the Rain | Major | water.gourd | Vessel | A perfect guard fills a little of an empty gourd. | Trigger: perfect guard with any flask empty. Counts as 35 damage toward earning one back. |
| water.second | Second Gourd | Minor | water.catch | Vessel | You carry one more flask. | Three to four. |
| water.longdraught | Long Draught | Minor | water.catch | Vessel | Wellspring lasts longer. | 12 s to 16 s. |
| water.poured | Poured Blade | Spell | water.catch | Vessel | Hold the spell button to pour a flask along your weapon. For a while it reaches further, every blow mends you a little, and the last blow of a chain throws a wave. One flask and 20 mana. | A timed boon paid with a flask. 12 s: weapon reach +40%, each blow heals 2 and soaks, and the final blow of a chain throws a wave 5 m for 12 damage. About the same healing as drinking, if the player lands twenty blows. |
| water.well | Deep Well | Keystone | water.poured | Vessel | You carry one more flask. A flask no longer mends you at once: it fills a well that mends you only while you are landing blows. | Flasks +1. Each flask drunk adds 70 to a well shown beside the health bar (140 at most). The well pays out 6 health a second while a blow has landed in the last 3 s. More healing, and none of it for running away. |
| water.sea | The Sea Rises | Signature | none | Crown | While Wellspring lasts, hold the surge button to flood the ground. Your blows run out across the water as waves. When the time is done, the sea draws back to you and breaks where you face. One flask. | BESPOKE. Pressed: hold L2 0.5 s while Wellspring lasts; costs one flask. Seen: a dark mirror of water with a hard white edge spreads over the floor to 14 m in half a second; the fighters stand in their own reflections. For 8 s: every enemy on it is Soaked; every weapon blow sends a white wave line 6 m along the water in the direction of the swing, hitting each enemy once for 40% of the blow; the dodge slides half again as far. At the end, or on L2 again: the water runs back to the player's feet over 0.8 s, pulling enemies 3 m with it; the player holds the weapon low for 0.5 s (cannot be made to flinch, can be hurt); then one wave breaks forward, 9 m long and 120 degrees wide: 55 damage, 90 stagger, and ordinary enemies are thrown down. |

Counts: 1 root, 13 minors, 8 majors, 5 spells, 3 keystones, 1 signature: 31. Three BESPOKE (Unbroken Chain, River
Without End, The Sea Rises). No weapon marks: every Water mark works with any weapon.

---

## Life

**Idea.** Life spends its own health as if it were mana and wins it back with the weapon, so the bar is always
moving and a low bar is a kind of strength. And it grows things: thorns, saplings, a tree. A Life player's fight
has a place, their garden, and the enemy is made to fight in it.

**Base spell: Briar Patch** (tap, 20 mana). A bed of thorns 2.5 m across springs up 3 m ahead and stands 6 s:
enemies in it take 4 damage twice a second, are Seeded, and move a fifth slower. It counts as something grown.

**Surge: Regrowth** (L2, 40 mana, 6 s). The user's "heal". Made worth pressing: at the moment it is pressed all
grey health becomes real health, and then it heals 4 a second, doubled for a second after each blow the player
lands. It is strongest pressed just after guarding something heavy, and it does not pay for hiding.

**Affliction: Seeded.** 8 s, topped up by any blow. Each blow on a Seeded enemy heals the player (2 for a light
blow, 4 for a heavy one). When a Seeded enemy dies it bursts into thorns: 14 damage in 3 m, and everything the
thorns reach is Seeded.

**Boughs.**

- **Offering**: health as the price. Pay in blood, fight thin, win it back.
- **Garden**: things that grow where you fight. Thornbushes, saplings, the tree, and the harvest.
- **Bramble**: the seed and the thorn. Enemies that hurt you are hurt, held and made to share the wound.

| id | Name | Kind | Grows from | Bough | Description | Design |
|---|---|---|---|---|---|---|
| life.seed | Seeded | Root | none | Root | Blows plant a seed in an enemy. Each blow on a Seeded enemy mends you a little. When it dies, it bursts into thorns. | Trigger: any weapon blow lands. Values as above. |
| life.price | Paid in Blood | Major | life.seed | Offering | A spell you cannot pay for in mana is paid for in health. | For each 2 mana short, 1 health. It cannot take the last point of health. The weapon skill is included. |
| life.deeproots | Deep Roots | Minor | life.price | Offering | You have more health. | +12%. |
| life.sweetsap | Sweet Sap | Minor | life.price | Offering | Blows on Seeded enemies mend more. | 2 and 4 to 3 and 6. |
| life.thin | Thin Blood | Major | life.price | Offering | Below a third of your health, your blows strike harder and mend twice as much. | Trigger: health below a third. Weapon damage +25%; healing from Seeded doubled. |
| life.stubborn | Stubborn | Minor | life.thin | Offering | Below a third of your health, wounds take less. | Damage taken -15% below a third. |
| life.quickening | Quickening | Minor | life.thin | Offering | Regrowth costs less mana. | 40 to 32. |
| life.harvest | Red Harvest | Spell | life.thin | Offering | Hold the spell button to give a quarter of your health to the ground. For a while every blow is critical and mends you. Win back more than you gave and the ground bursts into thorns. No mana. | A timed boon paid in health. The player kneels (0.5 s) and a red and green ring 5 m across follows them for 8 s. Cost: a quarter of present health. Every weapon blow is critical and heals 20% of its damage. At the end, if the healing from it passed the price: thorns in 5 m, 30 damage, 50 stagger, Seeded. If not, nothing. |
| life.heartwood | Heartwood | Keystone | life.harvest | Offering | You have no mana. Spells and skills are paid for in health, and everything that gave mana mends you instead. | The mana bar is gone. Every mana cost is paid as 1 health for 2 mana and cannot take the last point. Every mana gain becomes 1 health for 3 mana: a perfect guard heals 5, a finisher 5, a blow about 1. |
| life.fallen | Fallen Seeds | Major | life.seed | Garden | Where a Seeded enemy dies, a thornbush grows and stands for a while. | Trigger: a blow kills an afflicted enemy. A 2 m bed of thorns for 8 s: 4 damage twice a second and Seeded. Counts as something grown. |
| life.season | Long Season | Minor | life.fallen | Garden | What you grow stands longer. | Briar Patch, thornbushes, saplings and the tree last 40% longer. |
| life.sharp | Sharp Thorns | Minor | life.fallen | Garden | What you grow strikes harder. | Their damage +25%. |
| life.sapling | Sapling | Major | life.fallen | Garden | A perfect guard plants a sapling at your feet, up to three. Each lashes the nearest enemy and seeds it. | Trigger: perfect guard. A planted totem for 12 s: every 1.5 s it whips the nearest enemy within 3.5 m for 6 damage, 8 stagger and Seeded. A fourth replaces the oldest. |
| life.crowded | Crowded Bed | Minor | life.sapling | Garden | You may keep one more sapling. | Three to four. |
| life.spreading | Spreading Briar | Minor | life.sapling | Garden | Briar Patch covers more ground. | 2.5 m to 3.3 m across. |
| life.soil | Ready Soil | Minor | life.sapling | Garden | Briar Patch costs less mana. | 20 to 15. |
| life.reap | Reap What Is Sown | Spell | life.sapling | Garden | Hold the spell button to call in the harvest. Everything you have grown bursts at once, every seed in every enemy with it, and each burst mends you. 30 mana. | Sets off everything. Each grown thing: a 3 m burst for 16 damage and 25 stagger, and it is gone. Each Seeded enemy: its thorn burst now, and its seed is spent. The player heals 5 for each burst. |
| life.shade | Shade | Major | life.sapling | Garden | Standing near anything you have grown, you mend slowly and your stamina returns faster. | Within 4 m of a grown thing, in a fight: 2 health a second, stamina returns 25% faster. |
| life.tree | Elder Tree | Spell | life.shade | Garden | Hold the spell button to plant a tree. It takes a few moments to grow and can be cut down while it does. Grown, it seeds all beneath it, mends you in its shade and strikes the staggered with its roots. 45 mana. | A planted totem with one addition: it grows. Three shapes over 3 s (shoot, sapling, tree), with 40 health until grown, and enemies near it turn on it. Grown, for 15 s: a ring 6 m across is drawn under its crown; enemies inside are Seeded without pause; the player inside heals 3 a second; every 3 s a root comes up under one enemy inside, a staggered one first, for 16 damage and 30 stagger. One tree at a time. |
| life.wildwood | Wildwood | Keystone | life.tree | Garden | What you grow does not wither while the fight lasts, and you may keep twice as many. Far from your garden, your blows are weaker. | Grown things have no clock in a fight (the tree still takes 3 s to grow). Limits doubled: six saplings, eight thornbushes, two trees. More than 7 m from every grown thing: weapon damage -20%. |
| life.ripen | Ripening | Major | life.seed | Bramble | Each blow ripens the seed in an enemy, up to five. A heavy blow on a ripe one bursts it without waiting for death. | Seeded gains depth: 1 a blow, five at most, shown as buds on the enemy. The thorn burst deals 30% more a stack. Heavy blow at five: the burst now (35 damage in 3 m), the player heals 10, the seed is spent. |
| life.deepseed | Deep Seed | Minor | life.ripen | Bramble | Seeded lasts longer. | 8 s to 12 s. |
| life.wideburst | Wide Burst | Minor | life.ripen | Bramble | Thorns burst wider. | 3 m to 4 m. |
| life.coat | Briar Coat | Major | life.ripen | Bramble | An enemy that strikes you by hand is cut by thorns and Seeded. | Trigger: the player is struck, or guards, a blow dealt by hand. The attacker takes 8 damage and is Seeded. |
| life.bark | Bark | Minor | life.coat | Bramble | Wounds take less while Regrowth lasts. | Damage taken -15% during the surge. |
| life.longgrowth | Long Regrowth | Minor | life.coat | Bramble | Regrowth lasts longer. | 6 s to 9 s. |
| life.greenwood | Green Wood | Minor | life.coat | Bramble | Grey health waits longer before it fades. | 2.5 s to 4 s. |
| life.rind | Bitter Rind | Major | life.coat | Bramble | A plain guard seeds the attacker. A perfect guard roots it where it stands for a moment. | Plain guard: Seeded. Perfect guard: held still for 1.0 s (elites 0.7 s, gods 0.4 s), on top of the stagger a perfect guard throws. |
| life.ring | Thorn Ring | Spell | life.rind | Bramble | Hold the spell button to raise a ring of thorns round you and the enemy faced. None leave and none enter until it falls. 40 mana. | A wall, bent into a ring. Reach 5 m. The ring is 9 m across, centred between the two, and stands 7 s. Anything touching it from either side takes 8 damage, is Seeded and is thrown back the way it came; thrown attacks from outside stop at it. The player cannot pass it either. A duel, made on demand. |
| life.roots | Shared Roots | Keystone | life.ring | Bramble | A third of every wound you take is carried by the Seeded enemies near you instead. Blows on Seeded enemies no longer mend you. | A thin root is drawn along the ground from the player to each Seeded enemy within 8 m. A third of each blow's damage is taken off the player and split among them. The healing from Seeded (and Sweet Sap, Thin Blood's doubling) is gone; other healing stays. |
| life.spring | First Spring | Signature | none | Crown | While Regrowth lasts, hold the surge button and give half your health to the ground. A forest comes up through everything Seeded and holds it. Strike what it holds and the health returns. | BESPOKE. Pressed: hold L2 0.8 s while Regrowth lasts; the player kneels with a hand on the ground and a blow taken breaks the hold with nothing spent. Costs half of present health. Seen: a green ring runs out to 9 m and the stone behind it floods with grass and pale flowers; a great root comes up through every Seeded enemy and lifts and holds it (4 s; elites 2.5 s, gods 1.2 s), dealing 20 plus 40% of the health given; every sapling and thornbush on the field becomes a grown Elder Tree for 10 s. For those 10 s each blow on a held or Seeded enemy returns 8% of the health given. Then the flowers fall as petals and the stone is stone again. |

Counts: 1 root, 14 minors, 8 majors, 4 spells, 3 keystones, 1 signature: 31. One BESPOKE (First Spring) and one
addition to a form (Elder Tree grows and can be cut down). No weapon marks.

---

# The mixed branches

Each grows in the gap between two trees and is open to a player who holds one of the two elements and has consumed
or held the other in this run. Every branch starts the same way: its first mark makes the weapon lay **both**
afflictions, each by its own root's rules, because a player holding one element has no other way to lay the second.
Every mark is written to work whichever of the two elements is held: none leans on a mark from either parent tree,
and "your surge" means whichever surge the player has. The **Crown** is never sold: it is given when the neighbour's
god is consumed, as Plasma is today.

## Plasma (Fire + Lightning)

**Idea.** Burn and Static on one enemy are unstable together: each sets the other off, and the bursts travel. A
Plasma player loads one enemy with both and lets the whole room go up from it.

**The reason to want it:** Ball Lightning. **The crown:** Plasma, the mark that already exists.

| id | Name | Kind | Cost | Grows from | Description | Design |
|---|---|---|---|---|---|---|
| plasma.embers | Charged Embers | Major | 3 | fire.cinder or lightning.static | Your blows lay Burn and Static together. | The affliction of the element not held is laid by weapon blows under its own root's rules (Burn 1 a blow; Static 1 a light blow, discharged by a heavy blow or a finisher). |
| plasma.arcfire | Arc Fire | Major | 3 | plasma.embers | A discharge of Static carries Burn to everyone it reaches. A burn set off leaves Static on everyone it reaches. | Discharge arcs lay Burn equal to half the stacks discharged, rounded up. Any burst that spends Burn lays 2 Static on all it hits. |
| plasma.twin | Twin Break | Major | 4 | plasma.embers | A perfect guard sets off Burn and Static together on the one who struck. Set off together, they strike half again as hard and stop it where it stands. | Trigger: perfect guard on an enemy carrying both. Both are spent: (10 a Burn stack plus 7 a Static stack) times 1.5, stagger 5 a stack, and a 0.6 s flinch that breaks an attack already shown (not a god's). |
| plasma.ball | Ball Lightning | Spell | 4 | plasma.arcfire | Hold the spell button to loose a slow ball of white fire. Strike it to send it where the blow points, larger each time, up to three. It bursts on the first enemy it meets. 40 mana. | BESPOKE in one part: a slow tracking orb that the player's own blows redirect. It appears 1.5 m ahead and drifts at 2 m a second toward the nearest enemy for 8 s. Any weapon blow on it sends it at 9 m a second along the swing and grows it a step, three at most, with a ring round it for each. Burst: 16 damage plus 12 a strike, radius 2.5 m plus 0.7 m a strike, stagger 20 plus 10 a strike, 2 Burn and 2 Static plus 1 of each a strike. Left alone it bursts small. |
| plasma.chain | Chain Reaction | Major | 4 | plasma.arcfire | An enemy that dies carrying both Burn and Static bursts, and passes both on at full depth. | Trigger: a blow kills an enemy with both. 18 damage in 4 m; everything reached takes that enemy's Burn and Static stacks. |
| plasma.meltdown | Meltdown | Major | 5 | plasma.chain | An enemy at five Burn and five Static begins to shake with light. A few moments later it bursts, far harder than either alone. Nothing stops it. | Trigger: both afflictions at full depth. A white pulse on the enemy quickens for 3 s and a 5 m ring is drawn round it; then 45 damage and 60 stagger to it and half of each to everything else in the ring. Both are spent. The player is not harmed. An elite or god cannot melt down again for 10 s. |
| fire.plasma | Plasma | Crown | 0 | none | A heavy blow or a finisher on a burning enemy bursts, and sets alight everyone near. | As built: 18 damage and 16 stagger in 3.2 m, Burn on all reached. Keeps its id. Moved here from Fire's crown. With Charged Embers held, the burst also lays 2 Static. It is the same gift whichever way round the two elements are held. |

Counts: 7 marks: 5 majors, 1 spell, the crown.

## Tempest (Lightning + Wind)

**Idea.** The storm proper: Static in an enemy that has lost its footing. Discharges throw enemies into the sky and
the sky answers. A Tempest player fights underneath their own weather.

**The reason to want it:** Eye of the Storm. **The crown:** Falling Thunder.

| id | Name | Kind | Cost | Grows from | Description | Design |
|---|---|---|---|---|---|---|
| tempest.charged | Charged Air | Major | 3 | lightning.static or wind.unsteady | Your blows lay Static and Unsteady together. | As the first mark of every mix. |
| tempest.lift | Thunder Lift | Major | 4 | tempest.charged | A discharge of five Static throws its enemy into the air. | Uses "in the air" as written under Wind (1.0 s, with its limits for elites, gods and colossal forms). For a Lightning player this is the launcher. |
| tempest.sky | The Sky Finds Them | Major | 3 | tempest.lift | An enemy in the air is struck by a bolt at the top of its rise. | Once a lift: 14 damage, 2 Static, arcing to two more within 4 m. |
| tempest.slip | Slip Current | Major | 3 | tempest.charged | Each enemy you pass in a dodge is left one deeper in Static and Unsteady, and pays half the stamina back. | Trigger: a dodge ends, for each enemy its path crossed. |
| tempest.cloud | Storm Overhead | Spell | 4 | tempest.slip | Hold the spell button to call a low cloud that follows you. Each dodge beneath it calls a bolt down where you stood. A perfect dodge calls three. 40 mana. | Rain over an area that follows the player: a dark circle 4 m across on the ground for 10 s. 0.3 s after a dodge begins, a bolt lands on its starting point: 14 damage, 20 stagger, 1 Static in 2 m. Perfect dodge: three bolts 0.15 s apart, on the starting point, on the attacker and on the nearest other enemy. |
| tempest.eye | Eye of the Storm | Spell | 5 | tempest.lift | Hold the spell button to become the storm: a turning column of wind and lightning that drags enemies in and carries the light ones round. Let go and it throws them out, and a bolt meets each where it lands. 50 mana. | BESPOKE. Pressed: hold Triangle, up to 2.5 s; the stick walks the column at 70% speed. Seen: the player is the dark centre of a turning ring 7 m across, pale green with blue arcs. Enemies within it are drawn in at 2 m a second; every 0.4 s each takes 6 damage, 1 Static and 1 Unsteady; ordinary enemies are lifted and carried round the player and cannot act. The player cannot be made to flinch and takes half damage. On release: everything is thrown 5 m outward, and a bolt lands on each as it stops for 12. |
| tempest.falling | Falling Thunder | Crown | 0 | none | Your finisher changes. You throw the enemy skyward, meet it there, and ride it down as a bolt. Where you land, everything near is shocked and thrown back. | BESPOKE. Pressed: heavy on a staggered enemy, as now. Seen: a rising cut throws the enemy up (0.25 s); the player leaves the ground as a line of light; for 0.4 s both are out of the top of the frame while a ring closes on the floor where they will land; then one bolt, and the player is kneeling on the enemy in a scorched ring. The finisher's own damage is unchanged (three times). Added: a 4.5 m burst, 22 damage, 40 stagger, 3 Static, 3 Unsteady and a 3 m push. Untouchable throughout, as the finisher is. Against a colossus the bolt lands on the fallen head. |

Counts: 7 marks: 4 majors, 2 spells, the crown.

## Blizzard (Wind + Ice)

**Idea.** Cold that moves. An enemy that is both chilled and Unsteady freezes without help, and a Frozen enemy is
something to be thrown. A Blizzard player turns frozen enemies into the weapon.

**The reason to want it:** Sliding Ice. **The crown:** Diamond Dust.

| id | Name | Kind | Cost | Grows from | Description | Design |
|---|---|---|---|---|---|---|
| blizzard.biting | Biting Wind | Major | 3 | wind.unsteady or ice.chill | Your blows lay Unsteady and Chill together. | As the first mark of every mix. |
| blizzard.windchill | Windchill | Major | 3 | blizzard.biting | An enemy both Unsteady and chilled grows colder by itself. | While it carries both: 1 Chill every 2 s. |
| blizzard.front | Cold Front | Major | 3 | blizzard.biting | A perfect dodge leaves a gust of ice behind you. It throws the attacker back, two deeper in Chill and Unsteady. | Trigger: perfect dodge. A 4 m, 90 degree cone from where the player stood toward the attacker: 10 damage, a 3 m push, 2 of each. |
| blizzard.sliding | Sliding Ice | Major | 4 | blizzard.windchill | A light blow sends a Frozen enemy sliding until it meets something. It shatters there, and what it met is hurt as much. | A light blow on a Frozen enemy pushes it along the swing at 9 m a second for up to 10 m; it stays Frozen while it slides. Meeting a wall or another enemy: it shatters for twice a heavy blow's damage, the thing it met takes the same, and the shards fly as usual. A heavy blow still shatters it where it stands. Small ask: the push collision that Wind's Against the Wall also needs. Elites slide 4 m; gods do not slide. |
| blizzard.whiteout | Whiteout | Spell | 4 | blizzard.windchill | Hold the spell button to call down driving snow around you. Enemies in it lose sight of you beyond arm's reach and turn slowly. A blow from behind is critical there. 40 mana. | Rain over an area. 7 m radius where it was cast, 8 s. Enemies inside: they notice the player only within 3 m, thrown attacks cannot be aimed, turning is 40% slower, 1 Chill every 2 s. The player's blows on an enemy's back inside it are critical. Gods keep their sight and take only the slow turn and the Chill. |
| blizzard.avalanche | Avalanche | Spell | 5 | blizzard.sliding | Hold the spell button to send a wall of snow rolling out before you. It carries enemies along and buries them where it stops, Frozen. 50 mana. | A wave, slow and wide: 4 m wide, 9 m at 4.5 m a second, so the player can run beside it and strike what it carries. Ordinary enemies are carried to its end and Frozen there; if it meets a wall they take 25 as well. Elites are pushed 3 m and take 4 Chill; gods take 3 Chill and 30 stagger. |
| blizzard.dust | Diamond Dust | Crown | 0 | none | Shards from a shattered enemy do not fall. They hang round you, up to twelve, and your next dodge throws them down its path. | BESPOKE. Pressed: the dodge. Seen: each shatter leaves four bright points of ice turning slowly round the player, twelve at most, for 10 s. The next dodge carries them along it and 4 m beyond its end in a lane 2 m wide: each shard 6 damage, and every third shard to hit one enemy lays 1 Chill and 1 Unsteady. A shatter becomes the opening of the next attack. |

Counts: 7 marks: 4 majors, 2 spells, the crown.

## Glacier (Ice + Water)

**Idea.** Water that has stopped. A Soaked enemy freezes sooner, guarding builds ice on the player's own body, and
the great spell is a wave caught in the act of breaking. Slow, heavy, and sure.

**The reason to want it:** The Wave Stands Still. **The crown:** Weight of Ice.

| id | Name | Kind | Cost | Grows from | Description | Design |
|---|---|---|---|---|---|---|
| glacier.coldwater | Cold Water | Major | 3 | ice.chill or water.soak | Your blows lay Chill and Soaked together. | As the first mark of every mix. Soaked already spreads Chill to other Soaked enemies. |
| glacier.flash | Flash Freeze | Major | 3 | glacier.coldwater | A Soaked enemy freezes at three Chill, not five. | The freeze spends Soaked with the Chill. |
| glacier.breaker | Icebreaker | Major | 3 | glacier.flash | Shattering an enemy gives mana and fills a little of an empty gourd. | 12 mana, and 40 damage counted toward earning a flask back. |
| glacier.bones | Cold in the Bones | Major | 4 | glacier.flash | An enemy both Soaked and chilled is slow to wind up its blows. The moment to guard them is easier to find. | The wind-up of its attacks lasts 25% longer and the mark on the ground fills at that pace. On top of Chill's own slowing. |
| glacier.creeping | Creeping Ice | Spell | 4 | glacier.coldwater | Hold the spell button to send a line of ice along the ground. Those it reaches are held to the floor by their feet: they can strike but cannot turn or step. 35 mana. | A spiked floor, in a line: 8 m long, 1.2 m wide, crossing in 0.4 s. 8 damage, 2 Chill, Soaked, and held by the feet for 2 s (elites 1.2 s, gods 0.8 s): no walking and no turning, so the way round to its back is open. |
| glacier.wave | The Wave Stands Still | Spell | 5 | glacier.bones | Hold the spell button to throw a great wave. Let go, and it freezes where it stands with everything in it. One heavy blow shatters the wall and all it holds. 50 mana. | BESPOKE. Pressed: hold Triangle; release when wanted. Seen: a wave 6 m wide rolls out from the player at 6 m a second for as long as the button is held, up to 9 m, with a white crest; it pushes ordinary enemies along and soaks them for 10 damage. On release, or at 9 m, it turns to ice in one frame, crest curled over, a wall of pale blue with dark shapes caught in it. Everything it holds or touches is Frozen (elites and gods take 3 Chill, which freezes them only if they were Soaked and Flash Freeze is held). It stands 4 s. A heavy blow on the wall or on anyone in it shatters all of it at once, each enemy as a shatter. Left alone it melts and leaves them Soaked. |
| glacier.weight | Weight of Ice | Crown | 0 | none | Each blow you guard lays a plate of ice on you, up to five. A perfect guard lays two. Plates turn a little of every wound. Your next heavy blow spends them all in one great edge of ice. | BESPOKE. Pressed: guard, then heavy. Seen: plates of ice build on the guarding arm and shoulder, counted as five shards by the health bar; on the heavy blow they run down the weapon and it swings as a blade of ice far longer than itself. Each plate: damage taken -5%. Heavy blow with plates: reach +15% a plate, damage +18% a plate, 1 Chill a plate; with all five, a 3 m burst of shards for 14 as well. Plates fall away after 12 s without a guard. |

Counts: 7 marks: 4 majors, 2 spells, the crown.

## Bloom (Water + Life)

**Idea.** Watered seeds flower. The healing of both parents is turned outward: mending past full becomes a weapon,
and the ground the player stands on becomes a place enemies should not have followed them into.

**The reason to want it:** Morning Dew. **The crown:** Great Lotus.

| id | Name | Kind | Cost | Grows from | Description | Design |
|---|---|---|---|---|---|---|
| bloom.wetseed | Wet Seed | Major | 3 | water.soak or life.seed | Your blows lay Soaked and Seeded together. | As the first mark of every mix. |
| bloom.flower | Flowering | Major | 3 | bloom.wetseed | The seed in a Soaked enemy flowers after a few moments. A heavy blow on the flower bursts it: you are mended and everyone near is Seeded and Soaked. Left alone, it withers. | An enemy that has carried both for 4 s shows a pale flower for 3 s. Heavy blow: the player heals 12; burst of 12 damage in 3.5 m laying both. After it bursts or withers the 4 s begins again. |
| bloom.roots | Drowned Roots | Major | 3 | bloom.wetseed | A perfect guard against an enemy both Soaked and Seeded binds it in roots for a moment. | Held still 1.2 s (elites 0.8 s, gods 0.5 s). |
| bloom.dew | Morning Dew | Major | 4 | bloom.flower | Mending beyond full health is kept as Dew. While you hold Dew, each blow throws a blade of water with it. Dew dries slowly, and a wound takes it first. | Healing past full fills a pale second bar over the health bar, up to 30% of health. It falls by 1 a second and takes damage before health does. With any Dew: each weapon blow throws a blade 3 m ahead for 25% of the blow, Soaked. Every source of healing in both trees now feeds the offence. |
| bloom.lotus | Lotus Step | Spell | 4 | bloom.roots | Hold the spell button, and for a while each dodge leaves a lotus where it ends, up to four. Step on one to be mended. An enemy that steps on one is cut, Soaked and Seeded. 30 mana. | A timed boon, 10 s. Each lotus opens 0.8 s after it is left and waits 12 s. The player on it: heals 8. An enemy on it: 10 damage, 15 stagger, both afflictions. A fifth replaces the oldest. |
| bloom.vine | Strangling Vine | Spell | 5 | bloom.flower | Hold the spell button to raise a great vine under the enemy faced. It lifts and holds it, and while it holds, each blow mends you three times over. Then it throws the enemy down. 45 mana. | A pillar that holds. Reach 5 m; it rises in 0.5 s on a drawn circle. The enemy is held for 3 s (elites 2 s, gods 1 s), Soaked and Seeded; healing from blows on it is tripled. At the end it is thrown down: 20 damage and 50 stagger in 3 m. |
| bloom.great | Great Lotus | Crown | 0 | none | A flask drunk while your surge lasts opens a great lotus under you. Inside it you mend and enemies are Soaked and Seeded. Each perfect guard inside closes a petal on the attacker and holds it, up to five. When the last petal closes, the lotus bursts. | BESPOKE. Pressed: the flask, while Wellspring or Regrowth lasts. Seen: five broad pale petals open flat on the ground round the player, 6 m across, where they drank; it stays 10 s. Inside: the player heals half a flask more, spread over its time; enemies are Soaked and Seeded without pause. A perfect guard inside: one petal folds up over the attacker and holds it 1.5 s (elites 1 s, gods 0.6 s). When five have closed, or the time ends, it bursts: 8 damage plus 10 for each petal closed, in 5 m. A reason to drink in the middle of the fight and then stand there. |

Counts: 7 marks: 4 majors, 2 spells, the crown.

---

# Checks made

- Every "Grows from" id is a row in this document; each element's tree has one root and every sold mark is reached
  from it; signatures and crowns hang from nothing because they are given, not bought.
- Own marks per element: Fire 31, Lightning 31, Wind 31, Ice 31, Water 31, Life 31. Mixes: 7 each.
- No name is used twice in this document. (`trees_b.md` was not seen; a clash between the two is possible.)
- BESPOKE per element: Fire 1, Lightning 1, Wind 4, Ice 2, Water 3, Life 1. In the mixes: Plasma 1, Tempest 2,
  Blizzard 1, Glacier 2, Bloom 1.
- Weapon marks: one each for Fire, Lightning, Wind and Ice, all for the Katana; none for Water or Life.
- Keystones cost 18 for all three, and a whole tree costs about 74 embers before any mix, against 19 from a castle
  and its god. The cheapest road to any one keystone (root, the bough's spine, the keystone) is 18 to 22, so one
  run reaches one keystone with almost nothing to spare, and never two.
- Descriptions: no exclamation marks, no em dashes, and no numbers except counts the player sees and a spell's mana
  cost. Minors are one sentence. Spells, keystones and signatures run to three or four short sentences where the
  move has that many parts, as Heat's text does today; they can be cut if the panel is small.

# The ten strongest moves

1. **The Wind Remembers** (Wind, signature): every cut made during Tailwind hangs in the air, and when the weapon
   is lowered they all fall again.
2. **The Wave Stands Still** (Glacier, spell): throw a wave, freeze it mid-break with everyone inside, and shatter
   the wall with one blow.
3. **Seven Flashes** (Lightning, spell): seven lines of light through the enemy, a still pose with the back turned,
   and then the cuts.
4. **Sky Dance** (Wind, spell): follow a launched enemy into the air, cut it in time five times, and throw it down
   where you point.
5. **The World Holds Still** (Ice, signature): the shell breaks outward, the rain stops in the air, everything is
   Frozen, and one shatter runs down the line.
6. **Falling Sun** (Fire, signature): raise a sun, keep it alive by not being hit, and bring it down.
7. **Moon on Water** (Water, spell): stand open, turn every blow that comes, and give all of it back in one cut.
8. **Ball Lightning** (Plasma, spell): a slow ball of white fire that you bat at the enemy with your own weapon.
9. **Falling Thunder** (Tempest, crown): the finisher becomes a throw into the sky and a ride down as a bolt.
10. **Sliding Ice** (Blizzard, major): a light blow sends a Frozen enemy across the floor to shatter on its friends.

Close behind: Thorn Ring (a duel made on demand), Hollow Statue, First Spring, Closing Circle, Weight of Ice.

# Open questions for the user

1. **Fire's root.** The brief asks that the root turn the affliction on, so this document adds Cinder as Fire's
   root and makes Heat the foot of Pressure, and Burn and Detonate loses its first sentence. The other way is to
   leave Heat as the root, as built, and keep Burn and Detonate whole. Which?
2. **Plasma's place.** Plasma is moved from Fire's crown to the Plasma branch's crown (it is given by consuming,
   which is what a mix crown is), and Fire gets Falling Sun for the player who takes Fire. Agreed?
3. **Overclock** is made the Thunder keystone with a second sentence and a higher Overdrive cost. If it should
   stay exactly as built, it becomes a Major and Thunder needs a new keystone.
4. **In the air.** Wind's Updraft bough and half of Tempest rest on lifting enemies. This document lets human-sized
   gods be lifted briefly and makes colossal ones reel. Is a god in the air acceptable at all, or should gods only
   ever reel?
5. **Critical blows** are named by Lightning, Life and Blizzard and do not exist yet. One and a half times damage is
   assumed. Light, in the other document, depends on them too.
6. **Health as a price.** Life pays for spells in health at 1 health for 2 mana, and Heartwood removes mana
   altogether. These rates need the player's real health figure, which this document did not have.
7. **The hold-flask button** is asked for by Firebomb Flask and Broken Gourd, and the hold-spell slot by every
   spell. One worn spell at a time is assumed; is a second slot wanted once a tree has four?
8. **A second press of the surge button** is used by Falling Sun, The World Holds Still, The Sea Rises and First
   Spring (press or hold L2 while the surge already lasts). Is that the right home for signatures?
9. **Holds on gods.** Freezes, roots and holds are given short values for gods (0.4 s to 1.5 s). Should gods be
   immune to all of them instead, and take stagger in their place?
10. **Soaked for a lone Water player** is weak by the brief's definition (it serves spells, Static and Chill), so
    Undertow and Heavy Water were added to give it weight in close combat. Is that the right reading of Water?
