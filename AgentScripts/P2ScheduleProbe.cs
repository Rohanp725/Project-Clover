using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Build;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Phase 2, task 2.4: the Hand's slot schedule outside the playtest, played for real. In Sun City, with Fire and four
// spells (its own and three learned): West and North are filled and East and South are shut, also in the Hand at a
// rest fire (refused there); the Champion of the Gate is killed, which sets elite.champion_gate, and East opens and
// fills. Then through the Gate of Noon into the Sun Court: the man and the Sun-Bearer are felled, which sets god.fire,
// and South opens and fills. Progress stays in the sandbox (runprobe.sh sets it). Frame: review/shots/current/p2_hand_shut.png.
// Every judged line ends OK or WRONG. bash AgentScripts/runprobe.sh SunCity P2ScheduleProbe 120
public static class P2ScheduleProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("P2 Schedule Probe").AddComponent<P2ScheduleProbeRunner>();
        return "probe started";
    }
}

public class P2ScheduleProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerSpells spells;
    private PlayerBuild build;
    private PlayerUpgrades upgrades;
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

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        upgrades = player.GetComponent<PlayerUpgrades>();
    }

    private string SlotsText() => $"{Show(0)}, {Show(1)}, {Show(2)}, {Show(3)} [open {spells.Slots.OpenCount}, schedule {build.Open.Slots}, hand set {RunState.HandSet}, in force {string.Join("/", upgrades.SpellsInForce)}, carried {(player.GetComponent<PlayerElement>().HasElement ? player.GetComponent<PlayerElement>().Info.Name : "-")}]";
    private string Show(int slot) => string.IsNullOrEmpty(spells.Slots[slot]) ? "-" : spells.Slots[slot];

    // Reads any words said on arriving through to their end, so the reading box does not hold the player.
    private IEnumerator ReadAll()
    {
        for (int i = 0; i < 40 && ReadingBox.Instance != null && ReadingBox.Instance.IsOpen; i++) yield return Tap(Key.E);
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return Real(1.5f);
        yield return ReadAll();
        FindPlayer();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest {PlaytestMode.On}, sandboxed {Progress.Sandboxed}");
        Judge("outside the playtest and sandboxed", !PlaytestMode.On && Progress.Sandboxed);
        Judge("no flag set yet", !Progress.Has(SlotSchedule.ChampionFlag) && !Progress.Has(SlotSchedule.FireGodFlag));

        // 1. Fire and four spells: only West and North fill.
        build.Attune(Elements.Fire, "");
        foreach (string id in new[] { Clover.Player.Trees.FireTree.Cinder, Clover.Player.Trees.FireTree.WildfireRush, "fire.kiln", "fire.meteor" })
            upgrades.Give(Upgrade.ById(id));
        upgrades.RefreshNow();
        yield return Real(0.5f);
        Judge("at the start two slots are open", build.UseSchedule && build.Open.Slots == 2 && spells.Slots.OpenCount == 2, $"open {build.Open.Slots}");
        Judge("West and North are filled, East and South stay empty", spells.Slots[0] != "" && spells.Slots[1] != "" && spells.Slots[2] == "" && spells.Slots[3] == "", SlotsText());

        // 2. At a rest fire the Hand shows East and South shut and will not open them.
        var fire = FindObjectsByType<RestPoint>(FindObjectsSortMode.None).OrderBy(point => point.transform.position.z).First();
        player.Teleport(fire.transform.position + new Vector3(0f, 0.1f, -1.5f), Quaternion.identity);
        yield return Real(0.5f);
        yield return Tap(Key.E);
        yield return Real(0.3f);
        yield return ReadAll();
        Judge("the fire opens the rest menu", RestMenu.Showing);
        yield return Tap(Key.E);
        yield return Real(0.2f);
        var hand = HandMenu.Instance;
        Judge("the Hand opens", HandMenu.Showing);
        yield return Tap(Key.S);
        yield return Tap(Key.S);
        Judge("the cursor is on East", hand != null && hand.Item == 2, $"item {hand?.Item}");
        yield return Real(0.2f);
        ScreenCapture.CaptureScreenshot("review/shots/current/p2_hand_shut.png");
        yield return Real(0.2f);
        yield return Tap(Key.E);
        Judge("a shut slot cannot be chosen for", hand != null && !hand.Choosing && spells.Slots[2] == "");
        yield return Tap(Key.Space);
        yield return Real(0.2f);
        yield return Tap(Key.Space);
        yield return Until(() => !RestMenu.Showing && Time.timeScale > 0f, 2f);
        yield return Real(0.3f);
        Judge("the fire is left and the world runs", !RestMenu.Showing && !HandMenu.Showing && Time.timeScale > 0f && !player.Suspended);

        // 3. The Champion of the Gate falls: elite.champion_gate is set and East opens.
        var director = FindAnyObjectByType<AreaDirector>();
        var champion = director.Enemies.FirstOrDefault(enemy => enemy != null && enemy.Kind != null && enemy.Kind.Name == EnemyKind.ChampionName);
        Judge("the Champion of the Gate is found", champion != null);
        for (int blow = 0; blow < 200 && champion != null && !champion.IsDead; blow++) champion.TakeHit(new Hit(80f, 0f, Vector3.forward, true, true));
        yield return Real(0.5f);
        Judge("his fall sets elite.champion_gate", Progress.Has(SlotSchedule.ChampionFlag));
        Judge("three slots are open and East fills", build.Open.Slots == 3 && spells.Slots.OpenCount == 3 && spells.Slots[2] != "" && spells.Slots[3] == "", SlotsText());

        // 4. Through the gate into the Sun Court; the man and then the Fire god are felled.
        var gate = FindAnyObjectByType<GateOfNoon>();
        player.Teleport(gate.transform.position + new Vector3(0f, 0.1f, -1.2f), Quaternion.identity);
        yield return Real(0.5f);
        yield return Tap(Key.E);
        yield return Until(() => SceneManager.GetActiveScene().name == "SunCourt", 15f);
        yield return Real(1.5f);
        yield return ReadAll();
        FindPlayer();
        Judge("the gate leads to the Sun Court, the flag kept and East still open", SceneManager.GetActiveScene().name == "SunCourt" && build.Open.Slots == 3 && spells.Slots[2] != "", SlotsText());
        var court = FindAnyObjectByType<SunCourtDirector>();
        var form = FindAnyObjectByType<SunBearer>();
        var man = FindAnyObjectByType<AreaDirector>().Enemies[0];
        form.OnlyWhenAsked = true;
        for (int blow = 0; blow < 200 && !man.IsDead; blow++) man.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        yield return Until(() => form.HasRisen, 10f);
        yield return Until(() => form.Doing != "Entering", 10f);
        yield return ReadAll();
        for (int attempt = 0; attempt < 12 && !form.IsDead; attempt++)
        {
            form.ForceBreak();
            yield return new WaitForSeconds(0.6f);
            for (int blow = 0; blow < 40 && !form.IsDead; blow++) form.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
            yield return new WaitForSeconds(0.3f);
        }
        yield return Until(() => court.Felled, 12f);
        yield return Real(0.5f);
        Judge("the Fire god falls and sets god.fire", court.Felled && Progress.Has(SlotSchedule.FireGodFlag), $"felled {court.Felled}, gods {RunState.GodsFelled}");
        Judge("all four slots are open and South fills", build.Open.Slots == 4 && spells.Slots.OpenCount == 4 && spells.Slots[3] != "", SlotsText());

        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
