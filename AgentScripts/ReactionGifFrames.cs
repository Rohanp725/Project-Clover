using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Frames for a GIF of each of the 55 reaction looks, filmed in the game: the Proving Ground's still dummy copied to the
// open floor, the player standing still 3 m west of it, the guide read through first. For each reaction in turn it
// waits until the pool is empty (no part, lamp or showing left from the one before) and a second more, then fixes game
// time at 20 frames a second (Time.captureFramerate) and grabs the game camera's frame every frame, the 960x540 window
// about the dummy cut from the 1920x1080 view at 1:1:
// 0.15 s before the look is set off (ReactionLook.Play, as a blow from the player would carry it), through the look
// until its showing has ended and the pool is empty, and 0.4 s after. Frames go to
// review/gifs/reactions/frames/<name>/<nnn>.png; review/gifs/reactions/frames/<name>/info.txt holds the frame the look
// starts on, its impact time and frame, and whether it is designed or composed. captureFramerate is put back after.
// Set the game view to 1920x1080 first (AgentScripts/GameViewSize.cs). Assemble with AgentScripts/reaction_gifs.py.
// bash AgentScripts/runprobe.sh ProvingGround ReactionGifFrames 400
public static class ReactionGifFrames
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Reaction Gif Frames").AddComponent<ReactionGifFramesRunner>();
        return "probe started";
    }
}

public class ReactionGifFramesRunner : MonoBehaviour
{
    // AgentScripts/reaction_gif_only.txt may hold names (as AfflictionPairs.Name gives them) with commas between to film
    // just those; missing or empty films all 55.
    private static string[] Only = { };
    // Open floor between the training dummies (z 5) and the afflicted row (z -5); the player 3 m west, facing east,
    // so the look has the frame's height above it and the blow carries east.
    private static readonly Vector3 Middle = new Vector3(28f, 0f, 0f), Stand = new Vector3(-3f, 0f, 0f), Away = Vector3.right;
    private const int Rate = 20, Before = 3, After = 8, MostFrames = 240, Width = 960, Height = 540;
    private const string Root = "review/gifs/reactions/frames/";

    private Texture2D picture;
    private FieldInfo instanceField, showingsField;
    private FieldInfo activeField, impactField, ageField;

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

    // The pool's showings are private: read through reflection, for a probe only.
    private System.Array Showings()
    {
        var art = instanceField.GetValue(null);
        return art != null ? (System.Array)showingsField.GetValue(art) : null;
    }

    private int ActiveShowings()
    {
        var all = Showings();
        if (all == null) return 0;
        int active = 0;
        foreach (var one in all)
            if ((bool)activeField.GetValue(one)) active++;
        return active;
    }

    private float NewestImpact()
    {
        var all = Showings();
        float impact = -1f, youngest = float.MaxValue;
        if (all == null) return impact;
        foreach (var one in all)
        {
            if (!(bool)activeField.GetValue(one)) continue;
            float age = (float)ageField.GetValue(one);
            if (age < youngest)
            {
                youngest = age;
                impact = (float)impactField.GetValue(one);
            }
        }
        return impact;
    }

    private bool PoolEmpty() => ReactionArt.PartsShown == 0 && ReactionArt.LampsShown == 0 && ActiveShowings() == 0;

