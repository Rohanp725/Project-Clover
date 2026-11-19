using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Swings;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;

// Frames for a GIF of each weapon swung with each element on it (the swing looks of 2026-11-19), filmed in the Proving
// Ground on real enemies as the owner asked: the castle pen's soldier let out (KnockdownProbe's way) and copied twice,
// the three stood in a spread arc in front of the player, healed, cleared and stood again before each clip, and kept
// from striking back (a wind-up is turned into a moment's pause, so a soldier hit still flinches, freezes, falls and
// is thrown, but the player's swings are never cut short). The guide is read through first; the new effects are put
// on with F8's switch if they are not; the floor's painted words and the HUD are hidden.
// For each weapon (the Katana, the Massive Sword, the Great Hammer and the Twin Knives first, then the Scythe, the Great
// Spear, the Chain Sickle, the Great Bow, bare hands and the Armament; every one equipped through PlayerCombat.Equip)
// and each element on the ring (attuned alone through PlayerBuild.Attune), one clip: the blade charged with the
// element and the light chain pressed three times into the soldiers (the bow adds its shot, the arrow in flight),
// then the infused strike (L2 + RB on a virtual pad), then the clip runs on until the trails, the sheath and the pools
// are back to the idle floor and 0.4 s more.
// Each clip: game time fixed at 20 frames a second (Time.captureFramerate), the game camera's frame grabbed every
// frame, the 960x540 window about the fight cut at 1:1 from the 1920x1080 game view, the camera held on where the
// player stood. Frames to review/gifs/swings/frames/<weapon>_<element>/<nnn>.png with info.txt (weapon, element,
// start and peak frames, frame count, whether it ended inside the cap and was set off, and what SwingTrails counted:
// swings drawn, landings, ground blows, the most trails and sheaths shown, the lines sweeping and how many of them
// were measured onto the prefab). review/gifs/swings/frames/summary.txt lists every clip.
// Set the game view to 1920x1080 first (AgentScripts/GameViewSize.cs) and back to free after. Assemble with
// AgentScripts/swing_gifs.py. AgentScripts/swing_gif_only.txt may hold entries such as "katana", "fire" or
// "katana_fire", comma separated, to film only those. Progress is sandboxed by runprobe.sh.
// bash AgentScripts/runprobe.sh ProvingGround SwingLookProbe 1500
public static class SwingLookProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Swing Look Probe").AddComponent<SwingLookProbeRunner>();
        return "probe started";
    }
}

public class SwingLookProbeRunner : MonoBehaviour
{
    private const int Rate = 20, Before = 3, After = 8, MostFrames = 260, Width = 960, Height = 540;
    private const string Root = "review/gifs/swings/frames/";
    // The weapons in the order they are filmed and set out on the sheet, with the name each folder takes.
    private static readonly (string weapon, string tag, float stand)[] Filmed =
    {
        (Weapons.KatanaName, "katana", 1.9f), (Weapons.MassiveSwordName, "gsword", 2.4f), (Weapons.GreatHammerName, "hammer", 1.9f),
        (Weapons.TwinKnivesName, "knives", 1.4f), (Weapons.ScytheName, "scythe", 2.4f), (Weapons.GreatSpearName, "spear", 2.6f),
        (Weapons.ChainSickleName, "sickle", 1.9f), (Weapons.GreatBowName, "bow", 1.8f), (Weapons.FistsName, "fists", 1.3f),
        (Weapons.ArmamentName, "armament", 1.5f),
    };
    // The middle of the castle pen: the soldiers north of it, the player south, facing north.
    private static readonly Vector3 Middle = new Vector3(-14f, 0.05f, 30f);

    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerSpells spells;
    private PlayerBuild build;
    private SwingTrails trails;
    private EnemyPen castle;
    private readonly List<Enemy> soldiers = new List<Enemy>();
    private readonly List<Enemy> copies = new List<Enemy>();
    private TopDownCamera view;
    private Transform anchor;
    private XInputControllerWindows pad;
    private Texture2D picture;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private readonly List<GameObject> hidden = new List<GameObject>();
    private readonly List<Canvas> screens = new List<Canvas>();
    private int idleSolids, idleMarks, idleHazards;
    private string[] only = new string[0];

