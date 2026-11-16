# The great chart, part B: Earth, Metal, Light, Dark, Poison and six mixes

Written to `docs/design/TREE_BRIEF.md`. Everything here is a proposal for the user: nothing is decided, and every
number is a first value to be tuned by play. Part A (Fire, Lightning, Wind, Ice, Water, Life and the other five mixes)
is `trees_a.md`.

## How to read the tables

- **Costs go by kind**: Root 2, Minor 1, Major 3, Spell 4, Keystone 6, Signature never sold. Mix marks carry their own
  cost (3 to 5) at the start of the Design column.
- **Buttons**: tap Triangle casts the element's base spell; hold Triangle (0.3 s) casts the one learned spell that is
  worn; L2 is the surge. "The enemy you face" is the soft-aim target.
- **In-game words** are the exact text for `Upgrade.Description`. Spells end with their mana cost, as Meteor Leap and
  Lightning Rod do today. If the user would rather costs were shown by the tree and not in the sentence, strike them.
- **Design** is written in the brief's triggers, actions, numbers and spell forms. **BESPOKE** means new code beyond
  them, with what is pressed, seen and done. "Small extension" means an existing trigger or action with one more
  condition, and is not counted as BESPOKE.
- **Assumed** (none of it built yet): the player has 100 health; a critical blow deals 1.5 times damage; ordinary
  weapon blows are in the range the Katana has today (Flame Wave 20, Meteor Leap 46 for scale); "held still" on a god
  or an elite is always shortened as the row says.
- A bough is learned from its foot outward, as the tree works today. The Signature hangs from the root at the crown
  and is given, never bought.
- Three keystones cost 18 and their boughs far more, so no run finishes more than one bough and part of another.

## Counts

| Tree | Root | Minor | Major | Spell | Keystone | Signature | Total | BESPOKE |
|---|---|---|---|---|---|---|---|---|
| Earth | 1 | 14 | 7 | 5 | 3 | 1 | 31 | 4 |
| Metal | 1 | 14 | 7 | 4 | 3 | 1 | 30 | 4 |
| Light | 1 | 14 | 7 | 5 | 3 | 1 | 31 | 5 |
| Dark | 1 | 14 | 7 | 5 | 3 | 1 | 31 | 4 |
| Poison | 1 | 14 | 7 | 5 | 3 | 1 | 31 | 3 |
| Each mix (six) | | | 3 or 4 | 1 or 2 | 1 | 1 (the crown) | 7 | 1 or 2 |

---

# Earth

**The idea.** Earth does not get out of the way. Its player stops rolling, takes the blow on stone, raises the ground
into pillars and walls and then breaks them over the enemy, and lands few blows that each count.

**Affliction: Tremor.** Up to three deep, 6 s, refreshed by any weapon blow. Each depth slows the enemy's movement and
turning by 10%. At any depth its stagger meter does not drain. Shown as cracked ground that follows its feet, one
ring per depth.

**Base spell: Stone Rise** (tap, 25 mana). A pillar of rock bursts up under the enemy you face, up to 5 m off:
14 damage, 30 stagger, Tremor 1. The pillar then stands for 6 s (one at a time) and blocks movement and thrown
attacks. A heavy blow of yours on the pillar shatters it into a cone of shards away from you: 5 m, 70 degrees,
18 damage, 20 stagger. Raise it, fight round it, break it. (Spell form: pillar that blocks and bursts.)

**Surge: Bedrock** (L2, 40 mana, 10 s). Every affliction on you is shed and none takes hold. Blows that can be guarded
no longer make you flinch or push you, and you take 20% less from everything. It is the button for trading: press it
and swing through what would have stopped you. Stone climbs your legs and forearms while it lasts.

**Boughs.**

