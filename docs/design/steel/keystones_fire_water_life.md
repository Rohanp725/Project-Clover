# New keystones for Fire, Water and Life

Written 2026-11-20 after the owner's decision that trees may have three keystones ("fire water and life might need to be
researched and have more keystones"). Earth, Metal, Light and Poison have three; Fire (White Heat), Water (Deep Well)
and Life (Wildwood) have one. Each needs two more. Below are **three candidates per element, so the owner picks two**,
best one for each open bough. Everything is a **Proposal**; every number is a first guess.

## What makes a keystone (the bar the other trees set)

- **It rewrites a rule for the whole kit**, not one moment. Single Stroke changes what Static is for; The Slow Hand
  changes every swing; Volatile turns the poison's beat off. A major adds an answer to a moment (a perfect guard, a
  death); a keystone changes what the player does all fight.
- **The price is felt in every fight** and is in the element's own terms: White Heat burns you when struck, Never There
  halves the guard, Slow Death weakens the weapon, Wildwood weakens you far from the garden. The best prices shrink as
  skill grows (the Slay the Spire lesson in `depth_elements.md`).
- **It stays in the element's substance and timing** (`elements_a.md`, "Masters"): Fire on what is burning, Water on
  the return, Life on or beside what is grown.
- **It is seen from above** without words.
- **It does not repeat** its own tree's Devotion, its other keystones, its Heresies, or another tree's keystone.

## The open places

Each tree has 36 places for 34 marks (`BranchPattern`), and the two empty places are exactly where the new keystones
go, so each bough ends in one keystone. Every road below costs 14 Shards (root 2, two majors 3 + 3, keystone 6), under
the 22 the chart test allows.

