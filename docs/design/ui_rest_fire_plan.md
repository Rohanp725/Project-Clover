# The rest fire's screens: a UI pass for the Hand and the Way

Written by Claude on 2026-11-21 for the owner, from the owner's answer to the Way plan's Q9
(`docs/design/steel/way_build_plan.md`, "Owner's answers"): "There needs to be a pass that cleans up the ui completly
and makes it much more readable and understandable, and not just what the hand is right now. Also the way should be
changed on a similar screen to the hand, but the ui needs to be well designed, maybe even having them both on screen is
possible but do some reasearch on the best uis and whatnot to work with."

This is a plan only. Nothing here is built, and no decision in it is final until the owner answers section 6.

**Progress** (sections are committed one at a time; a section not ticked is not written yet)

- [x] 1. What exists now
- [x] 2. What the best build screens do (research)
- [x] 3. Three layouts for the Hand and the Way together
- [x] 4. A style guide for every menu
- [x] 5. The build plan and the checks
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

### 3.4 The recommendation

**Layout A.** It puts the Hand and the Way on one screen, as the owner asked, without tabs; it is the only layout
that keeps the details pane and the comparison in view at all times; and the rule "the list opens over the half you
are not changing" makes one way of choosing for both the spells and the aspects. It takes the parts of B and C that
are worth having: B's aspect cards (they slide in over the Element column) and C's weapon picture (a small one in the
Steel header, once the weapon renders exist). The rest menu becomes: **Loadout**, **The element chart**, **The Way
chart**, **Leave** (and **To the hub** once built); "Set the Hand" and "Shapes" fold into Loadout (question Q4).

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
- Text is drawn with TextMeshPro (signed-distance fonts stay sharp at every size), not the old `Text` (question Q2).

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
| Gold | `#FFCC73` | Titles, the focus frame, the set keystone's frame |
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

The pane sits in the same place on every screen that has one: on the right, 520 px wide, always describing the
focused thing. There are no floating tooltips; a pad cannot hover (P12).

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
- A (use) chooses or sets; B (dodge) backs out one level; LB and RB change page or weapon; X keeps (a Hand); the
  right stick scrolls the pane; L3 changes text size. The mouse clicks what it points at and the wheel scrolls; the
  keys follow the same map through `ControlNames`.
- The cursor slides in 0.12 s with a soft tick on the UI mixer group; nothing waits on an animation (P19).

### 4.6 Layout

- Panels on solid Panel colour, with the world dimmed to about 70% behind them; the HUD's bars, the spell diamond and
  the floor's words are hidden while a full-screen menu is open (1.2, 1.3).
- Layout by layout groups and anchors, not typed pixel positions, so 125% and 150% text reflow instead of
  overlapping.
- Keep every word at least 64 px from the screen's edge.
- The same frame sprite, title position and footer on every screen.

---

## 5. The build plan and the checks

Assumes the recommended answers in section 6. Each step is one `feature-loop` slice: the check first, the code in the
right assembly, verified in the editor, then handed over. The Katana is the first weapon the Steel half shows, as the
Way plan builds it first.

### 5.1 The order

| Step | What | Replaces | Needs first |
|---|---|---|---|
| U0 | **The menu kit.** `MenuStyle` (the colours and sizes of section 4, one place); a kit that makes a panel, a TextMeshPro label, a focus frame, a glyph and a pane by name; `FocusGrid`, a plain class that holds each place's up, down, left and right neighbour (no wrapping); one shared reader for the stick, d-pad, repeat, mouse and wheel | `MenuParts.cs`, the copies in `GuidePage.cs:190`, the four screens' own `Update` loops | The body font (5.2); a check that TextMeshPro ships inside uGUI 2.0 in Unity 6000.6 (the `unity-api-checker` agent) |
| U1 | **Icons and glyphs.** The five role icons and the first-pass spell icons painted in `UiArt`; button glyphs (5.2) | Role words, initials | U0 |
| U2 | **The details pane and the words.** One `Describe` for spells, marks, aspects, keystones and bearings, giving the lines of 4.4; a "what it does" line for every spell in the Hand's lists (today an element's own spell says only "Fire's own spell", 1.3 item 1); each missing line ledgered in `content_open.tsv` until written | `HandMenu.Describe` (612 to 630), the playtest-only details page | U0 |
| U3 | **Loadout, the Element half.** The diamond on `FocusGrid`, the list that slides over the Steel half, shapes swapped with left and right, the role strip, the Hands row | `HandMenu.cs`; the spell part of `ShapeMenu.cs` | U0 to U2 |
| U4 | **Loadout, the Steel half.** Aspect cards, the keystone and bearing pairs, technique, Temper and the infused strikes shown | Nothing (new); closes `way_build_plan.md` Q9 | `way_build_plan.md` 2.0 (`PlayerBuild.Rearm`), so a set aspect changes the sword; 2.4 for bearings |
| U5 | **The rest menu.** Loadout, the element chart, the Way chart, (to the hub), leave; restyled with the kit | `RestChoices` entries, `RestMenu.cs:139` | U3 |
| U6 | **The charts.** The element chart's right panel becomes the pane; its hint lines become glyphs; the details stop being playtest-only; the "next slot round" stopgap goes. The Way chart (`way_build_plan.md` 2.5) is built on the kit from the start | `UpgradeMenu.cs:530` to 537, 814 to 830 | U0 to U2 |
| U7 | **Pause, controls, guide and reading pages** restyled; the text size setting (100, 125, 150%) | Their own colour copies | U0 |
| U8 | Later: the HUD's spell diamond wears the spell icons; the aspect cards' large pictures from Blender | `SpellDiamondHud.cs:169` initials | U1; weapon renders |

