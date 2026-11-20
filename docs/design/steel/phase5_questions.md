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
