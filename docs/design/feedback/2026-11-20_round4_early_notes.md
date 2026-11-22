# Notes for the fourth round of effects comments (held for the weekend)

On the evening of 2026-11-20 the owner said there will be another round of comments and fixes on the reaction visuals on Saturday night or Sunday. They asked that nothing be fixed before then. These notes came in early and belong to that round.

## Chill (Ice's base affliction)

The owner said, word for word: "the ice fix didnt happen yet, the crystals poking out the back of the enemy still dont look attached to the enemy, as a matter of fact it shouldnt even have those crystals in the base effect, just the reactions like the rime one."

- Remove the crystals from Chill's base look. Chill keeps only its frost or crust on the body.
- Crystals growing from the back belong to reactions such as Rime, and only there.
- Where a reaction keeps crystals, they must sit on the body, not float off it. The worker's close-up check on 2026-11-20 said Chill's crust was "on the body", but the owner still saw floating back crystals.

## Questions for that round (from the Unity worker, 2026-11-20 night)

- **The finish mark** (`WorldBars.FinishMark`) is the "finish it" prompt over a staggered enemy: a gold broken ring around a cross. GAME.md records it as a decided cue, so its ring was kept. Its white was toned down to pale gold, and the stagger ring that widens on the enemy was made gold instead of blue-white (21319c7). Should the ring go, or be tinted per element?
- **The white cut line across a struck enemy** (`m_cut_white`) is still near-white. Keep it, or tint it?

## Idea saved by the owner (2026-11-21)

- **Poison + Life reaction:** green roots growing out of the enemy's body. This came from the Poison infused-strike veins, which the owner said looked "almost like green roots growing out of them, which actually could be a cool visual concept for the posion life reaction save that idea". Consider it for the Poison + Life reaction look in the weekend round.

## Effects on small and large enemies (the owner, 2026-11-21)

The owner said, word for word: "I have noticed some effects look misplaced on smaller enemies compared to bigger ones and vice versa, so we need to come up with a solution that will work".

**The solution the owner agreed to:**
- **Anchors on every enemy:** feet, waist, chest, head, back and hands, plus a body size (height and girth). Measure them from each enemy's real mesh and bones when the enemy is built (EnemyBuilder), not from fixed numbers. The existing BodyGirth fit and the short-enemy chest-height fix (ee4e0df) are folded into this.
- **Every on-body effect piece is placed by an anchor,** relative to it, instead of at a fixed height. This covers afflictions, reaction pieces, strike pieces (hands, veins, daggers), flames on bodies, and ice crust.
- **Size:** on-body pieces scale with the body, between about 0.6x and 1.8x. Floor and area effects keep their gameplay size, because their size shows reach. (Owner's pick: "Scale on-body, not areas".)
- **The check:** a probe films a chosen set of effects side by side on a small enemy (the Knife Acolyte), a medium one (the Temple Soldier) and a large one (the Gate Captain, plus a boss form). Read the close-ups for every effect touched. This becomes the standing size check for every new effect.
- **When:** in the weekend effects round, before its re-films, so every re-filmed look already sits right on every size.

## The filming rig (the owner, 2026-11-22)

The owner said, word for word: "for some reactions, the enemies are getting hit by something, even after the effect, also a lot of the gifs still have the issue of a second cast of both element afflictions on the enemy, even when it cant react cause the duo reaction just happened. We might need to update how we are taking the gifs".

**Plan: rebuild the reaction filming rig (v2) before any round-4 re-films.**
- **Fresh bodies for every clip.** Fresh enemies, with every affliction, hazard and look cleared. Nothing carries over between clips.
- **One trigger per clip.** Lay the first element once, then cast the second element ONCE with the real spell, which sets off the reaction. No re-casting, no player attacks and no auto-blows after it.
- **Quiet enemies.** Enemies can't attack or cast during the clip; no Storm Caller bolts. They still react: flinch, stagger, knock-down.
- **No leftovers.** No hazards or ground effects left from earlier clips.
- **The rig checks itself, and fails the clip if:**
  - any affliction is laid after the reaction fires, apart from what the reaction itself lays;
  - any damage lands after the effect ends, apart from the reaction's own damage over time;
  - the second element's own affliction look shows on a body that just reacted.

  These checks come from logging every affliction, hit and look per frame.
- **Re-film only after the rig passes on a sample of 5 reactions,** with the frames read by eye.
