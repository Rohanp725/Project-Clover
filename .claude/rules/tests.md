---
paths:
  - "Assets/_Project/Scripts/Tests/**"
---

# Tests

- EditMode tests (`Clover.Tests.EditMode`) check conventions and pure logic: they run in seconds and gate every session.
  PlayMode tests (`Clover.Tests.PlayMode`) load scenes and drive systems for a few frames; they are the smoke tests.
- The conventions suite (`ConventionsTests`) is the project's rules as code: no editor usings or legacy input in runtime,
  every art and audio file in the manifest, no broken shaders, no missing scripts in prefabs. When a rule is added to
  `.claude/rules/`, add the matching test if a scan can check it.
- Gameplay logic is tested on the plain C# classes, not MonoBehaviours; when a MonoBehaviour must be tested, build it in a
  fresh `GameObject` in the test and destroy it in `[TearDown]`.
- Run: `unity command test run -- --mode editor` (all EditMode) or `--filter ConventionsTests`; PlayMode only
  asynchronously: `test run -- --mode playmode --async-tests true` then poll `test status` until `completed`; `test list`
  to see names. Read `failed` and each failure's message; a failing selected test can return an opaque result, so narrow
  the filter and re-run.
- Performance assertions use `Unity.PerformanceTesting` (`[Performance]`, `Measure.Frames()`) with budgets from
  `docs/GAME.md`; keep them in `Tests/PlayMode/Performance/` and run them on request, not every session.
- Never `Assert.Ignore` to make a suite green; ignore only when the precondition is missing (no scene in Build Settings yet)
  and say so in the message.
