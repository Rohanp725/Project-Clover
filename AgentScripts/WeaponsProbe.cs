using System;
using System.Collections;
using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe for the newer weapons (Massive Sword, Scythe, Armament, Great Spear, Great Hammer). In CombatTest, for each: equips it, throws
// the whole light chain, the heavy, the charged heavy, the chain heavies (all four enders for the Armament), the skill
// and the finisher at a training dummy, and writes what each dealt (damage, stagger, how far off the target stood, how
// long it took) beside the design's numbers; then casts the held element's spell with the weapon in hand and lays an
// affliction with a blow. Frames at the telling moments go to review/shots/current/weapon_<name>_<move>_<n>.png.
// In Castle (or anywhere with real enemies) it instead fights the nearest soldier once with each weapon.
// The spear also thrusts from behind its guard, throws itself and is run to (into the dummy, into the ground, and
// refused); the hammer's toss is timed out and back, and its late perfect guard is tried early and on time.
// To run only some weapons, write their clip prefixes on one line in AgentScripts/WeaponsProbe.only (e.g. "spear hammer");
// with no such file every weapon is run.
//   bash AgentScripts/runprobe.sh CombatTest WeaponsProbe 250
public static class WeaponsProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Weapons Probe").AddComponent<WeaponsProbeRunner>();
        return "probe started";
    }
}

