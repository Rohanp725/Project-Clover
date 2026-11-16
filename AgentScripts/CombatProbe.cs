using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the combat arena: plays through every mechanic with simulated keys at full speed and writes what
// happened to the console as "[Probe] ..." lines. Run in Play Mode with
//   unity command script run -- --file AgentScripts/CombatProbe.cs --entry CombatProbe.Run
public static class CombatProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Combat Probe").AddComponent<CombatProbeRunner>();
        return "probe started";
    }
}

public class CombatProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private DummyAttacker attacker;
    private TrainingDummy north;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Place(Vector3 position, Vector3 facing)
    {
        var body = player.GetComponent<CharacterController>();
        body.enabled = false;
        player.transform.SetPositionAndRotation(position, Quaternion.LookRotation(facing, Vector3.up));
        body.enabled = true;
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

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        attacker = FindAnyObjectByType<DummyAttacker>();
        foreach (var dummy in FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None))
            if (Vector3.Distance(dummy.transform.position, new Vector3(0f, 0f, 4f)) < 0.5f) north = dummy;
        Say($"start: health {defence.Health.Current:0} flasks {defence.Flasks.Count} mana {combat.Mana.Current:0} attacker {(attacker != null)} north {(north != null)}");
        yield return new WaitForSeconds(0.3f);

        // A: a tapped heavy.
        Place(new Vector3(0f, 0f, 1.6f), Vector3.forward);
        yield return null;
        int blows = combat.BlowsLanded;
        yield return Tap(Key.K);
        yield return Until(() => combat.Combo.Current != null, 1f);
        string name = combat.Combo.Current != null ? combat.Combo.Current.Name : "none";
        float windup = combat.Combo.Current != null ? combat.Combo.Current.Windup : 0f;
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"A heavy tap: attack {name} windup {windup:0.00} blows {combat.BlowsLanded - blows} dummy health {north.Health.Current:0} stagger {north.Stagger.Value:0}");
        yield return new WaitForSeconds(0.5f);

        // B: a light, then a heavy to end the chain.
        yield return Tap(Key.J);
        yield return new WaitForSeconds(0.12f);
        yield return Tap(Key.K);
        yield return Until(() => combat.Combo.Current != null && combat.Combo.Current.Heavy, 1f);
        name = combat.Combo.Current != null ? combat.Combo.Current.Name : "none";
        windup = combat.Combo.Current != null ? combat.Combo.Current.Windup : 0f;
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"B light then heavy: second attack {name} windup {windup:0.00} (chain heavy is 0.20)");
        yield return new WaitForSeconds(0.5f);

        // C: hold for the skill.
        combat.Mana.Gain(100f);
        float manaBefore = combat.Mana.Current;
        Keys(Key.K);
        yield return new WaitForSeconds(0.6f);
        bool charged = combat.Combo.IsCharged;
        Keys();
        yield return Until(() => combat.Combo.Current != null, 1f);
        name = combat.Combo.Current != null ? combat.Combo.Current.Name : "none";
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"C hold: charged {charged} attack {name} skills {combat.SkillsUsed} mana {manaBefore:0} -> {combat.Mana.Current:0}");
        yield return new WaitForSeconds(0.5f);

        // D: a heavy on a staggered dummy is a finisher.
        Place(new Vector3(0f, 0f, 1.6f), Vector3.forward);
        north.Stagger.Add(1000f);
        float healthBefore = north.Health.Current;
        yield return Tap(Key.K);
        yield return Until(() => combat.Combo.Current != null, 1f);
        name = combat.Combo.Current != null ? combat.Combo.Current.Name : "none";
        Place(new Vector3(0f, 0f, 1.6f), Vector3.forward);
        bool passedThrough = defence.TakeHit(new Hit(50f, 0f, Vector3.back)) == HitOutcome.Dodged;
        yield return Until(() => combat.Finishers > 0, 2f);
        Say($"D finisher move: attack {name} blows pass through {passedThrough}");
        Say($"D finisher: finishers {combat.Finishers} dummy health {healthBefore:0} -> {north.Health.Current:0} still staggered {north.Stagger.IsStaggered} time scale {Time.timeScale:0.00}");
        yield return Until(() => !combat.IsAttacking, 2f);
        yield return new WaitForSeconds(0.3f);

        // E: stand in front of the sparring dummy and take a blow.
        Vector3 spot = attacker.transform.position + new Vector3(0f, 0f, -2.2f);
        Place(spot, Vector3.forward);
        yield return Until(() => defence.BlowsTaken > 0, 6f);
        Say($"E unguarded blow: taken {defence.BlowsTaken} health {defence.Health.Current:0} hurt {defence.IsHurt}");
        yield return new WaitForSeconds(0.5f);

        // F: hold the guard through the next one.
        Place(spot, Vector3.forward);
        Keys(Key.L);
        yield return new WaitForSeconds(0.6f);
        yield return Until(() => defence.Blocks > 0 || defence.BlowsTaken > 1, 6f);
        Say($"F held guard: blocks {defence.Blocks} taken {defence.BlowsTaken} health {defence.Health.Current:0} grey {defence.Health.Grey:0.0} stamina {player.Stamina.Current:0} red {attacker.Cycle.IsRed}");
        Keys();
        yield return new WaitForSeconds(0.4f);

        // G: the third blow is red; guard it at the last moment.
        Place(spot, Vector3.forward);
        yield return Until(() => attacker.Cycle.Phase == EnemyAttackPhase.Telegraph && attacker.Cycle.TelegraphProgress >= 0.88f, 6f);
        bool red = attacker.Cycle.IsRed;
        Keys(Key.L);
        var attackerDummy = attacker.GetComponent<TrainingDummy>();
        yield return Until(() => defence.PerfectGuards > 0 || defence.BlowsTaken > 1, 2f);
        Say($"G perfect guard on red {red}: perfect {defence.PerfectGuards} taken {defence.BlowsTaken} mana {combat.Mana.Current:0} dummy stagger {attackerDummy.Stagger.Value:0}");
        yield return new WaitForSeconds(0.3f);

        // H: a red blow goes through a guard that is merely held.
        attacker.Cycle.RedEvery = 1;
        Place(spot, Vector3.forward);
        int takenBefore = defence.BlowsTaken;
        int blocksBefore = defence.Blocks;
        yield return Until(() => defence.BlowsTaken > takenBefore || defence.Blocks > blocksBefore, 6f);
        Say($"H red through held guard: taken +{defence.BlowsTaken - takenBefore} blocks +{defence.Blocks - blocksBefore} health {defence.Health.Current:0}");
        Keys();
        attacker.Cycle.RedEvery = 3;
        yield return new WaitForSeconds(0.6f);

        // I: roll away at the last moment.
        Place(spot, Vector3.forward);
        yield return Until(() => attacker.Cycle.Phase == EnemyAttackPhase.Telegraph && attacker.Cycle.TelegraphProgress >= 0.94f, 6f);
        manaBefore = combat.Mana.Current;
        takenBefore = defence.BlowsTaken;
        Keys(Key.S, Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.15f);
        float slowed = Time.timeScale;
        yield return new WaitForSeconds(0.5f);
        Say($"I perfect dodge: perfect {defence.PerfectDodges} plain {defence.Dodges} taken +{defence.BlowsTaken - takenBefore} mana {manaBefore:0} -> {combat.Mana.Current:0} time scale during {slowed:0.00} after {Time.timeScale:0.00}");

        // J: drink.
        Place(new Vector3(0f, 0f, -6f), Vector3.forward);
        healthBefore = defence.Health.Current;
        yield return Tap(Key.R);
        yield return new WaitForSeconds(0.2f);
        bool drinking = defence.IsDrinking;
        yield return new WaitForSeconds(1f);
        Say($"J flask: was drinking {drinking} health {healthBefore:0} -> {defence.Health.Current:0} flasks {defence.Flasks.Count}");

        // L: the guard covers the front only. No enemy is near here, so the guard does not turn.
        Place(new Vector3(0f, 0f, -12f), Vector3.forward);
        Keys(Key.L);
        yield return new WaitForSeconds(0.6f);
        var front = defence.TakeHit(new Hit(10f, 0f, Vector3.back));
        yield return new WaitForSeconds(0.1f);
        var behind = defence.TakeHit(new Hit(10f, 0f, Vector3.back, false, false, player.transform.position + Vector3.back * 2f));
        Keys();
        Say($"L directional guard: from the front {front}, from behind {behind}");
        yield return new WaitForSeconds(0.6f);

        // K: fall and stand up again.
        defence.TakeHit(new Hit(1000f, 0f, Vector3.back));
        yield return null;
        bool dead = defence.IsDead;
        yield return new WaitForSeconds(defence.ReviveAfterSeconds + 0.3f);
        Say($"K fall: dead {dead} deaths {defence.Deaths} then health {defence.Health.Current:0} flasks {defence.Flasks.Count}");

        Say($"done: time scale {Time.timeScale:0.00} callouts {defence.Callouts} last '{defence.LastCallout}'");
        Destroy(gameObject);
    }
}
