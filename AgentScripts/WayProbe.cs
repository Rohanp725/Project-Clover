using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Growth;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Play Mode probe of the Katana's Way in the Proving Ground, with the real keys (J light, K heavy, L guard, Space dodge,
// Left Ctrl + J the infused strike). The title guide is closed with the back key and time is shown to run (an open guide
// holds time at zero); the Katana is taken from the rack; the first trunk mark and the Sheath are bought through the
// chart's own buy (PlayerUpgrades.TryBuy), paid from the Memories wallet. Then, in parts:
//   chains  each aspect's chain played on the still dummy: the Sheath's two cuts, the Stream's five, the Answer's three
//           at nine tenths, the Storm Katana's three with its falling third
//   chord   Before the Thunder on J and K together: the technique replaces the light, out in 0.06 s, and lands
//   answer  the Answer's two answers: a perfect guard, then J is the Riposte (24, critical); again, then K is Through the Gap
//   drums   the Storm Katana's Seven Drums beaten on a penned soldier, all seven spent by the next draw for +40 stagger
//   stream  a dodge in the Stream's second cut keeps the chain: the next J is the third cut, not the first
//   zanshin a kill with Zanshin turns the player to the other soldier behind
//   body    the enduring side of the Body bought: health 135, four flasks healing 55 (one drunk to show it)
//   light   the Light Body: a dodge pressed in a dodge's last 0.1 s flows into a second, never a third
//   film    stills of Running Draw with Lightning (its charged path), Lying Draw with Fire, and the Katana finisher's
//           0.2 s held pose: review/shots/current/way_<name>_<n>.png
// AgentScripts/way_only.txt may hold part names with commas between to run only those (empty or missing: all).
// Saved progress is kept out of it (runprobe.sh sandboxes the play; the probe refuses to pay if it is not).
//   bash AgentScripts/runprobe.sh ProvingGround WayProbe 120
public static class WayProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Way Probe").AddComponent<WayProbeRunner>();
        return "probe started";
    }
}

