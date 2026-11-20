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

// Black Chain in real play: the chain is drawn live between the two bodies the game bound (BoundChains), not toward the
// spot the recipe stood the second body. The Proving Ground's castle soldiers are let out onto the open floor at
// (28, 0, 0), the player 3 m west facing east, as ReactionGifFrames stands them. The struck enemy stands at the middle;
// the second at a place the recipe never stood one (5 m off along the player's way, 3.5 m and more
// from either recipe spot, so the look's old search, 1.6 m about a recipe spot, finds no one there); a third 7.5 m off,
// beyond the bind's 6 m. Cursed and Sundered are laid on the struck one for real, so the game's own Black Chain goes off
// and binds whoever it binds. 2.4 s in, the bound one is sent walking 1.5 m to a new spot, to see the chain follow it.
// Logged every 0.5 s: who the chain binds, where its last link lies against the bound one's chest, and how far the
// bound one stands from the recipe's spots. Frames as ReactionGifFrames films them (20 a second, the 960x540 window
// centred between the two) to review/gifs/reactions/frames/black_chain_live/; strip with
// `python AgentScripts/reaction_strips.py black_chain_live </dev/null`. Set the Game View to 1920x1080 first.
// bash AgentScripts/runprobe.sh ProvingGround ChainProbe 120
public static class ChainProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Chain Probe").AddComponent<ChainProbeRunner>();
        return "probe started";
    }
}

public class ChainProbeRunner : MonoBehaviour
{
    private static readonly Vector3 Middle = new Vector3(28f, 0f, 0f);
    private static readonly Vector3 Stand = new Vector3(-3f, 0f, 0f), Away = Vector3.right;
    // 5 m from the struck one straight along the player's way (east): to its right on screen, 3.5 m and more from both recipe spots.
    private const float Apart = 5f, Angle = 0f, Far = 7.5f;
    private const int Rate = 20, Width = 960, Height = 540, Frames = 130;
    private const string Folder = "review/gifs/reactions/frames/black_chain_live";