| Element | Keystone now | Open place 1 | Open place 2 | Must not repeat |
|---|---|---|---|---|
| Fire | White Heat (Pressure bough, trunk) | Answer bough's crown (`L.crown`), from Flame Trail | Ruin bough's crown (`R.crown`), from Catching Fire | Fire Without End; Given Fire, Kneel to the Sun; Catching Fire |
| Water | Deep Well (Vessel bough's crown) | Current bough, the trunk (`T.1`), from Spreading Water | Tide bough's crown (`L.crown`), from Heavy Water | The Sea Comes In; The Tide Comes Back, Water Takes the Blow; Spreading Water |
| Life | Wildwood (Harvest bough's crown) | Garden bough, the trunk (`T.1`), from Root and Branch | Bramble bough's crown (`R.crown`), from Briar Coat | Wood Grows Back; Grown Through Them, The Club's Two Ends |

A keystone can sit in either open place of its tree if its line stays clear; the places given are where each fits
best.

---

## Fire: pressure, burning, heat

| | **Into the Fire** | **The Blade Drinks Fire** | **Creeping Fire** |
|---|---|---|---|
| Rule | A dodge toward or through the enemy faced passes through bodies, is safe for its whole length, and sets everything it passes burning two deep; a perfect one adds a level of Heat. A dodge away from it costs twice the stamina and is never perfect. | Your weapon no longer sets burns: it drinks them. Each blow on a burning enemy takes one Burn off it and strikes 6 harder for it (a heavy takes three, a finisher all). Burns no longer bite by themselves. | Fire you leave on the ground creeps: as each patch dies it lights a new one 1.5 m toward the nearest enemy within 6 m, three times over. Your own fire burns you as it burns them. |
| Price | Backing off is dear and has no perfect window, so red attacks and great blows must be met by going through them. | Cinder's burn on every blow is off, and a burn left alone does nothing. The fire must come from spells, the ground, the strikes and reactions. | You cannot stand in your own fire any more; the floor you light closes on you too and narrows where you can fight. |
| Why a keystone | It rewrites the dodge and the whole of the player's footwork: Fire stops retreating. A major would add fire to a dodge; this decides where you may dodge. | Burn stops being damage over time and becomes the blade's fuel: spells light, steel spends. Every set-off in the tree (Breath of the Lioness, Black Pitch, Catching Fire) now competes with the blade for the same fuel. With Fire Without End the room cooks up fuel the blade comes back for. | Every flame laid (Flame Wave's patches, Running Fire, Flame Trail, the crater, the Firebomb, the charged blade's third blow, Fuel to It) becomes a front that hunts, and the player must manage their own fire's spread. |
| On screen | The roll becomes a lunge through a body in a burst of flame and embers; the enemy passed catches in two rings of flame. A dodge back trails a thin drag of smoke and the stamina bar flashes. | At each blow the enemy's flame is drawn into the edge: the blade flares orange and sheds embers, and one ring of flame on the enemy goes out. A heavy blow pulls a rope of flame off it. | Tongues of flame crawl a step at a time along the floor toward the enemies, a black char line behind each. Standing in your own fire, your legs smoke and the health bar takes small bites. |
| Not the same as | Never There (Lightning) makes every dodge better; this makes one way better and the other worse. Wildfire Rush is one spell; this is every dodge. | Single Stroke banks Static for one discharge; this spends on every blow. Given Fire spends the Burn on you. | Catching Fire spreads by a death, onto bodies; Fire Without End deepens burns on bodies. This is the floor. The cut Scorched Earth was its opposite (safe in your own fire). |
| Engine has | `DodgeStart`, `DodgeEnd`, `PerfectDodge`; `Afflict` with a radius; `DodgeStamina`, `DodgeSafeTime`; Wildfire Rush's step already passes bodies; Heat filled by hand as Kindle fills it. | `AnyHit`, `HeavyHit`, `FinisherHit` with `Needs = Burn`; `Spend` with `Count` and `DamagePerSpent` on a small `Burst` at the target (the stand-in for the blow's own damage). | `Hazard` and every ground-fire source; `StatWhen.InOwnFire` with `DamageTaken` (stand-in price: 30% more taken in your own fire); `GroundSeconds`. |
| Missing | `dodge-direction`: a dodge tested as toward or away from the enemy faced. `dodge-through`: the plain dodge passing bodies and laying on each. A Heat level given from data (small). | `root-silenced`: a switch that stops the root laying on blows. `burn-bite-off`: Burn does no damage over time, as `Afflictions.PoisonBeatOff` is for poison (small). | `ground-creeps`: a hazard that at its end lays its successor toward an enemy, with a generation count and a cap on patches standing. `afflictions-on-player` (ledger) for the full price, a Burn on you. |
| Place | Answer bough's crown, from Flame Trail. | Ruin bough's crown, from Catching Fire (or the Answer crown). | Ruin bough's crown, from Catching Fire. |

**Recommended: Into the Fire and The Blade Drinks Fire.** The three boughs then end in three different answers to
"never let up": build Heat (White Heat), go through them (Into the Fire), eat the fire you lit (The Blade Drinks Fire).
The second is mostly data and two switches. Creeping Fire looks the most like fire, but it needs the most new code,
spreading ground is a cost on the frame, and its price is the hardest to read.

---

## Water: flow, soak, ebb and tide

| | **High Water, Low Water** | **Drawn Back to Break** | **Spring Tide** (re-aimed) |
|---|---|---|---|
| Rule | The tide turns every 6 s. At high water your spells strike a third harder and your weapon a fifth softer; at low water it is the other way round. A perfect guard or a perfect dodge turns it at once. | Each dodge that takes you away from the enemy faced draws the sea back with you, one wave, three at most. Your next weapon blow that lands breaks them all on it: a 4 m arc, 12 damage and 20 stagger a wave, Soaked, thrown 2 m away. With no wave drawn, your blows strike a fifth softer. | Everything you pay for in mana (the larger spell, the surge, the deep strike, a duo surge) takes all the mana you hold, and is as great as what it takes: at a full bar, twice as strong and half again as wide. |
| Price | For half of every fight, half your kit is weak. A player who will not switch between spells and blade loses more than they gain; a skilled one turns the tide to suit, so the price falls as skill rises. | Standing and trading blows is weak. The player must keep going out and coming back in, and a dodge forward draws nothing. | Nothing paid in mana can follow another, and a cast at its bare cost is plain. Out of a fight, mana goes out with the tide to nothing instead of settling to half. |
| Why a keystone | The whole fight takes a rhythm of two halves: pillar 2's weave written as Water's tide. Ebb and Flood, at this bough's foot, gives short boons at a moment; this is the clock they ride on. | A footwork rule for the whole fight, with the return as the way every exchange is made. It pushes, where the tree already has eight pulls (`review_elements.md`). It is Into the Fire's mirror: Fire never steps back, Water always does. | Mana stops being a budget and becomes one wave the player chooses when to break. Every mana-paid thing in the Hand changes its timing. |
| On screen | A ring of water round the player's feet rises over the ankles at high water and drains to wet sand and foam at low, with a rush and a hiss as it turns. At high water the spell diamond's rim runs with water; at low the film of water on the blade thickens. | Water streams off the floor after the player as they dodge back and curls into a wave behind the shoulders, one, two, three. The blow throws them forward as one white-lipped breaker that spills foam over the floor. | The mana bar is drawn as a tide line. A full cast swells visibly (Poured Blade's copy a head taller, The Sea Rises' flood wider), and the bar drains with the sound of water going out over shingle. |
| Not the same as | Full Daylight (Light's Devotion) is a rhythm per enemy; the poison's beat is the enemy's clock. This is the player's. | The Tide Turns is one ebb and flood on a button; this makes every dodge an ebb. | Drawn From the Cloud (a Lightning Heresy) takes mana from the enemies' Static; this changes how mana is spent. |
| Engine has | `StatMod` on `SpellDamage` and `WeaponDamage`; `PerfectGuard`, `PerfectDodge`. | `DodgeEnd`; a mark's count (`Bank`, `Tally` capped at 3); `AnyHit` with `SpendsAll`, then `Cone` and `Push` with `PerStack`; `WeaponDamage`. | `ManaPool`; `SpellSpec.Cost`; `RuleBook.Mana` (a `ManaMode`, not yet read by anything). |
| Missing | `tide-clock`: a player clock of two halves read as two `StatWhen`s, an action that turns it, and its ring and HUD sign. | `dodge-direction` (shared with Into the Fire). `while-count`: a `StatWhen` that holds while a mark's count is empty (small). | `spend-all-mana`: a `ManaMode` in which a mana cast takes the whole bar and passes the share over its cost to the form's damage and size; the settle-to-half switched off. |
| Place | Current bough, the trunk, from Spreading Water. | Tide bough's crown, from Heavy Water. | Current bough, the trunk, in place of High Water, Low Water (the place `phase4_questions.md` Water 3 offered it). |

**Recommended: High Water, Low Water and Drawn Back to Break.** One per open bough, both are Water's own timing, and
neither adds a pull. Take Spring Tide instead of the first if the owner wants a mana build in the tree. The weapon's
skill is left out of Spring Tide on purpose, so that an element keystone does not rewrite the Way.

---

## Life: growth, mending, the garden

| | **Shared Roots** (returned) | **In Its Season** | **Fed to the Grove** |
|---|---|---|---|
| Rule | A third of every wound you take is carried instead by the Seeded enemies within 8 m, split among them. Your seeds no longer mend you. | A seed left alone ripens: one that has lain 6 s with no fresh seed laid on it flowers, and stops fading. A ripe seed's burst (Ripening, Reap What Is Sown, its death, a reaction) is three times as strong and leaves a thornbush; an unripe seed's burst is half as strong. | Nothing mends you but your flasks. All the mending your seeds, spells, marks and reactions would give goes to your garden instead: each 10 points makes everything you have grown within 12 m strike a tenth harder and stand 2 s longer, for 8 s, up to twice as hard. |
| Price | The root's mending, Life's steady way of staying alive, is gone; with nothing Seeded near, nothing is shared. | Harvesting early is poor (Ripening's heavy burst and Reap on fresh seeds lose half), and the enemy you keep cutting never ripens, because each blow sows it again. The garden must be planted ahead and the blade turned on another while it grows. | Life gives up every heal but the flask: Elder Tree's ring, Root and Branch, Reap's mending, Drawn From the Wood's, the seed's own. |
| Why a keystone | Life's defence changes from mending yourself to making the sown bleed for you, as mistletoe drinks from its host. You seed a crowd to wear it as armour, and wading in becomes safer than standing back. | It sets the timing of the whole Garden kit: sow, leave, harvest. Every burst in the tree now looks for a ripe enemy, and the order you strike enemies in changes. | One system turned into another, Path of Exile's kind: the tree that heals most becomes a garden that fights. It answers the review's count of eleven mends in one tree. |
| On screen | Thin roots run over the floor from the player's heels to every Seeded enemy. When the player is struck a pulse runs down them, and each enemy jerks as bark splinters off it and leaves fall. | The seed is a green bud on the enemy's shoulder that swells for 6 s, then opens as a red and yellow flower (petals, not light). A ripe burst throws a heavy spray of thorns and petals, and a bush heaves up. | Where a heal would show, thin green sap runs from the player into the ground toward the nearest plants, which thicken, darken and flower. |
| Not the same as | Hidden in the Tree (Ice and Life) turns blows near what is grown; this passes the wound to the enemy. | Fire Without End rewards leaving a burn alone by deepening it; this ripens once and changes the payoff. Grown Through Them deepens the seed to root the enemy, not to burst it. | The Club's Two Ends heals on heavy blows only; this heals nothing and feeds the garden. |
| Engine has | `Struck`; `Arc` with `FromTarget` and `LeapsTo = Seeded` (a stand-in for the split); `DamageTaken`. | `Condition` (its time, `ConditionEnded`, `Keeps = Seeded` while it runs); the "left alone" clock in `AfflictionRules` that Fire Without End uses; `SetOff`, `ConditionDied`, `BurstGrown`; Fallen Seeds' thornbush. | `Boon` on `GrownDamage` and `GrownSeconds`; flasks untouched. |
| Missing | `wounds-shared`: a share of each wound taken off the player and dealt, split, to the afflicted enemies near (close to the ledger's `links-pass-blows`). `seed-mending-off`: a switch for the root's mending (small; Grown Through Them needs it too). | `needs-condition`: an effect that asks whether the target carries a named condition, to scale a burst up or down; and the ripe condition set by the left-alone clock. | `heal-to-garden`: a `HealthMode` in the RuleBook that turns marks' healing into a count, and the moment that count pays out. |
| Place | Bramble bough's crown, from Briar Coat. | Garden bough, the trunk, from Root and Branch. | Garden bough, the trunk, in place of In Its Season (or the Bramble crown). |

**Recommended: Shared Roots and In Its Season.** Shared Roots was never broken, only cut so the tree would keep two
keystones; it comes back as written in `trees_a.md`. Take Fed to the Grove instead of In Its Season if the owner wants
Life to be able to give up healing altogether. Shared Roots and Fed to the Grove both take the seed's mending, so as a
pair they cost less than they look.

---

## The retired keystones, and what came of them

| Keystone | What it did | Why it was cut | Here |
|---|---|---|---|
| Spring Tide (Water) | Every spell spent all the mana held and was as great as what it spent. | Basic spells moved to cooldowns, so "every spell" stopped meaning anything. | Re-aimed at what mana still pays for. |
| River Without End (Water) | The light chain looped with no recovery, quicker each time round, every blow softer. | Cut for the count; and the Katana's Stream aspect is now that loop. It belongs to the Way. | Not offered. |
| Heartwood (Life) | No mana; everything paid in health; mana gains healed you. | It rested on spells costing mana. Health as a price is Dark's now. | Not offered. |
| Paid in Blood (Life, a major) | A spell you could not pay for was paid in health. | The same; Dark's Blood Price does it now. | Not offered. |
| Shared Roots (Life) | As above. | Only the count. | Returned. |
| Open Furnace (Fire) | A perfect guard set off every burning enemy near you. | Cut for the count; its set-off now lives in Breath of the Lioness. | Not offered. |
| Scorched Earth (Fire) | Your ground fire lasted twice as long, and standing in it kept Kindle lit and gave more mana; spells cost more. | Its price stopped meaning anything once spells were on cooldowns. | Creeping Fire is its opposite. |

## The engine pieces the candidates need

Every candidate can be built today from existing triggers and actions plus the pieces below. None needs a new spell
form.

| Piece | Kind of work | For |
|---|---|---|
| `dodge-direction` | A test on the dodge: toward, or away from, the enemy faced | Into the Fire, Drawn Back to Break (one piece buys both) |
| `dodge-through` | The plain dodge passing bodies, from Wildfire Rush's step | Into the Fire |
| Heat from data | An action that raises Heat a level (small) | Into the Fire |
| `root-silenced` | A RuleBook switch: the root lays nothing on blows (small) | The Blade Drinks Fire |
| `burn-bite-off` | An affliction rule: Burn does no damage over time, like `PoisonBeatOff` (small) | The Blade Drinks Fire |
| `ground-creeps` | A hazard that lays its successor toward an enemy, capped | Creeping Fire |
| `afflictions-on-player` | Already in the ledger | Creeping Fire's full price |
| `tide-clock` | A player clock of two halves, two `StatWhen`s, a turn action, the feet ring and HUD | High Water, Low Water |
| `while-count` | A `StatWhen` for a mark's count being empty (small) | Drawn Back to Break |
| `spend-all-mana` | A `ManaMode` that takes the whole bar and scales the form | Spring Tide |
| `wounds-shared` | A share of each wound passed, split, to afflicted enemies near | Shared Roots |
| `seed-mending-off` | A switch for the root's mending (small; Grown Through Them needs it too) | Shared Roots, Fed to the Grove |
| `needs-condition` | An effect that asks whether the target carries a named condition | In Its Season |
| `heal-to-garden` | A `HealthMode` that turns healing into a count that pays out | Fed to the Grove |

Until each piece is built, a candidate can sit on the tree with its nearest working version and a **Not built yet**
line, as the other marks do.

## For the owner

1. **Fire**: two of Into the Fire, The Blade Drinks Fire, Creeping Fire. (Claude's pick: the first two.)
2. **Water**: two of High Water, Low Water, Drawn Back to Break, Spring Tide. (Claude's pick: the first two.)
3. **Life**: two of Shared Roots, In Its Season, Fed to the Grove. (Claude's pick: the first two.)
4. Any name to change. Shared Roots and Spring Tide keep their old names but need new ids (`life.sharedroots`,
   `water.springtide`): `RetiredMarks` already refunded `life.roots` and `water.spring`, and the chart test holds that a
   retired id is never used again.