    private bool filming, keepCalm;
    private int frame;
    private string folder;
    private RectInt window;
    private int mostRibbons, mostSheaths;

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        foreach (var soldier in soldiers)
        {
            if (soldier == null || soldier.IsDead) continue;
            if (soldier.Health.Fraction < 0.6f) soldier.Health.Fill();
            // A wind-up at the player is turned into a moment's pause: the soldiers take blows and never give them.
            if (keepCalm && soldier.Brain.State == EnemyState.Telegraph) soldier.Brain.Occupy(0.35f);
        }
        if (filming && trails != null)
        {
            mostRibbons = Mathf.Max(mostRibbons, trails.RibbonsShown);
            mostSheaths = Mathf.Max(mostSheaths, trails.SheathsShown);
        }
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

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private bool Busy() =>
        combat.IsAttacking || combat.Blade.Charged || trails.RibbonsShown > 0 || FormFx.SolidsShown > idleSolids || FormFx.MarksShown > idleMarks
        || Hazards.Alive > idleHazards || soldiers.Any(soldier => soldier != null && soldier.Afflictions.Any);

    private string Why() =>
        $"attacking {combat.IsAttacking}, charged {combat.Blade.Charged}, trails {trails.RibbonsShown}, solids {FormFx.SolidsShown}/{idleSolids}, marks {FormFx.MarksShown}/{idleMarks}, hazards {Hazards.Alive}/{idleHazards}, afflicted {soldiers.Count(soldier => soldier != null && soldier.Afflictions.Any)}";

