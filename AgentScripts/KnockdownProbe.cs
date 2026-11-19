using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using R = Clover.Player.Trees.Reactions;

// Phase 3, task 3.7, in the Proving Ground: knocked down. A soldier from the castle's pen winds up an attack at the
// player; Soaked and then Unsteady laid on it make Capsize, which knocks it flat: the wind-up is lost, it falls on its
// back (the soldier_knockdown clip), lies for the condition's 1.2 s (soldier_down_loop) with its head near the floor,
// and in that time strikes nothing, walks nowhere, does not flinch, cannot guard and is not forced into an attack. Then
// it gets up (soldier_getup), stands with its head at standing height, and attacks again. Run after Clover > Forge >
// Bake Enemy Animations and a rebuild of the Proving Ground, or the clip lines say the clips are missing.
// Frames review/shots/current/p3_knockdown_*.png. Every judged line ends OK or WRONG.
// bash AgentScripts/runprobe.sh ProvingGround KnockdownProbe 60
public static class KnockdownProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Knockdown Probe").AddComponent<KnockdownProbeRunner>();
        return "probe started";
    }
}

public class KnockdownProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerReactions reactions;
    private Enemy soldier;
    private EnemyPen castle;
    private Animator animator;
    private Transform head;
    private int ok, wrong;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (soldier != null && !soldier.IsDead && soldier.Health.Fraction < 0.5f) soldier.Health.Fill();
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

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
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

    // Stands the player this far in front of the soldier (south of it, inside the pen), facing it.
    private void StandOff(float metres)
    {
        Vector3 at = soldier.transform.position + Vector3.back * metres;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
    }

    private float HeadHeight => head != null ? head.position.y - soldier.transform.position.y : -1f;

    private bool Playing(string clip) => animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName(clip);

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        reactions = player.GetComponent<PlayerReactions>();
        Say($"start: scene {SceneManager.GetActiveScene().name}, sandboxed {Progress.Sandboxed}");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Include).FirstOrDefault(pen => pen.Land == "The castle");
        castle.Release(new[] { 0 });
        soldier = castle.Enemies[0];
        yield return Real(0.5f);
        Judge("a soldier is let out of the castle's pen", soldier != null && soldier.gameObject.activeInHierarchy && soldier.Kind.Name == EnemyKind.SoldierName, soldier != null ? soldier.Kind.Name : "none");
        animator = soldier.GetComponentInChildren<Animator>();
        head = soldier.GetComponentsInChildren<Transform>().FirstOrDefault(bone => bone.name == "head");
        string prefix = soldier.Kind.Prefix;
        Judge("its animator has the knocked-down clips (baked by the Forge, the scene rebuilt)",
            animator != null && animator.HasState(0, Animator.StringToHash(prefix + "_knockdown")) && animator.HasState(0, Animator.StringToHash(prefix + "_down_loop"))
            && animator.HasState(0, Animator.StringToHash(prefix + "_getup")));
        float standing = HeadHeight;
        Say($"standing, its head is {standing:0.00} m up");

        // 1. In reach it winds up at the player; Capsize knocks it flat out of the wind-up.
        StandOff(1.7f);
        yield return Until(() => soldier.Brain.State == EnemyState.Telegraph, 4f);
        Judge("in reach it winds up an attack", soldier.Brain.State == EnemyState.Telegraph, soldier.Brain.State.ToString());
        int pair = AfflictionPairs.Index(AfflictionKind.Soaked, AfflictionKind.Unsteady);
        int shown = reactions.TimesShown(pair);
        int struck = soldier.BlowsStruck;
        soldier.Afflictions.Lay(AfflictionKind.Soaked, 1, 0f, 1f, LaySource.Spell, soldier);
        soldier.Afflictions.Lay(AfflictionKind.Unsteady, 1, 0f, 1f, LaySource.Spell, soldier);
        yield return Until(() => soldier.Brain.IsDown, 1.5f);
        float downAt = Time.time;
        Judge("Capsize reacts on it and it is knocked flat out of its wind-up",
            reactions.TimesShown(pair) > shown && soldier.Afflictions.Conditions.Holds(R.Capsized) && soldier.Brain.IsDown && soldier.Brain.Current == null,
            $"shown {reactions.TimesShown(pair) - shown}, down {soldier.Afflictions.Conditions.Left(R.Capsized):0.00} s, state {soldier.Brain.State}");
        yield return Real(0.2f);
        yield return Shot("p3_knockdown_fall");
        Judge("it falls with the knockdown clip", Playing(prefix + "_knockdown"), Playing(prefix + "_down_loop") ? "already lying" : "another state");

        // 2. Lying: it does nothing, whatever is tried.
        Vector3 lyingAt = soldier.transform.position;
        yield return Until(() => Time.time - downAt >= EnemyBrain.FallSeconds + 0.15f, 1f);
        yield return Shot("p3_knockdown_lying");
        float lying = HeadHeight;
        Judge("it lies on its back with the down loop, its head near the floor", Playing(prefix + "_down_loop") && lying >= 0f && lying < 0.6f,
            $"head {lying:0.00} m up (standing {standing:0.00} m)");
        Judge("lying, it does not flinch", !soldier.Flinch(0.4f));
        Judge("lying, it is not forced into an attack", !soldier.Brain.Force(soldier.Brain.Attacks[0].Name));
        if (soldier.Kind.Guards)
        {
            soldier.GuardOpenFor = 0f;
            var outcome = soldier.TakeHit(new Hit(4f, 2f, -soldier.transform.forward));
            Judge("lying, it cannot guard: a light blow from the front lands", outcome == HitOutcome.Landed, outcome.ToString());
        }
        else Say("the soldier does not guard: its guard is not tried while it lies");
        bool began = false;
        while (soldier.Brain.IsDown && !soldier.Brain.IsGettingUp && Time.time - downAt < 3f)
        {
            if (soldier.Brain.State == EnemyState.Telegraph) began = true;
            yield return null;
        }
        float lay = Time.time - downAt;
        float moved = Vector3.Distance(Flat(soldier.transform.position), Flat(lyingAt));
        Judge("lying, it begins no attack, strikes nothing and walks nowhere", !began && soldier.BlowsStruck == struck && moved < 0.15f,
            $"blows {soldier.BlowsStruck - struck}, moved {moved:0.00} m");
        Judge("it lies for the condition's 1.2 s", soldier.Brain.IsGettingUp && lay > 1.05f && lay < 1.5f, $"{lay:0.00} s, then {soldier.Brain.State}");

        // 3. It gets up, stands at its height, and is back in the fight.
        yield return Real(EnemyBrain.GetUpSeconds * 0.4f);
        yield return Shot("p3_knockdown_getup");
        Judge("it gets up with the getup clip", Playing(prefix + "_getup") && soldier.Brain.IsGettingUp, soldier.Brain.State.ToString());
        yield return Until(() => !soldier.Brain.IsDown, 2f);
        float upAfter = Time.time - downAt;
        yield return Real(0.3f);
        yield return Shot("p3_knockdown_stood");
        float stood = HeadHeight;
        Judge("it stands again, its head at standing height", !soldier.Brain.IsDown && stood > standing - 0.25f, $"up after {upAfter:0.00} s, head {stood:0.00} m");
        yield return Until(() => soldier.Brain.State == EnemyState.Telegraph || soldier.BlowsStruck > struck, 4f);
        Judge("on its feet, it attacks again", soldier.Brain.State == EnemyState.Telegraph || soldier.Brain.IsAttacking || soldier.BlowsStruck > struck, soldier.Brain.State.ToString());

        castle.Clear();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }

    private static Vector3 Flat(Vector3 at) => new Vector3(at.x, 0f, at.z);
}
