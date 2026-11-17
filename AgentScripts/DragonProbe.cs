using System;
using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the storm dragon at the summit: starts the godly form directly, pictures its entrance and its body
// in the cloud, forces each attack in turn (standing still, leaving the mark, rolling, guarding at the last instant) with
// pictures of the tell, the blow and what follows, strikes the head and the tail in their openings, then the stagger,
// the finisher, the fury and its storm, the death, and the choice of what he leaves. Writes "[Probe] ..." lines and
// pictures named dragon_*.png. Run with
//   bash AgentScripts/runprobe.sh Summit DragonProbe 300
// To try some parts alone, put their names (attacks, or Look, TailStrips, Storm, Openings, Stagger, Fury), with commas between, in AgentScripts/dragon_only.txt;
// an empty or missing file runs everything.
public static class DragonProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Dragon Probe").AddComponent<DragonProbeRunner>();
        return "probe started";
    }
}

public class DragonProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private StormDragon dragon;
    private bool keepAlive = true;
    private Vector3? hold;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Update()
    {
        if (keepAlive && defence != null) defence.Health.Heal(100f);
        // Blows knock the player about; a place under test is a place stood in.
        if (hold.HasValue && player != null && !defence.IsHelpless && !player.Dodge.IsRolling)
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
        ScreenCapture.CaptureScreenshot($"review/shots/current/dragon_{name}.png");
        yield return null;
        yield return null;
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

    private IEnumerator Begin(DragonAttack attack, Vector3 where)
    {
        yield return Until(() => dragon.Doing == "Idle" && !defence.IsHelpless && !player.Dodge.IsRolling, 16f);
        dragon.Queued = attack;
        player.Teleport(where, Quaternion.identity);
        hold = where;
        yield return Until(() => dragon.Doing == attack.ToString(), 8f);
    }

    // The attack is over when he is doing something else and nothing it left on the floor is still to fall.
    private IEnumerator Over(DragonAttack attack, float linger = 0f)
    {
        yield return Until(() => dragon.Doing != attack.ToString(), 14f);
        if (linger > 0f) yield return new WaitForSeconds(linger);
        hold = null;
        Keys();
    }

    // Everything asked of an attack: pictures of its tell, its blow and what follows; that it hits who stands, misses
    // who has left its mark, is rolled through, and answers a guard at the last instant.
    private IEnumerator Try(DragonAttack attack, Vector3 where, Vector3 away, float tellAt, float blowAt, float afterAt, float linger, float balanceWanted)
    {
        string name = attack.ToString();
        string file = name.ToLower();

        // Standing still, with the pictures.
        int taken = defence.BlowsTaken;
        yield return Begin(attack, where);
        yield return Until(() => dragon.DoingFor >= tellAt || dragon.Doing != name, 8f);
        yield return Shot(file + "_1");
        yield return Until(() => dragon.DoingFor >= blowAt + 0.05f || dragon.Doing != name, 8f);
        yield return Shot(file + "_2");
        if (afterAt > 0f)
        {
            yield return Until(() => dragon.DoingFor >= afterAt || dragon.Doing != name, 8f);
            yield return Shot(file + "_3");
        }
        yield return Over(attack, linger);
        int still = defence.BlowsTaken - taken;

        // Leaving the mark once it has stopped following.
        taken = defence.BlowsTaken;
        yield return Begin(attack, where);
        yield return Until(() => dragon.DoingFor >= Mathf.Max(0.2f, blowAt - 0.25f), 8f);
        hold = away;
        player.Teleport(away, Quaternion.identity);
        yield return Over(attack, linger);
        int left = defence.BlowsTaken - taken;

        // Rolling as it comes.
        int rolled = defence.Dodges + defence.PerfectDodges;
        int perfectRolls = defence.PerfectDodges;
        taken = defence.BlowsTaken;
        float[] rollLeads = { 0.12f, 0.22f, 0.05f };
        int rollTries = 0;
        for (; rollTries < rollLeads.Length && defence.Dodges + defence.PerfectDodges == rolled; rollTries++)
        {
            yield return Begin(attack, where);
            float lead = rollLeads[rollTries];
            yield return Until(() => dragon.DoingFor >= blowAt - lead, 8f);
            hold = null;
            Keys(Key.Space);
            yield return null;
            yield return null;
            Keys();
            yield return Over(attack, linger);
        }
        int rolls = defence.Dodges + defence.PerfectDodges - rolled;

        // Guard pressed just before the blow.
        int perfect = defence.PerfectGuards;
        float balanceBefore = 0f, balanceAfter = 0f;
        bool broke = false;
        float[] leads = { 0.1f, 0.14f, 0.07f, 0.18f };
        int tries = 0;
        for (; tries < leads.Length && defence.PerfectGuards == perfect; tries++)
        {
            dragon.Stagger.Reset();
            yield return Begin(attack, where);
            int staggers = dragon.TimesStaggered;
            balanceBefore = dragon.Stagger.Value;
            float lead = leads[tries];
            yield return Until(() => dragon.DoingFor >= blowAt - lead, 8f);
            Keys(Key.L);
            yield return Until(() => defence.PerfectGuards > perfect || dragon.DoingFor >= blowAt + 0.3f || dragon.Doing != name, 2f);
            balanceAfter = dragon.Stagger.Value;
            broke = dragon.TimesStaggered > staggers;
            Keys();
            yield return Over(attack, linger);
        }
        Say($"{name}: standing still it landed {still}; having left the mark {left}; rolled through +{rolls} (perfect {defence.PerfectDodges - perfectRolls}) in {rollTries} tries, landed anyway {defence.BlowsTaken - taken}; "
            + $"perfect guards +{defence.PerfectGuards - perfect} in {tries} tries, his balance {balanceBefore:0} -> {(broke ? "broken" : balanceAfter.ToString("0"))} (wanted +{balanceWanted:0})");
        if (dragon.IsStaggered) yield return Until(() => !dragon.IsStaggered, 8f);
        yield return Until(() => !defence.IsHelpless, 3f);
    }

    // An opening: the attack is made, the piece lies open, the player walks up from where the blow left them and strikes it.
    private IEnumerator Opening(DragonAttack attack, Vector3 where, ColossusPart part, string what, Vector3 standOff)
    {
        dragon.Health.Fill();
        dragon.Stagger.Reset();
        yield return Begin(attack, where);
        yield return Until(() => part.Vulnerable || dragon.Doing != attack.ToString(), 10f);
        hold = null;
        float openAt = Time.time;
        bool opened = part.Vulnerable;
        yield return Until(() => !defence.IsHelpless, 2f);
        // A piece that bends has many shapes along it: the nearest of them that lies on the floor is the one walked up to.
        Vector3 near = part.transform.position;
        float nearest = float.MaxValue;
        foreach (var reach in part.GetComponentsInChildren<Collider>())
        {
            if (!reach.enabled) continue;
            Vector3 point = reach.ClosestPoint(player.transform.position);
            if (new Vector2(point.x, point.z).magnitude > 14f) continue;
            float away = Vector3.Distance(point, player.transform.position);
            if (away >= nearest) continue;
            nearest = away;
            near = point;
        }
        float gap = Vector3.Distance(new Vector3(near.x, 0f, near.z), new Vector3(player.transform.position.x, 0f, player.transform.position.z));
        // Stood at its side, facing it, as a player who walked up would be.
        Vector3 stand = new Vector3(near.x, 0.1f, near.z) + standOff;
        player.Teleport(stand, Quaternion.LookRotation(new Vector3(-standOff.x, 0f, -standOff.z)));
        yield return null;
        yield return Shot(attack.ToString().ToLower() + "_opening");
        float health = dragon.Health.Current;
        int landed = combat.BlowsLanded;
        int finishers = combat.Finishers;
        yield return TapWhenFree(Key.J, () => combat.BlowsLanded > landed);
        float after = dragon.Health.Current;
        bool whileOpen = part.Vulnerable;
        yield return Until(() => !part.Vulnerable, 6f);
        Say($"opening after {attack}: the {what} lay open {opened}, {gap:0.0} m from where the blow left the player; a blow landed +{combat.BlowsLanded - landed} while open {whileOpen}, health {health:0} -> {after:0}; "
            + $"finishers +{combat.Finishers - finishers}; open for {Time.time - openAt:0.00} s");
        yield return Until(() => !combat.IsAttacking, 2f);
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        var summit = FindAnyObjectByType<SummitDirector>();
        var boss = FindAnyObjectByType<Enemy>();
        dragon = FindAnyObjectByType<StormDragon>();
        const string onlyFile = "AgentScripts/dragon_only.txt";
        string only = System.IO.File.Exists(onlyFile) ? System.IO.File.ReadAllText(onlyFile).Trim() : "";
        string wanted = "," + only.Replace(" ", "") + ",";
        bool Wants(string part) => only == "" || wanted.Contains("," + part + ",");
        bool pictures = Wants("Look");
        if (dragon == null)
        {
            Say("no storm dragon in this scene");
            Say("done");
            yield break;
        }

        // 1: the second form directly: the swordsman is felled at once, and the dragon comes.
        player.Teleport(Middle, Quaternion.identity);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        yield return Until(() => dragon.HasRisen, 8f);
        // He does nothing of his own until the probe asks: the first rest is set while he is still coming.
        float usualRest = dragon.RestBetween;
        dragon.RestBetween = 60f;
        float risenAt = Time.time;
        int takenBefore = defence.BlowsTaken;
        float[] moments = { 0.7f, dragon.EntranceLeftClawAt + 0.1f, dragon.EntranceRightClawAt + 0.6f, dragon.EntranceRoarAt - 0.05f, dragon.EntranceRoarAt + 0.2f, dragon.EntranceRoarAt + 0.55f };
        string[] names = { "coils", "left_grips", "neck", "reared", "roar", "roar_ring" };
        var outcome = HitOutcome.Landed;
        for (int i = 0; i < moments.Length; i++)
        {
            float at = moments[i];
            yield return Until(() => dragon.DoingFor >= at || dragon.Doing != "Rising", 6f);
            if (pictures) yield return Shot("entrance_" + names[i]);
            if (i == 2) outcome = dragon.RightClawPart.TakeHit(new Hit(60f, 50f, Vector3.forward));
        }
        yield return Until(() => dragon.Doing != "Rising", 6f);
        Say($"1 entrance: took {Time.time - risenAt:0.00} s; in fight after it {dragon.InFight}; a blow on a forefoot during it {outcome}; blows on the player during it {defence.BlowsTaken - takenBefore}; callout '{defence.LastCallout}'; arches risen {dragon.CoilsRisen}");

        // 2: the body in the cloud, seen from three places while he does nothing.
        if (pictures)
        {
            yield return Until(() => dragon.Doing == "Idle", 4f);
            Vector3[] places = { new Vector3(0f, 0.1f, -10f), Middle, new Vector3(9f, 0.1f, 6f), new Vector3(-9f, 0.1f, 8f) };
            string[] placeNames = { "south", "middle", "east", "west" };
            int least = 99, most = 0;
            for (int i = 0; i < places.Length; i++)
            {
                player.Teleport(places[i], Quaternion.identity);
                hold = places[i];
                for (float waited = 0f; waited < 2.2f; waited += Time.deltaTime)
                {
                    least = Mathf.Min(least, dragon.CoilsRisen);
                    most = Mathf.Max(most, dragon.CoilsRisen);
                    yield return null;
                }
                yield return Shot("body_" + placeNames[i]);
            }
            Say($"2 the body in the cloud: arches standing at once, least {least} most {most}; jaw at rest {dragon.JawOpen:0} degrees");
        }

        // 3: each attack. He still does nothing of his own between them.
        float beamBlow = dragon.BeamTelegraph + dragon.BeamSeconds * 0.5f - 0.1f;
        float sweepBlow = dragon.TailSweepTelegraph + dragon.TailSweepSeconds * 0.4f;
        var east = new Vector3(6f, 0.1f, 0f);
        var north = new Vector3(0f, 0.1f, 9f);
        if (Wants("Beam")) yield return Try(DragonAttack.Beam, Middle, new Vector3(-14f, 0.1f, 9f), dragon.BeamTelegraph * 0.8f, beamBlow, dragon.BeamTelegraph + dragon.BeamSeconds + 0.7f, 0f, dragon.PerfectGuardBalance * 0.5f);
        if (Wants("Breath")) yield return Try(DragonAttack.Breath, Middle, new Vector3(13f, 0.1f, 4f), dragon.BreathTelegraph * 0.85f, dragon.BreathTelegraph, 0f, 0f, dragon.PerfectGuardBalance * 0.5f);
        if (Wants("Bite")) yield return Try(DragonAttack.Bite, north, new Vector3(6f, 0.1f, 6f), dragon.BiteTelegraph * 0.85f, dragon.BiteTelegraph + dragon.BiteDrop, dragon.BiteTelegraph + dragon.BiteDrop + 0.6f, 0f, dragon.PerfectGuardBalance);
        if (Wants("Bolts")) yield return Try(DragonAttack.Bolts, Middle, new Vector3(3.2f, 0.1f, 2f), 1.1f, 0.4f + dragon.BoltWarning, 0.4f + dragon.BoltWaveEvery * 2f + 0.6f, 0.3f, dragon.PerfectGuardBalance);
        if (Wants("Sigils")) yield return Try(DragonAttack.Sigils, Middle, new Vector3(0f, 0.1f, -9.5f), 1.05f, 0.3f + dragon.SigilStepEvery * 2f + dragon.SigilWarning * 0.8f, 0.3f + dragon.SigilStepEvery + 0.6f, 0.3f, dragon.PerfectGuardBalance);
        if (Wants("Orbs")) yield return Try(DragonAttack.Orbs, Middle, new Vector3(-10f, 0.1f, -8f), 1.3f, dragon.OrbFirstBurst, dragon.OrbFirstBurst + 0.5f, 0.3f, dragon.PerfectGuardBalance);
        if (Wants("TailSweep")) yield return Try(DragonAttack.TailSweep, east, new Vector3(-8f, 0.1f, 0f), dragon.TailSweepTelegraph * 0.85f, sweepBlow, dragon.TailSweepTelegraph + dragon.TailSweepSeconds + 0.1f, 0f, dragon.PerfectGuardBalance);
        if (Wants("TailSlam")) yield return Try(DragonAttack.TailSlam, east, new Vector3(6f, 0.1f, 6f), dragon.TailSlamTelegraph * 0.85f, dragon.TailSlamTelegraph + dragon.TailSlamDrop * 0.72f, dragon.TailSlamTelegraph + dragon.TailSlamDrop + 0.7f, 0f, dragon.PerfectGuardBalance);

        // The tail, five pictures across each of its attacks: it has to read as a tail, never as a stick being turned.
        if (Wants("TailStrips"))
        {
            float st = dragon.TailSweepTelegraph, ss = dragon.TailSweepSeconds;
            float[] slither = { st * 0.45f, st * 0.95f, st + ss * 0.45f, st + ss + 0.3f, st + ss + dragon.TailSweepRest + 0.45f };
            yield return Begin(DragonAttack.TailSweep, east);
            for (int i = 0; i < slither.Length; i++)
            {
                float at = slither[i];
                yield return Until(() => dragon.DoingFor >= at || dragon.Doing != "TailSweep", 6f);
                yield return Shot("slither_" + (i + 1));
            }
            yield return Over(DragonAttack.TailSweep);
            float ft = dragon.TailSlamTelegraph, fd = dragon.TailSlamDrop;
            float[] flop = { 0.35f, ft * 0.9f, ft + fd * 0.55f, ft + fd + 0.14f, ft + fd + 1.2f };
            yield return Begin(DragonAttack.TailSlam, east);
            for (int i = 0; i < flop.Length; i++)
            {
                float at = flop[i];
                yield return Until(() => dragon.DoingFor >= at || dragon.Doing != "TailSlam", 6f);
                yield return Shot("flop_" + (i + 1));
            }
            yield return Over(DragonAttack.TailSlam);
        }

        // The storm is not one blow but ground that burns: stood in, and left.
        if (Wants("Storm"))
        {
            var inIt = new Vector3(6f, 0.1f, 6f);
            yield return Begin(DragonAttack.Storm, inIt);
            yield return Until(() => dragon.DoingFor >= dragon.StormCast * 0.8f || dragon.Doing != "Storm", 6f);
            yield return Shot("storm_1");
            yield return Until(() => dragon.StormOn, 6f);
            dragon.Queued = DragonAttack.Breath;
            int taken = defence.BlowsTaken;
            yield return new WaitForSeconds(1.6f);
            yield return Shot("storm_2");
            int burned = defence.BlowsTaken - taken;
            var outOfIt = new Vector3(-6f, 0.1f, 6f);
            hold = outOfIt;
            player.Teleport(outOfIt, Quaternion.identity);
            bool marked = dragon.InStorm(inIt), clear = !dragon.InStorm(outOfIt);
            float left = Time.time;
            yield return Until(() => !dragon.StormOn, 12f);
            yield return Shot("storm_3");
            Say($"Storm: stood in a marked quarter it burned {burned} times in 1.6 s; that quarter marked {marked}, the next one clear {clear}; it lasted {Time.time - left + 1.6f:0.0} s more; he went on to {dragon.Doing} while it burned");
            hold = null;
        }

        // 4: the openings.
        if (Wants("Openings"))
        {
            yield return Opening(DragonAttack.Bite, north, dragon.HeadPart, "head", new Vector3(1.6f, 0f, -1.2f));
            yield return Opening(DragonAttack.Beam, new Vector3(0f, 0.1f, 6f), dragon.HeadPart, "head", new Vector3(1.6f, 0f, -1.2f));
            yield return Opening(DragonAttack.TailSlam, east, dragon.TailPart, "tail", new Vector3(0f, 0f, -1.1f));
            // A forefoot, at any time.
            yield return Until(() => dragon.Doing == "Idle", 10f);
            float health = dragon.Health.Current;
            var foot = dragon.RightClawPart;
            Vector3 at = foot.transform.position;
            player.Teleport(new Vector3(at.x * 0.82f, 0.1f, at.z * 0.82f), Quaternion.LookRotation(new Vector3(at.x, 0f, at.z)));
            int landed = combat.BlowsLanded;
            yield return null;
            yield return Shot("forefoot");
            yield return TapWhenFree(Key.J, () => combat.BlowsLanded > landed);
            Say($"a forefoot: open {foot.Vulnerable}; a blow landed +{combat.BlowsLanded - landed}, health {health:0} -> {dragon.Health.Current:0} (it lets {dragon.ClawDamage:0.00} through)");
            yield return Until(() => !combat.IsAttacking, 2f);
        }

        // 5: the balance broken, and the finisher.
        if (Wants("Stagger"))
        {
            dragon.Health.Fill();
            yield return Until(() => dragon.Doing == "Idle", 10f);
            dragon.Unbalance(10000f);
            yield return new WaitForSeconds(0.8f);
            bool broken = dragon.IsStaggered;
            float health = dragon.Health.Current;
            var skull = dragon.HeadPart.GetComponent<Collider>();
            Vector3 beside = skull.bounds.center + new Vector3(0f, 0f, -3.6f);
            player.Teleport(new Vector3(beside.x, 0.1f, beside.z), Quaternion.identity);
            yield return null;
            yield return Shot("staggered");
            int finishers = combat.Finishers;
            yield return TapWhenFree(Key.K, () => combat.Finishers > finishers);
            yield return Shot("finisher");
            Say($"5 broken: staggered {broken} head open {dragon.HeadPart.Vulnerable || combat.Finishers > finishers} finishers +{combat.Finishers - finishers} health {health:0} -> {dragon.Health.Current:0} (22% of {dragon.MaximumHealth:0} is {dragon.MaximumHealth * dragon.FinisherShare:0}); now {dragon.Doing}");
            yield return Until(() => !combat.IsAttacking && dragon.Doing == "Idle", 6f);
        }

        // 6: fury, which opens with the storm and brings the beam back the way it went.
        if (Wants("Fury"))
        {
            bool enraged = false;
            dragon.Enraged += _ => enraged = true;
            dragon.RestBetween = usualRest;
            yield return Until(() => dragon.Doing == "Idle", 10f);
            dragon.Health.Fill();
            dragon.Health.Damage(dragon.MaximumHealth * 0.55f);
            player.Teleport(Middle, Quaternion.identity);
            hold = Middle;
            yield return null;
            yield return null;
            yield return Shot("fury_begins");
            string callout = defence.LastCallout;
            yield return Until(() => dragon.Doing != "Idle", 6f);
            string first = dragon.Doing;
            yield return Until(() => dragon.Doing == "Idle", 8f);
            // Out of the storm his fury opened with, so that what lands is the beam alone.
            var clear = dragon.InStorm(new Vector3(3f, 0f, 2f)) ? new Vector3(-3f, 0.1f, 2f) : new Vector3(3f, 0.1f, 2f);
            hold = clear;
            player.Teleport(clear, Quaternion.identity);
            dragon.Queued = DragonAttack.Beam;
            yield return Until(() => dragon.Doing == "Beam", 8f);
            int taken = defence.BlowsTaken;
            yield return Until(() => dragon.DoingFor >= dragon.BeamTelegraph + dragon.BeamSeconds * 1.5f || dragon.Doing != "Beam", 10f);
            yield return Shot("fury_beam_back");
            yield return Until(() => dragon.Doing != "Beam", 10f);
            Say($"6 fury: began {enraged} callout '{callout}' furious {dragon.Furious}; his first act in it {first}; a beam in fury crossed the still player {defence.BlowsTaken - taken} times");
            hold = null;
        }

        // 7: the death, and what he leaves.
        hold = Middle;
        player.Teleport(Middle, Quaternion.identity);
        for (int attempt = 0; attempt < 12 && !dragon.IsDead; attempt++)
        {
            yield return Until(() => dragon.Doing == "Idle", 10f);
            dragon.Unbalance(10000f);
            yield return new WaitForSeconds(0.7f);
            for (int blow = 0; blow < 40 && !dragon.IsDead; blow++) dragon.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
        }
        float diedAt = Time.time;
        hold = new Vector3(0f, 0.1f, 6f);
        player.Teleport(hold.Value, Quaternion.identity);
        float[] deathMoments = { 0.6f, dragon.DeathBoltsAt + 0.5f, dragon.DeathRightLetsGoAt + 0.3f, dragon.DeathHeadLandsAt + 0.1f, dragon.DeathHeadSlidesAt + 0.7f };
        string[] deathNames = { "rears", "bolts", "arches_sink", "head_lands", "slides" };
        for (int i = 0; i < deathMoments.Length; i++)
        {
            float at = deathMoments[i];
            yield return Until(() => dragon.DoingFor >= at || dragon.Doing != "Dying", 8f);
            if (pictures) yield return Shot("death_" + deathNames[i]);
        }
        yield return Until(() => summit.Felled, 10f);
        Say($"7 death: from the last blow to the empty rim {Time.time - diedAt:0.00} game s; dead {dragon.IsDead} felled {summit.Felled} bar shown {dragon.InFight} gods felled {RunState.GodsFelled} callout '{defence.LastCallout}'");
        yield return new WaitForSecondsRealtime(summit.RewardDelay + 2f);
        hold = null;
        keepAlive = false;
        GodReward take = null, consume = null;
        foreach (var reward in FindObjectsByType<GodReward>(FindObjectsSortMode.None))
        {
            if (reward.Takes) take = reward;
            else consume = reward;
        }
        Say($"8 what he leaves: take shown {take != null} consume shown {consume != null} time scale {Time.timeScale:0.00}");
        if (consume != null)
        {
            var element = player.GetComponent<PlayerElement>();
            player.Teleport(consume.transform.position + new Vector3(0f, 0.1f, -1.4f), Quaternion.identity);
            yield return new WaitForSeconds(0.4f);
            yield return Shot("rewards");
            yield return TapWhenFree(Key.E, () => consume.Chosen);
            var door = FindAnyObjectByType<SceneDoor>();
            if (!consume.Chosen) Say($"8 why not: player at {player.transform.position} dead {defence.IsDead} hurt {defence.IsHurt} attacking {combat.IsAttacking} rolling {player.Dodge.IsRolling} health {defence.Health.Current:0} leaping {player.IsLeaping} form hold {player.FormHold}; consume at {consume.transform.position} active {consume.gameObject.activeInHierarchy}");
            Say($"8 consumed: chosen {consume.Chosen} director has chosen {summit.HasChosen} element {(element != null ? element.Kit.Name : "?")} weapon {combat.Weapon.Name}; the other still there {take != null && take.gameObject.activeSelf}; way down open {door != null && door.Open}");
        }
        Say("done");
    }
}
