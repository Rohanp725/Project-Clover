using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;

// The elements' approved effects in the game (docs/GAME.md, "Elemental effects in the game"), in the Proving Ground:
// for each of the eleven elements its six moments, each played through the game's own code and pictured at the game
// camera with time stopped at its strongest: the affliction on a copy of the still dummy, the tap spell mid-cast at it,
// the hit (a charged blow landing), the charged blade mid-swing at the air, the infused strike (L2 + RB on a virtual
// pad) and the surge (F). Frames review/shots/current/fx_<element>_<moment>.png. Then the owner's switch: F8 shows the
// old effects (fx_fire_spell_old.png) and F8 again the new. Each judged line ends OK or WRONG; the last is the tally.
// With %TEMP%/fxprobe_soak.txt present it skips the pictures and holds a busy moment for the profiler instead: six
// dummies each carrying an element's affliction, a surge lit, and every element's spell cast in turn, round and round,
// for the seconds written in the file (default 20), printing "[Probe] soak start" when it begins.
// Progress is sandboxed by runprobe.sh; the dummies are made in play and gone with it; the pad is removed after and the
// Input System's focus settings put back. bash AgentScripts/runprobe.sh ProvingGround FxProbe 200
public static class FxProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Fx Probe").AddComponent<FxProbeRunner>();
        return "probe started";
    }
}

