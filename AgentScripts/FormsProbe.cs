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

// Play Mode probe for the spell forms. In the combat arena it gathers three dummies into a group, and for every form
// with a sample spell: takes the element that suits it, casts it at the group, photographs it forming, landing and
// after (review/shots/current/form_<id>_<n>.png), and writes what each dummy lost and what was laid on it. Then it
// casts everything twice over on top of itself, and lets the player fall with spells in the air. In any other scene it
// casts each form once at the nearest enemy and says what that enemy lost.
// A file %TEMP%/forms_probe.txt may hold: a comma list of form ids to play only those; "soak=<seconds>" to keep
// casting every form in turn for that long (for the profiler) instead of the survey.
//   unity command script run -- --file AgentScripts/FormsProbe.cs --entry FormsProbe.Run
public static class FormsProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Forms Probe").AddComponent<FormsProbeRunner>();
        return "probe started";
    }
}

public class FormsProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private readonly List<TrainingDummy> group = new List<TrainingDummy>();
    private int errors;
    private string firstError = "";

    // Seconds after the cast at which each form is photographed: forming, landing, after.
    private static readonly Dictionary<string, float[]> Shots = new Dictionary<string, float[]>
    {
        { "blades", new[] { 0.45f, 0.86f, 1.5f } }, { "sword", new[] { 0.6f, 1.13f, 1.8f } }, { "beam", new[] { 0.2f, 0.9f, 2.7f } },
        { "phalanx", new[] { 0.45f, 1.0f, 2.0f } }, { "dragon", new[] { 0.25f, 0.5f, 0.85f } }, { "nova", new[] { 0.3f, 0.5f, 1.1f } },
        { "orb", new[] { 0.6f, 1.75f, 2.4f } }, { "bomb", new[] { 0.8f, 1.35f, 1.62f } }, { "spikes", new[] { 0.16f, 0.5f, 1.1f } },
        { "field", new[] { 0.3f, 1.2f, 3f } }, { "drill", new[] { 0.12f, 0.3f, 0.75f } }, { "minions", new[] { 0.35f, 1.6f, 3f } },
        { "bullets", new[] { 0.35f, 0.68f, 0.9f } }, { "reverse", new[] { 0.45f, 0.95f, 1.4f } }, { "parry", new[] { 0.3f, 0.52f, 1f } },
        { "wall", new[] { 0.2f, 0.8f, 2.3f } }, { "discs", new[] { 0.4f, 1.5f, 3f } }, { "brand", new[] { 0.4f, 1.3f, 2.6f } },
        { "rain", new[] { 0.4f, 1.5f, 3f } }, { "clone", new[] { 0.6f, 1.6f, 2.6f } }, { "flash", new[] { 0.22f, 0.6f, 1.02f } },
        { "ring", new[] { 0.25f, 1.2f, 3f } }, { "pillars", new[] { 0.25f, 0.47f, 1f } }, { "step", new[] { 0.08f, 0.22f, 0.8f } },
    };

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private static void Shot(string name) => ScreenCapture.CaptureScreenshot("review/shots/current/form_" + name + ".png");

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

    private bool finished;

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
        if (finished) return;
        Say($"the probe was taken away before it finished (the scene changed?); errors logged {errors}{(errors > 0 ? ": " + firstError : "")}");
        Say("done");
    }

    private bool Cast(Upgrade sample)
    {
        if (element.Info.Name != sample.Element) element.Take(ElementKit.ByName(sample.Element, false));
        upgrades.Lend(sample);
        combat.Mana.Gain(1000f);
        return upgrades.TrySpecial();
    }

    private static string Laid(TrainingDummy dummy)
    {
        string found = "";
        foreach (AfflictionKind kind in Enum.GetValues(typeof(AfflictionKind)))
        {
            if (kind == AfflictionKind.None) continue;
            if (kind == AfflictionKind.Burn ? dummy.Burn.IsBurning : dummy.Afflictions.Stacks(kind) > 0)
                found += (found.Length > 0 ? "+" : "") + kind + (kind == AfflictionKind.Burn ? "" : "x" + dummy.Afflictions.Stacks(kind));
        }
        return found.Length > 0 ? found : "none";
    }

    private IEnumerator Start()
    {
        Application.logMessageReceived += OnLog;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        string only = "";
        float soak = 0f;
        string config = Path.Combine(Path.GetTempPath(), "forms_probe.txt");
        if (File.Exists(config))
        {
            only = File.ReadAllText(config).Trim();
            if (only.StartsWith("soak="))
            {
                float.TryParse(only.Substring(5), out soak);
                only = "";
            }
        }
        var samples = FormSamples.All.Where(sample => SpellForms.Get(sample.Form) != null && (only.Length == 0 || only.Split(',').Contains(sample.Form))).ToList();
        Say($"start: {samples.Count} forms to play of {FormSamples.All.Count} samples, {SpellForms.All.Count} forms registered; works {(FindAnyObjectByType<SpellWorks>() != null)} fx {(FindAnyObjectByType<FormFx>() != null)}");
        yield return new WaitForSeconds(0.3f);

        var dummies = FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None).Where(dummy => dummy.GetComponent<DummyAttacker>() == null).ToList();
        if (dummies.Count >= 3) yield return Yard(dummies, samples, soak);
        else yield return Elsewhere(samples);
        Say($"errors logged {errors}{(errors > 0 ? ": " + firstError : "")}");
        finished = true;
        Say("done");
    }

    private IEnumerator Yard(List<TrainingDummy> dummies, List<Upgrade> samples, float soak)
    {
        // Three together east of the middle: the pair as built, and the third brought beside them.
        var spots = new[] { new Vector3(5f, 0f, -2f), new Vector3(6.5f, 0f, -1.1f), new Vector3(5.7f, 0f, -3.4f) };
        for (int i = 0; i < 3; i++)
        {
            dummies[i].transform.position = spots[i];
            dummies[i].Health.Maximum = 100000f;
            dummies[i].Health.Fill();
            dummies[i].RestoreAfter = 99999f;
            group.Add(dummies[i]);
        }
        Physics.SyncTransforms();
        Vector3 stand = new Vector3(2.5f, 0f, -1.9f);
        Vector3 toward = (spots[0] - stand).normalized;
        defence.DamageTaken = 0f;

        if (soak > 0f)
        {
            Say($"soak: casting every form in turn for {soak:0} s");
            float until = Time.time + soak;
            int cast = 0;
            while (Time.time < until)
            {
                var sample = samples[cast % samples.Count];
                if (player.FormHold == 0)
                {
                    if (cast % 6 == 0) player.Teleport(stand, Quaternion.LookRotation(toward));
                    Cast(sample);
                    cast++;
                }
                yield return new WaitForSeconds(0.45f);
            }
            Say($"soak: {cast} casts, runs {SpellWorks.Running}, solids {FormFx.SolidsShown}, marks {FormFx.MarksShown}");
            yield break;
        }

        foreach (var sample in samples)
        {
            string id = sample.Form;
            // Clean ground: nothing of the last form left, the player back at the mark, the dummies with nothing on them.
            yield return new WaitUntil(() => player.FormHold == 0 && !player.IsLeaping);
            player.Teleport(stand, Quaternion.LookRotation(toward));
            foreach (var dummy in group)
            {
                dummy.Afflictions.Clear();
                dummy.Stagger.Reset();
            }
            yield return new WaitForSeconds(0.35f);
            var before = group.Select(dummy => dummy.Health.Current).ToArray();
            int shovesBefore = group.Sum(dummy => dummy.Shoves);
            int begunBefore = SpellWorks.Begun;
            bool castOk = Cast(sample);
            float castAt = Time.time;
            float[] at = Shots.TryGetValue(id, out var times) ? times : new[] { 0.3f, 0.8f, 1.5f };
            bool acted = false;
            int crits = combat.Criticals;
            int peakSolids = 0, peakMarks = 0;
            for (int shot = 0; shot < at.Length; shot++)
            {
                while (Time.time - castAt < at[shot])
                {
                    peakSolids = Mathf.Max(peakSolids, FormFx.SolidsShown);
                    peakMarks = Mathf.Max(peakMarks, FormFx.MarksShown);
                    float since = Time.time - castAt;
                    // What some forms wait on: a blow of the weapon, or a blow taken.
                    if (!acted && id == "parry" && since >= 0.4f)
                    {
                        acted = true;
                        var outcome = defence.TakeHit(new Hit(20f, 0f, -toward, false, false, spots[0]));
                        Say($"parry: a blow from the first dummy came back as {outcome}");
                    }
                    if (!acted && (id == "phalanx" || id == "wall") && since >= (id == "wall" ? 1.2f : 1.1f))
                    {
                        acted = true;
                        yield return Tap(Key.K);
                    }
                    if (!acted && id == "orb" && since >= 1.35f)
                    {
                        acted = true;
                        yield return Tap(Key.J);
                    }
                    if (!acted && (id == "brand" || id == "clone") && since >= 0.55f)
                    {
                        acted = true;
                        for (int blow = 0; blow < 3; blow++)
                        {
                            int landed = combat.BlowsLanded;
                            yield return Tap(Key.J);
                            float give = Time.unscaledTime + 1f;
                            while (combat.BlowsLanded == landed && Time.unscaledTime < give) yield return null;
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    if (!acted && id == "step" && since >= 0.45f)
                    {
                        acted = true;
                        yield return Tap(Key.K);
                    }
                    yield return null;
                }
                Shot(id + "_" + (shot + 1));
                yield return null;
            }
            // Let whatever is left of it finish, within reason.
            float limit = Time.time + 9f;
            while (SpellWorks.RunningOf(id) > 0 && Time.time < limit) yield return null;
            yield return new WaitForSeconds(0.6f);
            string lost = "";
            int struck = 0;
            for (int i = 0; i < group.Count; i++)
            {
                float took = before[i] - group[i].Health.Current;
                if (took > 0.01f) struck++;
                lost += (i > 0 ? ", " : "") + took.ToString("0");
            }
            Say($"{id} ({sample.Element}) cast {castOk} runs begun {SpellWorks.Begun - begunBefore}: struck {struck} of 3, lost [{lost}], laid {Laid(group[0])} / {Laid(group[1])} / {Laid(group[2])}, shoves {group.Sum(dummy => dummy.Shoves) - shovesBefore}, criticals {combat.Criticals - crits}, peak solids {peakSolids} marks {peakMarks}, still running {SpellWorks.RunningOf(id)}, hold {player.FormHold} hidden {player.Hidden}, player at {player.transform.position.x:0.0},{player.transform.position.z:0.0}");
        }

        // Everything twice over, on top of itself.
        yield return new WaitUntil(() => player.FormHold == 0);
        player.Teleport(stand, Quaternion.LookRotation(toward));
        int peakRuns = 0, most = 0, mostMarks = 0;
        for (int round = 0; round < 2; round++)
            foreach (var sample in samples)
            {
                Cast(sample);
                Cast(sample);
                for (int frame = 0; frame < 6; frame++)
                {
                    peakRuns = Mathf.Max(peakRuns, SpellWorks.Running);
                    most = Mathf.Max(most, FormFx.SolidsShown);
                    mostMarks = Mathf.Max(mostMarks, FormFx.MarksShown);
                    yield return null;
                }
            }
        Say($"twice over: peak runs {peakRuns}, solids {most}, marks {mostMarks}, hazards {Clover.World.Hazards.Alive}");
        Shot("all_at_once");
        yield return null;

        // The player falls with spells in the air: every run ends with them and the body is their own again.
        foreach (var sample in samples.Take(8)) Cast(sample);
        yield return null;
        int runsBeforeFall = SpellWorks.Running;
        defence.DamageTaken = 1f;
        player.BreakFormHold();
        defence.Health.Damage(100000f);
        defence.TakeHit(new Hit(10f, 0f, Vector3.forward, false, true, stand + Vector3.forward));
        yield return null;
        yield return null;
        yield return null;
        Say($"fall: dead {defence.IsDead}, runs {runsBeforeFall} -> {SpellWorks.Running}, hold {player.FormHold}, hidden {player.Hidden}, clone showing {(SpellWorks.Clone != null && SpellWorks.Clone.Showing)}");
        yield return new WaitForSeconds(3.2f);
        Say($"up again: dead {defence.IsDead}, solids {FormFx.SolidsShown}, marks {FormFx.MarksShown}, runs {SpellWorks.Running}");
    }

    // Words on the screen stop the world; the probe clicks through them so the fight goes on.
    private IEnumerator ClickThrough()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (Time.timeScale > 0f && !player.Suspended) continue;
            stops++;
            Keys(Key.E);
            yield return new WaitForSecondsRealtime(0.1f);
            Keys();
        }
    }

    private int stops;

    // The upgrades write the player's own numbers back every moment, so the probe keeps them standing by hand.
    private IEnumerator KeepAlive()
    {
        while (true)
        {
            if (!defence.IsDead) defence.Health.Fill();
            yield return null;
        }
    }

    private IEnumerator Elsewhere(List<Upgrade> samples)
    {
        // Another scene: each form once at whatever enemy is nearest, to be sure nothing throws on the real thing.
        defence.DamageTaken = 0f;
        StartCoroutine(ClickThrough());
        StartCoroutine(KeepAlive());
        var boss = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Where(enemy => !enemy.IsDead)
            .OrderByDescending(enemy => enemy.Kind.IsBoss).ThenBy(enemy => (enemy.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
        var parts = FindObjectsByType<ColossusPart>(FindObjectsSortMode.None);
        Say($"elsewhere: scene {gameObject.scene.name}, enemy {(boss != null ? boss.name : "none")}, colossus parts {parts.Length}");
        if (boss == null) yield break;
        boss.Health.Maximum = 100000f;
        boss.Health.Fill();
        foreach (var sample in samples)
        {
            float patience = Time.unscaledTime + 8f;
            while ((player.FormHold != 0 || player.IsLeaping) && Time.unscaledTime < patience) yield return null;
            if (boss == null || boss.IsDead || !boss.gameObject.activeInHierarchy)
            {
                Say($"the enemy is gone before {sample.Form}");
                break;
            }
            // Beside it on whichever side has ground under it: the summit has edges.
            Vector3 from = boss.transform.position + Vector3.right * 3.2f;
            for (int side = 0; side < 8; side++)
            {
                Vector3 tried = boss.transform.position + Quaternion.Euler(0f, side * 45f, 0f) * (Vector3.zero - boss.transform.position).normalized * 3.2f;
                if (!Physics.Raycast(tried + Vector3.up * 2f, Vector3.down, out RaycastHit under, 4f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (Mathf.Abs(under.point.y - boss.transform.position.y) > 0.6f) continue;
                from = tried;
                break;
            }
            player.Teleport(from, Quaternion.LookRotation(boss.transform.position - from));
            yield return new WaitForSeconds(0.2f);
            float health = boss.Health.Current;
            bool castOk = Cast(sample);
            float limit = Time.time + 6f;
            yield return new WaitForSeconds(1.2f);
            if (sample.Form == "brand" || sample.Form == "clone" || sample.Form == "phalanx" || sample.Form == "orb" || sample.Form == "wall")
                yield return Tap(Key.K);
            while (SpellWorks.RunningOf(sample.Form) > 0 && Time.time < limit) yield return null;
            Say($"{sample.Form} at {boss.name}: cast {castOk}, it lost {health - boss.Health.Current:0}, it is {boss.Brain.State}, player at {player.transform.position.x:0.0},{player.transform.position.y:0.0},{player.transform.position.z:0.0}, hold {player.FormHold}, stops clicked through {stops}, errors so far {errors}");
        }
    }
}
