using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the Sun-Bearer's fight as reworked on 2026-11-18 (fire_land.md, "As built: the fight reworked"):
//   Clip     every attack from three places by the rim and one near the obelisk, the exposed tumble, the broken
//            balance and the falcon's fall, with the worst reach of any piece of either shape or of the sun past the
//            court's rim (into the wall, or out over the floor's edge low down) and into the obelisk.
//   Camp     a player holding the scarab's right flank and attacking for 20 s while it fights as it likes.
// More parts are added below as the rework goes. Run with
//   bash AgentScripts/runprobe.sh SunCourt SunFightProbe 200
// AgentScripts/sunfight_only.txt may hold part names with spaces between; empty or missing runs everything.
public static class SunFightProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Sun Fight Probe").AddComponent<SunFightProbeRunner>();
        return "probe started";
    }
}

public class SunFightProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private SunCourtDirector court;
    private SunBearer form;
    private Enemy boss;
    private string only = "";
    private Func<Vector3> follow;
    private bool keepAlive = true;

    // The clip measure.
    private bool measuring;
    private Renderer[] pieces = new Renderer[0];
    private Transform sunBall;
    private readonly Dictionary<string, float> worstRim = new Dictionary<string, float>();
    private readonly Dictionary<string, string> worstRimWho = new Dictionary<string, string>();
    private readonly Dictionary<string, float> worstStone = new Dictionary<string, float>();
    private readonly Dictionary<string, string> worstStoneWho = new Dictionary<string, string>();
    private readonly Dictionary<string, float> worstFloor = new Dictionary<string, float>();
    private readonly Dictionary<string, string> worstFloorWho = new Dictionary<string, string>();
    private string label = "";
    private readonly Vector3[] corners = new Vector3[8];

    private const float Rim = 20f, WallFace = 20.4f;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private bool Wants(string word) => only.Length == 0 || only.Split(' ').Contains(word);

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.time + timeout;
        while (!condition() && Time.time < end) yield return null;
    }

    private void Go(Vector3 to, Vector3 facing)
    {
        facing.y = 0f;
        player.Teleport(new Vector3(to.x, 0.1f, to.z), facing.sqrMagnitude > 0.01f ? Quaternion.LookRotation(facing) : Quaternion.identity);
    }

    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    private static Vector3 Polar(float degrees, float radius) => Quaternion.Euler(0f, degrees, 0f) * Vector3.forward * radius;

    private void Update()
    {
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (follow != null && player != null) player.Teleport(follow(), player.transform.rotation);
    }

    // ------------------------------------------------------------------ the clip measure

    // The wall is three metres high on the north, east and west and 1.2 low on the south; the forecourt's mouth has none.
    private static float WallTop(Vector3 point)
    {
        float angle = Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
        float fromSouth = Mathf.Abs(Mathf.DeltaAngle(angle, 180f));
        return fromSouth < 75f ? 1.2f : 3f;
    }

    // How far a point is inside the obelisk (a square 2.4 m across at its foot, 1.4 at 13 m), 0 when outside.
    private static float InStone(Vector3 point)
    {
        if (point.y > 14.4f || point.y < -0.5f) return 0f;
        float half = Mathf.Lerp(1.2f, 0.7f, Mathf.Clamp01(point.y / 13f));
        float inX = half - Mathf.Abs(point.x), inZ = half - Mathf.Abs(point.z);
        return inX > 0f && inZ > 0f ? Mathf.Min(inX, inZ) : 0f;
    }

    private void Note(Dictionary<string, float> worst, Dictionary<string, string> who, float amount, string piece)
    {
        if (!worst.TryGetValue(label, out float was)) was = -99f;
        if (amount <= was) return;
        worst[label] = amount;
        who[label] = piece + " (" + form.Doing + ")";
    }

    private void Measure(Vector3 point, string piece)
    {
        float r = Flat(point).magnitude;
        // Past the rim low enough to be in the wall, or in the air over the floor's edge where nothing stands.
        if (point.y < WallTop(point) + 0.2f) Note(worstRim, worstRimWho, r - Rim, piece);
        Note(worstStone, worstStoneWho, InStone(point), piece);
        // Not while it is meant to be in the ground: digging in, underground, or bursting up out of it.
        if (form.BodyAt.y > -0.05f && form.Doing != "Burrow") Note(worstFloor, worstFloorWho, -point.y, piece);
    }

    private void LateUpdate()
    {
        if (!measuring || form == null || !form.HasRisen || form.IsDead) return;
        string doing = form.Doing;
        if (doing == "Entering" || doing == "Dying" || doing == "Dead") return;
        foreach (var shown in pieces)
        {
            if (shown == null || !shown.enabled || !shown.gameObject.activeInHierarchy) continue;
            var bounds = shown.localBounds;
            Matrix4x4 toWorld = shown is SkinnedMeshRenderer skinned && skinned.rootBone != null ? skinned.rootBone.localToWorldMatrix : shown.transform.localToWorldMatrix;
            Vector3 c = bounds.center, e = bounds.extents;
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z);
                Measure(toWorld.MultiplyPoint3x4(c + corner), shown.name);
            }
        }
        if (sunBall != null && sunBall.gameObject.activeInHierarchy)
        {
            float radius = 1.5f * sunBall.lossyScale.x;
            Vector3 at = sunBall.position;
            Vector3 outward = Flat(at).sqrMagnitude > 0.01f ? Flat(at).normalized : Vector3.forward;
            Measure(at + outward * radius, "sun");
            Measure(at - outward * radius, "sun");
            Measure(at + Vector3.down * radius, "sun");
        }
    }

    private string Worst(Dictionary<string, float> worst, Dictionary<string, string> who, string key)
        => worst.TryGetValue(key, out float amount) ? $"{amount:0.00} m by {who[key]}" : "none";

    private void Report(string key)
    {
        Say($"   clip {key}: rim {Worst(worstRim, worstRimWho, key)}; obelisk {Worst(worstStone, worstStoneWho, key)}; under the floor {Worst(worstFloor, worstFloorWho, key)}");
    }

    // ------------------------------------------------------------------ start

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        court = FindAnyObjectByType<SunCourtDirector>();
        form = FindAnyObjectByType<SunBearer>();
        boss = FindAnyObjectByType<AreaDirector>().Enemies[0];
        only = System.IO.File.Exists("AgentScripts/sunfight_only.txt") ? System.IO.File.ReadAllText("AgentScripts/sunfight_only.txt").Trim() : "";
        var root = form.transform;
        var listed = new List<Renderer>();
        foreach (var shown in root.GetComponentsInChildren<Renderer>(true))
        {
            string path = shown.transform.name;
            for (var up = shown.transform.parent; up != null && up != root; up = up.parent) path = up.name + "/" + path;
            if (!path.StartsWith("Body/")) continue;
            if (path.Contains("Reach") || path.Contains("Bulk")) continue;
            listed.Add(shown);
        }
        pieces = listed.ToArray();
        sunBall = root.Find("Sun");
        Say($"start: {pieces.Length} pieces measured; parts '{only}'");
        combat.Equip(Weapons.ByName(Weapons.GreatSpearName));
        yield return new WaitForSeconds(1.2f);
        yield return Read();
        Go(new Vector3(0f, 0f, -18.6f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        yield return Read();
        if (Wants("Ring")) yield return Ring();
        yield return Rise();

        if (Wants("Stagger")) yield return StaggerLoop();
        if (Wants("Sun")) yield return SunAsBall();
        if (Wants("Hot")) yield return HotFloor();
        if (Wants("Burrow")) yield return Burrowing();
        if (Wants("Attacks")) yield return NewAttacks();
        if (Wants("Clip")) yield return Clip();
        if (Wants("Camp")) yield return Camp();
        if (Wants("Perf")) yield return Busiest();
        Say($"lanes moved aside so the player kept a way through {form.LanesShifted}, laid without fire {form.LanesUnlit}");

        Say("done");
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }

    private IEnumerator Read()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        var box = ReadingBox.Instance;
        for (int i = 0; i < 30 && box != null && box.IsOpen; i++)
        {
            Keys(Key.E);
            yield return null;
            yield return null;
            Keys();
            yield return null;
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    private IEnumerator Rise()
    {
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(0f, 0.05f, 8f), Quaternion.LookRotation(Vector3.back));
        Go(new Vector3(0f, 0f, -6f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        form.OnlyWhenAsked = true;
        yield return Until(() => form.HasRisen, 8f);
        yield return Until(() => form.Doing == "Idle", 14f);
        Say($"the form has risen: doing {form.Doing}");
    }

    private IEnumerator Idle(float timeout = 16f)
    {
        yield return Until(() => form.Doing == "Idle", timeout);
    }

    // Asks for a move by name (skipped when this build has no such move) and waits for it and whatever change of shape it needs.
    private IEnumerator Ask(string name, Vector3 stand, float timeout = 14f)
    {
        if (!Enum.TryParse(name, out SunBearerAttack attack)) yield break;
        yield return Idle();
        Go(stand, form.BodyAt - stand);
        form.Queued = attack;
        float began = Time.time;
        yield return Until(() => form.Doing == name || Time.time - began > timeout, timeout);
        yield return Until(() => form.Doing != name, timeout);
    }

    // ------------------------------------------------------------------ Clip

    private static readonly string[] Attacks = { "Roll", "Ram", "Sparks", "Furrows", "Ricochet", "Burrow", "Flares", "Rising", "Stoop", "Feathers", "Pass", "Gust", "Grab", "Sunfall", "Zenith", "Setting" };
    private static readonly Vector3[] Places = { Polar(10f, 16.5f), Polar(95f, 16.5f), Polar(220f, 16.5f), Polar(300f, 6f) };

    private IEnumerator Clip()
    {
        Say("Clip: every attack from four places, then the exposed tumble, the broken balance and the falcon's fall");
        measuring = true;
        // AgentScripts/sunfight_clip.txt may name only some attacks (spaces between), and "NoStates" to leave out the tumble and the falls.
        string clipOnly = System.IO.File.Exists("AgentScripts/sunfight_clip.txt") ? System.IO.File.ReadAllText("AgentScripts/sunfight_clip.txt").Trim() : "";
        foreach (string name in Attacks)
        {
            if (!Enum.TryParse(name, out SunBearerAttack _)) continue;
            if (clipOnly.Length > 0 && !clipOnly.Split(' ').Contains(name)) continue;
            label = name;
            for (int i = 0; i < Places.Length; i++)
            {
                // The change of shape asked by name is done whichever shape it is in: the other first.
                if (name == "Setting" && !form.IsFalcon) yield return Ask("Rising", Places[i]);
                if (name == "Rising" && form.IsFalcon) yield return Ask("Setting", Places[i]);
                label = name;
                yield return Ask(name, Places[i]);
                yield return Until(() => form.Doing == "Idle", 10f);
            }
            Report(name);
        }

        // Knocked off the sun mid-roll and at the far rim, three times each, and what follows until it holds the sun again.
        if (clipOnly.Contains("NoStates")) goto Summary;
        label = "Exposed";
        for (int i = 0; i < 3; i++)
        {
            if (form.IsFalcon) yield return Ask("Setting", Places[i]);
            yield return Idle();
            Go(Places[i], form.BodyAt - Places[i]);
            form.Queued = SunBearerAttack.Roll;
            yield return Until(() => form.Doing == "Roll" || form.Doing == "Furrows", 12f);
            yield return Until(() => form.HotLanes > 0 || form.Doing != "Roll", 8f);
            yield return new WaitForSeconds(i * 0.6f);
            for (int blow = 0; blow < 4 && !form.SunLoose; blow++) form.BodyPart.TakeHit(new Hit(10f, 0f, Vector3.right));
            yield return Until(() => !form.Exposed && form.Doing == "Idle", 16f);
        }
        Report("Exposed");

        // Broken where it stands at the rim after a roll, and as the falcon on its sun after a stoop.
        label = "Staggered";
        for (int i = 0; i < 3; i++)
        {
            if (form.IsFalcon) yield return Ask("Setting", Places[i]);
            yield return Ask("Roll", Places[i]);
            form.ForceBreak();
            yield return Until(() => !form.IsStaggered && !form.Exposed && form.Doing == "Idle", 18f);
        }
        Report("Staggered");
        label = "Falcon down";
        for (int i = 0; i < 3; i++)
        {
            yield return Idle();
            Go(Places[i] * 0.95f, Vector3.forward);
            form.Queued = SunBearerAttack.Stoop;
            yield return Until(() => form.Doing == "Stoop", 14f);
            yield return Until(() => form.BodyPart.Vulnerable || form.Doing != "Stoop", 5f);
            if (i == 0) form.ForceBreak();
            else for (int blow = 0; blow < 4 && !form.SunLoose; blow++) form.BodyPart.TakeHit(new Hit(10f, 0f, Vector3.right));
            yield return Until(() => !form.IsStaggered && !form.Exposed && form.Doing == "Idle", 18f);
        }
        Report("Falcon down");
        Summary:
        measuring = false;

        var keys = worstRim.Keys.Union(worstStone.Keys).ToList();
        float rimAll = keys.Count > 0 ? keys.Max(k => worstRim.TryGetValue(k, out float a) ? a : 0f) : 0f;
        float stoneAll = keys.Count > 0 ? keys.Max(k => worstStone.TryGetValue(k, out float a) ? a : 0f) : 0f;
        Say($"Clip overall: worst past the rim {rimAll:0.00} m, worst into the obelisk {stoneAll:0.00} m {(rimAll <= 0.05f && stoneAll <= 0.05f ? "OK" : "WRONG")}");
    }

    // ------------------------------------------------------------------ Camp

    // A player who keeps to the scarab's right flank, three metres off its middle, and attacks without stopping.
    private IEnumerator Camp()
    {
        Say("Camp: twenty seconds on the scarab's right flank, attacking");
        if (form.IsFalcon) yield return Ask("Setting", Places[2]);
        yield return Idle();
        form.OnlyWhenAsked = false;
        int taken = defence.BlowsTaken, landed = combat.BlowsLanded;
        float healthBefore = form.Health.Current;
        var blows = new List<string>();
        var doings = new List<string>();
        string lastBlow = form.LastBlow, lastDoing = "";
        int lastTaken = taken;
        float began = Time.time, flankTime = 0f, hurt = 0f;
        float healthWas = defence.Health.Current;
        int tick = 0;
        bool heated = false, answered = false;
        keepAlive = false;
        while (Time.time - began < 20f)
        {
            if (defence.Health.Current < healthWas) hurt += healthWas - defence.Health.Current;
            if (defence.Health.Current < 60f) defence.Health.Heal(100f);
            healthWas = defence.Health.Current;
            if (form.Doing != lastDoing)
            {
                lastDoing = form.Doing;
                doings.Add(lastDoing);
                if ((lastDoing == "ShellBurst" || lastDoing == "Kick") && !answered)
                {
                    answered = true;
                    yield return new WaitForSeconds(lastDoing == "Kick" ? 0.3f : 0.4f);
                    yield return Frozen("camp_answer");
                }
            }
            if (!heated && form.CampHeatNow > 0.75f)
            {
                heated = true;
                yield return Frozen("camp_heat");
            }
            if (defence.BlowsTaken != lastTaken)
            {
                lastTaken = defence.BlowsTaken;
                blows.Add(form.LastBlow);
            }
            if (!form.IsFalcon && !form.Doing.StartsWith("Burrow"))
            {
                float yaw = form.transform.Find("Body").eulerAngles.y;
                Vector3 right = Quaternion.Euler(0f, yaw + 90f, 0f) * Vector3.forward;
                Vector3 spot = Flat(form.BodyAt) + right * 3f + Vector3.up * 0.1f;
                if (Flat(spot).magnitude > 18.5f) spot = Flat(spot).normalized * 18.5f + Vector3.up * 0.1f;
                if (Flat(player.transform.position - spot).magnitude > 1.2f && !combat.IsAttacking) Go(spot, form.BodyAt - spot);
                if (Flat(player.transform.position - form.BodyAt).magnitude < 4.5f) flankTime += Time.deltaTime;
                tick++;
                if (!combat.IsAttacking && tick % 8 < 2)
                {
                    if (tick % 8 == 0) Go(player.transform.position, form.BodyAt - player.transform.position);
                    // Three light blows to one heavy.
                    Keys(tick / 8 % 4 == 3 ? Key.K : Key.J);
                }
                else Keys();
            }
            else Keys();
            yield return null;
        }
        Keys();
        keepAlive = true;
        form.OnlyWhenAsked = true;
        Say($"Camp: on the flank {flankTime:0.0} s of 20; blows landed {combat.BlowsLanded - landed}; the form lost {healthBefore - form.Health.Current:0} health; "
            + $"the player was struck {defence.BlowsTaken - taken} times for {hurt:0} ({string.Join(", ", blows)}); what it did: {string.Join(" > ", doings)}");
    }

    // ------------------------------------------------------------------ pictures

    // Time is stopped for the picture, so it shows the instant asked for; then time goes on at its usual speed.
    private IEnumerator Frozen(string name)
    {
        Time.timeScale = 0f;
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/sunfight_" + name + ".png");
        yield return null;
        yield return null;
        Time.timeScale = 1f;
        Say($"   picture sunfight_{name}");
    }

    private IEnumerator Tap(Key key)
    {
        Keys(key);
        yield return null;
        yield return null;
        Keys();
        yield return null;
    }

    private static readonly Vector3 South = new Vector3(0f, 0f, -10f);

    // Knocks the sun out of the scarab's hold with blows on its body while it stands idle.
    private IEnumerator KnockLoose()
    {
        if (form.IsFalcon) yield return Ask("Setting", South);
        yield return Idle();
        // Not while it is poised after a fall: then blows on the shell only hurt it.
        yield return Until(() => !form.Poise.Holds, 14f);
        for (int i = 0; i < 6 && !form.SunLoose; i++) form.BodyPart.TakeHit(new Hit(10f, 0f, Vector3.right));
        yield return new WaitForSeconds(0.2f);
    }

    // ------------------------------------------------------------------ Ring: the man's planted spear

    private IEnumerator Ring()
    {
        Say("Ring: the man's planted spear, a field of flame inside and a wall round it");
        boss.Restore();
        // South-west of the obelisk, so the camera over the player sees the whole ring with nothing in front of it.
        boss.transform.SetPositionAndRotation(new Vector3(-6f, 0.05f, -8f), Quaternion.LookRotation(Vector3.back));
        Go(boss.transform.position + Vector3.back * 3f, Vector3.forward);
        boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        yield return new WaitForSeconds(0.2f);
        Vector3 stand = boss.transform.position + boss.transform.forward * 3f;
        Go(stand, boss.transform.position - stand);
        bool forced = boss.Brain.Force("sunking_plant");
        yield return Until(() => boss.Brain.Current == null || boss.Brain.State != EnemyState.Telegraph, 4f);
        // Out of it to see it whole.
        Go(boss.transform.position + new Vector3(1.5f, 0f, -7.5f), Vector3.forward);
        yield return new WaitForSeconds(0.35f);
        yield return Frozen("ring_1");
        yield return new WaitForSeconds(2.5f);
        yield return Frozen("ring_2");
        yield return new WaitForSeconds(4.6f);
        yield return Frozen("ring_dying");
        yield return new WaitForSeconds(1.2f);
        yield return Frozen("ring_char");
        Say($"Ring: forced {forced}");
        yield return new WaitForSeconds(1f);
    }

    // ------------------------------------------------------------------ Stagger: the old endless loop

    // What made the scarab stagger without end: knock its sun away, and every time it comes back with it, heavy blows.
    // Thirty seconds of a player doing exactly that, its health kept up so it does not die of it.
    private IEnumerator StaggerLoop()
    {
        Say("Stagger: thirty seconds of heavy blows on it every time it can be struck, its sun knocked away first");
        yield return KnockLoose();
        int staggers = form.TimesStaggered, knocks = form.SunKnocks;
        float began = Time.time, downFor = 0f, poisedFor = 0f;
        var doings = new List<string>();
        string last = "";
        float untilBlow = 0f;
        while (Time.time - began < 30f)
        {
            form.Health.Heal(1000f);
            if (form.Doing != last)
            {
                last = form.Doing;
                doings.Add(last + (form.Poise.Holds ? "*" : ""));
            }
            if (form.IsStaggered || (form.Exposed && !form.Poise.Returning)) downFor += Time.deltaTime;
            if (form.Poise.Holds) poisedFor += Time.deltaTime;
            untilBlow -= Time.deltaTime;
            if (untilBlow <= 0f)
            {
                untilBlow = 0.55f;
                // The great spear's heavy blow: 44 and 56 of balance, on whatever can be struck.
                var heavy = new Hit(44f, 56f, Vector3.forward, true);
                if (form.HeadPart.Vulnerable) form.HeadPart.TakeHit(heavy);
                else if (form.BodyPart.Vulnerable) form.BodyPart.TakeHit(heavy);
            }
            yield return null;
        }
        Say($"Stagger: in 30 s broken {form.TimesStaggered - staggers} time(s), sun knocked {form.SunKnocks - knocks} time(s); down (lying or broken) {downFor:0.0} s, poised {poisedFor:0.0} s "
            + $"({(form.TimesStaggered - staggers <= 2 ? "OK" : "WRONG")}); what it did (* poised): {string.Join(" > ", doings)}");
        yield return Until(() => form.Doing == "Idle", 16f);
    }

    // ------------------------------------------------------------------ Sun: the loose sun as a ball

    private IEnumerator SunAsBall()
    {
        Say("Sun: the loose sun pays nothing, shows where a heavy blow sends it, and set rolling into the scarab sets its shell alight");
        yield return KnockLoose();
        Say($"   the sun knocked loose: {form.SunLoose}, exposed {form.Exposed}");
        yield return new WaitForSeconds(0.9f);
        // A light blow on the sun with flasks empty and mana spent: nothing is earned.
        while (defence.Flasks.Count > 0 && defence.Flasks.TryTake()) { }
        combat.Mana.Empty();
        float flaskBefore = defence.Flasks.RechargeFraction, manaBefore = combat.Mana.Current;
        int pushes = form.SunPushes;
        Vector3 sun = Flat(form.SunAt);
        Vector3 toBody = Flat(form.BodyAt - sun).normalized;
        // Stand on the far side of the sun from the scarab, facing through it.
        Vector3 stand = sun - toBody * 2.6f;
        Go(stand, sun - stand);
        yield return new WaitForSeconds(0.25f);
        bool aiming = form.Aiming;
        yield return Frozen("aim_1");
        yield return Tap(Key.J);
        yield return new WaitForSeconds(0.7f);
        Say($"   a light blow on the sun: pushes +{form.SunPushes - pushes} mana {manaBefore:0} to {combat.Mana.Current:0}, flask earned {flaskBefore:0.000} to {defence.Flasks.RechargeFraction:0.000} "
            + $"aim line shown {aiming} {(form.SunPushes > pushes && combat.Mana.Current <= manaBefore + 0.01f && defence.Flasks.RechargeFraction <= flaskBefore + 0.0001f && aiming ? "OK" : "WRONG")}");
        defence.Flasks.Refill();
        // Now a heavy blow straight at the scarab.
        int strikes = form.SunStrikes;
        float balance = form.Stagger.Value, health = form.Health.Current;
        sun = Flat(form.SunAt);
        toBody = Flat(form.BodyAt - sun).normalized;
        stand = sun - toBody * 2.6f;
        Go(stand, sun - stand);
        yield return new WaitForSeconds(0.2f);
        yield return Tap(Key.K);
        float struckAt = -1f;
        float until = Time.time + 3f;
        bool pictured = false;
        while (Time.time < until && form.SunStrikes == strikes)
        {
            if (!pictured && form.SunRollingHard && Flat(form.SunAt - sun).magnitude > 2.5f)
            {
                pictured = true;
                yield return Frozen("rolled_1");
            }
            yield return null;
        }
        if (form.SunStrikes > strikes) struckAt = Time.time;
        yield return new WaitForSeconds(0.12f);
        yield return Frozen("shellfire_1");
        yield return new WaitForSeconds(0.8f);
        yield return Frozen("shellfire_2");
        Say($"   a heavy blow rolled it into the scarab: strikes +{form.SunStrikes - strikes} balance {balance:0} to {form.Stagger.Value:0} (broken {form.IsStaggered}) health {health:0} to {form.Health.Current:0} "
            + $"says '{defence.LastCallout}' {(form.SunStrikes > strikes ? "OK" : "WRONG")}");
        yield return Until(() => form.Doing == "Idle", 20f);
    }

    // ------------------------------------------------------------------ Hot: the hot floor's rule and look

    private IEnumerator HotFloor()
    {
        Say("Hot: a roll's lane burns six seconds, narrowing from its edges; walked across, one bite; rolled across, none");
        if (form.IsFalcon) yield return Ask("Setting", South);
        yield return Idle();
        Vector3 stand = Quaternion.Euler(0f, 75f, 0f) * Flat(form.BodyAt).normalized * 12.5f;
        Go(stand, form.BodyAt - stand);
        form.Queued = SunBearerAttack.Roll;
        yield return Until(() => form.Doing == "Roll", 12f);
        yield return Until(() => form.Locked, 6f);
        // Out of its way.
        Vector3 across = new Vector3(form.LaneAlong.z, 0f, -form.LaneAlong.x);
        Vector3 aside = stand + across * 7f;
        if (aside.magnitude > 17f) aside = stand - across * 7f;
        Go(aside, form.BodyAt - aside);
        yield return Until(() => form.HotLanes > 0, 6f);
        float lit = Time.time;
        Vector3 from = form.LaneFrom, along = form.LaneAlong;
        float length = form.LaneLength;
        yield return new WaitForSeconds(0.45f);
        yield return Frozen("lane_0");
        // Across the start of the lane once the sun and the scarab are past it, while it burns its hottest.
        yield return Until(() => Time.time - lit > 0.9f, 3f);
        Vector3 middle = from + along * 3.5f;
        Vector3 side = new Vector3(along.z, 0f, -along.x);
        // Walked straight across at the player's own pace.
        int bites = form.HotBites;
        Vector3 start = middle - side * 4f, end = middle + side * 4f;
        float walkedFor = 0f;
        follow = () => Vector3.Lerp(start, end, Mathf.Clamp01(walkedFor / (8f / player.MoveSpeed))) + Vector3.up * 0.1f;
        while (walkedFor < 8f / player.MoveSpeed)
        {
            walkedFor += Time.deltaTime;
            yield return null;
        }
        follow = null;
        int walked = form.HotBites - bites;
        yield return Until(() => Time.time - lit > 1.6f, 3f);
        yield return Frozen("lane_1");
        // Rolled across: put just short of it and roll over it.
        bites = form.HotBites;
        Go(middle + side * 3.4f, -side);
        yield return null;
        Keys(Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.45f);
        Go(middle - side * 5f, side);
        int rolled = form.HotBites - bites;
        // And stood in it, for the count.
        bites = form.HotBites;
        float stoodFrom = Time.time;
        follow = () => middle + Vector3.up * 0.1f;
        yield return new WaitForSeconds(1.1f);
        follow = null;
        int stood = form.HotBites - bites;
        Go(middle - side * 5f, side);
        Say($"   (walked across {walked}, rolled across {rolled}, stood 1.1 s in it {stood}, all done {Time.time - lit:0.0} s after the lane was lit)");
        yield return Until(() => Time.time - lit > 3.6f, 5f);
        yield return Frozen("lane_3");
        yield return Until(() => !form.OnHotFloor(middle + side * 1.7f) || Time.time - lit > 8f, 8f);
        float edgeOut = Time.time - lit;
        yield return Frozen("lane_cooling");
        yield return Until(() => form.HotLanes == 0, 10f);
        float burnt = Time.time - lit;
        yield return new WaitForSeconds(0.6f);
        yield return Frozen("lane_char");
        Say($"   the lane: its edge stopped burning after {edgeOut:0.0} s, all of it after {burnt:0.0} s ({(burnt < 7.5f ? "OK" : "WRONG")}); walked across: {walked} bite(s) ({(walked <= 1 ? "OK" : "WRONG")}); "
            + $"rolled across: {rolled} ({(rolled == 0 ? "OK" : "WRONG")}); stood in it 1.1 s: {stood} bite(s) ({(stood == 2 ? "OK" : "WRONG")})");
        // The furrows, three lanes, pictured from beside them.
        yield return Idle();
        stand = Quaternion.Euler(0f, 75f, 0f) * Flat(form.BodyAt).normalized * 12.5f;
        Go(stand, form.BodyAt - stand);
        form.Queued = SunBearerAttack.Furrows;
        yield return Until(() => form.Doing == "Furrows", 12f);
        yield return Until(() => form.HotLanes >= 2 || form.Doing != "Furrows", 12f);
        follow = () => SafeSpot(new Vector3(0f, 0f, -12f));
        yield return Until(() => form.HotLanes >= 3 || form.Doing != "Furrows", 8f);
        yield return new WaitForSeconds(0.3f);
        yield return Frozen("furrows_hot");
        follow = null;
        yield return Until(() => form.Doing == "Idle", 12f);
    }

    // A place near the given one that is not on burning floor nor next to the burning sun.
    private Vector3 SafeSpot(Vector3 near)
    {
        for (int ring = 0; ring < 6; ring++)
            for (int i = 0; i < 12; i++)
            {
                Vector3 spot = near + Quaternion.Euler(0f, i * 30f, 0f) * Vector3.forward * (ring * 1.6f);
                if (spot.magnitude > 18f || spot.magnitude < 4.5f || form.OnHotFloor(spot) || Flat(spot - form.SunAt).magnitude < 3.4f) continue;
                return spot + Vector3.up * 0.1f;
            }
        return near + Vector3.up * 0.1f;
    }

    // ------------------------------------------------------------------ Burrow: never vanishing

    private IEnumerator Burrowing()
    {
        Say("Burrow: knocked off its sun, it digs in, goes under a ridge, bursts up beside the sun; then the Burrow as an attack");
        yield return KnockLoose();
        // Push the sun well away so it has to go underground for it.
        Vector3 sun = Flat(form.SunAt);
        // Along the ring rather than straight away, which can be into the rim a few metres off.
        Vector3 away = new Vector3(sun.z, 0f, -sun.x).normalized;
        if (Vector3.Dot(away, Flat(sun - form.BodyAt)) < 0f) away = -away;
        // A heavy blow sends it a long way: far enough that the scarab must dig for it.
        form.SunPart.TakeHit(new Hit(1f, 0f, away, true));
        yield return new WaitForSeconds(1.2f);
        if (form.SunLoose && Flat(form.SunAt - form.BodyAt).magnitude < 9f)
        {
            Say($"   (the sun stopped {Flat(form.SunAt - form.BodyAt).magnitude:0.0} m off; struck again)");
            form.SunPart.TakeHit(new Hit(1f, 0f, Flat(form.SunAt - form.BodyAt).normalized, true));
            yield return new WaitForSeconds(1.2f);
        }
        Say($"   the sun lies {Flat(form.SunAt - form.BodyAt).magnitude:0.0} m from the scarab");
        // Beside the way between the scarab and the sun, so the camera over the player sees the dig, the ridge and where it comes up.
        Vector3 between = (Flat(form.BodyAt) + Flat(form.SunAt)) * 0.5f;
        Vector3 across = new Vector3(away.z, 0f, -away.x);
        Vector3 watch = between + across * 4f;
        if (watch.magnitude > 15f || watch.magnitude < 5f) watch = between - across * 4f;
        Go(watch, between - watch);
        bool vanished = false;
        int seenDig = 0;
        float began = Time.time;
        bool dug = false, ridge = false, rising = false;
        while (Time.time - began < 20f && (form.Exposed || form.IsStaggered))
        {
            // It is always somewhere: drawn, or a ridge under the paving.
            bool drawn = form.transform.Find("Body").gameObject.activeInHierarchy;
            if (!drawn && !form.Underground) vanished = true;
            if (!dug && form.BodyAt.y < -0.6f && !form.Underground)
            {
                dug = true;
                seenDig++;
                yield return Frozen("dig_1");
            }
            if (!ridge && form.Underground && Time.time - began > 0.5f)
            {
                yield return new WaitForSeconds(0.35f);
                ridge = true;
                yield return Frozen("ridge_1");
            }
            if (ridge && !rising && !form.Underground && form.BodyAt.y < -1f)
            {
                rising = true;
                yield return Frozen("emerge_1");
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        yield return Frozen("emerge_2");
        Say($"   the return: dug {dug} ridge {ridge} came up {rising} never vanished {!vanished} holds the sun {!form.SunLoose} poised {form.Poise.Holds} {(dug && ridge && rising && !vanished && !form.SunLoose ? "OK" : "WRONG")}");
        yield return Until(() => form.Doing == "Idle", 10f);

        // The Burrow as an attack: the ridge chases a player walking away, locks, and bursts up.
        Vector3 stand = Flat(form.BodyAt) + Flat(South - form.BodyAt).normalized * 8f;
        Go(stand, form.BodyAt - stand);
        form.Queued = SunBearerAttack.Burrow;
        yield return Until(() => form.Doing == "Burrow", 12f);
        int taken = defence.BlowsTaken;
        Vector3 walkFrom = stand, walkTo = Quaternion.Euler(0f, 70f, 0f) * stand;
        float walked = 0f;
        follow = () => Vector3.Lerp(walkFrom, walkTo, Mathf.Clamp01(walked / 3f)) + Vector3.up * 0.1f;
        bool chased = false;
        while (form.Doing == "Burrow" && !form.Locked && walked < 6f)
        {
            walked += Time.deltaTime;
            if (!chased && form.Underground && walked > 1.1f)
            {
                chased = true;
                yield return Frozen("burrow_chase");
            }
            yield return null;
        }
        follow = null;
        yield return new WaitForSeconds(0.4f);
        yield return Frozen("burrow_locked");
        // The ridge locks where it is, a little behind a player walking away: stand in the circle for the blow.
        Go(form.MarkAt + Flat(South - form.MarkAt).normalized * 0.8f, form.MarkAt - South);
        yield return Until(() => !form.Underground || form.Doing != "Burrow", 3f);
        yield return new WaitForSeconds(0.08f);
        yield return Frozen("burrow_erupt");
        yield return new WaitForSeconds(0.6f);
        bool open = form.HeadPart.Vulnerable;
        yield return Frozen("burrow_halfout");
        Say($"   the Burrow: chased {chased} struck +{defence.BlowsTaken - taken} (last: {form.LastBlow}) head open half out {open} {(open && defence.BlowsTaken > taken ? "OK" : "WRONG")}");
        yield return Until(() => form.Doing == "Idle", 10f);
    }

    // ------------------------------------------------------------------ the new attacks

    private IEnumerator NewAttacks()
    {
        Say("Attacks: each new attack on a player standing still, then answered");
        // The Ricochet: its path drawn as three lanes, then the sun bouncing down them.
        yield return Idle();
        Vector3 stand = Quaternion.Euler(0f, 120f, 0f) * Flat(form.BodyAt).normalized * 10f;
        Go(stand, form.BodyAt - stand);
        int taken = defence.BlowsTaken;
        form.Queued = SunBearerAttack.Ricochet;
        yield return Until(() => form.Doing == "Ricochet", 12f);
        yield return Until(() => form.Locked || form.Doing != "Ricochet", 6f);
        yield return new WaitForSeconds(0.25f);
        yield return Frozen("ricochet_1");
        yield return Until(() => form.HotLanes > 0 || form.Doing != "Ricochet", 4f);
        yield return new WaitForSeconds(0.55f);
        yield return Frozen("ricochet_2");
        yield return new WaitForSeconds(0.7f);
        yield return Frozen("ricochet_3");
        float shellBefore = form.Health.Current;
        form.BodyPart.TakeHit(new Hit(20f, 0f, Vector3.forward));
        float cool = shellBefore - form.Health.Current;
        yield return Until(() => form.Doing != "Ricochet", 6f);
        Say($"   Ricochet standing still: struck +{defence.BlowsTaken - taken} (last: {form.LastBlow}); a blow of 20 on the cool shell took {cool:0} {(cool > 15f ? "OK" : "WRONG")}; then {form.Doing}");
        yield return Until(() => form.Doing == "Idle", 16f);

        // The Flares: lines from the planted sun, fired one after another.
        stand = Flat(form.BodyAt) + Flat(South - form.BodyAt).normalized * 9f;
        Go(stand, form.BodyAt - stand);
        taken = defence.BlowsTaken;
        form.Queued = SunBearerAttack.Flares;
        yield return Until(() => form.Doing == "Flares", 12f);
        yield return Until(() => form.DoingFor >= 1.1f || form.Doing != "Flares", 4f);
        yield return Frozen("flares_1");
        yield return Until(() => form.DoingFor >= 1.49f || form.Doing != "Flares", 4f);
        yield return Frozen("flares_2");
        yield return Until(() => form.DoingFor >= 2.0f || form.Doing != "Flares", 4f);
        yield return Frozen("flares_3");
        yield return Until(() => form.Doing != "Flares", 6f);
        Say($"   Flares standing still: struck +{defence.BlowsTaken - taken} (last: {form.LastBlow}) {(defence.BlowsTaken > taken ? "OK" : "WRONG")}");
        // And in a gap between the lines: the line through the player fires last, so stepping off it is the answer.
        yield return Idle();
        Go(stand, form.BodyAt - stand);
        taken = defence.BlowsTaken;
        form.Queued = SunBearerAttack.Flares;
        yield return Until(() => form.Doing == "Flares", 12f);
        yield return Until(() => form.DoingFor >= 0.7f, 4f);
        // Between the middle line and the next: nineteen degrees round from the line through them.
        Vector3 planted = Flat(form.SunAt);
        Vector3 gap = planted + Quaternion.Euler(0f, 19f, 0f) * Flat(stand - planted).normalized * 6f;
        Go(gap, planted - gap);
        yield return Until(() => form.Doing != "Flares", 6f);
        Say($"   Flares answered in a gap: struck +{defence.BlowsTaken - taken} {(defence.BlowsTaken == taken ? "OK" : "WRONG")}");
        yield return Until(() => form.Doing == "Idle", 12f);

        // The falcon's: the Gust standing still (shoved), with the guard held (holds ground).
        yield return Ask("Rising", South);
        for (int pass = 0; pass < 2; pass++)
        {
            yield return Idle();
            stand = new Vector3(4f, 0f, -9f);
            Go(stand, Vector3.forward);
            int shoves = player.Shoves, blocks = defence.Blocks;
            taken = defence.BlowsTaken;
            form.Queued = SunBearerAttack.Gust;
            yield return Until(() => form.Doing == "Gust", 12f);
            if (pass == 1)
            {
                yield return Until(() => form.Locked, 4f);
                Go(player.transform.position, form.BodyAt - player.transform.position);
                Keys(Key.L);
            }
            yield return Until(() => form.DoingFor >= form.GustTell * 0.95f || form.Doing != "Gust", 4f);
            if (pass == 0) yield return Frozen("gust_1");
            Vector3 was = Flat(player.transform.position);
            yield return Until(() => form.DoingFor >= form.GustTell + 0.45f || form.Doing != "Gust", 4f);
            if (pass == 0) yield return Frozen("gust_2");
            yield return Until(() => form.DoingFor >= form.GustTell + 0.9f || form.Doing != "Gust", 4f);
            if (pass == 0) yield return Frozen("gust_3");
            yield return Until(() => form.Doing != "Gust", 4f);
            yield return new WaitForSeconds(0.2f);
            Keys();
            float moved = Flat(player.transform.position - was).magnitude;
            Say($"   Gust {(pass == 0 ? "standing still" : "guard held")}: struck +{defence.BlowsTaken - taken} blocked +{defence.Blocks - blocks} shoved +{player.Shoves - shoves} moved {moved:0.0} m "
                + $"{((pass == 0 ? player.Shoves > shoves && moved > 2.5f : player.Shoves == shoves && moved < 1f) ? "OK" : "WRONG")}");
            yield return Until(() => form.Doing == "Idle" || form.Doing == "Stoop", 8f);
        }

        // The Grab: caught standing still (carried up and dropped), then rolled out of (it lands, open).
        for (int pass = 0; pass < 2; pass++)
        {
            yield return Idle();
            stand = new Vector3(-6f, 0f, -8f);
            Go(stand, Vector3.forward);
            int grabbed = form.Grabbed, missed = form.GrabsMissed;
            taken = defence.BlowsTaken;
            float health = defence.Health.Current;
            keepAlive = false;
            form.Queued = SunBearerAttack.Grab;
            yield return Until(() => form.Doing == "Grab", 12f);
            yield return Until(() => form.DoingFor >= form.GrabFollow + 0.2f, 4f);
            if (pass == 0) yield return Frozen("grab_1");
            if (pass == 1)
            {
                // A roll out of the circle just before it comes down.
                yield return Until(() => form.DoingFor >= form.GrabFollow + form.GrabHold - 0.12f, 3f);
                Go(player.transform.position, Vector3.right);
                Keys(Key.D, Key.Space);
                yield return null;
                yield return null;
                Keys();
            }
            float top = 0f;
            bool high = false, heldThen = false;
            float until = Time.time + 6f;
            while (Time.time < until && form.Doing == "Grab")
            {
                top = Mathf.Max(top, player.transform.position.y);
                if (pass == 0 && !high && player.transform.position.y > 5f)
                {
                    high = true;
                    heldThen = player.IsHeld;
                    yield return Frozen("grab_2");
                }
                if (pass == 1 && form.HeadPart.Vulnerable && !high)
                {
                    high = true;
                    yield return new WaitForSeconds(0.3f);
                    yield return Frozen("grab_missed");
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            bool free = !player.IsHeld && !player.Suspended;
            float lost = health - defence.Health.Current;
            keepAlive = true;
            defence.Health.Heal(200f);
            Say(pass == 0
                ? $"   Grab standing still: caught +{form.Grabbed - grabbed} highest {top:0.0} m held up there {heldThen} lost {lost:0} health; free after {free} on the floor at {player.transform.position.y:0.00} {(form.Grabbed > grabbed && top > 5f && free && lost >= 25f ? "OK" : "WRONG")}"
                : $"   Grab rolled out of: missed +{form.GrabsMissed - missed} caught +{form.Grabbed - grabbed} lost {lost:0}; free {free} {(form.GrabsMissed > missed && form.Grabbed == grabbed ? "OK" : "WRONG")}");
            yield return Until(() => form.Doing == "Idle", 10f);
            // A step and a blow after: the player's control is theirs again.
            if (pass == 0)
            {
                Vector3 before = Flat(player.transform.position);
                Keys(Key.W);
                yield return new WaitForSeconds(0.4f);
                Keys();
                Say($"   after the drop the player walks: {Flat(player.transform.position - before).magnitude:0.0} m {(Flat(player.transform.position - before).magnitude > 1f ? "OK" : "WRONG")}");
            }
        }

        // The Sunfall: pieces of the sun on circles round the player.
        yield return Idle();
        stand = new Vector3(5f, 0f, -10f);
        Go(stand, Vector3.forward);
        taken = defence.BlowsTaken;
        form.Queued = SunBearerAttack.Sunfall;
        yield return Until(() => form.Doing == "Sunfall", 12f);
        yield return Until(() => form.DoingFor >= 1.25f || form.Doing != "Sunfall", 4f);
        yield return Frozen("sunfall_1");
        yield return Until(() => form.DoingFor >= 1.78f || form.Doing != "Sunfall", 4f);
        yield return Frozen("sunfall_2");
        yield return Until(() => form.Doing != "Sunfall", 6f);
        yield return new WaitForSeconds(0.3f);
        yield return Frozen("sunfall_3");
        Say($"   Sunfall standing still: struck +{defence.BlowsTaken - taken} (last: {form.LastBlow}) small fires {form.HotPatches} {(defence.BlowsTaken > taken ? "OK" : "WRONG")}");
        yield return Ask("Setting", South);
    }

    // ------------------------------------------------------------------ Perf: the busiest it gets

    private IEnumerator Busiest()
    {
        Say("Perf: the fury asked for its fieriest run (the Furrows, the Flares, the Ricochet, the Rising, the Sunfall, the Pass), the player running round, every lane burning");
        if (form.IsFalcon) yield return Ask("Setting", South);
        yield return Idle();
        form.Health.Damage(Mathf.Max(0f, form.Health.Current - form.MaximumHealth * 0.45f));
        // The fury opens with the blaze: let it pass before measuring.
        form.OnlyWhenAsked = true;
        yield return Until(() => form.Doing == "Zenith", 10f);
        yield return Until(() => form.Doing == "Idle" && !form.IsFalcon, 30f);
        var garbage = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory, "GC Allocated In Frame");
        int frames = 0;
        long bytes = 0;
        float total = 0f, worst = 0f, began = Time.time;
        int mostHot = 0, mostLanes = 0;
        keepAlive = true;
        Say("busiest: the fury is fighting");
        var run = new[] { SunBearerAttack.Furrows, SunBearerAttack.Flares, SunBearerAttack.Ricochet, SunBearerAttack.Sunfall, SunBearerAttack.Pass };
        int next = 0;
        while (Time.time - began < 30f)
        {
            float t = Time.time - began;
            follow = () => new Vector3(Mathf.Sin(t * 0.35f) * 11f, 0.1f, Mathf.Cos(t * 0.35f) * 11f);
            if (form.Health.Current < form.MaximumHealth * 0.3f) form.Health.Heal(60f);
            if (form.Doing == "Idle" && form.Queued == SunBearerAttack.None && next < run.Length) form.Queued = run[next++];
            mostHot = Mathf.Max(mostHot, form.HotPatches);
            mostLanes = Mathf.Max(mostLanes, form.HotLanes);
            frames++;
            bytes += garbage.LastValue;
            float ms = Time.unscaledDeltaTime * 1000f;
            total += ms;
            worst = Mathf.Max(worst, ms);
            yield return null;
        }
        follow = null;
        garbage.Dispose();
        Say($"Perf: {frames} frames, frame ms mean {total / Mathf.Max(1, frames):0.0} worst {worst:0.0}; garbage {bytes / Mathf.Max(1, frames)} bytes a frame; most hot patches at once {mostHot} (lanes {mostLanes}); attacks asked {next}; screen {Screen.width}x{Screen.height}");
    }
}
