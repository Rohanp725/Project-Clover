# The rest fire's screens: a UI pass for the Hand and the Way

Written by Claude on 2026-11-21 for the owner, from the owner's answer to the Way plan's Q9
(`docs/design/steel/way_build_plan.md`, "Owner's answers"): "There needs to be a pass that cleans up the ui completly
and makes it much more readable and understandable, and not just what the hand is right now. Also the way should be
changed on a similar screen to the hand, but the ui needs to be well designed, maybe even having them both on screen is
possible but do some reasearch on the best uis and whatnot to work with."

This is a plan only. Nothing here is built, and no decision in it is final until the owner answers section 6.

**Progress** (sections are committed one at a time; a section not ticked is not written yet)

- [x] 1. What exists now
- [ ] 2. What the best build screens do (research)
- [ ] 3. Three layouts for the Hand and the Way together
- [ ] 4. A style guide for every menu
- [ ] 5. The build plan and the checks
- [ ] 6. Owner questions

---

## 1. What exists now

Paths are under `Assets/_Project/Scripts/Runtime/UI/` unless they say otherwise. Line numbers are as of commit
`b08abac`. Screenshots are in `review/shots/current/` (`p2_hand_1080.png`, `p2_hand_list_west_1280.png`,
`p2_rest_menu_1280.png`, `p2_rest_chart_1280.png`).

### 1.1 How every menu is drawn

- **uGUI with the old `UnityEngine.UI.Text`**, built by code at play (`MenuParts.cs:11` to 85), on the HUD's canvas,
  which is scaled to a 1920 by 1080 board (`Editor/Builders/PlayerBuilder.cs:552`, `ScreenFit.cs:31` to 37). No
  TextMeshPro, no UI Toolkit, no layout groups: every box and label sits at a hand-typed pixel position.
- **One font**, Spectral SemiBold, a book serif (`Art/Fonts/spectral_semibold.ttf`, loaded in
  `Editor/Builders/UiArt.cs:16`). Every label is shrunk to 86% of the size asked for (`MenuParts.cs:39`) and gets a
  2-pixel black `Outline` component (`MenuParts.cs:45` to 47).
- **Four colours**, as constants: `Ink` (near black), `Plain` (near white), `Gold`, and `Faint`, which is pure white
  at 60% (`MenuParts.cs:13` to 17). `GuidePage.cs:38` to 40, `PauseMenu.cs:28` and `ControlsPage.cs:65` to 67 each keep
  their own copy, and `ControlsPage`'s `Faint` is 45%, not 60%. Inline colour codes are typed into strings
  (`HandMenu.cs:550`, 551).
- **Every screen reads its own buttons** in its own `Update` (for the Hand, `HandMenu.cs:648` to 697), with its own
  repeat timer, its own mouse picking and its own hint line.

What this costs: the old `Text` has no signed-distance-field rendering, so small words go soft at 720p and blurry when
scaled to 4K; the outline is drawn as four extra copies of each word, which thickens the serif's thin strokes into a
smudge at small sizes. Nothing is shared, so the screens drift apart (three different "faint" greys already).

### 1.2 The rest menu (`RestMenu.cs`, `RestChoices.cs`)

A narrow panel on the left of the fire: "Set the Hand", "Shapes", "The chart", "Leave the fire", with one note under
the list (`RestMenu.cs:98` to 128; `p2_rest_menu_1280.png`).

- **The Way never shows.** `RestMenu.cs:28` and 139 build `new RestChoices()`, which leaves the Way out
  (`RestChoices.cs:21`, 24 to 32).
- The note under the list (`RestMenu.cs:126`) is in `Faint` at 24 (20 px at 1080p, 14 px at 720p), and it is the only
  place that says what an entry is for.
- The hint (`RestMenu.cs:128`, 260) is 21 (18 px at 1080p, 12 px at 720p).
- The HUD's own bars, the spell diamond and the floor's words stay visible around the panel and compete with it; the
  dim is only 55% (`RestMenu.cs:104`).
