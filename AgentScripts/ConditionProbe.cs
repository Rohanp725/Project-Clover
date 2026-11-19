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

// Phase 3, task 3.3, in the Proving Ground: enemy conditions on a real soldier from the castle's pen. Rooted, it notices
// the player and does not walk, is not shoved, and still strikes what comes into reach; freed, it walks again. Begins no
// attack: in reach it begins none. Cannot guard: a light blow from the front lands instead of being turned. Wind-up
// slowed by half: its forced attack is shown about twice as long. Sight cut short: it stands and does nothing at 5 m.
// Damage taken and dealt shares. Then Flashover laid on it mid-wind-up makes it flinch (the reaction's Flinch action).
// Frames review/shots/current/p3_condition_*.png. Every judged line ends OK or WRONG.
// bash AgentScripts/runprobe.sh ProvingGround ConditionProbe 90
public static class ConditionProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Condition Probe").AddComponent<ConditionProbeRunner>();
        return "probe started";
    }
}

public class ConditionProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerReactions reactions;
    private Enemy soldier;
    private EnemyPen castle;
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

    private EnemyConditions Conditions => soldier.Afflictions.Conditions;

    private void Set(string id, ConditionFlags flags, float scale = 1f, float sight = 0f, float seconds = 10f) =>
        Conditions.Set(new ConditionSpec { Id = id, Seconds = seconds, Flags = flags, Scale = scale, Sight = sight }, soldier);

    // Stands the player this far in front of the soldier (south of it, inside the pen), facing it.
    private void StandOff(float metres)
    {
        Vector3 at = soldier.transform.position + Vector3.back * metres;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
    }

    private IEnumerator Calm()
    {
        yield return Until(() => !soldier.Brain.IsAttacking && soldier.Brain.State != EnemyState.Flinch, 4f);
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

        // 1. Rooted: it notices and does not walk; shoved, it stays; in reach it still strikes.
        Set("probe.rooted", ConditionFlags.Rooted);
        Vector3 rootedAt = soldier.transform.position;
        StandOff(5f);
        yield return Real(2f);
        float walked = Vector3.Distance(Flat(soldier.transform.position), Flat(rootedAt));
        Judge("rooted, it notices the player 5 m off and does not walk to them", soldier.IsAware && walked < 0.1f, $"state {soldier.Brain.State}, moved {walked:0.00} m");
        soldier.Push(Vector3.forward * 3f);
        yield return Real(0.6f);
        walked = Vector3.Distance(Flat(soldier.transform.position), Flat(rootedAt));
        Judge("rooted, a shove does not move it", walked < 0.1f, $"moved {walked:0.00} m");
        int struck = soldier.BlowsStruck;
        StandOff(1.7f);
        yield return Until(() => soldier.Brain.State == EnemyState.Telegraph, 3f);
        yield return Shot("p3_condition_rooted");
        yield return Until(() => soldier.BlowsStruck > struck, 4f);
        walked = Vector3.Distance(Flat(soldier.transform.position), Flat(rootedAt));
        Judge("rooted, it still strikes what comes into reach, and its feet stay put", soldier.BlowsStruck > struck && walked < 0.1f,
            $"blows {soldier.BlowsStruck - struck}, moved {walked:0.00} m, attack {soldier.Brain.Current?.Name}");
        yield return Calm();
        Conditions.Clear();
        StandOff(5f);
        Vector3 freedAt = soldier.transform.position;
        yield return Real(1.5f);
        walked = Vector3.Distance(Flat(soldier.transform.position), Flat(freedAt));
        Judge("freed, it walks to the player again", walked > 0.5f, $"moved {walked:0.00} m");

        // 2. Begins no attack: in reach for 2.5 s it begins none, and still turns and closes.
        yield return Calm();
        Set("probe.breath", ConditionFlags.BeginsNoAttack);
        StandOff(1.7f);
        struck = soldier.BlowsStruck;
        bool began = false;
        float end = Time.unscaledTime + 2.5f;
        while (Time.unscaledTime < end)
        {
            if (soldier.Brain.State == EnemyState.Telegraph) began = true;
            yield return null;
        }
        Judge("breathless, in reach it begins no attack", !began && soldier.BlowsStruck == struck, $"state {soldier.Brain.State}");
        Conditions.Clear();
        yield return Until(() => soldier.Brain.State == EnemyState.Telegraph, 3f);
        Judge("its breath back, it attacks again", soldier.Brain.State == EnemyState.Telegraph || soldier.Brain.IsAttacking);
        yield return Calm();

        // 3. Cannot guard: a light blow from its front lands instead of being turned.
        if (soldier.Kind.Guards)
        {
            Set("probe.open", ConditionFlags.CannotGuard | ConditionFlags.BeginsNoAttack);
            yield return Until(() => soldier.Brain.State == EnemyState.Stalk, 2f);
            soldier.GuardOpenFor = 0f;
            var outcome = soldier.TakeHit(new Hit(4f, 2f, -soldier.transform.forward));
            Judge("unable to guard, a light blow from the front lands", outcome == HitOutcome.Landed, outcome.ToString());
            Conditions.Clear();
            Set("probe.calm", ConditionFlags.BeginsNoAttack);
            yield return Until(() => soldier.Brain.State == EnemyState.Stalk, 2f);
            soldier.GuardOpenFor = 0f;
            outcome = soldier.TakeHit(new Hit(4f, 2f, -soldier.transform.forward));
            Judge("able to guard again, the same blow is turned", outcome == HitOutcome.Blocked, outcome.ToString());
            Conditions.Clear();
        }
        else Say("the soldier does not guard: Cannot guard is not tried on it");
        yield return Calm();

        // 4. Wind-up slowed by half: its first attack forced, shown about twice as long.
        var attack = soldier.Brain.Attacks[0];
        Set("probe.windup", ConditionFlags.WindupSlowed, 0.5f);
        float slowed = 0f;
        yield return Windup(attack.Name, seconds => slowed = seconds);
        Conditions.Clear();
        yield return Calm();
        float usual = 0f;
        yield return Windup(attack.Name, seconds => usual = seconds);
        Judge("its wind-up slowed by half is shown about twice as long", usual > 0f && slowed / usual > 1.6f && slowed / usual < 2.5f,
            $"{attack.Name}: {usual:0.00} s as usual, {slowed:0.00} s slowed (written {attack.Telegraph:0.00} s)");
        yield return Calm();

        // 5. Sight cut short to 2 m: at 5 m it stands and begins nothing.
        Set("probe.sight", ConditionFlags.SightShort, 1f, 2f);
        StandOff(5f);
        Vector3 blindAt = soldier.transform.position;
        struck = soldier.BlowsStruck;
        yield return Real(1.5f);
        walked = Vector3.Distance(Flat(soldier.transform.position), Flat(blindAt));
        Judge("its sight cut to 2 m, it does not come for the player 5 m off", walked < 0.1f && soldier.BlowsStruck == struck && !soldier.Brain.IsAttacking, $"moved {walked:0.00} m, state {soldier.Brain.State}");
        Conditions.Clear();

        // 6. The shares: what it takes and deals.
        yield return Calm();
        Set("probe.taken", ConditionFlags.DamageTakenShare | ConditionFlags.BeginsNoAttack, 1.5f);
        Set("probe.dealt", ConditionFlags.DamageDealtShare, 0.5f);
        float before = soldier.Health.Current;
        soldier.TakeHit(new Hit(10f, 0f, soldier.transform.forward, unblockable: true));
        float took = before - soldier.Health.Current;
        Judge("taking half again, a blow of 10 takes 15", Mathf.Abs(took - 15f) < 0.6f, $"took {took:0.0}");
        Judge("dealing half, what it deals is halved", Mathf.Abs(soldier.Afflictions.DamageDealtMultiplier - 0.5f) < 0.01f, $"x{soldier.Afflictions.DamageDealtMultiplier:0.00}");
        Conditions.Clear();
        yield return Calm();

        // 7. A reaction's Flinch: Flashover laid on it while it winds up breaks the attack (unless it is steadfast).
        StandOff(1.7f);
        soldier.Brain.Force(attack.Name);
        yield return null;
        int flinches = soldier.EffectFlinches;
        int pair = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);
        int shown = reactions.TimesShown(pair);
        soldier.Afflictions.Lay(AfflictionKind.Static, 2, 0f, 1f, LaySource.Spell, soldier);
        soldier.Afflictions.Lay(AfflictionKind.Burn, 1, 0f, 1f, LaySource.Spell, soldier);
        yield return Until(() => reactions.TimesShown(pair) > shown, 1.5f);
        yield return null;
        yield return Shot("p3_condition_flashover_flinch");
        Judge(soldier.Kind.Steadfast ? "Flashover on a steadfast soldier: shown, and its wind-up goes on" : "Flashover mid-wind-up makes it flinch: the attack is broken",
            reactions.TimesShown(pair) > shown && (soldier.Kind.Steadfast ? soldier.EffectFlinches == flinches : soldier.EffectFlinches == flinches + 1 && soldier.Brain.Current == null),
            $"flinches {soldier.EffectFlinches - flinches}, state {soldier.Brain.State}");

        castle.Clear();
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }

    // Forces the attack and times its wind-up, in real seconds.
    private IEnumerator Windup(string name, Action<float> took)
    {
        StandOff(1.7f);
        soldier.Brain.Force(name);
        float from = Time.time;
        yield return Until(() => soldier.Brain.State != EnemyState.Telegraph, 4f);
        took(Time.time - from);
    }

    private static Vector3 Flat(Vector3 at) => new Vector3(at.x, 0f, at.z);
}
