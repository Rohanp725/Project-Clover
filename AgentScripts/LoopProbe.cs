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

// Play Mode probe for a second loop: a run begun with the Katana and Lightning already won, the Lightning kit and its
// upgrades played in the castle and on the summit, then each of the god's rewards followed by the road home and a
// fresh start. It forgets saved progress before and after. Run in Play Mode, in the Hub scene.
public static class LoopProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Loop Probe").AddComponent<LoopProbeRunner>();
        return "probe started";
    }
}

public class LoopProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private int errors, fails;
    private readonly List<string> firstErrors = new List<string>();
    private float hubCam;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);
    private void Check(bool ok, string what) { if (!ok) { fails++; Say("FAIL " + what); } }

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
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
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

    private IEnumerator Arrive(string scene)
    {
        yield return Until(() => SceneManager.GetActiveScene().name == scene, 8f);
        yield return null;
        yield return null;
        FindPlayer();
        Check(SceneManager.GetActiveScene().name == scene, $"expected scene {scene}, in {SceneManager.GetActiveScene().name}");
    }

    private IEnumerator ReadAll(float wait)
    {
        yield return Until(() => ReadingBox.Instance != null && ReadingBox.Instance.IsOpen, wait);
        for (int i = 0; i < 60 && ReadingBox.Instance != null && ReadingBox.Instance.IsOpen; i++) yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);
    }

    private float CamOff()
    {
        var view = FindAnyObjectByType<TopDownCamera>();
        return view != null ? Vector3.Distance(view.transform.position, player.transform.position) : -1f;
    }

    private static string Hud()
    {
        var hud = FindAnyObjectByType<PlayerHud>();
        if (hud == null) return "no hud";
        var field = typeof(PlayerHud).GetField("embers", BindingFlags.NonPublic | BindingFlags.Instance);
        var text = field != null ? field.GetValue(hud) as UnityEngine.UI.Text : null;
        return text != null ? text.text : "no text";
    }

    private string State() =>
        $"scene {SceneManager.GetActiveScene().name} time {Time.timeScale:0.00} suspended {player.Suspended} weapon {combat.Weapon.Name} element {element.Kit.Name} run[w '{RunState.Weapon}' e '{RunState.Element}' consumed {RunState.ElementConsumed} gods {RunState.GodsFelled} pts {RunState.Points} learned [{string.Join(",", RunState.Unlocked)}]] hud '{Hud()}' cam {CamOff():0.00} errors {errors}";

    private IEnumerator Use(Component thing, Vector3 offset, Func<bool> worked)
    {
        for (int attempt = 0; attempt < 5 && !worked(); attempt++)
        {
            if (thing == null) yield break;
            player.Teleport(thing.transform.position + offset, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            yield return Tap(Key.E);
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    private static LoadoutChoice Stand(string choice)
    {
        foreach (var stand in FindObjectsByType<LoadoutChoice>(FindObjectsSortMode.None))
            if (stand.Choice == choice) return stand;
        return null;
    }

    private static void FellAll(AreaDirector director)
    {
        foreach (var enemy in director.Enemies)
            for (int blow = 0; blow < 60 && !enemy.IsDead; blow++) enemy.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
    }

    // The player's own buttons against whatever is near: blows, heavy blows, rolls, the spell, the surge and the held spell.
    private IEnumerator Brawl(float seconds, Func<Transform> near)
    {
        float end = Time.unscaledTime + seconds;
        int turn = 0;
        while (Time.unscaledTime < end)
        {
            combat.Mana.Gain(100f);
            Transform foe = near();
            if (foe != null && turn % 4 == 0)
            {
                Vector3 gap = foe.position - player.transform.position;
                gap.y = 0f;
                if (gap.magnitude > 3.5f)
                {
                    Vector3 at = foe.position - gap.normalized * 2.2f;
                    at.y = player.transform.position.y + 0.05f;
                    player.Teleport(at, Quaternion.LookRotation(gap.normalized, Vector3.up));
                }
            }
            switch (turn % 9)
            {
                case 0: yield return Tap(Key.J); yield return new WaitForSeconds(0.25f); yield return Tap(Key.J); break;
                case 1: yield return Tap(Key.U); break;
                case 2: Keys(Key.Space, Key.W); yield return null; yield return null; Keys(); break;
                case 3: yield return Tap(Key.I); break;
                case 4: yield return Tap(Key.J); yield return new WaitForSeconds(0.2f); yield return Tap(Key.K); break;
                case 5: Keys(Key.U); yield return new WaitForSeconds(0.9f); Keys(); break;
                case 6: Keys(Key.Space, Key.A); yield return null; yield return null; Keys(); break;
                case 7: Keys(Key.J); yield return new WaitForSeconds(0.8f); Keys(); break;
                case 8: yield return Tap(Key.K); break;
            }
            yield return new WaitForSeconds(0.35f);
            turn++;
        }
        Keys();
        yield return new WaitForSeconds(0.5f);
    }

    private Transform NearestFoe()
    {
        Transform best = null;
        float bestGap = float.MaxValue;
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (enemy.IsDead || !enemy.gameObject.activeInHierarchy) continue;
            float gap = (enemy.transform.position - player.transform.position).sqrMagnitude;
            if (gap < bestGap) { bestGap = gap; best = enemy.transform; }
        }
        return best;
    }

    private IEnumerator FellGods(bool brawl)
    {
        var summit = FindAnyObjectByType<SummitDirector>();
        Enemy boss = null;
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            if (enemy.Kind.IsBoss) boss = enemy;
        if (brawl)
        {
            yield return Brawl(7f, NearestFoe);
            Say($"summit, the swordsman fought with the kit: boss health {boss.Health.Fraction:0.00} spells {element.SpellsCast} surges {element.Surges} discharges {upgrades.Discharges} free dodges {upgrades.FreeDodges} blows {combat.BlowsLanded}; {State()}");
        }
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        var colossus = FindAnyObjectByType<GodlyForm>();
        yield return Until(() => colossus.Doing == "Idle", 14f);
        if (brawl)
        {
            var head = colossus.HeadPart.transform;
            yield return Brawl(7f, () => null);
            Say($"summit, the storm fought with the kit: doing {colossus.Doing} health {colossus.Health.Fraction:0.00} spells {element.SpellsCast} surges {element.Surges} discharges {upgrades.Discharges}; {State()}");
        }
        for (int attempt = 0; attempt < 12 && !colossus.IsDead; attempt++)
        {
            yield return Until(() => colossus.Doing == "Idle", 12f);
            colossus.Unbalance(10000f);
            yield return new WaitForSeconds(0.7f);
            for (int blow = 0; blow < 40 && !colossus.IsDead; blow++) colossus.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
        }
        yield return Until(() => summit.Felled, 10f);
        yield return new WaitForSecondsRealtime(summit.RewardDelay + 2.5f);
        Check(boss.IsDead && colossus.IsDead && summit.Felled, "the god did not fall");
    }

    private IEnumerator Choose(bool takes)
    {
        GodReward wanted = null;
        foreach (var reward in FindObjectsByType<GodReward>(FindObjectsSortMode.None))
            if (reward.Takes == takes) wanted = reward;
        Check(wanted != null, "no reward found");
        if (wanted == null) yield break;
        yield return Use(wanted, new Vector3(0f, 0.1f, -1.4f), () => wanted.Chosen);
        yield return new WaitForSeconds(0.4f);
        Check(wanted.Chosen, "the reward was not chosen");
    }

    private IEnumerator Home()
    {
        var door = FindAnyObjectByType<SceneDoor>();
        yield return Use(door, new Vector3(0f, 0.1f, 1f), () => SceneManager.GetActiveScene().name == "Hub");
        yield return Arrive("Hub");
        yield return ReadAll(1.2f);
        yield return new WaitForSeconds(0.6f);
    }

    private void CheckFresh(string label)
    {
        Check(Time.timeScale == 1f, label + ": time scale " + Time.timeScale);
        Check(!player.Suspended, label + ": player suspended");
        Check(RunState.Points == 0 && RunState.Unlocked.Count == 0 && RunState.GodsFelled == 0 && !RunState.ElementConsumed && RunState.Element == "", label + ": run state not reset");
        Check(combat.Weapon != null && element.Kit != null, label + ": no weapon or element");
        Check(Mathf.Abs(CamOff() - hubCam) < 0.6f, $"{label}: camera {CamOff():0.00} against {hubCam:0.00}");
        Check(Hud().EndsWith(" " + RunState.Points), $"{label}: hud '{Hud()}' against {RunState.Points}");
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        ForgetAll();
        Progress.Unlock(Progress.WeaponKey(Weapons.KatanaName));
        Progress.Unlock(Progress.ElementKey(ElementKit.LightningName));
        SceneManager.LoadScene("Hub");
        yield return null;
        yield return null;
        FindPlayer();
        yield return ReadAll(4f);
        yield return new WaitForSeconds(0.6f);
        hubCam = CamOff();

        // 1: the stand and the brazier, with the use button.
        var katana = Stand(Weapons.KatanaName);
        var storm = Stand(ElementKit.LightningName);
        Check(katana != null && katana.IsUnlocked && storm != null && storm.IsUnlocked, "the stand or the brazier is not open");
        yield return Use(katana, new Vector3(1.3f, 0.05f, 0.6f), () => katana.Taken > 0);
        yield return Use(storm, new Vector3(-1.3f, 0.05f, 0.6f), () => storm.Taken > 0);
        Say($"1 hub loadout: katana taken {katana.Taken} storm taken {storm.Taken}; {State()}");
        Check(combat.Weapon.Name == Weapons.KatanaName && element.Kit.Name == ElementKit.LightningName, "the loadout was not taken");
        var gate = FindAnyObjectByType<SceneDoor>();
        yield return Use(gate, new Vector3(0f, 0.05f, -1.8f), () => SceneManager.GetActiveScene().name == "Castle");
        yield return Arrive("Castle");
        Say($"2 castle arrived: {State()}");
        Check(combat.Weapon.Name == Weapons.KatanaName && element.Kit.Name == ElementKit.LightningName, "the loadout was not carried through the gate");

        // 2: the plain Lightning kit against the castle.
        var director = FindAnyObjectByType<AreaDirector>();
        yield return Brawl(7f, NearestFoe);
        Say($"3 castle, plain kit: spells {element.SpellsCast} spell hits {element.SpellHits} surges {element.Surges} blows {combat.BlowsLanded} alive {director.EnemiesAlive}; {State()}");
        Check(element.SpellsCast > 0 && element.Surges > 0, "the spell or the surge never went off");
        FellAll(director);
        yield return new WaitForSeconds(2.5f);
        int earned = RunState.Points;
        Check(Hud().EndsWith(" " + RunState.Points), $"hud '{Hud()}' against {RunState.Points} after the castle");

        // 3: the fire's menu, bought by hand.
        RestPoint fire = null;
        foreach (var rest in FindObjectsByType<RestPoint>(FindObjectsSortMode.None))
            if (rest.transform.position.y > 1f) fire = rest;
        yield return Use(fire, new Vector3(1.5f, 0.1f, 0f), () => UpgradeMenu.Instance.IsOpen);
        bool opened = UpgradeMenu.Instance.IsOpen;
        for (int i = 0; i < 8; i++)
        {
            yield return Tap(Key.E);
            yield return Tap(Key.S);
        }
        string bought = string.Join(",", RunState.Unlocked);
        yield return Tap(Key.Space);
        yield return new WaitForSecondsRealtime(0.4f);
        Say($"4 rest fire: menu opened {opened} embers {earned} bought [{bought}] left {RunState.Points}; {State()}");
        Check(opened && !UpgradeMenu.Instance.IsOpen && Time.timeScale == 1f && !player.Suspended, "the fire's menu did not open and close cleanly");
        foreach (string id in RunState.Unlocked) Check(id.StartsWith("lightning."), "bought something not of Lightning: " + id);
        yield return Brawl(6f, NearestFoe);
        Say($"5 castle, bought upgrades: discharges {upgrades.Discharges} free dodges {upgrades.FreeDodges} spells {element.SpellsCast} surges {element.Surges}; {State()}");

        // 4: every Lightning upgrade at once.
        foreach (var upgrade in Upgrade.All)
            if (upgrade.Element == ElementKit.LightningName) RunState.Unlocked.Add(upgrade.Id);
        yield return new WaitForSeconds(0.4f);
        string have = "";
        foreach (var upgrade in Upgrade.All)
            if (upgrade.Element == ElementKit.LightningName) have += $"{upgrade.Id}={upgrades.Has(upgrade.Id)} ";
        yield return Brawl(12f, NearestFoe);
        Say($"6 castle, all of Lightning: {have}; discharges {upgrades.Discharges} free dodges {upgrades.FreeDodges} spells {element.SpellsCast} surges {element.Surges} blows {combat.BlowsLanded} alive {director.EnemiesAlive}; {State()}");
        FellAll(director);
        yield return new WaitForSeconds(1.5f);

        // 5: the summit with the whole kit.
        var way = FindAnyObjectByType<SealedWay>();
        yield return Use(way, new Vector3(0f, 0.1f, -1f), () => SceneManager.GetActiveScene().name == "Summit");
        yield return Arrive("Summit");
        Say($"7 summit arrived: {State()}");
        Check(combat.Weapon.Name == Weapons.KatanaName && element.Kit.Name == ElementKit.LightningName && RunState.Unlocked.Contains(Upgrade.LightningRod), "the kit was not carried to the summit");
        float summitCam = CamOff();
        yield return FellGods(true);
        yield return new WaitForSeconds(1.5f);
        Say($"8 the god fell: camera {CamOff():0.00} (at arrival {summitCam:0.00}); {State()}");

        // 6: take, already holding Lightning.
        int before = RunState.Points;
        string learnedBefore = string.Join(",", RunState.Unlocked);
        yield return Choose(true);
        Say($"9 TAKE holding Lightning: before pts {before} learned [{learnedBefore}]; after {State()}");
        Check(element.Kit != null && element.Kit.Name == ElementKit.LightningName && combat.Weapon != null && combat.Weapon.Name == Weapons.StormKatanaName, "take left the wrong element or weapon");
        yield return Brawl(3f, () => null);
        yield return Home();
        Say($"10 home after take: runs ended {RunState.RunsEnded} fell {RunState.LastRunFell} gods {RunState.LastRunGods}; {State()}");
        CheckFresh("home after take");

        // 7: consume holding Lightning.
        storm = Stand(ElementKit.LightningName);
        yield return Use(storm, new Vector3(-1.3f, 0.05f, 0.6f), () => storm.Taken > 0);
        Check(element.Kit.Name == ElementKit.LightningName, "the brazier did not give Lightning on the second start");
        RunState.Unlocked.Add(Upgrade.Static);
        SceneManager.LoadScene("Summit");
        yield return Arrive("Summit");
        yield return FellGods(false);
        yield return Choose(false);
        Say($"11 CONSUME holding Lightning: {State()}");
        Check(element.Kit.Name == ElementKit.LightningName && RunState.ElementConsumed && !RunState.Unlocked.Contains(Upgrade.Plasma) && RunState.Unlocked.Contains(Upgrade.Static) && combat.Weapon.Name == Weapons.KatanaName, "consume with Lightning went wrong");
        yield return Brawl(3f, () => null);
        yield return Home();
        Say($"12 home after consume (Lightning): runs ended {RunState.RunsEnded}; {State()}");
        CheckFresh("home after consume with Lightning");

        // 8: consume holding Fire.
        SceneManager.LoadScene("Summit");
        yield return Arrive("Summit");
        Check(element.Kit.Name == ElementKit.FireName, "a fresh run did not begin with Fire");
        yield return FellGods(false);
        yield return Choose(false);
        yield return new WaitForSeconds(0.4f);
        Say($"13 CONSUME holding Fire: plasma owned {RunState.Unlocked.Contains(Upgrade.Plasma)} in force {upgrades.Has(Upgrade.Plasma)}; {State()}");
        Check(element.Kit.Name == ElementKit.FireName && RunState.ElementConsumed && RunState.Unlocked.Contains(Upgrade.Plasma), "consume with Fire went wrong");
        yield return Brawl(3f, () => null);
        Say($"13b plasma bursts {upgrades.PlasmaBursts} errors {errors}");
        yield return Home();
        Say($"14 home after consume (Fire): runs ended {RunState.RunsEnded}; {State()}");
        CheckFresh("home after consume with Fire");

        // 9: a fresh start through the gate, and a blow landed.
        gate = FindAnyObjectByType<SceneDoor>();
        yield return Use(gate, new Vector3(0f, 0.05f, -1.8f), () => SceneManager.GetActiveScene().name == "Castle");
        yield return Arrive("Castle");
        int blows = combat.BlowsLanded;
        yield return Brawl(4f, NearestFoe);
        Say($"15 fresh castle: blows landed {combat.BlowsLanded - blows}; {State()}");
        Check(combat.BlowsLanded > blows, "no blow landed in the fresh run");
        Check(RunState.Unlocked.Count == 0 && element.Kit.Name == ElementKit.FireName, "the fresh run was not clean");

        RunState.Reset();
        ForgetAll();
        foreach (string error in firstErrors) Say("ERROR " + error);
        Say($"totals: errors {errors} failed checks {fails}");
        Say("done");
        Destroy(gameObject);
    }
}
