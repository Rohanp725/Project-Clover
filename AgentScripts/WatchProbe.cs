using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees.Duo;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Phase 4 steps 5 and 6, the commits to watch (phase4_merge_plan.md, item 8), in the Proving Ground with the playtest on:
// - Cold Damage (Venom Rime's second shape): the heavy blow that shatters a frozen enemy sets its doses off, though the
//   shatter has ended the Chill by the time the answer runs (StruckBy.Shatter).
// - Six Levels Down: each bite on a frozen enemy keeps it frozen 0.3 s longer (KeepFrozen), so it stays frozen past 2.5 s.
// - Fast Water: poison on a Soaked enemy bites more often, and its own bite does not feed itself without end.
// - A reaction laid and forced in the same frame is shown once (The Sisters' Quarrel's pattern), through the live engine.
// Each judged line ends OK or WRONG. Progress stays sandboxed (runprobe.sh).
// bash AgentScripts/runprobe.sh ProvingGround WatchProbe 60
public static class WatchProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Watch Probe").AddComponent<WatchProbeRunner>();
        return "probe started";
    }
}

public class WatchProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerUpgrades upgrades;
    private PlayerBuild build;
    private PlayerReactions reactions;
    private TrainingDummy still, host;
    private int ok, wrong;
    private bool playtestWas;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);

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

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while ((ReadingBox.Instance != null && ReadingBox.Instance.IsOpen || GuidePage.Showing) && guard++ < 60)
        {
            Keys(Key.E);
            yield return null;
            yield return null;
            Keys();
            yield return null;
            yield return null;
        }
    }

    private IEnumerator Fresh()
    {
        if (host != null) Destroy(host.gameObject);
        yield return null;
        host = Instantiate(still.gameObject, Middle, still.transform.rotation).GetComponent<TrainingDummy>();
        host.name = "Probe Host";
        yield return Until(() => Hazards.Alive == 0, 3f);
        player.Teleport(Middle + Vector3.back * 3f, Quaternion.identity);
        yield return Real(0.2f);
    }

    private void Lay(AfflictionKind kind, int stacks) => host.Afflictions.Lay(kind, stacks, 0f, 1f, LaySource.Spell, host);

    private void Give(string id)
    {
        upgrades.Give(Upgrade.ById(id));
        upgrades.RefreshNow();
    }

    private void Take(string id)
    {
        RunState.Unlocked.Remove(id);
        upgrades.RefreshNow();
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
        Judge("progress is sandboxed", Progress.Sandboxed);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null) { Finish(); yield break; }

        yield return ColdDamage();
        yield return SixLevelsDown();
        yield return FastWater();
        yield return LaidAndForced();
        Finish();
    }

    private IEnumerator ColdDamage()
    {
        Say("--- Cold Damage (Venom Rime's second shape)");
        build.Attune(Elements.Poison, Elements.Ice);
        Give(VenomRimeDuo.Shape);
        int pair = AfflictionPairs.Index(AfflictionKind.Poison, AfflictionKind.Chill);
        Judge("Cold Damage chosen at a rest fire", build.ChooseShape(ReactionTable.ShapeKey(pair), 1, true));
        yield return Fresh();
        int shown = reactions.TimesShown(pair);
        Lay(AfflictionKind.Poison, 4);
        yield return null;
        Lay(AfflictionKind.Chill, 2);
        yield return Until(() => reactions.TimesShown(pair) > shown, 1.5f);
        Judge("Venom Rime runs in Cold Damage", reactions.TimesShown(pair) > shown && reactions.LastShape == "Cold Damage", $"shape '{reactions.LastShape}'");
        Judge("its condition holds on the host", host.Afflictions.Conditions.Holds(VenomRimeDuo.ColdDamage));
        Lay(AfflictionKind.Chill, 5);
        yield return null;
        Judge("the host freezes", host.Afflictions.IsFrozen);
        int doses = host.Afflictions.Stacks(AfflictionKind.Poison);
        float before = host.Health.Current;
        host.TakeHit(new Hit(10f, 0f, Vector3.forward, heavy: true));
        float shatterOnly = before - host.Health.Current;
        yield return Real(0.3f);
        float after = host.Health.Current;
        int left = host.Afflictions.Stacks(AfflictionKind.Poison);
        Judge("the shattering heavy blow sets every dose off", doses > 0 && left == 0, $"doses {doses} -> {left}");
        Judge("and they land on top of the shatter, 6 a dose", before - after >= shatterOnly + 6f * doses - 0.5f,
            $"shatter and blow {shatterOnly:0.0}, all {before - after:0.0}, owed {6f * doses:0.0} more");
        yield return Fresh();
        Lay(AfflictionKind.Poison, 4);
        yield return null;
        Lay(AfflictionKind.Chill, 2);
        yield return Real(0.3f);
        doses = host.Afflictions.Stacks(AfflictionKind.Poison);
        host.TakeHit(new Hit(10f, 0f, Vector3.forward, heavy: true));
        yield return Real(0.3f);
        Judge("a heavy blow on it unfrozen sets nothing off", host.Afflictions.Stacks(AfflictionKind.Poison) == doses, $"doses {doses} -> {host.Afflictions.Stacks(AfflictionKind.Poison)}");
        Take(VenomRimeDuo.Shape);
    }

    private IEnumerator SixLevelsDown()
    {
        Say("--- Six Levels Down");
        float plain = 0f, kept = 0f;
        for (int run = 0; run < 2; run++)
        {
            if (run == 1) Give(VenomRimeDuo.SixLevelsDown);
            yield return Fresh();
            Lay(AfflictionKind.Poison, 4);
            yield return null;
            Lay(AfflictionKind.Chill, 5);
            yield return null;
            float start = Time.time;
            yield return Until(() => !host.Afflictions.IsFrozen, 8f);
            if (run == 0) plain = Time.time - start; else kept = Time.time - start;
        }
        Judge("without it the freeze lasts its 2.5 s", Mathf.Abs(plain - 2.5f) < 0.3f, $"{plain:0.00} s");
        Judge("with it each bite keeps the freeze 0.3 s longer", kept > plain + 0.25f, $"{kept:0.00} s, freezes kept {upgrades.FreezesKept}");
        Take(VenomRimeDuo.SixLevelsDown);
    }

    private IEnumerator FastWater()
    {
        Say("--- Fast Water");
        build.Attune(Elements.Poison, Elements.Water);
        int plain = 0, fast = 0;
        for (int run = 0; run < 2; run++)
        {
            if (run == 1) Give(FishPoisonDuo.FastWater);
            yield return Fresh();
            // Fish Poison itself would take the Soaked away: rested, the poison bites on a body that stays Soaked.
            host.Afflictions.RestReaction(AfflictionPairs.Index(AfflictionKind.Poison, AfflictionKind.Soaked), 30f);
            Lay(AfflictionKind.Soaked, 1);
            yield return null;
            int bites = upgrades.PoisonBites;
            int done = upgrades.EffectsDone;
            var times = new System.Text.StringBuilder();
            float from = Time.time;
            Action<Component, int, float> heard = (who, depth, damage) => { if (who == host) times.Append($" {Time.time - from:0.00}"); };
            CombatEvents.PoisonBit += heard;
            Lay(AfflictionKind.Poison, 2);
            yield return Real(4f);
            CombatEvents.PoisonBit -= heard;
            Say($"  run {run}: bites at{times}, effects done {upgrades.EffectsDone - done}");
            if (run == 0) plain = upgrades.PoisonBites - bites; else fast = upgrades.PoisonBites - bites;
            Say($"  run {run}: Fast Water in force {upgrades.Has(FishPoisonDuo.FastWater)}, Soaked still {host.Afflictions.Has(AfflictionKind.Soaked)}, poison {host.Afflictions.Stacks(AfflictionKind.Poison)}");
        }
        Judge("with it a Soaked enemy's poison bites more often", fast > plain, $"bites in 4 s: {plain} without, {fast} with");
        Judge("and no more than twice a second more: its own bite does not feed itself", fast <= plain + 9, $"{fast - plain} more");
        Take(FishPoisonDuo.FastWater);
    }

    private IEnumerator LaidAndForced()
    {
        Say("--- A reaction laid and forced in one frame");
        build.Attune(Elements.Fire, Elements.Water);
        yield return Fresh();
        int pair = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
        int shown = reactions.TimesShown(pair);
        Lay(AfflictionKind.Burn, 2);
        Lay(AfflictionKind.Soaked, 1);
        bool forced = host.Afflictions.ForceReaction(pair, host);
        yield return Real(0.4f);
        Judge("Steam laid and then forced on the same frame is shown once", forced && reactions.TimesShown(pair) == shown + 1, $"shown {reactions.TimesShown(pair) - shown}");
        yield return null;
        shown = reactions.TimesShown(pair);
        // Steam spent the two; laid again while the pair rests, nothing reacts until it is forced.
        Lay(AfflictionKind.Burn, 2);
        Lay(AfflictionKind.Soaked, 1);
        Judge("a later force still goes through its rest", host.Afflictions.ForceReaction(pair, host));
        yield return Real(0.4f);
        Judge("and is shown", reactions.TimesShown(pair) == shown + 1, $"shown {reactions.TimesShown(pair) - shown}");
    }

    private void Finish()
    {
        if (host != null) Destroy(host.gameObject);
        foreach (var id in new[] { VenomRimeDuo.Shape, VenomRimeDuo.SixLevelsDown, FishPoisonDuo.FastWater }) RunState.Unlocked.Remove(id);
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
