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

// Play Mode probe for the combat feedback, run in the Castle: stages the low-health edge, the brackets under the enemy
// an attack would turn to, the cut lines, the framed bars, the mark over a staggered enemy and a death burning away,
// and takes a screenshot of each at the instant it shows (review/shots/current/fb_*.png). Run in Play Mode with
//   unity command script run -- --file AgentScripts/FeedbackProbe.cs --entry FeedbackProbe.Run
public static class FeedbackProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Feedback Probe").AddComponent<FeedbackProbeRunner>();
        return "probe started";
    }
}

public class FeedbackProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private AreaDirector director;
    private bool keepAlive;

    private void Update()
    {
        if (keepAlive && defence != null) defence.Health.Heal(100f);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private static IEnumerator Shot(string name)
    {
        ScreenCapture.CaptureScreenshot("review/shots/current/fb_" + name + ".png");
        yield return null;
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
    }

    private Enemy Find(string kind, int which)
    {
        foreach (var enemy in director.Enemies)
            if (enemy.Kind.Name == kind && which-- == 0) return enemy;
        return null;
    }

    private void StandBefore(Enemy enemy, float distance)
    {
        Vector3 at = enemy.transform.position + enemy.transform.forward * distance;
        player.Teleport(at, Quaternion.LookRotation(-enemy.transform.forward, Vector3.up));
    }

    // Presses a button until a blow lands, and takes the picture in the frame it does.
    private IEnumerator BlowAndShot(Key key, string name, Enemy on)
    {
        int cuts = CutMarks.Drawn;
        for (int attempt = 0; attempt < 5; attempt++)
        {
            yield return Until(() => !defence.IsHelpless && !combat.IsAttacking && !player.Dodge.IsRolling, 3f);
            StandBefore(on, 1.5f);
            yield return null;
            int blows = combat.BlowsLanded;
            yield return Tap(key);
            yield return Until(() => combat.BlowsLanded > blows, 1.5f);
            if (CutMarks.Drawn > cuts) break;
        }
        var reticle = player.GetComponent<TargetReticle>();
        Say($"{name}: strokes drawn +{CutMarks.Drawn - cuts} brackets flaring {reticle.IsFlaring} time scale {Time.timeScale:0.00}");
        yield return Shot(name);
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        director = FindAnyObjectByType<AreaDirector>();
        var reticle = player.GetComponent<TargetReticle>();
        var warning = FindAnyObjectByType<LowHealthVignette>();
        yield return new WaitForSeconds(0.5f);

        // In the Summit only the mark over the fallen head of the godly form is looked at.
        var colossus = FindAnyObjectByType<Colossus>(FindObjectsInactive.Include);
        if (colossus != null)
        {
            keepAlive = true;
            colossus.Rise();
            yield return new WaitForSeconds(colossus.RiseSeconds + 0.5f);
            colossus.Unbalance(100000f);
            yield return new WaitForSeconds(1.1f);
            var headMark = colossus.GetComponentInChildren<StaggerMark>(true);
            player.Teleport(new Vector3(0f, 0.1f, colossus.FloorRadius - 13.5f), Quaternion.identity);
            yield return new WaitForSeconds(0.4f);
            Say($"2 colossus: staggered {colossus.IsStaggered} mark shown {headMark != null && headMark.IsShown}");
            yield return Shot("stagger_colossus");
            yield return new WaitForSeconds(colossus.StaggeredSeconds);
            Say($"2 colossus after: staggered {colossus.IsStaggered} mark shown {headMark != null && headMark.IsShown}");
            Say("done: summit");
            Destroy(gameObject);
            yield break;
        }

        // 5: low health, staged first while nothing in the castle knows the player is there.
        Say($"5 start: health {defence.Health.Fraction:0.00} edge strength {warning.Strength:0.00} brackets on {(reticle.Target != null ? reticle.Target.name : "nothing")}");
        defence.Health.Damage(defence.Health.Maximum * 0.74f);
        yield return new WaitForSecondsRealtime(1.2f);
        Say($"5 hurt: health {defence.Health.Fraction:0.00} edge strength {warning.Strength:0.00}");
        yield return Shot("lowhealth_26");
        defence.Health.Damage(defence.Health.Maximum * 0.16f);
        // Two pictures half a breath apart, to see it breathe.
        yield return new WaitForSecondsRealtime(1.0f);
        Say($"5 worst: health {defence.Health.Fraction:0.00} edge strength {warning.Strength:0.00}");
        yield return Shot("lowhealth_10_a");
        yield return new WaitForSecondsRealtime(0.62f);
        yield return Shot("lowhealth_10_b");
        defence.Health.Heal(1000f);
        yield return new WaitForSecondsRealtime(1.0f);
        Say($"5 healed: health {defence.Health.Fraction:0.00} edge strength {warning.Strength:0.00}");
        keepAlive = true;

        // 3: the brackets under whoever the next attack would turn to.
        var soldier = Find(EnemyKind.SoldierName, 0);
        StandBefore(soldier, 2.2f);
        yield return new WaitForSeconds(0.45f);
        Say($"3 brackets: on {(reticle.Target != null ? reticle.Target.name : "nothing")} soldier is {soldier.name} aim target {(combat.AimTarget() != null ? combat.AimTarget().name : "nothing")}");
        yield return Shot("brackets");

        // 6 and 4: a light blow, its cut line, and the bars with the pale piece of what was lost.
        yield return BlowAndShot(Key.J, "cut_light", soldier);
        yield return new WaitForSecondsRealtime(0.22f);
        yield return Shot("bars_lag");
        yield return Until(() => !combat.IsAttacking, 2f);
        yield return BlowAndShot(Key.K, "cut_heavy", soldier);
        yield return Until(() => !combat.IsAttacking, 2f);

        // 2: the mark over a staggered enemy, as it lands and once it has.
        soldier.TakeHit(new Hit(1f, 1000f, Vector3.forward));
        StandBefore(soldier, 2.4f);
        yield return null;
        yield return null;
        var mark = soldier.GetComponentInChildren<StaggerMark>(true);
        Say($"2 stagger: staggered {soldier.IsStaggered} mark shown {mark != null && mark.IsShown}");
        yield return Shot("stagger_landing");
        yield return new WaitForSecondsRealtime(0.5f);
        yield return Shot("stagger");

        // 6: the finisher's crossed cut.
        int finishers = combat.Finishers;
        int strokes = CutMarks.Drawn;
        yield return Tap(Key.K);
        yield return Until(() => combat.Finishers > finishers, 2.5f);
        Say($"6 finisher: finishers +{combat.Finishers - finishers} strokes +{CutMarks.Drawn - strokes} mark shown {mark != null && mark.IsShown} soldier dead {soldier.IsDead}");
        yield return Shot("cut_finisher");
        yield return new WaitForSecondsRealtime(0.5f);
        Say($"2 after: staggered {soldier.IsStaggered} mark shown {mark != null && mark.IsShown}");
        yield return Until(() => !combat.IsAttacking && Time.timeScale > 0.99f, 3f);

        // 1: the death. The ember is credited at the blow; the mote and the count on screen come after.
        yield return new WaitForSeconds(2.5f);
        soldier = Find(EnemyKind.SoldierName, 1);
        StandBefore(soldier, 2.6f);
        yield return new WaitForSeconds(0.3f);
        int before = RunState.Points;
        int arrived = EmberMotes.Arrived;
        if (!soldier.IsDead) soldier.TakeHit(new Hit(1000f, 0f, -soldier.transform.forward));
        float died = Time.time;
        Say($"1 death: dead {soldier.IsDead} embers {before} -> {RunState.Points} at once, held back from the count {EmberMotes.Pending}");
        string[] names = { "death_fallen", "death_bolt", "death_ink", "death_breaking", "death_mote", "death_taken", "death_gone" };
        float[] at = { 0.8f, 0.99f, 1.2f, 1.5f, 1.4f, 1.62f, 2.3f };
        // In the order of time, whatever order they are listed in.
        Array.Sort(at, names);
        for (int i = 0; i < at.Length; i++)
        {
            float when = at[i];
            yield return Until(() => Time.time - died >= when, 4f);
            Say($"1 {names[i]} at {Time.time - died:0.00}: body active {soldier.gameObject.activeSelf} pending {EmberMotes.Pending} arrived +{EmberMotes.Arrived - arrived} embers {RunState.Points}");
            yield return Shot(names[i]);
        }

        // 4: a crowd. Every enemy takes a small blow so its bars show, and the player stands among the next pair.
        foreach (var enemy in director.Enemies)
            if (!enemy.IsDead) enemy.TakeHit(new Hit(14f, 20f, Vector3.forward, true));
        // Two of them shoulder to shoulder, so their bars would lie on one another if all were at one height.
        var third = Find(EnemyKind.SoldierName, 2);
        var fourth = Find(EnemyKind.SoldierName, 3);
        if (third != null && fourth != null)
        {
            var body = fourth.GetComponent<CharacterController>();
            body.enabled = false;
            fourth.transform.position = third.transform.position + new Vector3(0.9f, 0f, 0.5f);
            body.enabled = true;
            third.TakeHit(new Hit(20f, 30f, Vector3.forward, true));
            // From behind them, so they and their bars are in the lower half of the screen.
            StandBefore(third, -2.6f);
            yield return new WaitForSeconds(0.25f);
            yield return Shot("bars_crowd");
            yield return new WaitForSeconds(0.8f);
            yield return Shot("bars_crowd_later");
        }

        Say($"done: time scale {Time.timeScale:0.00} embers {RunState.Points} pending {EmberMotes.Pending}");
        Destroy(gameObject);
    }
}
