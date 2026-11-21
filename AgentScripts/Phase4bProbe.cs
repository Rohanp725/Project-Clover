using System;
using System.Collections;
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
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;

// The Phase 4b pass (the owner's answers of 2026-11-20), in the Proving Ground with the playtest on, on a copy of the still
// dummy: the six new keystones (Into the Fire, The Blade Drinks Fire, High Water, Low Water, Drawn Back to Break, Shared
// Roots, In Its Season), a reaction's own numbers changed by its minor (Deep Swell), a duo surge worn in the surge slot
// (Ash and Thunder), Three Weasels' three presses and Under the Black Water's change of places. Each judged line ends OK or
// WRONG. Progress stays sandboxed.
//   bash AgentScripts/runprobe.sh ProvingGround Phase4bProbe 120
public static class Phase4bProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Phase4b Probe").AddComponent<Phase4bProbeRunner>();
        return "probe started";
    }
}

public class Phase4bProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerDefence defence;
    private PlayerCombat combat;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerBuild build;
    private PlayerReactions reactions;
    private TrainingDummy still, host;
    private XInputController pad;
    private int ok, wrong;
    private bool playtestWas;
    private InputSettings.BackgroundBehavior backgroundWas;
    private InputSettings.EditorInputBehaviorInPlayMode editorWas;
    private static readonly Vector3 Middle = new Vector3(25f, 0f, 6f);
    private readonly System.Collections.Generic.List<string> given = new System.Collections.Generic.List<string>();

    private static void Say(string text) => Debug.Log("[Probe] " + text);

    private void Judge(string what, bool good, string detail = "")
    {
        if (good) ok++; else wrong++;
        Say($"{what}{(string.IsNullOrEmpty(detail) ? "" : ": " + detail)} {(good ? "OK" : "WRONG")}");
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

    private void Set(InputControl control, float value)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            ((InputControl<float>)control).WriteValueIntoEvent(value, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    private void Stick(Vector2 way)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            pad.leftStick.WriteValueIntoEvent(way, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    private IEnumerator Press(InputControl button)
    {
        Set(button, 1f);
        yield return null;
        yield return null;
        Set(button, 0f);
        yield return null;
    }

    // A dodge the way the stick points, held through the roll.
    private IEnumerator Dodge(Vector2 way)
    {
        Stick(way);
        yield return null;
        yield return Press(pad.buttonEast);
        yield return Until(() => !player.Dodge.IsRolling, 1.5f);
        Stick(Vector2.zero);
        yield return Real(0.2f);
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while ((ReadingBox.Instance != null && ReadingBox.Instance.IsOpen || GuidePage.Showing) && guard++ < 60)
        {
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.E));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
            yield return null;
            yield return null;
        }
    }

    // A fresh copy of the still dummy at the middle, and the player a few metres south of it facing it.
    private IEnumerator Fresh(float gap)
    {
        if (host != null) Destroy(host.gameObject);
        yield return null;
        host = Instantiate(still.gameObject, Middle, still.transform.rotation).GetComponent<TrainingDummy>();
        host.name = "Probe Host";
        yield return Until(() => Hazards.Alive == 0, 3f);
        player.Teleport(Middle + Vector3.back * gap, Quaternion.identity);
        defence.Restore();
        player.Stamina.Fill();
        yield return Real(0.3f);
    }

    private void Give(string id)
    {
        upgrades.Give(Upgrade.ById(id));
        given.Add(id);
        upgrades.RefreshNow();
    }

    private void Carry(string first, string second)
    {
        build.Attune(first, second);
        upgrades.RefreshNow();
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
        defence = player.GetComponent<PlayerDefence>();
        combat = player.GetComponent<PlayerCombat>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        spells = player.GetComponent<PlayerSpells>();
        build = player.GetComponent<PlayerBuild>();
        reactions = player.GetComponent<PlayerReactions>();
        Judge("progress is sandboxed", Progress.Sandboxed);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        still = FindObjectsByType<TrainingDummy>(FindObjectsInactive.Exclude).FirstOrDefault(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.transform.position.x < 25f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found to copy", still != null);
        if (still == null) { Finish(); yield break; }
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;

        // ---- Into the Fire: toward the enemy, through it and setting it burning; away, twice the stamina.
        Carry(Elements.Fire, "");
        Give(FireTree.IntoTheFire);
        yield return Fresh(2.5f);
        float startZ = player.transform.position.z;
        yield return Dodge(Vector2.up);
        Judge("Into the Fire: a dodge toward the enemy is told as toward", upgrades.LastDodgeWay == DodgeWay.Toward, $"{upgrades.LastDodgeWay}");
        Judge("it went through the enemy's body and came out beyond it", player.transform.position.z > host.transform.position.z + 0.3f,
            $"from z {startZ:0.0} to {player.transform.position.z:0.0}, the enemy at {host.transform.position.z:0.0}");
        Judge("and everything it passed burns two deep", host.Afflictions.Stacks(AfflictionKind.Burn) >= 2, $"Burn x{host.Afflictions.Stacks(AfflictionKind.Burn)}");
        yield return Fresh(2.5f);
        player.Stamina.Fill();
        float before = player.Stamina.Current;
        Stick(Vector2.down);
        yield return null;
        yield return Press(pad.buttonEast);
        float spent = before - player.Stamina.Current;
        yield return Until(() => !player.Dodge.IsRolling, 1.5f);
        Stick(Vector2.zero);
        Judge("a dodge away costs twice the stamina", Mathf.Abs(spent - player.Dodge.StaminaCost * 2f) < 2f, $"spent {spent:0.0}, a dodge costs {player.Dodge.StaminaCost:0.0}");
        Judge("and is never perfect", upgrades.LastDodgeWay == DodgeWay.Away);

        // ---- The Blade Drinks Fire: the blow drinks a Burn and strikes harder; burns do not bite; the root lays nothing.
        Give(FireTree.BladeDrinksFire);
        yield return Fresh(1.6f);
        Judge("The Blade Drinks Fire: Cinder lays nothing with the weapon", combat.WeaponAfflictionStacks == 0, $"stacks {combat.WeaponAfflictionStacks}");
        Judge("burns no longer bite by themselves", Afflictions.BurnBiteOff);
        host.Afflictions.Lay(AfflictionKind.Burn, 3, 0f, 1f, LaySource.Spell, host);
        float healthBefore = host.Health.Current;
        yield return Real(1.2f);
        Judge("a burning enemy left alone loses nothing", Mathf.Abs(host.Health.Current - healthBefore) < 0.01f, $"lost {healthBefore - host.Health.Current:0.0}");
        int drunk = combat.BurnsDrunk;
        yield return Press(pad.rightShoulder);
        yield return Real(0.6f);
        Judge("a light blow drinks one Burn", combat.BurnsDrunk == drunk + 1 && host.Afflictions.Stacks(AfflictionKind.Burn) == 2,
            $"drunk {combat.BurnsDrunk - drunk}, Burn x{host.Afflictions.Stacks(AfflictionKind.Burn)}, blow {combat.LastBlowDamage:0.0}");

        // ---- High Water, Low Water: the tide turns every six seconds.
        Carry(Elements.Water, "");
        Give(WaterTree.HighWaterLowWater);
        yield return Real(0.3f);
        Judge("High Water, Low Water: the tide starts high", upgrades.KeepsTide && upgrades.TideHigh);
        Judge("at high water spells a third harder and the weapon a fifth softer",
            Mathf.Abs(upgrades.Stats.Multiplier(PlayerStat.SpellDamage) - 1.33f) < 0.01f && Mathf.Abs(upgrades.Stats.Multiplier(PlayerStat.WeaponDamage) - 0.8f) < 0.01f,
            $"spells x{upgrades.Stats.Multiplier(PlayerStat.SpellDamage):0.00}, weapon x{upgrades.Stats.Multiplier(PlayerStat.WeaponDamage):0.00}");
        yield return Until(() => !upgrades.TideHigh, 7f);
        Judge("six seconds on it turns low: the weapon a third harder", !upgrades.TideHigh && Mathf.Abs(upgrades.Stats.Multiplier(PlayerStat.WeaponDamage) - 1.33f) < 0.01f,
            $"weapon x{upgrades.Stats.Multiplier(PlayerStat.WeaponDamage):0.00}");
        RunState.Unlocked.Remove(WaterTree.HighWaterLowWater);
        upgrades.RefreshNow();

        // ---- Drawn Back to Break: a wave for each dodge away, all broken by the next blow.
        Give(WaterTree.DrawnBackToBreak);
        yield return Fresh(2.5f);
        Judge("Drawn Back to Break: with no wave, the weapon a fifth softer", Mathf.Abs(upgrades.Stats.Multiplier(PlayerStat.WeaponDamage) - 0.8f) < 0.01f,
            $"weapon x{upgrades.Stats.Multiplier(PlayerStat.WeaponDamage):0.00}");
        yield return Dodge(Vector2.down);
        player.Stamina.Fill();
        yield return Real(0.3f);
        // The roll turns the player south, where the afflicted dummies stand 4 m off and would be the enemy faced: face the
        // host again from where the first dodge began, as a player turning back to it would.
        player.Teleport(Middle + Vector3.back * 2.5f, Quaternion.identity);
        yield return Real(0.2f);
        yield return Dodge(Vector2.down);
        Judge("two dodges away draw two waves", upgrades.Tally(WaterTree.DrawnBackToBreak) == 2, $"waves {upgrades.Tally(WaterTree.DrawnBackToBreak)}");
        player.Teleport(host.transform.position + Vector3.back * 1.6f, Quaternion.identity);
        yield return Real(0.2f);
        Vector3 was = host.transform.position;
        yield return Press(pad.rightShoulder);
        yield return Real(0.6f);
        Judge("the next blow breaks them all: the waves are spent, the enemy Soaked and thrown", upgrades.Tally(WaterTree.DrawnBackToBreak) == 0 && host.Afflictions.Has(AfflictionKind.Soaked),
            $"waves {upgrades.Tally(WaterTree.DrawnBackToBreak)}, Soaked {host.Afflictions.Has(AfflictionKind.Soaked)}, moved {(host.transform.position - was).magnitude:0.0} m");

        // ---- Shared Roots: a third of a wound is carried by the Seeded near.
        Carry(Elements.Life, "");
        Give(LifeTree.SharedRoots);
        yield return Fresh(2f);
        host.Afflictions.Lay(AfflictionKind.Seeded, 1, 0f, 1f, LaySource.Spell, host);
        yield return Real(0.2f);
        float mine = defence.Health.Current, theirs = host.Health.Current;
        defence.TakeHit(new Hit(30f, 0f, Vector3.forward, false, false, host.transform.position));
        yield return Real(0.3f);
        Judge("Shared Roots: you take two thirds of the wound", Mathf.Abs((mine - defence.Health.Current) - 20f) < 1f, $"took {mine - defence.Health.Current:0.0} of 30");
        Judge("and the Seeded enemy near takes the rest", Mathf.Abs((theirs - host.Health.Current) - 10f) < 1f, $"it took {theirs - host.Health.Current:0.0}");

        // ---- In Its Season: a seed left alone six seconds ripens and stops fading.
        Give(LifeTree.InItsSeason);
        yield return Fresh(4f);
        host.Afflictions.Lay(AfflictionKind.Seeded, 2, 0f, 1f, LaySource.Spell, host);
        yield return Real(6.4f);
        Judge("In Its Season: six seconds alone, the seed is ripe", host.Afflictions.IsRipe(AfflictionKind.Seeded) && Mathf.Approximately(host.Afflictions.RipeShare(AfflictionKind.Seeded), 3f));
        yield return Real(3f);
        Judge("and stops fading (Seeded lasts 8 s)", host.Afflictions.Stacks(AfflictionKind.Seeded) == 2, $"Seeded x{host.Afflictions.Stacks(AfflictionKind.Seeded)}");

        // ---- A reaction's own numbers: Deep Swell keeps Drowned Weight's swelling 7 s, not 5.
        Carry(Elements.Water, Elements.Dark);
        Give(DrownedWeightDuo.DeepSwell);
        yield return Fresh(3f);
        int drowned = AfflictionPairs.Index(AfflictionKind.Soaked, AfflictionKind.Cursed);
        int shown = reactions.TimesShown(drowned);
        host.Afflictions.Lay(AfflictionKind.Soaked, 1, 0f, 1f, LaySource.Spell, host);
        yield return null;
        host.Afflictions.Lay(AfflictionKind.Cursed, 1, 0f, 1f, LaySource.Spell, host);
        yield return Until(() => reactions.TimesShown(drowned) > shown, 1.5f);
        yield return Real(0.3f);
        Judge("Deep Swell: Drowned Weight's swelling is set for 7 s", host.Afflictions.Conditions.Left("drownedweight.swollen") > 6f, $"left {host.Afflictions.Conditions.Left("drownedweight.swollen"):0.0} s");

        // ---- Under the Black Water: you and the enemy change places.
        Give(DrownedWeightDuo.UnderTheBlackWater);
        spells.SetSlot(0, DrownedWeightDuo.UnderTheBlackWater);
        upgrades.RefreshNow();
        yield return Fresh(5f);
        spells.Cooldowns.ResetAll();
        Vector3 myStart = player.transform.position, itsStart = host.transform.position;
        int swapped = upgrades.PlacesSwapped;
        Judge("Under the Black Water is cast", spells.TryCast(0));
        yield return Real(1f);
        Judge("you and it change places", upgrades.PlacesSwapped == swapped + 1 && (host.transform.position - myStart).magnitude < 1.5f && (player.transform.position - itsStart).magnitude < 1.5f,
            $"it {(host.transform.position - myStart).magnitude:0.0} m from where you stood, you {(player.transform.position - itsStart).magnitude:0.0} m from where it stood");

        // ---- Three Weasels: three presses of one slot.
        Carry(Elements.Wind, Elements.Metal);
        Give(SickleWindDuo.ThreeWeasels);
        spells.SetSlot(0, SickleWindDuo.ThreeWeasels);
        upgrades.RefreshNow();
        yield return Fresh(4f);
        spells.Cooldowns.ResetAll();
        int presses = upgrades.PressesDone;
        float weaselStart = player.transform.position.z;
        Judge("Three Weasels' first press is cast", spells.TryCast(0));
        yield return Real(0.7f);
        float afterRush = player.transform.position.z, hostAfterRush = host.transform.position.z;
        Judge("the second press passes through", spells.TryCast(0) && upgrades.PressesDone == presses + 1);
        yield return Real(0.5f);
        // The first weasel is the step form, which already steps through the enemy, so the second comes back through it:
        // judged as the player ending on the other side of it from where the rush left them.
        Judge("and comes out beyond the enemy", (afterRush - hostAfterRush) * (player.transform.position.z - host.transform.position.z) < 0f,
            $"you z {weaselStart:0.0} -> {afterRush:0.0} (rush) -> {player.transform.position.z:0.0}, it z {hostAfterRush:0.0} -> {host.transform.position.z:0.0}");
        Judge("the third press steps back", spells.TryCast(0) && upgrades.PressesDone == presses + 2);
        yield return Real(0.5f);
        Judge("a fourth press is a new cast, refused while the clock runs", !spells.TryCast(0));

        // ---- A duo surge in the surge slot: Ash and Thunder for its own 40 mana.
        Carry(Elements.Dark, Elements.Lightning);
        Give(TheGrudgeDuo.AshAndThunder);
        element.WearDuoSurge(Upgrade.ById(TheGrudgeDuo.AshAndThunder));
        upgrades.RefreshNow();
        yield return Real(0.3f);
        Judge("Ash and Thunder is the surge worn", element.SurgeKit.SurgeName == "Ash and Thunder" && build.Read().Surge == TheGrudgeDuo.AshAndThunder, $"'{element.SurgeKit.SurgeName}', '{build.Read().Surge}'");
        Judge("and its mark is in force only now", upgrades.Has(TheGrudgeDuo.AshAndThunder));
        combat.Mana.Gain(100f);
        float mana = combat.Mana.Current;
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.F));
        yield return null;
        yield return null;
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        yield return Real(0.3f);
        Judge("lit for its own 40 mana", element.SurgeActive && Mathf.Abs(mana - combat.Mana.Current - 40f) < 1f, $"paid {mana - combat.Mana.Current:0.0}");
        Finish();
    }

    private void Finish()
    {
        if (host != null) Destroy(host.gameObject);
        foreach (string id in given) RunState.Unlocked.Remove(id);
        if (spells != null) spells.SetSlot(0, "");
        if (build != null) build.Attune(Elements.Fire, "");
        if (upgrades != null) upgrades.RefreshNow();
        if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        PlaytestMode.Set(playtestWas);
        InputSystem.settings.backgroundBehavior = backgroundWas;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorWas;
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }
}