U0 to U3 can start now. U4 waits on the Way's wiring (step 2.0 of the Way plan), which is the Way plan's own first
step anyway.

### 5.2 What is needed from outside the code

- **Fonts**: Atkinson Hyperlegible (SIL Open Font Licence), downloaded to `staging/fonts/`, pinned to one release,
  brought in through the `asset-intake` skill with its `ASSETS.csv` row; its TextMeshPro font asset, and Spectral's,
  made by an editor builder into `Assets/_Project/Generated/` under stable names, Latin characters only.
- **Button glyphs**: Kenney's "Input Prompts" pack (CC0, free, no account) pinned to the release downloaded, through
  staging and intake; or painted by code in `UiArt` (question Q7).
- **Icons**: painted by code in `UiArt`, as the chart's are: no outside art for U1. The aspect, keystone and bearing
  icons are added weapon by weapon as each Way is built (the Katana's 4 aspects, 8 keystones and the Body's 2 first).
- **Pictures for the aspect cards** (later, U8): renders of the approved weapon models in each aspect's look, from
  Blender through the `blender-prop` skill, into staging for the owner to approve.
- **A sound**: one soft tick for the cursor and one for setting a thing, through `audio-intake` into
  `Audio/UI`, on the UI mixer group, meeting `Loudness.cs`.

### 5.3 How each screen is checked

Every screen, before the owner sees it (the `unity-check` skill, then the `playmode-probe` loop; probes run with saved
progress sandboxed, never wiping it):

1. **Tests (EditMode)**:
   - `FocusGrid` for each screen: every place reachable from the opening place; pushing a direction and then its
     opposite comes back; nothing wraps; no place is a dead end.
   - `MenuStyle`: every text-on-background pair in use is at least 4.5 to 1 (computed from the colours, not by eye).
   - Built menu: no TextMeshPro label under 24 px at 100%.
   - `Describe`: every spell, aspect, keystone and bearing has a name, a kind line, a "what it does" line and a cost
     line.
2. **The words**: every "what it does" line is 15 words or fewer per sentence and reads at grade 8 or below on a
   Flesch-Kincaid count (a small Python script in `AgentScripts/`, with `textstat` pinned to an exact version).
3. **Screenshots** (`capture game_view`) at **1920x1080 and 1280x720**, and once at **150% text**, of each state: the
   screen opened; choosing a spell; choosing an aspect; a locked keystone; Devotion on (all in); on pad glyphs and on
   key glyphs. Named `review/shots/current/ui_<screen>_<state>_<width>.png`. Old and new are shown side by side to the
   owner. A golden frame is taken only when the owner accepts the look.
4. **A readability read** of every shot, by the `shot-reviewer` agent and by eye, against a list: the smallest words
   at 720p are 16 px or more; nothing clipped, overlapping or running off a box; the focused thing is obvious with the
   colour taken away (the shot turned grey); every press has its glyph next to what it does; nothing pure white.
5. **The pad walk**: in Play Mode, the probe opens the fire and the loadout, walks the d-pad over every place (logging
   each one reached), opens every list, sets a spell, swaps a shape, wears and keeps a Hand, sets an aspect, a
   keystone and a bearing, and backs out with B from every depth. It checks the build changed through `script eval`
   (the loadout's slots, `WayRules.ActiveAspect`) and that the sword was re-armed. Then the same with the mouse:
   every click lands on what it points at.
6. **Performance**: with the loadout open and the cursor moving, `diagnostics profiler_record` shows no steady
   per-frame allocation (`profiler_summary`).
