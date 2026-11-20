using System;
using System.Collections;
using System.Linq;
using System.Text;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The trial outfit ronin_gen against the current ronin, in the Proving Ground (2026-11-20).
// First the owner's way in: the pause menu's "Trial ronin" row is chosen and pressed, the menu photographed, the
// outfit worn checked, then pressed again to take it off. Then for each body (the current ronin, then ronin_gen):
// stills with time stopped (front, three-quarter and back, close and at the game's own distance), and four clips
// filmed at 20 frames a second (Time.captureFramerate) as the swing GIFs are: the Katana's light chain, the Katana's
// heavy, a dodge roll, and the Great Hammer's light chain and heavy. Close shots bring the game camera in to 5.5 m
// along its own angle. Every frame is a 1:1 window cut from the 1920x1080 game view: 960x540 for clips, 720x720 for
// close stills, 360x360 for stills at the game's distance.
// Frames: review/gifs/outfits/frames/<body>_<clip>/<nnn>.png with info.txt (the step and phase on every frame);
// stills: review/shots/current/outfit_trial/<body>_<pose>.png. Assemble with AgentScripts/outfit_gifs.py.
// Set the game view to 1920x1080 first (AgentScripts/GameViewSize.cs) and back to free after.
// bash AgentScripts/runprobe.sh ProvingGround OutfitTrialProbe 400
public static class OutfitTrialProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Outfit Trial Probe").AddComponent<OutfitTrialProbeRunner>();
        return "probe started";
    }
}

public class OutfitTrialProbeRunner : MonoBehaviour
{
    private const int Rate = 20;
    private const string Frames = "review/gifs/outfits/frames/";
    private const string Stills = "review/shots/current/outfit_trial/";
    private static readonly Vector3 Middle = new Vector3(28f, 0.05f, -2f);
    private const float Near = 5.5f;

    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerOutfit outfit;
    private TopDownCamera follow;
    private float farDistance;
    private Texture2D picture;
    private readonly System.Collections.Generic.List<GameObject> hidden = new System.Collections.Generic.List<GameObject>();
    private readonly System.Collections.Generic.List<Canvas> screens = new System.Collections.Generic.List<Canvas>();
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private bool filming;
    private int frame;
    private string folder;
    private RectInt window;
    private StringBuilder perFrame = new StringBuilder();

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (player != null) player.Stamina.Fill();
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Steps(int count)
    {
        for (int i = 0; i < count; i++) yield return null;
    }

    private IEnumerator Until(Func<bool> condition, int mostFrames)
    {
        for (int i = 0; i < mostFrames && !condition(); i++) yield return null;
    }

