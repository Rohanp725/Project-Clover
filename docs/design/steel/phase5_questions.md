# Phase 5: questions for the owner

Choices the design left open while the Way, Memories and the Katana's Way were written (tasks 5.1, 5.6 and the data
side of 5.2, branch `phase5`, written offline on 2026-11-20 with no editor). Each was built with the default given here;
every one is easy to change.

## Task 5.1: the Way's rules and the wallet

1. **Where the run's Shards live.** The plan moves `RunState.Points` into the wallet in this task. Default: the wallet
   (`Growth/Wallet.cs`) is written and tested with both currencies, and Memories live there now (in the save, so they
   are kept through death, as decided), but the chart still spends the run's Shards from `RunState.Points` until the
   death rule (task L.3) is built. Moving them first would make Shards survive the old run end (a fall still ends the
   run in the hub) without the Bloodborne rule that is meant to put them at risk. Alternatives: move them now and let
   them persist across runs until L.3; or move them with the save's wiring of `Learned` (Track L).
2. **Gates in the playtest.** A technique waits for its elite, a temper rank for a count of gods. Default: the
   playtest opens every gate, as it opens every tree and sells the gods' gifts ("everything can be unlocked").
   Alternative: keep the gates in the playtest too, so testers meet the elites before their techniques.
3. **The first aspect and keystone bought.** The design says the aspect and the keystone are set at a rest fire.
   Default: buying a weapon's first aspect makes it the active one, and buying a bough's first keystone (or the Body's
   first bearing) makes it the set one, so nothing bought sleeps until the next fire; later ones wait to be chosen at a
   fire. Alternative: nothing is set by buying; the player always chooses at the fire (a bought mark may then do
   nothing at first).
4. **A Way mark with no weapon carried.** An element tree's mark that names a weapon counts any weapon when the
   loadout names none (an old leniency for tests and scenes with no weapon). Default: a Way mark needs its weapon kind
   carried, strictly; with no weapon nothing of any Way is in force (the Body still is). Alternative: the old leniency.
5. **The Armament's Way.** `weapons.md` makes the Armament the Fists' third aspect, but it is still its own weapon on
   the rack. Default: the Armament counts as the Fists' kind (`WayRules.KindOf`), so the Fists' trunk serves it.
   Alternative: keep it a kind of its own until its bough is written.
6. **How a stage gate is written.** `depth_weapons.md` 6 opens Temper III, IV and V at the third, fifth and seventh
   god. Default: a gate is either a progress flag (`elite.last_retainer`) or `gods.N`, open once N gods have fallen.
   The "one bough per weapon in the Fire land, a second at its god, a third at the third god" stage rule is not built:
   it counts boughs bought, not a mark's own gate.
   Alternatives: flags named for each stage (`stage.tier2`); or the bough count as a rule in `WayRules`.
7. **The Fists' two techniques before one is chosen.** Default: with no choice made, both techniques are in force and
   the chord takes the first (task 5.3 decides which). Alternative: neither works until one is chosen at a fire.

## Task 5.6: Memories income

1. **One payment per defeat.** An elite, a named fighter or a god is also the first of its kind. Default: it is paid as
   what it is (an elite 4), not also as a new kind (+1), so way_plan.md's land table adds up (the Fire land 21).
   Alternative: both, about one Memory more per elite and god.
2. **Where a god is paid.** A god fights in more than one body (Khepharu as a man, then the Sun-Bearer). Default: its
   bodies pay nothing as kinds; the god pays 8 once, under its fall's flag (`god.fire`), when the fall is reported.
   Alternative: pay through the last body's death.
3. **What wires the income in.** Default: the rules and their tests only (`Growth/Income.cs`, `IncomeTests`); nothing
   pays yet, because the hooks (`AreaDirector`'s deaths, the god directors' falls, a found-Memory pickup) want a probe
   in the editor. When wired, the Proving Ground's pens pay no Memories, as they pay no Shards (the playtest tops
   Memories up at a rest anyway). Alternative: wire `AreaDirector` now, unprobed.
4. **Named fighters, the Teacher and the last champion.** No enemy kind is any of these yet. Default: their prices are
   in `Income` (2, 4, 12) and `Income.TierOf` reads only gods and elites, with a `NEEDS` for a field on `EnemyKind` when
   the first is built.
5. **A new climb at 1.5 times** (`depth_weapons.md` 6) is not built; it waits for a second climb to exist.

## Task 5.2, the data side: aspects, the armoury and the Katana's Way

1. **How many marks move from the element trees.** The task names Burning Crescent, Delayed Cuts and Answering Draw;
   `weapons.md` 1 and `RetiredMarks.Moving` list five, with Drawing Wind (Wind) and Winter Draw (Ice). Default: all
   five are the Katana's element infusion marks, under their old ids, so the Way costs the 132 Memories way_plan.md 0.3
   gives it. Alternative: only the three; Drawing Wind and Winter Draw retired and refunded.
