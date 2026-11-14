# AgentScripts

C# files here are **not** part of the Unity project (the folder is outside `Assets/`). The Unity CLI compiles one in memory
and runs a named static entry point, with no domain reload:

```
unity command script run --file AgentScripts/Example.cs --entry Example.Run
unity command script run --file AgentScripts/Example.cs --entry Example.Run --args '[3, "Green"]'
unity command script run --file AgentScripts/Example.cs --dry-run true      # compile only
```

Use this for bulk construction (build a test scene, place a hundred props, wire fields), one-off probes in Play Mode, and
anything longer than a `script eval` one-liner. The code sees the project's assemblies (`Clover.Runtime`, `Clover.Editor`),
`UnityEditor` and the active editor defines. Exceptions come back with `file:line` in your source.

Rules: one static class per file, one entry point, delete the file when the job is done or move it into
`Assets/_Project/Scripts/Editor/` as a proper menu tool once it is worth keeping. Compiled leftovers (`*.dll`, `*.pdb`) are
git-ignored.
