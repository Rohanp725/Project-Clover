using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Magic;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;

// Phase 2 (tasks 2.1 to 2.4) in the Proving Ground with the playtest on. The player arrives carrying Fire; the altars'
// prompts are read and Lightning is carried second at its altar; the Katana in hand. Then the rest fire opens the rest menu, the Hand is set there with the keys and with a pad (Flame Wave West,
// Thunderclap North), a saved Hand kept and worn again, the Hand refused away from a fire, the chart still reached from
// the rest menu; then at the still dummy: each spell lays its own affliction and is cast in its own colour, the blade
// takes the landed spell's own element (Fire after Flame Wave, Lightning after Thunderclap), weapon blows lay only
// Fire's (Cinder and Static both learned), L2 with R1 strikes with Fire and L2 with R2 with Lightning (no mana), all in
// (Fire's altar again, then again) and with one element R2 is the deep strike again (20 mana). Thunderclap lays nothing
// of its own, so it is checked to strike and charge the blade. Progress stays in the sandbox (runprobe.sh sets it); the
// run's Hands and attunements live in memory and are put back to Fire alone at the end. Frames:
// review/shots/current/p2_*.png. Every judged line ends OK or WRONG.
// bash AgentScripts/runprobe.sh ProvingGround P2Probe 200
public static class P2Probe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("P2 Probe").AddComponent<P2ProbeRunner>();
        return "probe started";
    }
}

