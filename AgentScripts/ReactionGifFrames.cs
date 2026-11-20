using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Frames for a GIF of each of the 55 reactions, filmed in the game on REAL enemies (the owner's comment of 2026-11-19:
// the training dummy hid floor marks, never reacted and stood alone). The Proving Ground's penned enemies (the castle's
// soldiers first) are let out and stood on the open floor at (28, 0, 0), the player 3 m west facing east, the guide read
// through first, progress sandboxed. The enemies are kept alive (healed), kept from chasing or striking the player (their
// aggro range is zero and their leash a step, so they stand where they are put) and can neither guard nor be put to
// sleep, but they flinch, freeze, fall and are thrown as the game makes them. For each reaction the two afflictions are
// laid for real (two stacks each, the first then the second, as a spell lays them) on the main enemy, so the game's own
// reaction goes off: its look, its words and everything it does to the bodies. A reaction that reaches others (chains,
// areas, carried afflictions, a bolt's discharge: effects.md) is filmed with four more enemies spread round the main one
// 1.5 to 2.6 m off; the rest on the main enemy alone. Before each: every enemy put back (healed, afflictions and
// conditions gone) and stood again, the look pool empty and the ground clear, and a second of nothing. Then game time is
// fixed at 20 frames a second and the game camera's frame grabbed every frame, the 960x540 window about the main enemy
// cut from the 1920x1080 view at 1:1 (its floor marks in view): 0.15 s before the afflictions are laid, through the look
// until its showing has ended and the pool is empty (and at least 2.6 s, 4.5 s for the reactions that beat on the
// poison's bites, so the bites are seen), and 0.4 s after. Frames go to review/gifs/reactions/frames/<name>/<nnn>.png
// with info.txt (start frame, impact time and frame, designed or composed, group or alone, set off or not).
// Set the game view to 1920x1080 first (AgentScripts/GameViewSize.cs). Assemble with AgentScripts/reaction_gifs.py.
// bash AgentScripts/runprobe.sh ProvingGround ReactionGifFrames 900
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
    private static string[] Only = { };
    private static readonly Vector3 Middle = new Vector3(28f, 0f, 0f), Stand = new Vector3(-3f, 0f, 0f), Away = Vector3.right;
    // Where the group stands about the main enemy: spread round it, mostly beyond it from the player.
    private static readonly Vector3[] Spread = { new Vector3(1.7f, 0f, 1.5f), new Vector3(1.9f, 0f, -1.4f), new Vector3(-0.4f, 0f, 2.2f), new Vector3(2.9f, 0f, 0.1f) };
    // The reactions that reach more than the one struck (effects.md: everyone within, the nearest other, carried, chained).
    private static readonly string[] Reaching =
    {
        "Flashover", "Thunder Gust", "Rime", "Quickening", "Split Rock", "Anvil Stone", "Glint", "Shut in the Dark", "Fire Medicine", "Carried Embers",
        "Sleet Charge", "Winter Seed", "Iron Thorns", "Earthlight", "Black Chain", "Burning Sap", "Smoulder", "Seized", "Rust", "Shining Herb",
        "Quicksilver", "Ill Wind", "Steam", "Lightning Flower", "Caught Light", "Chokedamp", "Red Hot", "The Bolt Runs True", "Firebloom", "Earthed",
        "Sickle Wind", "Drowned Weight", "Rot Roots", "Live Iron", "Fish Poison",
    };
    // The reactions whose condition beats at the poison's bites: filmed long enough to see them.
    private static readonly string[] Biting = { "Nerve Fire", "Venom Rime" };
    private const int Rate = 20, Before = 3, After = 8, MostFrames = 260, Width = 960, Height = 540;
    private const float LeastSeconds = 2.6f, BitingSeconds = 4.5f;
    private const string Root = "review/gifs/reactions/frames/";

    private Texture2D picture;
    private FieldInfo instanceField, showingsField;
    private FieldInfo activeField, impactField, ageField;
    private PlayerDefence defence;
    private readonly List<Enemy> cast = new List<Enemy>();
    private readonly List<GameObject> hidden = new List<GameObject>();

    private static readonly System.Reflection.FieldInfo SinceHurt = typeof(Enemy).GetField("sinceHurt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private static readonly System.Reflection.FieldInfo BarsCalled = typeof(Enemy).GetField("barsCalled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    // Everyone kept alive, and no enemy able to guard.
    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        foreach (var enemy in cast)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (enemy.Health.Fraction < 0.5f) enemy.Health.Fill();
            enemy.GuardOpenFor = 5f;
            enemy.Brain.AggroRange = 0f;
            enemy.Brain.LeashRange = 0.5f;
            // In a fight its bars are up, and with them what it carries is drawn on its body: kept up here, as the
            // enemy stands still and is never hurt into showing them.
            SinceHurt.SetValue(enemy, 0f);
            BarsCalled.SetValue(enemy, true);
        }
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

    // A bespoke reaction look (combos 4) plays once through the element looks' pool: it counts too.
    private bool PoolEmpty() => ReactionArt.PartsShown == 0 && ReactionArt.LampsShown == 0 && ActiveShowings() == 0 && ElementLooks.PlayingOnce == 0;

    private RectInt Window(Vector3 host)
    {
        var cam = Camera.main;
        Vector3 onScreen = cam.WorldToScreenPoint(host + Stand * 0.25f + Vector3.up * 1.2f);
        float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
        int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - Width / 2, 0, 1920 - Width);
        int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - Height / 2, 0, 1080 - Height);
        return new RectInt(left, bottom, Width, Height);
    }

    private void Grab(string folder, int index, RectInt window)
    {
        var shot = ScreenCapture.CaptureScreenshotAsTexture();
        if (shot.width != 1920 || shot.height != 1080)
        {
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

    // Puts every enemy back and stands the ones this clip wants: the main one at the middle, the group round it.
    // The Bolt Runs True runs down an 8 m lane away from the player (2026-11-20): a second enemy stands on it further out,
    // and the window is carried 2.4 m down it so the bolts running away are seen.
    private bool lane;
    private static readonly Vector3 OnTheLane = new Vector3(4.6f, 0f, -0.2f);

    private void StandCast(int count)
    {
        var homeField = typeof(Enemy).GetField("home", BindingFlags.NonPublic | BindingFlags.Instance);
        var homeTurnField = typeof(Enemy).GetField("homeRotation", BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < cast.Count; i++)
        {
            var enemy = cast[i];
            if (i >= count)
            {
                enemy.Restore();
                enemy.gameObject.SetActive(false);
                continue;
            }
            Vector3 at = Middle + (i == 0 ? Vector3.zero : lane && i == 2 ? OnTheLane : Spread[i - 1]);
            at.y = 0.05f;
            // Facing the player, as a fight would have them. Its home is made the spot, so putting it back stands it
            // there and an idle enemy has nowhere else to walk to.
            Vector3 look = Middle + Stand - at;
            look.y = 0f;
            homeField.SetValue(enemy, at);
            homeTurnField?.SetValue(enemy, Quaternion.LookRotation(look.normalized, Vector3.up));
            enemy.Restore();
            enemy.Brain.AggroRange = 0f;
            enemy.Brain.LeashRange = 0.5f;
            enemy.WakeNow();
        }
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        Only = System.IO.File.Exists("AgentScripts/reaction_gif_only.txt")
            ? System.IO.File.ReadAllText("AgentScripts/reaction_gif_only.txt").Split(',').Select(part => part.Trim()).Where(part => part.Length > 0).ToArray()
            : new string[0];
        yield return null;
        var backgroundWas = InputSystem.settings.backgroundBehavior;
        var editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        var reactions = player.GetComponent<PlayerReactions>();
        Say($"start: sandboxed {Progress.Sandboxed}, reaction art ready {ReactionArt.Ready}");
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

        // The castle's soldiers first, then whatever else the pens hold, five in all.
        var pens = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).OrderBy(pen => pen.Land == "The castle" ? 0 : 1).ToList();
        foreach (var pen in pens)
        {
            if (cast.Count >= 5) break;
            pen.Release(Enumerable.Range(0, pen.Enemies.Length).ToArray());
            foreach (var enemy in pen.Enemies)
            {
                if (enemy == null) continue;
                if (cast.Count < 5 && (pen.Land == "The castle" || enemy.name == "Gilded Spearman")) cast.Add(enemy);
                // One let out but not filmed is put away, so nothing walks in on the frame.
                else enemy.gameObject.SetActive(false);
            }
        }
        if (!Progress.Sandboxed || cast.Count == 0 || !ReactionArt.Ready || showingsField == null || activeField == null)
        {
            Say($"WRONG: sandboxed {Progress.Sandboxed}, enemies {cast.Count}, reaction art {ReactionArt.Ready}, showings {showingsField != null}");
            Say("done");
            Destroy(gameObject);
            yield break;
        }
        Say($"cast: {string.Join(", ", cast.Select(enemy => enemy.Kind.Name))}");
        // The floor's painted words, the training dummies and the afflicted row near the floor are not the look.
        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 14f).Select(text => text.gameObject));
        hidden.AddRange(FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).Where(dummy => (dummy.transform.position - Middle).magnitude < 14f).Select(dummy => dummy.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);
        player.Teleport(Middle + Stand, Quaternion.LookRotation(Away, Vector3.up));
        StandCast(1);
        yield return Real(1.5f);
        Say($"screen {Screen.width}x{Screen.height}");

        picture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        var summary = new StringBuilder();
        int filmed = 0, setOff = 0, grouped = 0;
        for (int pair = 0; pair < AfflictionPairs.Count; pair++)
        {
            string look = AfflictionPairs.Name(pair);
            if (Only.Length > 0 && !Only.Contains(look)) continue;
            string name = look.ToLowerInvariant().Replace(' ', '_').Replace("'", "");
            string folder = Root + name;
            if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
            System.IO.Directory.CreateDirectory(folder);
            bool designed = ReactionArt.IsDesigned(pair);
            bool bespoke = ElementLooks.Find(ReactionLook.SlugOf(pair)) >= 0;
            string kind = bespoke ? "bespoke" : designed ? "designed" : "composed";
            bool group = Reaching.Contains(look);
            int count = group ? Mathf.Min(cast.Count, 5) : 1;

            // A clean start: everyone put back and stood again, the player still, the pool empty and the ground clear.
            player.Teleport(Middle + Stand, Quaternion.LookRotation(Away, Vector3.up));
            lane = look == "The Bolt Runs True";
            StandCast(count);
            float clear = Time.unscaledTime + 8f;
            while ((!PoolEmpty() || Hazards.Alive > 0) && Time.unscaledTime < clear) yield return null;
            bool wasClean = PoolEmpty();
            // Ground the last reaction left (Caught Light's marking light) lays its affliction again on a body put back
            // while it lasts: cleared once more now it is gone, or the next reaction starts on a third affliction and
            // sets off others beside it (Girdled showed Diamond Ring and Shining Herb, 2026-11-20).
            foreach (var enemy in cast) if (enemy != null && enemy.gameObject.activeInHierarchy) enemy.Afflictions.Clear();
            yield return Real(1f);
            var main = cast[0];

            var window = Window(main.transform.position + (lane ? Away * 2.4f : Vector3.zero));
            Time.captureFramerate = Rate;
            int frame = 0;
            for (; frame < Before; frame++)
            {
                yield return new WaitForEndOfFrame();
                Grab(folder, frame, window);
                yield return null;
            }
            int start = frame;
            int shownBefore = reactions.TimesShown(pair);
            var first = AfflictionPairs.First(pair);
            var second = AfflictionPairs.Second(pair);
            main.Afflictions.Lay(first, 2, 0f, 1f, LaySource.Spell, main);
            main.Afflictions.Lay(second, 2, 0f, 1f, LaySource.Spell, main);
            float impact = -1f;
            int tail = -1, shownAt = -1;
            float least = Biting.Contains(look) ? BitingSeconds : LeastSeconds;
            while (frame < MostFrames)
            {
                yield return new WaitForEndOfFrame();
                Grab(folder, frame, window);
                frame++;
                if (shownAt < 0 && reactions.TimesShown(pair) > shownBefore)
                {
                    shownAt = frame;
                    impact = NewestImpact();
                }
                bool longEnough = (frame - start) / (float)Rate >= least;
                if (tail < 0 && frame - start >= 4 && PoolEmpty() && longEnough && (shownAt >= 0 || frame - start > 40)) tail = 0;
                if (tail >= 0 && tail++ >= After) break;
                yield return null;
            }
            Time.captureFramerate = 0;
            int lookStart = shownAt >= 0 ? shownAt - 1 : start;
            int impactFrame = lookStart + Mathf.RoundToInt(Mathf.Max(0f, impact) * Rate);
            bool wentOff = shownAt >= 0;
            System.IO.File.WriteAllText(folder + "/info.txt",
                $"name={look}\nkind={kind}\nstart={start}\nshown_frame={lookStart}\nimpact={impact:0.000}\nimpact_frame={impactFrame}\nframes={frame}\nended={(tail >= 0)}\nclean_before={wasClean}\n" +
                $"enemies={count}\nset_off={wentOff}\n");
            summary.AppendLine($"{name}: {frame} frames, {count} enemies, {(wentOff ? "set off" : "NOT SET OFF")}, impact {impact:0.00} s, {(tail >= 0 ? "ended" : "CUT at the cap")}{(wasClean ? "" : ", NOT clean before")}");
            Say($"{name}: {kind}, {count} enemies, {frame} frames, impact {impact:0.00} s{(wentOff ? "" : ", WRONG: the reaction did not go off")}{(tail >= 0 ? "" : ", WRONG: cut at the cap")}{(wasClean ? "" : ", WRONG: pool not empty before")}");
            filmed++;
            if (wentOff) setOff++;
            if (group) grouped++;
            yield return null;
        }
        Time.captureFramerate = 0;
        if (Only.Length == 0) System.IO.File.WriteAllText(Root + "summary.txt", summary.ToString());
        Destroy(picture);
        foreach (var pen in pens) pen.Clear();
        foreach (var thing in hidden) if (thing != null) thing.SetActive(true);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"filmed {filmed}, set off {setOff}, on a group {grouped}");
        Say("done");
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.captureFramerate = 0;
}
