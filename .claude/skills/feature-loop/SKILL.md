---
name: feature-loop
description: The way a gameplay or systems feature is built here - slice it, write the check first, implement in the right assembly, verify in the editor, review, then hand over. Use for any feature or fix bigger than a one-line tweak.
---

# Feature loop

1. **Slice**: restate the feature as the smallest thing the player would notice, plus the next two slices. Read
   `docs/GAME.md` for the pillar it serves; if it serves none, say so before building. Agree the slice with the user only when
   the readings differ materially; otherwise pick the careful default and proceed.
2. **Design in plain C#**: data and rules as plain classes in `Clover.Runtime` with no `UnityEngine` dependency where
   possible; MonoBehaviours only adapt them to the scene. Name the tuning values; they will be set live later.
3. **Check first**: an EditMode test for the rule (`Tests/EditMode/<Area>/<Thing>Tests.cs`) or, when it is about feel, a
   written play-mode probe plan (what input, what should be seen). The test fails before the code exists.
4. **Implement** in small steps, compiling after each (`unity --json recompile`). Statics get their reset. Input through
   actions. Scene wiring through builders or `unity command` calls, never YAML edits.
5. **Verify** with the `unity-check` skill: compile, console, tests, and a play-mode probe with a screenshot for anything
   visible. Tune live with `component set_field` or `[MethodReload]`, then write the final values back into code.
6. **Review**: run the code-reviewer agent on the diff; fix what it finds that is real. For an API you have not used in
   Unity 6000.6 before, ask the unity-api-checker agent before relying on it.
7. **Hand over**: tell the user what the slice does, how it was verified (with the screenshot path), the tuning values and
   where they live, and what the next slice would be. Offer a one-sentence commit message; commit only when asked.
