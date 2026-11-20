using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Films built looks the game no longer reaches by its own moves, played straight from the pool on a real enemy: the
// keys named in AgentScripts/look_film_only.txt (comma separated, "ice_rime,metal_rust": the element looks Rime and Rust
// played before their bespoke combos 4 looks). One castle soldier stood on the open floor at (28, 0, 0), the player 3 m
// west facing east, as ReactionGifFrames stands them; each look played once (ElementLooks.Play) and filmed at 20 frames a
// second from 0.15 s before to 0.4 s after it has gone, the 960x540 window about the enemy, to
// review/gifs/reactions/frames/look_<key>/ (strip with reaction_strips.py look_<key>). Game View 1920x1080 first.
// bash AgentScripts/runprobe.sh ProvingGround LookFilm 90
public static class LookFilm
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Look Film").AddComponent<LookFilmRunner>();
        return "probe started";
    }
}

public class LookFilmRunner : MonoBehaviour
{
    private static readonly Vector3 Middle = new Vector3(28f, 0f, 0f);
    private static readonly Vector3 Stand = new Vector3(-3f, 0f, 0f), Away = Vector3.right;
    private const int Rate = 20, Width = 960, Height = 540, Before = 3, After = 8, MostFrames = 160;
    private Enemy enemy;
    private Texture2D picture;
    private readonly List<GameObject> hidden = new List<GameObject>();
    private static readonly FieldInfo SinceHurt = typeof(Enemy).GetField("sinceHurt", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo BarsCalled = typeof(Enemy).GetField("barsCalled", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo HomeField = typeof(Enemy).GetField("home", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo HomeTurnField = typeof(Enemy).GetField("homeRotation", BindingFlags.NonPublic | BindingFlags.Instance);

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Update()
    {
        if (enemy == null || enemy.IsDead) return;
        if (enemy.Health.Fraction < 0.5f) enemy.Health.Fill();
        enemy.Brain.AggroRange = 0f;
        SinceHurt.SetValue(enemy, 0f);
        BarsCalled.SetValue(enemy, true);
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

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        string[] keys = System.IO.File.Exists("AgentScripts/look_film_only.txt")
            ? System.IO.File.ReadAllText("AgentScripts/look_film_only.txt").Split(',').Select(part => part.Trim()).Where(part => part.Length > 0).ToArray()
            : new string[0];
        var backgroundWas = InputSystem.settings.backgroundBehavior;
        var editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var player = FindAnyObjectByType<PlayerController>();
        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
        var pens = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).OrderBy(pen => pen.Land == "The castle" ? 0 : 1).ToList();
        foreach (var pen in pens)
        {
            pen.Release(Enumerable.Range(0, pen.Enemies.Length).ToArray());
            foreach (var one in pen.Enemies)
            {
                if (one == null) continue;
                if (enemy == null && pen.Land == "The castle" && one.Kind.Name != "Gate Captain") enemy = one;
                else one.gameObject.SetActive(false);
            }
        }
        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 14f).Select(text => text.gameObject));
        hidden.AddRange(FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).Where(dummy => (dummy.transform.position - Middle).magnitude < 14f).Select(dummy => dummy.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);
        player.Teleport(Middle + Stand, Quaternion.LookRotation(Away, Vector3.up));
        Vector3 at = Middle + Vector3.up * 0.05f;
        HomeField.SetValue(enemy, at);
        HomeTurnField?.SetValue(enemy, Quaternion.LookRotation(-Away, Vector3.up));
        enemy.Restore();
        enemy.Brain.AggroRange = 0f;
        enemy.Brain.LeashRange = 0.5f;
        enemy.WakeNow();
        yield return Real(2f);
        picture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        foreach (string key in keys)
        {
            int look = ElementLooks.Find(key);
            if (look < 0)
            {
                Say($"WRONG: no look {key}");
                continue;
            }
            string folder = "review/gifs/reactions/frames/look_" + key;
            if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
            System.IO.Directory.CreateDirectory(folder);
            while (ElementLooks.Playing > 0) yield return null;
            yield return Real(0.5f);
            Vector3 onScreen = Camera.main.WorldToScreenPoint(enemy.transform.position + Stand * 0.25f + Vector3.up * 1.2f);
            var window = new RectInt(Mathf.Clamp(Mathf.RoundToInt(onScreen.x * 1920f / Screen.width) - Width / 2, 0, 1920 - Width),
                Mathf.Clamp(Mathf.RoundToInt(onScreen.y * 1080f / Screen.height) - Height / 2, 0, 1080 - Height), Width, Height);
            Time.captureFramerate = Rate;
            int frame = 0, tail = -1, most = 0;
            for (; frame < MostFrames; frame++)
            {
                yield return new WaitForEndOfFrame();
                var shot = ScreenCapture.CaptureScreenshotAsTexture();
                picture.SetPixels(shot.GetPixels(window.x, window.y, Width, Height));
                picture.Apply();
                Destroy(shot);
                System.IO.File.WriteAllBytes($"{folder}/{frame:000}.png", picture.EncodeToPNG());
                if (frame == Before - 1) ElementLooks.Play(look, enemy.transform);
                most = Mathf.Max(most, ElementLooks.Shown);
                if (frame > Before + 2 && tail < 0 && ElementLooks.PlayingOnce == 0) tail = 0;
                if (tail >= 0 && tail++ >= After) break;
                yield return null;
            }
            Time.captureFramerate = 0;
            System.IO.File.WriteAllText(folder + "/info.txt", $"name={key} (played from the pool)\nkind=element look\nstart={Before}\nshown_frame={Before}\nframes={frame}\nenemies=1\n");
            Say($"{key}: {frame} frames, {most} parts drawn at most");
        }
        Destroy(picture);
        foreach (var pen in pens) pen.Clear();
        foreach (var thing in hidden) if (thing != null) thing.SetActive(true);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say("done");
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.captureFramerate = 0;
}
