using System;
using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Play Mode probe for the pause menu, with key presses only: open and close in the hub, the controls page, the
// questions before quitting, giving up and forgetting, the times it must not open, and pausing in the middle of an
// attack, a roll and a hit-stop. It forgets saved progress before and after. Run in Play Mode, in the Hub scene.
public static class PauseProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Pause Probe").AddComponent<PauseProbeRunner>();
        return "probe started";
    }
}

public class PauseProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private bool keepAlive = true;

    private void Update()
    {
        if (keepAlive && defence != null) defence.Health.Heal(100f);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private static PauseMenu Menu => PauseMenu.Instance;

    private IEnumerator Shot(string name)
    {
        ScreenCapture.CaptureScreenshot("review/shots/current/pause_" + name + ".png");
        yield return null;
        yield return null;
    }

    private static void ForgetAll()
    {
        Progress.Forget(Progress.MetSponsor);
        Progress.Forget(Progress.WeaponKey(Weapons.KatanaName));
        Progress.Forget(Progress.ElementKey(ElementKit.LightningName));
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
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

    private void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 60) yield return Tap(Key.E);
    }

    private string State() => $"open {Menu.IsOpen} time {Time.timeScale:0.00} suspended {player.Suspended}";

    private IEnumerator Load(string scene)
    {
        SceneManager.LoadScene(scene);
        yield return null;
        yield return null;
        FindPlayer();
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        ForgetAll();
        yield return Load("Hub");
        Say($"start: scene {SceneManager.GetActiveScene().name} menu {(Menu != null)} entries {Menu.Choices.Count}");
        yield return new WaitForSeconds(0.3f);
        yield return Shot("hub_line");

        // A: not over spoken words.
        yield return Until(() => ReadingBox.Instance.IsOpen, 5f);
        bool reading = ReadingBox.Instance.IsOpen;
        yield return Tap(Key.Escape);
        Say($"A over the reading box: box open {reading}; after Esc {State()}");
        yield return ReadAll();
        yield return null;
        yield return null;

        // B: open, look, close.
        string before = State();
        yield return Tap(Key.Escape);
        string during = State();
        yield return Shot("menu");
        yield return Tap(Key.S);
        int chosen = Menu.Choices.Selected;
        yield return Tap(Key.E);
        bool page = Menu.ShowingControls;
        yield return Shot("controls");
        yield return Tap(Key.Space);
        bool backToList = Menu.IsOpen && !Menu.ShowingControls;
        yield return Tap(Key.Space);
        yield return null;
        Say($"B hub: before [{before}] during [{during}] chose {chosen} controls page {page} back to list {backToList} after [{State()}] rolling {player.Dodge.IsRolling}");

        // B2: Resume by the use button, and Esc closes from the controls page.
        yield return Tap(Key.Escape);
        yield return Tap(Key.E);
        yield return null;
        string resumed = State();
        yield return Tap(Key.Escape);
        yield return Tap(Key.S);
        yield return Tap(Key.E);
        yield return Tap(Key.Escape);
        yield return null;
        Say($"B2 resume by use [{resumed}]; Esc from the controls page [{State()}]");

        // C: quit asks first, and the editor goes on.
        yield return Tap(Key.Escape);
        yield return Tap(Key.W);
        var entry = Menu.Choices.Current;
        yield return Tap(Key.E);
        bool asked = Menu.Choices.Asking && Menu.IsOpen;
        yield return Shot("confirm");
        yield return Tap(Key.Space);
        bool dropped = !Menu.Choices.Asking && Menu.IsOpen;
        yield return Tap(Key.E);
        yield return Tap(Key.E);
        yield return null;
        Say($"C quit: entry {entry} asked {asked} quits then {0}; back drops it {dropped}; pressed twice: quits {Menu.Quits} still playing {Application.isPlaying} [{State()}]");

        // D: not over the fire's menu.
        UpgradeMenu.Instance.Open();
        yield return null;
        bool tree = UpgradeMenu.Instance.IsOpen;
        yield return Tap(Key.Escape);
        string overTree = State();
        yield return Tap(Key.Space);
        yield return null;
        yield return null;
        Say($"D over the upgrade menu: tree open {tree}; after Esc [{overTree}]; tree closed {!UpgradeMenu.Instance.IsOpen} [{State()}]");

        // E: the castle. Mid-attack.
        yield return Load("Castle");
        yield return new WaitForSeconds(0.5f);
        player.Teleport(player.transform.position, Quaternion.identity);
        Keys(Key.J);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => combat.IsAttacking, 1f);
        bool attacking = combat.IsAttacking;
        string move = combat.Combo.Current != null ? combat.Combo.Current.Name : "none";
        yield return Tap(Key.Escape);
        during = State();
        yield return new WaitForSecondsRealtime(0.5f);
        bool held = combat.IsAttacking;
        yield return Tap(Key.Escape);
        bool still = combat.IsAttacking;
        yield return Until(() => !combat.IsAttacking, 3f);
        Say($"E mid-attack: attacking {attacking} ({move}); paused [{during}] still attacking after 0.5 s {held}; on resume attacking {still}, finished {!combat.IsAttacking} [{State()}] entries {Menu.Choices.Count}");

        // F: mid-roll.
        yield return new WaitForSeconds(0.4f);
        Keys(Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => player.Dodge.IsRolling, 1f);
        bool rolling = player.Dodge.IsRolling;
        Vector3 from = player.transform.position;
        yield return Tap(Key.Escape);
        during = State();
        Vector3 pausedAt = player.transform.position;
        yield return new WaitForSecondsRealtime(0.5f);
        float drift = Vector3.Distance(pausedAt, player.transform.position);
        bool heldRoll = player.Dodge.IsRolling;
        yield return Tap(Key.Escape);
        yield return Until(() => !player.Dodge.IsRolling, 3f);
        Say($"F mid-roll: rolling {rolling}; paused [{during}] moved while paused {drift:0.000} m, still rolling {heldRoll}; resumed, roll ended {!player.Dodge.IsRolling}, travelled {Vector3.Distance(from, player.transform.position):0.00} m [{State()}]");

        // G: during a hit-stop.
        yield return new WaitForSeconds(0.4f);
        var stop = FindAnyObjectByType<HitStop>();
        stop.Freeze(0.6f);
        yield return null;
        float frozen = Time.timeScale;
        yield return Tap(Key.Escape);
        during = State();
        yield return new WaitForSecondsRealtime(0.2f);
        yield return Tap(Key.Escape);
        yield return null;
        string after = State();
        yield return new WaitForSecondsRealtime(0.8f);
        Say($"G hit-stop: time in the stop {frozen:0.00}; paused [{during}]; resumed [{after}]; 0.8 s later time {Time.timeScale:0.00}");

        // H: give up the run.
        RunState.Weapon = Weapons.KatanaName;
        RunState.Points = 3;
        int runs = RunState.RunsEnded;
        yield return Tap(Key.Escape);
        yield return Tap(Key.S);
        yield return Tap(Key.S);
        entry = Menu.Choices.Current;
        yield return Tap(Key.E);
        asked = Menu.Choices.Asking;
        yield return Shot("give_up");
        string scene = SceneManager.GetActiveScene().name;
        yield return Tap(Key.E);
        yield return Until(() => SceneManager.GetActiveScene().name == "Hub", 5f);
        yield return null;
        yield return null;
        FindPlayer();
        Say($"H give up: entry {entry} asked {asked} (still in {scene}); then scene {SceneManager.GetActiveScene().name} runs ended {runs} -> {RunState.RunsEnded} fell {RunState.LastRunFell} weapon {combat.Weapon.Name} run weapon '{RunState.Weapon}' embers {RunState.Points} [{State()}]");

        // I: forget everything.
        Progress.Unlock(Progress.WeaponKey(Weapons.KatanaName));
        Progress.Unlock(Progress.ElementKey(ElementKit.LightningName));
        bool met = Progress.Has(Progress.MetSponsor);
        yield return new WaitForSeconds(0.5f);
        yield return Tap(Key.Escape);
        yield return Tap(Key.S);
        yield return Tap(Key.S);
        entry = Menu.Choices.Current;
        yield return Tap(Key.E);
        asked = Menu.Choices.Asking;
        yield return Shot("forget");
        var oldPlayer = player;
        yield return Tap(Key.E);
        yield return Until(() => oldPlayer == null, 3f);
        yield return null;
        FindPlayer();
        bool reloaded = oldPlayer == null;
        string keys = $"met {Progress.Has(Progress.MetSponsor)} katana {Progress.Has(Progress.WeaponKey(Weapons.KatanaName))} lightning {Progress.Has(Progress.ElementKey(ElementKit.LightningName))}";
        yield return Until(() => ReadingBox.Instance != null && ReadingBox.Instance.IsOpen, 5f);
        Say($"I forget: entry {entry} asked {asked} met before {met}; hub reloaded {reloaded} scene {SceneManager.GetActiveScene().name}; keys: {keys}; first waking plays again {ReadingBox.Instance.IsOpen} time {Time.timeScale:0.00}");
        yield return ReadAll();

        // J: not once the player has fallen, and the fall still ends the run.
        yield return Load("Castle");
        yield return new WaitForSeconds(0.5f);
        keepAlive = false;
        runs = RunState.RunsEnded;
        yield return null;
        defence.TakeHit(new Hit(1000f, 0f, Vector3.back));
        yield return null;
        bool dead = defence.IsDead;
        yield return Tap(Key.Escape);
        string fallen = State();
        yield return new WaitForSecondsRealtime(1.8f);
        yield return Tap(Key.Escape);
        string fading = $"open {Menu.IsOpen} time {Time.timeScale:0.00}";
        yield return Until(() => SceneManager.GetActiveScene().name == "Hub", 8f);
        yield return null;
        yield return null;
        FindPlayer();
        Say($"J the fall: dead {dead}; Esc at once [{fallen}]; Esc in the fade [{fading}]; woke in {SceneManager.GetActiveScene().name} runs ended {runs} -> {RunState.RunsEnded} [{State()}]");

        ForgetAll();
        Say($"done: time scale {Time.timeScale:0.00} suspended {player.Suspended}");
        Destroy(gameObject);
    }
}
