using System.Collections;
using System.IO;
using Clover.UI;
using Clover.UI.Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The Loadout's editor-step check (ui_rest_fire_plan.md 7, step 9), stills only: in the Proving Ground, the guide closed
// with Escape and time shown to run, the Loadout opened as a rest fire opens it, then a still of it opened, after a pad
// press into a list (A), and closed again with B. Game view size from AgentScripts/gameview_size.txt (GameViewSize.cs);
// the file name carries it. Stills review/shots/current/loadout/<size>_<state>.png.
//   bash AgentScripts/runprobe.sh ProvingGround LoadoutShots 60
public static class LoadoutShots
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Loadout Shots").AddComponent<LoadoutShotsRunner>();
        return "probe started";
    }
}

public class LoadoutShotsRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/loadout";
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

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

    private IEnumerator Pad(GamepadButton button)
    {
        var pad = Gamepad.current;
        if (pad == null) yield break;
        InputSystem.QueueStateEvent(pad, new GamepadState(button));
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(pad, new GamepadState());
        yield return null;
        yield return null;
    }

    private IEnumerator Still(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot($"{Folder}/{Screen.width}x{Screen.height}_{name}.png");
        yield return null;
        yield return null;
    }

    private IEnumerator Start()
    {
        yield return null;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Directory.CreateDirectory(Folder);
        float waited = Time.unscaledTime + 5f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        for (int press = 0; press < 6 && GuidePage.Showing; press++) yield return Tap(Key.Escape);
        float before = Time.time;
        yield return Real(0.5f);
        Say($"guide closed {!GuidePage.Showing}, time runs {Time.time > before}");
        if (Gamepad.current == null) InputSystem.AddDevice<Gamepad>();
        var screen = LoadoutScreen.Ensure();
        bool opened = screen != null && screen.Open();
        yield return Real(0.8f);
        Say($"loadout found {screen != null}, opened {opened}, showing {LoadoutScreen.Showing}, project-wide actions {(InputSystem.actions != null ? InputSystem.actions.name : "none")}");
        yield return Still("opened");
        yield return Pad(GamepadButton.South);
        yield return Real(0.4f);
        yield return Still("list");
        yield return Pad(GamepadButton.East);
        yield return Real(0.3f);
        yield return Pad(GamepadButton.East);
        yield return Real(0.3f);
        yield return Pad(GamepadButton.East);
        yield return Real(0.4f);
        Say($"after B from the list: showing {LoadoutScreen.Showing}");
        if (LoadoutScreen.Showing && screen != null) screen.Close();
        Say("done");
        UnityEditor.EditorApplication.ExitPlaymode();
    }
}
