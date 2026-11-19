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

// Phase 3, task 3.2, in the Proving Ground: the first six reactions. Five copies of the still dummy are set round a host
// on the east terrace (A and B 2 m west and east of it, C 2.2 m north, D 4.5 m south). For each pair the player carries
// its two elements (PlayerBuild.Attune), the first affliction is laid on the host as a spell's, then the second: the
// reaction must come once (the engine's report and the adapter's), show its name over the host, do its effect, and rest
// (laid again at once nothing comes). Flashover's discharge, Steam's puff (and its 3 s rest run out), The Ground Gives
// (sunk, and held by a heavy blow), Live Iron (a weapon blow arcs to the notched A, never the clean B), Rot Roots (three
// joined, a bite passed), Carried Embers (fire lands downwind of D, by H and C; H carried Unsteady and the reaction's
// fire set nothing off). Then a laying counted as a reaction's starts nothing, and a real Flame Wave (a spell's fire,
// given in seconds) on a Soaked dummy is Steam. Frames review/shots/current/p3_*.png with time stopped. Every judged line
// ends OK or WRONG. The copies are made in play and gone with it; progress stays sandboxed (runprobe.sh).
// bash AgentScripts/runprobe.sh ProvingGround ReactionProbe 120
public static class ReactionProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Reaction Probe").AddComponent<ReactionProbeRunner>();
        return "probe started";
    }
}

