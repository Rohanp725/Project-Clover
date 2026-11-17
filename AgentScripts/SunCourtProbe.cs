using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the sun-court: the forecourt and its fire, the gold line and the king's words, every move of
// Khepharu the man (standing still, then answered), the shafts of noon against someone in the obelisk's shadow, his
// fall and the Sun-Bearer's entrance, every move of the form (standing still, then answered), the sun knocked away
// and pushed, the blaze inside and outside the shadow, balance broken and the finisher, fury and the shadow's
// turn-back, a fall mid-fight (back at the fire, the god whole again), the form's death, the coal taken (Fire held, a
// spell cast), the sponsor's last words and the way out. Frames: review/shots/current/suncourt_<what>_<n>.png. Run with
//   bash AgentScripts/runprobe.sh SunCourt SunCourtProbe 200
// AgentScripts/suncourt_only.txt may hold "Hold" to go straight to the blaze and hold it (for aspect_court.sh) or
// "Man" / "Form" to play only that half.
public static class SunCourtProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Sun Court Probe").AddComponent<SunCourtProbeRunner>();
        return "probe started";
    }
}

public class SunCourtProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerInteraction interaction;
    private AreaDirector director;
    private SunCourtDirector court;
    private SunBearer form;
    private Enemy boss;
    private bool keepAlive = true;
    private Func<Vector3> follow;

    private ProfilerRecorder garbage, mainThread;
    private bool measuring;
    private int frames;
    private long garbageTotal;
    private double mainTotal;
    private float frameTotal, worst;

    private void Update()
    {
        if (measuring)
        {
            frames++;
            garbageTotal += garbage.LastValue;
            mainTotal += mainThread.LastValue * 1e-6;
            float ms = Time.unscaledDeltaTime * 1000f;
            frameTotal += ms;
            if (ms > worst) worst = ms;
        }
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (follow != null && player != null) player.Teleport(follow(), player.transform.rotation);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.time + timeout;
        while (!condition() && Time.time < end) yield return null;
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

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/suncourt_" + name + ".png");
        yield return null;
        yield return null;
    }

    private IEnumerator Hear(string when, float wait = 0.5f)
    {
        yield return new WaitForSecondsRealtime(wait);
        var box = ReadingBox.Instance;
        if (box == null || !box.IsOpen)
        {
            Say($"   {when}: nothing was said");
            yield break;
        }
        int before = box.LinesRead;
        for (int i = 0; i < 24 && box.IsOpen; i++)
        {
            yield return Tap(Key.E);
            yield return new WaitForSecondsRealtime(0.05f);
        }
        yield return new WaitForSecondsRealtime(0.1f);
        Say($"   {when}: {box.LinesRead - before} line(s) read, box closed {!box.IsOpen}, player free {!player.Suspended}");
    }

    private void Go(Vector3 to, Vector3 facing)
    {
        facing.y = 0f;
        player.Teleport(new Vector3(to.x, 0.1f, to.z), facing.sqrMagnitude > 0.01f ? Quaternion.LookRotation(facing) : Quaternion.identity);
    }

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        interaction = player.GetComponent<PlayerInteraction>();
        director = FindAnyObjectByType<AreaDirector>();
        court = FindAnyObjectByType<SunCourtDirector>();
        form = FindAnyObjectByType<SunBearer>();
        boss = director.Enemies[0];
        string only = System.IO.File.Exists("AgentScripts/suncourt_only.txt") ? System.IO.File.ReadAllText("AgentScripts/suncourt_only.txt").Trim() : "";
        yield return new WaitForSeconds(1.2f);
        yield return Hear("on arriving", 0.2f);

        combat.Equip(Weapons.ByName(Weapons.GreatSpearName));
        Say($"1 start: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} player at {player.transform.position:0.0} weapon {combat.Weapon.Name} has element {element.HasElement} "
            + $"sandboxed {Progress.Sandboxed} boss {boss.name} health {boss.Health.Current:0} form risen {form.HasRisen} shadow in use {Sunlight.Hand != null} begun {court.Begun}");
        yield return Shot("forecourt_1");
        var rest = FindAnyObjectByType<RestPoint>();
        Go(rest.transform.position + new Vector3(0f, 0f, -1.6f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        rest.Use(interaction);
        yield return new WaitForSeconds(0.4f);
        Say($"1 rest: lit {rest.IsLit} says '{defence.LastCallout}' return point {director.ReturnPoint.position:0.0}");
        yield return Shot("forecourt_2");

        if (only == "Hold")
        {
            yield return Hold();
            yield break;
        }

        // Across the line.
        Go(new Vector3(0f, 0f, -18.6f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        yield return Shot("line_1");
        yield return Hear("the king at the line", 0.3f);
        var wall = GameObject.Find("Wall Of Heat").GetComponent<Collider>();
        Say($"2 the line: begun {court.Begun} wall closed {wall.enabled} king aware {boss.IsAware} shadow angle {court.Hand.Angle:0.0} rate {court.Hand.DegreesPerSecond:0.0}");
        Go(new Vector3(0f, 0f, -8f), Vector3.forward);
        yield return new WaitForSeconds(1.2f);
        yield return Shot("court_1");
        Go(new Vector3(-9f, 0f, 2f), Vector3.right);
        yield return new WaitForSeconds(1f);
        yield return Shot("court_2");

        if (only == "Show")
        {
            // The entrance and the death alone, seen from the north-east and from across the obelisk.
            form.OnlyWhenAsked = true;
            yield return FellMan(new Vector3(12f, 0f, 10f), 8);
            yield return PerfectGuard(new Vector3(0f, 0f, -9f));
            yield return Idle();
            yield return Death(new Vector3(-10f, 0f, -10f), 8);
            Say("done");
            Debug.Log("[Probe] done");
            Destroy(gameObject);
            yield break;
        }
        if (only != "Form") yield return Man();
        yield return FellMan(new Vector3(0f, 0f, -6f), 1);
        if (only != "Man") yield return Form();
        Say("done");
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }

    // ------------------------------------------------------------------ the man

    // Puts the player in front of him at a distance, forces a move, pictures its tell and its blow, and says what it did
    // to someone standing still; then again with the answer.
    private IEnumerator ManMove(string name, float distance, bool red, int chain = 1)
    {
        for (int pass = 0; pass < 2; pass++)
        {
            boss.Restore();
            boss.transform.SetPositionAndRotation(new Vector3(6f, 0.05f, 6f), Quaternion.LookRotation(Vector3.back));
            Vector3 stand = boss.transform.position + Vector3.back * distance;
            Go(stand, Vector3.forward);
            boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
            yield return new WaitForSeconds(0.15f);
            // He keeps his own distance: the player is put where the move asks, at the moment it is forced.
            stand = boss.transform.position + boss.transform.forward * distance;
            Go(stand, boss.transform.position - stand);
            int taken = defence.BlowsTaken, blocks = defence.Blocks, perfect = defence.PerfectGuards;
            bool forced = boss.Brain.Force(name);
            var attack = boss.Brain.Current;
            float total = 0f;
            var step = attack;
            for (int i = 0; i < chain && step != null; i++)
            {
                total += step.Total;
                step = string.IsNullOrEmpty(step.ChainInto) ? null : boss.Brain.Attacks.Find(a => a.Name == step.ChainInto);
            }
            if (pass == 1 && !red) Keys(Key.L);
            yield return Until(() => boss.Brain.TelegraphProgress >= 0.7f, 3f);
            if (pass == 0) yield return Shot(name + "_1");
            if (pass == 1 && red)
            {
                // Out of the mark: off the lane to the side, or out of the circle.
                Vector3 side = Vector3.right * 4.5f + Vector3.back * (attack != null && attack.IsCircle ? 5f : 0f);
                Go(player.transform.position + side, Vector3.forward);
                if (name.StartsWith("sunking_noon")) follow = () => new Vector3(-14f + Mathf.Repeat(Time.time * 6f, 20f), 0.1f, -9f);
            }
            float left = attack != null ? attack.Telegraph * 0.3f + attack.Strike * 0.5f : 0.3f;
            yield return new WaitForSeconds(Mathf.Max(0.02f, left));
            if (pass == 0) yield return Shot(name + "_2");
            yield return new WaitForSeconds(Mathf.Max(0.3f, total - (attack != null ? attack.Telegraph : 0f)) + 0.35f);
            if (pass == 0 && chain > 1) yield return Shot(name + "_3");
            follow = null;
            Keys();
            yield return null;
            Say($"   {name} {(pass == 0 ? "standing still" : red ? "having left the mark" : "guard held")}: forced {forced} blows taken +{defence.BlowsTaken - taken} blocked +{defence.Blocks - blocks} "
                + $"perfect +{defence.PerfectGuards - perfect} at {Flat(boss.transform.position - player.transform.position).magnitude:0.0} m spear stood {court.SpearStands} {((pass == 0) == (defence.BlowsTaken > taken) ? "OK" : "WRONG")}");
        }
    }

    private IEnumerator Man()
    {
        Say("3 the man: each move standing still and then answered");
        yield return ManMove("sunking_thrust_1", 4f, false, 3);
        yield return ManMove("sunking_reach", 5.5f, false);
        yield return ManMove("sunking_sweep", 2.4f, false);
        yield return ManMove("sunking_butt", 1.7f, false);
        yield return ManMove("sunking_throw", 9f, true, 2);
        yield return ManMove("sunking_plant", 3f, true);
        yield return ManMove("sunking_noon_1", 10f, true, 3);

        // The thrown spear: seen standing in the floor while he is bare-handed.
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(-8f, 0.05f, 6f), Quaternion.LookRotation(Vector3.right));
        Go(new Vector3(1f, 0f, 9f), Vector3.left);
        boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        yield return new WaitForSeconds(0.4f);
        boss.Brain.Force("sunking_throw");
        yield return Until(() => court.SpearStands, 3f);
        yield return new WaitForSeconds(0.5f);
        bool stood = court.SpearStands;
        yield return Shot("spear_stands_1");
        yield return Until(() => boss.Brain.Current != null && boss.Brain.Current.Name == "sunking_ray" && boss.Brain.TelegraphProgress > 0.8f, 3f);
        yield return Shot("spear_stands_2");
        yield return Until(() => !court.SpearStands, 3f);
        Say($"   the thrown spear: stood in the floor {stood}, back in his hand {!court.SpearStands}");

        // A shaft of noon cannot be aimed at someone in the shadow.
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(12f, 0.05f, -6f), Quaternion.LookRotation(Vector3.left));
        boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        follow = () => court.Hand.Centre + court.Hand.Direction * 9f + Vector3.up * 0.1f;
        yield return new WaitForSeconds(0.4f);
        int before = defence.BlowsTaken;
        bool lit = Sunlight.IsLit(player.transform.position);
        boss.Brain.Force("sunking_noon_1");
        yield return Until(() => boss.Brain.TelegraphProgress >= 0.8f, 3f);
        yield return Shot("noon_shadow_1");
        yield return new WaitForSeconds(2.6f);
        follow = null;
        Say($"   noon against someone standing in the shadow: lit there {lit}, blows taken +{defence.BlowsTaken - before} {(defence.BlowsTaken == before ? "OK" : "WRONG")}");

        // Half health: the planted spear opens his second half.
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(0f, 0.05f, 8f), Quaternion.LookRotation(Vector3.back));
        Go(new Vector3(0f, 0f, 0.5f), Vector3.forward);
        boss.TakeHit(new Hit(boss.Health.Maximum * 0.55f, 0f, Vector3.forward, true, true));
        yield return new WaitForSeconds(0.6f);
        Say($"   below half: phase {boss.Brain.Phase} doing {(boss.Brain.Current != null ? boss.Brain.Current.Name : "nothing")} says '{defence.LastCallout}'");
        yield return new WaitForSeconds(2.5f);
    }

    // A perfect guard against the rolling sun: the guard goes up as it arrives, facing it, and the sun is knocked away.
    private IEnumerator PerfectGuard(Vector3 stand)
    {
        yield return Idle();
        // Round the court from it, not across the obelisk: a lane ends at the stone.
        stand = RoundFrom(75f, 12.5f);
        form.Queued = SunBearerAttack.Roll;
        Go(stand, form.BodyAt - stand);
        yield return Until(() => form.Doing == "Roll", 12f);
        yield return Until(() => form.Locked || form.Doing != "Roll", 6f);
        string began = form.Doing + (form.Locked ? " locked" : " not locked");
        Go(stand, form.SunAt - stand);
        int knocksBefore = form.SunKnocks, perfectBefore = defence.PerfectGuards, takenBefore = defence.BlowsTaken, staggersBefore = form.TimesStaggered;
        float guardUntil = Time.time + 8f, pressedAt = -1f, nearest = 999f;
        while (Time.time < guardUntil && form.Doing == "Roll" && form.SunKnocks == knocksBefore && defence.BlowsTaken == takenBefore)
        {
            float off = Flat(form.SunAt - player.transform.position).magnitude;
            if (off < nearest) nearest = off;
            if (pressedAt < 0f && off < 3.1f)
            {
                Keys(Key.L);
                pressedAt = off;
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.4f);
        Keys();
        bool turned = defence.PerfectGuards > perfectBefore && form.SunKnocks > knocksBefore && form.SunLoose && defence.BlowsTaken == takenBefore;
        Say($"6 a perfect guard on the rolling sun ({began}; nearest {nearest:0.0} m; guard raised with it {pressedAt:0.0} m off): perfect +{defence.PerfectGuards - perfectBefore} knocks +{form.SunKnocks - knocksBefore} loose {form.SunLoose} "
            + $"exposed {form.Exposed} broken +{form.TimesStaggered - staggersBefore} blows taken +{defence.BlowsTaken - takenBefore} doing {form.Doing} says '{defence.LastCallout}' {(turned ? "OK" : "WRONG")}");
        yield return Shot("perfect_guard_1");
        yield return Until(() => !form.Exposed && !form.IsStaggered, 20f);
    }

    private static readonly float[] EntranceShots = { 0.6f, 1.5f, 2.1f, 2.6f, 3.0f, 3.6f, 4.3f };
    private static readonly float[] DeathShots = { 0.7f, 1.5f, 1.9f, 2.8f, 3.6f, 4.3f, 5.4f };

    // Kills the form where it stands with the player at this place, and pictures its death.
    private IEnumerator Death(Vector3 stand, int first)
    {
        Go(stand, form.BodyAt - stand);
        for (int attempt = 0; attempt < 12 && !form.IsDead; attempt++)
        {
            form.Unbalance(999f);
            yield return new WaitForSeconds(0.6f);
            for (int blow = 0; blow < 40 && !form.IsDead; blow++) form.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
            yield return new WaitForSeconds(0.3f);
        }
        for (int i = 0; i < DeathShots.Length; i++)
        {
            float at = DeathShots[i];
            yield return Until(() => form.DoingFor >= at || court.Felled, 8f);
            yield return Shot("death_" + (first + i));
        }
        Say($"   death seen from {stand:0.0}: the form fell at {form.BodyAt:0.0}, free to move {!player.Suspended}");
    }

    private IEnumerator FellMan(Vector3 stand, int first)
    {
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(0f, 0.05f, 8f), Quaternion.LookRotation(Vector3.back));
        Go(stand, Vector3.forward);
        boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        yield return new WaitForSeconds(0.8f);
        yield return Shot("man_1");
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        float fellAt = Time.time;
        Say($"4 the man falls: dead {boss.IsDead} second form called {court.SecondFormCalled}");
        yield return Until(() => form.HasRisen, 6f);
        Say($"4 the sun comes down after {Time.time - fellAt:0.0} s: says '{defence.LastCallout}'");
        bool held = player.Suspended, barEarly = form.InFight;
        for (int i = 0; i < EntranceShots.Length; i++)
        {
            float at = EntranceShots[i];
            yield return Until(() => form.Doing != "Entering" || form.DoingFor >= at, 8f);
            if (i == 5) Say($"4 after the roar: bar shown {form.InFight} says '{defence.LastCallout}'");
            yield return Shot("entrance_" + (first + i));
        }
        yield return Until(() => form.Doing != "Entering", 6f);
        yield return null;
        Say($"4 the entrance from {stand:0.0}: player held {held}, bar before the roar {barEarly}, free after {!player.Suspended} {(held && !barEarly && !player.Suspended ? "OK" : "WRONG")}");
        Say($"4 the form: in fight after {Time.time - fellAt:0.0} s, says '{defence.LastCallout}', health {form.Health.Current:0} shadow rate {court.Hand.DegreesPerSecond:0.0}");
    }

    // ------------------------------------------------------------------ the form

    private IEnumerator Idle(float timeout = 14f)
    {
        yield return Until(() => form.Doing == "Idle", timeout);
    }

    // Queues a move, waits for it to begin, pictures it, and says what it did. avoid: null to stand still; otherwise
    // called every frame once the mark is up for where the player should be.
    private IEnumerator FormMove(SunBearerAttack attack, Vector3 stand, bool answered, float tellShot, float blowShot, float lasts)
    {
        string name = attack.ToString();
        yield return Idle();
        // No feather is marked in the obelisk's shadow: someone who happens to stand in it is not struck, which is the
        // rule and not a miss. To see the feathers land, the still player stands well clear of where the shadow lies.
        if (attack == SunBearerAttack.Feathers)
        {
            court.Hand.Measure(stand, out float along, out float across);
            if (along > 0f && across < 6f) stand = new Vector3(-stand.x, stand.y, -stand.z);
        }
        Go(stand, form.BodyAt - stand);
        form.Queued = attack;
        yield return Until(() => form.Doing == name, 12f);
        if (form.Doing != name)
        {
            Say($"   {name}: never began (doing {form.Doing})");
            yield break;
        }
        Go(stand, form.BodyAt - stand);
        int taken = defence.BlowsTaken, blocks = defence.Blocks;
        float began = Time.time;
        bool pale = attack == SunBearerAttack.Ram;
        if (answered && pale) Keys(Key.L);
        yield return Until(() => form.DoingFor >= tellShot || form.Doing != name, 6f);
        if (!answered) yield return Shot(name.ToLower() + "_1");
        if (answered && !pale)
        {
            // Not before the mark has stopped following: it would only follow the player to where they went.
            if (attack != SunBearerAttack.Zenith && attack != SunBearerAttack.Sparks && attack != SunBearerAttack.Feathers) yield return Until(() => form.Locked || form.Doing != name, 6f);
            if (attack == SunBearerAttack.Zenith) follow = () => court.Hand.Centre + court.Hand.Direction * 9f + Vector3.up * 0.1f;
            else if (attack == SunBearerAttack.Sparks || attack == SunBearerAttack.Feathers) follow = () => SafeFromFalls(stand);
            else
            {
                // Off the lane, or out of the circle.
                Vector3 across = new Vector3(form.LaneAlong.z, 0f, -form.LaneAlong.x);
                bool lane = SunBearerChooser.LaysLane(attack);
                Vector3 away = lane ? across * 6f : Quaternion.Euler(0f, 90f, 0f) * stand.normalized * 7f;
                if (lane && (stand + away).magnitude > 17f) away = -away;
                if (attack == SunBearerAttack.Furrows) follow = BesideTheLane;
                else Go(stand + away, form.BodyAt - stand);
            }
        }
        yield return Until(() => form.DoingFor >= blowShot || form.Doing != name, 8f);
        if (!answered) yield return Shot(name.ToLower() + "_2");
        yield return Until(() => form.Doing != name || Time.time - began > lasts, lasts + 2f);
        if (!answered) yield return Shot(name.ToLower() + "_3");
        // Off whatever floor was left hot, so the next count starts clean.
        int hot = form.HotBites;
        follow = null;
        Keys();
        yield return null;
        bool hurt = defence.BlowsTaken > taken;
        bool right = answered ? (!hurt || pale && defence.Blocks > blocks) : hurt;
        Say($"   {name} {(!answered ? "standing still" : pale ? "guard held" : attack == SunBearerAttack.Zenith ? "walking with the shadow" : "having left the mark")}: blows taken +{defence.BlowsTaken - taken} "
            + $"(last: {form.LastBlow}) blocked +{defence.Blocks - blocks} hot lanes {form.HotLanes} hot patches {form.HotPatches} hot bites so far {hot} blaze bites {form.BlazeBites} spared {form.BlazeSpared} falcon {form.IsFalcon} {(right ? "OK" : "WRONG")}");
        if (form.OnHotFloor(player.transform.position)) Go(SafeFromFalls(new Vector3(0f, 0f, -15f)), Vector3.forward);
    }

    // Seven metres to one side of the lane as it is drawn now, about its middle, on floor that is not hot.
    private Vector3 BesideTheLane()
    {
        Vector3 across = new Vector3(form.LaneAlong.z, 0f, -form.LaneAlong.x);
        Vector3 middle = form.LaneFrom + form.LaneAlong * Mathf.Min(form.LaneLength * 0.5f, 12f);
        Vector3 best = player.transform.position;
        float bestScore = -100f;
        foreach (float side in new[] { 7f, -7f, 11f, -11f, 4.5f, -4.5f })
        {
            Vector3 spot = middle + across * side;
            float score = -spot.magnitude - (form.OnHotFloor(spot) ? 40f : 0f) - (spot.magnitude > 17f ? 40f : 0f) - (spot.magnitude < 4.5f ? 40f : 0f) - (spot - player.transform.position).magnitude * 0.5f;
            if (score <= bestScore) continue;
            bestScore = score;
            best = spot;
        }
        return new Vector3(best.x, 0.1f, best.z);
    }

    // A point round the court from the scarab, so the lane to it does not run into the obelisk.
    private Vector3 RoundFrom(float degrees, float radius)
    {
        Vector3 flat = Flat(form.BodyAt);
        if (flat.sqrMagnitude < 1f) flat = Vector3.forward;
        return Quaternion.Euler(0f, degrees, 0f) * flat.normalized * radius;
    }

    // A place near where the player was that no falling thing is marked on and no floor is hot: found by trying a ring of spots.
    private Vector3 SafeFromFalls(Vector3 near)
    {
        var marks = GameObject.Find("Sun Bearer").transform.Find("Marks");
        for (int ring = 0; ring < 5; ring++)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector3 spot = near + Quaternion.Euler(0f, i * 30f, 0f) * Vector3.forward * (ring * 1.6f);
                if (spot.magnitude > 18f || spot.magnitude < 4.5f || form.OnHotFloor(spot)) continue;
                bool marked = false;
                foreach (Transform mark in marks)
                {
                    if (!mark.name.StartsWith("Fall Outline")) continue;
                    var shown = mark.GetComponent<Renderer>();
                    if (shown.enabled && Flat(mark.position - spot).magnitude < mark.localScale.x + 0.9f) marked = true;
                }
                if (!marked) return spot + Vector3.up * 0.1f;
            }
        }
        return near;
    }

    private IEnumerator Form()
    {
        Say("5 the form: each move standing still and then answered");
        form.OnlyWhenAsked = true;
        var south = new Vector3(0f, 0f, -9f);
        foreach (bool answered in new[] { false, true })
        {
            yield return Idle();
            yield return FormMove(SunBearerAttack.Roll, RoundFrom(75f, 12.5f), answered, 1.1f, 2.1f, 7f);
            yield return Until(() => form.HotLanes == 0, 12f);
            yield return FormMove(SunBearerAttack.Ram, Flat(form.BodyAt) + Flat(south - form.BodyAt).normalized * 6f, answered, 0.8f, 1.32f, 6f);
            yield return FormMove(SunBearerAttack.Sparks, Flat(form.BodyAt) + Flat(south - form.BodyAt).normalized * 7f, answered, 1.0f, 1.4f, 5f);
            yield return Idle();
            yield return FormMove(SunBearerAttack.Furrows, RoundFrom(75f, 12.5f), answered, 0.9f, 1.9f, 16f);
            yield return Until(() => form.HotLanes == 0, 16f);
            yield return FormMove(SunBearerAttack.Stoop, new Vector3(6f, 0f, -8f), answered, 1.2f, 1.76f, 6f);
            yield return FormMove(SunBearerAttack.Feathers, new Vector3(-7f, 0f, -8f), answered, 0.9f, 1.4f, 6f);
            yield return Until(() => form.HotPatches == 0, 12f);
            yield return FormMove(SunBearerAttack.Pass, new Vector3(8f, 0f, -7f), answered, 0.9f, 1.6f, 6f);
            yield return Until(() => form.HotLanes == 0, 12f);
            // Standing still in the open for the first; in the shadow, walking with it, for the second.
            if (!answered) yield return FormMove(SunBearerAttack.Zenith, new Vector3(-8f, 0f, -9f), false, 3f, 5.5f, 14f);
            else
            {
                // The busiest the fight gets: the blaze, with lanes still hot from a pass and a stoop's crater. Measured as PerfProbe measures.
                form.Queued = SunBearerAttack.Pass;
                Go(new Vector3(8f, 0f, -7f), Vector3.forward);
                yield return Until(() => form.Doing == "Pass", 12f);
                yield return Until(() => form.Doing != "Pass", 8f);
                Go(new Vector3(-8f, 0f, -9f), Vector3.forward);
                garbage = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
                mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
                frames = 0; garbageTotal = 0; mainTotal = 0; frameTotal = 0f; worst = 0f;
                measuring = true;
                yield return FormMove(SunBearerAttack.Zenith, new Vector3(-8f, 0f, -9f), true, 3f, 5.5f, 14f);
                measuring = false;
                Say($"   perf over the Zenith with hot floor ({frames} frames): frame ms mean {frameTotal / Mathf.Max(1, frames):0.0} worst {worst:0.0}; main thread mean {mainTotal / Mathf.Max(1, frames):0.0} ms; "
                    + $"garbage {garbageTotal / Mathf.Max(1, frames)} bytes a frame; screen {Screen.width}x{Screen.height}");
                garbage.Dispose();
                mainThread.Dispose();
            }
            yield return Until(() => form.Doing == "Spent", 4f);
            yield return new WaitForSeconds(1.8f);
            if (!answered) yield return Shot("spent_1");
            Say($"   after the blaze: doing {form.Doing} head open {form.HeadPart.Vulnerable} falcon {form.IsFalcon}");
        }

        // The sun knocked out of its hold by damage to the body during one attack, then pushed, then fetched.
        yield return Idle();
        Go(new Vector3(6f, 0f, -8f), Vector3.forward);
        form.Queued = SunBearerAttack.Stoop;
        yield return Until(() => form.Doing == "Stoop", 12f);
        yield return new WaitForSeconds(0.9f);
        Go(new Vector3(6f, 0f, -14f), Vector3.forward);
        yield return Until(() => form.BodyPart.Vulnerable || form.Doing != "Stoop", 4f);
        float health = form.Health.Current;
        int knocks = form.SunKnocks;
        Vector3 sunWas = form.SunAt;
        for (int i = 0; i < 4 && !form.SunLoose; i++) form.BodyPart.TakeHit(new Hit(10f, 0f, Vector3.right));
        yield return new WaitForSeconds(0.8f);
        Say($"6 the sun knocked away by blows on the falcon: knocks +{form.SunKnocks - knocks} loose {form.SunLoose} exposed {form.Exposed} health lost {health - form.Health.Current:0.0} (a quarter of the blows) "
            + $"sun went {Flat(form.SunAt - sunWas).magnitude:0.0} m says '{defence.LastCallout}' balance {form.Stagger.Value:0}");
        yield return Shot("exposed_falcon_1");
        Vector3 before = form.SunAt;
        int pushes = form.SunPushes;
        form.SunPart.TakeHit(new Hit(20f, 0f, Vector3.left, true));
        yield return new WaitForSeconds(0.5f);
        float open = form.Health.Current;
        form.BodyPart.TakeHit(new Hit(10f, 0f, Vector3.right));
        Say($"6 the loose sun pushed by a heavy blow: pushes +{form.SunPushes - pushes} moved {Flat(form.SunAt - before).magnitude:0.0} m; a blow of 10 on the exposed body took {open - form.Health.Current:0.0}");
        float lay = Time.time;
        yield return Until(() => !form.Exposed, 14f);
        Say($"6 it took the sun up again after {Time.time - lay + 1.3f:0.0} s: loose {form.SunLoose} doing {form.Doing}");

        // The scarab on its back, for the picture.
        yield return Idle();
        form.Queued = SunBearerAttack.Roll;
        Go(south, Vector3.forward);
        yield return Until(() => form.Doing == "Roll", 12f);
        yield return new WaitForSeconds(0.8f);
        Go(new Vector3(12f, 0f, -12f), Vector3.forward);
        for (int i = 0; i < 4 && !form.SunLoose; i++) form.BodyPart.TakeHit(new Hit(10f, 0f, Vector3.right));
        yield return new WaitForSeconds(1.2f);
        yield return Shot("exposed_scarab_1");
        Say($"6 the scarab knocked off the sun mid-roll: loose {form.SunLoose} exposed {form.Exposed}");
        yield return Until(() => !form.Exposed, 14f);

        yield return PerfectGuard(south);

        // Balance broken and the finisher.
        yield return Idle();
        Go(Flat(form.BodyAt) + Flat(south - form.BodyAt).normalized * 5f, form.BodyAt - south);
        form.Unbalance(999f);
        yield return new WaitForSeconds(1f);
        yield return Shot("broken_1");
        health = form.Health.Current;
        var outcome = form.HeadPart.TakeHit(new Hit(30f, 0f, Vector3.forward, true));
        Say($"7 balance broken: staggered {form.TimesStaggered} time(s), a heavy blow on the fallen head was a {outcome} worth {health - form.Health.Current:0} (at least {form.MaximumHealth * form.FinisherShare:0} wanted) doing {form.Doing}");
        yield return Until(() => !form.Exposed, 14f);

        // Fury: it opens with the blaze, and the shadow stops and comes back.
        form.Health.Damage(Mathf.Max(0f, form.Health.Current - form.MaximumHealth * 0.45f));
        yield return new WaitForSeconds(0.3f);
        Say($"8 fury: furious {form.Furious} says '{defence.LastCallout}' shadow rate {court.Hand.DegreesPerSecond:0.0}");
        follow = () => court.Hand.Centre + court.Hand.Direction * 9f + Vector3.up * 0.1f;
        yield return Until(() => form.Blazing, 20f);
        int bites = form.BlazeBites;
        bool stopped = false;
        float rateBefore = court.Hand.DegreesPerSecond;
        float blazeBegan = Time.time;
        while (form.Blazing && Time.time - blazeBegan < 12f)
        {
            if (court.Hand.Stopped > 0f && !stopped)
            {
                stopped = true;
                yield return Shot("fury_blaze_1");
            }
            yield return null;
        }
        follow = null;
        Say($"8 the fury's blaze: lasted {Time.time - blazeBegan:0.0} s, the shadow stopped {stopped} and turned back (rate {rateBefore:0.0} -> {court.Hand.DegreesPerSecond:0.0}), bites while walking with it +{form.BlazeBites - bites}");

        // A fall mid-fight.
        yield return new WaitForSeconds(1f);
        keepAlive = false;
        int deaths = defence.Deaths;
        defence.TakeHit(new Hit(9999f, 0f, Vector3.forward, false, true));
        yield return Until(() => defence.IsDead, 1f);
        float fell = Time.time;
        while (defence.IsDead && Time.time - fell < 20f) yield return null;
        yield return new WaitForSeconds(0.5f);
        keepAlive = true;
        yield return Hear("after the fall", 1f);
        Vector3 at = player.transform.position, wanted = director.ReturnPoint.position;
        Say($"9 a fall mid-fight: deaths +{defence.Deaths - deaths} woke at {at:0.0} return point {wanted:0.0} {((at - wanted).magnitude < 1.5f ? "AT THE FIRE" : "ELSEWHERE")}; form risen {form.HasRisen} "
            + $"king dead {boss.IsDead} king health {boss.Health.Current:0} of {boss.Health.Maximum:0} begun {court.Begun} retries {court.Retries} shadow angle {court.Hand.Angle:0.0} rate {court.Hand.DegreesPerSecond:0.0} element {element.HasElement}");
        yield return Shot("returned_1");

        // Again, to the end.
        Go(new Vector3(0f, 0f, -17f), Vector3.forward);
        yield return new WaitForSeconds(0.5f);
        yield return Hear("the king, the second time", 0.2f);
        Say($"9 across the line again: begun {court.Begun}");
        yield return FellMan(new Vector3(0f, 0f, -6f), 1);
        yield return Idle();
        int gods = RunState.GodsFelled, embers = RunState.Points;
        var upgrades = player.GetComponent<PlayerUpgrades>();
        float diedAt = Time.time;
        yield return Death(Flat(form.BodyAt) + Flat(south - form.BodyAt).normalized * 5f, 1);
        yield return Until(() => court.Felled, 8f);
        Say($"10 death: {Time.time - diedAt:0.0} s from the last blow; dead {form.IsDead} felled {court.Felled} gods felled +{RunState.GodsFelled - gods} bar shown {form.InFight} says '{defence.LastCallout}'");
        yield return Hear("the king dying", 0.6f);
        var coal = FindAnyObjectByType<GodReward>(FindObjectsInactive.Include);
        yield return Until(() => coal.gameObject.activeInHierarchy, 5f);
        bool inShadow = Sunlight.InHand(coal.transform.position);
        Go(coal.transform.position + new Vector3(0f, 0f, -1.4f), Vector3.forward);
        yield return new WaitForSeconds(0.6f);
        yield return Shot("coal_1");
        float angle = court.Hand.Angle;
        yield return new WaitForSeconds(0.5f);
        Say($"10 the coal: shown {coal.gameObject.activeInHierarchy} in the shadow {inShadow} prompt '{coal.Prompt}' shadow stopped {Mathf.Approximately(angle, court.Hand.Angle)} element before {element.HasElement}");
        coal.Use(interaction);
        yield return new WaitForSeconds(0.4f);
        string said = defence.LastCallout;
        yield return Hear("the sponsor, after", 0.3f);
        var door = FindAnyObjectByType<SceneDoor>();
        float mana = combat.Mana.Current;
        combat.Mana.Gain(100f);
        Go(new Vector3(0f, 0f, -10f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        element.CastBaseFree();
        yield return new WaitForSeconds(0.5f);
        yield return Shot("fire_1");
        Say($"11 taking Fire: said '{said}' has element {element.HasElement} kit {element.Kit.Name} run element '{RunState.Element}' chosen {court.HasChosen} gift owned {upgrades.Owns(Gifts.SignatureOf(Elements.Fire).Id)} "
            + $"weapon {combat.Weapon.Name} spell cast without error; way out open {door.Open} prompt '{door.Prompt}' leads to {door.LeadsTo}; embers {embers} -> {RunState.Points}");
    }

    // Straight to the blaze, and held there with the game all but stopped, for pictures at every screen shape.
    private IEnumerator Hold()
    {
        Go(new Vector3(0f, 0f, -17f), Vector3.forward);
        yield return new WaitForSeconds(0.4f);
        yield return Hear("the king", 0.2f);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        yield return Until(() => form.InFight, 12f);
        follow = () => court.Hand.Centre + court.Hand.Direction * 12f + Vector3.up * 0.1f;
        form.Queued = SunBearerAttack.Pass;
        yield return Until(() => form.Doing == "Pass" && form.DoingFor > 1.6f, 20f);
        Time.timeScale = 0.0001f;
        Debug.Log("[Probe] holding pass");
        yield return new WaitForSecondsRealtime(45f);
        Time.timeScale = 1f;
        form.Queued = SunBearerAttack.Zenith;
        yield return Until(() => form.Blazing, 30f);
        yield return new WaitForSeconds(1.5f);
        Time.timeScale = 0.0001f;
        Debug.Log("[Probe] holding blaze");
        yield return new WaitForSecondsRealtime(45f);
        Time.timeScale = 1f;
        follow = null;
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }
}