- This one is close to fine. It is a short list, the fire shows beside it, and the cursor is clear.

### 1.3 The Hand (`HandMenu.cs`)

What the player chooses here: a spell in each of the four slots (West, North, East, South, shaped like the face
buttons), a surge, or one of a suggested Hand and three saved Hands. The two elements carried are shown but cannot be
changed here (`HandMenu.cs:397`: "set at the altars" or "chosen in the hub"). Shapes are on another screen (1.4).
Devotion, the Empowers and the two infused strikes, which `STEEL_AND_ELEMENT.md` 5.1 counts as part of the build, are
not shown at all.

What is wrong with how it reads (`p2_hand_1080.png`, `p2_hand_list_west_1280.png`):

1. **It does not say what a spell does.** For an element's own spell the right side reads "Fire, Opener. Fire's own
   spell." (`HandMenu.cs:612` to 621, the `else` at 621). The player learns the name, the element and a role word,
   and nothing about the spell.
2. **The role words are never explained.** "Opener", "Breaker" (`HandMenu.cs:179`, 412, 550) are shown as bare words.
   The design asks for a small role icon on each spell and a Hand that reads "like a hand of cards: two openers and no
   detonator" (`STEEL_AND_ELEMENT.md` 5.3); the screen never sums the roles up, so the gap the design wants the
   player to see is hidden.
3. **The stick does not move where it points.** Up, down, left and right all mean "next" or "back one" round a fixed
   ring of nine places: the four slots, the surge, the suggested Hand and the three saved Hands
   (`HandMenu.cs:680` to 689, `Move` at 263 to 273). Pushing up on West goes to Saved Hand 3. On a pad that is
   the first thing a player notices, and it breaks the promise of a diamond laid out like the buttons.
4. **No comparison.** Choosing for a slot swaps the right side into a list (`HandMenu.cs:530` to 574). The spell
   now in the slot is no longer described anywhere, so there is nothing to compare the highlighted choice against.
5. **The highlighted choice's words are far from it.** They sit at the bottom of the panel (`choiceLine`,
   `HandMenu.cs:206`, y = -720), in `Faint` at 23 (17 px at 720p), up to twelve rows below the row the eye is on.
6. **Text too small at 720p.** The list's rows are set to 21 and its headings to 18 directly (`HandMenu.cs:546`,
   554), skipping the 86% rule, so they are 21 and 18 px at 1080p and 14 and 12 px at 720p. The slot's key line is 20
   (`HandMenu.cs:178`, 17 px at 1080p), the role line under a spell's name is 19 (`HandMenu.cs:180`, 16 px), the hint
   is 22 (`HandMenu.cs:209`, 19 px at 1080p, 13 px at 720p). At 720p most of the screen is under 16 px.
7. **Gold on orange for the chosen row.** The list's cursor is an orange brush stroke at 30% (`HandMenu.cs:199`)
   and the chosen row's words turn `Gold` on top of it (`HandMenu.cs:553`): two warm colours with little contrast
   between them.
8. **Half the panel is empty.** The diamond takes the left half; the right half holds one or two lines and the four
   Hand rows (`HandMenu.cs:187` to 196). The space that should hold the description holds nothing.
9. **Element colour is on the box rims only** (`HandMenu.cs:470`). The spell's name is the same near-white whatever
   its element, so a glance does not tell Fire from Lightning.
