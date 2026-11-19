using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// The reactions' looks in the Proving Ground (docs/GAME.md, "Reaction effects in the game"). Read through the guide,
// then on a copy of the still dummy (the player 3 m south of it) each of the 55 reactions is set off in turn by laying
// its two afflictions: a designed one must show its look (parts in the pool in use, a frame captured to
// review/shots/current/reactart_<name>.png), and every part and lamp must be back in the pool once it has run; one with
// no design must draw the composed look from its parents' art (frame reactart_composed_<name>.png). Then F8's switch
// off shows the old puff and no parts. Then a burst: a dozen dummies, about 17 reactions a second of every pair for 6 s
// against 3 s of quiet before it, reporting frame time, the GC counter (editor included), the most parts and lamps in
// use, and looks ended early to make room; the pool must be empty 3 s after. Every judged line ends OK or WRONG.
// The copies are made in play and gone with it; progress stays sandboxed (runprobe.sh).
// bash AgentScripts/runprobe.sh ProvingGround ReactionArtProbe 200
public static class ReactionArtProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Reaction Art Probe").AddComponent<ReactionArtProbeRunner>();
        return "probe started";
    }
}

public class ReactionArtProbeRunner : MonoBehaviour
{
    private const float BurstSeconds = 6f;
    private const float QuietSeconds = 3f;
    private const float ReactionsASecond = 17f;

