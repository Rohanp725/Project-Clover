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

// Play Mode probe for the marks finished on the evening of 2026-11-16 (Sky Dance, poison's beat, Dead Already) and Great Lotus's five petals.
// It was cut from SignaturesProbe and keeps its manner: In the combat arena it gathers three
// dummies, and for each move: takes the element, is given the mark as a shrine would give it, does what the mark's
// words ask (lights the surge, presses it again, guards, lands a finisher...), photographs the telling moments
// (review/shots/current/sig_<name>_<n>.png) and writes what each dummy lost and what was laid on it. In any other
// scene it does three of them at the nearest real enemy, to be sure nothing throws.
// A file %TEMP%/marks_probe.txt may hold a comma list of names to play only those; "soak=<seconds>" keeps lighting
// Fire's surge and dropping the sun for that long, for the profiler.
//   bash AgentScripts/runprobe.sh CombatTest MarksProbe 200
public static class MarksProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Marks Probe").AddComponent<MarksProbeRunner>();
        return "probe started";
    }
}

public class MarksProbeRunner : MonoBehaviour
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
        ScreenCapture.CaptureScreenshot($"review/shots/current/mark_{current}_{shot}.png");
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
        string config = Path.Combine(Path.GetTempPath(), "marks_probe.txt");
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
            ("skydance", SkyDance), ("beat", Beat), ("lance", Lance), ("volatile", Volatile), ("dead", Dead), ("lotus", Lotus), ("flash", Flash),
            ("circle", Circle), ("eye", Eye), ("furnace", Furnace), ("stroke", Stroke), ("allatonce", AllAtOnce), ("cleanbreak", CleanBreak), ("heartwood", Heartwood), ("unmoved", Unmoved), ("leaf", Leaf), ("tide", Tide), ("answer", Answer), ("well", WellMove),
            ("rush", Rush), ("sunspot", Sunspot), ("scorched", Scorched), ("thunder", Thunder), ("lodestone", Lodestone), ("kiln", Kiln), ("twin", Twin), ("meltdown", Meltdown), ("front", Front), ("reserve", Reserve),
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
                moves = moves.Where(move => move.name == "circle" || move.name == "eye" || move.name == "furnace" || move.name == "cleanbreak" || move.name == "rush" || move.name == "sunspot" || move.name == "lodestone").OrderBy(move => move.name == "cleanbreak").ToList();
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

    // ---- The keystones and spells of the night of 2026-11-16.

    private string Up()
    {
        string up = "";
        foreach (var set in Sets()) up += (up.Length > 0 ? "/" : "") + (set.IsLifted ? "up" : set.IsAirborne ? "reels" : "down");
        return up;
    }

    // ---- The marks built to their designs on 2026-11-17.

    private IEnumerator Near()
    {
        Vector3 at = FirstAt - toward * 1.5f;
        if (real != null) at = real.transform.position + (player.transform.position - real.transform.position).normalized * 1.5f;
        player.Teleport(at, Quaternion.LookRotation(FirstAt - at));
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator Rush()
    {
        yield return Begin("rush", "Wildfire Rush");
        int heat = upgrades.Heat.Level, patches = Clover.World.Hazards.Alive;
        Vector3 start = player.transform.position;
        bool cast = upgrades.TrySpecial();
        yield return ShotAfter(0.3f);
        yield return ShotAfter(0.45f);
        Say($"rush: cast {cast}: went {(player.transform.position - start).magnitude:0.0} m, Heat {heat} -> {upgrades.Heat.Level}, patches of ground {patches} -> {Clover.World.Hazards.Alive}; {Lost()}");
        Mark();
        yield return ShotAfter(1.2f);
        Say($"rush: the line burning 1.2 s on: {Lost()}");
    }

    private IEnumerator Sunspot()
    {
        yield return Begin("sunspot", "Sunspot");
        Sets()[0].Apply(AfflictionKind.Burn, 3);
        bool cast = upgrades.TrySpecial();
        yield return new WaitForSeconds(0.5f);
        yield return Near();
        Mark();
        yield return Blows(3, Key.J);
        yield return ShotAfter(0.1f);
        Say($"sunspot: cast {cast}, runs {SpellWorks.RunningOf("sunspot")}, after three light blows (the brand itself has done nothing yet): {Lost()}");
        Mark();
        yield return Blows(1, Key.K);
        yield return ShotAfter(0.03f);
        yield return ShotAfter(0.2f);
        Say($"sunspot: set off by the heavy blow with three fed (the burst alone is 12 + 9 x 3 = 39, on top of the blow): runs {SpellWorks.RunningOf("sunspot")}; {Lost()}");
        // Left alone: it bursts at half when its time is up.
        if (real == null)
        {
            yield return Begin("sunspot", "Sunspot");
            upgrades.Worn.Spell.Seconds = 1.2f;
            upgrades.TrySpecial();
            yield return new WaitForSeconds(1.7f);
            upgrades.Worn.Spell.Seconds = 7f;
            Say($"sunspot: left alone until its time ran out (half of 12 = 6): {Lost()}");
        }
    }

    private IEnumerator Scorched()
    {
        yield return Begin("scorched", "Scorched Earth");
        Vector3 fire = player.transform.position;
        Clover.World.Hazards.Lay(Clover.World.HazardKind.Fire, fire, 1.6f, 4f, 1f, 0f, 2f);
        yield return Surge(6f);
        yield return null;
        float lit = element.Kit.SurgeLeft;
        yield return ShotAfter(2f);
        Say($"scorched: stood in own fire 2 s: Kindle {lit:0.00} -> {element.Kit.SurgeLeft:0.00} s left, held {upgrades.SurgeHeld}, mana from blows x{upgrades.Stats.Multiplier(PlayerStat.ManaFromBlows):0.00}, spell cost x{upgrades.Stats.Multiplier(PlayerStat.SpellCost):0.00}, fire lasts x{Clover.World.Hazards.FireSecondsScale:0}");
        yield return new WaitForSeconds(3f);
        Say($"scorched: 5 s after a 4 s patch was laid, fire still under the feet {Clover.World.Hazards.FireUnder(player.transform.position)} (it burns twice as long)");
        player.Teleport(fire - toward * 6f, Quaternion.LookRotation(toward));
        float left = element.Kit.SurgeLeft;
        yield return new WaitForSeconds(1f);
        Say($"scorched: out of the fire 1 s: Kindle {left:0.00} -> {element.Kit.SurgeLeft:0.00}, held {upgrades.SurgeHeld}, mana from blows x{upgrades.Stats.Multiplier(PlayerStat.ManaFromBlows):0.00}");
        yield return SurgeEnds();
    }

    private IEnumerator Thunder()
    {
        yield return Begin("thunder", "Waiting Thunder");
        bool cast = upgrades.TrySpecial();
        yield return Near();
        yield return ShotAfter(1.6f);
        Say($"thunder: cast {cast}: hung 1.8 s with no blow thrown, runs {SpellWorks.RunningOf("waiting")}: {Lost()} (nothing flies by itself)");
        Mark();
        yield return Blows(2, Key.J);
        yield return ShotAfter(0.12f);
        yield return new WaitForSeconds(0.3f);
        Say($"thunder: two light blows, a bolt with each: {Lost()}");
        Mark();
        yield return Blows(1, Key.K);
        yield return ShotAfter(0.12f);
        yield return new WaitForSeconds(0.5f);
        Say($"thunder: a heavy blow sends the three left: runs {SpellWorks.RunningOf("waiting")}; {Lost()}");
    }

    private IEnumerator Kiln()
    {
        yield return Begin("kiln", "Kiln Stance");
        yield return Near();
        Mark();
        float mana = combat.Mana.Current;
        int guards = defence.PerfectGuards;
        bool cast = upgrades.TrySpecial();
        yield return new WaitForSeconds(0.25f);
        float after = combat.Mana.Current;
        var outcome = defence.TakeHit(new Hit(20f, 0f, -toward, false, false, FirstAt));
        yield return ShotAfter(0.1f);
        yield return ShotAfter(0.2f);
        Say($"kiln: cast {cast}: the blow was {outcome}, perfect guards {guards} -> {defence.PerfectGuards}, mana {mana:0} -> {after:0} (the cost) -> {combat.Mana.Current:0} (the guard's gift), health lost {defence.Health.Maximum - defence.Health.Current:0}; {Lost()}");
    }

    private IEnumerator Twin()
    {
        yield return Begin("twin", "Twin Break", "Fire");
        yield return Near();
        Sets()[0].Apply(AfflictionKind.Burn, 3);
        Sets()[0].Apply(AfflictionKind.Static, 2);
        Mark();
        yield return Guarded(true);
        yield return ShotAfter(0.02f);
        yield return ShotAfter(0.15f);
        Say($"twin: guard {lastGuard}, twin breaks {upgrades.TwinBreaks} ((10 x 3 + 7 x 2) x 1.5 = 66): {Lost()}");
    }

    private IEnumerator Meltdown()
    {
        yield return Begin("meltdown", "Meltdown", "Fire");
        Sets()[0].Apply(AfflictionKind.Burn, 5);
        Sets()[0].Apply(AfflictionKind.Static, 4);
        yield return new WaitForSeconds(0.6f);
        Say($"meltdown: at five Burn and four Static, melting begun {defence.LastCallout == "Meltdown"}, meltdowns {upgrades.Meltdowns}");
        Sets()[0].Apply(AfflictionKind.Burn, 5);
        Sets()[0].Apply(AfflictionKind.Static, 5);
        Mark();
        yield return ShotAfter(1.5f);
        Sets()[0].Apply(AfflictionKind.Burn, 5);
        yield return ShotAfter(1.2f);
        yield return new WaitForSeconds(0.45f);
        yield return ShotAfter(0.05f);
        yield return new WaitForSeconds(0.4f);
        Say($"meltdown: at five and five: callout '{defence.LastCallout}', meltdowns {upgrades.Meltdowns} (45 to it, half to the rest, less whatever the burn did meanwhile): {Lost()}");
    }

    private IEnumerator Front()
    {
        yield return Begin("front", "Storm Front");
        yield return Near();
        int[] shoves = group.Select(dummy => dummy.Shoves).ToArray();
        float mana = combat.Mana.Current;
        int casts = element.SpellsCast;
        Mark();
        yield return Blows(1, Key.K);
        yield return ShotAfter(0.1f);
        yield return ShotAfter(0.2f);
        Say($"front: a heavy blow landed: free Gusts {element.FreeCasts}, spells counted as cast {element.SpellsCast - casts}, mana {mana:0} -> {combat.Mana.Current:0}, shoves {string.Join(", ", group.Select((dummy, i) => dummy.Shoves - shoves[i]))}; {Lost()}");
        Mark();
        yield return Blows(2, Key.J);
        Say($"front: two light blows: free Gusts {element.FreeCasts} (no more)");
    }

    private IEnumerator Reserve()
    {
        yield return Begin("reserve", "Deep Reserve");
        upgrades.Give(Upgrade.All.First(upgrade => upgrade.Id == Upgrade.StoredCharge));
        yield return null;
        string id = Upgrade.All.First(upgrade => upgrade.Name == "Deep Reserve").Id;
        string banked = "";
        for (int i = 0; i < 4; i++)
        {
            yield return Guarded(true);
            banked += (i > 0 ? ", " : "") + lastGuard + " -> " + upgrades.Tally(id);
            yield return new WaitForSeconds(0.25f);
        }
        Say($"reserve: four perfect guards bank {banked} (three at most); charge ready {upgrades.ChargeStored}");
        yield return Near();
        Shot();
        string spent = "";
        for (int i = 0; i < 4; i++)
        {
            Mark();
            yield return Blows(1, Key.J);
            yield return new WaitForSeconds(0.2f);
            spent += (i > 0 ? "; " : "") + $"blow {i + 1} lost {before[0] - group[0].Health.Current:0}, left {upgrades.Tally(id)}";
        }
        Say($"reserve: {spent} (the fourth blow is a plain one)");
    }

    private IEnumerator Lodestone()
    {
        yield return Begin("lodestone", "Lodestone");
        Sets()[0].Apply(AfflictionKind.Static, 4);
        if (real != null)
        {
            Vector3 from = real.transform.position + (player.transform.position - real.transform.position).normalized * 6f;
            player.Teleport(from, Quaternion.LookRotation(real.transform.position - from));
            yield return new WaitForSeconds(0.2f);
        }
        float gap = Vector3.Distance(FirstAt, player.transform.position);
        int[] shoves = group.Select(dummy => dummy.Shoves).ToArray();
        Mark();
        bool cast = upgrades.TrySpecial();
        float nearest = gap;
        for (int i = 0; i < 3; i++)
        {
            float until = Time.time + (i == 0 ? 0.16f : i == 1 ? 0.2f : 0.12f);
            while (Time.time < until)
            {
                nearest = Mathf.Min(nearest, Vector3.Distance(FirstAt, player.transform.position));
                yield return null;
            }
            Shot();
        }
        float after = Time.time + 0.5f;
        while (Time.time < after)
        {
            nearest = Mathf.Min(nearest, Vector3.Distance(FirstAt, player.transform.position));
            yield return null;
        }
        string moved = real != null ? $"gap {gap:0.0} m before, nearest {nearest:0.0}, {Vector3.Distance(FirstAt, player.transform.position):0.0} half a second after" : "shoves " + string.Join(", ", group.Select((dummy, i) => dummy.Shoves - shoves[i]));
        Say($"lodestone: cast {cast}: first carried Static x4 (owed 1 + 0.8 x 4 = 4.2 m, to sword reach at most), {moved}; Static left on first x{Sets()[0].Stacks(AfflictionKind.Static)}; {Lost()}");
    }

    private IEnumerator Circle()
    {
        yield return Begin("circle", "Closing Circle");
        Vector3 start = player.transform.position;
        bool cast = upgrades.TrySpecial();
        yield return null;
        Say($"circle: begun: runs {SpellWorks.RunningOf("circle")}, hold {player.FormHold}, apart {(FirstAt - player.transform.position).magnitude:0.0}");
        yield return ShotAfter(0.4f);
        yield return ShotAfter(0.36f);
        yield return ShotAfter(0.2f);
        Say($"circle: at the column: {Up()}, taken {CircleForm.Taken}");
        float give = Time.time + 3f;
        while (SpellWorks.RunningOf("circle") > 0 && Time.time < give) yield return null;
        var body = player.GetComponent<CharacterController>();
        Say($"circle: cast {cast}: taken {CircleForm.Taken}; {Lost()}; player ended {(player.transform.position - start).magnitude:0.00} m from the start, body on {body.enabled}, hold {player.FormHold}");
        yield return new WaitForSeconds(1.2f);
    }

    private IEnumerator Eye()
    {
        yield return Begin("eye", "Eye of the Storm");
        Vector3[] was = real != null ? new[] { real.transform.position } : group.Select(dummy => dummy.transform.position).ToArray();
        EyeForm.HoldForProbe = true;
        bool cast = upgrades.TrySpecial();
        yield return null;
        Say($"eye: begun: runs {SpellWorks.RunningOf("eye")}, hold {player.FormHold}");
        yield return ShotAfter(0.9f);
        Say($"eye: turning: {Up()}; unflinching {defence.Unflinching}, damage taken x{upgrades.Stats.Multiplier(PlayerStat.DamageTaken):0.00}, walking x{upgrades.Stats.Multiplier(PlayerStat.MoveSpeed):0.00}, bites {EyeForm.Bites}");
        yield return ShotAfter(0.9f);
        float give = Time.time + 2f;
        while (SpellWorks.RunningOf("eye") > 0 && EyeForm.Bolts == 0 && Time.time < give) yield return null;
        Shot();
        yield return null;
        EyeForm.HoldForProbe = false;
        give = Time.time + 2f;
        while (SpellWorks.RunningOf("eye") > 0 && Time.time < give) yield return null;
        string moved = "";
        if (real != null) moved = (real.transform.position - was[0]).magnitude.ToString("0.0");
        else for (int i = 0; i < group.Count; i++) moved += (i > 0 ? ", " : "") + (group[i].transform.position - was[i]).magnitude.ToString("0.0");
        Say($"eye: cast {cast}: bites {EyeForm.Bites}, bolts {EyeForm.Bolts}; {Lost()}; moved [{moved}] m; unflinching after {defence.Unflinching}");
        yield return new WaitForSeconds(0.8f);
        if (real == null)
        {
            for (int i = 0; i < group.Count; i++) group[i].transform.position = spots[i];
            Physics.SyncTransforms();
        }
    }

    private IEnumerator Furnace()
    {
        yield return Begin("furnace", "Open Furnace");
        LayAll(AfflictionKind.Burn, 4);
        yield return new WaitForSeconds(0.2f);
        Mark();
        int bursts = upgrades.FurnaceBursts;
        yield return Guarded(true);
        Shot();
        yield return null;
        Say($"furnace: perfect guard {lastGuard}: set off {upgrades.FurnaceBursts - bursts}; {Lost()}; grey of a plain guard x{upgrades.Stats.Multiplier(PlayerStat.GuardGrey):0.00}");
        yield return ShotAfter(0.2f);
    }

    private IEnumerator Stroke()
    {
        yield return Begin("stroke", "Single Stroke");
        upgrades.Give(Upgrade.ById(Upgrade.Static));
        yield return new WaitForSeconds(0.5f);
        var sets = Sets();
        sets[0].Apply(AfflictionKind.Static, 10);
        Say($"stroke: Static laid ten deep stands {sets[0].Stacks(AfflictionKind.Static)} deep");
        Mark();
        int strokes = upgrades.StrokeDischarges;
        yield return Tap(Key.K);
        float give = Time.time + 2.5f;
        while (upgrades.StrokeDischarges == strokes && Time.time < give) yield return null;
        Shot();
        yield return new WaitForSeconds(0.3f);
        Say($"stroke: discharges kept to one {upgrades.StrokeDischarges - strokes}; {Lost()}");
        yield return new WaitForSeconds(0.8f);
    }

    private IEnumerator AllAtOnce()
    {
        yield return Begin("allatonce", "All at Once");
        yield return new WaitForSeconds(0.5f);
        var sets = Sets();
        if (sets.Length > 1) sets[1].Apply(AfflictionKind.Chill, 3);
        sets[0].Apply(AfflictionKind.Chill, 5);
        yield return null;
        Say($"allatonce: frozen {sets[0].IsFrozen}; one chilled three deep moves at x{(sets.Length > 1 ? sets[1].SpeedMultiplier : -1f):0.00}; shatter x{Afflictions.ShatterDamage:0.00}");
        Mark();
        yield return Tap(Key.K);
        float give = Time.time + 2.5f;
        while (sets[0].IsFrozen && Time.time < give) yield return null;
        Shot();
        yield return new WaitForSeconds(0.3f);
        Say($"allatonce: after the heavy blow: {Lost()}");
        yield return new WaitForSeconds(0.6f);
    }

    // A real enemy only: a dummy has no life to end.
    private IEnumerator CleanBreak()
    {
        if (real == null) { Say("cleanbreak: needs a real enemy; played in Castle"); yield break; }
        yield return Begin("cleanbreak", "Clean Break", "Ice");
        upgrades.Give(Upgrade.All.First(upgrade => upgrade.Element == "Ice" && upgrade.Kind == UpgradeKind.Root));
        yield return new WaitForSeconds(0.5f);
        real.Health.Damage(real.Health.Maximum * 0.9f);
        Vector3 close = real.transform.position + (player.transform.position - real.transform.position).normalized * 1.5f;
        player.Teleport(close, Quaternion.LookRotation(real.transform.position - close));
        real.Afflictions.Apply(AfflictionKind.Chill, 5);
        yield return null;
        Say($"cleanbreak: frozen {real.Afflictions.IsFrozen}, life left {real.HealthFraction:0.00}");
        int broke = upgrades.CleanBreaks;
        yield return Tap(Key.K);
        float give = Time.time + 2.5f;
        while (upgrades.CleanBreaks == broke && Time.time < give) yield return null;
        Shot();
        yield return new WaitForSeconds(0.4f);
        Say($"cleanbreak: blows landed {combat.BlowsLanded}, still frozen {real.Afflictions.IsFrozen}, brittle {real.Afflictions.IsBrittle}");
        Say($"cleanbreak: lives ended {upgrades.CleanBreaks - broke}; dead {real.IsDead}, life left {real.HealthFraction:0.00}");
    }

    private IEnumerator Heartwood()
    {
        yield return Begin("heartwood", "Heartwood");
        yield return new WaitForSeconds(0.6f);
        float paid = upgrades.HealthPaid;
        float health = defence.Health.Current;
        Say($"heartwood: no mana {combat.Mana.Sealed}, the bar reads {combat.Mana.Fraction:0.00}, health {health:0}");
        yield return Tap(Key.Q);
        yield return ShotAfter(0.35f);
        yield return new WaitForSeconds(0.4f);
        Say($"heartwood: the base spell cast: health paid {upgrades.HealthPaid - paid:0.0} (grey {defence.Health.Grey:0.0}), health now {defence.Health.Current:0}; {Lost()}");
        float mended = upgrades.HeartwoodMended;
        float low = defence.Health.Current;
        combat.Mana.Gain(30f);
        yield return null;
        yield return null;
        Say($"heartwood: thirty mana gained mends {upgrades.HeartwoodMended - mended:0.0}; health {low:0} to {defence.Health.Current:0}");
        upgrades.Forget();
        yield return new WaitForSeconds(0.6f);
        Say($"heartwood: forgotten: no mana {combat.Mana.Sealed}");
    }

    private IEnumerator Unmoved()
    {
        foreach (string name in new[] { "Unmoved", "Heavy Hand" })
        {
            yield return Begin("unmoved", name);
            yield return new WaitForSeconds(0.6f);
            Keys(Key.K);
            yield return null;
            yield return null;
            Keys();
            float give = Time.time + 1.5f;
            while (!combat.InHeavySwing && Time.time < give) yield return null;
            yield return null;
            int poised = defence.PoisedBlows;
            int landed = combat.BlowsLanded;
            float health = defence.Health.Current;
            float share = upgrades.Stats.Multiplier(PlayerStat.DamageTaken);
            bool swinging = combat.InHeavySwing;
            defence.DamageTaken = share;
            defence.TakeHit(new Hit(20f, 0f, -toward, false, false, FirstAt));
            yield return null;
            bool helpless = defence.IsHelpless;
            give = Time.time + 1.5f;
            while (combat.BlowsLanded == landed && Time.time < give) yield return null;
            Say($"unmoved: {name}: struck in a heavy swing {swinging}: damage taken x{share:0.00}, lost {health - defence.Health.Current:0.0} of 20, walked through {defence.PoisedBlows - poised}, flinched {helpless}, the blow still landed {combat.BlowsLanded > landed}");
            yield return new WaitForSeconds(0.6f);
        }
    }

    private IEnumerator Leaf()
    {
        yield return Begin("leaf", "Leaf on the Wind");
        yield return new WaitForSeconds(0.6f);
        string id = Upgrade.All.First(upgrade => upgrade.Name == "Leaf on the Wind").Id;
        upgrades.BreathSeconds = 60f;
        player.Stamina.Fill();
        float stamina = player.Stamina.Current;
        Say($"leaf: breaths {upgrades.Tally(id)}, shown as '{upgrades.TallyText.Trim()}'");
        int dodges = upgrades.DodgeCount;
        for (int i = 0; i < 4; i++)
        {
            Keys(Key.A, Key.Space);
            yield return null;
            yield return null;
            Keys();
            yield return new WaitForSeconds(0.75f);
        }
        Say($"leaf: four presses: dodged {upgrades.DodgeCount - dodges}, breaths spent {upgrades.BreathsSpent}, breaths left {upgrades.Tally(id)}, stamina spent {stamina - player.Stamina.Current:0}");
        player.Teleport(stand, Quaternion.LookRotation(toward));
        yield return new WaitForSeconds(0.2f);
        yield return Blows(3, Key.J);
        Say($"leaf: after three blows landed: breaths {upgrades.Tally(id)}");
        upgrades.BreathSeconds = 2f;
        yield return new WaitForSeconds(2.3f);
        Say($"leaf: after two seconds more: breaths {upgrades.Tally(id)}; damage taken x{upgrades.Stats.Multiplier(PlayerStat.DamageTaken):0.00}");
        upgrades.Forget();
        yield return new WaitForSeconds(0.6f);
        Say($"leaf: forgotten: the dodge asks stamina again {player.Dodge.PayInstead == null}");
    }

    private IEnumerator Tide()
    {
        yield return Begin("tide", "Spring Tide");
        yield return new WaitForSeconds(0.6f);
        float held = combat.Mana.Current;
        int tides = upgrades.Tides;
        yield return Tap(Key.Q);
        yield return ShotAfter(0.35f);
        yield return new WaitForSeconds(0.7f);
        Say($"tide: the base spell ({element.Kit.SpellName}, {element.Kit.SpellCost:0} mana, {element.Kit.SpellDamage:0} damage) cast with {held:0} held: tides {upgrades.Tides - tides}, x{upgrades.LastTide:0.00}, mana left {combat.Mana.Current:0}; {Lost()}");
        yield return new WaitForSeconds(1f);
        Mark();
        combat.Mana.Gain(element.Kit.SpellCost + 0.5f);
        yield return Tap(Key.Q);
        yield return new WaitForSeconds(1.1f);
        Say($"tide: cast again with only its cost held: x{upgrades.LastTide:0.00}, mana left {combat.Mana.Current:0}; {Lost()}");
    }

    private IEnumerator Answer()
    {
        yield return Begin("answer", "Master's Answer");
        yield return new WaitForSeconds(0.6f);
        float plain = combat.Combo.SkillManaCost;
        Say($"answer: the skill costs {combat.Combo.SkillPrice:0} where it cost {plain:0}; at once {combat.Combo.SkillAtOnce}");
        yield return Guarded(true);
        yield return null;
        float mana = combat.Mana.Current;
        float pressed = Time.time;
        Say($"answer: after a perfect guard ({lastGuard}): the skill costs {combat.Combo.SkillPrice:0}, at once {combat.Combo.SkillAtOnce}");
        Keys(Key.K);
        float give = Time.time + 1.2f;
        while (combat.Combo.Current != combat.Combo.Skill && Time.time < give) yield return null;
        bool came = combat.Combo.Current == combat.Combo.Skill;
        float took = Time.time - pressed;
        Keys();
        yield return ShotAfter(0.25f);
        yield return new WaitForSeconds(1f);
        Say($"answer: heavy held: the skill came {came} after {took:0.00} s, mana spent {mana - combat.Mana.Current:0}; {Lost()}");
        yield return new WaitForSeconds(1.2f);
        Say($"answer: two seconds on: the skill costs {combat.Combo.SkillPrice:0}, at once {combat.Combo.SkillAtOnce}");
    }

    private IEnumerator WellMove()
    {
        yield return Begin("well", "Deep Well");
        yield return new WaitForSeconds(0.6f);
        defence.Health.Chip(60f);
        yield return null;
        float low = defence.Health.Current;
        int flasks = defence.Flasks.Count;
        yield return Tap(Key.R);
        float give = Time.time + 3f;
        while (upgrades.Well <= 0f && Time.time < give) yield return null;
        yield return new WaitForSeconds(0.3f);
        Say($"well: flasks carried {flasks}; one drunk at health {low:0}: health {defence.Health.Current:0}, the well holds {upgrades.Well:0}, shown as '{upgrades.TallyText.Trim()}'");
        yield return new WaitForSeconds(1.5f);
        float waited = defence.Health.Current;
        float held = upgrades.Well;
        yield return Blows(4, Key.J);
        yield return new WaitForSeconds(1f);
        Say($"well: a second and a half with no blow: health {waited:0}; then four blows: health {defence.Health.Current:0}, the well paid {held - upgrades.Well:0.0} and holds {upgrades.Well:0}");
    }

    // ---- The gods' gifts.


    private IEnumerator SkyDance()
    {
        for (int pass = 0; pass < 2; pass++)
        {
            if (pass == 1) yield return new WaitForSeconds(2.5f);
            yield return Begin("skydance", "Sky Dance");
            if (real != null) Say($"skydance: the soldier: staggered {real.IsStaggered}, airborne {real.Afflictions.IsAirborne}, dead {real.IsDead}");
            Vector3 was = real != null ? real.transform.position : group[0].transform.position;
            bool cast = upgrades.TrySpecial();
            var spec = upgrades.Worn.Spell;
            yield return null;
            Say($"skydance: pass {pass} begun: runs {SpellWorks.RunningOf("skydance")}, hold {player.FormHold}, enemy ahead {combat.EnemyAhead(12f) != null}, apart {(FirstAt - player.transform.position).magnitude:0.0}");
            yield return new WaitForSeconds(0.22f);
            for (int cut = 0; cut < spec.Count; cut++)
            {
                // In time on the first pass; on the second no press at all.
                yield return new WaitForSeconds(spec.Interval * 0.78f);
                if (cut == 1) Shot();
                if (pass == 0) yield return Tap(Key.J);
                else yield return new WaitForSeconds(spec.Interval * 0.22f);
            }
            Keys(Key.D);
            yield return ShotAfter(0.3f);
            float give = Time.time + 3f;
            while (SpellWorks.RunningOf("skydance") > 0 && player.FormHold != 0 && Time.time < give) yield return null;
            Keys();
            yield return ShotAfter(0.05f);
            yield return new WaitForSeconds(0.6f);
            Vector3 now = real != null ? real.transform.position : group[0].transform.position;
            var body = player.GetComponent<CharacterController>();
            Say($"skydance: pass {pass} cast {cast}: cuts in time {SkyDanceForm.Landed}; {Lost()}; thrown {(now - was).magnitude:0.0} m; player y {player.transform.position.y:0.00}, body on {body.enabled}, hold {player.FormHold}, runs {SpellWorks.RunningOf("skydance")}");
            if (real == null) { group[0].transform.position = spots[0]; Physics.SyncTransforms(); }
        }
    }

    private IEnumerator Beat()
    {
        yield return Begin("beat", "Catching");
        int bites = upgrades.PoisonBites;
        Sets()[0].Apply(AfflictionKind.Poison, 4);
        yield return new WaitForSeconds(2.4f);
        Shot();
        Say($"beat: Catching: bites heard {upgrades.PoisonBites - bites}, doses passed {upgrades.Passed}; {Lost()}");
        yield return Begin("beat", "In Time");
        Sets()[0].Apply(AfflictionKind.Poison, 2);
        // A blow thrown to land near the two-second bite, then one well away from it.
        if (real == null) player.Teleport(group[0].transform.position - toward * 1.6f, Quaternion.LookRotation(toward));
        yield return new WaitForSeconds(1.75f);
        yield return Blows(1, Key.J);
        yield return new WaitForSeconds(0.1f);
        float crit = upgrades.Stats.Sum(PlayerStat.CritChance);
        Say($"beat: In Time: a blow on the beat: {Lost()}; critical chance now {crit:0.00}");
        yield return new WaitForSeconds(0.9f);
        int deep = Sets()[0].Stacks(AfflictionKind.Poison);
        yield return Blows(1, Key.J);
        Say($"beat: In Time: a blow off the beat: poison {deep} -> {Sets()[0].Stacks(AfflictionKind.Poison)}; critical chance {upgrades.Stats.Sum(PlayerStat.CritChance):0.00}");
    }

    private IEnumerator Lance()
    {
        yield return Begin("lance", "Lance the Wound");
        LayAll(AfflictionKind.Poison, 4);
        yield return new WaitForSeconds(0.3f);
        Mark();
        yield return Guarded(true);
        Shot();
        Say($"lance: perfect guard {lastGuard}: {Lost()} (a bite of four doses is 8)");
    }

    private IEnumerator Volatile()
    {
        yield return Begin("volatile", "Volatile");
        LayAll(AfflictionKind.Poison, 4);
        yield return new WaitForSeconds(2.6f);
        Say($"volatile: beat stilled {Afflictions.PoisonBeatOff}: after 2.6 s {Lost()}");
        yield return Begin("volatile", "Slow Death");
        LayAll(AfflictionKind.Poison, 30);
        yield return new WaitForSeconds(7f);
        Say($"slowdeath: keeps {Afflictions.PoisonKeeps}, depth allowed {Afflictions.Depth(AfflictionKind.Poison)}: after 7 s {Lost()}");
        Shot();
    }

    private IEnumerator Dead()
    {
        yield return Begin("dead", "Dead Already");
        float health = defence.Health.Current;
        bool risen = upgrades.Stats.Holds(StatWhen.Risen);
        yield return new WaitForSeconds(3f);
        Say($"dead: Risen holds {risen}; health {health:0} -> {defence.Health.Current:0} in 3 s by an enemy (drained {upgrades.DeadDrained}); rises again {upgrades.Rise(FirstAt)}");
        yield return Begin("dead", "Hollow Strength");
        Say($"dead: Hollow Strength without the Rise: unflinching {defence.Unflinching}");
        defence.Restore();
    }

    private IEnumerator Lotus()
    {
        yield return Begin("lotus", "Great Lotus");
        defence.DamageTaken = 0.3f;
        defence.TakeHit(new Hit(60f, 0f, -toward, false, true, FirstAt));
        defence.DamageTaken = 0f;
        yield return new WaitForSeconds(0.8f);
        yield return Surge(14f);
        yield return Tap(Key.R);
        float give = Time.unscaledTime + 4f;
        while (SpellWorks.RunningOf("lotus") == 0 && Time.unscaledTime < give) yield return null;
        yield return new WaitForSeconds(0.6f);
        Mark();
        string outcomes = "";
        float began = Time.time;
        for (int i = 0; i < 5 && SpellWorks.RunningOf("lotus") > 0; i++)
        {
            yield return Guarded(true);
            outcomes += lastGuard + " ";
            if (i == 3) Shot();
            yield return new WaitForSeconds(0.45f);
        }
        Shot();
        yield return ShotAfter(0.25f);
        yield return new WaitForSeconds(0.5f);
        Say($"lotus: five guards [{outcomes}] in {Time.time - began:0.0} s; lotus still runs {SpellWorks.RunningOf("lotus")}; burst: {Lost()}");
    }

    private IEnumerator Flash()
    {
        yield return Begin("flash", "Flashpoint");
        if (real == null) player.Teleport(group[0].transform.position - toward * 1.6f, Quaternion.LookRotation(toward));
        yield return new WaitForSeconds(0.2f);
        int dodges = defence.PerfectDodges;
        Keys(Key.Space);
        yield return null;
        yield return null;
        Keys();
        float wait = Time.time + 0.3f;
        while (!player.Dodge.IsInvulnerable && Time.time < wait) yield return null;
        var outcome = defence.TakeHit(new Hit(20f, 0f, -toward, false, false, FirstAt));
        yield return new WaitForSeconds(0.05f);
        float held = upgrades.Stats.Sum(PlayerStat.AfflictionStacks);
        float give = Time.time + 1.2f;
        while (player.Dodge.IsRolling && Time.time < give) yield return null;
        if (real == null) player.Teleport(group[0].transform.position - toward * 1.6f, Quaternion.LookRotation(toward));
        yield return Blows(1, Key.J);
        yield return new WaitForSeconds(0.15f);
        Say($"flash: Flashpoint: dodge {outcome}, perfect dodges +{defence.PerfectDodges - dodges}, stacks boon held {held:0} then {upgrades.Stats.Sum(PlayerStat.AfflictionStacks):0} after one blow that landed; {Lost()}");
    }
}
