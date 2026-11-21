using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Frames for a GIF of every element brazier idling with its contents (the Blender worker's loops, built by Clover > Build
// > Brazier Looks and hung on each brazier's Shown group): for each, one 2.4 s loop (48 frames at 20 a second, time fixed
// by Time.captureFramerate) seen two ways:
//   close  a camera of its own 2.7 m from the bowl along the game's 55-degree line, 960x540;
//   game   the game camera itself, the player stood 2.6 m off toward the plaza's middle, the 960x540 window round the bowl
//          cut at 1:1 from the 1920x1080 game view.
// The Fire brazier is filmed a third and fourth time with the Blender worker's flame tongues in place of the flame field
// (fire_kit_close, fire_kit_game), to compare. With "lineup" in brazier_gif_only.txt it instead stands all eleven in two
// rows 2.2 m apart (six in front, five behind, the ring's order) on the open floor ahead of the player and films one loop
// of the whole 1920x1080 game view (lineup/), then puts them back. The HUD is hidden. Frames to review/gifs/braziers/frames/<element>_<view>/.
// Set the game view to 1920x1080 first (AgentScripts/GameViewSize.cs) and back to free after. Assemble with
// AgentScripts/brazier_gifs.py.  bash AgentScripts/runprobe.sh ProvingGround BrazierGifFrames 400
public static class BrazierGifFrames
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Brazier Gif Frames").AddComponent<BrazierGifFramesRunner>();
        return "probe started";
    }
}