    private PlayerController player;
    private PlayerDefence defence;
    private PlayerBuild build;
    private PlayerReactions reactions;
    private TrainingDummy host;
    private readonly List<TrainingDummy> made = new List<TrainingDummy>();
    private int ok, wrong;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        for (int i = 0; i < made.Count; i++)
            if (made[i] != null) made[i].Health.Fill();
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
    }

    private IEnumerator Shot(string name)
    {
        Time.timeScale = 0f;
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
        Time.timeScale = 1f;
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    // The Proving Ground opens on the guide, which stops the game: read it through first.
    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private TrainingDummy Copy(TrainingDummy from, string name, Vector3 at)
    {
        var copy = Instantiate(from.gameObject, at, from.transform.rotation).GetComponent<TrainingDummy>();
        copy.name = name;
        made.Add(copy);
        return copy;
    }

    private static string ElementOf(AfflictionKind kind) => Elements.All.First(info => info.Affliction == kind).Name;
    private static string FileWord(int pair) => AfflictionPairs.Name(pair).ToLowerInvariant().Replace(' ', '_');

    // Static is laid three deep (Seized and The Grudge wait for two); the rest two.
    private static void Lay(TrainingDummy on, AfflictionKind kind) =>
        on.Afflictions.Lay(kind, kind == AfflictionKind.Static ? 3 : 2, 0f, 1f, LaySource.Spell, on);

    // Everything off every copy, the ground of the last reaction gone (or given a few seconds), the last look ended.
    private IEnumerator Clean()
    {
        foreach (var dummy in made) dummy.Afflictions.Clear();
        yield return Until(() => Hazards.Alive == 0 && ReactionArt.PartsShown == 0 && ReactionArt.LampsShown == 0, 4f);
        foreach (var dummy in made)
        {
            dummy.Afflictions.Clear();
            dummy.Health.Fill();
            dummy.Stagger.Reset();
        }
        yield return null;
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        build = player.GetComponent<PlayerBuild>();
        reactions = player.GetComponent<PlayerReactions>();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest {PlaytestMode.On}, sandboxed {Progress.Sandboxed}, reactions built {ReactionTable.Built}");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        Judge("the game is running (the guide read through)", Time.timeScale > 0.5f && !GuidePage.Showing, $"time scale {Time.timeScale}");
        Judge("all 55 reactions are built", ReactionTable.Built == AfflictionPairs.Count, $"{ReactionTable.Built} built");
        Judge("the element art is in the scene and on", ElementArt.Ready);
        Judge("the designed looks are in the scene (the element art carries them)", ReactionArt.Ready);
        int designedHere = Enumerable.Range(0, AfflictionPairs.Count).Count(ReactionArt.IsDesigned);
        Judge("24 pairs have a designed look, the 31 others are composed", designedHere == 24 && Enumerable.Range(0, AfflictionPairs.Count).All(pair => ReactionArt.IsDesigned(pair) == ReactionDesigns.IsDesigned(pair)),
            $"{designedHere} designed here");

        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null || !ReactionArt.Ready)
        {
            Finish();
            yield break;
        }
        host = Copy(still, "Probe Host", Middle);
        // Neighbours, as ReactionProbe2 sets them, for the reactions that reach another enemy (Glint's ray, Caught Light's net).
        Copy(still, "Probe A", Middle + new Vector3(-2f, 0f, 0f));
        Copy(still, "Probe C", Middle + new Vector3(0f, 0f, 2.2f));
        yield return Real(0.3f);

        for (int pair = 0; pair < AfflictionPairs.Count; pair++) yield return OnePair(pair);
        yield return TheSwitch();
        yield return Burst(still);

        Say($"counts: designed played {ReactionArt.Played}, composed {ReactionArt.ComposedCount}, ended early {ReactionArt.Ended}, shown {reactions.ShownCount}, unbuilt {reactions.Unbuilt}");
        Finish();
    }

    private IEnumerator OnePair(int pair)
    {
        string name = AfflictionPairs.Name(pair);
        var first = AfflictionPairs.First(pair);
        var second = AfflictionPairs.Second(pair);
        build.Attune(ElementOf(first), ElementOf(second));
        yield return Clean();
        player.Teleport(host.transform.position + Vector3.back * 3f, Quaternion.identity);
        yield return Real(0.25f);
        int shown = reactions.TimesShown(pair), played = ReactionArt.Played, composed = ReactionArt.ComposedCount;
        Lay(host, first);
        yield return null;
        Lay(host, second);
        yield return Until(() => reactions.TimesShown(pair) > shown, 1.5f);
        bool came = reactions.TimesShown(pair) > shown;
        if (ReactionArt.IsDesigned(pair))
        {
            // Past the pop, every part stands.
            yield return Real(0.15f);
            int parts = ReactionArt.PartsShown, lamps = ReactionArt.LampsShown;
            Judge($"{name}: its designed look is shown", came && ReactionArt.Played > played && parts > 0, $"reacted {came}, parts {parts}, lamps {lamps}");
            yield return Shot("reactart_" + FileWord(pair));
            yield return Until(() => ReactionArt.PartsShown == 0 && ReactionArt.LampsShown == 0, 3f);
            Judge($"{name}: every part and lamp is back in the pool after", ReactionArt.PartsShown == 0 && ReactionArt.LampsShown == 0,
                $"parts {ReactionArt.PartsShown}, lamps {ReactionArt.LampsShown}");
        }
        else
        {
            yield return Real(0.15f);
            Judge($"{name}: no design, so a look composed from its parents", came && ReactionArt.ComposedCount > composed && ReactionArt.PartsShown == 0,
                $"reacted {came}, composed {ReactionArt.ComposedCount - composed}, designed parts {ReactionArt.PartsShown}");
            yield return Shot("reactart_composed_" + FileWord(pair));
        }
    }

    // F8 off: the reaction shows the old puff, no designed parts and nothing composed; on again, the look is back.
    private IEnumerator TheSwitch()
    {
        yield return Clean();
        int designed = Enumerable.Range(0, AfflictionPairs.Count).First(ReactionArt.IsDesigned);
        int plain = Enumerable.Range(0, AfflictionPairs.Count).First(pair => !ReactionArt.IsDesigned(pair));
        ElementArt.Flip();
        int played = ReactionArt.Played, composed = ReactionArt.ComposedCount;
        ReactionLook.Play(designed, host.transform.position, host.transform, Vector3.forward);
        ReactionLook.Play(plain, host.transform.position, host.transform, Vector3.forward);
        yield return Real(0.15f);
        Judge("F8 off: the old puff, no designed parts and nothing composed", !ElementArt.On && ReactionArt.Played == played && ReactionArt.ComposedCount == composed && ReactionArt.PartsShown == 0,
            $"played {ReactionArt.Played - played}, composed {ReactionArt.ComposedCount - composed}, parts {ReactionArt.PartsShown}");
        yield return Shot("reactart_switch_old");
        ElementArt.Flip();
        ReactionLook.Play(designed, host.transform.position, host.transform, Vector3.forward);
        yield return Real(0.15f);
        Judge("F8 on again: the designed look is back", ElementArt.On && ReactionArt.Played == played + 1 && ReactionArt.PartsShown > 0, $"parts {ReactionArt.PartsShown}");
        yield return Until(() => ReactionArt.PartsShown == 0 && ReactionArt.LampsShown == 0, 3f);
    }

    // A dozen bodies and about 17 reactions a second of every pair, played straight through the look (rests and
    // afflictions aside, so the rate is the probe's), against a quiet stretch before it.
    private IEnumerator Burst(TrainingDummy still)
    {
        yield return Clean();
        var bodies = new List<TrainingDummy>(made);
        for (int i = bodies.Count; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12f;
            bodies.Add(Copy(still, "Probe Crowd " + i, Middle + new Vector3(Mathf.Cos(angle) * 4f, 0f, Mathf.Sin(angle) * 4f)));
        }
        player.Teleport(Middle + Vector3.back * 6f, Quaternion.identity);
        yield return Real(0.5f);

        var allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        var quiet = new float[4000];
        int quietCount = 0;
        long quietGarbage = 0;
        float end = Time.unscaledTime + QuietSeconds;
        while (Time.unscaledTime < end)
        {
            yield return null;
            if (quietCount < quiet.Length) quiet[quietCount++] = Time.unscaledDeltaTime * 1000f;
            quietGarbage += allocated.LastValue;
        }

        var busy = new float[8000];
        int busyCount = 0, mostParts = 0, mostLamps = 0, played = ReactionArt.Played, composed = ReactionArt.ComposedCount, ended = ReactionArt.Ended;
        long busyGarbage = 0;
        var random = new System.Random(23);
        float owed = 0f;
        var playerAt = player.transform;
        end = Time.unscaledTime + BurstSeconds;
        while (Time.unscaledTime < end)
        {
            owed += Time.unscaledDeltaTime * ReactionsASecond;
            while (owed >= 1f)
            {
                owed -= 1f;
                var body = bodies[random.Next(bodies.Count)];
                int pair = random.Next(AfflictionPairs.Count);
                Vector3 at = body.transform.position;
                ReactionLook.Play(pair, at, body.transform, at - playerAt.position);
            }
            yield return null;
            if (busyCount < busy.Length) busy[busyCount++] = Time.unscaledDeltaTime * 1000f;
            busyGarbage += allocated.LastValue;
            mostParts = Mathf.Max(mostParts, ReactionArt.PartsShown);
            mostLamps = Mathf.Max(mostLamps, ReactionArt.LampsShown);
        }
        allocated.Dispose();
        yield return Shot("reactart_burst");

        var quietSorted = quiet.Take(quietCount).OrderBy(ms => ms).ToArray();
        var busySorted = busy.Take(busyCount).OrderBy(ms => ms).ToArray();
        float quietP95 = quietSorted[(int)(quietSorted.Length * 0.95f)], busyP95 = busySorted[(int)(busySorted.Length * 0.95f)];
        long quietPerFrame = quietGarbage / Mathf.Max(1, quietCount), busyPerFrame = busyGarbage / Mathf.Max(1, busyCount);
        int looks = ReactionArt.Played - played + ReactionArt.ComposedCount - composed;
        Say($"perf quiet: {quietCount} frames, ms mean {quietSorted.Average():0.0} p95 {quietP95:0.0} worst {quietSorted[quietSorted.Length - 1]:0.0}, garbage {quietPerFrame} bytes a frame (editor included)");
        Say($"perf burst: {busyCount} frames, ms mean {busySorted.Average():0.0} p95 {busyP95:0.0} worst {busySorted[busySorted.Length - 1]:0.0}, garbage {busyPerFrame} bytes a frame (editor included); " +
            $"{ReactionArt.Played - played} designed and {ReactionArt.ComposedCount - composed} composed looks ({looks / BurstSeconds:0.0} a second), most parts {mostParts}, most lamps {mostLamps}, ended early {ReactionArt.Ended - ended}");
        Judge("the burst keeps the frame under 16.6 ms at the 95th percentile", busyP95 < 16.6f, $"p95 {busyP95:0.0} ms (quiet {quietP95:0.0})");
        Judge("the burst adds no garbage of its own (within 1 KB a frame of the quiet, the editor's own included)", busyPerFrame - quietPerFrame < 1024, $"{busyPerFrame} against {quietPerFrame} bytes a frame");
        Judge("about 17 looks a second were drawn", looks > BurstSeconds * ReactionsASecond * 0.8f, $"{looks} in {BurstSeconds} s");
        yield return Real(3f);
        Judge("3 s after the burst every part and lamp is back in the pool", ReactionArt.PartsShown == 0 && ReactionArt.LampsShown == 0,
            $"parts {ReactionArt.PartsShown}, lamps {ReactionArt.LampsShown}");
    }

    private void Finish()
    {
        foreach (var dummy in made)
            if (dummy != null) Destroy(dummy.gameObject);
        made.Clear();
        if (!ElementArt.On) ElementArt.Flip();
        if (build != null) build.Attune(Elements.Fire, "");
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
