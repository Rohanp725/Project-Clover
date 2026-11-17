using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the Fire land's second pass in the test yard: each new mechanic forced and measured, with frames at
// its tell and at its hit in review/shots/current/fire2_<what>_<n>.png. Run with
//   bash AgentScripts/runprobe.sh FireYard FireMechanicsProbe 200
public static class FireMechanicsProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Fire Mechanics Probe").AddComponent<FireMechanicsProbeRunner>();
        return "probe started";
    }
}

public class FireMechanicsProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private AreaDirector director;
    private NoonHeat heat;
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

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    private Enemy Find(string kind, int which)
    {
        foreach (var enemy in director.Enemies)
            if (enemy.Kind.Name == kind && which-- == 0) return enemy;
        return null;
    }

    private static void Put(Enemy enemy, Vector3 at, Vector3 face)
    {
        var body = enemy.GetComponent<CharacterController>();
        body.enabled = false;
        enemy.transform.position = at;
        face.y = 0f;
        if (face.sqrMagnitude > 0.01f) enemy.transform.rotation = Quaternion.LookRotation(face, Vector3.up);
        body.enabled = true;
        // Physics reads bodies where they were last moved; a teleport must be told to it before the next overlap test.
        Physics.SyncTransforms();
    }

    private void Stand(Vector3 at, Vector3 face)
    {
        face.y = 0f;
        player.Teleport(at, Quaternion.LookRotation(face.sqrMagnitude > 0.01f ? face : Vector3.forward, Vector3.up));
    }

    // Everyone back where they began, alive and unaware, so no one walks into the next test.
    private void Calm()
    {
        foreach (var enemy in director.Enemies)
            if (!enemy.Kind.IsElite) enemy.Restore();
        defence.Restore();
        if (heat != null) heat.SetClock(0f);
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        defence = player.GetComponent<PlayerDefence>();
        director = FindAnyObjectByType<AreaDirector>();
        heat = FindAnyObjectByType<NoonHeat>();
        land = FindAnyObjectByType<LandReturn>();
        yield return new WaitForSeconds(0.5f);
        Say($"start: enemies {director.Enemies.Length} heat {(heat != null)} first swell at {heat.Schedule.FirstAt} floor capacity {MoltenFloor.Capacity} land return {(land != null)} embers return {land.EmbersReturnWithTheDead}");

        yield return Ashen();
        yield return AshenEarly();
        yield return WickSpread();
        yield return Noon();
        yield return Embers();
        yield return Gold();
        yield return Wall();
        yield return Pour();
        yield return Buried();
        yield return Jar();
        yield return Beam();
        yield return Mirror();
        yield return Spear();
        Calm();
        Say("done");
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }

    // 1. The ashen swells beside a dormant wick and a levy and bursts: the player, the levy and the wick are all caught.
    private IEnumerator Ashen()
    {
        Calm();
        var ashen = Find(EnemyKind.AshenName, 0);
        var wick = Find(EnemyKind.WickName, 2);
        var levy = Find(EnemyKind.GildedLevyName, 1);
        var burst = ashen.GetComponent<AshenBurst>();
        Vector3 at = new Vector3(46f, 0.05f, 16f);
        Put(ashen, at, Vector3.back);
        Put(wick, at + new Vector3(2f, 0f, 0.5f), Vector3.left);
        Put(levy, at + new Vector3(-2f, 0f, 0.6f), Vector3.right);
        Stand(at + Vector3.back * 1.6f, Vector3.forward);
        yield return null;
        float levyBefore = levy.Health.Current, playerBefore = defence.Health.Current;
        keepAlive = false;
        ashen.Brain.Alert();
        bool forced = ashen.Brain.Force("ashen_swell");
        yield return Until(() => ashen.Brain.TelegraphProgress > 0.35f, 3f);
        yield return Shot("fire2_ashen_swell_0");
        yield return Until(() => ashen.Brain.TelegraphProgress > 0.9f, 3f);
        yield return Shot("fire2_ashen_swell_1");
        yield return Until(() => ashen.IsDead, 3f);
        yield return new WaitForSeconds(0.08f);
        yield return Shot("fire2_ashen_burst_2");
        yield return new WaitForSeconds(0.25f);
        yield return Shot("fire2_ashen_burst_3");
        var flame = wick.GetComponent<WickFlame>();
        Say($"1 ashen at burst: levy {(levy.transform.position - burst.BurstAt).magnitude:0.0} m off, wick {(wick.transform.position - burst.BurstAt).magnitude:0.0} m off, player {(player.transform.position - burst.BurstAt).magnitude:0.0} m off; struck '{burst.LastStruck}' wick health {wick.Health.Current:0} has flame {(wick.GetComponent<WickFlame>() != null)}");
        Say($"1 ashen swell: forced {forced} burst {burst.Bursts} dead {ashen.IsDead} player hurt {burst.PlayerHurt} ({playerBefore:0} -> {defence.Health.Current:0}) enemies hurt {burst.EnemiesHurt} levy {levyBefore:0} -> {levy.Health.Current:0} wicks lit {burst.WicksLit} wick lit {flame.IsLit} wick dormant {wick.IsDormant}");
        keepAlive = true;
        yield return new WaitForSeconds(1.2f);
        yield return Shot("fire2_wick_lit_0");
    }

    // 2. Killed before it swells, the ashen bursts where it fell a moment later, with its ring on the ground.
    private IEnumerator AshenEarly()
    {
        Calm();
        var ashen = Find(EnemyKind.AshenName, 1);
        var levy = Find(EnemyKind.GildedLevyName, 0);
        var burst = ashen.GetComponent<AshenBurst>();
        Vector3 at = new Vector3(50f, 0.05f, 8f);
        Put(ashen, at, Vector3.back);
        Put(levy, at + new Vector3(1.6f, 0f, 0f), Vector3.left);
        Stand(at + new Vector3(0f, 0f, -5f), Vector3.forward);
        yield return null;
        float levyBefore = levy.Health.Current;
        int burstsBefore = burst.Bursts;
        ashen.TakeHit(new Hit(999f, 0f, Vector3.forward, true, true));
        Vector3 fell = ashen.transform.position;
        yield return new WaitForSeconds(0.35f);
        yield return Shot("fire2_ashen_early_0");
        bool pendingThen = burst.Pending;
        yield return Until(() => burst.Bursts > burstsBefore, 2f);
        yield return new WaitForSeconds(0.06f);
        yield return Shot("fire2_ashen_early_1");
        Say($"2 ashen killed early: pending after 0.35 s {pendingThen} burst {burst.Bursts - burstsBefore} struck '{burst.LastStruck}' where it fell {(burst.BurstAt - fell).magnitude:0.00} m off; levy {levyBefore:0} -> {levy.Health.Current:0}; player 5 m off hurt {burst.PlayerHurt}");
    }

    // 3. A burning wick walks into a dormant one and lights it; it leaves burning footprints.
    private IEnumerator WickSpread()
    {
        Calm();
        var walker = Find(EnemyKind.WickName, 0);
        var sleeper = Find(EnemyKind.WickName, 3);
        var flame = walker.GetComponent<WickFlame>();
        Put(walker, new Vector3(40f, 0.05f, 20f), Vector3.right);
        Put(sleeper, new Vector3(45f, 0.05f, 20f), Vector3.left);
        sleeper.MakeDormant(0.5f);
        sleeper.Restore();
        Put(sleeper, new Vector3(45f, 0.05f, 20f), Vector3.left);
        Stand(new Vector3(51f, 0.05f, 20f), Vector3.left);
        yield return null;
        bool lit = flame.Light();
        int marks = MoltenFloor.Count;
        yield return Until(() => sleeper.GetComponent<WickFlame>().IsLit, 6f);
        yield return Shot("fire2_wick_spread_0");
        yield return new WaitForSeconds(0.8f);
        yield return Shot("fire2_wick_spread_1");
        Say($"3 wick flame: lit {lit} pace {walker.FlamePace:0.00} spread {flame.Spread} sleeper lit {sleeper.GetComponent<WickFlame>().IsLit} sleeper awake {!sleeper.IsDormant} footprints and marks {marks} -> {MoltenFloor.Count}");
        sleeper.MakeDormant(3.5f);
    }

    // 4. The heat of noon: the warning, a swell in the sun burns, in shade it does not, in the calm gate it does not; wicks quicken.
    private IEnumerator Noon()
    {
        Calm();
        keepAlive = false;
        var wick = Find(EnemyKind.WickName, 0);
        Stand(new Vector3(64f, 0.05f, -2f), Vector3.forward);
        heat.SetClock(heat.Schedule.FirstAt - 2.6f);
        yield return new WaitForSeconds(0.5f);
        Say($"4 noon: phase {heat.Current} progress {heat.Progress:0.00} callout '{defence.LastCallout}'");
        yield return Shot("fire2_noon_warning_0");
        yield return Until(() => heat.Current == HeatSchedule.Phase.Swell, 4f);
        float before = defence.Health.Current;
        float burnt = heat.Burnt;
        yield return new WaitForSeconds(2.5f);
        yield return Shot("fire2_noon_swell_1");
        float sun = defence.Health.Current;
        Say($"   swell in the sun: lit {Sunlight.IsLit(player.transform.position)} health {before:0} -> {sun:0} burnt {heat.Burnt - burnt:0} wick swell pace {wick.SwellPace:0.00} swelling {NoonHeat.Swelling}");
        var shade = Clover.EditorTools.FireYardBuilder.AwningShade;
        Stand(new Vector3((shade.x + shade.y) * 0.5f, 0.05f, (shade.z + shade.w) * 0.5f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        before = defence.Health.Current;
        burnt = heat.Burnt;
        yield return new WaitForSeconds(2.5f);
        yield return Shot("fire2_noon_shade_2");
        Say($"   swell in the awning's shade: lit {Sunlight.IsLit(player.transform.position)} health {before:0} -> {defence.Health.Current:0} burnt {heat.Burnt - burnt:0}");
        var calm = Clover.EditorTools.FireYardBuilder.GateCalm;
        Stand(new Vector3((calm.x + calm.y) * 0.5f, 0.05f, (calm.z + calm.w) * 0.5f - 1f), Vector3.forward);
        yield return new WaitForSeconds(0.3f);
        before = defence.Health.Current;
        burnt = heat.Burnt;
        yield return new WaitForSeconds(2f);
        Say($"   swell at the calm gate: lit {Sunlight.IsLit(player.transform.position)} calm {heat.IsCalm(player.transform.position)} health {before:0} -> {defence.Health.Current:0} burnt {heat.Burnt - burnt:0}; swells so far {heat.Swells}");
        keepAlive = true;
        heat.SetClock(0f);
        yield return null;
    }

    // 5. Embers after a fall: carried embers go to the two killed; each glows and gives its share back; a second fall loses the rest.
    private IEnumerator Embers()
    {
        Calm();
        // A rest first, so only what is killed from here on carries the fall's embers.
        director.Rest(director.ReturnPoint);
        var first = Find(EnemyKind.ThirstName, 0);
        var second = Find(EnemyKind.WickName, 1);
        Clover.Core.RunState.Points = 9;
        first.TakeHit(new Hit(999f, 0f, Vector3.forward, true, true));
        second.TakeHit(new Hit(999f, 0f, Vector3.forward, true, true));
        yield return new WaitForSeconds(0.3f);
        int carried = Clover.Core.RunState.Points;
        keepAlive = false;
        defence.TakeHit(new Hit(9999f, 0f, Vector3.forward, false, true));
        yield return Until(() => !defence.IsDead, 6f);
        yield return new WaitForSeconds(0.5f);
        int firstIndex = Array.IndexOf(director.Enemies, first), secondIndex = Array.IndexOf(director.Enemies, second);
        Say($"5 embers: carried {carried} at the fall; after it carried {Clover.Core.RunState.Points} waiting {land.Debt.Waiting} ({first.Kind.Name} {land.Debt.Owed(firstIndex)}, {second.Kind.Name} {land.Debt.Owed(secondIndex)}) callout '{defence.LastCallout}'");
        keepAlive = true;
        Stand(second.transform.position + second.transform.forward * 3.5f, -second.transform.forward);
        yield return new WaitForSeconds(0.6f);
        yield return Shot("fire2_embers_carrier_0");
        int before = Clover.Core.RunState.Points;
        second.TakeHit(new Hit(999f, 0f, Vector3.forward, true, true));
        yield return new WaitForSeconds(0.2f);
        Say($"   carrier killed again: embers {before} -> {Clover.Core.RunState.Points} reclaimed {land.Reclaimed} waiting {land.Debt.Waiting} callout '{defence.LastCallout}'");
        yield return new WaitForSeconds(1.8f);
        yield return Shot("fire2_embers_reclaimed_1");
        keepAlive = false;
        int beforeFall = Clover.Core.RunState.Points;
        defence.TakeHit(new Hit(9999f, 0f, Vector3.forward, false, true));
        yield return Until(() => !defence.IsDead, 6f);
        yield return new WaitForSeconds(0.5f);
        Say($"   second fall carrying {beforeFall}: forfeited {land.Forfeited} waiting {land.Debt.Waiting} first still carries {land.Debt.Owed(firstIndex)} callout '{defence.LastCallout}'");
        keepAlive = true;
        Clover.Core.RunState.Points = 0;
    }

    // 6. Running gold: blows on a levy splash molten gold that burns while hot and slows while it cools; the pool is capped.
    private IEnumerator Gold()
    {
        Calm();
        var levy = Find(EnemyKind.GildedLevyName, 2);
        Put(levy, new Vector3(56f, 0.05f, 4f), Vector3.back);
        Stand(new Vector3(56f, 0.05f, 2.4f), Vector3.forward);
        yield return null;
        int splashes = MoltenFloor.Splashes;
        for (int i = 0; i < 4; i++)
        {
            // From behind, where the gold does not turn the blow.
            levy.TakeHit(new Hit(4f, 0f, levy.transform.forward, i == 3, false));
            yield return new WaitForSeconds(0.15f);
        }
        yield return Shot("fire2_gold_splash_0");
        int gold = MoltenFloor.GoldCount;
        Say($"6 gold: blows 4 splashes +{MoltenFloor.Splashes - splashes} gold lying {gold}");
        // A puddle under the player's feet, hot then cooling, with the levy put back so it cannot knock the player off it.
        Calm();
        keepAlive = false;
        Vector3 spot = new Vector3(60f, 0.05f, 4f);
        Stand(spot + Vector3.back * 3f, Vector3.forward);
        MoltenFloor.Splash(spot, 0.8f);
        yield return new WaitForSeconds(0.3f);
        Stand(spot, Vector3.forward);
        float before = defence.Health.Current;
        yield return new WaitForSeconds(1.2f);
        Say($"   standing in hot gold: burns {MoltenFloor.BurnsAt(spot)} health {before:0} -> {defence.Health.Current:0} pace {player.GroundFactor:0.00}");
        keepAlive = true;
        yield return new WaitForSeconds(1.8f);
        yield return Shot("fire2_gold_cool_1");
        Say($"   cooled: burns {MoltenFloor.BurnsAt(spot)} pace on it {player.GroundFactor:0.00} (MoltenFloor.PaceAt {MoltenFloor.PaceAt(spot):0.00})");
        for (int i = 0; i < 200; i++) MoltenFloor.Splash(spot + new Vector3(UnityEngine.Random.Range(-6f, 6f), 0f, UnityEngine.Random.Range(-6f, 6f)), 0.5f);
        yield return null;
        Say($"   200 more splashes at once: lying {MoltenFloor.Count} of capacity {MoltenFloor.Capacity}");
        yield return Shot("fire2_gold_many_2");
    }

    // 7. The shield wall: three levies lock into a line and advance; a light blow from the front is turned; a heavy one opens it.
    private IEnumerator Wall()
    {
        Calm();
        var wall = FindAnyObjectByType<ShieldWall>();
        var levies = new[] { Find(EnemyKind.GildedLevyName, 1), Find(EnemyKind.GildedLevyName, 2), Find(EnemyKind.GildedLevyName, 3) };
        Stand(new Vector3(48.3f, 0.05f, 24.5f), Vector3.forward);
        foreach (var levy in levies) levy.Brain.Alert();
        yield return new WaitForSeconds(2.2f);
        yield return Shot("fire2_wall_0");
        var middle = levies[1];
        Vector3 fromPlayer = middle.transform.position - player.transform.position;
        fromPlayer.y = 0f;
        Say($"7 wall: formed {wall.Formed} levies at {levies[0].transform.position.x:0.0},{levies[0].transform.position.z:0.0} / {levies[1].transform.position.x:0.0},{levies[1].transform.position.z:0.0} / {levies[2].transform.position.x:0.0},{levies[2].transform.position.z:0.0} player at z {player.transform.position.z:0.0}");
        var light = middle.TakeHit(new Hit(10f, 5f, fromPlayer.normalized, false, false));
        var heavy = middle.TakeHit(new Hit(20f, 10f, fromPlayer.normalized, true, false));
        yield return Shot("fire2_wall_break_1");
        Say($"   light blow from the front: {light}; heavy blow on the shield: {heavy}; broken {wall.IsBroken} breaks {wall.Breaks} guard open {levies[0].GuardOpenFor:0.0} s");
        var after = levies[0].TakeHit(new Hit(10f, 5f, (levies[0].transform.position - player.transform.position).normalized, false, false));
        Say($"   light blow from the front while the line is open: {after}");
    }

    // 8. The warden's pour: an arc of molten gold that the player cannot walk through.
    private IEnumerator Pour()
    {
        Calm();
        var warden = Find(EnemyKind.GildedWardenName, 0);
        Vector3 at = new Vector3(2f, 0.05f, 30f);
        Put(warden, at, Vector3.back);
        Stand(at + Vector3.back * 4f, Vector3.forward);
        yield return null;
        warden.Brain.Alert();
        bool forced = warden.Brain.Force("gilded_pour");
        yield return Until(() => warden.Brain.TelegraphProgress > 0.7f, 3f);
        yield return Shot("fire2_pour_0");
        yield return Until(() => warden.Brain.State == EnemyState.Strike || warden.Brain.State == EnemyState.Recover, 3f);
        yield return new WaitForSeconds(0.25f);
        yield return Shot("fire2_pour_1");
        int walls = MoltenFloor.WallCount;
        // Walk straight at him through the gold.
        Stand(at + Vector3.back * 5.5f, Vector3.forward);
        warden.Brain.Alert();
        float nearest = 99f;
        Keys(Key.W);
        float end = Time.unscaledTime + 1.4f;
        while (Time.unscaledTime < end)
        {
            yield return null;
            nearest = Mathf.Min(nearest, (player.transform.position - at).magnitude);
        }
        Keys();
        Say($"8 pour: forced {forced} wall pieces {walls}; walking at him through it, nearest {nearest:0.00} m (the arc lies at {warden.Kind.Brain.Attacks.Find(a => a.Pours).Reach:0.0} m)");
        yield return Shot("fire2_pour_2");
    }

    // 9. The buried thirsting: a breathing mound, then a burst of dust and a lunge as the player comes near.
    private IEnumerator Buried()
    {
        Calm();
        var buried = FindAnyObjectByType<BuriedMound>();
        var enemy = buried.GetComponent<Enemy>();
        Stand(enemy.transform.position + new Vector3(-5f, 0f, 0f), Vector3.right);
        yield return new WaitForSeconds(1.8f);
        yield return Shot("fire2_buried_0");
        Say($"9 buried: under {buried.Buried} dormant {enemy.IsDormant}");
        Stand(enemy.transform.position + new Vector3(-2.6f, 0f, 0f), Vector3.right);
        yield return Until(() => buried.Bursts > 0, 2f);
        yield return new WaitForSeconds(0.08f);
        yield return Shot("fire2_buried_burst_1");
        yield return Until(() => enemy.Brain.State == EnemyState.Telegraph, 2f);
        string lunge = enemy.Brain.Current != null ? enemy.Brain.Current.Name : "none";
        yield return new WaitForSeconds(0.3f);
        yield return Shot("fire2_buried_lunge_2");
        Say($"   burst {buried.Bursts} under {buried.Buried} lunge '{lunge}' state {enemy.Brain.State}");
    }

    // 10. A broken jar: a hurt thirsting leaves the fight, drinks it, and comes back healed and swollen.
    private IEnumerator Jar()
    {
        Calm();
        var thirst = Find(EnemyKind.ThirstName, 3);
        var drinker = thirst.GetComponent<ThirstDrinker>();
        WaterJar jar = null;
        foreach (var each in FindObjectsByType<WaterJar>(FindObjectsSortMode.None))
            if (jar == null || (each.transform.position - thirst.transform.position).sqrMagnitude < (jar.transform.position - thirst.transform.position).sqrMagnitude) jar = each;
        Stand(jar.transform.position + new Vector3(0f, 0f, -3f), Vector3.forward);
        thirst.TakeHit(new Hit(25f, 0f, Vector3.forward, false, true));
        float hurt = thirst.Health.Current, most = thirst.Health.Maximum;
        Stand(new Vector3(60f, 0.05f, 30f), Vector3.forward);
        yield return null;
        jar.TakeHit(new Hit(5f, 0f, Vector3.forward));
        yield return new WaitForSeconds(0.1f);
        yield return Shot("fire2_jar_break_0");
        yield return Until(() => thirst.IsSipping, 8f);
        yield return new WaitForSeconds(0.4f);
        yield return Shot("fire2_jar_drink_1");
        yield return Until(() => drinker.Drinks > 0, 4f);
        Say($"10 jar: broken {jar.Broken} drinks {drinker.Drinks} health {hurt:0}/{most:0} -> {thirst.Health.Current:0}/{thirst.Health.Maximum:0} from {(thirst.transform.position - jar.transform.position).magnitude:0.0} m of the jar");
        jar.Mend();
    }

    // 11. The priest's mirror: a beam down a lane that burns and lights the ground; the calling draws the gilded.
    private IEnumerator Beam()
    {
        Calm();
        var priest = Find(EnemyKind.SunBlindName, 0);
        var mirror = priest.GetComponent<SunMirror>();
        var levy = Find(EnemyKind.GildedLevyName, 0);
        Vector3 at = new Vector3(27f, 0.05f, 22f);
        Put(priest, at, Vector3.back);
        Stand(at + Vector3.back * 7f, Vector3.forward);
        yield return null;
        keepAlive = false;
        float before = defence.Health.Current;
        priest.Brain.Alert();
        bool forced = priest.Brain.Force("sunblind_beam");
        yield return Until(() => priest.Brain.TelegraphProgress > 0.8f, 3f);
        yield return Shot("fire2_beam_0");
        yield return Until(() => mirror.Beams > 0, 3f);
        yield return new WaitForSeconds(0.05f);
        yield return Shot("fire2_beam_1");
        Vector3 lane = at + Vector3.back * 4f;
        Say($"11 beam: forced {forced} beams {mirror.Beams} health {before:0} -> {defence.Health.Current:0} lane lit {Sunlight.InBeam(lane)}");
        yield return new WaitForSeconds(1f);
        yield return Shot("fire2_beam_lane_2");
        keepAlive = true;
        Put(levy, at + new Vector3(6f, 0f, 2f), Vector3.left);
        bool awareBefore = levy.IsAware;
        mirror.Call();
        yield return new WaitForSeconds(0.1f);
        yield return Shot("fire2_calling_0");
        Say($"   calling: calls {mirror.Calls} levy aware {awareBefore} -> {levy.IsAware} player lit by it {Sunlight.IsLit(player.transform.position)}");
    }

    // 12. The mirror stand: turned by the player, its beam burns a wick standing in it and lights the sun-disc, which opens the door.
    private IEnumerator Mirror()
    {
        Calm();
        var stand = FindAnyObjectByType<MirrorStand>();
        var target = FindAnyObjectByType<MirrorTarget>();
        var door = GameObject.Find("Sun Door");
        var wick = Find(EnemyKind.WickName, 0);
        Put(wick, stand.transform.position + new Vector3(-4f, 0f, 0f), Vector3.right);
        Stand(stand.transform.position + new Vector3(1.6f, 0f, -1.2f), stand.transform.position - player.transform.position);
        yield return new WaitForSeconds(1.2f);
        yield return Shot("fire2_mirror_0");
        Say($"12 mirror: beam ends at {stand.BeamEnd.x:0.0},{stand.BeamEnd.z:0.0} enemies burnt {stand.EnemiesBurnt} wick lit {wick.GetComponent<WickFlame>().IsLit} wick health {wick.Health.Current:0}");
        Stand(stand.transform.position + new Vector3(1.2f, 0f, -0.6f), stand.transform.position - player.transform.position);
        yield return null;
        Keys(Key.E);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => target.IsLit, 4f);
        yield return new WaitForSeconds(0.3f);
        yield return Shot("fire2_mirror_1");
        Say($"   turned {stand.Turns} times; target lit {target.IsLit} door standing {(door != null && door.activeSelf)} beam ends at {stand.BeamEnd.x:0.0},{stand.BeamEnd.z:0.0}");
    }

    // 13. The spearman's throw: the spear stands glowing in the floor; bare-handed he has no guard and walks to fetch it.
    private IEnumerator Spear()
    {
        Calm();
        var spearman = Find(EnemyKind.GildedSpearmanName, 0);
        var cast = spearman.GetComponent<SpearCast>();
        Vector3 at = new Vector3(-7f, 0.05f, 29f);
        Stand(at + Vector3.back * 8f, Vector3.forward);
        Put(spearman, at, Vector3.back);
        yield return null;
        keepAlive = false;
        float before = defence.Health.Current;
        spearman.Brain.Alert();
        int wardsBefore = defence.Wards;
        int blowsBefore = defence.BlowsTaken, dodgesBefore = defence.Dodges, blocksBefore = defence.Blocks, perfectBefore = defence.PerfectGuards;
        int throwsBefore = cast.Throws;
        bool forced = spearman.Brain.Force("gilded_cast");
        yield return Until(() => spearman.Brain.TelegraphProgress > 0.8f, 3f);
        yield return Shot("fire2_spear_0");
        // What else reached the player during the tell: a blow then leaves them flinching, and a blow that arrives
        // while they flinch is ignored by design (PlayerDefence.TakeHit), which is how the throw can seem harmless.
        int blowsInTell = 0;
        bool hurtBefore = false;
        // He may already have thrown earlier in the probe, awake from another section: wait for this throw, not any.
        while (cast.Throws == throwsBefore && spearman.Brain.Current != null)
        {
            blowsInTell = defence.BlowsTaken - blowsBefore;
            hurtBefore = defence.IsHurt;
            yield return null;
        }
        yield return Until(() => cast.Throws > throwsBefore, 3f);
        float atThrow = defence.Health.Current;
        string state = $"leaping {player.IsLeaping} suspended {player.Suspended} hurt {defence.IsHurt} finishing {player.GetComponent<PlayerCombat>().IsFinishing} form {player.FormHold} wards {wardsBefore} -> {defence.Wards} screened {Clover.Player.Forms.SpellWorks.Screens(spearman.transform.position, player.transform.position)}";
        int blowsAtThrow = defence.BlowsTaken - blowsBefore, dodgesAtThrow = defence.Dodges - dodgesBefore, blocksAtThrow = defence.Blocks - blocksBefore, perfectAtThrow = defence.PerfectGuards - perfectBefore;
        Vector3 offAtThrow = player.transform.position - spearman.transform.position;
        float facingOff = Vector3.Angle(Vector3.ProjectOnPlane(spearman.transform.forward, Vector3.up), Vector3.ProjectOnPlane(offAtThrow, Vector3.up));
        yield return new WaitForSeconds(0.1f);
        // Read before the player is healed again: the heal of the next frame would hide the blow.
        float after = defence.Health.Current;
        yield return Shot("fire2_spear_1");
        keepAlive = true;
        Stand(at + new Vector3(4f, 0f, 2f), Vector3.left);
        yield return null;
        var guarded = spearman.TakeHit(new Hit(4f, 0f, (spearman.transform.position - player.transform.position).normalized, false, false));
        Say($"13 spear: forced {forced} thrown {cast.Thrown} stands {(cast.StandsAt - at).magnitude:0.0} m from him health {before:0} -> {after:0} {(after < before ? "OK" : "WRONG")} (blows taken in the tell {blowsInTell}, flinching the frame before {hurtBefore}; at the throw: health {atThrow:0} blows +{blowsAtThrow} dodged +{dodgesAtThrow} blocked +{blocksAtThrow} perfect +{perfectAtThrow}, the player {Vector3.ProjectOnPlane(offAtThrow, Vector3.up).magnitude:0.0} m off and {facingOff:0} degrees off his facing; {state}; blows in all +{defence.BlowsTaken - blowsBefore}); a light blow from the front while bare-handed: {guarded}");
        yield return Until(() => cast.Fetched > 0, 8f);
        yield return Shot("fire2_spear_2");
        Say($"   fetched {cast.Fetched} thrown {cast.Thrown}");
    }
}
