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
