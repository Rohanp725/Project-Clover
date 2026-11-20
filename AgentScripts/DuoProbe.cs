using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Phase 4, task 4.5, in the Proving Ground with the playtest on: the duo branches and the heart, and "a shape swap changes
// the reaction". Copies of the still dummy stand on the east terrace as ReactionProbe2 sets them (the host, A 2 m west, C
// 2.2 m north); the player stands 3 m south of the host.
//  1. Flashover (Fire and Lightning): plain before Plasma's Shape mark is owned (its Static discharged, the live flame);
//     owned, Corona (nothing spent, the corona held, arcs into A); the shape swapped at a rest fire (refused away from one),
//     Fulgurite (a patch laid, no corona); the pair broken (Fire alone), the plain Flashover again.
//  2. Sleet Charge (Ice and Lightning): Thundersnow (the snow held, then a bolt on the chilled A and C), swapped to Glass
//     Spear (the spear held, then the host frozen when the charge ends).
//  3. Glass Storm: given and slotted, cast from the 1 key: its 14 s clock, Chill on the host, the blade charged with Ice;
//     refused from a slot with Fire and Lightning carried.
//  4. The chart's heart: Lightning's tree with Ice picked, then Fire; Glass Storm and Ball Lightning chosen in it; frames.
// Frames review/shots/current/duo_*.png. Every judged line ends OK or WRONG. The copies and the marks given are gone at
// the end, the player back to Fire alone; progress stays sandboxed (runprobe.sh).
// bash AgentScripts/runprobe.sh ProvingGround DuoProbe 120
public static class DuoProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Duo Probe").AddComponent<DuoProbeRunner>();
        return "probe started";
    }
}

