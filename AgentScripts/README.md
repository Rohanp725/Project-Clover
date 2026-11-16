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

## Probes

`runprobe.sh <SceneName> <ProbeName> [maxPolls]` recompiles, opens the scene, enters Play Mode, runs
`AgentScripts/<ProbeName>.cs` (entry `<ProbeName>.Run`), waits for its `[Probe] done` line, prints the probe's lines and
any console warnings or errors, and stops Play Mode. Scene names: `Hub`, `Castle`, `Summit`, `CombatTest`.

| Probe | Scene | What it plays |
|---|---|---|
| `RunProbe` | Hub | one whole run through every door: gate, castle cleared, sealed way, both forms of the god, the choice, home |
| `HubProbe` | Hub | the sponsor's words, the stands and braziers sealed and open, the gate, a fall and waking |
| `CastleProbe` | Castle | fists, spell, surge, a finisher, the caster, a storm strike, the Katana, the rest fire and its menu, a fall |
| `CaptainProbe` | Castle | the Gate Captain: every attack, guard, perfect guard, second phase, finisher, death |
| `SummitProbe` | Summit | the swordsman, then the storm itself: rising, attacks, a hand cut, the head finished, the rewards |
| `StormProbe` | Summit | the storm itself in detail: entrance, each attack hit and avoided, openings, fury, death |
| `UpgradeProbe` | CombatTest | the upgrade menu and every Fire and Lightning upgrade, Storm Step and Plasma |
| `FeedbackProbe` | Castle | death burn-away, finish mark, target brackets, bars, low-health edge, cut marks |
| `FinisherProbe` | Castle | the Katana's finisher and its camera |
| `PauseProbe` | Hub | the pause menu: every entry, pausing mid-attack and mid-roll, giving up, forgetting |
| `TipsProbe` | Castle | the three first-fight tips |
| `SoakProbe` | any | random play for minutes; mode, seed and seconds are read from `%TEMP%/soak_config.txt` |
| `TourProbe`, `TitleProbe` | any | photographs of each scene's places and its title card, to `review/shots/current/` |
| `PerfProbe` | any | ten seconds of frame time, garbage and draw calls |

Probes that touch saved progress forget it before and after, so the first real start still shows the first waking.
