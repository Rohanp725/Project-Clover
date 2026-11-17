using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Play Mode probe for falling everywhere a run can end, and for the pause menu in the places the pause probe did not
// try, and for the practice arena's standing up again. After each fall it checks the hub and the state the next run
// would begin with. It forgets saved progress before and after. Run in Play Mode, in the Hub scene.
public static class FallProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Fall Probe").AddComponent<FallProbeRunner>();
        return "probe started";
    }
}

public class FallProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private bool keepAlive = true;
    private int errors, fails;
    private readonly List<string> firstErrors = new List<string>();
    private float hubCam;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private void Check(bool ok, string what) { if (!ok) { fails++; Say("FAIL " + what); } }
    private static PauseMenu Menu => PauseMenu.Instance;

    private void OnLog(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        errors++;
        string head = message.Length > 160 ? message.Substring(0, 160) : message;
        foreach (string seen in firstErrors) if (seen.StartsWith(head)) return;
        if (firstErrors.Count < 5) firstErrors.Add(head + " || " + (stack.Length > 700 ? stack.Substring(0, 700) : stack).Replace("\n", " | "));
    }

    private void OnEnable() => Application.logMessageReceived += OnLog;
    private void OnDisable() => Application.logMessageReceived -= OnLog;

    private static void ForgetAll()
    {
        Progress.Forget(Progress.MetSponsor);
        Progress.Forget(Progress.WeaponKey(Weapons.KatanaName));
        Progress.Forget(Progress.ElementKey(ElementKit.LightningName));
    }

    private void Update()
    {
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
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

    private void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
    }

    private IEnumerator Load(string scene)
    {
        keepAlive = true;
        SceneManager.LoadScene(scene);
        yield return null;
        yield return null;
        FindPlayer();
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ReadAll(float wait)
    {
        yield return Until(() => ReadingBox.Instance != null && ReadingBox.Instance.IsOpen, wait);
        for (int i = 0; i < 60 && ReadingBox.Instance != null && ReadingBox.Instance.IsOpen; i++) yield return Tap(Key.E);
    }

    private TopDownCamera View => FindAnyObjectByType<TopDownCamera>();
    private float CamOff() => Vector3.Distance(View.transform.position, player.transform.position);

    private static string Hud()
    {
        var hud = FindAnyObjectByType<PlayerHud>();
        if (hud == null) return "no hud";
        var field = typeof(PlayerHud).GetField("embers", BindingFlags.NonPublic | BindingFlags.Instance);
        var text = field != null ? field.GetValue(hud) as UnityEngine.UI.Text : null;
        return text != null ? text.text : "no text";
    }

    private string State() =>
        $"scene {SceneManager.GetActiveScene().name} time {Time.timeScale:0.00} suspended {player.Suspended} dead {defence.IsDead} pause {(Menu != null && Menu.IsOpen)} run[w '{RunState.Weapon}' e '{RunState.Element}' gods {RunState.GodsFelled} pts {RunState.Points} learned {RunState.Unlocked.Count}] cam {CamOff():0.00} framing {View.HasFraming} errors {errors}";

    // Something for the fall to take away.
    private void Carry()
    {
        combat.Equip(Weapons.ByName(Weapons.KatanaName));
        RunState.Points += 5;
        RunState.Unlocked.Add(Upgrade.Heat);
    }

    private string Kill()
    {
        keepAlive = false;
        var outcome = defence.TakeHit(new Hit(1000f, 0f, Vector3.back, true, true));
        if (defence.IsDead) return "blow " + outcome;
        defence.Health.Damage(100000f);
        return "blow " + outcome + ", then health taken";
    }

    // After a fall: the dark, the hub, and everything a new run needs put back.
    private IEnumerator Woke(string label, string how, int runsBefore = -1)
    {
        int runs = runsBefore >= 0 ? runsBefore : RunState.RunsEnded;
        string fromScene = SceneManager.GetActiveScene().name;
        float began = Time.unscaledTime;
        var old = player;
        yield return Until(() => old == null && SceneManager.GetActiveScene().name == "Hub", 9f);
        float took = Time.unscaledTime - began;
        yield return null;
        yield return null;
        FindPlayer();
        keepAlive = true;
        yield return ReadAll(0.8f);
        yield return new WaitForSecondsRealtime(0.8f);
        Say($"{label}: fell in {fromScene} by {how}; hub after {took:0.0} s; runs ended +{RunState.RunsEnded - runs} fell {RunState.LastRunFell}; {State()} hud '{Hud()}'");
        Check(SceneManager.GetActiveScene().name == "Hub", label + ": not in the hub");
        Check(Time.timeScale == 1f, label + ": time scale " + Time.timeScale);
        Check(!player.Suspended && !defence.IsDead, label + ": suspended or dead in the hub");
        Check(RunState.Points == 0 && RunState.Unlocked.Count == 0 && RunState.GodsFelled == 0 && RunState.Element == "" && !RunState.ElementConsumed, label + ": run state not reset");
        Check(combat.Weapon.Name == Weapons.FistsName, label + ": weapon in the hub is " + combat.Weapon.Name);
        Check(RunState.RunsEnded == runs + 1, label + $": runs ended went up by {RunState.RunsEnded - runs}");
        Check(Mathf.Abs(CamOff() - hubCam) < 0.6f && !View.HasFraming, $"{label}: camera {CamOff():0.00} against {hubCam:0.00}");
        Check((Menu == null || !Menu.IsOpen) && (UpgradeMenu.Instance == null || !UpgradeMenu.Instance.IsOpen), label + ": a menu is open in the hub");
        Check(Hud().EndsWith(" 0"), label + ": hud says '" + Hud() + "'");
    }

    // The next run, through the gate with the use button, and a blow landed.
    private IEnumerator NextRun(string label)
    {
        var gate = FindAnyObjectByType<SceneDoor>();
        for (int attempt = 0; attempt < 5 && SceneManager.GetActiveScene().name != "Castle"; attempt++)
        {
            player.Teleport(gate.transform.position + new Vector3(0f, 0.05f, -1.8f), Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            yield return Tap(Key.E);
            yield return new WaitForSecondsRealtime(0.3f);
        }
        yield return null;
        yield return null;
        FindPlayer();
        yield return new WaitForSeconds(0.4f);
        Enemy foe = null;
        foreach (var enemy in FindAnyObjectByType<AreaDirector>().Enemies)
            if (!enemy.IsDead && (foe == null || (enemy.transform.position - player.transform.position).sqrMagnitude < (foe.transform.position - player.transform.position).sqrMagnitude)) foe = enemy;
        int blows = combat.BlowsLanded;
        for (int attempt = 0; attempt < 6 && combat.BlowsLanded == blows && foe != null; attempt++)
        {
            Vector3 gap = foe.transform.position - player.transform.position;
            gap.y = 0f;
            player.Teleport(foe.transform.position - gap.normalized * 1.6f + Vector3.up * 0.05f, Quaternion.LookRotation(gap.normalized, Vector3.up));
            yield return null;
            yield return Tap(Key.J);
            yield return new WaitForSeconds(0.6f);
        }
        Say($"{label}: next run: blows landed {combat.BlowsLanded - blows}; {State()}");
        Check(SceneManager.GetActiveScene().name == "Castle" && combat.BlowsLanded > blows, label + ": the next run did not land a blow in the castle");
        Check(RunState.Unlocked.Count == 0 && RunState.Points == 0 && combat.Weapon.Name == Weapons.FistsName, label + ": the next run was not clean");
    }

    private Enemy Find(Func<Enemy, bool> which)
    {
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            if (which(enemy)) return enemy;
        return null;
    }

    private IEnumerator StageFinisher()
    {
        combat.Equip(Weapons.ByName(Weapons.KatanaName));
        Enemy enemy = Find(e => e.Kind.Name == EnemyKind.SoldierName && !e.IsDead);
        player.Teleport(enemy.transform.position + enemy.transform.forward * 2.5f, Quaternion.LookRotation(-enemy.transform.forward, Vector3.up));
        yield return new WaitForSeconds(0.6f);
        enemy.TakeHit(new Hit(1f, 1000f, Vector3.forward));
        yield return null;
        Keys(Key.K);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => combat.IsFinishing, 1f);
    }

    private IEnumerator StageLeap()
    {
        RunState.Unlocked.Add(Upgrade.MeteorLeap);
        yield return new WaitForSeconds(0.4f);
        combat.Mana.Gain(100f);
        Keys(Key.U);
        yield return Until(() => player.IsLeaping, 2f);
        Keys();
        yield return null;
    }

    private GodlyForm colossus;
    private IEnumerator ToColossus(string doing)
    {
        yield return Load("Summit");
        Carry();
        Enemy boss = Find(e => e.Kind.IsBoss);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        colossus = FindAnyObjectByType<GodlyForm>();
        yield return Until(() => colossus.Doing == doing, 16f);
    }

    private IEnumerator PauseHere(string label)
    {
        var view = View;
        bool framing = view.HasFraming;
        string doing = colossus != null ? colossus.Doing : "-";
        yield return Tap(Key.Escape);
        bool open = Menu.IsOpen;
        float stopped = Time.timeScale;
        Vector3 cameraAt = view.transform.position, playerAt = player.transform.position;
        yield return new WaitForSecondsRealtime(0.6f);
        float cameraDrift = Vector3.Distance(cameraAt, view.transform.position), playerDrift = Vector3.Distance(playerAt, player.transform.position);
        bool framingHeld = view.HasFraming;
        yield return Tap(Key.Escape);
        yield return null;
        yield return null;
        Say($"{label}: (storm {doing}) opened {open} time {stopped:0.00} framing before {framing} while paused {framingHeld} camera moved {cameraDrift:0.000} m player moved {playerDrift:0.000} m; resumed: {State()}");
        Check(open, label + ": the menu did not open");
        Check(!Menu.IsOpen && !player.Suspended && Time.timeScale > 0f, label + ": did not resume");
        Check(framing == framingHeld && framing == view.HasFraming, label + ": the framing changed");
        Check(playerDrift < 0.01f, label + ": the player moved while paused");
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        ForgetAll();
        yield return Load("Hub");
        yield return ReadAll(4f);
        yield return new WaitForSeconds(0.8f);
        hubCam = CamOff();
        Say($"start: {State()}");

        // With this file in the temporary folder only the last two parts are played, to look at them again quickly.
        bool onlyTheEnd = System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "loop_fall_end_only.txt"));
        string how = "";
        int finishers = 0;
        if (!onlyTheEnd)
        {
            // A: to the Gate Captain.
            yield return Load("Castle");
            Carry();
            Enemy captain = Find(e => e.Kind.Name == EnemyKind.CaptainName);
            Check(captain != null, "no Gate Captain in the castle");
            if (captain != null)
            {
                keepAlive = false;
                defence.Health.Damage(defence.Health.Current - 1f);
                player.Teleport(captain.transform.position + captain.transform.forward * 2f + Vector3.up * 0.05f, Quaternion.LookRotation(-captain.transform.forward, Vector3.up));
                yield return Until(() => defence.IsDead, 8f);
            }
            how = defence.IsDead ? "the captain's own blow" : Kill();
            yield return Woke("A captain", how);
            yield return NextRun("A captain");

            // B: with the fire's menu just opened.
            Carry();
            RestPoint fire = null;
            foreach (var rest in FindObjectsByType<RestPoint>(FindObjectsSortMode.None))
                if (rest.transform.position.y > 1f) fire = rest;
            for (int attempt = 0; attempt < 5 && !UpgradeMenu.Instance.IsOpen; attempt++)
            {
                player.Teleport(fire.transform.position + new Vector3(1.5f, 0.1f, 0f), Quaternion.identity);
                yield return new WaitForSeconds(0.5f);
                yield return Tap(Key.E);
                yield return new WaitForSecondsRealtime(0.3f);
            }
            bool menuOpen = UpgradeMenu.Instance.IsOpen;
            how = Kill() + $" with the fire's menu open {menuOpen} time {Time.timeScale:0.00}";
            yield return Woke("B fire menu open", how);

            // B2: the menu closed and the fall in the same moment.
            yield return Load("Castle");
            Carry();
            UpgradeMenu.Instance.Open();
            yield return null;
            yield return Tap(Key.Space);
            how = Kill() + $" just after the fire's menu closed (suspended {player.Suspended})";
            yield return Woke("B2 fire menu closed", how);

            // C: in the air.
            yield return Load("Castle");
            Carry();
            yield return StageLeap();
            bool leaping = player.IsLeaping;
            how = Kill() + $" in the air {leaping}";
            yield return Woke("C meteor leap", how);

            // D: mid-finisher, in the wind-up and in the slow after the cut.
            yield return Load("Castle");
            Carry();
            yield return StageFinisher();
            how = Kill() + $" in a finisher's wind-up {combat.IsFinishing}";
            yield return Woke("D finisher wind-up", how);
            yield return Load("Castle");
            Carry();
            finishers = combat.Finishers;
            yield return StageFinisher();
            yield return Until(() => combat.Finishers > finishers, 2f);
            yield return null;
            how = Kill() + $" in a finisher's slow (landed {combat.Finishers > finishers} time {Time.timeScale:0.00})";
            yield return Woke("D2 finisher slow", how);

            // E: pause around the fall.
            yield return Load("Castle");
            Carry();
            yield return Tap(Key.Escape);
            bool wasOpen = Menu.IsOpen;
            yield return Tap(Key.Escape);
            how = Kill() + $" straight after a pause (opened {wasOpen})";
            yield return new WaitForSecondsRealtime(1.8f);
            yield return Tap(Key.Escape);
            bool refused = !Menu.IsOpen;
            Check(refused, "E: the pause menu opened during the death fade");
            yield return Tap(Key.Escape);
            yield return Woke("E pause then fall", how + $"; pause refused in the fade {refused}");
            yield return Load("Castle");
            Carry();
            yield return Tap(Key.Escape);
            how = Kill() + $" with the pause menu open {Menu.IsOpen}";
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Tap(Key.Escape);
            yield return Woke("E2 fall while paused, then closed", how);
            yield return NextRun("E2");

            // F: at the instant of a door.
            foreach (var enemy in FindAnyObjectByType<AreaDirector>().Enemies)
                for (int blow = 0; blow < 60 && !enemy.IsDead; blow++) enemy.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
            yield return new WaitForSeconds(0.5f);
            var way = FindAnyObjectByType<SealedWay>();
            player.Teleport(way.transform.position + new Vector3(0f, 0.1f, -1f), Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            int runsBefore = RunState.RunsEnded;
            Keys(Key.E);
            yield return null;
            how = Kill() + " on the frame after the door's button";
            yield return null;
            Keys();
            yield return new WaitForSecondsRealtime(5f);
            FindPlayer();
            keepAlive = true;
            Say($"F door and fall together ({how}): {State()} runs ended +{RunState.RunsEnded - runsBefore}");
            Check(!defence.IsDead && Time.timeScale == 1f && !player.Suspended, "F: left dead, stopped or suspended after the door");
            Check(SceneManager.GetActiveScene().name == "Summit" || SceneManager.GetActiveScene().name == "Hub", "F: in neither the summit nor the hub");

            // G: to the swordsman.
            yield return Load("Summit");
            Carry();
            float summitCam = CamOff();
            yield return new WaitForSeconds(1f);
            yield return PauseHere("P1 pause, swordsman fight");
            yield return Woke("G swordsman", Kill());

            // H: the storm's entrance, fight, stagger and death; a pause in each of the first two.
            yield return ToColossus("Rising");
            yield return new WaitForSeconds(0.8f);
            yield return PauseHere("P2 pause, the storm rising");
            how = Kill() + $" while the storm was {colossus.Doing}";
            yield return Woke("H storm rising", how);

            yield return ToColossus("Idle");
            yield return new WaitForSeconds(1.5f);
            float fightCam = CamOff();
            yield return PauseHere("P3 pause, the storm's fight");
            yield return new WaitForSeconds(1f);
            Say($"P3: camera before the pause {fightCam:0.00}, a second after {CamOff():0.00}, at arrival {summitCam:0.00}; framing {View.HasFraming}");
            Check(Mathf.Abs(CamOff() - fightCam) < 1.5f && View.HasFraming, "P3: the storm's framing did not survive the pause");
            how = Kill() + $" while the storm was {colossus.Doing}";
            yield return Woke("H2 storm fight", how);

            yield return ToColossus("Idle");
            colossus.Unbalance(10000f);
            yield return new WaitForSeconds(0.5f);
            how = Kill() + $" while the storm was {colossus.Doing}";
            yield return Woke("H3 storm staggered", how);

            yield return ToColossus("Idle");
            for (int attempt = 0; attempt < 12 && !colossus.IsDead; attempt++)
            {
                yield return Until(() => colossus.Doing == "Idle", 12f);
                colossus.Unbalance(10000f);
                yield return new WaitForSeconds(0.7f);
                for (int blow = 0; blow < 40 && !colossus.IsDead; blow++) colossus.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
            }
            yield return new WaitForSeconds(0.4f);
            how = Kill() + $" while the storm was {colossus.Doing} gods {RunState.GodsFelled}";
            yield return Woke("H4 storm dying", how);
            Say($"H4: last run gods {RunState.LastRunGods}");
            yield return NextRun("H4");

            // I: pause in the air and in a finisher's slow.
            yield return StageLeap();
            bool inAir = player.IsLeaping;
            float progress = player.LeapProgress;
            yield return PauseHere($"P4 pause in the air (leaping {inAir} at {progress:0.00})");
            Check(inAir, "P4: was not in the air");
            yield return Until(() => !player.IsLeaping, 3f);
            Check(!player.IsLeaping, "P4: the leap never came down after the pause");
            yield return new WaitForSeconds(0.6f);
            RunState.Unlocked.Remove(Upgrade.MeteorLeap);
            finishers = combat.Finishers;
            yield return StageFinisher();
            yield return Until(() => combat.Finishers > finishers, 2f);
            yield return null;
            float slow = Time.timeScale;
            yield return PauseHere($"P5 pause in a finisher's slow (landed {combat.Finishers > finishers}, time {slow:0.00})");
            yield return new WaitForSecondsRealtime(1.2f);
            Say($"P5: 1.2 s later time {Time.timeScale:0.00} finishing {combat.IsFinishing} attacking {combat.IsAttacking}");
            Check(Time.timeScale == 1f && !combat.IsFinishing, "P5: time or the finisher was left hanging after the pause");
        }

        // J: give up the run from the summit, in the storm's fight.
        yield return ToColossus("Idle");
        yield return new WaitForSeconds(1f);
        int runsAtGiveUp = RunState.RunsEnded;
        string gaveUpIn = SceneManager.GetActiveScene().name + " with the storm " + colossus.Doing + " framing " + View.HasFraming;
        yield return Tap(Key.Escape);
        yield return Tap(Key.S);
        yield return Tap(Key.S);
        var entry = Menu.Choices.Current;
        yield return Tap(Key.E);
        yield return Tap(Key.E);
        yield return Woke("J give up from the summit", "the menu's " + entry + " in " + gaveUpIn, runsAtGiveUp);

        // K: the practice arena stands the player up again.
        yield return Load("CombatTest");
        int runsEnded = RunState.RunsEnded;
        int entries = Menu != null ? Menu.Choices.Count : -1;
        bool ender = FindAnyObjectByType<RunEnder>() != null;
        yield return PauseHere("K arena pause");
        defence.TakeHit(new Hit(5f, 0f, Vector3.back));
        yield return new WaitForSeconds(1f);
        defence.TakeHit(new Hit(5f, 0f, Vector3.back));
        yield return new WaitForSeconds(1f);
        string tips = $"guard tip shown {Tips.Rules.WasShown(Tip.Guard)}";
        for (int round = 0; round < 2; round++)
        {
            how = Kill();
            int deaths = defence.Deaths;
            yield return Tap(Key.Escape);
            bool refusedDead = !Menu.IsOpen;
            yield return Tap(Key.Escape);
            yield return Until(() => !defence.IsDead, defence.ReviveAfterSeconds + 3f);
            keepAlive = true;
            yield return new WaitForSeconds(0.5f);
            int blows = combat.BlowsLanded;
            yield return Tap(Key.J);
            yield return new WaitForSeconds(0.6f);
            Say($"K arena fall {round + 1}: {how}; run ender present {ender}; pause refused while down {refusedDead}; stood up {!defence.IsDead} health {defence.Health.Fraction:0.00} revive after {defence.ReviveAfterSeconds:0.0} s; runs ended +{RunState.RunsEnded - runsEnded}; pause entries {entries}; {tips}; {State()}");
            Check(!defence.IsDead && SceneManager.GetActiveScene().name == "CombatTest" && RunState.RunsEnded == runsEnded && Time.timeScale == 1f && !player.Suspended, "K: the arena did not stand the player up cleanly");
        }
        yield return PauseHere("K arena pause after standing up");

        RunState.Reset();
        ForgetAll();
        foreach (string error in firstErrors) Say("ERROR " + error);
        Say($"totals: errors {errors} failed checks {fails}");
        Say("done");
        Destroy(gameObject);
    }
}
