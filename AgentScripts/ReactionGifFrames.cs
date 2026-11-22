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
// 1.5 to 2.6 m off; the rest on the main enemy alone. A bespoke look (combos 4) is filmed instead on as many enemies as
// its recipe has bodies, stood where the recipe stood them, turned to the game's way. Before each: every enemy put back (healed, afflictions and
// conditions gone) and stood again, the look pool empty and the ground clear, and a second of nothing. Then game time is
// fixed at 20 frames a second and the game camera's frame grabbed every frame, the 960x540 window about the main enemy
// cut from the 1920x1080 view at 1:1 (its floor marks in view): 0.15 s before the afflictions are laid, through the look
// until its showing has ended and the pool is empty (and at least 2.6 s, 4.5 s for the reactions that beat on the
// poison's bites, so the bites are seen), and 0.4 s after. Frames go to review/gifs/reactions/frames/<name>/<nnn>.png
// with info.txt (start frame, impact time and frame, designed or composed, group or alone, set off or not).
// Set the game view to 1920x1080 first (AgentScripts/GameViewSize.cs). Assemble with AgentScripts/reaction_gifs.py.
//
// The rig, v2 (the owner, 2026-11-22: "the enemies are getting hit by something, even after the effect ... a second
// cast of both element afflictions on the enemy, even when it cant react"; docs/design/feedback/2026-11-20_round4_early_notes.md):
// - Fresh bodies for every clip: every enemy put back, afflictions cleared, the look pool and the ground empty.
// - One trigger: the first element's affliction laid once (two stacks, as a spell lays it), then the second element's
//   real base spell cast ONCE by the player at the main enemy, which sets the reaction off. Nothing is cast or struck
//   after it. Only if that spell sets nothing off within 1.6 s is the second laid by hand, once (info.txt: trigger=laid).
// - Quiet enemies: no ranged casters in the cast, the areas' storm strikes off, an attack's wind-up broken at once,
//   never provoked; they still flinch, stagger and fall.
// - Self-checks from a log of every affliction laid (CombatEvents.AfflictionLaid), every blow taken (Enemy.Struck) and
//   the looks on the struck one, frame by frame. A clip FAILS when, after the reaction fires, an affliction is laid by
//   anything but a reaction; a blow lands that is not the reaction's own, or any blow lands after the effect has ended;
//   or the second element's own affliction look shows on the body that just reacted. The reasons are in info.txt
//   (check=PASS or FAIL ...), the summary and the probe's lines, and every logged event is in log.txt.
// - AgentScripts/reaction_gif_stills.txt present: a check run by day, a few frames only (before, 0.2 s and 1 s after
//   the reaction, and the last), the checks the same.
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
    private static readonly Vector3 Middle = new Vector3(28f, 0f, 0f);
    // Where the player stands from the main enemy and the way it faces: 3 m west, facing east, unless the film turns it.
    private Vector3 Stand = new Vector3(-3f, 0f, 0f), Away = Vector3.right;
    // Films whose player faces another way (degrees from north), so what the recipe stood to the player's side stands to
    // the side on screen, where no health bar covers what runs between the bodies. Black Chain was filmed facing 150
    // degrees while its chain was baked toward the recipe's spot (straight behind the struck one from the game camera,
    // behind its bar); its chain is now drawn live to whichever enemy the game binds (BoundChains), arcing up between
    // the two, so it is filmed in normal play facing.
    private static readonly Dictionary<string, float> Facings = new Dictionary<string, float>();
    // Reactions that bind the nearest other enemy (Black Chain): the window is centred between the struck one and that
    // one, so both ends of what runs between them are in it.
    private static readonly string[] Binding = { "Black Chain" };

    // Where the enemy nearest the main one stands from it, of the first count stood.
    private Vector3 NearestOther(int count)
    {
        Vector3 best = Vector3.zero;
        for (int i = 1; i < count && i < layout.Count; i++)
            if (best == Vector3.zero || layout[i].sqrMagnitude < best.sqrMagnitude) best = layout[i];
        return best;
    }
    // Where the group stands about the main enemy: spread round it, mostly beyond it from the player.
    private static readonly Vector3[] Spread = { new Vector3(1.7f, 0f, 1.5f), new Vector3(1.9f, 0f, -1.4f), new Vector3(-0.4f, 0f, 2.2f), new Vector3(2.9f, 0f, 0.1f) };
    // The reactions that reach more than the one struck (effects.md: everyone within, the nearest other, carried, chained).
    private static readonly string[] Reaching =
    {
        "Flashover", "Thunder Gust", "Rime", "Quickening", "Split Rock", "Anvil Stone", "Glint", "Shut in the Dark", "Fire Medicine", "Carried Embers",
        "Sleet Charge", "Winter Seed", "Iron Thorns", "Earthlight", "Black Chain", "Burning Sap", "Smoulder", "Seized", "Rust", "Shining Herb",
        "Quicksilver", "Ill Wind", "Steam", "Lightning Flower", "Caught Light", "Chokedamp", "Red Hot", "The Bolt Runs True", "Firebloom", "Earthed",
        "Sickle Wind", "Drowned Weight", "Rot Roots", "Live Iron", "Fish Poison", "Windthrow",
    };
    // The reactions whose condition beats at the poison's bites: filmed long enough to see them.
    private static readonly string[] Biting = { "Nerve Fire", "Venom Rime" };
    private const int Rate = 20, Before = 3, After = 8, MostFrames = 260, Width = 960, Height = 540;
    private const float LeastSeconds = 4.2f, BitingSeconds = 4.5f;
    // AgentScripts/reaction_gif_close.txt present: each film is a close-up, the game camera drawn in to about 5 m from the
    // struck one (the owner's note of 2026-11-20 on pieces floating off the body), to review/gifs/reactions/close/.
    // The only-list may also name an affliction alone, "aff:Chill:2" (the kind and its depth), laid on every enemy stood.
    private static bool Close => System.IO.File.Exists("AgentScripts/reaction_gif_close.txt");
    private static string Root => Close ? "review/gifs/reactions/close/" : "review/gifs/reactions/frames/";
    private const float CloseDistance = 5f;

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
            // Quiet: never provoked into a fight, and an attack begun is broken off in its wind-up.
            enemy.Brain.ProvokedSeconds = 0f;
            if (enemy.Brain.State == EnemyState.Telegraph) enemy.Brain.Reset();
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

    // The element looks playing now, by key and body, and the old reaction art's and the effects' counts.
    private static string LooksPlaying()
    {
        var looksField = typeof(ElementLooks).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
        var looks = looksField.GetValue(null);
        if (looks == null) return "no element looks";
        var showings = (System.Array)typeof(ElementLooks).GetField("showings", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(looks);
        var set = (ElementLookSet)typeof(ElementLooks).GetField("set", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(looks);
        var type = showings.GetType().GetElementType();
        var text = new StringBuilder();
        foreach (var one in showings)
        {
            if (!(bool)type.GetField("Active").GetValue(one)) continue;
            var body = (Transform)type.GetField("Body").GetValue(one);
            float age = (float)type.GetField("Age").GetValue(one), veiledAt = (float)type.GetField("VeiledAt").GetValue(one), keptAt = (float)type.GetField("KeptAt").GetValue(one);
            text.Append(set.Looks[(int)type.GetField("Look").GetValue(one)].Key).Append(" on ").Append(body != null ? body.name : "-")
                .Append($" age {age:0.00}{(veiledAt >= 0f && veiledAt >= keptAt ? " veiled" : "")}").Append("; ");
        }
        text.Append($"reaction art parts {ReactionArt.PartsShown}, lamps {ReactionArt.LampsShown}");
        return text.ToString();
    }

    // A bespoke reaction look (combos 4) plays once through the element looks' pool: it counts too.
    private bool PoolEmpty() => ReactionArt.PartsShown == 0 && ReactionArt.LampsShown == 0 && ActiveShowings() == 0 && ElementLooks.PlayingOnce == 0;

    // The window about a group the look reaches: centred on the middle of all their bodies as the camera sees them (feet
    // and heads), so a look carried to enemies 5 m behind (Golden Feathers) is filmed reaching them, not leaving the frame.
    private RectInt GroupWindow(Vector3 main, int count)
    {
        var cam = Camera.main;
        float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
        float low = float.MaxValue, high = float.MinValue, under = float.MaxValue, over = float.MinValue;
        for (int i = 0; i < count && i < Mathf.Max(1, layout.Count); i++)
        {
            Vector3 feet = main + (i == 0 ? Vector3.zero : layout[i]);
            foreach (var point in new[] { cam.WorldToScreenPoint(feet), cam.WorldToScreenPoint(feet + Vector3.up * 3f) })
            {
                low = Mathf.Min(low, point.x * scaleX);
                high = Mathf.Max(high, point.x * scaleX);
                under = Mathf.Min(under, point.y * scaleY);
                over = Mathf.Max(over, point.y * scaleY);
            }
        }
        // As wide as the group needs with a margin (the game camera's own view, drawn smaller), never less than the
        // usual 1:1 window nor more than the whole view.
        float wide = Mathf.Clamp(Mathf.Max(high - low + 500f, (over - under + 240f) * 16f / 9f), Width, 1440f);
        int across = Mathf.RoundToInt(wide), up = Mathf.RoundToInt(wide * 9f / 16f);
        int left = Mathf.Clamp(Mathf.RoundToInt((low + high) * 0.5f) - across / 2, 0, 1920 - across);
        int bottom = Mathf.Clamp(Mathf.RoundToInt((under + over) * 0.5f) - up / 2, 0, 1080 - up);
        return new RectInt(left, bottom, across, up);
    }

    // A close-up's window: 1440x810 of the 1920x1080 view (drawn down to the film's size) about the struck one's chest.
    private RectInt CloseWindow(Vector3 host)
    {
        var cam = Camera.main;
        Vector3 onScreen = cam.WorldToScreenPoint(host + Vector3.up * 1.1f);
        float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
        int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - 720, 0, 1920 - 1440);
        int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - 405, 0, 1080 - 810);
        return new RectInt(left, bottom, 1440, 810);
    }

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
        if ((window.width != Width || window.height != Height) && shot.width == 1920 && shot.height == 1080)
        {
            // A window wider than the film: the game view's part drawn down to the film's size, averaged on the CPU from
            // the screenshot's own bytes (a blit through a render texture lightened every frame, a second gamma).
            var source = shot.GetPixels32();
            var drawn = new Color32[Width * Height];
            float step = window.width / (float)Width;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    float sx = window.x + (x + 0.5f) * step - 0.5f, sy = window.y + (y + 0.5f) * step - 0.5f;
                    int x0 = Mathf.Clamp((int)sx, 0, 1918), y0 = Mathf.Clamp((int)sy, 0, 1078);
                    float fx = Mathf.Clamp01(sx - x0), fy = Mathf.Clamp01(sy - y0);
                    var a = source[y0 * 1920 + x0];
                    var b = source[y0 * 1920 + x0 + 1];
                    var c = source[(y0 + 1) * 1920 + x0];
                    var d = source[(y0 + 1) * 1920 + x0 + 1];
                    drawn[y * Width + x] = Color32.Lerp(Color32.Lerp(a, b, fx), Color32.Lerp(c, d, fx), fy);
                }
            picture.SetPixels32(drawn);
        }
        else if (shot.width != 1920 || shot.height != 1080)
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

    // Where the Blender worker's film stood the bodies about the main one (combos4_recipe.json), turned as the game turns
    // the look (the recipe's player faced the main body; here the player faces east): so a chain, a ray or a thrown thorn
    // reaches the enemy it was drawn to reach (2026-11-20: Black Chain's chain pointed at no one on the spread).
    private readonly List<Vector3> layout = new List<Vector3>();
    private Dictionary<string, List<Vector3>> recipeLayouts;

    private List<Vector3> RecipeLayout(string slug)
    {
        if (recipeLayouts == null)
        {
            recipeLayouts = new Dictionary<string, List<Vector3>>();
            var text = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(Clover.EditorTools.ElementLookBuilder.ReactionRecipePath);
            if (text != null)
                foreach (Dictionary<string, object> look in (List<object>)((Dictionary<string, object>)Clover.EditorTools.ReactionRecipe.ReadJson(text.text))["looks"])
                {
                    var bodies = new List<Vector3>();
                    foreach (Dictionary<string, object> body in (List<object>)look["bodies"]) bodies.Add(Vec(body["position"]));
                    Vector3 player = Vec(((Dictionary<string, object>)look["player"])["position"]);
                    if (bodies.Count == 0) continue;
                    // Kept turned so the recipe's player faces north; turned to this film's way when stood.
                    Vector3 toward = bodies[0] - player;
                    float facing = Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg;
                    var turn = Quaternion.Euler(0f, -facing, 0f);
                    var placed = new List<Vector3>();
                    for (int i = 0; i < bodies.Count; i++) placed.Add(turn * (bodies[i] - bodies[0]));
                    recipeLayouts[(string)look["slug"]] = placed;
                }
        }
        if (!recipeLayouts.TryGetValue(slug, out var found)) return null;
        var toFilm = Quaternion.Euler(0f, Mathf.Atan2(Away.x, Away.z) * Mathf.Rad2Deg, 0f);
        var stood = new List<Vector3>();
        for (int i = 0; i < found.Count; i++)
        {
            Vector3 at = toFilm * found[i];
            // A body stood between the game camera and the main one hides it on film (Earthlight's Gate Captain,
            // 2026-11-20): moved clear, no further than the game's look still finds it (1.6 m).
            if (i > 0 && at.z < -0.4f && Mathf.Abs(at.x) < 1.4f)
            {
                if (at.magnitude < 1.5f) at.x = (at.x < 0f ? -1f : 1f) * 1.4f;
                else if (at.z > -2.5f) at.z = Mathf.Max(-2.5f, at.z - 1.2f);
            }
            stood.Add(at);
        }
        return stood;
    }

    private static Vector3 Vec(object value)
    {
        var list = (List<object>)value;
        return new Vector3(System.Convert.ToSingle(list[0]), System.Convert.ToSingle(list[1]), System.Convert.ToSingle(list[2]));
    }

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
            Vector3 at = Middle + (i == 0 ? Vector3.zero : layout.Count > i ? layout[i] : lane && i == 2 ? OnTheLane : Spread[i - 1]);
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
        // A guide opened as a list (the playtest's guide since Phase 4b) turns its pages on E and never closes on it:
        // the back key closes it, and failing that it is closed outright, so time is never left stopped.
        if (GuidePage.Showing) yield return Tap(Key.Space);
        if (GuidePage.Showing) GuidePage.Instance.Close();
        if (Time.timeScale <= 0f) Debug.Log("[Probe] WRONG: time still stopped after the guide");

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
                // No caster among the cast: a ranged enemy's bolts landed after the effect.
                bool quiet = !enemy.IsRanged && !enemy.Kind.IsBoss;
                if (cast.Count < 5 && quiet && (pen.Land == "The castle" || enemy.name == "Gilded Spearman")) cast.Add(enemy);
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
        // No storm strikes from the areas while filming.
        directors = FindObjectsByType<AreaDirector>(FindObjectsInactive.Exclude).Where(director => director.enabled).ToList();
        foreach (var director in directors) director.enabled = false;
        CombatEvents.AfflictionLaid += OnLaid;
        CombatEvents.Reacted += OnReacted;
        foreach (var enemy in cast) enemy.Struck += OnStruck;
        element = player.GetComponent<PlayerElement>();
        // The floor's painted words, the training dummies and the afflicted row near the floor are not the look.
        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 14f).Select(text => text.gameObject));
        hidden.AddRange(FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).Where(dummy => (dummy.transform.position - Middle).magnitude < 14f).Select(dummy => dummy.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);
        // The player's target brackets under the main enemy are the game's aim, not the reaction: on film they read as
        // the "dashed rings" round the enemy's feet (the audit of 2026-11-20, Rime). Put away while filming.
        var reticle = player.GetComponent<TargetReticle>();
        var reticleRing = typeof(TargetReticle).GetField("ringRenderer", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(reticle) as Renderer;
        if (reticle != null) reticle.enabled = false;
        if (reticleRing != null) reticleRing.enabled = false;
        // A group's window can reach the screen's edges: the screen's own HUD (the player's bars, the boss's name, the
        // Shards) is put away while filming, so only the game world is in the film.
        var huds = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude).Where(canvas => canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace && canvas.enabled).ToList();
        foreach (var canvas in huds) canvas.enabled = false;
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
            // A bespoke look is filmed on the bodies as its recipe stood them, all of them (those it reaches and those
            // it leaves alone), so its reach shows as it was drawn.
            layout.Clear();
            Away = Facings.TryGetValue(look, out float facingWay) ? Quaternion.Euler(0f, facingWay, 0f) * Vector3.forward : Vector3.right;
            Stand = -3f * Away;
            var recipeLayout = bespoke ? RecipeLayout(ReactionLook.SlugOf(pair)) : null;
            if (recipeLayout != null && recipeLayout.Count > 1)
            {
                layout.AddRange(recipeLayout);
                // The Gate Captain is the tallest of the cast: stood in front of the main one he hides it (Split Rock and
                // Earthlight, 2026-11-20), so he takes the place furthest back and whoever had it takes his. The looks
                // find whoever stands at each place, so the reach is the same.
                int captain = cast.FindIndex(enemy => enemy != null && enemy.Kind.Name == "Gate Captain");
                if (captain > 0 && captain < layout.Count && layout[captain].z < -0.4f)
                {
                    int back = captain;
                    for (int i = 1; i < layout.Count; i++) if (layout[i].z > layout[back].z) back = i;
                    (layout[captain], layout[back]) = (layout[back], layout[captain]);
                }
                count = Mathf.Min(cast.Count, recipeLayout.Count);
                group = true;
            }

            // The second element is the one the player carries and casts: taken now, before the clean start.
            var secondElement = Elements.All.FirstOrDefault(info => info.Affliction == AfflictionPairs.Second(pair));
            if (secondElement != null && (!element.HasElement || element.Info != secondElement)) element.Take(ElementKit.ByName(secondElement.Name, false));
            // A clean start: everyone put back and stood again, the player still, the pool empty and the ground clear.
            player.Teleport(Middle + Stand, Quaternion.LookRotation(Away, Vector3.up));
            lane = look == "The Bolt Runs True";
            StandCast(count);
            // A group the look reaches is framed by the game camera itself: it looks ahead from the player to the middle of
            // the group, at the game's own distance and angle, as it frames a fight (TopDownCamera.SetFraming). The film's
            // window is then cut about the group (GroupWindow).
            var gameCamera = Camera.main != null ? Camera.main.GetComponent<TopDownCamera>() : null;
            if (gameCamera != null && layout.Count > 1 && !lane && !Binding.Contains(look))
            {
                Vector3 middle = Vector3.zero;
                for (int i = 0; i < count && i < layout.Count; i++) middle += i == 0 ? Vector3.zero : layout[i];
                middle /= Mathf.Max(1, count);
                Vector3 ahead = Middle + middle * 0.8f - (Middle + Stand);
                ahead.y = 0f;
                gameCamera.SetFraming(1f, ahead);
            }
            else if (gameCamera != null) gameCamera.ClearFraming();
            if (gameCamera != null && Close) gameCamera.SetFraming(CloseDistance / gameCamera.Distance, -Stand);
            float clear = Time.unscaledTime + 8f;
            while ((!PoolEmpty() || Hazards.Alive > 0) && Time.unscaledTime < clear) yield return null;
            bool wasClean = PoolEmpty();
            // Ground the last reaction left (Caught Light's marking light) lays its affliction again on a body put back
            // while it lasts: cleared once more now it is gone, or the next reaction starts on a third affliction and
            // sets off others beside it (Girdled showed Diamond Ring and Shining Herb, 2026-11-20).
            foreach (var enemy in cast) if (enemy != null && enemy.gameObject.activeInHierarchy) enemy.Afflictions.Clear();
            // Long enough for a sun-wheel the last film's Marked laid (1.5 s) to be gone: it stood under Girdled, Smoulder
            // and Breath Taken at t 0 as a "beige compass ring" (the audit of 2026-11-20).
            yield return Real(1.8f);
            var main = cast[0];

            // A film turned toward the screen's right edge has the spell slots' corner in its window: put away while filming.
            // The spell slots' corner is put away while filming: a window carried to the group's side has it in view.
            var slots = SpellDiamondHud.Instance != null ? SpellDiamondHud.Instance.gameObject : null;
            if (slots != null) slots.SetActive(false);
            var window = Window(main.transform.position + (lane ? Away * 2.4f : Binding.Contains(look) && layout.Count > 1 ? NearestOther(count) * 0.5f
                : Facings.ContainsKey(look) && recipeLayout != null && recipeLayout.Count > 1 ? recipeLayout[1] * 0.5f : Vector3.zero));
            if (!lane && !Binding.Contains(look) && layout.Count > 1 && !Close) window = GroupWindow(main.transform.position, count);
            if (Close) window = CloseWindow(main.transform.position);
            Say($"{name} window {window.x},{window.y} {window.width}x{window.height}");
            Time.captureFramerate = Rate;
            int frame = 0;
            laid.Clear();
            struckLog.Clear();
            reactedFrame = -1;
            recording = true;
            mainBody = main;
            for (; frame < Before; frame++)
            {
                clipFrame = frame;
                yield return new WaitForEndOfFrame();
                if (!Stills || frame == Before - 1) Grab(folder, frame, window);
                yield return null;
            }
            int start = frame;
            int shownBefore = reactions.TimesShown(pair);
            var first = AfflictionPairs.First(pair);
            var second = AfflictionPairs.Second(pair);
            // One trigger: the first laid once, the second's own spell cast once at the main enemy.
            clipFrame = frame;
            main.Afflictions.Lay(first, 2, 0f, 1f, LaySource.Spell, main);
            var spellsHeld = player.GetComponent<PlayerSpells>();
            if (spellsHeld != null) spellsHeld.Cooldowns.ResetAll();
            player.transform.rotation = Quaternion.LookRotation(Flat(main.transform.position - player.transform.position).normalized, Vector3.up);
            bool cast2 = secondElement != null && !LayByHand.Contains(secondElement.Name) && element.CastBase(secondElement.Name);
            string trigger = cast2 ? "cast" : "laid";
            if (!cast2) main.Afflictions.Lay(second, 2, 0f, 1f, LaySource.Spell, main);
            float impact = -1f;
            int tail = -1, shownAt = -1, effectEnd = -1, lateOwn = 0;
            float lastSecondAge = -1f;
            var fails = new List<string>();
            var secondLooks = Enumerable.Range(1, 5).Select(depth => ElementLooks.Find(AfflictionArt.LookOf(second, depth))).Where(index => index >= 0).Distinct().ToArray();
            float least = Biting.Contains(look) ? BitingSeconds : LeastSeconds;
            while (frame < MostFrames)
            {
                clipFrame = frame;
                yield return new WaitForEndOfFrame();
                bool keep = !Stills || (reactedFrame >= 0 && (frame == reactedFrame + 4 || frame == reactedFrame + 20));
                if (keep) Grab(folder, frame, window);
                lastWindow = window;
                // The cast set nothing off: the second laid by hand, once.
                if (reactedFrame < 0 && cast2 && frame - start == 32)
                {
                    trigger = "laid";
                    main.Afflictions.Lay(second, 2, 0f, 1f, LaySource.Spell, main);
                }
                // The second element's own look shown again on the body that just reacted: begun after the reaction (it
                // was not showing, or its showing started over), as a second cast of it would. One laid with the trigger
                // and still playing on is the affliction the body carries, not a second cast.
                if (reactedFrame >= 0 && frame > reactedFrame + 1)
                {
                    float age = YoungestAge(secondLooks, main.transform);
                    if (age >= 0f && (lastSecondAge < 0f || age + 0.001f < lastSecondAge) && frame > reactedFrame + 2 && !fails.Any(f => f.StartsWith("look")))
                        fails.Add($"look: {second}'s own look begun again on the struck one {(frame - reactedFrame) / (float)Rate:0.00} s after the reaction");
                    lastSecondAge = age;
                }
                else if (reactedFrame >= 0) lastSecondAge = YoungestAge(secondLooks, main.transform);
                if (reactedFrame >= 0 && effectEnd < 0 && frame > reactedFrame + 2 && PoolEmpty()) effectEnd = frame;
                frame++;
                if (shownAt < 0 && reactions.TimesShown(pair) > shownBefore)
                {
                    shownAt = frame;
                    impact = NewestImpact();
                }
                // What is drawn 0.8 s in, to tell the reaction's own look from the afflictions' and the game's.
                if (frame - start == 16) Say($"{name} at 0.8 s: {LooksPlaying()}");
                // After the reaction's look and its held aftermath: what the struck one still carries, and its looks' ages
                // (an affliction cast again shows an age under the time since the reaction).
                if (frame - start == 70 || frame - start == 80)
                {
                    var others = new StringBuilder();
                    for (int i = 1; i < count && i < cast.Count; i++)
                    {
                        var other = cast[i];
                        if (other == null || !other.gameObject.activeInHierarchy) continue;
                        Vector3 off = other.transform.position - main.transform.position;
                        others.Append($"{other.Kind.Name} {Vector3.Dot(off, Away):0.0} m on, {Vector3.Cross(Away, off).magnitude:0.0} m off: {AfflictionPairs.First(pair)} {other.Afflictions.Stacks(AfflictionPairs.First(pair))} {AfflictionPairs.Second(pair)} {other.Afflictions.Stacks(AfflictionPairs.Second(pair))}; ");
                    }
                    Say($"{name} at {(frame - start) / (float)Rate:0.0} s: {AfflictionPairs.First(pair)} {main.Afflictions.Stacks(AfflictionPairs.First(pair))}, {AfflictionPairs.Second(pair)} {main.Afflictions.Stacks(AfflictionPairs.Second(pair))}; others: {others}; {LooksPlaying()}");
                }
                bool longEnough = (frame - start) / (float)Rate >= least;
                if (tail < 0 && frame - start >= 4 && PoolEmpty() && longEnough && (shownAt >= 0 || frame - start > 40)) tail = 0;
                if (tail >= 0 && tail++ >= After) break;
                yield return null;
            }
            if (Stills) Grab(folder, frame, lastWindow);
            recording = false;
            Time.captureFramerate = 0;
            if (slots != null) slots.SetActive(true);
            if (gameCamera != null) gameCamera.ClearFraming();
            // The checks, from the log.
            if (reactedFrame >= 0)
            {
                foreach (var one in laid)
                    if (one.frame > reactedFrame && one.source != LaySource.Reaction)
                        fails.Add($"laid: {one.kind} x{one.stacks} by {one.source} on {Who(one.body)} {(one.frame - reactedFrame) / (float)Rate:0.00} s after the reaction");
                foreach (var one in struckLog)
                {
                    // A blow of no damage (a hold, a shove) is not a hit.
                    if (one.frame <= reactedFrame || one.damage <= 0f) continue;
                    // The reaction's own damage over time is its own, even after its look has gone (noted, not failed).
                    if (effectEnd >= 0 && one.frame >= effectEnd && one.reaction) lateOwn++;
                    else if (effectEnd >= 0 && one.frame >= effectEnd)
                        fails.Add($"hit after the effect ended: {Who(one.body)} {one.damage:0.#} ({(one.reaction ? "reaction" : one.spell ? "spell" : "blow")}) {(one.frame - effectEnd) / (float)Rate:0.00} s after");
                    else if (!one.reaction)
                        fails.Add($"hit not the reaction's: {Who(one.body)} {one.damage:0.#} ({(one.spell ? "spell" : "blow")}) {(one.frame - reactedFrame) / (float)Rate:0.00} s after the reaction");
                }
            }
            else fails.Add("never: the reaction never fired");
            string note = lateOwn > 0 ? $"; note: {lateOwn} blows of the reaction's own after its look ended" : "";
            // One line for each kind of failure is enough to read; the log keeps them all.
            var reasons = fails.GroupBy(f => f.Substring(0, f.IndexOf(':'))).Select(g => g.First() + (g.Count() > 1 ? $" (and {g.Count() - 1} more)" : "")).ToList();
            bool passes = reasons.Count == 0;
            checkedClips++;
            if (passes) passed++;
            var logText = new StringBuilder();
            foreach (var one in laid) logText.AppendLine($"frame {one.frame}: laid {one.kind} x{one.stacks} by {one.source} on {Who(one.body)}");
            foreach (var one in struckLog) logText.AppendLine($"frame {one.frame}: struck {Who(one.body)} {one.damage:0.#} reaction {one.reaction} spell {one.spell}");
            logText.AppendLine($"trigger {trigger}; reacted frame {reactedFrame}; effect ended frame {effectEnd}");
            System.IO.File.WriteAllText(folder + "/log.txt", logText.ToString());
            Say($"{name}: trigger {trigger}, check {(passes ? "PASS" : "FAIL: " + string.Join("; ", reasons))}{note}");
            int lookStart = shownAt >= 0 ? shownAt - 1 : start;
            int impactFrame = lookStart + Mathf.RoundToInt(Mathf.Max(0f, impact) * Rate);
            bool wentOff = shownAt >= 0;
            System.IO.File.WriteAllText(folder + "/info.txt",
                $"name={look}\nkind={kind}\nstart={start}\nshown_frame={lookStart}\nimpact={impact:0.000}\nimpact_frame={impactFrame}\nframes={frame}\nended={(tail >= 0)}\nclean_before={wasClean}\n" +
                $"enemies={count}\nset_off={wentOff}\ntrigger={trigger}\ncheck={(passes ? "PASS" : "FAIL " + string.Join(" | ", reasons))}\n");
            summary.AppendLine($"{name}: {frame} frames, {count} enemies, {(wentOff ? "set off" : "NOT SET OFF")}, impact {impact:0.00} s, {(tail >= 0 ? "ended" : "CUT at the cap")}{(wasClean ? "" : ", NOT clean before")}, {trigger}, {(passes ? "PASS" : "FAIL " + string.Join(" | ", reasons))}");
            Say($"{name}: {kind}, {count} enemies, {frame} frames, impact {impact:0.00} s{(wentOff ? "" : ", WRONG: the reaction did not go off")}{(tail >= 0 ? "" : ", WRONG: cut at the cap")}{(wasClean ? "" : ", WRONG: pool not empty before")}");
            filmed++;
            if (wentOff) setOff++;
            if (group) grouped++;
            yield return null;
        }
        foreach (var entry in Only)
        {
            if (!entry.StartsWith("aff:")) continue;
            yield return FilmAffliction(entry, player, summary);
            filmed++;
        }
        Time.captureFramerate = 0;
        if (Only.Length == 0) System.IO.File.WriteAllText(Root + "summary.txt", summary.ToString());
        Destroy(picture);
        foreach (var pen in pens) pen.Clear();
        foreach (var thing in hidden) if (thing != null) thing.SetActive(true);
        if (reticle != null) reticle.enabled = true;
        foreach (var canvas in huds) if (canvas != null) canvas.enabled = true;
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        foreach (var director in directors) if (director != null) director.enabled = true;
        Say($"filmed {filmed}, set off {setOff}, on a group {grouped}, checks passed {passed} of {checkedClips}");
        Say("done");
        Destroy(gameObject);
    }

    // One affliction alone, laid to the depth named on every enemy stood (the main one and four round it), filmed 3.5 s.
    private IEnumerator FilmAffliction(string entry, PlayerController player, StringBuilder summary)
    {
        var parts = entry.Split(':');
        if (parts.Length < 3 || !System.Enum.TryParse(parts[1], out AfflictionKind kind) || !int.TryParse(parts[2], out int depth)) yield break;
        string name = $"aff_{kind.ToString().ToLowerInvariant()}{depth}";
        string folder = Root + name;
        if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
        System.IO.Directory.CreateDirectory(folder);
        layout.Clear();
        Away = Vector3.right;
        Stand = -3f * Away;
        lane = false;
        player.Teleport(Middle + Stand, Quaternion.LookRotation(Away, Vector3.up));
        int count = Mathf.Min(cast.Count, 5);
        StandCast(count);
        var gameCamera = Camera.main != null ? Camera.main.GetComponent<TopDownCamera>() : null;
        if (gameCamera != null)
        {
            if (Close) gameCamera.SetFraming(CloseDistance / gameCamera.Distance, -Stand);
            else gameCamera.ClearFraming();
        }
        float clear = Time.unscaledTime + 8f;
        while ((!PoolEmpty() || Hazards.Alive > 0) && Time.unscaledTime < clear) yield return null;
        foreach (var enemy in cast) if (enemy != null && enemy.gameObject.activeInHierarchy) enemy.Afflictions.Clear();
        yield return Real(1.8f);
        var main = cast[0];
        var slots = SpellDiamondHud.Instance != null ? SpellDiamondHud.Instance.gameObject : null;
        if (slots != null) slots.SetActive(false);
        var window = Close ? CloseWindow(main.transform.position) : GroupWindow(main.transform.position, count);
        Time.captureFramerate = Rate;
        int frame = 0;
        for (; frame < Before; frame++)
        {
            yield return new WaitForEndOfFrame();
            Grab(folder, frame, window);
            yield return null;
        }
        for (int i = 0; i < count; i++)
            if (cast[i] != null && cast[i].gameObject.activeInHierarchy) cast[i].Afflictions.Lay(kind, depth, 0f, 1f, LaySource.Spell, cast[i]);
        int start = frame;
        while (frame - start < 70)
        {
            yield return new WaitForEndOfFrame();
            Grab(folder, frame, window);
            frame++;
            yield return null;
        }
        Time.captureFramerate = 0;
        if (slots != null) slots.SetActive(true);
        if (gameCamera != null) gameCamera.ClearFraming();
        System.IO.File.WriteAllText(folder + "/info.txt",
            $"name={kind} {depth}\nkind=affliction\nstart={start}\nshown_frame={start}\nimpact=0\nimpact_frame={start}\nframes={frame}\nended=True\nclean_before=True\nenemies={count}\nset_off=True\n");
        Say($"{name}: {count} enemies, {frame} frames, main carries {main.Afflictions.Stacks(kind)}");
    }

    // ---- The v2 rig's log and checks.

    // Elements whose base spell lingers and doses again (Poison's Miasma bites once a second, Fire's Flame Wave leaves
    // burning ground, Life's Briar Patch keeps seeding): cast for real it lays its
    // affliction again after the reaction, the second cast the owner saw. Their affliction is laid once by hand.
    private static readonly string[] LayByHand = { Elements.Poison, Elements.Fire, Elements.Life };

    private static bool Stills => System.IO.File.Exists("AgentScripts/reaction_gif_stills.txt");
    private List<AreaDirector> directors = new List<AreaDirector>();
    private PlayerElement element;
    private bool recording;
    private int clipFrame, reactedFrame = -1, passed, checkedClips;
    private Enemy mainBody;
    private RectInt lastWindow;
    private readonly List<(int frame, int body, AfflictionKind kind, int stacks, LaySource source)> laid = new List<(int, int, AfflictionKind, int, LaySource)>();
    private readonly List<(int frame, int body, float damage, bool reaction, bool spell)> struckLog = new List<(int, int, float, bool, bool)>();

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // The youngest showing of any of these looks on a body, its age in seconds; -1 when none shows.
    private static float YoungestAge(int[] looks, Transform body)
    {
        var instance = typeof(ElementLooks).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        if (instance == null || looks.Length == 0) return -1f;
        var showings = (System.Array)typeof(ElementLooks).GetField("showings", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
        var type = showings.GetType().GetElementType();
        var active = type.GetField("Active");
        var ending = type.GetField("Ending");
        var lookField = type.GetField("Look");
        var bodyField = type.GetField("Body");
        var ageField = type.GetField("Age");
        float youngest = -1f;
        foreach (var one in showings)
        {
            if (!(bool)active.GetValue(one) || (ending != null && (bool)ending.GetValue(one))) continue;
            if (!ReferenceEquals(bodyField.GetValue(one), body) || System.Array.IndexOf(looks, (int)lookField.GetValue(one)) < 0) continue;
            float age = (float)ageField.GetValue(one);
            if (youngest < 0f || age < youngest) youngest = age;
        }
        return youngest;
    }

    private string Who(int body) => body >= 0 && body < cast.Count ? (body == 0 ? "the main one" : cast[body].Kind.Name) : "the player";

    private void OnLaid(AfflictionSet set, Component who, AfflictionKind kind, int stacks, LaySource source)
    {
        if (!recording) return;
        int body = cast.FindIndex(enemy => enemy != null && enemy.Afflictions == set);
        if (body < 0) return;
        laid.Add((clipFrame, body, kind, stacks, source));
    }

    private void OnStruck(Enemy enemy, Hit hit, HitOutcome outcome)
    {
        if (!recording) return;
        struckLog.Add((clipFrame, cast.IndexOf(enemy), hit.Damage, hit.FromReaction, hit.Spell));
    }

    private void OnReacted(Component who, int pair, AfflictionKind laidKind)
    {
        if (!recording || reactedFrame >= 0 || mainBody == null) return;
        if (who == mainBody || (who != null && who.transform == mainBody.transform)) reactedFrame = clipFrame;
    }

    private void OnDestroy()
    {
        Time.captureFramerate = 0;
        CombatEvents.AfflictionLaid -= OnLaid;
        CombatEvents.Reacted -= OnReacted;
        foreach (var enemy in cast) if (enemy != null) enemy.Struck -= OnStruck;
        foreach (var director in directors) if (director != null) director.enabled = true;
    }
}
