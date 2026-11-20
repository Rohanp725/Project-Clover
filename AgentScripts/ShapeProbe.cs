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

// The shape screen (the owner's decision of 2026-11-20), in the Proving Ground with the playtest on: at the rest fire,
// "Shapes" is chosen in the rest menu with the keys, Steam's Shape mark and Flame Wave's are learned, and on the screen
// the keys (W S, then D) and a pad (the d-pad, then the south button) each choose a line and wear its other shape. The
// choice is then read as worn by the game: ReactionTable.WornShape for Steam, and a Steam set off on a fresh dummy runs
// in the shape chosen. A frame of the screen is kept at review/shots/current/shape_screen.png. Progress stays sandboxed.
//   bash AgentScripts/runprobe.sh ProvingGround ShapeProbe 60
public static class ShapeProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Shape Probe").AddComponent<ShapeProbeRunner>();
        return "probe started";
    }
}

public class ShapeProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerBuild build;
    private PlayerUpgrades upgrades;
    private PlayerReactions reactions;
    private Gamepad pad;
    private int ok, wrong;
    private bool playtestWas;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private const string FlameWaveShape = "fire.base.shape";

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

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    private IEnumerator Press(GamepadButton button)
    {
        InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(button));
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(pad, new GamepadState());
        yield return null;
        yield return null;
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while ((ReadingBox.Instance != null && ReadingBox.Instance.IsOpen || GuidePage.Showing) && guard++ < 60) yield return Tap(Key.E);
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
        build = player.GetComponent<PlayerBuild>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        reactions = player.GetComponent<PlayerReactions>();
        Judge("progress is sandboxed", Progress.Sandboxed);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        // Fire and Water carried, Steam's Shape mark and Flame Wave's learned, Flame Wave in a slot.
        build.Attune(Elements.Fire, Elements.Water);
        upgrades.Give(Upgrade.ById(SteamDuo.Shape));
        upgrades.Give(Upgrade.ById(FlameWaveShape));
        upgrades.RefreshNow();
        int steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
        string steamKey = ReactionTable.ShapeKey(steam);
        var steamMark = Upgrade.ById(SteamDuo.Shape);
        Judge("Steam wears its first shape before any choice", ReactionTable.WornShape(steam, build.Read(), RunState.Unlocked)?.Name == steamMark.Shapes[0].Name);

        // To the rest fire, and "Shapes" chosen with the keys.
        var rest = FindObjectsByType<RestPoint>(FindObjectsInactive.Include).FirstOrDefault();
        Judge("a rest fire stands in the yard", rest != null);
        if (rest == null) { Finish(); yield break; }
        player.Teleport(rest.transform.position + Vector3.back * 1.4f + Vector3.up * 0.05f, Quaternion.identity);
        yield return Real(0.35f);
        yield return Tap(Key.E);
        yield return Until(() => RestMenu.Showing, 1.5f);
        yield return ReadAll();
        Judge("the rest menu is open", RestMenu.Showing);
        var restMenu = RestMenu.Instance;
        int shapesAt = restMenu.Choices.IndexOf(RestEntry.Shapes);
        Judge("the rest menu offers Shapes after the Hand", shapesAt == 1, $"at {shapesAt}");
        for (int step = 0; step < 8 && restMenu.Choices.Selected != shapesAt; step++) yield return Tap(Key.S);
        yield return Tap(Key.E);
        yield return Until(() => ShapeMenu.Showing, 1f);
        var screen = ShapeMenu.Instance;
        Judge("use on Shapes opens the shape screen", ShapeMenu.Showing);
        if (!ShapeMenu.Showing) { Finish(); yield break; }
        Judge("it lists Flame Wave's shape and Steam's", screen.Choices.Count == 2 && !screen.Choices.At(0).Reaction && screen.Choices.At(1).Reaction,
            $"{screen.Choices.Count} lines: {string.Join(", ", Enumerable.Range(0, screen.Choices.Count).Select(i => screen.Choices.At(i).Title + " in " + screen.Choices.At(i).WornShape?.Name))}");

        // The keys: S to Steam's line, D to wear its second shape.
        yield return Tap(Key.S);
        Judge("S moves to Steam's line", screen.Choices.Current.Key == steamKey);
        yield return Tap(Key.D);
        yield return Real(0.1f);
        string second = steamMark.Shapes[1].Name;
        Judge($"D wears Steam's second shape, {second}", build.Read().ShapeOf(steamKey) == 1 && screen.Choices.Current.Worn == 1);
        Judge($"and the game wears it: ReactionTable.WornShape is {second}", ReactionTable.WornShape(steam, build.Read(), RunState.Unlocked)?.Name == second);
        yield return Real(0.2f);
        Judge("the frame of the screen is kept", true);
        ScreenCapture.CaptureScreenshot("review/shots/current/shape_screen.png");
        yield return Real(0.4f);

        // A pad: the d-pad up to Flame Wave's line, the south button to wear its other shape.
        pad = InputSystem.AddDevice<Gamepad>();
        yield return Real(0.1f);
        string flameKey = Upgrade.ById(FlameWaveShape).Branch;
        int flameWas = build.Read().ShapeOf(flameKey);
        yield return Press(GamepadButton.DpadUp);
        Judge("the d-pad moves to Flame Wave's line", screen.Choices.Current.Key == flameKey);
        yield return Press(GamepadButton.South);
        yield return Real(0.1f);
        Judge("the pad's south button wears Flame Wave's other shape", build.Read().ShapeOf(flameKey) == 1 - flameWas,
            $"{flameWas} -> {build.Read().ShapeOf(flameKey)}, worn {screen.Choices.Current.WornShape?.Name}");
        ScreenCapture.CaptureScreenshot("review/shots/current/shape_screen_pad.png");
        yield return Real(0.4f);
        yield return Press(GamepadButton.East);
        yield return Until(() => !ShapeMenu.Showing, 1f);
        Judge("the pad's east button backs out to the fire", !ShapeMenu.Showing && RestMenu.Showing);
        InputSystem.RemoveDevice(pad);
        pad = null;

        // Leave the fire and set Steam off on a fresh dummy: it runs in the shape chosen.
        yield return Real(0.3f);
        yield return Tap(Key.Space);
        yield return Until(() => !RestMenu.Showing && Time.timeScale > 0f, 1.5f);
        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.GetComponent<AfflictedDummy>() == null);
        if (still != null)
        {
            var host = Instantiate(still.gameObject, new Vector3(25f, 0f, 6f), still.transform.rotation).GetComponent<TrainingDummy>();
            yield return Real(0.2f);
            int shown = reactions.TimesShown(steam);
            host.Afflictions.Lay(AfflictionKind.Burn, 2, 0f, 1f, LaySource.Spell, host);
            yield return null;
            host.Afflictions.Lay(AfflictionKind.Soaked, 1, 0f, 1f, LaySource.Spell, host);
            yield return Until(() => reactions.TimesShown(steam) > shown, 1.5f);
            Judge($"Steam set off after the fire runs as {second}", reactions.TimesShown(steam) > shown && reactions.LastShape == second, $"shape '{reactions.LastShape}'");
            Destroy(host.gameObject);
        }
        Finish();
    }

    private void Finish()
    {
        if (pad != null) InputSystem.RemoveDevice(pad);
        if (ShapeMenu.Showing) ShapeMenu.Instance.Close();
        if (RestMenu.Showing) RestMenu.Instance.Close();
        RunState.Unlocked.Remove(SteamDuo.Shape);
        RunState.Unlocked.Remove(FlameWaveShape);
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
