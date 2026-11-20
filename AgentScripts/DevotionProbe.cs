using System;
using System.Collections;
using System.Linq;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Growth;
using Clover.Player.Magic;
using Clover.Player.Trees;
using Clover.UI;
using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;

// Phase 4, task 4.4, in the Proving Ground with the playtest on: all in on Fire, then on Lightning, each Devotion given
// (bought in the game) and read through the RuleBook. Fire Without End: out of force before it is owned and in a mix;
// all in, a burn laid once on the still dummy never goes out and deepens by one every 4 s to five while the player
// stands off; L2 with R2 is Burnt Offering for 20 mana. In One Breath: a perfect guard and a perfect dodge each give a
// 2.5 s breath in which a waiting spell is ready, a spell cast in it starts no clock until the breath ends and then its
// whole wait, the L2 strikes keep their clocks; L2 with R2 is Down the Channel for 20 mana. The player is put back to Fire
// alone at the end. Frames: review/shots/current/devotion_*.png. Every judged line ends OK or WRONG.
// bash AgentScripts/runprobe.sh ProvingGround DevotionProbe 120
public static class DevotionProbe
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Devotion Probe").AddComponent<DevotionProbeRunner>();
        return "probe started";
    }
}

public class DevotionProbeRunner : MonoBehaviour
{
    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerElement element;
    private PlayerUpgrades upgrades;
    private PlayerSpells spells;
    private PlayerBuild build;
    private LoadoutChoice fireAltar, lightningAltar;
    private TrainingDummy still;
    private XInputControllerWindows pad;
    private int ok, wrong;

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