public class WeaponsProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private TrainingDummy dummy;
    private string weaponTag = "";
    private bool keepWell = true;

    private void Update()
    {
        if (!keepWell || defence == null) return;
        defence.Health.Heal(100f);
        player.Stamina.Fill();
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private IEnumerator Shot(string move, int n)
    {
        ScreenCapture.CaptureScreenshot($"review/shots/current/weapon_{weaponTag}_{move}_{n}.png");
        yield return null;
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
    }

    // Stands the player off the dummy by a distance, facing it, with the dummy whole again.
    private IEnumerator Stage(float distance)
    {
        yield return Until(() => !combat.IsAttacking, 3f);
        yield return new WaitForSecondsRealtime(0.25f);
        dummy.Health.Fill();
        dummy.Stagger.Reset();
        Vector3 at = dummy.transform.position - Vector3.forward * distance;
        player.Teleport(at, Quaternion.LookRotation(Vector3.forward, Vector3.up));
        combat.Mana.Gain(100f);
        yield return new WaitForSecondsRealtime(0.15f);
    }

    // Watches one attack from its start to its end and says what it did. Pictures: the end of the wind-up, the blow, and a little after.
    private IEnumerator Watch(string move, AttackStep expected, string design, bool pictures = true)
    {
        yield return Until(() => combat.Combo.Current == expected, 2f);
        if (combat.Combo.Current != expected)
        {
            Say($"{weaponTag} {move}: did NOT start (doing {(combat.Combo.Current != null ? combat.Combo.Current.Name : "nothing")})");
            yield break;
        }
        float began = Time.time;
        float health = dummy.Health.Current, stagger = dummy.Stagger.Value;
        int blows = combat.BlowsLanded, sweet = combat.SweetBlows;
        Vector3 from = player.transform.position;
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup || combat.Combo.StepProgress * expected.Total >= expected.Windup * 0.85f, 3f);
        if (pictures) yield return Shot(move, 1);
        yield return Until(() => combat.Combo.Current != expected || combat.Combo.Phase != AttackPhase.Windup, 3f);
        float hitAt = Time.time - began;
        yield return null;
        if (pictures) yield return Shot(move, 2);
        yield return new WaitForSecondsRealtime(0.14f);
        if (pictures) yield return Shot(move, 3);
        yield return Until(() => combat.Combo.Current != expected || combat.Combo.Phase == AttackPhase.Recovery, 3f);
        float moved = Vector3.Dot(player.transform.position - from, Vector3.forward);
        bool staggered = dummy.IsStaggered;
        Say($"{weaponTag} {move} ({expected.Name}): blows +{combat.BlowsLanded - blows} damage {health - dummy.Health.Current:0.#} stagger {(staggered ? "BROKEN" : (dummy.Stagger.Value - stagger).ToString("0.#"))}"
            + $" from {combat.LastBlowDistance:0.00} m (reach {expected.Reach}) tip blows +{combat.SweetBlows - sweet}; hit at {hitAt:0.00} s (wind-up {expected.Windup}), moved {moved:0.00} m; design: {design}");
    }

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        yield return new WaitForSeconds(0.5f);

        var director = FindAnyObjectByType<AreaDirector>();
        if (director != null)
        {
            yield return RealEnemies(director);
            Say("done: weapons against real enemies");
            Destroy(gameObject);
            yield break;
        }

        // The lone dummy north of the middle.
        foreach (var candidate in FindObjectsByType<TrainingDummy>())
            if (candidate.GetComponent<DummyAttacker>() == null && (dummy == null || Mathf.Abs(candidate.transform.position.x) < Mathf.Abs(dummy.transform.position.x))) dummy = candidate;
        if (dummy == null) { Say("done: no dummy"); yield break; }
        dummy.RestoreAfter = 1000f;
        var rack = FindObjectsByType<LoadoutChoice>();
        Say($"start: rack stands {rack.Length}, dummy at {dummy.transform.position}, element {element.Info.Name}");

        string only = System.IO.File.Exists("AgentScripts/WeaponsProbe.only") ? " " + System.IO.File.ReadAllText("AgentScripts/WeaponsProbe.only").Trim() + " " : null;
        if (only != null) Say($"only:{only}");
        if (only == null || only.Contains(" gsword ")) yield return Sword();
        if (only == null || only.Contains(" scythe ")) yield return Scythe();
        if (only == null || only.Contains(" arm ")) yield return Armament();
        if (only == null || only.Contains(" spear ")) yield return Spear();
        if (only == null || only.Contains(" hammer ")) yield return Hammer();

        Say($"errors none seen by the probe; time scale {Time.timeScale:0.00}");
        Say("done: weapons");
        Destroy(gameObject);
    }

    private IEnumerator Common(Weapon weapon, float stand)
    {
        var combo = weapon.Combo;
        Say($"{weaponTag}: equipped {combat.Weapon.Name} shown as '{combat.Weapon.ShownName}', guard arc {defence.Guard.ArcDegrees} perfect {defence.Guard.PerfectWindow} cost {defence.Guard.StaminaPerDamage}, dodge {player.Dodge.Distance} m in {player.Dodge.Duration} s for {player.Dodge.StaminaCost}");
        // The light chain, each press made as the last attack's swing ends.
        yield return Stage(stand);
        float chainBegan = Time.time, chainHealth = dummy.Health.Current;
        float staminaBefore = player.Stamina.Current;
        keepWell = false;
        for (int i = 0; i < combo.Steps.Count; i++)
        {
            yield return Tap(Key.J);
            var step = combo.Steps[i];
            yield return Watch("light" + (i + 1), step, $"damage {step.Damage} stagger {step.Stagger}");
            dummy.Stagger.Reset();
        }
        float spent = staminaBefore - player.Stamina.Current;
        keepWell = true;
        yield return Until(() => !combat.IsAttacking, 3f);
        Say($"{weaponTag} light chain: {Time.time - chainBegan:0.00} s, damage {chainHealth - dummy.Health.Current:0.#}, stamina about {spent:0.#}");

        yield return Stage(stand);
        yield return Tap(Key.K);
        yield return Watch("heavy", combo.Heavy, $"damage {combo.Heavy.Damage} stagger {combo.Heavy.Stagger} lunge {combo.Heavy.Lunge}");
    }

    private IEnumerator Hold(float seconds)
    {
        Keys(Key.K);
        yield return new WaitForSeconds(seconds);
        Keys();
    }

    private IEnumerator Finish(Weapon weapon, float stand)
    {
        var step = weapon.Combo.Finisher;
        yield return Stage(stand);
        dummy.TakeHit(new Hit(0f, 1000f, Vector3.forward));
        yield return null;
        int finishers = combat.Finishers;
        float health = dummy.Health.Current;
        yield return Tap(Key.K);
        yield return Until(() => combat.IsFinishing, 1f);
        float began = Time.time, real = Time.unscaledTime;
        yield return new WaitForSeconds(Mathf.Max(0.05f, step.HoldAt * 0.5f));
        yield return Shot("finisher", 1);
        yield return Until(() => combat.Combo.Elapsed >= step.HoldAt + 0.04f || !combat.IsFinishing, 3f);
        float holdScale = Time.timeScale;
        yield return Shot("finisher", 2);
        yield return Until(() => combat.Finishers > finishers || !combat.IsFinishing, 4f);
        float hitAt = Time.time - began, hitReal = Time.unscaledTime - real;
        yield return null;
        yield return Shot("finisher", 3);
        yield return new WaitForSecondsRealtime(0.4f);
        yield return Shot("finisher", 4);
        Say($"{weaponTag} finisher ({step.Name}): staggered first, finishers +{combat.Finishers - finishers}, damage {health - dummy.Health.Current:0.#} (step {step.Damage} x {dummy.FinisherDamageMultiplier}), blow at {hitAt:0.00} game s / {hitReal:0.00} real s (wind-up {step.Windup}), time scale in the hold {holdScale:0.00}, beats {(step.Beats != null ? step.Beats.Length : 0)}");
        yield return new WaitForSecondsRealtime(1.2f);
    }

    // The held element with the weapon in hand: its spell is cast, and a blow lays its affliction as the tree's root makes it.
    private IEnumerator Element(Weapon weapon, float stand, bool laysByItself)
    {
        yield return Stage(stand);
        int casts = element.SpellsCast;
        yield return Tap(Key.U);
        yield return new WaitForSeconds(0.9f);
        bool cast = element.SpellsCast > casts;
        yield return Stage(stand);
        dummy.Afflictions.Clear();
        if (!laysByItself)
        {
            combat.WeaponAffliction = element.Info.Affliction;
            combat.WeaponAfflictionStacks = 1;
        }
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Phase == AttackPhase.Recovery, 2f);
        bool laid = dummy.Afflictions.Any || dummy.Burn.IsBurning;
        if (!laysByItself)
        {
            combat.WeaponAffliction = AfflictionKind.None;
            combat.WeaponAfflictionStacks = 0;
        }
        Say($"{weaponTag} with {element.Info.Name}: spell cast {cast} (casts +{element.SpellsCast - casts}); a blow laid {element.Info.Affliction}: {laid}{(laysByItself ? " by the coat itself" : " as the tree's root would")}");
        dummy.Afflictions.Clear();
    }

    private IEnumerator Sword()
    {
        weaponTag = "gsword";
        var weapon = Weapons.ByName(Weapons.MassiveSwordName);
        combat.Equip(weapon);
        var combo = weapon.Combo;
        yield return Common(weapon, 2.6f);

        yield return Stage(2.6f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        yield return Tap(Key.K);
        yield return Watch("chain", combo.ChainHeavy, $"damage {combo.ChainHeavy.Damage} stagger {combo.ChainHeavy.Stagger}");

        yield return Stage(3.2f);
        yield return Hold(combo.ChargeTime + 0.15f);
        yield return Watch("charged", combo.ChargedHeavy, $"damage {combo.ChargedHeavy.Damage} stagger {combo.ChargedHeavy.Stagger}, released from tier 1 at {combo.ChargeTime} s");

        // Poise: a blow of 10 inside the heavy's wind-up does not stop it; a blow of 30 does.
        yield return Stage(2.6f);
        yield return Tap(Key.K);
        yield return Until(() => combat.Combo.Current == combo.Heavy, 1f);
        yield return new WaitForSeconds(0.2f);
        keepWell = false;
        float before = defence.Health.Current;
        defence.TakeHit(new Hit(10f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        yield return null;
        bool went = combat.Combo.Current == combo.Heavy;
        Say($"gsword poise: a blow of 10 in the heavy's wind-up: health {before:0} -> {defence.Health.Current:0}, swing went on {went}, poised blows {defence.PoisedBlows}");
        keepWell = true;
        yield return Until(() => !combat.IsAttacking, 3f);
        yield return Stage(2.6f);
        yield return Tap(Key.K);
        yield return Until(() => combat.Combo.Current == combo.Heavy, 1f);
        yield return new WaitForSeconds(0.2f);
        defence.TakeHit(new Hit(30f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        yield return null;
        Say($"gsword poise: a blow of 30 in the same place: swing went on {combat.Combo.Current == combo.Heavy} (wanted False)");
        yield return new WaitForSeconds(0.8f);

        // The Spike Line: held to the second tier, with the mana; the dummy stands well down the lane.
        yield return Stage(7f);
        var spikes = player.GetComponent<WeaponSpikes>();
        int raised = spikes.Raised, struck = spikes.Struck, skills = combat.SkillsUsed;
        float health = dummy.Health.Current, mana = combat.Mana.Current;
        yield return Hold(combo.SkillChargeTime + 0.15f);
        yield return Until(() => combat.Combo.Current == combo.Skill, 1f);
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup, 1f);
        yield return Shot("skill", 1);
        yield return new WaitForSeconds(0.35f);
        yield return Shot("skill", 2);
        yield return new WaitForSeconds(0.5f);
        yield return Shot("skill", 3);
        Say($"gsword skill (Elemental Spike Line): skills +{combat.SkillsUsed - skills}, mana {mana:0} -> {combat.Mana.Current:0} (cost {combo.SkillManaCost}), spikes raised +{spikes.Raised - raised} (design 8), struck +{spikes.Struck - struck}, dummy 7 m down the lane lost {health - dummy.Health.Current:0.#} (design 34)");
        yield return new WaitForSeconds(1.2f);

        yield return Finish(weapon, 3f);
        yield return Element(weapon, 2.6f, false);
    }

    private IEnumerator Scythe()
    {
        weaponTag = "scythe";
        var weapon = Weapons.ByName(Weapons.ScytheName);
        combat.Equip(weapon);
        var combo = weapon.Combo;
        // At the tip: the dummy's nearest side 2.9 m off.
        yield return Common(weapon, 3.35f);

        // The three bands with the first cut: haft, middle, tip.
        foreach (float stand in new[] { 1.4f, 2.4f, 3.35f })
        {
            yield return Stage(stand);
            float health = dummy.Health.Current, stagger = dummy.Stagger.Value;
            yield return Tap(Key.J);
            yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
            Say($"scythe bands: standing {stand} m off (nearest side {combat.LastBlowDistance:0.00} m): damage {health - dummy.Health.Current:0.#} stagger {dummy.Stagger.Value - stagger:0.#} (plain 13 and 12; haft x0.6 under 1.5 m, tip x1.5 and x1.6 from 2.5 m)");
        }

        yield return Stage(3.35f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        yield return Tap(Key.K);
        yield return Watch("chain", combo.ChainHeavy, $"damage {combo.ChainHeavy.Damage} stagger {combo.ChainHeavy.Stagger}");

        // The back reap from too close: the step back should put the enemy at the tip.
        yield return Stage(1.9f);
        yield return Tap(Key.K);
        yield return Watch("reap", combo.Heavy, $"from 1.9 m off, a hop back of {-combo.Heavy.Lunge} m; damage {combo.Heavy.Damage} x1.5 at the tip");

        // The Crescent: thrown past the dummy, it should strike on the way and then hang and bite.
        yield return Stage(6.2f);
        var shots = player.GetComponent<WeaponShots>();
        int thrown = shots.Thrown, struck = shots.Struck, bites = shots.Bites, skills = combat.SkillsUsed;
        float before = dummy.Health.Current;
        yield return Hold(combo.ChargeTime + 0.15f);
        yield return Until(() => combat.Combo.Current == combo.Skill, 1f);
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup, 1f);
        yield return Shot("skill", 1);
        yield return new WaitForSeconds(0.3f);
        yield return Shot("skill", 2);
        yield return new WaitForSeconds(0.6f);
        bool hanging = shots.Hanging > 0;
        yield return Shot("skill", 3);
        yield return new WaitForSeconds(1.6f);
        Say($"scythe skill (Crescent Slash): skills +{combat.SkillsUsed - skills}, thrown +{shots.Thrown - thrown}, struck in flight +{shots.Struck - struck} (design 20 damage), hanging after 0.9 s {hanging}, bites +{shots.Bites - bites} (design 6 every 0.4 s for 1.6 s), dummy lost {before - dummy.Health.Current:0.#}, still hanging at the end {shots.Hanging > 0}");

        yield return Finish(weapon, 3f);
        yield return Element(weapon, 3.35f, false);
    }

    private IEnumerator Armament()
    {
        weaponTag = "arm";
        combat.Equip(Weapons.ByName(Weapons.ArmamentName));
        // Made in the held element.
        combat.Equip(Weapons.Armament(element.Info.Name));
        var weapon = combat.Weapon;
        var combo = weapon.Combo;
        yield return Common(weapon, 1.9f);

        string[] names = { "", "ram", "rising", "slam", "burst" };
        for (int lights = 1; lights <= 4; lights++)
        {
            yield return Stage(lights == 4 ? 2.2f : 1.9f);
            for (int i = 0; i < lights; i++)
            {
                yield return Tap(Key.J);
                yield return Until(() => combat.Combo.Current == combo.Steps[i] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
            }
            int shoves = dummy.Shoves;
            yield return Tap(Key.K);
            var ender = combo.ChainHeavies[lights];
            yield return Watch(names[lights], ender, $"damage {ender.Damage} stagger {ender.Stagger}" + (ender.Travel > 0f ? $" travel {ender.Travel} shove {-ender.Pull}" : "") + (ender.Radius > 0f ? $" disc {ender.Radius}" : ""));
            if (lights == 1) Say($"arm ram: the dummy was shoved +{dummy.Shoves - shoves} times, last by {dummy.LastShove.magnitude:0.0} m");
        }

        yield return Stage(3.2f);
        yield return Hold(combo.ChargeTime + 0.2f);
        yield return Watch("meteor", combo.ChargedHeavy, $"damage {combo.ChargedHeavy.Damage} stagger {combo.ChargedHeavy.Stagger} leap {combo.ChargedHeavy.Lunge}");

        // Energy Reflect: braced, a blow of 20 from the dummy's side is taken on the coat and thrown back.
        yield return Stage(2.5f);
        keepWell = false;
        Keys(Key.K);
        yield return Until(() => combat.Combo.IsCharged, 1f);
        yield return new WaitForSeconds(0.15f);
        yield return Shot("reflect", 1);
        float health = defence.Health.Current, mana = combat.Mana.Current, dummyHealth = dummy.Health.Current, dummyStagger = dummy.Stagger.Value;
        int counters = combat.Counters;
        var outcome = defence.TakeHit(new Hit(20f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        Keys();
        yield return null;
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup, 1f);
        yield return null;
        yield return Shot("reflect", 2);
        yield return new WaitForSecondsRealtime(0.2f);
        yield return Shot("reflect", 3);
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"arm skill (Energy Reflect): braced, a blow of 20 came back as {outcome}; counters +{combat.Counters - counters}; player health {health:0} -> {defence.Health.Current:0}; mana {mana:0} -> {combat.Mana.Current:0} (cost 25); the dummy lost {dummyHealth - dummy.Health.Current:0.#} (design 20 + the blow's 20) and its balance {(dummy.IsStaggered ? "BROKE" : (dummy.Stagger.Value - dummyStagger).ToString("0.#"))} (design 40 + 55)");
        // With no mana the stance is only a stance.
        yield return Stage(2.5f);
        combat.Mana.Empty();
        Keys(Key.K);
        yield return Until(() => combat.Combo.IsCharged, 1f);
        float unpaid = defence.Health.Current;
        var landed = defence.TakeHit(new Hit(20f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        Keys();
        yield return null;
        Say($"arm skill with no mana: the blow {landed}, health {unpaid:0} -> {defence.Health.Current:0} (wanted Landed)");
        keepWell = true;
        yield return new WaitForSeconds(0.8f);

        yield return Finish(weapon, 2.4f);
        yield return Element(weapon, 1.9f, true);
        Say($"arm: worn in {element.Info.Name}, shown as '{combat.Weapon.ShownName}'");
    }

    private IEnumerator Spear()
    {
        weaponTag = "spear";
        var weapon = Weapons.ByName(Weapons.GreatSpearName);
        combat.Equip(weapon);
        var combo = weapon.Combo;
        var shots = player.GetComponent<WeaponShots>();
        yield return Common(weapon, 3.3f);

        // The haft: from too close the thrust does little.
        yield return Stage(1.5f);
        float health = dummy.Health.Current;
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        Say($"spear haft: standing 1.5 m off (nearest side {combat.LastBlowDistance:0.00} m): damage {health - dummy.Health.Current:0.#} (plain 10; x0.6 under 1.4 m)");

        // From behind the guard: the thrust is made, the guard stays up, and a blow that arrives during it is blocked.
        yield return Stage(3.2f);
        Keys(Key.L);
        yield return Until(() => defence.IsGuarding, 1f);
        yield return new WaitForSeconds(0.4f);
        health = dummy.Health.Current;
        Keys(Key.L, Key.J);
        yield return null;
        yield return null;
        Keys(Key.L);
        yield return Until(() => combat.Combo.Current == combo.GuardAttack, 1f);
        bool started = combat.Combo.Current == combo.GuardAttack, upAtStart = defence.IsGuarding;
        yield return new WaitForSeconds(0.1f);
        yield return Shot("guardthrust", 1);
        keepWell = false;
        float mine = defence.Health.Current;
        int blocks = defence.Blocks;
        var outcome = defence.TakeHit(new Hit(10f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup, 1f);
        yield return null;
        yield return Shot("guardthrust", 2);
        bool wentOn = combat.Combo.Current == combo.GuardAttack;
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"spear guard thrust ({combo.GuardAttack.Name}): started {started}, guard up at its start {upAtStart} and after {defence.IsGuarding}; a blow of 10 inside it was {outcome} (blocks +{defence.Blocks - blocks}, health {mine:0} -> {defence.Health.Current:0}), thrust went on {wentOn}; dummy lost {health - dummy.Health.Current:0.#} (design {combo.GuardAttack.Damage})");
        keepWell = true;
        Keys();
        yield return new WaitForSeconds(0.3f);

        yield return Stage(3.3f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        int shoves = dummy.Shoves;
        yield return Tap(Key.K);
        yield return Watch("vault", combo.ChainHeavy, $"damage {combo.ChainHeavy.Damage} stagger {combo.ChainHeavy.Stagger} lunge {combo.ChainHeavy.Lunge} shove {-combo.ChainHeavy.Pull}");
        Say($"spear vault: the dummy was shoved +{dummy.Shoves - shoves} times, last by {dummy.LastShove.magnitude:0.0} m");

        yield return Stage(6.5f);
        shoves = dummy.Shoves;
        yield return Hold(combo.ChargeTime + 0.15f);
        yield return Watch("charged", combo.ChargedHeavy, $"damage {combo.ChargedHeavy.Damage} stagger {combo.ChargedHeavy.Stagger} travel {combo.ChargedHeavy.Travel} shove {-combo.ChargedHeavy.Pull}, released from tier 1 at {combo.ChargeTime} s");
        Say($"spear charged: the dummy was shoved +{dummy.Shoves - shoves} times");

        // The Flying Spear into the dummy, 8 m off: it sticks, the bearer runs to it and arrives with the turning cut.
        yield return Stage(8f);
        yield return FlyingSpear(combo, shots, "skill", "into the dummy 8 m off (design: 26, then the recall's 18)", false);

        // Into the ground: thrown where no one stands, it flies its 11 m and the bearer follows all the way.
        yield return Stage(3f);
        player.Teleport(dummy.transform.position - Vector3.forward * 6f, Quaternion.LookRotation(-Vector3.forward, Vector3.up));
        yield return new WaitForSecondsRealtime(0.15f);
        yield return FlyingSpear(combo, shots, "skillground", "at empty ground (design: an 11 m flight, then the run)", false);

        // Refused: the stick held away from the throw; the spear comes back to the hand and nothing can be swung meanwhile.
        yield return Stage(8f);
        yield return FlyingSpear(combo, shots, "skillrefused", "with the stick held away (design: no run; the spear back in 0.5 s)", true);

        yield return Finish(weapon, 3f);
        yield return Element(weapon, 3.3f, false);
    }

    private IEnumerator FlyingSpear(WeaponCombo combo, WeaponShots shots, string move, string design, bool refuse)
    {
        int thrown = shots.Thrown, struck = shots.Struck, sticking = shots.Sticking, dashes = combat.FollowDashes, refused = combat.FollowsRefused, catches = combat.Catches, skills = combat.SkillsUsed, blows = combat.BlowsLanded;
        float health = dummy.Health.Current, mana = combat.Mana.Current;
        Vector3 from = player.transform.position;
        yield return Hold(combo.SkillChargeTime + 0.15f);
        yield return Until(() => combat.Combo.Current == combo.Skill, 1f);
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup || combat.Combo.Elapsed >= combo.Skill.Windup * 0.8f, 1f);
        yield return Shot(move, 1);
        yield return Until(() => combat.WeaponAway, 1f);
        float left = Time.time;
        if (refuse) Keys(Key.S);
        yield return null;
        yield return null;
        yield return Shot(move, 2);
        // With the spear out of the hand a light press starts nothing.
        bool idleHands = true;
        if (refuse)
        {
            yield return Until(() => !combat.IsAttacking, 1f);
            Keys(Key.S, Key.J);
            yield return null;
            yield return null;
            Keys(Key.S);
            yield return new WaitForSeconds(0.1f);
            idleHands = !combat.IsAttacking;
            yield return Shot(move, 3);
        }
        else
        {
            yield return Until(() => combat.IsDashing, 1.5f);
            float dashAt = Time.time - left;
            yield return null;
            yield return null;
            yield return Shot(move, 3);
            yield return Until(() => combat.Combo.Current == combo.Skill.FollowStep, 1.5f);
            float arrived = Time.time - left;
            yield return Until(() => combat.Combo.Current != combo.Skill.FollowStep || combat.Combo.Phase != AttackPhase.Windup, 1f);
            yield return null;
            yield return Shot(move, 4);
            yield return new WaitForSecondsRealtime(0.12f);
            yield return Shot(move, 5);
            Say($"spear {move} timing: the run began {dashAt:0.00} s after the throw and the recall began at {arrived:0.00} s (flight + {combo.Skill.FollowDelay} + {combo.Skill.FollowSeconds})");
        }
        yield return Until(() => !combat.WeaponAway, 2.5f);
        float home = Time.time - left;
        Keys();
        yield return Until(() => !combat.IsAttacking, 2f);
        Say($"spear {move} (Flying Spear) {design}: skills +{combat.SkillsUsed - skills}, mana {mana:0} -> {combat.Mana.Current:0} (cost {combo.SkillManaCost}), thrown +{shots.Thrown - thrown}, struck +{shots.Struck - struck}, stuck +{shots.Sticking - sticking}, runs +{combat.FollowDashes - dashes}, refused +{combat.FollowsRefused - refused}, came back by itself +{combat.Catches - catches}, weapon blows +{combat.BlowsLanded - blows}, dummy lost {health - dummy.Health.Current:0.#}, the bearer moved {Vector3.Distance(from, player.transform.position):0.0} m, in hand again after {home:0.00} s, away now {combat.WeaponAway}{(refuse ? $", a light press while it was away started nothing: {idleHands}" : "")}");
        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator Hammer()
    {
        weaponTag = "hammer";
        var weapon = Weapons.ByName(Weapons.GreatHammerName);
        combat.Equip(weapon);
        var combo = weapon.Combo;
        var shots = player.GetComponent<WeaponShots>();
        yield return Common(weapon, 2.8f);

        yield return Stage(2.8f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        int shoves = dummy.Shoves;
        yield return Tap(Key.K);
        yield return Watch("lift", combo.ChainHeavy, $"damage {combo.ChainHeavy.Damage} stagger {combo.ChainHeavy.Stagger} shove {-combo.ChainHeavy.Pull}");
        Say($"hammer lift: the dummy was shoved +{dummy.Shoves - shoves} times, last by {dummy.LastShove.magnitude:0.0} m");

        yield return Stage(3f);
        yield return Hold(combo.ChargeTime + 0.15f);
        yield return Watch("charged", combo.ChargedHeavy, $"damage {combo.ChargedHeavy.Damage} stagger {combo.ChargedHeavy.Stagger} disc {combo.ChargedHeavy.Radius}, released from tier 1 at {combo.ChargeTime} s");

        // The toss with the dummy inside the spin: two blows all round, then out through it and back through it.
        yield return Stage(2.6f);
        int thrown = shots.Thrown, struck = shots.Struck, returned = shots.Returned, catches = combat.Catches, skills = combat.SkillsUsed, blows = combat.BlowsLanded;
        float health = dummy.Health.Current, mana = combat.Mana.Current;
        dummy.Stagger.Reset();
        yield return Hold(combo.SkillChargeTime + 0.15f);
        yield return Until(() => combat.Combo.Current == combo.Skill, 1f);
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup, 1f);
        yield return null;
        yield return Shot("skill", 1);
        yield return Until(() => combat.WeaponAway, 1f);
        float left = Time.time;
        yield return null;
        yield return null;
        yield return Shot("skill", 2);
        yield return new WaitForSeconds(0.3f);
        yield return Shot("skill", 3);
        Keys(Key.J);
        yield return null;
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.2f);
        bool chained = combat.Combo.Current != null && combat.Combo.Current != combo.Skill;
        yield return Shot("skill", 4);
        yield return Until(() => !combat.WeaponAway, 3f);
        float away = Time.time - left;
        yield return null;
        yield return null;
        yield return Shot("skill", 5);
        Say($"hammer skill (Spinning Hammer Toss): skills +{combat.SkillsUsed - skills}, mana {mana:0} -> {combat.Mana.Current:0} (cost {combo.SkillManaCost}), spin blows +{combat.BlowsLanded - blows} (design 2 of 14), thrown +{shots.Thrown - thrown}, struck out and back +{shots.Struck - struck} (design 28 then 14), came home +{shots.Returned - returned}, caught +{combat.Catches - catches}, out of the hand {away:0.00} s (design 0.9), a light press meanwhile started an attack: {chained} (wanted False), dummy lost {health - dummy.Health.Current:0.#} (design 14 + 14 + 28 + 14 = 70)");
        yield return new WaitForSeconds(0.6f);

        // The guard that is slow to rise: struck at once it is only a block; struck a tenth of a second later it is perfect.
        yield return Stage(2.8f);
        keepWell = false;
        Keys(Key.L);
        yield return null;
        yield return null;
        var early = defence.TakeHit(new Hit(10f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        Keys();
        yield return new WaitForSeconds(0.7f);
        Keys(Key.L);
        yield return new WaitForSeconds(0.13f);
        var timed = defence.TakeHit(new Hit(10f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        yield return new WaitForSecondsRealtime(0.1f);
        yield return Shot("guard", 1);
        Keys();
        keepWell = true;
        Say($"hammer guard: a blow two frames after the press was {early} (wanted Blocked); one 0.13 s after it was {timed} (wanted PerfectGuard; the window is {defence.Guard.PerfectFrom} to {defence.Guard.PerfectWindow} s)");
        yield return new WaitForSeconds(0.5f);

        yield return Finish(weapon, 3f);
        yield return Element(weapon, 2.8f, false);
    }

    // One fight with each weapon against the nearest soldier: the light chain, a heavy, the charged heavy or skill, and a finisher if it staggers.
    private IEnumerator RealEnemies(AreaDirector director)
    {
        foreach (string name in new[] { Weapons.GreatSpearName, Weapons.GreatHammerName, Weapons.MassiveSwordName, Weapons.ScytheName, Weapons.ArmamentName })
        {
            Enemy enemy = null;
            foreach (var candidate in director.Enemies)
                if (candidate != null && candidate.gameObject.activeInHierarchy && !candidate.IsDead && candidate.Kind.Name == EnemyKind.SoldierName) { enemy = candidate; break; }
            // With the soldiers used up, whoever else is still standing will do.
            if (enemy == null)
                foreach (var candidate in director.Enemies)
                    if (candidate != null && candidate.gameObject.activeInHierarchy && !candidate.IsDead) { enemy = candidate; break; }
            if (enemy == null) { Say($"{name}: no enemy left"); continue; }
            string kind = enemy.Kind.Name;
            combat.Equip(Weapons.ByName(name));
            weaponTag = "castle_" + combat.Weapon.Prefix;
            var combo = combat.Weapon.Combo;
            float stand = name == Weapons.ArmamentName ? 1.8f : 3f;
            player.Teleport(enemy.transform.position + enemy.transform.forward * stand, Quaternion.LookRotation(-enemy.transform.forward, Vector3.up));
            combat.Mana.Gain(100f);
            yield return new WaitForSeconds(0.3f);
            int blows = combat.BlowsLanded, taken = defence.BlowsTaken, finishers = combat.Finishers;
            float end = Time.time + 9f;
            int presses = 0;
            while (Time.time < end && !enemy.IsDead)
            {
                if (enemy.IsStaggered || presses % 5 == 4) yield return Tap(Key.K);
                else yield return Tap(Key.J);
                presses++;
                if (presses == 3) yield return Shot("fight", 1);
                yield return new WaitForSeconds(0.28f);
            }
            yield return Shot("fight", 2);
            Say($"{name} against a {kind}: blows landed +{combat.BlowsLanded - blows}, finishers +{combat.Finishers - finishers}, dead {enemy.IsDead}, blows taken +{defence.BlowsTaken - taken}, poised through {defence.PoisedBlows}, tip blows {combat.SweetBlows}");
            yield return new WaitForSeconds(1.5f);
        }
    }
}