- **Standing**: hold your ground and be paid for it. The guard, plates of stone, never rolling.
- **Weight**: slow certain blows. Heavy blows that cannot be stopped, chains that grow, one enormous swing.
- **Ground**: walls you raise and break. Pillars as weapons, the crack in the floor, the falling wall, the duel ring.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| earth.tremor | Tremor | Root | (the middle) | Trunk | Heavy blows and finishers shake an enemy's footing, up to three deep. A shaken enemy is slow, and its balance does not come back. | Heavy blow lands: lay 1 Tremor. Finisher lands: lay 3. Tremor as defined above. |
| earth.rooted | Rooted | Major | earth.tremor | Standing | Fight without rolling for a few breaths and you are Rooted. Rooted, nothing pushes you back and your guard costs less. A roll ends it. | Every 3 s in a fight with no dodge begun: boon Rooted until the next dodge begins. Guard stamina cost down 30%, no push from flinches or guarded blows, damage taken down 8%. A ring of cracked stone under the feet shows it. |
| earth.deepfooting | Deep Footing | Minor | earth.rooted | Standing | You become Rooted sooner. | 3 s to 2 s. |
| earth.thickhide | Thick Hide | Minor | earth.rooted | Standing | Rooted, you take less from every blow. | Rooted's damage taken down goes from 8% to 16%. |
| earth.stoneskin | Stone Skin | Major | earth.rooted | Standing | A perfect guard grows a plate of stone on you, up to three. A plate takes most of one blow, and breaks. | Perfect guard: +1 plate, to 3 (slabs on the shoulders and back; three pips by the health bar). Player is struck with a plate up: that blow does 60% less and does not flinch; one plate breaks. Plates stay until broken. |
| earth.steadyarm | Steady Arm | Minor | earth.stoneskin | Standing | Holding your guard costs less stamina. | Guard cost 0.9 to 0.75 stamina a point. |
| earth.oldgranite | Old Granite | Minor | earth.stoneskin | Standing | Grey health lingers longer before it fades. | Grey health kept 2.5 s to 4 s. |
| earth.aftershock | Aftershock | Major | earth.stoneskin | Standing | A plate that breaks bursts outward and shakes everyone near. | Player is struck and a plate breaks: burst round the player, 3 m: 14 damage, 24 stagger, Tremor 1. |
| earth.deepbedrock | Deep Bedrock | Minor | earth.aftershock | Standing | Bedrock lasts longer. | Surge length 10 s to 13 s. |
| earth.stance | Mountain Stance | Spell | earth.aftershock | Standing | Hold the spell button to stand as stone. The first blow to land on you is turned, and a fist of rock rises under the one who struck. 35 mana. | Form: elemental parry. Up to 1.5 s, no moving. Any blow in that time, red ones too, is turned for no damage; 0.2 s later a pillar bursts under the attacker: 30 damage, 60 stagger, Tremor 2. If nothing strikes, 15 mana comes back. |
| earth.immovable | Immovable | Keystone | earth.stance | Standing | You no longer roll. The dodge button braces you where you stand, and a brace timed well turns any blow, the red ones too. You are always Rooted. | BESPOKE. Pressed: dodge. Seen: the player drops into a wide stance, a hexagon of stone snaps up round the feet for a third of a second. Does: no movement and no safe time; a blow in the first 0.2 s counts as a perfect guard (all triggers, 15 mana) whatever its colour; a blow in the rest of the 0.35 s is a plain guard at half stamina. Costs the roll's stamina. Rooted never ends. Drawback: ground hazards and lanes must be walked out of or braced through. |
| earth.heavyhand | Heavy Hand | Major | earth.tremor | Weight | Your heavy blows are not stopped by being struck. You take less from that blow, and yours still lands. | From a heavy blow's wind-up to its hit: no flinch, damage taken down 35%. |
| earth.breakingweight | Breaking Weight | Minor | earth.heavyhand | Weight | Heavy blows break balance sooner. | Stagger dealt by heavy blows up 15%. |
| earth.fullswing | Full Swing | Minor | earth.heavyhand | Weight | Heavy blows and finishers cut deeper. | Weapon damage of heavy blows and finishers up 10%. |
| earth.landslide | Landslide | Major | earth.heavyhand | Weight | Each blow in a chain lands harder than the one before. The heavy blow that ends a chain shakes the ground round its target. | Each blow after the first in a chain: +8% damage and stagger per earlier blow, to +24%. Heavy blow that ends a chain lands: burst round the target, 2.5 m: 10 damage, 15 stagger, Tremor 1 to all. |
| earth.loosefooting | Shaken Ground | Minor | earth.landslide | Weight | Shaken enemies take more from your weapon. | Damage to enemies with Tremor up 10%. |
| earth.longtremor | Long Tremor | Minor | earth.landslide | Weight | Tremor lasts longer. | Affliction length 6 s to 9 s. |
| earth.quakestep | Quake Step | Spell | earth.landslide | Weight | Hold the spell button to raise a foot and bring it down. The ground heaves all round you, and an enemy caught in the middle of its own attack loses its footing. 35 mana. | Form: nova round the caster. 0.45 s with the leg raised (a held beat), then a ring to 5 m: 16 damage, 30 stagger, Tremor 2. An enemy showing an attack takes double stagger and the attack is broken (a god's or elite's red attack takes the stagger only). |
| earth.easyburden | Easy Burden | Minor | earth.quakestep | Weight | Heavy blows cost less stamina. | Stamina cost of heavy blows down 15%. |
| earth.cleaver | Mountain Cleaver | Spell | earth.quakestep | Weight | Hold the spell button and stone gathers on your weapon, in three stages. Let go to bring it down. While you hold you can be hurt but not stopped. 45 mana. | BESPOKE. Pressed: hold Triangle, as long as 2 s; release to strike. Seen: the weapon is drawn back over the shoulder and rock climbs it into a blade three times its length; three rings on the ground ahead fill one by one (0.5, 1.2, 2.0 s) and the lane the blow will take is drawn, 3.5, 5 and 6.5 m long. Does: one overhead blow down the lane, as a heavy blow: 30 / 55 / 90 damage, 40 / 70 / 120 stagger, Tremor 1 / 2 / 3; the third stage leaves a line of spikes for 3 s. No flinch and 50% less damage while holding. A roll gives it up and the mana is lost. |
| earth.slowhand | The Slow Hand | Keystone | earth.cleaver | Weight | Every swing is slower and costs more stamina. Every swing lands as a heavy blow. | Attack speed down 22%, stamina cost of attacks up 20%. Light blows: +35% damage, +60% stagger, always flinch, pass an enemy's guard as a heavy does, lay Tremor 1, and count as heavy blows for every trigger. Heavy blows and the weapon skill are unchanged in power. |
| earth.standingstones | Standing Stones | Major | earth.tremor | Ground | Two pillars may stand at once. An enemy driven into a pillar breaks it and is hurt. | Stone Rise's pillar limit 1 to 2. An enemy pushed into any pillar of yours (a heavy blow's shove, a finisher, Quake Step): 20 damage, 35 stagger, Tremor 1; the pillar shatters in its cone, away from you. |
| earth.hardstone | Hard Stone | Minor | earth.standingstones | Ground | Pillars and their shards strike harder. | Spell damage up 15% (Earth's spells and shards). |
| earth.nearathand | Near to Hand | Minor | earth.standingstones | Ground | Stone Rise costs less mana. | 25 to 20. |
| earth.toppling | Toppling | Major | earth.standingstones | Ground | A perfect guard beside one of your pillars topples it onto the one who struck. | Perfect guard with a pillar within 4 m of the player: the pillar falls toward the attacker along a lane 4 m by 1.5 m, drawn for 0.3 s: 26 damage, 45 stagger, Tremor 2. |
| earth.farroots | Far Roots | Minor | earth.toppling | Ground | Stone rises further from you, and its shards fly further. | Spell reach up 25%. |
| earth.fissure | Fissure | Spell | earth.toppling | Ground | Hold the spell button to drive your weapon into the ground. A crack runs ahead, waits a breath, and breaks open in a row of spikes that hold what they catch. 40 mana. | Form: spiked floor, a line. The crack runs 8 m in 0.4 s and glows for 0.9 s, then erupts: 28 damage, 35 stagger, Tremor 1, and those caught are held still 1.2 s (a god or elite is not held and takes Tremor 2). The spikes stand 3 s as a wall and can be shattered like a pillar. |
| earth.oldstones | Old Stones | Minor | earth.fissure | Ground | Pillars, spikes and walls stand longer. | Their lifetimes up 50%. |
| earth.rampart | Rampart | Spell | earth.fissure | Ground | Hold the spell button to raise a wall before you. Strike it with a heavy blow and it falls forward on whatever stands behind it. 40 mana. | Form: wall that blocks and bursts. A curved wall 5 m wide rises 2.5 m ahead and stands 8 s; it stops movement and thrown attacks both ways. A heavy blow of yours on it: it falls forward over a 5 m by 4 m slab drawn for 0.35 s: 34 damage, 50 stagger, Tremor 2. Left alone it crumbles and does nothing. |
| earth.closedring | The Closed Ring | Keystone | earth.rampart | Ground | Bedrock raises a ring of stone round you and the enemy you face. Nothing enters and nothing leaves while it stands, you least of all. | BESPOKE. Pressed: the surge, as always. Seen: eight tall stones burst up in a circle 12 m across, set on the midpoint between you and the enemy faced; the floor inside darkens and the stones glow at their feet. Does: for the surge's length nothing walks, rolls or is thrown across the ring (enemies outside wait; their thrown attacks break on it). An enemy driven into a stone takes 20 damage and 35 stagger and that stone shatters as a pillar does (the gap closes in 2 s). Inside it you are Rooted. Drawback: you are shut in with what you chose, and Bedrock costs 50. |
| earth.hand | Hand of the Mountain | Signature | earth.tremor | Crown | While Bedrock lasts, a hand of stone as tall as a tower follows your arm. Each heavy blow you land, it lands again a moment after. | BESPOKE. Pressed: nothing new; it rides the surge. Seen: a forearm and fist of rock, four times the player's height, stands out of the ground behind the player and mirrors the sword arm; when a heavy blow or finisher lands, a 3 m circle is drawn under that enemy and half a second later the fist comes down on it, with the long shake of a finisher. Does: 70% of the blow's damage and all of its stagger again, to everything in the circle, and Tremor 1. Lasts the surge. Costs nothing more. |

---

# Metal

**The idea.** Metal is the weapon and the guard and nothing else. Its player wins by meeting blows exactly: every
perfect guard forges something, breaks armour or opens a line, and the blades it conjures are thrown in close, driven
in by hand and called back through the enemy.

**Affliction: Sundered.** Up to three notches, 8 s, refreshed whenever a notch is added. Each notch makes the enemy
take 8% more from weapons. At three its guard turns nothing: a soldier's spear or a captain's guard takes blows in
full until the armour mends. Shown as bright cracks across the enemy's plate, one per notch.

**Base spell: Flung Blade** (tap, 25 mana). A conjured blade is thrown at the enemy you face, up to 9 m: 14 damage,
8 stagger, one notch. It stays standing in the enemy for 6 s (one at a time), and the next weapon blow on that enemy
drives it in: 12 more damage and 15 stagger, and the blade is gone. Throw, close, drive. (Spell form: a mark that
detonates when struck.)

**Surge: Temper** (L2, 40 mana, 10 s). You take 30% less. A blow held on your guard lets no grey health through,
throws 20 stagger back at the attacker and gives 5 mana. For ten seconds a plain guard is nearly as good as a perfect
one, so it is pressed when a flurry is coming that cannot all be read. The weapon and forearms glow white-blue.

**Boughs.**

- **Guard**: the perfect guard forges something. Blades made from turned blows, weapons thrown wide, the guard as a blade.
- **Edge**: the weapon itself. Cutting into openings, breaking armour for good, honing in the open, the free skill.
- **Arsenal**: blades conjured. Blades left standing in enemies and in the ground, ricochets, and the recall.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| metal.sunder | Sunder | Root | (the middle) | Trunk | Heavy blows and perfect guards notch an enemy's armour, up to three. Each notch lets your weapon bite deeper. At three, its guard turns nothing. | Heavy blow lands: 1 notch. Perfect guard: 1 notch on the attacker. Finisher: 3. Sundered as defined above. |
| metal.forgedanswer | Forged Answer | Major | metal.sunder | Guard | A perfect guard forges a blade from the blow it turned. Up to three hang at your shoulder. Your next heavy blow sends them all. | Perfect guard: +1 hovering blade, to 3, each lasting 15 s (form: phalanx blades). Heavy blow or finisher lands: every blade fires into that enemy: 10 damage and 12 stagger each; three together add a notch. |
| metal.practisedwrist | Practised Wrist | Minor | metal.forgedanswer | Guard | The moment for a perfect guard is a little longer. | Perfect guard window 0.18 s to 0.21 s. |
| metal.brightsparks | Bright Sparks | Minor | metal.forgedanswer | Guard | Perfect guards give more mana. | 15 to 20. |
| metal.openline | Open Line | Major | metal.forgedanswer | Guard | A perfect guard throws the attacker's weapon wide. For a moment it cannot guard, and cannot turn to face you. | Perfect guard against a blow struck by hand: for 1.2 s the attacker has no guard and no turning, and your blows on it count as from behind (0.6 s on a god or elite). |
| metal.hardreturn | Hard Return | Minor | metal.openline | Guard | A perfect guard breaks more of the attacker's balance. | Stagger thrown back by a perfect guard up 15%. |
| metal.bracedframe | Braced Frame | Minor | metal.openline | Guard | A blow held on your guard lets less through. | Grey health from a guarded blow 35% to 25% of its damage. |
| metal.circle | Circle of Steel | Spell | metal.openline | Guard | Hold the spell button and your blade turns all round you. For a breath, every blow from any side is met as a perfect guard. 40 mana. | Form: elemental parry, all round. 1.2 s, no moving or attacking, then 0.3 s to recover. Up to four blows, red ones too, each a perfect guard for every trigger (stagger, notch, Forged Answer) but 5 mana each, not 15. |
| metal.longtemper | Long Temper | Minor | metal.circle | Guard | Temper lasts longer. | Surge length 10 s to 13 s. |
| metal.edgeguard | Edge Guard | Keystone | metal.circle | Guard | Your guard no longer holds a blow: only the perfect guard turns one. Its moment is longer, it may be pressed again at once, and each one cuts the attacker as your weapon would. | BESPOKE. Pressed: guard, as a tap. Seen: no blue band on the ground any more; each press is a short bright arc of the blade, and a success throws the gold flare and a white cut line across the attacker. Does: a held guard blocks nothing. The perfect window is 0.28 s (from 0.18) and the wait before another is 0.2 s (from 0.4). Each perfect guard also deals a light blow's damage to the attacker and counts as a blow landed (mana from a blow, grey health won back, a notch). Under Temper the window is 0.33 s in place of Temper's guard gifts. Drawback: there is nothing to fall back on. |
| metal.countercut | Counter Cut | Major | metal.sunder | Edge | A blow that lands while an enemy is showing its own attack is critical. | A weapon blow lands on an enemy in its wind-up: critical, and 25% more stagger. (Small extension: the blow-lands triggers read the target's state.) |
| metal.deepbite | Deep Bite | Minor | metal.countercut | Edge | Each notch lets your weapon bite deeper still. | Affliction strength 8% to 10% a notch. |
| metal.finishingedge | Finishing Edge | Minor | metal.countercut | Edge | The last blow of a chain cuts deeper. | Weapon damage of the blow that ends a chain up 12%. |
| metal.splinter | Splinter | Major | metal.countercut | Edge | A blow on armour notched three deep throws its splinters into whoever stands behind. | An enemy at full depth is hit by the weapon: a cone behind it, 4 m, 60 degrees: 8 damage, 8 stagger, 1 notch (doubled from a heavy blow). |
| metal.cleanedge | Clean Edge | Minor | metal.splinter | Edge | Critical blows cut deeper. | Critical damage 1.5 to 1.65 times. |
| metal.shatterpoint | Shatter Point | Major | metal.splinter | Edge | A finisher on notched armour breaks it for good. The notches never mend, and the finisher strikes harder for each. | Finisher lands on an enemy with any notch: +10% finisher damage per notch it had, and that enemy stays at three notches for as long as it lives. |
| metal.slowmending | Slow Mending | Minor | metal.shatterpoint | Edge | Notched armour takes longer to mend. | Affliction length 8 s to 12 s. |
| metal.hone | Hone | Spell | metal.shatterpoint | Edge | Hold the spell button to draw a stone down the blade, in the open. Your next five blows cannot be turned, and each one notches. A blow taken while honing wastes it. 30 mana. | A cast of 0.8 s (a held pose, sparks running the length of the blade) that being struck or rolling breaks, the mana spent. Then a boon for 12 s or five weapon blows, shown as five glints by the mana bar: those blows pass any enemy guard, deal 10% more and lay a notch each. |
| metal.balancedblade | Balanced Blade | Minor | metal.hone | Edge | Swings cost less stamina. | Stamina cost of attacks down 10%. |
| metal.mastersanswer | Master's Answer | Keystone | metal.hone | Edge | After a perfect guard, your weapon's skill is free and comes at once, with no time to charge. At any other time it costs more. | Perfect guard: a 2 s boon in which the weapon skill costs no mana and needs no hold (holding heavy releases it at once: the Iaido Slash straight out of a turned blow). Otherwise the weapon skill costs half again as much (Iaido Slash 30 to 45). |
| metal.buriedblades | Buried Blades | Major | metal.sunder | Arsenal | Up to three of your thrown blades may stand in one enemy. A heavy blow drives in every one at once. | Flung Blade's limit in an enemy 1 to 3. Heavy blow or finisher lands: every standing blade is driven (12 damage, 15 stagger, 1 notch each). A light blow still drives one. |
| metal.trueflight | True Flight | Minor | metal.buriedblades | Arsenal | Conjured blades strike harder. | Spell damage up 15%. |
| metal.readysteel | Ready Steel | Minor | metal.buriedblades | Arsenal | Flung Blade costs less mana. | 25 to 20. |
| metal.ricochet | Ricochet | Major | metal.buriedblades | Arsenal | A blade that strikes, or is driven in, springs on to the next enemy near, and then the next. | A conjured blade hits or is driven: it leaps to the nearest other enemy within 6 m, twice, for 70% and then 50% of its damage, and stands in each. |
| metal.longthrow | Long Throw | Minor | metal.ricochet | Arsenal | Blades fly further and spring further. | Spell reach up 25%. |
| metal.returningedge | Returning Edge | Spell | metal.ricochet | Arsenal | Hold the spell button to throw a great turning blade out and back. Guard at the instant it returns and it goes out again, faster and harder, up to three times. 35 mana. | BESPOKE. Pressed: hold Triangle to throw; then L1 as it comes home. Seen: a spinning blade a metre and a half across flies 8 m along the facing and back to the player's hand, drawing its lane on the ground; a ring closes on the player as it nears. Does: 12 damage and 10 stagger to each enemy it passes, each way, and a notch on the first pass. A guard inside the perfect window as it arrives bats it out again along the current facing at 1.4 times the speed and 1.5 times the damage each time (three returns at most); each counts as a perfect guard for Metal's marks but gives no mana. Missed, it is simply caught. |
| metal.paidinblows | Paid in Blows | Minor | metal.returningedge | Arsenal | Blows on notched armour give more mana. | Mana from blows on Sundered enemies up 25%. |
| metal.graveofswords | Grave of Swords | Spell | metal.returningedge | Arsenal | Hold the spell button and twelve blades fall in a ring round you and stand in the ground. Each blow you land calls one through its target. A heavy blow calls all that remain. 50 mana. | Forms: blades from above, then phalanx blades. Twelve blades fall on a ring 4.5 m from the player (8 damage to anything under one) and stand 12 s. Light blow lands: the nearest standing blade flies through that enemy: 9 damage, 6 stagger. Heavy blow or finisher lands: every blade left does the same, and each three add a notch. The blades left are counted on the ground for all to see. |
| metal.allbladeshome | All Blades Home | Keystone | metal.graveofswords | Arsenal | Blades you throw or plant no longer fade. Beginning Temper calls every one of them back to you, through whatever stands between. Temper costs more. | BESPOKE. Pressed: the surge. Seen: every conjured blade in the world (standing in enemies, in the ground, hovering) turns its point to the player, hangs for a quarter of a second, and flies home in straight bright lines; they arrive as a wheel of blades that folds into the weapon. Does: blades have no lifetime (at most sixteen exist; the oldest goes). On the surge beginning each flies to the player and deals 12 damage, 8 stagger and a notch to every enemy on its line; each blade that arrives adds 0.4 s to Temper. Drawback: Temper costs 55, and blades once called are gone. |
| metal.hangingswords | Hanging Swords | Signature | metal.sunder | Crown | While Temper lasts, every blow you hold on your guard is a perfect guard, and a sword is forged above the one who struck. When Temper ends, they fall. | BESPOKE. Pressed: the surge, then simply hold guard and face the blows. Seen: each blow met throws the gold flare, and a pale sword appears point down in the air over the attacker; they gather in a ring over its head, up to eight; as Temper ends they hang one beat and drop together. Does: a held guard treats every blow that can be guarded as a perfect guard (stagger, notch, all triggers; 5 mana each). Red attacks still need the true timing. At the surge's end each sword falls on its enemy: 16 damage and 20 stagger a sword. Lasts the surge. Costs nothing more. |

---

# Light

**The idea.** Light rewards the exact moment and the clean fight. Its player marks an enemy, lands the few blows
that matter as criticals, keeps count of every wound given and taken, and ends a fight with a sentence that falls
from above.

**Affliction: Marked.** A mark lasts 8 s and carries three critical blows: the next three weapon blows on a Marked
enemy are critical (1.5 times) and give half again as much mana, and then the mark is spent. Marking again refills
the three. Shown as a bright brand over the enemy with three pips that go out one by one.

**Base spell: Glare** (tap, 25 mana). A flash thrown in a cone 4 m ahead: 8 damage, 22 stagger, and everything in it
is Marked. It does almost nothing by itself: it makes the next three blows count. (Spell form: wave.)

**Surge: Clarity** (L2, 40 mana, 8 s). Purification: every affliction on you is burned away, none takes hold while it
lasts, and your grey health is made whole at once. And for those eight seconds the moments for a perfect guard and a
perfect dodge are half again as long (0.27 s and 0.2 s). It is pressed before the hard part of a fight, not after.
A thin ring of light stands round the player.

**Boughs.**

- **Sight**: precision. The chain timed to the instant, the blinded enemy, the step behind, the fight without a wound.
- **Sentence**: marks and judgement. Marks that deepen, the falling blade, the count kept, the seal, the condemned.
- **Radiance**: the light itself. A full bar that overflows into the blade, the lantern, the gathered beam, the second sun.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| light.mark | Marked | Root | (the middle) | Trunk | The last blow of a chain and every perfect guard leave a mark of light. The next three blows on a Marked enemy are critical and give more mana. | The blow that ends a chain lands (third light, or a heavy after lights): Mark. Perfect guard: Mark the attacker. Marked as defined above. |
| light.justcut | Just Cut | Major | light.mark | Sight | Press the next blow of a chain at the instant the last one lands and it flashes. A flashing blow is critical and costs no stamina. | BESPOKE. Pressed: the next attack of a chain within 0.08 s either side of the moment the current blow hits. Seen: a white glint runs the blade at that instant, and a flashing blow leaves a longer, brighter cut line. Does: that next blow is critical and free of stamina. A press at any other time is the ordinary buffered blow: nothing is lost by missing. |
| light.sharplight | Sharp Light | Minor | light.justcut | Sight | Critical blows strike harder. | Critical damage 1.5 to 1.65 times. |
| light.measuredguard | Measured Guard | Minor | light.justcut | Sight | The moment for a perfect guard is a little longer. | 0.18 s to 0.21 s. |
| light.dazzle | Dazzle | Major | light.justcut | Sight | A perfect dodge flashes in the attacker's eyes. Blinded, it cannot turn to follow you, and it is Marked. | Perfect dodge: the attacker is Marked and for 2 s cannot turn; any attack it begins goes where it was already facing (1 s on a god or elite). |
| light.narrowescape | Narrow Escape | Minor | light.dazzle | Sight | The moment for a perfect dodge is a little longer. | 0.13 s to 0.16 s. |
| light.brightevasion | Bright Evasion | Minor | light.dazzle | Sight | Perfect dodges give more mana. | 12 to 17. |
| light.step | Step Into Light | Spell | light.dazzle | Sight | Hold the spell button to vanish and stand behind the enemy you face. Do it as its blow is about to land, and your next blow strikes as a finisher does. 35 mana. | Action: a short step to behind the target, up to 7 m; it is Marked. Cast within 0.2 s before a blow would have landed: it counts as a perfect dodge, and the next weapon blow within 1.5 s deals a finisher's damage (three times) without ending any stagger. |
| light.longclarity | Long Clarity | Minor | light.step | Sight | Clarity lasts longer. | Surge length 8 s to 11 s. |
| light.unblemished | Unblemished | Keystone | light.step | Sight | Fight a while without a wound and every blow you land is critical. A wound ends it, and until it returns you take more from every blow. | After 8 s in a fight with no unguarded blow taken: a boon with no end: every weapon blow is critical and mana from blows is up 25%. Player is struck (a blow held on the guard does not count): the boon ends, and for the 8 s until it returns damage taken is up 15%. The player's ink outline turns bright while it holds. |
| light.weighed | Weighed | Major | light.mark | Sentence | Marks deepen, up to three. Each one makes your critical blows strike harder. | Affliction depth 1 to 3: each marking adds a depth and refills the three blows. Each depth beyond the first adds 15% to critical damage against that enemy. Shown as one, two or three brands. |
| light.plainguilt | Plain Guilt | Minor | light.weighed | Sentence | Marked enemies take more from your weapon. | Damage to afflicted enemies up 6%. |
| light.lastingmark | Lasting Mark | Minor | light.weighed | Sentence | A mark gives one more critical blow before it is spent. | Three blows to four. |
| light.sentence | Sentence | Major | light.weighed | Sentence | A heavy blow on an enemy marked three deep passes sentence. The marks are spent, and a blade of light falls on it a moment later. | Heavy blow lands on an enemy at full depth: set the affliction off: a blade falls on the target after 0.8 s (a 1.8 m circle that follows it): 36 damage, 45 stagger. |
| light.richmark | Rich Mark | Minor | light.sentence | Sentence | Blows on Marked enemies give still more mana. | The mark's mana bonus 50% to 75%. |
| light.bowedhead | Bowed Head | Minor | light.sentence | Sentence | Critical blows break balance sooner. | Stagger dealt by critical blows up 20%. |
| light.spears | Ring of Spears | Spell | light.sentence | Sentence | Hold the spell button. Six spears of light stand in the air round the enemy you face, wait a breath, and drive in together. It is held where it stands. 40 mana. | Form: ring of many bullets set round the target, then fired together. Six spears on a 3 m ring hang 0.6 s and strike: 6 damage each, 30 stagger in all, one depth of Mark, and the target is held still 1.5 s (0.5 s on a god or elite). |
| light.whatitowes | What It Owes | Major | light.spears | Sentence | Light keeps count of every wound an enemy gives you. Sentence pays them back. | Player is struck: the attacker is Marked once and gains a tally, to five (small strokes of light over its bar). Sentence's blade deals 8 more for each tally and spends them. |
| light.reckoning | Reckoning | Spell | light.whatitowes | Sentence | Hold the spell button to set a seal on the enemy you face. A hand turns once round it. When it comes round, half of all your weapon did to it in that time is done again at once. 45 mana. | BESPOKE. Pressed: hold Triangle. Seen: a ring like a clock face lies on the ground under the enemy and follows it; one hand sweeps it in 6 s and the ring fills behind the hand; on the last tick the ring breaks upward in a column of light. Does: counts all weapon damage you deal to that enemy for 6 s, then deals half of the count again in one burst (at most 150) with 40 stagger. If the enemy dies first the seal breaks at once on whoever is nearest it, at half strength. |
| light.heavyverdict | Heavy Verdict | Minor | light.reckoning | Sentence | Blades and seals of judgement strike harder. | Damage of Sentence, Reckoning and The Scales up 15%. |
| light.condemned | Condemned | Keystone | light.reckoning | Sentence | An enemy marked three deep and near death is Condemned: it may be finished whatever its balance. Your weapon deals less to enemies that bear no mark. | BESPOKE. Pressed: heavy, as for any finisher. Seen: a Condemned enemy carries the gold broken ring of the staggered, in white, with a thin sword of light hanging over it. Does: an enemy at full depth of Mark and under a quarter of its health (under a tenth for a god or elite) can be finished as if staggered; the finisher kills an ordinary enemy and takes its usual fixed share from a god or elite. Drawback: weapon damage to enemies with no mark is down 15%. |
| light.brimming | Brimming | Major | light.mark | Radiance | While your mana is full, every heavy blow throws a wave of light ahead of it. | Mana full and a heavy blow is swung: a wave 5 m ahead: 12 damage, 12 stagger, Marks. It costs nothing, so a full bar is worth keeping full until the right spell. |
| light.gleaning | Gleaning | Minor | light.brimming | Radiance | Every blow of the weapon gives a little more mana. | Mana from blows up 10%. |
| light.hardlight | Hard Light | Minor | light.brimming | Radiance | Spells of light strike harder. | Spell damage up 12%. |
| light.returninglight | Returning Light | Major | light.brimming | Radiance | A spell that strikes a Marked enemy gives back part of what it cost. | A spell hits a Marked enemy: 30% of that spell's cost returns as mana, once a cast. |
| light.longrays | Long Rays | Minor | light.returninglight | Radiance | Waves, rays and lanterns reach further. | Spell reach up 25%. |
| light.lantern | Standing Light | Spell | light.returninglight | Radiance | Hold the spell button to plant a lantern of light. Enemies in its glow are Marked again and again, and a perfect guard inside it flashes and staggers all near you. 35 mana. | Form: planted totem. A 5 m glow for 10 s. Every 2.5 s: one depth of Mark on each enemy inside. Perfect guard while the player stands inside: burst round the player, 3.5 m: 10 damage, 25 stagger. |
| light.readylight | Ready Light | Minor | light.lantern | Radiance | Glare costs less mana. | 25 to 20. |
| light.daybreak | Daybreak | Spell | light.lantern | Radiance | Hold the spell button and the blade begins to gather light. Every blow you land adds to it, up to eight. Hold again to loose it all down one line. 45 mana. | BESPOKE. Pressed: hold Triangle to begin; fight; hold Triangle again to release (it releases by itself after 7 s). Seen: bright rings stack up the blade, one for each blow landed, and at eight the blade is too bright to look at; on release the player levels it and a beam 12 m long and 1.6 m wide stands for 0.4 s. Does: the beam deals 8 damage and 6 stagger for each ring (64 and 48 at eight) to everything on the line and Marks it. With no rings there is no beam and the mana is lost. Being struck does not lose the rings. |
| light.twosuns | Two Suns | Keystone | light.daybreak | Radiance | When you cast a spell, a figure of light casts it again from the far side of your enemy. Spells cost more. | A spell is cast: a copy of the player made of light appears on the far side of the enemy faced, as far from it as the player is (6 m at most), and 0.4 s later casts the same spell back toward the player at 60% damage; neither harms the player. Spell cost up 25%. (Small extension: the copy action repeats the last spell where it now repeats the last blow.) |
| light.scales | The Scales | Signature | light.mark | Crown | While Clarity lasts, the wounds you deal and the wounds you take are weighed. If yours are the heavier when it ends, a sword of light falls on every Marked enemy. If not, the light leaves you for a while. | BESPOKE. Pressed: the surge. Seen: two wide pans of light lie on the floor either side of the fight, joined by a beam that tips as each fills (damage dealt on the right in gold, damage taken on the left in white); when Clarity ends the beam locks, and if gold is heavier a sword as tall as the screen comes down point first on each Marked enemy within 12 m (on the enemy faced if none is Marked). Does: each sword deals 60% of the difference between damage dealt and damage taken during Clarity, at most 120, with 80 stagger. If the wounds taken are the heavier: no sword, and no mana can be gained for 5 s. Lasts the surge. Costs nothing more. |

---

# Dark

**The idea.** Dark buys everything with its own health and takes it back out of the enemy. Its player is always in
debt, fights hardest when nearly dead, and has one death to spend.

**Affliction: Cursed.** 8 s, refreshed by any weapon blow, one depth. A Cursed enemy deals 15% less damage, and
killing it heals the player 6. Shown as a violet smoke that rises from the enemy and leans toward the player.

**Base spell: Grasp** (tap, 25 mana). A hand of shadow reaches 6 m along the ground to the enemy you face and drags
it to your feet: 10 damage, 12 stagger, Cursed. What cannot be dragged (a god, an elite) drags you to it instead.
Either way the fight is at arm's length again. (Action: a pull.)

**Surge: Hunger** (L2, no mana, 10 s). The price is health: it drains 3 a second while Hunger lasts (30 in all; the
drain cannot take the last point). In return the weapon deals 30% more and 30% of all weapon damage dealt comes back
as health. Land a hundred damage in ten seconds and it has paid for itself; land more and it is a heal. It is a
taste, by choice, of what follows.

**The Rise** (Dark's gift, not a button; once in a run). When health reaches nothing the player falls, and gets up.
A second and a quarter of rising in which nothing can touch them, a burst that shoves everything within 4 m away,
and the health bar fills again in violet. Risen: health drains 5 a second while any enemy is aware of the player;
40% of weapon damage dealt returns as health; flasks do nothing; an empty bar is death, and the run ends. Resting at
a fire ends the Rise and restores the living. This is the user's one revive, as decided; the tree grows it.

**Boughs.**

- **Debt**: power bought with health. Spells paid in blood, strength from what is owed, the pawned heart.
- **Hunger**: taking it back. Finishers that feed, health beyond full, fear among the Cursed, the jaw, no flasks at all.
- **Grave**: the edge of death and past it. Strength near the end, the Risen state, the shadow and the dead that fight.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| dark.curse | Curse | Root | (the middle) | Trunk | Blows of the weapon curse an enemy. A Cursed enemy deals less, and killing it heals you. | Any weapon blow lands: lay Cursed. Cursed as defined above. |
| dark.bloodprice | Blood Price | Major | dark.curse | Debt | With too little mana for a spell, the rest is paid in health. Health paid this way is only owed: blows win it back. | A spell is cast with too little mana: each missing point of mana costs half a point of health, turned to grey health (won back by blows as grey health is). It cannot take the last point of health. |
| dark.slowcollector | Slow Collector | Minor | dark.bloodprice | Debt | Health that is owed lingers longer before it is lost. | Grey health kept 2.5 s to 4 s. |
| dark.smallcuts | Small Cuts | Minor | dark.bloodprice | Debt | Prices paid in health are smaller. | Health costs (Blood Price, Hunger's drain, Pawned Heart's debt) down 20%. |
| dark.borrowed | Borrowed Strength | Major | dark.bloodprice | Debt | The more of your health is owed, the harder you strike. | Weapon damage up 0.75% for each point of grey health held, to 30%. |
| dark.deepwell | More to Spend | Minor | dark.borrowed | Debt | You have more health to spend. | Health up 10. |
| dark.recklessarm | Reckless Arm | Minor | dark.borrowed | Debt | While any health is owed, swings cost less stamina. | Stamina cost of attacks down 15% while grey health is held. |
| dark.debtscalled | Debts Called | Spell | dark.borrowed | Debt | Hold the spell button. Everything near is dragged to you, and then thrown back by a burst that is greater the less health you have. 40 mana. | Form: reverse blast. Pulls everything within 6 m in over 0.5 s, then a burst to 3.5 m: 18 damage, rising evenly to 36 at one point of health; 35 stagger; Cursed. |
| dark.bitterharvest | Bitter Harvest | Minor | dark.debtscalled | Debt | Below half your health, blows give more mana. | Mana from blows up 25% under half health. |
| dark.pawnedheart | Pawned Heart | Spell | dark.debtscalled | Debt | Hold the spell button to tear out your heart and set it down. While it beats there you stand one blow from death, and strike as you never have. Come back for it in time and it returns with what your blows have earned. No mana. | BESPOKE. Pressed: hold Triangle; later, walk onto the heart. Seen: a held pose, hand to the chest, and a dark heart the size of a head is set on the ground in a ring that shrinks over 10 s; it beats, and each beat sends a violet pulse across the floor that grows as the player deals damage; the player's body goes to an ink shape with a hole of light in the chest. Does: health drops to 1 and the rest is held in the heart. For up to 10 s the weapon deals 60% more and attacks cost no stamina. Touching the heart ends it: the health held comes back, plus 30% of the damage dealt meanwhile (to the top of the bar). If the ring closes first, the heart stops: the player keeps 1 health and half of what was held returns as grey health. Enemies do not strike the heart. Any blow in between is death, or the Rise. |
| dark.unpaid | Unpaid | Keystone | dark.pawnedheart | Debt | Every wound is only owed. All of it can be won back with blows before it is collected. Flasks heal half as much. | Numbers: every blow taken leaves all of its damage as grey health (the guard already leaves its share), and a fresh wound no longer wipes the grey health already held; it is collected (lost) 4 s after the last blow landed or guarded; blows win it back at 0.6 a point of damage as now. Flask healing down 50%. |
| dark.feast | Feast | Major | dark.curse | Hunger | A finisher on a Cursed enemy feeds you. It heals, and Hunger lasts a while longer. | Finisher lands on a Cursed enemy: heal 12; if the surge is on, it lasts 3 s more (and keeps draining). |
| dark.longcurse | Long Curse | Minor | dark.feast | Hunger | Curse lasts longer. | Affliction length 8 s to 12 s. |
| dark.heavycurse | Heavy Curse | Minor | dark.feast | Hunger | Cursed enemies deal less still. | Affliction strength 15% to 22%. |
| dark.glut | Glut | Major | dark.feast | Hunger | Healing beyond a full bar is kept as a second, dark measure of health that slowly drains away. | Any healing over full health is kept as a dark bar over the health bar, to 30 points; it loses 1 a second and takes damage before health does. (Small extension: a ward with a size.) |
| dark.fatofthekill | Fat of the Kill | Minor | dark.glut | Hunger | Killing a Cursed enemy heals more. | 6 to 9. |
| dark.dread | Dread | Major | dark.glut | Hunger | When a Cursed enemy dies, the other Cursed lose heart. Their balance buckles and their guard falls for a moment. | A blow kills a Cursed enemy: every other Cursed enemy within 8 m takes 30 stagger, has no guard for 3 s, and drops an attack it is showing unless it is red. |
| dark.easyprey | Easy Prey | Minor | dark.dread | Hunger | Cursed enemies take more from your weapon. | Damage to afflicted enemies up 8%. |
| dark.maw | Devouring Maw | Spell | dark.dread | Hunger | Hold the spell button and a great jaw rises out of your shadow and bites what stands before you. If the bite kills, or its prey is staggered, it swallows, and you are fed. 40 mana. | Form: dragon head that lunges. A jaw twice the player's height lunges 5 m along a lane 1.6 m wide: 30 damage, 30 stagger, Cursed. If it kills, or the enemy is staggered: heal 20. |
| dark.longhunger | Long Hunger | Minor | dark.maw | Hunger | Hunger lasts longer and takes no more for it. | Surge length 10 s to 13 s; the whole drain stays 30. |
| dark.neversated | Never Sated | Keystone | dark.maw | Hunger | You carry no flasks. Every blow you land feeds you a little, and every kill and finisher a great deal. | Flask count 0 (so nothing to throw, either). 6% of all weapon damage dealt returns as health at all times. Killing a Cursed enemy heals three times as much. A finisher heals 20. |
| dark.nearthegrave | Near the Grave | Major | dark.curse | Grave | Below a third of your health you strike harder, and each kill wraps you in a shade that takes the next blow. | Health below a third: weapon damage up 20%. A blow kills while below a third: a ward that blocks the next hit (one at a time). |
| dark.hardtobury | Hard to Bury | Minor | dark.nearthegrave | Grave | Below a third of your health you take less from every blow. | Damage taken down 12% under a third. |
| dark.hollowstrength | Hollow Strength | Major | dark.nearthegrave | Grave | Risen, you strike harder and no blow makes you flinch. Each kill stills the draining for a few breaths. | While Risen: weapon damage up 25%, no flinch. A blow kills while Risen: the drain stops for 4 s. |
| dark.slowrot | Slow Rot | Minor | dark.hollowstrength | Grave | Risen, your health drains more slowly. | 5 a second to 4. |
| dark.graveappetite | Grave Appetite | Minor | dark.hollowstrength | Grave | Risen, your blows feed you more. | 40% to 50% of weapon damage. |
| dark.secondshadow | Second Shadow | Spell | dark.hollowstrength | Grave | Hold the spell button to leave your shadow where you stand. It strikes when you strike, at whatever is near it. Hold again to change places with it. 45 mana. | BESPOKE. Pressed: hold Triangle to leave it; hold Triangle again to swap (once). Seen: the player's shape in flat black with violet eyes stays behind, joined to the player by a thin line along the ground. Does: for 8 s the shadow repeats every blow the player lands, a tenth of a second later, on the nearest enemy within its own reach, at 50% damage and stagger, laying Curse. The swap is instant, passes through blows as a perfect dodge does if timed as one, and ends the shadow 2 s later. Built on the form "a clone that fights beside you"; the swap is the new part. |
| dark.longshadows | Long Shadows | Minor | dark.secondshadow | Grave | Shades and shadows strike harder. | Damage of Second Shadow, The Dead Stand and Devouring Maw up 15%. |
| dark.deadstand | The Dead Stand | Spell | dark.secondshadow | Grave | Hold the spell button and those you have killed in this fight stand up where they fell, up to three. Each strikes a few times for you and is gone. 40 mana. | Form: minions. The three most recent kills within 12 m rise as violet shades of themselves; each walks to the nearest enemy and strikes three times over 5 s (12 damage, 12 stagger, Cursed), then falls apart. With no dead to raise nothing happens and nothing is spent. |
| dark.deadalready | Dead Already | Keystone | dark.deadstand | Grave | You do not wait to die. You are Risen from now on: in every fight your health drains, and dealing damage feeds you. You strike far harder. There is no second rising. | BESPOKE. Pressed: nothing; learning it changes the player for the run. Seen: the health bar turns violet for good and the player's eyes and wounds glow; the screen's edge carries a faint dark breath in every fight. Does: the Rise is spent the moment this is learned. In a fight health drains 2 a second; 30% of weapon damage dealt returns as health; weapon damage up 25%; no flinch from blows that can be guarded. Flasks still heal. Hollow Strength, Slow Rot and Grave Appetite apply at all times. Drawback: death is final from here on, and standing off costs health. |
| dark.lifeforalife | A Life for a Life | Signature | dark.curse | Crown | Risen, land a finisher on the one that killed you. You live again, and you may rise once more. | BESPOKE. Pressed: the finisher (heavy on a staggered enemy), as always. Seen: when the player rises, the enemy that struck the killing blow is branded with a violet sigil that can be seen through walls; when the finisher lands on it the whole screen goes to black ink with violet outlines for the held pose, a light is drawn out of the enemy into the player, and colour comes back with the health bar. Does: ends the Rise: the player is alive with half their health, and the Rise is ready again. If that enemy dies any other way the sigil passes to the nearest enemy. Held with Dead Already there is no Rise to return: each finisher stops the drain for 6 s instead. |

---

# Poison

**The idea.** Poison is the long game played at arm's length. Its player lays doses with every blow, then guards and
waits while the poison works, and chooses the moment to burst it or let it spread through the whole fight.

**Affliction: Poisoned.** Doses stack up to eight deep. Every 2 s (the beat) the poison bites: 2 damage and
1 stagger for each dose, all at once. All the doses fade together 6 s after the last one was added. Shown as green
pips round the enemy and a ring that closes on it with each beat, so the bite can be seen coming.

**Base spell: Miasma** (tap, 25 mana). A cloud is thrown 4 m ahead and hangs there for 4 s, 2.5 m across. Enemies in
it take one dose a second. The player stands in it unharmed, so it is a place to hold a fight, not a thing thrown
from safety. (Spell form: a field of ground hazard.)

**Surge: Fever** (L2, 40 mana, 10 s). While Fever lasts the beat comes twice as fast, and 40% of all the damage your
poison deals returns as health. It is worth nothing on a clean enemy and a great deal on three deep ones: the button
is pressed when the work is already done.

**Boughs.**

- **Patience**: depth and the beat. Blows timed to the bite, poison that ripens when left alone, the coiled strike.
- **Rupture**: bursts. Poison set off by a heavy blow, the dead that burst, the blister, the snap of the fingers.
- **Contagion**: spreading. Doses that leap, one beat for every enemy, the censer, the shared sickness, the carrier.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| poison.venom | Venom | Root | (the middle) | Trunk | Blows of the weapon poison, up to eight deep. On a slow, steady beat the poison bites for all its depth at once. | A light blow lands: 1 dose. A heavy blow: 2. A finisher: 4. Poisoned as defined above. |
| poison.intime | In Time | Major | poison.venom | Patience | A blow that lands as the poison bites is critical and doses twice. | A weapon blow lands within 0.2 s either side of the target's beat: critical, and one more dose. (Small extension: a timing condition on the blow-lands triggers. The closing ring is the cue.) |
| poison.deeperdose | Deeper Dose | Minor | poison.intime | Patience | Poison goes two doses deeper. | Affliction depth 8 to 10. |
| poison.bitterdose | Bitter Dose | Minor | poison.intime | Patience | Each dose bites harder. | Affliction strength 2 to 2.4 damage a dose. |
| poison.ripen | Left to Ripen | Major | poison.intime | Patience | Poison you leave alone ripens. Each bite that comes without a fresh blow from you is stronger than the last. | Each beat on an enemy that has had no dose from a weapon blow since its last beat: that bite and those after it deal 25% more, building to double. A dose from a weapon blow resets it; doses from clouds, leaps and spells do not. |
| poison.lingering | Lingering | Minor | poison.ripen | Patience | Poison stays longer before it fades. | Affliction length 6 s to 9 s. |
| poison.lance | Lance the Wound | Major | poison.ripen | Patience | A perfect guard makes the attacker's poison bite at once, and with it the poison in everyone near. | Perfect guard: an extra bite, at once, on the attacker and on every poisoned enemy within 4 m of it. No doses are spent and the beat goes on as it was. |
| poison.waitinggame | Waiting Game | Minor | poison.lance | Patience | Perfect guards give more mana. | 15 to 20. |
| poison.calmpulse | Calm Pulse | Minor | poison.lance | Patience | Holding your guard costs less stamina. | Guard cost 0.9 to 0.75 stamina a point. |
| poison.coiled | Coiled Strike | Spell | poison.lance | Patience | Hold the spell button to coil, for as long as you dare. Let go to strike through the enemy you face, deeper for every breath you waited. Let go as a blow is about to land and it passes through you. 35 mana. | BESPOKE. Pressed: hold Triangle up to 2.4 s; release. Seen: the player sinks low with the weapon drawn back along the body; three green fangs light over the head at 0.6, 1.4 and 2.4 s; on release a straight green line through the target, and the player is standing behind it. Does: a step through the enemy faced (6 m reach): 14 / 24 / 38 damage, 15 / 25 / 40 stagger and 2 / 4 / 8 doses by the fangs lit. Released within 0.2 s before a blow would land: the blow passes through (a perfect dodge, with its mana) and the strike counts as three fangs. The player can be struck while coiling and loses the coil and the mana. |
| poison.slowdeath | Slow Death | Keystone | poison.coiled | Patience | Your weapon wounds far less. Your poison has no limit to its depth, and it does not fade while its bearer lives. | Weapon damage down 30% (stagger unchanged). Affliction depth has no top (each dose past eight bites at half strength) and no length. Cannot be held with Volatile: learning one closes the other. |
| poison.rupture | Rupture | Major | poison.venom | Rupture | A heavy blow on an enemy poisoned to its full depth bursts the poison. All of it lands at once, and the depth is spent. | Heavy blow lands on an enemy at full depth: set the affliction off: 6 damage and 4 stagger for each dose (48 and 32 at eight) to that enemy. |
| poison.ripe | Ripe | Minor | poison.rupture | Rupture | Bursting poison deals more. | Damage of every poison burst up 15%. |
| poison.wideburst | Wider Rupture | Minor | poison.rupture | Rupture | Bursts of poison reach further. | Burst reach up 25%. |
| poison.rottenfruit | Rotten Fruit | Major | poison.rupture | Rupture | A poisoned enemy that dies bursts. Those the burst reaches take its depth. | A poisoned enemy dies: a burst round it, 3.5 m: 10 damage, and every enemy reached takes as many doses as the dead one carried. |
| poison.buckling | Buckling | Minor | poison.rottenfruit | Rupture | Bursting poison breaks balance sooner. | Stagger dealt by poison bursts up 25%. |
| poison.blister | Blister | Spell | poison.rottenfruit | Rupture | Hold the spell button to raise a blister on the enemy you face. It swells each time its poison bites, through three sizes. Break it with a heavy blow. Wait too long and it withers. 40 mana. | Form: a mark that detonates when struck (small extension: it grows). The blister starts at the first size and gains one each time the bearer's poison bites, to three. A heavy blow on the bearer bursts it: 20 / 40 / 70 damage in 2 / 3 / 4 m, and 2 / 4 / 6 doses on everyone else reached. It withers 4 s after reaching the third size, or 12 s after it was raised. |
| poison.spentvenom | Spent Venom | Minor | poison.blister | Rupture | Blows on poisoned enemies give more mana. | Mana from blows on afflicted enemies up 20%. |
| poison.onebyone | One by One | Spell | poison.blister | Rupture | Hold the spell button and raise your hand. At the snap of your fingers every poisoned enemy bursts in turn, nearest first, each passing its depth on to the next. 50 mana. | BESPOKE. Pressed: hold Triangle. Seen: the player lifts a hand and holds it (0.5 s) while a thin green line runs along the ground from enemy to enemy in the order they will go, nearest first; the fingers snap with a one-frame wash, and the bursts go off along the line a quarter of a second apart. Does: each poisoned enemy within 10 m has its poison set off (as Rupture: 6 damage and 4 stagger a dose) and its doses are added to the next in line before that one bursts, to twice the depth limit. The last burst is the largest. Being struck in the held pose loses the cast and the mana. |
| poison.volatile | Volatile | Keystone | poison.onebyone | Rupture | Your poison no longer bites on its beat. It builds until it is full, and then bursts by itself and begins again. | The beat is off. Affliction reaches full depth: set it off at once: 7 damage and 4 stagger a dose to everything within 2.5 m, and the depth returns to nothing. Fever heals from bursts. Marks that speak of the bite (In Time, Left to Ripen, One Sickness) do nothing; Lance the Wound adds two doses instead. Cannot be held with Slow Death. |
| poison.catching | Catching | Major | poison.venom | Contagion | Each time poison bites, one dose leaps to the nearest enemy within reach. | On each beat: an arc from the bitten enemy to the nearest other enemy within 4 m lays 1 dose there. The first keeps its own. |
| poison.stillair | Still Air | Minor | poison.catching | Contagion | Clouds of poison hang longer. | Miasma and censer smoke last 50% longer. |
| poison.widecloud | Wide Cloud | Minor | poison.catching | Contagion | Clouds of poison are larger. | Their reach up 25%. |
| poison.onesickness | One Sickness | Major | poison.catching | Contagion | The poison in every enemy bites on the same beat. Each poisoned enemy beyond the first makes every bite stronger. | Every enemy's beat is the same beat. Each poisoned enemy in the fight after the first adds 10% to all poison bites, to 40%. |
| poison.longleap | Long Leap | Minor | poison.onesickness | Contagion | Poison leaps further between enemies. | Reach of Catching and of every dose that spreads 4 m to 6 m. |
| poison.censer | Swinging Censer | Spell | poison.onesickness | Contagion | Hold the spell button to set a smoking censer swinging round you on its chain. It doses whatever it passes and leaves its smoke behind. 35 mana. | Form: spinning disc that orbits (one, not two). It circles 3 m from the player once every 1.2 s for 8 s: each pass deals 4 damage and 1 dose, and it trails patches of cloud that last 2 s and dose once a second. |
| poison.readypoison | Ready Poison | Minor | poison.censer | Contagion | Miasma costs less mana. | 25 to 20. |
| poison.sharealike | Share Alike | Spell | poison.censer | Contagion | Hold the spell button to rush through everything before you. Every enemy you pass is poisoned as deeply as the sickest among them. 40 mana. | Form: drill dash through enemies. 7 m, 10 damage and 10 stagger to each enemy passed. Then every enemy passed is raised to the depth of the deepest among them (to at least two). |
| poison.sweetfever | Sweet Fever | Minor | poison.sharealike | Contagion | Fever returns more of what the poison takes. | Fever's healing 40% to 55% of poison damage. |
| poison.plaguebearer | Plague Bearer | Keystone | poison.sharealike | Contagion | You carry the sickness. Enemies near you are poisoned without a blow, deeper the longer they stay. Your flasks heal less. | Every 1.5 s in a fight: lay 1 dose on everything within 4 m of the player (a faint green ring on the ground shows the reach). Flask healing down 30%. |
| poison.cleanblade | Clean Blade | Signature | poison.venom | Crown | While Fever lasts the poison waits, and every blow doses twice. When it ends you flick the blade clean, and every bite that was owed lands at once. | BESPOKE. Pressed: the surge; the ending plays by itself. Seen: while Fever lasts the closing rings on every enemy hang still and a tally of owed bites climbs over each one; as Fever ends the player stops, snaps the weapon out to the side to throw the venom off it (a held pose, half a second, time slowed), and on the flick every tally breaks at once in a green burst. Does: during Fever no poison bites, doses have no depth limit, and each weapon blow lays double. At the end each enemy takes every bite it would have taken at Fever's doubled beat, with its depth as it stands at the end, in one blow that staggers; Fever's healing is paid on all of it together. The player cannot be flinched during the flick. Lasts the surge. Costs nothing more. |

---

# The mixed branches

Each grows in the gap between two neighbours and opens to a player who holds one of the two elements and has consumed
or held the other in this run. Every mark is written to work for **either** holder: none of them leans on a spell or
surge that only one of the two elements owns. Where a mark says "your surge", it means whichever surge the player has.
The first mark of each branch grows from either root. The **crown** is the mix's Signature: never sold, given when
the second element's god is consumed (as Plasma is today). Cost is the first thing in each Design cell.

## Overgrowth (Life + Earth)

**The idea.** The ground itself takes the player's side. Things are planted and grow while the fight goes on, and
the player who stands in one place is fed by it.

**Why want it:** Stone Oak. **Crown:** The Ground Wakes.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| overgrowth.rootgrip | Root Grip | Major | earth.tremor or Life's root | Overgrowth | Heavy blows lay both Tremor and Seed. A heavy blow on an enemy that bears both sends roots up its legs and holds it where it stands. | Cost 3. Heavy blow lands: lay Tremor 1 and Seeded, whichever element is held. Heavy blow lands on an enemy with both: held still 1.2 s (a god or elite: Tremor to full depth instead). |
| overgrowth.moss | Moss on Stone | Major | overgrowth.rootgrip | Overgrowth | While you fight without rolling you heal slowly, and thorns grow on you that cut whoever strikes you. | Cost 3. Every 3 s in a fight with no dodge begun: heal 2. Player is struck after 3 s without a dodge: the attacker takes 8 damage and is Seeded. A roll resets both. |
| overgrowth.briarpatch | Briar Patch | Major | overgrowth.moss | Overgrowth | The thorns of a Seeded enemy's death take root where they fall. The patch slows what walks in it and seeds it. | Cost 3. A Seeded enemy dies: a field of ground hazard, 3 m, 6 s: Tremor 1 and Seeded on entry and every 2 s, 3 damage a second. |
| overgrowth.thornwall | Thorn Wall | Spell | overgrowth.moss | Overgrowth | Hold the spell button to raise a hedge of stone and briar before you. Enemies that touch it are cut and Seeded. A heavy blow breaks it into thorns. 35 mana. | Cost 4. Form: wall that blocks and bursts. 5 m wide, 2.5 m ahead, 8 s. Enemies touching it: 4 damage a second, Seeded. A heavy blow of yours shatters it forward in a cone, 5 m: 20 damage, 25 stagger, Seeded and Tremor 1. |
| overgrowth.stoneoak | Stone Oak | Spell | overgrowth.thornwall | Overgrowth | Hold the spell button to plant a sapling. It grows as you land blows near it, through three ages. Full grown, its roots strike what comes near and its shade heals you. 45 mana. | Cost 5. Form: planted totem that keeps striking (small extension: it grows). Lives 20 s. Six weapon blows landed within 8 m of it grow it an age; it has three. Sapling: nothing. Young tree: its roots strike the nearest enemy within 5 m every 2 s (10 damage, 15 stagger, Tremor 1). Old oak: the strikes hit all within 6 m and Seed, and the player heals 3 a second inside its shade (4 m). It blocks movement like a pillar. |
| overgrowth.livingrampart | Living Rampart | Keystone | overgrowth.briarpatch | Overgrowth | Your guard is alive. Every blow held on it heals you a little and seeds the one who struck. A perfect guard roots the attacker where it stands. Guarding costs more stamina. | Cost 5. Plain guard: heal 2, attacker Seeded. Perfect guard: attacker held still 1 s (0.4 s on a god or elite) and Tremor 2. Guard stamina cost up 15%. |
| overgrowth.groundwakes | The Ground Wakes | Signature | overgrowth.rootgrip | Crown | When your surge begins, the ground rises round you in a ring of root and stone and holds every enemy inside it. Then it flowers, and you are healed for each one held. | BESPOKE. Pressed: the surge. Seen: a ring 14 m across cracks open round the player and roots as thick as trunks, with slabs of the floor still on them, arch up and inward over every enemy inside; they hold for two seconds; then every root flowers pale green at once and sinks back. Does: every enemy within 7 m is held still 2 s (0.8 s for a god or elite) with Tremor at full depth and Seeded; at the flowering each one held takes 15 damage and the player heals 8 for each. Costs nothing more than the surge. |

## Forge (Earth + Metal)

**The idea.** The battlefield is a smithy: stone is the anvil, the weapon is the hammer, and the enemy is what lies
between. Blows are few, heavy and timed, and things are made by them.

**Why want it:** Drawn From Stone. **Crown:** World Anvil.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| forge.hammeranvil | Hammer and Anvil | Major | earth.tremor or metal.sunder | Forge | A heavy blow on an enemy with stone at its back strikes twice: once from your weapon, once from the stone. | Cost 3. Heavy blow lands on an enemy within 1.5 m of a wall, a pillar or any standing thing: 60% more damage and stagger, a notch and Tremor 1 (both laid whichever element is held). |
| forge.struckore | Struck Ore | Major | forge.hammeranvil | Forge | A perfect guard strikes a spike of ore from the ground under the attacker. It stands. A heavy blow breaks it into blades. | Cost 3. Perfect guard: a short iron pillar bursts up under the attacker: 10 damage, 20 stagger; it stands 8 s (two at most) and counts as stone for Hammer and Anvil. A heavy blow of yours on it throws three blades in a cone away from you: 9 damage and a notch each. |
| forge.buckled | Buckled | Major | forge.struckore | Forge | An enemy both shaken and notched loses its balance far sooner. | Cost 3. An enemy with Tremor and Sundered at once takes 40% more stagger from every source. |
| forge.triphammer | Trip Hammer | Spell | forge.struckore | Forge | Hold the spell button and a great hammer forms above the enemy you face, and falls. Land a heavy blow of your own as it lands and both strike twice as hard. 40 mana. | Cost 4. Form: a pillar that falls on the target after a delay. A 2.5 m circle follows the enemy for 0.5 s, holds 0.5 s (the circle's rim closes to show the instant), and the hammer lands: 30 damage, 50 stagger, Tremor 1, a notch. A heavy blow of yours landing on that enemy within 0.2 s of the hammer: the hammer and that blow both deal double. (Small extension: the timing check.) |
| forge.drawnfromstone | Drawn From Stone | Spell | forge.triphammer | Forge | Hold the spell button to thrust your hand into the ground and draw out a sword of stone and iron far too large for you. It gives three great swings. The third breaks it. 50 mana. | Cost 5. BESPOKE. Pressed: hold Triangle; then the light or heavy button three times. Seen: a held pose on one knee, arm in the ground to the elbow (0.6 s), then the player stands dragging out a slab-bladed sword twice their height, carried on the shoulder; each swing is a slow half circle drawn on the ground before it lands; on the third the blade bursts into shards down its line. Does: for three swings (or 10 s) the weapon is replaced: each swing is a heavy blow over 180 degrees to 4 m: 40 damage, 60 stagger, Tremor 1, a notch, and it passes any guard. The third also throws a cone of shards 6 m: 20 damage. No flinch while drawing it or swinging, 30% less damage taken; the swings are slow (0.7 s wind-up) and cannot be cancelled by a roll. |
| forge.foldedsteel | Folded Steel | Keystone | forge.buckled | Forge | Every finisher folds your blade once more, up to ten. Each fold adds to every blow for the rest of the run. Each fold makes it heavier in the hand. | Cost 5. Finisher lands: +1 fold, to ten, kept until the run ends (ten small lines on the HUD by the weapon). Each fold: weapon damage up 3%, stamina cost of attacks up 1%. Taking another element keeps the folds; they belong to the blade. |
| forge.worldanvil | World Anvil | Signature | forge.hammeranvil | Crown | Your finisher is struck on an anvil that rises out of the ground. Its ring breaks the armour of everyone near and shakes them, and a blade is forged for each one it reaches. | BESPOKE. Pressed: the finisher, as always. Seen: as the player steps in, a block of dark stone with an iron face heaves up under the staggered enemy and lifts it; the blow comes down on it like a hammer; one white frame, and a ring of sparks runs out across the floor to 6 m; over the player a blade appears for each enemy the ring touched. Does: the finisher itself is unchanged. The ring: 15 damage, 30 stagger, Tremor and Sundered to full depth on every other enemy within 6 m. The blades (six at most) hover and fire with the next heavy blow, as Forged Answer's do: 10 damage and 12 stagger each. |

## Halo (Metal + Light)

**The idea.** The perfect guard made radiant. Critical blows forge blades of light that stand in a ring behind the
player's shoulders, and a guard at the exact instant looses them, blinds, and sends back what was thrown.

**Why want it:** Halo of Blades. **Crown:** Six Mirrors.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| halo.brightsteel | Bright Steel | Major | metal.sunder or light.mark | Halo | A perfect guard flashes off the blade. The attacker is Marked, and its armour is notched. | Cost 3. Perfect guard: lay Marked and one notch of Sundered on the attacker, whichever element is held. |
| halo.haloofblades | Halo of Blades | Major | halo.brightsteel | Halo | Each critical blow forges a blade of light in a ring behind your shoulders, up to six. A perfect guard looses them all at the one who struck. | Cost 4. A critical blow lands: +1 blade, to six, standing in a wheel behind the player (they stay until loosed). Perfect guard: all fire at the attacker (form: phalanx blades): 9 damage and 8 stagger each; every two add a notch. |
| halo.mirroredge | Mirror Edge | Major | halo.haloofblades | Halo | A perfect guard against anything thrown sends it back as light. | Cost 3. Perfect guard against a thrown attack (a bolt, a knife, a caster's spell; a beam's one burn): a projectile returns to the thrower for twice the attack's damage as a critical, with 30 stagger, and Marks it. |
| halo.answeringdraw | Answering Draw | Major | halo.brightsteel | Halo | Needs the Katana. Release the Iaido Slash at the instant a blow would land. The blow is turned, and the draw cuts as light: critical, and through any guard. | Cost 4. Needs the Katana. The held heavy is released within the perfect guard window of an incoming blow, red ones too: that blow is turned as a perfect guard (all triggers), and the Iaido Slash is critical, passes any guard, Marks and lays a notch on everything it cuts. (Small extension: the perfect guard check on the skill's release.) |
| halo.sunwheels | Sun Wheels | Spell | halo.haloofblades | Halo | Hold the spell button and two wheels of bright steel turn round you. A perfect guard sends both out through the attacker and back. 40 mana. | Cost 4. Form: twin spinning discs that orbit. 2.5 m from the player, 9 s: each pass 5 damage and 4 stagger; every fourth pass on an enemy Marks it. Perfect guard: both fly through the attacker and return (form: returning projectile): 14 damage and a notch each way. |
| halo.blindingguard | Blinding Guard | Keystone | halo.mirroredge | Halo | A perfect guard blinds everyone before you. They are Marked, and for a breath none of them can begin an attack. A blow held on a plain guard costs more stamina. | Cost 5. Perfect guard: a cone ahead, 6 m, 120 degrees: every enemy in it is Marked, takes 15 stagger, and cannot start an attack for 1.5 s (0.6 s for a god or elite; an attack already shown goes on). Plain guard stamina cost up 20%. |
| halo.sixmirrors | Six Mirrors | Signature | halo.brightsteel | Crown | Hold the spell button and six mirrors of steel stand in a ring round the enemy you face. You pass through it from each one in turn, faster than can be followed. The cuts land when you are back where you began. 60 mana. | BESPOKE. A spell, worn in the hold-spell slot like any other. Pressed: hold Triangle. Seen: six tall blades with mirror faces drop into a ring 4 m from the enemy (0.3 s), each showing the player's reflection; the player vanishes; six straight white lines are drawn through the enemy, mirror to mirror, one after another in under a second, with a figure of light at the end of each; the player is standing where they started, weapon lowered; the mirrors crack together, and only then does the enemy take the cuts. Does: six cuts of 14 damage and 12 stagger, each critical, Marking and laying a notch; other enemies on a line take half. Nothing can touch the player from the vanishing to the last cut landing (1.4 s in all); the player can be struck during the 0.3 s in which the mirrors fall, and loses the cast and the mana. |

## Eclipse (Light + Dark)

**The idea.** The pair the last god carries. Light pays in mana for the clean fight and Dark pays in health for the
desperate one, and the player who holds both is paid twice for walking the line between them.

**Why want it:** Day and Night. **Crown:** Totality.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| eclipse.twilight | Twilight | Major | light.mark or dark.curse | Eclipse | The blow that ends a chain both Marks and Curses. Critical blows on an enemy that bears both feed you. | Cost 3. The blow that ends a chain lands: lay Marked and Cursed, whichever element is held. A critical blow lands on an enemy with both: heal 15% of its damage. |
| eclipse.daynight | Day and Night | Major | eclipse.twilight | Eclipse | A perfect guard is of the day and gives mana. A perfect dodge is of the night and gives health. Do one after the other, and the second gives double. | Cost 4. Perfect dodge: heal 5 (as well as its mana). Perfect guard: as now. A perfect guard within 6 s of a perfect dodge, or a perfect dodge within 6 s of a perfect guard: the second one's mana and healing are doubled. A small sun or moon by the mana bar shows which was last. |
| eclipse.lowsun | Low Sun | Major | eclipse.daynight | Eclipse | The less health you have, the more mana every blow gives. | Cost 3. Mana from blows rises evenly from its usual amount at full health to double at one point of health. |
| eclipse.blacksun | Black Sun | Spell | eclipse.daynight | Eclipse | Hold the spell button to hang a black sun over the enemy you face. It draws everything beneath it inward, and it grows with each blow you land in its shadow. Then it sets, and its rim breaks outward as light. 50 mana. | Cost 4. Forms: slow orb that detonates, with a pull. A black disc with a bright rim hangs over the target's place and throws a round shadow 5 m across for 5 s: enemies under it are Cursed and pulled gently to the middle. Each weapon blow you land under it adds to it, to ten. When it sets: a ring of light bursts from the rim to 6 m: 20 damage plus 5 a blow (70 at most), 40 stagger, Marked, and critical against the Cursed. |
| eclipse.duskdawn | Dusk and Dawn | Spell | eclipse.twilight | Eclipse | Hold the spell button to cut through the enemy you face with a blade of dark. It wounds nothing yet. Land a heavy blow before the line fades and the cut of light answers along it, and both land together. 45 mana. | Cost 4. BESPOKE. Pressed: hold Triangle; then a heavy blow within 3 s. Seen: the player passes through the enemy leaving a black line hanging across its body and on the ground under it; the line thins as the 3 s run out; when the heavy blow lands the line turns white from one end to the other and a second cut tears along it. Does: the first cut is a step through the target (5 m), no damage, Cursed. The heavy blow that follows lands with the stored cut: 45 damage and 40 stagger more, critical, Marked, and 10% of all of it returns as health. Being struck before the heavy blow wipes the line. |
| eclipse.dividingline | The Dividing Line | Keystone | eclipse.lowsun | Eclipse | Above half your health your blows are of the day: critical on the Marked, and rich in mana. Below half they are of the night: they Curse and they feed you. Each time you cross the line, it bursts. | Cost 5. Above half health: critical damage up 20%, mana from blows up 20%. Below half: every weapon blow lays Cursed and returns 8% of its damage as health. Crossing half health in either direction: a burst round the player, 3.5 m: 16 damage, 25 stagger, Marked going up and Cursed going down (once every 4 s). A line across the middle of the health bar shows it. |
| eclipse.totality | Totality | Signature | eclipse.twilight | Crown | When your surge begins, the sun is covered. While the dark lasts every enemy is Marked and Cursed and every blow you land is critical and feeds you. When the light returns, it strikes all you wounded. | BESPOKE. Pressed: the surge. Seen: a black disc slides over the whole screen's light in half a second: the world goes to near black, with every fighter drawn only in a thin white rim and the floor's edge in a ring of fire like a corona; when it ends the disc slides off and a white flash crosses the screen. Does: for 6 s (or the surge's length if shorter) every enemy within 14 m is Marked and Cursed without the marks being spent; every weapon blow is critical and returns 10% of its damage as health. At the end each enemy the player wounded in the dark takes 20% of that damage again with 30 stagger. Costs nothing more than the surge. |

## Blight (Dark + Poison)

**The idea.** Rot that feeds its owner. The poison's bite comes back as health, the Cursed dead carry the sickness to
the living, and the player can take hold of an enemy and drink it.

**Why want it:** Wasting Grip. **Crown:** The Reaping.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| blight.wither | Wither | Major | dark.curse or poison.venom | Blight | Heavy blows both Curse and poison. Poison bites harder on the Cursed, and does not fade from them. | Cost 3. Heavy blow lands: lay Cursed and 2 doses, whichever element is held. Poison on a Cursed enemy: bites deal 25% more and the doses do not fade while the Curse lasts. |
| blight.feedingrot | Feeding Rot | Major | blight.wither | Blight | Each time poison bites a Cursed enemy, a little of it comes back to you as health. | Cost 3. Poison bites a Cursed enemy: heal 10% of the bite's damage. |
| blight.carrionflies | Carrion Flies | Major | blight.feedingrot | Blight | An enemy that dies Cursed and poisoned lets go a swarm. It finds the nearest living enemy and gives it everything the dead one carried. | Cost 3. An enemy with both dies: a slow homing projectile (a dark swarm, 5 m/s, 10 m reach) goes to the nearest enemy and lays Cursed and all of the dead one's doses. |
| blight.wastinggrip | Wasting Grip | Spell | blight.wither | Blight | Hold the spell button to seize the enemy before you and keep hold. While you hold, its poison bites again and again and every bite feeds you. Let go to throw it at the others. A blow from anyone breaks your grip. 40 mana. | Cost 4. BESPOKE. Pressed: hold Triangle and keep holding, to 2.5 s; release to throw. Seen: the player lunges 2 m and takes the enemy by the throat, lifting an ordinary one off its feet (on a god or elite the hand clamps to the body and it is not lifted); violet-green light runs down the arm into the player in pulses. Does: the enemy is Cursed and held (an ordinary enemy cannot act; a god or elite fights on); every 0.5 s its poison bites and all of that bite returns as health. On release an ordinary enemy is thrown 5 m along the stick: 15 damage and 25 stagger to it and whatever it hits, and they take its doses. Any blow on the player ends it at once; turn-taking does not protect the player here. |
| blight.gravesoil | Grave Soil | Spell | blight.feedingrot | Blight | Hold the spell button to foul the ground round you. On it every enemy is Cursed and poison bites twice as often. 40 mana. | Cost 4. Form: spiked floor, a field. 5 m round the place it was cast, 8 s: enemies standing on it are Cursed, take 1 dose every 2 s, and their poison's beat is doubled. |
| blight.onedeathfeeds | One Death Feeds | Keystone | blight.carrionflies | Blight | When a Cursed enemy dies, the poison in every other enemy bites at once, and you are fed by all of it. | Cost 5. A Cursed enemy dies: an extra bite at once on every poisoned enemy within 12 m, and 20% of that damage returns as health. |
| blight.reaping | The Reaping | Signature | blight.wither | Crown | Your finisher on a Cursed, poisoned enemy reaps the field. All the poison in it and in every enemy near lands at once, and half of it is yours. | BESPOKE. Pressed: the finisher, as always. Seen: the finisher's held pose is drawn out by half a second; behind the player a blade of shadow with a green edge, as long as the room is wide, swings one flat circle across the floor at knee height; every poisoned enemy it crosses bursts green and a thread of light runs from each back to the player. Does: on a finisher against an enemy with both afflictions, every poisoned enemy within 9 m has its poison set off (6 damage and 4 stagger a dose) and half of all that damage returns as health (beyond full, it is lost unless Glut is held). The finisher itself is unchanged. |

## Pyre (Poison + Fire)

**The idea.** Poison is the fuel and fire is the spark. Doses are laid with patience and then lit, and what was a
slow death becomes an explosion.

**Why want it:** Flashpoint. **Crown:** Light the Pyres.

| id | Name | Kind | Grows from | Bough | In-game words | Design |
|---|---|---|---|---|---|---|
| pyre.fuel | Fuel | Major | poison.venom or fire.heat | Pyre | Heavy blows both poison and burn. | Cost 3. Heavy blow lands: lay 2 doses of poison and 1 stack of Burn, whichever element is held (Burn as Fire's: up to five deep). |
| pyre.flashpoint | Goes Up | Major | pyre.fuel | Pyre | When a burning enemy's poison reaches its full depth, it goes up. The poison explodes and sets alight everyone near. | Cost 4. Poison reaches full depth on a burning enemy (or Burn is laid on one already at full depth): set the poison off as an explosion round it, 3 m: 6 damage and 4 stagger a dose, and 2 stacks of Burn on everyone reached. The depth is spent. |
| pyre.funeralfire | Funeral Fire | Major | pyre.flashpoint | Pyre | An enemy that dies poisoned and burning explodes, and leaves the ground burning where it fell. | Cost 3. An enemy with both dies: a burst, 3.5 m: 20 damage, 20 stagger, 2 doses and 1 Burn; then a patch of burning ground 3 m across for 4 s. |
| pyre.fireeater | Fire Eater | Spell | pyre.fuel | Pyre | Hold the spell button to take a mouthful and breathe it out as green fire, for as long as you hold. It poisons and burns all it sweeps across. 40 mana. | Cost 4. Form: wave, held. A cone 5 m long and 50 degrees wide stands while the button is held, to 2 s; the player turns slowly with the stick and cannot move. Every 0.25 s: 4 damage, 1 dose and (every other tick) 1 Burn to all in it. |
| pyre.slowfuse | Lit Fuse | Spell | pyre.flashpoint | Pyre | Hold the spell button to set a fuse on the enemy you face. It burns down while you work. Every dose you lay makes the blast greater, and a perfect guard against that enemy sets it off at once, and harder. 45 mana. | Cost 5. Form: a mark that detonates (small extension: by time, and by a perfect guard). A burning line circles the enemy and shortens over 5 s. At its end: an explosion, 3.5 m: 20 damage plus 6 for every dose laid on that enemy since the fuse was set (80 at most), 40 stagger, 2 Burn on all reached. A perfect guard against that enemy: it goes off at once at one and a half times. |
| pyre.slowburn | Slow Burn | Keystone | pyre.funeralfire | Pyre | Burn and poison keep one time. Both bite together on the beat, and each bites harder for the depth of the other. | Cost 5. Burn stops ticking each second and deals its damage with the poison's beat instead (the same damage over time, in bursts). Each bite of poison is up 6% for every stack of Burn on that enemy (30% at five), and each bite of Burn is up 4% for every dose of poison (32% at eight). |
| pyre.lightthepyres | Light the Pyres | Signature | pyre.fuel | Crown | Hold the spell button and drive your weapon into the ground. A line of fire runs to every poisoned or burning enemy near. Stand through three breaths, and a pillar of flame rises under each. 60 mana. | BESPOKE. A spell, worn in the hold-spell slot. Pressed: hold Triangle and keep holding for 2.2 s. Seen: the player kneels on the planted weapon; thin lines of green fire race along the ground from it to each afflicted enemy within 9 m and follow them as they move; three rings close on the player one after another; on the third, a pillar of green and orange flame as tall as the screen stands under every enemy on a line for one second. Does: each such enemy has its poison and its Burn set off together at one and a half times their usual burst, with 40 stagger; one that dies leaves a pyre that burns the ground for 6 s. The player takes 40% less while kneeling but is not safe: a blow that flinches breaks the lines, and letting go early gives nothing. The mana is spent either way. |

---

## The ten strongest moves

1. **Pawned Heart** (Dark): tear out your heart, fight at one health with enormous strength, and get back to it before the ring closes.
2. **Hanging Swords** (Metal's signature): hold your guard through a whole flurry while swords gather over the attacker's head, and watch them all fall when Temper ends.
3. **Six Mirrors** (Halo's crown): six mirror blades ring the enemy, six lines of light cross it, and the cuts land when you are standing where you began.
4. **The Scales** (Light's signature): the fight is weighed on the floor in two pans of light, and if you gave more than you took a sword the height of the screen falls on everything Marked.
5. **Clean Blade** (Poison's signature): the poison waits for the length of Fever, then the blade is flicked clean and every owed bite lands at once.
6. **A Life for a Life** (Dark's signature): after dying, land a finisher on the one that killed you, live again, and keep the right to rise once more.
7. **Drawn From Stone** (Forge): pull a sword twice your height out of the floor for three slow, unstoppable swings, the last of which breaks it.
8. **The Closed Ring** (Earth's keystone): the surge shuts you and your enemy inside a ring of standing stones until it ends.
9. **One by One** (Poison): a raised hand, a snap of the fingers, and every poisoned enemy bursts in turn down a line, each larger than the last.
10. **Returning Edge** (Metal): throw a great spinning blade and bat it back out with a perfect guard on your own weapon, up to three times, harder each time.

Close behind: Reckoning (Light), Dusk and Dawn (Eclipse), Mountain Cleaver (Earth), Answering Draw (Halo, the Katana),
Hand of the Mountain (Earth's signature), Wasting Grip (Blight).

## Open questions for the user

1. **Critical blows do not exist yet** (Static was rewritten round that). Light, and parts of Metal, Poison, Halo and
   Eclipse, assume one rule: a critical blow deals 1.5 times damage and there is no random chance, only marks that
   say when a blow is critical. Is that the rule?
2. **Dark's surge costs health, not mana**, and the revive is a passive that happens once in a run. Is a surge with no
   mana cost acceptable, and should the Rise end at a rest fire (as written), end when the fight ends, or never end?
3. **A Life for a Life makes the revive repeatable** for a player who can land the finisher. Too generous for "dying
   ends the run", or exactly the exception the user meant?
4. **Immovable removes the roll** and **Edge Guard removes the held guard**. Both are the boldest keystones here and
   both need the gods' red attacks checked against them (the storm's sweep and breath can be braced through or walked
   out of, by the numbers in GAME.md). Keep, soften, or drop?
5. **Dead Already** turns the whole run into the zombie state by choice. Wanted, or is the zombie state meant to stay rare?
6. **Slow Death and Volatile are written as closed to each other.** The tree has no such rule today. Add it, or let
   the player hold both and have Volatile win?
7. **Stone Rise's pillar and Rampart block movement and thrown attacks.** With enemies taking turns and holding a
   ring, walls may need the enemy's path-finding to respect them. Is that work wanted in the first pass, or should
   pillars only block projectiles and bodies they are thrown at to begin with?
8. **Mana costs sit in the spell descriptions**, as Meteor Leap's does. Keep them there or let the tree show the cost?
9. **Mix roots**: each mix's first mark lays both afflictions on heavy blows or chain enders so the branch works for
   either holder. Part A's writer may have chosen another way of giving the second affliction; one rule should win.
   Overgrowth's first mark is written as growing from "Life's root", whose id is in `trees_a.md`.
10. **Only one mark here needs a weapon** (Answering Draw, the Katana). Should each element get a Katana mark of its
    own beside Burning Crescent and Delayed Cuts? Candidates: Earth, a draw that raises a wall of stone along its cut;
    Poison, a draw whose cuts each lay a dose when the blade is home; Dark, a draw paid in health that cannot be guarded.
11. **Folded Steel lasts the run and survives taking a new element.** It is the only mark here that outlives its
    tree. Acceptable?
12. **Player health is assumed to be 100** for every health price and heal in this document. If it is not, those scale.
