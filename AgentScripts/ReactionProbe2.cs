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
using R = Clover.Player.Trees.Reactions;

// Phase 3, task 3.4, in the Proving Ground: the other 49 reactions (ReactionProbe has the first six). Copies of the still
// dummy stand round a host on the east terrace as ReactionProbe sets them (A 2 m west, C 2.2 m north, D 4.5 m south),
// with B 3.5 m east, so A is the one nearest. The player stands 3 m south of the host, so "downwind" is north, toward C.
// For each pair: the first affliction alone starts nothing; with the second it reacts once on the host, its name is
// written over it, and it rests as its entry says; then its main effect is checked (a burst on A or C, a condition on the
// host, a blow answered, a beat, an end). Seized and The Grudge are checked to wait for two Static. Then a soldier from
// the castle's pen: Nerve Fire makes it flinch, and killed while it carries Smoulder and Shut in the Dark, both answer its
// death. No pair is left unbuilt. Frames review/shots/current/p3b_*.png. Every judged line ends OK or WRONG. The copies
// are made in play and gone with it; progress stays sandboxed (runprobe.sh).
// bash AgentScripts/runprobe.sh ProvingGround ReactionProbe2 160
public static class ReactionProbe2
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Reaction Probe 2").AddComponent<ReactionProbe2Runner>();
        return "probe started";
    }
}

