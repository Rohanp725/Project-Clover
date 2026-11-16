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

// Play Mode probe for the godly form at the summit: its entrance, what it chooses to do against a player standing in
// the south, the middle and under its head, whether each of its newer attacks hits, misses, and answers a guard as
// designed, its openings, its fury and its death. Writes "[Probe] ..." lines and pictures named storm2_*.png. Run with
//   unity command script run -- --file AgentScripts/StormProbe.cs --entry StormProbe.Run
public static class StormProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Storm Probe").AddComponent<StormProbeRunner>();
        return "probe started";
    }
}

public class StormProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private Colossus colossus;
    private bool keepAlive = true;
    private Vector3? hold;
    private readonly HashSet<string> pictured = new HashSet<string>();

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Update()
    {
        if (keepAlive && defence != null) defence.Health.Heal(100f);
        // Blows knock the player about; a place under test is a place stood in.
        if (hold.HasValue && player != null && !defence.IsHelpless)
        {
            Vector3 off = player.transform.position - hold.Value;
            off.y = 0f;
            if (off.magnitude > 0.8f) player.Teleport(hold.Value, Quaternion.identity);
        }
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Shot(string name)
    {
        ScreenCapture.CaptureScreenshot($"review/shots/current/storm2_{name}.png");
        yield return null;
        yield return null;
    }

    // One picture of a thing the first time it is asked for under that name.
    private IEnumerator ShotOnce(string name)
    {
        if (!pictured.Add(name)) yield break;
        yield return Shot(name);
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

    private static readonly Vector3 Middle = new Vector3(0f, 0.1f, 2f);
    private static readonly Vector3 South = new Vector3(0f, 0.1f, -10f);
    private static readonly Vector3 North = new Vector3(0f, 0.1f, 12.5f);

    // When in each attack its tell is at its clearest and when its blow is on the floor, in the attack's own seconds.
    private (float tell, float strike) Moments(string doing)
    {
        switch (doing)
        {
            case "Slam": return (colossus.SlamTelegraph * 0.8f, colossus.SlamTelegraph + colossus.SlamDrop + 0.08f);
            case "Sweep": return (colossus.SweepTelegraph * 0.8f, colossus.SweepTelegraph + colossus.SweepSeconds * 0.5f);
            case "Bolts": return (1.2f, 2.2f);
            case "Breath": return (colossus.BreathTelegraph * 0.85f, colossus.BreathTelegraph + colossus.BreathSeconds * 0.5f);
            case "Bite": return (colossus.BiteTelegraph * 0.85f, colossus.BiteTelegraph + colossus.BiteDrop + 0.3f);
            case "Clap": return (colossus.ClapTelegraph * 0.85f, colossus.ClapTelegraph + colossus.ClapSeconds + 0.1f);
            default: return (-1f, -1f);
        }
    }

    // Stands in one place and writes down what he does, with a picture of each kind of attack's tell and blow.
    private IEnumerator Watch(string place, Vector3 where, float seconds, int attacks = 0)
    {
        player.Teleport(where, Quaternion.identity);
        hold = where;
        string seen = "";
        string last = colossus.Doing;
        int count = 0;
        int taken = defence.BlowsTaken;
        int lulls = colossus.Lulls;
        float end = Time.time + seconds;
        while (Time.time < end && (attacks == 0 || count < attacks || colossus.Doing != "Idle"))
        {
            string doing = colossus.Doing;
            if (doing != last)
            {
                last = doing;
                if (doing != "Idle" && doing != "Recovering")
                {
                    seen += doing + " ";
                    count++;
                }
            }
            var (tell, strike) = Moments(doing);
            if (tell > 0f)
            {
                if (colossus.DoingFor >= tell && colossus.DoingFor < strike) yield return ShotOnce($"{place}_{doing.ToLower()}_tell");
                else if (colossus.DoingFor >= strike) yield return ShotOnce($"{place}_{doing.ToLower()}_strike");
            }
            yield return null;
        }
        hold = null;
        Say($"{place}: he chose {seen}; lulls +{colossus.Lulls - lulls}; blows that landed on the player +{defence.BlowsTaken - taken}");
    }

    // Has him make one attack of the probe's choosing against a player placed by the probe, and waits for it to begin.
    private IEnumerator Begin(ColossusAttack attack, Vector3 where)
    {
        yield return Until(() => colossus.Doing == "Idle" && !defence.IsHelpless, 14f);
        colossus.Queued = attack;
        player.Teleport(where, Quaternion.identity);
        hold = where;
        yield return Until(() => colossus.Doing == attack.ToString(), 8f);
    }

    private IEnumerator Over(ColossusAttack attack)
    {
        yield return Until(() => colossus.Doing != attack.ToString(), 10f);
        hold = null;
        Keys();
    }

    // The four things asked of every newer attack: it hits who stands, misses who has left its mark, and answers a
    // held guard and a guard at the last instant as it was designed to.
    private IEnumerator Try(ColossusAttack attack, Vector3 where, Vector3 away, float blowAt, bool guardable, float balanceWanted)
    {
        string name = attack.ToString();
        // Standing still.
        int taken = defence.BlowsTaken;
        yield return Begin(attack, where);
        yield return Over(attack);
        int still = defence.BlowsTaken - taken;

        // Leaving the mark once it has stopped following.
        taken = defence.BlowsTaken;
        yield return Begin(attack, where);
        yield return Until(() => colossus.DoingFor >= blowAt - 0.22f, 6f);
        hold = away;
        player.Teleport(away, Quaternion.identity);
        yield return Over(attack);
        int left = defence.BlowsTaken - taken;

        // Guard held throughout.
        taken = defence.BlowsTaken;
        int blocks = defence.Blocks;
        float staminaBefore = player.Stamina.Current;
        yield return Begin(attack, where);
        Keys(Key.L);
        yield return Over(attack);
        int heldTaken = defence.BlowsTaken - taken;
        int heldBlocks = defence.Blocks - blocks;

        // Guard pressed just before the blow.
        int perfect = defence.PerfectGuards;
        float balanceBefore = 0f, balanceAfter = 0f;
        bool broke = false;
        float[] leads = { 0.1f, 0.14f, 0.07f, 0.17f, 0.12f };
        int tries = 0;
        for (; tries < leads.Length && defence.PerfectGuards == perfect; tries++)
        {
            yield return Begin(attack, where);
            int staggers = colossus.TimesStaggered;
            balanceBefore = colossus.Stagger.Value;
            float lead = leads[tries];
            yield return Until(() => colossus.DoingFor >= blowAt - lead, 6f);
            Keys(Key.L);
            yield return Until(() => defence.PerfectGuards > perfect || colossus.DoingFor >= blowAt + 0.3f || colossus.Doing != name, 2f);
            balanceAfter = colossus.Stagger.Value;
            broke = colossus.TimesStaggered > staggers;
            Keys();
            yield return Over(attack);
        }
        Say($"{name}: standing still it landed {still}; having left the mark {left}; guard held: blocked {heldBlocks} landed {heldTaken} (wanted {(guardable ? "blocked" : "landed")}); "
            + $"perfect guards +{defence.PerfectGuards - perfect} in {tries} tries, his balance {balanceBefore:0} -> {(broke ? "broken" : balanceAfter.ToString("0"))} (wanted +{balanceWanted:0})");
        if (colossus.IsStaggered) yield return Until(() => !colossus.IsStaggered, 8f);
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        var summit = FindAnyObjectByType<SummitDirector>();
        var boss = FindAnyObjectByType<Enemy>();
        colossus = FindAnyObjectByType<Colossus>();
        var rods = FindAnyObjectByType<StormRods>();
        var hud = FindAnyObjectByType<Clover.UI.BossHud>();

        // 1: the swordsman falls; the entrance.
        player.Teleport(Middle, Quaternion.identity);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        yield return Until(() => colossus.HasRisen, 8f);
        float risenAt = Time.time;
        int takenBefore = defence.BlowsTaken;
        bool barDuring = colossus.InFight;
        float[] moments = { colossus.EntranceLeftSlamAt - 0.15f, colossus.EntranceLeftSlamAt + 0.04f, colossus.EntranceRightSlamAt + 0.04f,
            colossus.EntranceRoarAt - 0.6f, colossus.EntranceRoarAt - 0.03f, colossus.EntranceRoarAt + 0.12f, colossus.EntranceRoarAt + 0.45f };
        string[] names = { "left_raised", "left_down", "right_down", "hauling", "reared", "roar", "roar_ring" };
        var outcome = HitOutcome.Landed;
        float healthBefore = colossus.Health.Current;
        for (int i = 0; i < moments.Length; i++)
        {
            float at = moments[i];
            yield return Until(() => colossus.DoingFor >= at || colossus.Doing != "Rising", 6f);
            yield return Shot("entrance_" + names[i]);
            if (i == 2)
            {
                // His hands are on the floor: nothing the player does to them counts yet.
                outcome = colossus.LeftPart.TakeHit(new Hit(60f, 50f, Vector3.forward));
                colossus.Unbalance(10000f);
            }
        }
        yield return Until(() => colossus.Doing != "Rising", 6f);
        Say($"1 entrance: took {Time.time - risenAt:0.00} s; bar shown during it {barDuring}, after it {colossus.InFight}; a blow on his planted hand {outcome}, health {healthBefore:0} -> {colossus.Health.Current:0}, staggered {colossus.IsStaggered}; "
            + $"blows on the player during it {defence.BlowsTaken - takenBefore}; callout '{defence.LastCallout}'");
        yield return null;
        yield return Shot("entrance_begun");

        // 2: what he chooses, by where the player stands. The middle first: this is his opening pattern.
        yield return Watch("middle", Middle, 60f, 4);
        yield return Watch("south", South, 34f);
        yield return Watch("north", North, 34f);
        Say($"2 counts so far: slams {colossus.Slams} sweeps {colossus.Sweeps} bolts {colossus.BoltsCalled} breaths {colossus.Breaths} bites {colossus.Bites} claps {colossus.Claps} lulls {colossus.Lulls}");

        // 3: each newer attack against a still player, one who leaves, a held guard and a perfect guard.
        colossus.Health.Fill();
        yield return Try(ColossusAttack.Breath, new Vector3(3f, 0.1f, 0f), new Vector3(11f, 0.1f, 0f), colossus.BreathTelegraph, false, colossus.PerfectGuardBalance * 0.5f);
        yield return Try(ColossusAttack.Bite, North, new Vector3(0f, 0.1f, 3f), colossus.BiteTelegraph + colossus.BiteDrop, true, colossus.PerfectGuardBalance);
        yield return Try(ColossusAttack.Clap, Middle, new Vector3(0f, 0.1f, -5f), colossus.ClapTelegraph + colossus.ClapSeconds, true, colossus.PerfectGuardBalance * colossus.ClapPerfectGuards);

        // 4: the openings. The head stuck after a bite takes a blow at plain damage and cannot be finished.
        colossus.Health.Fill();
        colossus.Stagger.Reset();
        yield return Begin(ColossusAttack.Bite, North);
        yield return Until(() => colossus.HeadPart.Vulnerable || colossus.Doing != "Bite", 6f);
        hold = null;
        float stuckAt = Time.time;
        yield return Until(() => !defence.IsHelpless, 2f);
        // Wherever the blow left the player: the opening has to be usable from there, without being carried to it.
        float fromHead = Vector3.Distance(player.transform.position, colossus.HeadPart.GetComponent<Collider>().bounds.center);
        yield return null;
        yield return Shot("bite_stuck");
        float health = colossus.Health.Current;
        int landed = combat.BlowsLanded;
        int finishers = combat.Finishers;
        yield return TapWhenFree(Key.K, () => combat.BlowsLanded > landed);
        bool whileStuck = colossus.Doing == "Bite";
        float blowDamage = health - colossus.Health.Current;
        yield return Until(() => colossus.Doing != "Bite", 4f);
        Say($"4 bite opening: head open {whileStuck}; the player was left {fromHead:0.0} m from the middle of the head; a heavy blow landed +{combat.BlowsLanded - landed}, health {health:0} -> {health - blowDamage:0}; finishers +{combat.Finishers - finishers}; staggered {colossus.IsStaggered}; stuck for {Time.time - stuckAt:0.00} s");
        yield return Until(() => !combat.IsAttacking, 2f);

        // The hands stunned after a clap.
        colossus.Stagger.Reset();
        yield return Begin(ColossusAttack.Clap, Middle);
        yield return Until(() => (colossus.LeftPart.Vulnerable && colossus.RightPart.Vulnerable) || colossus.Doing != "Clap", 6f);
        hold = null;
        float openAt = Time.time;
        int staggersBefore = colossus.TimesStaggered;
        yield return Until(() => !defence.IsHelpless, 2f);
        yield return new WaitForSeconds(0.25f);
        player.Teleport(colossus.LeftPart.transform.position + new Vector3(0f, 0.1f, -2.6f), Quaternion.identity);
        yield return null;
        yield return Shot("clap_stunned");
        health = colossus.Health.Current;
        landed = combat.BlowsLanded;
        bool bothOpen = colossus.LeftPart.Vulnerable && colossus.RightPart.Vulnerable;
        yield return TapWhenFree(Key.J, () => combat.BlowsLanded > landed);
        float after = colossus.Health.Current;
        yield return Until(() => colossus.Doing != "Clap", 5f);
        Say($"4 clap opening: both hands open {bothOpen}; a blow landed +{combat.BlowsLanded - landed}, health {health:0} -> {after:0}; open for {Time.time - openAt:0.00} s; staggered by it {colossus.TimesStaggered > staggersBefore}");
        yield return Until(() => !combat.IsAttacking, 2f);

        // 5: fury.
        bool enraged = false;
        colossus.Enraged += _ => enraged = true;
        yield return Until(() => colossus.Doing == "Idle", 10f);
        colossus.Health.Fill();
        colossus.Health.Damage(colossus.MaximumHealth * 0.55f);
        player.Teleport(Middle, Quaternion.identity);
        hold = Middle;
        yield return null;
        yield return null;
        yield return Shot("fury_begins");
        string callout = defence.LastCallout;
        colossus.Queued = ColossusAttack.Bolts;
        yield return Until(() => colossus.Doing == "Bolts", 8f);
        int burnsBefore = colossus.PoolBurns;
        yield return Until(() => colossus.PoolsLaid >= 3, 8f);
        yield return Shot("fury_pools");
        yield return Until(() => colossus.Doing != "Bolts", 8f);
        yield return Shot("fury_pools_after");
        int poolsLaid = colossus.PoolsLaid;
        yield return new WaitForSeconds(1.2f);
        int burns = colossus.PoolBurns - burnsBefore;
        // Out of the pools, nothing burns.
        hold = new Vector3(-9f, 0.1f, -6f);
        player.Teleport(hold.Value, Quaternion.identity);
        colossus.Queued = ColossusAttack.Slam;
        yield return Until(() => colossus.Doing == "Slam", 8f);
        yield return Until(() => colossus.DoingFor >= colossus.SlamTelegraph * 0.8f, 4f);
        yield return Shot("fury_slam");
        float firstSlamOver = 0f;
        int slams = colossus.Slams;
        yield return Until(() => colossus.Doing == "Idle", 8f);
        firstSlamOver = Time.time;
        yield return Until(() => colossus.Doing == "Slam", 8f);
        float restInFury = Time.time - firstSlamOver;
        yield return Until(() => colossus.Doing == "Idle", 8f);
        Say($"5 fury: began {enraged} callout '{callout}' furious {colossus.Furious}; the bolts left {poolsLaid} pools, which burned the player standing in them {burns} times; slams one after the other +{colossus.Slams - slams + 1}, {restInFury:0.00} s apart");
        yield return Watch("fury", Middle, 16f);

        // 6: the death.
        hold = Middle;
        player.Teleport(Middle, Quaternion.identity);
        for (int attempt = 0; attempt < 12 && !colossus.IsDead; attempt++)
        {
            yield return Until(() => colossus.Doing == "Idle", 10f);
            colossus.Unbalance(10000f);
            yield return new WaitForSeconds(0.7f);
            for (int blow = 0; blow < 40 && !colossus.IsDead; blow++) colossus.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
        }
        float diedAt = Time.time;
        float realAt = Time.unscaledTime;
        yield return null;
        float scaleAtDeath = Time.timeScale;
        hold = new Vector3(0f, 0.1f, 6f);
        player.Teleport(hold.Value, Quaternion.identity);
        float[] deathMoments = { 0.05f, 0.6f, colossus.DeathBoltsAt + 0.3f, colossus.DeathLeftBreaksAt + 0.15f, colossus.DeathRightBreaksAt + 0.2f,
            colossus.DeathHeadFallsAt + 0.3f, colossus.DeathHeadLandsAt + 0.05f, colossus.DeathHeadSlidesAt + 0.5f, colossus.DeathSeconds - 0.3f };
        string[] deathNames = { "stop", "rears", "bolts", "left_breaks", "right_breaks", "head_falls", "head_lands", "slides", "gone" };
        for (int i = 0; i < deathMoments.Length; i++)
        {
            float at = deathMoments[i];
            yield return Until(() => colossus.DoingFor >= at || colossus.Doing != "Dying", 8f);
            yield return Shot("death_" + deathNames[i]);
        }
        yield return Until(() => summit.Felled, 8f);
        Say($"6 death: time scale just after the last blow {scaleAtDeath:0.00}; from the last blow to the empty rim {Time.time - diedAt:0.00} game s, {Time.unscaledTime - realAt:0.00} real s; "
            + $"felled {summit.Felled} bar shown {colossus.InFight} gods felled {RunState.GodsFelled} callout '{defence.LastCallout}'");
        yield return new WaitForSecondsRealtime(summit.RewardDelay + 1.5f);
        yield return Shot("death_cleared");
        int rewards = FindObjectsByType<GodReward>(FindObjectsSortMode.None).Length;
        Say($"6 after: rewards shown {rewards}; storm rods running {rods != null && rods.enabled}; time scale {Time.timeScale:0.00}");
        hold = null;
        Say("done");
    }
}