    private void Set(InputControl control, float value)
    {
        using (StateEvent.From(pad, out InputEventPtr eventPtr))
        {
            ((InputControl<float>)control).WriteValueIntoEvent(value, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
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

    private IEnumerator ReadAll()
    {
        int guard = 0;
        while (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen && guard++ < 40) yield return Tap(Key.E);
        guard = 0;
        while (GuidePage.Showing && guard++ < 40) yield return Tap(Key.E);
    }

    private static T Find<T>(Func<T, bool> which) where T : UnityEngine.Object =>
        FindObjectsByType<T>(FindObjectsInactive.Include).FirstOrDefault(which);

    private IEnumerator Use(Interactable thing, float back = 1.0f)
    {
        Vector3 at = thing.transform.position + Vector3.back * back;
        at.y = 0.05f;
        player.Teleport(at, Quaternion.identity);
        yield return Real(0.35f);
        yield return Tap(Key.E);
        yield return Real(0.3f);
    }

    private IEnumerator WhenFree()
    {
        yield return Until(() => !combat.IsAttacking && !element.IsCasting && !player.Dodge.IsRolling && player.FormHold == 0 && !player.IsLeaping && !defence.IsDrinking, 6f);
        yield return Real(0.15f);
    }

    private IEnumerator AtDummy(float back = 1.7f)
    {
        player.Teleport(still.transform.position + Vector3.back * back, Quaternion.identity);
        yield return WhenFree();
    }

    private bool AllInOn(string name) => element.HasElement && element.AllIn && element.Info != null && element.Info.Name == name;
    private bool AloneOn(string name) => element.HasElement && !element.AllIn && !element.Mixed && element.Info != null && element.Info.Name == name;
    private string Carried() => element.HasElement ? element.Info.Name + (element.SecondInfo != null ? " + " + element.SecondInfo.Name : "") : "nothing";

    // An altar used until its element is carried all in, as a tester would (alone, then all in; at most five uses).
    private IEnumerator AllIn(LoadoutChoice altar)
    {
        for (int use = 0; use < 5 && !AllInOn(altar.Choice); use++) yield return Use(altar);
    }

    private IEnumerator Alone(LoadoutChoice altar)
    {
        for (int use = 0; use < 5 && !AloneOn(altar.Choice); use++) yield return Use(altar);
    }

    private string Book() => $"rules: standard {upgrades.Rules.IsStandard}, keeps Burn {upgrades.Rules.Afflictions.Keeps(AfflictionKind.Burn)}, breath {upgrades.Rules.BreathAfterPerfect:0.0}s, enemies keep Burn {Afflictions.Rules.Keeps(AfflictionKind.Burn)}";

    // A blow from in front while the guard was pressed this frame: a perfect guard.
    private IEnumerator PerfectGuard()
    {
        Keys(Key.LeftShift);
        yield return null;
        yield return null;
        lastOutcome = defence.TakeHit(new Hit(10f, 10f, -player.transform.forward, false, false, player.transform.position + player.transform.forward * 1.5f));
        yield return null;
        Keys();
        yield return Real(0.1f);
    }

    // A blow in the roll's first moment: a perfect dodge.
    private IEnumerator PerfectDodge()
    {
        Keys(Key.Space);
        yield return Until(() => player.Dodge.IsInvulnerable, 0.5f);
        lastOutcome = defence.TakeHit(new Hit(10f, 10f, -player.transform.forward, false, false, player.transform.position + player.transform.forward * 1.5f));
        Keys();
        yield return Real(0.1f);
    }

    private HitOutcome lastOutcome;

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
        Say($"start: scene {SceneManager.GetActiveScene().name}, playtest mode {PlaytestMode.On}, sandboxed {Progress.Sandboxed}, carried {Carried()}");
        Judge("progress is sandboxed (nothing saved is touched)", Progress.Sandboxed);
        yield return Until(() => GuidePage.Showing, 4f);
        yield return ReadAll();
        fireAltar = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Fire);
        lightningAltar = Find<LoadoutChoice>(choice => choice.IsElement && choice.Choice == Elements.Lightning);
        var katana = Find<LoadoutChoice>(choice => !choice.IsElement && choice.Choice == Weapons.KatanaName);
        yield return Use(katana);
        still = Find<TrainingDummy>(dummy => dummy.name == "Training Dummy" && dummy.transform.position.x > 15f && dummy.GetComponent<AfflictedDummy>() == null);
        Judge("the altars, the still dummy and the build are found", fireAltar != null && lightningAltar != null && still != null && build != null);
        pad = InputSystem.AddDevice<XInputControllerWindows>();
        yield return null;

        yield return Fire();
        yield return Lightning();

        yield return Alone(fireAltar);
        Judge("put back to Fire alone", AloneOn(Elements.Fire), Carried());
        if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
        Say($"tally: {ok} OK, {wrong} WRONG");
        Say("done");
        Destroy(gameObject);
    }

    // ---- All in on Fire: Fire Without End, Burnt Offering.
    private IEnumerator Fire()
    {
        Say("--- all in on Fire");
        yield return AllIn(fireAltar);
        Judge("all in on Fire", AllInOn(Elements.Fire) && build.Read().AllIn, Carried());
        Judge("before it is owned, Fire Without End is out of force and the book is standard", !upgrades.Has(FireTree.FireWithoutEnd) && upgrades.Rules.IsStandard, Book());

        upgrades.Give(Upgrade.ById(FireTree.FireWithoutEnd));
        upgrades.RefreshNow();
        yield return null;
        Judge("owned and all in, it writes into the book and the enemies read it", upgrades.Has(FireTree.FireWithoutEnd) && upgrades.Rules.Afflictions.Keeps(AfflictionKind.Burn)
            && Afflictions.Rules.Keeps(AfflictionKind.Burn) && Mathf.Approximately(Afflictions.Rules.DeepensEvery(AfflictionKind.Burn), 4f) && Afflictions.Rules.DeepensTo(AfflictionKind.Burn) == 5, Book());

        // A burn laid once, then the player stands well off so nothing more is laid.
        still.Afflictions.Clear();
        still.Afflictions.Lay(AfflictionKind.Burn, 1, 0f, 1f, LaySource.Spell, still);
        player.Teleport(still.transform.position + Vector3.back * 9f, Quaternion.identity);
        yield return Real(9.5f);
        Judge("left alone 9.5 s, the burn still burns and has deepened twice", still.Afflictions.Burn.IsBurning && still.Afflictions.Burn.Stacks >= 3,
            $"burning {still.Afflictions.Burn.IsBurning}, {still.Afflictions.Burn.SecondsLeft:0.0}s left, depth {still.Afflictions.Burn.Stacks}");
        yield return Real(8.5f);
        Judge("after 18 s it burns on at five", still.Afflictions.Burn.IsBurning && still.Afflictions.Burn.Stacks == 5, $"depth {still.Afflictions.Burn.Stacks}");
        yield return AtDummy(3.5f);
        yield return Shot("devotion_fire_burning");

        // L2 with R2, all in: Burnt Offering for 20 mana.
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(100f);
        yield return AtDummy();
        int deep = combat.DeepStrikes;
        float paid = spells.StrikeManaPaid;
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow, 1f);
        yield return Real(0.25f);
        yield return Shot("devotion_fire_deep");
        yield return WhenFree();
        Judge("all in, L2 + RT is Burnt Offering for 20 mana", combat.DeepStrikes == deep + 1 && Mathf.Approximately(spells.StrikeManaPaid - paid, 20f),
            $"deep {deep}->{combat.DeepStrikes}, paid {spells.StrikeManaPaid - paid:0}");

        // In a mix the Devotion leaves force, and a burn goes out again.
        yield return Use(lightningAltar);
        Judge("Lightning taken beside Fire: a mix", element.Mixed, Carried());
        Judge("in a mix Fire Without End is out of force and the enemies' rules are clear", !upgrades.Has(FireTree.FireWithoutEnd) && !Afflictions.Rules.Any, Book());
        still.Afflictions.Clear();
        still.Afflictions.Lay(AfflictionKind.Burn, 1, 0f, 1f, LaySource.Spell, still);
        player.Teleport(still.transform.position + Vector3.back * 9f, Quaternion.identity);
        yield return Real(5f);
        Judge("and a burn left alone goes out in its 4 s", !still.Afflictions.Burn.IsBurning, $"{still.Afflictions.Burn.SecondsLeft:0.0}s left");
    }

