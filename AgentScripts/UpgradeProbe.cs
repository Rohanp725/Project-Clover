using System;
using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the element trees, in the combat arena: opens the menu and learns with simulated keys, then
// plays each upgrade against the training dummy and writes what happened as "[Probe] ..." lines. Run in Play Mode with
//   unity command script run -- --file AgentScripts/UpgradeProbe.cs --entry UpgradeProbe.Run
public static class UpgradeProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Upgrade Probe").AddComponent<UpgradeProbeRunner>();
        return "probe started";
    }
}

public class UpgradeProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private TrainingDummy north;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private static void Shot(string name) => ScreenCapture.CaptureScreenshot("review/shots/current/upg_" + name + ".png");

    private void Place(Vector3 position, Vector3 facing)
    {
        player.Teleport(position, Quaternion.LookRotation(facing, Vector3.up));
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
    }

    private IEnumerator Hold(Key key, float seconds)
    {
        Keys(key);
        yield return new WaitForSecondsRealtime(seconds);
        Keys();
        yield return null;
    }

    private IEnumerator Lights(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int before = combat.BlowsLanded;
            yield return Tap(Key.J);
            yield return Until(() => combat.BlowsLanded > before, 1.2f);
            yield return new WaitForSeconds(0.12f);
        }
        yield return Until(() => !combat.IsAttacking, 1.5f);
    }

    private void Learn(params string[] ids)
    {
        foreach (string id in ids) RunState.Unlocked.Add(id);
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        foreach (var dummy in FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None))
            if (Vector3.Distance(dummy.transform.position, new Vector3(0f, 0f, 4f)) < 0.5f) north = dummy;
        north.Health.Maximum = 5000f;
        north.Health.Fill();
        Say($"start: element {element.Kit.Name} weapon {combat.Weapon.Name} embers {upgrades.Points} menu {(UpgradeMenu.Instance != null)} hazards {(FindAnyObjectByType<Clover.World.Hazards>() != null)}");
        yield return new WaitForSeconds(0.3f);

        // A: the menu. Opened as a rest fire would, it stops the world; S moves down, E learns, Space leaves.
        Place(new Vector3(6f, 0f, -6f), Vector3.forward);
        upgrades.Earn(6);
        var menu = UpgradeMenu.Instance;
        menu.Open();
        yield return null;
        yield return null;
        bool stopped = Time.timeScale == 0f && player.Suspended;
        Shot("menu");
        // The far end of a branch cannot be learned before its foot.
        menu.Choose(Upgrade.MeteorLeap);
        yield return Tap(Key.E);
        bool early = upgrades.Owns(Upgrade.MeteorLeap);
        menu.Choose(Upgrade.Heat);
        yield return Tap(Key.E);
        bool heat = upgrades.Owns(Upgrade.Heat);
        yield return Tap(Key.W);
        string selected = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        yield return Tap(Key.E);
        bool second = upgrades.Owns(Upgrade.BurningCrescent);
        yield return Tap(Key.W);
        string above = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        yield return Tap(Key.E);
        bool third = upgrades.Owns(Upgrade.MeteorLeap);
        yield return Tap(Key.A);
        string left = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        yield return null;
        Shot("menu_learned");
        yield return null;
        Vector3 before = player.transform.position;
        yield return Tap(Key.Space);
        yield return new WaitForSecondsRealtime(0.4f);
        Say($"A menu: stopped {stopped} far end before its foot {early} heat learned {heat} W moved to {selected}, learned {second}; W again to {above}, learned with 1 ember {third}; A moved to {left}; embers left {upgrades.Points} closed {!menu.IsOpen} time {Time.timeScale:0} rolled on close {(player.transform.position - before).magnitude > 0.5f}");

        // B: Heat and the deepening burn, from blows of the sword.
        Learn(Upgrade.Heat, Upgrade.BurnAndDetonate, Upgrade.Backdraft, Upgrade.FlameTrail, Upgrade.BurningCrescent, Upgrade.FirebombFlask, Upgrade.MeteorLeap);
        yield return new WaitForSeconds(0.4f);
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        yield return Lights(6);
        Shot("heat");
        Say($"B heat: level {upgrades.Heat.Level} damage x{combat.UpgradeDamage:0.00} burn stacks {north.Burn.Stacks} burning {north.Burn.IsBurning}");

        // C: a perfect guard sets the burn off and throws fire back.
        float health = north.Health.Current;
        int stacks = north.Burn.Stacks;
        upgrades.OnPerfectGuard(north.transform.position);
        yield return null;
        Shot("backdraft");
        Say($"C detonate: stacks {stacks} -> {north.Burn.Stacks} dummy lost {health - north.Health.Current:0} detonations {upgrades.Detonations}");
        yield return new WaitForSeconds(0.6f);

        // D: the dodge leaves fire. Roll past the dummy and wait for the trail to bite.
        Place(new Vector3(0.9f, 0f, 1.5f), Vector3.forward);
        yield return null;
        health = north.Health.Current;
        Keys(Key.W, Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.5f);
        Shot("trail");
        yield return new WaitForSeconds(1.2f);
        Say($"D trail: dummy lost {health - north.Health.Current:0} burning {north.Burn.IsBurning}");

        // E: the Iaido throws a crescent of flame on ahead.
        combat.Mana.Gain(100f);
        Place(new Vector3(0f, 0f, -3.5f), Vector3.forward);
        yield return null;
        health = north.Health.Current;
        int skills = combat.SkillsUsed;
        Keys(Key.K);
        yield return Until(() => combat.Combo.IsCharged, 2f);
        yield return null;
        Keys();
        yield return Until(() => combat.SkillsUsed > skills, 1f);
        yield return new WaitForSeconds(0.45f);
        Shot("crescent");
        yield return new WaitForSeconds(0.8f);
        Say($"E crescent: skill used {combat.SkillsUsed - skills} dummy (7.5 m away) lost {health - north.Health.Current:0}");

        // F: the flask, held, is thrown.
        Place(new Vector3(0f, 0f, -2f), Vector3.forward);
        yield return null;
        health = north.Health.Current;
        int flasks = defence.Flasks.Count;
        yield return Hold(Key.R, 0.45f);
        yield return new WaitForSeconds(0.12f);
        Shot("flask_in_air");
        yield return new WaitForSeconds(0.4f);
        Shot("firebomb");
        yield return new WaitForSeconds(0.6f);
        Say($"F firebomb: thrown {upgrades.FlasksThrown} flasks {flasks} -> {defence.Flasks.Count} drinking {defence.IsDrinking} dummy lost {health - north.Health.Current:0}");

        // G: the spell button, held, is the leap.
        combat.Mana.Gain(100f);
        Place(new Vector3(0f, 0f, -3f), Vector3.forward);
        yield return null;
        health = north.Health.Current;
        float mana = combat.Mana.Current;
        Keys(Key.U);
        yield return Until(() => player.IsLeaping, 1f);
        Keys();
        bool leaping = player.IsLeaping;
        yield return new WaitForSeconds(0.3f);
        float height = player.transform.position.y;
        Shot("leap_air");
        yield return Until(() => !player.IsLeaping, 2f);
        yield return null;
        Shot("leap_land");
        yield return new WaitForSeconds(0.5f);
        Say($"G leap: leaping {leaping} height mid-air {height:0.0} leaps {upgrades.Leaps} mana {mana:0} -> {combat.Mana.Current:0} landed at {player.transform.position} dummy lost {health - north.Health.Current:0} casts {element.SpellsCast}");

        // H: a tap of the spell button is still the spell.
        combat.Mana.Gain(100f);
        int casts = element.SpellsCast;
        yield return Tap(Key.U);
        yield return Until(() => element.SpellsCast > casts, 1.5f);
        Say($"H tap still casts: {element.SpellsCast - casts}; a tap of the flask still drinks: see next");
        defence.Health.Damage(40f);
        yield return new WaitForSeconds(0.6f);
        flasks = defence.Flasks.Count;
        yield return Tap(Key.R);
        yield return new WaitForSeconds(1f);
        Say($"H drink: flasks {flasks} -> {defence.Flasks.Count} health {defence.Health.Current:0}");

        // I: Lightning. Static builds on light blows and a heavy sets it off.
        element.Take(ElementKit.Lightning());
        upgrades.Forget();
        Learn(Upgrade.Static, Upgrade.StoredCharge, Upgrade.Blink, Upgrade.Afterimage, Upgrade.DelayedCuts, Upgrade.LightningRod, Upgrade.Overclock);
        yield return new WaitForSeconds(0.4f);
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        yield return Lights(3);
        health = north.Health.Current;
        yield return Tap(Key.K);
        yield return Until(() => upgrades.Discharges > 0, 2f);
        Shot("static");
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"I static: discharges {upgrades.Discharges} heavy and discharge took {health - north.Health.Current:0} heat shown {upgrades.ShowsHeat}");

        // J: a perfect guard stores a charge and the next blow spends it.
        upgrades.OnPerfectGuard(north.transform.position);
        bool stored = upgrades.ChargeStored;
        health = north.Health.Current;
        yield return Lights(1);
        Say($"J stored charge: stored {stored} spent {!upgrades.ChargeStored} the blow took {health - north.Health.Current:0}");

        // K: the rod.
        combat.Mana.Gain(100f);
        Place(new Vector3(0f, 0f, 1f), Vector3.forward);
        yield return null;
        health = north.Health.Current;
        mana = combat.Mana.Current;
        yield return Hold(Key.U, 0.45f);
        yield return new WaitForSeconds(1.3f);
        Shot("rod");
        yield return new WaitForSeconds(2f);
        Say($"K rod: mana {mana:0} -> {combat.Mana.Current:0} dummy lost {health - north.Health.Current:0} in 3.3 s");

        // L: Overclock slows the enemies when the surge is lit; the blink shocks what the dodge passes.
        combat.Mana.Gain(100f);
        yield return Tap(Key.I);
        yield return Until(() => element.Surges > 0, 1f);
        float scale = WorldClock.EnemyScale;
        yield return new WaitForSeconds(3f);
        Place(new Vector3(0.6f, 0f, 2.4f), Vector3.forward);
        yield return null;
        health = north.Health.Current;
        Keys(Key.W, Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.6f);
        Say($"L overclock: enemy time x{scale:0.00} then x{WorldClock.EnemyScale:0.00}; blink through the dummy took {health - north.Health.Current:0}");

        // M: the afterimage and the delayed cuts.
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        health = north.Health.Current;
        upgrades.OnDodge(player.transform.position, Vector3.back, 0f, 0.3f);
        upgrades.OnPerfectDodge();
        yield return new WaitForSeconds(0.7f);
        float afterimage = health - north.Health.Current;
        combat.Mana.Gain(100f);
        health = north.Health.Current;
        Keys(Key.K);
        yield return Until(() => combat.Combo.IsCharged, 2f);
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.35f);
        float cut = health - north.Health.Current;
        yield return new WaitForSeconds(0.8f);
        Shot("delayed");
        Say($"M afterimage took {afterimage:0}; iaido took {cut:0} at once and {health - north.Health.Current - cut:0} a moment later");

        // N: Storm Step. With the surge lit, a dodge costs nothing, shocks, and leaves a shape of light behind.
        Learn(Upgrade.StormStep);
        RunState.Unlocked.Remove(Upgrade.Blink);
        yield return new WaitForSeconds(0.4f);
        combat.Mana.Gain(100f);
        yield return Tap(Key.I);
        yield return Until(() => element.Kit.SurgeActive, 1f);
        yield return new WaitForSeconds(3f);
        Place(new Vector3(0.6f, 0f, 2.4f), Vector3.forward);
        player.Stamina.Fill();
        yield return null;
        health = north.Health.Current;
        var ghost = player.GetComponent<Afterimage>();
        int ghosts = ghost != null ? ghost.Shown : -1;
        Keys(Key.W, Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.1f);
        Shot("storm_step");
        yield return new WaitForSeconds(0.5f);
        Say($"N storm step: free dodges {upgrades.FreeDodges} stamina {player.Stamina.Current:0} of {player.Stamina.Maximum:0} afterimages +{(ghost != null ? ghost.Shown - ghosts : 0)} shock took {health - north.Health.Current:0}");

        // O: Plasma. Back to Fire with the storm consumed: a heavy blow on a burning enemy bursts.
        element.Take(ElementKit.Fire(true));
        upgrades.Forget();
        Learn(Upgrade.Plasma);
        yield return new WaitForSeconds(0.4f);
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        north.TakeHit(new Hit(0f, 0f, Vector3.forward, false, true, null, 6f));
        health = north.Health.Current;
        yield return Tap(Key.K);
        yield return Until(() => upgrades.PlasmaBursts > 0, 2f);
        Shot("plasma");
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"O plasma: bursts {upgrades.PlasmaBursts} the heavy and the burst took {health - north.Health.Current:0}");

        Say("done");
        Destroy(gameObject);
    }
}
