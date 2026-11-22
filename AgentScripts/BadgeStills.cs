using System.Collections;
using System.IO;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The affliction marks by the health bar at the menus' smallest size, in Atkinson: four laid on the player (short and
// long names), a still of the whole screen and one of the top-left corner at the Game View's size.
//   printf 1920x1080 > AgentScripts/gameview_size.txt; script run GameViewSize; bash AgentScripts/runprobe.sh Castle BadgeStills 30
// Stills: review/shots/current/badge/badge_<w>x<h>.png and badge_<w>x<h>_corner.png.
public static class BadgeStills
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Badge Stills").AddComponent<BadgeStillsRunner>();
        return "probe started";
    }
}

public class BadgeStillsRunner : MonoBehaviour
{
    private const string Folder = "review/shots/current/badge";
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        var was = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Directory.CreateDirectory(Folder);
        float waited = Time.unscaledTime + 5f;
        while (!GuidePage.Showing && Time.unscaledTime < waited) yield return null;
        int presses = 0;
        while (GuidePage.Showing && presses++ < 20) { Keys(Key.Escape); yield return null; yield return null; Keys(); yield return null; yield return null; }
        int frame = Time.frameCount;
        float time = Time.time;
        yield return Real(0.4f);
        Say($"guide {(GuidePage.Showing ? "STILL OPEN" : "closed")}; time runs: {Time.time > time} ({Time.frameCount - frame} frames)");

        var player = FindAnyObjectByType<PlayerController>();
        var defence = player.GetComponent<PlayerDefence>();
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsInactive.Exclude)) enemy.Brain.AggroRange = 0f;
        defence.Afflictions.Clear();
        defence.Afflictions.Lay(AfflictionKind.Static, 2);
        defence.Afflictions.Lay(AfflictionKind.Poison, 3);
        defence.Afflictions.Lay(AfflictionKind.Sundered, 1);
        defence.Afflictions.Lay(AfflictionKind.Burn, 1);
        yield return Real(0.5f);
        var marks = FindAnyObjectByType<AfflictionBadges>();
        string read = marks != null ? marks.Reading() : "(no marks)";
        string font = marks != null && marks.WordsFont != null ? marks.WordsFont.name : "(none)";
        string size = $"{Screen.width}x{Screen.height}";
        float scale = Screen.height / 1080f;
        Say($"marks at {size}: \"{read}\" in {font}, {AfflictionBadges.WordsSize} board px = {AfflictionBadges.WordsSize * scale:0.0} screen px");
        Time.timeScale = 0f;
        yield return null;
        ScreenCapture.CaptureScreenshot($"{Folder}/badge_{size}.png");
        yield return null;
        yield return null;
        // The corner, cut from the screen at its own pixels (no scaling), so the words' real size shows.
        yield return new WaitForEndOfFrame();
        var whole = ScreenCapture.CaptureScreenshotAsTexture();
        int w = Mathf.Min(whole.width, Mathf.RoundToInt(1150 * scale)), h = Mathf.RoundToInt(170 * scale);
        var corner = new Texture2D(w, h, TextureFormat.RGB24, false);
        corner.SetPixels(whole.GetPixels(0, whole.height - h, w, h));
        corner.Apply();
        File.WriteAllBytes($"{Folder}/badge_{size}_corner.png", corner.EncodeToPNG());
        Destroy(whole);
        Destroy(corner);
        Time.timeScale = 1f;
        yield return Real(1.2f);
        Say($"1.2 s later: \"{(marks != null ? marks.Reading() : "")}\"");
        defence.Afflictions.Clear();
        InputSystem.settings.editorInputBehaviorInPlayMode = was;
        Say("done");
        Destroy(gameObject);
    }
}