public class BrazierGifFramesRunner : MonoBehaviour
{
    private const int Rate = 20, Loop = 48, Width = 960, Height = 540;
    private const string Root = "review/gifs/braziers/frames/";
    private Texture2D picture;
    private readonly List<Canvas> screens = new List<Canvas>();

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        var backgroundWas = InputSystem.settings.backgroundBehavior;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
        // A guide opened as a list (the playtest's guide since Phase 4b) turns its pages on E and never closes on it:
        // the back key closes it, and failing that it is closed outright, so time is never left stopped.
        if (GuidePage.Showing) yield return Tap(Key.Space);
        if (GuidePage.Showing) GuidePage.Instance.Close();
        if (Time.timeScale <= 0f) Debug.Log("[Probe] WRONG: time still stopped after the guide");
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace && canvas.enabled)
            {
                canvas.enabled = false;
                screens.Add(canvas);
            }
        var player = FindAnyObjectByType<PlayerController>();
        var braziers = FindObjectsByType<BrazierLook>(FindObjectsInactive.Include).OrderBy(look => look.Key).ToList();
        Say($"braziers with contents: {braziers.Count}: {string.Join(", ", braziers.Select(look => look.Key + (look.gameObject.activeInHierarchy ? "" : " (hidden)") + " " + look.Parts + " parts"))}");
        var only = System.IO.File.Exists("AgentScripts/brazier_gif_only.txt")
            ? System.IO.File.ReadAllText("AgentScripts/brazier_gif_only.txt").Split(',').Select(part => part.Trim()).Where(part => part.Length > 0).ToArray()
            : new string[0];
        if (only.Contains("lineup"))
        {
            yield return Lineup(player, braziers);
            only = only.Where(part => part != "lineup").ToArray();
            if (only.Length == 0) braziers.Clear();
        }
        foreach (var look in braziers)
        {
            string element = look.Key.Replace("brazier_", "");
            if (only.Length > 0 && !only.Contains(element)) continue;
            if (!look.gameObject.activeInHierarchy)
            {
                Say($"WRONG: {look.Key} is not shown");
                continue;
            }
            Vector3 foot = look.transform.position;
            Vector3 inward = new Vector3(-foot.x, 0f, -foot.z);
            inward = inward.sqrMagnitude > 0.01f ? inward.normalized : Vector3.back;
            player.Teleport(foot + inward * 2.6f + Vector3.up * 0.05f, Quaternion.LookRotation(-inward, Vector3.up));
            yield return new WaitForSecondsRealtime(1.2f);
            bool field = look.UsesFlameField;
            yield return Film(look, element + "_close", true);
            yield return Film(look, element + "_game", false);
            if (field)
            {
                look.UseFlameField(false);
                yield return new WaitForSecondsRealtime(0.3f);
                yield return Film(look, element + "_kit_close", true);
                yield return Film(look, element + "_kit_game", false);
                look.UseFlameField(true);
            }
            Say($"{look.Key}: filmed{(field ? " (flame field and the kit's tongues)" : "")}");
        }
        foreach (var canvas in screens) if (canvas != null) canvas.enabled = true;
        InputSystem.settings.backgroundBehavior = backgroundWas;
        if (picture != null) Destroy(picture);
        Say("done");
        Destroy(gameObject);
    }

    // All eleven in two rows on the open floor at (28, 0, 0), the player south of them, filmed whole at the game camera.
    private IEnumerator Lineup(PlayerController player, List<BrazierLook> braziers)
    {
        var order = new[] { "fire", "lightning", "wind", "ice", "water", "life", "earth", "metal", "light", "dark", "poison" };
        var moved = new List<(Transform root, Vector3 at, Quaternion turn)>();
        var middle = new Vector3(28f, 0f, 1.2f);
        for (int i = 0; i < order.Length; i++)
        {
            var look = braziers.FirstOrDefault(one => one.Key == "brazier_" + order[i] && one.gameObject.activeInHierarchy);
            if (look == null) continue;
            // The brazier's root is the Shown group's parent.
            var root = look.transform.parent;
            moved.Add((root, root.position, root.rotation));
            bool front = i < 6;
            float x = front ? (i - 2.5f) * 2.2f : (i - 6 - 2f) * 2.2f;
            root.SetPositionAndRotation(middle + new Vector3(x, 0f, front ? -1.1f : 1.1f), Quaternion.identity);
        }
        // The floor's words and the dummies in front of the rows are not the braziers: put away while filming.
        var hidden = new List<GameObject>();
        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - middle).magnitude < 14f).Select(text => text.gameObject));
        hidden.AddRange(FindObjectsByType<Clover.Gameplay.TrainingDummy>(FindObjectsInactive.Exclude).Where(dummy => (dummy.transform.position - middle).magnitude < 16f).Select(dummy => dummy.gameObject));
        hidden.AddRange(FindObjectsByType<Clover.Gameplay.DummyAttacker>(FindObjectsInactive.Exclude).Select(dummy => dummy.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);
        // Just south of the front row, in the gap between its two middle braziers, so both rows are in the game camera's frame.
        player.Teleport(middle + new Vector3(0f, 0.05f, -1.9f), Quaternion.LookRotation(Vector3.forward, Vector3.up));
        yield return new WaitForSecondsRealtime(1.5f);
        string folder = Root + "lineup";
        if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
        System.IO.Directory.CreateDirectory(folder);
        var whole = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        Time.captureFramerate = Rate;
        yield return null;
        for (int frame = 0; frame < Loop; frame++)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            whole.SetPixels(shot.GetPixels(0, 0, Mathf.Min(1920, shot.width), Mathf.Min(1080, shot.height)));
            whole.Apply();
            Destroy(shot);
            System.IO.File.WriteAllBytes($"{folder}/{frame:000}.png", whole.EncodeToPNG());
        }
        Time.captureFramerate = 0;
        Destroy(whole);
        System.IO.File.WriteAllText(folder + "/info.txt", $"brazier=lineup\nview=game\nframes={Loop}\nbraziers={moved.Count}\n");
        foreach (var (root, at, turn) in moved) root.SetPositionAndRotation(at, turn);
        foreach (var thing in hidden) if (thing != null) thing.SetActive(true);
        Say($"lineup: {moved.Count} braziers filmed in a row");
    }

    private IEnumerator Film(BrazierLook look, string name, bool close)
    {
        string folder = Root + name;
        if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
        System.IO.Directory.CreateDirectory(folder);
        Vector3 bowl = look.transform.position + Vector3.up * 1.6f;
        Camera camera = null;
        RenderTexture target = null;
        RectInt window = default;
        if (close)
        {
            // Along the game camera's own line (55 degrees down, looking north as it does), 2.7 m back.
            var game = Camera.main;
            Vector3 back = game != null ? -game.transform.forward : new Vector3(0f, 0.82f, -0.57f);
            var holder = new GameObject("Brazier Close Camera");
            camera = holder.AddComponent<Camera>();
            if (game != null) camera.CopyFrom(game);
            camera.fieldOfView = 40f;
            camera.transform.SetPositionAndRotation(bowl + back.normalized * 2.7f, Quaternion.LookRotation(-back.normalized, Vector3.up));
            target = new RenderTexture(Width, Height, 24);
            camera.targetTexture = target;
        }
        else
        {
            var game = Camera.main;
            Vector3 onScreen = game.WorldToScreenPoint(bowl);
            float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
            int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - Width / 2, 0, 1920 - Width);
            int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - Height / 2, 0, 1080 - Height);
            window = new RectInt(left, bottom, Width, Height);
        }
        if (picture == null) picture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        Time.captureFramerate = Rate;
        yield return null;
        for (int frame = 0; frame < Loop; frame++)
        {
            yield return new WaitForEndOfFrame();
            if (close)
            {
                var was = RenderTexture.active;
                RenderTexture.active = target;
                picture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                RenderTexture.active = was;
            }
            else
            {
                var shot = ScreenCapture.CaptureScreenshotAsTexture();
                picture.SetPixels(shot.GetPixels(window.x, window.y, window.width, window.height));
                Destroy(shot);
            }
            picture.Apply();
            System.IO.File.WriteAllBytes($"{folder}/{frame:000}.png", picture.EncodeToPNG());
        }
        Time.captureFramerate = 0;
        System.IO.File.WriteAllText(folder + "/info.txt", $"brazier={look.Key}\nview={(close ? "close" : "game")}\nframes={Loop}\nflame_field={look.UsesFlameField}\n");
        if (camera != null)
        {
            camera.targetTexture = null;
            Destroy(camera.gameObject);
        }
        if (target != null) target.Release();
    }

    private void OnDestroy() => Time.captureFramerate = 0;
}