    // ---- All in on Lightning: In One Breath, Down the Channel.
    private IEnumerator Lightning()
    {
        Say("--- all in on Lightning");
        yield return AllIn(lightningAltar);
        Judge("all in on Lightning", AllInOn(Elements.Lightning) && build.Read().AllIn, Carried());
        Judge("before it is owned, no breath", !upgrades.Has(LightningTree.InOneBreath) && upgrades.Rules.BreathAfterPerfect == 0f && !upgrades.Has(FireTree.FireWithoutEnd), Book());
        upgrades.Give(Upgrade.ById(LightningTree.InOneBreath));
        upgrades.RefreshNow();
        yield return null;
        Judge("owned and all in, the book gives a 2.5 s breath", upgrades.Has(LightningTree.InOneBreath) && Mathf.Approximately(upgrades.Rules.BreathAfterPerfect, 2.5f), Book());

        spells.SetSlot(0, "lightning.base");
        combat.Mana.Gain(100f);
        yield return AtDummy(3f);
        spells.Cooldowns.ResetAll();
        yield return Tap(Key.Digit1);
        yield return WhenFree();
        float table = spells.Cooldowns.Length(0);
        Judge("Thunderclap cast: its clock runs", !spells.Cooldowns.Ready(0) && spells.Cooldowns.Remaining(0) > 1f, $"{spells.Cooldowns.Remaining(0):0.00} of {table:0.00}s");

        // The breath after a perfect guard.
        int breaths = spells.Breaths;
        yield return PerfectGuard();
        Judge("a perfect guard gives a breath: the waiting spell is ready", lastOutcome == HitOutcome.PerfectGuard && spells.Breaths == breaths + 1 && spells.Cooldowns.InBreath && spells.Cooldowns.Ready(0),
            $"{lastOutcome}, breath {spells.Cooldowns.BreathLeft:0.00}s, ready {spells.Cooldowns.Ready(0)}");
        yield return Shot("devotion_lightning_breath");
        yield return WhenFree();
        int casts = spells.Casts;
        yield return Tap(Key.Digit1);
        yield return Real(0.1f);
        Judge("cast in the breath: no clock now, its wait owed", spells.Casts == casts + 1 && spells.Cooldowns.InBreath && spells.Cooldowns.Ready(0) && spells.Cooldowns.Owed(0) > 1f,
            $"owed {spells.Cooldowns.Owed(0):0.00}s, breath {spells.Cooldowns.BreathLeft:0.00}s");
        yield return WhenFree();
        if (spells.Cooldowns.InBreath)
        {
            casts = spells.Casts;
            yield return Tap(Key.Digit1);
            yield return Real(0.1f);
            Judge("cast again in the same breath", spells.Casts == casts + 1, $"casts {casts}->{spells.Casts}, breath {spells.Cooldowns.BreathLeft:0.00}s");
        }
        yield return Until(() => !spells.Cooldowns.InBreath, 3f);
        yield return null;
        Judge("when the breath ends the whole wait starts", !spells.Cooldowns.InBreath && !spells.Cooldowns.Ready(0) && spells.Cooldowns.Remaining(0) > table - 0.6f,
            $"{spells.Cooldowns.Remaining(0):0.00} of {spells.Cooldowns.Length(0):0.00}s");

        // The breath after a perfect dodge.
        yield return WhenFree();
        spells.Cooldowns.ResetAll();
        yield return Tap(Key.Digit1);
        yield return WhenFree();
        breaths = spells.Breaths;
        yield return PerfectDodge();
        Judge("a perfect dodge gives a breath too", lastOutcome == HitOutcome.PerfectDodge && spells.Breaths == breaths + 1 && spells.Cooldowns.Ready(0), $"{lastOutcome}, breath {spells.Cooldowns.BreathLeft:0.00}s");
        yield return Until(() => !spells.Cooldowns.InBreath, 3f);

        // The strikes are not spells: their clocks run through a breath.
        yield return WhenFree();
        spells.Cooldowns.ResetAll();
        yield return AtDummy();
        yield return PadStrike(pad.rightShoulder);
        yield return WhenFree();
        bool striking = !spells.Cooldowns.Ready(SpellCooldowns.StrikeFirst);
        yield return PerfectGuard();
        Judge("in a breath the L2 strikes keep their clocks", striking && spells.Cooldowns.InBreath && !spells.Cooldowns.Ready(SpellCooldowns.StrikeFirst),
            $"R1 clock {spells.Cooldowns.Remaining(SpellCooldowns.StrikeFirst):0.00}s, breath {spells.Cooldowns.InBreath}");
        yield return Until(() => !spells.Cooldowns.InBreath, 3f);

        // L2 with R2, all in: Down the Channel for 20 mana.
        yield return WhenFree();
        spells.Cooldowns.ResetAll();
        combat.Mana.Gain(100f);
        yield return AtDummy();
        int deep = combat.DeepStrikes;
        float paid = spells.StrikeManaPaid;
        yield return PadStrike(pad.rightTrigger);
        yield return Until(() => combat.InfusedNow, 1f);
        yield return Real(0.5f);
        yield return Shot("devotion_lightning_deep");
        yield return WhenFree();
        Judge("all in, L2 + RT is Down the Channel for 20 mana", combat.DeepStrikes == deep + 1 && Mathf.Approximately(spells.StrikeManaPaid - paid, 20f),
            $"deep {deep}->{combat.DeepStrikes}, paid {spells.StrikeManaPaid - paid:0}");
    }
}
