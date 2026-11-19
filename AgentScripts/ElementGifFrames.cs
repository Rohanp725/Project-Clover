using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Magic;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;

// Frames for a GIF of each of the eleven elements' own moments, one element carried alone and no reaction, filmed in
// the game with the new effects on (F8 is checked and put on the new if it is not): a copy of the still dummy on the
// open floor at (28, 0, 0), the player west of it facing east, the guide read through first; the floor's painted
// words, the afflicted row (its afflictions are drawn all the time), the sparring dummy (its swing's wedge) and the
// HUD on the screen hidden.
// For each element (attuned alone through PlayerBuild.Attune) six clips, each moment set off through the game's code:
//   affliction  the element's affliction laid three deep on the clean dummy for 3 s, until it wears off by itself;
//   spell       the base spell cast from its slot (PlayerSpells.TryCast) at the dummy 3 m away, flight and landing,
//               with its landing kept from charging the blade (that is the next clip);
//   hit         the blade charged with the element and its three charged blows landing on the dummy until spent;
//   charged     the blade charged and swung three times at the air, turned away from the dummy 8 m behind, then let
//               go plain (the blade only spends a charge on a blow that lands, so it is cleared as a spent one is);
//   infused     the infused strike, L2 + RB on a virtual pad, on the dummy;
//   surge       the surge on F, shortened to 4 s so its lighting, holding and going out fit one GIF.
// In every clip but the affliction's, what the moment lays on the dummy is cleared at once, so each clip shows its own
// moment and the affliction has its own clip. Before each clip it waits until nothing is left from the one before
// (the pools and particles back to what the idle floor shows, no spell running, no hazard, no charge, no surge) and
// a second more.
// Each clip: game time fixed at 20 frames a second (Time.captureFramerate), the game camera's frame grabbed every
// frame, the 960x540 window about the moment cut at 1:1 from the 1920x1080 game view (1200x675 for the spell, the
// infused strike and the surge, which reach far; element_gifs.py brings it down to 960x540), from 0.15 s before the
// moment is set off to 0.4 s after it has fully cleared, the camera holding on where the player stood (it still
// shakes) so the floor does not move. Frames to review/gifs/elements/frames/<element>_<moment>/<nnn>.png with info.txt
// (element, moment, its name, start and peak frames, frame count, whether it ended inside the cap, was clean before
// and was set off).
// Set the game view to 1920x1080 first (AgentScripts/GameViewSize.cs) and back to free after. Assemble with
// AgentScripts/element_gifs.py. AgentScripts/element_gif_only.txt may hold "fire" or "fire_spell" entries, comma
// separated, to film only those. Progress is sandboxed by runprobe.sh; the dummy and the pad are gone after.
// bash AgentScripts/runprobe.sh ProvingGround ElementGifFrames 1200
public static class ElementGifFrames
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Element Gif Frames").AddComponent<ElementGifFramesRunner>();
        return "probe started";
    }
}

public class ElementGifFramesRunner : MonoBehaviour
{
    private static readonly Vector3 Middle = new Vector3(28f, 0f, 0f), Away = Vector3.right;
    private const int Rate = 20, Before = 3, After = 8, MostFrames = 400, Width = 960, Height = 540;
    private const float SurgeSeconds = 4f, AfflictionSeconds = 3f;
    private const string Root = "review/gifs/elements/frames/";
    private static readonly string[] Moments = { "affliction", "spell", "hit", "charged", "infused", "surge" };
    // Seconds from the cast to the spell's strongest moment, by the element's place on the ring (from FxProbe).
    private static readonly float[] SpellPeak = { 0.5f, 0.2f, 0.42f, 0.62f, 0.72f, 1.0f, 0.95f, 0.34f, 0.26f, 0.5f, 1.3f };

    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerSpells spells;
    private PlayerBuild build;
    private TrainingDummy host;
    private TopDownCamera view;
    private Transform anchor;
    private XInputControllerWindows pad;
    private Texture2D picture;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private readonly List<GameObject> hidden = new List<GameObject>();
    private readonly List<Canvas> screens = new List<Canvas>();
    private readonly HashSet<ParticleSystem> idleParticles = new HashSet<ParticleSystem>();
    private int idleSolids, idleMarks, idleRuns, idleHazards;
    private string[] only = new string[0];

