using System;
using System.Collections;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The standins merge at d31e11af, played in the Castle against a Temple Soldier: pieces the offline rounds could not run.
//   held    - Thunderclap's Held Thunder held on key 1: the slot gathers while held and is loosed once (PlayerSpells.HeldCasts);
//   mist    - Miasma's Low Mist poured on the soldier, then a dodge begun in it doses everyone in it (GroundsDosed);
//   rings   - Daybreak begun, three light blows landed, the HUD's count by its slot reads "3 of 8 rings";
//   edge    - Returning Edge's Through the Line: the blade waits 8 m out; a dodge calls it home through the soldier between;
//   ring    - The Closed Ring: Earth's surge raises the stones; the soldier driven outward is stopped at the rim (StoppedAtStones).
// One still each: review/shots/current/merge/<piece>.png.   bash AgentScripts/runprobe.sh Castle MergeProbe 90
public static class MergeProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Merge Probe").AddComponent<MergeProbeRunner>();
        return "probe started";
    }
}

public class MergeProbeRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/merge";
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerCombat combat;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerBuild build;
    private PlayerElement element;
    private Enemy soldier;
    private Enemy[] everyone;
    private int ok, failed;
    private bool healEnemies = true;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private static readonly Vector3 SoldierSpot = new Vector3(-3f, 0f, 40f);

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Judge(string what, bool good, string detail)
    {
        if (good) ok++; else failed++;
        Say($"{(good ? "OK" : "FAIL")} {what}: {detail}");
    }

    private static IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private static IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private static IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    private static IEnumerator Still(string name)
    {
        float scale = Time.timeScale;
        Time.timeScale = 0f;
        yield return null;
        ScreenCapture.CaptureScreenshot($"{Folder}/{name}.png");
        yield return null;
        yield return null;
        Time.timeScale = scale > 0f ? scale : 1f;
    }

    private void Update()
    {
        if (defence != null) defence.Health.Heal(1000f);
        if (player != null) player.Stamina.Fill();
        if (everyone == null || !healEnemies) return;
        foreach (var enemy in everyone)
            if (enemy != null && !enemy.IsDead) enemy.Health.Heal(1000f);
    }

    private IEnumerator FaceSoldier(float gap)
    {
        Keys();
        soldier.Brain.Reset();
        soldier.transform.SetPositionAndRotation(SoldierSpot, Quaternion.Euler(0f, 180f, 0f));
        player.Teleport(SoldierSpot + Vector3.back * gap, Quaternion.identity);
        defence.Restore();
        yield return Real(0.4f);
    }

    private IEnumerator WhenFree()
    {
        yield return Until(() => !element.IsCasting && !player.Dodge.IsRolling, 3f);
        yield return Real(0.2f);
    }

    private void Ready(string elementName, string slotted, params string[] marks)
    {
        build.Attune(elementName, "");
        foreach (var id in marks) upgrades.Give(Upgrade.ById(id));
        upgrades.RefreshNow();
        if (slotted != null) spells.SetSlot(0, slotted);
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(1000f);
    }

    private void Forget(params string[] marks)
    {
        spells.SetSlot(0, "");
        foreach (var id in marks) RunState.Unlocked.Remove(id);
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
        Directory.CreateDirectory(Folder);
        float waited = Time.unscaledTime + 5f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        int presses = 0;
        while (GuidePage.Showing && presses++ < 20) yield return Tap(Key.Escape);
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && presses++ < 40) yield return Tap(Key.E);
        int frame = Time.frameCount;
        float time = Time.time;
        yield return Real(0.4f);
        Say($"guide {(GuidePage.Showing ? "STILL OPEN" : "closed")}; time runs: {Time.time > time} ({Time.frameCount - frame} frames)");

        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        combat = player.GetComponent<PlayerCombat>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        element = player.GetComponent<PlayerElement>();
        everyone = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
        soldier = everyone.Where(e => e.Kind.Name == EnemyKind.SoldierName).OrderBy(e => (e.transform.position - SoldierSpot).sqrMagnitude).FirstOrDefault();
        Say($"{everyone.Length} enemies, soldier {(soldier != null ? soldier.transform.position.ToString("0.0") : "MISSING")}");
        if (soldier == null) { Finish(); yield break; }
        // Every enemy stands: the pieces are about what the player's spells do to one still soldier.
        foreach (var enemy in everyone) { enemy.Brain.AggroRange = 0f; enemy.Brain.Reset(); }

        string only = File.Exists("AgentScripts/merge_only.txt") ? File.ReadAllText("AgentScripts/merge_only.txt").Trim() : "";
        bool Wants(string piece) => only.Length == 0 || only.Split(',').Any(p => p.Trim() == piece);
        if (Wants("held")) yield return Run("held", Held());
        if (Wants("mist")) yield return Run("mist", Mist());
        if (Wants("rings")) yield return Run("rings", Rings());
        if (Wants("edge")) yield return Run("edge", Edge());
        if (Wants("ring")) yield return Run("ring", Ring());
        Finish();
    }

    // One piece; an exception inside it is a finding, not the end of the probe.
    private IEnumerator Run(string piece, IEnumerator body)
    {
        while (true)
        {
            object current;
            try
            {
                if (!body.MoveNext()) break;
                current = body.Current;
            }
            catch (Exception error)
            {
                Judge(piece, false, "threw " + error.GetType().Name + ": " + error.Message);
                break;
            }
            yield return current;
        }
        healEnemies = true;
        Keys();
        yield return WhenFree();
    }

    // ---- Held Thunder on key 1: gathers while held (nothing cast yet), loosed once at the full second.
    private IEnumerator Held()
    {
        string shape = SpellBranch.ShapeId(LightningTree.Thunderclap);
        Ready(Elements.Lightning, LightningTree.Thunderclap, shape);
        build.ChooseShape(LightningTree.Thunderclap, 1, true);
        upgrades.RefreshNow();
        string worn = SpellBranches.Worn(Upgrade.ById(shape), build.Current)?.Name;
        yield return FaceSoldier(5f);
        yield return WhenFree();
        int casts = spells.Casts, held = spells.HeldCasts;
        Keys(Key.Digit1);
        yield return Real(0.5f);
        bool gatheringMid = spells.Gathering == 0;
        int castsMid = spells.Casts;
        yield return Until(() => spells.HeldCasts > held, 1.5f);
        yield return Real(0.08f);
        yield return Still("held");
        Keys();
        yield return Real(0.6f);
        Judge("held", worn == "Held Thunder" && gatheringMid && castsMid == casts && spells.HeldCasts == held + 1 && spells.Casts == casts + 1,
            $"worn '{worn}'; at 0.5 s held: gathering slot {(gatheringMid ? "0" : "none")}, casts {casts}->{castsMid}; loosed: held casts {held}->{spells.HeldCasts}, casts ->{spells.Casts}");
        Forget(shape);
    }

    // ---- Low Mist: the sheet poured over the soldier; a dodge begun in it doses everyone standing in it.
    private IEnumerator Mist()
    {
        string shape = SpellBranch.ShapeId(PoisonTree.Miasma);
        Ready(Elements.Poison, PoisonTree.Miasma, shape);
        build.ChooseShape(PoisonTree.Miasma, 0, true);
        upgrades.RefreshNow();
        string worn = SpellBranches.Worn(Upgrade.ById(shape), build.Current)?.Name;
        soldier.Afflictions.Clear();
        yield return FaceSoldier(3f);
        yield return WhenFree();
        int casts = spells.Casts;
        yield return Tap(Key.Digit1);
        yield return Until(() => spells.Casts > casts, 1f);
        yield return Real(0.5f);
        int dosedBefore = upgrades.GroundsDosed, stacksBefore = soldier.Afflictions.Stacks(AfflictionKind.Poison);
        yield return Tap(Key.Space);
        yield return Real(0.25f);
        int dosed = upgrades.GroundsDosed, stacks = soldier.Afflictions.Stacks(AfflictionKind.Poison);
        yield return Still("mist");
        yield return Real(0.6f);
        Judge("mist", worn == "Low Mist" && spells.Casts > casts && dosed == dosedBefore + 1 && stacks > stacksBefore,
            $"worn '{worn}', cast {spells.Casts > casts}; the dodge in the sheet: grounds dosed {dosedBefore}->{dosed}, soldier's Poison {stacksBefore}->{stacks}");
        Forget(shape);
    }

    // ---- Daybreak: begun, three light blows landed, the HUD's count by the West slot.
    private IEnumerator Rings()
    {
        Ready(Elements.Light, LightTree.Daybreak, LightTree.Daybreak);
        yield return FaceSoldier(1.7f);
        yield return WhenFree();
        yield return Tap(Key.Digit1);
        yield return Until(() => upgrades.Gathering, 1f);
        bool begun = upgrades.Gathering;
        for (int blow = 0; blow < 6 && upgrades.RingsHeld < 3; blow++)
        {
            int before = upgrades.RingsHeld;
            yield return Tap(Key.J);
            yield return Until(() => upgrades.RingsHeld > before, 0.9f);
            yield return Real(0.35f);
        }
        yield return Real(0.2f);
        string shown = HudCount(0);
        int rings = upgrades.RingsHeld, most = upgrades.RingsMost;
        yield return Still("rings");
        int loosed = upgrades.RingsLoosed;
        combat.Mana.Gain(1000f);
        yield return Tap(Key.Digit1);
        yield return Until(() => !upgrades.Gathering, 1f);
        yield return Real(0.5f);
        Judge("rings", begun && rings == 3 && most == 8 && shown == "3 of 8 rings" && upgrades.RingsLoosed == loosed + 1 && HudCount(0) == "",
            $"begun {begun}; after the blows {rings} of {most}, HUD West count '{shown}'; second press loosed {upgrades.RingsLoosed - loosed}, count after '{HudCount(0)}'");
        Forget(LightTree.Daybreak);
    }

    private static string HudCount(int slot)
    {
        var hud = FindAnyObjectByType<SpellDiamondHud>();
        if (hud == null) return "<no hud>";
        var slotsField = typeof(SpellDiamondHud).GetField("slots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var array = slotsField?.GetValue(hud) as Array;
        var button = array?.GetValue(slot);
        if (button == null) return "<no slot>";
        var count = button.GetType().GetField("Count")?.GetValue(button);
        return count?.GetType().GetProperty("text")?.GetValue(count) as string ?? "<no text>";
    }

    // ---- Returning Edge, Through the Line: out 8 m past the soldier, waiting; a dodge calls it home through him.
    private IEnumerator Edge()
    {
        string shape = SpellBranch.ShapeId(MetalTree.ReturningEdge);
        Ready(Elements.Metal, MetalTree.ReturningEdge, MetalTree.ReturningEdge, shape);
        build.ChooseShape(MetalTree.ReturningEdge, 1, true);
        upgrades.RefreshNow();
        string worn = SpellBranches.Worn(Upgrade.ById(shape), build.Current)?.Name;
        yield return FaceSoldier(4f);
        yield return WhenFree();
        int casts = spells.Casts;
        yield return Tap(Key.Digit1);
        yield return Until(() => spells.Casts > casts, 1f);
        // Out 8 m at 16 m/s: it waits there from about half a second on.
        yield return Real(1.0f);
        healEnemies = false;
        float before = soldier.Health.Current;
        Vector3 playerBefore = player.transform.position;
        yield return Tap(Key.Space);
        yield return Real(0.15f);
        float after = soldier.Health.Current;
        yield return Still("edge");
        yield return Real(1.2f);
        healEnemies = true;
        Judge("edge", worn == "Through the Line" && spells.Casts > casts && before - after >= 15f,
            $"worn '{worn}', cast {spells.Casts > casts}; the dodge called it home: soldier {before:0}->{after:0} ({before - after:0} taken), player moved {(player.transform.position - playerBefore).magnitude:0.0} m");
        Forget(MetalTree.ReturningEdge, shape);
    }

    // ---- The Closed Ring: Bedrock's surge raises the stones round the player; the soldier driven out is stopped at the rim.
    private IEnumerator Ring()
    {
        Ready(Elements.Earth, null, EarthTree.TheClosedRing);
        element.WearSurge(Elements.Earth);
        upgrades.RefreshNow();
        yield return FaceSoldier(4f);
        yield return WhenFree();
        combat.Mana.Gain(1000f);
        Vector3 middle = player.transform.position;
        int stoppedBefore = upgrades.StoppedAtStones;
        yield return Tap(Key.F);
        yield return Until(() => element.SurgeActive, 1.5f);
        bool surging = element.SurgeActive;
        yield return Real(0.4f);
        Vector3 away = soldier.transform.position - middle;
        away.y = 0f;
        away.Normalize();
        float startFrom = Vector3.Distance(Flat(soldier.transform.position), Flat(middle));
        soldier.Push(away * 25f);
        yield return Real(1.0f);
        float endFrom = Vector3.Distance(Flat(soldier.transform.position), Flat(middle));
        yield return Still("ring");
        Judge("ring", surging && upgrades.StoppedAtStones > stoppedBefore && endFrom <= 6.4f,
            $"surge {surging} ({element.SurgeId}); soldier pushed out from {startFrom:0.0} m, ended {endFrom:0.0} m from the middle (ring 6 m); stopped at stones {stoppedBefore}->{upgrades.StoppedAtStones}");
        yield return Until(() => !element.SurgeActive, 12f);
        Forget(EarthTree.TheClosedRing);
    }

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    private void Finish()
    {
        Keys();
        if (spells != null) spells.SetSlot(0, "");
        if (build != null) build.Attune(Elements.Fire, "");
        if (upgrades != null) upgrades.RefreshNow();
        if (defence != null) defence.Afflictions.Clear();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Time.timeScale = 1f;
        Say($"tally: {ok} OK, {failed} FAIL");
        Say("done");
        Destroy(gameObject);
    }
}
