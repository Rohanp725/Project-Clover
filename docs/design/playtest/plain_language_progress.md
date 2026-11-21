# Plain-language pass: progress

The owner's rule (2026-11-20): "there needs to be a pass on all language so it's readable for people, a lot of the notes
and whatnot explaining what each thing does is not readable for a normal human, it needs to be readable as if they were a
8th grader".

How the pass works: one file at a time, each committed as soon as it is rewritten, and this list updated in the same
commit. Short sentences, common words, what happens in the order it happens, plain numbers, no code words. Every fact and
number stays the same; no design changes. The "## id", "Title:" and "Group:" lines, headings, bold labels and bullets
are what the Playtest Texts builder reads, so they stay exactly as they were. Text above a file's first "## " line is not
shown in the game and is left alone.

**Playtest Texts needs a rebuild in the editor** (Clover > Build > Playtest Texts) once this pass is done. It has not been
run by this pass.

This file is not read by the game: the builder reads only effects.md, whats_new.md and marks/*.md.

## Unsure lines

Lines whose meaning was not clear. The facts were kept and written as clearly as possible; please check them.

- marks/blight.md, blight.wither: "The numbers" says Phase 4 stopped heavy blows adding a Curse and poison, but the
  Example still says "the heavy blows Curse the soldier as they go". Both kept; one is probably out of date.
- marks/bloom.md, bloom.great: the mark goes in the surge slot, but the Example says "Light Wellspring" (another surge?).
  Kept as written.
- marks/breathtaken.md, breathtaken.short, "Not built yet": "It keeps its own winding from each Breath Taken" was read as
  "it keeps its own timer for how long the enemy is winded".
- marks/burningsap.md, burningsap.brightvenom: "the held bites" was read as "the saved-up poison bites" (the bites
  Burning Sap holds back until its end).
- marks/earthed.md: "the burnt figure" and "the fern" were read as the fern-shaped burn mark Earthed leaves on the
  ground.
- marks/light.md, light.daybreak: the beam "bites every 0.1 seconds" for 12 a bite (48 over 0.4 seconds) and is also
  "six rings' worth" with rings of 8 (also 48); the Empower makes "each ring 10" (60). Both ways of counting were
  kept as written.
- marks/fire.md, fire.sun: "it rises in 1.5 seconds if Kindle has 3 left" was read as "if Kindle has 3 seconds left".

## Files

### 1. Mark details (marks/*.md)

- [x] marks/blackchain.md
- [x] marks/blight.md
- [x] marks/blizzard.md
- [x] marks/bloom.md
- [x] marks/breathtaken.md
- [x] marks/burningsap.md
- [x] marks/burntmound.md
- [x] marks/capsize.md
- [x] marks/carriedembers.md
- [x] marks/caughtlight.md
- [x] marks/chokedamp.md
- [x] marks/coldshort.md
- [x] marks/dark.md
- [x] marks/drownedweight.md
- [x] marks/earth.md
- [x] marks/earthed.md
- [x] marks/earthlight.md
- [x] marks/eclipse.md
- [x] marks/fire.md
- [x] marks/firebloom.md
- [x] marks/fishpoison.md
- [x] marks/forge.md
- [x] marks/frostheave.md
- [x] marks/girdled.md
- [x] marks/glacier.md
- [x] marks/goldenfeathers.md
- [x] marks/halo.md
- [x] marks/ice.md
- [x] marks/illwind.md
- [x] marks/ironthorns.md
- [x] marks/kindlingpoint.md
- [x] marks/life.md
- [x] marks/light.md
- [x] marks/lightning.md
- [x] marks/lightningflower.md
- [x] marks/liveiron.md
- [x] marks/loganstone.md
- [x] marks/metal.md
- [x] marks/mocksuns.md
- [x] marks/nervefire.md
- [x] marks/overgrowth.md
- [x] marks/plasma.md
- [x] marks/poison.md
- [x] marks/pyre.md
- [x] marks/quicksilver.md
- [ ] marks/redhot.md
- [ ] marks/rotroots.md
- [ ] marks/rust.md
- [ ] marks/seized.md
- [ ] marks/shiningherb.md
- [ ] marks/sicklewind.md
- [ ] marks/sleetcharge.md
- [ ] marks/smoulder.md
- [ ] marks/steam.md
- [ ] marks/stolenwarmth.md
- [ ] marks/tempest.md
- [ ] marks/theboltrunstrue.md
- [ ] marks/thegroundgives.md
- [ ] marks/thegrudge.md
- [ ] marks/thermalshock.md
- [ ] marks/turnedtostone.md
- [ ] marks/venomrime.md
- [ ] marks/water.md
- [ ] marks/wind.md
- [ ] marks/windthrow.md
- [ ] marks/winterseed.md

### 2. The guide to effects

- [ ] effects.md

### 3. What's new (older sections only; the 0.5 Phase 4 section is already plain)

- [ ] whats_new.md

### 4. The game guide

- [ ] docs/GUIDE.md

### 5. Other player-facing files

- [ ] readme.md (the READ ME beside the playtest exe; filled in by CombatPlaytestBuild, not by Playtest Texts)
