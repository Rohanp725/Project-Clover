using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Magic;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Phase 1, tasks 1.5 to 1.8, in the Proving Ground with the playtest on: the spell diamond small and large (L2 held),
// its button names on the pad and on the keys and after a rebinding (made in memory, never saved); Fire, then Ice: a
// spell that lands charges the blade, three blows carry the affliction and hit 1.2 times, the third does its extra, the
// fourth is plain; the infused strike (L2 + R1) and the deep strike (L2 + R2) each hit, each has its 8 s clock, the deep
// one takes 20 mana and is refused without it, a strike never charges the blade and its own blow does not cut its own
// clock; the chord cancels a light in its first moment (with a stand-in technique) and does nothing without one;
// Hunger's 25 s wait. Frames: review/shots/current/p1b_*.png. Every judged line ends OK or WRONG.
// bash AgentScripts/runprobe.sh ProvingGround P1bProbe 170
public static class P1bProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("P1b Probe").AddComponent<P1bProbeRunner>();
        return "probe started";
    }
}

public class P1bProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private BladeLook look;
    private XInputControllerWindows pad;
    private TrainingDummy still;
    private bool keepAlive = true;
    private int ok, wrong;

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

    // A picture of one instant: time held still for the frame it is taken in.
    private IEnumerator Still(string name)
    {
        Time.timeScale = 0f;
        yield return Shot(name);
        // Set back to 1, not to what it was: a hit-stop that was running would otherwise keep the stopped time.
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

    // An altar of the Proving Ground sets either attunement (Phase 2): carrying one element, another altar adds its
    // element second and the same altar goes all in; used again it leaves its element alone. This uses it until its
    // element is the only one carried, as a tester would (at most twice).
    private IEnumerator HoldAlone(LoadoutChoice altar)
    {
        for (int use = 0; use < 3; use++)
        {
            if (element.HasElement && element.Info != null && element.Info.Name == altar.Choice && !element.Mixed && !element.AllIn && use > 0) break;
            yield return Use(altar, 1.0f);
        }
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

    // L2 held, then R1 or R2, then both let go: an infused strike from the layer.
    private IEnumerator PadStrike(InputControl attack, Action midway = null)
    {
        Set(pad.leftTrigger, 1f);
        yield return null;
        yield return null;
        Set(attack, 1f);
        yield return null;
        yield return null;
        Set(attack, 0f);
        yield return null;
        midway?.Invoke();
        Set(pad.leftTrigger, 0f);
        yield return null;
        yield return null;
    }

    private void FindPlayer()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        element = player.GetComponent<PlayerElement>();
        upgrades = player.GetComponent<PlayerUpgrades>();
        spells = player.GetComponent<PlayerSpells>();
        look = player.GetComponent<BladeLook>();
    }

    private IEnumerator Use(Interactable thing, float back = 0.8f)
    {
        Vector3 at = thing.transform.position + Vector3.back * back;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
        yield return Real(0.35f);
        yield return Tap(Key.E);
        yield return Real(0.2f);
    }

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private static T Find<T>(Func<T, bool> which) where T : UnityEngine.Object =>
        FindObjectsByType<T>(FindObjectsInactive.Include).FirstOrDefault(which);

    private IEnumerator WhenFree()
    {
        yield return Until(() => !combat.IsAttacking && !element.IsCasting && !player.Dodge.IsRolling && player.FormHold == 0 && !player.IsLeaping && !defence.IsDrinking, 6f);
        yield return Real(0.15f);
    }

    private void StandAtOpenGround() => player.Teleport(new Vector3(0f, 0.05f, -9f), Quaternion.identity);

    // In front of the still dummy, facing it, a step back.
    private IEnumerator AtDummy(float back = 1.7f)
    {
        player.Teleport(still.transform.position + Vector3.back * back, Quaternion.identity);
        yield return WhenFree();
    }

    private Text DiamondText(string path)
    {
        var diamond = SpellDiamondHud.Instance;
        var found = diamond != null ? diamond.transform.Find(path) : null;
        return found != null ? found.GetComponent<Text>() : null;
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        yield return null;
        FindPlayer();
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest mode {PlaytestMode.On}, sandboxed {Progress.Sandboxed}, bindings key {ControlBindings.Key}, owner's bindings saved {PlayerPrefs.HasKey("clover.controls.bindings")}");
        bool ownerBindings = PlayerPrefs.HasKey("clover.controls.bindings");
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        Judge("the diamond is built on the HUD", SpellDiamondHud.Instance != null);
        Judge("the blade's look is on the player", look != null);

        var fire = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Fire);
        yield return HoldAlone(fire);
        var katana = Find<LoadoutChoice>(choice => !choice.IsElement && choice.Choice == Weapons.KatanaName);
        yield return Use(katana, 1.0f);
        Judge("Fire held and the Katana in hand", element.Kit.Name == Elements.Fire && combat.Weapon.Name == Weapons.KatanaName, $"{element.Kit.Name}, {combat.Weapon.Name}");
        still = Find<TrainingDummy>(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the still dummy is found", still != null);

        // 1. The diamond: small at rest, large while L2 is held, the pad's names.
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;
        StandAtOpenGround();
        yield return Real(0.5f);
        var diamond = SpellDiamondHud.Instance;
        Judge("small at rest", diamond != null && !diamond.Large && diamond.Scale < diamond.SmallScale + 0.01f, $"scale {diamond?.Scale:0.00}");
        yield return Shot("p1b_diamond_small");
        Set(pad.leftTrigger, 1f);
        yield return Real(0.4f);
        Judge("large while L2 is held", diamond.Large && diamond.Scale > 0.95f, $"scale {diamond.Scale:0.00}");
        var westKey = DiamondText("Slot West/Key");
        var r1Key = DiamondText("Strike R1/Key");
        Judge("the pad's names: West is X, the strike RB", westKey != null && westKey.text == "X" && r1Key != null && r1Key.text == "RB", $"'{westKey?.text}', '{r1Key?.text}', device {ControlDevices.Current}");
        yield return Shot("p1b_diamond_large");
        Set(pad.leftTrigger, 0f);
        yield return Real(0.4f);
        Judge("small again when let go", !diamond.Large && diamond.Scale < diamond.SmallScale + 0.01f, $"scale {diamond.Scale:0.00}");

        // 2. The charged blade with Fire: the base spell lands on the dummy and charges it.
        combat.Blade.Clear();
        still.Afflictions.Clear();
        yield return AtDummy(2.2f);
        spells.Cooldowns.ResetAll();
        int charges = spells.BladeCharges;
        yield return Tap(Key.Digit1);
        yield return Until(() => combat.Blade.Charged, 2.5f);
        Judge("Flame Wave lands and charges the blade with Fire", combat.Blade.Charged && combat.Blade.Element == Elements.Fire && spells.BladeCharges == charges + 1 && combat.Blade.Left == 3,
            $"charged {combat.Blade.Charged} {combat.Blade.Element} x{combat.Blade.Left}, charges {charges}->{spells.BladeCharges}");
        yield return WhenFree();
        yield return Real(0.3f);
        Judge("the blade shows Fire on the weapon", look.ShownElement == Elements.Fire && look.PiecesHeld > 0, $"{look.ShownElement}, {look.PiecesHeld} pieces");
        yield return Showcase("fire");
        still.Afflictions.Clear();
        yield return BladeBlows(Elements.Fire, AfflictionKind.Burn, "fire");

        // 3. The infused strike (L2 + RB) with Fire: Burning Edge.
        combat.Blade.Clear();
        still.Afflictions.Clear();
        spells.Cooldowns.ResetAll();
        combat.Mana.Empty();
        combat.Mana.Gain(50f);
        yield return AtDummy();
        int strikes = combat.InfusedStrikes, blowsBefore = combat.BlowsLanded, hits = still.HitsTaken;
        float manaBefore = combat.Mana.Current;
        yield return PadStrike(pad.rightShoulder);
        yield return Until(() => combat.InfusedNow && combat.Combo.Phase == AttackPhase.Active, 1f);
        yield return Still("p1b_strike_fire");
        yield return Real(0.35f);
        yield return Shot("p1b_strike_fire_after");
        Judge("L2 + RB plays the infused strike and it lands", combat.InfusedStrikes == strikes + 1 && still.HitsTaken > hits && combat.BlowsLanded > blowsBefore,
            $"strikes {strikes}->{combat.InfusedStrikes}, dummy hits {hits}->{still.HitsTaken}, footprint {combat.LastFootprint.Shape} {combat.LastFootprint.Reach:0.0} m");
        Judge("Burning Edge burns what it reaches (2 Burn and more)", still.Afflictions.Burn.IsBurning && still.Afflictions.Stacks(AfflictionKind.Burn) >= 2, $"burn x{still.Afflictions.Stacks(AfflictionKind.Burn)}");
        Judge("its clock is 8 s, and the strike's own blow did not cut it", Mathf.Abs(spells.Cooldowns.Length(SpellCooldowns.StrikeFirst) - 8f) < 0.01f && spells.Cooldowns.Remaining(SpellCooldowns.StrikeFirst) > 7.3f,
            $"{spells.Cooldowns.Remaining(SpellCooldowns.StrikeFirst):0.00} of {spells.Cooldowns.Length(SpellCooldowns.StrikeFirst):0.00}");
        Judge("it paid no mana and counted as a weapon blow (mana earned)", combat.Mana.Current >= manaBefore, $"mana {manaBefore:0}->{combat.Mana.Current:0}");
        Judge("a strike never charges the blade", !combat.Blade.Charged);
        yield return WhenFree();
        int refused = combat.StrikesRefused;
        strikes = combat.InfusedStrikes;
        yield return PadStrike(pad.rightShoulder);
        yield return Real(0.2f);
        Judge("pressed again while its clock runs it is refused", combat.StrikesRefused == refused + 1 && combat.InfusedStrikes == strikes, defence.LastCallout);
        // Weapon play cuts the strike's clock like a spell's.
        yield return WhenFree();
        float r0 = spells.Cooldowns.Remaining(SpellCooldowns.StrikeFirst), t0 = Time.time;
        int blows = combat.BlowsLanded;
        yield return Tap(Key.J);
        yield return Until(() => combat.BlowsLanded > blows, 1.5f);
        yield return Real(0.05f);
        float extra = (r0 - spells.Cooldowns.Remaining(SpellCooldowns.StrikeFirst)) - (Time.time - t0);
        Judge("a landed light blow cuts the strike's clock 0.3 s", Mathf.Abs(extra - 0.3f) < 0.06f, $"fell {extra:0.00} s more than time");

        // 4. The deep strike (L2 + RT): 20 mana, its own clock, once Fire's Devotion is bought (the owner's answer of
        // 2026-11-20). Without it, R2 is Fire's plain strike and costs nothing.
        yield return WhenFree();
        yield return AtDummy();
        spells.Cooldowns.ResetAll();
        combat.Mana.Empty();
        combat.Mana.Gain(50f);
        int deep = combat.DeepStrikes;
        float paid = spells.StrikeManaPaid;
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow, 1f);
        yield return WhenFree();
        Judge("without Fire's Devotion, L2 + RT is the plain strike and costs nothing", combat.DeepStrikes == deep && spells.StrikeManaPaid == paid,
            $"deep {deep}->{combat.DeepStrikes}, paid {spells.StrikeManaPaid - paid:0}");
        RunState.Unlocked.Add(Clover.Player.Trees.FireTree.FireWithoutEnd);
        RunState.Unlocked.Add(Clover.Player.Trees.IceTree.DeepWinter);
        combat.GetComponent<PlayerUpgrades>().RefreshNow();
        spells.Cooldowns.ResetAll();
        yield return AtDummy();
        combat.Mana.Empty();
        combat.Mana.Gain(50f);
        deep = combat.DeepStrikes;
        paid = spells.StrikeManaPaid;
        hits = still.HitsTaken;
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow && combat.Combo.Phase == AttackPhase.Active, 1f);
        yield return Still("p1b_deep_fire");
        yield return Real(0.4f);
        yield return Shot("p1b_deep_fire_after");
        Judge("L2 + RT is the deep strike, it lands and costs 20 mana", combat.DeepStrikes == deep + 1 && spells.StrikeManaPaid - paid == 20f && still.HitsTaken > hits,
            $"deep {deep}->{combat.DeepStrikes}, paid {spells.StrikeManaPaid - paid:0}, mana now {combat.Mana.Current:0}, dummy last {still.LastDamage:0.0}");
        Judge("the deep strike runs its own 8 s clock", spells.Cooldowns.Remaining(SpellCooldowns.StrikeSecond) > 7f && Mathf.Abs(spells.Cooldowns.Length(SpellCooldowns.StrikeSecond) - 8f) < 0.01f,
            $"{spells.Cooldowns.Remaining(SpellCooldowns.StrikeSecond):0.0}");
        yield return WhenFree();
        spells.Cooldowns.Reset(SpellCooldowns.StrikeSecond);
        combat.Mana.Empty();
        combat.Mana.Gain(10f);
        refused = combat.StrikesRefused;
        deep = combat.DeepStrikes;
        yield return PadStrike(pad.rightTrigger);
        yield return Real(0.2f);
        Judge("with 10 mana the deep strike is refused and nothing is paid", combat.StrikesRefused == refused + 1 && combat.DeepStrikes == deep && spells.StrikeManaPaid == paid + 20f && defence.LastCallout == "No mana", $"'{defence.LastCallout}', mana {combat.Mana.Current:0.0} (settling toward half out of a fight)");

        // 5. The diamond with clocks running, small and large.
        spells.Cooldowns.ResetAll();
        yield return WhenFree();
        yield return Tap(Key.Digit1);
        yield return WhenFree();
        combat.Mana.Gain(60f);
        yield return PadStrike(pad.rightShoulder);
        yield return WhenFree();
        yield return Shot("p1b_diamond_cooldowns_small");
        Set(pad.leftTrigger, 1f);
        yield return Real(0.35f);
        var westClock = diamond.transform.Find("Slot West/Clock")?.GetComponent<Image>();
        var r1Clock = diamond.transform.Find("Strike R1/Clock")?.GetComponent<Image>();
        Judge("the diamond shows West's and the strike's clocks", westClock != null && westClock.fillAmount > 0.05f && r1Clock != null && r1Clock.fillAmount > 0.3f,
            $"West {westClock?.fillAmount:0.00}, R1 {r1Clock?.fillAmount:0.00}, pips shown {diamond.transform.Find("Blade 1")?.gameObject.activeSelf}");
        yield return Shot("p1b_diamond_cooldowns_large");
        Set(pad.leftTrigger, 0f);
        yield return Real(0.3f);

        // 6. The chord: with a stand-in technique a heavy close behind a light cancels it; with none it is only a heavy.
        yield return WhenFree();
        StandAtOpenGround();
        yield return WhenFree();
        var technique = combat.Weapon.Combo.Heavy.Clone();
        combat.TechniqueStep = technique;
        int chords = combat.ChordsTaken;
        Keys(Key.J);
        yield return null;
        bool lightAtOnce = combat.Combo.LightWindingUp;
        Keys(Key.J, Key.K);
        yield return null;
        yield return null;
        Keys();
        Judge("the light starts on its own frame, not held back for a chord", lightAtOnce, $"winding up {lightAtOnce}");
        Judge("a heavy within 0.08 s cancels the light into the technique", combat.ChordsTaken == chords + 1 && combat.Combo.Current == technique, $"chords {chords}->{combat.ChordsTaken}, now {combat.Combo.Current?.Name}");
        yield return WhenFree();
        combat.TechniqueStep = null;
        chords = combat.ChordsTaken;
        Keys(Key.J);
        yield return null;
        Keys(Key.J, Key.K);
        yield return null;
        yield return null;
        Keys();
        yield return null;
        Judge("with no technique (as now) the chord is not taken and the light goes on", combat.ChordsTaken == chords && combat.Combo.Current != null && !combat.Combo.Current.Heavy, $"now {combat.Combo.Current?.Name}");
        yield return WhenFree();

        // 7. Ice: its altar, the charged blade, the strike and the deep strike.
        var ice = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Ice);
        yield return HoldAlone(ice);
        Judge("Ice held", element.Kit.Name == Elements.Ice, element.Kit.Name);
        combat.Blade.Clear();
        still.Afflictions.Clear();
        spells.Cooldowns.ResetAll();
        yield return AtDummy(2.2f);
        charges = spells.BladeCharges;
        // Taken through Fire and Ice (Phase 2's altars), Ice's own spell keeps the slot it had as the second: cast that one.
        int iceSlot = Mathf.Max(0, spells.Slots.SlotOf("ice.base"));
        Say($"  Ice's own spell sits in slot {iceSlot + 1} ({spells.Slots[0]}, {spells.Slots[1]}, {spells.Slots[2]}, {spells.Slots[3]})");
        yield return Tap((Key)((int)Key.Digit1 + iceSlot));
        yield return Until(() => combat.Blade.Charged, 2.5f);
        Judge("Ice's spell lands and charges the blade with Ice", combat.Blade.Charged && combat.Blade.Element == Elements.Ice && spells.BladeCharges == charges + 1, $"{combat.Blade.Element} x{combat.Blade.Left}");
        yield return WhenFree();
        yield return Real(0.3f);
        Judge("the blade shows Ice on the weapon", look.ShownElement == Elements.Ice && look.PiecesHeld > 0, $"{look.ShownElement}, {look.PiecesHeld} pieces");
        yield return Showcase("ice");
        still.Afflictions.Clear();
        yield return BladeBlows(Elements.Ice, AfflictionKind.Chill, "ice");

        combat.Blade.Clear();
        still.Afflictions.Clear();
        spells.Cooldowns.ResetAll();
        yield return AtDummy();
        strikes = combat.InfusedStrikes;
        hits = still.HitsTaken;
        yield return PadStrike(pad.rightShoulder);
        yield return Until(() => combat.InfusedNow && combat.Combo.Phase == AttackPhase.Active, 1f);
        yield return Real(0.2f);
        Judge("Frost Bite lands: 2 Chill and held", combat.InfusedStrikes == strikes + 1 && still.Afflictions.Stacks(AfflictionKind.Chill) >= 2,
            $"chill x{still.Afflictions.Stacks(AfflictionKind.Chill)}, held {still.Afflictions.HeldLeft:0.00}s, frozen {still.Afflictions.IsFrozen}");
        yield return Still("p1b_strike_ice");
        yield return WhenFree();
        combat.Mana.Gain(60f);
        still.Afflictions.Apply(AfflictionKind.Chill, 3);
        deep = combat.DeepStrikes;
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow && combat.Combo.Phase == AttackPhase.Active, 1f);
        yield return Real(0.25f);
        Judge("Ice Through freezes an enemy already three deep", combat.DeepStrikes == deep + 1 && still.Afflictions.IsFrozen, $"frozen {still.Afflictions.IsFrozen}, chill x{still.Afflictions.Stacks(AfflictionKind.Chill)}");
        yield return Still("p1b_deep_ice");
        yield return WhenFree();

        // 8. Hunger's wait: Dark's surge, lit, run out, then refused for 25 s; the diamond's middle shades.
        var dark = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Dark);
        yield return HoldAlone(dark);
        StandAtOpenGround();
        yield return WhenFree();
        int surges = element.Surges;
        yield return Tap(Key.F);
        yield return Real(0.2f);
        Judge("Hunger is lit", element.Surges == surges + 1 && element.Kit.SurgeActive, element.Kit.SurgeName);
        yield return Until(() => !element.Kit.SurgeActive, element.Kit.SurgeSeconds + 3f);
        Judge("when it ends it waits 25 s", element.Kit.SurgeWaiting && element.Kit.SurgeWaitLeft > 23f, $"{element.Kit.SurgeWaitLeft:0.0} s");
        surges = element.Surges;
        yield return Tap(Key.F);
        yield return Real(0.2f);
        Judge("pressed while it waits it is not lit", element.Surges == surges && !element.Kit.SurgeActive, $"'{defence.LastCallout}'");
        yield return Shot("p1b_diamond_hunger_wait");

        // 9. The keys' names, and a rebinding shown at once (made in memory, never saved).
        Keys(Key.W);
        yield return Real(0.2f);
        Keys();
        yield return Real(0.2f);
        westKey = DiamondText("Slot West/Key");
        r1Key = DiamondText("Strike R1/Key");
        Judge("the keys' names: West is 1, the strike Ctrl+LMB", westKey != null && westKey.text == "1" && r1Key != null && r1Key.text == "Ctrl+LMB", $"'{westKey?.text}', '{r1Key?.text}', device {ControlDevices.Current}");
        yield return Shot("p1b_diamond_keys");
        var west = player.Controls.FindAction("SpellWest");
        int westIndex = ControlBindings.IndexOf(west, ControlBindings.KeysGroup);
        west.ApplyBindingOverride(westIndex, "<Keyboard>/z");
        ControlDevices.BindingsChanged();
        yield return Real(0.2f);
        Judge("after rebinding West to Z the diamond says Z", DiamondText("Slot West/Key")?.text == "Z", DiamondText("Slot West/Key")?.text);
        west.RemoveAllBindingOverrides();
        ControlDevices.BindingsChanged();
        yield return Real(0.2f);
        Judge("and 1 again when it is undone", DiamondText("Slot West/Key")?.text == "1", DiamondText("Slot West/Key")?.text);

        Judge("the owner's bindings key is as it was", PlayerPrefs.HasKey("clover.controls.bindings") == ownerBindings && !PlayerPrefs.HasKey(ControlBindings.SandboxKey));
        if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }

    // The charged weapon on open ground, the camera brought in: at rest (the Katana in its sheath) and mid-swing at the
    // air (nothing struck, so nothing is spent). The spell's own flames are left behind at the dummy.
    private IEnumerator Showcase(string tag)
    {
        StandAtOpenGround();
        yield return Real(0.8f);
        var camera = FindAnyObjectByType<TopDownCamera>();
        if (camera != null) camera.CloseIn(0.45f, Vector3.zero, 3f);
        yield return Real(0.5f);
        yield return Still($"p1b_blade_{tag}");
        int left = combat.Blade.Left;
        Keys(Key.J);
        yield return null;
        yield return null;
        Keys();
        yield return Until(() => combat.Combo.Phase == AttackPhase.Active || (combat.Combo.Phase == AttackPhase.Windup && combat.Combo.Elapsed > 0.08f), 0.6f);
        yield return Still($"p1b_blade_{tag}_swing");
        yield return WhenFree();
        Judge($"a swing at the air spends no charge ({tag})", combat.Blade.Left == left, $"{left}->{combat.Blade.Left}");
    }

    // Three charged blows at the still dummy, each 1.2 times its move and laying the element; the fourth plain.
    private IEnumerator BladeBlows(string elementName, AfflictionKind kind, string tag)
    {
        for (int blow = 1; blow <= 4; blow++)
        {
            yield return AtDummy();
            still.Afflictions.Clear();
            int before = still.Afflictions.Stacks(kind);
            bool frozenBefore = still.Afflictions.IsFrozen;
            int landed = combat.BlowsLanded, charged = combat.ChargedBlows, left = combat.Blade.Left;
            Keys(Key.J);
            yield return null;
            yield return null;
            Keys();
            yield return Until(() => combat.BlowsLanded > landed, 1.5f);
            yield return null;
            var step = combat.Combo.Current;
            float expected = step != null ? step.Damage * (blow <= 3 ? 1.2f : 1f) : -1f;
            int after = still.Afflictions.Stacks(kind);
            bool laid = after > before || (kind == AfflictionKind.Chill && still.Afflictions.IsFrozen && !frozenBefore);
            if (blow <= 3)
                Judge($"{elementName} charged blow {blow}: 1.2 times and lays {kind}", combat.ChargedBlows == charged + 1 && Mathf.Abs(combat.LastBlowDamage - expected) < 0.05f && laid,
                    $"{step?.Name} {combat.LastBlowDamage:0.00} (expected {expected:0.00}), {kind} {before}->{after}, charge {left}->{combat.Blade.Left}");
            else
                Judge($"{elementName}: the fourth blow is plain", combat.ChargedBlows == charged && Mathf.Abs(combat.LastBlowDamage - expected) < 0.05f && !laid && !combat.Blade.Charged,
                    $"{step?.Name} {combat.LastBlowDamage:0.00} (expected {expected:0.00}), {kind} {before}->{after}");
            if (blow == 3)
            {
                yield return Real(0.15f);
                yield return Still($"p1b_blade_{tag}_third");
                Judge($"{elementName}: the blade's look is gone after the third", look.ShownElement == "", look.ShownElement);
                // The third's fire on the ground burns out before the plain blow is judged.
                yield return Real(3.4f);
            }
            yield return WhenFree();
            yield return Real(0.4f);
        }
    }
}
