using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the Fire land's test yard: the player with no element fights each of the land's four people with
// fists, each special rule is tried (the snatch and how it is undone, the hold and how it is broken, the ranks of the
// gilded, the priest's light and shade), and frames are written to review/shots/current/fire_*.png. Run with
//   bash AgentScripts/runprobe.sh FireYard FireProbe 250
public static class FireProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Fire Probe").AddComponent<FireProbeRunner>();
        return "probe started";
    }
}

public class FireProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private AreaDirector director;
    private bool keepAlive = true;

    private void Update()
    {
        if (keepAlive && defence != null) defence.Health.Heal(100f);
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

    private void StandBefore(Enemy enemy, float distance)
    {
        Vector3 at = enemy.transform.position + enemy.transform.forward * distance;
        player.Teleport(at, Quaternion.LookRotation(-enemy.transform.forward, Vector3.up));
    }

    // Everyone but the one being tried is put back to sleep where it stood, so no one else walks into the test.
    private void Only(Enemy one)
    {
        foreach (var enemy in director.Enemies)
            if (enemy != one && !enemy.IsDead) enemy.Restore();
    }

    // Forces one attack on a player who stands still in front of it, and says what came of it.
    private IEnumerator Still(Enemy enemy, string attack, float distance, string shot = null)
    {
        Only(enemy);
        yield return Until(() => !defence.IsHelpless && !player.IsHeld, 3f);
        defence.Restore();
        StandBefore(enemy, distance);
        yield return null;
        enemy.transform.rotation = Quaternion.LookRotation(player.transform.position - enemy.transform.position, Vector3.up);
        int taken = defence.BlowsTaken;
        float health = defence.Health.Current;
        keepAlive = false;
        enemy.Brain.Alert();
        bool forced = enemy.Brain.Force(attack);
        var forcedAttack = enemy.Brain.Current;
        if (shot != null)
        {
            yield return Until(() => enemy.Brain.TelegraphProgress > 0.8f, 3f);
            yield return Shot(shot);
        }
        yield return Until(() => enemy.Brain.Current != forcedAttack || enemy.Brain.State == EnemyState.Recover || enemy.Brain.State == EnemyState.Busy, 4f);
        yield return new WaitForSeconds(0.1f);
        Say($"   {attack} on a still player at {distance:0.0} m: forced {forced} landed +{defence.BlowsTaken - taken} health {health:0} -> {defence.Health.Current:0} red {forcedAttack != null && forcedAttack.Unblockable}");
        keepAlive = true;
    }

    // The same attack met with a held guard.
    private IEnumerator Guarded(Enemy enemy, string attack, float distance)
    {
        Only(enemy);
        yield return Until(() => !defence.IsHelpless && !player.IsHeld, 3f);
        defence.Restore();
        StandBefore(enemy, distance);
        yield return null;
        enemy.transform.rotation = Quaternion.LookRotation(player.transform.position - enemy.transform.position, Vector3.up);
        int taken = defence.BlowsTaken, blocks = defence.GuardReactions;
        Keys(Key.L);
        yield return new WaitForSeconds(0.45f);
        enemy.Brain.Alert();
        enemy.Brain.Force(attack);
        var forcedAttack = enemy.Brain.Current;
        yield return Until(() => enemy.Brain.Current != forcedAttack || enemy.Brain.State == EnemyState.Recover || enemy.Brain.State == EnemyState.Busy || enemy.IsStaggered, 4f);
        yield return new WaitForSeconds(0.1f);
        Keys();
        Say($"   {attack} on a held guard: guarded +{defence.GuardReactions - blocks} landed +{defence.BlowsTaken - taken} (perfect guards so far {defence.PerfectGuards}) enemy state {enemy.Brain.State}");
        yield return null;
    }

    // The same attack met by rolling out of it sideways as it is shown.
    private IEnumerator Rolled(Enemy enemy, string attack, float distance)
    {
        Only(enemy);
        yield return Until(() => !defence.IsHelpless && !player.IsHeld, 3f);
        defence.Restore();
        player.Stamina.Fill();
        StandBefore(enemy, distance);
        yield return null;
        enemy.transform.rotation = Quaternion.LookRotation(player.transform.position - enemy.transform.position, Vector3.up);
        int taken = defence.BlowsTaken;
        enemy.Brain.Alert();
        enemy.Brain.Force(attack);
        var forcedAttack = enemy.Brain.Current;
        yield return Until(() => enemy.Brain.TelegraphProgress > 0.72f, 3f);
        Keys(Key.D, Key.Space);
        yield return null;
        yield return null;
        Keys(Key.D);
        yield return Until(() => enemy.Brain.Current != forcedAttack || enemy.Brain.State == EnemyState.Recover || enemy.Brain.State == EnemyState.Busy, 4f);
        Keys();
        yield return new WaitForSeconds(0.1f);
        Say($"   {attack} rolled out of sideways: landed +{defence.BlowsTaken - taken} dodges {defence.Dodges + defence.PerfectDodges} held {player.IsHeld}");
    }

    private IEnumerator Kill(Enemy enemy, string shot)
    {
        yield return Until(() => !player.IsHeld, 3f);
        int embers = Clover.Core.RunState.Points;
        float health = enemy.Health.Current;
        // Fists until it falls: the real blows, with a hard stop so a guard cannot hold the probe up for ever.
        StandBefore(enemy, 1.4f);
        int blows = combat.BlowsLanded;
        float end = Time.unscaledTime + 5f;
        while (!enemy.IsDead && Time.unscaledTime < end)
        {
            if (!combat.IsAttacking && !defence.IsHelpless)
            {
                Vector3 to = enemy.transform.position - player.transform.position;
                to.y = 0f;
                if (to.magnitude > 1.8f) StandBefore(enemy, 1.4f);
                yield return Tap(Key.J);
            }
            yield return null;
        }
        int fistBlows = combat.BlowsLanded - blows;
        float after = enemy.Health.Current;
        if (!enemy.IsDead) enemy.TakeHit(new Hit(2000f, 0f, enemy.transform.forward, true, true));
        yield return new WaitForSeconds(0.45f);
        yield return Shot(shot);
        yield return new WaitForSeconds(3.2f);
        Say($"   dies: fist blows landed {fistBlows} took it {health:0} -> {after:0}; dead {enemy.IsDead} body cleared {!enemy.gameObject.activeSelf} embers {embers} -> {Clover.Core.RunState.Points} (kind pays {enemy.Kind.Embers})");
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        director = FindAnyObjectByType<AreaDirector>();
        yield return new WaitForSeconds(0.5f);
        Say($"start: weapon {combat.Weapon.Name} has element {element.HasElement} holds '{element.Info.Name}' affliction {element.Info.Affliction} run element '{Clover.Core.RunState.Element}' enemies {director.EnemiesAlive} sunlight in use {Sunlight.InUse} open sky {Sunlight.OpenSky} rectangles {Sunlight.Rectangles}");
        yield return Shot("fire_yard_0");

        // 0: no element. The spell and surge buttons do nothing even with mana; the rest fire does not open the chart.
        combat.Mana.Gain(100f);
        yield return Tap(Key.U);
        yield return new WaitForSeconds(0.3f);
        yield return Tap(Key.I);
        yield return new WaitForSeconds(0.3f);
        Say($"0 no element: spells cast {element.SpellsCast} surges {element.Surges} casting {element.IsCasting} surge active {element.Kit.SurgeActive} mana kept {combat.Mana.Current:0} weapon affliction {combat.WeaponAffliction}");
        var rest = FindAnyObjectByType<RestPoint>();
        player.Teleport(rest.transform.position + new Vector3(0f, 0.1f, 1.5f), Quaternion.identity);
        yield return new WaitForSeconds(0.5f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
        var menu = Clover.UI.UpgradeMenu.Instance;
        Say($"0 rest fire: rests {rest.Rests} chart open {menu != null && menu.IsOpen} says '{defence.LastCallout}' time {Time.timeScale:0}");
        yield return Shot("fire_yard_1");

        // 1: the thirsting.
        var thirst = Find(EnemyKind.ThirstName, 0);
        Say($"1 THE THIRSTING: health {thirst.Health.Current:0} balance {thirst.Stagger.Maximum:0} embers {thirst.Kind.Embers}");
        StandBefore(thirst, 3f);
        yield return new WaitForSeconds(0.7f);
        yield return Shot("fire_thirst_0");
        float mana = combat.Mana.Current;
        combat.Mana.Empty();
        StandBefore(thirst, 1.4f);
        yield return Tap(Key.J);
        yield return new WaitForSeconds(0.5f);
        Say($"   one fist blow: mana earned {combat.Mana.Current:0.0} (had {mana:0})");
        yield return Still(thirst, "thirst_claw_1", 1.6f, "fire_thirst_1");
        yield return Guarded(thirst, "thirst_claw_1", 1.6f);
        thirst.Restore();
        yield return Still(thirst, "thirst_pounce", 4.5f, "fire_thirst_2");
        yield return Guarded(thirst, "thirst_pounce", 4.5f);
        yield return Rolled(thirst, "thirst_pounce", 4.5f);
        thirst.Restore();

        // The snatch drawn by a drink: the flask goes, and comes back when the thirsting is killed before it has drunk.
        Only(thirst);
        defence.Restore();
        StandBefore(thirst, 7.6f);
        thirst.Brain.Alert();
        yield return Until(() => thirst.Brain.State == EnemyState.Stalk, 2f);
        keepAlive = false;
        defence.Health.Damage(60f);
        int flasks = defence.Flasks.Count;
        float hurt = defence.Health.Current;
        yield return Tap(Key.R);
        yield return null;
        string answered = thirst.Brain.Current != null ? thirst.Brain.Current.Name : "nothing";
        yield return Until(() => thirst.IsDrinking || thirst.Brain.State == EnemyState.Stalk, 3f);
        yield return new WaitForSeconds(0.25f);
        yield return Shot("fire_thirst_3");
        Say($"   snatch: a drink at 7.6 m drew '{answered}'; flasks {flasks} -> {defence.Flasks.Count} (one drunk, taken {thirst.FlasksTaken}); health {hurt:0} -> {defence.Health.Current:0}; thirsting drinking {thirst.IsDrinking}");
        int held = defence.Flasks.Count;
        thirst.TakeHit(new Hit(1f, 1000f, thirst.transform.forward));
        yield return null;
        yield return null;
        Say($"   balance broken mid-drink: flasks {held} -> {defence.Flasks.Count} says '{defence.LastCallout}' staggered {thirst.IsStaggered}");
        keepAlive = true;
        thirst.Restore();

        // Left to finish: the flask is gone and the thirsting is whole again.
        defence.Restore();
        StandBefore(thirst, 7.6f);
        thirst.Brain.Alert();
        yield return Until(() => thirst.Brain.State == EnemyState.Stalk, 2f);
        thirst.Health.Damage(25f);
        keepAlive = false;
        defence.Health.Damage(60f);
        flasks = defence.Flasks.Count;
        yield return Tap(Key.R);
        yield return Until(() => thirst.IsDrinking, 3f);
        yield return Until(() => !thirst.IsDrinking, 3f);
        yield return null;
        Say($"   left to drink: flasks {flasks} -> {defence.Flasks.Count} drunk {thirst.FlasksDrunk} says '{defence.LastCallout}' thirsting health {thirst.Health.Current:0}/{thirst.Health.Maximum:0}");
        keepAlive = true;
        thirst.Restore();

        // Prevented: the drink begun, then a roll out of the snatch's lane.
        defence.Restore();
        player.Stamina.Fill();
        StandBefore(thirst, 7.6f);
        thirst.Brain.Alert();
        yield return Until(() => thirst.Brain.State == EnemyState.Stalk, 2f);
        keepAlive = false;
        defence.Health.Damage(60f);
        flasks = defence.Flasks.Count;
        int took = thirst.FlasksTaken;
        yield return Tap(Key.R);
        yield return Until(() => thirst.Brain.TelegraphProgress > 0.72f, 2f);
        Keys(Key.D, Key.Space);
        yield return null;
        yield return null;
        Keys(Key.D);
        yield return new WaitForSeconds(0.9f);
        Keys();
        Say($"   prevented by rolling aside: flasks {flasks} -> {defence.Flasks.Count} taken +{thirst.FlasksTaken - took}");
        keepAlive = true;
        thirst.Restore();
        defence.Restore();
        yield return Guarded(thirst, "thirst_snatch", 4.5f);
        Say($"   snatch on a held guard: taken so far {thirst.FlasksTaken} flasks {defence.Flasks.Count}");
        thirst.Restore();
        yield return Kill(thirst, "fire_thirst_4");

        // The one kneeling at the dry pool.
        var kneeler = Find(EnemyKind.ThirstName, 1);
        player.Teleport(kneeler.transform.position - kneeler.transform.forward * 6f, kneeler.transform.rotation);
        yield return new WaitForSeconds(0.6f);
        bool asleep = kneeler.IsDormant;
        yield return Shot("fire_thirst_5");
        player.Teleport(kneeler.transform.position - kneeler.transform.forward * 2.4f, kneeler.transform.rotation);
        yield return new WaitForSeconds(0.2f);
        Say($"   kneeling at the pool: dormant at 6 m {asleep}; at 2.4 m dormant {kneeler.IsDormant} rising {kneeler.IsRising} state {kneeler.Brain.State}");
        yield return new WaitForSeconds(0.6f);
        kneeler.TakeHit(new Hit(2000f, 0f, kneeler.transform.forward, true, true));

        // 2: the wick.
        var wick = Find(EnemyKind.WickName, 0);
        Say($"2 THE WICK: health {wick.Health.Current:0} balance {wick.Stagger.Maximum:0} embers {wick.Kind.Embers}");
        Only(wick);
        StandBefore(wick, 3f);
        yield return new WaitForSeconds(0.7f);
        yield return Shot("fire_wick_0");
        yield return Still(wick, "wick_swipe", 1.8f, "fire_wick_1");
        yield return Guarded(wick, "wick_swipe", 1.8f);
        wick.Restore();
        yield return Still(wick, "wick_slam", 1.8f);
        yield return Guarded(wick, "wick_slam", 1.8f);
        wick.Restore();
        yield return Rolled(wick, "wick_grab", 2.6f);
        yield return Until(() => wick.Brain.State == EnemyState.Stalk, 3f);
        yield return Guarded(wick, "wick_grab", 2.6f);
        yield return Until(() => !player.IsHeld, 4f);
        wick.Restore();

        // The hold, left to run its time.
        defence.Restore();
        yield return Still(wick, "wick_grab", 2.6f, "fire_wick_2");
        keepAlive = false;
        float before = defence.Health.Current;
        bool wasHeld = player.IsHeld;
        Keys(Key.W);
        Vector3 stood = player.transform.position;
        yield return new WaitForSeconds(0.6f);
        yield return Shot("fire_wick_3");
        float moved = (player.transform.position - stood).magnitude;
        Keys();
        float t0 = Time.time;
        yield return Until(() => !player.IsHeld, 4f);
        Say($"   the hold, left alone: held {wasHeld} moved {moved:0.00} m with the stick pushed; lasted {Time.time - t0 + 0.6f:0.0}+ s bites {wick.Grip.Bites} health {before:0} -> {defence.Health.Current:0}; wick busy {wick.Brain.State} balance {wick.Stagger.Value:0}");
        keepAlive = true;
        yield return new WaitForSeconds(1.2f);
        wick.Restore();

        // Broken by pressing.
        defence.Restore();
        yield return Still(wick, "wick_grab", 2.6f);
        keepAlive = false;
        before = defence.Health.Current;
        t0 = Time.time;
        for (int i = 0; i < 8 && player.IsHeld; i++)
        {
            yield return Tap(i % 2 == 0 ? Key.J : Key.Space);
            yield return new WaitForSeconds(0.12f);
        }
        yield return null;
        Say($"   the hold, struggled against: presses {wick.Grip.Presses} broke it {wick.Grip.BrokenByStruggle} after {Time.time - t0:0.0} s bites {wick.Grip.Bites} health {before:0} -> {defence.Health.Current:0}; wick state {wick.Brain.State} balance {wick.Stagger.Value:0}/{wick.Stagger.Maximum:0}");
        keepAlive = true;
        yield return new WaitForSeconds(1.2f);
        wick.Restore();

        // It does not kill.
        defence.Restore();
        yield return Still(wick, "wick_grab", 2.6f);
        keepAlive = false;
        defence.Health.Damage(defence.Health.Current - 9f);
        yield return Until(() => !player.IsHeld, 4f);
        Say($"   the hold on 9 health: ends with {defence.Health.Current:0.0} health, dead {defence.IsDead}, bites {wick.Grip.Bites}");
        keepAlive = true;
        yield return new WaitForSeconds(1.2f);
        wick.Restore();

        // A heavy blow breaks the grab while it is shown.
        defence.Restore();
        StandBefore(wick, 1.6f);
        yield return null;
        wick.Brain.Alert();
        wick.Brain.Force("wick_grab");
        yield return new WaitForSeconds(0.3f);
        var shownState = wick.Brain.State;
        wick.TakeHit(new Hit(8f, 8f, wick.transform.forward, true));
        yield return null;
        Say($"   a heavy blow while the grab is shown ({shownState}): wick state {wick.Brain.State} held {player.IsHeld}");
        yield return Until(() => !combat.IsAttacking, 2f);
        wick.Restore();
        yield return Kill(wick, "fire_wick_4");

        // The dormant one between the niches.
        var sleeper = Find(EnemyKind.WickName, 1);
        Only(sleeper);
        player.Teleport(sleeper.transform.position + sleeper.transform.forward * 5.5f, Quaternion.LookRotation(-sleeper.transform.forward));
        yield return new WaitForSeconds(0.7f);
        asleep = sleeper.IsDormant;
        yield return Shot("fire_wick_5");
        float sleeperHealth = sleeper.Health.Current;
        sleeper.TakeHit(new Hit(10f, 0f, -sleeper.transform.forward));
        Say($"   dormant wick: dormant at 5.5 m {asleep}; a blow of 10 took {sleeperHealth - sleeper.Health.Current:0}; now dormant {sleeper.IsDormant} rising {sleeper.IsRising}");
        yield return new WaitForSeconds(0.5f);
        yield return Shot("fire_wick_6");
        yield return Until(() => !sleeper.IsRising, 2f);
        Say($"   risen after its moment: state {sleeper.Brain.State}");
        sleeper.Restore();
        player.Teleport(sleeper.transform.position + sleeper.transform.forward * 3f, Quaternion.LookRotation(-sleeper.transform.forward));
        yield return new WaitForSeconds(0.3f);
        Say($"   walked to 3 m: dormant {sleeper.IsDormant} rising {sleeper.IsRising}");
        sleeper.TakeHit(new Hit(2000f, 0f, -sleeper.transform.forward, true, true));
        yield return new WaitForSeconds(0.5f);

        // 3: the gilded, rank by rank.
        for (int rank = 1; rank <= 3; rank++)
        {
            var gilded = Find(EnemyKind.GildedName(rank), 0);
            Say($"3 {gilded.Kind.Name.ToUpperInvariant()} (rank {rank}): health {gilded.Health.Current:0} balance {gilded.Stagger.Maximum:0} poise {gilded.Kind.PoiseHits} steadfast {gilded.Kind.Steadfast} attacks {gilded.Brain.Attacks.Count} embers {gilded.Kind.Embers} scale {gilded.Kind.Scale}");
            Only(gilded);
            StandBefore(gilded, 3.4f);
            yield return new WaitForSeconds(0.7f);
            yield return Shot($"fire_gilded{rank}_0");
            // Its guard: a light blow from the front, then one from behind.
            gilded.Restore();
            gilded.Brain.Alert();
            float h = gilded.Health.Current;
            var front = gilded.TakeHit(new Hit(20f, 5f, -gilded.transform.forward));
            float lostFront = h - gilded.Health.Current;
            h = gilded.Health.Current;
            var heavy = gilded.TakeHit(new Hit(20f, 5f, -gilded.transform.forward, true));
            float lostHeavy = h - gilded.Health.Current;
            gilded.Restore();
            gilded.Brain.Alert();
            h = gilded.Health.Current;
            var behind = gilded.TakeHit(new Hit(20f, 5f, gilded.transform.forward));
            Say($"   guard: a light blow of 20 from the front {front} took {lostFront:0.0}; a heavy one {heavy} took {lostHeavy:0.0}; a light one from behind {behind} took {h - gilded.Health.Current:0.0}");
            gilded.Restore();
            foreach (var attack in gilded.Brain.Attacks)
            {
                if (attack.MaxRange <= 0f) continue;
                float distance = Mathf.Clamp((attack.MinRange + attack.MaxRange) * 0.5f, 1.8f, 4.2f);
                yield return Still(gilded, attack.Name, distance, attack.Name == "gilded_thrust" ? $"fire_gilded{rank}_1" : attack.Name == "gilded_plant" ? "fire_gilded3_2" : attack.Name == "gilded_lunge" ? $"fire_gilded{rank}_3" : null);
                if (attack.BurnSeconds > 0f)
                {
                    int taken = defence.BlowsTaken;
                    keepAlive = false;
                    gilded.Brain.Occupy(4f);
                    float h2 = defence.Health.Current;
                    yield return new WaitForSeconds(1.2f);
                    yield return Shot("fire_gilded3_4");
                    yield return new WaitForSeconds(2.2f);
                    Say($"   standing in the planted ring for its 3 s: fire bit +{defence.BlowsTaken - taken} health {h2:0} -> {defence.Health.Current:0}");
                    keepAlive = true;
                }
                yield return Until(() => gilded.Brain.State == EnemyState.Stalk, 3f);
                if (!attack.Unblockable) yield return Guarded(gilded, attack.Name, distance);
                yield return Until(() => gilded.Brain.State == EnemyState.Stalk || gilded.IsStaggered, 3f);
                gilded.Restore();
            }
            yield return Kill(gilded, $"fire_gilded{rank}_5");
        }

        // 4: the sun-blind priest.
        var priest = Find(EnemyKind.SunBlindName, 0);
        Say($"4 SUN-BLIND PRIEST: health {priest.Health.Current:0} balance {priest.Stagger.Maximum:0} embers {priest.Kind.Embers}");
        Only(priest);
        var shade = Clover.EditorTools.FireYardBuilder.PorchShade;
        Vector3 inShade = new Vector3((shade.x + shade.y) * 0.5f, 0.05f, (shade.z + shade.w) * 0.5f);
        Vector3 inSun = priest.transform.position + new Vector3(0f, 0f, -9.5f);
        Say($"   the rule: lit at the sun spot {Sunlight.IsLit(inSun)}; lit under the porch {Sunlight.IsLit(inShade)}; lit just south of the shade {Sunlight.IsLit(new Vector3(inShade.x, 0f, shade.z - 0.5f))}");
        player.Teleport(inShade, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);
        yield return Shot("fire_sunblind_0");
        float apart = (priest.transform.position - inShade).magnitude;
        Say($"   player in shade {apart:0.0} m from him for 1.5 s: found {priest.HasFound} state {priest.Brain.State}");
        float ph = priest.Health.Current;
        priest.TakeHit(new Hit(18f, 10f, Vector3.back, true));
        Say($"   a heavy fist blow (18) on a priest who has not found the player took {ph - priest.Health.Current:0} and {priest.Stagger.Value:0} balance; found now {priest.HasFound} (the heat of a fight)");
        priest.Restore();
        yield return new WaitForSeconds(0.2f);

        // In the sun: found, and the ray.
        defence.Restore();
        player.Teleport(inSun, Quaternion.identity);
        float litAt = Time.time;
        yield return Until(() => priest.HasFound, 2f);
        float foundAfter = Time.time - litAt;
        int struck = priest.BlowsStruck;
        int takenRay = defence.BlowsTaken;
        yield return Until(() => priest.Brain.TelegraphProgress > 0.8f, 6f);
        string showing = priest.Brain.Current != null ? priest.Brain.Current.Name : "nothing";
        yield return Shot("fire_sunblind_1");
        yield return Until(() => priest.BlowsStruck > struck, 3f);
        yield return new WaitForSeconds(0.15f);
        yield return Shot("fire_sunblind_2");
        Say($"   player in the sun 9.5 m off: found after {foundAfter:0.00} s; he showed '{showing}'; it landed on a still player +{defence.BlowsTaken - takenRay}");
        // Moving out of the circle.
        yield return Until(() => priest.Brain.State == EnemyState.Telegraph, 6f);
        takenRay = defence.BlowsTaken;
        Keys(Key.A);
        yield return Until(() => priest.Brain.State != EnemyState.Telegraph, 3f);
        Keys();
        yield return new WaitForSeconds(0.1f);
        Say($"   the next ray walked out of: landed +{defence.BlowsTaken - takenRay}");

        // Into the shade: lost; he goes to where they were, searches, and gives up.
        yield return Until(() => priest.Brain.State == EnemyState.Stalk, 3f);
        Vector3 lastSeen = player.transform.position;
        player.Teleport(inShade, Quaternion.identity);
        float lostAt = Time.time;
        yield return new WaitForSeconds(0.3f);
        bool lost = !priest.HasFound;
        int raysBefore = priest.BlowsStruck;
        yield return Until(() => priest.IsSearching, 12f);
        float searchingAfter = Time.time - lostAt;
        float fromLast = (priest.transform.position - lastSeen).magnitude;
        yield return Shot("fire_sunblind_3");
        yield return Until(() => priest.Brain.State == EnemyState.Idle, 8f);
        Say($"   player steps into shade: lost {lost}; searching after {searchingAfter:0.0} s, {fromLast:0.0} m from where they were last lit; gave up after {Time.time - lostAt:0.0} s (state {priest.Brain.State}); rays thrown meanwhile +{priest.BlowsStruck - raysBefore}");
        // At arm's length: found by heat, even in shade.
        player.Teleport(priest.transform.position + priest.transform.forward * 2f, Quaternion.identity);
        yield return new WaitForSeconds(0.2f);
        Say($"   at 2 m: found {priest.HasFound} (the heat of a body)");
        yield return Still(priest, "sunblind_brand", 2f);
        yield return Guarded(priest, "sunblind_brand", 2f);
        priest.Restore();
        yield return Still(priest, "sunblind_flare", 2f, "fire_sunblind_4");
        Say($"   after the flare the ground round him counts as lit: {Sunlight.IsLit(priest.transform.position)}");
        priest.Restore();
        yield return Kill(priest, "fire_sunblind_5");

        // The yard as a whole.
        foreach (var enemy in director.Enemies) enemy.Restore();
        Vector3[] views = { new Vector3(0f, 0.05f, 2f), new Vector3(-15f, 0.05f, 20f), new Vector3(-9f, 0.05f, 29f), new Vector3(2f, 0.05f, 33f), new Vector3(27f, 0.05f, 27f), new Vector3(10f, 0.05f, 8f) };
        for (int i = 0; i < views.Length; i++)
        {
            foreach (var enemy in director.Enemies) enemy.Restore();
            player.Teleport(views[i], Quaternion.identity);
            yield return new WaitForSeconds(0.9f);
            yield return Shot("fire_yard_" + (i + 2));
        }
        Say($"embers carried at the end {Clover.Core.RunState.Points}; has element {element.HasElement}");
        Say($"done: time scale {Time.timeScale:0.00}");
        Destroy(gameObject);
    }
}
