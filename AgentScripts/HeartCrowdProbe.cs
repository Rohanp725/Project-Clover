using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The Phase 4 merge's profiler pass (phase4_merge_plan.md, steps 3 and 4): CrowdProbe's crowd fight with the playtest on,
// every duo branch's mark learned and every reaction shape chosen (alternately the first and second), the carried pair
// turned every 6 s through The Grudge (Dark and Lightning: Given Bones, The Exile Returns, Ash and Thunder), Ill Wind
// (Poison and Wind: No Still Air), Kindling Point (Light and Fire) and Plasma (Fire and Lightning), so the marks that
// answer every blow and set-off are all in force at some time. Writes "soak start" for the profiler, then frame time,
// garbage a frame (the editor's share included) and the reactions shown; nothing may run away (the queue never full).
// bash AgentScripts/profile_heart.sh
public static class HeartCrowdProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Heart Crowd Probe").AddComponent<HeartCrowdProbeRunner>();
        return "probe started";
    }
}

public class HeartCrowdProbeRunner : MonoBehaviour
{
    private const float SecondsEachPair = 6f;
    private const float LayEvery = 0.5f;
    private const int LaysEachTime = 2;

    private static readonly string[][] Pairs =
    {
        new[] { Elements.Dark, Elements.Lightning },
        new[] { Elements.Poison, Elements.Wind },
        new[] { Elements.Light, Elements.Fire },
        new[] { Elements.Fire, Elements.Lightning },
    };

    private PlayerDefence defence;
    private readonly List<Enemy> crowd = new List<Enemy>();
    private int ok, wrong;
    private bool playtestWas;

    private static void Say(string line) => Debug.Log("[Probe] " + line);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}: {detail} {(good ? "OK" : "WRONG")}");
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    private static IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private void Update()
    {
        if (defence != null) defence.Health.Heal(1000f);
        for (int i = 0; i < crowd.Count; i++)
            if (crowd[i] != null && !crowd[i].IsDead) crowd[i].Health.Heal(1000f);
    }

    private void OnDestroy() => PlaytestMode.Set(playtestWas);

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        var backgroundWas = InputSystem.settings.backgroundBehavior;
        var editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        playtestWas = PlaytestMode.On;
        PlaytestMode.Set(true);
        float waited = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        yield return ReadAll();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        var player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        var reactions = player.GetComponent<PlayerReactions>();
        var upgrades = player.GetComponent<PlayerUpgrades>();
        var build = player.GetComponent<PlayerBuild>();
        Judge("progress is sandboxed", Progress.Sandboxed);

        // Every duo branch's mark learned, every reaction shape chosen.
        int given = 0, shaped = 0;
        foreach (var mark in Upgrade.All)
            if (mark.IsMix && !mark.IsHidden && upgrades.Give(mark)) given++;
        for (int pair = 0; pair < AfflictionPairs.Count; pair++)
            if (ReactionTable.ShapeMarkFor(pair) != null && build.ChooseShape(ReactionTable.ShapeKey(pair), pair % 2, true)) shaped++;
        upgrades.RefreshNow();
        Judge("the duo branches' marks are learned and the shapes chosen", given > 100 && shaped > 20, $"{given} marks, {shaped} shapes");

        foreach (var pen in FindObjectsByType<EnemyPen>(FindObjectsInactive.Include))
        {
            pen.Release(Enumerable.Range(0, pen.Enemies.Length).ToArray());
            crowd.AddRange(pen.Enemies.Where(enemy => enemy != null));
        }
        yield return new WaitForSecondsRealtime(0.5f);
        Vector3 centre = player.transform.position;
        for (int i = 0; i < crowd.Count; i++)
        {
            float angle = i * Mathf.PI * 2f / Mathf.Max(1, crowd.Count) * 3f;
            float radius = i % 2 == 0 ? 3f : 6f;
            crowd[i].transform.position = centre + new Vector3(Mathf.Cos(angle) * radius, 0.05f, Mathf.Sin(angle) * radius);
        }
        Say($"crowd: {crowd.Count} enemies; reaction queue {ReactionEngine.MostWaiting}");
        Say("soak start");

        var allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        var frames = new float[30000];
        int count = 0, waitingPeak = 0, shownBefore = reactions.ShownCount;
        long garbage = 0;
        var random = new System.Random(19);
        for (int p = 0; p < Pairs.Length; p++)
        {
            build.Attune(Pairs[p][0], Pairs[p][1]);
            upgrades.RefreshNow();
            int inForce = Upgrade.All.Count(mark => mark.IsMix && upgrades.Has(mark.Id));
            int shownAtPair = reactions.ShownCount;
            float end = Time.unscaledTime + SecondsEachPair, nextLay = 0f;
            while (Time.unscaledTime < end)
            {
                if (Time.unscaledTime >= nextLay)
                {
                    nextLay = Time.unscaledTime + LayEvery;
                    for (int n = 0; n < LaysEachTime; n++)
                    {
                        var enemy = crowd[random.Next(crowd.Count)];
                        if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy) continue;
                        var first = (AfflictionKind)random.Next(1, 12);
                        var second = (AfflictionKind)random.Next(1, 12);
                        if (second == first) continue;
                        enemy.Afflictions.Lay(first, 1 + random.Next(3), 0f, 1f, LaySource.Spell, enemy);
                        enemy.Afflictions.Lay(second, 1 + random.Next(3), 0f, 1f, LaySource.Spell, enemy);
                    }
                }
                yield return null;
                if (count < frames.Length) frames[count++] = Time.unscaledDeltaTime * 1000f;
                waitingPeak = Mathf.Max(waitingPeak, ReactionEngine.Shared.Waiting);
                garbage += allocated.LastValue;
            }
            Say($"{Pairs[p][0]} and {Pairs[p][1]}: {inForce} duo marks in force, {reactions.ShownCount - shownAtPair} reactions shown in {SecondsEachPair} s");
        }
        allocated.Dispose();
        int shown = reactions.ShownCount - shownBefore;
        var sorted = frames.Take(count).OrderBy(ms => ms).ToArray();
        float mean = sorted.Average(), p95 = sorted[(int)(sorted.Length * 0.95f)], worst = sorted[sorted.Length - 1];
        Say($"perf heart crowd: {count} frames; frame ms mean {mean:0.0} p95 {p95:0.0} worst {worst:0.0}; garbage {garbage / Mathf.Max(1, count)} bytes a frame (editor included); reactions shown {shown}");
        Judge("the frame stays under the 16.6 ms budget at the 95th percentile", p95 < 16.6f, $"p95 {p95:0.0} ms");
        Judge("reactions never queue to the brim (no runaway cascade)", waitingPeak < ReactionEngine.MostWaiting, $"peak waiting {waitingPeak} of {ReactionEngine.MostWaiting}");
        Judge("reactions went off throughout", shown > 40, $"{shown} shown");
        build.Attune(Elements.Fire, "");
        upgrades.Forget();
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
