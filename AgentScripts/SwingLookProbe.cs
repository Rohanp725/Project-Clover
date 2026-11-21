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
// Ground on real enemies as the owner asked, the reaction GIFs' rig (ReactionGifFrames): the pens' enemies let out, the
// castle's first three (soldier, acolyte, caster) stood on the open floor at (28, 0, 0) in a spread arc in front of
// the player, the rest put away. They are kept alive (healed), kept from chasing or striking (aggro range zero, leash
// a step, their home made their spot so putting them back stands them there), can never guard, and keep their bars
// up; they still flinch, freeze, fall and are thrown as the game makes them. Before each clip they are put back,
// cleared and stood again. The guide is read through first; the new effects are put on with F8's switch if they are
// not; the floor's painted words, the training dummies near the floor and the HUD are hidden.
// For each weapon (the Katana, the Massive Sword, the Great Hammer and the Twin Knives first, then the Scythe, the Great
// Spear, the Chain Sickle, the Great Bow, bare hands, the Armament and last the War Fan, which has no moves yet and is
// swung on the Katana's under its own carry, its prefab hung on the rack for the play; every one equipped through PlayerCombat.Equip)
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
    // Clips with the enemies stood off (swing_gif_alone.txt) go to their own folder, so the grid on the enemies is kept.
    private string Root => alone ? "review/gifs/swings/alone/" : "review/gifs/swings/frames/";
    // The weapons in the order they are filmed and set out on the sheet, with the name each folder takes.
    private static readonly (string weapon, string tag, float stand)[] Filmed =
    {
        (Weapons.KatanaName, "katana", 1.9f), (Weapons.MassiveSwordName, "gsword", 2.4f), (Weapons.GreatHammerName, "hammer", 1.9f),
        (Weapons.TwinKnivesName, "knives", 1.4f), (Weapons.ScytheName, "scythe", 2.4f), (Weapons.GreatSpearName, "spear", 2.6f),
        (Weapons.ChainSickleName, "sickle", 1.9f), (Weapons.GreatBowName, "bow", 1.8f), (Weapons.FistsName, "fists", 1.3f),
        (Weapons.ArmamentName, "armament", 1.5f), (FanName, "fan", 1.9f),
    };
    // The War Fan has its prefab and blade line but no Weapon and no moves yet: it is filmed on the Katana's moves
    // (the nearest one-handed cuts) under its own carry, so the fan's line, shape and looks are what is seen.
    private const string FanName = "War Fan";
    // The open floor the reaction GIFs are filmed on: the enemies north of it, the player south, facing north.
    private static readonly Vector3 Middle = new Vector3(28f, 0.05f, 0f);
    private const int CastCount = 3;
    private static readonly System.Reflection.FieldInfo SinceHurt = typeof(Enemy).GetField("sinceHurt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private static readonly System.Reflection.FieldInfo BarsCalled = typeof(Enemy).GetField("barsCalled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private static readonly System.Reflection.FieldInfo HomeField = typeof(Enemy).GetField("home", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private static readonly System.Reflection.FieldInfo FlashLeft = typeof(Enemy).GetField("flashLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private static readonly System.Reflection.FieldInfo FlashColourField = typeof(Enemy).GetField("flashColour", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    private static readonly System.Reflection.FieldInfo HomeTurnField = typeof(Enemy).GetField("homeRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerSpells spells;
    private PlayerBuild build;
    private SwingTrails trails;
    private readonly List<EnemyPen> pens = new List<EnemyPen>();
    private readonly List<Enemy> soldiers = new List<Enemy>();
    private TopDownCamera view;
    private Transform anchor;
    private XInputControllerWindows pad;
    private Texture2D picture;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private readonly List<GameObject> hidden = new List<GameObject>();
    private readonly List<Canvas> screens = new List<Canvas>();
    private TargetReticle reticle;
    private int idleSolids, idleMarks, idleHazards;
    private string[] only = new string[0];
    // AgentScripts/swing_gif_alone.txt present: the enemies stand 30 m off, so the swing is seen with no blow landing.
    private bool alone;

    private bool filming, keepCalm;
    private Vector3 filmStand;
    private int frame;
    private string folder;

    // What is thrown or begun on the floor each frame of a clip, to trace a ring at the feet to its source: births in the
    // ring emitters (Ring, TintRing) with their place and colour, and element looks begun (their key and body). Written
    // to <clip>/rings.txt.
    private readonly StringBuilder ringLog = new StringBuilder();
    private ParticleSystem[] ringEmitters, allEmitters;
    private readonly HashSet<Enemy> staggeredWas = new HashSet<Enemy>();
    private readonly int[] ringCounts = new int[2];
    private readonly ParticleSystem.Particle[] ringParticles = new ParticleSystem.Particle[256];
    private readonly HashSet<int> stampsSeen = new HashSet<int>();

    private void LogRings()
    {
        if (ringEmitters == null)
        {
            var vfx = FindAnyObjectByType<Vfx>();
            var all = vfx != null ? (ParticleSystem[])typeof(Vfx).GetField("emitters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(vfx) : null;
            if (all == null) return;
            ringEmitters = new[] { all[(int)VfxKind.Ring], all[(int)VfxKind.TintRing] };
            allEmitters = all;
        }
        // Every effect thrown this frame by kind, with its colour and place, to trace a flash to its source.
        for (int e = 0; allEmitters != null && e < allEmitters.Length; e++)
        {
            var emitter = allEmitters[e];
            if (emitter == null || e == (int)VfxKind.Ring || e == (int)VfxKind.TintRing) continue;
            int count = emitter.GetParticles(ringParticles);
            int born = 0;
            Color first = Color.clear;
            Vector3 where = Vector3.zero;
            for (int q = 0; q < count; q++)
            {
                if (ringParticles[q].startLifetime - ringParticles[q].remainingLifetime > Time.deltaTime * 1.5f) continue;
                if (born++ == 0)
                {
                    first = ringParticles[q].startColor;
                    where = ringParticles[q].position - (player != null ? player.transform.position : Vector3.zero);
                }
            }
            if (born > 0) ringLog.AppendLine($"f{frame} vfx {(VfxKind)e} x{born} at {where} from player, colour {first.r:0.00},{first.g:0.00},{first.b:0.00}");
        }
        foreach (var soldier in soldiers)
            if (soldier != null && soldier.IsStaggered != staggeredWas.Contains(soldier))
            {
                if (soldier.IsStaggered) staggeredWas.Add(soldier); else staggeredWas.Remove(soldier);
                ringLog.AppendLine($"f{frame} finish mark {(soldier.IsStaggered ? "shown" : "hidden")} on {soldier.Kind.Name} at {soldier.transform.position - player.transform.position} from player");
            }
        for (int e = 0; e < ringEmitters.Length; e++)
        {
            var emitter = ringEmitters[e];
            if (emitter == null) continue;
            int count = emitter.GetParticles(ringParticles);
            for (int p = 0; p < count; p++)
            {
                if (ringParticles[p].startLifetime - ringParticles[p].remainingLifetime > Time.deltaTime * 1.5f) continue;
                var c = ringParticles[p].startColor;
                ringLog.AppendLine($"f{frame} {(e == 0 ? "Ring" : "TintRing")} at {ringParticles[p].position - (player != null ? player.transform.position : Vector3.zero)} from player, colour {c.r},{c.g},{c.b}, size {ringParticles[p].startSize:0.00}");
            }
        }
        var looksField = typeof(ElementLooks).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var looks = looksField.GetValue(null);
        if (looks == null) return;
        var showings = (System.Array)typeof(ElementLooks).GetField("showings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(looks);
        var set = (ElementLookSet)typeof(ElementLooks).GetField("set", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(looks);
        var type = showings.GetType().GetElementType();
        foreach (var one in showings)
        {
            if (!(bool)type.GetField("Active").GetValue(one)) continue;
            int stamp = (int)type.GetField("Stamp").GetValue(one);
            if (!stampsSeen.Add(stamp)) continue;
            var body = (Transform)type.GetField("Body").GetValue(one);
            ringLog.AppendLine($"f{frame} look {set.Looks[(int)type.GetField("Look").GetValue(one)].Key} on {(body != null ? body.name : "the ground")}, swept {(trails != null ? trails.Swept : 0f):0} degrees, mirrored {((Vector3)type.GetField("Mirror").GetValue(one)).sqrMagnitude > 0.5f}");
        }
    }
    private RectInt window;
    private int mostRibbons, mostSheaths, mostBladeSheaths;
    // The frames on which a cut streak was drawn (CutMarks.Drawn rose) or a blow landed (Wind's draw no streak): the
    // moments of contact, for the strips.
    private readonly List<int> contacts = new List<int>();
    private int drawnWas, landedWas;
    // Frames on which an enemy showed a struck flash, and of those how many were plain white (the blow carried no tint).
    private int flashFrames, whiteFlashFrames;
    private readonly List<int> whiteAt = new List<int>();

    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        foreach (var soldier in soldiers)
        {
            if (soldier == null || soldier.IsDead) continue;
            if (soldier.Health.Fraction < 0.6f) soldier.Health.Fill();
            soldier.GuardOpenFor = 5f;
            soldier.Brain.AggroRange = 0f;
            soldier.Brain.LeashRange = 0.5f;
            SinceHurt.SetValue(soldier, 0f);
            BarsCalled.SetValue(soldier, true);
            // Should a wind-up start all the same, it is turned into a moment's pause: the player's swings are never cut short.
            if (keepCalm && soldier.Brain.State == EnemyState.Telegraph) soldier.Brain.Occupy(0.35f);
        }
        if (filming && FlashLeft != null && FlashColourField != null)
        {
            bool flashed = false, white = false;
            foreach (var soldier in soldiers)
            {
                if (soldier == null || (float)FlashLeft.GetValue(soldier) <= 0f) continue;
                flashed = true;
                white |= (Color)FlashColourField.GetValue(soldier) == Enemy.StruckFlash;
            }
            if (flashed) flashFrames++;
            if (white) { whiteFlashFrames++; if (whiteAt.Count < 40) whiteAt.Add(frame); }
        }
        if (filming && trails != null)
        {
            mostRibbons = Mathf.Max(mostRibbons, trails.RibbonsShown);
            mostSheaths = Mathf.Max(mostSheaths, trails.SheathsShown);
            mostBladeSheaths = Mathf.Max(mostBladeSheaths, trails.BladeSheathsShown);
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
        // A guide opened as a list (the playtest's guide since Phase 4b) turns its pages on E and never closes on it:
        // the back key closes it, and failing that it is closed outright, so time is never left stopped.
        guard = 0;
        while (GuidePage.Showing && guard++ < 5) yield return Tap(Key.Escape);
        if (GuidePage.Showing) GuidePage.Instance.Close();
        // Proof that the game runs again, not only that the page is shut.
        float timeWas = Time.time;
        yield return Real(0.3f);
        Debug.Log($"[Probe] guide closed {!GuidePage.Showing}, time {timeWas:0.00} -> {Time.time:0.00}" + (Time.time > timeWas ? "" : ", WRONG: time is not moving"));
    }

    private bool Busy() =>
        combat.IsAttacking || combat.Blade.Charged || trails.RibbonsShown > 0 || FormFx.SolidsShown > idleSolids || FormFx.MarksShown > idleMarks
        || Hazards.Alive > idleHazards || StrikeScorch.Showing > 0 || soldiers.Any(soldier => soldier != null && soldier.Afflictions.Any);

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
            // With no one to stop it, Lightning's infused strike carries the player out of a held camera and the clip
            // ends on empty floor (the alone films of 2026-11-20): alone, the camera follows the player.
            // Into enemies the camera keeps half way between where the player stood and where they are, so a lunge
            // that carries the player past the enemies keeps both in the frame (the films of 2026-11-21).
            if (anchor != null && player != null) anchor.position = alone ? player.transform.position : Vector3.Lerp(filmStand, player.transform.position, 0.5f);
            if (CutMarks.Drawn != drawnWas || combat.BlowsLanded != landedWas)
            {
                drawnWas = CutMarks.Drawn;
                landedWas = combat.BlowsLanded;
                contacts.Add(frame);
            }
            LogRings();
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
        alone = System.IO.File.Exists("AgentScripts/swing_gif_alone.txt");
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

        // The castle's enemies first, as the reaction rig takes them; every other one let out is put away.
        // The Storm Caller is left out: its own white-blue bolt fell into the strike films of 2026-11-21. Should the castle
        // have fewer than three others, the nearest kinds from the other pens fill in.
        pens.AddRange(FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).OrderBy(pen => pen.Land == "The castle" ? 0 : 1));
        foreach (var pen in pens)
        {
            pen.Release(Enumerable.Range(0, pen.Enemies.Length).ToArray());
            foreach (var enemy in pen.Enemies)
            {
                if (enemy == null) continue;
                if (soldiers.Count < CastCount && enemy.Kind.Name != EnemyKind.CasterName && !enemy.Kind.Name.Contains("Caller")) soldiers.Add(enemy);
                else enemy.gameObject.SetActive(false);
            }
        }
        if (soldiers.Count == 0)
        {
            Say("WRONG: no castle enemies in the pens");
            Finish();
            yield break;
        }
        yield return Real(0.3f);
        Say($"cast: {string.Join(", ", soldiers.Select(soldier => soldier.Kind.Name))}");

        hidden.AddRange(FindObjectsByType<TextMesh>(FindObjectsInactive.Exclude).Where(text => (text.transform.position - Middle).magnitude < 14f).Select(text => text.gameObject));
        hidden.AddRange(FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).Where(dummy => (dummy.transform.position - Middle).magnitude < 14f).Select(dummy => dummy.gameObject));
        foreach (var thing in hidden) thing.SetActive(false);
        // The player's target brackets under the enemy aimed at are the game's aim, not the swing: on film they read as
        // a dashed ring round the middle enemy's feet that flashes white on every blow (the swing films of 2026-11-20).
        // Put away while filming, as the reaction films do.
        reticle = player.GetComponent<TargetReticle>();
        var reticleRing = typeof(TargetReticle).GetField("ringRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(reticle) as Renderer;
        if (reticle != null) reticle.enabled = false;
        if (reticleRing != null) reticleRing.enabled = false;
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
        if (System.IO.File.Exists("AgentScripts/swing_soak.txt"))
        {
            yield return Soak();
            Finish();
            yield break;
        }

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
            if (weaponName == FanName && !HangFan())
            {
                Say("WRONG: the War Fan's prefab could not be hung on the player; build it with Clover > Weapons > Build Weapon Prefabs");
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
                combat.Equip(weaponName == Weapons.ArmamentName ? Weapons.Armament(info.Name) : weaponName == FanName ? FanOnKatanaMoves() : Weapons.ByName(weaponName));
                yield return Real(0.2f);
                if (combat.Weapon == null || combat.Weapon.Name != weaponName)
                {
                    Say($"WRONG: {weaponName} is not in hand ({(combat.Weapon != null ? combat.Weapon.Name : "none")})");
                    continue;
                }
                var clip = new Clip();
                yield return Film(info, weaponName, tag, stand, place, clip);
                summary.AppendLine($"{tag}_{name}: {clip.Frames} frames ({clip.Frames / (float)Rate:0.00} s), blows {clip.Blows}, landings {clip.Landings}, ground {clip.Ground}, trails {clip.Ribbons}, sheaths {clip.Sheaths}, blade sheaths {clip.BladeSheaths}, lines {clip.Lines} ({clip.Fitted} measured), flashes {flashFrames} ({whiteFlashFrames} white){(clip.Ended ? "" : ", CUT at the cap")}{(clip.SetOff ? "" : ", NOT set off")}");
                Say($"{tag}_{name}: {clip.Frames} frames, blows {clip.Blows}, landings {clip.Landings}, trails {clip.Ribbons}, sheaths {clip.Sheaths}, blade sheaths {clip.BladeSheaths}{(clip.Ended ? "" : ", WRONG: cut at the cap (" + clip.Dirty + ")")}{(clip.SetOff ? "" : ", WRONG: not set off")}");
                filmed++;
                if (clip.Ended && clip.SetOff) whole++;
            }
        }
        System.IO.Directory.CreateDirectory(Root);
        System.IO.File.WriteAllText(Root + "summary.txt", summary.ToString());
        Say($"filmed {filmed}, {whole} whole and set off");
        Finish();
    }

    // The Katana's light chain, heavy and infused strike under the fan's carry, so the rack shows the fan and the
    // trails follow its line with its swing shape; the Katana's sheath and drawn sword stay hidden.
    private static Weapon FanOnKatanaMoves()
    {
        var weapon = Weapons.Katana();
        weapon.UsesSword = false;
        weapon.Stowed = false;
        weapon.Name = FanName;
        weapon.Carry = BladeLines.FanCarry;
        return weapon;
    }

    // A player built before the fan's Spec has no fan on its rack: its prefab is hung in the right hand as the builder
    // would (WeaponBuilder.Hang), added to the rack's sets, and the trails look for lines again.
    private bool HangFan()
    {
        var rack = player.GetComponentInChildren<WeaponRack>();
        if (rack == null) return false;
        if (rack.Has(BladeLines.FanCarry)) return true;
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Generated/Weapons/fan/fan.prefab");
        var hand = WeaponRack.FindSocket(rack.transform, WeaponRack.HandRight);
        if (prefab == null || hand == null) return false;
        var fan = Instantiate(prefab, hand);
        fan.name = "fan_hand";
        fan.transform.localPosition = Vector3.zero;
        fan.transform.localRotation = Quaternion.identity;
        var tinted = new List<Renderer>();
        var indices = new List<int>();
        var glow = new List<float>();
        var plain = new List<Color>();
        foreach (var renderer in fan.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;
                bool inlay = materials[i].name.EndsWith("_inlay"), accent = materials[i].name.EndsWith("_accent");
                if (!inlay && !accent) continue;
                tinted.Add(renderer);
                indices.Add(i);
                glow.Add(inlay ? 1f : 0f);
                plain.Add(materials[i].GetColor("_BaseColor"));
            }
        }
        fan.SetActive(false);
        var setsField = typeof(WeaponRack).GetField("sets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var sets = ((WeaponRack.Set[])setsField.GetValue(rack)).ToList();
        sets.Add(new WeaponRack.Set
        {
            Carry = BladeLines.FanCarry, InHand = new[] { fan }, AtRest = new GameObject[0],
            Tinted = tinted.ToArray(), Index = indices.ToArray(), Glow = glow.ToArray(), Plain = plain.ToArray(),
        });
        rack.Configure(sets.ToArray());
        typeof(SwingTrails).GetMethod("FindSources", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(trails, null);
        Say($"War Fan hung on the rack for this play; blade lines now {trails.SourcesTotal} ({trails.SourcesFitted} measured)");
        return rack.Has(BladeLines.FanCarry);
    }

    private class Clip
    {
        public int Start, Peak, Frames, Blows, Landings, Ground, Ribbons, Sheaths, BladeSheaths, Lines, Fitted;
        public bool Ended, SetOff;
        public string Dirty = "";
    }

    // The soldiers healed, cleared and stood in a spread arc in front of the player; the pools back to the idle floor.
    // The swing soak (AgentScripts/swing_soak.txt): the struck tint's calls measured directly, then 20 s of the Katana
    // swung into the soldiers with Fire, Lightning, Ice and Poison in turn (charged chain and infused strike) and plain
    // chains between, for the profiler to record. Nothing is filmed.
    private IEnumerator Soak()
    {
        yield return Ready(1.9f);
        var soldier = soldiers[0];
        Color tint = SwingLooks.Fire.Tint;
        Vector3 at = soldier.transform.position + Vector3.up;
        // Warm every path once so first-call costs are not counted.
        CutMarks.Light(at, Vector3.right, tint); CutMarks.Light(at, Vector3.right); _ = combat.StruckTint;
        _ = StruckLook.Flash(Enemy.StruckFlash, tint); _ = StruckLook.Stroke(Color.white, tint); _ = StruckLook.Ink(Color.black, tint);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
        {
            CutMarks.Light(at, Vector3.right, tint);
            CutMarks.Heavy(at, Vector3.right, tint);
            CutMarks.Finisher(at, Vector3.right, tint);
            CutMarks.Light(at, Vector3.right);
        }
        long marks = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        Color sum = Color.black;
        for (int i = 0; i < 1000; i++)
        {
            sum += combat.StruckTint;
            sum += StruckLook.Flash(Enemy.StruckFlash, tint) + StruckLook.Stroke(Color.white, tint) + StruckLook.Ink(Color.black, tint) + Enemy.FlashColour(tint);
        }
        long looks = GC.GetAllocatedBytesForCurrentThread() - before;
        // A blow with a tint against the same blow without: what the tint adds to TakeHit and the flash.
        var plainHit = new Hit(0.01f, 0f, Vector3.forward);
        var tintedHit = new Hit(0.01f, 0f, Vector3.forward, tint: tint);
        soldier.TakeHit(plainHit); soldier.TakeHit(tintedHit);
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 200; i++) soldier.TakeHit(plainHit);
        long plain = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 200; i++) soldier.TakeHit(tintedHit);
        long tinted = GC.GetAllocatedBytesForCurrentThread() - before;
        soldier.Health.Fill();
        Say($"measured: 4000 cut marks (3000 tinted) {marks} B; 1000 x StruckTint and the StruckLook sums {looks} B (sum {sum.a:0}); 200 plain TakeHit {plain} B, 200 tinted TakeHit {tinted} B");
        yield return Real(0.5f);

        string[] turns = { Elements.Fire, Elements.Lightning, Elements.Ice, Elements.Poison };
        combat.Equip(Weapons.ByName(Weapons.KatanaName));
        Say("soaking");
        float end = Time.unscaledTime + 22f;
        int turn = 0, blowsWere = combat.BlowsLanded, drawnAt = CutMarks.Drawn;
        while (Time.unscaledTime < end)
        {
            string name = turns[turn++ % turns.Length];
            build.Attune(name, "");
            yield return Frames(2);
            spells.Cooldowns.ResetAll();
            combat.Mana.Gain(100f);
            combat.Blade.Charge(name);
            yield return Frames(4);
            for (int press = 0; press < 3; press++)
            {
                yield return Tap(Key.J);
                yield return UntilReal(() => !combat.IsAttacking, 1.2f);
            }
            combat.Blade.Clear();
            Set(pad.leftTrigger, 1f);
            yield return Frames(2);
            Set(pad.rightShoulder, 1f);
            yield return Frames(2);
            Set(pad.rightShoulder, 0f);
            yield return null;
            Set(pad.leftTrigger, 0f);
            yield return UntilReal(() => !combat.IsAttacking, 2f);
            // A plain chain between, so the white path is soaked too.
            for (int press = 0; press < 2; press++)
            {
                yield return Tap(Key.J);
                yield return UntilReal(() => !combat.IsAttacking, 1.2f);
            }
            foreach (var one in soldiers) if (one != null && one.Health.Fraction < 0.6f) one.Health.Fill();
        }
        Say($"soak done: {combat.BlowsLanded - blowsWere} blows landed, {CutMarks.Drawn - drawnAt} cut marks drawn, {turn} turns");
    }

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
            float angle = (i - 1) * 32f;
            Vector3 spot = at + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * stand + (alone ? Vector3.forward * 30f : Vector3.zero);
            Vector3 look = at - spot;
            look.y = 0f;
            // Its home is made the spot, so putting it back stands it there and an idle enemy has nowhere to walk to.
            HomeField.SetValue(soldier, spot);
            HomeTurnField?.SetValue(soldier, Quaternion.LookRotation(look.normalized, Vector3.up));
            soldier.Restore();
            soldier.Afflictions.Clear();
            soldier.Brain.AggroRange = 0f;
            soldier.Brain.LeashRange = 0.5f;
            soldier.WakeNow();
        }
        yield return UntilReal(() => !Busy(), 8f);
        foreach (var soldier in soldiers) soldier?.Afflictions.Clear();
        yield return Real(0.6f);
    }

    private void Regroup(Vector3 standAt)
    {
        player.Teleport(standAt, Quaternion.identity);
        foreach (var soldier in soldiers)
        {
            if (soldier == null || soldier.IsDead) continue;
            var spot = (Vector3)HomeField.GetValue(soldier);
            var turn = HomeTurnField != null ? (Quaternion)HomeTurnField.GetValue(soldier) : soldier.transform.rotation;
            var controller = soldier.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            soldier.transform.SetPositionAndRotation(spot, turn);
            if (controller != null) controller.enabled = true;
        }
    }

    private IEnumerator Film(ElementInfo info, string weaponName, string tag, float stand, int order, Clip clip)
    {
        string name = info.Name.ToLowerInvariant();
        folder = Root + tag + "_" + name;
        if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
        System.IO.Directory.CreateDirectory(folder);
        yield return Ready(stand);
        Vector3 standAt = player.transform.position;
        filmStand = standAt;
        if (view != null)
        {
            anchor.position = standAt;
            view.Configure(anchor);
        }
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(100f);
        int swingsWere = trails.SwingsDrawn, landingsWere = trails.Landings, groundWere = trails.GroundBlows, blowsWere = combat.BlowsLanded, strikesWere = combat.InfusedStrikes;
        mostRibbons = mostSheaths = mostBladeSheaths = 0;
        window = WindowAbout(standAt + Vector3.forward * (stand * 0.6f) + Vector3.up * 1.7f);
        Time.captureFramerate = Rate;
        frame = 0;
        contacts.Clear();
        flashFrames = whiteFlashFrames = 0;
        whiteAt.Clear();
        drawnWas = CutMarks.Drawn;
        landedWas = combat.BlowsLanded;
        ringLog.Clear();
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
        // The chain's blows push the soldiers off and the lunges carry the player on: the strike whiffed on the Massive
        // Sword and the bow (the films of 2026-11-21), and Dark and Poison draw nothing on a miss. Everyone is stood back
        // on their spots, afflictions and stagger kept, so the strike lands on the group.
        if (!alone) Regroup(standAt);
        yield return Frames(2);

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
        clip.BladeSheaths = mostBladeSheaths;
        clip.Fitted = trails.SourcesFitted;
        System.IO.File.WriteAllText(folder + "/rings.txt", ringLog.ToString());
        System.IO.File.WriteAllText(folder + "/info.txt",
            $"weapon={weaponName}\ntag={tag}\nelement={info.Name}\nstart={clip.Start}\npeak_frame={clip.Peak}\nframes={clip.Frames}\nended={clip.Ended}\nset_off={clip.SetOff}\n" +
            $"blows={clip.Blows}\nlandings={clip.Landings}\nground={clip.Ground}\nmost_trails={clip.Ribbons}\nmost_sheaths={clip.Sheaths}\nmost_blade_sheaths={clip.BladeSheaths}\nlines={clip.Lines}\nfitted={clip.Fitted}\norder={order}\ncontacts={string.Join(",", contacts)}\nflash_frames={flashFrames}\nwhite_flash_frames={whiteFlashFrames}\nwhite_at={string.Join(",", whiteAt)}\n");
    }

    private void Finish()
    {
        filming = false;
        keepCalm = false;
        Time.captureFramerate = 0;
        if (picture != null) Destroy(picture);
        foreach (var pen in pens) if (pen != null) pen.Clear();
        if (view != null && player != null) view.Configure(player.transform);
        if (anchor != null) Destroy(anchor.gameObject);
        foreach (var thing in hidden) if (thing != null) thing.SetActive(true);
        if (reticle != null) reticle.enabled = true;
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
