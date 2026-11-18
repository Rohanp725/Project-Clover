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
        if (only == null || only.Contains(" knife ")) yield return Knives();
        if (only == null || only.Contains(" bow ")) yield return Bow();
        if (only == null || only.Contains(" sickle ")) yield return Sickle();

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
        yield return Tap(Key.Q);
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

    // Where the player stands by the dummy: how far, whether beyond it (north of it), and whether turned to it.
    private string Place()
    {
        Vector3 gap = dummy.transform.position - player.transform.position;
        gap.y = 0f;
        float facing = gap.sqrMagnitude > 0.01f ? Vector3.Dot(player.transform.forward, gap.normalized) : 0f;
        float behind = gap.sqrMagnitude > 0.01f ? Vector3.Dot(dummy.transform.forward, -gap.normalized) : 0f;
        return $"{gap.magnitude:0.00} m from the dummy, {(gap.z < 0f ? "beyond" : "short of")} it, facing it {facing:0.00}, on its {(behind < combat.BackDot ? "BACK" : "front or side")} ({behind:0.00})";
    }

    private IEnumerator Knives()
    {
        weaponTag = "knife";
        var weapon = Weapons.ByName(Weapons.TwinKnivesName);
        combat.Equip(weapon);
        var combo = weapon.Combo;
        var shots = player.GetComponent<WeaponShots>();
        int backs = combat.BackBlows;
        // "pass" beside the prefix in WeaponsProbe.only begins at the passing cut, for looking at that alone.
        bool passOnly = System.IO.File.Exists("AgentScripts/WeaponsProbe.only") && System.IO.File.ReadAllText("AgentScripts/WeaponsProbe.only").Contains("pass");
        if (!passOnly)
        {
            yield return Common(weapon, 1.5f);
            Say($"knife after the chain and a heavy: back blows so far +{combat.BackBlows - backs}; {Place()}");
        }

        // The fifth cut alone: where it leaves the player.
        yield return Stage(1.5f);
        for (int i = 0; i < 5; i++)
        {
            yield return Tap(Key.J);
            var wanted = combo.Steps[i];
            if (i == 4)
            {
                // Frame by frame through the passing cut: the phase, whether the body is being carried, and where it is.
                var trace = new System.Text.StringBuilder();
                for (int frame = 0; frame < 26; frame++)
                {
                    trace.Append($"{(combat.Combo.Current != null ? combat.Combo.Current.Name.Replace("attack_knife_", "") : "-")}{combat.Combo.Phase.ToString()[0]}{(combat.PassingThrough ? "*" : "")} {player.transform.position.z:0.00} | ");
                    yield return null;
                }
                Say("knife passing cut, by frame (step, phase, * carried, z; the dummy is at 4): " + trace);
            }
            yield return Until(() => combat.Combo.Current == wanted && combat.Combo.Phase == AttackPhase.Recovery, 2f);
            if (i >= 3) Say($"knife after cut {i + 1} of a chain on one dummy (doing {(combat.Combo.Current != null ? combat.Combo.Current.Name : "nothing")}, dummy staggered {dummy.IsStaggered}): {Place()}");
        }
        yield return new WaitForSeconds(0.1f);
        Say($"knife after the fifth cut (design: through, 2.4 m, turned to its back): {Place()}");
        yield return Shot("pass", 1);

        // One cut on the front (the usual place to stand is the dummy's back: it faces north): the plain number.
        yield return Stage(1.5f);
        player.Teleport(dummy.transform.position + Vector3.forward * 1.5f, Quaternion.LookRotation(-Vector3.forward, Vector3.up));
        yield return new WaitForSecondsRealtime(0.15f);
        backs = combat.BackBlows;
        float health = dummy.Health.Current;
        Say($"knife staged in front: {Place()}");
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        Say($"knife first cut on the front: damage {health - dummy.Health.Current:0.##} (design 5), back blows +{combat.BackBlows - backs} (wanted 0)");

        // The crossing step after one light.
        yield return Stage(1.5f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        yield return Tap(Key.K);
        yield return Watch("chain", combo.ChainHeavy, $"damage {combo.ChainHeavy.Damage} stagger {combo.ChainHeavy.Stagger} travel {combo.ChainHeavy.Travel} through");
        yield return new WaitForSeconds(0.1f);
        Say($"knife after the crossing step: {Place()}");

        // The cut out of the sidestep: the dodge toward the dummy from range, the light pressed in its second half.
        yield return Stage(4.6f);
        int dashes = combat.DashAttacks;
        Keys(Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => player.Dodge.IsRolling && player.Dodge.Progress >= 0.6f, 1f);
        yield return Shot("roll", 1);
        Keys(Key.J);
        yield return null;
        yield return null;
        Keys();
        yield return Watch("dash", combo.DashAttack, $"damage {combo.DashAttack.Damage} stagger {combo.DashAttack.Stagger}");
        Say($"knife dash attack: started +{combat.DashAttacks - dashes} (wanted 1)");

        // Fan of Knives from a step away, then from range.
        foreach (float stand in new[] { 1.5f, 6f })
        {
            yield return Stage(stand);
            int thrown = shots.Thrown, struck = shots.Struck, skills = combat.SkillsUsed;
            health = dummy.Health.Current;
            float mana = combat.Mana.Current;
            Vector3 from = player.transform.position;
            dummy.Stagger.Reset();
            yield return Hold(combo.ChargeTime + 0.12f);
            yield return Until(() => combat.Combo.Current == combo.Skill, 1f);
            if (stand < 2f) yield return Shot("skill", 1);
            yield return Until(() => combat.Combo.Phase != AttackPhase.Windup, 1f);
            yield return null;
            yield return null;
            if (stand < 2f) yield return Shot("skill", 2);
            yield return new WaitForSeconds(0.12f);
            if (stand < 2f) yield return Shot("skill", 3);
            yield return new WaitForSeconds(0.6f);
            if (stand < 2f) yield return Shot("skill", 4);
            Say($"knife skill (Fan of Knives) from {stand} m: skills +{combat.SkillsUsed - skills}, mana {mana:0} -> {combat.Mana.Current:0} (cost {combo.SkillManaCost}), thrown +{shots.Thrown - thrown} (design 5), struck +{shots.Struck - struck}, dummy lost {health - dummy.Health.Current:0.#} (design 45 with all five), sprang back {Vector3.Dot(from - player.transform.position, Vector3.forward):0.00} m (design 2.6)");
        }

        // The guard: struck two frames after the press it is perfect, and a blocked blow costs dear.
        yield return Stage(1.5f);
        keepWell = false;
        Keys(Key.L);
        yield return null;
        yield return null;
        var timed = defence.TakeHit(new Hit(10f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        yield return new WaitForSeconds(0.4f);
        float before = player.Stamina.Current, life = defence.Health.Current;
        var late = defence.TakeHit(new Hit(10f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        yield return Shot("guard", 1);
        Say($"knife guard: a blow two frames after the press was {timed} (wanted PerfectGuard, window {defence.Guard.PerfectWindow}); one 0.4 s later was {late}, cost {before - player.Stamina.Current:0.#} stamina (design 12.5) and {life - defence.Health.Current:0.#} health (design 4.5)");
        Keys();
        keepWell = true;
        yield return new WaitForSeconds(0.4f);

        yield return Finish(weapon, 1.6f);
        Say($"knife after Three Crossings: {Place()}");
        yield return Element(weapon, 1.5f, false);
    }

    // The Great Bow: the chain and what it leaves standing, three shots and the swat that cashes them, the fourth arrow,
    // an arrow in the ground and one set off, the kick shot, the full draw, Landmines, the guard's arrow, and Nailed.
    private IEnumerator Bow()
    {
        weaponTag = "bow";
        var weapon = Weapons.ByName(Weapons.GreatBowName);
        combat.Equip(weapon);
        var combo = weapon.Combo;
        var shots = player.GetComponent<WeaponShots>();
        var arrows = combat.Arrows;
        yield return Common(weapon, 1.8f);
        yield return new WaitForSeconds(0.3f);
        Say($"bow after swat, stab and a snap shot: arrows standing in the dummy {arrows.CountIn(dummy)} (design 2: the stab's and the shot's), planted {arrows.Planted}, snapped {arrows.Snapped}");

        // Three shots from range, a fourth, then the swat.
        yield return Stage(6f);
        yield return new WaitForSeconds(1.2f);
        int thrown = shots.Thrown;
        combat.Mana.TrySpend(50f);
        float mana = combat.Mana.Current, health0 = dummy.Health.Current;
        float began = Time.time;
        for (int i = 0; i < 4; i++)
        {
            int struckBefore = shots.Struck;
            began = Time.time;
            yield return Tap(Key.K);
            yield return Until(() => combat.Combo.Current == combo.Heavy, 1f);
            if (i == 0)
            {
                yield return Until(() => combat.Combo.StepProgress * combo.Heavy.Total >= combo.Heavy.Windup * 0.8f, 1f);
                yield return Shot("draw", 1);
            }
            yield return Until(() => shots.Struck > struckBefore, 2f);
            if (i == 0) Say($"bow snap shot from 6 m: struck {Time.time - began:0.00} s after the press (wind-up 0.32 and the flight), dummy lost {health0 - dummy.Health.Current:0.#} (design 18), mana {mana:0} -> {combat.Mana.Current:0} (design +8)");
            yield return Until(() => combat.Combo.Current != combo.Heavy || combat.Combo.Phase == AttackPhase.None, 2f);
            if (i == 0) Say($"bow snap shot: the whole step {Time.time - began:0.00} s (design 0.78)");
            if (i == 2) Say($"bow three shots: thrown +{shots.Thrown - thrown}, arrows in the dummy {arrows.CountIn(dummy)} (design 3)");
        }
        Say($"bow a fourth shot: arrows in the dummy {arrows.CountIn(dummy)} (design 3: the oldest makes way)");
        yield return Shot("arrows", 1);
        yield return Stage(1.8f);
        int snapped = arrows.Snapped;
        float health = dummy.Health.Current;
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase != AttackPhase.Windup, 1f);
        yield return Shot("snap", 1);
        yield return new WaitForSeconds(0.1f);
        yield return Shot("snap", 2);
        yield return new WaitForSeconds(0.4f);
        Say($"bow swat on three arrows: snapped +{arrows.Snapped - snapped}, dummy lost {health - dummy.Health.Current:0.#} (design 39), stagger {(dummy.IsStaggered ? "BROKEN" : dummy.Stagger.Value.ToString("0.#"))} (design 56), arrows left {arrows.CountIn(dummy)}");

        // A shot at nothing stands in the ground; an arrow in the ground beside an enemy goes off.
        yield return Stage(6f);
        player.Teleport(dummy.transform.position + Vector3.forward * 1.5f, Quaternion.LookRotation(Vector3.forward, Vector3.up));
        yield return new WaitForSecondsRealtime(0.15f);
        int grounded = arrows.Grounded, tripped = arrows.Tripped, struckThen = shots.Struck;
        yield return Tap(Key.K);
        yield return new WaitForSeconds(1.2f);
        Say($"bow a shot at nothing (north, the dummy behind): struck +{shots.Struck - struckThen}, arrows in the ground +{arrows.Grounded - grounded}, standing {arrows.Standing} (design 1, for 8 s)");
        yield return Shot("ground", 1);
        health = dummy.Health.Current;
        arrows.PlantGround(dummy.transform.position + Vector3.right * 0.9f, Vector3.forward);
        yield return new WaitForSeconds(0.6f);
        Say($"bow an arrow in the ground 0.9 m from the dummy: set off +{arrows.Tripped - tripped} (design 1), dummy lost {health - dummy.Health.Current:0.#} (design 10)");

        // The kick shot after one light.
        yield return Stage(1.7f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        thrown = shots.Thrown;
        health = dummy.Health.Current;
        Vector3 from = player.transform.position;
        yield return Tap(Key.K);
        yield return Watch("chain", combo.ChainHeavy, $"kick {combo.ChainHeavy.Damage} then the shot {combo.ChainHeavy.Shot.Damage}, recoil {combo.ChainHeavy.Recoil}");
        yield return new WaitForSeconds(0.12f);
        yield return Shot("chain", 4);
        yield return new WaitForSeconds(0.5f);
        Say($"bow kick shot: thrown +{shots.Thrown - thrown}, dummy lost {health - dummy.Health.Current:0.#} (design 26 and what the kick snapped), sprang back {Vector3.Dot(from - player.transform.position, Vector3.forward):0.00} m (design 3.0 less the step in), arrows in the dummy {arrows.CountIn(dummy)}");

        // The full draw from range.
        yield return Stage(7f);
        dummy.Afflictions.Clear();
        int struckFull = shots.Struck;
        health = dummy.Health.Current;
        thrown = shots.Thrown;
        int groundedBefore = arrows.Grounded;
        from = player.transform.position;
        yield return Hold(combo.ChargeTime + 0.15f);
        yield return Watch("charged", combo.ChargedHeavy, $"shot {combo.ChargedHeavy.Shot.Damage} through everything, recoil {combo.ChargedHeavy.Recoil}");
        yield return new WaitForSeconds(0.5f);
        Say($"bow full draw from 7 m (the player now at z {player.transform.position.z:0.00}): struck +{shots.Struck - struckFull}, thrown +{shots.Thrown - thrown}, dummy lost {health - dummy.Health.Current:0.#} (design 34), rocked back {Vector3.Dot(from - player.transform.position, Vector3.forward):0.00} m (design 0.8), arrows in the dummy {arrows.CountIn(dummy)}, in the ground beyond +{arrows.Grounded - groundedBefore}");

        // Landmines round the dummy from five metres.
        yield return Stage(5f);
        int rained = arrows.Rained, skills = combat.SkillsUsed;
        grounded = arrows.Grounded;
        health = dummy.Health.Current;
        mana = combat.Mana.Current;
        yield return Hold(combo.SkillChargeTime + 0.2f);
        yield return Until(() => combat.Combo.Current == combo.Skill, 1f);
        yield return Until(() => combat.Combo.StepProgress * combo.Skill.Total >= combo.Skill.Windup * 0.8f, 1f);
        yield return Shot("skill", 1);
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup, 1f);
        yield return Shot("skill", 2);
        yield return new WaitForSeconds(0.75f);
        yield return Shot("skill", 3);
        yield return new WaitForSeconds(0.5f);
        Say($"bow skill (Landmines): skills +{combat.SkillsUsed - skills}, mana {mana:0} -> {combat.Mana.Current:0} (cost {combo.SkillManaCost}), arrows dropped +{arrows.Rained - rained} (design 5), standing in the ground {arrows.Standing} (+{arrows.Grounded - grounded}), dummy in the middle lost {health - dummy.Health.Current:0.#} (design 0: the ring is round it)");

        // The guard: a perfect guard leaves the arrow from the fist in the attacker.
        yield return Stage(1.6f);
        int riders = combat.GuardRiders, inDummy = arrows.CountIn(dummy);
        Keys(Key.L);
        yield return null;
        yield return null;
        var timed = defence.TakeHit(new Hit(10f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        yield return new WaitForSeconds(0.25f);
        yield return Shot("guard", 1);
        Say($"bow guard: a blow two frames after the press was {timed}; riders +{combat.GuardRiders - riders}, arrows in the attacker {inDummy} -> {arrows.CountIn(dummy)} (design +1)");
        Keys();
        yield return new WaitForSeconds(0.4f);

        // Nailed, on a dummy carrying arrows: they go off after the blow.
        yield return Stage(6f);
        for (int i = 0; i < 2; i++)
        {
            yield return Tap(Key.K);
            yield return new WaitForSeconds(0.9f);
        }
        snapped = arrows.Snapped;
        int carried = arrows.CountIn(dummy);
        yield return Finish(weapon, 1.6f);
        Say($"bow after Nailed: the dummy carried {carried} arrows, snapped +{arrows.Snapped - snapped}");
        yield return Element(weapon, 1.8f, false);
    }

    private string Off() => $"{Vector3.Distance(player.transform.position, dummy.transform.position):0.00} m from the dummy";

    // The Chain Sickle: the chain of four (near, far, near, far), the weight from range, the yank on something too
    // heavy to come (the player goes) and on something light (it comes), Entangle and the heavy button while it
    // holds, the bind broken by a roll, the guard's yank, and Unwinding.
    private IEnumerator Sickle()
    {
        weaponTag = "sickle";
        var weapon = Weapons.ByName(Weapons.ChainSickleName);
        combat.Equip(weapon);
        var combo = weapon.Combo;
        var chain = player.GetComponent<ChainLine>();
        int shoves = dummy.Shoves;
        yield return Common(weapon, 1.8f);
        Say($"sickle after the chain and the thrown sickle: the chain's end went {chain.Furthest:0.00} m from the hand at its furthest (design 4.6), the hook's pull shoved the dummy +{dummy.Shoves - shoves} (design 1, 0.6 m)");

        // The weight alone from beyond any blade.
        yield return Stage(4.6f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        float health = dummy.Health.Current;
        chain.Furthest = 0f;
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[1] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        yield return Shot("far", 1);
        Say($"sickle weight snap with the dummy {Off()}: dummy lost {health - dummy.Health.Current:0.#} (design 11), chain out {chain.Furthest:0.00} m");

        // The yank on the post (too heavy to come): the player goes to it.
        yield return Stage(4.6f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        int pulls = combat.SelfPulls;
        shoves = dummy.Shoves;
        string before = Off();
        yield return Tap(Key.K);
        yield return Watch("chain", combo.ChainHeavy, $"damage {combo.ChainHeavy.Damage} stagger {combo.ChainHeavy.Stagger}, pull {combo.ChainHeavy.Pull} or the player goes");
        yield return new WaitForSeconds(0.2f);
        yield return Shot("chain", 4);
        yield return new WaitForSeconds(0.5f);
        Say($"sickle yank on something too heavy (weight {dummy.Weight}): pulled self +{combat.SelfPulls - pulls} (design 1), dummy shoved +{dummy.Shoves - shoves} (design 0); the player was {before}, is now {Off()}");

        // The same on something light: it comes.
        dummy.Weight = 1f;
        yield return Stage(4.6f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Current == combo.Steps[0] && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        pulls = combat.SelfPulls;
        shoves = dummy.Shoves;
        yield return Tap(Key.K);
        yield return Until(() => combat.Combo.Current == combo.ChainHeavy && combat.Combo.Phase == AttackPhase.Recovery, 2f);
        yield return new WaitForSeconds(0.4f);
        Say($"sickle yank on something light (weight 1): pulled self +{combat.SelfPulls - pulls} (design 0), dummy shoved +{dummy.Shoves - shoves} by {dummy.LastShove.magnitude:0.00} m toward the player {Vector3.Dot(dummy.LastShove.normalized, -Vector3.forward):0.00} (design 3.0, never past the feet)");
        dummy.Weight = 10f;

        // Entangle from six metres, and the heavy button while it holds.
        yield return Stage(6f);
        int binds = combat.Bindings, skills = combat.SkillsUsed;
        health = dummy.Health.Current;
        float mana = combat.Mana.Current;
        chain.Furthest = 0f;
        yield return Hold(combo.ChargeTime + 0.12f);
        yield return Until(() => combat.Combo.Current == combo.Skill, 1f);
        yield return Shot("skill", 1);
        yield return Until(() => combat.Combo.Phase != AttackPhase.Windup, 1f);
        yield return null;
        yield return Shot("skill", 2);
        yield return Until(() => combat.Combo.Current != combo.Skill || combat.Combo.Phase == AttackPhase.Recovery, 1f);
        yield return new WaitForSeconds(0.25f);
        yield return Shot("skill", 3);
        Say($"sickle skill (Entangle) from 6 m: skills +{combat.SkillsUsed - skills}, mana {mana:0} -> {combat.Mana.Current:0} (cost {combo.SkillManaCost}), dummy lost {health - dummy.Health.Current:0.#} (design 12), binds +{combat.Bindings - binds}, bound to the dummy {combat.BoundTarget == dummy} for {combat.BoundLeft:0.00} s more (design 2.5 from the blow), dummy held {dummy.Afflictions.IsHeld}, chain out {chain.Furthest:0.00} m");
        yield return Until(() => !combat.IsAttacking, 1f);
        pulls = combat.SelfPulls;
        dummy.Stagger.Reset();
        yield return Tap(Key.K);
        yield return Until(() => combat.Combo.Current != null, 0.5f);
        bool hauled = combat.Combo.Current == combo.ChainHeavy;
        yield return Until(() => combat.Combo.Current == combo.ChainHeavy && combat.Combo.Phase == AttackPhase.Recovery, 1f);
        float boundStagger = dummy.Stagger.Value;
        yield return new WaitForSeconds(0.6f);
        Say($"sickle heavy button with the dummy bound, from a standing start: the yank {hauled} (design true), stagger {boundStagger:0.#} (design 30 x 1.25 = 37.5), pulled self +{combat.SelfPulls - pulls}, now {Off()}");
        yield return new WaitForSeconds(2.2f);
        Say($"sickle 2.5 s after the bind: still bound {combat.BoundTarget != null} (design false)");

        // A roll lets go of it.
        yield return Stage(6f);
        yield return Hold(combo.ChargeTime + 0.12f);
        yield return Until(() => combat.BoundTarget != null, 1.5f);
        bool was = combat.BoundTarget != null;
        yield return Until(() => !combat.IsAttacking, 1.5f);
        Keys(Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return new WaitForSeconds(0.3f);
        Say($"sickle a roll while bound: bound before {was}, after {combat.BoundTarget != null} (design false)");
        yield return new WaitForSeconds(0.5f);

        // The guard: a perfect guard yanks the attacker in and shakes it.
        yield return Stage(1.6f);
        int riders = combat.GuardRiders;
        shoves = dummy.Shoves;
        Keys(Key.L);
        yield return null;
        yield return null;
        float staggerBefore = dummy.Stagger.Value;
        var timed = defence.TakeHit(new Hit(10f, 0f, -Vector3.forward, false, false, dummy.transform.position));
        yield return null;
        float rider = dummy.Stagger.Value - staggerBefore;
        yield return new WaitForSeconds(0.2f);
        yield return Shot("guard", 1);
        Say($"sickle guard: a blow two frames after the press was {timed}; riders +{combat.GuardRiders - riders}, the attacker shoved +{dummy.Shoves - shoves} by {dummy.LastShove.magnitude:0.00} m (design 1.2, less when it is already close), stagger +{rider:0.#} (design 15)");
        Keys();
        yield return new WaitForSeconds(0.4f);

        shoves = dummy.Shoves;
        yield return Finish(weapon, 1.6f);
        Vector3 gap = dummy.transform.position - player.transform.position;
        gap.y = 0f;
        Say($"sickle after Unwinding: {Off()} (design about 2.6), back to it {Vector3.Dot(player.transform.forward, gap.normalized):0.00} (design near -1), heaves on the dummy +{dummy.Shoves - shoves} (design 2)");
        yield return Element(weapon, 1.8f, false);
    }

    // One fight with each weapon against the nearest soldier: the light chain, a heavy, the charged heavy or skill, and a finisher if it staggers.
    private IEnumerator RealEnemies(AreaDirector director)
    {
        foreach (string name in new[] { Weapons.ChainSickleName, Weapons.GreatBowName, Weapons.TwinKnivesName, Weapons.GreatSpearName, Weapons.GreatHammerName, Weapons.MassiveSwordName, Weapons.ScytheName, Weapons.ArmamentName })
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
            float stand = name == Weapons.TwinKnivesName ? 1.5f : name == Weapons.ArmamentName ? 1.8f : 3f;
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
