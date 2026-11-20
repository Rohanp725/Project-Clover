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
            text.Append(set.Looks[(int)type.GetField("Look").GetValue(one)].Key).Append(" on ").Append(body != null ? body.name : "-").Append("; ");
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
            if (!lane && !Binding.Contains(look) && layout.Count > 1) window = GroupWindow(main.transform.position, count);
            Say($"{name} window {window.x},{window.y} {window.width}x{window.height}");
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
                // What is drawn 0.8 s in, to tell the reaction's own look from the afflictions' and the game's.
                if (frame - start == 16) Say($"{name} at 0.8 s: {LooksPlaying()}");
                bool longEnough = (frame - start) / (float)Rate >= least;
                if (tail < 0 && frame - start >= 4 && PoolEmpty() && longEnough && (shownAt >= 0 || frame - start > 40)) tail = 0;
                if (tail >= 0 && tail++ >= After) break;
                yield return null;
            }
            Time.captureFramerate = 0;
            if (slots != null) slots.SetActive(true);
            if (gameCamera != null) gameCamera.ClearFraming();
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
        if (reticle != null) reticle.enabled = true;
        foreach (var canvas in huds) if (canvas != null) canvas.enabled = true;
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"filmed {filmed}, set off {setOff}, on a group {grouped}");
        Say("done");
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.captureFramerate = 0;
}
