using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the gods' gifts and the crowns of the mixed branches. In the combat arena it gathers three
// dummies, and for each move: takes the element, is given the mark as a shrine would give it, does what the mark's
// words ask (lights the surge, presses it again, guards, lands a finisher...), photographs the telling moments
// (review/shots/current/sig_<name>_<n>.png) and writes what each dummy lost and what was laid on it. In any other
// scene it does three of them at the nearest real enemy, to be sure nothing throws.
// A file %TEMP%/sig_probe.txt may hold a comma list of names to play only those; "soak=<seconds>" keeps lighting
// Fire's surge and dropping the sun for that long, for the profiler.
//   bash AgentScripts/runprobe.sh CombatTest SignaturesProbe 200
public static class SignaturesProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Signatures Probe").AddComponent<SignaturesProbeRunner>();
        return "probe started";
    }
}

public class SignaturesProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private readonly List<TrainingDummy> group = new List<TrainingDummy>();
    private Enemy real;
    private int errors;
    private string firstError = "";
    private bool finished;
    private string current = "";
    private int shot;
    private float[] before;
    private readonly Vector3[] spots = { new Vector3(5f, 0f, -2f), new Vector3(6.5f, 0f, -1.1f), new Vector3(5.7f, 0f, -3.4f) };
    private readonly Vector3 stand = new Vector3(2.5f, 0f, -1.9f);
    private Vector3 toward;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Shot()
    {
        shot++;
        ScreenCapture.CaptureScreenshot($"review/shots/current/sig_{current}_{shot}.png");
    }

    private IEnumerator ShotAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Shot();
        yield return null;
    }

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
    }

    private void OnLog(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception) return;
        errors++;
        if (firstError.Length == 0) firstError = message;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
        if (finished) return;
        Say($"the probe was taken away before it finished; errors logged {errors}{(errors > 0 ? ": " + firstError : "")}");
        Say("done");
    }

    private static string Laid(IAfflictable who)
    {
        string found = "";
        foreach (AfflictionKind kind in Enum.GetValues(typeof(AfflictionKind)))
        {
            if (kind == AfflictionKind.None) continue;
            if (who.Afflictions.Has(kind)) found += (found.Length > 0 ? "+" : "") + kind + "x" + who.Afflictions.Stacks(kind);
        }
        if (who.Afflictions.IsFrozen) found += "+FROZEN";
        if (who.Afflictions.IsHeld) found += "+HELD";
        return found.Length > 0 ? found : "none";
    }

    private string Lost()
    {
        if (real != null) return $"lost [{before[0] - real.Health.Current:0}] laid {Laid(real)}";
        string lost = "";
        for (int i = 0; i < group.Count; i++) lost += (i > 0 ? ", " : "") + (before[i] - group[i].Health.Current).ToString("0");
        return $"lost [{lost}] laid {Laid(group[0])} / {Laid(group[1])} / {Laid(group[2])}";
    }

    private void Mark() => before = real != null ? new[] { real.Health.Current } : group.Select(dummy => dummy.Health.Current).ToArray();

    private AfflictionSet[] Sets() => real != null ? new[] { real.Afflictions } : group.Select(dummy => dummy.Afflictions).ToArray();

    private Vector3 FirstAt => real != null ? real.transform.position : spots[0];

    // Clean ground, the element held, the mark given as a shrine gives it and nothing else learned.
    private IEnumerator Begin(string name, string markName, string hold = null)
    {
        current = name;
        shot = 0;
        float patience = Time.unscaledTime + 8f;
        while ((player.FormHold != 0 || player.IsLeaping || SpellWorks.Running > 0) && Time.unscaledTime < patience) yield return null;
        var mark = Upgrade.All.First(upgrade => upgrade.Name == markName);
        string held = hold ?? mark.Element;
        element.Take(ElementKit.ByName(held, false));
        upgrades.Forget();
        upgrades.Give(mark);
        defence.DamageTaken = 0f;
        defence.Restore();
        if (real == null)
        {
            player.Teleport(stand, Quaternion.LookRotation(toward));
            foreach (var dummy in group)
            {
                dummy.Afflictions.Clear();
                dummy.Stagger.Reset();
                dummy.Health.Fill();
            }
        }
        else
        {
            Vector3 from = real.transform.position + (player.transform.position - real.transform.position).normalized * 3f;
            player.Teleport(from, Quaternion.LookRotation(real.transform.position - from));
            real.Health.Maximum = 100000f;
            real.Health.Fill();
            real.Afflictions.Clear();
        }
        combat.Mana.Gain(1000f);
        yield return new WaitForSeconds(0.4f);
        Mark();
        Say($"{name}: holding {held}, given {mark.Name} ({mark.Id}), in force {upgrades.Has(mark.Id)}, worn {(upgrades.Worn != null ? upgrades.Worn.Name : "none")}");
    }

    private IEnumerator Surge(float seconds)
    {
        element.Kit.SurgeSeconds = seconds;
        combat.Mana.Gain(1000f);
        yield return Tap(Key.F);
        float give = Time.unscaledTime + 1f;
        while (!element.Kit.SurgeActive && Time.unscaledTime < give) yield return null;
        yield return null;
    }

    private IEnumerator SurgeEnds()
    {
        float give = Time.unscaledTime + 20f;
        while (element.Kit.SurgeActive && Time.unscaledTime < give) yield return null;
    }

    private IEnumerator Blows(int count, Key key)
    {
        for (int blow = 0; blow < count; blow++)
        {
            int landed = combat.BlowsLanded;
            yield return Tap(key);
            float give = Time.unscaledTime + 1.5f;
            while (combat.BlowsLanded == landed && Time.unscaledTime < give) yield return null;
            yield return new WaitForSeconds(0.12f);
        }
    }

    private IEnumerator Finisher()
    {
        if (real != null) real.Stagger.Add(100000f);
        else group[0].Stagger.Add(100000f);
        yield return null;
        int finishers = combat.Finishers;
        yield return Tap(Key.K);
        float give = Time.unscaledTime + 2.5f;
        while (combat.Finishers == finishers && Time.unscaledTime < give) yield return null;
    }

    // A blow from the first dummy while the guard is up. Fresh is a guard raised this moment (a perfect guard).
    private IEnumerator Guarded(bool fresh)
    {
        player.Stamina.Fill();
        Keys(Key.L);
        yield return null;
        yield return null;
        if (!fresh) yield return new WaitForSeconds(0.5f);
        var outcome = defence.TakeHit(new Hit(20f, 0f, -toward, false, false, FirstAt));
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.15f);
        lastGuard = outcome;
    }

    private HitOutcome lastGuard;

    private void LayAll(AfflictionKind kind, int stacks)
    {
        foreach (var set in Sets()) set.Apply(kind, stacks);
    }

    private IEnumerator Start()
    {
        Application.logMessageReceived += OnLog;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        toward = (spots[0] - stand).normalized;
        string only = "";
        float soak = 0f;
        string config = Path.Combine(Path.GetTempPath(), "sig_probe.txt");
        if (File.Exists(config))
        {
            only = File.ReadAllText(config).Trim();
            if (only.StartsWith("soak="))
            {
                float.TryParse(only.Substring(5), out soak);
                only = "";
            }
        }
        var moves = new List<(string name, Func<IEnumerator> play)>
        {
            ("fire", Fire), ("wind", Wind), ("ice", Ice), ("water", Water), ("life", Life), ("earth", Earth), ("metal", Metal), ("light", Light),
            ("dark", Dark), ("poison", Poison), ("tempest", Tempest), ("blizzard", Blizzard), ("glacier", Glacier), ("bloom", Bloom),
            ("overgrowth", Overgrowth), ("forge", Forge), ("halo", Halo), ("eclipse", Eclipse), ("blight", Blight), ("pyre", Pyre),
            ("flashes", Flashes), ("ball", Ball), ("heart", Heart), ("wave", Wave),
        };
        Say($"start: {SpellForms.All.Count} forms registered; works {(FindAnyObjectByType<SpellWorks>() != null)} fx {(FindAnyObjectByType<FormFx>() != null)}; scene {gameObject.scene.name}");
        yield return new WaitForSeconds(0.3f);
        var dummies = FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None).Where(dummy => dummy.GetComponent<DummyAttacker>() == null).ToList();
        if (dummies.Count >= 3)
        {
            for (int i = 0; i < 3; i++)
            {
                dummies[i].transform.position = spots[i];
                dummies[i].Health.Maximum = 100000f;
                dummies[i].Health.Fill();
                dummies[i].RestoreAfter = 99999f;
                group.Add(dummies[i]);
            }
            Physics.SyncTransforms();
        }
        else
        {
            real = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Where(enemy => !enemy.IsDead)
                .OrderBy(enemy => (enemy.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
            Say($"elsewhere: enemy {(real != null ? real.name : "none")}");
            if (real == null) moves.Clear();
            else
            {
                toward = (real.transform.position - player.transform.position).normalized;
                moves = moves.Where(move => move.name == "fire" || move.name == "ice" || move.name == "tempest" || move.name == "pyre" || move.name == "halo").ToList();
                StartCoroutine(ClickThrough());
            }
        }
        if (soak > 0f)
        {
            yield return Begin("soak", "Falling Sun");
            float until = Time.time + soak;
            int suns = 0;
            while (Time.time < until)
            {
                yield return Surge(1.2f);
                yield return SurgeEnds();
                yield return new WaitForSeconds(0.2f);
                suns++;
            }
            Say($"soak: {suns} suns, runs {SpellWorks.Running}, solids {FormFx.SolidsShown}, marks {FormFx.MarksShown}");
        }
        else
        {
            foreach (var move in moves)
            {
                if (only.Length > 0 && !only.Split(',').Contains(move.name)) continue;
                int errorsBefore = errors;
                yield return move.play();
                yield return new WaitForSeconds(0.5f);
                Say($"{move.name}: after, runs {SpellWorks.Running}, solids {FormFx.SolidsShown}, marks {FormFx.MarksShown}, hold {player.FormHold}, hidden {player.Hidden}, time scale {Time.timeScale:0.00}, errors +{errors - errorsBefore}");
            }
        }
        Say($"errors logged {errors}{(errors > 0 ? ": " + firstError : "")}");
        finished = true;
        Say("done");
    }

    private IEnumerator ClickThrough()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (!defence.IsDead) defence.Health.Fill();
            if (Time.timeScale > 0f && !player.Suspended) continue;
            Keys(Key.E);
            yield return new WaitForSecondsRealtime(0.1f);
            Keys();
        }
    }

    // ---- The gods' gifts.

    private IEnumerator Fire()
    {
        yield return Begin("fire", "Falling Sun");
        // From eight metres off, so the dawn has a road to roll along.
        player.Teleport(stand - toward * 5f, Quaternion.LookRotation(toward));
        LayAll(AfflictionKind.Burn, 3);
        Mark();
        yield return Surge(9f);
        yield return ShotAfter(0.75f);
        Say($"fire: dawn, the sun rolled {Vector3.Distance(player.transform.position, FirstAt):0.0} m off the first; runs of sun {SpellWorks.RunningOf("sun")}, solids {FormFx.SolidsShown}");
        yield return ShotAfter(0.85f);
        Say($"fire: dawn's end: {Lost()}");
        // Up close for the noon and the setting, so the camera has them.
        player.Teleport(stand, Quaternion.LookRotation(toward));
        yield return ShotAfter(0.45f);
        yield return ShotAfter(1.6f);
        Say($"fire: noon, sun hanging, runs of sun {SpellWorks.RunningOf("sun")}, burn on first x{Sets()[0].Stacks(AfflictionKind.Burn)}");
        Mark();
        yield return Tap(Key.F);
        yield return ShotAfter(0.2f);
        yield return ShotAfter(0.14f);
        yield return ShotAfter(0.5f);
        Say($"fire: set on a second press (surge again heard {upgrades.SurgeAgain}, surge still lit {element.Kit.SurgeActive}): {Lost()}");
        // Called down while still being rolled: half weight.
        yield return SurgeEnds();
        player.Teleport(stand - toward * 5f, Quaternion.LookRotation(toward));
        yield return Surge(9f);
        yield return new WaitForSeconds(0.9f);
        Mark();
        yield return Tap(Key.F);
        yield return ShotAfter(0.12f);
        yield return new WaitForSeconds(0.3f);
        Say($"fire: called at dawn, runs of sun {SpellWorks.RunningOf("sun")}: {Lost()}");
        // A third sun, sent under by a blow, rises again; struck with too little surge left, it does not.
        yield return SurgeEnds();
        player.Teleport(stand, Quaternion.LookRotation(toward));
        defence.DamageTaken = 0.01f;
        yield return Surge(8f);
        yield return new WaitForSeconds(2.2f);
        defence.TakeHit(new Hit(1f, 0f, -toward, false, true, FirstAt));
        yield return ShotAfter(0.25f);
        Say($"fire: struck at noon with {element.Kit.SurgeLeft:0.0} s of surge left, runs of sun {SpellWorks.RunningOf("sun")} (1 = under, to rise), callout '{defence.LastCallout}'");
        yield return ShotAfter(1.9f);
        Say($"fire: risen again: runs of sun {SpellWorks.RunningOf("sun")}");
        yield return new WaitForSeconds(1.6f);
        defence.TakeHit(new Hit(1f, 0f, -toward, false, true, FirstAt));
        yield return new WaitForSeconds(0.6f);
        Say($"fire: struck with {element.Kit.SurgeLeft:0.0} s left, runs of sun {SpellWorks.RunningOf("sun")} (0 = gone under for good)");
        defence.DamageTaken = 0f;
    }

    private IEnumerator Wind()
    {
        yield return Begin("wind", "The Wind Remembers");
        yield return Surge(4f);
        yield return Blows(5, Key.J);
        Shot();
        yield return null;
        Say($"wind: blows landed in Tailwind, runs {SpellWorks.RunningOf("remember")}, so far {Lost()}");
        Mark();
        yield return SurgeEnds();
        yield return ShotAfter(0.2f);
        yield return ShotAfter(0.2f);
        yield return ShotAfter(0.6f);
        Say($"wind: the cuts fell again: {Lost()}");
    }

    private IEnumerator Ice()
    {
        yield return Begin("ice", "The World Holds Still");
        yield return Surge(12f);
        yield return ShotAfter(0.6f);
        yield return Tap(Key.F);
        yield return ShotAfter(0.25f);
        yield return ShotAfter(0.5f);
        Say($"ice: broken outward: {Lost()}; frozen {Sets().Count(set => set.IsFrozen)} of {Sets().Length}");
        Mark();
        yield return Blows(1, Key.K);
        yield return ShotAfter(0.3f);
        yield return new WaitForSeconds(1f);
        Say($"ice: one shattered with a heavy blow: {Lost()}; still frozen {Sets().Count(set => set.IsFrozen)}");
    }

    private IEnumerator Water()
    {
        yield return Begin("water", "The Sea Rises");
        yield return Surge(12f);
        int flasks = defence.Flasks.Count;
        yield return Tap(Key.F);
        yield return ShotAfter(0.5f);
        yield return Blows(3, Key.J);
        Shot();
        yield return null;
        Say($"water: flooded for {flasks - defence.Flasks.Count} flask, three blows as waves: {Lost()}");
        float give = Time.time + 8f;
        while (SpellWorks.RunningOf("sea") > 0 && Time.time < give)
        {
            // The moment it breaks.
            if (Time.timeScale < 0.9f && shot < 3) Shot();
            yield return null;
        }
        yield return ShotAfter(0.15f);
        Say($"water: the sea drew back and broke: {Lost()}");
    }

    private IEnumerator Life()
    {
        yield return Begin("life", "First Spring");
        LayAll(AfflictionKind.Seeded, 3);
        yield return Surge(10f);
        float health = defence.Health.Current;
        yield return Tap(Key.F);
        yield return ShotAfter(0.35f);
        float after = defence.Health.Current;
        yield return ShotAfter(0.5f);
        Say($"life: health {health:0} -> {after:0} (grey {defence.Health.Grey:0}); {Lost()}");
        yield return Blows(4, Key.J);
        Shot();
        yield return null;
        Say($"life: after four blows on the held, health {defence.Health.Current:0}; {Lost()}");
        yield return new WaitForSeconds(3f);
    }

    private IEnumerator Earth()
    {
        yield return Begin("earth", "Hand of the Mountain");
        yield return Surge(8f);
        yield return ShotAfter(0.8f);
        yield return Blows(1, Key.K);
        float blow = combat.LastBlowDamage;
        yield return ShotAfter(0.25f);
        yield return ShotAfter(0.2f);
        yield return ShotAfter(0.4f);
        Say($"earth: a heavy blow of {blow:0} and the hand after it: {Lost()}");
        yield return SurgeEnds();
    }

    private IEnumerator Metal()
    {
        yield return Begin("metal", "Hanging Swords");
        yield return Surge(6f);
        string outcomes = "";
        for (int i = 0; i < 4; i++)
        {
            yield return Guarded(false);
            outcomes += lastGuard + " ";
        }
        Shot();
        yield return null;
        Say($"metal: four blows held on a guard half a second old came back as [{outcomes}], perfect guards {defence.PerfectGuards}, mana {combat.Mana.Current:0}");
        yield return SurgeEnds();
        yield return ShotAfter(0.25f);
        yield return ShotAfter(0.25f);
        yield return ShotAfter(0.7f);
        Say($"metal: the swords fell: {Lost()}");
    }

    private IEnumerator Light()
    {
        yield return Begin("light", "The Scales");
        LayAll(AfflictionKind.Marked, 1);
        yield return Surge(4.5f);
        yield return Blows(4, Key.J);
        LayAll(AfflictionKind.Marked, 1);
        Shot();
        yield return null;
        Say($"light: four blows weighed: {Lost()}");
        Mark();
        yield return SurgeEnds();
        yield return ShotAfter(0.3f);
        yield return ShotAfter(0.25f);
        yield return ShotAfter(0.6f);
        Say($"light: judged '{defence.LastCallout}': {Lost()}");
        // Found wanting: wounded and dealing nothing.
        defence.DamageTaken = 0.2f;
        yield return Surge(1.5f);
        defence.TakeHit(new Hit(40f, 0f, -toward, false, true, FirstAt));
        yield return SurgeEnds();
        yield return new WaitForSeconds(0.3f);
        Say($"light: wounded and dealing nothing: '{defence.LastCallout}', mana {combat.Mana.Current:0}");
        defence.DamageTaken = 0f;
    }

    private IEnumerator Dark()
    {
        yield return Begin("dark", "A Life for a Life");
        // The Rise is the root's since 2026-11-20 (rise-on-root); the gift buys it back.
        upgrades.Give(Upgrade.ById(Clover.Player.Trees.DarkTree.Curse));
        defence.DamageTaken = 1f;
        var outcome = defence.TakeHit(new Hit(100000f, 0f, -toward, false, true, FirstAt));
        yield return ShotAfter(0.3f);
        float risenWith = defence.Health.Current;
        Say($"dark: a killing blow came back as {outcome}; dead {defence.IsDead}, risen {upgrades.Risen}, health {risenWith:0} of {defence.Health.Maximum:0}, callout '{defence.LastCallout}'");
        defence.DamageTaken = 0f;
        yield return new WaitForSeconds(2.5f);
        Shot();
        yield return null;
        Say($"dark: two and a half seconds on, health {defence.Health.Current:0} (drained {risenWith - defence.Health.Current:0})");
        yield return Finisher();
        yield return ShotAfter(0.3f);
        yield return new WaitForSeconds(0.2f);
        Say($"dark: a finisher on the killer: risen {upgrades.Risen}, health {defence.Health.Current:0}, callout '{defence.LastCallout}'");
        // Past the finisher, through which nothing lands.
        yield return new WaitForSeconds(1.2f);
        defence.DamageTaken = 1f;
        defence.TakeHit(new Hit(100000f, 0f, -toward, false, true, FirstAt));
        yield return new WaitForSeconds(0.2f);
        Say($"dark: killed again: dead {defence.IsDead}, risen {upgrades.Risen} (may rise once more)");
        // Past the flinch of the last blow, which lets nothing land.
        yield return new WaitForSeconds(1.5f);
        defence.TakeHit(new Hit(100000f, 0f, -toward, false, true, FirstAt));
        yield return new WaitForSeconds(0.8f);
        Say($"dark: and killed while Risen: dead {defence.IsDead}, risen {upgrades.Risen}");
        float give = Time.unscaledTime + 8f;
        while (defence.IsDead && Time.unscaledTime < give) yield return null;
        yield return new WaitForSeconds(0.5f);
        defence.DamageTaken = 0f;
    }

    private IEnumerator Poison()
    {
        yield return Begin("poison", "Clean Blade");
        LayAll(AfflictionKind.Poison, 5);
        yield return Surge(5f);
        yield return Blows(2, Key.J);
        yield return new WaitForSeconds(0.5f);
        Shot();
        yield return null;
        Say($"poison: in Fever, poison waits {Afflictions.PoisonWaits}; {Lost()}; owed seconds on first {Sets()[0].OwedSeconds:0.0}");
        Mark();
        yield return SurgeEnds();
        yield return ShotAfter(0.12f);
        yield return ShotAfter(0.2f);
        yield return ShotAfter(0.4f);
        Say($"poison: the flick paid {CleanBladeForm.LastOwed:0} in all: {Lost()}; poison waits {Afflictions.PoisonWaits}");
    }

    // ---- The crowns.

    private IEnumerator Tempest()
    {
        yield return Begin("tempest", "Falling Thunder");
        yield return Finisher();
        yield return ShotAfter(0.12f);
        yield return ShotAfter(0.28f);
        yield return ShotAfter(0.4f);
        Say($"tempest: {Lost()}; player at {player.transform.position.x:0.0},{player.transform.position.z:0.0}");
    }

    private IEnumerator Blizzard()
    {
        yield return Begin("blizzard", "Diamond Dust");
        LayAll(AfflictionKind.Chill, Afflictions.Depth(AfflictionKind.Chill));
        yield return new WaitForSeconds(0.2f);
        yield return Blows(1, Key.K);
        yield return ShotAfter(0.5f);
        Say($"blizzard: frozen {Sets().Count(set => set.IsFrozen)} after one heavy blow; dust runs {SpellWorks.RunningOf("dust")}, solids {FormFx.SolidsShown}; {Lost()}");
        Mark();
        player.Teleport(stand, Quaternion.LookRotation(toward));
        yield return null;
        Keys(Key.D, Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return ShotAfter(0.1f);
        yield return ShotAfter(0.3f);
        Say($"blizzard: thrown down the dodge ({upgrades.LastDodgeDirection.x:0.0},{upgrades.LastDodgeDirection.z:0.0}): {Lost()}");
    }

    private IEnumerator Glacier()
    {
        yield return Begin("glacier", "Weight of Ice");
        string outcomes = "";
        for (int i = 0; i < 3; i++)
        {
            yield return Guarded(i == 0);
            outcomes += lastGuard + " ";
        }
        Shot();
        yield return null;
        Say($"glacier: guards [{outcomes}], plates runs {SpellWorks.RunningOf("plates")}, solids {FormFx.SolidsShown}, damage taken x{upgrades.Stats.Multiplier(PlayerStat.DamageTaken):0.00}");
        Mark();
        Keys(Key.K);
        yield return null;
        yield return null;
        Keys();
        float give = Time.unscaledTime + 2f;
        while (SpellWorks.RunningOf("plates") > 0 && Time.unscaledTime < give) yield return null;
        yield return ShotAfter(0.1f);
        yield return ShotAfter(0.6f);
        Say($"glacier: spent on a heavy swing: {Lost()}");
    }

    private IEnumerator Bloom()
    {
        yield return Begin("bloom", "Great Lotus");
        defence.DamageTaken = 0.3f;
        defence.TakeHit(new Hit(60f, 0f, -toward, false, true, FirstAt));
        defence.DamageTaken = 0f;
        yield return new WaitForSeconds(0.8f);
        yield return Surge(14f);
        float health = defence.Health.Current;
        yield return Tap(Key.R);
        float give = Time.unscaledTime + 4f;
        while (SpellWorks.RunningOf("lotus") == 0 && Time.unscaledTime < give) yield return null;
        yield return ShotAfter(0.8f);
        string outcomes = "";
        for (int i = 0; i < 2; i++)
        {
            yield return Guarded(true);
            outcomes += lastGuard + " ";
        }
        yield return ShotAfter(0.3f);
        Say($"bloom: lotus runs {SpellWorks.RunningOf("lotus")}, guards inside [{outcomes}], health {health:0} -> {defence.Health.Current:0}; {Lost()}");
        give = Time.time + 11f;
        while (SpellWorks.RunningOf("lotus") > 0 && Time.time < give) yield return null;
        yield return ShotAfter(0.08f);
        yield return ShotAfter(0.4f);
        Say($"bloom: burst: {Lost()}");
    }

    private IEnumerator Overgrowth()
    {
        yield return Begin("overgrowth", "The Ground Wakes");
        defence.DamageTaken = 0.3f;
        defence.TakeHit(new Hit(100f, 0f, -toward, false, true, FirstAt));
        defence.DamageTaken = 0f;
        yield return new WaitForSeconds(0.8f);
        float health = defence.Health.Current;
        yield return Surge(6f);
        yield return ShotAfter(0.5f);
        Say($"overgrowth: risen: {Lost()}");
        yield return ShotAfter(1.6f);
        yield return ShotAfter(0.4f);
        Say($"overgrowth: flowered: health {health:0} -> {defence.Health.Current:0}; {Lost()}");
        yield return SurgeEnds();
    }

    private IEnumerator Forge()
    {
        yield return Begin("forge", "World Anvil");
        yield return Finisher();
        yield return ShotAfter(0.2f);
        yield return ShotAfter(0.8f);
        Say($"forge: the anvil rang: {Lost()}; anvil runs {SpellWorks.RunningOf("anvil")}, solids {FormFx.SolidsShown}");
        Mark();
        yield return new WaitForSeconds(1.2f);
        yield return Blows(1, Key.K);
        yield return ShotAfter(0.1f);
        yield return new WaitForSeconds(0.4f);
        Say($"forge: blades flown with a heavy blow: {Lost()}");
    }

    private IEnumerator Halo()
    {
        yield return Begin("halo", "Six Mirrors");
        bool cast = upgrades.TrySpecial();
        yield return ShotAfter(0.3f);
        yield return ShotAfter(0.5f);
        float give = Time.time + 2f;
        while (Time.timeScale > 0.9f && SpellWorks.RunningOf("mirrors") > 0 && Time.time < give) yield return null;
        Shot();
        yield return new WaitForSeconds(0.9f);
        Say($"halo: cast {cast}: {Lost()}; player at {player.transform.position.x:0.0},{player.transform.position.z:0.0} (began at {stand.x:0.0},{stand.z:0.0})");
    }

    private IEnumerator Eclipse()
    {
        yield return Begin("eclipse", "Totality");
        yield return Surge(5f);
        yield return ShotAfter(0.8f);
        int crits = combat.Criticals;
        yield return Blows(4, Key.J);
        Shot();
        yield return null;
        Say($"eclipse: in the dark, four blows, criticals +{combat.Criticals - crits}: {Lost()}");
        Mark();
        yield return SurgeEnds();
        yield return ShotAfter(0.3f);
        yield return ShotAfter(0.5f);
        Say($"eclipse: the light returned: {Lost()}");
    }

    private IEnumerator Blight()
    {
        yield return Begin("blight", "The Reaping");
        LayAll(AfflictionKind.Poison, 6);
        LayAll(AfflictionKind.Cursed, 2);
        defence.DamageTaken = 0.3f;
        defence.TakeHit(new Hit(100f, 0f, -toward, false, true, FirstAt));
        defence.DamageTaken = 0f;
        yield return new WaitForSeconds(0.8f);
        LayAll(AfflictionKind.Poison, 6);
        LayAll(AfflictionKind.Cursed, 2);
        float health = defence.Health.Current;
        Mark();
        yield return Finisher();
        yield return ShotAfter(0.08f);
        yield return ShotAfter(0.1f);
        yield return ShotAfter(0.5f);
        Say($"blight: reaped {ReapingForm.LastReaped:0} in all, health {health:0} -> {defence.Health.Current:0}: {Lost()}");
    }

    // ---- Bespoke spells that are not gifts.

    private IEnumerator Flashes()
    {
        yield return Begin("flashes", "Seven Flashes");
        LayAll(AfflictionKind.Static, 3);
        bool cast = upgrades.TrySpecial();
        yield return ShotAfter(0.5f);
        yield return ShotAfter(0.55f);
        float give = Time.time + 2f;
        while (Time.timeScale > 0.9f && SpellWorks.RunningOf("flashes") > 0 && Time.time < give) yield return null;
        Shot();
        yield return new WaitForSeconds(0.9f);
        Say($"flashes: cast {cast}: {Lost()}; discharged {FlashesForm.Discharged}; player at {player.transform.position.x:0.0},{player.transform.position.z:0.0} (began at {stand.x:0.0},{stand.z:0.0})");
        // Struck while still: half, and nothing discharges.
        foreach (var set in Sets()) set.Clear();
        LayAll(AfflictionKind.Static, 3);
        Mark();
        combat.Mana.Gain(1000f);
        defence.DamageTaken = 0.01f;
        upgrades.TrySpecial();
        yield return new WaitForSeconds(1.0f);
        var outcome = defence.TakeHit(new Hit(1f, 0f, -toward, false, true, FirstAt));
        yield return new WaitForSeconds(1.2f);
        defence.DamageTaken = 0f;
        Say($"flashes: struck while still ({outcome}): {Lost()}; discharged {FlashesForm.Discharged}");
    }

    private IEnumerator Ball()
    {
        yield return Begin("ball", "Ball Lightning");
        // From well back, so there is room to bat it.
        Vector3 back = stand - toward * 3f;
        player.Teleport(back, Quaternion.LookRotation(toward));
        yield return new WaitForSeconds(0.2f);
        bool cast = upgrades.TrySpecial();
        yield return ShotAfter(0.45f);
        yield return Tap(Key.J);
        yield return ShotAfter(0.3f);
        float give = Time.time + 3f;
        while (SpellWorks.RunningOf("ball") > 0 && Time.time < give) yield return null;
        yield return ShotAfter(0.05f);
        yield return new WaitForSeconds(0.2f);
        Say($"ball: cast {cast}, batted {BallForm.LastStrikes} time(s), burst on {BallForm.LastStruck}: {Lost()}");
        // Left alone: it drifts to them and bursts small.
        foreach (var set in Sets()) set.Clear();
        Mark();
        player.Teleport(stand - toward * 1.5f, Quaternion.LookRotation(toward));
        combat.Mana.Gain(1000f);
        upgrades.TrySpecial();
        give = Time.time + 9f;
        yield return new WaitForSeconds(0.3f);
        while (SpellWorks.RunningOf("ball") > 0 && Time.time < give) yield return null;
        yield return new WaitForSeconds(0.4f);
        Say($"ball: left alone, batted {BallForm.LastStrikes}, burst on {BallForm.LastStruck}: {Lost()}");
    }

    private IEnumerator Heart()
    {
        yield return Begin("heart", "Pawned Heart");
        // The cast turns the player to the enemy faced and sets the heart down behind them.
        float had = defence.Health.Current;
        bool cast = upgrades.TrySpecial();
        Vector3 heart = player.transform.position - player.transform.forward * 1.8f;
        yield return ShotAfter(0.6f);
        Say($"heart: cast {cast}, health {had:0} -> {defence.Health.Current:0}, weapon damage x{upgrades.Stats.Multiplier(PlayerStat.WeaponDamage):0.00}");
        player.Teleport(stand + toward * 1f, Quaternion.LookRotation(toward));
        yield return new WaitForSeconds(0.2f);
        yield return Blows(3, Key.J);
        Shot();
        yield return null;
        Say($"heart: three blows while it beats: {Lost()}");
        player.Teleport(heart, Quaternion.LookRotation(toward));
        yield return ShotAfter(0.15f);
        yield return new WaitForSeconds(0.5f);
        Say($"heart: walked back onto it: '{defence.LastCallout}', returned {HeartForm.LastReturned:0}, health {defence.Health.Current:0}, runs {SpellWorks.RunningOf("heart")}");
        // And left to stop.
        defence.Restore();
        player.Teleport(stand, Quaternion.LookRotation(toward));
        yield return new WaitForSeconds(0.2f);
        bool again = upgrades.TrySpecial();
        yield return new WaitForSeconds(0.5f);
        Say($"heart: cast again {again}, health {defence.Health.Current:0}, runs {SpellWorks.RunningOf("heart")}, hold {player.FormHold}");
        player.Teleport(stand + toward * 1f, Quaternion.LookRotation(toward));
        float give = Time.time + 12f;
        while (SpellWorks.RunningOf("heart") > 0 && Time.time < give) yield return null;
        yield return new WaitForSeconds(0.3f);
        Say($"heart: left until the ring closed: '{defence.LastCallout}', health {defence.Health.Current:0}, grey {defence.Health.Grey:0}");
        defence.Restore();
    }

    private IEnumerator Wave()
    {
        yield return Begin("wave", "The Wave Stands Still");
        Keys(Key.Q);
        float give = Time.unscaledTime + 2f;
        while (SpellWorks.RunningOf("frozenwave") == 0 && Time.unscaledTime < give) yield return null;
        yield return ShotAfter(0.3f);
        yield return new WaitForSeconds(0.15f);
        Keys();
        yield return ShotAfter(0.2f);
        yield return ShotAfter(0.5f);
        Say($"wave: let go: {Lost()}; frozen {Sets().Count(set => set.IsFrozen)} of {Sets().Length}; first dummy now at {group[0].transform.position.x:0.0},{group[0].transform.position.z:0.0}; runs {SpellWorks.RunningOf("frozenwave")}");
        Mark();
        player.Teleport(group[0].transform.position - toward * 2.2f, Quaternion.LookRotation(toward));
        yield return new WaitForSeconds(0.2f);
        Keys(Key.K);
        yield return null;
        yield return null;
        Keys();
        give = Time.unscaledTime + 2f;
        while (SpellWorks.RunningOf("frozenwave") > 0 && Time.unscaledTime < give) yield return null;
        yield return ShotAfter(0.05f);
        yield return ShotAfter(0.4f);
        Say($"wave: shattered with a heavy swing: struck {FrozenWaveForm.Shattered}; {Lost()}; still frozen {Sets().Count(set => set.IsFrozen)}");
    }

    private IEnumerator Pyre()
    {
        yield return Begin("pyre", "Light the Pyres");
        LayAll(AfflictionKind.Burn, 3);
        LayAll(AfflictionKind.Poison, 4);
        Mark();
        bool cast = upgrades.TrySpecial();
        yield return ShotAfter(1.2f);
        yield return new WaitForSeconds(0.9f);
        float give = Time.time + 2f;
        while (Time.timeScale > 0.9f && SpellWorks.RunningOf("pyres") > 0 && Time.time < give) yield return null;
        Shot();
        yield return ShotAfter(0.12f);
        yield return new WaitForSeconds(0.5f);
        Say($"pyre: cast {cast}: {Lost()}");
        // Broken by the dodge: nothing rises.
        LayAll(AfflictionKind.Burn, 3);
        Mark();
        combat.Mana.Gain(1000f);
        upgrades.TrySpecial();
        yield return new WaitForSeconds(0.8f);
        yield return Tap(Key.Space);
        yield return new WaitForSeconds(2.2f);
        Say($"pyre: broken by a dodge: {Lost()} (burn ticks only), runs {SpellWorks.RunningOf("pyres")}");
    }
}