10. **The hint line carries four instructions in keyboard words** ("W S, the wheel or the mouse: choose", "E or a
    click: open", "Q: the numbers", "Space: back to the fire"; `HandMenu.cs:576` to 584), in one faint line along the
    bottom. Nothing shows the button glyph next to the thing it acts on.
11. **"Q: the numbers"** (the `WeaponSkill` button as a details toggle, `HandMenu.cs:135`, 664) hides the mana cost
    and cooldown by default. Cost is part of the choice, not a detail.
12. **Saved Hands are a row of commas.** "Hand 1: Flame Wave, Thunderclap, empty, empty; Kindle"
    (`HandMenu.cs:428`, `Summary` at 632 to 644), with no element colour and no picture.
13. **Shut slots say why in two places at two lengths** ("Opens at the Champion" in the box, a long sentence on the
    right, `HandMenu.cs:408`, 482).

### 1.4 The shapes (`ShapeMenu.cs`, `ShapeChoices.cs`)

A separate screen from the rest menu: each learned spell and reaction in a list on the left, and its two shapes in
full in two boxes on the right; left and right (or use) wear the other shape (`ShapeMenu.cs:12` to 18, layout 122 to
152).

- The shape is part of the spell, but it is set on a different screen from the slot it goes in. A player choosing a
  spell for West cannot see which shape it will be in.
- Thirteen rows of 25 (21 px at 1080p, 14 px at 720p; `ShapeMenu.cs:51`, 129), the same gold-on-orange cursor
  (`ShapeMenu.cs:126`), the same faint hint (152).
- This screen reads better than the Hand: the two shapes are shown side by side in full words, which is a real
  comparison. That is the thing to keep.

### 1.5 The chart (`UpgradeMenu.cs`, `MarkDetails.cs`, `GuidePage.cs`)

The element trees on a night sky, with a details panel on the right (`p2_rest_chart_1280.png`).

- It looks good and the icons read. The faults are the words: the hint is two lines of small faint text along the
  bottom (`UpgradeMenu.cs:814` to 830; about 11 px at 720p in the screenshot), listing seven buttons.
- **Full details exist only in the playtest.** `OpenDetails` returns false unless `PlaytestMode.On`
  (`UpgradeMenu.cs:533`), and the details open as a separate full-screen page (`GuidePage`) over the chart
  (`UpgradeMenu.cs:537`), so the chart vanishes while the player reads about a mark on it.
- The use button on a learned spell "puts it in the next spell slot round", the stopgap from before the Hand
  (`UpgradeMenu.cs:17`). Two ways to set a slot, one of them blind.

### 1.6 The Way (not built)

- `WayRules.ChooseAspect` and `ChooseKeystone` exist (`Runtime/Player/Way/WayRules.cs:142`, 152); nothing in the UI
  calls them. The Way chart (`WayLayout`) is not written (`way_build_plan.md` 1.2 item 3, 2.5).
- What the player will choose there, per carried weapon: **one aspect** of three plain boughs and, once taken, the
  god's bough (the Katana's Sheath, Stream, Answer and Storm Katana); **one keystone** of the two on the active bough
  (the Sheath's The Cut Lands at the Click or A Hundred Days at the Shrine); and **one bearing** of two on the shared
  Body branch (the Enduring Body or the Light Body). The technique, Temper rank and the bought marks are shown but
  not chosen. Everything is bought with Memories on the chart; only the three picks are made at the fire.
- The owner wants those picks made "on a similar screen to the hand", maybe the same one.

### 1.7 The rest of the menus, for the style guide

- **Pause menu** (`PauseMenu.cs`) and **controls page** (`ControlsPage.cs`): a list in the same frame and brush; its
  own colour copies (`PauseMenu.cs:28` to 29, `ControlsPage.cs:65` to 67).
- **Guide and reading pages** (`GuidePage.cs`): a list on the left and a body of 29 on the right (25 px at 1080p),
  split into pages; its own label maker, a copy of `MenuParts.Label` (`GuidePage.cs:190` to 205).
- **The HUD's spell diamond** (`SpellDiamondHud.cs`) shows each spell as two initials ("BE", "BO", "K",
  `SpellDiamondHud.cs:169`) in a coloured ring, not a picture. Out of this pass's scope, but the icons this pass makes
  should be the same ones it later wears.

### 1.8 In one line

The screens are pixel-placed `Text` with three greys and no icons; the Hand names things without saying what they do,
its stick does not follow the diamond, and nothing is compared; the Way has no screen at all.
