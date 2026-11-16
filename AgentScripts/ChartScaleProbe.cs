using System.Collections;
using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the great chart at its planned size: fills the nine elements that have no tree yet, and the
// branches between them, with the pattern tree of docs/UPGRADE_SCHEMA.md (about 380 marks in all), then photographs
// the chart close and pulled back for three elements, walks it with the stick, and counts the garbage made each frame
// while the chart is open and while afflictions tick on the dummies. After its own lines it holds the chart open for
// twenty seconds and then the afflictions for twenty more, so the Profiler can be pointed at each. Run in the combat
// arena:  bash AgentScripts/runprobe.sh CombatTest ChartScaleProbe 60
public static class ChartScaleProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Chart Scale Probe").AddComponent<ChartScaleProbeRunner>();
        return "probe started";
    }
}

public class ChartScaleProbeRunner : MonoBehaviour
{
    private static readonly (string slot, float x, float y, UpgradeKind kind, string parent)[] Tree =
    {
        ("root", 0f, 1.2f, UpgradeKind.Root, ""),
        ("L1", -1.21f, 2.4f, UpgradeKind.Major, "root"), ("L2", -1.61f, 4.2f, UpgradeKind.Major, "L1"), ("LS", -2.0f, 6.0f, UpgradeKind.Spell, "L2"), ("LK", -2.4f, 7.8f, UpgradeKind.Keystone, "LS"),
        ("la", -1.98f, 3.42f, UpgradeKind.Minor, "L1"), ("lb", -2.4f, 5.23f, UpgradeKind.Minor, "L2"), ("lc", -1.2f, 4.97f, UpgradeKind.Minor, "L2"), ("ld", -2.8f, 7.03f, UpgradeKind.Minor, "LS"), ("le", -1.6f, 6.77f, UpgradeKind.Minor, "LS"),
        ("M1", 0f, 2.9f, UpgradeKind.Major, "root"), ("M2", 0f, 4.7f, UpgradeKind.Major, "M1"), ("MS1", 0f, 6.5f, UpgradeKind.Spell, "M2"), ("M3", 0f, 8.3f, UpgradeKind.Major, "MS1"), ("MK", 0f, 10.1f, UpgradeKind.Keystone, "M3"), ("MS2", 0.75f, 9.3f, UpgradeKind.Spell, "M3"),
        ("m1", 0.62f, 3.8f, UpgradeKind.Minor, "M1"), ("m2", -0.62f, 5.75f, UpgradeKind.Minor, "M2"), ("m3", 0.62f, 7.4f, UpgradeKind.Minor, "MS1"), ("m4", -0.62f, 9.2f, UpgradeKind.Minor, "M3"),
        ("R1", 1.21f, 2.4f, UpgradeKind.Major, "root"), ("R2", 1.61f, 4.2f, UpgradeKind.Major, "R1"), ("RS", 2.0f, 6.0f, UpgradeKind.Spell, "R2"), ("RK", 2.4f, 7.8f, UpgradeKind.Keystone, "RS"),
        ("ra", 1.98f, 3.42f, UpgradeKind.Minor, "R1"), ("rb", 2.4f, 5.23f, UpgradeKind.Minor, "R2"), ("rc", 1.2f, 4.97f, UpgradeKind.Minor, "R2"), ("rd", 2.8f, 7.03f, UpgradeKind.Minor, "RS"), ("re", 1.6f, 6.77f, UpgradeKind.Minor, "RS"),
        ("gift", 0f, 12f, UpgradeKind.Signature, ""),
    };