public class FxProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerSpells spells;
    private PlayerBuild build;
    private TrainingDummy host;
    private readonly List<TrainingDummy> made = new List<TrainingDummy>();
    private XInputControllerWindows pad;
    private int ok, wrong;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);

    // Seconds from the cast to the spell's strongest moment, by the element's place on the ring.
    private static readonly float[] SpellPeak = { 0.5f, 0.2f, 0.42f, 0.62f, 0.72f, 1.0f, 0.95f, 0.34f, 0.26f, 0.5f, 1.3f };

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
    }

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    private IEnumerator Still(string name)
    {
        Time.timeScale = 0f;
        yield return Shot(name);
        Time.timeScale = 1f;
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    // Game time, so a cast's moment is read on the clock it runs on.
    private IEnumerator Game(float seconds)
    {
        float end = Time.time + seconds;
        while (Time.time < end) yield return null;
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

    private IEnumerator PadStrike(InputControl attack)
    {
        Set(pad.leftTrigger, 1f);
        yield return null;
        yield return null;
        Set(attack, 1f);
        yield return null;
        yield return null;
        Set(attack, 0f);
        yield return null;
        Set(pad.leftTrigger, 0f);
        yield return null;
        yield return null;
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private IEnumerator WhenFree()
    {
        yield return Until(() => !combat.IsAttacking && !element.IsCasting && !player.Dodge.IsRolling && player.FormHold == 0 && !player.IsLeaping && !defence.IsDrinking, 6f);
        yield return Real(0.1f);
    }

    private TrainingDummy Copy(TrainingDummy from, string name, Vector3 at)
    {
        var copy = Instantiate(from.gameObject, at, from.transform.rotation).GetComponent<TrainingDummy>();
        copy.name = name;
        made.Add(copy);
        return copy;
    }

    // Everything from the last moment out of the way: afflictions, ground, the charge, the surge's pieces.
    private IEnumerator Clean()
    {
        foreach (var dummy in made) if (dummy != null) dummy.Afflictions.Clear();
        combat.Blade.Clear();
        yield return Until(() => Hazards.Alive == 0, 5f);
        foreach (var dummy in made) if (dummy != null) dummy.Afflictions.Clear();
        yield return Until(() => SpellWorks.Running == 0, 4f);
        yield return Real(0.25f);
    }

    // The player south of the host by the metres given, facing it (north).
    private IEnumerator Face(float back)
    {
        player.Teleport(host.transform.position + Vector3.back * back, Quaternion.identity);
        yield return WhenFree();
        yield return Real(0.2f);
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
        element = player.GetComponent<PlayerElement>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest {PlaytestMode.On}, sandboxed {Progress.Sandboxed}, owner's bindings saved {PlayerPrefs.HasKey("clover.controls.bindings")}");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        Judge("the scene has the element art and the new effects are on", ElementArt.Ready, $"switch {ElementArt.On}");
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null) { Finish(); yield break; }
        host = Copy(still, "Fx Host", Middle);
        var katana = FindObjectsByType<LoadoutChoice>(FindObjectsInactive.Include).FirstOrDefault(choice => !choice.IsElement && choice.Choice == Weapons.KatanaName);
        if (katana != null)
        {
            player.Teleport(katana.transform.position + Vector3.back * 1f, Quaternion.identity);
            yield return Real(0.35f);
            yield return Tap(Key.E);
            yield return Real(0.3f);
        }
        Judge("the Katana is in hand (a blade to charge)", combat.Weapon != null && combat.Weapon.Name == Weapons.KatanaName, combat.Weapon != null ? combat.Weapon.Name : "none");
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;

        string soakPath = Path.Combine(Path.GetTempPath(), "fxprobe_soak.txt");
        if (File.Exists(soakPath))
        {
            float seconds = 20f;
            float.TryParse(File.ReadAllText(soakPath).Trim(), out seconds);
            yield return Soak(still, Mathf.Max(5f, seconds));
            Finish();
            yield break;
        }

        foreach (var info in Elements.All)
        {
            yield return Moments(info);
        }

        // The owner's switch: F8 shows the old effects, F8 again the new.
        yield return Clean();
        build.Attune(Elements.Fire, "");
        int switched = ElementArt.Switched;
        yield return Tap(Key.F8);
        yield return Real(0.2f);
        Judge("F8 turns the old effects on", !ElementArt.On && !ElementArt.Ready && ElementArt.Switched == switched + 1, $"switch {ElementArt.On}");
        yield return Face(3f);
        element.CastBase(Elements.Fire);
        yield return Game(SpellPeak[0]);
        yield return Still("fx_fire_spell_old");
        yield return Tap(Key.F8);
        yield return Real(0.2f);
        Judge("F8 again brings the new effects back", ElementArt.On && ElementArt.Ready && ElementArt.Switched == switched + 2);
        Finish();
    }

    private IEnumerator Moments(ElementInfo info)
    {
        string name = info.Name.ToLowerInvariant();
        yield return Clean();
        build.Attune(info.Name, "");
        yield return Real(0.2f);
        Judge($"{info.Name}: carried alone", element.HasElement && element.Info != null && element.Info.Name == info.Name && !element.Mixed, element.Info != null ? element.Info.Name : "none");

        // 1. The affliction on the host, three deep, as a spell lays it.
        yield return Face(3.2f);
        int beatsBefore = AfflictionArt.Shown(info.Affliction);
        host.Afflictions.Lay(info.Affliction, 3, 0f, 1f, LaySource.Spell, host);
        yield return Game(1.3f);
        Judge($"{info.Name}: its affliction ({info.Affliction}) is drawn by the element art", AfflictionArt.Shown(info.Affliction) > beatsBefore, $"{AfflictionArt.Shown(info.Affliction) - beatsBefore} beats, solids {FormFx.SolidsShown}, marks {FormFx.MarksShown}");
        yield return Still($"fx_{name}_affliction");

        // 2. The tap spell at the host, pictured at its strongest.
        yield return Clean();
        yield return Face(3f);
        bool cast = element.CastBase(info.Name);
        yield return Game(SpellPeak[info.Index]);
        Judge($"{info.Name}: {element.Kit.SpellName} is cast", cast);
        yield return Still($"fx_{name}_spell");

        // 3. The hit: a charged blow landing on the host.
        yield return Clean();
        yield return Face(1.6f);
        combat.Blade.Charge(info.Name);
        yield return Real(0.15f);
        int hits = host.HitsTaken;
        Keys(Key.J);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => host.HitsTaken > hits, 1.5f);
        yield return Game(0.06f);
        Judge($"{info.Name}: a charged blow lands", host.HitsTaken > hits);
        yield return Still($"fx_{name}_hit");

        // 4. The charged blade mid-swing at the air, the host well out of reach.
        yield return Clean();
        yield return Face(6f);
        combat.Blade.Charge(info.Name);
        yield return Real(0.3f);
        Keys(Key.J);
        yield return null;
        yield return null;
        Keys();
        yield return Game(0.12f);
        yield return Still($"fx_{name}_charged");

        // 5. The infused strike on the host (L2 + RB).
        yield return Clean();
        yield return WhenFree();
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(100f);
        yield return Face(1.8f);
        int strikes = combat.InfusedStrikes, drawn = StrikeArt.Played;
        yield return PadStrike(pad.rightShoulder);
        yield return Until(() => combat.InfusedStrikes > strikes, 1.5f);
        // The element's strike goes off once the carrier's blow is out.
        yield return Until(() => StrikeArt.Played > drawn, 2f);
        yield return Game(0.18f);
        Judge($"{info.Name}: {ElementSteel.StrikeName(info.Name, false)} plays and the element art draws it", combat.InfusedStrikes > strikes && StrikeArt.Played > drawn, defence.LastCallout);
        yield return Still($"fx_{name}_infused");

        // 6. The surge (F), with the mana for it.
        yield return Clean();
        yield return WhenFree();
        combat.Mana.Gain(100f);
        yield return Face(3f);
        yield return Tap(Key.F);
        yield return Until(() => element.SurgeActive, 1.5f);
        yield return Game(1.1f);
        Judge($"{info.Name}: {element.Kit.SurgeName} is lit", element.SurgeActive, defence.LastCallout);
        yield return Still($"fx_{name}_surge");
        // Carrying another element next takes this surge with it.
    }

    // A busy moment held for the profiler: six afflicted dummies round the host, a surge lit, a spell of each element in turn.
    private IEnumerator Soak(TrainingDummy still, float seconds)
    {
        var others = new List<TrainingDummy>();
        for (int i = 0; i < 6; i++)
            others.Add(Copy(still, "Fx Soak " + i, Middle + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * 2.6f));
        build.Attune(Elements.Dark, Elements.Fire);
        yield return Face(3.4f);
        combat.Mana.Gain(100f);
        yield return Tap(Key.F);
        Say("soak start");
        float end = Time.unscaledTime + seconds;
        int turn = 0, casts = 0;
        while (Time.unscaledTime < end)
        {
            var info = Elements.All[turn % Elements.All.Count];
            for (int i = 0; i < others.Count; i++)
            {
                var kind = Elements.All[(turn + i) % Elements.All.Count].Affliction;
                if (!others[i].Afflictions.Has(kind)) others[i].Afflictions.Lay(kind, 3, 0f, 1f, LaySource.Spell, others[i]);
            }
            // Carrying the element (with Dark kept for its surge) and throwing its spell at the ring.
            build.Attune(Elements.Dark, info.Name == Elements.Dark ? Elements.Fire : info.Name);
            if (!element.SurgeActive)
            {
                combat.Mana.Gain(100f);
                yield return Tap(Key.F);
            }
            if (element.CastBase(info.Name)) casts++;
            yield return Real(0.7f);
            turn++;
        }
        Say($"soak end: {casts} casts over {seconds:0} s, solids {FormFx.SolidsShown}, marks {FormFx.MarksShown}, runs {SpellWorks.Running}");
    }

    private void Finish()
    {
        foreach (var dummy in made) if (dummy != null) Destroy(dummy.gameObject);
        if (build != null) build.Attune(Elements.Fire, "");
        if (pad != null) InputSystem.RemoveDevice(pad);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Time.timeScale = 1f;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
