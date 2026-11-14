---
paths:
  - "Assets/_Project/Scripts/Runtime/**"
---

# Runtime code (assembly Clover.Runtime)

- Namespaces are `Clover.<Area>` matching the folder (`Clover.Core`, `Clover.Player`, `Clover.World`, `Clover.Gameplay`,
  `Clover.UI`, `Clover.Audio`). One public type per file, file named after the type.
- No `using UnityEditor` here, even inside `#if UNITY_EDITOR`; editor-only behaviour goes in `Clover.Editor` and reaches
  runtime types through public methods. `ProjectChecks` and the EditMode tests fail on it.
- Input only through `UnityEngine.InputSystem`: read actions from the `Controls.inputactions` asset (generated C# wrapper or
  `InputActionReference`); never `Input.GetKey`, `Input.GetAxis`, `Input.mousePosition`.
- Statics: domain reload is off, so every mutable static field, event subscription and cached instance gets a reset in a
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` method in the same class. Stamps from a
  previous play (frame counts, times) are the usual culprit when "nothing responds" after the second play.
- MonoBehaviours are thin: serialized references in, events out, logic in plain C# classes that tests can construct. Prefer
  `[SerializeField] private` fields with a public property; expose tuning values as fields so `script eval` and
  `component set_field` can change them live.
- No `Find*`, `GetComponent` or allocation in per-frame code; cache in `Awake`. Steady-state GC allocation is zero per frame
  (check with `diagnostics profiler_summary`). Frame budget: 16.6 ms at 1080p on the user's RTX 3080 unless `docs/GAME.md`
  says otherwise.
- Async: use `Awaitable` (Unity 6) for waits and loads; coroutines only where an API demands them. Cancel on destroy.
- Rendering: URP only. Post-processing through Volume profiles; custom effects as Renderer Features or Shader Graph, never
  `OnRenderImage`. Shaders live in `Art/Shaders` with HLSL includes next to them.
- Audio plays through an `AudioSource` routed to the right `AudioMixer` group (Master > Music / Ambience / SFX / Voice / UI);
  clips are referenced, not loaded by string, unless a catalogue exists.
- Logging: `Debug.Log($"[Clover] ...")` with the area in the brackets (`[Clover.Player]`) so console filters work; errors are
  for things a tester must not see.
- Tuning loops: tag a method `[MethodReload]` (from `Unity.Pipeline`) when iterating on its body in Play Mode with
  `script.methodreload reload`; remove the tag before committing unless the method is a known tuning point.
- Text the player reads at their own pace goes through a click-through reading UI; `ShowMessage`-style timed text is for
  short status lines and scares. Tips appear only after the player could have noticed the thing and say what to do.
