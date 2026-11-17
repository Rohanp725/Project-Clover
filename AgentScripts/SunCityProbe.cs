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

// Play Mode probe for the sun-king's city: a scripted walk through every district and both side paths with frames
// (review/shots/current/suncity_<district>_<n>.png), the stairs walked with the keys, each rest fire lit, the carvings
// and paintings read, the waterskin, the spear and the grave dish taken, a fall that proves the return rule
// (position, enemies back, embers lost), the champion's attacks and his death, a second fall and a rest that prove he
// stays dead, and the gate of noon sealed and open. Run with
//   bash AgentScripts/runprobe.sh SunCity SunCityProbe 120
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
    private bool keepAlive = true;

    private void Update()
    {
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
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
        yield return null;
        yield return null;
    }

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/suncity_" + name + ".png");
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

    private int Aware() => director.Enemies.Count(enemy => enemy != null && enemy.IsAware);

    private void Calm()
    {
        foreach (var enemy in director.Enemies)
            if (enemy != null && !enemy.IsDead) enemy.Restore();
    }

    private IEnumerator Look(string name, float x, float y, float z, float wait = 0.9f)
    {
        Go(x, y, z);
        yield return new WaitForSeconds(wait);
        yield return Shot(name);
    }

    // Holds a key from a place and says where the player ended: proves a stair can be walked.
    private IEnumerator Walk(string what, Vector3 from, Key key, float seconds, float wantedHeight)
    {
        Calm();
        Go(from.x, from.y, from.z);
        yield return new WaitForSeconds(0.2f);
        Keys(key);
        yield return new WaitForSeconds(seconds);
        Keys();
        yield return null;
        Vector3 at = player.transform.position;
        Say($"   walk {what}: from {from} ended {at:0.0} wanted height {wantedHeight:0.0} {(Mathf.Abs(at.y - wantedHeight) < 0.3f ? "OK" : "WRONG")}");
    }

    private IEnumerator Read(string what, Interactable thing)
    {
        Calm();
        Vector3 at = thing.transform.position;
        player.Teleport(new Vector3(at.x, at.y + 0.1f, at.z) + (thing is Notice && at.y < 0f ? new Vector3(0f, 0.9f, -1.3f) : -thing.transform.forward * 1.2f), Quaternion.identity);
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
        Say($"   read {what}: box opened {opened} closed again {box != null && !box.IsOpen} lines read {(box != null ? box.LinesRead - before : 0)} of {(thing is Notice notice ? notice.Lines.Length : 0)} player free {!player.Suspended}");
    }

    private IEnumerator Fall(string what)
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
        Vector3 wanted = director.ReturnPoint != null ? director.ReturnPoint.position : Vector3.zero;
        Say($"   fall {what}: deaths +{defence.Deaths - deaths} fell at {fell:0.0} woke at {at:0.0} return point {wanted:0.0} {((at - wanted).magnitude < 1.5f ? "AT THE FIRE" : "ELSEWHERE")} health {defence.Health.Current:0} says '{defence.LastCallout}'");
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        var element = player.GetComponent<PlayerElement>();
        director = FindAnyObjectByType<AreaDirector>();
        land = FindAnyObjectByType<LandReturn>();
        yield return Hear("on waking", 3.2f);
        var fires = FindObjectsByType<RestPoint>(FindObjectsSortMode.None).OrderBy(fire => fire.transform.position.z).ToArray();
        var notices = FindObjectsByType<Notice>(FindObjectsSortMode.None).OrderBy(notice => notice.transform.position.z).ToArray();
        Say($"start: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} weapon {combat.Weapon.Name} has element {element.HasElement} enemies {director.EnemiesAlive} of {director.Enemies.Length} fires {fires.Length} things to read {notices.Length} asleep by distance {land.Asleep} sunlight rectangles {Sunlight.Rectangles} open sky {Sunlight.OpenSky} sandboxed {Progress.Sandboxed}");
        foreach (var kind in director.Enemies.GroupBy(enemy => enemy.Kind.Name)) Say($"   {kind.Count()} x {kind.Key}");

        // 1: the walk, district by district, with frames. The people near each place wake as the player arrives.
        yield return Look("river_0", 0f, 0f, 4f, 1.5f);
        yield return Look("river_1", -1f, 0f, 22f);
        yield return Look("river_2", 0f, 0f, 58f);
        yield return Look("river_3", -4f, 0f, 84f);
        yield return Look("river_4", 0f, 0f, 103f);
        yield return Look("river_7", -3f, 0f, 70f);
        yield return Look("river_8", -1f, 0f, 94f);
        yield return Look("river_5", 0f, 2f, 129f);
        Say($"1 dry river: aware after the walk {Aware()}");
        Calm();
        yield return Look("well_0", -17f, 1.5f, 81f);
        yield return Look("well_1", -41f, 1.5f, 75.5f);
        yield return Look("well_2", -41f, 1.5f, 80.6f);
        Say($"1 well shrine: aware {Aware()}");
        Calm();
        yield return Look("wicks_0", 2.5f, 2f, 145f);
        yield return Look("wicks_1", 2.5f, 2f, 164f);
        yield return Look("wicks_2", 9f, 2f, 182.5f);
        yield return Look("wicks_3", 22.5f, 2f, 197f);
        yield return Look("wicks_4", 2f, 2f, 220f);
        yield return Look("wicks_5", 0f, 2f, 239f);
        yield return Look("wicks_6", 0f, 2f, 252f);
        yield return Look("wicks_7", 0f, 2f, 272f, 1.6f);
        Say($"1 city of wicks: aware after the walk {Aware()}");
        Calm();
        yield return Look("tombs_0", -31f, -1f, 262f);
        yield return Look("tombs_1", -41f, -1f, 261.5f);
        yield return Look("tombs_2", -51f, -1f, 261.5f);
        Say($"1 painted tombs: aware {Aware()} (the two here sleep until the last painting is read)");
        Calm();
        yield return Look("golden_0", 0f, 2f, 297f);
        yield return Look("golden_1", 0f, 2f, 318f);
        yield return Look("golden_5", -6.5f, 2f, 301f);
        yield return Look("golden_6", -5.5f, 3f, 345f);
        yield return Look("golden_2", -2f, 3f, 345f);
        yield return Look("golden_3", 0f, 3f, 366f);
        yield return Look("golden_4", 0f, 4f, 389f);
        Say($"1 golden way: aware after the walk {Aware()}");
        Calm();
        yield return Look("hall_0", 0f, 4f, 402f);
        Vector3 shadeSpot = new Vector3(-10f, 4.1f, 420f);
        Go(shadeSpot.x, 4f, shadeSpot.z);
        yield return new WaitForSeconds(2f);
        int foundInShade = Aware();
        yield return Shot("hall_1");
        Go(-10f, 4f, 430f);
        yield return new WaitForSeconds(2f);
        int foundInLight = Aware();
        yield return Shot("hall_2");
        Say($"1 hall: at (-10, 420) lit {Sunlight.IsLit(shadeSpot)} found by {foundInShade}; at (-10, 430) lit {Sunlight.IsLit(new Vector3(-10f, 4f, 430f))} found by {foundInLight}");
        yield return Look("hall_3", 2f, 4f, 446f);
        yield return Look("hall_4", 0f, 4f, 465f);
        Calm();
        yield return Look("gate_0", 0f, 4f, 468.5f);
        Say($"1 last gate, south of the gold line: aware {Aware()} (the champion notices nobody there)");

        // 2: the stairs, walked with the keys.
        yield return Walk("quay stair north", new Vector3(0f, 0f, 112.5f), Key.W, 2f, 2f);
        yield return Walk("gully west to the well", new Vector3(-12.5f, 0f, 81f), Key.A, 2.2f, 1.5f);
        yield return Walk("tomb stair down", new Vector3(-18.5f, 2f, 262f), Key.A, 2.2f, -1f);
        yield return Walk("tomb stair up", new Vector3(-27.5f, -1f, 262f), Key.D, 2.2f, 2f);
        yield return Walk("avenue first stair", new Vector3(0f, 2f, 338.5f), Key.W, 1.2f, 3f);
        yield return Walk("avenue second stair", new Vector3(0f, 3f, 382.5f), Key.W, 1.2f, 4f);
        yield return Walk("through the pylon", new Vector3(0f, 4f, 390f), Key.W, 2.4f, 4f);
        Say($"   after the pylon the player is at z {player.transform.position.z:0.0} (past 400 is through)");
        yield return Walk("street of lamps", new Vector3(2.5f, 2f, 136f), Key.W, 3f, 2f);
        Say($"   up the street the player is at z {player.transform.position.z:0.0} (past 150 is in the street)");

        // 3: lore, read with the use button, and the things taken.
        foreach (var notice in notices.Where(notice => !(notice is WakingNotice))) yield return Read(notice.Speaker + " @z" + notice.transform.position.z.ToString("0"), notice);
        var last = notices.OfType<WakingNotice>().First();
        int tombWicks = director.Enemies.Count(enemy => enemy.transform.position.y < 0f && enemy.IsDormant);
        yield return Read("the last painting", last);
        yield return new WaitForSeconds(1.6f);
        Say($"3 last painting: woken {last.Woken} dormant in the tombs before {tombWicks} after {director.Enemies.Count(enemy => enemy.transform.position.y < 0f && enemy.IsDormant)} aware {Aware()} says '{defence.LastCallout}'");
        yield return Shot("tombs_3");
        Calm();
        foreach (var gift in FindObjectsByType<LandGift>(FindObjectsSortMode.None))
        {
            Calm();
            int flasks = defence.Flasks.Maximum, points = RunState.Points;
            player.Teleport(gift.transform.position + Vector3.up * 0.1f, Quaternion.identity);
            yield return new WaitForSeconds(0.4f);
            yield return Tap(Key.E);
            yield return new WaitForSeconds(0.2f);
            Say($"3 {gift.name}: taken {gift.Taken} flasks {flasks} -> {defence.Flasks.Maximum} embers {points} -> {RunState.Points} says '{defence.LastCallout}'");
        }
        var pickup = FindAnyObjectByType<WeaponPickup>();
        Calm();
        player.Teleport(pickup.transform.position + Vector3.up * 0.1f, Quaternion.identity);
        yield return new WaitForSeconds(0.5f);
        yield return Shot("wicks_8");
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        Say($"3 spear: taken {pickup.Taken} weapon now {combat.Weapon.Name}");

        // 4: the rest fires, each lit, and the return rule.
        Say($"4 before any fire the return point is {(director.ReturnPoint != null ? director.ReturnPoint.position.ToString("0.0") : "none")}");
        for (int i = 0; i < fires.Length; i++)
        {
            Calm();
            player.Teleport(fires[i].transform.position + new Vector3(0f, 0.1f, -1.5f), Quaternion.identity);
            yield return new WaitForSeconds(0.4f);
            yield return Tap(Key.E);
            yield return new WaitForSeconds(0.3f);
            Say($"4 fire {i + 1} at {fires[i].transform.position:0.0}: rests {fires[i].Rests} lit {fires[i].IsLit} return point {director.ReturnPoint.position:0.0} says '{defence.LastCallout}' chart open {UpgradeMenu.Instance != null && UpgradeMenu.Instance.IsOpen}");
            if (i == 0) yield return Shot("river_6");
            yield return Hear("at fire " + (i + 1));
        }
        // Rest at the second fire, kill three ordinary enemies, carry their embers, and fall far from the fire.
        player.Teleport(fires[1].transform.position + new Vector3(0f, 0.1f, -1.5f), Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        int total = director.Enemies.Length;
        int carriedBefore = RunState.Points;
        for (int i = 0; i < 3; i++) director.Enemies[i].TakeHit(new Hit(9999f, 0f, Vector3.forward, true));
        yield return new WaitForSeconds(0.5f);
        Say($"4 three killed: alive {director.EnemiesAlive} of {total} embers {carriedBefore} -> {RunState.Points}");
        Go(0f, 0f, 60f);
        yield return new WaitForSeconds(0.3f);
        yield return Fall("in the riverbed, last fire the quay");
        Say($"4 after the fall: alive {director.EnemiesAlive} of {total} embers carried {RunState.Points} falls {land.Falls} embers lost {land.EmbersLost} (flag LoseCarriedEmbers {land.LoseCarriedEmbers})");
        land.LoseCarriedEmbers = false;
        director.Enemies[4].TakeHit(new Hit(9999f, 0f, Vector3.forward, true));
        yield return new WaitForSeconds(0.4f);
        int kept = RunState.Points;
        yield return Fall("with the flag off");
        Say($"4 flag off: embers before {kept} after {RunState.Points} alive {director.EnemiesAlive} of {total}");
        land.LoseCarriedEmbers = true;

        // 5: the champion.
        var champion = director.Enemies.First(enemy => enemy.Kind.Name == EnemyKind.ChampionName);
        var gate = FindAnyObjectByType<GateOfNoon>();
        Calm();
        player.Teleport(gate.transform.position + new Vector3(0f, 0.1f, -1.2f), Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.2f);
        Say($"5 gate while he stands: tries {gate.Tries} opened {gate.Opened} says '{defence.LastCallout}'");
        Calm();
        Go(0f, 4f, 484.6f);
        yield return new WaitForSeconds(0.25f);
        Say($"5 champion: health {champion.Health.Current:0} balance {champion.Stagger.Maximum:0} aware north of the line {champion.IsAware} elite {champion.Kind.IsElite}");
        yield return Shot("gate_1");
        int phases = 0;
        champion.PhaseChanged += _ => phases++;
        string[] moves = { "champion_thrust_1", "champion_sweep", "champion_butt", "champion_throw", "champion_plant", "champion_reap", "champion_throw_twice" };
        for (int i = 0; i < moves.Length; i++)
        {
            champion.Restore();
            yield return null;
            if (i >= 5) champion.Brain.Phase = 1;
            float distance = moves[i].Contains("throw") ? 8f : moves[i] == "champion_butt" || moves[i] == "champion_plant" ? 2.2f : 3.2f;
            player.Teleport(champion.transform.position + champion.transform.forward * distance + Vector3.up * 0.1f, Quaternion.LookRotation(-champion.transform.forward));
            yield return null;
            int taken = defence.BlowsTaken;
            champion.Brain.Alert();
            bool forced = champion.Brain.Force(moves[i]);
            var attack = champion.Brain.Current;
            yield return Until(() => champion.Brain.TelegraphProgress > 0.85f, 3f);
            yield return Shot("gate_" + (i + 2));
            yield return Until(() => champion.Brain.State == EnemyState.Recover || champion.Brain.Current != attack, 4f);
            yield return new WaitForSeconds(0.9f);
            Say($"   {moves[i]} at {distance:0.0} m: forced {forced} red {attack != null && attack.Unblockable} blows landed on a still player +{defence.BlowsTaken - taken}");
        }
        champion.Restore();
        yield return null;
        Go(0f, 4f, 484f);
        champion.Brain.Alert();
        int blows = 0;
        float front = champion.Health.Current;
        champion.TakeHit(new Hit(36f, 0f, -champion.transform.forward, false, false, champion.transform.position + champion.transform.forward * 2f));
        float guarded = front - champion.Health.Current;
        while (!champion.IsDead && blows < 80)
        {
            // From behind, where the gold does not turn the blow; no stagger, so the count is of health alone.
            champion.TakeHit(new Hit(20f, 0f, champion.transform.forward, false, false, champion.transform.position - champion.transform.forward));
            blows++;
            if (phases == 1 && champion.Brain.Phase == 1 && blows < 79) { blows = 79; Say($"   second phase at health {champion.Health.Current:0} of {champion.Kind.Health:0}: phase {champion.Brain.Phase} doing {(champion.Brain.Current != null ? champion.Brain.Current.Name : "nothing")}"); yield return new WaitForSeconds(0.9f); yield return Shot("gate_9"); blows = 40; }
            yield return new WaitForSeconds(0.12f);
        }
        yield return new WaitForSeconds(1.2f);
        Say($"5 champion beaten: dead {champion.IsDead}; a light blow of 36 on his guard took {guarded:0}; second phase began {phases} time(s); embers now {RunState.Points}");
        yield return Shot("gate_10");
        player.Teleport(fires[fires.Length - 1].transform.position + new Vector3(0f, 0.1f, -1.5f), Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        Go(0f, 4f, 476f);
        yield return Fall("in the champion's court, last fire the porch");
        Say($"5 after a fall: champion dead {champion.IsDead} alive {director.EnemiesAlive} of {total} (all but him)");
        player.Teleport(fires[fires.Length - 1].transform.position + new Vector3(0f, 0.1f, -1.5f), Quaternion.identity);
        yield return new WaitForSeconds(0.3f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        Say($"5 after a rest: champion dead {champion.IsDead} alive {director.EnemiesAlive} of {total}");
        player.Teleport(gate.transform.position + new Vector3(0f, 0.1f, -1.2f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        Say($"5 gate after: opened {gate.Opened} says '{defence.LastCallout}' leads to '{gate.LeadsTo}' which the game can load {Application.CanStreamedLevelBeLoaded(gate.LeadsTo)} still in {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        yield return Shot("gate_11");
        Say($"asleep by distance at the gate {land.Asleep} of {total}");
        Say("done");
    }
}
