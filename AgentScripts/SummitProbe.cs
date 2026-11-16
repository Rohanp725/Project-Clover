using System;
using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the summit: watches the god attack, wears him down through his second phase, fells him, takes his
// element and goes back down. Writes what happened to the console as "[Probe] ..." lines. Run in Play Mode with
//   unity command script run -- --file AgentScripts/SummitProbe.cs --entry SummitProbe.Run
public static class SummitProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Summit Probe").AddComponent<SummitProbeRunner>();
        return "probe started";
    }
}

public class SummitProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private Enemy boss;
    private bool keepAlive = true;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Update()
    {
        if (keepAlive && defence != null) defence.Health.Heal(100f);
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator TapWhenFree(Key key, Func<bool> worked)
    {
        for (int attempt = 0; attempt < 6 && !worked(); attempt++)
        {
            yield return Until(() => !defence.IsHelpless && !combat.IsAttacking && !player.Dodge.IsRolling, 3f);
            Keys(key);
            yield return null;
            yield return null;
            Keys();
            yield return Until(worked, 0.8f);
        }
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        boss = FindAnyObjectByType<Enemy>();
        var summit = FindAnyObjectByType<SummitDirector>();
        Say($"start: boss {boss.Kind.Name} health {boss.Health.Current:0} weapon {combat.Weapon.Name} element {element.Kit.Name}");

        // A: stand in front of him and count what he does for a while.
        player.Teleport(boss.transform.position + boss.transform.forward * 3f, Quaternion.LookRotation(-boss.transform.forward));
        var seen = new System.Collections.Generic.HashSet<string>();
        float end = Time.unscaledTime + 14f;
        int taken = defence.BlowsTaken;
        while (Time.unscaledTime < end)
        {
            if (boss.Brain.Current != null) seen.Add(boss.Brain.Current.Name.Replace("boss_", ""));
            yield return null;
        }
        Say($"A he attacks: {string.Join(" ", seen)}; blows struck {boss.BlowsStruck}, blows taken by the player {defence.BlowsTaken - taken}");

        // B: a perfect guard throws his blow back on his balance. Hold guard pressed just before a blow that can be guarded.
        float staggerBefore = boss.Stagger.Value;
        int perfect = defence.PerfectGuards;
        for (int attempt = 0; attempt < 8 && defence.PerfectGuards == perfect; attempt++)
        {
            player.Teleport(boss.transform.position + boss.transform.forward * 2.6f, Quaternion.LookRotation(-boss.transform.forward));
            yield return Until(() => boss.Brain.State == EnemyState.Telegraph && !boss.Brain.Current.Unblockable && !boss.Brain.Current.IsCircle
                && boss.Brain.TelegraphProgress >= 0.8f, 6f);
            Keys(Key.L);
            yield return new WaitForSeconds(0.25f);
            Keys();
            yield return new WaitForSeconds(0.5f);
        }
        Say($"B perfect guard: perfect {defence.PerfectGuards - perfect} boss balance {staggerBefore:0} -> {boss.Stagger.Value:0}");

        // C: his guard turns light blows from the front.
        int guarded = boss.BlowsGuarded;
        for (int attempt = 0; attempt < 6 && boss.BlowsGuarded == guarded; attempt++)
        {
            yield return Until(() => boss.Brain.State == EnemyState.Stalk, 4f);
            boss.TakeHit(new Hit(8f, 6f, -boss.transform.forward));
            yield return null;
        }
        Say($"C his guard: blows turned {boss.BlowsGuarded - guarded}");

        // D: wear him down to the second phase.
        bool phased = false;
        boss.PhaseChanged += _ => phased = true;
        float to = boss.Kind.Health * 0.48f;
        while (boss.Health.Current > to) boss.TakeHit(new Hit(40f, 0f, boss.transform.forward, true));
        yield return Until(() => phased, 4f);
        string opening = boss.Brain.Current != null ? boss.Brain.Current.Name : "none";
        Say($"D second phase: entered {phased} phase {boss.Brain.Phase} opening attack {opening} health {boss.Health.Current:0}");
        yield return new WaitForSeconds(2.5f);

        // E: break his balance and finish him: a finisher takes a fixed share of a god.
        boss.TakeHit(new Hit(1f, 10000f, boss.transform.forward, true));
        yield return null;
        bool staggered = boss.IsStaggered;
        float healthBefore = boss.Health.Current;
        player.Teleport(boss.transform.position + boss.transform.forward * 2.5f, Quaternion.LookRotation(-boss.transform.forward));
        int finishers = combat.Finishers;
        yield return TapWhenFree(Key.K, () => combat.Finishers > finishers);
        Say($"E finisher: staggered {staggered} finishers +{combat.Finishers - finishers} health {healthBefore:0} -> {boss.Health.Current:0} (a fifth of {boss.Kind.Health:0} is {boss.Kind.Health * 0.2f:0})");
        yield return Until(() => !combat.IsAttacking, 2f);

        // F: fell him.
        while (!boss.IsDead) boss.TakeHit(new Hit(60f, 0f, boss.transform.forward, true));
        yield return null;
        Say($"F the swordsman falls: dead {boss.IsDead} second form called {summit.SecondFormCalled} felled yet {summit.Felled}");

        // F2: what he became rises out of the cloud.
        var colossus = FindAnyObjectByType<Colossus>();
        yield return Until(() => colossus != null && colossus.HasRisen, 6f);
        yield return new WaitForSeconds(1.2f);
        ScreenCapture.CaptureScreenshot("review/shots/current/storm_rising.png");
        yield return Until(() => colossus.Doing == "Idle", 6f);
        player.Teleport(new Vector3(0f, 0.1f, 6f), Quaternion.identity);
        yield return new WaitForSeconds(0.6f);
        ScreenCapture.CaptureScreenshot("review/shots/current/storm_risen.png");
        Say($"F2 risen: {colossus.HasRisen} doing {colossus.Doing} health {colossus.Health.Current:0} in fight {colossus.InFight}");

        // F3: stand in the middle and let it do its worst for a while; the probe keeps the player alive.
        int stormTaken = defence.BlowsTaken;
        string stormSeen = "";
        string stormLast = "";
        int shots = 0;
        float changedAt = 0f;
        bool pictured = true;
        float watchUntil = Time.time + 24f;
        while (Time.time < watchUntil)
        {
            if (colossus.Doing != stormLast)
            {
                stormLast = colossus.Doing;
                stormSeen += stormLast + " ";
                changedAt = Time.time;
                pictured = false;
            }
            // One picture of each attack, a second into it.
            if (!pictured && stormLast != "Idle" && Time.time - changedAt > 1f && shots < 8)
            {
                pictured = true;
                ScreenCapture.CaptureScreenshot($"review/shots/current/storm_{stormLast.ToLower()}_{shots}.png");
                shots++;
            }
            yield return null;
        }
        Say($"F3 it attacks: {stormSeen}; slams {colossus.Slams} sweeps {colossus.Sweeps} bolts {colossus.BoltsCalled}; struck the player {colossus.BlowsStruck} times, of which landed {defence.BlowsTaken - stormTaken}");

        // F4: a hand that has come down can be cut.
        yield return Until(() => colossus.LeftPart.Vulnerable || colossus.RightPart.Vulnerable, 20f);
        var open = colossus.LeftPart.Vulnerable ? colossus.LeftPart : colossus.RightPart;
        float stormHealth = colossus.Health.Current;
        player.Teleport(open.transform.position + new Vector3(0f, 0.1f, -2.6f), Quaternion.identity);
        yield return null;
        int landed = combat.BlowsLanded;
        yield return TapWhenFree(Key.J, () => combat.BlowsLanded > landed);
        yield return Until(() => combat.BlowsLanded > landed, 1f);
        Say($"F4 a hand lying open: vulnerable {open.Vulnerable} blows landed +{combat.BlowsLanded - landed} its health {stormHealth:0} -> {colossus.Health.Current:0} balance {colossus.Stagger.Value:0}");
        yield return Until(() => !combat.IsAttacking, 2f);

        // F5: break its balance: the head falls, and can be finished.
        yield return Until(() => colossus.Doing == "Idle", 8f);
        colossus.Unbalance(10000f);
        yield return new WaitForSeconds(0.9f);
        bool broken = colossus.IsStaggered;
        stormHealth = colossus.Health.Current;
        player.Teleport(colossus.HeadPart.transform.position + new Vector3(0f, 0.1f, -4.6f), Quaternion.identity);
        yield return new WaitForSeconds(0.2f);
        ScreenCapture.CaptureScreenshot("review/shots/current/storm_broken.png");
        finishers = combat.Finishers;
        yield return TapWhenFree(Key.K, () => combat.Finishers > finishers);
        Say($"F5 broken: staggered {broken} head open {colossus.HeadPart.Vulnerable} finishers +{combat.Finishers - finishers} health {stormHealth:0} -> {colossus.Health.Current:0} (22% of {colossus.MaximumHealth:0} is {colossus.MaximumHealth * 0.22f:0}) now {colossus.Doing}");
        yield return Until(() => !combat.IsAttacking, 2f);

        // F6: fell it.
        for (int attempt = 0; attempt < 12 && !colossus.IsDead; attempt++)
        {
            yield return Until(() => colossus.Doing == "Idle", 10f);
            colossus.Unbalance(10000f);
            yield return new WaitForSeconds(0.7f);
            for (int blow = 0; blow < 40 && !colossus.IsDead; blow++) colossus.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
        }
        yield return new WaitForSeconds(1f);
        ScreenCapture.CaptureScreenshot("review/shots/current/storm_dying.png");
        yield return Until(() => summit.Felled, 8f);
        Say($"F6 felled: storm dead {colossus.IsDead} director felled {summit.Felled} gods felled this run {RunState.GodsFelled}");
        yield return new WaitForSecondsRealtime(summit.RewardDelay + 2.5f);

        // G: take the storm.
        GodReward take = null;
        foreach (var reward in FindObjectsByType<GodReward>(FindObjectsSortMode.None))
            if (reward.Takes) take = reward;
        Say($"G rewards shown: {(take != null)} time scale {Time.timeScale:0.00}");
        if (take != null)
        {
            player.Teleport(take.transform.position + new Vector3(0f, 0.1f, -1.4f), Quaternion.identity);
            yield return new WaitForSeconds(0.4f);
            yield return TapWhenFree(Key.E, () => take.Chosen);
            Say($"G taken: chosen {take.Chosen} element {element.Kit.Name} spell {element.Kit.SpellName} weapon {combat.Weapon.Name} run element {RunState.Element}");
        }

        // H: the new element's surge and spell.
        combat.Mana.Gain(100f);
        yield return TapWhenFree(Key.I, () => element.Surges > 0);
        yield return TapWhenFree(Key.U, () => element.SpellsCast > 0);
        Say($"H lightning kit: surges {element.Surges} swing speed x{combat.SwingSpeed:0.0} spells {element.SpellsCast}");

        // I: the way down.
        var door = FindAnyObjectByType<SceneDoor>();
        Say($"I way down: open {door.Open}; going down. time scale {Time.timeScale:0.00}");
        player.Teleport(door.transform.position + new Vector3(0f, 0.1f, 1f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        Say("done");
        Keys(Key.E);
        yield return null;
        yield return null;
        Keys();
    }
}
