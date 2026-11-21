using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for Phase 3.6: every penned enemy of the Proving Ground let out at once round the player, everyone kept
// alive, and two afflictions laid on two of them every half second (about four reactions a second, a heavy real fight) so reactions go off all over the crowd for 20 s.
// Reports frame time, the peak of the player's ground (Hazards) against its capacity, the peak of reactions waiting
// against the engine's queue, and how many reactions were shown. GC is read by the profiler's own counter, which in the
// editor includes the editor's own windows; read a capture's markers for the game's share.
// bash AgentScripts/runprobe.sh ProvingGround CrowdProbe 80
public static class CrowdProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Crowd Probe").AddComponent<CrowdProbeRunner>();
        return "probe started";
    }
}

public class CrowdProbeRunner : MonoBehaviour
{
    private const float Seconds = 20f;
    private const float LayEvery = 0.5f;
    private const int LaysEachTime = 2;

    private PlayerDefence defence;
    private readonly List<Enemy> crowd = new List<Enemy>();
    private int ok, wrong;

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

    // The Proving Ground opens on the guide, which stops the game: read it through first.
    private static IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
        // A guide opened as a list (the playtest's guide since Phase 4b) turns its pages on E and never closes on it:
        // the back key closes it, and failing that it is closed outright, so time is never left stopped.
        if (GuidePage.Showing) yield return Tap(Key.Space);
        if (GuidePage.Showing) GuidePage.Instance.Close();
        if (Time.timeScale <= 0f) Debug.Log("[Probe] WRONG: time still stopped after the guide");
    }

    private void Update()
    {
        if (defence != null) defence.Health.Heal(1000f);
        for (int i = 0; i < crowd.Count; i++)
            if (crowd[i] != null && !crowd[i].IsDead) crowd[i].Health.Heal(1000f);
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        var backgroundWas = InputSystem.settings.backgroundBehavior;
        var editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        float waited = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        yield return ReadAll();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Judge("the game is running (the guide read through, time not stopped)", Time.timeScale > 0.5f && !GuidePage.Showing, $"time scale {Time.timeScale}");
        var player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        var reactions = player.GetComponent<PlayerReactions>();

        foreach (var pen in FindObjectsByType<EnemyPen>(FindObjectsInactive.Include))
        {
            pen.Release(Enumerable.Range(0, pen.Enemies.Length).ToArray());
            crowd.AddRange(pen.Enemies.Where(enemy => enemy != null));
        }
        yield return new WaitForSecondsRealtime(0.5f);
        // Round the player, in two rings, so they fight and every reaction's ground lands among them.
        Vector3 centre = player.transform.position;
        for (int i = 0; i < crowd.Count; i++)
        {
            float angle = i * Mathf.PI * 2f / Mathf.Max(1, crowd.Count) * 3f;
            float radius = i % 2 == 0 ? 3f : 6f;
            crowd[i].transform.position = centre + new Vector3(Mathf.Cos(angle) * radius, 0.05f, Mathf.Sin(angle) * radius);
        }
        Say($"crowd: {crowd.Count} enemies let out of {FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).Length} pens, hazard capacity {FindAnyObjectByType<Hazards>()?.Capacity}, reaction queue {ReactionEngine.MostWaiting}, most per frame {ReactionEngine.Shared.MostPerFrame}");

        var allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        var frames = new float[20000];
        int count = 0, hazardPeak = 0, waitingPeak = 0, shownBefore = TotalShown(reactions);
        long garbage = 0;
        var random = new System.Random(19);
        float end = Time.unscaledTime + Seconds, nextLay = 0f;
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
            hazardPeak = Mathf.Max(hazardPeak, Hazards.Alive);
            waitingPeak = Mathf.Max(waitingPeak, ReactionEngine.Shared.Waiting);
            garbage += allocated.LastValue;
        }
        allocated.Dispose();
        int shown = TotalShown(reactions) - shownBefore;
        var sorted = frames.Take(count).OrderBy(ms => ms).ToArray();
        float mean = sorted.Average(), p95 = sorted[(int)(sorted.Length * 0.95f)], worst = sorted[sorted.Length - 1];
        int standing = crowd.Count(enemy => enemy != null && !enemy.IsDead && enemy.gameObject.activeInHierarchy);
        Say($"perf crowd: {count} frames in {Seconds} s; frame ms mean {mean:0.0} p95 {p95:0.0} worst {worst:0.0}; garbage {garbage / Mathf.Max(1, count)} bytes a frame (editor included); reactions shown {shown}; enemies standing {standing} of {crowd.Count}");
        var hazards = FindAnyObjectByType<Hazards>();
        int capacity = hazards != null ? hazards.Capacity : 0;
        Judge("the frame stays under the 16.6 ms budget at the 95th percentile", p95 < 16.6f, $"p95 {p95:0.0} ms");
        Judge("the player's ground never fills (a full ring drops the oldest patch)", hazardPeak < capacity, $"peak {hazardPeak} of {capacity}");
        Judge("reactions never queue to the brim", waitingPeak < ReactionEngine.MostWaiting, $"peak waiting {waitingPeak} of {ReactionEngine.MostWaiting}");
        Judge("reactions went off throughout", shown > 40, $"{shown} shown");
        // Nothing laid now: ground that ends must end, or the ring is leaking.
        int atEnd = Hazards.Alive;
        float quietUntil = Time.unscaledTime + 12f;
        while (Time.unscaledTime < quietUntil) yield return null;
        Judge("12 s after the last laying the ground has gone", Hazards.Alive <= 4, $"alive {atEnd} at the end, {Hazards.Alive} after 12 s");
        // What is still there, read from the ring itself.
        var field = typeof(Hazards).GetField("zones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (hazards != null && field?.GetValue(hazards) is System.Array zones)
        {
            var kinds = new Dictionary<string, int>();
            foreach (var zone in zones)
            {
                var type = zone.GetType();
                if (!(bool)type.GetField("Active").GetValue(zone)) continue;
                var spec = (HazardSpec)type.GetField("Spec").GetValue(zone);
                float left = (float)type.GetField("Left").GetValue(zone), delay = (float)type.GetField("Delay").GetValue(zone);
                string key = $"{spec.Kind} seconds {spec.Seconds:0.#} left {(left > 1000f ? "huge" : left.ToString("0"))} delay {delay:0.#} follow {spec.Follow} grown {spec.Grown} once {spec.Once} bites {spec.Bites} reaction {spec.FromReaction}";
                kinds[key] = kinds.TryGetValue(key, out int n) ? n + 1 : 1;
            }
            foreach (var pair in kinds.OrderByDescending(entry => entry.Value).Take(8)) Say($"   still alive x{pair.Value}: {pair.Key}");
        }
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }

    private static int TotalShown(PlayerReactions reactions)
    {
        int total = 0;
        for (int pair = 0; pair < AfflictionPairs.Count; pair++) total += reactions.TimesShown(pair);
        return total;
    }
}
