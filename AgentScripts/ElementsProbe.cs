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

// Play Mode probe for the eleven elements' own moves. In the combat arena it gathers three dummies, and for each
// element: takes it, taps its spell at the group with the real button, photographs three moments of it
// (review/shots/current/element_<name>_tap_<n>.png), lights its surge and photographs that
// (element_<name>_surge.png), and writes what each dummy lost, what was laid on it and which way and how far it was
// shoved (dummies are posts: the shoves are summed, not travelled). In any other scene it casts Riptide, Gust, Grasp
// and Stone Rise at the nearest real enemy and measures how far that enemy was moved toward or away from the player.
// A file %TEMP%/elements_probe.txt may hold a comma list of element names to play only those, or "soak=<seconds>" to
// keep casting every tap and surge in turn for that long (for the profiler) instead of the survey.
//   bash AgentScripts/runprobe.sh CombatTest ElementsProbe 250
public static class ElementsProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Elements Probe").AddComponent<ElementsProbeRunner>();
        return "probe started";
    }
}

public class ElementsProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private readonly List<TrainingDummy> group = new List<TrainingDummy>();
    private int errors;
    private string firstError = "";
    private bool finished;
    private float soak;

    // Seconds after the spell leaves the hand at which each is photographed.
    private static readonly Dictionary<string, float[]> Shots = new Dictionary<string, float[]>
    {
        { "Fire", new[] { 0.12f, 0.4f, 1.6f } }, { "Lightning", new[] { 0.02f, 0.1f, 0.5f } }, { "Wind", new[] { 0.08f, 0.24f, 0.5f } },
        { "Ice", new[] { 0.14f, 0.45f, 1.6f } }, { "Water", new[] { 0.2f, 0.45f, 0.78f } }, { "Life", new[] { 0.3f, 1.05f, 3f } },
        { "Earth", new[] { 0.2f, 0.42f, 2f } }, { "Metal", new[] { 0.3f, 1.0f, 1.6f } }, { "Light", new[] { 0.04f, 0.2f, 1f } },
        { "Dark", new[] { 0.2f, 0.42f, 0.75f } }, { "Poison", new[] { 0.4f, 1.6f, 3.4f } },
    };

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

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
        string only = "";
        string config = Path.Combine(Path.GetTempPath(), "elements_probe.txt");
        if (File.Exists(config)) only = File.ReadAllText(config).Trim();
        if (only.StartsWith("soak="))
        {
            float.TryParse(only.Substring(5), out soak);
            only = "";
        }
        var names = Elements.All.Select(info => info.Name).Where(name => only.Length == 0 || only.Split(',').Contains(name)).ToList();
        Say($"start: {names.Count} elements; works {(FindAnyObjectByType<SpellWorks>() != null)} fx {(FindAnyObjectByType<FormFx>() != null)}");
        yield return new WaitForSeconds(0.3f);
        var dummies = FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None).Where(dummy => dummy.GetComponent<DummyAttacker>() == null).ToList();
        if (dummies.Count >= 3) yield return Yard(dummies, names);
        else yield return Elsewhere();
        Say($"errors logged {errors}{(errors > 0 ? ": " + firstError : "")}");
        finished = true;
        Say("done");
    }

    private IEnumerator Yard(List<TrainingDummy> dummies, List<string> names)
    {
        var spots = new[] { new Vector3(5.6f, 0f, -2f), new Vector3(6.9f, 0f, -1.1f), new Vector3(6.2f, 0f, -3.3f) };
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

        if (soak > 0f)
        {
            // For the profiler: every element's tap and surge in turn, one on top of another, with afflictions on the dummies.
            Say($"soak: every tap and surge in turn for {soak:0} s");
            float until = Time.time + soak;
            int cast = 0;
            while (Time.time < until)
            {
                player.Teleport(stand, Quaternion.LookRotation(toward));
                element.Take(ElementKit.ByName(names[cast % names.Count], false));
                combat.Mana.Gain(1000f);
                yield return Tap(Key.U);
                yield return new WaitForSeconds(0.35f);
                yield return Tap(Key.I);
                yield return new WaitForSeconds(0.45f);
                cast++;
            }
            Say($"soak: {cast} casts, runs {SpellWorks.Running}, solids {FormFx.SolidsShown}, marks {FormFx.MarksShown}");
            yield break;
        }

        foreach (string name in names)
        {
            yield return new WaitUntil(() => player.FormHold == 0);
            float waited = 0f;
            while (SpellWorks.Running > 0 && waited < 7f)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            player.Teleport(stand, Quaternion.LookRotation(toward));
            foreach (var dummy in group)
            {
                dummy.Afflictions.Clear();
                dummy.Stagger.Reset();
            }
            element.Take(ElementKit.ByName(name, false));
            combat.Mana.Gain(1000f);
            yield return new WaitForSeconds(0.4f);
            var before = group.Select(dummy => dummy.Health.Current).ToArray();
            var shoves = group.Select(dummy => dummy.Shoves).ToArray();
            var shoved = new Vector3[3];
            int castsBefore = element.SpellsCast;
            yield return Tap(Key.U);
            float waitedForCast = 0f;
            while (element.SpellsCast == castsBefore && waitedForCast < 1f)
            {
                waitedForCast += Time.deltaTime;
                yield return null;
            }
            float castAt = Time.time;
            float[] at = Shots[name];
            int peakSolids = 0, peakMarks = 0;
            bool struck = false;
            for (int shot = 0; shot < at.Length; shot++)
            {
                while (Time.time - castAt < at[shot])
                {
                    peakSolids = Mathf.Max(peakSolids, FormFx.SolidsShown);
                    peakMarks = Mathf.Max(peakMarks, FormFx.MarksShown);
                    for (int i = 0; i < 3; i++)
                        if (group[i].Shoves != shoves[i])
                        {
                            shoves[i] = group[i].Shoves;
                            shoved[i] += group[i].LastShove;
                        }
                    // Metal's blade is driven in, and Ice's crystals shattered, by a blow of the weapon.
                    if (!struck && ((name == "Metal" && Time.time - castAt >= 1.1f) || (name == "Ice" && Time.time - castAt >= 1.7f)))
                    {
                        struck = true;
                        player.Teleport(spots[0] - toward * 1.5f, Quaternion.LookRotation(toward));
                        yield return Tap(Key.K);
                    }
                    yield return null;
                }
                ScreenCapture.CaptureScreenshot($"review/shots/current/element_{name.ToLowerInvariant()}_tap_{shot + 1}.png");
                yield return null;
            }
            if (name == "Ice")
            {
                float until = Time.time + 1.2f;
                while (Time.time < until)
                {
                    if (!struck && Time.time - castAt >= 1.7f)
                    {
                        struck = true;
                        player.Teleport(spots[0] - toward * 1.5f, Quaternion.LookRotation(toward));
                        yield return Tap(Key.K);
                    }
                    yield return null;
                }
                ScreenCapture.CaptureScreenshot("review/shots/current/element_ice_tap_4_shattered.png");
            }
            yield return new WaitForSeconds(0.5f);
            string line = $"{name} tap '{element.Kit.SpellName}' cast {(element.SpellsCast > castsBefore)}: peak solids {peakSolids} marks {peakMarks};";
            for (int i = 0; i < 3; i++)
            {
                float alongToPlayer = Vector3.Dot(shoved[i], -toward);
                line += $" d{i} lost {before[i] - group[i].Health.Current:0.#} {Laid(group[i])} shoved {shoved[i].magnitude:0.00}m ({(shoved[i].magnitude < 0.01f ? "not" : alongToPlayer > 0f ? "toward player" : "away")} {alongToPlayer:0.00})";
            }
            Say(line);

            // The surge, seen on the body.
            player.Teleport(stand, Quaternion.LookRotation(toward));
            int surgesBefore = element.Surges;
            float health = defence.Health.Current;
            yield return Tap(Key.I);
            yield return new WaitForSeconds(0.7f);
            ScreenCapture.CaptureScreenshot($"review/shots/current/element_{name.ToLowerInvariant()}_surge.png");
            yield return null;
            yield return new WaitForSeconds(0.2f);
            Say($"{name} surge '{element.Kit.SurgeName}' lit {(element.Surges > surgesBefore)} active {element.Kit.SurgeActive} solids {FormFx.SolidsShown}");
        }
    }

    private IEnumerator Elsewhere()
    {
        foreach (string name in new[] { "Water", "Wind", "Dark", "Earth" })
        {
            var enemy = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Where(e => !e.IsDead && e.gameObject.activeInHierarchy)
                .OrderBy(e => (e.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
            if (enemy == null)
            {
                Say($"{name}: no enemy in this scene");
                continue;
            }
            // Four metres off it, facing it, from the side the player is already on.
            Vector3 away = player.transform.position - enemy.transform.position;
            away.y = 0f;
            away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.back;
            player.Teleport(enemy.transform.position + away * 4f, Quaternion.LookRotation(-away));
            element.Take(ElementKit.ByName(name, false));
            combat.Mana.Gain(1000f);
            defence.Health.Fill();
            yield return new WaitForSeconds(0.15f);
            float gapBefore = Vector3.Distance(enemy.transform.position, player.transform.position);
            float healthBefore = enemy.Health.Current;
            int castsBefore = element.SpellsCast;
            yield return Tap(Key.U);
            float waited = 0f;
            while (element.SpellsCast == castsBefore && waited < 1f)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            Vector3 castFrom = player.transform.position;
            float least = gapBefore, most = gapBefore;
            float until = Time.time + 1.3f;
            bool shot = false;
            while (Time.time < until)
            {
                float gap = Vector3.Distance(enemy.transform.position, castFrom);
                least = Mathf.Min(least, gap);
                most = Mathf.Max(most, gap);
                if (!shot && Time.time > until - 0.75f)
                {
                    shot = true;
                    ScreenCapture.CaptureScreenshot($"review/shots/current/element_{name.ToLowerInvariant()}_real.png");
                }
                yield return null;
            }
            Say($"{name} at a real enemy ({enemy.name}): cast {(element.SpellsCast > castsBefore)}, gap {gapBefore:0.00} m before, nearest {least:0.00}, farthest {most:0.00}, lost {healthBefore - enemy.Health.Current:0.#}, soaked {enemy.Afflictions.Stacks(AfflictionKind.Soaked)}");
            yield return new WaitForSeconds(1.5f);
        }
    }
}
