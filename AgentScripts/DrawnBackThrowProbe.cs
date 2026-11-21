using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;

// Drawn Back to Break's throw on an enemy that can be moved (the still dummy of Phase4bProbe cannot be): a soldier let out of
// the castle's pen in the Proving Ground. First a plain light blow with no wave held, to see how far a blow alone shoves it;
// then two dodges away bank two waves and the next light blow breaks them, and the soldier's slide is measured straight
// away from the player. The description promises "thrown 2 metres back". Frames review/shots/current/p4c_drawnback_*.png.
// bash AgentScripts/runprobe.sh ProvingGround DrawnBackThrowProbe 60
public static class DrawnBackThrowProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Drawn Back Throw Probe").AddComponent<DrawnBackThrowProbeRunner>();
        return "probe started";
    }
}

public class DrawnBackThrowProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerUpgrades upgrades;
    private PlayerBuild build;
    private Enemy soldier;
    private EnemyPen castle;
    private XInputController pad;
    private int ok, wrong;
    private bool playtestWas;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
    }

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (soldier != null && !soldier.IsDead && soldier.Health.Fraction < 0.6f) soldier.Health.Fill();
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

    private void Set(InputControl control, float value)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            ((InputControl<float>)control).WriteValueIntoEvent(value, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    private void Stick(Vector2 way)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            pad.leftStick.WriteValueIntoEvent(way, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    private IEnumerator Press(InputControl button)
    {
        Set(button, 1f);
        yield return null;
        yield return null;
        Set(button, 0f);
        yield return null;
    }

    private IEnumerator Tap(Key key)
    {
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(key));
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        yield return null;
        yield return null;
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
        // A guide opened as a list turns its pages on E and never closes on it: the back key closes it.
        if (GuidePage.Showing) yield return Tap(Key.Space);
        if (GuidePage.Showing) GuidePage.Instance.Close();
        if (Time.timeScale <= 0f) Say("WRONG: time still stopped after the guide");
    }

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // The player this far south of the soldier, facing it.
    private void StandOff(float metres)
    {
        Vector3 at = soldier.transform.position + Vector3.back * metres;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
    }

    // One light blow, then the furthest the soldier slid straight away from the player over the next 1.5 s.
    private IEnumerator Blow(string shot, Action<float, float> told)
    {
        // A light blow from the front would be turned on its guard; hold the guard open so the blow lands.
        soldier.GuardOpenFor = 2f;
        Vector3 from = Flat(soldier.transform.position);
        Vector3 away = (from - Flat(player.transform.position)).normalized;
        float healthWas = soldier.Health.Current;
        string before = soldier.Brain.State.ToString();
        yield return Press(pad.rightShoulder);
        // What it was doing when struck: a soldier mid-lunge drives against the throw and falls short, as the owner allows.
        Say($"{shot}: the soldier was {before} as the blow went in, {soldier.Brain.State} after");
        float most = 0f, end = Time.unscaledTime + 1.5f, gameStart = Time.time;
        bool shotTaken = false;
        while (Time.unscaledTime < end)
        {
            float along = Vector3.Dot(Flat(soldier.transform.position) - from, away);
            most = Mathf.Max(most, along);
            if (!shotTaken && Time.unscaledTime > end - 1.25f) { shotTaken = true; yield return Shot(shot); }
            yield return null;
        }
        Say($"{shot}: game time ran {Time.time - gameStart:0.00} s, the soldier lost {healthWas - soldier.Health.Current:0.0} (health refilled below 60%)");
        told(most, Vector3.Distance(Flat(soldier.transform.position), from));
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
        Judge("progress is sandboxed", Progress.Sandboxed);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        int frame = Time.frameCount;
        float gameTime = Time.time;
        yield return Real(0.5f);
        Judge("time runs after the guide", Time.time > gameTime + 0.3f && Time.frameCount > frame, $"time scale {Time.timeScale:0.00}");

        castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).FirstOrDefault(pen => pen.Land == "The castle");
        if (castle == null) { Judge("the castle's pen is found", false); Finish(); yield break; }
        castle.Release(new[] { 0 });
        soldier = castle.Enemies[0];
        yield return Real(0.5f);
        Judge("a soldier is let out of the castle's pen", soldier != null && soldier.gameObject.activeInHierarchy, soldier != null ? soldier.Kind.Name : "none");
        if (soldier == null) { Finish(); yield break; }
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;

        build.Attune(Elements.Water, "");
        upgrades.Give(Upgrade.ById(WaterTree.DrawnBackToBreak));
        upgrades.RefreshNow();

        // A plain blow first, no wave held: how far a light blow alone moves it.
        StandOff(1.6f);
        yield return Real(0.25f);
        float plain = 0f, plainEnd = 0f;
        yield return Blow("p4c_drawnback_plain", (most, net) => { plain = most; plainEnd = net; });
        Say($"a plain light blow (no wave) slid it {plain:0.00} m away at most, {plainEnd:0.00} m from where it stood after 1.5 s");

        // Two dodges away from it bank two waves.
        StandOff(2.5f);
        yield return Real(0.2f);
        for (int i = 0; i < 2; i++)
        {
            player.Stamina.Fill();
            Stick(Vector2.down);
            yield return null;
            yield return Press(pad.buttonEast);
            yield return Until(() => !player.Dodge.IsRolling, 1.5f);
            Stick(Vector2.zero);
            yield return Real(0.25f);
        }
        int waves = upgrades.Tally(WaterTree.DrawnBackToBreak);
        Judge("two dodges away bank two waves", waves == 2, $"waves {waves}, held {(upgrades.DodgeHeld != null ? upgrades.DodgeHeld.name : "nothing")}");

        StandOff(1.6f);
        yield return Real(0.1f);
        float broke = 0f, brokeEnd = 0f;
        yield return Blow("p4c_drawnback_break", (most, net) => { broke = most; brokeEnd = net; });
        int left = upgrades.Tally(WaterTree.DrawnBackToBreak);
        Judge("the blow breaks the waves on the soldier and Soaks it", left == 0 && soldier.Afflictions.Has(AfflictionKind.Soaked), $"waves left {left}, Soaked {soldier.Afflictions.Has(AfflictionKind.Soaked)}");
        Judge("the soldier is thrown back about 2 m beyond a plain blow's shove", broke - plain > 1.5f,
            $"slid {broke:0.00} m away at most ({brokeEnd:0.00} m net after 1.5 s); a plain blow slid it {plain:0.00} m");
        Finish();
    }

    private void Finish()
    {
        RunState.Unlocked.Remove(WaterTree.DrawnBackToBreak);
        if (build != null) build.Attune(Elements.Fire, "");
        if (upgrades != null) upgrades.RefreshNow();
        if (castle != null) castle.Clear();
        if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        PlaytestMode.Set(playtestWas);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