    private readonly List<Enemy> cast = new List<Enemy>();
    private readonly List<GameObject> hidden = new List<GameObject>();
    private PlayerDefence defence;
    private Texture2D picture;
    private static readonly FieldInfo SinceHurt = typeof(Enemy).GetField("sinceHurt", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo BarsCalled = typeof(Enemy).GetField("barsCalled", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo HomeField = typeof(Enemy).GetField("home", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo HomeTurnField = typeof(Enemy).GetField("homeRotation", BindingFlags.NonPublic | BindingFlags.Instance);

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        foreach (var enemy in cast)
        {
            if (enemy == null || enemy.IsDead) continue;
            if (enemy.Health.Fraction < 0.5f) enemy.Health.Fill();
            enemy.GuardOpenFor = 5f;
            enemy.Brain.AggroRange = 0f;
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

    private void Place(Enemy enemy, Vector3 at)
    {
        at.y = 0.05f;
        Vector3 look = Middle + Stand - at;
        look.y = 0f;
        HomeField.SetValue(enemy, at);
        HomeTurnField?.SetValue(enemy, Quaternion.LookRotation(look.normalized, Vector3.up));
        enemy.Restore();
        enemy.Brain.AggroRange = 0f;
        enemy.Brain.LeashRange = 0.5f;
        enemy.WakeNow();
    }

    private RectInt Window(Vector3 host)
    {
        Vector3 onScreen = Camera.main.WorldToScreenPoint(host + Vector3.up * 1.2f);
        float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
        int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - Width / 2, 0, 1920 - Width);
        int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - Height / 2, 0, 1080 - Height);
        return new RectInt(left, bottom, Width, Height);
    }

    private void Grab(int index, RectInt window)
    {
        var shot = ScreenCapture.CaptureScreenshotAsTexture();
        picture.SetPixels(shot.GetPixels(window.x, window.y, Width, Height));
        picture.Apply();
        Destroy(shot);
        System.IO.File.WriteAllBytes($"{Folder}/{index:000}.png", picture.EncodeToPNG());
    }

    // The recipe's spots for Black Chain's other bodies, turned as the game turns the look (its player faced the main
    // body; this one faces east), from the main body.
    private static List<Vector3> RecipeSpots()
    {
        var spots = new List<Vector3>();
        var text = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(Clover.EditorTools.ElementLookBuilder.ReactionRecipePath);
        foreach (Dictionary<string, object> look in (List<object>)((Dictionary<string, object>)Clover.EditorTools.ReactionRecipe.ReadJson(text.text))["looks"])
        {
            if ((string)look["slug"] != "black_chain") continue;
            var bodies = ((List<object>)look["bodies"]).Select(body => Vec(((Dictionary<string, object>)body)["position"])).ToList();
            Vector3 toward = bodies[0] - Vec(((Dictionary<string, object>)look["player"])["position"]);
            var turn = Quaternion.Euler(0f, Mathf.Atan2(Away.x, Away.z) * Mathf.Rad2Deg - Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg, 0f);
            for (int i = 1; i < bodies.Count; i++) spots.Add(turn * (bodies[i] - bodies[0]));
        }
        return spots;
    }

    private static Vector3 Vec(object value)
    {
        var list = (List<object>)value;
        return new Vector3(System.Convert.ToSingle(list[0]), System.Convert.ToSingle(list[1]), System.Convert.ToSingle(list[2]));
    }

    private string Report(Vector3 middleSpot, List<Vector3> spots, string when)
    {
        if (!BoundChains.TryChain(0, out var host, out var bound, out var firstLink, out var lastLink, out int links))
            return $"{when}: no chain drawn (chains {BoundChains.Drawn})";
        Vector3 chest = bound.position + Vector3.up * 1.12f;
        Vector3 hostChest = host.position + Vector3.up * 1.12f;
        float fromSpot = spots.Count > 0 ? spots.Min(spot => Vector3.Distance(Flat(bound.position), Flat(Middle + spot))) : -1f;
        return $"{when}: chain {host.name} -> {bound.name}, {links} links; first link {Vector3.Distance(firstLink, hostChest):0.00} m from the struck chest, " +
            $"last link {Vector3.Distance(lastLink, chest):0.00} m from the bound chest (bound at {bound.position.x:0.00}, {bound.position.z:0.00}; " +
            $"{Vector3.Distance(Flat(bound.position), Flat(host.position)):0.00} m apart; {fromSpot:0.00} m from the nearest recipe spot)";
    }

    private static Vector3 Flat(Vector3 at) => new Vector3(at.x, 0f, at.z);

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        var backgroundWas = InputSystem.settings.backgroundBehavior;
        var editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        var reactions = player.GetComponent<PlayerReactions>();
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
            foreach (var enemy in pen.Enemies)
            {
                if (enemy == null) continue;
                if (cast.Count < 3 && pen.Land == "The castle" && enemy.Kind.Name != "Gate Captain") cast.Add(enemy);
                else enemy.gameObject.SetActive(false);
            }
        }
        if (!Progress.Sandboxed || cast.Count < 3 || !BoundChains.Ready)
        {
            Say($"WRONG: sandboxed {Progress.Sandboxed}, enemies {cast.Count}, live chains {BoundChains.Ready}");
            Say("done");
            Destroy(gameObject);
            yield break;
        }
        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 14f).Select(text => text.gameObject));
        hidden.AddRange(FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).Where(dummy => (dummy.transform.position - Middle).magnitude < 14f).Select(dummy => dummy.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);

        var spots = RecipeSpots();
        Vector3 second = Quaternion.Euler(0f, -Angle, 0f) * Vector3.right * Apart;
        Vector3 third = Quaternion.Euler(0f, -70f, 0f) * Vector3.right * Far;
        Say($"recipe spots {string.Join(" ", spots.Select(spot => $"({spot.x:0.00}, {spot.z:0.00})"))}; second stood at ({second.x:0.00}, {second.z:0.00}), " +
            $"{spots.Min(spot => Vector3.Distance(spot, second)):0.00} m from the nearest; third {Far} m off");
        player.Teleport(Middle + Stand, Quaternion.LookRotation(Away, Vector3.up));
        Place(cast[0], Middle);
        Place(cast[1], Middle + second);
        Place(cast[2], Middle + third);
        yield return Real(2f);
        foreach (var enemy in cast) enemy.Afflictions.Clear();
        yield return Real(1f);
        Say($"cast: struck {cast[0].name}, second {cast[1].name} at {Vector3.Distance(Flat(cast[1].transform.position), Flat(cast[0].transform.position)):0.00} m, " +
            $"third {cast[2].name} at {Vector3.Distance(Flat(cast[2].transform.position), Flat(cast[0].transform.position)):0.00} m");

        System.IO.Directory.CreateDirectory(Folder);
        foreach (var old in System.IO.Directory.GetFiles(Folder)) System.IO.File.Delete(old);
        picture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        var slots = SpellDiamondHud.Instance != null ? SpellDiamondHud.Instance.gameObject : null;
        if (slots != null) slots.SetActive(false);
        // Centred between the two, so both ends of the chain are in it.
        var window = Window(Middle + second * 0.5f);
        int pair = AfflictionPairs.Index(AfflictionKind.Sundered, AfflictionKind.Cursed);
        int shownBefore = reactions.TimesShown(pair);
        Time.captureFramerate = Rate;
        const int Before = 3;
        for (int frame = 0; frame < Frames; frame++)
        {
            yield return new WaitForEndOfFrame();
            Grab(frame, window);
            if (frame == Before - 1)
            {
                cast[0].Afflictions.Lay(AfflictionPairs.First(pair), 2, 0f, 1f, LaySource.Spell, cast[0]);
                cast[0].Afflictions.Lay(AfflictionPairs.Second(pair), 2, 0f, 1f, LaySource.Spell, cast[0]);
            }
            int since = frame - Before;
            if (since > 0 && since % 10 == 0) Say(Report(Middle, spots, $"{since / (float)Rate:0.0} s"));
            // Sent walking to a new spot 1.5 m round, so the chain is seen to follow it.
            if (since == 48)
            {
                Vector3 moved = Middle + Quaternion.Euler(0f, -20f, 0f) * second;
                HomeField.SetValue(cast[1], new Vector3(moved.x, 0.05f, moved.z));
                cast[1].Brain.LeashRange = 0.2f;
                Say($"2.4 s: {cast[1].name} sent to ({moved.x - Middle.x:0.00}, {moved.z - Middle.z:0.00}) from the struck one");
            }
            yield return null;
        }
        Time.captureFramerate = 0;
        if (slots != null) slots.SetActive(true);
        System.IO.File.WriteAllText(Folder + "/info.txt", $"name=Black Chain (bound one stood where the recipe stood no one)\nkind=bespoke\nstart={Before}\nshown_frame={Before}\nframes={Frames}\nenemies=3\nset_off={reactions.TimesShown(pair) > shownBefore}\n");
        Say($"set off {reactions.TimesShown(pair) > shownBefore}, bespoke plays {ReactionLook.Bespoke}, chains now {BoundChains.Drawn}");
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
