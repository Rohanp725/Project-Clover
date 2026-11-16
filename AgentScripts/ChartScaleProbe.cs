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

// Play Mode probe for the great chart at its real size (the eleven trees and eleven branches of the tree files): for
// every element it opens the chart, learns the road to the first spell, photographs the tree close, at its crown and,
// for Fire, Wind, Water and Dark, pulled back to the whole ring; walks it with the stick; and counts the garbage made
// each frame while the chart is open and while afflictions tick on the dummies. After its own lines it holds the chart
// open for eight seconds and then the afflictions for eight more, so the Profiler can be pointed at each. Run in the
// combat arena:  bash AgentScripts/runprobe.sh CombatTest ChartScaleProbe 70
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
        Say($"the chart: {Upgrade.All.Count} marks; effects {Upgrade.EffectCount}");
        yield return Garbage("the game with the chart closed and nothing happening (the editor's own share)", 120);

        foreach (var info in Elements.All)
        {
            string held = info.Name;
            element.Take(ElementKit.ByName(held, false));
            upgrades.Forget();
            RunState.Points = 0;
            upgrades.Earn(19);
            yield return new WaitForSeconds(0.3f);
            menu.Open();
            yield return null;
            // Learn the road to the first spell so the chart shows every state a mark can be in.
            Upgrade spell = null;
            foreach (var mark in Upgrade.All)
                if (mark.Element == held && !mark.IsMix && mark.Kind == UpgradeKind.Spell && spell == null) spell = mark;
            var road = new List<Upgrade>();
            for (var at = spell; at != null; at = at.Requires.Length > 0 ? Upgrade.ById(at.Requires[0]) : null) road.Insert(0, at);
            int learned = 0;
            foreach (var mark in road)
            {
                if (!menu.Choose(mark.Id)) continue;
                menu.LearnSelected();
                if (upgrades.Owns(mark.Id)) learned++;
            }
            menu.Choose(road[0].Id);
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
            bool wide = false;
            float zoom = menu.Zoom;
            if (held == Elements.Fire || held == Elements.Wind || held == Elements.Water || held == Elements.Dark)
            {
                yield return Tap(Key.K);
                yield return new WaitForSecondsRealtime(1f);
                Shot(held.ToLowerInvariant() + "_ring");
                yield return null;
                yield return null;
                wide = menu.IsWide;
                zoom = menu.Zoom;
                yield return Tap(Key.K);
                yield return new WaitForSecondsRealtime(0.6f);
            }
            Say($"{held}: chart of {menu.MarkCount}; learned {learned} of the {road.Count} on the road to {spell.Name} with {19 - upgrades.Points} of 19 embers; W walked {steps} marks outward to {last}; wide {wide} zoom {zoom:0.000}");
            yield return Tap(Key.Space);
            yield return new WaitForSecondsRealtime(0.3f);
        }
        element.Take(ElementKit.Fire());
        upgrades.Forget();
        yield return new WaitForSeconds(0.3f);

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

        // Held for the Profiler: eight seconds of the chart, then eight of the afflictions.
        Say("holding the chart open for the profiler");
        menu.Open();
        float until = Time.unscaledTime + 8f;
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
        until = Time.unscaledTime + 8f;
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
