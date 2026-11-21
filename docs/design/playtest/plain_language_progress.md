# Plain-language pass: progress

**Summary (pass finished 2026-11-21).** All 70 files in scope are done: the 66 mark detail files, effects.md, the older
sections of whats_new.md (0.1 to 0.4; the 0.5 section was left as it was), docs/GUIDE.md, and the playtest readme.md.
Ids, "Title:" and "Group:" lines, headings and bold labels are unchanged (checked against the files before the pass).
Thirteen lines were unclear or looked out of date; they are listed under "Unsure lines" below, with the facts kept as
written. **Playtest Texts still needs a rebuild in the editor (Clover > Build > Playtest Texts)**; this pass did not
run it. GUIDE.md's generated "Every mark" tables were left alone (see the note on GUIDE.md below).

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

## Sync with the stand-in passes (2026-11-21)

Marks whose behaviour changed in code after the pass (list: the stand-in run's "Mark details to update"). Each file is
checked against its code and committed on its own.

- [x] marks/nervefire.md: Five Thunders (exact fifth bite on each enemy), Nerve Fire shapes (elite flinch wait per elite)
- [x] marks/quicksilver.md: shapes (half-second bite wait per enemy; Running Silver passes every dose and notch),
  Cinnabar (notches kept while poisoned)
- [x] marks/sleetcharge.md: Glass Storm (every other pass a Static, on each enemy), Glass Spear (frozen 1.5 s)
- [x] marks/thegrudge.md: Ash and Thunder (every third blow on each Cursed enemy), Long Memory (Curse kept while charged)
- [x] marks/theboltrunstrue.md: The Diamond Falls (every third critical on each Marked enemy)
- [x] marks/liveiron.md: Three Smiths (every third blow on each notched, charged enemy)
- [x] marks/loganstone.md: The Prophecy Broken (every fourth Logan Stone on each elite; no 6-second wait)
- [x] effects.md: plain Nerve Fire (elite flinch wait per elite), plain Quicksilver (bite wait per enemy), plain Stolen
  Warmth (8 health a second from each enemy), Lightning's Flash Cut (nothing can hit you through the lunge)
- [x] marks/burningsap.md: Bared to the Sun (once on each enemy in each cast)
- [x] marks/frostheave.md: Permafrost (Tremor kept while chilled), Winter in the Bones (shaken freeze twice as long)
- [x] marks/thermalshock.md: Neither Wins (Burn and Chill each kept while the other is on)
- [x] marks/blackchain.md: Rust in the Curse (notches kept while Cursed)
- [x] marks/rust.md: Brine (Soaked twice as long on a notched enemy), Rust's shapes (notches up to five)
- [x] effects.md: plain Rust (notches up to five, plate falls at five)
- [x] marks/metal.md: Poured Bronze (held 0.4 s a notch), Circle of Steel (half the wait when nothing hits)
- [x] marks/dark.md: Gone to Ash (half the wait when nothing hits)
- [x] marks/girdled.md: Death Turned Back (half the wait when no blow comes)
- [x] marks/mocksuns.md: Sunstone (half the wait when nothing hits), Wide Fan (unsure line resolved)
- [x] marks/turnedtostone.md: Stone Does Not Forgive (Tremor and Curse kept for good), Heavy Curse (unsure line resolved)
- [x] marks/earth.md: Cairn (15% slower for each stone)
- [x] marks/light.md: Reckoning (8 for each blow, the owner's decided number), Daybreak (unsure line resolved)
- [x] marks/redhot.md: Red Hot's shapes (Burn bites twice as hard while it glows)
- [x] effects.md: plain Red Hot (Burn bites twice as hard while it glows)
- [x] marks/venomrime.md: Cold Damage (bites twice as hard while frozen; doses go off on the shatter), Six Levels Down (0.3 s more freeze a bite, wait per enemy)
- [x] marks/smoulder.md: Night Sun (heals 1 for each Burn)
- [x] effects.md: Night Sun, Glass Spear and Cold Damage in the reaction shapes
- [x] marks/blight.md: Wither (unsure line resolved)
- [x] marks/bloom.md: Great Lotus (unsure line resolved)
- [x] marks/burningsap.md: Bright Venom (built now: held bites 15% harder; unsure line resolved)

## Unsure lines

Lines whose meaning was not clear. The facts were kept and written as clearly as possible; please check them.

- marks/blight.md, blight.wither: "The numbers" says Phase 4 stopped heavy blows adding a Curse and poison, but the
  Example still says "the heavy blows Curse the soldier as they go". Both kept; one is probably out of date.
  **Resolved:** the code gives Wither no blows that add afflictions (only the 25% and the kept doses), so the
  leftover lines about heavy blows were cut and the Example now Curses with Grasp or the second strike.
- marks/bloom.md, bloom.great: the mark goes in the surge slot, but the Example says "Light Wellspring" (another surge?).
  Kept as written. **Resolved:** the Hand has one surge slot (GAME.md, 2026-11-21), so Wellspring and the Great Lotus
  can't be lit together; the Example now lights the Great Lotus, and Wellspring left its "Works with".
- marks/breathtaken.md, breathtaken.short, "Not built yet": "It keeps its own winding from each Breath Taken" was read as
  "it keeps its own timer for how long the enemy is winded". **Resolved:** the code confirms it (its own 1.5-second
  winding, set at each Breath Taken).
- marks/burningsap.md, burningsap.brightvenom: "the held bites" was read as "the saved-up poison bites" (the bites
  Burning Sap holds back until its end). **Resolved:** the code confirms it (the held bites); the mark is also built
  now (15% harder), so its "one extra dose" stand-in and "Not built yet" were replaced.
- marks/earthed.md: "the burnt figure" and "the fern" were read as the fern-shaped burn mark Earthed leaves on the
  ground.
- marks/light.md, light.daybreak: the beam "bites every 0.1 seconds" for 12 a bite (48 over 0.4 seconds) and is also
  "six rings' worth" with rings of 8 (also 48); the Empower makes "each ring 10" (60). Both ways of counting were
  kept as written. **Resolved:** the code fires four hits of 12 (48), and the Empower makes each hit 15 (60); the
  numbers now count hits, and six rings of 8 is named only as the plan not built yet.
- marks/mocksuns.md, mocksuns.widefan: "What it does" says the Prism fan reaches 8 metres, "The numbers" says 7 metres
  beyond the enemy. Both kept. **Resolved:** the code puts five hits in a line from 1 to 7 metres past the enemy;
  the 8-metre fan of rays is written as not built yet.
- marks/turnedtostone.md, turnedtostone.heavycurse: says the stone climbs "in half a second, not one", but the shape's
  numbers say the enemy slows to a stop "over 2 seconds". Both kept. **Resolved:** the climb is 1 second at 30% slower,
  then 1 second at 60% slower; Heavy Curse makes the first part half a second, so 1.5 seconds in all.
- effects.md, two-elements: "(the owner's decision of 2026-11-19)" is a note to the team, not to players. Kept as
  "the owner decided this on 2026-11-19"; it could probably be cut.
- docs/GUIDE.md, sections 4 to 6: these look out of date next to section 1 and the guide to effects (they still
  give tap spells mana costs and say to cast with Triangle / U, and section 6's spell forms table still lists form
  ids). The facts were kept as written; the form ids in the table were kept too, since they are names, and the
  column was renamed "Form". These sections probably need a content update, not only plain words.
- marks/fire.md, fire.sun: "it rises in 1.5 seconds if Kindle has 3 left" was read as "if Kindle has 3 seconds left".
- readme.md, "NOT BUILT YET": it still lists reactions, the spell branches and "written details for every tree but
  Fire and Lightning" as coming later, though all three are in now. Kept as written; it needs a content update.
- docs/GUIDE.md, section 8: editor menu names (Clover > Build > ...) were kept, since they are what you click; scene
  file paths became scene names ("the Hub scene").

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
- [x] marks/redhot.md
- [x] marks/rotroots.md
- [x] marks/rust.md
- [x] marks/seized.md
- [x] marks/shiningherb.md
- [x] marks/sicklewind.md
- [x] marks/sleetcharge.md
- [x] marks/smoulder.md
- [x] marks/steam.md
- [x] marks/stolenwarmth.md
- [x] marks/tempest.md
- [x] marks/theboltrunstrue.md
- [x] marks/thegroundgives.md
- [x] marks/thegrudge.md
- [x] marks/thermalshock.md
- [x] marks/turnedtostone.md
- [x] marks/venomrime.md
- [x] marks/water.md
- [x] marks/wind.md
- [x] marks/windthrow.md
- [x] marks/winterseed.md

### 2. The guide to effects

- [x] effects.md (too long to rewrite in one go, so it is done in three parts, each committed):
  part 1, "# The guide to effects" to "## memories": done. Part 2, "## reactions" to "## reaction-kindling-point":
  done. Part 3, "## reaction-the-grudge" to the end: done.

### 3. What's new (older sections only; the 0.5 Phase 4 section is already plain)

- [x] whats_new.md

### 4. The game guide

- [x] docs/GUIDE.md (very long, so done in parts, each committed). Code file names, class names and save keys are
  taken out of the prose (they are in git history before this pass). The "Every mark" tables in section 7 are
  made by AgentScripts/DumpMarks.cs from the game's own mark data, so they are left as they are: plain words there
  need the mark data in code changed. Parts: sections 1 and 2: done. Section 3 (Weapons): done. Sections 4 to 6:
  done. Section 7 text before the tables: done. Sections 8 to 10: done.

### 5. Other player-facing files

- [x] readme.md (the READ ME beside the playtest exe; filled in by CombatPlaytestBuild, not by Playtest Texts)
