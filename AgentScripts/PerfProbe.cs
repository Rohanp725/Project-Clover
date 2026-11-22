using System.Collections;
using System.Collections.Generic;
using Clover.Player;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

// Play Mode probe: stands the player in the middle of a fight for ten seconds and reports frame time and the memory
// thrown away each frame, read from the profiler's own counters. Numbers in the editor are worse than in a build.
public static class PerfProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Perf Probe").AddComponent<PerfProbeRunner>();
        return "probe started";
    }
}

public class PerfProbeRunner : MonoBehaviour
{
    private PlayerDefence defence;

    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
    }

    private static long Triangles(Mesh mesh)
    {
        long indices = 0;
        for (int i = 0; i < mesh.subMeshCount; i++) indices += mesh.GetIndexCount(i);
        return indices / 3;
    }

    private IEnumerator Start()
    {
        // The guide closed with Escape and time shown to run before anything is measured.
        float waited = Time.unscaledTime + 5f;
        while (!Clover.UI.GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        for (int presses = 0; Clover.UI.GuidePage.Showing && presses < 20; presses++)
        {
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Escape));
            yield return null;
            yield return null;
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState());
            yield return null;
        }
        float timeWas = Time.time;
        yield return new WaitForSecondsRealtime(0.4f);
        Debug.Log($"[Probe] guide {(Clover.UI.GuidePage.Showing ? "STILL OPEN" : "closed")}; time runs: {Time.time > timeWas}");
        var player = FindAnyObjectByType<PlayerController>();
        var outfit = player.GetComponentInChildren<PlayerOutfit>();
        if (outfit != null) Debug.Log($"[Probe] outfit {outfit.Worn}: {Triangles(outfit.Body.sharedMesh)} triangles, {outfit.Body.bones.Length} bones, hand grip {(player.GetComponentInChildren<HandGrip>() != null)}");
        defence = player.GetComponent<PlayerDefence>();
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Castle") player.Teleport(new Vector3(0f, 0.1f, 44f), Quaternion.identity);
        if (scene == "SunCity")
        {
            // The busiest place in the city (second pass): the Market of the Cold Oven, with everyone within 60 m (the alley,
            // the roofs, the Lamp Street, the gate and the yard) awake and coming, and the sun swelling over them.
            player.Teleport(new Vector3(-18f, 6.1f, 188f), Quaternion.identity);
            var swell = FindAnyObjectByType<Clover.World.NoonHeat>();
            if (swell != null) swell.SetSchedule(new Clover.Gameplay.HeatSchedule(swell.Clock + 1f, 200f));
            int woken = 0;
            foreach (var enemy in FindObjectsByType<Clover.Gameplay.Enemy>(FindObjectsSortMode.None))
            {
                if ((enemy.transform.position - player.transform.position).magnitude > 60f) continue;
                enemy.TakeHit(new Clover.Gameplay.Hit(0.01f, 0f, Vector3.forward));
                woken++;
            }
            var land = FindAnyObjectByType<Clover.World.LandReturn>();
            if (System.IO.File.Exists("AgentScripts/perf_nosleep.txt") && land != null) land.SleepBeyond = 0f;
            Debug.Log($"[Probe] perf SunCity: {woken} enemies struck awake round the market; far enemies sleep beyond {(land != null ? land.SleepBeyond : -1f):0} m");
        }
        if (scene == "SunCourt")
        {
            // The fight with the form: the man felled at once, the Sun-Bearer risen and left to its own choices, with a
            // roll asked of it first so a furrow burns while it is measured (fire_boss_visuals.md: the effects' cost).
            var court = FindAnyObjectByType<Clover.World.SunCourtDirector>();
            var form = FindAnyObjectByType<Clover.Gameplay.SunBearer>();
            player.Teleport(new Vector3(0f, 0.1f, -18.6f), Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            var reading = Clover.UI.ReadingBox.Instance;
            for (int i = 0; i < 30 && reading != null && reading.IsOpen; i++)
            {
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.E));
                yield return null;
                yield return null;
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState());
                yield return new WaitForSecondsRealtime(0.08f);
            }
            var man = FindAnyObjectByType<Clover.World.AreaDirector>().Enemies[0];
            player.Teleport(new Vector3(0f, 0.1f, -6f), Quaternion.identity);
            for (int blow = 0; blow < 200 && !man.IsDead; blow++) man.TakeHit(new Clover.Gameplay.Hit(60f, 0f, Vector3.forward, true, true));
            float until = Time.time + 14f;
            while (Time.time < until && !(form.InFight && form.Doing == "Idle")) yield return null;
            form.Queued = Clover.Gameplay.SunBearerAttack.Roll;
            player.Teleport(new Vector3(9f, 0.1f, -8f), Quaternion.identity);
            Debug.Log($"[Probe] perf SunCourt: begun {court.Begun} the man dead {man.IsDead} form in the fight {form.InFight} doing {form.Doing}");
        }
        yield return new WaitForSeconds(2f);
        var allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        var mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
        var drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        var triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        var setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        var times = new List<float>(2000);
        long garbage = 0, calls = 0, batchTotal = 0, triangleTotal = 0, setPassTotal = 0;
        double mainMs = 0;
        float end = Time.unscaledTime + 10f;
        while (Time.unscaledTime < end)
        {
            yield return null;
            times.Add(Time.unscaledDeltaTime * 1000f);
            garbage += allocated.LastValue;
            mainMs += mainThread.LastValue * 1e-6;
            calls += drawCalls.LastValue;
            batchTotal += batches.LastValue;
            triangleTotal += triangles.LastValue;
            setPassTotal += setPass.LastValue;
        }
        times.Sort();
        float sum = 0f;
        foreach (float t in times) sum += t;
        int n = times.Count;
        Debug.Log($"[Probe] perf {scene}: {n} frames in 10 s; frame ms mean {sum / n:0.0} median {times[n / 2]:0.0} p95 {times[(int)(n * 0.95f)]:0.0} worst {times[n - 1]:0.0}; main thread mean {mainMs / n:0.0} ms; garbage {garbage / n} bytes a frame; draw calls {calls / n} batches {batchTotal / n} set-pass calls {setPassTotal / n} triangles {triangleTotal / n}; screen {Screen.width}x{Screen.height}; vsync {QualitySettings.vSyncCount} target {Application.targetFrameRate}");
        allocated.Dispose();
        mainThread.Dispose();
        drawCalls.Dispose();
        batches.Dispose();
        triangles.Dispose();
        setPass.Dispose();
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }
}
