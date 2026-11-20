using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Magic;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Phase 4, tasks 4.1 to 4.3, in the Proving Ground with the playtest on: one spell branch of Fire (Flame Wave, the base
// spell) and one of Lightning (Thunderclap) learned mark by mark (Learn given with the element, then Empower, Shape and
// Master), the shape swapped at a rest fire with PlayerBuild.ChooseShape (refused away from one), each cast through the
// keys, and the clocks read: the table's wait, the Empower's shorter one, a shape keeping its branch's wait, and each
// Master's timing (Flame Wave: cast within half a second of the charged blade's third blow and no clock starts, later
// and it does; Thunderclap: a perfect guard ends its wait and makes the next cast start none). The chart is filmed on
// both trees. Frames: review/shots/current/branch_*.png. Every judged line ends OK or WRONG.
// bash AgentScripts/runprobe.sh ProvingGround BranchProbe 150
public static class BranchProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Branch Probe").AddComponent<BranchProbeRunner>();
        return "probe started";
    }
}

public class BranchProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerBuild build;
    private int ok, wrong;

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

    private IEnumerator Use(Interactable thing, float back = 0.8f)
    {
        Vector3 at = thing.transform.position + Vector3.back * back;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
        yield return Real(0.35f);
        yield return Tap(Key.E);
        yield return Real(0.2f);
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private static T Find<T>(Func<T, bool> which) where T : UnityEngine.Object =>
        FindObjectsByType<T>(FindObjectsInactive.Include).FirstOrDefault(which);

    private IEnumerator WhenFree()
    {
        yield return Until(() => !combat.IsAttacking && !element.IsCasting && !player.Dodge.IsRolling && player.FormHold == 0 && !player.IsLeaping && !defence.IsDrinking, 6f);
        yield return Real(0.15f);
    }

    // An altar used until its element is the only one carried, as a tester would (at most three uses).
    private IEnumerator HoldAlone(LoadoutChoice altar)
    {
        for (int use = 0; use < 4; use++)
        {
            if (element.HasElement && element.Info != null && element.Info.Name == altar.Choice && !element.Mixed && !element.AllIn) break;
            yield return Use(altar, 1.0f);
        }
    }

    private string SlotsText() => string.Join(", ", Enumerable.Range(0, 4).Select(i => $"{i}:{spells.Slots[i]}"));

    // In front of the still dummy, so a cast has something to strike.
    private TrainingDummy still;
    private void StandAtDummy() => player.Teleport(still.transform.position + Vector3.back * 2.2f, Quaternion.identity);

    // Casts slot 0 with the key 1 from a clear clock, and says how long its clock is.
    private IEnumerator CastWest(string what)
    {
        combat.Mana.Gain(100f);
        StandAtDummy();
        yield return WhenFree();
        spells.Cooldowns.ResetAll();
        int casts = spells.Casts;
        yield return Tap(Key.Digit1);
        yield return Real(0.1f);
        lastCast = spells.Casts == casts + 1 && spells.LastSlot == 0;
        lastLength = spells.Cooldowns.Length(0);
        lastLeft = spells.Cooldowns.Remaining(0);
        Say($"  {what}: cast {lastCast}, clock {lastLeft:0.00} of {lastLength:0.00}s, form '{SpellBranches.FormOf(Upgrade.ById(spells.Slots[0]), RunState.Unlocked, build.Current)}', through mark {upgrades.BaseCastsThroughMark(spells.Slots[0])}");
        yield return WhenFree();
    }

    private bool lastCast;
    private float lastLength, lastLeft;

    private IEnumerator Rest()
    {
        var rest = Find<RestPoint>(point => true);
        yield return Use(rest, 1.4f);
        yield return Until(() => RestMenu.Showing, 1.5f);
    }

    private IEnumerator LeaveRest()
    {
        float until = Time.unscaledTime + 1.5f;
        while (RestMenu.Showing && RestMenu.Instance.Away && Time.unscaledTime < until) yield return null;
        if (!RestMenu.Showing) yield break;
        yield return null;
        yield return Tap(Key.Space);
        until = Time.unscaledTime + 1f;
        while ((RestMenu.Showing || Time.timeScale == 0f) && Time.unscaledTime < until) yield return null;
        yield return null;
    }

    private IEnumerator ChartFromRest()
    {
        if (!RestMenu.Showing || RestMenu.Instance.Away) yield break;
        var restMenu = RestMenu.Instance;
        int chartAt = restMenu.Choices.IndexOf(RestEntry.Chart);
        for (int step = 0; step < 8 && restMenu.Choices.Selected != chartAt; step++) yield return Tap(Key.S);
        yield return Tap(Key.E);
        yield return Until(() => UpgradeMenu.Instance != null && UpgradeMenu.Instance.IsOpen, 1f);
    }

    private bool Buy(string id)
    {
        var mark = Upgrade.ById(id);
        bool can = upgrades.CanBuy(mark);
        bool bought = upgrades.TryBuy(mark);
        Say($"  buy {id} ({mark?.Name}, {mark?.Cost} shards): {(bought ? "bought" : "refused")}, shards left {RunState.Points}");
        return bought;
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest mode {PlaytestMode.On}, sandboxed {Progress.Sandboxed}");
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        still = Find<TrainingDummy>(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy and the player's build are found", still != null && build != null);

        var katana = Find<LoadoutChoice>(choice => !choice.IsElement && choice.Choice == Weapons.KatanaName);
        yield return Use(katana, 1.0f);
        var fireAltar = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Fire);
        yield return HoldAlone(fireAltar);
        Judge("Fire alone and the Katana in hand", element.Kit.Name == Elements.Fire && !element.Mixed && combat.Weapon.Name == Weapons.KatanaName, $"{element.Kit.Name}, {combat.Weapon.Name}");
        RunState.Points = Math.Max(RunState.Points, 60);
        yield return Branch(Elements.Fire, "fire.base", 7f, 5.5f, FireMaster);

        var lightningAltar = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Lightning);
        yield return HoldAlone(lightningAltar);
        Judge("Lightning alone", element.Kit.Name == Elements.Lightning && !element.Mixed, $"{element.Kit.Name}, mixed {element.Mixed}");
        RunState.Points = Math.Max(RunState.Points, 60);
        yield return Branch(Elements.Lightning, "lightning.base", 9f, 7f, LightningMaster);

        // The chart, from a rest fire, on each tree with its base spell's Shape chosen.
        yield return Rest();
        yield return ChartFromRest();
        var chart = UpgradeMenu.Instance;
        Judge("the chart opens from the rest fire", chart != null && chart.IsOpen);
        if (chart != null && chart.IsOpen)
        {
            chart.View(Elements.Fire);
            chart.Choose("fire.base.shape");
            yield return Real(0.8f);
            yield return Shot("branch_chart_fire");
            Say($"  chart on Fire: chosen {chart.Chosen?.Name}, {chart.MarkCount} marks drawn");
            chart.View(Elements.Lightning);
            chart.Choose("lightning.base.shape");
            yield return Real(0.8f);
            yield return Shot("branch_chart_lightning");
            Say($"  chart on Lightning: chosen {chart.Chosen?.Name}");
            chart.ToggleWide();
            yield return Real(0.8f);
            yield return Shot("branch_chart_wide");
            chart.ToggleWide();
            yield return Tap(Key.Space);
        }
        yield return LeaveRest();

        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }

    // One base spell's branch, Learn to Master, with the clocks read at each mark.
    private IEnumerator Branch(string elementName, string branch, float table, float empowered, Func<string, IEnumerator> master)
    {
        string empower = SpellBranch.EmpowerId(branch), shape = SpellBranch.ShapeId(branch), masterId = SpellBranch.MasterId(branch);
        Say($"--- {elementName}: {Upgrade.ById(branch)?.Name} ({branch})");
        if (spells.Slots[0] != branch) spells.SetSlot(0, branch);
        Judge($"{branch}'s Learn is owned with its element and never sold", upgrades.Owns(branch) && !upgrades.CanBuy(Upgrade.ById(branch)), SlotsText());
        Judge($"the table's wait is {table}s", Mathf.Approximately(SpellTimings.For(branch).Cooldown, table), $"{SpellTimings.For(branch).Cooldown}");

        yield return CastWest("Learn only");
        Judge($"Learn: the base spell casts by its kit and waits {table}s", lastCast && Mathf.Abs(lastLength - table) < 0.05f && !upgrades.BaseCastsThroughMark(branch), $"{lastLength:0.00}s");

        Judge("a Shape is not sold before its Empower", !upgrades.CanBuy(Upgrade.ById(shape)));
        Judge("the Empower is bought", Buy(empower) && upgrades.Has(empower));
        yield return CastWest("Empowered");
        Judge($"Empower: the wait is {empowered}s", lastCast && Mathf.Abs(lastLength - empowered) < 0.05f, $"{lastLength:0.00}s");

        Judge("a Master is not sold before its Shape", !upgrades.CanBuy(Upgrade.ById(masterId)));
        Judge("the Shape is bought", Buy(shape) && upgrades.Has(shape));
        var shapeMark = Upgrade.ById(shape);
        string first = shapeMark.Shapes[0].Name, second = shapeMark.Shapes[1].Name;
        yield return CastWest($"shape 0 ({first})");
        Judge($"Shape: {first} is worn by default, cast through the mark in its own form, and keeps the Empower's wait",
            lastCast && upgrades.BaseCastsThroughMark(branch) && SpellBranches.FormOf(Upgrade.ById(branch), RunState.Unlocked, build.Current) == shapeMark.Shapes[0].Form
            && Mathf.Abs(lastLength - empowered) < 0.05f, $"{lastLength:0.00}s");

        // Away from a fire the shape cannot change; at a rest fire it can.
        int refusedBefore = build.HandsRefused;
        Judge("away from a rest fire the shape is not changed", !build.ChooseShape(branch, 1, RestMenu.Showing) && build.HandsRefused == refusedBefore + 1
            && SpellBranches.Worn(shapeMark, build.Current) == shapeMark.Shapes[0]);
        yield return Rest();
        Judge("the rest menu is up at the fire", RestMenu.Showing);
        Judge($"at the rest fire {second} is chosen", build.ChooseShape(branch, 1, RestMenu.Showing) && SpellBranches.Worn(shapeMark, build.Current) == shapeMark.Shapes[1]
            && RunState.Shapes.Any(choice => choice.Id == branch && choice.Choice == 1), string.Join(", ", RunState.Shapes.Select(choice => $"{choice.Id}={choice.Choice}")));
        yield return Shot($"branch_{elementName.ToLowerInvariant()}_rest");
        yield return LeaveRest();
        yield return CastWest($"shape 1 ({second})");
        Judge($"{second} casts in its own form with the same wait", lastCast && SpellBranches.FormOf(Upgrade.ById(branch), RunState.Unlocked, build.Current) == shapeMark.Shapes[1].Form
            && Mathf.Abs(lastLength - empowered) < 0.05f, $"form {shapeMark.Shapes[1].Form}, {lastLength:0.00}s");
        yield return Shot($"branch_{elementName.ToLowerInvariant()}_shape");

        Judge("the Master is bought", Buy(masterId) && upgrades.Has(masterId));
        yield return master(branch);

        // The branch's marks hold only while its spell is slotted. In play an emptied slot fills itself again until a
        // Hand is set at a rest fire (Phase 1's stopgap, PlayerSpells.Look), so the rule is asked of the player's own
        // loadout with the spell taken out.
        var without = build.Current.Clone();
        for (int i = 0; i < without.Slots.Length; i++) if (without.Slots[i] == branch) without.Slots[i] = "";
        bool outOfForce = !MarkRules.InForce(Upgrade.ById(empower), without) && !MarkRules.InForce(shapeMark, without) && !MarkRules.InForce(Upgrade.ById(masterId), without);
        bool inForce = MarkRules.InForce(Upgrade.ById(empower), build.Current) && MarkRules.InForce(shapeMark, build.Current) && MarkRules.InForce(Upgrade.ById(masterId), build.Current);
        Judge("unslotted, the Empower, Shape and Master are out of force; slotted, in", outOfForce && inForce && upgrades.Has(empower) && upgrades.Has(shape) && upgrades.Has(masterId), SlotsText());
        yield return null;
    }

    // Flame Wave's Master: cast in the same instant as the charged blade's third blow lands (a 0.5 s window) and the
    // clock does not start. The blow is reported to the marks as the charged blade does (OnChargedBlow).
    private IEnumerator FireMaster(string branch)
    {
        combat.Mana.Gain(100f);
        StandAtDummy();
        yield return WhenFree();
        spells.Cooldowns.ResetAll();
        upgrades.OnChargedBlow(still, true);
        yield return null;
        Judge("the third charged blow promises the next Flame Wave", spells.Cooldowns.IsInstant(0));
        int casts = spells.Casts;
        yield return Tap(Key.Digit1);
        yield return Real(0.05f);
        Judge("cast within the window, no clock starts", spells.Casts == casts + 1 && spells.Cooldowns.Ready(0) && !spells.Cooldowns.IsInstant(0), $"left {spells.Cooldowns.Remaining(0):0.00}s");
        yield return WhenFree();

        spells.Cooldowns.ResetAll();
        upgrades.OnChargedBlow(still, true);
        yield return Real(0.8f);
        Judge("the promise lapses after half a second", !spells.Cooldowns.IsInstant(0));
        casts = spells.Casts;
        yield return Tap(Key.Digit1);
        yield return Real(0.05f);
        Judge("cast late, the clock starts at the Empower's 5.5 s", spells.Casts == casts + 1 && Mathf.Abs(spells.Cooldowns.Length(0) - 5.5f) < 0.05f, $"{spells.Cooldowns.Remaining(0):0.00} of {spells.Cooldowns.Length(0):0.00}s");
        yield return WhenFree();

        // With the clock running the promise does not end it: the spell had to be ready.
        float left = spells.Cooldowns.Remaining(0);
        upgrades.OnChargedBlow(still, true);
        yield return null;
        int refusals = spells.Refusals;
        casts = spells.Casts;
        yield return Tap(Key.Digit1);
        Judge("while its clock runs the third blow does not end the wait", spells.Casts == casts && spells.Refusals == refusals + 1 && spells.Cooldowns.Remaining(0) > 0f, $"{left:0.00}s -> {spells.Cooldowns.Remaining(0):0.00}s");
        yield return Real(0.6f);
    }

    // Thunderclap's Master: a perfect guard ends its wait and makes the next cast start no clock.
    private IEnumerator LightningMaster(string branch)
    {
        combat.Mana.Gain(100f);
        StandAtDummy();
        yield return WhenFree();
        spells.Cooldowns.ResetAll();
        yield return Tap(Key.Digit1);
        yield return WhenFree();
        float before = spells.Cooldowns.Remaining(0);
        int perfect = defence.PerfectGuards;
        Keys(Key.LeftShift);
        yield return null;
        yield return null;
        var outcome = defence.TakeHit(new Hit(10f, 10f, -player.transform.forward, false, false, player.transform.position + player.transform.forward * 1.5f));
        yield return null;
        Keys();
        yield return Real(0.15f);
        Judge("a perfect guard ends Thunderclap's wait and promises the next", outcome == HitOutcome.PerfectGuard && defence.PerfectGuards == perfect + 1
            && before > 1f && spells.Cooldowns.Ready(0) && spells.Cooldowns.IsInstant(0), $"{outcome}, {before:0.00}s -> {spells.Cooldowns.Remaining(0):0.00}s, instant {spells.Cooldowns.IsInstant(0)}");
        yield return WhenFree();
        int casts = spells.Casts;
        yield return Tap(Key.Digit1);
        yield return Real(0.1f);
        Judge("the next Thunderclap starts no clock", spells.Casts == casts + 1 && spells.Cooldowns.Ready(0) && !spells.Cooldowns.IsInstant(0), $"left {spells.Cooldowns.Remaining(0):0.00}s");
        yield return WhenFree();
        casts = spells.Casts;
        yield return Tap(Key.Digit1);
        yield return Real(0.1f);
        Judge("the one after waits the Empower's 7 s again", spells.Casts == casts + 1 && Mathf.Abs(spells.Cooldowns.Length(0) - 7f) < 0.05f, $"{spells.Cooldowns.Remaining(0):0.00} of {spells.Cooldowns.Length(0):0.00}s");
        yield return WhenFree();
    }
}