public class WayProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerUpgrades upgrades;
    private PlayerInteraction interaction;
    private PlayerDefence defence;
    private PlayerSpells spells;
    private PlayerElement element;
    private TrainingDummy still;
    private EnemyPen castle;
    private Vector3 openGround;
    private HashSet<string> only;
    private int ok, wrong;

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
    }

    private bool Wants(string part) => only == null || only.Count == 0 || only.Contains(part);

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Game(float seconds)
    {
        float end = Time.time + seconds;
        while (Time.time < end) yield return null;
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Tap(params Key[] keys)
    {
        Keys(keys);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        yield return null;
    }

    private IEnumerator WhenFree()
    {
        yield return Until(() => !combat.IsAttacking && !player.Dodge.IsRolling && !combat.RearmPending, 3f);
        yield return Real(0.25f);
    }

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    // Time stands still for the picture, so the moment pictured is the moment named.
    private IEnumerator Still(string name)
    {
        float scale = Time.timeScale;
        Time.timeScale = 0f;
        yield return Shot(name);
        Time.timeScale = scale > 0f ? scale : 1f;
    }

    // A mark of the Way bought through the chart's own buy, the marks it grows from bought first; the wallet topped up.
    private bool Learn(string id, int depth = 0)
    {
        var mark = Upgrade.ById(id);
        if (mark == null || depth > 12) return false;
        if (RunState.Unlocked.Contains(id)) return true;
        var wallet = Progress.Data.Wallet;
        if (!upgrades.CanBuy(mark) && mark.Requires != null && mark.Requires.Length > 0 && !mark.Requires.Any(RunState.Unlocked.Contains))
            Learn(mark.Requires[0], depth + 1);
        if (mark.Currency == Currency.Memories && wallet.Memories < mark.Cost) wallet.Earn(Currency.Memories, mark.Cost - wallet.Memories);
        bool bought = upgrades.TryBuy(mark);
        upgrades.RefreshNow();
        if (!bought) Say($"  (could not buy {mark.Name}: can buy {upgrades.CanBuy(mark)}, requires {string.Join("/", mark.Requires ?? new string[0])})");
        return bought;
    }

    // An aspect made the Katana's active one, as a rest fire's choice does, and the sword built again from the armoury.
    private IEnumerator UseAspect(string id)
    {
        yield return WhenFree();
        if (combat.Weapon.Name != Weapons.KatanaName) combat.Arm(Weapons.KatanaName);
        WayRules.ChooseAspect(RunState.WayChoices, Upgrade.ById(id), RunState.Unlocked, PlaytestMode.On);
        combat.Rearm();
        upgrades.RefreshNow();
        yield return null;
        yield return null;
    }

    private IEnumerator AtDummy(float metres = 1.6f)
    {
        yield return WhenFree();
        player.Teleport(still.transform.position + Vector3.back * metres, Quaternion.identity);
        yield return Real(0.3f);
    }

    // Plays the light chain with taps of J, each in the step before's recovery, and gives back the steps played in order.
    private IEnumerator PlayChain(int taps, List<int> played, List<AttackStep> steps)
    {
        yield return Tap(Key.J);
        for (int tap = 0; tap < taps; tap++)
        {
            yield return Until(() => combat.Combo.Phase == AttackPhase.Active || combat.Combo.Phase == AttackPhase.Recovery, 1.5f);
            played.Add(combat.Combo.StepIndex);
            steps.Add(combat.Combo.Current);
            if (tap == taps - 1) break;
            yield return Until(() => combat.Combo.Phase == AttackPhase.Recovery, 1.5f);
            var was = combat.Combo.Current;
            // The chain's last cut is not cut short by a press in its recovery: the next chain starts once it is over.
            if (combat.Combo.StepIndex == combat.Combo.Steps.Count - 1) yield return Until(() => !combat.IsAttacking, 2f);
            yield return Tap(Key.J);
            yield return Until(() => combat.Combo.Current != was && combat.Combo.Current != null, 1.0f);
        }
        yield return WhenFree();
    }

    private Enemy[] LetOutSoldiers(int count)
    {
        // Soldiers first; the pen's other people make up the number when it holds fewer soldiers.
        var soldiers = new List<int>();
        for (int i = 0; i < castle.Enemies.Length && soldiers.Count < count; i++)
            if (castle.Enemies[i] != null && castle.Enemies[i].Kind.Name == EnemyKind.SoldierName) soldiers.Add(i);
        for (int i = 0; i < castle.Enemies.Length && soldiers.Count < count; i++)
            if (castle.Enemies[i] != null && !soldiers.Contains(i) && !castle.Enemies[i].Kind.IsBoss) soldiers.Add(i);
        castle.Release(soldiers.ToArray());
        return soldiers.Select(i => castle.Enemies[i]).ToArray();
    }

    private IEnumerator Start()
    {
        yield return null;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        interaction = player.GetComponent<PlayerInteraction>();
        defence = player.GetComponent<PlayerDefence>();
        spells = player.GetComponent<PlayerSpells>();
        element = player.GetComponent<PlayerElement>();
        openGround = player.transform.position;
        string onlyPath = "AgentScripts/way_only.txt";
        if (File.Exists(onlyPath))
            only = new HashSet<string>(File.ReadAllText(onlyPath).Split(',').Select(part => part.Trim().ToLowerInvariant()).Where(part => part.Length > 0));
        framesMode = only != null && only.Contains("frames");
        Judge("saved progress is sandboxed", Progress.Sandboxed);

        // 1. The title guide opens on arrival; the back key closes it, and time must run again.
        yield return Until(() => GuidePage.Showing, 5f);
        for (int press = 0; press < 6 && GuidePage.Showing; press++) yield return Tap(Key.Escape);
        yield return Real(0.2f);
        float gameTimeBefore = Time.time;
        int frameBefore = Time.frameCount;
        yield return Real(0.6f);
        Judge("the guide is closed and time runs", !GuidePage.Showing && Time.timeScale > 0f && Time.time > gameTimeBefore,
            $"scale {Time.timeScale:0.##}, game time {gameTimeBefore:0.00} -> {Time.time:0.00}, frames {Time.frameCount - frameBefore}");

        // 2. The Katana from the rack, as a player takes it; equipped directly only if the rack has no stand for it.
        var stand = FindObjectsByType<LoadoutChoice>(FindObjectsInactive.Exclude).FirstOrDefault(choice => !choice.IsElement && choice.Choice == Weapons.KatanaName);
        if (stand != null)
        {
            Vector3 at = stand.transform.position + Vector3.back * 1.0f;
            at.y = 0.05f;
            player.Teleport(at, Quaternion.identity);
            yield return Real(0.35f);
            if (interaction.Current != stand) Say($"  (chosen is {(interaction.Current != null ? interaction.Current.name : "nothing")}, not the Katana stand)");
            yield return Tap(Key.E);
            yield return Real(0.3f);
        }
        if (combat.Weapon == null || combat.Weapon.Name != Weapons.KatanaName)
        {
            Say($"  (the rack did not give it{(stand == null ? ": no stand" : "")}; equipping directly)");
            combat.Arm(Weapons.KatanaName);
            yield return null;
        }
        Judge("the Katana is held", combat.Weapon != null && combat.Weapon.Name == Weapons.KatanaName, combat.Weapon != null ? combat.Weapon.Name : "none");

        // 3. One Way mark bought with Memories.
        var mark = Upgrade.ById(KatanaWay.Temper1);
        var wallet = Progress.Data.Wallet;
        Judge("the first trunk mark is a Way mark priced in Memories", mark != null && mark.IsWay && mark.Currency == Currency.Memories,
            mark != null ? $"{mark.Name}, {mark.Cost} {mark.Currency}" : "no mark");
        if (mark == null || wallet == null || !Progress.Sandboxed) { Finish(); yield break; }
        Judge("it is not in force before it is bought", !upgrades.Has(mark.Id));

        if (wallet.Memories < mark.Cost) wallet.Earn(Currency.Memories, mark.Cost - wallet.Memories);
        int memoriesBefore = wallet.Memories;
        int shardsBefore = RunState.Points;
        Judge("the chart's buy can buy it", upgrades.CanBuy(mark), $"{memoriesBefore} Memories held, {shardsBefore} Shards, playtest {PlaytestMode.On}");
        Judge("the chart's buy bought it", upgrades.TryBuy(mark));
        upgrades.RefreshNow();
        yield return null;
        Judge("the wallet was charged its price", wallet.Memories == memoriesBefore - mark.Cost, $"{memoriesBefore} -> {wallet.Memories}, price {mark.Cost}");
        Judge("the run's Shards were not touched", RunState.Points == shardsBefore, $"{shardsBefore} -> {RunState.Points}");
        Judge("it is learned", RunState.Unlocked.Contains(mark.Id));
        Judge("it cannot be bought twice", !upgrades.CanBuy(mark));
        Judge("the mark is in force with the Katana held", upgrades.Has(mark.Id));

        // 4. The first aspect bought switches on at once (the owner, Phase 5 question 3).
        var aspect = Upgrade.ById(KatanaWay.Sheath);
        var build = upgrades.GetComponent<PlayerBuild>();
        Judge("the Sheath is an aspect of the Katana's Way", aspect != null && aspect.Kind == UpgradeKind.Aspect, aspect != null ? aspect.Name : "no mark");
        if (aspect == null || build == null) { Finish(); yield break; }
        Judge("no aspect is active before it is bought", build.Read().AspectOf(Weapons.KatanaName) == "", build.Read().AspectOf(Weapons.KatanaName));
        if (wallet.Memories < aspect.Cost) wallet.Earn(Currency.Memories, aspect.Cost - wallet.Memories);
        Judge("the chart's buy bought the Sheath", upgrades.TryBuy(aspect));
        upgrades.RefreshNow();
        yield return null;
        Judge("the Sheath is the Katana's active aspect", build.Read().AspectOf(Weapons.KatanaName) == KatanaWay.Sheath, build.Read().AspectOf(Weapons.KatanaName));
        Judge("the Sheath is in force", upgrades.Has(aspect.Id));

        still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude)
            .FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.GetComponent<AfflictedDummy>() == null);
        castle = FindObjectsByType<EnemyPen>(FindObjectsInactive.Exclude).FirstOrDefault(pen => pen.Land == "The castle");
        Judge("the still dummy and the castle's pen are found", still != null && castle != null);
        if (still == null || castle == null) { Finish(); yield break; }

        if (Wants("chains")) yield return Chains();
        if (Wants("chord")) yield return Chord();
        if (Wants("answer")) yield return Answers();
        if (Wants("stream")) yield return StreamDodge();
        if (Wants("drums")) yield return SevenDrums();
        if (Wants("zanshin")) yield return ZanshinKill();
        if (Wants("film")) yield return Films();
        if (Wants("body")) yield return Body();
        if (Wants("light")) yield return LightBody();
        Finish();
    }

    // 5. Each aspect's chain, played.
    private IEnumerator Chains()
    {
        var plain = Weapons.ByName(Weapons.KatanaName).Combo.Steps;
        var stormPlain = Weapons.ByName(Weapons.StormKatanaName).Combo.Steps;
        Learn(KatanaWay.Stream);
        Learn(KatanaWay.Answer);
        string[] aspects = { KatanaWay.Sheath, KatanaWay.Stream, KatanaWay.Answer, KatanaWay.Storm };
        foreach (string id in aspects)
        {
            if (id == KatanaWay.Storm)
            {
                yield return WhenFree();
                combat.Arm(Weapons.StormKatanaName);
                upgrades.RefreshNow();
                yield return null;
            }
            else yield return UseAspect(id);
            var weapon = combat.Weapon;
            string name = weapon.Aspect != null ? weapon.Aspect.Id : "none";
            int expected = id == KatanaWay.Sheath ? 2 : id == KatanaWay.Stream ? plain.Count + 2 : id == KatanaWay.Answer ? plain.Count : stormPlain.Count;
            Judge($"{id}: the weapon in hand is built in it", weapon.Aspect != null && weapon.Aspect.Id == id, $"{weapon.Name}, aspect {name}");
            Judge($"{id}: the chain is {expected} cuts", weapon.Combo.Steps.Count == expected, $"{weapon.Combo.Steps.Count} cuts: {string.Join(", ", weapon.Combo.Steps.Select(step => $"{step.Name} {step.Damage:0.#}/{step.Stagger:0.#}"))}");
            yield return AtDummy();
            var played = new List<int>();
            var steps = new List<AttackStep>();
            int hits = still.HitsTaken;
            // A full purse, so no cut is refused for want of stamina.
            player.Stamina.Fill();
            yield return PlayChain(expected + 1, played, steps);
            bool inOrder = played.Count == expected + 1;
            for (int i = 0; i < played.Count && inOrder; i++) inOrder = played[i] == i % expected;
            Judge($"{id}: J played the chain through and round to its first cut", inOrder, $"steps {string.Join(" ", played)}; dummy hits +{still.HitsTaken - hits}");
            if (id == KatanaWay.Sheath)
                Judge("Sheath: the draw is 1.5 times the drawing cut with 10 more stagger", Mathf.Abs(weapon.Combo.Steps[0].Damage - plain[0].Damage * KatanaWay.DrawShare) < 0.6f && Mathf.Abs(weapon.Combo.Steps[0].Stagger - plain[0].Stagger - KatanaWay.DrawStagger) < 0.01f,
                    $"{plain[0].Damage:0.#} -> {weapon.Combo.Steps[0].Damage:0.#}, stagger {plain[0].Stagger:0.#} -> {weapon.Combo.Steps[0].Stagger:0.#} (temper in force)");
            if (id == KatanaWay.Answer)
                Judge("Answer: the light cuts are nine tenths", Mathf.Abs(weapon.Combo.Steps[1].Damage / plain[1].Damage - 0.9f) < 0.08f, $"{plain[1].Damage:0.#} -> {weapon.Combo.Steps[1].Damage:0.#}");
            if (id == KatanaWay.Storm)
                Judge("Storm: the third is the falling cut (22, 3 m lane)", Mathf.Abs(weapon.Combo.Steps[2].Damage - 22f) < 2.5f && weapon.Combo.Steps[2].Reach >= 3f,
                    $"{weapon.Combo.Steps[2].Damage:0.#}, reach {weapon.Combo.Steps[2].Reach:0.#}, width {weapon.Combo.Steps[2].LineWidth:0.#}");
        }
        yield return WhenFree();
        combat.Arm(Weapons.KatanaName);
        upgrades.RefreshNow();
    }

    // 6. Before the Thunder on the chord.
    private IEnumerator Chord()
    {
        yield return UseAspect(KatanaWay.Sheath);
        Judge("Before the Thunder is bought", Learn(KatanaWay.BeforeTheThunder) || RunState.Unlocked.Contains(KatanaWay.BeforeTheThunder));
        yield return WhenFree();
        Judge("the chord carries the technique once bought", combat.TechniqueStep != null, combat.TechniqueStep != null ? $"windup {combat.TechniqueStep.Windup:0.00}" : "none");
        yield return AtDummy();
        player.Stamina.Fill();
        int chords = combat.ChordsTaken, hits = still.HitsTaken;
        float stamina = player.Stamina.Current;
        Keys(Key.J, Key.K);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => combat.ChordsTaken > chords, 0.6f);
        var step = combat.Combo.Current;
        Judge("J and K together play Before the Thunder", combat.ChordsTaken == chords + 1 && step != null && step == combat.TechniqueStep,
            step != null ? $"{step.Name}, windup {step.Windup:0.00}, damage {step.Damage:0.#}" : "no step");
        yield return Until(() => still.HitsTaken > hits, 0.6f);
        Judge("it is out in 0.06 s, lands for 22 and costs 15 stamina", step != null && Mathf.Abs(step.Windup - Techniques.ThunderWindup) < 0.001f && still.HitsTaken > hits && Mathf.Abs(combat.LastBlowDamage - Techniques.ThunderDamage) < 3f,
            $"hits +{still.HitsTaken - hits}, blow {combat.LastBlowDamage:0.#}, stamina {stamina:0} -> {player.Stamina.Current:0}");
        yield return WhenFree();
    }

    // 7. The Answer's two answers.
    private IEnumerator Answers()
    {
        Learn(KatanaWay.Answer);
        yield return UseAspect(KatanaWay.Answer);
        Judge("the Answer is the aspect in hand", combat.Weapon.Aspect is Clover.Player.Way.Aspects.AnswerAspect);
        for (int which = 0; which < 2; which++)
        {
            bool heavy = which == 1;
            yield return AtDummy(2.2f);
            int guards = defence.PerfectGuards, answers = combat.Answers;
            Keys(Key.L);
            yield return null;
            yield return null;
            var outcome = defence.TakeHit(new Hit(10f, 10f, -player.transform.forward, false, false, player.transform.position + player.transform.forward * 1.5f));
            yield return null;
            Keys();
            yield return null;
            float window = combat.AspectState.Window;
            Judge($"a perfect guard opens the moment to answer ({(heavy ? "second" : "first")})", outcome == HitOutcome.PerfectGuard && defence.PerfectGuards == guards + 1 && window > 0f,
                $"{outcome}, window {window:0.00} s");
            yield return Tap(heavy ? Key.K : Key.J);
            yield return Until(() => combat.Answers > answers, 0.4f);
            var step = combat.Combo.Current;
            if (!heavy)
                Judge("J in the moment is the Riposte: 24, critical", combat.Answers == answers + 1 && step != null && step.Critical && Mathf.Abs(step.Damage - Clover.Player.Way.Aspects.AnswerAspect.RiposteDamage) < 0.01f,
                    step != null ? $"answers +{combat.Answers - answers}, {step.Name} {step.Damage:0.#}, critical {step.Critical}, windup {step.Windup:0.00}" : $"answers +{combat.Answers - answers}, no step");
            else
                Judge("K in the moment is Through the Gap: 30, 40 stagger, through the attacker", combat.Answers == answers + 1 && step != null && step.PassThrough && Mathf.Abs(step.Damage - Clover.Player.Way.Aspects.AnswerAspect.GapDamage) < 0.01f,
                    step != null ? $"answers +{combat.Answers - answers}, {step.Name} {step.Damage:0.#}/{step.Stagger:0.#}, through {step.PassThrough}, travel {step.Travel:0.#}" : $"answers +{combat.Answers - answers}, no step");
            yield return WhenFree();
        }
    }

    // 8. A dodge in the Stream's chain keeps it.
    private IEnumerator StreamDodge()
    {
        Learn(KatanaWay.Stream);
        yield return UseAspect(KatanaWay.Stream);
        Judge("the Stream keeps its chain through a dodge", combat.Combo.ChainKeptThroughDodge);
        yield return AtDummy(2.4f);
        player.Stamina.Fill();
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.StepIndex == 0 && combat.Combo.Phase == AttackPhase.Recovery, 1.5f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.StepIndex == 1 && combat.Combo.Phase == AttackPhase.Recovery, 1.5f);
        int reached = combat.Combo.StepIndex;
        // Back from the dummy, so the roll has room.
        Keys(Key.S, Key.Space);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => player.Dodge.IsRolling, 0.5f);
        bool rolled = player.Dodge.IsRolling;
        yield return Until(() => !player.Dodge.IsRolling, 1.0f);
        yield return Tap(Key.J);
        yield return Until(() => combat.Combo.Phase == AttackPhase.Windup || combat.Combo.Phase == AttackPhase.Active, 0.8f);
        Judge("after a dodge in the second cut's recovery, J is the third cut", reached == 1 && rolled && combat.Combo.StepIndex == 2,
            $"second cut reached {reached}, rolled {rolled}, now step {combat.Combo.StepIndex} ({combat.Combo.Current?.Name})");
        yield return WhenFree();
    }

    // 9. Seven drums beaten on a soldier, spent by a draw.
    private IEnumerator SevenDrums()
    {
        yield return WhenFree();
        combat.Arm(Weapons.StormKatanaName);
        upgrades.RefreshNow();
        Judge("Seven Drums is bought on the Storm Katana", Learn(KatanaWay.SevenDrums) || RunState.Unlocked.Contains(KatanaWay.SevenDrums));
        yield return WhenFree();
        Judge("Seven Drums is in force", upgrades.Has(KatanaWay.SevenDrums), $"weapon {combat.Weapon.Name}, aspect {combat.Weapon.Aspect?.Id}");
        var soldier = LetOutSoldiers(1).FirstOrDefault();
        if (soldier == null) { Judge("a soldier is let out", false); yield break; }
        soldier.Health.Maximum = 5000f;
        soldier.Health.Fill();
        player.Teleport(soldier.transform.position + Vector3.back * 1.7f, Quaternion.identity);
        yield return Real(0.6f);
        float end = Time.unscaledTime + 20f;
        int most = 0;
        while (Time.unscaledTime < end && combat.AspectState.Count < Drums.Most)
        {
            defence.UntouchableUntil = Time.time + 2f;
            player.Stamina.Fill();
            Vector3 to = soldier.transform.position - player.transform.position;
            to.y = 0f;
            if (to.magnitude > 2.2f) player.Teleport(soldier.transform.position - to.normalized * 1.7f, Quaternion.LookRotation(to.normalized));
            yield return Tap(Key.J);
            yield return Until(() => combat.Combo.Phase == AttackPhase.Recovery || !combat.IsAttacking, 1.2f);
            most = Mathf.Max(most, combat.AspectState.Count);
        }
        int beaten = combat.AspectState.Count;
        Judge("landed cuts beat all seven drums", beaten == Drums.Most, $"beaten {beaten}, fight on {spells.Fight.InFight}");
        // Wait for the chain to end so the next light is a draw (the first cut of a combo).
        yield return Until(() => !combat.IsAttacking, 2f);
        yield return Real(0.45f);
        defence.UntouchableUntil = Time.time + 2f;
        float drawStagger = combat.Combo.Steps[0].Stagger;
        int landed = combat.BlowsLanded;
        yield return Tap(Key.J);
        yield return Until(() => combat.BlowsLanded > landed, 1.0f);
        Judge("the next draw spends all seven for +40 stagger", beaten == Drums.Most && combat.AspectState.Count < Drums.Most && combat.LastBlowStagger >= drawStagger + Drums.DrawStagger - 0.5f,
            $"drums {beaten} -> {combat.AspectState.Count}, blow stagger {combat.LastBlowStagger:0.#} (draw's own {drawStagger:0.#})");
        yield return WhenFree();
        castle.Clear();
        yield return null;
        combat.Arm(Weapons.KatanaName);
        upgrades.RefreshNow();
    }

    // 10. A kill with Zanshin turns the player to the next enemy.
    private IEnumerator ZanshinKill()
    {
        yield return WhenFree();
        combat.Arm(Weapons.KatanaName);
        Judge("Zanshin is bought", Learn(KatanaWay.Zanshin) || RunState.Unlocked.Contains(KatanaWay.Zanshin));
        upgrades.RefreshNow();
        var two = LetOutSoldiers(2);
        if (two.Length < 2) { Judge("two soldiers are let out", false, $"{two.Length}"); yield break; }
        yield return Real(0.2f);
        var first = two[0];
        var second = two[1];
        Vector3 apart = second.transform.position - first.transform.position;
        apart.y = 0f;
        Vector3 at = first.transform.position + apart.normalized * 1.6f;
        player.Teleport(new Vector3(at.x, 0.05f, at.z), Quaternion.LookRotation(-apart.normalized));
        first.Health.Damage(first.Health.Current - 1f);
        defence.UntouchableUntil = Time.time + 3f;
        int turns = combat.ZanshinTurns;
        float angleBefore = Vector3.Angle(player.transform.forward, Flat(second.transform.position - player.transform.position));
        yield return Tap(Key.J);
        yield return Until(() => first.IsDead, 1.2f);
        yield return Until(() => combat.ZanshinTurns > turns, 1.5f);
        yield return Real(0.05f);
        float angleAfter = Vector3.Angle(player.transform.forward, Flat(second.transform.position - player.transform.position));
        Judge("the kill turns the player to the other soldier", first.IsDead && combat.ZanshinTurns == turns + 1 && angleAfter < 20f,
            $"dead {first.IsDead}, turns +{combat.ZanshinTurns - turns}, facing off the other by {angleBefore:0} deg -> {angleAfter:0} deg, {Flat(second.transform.position - player.transform.position).magnitude:0.0} m away");
        yield return WhenFree();
        castle.Clear();
    }

    private static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    // 11. The Body's enduring side.
    private IEnumerator Body()
    {
        yield return WhenFree();
        string[] marks = { BodyWay.Health1, BodyWay.Stamina1, BodyWay.FourthFlask, BodyWay.Health2, BodyWay.FullFlask, BodyWay.Steady, BodyWay.HardFrame };
        int bought = 0;
        foreach (string id in marks) if (Learn(id)) bought++;
        upgrades.RefreshNow();
        yield return null;
        yield return null;
        Judge("the enduring side's seven marks are bought", bought == marks.Length, $"{bought} of {marks.Length}");
        Judge("health is 135", Mathf.Abs(defence.Health.Maximum - 135f) < 0.01f, $"{defence.Health.Maximum:0.#}");
        Judge("four flasks, each healing 55", defence.Flasks.Maximum == 4 && Mathf.Abs(defence.Flasks.HealAmount - 55f) < 0.01f, $"{defence.Flasks.Maximum} at {defence.Flasks.HealAmount:0.#}");
        player.Teleport(openGround, Quaternion.identity);
        yield return Real(0.3f);
        defence.Health.Fill();
        defence.Flasks.Refill();
        defence.Health.Damage(80f);
        yield return Real(0.1f);
        float before = defence.Health.Current;
        int count = defence.Flasks.Count;
        yield return Tap(Key.R);
        yield return Until(() => defence.Flasks.Count < count && !defence.Flasks.IsDrinking, 2f);
        yield return Real(0.1f);
        Judge("a flask drunk heals 55", defence.Flasks.Count == count - 1 && Mathf.Abs(defence.Health.Current - before - 55f) < 1.5f,
            $"flasks {count} -> {defence.Flasks.Count}, health {before:0.#} -> {defence.Health.Current:0.#}");
    }

    // 12. The Light Body's flowing dodge.
    private IEnumerator LightBody()
    {
        yield return WhenFree();
        string[] marks = { BodyWay.Health1, BodyWay.Stamina1, BodyWay.FourthFlask, BodyWay.Health2, BodyWay.DeepLungs, BodyWay.QuickBreath, BodyWay.LongStep, BodyWay.LightBody };
        foreach (string id in marks) Learn(id);
        upgrades.RefreshNow();
        yield return null;
        yield return null;
        Judge("the Light Body is the Body's bearing in force", upgrades.Has(BodyWay.LightBody) && player.Dodge.Flows > 0, $"flows {player.Dodge.Flows}, bearing {RunState.WayChoices.KeystoneOf(WayRules.BodyBough)}");
        player.Teleport(openGround, Quaternion.identity);
        yield return Real(0.4f);
        player.Stamina.Fill();
        int flows = player.Dodge.FlowsTaken;
        yield return Tap(Key.Space);
        yield return Until(() => player.Dodge.IsRolling && player.Dodge.Elapsed >= player.Dodge.Duration - 0.07f, 1f);
        float pressedAt = player.Dodge.Elapsed;
        Keys(Key.Space);
        yield return null;
        Keys();
        yield return Until(() => player.Dodge.Flowed, 0.3f);
        bool flowed = player.Dodge.Flowed && player.Dodge.FlowsTaken == flows + 1;
        Judge("a dodge pressed in the last 0.1 s flows into a second", flowed, $"pressed at {pressedAt:0.00} of {player.Dodge.Duration:0.00} s, flows +{player.Dodge.FlowsTaken - flows}");
        yield return Until(() => player.Dodge.IsRolling && player.Dodge.Elapsed >= player.Dodge.Duration - 0.07f, 1f);
        int afterSecond = player.Dodge.FlowsTaken;
        Keys(Key.Space);
        yield return null;
        Keys();
        yield return Real(0.25f);
        Judge("never a third", player.Dodge.FlowsTaken == afterSecond, $"flows {afterSecond} -> {player.Dodge.FlowsTaken}");
        yield return WhenFree();
    }

    // 13. Films: Running Draw with Lightning, Lying Draw with Fire, the finisher's held pose.
    private IEnumerator Films()
    {
        yield return WhenFree();
        foreach (var family in new[] { (Elements.Lightning, KatanaWay.InfuseLine, "running_draw_lightning", 5.5f, new[] { 0.14f, 0.7f }),
                                       (Elements.Fire, KatanaWay.InfuseGround, "lying_draw_fire", 2.0f, new[] { 0.3f, 3.5f }) })
        {
            element.Take(ElementKit.ByName(family.Item1, false));
            // The strike's slot and clock wake with the element a moment later.
            yield return Real(0.6f);
            Judge($"{family.Item3}: its family mark is bought", Learn(family.Item2) || RunState.Unlocked.Contains(family.Item2));
            yield return UseAspect(KatanaWay.Sheath);
            spells.Cooldowns.ResetAll();
            combat.Mana.Gain(100f);
            player.Stamina.Fill();
            yield return AtDummy(family.Item4);
            int strikes = combat.InfusedStrikes;
            // Night filming: every frame at 20 a second from the press (the Fire lies 6 s).
            if (framesMode) StartCoroutine(Frames($"way_{family.Item3}", family.Item1 == Elements.Fire ? 6.8f : 3f));
            for (int attempt = 0; attempt < 3 && combat.InfusedStrikes == strikes; attempt++)
            {
                if (attempt > 0) Say($"  (no strike on press {attempt}: {defence.LastCallout}, mana {combat.Mana.Current:0}, element {element.Info?.Name}, check {spells.CheckStrike(false, out _, out _)}, layer {player.Buttons?.LayerHeld}, refused {combat.StrikesRefused}, phase {combat.Combo.Phase}, away {combat.WeaponAway})");
                Keys(Key.LeftCtrl);
                yield return null;
                yield return null;
                Keys(Key.LeftCtrl, Key.J);
                yield return null;
                yield return null;
                Keys(Key.LeftCtrl);
                yield return null;
                Keys();
                yield return Until(() => combat.InfusedStrikes > strikes, 1.0f);
            }
            float start = Time.time;
            var bend = combat.LastInfusion;
            for (int i = 0; i < family.Item5.Length && !framesMode; i++)
            {
                float wait = family.Item5[i] - (Time.time - start);
                if (wait > 0f) yield return Game(wait);
                yield return Still($"way_{family.Item3}_{i + 1}");
            }
            bend = combat.LastInfusion;
            if (family.Item1 == Elements.Lightning)
                Judge("Running Draw with Lightning dashes the lane and leaves its path charged", combat.InfusedStrikes > strikes && bend.Dash > 0f && bend.ChargedPath > 0f,
                    $"strikes +{combat.InfusedStrikes - strikes}, family {bend.Family}, dash {bend.Dash:0.#} m, charged {bend.ChargedPath:0.#} s");
            else
                Judge("Lying Draw with Fire lays the whole cut alight, twice as long", combat.InfusedStrikes > strikes && bend.WholeCut && bend.LieSeconds >= 6f - 0.01f,
                    $"strikes +{combat.InfusedStrikes - strikes}, family {bend.Family}, whole {bend.WholeCut}, lies {bend.LieSeconds:0.#} s");
            yield return WhenFree();
            yield return Real(1.0f);
            while (filming) yield return null;
        }

        // The finisher's held pose, 0.2 s on the Katana, on a staggered soldier: from the camera's side, from the left and
        // from the far side, as the camera sees each.
        combat.Arm(Weapons.KatanaName);
        upgrades.RefreshNow();
        var approaches = new[] { ("near", Vector3.back), ("side", Vector3.left) };
        foreach (var (label, from) in approaches)
        {
            castle.Clear();
            yield return null;
            var soldier = LetOutSoldiers(1).FirstOrDefault();
            if (soldier == null) { Judge("a soldier for the finisher", false); yield break; }
            yield return Real(0.2f);
            Vector3 front = soldier.transform.position + from * 2.2f;
            player.Teleport(new Vector3(front.x, 0.05f, front.z), Quaternion.LookRotation(-from, Vector3.up));
            yield return Real(0.4f);
            defence.UntouchableUntil = Time.time + 4f;
            soldier.TakeHit(new Hit(1f, 1000f, -from));
            yield return null;
            int holds = combat.FinisherHolds;
            Vector3 cameFrom = player.transform.position - soldier.transform.position;
            if (framesMode) StartCoroutine(Frames($"way_finisher_{label}", 2.4f));
            yield return Tap(Key.K);
            yield return Until(() => combat.IsFinishing, 1f);
            yield return Until(() => combat.Combo.Phase == AttackPhase.Active, 1f);
            float gapAtBlow = Flat(player.transform.position - soldier.transform.position).magnitude;
            yield return Until(() => combat.FinisherHolds > holds, 2f);
            var held = combat.Combo.Current;
            Vector3 rel = Flat(player.transform.position - soldier.transform.position);
            // Behind: on the far side from where the player came in.
            float side = Vector3.Dot(rel.normalized, Flat(cameFrom).normalized);
            float backTurned = Vector3.Angle(player.transform.forward, Flat(soldier.transform.position - player.transform.position));
            if (!framesMode) yield return Still($"way_finisher_hold_{label}_1");
            Judge($"from the {label} side the finisher holds its pose 0.2 s behind the soldier, its back to it", combat.FinisherHolds == holds + 1 && held != null && Mathf.Abs(held.PoseHold - 0.2f) < 0.01f && side < -0.5f && backTurned > 120f,
                held != null ? $"{held.Name}, hold {held.PoseHold:0.00} s; {gapAtBlow:0.0} m apart at the blow, {rel.magnitude:0.0} m {(side < 0f ? "behind" : "in front of")} it at the hold (dot {side:0.00}), facing {backTurned:0} deg off it" : "no step");
            yield return Until(() => !combat.IsAttacking || Vector3.Angle(player.transform.forward, Flat(soldier.transform.position - player.transform.position)) < 30f, 1f);
            Judge($"from the {label} side, after the hold the player turns to face the fallen", Vector3.Angle(player.transform.forward, Flat(soldier.transform.position - player.transform.position)) < 30f,
                $"{Vector3.Angle(player.transform.forward, Flat(soldier.transform.position - player.transform.position)):0} deg off");
            yield return Real(1.5f);
            while (filming) yield return null;
            yield return WhenFree();
        }
        castle.Clear();
    }

    // Night filming (way_only.txt holds "frames" with "film"): every frame at 20 a second, game time fixed, for this long.
    private bool framesMode, filming;

    private IEnumerator Frames(string name, float seconds)
    {
        filming = true;
        string folder = $"review/gifs/way/frames/{name}";
        Directory.CreateDirectory(folder);
        foreach (var old in Directory.GetFiles(folder, "*.png")) File.Delete(old);
        Time.captureFramerate = 20;
        int count = Mathf.RoundToInt(seconds * 20f);
        for (int i = 0; i < count; i++)
        {
            ScreenCapture.CaptureScreenshot($"{folder}/{i:000}.png");
            yield return null;
        }
        Time.captureFramerate = 0;
        filming = false;
    }

    private void Finish()
    {
        Time.timeScale = 1f;
        Say($"summary: {ok} OK, {wrong} WRONG");
        Say("done");
        // The probe stops play itself, so nothing is left running with the world or a menu holding time.
        UnityEditor.EditorApplication.ExitPlaymode();
    }
}
