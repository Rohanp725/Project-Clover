using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Phase 4, tasks 4.5 to 4.17, in the Proving Ground with the playtest on: every duo branch's reaction, plain and then in
// each of its two shapes, round all 55 pairs in the README's order (distance 1, the reshaped ring mixes; distance 2 to 5,
// the new branches). DuoProbe goes deeper on Flashover and Sleet Charge; this one only asks, for each pair, that the plain
// reaction runs while its Shape mark is not owned, and that each shape chosen at a rest fire runs instead once it is.
//
// For each pair: its two elements carried, its Shape mark taken away, a fresh copy of the still dummy (so no reaction is
// resting on it) given the pair's two afflictions, first the README's first then its second (and the other way round if
// that does not react), deep enough for a reaction that waits for a depth; the plain reaction judged; the Shape mark
// given; each shape chosen at a rest fire and judged on a fresh copy again. What each shape did is logged beside it (the
// host's hits, ground laid, the conditions the shape sets on its host that hold), for a person to read; only the shape's
// name is judged. A neighbour copy stands 2 m west for the shapes that reach others. No frames are taken.
// Every judged line ends OK or WRONG. The copies and the Shape marks given are gone at the end, the player back to Fire
// alone; progress stays sandboxed (runprobe.sh). 55 pairs and three reactions each take about eight minutes.
// It writes about 390 lines and runprobe.sh prints only the last 150: read them all after the run with
// `unity command console get -- --tail 450 | grep -o "\[Probe\][^\"]*"` (Play Mode stopped keeps the console), or grep
// for WRONG.
// bash AgentScripts/runprobe.sh ProvingGround DuoProbeAll 200
public static class DuoProbeAll
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Duo Probe All").AddComponent<DuoProbeAllRunner>();
        return "probe started";
    }
}

