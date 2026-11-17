using System;
using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Play Mode probe for one whole run, start to finish, by the doors a player would use: the hub's gate, the castle
// cleared, its sealed way, both of the god's forms, the choice, and the road home. Enemies are felled by the probe;
// the doors, the choices and the carrying of things from scene to scene are done with the player's own buttons.
// It forgets saved progress before and after. Run in Play Mode, in the Hub scene.
public static class RunProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Run Probe").AddComponent<RunProbeRunner>();
        return "probe started";
    }
}

public class RunProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private static void ForgetAll()
    {
        Progress.Forget(Progress.MetSponsor);
        Progress.Forget(Progress.WeaponKey(Weapons.KatanaName));
        Progress.Forget(Progress.ElementKey(ElementKit.LightningName));
    }

    private void Update()
    {
        if (defence != null) defence.Health.Heal(100f);
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
    }

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

    private static void FellAll(AreaDirector director)
    {
        foreach (var enemy in director.Enemies)
            for (int blow = 0; blow < 60 && !enemy.IsDead; blow++) enemy.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        ForgetAll();
        SceneManager.LoadScene("Hub");
        yield return null;
        yield return null;
        FindPlayer();
        // The first words of the voice, read through.
        yield return Until(() => Clover.UI.ReadingBox.Instance != null && Clover.UI.ReadingBox.Instance.IsOpen, 4f);
        for (int i = 0; i < 40 && Clover.UI.ReadingBox.Instance.IsOpen; i++) yield return Tap(Key.E);
        yield return new WaitForSeconds(0.3f);

        // 1: through the gate.
        var gate = FindAnyObjectByType<SceneDoor>();
        yield return Use(gate, new Vector3(0f, 0.05f, -1.8f), () => SceneManager.GetActiveScene().name == "Castle");
        yield return Until(() => SceneManager.GetActiveScene().name == "Castle", 5f);
        yield return null;
        yield return null;
        FindPlayer();
        Say($"1 castle: scene {SceneManager.GetActiveScene().name} weapon {combat.Weapon.Name} element {element.Kit.Name}");

        // 2: draw the Katana, fell everyone, and rest once to spend what they gave.
        var stone = FindAnyObjectByType<WeaponPickup>();
        yield return Use(stone, new Vector3(1.4f, 0.1f, -1.2f), () => stone.Taken);
        var director = FindAnyObjectByType<AreaDirector>();
        FellAll(director);
        yield return new WaitForSeconds(0.5f);
        int embers = RunState.Points;
        RestPoint fire = null;
        foreach (var rest in FindObjectsByType<RestPoint>(FindObjectsSortMode.None))
            if (rest.transform.position.y > 1f) fire = rest;
        yield return Use(fire, new Vector3(1.5f, 0.1f, 0f), () => Clover.UI.UpgradeMenu.Instance.IsOpen);
        bool opened = Clover.UI.UpgradeMenu.Instance.IsOpen;
        for (int i = 0; i < 4; i++)
        {
            yield return Tap(Key.E);
            yield return Tap(Key.S);
        }
        string learned = string.Join(",", RunState.Unlocked);
        int left = RunState.Points;
        yield return Tap(Key.Space);
        yield return new WaitForSecondsRealtime(0.4f);
        // Resting brought the enemies back; the way stays sealed until they are down again.
        int back = director.EnemiesAlive;
        FellAll(director);
        yield return new WaitForSeconds(0.5f);
        Say($"2 castle cleared: weapon {combat.Weapon.Name} embers earned {embers} menu opened {opened} learned [{learned}] embers left {left}; the rest brought back {back}; embers after felling them again {RunState.Points}; alive {director.EnemiesAlive}");

        // 3: the sealed way.
        var way = FindAnyObjectByType<SealedWay>();
        yield return Use(way, new Vector3(0f, 0.1f, -1f), () => SceneManager.GetActiveScene().name == "Summit");
        yield return Until(() => SceneManager.GetActiveScene().name == "Summit", 5f);
        yield return null;
        yield return null;
        FindPlayer();
        Say($"3 summit: scene {SceneManager.GetActiveScene().name} weapon {combat.Weapon.Name} element {element.Kit.Name} learning kept [{string.Join(",", RunState.Unlocked)}] embers {RunState.Points}");

        // 4: both of the forms of the god.
        var summit = FindAnyObjectByType<SummitDirector>();
        Enemy boss = null;
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            if (enemy.Kind.IsBoss) boss = enemy;
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        var colossus = FindAnyObjectByType<GodlyForm>();
        yield return Until(() => colossus.Doing == "Idle", 12f);
        for (int attempt = 0; attempt < 12 && !colossus.IsDead; attempt++)
        {
            yield return Until(() => colossus.Doing == "Idle", 12f);
            colossus.Unbalance(10000f);
            yield return new WaitForSeconds(0.7f);
            for (int blow = 0; blow < 40 && !colossus.IsDead; blow++) colossus.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
        }
        yield return Until(() => summit.Felled, 10f);
        yield return new WaitForSecondsRealtime(summit.RewardDelay + 2.5f);
        Say($"4 the god: swordsman dead {boss.IsDead} storm dead {colossus.IsDead} felled {summit.Felled} embers {RunState.Points}");

        // 5: consume the storm, and go home.
        GodReward consume = null;
        foreach (var reward in FindObjectsByType<GodReward>(FindObjectsSortMode.None))
            if (!reward.Takes) consume = reward;
        yield return Use(consume, new Vector3(0f, 0.1f, -1.4f), () => consume.Chosen);
        Say($"5 consumed: chosen {consume.Chosen} element {element.Kit.Name} consumed {RunState.ElementConsumed} plasma {RunState.Unlocked.Contains(Upgrade.Plasma)} embers {RunState.Points}");
        var door = FindAnyObjectByType<SceneDoor>();
        yield return Use(door, new Vector3(0f, 0.1f, 1f), () => SceneManager.GetActiveScene().name == "Hub");
        yield return Until(() => SceneManager.GetActiveScene().name == "Hub", 5f);
        yield return null;
        yield return null;
        FindPlayer();
        bool katanaOpen = false, stormOpen = false;
        foreach (var choice in FindObjectsByType<LoadoutChoice>(FindObjectsSortMode.None))
        {
            if (choice.Choice == Weapons.KatanaName) katanaOpen = choice.IsUnlocked;
            if (choice.Choice == ElementKit.LightningName) stormOpen = choice.IsUnlocked;
        }
        Say($"6 home: scene {SceneManager.GetActiveScene().name} runs ended {RunState.RunsEnded} fell {RunState.LastRunFell} gods last run {RunState.LastRunGods} weapon {combat.Weapon.Name} embers {RunState.Points} katana stand open {katanaOpen} lightning brazier open {stormOpen} (the storm was consumed, so it should be cold)");

        ForgetAll();
        Say("done");
        Destroy(gameObject);
    }
}
