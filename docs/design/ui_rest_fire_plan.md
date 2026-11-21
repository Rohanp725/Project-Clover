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