    // While filming.
    private bool filming, suppress;
    private int frame;
    private string folder;
    private RectInt window;

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
    }

    // What a moment lays on the dummy goes at once, so the clip is its own moment's; the affliction has its own clip.
    private void LateUpdate()
    {
        if (suppress && host != null && host.Afflictions.Any) host.Afflictions.Clear();
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Frames(int count)
    {
        for (int i = 0; i < count; i++) yield return null;
    }

    private IEnumerator Until(Func<bool> condition, int mostFrames)
    {
        for (int i = 0; i < mostFrames && !condition(); i++) yield return null;
    }

    private IEnumerator UntilReal(Func<bool> condition, float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (!condition() && Time.unscaledTime < end) yield return null;
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

    private void Set(InputControl control, float value)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            ((InputControl<float>)control).WriteValueIntoEvent(value, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    private bool Busy() =>
        FormFx.SolidsShown > idleSolids || FormFx.MarksShown > idleMarks || SpellWorks.Running > idleRuns || Hazards.Alive > idleHazards
        || element.IsCasting || combat.IsAttacking || element.SurgeActive || combat.Blade.Charged || host.Afflictions.Any || Particles() > 0;

    // Particles still alive outside the ones the idle floor always has (the frozen rain): a patch's flames and embers go
    // on a moment after the patch itself has ended.
    private int Particles()
    {
        int alive = 0;
        foreach (var system in FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude))
            if (system.particleCount > 0 && !idleParticles.Contains(system)) alive++;
        return alive;
    }

    private string Why() =>
        $"solids {FormFx.SolidsShown}/{idleSolids}, marks {FormFx.MarksShown}/{idleMarks}, runs {SpellWorks.Running}, hazards {Hazards.Alive}, particles {Particles()}, casting {element.IsCasting}, attacking {combat.IsAttacking}, surge {element.SurgeActive}, charged {combat.Blade.Charged}, afflicted {host.Afflictions.Any}";

    // Where the window sits in the 1920x1080 game view, centred on a point of the world, fixed for the clip: 960x540, or
    // a wider one for the spells, infused strikes and surges, which reach far (element_gifs.py brings it down to 960x540).
    private RectInt WindowAbout(Vector3 point, float wider = 1f)
    {
        var cam = Camera.main;
        Vector3 onScreen = cam.WorldToScreenPoint(point);
        float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
        int width = Mathf.RoundToInt(Width * wider), height = Mathf.RoundToInt(Height * wider);
        int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - width / 2, 0, 1920 - width);
        int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - height / 2, 0, 1080 - height);
        return new RectInt(left, bottom, width, height);
    }

    private void Grab(int index)
    {
        var shot = ScreenCapture.CaptureScreenshotAsTexture();
        if (picture == null || picture.width != window.width || picture.height != window.height)
        {
            if (picture != null) Destroy(picture);
            picture = new Texture2D(window.width, window.height, TextureFormat.RGB24, false);
        }
        if (shot.width != 1920 || shot.height != 1080)
        {
            // Not the size asked for: scaled to it first so the window means the same thing.
            var whole = RenderTexture.GetTemporary(1920, 1080, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(shot, whole);
            var was = RenderTexture.active;
            RenderTexture.active = whole;
            picture.ReadPixels(new Rect(window.x, window.y, window.width, window.height), 0, 0);
            RenderTexture.active = was;
            RenderTexture.ReleaseTemporary(whole);
        }
        else picture.SetPixels(shot.GetPixels(window.x, window.y, window.width, window.height));
        picture.Apply();
        Destroy(shot);
        System.IO.File.WriteAllBytes($"{folder}/{index:000}.png", picture.EncodeToPNG());
    }

    // Grabs every frame, at its end, while the clip is filming.
    private IEnumerator Filmer()
    {
        while (filming)
        {
            yield return new WaitForEndOfFrame();
            if (!filming) break;
            Grab(frame);
            frame++;
        }
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        string onlyPath = "AgentScripts/element_gif_only.txt";
        if (System.IO.File.Exists(onlyPath))
            only = System.IO.File.ReadAllText(onlyPath).Split(',').Select(part => part.Trim().ToLowerInvariant()).Where(part => part.Length > 0).ToArray();
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        Say($"start: sandboxed {Progress.Sandboxed}, element art ready {ElementArt.Ready}, switch {ElementArt.On}");
        if (!Progress.Sandboxed)
        {
            Say("WRONG: progress is not sandboxed, nothing filmed");
            Finish();
            yield break;
        }

        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);

        // The owner's switch on the new effects.
        if (!ElementArt.On) ElementArt.Flip();
        yield return Real(0.2f);
        Say($"new effects on: {ElementArt.On && ElementArt.Ready}");
        if (!ElementArt.Ready)
        {
            Say("WRONG: the new effects are not on, nothing filmed");
            Finish();
            yield break;
        }

        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        if (still == null)
        {
            Say("WRONG: no still dummy");
            Finish();
            yield break;
        }
        host = Instantiate(still.gameObject, Middle, still.transform.rotation).GetComponent<TrainingDummy>();
        host.name = "Element Gif Host";
        // The floor's painted words and the afflicted row (whose afflictions are drawn all the time) are not the moment.
        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 12f).Select(text => text.gameObject));
        hidden.AddRange(FindObjectsByType<AfflictedDummy>(FindObjectsInactive.Exclude).Select(dummy => dummy.gameObject));
        // The sparring dummy's swing is shown by a wedge on the floor that would come and go in the frame.
        hidden.AddRange(FindObjectsByType<DummyAttacker>(FindObjectsInactive.Exclude).Select(dummy => dummy.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);
        // The HUD on the screen (the player's bars, the button prompts, the surge's name) is not the moment either, and
        // the wider windows reach its corners.
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace && canvas.enabled)
            {
                canvas.enabled = false;
                screens.Add(canvas);
            }

        var katana = FindObjectsByType<LoadoutChoice>(FindObjectsInactive.Include).FirstOrDefault(choice => !choice.IsElement && choice.Choice == Weapons.KatanaName);
        if (katana != null)
        {
            player.Teleport(katana.transform.position + Vector3.back * 1f, Quaternion.identity);
            yield return Real(0.35f);
            yield return Tap(Key.E);
            yield return Real(0.3f);
        }
        Say($"weapon: {(combat.Weapon != null ? combat.Weapon.Name : "none")}");
        view = Camera.main != null ? Camera.main.GetComponentInParent<TopDownCamera>() : null;
        if (view == null) view = FindAnyObjectByType<TopDownCamera>();
        anchor = new GameObject("Element Gif Camera Anchor").transform;
        Say($"camera follow found: {view != null}");
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;
        // Spells and blows go where the player faces, not toward the cursor.
        ControlDevices.Notice(pad);

        player.Teleport(Middle + Vector3.left * 3f, Quaternion.LookRotation(Away, Vector3.up));
        yield return Real(2f);
        // What the idle floor shows of the pools, read over a second.
        float end = Time.unscaledTime + 1f;
        while (Time.unscaledTime < end)
        {
            idleSolids = Mathf.Max(idleSolids, FormFx.SolidsShown);
            idleMarks = Mathf.Max(idleMarks, FormFx.MarksShown);
            idleRuns = Mathf.Max(idleRuns, SpellWorks.Running);
            idleHazards = Mathf.Max(idleHazards, Hazards.Alive);
            foreach (var system in FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude))
                if (system.particleCount > 0) idleParticles.Add(system);
            yield return null;
        }
        Say($"screen {Screen.width}x{Screen.height}; idle floor: solids {idleSolids}, marks {idleMarks}, runs {idleRuns}, hazards {idleHazards}, particle systems {idleParticles.Count}");

        var summary = new StringBuilder();
        int filmed = 0, uncut = 0;
        foreach (var info in Elements.All)
        {
            string name = info.Name.ToLowerInvariant();
            build.Attune(info.Name, "");
            yield return Real(0.3f);
            if (!element.HasElement || element.Info == null || element.Info.Name != info.Name || element.Mixed)
            {
                Say($"WRONG: {info.Name} is not carried alone ({(element.Info != null ? element.Info.Name : "none")})");
                continue;
            }
            foreach (var moment in Moments)
            {
                if (only.Length > 0 && !only.Contains(name) && !only.Contains(name + "_" + moment)) continue;
                var result = new Clip();
                yield return Film(info, moment, result);
                summary.AppendLine($"{name}_{moment}: {result.Label}, {result.Frames} frames ({result.Frames / (float)Rate:0.00} s), {(result.Ended ? "ended" : "CUT at the cap")}{(result.CleanBefore ? "" : ", NOT clean before")}{(result.SetOff ? "" : ", NOT set off")}");
                Say($"{name}_{moment}: {result.Label}, {result.Frames} frames, peak {result.Peak}{(result.Ended ? "" : ", WRONG: cut at the cap (" + result.Dirty + ")")}{(result.CleanBefore ? "" : ", WRONG: not clean before (" + result.Dirty + ")")}{(result.SetOff ? "" : ", WRONG: not set off")}");
                filmed++;
                if (result.Ended && result.CleanBefore && result.SetOff) uncut++;
            }
        }
        System.IO.Directory.CreateDirectory(Root);
        System.IO.File.WriteAllText(Root + "summary.txt", summary.ToString());
        Say($"filmed {filmed}, {uncut} whole and clean");
        Finish();
    }

    private class Clip
    {
        public string Label = "";
        public int Start, Peak, Frames;
        public bool Ended, CleanBefore, SetOff;
        public string Dirty = "";
    }

    // The clean start: the dummy and blade plain, the pools back to idle, the player put where the moment wants him.
    private IEnumerator Ready(Vector3 stand, Vector3 facing)
    {
        suppress = false;
        host.Afflictions.Clear();
        combat.Blade.Clear();
        yield return UntilReal(() => !Busy(), 15f);
        host.Afflictions.Clear();
        player.Teleport(stand, Quaternion.LookRotation(facing, Vector3.up));
        yield return UntilReal(() => !combat.IsAttacking && !element.IsCasting && !player.Dodge.IsRolling && player.FormHold == 0 && !player.IsLeaping && !defence.IsDrinking, 6f);
        yield return UntilReal(() => !Busy(), 15f);
        yield return Real(1f);
    }

    private IEnumerator Film(ElementInfo info, string moment, Clip clip)
    {
        string name = info.Name.ToLowerInvariant();
        folder = Root + name + "_" + moment;
        if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
        System.IO.Directory.CreateDirectory(folder);
        Vector3 chest = Vector3.up * 1.2f;

        // Where the player stands and what the window is about.
        Vector3 stand, about, facing = Away;
        switch (moment)
        {
            case "hit": stand = Middle + Vector3.left * 1.6f; about = Middle + Vector3.left * 0.6f + chest; break;
            case "infused": stand = Middle + Vector3.left * 1.8f; about = Middle + Vector3.right * 0.4f + chest; break;
            // Turned away from the dummy, far behind, so no blow can find it.
            case "charged": stand = Middle + Vector3.left * 8f; about = stand + Vector3.left * 1.2f + chest; facing = Vector3.left; break;
            // The surge spreads on the floor round the feet, so the window is about them.
            case "surge": stand = Middle + Vector3.left * 3f; about = Middle + Vector3.left * 2.4f + Vector3.up * 0.4f; break;
            case "spell": stand = Middle + Vector3.left * 3f; about = Middle + Vector3.left * 0.6f + chest; break;
            default: stand = Middle + Vector3.left * 3f; about = Middle + Vector3.left * 0.75f + Vector3.up * 1.4f; break;
        }
        yield return Ready(stand, facing);
        clip.CleanBefore = !Busy();
        if (!clip.CleanBefore) clip.Dirty = Why();
        if (moment == "infused" || moment == "spell" || moment == "surge")
        {
            spells.Cooldowns.ResetAll();
            combat.Mana.Gain(100f);
        }
        // The spell's landing would charge the blade (its own clip): not here, so the spell's clip ends with the spell.
        float windowWas = spells.ChargeWindow;
        if (moment == "spell") spells.ChargeWindow = -1f;
        float surgeWas = element.SurgeKit != null ? element.SurgeKit.SurgeSeconds : 0f;
        if (moment == "surge" && element.SurgeKit != null) element.SurgeKit.SurgeSeconds = SurgeSeconds;

        // The camera holds on where the player stood, so a lunge does not move the floor and the GIF loops on the same
        // ground; it still shakes and closes in as the game makes it.
        if (view != null)
        {
            anchor.position = player.transform.position;
            view.Configure(anchor);
        }
        window = WindowAbout(about, moment == "infused" || moment == "spell" || moment == "surge" ? 1.25f : 1f);
        Time.captureFramerate = Rate;
        frame = 0;
        filming = true;
        StartCoroutine(Filmer());
        yield return Until(() => frame >= Before, 20);
        clip.Start = frame;
        suppress = moment != "affliction";

        switch (moment)
        {
            case "affliction":
                clip.Label = info.Affliction.ToString();
                clip.SetOff = host.Afflictions.Lay(info.Affliction, 3, AfflictionSeconds, 1f, LaySource.Spell, host).Applied;
                clip.Peak = clip.Start + 26;
                break;
            case "spell":
            {
                clip.Label = element.Kit.SpellName;
                int slot = spells.Slots.SlotOf(SpellSlots.BaseId(info.Name));
                clip.SetOff = slot >= 0 ? spells.TryCast(slot) : element.CastBase(info.Name);
                if (slot < 0) clip.Label += " (no slot: cast as the slot would)";
                clip.Peak = clip.Start + Mathf.RoundToInt(SpellPeak[info.Index] * Rate);
                break;
            }
            case "hit":
            {
                clip.Label = "charged blows landing";
                combat.Blade.Charge(info.Name);
                yield return Frames(4);
                int first = -1;
                for (int press = 0; press < 8 && combat.Blade.Charged; press++)
                {
                    int hits = host.HitsTaken;
                    yield return Tap(Key.J);
                    yield return Until(() => host.HitsTaken > hits, 30);
                    if (host.HitsTaken > hits && first < 0) first = frame;
                    yield return Frames(3);
                }
                clip.SetOff = first >= 0 && !combat.Blade.Charged;
                clip.Peak = first >= 0 ? first + 1 : clip.Start + 10;
                break;
            }
            case "charged":
            {
                clip.Label = "charged blade swung at the air";
                combat.Blade.Charge(info.Name);
                yield return Frames(4);
                int first = -1;
                for (int swing = 0; swing < 3; swing++)
                {
                    yield return Tap(Key.J);
                    if (first < 0 && combat.IsAttacking) first = frame;
                    yield return Frames(6);
                }
                yield return Until(() => !combat.IsAttacking, 60);
                yield return Frames(4);
                clip.SetOff = first >= 0;
                combat.Blade.Clear();
                clip.Peak = first >= 0 ? first + 2 : clip.Start + 8;
                break;
            }
            case "infused":
            {
                clip.Label = ElementSteel.StrikeName(info.Name, false);
                int strikes = combat.InfusedStrikes, drawn = StrikeArt.Played;
                Set(pad.leftTrigger, 1f);
                yield return Frames(2);
                Set(pad.rightShoulder, 1f);
                yield return Frames(2);
                Set(pad.rightShoulder, 0f);
                yield return null;
                Set(pad.leftTrigger, 0f);
                yield return Until(() => StrikeArt.Played > drawn, 60);
                clip.SetOff = combat.InfusedStrikes > strikes && StrikeArt.Played > drawn;
                clip.Peak = frame + 3;
                break;
            }
            case "surge":
                clip.Label = element.SurgeKit != null ? element.SurgeKit.SurgeName : "surge";
                yield return Tap(Key.F);
                yield return Until(() => element.SurgeActive, 20);
                clip.SetOff = element.SurgeActive;
                clip.Peak = clip.Start + 22;
                break;
        }

        // Until the moment has cleared, then the tail.
        yield return Until(() => (frame - clip.Start >= 4 && !Busy()) || frame >= MostFrames - After, MostFrames);
        clip.Ended = !Busy();
        if (!clip.Ended) clip.Dirty = Why();
        int stop = frame + After;
        yield return Until(() => frame >= stop || frame >= MostFrames, After + 4);
        filming = false;
        yield return null;
        yield return null;
        if (view != null) view.Configure(player.transform);
        Time.captureFramerate = 0;
        suppress = false;
        if (moment == "surge" && element.SurgeKit != null) element.SurgeKit.SurgeSeconds = surgeWas;
        spells.ChargeWindow = windowWas;
        clip.Frames = frame;
        clip.Peak = Mathf.Clamp(clip.Peak, 0, Mathf.Max(0, frame - 1));
        System.IO.File.WriteAllText(folder + "/info.txt",
            $"element={info.Name}\nmoment={moment}\nlabel={clip.Label}\nstart={clip.Start}\npeak_frame={clip.Peak}\nframes={clip.Frames}\nended={clip.Ended}\nclean_before={clip.CleanBefore}\nset_off={clip.SetOff}\norder={info.Index * Moments.Length + Array.IndexOf(Moments, moment)}\n");
    }

    private void Finish()
    {
        filming = false;
        suppress = false;
        Time.captureFramerate = 0;
        if (picture != null) Destroy(picture);
        if (host != null) Destroy(host.gameObject);
        if (view != null && player != null) view.Configure(player.transform);
        if (anchor != null) Destroy(anchor.gameObject);
        foreach (var thing in hidden) if (thing != null) thing.SetActive(true);
        foreach (var canvas in screens) if (canvas != null) canvas.enabled = true;
        if (combat != null) combat.Blade.Clear();
        if (build != null) build.Attune(Elements.Fire, "");
        if (pad != null) InputSystem.RemoveDevice(pad);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say("done");
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.captureFramerate = 0;
}
