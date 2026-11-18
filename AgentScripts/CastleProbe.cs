using System;
using System.Collections;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the castle: fights, casts, takes the Katana, rests, falls and comes back, with simulated keys at
// full speed, and writes what happened to the console as "[Probe] ..." lines. Run in Play Mode with
//   unity command script run -- --file AgentScripts/CastleProbe.cs --entry CastleProbe.Run
public static class CastleProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Castle Probe").AddComponent<CastleProbeRunner>();
        return "probe started";
    }
}

public class CastleProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerInteraction interaction;
    private AreaDirector director;
    private bool keepAlive = true;

    // The probe is about whether things work, not whether it can survive: wounds are healed as they come, except in the step that tests falling.
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
    }

    // Presses a key at a moment the player is free to act, and tries again if a blow got in the way.
    private IEnumerator TapWhenFree(Key key, Func<bool> worked)
    {
        for (int attempt = 0; attempt < 6 && !worked(); attempt++)
        {
            defence.Restore();
            yield return Until(() => !defence.IsHelpless && !combat.IsAttacking && !player.Dodge.IsRolling, 3f);
            yield return Tap(key);
            yield return Until(worked, 0.8f);
        }
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

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        interaction = player.GetComponent<PlayerInteraction>();
        director = FindAnyObjectByType<AreaDirector>();
        Say($"start: weapon {combat.Weapon.Name} enemies {director.EnemiesAlive} health {defence.Health.Current:0}");
        yield return new WaitForSeconds(0.3f);

        // A: the fist chain on the first soldier.
        var soldier = Find(EnemyKind.SoldierName, 0);
        StandBefore(soldier, 1.5f);
        yield return null;
        int blows = combat.BlowsLanded;
        float healthBefore = soldier.Health.Current;
        string names = "";
        for (int i = 0; i < 4; i++)
        {
            yield return Tap(Key.J);
            yield return Until(() => combat.Combo.Current != null, 0.5f);
            if (combat.Combo.Current != null) names += combat.Combo.Current.Name.Replace("attack_", "") + " ";
            yield return new WaitForSeconds(0.2f);
        }
        yield return Until(() => !combat.IsAttacking, 1.5f);
        Say($"A fists: {names}blows {combat.BlowsLanded - blows} soldier health {healthBefore:0} -> {soldier.Health.Current:0} stagger {soldier.Stagger.Value:0} state {soldier.Brain.State} mana {combat.Mana.Current:0}");

        // B: the spell.
        combat.Mana.Gain(100f);
        StandBefore(soldier, 2.5f);
        yield return null;
        healthBefore = soldier.Health.Current;
        yield return TapWhenFree(Key.Q, () => element.SpellsCast > 0);
        yield return new WaitForSeconds(0.5f);
        Say($"B spell: cast {element.SpellsCast} hits {element.SpellHits} burning {soldier.Burn.IsBurning} soldier health {healthBefore:0} -> {soldier.Health.Current:0} mana {combat.Mana.Current:0}");

        // C: the surge.
        combat.Mana.Gain(100f);
        yield return TapWhenFree(Key.F, () => element.Surges > 0);
        Say($"C surge: surges {element.Surges} active {element.Kit.SurgeActive} damage x{combat.DamageMultiplier:0.0} burn on hit {combat.BurnOnHit:0.0} mana {combat.Mana.Current:0}");

        // D: stand still and be hit by the soldier.
        defence.Restore();
        int taken = defence.BlowsTaken;
        StandBefore(soldier, 2.5f);
        yield return Until(() => defence.BlowsTaken > taken || soldier.IsDead, 8f);
        Say($"D soldier strikes: taken +{defence.BlowsTaken - taken} soldier blows {soldier.BlowsStruck} health {defence.Health.Current:0} soldier state {soldier.Brain.State}");

        // E: stagger and finish it.
        yield return Until(() => !defence.IsHelpless, 2f);
        soldier.TakeHit(new Hit(1f, 1000f, Vector3.forward));
        yield return null;
        bool staggered = soldier.IsStaggered;
        StandBefore(soldier, 2.5f);
        yield return null;
        int finishers = combat.Finishers;
        yield return Tap(Key.K);
        yield return Until(() => combat.Finishers > finishers, 2f);
        string finisherName = combat.Combo.Current != null ? combat.Combo.Current.Name : "none";
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"E finisher: staggered {staggered} move {finisherName} finishers +{combat.Finishers - finishers} soldier dead {soldier.IsDead} health {soldier.Health.Current:0}");
        if (!soldier.IsDead) soldier.TakeHit(new Hit(1000f, 0f, Vector3.forward));
        yield return new WaitForSeconds(3f);
        Say($"E after: soldier active {soldier.gameObject.activeSelf} enemies alive {director.EnemiesAlive}");

        // F: the caster's bolt.
        defence.Restore();
        var caster = Find(EnemyKind.CasterName, 0);
        StandBefore(caster, 7f);
        taken = defence.BlowsTaken;
        yield return Until(() => caster.BlowsStruck > 0, 8f);
        yield return new WaitForSeconds(0.2f);
        Say($"F caster bolt: caster blows {caster.BlowsStruck} taken +{defence.BlowsTaken - taken} health {defence.Health.Current:0}");

        // G: the storm strikes an enemy that is in the fight.
        director.StormStrikeMin = 0.5f;
        director.StormStrikeMax = 1f;
        yield return Until(() => director.StormStrikes > 0, 22f);
        int charged = 0;
        foreach (var enemy in director.Enemies) if (enemy.IsCharged) charged++;
        Say($"G storm strike: strikes {director.StormStrikes} charged enemies {charged}");
        director.StormStrikeMin = 8f;
        director.StormStrikeMax = 15f;

        // H: take the Katana.
        var pickup = FindAnyObjectByType<WeaponPickup>();
        player.Teleport(pickup.transform.position + new Vector3(0f, 0.1f, -1.6f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        string prompt = interaction.Current != null ? interaction.Current.Prompt : "none";
        yield return TapWhenFree(Key.E, () => pickup.Taken);
        Say($"H pickup: prompt '{prompt}' weapon {combat.Weapon.Name} taken {pickup.Taken}");

        // I: rest at the terrace brazier.
        RestPoint second = null;
        foreach (var rest in FindObjectsByType<RestPoint>(FindObjectsSortMode.None))
            if (rest.transform.position.y > 1f) second = rest;
        player.Teleport(second.transform.position + new Vector3(1.5f, 0.1f, 0f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        yield return TapWhenFree(Key.E, () => second.Rests > 0);
        Say($"I rest: rests {second.Rests} lit {second.IsLit} enemies alive {director.EnemiesAlive} soldier active {soldier.gameObject.activeSelf} health {defence.Health.Current:0}");

        // The fire opens the element's tree and the world stops; the dodge button leaves it.
        var menu = Clover.UI.UpgradeMenu.Instance;
        yield return null;
        bool opened = menu != null && menu.IsOpen;
        float stoppedAt = Time.timeScale;
        ScreenCapture.CaptureScreenshot("review/shots/current/castle_menu.png");
        yield return null;
        yield return Tap(Key.Space);
        yield return null;
        yield return null;
        Say($"I menu: opened {opened} time {stoppedAt:0} embers {Clover.Core.RunState.Points} closed {!menu.IsOpen} time {Time.timeScale:0}");

        // J: the sealed door.
        var way = FindAnyObjectByType<SealedWay>();
        player.Teleport(way.transform.position + new Vector3(0f, 0.1f, -1f), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        yield return Tap(Key.E);
        yield return new WaitForSeconds(0.2f);
        Say($"J sealed way: tries {way.Tries} says '{defence.LastCallout}'");

        // K: a fall ends the run: the dark closes and the player wakes in the hub with nothing carried.
        DontDestroyOnLoad(gameObject);
        keepAlive = false;
        yield return null;
        defence.TakeHit(new Hit(1000f, 0f, Vector3.back));
        yield return null;
        bool dead = defence.IsDead;
        float until = Time.unscaledTime + 8f;
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Hub" && Time.unscaledTime < until) yield return null;
        yield return null;
        yield return null;
        var woken = FindAnyObjectByType<PlayerCombat>();
        Say($"K fall: dead {dead} woke in {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} weapon {woken.Weapon.Name} embers {Clover.Core.RunState.Points} runs ended {Clover.Core.RunState.RunsEnded}");
        Clover.Core.Progress.Forget(Clover.Core.Progress.MetSponsor);
        Clover.Core.Progress.Forget(Clover.Core.Progress.WeaponKey(Weapons.KatanaName));

        Say($"done: time scale {Time.timeScale:0.00}");
        Destroy(gameObject);
    }
}
