# The rest fire's screens: a UI pass for the Hand and the Way

Written by Claude on 2026-11-21 for the owner, from the owner's answer to the Way plan's Q9
(`docs/design/steel/way_build_plan.md`, "Owner's answers"): "There needs to be a pass that cleans up the ui completly
and makes it much more readable and understandable, and not just what the hand is right now. Also the way should be
changed on a similar screen to the hand, but the ui needs to be well designed, maybe even having them both on screen is
possible but do some reasearch on the best uis and whatnot to work with."

This is a plan only. Nothing here is built. The owner answered section 6 on 2026-11-21 (layout C, UI Toolkit); section
3.5 is the chosen layout in full, section 4 is adjusted to it, and section 5 is rewritten for UI Toolkit. A static
mockup of 3.5 at 1920 by 1080 is `docs/design/mockups/loadout_c.html` (picture: `loadout_c.png`); the owner approved
its look and answered 6.1's questions the same day.

**Progress** (sections are committed one at a time; a section not ticked is not written yet)

- [x] 1. What exists now
- [x] 2. What the best build screens do (research)
- [x] 3. Three layouts for the Hand and the Way together
- [x] 3.5 Layout C as chosen, in full (after the owner's answers)
- [x] 4. A style guide for every menu
- [x] 5. The build plan and the checks (rewritten for UI Toolkit)
- [x] 6. Owner questions (answered, 6.1 included)

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

---

## 2. What the best build screens do (research)

Principles taken from how other games lay out build, loadout, tree and charm screens, and from the published
readability guidelines. Each says what it is, where it comes from, and what it means here. They are principles, not
screens to copy: nothing in section 3 recreates another game's layout. Where a source is a player's critique rather
than the studio's own word, it says so.

### 2.1 Reading at a distance

**P1. Body text at least 28 px at 1080p; nothing a player must read under 24 px.** Xbox's guideline sets 26 px at
1080p as the console minimum (18 px for PC), measured from the lowest descender to the highest ascender, with text
scalable to 200%. The Game Accessibility Guidelines take Amazon's 10-foot rule of 28 px at 1080p as a minimum, not a
target; the IGDA's accessibility group asks for 32 px. A pad player on a sofa sits about 3 m (10 feet) from the
screen, a desk player about 60 cm. *Here*: the game is played on a pad, so the rest screens are sized for the sofa:
body 30, nothing below 24, at 1080p. Today most of the Hand is 16 to 21 px (1.3 item 6). Sources:
[XAG 101](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/101),
[Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/use-an-easily-readable-default-font-size/),
[IGDA GA-SIG](https://igda-gasig.org/how/platform-level-accessibility-recommendations/text-size/).

**P2. Contrast of 4.5 to 1 against whatever is behind the words.** Microsoft's "Clear Text" tag asks for 4.5:1
against all backgrounds and a sans-serif face, or an option for a solid backing; WCAG asks the same of body text.
*Here*: words sit on a solid ink panel, never on the dimmed world; grey "faint" text is checked against the panel,
not guessed. Source: [Xbox accessibility feature tags](https://learn.microsoft.com/en-us/xbox/accessibility/accessibility-feature-tags).

**P3. Simple, bold, symmetrical icons read at small sizes.** Ghost of Tsushima's icons were drawn from Japanese
family crests because flat, symmetrical shapes with bold lines stay readable small; its menu art was crushed to
strong black and white. *Here*: the chart's code-painted icons (`UpgradeIcons.cs`) already follow this; role icons
and spell icons are made the same way. Source: [Mike Ackerman, Ghost of Tsushima UI](https://mikeackerman.art/project/ghost-of-tsushima-ui).

### 2.2 Equipped and available

**P4. Show what is worn and what is owned in two clearly separate places, on one screen.** Hollow Knight's charm
screen keeps the equipped charms and their notch bar in a row at the top, the collection below, and the chosen
charm's picture, cost and words in a pane at the side; equipped charms glow, the rest are dimmed. *Here*: the worn
Hand and Way stay on screen while choosing; today the Hand swaps its right side for a list and the worn spell
disappears from view (1.3 item 4). Source: [Hollow Knight wiki, Charms](https://hollowknight.wiki/w/Charms).

**P5. Put a choice's cost on its face, not only behind a selection.** A player's critique of Hollow Knight faults the
charm screen for showing a charm's notch cost only once it is selected; Hades II prints each Arcana card's cost in
its corner and fills a gauge as cards are turned on; Hades shows an unaffordable Mirror cost in red. *Here*: mana or
cooldown on every spell row, Memories on every mark, and a reason on every locked one. Sources:
[critique, Medium](https://medium.com/@sam.cuevasp/hollow-knight-1578e3ebc781),
[Hades II wiki, Arcana](https://hades2.wiki.fextralife.com/Arcana_Cards),
[Hades patch notes](https://hades.fandom.com/wiki/Patch_004).

**P6. A pair where only one can be active is drawn as a pair.** Hades's Mirror of Night was reworked so each talent
has a flip side and only one of the two is active. *Here*: a bough's two keystones and the Body's two bearings are
exactly this: both shown side by side, one lit, and one press flips them. Source:
[Hades Patch 040, the Nighty Night Update](https://hades.fandom.com/wiki/Patch_040).

**P7. Learning and equipping must not hide from each other.** Sekiro's players were confused that a combat art
learned on the skill screen also had to be equipped on another; one wrote of being "beaten by the menu". Monster
Hunter Wilds' players complain of separate places to change gear and to read about it. *Here*: the spell, its shape
and its slot are one decision and belong on one screen; the chart's stopgap that drops a spell "in the next slot
round" (1.5) goes. Sources: [Sekiro, Steam discussion](https://steamcommunity.com/app/814380/discussions/0/1815422173039024825),
[Monster Hunter Wilds, Steam discussion](https://steamcommunity.com/app/2246340/discussions/0/599646660884199764/).

### 2.3 Comparing before committing

**P8. Compare against what is worn, value beside value, not by colour alone.** A critique of Elden Ring's equipment
screen asks for the worn value next to the new one (as Dark Souls's upgrade screen does: old, arrow, new) rather than
numbers that only change colour; a UX case study notes players open that screen to compare two loadouts, not to read
one. *Here*: when choosing for a slot, the worn spell and the highlighted one are shown side by side. Sources:
[Lexeditor issue 801](https://github.com/Lexer-Lux/Lexeditor/issues/801),
[Elden Ring UX case study](https://medium.com/@haajmuskid/redesigning-elden-rings-ui-a-ux-case-study-a5bd8fb4ba47).

**P9. Say what you lose as well as what you gain.** Diablo IV's advanced comparison lists the properties gained, the
properties lost, and the change to equipped skills. *Here*: swapping a spell can leave a Hand with no opener, and
switching aspect puts that bough's marks to sleep; both are said before the press. Sources:
[Diablo IV advanced tooltips](https://www.d4itemssale.com/news/diablo-4-advanced-tooltips.html),
[Diablo IV forum](https://us.forums.blizzard.com/en/d4/t/how-to-compare-new-item-with-a-masterworked-item/165341).

**P10. Summarise the whole build at a glance.** The Steel and Element design already asks for role icons so a Hand
"reads like a hand of cards: two openers and no detonator" (`STEEL_AND_ELEMENT.md` 5.3); Hades II's Grasp gauge
fills as cards are chosen so the total is always in view. *Here*: a strip of the four role icons with counts, and a
plain line when a role is missing.

### 2.4 Moving with a pad

**P11. The stick moves where it points.** Controller guides agree: exactly one thing is focused, focus moves to the
neighbour in the direction pushed, never across the layout, and grids get explicit rules at their edges. *Here*:
today every direction means "next" (1.3 item 3). Each place on the new screen names its up, down, left and right
neighbours. Sources: [GameDev.net, menu navigation pass](https://gamedev.net/blogs/entry/2298688-a-menu-navigation-pass-for-controller-players-eight-checks-before-your-first-console-build),
[Microsoft, gamepad interactions](https://learn.microsoft.com/en-us/windows/apps/develop/input/gamepad-and-remote-interactions).

**P12. Bumpers change the page; face buttons act; the screen opens on the likely choice.** Shoulder buttons switch
tabs or pages; confirm and back act; a screen opens with focus on what is most often wanted; anything a mouse would
hover (a tooltip, a comparison) has a pad equivalent on the focused item. *Here*: open on the slot last changed;
details sit on the focused item, not behind a separate page. Sources: as P11, and
[StraySpark, game UI principles](https://www.strayspark.studio/blog/game-ui-ux-design-principles).

**P13. Do not loop the cursor round the ends.** An accessibility guide asks menus to stop at their ends, with a sound,
so players know where they are. *Here*: the Hand's ring of nine wraps today. Source:
[Accessible Game Design, menus](https://accessiblegamedesign.com/guidelines/menus.html).

**P14. Few presses to read, and every button named where it acts.** Monster Hunter Wilds is criticised for the
presses needed to read a skill's words; World is praised for showing them on hover. Elden Ring players found a
useful menu button after hundreds of hours because nothing named it. *Here*: the focused item's words are always
shown; each button's glyph sits next to the thing it acts on, not in one line at the bottom. Sources:
[Monster Hunter Wilds, Steam discussion](https://steamcommunity.com/app/2246340/discussions/0/599646477861527777/),
[Elden Ring UX case study](https://medium.com/@haajmuskid/redesigning-elden-rings-ui-a-ux-case-study-a5bd8fb4ba47).

### 2.5 Words and pictures

**P15. One exact effect line first, the cost next, the long words last.** Hades's aspects are one short sentence
("Your Special automatically seeks the foe last struck by your Attack.") and one number that grows with rank; Diablo
IV puts ranges and fine print behind an option. A player's critique of Hades calls its vaguer boon words its worst
habit, so the short line must still be exact. *Here*: every spell, aspect and keystone gets one "what it does"
sentence of 15 words or fewer, then cost and timing on one line, then the long words in the details pane. Sources:
[PSNProfiles, Hades aspects](https://psnprofiles.com/trophy/13658-hades/17-infernal-arms),
[Scientific Gamer, Hades](https://scientificgamer.com/thoughts-hades/).

**P16. Show numbers when the choice depends on them.** A design analysis of Ghost of Tsushima notes that its charms
and armour change stats without ever showing a number. That suits a minimal game, not a build screen. *Here*: mana
and cooldown are always shown; multipliers sit in the details. Source:
[Game Developer, Ghost of Tsushima design analysis](https://www.gamedeveloper.com/game-platforms/ghost-of-tsushima-design-analysis).

**P17. Keep a build's parts together; show synergy by placing things side by side.** Path of Exile 2 moved support
gems off the gear into one skill window, each skill's supports in its own row, so what changes a skill sits beside
it. *Here*: a slot shows its spell, its shape and its role together; the weapon card shows aspect, keystone,
technique and Temper together; an element's colour ties a spell to the infused strike of that element. Source:
[Path of Exile 2 wiki, Gems](https://pathofexile2.wiki.fextralife.com/Gems).

**P18. Saved builds are one press away and read as pictures.** Monster Hunter players missed World's loadouts and
Capcom added "View Loadouts" back to Wilds; Hades II keeps presets for its Arcana. *Here*: the three saved Hands
become a row of four small icons each, worn with one press, kept with another. Sources:
[HappyGamer, Wilds loadouts](https://happygamer.com/monster-hunter-wilds-is-finally-fixing-its-most-annoying-menu-problem-130777/),
[Hades II wiki](https://hades2.wiki.fextralife.com/Arcana_Cards).

**P19. Motion is short and says where focus went.** Hades's menus open in under a second, each in its own style, and
lead the eye to what matters. *Here*: a 0.12 s slide of the cursor and a soft tick on every move; nothing waits on an
animation. Sources: [UI Noir: Hades](https://medium.com/@donxu29/wip-9e7289c913e4),
[WhatCulture, Hades details](https://whatculture.com/gaming/hades-14-awesome-details-you-totally-missed?page=5).

**Not found**: no studio write-up of the Hades weapon rack, Death's Door's menus or Tunic's menus turned up. Tunic's
illustrated manual (diagrams over paragraphs) is noted only as a mood for the details pane's pictures
([Tunic wiki, Instruction Booklet](https://tunic.wiki/books/instruction-booklet)).

---

## 3. Three layouts for the Hand and the Way together

### 3.0 What every layout must hold

| Part | Chosen here? | From |
|---|---|---|
| The two elements carried | No, shown only (set in the hub; in the playtest at the altars) | `HandMenu.cs:397` |
| Devotion, when both are the same element | No, shown as a line when it is on | `STEEL_AND_ELEMENT.md` 5.6 |
| Four spell slots, each with its spell and that spell's shape | Yes | the Hand and the shapes screen today |
| The surge | Yes | |
| A suggested Hand and three saved Hands | Yes (wear, keep) | |
| The two infused strikes (which element on which button) | No, shown: they tie the weapon to the elements | `STEEL_AND_ELEMENT.md` 5.1 |
| The weapon's aspect (three plain boughs, plus the god's once taken) | Yes | `WayRules.ChooseAspect` |
| The active bough's keystone (one of two, once bought) | Yes | `WayRules.ChooseKeystone` |
| The Body's bearing (one of two, once bought) | Yes | `way_build_plan.md` 2.4 |
| The technique, the Temper rank, the Memories and Shards held | No, shown | |

Buying stays on the two charts (the element chart and the Way chart). The loadout screen only arranges what is
already bought, and every locked thing says where it is bought and for how much.

Two rules hold in every layout below:

- **The details pane is always on screen** and always describes the focused thing, in the format of 4.4. No details
  toggle, no separate page (P12, P14).
- **What you are changing stays in view while you choose** (P4, P8). The list of choices never covers the slot it
  fills, and the pane shows the worn choice beside the focused one.

Sizes below are on the 1920 by 1080 board the HUD already uses.

### 3.1 Layout A: one screen, Steel on the left and Element on the right (recommended)

```
+--------------------------------------------------------------------------------------------------+
|  LOADOUT                                                   Memories 12    Shards 340              |
+------------------------------+------------------------------------+------------------------------+
|  STEEL  Katana               |  ELEMENT  Fire  +  Lightning       |  THUNDERCLAP                 |
|                              |  (carried from the hub)            |  Lightning spell  [Breaker]  |
|  Aspect                      |                                    |                              |
|  [ The Sheath            > ] |              [ N ]                 |  A clap of thunder all       |
|                              |           Thunderclap              |  round you. It fills the     |
|  Keystone                    |           Rolling  [B]             |  stagger bar fast.           |
|  [*Click*  ] [ Hundred Days] |  [ W ]       [Surge]      [ E ]    |                              |
|                              |  Flame Wave   Kindle      empty    |  20 mana    Ready in 6 s     |
|  Bearing                     |  Wide   [O]   Fire                 |  Shape: Rolling Thunder      |
|  [ Enduring] [*Light Body*]  |              [ S ]                 |    (left / right to swap)    |
|                              |              empty                 |  --------------------------  |
|  Technique  Before the       |                                    |  Worn here now: Thunderclap  |
|             Thunder (sealed) |  Roles [O]1 [B]1 [D]0 [C]0 [M]0    |                              |
|  Temper     III              |  No detonator: nothing spends your |  (A) Choose a spell here     |
|                              |  burns and charges yet.            |                              |
|  Infused strikes             |                                    |                              |
|  LT+RB Fire   LT+RT Lightning|  Hands  [Suggested] [1] [2] [3]    |                              |
+------------------------------+------------------------------------+------------------------------+
|  (B) Back to the fire                                                       (L3) Bigger text      |
+--------------------------------------------------------------------------------------------------+
   560 px                          760 px                              520 px
```

- **What the player sees first**: the two headers, "Steel: Katana" and "Element: Fire + Lightning", in the largest
  words on the screen; then the glow round the focused slot; then the pane on the right. On opening, focus is on
  the last thing changed at any fire (the first time, the West slot).
- **How the pad moves** (P11, P13): the stick and d-pad move to the neighbour in the direction pushed and stop at the
  edges. The Steel column is a grid of rows: Aspect; Keystone left and right; Bearing left and right; Technique and
  Temper (focusable to read, nothing to press). The Element column is the diamond (W, N, E, S round the surge, moving
  as the face buttons sit) and, under it, the four Hand buttons in a row. Right from any Steel row goes to the
  diamond's West; left from West goes back to the Steel row at that height. Down from South goes to the Hands row.
  The pane on the right never takes focus.
- **The buttons**, each shown by its glyph next to what it acts on, never in a line of instructions (P14):
  - A (use) on a slot: opens that slot's list. A on the aspect: opens the aspect cards. A on an unset keystone or
    bearing: sets it.
  - Left and right on a focused spell, in the list or in the slot: swap its shape. This is where the shapes screen's
    job goes.
  - X on a Hands button: keep the Hand worn now there. A on it: wear it.
  - B: back out of a list, or out to the fire. L3: bigger text (the 125% and 150% sizes of 4.1).
- **Choosing a spell**: the list slides in over the Steel column (Steel is not what is being changed), so the diamond
  with the slot being filled stays in view, pulsing. The list is grouped by element under coloured headers, each row
  with the spell's role icon, name, shape and cost. The pane compares: the focused spell's words on top, "Worn here
  now" with the worn spell's name and cost under the rule, and what the swap does to the Hand ("You will have no
  opener."). A wears it and the list closes.
- **Choosing an aspect**: the aspect cards slide in over the Element column: three cards in a row (four with a god's
  bough), each with the bough's picture, its name, one line, and "Bought 6 of 10 marks". The pane says what changes:
  "Your combo becomes a draw, a return cut, then the sheathe. The Stream's 5 marks go to sleep. Keystone: The Cut
  Lands at the Click." A sets it; the weapon is re-armed (`way_build_plan.md` 2.0).
- **A keystone or bearing** is a pair of cells side by side, the set one lit and framed (P6). Focus one and the pane
  shows its words and, under the rule, "Set now: A Hundred Days at the Shrine". A sets it. An unbought one is drawn
  dark with its price and the line "Buy it on the Way chart. 4 Memories."
- **How a mark's effect reads**, with The Cut Lands at the Click focused:

  ```
  THE CUT LANDS AT THE CLICK                       Keystone of the Sheath
  Hold the draw for up to two seconds, then let go.
  You dash and cut through any guard. The wounds open when the sword clicks home.
  Press  Hold RB, let go             Costs  nothing
  ----------------------------------------------------------------
  Set now: A Hundred Days at the Shrine. Setting this one turns that one off.
  (A) Set this keystone
  ```

- **Why this one**: it is the owner's "both on screen", and it is the only layout where the weapon and the
  elements are seen together, which is what the infused strikes and the Way's infusion marks are about (P17). Nothing
  needs a tab. The cost is width: three columns at sofa size leave the Steel column 560 px, so its words must stay
  short (names, not sentences) and the sentences live in the pane.

### 3.2 Layout B: one screen with two tabs, and a summary strip across the top

```
+--------------------------------------------------------------------------------------------------+
|  (LB)  [ ELEMENT ]   [ STEEL ]  (RB)                         Memories 12    Shards 340           |
|  Katana: the Sheath, the Click  |  Fire + Lightning  |  Flame Wave  Thunderclap  -  -  |  Kindle   |
+-----------------------------------------------+--------------------------------------------------+
|  STEEL tab                                    |  THE STREAM                     Aspect           |
|                                               |  Long combos that flow into     |
|  +----------+ +----------+ +----------+ +---+ |  one another. Dodge mid-combo   |
|  | SHEATH * | |  STREAM  | |  ANSWER  | |STM| |  and keep it going.             |
|  |  [pic]   | |  [pic]   | |  [pic]   | |   | |                                  |
|  | 6 of 10  | | 2 of 10  | | 0 of 10  | |   | |  Changes: your combo grows to   |
|  +----------+ +----------+ +----------+ +---+ |  five cuts. The Sheath's 6      |
|                                               |  marks go to sleep.             |
|  Keystone   [*Click*]   [Hundred Days]        |  ------------------------------ |
|  Bearing    [Enduring]  [*Light Body*]        |  Set now: the Sheath            |
|  Technique  Before the Thunder (sealed)       |  (A) Make this the active aspect|
|  Temper III         Infused: Fire, Lightning  |                                  |
+-----------------------------------------------+--------------------------------------------------+
|  (B) Back to the fire                                                                            |
+--------------------------------------------------------------------------------------------------+
```

The Element tab has the diamond on the left, the list in the middle (always on screen, not swapped in) and the same
pane on the right.

- **What the player sees first**: the tab they last used, and the summary strip, which always shows the whole build
  in one line of names.
- **How the pad moves**: LB and RB change tab (P12). Inside a tab the stick moves to neighbours and stops at edges.
  On the Steel tab the aspect cards are one row; down goes to the keystone pair, then the bearing pair.
- **How a mark's effect reads**: as in A; the aspect cards also show a picture, so the bough can be told by shape.
- **For**: each tab has the whole width, so the aspect cards can be big and the Hand's list need never slide over
  anything. **Against**: the weapon and the elements are never seen together except as a strip of names, and the
  player has to know the other tab exists (P7). It is the Hand and the Way as two screens with a shared frame.

### 3.3 Layout C: the weapon in the middle

```
+--------------------------------------------------------------------------------------------------+
|  LOADOUT                                                       Memories 12    Shards 340          |
|                                                                                                  |
|        ASPECT                         [ large picture of the Katana ]                              |
|   [ The Sheath  > ]                    in the active aspect's look          ELEMENTS Fire + Lightning|
|                                                                                                  |
|   KEYSTONE                   LT+RB Fire  <------ infused ------>  LT+RT Lightning              |
|   [*Click*][Hundred]                                                                [ N ]        |
|                                                                               [ W ] [Sg] [ E ]   |
|   BEARING                    Technique: Before the Thunder (sealed)                 [ S ]        |
|   [Enduring][*Light*]        Temper III                                                          |
|                                                                                                  |
+--------------------------------------------------------------------------------------------------+
|  THE CUT LANDS AT THE CLICK   Hold the draw up to two seconds, then let go. You dash ...  (A) Set |
+--------------------------------------------------------------------------------------------------+
```

- **What the player sees first**: the weapon. The screen says "this is your weapon, and this is what you carry
  into it".
- **How the pad moves**: the Way's choices down the left, the diamond on the right, the weapon in the middle not
  focusable; left and right cross between the two sides through the middle. The pane is a band along the bottom.
- **How a mark's effect reads**: the bottom band, one or two lines; long words do not fit without paging.
- **For**: the strongest picture and the clearest tie between steel and element. **Against**: it needs a picture of
  every weapon in every aspect (10 weapons, about 35 pictures, from Blender), the bottom band is too short for the
  keystones' words, and lists still have to open over something.

### 3.4 The recommendation (superseded: the owner chose C; see 3.5)

**Layout A.** It puts the Hand and the Way on one screen, as the owner asked, without tabs; it is the only layout
that keeps the details pane and the comparison in view at all times; and the rule "the list opens over the half you
are not changing" makes one way of choosing for both the spells and the aspects. It takes the parts of B and C that
are worth having: B's aspect cards (they slide in over the Element column) and C's weapon picture (a small one in the
Steel header, once the weapon renders exist). The rest menu becomes: **Loadout**, **The element chart**, **The Way
chart**, **Leave** (and **To the hub** once built); "Set the Hand" and "Shapes" fold into Loadout (question Q4).

### 3.5 Layout C as chosen, in full

The owner's answer to Q1: "The weapon's picture sits in the middle with its Way choices around it, the Hand below,
and the details pane." This section replaces 3.3 as the layout to build. Where it differs from 3.3: the details pane
is a full column on the right (3.3's bottom band was too short), the Hand sits under the weapon on the same centre
line (3.3 put it on the right), and the Way's choices sit on both sides of the weapon.

#### 3.5.1 What C keeps from A

| A's good part | How C has it |
|---|---|
| The details pane, always on | Columns 9 to 12, the whole height. It describes whatever has focus; no toggle, no separate page (P12, P14) |
| "Worn here now" | Every pane that describes a choice has a compare block under its rule: "Worn here now" for a spell, "Set now" for an aspect, keystone or bearing, "Worn now" for a saved Hand |
| Lists that keep the slot visible | A spell list opens over the Steel half, so the diamond and the slot being filled stay in view. The aspect cards open over the Element half, so the weapon and the keystones stay in view. A list never covers the thing it fills (3.0) |
| The stick goes where it points | Every place names its four neighbours in a table (3.5.4); nothing wraps; the map is tested (5.3) |
| The role-count strip | The top-right corner of the Element half: five role icons with counts, and the missing role named in words |

One thing from A does not carry over: A swapped a spell's shape with left and right on its slot. In C left and right
always move focus, so the shape is swapped with **Y** (3.5.4). That keeps the stick's one rule.

#### 3.5.2 The wireframe at 1920 by 1080

The board is the 1920 by 1080 board the HUD already uses. **The grid**: 12 columns of 120 px with 32 px gutters and
64 px side margins (column n starts at x = 64 + 152 x (n - 1); columns 1 to 8 end at x = 1248, columns 9 to 12 run
1280 to 1856). Top and bottom margins are 56 px, inside the 5% title-safe band. Columns 1 to 8 are the build;
columns 9 to 12 are the pane. The weapon, the infused strikes and the diamond share one centre line at x = 656, the
middle of columns 1 to 8.

```
  LOADOUT  (Spectral 48, Gold)                                                        12 Memories   340 Shards


  ASPECT                            KATANA        WEAPON                      +-THE PANE-------------------------+
                                                                              | [64] A HUNDRED DAYS AT THE       |
  +-------------------------+                                                 |      SHRINE                      |
  | [64] The Sheath         | +-----------------+ [ Temper II     * * . . . ] |      Keystone of the Sheath      |
  |      6 of 10 bought     | |                 |                             |      Bought for 4 Memories       |
  |                         | |   the weapon in | [ Before the Thunder  (x) ] |                                  |
  +-------------------------+ |   its aspect's  |                             | Every light attack is a quick    |
                              |   look, on an   |                             | draw. You draw and put the       |
  KEYSTONE                    |   ink disc      | BEARING                     | sword away in one stroke. ...    |
  +-------------------------+ |                 | +------------------------+  |                                  |
  | * SET  The Cut Lands at | |   (no focus)    | | [48] The Enduring Body |  | 10 stamina a draw                |
  |        the Click        | |                 | |      Hits push you les |  | Press [RB] again and again       |
  +-------------------------+ |                 | +------------------------+  | -------------------------------- |
                              |                 |                             | Set now: The Cut Lands at the    |
  +-FOCUS (Gold)------------+ |                 | +------------------------+  | Click. Setting this one turns    |
  | [48]  A Hundred Days at | |                 | | * SET  The Light Body  |  | that one off.                    |
  |       the Shrine        | |                 | |        Quicker dodges  |  | You lose: the long held draw.    |
  +-------------------------+ +-----------------+ +------------------------+  |                                  |
                                                                              | [A] Set this keystone            |
            [ Infused strikes  [LT]+[RB] Fire strike . [LT]+[RT] Water strike ]   | -------------------------------- |
                                                                              | A draw is a cut made straight    |
                                                                              | from the sheath.                 |
  ELEMENT  Fire + Water     +-(Y) North----------+ ROLES  No movement spell   |                                  |
  Carried from the hub      | [56] Deep Crack    |                            |                                  |
                            |   Detonator . 11 s | O1 B1 D1 C1 M0             |                                  |
                            |   Shape: Ink       |                            |                                  |
                            |                    |                            |                                  |
    +-(X) West-----------+  +--------------------+ +-(B) East------------+    |                                  |
    | [56] Flame Wave    |                         | [56] Whirlpool      |    |                                  |
    |   Opener . 7 s     |     (LT) ( ) Kindle     |   Breaker . 10 s    |    |                                  |
    |   Shape: Cinder Fa |                         |   Shape: Maelstrom  |    |                                  |
    |                    |                         |                     |    |                                  |
    +--------------------+  +-(A) South----------+ +---------------------+    |                                  |
                            | [56] Kiln Stance   |                            |                                  |
                            |   Counter . 9 s    |                            |                                  |
                            |   Shape: The Kiln  |                            |                                  |
                            |                    |                            |                                  |
  HANDS                     +--------------------+                            |                                  |
  +---------------+ +-------(Worn)--+ +---------------+ +---------------+     |                                  |
  | Starter FFWW F| | Hand 1  FWWF F| | Hand 2  WWWF W| | Hand 3  Empty |     |                                  |
  +---------------+ +---------------+ +---------------+ +---------------+     +----------------------------------+


  (B) Back to the fire                                         (LB)(RB) Steel / Element   (L3) Text size   (RS) Scroll
```

One character is 16 px across and one line 20 px down. Every box, in pixels (x from the left edge, y from the top, at 100% text):

| Place | x | y | Width x height | Holds |
|---|---|---|---|---|
| Title | 64 | 56 | 600 x 48 | "Loadout", Spectral 48, Gold |
| Wallet | 1280 to 1856, right-aligned | 64 | 576 x 32 | "12 Memories" in Memories amber, "340 Shards" in the Shard colour, 26, each with its 28 px icon |
| Aspect label, Keystone label | 64 | 128, 272 | 424 x 28 | 24 bold, Text 2 |
| Aspect card | 64 | 160 | 424 x 96 | 64 px aspect icon; name 32 bold; "6 of 10 marks bought" 24, Text 2 |
| Keystone cells | 64 | 304, 396 | 424 x 84 each, 8 apart | 48 px icon; name 28 bold, up to two lines; the set one has the "Set" tag |
| Weapon name | 520 to 792, centred | 128 | 272 x 44 | "Katana", Spectral 36 |
| Weapon picture | 504 to 808 (bleeds 16 px into each gutter; not text) | 176 | 304 x 304 | The carried weapon in the active aspect's look, on an ink disc tinted by the bough; not focusable |
| Weapon label, Bearing label | 824 | 128, 272 | 424 x 28 | 24 bold, Text 2 |
| Temper row, Technique row | 824 | 160, 210 | 424 x 46 each, 4 apart | "Temper II" with five pips (two filled); "Before the Thunder" with a lock and "Sealed" in Text off |
| Bearing cells | 824 | 304, 396 | 424 x 84 each | 48 px icon; name 28 bold; a short line 24 ("Hits push you less", "Quicker dodges") |
| Infused strikes | 216 | 496 | 880 x 40 | "Infused strikes" 24 bold, Text 2; then "[LT]+[RB] Fire strike" and "[LT]+[RT] Water strike", 26 bold, glyphs 28 tall, the element words in their colours; one line, never wrapped |
| Element header (corner) | 64 | 552 | 396 x 73 | "Element" 24 bold and "Fire + Water" (Spectral 36, each word in its element's text tint) on one line; "Carried from the hub" 24 under it |
| Role strip (corner) | 852 | 552 | 396 x 80 | "Roles" 24 bold with the missing role's line beside it ("No movement spell", 24, Loss); under them five cells of a 32 px role icon and its count (28 bold) |
| North slot | 476 | 552 | 360 x 106 | see "A slot card" below |
| West slot, East slot | 100, 852 | 647 | 360 x 106 each | 16 px clear of North and South at the sides, so the four overlap in height like the pad's buttons |
| Surge | 576 | 668 | 200 x 64 | a 64 px ring with the surge's icon, on the centre line (624 to 688); the LT glyph to its left; its name 24 bold to its right |
| South slot | 476 | 742 | 360 x 106 | its A glyph hangs 13 px below, to y 861 |
| Hands label | 64 | 842 | 192 x 28 | "Hands", 24 bold, Text 2 |
| Hands strip | 64, 363, 662, 961 | 876 | 287 x 66 each, 12 apart | Its own row under the whole diamond: "Starter" (the suggested Hand), "Hand 1", "Hand 2", "Hand 3", each a name 24 bold then four 28 px spell icons in W, N, E, S order and the surge's icon on one line; the worn one has a "Worn" tag on its top border, drawn like the "Set" tag; an empty one reads "Empty" in Text off |
| The pane | 1280 | 128 | 576 x 814 | 28 px padding top and bottom, 32 at the sides; the lines of 4.4; scrolls with the right stick |
| Footer | 64 to 1856 | 984 | 1792 x 40 | 32 px glyphs with 24 captions: Back on the left; the screen-wide presses on the right |

**A slot card** (360 x 106): the button glyph that casts it (X, Y, B or A; the keyboard's 1 to 4) sits on the card's
outer edge, half over the border (on North and South, toward the right end, clear of the name), so the diamond reads
like the pad; a 56 px spell icon at the left (the element's
icon with the role's icon in its corner, the first-pass picture of 4.3); to its right three lines: the name (28 bold),
"Opener · 7 s" (24, Text 2, with the 24 px role icon), and "Shape: Cinder Fan" (24, Text 2). An empty slot reads
"Empty" in Text off with a dashed rim; a shut slot reads "Opens at the Champion" in Text off with a lock, and the pane
says the rest. The card's rim is the spell's element colour.

**The set mark of a pair** (keystone or bearing) is a filled lamp at the cell's left and a "Set" tag on its top
border, in Text on a Raised chip. Gold is kept for the focus frame alone, so the set one and the focused one never
look alike (4.2 is changed to match).

**Ink lines**: a 2 px line in Text off at 40% runs from the foot of the weapon's disc to the infused strikes row, and
from there to the North slot: the picture of "this is your weapon, and this is what you carry into it". Decoration
only.

#### 3.5.3 What the player sees first

1. **The weapon.** It is the one large picture on the screen and the only thing on the centre line above the Hand, on
   an ink disc lit faintly in the active aspect's colour. On opening it settles in over 0.2 s (a fade and a 12 px
   rise); nothing waits on it.
2. **The focused place**, by its Gold frame. On opening, focus is on the last place changed at any fire; the first
   time ever, the West slot. The pane already describes it.
3. **The two names in Spectral 36**: "Katana" over the weapon and "Fire + Water" at the Element half's corner. They
   say what this build is in three words.
4. Then the rest, read from the centre outward: the Way's choices beside the weapon, the diamond under it, the role
   strip, the Hands.

The world behind is dimmed to 70% and the HUD is hidden (4.6). The screen's title is small next to the weapon on
purpose: the player knows where they are.

#### 3.5.4 Navigation

**The places that take focus** (16, plus the lists): the aspect card; the two keystone cells; the Temper row; the
Technique row; the two bearing cells; the infused strikes row; the element header; the role strip; North, West, the
surge, East and South; Starter, Hand 1, Hand 2, Hand 3. The weapon picture and the pane never take focus. Temper,
Technique, the infused strikes, the element header and the role strip take focus so the pane can explain them; A does
nothing on them except on the element header (it opens the Reactions page, Q4) and no A glyph is shown.

**The stick and d-pad** (one press, one move; held, it repeats after 0.32 s, then every 0.2 s; a move with nowhere to
go plays a soft dull tick and stays):

| From | Up | Down | Left | Right |
|---|---|---|---|---|
| Aspect | stays | Keystone 1 | stays | Temper |
| Keystone 1 (top) | Aspect | Keystone 2 | stays | Bearing 1 |
| Keystone 2 | Keystone 1 | Infused strikes | stays | Bearing 2 |
| Temper | stays | Technique | Aspect | stays |
| Technique | Temper | Bearing 1 | Aspect | stays |
| Bearing 1 (top) | Technique | Bearing 2 | Keystone 1 | stays |
| Bearing 2 | Bearing 1 | Infused strikes | Keystone 2 | stays |
| Infused strikes | back where it came from (Keystone 2 the first time) | North | stays | stays |
| Element header | Infused strikes | West | stays | North |
| North | Infused strikes | Surge | Element header | Role strip |
| Role strip | Infused strikes | East | North | stays |
| West | Element header | Starter | stays | back where it came from (Surge the first time) |
| Surge | North | South | West | East |
| East | Role strip | Hand 3 | back where it came from (Surge the first time) | stays |
| South | Surge | back where it came from (Hand 1 the first time) | West | East |
| Starter | West | stays | stays | Hand 1 |
| Hand 1 | South | stays | Starter | Hand 2 |
| Hand 2 | South | stays | Hand 1 | Hand 3 |
| Hand 3 | East | stays | Hand 2 | stays |

The rule behind "back where it came from": when two places share one neighbour, that neighbour remembers which one
focus came from and returns there, so pushing a direction and then its opposite always comes back (the test in 5.3).
Left and right between the two Way columns cross the weapon in one move; nothing stops on the picture.

**The buttons** (Xbox names, PlayStation in brackets; each shown as its glyph on the thing it acts on or on the
pane's last line, never in a line of instructions):

| Button | On what | Does |
|---|---|---|
| A (Cross) | a slot or the surge | Opens its spell list (3.5.5) |
| | the aspect card | Opens the aspect cards |
| | a keystone or bearing not set | Sets it; the one beside it turns off. On the set one: nothing, and no glyph |
| | a Hand | Wears it |
| | the element header | Opens the Reactions page (the reactions' shapes, Q4) |
| | a list row or aspect card | Chooses it and closes the list |
| B (Circle) | anywhere | Closes an open list or the Reactions page with nothing changed; otherwise back to the rest menu at the fire |
| X (Square) | Hand 1, 2 or 3 | Keeps the worn Hand there. On an empty Hand, a press. On one that holds a Hand, a 0.6 s hold, with a ring filling round the glyph, because it replaces what was kept. Nothing on Starter, which cannot be overwritten |
| Y (Triangle) | a slot, or a row in a spell list | Cycles the spell to its next shape (N3). If its Shape mark is not bought, nothing, and the pane's last line says where to buy it |
| LB, RB | anywhere | LB jumps focus to the Steel half, RB to the Element half; each half remembers where focus last was. In a spell list, LB and RB jump between the element groups |
| Right stick | anywhere | Scrolls the pane when its words run long |
| L3 (left stick press) | anywhere | Steps the text size: 100%, 125%, 150%, then back to 100% (3.5.8) |
| Menu, View | | Nothing here. The fire is already a pause |

**Mouse and keyboard.** The keys are the game's own (the controls table, `STEEL_AND_ELEMENT.md` 3.5) and follow
rebinding through `ControlNames`; the glyphs switch to key and mouse pictures the moment a key or the mouse is used,
and back to the pad's when the pad is touched.

| Pad | Keys | Mouse |
|---|---|---|
| Stick, d-pad | Arrow keys, or W A S D | Moving the pointer over a place focuses it, so the pane follows the pointer |
| A | E or Enter | Left click on the place |
| B | Space, Escape or Backspace | Right click anywhere |
| X | R (held 0.6 s to replace a kept Hand) | Left click on the X glyph shown on a Hand |
| Y | Q | Left click on the Y glyph shown on the focused slot or row |
| LB, RB | Tab and Shift+Tab | (the pointer reaches both halves) |
| Right stick | Page Up and Page Down | The wheel over the pane; the wheel over a list scrolls the list |
| L3 | T | The pause menu's setting |

#### 3.5.5 Choosing: preview, then commit

Nothing changes until A; B always leaves with nothing changed. A focused choice is previewed in three places at once:
the pane (in words), the role strip (in counts) and the weapon picture (for an aspect).

**A spell.** A on West: the list slides down over the Steel half (columns 1 to 8, y 128 to 536) in 0.12 s; the
diamond stays, and the West card pulses in Gold with "Choosing for (X) West" on the list's top line.

- The list is one column per carried element, under that element's coloured header (Fire, Water), and a third,
  "Both", for duo spells once learned; all in on one element, one column. Each column is 576 wide (three columns: 373),
  rows 72 tall: a 40 px icon, the name (28 bold), and "Opener · 7 s · Cinder Fan" (24, Text 2). Five rows show; the
  list scrolls and keeps the focused row in view. The last row of the first column is "Leave empty".
- Focus opens on the spell worn in the slot, or the first row if the slot is empty. The row worn here has a "Worn"
  tag; a spell worn in another slot has "In (Y) North".
- The preview: the pane describes the focused spell (4.4), and under its rule: "Worn here now: Flame Wave, Opener,
  7 s" and what the swap does: "You will have no opener." or "It moves here from North. North gets Flame Wave." The
  role strip shows the counts as they would be, a changed count outlined with its old number beside it (Opener 1, now
  0). Y cycles the focused row's shape before choosing.
- The commit: A wears it in that shape; the list closes and focus goes back to West; one soft set sound. A spell
  that is in another slot changes places with what was here.

**An aspect.** A on the aspect card: the aspect cards rise over the Element half (columns 1 to 8, y 552 to 942): three
cards in a row, four with the god's bough, each 278 x 390 (four) or 376 x 390 (three): the bough's picture, its name
(32 bold), one line (26), "6 of 10 marks bought" (24), and the "Set" tag on the active one. The preview: the weapon
picture in the centre takes the focused bough's look; the keystone cells show that bough's two keystones, dimmed if
not bought; the pane says what changes: "Your combo becomes a draw, a return cut, then the sheathe. The Stream's 5
marks go to sleep. Keystone: The Cut Lands at the Click." The commit: A sets it and the sword is re-armed
(`way_build_plan.md` 2.0).

**A keystone or bearing.** No list: the pair is on screen. Focus the one not set; the pane shows its words and "Set
now: ... Setting this one turns that one off." and what is lost. A sets it; the lamp slides across in 0.12 s. One
press is enough, because nothing here is lost for good (`STEEL_AND_ELEMENT.md` 5.5).

**A saved Hand.** Focus it: the pane lists its four spells and surge beside the worn ones, the ones that differ
marked, and the role strip previews its counts. A wears it. X keeps the worn Hand there (a hold if it replaces one).

**A locked thing** (a keystone not bought, a shut slot): drawn in Text off with a lock. Its pane's first line is the
reason and the fix: "Buy it on the Way chart. 4 Memories." A does nothing and shows no glyph.

#### 3.5.6 How a mark reads in the pane (the mockup's state)

A Hundred Days at the Shrine focused, The Cut Lands at the Click set:

```
[icon] A HUNDRED DAYS AT THE SHRINE                    (32 bold)
       Keystone of the Sheath · Bought for 4 Memories  (26, Text 2; the price in Memories amber)
Every light attack is a quick draw. You draw and put the sword away in one stroke. If no one hits you, every
fourth cut hangs in the air. Anyone who walks into it is cut again.          (30)
10 stamina a draw                                      (26)
Press [RB] again and again                             (26)
-------------------------------------------------------
Set now: The Cut Lands at the Click.
Setting this one turns that one off.
You lose: the long held draw that cuts through guards. ("You lose" in Loss)
[A] Set this keystone
-------------------------------------------------------
A draw is a cut made straight from the sheath. It hits harder.   (the keyword's line, 24, Text 2)
```

#### 3.5.7 720p and other screens

UI Toolkit's Panel Settings scale the board: **Scale With Screen Size**, reference 1920 x 1080, screen match mode
**Expand** (the board is never smaller than 1920 x 1080 in either direction; this is what `ScreenFit.cs` does for the
HUD today). So:

| Screen | Scale | Body (30) | Smallest (24) | Focus frame (3) | Notes |
|---|---|---|---|---|---|
| 1920 x 1080 | 1 | 30 px | 24 px | 3 px | As drawn |
| 1280 x 720 | 0.667 | 20 px | 16 px | 2 px | The 16 px floor of 5.3 item 4 is met exactly; nothing below 24 is allowed for this reason |
| 2560 x 1440 | 1.333 | 40 px | 32 px | 4 px | |
| 3840 x 2160 | 2 | 60 px | 48 px | 6 px | SDF text stays sharp; the icons are painted at 2x (128 px) so they do too |
| 1280 x 800 (16:10) | 0.667 | 20 px | 16 px | 2 px | The board is 1920 x 1200: the layout stays 1080 tall, centred, with 60 px more dim above and below |
| 2560 x 1080 (21:9) | 1 | 30 px | 24 px | 3 px | The board is 2560 wide: the layout keeps its 1920 width, centred; the dim fills the sides |

The layout's root is a 1920 x 1080 box centred on the board (`max-width` and `max-height` in USS), so no column ever
stretches. Pixel values in the USS are board pixels and scale with the panel. Lines thinner than 2 px are not used,
so nothing vanishes at 0.667.

#### 3.5.8 The text-size setting

Three sizes, **100%, 125%, 150%**, in the pause menu's settings and on L3 (T) in the Loadout; saved in the settings
with the other options; it changes every menu, not the HUD.

- **How**: every size in the USS is a variable in the shared tokens sheet (`--size-body: 30px` and so on, 4.1). The
  setting puts a class on each panel's root, `size-125` or `size-150`, which redefines those variables; variables
  inherit, so every label, icon and row height that uses them grows at once. USS has no `rem`, so this is the way.
  Icons in lines grow with the text; the 64 px slot icons do not.
- **100%**: the wireframe above. Everything fits.
- **125% and 150%**: both halves no longer fit on 1080 at once. **The half without focus folds** to one summary line
  of 72 px: Steel folded reads "[weapon icon] Katana · The Sheath · The Cut Lands at the Click · The Light Body ·
  Temper II"; Element folded reads "Fire + Water · [the four spells' icons and the surge's] · O1 B1 D1 C1 M0". The
  half with focus gets the room (the weapon picture shrinks to fit its column; slot cards drop their shape line, which
  the pane shows anyway; names may wrap to two lines). LB and RB, or moving off the edge of a half, swap which one is
  open (0.12 s). Lists open over the folded half, as at 100%.
- **The pane** keeps its 576 px at every size; at 150% (45 px body) a line holds about 22 characters and the right
  stick scrolls it. A "more below" fade and the RS glyph show at its foot whenever it can scroll.
- **Checked** at every size on 1080 and 720 (5.3 item 3); 150% at 720 is 30 px body, the largest the game shows.

#### 3.5.9 What C needs that A did not

- **The carried weapon in its aspect's look** (3.3's "Against"). The owner chose N1 (a): the weapon's own 3D model,
  drawn live behind the menu and slowly turning, so it shows each aspect's look as soon as its props are in.
- **A summary line for each half**, for 125% and 150% (3.5.8).
- **The Reactions page** opened from the element header (Q4): a small list page in the same kit, a reaction per row
  with its two shapes side by side, as the shapes screen shows them today (1.4).

---

## 4. A style guide for every menu

For every screen the player reads: the rest menu, the loadout, both charts, the pause menu, the controls page, the
guide and the reading pages. The HUD in a fight is out of scope, except that it should wear the same icons.

All sizes are pixels on the 1920 by 1080 board, set directly: the 86% shrink in `MenuParts.cs:39` goes. At 720p
everything is two-thirds of this; at 4K, double.

### 4.1 Type

| Use | Face | Size | Colour |
|---|---|---|---|
| Screen title ("Loadout", "Rest") | Spectral SemiBold, the game's face | 48 | Gold |
| Column or section header ("Steel: Katana") | Spectral SemiBold | 36 | Text |
| Item name: a slot's spell, the pane's title | the body face, bold | 32 | Text |
| Body: the pane's words, a list's rows | the body face, regular | 30 (rows 28) | Text |
| Meta: role, kind, cost, shape | the body face, regular | 26 | Text 2 |
| The smallest anything may be: glyph captions, counts | the body face, bold | 24 | Text 2 |

- **Two faces.** Spectral stays for titles and headers, where its book look sets the mood. Body text moves to a plain
  sans with open shapes, because Microsoft's "Clear Text" test asks for a sans-serif and Spectral's thin strokes
  smear at small sizes on a TV (P1, P2). The recommended body face is **Atkinson Hyperlegible** (SIL Open Font
  Licence, made by the Braille Institute for low vision; free, no account), pinned to one release at intake, with a
  row in `ASSETS.csv` (question Q3).
- **Nothing a player must read is under 24.** Body is 30, the sofa size (P1).
- **Line length** 30 to 45 characters in the pane; line height 1.3 times the size; normal letter spacing (never
  condensed).
- **No outline component on menu text.** Words sit on a solid panel, so they need no outline; the drop outline stays
  only for words drawn over the world (the HUD, floor labels).
- **Text size option**: 100%, 125% and 150%, in the pause menu's settings and on L3 in the loadout. Every screen is
  checked at 150% (5.3).
- Text is drawn by UI Toolkit with TextCore signed-distance font assets (sharp at every size), not the old `Text`
  (Q2 answered: UI Toolkit; 5.0). Every size above is a USS variable, so the text-size setting is one class (3.5.8).

### 4.2 Colour

One set of named colours in one place, used by every screen (today there are four copies and three greys, 1.1).

| Name | Value | Use |
|---|---|---|
| Panel | `#0A0914` at 96% | Every menu's backing. Words never sit on the dimmed world |
| Raised | `#17142A` | Slots, cards, list rows' backs |
| Focus fill | `#2B2134` | Behind the focused slot, card or row |
| Text | `#EEE9E1` | Near-white main text. Never `#FFFFFF` |
| Text 2 | `#B8B1A5` | Meta lines, captions (about 9 to 1 on Panel) |
| Text off | `#86808F` | Locked, sleeping, not bought (about 5 to 1 on Panel; every grey is measured, not guessed) |
| Gold | `#FFCC73` | Titles and the focus frame. Not the set keystone: a set mark is a lamp and a "Set" tag (3.5.2), so set and focused never look alike |
| Loss | `#FF7A5C` | "You will lose", "Not enough Memories". Always with words, never colour alone (P8) |
| Memories | `#E8A24A` (amber) | The Memories count and prices, matching the paid mote (`way_build_plan.md` Q10) |
| Shards | the HUD's Shard colour | Shard prices and counts |
| Element colours | `Elements.Colour` | Element names, slot rims, list group headers, the element's icons |

- **No pure white anywhere in a menu.** The standing rule bans pure white in effects; menus may use near-white for
  text (`Text` above). `MenuParts.Faint`, which is pure white at 60%, is retired.
- **Element colours on text** only for a name or a header, and only where the colour meets 4.5 to 1 on Panel. An
  element whose colour is too dark for that (Dark, perhaps Poison or Metal) gets a lighter text tint of the same hue,
  kept beside its colour in `Elements`.
- **The focused thing** gets the Focus fill and a 3 px Gold frame outside its edge; its words stay `Text`. The orange
  brush stroke with gold words on top (1.3 item 7) is retired from lists; it may stay as a decoration on the rest menu,
  under near-white words.

### 4.3 Icons

- **Style**: flat, symmetrical, bold shapes with no stroke thinner than 4 px at 64 px, one colour on the ink, tinted by
  element; the way the chart's icons are already painted (`UpgradeIcons.cs`, `Editor/Builders/UiArt.cs`), and as
  Ghost of Tsushima drew its crests (P3).
- **Sizes**: 64 in a slot or card, 40 in a list row, 28 inline in a line of text.
- **An icon always has its word beside it**, except in the role-count strip, whose words are in the pane.
- **Sets this pass needs**:
  - The five roles (Opener, Breaker, Detonator, Counter, Movement), so a Hand reads like a hand of cards (P10).
  - A picture for each spell. First pass: the element's icon with the role's icon in its corner, so no spell is a
    blank box; each spell's own picture comes later, ledgered.
  - Way pictures: an icon per aspect (about 35 across ten weapons), per keystone (about 70) and per bearing (2); the
    technique's and Temper's.
  - Button glyphs for the pad (Xbox and PlayStation shapes) and the keys, as pictures, replacing "E", "Space" and
    "LT tap" written as words. They must follow rebinding (`ControlNames`).
  - Later, the aspect cards' large pictures: renders of the approved weapon models in each aspect's look, from
    Blender, through staging and intake.

### 4.4 How a thing is described (the details pane)

The pane sits in the same place on every screen that has one: on the right, grid columns 9 to 12 (576 px wide, from
x = 1280), always describing the focused thing. There are no floating tooltips; a pad cannot hover (P12).

```
NAME                                                (32, bold)
Kind line: "Lightning spell · Breaker", "Keystone of the Sheath"   (26, Text 2)
What it does: one or two sentences, each 15 words or fewer.        (30)
Cost line: "20 mana · Ready again in 6 s", or "Costs nothing".      (26)
Press line, when it has its own press: "Hold RB, let go".           (26)
Shape line, for a spell: "Shape: Rolling Thunder (left or right to swap)"
------------------------------------------------------------------
Compare: "Worn here now: ..." or "Set now: ... Setting this one turns that one off."
What changes: "You will have no opener." "The Stream's 5 marks go to sleep."
The press: "(A) Set this keystone"
```

- **Words**: 8th-grade, short sentences, present tense, "you", verbs first ("You dash and cut through any guard.").
  Digits for numbers. Seconds as "s" only on the cost line; in sentences, "two seconds". No jargon a new player has
  not met (no "DoT", "proc", "i-frames").
- **A locked thing's first line is the reason and the way to fix it**: "Buy it on the Way chart. 4 Memories." "Opens
  when the Fire god falls." (`way_build_plan.md` 2.5 already words these.)
- **Keywords** (Burn, Static, stagger, the infused strike) are written in their colour; the first time one shows on a
  screen, its one-line meaning is added at the foot of the pane.
- **The long words** (the `docs/marks/` texts the chart's details page shows) follow under the rule; when they do not
  fit, the right stick scrolls the pane. The separate full-page details (`UpgradeMenu.cs:530` to 537) go, and the
  details stop being playtest-only.
- **Numbers that decide a choice are always shown** (mana, cooldown, price); multipliers and fine print are in the
  long words (P16).

### 4.5 Focus, movement and buttons

- One thing focused at all times; the stick and d-pad move to the neighbour in the direction pushed; nothing wraps
  round the ends; each screen opens on the likely choice (P11 to P13). A held direction repeats after 0.32 s, then
  every 0.2 s (as today).
- A face button that acts on the focused thing shows its glyph on that thing or on the pane's last line. The footer
  shows only Back and the screen-wide presses. No line of instructions (P14).
- A (use) chooses or sets; B (dodge) backs out one level; LB and RB change page or half (on the Loadout, the Steel
  and Element halves); X keeps (a Hand); Y swaps a spell's shape; the right stick scrolls the pane; L3 changes text
  size. The Loadout's full map is 3.5.4. The mouse clicks what it points at and the wheel scrolls; the
  keys follow the same map through `ControlNames`.
- The cursor slides in 0.12 s with a soft tick on the UI mixer group; nothing waits on an animation (P19).

### 4.6 Layout

- Panels on solid Panel colour, with the world dimmed to about 70% behind them; the HUD's bars, the spell diamond and
  the floor's words are hidden while a full-screen menu is open (1.2, 1.3).
- Layout by USS flex rows and columns on the 12-column grid of 3.5.2, not typed pixel positions, so 125% and 150%
  text reflow instead of overlapping.
- Keep every word at least 64 px from the sides of the screen and 56 px from its top and bottom.
- The same frame sprite, title position and footer on every screen.

---

## 5. The build plan and the checks

Rewritten on 2026-11-21 for the owner's answers: layout C (3.5) in **UI Toolkit** (Q2 b), Atkinson Hyperlegible for
reading and Spectral for titles (Q3), Kenney's glyphs (Q7), the text-size setting now (Q8). Each step is one
`feature-loop` slice: the check first, the code in the right assembly, verified in the editor, then handed over. The
Katana is the first weapon the Steel half shows, as the Way plan builds it first.

### 5.0 How UI Toolkit fits this project

**What the project draws with now** (checked 2026-11-21): every menu and the HUD are uGUI built by code at play
(`MenuParts.cs`; the HUD canvas at 1920 by 1080, fitted by `ScreenFit.cs`); the editor tools are IMGUI. There is no
UXML, USS or Panel Settings under `_Project` (the only USS files are the template's, in `Assets/TutorialInfo/`), and
**no EventSystem** in any scene: each screen reads `Controls` in its own `Update`. Two facts that matter below: the
Input System's project-wide actions are the template's `Assets/InputSystem_Actions.inputactions`, not
`Controls.inputactions` (`ProjectSettings/EditorBuildSettings.asset`, `com.unity.input.settings.actions`), and
`Controls` has only the Player and Spells maps, no UI map.

**What the Unity 6 manual says** (the 6000.3 to 6000.6 pages, read 2026-11-21; links at the end of 5.0):

- With **no EventSystem** in the scene, UI Toolkit makes its own runtime event system at play and detects the active
  input handling. With the Input System it takes its actions from the **project-wide actions asset**: UI/Navigate
  gives `NavigationMoveEvent`, UI/Submit `NavigationSubmitEvent`, UI/Cancel `NavigationCancelEvent`, UI/Point,
  UI/Click and UI/ScrollWheel the pointer and wheel events. Adding a uGUI EventSystem switches that built-in
  processing off and routes focus through `currentSelectedGameObject`.
- **Focus**: each panel has a `FocusController` and a focus ring (`IFocusRing`); the default order is a depth-first
  walk of the tree, changed by `tabIndex` (negative leaves Tab order). An element takes focus when `focusable` is
  true; `delegatesFocus` passes it to a child; `Focus()` and `Blur()` set it. The FAQ advises focusing the first
  element from `Start`, and again whenever the document's object is re-enabled, because disabling a `UIDocument`'s
  object rebuilds its whole tree.
- **Directional navigation** can be overridden per element: on `NavigationMoveEvent`, switch on `evt.direction`, call
  `Focus()` on the chosen neighbour, then `evt.StopPropagation()` and `focusController.IgnoreEvent(evt)` "to prevent
  default navigation" (the 6000.5 FAQ; older pages show `PreventDefault`). Navigation events trickle down and bubble
  up; listen in the bubble-up phase, since controls such as `ListView` cancel the ones they use. Tab and Shift+Tab
  cannot be rebound through the input modules. The default directional pick (the nearest focusable element in that
  direction whose box overlaps on the other axis) is described only by Unity staff on the forums, not in the manual.
- **Text** is TextCore: USS sets a font asset with `-unity-font-definition: url(...)` (or `resource(...)`), which wins
  over `-unity-font`. It must be a TextCore `FontAsset`, not a TextMeshPro `TMP_FontAsset` (USS warns "only Font,
  FontAsset" are supported). Static font assets default to SDF32, the mode for small text. A Panel Text Settings
  asset holds fallbacks.
- **Panel Settings** carry the scale mode (Scale With Screen Size, a reference resolution and a screen match mode),
  the sort order and the theme style sheet applied to every document on the panel; many `UIDocument`s may share one.
- **USS** supports the `:focus`, `:hover`, `:active`, `:disabled` and `:checked` pseudo-classes, custom properties
  (`--name`, read with `var()`), and `transition`; it has no `outline` property, no pseudo-elements and no `rem`
  unit. So the focus frame is a child element, and the text-size setting is variables redefined by a class (3.5.8).

**How the project will use it**:

| Piece | What | Where | Made by |
|---|---|---|---|
| Tokens | Every colour and size of section 4 as USS variables (`--color-panel`, `--size-body`), and the `.size-125` and `.size-150` classes that redefine the sizes. The one place colours live; the C# side reads none | `Assets/_Project/UI/Theme/Tokens.uss` | Written as text |
| The shared sheet | The kit's parts as classes: `.panel`, `.place` (anything focusable), `.place:focus` and its `.focus-frame` child (6 px outside the edge, 3 px Gold), `.glyph`, `.pane`, `.lamp`, the type classes (`.t-title`, `.t-header`, `.t-name`, `.t-body`, `.t-meta`, `.t-small`), the 0.12 s transitions. Overrides the default theme's own focus looks | `Assets/_Project/UI/Theme/Base.uss` | Written as text |
| The theme | Imports Unity's default runtime theme (`@import url("unity-theme://default");`), then Tokens and Base | `Assets/_Project/UI/Theme/Clover.tss` | Written as text |
| A screen | Its tree and its own layout: `Loadout.uxml` and `Loadout.uss` (the grid of 3.5.2 as flex rows and columns; names on every place the code or a probe needs) | `Assets/_Project/UI/<Screen>/` | Written as text; viewable in the UI Builder |
| Panel Settings | Scale With Screen Size, 1920 x 1080, **Expand** (3.5.7), theme `Clover.tss`, sort order above the HUD canvas | `Assets/_Project/Generated/UI/MenuPanel.asset` | An editor builder, `Editor/Builders/UiToolkitBuilder.cs`; never hand-written (`.asset`) |
| Fonts | TextCore font assets of Spectral SemiBold and Atkinson Hyperlegible Regular and Bold: static, SDF32, Latin characters, padding for the 0.667x to 2x range; a Panel Text Settings asset with Atkinson as the fallback | `Assets/_Project/Generated/UI/Fonts/` under stable names | The same builder, from the `.ttf` files in `Art/Fonts/` |
| Screens at play | One `Menus` object with a `UIDocument` per screen, all on `MenuPanel`; a screen shows and hides by its root's `display`, never by disabling the object (which rebuilds the tree) | built by `PlayerBuilder` | Code |
| Runtime code | `MenuScreen` (base: show, hide, first focus one frame after showing, the navigation callback), `FocusGrid`, `MenuInput`, `Glyphs`, `Describe`, and one class per screen | `Runtime/UI/Toolkit/` in `Clover.Runtime` (`UnityEngine.UIElements` is an engine module, so no new assembly reference; U0 confirms) | Code |

UXML, USS and TSS files are text that Unity imports, like `.cs` files; they are not on the list of serialized files
that must not be hand-edited, and their `.meta` files are left to Unity. The new `Assets/_Project/UI/` folder goes
into CLAUDE.md's table and `Clover > Setup > Ensure Project Layout` in U0.

**Input, decided for this project**: a `UI` map is added to `Controls.inputactions` with the standard actions
(Navigate, Submit, Cancel, Point, Click, RightClick, ScrollWheel) and the game's own (Keep, Shape, HalfLeft,
HalfRight, ScrollPane, TextSize), bound for the pad and the keys of 3.5.4, and **`Controls` becomes the project-wide
actions asset** (set by the builder; the template asset is then deleted). One asset holds every binding, so rebinding
covers the menus, and the built-in event system reads the same bindings as the game. No EventSystem is added.

**Navigation, decided**: places get `focusable = true` and keep `tabIndex` 0. Each screen's root handles
`NavigationMoveEvent` in the bubble-up phase: Up, Down, Left and Right look the neighbour up in `FocusGrid` (3.5.4,
with its "back where it came from" memory), `Focus()` it, stop the event and `IgnoreEvent` it; Next and Previous (Tab,
Shift+Tab) jump halves like RB and LB. `NavigationSubmitEvent` is A, `NavigationCancelEvent` is B; `MenuInput` reads
Keep, Shape, the halves, ScrollPane and TextSize from the UI map. The pointer: `PointerEnterEvent` focuses a place,
`ClickEvent` acts on it, `WheelEvent` over the pane scrolls it. **Repeat timing** (0.32 s, then 0.2 s, 4.5): the
manual does not say whether the built-in event system's move repeat can be set. U0 checks it; if it cannot,
`MenuInput` reads UI/Navigate itself and sends `NavigationMoveEvent.GetPooled(direction)` at that timing, and Navigate
is left unbound for the built-in system so a move never arrives twice.

**What else to check in U0** before any screen is built (the `unity-api-checker` agent, then a scratch screen in the
editor): `PanelScreenMatchMode.Expand` exists on Panel Settings in 6000.6 and matches `ScreenFit`; `capture
game_view` includes a screen-space UI Toolkit panel (if it does not, the screenshots of 5.3 use `capture
editor_window` on the Game view); a UI Toolkit panel draws over the uGUI HUD canvas at the chosen sort order; the
Input System's `simulate_key` reaches the built-in event system, so the probes can drive it.

Sources: [Focus order](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-events/uie-focus-order),
[Runtime event system](https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/uielements/uie-support-for-runtime-ui/uie-render-runtime-ui/uie-runtime-event-system),
[Navigation events](https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-events/reference/uie-navigation-events),
[FAQ for input and event systems](https://docs.unity.com/en-us/engine/6000.5/manual/uitoolkits/uielements/uie-support-for-runtime-ui/uie-render-runtime-ui/uie-faq-event-and-input-system),
[Font assets](https://docs.unity.com/en-us/engine/6000.0/manual/uitoolkits/uielements/uie-work-with-text/uie-font-asset/uie-font-asset),
[FontDefinition.fontAsset](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/uielements/fontdefinition/fontasset),
[PanelSettings](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/uielements/panelsettings),
[the default navigation pick, Unity Discussions](https://discussions.unity.com/t/gamepad-support-878706/878706).

### 5.1 The order: what moves, and when

The HUD (`PlayerHud`, `BossHud`, `LandHud`, `SpellDiamondHud`, the prompt and reading lines, the title card, the
fades and the reaction words) **stays uGUI** in this pass: it is drawn over the world, changes every frame and is out
of scope (4). Every menu moves to UI Toolkit, one screen at a time, and each old screen is deleted in the same slice
that replaces it, so no screen exists twice. While both kinds of screen exist, only one menu is open at a time, the old
ones keep reading `Controls` themselves, and a hidden UI Toolkit screen holds no focus.

| Step | What | Replaces | Needs first |
|---|---|---|---|
| U0 | **The kit.** Tokens, Base and the theme; `UiToolkitBuilder` (Panel Settings, font assets, Panel Text Settings); the UI map and `Controls` as the project-wide actions; `MenuScreen`, `FocusGrid` (plain C#, no wrapping, the "came from" memory), `MenuInput`; the U0 checks of 5.0; a scratch screen that proves focus, the frame, a glyph and the text sizes at 1080 and 720 | Nothing yet: `MenuParts` stays until U7 | Atkinson Hyperlegible through staging and intake (5.2) |
| U1 | **Icons and glyphs.** The five role icons and the first-pass spell icons painted in `UiArt` at 128 px; the Katana's aspect, keystone and bearing icons; Kenney's glyphs; `Glyphs` picks the picture for an action's current binding and the last device used. The stand-in icons get their `content_open.tsv` line (Q6) | Role words, initials | U0 |
| U2 | **The pane and the words.** One `Describe` for spells, marks, aspects, keystones, bearings, Temper, techniques, the infused strikes and the roles, giving the lines of 4.4; a "what it does" line for every spell; each missing line ledgered in `content_open.tsv` until written | `HandMenu.Describe` (612 to 630) | U0 |
| U3 | **Loadout, the Element half** in UI Toolkit: the diamond, the surge, the element header, the role strip with its preview, the Hands (hold X to replace), the spell lists over the Steel half, Y for shapes; the Steel half drawn as a placeholder row | `HandMenu.cs`; the spell part of `ShapeMenu.cs` | U0 to U2 |
| U4 | **Loadout, the Steel half**: the weapon picture, the aspect card and cards, the keystone and bearing pairs, Temper, the technique, the infused strikes; the 125% and 150% folding (3.5.8) | Nothing (new); closes `way_build_plan.md` Q9 | `way_build_plan.md` 2.0 (`PlayerBuild.Rearm`), so a set aspect changes the sword; 2.4 for bearings; the first weapon picture (3.5.9) |
| U5 | **The rest menu and the Reactions page.** Loadout, the element chart, the Way chart, (to the hub), leave | `RestMenu.cs`, `RestChoices`; the reaction part of `ShapeMenu.cs`, which is then deleted | U3 |
| U6 | **The charts.** The Way chart (`way_build_plan.md` 2.5) is built in UI Toolkit from the start, its lines drawn with `generateVisualContent` and `Painter2D`. The element chart's right panel becomes the pane and its hint lines glyphs, in UI Toolkit over the chart; the details stop being playtest-only and the "next slot round" stopgap goes. Moving the night-sky chart itself is the last and largest port, done after the Way chart proves the drawing | `UpgradeMenu.cs:530` to 537, 814 to 830; `MarkDetails.cs` | U0 to U2 |
| U7 | **Pause, controls, guide and reading pages**, and the text-size setting in the pause menu. Then `MenuParts`, `GuidePage`'s label maker and the four colour copies are deleted | `PauseMenu.cs`, `ControlsPage.cs`, `GuidePage.cs` | U0 |
| U8 | Later: the HUD's spell diamond wears the spell icons (still uGUI); the per-aspect weapon pictures; whether the HUD moves to UI Toolkit is its own decision | `SpellDiamondHud.cs:169` initials | U1; weapon renders |

U0 to U3 can start now. U4 waits on the Way's wiring (step 2.0 of the Way plan), which is the Way plan's own first
step anyway. The probes change with the screens: they find places by name in the tree (`root.Q("slot-west")`) and
read `focusController.focusedElement` through `script eval`, instead of reading uGUI `Text`.

### 5.2 What is needed from outside the code

- **Fonts**: Atkinson Hyperlegible Regular and Bold (SIL Open Font Licence), downloaded to `staging/fonts/`, pinned to
  one release, brought in through the `asset-intake` skill to `Art/Fonts/` with their `ASSETS.csv` rows and licence
  file; their TextCore font assets, and Spectral's, made by `UiToolkitBuilder` (5.0).
- **Button glyphs**: Kenney's "Input Prompts" pack (CC0, free, no account), pinned to the release downloaded, through
  staging and intake with `ASSETS.csv` rows (Q7): the Xbox, PlayStation and keyboard-and-mouse sets only.
- **Icons**: painted by code in `UiArt`, as the chart's are: no outside art for U1. A stand-in by the owner's word
  (Q6), ledgered so every spell, aspect and keystone gets its own art later.
- **The weapon picture** (U4): drawn live from the weapon's own model (N1 (a), 6.1), so no stills are needed.
- **A sound**: one soft tick for the cursor, one dull tick for a move that goes nowhere, one for setting a thing,
  through `audio-intake` into `Audio/UI`, on the UI mixer group, meeting `Loudness.cs`.

### 5.3 How each screen is checked

Every screen, before the owner sees it (the `unity-check` skill, then the `playmode-probe` loop; probes run with saved
progress sandboxed, never wiping it):

1. **Tests (EditMode)**:
   - `FocusGrid` for each screen: every place reachable from the opening place; pushing a direction and then its
     opposite comes back; nothing wraps; no place is a dead end.
   - The tokens: every text-on-background pair in use is at least 4.5 to 1, computed from the colour variables read
     out of `Tokens.uss`, not by eye.
   - The sizes: every size variable in `Tokens.uss` is 24 px or more at 100%, and `.size-125` and `.size-150` scale
     every one of them.
   - The tree: each screen's UXML loads (`VisualTreeAsset.CloneTree`), every place named in its `FocusGrid` exists in
     it and is focusable, and nothing else is.
   - `Describe`: every spell, aspect, keystone and bearing has a name, a kind line, a "what it does" line and a cost
     line.
   - **PlayMode**: with the screen shown at each text size, no `Label`'s resolved font size is under 24 at 100%, and
     no place's box runs past the 1920 by 1080 layout box or overlaps another place.
2. **The words**: every "what it does" line is 15 words or fewer per sentence and reads at grade 8 or below on a
   Flesch-Kincaid count (a small Python script in `AgentScripts/`, with `textstat` pinned to an exact version).
3. **Screenshots** (`capture game_view`) at **1920x1080 and 1280x720**, and once at **150% text**, of each state: the
   screen opened; choosing a spell; choosing an aspect; a keystone focused that is not set; a locked keystone; a
   saved Hand focused; Devotion on (all in); 125% and 150% with each half folded; on pad glyphs and on key glyphs.
   The opened Loadout is also compared by eye with the mockup, `docs/design/mockups/loadout_c.png`. Named `review/shots/current/ui_<screen>_<state>_<width>.png`. Old and new are shown side by side to the
   owner. A golden frame is taken only when the owner accepts the look.
4. **A readability read** of every shot, by the `shot-reviewer` agent and by eye, against a list: the smallest words
   at 720p are 16 px or more; nothing clipped, overlapping or running off a box; the focused thing is obvious with the
   colour taken away (the shot turned grey); every press has its glyph next to what it does; nothing pure white.
5. **The pad walk**: in Play Mode, the probe opens the fire and the loadout, walks the d-pad over every place (logging
   each one reached, and checking each move against the table in 3.5.4), opens every list, sets a spell, swaps a
   shape with Y, wears and keeps a Hand (and replaces one with a held X), sets an aspect, a keystone and a bearing,
   jumps halves with LB and RB, steps the text size with L3, and backs out with B from every depth. It checks the build changed through `script eval`
   (the loadout's slots, `WayRules.ActiveAspect`) and that the sword was re-armed. Then the same with the mouse:
   every click lands on what it points at.
6. **Performance**: with the loadout open and the cursor moving, `diagnostics profiler_record` shows no steady
   per-frame allocation (`profiler_summary`).

---

## 6. Owner questions

The recommended option is first in each.

**Q1. The layout for the Hand and the Way.**
- (a, recommended) Layout A: one Loadout screen, Steel (the Way's aspect, keystone and bearing) on the left, Element
  (the four spells, the surge and the saved Hands) on the right, the details pane always on the far right.
- (b) Layout B: one screen with two tabs, Element and Steel, switched with LB and RB, and a strip across the top
  naming the whole build.
- (c) Layout C: the weapon's picture in the middle, the Way's choices on its left and the spells on its right.

**Q2. How the menus are drawn.**
- (a, recommended) Stay with uGUI built by code, but switch every menu's words to TextMeshPro (sharp at every size)
  and build every screen from one shared kit of colours, sizes and parts.
- (b) Rebuild every menu in UI Toolkit, with one style sheet for the whole game. Cleaner in the end, but every menu
  and its probes are rewritten.
- (c) Keep the old text and only make it bigger and recolour it.

**Q3. The font for reading.**
- (a, recommended) Spectral stays for titles and headers; body text moves to Atkinson Hyperlegible, a free sans
  made for easy reading.
- (b) Spectral everywhere, only bigger.
- (c) One sans for everything, titles included.

**Q4. What happens to "Set the Hand" and "Shapes" on the rest menu.**
- (a, recommended) Both fold into Loadout: a spell's shape is swapped right on its slot with left and right. The
  reactions' shapes move to a small Reactions page opened from the Element header.
- (b) Loadout sets the slots and the Way; Shapes stays its own entry for spells and reactions.

**Q5. Which weapon's Way the Loadout sets.**
- (a, recommended) Only the weapon you carry. Other weapons' aspects are set on the Way chart, where LB and RB already
  change weapon.
- (b) Every weapon you own, switched with LB and RB on the Loadout.

**Q6. Icons for spells and the Way.**
- (a, recommended) Painted by code like the chart's icons. At first each spell shows its element's icon with its role
  in the corner; each spell's own picture comes later, ledgered.
- (b) Draw a picture for every spell, aspect and keystone before the new screen ships.
- (c) Words only for now; icons later.

**Q7. Button glyphs (the pictures of A, B, LB, E, Space).**
- (a, recommended) Kenney's free Input Prompts pack (CC0, no account), pinned and brought in through staging.
- (b) Painted by code in the game's own style.
- (c) Keep writing the button names as words.

**Q8. A text size setting.**
- (a, recommended) In this pass: 100%, 125% and 150%, in the pause menu and on L3 in the Loadout, with every screen
  checked at 150%.
- (b) Later, after the screens are rebuilt.

## Owner's answers (2026-11-21)

- **Q1, the layout: C, the weapon in the centre (not the recommended A).** The weapon's picture sits in the middle with its Way choices around it, the Hand below, and the details pane. Rework section 3's wireframe and navigation for layout C, keeping A's good parts: an always-on details pane, "Worn here now", lists that keep the slot visible, and stick moves that go to the neighbour they point at.
- **Q2, how it's built: (b), rebuilt in UI Toolkit** (not the recommended uGUI). Check the UI Toolkit skill and the project's existing UI first.
- **Q3:** Spectral for titles, Atkinson Hyperlegible for reading text.
- **Q4:** "Set the Hand" and "Shapes" fold into Loadout; reaction shapes move to a small Reactions page.
- **Q5:** only the carried weapon; the other weapons are set on the Way chart.
- **Q6:** simple icons painted by code first. This is a STAND-IN: ledger it in content_open.tsv so every spell, aspect and keystone gets real icon art in a later pass.
- **Q7:** Kenney's Input Prompts pack (CC0). It enters through staging and the asset intake, with ASSETS.csv rows.
- **Q8:** a text-size setting (100%, 125%, 150%) in this pass.

Later the same day, on the questions in 6.1 and the mockup:

- **N1: (a), drawn live.** The weapon's own 3D model, drawn behind the menu and slowly turning.
- **N2: (a), folding.** At 125% and 150% text, the half you are not in folds to a one-line summary (3.5.8).
- **N3: (a), Y.** Y on a slot or on a spell-list row cycles the spell's shape (3.5.4).
- **The mockup's look is approved**, with one fix: the South slot sat inside the saved Hands' row, which was
  confusing. The diamond and its surge now sit fully above the Hands, and the Hands are their own strip (3.5.2;
  `docs/design/mockups/loadout_c.html` and `.png` redrawn).

### 6.1 New questions from the layout C rework (2026-11-21; answered, see "Owner's answers")

The recommended option is first in each.

**N1. The weapon picture in the middle.**
- (a, recommended) Drawn live: the carried weapon's own model on a small lit stage behind the menu, in the game's
  cel shading, turning slowly. It always matches the game, shows every aspect's look as soon as its props are in, and
  needs no pictures kept up to date.
- (b) Still pictures rendered in Blender: one per weapon first, then one per weapon and aspect (about 35), each through
  staging for approval.

**N2. At 125% and 150% text, when both halves no longer fit.**
- (a, recommended) The half you are not in folds to a one-line summary, and LB, RB or moving off its edge opens it
  (3.5.8).
- (b) The whole screen scrolls up and down.
- (c) The weapon picture is hidden at those sizes and nothing folds.

**N3. Swapping a spell's shape** (left and right now always move, so A's "left or right swaps the shape" cannot stay).
- (a, recommended) Y on the slot, or on a row in the spell list.
- (b) Only in the spell list (Y on a row); a slot in the diamond has no shape button.