    private static readonly (string slot, float x, float y, UpgradeKind kind, string parent)[] Mix =
    {
        ("x1", 0f, 1f, UpgradeKind.Mix, ""), ("x2", 0.45f, 2f, UpgradeKind.Mix, "x1"), ("x3", -0.45f, 3f, UpgradeKind.Mix, "x2"), ("x4", 0.45f, 4f, UpgradeKind.Mix, "x3"),
        ("x5", -0.45f, 5f, UpgradeKind.Mix, "x4"), ("x6", 0.45f, 6f, UpgradeKind.Mix, "x5"), ("xs", 0f, 7.2f, UpgradeKind.Mix, "x6"), ("xgift", 0f, 8.6f, UpgradeKind.Signature, ""),
    };

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Shot(string name) => ScreenCapture.CaptureScreenshot("review/shots/current/chart_" + name + ".png");

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
    }

    private static void Fill(List<Upgrade> list)
    {
        int picture = 0;
        foreach (var element in Elements.All)
        {
            bool empty = element.Name != Elements.Fire && element.Name != Elements.Lightning;
            if (empty)
            {
                string prefix = "probe." + element.Name.ToLowerInvariant() + ".";
                foreach (var (slot, x, y, kind, parent) in Tree)
                    list.Add(new Upgrade
                    {
                        Id = prefix + slot, Name = element.Name + " " + slot, Element = element.Name, Kind = kind, X = x, Y = y,
                        Requires = parent == "" ? new string[0] : new[] { prefix + parent },
                        Icon = UpgradeIcons.All[picture++ % UpgradeIcons.All.Count],
                        Description = "A mark put here by the probe to fill the chart to its planned size.",
                        Form = kind == UpgradeKind.Spell ? SpellForms.Wave : "", Spell = kind == UpgradeKind.Spell ? new SpellSpec() : null,
                    });
            }
            // Plasma has marks of its own already.
            if (element.Name == Elements.Fire) continue;
            string mixPrefix = "probe." + element.MixName.ToLowerInvariant() + ".";
            foreach (var (slot, x, y, kind, parent) in Mix)
                list.Add(new Upgrade
                {
                    Id = mixPrefix + slot, Name = element.MixName + " " + slot, Element = element.Name, Element2 = element.MixWith, Kind = kind, X = x, Y = y, Cost = kind == UpgradeKind.Mix ? 4 : 0,
                    Requires = parent == "" ? new string[0] : new[] { mixPrefix + parent },
                    Icon = UpgradeIcons.All[picture++ % UpgradeIcons.All.Count],
                    Description = "A mark put here by the probe to fill the chart to its planned size.",
                });
        }
    }

    private IEnumerator Garbage(string label, int frames, System.Action eachFrame = null)
    {
        var allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        yield return null;
        long total = 0, most = 0;
        int clean = 0;
        for (int i = 0; i < frames; i++)
        {
            eachFrame?.Invoke();
            yield return null;
            long bytes = allocated.LastValue;
            total += bytes;
            if (bytes > most) most = bytes;
            if (bytes == 0) clean++;
        }
        allocated.Dispose();
        Say($"garbage, {label}: {frames} frames, {clean} with none, mean {total / (float)frames:0} bytes, worst {most}");
    }

    private IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var upgrades = player.GetComponent<PlayerUpgrades>();
        var element = player.GetComponent<PlayerElement>();
        var menu = UpgradeMenu.Instance;
        int before = Upgrade.All.Count;
        Upgrade.Gather(Fill);
        upgrades.CatalogueChanged();
        menu.Rebuild();
        yield return null;
        Say($"filled: {before} marks -> {Upgrade.All.Count}; effects {Upgrade.EffectCount}");
        yield return Garbage("the game with the chart closed and nothing happening (the editor's own share)", 120);

        foreach (string held in new[] { Elements.Fire, Elements.Water, Elements.Dark })
        {
            element.Take(ElementKit.ByName(held, false));
            upgrades.Forget();
            upgrades.Earn(40);
            yield return new WaitForSeconds(0.3f);
            menu.Open();
            yield return null;
            // Learn a path up the middle so the chart shows every state a mark can be in.
            string prefix = "probe." + held.ToLowerInvariant() + ".";
            foreach (string slot in new[] { "root", "M1", "m1", "M2", "MS1", "L1" })
            {
                if (!menu.Choose(prefix + slot)) continue;
                menu.LearnSelected();
            }
            if (held == Elements.Fire)
            {
                menu.Choose(Upgrade.Heat);
                menu.LearnSelected();
                menu.Choose(Upgrade.BurningCrescent);
                menu.LearnSelected();
                menu.Choose(Upgrade.MeteorLeap);
                menu.LearnSelected();
            }
            yield return new WaitForSecondsRealtime(0.8f);
            Shot(held.ToLowerInvariant() + "_close");
            yield return null;
            yield return null;
            // Walk outward as far as the stick goes, counting the marks passed.
            int steps = 0;
            string last = menu.Chosen != null ? menu.Chosen.Id : "";
            for (int i = 0; i < 20; i++)
            {
                yield return Tap(Key.W);
                if (menu.Chosen == null || menu.Chosen.Id == last) break;
                last = menu.Chosen.Id;
                steps++;
            }
            yield return new WaitForSecondsRealtime(0.7f);
            Shot(held.ToLowerInvariant() + "_crown");
            yield return null;
            yield return null;
            yield return Tap(Key.K);
            yield return new WaitForSecondsRealtime(1f);
            Shot(held.ToLowerInvariant() + "_ring");
            yield return null;
            yield return null;
            Say($"{held}: chart of {menu.MarkCount}; W walked {steps} marks outward to {last}; wide {menu.IsWide} zoom {menu.Zoom:0.000}");
            yield return Tap(Key.K);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return Tap(Key.Space);
            yield return new WaitForSecondsRealtime(0.3f);
        }

        // Garbage: the chart open and still, the chart moving between its two views, and afflictions ticking with their pips shown.
        menu.Open();
        yield return new WaitForSecondsRealtime(0.5f);
        yield return Garbage("the chart open and still", 180);
        int frame = 0;
        yield return Garbage("the chart open, pulled back and brought close again and again", 240, () =>
        {
            if (frame++ % 50 == 0) menu.ToggleWide();
        });
        if (menu.IsWide) menu.ToggleWide();
        menu.Close();
        yield return new WaitForSecondsRealtime(0.3f);
        var dummies = FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None);
        System.Action afflict = () =>
        {
            foreach (var dummy in dummies)
            {
                if (dummy.Afflictions.Stacks(AfflictionKind.Poison) > 0) continue;
                for (int kind = 1; kind < Afflictions.Count; kind++)
                    if (kind != (int)AfflictionKind.Chill) dummy.Afflictions.Apply((AfflictionKind)kind, 2);
            }
        };
        afflict();
        yield return new WaitForSeconds(0.3f);
        yield return Garbage($"ten afflictions ticking on each of {dummies.Length} dummies, pips shown", 240, afflict);

        // Held for the Profiler: twenty seconds of the chart, then twenty of the afflictions.
        Say("holding the chart open for the profiler");
        menu.Open();
        float until = Time.unscaledTime + 20f;
        float nextToggle = 0f;
        while (Time.unscaledTime < until)
        {
            if (Time.unscaledTime >= nextToggle)
            {
                nextToggle = Time.unscaledTime + 1.2f;
                menu.ToggleWide();
            }
            yield return null;
        }
        if (menu.IsWide) menu.ToggleWide();
        menu.Close();
        yield return null;
        Say("holding the afflictions for the profiler");
        until = Time.unscaledTime + 20f;
        while (Time.unscaledTime < until)
        {
            afflict();
            yield return null;
        }
        foreach (var dummy in dummies) dummy.Afflictions.Clear();
        Say("done");
        Destroy(gameObject);
    }
}