2. **A moved mark a save already holds.** Default (phase4_questions.md 2 kept): it stays learned, with no refund and no
   Memories charged, and is in force again on the Katana's trunk while its element is carried. Alternatives: refund its
   Shards now and leave the Way mark unbought; or charge nothing but make it sleep until bought again.
3. **Answering Draw on the Way.** It was Halo's (Metal and Light); on the trunk it is Light's, and the weapon's blows
   lay only the first element's affliction, so its notch (Sundered) lays only when Metal is first. Default: kept as
   built, under the blow rule. Alternatives: let it lay both, as a duo mark's blows may; or drop the notch.
4. **When the element infusion marks open.** Default: the four family marks and the five element ones are gated
   `gods.1` (depth_weapons.md 6: "the infusion marks" open at the Fire god, when an element is first owned); the
   element must also be carried for them to work. A save that holds one keeps it. Alternative: no gate.
5. **The names of the four family marks.** `weapons.md` names the families (Ground, Line, Weight, Draw), not the
   Katana's marks. Default: Lying Draw, Running Draw, Settling Draw, Dragging Draw; and each Way's tempers carry their
   weapon's name (Katana Temper I) so every mark's name stays its own. Alternative: other names.
6. **The god's bough's opening.** Default: given by Take (`Currency.None`, never sold, a playtest unlock as a god's gift
   is); carrying the Storm Katana itself (from the rack) makes his bough the active aspect without the mark; his
   bough's marks are bought with Memories as any. Alternative: in the playtest, the bough open only once its mark is
   unlocked.
7. **First pass of the moves.** Default (the plan's simplest version, 8.4): where a move can be said as steps it plays
   an existing clip, retimed: the Sheath's two-cut chain, A Hundred Days' one-cut chain (the drawing cut, 0.42 s), the
   Stream's five cuts (the turning cut on the return cut's clip, the rising cut on the drawing cut's), the Rising Draw
   and Stepping Water as the dodge's cut, Guard Cut as the guard's light attack, Kiri-otoshi's two-cut chain, the
   Storm Katana's third cut down a lane, The Red Draw as the held heavy on the Thunder Draw's clip. Everything else is
   its words and a `NEEDS` line (ledger lines `way-*` in content_open.tsv). Alternative: leave every move that needs
   its own clip doing nothing until the clip exists, so nothing looks wrong in play.
8. **The Red Draw's price.** The design's red draw costs 40 mana; a charged heavy is free in the engine. Default: the
   first pass is free (ledgered). Alternative: leave it out until the engine can charge for it.
9. **Numbers the design leaves out.** Defaults: the Stream's turning and rising cuts stagger as much as they cut (10
   and 14); Turned Edge adds a flat 8 to a perfect guard's stagger (the design's "25% more" needs a share); Kiri-otoshi's
   0.03 s wider window is +17% on the window (0.03 of the usual 0.18); Zanshin's 0.05 s is +28% for a second; the
   Storm Katana's third cut is 22 as written, not 22 and the god's 15% again; under A Hundred Days the draw is 1.3
   times whether or not Sharp Draw is bought. Alternatives: any other numbers; all are named constants.
10. **What the Way's details say.** Default: the auto account (`MarkDetails`), which now names the Way, the bough and
    the price in Memories; written details (`marks/way/katana.md`) come with the Way's chart (task 5.5). For a mark
    that is only words yet it still says "written by hand in the code", as Phase 4's unbuilt masters do.
11. **The Bond mark at the top of the trunk** is not written: it is given at Bond V (phase 6, task 6.1).
12. **Where the marks sit on the chart.** Default: X and Y left at zero until the Way's chart (`WayLayout`, task 5.5)
    places them; the ring and the heart never draw Way marks, and the chart tests leave them to `WayLayout`.
13. **The god's bough chosen on the plain Katana.** The owner's rule is that the player changes back and forth between
    the god's bough (his sword) and the plain boughs. Default: the loadout keeps the kind (`Weapon = "Katana"`) and the
    aspect (`katana.storm`), and `Armoury.Resolve` builds the Storm Katana from them; whatever equips the weapon (the
    editor half of 5.2, `PlayerBuild`) must equip what `Resolve` gives, not `Weapons.ByName(loadout.Weapon)`. The
    playtest's rack still hands over "Storm Katana" by name, which counts as his bough. Alternative: the loadout's weapon
    follows the choice (`Storm Katana` while his bough is active).
14. **Taking a new god's element under the old rule** (`GodReward`'s forget-and-refund, retired by task L.2). Default:
    the Way's and the Body's marks are neither refunded in Shards nor forgotten, since Memories are never lost (found in
    review: they would have been refunded as Shards and wiped).
