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

        // A: the chart. Opened as a rest fire would, it stops the world; W moves outward, E learns, K shows the whole ring, Space leaves.
        Place(new Vector3(6f, 0f, -6f), Vector3.forward);
        upgrades.Earn(10);
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
        menu.Choose(Upgrade.Heat);
        yield return Tap(Key.E);
        bool heat = upgrades.Owns(Upgrade.Heat);
        yield return Tap(Key.W);
        string selected = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        yield return Tap(Key.E);
        bool second = upgrades.Owns(Upgrade.BurningCrescent);
        yield return Tap(Key.W);
        string minor = menu.Chosen != null ? menu.Chosen.Name : "nothing";
        yield return Tap(Key.E);
        yield return Tap(Key.W);
        string above = menu.Chosen != null ? menu.Chosen.Name : "nothing";
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
        // A mixed branch the player cannot open is there to be read, sealed.
        menu.Choose("plasma.ionised");
        yield return Tap(Key.E);
        bool sealedBought = upgrades.Owns("plasma.ionised");
        yield return new WaitForSecondsRealtime(0.5f);
        Shot("menu_sealed");
        yield return null;
        Vector3 before = player.transform.position;
        yield return Tap(Key.Space);
        yield return new WaitForSecondsRealtime(0.4f);
        Say($"A chart: {menu.MarkCount} marks; stopped {stopped} far end before its foot {early} heat learned {heat} W moved to {selected}, learned {second}; W to {minor}; W to {above}, learned {third}, worn {wornNow}; A moved to {left}; whole ring {wide} at zoom {far:0.00} then back {!menu.IsWide} at {menu.Zoom:0.00}; sealed mix bought {sealedBought}; embers left {upgrades.Points} closed {!menu.IsOpen} time {Time.timeScale:0.0} moved {(player.transform.position - before).magnitude:0.00}");

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
        Learn(Upgrade.Heat, Upgrade.Backdraft, Upgrade.BurningCrescent, "fire.kindling", "fire.stoked");
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

        // R: effects that are only data. Searing Guard burns whoever is blocked; Grounding arcs from a perfect guard; Slipstream quickens after a perfect dodge.
        Learn(Upgrade.BurnAndDetonate, "fire.searing");
        yield return new WaitForSeconds(0.4f);
        set.Clear();
        int done = upgrades.EffectsDone;
        upgrades.OnGuard(north.transform.position);
        yield return null;
        yield return null;
        Say($"R1 searing guard: effects done +{upgrades.EffectsDone - done}; the blocked dummy burns {north.Burn.IsBurning} {north.Burn.Stacks} deep");
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
        Learn(Upgrade.Heat, Upgrade.BurningCrescent, Upgrade.MeteorLeap, "fire.fan");
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
        yield return Hold(Key.U, 0.45f);
        yield return new WaitForSeconds(0.15f);
        Shot("cinder_fan");
        yield return new WaitForSeconds(0.3f);
        float fanTook = health - north.Health.Current;
        int fanBurn = north.Burn.Stacks;
        yield return Tap(Key.U);
        yield return Until(() => element.SpellsCast > casts, 1.5f);
        Say($"S worn spell: first worn {wornFirst}; after choosing on the chart {wornSecond}; held: worn casts +{upgrades.WornCasts - wornCasts}, mana {mana:0} -> spent {mana - combat.Mana.Current:0} before the tap, dummy lost {fanTook:0}, burn {fanBurn} deep; a tap still cast the base spell {element.SpellsCast - casts}");
        yield return new WaitForSeconds(0.6f);

        // T: a mix mark. Sealed to Fire alone; with the storm consumed it opens, is bought on the chart, and answers an affliction reaching full depth.
        upgrades.Forget();
        Learn(Upgrade.Heat);
        yield return new WaitForSeconds(0.4f);
        var ionised = Upgrade.ById("plasma.ionised");
        bool openBefore = upgrades.Open(ionised);
        RunState.ElementsConsumed.Add(ElementKit.LightningName);
        bool openAfter = upgrades.Open(ionised);
        upgrades.Earn(3);
        menu.Open();
        yield return null;
        menu.Choose("plasma.ionised");
        yield return Tap(Key.E);
        yield return new WaitForSecondsRealtime(0.5f);
        Shot("menu_mix");
        yield return null;
        yield return Tap(Key.Space);
        yield return new WaitForSecondsRealtime(0.4f);
        bool bought = upgrades.Has("plasma.ionised");
        set.Clear();
        health = north.Health.Current;
        done = upgrades.EffectsDone;
        north.TakeHit(Lay(AfflictionKind.Burn, 5));
        yield return null;
        yield return null;
        Say($"T mix: open before the storm is consumed {openBefore} (true here: Lightning was held earlier in this play; step A shows it sealed); with Lightning consumed {openAfter}; bought on the chart {bought}; a burn brought to five deep set off an arc (effects +{upgrades.EffectsDone - done}) that took {health - north.Health.Current:0}");
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
            yield return Tap(Key.U);
            yield return Until(() => element.SpellsCast > casts, 1.5f);
            yield return null;
            if (info.Name == Elements.Ice || info.Name == Elements.Poison || info.Name == Elements.Dark) Shot("kit_" + info.Name.ToLowerInvariant());
            int laid = set.Stacks(info.Affliction);
            yield return new WaitForSeconds(0.45f);
            int surges = element.Surges;
            yield return Tap(Key.I);
            yield return Until(() => element.Surges > surges, 1f);
            yield return null;
            kits += $"{info.Name}: {element.Kit.SpellName} cast {element.SpellsCast - casts} took {health - north.Health.Current:0} laid {info.Affliction} {laid}; {element.Kit.SurgeName} lit {element.Surges - surges} wards {defence.Wards}. ";
            yield return Lights(1);
            if (info.Name == Elements.Wind) kits += $"(Wind's dodge goes {player.Dodge.Distance:0.0} m in its surge) ";
            yield return new WaitForSeconds(0.2f);
        }
        Say("U kits: " + kits);
        element.Take(ElementKit.Fire());
        upgrades.Forget();
        set.Clear();

                Say("done");
        Destroy(gameObject);
    }
}
