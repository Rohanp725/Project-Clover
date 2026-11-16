using System;
using System.Collections;
using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the Gate Captain, in the Castle scene: that his ground is his own, what he does and whether each
// attack can land, what a guard does to his blows, what his guard does to the player's, his second phase, the stagger
// and the finisher, and what he pays. Takes one picture of each attack's telegraph. Writes "[Probe] ..." lines. Run with
//   unity command script run -- --file AgentScripts/CaptainProbe.cs --entry CaptainProbe.Run
public static class CaptainProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Captain Probe").AddComponent<CaptainProbeRunner>();
        return "probe started";
    }
}

public class CaptainProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private AreaDirector director;
    private Enemy captain;
    private readonly List<Enemy> ward = new List<Enemy>();
    private readonly HashSet<string> pictured = new HashSet<string>();
    private readonly SortedDictionary<string, int[]> tally = new SortedDictionary<string, int[]>();
    private string lastAttack = "";
    private bool lastRed;
    private int lastStruck, lastTaken, lastBlocked, lastPerfect;
    private int plainGuarded, plainThrough, redGuarded, redThrough;
    private bool watching;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    // The probe is about whether things work, not whether it can survive.
    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
    }

    // Every frame while watching: which attack he is in, a picture of each one's mark the first time it is well shown,
    // and for each blow he strikes whether it reached the player and what the player's guard made of it.
    private void LateUpdate()
    {
        if (!watching || captain == null) return;
        var current = captain.Brain.Current;
        if (current != null)
        {
            lastAttack = current.Name;
            lastRed = current.Unblockable;
            if (!tally.ContainsKey(lastAttack)) tally[lastAttack] = new int[3];
            if (captain.Brain.State == EnemyState.Telegraph && captain.Brain.TelegraphProgress >= 0.72f && pictured.Add(lastAttack))
                ScreenCapture.CaptureScreenshot($"review/shots/current/{lastAttack}.png");
        }
        if (captain.BlowsStruck != lastStruck)
        {
            bool taken = defence.BlowsTaken != lastTaken, blocked = defence.Blocks != lastBlocked, perfect = defence.PerfectGuards != lastPerfect;
            if (tally.TryGetValue(lastAttack, out var counts))
            {
                counts[0]++;
                if (taken || blocked || perfect) counts[1]++;
                if (taken) counts[2]++;
            }
            if (blocked && !lastRed) plainGuarded++;
            if (taken && !lastRed) plainThrough++;
            if (blocked && lastRed) redGuarded++;
            if (taken && lastRed) redThrough++;
        }
        lastStruck = captain.BlowsStruck;
        lastTaken = defence.BlowsTaken;
        lastBlocked = defence.Blocks;
        lastPerfect = defence.PerfectGuards;
    }

    private string Tally()
    {
        var parts = new List<string>();
        foreach (var pair in tally)
            parts.Add($"{pair.Key.Replace("captain_", "")} struck {pair.Value[0]} reached {pair.Value[1]} hurt {pair.Value[2]}");
        return string.Join("; ", parts);
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
            defence.Restore();
            yield return Until(() => !defence.IsHelpless && !combat.IsAttacking && !player.Dodge.IsRolling, 3f);
            Keys(key);
            yield return null;
            yield return null;
            Keys();
            yield return Until(worked, 0.8f);
        }
    }

    // Puts the player in front of him at a distance, kept on his ground so he does not lose interest.
    private void StandBefore(float distance)
    {
        Vector3 at = captain.transform.position + captain.transform.forward * distance;
        at.x = Mathf.Clamp(at.x, -16f, 16f);
        at.z = Mathf.Clamp(at.z, 114.6f, 120f);
        Vector3 look = captain.transform.position - at;
        look.y = 0f;
        player.Teleport(at, look.sqrMagnitude > 0.01f ? Quaternion.LookRotation(look) : Quaternion.identity);
    }

    // Stands at one distance after another for a while, moving only between his attacks so each can land.
    private IEnumerator Watch(float seconds, float[] distances)
    {
        float end = Time.unscaledTime + seconds, movedAt = -10f;
        int at = 0;
        while (Time.unscaledTime < end)
        {
            if (captain.Brain.State == EnemyState.Stalk && Time.unscaledTime - movedAt > 2.2f)
            {
                StandBefore(distances[at++ % distances.Length]);
                movedAt = Time.unscaledTime;
            }
            yield return null;
        }
        // An attack begun inside the time is watched to its end, so it is counted.
        yield return Until(() => !captain.Brain.IsAttacking, 5f);
        yield return null;
    }

    private int WardAware()
    {
        int aware = 0;
        foreach (var enemy in ward) if (enemy.IsAware) aware++;
        return aware;
    }

    private float WardNorthmost()
    {
        float north = 0f;
        foreach (var enemy in ward) north = Mathf.Max(north, enemy.transform.position.z);
        return north;
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        director = FindAnyObjectByType<AreaDirector>();
        foreach (var enemy in director.Enemies)
        {
            if (enemy.Kind.Name == EnemyKind.CaptainName) captain = enemy;
            else if (enemy.transform.position.y > 3f) ward.Add(enemy);
        }
        var plate = captain.transform.Find("Bars/Name Plate");
        var plateText = plate != null ? plate.GetComponent<UnityEngine.UI.Text>() : null;
        Vector3 post = captain.transform.position;
        Say($"start: {captain.Kind.Name} at {post.x:0.0},{post.z:0.0} health {captain.Health.Current:0} balance {captain.Stagger.Maximum:0} scale {captain.Kind.Scale:0.00}; ward enemies {ward.Count}; enemies alive {director.EnemiesAlive}; name plate '{(plateText != null ? plateText.text : "none")}'");
        yield return new WaitForSeconds(0.3f);

        // A1: the south end of the ward. The five there wake; he does not.
        player.Teleport(new Vector3(0f, 4.1f, 109.5f), Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        float toCaptain = Vector3.Distance(player.transform.position, captain.transform.position);
        Say($"A1 south of his line (z 109.5, {toCaptain:0.0} m from him): ward aware {WardAware()}/{ward.Count}, captain aware {captain.IsAware}");

        // A2: over the line with all five in pursuit. They stop at it and let the player go; he wakes.
        player.Teleport(new Vector3(0f, 4.1f, 117.5f), Quaternion.LookRotation(Vector3.forward));
        yield return new WaitForSeconds(0.5f);
        int stillChasing = WardAware();
        bool plateUp = plate != null && plate.gameObject.activeInHierarchy;
        yield return new WaitForSeconds(4.2f);
        int lingering = WardAware();
        float northmost = WardNorthmost();
        yield return new WaitForSeconds(2.3f);
        Say($"A2 on his ground with them in pursuit: ward aware after 0.5 s {stillChasing}, after 4.7 s {lingering}, after 7 s {WardAware()}; northmost of them z {northmost:0.0} (their ground ends at 112.5); captain aware {captain.IsAware}, his name plate showing {plateUp}");

        // A3: walk to him from the stair side of his ground with nobody roused: rest first so everyone is back at their post.
        foreach (var enemy in director.Enemies) enemy.Restore();
        player.Teleport(new Vector3(15f, 4.1f, 114.2f), Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        int rousedOnTheWay = 0;
        float walkEnd = Time.unscaledTime + 6f;
        Keys(Key.A);
        while (Time.unscaledTime < walkEnd && player.transform.position.x > 2f)
        {
            Keys(player.transform.position.z < 116.5f ? new[] { Key.A, Key.W } : new[] { Key.A });
            rousedOnTheWay = Mathf.Max(rousedOnTheWay, WardAware());
            yield return null;
        }
        Keys();
        yield return new WaitForSeconds(0.3f);
        ScreenCapture.CaptureScreenshot("review/shots/current/captain_meets.png");
        yield return null;
        Vector3 walked = player.transform.position;
        Say($"A3 walked along his ground to {walked.x:0.0},{walked.z:0.0}: most ward enemies aware at any moment {rousedOnTheWay}, captain aware {captain.IsAware}, state {captain.Brain.State}");

        // B: forty seconds of him, from every distance, taking it on the chin.
        watching = true;
        lastStruck = captain.BlowsStruck;
        lastTaken = defence.BlowsTaken;
        lastBlocked = defence.Blocks;
        lastPerfect = defence.PerfectGuards;
        yield return Watch(40f, new[] { 3f, 1.4f, 4f, 6.5f, 2.6f, 8f, 5.5f });
        Say($"B 40 s unguarded, phase {captain.Brain.Phase}: {Tally()}; ward aware {WardAware()}; charged by the storm {captain.IsCharged}");

        // C: the same with the guard held throughout.
        tally.Clear();
        plainGuarded = plainThrough = redGuarded = redThrough = 0;
        float guardEnd = Time.unscaledTime + 60f, moved = -10f, restored = 0f;
        int place = 0;
        float[] guardDistances = { 8.6f, 3f, 9f, 2f, 8.6f, 6.5f, 9.4f };
        while (Time.unscaledTime < guardEnd && !(plainGuarded >= 4 && redThrough >= 2))
        {
            Keys(Key.L);
            if (Time.unscaledTime - restored > 0.5f && !defence.IsHelpless)
            {
                // Stamina is topped up so the question is what the guard does, not how long it lasts.
                player.Stamina.Fill();
                restored = Time.unscaledTime;
            }
            if (captain.Brain.State == EnemyState.Stalk && Time.unscaledTime - moved > 2.2f)
            {
                StandBefore(guardDistances[place++ % guardDistances.Length]);
                moved = Time.unscaledTime;
            }
            yield return null;
        }
        Keys();
        Say($"C guard held: plain blows turned {plainGuarded}, plain blows that got through {plainThrough}; red blows turned {redGuarded}, red blows that went through the guard {redThrough}; by attack: {Tally()}");
        watching = false;
        yield return new WaitForSeconds(0.6f);

        // D: his guard. Four light blows from the front are turned, and the fourth brings his answer.
        yield return Until(() => captain.Brain.State == EnemyState.Stalk, 6f);
        StandBefore(2.2f);
        yield return null;
        yield return Until(() => captain.Brain.State == EnemyState.Stalk, 6f);
        int guarded = captain.BlowsGuarded;
        float health = captain.Health.Current, balance = captain.Stagger.Value;
        int turned = 0;
        string answer = "none";
        float answeredAfter = -1f;
        for (int blow = 0; blow < 4; blow++)
        {
            if (captain.Brain.State != EnemyState.Stalk) break;
            var outcome = captain.TakeHit(new Hit(10f, 10f, captain.transform.forward * -1f));
            if (outcome == HitOutcome.Blocked) turned++;
        }
        float struckAt = Time.time;
        yield return Until(() => captain.Brain.State == EnemyState.Telegraph, 1f);
        if (captain.Brain.State == EnemyState.Telegraph)
        {
            answer = captain.Brain.Current.Name;
            answeredAfter = Time.time - struckAt;
        }
        Say($"D his guard: 4 light blows of 10: turned {turned} (counted {captain.BlowsGuarded - guarded}), health {health:0.0} -> {captain.Health.Current:0.0}, balance {balance:0} -> {captain.Stagger.Value:0}; his answer {answer} begun {answeredAfter:0.00} s later (telegraph {(captain.Brain.Current != null ? captain.Brain.Current.Telegraph : 0f):0.00} s)");

        // D2: a heavy blow on his guard, and a heavy blow into an attack he is showing.
        yield return Until(() => captain.Brain.State == EnemyState.Stalk, 6f);
        health = captain.Health.Current;
        balance = captain.Stagger.Value;
        var heavyOutcome = captain.TakeHit(new Hit(30f, 34f, captain.transform.forward * -1f, true));
        string onGuard = $"{heavyOutcome}, health {health:0.0} -> {captain.Health.Current:0.0}, balance {balance:0} -> {captain.Stagger.Value:0}";
        StandBefore(3f);
        yield return Until(() => captain.Brain.State == EnemyState.Telegraph && captain.Brain.TelegraphProgress > 0.3f, 8f);
        string showing = captain.Brain.Current != null ? captain.Brain.Current.Name : "none";
        health = captain.Health.Current;
        var intoAttack = captain.TakeHit(new Hit(30f, 0f, captain.transform.forward * -1f, true));
        yield return null;
        Say($"D2 heavy blows: on his guard {onGuard}; into {showing} while shown: {intoAttack}, health {health:0.0} -> {captain.Health.Current:0.0}, still {captain.Brain.State} {(captain.Brain.Current != null ? captain.Brain.Current.Name : "nothing")}");

        // E: a perfect guard throws his blow back on his balance.
        yield return Until(() => captain.Brain.State == EnemyState.Stalk, 6f);
        yield return new WaitForSeconds(2.2f);
        float balanceBefore = captain.Stagger.Value;
        int perfect = defence.PerfectGuards;
        string perfectAgainst = "none";
        for (int attempt = 0; attempt < 10 && defence.PerfectGuards == perfect; attempt++)
        {
            defence.Restore();
            StandBefore(2.8f);
            yield return Until(() => captain.Brain.State == EnemyState.Telegraph && !captain.Brain.Current.Unblockable && !captain.Brain.Current.IsCircle
                && (1f - captain.Brain.TelegraphProgress) * captain.Brain.Current.Telegraph <= 0.1f, 6f);
            if (captain.Brain.Current != null) perfectAgainst = captain.Brain.Current.Name;
            balanceBefore = captain.Stagger.Value;
            Keys(Key.L);
            yield return new WaitForSeconds(0.25f);
            Keys();
            yield return new WaitForSeconds(0.6f);
        }
        Say($"E perfect guard against {perfectAgainst}: perfect guards +{defence.PerfectGuards - perfect}, his balance {balanceBefore:0} -> {captain.Stagger.Value:0} of {captain.Stagger.Maximum:0}");

        // F: worn down to half, the second phase.
        bool phased = false;
        captain.PhaseChanged += _ => phased = true;
        yield return Until(() => captain.Brain.State == EnemyState.Stalk, 6f);
        StandBefore(5f);
        while (captain.Health.Current > captain.Kind.Health * 0.49f) captain.TakeHit(new Hit(10f, 0f, captain.transform.forward, true, true));
        yield return Until(() => phased, 3f);
        string opening = captain.Brain.Current != null ? captain.Brain.Current.Name : "none";
        Say($"F second phase: entered {phased} at health {captain.Health.Current:0} of {captain.Kind.Health:0}, phase {captain.Brain.Phase}, opening attack {opening}");
        tally.Clear();
        watching = true;
        yield return Watch(40f, new[] { 2.8f, 7f, 3f, 6f, 2.4f, 8.5f, 3.2f });
        watching = false;
        Say($"F2 40 s of the second phase, unguarded: {Tally()}");

        // G: break his balance and finish him.
        yield return Until(() => captain.Brain.State == EnemyState.Stalk, 6f);
        captain.TakeHit(new Hit(1f, 10000f, captain.transform.forward, true, true));
        yield return null;
        bool staggered = captain.IsStaggered;
        float before = captain.Health.Current;
        StandBefore(2.6f);
        yield return new WaitForSeconds(0.5f);
        ScreenCapture.CaptureScreenshot("review/shots/current/captain_staggered.png");
        yield return null;
        int finishers = combat.Finishers;
        yield return TapWhenFree(Key.K, () => combat.Finishers > finishers);
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"G finisher with {combat.Weapon.Name}: staggered {staggered}, finishers +{combat.Finishers - finishers}, health {before:0} -> {captain.Health.Current:0} (a finisher takes at least {captain.Kind.Health * captain.Kind.FinisherHealthShare:0}), dead {captain.IsDead}");

        // H: fell him, and count what he pays.
        int embers = RunState.Points, alive = director.EnemiesAlive;
        for (int blow = 0; blow < 60 && !captain.IsDead; blow++) captain.TakeHit(new Hit(20f, 0f, captain.transform.forward, true, true));
        yield return null;
        int paid = RunState.Points - embers;
        yield return new WaitForSeconds(0.6f);
        ScreenCapture.CaptureScreenshot("review/shots/current/captain_falls.png");
        yield return new WaitForSeconds(2.6f);
        var way = FindAnyObjectByType<SealedWay>();
        player.Teleport(way.transform.position + new Vector3(0f, 0.1f, -1f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        Keys(Key.E);
        yield return null;
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.2f);
        Say($"H he falls: dead {captain.IsDead}, embers paid {paid} (a soldier pays {EnemyKind.Soldier().Embers}), enemies alive {alive} -> {director.EnemiesAlive}, body cleared {!captain.gameObject.activeSelf}; the door says '{defence.LastCallout}'");

        Say($"done: pictures of {string.Join(" ", pictured)}; time scale {Time.timeScale:0.00}");
        Destroy(gameObject);
    }
}