public class ReactionProbe2Runner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerUpgrades upgrades;
    private PlayerBuild build;
    private PlayerReactions reactions;
    private TrainingDummy host, a, b, c, d;
    private readonly List<TrainingDummy> made = new List<TrainingDummy>();
    private int ok, wrong;
    private bool healing = true;
    private float hurtAt;
    private int hostBites;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);

    private void Update()
    {
        if (healing && defence != null && !defence.IsDead) defence.Health.Heal(100f);
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

    private IEnumerator ReadAll()
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

    private TrainingDummy Copy(TrainingDummy from, string name, Vector3 at)
    {
        var copy = Instantiate(from.gameObject, at, from.transform.rotation).GetComponent<TrainingDummy>();
        copy.name = name;
        made.Add(copy);
        return copy;
    }

    // Everything off every copy, its health and balance back, and the ground left by the last part gone.
    private IEnumerator Clean()
    {
        foreach (var dummy in made) dummy.Afflictions.Clear();
        yield return Until(() => Hazards.Alive == 0, 7f);
        foreach (var dummy in made)
        {
            dummy.Afflictions.Clear();
            dummy.Health.Fill();
            dummy.Stagger.Reset();
        }
        yield return null;
    }

    private static string ElementOf(AfflictionKind kind) => Elements.All.First(info => info.Affliction == kind).Name;
    private static int PairNamed(string name) => Enumerable.Range(0, AfflictionPairs.Count).First(pair => AfflictionPairs.Name(pair) == name);

    private static void Lay(TrainingDummy on, AfflictionKind kind, int stacks, LaySource source = LaySource.Spell) =>
        on.Afflictions.Lay(kind, stacks, 0f, 1f, source, on);

    private static int Depth(TrainingDummy on, AfflictionKind kind) => on.Afflictions.Stacks(kind);
    private static bool Holds(TrainingDummy on, string condition) => on.Afflictions.Conditions.Holds(condition);

    // A blow of the weapon on the dummy, light or heavy, then time for the answers to be done.
    private static IEnumerator Strike(TrainingDummy on, bool heavy, float wait = 0.3f)
    {
        on.TakeHit(new Hit(4f, 0f, Vector3.forward, heavy: heavy));
        float end = Time.unscaledTime + wait;
        while (Time.unscaledTime < end) yield return null;
    }

    // The player is hurt so a heal can be read; Healed() says by how much since, and lets the probe heal again.
    private void Hurt()
    {
        healing = false;
        defence.Health.Damage(50f);
        hurtAt = defence.Health.Current;
    }

    private float Healed()
    {
        float gained = defence.Health.Current - hurtAt;
        healing = true;
        return gained;
    }

    // Carries the pair's two elements, stands south of the target, clears everything, lays the first then the second.
    // Returns through the field `came` whether the reaction was shown once on the target.
    private bool came;
    private IEnumerator Pair(string name, TrainingDummy on, AfflictionKind first, int firstStacks, AfflictionKind second, int secondStacks, Action ready = null)
    {
        int pair = AfflictionPairs.Index(first, second);
        Judge($"{name}: the pair's reaction is built and named so", AfflictionPairs.Name(pair) == name && ReactionTable.MarkFor(pair) != null);
        build.Attune(ElementOf(AfflictionPairs.First(pair)), ElementOf(AfflictionPairs.Second(pair)));
        yield return Clean();
        ready?.Invoke();
        player.Teleport(host.transform.position + Vector3.back * 3f, Quaternion.identity);
        yield return Real(0.3f);
        int shownBefore = reactions.TimesShown(pair), wordsBefore = ReactionWords.Instance != null ? ReactionWords.Instance.Written : -1;
        Lay(on, first, firstStacks);
        yield return null;
        Judge($"{name}: the first affliction alone starts nothing", reactions.TimesShown(pair) == shownBefore);
        Lay(on, second, secondStacks);
        yield return Until(() => reactions.TimesShown(pair) > shownBefore, 1.5f);
        came = reactions.TimesShown(pair) == shownBefore + 1 && reactions.LastOn == on;
        Judge($"{name}: it reacts once on {on.name}", came, $"shown {reactions.TimesShown(pair) - shownBefore}, last '{reactions.LastShown}' on {(reactions.LastOn != null ? reactions.LastOn.name : "nothing")}");
        var words = ReactionWords.Instance;
        Judge($"{name}: its name is written over it", words != null && words.Written >= wordsBefore + 1 && words.LastWord == name, words != null ? $"'{words.LastWord}'" : "no words on the HUD");
        float rest = on.Afflictions.ReactionRest(pair);
        Judge($"{name}: it rests for {ReactionTable.RestFor(pair):0} s", rest > ReactionTable.RestFor(pair) - 0.5f && rest <= ReactionTable.RestFor(pair), $"{rest:0.00} s left");
    }

    // Laid again at once: the pair is resting, nothing comes.
    private IEnumerator Rests(string name, TrainingDummy on, AfflictionKind kind)
    {
        int pair = PairNamed(name);
        int shown = reactions.TimesShown(pair);
        Lay(on, kind, 1);
        yield return Real(0.25f);
        Judge($"{name}: laid again at once, it is resting and nothing comes", reactions.TimesShown(pair) == shown);
    }

    // Seized and The Grudge: one Static is not enough, and nothing rests; the second sets it off.
    private IEnumerator WaitsForTwoStatic(string name, AfflictionKind other)
    {
        int pair = PairNamed(name);
        build.Attune(ElementOf(AfflictionPairs.First(pair)), ElementOf(AfflictionPairs.Second(pair)));
        yield return Clean();
        int shown = reactions.TimesShown(pair);
        Lay(host, other, 1);
        Lay(host, AfflictionKind.Static, 1);
        yield return Real(0.25f);
        Judge($"{name}: one Static is not enough, and the pair does not rest", reactions.TimesShown(pair) == shown && host.Afflictions.ReactionRest(pair) <= 0f,
            $"shown {reactions.TimesShown(pair) - shown}, rest {host.Afflictions.ReactionRest(pair):0.0}");
    }

    private void OnPoisonBit(Component who, int depth, float damage)
    {
        if (who == host) hostBites++;
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
        upgrades = player.GetComponent<PlayerUpgrades>();
        build = player.GetComponent<PlayerBuild>();
        reactions = player.GetComponent<PlayerReactions>();
        CombatEvents.PoisonBit += OnPoisonBit;
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest {PlaytestMode.On}, sandboxed {Progress.Sandboxed}, reactions built {ReactionTable.Built}");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        Judge("all 55 reactions are built", ReactionTable.Built == AfflictionPairs.Count, $"{ReactionTable.Built} built");
        Judge("the player carries the reactions' adapter and the HUD its words", reactions != null && ReactionWords.Instance != null);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null) { Finish(); yield break; }
        host = Copy(still, "Probe Host", Middle);
        a = Copy(still, "Probe A", Middle + new Vector3(-2f, 0f, 0f));
        b = Copy(still, "Probe B", Middle + new Vector3(3.5f, 0f, 0f));
        c = Copy(still, "Probe C", Middle + new Vector3(0f, 0f, 2.2f));
        d = Copy(still, "Probe D", Middle + new Vector3(0f, 0f, -4.5f));
        int unbuilt = reactions.Unbuilt;
        yield return Real(0.3f);

        yield return DistanceOne();
        yield return DistanceTwo();
        yield return DistanceThree();
        yield return DistanceFour();
        yield return DistanceFive();
        yield return TheSoldier();

        Judge("no reaction heard in all this was unbuilt", reactions.Unbuilt == unbuilt, $"{reactions.Unbuilt - unbuilt} unbuilt");
        Say($"counts: heard {reactions.Heard}, shown {reactions.ShownCount}, unbuilt {reactions.Unbuilt}, conditions set {upgrades.ConditionsSet}, answers {reactions.ConditionAnswers}, arcs {upgrades.ArcsLeapt}, dropped {reactions.Dropped}");
        Finish();
    }

    private IEnumerator DistanceOne()
    {
        // Thunder Gust: thrown 3 m away from the player (north), then its Static goes off, 7 a stack in 4 m (A takes 21).
        int aHits = 0, shoves = 0;
        yield return Pair("Thunder Gust", host, AfflictionKind.Static, 3, AfflictionKind.Unsteady, 1, () => { aHits = a.HitsTaken; shoves = host.Shoves; });
        yield return Real(0.1f);
        Judge("Thunder Gust: thrown 3 m away from the player", host.Shoves == shoves + 1 && host.LastShove.z > 2.5f && Mathf.Abs(host.LastShove.x) < 0.5f, $"shove {host.LastShove}");
        yield return Real(0.5f);
        Judge("Thunder Gust: where it lands its Static goes off, 21 to A", Depth(host, AfflictionKind.Static) == 0 && a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 21f) < 0.6f,
            $"static {Depth(host, AfflictionKind.Static)}, A hits {a.HitsTaken - aHits}, last {a.LastDamage:0.0}");
        yield return Rests("Thunder Gust", host, AfflictionKind.Unsteady);

        // Rime: a crust; the next heavy blows the ice out of its far side, north onto C: Chill 2 and Unsteady 1, not A.
        yield return Pair("Rime", host, AfflictionKind.Chill, 1, AfflictionKind.Unsteady, 1);
        Judge("Rime: the crust grows", Holds(host, R.Crust));
        int cHits = c.HitsTaken;
        aHits = a.HitsTaken;
        yield return Strike(host, false);
        Judge("Rime: a light blow does not break it", Holds(host, R.Crust) && c.HitsTaken == cHits);
        yield return Strike(host, true);
        yield return Shot("p3b_rime");
        Judge("Rime: the heavy blows the ice out behind it onto C, 10 with 2 Chill and an Unsteady", c.HitsTaken > cHits && Depth(c, AfflictionKind.Chill) == 2 && Depth(c, AfflictionKind.Unsteady) == 1 && !Holds(host, R.Crust),
            $"C hits {c.HitsTaken - cHits}, chill {Depth(c, AfflictionKind.Chill)}, unsteady {Depth(c, AfflictionKind.Unsteady)}");
        Judge("Rime: nothing to the side (A)", a.HitsTaken == aHits);

        // Supercooled: the next weapon blow freezes it outright and spends the water.
        yield return Pair("Supercooled", host, AfflictionKind.Soaked, 1, AfflictionKind.Chill, 1);
        Judge("Supercooled: the water goes still", Holds(host, R.StillWater) && !host.Afflictions.IsFrozen);
        yield return Strike(host, false);
        Judge("Supercooled: the next blow freezes it outright and the water is spent", host.Afflictions.IsFrozen && !host.Afflictions.Has(AfflictionKind.Soaked) && !Holds(host, R.StillWater),
            $"frozen {host.Afflictions.IsFrozen}, soaked {host.Afflictions.Has(AfflictionKind.Soaked)}");
        yield return Rests("Supercooled", host, AfflictionKind.Soaked);

        // Quickening: Seeded to full, a flower; a heavy heals 10 and Soaks and Seeds all within 3 m.
        yield return Pair("Quickening", host, AfflictionKind.Seeded, 1, AfflictionKind.Soaked, 1);
        Judge("Quickening: the seed flowers to full", Depth(host, AfflictionKind.Seeded) == Afflictions.Depth(AfflictionKind.Seeded) && Holds(host, R.Flower), $"seeded {Depth(host, AfflictionKind.Seeded)}");
        Hurt();
        yield return Strike(host, true);
        float healed = Healed();
        Judge("Quickening: the heavy bursts it: you heal 10, and A is Soaked and Seeded", healed > 9.5f && healed < 12f && a.Afflictions.Has(AfflictionKind.Soaked) && a.Afflictions.Has(AfflictionKind.Seeded) && !Holds(host, R.Flower),
            $"healed {healed:0.0}, A soaked {a.Afflictions.Has(AfflictionKind.Soaked)}, seeded {a.Afflictions.Has(AfflictionKind.Seeded)}");

        // Split Rock: held 2 s; a heavy while held splits the floor, 10, Tremor and Seeded to A.
        yield return Pair("Split Rock", host, AfflictionKind.Tremor, 1, AfflictionKind.Seeded, 1, () => aHits = a.HitsTaken);
        Judge("Split Rock: roots hold it 2 s", host.Afflictions.IsHeld && host.Afflictions.HeldLeft > 1.5f && Holds(host, R.Wedged), $"held {host.Afflictions.HeldLeft:0.00} s");
        yield return Strike(host, true);
        Judge("Split Rock: the heavy splits the floor, 10 with Tremor and Seeded to A", a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 10f) < 0.6f && a.Afflictions.Has(AfflictionKind.Tremor) && a.Afflictions.Has(AfflictionKind.Seeded),
            $"A last {a.LastDamage:0.0}, tremor {a.Afflictions.Has(AfflictionKind.Tremor)}, seeded {a.Afflictions.Has(AfflictionKind.Seeded)}");

        // Anvil Stone: the next heavy breaks its armour to full and rings out 10 and a notch.
        yield return Pair("Anvil Stone", host, AfflictionKind.Sundered, 1, AfflictionKind.Tremor, 1, () => aHits = a.HitsTaken);
        Judge("Anvil Stone: the anvil stands", Holds(host, R.Anvil));
        yield return Strike(host, true);
        Judge("Anvil Stone: the heavy drives it on: notched to full, and A takes 10 and a notch",
            Depth(host, AfflictionKind.Sundered) == Afflictions.Depth(AfflictionKind.Sundered) && a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 10f) < 0.6f && a.Afflictions.Has(AfflictionKind.Sundered),
            $"host notches {Depth(host, AfflictionKind.Sundered)}, A last {a.LastDamage:0.0}");

        // Glint: a ray to the nearest other (A): 10, Marked and a notch; the host is not struck by it.
        int hostHits = 0;
        yield return Pair("Glint", host, AfflictionKind.Sundered, 1, AfflictionKind.Marked, 1, () => { aHits = a.HitsTaken; hostHits = host.HitsTaken; });
        yield return Real(0.2f);
        yield return Shot("p3b_glint");
        Judge("Glint: the ray strikes A, the nearest, for 10 with Marked and a notch", a.HitsTaken == aHits + 1 && Mathf.Abs(a.LastDamage - 10f) < 0.6f && a.Afflictions.Has(AfflictionKind.Marked) && a.Afflictions.Has(AfflictionKind.Sundered),
            $"A hits {a.HitsTaken - aHits}, last {a.LastDamage:0.0}");
        Judge("Glint: never back into the host", host.HitsTaken == hostHits);

        // Diamond Ring: 0.5 s on, 16 and 30 stagger on it; for 4 s its Mark is not spent by blows.
        yield return Pair("Diamond Ring", host, AfflictionKind.Marked, 1, AfflictionKind.Cursed, 1, () => { hostHits = host.HitsTaken; aHits = a.HitsTaken; });
        yield return Real(0.8f);
        Judge("Diamond Ring: the bead breaks on it, 16", host.HitsTaken == hostHits + 1 && Mathf.Abs(host.LastDamage - 16f) < 0.6f && a.HitsTaken == aHits, $"host last {host.LastDamage:0.0}, A hits {a.HitsTaken - aHits}");
        for (int i = 0; i < 3; i++) host.Afflictions.BlowLanded();
        Judge("Diamond Ring: three blows do not spend its Mark", host.Afflictions.Has(AfflictionKind.Marked) && Holds(host, R.Disc));

        // Shut in the Dark: its doses stop fading for 8 s; then the jar breaks, 5 a dose (20) in 3 m and half the doses (2) on A.
        yield return Pair("Shut in the Dark", host, AfflictionKind.Poison, 4, AfflictionKind.Cursed, 1, () => aHits = a.HitsTaken);
        yield return Real(7f);
        Judge("Shut in the Dark: sealed, its doses have not faded in 7 s", Holds(host, R.Jar) && Depth(host, AfflictionKind.Poison) == 4, $"doses {Depth(host, AfflictionKind.Poison)}");
        yield return Until(() => !Holds(host, R.Jar), 2f);
        yield return Real(0.3f);
        Judge("Shut in the Dark: the jar breaks, 20 to A and two doses", a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 20f) < 0.6f && Depth(a, AfflictionKind.Poison) == 2,
            $"A last {a.LastDamage:0.0}, doses {Depth(a, AfflictionKind.Poison)}");

        // Fire Medicine: a fuse, then a bang: 16 for the doses and 12 for the Burn on A, lit; half the doses spent (4 to 2).
        yield return Pair("Fire Medicine", host, AfflictionKind.Poison, 4, AfflictionKind.Burn, 2, () => aHits = a.HitsTaken);
        yield return Real(1f);
        Judge("Fire Medicine: it bangs on A twice (the doses' and the Burn's) and lights it", a.HitsTaken == aHits + 2 && a.Afflictions.Burn.IsBurning, $"A hits {a.HitsTaken - aHits}, burning {a.Afflictions.Burn.IsBurning}");
        Judge("Fire Medicine: half its doses are spent", Depth(host, AfflictionKind.Poison) == 2, $"doses {Depth(host, AfflictionKind.Poison)}");
    }

    private IEnumerator DistanceTwo()
    {
        // Sleet Charge: a Static every half second for 3 s, then a bolt discharges it all, A chilled.
        int aHits = 0;
        yield return Pair("Sleet Charge", host, AfflictionKind.Chill, 1, AfflictionKind.Static, 1, () => aHits = a.HitsTaken);
        yield return Real(1.6f);
        Judge("Sleet Charge: the frost charges itself", Holds(host, R.Charging) && Depth(host, AfflictionKind.Static) >= 3, $"static {Depth(host, AfflictionKind.Static)}");
        yield return Until(() => !Holds(host, R.Charging), 2f);
        yield return Real(0.3f);
        Judge("Sleet Charge: the bolt discharges it all and chills A", Depth(host, AfflictionKind.Static) == 0 && a.HitsTaken > aHits && a.Afflictions.Has(AfflictionKind.Chill),
            $"static {Depth(host, AfflictionKind.Static)}, A hits {a.HitsTaken - aHits}, last {a.LastDamage:0.0}");

        // Capsize: it is knocked flat 1.2 s (the Down condition; KnockdownProbe sees a soldier lie and stand).
        yield return Pair("Capsize", host, AfflictionKind.Soaked, 1, AfflictionKind.Unsteady, 1);
        Judge("Capsize: it goes down for 1.2 s", Holds(host, R.Capsized) && host.Afflictions.Conditions.Has(ConditionFlags.Down) && host.Afflictions.Conditions.Left(R.Capsized) > 0.9f,
            $"down {host.Afflictions.Conditions.Left(R.Capsized):0.00} s");
        yield return Rests("Capsize", host, AfflictionKind.Soaked);

        // Winter Seed: the seed sleeps while the Chill lasts; the Chill gone, thorns of 12 a seed (24) and you heal 3 a seed.
        yield return Pair("Winter Seed", host, AfflictionKind.Seeded, 2, AfflictionKind.Chill, 1, () => aHits = a.HitsTaken);
        Judge("Winter Seed: the seed sleeps", Holds(host, R.Asleep) && host.Afflictions.Conditions.KeepsKind(AfflictionKind.Seeded));
        Hurt();
        host.Afflictions.Spend(AfflictionKind.Chill, 9);
        yield return Real(0.4f);
        float healed = Healed();
        Judge("Winter Seed: the cold gone, it wakes: 24 to A, Seeded, and you heal 6", !Holds(host, R.Asleep) && a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 24f) < 0.6f && a.Afflictions.Has(AfflictionKind.Seeded) && Mathf.Abs(healed - 6f) < 1f,
            $"A last {a.LastDamage:0.0}, healed {healed:0.0}");

        // Iron Thorns: each weapon blow snaps a thorn at A, 6, Seeded and a notch; three thorns.
        yield return Pair("Iron Thorns", host, AfflictionKind.Sundered, 1, AfflictionKind.Seeded, 1, () => aHits = a.HitsTaken);
        yield return Strike(host, false);
        Judge("Iron Thorns: a blow snaps a thorn at A: 6, Seeded and a notch", a.HitsTaken == aHits + 1 && a.Afflictions.Has(AfflictionKind.Seeded) && a.Afflictions.Has(AfflictionKind.Sundered),
            $"A hits {a.HitsTaken - aHits}, last {a.LastDamage:0.0}");
        yield return Strike(host, false);
        yield return Strike(host, false);
        Judge("Iron Thorns: three thorns, then none", a.HitsTaken == aHits + 3 && !Holds(host, R.Thorns), $"A hits {a.HitsTaken - aHits}");

        // Earthlight: light through the seams, 14 and Marked on A; its own Mark renewed.
        yield return Pair("Earthlight", host, AfflictionKind.Marked, 1, AfflictionKind.Tremor, 1, () => aHits = a.HitsTaken);
        yield return Real(0.8f);
        Judge("Earthlight: 14 and Marked to A, its own Mark renewed", a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 14f) < 0.6f && a.Afflictions.Has(AfflictionKind.Marked)
            && host.Afflictions.BlowsLeft(AfflictionKind.Marked) >= 3, $"A last {a.LastDamage:0.0}, host blows left {host.Afflictions.BlowsLeft(AfflictionKind.Marked)}");

        // Black Chain: A, the nearest, is chained to it for 5 s.
        yield return Pair("Black Chain", host, AfflictionKind.Sundered, 1, AfflictionKind.Cursed, 1);
        Judge("Black Chain: the nearest (A) is chained to it, and no other", Holds(a, R.Chain) && !Holds(b, R.Chain) && !Holds(c, R.Chain) && !Holds(host, R.Chain));

        // Burning Sap: two blows in its 3 s raise two weals; at the end each bursts two doses on A (four).
        yield return Pair("Burning Sap", host, AfflictionKind.Poison, 2, AfflictionKind.Marked, 1);
        yield return Strike(host, false, 0.2f);
        yield return Strike(host, false, 0.2f);
        yield return Until(() => !Holds(host, R.SapWaits), 3.5f);
        yield return Real(0.3f);
        Judge("Burning Sap: two weals burst, two doses each on A", Depth(a, AfflictionKind.Poison) == 4, $"A doses {Depth(a, AfflictionKind.Poison)}");

        // Smoulder: the Burn deepens every 2 s and does not fade; the curse ended, it flares 10 a stack (30) on A.
        yield return Pair("Smoulder", host, AfflictionKind.Burn, 1, AfflictionKind.Cursed, 1, () => aHits = a.HitsTaken);
        yield return Real(4.3f);
        Judge("Smoulder: sunk in, its Burn deepened to 3 and still burns past its 4 s", host.Afflictions.Burn.IsBurning && Depth(host, AfflictionKind.Burn) == 3, $"burn {Depth(host, AfflictionKind.Burn)}");
        host.Afflictions.Spend(AfflictionKind.Cursed, 1);
        yield return Real(0.4f);
        yield return Shot("p3b_smoulder");
        Judge("Smoulder: the curse gone, it flares: 30 to A, lit", !Holds(host, R.Sunk) && a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 30f) < 0.6f && a.Afflictions.Burn.IsBurning,
            $"A last {a.LastDamage:0.0}");

        // Nerve Fire: each bite spends a Static and jolts it for 7.
        int hostHits = 0;
        yield return Pair("Nerve Fire", host, AfflictionKind.Poison, 2, AfflictionKind.Static, 3, () => { hostBites = 0; hostHits = host.HitsTaken; });
        yield return Until(() => hostBites > 0, 2.5f);
        yield return Real(0.3f);
        Judge("Nerve Fire: the bite spends a Static and jolts it, 7", Holds(host, R.Nerves) && Depth(host, AfflictionKind.Static) == 2 && host.HitsTaken > hostHits,
            $"bites {hostBites}, static {Depth(host, AfflictionKind.Static)}, hits {host.HitsTaken - hostHits}, last {host.LastDamage:0.0}");
    }

    private IEnumerator DistanceThree()
    {
        // Thermal Shock: 14, Cracked; the next heavy splits it for 30.
        yield return Pair("Thermal Shock", host, AfflictionKind.Burn, 1, AfflictionKind.Chill, 1);
        yield return Real(0.2f);
        Judge("Thermal Shock: the body cracks, 14", Mathf.Abs(host.LastDamage - 14f) < 0.6f && Holds(host, R.Cracked), $"last {host.LastDamage:0.0}");
        yield return Strike(host, true);
        Judge("Thermal Shock: the heavy splits it, 30; both afflictions stay", Mathf.Abs(host.LastDamage - 30f) < 0.6f && !Holds(host, R.Cracked) && host.Afflictions.Burn.IsBurning && host.Afflictions.Has(AfflictionKind.Chill),
            $"last {host.LastDamage:0.0}");

        // Seized: waits for two Static; then held 1 s and a live film charges A.
        yield return WaitsForTwoStatic("Seized", AfflictionKind.Soaked);
        int pair = PairNamed("Seized");
        int shown = reactions.TimesShown(pair), alive = Hazards.Alive;
        Lay(host, AfflictionKind.Static, 1);
        yield return Until(() => reactions.TimesShown(pair) > shown, 1.5f);
        Judge("Seized: the second Static sets it off", reactions.TimesShown(pair) == shown + 1 && Mathf.Abs(host.Afflictions.ReactionRest(pair) - ReactionTable.RestFor(pair)) < 0.5f);
        Judge("Seized: held 1 s, and a live film on the floor", host.Afflictions.IsHeld && Hazards.Alive > alive, $"held {host.Afflictions.HeldLeft:0.00} s");
        yield return Real(1.2f);
        Judge("Seized: A, standing in the film, is charged", a.Afflictions.Has(AfflictionKind.Static), $"A static {Depth(a, AfflictionKind.Static)}");

        // Windthrow: rooted 2 s; a heavy tears it out: 14, thrown 6 m away, a pit.
        int shoves = 0;
        yield return Pair("Windthrow", host, AfflictionKind.Seeded, 1, AfflictionKind.Unsteady, 1, () => shoves = host.Shoves);
        Judge("Windthrow: rooted", Holds(host, R.WindRooted) && host.Afflictions.Conditions.Has(ConditionFlags.Rooted));
        alive = Hazards.Alive;
        yield return Strike(host, true);
        Judge("Windthrow: the heavy tears it out: 14, thrown 6 m away, and a pit", Mathf.Abs(host.LastDamage - 14f) < 0.6f && host.Shoves == shoves + 1 && host.LastShove.z > 5f && Hazards.Alive > alive && !Holds(host, R.WindRooted),
            $"last {host.LastDamage:0.0}, shove {host.LastShove}");

        // Frost Heave: 0.4 s on, the floor heaves it up: in the air, 12.
        yield return Pair("Frost Heave", host, AfflictionKind.Tremor, 1, AfflictionKind.Chill, 1);
        yield return Until(() => host.Afflictions.IsLifted, 1f);
        Judge("Frost Heave: the floor heaves it up, 12", host.Afflictions.IsLifted && Mathf.Abs(host.LastDamage - 12f) < 0.6f, $"lifted {host.Afflictions.IsLifted}, last {host.LastDamage:0.0}");

        // Rust: the notches spread one every 2 s while wet, and once full a plate falls (16% more from the weapon).
        yield return Pair("Rust", host, AfflictionKind.Sundered, 1, AfflictionKind.Soaked, 1);
        yield return Real(2.3f);
        Judge("Rust: the rust spreads a notch", Depth(host, AfflictionKind.Sundered) == 2, $"notches {Depth(host, AfflictionKind.Sundered)}");
        yield return Real(4.1f);
        Judge("Rust: rusted through, a plate falls", Holds(host, R.PlateFallen) && Mathf.Abs(host.Afflictions.Conditions.WeaponTakenScale - 1.16f) < 0.01f,
            $"notches {Depth(host, AfflictionKind.Sundered)}, weapon x{host.Afflictions.Conditions.WeaponTakenScale:0.00}");

        // Shining Herb: left unpicked 6 s, it flowers: A Marked and Seeded.
        yield return Pair("Shining Herb", host, AfflictionKind.Seeded, 1, AfflictionKind.Marked, 1);
        Judge("Shining Herb: the herb stands", Holds(host, R.Herb) && !a.Afflictions.Has(AfflictionKind.Marked));
        yield return Real(6.4f);
        Judge("Shining Herb: unpicked, it flowers: A Marked and Seeded", a.Afflictions.Has(AfflictionKind.Marked) && a.Afflictions.Has(AfflictionKind.Seeded));

        // Turned to Stone: slowed 30%, then 60%, then stone: held, double stagger, half from spells; a heavy breaks it out.
        yield return Pair("Turned to Stone", host, AfflictionKind.Tremor, 1, AfflictionKind.Cursed, 1);
        yield return Real(0.3f);
        float first = host.Afflictions.Conditions.Pace;
        yield return Real(1f);
        float second = host.Afflictions.Conditions.Pace;
        yield return Until(() => Holds(host, R.Stone), 1.5f);
        var held = host.Afflictions.Conditions;
        Judge("Turned to Stone: stone climbs it, 30% then 60% slower, then it stands as stone", Mathf.Abs(first - 0.7f) < 0.01f && Mathf.Abs(second - 0.4f) < 0.01f && held.Has(ConditionFlags.Rooted)
            && held.Has(ConditionFlags.BeginsNoAttack) && Mathf.Abs(held.StaggerScale - 2f) < 0.01f && Mathf.Abs(held.SpellTakenScale - 0.5f) < 0.01f,
            $"pace {first:0.00} then {second:0.00}, stagger x{held.StaggerScale:0.00}, spells x{held.SpellTakenScale:0.00}");
        yield return Strike(host, true);
        Judge("Turned to Stone: a heavy breaks it out", !Holds(host, R.Stone) && !held.Has(ConditionFlags.Rooted));

        // Quicksilver: a weapon blow makes its poison bite at once.
        yield return Pair("Quicksilver", host, AfflictionKind.Sundered, 1, AfflictionKind.Poison, 2, () => hostBites = 0);
        int bites = hostBites;
        yield return Strike(host, false, 0.25f);
        Judge("Quicksilver: a weapon blow makes the poison bite at once", Holds(host, R.Beads) && hostBites > bites, $"bites {hostBites - bites}");

        // Kindling Point: each blow deepens the Burn; the third sets it alight, 10 a Burn (40), and it burns on two deep.
        yield return Pair("Kindling Point", host, AfflictionKind.Burn, 1, AfflictionKind.Marked, 1);
        yield return Strike(host, false, 0.2f);
        Judge("Kindling Point: a critical blow deepens the Burn", Depth(host, AfflictionKind.Burn) == 2, $"burn {Depth(host, AfflictionKind.Burn)}");
        yield return Strike(host, false, 0.2f);
        yield return Strike(host, false, 0.3f);
        Judge("Kindling Point: the third sets it alight, 40, and it burns on two deep", Mathf.Abs(host.LastDamage - 40f) < 0.6f && Depth(host, AfflictionKind.Burn) == 2 && !Holds(host, R.BurningPoint),
            $"last {host.LastDamage:0.0}, burn {Depth(host, AfflictionKind.Burn)}");

        // The Grudge: waits for two Static; then a cloud gathers, and it rests 8 s.
        yield return WaitsForTwoStatic("The Grudge", AfflictionKind.Cursed);
        pair = PairNamed("The Grudge");
        shown = reactions.TimesShown(pair);
        Lay(host, AfflictionKind.Static, 1);
        yield return Until(() => reactions.TimesShown(pair) > shown, 1.5f);
        Judge("The Grudge: the second Static gathers the cloud, resting 8 s", reactions.TimesShown(pair) == shown + 1 && Holds(host, R.Cloud) && host.Afflictions.ReactionRest(pair) > 7.5f,
            $"rest {host.Afflictions.ReactionRest(pair):0.0}");

        // Ill Wind: a heavy shoves it and it sheds all its poison in a plume behind it, dosing C.
        yield return Pair("Ill Wind", host, AfflictionKind.Poison, 3, AfflictionKind.Unsteady, 1);
        Judge("Ill Wind: its poison goes loose", Holds(host, R.LooseDust));
        alive = Hazards.Alive;
        yield return Strike(host, true);
        Judge("Ill Wind: the heavy sheds all of it in a plume behind it", Depth(host, AfflictionKind.Poison) == 0 && Hazards.Alive >= alive + 3, $"doses {Depth(host, AfflictionKind.Poison)}, patches {Hazards.Alive - alive}");
        yield return Real(1.2f);
        Judge("Ill Wind: C, in the plume, is dosed", c.Afflictions.Has(AfflictionKind.Poison));
    }

    private IEnumerator DistanceFour()
    {
        // Lightning Flower: a bolt to the nearest other Seeded (A): 8 and a Static; C, clean, is left alone.
        int aHits = 0, cHits = 0;
        yield return Pair("Lightning Flower", host, AfflictionKind.Seeded, 1, AfflictionKind.Static, 1, () => { Lay(a, AfflictionKind.Seeded, 1); aHits = a.HitsTaken; cHits = c.HitsTaken; });
        yield return Real(0.2f);
        Judge("Lightning Flower: a bolt to the Seeded A, 8 and a Static, not to C", a.HitsTaken == aHits + 1 && Mathf.Abs(a.LastDamage - 8f) < 0.6f && a.Afflictions.Has(AfflictionKind.Static) && c.HitsTaken == cHits,
            $"A last {a.LastDamage:0.0}");

        // Logan Stone: it rocks; blows break its balance harder; the third throws it down: flat 1.8 s, 20 to A.
        yield return Pair("Logan Stone", host, AfflictionKind.Unsteady, 1, AfflictionKind.Tremor, 1, () => aHits = a.HitsTaken);
        Judge("Logan Stone: it rocks, balance breaking at 1.25", Holds(host, R.Rocking) && Mathf.Abs(host.Afflictions.Conditions.StaggerScale - 1.25f) < 0.01f);
        yield return Strike(host, false, 0.2f);
        Judge("Logan Stone: then at 1.5", Mathf.Abs(host.Afflictions.Conditions.StaggerScale - 1.5f) < 0.01f, $"x{host.Afflictions.Conditions.StaggerScale:0.00}");
        yield return Strike(host, false, 0.2f);
        yield return Strike(host, false, 0.3f);
        Judge("Logan Stone: the third throws it down hard: down 1.8 s, 20 and a Tremor to A", Holds(host, R.Felled) && a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 20f) < 0.6f && !Holds(host, R.Rocking),
            $"down {host.Afflictions.Conditions.Left(R.Felled):0.00} s, A last {a.LastDamage:0.0}");

        // Cold Short: brittle, notches kept; the heavy cracks the plates off: 25% more from the weapon, notches stay.
        yield return Pair("Cold Short", host, AfflictionKind.Sundered, 1, AfflictionKind.Chill, 1);
        Judge("Cold Short: brittle, its notches kept", Holds(host, R.Brittle) && host.Afflictions.Conditions.KeepsKind(AfflictionKind.Sundered));
        yield return Strike(host, true);
        Judge("Cold Short: the heavy cracks the plates off: 25% more from the weapon, the notches kept", Holds(host, R.PlatesOff) && Holds(host, R.Bare)
            && Mathf.Abs(host.Afflictions.Conditions.WeaponTakenScale - 1.25f) < 0.01f && !Holds(host, R.Brittle), $"weapon x{host.Afflictions.Conditions.WeaponTakenScale:0.00}");

        // Caught Light: a net 3 m round it: A caught (a quarter slower) and Marked.
        int alive = 0;
        yield return Pair("Caught Light", host, AfflictionKind.Soaked, 1, AfflictionKind.Marked, 1, () => alive = Hazards.Alive);
        yield return Real(0.8f);
        Judge("Caught Light: the net lies, and A in it is caught and Marked", Hazards.Alive > alive && Holds(a, R.LightNet) && Mathf.Abs(a.Afflictions.Conditions.Pace - 0.75f) < 0.01f && a.Afflictions.Has(AfflictionKind.Marked),
            $"A pace {a.Afflictions.Conditions.Pace:0.00}");

        // Girdled: one coil (6% more), each blow tighter and mends you 1; the third blow girdles it: held, 20, you heal 4 more.
        yield return Pair("Girdled", host, AfflictionKind.Seeded, 1, AfflictionKind.Cursed, 1);
        var coils = host.Afflictions.Conditions;
        Judge("Girdled: one coil: slowed, 6% more from the weapon", Holds(host, R.Coils) && Mathf.Abs(coils.Pace - 0.75f) < 0.01f && Mathf.Abs(coils.WeaponTakenScale - 1.06f) < 0.01f,
            $"pace {coils.Pace:0.00}, weapon x{coils.WeaponTakenScale:0.00}");
        Hurt();
        yield return Strike(host, false, 0.2f);
        Judge("Girdled: a blow draws it tighter (12%)", Mathf.Abs(coils.WeaponTakenScale - 1.12f) < 0.01f, $"x{coils.WeaponTakenScale:0.00}");
        yield return Strike(host, false, 0.2f);
        yield return Strike(host, false, 0.3f);
        float healed = Healed();
        Judge("Girdled: at four coils it is crushed: held, 20, and you mended 3 and healed 4", host.Afflictions.IsHeld && host.LastDamage > 19f && host.LastDamage < 26f && Mathf.Abs(healed - 7f) < 1f && !Holds(host, R.Coils),
            $"held {host.Afflictions.HeldLeft:0.00} s, last {host.LastDamage:0.0}, healed {healed:0.0}");

        // Chokedamp: the damp runs to the shaken A, not the clean B and C; A takes a dose a second, slowed.
        yield return Pair("Chokedamp", host, AfflictionKind.Tremor, 1, AfflictionKind.Poison, 1, () => Lay(a, AfflictionKind.Tremor, 1));
        Judge("Chokedamp: the damp pools under it and runs to the shaken A only", Holds(host, R.Damp) && Holds(a, R.Damp) && Holds(a, R.DampHeavy) && !Holds(b, R.Damp) && !Holds(c, R.Damp));
        yield return Real(1.3f);
        Judge("Chokedamp: in the damp A takes a dose and is slowed", a.Afflictions.Has(AfflictionKind.Poison) && Mathf.Abs(a.Afflictions.Conditions.Pace - 0.8f) < 0.01f, $"A pace {a.Afflictions.Conditions.Pace:0.00}");

        // Red Hot: red for 5 s; a heavy sprays sparks out of its far side: C lit, A not.
        yield return Pair("Red Hot", host, AfflictionKind.Sundered, 1, AfflictionKind.Burn, 1, () => { aHits = a.HitsTaken; cHits = c.HitsTaken; });
        Judge("Red Hot: the armour glows", Holds(host, R.Glowing));
        yield return Strike(host, true);
        Judge("Red Hot: the heavy sprays sparks behind it: C lit, A untouched", c.HitsTaken > cHits && c.Afflictions.Burn.IsBurning && a.HitsTaken == aHits, $"C hits {c.HitsTaken - cHits}");

        // The Bolt Runs True: 0.5 s on, a bolt of 20 on it and its Static discharged at full (14 to A); the Mark kept.
        int hostHits = 0;
        yield return Pair("The Bolt Runs True", host, AfflictionKind.Marked, 1, AfflictionKind.Static, 2, () => { aHits = a.HitsTaken; hostHits = host.HitsTaken; });
        yield return Real(0.8f);
        yield return Shot("p3b_bolt");
        Judge("The Bolt Runs True: the bolt and the discharge: 14 to A, its Static spent, the Mark kept", host.HitsTaken >= hostHits + 1 && a.HitsTaken > aHits && Mathf.Abs(a.LastDamage - 14f) < 0.6f
            && Depth(host, AfflictionKind.Static) == 0 && host.Afflictions.Has(AfflictionKind.Marked), $"host hits {host.HitsTaken - hostHits}, A last {a.LastDamage:0.0}");

        // Breath Taken: winded: begins no attack, takes 30% more stagger.
        yield return Pair("Breath Taken", host, AfflictionKind.Cursed, 1, AfflictionKind.Unsteady, 1);
        Judge("Breath Taken: winded: it begins no attack and takes 30% more stagger", Holds(host, R.Winded) && host.Afflictions.Conditions.Has(ConditionFlags.BeginsNoAttack)
            && Mathf.Abs(host.Afflictions.Conditions.StaggerScale - 1.3f) < 0.01f);
        yield return Rests("Breath Taken", host, AfflictionKind.Cursed);

        // Venom Rime: each bite lays a Chill; the poison kept.
        yield return Pair("Venom Rime", host, AfflictionKind.Poison, 2, AfflictionKind.Chill, 1, () => hostBites = 0);
        yield return Until(() => hostBites > 0, 2.5f);
        yield return Real(0.3f);
        Judge("Venom Rime: the bite lays a Chill, and the poison does not fade", Holds(host, R.VenomCrust) && Depth(host, AfflictionKind.Chill) == 2 && host.Afflictions.Conditions.KeepsKind(AfflictionKind.Poison),
            $"chill {Depth(host, AfflictionKind.Chill)}");
    }

    private IEnumerator DistanceFive()
    {
        // Firebloom: the two seeds are spent and grow into two flame lilies; its Burn stays.
        int alive = 0;
        yield return Pair("Firebloom", host, AfflictionKind.Seeded, 2, AfflictionKind.Burn, 1, () => alive = Hazards.Alive);
        yield return Real(0.2f);
        yield return Shot("p3b_firebloom");
        Judge("Firebloom: a lily for each seed (two), the seeds spent, the Burn kept", Hazards.Alive == alive + 2 && !host.Afflictions.Has(AfflictionKind.Seeded) && host.Afflictions.Burn.IsBurning,
            $"lilies {Hazards.Alive - alive}");

        // Earthed: its three Static run to ground: 15 to A and a Tremor; it is rooted 1 s.
        float aHealth = 0f;
        yield return Pair("Earthed", host, AfflictionKind.Tremor, 1, AfflictionKind.Static, 3, () => aHealth = a.Health.Current);
        yield return Real(0.2f);
        Judge("Earthed: the charge runs to ground: 15 to A and a Tremor, its Static gone, rooted", Mathf.Abs(aHealth - a.Health.Current - 15f) < 0.6f && a.Afflictions.Has(AfflictionKind.Tremor)
            && Depth(host, AfflictionKind.Static) == 0 && Holds(host, R.Grounded), $"A lost {aHealth - a.Health.Current:0.0}");

        // Sickle Wind: its plates whirl round it and cut it too, 3 a shard (two notches: three shards, 9; its own notches
        // open it to the cut, about 10.4).
        int hostHits = 0, aHits = 0;
        yield return Pair("Sickle Wind", host, AfflictionKind.Sundered, 2, AfflictionKind.Unsteady, 1, () => { alive = Hazards.Alive; hostHits = host.HitsTaken; });
        yield return Real(1f);
        Judge("Sickle Wind: the whirl cuts it, about 9 a cut", Hazards.Alive > alive && host.HitsTaken > hostHits && host.LastDamage > 8.5f && host.LastDamage < 11f,
            $"cuts {host.HitsTaken - hostHits}, last {host.LastDamage:0.0}");

        // Mock Suns: a weapon blow lays two Chill and the Mark is not spent.
        yield return Pair("Mock Suns", host, AfflictionKind.Marked, 1, AfflictionKind.Chill, 1);
        yield return Strike(host, false);
        for (int i = 0; i < 3; i++) host.Afflictions.BlowLanded();
        Judge("Mock Suns: a blow lays two Chill, and the Mark is kept", Holds(host, R.Suns) && Depth(host, AfflictionKind.Chill) == 3 && host.Afflictions.Has(AfflictionKind.Marked),
            $"chill {Depth(host, AfflictionKind.Chill)}");

        // Drowned Weight: swollen, slower, 15% more from the weapon; a heavy bursts it: 14, Soaked and Cursed on A.
        yield return Pair("Drowned Weight", host, AfflictionKind.Soaked, 1, AfflictionKind.Cursed, 1, () => aHits = a.HitsTaken);
        var swollen = host.Afflictions.Conditions;
        Judge("Drowned Weight: it swells: 20% slower, 15% more from the weapon", Holds(host, R.Swollen) && Mathf.Abs(swollen.Pace - 0.8f) < 0.01f && Mathf.Abs(swollen.WeaponTakenScale - 1.15f) < 0.01f);
        yield return Strike(host, true);
        Judge("Drowned Weight: the heavy bursts it: A takes 14, Soaked and Cursed", a.HitsTaken > aHits && a.Afflictions.Has(AfflictionKind.Soaked) && a.Afflictions.Has(AfflictionKind.Cursed) && !Holds(host, R.Swollen),
            $"A hits {a.HitsTaken - aHits}");

        // Burnt Mound: three hot stones round it.
        yield return Pair("Burnt Mound", host, AfflictionKind.Tremor, 1, AfflictionKind.Burn, 1, () => alive = Hazards.Alive);
        yield return Real(0.2f);
        Judge("Burnt Mound: three stones thrown up round it", Hazards.Alive == alive + 3, $"{Hazards.Alive - alive} stones");

        // Golden Feathers: feathers ride downwind (north) onto C: Marked and Unsteady; the host keeps its Mark.
        yield return Pair("Golden Feathers", host, AfflictionKind.Marked, 1, AfflictionKind.Unsteady, 1);
        yield return Real(0.6f);
        Judge("Golden Feathers: C, downwind, is Marked and made Unsteady; the host keeps its Mark", c.Afflictions.Has(AfflictionKind.Marked) && c.Afflictions.Has(AfflictionKind.Unsteady) && host.Afflictions.Has(AfflictionKind.Marked)
            && !a.Afflictions.Has(AfflictionKind.Marked));

        // Stolen Warmth: each weapon blow chills it and draws 2 health into you.
        yield return Pair("Stolen Warmth", host, AfflictionKind.Cursed, 1, AfflictionKind.Chill, 1);
        Hurt();
        yield return Strike(host, false);
        float healed = Healed();
        Judge("Stolen Warmth: a blow chills it and draws 2 health into you", Holds(host, R.Draw) && Depth(host, AfflictionKind.Chill) == 2 && Mathf.Abs(healed - 2f) < 0.5f, $"chill {Depth(host, AfflictionKind.Chill)}, healed {healed:0.0}");

        // Fish Poison: a slick; four doses deep it floats up stunned and the next blow is critical, spending two doses.
        yield return Pair("Fish Poison", host, AfflictionKind.Poison, 4, AfflictionKind.Soaked, 1, () => alive = Hazards.Alive);
        yield return Real(0.2f);
        Judge("Fish Poison: the slick lies, and four doses deep it floats up stunned", Hazards.Alive > alive && host.Afflictions.IsLifted && Holds(host, R.Floating) && host.Afflictions.MakesBlowsCritical);
        yield return Strike(host, false);
        Judge("Fish Poison: the next weapon blow spends two doses for 12", Depth(host, AfflictionKind.Poison) == 2 && Mathf.Abs(host.LastDamage - 12f) < 0.6f && !Holds(host, R.Floating),
            $"doses {Depth(host, AfflictionKind.Poison)}, last {host.LastDamage:0.0}");
        int fish = PairNamed("Fish Poison"), fishShown = reactions.TimesShown(fish);
        Lay(d, AfflictionKind.Poison, 2);
        Lay(d, AfflictionKind.Soaked, 1);
        yield return Until(() => reactions.TimesShown(fish) > fishShown, 1.5f);
        yield return Real(0.2f);
        Judge("Fish Poison: two doses deep, D reacts but does not float", reactions.TimesShown(fish) == fishShown + 1 && !d.Afflictions.IsLifted && !Holds(d, R.Floating));
    }

    // A real soldier: Nerve Fire's jolt makes it flinch, and its death is answered by what it carried.
    private IEnumerator TheSoldier()
    {
        yield return Clean();
        var castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).FirstOrDefault(pen => pen.Land == "The castle");
        Judge("the castle's pen is found", castle != null);
        if (castle == null) yield break;
        castle.Release(new[] { 0 });
        var soldier = castle.Enemies[0];
        yield return Real(0.5f);
        Judge("a soldier is let out of the castle's pen", soldier != null && soldier.gameObject.activeInHierarchy);
        if (soldier == null) yield break;

        // The poison bites 2 s after it is laid: the soldier is made to wind up just before, so the jolt has an attack to break.
        int nerve = PairNamed("Nerve Fire");
        int shown = reactions.TimesShown(nerve), flinches = soldier.EffectFlinches;
        Vector3 stand = soldier.transform.position + Vector3.back * 1.7f;
        stand.y = 0.05f;
        player.Teleport(stand, Quaternion.identity);
        soldier.Afflictions.Lay(AfflictionKind.Poison, 2, 0f, 1f, LaySource.Spell, soldier);
        soldier.Afflictions.Lay(AfflictionKind.Static, 3, 0f, 1f, LaySource.Spell, soldier);
        float laid = Time.time;
        yield return Until(() => reactions.TimesShown(nerve) > shown, 1.5f);
        yield return Until(() => Time.time - laid > 1.7f, 2f);
        if (soldier.Brain.Attacks.Count > 0) soldier.Brain.Force(soldier.Brain.Attacks[0].Name);
        yield return Until(() => soldier.EffectFlinches > flinches, 2f);
        player.Teleport(host.transform.position + Vector3.back * 3f, Quaternion.identity);
        Judge(soldier.Kind.Steadfast ? "Nerve Fire on a steadfast soldier: shown, no flinch" : "Nerve Fire: the bite jolts the soldier into a flinch",
            reactions.TimesShown(nerve) == shown + 1 && (soldier.Kind.Steadfast ? soldier.EffectFlinches == flinches : soldier.EffectFlinches > flinches), $"flinches {soldier.EffectFlinches - flinches}");
        soldier.Afflictions.Clear();
        yield return Real(0.3f);

        // Smoulder and Shut in the Dark at once (Cursed laid last meets both), then it dies: four answers, a burst on the floor.
        int smoulder = PairNamed("Smoulder"), jar = PairNamed("Shut in the Dark");
        int smoulderShown = reactions.TimesShown(smoulder), jarShown = reactions.TimesShown(jar);
        soldier.Afflictions.Lay(AfflictionKind.Burn, 2, 0f, 1f, LaySource.Spell, soldier);
        soldier.Afflictions.Lay(AfflictionKind.Poison, 4, 0f, 1f, LaySource.Spell, soldier);
        soldier.Afflictions.Lay(AfflictionKind.Cursed, 1, 0f, 1f, LaySource.Spell, soldier);
        yield return Until(() => reactions.TimesShown(smoulder) > smoulderShown && reactions.TimesShown(jar) > jarShown, 1.5f);
        yield return Real(0.2f);
        Judge("the soldier carries Smoulder and Shut in the Dark", soldier.Afflictions.Conditions.Holds(R.Sunk) && soldier.Afflictions.Conditions.Holds(R.Jar));
        int answers = reactions.ConditionAnswers, alive = Hazards.Alive;
        soldier.TakeHit(new Hit(9999f, 0f, Vector3.forward, unblockable: true));
        bool burst = false;
        float end = Time.unscaledTime + 0.6f;
        while (Time.unscaledTime < end)
        {
            if (Hazards.Alive > alive) burst = true;
            yield return null;
        }
        Judge("killed, its death is answered: the flare and the jar's break (four answers), and they burst on the floor", soldier.IsDead && reactions.ConditionAnswers >= answers + 4 && burst,
            $"dead {soldier.IsDead}, answers {reactions.ConditionAnswers - answers}");
        castle.Clear();
    }

    private void Finish()
    {
        CombatEvents.PoisonBit -= OnPoisonBit;
        healing = true;
        foreach (var dummy in made) if (dummy != null) Destroy(dummy.gameObject);
        if (build != null) build.Attune(Elements.Fire, "");
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