public class DuoProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerBuild build;
    private PlayerReactions reactions;
    private TrainingDummy host, a, c;
    private readonly List<TrainingDummy> made = new List<TrainingDummy>();
    private readonly List<string> given = new List<string>();
    private int ok, wrong;
    private bool playtestWas;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;

    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);
    private static readonly int Flashover = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);
    private static readonly int Sleet = AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Chill);

    private void Update()
    {
        if (defence != null && !defence.IsDead) defence.Health.Heal(100f);
    }

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
    }

    private IEnumerator Shot(string name)
    {
        yield return null;
        ScreenCapture.CaptureScreenshot("review/shots/current/" + name + ".png");
        yield return null;
        yield return null;
    }

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.unscaledTime + timeout;
        while (!condition() && Time.unscaledTime < end) yield return null;
    }

    private IEnumerator Real(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end) yield return null;
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

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private TrainingDummy Copy(TrainingDummy from, string name, Vector3 at)
    {
        var copy = Instantiate(from.gameObject, at, from.transform.rotation).GetComponent<TrainingDummy>();
        copy.name = name;
        made.Add(copy);
        return copy;
    }

    private static void Lay(TrainingDummy on, AfflictionKind kind, int stacks) => on.Afflictions.Lay(kind, stacks, 0f, 1f, LaySource.Spell, on);
    private static int Depth(TrainingDummy on, AfflictionKind kind) => on.Afflictions.Stacks(kind);
    private static bool Holds(TrainingDummy on, string condition) => on.Afflictions.Conditions.Holds(condition);

    private void Give(string id)
    {
        if (upgrades.Give(Upgrade.ById(id))) given.Add(id);
        upgrades.RefreshNow();
    }

    // Everything off every copy, their health and balance back, the ground laid gone, and the host's rest for the pair over.
    private IEnumerator Clean(int pair)
    {
        foreach (var dummy in made)
        {
            dummy.Afflictions.Clear();
            dummy.Health.Fill();
            dummy.Stagger.Reset();
        }
        yield return Until(() => Hazards.Alive == 0 && host.Afflictions.ReactionRest(pair) <= 0f, 9f);
        player.Teleport(host.transform.position + Vector3.back * 3f, Quaternion.identity);
        yield return Real(0.3f);
    }

    // Lays the first affliction, then the second, and waits for the pair to react once on the host.
    private IEnumerator React(string name, int pair, AfflictionKind first, int firstStacks, AfflictionKind second, int secondStacks)
    {
        int shown = reactions.TimesShown(pair);
        Lay(host, first, firstStacks);
        yield return null;
        Lay(host, second, secondStacks);
        yield return Until(() => reactions.TimesShown(pair) > shown, 1.5f);
        yield return Real(0.15f);
        Judge($"{name}: it reacts once on the host", reactions.TimesShown(pair) == shown + 1 && reactions.LastOn == host,
            $"shown {reactions.TimesShown(pair) - shown}, last '{reactions.LastShown}' in shape '{reactions.LastShape}'");
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        backgroundWas = InputSystem.settings.backgroundBehavior;
        editorWas = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        playtestWas = PlaytestMode.On;
        PlaytestMode.Set(true);
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        reactions = player.GetComponent<PlayerReactions>();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest {PlaytestMode.On}, sandboxed {Progress.Sandboxed}");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        Judge("the player has its build and the reactions' adapter", build != null && reactions != null && upgrades != null);
        Judge("Flashover's and Sleet Charge's Shape marks are in the table",
            ReactionTable.ShapeMarkFor(Flashover)?.Id == PlasmaTree.Shape && ReactionTable.ShapeMarkFor(Sleet)?.Id == SleetChargeDuo.Shape);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        var still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null) { Finish(); yield break; }
        host = Copy(still, "Probe Host", Middle);
        a = Copy(still, "Probe A", Middle + new Vector3(-2f, 0f, 0f));
        c = Copy(still, "Probe C", Middle + new Vector3(0f, 0f, 2.2f));
        yield return Real(0.3f);

        yield return FlashoverShapes();
        yield return SleetChargeShapes();
        yield return GlassStorm();
        yield return TheHeart();
        Finish();
    }

    // ---- 1. Flashover: plain, Corona, the swap, Fulgurite, the pair broken.
    private IEnumerator FlashoverShapes()
    {
        Say("--- Flashover");
        build.Attune(Elements.Fire, Elements.Lightning);
        RunState.Unlocked.Remove(PlasmaTree.Shape);
        upgrades.RefreshNow();
        yield return Clean(Flashover);
        int shaped = reactions.ShapedCount;
        yield return React("Flashover, Shape mark not owned", Flashover, AfflictionKind.Burn, 2, AfflictionKind.Static, 3);
        Judge("not owned: the plain Flashover runs (its Static discharged, the live flame)", reactions.LastShape == "" && reactions.ShapedCount == shaped
            && Depth(host, AfflictionKind.Static) == 0 && Holds(host, Reactions.LiveFlame) && host.Afflictions.Burn.IsBurning,
            $"shape '{reactions.LastShape}', static {Depth(host, AfflictionKind.Static)}, live {Holds(host, Reactions.LiveFlame)}");

        Give(PlasmaTree.Shape);
        Judge("the Shape mark is owned and in force with both carried", upgrades.Has(PlasmaTree.Shape));
        Judge("away from a rest fire the shape cannot be changed", !build.ChooseShape(ReactionTable.ShapeKey(Flashover), 1, false));
        Judge("at a rest fire Corona is chosen", build.ChooseShape(ReactionTable.ShapeKey(Flashover), 0, true) && build.Read().ShapeOf(ReactionTable.ShapeKey(Flashover)) == 0);
        yield return Clean(Flashover);
        int aHits = a.HitsTaken;
        yield return React("Flashover in Corona", Flashover, AfflictionKind.Burn, 2, AfflictionKind.Static, 3);
        Judge("Corona runs instead: nothing spent, the corona held, no live flame", reactions.LastShape == "Corona" && Depth(host, AfflictionKind.Static) == 3
            && Holds(host, PlasmaTree.Corona) && !Holds(host, Reactions.LiveFlame),
            $"shape '{reactions.LastShape}', static {Depth(host, AfflictionKind.Static)}, corona {Holds(host, PlasmaTree.Corona)}, live {Holds(host, Reactions.LiveFlame)}");
        yield return Real(1.6f);
        yield return Shot("duo_corona");
        Judge("Corona's arcs leap into A, burning and charging it", a.HitsTaken > aHits && a.Afflictions.Burn.IsBurning && Depth(a, AfflictionKind.Static) > 0,
            $"A hits {a.HitsTaken - aHits}, burning {a.Afflictions.Burn.IsBurning}, static {Depth(a, AfflictionKind.Static)}");

        // The swap at the fire changes the reaction.
        Judge("at a rest fire Fulgurite is chosen", build.ChooseShape(ReactionTable.ShapeKey(Flashover), 1, true));
        yield return Clean(Flashover);
        int alive = Hazards.Alive, hostHits = host.HitsTaken;
        aHits = a.HitsTaken;
        yield return React("Flashover in Fulgurite", Flashover, AfflictionKind.Burn, 2, AfflictionKind.Static, 3);
        Judge("Fulgurite runs instead: a patch is laid under it and no corona", reactions.LastShape == "Fulgurite" && Hazards.Alive > alive && !Holds(host, PlasmaTree.Corona),
            $"shape '{reactions.LastShape}', ground {alive}->{Hazards.Alive}, corona {Holds(host, PlasmaTree.Corona)}");
        yield return Real(1.4f);
        yield return Shot("duo_fulgurite");
        Judge("Fulgurite bites the host where it stands and leaves A, 2 m off, alone", host.HitsTaken > hostHits && a.HitsTaken == aHits,
            $"host hits {host.HitsTaken - hostHits}, A hits {a.HitsTaken - aHits}");

        // With the pair broken the plain reaction comes back (a Burn from the world still reacts).
        build.Attune(Elements.Fire, "");
        yield return Clean(Flashover);
        yield return React("Flashover with Fire alone", Flashover, AfflictionKind.Burn, 2, AfflictionKind.Static, 3);
        Judge("the pair broken: the plain Flashover again, the shape kept for later", reactions.LastShape == "" && Holds(host, Reactions.LiveFlame)
            && build.Read().ShapeOf(ReactionTable.ShapeKey(Flashover)) == 1, $"shape '{reactions.LastShape}'");
    }

    // ---- 2. Sleet Charge: Thundersnow, then Glass Spear.
    private IEnumerator SleetChargeShapes()
    {
        Say("--- Sleet Charge");
        build.Attune(Elements.Ice, Elements.Lightning);
        Give(SleetChargeDuo.Shape);
        Judge("Ice and Lightning carried, Sleet Charge's Shape mark in force", upgrades.Has(SleetChargeDuo.Shape) && !upgrades.Has(PlasmaTree.Shape));
        Judge("Thundersnow chosen at a rest fire", build.ChooseShape(ReactionTable.ShapeKey(Sleet), 0, true));
        yield return Clean(Sleet);
        Lay(a, AfflictionKind.Chill, 1);
        Lay(c, AfflictionKind.Chill, 1);
        int aHits = a.HitsTaken, cHits = c.HitsTaken;
        yield return React("Sleet Charge in Thundersnow", Sleet, AfflictionKind.Chill, 1, AfflictionKind.Static, 1);
        Judge("Thundersnow runs instead: the snow charges it", reactions.LastShape == "Thundersnow" && Holds(host, SleetChargeDuo.Snow) && !Holds(host, Reactions.Charging),
            $"shape '{reactions.LastShape}', snow {Holds(host, SleetChargeDuo.Snow)}, plain {Holds(host, Reactions.Charging)}");
        yield return Real(1.6f);
        Judge("the frost charges itself", Depth(host, AfflictionKind.Static) >= 3, $"static {Depth(host, AfflictionKind.Static)}");
        yield return Until(() => !Holds(host, SleetChargeDuo.Snow), 3f);
        yield return Real(0.4f);
        yield return Shot("duo_thundersnow");
        Judge("when it ends the bolt runs on to the chilled A and C, charging them", a.HitsTaken > aHits && c.HitsTaken > cHits && Depth(a, AfflictionKind.Static) > 0 && Depth(c, AfflictionKind.Static) > 0,
            $"A hits {a.HitsTaken - aHits} static {Depth(a, AfflictionKind.Static)}, C hits {c.HitsTaken - cHits} static {Depth(c, AfflictionKind.Static)}");

        Judge("Glass Spear chosen at a rest fire", build.ChooseShape(ReactionTable.ShapeKey(Sleet), 1, true));
        yield return Clean(Sleet);
        yield return React("Sleet Charge in Glass Spear", Sleet, AfflictionKind.Chill, 1, AfflictionKind.Static, 1);
        Judge("Glass Spear runs instead: the spear's charge held, no snow", reactions.LastShape == "Glass Spear" && Holds(host, SleetChargeDuo.Spear) && !Holds(host, SleetChargeDuo.Snow));
        yield return Until(() => !Holds(host, SleetChargeDuo.Spear), 4f);
        yield return Real(0.3f);
        yield return Shot("duo_glass_spear");
        Judge("when it ends the host is frozen, whatever its Chill was", host.Afflictions.IsFrozen, $"frozen {host.Afflictions.IsFrozen}, chill {Depth(host, AfflictionKind.Chill)}");
    }

    // ---- 3. Glass Storm from a slot.
    private IEnumerator GlassStorm()
    {
        Say("--- Glass Storm");
        build.Attune(Elements.Ice, Elements.Lightning);
        yield return Clean(Sleet);
        Give(SleetChargeDuo.GlassStorm);
        spells.SetSlot(0, SleetChargeDuo.GlassStorm);
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(100f);
        Judge("learned, it sits in the West slot with both carried", upgrades.CanSlot(SleetChargeDuo.GlassStorm) && spells.Slots[0] == SleetChargeDuo.GlassStorm, $"West '{spells.Slots[0]}'");
        player.Teleport(host.transform.position + Vector3.back * 1.6f, Quaternion.identity);
        yield return Real(0.4f);
        int casts = spells.Casts;
        float mana = combat.Mana.Current;
        yield return Tap(Key.Digit1);
        yield return Until(() => spells.Casts > casts, 1.5f);
        yield return Real(1.6f);
        yield return Shot("duo_glass_storm");
        Judge("cast from the 1 key: its 14 s clock runs and no mana is paid", spells.Casts == casts + 1 && spells.Cooldowns.Length(0) > 12f && spells.Cooldowns.Length(0) < 15f && combat.Mana.Current >= mana - 0.5f,
            $"clock {spells.Cooldowns.Length(0):0.0}s, mana {mana:0}->{combat.Mana.Current:0}");
        Judge("its shards chill the host and charge the blade with Ice", host.Afflictions.Has(AfflictionKind.Chill) && combat.Blade.Element == Elements.Ice,
            $"chill {Depth(host, AfflictionKind.Chill)}, static {Depth(host, AfflictionKind.Static)}, blade '{combat.Blade.Element}'");
        build.Attune(Elements.Fire, Elements.Lightning);
        Judge("with Fire and Lightning carried it leaves its slot", spells.Slots[0] != SleetChargeDuo.GlassStorm && !upgrades.CanSlot(SleetChargeDuo.GlassStorm), $"West '{spells.Slots[0]}'");
        yield return Until(() => Hazards.Alive == 0, 9f);
    }

    // ---- 4. The heart of the chart.
    private IEnumerator TheHeart()
    {
        Say("--- the heart");
        var menu = UpgradeMenu.Instance;
        Judge("the chart is there", menu != null);
        if (menu == null) yield break;
        build.Attune(Elements.Lightning, Elements.Ice);
        menu.Open();
        yield return null;
        Judge("open on Lightning with Ice carried: the heart shows Sleet Charge", menu.IsOpen && menu.Viewed == Elements.Lightning && menu.HeartPair == Sleet && menu.HeartPartner == Elements.Ice,
            $"viewed {menu.Viewed}, heart {menu.HeartPair} with '{menu.HeartPartner}'");
        Judge("Glass Storm can be chosen in the heart", menu.Choose(SleetChargeDuo.GlassStorm) && menu.Chosen?.Id == SleetChargeDuo.GlassStorm);
        Judge("Plasma's marks are not shown with Ice", !menu.Choose(PlasmaTree.BallLightning));
        yield return Real(0.8f);
        yield return Shot("duo_heart_sleet");
        Judge("the shoulder buttons' partner picker turns to Fire", menu.ViewPartner(1) && menu.HeartPartner == Elements.Fire && menu.HeartPair == Flashover, $"heart with '{menu.HeartPartner}'");
        Judge("Ball Lightning can be chosen in the heart now", menu.Choose(PlasmaTree.BallLightning));
        Judge("and Glass Storm not", !menu.Choose(SleetChargeDuo.GlassStorm));
        yield return Real(0.8f);
        yield return Shot("duo_heart_plasma");
        // The stick walks from the heart back up to the tree.
        bool left = false;
        for (int i = 0; i < 12 && !left; i++)
        {
            menu.Step(Vector2.up);
            left = menu.Chosen != null && !menu.Chosen.IsMix;
        }
        Judge("the stick walks out of the heart up into Lightning's tree", left, menu.Chosen?.Id);
        Judge("on Fire's tree the heart turns to Fire's one built partner, Lightning", menu.View(Elements.Fire) && menu.HeartPartner == Elements.Lightning, $"'{menu.HeartPartner}'");
        menu.Close();
        yield return Real(0.3f);
    }

    private void Finish()
    {
        foreach (var dummy in made) if (dummy != null) Destroy(dummy.gameObject);
        foreach (string id in given) RunState.Unlocked.Remove(id);
        if (spells != null && spells.Slots[0] == SleetChargeDuo.GlassStorm) spells.SetSlot(0, "");
        if (build != null) build.Attune(Elements.Fire, "");
        if (upgrades != null) upgrades.RefreshNow();
        PlaytestMode.Set(playtestWas);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