public class P2ProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerBuild build;
    private PlayerInteraction interaction;
    private LoadoutChoice fireAltar, lightningAltar;
    private XInputControllerWindows pad;
    private TrainingDummy still;
    private bool keepAlive = true;
    private int ok, wrong;
    private string lastSpellBody = "none";
    private string Diag() => $" [dummy hits {still.HitsTaken}, burn {still.Afflictions.Burn.SecondsLeft:0.0}s, static {still.Afflictions.Stacks(AfflictionKind.Static)}, landed {combat.BlowsLanded}, spell struck {lastSpellBody}, gap {Vector3.Distance(player.transform.position, still.transform.position):0.0} m]";

    private void Update()
    {
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
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

    private IEnumerator Still(string name)
    {
        Time.timeScale = 0f;
        yield return Shot(name);
        Time.timeScale = 1f;
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

    private void Set(InputControl control, float value)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            ((InputControl<float>)control).WriteValueIntoEvent(value, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    private IEnumerator Press(InputControl control)
    {
        Set(control, 1f);
        yield return null;
        yield return null;
        Set(control, 0f);
        yield return null;
        yield return null;
    }

    // L2 held, then R1 or R2, then both let go: an infused strike from the layer.
    private IEnumerator PadStrike(InputControl attack)
    {
        Set(pad.leftTrigger, 1f);
        yield return null;
        yield return null;
        Set(attack, 1f);
        yield return null;
        yield return null;
        Set(attack, 0f);
        yield return null;
        Set(pad.leftTrigger, 0f);
        yield return null;
        yield return null;
    }

    // Flashovers shown so far, and whether one came on the still dummy since a count was taken (Phase 3's reactions).
    private int Flashovers()
    {
        var reactions = player.GetComponent<PlayerReactions>();
        return reactions != null ? reactions.TimesShown(AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static)) : 0;
    }

    private bool FlashedOnStill(int before)
    {
        var reactions = player.GetComponent<PlayerReactions>();
        return reactions != null && Flashovers() > before && reactions.LastOn == still;
    }

    private static T Find<T>(Func<T, bool> which) where T : UnityEngine.Object =>
        FindObjectsByType<T>(FindObjectsInactive.Include).FirstOrDefault(which);

    private IEnumerator Use(Interactable thing, float back = 0.8f)
    {
        Vector3 at = thing.transform.position + Vector3.back * back;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
        yield return Real(0.35f);
        yield return Tap(Key.E);
        yield return Real(0.2f);
    }

    // Stands at an altar and reads what its prompt offers; then, if asked, uses it.
    private string offered = "";
    private IEnumerator Altar(LoadoutChoice altar, bool use, string shot = null)
    {
        Vector3 at = altar.transform.position + Vector3.back * 1.0f;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
        yield return Real(0.45f);
        offered = interaction.Current == altar ? altar.Prompt : "(not the altar: " + (interaction.Current != null ? interaction.Current.Prompt : "nothing") + ")";
        if (shot != null) yield return Shot(shot);
        if (!use) yield break;
        yield return Tap(Key.E);
        yield return Real(0.3f);
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private IEnumerator WhenFree()
    {
        yield return Until(() => !combat.IsAttacking && !element.IsCasting && !player.Dodge.IsRolling && player.FormHold == 0 && !player.IsLeaping && !defence.IsDrinking, 6f);
        yield return Real(0.15f);
    }

    // Clears the dummy until nothing comes back on it for a second (ground fire out), up to twelve seconds.
    private IEnumerator Clean()
    {
        float end = Time.unscaledTime + 12f;
        float quietSince = Time.unscaledTime;
        while (Time.unscaledTime < end)
        {
            if (still.Afflictions.Any) { still.Afflictions.Clear(); quietSince = Time.unscaledTime; }
            if (Time.unscaledTime - quietSince > 1f) break;
            yield return null;
        }
    }

    private IEnumerator AtDummy(float back = 1.7f)
    {
        player.Teleport(still.transform.position + Vector3.back * back, Quaternion.identity);
        yield return WhenFree();
    }

    // In the Hand's list, steps the cursor to a choice by its id with the keys (S down, W up).
    private IEnumerator ChooseWithKeys(string id)
    {
        var hand = HandMenu.Instance;
        int target = hand.Choices.ToList().FindIndex(choice => choice.Id == id);
        if (target < 0) yield break;
        int guard = 0;
        while (hand.Choice != target && guard++ < 30) yield return Tap(hand.Choice < target ? Key.S : Key.W);
    }

    private IEnumerator ChooseWithPad(string id)
    {
        var hand = HandMenu.Instance;
        int target = hand.Choices.ToList().FindIndex(choice => choice.Id == id);
        if (target < 0) yield break;
        int guard = 0;
        while (hand.Choice != target && guard++ < 30) yield return Press(hand.Choice < target ? pad.dpad.down : pad.dpad.up);
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        interaction = player.GetComponent<PlayerInteraction>();
        combat.SpellBlowLanded += (body, cast) => lastSpellBody = body != null ? body.name + " #" + cast : "null";
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest mode {PlaytestMode.On}, sandboxed {Progress.Sandboxed}, carried '{(element.HasElement ? element.Info.Name : "")}' + '{(element.SecondInfo != null ? element.SecondInfo.Name : "")}', run state '{RunState.Element}' + '{RunState.Element2}'");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        Judge("the build is on the player", build != null);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();

        // 0. The altars (task 2.4): each prompt says what it will do before the press.
        fireAltar = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Fire);
        lightningAltar = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Lightning);
        // The Proving Ground's player arrives carrying Fire alone, so its own altar offers all in and is left alone here.
        Judge("the player arrives carrying Fire alone", element.HasElement && element.Info.Name == Elements.Fire && !element.Mixed && !element.AllIn);
        yield return Altar(fireAltar, false, "p2_altar_all_in");
        Judge("with Fire carried alone the Fire altar offers all in", offered == "Carry Fire all in (lays Burn)", $"'{offered}'");
        var katana = Find<LoadoutChoice>(choice => !choice.IsElement && choice.Choice == Weapons.KatanaName);
        yield return Use(katana, 1.0f);
        still = Find<TrainingDummy>(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found", still != null);

        // 1. Two attunements: Fire first, Lightning second at its altar; both roots learned so the weapon's rule can be seen.
        yield return Altar(lightningAltar, false, "p2_altar_second");
        Judge("with Fire carried the Lightning altar offers it as second", offered == "Carry Lightning as second (lays Static)", $"'{offered}'");
        yield return Altar(lightningAltar, true);
        Judge("using it carries Fire, then Lightning", element.Mixed && element.Info.Name == Elements.Fire && element.SecondInfo.Name == Elements.Lightning && defence.LastCallout == "Fire, then Lightning",
            $"said '{defence.LastCallout}'");
        yield return Altar(fireAltar, false);
        Judge("with two carried the Fire altar offers Fire alone", offered == "Carry Fire alone (lays Burn)", $"'{offered}'");
        upgrades.Give(Upgrade.ById(Clover.Player.Trees.FireTree.Cinder));
        upgrades.Give(Upgrade.ById(Upgrade.Static));
        upgrades.RefreshNow();
        yield return Real(0.4f);
        var loadout = build.Read();
        Judge("Fire and Lightning are carried, Fire first", element.Mixed && element.Info.Name == Elements.Fire && element.SecondInfo.Name == Elements.Lightning && loadout.Mixed,
            $"{loadout.First} + {loadout.Second}, surge {loadout.Surge}");
        Judge("both roots learned and in force", upgrades.Has(Clover.Player.Trees.FireTree.Cinder) && upgrades.Has(Upgrade.Static));
        Judge("the weapon lays Fire's affliction alone", combat.WeaponAffliction == AfflictionKind.Burn && combat.WeaponAfflictionStacks >= 1, $"{combat.WeaponAffliction} x{combat.WeaponAfflictionStacks}");

        // 2. The rest fire opens the rest menu; the Hand is set with the keys.
        var rest = Find<RestPoint>(point => true);
        yield return Use(rest, 1.2f);
        yield return Real(0.3f);
        Judge("the rest fire opens the rest menu, the Hand first", RestMenu.Showing && RestMenu.Instance.Choices.At(0) == RestEntry.Hand && Time.timeScale == 0f && player.Suspended,
            $"open {RestMenu.Showing}, time {Time.timeScale}");
        yield return Shot("p2_rest_menu");
        yield return Tap(Key.E);
        yield return Real(0.2f);
        var hand = HandMenu.Instance;
        Judge("Set the Hand opens the Hand", HandMenu.Showing && hand.Item == 0, $"item {hand?.Item}");
        yield return Shot("p2_hand");
        // West: Flame Wave.
        yield return Tap(Key.E);
        Judge("West's list holds both base spells and nothing else carried", hand.Choosing && hand.Choices.Any(c => c.Id == "fire.base") && hand.Choices.Any(c => c.Id == "lightning.base") && hand.Choices.All(c => c.Id == "" || c.IsBase || c.IsDuo || c.Element == Elements.Fire || c.Element == Elements.Lightning),
            string.Join(", ", hand.Choices.Select(c => c.Id)));
        yield return Shot("p2_hand_list_west");
        yield return ChooseWithKeys("fire.base");
        yield return Tap(Key.E);
        Judge("West holds Flame Wave", spells.Slots[0] == "fire.base" && !hand.Choosing, spells.Slots[0]);
        // North: Thunderclap, with the pad.
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;
        yield return Press(pad.dpad.down);
        Judge("the pad's d-pad moves to North", hand.Item == 1, $"item {hand.Item}");
        yield return Press(pad.buttonSouth);
        yield return ChooseWithPad("lightning.base");
        yield return Press(pad.buttonSouth);
        Judge("North holds Thunderclap", spells.Slots[1] == "lightning.base", spells.Slots[1]);
        Judge("the slots no longer fill themselves", RunState.HandSet);
        // The surge: Overdrive.
        while (hand.Item != HandMenu.SurgeItem) yield return Press(pad.dpad.down);
        yield return Press(pad.buttonSouth);
        yield return ChooseWithPad("lightning.surge");
        yield return Press(pad.buttonSouth);
        Judge("the surge worn is Lightning's", element.SurgeInfo.Name == Elements.Lightning && build.Read().Surge == "lightning.surge", element.SurgeKit.SurgeName);
        yield return Shot("p2_hand_set");
        // A saved Hand: keep this one as Hand 1, change West, wear Hand 1 again.
        while (hand.Item != HandMenu.FirstSavedItem) yield return Press(pad.dpad.down);
        yield return Press(pad.rightTrigger);
        Judge("the Hand is kept as Hand 1", PlayerBuild.SavedHand(0) != null && PlayerBuild.SavedHand(0).Slots[0] == "fire.base" && hand.HandsKept == 1);
        var other = build.Read().Clone();
        other.Slots[0] = "";
        build.Apply(other, true);
        Judge("West emptied", spells.Slots[0] == "");
        yield return Press(pad.buttonSouth);
        Judge("wearing Hand 1 puts Flame Wave back", spells.Slots[0] == "fire.base" && spells.Slots[1] == "lightning.base", $"{spells.Slots[0]}, {spells.Slots[1]}");
        yield return Shot("p2_hand_saved");
        // Back out: the Hand, then the rest menu; the chart is still reached from the rest menu.
        yield return Press(pad.buttonEast);
        yield return Real(0.2f);
        Judge("the dodge button backs out of the Hand to the rest menu", !HandMenu.Showing && RestMenu.Showing && !RestMenu.Instance.Away);
        yield return Tap(Key.S);
        yield return Tap(Key.E);
        yield return Real(0.2f);
        Judge("The chart opens from the rest menu", UpgradeMenu.Instance != null && UpgradeMenu.Instance.IsOpen);
        yield return Shot("p2_rest_chart");
        yield return Tap(Key.Space);
        yield return Real(0.3f);
        Judge("closing the chart comes back to the rest menu, the world still stopped", RestMenu.Showing && !RestMenu.Instance.Away && Time.timeScale == 0f);
        yield return Tap(Key.Space);
        yield return Real(0.3f);
        Judge("leaving the fire gives the world back", !RestMenu.Showing && Time.timeScale > 0f && !player.Suspended, $"time {Time.timeScale}, held {player.Suspended}");
        var away = build.Read().Clone();
        away.Slots[0] = "";
        Judge("away from a fire the Hand is not changed", !build.Apply(away, false) && spells.Slots[0] == "fire.base");

        // 3. Each spell lays its own affliction, in its own colour; the blade takes the landed spell's own element.
        combat.Blade.Clear();
        still.Afflictions.Clear();
        spells.Cooldowns.ResetAll();
        yield return AtDummy(2.2f);
        Judge("West is cast in Fire, North in Lightning", spells.ElementIn(0)?.Name == Elements.Fire && spells.ElementIn(1)?.Name == Elements.Lightning);
        yield return Tap(Key.Digit1);
        yield return Until(() => combat.Blade.Charged, 2.5f);
        yield return Still("p2_cast_fire");
        Judge("Flame Wave burns and charges the blade with Fire", still.Afflictions.Burn.IsBurning && still.Afflictions.Stacks(AfflictionKind.Static) == 0 && combat.Blade.Element == Elements.Fire,
            $"burn {still.Afflictions.Burn.IsBurning}, static {still.Afflictions.Stacks(AfflictionKind.Static)}, blade {combat.Blade.Element}" + Diag());
        yield return WhenFree();
        // Flame Wave leaves ground burning in front of the dummy; it must be out before Thunderclap and the blows are read.
        yield return Clean();
        // Thunderclap lays nothing of its own (GUIDE.md: 16 damage, 45 stagger, there and gone; Static comes from blows).
        int hitsBefore = still.HitsTaken;
        yield return Tap(Key.Digit2);
        yield return Until(() => combat.Blade.Element == Elements.Lightning, 2.5f);
        yield return Still("p2_cast_lightning");
        Judge("Thunderclap strikes the dummy, lays no Burn, and charges the blade with Lightning", still.HitsTaken > hitsBefore && !still.Afflictions.Burn.IsBurning && combat.Blade.Element == Elements.Lightning,
            $"struck {still.HitsTaken - hitsBefore}, burn {still.Afflictions.Burn.IsBurning}, blade {combat.Blade.Element}" + Diag());
        yield return WhenFree();
        // A charged blow carries Lightning beside the root's Fire.
        yield return AtDummy();
        still.Afflictions.Clear();
        int landed = combat.BlowsLanded;
        int flashed = Flashovers();
        yield return Tap(Key.J);
        yield return Until(() => combat.BlowsLanded > landed, 1.5f);
        yield return null;
        yield return null;
        // Since Phase 3, Static laid beside Burn sets off Flashover, which spends the Static: either shows it was laid.
        Judge("a blow charged with Lightning lays Fire (the root) and Static (the blade)", combat.BlowsLanded > landed && still.Afflictions.Burn.IsBurning && (still.Afflictions.Stacks(AfflictionKind.Static) > 0 || FlashedOnStill(flashed)),
            $"burn {still.Afflictions.Burn.IsBurning}, static {still.Afflictions.Stacks(AfflictionKind.Static)}, flashover {Flashovers() - flashed}" + Diag());
        yield return WhenFree();

        // 4. Plain weapon blows lay only Fire's, though Static's root is learned.
        combat.Blade.Clear();
        yield return Clean();
        for (int blow = 1; blow <= 2; blow++)
        {
            yield return AtDummy();
            still.Afflictions.Clear();
            landed = combat.BlowsLanded;
            yield return Tap(Key.J);
            yield return Until(() => combat.BlowsLanded > landed, 1.5f);
            yield return null;
            Judge($"plain blow {blow} lands, lays Burn and no Static", combat.BlowsLanded > landed && still.Afflictions.Burn.IsBurning && still.Afflictions.Stacks(AfflictionKind.Static) == 0,
                $"burn {still.Afflictions.Burn.IsBurning}, static {still.Afflictions.Stacks(AfflictionKind.Static)}" + Diag());
            yield return WhenFree();
        }

        // 5. R1 strikes with Fire, R2 with Lightning (no mana: it is not the deep strike with two elements).
        combat.Blade.Clear();
        spells.Cooldowns.ResetAll();
        combat.Mana.Empty();
        combat.Mana.Gain(50f);
        yield return AtDummy();
        still.Afflictions.Clear();
        int strikes = combat.InfusedStrikes, deep = combat.DeepStrikes;
        int struckBefore = still.HitsTaken;
        yield return PadStrike(pad.rightShoulder);
        yield return Until(() => still.HitsTaken > struckBefore, 1.5f);
        yield return null;
        Judge("L2 + RB is Fire's Burning Edge", combat.InfusedStrikes == strikes + 1 && combat.DeepStrikes == deep && still.Afflictions.Burn.IsBurning, $"burn x{still.Afflictions.Stacks(AfflictionKind.Burn)}" + Diag());
        yield return WhenFree();
        yield return AtDummy();
        still.Afflictions.Clear();
        float paid = spells.StrikeManaPaid;
        strikes = combat.InfusedStrikes;
        flashed = Flashovers();
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow && combat.Combo.Phase == AttackPhase.Active, 1f);
        yield return Still("p2_strike_lightning");
        yield return Real(0.5f);
        Judge("L2 + RT is Lightning's Flash Cut, not the deep strike, and costs nothing", combat.InfusedStrikes == strikes + 1 && combat.DeepStrikes == deep && spells.StrikeManaPaid == paid && (still.Afflictions.Stacks(AfflictionKind.Static) > 0 || FlashedOnStill(flashed)),
            $"static x{still.Afflictions.Stacks(AfflictionKind.Static)}, flashover {Flashovers() - flashed}, paid {spells.StrikeManaPaid - paid:0}");
        Judge("neither strike charged the blade", !combat.Blade.Charged);
        yield return WhenFree();
        Set(pad.leftTrigger, 1f);
        yield return Real(0.4f);
        yield return Shot("p2_diamond_two_elements");
        Set(pad.leftTrigger, 0f);
        yield return Real(0.3f);

        // 6. All in, and one element: R2 is the deep strike again. At the altars: Fire alone, then Fire again goes all in.
        yield return Altar(fireAltar, true);
        Judge("the Fire altar with two carried leaves Fire alone", element.HasElement && !element.Mixed && !element.AllIn && element.Info.Name == Elements.Fire, $"said '{defence.LastCallout}'");
        yield return Altar(fireAltar, false);
        Judge("the Fire altar with Fire alone offers all in", offered == "Carry Fire all in (lays Burn)", $"'{offered}'");
        yield return Altar(fireAltar, true);
        Judge("all in on Fire", element.AllIn && build.Read().AllIn && defence.LastCallout == "Fire all in", $"said '{defence.LastCallout}'");
        // The deep strike asks for Fire's Devotion bought (the owner's answer of 2026-11-20): without it R2 is the plain strike.
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(60f);
        yield return AtDummy();
        deep = combat.DeepStrikes;
        paid = spells.StrikeManaPaid;
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow, 1f);
        yield return WhenFree();
        Judge("all in without the Devotion, L2 + RT is Fire's plain strike for no mana", combat.DeepStrikes == deep && spells.StrikeManaPaid == paid, $"paid {spells.StrikeManaPaid - paid:0}");
        RunState.Unlocked.Add(Clover.Player.Trees.FireTree.FireWithoutEnd);
        combat.GetComponent<PlayerUpgrades>().RefreshNow();
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(60f);
        yield return AtDummy();
        deep = combat.DeepStrikes;
        paid = spells.StrikeManaPaid;
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow, 1f);
        yield return WhenFree();
        Judge("all in, L2 + RT is the deep strike for 20 mana", combat.DeepStrikes == deep + 1 && spells.StrikeManaPaid - paid == 20f, $"paid {spells.StrikeManaPaid - paid:0}");
        Judge("Thunderclap left its slot with Lightning", spells.Slots[1] == "" && element.SurgeInfo.Name == Elements.Fire, $"North '{spells.Slots[1]}', surge {element.SurgeKit.SurgeName}");
        yield return Altar(fireAltar, true);
        Judge("the Fire altar all in starts again with Fire alone", element.HasElement && !element.AllIn && !element.Mixed, $"said '{defence.LastCallout}'");
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(60f);
        yield return AtDummy();
        deep = combat.DeepStrikes;
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow, 1f);
        yield return WhenFree();
        Judge("with one element, L2 + RT is the deep strike", combat.DeepStrikes == deep + 1);

        if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        for (int i = 0; i < RunState.SavedHands.Length; i++) RunState.SavedHands[i] = null;
        RunState.HandSet = false;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
