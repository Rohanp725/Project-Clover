using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Play Mode probe for the three one-line tips: opens the scene afresh, brings about each tip's moment, and writes when
// each tip was said, that none is said twice, and that they keep apart, as "[Probe] ..." lines. Run in Play Mode with
//   unity command script run -- --file AgentScripts/TipsProbe.cs --entry TipsProbe.Run
public static class TipsProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Tips Probe").AddComponent<TipsProbeRunner>();
        return "probe started";
    }
}

public class TipsProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private AreaDirector director;
    private int calloutsSeen;
    private int guardTips, breakTips, finishTips;
    private float lastTipAt = -100f;
    private float smallestGap = 100f;

    private int TipsSaid => guardTips + breakTips + finishTips;

    // Wounds are healed as they come, and every word put on the status line is looked at for a tip.
    private void Update()
    {
        if (defence == null) return;
        defence.Health.Heal(100f);
        if (defence.Callouts == calloutsSeen) return;
        calloutsSeen = defence.Callouts;
        string said = defence.LastCallout;
        bool tip = true;
        if (said == TipRules.GuardText) guardTips++;
        else if (said == TipRules.BreakGuardText) breakTips++;
        else if (said == TipRules.FinishText) finishTips++;
        else tip = false;
        if (!tip)
        {
            Say($"  (line: '{said}' at {Time.timeSinceLevelLoad:0.0}s)");
            return;
        }
        float gap = Time.time - lastTipAt;
        if (lastTipAt > 0f) smallestGap = Mathf.Min(smallestGap, gap);
        Say($"  tip said: '{said}' at {Time.timeSinceLevelLoad:0.0}s since load, {(lastTipAt > 0f ? gap.ToString("0.0") + "s after the last tip" : "the first tip")}");
        lastTipAt = Time.time;
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

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
    }

    private Enemy Find(string kind, int which)
    {
        foreach (var enemy in director.Enemies)
            if (enemy.Kind.Name == kind && !enemy.IsDead && which-- == 0) return enemy;
        return null;
    }

    private void StandBefore(Enemy enemy, float distance)
    {
        Vector3 at = enemy.transform.position + enemy.transform.forward * distance;
        player.Teleport(at, Quaternion.LookRotation(-enemy.transform.forward, Vector3.up));
    }

    // A blow with no guard in its way, struck when the last one's flinch is over.
    private IEnumerator Blow()
    {
        yield return Until(() => !defence.IsHelpless, 2f);
        defence.TakeHit(new Hit(1f, 0f, Vector3.back));
        yield return null;
    }

    private IEnumerator StaggerIt(Enemy enemy)
    {
        yield return Until(() => !enemy.IsStaggered, 6f);
        StandBefore(enemy, 2.5f);
        yield return null;
        enemy.TakeHit(new Hit(1f, 1000f, Vector3.forward));
        yield return null;
    }

    private IEnumerator Start()
    {
        // A fresh load, so the quiet time at the start of a scene can be watched.
        DontDestroyOnLoad(gameObject);
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
        yield return null;
        yield return null;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        director = FindAnyObjectByType<AreaDirector>();
        calloutsSeen = defence.Callouts;
        var rules = Tips.Rules;
        Say($"start: scene {scene} reloaded, {Time.timeSinceLevelLoad:0.0}s since load, shown guard {rules.WasShown(Tip.Guard)} break {rules.WasShown(Tip.BreakGuard)} finish {rules.WasShown(Tip.Finish)}");

        // A: two unguarded blows in the first second: the tip is earned but the scene is new, so it is not said.
        int taken = defence.BlowsTaken;
        yield return Blow();
        bool afterOne = rules.IsWaiting(Tip.Guard);
        yield return Blow();
        Say($"A quiet start: blows taken +{defence.BlowsTaken - taken} by {Time.timeSinceLevelLoad:0.0}s, waiting after one {afterOne}, after two {rules.IsWaiting(Tip.Guard)}");
        yield return Until(() => Time.timeSinceLevelLoad > 5.5f, 8f);
        Say($"A at {Time.timeSinceLevelLoad:0.0}s: tips said {TipsSaid}, still waiting {rules.IsWaiting(Tip.Guard)}, line '{defence.LastCallout}'");

        // B: the next unguarded blow, now the scene has settled.
        yield return Blow();
        yield return Until(() => guardTips > 0, 1f);
        yield return new WaitForSecondsRealtime(0.2f);
        ScreenCapture.CaptureScreenshot("review/shots/current/tip_guard.png");
        yield return null;
        yield return null;
        Say($"B guard tip: said {guardTips} line '{defence.LastCallout}' shown {rules.WasShown(Tip.Guard)}");

        // C: an enemy staggered beside the player straight after: too close behind the last tip, so it waits and is let go.
        var soldier = Find(EnemyKind.SoldierName, 0);
        yield return StaggerIt(soldier);
        float apart = Vector3.Distance(soldier.transform.position, player.transform.position);
        Say($"C stagger {Time.time - lastTipAt:0.0}s after the guard tip: staggered {soldier.IsStaggered} at {apart:0.0} m, waiting {rules.IsWaiting(Tip.Finish)}, tips said {TipsSaid}");
        yield return Until(() => Time.time - lastTipAt > 4.8f, 8f);
        Say($"C {Time.time - lastTipAt:0.0}s after: tips said {TipsSaid}, finish said {finishTips}, waiting {rules.IsWaiting(Tip.Finish)}");

        // D: staggered again once there is room.
        yield return Until(() => Time.time - lastTipAt > 6.2f, 8f);
        yield return StaggerIt(soldier);
        yield return Until(() => finishTips > 0, 1.5f);
        Say($"D finish tip: said {finishTips} line '{defence.LastCallout}' staggered {soldier.IsStaggered}");

        // E: light blows turned on a soldier's guard, twice within three seconds.
        yield return Until(() => Time.time - lastTipAt > 6.2f, 8f);
        var second = Find(EnemyKind.SoldierName, 1) ?? soldier;
        int turned = combat.BlowsTurned;
        for (int i = 0; i < 8 && breakTips == 0; i++)
        {
            yield return Until(() => !defence.IsHelpless && !combat.IsAttacking, 2f);
            if (second.IsDead) break;
            StandBefore(second, 1.5f);
            yield return null;
            yield return Tap(Key.J);
            yield return new WaitForSeconds(0.45f);
        }
        bool byHand = breakTips > 0;
        if (!byHand)
        {
            // The soldier would not hold its guard for the probe: report the two turned blows straight to the hook.
            Tips.BlowTurned();
            yield return new WaitForSeconds(0.5f);
            Tips.BlowTurned();
            yield return Until(() => breakTips > 0, 1.5f);
        }
        Say($"E break tip: blows turned by the soldier +{combat.BlowsTurned - turned}, said {breakTips} ({(byHand ? "from real blows" : "from the hook called by the probe")}) line '{defence.LastCallout}'");

        // F: every moment again, with room for a tip: none is said a second time.
        yield return Until(() => Time.time - lastTipAt > 6.5f, 8f);
        int before = TipsSaid;
        yield return Blow();
        yield return Blow();
        yield return new WaitForSeconds(1.2f);
        if (!soldier.IsDead) yield return StaggerIt(soldier);
        yield return new WaitForSeconds(1.2f);
        Tips.BlowTurned();
        yield return new WaitForSeconds(0.5f);
        Tips.BlowTurned();
        yield return new WaitForSeconds(1.5f);
        Say($"F again: tips said since +{TipsSaid - before}; totals guard {guardTips} break {breakTips} finish {finishTips}; smallest gap between tips {smallestGap:0.0}s");

        bool good = guardTips == 1 && breakTips == 1 && finishTips == 1 && smallestGap >= 6f;
        Say($"done: {(good ? "each tip once, at least 6s apart" : "NOT AS EXPECTED")} time scale {Time.timeScale:0.00}");
        Destroy(gameObject);
    }
}
