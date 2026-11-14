# Project Clover: the game

This file is the source of truth for what the game is. Claude reads it before designing anything and writes to it when a
decision is made. Empty sections mean the decision has not been made yet; do not invent an answer, ask.

## Pitch

One paragraph: who the player is, what they do, why it is tense or joyful, what makes it different.

## Pillars

Three to five sentences, each a thing every feature must serve. A feature that serves none is cut.

## Player fantasy and core loop

What the player does minute to minute, and the loop that brings them back (explore, gather, build, fight, talk...).

## Look

- Camera: (first person / third person / top-down), field of view, how close the player gets to surfaces.
- Style references: three to five, with what to take from each.
- Palette and lighting: time of day, mood, how much contrast.
- Fidelity targets: triangle budget per prop / character / scene, texture sizes by asset class, texel density, LOD policy.
- Performance budget: target resolution and frame time on the user's RTX 3080 (default 1080p at 16.6 ms, 0 B GC per frame).

## Sound

- Mix character: what sits in front (voice, music, world), how loud the quiet is.
- Music: style, instruments, how it reacts to play.
- Voice: who speaks, how written lines sound; consent rule for any cloned voice.

## Scope

- Platforms, players, session length, how long a run is.
- What is in the first playable, what is in the first playtest build, what is out.

## Decisions log

| date | decision | why |
|---|---|---|
| 2026-11-14 | Workflow set up before the game is designed; the Unity CLI is the only editor bridge | one bridge is safer and covers everything the project needs |