    private IEnumerator Tap(params Key[] keys)
    {
        Keys(keys);
        yield return null;
        yield return null;
        Keys();
        yield return null;
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private RectInt WindowAbout(Vector3 point, int width, int height)
    {
        Vector3 onScreen = Camera.main.WorldToScreenPoint(point);
        float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
        int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - width / 2, 0, 1920 - width);
        int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - height / 2, 0, 1080 - height);
        return new RectInt(left, bottom, width, height);
    }

    private void Grab(RectInt cut, string path)
    {
        var shot = ScreenCapture.CaptureScreenshotAsTexture();
        if (picture == null || picture.width != cut.width || picture.height != cut.height)
        {
            if (picture != null) Destroy(picture);
            picture = new Texture2D(cut.width, cut.height, TextureFormat.RGB24, false);
        }
        if (shot.width != 1920 || shot.height != 1080)
        {
            var whole = RenderTexture.GetTemporary(1920, 1080, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(shot, whole);
            var was = RenderTexture.active;
            RenderTexture.active = whole;
            picture.ReadPixels(new Rect(cut.x, cut.y, cut.width, cut.height), 0, 0);
            RenderTexture.active = was;
            RenderTexture.ReleaseTemporary(whole);
        }
        else picture.SetPixels(shot.GetPixels(cut.x, cut.y, cut.width, cut.height));
        picture.Apply();
        Destroy(shot);
        System.IO.File.WriteAllBytes(path, picture.EncodeToPNG());
    }

    private IEnumerator Filmer()
    {
        while (filming)
        {
            yield return new WaitForEndOfFrame();
            if (!filming) break;
            Grab(window, $"{folder}/{frame:000}.png");
            var combo = combat.Weapon != null ? combat.Combo : null;
            perFrame.Append($"{frame}:{(combo != null ? combo.StepIndex : -1)}:{(combo != null ? combo.Phase.ToString() : "-")}:{(player.Dodge.IsRolling ? "roll" : "-")};");
            frame++;
        }
    }

    // Time stopped where it is, the picture taken at the end of the frame.
    private IEnumerator Still(string name, float half)
    {
        float before = Time.timeScale;
        Time.timeScale = 0f;
        yield return null;
        yield return new WaitForEndOfFrame();
        int size = Mathf.RoundToInt(half * 2f);
        Grab(WindowAbout(player.transform.position + Vector3.up * 0.95f, size, size), Stills + name + ".png");
        Time.timeScale = before;
    }

    private void Stand(Vector3 facing)
    {
        player.Teleport(Middle, Quaternion.LookRotation(facing.normalized, Vector3.up));
    }

    private void Zoom(float distance)
    {
        follow.Distance = distance;
        follow.SnapToTarget();
    }

    private string Describe()
    {
        var body = player.GetComponentInChildren<SkinnedMeshRenderer>();
        return $"wears '{outfit.Worn}' rebound {outfit.Rebound} mesh {body.sharedMesh.name} tris {Enumerable.Range(0, body.sharedMesh.subMeshCount).Sum(i => (long)body.sharedMesh.GetIndexCount(i)) / 3} bones {body.bones.Length} " +
               $"materials {string.Join(",", body.sharedMaterials.Select(m => m != null ? m.name : "null"))}";
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        outfit = player.GetComponent<PlayerOutfit>();
        follow = FindAnyObjectByType<TopDownCamera>();
        farDistance = follow.Distance;
        Say($"start: sandboxed {Progress.Sandboxed}, playtest {PlaytestMode.On}, chosen key {Outfits.ChosenKey} '{Outfits.Chosen}', screen {Screen.width}x{Screen.height}; {Describe()}");
        if (!Progress.Sandboxed || outfit == null)
        {
            Say(!Progress.Sandboxed ? "WRONG: progress is not sandboxed, nothing done" : "WRONG: the player has no PlayerOutfit");
            Finish();
            yield break;
        }
        System.IO.Directory.CreateDirectory(Frames);
        System.IO.Directory.CreateDirectory(Stills);

        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        yield return ReadAll();
        yield return Real(0.3f);
        Stand(Vector3.back);
        yield return Real(0.8f);

        yield return PauseSwitch();

        // The floor's painted words and the dummies near the open floor, and the HUD, out of the pictures.
        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 12f).Select(text => text.gameObject));
        hidden.AddRange(FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).Where(dummy => (dummy.transform.position - Middle).magnitude < 12f).Select(dummy => dummy.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace && canvas.enabled)
            {
                canvas.enabled = false;
                screens.Add(canvas);
            }

        foreach (string id in new[] { Outfits.Ronin, Outfits.RoninGen })
        {
            string body = id == Outfits.Ronin ? "ronin_current" : "ronin_gen";
            bool worn = outfit.Wear(id);
            yield return null;
            Say($"{body}: worn {worn}, {Describe()}");
            yield return Body(body);
        }
        Finish();
    }

    // The owner's way to switch: the pause menu's row, as a player would choose and press it.
    private IEnumerator PauseSwitch()
    {
        var menu = PauseMenu.Instance;
        if (menu == null)
        {
            Say("WRONG: no pause menu");
            yield break;
        }
        string wornBefore = outfit.Worn, chosenBefore = Outfits.Chosen;
        menu.Open();
        yield return null;
        int guard = 0;
        while (menu.Choices.Current != PauseEntry.TrialOutfit && guard++ < 20) menu.Move(1);
        var rows = string.Join(" / ", Enumerable.Range(0, menu.Choices.Count).Select(i => PauseChoices.Label(menu.Choices.At(i))));
        Say($"pause rows: {rows}; chosen row {menu.Choices.Current}");
        if (menu.Choices.Current != PauseEntry.TrialOutfit)
        {
            Say("WRONG: the pause menu has no trial outfit row");
            menu.Close();
            yield break;
        }
        menu.Pick();
        yield return null;
        yield return new WaitForEndOfFrame();
        Grab(new RectInt(0, 0, 1920, 1080), Stills + "pause_trial_on.png");
        Say($"pressed once: chosen '{Outfits.Chosen}', worn '{outfit.Worn}' (was '{wornBefore}'), row reads '{PauseChoices.Label(PauseEntry.TrialOutfit)}'");
        menu.Pick();
        yield return null;
        Say($"pressed again: chosen '{Outfits.Chosen}', worn '{outfit.Worn}', last '{Outfits.Last}'");
        menu.Close();
        yield return Real(0.3f);
        Outfits.Chosen = chosenBefore;
    }

    private IEnumerator Body(string body)
    {
        combat.Equip(Weapons.ByName(Weapons.KatanaName));
        yield return Real(0.3f);
        // Stills: close, then at the game's own distance.
        var facings = new (string name, Vector3 facing)[] { ("front", Vector3.back), ("three_quarter", new Vector3(-0.6f, 0f, -1f)), ("back", Vector3.forward), ("side", Vector3.right) };
        foreach (var (name, facing) in facings)
        {
            Stand(facing);
            Zoom(Near);
            yield return Real(0.9f);
            yield return Still($"{body}_idle_{name}_near", 360f);
            Zoom(farDistance);
            yield return Real(0.4f);
            yield return Still($"{body}_idle_{name}_game", 180f);
        }

        Zoom(Near);
        // The Katana's light chain, three-quarter to the camera.
        yield return Clip(body, "chain", new Vector3(-0.6f, 0f, -1f), Chain(Key.J, 0));
        // The Katana's heavy.
        yield return Clip(body, "heavy", new Vector3(-0.6f, 0f, -1f), Chain(Key.K, 1));
        // A roll across the screen, the camera following.
        yield return Clip(body, "roll", Vector3.right, Roll());
        // The Great Hammer: its light chain, then the heavy overhead.
        combat.Equip(Weapons.ByName(Weapons.GreatHammerName));
        yield return Real(0.4f);
        yield return Clip(body, "hammer", new Vector3(-0.6f, 0f, -1f), Hammer());
        combat.Equip(Weapons.ByName(Weapons.KatanaName));
        Zoom(farDistance);
        yield return Real(0.3f);
    }

    // Presses the button, then again as each blow reaches its recovery, until the chain has run out (or once, for presses = 1).
    private IEnumerator Chain(Key key, int presses)
    {
        int most = presses > 0 ? presses : Mathf.Max(1, combat.Combo.Steps.Count);
        var steps = new StringBuilder();
        for (int press = 0; press < most; press++)
        {
            yield return Tap(key);
            yield return Until(() => combat.IsAttacking, 6);
            steps.Append(combat.Combo.StepIndex).Append(combat.Combo.Current != null && combat.Combo.Current.Heavy ? "h " : " ");
            if (press + 1 < most) yield return Until(() => combat.Combo.Phase == AttackPhase.Recovery || !combat.IsAttacking, 30);
        }
        yield return Until(() => !combat.IsAttacking, 60);
        Say($"  {key}: steps {steps}(the chain has {combat.Combo.Steps.Count})");
    }

    private IEnumerator Roll()
    {
        Keys(Key.D);
        yield return Steps(2);
        Keys(Key.D, Key.Space);
        yield return Steps(2);
        Keys(Key.D);
        yield return Until(() => player.Dodge.IsRolling, 6);
        Keys();
        bool rolled = player.Dodge.IsRolling;
        var hips = outfit.GetComponentInChildren<SkinnedMeshRenderer>().bones.FirstOrDefault(bone => bone.name == "hips");
        float lowest = 9f;
        for (int i = 0; i < 30 && player.Dodge.IsRolling; i++)
        {
            lowest = Mathf.Min(lowest, hips.position.y - player.transform.position.y);
            yield return null;
        }
        Say($"  roll: started {rolled}, hips lowest {lowest:0.00} m above the feet");
        yield return Steps(6);
    }

    private IEnumerator Hammer()
    {
        yield return Chain(Key.J, 0);
        yield return Steps(4);
        yield return Chain(Key.K, 1);
    }

    private IEnumerator Clip(string body, string name, Vector3 facing, IEnumerator moves)
    {
        Stand(facing);
        Zoom(Near);
        yield return Real(0.8f);
        folder = Frames + body + "_" + name;
        if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
        System.IO.Directory.CreateDirectory(folder);
        // The roll travels, so its window starts behind the player and the camera follows it there.
        Vector3 centre = player.transform.position + Vector3.up * 0.9f + (name == "roll" ? Vector3.right * 1.2f : Vector3.zero);
        window = WindowAbout(centre, 960, 540);
        perFrame.Clear();
        Time.captureFramerate = Rate;
        frame = 0;
        filming = true;
        StartCoroutine(Filmer());
        yield return Until(() => frame >= 3, 20);
        yield return moves;
        yield return Steps(8);
        filming = false;
        yield return null;
        yield return null;
        Time.captureFramerate = 0;
        System.IO.File.WriteAllText(folder + "/info.txt", $"body={body}\nclip={name}\nframes={frame}\nper_frame={perFrame}\n");
        Say($"{body}_{name}: {frame} frames");
        yield return Real(0.5f);
    }

    private void Finish()
    {
        filming = false;
        Time.captureFramerate = 0;
        if (picture != null) Destroy(picture);
        if (follow != null && farDistance > 0f) Zoom(farDistance);
        foreach (var thing in hidden) if (thing != null) thing.SetActive(true);
        foreach (var canvas in screens) if (canvas != null) canvas.enabled = true;
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say("done");
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.captureFramerate = 0;
}
