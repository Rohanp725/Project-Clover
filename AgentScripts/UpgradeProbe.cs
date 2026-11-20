using System;
using System.Collections;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using System.Collections.Generic;
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

        // A: the chart. Opened as a rest fire would, it stops the world; W moves outward, E learns, K shows the whole ring, Space leaves.
        Place(new Vector3(6f, 0f, -6f), Vector3.forward);
        upgrades.Earn(16);
        var menu = UpgradeMenu.Instance;
        menu.Open();
        yield return null;
        yield return null;
        bool stopped = Time.timeScale == 0f && player.Suspended;
        yield return new WaitForSecondsRealtime(0.3f);
        Shot("menu");
        // The far end of a branch cannot be learned before its foot.
        menu.Choose(Upgrade.MeteorLeap);
        yield return Tap(Key.E);
        bool early = upgrades.Owns(Upgrade.MeteorLeap);
        // The root first (Cinder), then Heat above it with the stick, then down the right bough to the leap.
        menu.Choose(FireTree.Cinder);
        yield return Tap(Key.E);
        bool heat = upgrades.Owns(FireTree.Cinder);
        yield return Tap(Key.W);
        string selected = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        yield return Tap(Key.E);
        bool second = menu.Chosen != null && upgrades.Owns(menu.Chosen.Id);
        yield return Tap(Key.W);
        string minor = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        menu.Choose(Upgrade.FirebombFlask);
        yield return Tap(Key.E);
        string above = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        menu.Choose(Upgrade.MeteorLeap);
        yield return Tap(Key.E);
        bool third = upgrades.Owns(Upgrade.MeteorLeap);
        string wornNow = upgrades.Worn != null ? upgrades.Worn.Name : "nothing";
        yield return Tap(Key.A);
        string left = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        yield return new WaitForSecondsRealtime(0.5f);
        Shot("menu_learned");
        yield return null;
        // The whole ring, and back.
        yield return Tap(Key.K);
        bool wide = menu.IsWide;
        yield return new WaitForSecondsRealtime(0.9f);
        float far = menu.Zoom;
        Shot("menu_ring");
        yield return null;
        yield return Tap(Key.K);
        yield return new WaitForSecondsRealtime(0.7f);
        // A mixed branch the player cannot open is there to be read, sealed. Pyre, on the ring beside Fire: Plasma has moved
        // into the chart's heart (task 4.5), shown only while Lightning is carried beside Fire.
        menu.Choose("pyre.flashpoint");
        yield return Tap(Key.E);
        bool sealedBought = upgrades.Owns("pyre.flashpoint");
        yield return new WaitForSecondsRealtime(0.5f);
        Shot("menu_sealed");
        yield return null;
        Vector3 before = player.transform.position;
        yield return Tap(Key.Space);
        yield return new WaitForSecondsRealtime(0.4f);
        Say($"A chart: {menu.MarkCount} marks; stopped {stopped} far end before its foot {early} root learned {heat} W moved to {selected}, learned {second}; W to {minor}; W to {above}, learned {third}, worn {wornNow}; A moved to {left}; whole ring {wide} at zoom {far:0.00} then back {!menu.IsWide} at {menu.Zoom:0.00}; sealed mix bought {sealedBought}; embers left {upgrades.Points} closed {!menu.IsOpen} time {Time.timeScale:0.0} moved {(player.transform.position - before).magnitude:0.00}");

        // B: Heat and the deepening burn, from blows of the sword.
        Learn(FireTree.Cinder, Upgrade.Heat, Upgrade.BurnAndDetonate, Upgrade.Backdraft, Upgrade.FlameTrail, Upgrade.BurningCrescent, Upgrade.FirebombFlask, Upgrade.MeteorLeap);
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
        Keys(Key.Q);
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
        yield return Tap(Key.Q);
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
        yield return Hold(Key.Q, 0.45f);
        yield return new WaitForSeconds(1.3f);
        Shot("rod");
        yield return new WaitForSeconds(2f);
        Say($"K rod: mana {mana:0} -> {combat.Mana.Current:0} dummy lost {health - north.Health.Current:0} in 3.3 s");

        // L: Overclock slows the enemies when the surge is lit; the blink shocks what the dodge passes.
        combat.Mana.Gain(100f);
        yield return Tap(Key.F);
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
        yield return Tap(Key.F);
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

        // P: the afflictions, laid straight onto the dummy so each can be measured.
        upgrades.Forget();
        yield return new WaitForSeconds(0.4f);
        var set = north.Afflictions;
        var bars = north.GetComponentInChildren<EnemyBars>();
        Hit Lay(AfflictionKind kind, int stacks) => new Hit(0f, 0f, Vector3.forward, false, true, null, 0f, 0, false, kind, stacks);
        set.Clear();
        north.TakeHit(Lay(AfflictionKind.Chill, 2));
        float slowed = set.SpeedMultiplier;
        north.TakeHit(Lay(AfflictionKind.Chill, 3));
        bool frozen = set.IsFrozen;
        yield return new WaitForSeconds(0.25f);
        Shot("frozen");
        yield return null;
        health = north.Health.Current;
        north.TakeHit(new Hit(10f, 0f, Vector3.forward, true));
        Say($"P1 chill: speed x{slowed:0.00} at two deep; frozen at five {frozen}; a heavy blow of 10 took {health - north.Health.Current:0} and broke the ice (frozen now {set.IsFrozen}, chill {set.Stacks(AfflictionKind.Chill)})");
        set.Clear();
        north.TakeHit(Lay(AfflictionKind.Poison, 3));
        health = north.Health.Current;
        yield return new WaitForSeconds(1.5f);
        float early2 = health - north.Health.Current;
        yield return new WaitForSeconds(0.8f);
        Say($"P2 poison three deep: lost {early2:0} in the first 1.5 s, {health - north.Health.Current:0} once the burst came at 2 s");
        set.Clear();
        north.TakeHit(new Hit(10f, 0f, Vector3.forward));
        float plain = north.LastDamage;
        north.TakeHit(Lay(AfflictionKind.Sundered, 3));
        north.TakeHit(new Hit(10f, 0f, Vector3.forward));
        float sundered = north.LastDamage;
        north.TakeHit(new Hit(10f, 0f, Vector3.forward, false, true, null, 0f, 0, false, AfflictionKind.None, 0, 0f, 1f, true));
        float spellOnSundered = north.LastDamage;
        north.TakeHit(Lay(AfflictionKind.Soaked, 2));
        north.TakeHit(new Hit(10f, 0f, Vector3.forward, false, true, null, 0f, 0, false, AfflictionKind.None, 0, 0f, 1f, true));
        float spellOnSoaked = north.LastDamage;
        Say($"P3 a blow of 10: plain {plain:0.0}, weapon on sundered {sundered:0.0}, spell on sundered {spellOnSundered:0.0}, spell on soaked {spellOnSoaked:0.0}");
        // Tremor: balance does not drain back.
        set.Clear();
        north.Stagger.Reset();
        north.TakeHit(new Hit(0.1f, 40f, Vector3.forward));
        north.TakeHit(Lay(AfflictionKind.Tremor, 1));
        yield return new WaitForSeconds(3.2f);
        float held = north.Stagger.Value;
        set.Clear();
        yield return new WaitForSeconds(3.2f);
        Say($"P4 tremor: balance of 40 was {held:0} after 3.2 s shaken, {north.Stagger.Value:0} after 3.2 s more without");
        // Marked: a blow of the weapon is critical and gives more mana. Seeded: it heals. Several at once show as pips.
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        set.Clear();
        yield return Lights(1);
        float ordinary = north.LastDamage;
        north.TakeHit(Lay(AfflictionKind.Marked, 2));
        north.TakeHit(Lay(AfflictionKind.Seeded, 3));
        north.TakeHit(Lay(AfflictionKind.Cursed, 1));
        north.TakeHit(Lay(AfflictionKind.Unsteady, 2));
        defence.Health.Damage(30f);
        float myHealth = defence.Health.Current;
        int crits = combat.Criticals;
        mana = combat.Mana.Current;
        yield return Lights(1);
        yield return null;
        Shot("critical_pips");
        yield return null;
        yield return null;
        Say($"P5 marked and seeded: plain blow {ordinary:0.0}, marked blow {north.LastDamage:0.0} critical {north.LastCritical} (criticals +{combat.Criticals - crits}); mana +{combat.Mana.Current - mana:0}; healed {defence.Health.Current - myHealth:0.0}; pips over the dummy {(bars != null ? bars.PipsShown : -1)}; deals x{set.DamageDealtMultiplier:0.00} stagger taken x{set.StaggerTakenMultiplier:0.00}");
        // Soaked carries Static to those near.
        set.Clear();
        TrainingDummy nearest = null;
        foreach (var dummy in FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None))
            if (dummy != north && (nearest == null || Vector3.Distance(dummy.transform.position, north.transform.position) < Vector3.Distance(nearest.transform.position, north.transform.position))) nearest = dummy;
        Vector3 nearestHome = nearest != null ? nearest.transform.position : Vector3.zero;
        if (nearest != null) nearest.transform.position = north.transform.position + new Vector3(2.5f, 0f, 0f);
        Physics.SyncTransforms();
        north.TakeHit(Lay(AfflictionKind.Soaked, 1));
        north.TakeHit(Lay(AfflictionKind.Static, 2));
        yield return null;
        yield return null;
        yield return null;
        Say($"P6 soaked carries: static on the struck dummy {set.Stacks(AfflictionKind.Static)}, on the one 2.5 m away {(nearest != null ? nearest.Afflictions.Stacks(AfflictionKind.Static) : -1)}");
        if (nearest != null)
        {
            nearest.Afflictions.Clear();
            nearest.transform.position = nearestHome;
        }
        set.Clear();
        defence.Restore();

        // Q: the player's numbers. A minor learned, a timed boon, and their going away again.
        element.Take(ElementKit.Fire());
        upgrades.Forget();
        float damageBefore = combat.StatDamage, windowBefore = defence.Guard.PerfectWindow, speedBefore = player.MoveSpeed;
        Learn(FireTree.Cinder, Upgrade.Heat, Upgrade.Backdraft, Upgrade.BurningCrescent, "fire.kindling", "fire.stoked");
        yield return new WaitForSeconds(0.4f);
        float damageAfter = combat.StatDamage, windowAfter = defence.Guard.PerfectWindow;
        upgrades.Stats.AddBoon(PlayerStat.MoveSpeed, 0.25f, 0.6f);
        upgrades.Stats.AddBoon(PlayerStat.Health, 20f, 0.6f);
        upgrades.Stats.AddBoon(PlayerStat.FlaskCount, 1f, 0.6f);
        yield return null;
        yield return null;
        float speedBoon = player.MoveSpeed, healthBoon = defence.Health.Maximum;
        int flasksBoon = defence.Flasks.Maximum;
        yield return new WaitForSeconds(0.8f);
        Say($"Q stats: weapon damage x{damageBefore:0.00} -> x{damageAfter:0.00} with Stoked; perfect guard window {windowBefore:0.000} -> {windowAfter:0.000} with Kindling; a boon took speed {speedBefore:0.0} -> {speedBoon:0.0} -> {player.MoveSpeed:0.0}, health {healthBoon:0} -> {defence.Health.Maximum:0}, flasks {flasksBoon} -> {defence.Flasks.Maximum}");

        // R: effects that are only data. Backdraft's burn is data now; Grounding arcs from a perfect guard; Slipstream quickens after a perfect dodge.
        Learn(Upgrade.BurnAndDetonate);
        yield return new WaitForSeconds(0.4f);
        set.Clear();
        int done = upgrades.EffectsDone;
        upgrades.OnPerfectGuard(north.transform.position);
        yield return null;
        yield return null;
        Say($"R1 backdraft's burn as data: effects done +{upgrades.EffectsDone - done}; the dummy burns {north.Burn.IsBurning} {north.Burn.Stacks} deep");
        element.Take(ElementKit.Lightning());
        upgrades.Forget();
        Learn(Upgrade.Static, Upgrade.StoredCharge, Upgrade.DelayedCuts, "lightning.grounding", Upgrade.Blink, Upgrade.Afterimage, "lightning.slipstream");
        yield return new WaitForSeconds(0.4f);
        set.Clear();
        health = north.Health.Current;
        done = upgrades.EffectsDone;
        upgrades.OnPerfectGuard(north.transform.position);
        yield return null;
        yield return null;
        float arc = health - north.Health.Current;
        speedBefore = player.MoveSpeed;
        upgrades.OnDodge(player.transform.position, Vector3.back, 0f, 0.3f);
        upgrades.OnPerfectDodge();
        yield return null;
        yield return null;
        Say($"R2 grounding: an arc from a perfect guard took {arc:0} (effects +{upgrades.EffectsDone - done}); slipstream: speed {speedBefore:0.0} -> {player.MoveSpeed:0.0}, swing x{combat.StatSwing:0.00}, boons running {upgrades.Stats.BoonsRunning}");
        yield return new WaitForSeconds(1f);

        // S: the worn spell. Two learned; the first is worn; choosing the other on the chart and pressing learn wears that one.
        element.Take(ElementKit.Fire());
        upgrades.Forget();
        Learn(FireTree.Cinder, Upgrade.Heat, Upgrade.BurningCrescent, Upgrade.MeteorLeap, "fire.fan");
        yield return new WaitForSeconds(0.4f);
        string wornFirst = upgrades.Worn != null ? upgrades.Worn.Name : "nothing";
        menu.Open();
        yield return null;
        menu.Choose("fire.fan");
        yield return Tap(Key.E);
        yield return new WaitForSecondsRealtime(0.5f);
        Shot("menu_worn");
        yield return null;
        yield return Tap(Key.Space);
        yield return new WaitForSecondsRealtime(0.4f);
        string wornSecond = upgrades.Worn != null ? upgrades.Worn.Name : "nothing";
        Place(new Vector3(0f, 0f, 1.2f), Vector3.forward);
        yield return null;
        set.Clear();
        combat.Mana.Gain(100f);
        mana = combat.Mana.Current;
        health = north.Health.Current;
        int wornCasts = upgrades.WornCasts;
        casts = element.SpellsCast;
        yield return Hold(Key.Q, 0.45f);
        yield return new WaitForSeconds(0.15f);
        Shot("cinder_fan");
        yield return new WaitForSeconds(0.3f);
        float fanTook = health - north.Health.Current;
        int fanBurn = north.Burn.Stacks;
        yield return Tap(Key.Q);
        yield return Until(() => element.SpellsCast > casts, 1.5f);
        Say($"S worn spell: first worn {wornFirst}; after choosing on the chart {wornSecond}; held: worn casts +{upgrades.WornCasts - wornCasts}, mana {mana:0} -> spent {mana - combat.Mana.Current:0} before the tap, dummy lost {fanTook:0}, burn {fanBurn} deep; a tap still cast the base spell {element.SpellsCast - casts}");
        yield return new WaitForSeconds(0.6f);

        // T: a mix mark on the ring. Sealed to Fire alone; with Poison consumed it opens and is bought on the chart. Pyre's
        // first mark, Fuel, now that Plasma's Charged Embers is retired (task 4.5; Plasma is in the chart's heart).
        upgrades.Forget();
        Learn(FireTree.Cinder);
        yield return new WaitForSeconds(0.4f);
        var ionised = Upgrade.ById(PyreTree.Fuel);
        bool openBefore = upgrades.Open(ionised);
        RunState.ElementsConsumed.Add(Elements.Poison);
        bool openAfter = upgrades.Open(ionised);
        upgrades.Earn(3);
        menu.Open();
        yield return null;
        menu.Choose(PyreTree.Fuel);
        yield return Tap(Key.E);
        yield return new WaitForSecondsRealtime(0.5f);
        Shot("menu_mix");
        yield return null;
        yield return Tap(Key.Space);
        yield return new WaitForSecondsRealtime(0.4f);
        bool bought = upgrades.Has(PyreTree.Fuel);
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        set.Clear();
        done = upgrades.EffectsDone;
        yield return Lights(1);
        yield return null;
        yield return null;
        // Fuel lays both on heavy blows only: a light blow with Cinder leaves one burn and no poison.
        Say($"T mix: open before Poison is consumed {openBefore}; with Poison consumed {openAfter}; bought on the chart {bought}; one light blow with Cinder and Fuel left burn {north.Burn.Stacks} deep (one) and poison {set.Stacks(AfflictionKind.Poison)} (none: Fuel answers heavy blows) (effects +{upgrades.EffectsDone - done})");
        RunState.ElementsConsumed.Clear();

        // U: the nine new elements. Each kit casts, lays its own affliction on the dummy, and lights its surge.
        string kits = "";
        foreach (var info in Elements.All)
        {
            if (info.Name == ElementKit.FireName || info.Name == ElementKit.LightningName) continue;
            element.Take(ElementKit.ByName(info.Name, false));
            upgrades.Forget();
            yield return new WaitForSeconds(0.3f);
            Place(new Vector3(0f, 0f, 1.6f), Vector3.forward);
            yield return null;
            set.Clear();
            combat.Mana.Gain(100f);
            casts = element.SpellsCast;
            health = north.Health.Current;
            yield return Tap(Key.Q);
            yield return Until(() => element.SpellsCast > casts, 1.5f);
            yield return null;
            if (info.Name == Elements.Ice || info.Name == Elements.Poison || info.Name == Elements.Dark) Shot("kit_" + info.Name.ToLowerInvariant());
            int laid = set.Stacks(info.Affliction);
            yield return new WaitForSeconds(0.45f);
            int surges = element.Surges;
            yield return Tap(Key.F);
            yield return Until(() => element.Surges > surges, 1f);
            yield return null;
            kits += $"{info.Name}: {element.Kit.SpellName} cast {element.SpellsCast - casts} took {health - north.Health.Current:0} laid {info.Affliction} {laid}; {element.Kit.SurgeName} lit {element.Surges - surges} wards {defence.Wards}. ";
            yield return Lights(1);
            if (info.Name == Elements.Wind) kits += $"(Wind's dodge goes {player.Dodge.Distance:0.0} m in its surge) ";
            yield return new WaitForSeconds(0.2f);
        }
        Say("U kits: " + kits);

        // V: the real trees. For four elements: the root, the road to the first spell, and one more mark, bought with embers
        // as the chart buys them; then blows and the worn spell on the dummy.
        foreach (string name in new[] { Elements.Wind, Elements.Ice, Elements.Earth, Elements.Poison })
        {
            element.Take(ElementKit.ByName(name, false));
            upgrades.Forget();
            RunState.Points = 0;
            upgrades.Earn(40);
            yield return new WaitForSeconds(0.3f);
            var info = Elements.ByName(name);
            Upgrade spell = null, root = null;
            foreach (var mark in Upgrade.All)
            {
                if (mark.Element != name || mark.IsMix) continue;
                if (mark.Kind == UpgradeKind.Root) root = mark;
                if (mark.Kind == UpgradeKind.Spell && spell == null) spell = mark;
            }
            var road = new List<Upgrade>();
            for (var at = spell; at != null; at = at.Requires.Length > 0 ? Upgrade.ById(at.Requires[0]) : null) road.Insert(0, at);
            string boughtNames = "";
            int boughtCount = 0;
            foreach (var mark in road)
                if (upgrades.TryBuy(mark)) { boughtNames += mark.Name + ", "; boughtCount++; }
            foreach (var mark in Upgrade.All)
            {
                if (boughtCount >= road.Count + 1) break;
                if (mark.Element == name && !mark.IsMix && mark.Kind == UpgradeKind.Minor && upgrades.TryBuy(mark)) { boughtNames += mark.Name + ", "; boughtCount++; }
            }
            yield return new WaitForSeconds(0.4f);
            Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
            yield return null;
            set.Clear();
            north.Burn.Reset();
            done = upgrades.EffectsDone;
            yield return Lights(2);
            yield return Tap(Key.K);
            yield return new WaitForSeconds(0.9f);
            yield return Until(() => !combat.IsAttacking, 2f);
            int laidByBlows = set.Stacks(info.Affliction);
            combat.Mana.Gain(100f);
            health = north.Health.Current;
            wornCasts = upgrades.WornCasts;
            yield return Hold(Key.Q, 0.45f);
            yield return new WaitForSeconds(0.5f);
            Shot("tree_" + name.ToLowerInvariant());
            yield return new WaitForSeconds(1.6f);
            Say($"V {name}: bought {boughtCount} for {40 - upgrades.Points} embers ({boughtNames.TrimEnd(',', ' ')}); two lights and a heavy laid {info.Affliction} {laidByBlows}; worn {(upgrades.Worn != null ? upgrades.Worn.Name + " (" + upgrades.Worn.Form + ")" : "nothing")} cast +{upgrades.WornCasts - wornCasts}, dummy lost {health - north.Health.Current:0}; effects +{upgrades.EffectsDone - done}");
            yield return new WaitForSeconds(0.5f);
        }

        // W: the gifts, from the two debug shrines. Ice held: its own gift; then Water consumed: Glacier opens, its crown is given,
        // and its first mark is bought and lays only the affliction not held on top of the root.
        var who = player.GetComponent<PlayerInteraction>();
        Clover.World.GiftShrine giftShrine = null, consumeShrine = null;
        foreach (var shrine in FindObjectsByType<Clover.World.GiftShrine>(FindObjectsSortMode.None))
        {
            if (shrine.Consumes) consumeShrine = shrine;
            else giftShrine = shrine;
        }
        if (giftShrine == null || consumeShrine == null) Say("W gifts: the debug shrines are missing from the yard");
        else
        {
            element.Take(ElementKit.ByName(Elements.Ice, false));
            upgrades.Forget();
            RunState.ElementsConsumed.Clear();
            RunState.ElementsHeld.Clear();
            RunState.ElementsHeld.Add(Elements.Ice);
            RunState.Points = 0;
            upgrades.Earn(10);
            yield return new WaitForSeconds(0.3f);
            // Glacier's foot is its Shape mark now, in the chart's heart (task 4.15): open only while Ice and Water are carried.
            var foot = Upgrade.ById(GlacierTree.Shape);
            bool sealedFirst = !upgrades.Open(foot);
            giftShrine.Use(who);
            yield return null;
            string own = giftShrine.LastGift;
            bool ownHeld = upgrades.Has(own);
            consumeShrine.Use(who);
            yield return null;
            string crown = consumeShrine.LastGift;
            bool crownHeld = upgrades.Has(crown), opened = upgrades.Open(foot);
            upgrades.TryBuy(Upgrade.ById(IceTree.Root));
            bool footBought = upgrades.TryBuy(foot);
            yield return new WaitForSeconds(0.4f);
            Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
            yield return null;
            set.Clear();
            yield return Lights(1);
            yield return null;
            yield return null;
            Shot("gift_shrines");
            Say($"W gifts: Glacier sealed to Ice alone {sealedFirst}; gift shrine gave {own} (held {ownHeld}); consume shrine marked Water consumed {RunState.ElementsConsumed.Contains(Elements.Water)}, gave {crown} (held {crownHeld}), Glacier open {opened}; {foot.Name} bought {footBought}; one light blow laid chill {set.Stacks(AfflictionKind.Chill)} (the root's one, not two) and soaked {set.Stacks(AfflictionKind.Soaked)}");
            // A gift that is a spell is worn: Poison consumes Fire and is left Light the Pyres.
            element.Take(ElementKit.ByName(Elements.Poison, false));
            upgrades.Forget();
            yield return new WaitForSeconds(0.3f);
            consumeShrine.Use(who);
            yield return new WaitForSeconds(0.3f);
            Say($"W gifted spell: consume shrine gave {consumeShrine.LastGift}; worn now {(upgrades.Worn != null ? upgrades.Worn.Id : "nothing")}");
            RunState.ElementsConsumed.Clear();
        }

        // X: the engine under the newer words, one line each.
        TrainingDummy second2 = null;
        foreach (var dummy in FindObjectsByType<TrainingDummy>(FindObjectsSortMode.None))
            if (dummy != north && (second2 == null || Vector3.Distance(dummy.transform.position, north.transform.position) < Vector3.Distance(second2.transform.position, north.transform.position))) second2 = dummy;
        Vector3 secondHome = second2.transform.position;
        second2.transform.position = north.transform.position + new Vector3(2.5f, 0f, 0f);
        Physics.SyncTransforms();
        // X1: a number that waits on a state (Life, Stubborn Root: less harm below a third).
        element.Take(ElementKit.ByName(Elements.Life, false));
        upgrades.Forget();
        Learn(LifeTree.Root, "life.stubborn", "life.rind");
        defence.Restore();
        yield return new WaitForSeconds(0.4f);
        float takenFull = defence.DamageTaken;
        defence.Health.Damage(75f);
        yield return new WaitForSeconds(0.2f);
        float takenLow = defence.DamageTaken;
        // X2: Hold (Life, Rind: a perfect guard roots the attacker).
        int holds = upgrades.Holds;
        set.Clear();
        upgrades.OnPerfectGuard(north.transform.position);
        yield return null;
        yield return null;
        bool heldNow = set.IsHeld;
        float heldFor = set.HeldLeft;
        Say($"X1 state: damage taken x{takenFull:0.00} at full health, x{takenLow:0.00} below a third (Stubborn Root). X2 hold: a perfect guard held the dummy {heldNow} for {heldFor:0.0} s (speed x{set.SpeedMultiplier:0.00}, holds +{upgrades.Holds - holds})");
        defence.Restore();
        // X3: a number against an affliction, a base-spell-only cost, and a kill that knows what the dead carried (Fire).
        element.Take(ElementKit.Fire());
        upgrades.Forget();
        Learn(FireTree.Cinder, "fire.tinder", "fire.shortbreath", "fire.catch");
        yield return new WaitForSeconds(0.4f);
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        set.Clear();
        north.Burn.Reset();
        yield return new WaitForSeconds(0.1f);
        yield return Lights(1);
        float onClean = north.LastDamage;
        yield return Lights(1);
        float onBurning = north.LastDamage;
        combat.Mana.Gain(100f);
        mana = combat.Mana.Current;
        casts = element.SpellsCast;
        yield return Tap(Key.Q);
        yield return Until(() => element.SpellsCast > casts, 1.5f);
        float baseCost = mana - combat.Mana.Current;
        yield return new WaitForSeconds(0.6f);
        second2.Afflictions.Clear();
        second2.Burn.Reset();
        var carried = new AfflictionSet();
        carried.Apply(AfflictionKind.Burn, 3);
        CombatEvents.RaiseDied(north, carried);
        yield return null;
        yield return null;
        int caught = second2.Burn.Stacks;
        CombatEvents.RaiseDied(north, new AfflictionSet());
        Say($"X3 against: the same light blow took {onClean:0.0} from a clean dummy and {onBurning:0.0} from a burning one (Dry Tinder). Base spell alone: Flame Wave cost {baseCost:0} (Short Breath). Kill with what it carried: a death burning three deep left the dummy beside it burning {caught} deep (Catching Fire)");
        // X4: a count banked and spent (Metal, Forged Answer), and the HUD's words for it.
        element.Take(ElementKit.ByName(Elements.Metal, false));
        upgrades.Forget();
        Learn(MetalTree.Root, "metal.forgedanswer");
        yield return new WaitForSeconds(0.4f);
        upgrades.OnPerfectGuard(north.transform.position);
        yield return null;
        upgrades.OnPerfectGuard(north.transform.position);
        yield return null;
        yield return null;
        int banked = upgrades.Tally("metal.forgedanswer");
        string words = upgrades.TallyText.Trim();
        Shot("tally_hud");
        health = north.Health.Current;
        yield return Tap(Key.K);
        yield return new WaitForSeconds(1.6f);
        Say($"X4 count: two perfect guards banked {banked} (HUD: '{words}'); the heavy blow spent them, left {upgrades.Tally("metal.forgedanswer")}, and with the blades took {health - north.Health.Current:0}; sundered {set.Stacks(AfflictionKind.Sundered)}");
        // X5: health as a price (Dark, Blood Price) and a flask filled by parts (Water, Catch the Rain).
        element.Take(ElementKit.ByName(Elements.Dark, false));
        upgrades.Forget();
        Learn("dark.curse", "dark.bloodprice");
        defence.Restore();
        yield return new WaitForSeconds(0.4f);
        combat.Mana.TrySpend(combat.Mana.Current);
        combat.Mana.Gain(5f);
        casts = element.SpellsCast;
        yield return Tap(Key.Q);
        yield return Until(() => element.SpellsCast > casts, 1.5f);
        float paid = upgrades.HealthPaid, grey = defence.Health.Grey;
        element.Take(ElementKit.ByName(Elements.Water, false));
        upgrades.Forget();
        Learn(WaterTree.Root, "water.catch");
        defence.Restore();
        yield return new WaitForSeconds(0.4f);
        defence.Flasks.TryThrow();
        int flasksLow = defence.Flasks.Count;
        for (int i = 0; i < 4; i++)
        {
            upgrades.OnPerfectGuard(north.transform.position);
            yield return null;
            yield return null;
        }
        Say($"X5 health as a price: Grasp cast with 5 mana {element.SpellsCast - casts}, health paid {paid:0.0}, left grey {grey:0.0} (Blood Price). Flask by parts: {flasksLow} flasks, four perfect guards later {defence.Flasks.Count} (Catch the Rain)");
        // X6: a shatter that is told (Ice, Shards), a set-off that carries its depth (Lightning, Full Discharge), the last blow of a chain (Light's root).
        element.Take(ElementKit.ByName(Elements.Ice, false));
        upgrades.Forget();
        Learn(IceTree.Root, "ice.shards");
        yield return new WaitForSeconds(0.4f);
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        set.Clear();
        second2.Afflictions.Clear();
        north.TakeHit(Lay(AfflictionKind.Chill, 5));
        bool wasFrozen = set.IsFrozen;
        yield return Tap(Key.K);
        yield return new WaitForSeconds(1.2f);
        int shardChill = second2.Afflictions.Stacks(AfflictionKind.Chill);
        element.Take(ElementKit.Lightning());
        upgrades.Forget();
        Learn(Upgrade.Static, "lightning.fulldischarge");
        yield return new WaitForSeconds(0.4f);
        set.Clear();
        north.TakeHit(Lay(AfflictionKind.Static, 5));
        mana = combat.Mana.Current;
        holds = upgrades.Holds;
        yield return Tap(Key.K);
        yield return new WaitForSeconds(1.0f);
        bool heldByDischarge = upgrades.Holds > holds;
        yield return Until(() => !combat.IsAttacking, 2f);
        element.Take(ElementKit.ByName(Elements.Light, false));
        upgrades.Forget();
        Learn(LightTree.Root);
        yield return new WaitForSeconds(0.4f);
        set.Clear();
        yield return Lights(1);
        int markedEarly = set.Stacks(AfflictionKind.Marked);
        yield return new WaitForSeconds(0.8f);
        yield return Lights(combat.Combo.Steps.Count);
        int markedEnd = set.Stacks(AfflictionKind.Marked);
        int blowsLeft = set.BlowsLeft(AfflictionKind.Marked);
        Say($"X6 shatter: frozen {wasFrozen}, the heavy blow broke it and the shards chilled the dummy beside it {shardChill} deep (Shards). Set-off: a discharge of five held the dummy {heldByDischarge} (Full Discharge). Chain end: marked after one light blow {markedEarly}, after a whole chain {markedEnd} with {blowsLeft} critical blows left (Light's root)");
        // X7: the air (Wind, Rising Cut and Open Air), with the dummy five deep in Unsteady.
        element.Take(ElementKit.ByName(Elements.Wind, false));
        upgrades.Forget();
        Learn(WindTree.Unsteady, "wind.rising", "wind.openair", "wind.hang");
        yield return new WaitForSeconds(0.4f);
        Place(new Vector3(0f, 0f, 2.2f), Vector3.forward);
        yield return null;
        set.Clear();
        north.TakeHit(Lay(AfflictionKind.Unsteady, 5));
        int lifts = upgrades.Lifts;
        float floor = north.transform.position.y;
        yield return Tap(Key.K);
        yield return Until(() => set.IsLifted, 2f);
        bool up = set.IsLifted;
        float hangs = set.AirLeft;
        yield return new WaitForSeconds(0.3f);
        float rose = north.transform.position.y - floor;
        Shot("in_the_air");
        yield return Until(() => !combat.IsAttacking, 1.5f);
        yield return Lights(1);
        float inAirBlow = north.LastDamage;
        int heldUp = set.AirBlows;
        yield return Until(() => !set.IsLifted, 4f);
        yield return new WaitForSeconds(0.2f);
        set.Clear();
        yield return Lights(1);
        Say($"X7 air: a heavy blow on five Unsteady threw the dummy up {up} (lifts +{upgrades.Lifts - lifts}) to hang {hangs:0.0} s, {rose:0.0} m off the ground; a light blow up there took {inAirBlow:0.0} and held it up ({heldUp} blows counted); the same blow on the ground took {north.LastDamage:0.0}; down again at {north.transform.position.y - floor:0.00} m, unsteady left {set.Stacks(AfflictionKind.Unsteady)}");
        // X8: what is grown (Life: saplings to a limit, standing near them, the harvest) and a hand-written number changed (Fire, Stoked).
        element.Take(ElementKit.ByName(Elements.Life, false));
        upgrades.Forget();
        Learn(LifeTree.Root, "life.sapling", "life.shade", "life.reap");
        defence.Restore();
        yield return new WaitForSeconds(0.4f);
        int grownBefore = Clover.World.Hazards.GrownCount;
        for (int i = 0; i < 4; i++)
        {
            Place(new Vector3(-6f + i * 1.5f, 0f, -6f), Vector3.forward);
            yield return null;
            upgrades.OnPerfectGuard(player.transform.position + Vector3.forward);
            yield return null;
            yield return null;
        }
        int grown = Clover.World.Hazards.GrownCount - grownBefore;
        bool near = upgrades.Stats.Holds(StatWhen.NearGrown);
        Place(new Vector3(8f, 0f, 8f), Vector3.forward);
        yield return new WaitForSeconds(0.1f);
        bool farOff = upgrades.Stats.Holds(StatWhen.FarFromGrown);
        defence.Health.Damage(40f);
        myHealth = defence.Health.Current;
        combat.Mana.Gain(100f);
        yield return Hold(Key.Q, 0.45f);
        yield return new WaitForSeconds(0.6f);
        Say($"X8 grown: four saplings planted, {grown} stand (three at most); near one {near}; far from all {farOff}; the harvest burst them (left {Clover.World.Hazards.GrownCount - grownBefore}) and mended {defence.Health.Current - myHealth:0}");
        element.Take(ElementKit.Fire());
        upgrades.Forget();
        defence.Restore();
        yield return new WaitForSeconds(0.3f);
        int blowsBefore = upgrades.Heat.BlowsPerLevel;
        Learn(FireTree.Cinder, Upgrade.Heat, "fire.stoked", "fire.banked", FireTree.WhiteHeat);
        yield return new WaitForSeconds(0.4f);
        Say($"X8 hand-written numbers: Heat rose every {blowsBefore} blows, with Stoked every {upgrades.Heat.BlowsPerLevel}; cools after {upgrades.Heat.CoolAfter:0.0} s with Banked; rises to {upgrades.Heat.MaximumLevel} with White Heat");
        second2.Afflictions.Clear();
        second2.transform.position = secondHome;
        Physics.SyncTransforms();
        defence.Restore();

        element.Take(ElementKit.Fire());
        upgrades.Forget();
        set.Clear();

                Say("done");
        Destroy(gameObject);
    }
}