public class DuoProbeAllRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerUpgrades upgrades;
    private PlayerBuild build;
    private PlayerReactions reactions;
    private TrainingDummy still, host, neighbour;
    private readonly List<string> given = new List<string>();
    private int ok, wrong;
    private bool playtestWas;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);
    // Stacks laid when the reaction asks no depth: enough for every plain reaction, short of freezing (Chill freezes at five).
    private const int Usual = 2;

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
    }

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
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

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private TrainingDummy Copy(string name, Vector3 at)
    {
        var copy = Instantiate(still.gameObject, at, still.transform.rotation).GetComponent<TrainingDummy>();
        copy.name = name;
        return copy;
    }

    private static void Lay(TrainingDummy on, AfflictionKind kind, int stacks) => on.Afflictions.Lay(kind, stacks, 0f, 1f, LaySource.Spell, on);
    private static int Depth(TrainingDummy on, AfflictionKind kind) => on.Afflictions.Stacks(kind);

    private void Give(string id)
    {
        if (upgrades.Give(Upgrade.ById(id))) given.Add(id);
        upgrades.RefreshNow();
    }

    private void Take(string id)
    {
        RunState.Unlocked.Remove(id);
        upgrades.RefreshNow();
    }

    // A fresh host (no reaction resting on it), the neighbour's afflictions gone, the ground of the last run given a moment
    // to clear (at most 2 s, so one long patch does not hold the whole probe up), the player 3 m south of the host.
    private IEnumerator Fresh()
    {
        if (host != null) Destroy(host.gameObject);
        yield return null;
        host = Copy("Probe Host", Middle);
        neighbour.Afflictions.Clear();
        neighbour.Health.Fill();
        neighbour.Stagger.Reset();
        yield return Until(() => Hazards.Alive == 0, 2f);
        player.Teleport(Middle + Vector3.back * 3f, Quaternion.identity);
        yield return Real(0.2f);
    }

    // How deep to lay one of the pair's afflictions: a reaction that waits for a depth of it gets that depth.
    private static int StacksOf(int pair, AfflictionKind kind)
    {
        var def = ReactionTable.MarkFor(pair)?.Reaction;
        return def != null && def.DepthOf == kind ? Math.Max(Usual, def.Depth) : Usual;
    }

    private string shownShape = "";
    private bool shownOnHost;

    // Lays the pair's two afflictions on a fresh host in the README's order, then the other way round on another fresh
    // host if that did not react, and keeps the shape the reaction ran in, read in the frame it was shown.
    private IEnumerator React(int pair)
    {
        shownShape = "";
        shownOnHost = false;
        var first = AfflictionPairs.First(pair);
        var second = AfflictionPairs.Second(pair);
        for (int order = 0; order < 2; order++)
        {
            yield return Fresh();
            var one = order == 0 ? first : second;
            var other = order == 0 ? second : first;
            int shown = reactions.TimesShown(pair);
            Lay(host, one, StacksOf(pair, one));
            yield return null;
            Lay(host, other, StacksOf(pair, other));
            yield return Until(() => reactions.TimesShown(pair) > shown, 1.5f);
            if (reactions.TimesShown(pair) > shown)
            {
                shownShape = reactions.LastShape;
                shownOnHost = reactions.LastOn == host;
                // Another pair may react on the same host in the same frame (ground left by an earlier pair), and LastShape is
                // then that one's: the pair's own shape is the table's, which is what Act ran it in.
                string own = ReactionTable.MarkFor(pair)?.Name ?? "";
                if (reactions.LastShown != own)
                {
                    shownShape = ReactionTable.WornShape(pair, build.Read(), RunState.Unlocked)?.Name ?? "";
                    Say($"  (then {reactions.LastShown} was shown on it in the same frame; {own}'s shape read from the table)");
                }
                if (order == 1) Say($"  ({AfflictionPairs.Name(pair)} reacted only with {second} laid before {first})");
                yield break;
            }
        }
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        playtestWas = PlaytestMode.On;
        PlaytestMode.Set(true);
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        build = player.GetComponent<PlayerBuild>();
        reactions = player.GetComponent<PlayerReactions>();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest {PlaytestMode.On}, sandboxed {Progress.Sandboxed}");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        Judge("the player has its build and the reactions' adapter", build != null && reactions != null && upgrades != null);
        int built = Enumerable.Range(0, AfflictionPairs.Count).Count(pair => ReactionTable.ShapeMarkFor(pair) != null);
        Judge("every pair has a duo branch with its Shape mark", built == AfflictionPairs.Count, $"{built} of {AfflictionPairs.Count}");
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null) { Finish(); yield break; }
        neighbour = Copy("Probe Neighbour", Middle + new Vector3(-2f, 0f, 0f));

        // PlayerPrefs "clover.probe.duoall.only" (reaction names, comma separated) runs only those pairs; empty runs all.
        string only = PlayerPrefs.GetString("clover.probe.duoall.only", "");
        for (int pair = 0; pair < AfflictionPairs.Count; pair++)
        {
            if (only.Length > 0 && !only.Split(',').Contains(AfflictionPairs.Name(pair))) continue;
            yield return OnePair(pair);
        }
        Finish();
    }

    private IEnumerator OnePair(int pair)
    {
        string name = AfflictionPairs.Name(pair);
        var mark = ReactionTable.ShapeMarkFor(pair);
        if (mark == null)
        {
            Judge($"{name}: its duo branch's Shape mark is in the table", false);
            yield break;
        }
        Say($"--- {name} ({mark.Element} and {mark.Element2}, distance {AfflictionPairs.Distance(pair)}): {mark.Id}");
        build.Attune(mark.Element, mark.Element2);
        Take(mark.Id);

        yield return React(pair);
        Judge($"{name}, Shape mark not owned: the plain reaction runs", shownOnHost && shownShape == "", $"shape '{shownShape}', on the host {shownOnHost}");

        Give(mark.Id);
        Judge($"{name}: its Shape mark is owned and in force with both carried", upgrades.Has(mark.Id));
        string key = ReactionTable.ShapeKey(pair);
        for (int choice = 0; choice < mark.Shapes.Length; choice++)
        {
            var shape = mark.Shapes[choice];
            Judge($"{name}: {shape.Name} chosen at a rest fire", build.ChooseShape(key, choice, true) && build.Read().ShapeOf(key) == choice);
            int alive = Hazards.Alive;
            yield return React(pair);
            int hits = host != null ? host.HitsTaken : -1;
            yield return Real(0.6f);
            Judge($"{name} in {shape.Name}: the shape runs instead of the plain reaction", shownOnHost && shownShape == shape.Name, $"shape '{shownShape}', on the host {shownOnHost}");
            Say($"  {shape.Name} did: host hits {(host != null ? host.HitsTaken - hits : -1)} more in 0.6 s, ground {alive}->{Hazards.Alive}, " +
                $"its conditions held {Held(shape)}, host {Carries(host, AfflictionPairs.First(pair))} {AfflictionPairs.First(pair)} and {Carries(host, AfflictionPairs.Second(pair))} {AfflictionPairs.Second(pair)}");
        }
        Take(mark.Id);
        given.Remove(mark.Id);
        build.Attune(Elements.Fire, "");
    }

    // The conditions the shape sets on its host as the reaction fires that the host still holds.
    private string Held(ShapeDef shape)
    {
        if (host == null) return "-";
        var ids = shape.Effects.Where(effect => effect.On == UpgradeTrigger.Reacted && effect.Do == UpgradeAction.Condition && effect.State != null && !effect.State.ToOthers)
            .Select(effect => effect.State.Id).Distinct().ToList();
        if (ids.Count == 0) return "(none set)";
        return string.Join(", ", ids.Select(id => id + (host.Afflictions.Conditions.Holds(id) ? " yes" : " no")));
    }

    private static int Carries(TrainingDummy on, AfflictionKind kind) => on != null ? Depth(on, kind) : -1;

    private void Finish()
    {
        if (host != null) Destroy(host.gameObject);
        if (neighbour != null) Destroy(neighbour.gameObject);
        foreach (string id in given) RunState.Unlocked.Remove(id);
        if (build != null) build.Attune(Elements.Fire, "");
        if (upgrades != null) upgrades.RefreshNow();
        PlaytestMode.Set(playtestWas);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
