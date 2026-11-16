# Spell forms: the fixed list of ids

Tree files name a form by id in `Upgrade.Form`; the forms are built by one worker while the trees are written by
others, so the ids are fixed here first. Every form takes the caster's element colour and affliction and a
`SpellSpec` (see `docs/UPGRADE_SCHEMA.md` section 10). The fields each form reads are listed; others are ignored.

Existing: `wave`, `bolt`, `leap`, `rod`.

| id | What the player sees and what it does | SpellSpec fields read |
|---|---|---|
| `blades` | Count blades appear high over the enemy faced (or the aim point), hang for Delay, then fall one after another, each striking a small Radius. | Count, Delay, Radius, Damage, Stagger, Stacks, Reach (how far away it may be aimed) |
| `sword` | One sword as tall as the screen falls on the aim point after Delay; a wide Radius, a held frame on impact, a crack left on the ground for Seconds that bites. | Delay, Radius, Damage, Stagger, Stacks, Seconds, LingerDamage, Reach |
| `beam` | A braced stance, then a thick beam Reach long and Radius wide, held for Seconds, biting every Interval; the player can turn slowly while it lasts. | Reach, Radius (half width), Seconds, Interval, Damage (per bite), Stagger, Stacks |
| `phalanx` | Count blades form in a fan behind the player's shoulders and hover for Seconds; each fires on its own at the nearest enemy within Reach, or all at once when the player lands a heavy blow. | Count, Seconds, Reach, Damage, Stagger, Stacks, Speed |
| `dragon` | A great head of the element forms at the player's shoulder and lunges Reach forward along a line Radius wide, jaws closing at the end for a burst; everything on the line is carried along a little. | Reach, Radius, Damage, Stagger, Stacks, Speed |
| `nova` | A burst round the caster out to Radius after Delay, with a ring that sweeps outward. | Radius, Delay, Damage, Stagger, Stacks |
| `orb` | A slow orb leaves the hand and follows the nearest enemy at Speed for up to Seconds, then detonates in Radius; it also detonates at once if the player strikes it with a weapon blow, larger. | Speed, Seconds, Radius, Damage, Stagger, Stacks |
| `bomb` | The player raises a growing sphere overhead for Delay (rooted), then throws it Reach forward: a great burst of Radius and a burning patch for Seconds. | Delay, Reach, Radius, Damage, Stagger, Stacks, Seconds, LingerDamage |
| `spikes` | Spikes run out along the ground in a line Reach long from the player, one after another at Speed, each erupting a moment after its mark shows; Count rows side by side widen it. | Reach, Speed, Count, Radius (of one spike), Damage, Stagger, Stacks |
| `field` | A patch of ground of Radius at the aim point grows spikes or flame or frost for Seconds, biting every Interval. | Reach, Radius, Seconds, Interval, LingerDamage, Stacks |
| `drill` | The player becomes a spinning point of the element and bores Reach forward through everything on the line, striking each enemy Count times, and comes out the far side facing back. Untouchable while it lasts. | Reach, Radius, Count, Damage (each strike), Stagger, Stacks, Speed |
| `minions` | Count small helpers of the element rise beside the player; each runs at the nearest enemy and strikes it a few times over Seconds, then goes. | Count, Seconds, Damage (each strike), Stagger, Stacks, Interval |
| `bullets` | Count bullets appear in a ring of Radius round the enemy faced, hang for Delay, then all fly inward together. | Count, Radius, Delay, Damage (each), Stagger, Stacks, Reach |
| `reverse` | A point Reach ahead pulls every enemy within Radius toward it for Delay, then bursts. | Reach, Radius, Delay, Damage, Stagger, Stacks |
| `parry` | A guard stance held for Seconds: the first blow that lands on it is turned for nothing and answered with a cone of the element (Reach, Arc). If nothing strikes it, the mana is lost. | Seconds, Reach, Arc, Damage, Stagger, Stacks |
| `wall` | A wall Arc metres wide (read as width, not degrees) rises Reach ahead after Delay, blocks movement and shots for Seconds, then bursts in Radius when it ends or when the player strikes it with a heavy blow. | Reach, Arc (width in metres), Delay, Seconds, Radius, Damage, Stagger, Stacks |
| `discs` | Count discs orbit the player at Radius for Seconds, striking what they pass every Interval. | Count, Radius, Seconds, Interval, Damage (each strike), Stagger, Stacks |
| `brand` | A sign is set on the enemy faced for Seconds. The next Count weapon blows on it each set off a burst of Radius. | Reach, Seconds, Count, Radius, Damage, Stagger, Stacks |
| `rain` | For Seconds, strikes fall at random inside Radius round the aim point, one every Interval, each with its own small mark on the ground first. | Reach, Radius, Seconds, Interval, Damage (each), Stagger, Stacks |
| `clone` | A copy of the player in the element's light stands beside them for Seconds and repeats every weapon blow a beat late for a share of its damage. | Seconds, Damage (read as percent of the blow, e.g. 40), Stacks |
| `flash` | The player vanishes and Count lines of light cross the enemy faced, one every Interval; the player reappears behind it, still, back turned; after Delay all the cuts land at once. Untouchable while it lasts. | Count, Interval, Delay, Damage (each cut), Stagger, Stacks, Reach |
| `ring` | A ring of Radius closes round the player and the enemy faced for Seconds: nothing crosses it; enemies that touch it are bitten. | Radius, Seconds, LingerDamage, Stacks, Reach |
| `pillars` | Count pillars burst from the ground under the nearest enemies within Reach, each lifting what it strikes for a moment. | Count, Reach, Radius, Damage, Stagger, Stacks, Delay |
| `step` | The player steps through the enemy faced (up to Reach) in a line of the element and cuts it in passing; if it lands, the next heavy blow within Seconds is a critical. | Reach, Damage, Stagger, Stacks, Seconds |

A spell the design calls BESPOKE that none of these can carry is given the nearest form for now and listed for
bespoke work later.