    // Where the 960x540 window sits in the 1920x1080 game view: centred on the dummy's chest, fixed for one reaction so
    // the picture does not move. The game camera's own pixels at 1:1, so the look is seen twice as large as in the whole
    // view scaled down, and the HUD's corners stay out.
    private RectInt Window(Transform host)
    {
        var cam = Camera.main;
        // A quarter of the way toward the player, so the player stands inside the left edge.
        Vector3 onScreen = cam.WorldToScreenPoint(host.position + Stand * 0.25f + Vector3.up * 1.4f);
        float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
        int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - Width / 2, 0, 1920 - Width);
        int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - Height / 2, 0, 1080 - Height);
        return new RectInt(left, bottom, Width, Height);
    }

    // Called at the end of a frame: the game view as drawn, the window about the dummy cut out of it.
    private void Grab(string folder, int index, RectInt window)
    {
        var shot = ScreenCapture.CaptureScreenshotAsTexture();
        if (shot.width != 1920 || shot.height != 1080)
        {
            // Not the size asked for: scaled to it first so the window means the same thing.
            var whole = RenderTexture.GetTemporary(1920, 1080, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(shot, whole);
            var was = RenderTexture.active;
            RenderTexture.active = whole;
            picture.ReadPixels(new Rect(window.x, window.y, Width, Height), 0, 0);
            RenderTexture.active = was;
            RenderTexture.ReleaseTemporary(whole);
        }
        else picture.SetPixels(shot.GetPixels(window.x, window.y, Width, Height));
        picture.Apply();
        Destroy(shot);
        System.IO.File.WriteAllBytes($"{folder}/{index:000}.png", picture.EncodeToPNG());
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        Only = System.IO.File.Exists("AgentScripts/reaction_gif_only.txt")
            ? System.IO.File.ReadAllText("AgentScripts/reaction_gif_only.txt").Split(',').Select(part => part.Trim()).Where(part => part.Length > 0).ToArray()
            : new string[0];
        yield return null;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var player = FindAnyObjectByType<PlayerController>();
        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);

        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        instanceField = typeof(ReactionArt).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
        showingsField = typeof(ReactionArt).GetField("showings", Private);
        var showingType = typeof(ReactionArt).GetNestedType("Showing", BindingFlags.NonPublic);
        activeField = showingType?.GetField("Active");
        impactField = showingType?.GetField("ImpactAt");
        ageField = showingType?.GetField("Age");
        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        if (still == null || !ReactionArt.Ready || instanceField == null || showingsField == null || activeField == null || impactField == null || ageField == null)
        {
            Say($"WRONG: no still dummy ({still != null}), reaction art ({ReactionArt.Ready}) or showings ({showingsField != null}, {activeField != null})");
            Say("done");
            Destroy(gameObject);
            yield break;
        }
        var host = Instantiate(still.gameObject, Middle, still.transform.rotation).GetComponent<TrainingDummy>();
        host.name = "Gif Host";
        // The floor's painted words (the sparring dummy's "FIGHTS BACK") are hidden while filming: they are not the look.
        var words = FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 12f).Select(text => text.gameObject).ToList();
        foreach (var word in words) word.SetActive(false);
        player.Teleport(Middle + Stand, Quaternion.LookRotation(Away, Vector3.up));
        yield return Real(1.5f);
        Say($"screen {Screen.width}x{Screen.height}");

        picture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        var summary = new StringBuilder();
        int filmed = 0;
        for (int pair = 0; pair < AfflictionPairs.Count; pair++)
        {
            string look = AfflictionPairs.Name(pair);
            if (Only.Length > 0 && !Only.Contains(look)) continue;
            string name = look.ToLowerInvariant().Replace(' ', '_').Replace("'", "");
            string folder = Root + name;
            if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
            System.IO.Directory.CreateDirectory(folder);
            bool designed = ReactionArt.IsDesigned(pair);

            // A clean start: the player put back and still, the pool empty, then a second of nothing.
            player.Teleport(Middle + Stand, Quaternion.LookRotation(Away, Vector3.up));
            float clear = Time.unscaledTime + 8f;
            while (!PoolEmpty() && Time.unscaledTime < clear) yield return null;
            bool wasClean = PoolEmpty();
            yield return Real(1f);

            var window = Window(host.transform);
            Time.captureFramerate = Rate;
            int frame = 0;
            for (; frame < Before; frame++)
            {
                yield return new WaitForEndOfFrame();
                Grab(folder, frame, window);
                yield return null;
            }
            int start = frame;
            ReactionLook.Play(pair, host.transform.position, host.transform, Away);
            float impact = NewestImpact();
            int tail = -1;
            while (frame < MostFrames)
            {
                yield return new WaitForEndOfFrame();
                Grab(folder, frame, window);
                frame++;
                if (tail < 0 && frame - start >= 4 && PoolEmpty()) tail = 0;
                if (tail >= 0 && tail++ >= After) break;
                yield return null;
            }
            Time.captureFramerate = 0;
            int impactFrame = start + Mathf.RoundToInt(Mathf.Max(0f, impact) * Rate);
            System.IO.File.WriteAllText(folder + "/info.txt",
                $"name={look}\nkind={(designed ? "designed" : "composed")}\nstart={start}\nimpact={impact:0.000}\nimpact_frame={impactFrame}\nframes={frame}\nended={(tail >= 0)}\nclean_before={wasClean}\n");
            summary.AppendLine($"{name}: {frame} frames, impact {impact:0.00} s, {(tail >= 0 ? "ended" : "CUT at the cap")}{(wasClean ? "" : ", NOT clean before")}");
            Say($"{name}: {(designed ? "designed" : "composed")}, {frame} frames, impact {impact:0.00} s{(tail >= 0 ? "" : ", WRONG: cut at the cap")}{(wasClean ? "" : ", WRONG: pool not empty before")}");
            filmed++;
            yield return null;
        }
        Time.captureFramerate = 0;
        System.IO.File.WriteAllText("review/gifs/reactions/frames/summary.txt", summary.ToString());
        Destroy(picture);
        Destroy(host.gameObject);
        foreach (var word in words) word.SetActive(true);
        Say($"filmed {filmed}");
        Say("done");
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.captureFramerate = 0;
}