    private RectInt WindowAbout(Vector3 point)
    {
        var cam = Camera.main;
        Vector3 onScreen = cam.WorldToScreenPoint(point);
        float scaleX = 1920f / Screen.width, scaleY = 1080f / Screen.height;
        int left = Mathf.Clamp(Mathf.RoundToInt(onScreen.x * scaleX) - Width / 2, 0, 1920 - Width);
        int bottom = Mathf.Clamp(Mathf.RoundToInt(onScreen.y * scaleY) - Height / 2, 0, 1080 - Height);
        return new RectInt(left, bottom, Width, Height);
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
        string onlyPath = "AgentScripts/swing_gif_only.txt";
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
        trails = player.GetComponent<SwingTrails>();
        Say($"start: sandboxed {Progress.Sandboxed}, swing trails {(trails != null ? (trails.Ready ? "ready" : "NOT ready") : "MISSING")}, switch {ElementArt.On}");
        if (!Progress.Sandboxed || trails == null || !trails.Ready)
        {
            Say(!Progress.Sandboxed ? "WRONG: progress is not sandboxed, nothing filmed" : "WRONG: the player has no working SwingTrails, nothing filmed");
            Finish();
            yield break;
        }
        Say($"blade lines: {trails.SourcesTotal} found on the player, {trails.SourcesFitted} measured onto the prefabs by the builder (the rest from the model notes)");

        float until = Time.unscaledTime + 4f;
        while (!GuidePage.Showing && Time.unscaledTime < until) yield return null;
        yield return ReadAll();
        if (!ElementArt.On) ElementArt.Flip();
        yield return Real(0.2f);

        castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).FirstOrDefault(pen => pen.Land == "The castle");
        if (castle == null)
        {
            Say("WRONG: no castle pen");
            Finish();
            yield break;
        }
        castle.Release(new[] { 0 });
        yield return Real(0.5f);
        var first = castle.Enemies[0];
        if (first == null || first.Kind.Name != EnemyKind.SoldierName)
        {
            Say($"WRONG: the castle pen's first enemy is not a soldier ({(first != null ? first.Kind.Name : "none")})");
            Finish();
            yield break;
        }
        soldiers.Add(first);
        for (int i = 0; i < 2; i++)
        {
            var copy = Instantiate(first.gameObject, first.transform.position + Vector3.right * (i + 1), first.transform.rotation).GetComponent<Enemy>();
            copy.name = "Swing Look Soldier " + (i + 2);
            copies.Add(copy);
            soldiers.Add(copy);
        }
        yield return Real(0.3f);
        Say($"soldiers: {soldiers.Count(soldier => soldier != null && soldier.gameObject.activeInHierarchy)} standing");

        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 14f).Select(text => text.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            if (canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace && canvas.enabled)
            {
                canvas.enabled = false;
                screens.Add(canvas);
            }
        view = Camera.main != null ? Camera.main.GetComponentInParent<TopDownCamera>() : null;
        if (view == null) view = FindAnyObjectByType<TopDownCamera>();
        anchor = new GameObject("Swing Look Camera Anchor").transform;
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;
        ControlDevices.Notice(pad);

        player.Teleport(Middle + Vector3.back * 2f, Quaternion.identity);
        yield return Real(1.5f);
        float end = Time.unscaledTime + 1f;
        while (Time.unscaledTime < end)
        {
            idleSolids = Mathf.Max(idleSolids, FormFx.SolidsShown);
            idleMarks = Mathf.Max(idleMarks, FormFx.MarksShown);
            idleHazards = Mathf.Max(idleHazards, Hazards.Alive);
            yield return null;
        }
        Say($"screen {Screen.width}x{Screen.height}; idle floor: solids {idleSolids}, marks {idleMarks}, hazards {idleHazards}");

        var summary = new StringBuilder();
        int filmed = 0, whole = 0, order = 0;
        foreach (var (weaponName, tag, stand) in Filmed)
        {
            bool weaponWanted = only.Length == 0 || only.Contains(tag) || Elements.All.Any(info => only.Contains(info.Name.ToLowerInvariant()) || only.Contains(tag + "_" + info.Name.ToLowerInvariant()));
            if (!weaponWanted)
            {
                order += Elements.All.Count;
                continue;
            }
            foreach (var info in Elements.All)
            {
                string name = info.Name.ToLowerInvariant();
                int place = order++;
                if (only.Length > 0 && !only.Contains(tag) && !only.Contains(name) && !only.Contains(tag + "_" + name)) continue;
                build.Attune(info.Name, "");
                yield return Real(0.2f);
                // The Armament is made of the element held, so it is taken up after the element.
                combat.Equip(weaponName == Weapons.ArmamentName ? Weapons.Armament(info.Name) : Weapons.ByName(weaponName));
                yield return Real(0.2f);
                if (combat.Weapon == null || combat.Weapon.Name != weaponName)
                {
                    Say($"WRONG: {weaponName} is not in hand ({(combat.Weapon != null ? combat.Weapon.Name : "none")})");
                    continue;
                }
                var clip = new Clip();
                yield return Film(info, weaponName, tag, stand, place, clip);
                summary.AppendLine($"{tag}_{name}: {clip.Frames} frames ({clip.Frames / (float)Rate:0.00} s), blows {clip.Blows}, landings {clip.Landings}, ground {clip.Ground}, trails {clip.Ribbons}, sheaths {clip.Sheaths}, lines {clip.Lines} ({clip.Fitted} measured){(clip.Ended ? "" : ", CUT at the cap")}{(clip.SetOff ? "" : ", NOT set off")}");
                Say($"{tag}_{name}: {clip.Frames} frames, blows {clip.Blows}, landings {clip.Landings}, trails {clip.Ribbons}, sheaths {clip.Sheaths}{(clip.Ended ? "" : ", WRONG: cut at the cap (" + clip.Dirty + ")")}{(clip.SetOff ? "" : ", WRONG: not set off")}");
                filmed++;
                if (clip.Ended && clip.SetOff) whole++;
            }
        }
        System.IO.Directory.CreateDirectory(Root);
        System.IO.File.WriteAllText(Root + "summary.txt", summary.ToString());
        Say($"filmed {filmed}, {whole} whole and set off");
        Finish();
    }

    private class Clip
    {
        public int Start, Peak, Frames, Blows, Landings, Ground, Ribbons, Sheaths, Lines, Fitted;
        public bool Ended, SetOff;
        public string Dirty = "";
    }

    // The soldiers healed, cleared and stood in a spread arc in front of the player; the pools back to the idle floor.
    private IEnumerator Ready(float stand)
    {
        keepCalm = true;
        combat.Blade.Clear();
        yield return UntilReal(() => !combat.IsAttacking, 4f);
        Vector3 at = Middle + Vector3.back * stand;
        player.Teleport(at, Quaternion.identity);
        for (int i = 0; i < soldiers.Count; i++)
        {
            var soldier = soldiers[i];
            if (soldier == null) continue;
            soldier.Restore();
            soldier.Afflictions.Clear();
            float angle = (i - 1) * 32f;
            Vector3 spot = at + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * stand;
            soldier.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(at - spot, Vector3.up));
        }
        yield return UntilReal(() => !Busy(), 8f);
        foreach (var soldier in soldiers) soldier?.Afflictions.Clear();
        yield return Real(0.6f);
    }

    private IEnumerator Film(ElementInfo info, string weaponName, string tag, float stand, int order, Clip clip)
    {
        string name = info.Name.ToLowerInvariant();
        folder = Root + tag + "_" + name;
        if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
        System.IO.Directory.CreateDirectory(folder);
        yield return Ready(stand);
        Vector3 standAt = player.transform.position;
        if (view != null)
        {
            anchor.position = standAt;
            view.Configure(anchor);
        }
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(100f);
        int swingsWere = trails.SwingsDrawn, landingsWere = trails.Landings, groundWere = trails.GroundBlows, blowsWere = combat.BlowsLanded, strikesWere = combat.InfusedStrikes;
        mostRibbons = mostSheaths = 0;
        window = WindowAbout(standAt + Vector3.forward * (stand * 0.6f) + Vector3.up * 0.9f);
        Time.captureFramerate = Rate;
        frame = 0;
        filming = true;
        StartCoroutine(Filmer());
        yield return Until(() => frame >= Before, 20);
        clip.Start = frame;

        // The charge lands and shows on the edge a moment, then the light chain three times into the soldiers.
        combat.Blade.Charge(info.Name);
        yield return Frames(6);
        clip.Lines = trails.SourcesActive;
        int firstBlow = -1;
        for (int press = 0; press < 3; press++)
        {
            int blows = combat.BlowsLanded;
            yield return Tap(Key.J);
            yield return Until(() => combat.BlowsLanded > blows || !combat.IsAttacking, 24);
            if (combat.BlowsLanded > blows && firstBlow < 0) firstBlow = frame;
            yield return Frames(3);
        }
        if (weaponName == Weapons.GreatBowName)
        {
            // The bow's shot: the arrow in flight carries the element.
            yield return Until(() => !combat.IsAttacking, 30);
            yield return Tap(Key.K);
            yield return Frames(14);
        }
        yield return Until(() => !combat.IsAttacking, 40);
        combat.Blade.Clear();
        yield return Frames(6);

        // The infused strike: the carrier swung with the element on it.
        Set(pad.leftTrigger, 1f);
        yield return Frames(2);
        Set(pad.rightShoulder, 1f);
        yield return Frames(2);
        Set(pad.rightShoulder, 0f);
        yield return null;
        Set(pad.leftTrigger, 0f);
        yield return Until(() => combat.InfusedStrikes > strikesWere && !combat.IsAttacking, 50);
        clip.SetOff = trails.SwingsDrawn > swingsWere && combat.BlowsLanded > blowsWere;
        clip.Peak = firstBlow >= 0 ? firstBlow + 1 : clip.Start + 10;

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
        clip.Frames = frame;
        clip.Peak = Mathf.Clamp(clip.Peak, 0, Mathf.Max(0, frame - 1));
        clip.Blows = combat.BlowsLanded - blowsWere;
        clip.Landings = trails.Landings - landingsWere;
        clip.Ground = trails.GroundBlows - groundWere;
        clip.Ribbons = mostRibbons;
        clip.Sheaths = mostSheaths;
        clip.Fitted = trails.SourcesFitted;
        System.IO.File.WriteAllText(folder + "/info.txt",
            $"weapon={weaponName}\ntag={tag}\nelement={info.Name}\nstart={clip.Start}\npeak_frame={clip.Peak}\nframes={clip.Frames}\nended={clip.Ended}\nset_off={clip.SetOff}\n" +
            $"blows={clip.Blows}\nlandings={clip.Landings}\nground={clip.Ground}\nmost_trails={clip.Ribbons}\nmost_sheaths={clip.Sheaths}\nlines={clip.Lines}\nfitted={clip.Fitted}\norder={order}\n");
    }

    private void Finish()
    {
        filming = false;
        keepCalm = false;
        Time.captureFramerate = 0;
        if (picture != null) Destroy(picture);
        foreach (var copy in copies) if (copy != null) Destroy(copy.gameObject);
        if (castle != null) castle.Clear();
        if (view != null && player != null) view.Configure(player.transform);
        if (anchor != null) Destroy(anchor.gameObject);
        foreach (var thing in hidden) if (thing != null) thing.SetActive(true);
        foreach (var canvas in screens) if (canvas != null) canvas.enabled = true;
        if (combat != null)
        {
            combat.Blade.Clear();
            combat.Equip(Weapons.ByName(Weapons.FistsName));
        }
        if (build != null) build.Attune(Elements.Fire, "");
        if (pad != null) InputSystem.RemoveDevice(pad);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say("done");
        Destroy(gameObject);
    }

    private void OnDestroy() => Time.captureFramerate = 0;
}
