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

// Play Mode probe for the sun-king's city, second pass (docs/design/fire_levels.md): frames at every room and vista
// (review/shots/current/suncity2_<place>_<n>.png), every stair and ramp walked with the keys (end heights), the four
// shortcuts tried from their near side (must stay shut) and their far side (must open), the mirror turned onto the sun
// door, the cracked wall, the swell in the open and in the planned shade and in the calm bowl, the districts' rhythms,
// each rest fire lit, falls that return to the right fire with the dead carrying the embers, the champion and the gate.
// Lines that can be judged end OK or WRONG. Run with
//   bash AgentScripts/runprobe.sh SunCity SunCityProbe 160
public static class SunCityProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Sun City Probe").AddComponent<SunCityProbeRunner>();
        return "probe started";
    }
}

public class SunCityProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private AreaDirector director;
    private LandReturn land;
    private LandZones zones;
    private bool keepAlive = true;
    private int oks, wrongs;

    private void Update()
    {
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private string Judge(bool good)
    {
        if (good) oks++; else wrongs++;
        return good ? "OK" : "WRONG";
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
        yield return null;
        yield return null;
    }

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/suncity2_" + name + ".png");
        yield return null;
        yield return null;
    }

    // Reads whatever the voice has put in the reading box to its end, so the buttons are the player's again.
    private IEnumerator Hear(string when, float wait = 0.5f)
    {
        yield return new WaitForSeconds(wait);
        var box = ReadingBox.Instance;
        if (box == null || !box.IsOpen) yield break;
        int before = box.LinesRead;
        for (int i = 0; i < 24 && box.IsOpen; i++)
        {
            yield return Tap(Key.E);
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.1f);
        Say($"   the voice {when}: {box.LinesRead - before} line(s), box closed {!box.IsOpen} player free {!player.Suspended}");
    }

    private void Go(float x, float y, float z) => player.Teleport(new Vector3(x, y + 0.1f, z), Quaternion.identity);

    private int Aware() => director.Enemies.Count(enemy => enemy != null && !enemy.IsDead && enemy.IsAware);

    private void Calm()
    {
        foreach (var enemy in director.Enemies)
            if (enemy != null && !enemy.IsDead) enemy.Restore();
    }

    // Puts the player at a place, everyone calmed, and takes a frame once the camera has settled.
    private IEnumerator Look(string name, float x, float y, float z, float wait = 1.1f)
    {
        Calm();
        Go(x, y, z);
        yield return new WaitForSeconds(wait);
        yield return Shot(name);
    }

    // Holds a key from a place and says where the player ended: proves a stair or ramp can be walked.
    private IEnumerator Walk(string what, Vector3 from, Key key, float seconds, float wantedHeight, float wantedZ = float.NaN)
    {
        Calm();
        Go(from.x, from.y, from.z);
        yield return new WaitForSeconds(0.25f);
        Keys(key);
        float end = Time.time + seconds;
        while (Time.time < end)
        {
            Calm();
            yield return null;
        }
        Keys();
        yield return new WaitForSeconds(0.2f);
        Vector3 at = player.transform.position;
        bool good = Mathf.Abs(at.y - wantedHeight) < 0.35f && (float.IsNaN(wantedZ) || Mathf.Abs(at.z - wantedZ) < 2.5f);
        Say($"   walk {what}: from {from} ended {at:0.0} wanted height {wantedHeight:0.0}{(float.IsNaN(wantedZ) ? "" : $" near z {wantedZ:0}")} {Judge(good)}");
    }

    private IEnumerator Use(Vector3 at)
    {
        Calm();
        player.Teleport(at + Vector3.up * 0.1f, Quaternion.identity);
        yield return new WaitForSeconds(0.35f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator Read(string what, Interactable thing)
    {
        Calm();
        Vector3 at = thing.transform.position;
        player.Teleport(new Vector3(at.x, at.y + 0.1f, at.z) - thing.transform.forward * 1.2f, Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        var box = ReadingBox.Instance;
        int before = box != null ? box.LinesRead : 0;
        yield return Tap(Key.E);
        bool opened = box != null && box.IsOpen;
        for (int i = 0; i < 16 && box != null && box.IsOpen; i++)
        {
            yield return Tap(Key.E);
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.1f);
        int lines = thing is Notice notice ? notice.Lines.Length : 0;
        Say($"   read {what}: box opened {opened} closed again {box != null && !box.IsOpen} lines read {(box != null ? box.LinesRead - before : 0)} of {lines} player free {!player.Suspended} {Judge(opened && box != null && !box.IsOpen && box.LinesRead - before == lines)}");
    }

    private IEnumerator Fall(string what, Vector3 wantedFire)
    {
        keepAlive = false;
        int deaths = defence.Deaths;
        defence.TakeHit(new Hit(9999f, 0f, Vector3.forward, false, true));
        yield return Until(() => defence.IsDead, 1f);
        Vector3 fell = player.transform.position;
        yield return Until(() => !defence.IsDead, 15f);
        yield return new WaitForSeconds(0.3f);
        keepAlive = true;
        Vector3 at = player.transform.position;
        yield return Hear("after the fall", 1.6f);
        Say($"   fall {what}: deaths +{defence.Deaths - deaths} fell at {fell:0.0} woke at {at:0.0}, the fire wanted is at {wantedFire:0.0} {Judge((at - wantedFire).magnitude < 3f)}");
    }

    private IEnumerator Light(RestPoint fire)
    {
        Calm();
        player.Teleport(fire.transform.position + new Vector3(0f, 0.1f, -1.5f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        yield return Hear("at a fire");
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        var element = player.GetComponent<PlayerElement>();
        director = FindAnyObjectByType<AreaDirector>();
        land = FindAnyObjectByType<LandReturn>();
        zones = FindAnyObjectByType<LandZones>();
        var heat = NoonHeat.Instance;
        yield return Hear("on waking", 3.2f);
        var fires = FindObjectsByType<RestPoint>(FindObjectsSortMode.None).OrderBy(fire => fire.transform.position.z).ToArray();
        var notices = FindObjectsByType<Notice>(FindObjectsSortMode.None).OrderBy(notice => notice.transform.position.z).ToArray();
        Say($"start: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} weapon {combat.Weapon.Name} has element {element.HasElement} enemies {director.EnemiesAlive} of {director.Enemies.Length} fires {fires.Length} things to read {notices.Length} sunlight rectangles {Sunlight.Rectangles} open sky {Sunlight.OpenSky} sandboxed {Progress.Sandboxed}");
        foreach (var kind in director.Enemies.GroupBy(enemy => enemy.Kind.Name)) Say($"   {kind.Count()} x {kind.Key}");
        Say($"   buried {FindObjectsByType<BuriedMound>(FindObjectsSortMode.None).Length} water jars {FindObjectsByType<WaterJar>(FindObjectsSortMode.None).Length} mirrors {FindObjectsByType<MirrorStand>(FindObjectsSortMode.None).Length} ladders {FindObjectsByType<Ladder>(FindObjectsSortMode.None).Length} barred doors {FindObjectsByType<BarredDoor>(FindObjectsSortMode.None).Length} shield walls {FindObjectsByType<ShieldWall>(FindObjectsSortMode.None).Length}");

        // 1: the walk, room by room and vista by vista, with frames.
        string only = System.IO.File.Exists("AgentScripts/suncity_only.txt") ? System.IO.File.ReadAllText("AgentScripts/suncity_only.txt").Trim() : "";
        var places = new (string name, float x, float y, float z, float wait)[]
        {
            ("narrows_0", 0f, 0f, 4f, 1.4f), ("vista_v1", 0f, 0f, 15f, 2.2f), ("pans_0", -2f, 0f, 22f, 1.1f), ("pans_1", -8f, 0f, 37f, 1.1f),
            ("jam_0", -4f, 0f, 61f, 1.1f), ("jam_1", -4f, 0f, 75f, 1.1f), ("bend_0", -5f, 0f, 80f, 1.1f), ("bend_1", -4f, 0f, 93f, 1.1f),
            ("deck_0", 3.4f, 3.5f, 84f, 1.1f), ("basin_0", 0f, 0f, 101f, 1.1f), ("basin_1", -18f, 0f, 111f, 1.1f), ("vista_v2", 0f, 4f, 118f, 2.2f),
            ("quay_0", -20f, 4f, 123f, 1.1f), ("bank_0", -28f, 3f, 40f, 1.1f), ("bank_1", -30f, 3f, 77f, 1.1f), ("well_0", -52f, 1f, 75f, 1.1f),
            ("bank_2", -8f, 3f, 13f, 1.1f), ("lamp_0", 0.5f, 4f, 138f, 1.1f), ("hearth_0", -6f, 4f, 141f, 1.1f), ("lamp_1", 4f, 6f, 155f, 1.1f),
            ("lamp_2", 11.5f, 6f, 165f, 1.1f), ("roofs_0", 20f, 10f, 180f, 1.1f), ("roofs_1", 0f, 10f, 192f, 1.1f), ("alley_0", 10f, 6f, 178f, 1.1f),
            ("alley_1", -4f, 6f, 178f, 1.1f), ("market_0", -14f, 6f, 184f, 1.1f), ("market_1", -20f, 6f, 196f, 1.1f), ("market_2", -10f, 8f, 207f, 1.1f),
            ("market_3", 0f, 8f, 199f, 1.1f), ("gate_0", 0f, 8f, 219f, 1.1f), ("yard_0", 0f, 8f, 230f, 1.1f), ("yard_1", 0f, 8f, 250f, 1.1f),
            ("yard_2", 0f, 10f, 267f, 1.1f), ("tombs_0", -21f, 8f, 250f, 1.1f), ("tombs_1", -40f, 4f, 249f, 1.1f), ("tombs_2", -52f, 4f, 249f, 1.1f),
            ("avenue_0", 0f, 10f, 278f, 1.1f), ("avenue_1", 0f, 10f, 296f, 1.1f), ("haul_0", -28f, 11.5f, 320f, 1.1f), ("vista_v3", 0f, 10f, 320f, 2.2f),
            ("court_0", -14f, 10f, 334f, 1.1f), ("vista_v4", 0f, 13f, 348f, 2.2f), ("masons_0", -4f, 13f, 360f, 1.1f), ("masons_1", 6f, 13f, 377f, 1.1f),
            ("scaffold_0", -8.5f, 16f, 366f, 1.1f), ("stair_0", 0f, 14.5f, 383f, 1.1f), ("warden_0", 0f, 16f, 390f, 1.1f), ("warden_1", 12f, 16f, 398f, 1.1f),
            ("hall_0", 0f, 16f, 421f, 1.1f), ("hall_1", -16f, 17.2f, 440f, 1.1f), ("hall_2", -6f, 16f, 450f, 1.1f), ("hall_3", 16f, 17.2f, 462f, 1.1f),
            ("chapel_0", -27f, 17.2f, 448f, 1.1f), ("passage_0", -30f, 16.4f, 410f, 1.1f), ("vestibule_0", -28f, 13f, 381f, 1.1f),
            ("vista_v5", 0f, 16f, 486f, 2.2f), ("bowl_0", 0f, 14.5f, 494f, 1.1f), ("bowl_1", -12f, 14.5f, 505f, 1.1f),
        };
        foreach (var place in places)
        {
            if (only.Length > 0 && !only.Contains("walk")) break;
            yield return Look(place.name, place.x, place.y, place.z, place.wait);
            if (place.name.StartsWith("vista")) Say($"   {place.name}: in vista {zones.InVista} or noticed by {Aware()} {Judge(zones.InVista >= 0 || Aware() > 0)}");
        }
        Say($"1 the walk: {places.Length} places pictured");
        if (only == "walk") { Say("done"); yield break; }

        // 2: every stair and ramp walked with the keys.
        yield return Walk("quay stair up", new Vector3(0f, 0f, 106.5f), Key.W, 2.2f, 4f);
        yield return Walk("bank ramp up from the basin", new Vector3(-23.5f, 0f, 113.6f), Key.S, 3f, 3f);
        yield return Walk("gangplank onto the deck", new Vector3(0.1f, 0f, 71f), Key.W, 1.4f, 3.5f);
        yield return Walk("gully down to the well", new Vector3(-38.5f, 3f, 77f), Key.A, 1.5f, 1f);
        yield return Walk("lamp street stair", new Vector3(0.5f, 4f, 146f), Key.W, 1.2f, 6f);
        yield return Walk("market stair", new Vector3(-8f, 6f, 197.6f), Key.W, 1.3f, 8f);
        yield return Walk("cart ramp", new Vector3(-32f, 6f, 190f), Key.W, 2.6f, 8f);
        yield return Walk("gallery stair", new Vector3(-18f, 8f, 256.4f), Key.W, 1.2f, 10f);
        yield return Walk("tomb stair down", new Vector3(-20.5f, 8f, 250f), Key.A, 2f, 4f);
        yield return Walk("tomb stair up", new Vector3(-31.5f, 4f, 250f), Key.D, 2f, 8f);
        yield return Walk("yard gate to the avenue", new Vector3(0f, 10f, 266f), Key.W, 1.6f, 10f);
        yield return Walk("levy ramp", new Vector3(0f, 10f, 316.5f), Key.W, 5.4f, 13f);
        yield return Walk("colonnade side stair", new Vector3(-21f, 10f, 333.8f), Key.W, 1.4f, 13f);
        yield return Walk("haul-ramp up to the masons' gate", new Vector3(-28f, 10f, 287f), Key.W, 10.5f, 13f);
        yield return Walk("spearman's stair", new Vector3(0f, 13f, 378.4f), Key.W, 1.6f, 16f);
        yield return Walk("pylon into the hall", new Vector3(0f, 16f, 408f), Key.W, 2.4f, 16f, 422f);
        yield return Walk("aisle steps", new Vector3(-9f, 16f, 468f), Key.A, 1f, 17.2f);
        yield return Walk("bowl steps down", new Vector3(0f, 16f, 484.6f), Key.W, 1.1f, 14.5f);

        // 3: the ladders, the shortcuts from their near side and their far side, the cracked wall.
        var ladders = FindObjectsByType<Ladder>(FindObjectsSortMode.None);
        var s1 = ladders.OrderBy(l => (l.Foot - new Vector3(-4.1f, 0f, 13f)).sqrMagnitude).First();
        yield return Use(new Vector3(-4.2f, 0f, 13f));
        Say($"3 S1 ladder from below while it lies: down {s1.IsDown} climbs {s1.Climbs} height {player.transform.position.y:0.0} says '{defence.LastCallout}' {Judge(!s1.IsDown && player.transform.position.y < 0.5f)}");
        yield return Use(new Vector3(-6.6f, 3f, 13f));
        Say($"   S1 kicked from the top: down {s1.IsDown} {Judge(s1.IsDown)}");
        yield return Tap(Key.E);
        yield return new WaitForSeconds(1.6f);
        Say($"   S1 climbed down: at {player.transform.position:0.0} {Judge(player.transform.position.y < 0.5f && Mathf.Abs(player.transform.position.x + 4f) < 1.5f)}");
        yield return Tap(Key.E);
        yield return new WaitForSeconds(1.6f);
        Say($"   S1 climbed up: at {player.transform.position:0.0} {Judge(Mathf.Abs(player.transform.position.y - 3f) < 0.4f)}");
        yield return Shot("ladder_s1_0");
        var roofLadder = ladders.OrderBy(l => (l.Foot - new Vector3(13.4f, 6f, 163f)).sqrMagnitude).First();
        yield return Use(new Vector3(13.1f, 6f, 163f));
        yield return new WaitForSeconds(1.6f);
        Say($"   roof ladder up: at {player.transform.position:0.0} {Judge(Mathf.Abs(player.transform.position.y - 10f) < 0.4f)}");
        var scaffold = ladders.OrderBy(l => (l.Foot - new Vector3(-6.3f, 13f, 358f)).sqrMagnitude).First();
        yield return Use(new Vector3(-6f, 13f, 358f));
        yield return new WaitForSeconds(1.6f);
        Say($"   scaffold ladder up: at {player.transform.position:0.0} {Judge(Mathf.Abs(player.transform.position.y - 16f) < 0.4f)}");

        var doors = FindObjectsByType<BarredDoor>(FindObjectsSortMode.None);
        foreach (var (name, door, near, far, through, key, wantedZ, wantedY) in new[]
        {
            ("S2 market door", doors.OrderBy(d => (d.transform.position - new Vector3(-28.5f, 6f, 176f)).sqrMagnitude).First(), new Vector3(-28.5f, 5.4f, 174.4f), new Vector3(-28.5f, 6f, 177.7f), Key.S, "south", 160f, 4f),
            ("S3 masons' gate", doors.OrderBy(d => (d.transform.position - new Vector3(-24f, 13f, 350f)).sqrMagnitude).First(), new Vector3(-25.6f, 13f, 350f), new Vector3(-22.4f, 13f, 350f), Key.A, "west", 349f, 13f),
            ("S4 vestibule door", doors.OrderBy(d => (d.transform.position - new Vector3(-24f, 13f, 381f)).sqrMagnitude).First(), new Vector3(-22.4f, 13f, 381f), new Vector3(-25.6f, 13f, 381f), Key.D, "east", 381f, 13f),
        })
        {
            yield return Use(near);
            bool shutFromNear = !door.IsOpen;
            string said = defence.LastCallout;
            yield return Use(far);
            Say($"3 {name}: from the near side open {!shutFromNear} says '{said}'; from the far side open {door.IsOpen} {Judge(shutFromNear && door.IsOpen)}");
            yield return Shot("shortcut_" + name.Substring(0, 2).ToLowerInvariant());
            Vector3 from = player.transform.position;
            Keys(through);
            yield return new WaitForSeconds(name.StartsWith("S2") ? 2.4f : 1.2f);
            Keys();
            yield return new WaitForSeconds(0.2f);
            Vector3 at = player.transform.position;
            bool passed = name.StartsWith("S2") ? at.z < 170f : name.StartsWith("S3") ? at.x < -25.5f : at.x > -22.5f;
            Say($"   walked {key} through it: from {from:0.0} to {at:0.0} {Judge(passed)}");
        }

        var wall = FindAnyObjectByType<BreakableWall>();
        wall.TakeHit(new Hit(12f, 0f, Vector3.left, false));
        bool afterLight = wall.Broken;
        wall.TakeHit(new Hit(30f, 0f, Vector3.left, true, false, wall.transform.position + Vector3.right));
        Say($"3 the cracked wall: a light blow broke it {afterLight}, a heavy one broke it {wall.Broken} {Judge(!afterLight && wall.Broken)}");
        yield return Look("hearth_1", 0.5f, 4f, 141f);

        // 4: the mirror turned until it throws the sun on the disc over the sun door.
        var m1 = FindObjectsByType<MirrorStand>(FindObjectsSortMode.None).OrderBy(m => (m.transform.position - new Vector3(-8f, 16f, 449.6f)).sqrMagnitude).First();
        var target = FindAnyObjectByType<MirrorTarget>();
        var sunDoor = GameObject.Find("Sun Door");
        Calm();
        Go(-8f, 16f, 447.6f);
        yield return new WaitForSeconds(0.4f);
        // Turned with the use button, a step at a time, until it faces due west (six steps from north).
        int tapped = 0;
        for (int i = 0; i < 14 && m1.Turns < 6; i++)
        {
            Calm();
            Go(-8f, 16f, 447.6f);
            yield return new WaitForSeconds(0.15f);
            yield return Tap(Key.E);
            tapped++;
            yield return new WaitForSeconds(0.6f);
        }
        Say($"   M1 turned by the use button {m1.Turns} time(s) in {tapped} presses");
        yield return new WaitForSeconds(2f);
        yield return Shot("mirror_0");
        Say($"4 mirror M1: turns {m1.Turns} lighting {(m1.Lighting != null ? m1.Lighting.name : "nothing")} beam ends at {m1.BeamEnd:0.0} target lit {target.IsLit} sun door shut {(sunDoor != null && sunDoor.activeInHierarchy)} {Judge(target.IsLit && (sunDoor == null || !sunDoor.activeInHierarchy))}");
        yield return Walk("through the sun door into the chapel", new Vector3(-20.5f, 17.2f, 448f), Key.A, 1f, 17.2f);
        Say($"   in the chapel at x {player.transform.position.x:0.0} {Judge(player.transform.position.x < -23f)}");

        // 5: the noon: the districts' rhythms, the swell in the open, in the shade and in the calm bowl, the forced first swell.
        foreach (var (where, at, every) in new[] { ("D1", new Vector3(0f, 0f, 20f), 100f), ("D2", new Vector3(0.5f, 4f, 140f), 70f), ("D3", new Vector3(0f, 10f, 290f), 50f), ("hall", new Vector3(0f, 16f, 440f), 0f) })
        {
            Calm();
            Go(at.x, at.y, at.z);
            yield return new WaitForSeconds(0.3f);
            Say($"5 {where}: district {zones.District} every {heat.Schedule.Every:0} swells {heat.Schedule.Swells} {Judge(every > 0f ? Mathf.Approximately(heat.Schedule.Every, every) && heat.Schedule.Swells : !heat.Schedule.Swells)}");
        }
        foreach (var (where, at, burns) in new[]
        {
            ("in the open in the Pans", new Vector3(0f, 0f, 34f), true), ("under the boat on trestles", new Vector3(-13.5f, 0f, 40.5f), false),
            ("in a boat-shed", new Vector3(-8f, 0f, 118f), false), ("in the open on the avenue", new Vector3(0f, 10f, 292f), true),
            ("under the court's colonnade", new Vector3(-14f, 10f, 344f), false), ("in the champion's bowl", new Vector3(-14f, 14.5f, 494f), false),
        })
        {
            Calm();
            Go(at.x, at.y, at.z);
            yield return new WaitForSeconds(0.3f);
            heat.SetSchedule(new HeatSchedule(heat.Clock + 0.1f, 200f));
            yield return new WaitForSeconds(0.5f);
            int before = heat.Burns;
            yield return new WaitForSeconds(2f);
            if (where.Contains("open in the Pans")) yield return Shot("swell_0");
            int bit = heat.Burns - before;
            Say($"5 swell {where}: lit {Sunlight.IsLit(player.transform.position)} burnt {bit} times in 2 s {Judge(burns ? bit > 0 : bit == 0)}");
            heat.SetSchedule(HeatSchedule.Never);
            yield return new WaitForSeconds(0.2f);
        }
        int forcedBefore = zones.Forced;
        Calm();
        Go(-4f, 0f, 29f);
        yield return new WaitForSeconds(0.2f);
        Keys(Key.W);
        yield return new WaitForSeconds(0.8f);
        Keys();
        yield return new WaitForSeconds(0.3f);
        Say($"5 forced swell at z 31: forced {zones.Forced - forcedBefore} phase {heat.Current} {Judge(zones.Forced - forcedBefore == 1 || forcedBefore > 0)}");
        heat.SetSchedule(HeatSchedule.Never);

        // 6: lore read, things taken.
        foreach (var notice in notices.Where(notice => !(notice is WakingNotice))) yield return Read(notice.Speaker + " @z" + notice.transform.position.z.ToString("0"), notice);
        var last = notices.OfType<WakingNotice>().First();
        int tombWicks = director.Enemies.Count(enemy => enemy.transform.position.y < 6f && enemy.transform.position.z > 240f && enemy.transform.position.z < 256f && enemy.IsDormant);
        yield return Read("the last painting", last);
        yield return new WaitForSeconds(1.6f);
        Say($"6 last painting: woken {last.Woken} dormant in the tombs before {tombWicks} aware after {Aware()} says '{defence.LastCallout}'");
        Calm();
        foreach (var gift in FindObjectsByType<LandGift>(FindObjectsSortMode.None))
        {
            int flasks = defence.Flasks.Maximum, points = RunState.Points;
            yield return Use(gift.transform.position);
            Say($"6 {gift.name}: taken {gift.Taken} flasks {flasks} -> {defence.Flasks.Maximum} embers {points} -> {RunState.Points} {Judge(gift.Taken)}");
        }
        var pickup = FindAnyObjectByType<WeaponPickup>();
        if (pickup != null)
        {
            yield return Use(pickup.transform.position);
            Say($"6 spear: taken {pickup.Taken} weapon now {combat.Weapon.Name}");
        }

        // 7: the rest fires, and falls that return to the right one with the dead carrying the embers.
        for (int i = 0; i < fires.Length; i++)
        {
            yield return Light(fires[i]);
            Say($"7 fire {i + 1} at {fires[i].transform.position:0.0}: lit {fires[i].IsLit} return point {director.ReturnPoint.position:0.0} {Judge(fires[i].IsLit && (director.ReturnPoint.position - fires[i].transform.position).magnitude < 2.5f)}");
            if (i == 0) yield return Shot("fire_1");
        }
        int total = director.Enemies.Length;
        yield return Light(fires[1]);
        RunState.Points += 9;
        int carried = RunState.Points;
        var basin = director.Enemies.Where(enemy => enemy.transform.position.z > 98f && enemy.transform.position.z < 116f && enemy.transform.position.y < 1f).Take(2).ToArray();
        foreach (var enemy in basin) enemy.TakeHit(new Hit(9999f, 0f, Vector3.forward, true));
        yield return new WaitForSeconds(0.6f);
        int before2 = RunState.Points;
        Go(0f, 0f, 60f);
        yield return new WaitForSeconds(0.3f);
        yield return Fall("in the riverbed, last fire the quay", director.ReturnPoint.position);
        int carriers = Enumerable.Range(0, total).Count(i => land.Debt.Carries(i));
        Say($"7 after the fall: alive {director.EnemiesAlive} of {total} embers carried {before2} -> {RunState.Points}, waiting with the dead {land.Debt.Waiting} on {carriers} carrier(s) {Judge(RunState.Points == 0 && land.Debt.Waiting == before2 && carriers == basin.Length)}");
        foreach (var enemy in basin)
        {
            enemy.TakeHit(new Hit(9999f, 0f, Vector3.forward, true));
            yield return new WaitForSeconds(0.9f);
        }
        Say($"   the carriers killed again: embers now {RunState.Points}, reclaimed {land.Reclaimed} {Judge(land.Debt.Waiting == 0 && land.Reclaimed >= before2 - 1)}");
        yield return Light(fires[3]);
        Go(0f, 13f, 360f);
        yield return new WaitForSeconds(0.3f);
        yield return Fall("on the masons' terrace, last fire the avenue", fires[3].transform.position + new Vector3(0f, 0f, -1.6f));

        // 8: the champion in his bowl.
        var champion = director.Enemies.First(enemy => enemy.Kind.Name == EnemyKind.ChampionName);
        var gate = FindAnyObjectByType<GateOfNoon>();
        yield return Use(gate.transform.position + new Vector3(0f, 0f, -1.4f));
        Say($"8 gate while he stands: tries {gate.Tries} opened {gate.Opened} says '{defence.LastCallout}' {Judge(gate.Opened == 0)}");
        Calm();
        Go(0f, 16f, 485f);
        yield return new WaitForSeconds(0.4f);
        Say($"8 champion: health {champion.Health.Current:0} aware from the porch {champion.IsAware} {Judge(!champion.IsAware)}");
        yield return Shot("champion_0");
        string[] moves = { "champion_thrust_1", "champion_sweep", "champion_throw", "champion_plant" };
        for (int i = 0; i < moves.Length; i++)
        {
            champion.Restore();
            yield return null;
            float distance = moves[i].Contains("throw") ? 8f : moves[i] == "champion_plant" ? 2.2f : 3.2f;
            player.Teleport(champion.transform.position + champion.transform.forward * distance + Vector3.up * 0.1f, Quaternion.LookRotation(-champion.transform.forward));
            yield return null;
            int taken = defence.BlowsTaken;
            champion.Brain.Alert();
            bool forced = champion.Brain.Force(moves[i]);
            var attack = champion.Brain.Current;
            yield return Until(() => champion.Brain.TelegraphProgress > 0.85f, 3f);
            yield return Shot("champion_" + (i + 1));
            yield return Until(() => champion.Brain.State == EnemyState.Recover || champion.Brain.Current != attack, 4f);
            yield return new WaitForSeconds(0.9f);
            Say($"   {moves[i]} at {distance:0.0} m: forced {forced} blows landed on a still player +{defence.BlowsTaken - taken} {Judge(forced && defence.BlowsTaken > taken)}");
        }
        champion.Restore();
        yield return null;
        int blows = 0;
        while (!champion.IsDead && blows < 120)
        {
            champion.TakeHit(new Hit(30f, 0f, champion.transform.forward, false, false, champion.transform.position - champion.transform.forward));
            blows++;
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1.2f);
        Say($"8 champion beaten: dead {champion.IsDead} {Judge(champion.IsDead)}");
        yield return Walk("bowl's north stair to the gate", new Vector3(0f, 14.5f, 514.4f), Key.W, 1.2f, 16f);
        yield return Light(fires[fires.Length - 1]);
        Go(0f, 14.5f, 500f);
        yield return Fall("in the bowl, last fire the porch", fires[fires.Length - 1].transform.position + new Vector3(0f, 0f, -1.6f));
        Say($"8 after a fall: champion dead {champion.IsDead} alive {director.EnemiesAlive} of {total} {Judge(champion.IsDead)}");

        Say($"tally before the gate: {oks} OK, {wrongs} WRONG");
        player.Teleport(gate.transform.position + new Vector3(0f, 0.1f, -1.4f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        // In the editor the gate leads on to the sun-court (which is not in the build list): this probe goes with the player.
        DontDestroyOnLoad(gameObject);
        yield return Shot("gate_0");
        yield return Tap(Key.E);
        int opened = gate.Opened;
        yield return new WaitForSecondsRealtime(2f);
        Say($"8 gate after: opened {opened} now in {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} {Judge(opened == 1)}");
        Say($"tally: {oks} OK, {wrongs} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