public class ReactionProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerBuild build;
    private PlayerReactions reactions;
    private TrainingDummy host, a, b, c, d;
    private readonly List<TrainingDummy> made = new List<TrainingDummy>();
    private int ok, wrong;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
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
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    private IEnumerator Still(string name)
    {
        Time.timeScale = 0f;
        yield return Shot(name);
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

    private IEnumerator Clean()
    {
        foreach (var dummy in made) dummy.Afflictions.Clear();
        // Ground left by the last part must be out before the next is read.
        yield return Until(() => Hazards.Alive == 0, 5f);
        foreach (var dummy in made) dummy.Afflictions.Clear();
        yield return null;
    }

    private static string ElementOf(AfflictionKind kind) => Elements.All.First(info => info.Affliction == kind).Name;

    private static void Lay(TrainingDummy on, AfflictionKind kind, int stacks, LaySource source = LaySource.Spell) =>
        on.Afflictions.Lay(kind, stacks, 0f, 1f, source, on);

    // Carries the pair's two elements, stands south of the target, clears everything, lays the first then the second.
    // Returns through the field `came` whether the reaction was shown once on the target.
    private bool came;
    private IEnumerator Pair(string name, TrainingDummy on, AfflictionKind first, int firstStacks, AfflictionKind second, int secondStacks, Action ready = null)
    {
        int pair = AfflictionPairs.Index(first, second);
        build.Attune(ElementOf(AfflictionPairs.First(pair)), ElementOf(AfflictionPairs.Second(pair)));
        yield return Clean();
        ready?.Invoke();
        player.Teleport(on.transform.position + Vector3.back * 3f, Quaternion.identity);
        yield return Real(0.3f);
        int shownBefore = reactions.TimesShown(pair), wordsBefore = ReactionWords.Instance != null ? ReactionWords.Instance.Written : -1;
        Lay(on, first, firstStacks);
        yield return null;
        Judge($"{name}: the first affliction alone starts nothing", reactions.TimesShown(pair) == shownBefore);
        Lay(on, second, secondStacks);
        yield return Until(() => reactions.TimesShown(pair) > shownBefore, 1.5f);
        came = reactions.TimesShown(pair) == shownBefore + 1 && reactions.LastOn == on;
        Judge($"{name}: it reacts once on the host", came, $"shown {reactions.TimesShown(pair) - shownBefore}, last '{reactions.LastShown}' on {(reactions.LastOn != null ? reactions.LastOn.name : "nothing")}");
        var words = ReactionWords.Instance;
        Judge($"{name}: its name is written over the host", words != null && words.Written == wordsBefore + 1 && words.LastWord == name && words.Showing > 0,
            words != null ? $"'{words.LastWord}', {words.Showing} up" : "no words on the HUD");
        float rest = on.Afflictions.ReactionRest(pair);
        Judge($"{name}: it rests on the host for {ReactionTable.RestFor(pair):0} s", rest > ReactionTable.RestFor(pair) - 0.5f && rest <= ReactionTable.RestFor(pair), $"{rest:0.00} s left");
    }

    // Laid again at once: the pair is resting, nothing comes.
    private IEnumerator Rests(string name, TrainingDummy on, AfflictionKind kind)
    {
        int pair = -1;
        for (int i = 0; i < AfflictionPairs.Count; i++) if (AfflictionPairs.Name(i) == name) pair = i;
        int shown = reactions.TimesShown(pair);
        Lay(on, kind, 1);
        yield return Real(0.25f);
        Judge($"{name}: laid again at once, it is resting and nothing comes", reactions.TimesShown(pair) == shown);
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
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        reactions = player.GetComponent<PlayerReactions>();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest {PlaytestMode.On}, sandboxed {Progress.Sandboxed}, reactions built {ReactionTable.Built}");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        Judge("the player carries the reactions' adapter and the HUD its words", reactions != null && ReactionWords.Instance != null);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null) { Finish(); yield break; }
        host = Copy(still, "Probe Host", Middle);
        a = Copy(still, "Probe A", Middle + new Vector3(-2f, 0f, 0f));
        b = Copy(still, "Probe B", Middle + new Vector3(2f, 0f, 0f));
        c = Copy(still, "Probe C", Middle + new Vector3(0f, 0f, 2.2f));
        d = Copy(still, "Probe D", Middle + new Vector3(0f, 0f, -4.5f));
        yield return Real(0.3f);

        // 1. Flashover (Burn and Static): the Static discharges through the flame at once, 7 a stack in 3 m; the Burn stays.
        // A's count is read as the pair is set up: the reaction strikes in the frame it is shown.
        int aHits = 0;
        yield return Pair("Flashover", host, AfflictionKind.Burn, 2, AfflictionKind.Static, 3, () => aHits = a.HitsTaken);
        yield return Real(0.3f);
        yield return Still("p3_flashover");
        Judge("Flashover: the host's Static is spent and its Burn is not", host.Afflictions.Stacks(AfflictionKind.Static) == 0 && host.Afflictions.Burn.IsBurning,
            $"static {host.Afflictions.Stacks(AfflictionKind.Static)}, burning {host.Afflictions.Burn.IsBurning}");
        Judge("Flashover: the discharge strikes A, 2 m off, for 7 a stack (21)", a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 21f) < 0.6f, $"A hits {a.HitsTaken - aHits}, last {a.LastDamage:0.0}");
        Judge("Flashover: the flame is live for 4 s (a condition for the bending discharges to come)", host.Afflictions.Conditions.Holds(Clover.Player.Trees.Reactions.LiveFlame),
            $"{host.Afflictions.Conditions.Left(Clover.Player.Trees.Reactions.LiveFlame):0.0} s");
        yield return Rests("Flashover", host, AfflictionKind.Static);

        // 2. Steam (Burn and Soaked): Soaked spent, half the Burn (3 to 1), 8 and 4 for each Burn spent to all in 2.5 m; rest 3 s.
        yield return Pair("Steam", host, AfflictionKind.Soaked, 1, AfflictionKind.Burn, 3, () => aHits = a.HitsTaken);
        yield return Real(0.3f);
        yield return Still("p3_steam");
        Judge("Steam: the water is gone and half the Burn with it (3 to 1)", !host.Afflictions.Has(AfflictionKind.Soaked) && host.Afflictions.Stacks(AfflictionKind.Burn) == 1,
            $"soaked {host.Afflictions.Has(AfflictionKind.Soaked)}, burn x{host.Afflictions.Stacks(AfflictionKind.Burn)}");
        Judge("Steam: the puff scalds A for 8 + 4 x 2 = 16", a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 16f) < 0.6f, $"A hits {a.HitsTaken - aHits}, last {a.LastDamage:0.0}");
        yield return Rests("Steam", host, AfflictionKind.Soaked);
        int steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
        yield return Until(() => host.Afflictions.ReactionRest(steam) <= 0f, 4f);
        int steamShown = reactions.TimesShown(steam);
        Lay(host, AfflictionKind.Burn, 1);
        yield return Until(() => reactions.TimesShown(steam) > steamShown, 1f);
        Judge("Steam: its 3 s rest over, Burn on the Soaked host steams again", reactions.TimesShown(steam) == steamShown + 1);

        // 3. The Ground Gives (Soaked and Tremor): sunk for 4 s at half pace; a heavy blow holds it 0.5 s.
        yield return Pair("The Ground Gives", host, AfflictionKind.Tremor, 1, AfflictionKind.Soaked, 1);
        yield return Real(0.2f);
        var sunk = host.Afflictions.Conditions;
        Judge("The Ground Gives: sunk, at half pace, for 4 s", sunk.Holds(Clover.Player.Trees.Reactions.SunkInSand) && Mathf.Abs(sunk.Pace - 0.5f) < 0.01f,
            $"pace {sunk.Pace:0.00}, {sunk.Left(Clover.Player.Trees.Reactions.SunkInSand):0.0} s left, speed {host.Afflictions.SpeedMultiplier:0.00}");
        host.TakeHit(new Hit(4f, 0f, Vector3.forward, heavy: false));
        yield return Real(0.2f);
        Judge("The Ground Gives: a light blow does not hold it", !host.Afflictions.IsHeld);
        host.TakeHit(new Hit(4f, 0f, Vector3.forward, heavy: true));
        yield return Real(0.15f);
        Judge("The Ground Gives: a heavy blow holds it fast", host.Afflictions.IsHeld, $"held {host.Afflictions.HeldLeft:0.00} s");

        // 4. Live Iron (Sundered and Static): for 6 s a weapon blow on the host arcs to the notched A, never the clean B or C.
        yield return Pair("Live Iron", host, AfflictionKind.Sundered, 1, AfflictionKind.Static, 1, () => Lay(a, AfflictionKind.Sundered, 1));
        yield return Real(0.2f);
        Judge("Live Iron: the host's iron is live", host.Afflictions.Conditions.Holds(Clover.Player.Trees.Reactions.IronLive));
        aHits = a.HitsTaken;
        int bHits = b.HitsTaken, cHits = c.HitsTaken, aNotches = a.Afflictions.Stacks(AfflictionKind.Sundered);
        int leapt = upgrades.ArcsLeapt;
        host.TakeHit(new Hit(30f, 10f, Vector3.forward));
        yield return Real(0.25f);
        yield return Still("p3_liveiron");
        Judge("Live Iron: a weapon blow arcs to the notched A for 35% (10.5) and notches it", a.HitsTaken == aHits + 1 && Mathf.Abs(a.LastDamage - 10.5f) < 1.5f && a.Afflictions.Stacks(AfflictionKind.Sundered) > aNotches,
            $"A hits {a.HitsTaken - aHits}, last {a.LastDamage:0.0}, notches {aNotches} to {a.Afflictions.Stacks(AfflictionKind.Sundered)}, arcs {upgrades.ArcsLeapt - leapt}");
        Judge("Live Iron: never to a clean enemy", b.HitsTaken == bHits && c.HitsTaken == cHits);
        aHits = a.HitsTaken;
        host.TakeHit(new Hit(30f, 10f, Vector3.forward, spell: true));
        yield return Real(0.4f);
        Judge("Live Iron: a spell's blow does not arc (weapon blows only)", a.HitsTaken == aHits);

        // 5. Rot Roots (Seeded and Poisoned): A, B and C (the three nearest) joined and Seeded; a bite on the host bites them for half.
        yield return Pair("Rot Roots", host, AfflictionKind.Poison, 2, AfflictionKind.Seeded, 1);
        yield return Real(0.2f);
        yield return Still("p3_rotroots");
        string roots = Clover.Player.Trees.Reactions.Roots;
        int joined = made.Count(dummy => dummy != host && dummy.Afflictions.Conditions.Holds(roots));
        Judge("Rot Roots: three joined, each Seeded, the host not joined to itself", joined == 3 && a.Afflictions.Has(AfflictionKind.Seeded) && b.Afflictions.Has(AfflictionKind.Seeded) && c.Afflictions.Has(AfflictionKind.Seeded) && !host.Afflictions.Conditions.Holds(roots),
            $"joined {joined}: {string.Join(", ", made.Where(dummy => dummy.Afflictions.Conditions.Holds(roots)).Select(dummy => dummy.name))}");
        Judge("Rot Roots: D, 4.5 m off and fourth nearest, is left alone", !d.Afflictions.Conditions.Holds(roots));
        int passed = reactions.BitesPassed;
        aHits = a.HitsTaken;
        yield return Until(() => reactions.BitesPassed > passed, 3.5f);
        yield return Real(0.1f);
        Judge("Rot Roots: a bite on the host bites the joined for half", reactions.BitesPassed >= passed + 3 && a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - host.Afflictions.BitDamage * 0.5f) < 0.6f,
            $"passed {reactions.BitesPassed - passed}, bite {host.Afflictions.BitDamage:0.0}, A took {a.LastDamage:0.0}");

        // 6. Carried Embers (Burn and Unsteady) on D, the player south of it: three fires land north of D, by the host and C.
        //    The host carries Unsteady: the embers' fire on it is the reaction's own and sets nothing off.
        int embers = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Unsteady);
        int alive = 0;
        yield return Pair("Carried Embers", d, AfflictionKind.Unsteady, 1, AfflictionKind.Burn, 1, () => { Lay(host, AfflictionKind.Unsteady, 1); alive = Hazards.Alive; });
        int embersShown = reactions.TimesShown(embers);
        yield return Real(0.15f);
        yield return Still("p3_embers");
        // The way they fly was settled as it reacted: the player steps up beside D so the frame shows where they land.
        player.Teleport(d.transform.position + new Vector3(1.5f, 0f, 1f), Quaternion.identity);
        yield return Real(0.6f);
        yield return Still("p3_embers_landed");
        Judge("Carried Embers: fires lie on the ground", Hazards.Alive >= alive + 3, $"{Hazards.Alive - alive} patches");
        Judge("Carried Embers: they land downwind (north of D, away from the player) and light the host and C", host.Afflictions.Burn.IsBurning && c.Afflictions.Burn.IsBurning,
            $"host {host.Afflictions.Burn.IsBurning}, C {c.Afflictions.Burn.IsBurning}, A {a.Afflictions.Burn.IsBurning}, player at z {player.transform.position.z:0.0}");
        Judge("Carried Embers: the host, Unsteady, is lit by the reaction's fire and starts nothing", reactions.TimesShown(embers) == embersShown);
        yield return Real(1.2f);
        Judge("Carried Embers: still nothing set off as the fires bite", reactions.TimesShown(embers) == embersShown);

        // 7. A laying counted as a reaction's starts nothing, whatever it meets.
        yield return Clean();
        int flashover = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);
        int heard = reactions.Heard;
        Lay(b, AfflictionKind.Static, 2);
        Lay(b, AfflictionKind.Burn, 2, LaySource.Reaction);
        b.Afflictions.Ignite(3f, 1, LaySource.Reaction, b);
        yield return Real(0.3f);
        Judge("a reaction's laying (and a reaction's fire) on a Static enemy starts nothing", reactions.Heard == heard && b.Afflictions.Burn.IsBurning && b.Afflictions.ReactionRest(flashover) <= 0f);

        // 8. A real spell's fire (Flame Wave, fire given in seconds) on a Soaked enemy is Steam: the burn goes through the laying now.
        yield return Clean();
        build.Attune(Elements.Fire, Elements.Water);
        yield return Real(0.2f);
        spells.SetSlot(0, "fire.base");
        spells.Cooldowns.ResetAll();
        Lay(b, AfflictionKind.Soaked, 1, LaySource.World);
        int steamBefore = reactions.TimesShown(steam);
        player.Teleport(b.transform.position + Vector3.back * 2.2f, Quaternion.identity);
        yield return Real(0.4f);
        yield return Tap(Key.Digit1);
        yield return Until(() => reactions.TimesShown(steam) > steamBefore, 2.5f);
        Judge("Flame Wave on the Soaked B (river water) is Steam", reactions.TimesShown(steam) > steamBefore && reactions.LastOn == b,
            $"steam {reactions.TimesShown(steam) - steamBefore}, last on {(reactions.LastOn != null ? reactions.LastOn.name : "nothing")}, b burning {b.Afflictions.Burn.IsBurning}, slot '{spells.Slots[0]}'");
        yield return Real(0.2f);
        yield return Still("p3_steam_from_spell");

        Say($"counts: heard {reactions.Heard}, shown {reactions.ShownCount}, unbuilt {reactions.Unbuilt}, conditions set {upgrades.ConditionsSet}, answers {reactions.ConditionAnswers}, bites passed {reactions.BitesPassed}");
        Finish();
    }

    private void Finish()
    {
        foreach (var dummy in made) if (dummy != null) Destroy(dummy.gameObject);
        if (build != null) build.Attune(Elements.Fire, "");
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
