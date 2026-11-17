# The Fire land, second pass

Written by Claude on 2026-11-17 from the user's review of the first pass. The user's words: the land "still needs a lot
of work, especially with level design (we dont [want] flat square areas, we want actually well designed areas) and the
fighting mechanics of the enemies (these need to be creative other than a reskin of other attacks (ie maybe a burning
ash enemy blows up too close to you and damages you?)) and the visuals of the bosses attacks". Their answers to the
first pass's questions are recorded in `WORLD.md` and at the end of section 11 of `fire_land.md`.

Everything below that is not in the user's words is Claude's proposal for the second pass, to be judged by the user.

## 1. What each enemy does that no other enemy does

Each mechanic comes out of the enemy's corruption. Moves already built stay where they are good (the flask snatch, the
wick's hold, the warden's ring, the priest's sun-or-shade sense); what is new is listed.

**The thirsting** (fire as drought).
- *Under the mud.* They lie buried in the cracked riverbed as low mounds whose cracks breathe dust. Walking within a few
  metres makes them burst up in a spray of dust and lunge for the player's belt. The mound is the tell, for a player
  who learns to read the ground.
- *They drink the dead.* A thirsting near a fallen enemy, a broken water jar or a spilled flask goes to it and drinks,
  healing and swelling. Water jars stand about the river and the city; breaking one is bait.
- Death: they crumble into a puff of dust and leave nothing.

**The wick** (they burn and are not consumed).
- *The flame spreads.* A burning wick that touches a dormant one sets it alight and wakes it. A burning wick leaves a
  short trail of burning footprints. Fire from any source (a burst, a brazier knocked over) wakes dormant wicks.
- **The ashen** (new, the user's example). A wick burned down to its last: a shell of grey ash round a white-hot core.
  It comes at the player at a stumbling run. Close in, it stops and swells: the core brightens, the cracks open, it
  hisses. Then it bursts in a ring of burning ash that hurts everything in it, other enemies too, and lights any wick
  it reaches. Killed before it bursts, it bursts where it stands after a short delay, so a player can kill it beside
  other enemies on purpose. Built on the wick's model with an ash material and a glowing core.

**The gilded** (the promised golden flesh came true as molten gold).
- *The gold runs.* Every blow that lands on a gilded splashes molten gold on the floor beside them; it cools in a few
  seconds into a bright puddle that burns while hot and slows while it cools. A long fight with a gilded fills the
  floor with gold.
- *The levy's shield wall.* Levies near each other lock shields into a line and advance in step, spears thrusting
  over the rim. The wall cannot be broken from the front by light blows; flank it, or break it with a heavy blow to
  one shield, which opens the line.
- *The spearman's leap.* From a ledge or a stair he leaps down at the player with the spear point first; on level
  ground he throws the spear, which stands glowing in the floor until he walks to it.
- *The warden's pour.* Beside the ring he already has, he tears gold from his own body and pours an arc of it on the
  floor, a low wall of molten gold that blocks the way for some seconds.

**The sun-blind priest** (they looked at their god until their eyes burned out).
- *The mirror.* Priests carry polished discs. A priest that has found the player throws a beam of sunlight from the
  disc along a line: it burns, and where it falls the ground counts as lit, so it can drive the player out of shade.
- *The calling.* A priest that has found the player raises the disc and lights the player up; nearby gilded turn
  towards the light and come.
- In the hall of columns, mirrors on stands can be turned by the player to throw a beam across the hall: to burn
  enemies, to light a door that opens to the sun, or to find a hidden way.

## 2. The heat of noon (the user's idea)

Out under the sky the sun swells at times. A swell is announced for about five seconds: the sky and the ground
whiten, a sun mark on the HUD fills, and the shade's edges sharpen. Then for about fifteen seconds anyone standing in
the sun burns, a little at a time. The player must reach shade: awnings, colonnades, the shadows walls cast, the
insides of houses and tombs. The corrupted feed on it: wicks flare and move faster, the gilded's gold runs from them
on its own, the thirsting go wild for water. Swells come on a rhythm per district, rarer in the first district and
not at all in the elite's fight, so the god's court is where the rule is pushed hardest.

The levels are drawn with this in mind: every open stretch has shade islands along it, and some fights are placed so
that a swell comes while the player is in the open.

## 3. Embers after a fall (the user's decision)

Unspent embers are lost on a fall. The enemies the player killed since the last rest come back carrying them: each
shines with an ember mote, and killing it again gives its share back. A second fall before they are all reclaimed
loses what is left for good. The HUD shows how many are waiting.

## 4. Level design

The first pass laid flat yards and scattered the kit on them. The second pass builds places:

- **Height.** Three or four levels in each district: the riverbed sunk below its banks, terraces of houses, stairs and
  ramps, roofs that can be walked, a sunken tomb. From the fixed camera height reads by cast shadow, inked edges and
  stairs, so drops and walls must be big enough to read.
- **Routes.** A main way that winds and branches, with side paths. Each district loops back to its rest fire by a
  shortcut opened from the far side: a barred door, a ladder kicked down, a fallen wall.
- **Rooms shaped for their enemies.** A long ramp for the shield wall; a narrow alley lined with oil jars for the ashen;
  the buried thirsting in open mud near a water jar; the priests in a hall of mirrors and columns; a spearman on a
  ledge over a stair.
- **Sight lines and landmarks.** The barque, the pylon, the obelisk and the colossus seen from far off, drawing the
  player on; doors and stairs framed so the next goal reads.
- **No rectangles.** Outlines that wander, ground that rises and falls, edges that crumble.

The detailed plan is in `fire_levels.md`.

## 5. The bosses' attacks

Every attack of the Champion, Khepharu and the Sun-Bearer gets an effect of its own that shows what it is: a thrust
leaves a streak of gold light, the thrown spear trails fire and stands smoking, the ray is a beam of real light, the
sun rolling scorches a furrow that smokes, the falcon's dive tears the air, the Zenith whitens the whole court and
heat shimmers off it. Ground marks stay as the readable warning; the effects are the attack itself.

**The parry made for the sun** (the user's maybe). A perfect guard against the sun does not only knock it loose: the
player strikes it back like a ball, a brief slowing of time and a flash, and it flies into the form and stuns it. Built
as a proposal for the user to judge.

**The Sun-Spear.** Take gives the god's weapon, so Fire comes with the Sun-Spear: the Great Spear's moves with the
king's spear as its model and one move of its own, the throw that stands in the floor and is called back with a ray.
