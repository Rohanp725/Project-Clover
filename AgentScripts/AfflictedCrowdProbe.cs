using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the owner's second effects review: every penned enemy of the Proving Ground let out round the
// player, kept still and alive, every one carrying the looks changed in this pass at once (Burn in real flame, Static
// with its arcs off the body, Cursed's pull into the hole, Tremor's dust and grit), laid again every two seconds so they
// never run out, and the player surging in Metal (Temper's steel) and then Lightning (Overdrive's arcs). Prints
// "[Probe] soak start" when the crowd is afflicted, so diagnostics profiler_record can be pointed at it, then holds for
// SoakSeconds. Reports the frame time (mean, p95, worst), the garbage a frame read from the profiler's own counter (in
// the editor that includes the editor's windows), the bodies burning in real flame and the steel level.
// bash AgentScripts/runprobe.sh ProvingGround AfflictedCrowdProbe 80
public static class AfflictedCrowdProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Afflicted Crowd Probe").AddComponent<AfflictedCrowdProbeRunner>();
        return "probe started";
    }
}

public class AfflictedCrowdProbeRunner : MonoBehaviour
{
    private const float SoakSeconds = 24f;
    // The looks of the second review and the rebuild from the Blender worker's batch 2: Burn's flame field, Static's arcs (ribbons),
    // Cursed's pull, Tremor's cracks and pebbles, Unsteady's curls (ribbons) and Poisoned's clouds.
    private static readonly AfflictionKind[] Changed = { AfflictionKind.Burn, AfflictionKind.Static, AfflictionKind.Cursed, AfflictionKind.Tremor, AfflictionKind.Unsteady, AfflictionKind.Poison };
    // The crowd the budget is judged on (the overnight brief of 2026-11-20: twelve afflicted enemies).
    private const int CrowdSize = 12;

    private PlayerDefence defence;
    private readonly List<Enemy> crowd = new List<Enemy>();

    private static void Say(string line) => Debug.Log("[Probe] " + line);
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

    private void Update()
    {
        if (defence != null) defence.Health.Heal(1000f);
        foreach (var enemy in crowd)
        {
            if (enemy == null || enemy.IsDead) continue;
            enemy.Health.Heal(1000f);
            enemy.Brain.AggroRange = 0f;
            enemy.Brain.LeashRange = 0.5f;
        }
    }

    private void LayAll(int round)
    {
        for (int i = 0; i < crowd.Count; i++)
        {
            var enemy = crowd[i];
            if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy) continue;
            // Two of the changed looks on each, turning round the crowd, so every look is on several bodies at once.
            var first = Changed[(i + round) % Changed.Length];
            var second = Changed[(i + round + 1) % Changed.Length];
            enemy.Afflictions.Lay(first, 2 + i % 2, 3f, 1f, LaySource.Spell, enemy);
            enemy.Afflictions.Lay(second, 1 + i % 3, 3f, 1f, LaySource.Spell, enemy);
        }
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
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);

        var player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        var element = player.GetComponent<PlayerElement>();
        var build = player.GetComponent<PlayerBuild>();
        var combat = player.GetComponent<PlayerCombat>();
        foreach (var pen in FindObjectsByType<EnemyPen>(FindObjectsInactive.Include))
        {
            pen.Release(Enumerable.Range(0, pen.Enemies.Length).ToArray());
            crowd.AddRange(pen.Enemies.Where(enemy => enemy != null));
        }
        // Exactly twelve: the rest are put away.
        for (int i = CrowdSize; i < crowd.Count; i++) if (crowd[i] != null) crowd[i].gameObject.SetActive(false);
        if (crowd.Count > CrowdSize) crowd.RemoveRange(CrowdSize, crowd.Count - CrowdSize);
        yield return new WaitForSecondsRealtime(0.5f);
        Vector3 centre = player.transform.position;
        for (int i = 0; i < crowd.Count; i++)
        {
            float angle = i * Mathf.PI * 2f / Mathf.Max(1, crowd.Count) * 3f;
            float radius = i % 2 == 0 ? 3f : 5.5f;
            crowd[i].transform.position = centre + new Vector3(Mathf.Cos(angle) * radius, 0.05f, Mathf.Sin(angle) * radius);
        }
        Say($"crowd: {crowd.Count} enemies, element art {ElementArt.Ready}, body flames ready {BodyFlames.Ready}");

        build.Attune(Elements.Metal, "");
        yield return new WaitForSecondsRealtime(0.3f);
        combat.Mana.Gain(100f);
        if (element.SurgeKit != null) element.SurgeKit.SurgeSeconds = SoakSeconds * 0.5f + 2f;
        yield return Tap(Key.F);
        LayAll(0);
        // Let everything settle into its steady state before measuring: the pools filled, the flames caught.
        yield return new WaitForSecondsRealtime(1.5f);
        Say($"surging {element.SurgeActive} in {element.SurgeInfo?.Name}; steel {element.SteelLevel:0.00}");
        Say("soak start");

        var allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        var frames = new float[40000];
        int count = 0, round = 1, burningPeak = 0, partsPeak = 0, looksPeak = 0;
        long garbage = 0;
        float steelPeak = 0f;
        float end = Time.unscaledTime + SoakSeconds, nextLay = Time.unscaledTime + 2f, half = Time.unscaledTime + SoakSeconds * 0.5f;
        bool switched = false;
        while (Time.unscaledTime < end)
        {
            if (Time.unscaledTime >= nextLay)
            {
                nextLay = Time.unscaledTime + 2f;
                LayAll(round++);
            }
            if (!switched && Time.unscaledTime >= half)
            {
                switched = true;
                build.Attune(Elements.Lightning, "");
                combat.Mana.Gain(100f);
                if (element.SurgeKit != null) element.SurgeKit.SurgeSeconds = SoakSeconds;
                StartCoroutine(Tap(Key.F));
            }
            yield return null;
            if (count < frames.Length) frames[count++] = Time.unscaledDeltaTime * 1000f;
            garbage += allocated.LastValue;
            burningPeak = Mathf.Max(burningPeak, BodyFlames.Burning);
            partsPeak = Mathf.Max(partsPeak, ElementLooks.Shown);
            looksPeak = Mathf.Max(looksPeak, ElementLooks.Playing);
            steelPeak = Mathf.Max(steelPeak, element.SteelLevel);
        }
        allocated.Dispose();
        var sorted = frames.Take(count).OrderBy(ms => ms).ToArray();
        float mean = sorted.Average(), p95 = sorted[(int)(sorted.Length * 0.95f)], worst = sorted[sorted.Length - 1];
        Say($"perf afflicted crowd: {count} frames in {SoakSeconds} s; frame ms mean {mean:0.00} p95 {p95:0.00} worst {worst:0.00}; garbage {garbage / Mathf.Max(1, count)} bytes a frame (editor included); bodies burning in real flame at most {burningPeak}; element looks playing at most {looksPeak} with {partsPeak} of {ElementLooks.PoolSize} pieces drawn; steel at most {steelPeak:0.00}; surging now {element.SurgeActive} in {element.SurgeInfo?.Name}");
        foreach (var enemy in crowd) if (enemy != null) enemy.Afflictions.Clear();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say("done");
        Destroy(gameObject);
    }
}
